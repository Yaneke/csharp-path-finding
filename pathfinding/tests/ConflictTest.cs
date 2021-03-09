using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathFinding.Graphs;
using PathFinding.Search;
using PathFinding.Search.CBS;
using System.Collections.Generic;

namespace PathFinding.Tests {
    [TestClass]
    public class SolutionTest {

        private HashSet<ConflictChecker> GetDefaultCheckers() {
            HashSet<ConflictChecker> checkers = new HashSet<ConflictChecker>();
            checkers.Add(new FollowingConflictChecker());
            checkers.Add(new VertexConflictChecker());
            return checkers;
        }

        [TestMethod]
        public void VertexConflicts() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            Path path1 = Astar.ShortestPath(g, g.GetVertexAt(0, 1), g.GetVertexAt(5, 1));
            Path path2 = Astar.ShortestPath(g, g.GetVertexAt(1, 0), g.GetVertexAt(1, 5));
            Solution s = new Solution();
            s.Add(path1);
            s.Add(path2);
            Conflict c = s.GetFirstConflict(GetDefaultCheckers());
            Assert.IsTrue(c is VertexConflict);
            Assert.AreEqual(1, c.timestep);
        }


        [TestMethod]
        public void EdgeConflicts() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            Path path1 = Astar.ShortestPath(g, g.GetVertexAt(0, 0), g.GetVertexAt(5, 5));
            Path path2 = Astar.ShortestPath(g, g.GetVertexAt(0, 0), g.GetVertexAt(5, 5));
            Solution s = new Solution();
            s.Add(path1);
            s.Add(path2);
            HashSet<ConflictChecker> checkers = new HashSet<ConflictChecker>();
            checkers.Add(new EdgeConflictChecker());
            Conflict c = s.GetFirstConflict(checkers);
            Assert.IsTrue(c is EdgeConflict);
            Assert.AreEqual(0, c.timestep);
        }

        [TestMethod]
        public void CardinalConflicts() {
            GridGraph g = new GridGraph("../../../data/empty-16-16.map");
            Path path1 = Astar.ShortestPath(g, g.GetVertexAt(0, 0), g.GetVertexAt(0, 5));
            Path path2 = Astar.ShortestPath(g, g.GetVertexAt(2, 0), g.GetVertexAt(2, 3));
            Solution s = new Solution();
            s.Add(path1);
            s.Add(path2);
            HashSet<ConflictChecker> checkers = new HashSet<ConflictChecker>();
            checkers.Add(new CardinalConflictChecker());
            Conflict c = s.GetFirstConflict(checkers);
            Assert.IsTrue(c is CardinalConflict);
            Assert.AreEqual(1, c.timestep);
        }
    }
}