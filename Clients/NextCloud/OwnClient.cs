using ObisoftNet.Html;
using ObisoftNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace ObisoftNet.Clients.NextCloud
{
    public static class OwnExtensions
    {
        public static FileResult ContainsGetFromFilename(this List<FileResult> items,string key)
        {
            for(var i =0;i<items.Count;i++)
            {
                if (items[i].FileName == key)
                    return items[i];
            }
            return null;
        }
    }
    public enum ResultStatus
    {
        Default,
        FileCreate,
        FileExist,
        FolderExist,
        FileShare,
        Error
    }
    public class FileResult
    {
        public ResultStatus Status = ResultStatus.Default;
        public string FileName = "";
        public string FilePath = "";
        public string Url = "";
    }

    public class OwnClient : IDisposable
    {
        private string _user;
        private string _password;
        private string _host;
        private HttpSession _session;
        private string _agent = "Mozilla/5.0 (Linux; Android 10; dandelion) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.101 Mobile Safari/537.36";

        private string _requesttoken = null;

        public bool Loged { get; set; } = false;
        public HttpSession Session => _session;

        public OwnClient(string user,string password,string host= "https://misarchivos.uci.cu/owncloud/")
        {
            _user = user;
            _password = password;
            _host = host;

            _session = new HttpSession();
        }

        private Dictionary<string,string> MakeHeaders()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            return result;
        }

        //Cliente Functions
        public bool Login()
        {
            var headers = MakeHeaders();
            string loginurl = $"{_host}index.php/login";
            var text = _session.GetString(loginurl,headers: headers);
            HtmlTag html = HtmlParser.Parse(text);
            var requesttoken = html.FindInAll("head").Attribs["data-requesttoken"];
            string timezone = "Europe/Berlin";
            string remember_login = "1";
            string timezone_offset = "2";
            Dictionary<string, string> formdata = new Dictionary<string, string>();
            formdata.Add("user",_user);
            formdata.Add("password", _password);
            formdata.Add("timezone", timezone);
            formdata.Add("remember_login", remember_login);
            formdata.Add("timezone_offset", timezone_offset);
            formdata.Add("requesttoken", requesttoken);
            formdata.Add("redirect_url", "/owncloud/index.php/apps/files/");
            var resp = _session.Post($"{loginurl}?redirect_url=/owncloud/index.php/apps/files/", data: formdata,headers: headers);
            text = _session.GetStringFromResponse(resp);
            html = HtmlParser.Parse(text);
            var title = html.FindInAll("title");
            if (title.Text.Contains("Archivos - ownCloud"))
            {
                _requesttoken = html.FindInAll("head").Attribs["data-requesttoken"];
                Loged = true;
                return true;
            }
            return false;
        }

        public delegate void ProgressFunc(string file,int index,int total,int speed,int time);
        public FileResult UploadFile(string filepath,string rootpath="", ProgressFunc progressfunc =null)
        {
            FileResult result = null;
            if (_requesttoken != null)
            {
                string uploadurl = $"{_host}remote.php/webdav/{rootpath}{filepath}";
                using (Stream stream = File.OpenRead(filepath))
                {
                    var headers = MakeHeaders();
                    headers.Add("requesttoken",_requesttoken);
                    var request = _session.MakePutRequest(uploadurl, headers: headers) as HttpWebRequest;
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        int chunk_por = 0;
                        int total = (int)stream.Length;
                        int time_start = DateTime.Now.Second;
                        int time_total = 0;
                        int size_per_second = 0;
                        int clock_start = DateTime.Now.Second;
                        byte[] chunk = new byte[1024];
                        int reading = 0;
                        while ((reading = stream.Read(chunk, 0, chunk.Length)) != 0)
                        {
                            chunk_por += chunk.Length;
                            size_per_second += chunk.Length;
                            int tcurrent = DateTime.Now.Second - time_start;
                            time_total += tcurrent;
                            time_start = DateTime.Now.Second;
                            int clock_time = (total - chunk_por) / (size_per_second);
                            if (progressfunc != null)
                                progressfunc(filepath, chunk_por, total, size_per_second, clock_time);
                            time_total = 0;
                            size_per_second = 0;
                            Array.Resize(ref chunk, reading);
                            reqStream.Write(chunk,0, chunk.Length);
                        }
                    }
                    var resp = request.GetResponseHttp();
                    var urltokens = filepath.Split('/');
                    string filename = urltokens[urltokens.Length - 1];
                    if (resp.StatusCode == HttpStatusCode.Created)
                        result = new FileResult() {Status=ResultStatus.FileCreate, FileName = filename, FilePath = filepath, Url =resp.ResponseUri.ToString()};
                    if (resp.StatusCode == HttpStatusCode.NoContent)
                        result = new FileResult() { Status = ResultStatus.FileExist,FileName=filename, FilePath = filepath, Url = resp.ResponseUri.ToString() };
                    if (resp.StatusCode == HttpStatusCode.Conflict)
                        result = new FileResult() { Status = ResultStatus.FolderExist, FileName = filename, FilePath = filepath, Url = "" };
                }
            }
            return result;
        }

        private string GetPropfind()
        {
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(ObisoftNet)}.Clients.NexCloud.propfind.txt"))
                {
                    using (TextReader reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
            }
            catch { }
            return null;
        }

        
        public List<FileResult> GetRoot(string path="")
        {
            List<FileResult> result = new List<FileResult>();
            string rooturl = $"{_host}remote.php/webdav/{path}";
            string propfind = GetPropfind();
            if (propfind != null && _requesttoken!=null)
            {
                var headers = MakeHeaders();
                headers.Add("Depth", "1");
                headers.Add("requesttoken", _requesttoken);
                var request = _session.MakeRequest("PROPFIND", rooturl, headers: headers);
                var response = request.GetResponseHttp();
                if (((int)(response.StatusCode)) == 207)
                {
                    string data = _session.GetStringFromResponse(response);
                    HtmlTag xml = HtmlParser.Parse(data.Replace("<d:", "<").Replace("</d:", "</"));
                    var root = xml.FindAll("href");
                    foreach(var item in root)
                    {
                        string url = $"{_host.Replace("/owncloud/","")}{item.Text}";
                        string filepath = item.Text.Replace("/owncloud/remote.php/webdav/", "");
                        var urltokens = url.Split('/');
                        string filename = urltokens[urltokens.Length - 1];
                        result.Add(new FileResult() { 
                            Status = ResultStatus.FileShare,
                            FileName = filename,
                            FilePath = filepath,
                            Url = url
                        });
                    }
                }
            }
            return result;
        }

        public string ShareFile(FileResult fr,string password="")
        {
            string shareurl = $"{_host}ocs/v2.php/apps/files_sharing/api/v1/shares?format=xml";
            bool passwordChanged = password != "";
            if (_requesttoken!=null)
            {
                var headers = MakeHeaders();
                headers.Add("requesttoken", _requesttoken);
                headers.Add("OCS-APIREQUEST", "true");
                Dictionary<string, string> formdata = new Dictionary<string, string>();
                formdata.Add("password", password);
                formdata.Add("passwordChanged", passwordChanged.ToString());
                formdata.Add("permissions", "19");
                formdata.Add("expireDate", "");
                formdata.Add("shareType", "3");
                formdata.Add("path", fr.FilePath);
                var resp = _session.Post(shareurl, data: formdata, headers: headers);
                var text = _session.GetStringFromResponse(resp);
                HtmlTag html = HtmlParser.Parse(text);
                return html.FindInAll("url")?.Text;
            }
            return "";
        }

        public bool DeleteFile(FileResult file)
        {
            string deleteurl = $"{_host}remote.php/webdav/{file.FilePath}";
            if (_requesttoken != null)
            {
                try
                {
                    var headers = MakeHeaders();
                    headers.Add("requesttoken", _requesttoken);
                    var request = _session.MakeRequest("DELETE", deleteurl, headers: headers);
                    var resp = request.GetResponseHttp();
                    if (((int)(resp.StatusCode)) == 204)
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        public bool DeleteFile(string filepath)
        {
            string deleteurl = $"{_host}remote.php/webdav/{filepath}";
            if (_requesttoken != null)
            {
                try
                {
                    var headers = MakeHeaders();
                    headers.Add("requesttoken", _requesttoken);
                    var request = _session.MakeRequest("DELETE", deleteurl, headers: headers);
                    var resp = request.GetResponseHttp();
                    if (((int)(resp.StatusCode)) == 204)
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        public void Dispose(){}
    }
}
