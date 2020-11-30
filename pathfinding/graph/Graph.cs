using System.Collections.Generic;

namespace graph {
    public abstract class Graph {
        private HashSet<Vertex> vertices;
        public int Count {
            get {
                return vertices.Count;
            }
        }


        public Graph() {
            this.vertices = new HashSet<Vertex>();
        }

        /// Compute the heuristic cost from n1 to n2.
        public abstract float HCost(Vertex source, Vertex destination);

        public bool Contains(Vertex node) {
            return this.vertices.Contains(node);
        }

        protected void Add(Vertex v) {
            if (v != null) {
                this.vertices.Add(v);
            }
        }


        public override string ToString() {
            return "" + this.vertices.Count;
        }
    }
}
