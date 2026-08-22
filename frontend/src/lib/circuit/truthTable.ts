import type { CircuitElement, Wire } from "../stores/circuit.svelte";
import { simulateCircuit } from "./engine";

export interface TruthTableRow {
    inputs: Record<string, number>;
    outputs: Record<string, number>;
}

export function generateTruthTable(elements: CircuitElement[], wires: Wire[]): { headers: { inputs: string[], outputs: string[] }, rows: TruthTableRow[] } {
    const inputs = elements.filter(e => e.type === "INPUT").sort((a, b) => a.y - b.y);
    const outputs = elements.filter(e => e.type === "OUTPUT").sort((a, b) => a.y - b.y);
    
    const rows: TruthTableRow[] = [];
    const numInputs = inputs.length;
    const numCombinations = Math.pow(2, numInputs);
    
    // We limit to 10 inputs (1024 rows) to prevent browser freeze
    const maxCombinations = Math.min(numCombinations, 1024);
    
    for (let i = 0; i < maxCombinations; i++) {
        const rowInputs: Record<string, number> = {};
        
        // Set input values for this combination
        for (let j = 0; j < numInputs; j++) {
            // j=0 is LSB or MSB depending on preference. Let's make j=0 the first input (MSB-like).
            const bit = (i >> (numInputs - 1 - j)) & 1;
            rowInputs[inputs[j].id] = bit;
        }
        
        const simulationResult = simulateCircuit(elements, wires, rowInputs);
        
        const rowOutputs: Record<string, number> = {};
        for (const out of outputs) {
            rowOutputs[out.id] = simulationResult[out.id] || 0;
        }
        
        rows.push({
            inputs: rowInputs,
            outputs: rowOutputs
        });
    }
    
    return {
        headers: {
            inputs: inputs.map((_, idx) => `IN ${idx + 1}`),
            outputs: outputs.map((_, idx) => `OUT ${idx + 1}`)
        },
        rows
    };
}
