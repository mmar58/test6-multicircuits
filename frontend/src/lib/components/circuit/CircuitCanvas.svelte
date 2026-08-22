<script lang="ts">
    import { onMount } from "svelte";
    import { circuitStore } from "../../stores/circuit.svelte";
    import { signalrService } from "../../signalr";
    import { simulateCircuit } from "../../circuit/engine";
    import GateElement from "./GateElement.svelte";
    import WireLayer from "./WireLayer.svelte";
    import { dashboardStore } from "$lib/stores/dashboard.svelte";
    
    let { isSimulating } = $props<{ isSimulating: boolean }>();
    
    let svgElement: SVGSVGElement;
    
    export function getSvgElement() {
        return svgElement;
    }
    
    // Interaction state
    let isDragging = $state(false);
    let dragElementId = $state<string | null>(null);
    let dragOffset = { x: 0, y: 0 };
    
    let isWiring = $state(false);
    let wireStart = $state<{el: string, pin: string} | null>(null);
    let wireCurrentPos = $state<{x: number, y: number} | null>(null);
    
    // Grid
    const gridSize = 20;
    
    function snapToGrid(val: number) {
        return Math.round(val / gridSize) * gridSize;
    }
    
    function getMouseCoords(e: MouseEvent) {
        if (!svgElement) return { x: 0, y: 0 };
        const pt = svgElement.createSVGPoint();
        pt.x = e.clientX;
        pt.y = e.clientY;
        const ctm = svgElement.getScreenCTM();
        if (!ctm) return { x: e.clientX, y: e.clientY };
        const svgP = pt.matrixTransform(ctm.inverse());
        return { x: svgP.x, y: svgP.y };
    }
    
    // Drag & Drop from palette
    function handleDragOver(e: DragEvent) {
        e.preventDefault();
        if (e.dataTransfer) e.dataTransfer.dropEffect = "copy";
    }
    
    async function handleDrop(e: DragEvent) {
        e.preventDefault();
        const type = e.dataTransfer?.getData("text/plain");
        if (!type) return;
        
        const coords = getMouseCoords(e as unknown as MouseEvent);
        const newEl = {
            id: Math.random().toString(36).substring(2, 9),
            type,
            x: snapToGrid(coords.x - 40),
            y: snapToGrid(coords.y - 25),
            value: 0
        };
        
        circuitStore.elements.push(newEl);
        
        const conn = signalrService.getConnection();
        if (conn && circuitStore.id) {
            await conn.invoke("UpdateElement", circuitStore.id, newEl);
        }
        
        runSimulation();
    }
    
    // Moving existing gates
    function handleGateMouseDown(e: MouseEvent, elId: string) {
        if (isSimulating || e.button !== 0) return;
        const el = circuitStore.elements.find(e => e.id === elId);
        if (!el) return;
        
        isDragging = true;
        dragElementId = elId;
        const coords = getMouseCoords(e);
        dragOffset = { x: coords.x - el.x, y: coords.y - el.y };
    }
    
    // Input toggling
    async function handleGateClick(e: MouseEvent, elId: string) {
        if (!isSimulating) return;
        const el = circuitStore.elements.find(e => e.id === elId);
        if (el && el.type === "INPUT") {
            el.value = el.value === 1 ? 0 : 1;
            
            runSimulation();
            
            const conn = signalrService.getConnection();
            if (conn && circuitStore.id) {
                await conn.invoke("UpdateElement", circuitStore.id, el);
            }
        }
    }
    
    // Wiring
    function handlePinMouseDown(e: MouseEvent, elId: string, pin: string) {
        e.stopPropagation();
        if (isSimulating || pin.startsWith("in")) return; // Only start from outputs for now
        
        isWiring = true;
        wireStart = { el: elId, pin };
        wireCurrentPos = getMouseCoords(e);
    }
    
    async function handlePinMouseUp(e: MouseEvent, elId: string, pin: string) {
        if (!isWiring || !wireStart) return;
        
        if (pin.startsWith("in") && elId !== wireStart.el) {
            const newWire = {
                id: Math.random().toString(36).substring(2, 9),
                fromElement: wireStart.el,
                fromPin: wireStart.pin,
                toElement: elId,
                toPin: pin
            };
            
            circuitStore.wires.push(newWire);
            
            const conn = signalrService.getConnection();
            if (conn && circuitStore.id) {
                await conn.invoke("AddWire", circuitStore.id, newWire);
            }
            
            runSimulation();
        }
        
        isWiring = false;
        wireStart = null;
        wireCurrentPos = null;
    }
    
    // Global mouse events
    let lastCursorUpdate = 0;
    
    function handleMouseMove(e: MouseEvent) {
        const coords = getMouseCoords(e);
        
        // Move gate
        if (isDragging && dragElementId) {
            const el = circuitStore.elements.find(e => e.id === dragElementId);
            if (el) {
                el.x = snapToGrid(coords.x - dragOffset.x);
                el.y = snapToGrid(coords.y - dragOffset.y);
            }
        }
        
        // Move wire
        if (isWiring) {
            wireCurrentPos = coords;
        }
        
        // Broadcast cursor (throttled)
        const now = Date.now();
        if (now - lastCursorUpdate > 100) {
            const conn = signalrService.getConnection();
            if (conn && circuitStore.id) {
                conn.invoke("UpdateCursor", circuitStore.id, coords.x, coords.y).catch(()=>{});
            }
            lastCursorUpdate = now;
        }
    }
    
    async function handleMouseUp(e: MouseEvent) {
        if (isDragging && dragElementId) {
            const el = circuitStore.elements.find(e => e.id === dragElementId);
            if (el) {
                const conn = signalrService.getConnection();
                if (conn && circuitStore.id) {
                    await conn.invoke("UpdateElement", circuitStore.id, el);
                }
            }
        }
        
        isDragging = false;
        dragElementId = null;
        isWiring = false;
        wireStart = null;
        wireCurrentPos = null;
    }
    
    // Delete wires
    async function handleWireClick(e: MouseEvent, wireId: string) {
        if (isSimulating) return;
        
        circuitStore.wires = circuitStore.wires.filter(w => w.id !== wireId);
        
        const conn = signalrService.getConnection();
        if (conn && circuitStore.id) {
            await conn.invoke("RemoveWire", circuitStore.id, wireId);
        }
        runSimulation();
    }
    
    // Simulation runner
    function runSimulation() {
        if (!isSimulating) return;
        const newValues = simulateCircuit(circuitStore.elements, circuitStore.wires);
        
        let changed = false;
        for (const el of circuitStore.elements) {
            if (newValues[el.id] !== undefined && el.value !== newValues[el.id]) {
                el.value = newValues[el.id];
                changed = true;
            }
        }
    }
    
    // React to simulation mode toggle
    $effect(() => {
        if (isSimulating) {
            runSimulation();
        }
    });

</script>

<svg 
    bind:this={svgElement}
    class="w-full h-full"
    ondragover={handleDragOver}
    ondrop={handleDrop}
    onmousemove={handleMouseMove}
    onmouseup={handleMouseUp}
    onmouseleave={handleMouseUp}
>
    <!-- Background Grid -->
    <defs>
        <pattern id="grid" width={gridSize} height={gridSize} patternUnits="userSpaceOnUse">
            <circle cx="1" cy="1" r="1" class="fill-border/50" />
        </pattern>
    </defs>
    <rect width="100%" height="100%" fill="url(#grid)" />
    
    <!-- Wires -->
    <WireLayer wires={circuitStore.wires} elements={circuitStore.elements} {isSimulating} />
    
    <!-- Drawing Wire -->
    {#if isWiring && wireStart && wireCurrentPos}
        {@const startEl = circuitStore.elements.find(e => e.id === wireStart?.el)}
        {#if startEl}
            <path 
                d="M {startEl.x + 80} {startEl.y + 25} C {startEl.x + 80 + Math.abs(wireCurrentPos.x - startEl.x - 80)*0.5} {startEl.y + 25}, {wireCurrentPos.x - Math.abs(wireCurrentPos.x - startEl.x - 80)*0.5} {wireCurrentPos.y}, {wireCurrentPos.x} {wireCurrentPos.y}" 
                fill="none" 
                class="stroke-cyan-500 stroke-2 border-dashed opacity-50"
            />
        {/if}
    {/if}
    
    <!-- Gates -->
    {#each circuitStore.elements as el (el.id)}
        <GateElement 
            element={el} 
            {isSimulating} 
            onmousedown={(e) => handleGateMouseDown(e, el.id)}
            onclick={(e) => handleGateClick(e, el.id)}
        />
        
        <!-- Interactive Pins overlay (transparent but clickable) -->
        {#if !isSimulating}
            <g transform="translate({el.x}, {el.y})">
                {#if el.type !== 'INPUT'}
                    <circle cx="0" cy={50 * 0.3} r="10" fill="transparent" class="cursor-crosshair" onmousedown={(e) => handlePinMouseDown(e, el.id, 'in-A')} onmouseup={(e) => handlePinMouseUp(e, el.id, 'in-A')} />
                    {#if !['NOT', 'OUTPUT'].includes(el.type)}
                        <circle cx="0" cy={50 * 0.7} r="10" fill="transparent" class="cursor-crosshair" onmousedown={(e) => handlePinMouseDown(e, el.id, 'in-B')} onmouseup={(e) => handlePinMouseUp(e, el.id, 'in-B')} />
                    {/if}
                {/if}
                {#if el.type !== 'OUTPUT'}
                    <circle cx={80} cy={25} r="10" fill="transparent" class="cursor-crosshair" onmousedown={(e) => handlePinMouseDown(e, el.id, 'out-main')} />
                {/if}
            </g>
        {/if}
    {/each}
    
    <!-- Remote Cursors -->
    {#each Object.values(circuitStore.cursors) as cursor}
        {@const u = dashboardStore.onlineUsers.find(u => u.id === cursor.userId)}
        {#if u}
            <g transform="translate({cursor.x}, {cursor.y})" class="pointer-events-none transition-transform duration-100 ease-out">
                <path d="M0 0 L15 10 L10 12 L15 20 L12 22 L7 14 L3 19 Z" class="fill-current" style="color: {u.color}" />
                <rect x="15" y="15" width="60" height="20" rx="4" fill={u.color} opacity="0.8" />
                <text x="45" y="29" fill="white" font-size="10" text-anchor="middle" font-family="sans-serif">{u.displayName}</text>
            </g>
        {/if}
    {/each}
</svg>
