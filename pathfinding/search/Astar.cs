using System.Collections.Generic;
using System;
using PathFinding.Graphs;
using PathFinding.DataStructures;
using Priority_Queue;


namespace PathFinding.Search {
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
                    GridVertex gv = (GridVertex)neighbourPath.vertex;
                    HashSet<Constraint> asConstraints = neighbourPath.ToConstraints();
                    if (!constraints.Overlaps(asConstraints)) {
                        // The path to the neighbour will be considered either if it is unknown or if it has a lower cost than the previously known one
                        if (!bestPaths.ContainsKey(neighbourPath.vertex)) {
                            bestPaths.Add(neighbourPath.vertex, neighbourPath);
                            openSet.Add(neighbourPath);
                        } else if (neighbourPath.CompareTo(bestPaths[neighbourPath.vertex]) < 0) {
                            bestPaths[neighbourPath.vertex] = neighbourPath;
                            if (openSet.Contains(neighbourPath)) {
                                openSet.Update(neighbourPath);
                            } else {
                                openSet.Add(neighbourPath);
                            }
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

        public static Path NewShortestPath(Graph graph, Vertex source, Vertex destination, HashSet<Constraint> constraints) {
            Heap<Path> openSet = new Heap<Path>();
            openSet.Add(new Path(source));
            Path currentPath = null;
            do {
                currentPath = openSet.Pop();
                Console.WriteLine("Current node = " + currentPath.vertex);
                if (currentPath.vertex.Equals(destination)) {
                    return currentPath;
                }
                foreach (Edge e in currentPath.GetEdges()) {
                    Path neighbourPath = new Path(currentPath, e, graph.HCost(e.neighbour, destination));
                    var asConstraints = neighbourPath.ToConstraints();
                    if (!constraints.Overlaps(asConstraints)) {
                        if (openSet.Contains(neighbourPath)) {
                            Console.WriteLine(neighbourPath);
                            openSet.UpdateIfBetter(neighbourPath);
                        } else {
                            openSet.Add(neighbourPath);
                        }
                    }
                }
            } while (openSet.Count > 0);
            return null;
        }


        public static Path FastShortestPath(Graph graph, Vertex source, Vertex destination) {
            return FastShortestPath(graph, source, destination, new HashSet<Constraint>());
        }

        /// <summary>
        /// A* implementation where the algorithm takes a set of constraints into account (i.e. vertex-time tuples to avoid).
        /// It uses the FastPriorityQueue instead of the custom Heap data structure.
        /// ** THIS APPEARS TO BE SLOWER -> SEE BENCHMARKS **
        /// </summary>
        public static Path FastShortestPath(Graph graph, Vertex source, Vertex destination, HashSet<Constraint> constraints) {
            if (!graph.Contains(source) || !graph.Contains(destination)) {
                return null;
            }
            FastPriorityQueue<AstarNode> openSet = new FastPriorityQueue<AstarNode>(graph.VertexCount);
            Dictionary<Vertex, AstarNode> bestPaths = new Dictionary<Vertex, AstarNode>(); // Best paths known so far for each vertex, used as closedSet
            AstarNode sourceNode = new AstarNode(new Path(source));
            bestPaths.Add(source, sourceNode);
            openSet.Enqueue(sourceNode, sourceNode.path.cost);
            AstarNode currentNode;
            do {
                currentNode = openSet.Dequeue();
                foreach (var edge in currentNode.path.vertex.GetEdges()) {
                    Path neighbourPath = new Path(currentNode.path, edge, graph.HCost(edge.neighbour, destination));
                    if (!constraints.Overlaps(neighbourPath.ToConstraints())) {
                        // The path to the neighbour will be considered either if it is unknown or if it has a lower cost than the previously known one
                        AstarNode neighbourNode;
                        if (!bestPaths.ContainsKey(neighbourPath.vertex)) {
                            neighbourNode = new AstarNode(neighbourPath);
                            bestPaths.Add(neighbourPath.vertex, neighbourNode);
                            openSet.Enqueue(neighbourNode, neighbourPath.cost);
                        } else if (neighbourPath.CompareTo(bestPaths[neighbourPath.vertex].path) < 0) {
                            neighbourNode = bestPaths[neighbourPath.vertex];
                            neighbourNode.path = neighbourPath; // <== This updates the path
                            openSet.UpdatePriority(neighbourNode, neighbourPath.cost);
                        }
                    }
                }
            } while (!currentNode.path.vertex.Equals(destination) && openSet.Count > 0);

            // Handles the case where there is no path.
            if (!bestPaths.ContainsKey(destination)) {
                return null;
            }
            return bestPaths[destination].path;
        }
    }
}