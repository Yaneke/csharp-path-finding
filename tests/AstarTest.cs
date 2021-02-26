using graph;
using search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;


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
            Path p = Astar.ShortestPath(g, v1, v2);
            Assert.IsNotNull(p);
        }


        [TestMethod]
        public void NewShouldWork() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            GridVertex v1 = g.GetVertexAt(0, 0);
            GridVertex v2 = g.GetVertexAt(5, 5);
            Path p = Astar.NewShortestPath(g, v1, v2, new System.Collections.Generic.HashSet<Constraint>());
            Assert.IsNotNull(p);
            Assert.AreEqual(10, p.cost);
        }

        [TestMethod]
        public void NewWithConstraints() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true, 1);
            GridVertex v1 = g.GetVertexAt(0, 0);
            GridVertex v2 = g.GetVertexAt(5, 5);
            Constraint c1 = new Constraint(g.GetVertexAt(0, 1), 1);
            Constraint c2 = new Constraint(g.GetVertexAt(1, 0), 1);
            HashSet<Constraint> constraints = new HashSet<Constraint>();
            constraints.Add(c1);
            constraints.Add(c2);
            Path p = Astar.NewShortestPath(g, v1, v2, constraints);
            Console.WriteLine(p);
            Assert.IsNotNull(p);
            Assert.AreEqual(11, p.cost);
        }

        [TestMethod]
        public void NewImpossible_path() {
            GridGraph g = new GridGraph("../../../data/full-16-16.map");
            GridVertex v1 = new GridVertex(0, 0);
            GridVertex v2 = new GridVertex(5, 5);
            g.Add(v1, 0, 0);
            g.Add(v2, 5, 5);
            Assert.IsNull(Astar.NewShortestPath(g, v1, v2, new System.Collections.Generic.HashSet<Constraint>()));
        }
    }
}
