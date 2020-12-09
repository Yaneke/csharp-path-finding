
namespace graph {
    public class GridVertex : Vertex {
        public int i { get; }
        public int j { get; }

        public GridVertex(int i, int j) : base("(" + i + ", " + j + ")") {
            this.i = i;
            this.j = j;
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != this.GetType()) {
                return false;
            }
            GridVertex other = (GridVertex)obj;
            return other.i == this.i && other.j == this.j;
        }


    }
}
