using graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace tests {
    [TestClass]
    public class VertexTest {
        [TestMethod]
        public void HashCodeTest() {
            GridVertex v1 = new GridVertex(5, 5);
            GridVertex v2 = new GridVertex(5, 5);
            Vertex v3 = new Vertex(v1.id);
            Vertex v4 = new Vertex(v2.id);
            Assert.IsTrue(v1.GetHashCode() == v2.GetHashCode());
            Assert.IsTrue(v3.GetHashCode() == v4.GetHashCode());
        }

        [TestMethod]
        public void DictionaryTest() {
            // Gridvertex
            GridVertex v1 = new GridVertex(5, 5);
            GridVertex v2 = new GridVertex(5, 5);
            Dictionary<GridVertex, int> d = new Dictionary<GridVertex, int>();
            d.Add(v1, 0);
            Assert.IsTrue(d.ContainsKey(v2));

            // Vertex
            Vertex v3 = new Vertex(v1.id);
            Vertex v4 = new Vertex(v2.id);
            Dictionary<Vertex, int> d2 = new Dictionary<Vertex, int>();
            d2.Add(v3, 0);
            Assert.IsTrue(d2.ContainsKey(v4));
        }
    }
}
