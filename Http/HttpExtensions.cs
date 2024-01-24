
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace obisoft.net.http
{
    public static class HttpExtensions
    {

        public static HttpWebResponse GetResponseHttp(this WebRequest resp) => resp.GetResponse() as HttpWebResponse;
        public static string GetString(this WebResponse resp)
        {
            try
            {
                using (var stream = resp.GetResponseStream())
                {
                    using (TextReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch { }
            return "";
        }
        public static byte[] GetBytes(this WebResponse resp)
        {
            List<byte> bytes = new List<byte>();
            try
            {
                using (Stream captchastream = resp.GetResponseStream())
                {
                    byte[] buff = new byte[1024];
                    int read = 0;
                    while ((read = captchastream.Read(buff, 0, buff.Length)) != 0)
                    {
                        Array.Resize(ref buff, read);
                        bytes.AddRange(buff);
                    }
                }
            }
            catch { }
            return bytes.ToArray();
        }
        public static void SendFile(this HttpListenerResponse response,string filepath)
        {
            FileInfo fi = new FileInfo(filepath);
            if (fi.Exists)
            {
                string mimeType = MimeType.GetMimeType(fi.Name);
                using (Stream filestream = File.OpenRead(fi.FullName))
                {
                    using (Stream outputstream = response.OutputStream)
                    {
                        response.ContentLength64 = fi.Length;
                        response.ContentType = mimeType;
                        response.Headers.Add("Content-Disposition", $"attachment; filename={fi.Name}");
                        byte[] buff = new byte[1024];
                        int read = 0;
                        while ((read=filestream.Read(buff,0,buff.Length))!=0)
                        {
                            byte[] buffwrite = new byte[read];
                            Array.Copy(buff, buffwrite, read);
                            outputstream.Write(buffwrite, 0, read);
                            outputstream.Flush();
                        }
                    }
                }
            }
        }
        public static void SendFile(this HttpListenerResponse response, Stream filestream,string filename)
        {
            string mimeType = MimeType.GetMimeType(filename);
            using (Stream outputstream = response.OutputStream)
                    {
                        response.ContentLength64 = filestream.Length;
                        response.ContentType = mimeType;
                        if (mimeType != "text/html")
                            response.Headers.Add("Content-Disposition", $"attachment; filename={filename}");
                        byte[] buff = new byte[1024];
                        int read = 0;
                        while ((read = filestream.Read(buff, 0, buff.Length)) != 0)
                        {
                            byte[] buffwrite = new byte[read];
                            Array.Copy(buff, buffwrite, read);
                            outputstream.Write(buffwrite, 0, read);
                            outputstream.Flush();
                        }
                    }
        }
        public static string GetText(this HttpListenerRequest req)
        {
            try
            {
                using (StreamReader stream = new StreamReader(req.InputStream, req.ContentEncoding))
                {
                    return stream.ReadToEnd();
                }
            }
            catch { }
            return null;
        }
        public static void SendJson(this HttpListenerResponse response, string json, string type = "json")
        {
            response.Send(json, $"application/{type}");
        }
        public static void Send(this HttpListenerResponse response, string resp, string mimeType = "text/html")
        {

            using (Stream filestream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
            {
                using (Stream outputstream = response.OutputStream)
                {
                    response.ContentLength64 = filestream.Length;
                    response.ContentType = mimeType;
                    byte[] buff = new byte[1024];
                    int read = 0;
                    while ((read = filestream.Read(buff, 0, buff.Length)) != 0)
                    {
                        byte[] buffwrite = new byte[read];
                        Array.Copy(buff, buffwrite, read);
                        outputstream.Write(buffwrite, 0, read);
                        outputstream.Flush();
                    }
                }
            }
        }

    }
}
