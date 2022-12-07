using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Html
{
    public class HtmlInline:IDisposable
    {
        public string HtmlText { get; private set; }


        public HtmlInline()
        {
            HtmlText = "<!DOCTYPE html>";
            Begin("html");
        }


        private List<string> EndList = new List<string>();
        private int IndexEnd = -1;
        public void Begin(string etiq,Dictionary<string,string> attribs = null)
        {
            if (etiq == null) return;
            if (etiq == "") return;
            EndList.Add(etiq);
            IndexEnd++;
            HtmlText += $"<{etiq}";
            if(attribs!=null)
            foreach(var attr in attribs)
            {
                if (attr.Key != null)
                {
                    if (attr.Key != "")
                    {
                        if (attr.Value != null)
                        {
                            HtmlText += $" {attr.Key.ToString()}";
                            if (attr.Value != "")
                                HtmlText += $"='{attr.Value.ToString()}'";
                        }
                    }
                }
            }
            HtmlText += $">\n";
        }
        public void BeginText(string text)
        {
            HtmlText += $"{text}";
        }
        public void End()
        {
                if (EndList.Count > 0)
                {
                    HtmlText += $"</{EndList[IndexEnd]}>\n";
                    EndList.RemoveAt(IndexEnd);
                    IndexEnd--;
                }
        }

        public void Dispose() => End();

        public byte[] ToBytes()
        {
            Dispose();
            return Encoding.ASCII.GetBytes(HtmlText);
        }
    }
}
