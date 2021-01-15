using System.Collections.Generic;
using PathPlanning.Example;
using System.IO;

namespace graph {
    public class Parser {
        public static PathPlanning.Example.Graph ParseFile(string filePath) {
            var name = System.IO.Path.GetFileName(filePath);
            var graph = new PathPlanning.Example.Graph(name);

            string[] lines = System.IO.File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++) {
                // Skip the four first lines that contain meta data
                if (i >= 4) {
                    string line = lines[i];
                    for (int j = 0; j < line.Length; j++) {
                        if (line[j] == '.') {
                            Coord vertex = new Coord(j, i - 4);
                            graph.AddVertex(vertex);
                            foreach (Coord neighbor in new List<Coord> { new Coord(j, i - 5), new Coord(j - 1, i - 4) }) {
                                try {
                                    graph.GetVertexAt(neighbor); // throws KeyNotFoundException if doesn't exist
                                    graph.AddEdge(vertex, neighbor);
                                }
                                catch (KeyNotFoundException) {}
                            }
                        } else {
                        }
                    }
                }
            }

            return graph;
        }
    }
}