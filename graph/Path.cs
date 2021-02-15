using System;
using System.Collections.Generic;
using search;

namespace graph {
    public class Path : IComparable<Path> {
        public List<Vertex> vertexPath { get; }
        public List<Edge> edgePath { get; }
        public Vertex vertex { get; }
        //public int length { get; }
        private float gCost;
        private float hCost;
        public float cost {
            get {
                return this.gCost + this.hCost;
            }
        }

        /// <summary> Create a Path from the source. <summary>
        public Path(Vertex source) {
            this.vertexPath = new List<Vertex>();
            this.vertexPath.Add(source);
            this.edgePath = new List<Edge>();
            this.gCost = 0f;
            this.hCost = 0f;
            //this.length = 0;
            this.vertex = source;
        }

        /// <summary> Build a NodePath from an edge. </summary>
        public Path(Path parent, Edge edge, float hCost) {
            this.vertex = edge.neighbour;
            this.vertexPath = new List<Vertex>(parent.vertexPath);
            this.vertexPath.Add(edge.neighbour);
            this.edgePath = new List<Edge>(parent.edgePath);
            this.edgePath.Add(edge);
            this.gCost = parent.gCost + edge.cost;
            this.hCost = hCost;
            //this.length = parent.length + 1;
        }

        public List<Edge> GetEdges() {
            return this.vertex.GetEdges();
        }

        /// <summary>Two paths compare to each other in terms of cost. In case of equality, 
        /// we compare the heuristic values.</summary>
        public int CompareTo(Path other) {
            if (this.cost < other.cost) {
                return -1;
            } else if (this.cost > other.cost) {
                return 1;
            } else { // If both paths have the same cost, compare the heuristics
                if (this.hCost > other.hCost) {
                    return 1;
                } else if (this.hCost < other.hCost) {
                    return -1;
                }
            }
            return 0;
        }

        public override int GetHashCode() {
            return this.vertex.GetHashCode();
        }

        public HashSet<Constraint> ToConstraints() {
            HashSet<Constraint> constraints = new HashSet<Constraint>();
            constraints.Add(new Constraint(this.vertex, this.edgePath.Count));
            CardinalDirection direction = this.edgePath[this.edgePath.Count - 1].ComputeDirection();
            constraints.Add(new Constraint(direction, this.edgePath.Count));
            return constraints;
        }

        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != this.GetType()) {
                return false;
            }
            if (this == obj) {
                return true;
            }
            Path other = (Path)obj;
            return other.vertex.Equals(this.vertex);
        }

        public override string ToString() {
            String res = "";
            String sep = "";
            for (int i = 0; i < this.vertexPath.Count; i++) {
                res += sep + this.vertexPath[i].ToString();
                sep = ", ";
            }
            return res;
        }


    }

}

