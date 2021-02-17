using graph;
using search;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace tests {
    [TestClass]
    public class AstarTest {
        [TestMethod]
        public void impossible_path() {
            GridGraph g = new GridGraph("../../../data/full-16-16.map");
            GridVertex v1 = new GridVertex(0, 0);
            GridVertex v2 = new GridVertex(5, 5);
            g.Add(v1, 0, 0);
            g.Add(v2, 5, 5);
            Assert.IsNull(Astar.ShortestPath(g, v1, v2));
        }


        [TestMethod]
        public void ShouldWork() {
            GridGraph g = new GridGraph("../../../data/Boston_0_256.map");
            GridVertex v1 = g.GetVertexAt(35, 173);
            GridVertex v2 = g.GetVertexAt(27, 204);
            Assert.IsNotNull(Astar.ShortestPath(g, v1, v2));
        }
    }
}
