using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using graph;
using search.cbs;

namespace visualisation {
    class HttpServer {
        private HttpListener listener;
        private GridGraph map;
        private CBS cbs;
        private IEnumerator<CBSNode> cbsEnumerator;
        private data_objects.PathRequestDO prevRequest;

        private static string HTML_DIR = "web/html/";

        public HttpServer(string url) {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(url);
            this.listener.Start();
        }


        public void HandleIncomingConnections() {
            while (true) {
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
                data = File.ReadAllBytes(HTML_DIR + path);
                if (path.EndsWith(".js")) {
                    resp.ContentType = "text/javascript";
                } else if (path.EndsWith(".css")) {
                    resp.ContentType = "text/css";
                } else {
                    resp.ContentType = "text/html";
                }
            }
            catch (System.IO.FileNotFoundException) {
                data = File.ReadAllBytes(HTML_DIR + "404.html");
                resp.ContentType = "text/html";
                resp.StatusCode = (int)HttpStatusCode.NotFound;
            }
            catch (System.IO.DirectoryNotFoundException) {
                data = File.ReadAllBytes(HTML_DIR + "404.html");
                resp.ContentType = "text/html";
                resp.StatusCode = (int)HttpStatusCode.NotFound;
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


        private void UpdateMapSettings(HttpListenerRequest req, HttpListenerResponse resp) {
            resp.ContentType = "text/plain";
            string data = this.ReadPostData(req);
            Console.WriteLine(data);
            data_objects.MapSettingsDO settings = JsonSerializer.Deserialize<data_objects.MapSettingsDO>(data);
            string fileName = "data/" + settings.map;
            resp.OutputStream.Write(File.ReadAllBytes(fileName));
            this.map = new GridGraph(fileName, settings.selfLoop, (int)settings.cost);
        }

        private string ReadPostData(HttpListenerRequest req) {
            using (StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding)) {
                return reader.ReadToEnd();
            }
        }

        private void SetMapSettings(HttpListenerRequest req, HttpListenerResponse resp) {
            string data = this.ReadPostData(req);
            try {
                data_objects.MapSettingsDO constraintUpdate = JsonSerializer.Deserialize<data_objects.MapSettingsDO>(data);
                resp.ContentLength64 = 0;
                resp.ContentType = "plain";
            }
            catch (System.Text.Json.JsonException) {
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }


        private void UpdateConstraints(HttpListenerRequest req, HttpListenerResponse resp) {
            string data = this.ReadPostData(req);
            try {
                data_objects.ConstraintUpdateDO constraintUpdate = JsonSerializer.Deserialize<data_objects.ConstraintUpdateDO>(data);
                this.cbs = new CBS();
                if (constraintUpdate.cardinal) {
                    this.cbs.WithCardinalConflicts();
                }
                if (constraintUpdate.following) {
                    this.cbs.WithFollowingConflicts();
                }
                if (constraintUpdate.vertex) {
                    this.cbs.WithVertexConflicts();
                }
                if (constraintUpdate.edge) {
                    this.cbs.WithEdgeConflicts();
                }
                resp.ContentLength64 = 0;
                resp.ContentType = "plain";
            }
            catch (System.Text.Json.JsonException) {
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private void GetPath(HttpListenerRequest req, HttpListenerResponse resp) {
            try {
                string data = this.ReadPostData(req);
                data_objects.PathRequestDO pathRequests = JsonSerializer.Deserialize<data_objects.PathRequestDO>(data);
                List<Vertex> sources = pathRequests.GetSources(this.map);
                List<Vertex> destinations = pathRequests.GetDestinations(this.map);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                search.Solution sol = this.cbs.ShortestPath(this.map, sources, destinations);
                sw.Stop();
                var res = new data_objects.PathAnswerDO(sol, sw.ElapsedMilliseconds);
                byte[] responseData = JsonSerializer.SerializeToUtf8Bytes(res);
                resp.ContentType = "text/json";
                resp.ContentLength64 = responseData.LongLength;
                resp.ContentEncoding = Encoding.UTF8;
                resp.OutputStream.Write(responseData);
            }
            catch (System.Text.Json.JsonException) {
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (System.Exception e) {
                byte[] data = Encoding.UTF8.GetBytes(e.Message);
                resp.ContentType = "text/plain";
                resp.ContentLength64 = data.LongLength;
                resp.ContentEncoding = Encoding.UTF8;
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
                resp.OutputStream.Write(data);
            }
        }


        private void GetPathStep(HttpListenerRequest req, HttpListenerResponse resp) {
            try {
                string data = this.ReadPostData(req);
                data_objects.PathRequestDO pathRequests = JsonSerializer.Deserialize<data_objects.PathRequestDO>(data);
                // If the request has changed, re-create the enumerator
                if (this.prevRequest == null || !this.prevRequest.Equals(pathRequests)) {
                    List<Vertex> sources = pathRequests.GetSources(this.map);
                    List<Vertex> destinations = pathRequests.GetDestinations(this.map);
                    this.cbsEnumerator = this.cbs.EnumerateCBSOrder(this.map, sources, destinations);
                    this.prevRequest = pathRequests;
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                if (!this.cbsEnumerator.MoveNext()) {
                    this.cbsEnumerator = null;
                    resp.ContentLength64 = 0;
                } else {
                    sw.Stop();
                    var res = new data_objects.CBSNodeDO(this.cbs.checkers, this.cbsEnumerator.Current, sw.ElapsedMilliseconds);
                    byte[] responseData = JsonSerializer.SerializeToUtf8Bytes(res);
                    resp.ContentType = "text/json";
                    resp.ContentLength64 = responseData.LongLength;
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.OutputStream.Write(responseData);
                }

            }
            catch (System.Text.Json.JsonException) {
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (System.Exception e) {
                byte[] data = Encoding.UTF8.GetBytes(e.Message);
                resp.ContentType = "text/plain";
                resp.ContentLength64 = data.LongLength;
                resp.ContentEncoding = Encoding.UTF8;
                resp.StatusCode = (int)HttpStatusCode.BadRequest;
                resp.OutputStream.Write(data);
            }
        }

        private void dispatch(HttpListenerRequest req, HttpListenerResponse resp) {
            switch (req.HttpMethod) {
                case "GET":
                    switch (req.Url.AbsolutePath) {
                        case "/maps":
                            this.GetMaps(resp);
                            break;
                        default:
                            this.ServeFile(req.Url.AbsolutePath, resp);
                            break;
                    }
                    break;
                case "POST":
                    switch (req.Url.AbsolutePath) {
                        case "/updateMapSettings":
                            this.UpdateMapSettings(req, resp);
                            break;
                        case "/getPath":
                            this.GetPath(req, resp);
                            break;
                        case "/getPathStep":
                            this.GetPathStep(req, resp);
                            break;
                        case "/constraints":
                            this.UpdateConstraints(req, resp);
                            break;
                        case "/mapSettings":
                            this.SetMapSettings(req, resp);
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