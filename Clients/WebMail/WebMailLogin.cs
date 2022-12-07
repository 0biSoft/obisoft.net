using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Clients.WebMail
{
    public class WebMailLogin
    {
        public WebMailClient Client { get; internal set; }
        public string App { get; internal set; }
        public string LoginPost { get; internal set; }
        public string LoginUrl { get; internal set; }
        public string AnchorString { get; internal set; }
        public string Anticsrf { get; internal set; }
        public string Url { get; internal set; }
        public Bitmap Captcha { get; internal set; }

        public bool PostResult(string captcha_code,string mail,string mailpassword)
        {
            Dictionary<string,string> formdata = new Dictionary<string, string>();
            formdata.Add("app", App);
            formdata.Add("login_post", LoginPost);
            formdata.Add("url", Url);
            formdata.Add("anchor_string", AnchorString);
            formdata.Add("anticsrf", Anticsrf);
            formdata.Add("horde_user", mail);
            formdata.Add("horde_pass", mailpassword);
            formdata.Add("horde_select_view", Client.View);
            formdata.Add("captcha_code", captcha_code);
            formdata.Add("new_lang", Client.Lang);
            var resp = Client.Session.Post(LoginUrl, data: formdata);
            if (resp.ResponseUri.ToString() != LoginUrl)
            {
                Client._datauri = resp.ResponseUri;
                return true;
            }
            return false;
        }
    }
}
