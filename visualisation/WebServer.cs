using System;
using System.IO;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using graph;
//using PathPlanning.Example;
using search;
using search.cbs;
//using PathPlanning.Common;

namespace visualisation {
    class HttpServer {
        private HttpListener listener;
        private bool runServer;
        private GridGraph map;
        //private PathPlanning.Example.Graph map;


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
                HttpListenerContext ctx = this.listener.GetContext();
                string log = "[" + ctx.Request.HttpMethod + "] " + ctx.Request.Url.AbsolutePath;
                Console.WriteLine(log);
                try {
                    this.dispatch(ctx.Request, ctx.Response);
                    ctx.Response.OutputStream.Close();
                    log = "\t-> " + ctx.Response.StatusCode + " " + ctx.Response.ContentType + " (" + ctx.Response.ContentLength64 + " bytes)";
                    Console.WriteLine(log);
                }
                catch (System.Net.HttpListenerException) {
                    Console.WriteLine("\t-> Error: Broken pipe...");
                }
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
            } else if (path.EndsWith(".css")) {
                resp.ContentType = "text/css";
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
            //this.map = Parser.ParseFile(fileName);
            this.map = new GridGraph(fileName, true);
        }

        private string ReadPostData(HttpListenerRequest req) {
            using (StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding)) {
                return reader.ReadToEnd();
            }
        }

        private void GetPath(HttpListenerRequest req, HttpListenerResponse resp) {
            try {
                string data = ReadPostData(req);
                Console.WriteLine(data);
                data_objects.PathRequestDO pathRequests = JsonSerializer.Deserialize<data_objects.PathRequestDO>(data);
                Solution sol = CBS.ShortestPath(this.map, pathRequests.GetSources(this.map), pathRequests.GetDestinations(this.map));
                var res = new data_objects.PathAnswerDO(sol);
                byte[] responseData = JsonSerializer.SerializeToUtf8Bytes(res);
                resp.ContentType = "text/json";
                resp.ContentLength64 = responseData.LongLength;
                resp.ContentEncoding = Encoding.UTF8;
                resp.OutputStream.Write(responseData);
            }
            catch (System.Text.Json.JsonException) {
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            /*
            int xPathStart = int.Parse(req.QueryString["pathStart[x]"]);
            int yPathStart = int.Parse(req.QueryString["pathStart[y]"]);
            int xPathEnd = int.Parse(req.QueryString["pathEnd[x]"]);
            int yPathEnd = int.Parse(req.QueryString["pathEnd[y]"]);
            map.AddPawn("P", new Coord(xPathStart, yPathStart), new Coord(xPathEnd, yPathEnd));

            var tree = new PathPlanning.Search.CBSTree(map);
            var solution = tree.Search().First();
            var path = solution.GetPaths("P").First();
            var pathString = path.Select((IVertex vertex, int _) =>
            {
                var coord = vertex.GetCoordinates();
                return coord.Y + ", " + coord.X; // TODO maybe use usual order (X, Y)
            });

            resp.ContentType = "text/json";
            resp.OutputStream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pathString.ToList())));
            */
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
                        default:
                            this.ServeFile(req.Url.AbsolutePath, resp);
                            break;
                    }
                    break;
                case "POST":
                    switch (req.Url.AbsolutePath) {
                        case "/getPath":
                            this.GetPath(req, resp);
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