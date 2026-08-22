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
    id: string | null = null;
    elements: CircuitElement[] = [];
    wires: Wire[] = [];
    cursors: Record<string, CursorPosition> = {};
    activeUserIds: string[] = [];
}

export const circuitStore = $state(new CircuitState());
