using System.Collections.Generic;
using graph;

namespace visualisation.data_objects {
    class PathRequestDO {
        public List<CoordinateDO> start { get; set; }
        public List<CoordinateDO> end { get; set; }

        public override string ToString() {
            return "start: " + this.start.ToString() + ", end = " + this.end.ToString();
        }

        public List<Vertex> GetSources(GridGraph graph) {
            List<Vertex> sources = new List<Vertex>();
            foreach (var coord in this.start) {
                Vertex v = graph.GetVertexAt(coord.y, coord.x);
                if (v == null) {
                    throw new System.Exception("Coordinate " + coord.ToString() + " cannot be a source: it does not belong to the graph");
                }
                sources.Add(v);
            }
            return sources;
        }

        public List<Vertex> GetDestinations(GridGraph graph) {
            List<Vertex> destinations = new List<Vertex>();
            foreach (var coord in this.end) {
                Vertex v = graph.GetVertexAt(coord.y, coord.x);
                if (v == null) {
                    throw new System.Exception("Coordinate " + coord.ToString() + " cannot be a destination: it does not belong to the graph");
                }
                destinations.Add(v);
            }
            return destinations;
        }

        // override object.Equals
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            PathRequestDO other = (PathRequestDO)obj;
            if (this.start.Count != other.start.Count || this.end.Count != other.end.Count) {
                return false;
            }
            for (int i = 0; i < this.start.Count; i++) {
                if (!this.start[i].Equals(other.start[i])) {
                    return false;
                }
            }
            for (int i = 0; i < this.end.Count; i++) {
                if (!this.end[i].Equals(other.end[i])) {
                    return false;
                }
            }
            return true;
        }

        // override object.GetHashCode
        public override int GetHashCode() {
            // TODO: write your implementation of GetHashCode() here
            throw new System.NotImplementedException();
            return base.GetHashCode();
        }

    }
}