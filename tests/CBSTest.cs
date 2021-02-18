using graph;
using search.cbs;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using search;


namespace tests {
    [TestClass]
    public class CBSTest {
        [TestMethod]
        public void SimplePath() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(0, 0));
            destinations.Add(g.GetVertexAt(5, 5));
            Solution s = CBS.Default().ShortestPath(g, sources, destinations);
            Assert.AreEqual(10, s.cost);
        }


        [TestMethod]
        public void ManyCardinalConflicts() {
            // In this situation, both agents have to go up all the time, hence many CardinalConflicts.
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true, 0);
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(0, 0));
            sources.Add(g.GetVertexAt(1, 0));
            destinations.Add(g.GetVertexAt(0, 5));
            destinations.Add(g.GetVertexAt(1, 5));
            Solution s = new CBS().WithCardinalConflicts().ShortestPath(g, sources, destinations);
            int totalLength = 0;
            foreach (Path p in s.GetPaths()) {
                totalLength += p.edgePath.Count;
            }
            Assert.AreEqual(10, s.cost);
            // Expected total length is 10: 5 time steps for the first and 10 for the second agent
            Assert.AreEqual(15, totalLength);
        }





        [TestMethod]
        public void CrossingPaths_2Agents() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true, 0);
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(5, 5));
            sources.Add(g.GetVertexAt(6, 6));
            destinations.Add(g.GetVertexAt(7, 5));
            destinations.Add(g.GetVertexAt(6, 4));
            Solution s = CBS.Default().ShortestPath(g, sources, destinations);
            Console.WriteLine(s.GetPath(0));
            Console.WriteLine(s.GetPath(1));
            Assert.IsNotNull(s);
            Assert.AreEqual(6, s.cost);
        }


        [TestMethod]
        public void CrossingPaths_4Agents() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true, 0);
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(5, 5));
            sources.Add(g.GetVertexAt(6, 6));
            sources.Add(g.GetVertexAt(7, 5));
            sources.Add(g.GetVertexAt(6, 4));
            destinations.Add(g.GetVertexAt(7, 5));
            destinations.Add(g.GetVertexAt(6, 4));
            destinations.Add(g.GetVertexAt(5, 5));
            destinations.Add(g.GetVertexAt(6, 6));
            Solution s = CBS.Default().ShortestPath(g, sources, destinations);
            Console.WriteLine(s.GetPath(0));
            Console.WriteLine(s.GetPath(1));
            Console.WriteLine(s.GetPath(2));
            Console.WriteLine(s.GetPath(3));
            Assert.IsNotNull(s);
            //Assert.AreEqual(16, s.cost);
        }

        /*
        [TestMethod]
        public void EdgeConflict() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true);
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(5, 5));
            sources.Add(g.GetVertexAt(5, 6));
            destinations.Add(g.GetVertexAt(5, 6));
            destinations.Add(g.GetVertexAt(5, 5));
            Solution s = CBS.ShortestPath(g, sources, destinations);
            Console.WriteLine(s.GetPath(0));
            Console.WriteLine(s.GetPath(1));
            Assert.IsNotNull(s);
            Assert.AreEqual(4, s.cost);
        }
        */


        [TestMethod]
        public void FollowingConflict() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true, 0);
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(5, 5));
            sources.Add(g.GetVertexAt(5, 6));
            destinations.Add(g.GetVertexAt(5, 6));
            destinations.Add(g.GetVertexAt(6, 6));
            Solution s = CBS.Default().ShortestPath(g, sources, destinations);
            Console.WriteLine(s.GetPath(0));
            Console.WriteLine(s.GetPath(1));
            Assert.IsNotNull(s);
            // Optimal path (cost = 2 with 0-cost stay):
            // agent 0: (5, 5), (5, 5), (5, 6)
            // agent 1: (5, 6), (6, 6)
            Assert.AreEqual(2, s.cost);
            Assert.AreEqual(3, s.GetPath(0).edgePath.Count + s.GetPath(1).edgePath.Count);
        }


        [TestMethod]
        public void CardinalConflicts_BigMap() {
            GridGraph g = new GridGraph("../../../data/Boston_0_256.map");
            // Path 1 (153,23) -> (153,33)
            // Path 2 (162,23) -> (162,35)
            List<Vertex> sources = new List<Vertex> { g.GetVertexAt(23, 153), g.GetVertexAt(23, 162) };
            List<Vertex> destinations = new List<Vertex> { g.GetVertexAt(33, 153), g.GetVertexAt(35, 162) };
            Solution sol = new CBS().WithCardinalConflicts().ShortestPath(g, sources, destinations);
            Assert.IsNotNull(sol);
        }

        [TestMethod]
        public void CardinalConflicts_BigMap2() {
            GridGraph g = new GridGraph("../../../data/Boston_0_256.map");
            List<Vertex> sources = new List<Vertex> { g.GetVertexAt(49, 182), g.GetVertexAt(41, 165) };
            List<Vertex> destinations = new List<Vertex> { g.GetVertexAt(30, 218), g.GetVertexAt(25, 204) };
            Solution sol = new CBS().WithCardinalConflicts().ShortestPath(g, sources, destinations);
            Assert.IsNotNull(sol);
        }

        public void PathWithConstraintThatMakesAstarReturnNull() {
            // TODO
        }
    }
}
