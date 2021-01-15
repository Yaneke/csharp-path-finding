namespace graph {

    public class Edge {
        public Vertex start;
        public Vertex neighbour;
        public float cost;

        public Edge(Vertex start, Vertex end, float cost) {
            this.start = start;
            this.neighbour = end;
            this.cost = cost;
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

    /*
    public class BidirectionalEdge : Edge {
        public BidirectionalEdge(Vertex v1, Vertex v2, float cost) : base(v1, v2, cost) { }

        public override bool Equals(object obj) {
            if (obj == null || this.GetType() != obj.GetType()) {
                return false;
            }
            Edge other = (Edge)obj;
            return this.cost == other.cost && ((this.start.Equals(other.start) && this.neighbour.Equals(other.neighbour)) || this.start.Equals(other.neighbour) && this.neighbour.Equals(other.start));
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
    */
}