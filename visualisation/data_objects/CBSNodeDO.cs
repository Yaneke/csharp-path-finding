using search;
using search.cbs;
using System.Collections.Generic;


namespace visualisation.data_objects {
    class CBSNodeDO {
        public Dictionary<int, HashSet<Constraint>> constraints { get; set; }
        public PathAnswerDO pathAnswer { get; set; }
        public Conflict conflict { get; set; }

        public CBSNodeDO(HashSet<ConflictChecker> checkers, CBSNode node, long elapsedMilliseconds) {
            this.pathAnswer = new PathAnswerDO(node.solution, elapsedMilliseconds);
            this.constraints = new Dictionary<int, HashSet<Constraint>>();
            this.conflict = node.solution.GetFirstConflict(checkers);

            int nagents = this.pathAnswer.paths.Count;
            for (int i = 0; i < nagents; i++) {
                this.constraints.Add(i, node.constraints.GetConstraints(i));
            }
        }
    }
}