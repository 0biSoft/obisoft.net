using ObisoftNet.Html;
using ObisoftNet.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ObisoftNet.Clients.WebMail
{
    public class WebMailClient
    {
        internal string _host;
        internal Uri _datauri;
        public  string View => "mobile";
        public  string Lang => "en_US";

        public HttpSession Session { get; private set; }

        public WebMailClient(string host= "https://webmail.nauta.cu/")
        {
            _host = host;
            Session = new HttpSession();
        }

        public List<WebMailChat> GetChats(string type="mailbox")
        {
            List<WebMailChat> result = new List<WebMailChat>();
            try
            {
                string chatsurl = _datauri.ToString().Split('&')[0] + "&page=" + type;
                var htmltext = Session.Get(chatsurl).GetString();

                var html = HtmlParser.Parse(htmltext);

                var tr_alls = html.FindAllInAll("tr");

                for (var i = tr_alls.Count-1; i > 0;i--)
                {
                    var tr = tr_alls[i];

                    var td_alls = tr.FindAll("td");
                    if (td_alls.Count > 0)
                    {
                        string U = td_alls[1].Text;
                        string From = td_alls[2].Text;
                        string Url = $"{_host.Remove(_host.Length-1)}{td_alls[3].Find("a").Attribs["href"]}";
                        string Subject = td_alls[3].Find("a").Text;

                        WebMailChat chat = new WebMailChat();
                        chat.MessagesUrl = new List<string>();

                        bool exist = false;
                        foreach (var ch in result)
                        {
                            if (ch.From == From)
                            {
                                chat = ch;
                                exist = true;
                                break;
                            }
                        }

                        chat.U = U;
                        chat.From = From;
                        chat.Client = this;
                        chat.Subject = Subject;
                        chat.MessagesUrl.Add(Url);
                        if(!exist)
                        result.Add(chat);
                    }
                }
            }
            catch { }
            return result;
        }

        public Bitmap GetCaptcha()
        {
            string captcha = $"{_host}/securimage/securimage_show.php";
            var resp = Session.Get(captcha);
            Bitmap bmp = resp.GetBitmap();
            return bmp;
        }
       

        public WebMailLogin MakeLogin(bool includecaptcha=false)
        {
            WebMailLogin loginresult = new WebMailLogin();
            Dictionary<string, string> attr = new Dictionary<string, string>();
            string loginurl = $"{_host}login.php";
            var resp = Session.Get(loginurl);
            string htmltext = resp.GetString();
            var html = HtmlParser.Parse(htmltext);
            
            attr.Add("name", "app");
            string app = html.FindInAll("input", attr).Attribs["value"];

            attr.Clear();
            attr.Add("name", "login_post");
            string post = html.FindInAll("input", attr).Attribs["value"];
            post = "1";

            attr.Clear();
            attr.Add("name", "url");
            string url = html.FindInAll("input", attr).Attribs["value"];

            attr.Clear();
            attr.Add("name", "anchor_string");
            string anchor_string = html.FindInAll("input", attr).Attribs["value"];

            attr.Clear();
            attr.Add("name", "anticsrf");
            string anticsrf = html.FindInAll("input", attr).Attribs["value"];

            loginresult.App = app;
            loginresult.LoginPost = post;
            loginresult.Url = url;
            loginresult.LoginUrl = loginurl;
            loginresult.AnchorString = anchor_string;
            loginresult.Anticsrf = anticsrf;
            loginresult.Client = this;

            if(includecaptcha)
            loginresult.Captcha = GetCaptcha();
            
            return loginresult;
        }
    }
}
