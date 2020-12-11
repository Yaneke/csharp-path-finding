using graph;
using game;
using System.Collections.Generic;


namespace search {
    public class ConstraintSet {
        private Dictionary<Agent, HashSet<Constraint>> agentConstraints;

        public ConstraintSet() {
            this.agentConstraints = new Dictionary<Agent, HashSet<Constraint>>();
        }

        public void Add(Agent agent, Constraint constraint) {
            if (this.agentConstraints.ContainsKey(agent)) {
                this.agentConstraints[agent].Add(constraint);
            } else {
                HashSet<Constraint> set = new HashSet<Constraint>();
                set.Add(constraint);
                this.agentConstraints.Add(agent, set);
            }
        }

        public HashSet<Constraint> GetConstraints(Agent agent) {
            return this.agentConstraints[agent];
        }
    }


    ///<summary> The Constraint class deliberatly does not contain a reference to the Agent
    /// because the low-level search of CBS (aka Constrained A*) does not know which agent is concerned.
    /// The Agent is related to the Constraint by the ConstraintSet class.
    /// </summary>
    public class Constraint {
        public Vertex vertex { get; }
        public int timestep { get; }

        public Constraint(Vertex vertex, int timestep) {
            this.vertex = vertex;
            this.timestep = timestep;
        }

        public override int GetHashCode() {
            return (this.vertex, this.timestep).GetHashCode();
        }
    }
}