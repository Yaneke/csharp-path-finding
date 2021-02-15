using System.Collections.Generic;


namespace search {
    public class ConstraintSet {
        public int Count { get; private set; }

        private Dictionary<int, HashSet<Constraint>> constraints;

        public ConstraintSet() {
            this.constraints = new Dictionary<int, HashSet<Constraint>>();
            this.Count = 0;
        }

        private ConstraintSet(ConstraintSet toCopy) {
            this.constraints = new Dictionary<int, HashSet<Constraint>>(toCopy.constraints);
            this.Count = toCopy.Count;
        }

        public ConstraintSet Clone() {
            return new ConstraintSet(this);
        }

        public void Add(Constraint constraint, int agent) {
            if (!this.constraints.ContainsKey(agent)) {
                this.constraints.Add(agent, new HashSet<Constraint>());
            }
            if (!this.constraints[agent].Contains(constraint)) {
                this.constraints[agent].Add(constraint);
                this.Count++;
            }
        }

        public void Add(List<Constraint> constraints, int agent) {
            foreach (Constraint c in constraints) {
                this.Add(c, agent);
            }
        }

        public HashSet<Constraint> GetConstraints(int agent_num) {
            if (this.constraints.ContainsKey(agent_num)) {
                return this.constraints[agent_num];
            }
            return new HashSet<Constraint>();

        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            string res = "ConstraintSet count=" + this.Count + "\n";
            foreach (var entry in this.constraints) {
                int agent = entry.Key;
                HashSet<Constraint> constraints = entry.Value;
                res += agent + " => \n";
                foreach (Constraint c in constraints) {
                    res += "\t" + c.ToString();
                }
            }
            return res;
        }
    }


    public class Constraint {
        public object constrainedObject { get; }
        public int timestep { get; }
        private int agent { get; }

        public Constraint(object constrainedObject, int timestep) {
            this.constrainedObject = constrainedObject;
            this.timestep = timestep;
            this.agent = agent;
        }

        public override int GetHashCode() {
            return (this.constrainedObject, this.timestep).GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            Constraint obj2 = (Constraint)obj;
            return obj2.timestep == this.timestep && obj2.constrainedObject.Equals(this.constrainedObject);
        }

        public override string ToString() {
            string type = this.constrainedObject.GetType().ToString();
            return "Constraint " + type + "=" + this.constrainedObject.ToString() + " t=" + this.timestep;
        }
    }

}