export interface CircuitProject {
    id: string;
    name: string;
    description: string;
    gridSize: number;
    activeUserIds: string[];
}

export class DashboardState {
    circuits: CircuitProject[] = [];
    onlineUsers: any[] = [];
}

export const dashboardStore = $state(new DashboardState());
