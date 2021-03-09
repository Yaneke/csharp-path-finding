using System;

namespace PathFinding.Search.CBS {
    public class CBSNode : IComparable<CBSNode> {
        public ConstraintSet constraints { get; set; }
        public Solution solution { get; set; }

        public CBSNode(ConstraintSet constraints, Solution solution) {
            this.constraints = constraints;
            this.solution = solution;
        }


        /// A node is smaller when its cost is lower than the other.
        /// In case of ties, the node with lowest number of constraints is considered to be lower.
        public int CompareTo(CBSNode other) {
            if (this.solution.cost < other.solution.cost) {
                return -1;
            } else if (this.solution.cost == other.solution.cost) {
                if (this.constraints.Count < other.constraints.Count) {
                    return -1;
                } else {
                    return 1;
                }
            }
            return 1;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            CBSNode obj2 = (CBSNode)obj;
            return obj2.constraints.Equals(this.constraints);
        }

        public override int GetHashCode() {
            int h = this.constraints.GetHashCode();
            return h;
        }
    }
}