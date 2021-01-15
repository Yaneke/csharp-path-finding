using System.Collections.Generic;
using graph;
using System;


namespace search {
    public class Solution {
        private Dictionary<int, Path> agentsPaths;
        public float cost {
            get {
                float res = 0;
                foreach (var path in this.agentsPaths.Values) {
                    res += path.cost;
                }
                return res;
            }
        }

        public Solution() {
            this.agentsPaths = new Dictionary<int, Path>();
        }

        public void Add(int agent_num, Path path) {
            this.agentsPaths.Add(agent_num, path);
        }

        public Path GetPath(int agent) {
            return this.agentsPaths[agent];
        }

        public bool IsValid() {
            return this.GetFirstConflict() == null;
        }

        /// <summary>
        /// Find the first conflict in a solution if any. Returns null if none.
        /// </summary>
        /// NB: This is implemented in a rather non-intuitive manner in order to spot 
        /// conflicts as soon as possible.
        public Conflict GetFirstConflict() {
            // Check initial positions
            Dictionary<Vertex, int> previousVertices = new Dictionary<Vertex, int>();
            foreach (var entry in this.agentsPaths) {
                int agent = entry.Key;
                Path path = entry.Value;
                if (previousVertices.ContainsKey(path.vertexPath[0])) {
                    throw new Exception("Agents cannot have the same initial position!");
                }
                previousVertices.Add(path.vertexPath[0], agent);
            }
            // Check max path length
            int nsteps = 0;
            foreach (var path in this.agentsPaths.Values) {
                if (path.edgePath.Count > nsteps) {
                    nsteps = path.edgePath.Count;
                }
            }
            // For each time step.
            for (int t = 0; t < nsteps; t++) {
                Dictionary<Vertex, int> currentVertices = new Dictionary<Vertex, int>();
                // For each agent
                foreach (var entry in this.agentsPaths) {
                    int agent = entry.Key;
                    Path path = entry.Value;
                    if (t < path.edgePath.Count) {
                        Vertex vertex = path.edgePath[t].neighbour;
                        // If the destination vertex is already occupied at time t+1, then there is a conflict at time t+1.
                        if (currentVertices.ContainsKey(vertex)) {
                            return new VertexConflict(vertex, t + 1, agent, currentVertices[vertex]);
                        }
                        // If the destination vertex was already occupied at time t and we move to it at t+1, then there is a conflict at time t+1.
                        if (previousVertices.ContainsKey(vertex) && previousVertices[vertex] != agent) {
                            return new FollowingConflict(vertex, t + 1, previousVertices[vertex], agent);
                        }
                        currentVertices[vertex] = agent;
                    }
                    // TODO: check for edge conflicts
                }
                previousVertices = currentVertices;
            }
            return null;
        }

        public override string ToString() {
            String res = "Solution: cost=" + this.cost;
            String sep = " ";
            foreach (var entry in this.agentsPaths) {
                int agent = entry.Key;
                Path path = entry.Value;
                res += sep + "agent" + agent + ": " + path.cost;
                sep = ", ";
            }

            return res;
        }
    }
}