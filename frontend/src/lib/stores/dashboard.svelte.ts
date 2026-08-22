export interface CircuitProject {
    id: string;
    name: string;
    description: string;
    gridSize: number;
    activeUserIds: string[];
}

export class DashboardState {
    circuits: CircuitProject[] = $state([]);
    onlineUsers: any[] = $state([]);
}

export const dashboardStore = new DashboardState();
