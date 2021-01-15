using graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace search {
    [TestClass]
    public class GridGraphTest {
        [TestMethod]
        public void ImpossibleVertices() {
            GridGraph g = new GridGraph("../../../data/full-16-16.map");
            Assert.AreEqual(16, g.ColCount);
            Assert.AreEqual(16, g.RowCount);

            Assert.IsNull(g.GetVertexAt(0, 0));
            Assert.IsNull(g.GetVertexAt(100, 10000));
            Assert.IsNull(g.GetVertexAt(0, -50));
            Assert.IsNull(g.GetVertexAt(42, -42));
        }


        [TestMethod]
        public void CheckExistingVertices() {
            GridGraph g = new GridGraph("../../../data/Boston_0_256.map");
            Assert.AreEqual(256, g.ColCount);
            Assert.AreEqual(256, g.RowCount);


            Assert.IsNotNull(g.GetVertexAt(0, 0));
            Assert.IsNotNull(g.GetVertexAt(64, 219));
            Assert.IsNotNull(g.GetVertexAt(19, 38));
            Assert.IsNotNull(g.GetVertexAt(82, 125));

            Assert.IsNull(g.GetVertexAt(3, 115));
        }

        [TestMethod]
        public void CheckNeighbours() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            Assert.AreEqual(16, g.ColCount);
            Assert.AreEqual(16, g.RowCount);

            Assert.AreEqual(2, g.GetVertexAt(0, 0).GetEdges().Count);
            Assert.AreEqual(3, g.GetVertexAt(0, 7).GetEdges().Count);
            Assert.AreEqual(4, g.GetVertexAt(4, 2).GetEdges().Count);

            GridGraph g2 = new GridGraph("../../../data/Boston_0_256.map", false);
            Assert.AreEqual(2, g2.GetVertexAt(0, 20).GetEdges().Count);
        }

    }
}
