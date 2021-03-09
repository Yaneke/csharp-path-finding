using PathFinding.Graphs;
using PathFinding.DataStructures;
using System.Collections.Generic;
using System;

namespace PathFinding.Search.CBS {
    public class CBS {

        public HashSet<ConflictChecker> checkers { get; }

        public CBS() {
            this.checkers = new HashSet<ConflictChecker>();
        }

        public CBS WithVertexConflicts() {
            this.checkers.Add(new VertexConflictChecker());
            return this;
        }

        public CBS WithFollowingConflicts() {
            this.checkers.Add(new FollowingConflictChecker());
            return this;
        }

        public CBS WithCardinalConflicts() {
            this.checkers.Add(new CardinalConflictChecker());
            return this;
        }

        public CBS WithEdgeConflicts() {
            this.checkers.Add(new EdgeConflictChecker());
            return this;
        }

        public static CBS Default() {
            return new CBS().WithFollowingConflicts().WithVertexConflicts().WithEdgeConflicts();
        }

        public Solution ShortestPath(Graph graph, List<Vertex> sources, List<Vertex> destinations) {
            ConstraintSet emptyConstraints = new ConstraintSet();
            Solution solution = CBS.LowLevelSearch(graph, sources, destinations, emptyConstraints);
            CBSNode root = new CBSNode(emptyConstraints, solution);
            Heap<CBSNode> ct = new Heap<CBSNode>();
            ct.Add(root);
            do {
                CBSNode bestNode = ct.Pop();
                Conflict conflict = bestNode.solution.GetFirstConflict(this.checkers);
                Console.WriteLine(conflict);
                if (conflict == null) { // No conflict -> found a solution
                    return bestNode.solution;
                }
                // For each agent in the conflict, create a new node.
                foreach (int agent in conflict.GetAgents()) {
                    ConstraintSet constraints = bestNode.constraints.Clone();
                    var cons = conflict.GetConstraint(agent);
                    if (cons != null) {
                        constraints.Add(cons, agent);
                        solution = CBS.LowLevelSearch(graph, sources[agent], destinations[agent], constraints.GetConstraints(agent), bestNode.solution, agent);
                        if (solution != null) {
                            ct.Add(new CBSNode(constraints, solution));
                        }
                    }
                }
            } while (ct.Count > 0);
            return null;
        }


        public IEnumerator<CBSNode> EnumerateCBSOrder(Graph graph, List<Vertex> sources, List<Vertex> destinations) {
            ConstraintSet emptyConstraints = new ConstraintSet();
            Solution solution = CBS.LowLevelSearch(graph, sources, destinations, emptyConstraints);
            CBSNode root = new CBSNode(emptyConstraints, solution);
            Heap<CBSNode> ct = new Heap<CBSNode>();
            ct.Add(root);
            do {
                CBSNode bestNode = ct.Pop();
                yield return bestNode;
                Conflict conflict = bestNode.solution.GetFirstConflict(this.checkers);
                Console.WriteLine(conflict);
                if (conflict == null) { // No conflict -> found a solution
                    break;
                }
                // For each agent in the conflict, create a new node.
                foreach (int agent in conflict.GetAgents()) {
                    ConstraintSet constraints = bestNode.constraints.Clone();
                    var cons = conflict.GetConstraint(agent);
                    if (cons != null) {
                        constraints.Add(cons, agent);
                        solution = CBS.LowLevelSearch(graph, sources[agent], destinations[agent], constraints.GetConstraints(agent), bestNode.solution, agent);
                        if (solution != null) {
                            ct.Add(new CBSNode(constraints, solution));
                        }
                    }
                }
            } while (ct.Count > 0);
        }



        private static Solution LowLevelSearch(Graph graph, List<Vertex> sources, List<Vertex> destinations, ConstraintSet constraints) {
            Solution s = new Solution();
            for (int i = 0; i < sources.Count; i++) {
                Path path = Astar.ShortestPath(graph, sources[i], destinations[i], constraints.GetConstraints(i));
                if (path == null) {
                    return null;
                }
                s.Add(path);
            }
            return s;
        }


        private static Solution LowLevelSearch(Graph graph, Vertex source, Vertex destination, HashSet<Constraint> constraints, Solution partialSolution, int agent) {
            Solution s = partialSolution.Clone();
            Path newPath = Astar.ShortestPath(graph, source, destination, constraints);
            if (newPath == null) {
                return null;
            }
            s.ReplacePath(agent, newPath);
            return s;
        }
    }
}