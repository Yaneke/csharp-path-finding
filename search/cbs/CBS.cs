using graph;
using data_structures;
using System.Collections.Generic;

namespace search.cbs {
    public class CBS {
        public static Solution ShortestPath(Graph graph, List<Vertex> sources, List<Vertex> destinations) {
            // TODO: check #sources == #destinations
            ConstraintSet constraints = new ConstraintSet();
            Solution solution = CBS.LowLevelSearch(graph, sources, destinations, constraints);
            CBSNode root = new CBSNode(constraints, solution);
            Heap<CBSNode> ct = new Heap<CBSNode>();
            ct.Add(root);
            do {
                CBSNode bestNode = ct.Pop();
                Conflict conflict = bestNode.solution.GetFirstConflict();
                if (conflict == null) { // No conflict -> found a solution
                    return bestNode.solution;
                }
                // TODO: check if the conflict should merge with an existing one? Does it work with the heap?
                // For each agent in the conflict, create a new node.
                foreach (int agent in conflict.GetAgents()) {
                    constraints = bestNode.constraints.Clone();
                    var cons = conflict.GetConstraint(agent);
                    if (cons != null) {
                        constraints.Add(cons);
                        solution = CBS.LowLevelSearch(graph, sources[agent], destinations[agent], constraints.GetConstraints(agent), bestNode.solution, agent);
                        if (solution != null) {
                            ct.Add(new CBSNode(constraints, solution));
                        }
                    }
                }
            } while (ct.Count > 0);

            return null;
        }

        private static Solution LowLevelSearch(Graph graph, List<Vertex> sources, List<Vertex> destinations, ConstraintSet constraints) {
            Solution s = new Solution();
            for (int i = 0; i < sources.Count; i++) {
                Path path = Astar.ShortestPath(graph, sources[i], destinations[i], constraints.GetConstraints(i));
                s.Add(path);
            }
            return s;
        }


        private static Solution LowLevelSearch(Graph graph, Vertex source, Vertex destination, HashSet<Constraint> constraints, Solution partialSolution, int agent) {
            Solution s = partialSolution.Clone();
            s.ReplacePath(agent, Astar.ShortestPath(graph, source, destination, constraints));
            return s;
        }
    }
}