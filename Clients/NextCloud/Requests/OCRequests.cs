using ObisoftNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObisoftNet.Clients.NextCloud.Requests
{
    public static class OwnRequests
    {
        private static OwnClient _client;
        private static string _user = "";
        private static string _password = "";
        private static int req_size = 10;
        private static int flujo = 10;
        private static string masterkey = "master";

        public class OwnResponseUI
        {
            public string filename = "";
            public int index = 0;
            public int total = 0;
            public int speed = 0;
            public int time = 0;
        }

        
        public static void Configure(string user,string password, int reqsize = 10,string key= "master", int fl = 3)
        {
            _user = user;
            _password = password;
            req_size = reqsize;
            flujo = fl;
            masterkey = key;
        }

        public static OwnClient GetClient(bool reinit=false)
        {
            if (_client == null || reinit)
            {
                _client = new OwnClient(_user, _password);
                bool loged = _client.Login();
                if (!loged)
                    throw new Exception("La configuracion no es valida para conectar con el servidor!");
            }
            return _client;
        }



        private static string CreateID(int size = 8)
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            string map = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string result = "";
            for (int i = 0; i < size; i++)
                result += $"{map[rnd.Next(0, map.Length - 1)]}";
            return result;
        }

        private static string make_req_file(string url)
        {
            string name = $"req-{CreateID()}.txt";
            using (Stream stream = File.Create(name))
            using (TextWriter writer = new StreamWriter(stream))
                writer.Write($"GET {url}");
            return name;
        }

        public class OwnResponse
        {
            private OwnClient _ownclient;

            public string Status = "NONE";
            public string Filename = "";
            public string Id = "";
            public Dictionary<string,string> Headers = new Dictionary<string, string>();

            public void SetOwn(OwnClient own) => _ownclient = own;

            private static string make_reqcontent_file(string id, int size, int index = 0, int flujo = 3,string key="master")
            {
                string name = $"reqcontent-{id}.txt";
                using (Stream stream = File.Create(name))
                using (TextWriter writer = new StreamWriter(stream))
                    writer.Write($"{size}-{flujo}-{index}-{key}");
                return name;
            }
            private static string make_delcontent_file_from_name(string name)
            {
                string reqmame = name.Replace("content-", "delcontent-");
                using (Stream stream = File.Create(reqmame))
                using (TextWriter writer = new StreamWriter(stream))
                    writer.Write($"{name}");
                return reqmame;
            }

            public delegate void ProgressIterFunc(OwnResponseUI ui);

            public static OwnResponseUI UI = new OwnResponseUI();

            public bool write_iter_content(Stream streamtowrite,int chunk_size = 1024, int file_index = 0, int timeout = 1000, ProgressIterFunc progress_iter_func = null)
            {
                var req = make_reqcontent_file(Id, req_size, file_index, flujo,masterkey);
                var data = _client.UploadFile(req);
                File.Delete(req);
                int i = 0;
                int current_size = file_index;
                int total_size = 0;
                int icontent = 1;
                if (Headers.TryGetValue("Content-Length",out string total)) {
                    total_size = int.Parse(total);
                }
                while (i < timeout)
                {
                    var contents = GetClient().GetRoot();
                    string contentname = req.Replace("reqcontent-", $"content-{icontent}").Replace(".txt", ".bin");
                    string permissionname = req.Replace("reqcontent-", "denieged-");
                    FileResult item = null;
                    if ((item = contents.ContainsGetFromFilename(permissionname)) != null) break;
                    if ((item = contents.ContainsGetFromFilename(contentname)) != null)
                    {
                        var request = GetClient().Session.MakeGetRequest(item.Url);
                        var resp = request.GetResponseHttp();
                        if(resp.StatusCode== System.Net.HttpStatusCode.OK)
                        using(Stream stream = resp.GetResponseStream())
                        {
                                TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
                                int timestamp = (int)t.TotalSeconds;
                                int time_start = timestamp;
                                int time_total = 0;
                                int size_per_second = 0;
                                int clock_start = timestamp;
                                byte[] chunk = new byte[1024];
                                int reading = 0;
                                while ((reading = stream.Read(chunk, 0, chunk.Length)) != 0)
                                {
                                    t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
                                    timestamp = (int)t.TotalSeconds;
                                    current_size += chunk.Length;
                                    size_per_second += chunk.Length;
                                    int tcurrent = timestamp - time_start;
                                    time_total += tcurrent;
                                    time_start = timestamp;
                                    int clock_time = (total_size - current_size) / (size_per_second);
                                    UI.filename = Filename;
                                    UI.index = current_size;
                                    UI.total = total_size;
                                    UI.speed = size_per_second;
                                    UI.time = clock_time;
                                    if (time_total >= 1)
                                    {
                                        if (progress_iter_func != null)
                                            progress_iter_func(UI);
                                        time_total = 0;
                                        size_per_second = 0;
                                    }
                                    byte[] newbuff = new byte[reading];
                                    Array.Copy(chunk, newbuff,reading);
                                    streamtowrite.Write(newbuff, 0, reading);
                                    streamtowrite.Flush();
                                }
                                Thread tdel = new Thread(new ParameterizedThreadStart(delete_thread));
                                tdel.Start(contentname);
                        }
                        icontent += 1;
                        if (icontent >= flujo)
                            icontent = 1;
                    }
                    string endcontentreq = req.Replace("reqcontent-", "endcontent-");
                    if ((item = contents.ContainsGetFromFilename(endcontentreq)) != null)
                    {
                        GetClient().DeleteFile(endcontentreq);
                        return true;
                    }
                }
                return false;
            }

            private void delete_thread(object obj)
            {
                string delfile = obj.ToString();
                bool deleted = GetClient().DeleteFile(delfile);
                string delcontent = make_delcontent_file_from_name(delfile);
                var data = GetClient().UploadFile(delcontent);
                if(File.Exists(delcontent))
                File.Delete(delcontent);
            }
        } 

        public static OwnResponse Get(string url,int timeout=100)
        {
            var req = make_req_file(url);
            var data = GetClient().UploadFile(req);
            File.Delete(req);
            if (data != null)
            {
                int i = 0;
                while (i < timeout)
                {
                    i++;
                    var responses = GetClient().GetRoot();
                    var respname = req.Replace("req-", "resp-");
                    FileResult item = null;
                    if ((item = responses.ContainsGetFromFilename(respname))!=null)
                    {
                        var stringcontent = GetClient().Session.GetString(item.Url);
                        var content = stringcontent.Replace("\"", "").Replace("\r", "").Split('\n');
                        bool deleted = GetClient().DeleteFile(respname);
                        string status = content[0];
                        Dictionary<string, string> headers = new Dictionary<string, string>();
                        foreach(var h in content)
                        {
                            try
                            {
                                if (h=="OK") continue;
                                var tokens = h.Split(':');
                                string key = tokens[0];
                                string value = tokens[1];
                                headers.Add(key, value);
                            }
                            catch { }
                        }
                        OwnResponse response = new OwnResponse();
                        string filename = "";
                        if (headers.TryGetValue("filename", out filename))
                            response.Filename = filename;
                        response.Headers = headers;
                        response.Status = status;
                        response.Id = req.Replace("req-", "").Replace(".txt", "");
                        response.SetOwn(GetClient());
                        return response;
                    }
                }
            }
            return null;
        }

        
    }
}