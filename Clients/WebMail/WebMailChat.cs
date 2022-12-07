using ObisoftNet.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Clients.WebMail
{
    public class WebMailChat
    {
        public string U { get; internal set; }
        public string From { get; internal set; }
        public WebMailClient Client { get; internal set; }
        public string Subject { get; internal set; }

        internal List<string> MessagesUrl;
        public int MessagesCount => MessagesUrl.Count;

        public WebMailMessage GetMessage(int index)
        {
            WebMailMessage message = new WebMailMessage();
            message.Subject = Subject;

            var htmltext = Client.Session.GetString(MessagesUrl[index].Replace("amp;",""));
            var html = HtmlParser.Parse(htmltext);

            var div_alls = html.FindAllInAll("div");

            foreach(var div in div_alls)
            {
                try
                {
                    try
                    {
                        if (div.Attribs["class"] == "mimePartData")
                        {
                            message.Text = div.Text;
                        }
                    }
                    catch { }
                    string key = div.Find("em").Text;
                    key = key.Remove(key.Length - 1);
                    string value = div.Text;
                    if (key == "Attachment")
                    {
                        string[] attachsplit = value.Split(' ');
                        string name = attachsplit[2];
                        string mimetype = attachsplit[4];
                        string urlattach = $"{Client._host.Remove(Client._host.Length-1)}{div.Find("a").Attribs["href"]}";

                        WebMailAttachment attachment = new WebMailAttachment();

                        attachment.Name = name;
                        attachment.MimeType = mimetype;
                        attachment._attachurl = urlattach;
                        attachment.Message = message;

                        message.Attachements.Add(attachment);
                        continue;
                    }
                    foreach(var prop in message.GetType().GetProperties())
                    {
                        if (prop.Name == key)
                        {
                            prop.SetValue(message, value);
                        }
                    }
                }
                catch { }
            }

            return message;
        }

        public List<WebMailMessage> GetMessages()
        {
            List<WebMailMessage> result = new List<WebMailMessage>();
            for (int i = 0; i < MessagesCount; i++)
            {
                result.Add(GetMessage(i));
            }
            return result;
        }

    }
}
