<script lang="ts">
    import { generateTruthTable, type TruthTableRow } from "../../circuit/truthTable";
    import type { CircuitElement, Wire } from "../../stores/circuit.svelte";
    import { X, Download } from "@lucide/svelte";
    
    let { elements, wires, onClose } = $props<{
        elements: CircuitElement[],
        wires: Wire[],
        onClose: () => void
    }>();
    
    let tableData = $derived(generateTruthTable(elements, wires));
    
    function exportCsv() {
        if (!tableData) return;
        
        let csv = "";
        
        // Headers
        csv += [...tableData.headers.inputs, ...tableData.headers.outputs].join(",") + "\n";
        
        // Rows
        for (const row of tableData.rows) {
            const inVals = Object.values(row.inputs);
            const outVals = Object.values(row.outputs);
            csv += [...inVals, ...outVals].join(",") + "\n";
        }
        
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = "truth_table.csv";
        link.click();
    }
</script>

<div class="absolute top-0 right-0 h-full w-96 bg-card border-l border-border shadow-2xl flex flex-col z-30 animate-in slide-in-from-right duration-300">
    <div class="p-4 border-b border-border flex items-center justify-between bg-card/80 backdrop-blur-md">
        <h2 class="font-semibold text-lg flex items-center gap-2">
            Truth Table
        </h2>
        <div class="flex items-center gap-2">
            <button onclick={exportCsv} class="p-1.5 rounded-md hover:bg-secondary text-muted-foreground hover:text-cyan-400 transition-colors" title="Export CSV">
                <Download class="w-4 h-4" />
            </button>
            <button onclick={onClose} class="p-1.5 rounded-md hover:bg-secondary text-muted-foreground hover:text-foreground transition-colors">
                <X class="w-4 h-4" />
            </button>
        </div>
    </div>
    
    <div class="flex-1 overflow-auto p-4 custom-scrollbar">
        {#if tableData.headers.inputs.length === 0}
            <div class="text-center py-10 text-muted-foreground">
                <p>No inputs found in the circuit.</p>
                <p class="text-sm mt-2">Add some INPUT gates to generate a truth table.</p>
            </div>
        {:else if tableData.headers.inputs.length > 10}
            <div class="text-center py-10 text-muted-foreground">
                <p>Too many inputs ({tableData.headers.inputs.length}).</p>
                <p class="text-sm mt-2">Truth table generation is limited to 10 inputs (1024 combinations) for performance.</p>
            </div>
        {:else}
            <div class="rounded-lg border border-border overflow-hidden">
                <table class="w-full text-sm text-center">
                    <thead class="bg-secondary/50 text-muted-foreground text-xs uppercase">
                        <tr>
                            {#each tableData.headers.inputs as header}
                                <th class="px-3 py-2 border-b border-r border-border font-medium">{header}</th>
                            {/each}
                            {#each tableData.headers.outputs as header}
                                <th class="px-3 py-2 border-b border-r last:border-r-0 border-border font-medium text-cyan-400/80">{header}</th>
                            {/each}
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-border/50 bg-card">
                        {#each tableData.rows as row}
                            <tr class="hover:bg-secondary/30 transition-colors">
                                {#each Object.values(row.inputs) as val}
                                    <td class="px-3 py-2 border-r border-border font-mono {val === 1 ? 'text-green-400 font-bold' : 'text-slate-500'}">{val}</td>
                                {/each}
                                {#each Object.values(row.outputs) as val}
                                    <td class="px-3 py-2 border-r last:border-r-0 border-border font-mono {val === 1 ? 'text-green-400 font-bold' : 'text-slate-500'} bg-cyan-950/10">{val}</td>
                                {/each}
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
            <div class="mt-4 text-xs text-muted-foreground text-center">
                Showing {tableData.rows.length} combinations
            </div>
        {/if}
    </div>
</div>

<style>
    .custom-scrollbar::-webkit-scrollbar {
        width: 6px;
        height: 6px;
    }
    .custom-scrollbar::-webkit-scrollbar-track {
        background: transparent;
    }
    .custom-scrollbar::-webkit-scrollbar-thumb {
        background-color: rgba(255, 255, 255, 0.1);
        border-radius: 10px;
    }
</style>
