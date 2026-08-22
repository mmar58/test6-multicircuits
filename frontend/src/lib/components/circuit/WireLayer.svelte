<script lang="ts">
    import type { Wire, CircuitElement } from "../../stores/circuit.svelte";
    
    let { wires, elements, isSimulating, selectedWireId, onSelect, onContextMenu } = $props<{
        wires: Wire[],
        elements: CircuitElement[],
        isSimulating: boolean,
        selectedWireId?: string | null,
        onSelect?: (e: MouseEvent, id: string) => void,
        onContextMenu?: (e: MouseEvent, id: string) => void
    }>();
    
    function getPinCoords(elementId: string, pin: string) {
        const el = elements.find(e => e.id === elementId);
        if (!el) return null;
        
        const width = 80;
        const height = 50;
        
        let cx = el.x;
        let cy = el.y;
        
        if (pin.startsWith("out")) {
            cx += width;
            cy += height / 2;
        } else if (pin === "in-A") {
            cy += height * 0.3;
        } else if (pin === "in-B") {
            cy += height * 0.7;
        } else if (pin === "in-main") {
            cy += height / 2;
        }
        
        return { x: cx, y: cy };
    }
    
    function getWirePath(w: Wire) {
        const from = getPinCoords(w.fromElement, w.fromPin);
        const to = getPinCoords(w.toElement, w.toPin);
        
        if (!from || !to) return "";
        
        const dx = Math.abs(to.x - from.x) * 0.5;
        
        return `M ${from.x} ${from.y} C ${from.x + dx} ${from.y}, ${to.x - dx} ${to.y}, ${to.x} ${to.y}`;
    }
    
    function getWireColor(w: Wire) {
        if (selectedWireId === w.id) return "stroke-cyan-400 drop-shadow-[0_0_8px_rgba(34,211,238,0.8)]";
        if (!isSimulating) return "stroke-slate-600";
        const fromEl = elements.find(e => e.id === w.fromElement);
        if (fromEl && fromEl.value === 1) return "stroke-green-400 drop-shadow-[0_0_8px_rgba(74,222,128,0.5)]";
        return "stroke-slate-700";
    }
</script>

<g>
    {#each wires as wire (wire.id)}
        <!-- Hit area (thicker transparent stroke for easier clicking) -->
        <path 
            d={getWirePath(wire)} 
            fill="none" 
            stroke="transparent"
            stroke-width="15"
            class="cursor-pointer pointer-events-stroke"
            onclick={(e) => onSelect?.(e, wire.id)}
            oncontextmenu={(e) => onContextMenu?.(e, wire.id)}
        />
        <!-- Visible wire -->
        <path 
            d={getWirePath(wire)} 
            fill="none" 
            class="{getWireColor(wire)} stroke-2 transition-all duration-300 pointer-events-none"
            data-wire={wire.id}
        />
    {/each}
</g>
