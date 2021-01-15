using graph;
using System.Collections.Generic;

namespace search {

    public abstract class Conflict {

        public abstract List<int> GetAgents();
        public abstract Constraint GetConstraint(int agent);
    }

    public class VertexConflict : Conflict {
        public int agent1 { get; }
        public int agent2 { get; }
        public Vertex vertex { get; }
        public int timestep { get; }

        public VertexConflict(Vertex vertex, int timestep, int agent1, int agent2) : base() {
            this.timestep = timestep;
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
                return new Constraint(this.vertex, this.timestep, this.agent1);
            } else if (agent == agent2) {
                return new Constraint(this.vertex, this.timestep, this.agent2);
            }
            return null;
        }

        public override string ToString() {
            return "Conflict at " + this.vertex.ToString() + " for agents " + this.agent1 + " and " + this.agent2;
        }

    }

    /// When Agent 1 is at vertex V1 at time t-1 and Agent 2 is at V1 at time t.
    public class FollowingConflict : Conflict {
        public int leaving_agent { get; }
        public int following_agent { get; }
        public int timestep { get; }
        public Vertex vertex { get; }

        public FollowingConflict(Vertex vertex, int timestep, int leaving_agent, int following_agent) : base() {
            this.timestep = timestep;
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
                return new Constraint(this.vertex, this.timestep - 1, this.leaving_agent);
            } else if (agent == following_agent) {
                return new Constraint(this.vertex, this.timestep, this.following_agent);
            }
            return null;
        }


        public override string ToString() {
            return "Following conflict at t=" + this.timestep + " v=" + this.vertex.ToString() + ": agent " + this.leaving_agent + " (leaving) and agent " + this.following_agent + "(following)";
        }
    }


    /// When Agent 1 is at vertex V1 at time t-1 and Agent 2 is at V1 at time t.
    public class EdgeConflict : Conflict {
        public int leaving_agent { get; }
        public int following_agent { get; }
        public int timestep { get; }
        public Edge edge { get; }

        public EdgeConflict(Edge edge, int timestep, int leaving_agent, int following_agent) : base() {
            this.timestep = timestep;
            this.edge = edge;
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
            return null;
        }


        public override string ToString() {
            return "Edge conflict for edge " + this.edge.ToString() + ": agent " + this.leaving_agent + " (leaving) and agent " + this.following_agent + "(following)";
        }
    }
}