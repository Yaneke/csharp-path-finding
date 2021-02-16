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

    public class EdgeConflictChecker : ConflictChecker {

        /// <summary>
        /// A Cardinal Conflict happens when two agents take the same direction at a given timestep.
        /// </summary>
        /// We have to remember the destination vertex of each conflicting agent to be able to 
        /// the corresponding issue constraints.
        public override Conflict Check(Solution solution, int timestep) {
            Dictionary<Edge, int> occupiedEdges = new Dictionary<Edge, int>();
            for (int agent = 0; agent < solution.AgentCount; agent++) {
                Path agentPath = solution.GetPath(agent);
                if (timestep < agentPath.edgePath.Count) {
                    Edge currentEdge = agentPath.edgePath[timestep];
                    if (!occupiedEdges.TryAdd(currentEdge, agent)) {
                        return new EdgeConflict(currentEdge, timestep, agent, occupiedEdges[currentEdge]);
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
            Dictionary<CardinalDirection, int> directionTaken = new Dictionary<CardinalDirection, int>();
            for (int agent = 0; agent < solution.AgentCount; agent++) {
                Path agentPath = solution.GetPath(agent);
                if (timestep < agentPath.edgePath.Count) {
                    Edge currentEdge = agentPath.edgePath[timestep];
                    CardinalDirection direction = currentEdge.ComputeDirection();
                    if (directionTaken.ContainsKey(direction)) {
                        int conflictingAgent = directionTaken[direction];
                        return new CardinalConflict(conflictingAgent, agent, direction, timestep + 1);
                    }
                    directionTaken.Add(direction, agent);
                }
            }
            return null;
        }
    }
}