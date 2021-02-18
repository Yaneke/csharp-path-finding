using graph;
using System.Collections.Generic;

namespace search {

    public abstract class Conflict {
        public int timestep { get; }
        public object constrainedObject { get; }
        public int[] agents { get; }

        /// <summary> 
        /// Returns the list of agents involved in the conflict.
        /// </summary>
        public abstract List<int> GetAgents();

        /// <summary> 
        /// Returns the constraint associated to the conflict if any. Returns null otherwise.
        /// </summary>
        public abstract Constraint GetConstraint(int agent);

        public Conflict(int timestep, object constrainedObject, int[] agents) {
            this.timestep = timestep;
            this.constrainedObject = constrainedObject;
            this.agents = agents;
        }
    }

    public class VertexConflict : Conflict {
        public int agent1 { get; }
        public int agent2 { get; }
        public Vertex vertex { get; }

        public VertexConflict(Vertex vertex, int timestep, int agent1, int agent2) : base(timestep, vertex, new int[] { agent1, agent2 }) {
            this.vertex = vertex;
            this.agent1 = agent1;
            this.agent2 = agent2;
        }

        public override List<int> GetAgents() {
            List<int> res = new List<int>();
            res.Add(this.agent1);
            res.Add(this.agent2);
            return res;
        }

        public override Constraint GetConstraint(int agent) {
            if (agent == this.agent1) {
                return new Constraint(this.vertex, this.timestep);
            } else if (agent == agent2) {
                return new Constraint(this.vertex, this.timestep);
            }
            return null;
        }

        public override string ToString() {
            return "Vertex conflict at t=" + this.timestep + " v=" + this.vertex.ToString() + " for agents " + this.agent1 + " and " + this.agent2;
        }

    }

    /// When Agent 1 is at vertex V1 at time t-1 and Agent 2 is at V1 at time t.
    public class FollowingConflict : Conflict {
        public int leaving_agent { get; }
        public int following_agent { get; }
        public Vertex vertex { get; }

        public FollowingConflict(Vertex vertex, int timestep, int leaving_agent, int following_agent) : base(timestep, vertex, new int[] { leaving_agent, following_agent }) {
            this.vertex = vertex;
            this.leaving_agent = leaving_agent;
            this.following_agent = following_agent;
        }

        public override List<int> GetAgents() {
            List<int> res = new List<int>();
            res.Add(this.leaving_agent);
            res.Add(this.following_agent);
            return res;
        }

        public override Constraint GetConstraint(int agent) {
            // Do not create constraints for t=0 (initial position).
            if (agent == this.leaving_agent && this.timestep > 1) {
                return new Constraint(this.vertex, this.timestep - 1);
            } else if (agent == following_agent) {
                return new Constraint(this.vertex, this.timestep);
            }
            return null;
        }


        public override string ToString() {
            return "Following conflict at t=" + this.timestep + " v=" + this.vertex.ToString() + ": agent " + this.leaving_agent + " (leaving) and agent " + this.following_agent + "(following)";
        }
    }


    /// When Agent 1 is at vertex V1 at time t-1 and Agent 2 is at V1 at time t.
    public class EdgeConflict : Conflict {
        public int agent1 { get; }
        public int agent2 { get; }
        public Edge edge { get; }

        public EdgeConflict(Edge edge, int timestep, int agent1, int agent2) : base(timestep, edge, new int[] { agent1, agent2 }) {
            this.edge = edge;
            this.agent1 = agent1;
            this.agent2 = agent2;
        }

        public override List<int> GetAgents() {
            List<int> res = new List<int>();
            res.Add(this.agent1);
            res.Add(this.agent2);
            return res;
        }

        public override Constraint GetConstraint(int agent) {
            if (agent == this.agent1 || agent == this.agent2) {
                return new Constraint(this.edge, this.timestep);
            }
            throw new System.Exception("The given agent is not part of the conflict!");
        }


        public override string ToString() {
            return "Edge conflict for edge " + this.edge.ToString() + ": agent " + this.agent1 + " and agent " + this.agent2 + " at t=" + this.timestep;
        }
    }

    public class CardinalConflict : Conflict {
        public int agent1 { get; }
        public int agent2 { get; }
        public CardinalDirection direction { get; }

        public CardinalConflict(int agent1, int agent2, CardinalDirection direction, int timestep) : base(timestep, direction, new int[] { agent1, agent2 }) {
            this.agent1 = agent1;
            this.agent2 = agent2;
            this.direction = direction;
        }

        public override List<int> GetAgents() {
            List<int> res = new List<int>();
            res.Add(this.agent1);
            res.Add(this.agent2);
            return res;
        }

        public override Constraint GetConstraint(int agent) {
            if (agent == this.agent1 || agent == this.agent2) {
                return new Constraint(this.direction, this.timestep);
            }
            throw new System.Exception("The given agent is not in the conflict!");
        }


        public override string ToString() {
            return "Cardinal conflict at t=" + this.timestep + " direction=" + this.direction.ToString() + ": agent " + this.agent1 + " and agent " + this.agent2;
        }

    }
}