using System.Collections.Generic;
using graph;
using search;

namespace visualisation.data_objects {
    class PathAnswerDO {
        public List<PathDO> paths { get; set; }
        public float cost { get; set; }
        public float duration { get; set; }

        public PathAnswerDO(Solution sol, long elapsedMilliseconds) {
            this.paths = new List<PathDO>();
            this.cost = sol.cost;
            this.duration = ((float)elapsedMilliseconds) / 1000;
            List<Path> paths = sol.GetPaths();
            foreach (var p in paths) {
                this.paths.Add(new PathDO(p));
            }
        }

    }
}