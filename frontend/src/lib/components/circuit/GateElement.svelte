<script lang="ts">
    import type { CircuitElement } from "../../stores/circuit.svelte";
    import { GATE_WIDTH, GATE_HEIGHT } from "../../circuit/constants";
    
    let { element, isSimulating, isSelected, onmousedown, onclick, oncontextmenu, label } = $props<{
        element: CircuitElement,
        isSimulating: boolean,
        isSelected?: boolean,
        onmousedown?: (e: MouseEvent) => void,
        onclick?: (e: MouseEvent) => void,
        oncontextmenu?: (e: MouseEvent) => void,
        label?: string
    }>();
    
    // Gate dimensions (shared constants — see circuit/constants.ts)
    const width = GATE_WIDTH;
    const height = GATE_HEIGHT;
    
    const isHigh = $derived(isSimulating && element.value === 1);
</script>

<g 
    transform="translate({element.x}, {element.y})" 
    class="cursor-move select-none {isSimulating ? 'cursor-pointer' : ''}"
    onmousedown={onmousedown}
    onclick={onclick}
    oncontextmenu={oncontextmenu}
>
    <!-- Highlight when high or selected -->
    {#if isHigh || isSelected}
        <rect x="-5" y="-5" width={width + 10} height={height + 10} rx="8" fill={isSelected ? "rgba(34, 211, 238, 0.3)" : "rgba(34, 197, 94, 0.2)"} class="blur-md" />
    {/if}

    <!-- Main Body -->
    <rect 
        x="0" y="0" 
        {width} {height} 
        rx="4" 
        class="{element.type === 'INPUT' ? 'fill-blue-900/40 stroke-blue-500' : element.type === 'OUTPUT' ? 'fill-purple-900/40 stroke-purple-500' : 'fill-slate-800 stroke-slate-500'} stroke-2 transition-colors duration-200 {isHigh ? 'stroke-green-400 fill-green-900/30' : ''} {isSelected ? 'stroke-cyan-400' : ''}" 
    />
    
    <!-- Label -->
    <text 
        x={width / 2} y={height / 2} 
        dominant-baseline="middle" 
        text-anchor="middle"
        class="fill-white font-mono text-xs pointer-events-none font-bold"
    >
        {label || element.type}
    </text>
    
    <!-- Input Pins -->
    {#if element.type !== 'INPUT'}
        <circle cx="0" cy={height * 0.3} r="4" class="fill-slate-900 stroke-slate-400 stroke-2 cursor-crosshair hover:stroke-cyan-400 hover:fill-cyan-900 transition-colors" data-pin="in-A" data-el={element.id} />
        {#if !['NOT', 'OUTPUT'].includes(element.type)}
            <circle cx="0" cy={height * 0.7} r="4" class="fill-slate-900 stroke-slate-400 stroke-2 cursor-crosshair hover:stroke-cyan-400 hover:fill-cyan-900 transition-colors" data-pin="in-B" data-el={element.id} />
        {/if}
    {/if}
    
    <!-- Output Pins -->
    {#if element.type !== 'OUTPUT'}
        <circle cx={width} cy={height / 2} r="4" class="fill-slate-900 stroke-slate-400 stroke-2 cursor-crosshair hover:stroke-cyan-400 hover:fill-cyan-900 transition-colors {isHigh ? 'stroke-green-400 fill-green-400' : ''}" data-pin="out-main" data-el={element.id} />
    {/if}
    
    <!-- Value indicator for Simulation -->
    {#if isSimulating}
        <text 
            x={width / 2} y={-10} 
            dominant-baseline="middle" 
            text-anchor="middle"
            class="font-mono text-sm font-bold {isHigh ? 'fill-green-400' : 'fill-slate-500'}"
        >
            {element.value || 0}
        </text>
    {/if}
</g>
