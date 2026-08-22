using backend.Models;
using System.Collections.Generic;

namespace backend.Services;

public static class BonusCircuitGenerator
{
    private static int _idCounter = 1;
    private static string NextId() => $"b_{_idCounter++}";

    public static void Generate(CircuitProject circuit)
    {
        var elements = circuit.Elements;
        var wires = circuit.Wires;
        
        double x = 100, y = 100;
        
        // Helper to add element
        CircuitElement AddEl(string type, double ex, double ey) {
            var el = new CircuitElement { Id = NextId(), Type = type, X = ex, Y = ey };
            elements.Add(el);
            return el;
        }
        
        // Helper to add wire
        void WireUp(CircuitElement from, string fromPin, CircuitElement to, string toPin) {
            wires.Add(new Wire { Id = NextId(), FromElement = from.Id, FromPin = fromPin, ToElement = to.Id, ToPin = toPin });
        }
        
        // Helper for 2-input gates
        CircuitElement AddGate(string type, CircuitElement inA, string pinA, CircuitElement inB, string pinB, double ex, double ey) {
            var gate = AddEl(type, ex, ey);
            WireUp(inA, pinA, gate, "in-A");
            WireUp(inB, pinB, gate, "in-B");
            return gate;
        }

        // 1. Inputs
        var X = AddEl("INPUT", 100, 200);
        var Y = AddEl("INPUT", 100, 400);
        var Z = AddEl("INPUT", 100, 600);
        
        // 2. S1 = X OR Y OR Z
        var or_xy = AddGate("OR", X, "out-main", Y, "out-main", 300, 150);
        var S1 = AddGate("OR", or_xy, "out-main", Z, "out-main", 450, 150);
        
        // 3. S2 = (X AND Y) OR (Y AND Z) OR (Z AND X)
        var and_xy = AddGate("AND", X, "out-main", Y, "out-main", 300, 300);
        var and_yz = AddGate("AND", Y, "out-main", Z, "out-main", 300, 400);
        var and_zx = AddGate("AND", Z, "out-main", X, "out-main", 300, 500);
        var or_xy_yz = AddGate("OR", and_xy, "out-main", and_yz, "out-main", 450, 350);
        var S2 = AddGate("OR", or_xy_yz, "out-main", and_zx, "out-main", 600, 400);
        
        // 4. S3 = X AND Y AND Z
        var and_xyz_1 = AddGate("AND", X, "out-main", Y, "out-main", 300, 650);
        var S3 = AddGate("AND", and_xyz_1, "out-main", Z, "out-main", 450, 650);
        
        // 5. N1 = NOT(S2)
        var N1 = AddEl("NOT", 750, 400);
        WireUp(S2, "out-main", N1, "in-A"); // NOT gates only have in-A in our model
        
        // 6. N2 = NOT(S3 OR (S1 AND N1))
        var s1_and_n1 = AddGate("AND", S1, "out-main", N1, "out-main", 900, 250);
        var t2 = AddGate("OR", S3, "out-main", s1_and_n1, "out-main", 1050, 450);
        var N2 = AddEl("NOT", 1200, 450);
        WireUp(t2, "out-main", N2, "in-A");
        
        // 7. W1 = N1 AND N2
        var W1 = AddGate("AND", N1, "out-main", N2, "out-main", 1350, 350);
        
        // 8. Generate ~X
        // ~X = W1 OR (N1 AND (Y OR Z)) OR (N2 AND (Y AND Z))
        var y_or_z = AddGate("OR", Y, "out-main", Z, "out-main", 1000, 50);
        var n1_y_or_z = AddGate("AND", N1, "out-main", y_or_z, "out-main", 1200, 100);
        var y_and_z = AddGate("AND", Y, "out-main", Z, "out-main", 1000, 800);
        var n2_y_and_z = AddGate("AND", N2, "out-main", y_and_z, "out-main", 1200, 750);
        
        var x_or1 = AddGate("OR", W1, "out-main", n1_y_or_z, "out-main", 1500, 150);
        var nx = AddGate("OR", x_or1, "out-main", n2_y_and_z, "out-main", 1650, 200);
        var out_X = AddEl("OUTPUT", 1800, 200);
        WireUp(nx, "out-main", out_X, "in-A");
        
        // 9. Generate ~Y
        // ~Y = W1 OR (N1 AND (X OR Z)) OR (N2 AND (X AND Z))
        var x_or_z = AddGate("OR", X, "out-main", Z, "out-main", 1000, 150);
        var n1_x_or_z = AddGate("AND", N1, "out-main", x_or_z, "out-main", 1200, 200);
        var x_and_z = AddGate("AND", X, "out-main", Z, "out-main", 1000, 900);
        var n2_x_and_z = AddGate("AND", N2, "out-main", x_and_z, "out-main", 1200, 850);
        
        var y_or1 = AddGate("OR", W1, "out-main", n1_x_or_z, "out-main", 1500, 450);
        var ny = AddGate("OR", y_or1, "out-main", n2_x_and_z, "out-main", 1650, 500);
        var out_Y = AddEl("OUTPUT", 1800, 500);
        WireUp(ny, "out-main", out_Y, "in-A");
        
        // 10. Generate ~Z
        // ~Z = W1 OR (N1 AND (X OR Y)) OR (N2 AND (X AND Y))
        var x_or_y = AddGate("OR", X, "out-main", Y, "out-main", 1000, 250);
        var n1_x_or_y = AddGate("AND", N1, "out-main", x_or_y, "out-main", 1200, 300);
        var n2_x_and_y = AddGate("AND", N2, "out-main", and_xy, "out-main", 1200, 950);
        
        var z_or1 = AddGate("OR", W1, "out-main", n1_x_or_y, "out-main", 1500, 750);
        var nz = AddGate("OR", z_or1, "out-main", n2_x_and_y, "out-main", 1650, 800);
        var out_Z = AddEl("OUTPUT", 1800, 800);
        WireUp(nz, "out-main", out_Z, "in-A");
    }
}
