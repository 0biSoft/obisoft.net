using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Clients.WebMail
{
    public class WebMailAttachment
    {
        internal string _attachurl;

        public string Name { get; internal set; }
        public string MimeType { get; internal set; }
        public WebMailMessage Message { get; internal set; }


        public delegate void WebMailAttachmentDownloadProgress(string filename);
        public void Download(string filepath="")
        {
            if (_attachurl != null)
            {

            }
        }
    }
}
