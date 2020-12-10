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
            catch (DirectoryNotFoundException) {
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
            this.map = new GridGraph(fileName);
        }

        private string ReadPostData(HttpListenerRequest req) {
            using (StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding)) {
                return reader.ReadToEnd();
            }
        }

        private void ComputePath(HttpListenerRequest req, HttpListenerResponse resp) {
            string data = ReadPostData(req);
            data_objects.PathRequest pathRequest = JsonSerializer.Deserialize<data_objects.PathRequest>(data);
            Vertex source = this.map.GetVertexAt(pathRequest.start.y, pathRequest.start.x);
            Vertex destination = this.map.GetVertexAt(pathRequest.end.y, pathRequest.end.x);
            graph.Path path = Astar.ShortestPath(this.map, source, destination);
            data_objects.Path pathDTO = new data_objects.Path(path);
            resp.ContentType = "text/plain";
            byte[] responseData = JsonSerializer.SerializeToUtf8Bytes(pathDTO);
            resp.ContentLength64 = responseData.LongLength;
            resp.ContentEncoding = Encoding.UTF8;
            resp.OutputStream.Write(responseData);
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
                        case "/shutdown":
                            this.runServer = false;
                            this.ServeFile("/", resp);
                            break;
                        case "/computePath":
                            this.ComputePath(req, resp);
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