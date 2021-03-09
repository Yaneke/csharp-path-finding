using PathFinding.Graphs;
using System;

namespace PathFinding.Search {
    public class Heuristics {
        public static float ManhattanDistance(GridVertex v1, GridVertex v2) {
            return Math.Abs(v1.i - v2.i) + Math.Abs(v1.j - v2.j);
        }
    }
}