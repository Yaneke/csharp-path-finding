using visualisation;

namespace pathfinding {
    class Program {
        static void Main(string[] args) {
            // var g = new GridGraph("data/maze-32-32-4.map");
            // var v1 = g.GetVertexAt(1, 1);
            // var v2 = g.GetVertexAt(3, 3);
            // Console.WriteLine(v1);
            // Console.WriteLine(v2);
            // var path = Astar.ShortestPath(g, v1, v2);
            // Console.WriteLine(path);
            var server = new HttpServer("http://localhost:8000/");

            // Handle requests

            server.HandleIncomingConnections();

        }
    }
}
