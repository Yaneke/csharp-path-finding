using System.Collections.Generic;
using graph;

namespace search {
    class MAPF {
        public static List<Path> CBS(Graph graph, Dictionary<Vertex, Vertex> sourceDestinations) {
            List<Path> res = new List<Path>();
            foreach (var path in sourceDestinations) {
                Vertex source = path.Key;
                Vertex destination = path.Value;
                res.Add(Astar.ShortestPath(graph, source, destination));
            }
            return res;
        }
    }
}