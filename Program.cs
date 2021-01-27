using System;
using visualisation;
using Benchmarks;
using BenchmarkDotNet.Running;

namespace pathfinding {
    class Program {
        static void Main(string[] args) {
            if (args.Length == 0 || args[0] == "runserver") {
                RunServer();
            } else if (args[0] == "benchmark") {
                var res1 = BenchmarkRunner.Run<PriorityQueueBenchmarks>();
                var res2 = BenchmarkRunner.Run<HeapBenchmarks>();
                Console.WriteLine(res1);
                Console.WriteLine(res2);

            } else {
                Console.WriteLine("Unknown command! Use \"runserver\" or \"benchmark\"");
            }
        }

        static void RunServer() {
            var server = new HttpServer("http://localhost:8000/");
            server.HandleIncomingConnections();
        }
    }
}
