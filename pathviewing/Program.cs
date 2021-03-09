using System;
using System.Collections.Generic;
using PathViewing.Web;
using PathFinding.Benchmarks;
using BenchmarkDotNet.Running;
using PathFinding.Graphs;
using PathFinding.Search;
using PathFinding.Search.CBS;

namespace PathViewing {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0 || args[0] == "runserver") {
                RunServer();
            } else if (args[0] == "tmp") {
                GridGraph g = new GridGraph("data/Boston_0_256.map");
                List<Vertex> sources = new List<Vertex> { g.GetVertexAt(49, 182), g.GetVertexAt(41, 165) };
                List<Vertex> destinations = new List<Vertex> { g.GetVertexAt(30, 218), g.GetVertexAt(25, 204) };
                Solution sol = new CBS().WithCardinalConflicts().ShortestPath(g, sources, destinations);
            } else if (args[0] == "benchmark") {
                RunBenchmarks(args);
            } else {
                Console.WriteLine("Unknown command! Use \"runserver\" or \"benchmark\"");
            }
        }

        static void RunServer() {
            var server = new HttpServer("http://localhost:8000/");
            server.HandleIncomingConnections();
        }

        static void RunBenchmarks(string[] args) {
            List<BenchmarkDotNet.Reports.Summary> results = new List<BenchmarkDotNet.Reports.Summary>();
            if (args.Length == 1 || args[1] == "all") {
                results.Add(BenchmarkRunner.Run<FastPriorityQueueBenchmarks>());
                results.Add(BenchmarkRunner.Run<HeapBenchmarks>());
                results.Add(BenchmarkRunner.Run<SimplePriorityQueueBenchmarks>());
                results.Add(BenchmarkRunner.Run<AstarBenchmarks>());
            } else {
                switch (args[1].ToLower()) {
                    case "astar":
                        results.Add(BenchmarkRunner.Run<AstarBenchmarks>());
                        break;
                    default:
                        Console.WriteLine("Unknown benchmark");
                        break;
                }
            }
        }
    }
}
