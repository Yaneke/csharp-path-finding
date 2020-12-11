using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using graph;


namespace data_structures {
    [TestClass]
    public class HeapTests {
        [TestMethod]
        public void TestIntOrdering() {
            Heap<int> heap = new Heap<int>();
            Random random = new Random();
            for (int i = 0; i < 1000; i++) {
                heap.Add(random.Next() % 200);
            }
            int previous = heap.Pop();
            while (heap.Count > 0) {
                int current = heap.Pop();
                Assert.IsTrue(current > previous);
                previous = current;
            }
        }

        [TestMethod]
        public void TestPathOrdering() {
            Heap<Path> heap = new Heap<Path>();
            GridGraph graph = new GridGraph("data/empty-8-8.map");
            Vertex source = graph.GetVertexAt(0, 0);
            Vertex destination = graph.GetVertexAt(0, 1);
            Path sourcePath = new Path(source);
            // Create a suboptimal path (0,0)->(1, 0)->(1, 1) -> (0, 1)
            Vertex v1 = graph.GetVertexAt(1, 0);
            Vertex v2 = graph.GetVertexAt(1, 1);
            Path p1 = new Path(sourcePath, source.GetEdgeTo(v1), graph.HCost(v1, destination));
            Path p2 = new Path(p1, v1.GetEdgeTo(v2), graph.HCost(v2, destination));
            Path p3 = new Path(p2, v2.GetEdgeTo(destination), graph.HCost(destination, destination));
            // Create a second path that is faster (0, 0) -> (1, 0)

            Path bestPath = new Path(sourcePath, source.GetEdgeTo(destination), graph.HCost(destination, destination));

            // Add the suboptimal path and then the optimal -> check that the optimal indeed replaces the suboptimal one
            heap.Add(p3);
            heap.Add(bestPath);
            Assert.Equals(heap.Count, 1);

            // Pop the item and check that the shortest path indeed pops out
            Path shortest = heap.Pop();
            Assert.Equals(shortest.vertex, source);
            Assert.Equals(shortest.edgePath, source.GetEdgeTo(destination));
        }
    }
}