using System;
using System.Collections.Generic;
using visualisation;
using Benchmarks;
using BenchmarkDotNet.Running;

namespace pathfinding {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0 || args[0] == "runserver") {
                RunServer();
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
