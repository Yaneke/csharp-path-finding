namespace PathFinding.Graphs {

    public class Edge {
        public Vertex start;
        public Vertex neighbour;
        public float cost;

        public Edge(Vertex start, Vertex end, float cost) {
            this.start = start;
            this.neighbour = end;
            this.cost = cost;
        }

        public CardinalDirection ComputeDirection() {
            GridVertex src = (GridVertex)this.start;
            GridVertex dst = (GridVertex)this.neighbour;
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

        public override string ToString() {
            return this.start.ToString() + "->" + this.neighbour.ToString();
        }


        public override int GetHashCode() {
            return (this.cost, this.start.GetHashCode(), this.neighbour.GetHashCode()).GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != this.GetType()) {
                return false;
            }
            Edge other = (Edge)obj;
            return this.cost == other.cost && this.start.Equals(other.start) && this.neighbour.Equals(other.neighbour);
        }
    }
}