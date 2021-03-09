using PathFinding.Graphs;

namespace PathViewing.Web.DataObjects {
    public class CoordinateDO {
        public int x { get; set; }
        public int y { get; set; }

        public CoordinateDO(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Vertex AsVertex(GridGraph graph) {
            return graph.GetVertexAt(this.y, this.x);
        }

        public override string ToString() {
            return "(" + x + ", " + y + ")";
        }


        // override object.Equals
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            CoordinateDO other = (CoordinateDO)obj;
            return this.x == other.x && this.y == other.y;
        }

        // override object.GetHashCode
        public override int GetHashCode() {
            // TODO: write your implementation of GetHashCode() here
            throw new System.NotImplementedException();
            // return base.GetHashCode();
        }
    }
}