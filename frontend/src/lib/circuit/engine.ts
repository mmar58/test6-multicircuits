import type { CircuitElement, Wire } from "../stores/circuit.svelte";

export function simulateCircuit(elements: CircuitElement[], wires: Wire[], initialValues?: Record<string, number>): Record<string, number> {
    const values: Record<string, number> = {};
    const inDegree: Record<string, number> = {};
    const adj: Record<string, { to: string, toPin: string, fromPin: string }[]> = {};
    
    // Initialize
    for (const el of elements) {
        adj[el.id] = [];
        inDegree[el.id] = 0;
        if (el.type === "INPUT") {
            values[el.id] = initialValues && initialValues[el.id] !== undefined ? initialValues[el.id] : (el.value || 0);
        }
    }
    
    // Build graph
    for (const wire of wires) {
        if (!adj[wire.fromElement]) continue;
        adj[wire.fromElement].push({ to: wire.toElement, toPin: wire.toPin, fromPin: wire.fromPin });
        if (inDegree[wire.toElement] !== undefined) {
            inDegree[wire.toElement]++;
        }
    }
    
    // Topological sort queue
    const queue: string[] = [];
    for (const el of elements) {
        if (inDegree[el.id] === 0) {
            queue.push(el.id);
        }
    }
    
    // Process
    const inputsMap: Record<string, Record<string, number>> = {};
    for (const el of elements) {
        inputsMap[el.id] = {};
    }
    
    while (queue.length > 0) {
        const currId = queue.shift()!;
        const el = elements.find(e => e.id === currId);
        if (!el) continue;
        
        let outVal = 0;
        
        if (el.type === "INPUT") {
            outVal = values[currId];
        } else {
            const ins = inputsMap[currId];
            const inA = ins['A'] || 0;
            const inB = ins['B'] || 0;
            
            switch (el.type) {
                case "AND": outVal = inA & inB; break;
                case "OR": outVal = inA | inB; break;
                case "NOT": outVal = inA === 0 ? 1 : 0; break;
                case "XOR": outVal = inA ^ inB; break;
                case "NAND": outVal = (inA & inB) === 0 ? 1 : 0; break;
                case "NOR": outVal = (inA | inB) === 0 ? 1 : 0; break;
                case "OUTPUT": outVal = inA; break;
                default: outVal = 0;
            }
            values[currId] = outVal;
        }
        
        // Propagate
        for (const edge of adj[currId]) {
            inputsMap[edge.to][edge.toPin] = outVal;
            inDegree[edge.to]--;
            if (inDegree[edge.to] === 0) {
                queue.push(edge.to);
            }
        }
    }
    
    return values;
}
