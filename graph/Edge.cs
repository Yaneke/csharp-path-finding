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
    }
}