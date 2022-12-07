using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Clients.WebMail
{
    public class WebMailMessage
    {
        public string Subject { get; internal set; } = "";
        public string Date { get; internal set; } = "";
        public string From { get; internal set; } = "";
        public string To { get; internal set; } = "";

        public List<WebMailAttachment> Attachements { get; set; } = new List<WebMailAttachment>();
        public string Text { get; internal set; } = "";
    }
}
