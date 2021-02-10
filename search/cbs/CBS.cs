using graph;
using data_structures;
using System.Collections.Generic;

namespace search.cbs {
    public class CBS {
        public static Solution ShortestPath(Graph graph, List<Vertex> sources, List<Vertex> destinations) {
            ConstraintSet emptyConstraints = new ConstraintSet();
            Solution solution = CBS.LowLevelSearch(graph, sources, destinations, emptyConstraints);
            CBSNode root = new CBSNode(emptyConstraints, solution);
            Heap<CBSNode> ct = new Heap<CBSNode>();
            ct.Add(root);
            do {
                CBSNode bestNode = ct.Pop();
                Conflict conflict = bestNode.solution.GetFirstConflict();
                if (conflict == null) { // No conflict -> found a solution
                    return bestNode.solution;
                }
                // For each agent in the conflict, create a new node.
                foreach (int agent in conflict.GetAgents()) {
                    ConstraintSet constraints = bestNode.constraints.Clone();
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