using Microsoft.VisualStudio.TestTools.UnitTesting;
using graph;
using search;

namespace tests {
    [TestClass]
    public class SolutionTest {
        [TestMethod]
        public void VertexConflicts() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            Path path1 = Astar.ShortestPath(g, g.GetVertexAt(0, 1), g.GetVertexAt(5, 1));
            Path path2 = Astar.ShortestPath(g, g.GetVertexAt(1, 0), g.GetVertexAt(1, 5));
            Solution s = new Solution();
            s.Add(path1);
            s.Add(path2);
            Conflict c = s.GetFirstConflict();
            Assert.IsTrue(c is VertexConflict);
        }
    }
}