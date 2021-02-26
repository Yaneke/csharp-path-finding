using Microsoft.VisualStudio.TestTools.UnitTesting;
using graph;

namespace tests {
    [TestClass]
    public class CardinalTest {


        [TestMethod]
        public void ComputeDirecionTests() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true, 0);
            Vertex center = g.GetVertexAt(5, 5);
            Vertex west = g.GetVertexAt(5, 4);
            Vertex east = g.GetVertexAt(5, 6);
            Vertex north = g.GetVertexAt(4, 5);
            Vertex south = g.GetVertexAt(6, 5);
            Assert.AreEqual(CardinalDirection.West, center.GetEdgeTo(west).ComputeDirection());
            Assert.AreEqual(CardinalDirection.East, center.GetEdgeTo(east).ComputeDirection());
            Assert.AreEqual(CardinalDirection.North, center.GetEdgeTo(north).ComputeDirection());
            Assert.AreEqual(CardinalDirection.South, center.GetEdgeTo(south).ComputeDirection());
            Assert.AreEqual(CardinalDirection.None, center.GetEdgeTo(center).ComputeDirection());
        }

    }
}