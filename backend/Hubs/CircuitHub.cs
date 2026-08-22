using Microsoft.AspNetCore.SignalR;
using backend.Models;
using backend.Services;

namespace backend.Hubs;

public class CircuitHub : Hub
{
    private readonly CircuitStateService _state;

    public CircuitHub(CircuitStateService state)
    {
        _state = state;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        if (_state.Users.TryGetValue(connectionId, out var user))
        {
            var circuitId = user.CurrentCircuitId;
            _state.RemoveUser(connectionId);
            
            // Broadcast user left
            if (circuitId != null)
            {
                await Groups.RemoveFromGroupAsync(connectionId, circuitId);
                await Clients.Group(circuitId).SendAsync("UserLeftCircuit", connectionId);
                await Clients.Group("dashboard").SendAsync("DashboardUpdated", _state.Circuits.Values);
            }
            else
            {
                await Groups.RemoveFromGroupAsync(connectionId, "dashboard");
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Auth / Identity
    public async Task<UserSession> RegisterName(string name)
    {
        var finalName = _state.RegisterUser(Context.ConnectionId, name);
        return _state.Users[Context.ConnectionId];
    }

    // Dashboard
    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        await Clients.Caller.SendAsync("DashboardInit", _state.Circuits.Values, _state.Users.Values);
    }

    public async Task LeaveDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "dashboard");
    }

    public async Task<CircuitProject> CreateCircuit(string name, string description, int gridSize)
    {
        var circuit = _state.CreateCircuit(name, description, gridSize);
        await Clients.Group("dashboard").SendAsync("DashboardUpdated", _state.Circuits.Values);
        return circuit;
    }

    public async Task DeleteCircuit(string circuitId)
    {
        if (_state.Circuits.TryRemove(circuitId, out _))
        {
            await Clients.Group("dashboard").SendAsync("DashboardUpdated", _state.Circuits.Values);
        }
    }

    // Circuit Editor
    public async Task<CircuitProject?> JoinCircuit(string circuitId)
    {
        if (!_state.Circuits.TryGetValue(circuitId, out var circuit))
            return null;

        var connectionId = Context.ConnectionId;
        if (_state.Users.TryGetValue(connectionId, out var user))
        {
            user.CurrentCircuitId = circuitId;
            circuit.ActiveUserIds.Add(connectionId);

            await Groups.RemoveFromGroupAsync(connectionId, "dashboard");
            await Groups.AddToGroupAsync(connectionId, circuitId);
            
            await Clients.Group(circuitId).SendAsync("UserJoinedCircuit", user);
            await Clients.Group("dashboard").SendAsync("DashboardUpdated", _state.Circuits.Values);
        }
        return circuit;
    }

    public async Task LeaveCircuit(string circuitId)
    {
        var connectionId = Context.ConnectionId;
        if (_state.Users.TryGetValue(connectionId, out var user))
        {
            user.CurrentCircuitId = null;
            if (_state.Circuits.TryGetValue(circuitId, out var circuit))
            {
                circuit.ActiveUserIds.Remove(connectionId);
                await Clients.Group(circuitId).SendAsync("UserLeftCircuit", connectionId);
            }

            await Groups.RemoveFromGroupAsync(connectionId, circuitId);
            await Groups.AddToGroupAsync(connectionId, "dashboard");
            await Clients.Group("dashboard").SendAsync("DashboardUpdated", _state.Circuits.Values);
        }
    }

    // Sync Commands
    public async Task UpdateElement(string circuitId, CircuitElement element)
    {
        if (_state.Circuits.TryGetValue(circuitId, out var circuit))
        {
            var existing = circuit.Elements.FirstOrDefault(e => e.Id == element.Id);
            if (existing != null)
            {
                existing.X = element.X;
                existing.Y = element.Y;
                existing.Value = element.Value;
            }
            else
            {
                circuit.Elements.Add(element);
            }
            await Clients.Group(circuitId).SendAsync("ElementUpdated", element);
        }
    }

    public async Task RemoveElement(string circuitId, string elementId)
    {
        if (_state.Circuits.TryGetValue(circuitId, out var circuit))
        {
            circuit.Elements.RemoveAll(e => e.Id == elementId);
            // Cascading delete wires
            var removedWires = circuit.Wires.Where(w => w.FromElement == elementId || w.ToElement == elementId).ToList();
            foreach (var w in removedWires) circuit.Wires.Remove(w);

            await Clients.Group(circuitId).SendAsync("ElementRemoved", elementId);
            foreach (var w in removedWires)
                await Clients.Group(circuitId).SendAsync("WireRemoved", w.Id);
        }
    }

    public async Task AddWire(string circuitId, Wire wire)
    {
        if (_state.Circuits.TryGetValue(circuitId, out var circuit))
        {
            if (!circuit.Wires.Any(w => w.Id == wire.Id))
            {
                circuit.Wires.Add(wire);
                await Clients.Group(circuitId).SendAsync("WireAdded", wire);
            }
        }
    }

    public async Task RemoveWire(string circuitId, string wireId)
    {
        if (_state.Circuits.TryGetValue(circuitId, out var circuit))
        {
            circuit.Wires.RemoveAll(w => w.Id == wireId);
            await Clients.Group(circuitId).SendAsync("WireRemoved", wireId);
        }
    }

    public async Task UpdateCursor(string circuitId, double x, double y)
    {
        await Clients.GroupExcept(circuitId, Context.ConnectionId)
                     .SendAsync("CursorMoved", Context.ConnectionId, x, y);
    }
}
