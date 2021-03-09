using System.Collections.Generic;
using PathFinding.Graphs;
using System;
using PathFinding.Search.CBS;


namespace PathFinding.Search {
    public class Solution {
        private List<Path> agentsPaths;
        public float cost {
            get {
                float res = 0;
                foreach (var path in this.agentsPaths) {
                    if (path != null) {
                        res += path.cost;
                    }
                }
                return res;
            }
        }
        public int AgentCount {
            get => this.agentsPaths.Count;
        }

        public Solution Clone() {
            Solution s = new Solution();
            foreach (var path in this.agentsPaths) {
                s.Add(path);
            }
            return s;
        }

        public Solution() {
            this.agentsPaths = new List<Path>();
        }

        public void Add(Path path) {
            this.agentsPaths.Add(path);
        }

        public void ReplacePath(int agent, Path newPath) {
            if (agent < this.agentsPaths.Count) {
                this.agentsPaths[agent] = newPath;
            } else {
                throw new Exception("Could not replace the Path of agent " + agent + " bacause there is no path for him.");
            }

        }

        public Path GetPath(int agent) {
            return this.agentsPaths[agent];
        }

        public List<Path> GetPaths() {
            return this.agentsPaths;
        }

        public bool IsValid(HashSet<ConflictChecker> checkers) {
            return this.GetFirstConflict(checkers) == null;
        }

        /// <summary>
        /// Find the first conflict in a solution if any. Returns null otherwise.
        /// </summary>
        /// NB: This is implemented in a rather non-intuitive manner in order to spot 
        /// conflicts as soon as possible.
        public Conflict GetFirstConflict(HashSet<ConflictChecker> checkers) {
            // Check max path length
            int nsteps = 0;
            foreach (var path in this.agentsPaths) {
                if (path != null && path.edgePath.Count > nsteps) {
                    nsteps = path.edgePath.Count;
                }
            }
            // For each time step.
            for (int t = 0; t < nsteps; t++) {
                foreach (var checker in checkers) {
                    Conflict c = checker.Check(this, t);
                    if (c != null) {
                        return c;
                    }
                }
            }
            return null;
        }

        public override string ToString() {
            String res = "Solution: cost=" + this.cost;
            String sep = " ";
            for (int agent = 0; agent < this.agentsPaths.Count; agent++) {
                Path path = this.agentsPaths[agent];
                res += sep + "agent" + agent + ": " + path.cost;
                sep = ", ";
            }

            return res;
        }
    }
}