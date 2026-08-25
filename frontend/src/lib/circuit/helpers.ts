import type { CircuitElement } from "../stores/circuit.svelte";

/**
 * Returns INPUT elements sorted top-to-bottom by Y position.
 * Used to derive stable In1, In2... labels and truth table ordering.
 */
export function getSortedInputs(elements: CircuitElement[]): CircuitElement[] {
    return elements.filter(e => e.type === "INPUT").sort((a, b) => a.y - b.y);
}

/**
 * Returns OUTPUT elements sorted top-to-bottom by Y position.
 * Used to derive stable Out1, Out2... labels and truth table ordering.
 */
export function getSortedOutputs(elements: CircuitElement[]): CircuitElement[] {
    return elements.filter(e => e.type === "OUTPUT").sort((a, b) => a.y - b.y);
}

/**
 * Returns a display label for an element based on its sorted position among
 * inputs/outputs (e.g. "In1", "Out2"). Returns undefined for gate types.
 */
export function getElementLabel(el: CircuitElement, elements: CircuitElement[]): string | undefined {
    if (el.type === "INPUT") {
        return `In${getSortedInputs(elements).findIndex(e => e.id === el.id) + 1}`;
    }
    if (el.type === "OUTPUT") {
        return `Out${getSortedOutputs(elements).findIndex(e => e.id === el.id) + 1}`;
    }
    return undefined;
}
