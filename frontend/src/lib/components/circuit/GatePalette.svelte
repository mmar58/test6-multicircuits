<script lang="ts">
    import { signalrService } from "$lib/signalr";
    import { circuitStore } from "$lib/stores/circuit.svelte";

    const gates = [
        {
            type: "INPUT",
            label: "Input",
            color: "bg-blue-500/20 text-blue-400 border-blue-500/50",
        },
        {
            type: "OUTPUT",
            label: "Output",
            color: "bg-purple-500/20 text-purple-400 border-purple-500/50",
        },
        {
            type: "AND",
            label: "AND",
            color: "bg-slate-700/50 text-slate-300 border-slate-600",
        },
        {
            type: "OR",
            label: "OR",
            color: "bg-slate-700/50 text-slate-300 border-slate-600",
        },
        {
            type: "NOT",
            label: "NOT",
            color: "bg-slate-700/50 text-slate-300 border-slate-600",
        },
        {
            type: "NAND",
            label: "NAND",
            color: "bg-slate-700/50 text-slate-300 border-slate-600",
        },
        {
            type: "NOR",
            label: "NOR",
            color: "bg-slate-700/50 text-slate-300 border-slate-600",
        },
        {
            type: "XOR",
            label: "XOR",
            color: "bg-slate-700/50 text-slate-300 border-slate-600",
        },
    ];

    function handleDragStart(e: DragEvent, type: string) {
        if (e.dataTransfer) {
            e.dataTransfer.setData("text/plain", type);
            e.dataTransfer.effectAllowed = "copy";
        }
    }
</script>

<div class="w-64 bg-card border-r border-border flex flex-col z-10 shrink-0">
    <div class="p-4 border-b border-border/50">
        <h2
            class="font-semibold text-sm text-muted-foreground uppercase tracking-wider"
        >
            Components
        </h2>
    </div>

    <div class="p-4 flex flex-col overflow-y-auto custom-scrollbar">
        <div
            class="mt-4 p-4 rounded-lg bg-cyan-900/10 border border-cyan-500/20 text-cyan-400/80 text-xs text-center"
        >
            Drag and drop components onto the canvas to build your circuit.
        </div>
        {#each gates as gate}
            <div
                draggable="true"
                ondragstart={(e) => handleDragStart(e, gate.type)}
                class="flex items-center p-3 rounded-lg border cursor-grab active:cursor-grabbing hover:bg-secondary transition-all {gate.color}"
            >
                <div
                    class="w-8 h-8 rounded bg-background/50 flex items-center justify-center mr-3 font-mono text-xs font-bold border border-current/20"
                >
                    {gate.type.substring(0, 2)}
                </div>
                <span class="font-medium text-sm">{gate.label} Gate</span>
            </div>
        {/each}
    </div>
</div>

<style>
    .custom-scrollbar::-webkit-scrollbar {
        width: 6px;
    }
    .custom-scrollbar::-webkit-scrollbar-track {
        background: transparent;
    }
    .custom-scrollbar::-webkit-scrollbar-thumb {
        background-color: rgba(255, 255, 255, 0.1);
        border-radius: 10px;
    }
</style>
