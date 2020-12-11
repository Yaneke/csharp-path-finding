
namespace search.cbs {
    public class ConstraintTree {
        private CBSNode root;

        public ConstraintTree() {
            this.root = new CBSNode();
        }

        private class CBSNode {
            private ConstraintSet constraints;
            private Solution solution;
            public CBSNode leftChild { get; }
            public CBSNode rightChild { get; }

            public bool IsGoal() {
                return this.solution.IsValid();
            }
        }
    }
}