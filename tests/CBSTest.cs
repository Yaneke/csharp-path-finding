using graph;
using search.cbs;
using System.Collections.Generic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace search {
    [TestClass]
    public class CBSTest {
        [TestMethod]
        public void SimplePath() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(0, 0));
            destinations.Add(g.GetVertexAt(5, 5));
            Solution s = CBS.ShortestPath(g, sources, destinations);
            Assert.AreEqual(10, s.cost);
        }

        [TestMethod]
        public void CrossingPaths_2Agents() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true);
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(5, 5));
            sources.Add(g.GetVertexAt(6, 6));
            destinations.Add(g.GetVertexAt(7, 5));
            destinations.Add(g.GetVertexAt(6, 4));
            Solution s = CBS.ShortestPath(g, sources, destinations);
            Console.WriteLine(s.GetPath(0));
            Console.WriteLine(s.GetPath(1));
            Assert.IsNotNull(s);
            Assert.AreEqual(6, s.cost);
        }


        [TestMethod]
        public void CrossingPaths_4Agents() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true);
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
            Solution s = CBS.ShortestPath(g, sources, destinations);
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
            GridGraph g = new GridGraph("../../../data/empty-16-16.map", true);
            List<Vertex> sources = new List<Vertex>();
            List<Vertex> destinations = new List<Vertex>();
            sources.Add(g.GetVertexAt(5, 5));
            sources.Add(g.GetVertexAt(5, 6));
            destinations.Add(g.GetVertexAt(5, 6));
            destinations.Add(g.GetVertexAt(6, 6));
            Solution s = CBS.ShortestPath(g, sources, destinations);
            Console.WriteLine(s.GetPath(0));
            Console.WriteLine(s.GetPath(1));
            Assert.IsNotNull(s);
            // Optimal path (cost = 2 with 0-cost stay):
            // agent 0: (5, 5), (5, 5), (5, 6)
            // agent 1: (5, 6), (6, 6 
            Assert.AreEqual(2, s.cost);
            Assert.AreEqual(3, s.GetPath(0).edgePath.Count + s.GetPath(1).edgePath.Count);
        }


        public void PathWithConstraintThatMakesAstarReturnNull() {
            // TODO
        }
    }
}
