using System.Collections.Generic;
using PathFinding.Graphs;
using PathFinding.Search;

namespace PathViewing.Web.DataObjects {
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