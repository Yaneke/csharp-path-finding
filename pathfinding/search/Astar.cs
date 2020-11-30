using System.Collections.Generic;
using graph;
using data_structures;

namespace search {
    class Astar {
        public static Path ShortestPath(Graph graph, Vertex source, Vertex destination) {
            if (!graph.Contains(source) || !graph.Contains(destination)) {
                return null;
            }
            Heap<Path> openSet = new Heap<Path>();
            Dictionary<Vertex, Path> bestPaths = new Dictionary<Vertex, Path>(); // Best paths known so far for each vertex, used as closedSet
            Path sourcePath = new Path(source);
            openSet.Add(sourcePath);
            Path currentPath;
            do {
                currentPath = openSet.Pop();
                foreach (var edge in currentPath.vertex.GetEdges()) {
                    Path neighbourPath = new Path(edge.neighbour, currentPath, graph.HCost(edge.neighbour, destination), edge.cost);
                    // The path to the neighbour will be considered either if it is unknown or if it has a lower cost than the previously known one
                    if (!bestPaths.ContainsKey(neighbourPath.vertex)) {
                        bestPaths.Add(neighbourPath.vertex, neighbourPath);
                        openSet.Add(neighbourPath);
                    } else if (neighbourPath.CompareTo(bestPaths[neighbourPath.vertex]) < 0) {
                        bestPaths[neighbourPath.vertex] = neighbourPath;
                        openSet.Update(neighbourPath);
                    }
                }
            } while (!currentPath.vertex.Equals(destination) && openSet.Count > 0);
            return bestPaths[destination];
        }
    }
}