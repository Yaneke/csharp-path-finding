using System.Collections.Generic;
using System;

namespace visualisation.data_objects {
    public class Path {
        public List<Coordinate> coordinates { get; set; }

        public Path(graph.Path path) {
            this.coordinates = new List<Coordinate>();
            foreach (var vertex in path.ToList()) {
                if (vertex is graph.GridVertex) {
                    var gridVertex = (graph.GridVertex)vertex;
                    this.coordinates.Add(new Coordinate(gridVertex.j, gridVertex.i));
                } else {
                    Console.WriteLine("Not a grid vertex!");
                    throw new Exception("Not yet implemented !");
                }
            }
        }

        public override string ToString() {
            return string.Join(", ", this.coordinates);
        }
    }
}
