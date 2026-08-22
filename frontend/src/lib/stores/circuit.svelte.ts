export interface CircuitElement {
    id: string;
    type: string;
    x: number;
    y: number;
    value: number;
}

export interface Wire {
    id: string;
    fromElement: string;
    fromPin: string;
    toElement: string;
    toPin: string;
}

export interface CursorPosition {
    userId: string;
    x: number;
    y: number;
}

export class CircuitState {
    id: string | null = $state(null);
    elements: CircuitElement[] = $state([]);
    wires: Wire[] = $state([]);
    cursors: Record<string, CursorPosition> = $state({});
    activeUserIds: string[] = $state([]);
}

export const circuitStore = new CircuitState();
