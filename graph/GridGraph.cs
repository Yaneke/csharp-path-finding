using System.Collections.Generic;
using System;

namespace graph {
    public class GridGraph : Graph {
        private List<List<Vertex>> vertexGrid;
        private Dictionary<Vertex, Tuple<int, int>> vertexPosition;
        public int ColCount { get; set; }
        public int RowCount { get; set; }

        public GridGraph(string fileName) : base() {
            this.vertexGrid = new List<List<Vertex>>();
            vertexPosition = new Dictionary<Vertex, Tuple<int, int>>();

            string[] lines = System.IO.File.ReadAllLines(fileName);
            this.RowCount = int.Parse(lines[1].Split(" ")[1]);
            this.ColCount = int.Parse(lines[2].Split(" ")[1]);
            for (int i = 0; i < lines.Length; i++) {
                // Skip the four first lines that contain meta data
                if (i >= 4) {
                    List<Vertex> rowVertices = new List<Vertex>();
                    string line = lines[i];
                    for (int j = 0; j < line.Length; j++) {
                        if (line[j] == '.') {
                            rowVertices.Add(new Vertex((i - 4) + ", " + j));
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
            if (vertexPosition.ContainsKey(v)) {
                return this.vertexPosition[v];
            }
            return null;
        }

        /// Compute the heuristic cost from n1 to n2 (manhattan distance).
        public override float HCost(Vertex source, Vertex destination) {
            var ijSource = this.vertexPosition[source];
            var ijDestination = this.vertexPosition[destination];
            return Math.Abs(ijDestination.Item1 - ijSource.Item1) + Math.Abs(ijDestination.Item2 - ijSource.Item2);
        }

        public void Add(Vertex v, int i, int j) {
            base.Add(v);
            this.vertexPosition.Add(v, new Tuple<int, int>(i, j));
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
