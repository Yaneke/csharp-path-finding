using Priority_Queue;
using PathFinding.Graphs;

namespace PathFinding.DataStructures {
    public class AstarNode : FastPriorityQueueNode {

        public Path path { get; set; }

        public AstarNode(Path p) : base() {
            this.path = p;
        }

        // override object.Equals
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            AstarNode other = (AstarNode)obj;
            return this.path.Equals(other.path);
        }

        // override object.GetHashCode
        public override int GetHashCode() {
            return this.path.GetHashCode();
        }

    }

}