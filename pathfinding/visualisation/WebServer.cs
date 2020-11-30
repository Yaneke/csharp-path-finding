using System;
using System.IO;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using graph;
using search;

namespace visualisation {
    class HttpServer {
        private HttpListener listener;
        private bool runServer;
        private GridGraph map;


        public HttpServer(string url) {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(url);
            this.listener.Start();
        }


        public void HandleIncomingConnections() {
            this.runServer = true;
            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (this.runServer) {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = listener.GetContext();
                this.dispatch(ctx.Request, ctx.Response);
                ctx.Response.OutputStream.Close();
            }
        }

        private void ServeFile(string path, HttpListenerResponse resp) {
            if (path == "/" || path == "") {
                path = "index.html";
            }
            byte[] data;
            try {
                data = File.ReadAllBytes("html/" + path);
            }
            catch (System.IO.FileNotFoundException) {
                data = File.ReadAllBytes("html/404.html");
            }
            if (path.EndsWith(".js")) {
                resp.ContentType = "text/javascript";
            } else {
                resp.ContentType = "text/html";
            }
            resp.ContentEncoding = Encoding.UTF8;
            resp.OutputStream.Write(data);


        }

        private void GetMaps(HttpListenerResponse resp) {
            DirectoryInfo d = new DirectoryInfo("data/");
            FileInfo[] files = d.GetFiles("*.map");
            List<string> fileNames = new List<string>();
            foreach (var f in files) {
                fileNames.Add(f.Name);
            }
            resp.ContentType = "text/json";
            resp.OutputStream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fileNames)));
        }


        private void GetSelectedMap(HttpListenerRequest req, HttpListenerResponse resp) {
            resp.ContentType = "text/plain";
            string fileName = "data/" + req.QueryString["map"];
            resp.OutputStream.Write(File.ReadAllBytes(fileName));
            this.map = new GridGraph(fileName);
        }

        private void GetPath(HttpListenerRequest req, HttpListenerResponse resp) {
            int xPathStart = int.Parse(req.QueryString["pathStart[x]"]);
            int yPathStart = int.Parse(req.QueryString["pathStart[y]"]);
            int xPathEnd = int.Parse(req.QueryString["pathEnd[x]"]);
            int yPathEnd = int.Parse(req.QueryString["pathEnd[y]"]);
            Vertex source = this.map.GetVertexAt(yPathStart, xPathStart);
            Vertex destination = this.map.GetVertexAt(yPathEnd, xPathEnd);
            graph.Path path = Astar.ShortestPath(this.map, source, destination);
            Dictionary<Vertex, Vertex> sourceDestinations = new Dictionary<Vertex, Vertex>();
            sourceDestinations.Add(source, destination);
            MAPF.CBS(this.map, sourceDestinations);
            resp.ContentType = "text/json";
            resp.OutputStream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(path.ToList())));
        }

        private void dispatch(HttpListenerRequest req, HttpListenerResponse resp) {
            switch (req.HttpMethod) {
                case "GET":
                    switch (req.Url.AbsolutePath) {
                        case "/maps":
                            this.GetMaps(resp);
                            break;
                        case "/getSelectedMap":
                            this.GetSelectedMap(req, resp);
                            break;
                        case "/getPath":
                            this.GetPath(req, resp);
                            break;
                        default:
                            this.ServeFile(req.Url.AbsolutePath, resp);
                            break;
                    }
                    break;
                case "POST":
                    switch (req.Url.AbsolutePath) {
                        case "/shutdown":
                            this.runServer = false;
                            this.ServeFile("/", resp);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("Unhandled method!");
                    break;
            }
        }
    }
}