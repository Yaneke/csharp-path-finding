using visualisation;

namespace pathfinding {
    class Program {
        static void Main(string[] args) {
            RunServer();
        }

        static void RunServer() {
            var server = new HttpServer("http://localhost:8000/");
            server.HandleIncomingConnections();
        }
    }
}
