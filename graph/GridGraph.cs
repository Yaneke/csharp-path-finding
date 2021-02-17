using System.Collections.Generic;
using System;

namespace graph {
    public class GridGraph : Graph {
        private List<List<GridVertex>> grid;
        public int ColCount { get; set; }
        public int RowCount { get; set; }

        public GridGraph(string fileName) : this(fileName, false, 0) { }

        public GridGraph(string fileName, bool withEdgeLoop, int loopCost) : base() {
            this.grid = new List<List<GridVertex>>();
            string[] lines = System.IO.File.ReadAllLines(fileName);
            this.RowCount = int.Parse(lines[1].Split(" ")[1]);
            this.ColCount = int.Parse(lines[2].Split(" ")[1]);
            for (int i = 0; i < lines.Length; i++) {
                // Skip the four first lines that contain meta data
                if (i >= 4) {
                    List<GridVertex> rowVertices = new List<GridVertex>();
                    string line = lines[i];
                    for (int j = 0; j < line.Length; j++) {
                        if (line[j] == '.') {
                            rowVertices.Add(new GridVertex(i - 4, j));
                        } else {
                            rowVertices.Add(null);
                        }
                    }
                    this.grid.Add(rowVertices);
                }
            }

            for (int i = 0; i < this.RowCount; i++) {
                for (int j = 0; j < this.grid[i].Count; j++) {
                    Vertex v = this.grid[i][j];
                    if (v != null) {
                        this.Add(v, i, j);
                        if (withEdgeLoop) {
                            v.AddNeighbour(v, loopCost);
                        }
                        if (i > 0) {// north
                            v.AddNeighbour(this.grid[i - 1][j], 1);
                        }
                        if (j > 0) { // west
                            v.AddNeighbour(this.grid[i][j - 1], 1);
                        }
                        if (i < this.RowCount - 1) { // south 
                            v.AddNeighbour(this.grid[i + 1][j], 1);
                        }
                        if (j < this.ColCount - 1) { // east
                            v.AddNeighbour(this.grid[i][j + 1], 1);
                        }
                    }
                }
            }
        }

        public GridVertex GetVertexAt(int i, int j) {
            if (i >= 0 && i < this.RowCount && j >= 0 && j < this.ColCount) {
                return this.grid[i][j];
            }
            return null;
        }

        public Tuple<int, int> GetVertexPos(Vertex v) {
            if (v is GridVertex && this.Contains(v)) {
                GridVertex gridVertex = (GridVertex)v;
                return new Tuple<int, int>(gridVertex.i, gridVertex.j);
            }
            return null;
        }

        /// Compute the heuristic cost from n1 to n2 (manhattan distance).
        public override float HCost(Vertex source, Vertex destination) {
            if (source is GridVertex && destination is GridVertex) {
                return search.Heuristics.ManhattanDistance((GridVertex)source, (GridVertex)destination);
            }
            return 0;
        }

        public void Add(Vertex v, int i, int j) {
            if (!(v is (GridVertex))) {
                throw new Exception("Should be a GridVertex in a GridGraph");
            }
            base.Add(v);
        }


        public override string ToString() {
            string res = "Line count:" + this.ColCount + "\n";
            res += "Row Count: " + this.RowCount + "\n";
            for (int i = 0; i < this.ColCount; i++) {
                for (int j = 0; j < this.RowCount; j++) {
                    if (this.grid[i][j] == null) {
                        res += "@";
                    } else {
                        res += ".";
                    }
                }
                res += "\n";
            }
            return res;
        }
    }
}
