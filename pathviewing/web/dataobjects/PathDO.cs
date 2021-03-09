using System.Collections.Generic;
using System;
using PathFinding.Graphs;

namespace PathViewing.Web.DataObjects {
    public class PathDO {
        public List<CoordinateDO> coordinates { get; set; }

        public PathDO(Path path) {
            this.coordinates = new List<CoordinateDO>();
            if (path != null) {
                foreach (var vertex in path.vertexPath) {
                    if (vertex is GridVertex) {
                        var gridVertex = (GridVertex)vertex;
                        this.coordinates.Add(new CoordinateDO(gridVertex.j, gridVertex.i));
                    } else {
                        Console.WriteLine("Not a grid vertex!");
                        throw new Exception("Not yet implemented !");
                    }
                }
            }
        }

        public override string ToString() {
            return string.Join(", ", this.coordinates);
        }
    }
}
