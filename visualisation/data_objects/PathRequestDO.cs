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
                sources.Add(graph.GetVertexAt(coord.y, coord.x));
            }
            return sources;
        }

        public List<Vertex> GetDestinations(GridGraph graph) {
            List<Vertex> destinations = new List<Vertex>();
            foreach (var coord in this.end) {
                destinations.Add(graph.GetVertexAt(coord.y, coord.x));
            }
            return destinations;
        }

    }
}