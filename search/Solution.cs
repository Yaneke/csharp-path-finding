using System.Collections.Generic;
using graph;
using System;
using search.cbs;


namespace search {
    public class Solution {
        private List<Path> agentsPaths;
        public float cost {
            get {
                float res = 0;
                foreach (var path in this.agentsPaths) {
                    if (path != null) {
                        res += path.cost;
                    }
                }
                return res;
            }
        }
        public int AgentCount {
            get => this.agentsPaths.Count;
        }

        public Solution Clone() {
            Solution s = new Solution();
            foreach (var path in this.agentsPaths) {
                s.Add(path);
            }
            return s;
        }

        public Solution() {
            this.agentsPaths = new List<Path>();
        }

        public void Add(Path path) {
            this.agentsPaths.Add(path);
        }

        public void ReplacePath(int agent, Path newPath) {
            if (agent < this.agentsPaths.Count) {
                this.agentsPaths[agent] = newPath;
            } else {
                throw new Exception("Could not replace the Path of agent " + agent + " bacause there is no path for him.");
            }

        }

        public Path GetPath(int agent) {
            return this.agentsPaths[agent];
        }

        public List<Path> GetPaths() {
            return this.agentsPaths;
        }

        public bool IsValid(HashSet<ConflictChecker> checkers) {
            return this.GetFirstConflict(checkers) == null;
        }

        /// <summary>
        /// Find the first conflict in a solution if any. Returns null otherwise.
        /// </summary>
        /// NB: This is implemented in a rather non-intuitive manner in order to spot 
        /// conflicts as soon as possible.
        public Conflict GetFirstConflict(HashSet<ConflictChecker> checkers) {
            // TODO: include this check in the vertex checks
            // Check initial positions
            Dictionary<Vertex, int> previousVertices = new Dictionary<Vertex, int>();
            for (int agent = 0; agent < this.agentsPaths.Count; agent++) {
                Path path = this.agentsPaths[agent];
                if (path != null) {
                    if (previousVertices.ContainsKey(path.vertexPath[0])) {
                        throw new Exception("Agents cannot have the same initial position!");
                    }
                    previousVertices.Add(path.vertexPath[0], agent);
                }
            }
            // Check max path length
            int nsteps = 0;
            foreach (var path in this.agentsPaths) {
                if (path != null && path.edgePath.Count > nsteps) {
                    nsteps = path.edgePath.Count;
                }
            }

            // For each time step.
            for (int t = 0; t < nsteps; t++) {
                foreach (var checker in checkers) {
                    Conflict c = checker.Check(this, t);
                    if (c != null) {
                        return c;
                    }
                }
                // Dictionary<Vertex, int> currentVertices = new Dictionary<Vertex, int>();
                // Dictionary<CardinalDirection, int> directionsByAgent = new Dictionary<CardinalDirection, int>();
                // // For each agent
                // for (int agent = 0; agent < this.agentsPaths.Count; agent++) {
                //     Path path = this.agentsPaths[agent];
                //     if (path != null && t < path.edgePath.Count) {
                //         Edge currentEdge = path.edgePath[t];
                //         Vertex currentVertex = currentEdge.neighbour;
                //         // If the destination vertex is already occupied at time t+1, then there is a conflict at time t+1.
                //         if (currentVertices.ContainsKey(currentVertex)) {
                //             return new VertexConflict(currentVertex, t + 1, agent, currentVertices[currentVertex]);
                //         }
                //         // If the destination vertex was already occupied at time t and we move to it at t+1, then there is a following conflict at time t+1.
                //         if (previousVertices.ContainsKey(currentVertex) && previousVertices[currentVertex] != agent) {
                //             return new FollowingConflict(currentVertex, t + 1, previousVertices[currentVertex], agent);
                //         }
                //         // CardinalDirection direction = this.ComputeDirection(currentEdge);
                //         // if (direction != CardinalDirection.None && directionsByAgent.ContainsKey(direction)) {
                //         //     int conflictingAgent = directionsByAgent[direction];
                //         //     Vertex agentDestination = this.agentsPaths[conflictingAgent].edgePath[t].neighbour;
                //         //     return new CardinalConflict(agent, currentVertex, conflictingAgent, agentDestination, t + 1, direction);
                //         // }
                //         // directionsByAgent.Add(direction, agent);
                //         currentVertices[currentVertex] = agent;
                //     }
                // }
                // previousVertices = currentVertices;
            }
            return null;
        }

        public override string ToString() {
            String res = "Solution: cost=" + this.cost;
            String sep = " ";
            for (int agent = 0; agent < this.agentsPaths.Count; agent++) {
                Path path = this.agentsPaths[agent];
                res += sep + "agent" + agent + ": " + path.cost;
                sep = ", ";
            }

            return res;
        }
    }
}