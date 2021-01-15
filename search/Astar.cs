using System.Collections.Generic;
using System;
using graph;
using data_structures;

namespace search {
    /// <summary>
    /// This class provides A* based algorithms.
    /// </summary>
    class Astar {
        /// <summary>
        /// Basic A*: shortest path from source to destination.
        /// </summary>
        public static Path ShortestPath(Graph graph, Vertex source, Vertex destination) {
            return ShortestPath(graph, source, destination, new HashSet<Constraint>());
        }

        /// <summary>
        /// A* implementation where the algorithm takes a set of constraints into account (i.e. vertex-time tuples to avoid).
        /// </summary>
        public static Path ShortestPath(Graph graph, Vertex source, Vertex destination, HashSet<Constraint> constraints) {
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
                    Path neighbourPath = new Path(currentPath, edge, graph.HCost(edge.neighbour, destination));
                    if (!constraints.Contains(neighbourPath.ToConstraint())) {
                        // The path to the neighbour will be considered either if it is unknown or if it has a lower cost than the previously known one
                        if (!bestPaths.ContainsKey(neighbourPath.vertex)) {
                            bestPaths.Add(neighbourPath.vertex, neighbourPath);
                            openSet.Add(neighbourPath);
                        } else if (neighbourPath.CompareTo(bestPaths[neighbourPath.vertex]) < 0) {
                            bestPaths[neighbourPath.vertex] = neighbourPath;
                            openSet.Update(neighbourPath);
                        }
                    }
                }
            } while (!currentPath.vertex.Equals(destination) && openSet.Count > 0);

            // Handles the case where there is no path.
            if (!bestPaths.ContainsKey(destination)) {
                return null;
            }
            return bestPaths[destination];
        }
    }
}