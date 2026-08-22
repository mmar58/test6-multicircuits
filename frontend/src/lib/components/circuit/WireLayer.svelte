<script lang="ts">
    import type { Wire, CircuitElement } from "../../stores/circuit.svelte";
    
    let { wires, elements, isSimulating } = $props<{
        wires: Wire[],
        elements: CircuitElement[],
        isSimulating: boolean
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
        if (!isSimulating) return "stroke-slate-600";
        const fromEl = elements.find(e => e.id === w.fromElement);
        if (fromEl && fromEl.value === 1) return "stroke-green-400 drop-shadow-[0_0_8px_rgba(74,222,128,0.5)]";
        return "stroke-slate-700";
    }
</script>

<g>
    {#each wires as wire (wire.id)}
        <path 
            d={getWirePath(wire)} 
            fill="none" 
            class="{getWireColor(wire)} stroke-2 transition-all duration-300 pointer-events-stroke hover:stroke-red-500 hover:stroke-[4px] cursor-pointer"
            data-wire={wire.id}
        />
    {/each}
</g>
