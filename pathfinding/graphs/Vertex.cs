using System.Collections.Generic;

namespace PathFinding.Graphs {
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

        /*
        public void AddNeighbour(Vertex v, float cost, bool isBidirectional) {
            if (v != null) {
                if (isBidirectional) {
                    BidirectionalEdge edge = new BidirectionalEdge(this, v, cost);
                    if (!v.GetEdges().Contains(edge)) {
                        this.edges.Add(edge);
                        v.edges.Add(edge);
                    }
                } else {
                    this.edges.Add(new Edge(this, v, cost));
                }

            }
        }
        */

        public List<Edge> GetEdges() {
            return this.edges;
        }

        public Edge GetEdgeTo(Vertex neighbour) {
            foreach (var edge in this.edges) {
                if (edge.neighbour.Equals(neighbour)) {
                    return edge;
                }
            }
            return null;
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
