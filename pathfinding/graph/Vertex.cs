using System.Collections.Generic;

namespace graph {
    public class Vertex {
        protected List<Edge> edges;
        public string id { get; set; }

        public Vertex(string id) {
            this.edges = new List<Edge>();
            this.id = id;
        }

        public void AddNeighbour(Vertex v, float cost) {
            if (v != null) {
                this.edges.Add(new Edge(this, v, cost));
            }
        }

        public List<Edge> GetEdges() {
            return this.edges;
        }

        public override string ToString() {
            return this.id;
        }

        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != this.GetType()) {
                return false;
            }
            Vertex other = (Vertex)obj;
            return other.id == this.id;
        }


    }
}
