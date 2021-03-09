using System;
using BenchmarkDotNet.Attributes;
using PathFinding.Search;
using PathFinding.Graphs;

namespace PathFinding.Benchmarks {
    public class AstarBenchmarks {
        [Params("maze-128-128-10.map", "Berlin_1_256.map", "empty-48-48.map")]
        public string mapFile;
        private const int N = 50;

        private GridGraph graph;
        private Vertex[] randomVertices;

        [GlobalSetup]
        public void GlobalSetup() {
            graph = new GridGraph("../../../../../../../data/" + mapFile);
            randomVertices = new Vertex[N];
            Random rand = new Random(34829061);
            for (int n = 0; n < N; n++) {
                Vertex v = null;
                while (v == null) {
                    int x = rand.Next() % graph.ColCount;
                    int y = rand.Next() % graph.RowCount;
                    v = graph.GetVertexAt(y, x);
                }
                randomVertices[n] = v;
            }
        }

        [Benchmark]
        public void ComputePaths() {
            for (int i = 0; i < N - 1; i++) {
                Astar.ShortestPath(graph, randomVertices[i], randomVertices[i + 1]);
            }
        }

        [Benchmark]
        public void ComputePathsFast() {
            for (int i = 0; i < N - 1; i++) {
                Astar.FastShortestPath(graph, randomVertices[i], randomVertices[i + 1]);
            }
        }
    }
}