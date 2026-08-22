using System.Collections.Concurrent;
using backend.Models;

namespace backend.Services;

public class CircuitStateService
{
    // CircuitId -> CircuitProject
    public ConcurrentDictionary<string, CircuitProject> Circuits { get; } = new();
    
    // ConnectionId -> UserSession
    public ConcurrentDictionary<string, UserSession> Users { get; } = new();

    private readonly string[] Colors = { "#ef4444", "#f97316", "#f59e0b", "#84cc16", "#22c55e", "#06b6d4", "#3b82f6", "#6366f1", "#a855f7", "#ec4899" };
    private int _colorIndex = 0;

    public CircuitStateService()
    {
        var bonus = new CircuitProject
        {
            Id = "bonus-challenge",
            Name = "Bonus 2-NOT Challenge",
            Description = "Invert 3 inputs using exactly 2 NOT gates.",
            GridSize = 20
        };
        BonusCircuitGenerator.Generate(bonus);
        Circuits[bonus.Id] = bonus;
    }

    public string RegisterUser(string connectionId, string requestedName)
    {
        string name = requestedName.Trim();
        if (string.IsNullOrEmpty(name)) name = "Anonymous";

        // Deduplicate name
        int count = 1;
        string finalName = name;
        while (Users.Values.Any(u => u.DisplayName == finalName))
        {
            count++;
            finalName = $"{name} {count}";
        }

        var color = Colors[_colorIndex % Colors.Length];
        _colorIndex++;

        var user = new UserSession
        {
            Id = connectionId,
            DisplayName = finalName,
            Color = color
        };

        Users[connectionId] = user;
        return finalName;
    }

    public void RemoveUser(string connectionId)
    {
        if (Users.TryRemove(connectionId, out var user))
        {
            if (user.CurrentCircuitId != null && Circuits.TryGetValue(user.CurrentCircuitId, out var circuit))
            {
                circuit.ActiveUserIds.Remove(connectionId);
            }
        }
    }

    public CircuitProject CreateCircuit(string name, string description, int gridSize)
    {
        var circuit = new CircuitProject
        {
            Id = Guid.NewGuid().ToString(),
            Name = string.IsNullOrEmpty(name) ? "Untitled Circuit" : name,
            Description = description,
            GridSize = gridSize
        };
        Circuits[circuit.Id] = circuit;
        return circuit;
    }
}
