using System.Collections.Generic;
using game;
using graph;


namespace search {
    public class Solution {
        private Dictionary<Agent, Path> agentsSolutions;
        public float cost {
            get {
                float res = 0;
                foreach (var path in this.agentsSolutions.Values) {
                    res += path.cost;
                }
                return res;
            }
        }

        public Solution() {
            this.agentsSolutions = new Dictionary<Agent, Path>();
        }

        public Path GetPath(Agent agent) {
            return this.agentsSolutions[agent];
        }

        public bool IsValid() {
            return false;
        }

        public List<VertexConflict> GetConlicts() {
            List<VertexConflict> vertexConflitcs = new List<VertexConflict>();
            List<EdgeConflict> edgeConflicts = new List<EdgeConflict>();
            int maxLen = 0;
            foreach (var path in this.agentsSolutions.Values) {
                if (path.length > maxLen) {
                    maxLen = path.length;
                }
            }
            // For each time step
            for (int i = 0; i < maxLen; i++) {
                Dictionary<Vertex, List<Agent>> occupiedVertices = new Dictionary<Vertex, List<Agent>>();
                Dictionary<Edge, List<Agent>> occupiedEdges = new Dictionary<Edge, List<Agent>>();
                // For each agent
                foreach (var entry in this.agentsSolutions) {
                    Agent agent = entry.Key;
                    Path path = entry.Value;
                    // Check for already occupied vertices
                    if (i < path.vertexPath.Count) {
                        Vertex vertex = path.vertexPath[i];
                        if (occupiedVertices.ContainsKey(vertex)) {
                            foreach (Agent other in occupiedVertices[vertex]) {
                                vertexConflitcs.Add(new VertexConflict(vertex, i, agent, other));
                            }
                        } else {
                            occupiedVertices.Add(vertex, new List<Agent>());
                        }
                        occupiedVertices[vertex].Add(agent);
                    }
                    // Check for already occupied edges
                    if (i < path.edgePath.Count) {
                        Edge edge = path.edgePath[i];
                        if (occupiedEdges.ContainsKey(edge)) {
                            foreach (Agent other in occupiedEdges[edge]) {
                                edgeConflicts.Add(new EdgeConflict(edge, i, agent, other));
                            }
                        } else {
                            occupiedEdges.Add(edge, new List<Agent>());
                        }
                        occupiedEdges[edge].Add(agent);
                    }

                }
            }
            return null;
        }
    }
}