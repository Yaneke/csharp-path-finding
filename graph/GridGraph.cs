using System.Collections.Generic;
using System;

namespace graph {
    public class GridGraph : Graph {
        private List<List<GridVertex>> vertexGrid;
        private Dictionary<GridVertex, Tuple<int, int>> vertexPosition;
        public int ColCount { get; set; }
        public int RowCount { get; set; }

        public GridGraph(string fileName) : base() {
            this.vertexGrid = new List<List<GridVertex>>();
            vertexPosition = new Dictionary<GridVertex, Tuple<int, int>>();

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
                    this.vertexGrid.Add(rowVertices);
                }
            }

            for (int i = 0; i < this.RowCount; i++) {
                for (int j = 0; j < this.vertexGrid[i].Count; j++) {
                    Vertex v = this.vertexGrid[i][j];
                    if (v != null) {
                        this.Add(v, i, j);
                        if (i > 0) {// north
                            v.AddNeighbour(this.vertexGrid[i - 1][j], 1);
                        }
                        if (j > 0) { // west
                            v.AddNeighbour(this.vertexGrid[i][j - 1], 1);
                        }
                        if (i < this.RowCount - 1) { // south 
                            v.AddNeighbour(this.vertexGrid[i + 1][j], 1);
                        }
                        if (j < this.ColCount - 1) { // east
                            v.AddNeighbour(this.vertexGrid[i][j + 1], 1);
                        }
                    }
                }
            }
        }

        public Vertex GetVertexAt(int i, int j) {
            if (i > 0 && i < this.RowCount && j > 0 && j < this.ColCount) {
                return this.vertexGrid[i][j];
            }
            return null;
        }

        public Tuple<int, int> GetVertexPos(Vertex v) {
            if (v is GridVertex) {
                GridVertex gridVertex = (GridVertex)v;
                if (vertexPosition.ContainsKey(gridVertex)) {
                    return this.vertexPosition[gridVertex];
                }
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
            this.vertexPosition.Add((GridVertex)v, new Tuple<int, int>(i, j));
        }


        public override string ToString() {
            string res = "Line count:" + this.ColCount + "\n";
            res += "Row Count: " + this.RowCount + "\n";
            for (int i = 0; i < this.ColCount; i++) {
                for (int j = 0; j < this.RowCount; j++) {
                    if (this.vertexGrid[i][j] == null) {
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
