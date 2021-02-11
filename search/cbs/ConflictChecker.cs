using graph;
using System.Collections.Generic;

namespace search.cbs {

    public abstract class ConflictChecker {
        public abstract Conflict Check(Solution solution, int timestep);

        public override int GetHashCode() {
            return this.GetType().ToString().GetHashCode();
        }
    }



    public class VertexConflictChecker : ConflictChecker {

        public override Conflict Check(Solution solution, int timestep) {
            Dictionary<Vertex, int> currentVertices = new Dictionary<Vertex, int>();
            for (int agent = 0; agent < solution.AgentCount; agent++) {
                Path agentPath = solution.GetPath(agent);
                if (timestep < agentPath.edgePath.Count) {
                    Vertex currentVertex = agentPath.edgePath[timestep].neighbour;
                    if (!currentVertices.TryAdd(currentVertex, agent)) {
                        return new VertexConflict(currentVertex, timestep + 1, agent, currentVertices[currentVertex]);
                    }
                }
            }
            return null;
        }
    }


    public class FollowingConflictChecker : ConflictChecker {

        public override Conflict Check(Solution solution, int timestep) {
            Dictionary<Vertex, int> previousVertices = new Dictionary<Vertex, int>();
            for (int agent = 0; agent < solution.AgentCount; agent++) {
                Path agentPath = solution.GetPath(agent);
                if (timestep < agentPath.edgePath.Count) {
                    // Add the start position of each agent
                    Vertex previousVertex = agentPath.edgePath[timestep].start;
                    previousVertices.Add(previousVertex, agent);
                }
            }
            // Then check for conflicts
            for (int agent = 0; agent < solution.AgentCount; agent++) {
                Path agentPath = solution.GetPath(agent);
                if (timestep < agentPath.edgePath.Count) {
                    Vertex currentVertex = agentPath.edgePath[timestep].neighbour;
                    if (previousVertices.ContainsKey(currentVertex) && previousVertices[currentVertex] != agent) {
                        return new FollowingConflict(currentVertex, timestep + 1, previousVertices[currentVertex], agent);
                    }
                }
            }
            return null;
        }
    }

    public class CardinalConflictChecker : ConflictChecker {

        /// <summary>
        /// A Cardinal Conflict happens when two agents take the same direction at a given timestep.
        /// </summary>
        /// We have to remember the destination vertex of each conflicting agent to be able to 
        /// the corresponding issue constraints.
        public override Conflict Check(Solution solution, int timestep) {
            Dictionary<CardinalDirection, (int, Vertex)> directionTaken = new Dictionary<CardinalDirection, (int, Vertex)>();
            for (int agent = 0; agent < solution.AgentCount; agent++) {
                Path agentPath = solution.GetPath(agent);
                if (timestep < agentPath.edgePath.Count) {
                    Edge currentEdge = agentPath.edgePath[timestep];
                    CardinalDirection direction = this.ComputeDirection(currentEdge);
                    if (directionTaken.ContainsKey(direction)) {
                        (int conflictingAgent, Vertex conflictingAgentVertex) = directionTaken[direction];
                        return new CardinalConflict(conflictingAgent, conflictingAgentVertex, agent, currentEdge.neighbour, timestep + 1, direction);
                    }
                    directionTaken.Add(direction, (agent, currentEdge.neighbour));
                }
            }
            return null;
        }


        private CardinalDirection ComputeDirection(Edge e) {
            GridVertex src = (GridVertex)e.start;
            GridVertex dst = (GridVertex)e.neighbour;
            int delta_i = dst.i - src.i;
            if (delta_i == 1) {
                return CardinalDirection.South;
            } else if (delta_i == -1) {
                return CardinalDirection.North;
            } else {
                int delta_j = dst.j - src.j;
                if (delta_j == 1) {
                    return CardinalDirection.East;
                } else if (delta_j == -1) {
                    return CardinalDirection.West;
                }
            }
            return CardinalDirection.None;
        }
    }
}