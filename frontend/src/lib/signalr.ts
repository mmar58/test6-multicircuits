import * as signalR from "@microsoft/signalr";
import { dashboardStore } from "./stores/dashboard.svelte";
import { circuitStore } from "./stores/circuit.svelte";

class SignalRService {
    private connection: signalR.HubConnection | null = null;

    public init() {
        if (this.connection) return this.connection;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("https://apimulticircuit.anzdevelopers.com/circuithub") // Update port as needed
            .withAutomaticReconnect()
            .build();

        this.registerEvents(this.connection);
        return this.connection;
    }

    private registerEvents(connection: signalR.HubConnection) {
        connection.on("DashboardInit", (circuits, users) => {
            dashboardStore.circuits = circuits;
            dashboardStore.onlineUsers = users;
        });

        connection.on("DashboardUpdated", (circuits) => {
            dashboardStore.circuits = circuits;
        });

        connection.on("UserJoinedCircuit", (user) => {
            if (!circuitStore.activeUserIds.includes(user.id)) {
                circuitStore.activeUserIds.push(user.id);
            }
            if (!dashboardStore.onlineUsers.find(u => u.id === user.id)) {
                dashboardStore.onlineUsers.push(user);
            }
        });

        connection.on("UserLeftCircuit", (userId) => {
            circuitStore.activeUserIds = circuitStore.activeUserIds.filter(id => id !== userId);
            delete circuitStore.cursors[userId];
        });

        connection.on("ElementUpdated", (element) => {
            const index = circuitStore.elements.findIndex(e => e.id === element.id);
            if (index !== -1) {
                circuitStore.elements[index] = element;
            } else {
                circuitStore.elements.push(element);
            }
        });

        connection.on("ElementRemoved", (elementId) => {
            circuitStore.elements = circuitStore.elements.filter(e => e.id !== elementId);
        });

        connection.on("WireAdded", (wire) => {
            if (!circuitStore.wires.find(w => w.id === wire.id)) {
                circuitStore.wires.push(wire);
            }
        });

        connection.on("WireRemoved", (wireId) => {
            circuitStore.wires = circuitStore.wires.filter(w => w.id !== wireId);
        });

        connection.on("CursorMoved", (userId, x, y) => {
            circuitStore.cursors[userId] = { userId, x, y };
        });
    }

    public getConnection() {
        return this.connection;
    }

    /**
     * Convenience wrapper: invokes a hub method only when the connection is active.
     * Eliminates the repeated `getConnection() + state === 'Connected'` guard.
     * Returns null silently if not connected.
     */
    public async invoke<T = void>(method: string, ...args: unknown[]): Promise<T | null> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return await this.connection.invoke<T>(method, ...args);
        }
        return null;
    }

    /**
     * Fire-and-forget variant of invoke — catches errors silently (e.g. cursor updates).
     */
    public send(method: string, ...args: unknown[]): void {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            this.connection.invoke(method, ...args).catch(() => { });
        }
    }
}

export const signalrService = new SignalRService();
