using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ObisoftNet.Html
{
    public class HtmlParser
    {
        private string regex1 = @"<(?:'[^']*'['']*|'[^']*'['']*|[^''>])+>";
        private string regex2 = @"<(?:'[^']*'['']*|'[^']*'['']*|[^''>])+>(.*?)<(?:'[^']*'['']*|'[^']*'['']*|[^''>])+>";
        private string regex3 = @"<([a-zA-Z]+)(?:[^>]*[^/]*)?>";

        private static int SetTagContent(int index,string[] lines,HtmlTag tag)
        {
            int endmax = 0;
            string text = "";
            for (int i = index + 1; i < lines.Length; i++)
            {
                string content = lines[i];

                if (content == "" ||
                    content == " " ||
                    content == "  " ||
                    content == "   ")
                    continue;

                if (Regex.IsMatch(content, "</(.*?)>"))
                {
                    endmax--;
                    if (endmax <= 1)
                    {
                        tag.Text = text;
                        return i;
                    }
                }
                else
                if (Regex.IsMatch(content, "<br />") || Regex.IsMatch(content, "<br/>"))
                {
                    
                }else
                if (Regex.IsMatch(content, "<(.*?)+>"))
                {
                    Match parse = Regex.Match(content, "<(.*?)+>");
                    string line = parse.Value;

                    string[] tokens = line.Split(' ');
                    string tagname = "";

                    tagname = tokens[0].Substring(1).Replace(">","");

                    HtmlTag newtag = new HtmlTag(tagname);
                    newtag.Tags = new List<HtmlTag>();
                    newtag.Text = "";
                    newtag.Attribs = new Dictionary<string, string>();

                    if (tokens.Length>0)
                    for(int itag = 1; itag < tokens.Length; itag++)
                    {
                        try
                        {
                            string[] attrsplit = tokens[itag].Split('=');
                            string key = attrsplit[0];
                            string val = "";
                            if (attrsplit.Length > 2)
                            {
                                    for (var item = 1; item < attrsplit.Length; item++)
                                        val += attrsplit[item] + "=";
                                    val = val.Remove(val.Length - 1);
                            }
                            else val = attrsplit[1];
                            newtag.Attribs.Add(key, val);
                        }
                        catch { }
                    }

                    if (Regex.IsMatch(parse.Value, "(.*?)/>"))
                    {
                        tag.Text = text;
                        tag.Tags.Add(newtag); 
                        endmax--;
                        continue;
                    }

                    var ii = SetTagContent(i, lines, newtag);
                    if (tag.Tags == null)
                        tag.Tags = new List<HtmlTag>();
                    tag.Tags.Add(newtag);
                    if(ii==i)
                        endmax++;
                    i = ii;
                    continue;
                }
                text += content;
            }
            tag.Text = text;
            return index;
        }

        public static HtmlTag Parse(string html)
        {
            string[] lines = html.Replace("\"", "'").
                Replace("='", "=").Replace("' ", " ").Replace("'>", " >").
                Replace(">", ">\n").Replace("</", "\n</").
                Split('\n');
            HtmlTag root = new HtmlTag("root");
            if(lines.Length>1)
            SetTagContent(0, lines, root);
            return root.Find("html");
        }

    }
}
