namespace backend.Models;

public class UserSession
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? CurrentCircuitId { get; set; }
}

public class CircuitProject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int GridSize { get; set; } = 20;
    
    public List<CircuitElement> Elements { get; set; } = new();
    public List<Wire> Wires { get; set; } = new();
    public HashSet<string> ActiveUserIds { get; set; } = new();
}

public class CircuitElement
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public int Value { get; set; } // 0 or 1, for inputs mostly or cached simulation
}

public class Wire
{
    public string Id { get; set; } = string.Empty;
    public string FromElement { get; set; } = string.Empty;
    public string FromPin { get; set; } = string.Empty;
    public string ToElement { get; set; } = string.Empty;
    public string ToPin { get; set; } = string.Empty;
}

public class CursorPosition
{
    public string UserId { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
}
