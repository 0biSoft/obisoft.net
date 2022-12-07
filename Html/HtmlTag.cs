using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObisoftNet.Html
{
    public class HtmlTag
    {
        public string Name { get; protected set; }
        public string Text { get; set; }
        public List<HtmlTag> Tags { get; set; }
        public Dictionary<string, string> Attribs { get; set; }

        public HtmlTag(string name)
        {
            Name = name;
            Attribs = new Dictionary<string, string>();
        }

        public HtmlTag Find(string tagname,Dictionary<string,string> attribs=null)
        {
            if (Tags != null)
                foreach (var t in Tags)
                    if (t.Name == tagname)
                        if (attribs == null)
                            return t;
                        else
                        if (Attribs != null)
                        {
                            List<bool> accepts = new List<bool>();
                            foreach(var attr in attribs)
                            {
                                string val = null;
                                if (t.Attribs.TryGetValue(attr.Key,out val))
                                {
                                    if(val==attr.Value)
                                    accepts.Add(true);
                                }
                            }
                            if (accepts.Count > 0)
                                return t;
                        }
            return null;
        }

        public List<HtmlTag> FindAll(string tagname, Dictionary<string, string> attribs = null)
        {
            List<HtmlTag> result = new List<HtmlTag>();
            if (Tags != null)
                foreach (var t in Tags)
                    if (t.Name == tagname)
                        if (attribs == null)
                            result.Add(t);
                        else
                        if (Attribs != null)
                        {
                            List<bool> accepts = new List<bool>();
                            foreach (var attr in attribs)
                            {
                                string val = null;
                                if (t.Attribs.TryGetValue(attr.Key, out val))
                                {
                                    if (val == attr.Value)
                                        accepts.Add(true);
                                }
                            }
                            if (accepts.Count > 0)
                                result.Add(t);
                        }
            return result;
        }

        private HtmlTag find_in_all(string tagname,HtmlTag tag, Dictionary<string, string> attribs = null)
        {
            HtmlTag result=null;
            if (tag.Tags!=null)
            foreach (var t in tag.Tags)
            {
                result = t.Find(tagname, attribs);
                if (result != null)
                    return result;
                result = find_in_all(tagname,t,attribs);
                if (result != null)
                    return result;
            }
            return result;
        }
        private List<HtmlTag> find_all_in_all(string tagname, HtmlTag tag, Dictionary<string, string> attribs = null)
        {
            List<HtmlTag> result = new List<HtmlTag>();
            if (tag.Tags != null)
                foreach (var t in tag.Tags)
                {
                    var findresult = t.FindAll(tagname, attribs);
                    if (findresult != null)
                        result.AddRange(findresult);
                    findresult = find_all_in_all(tagname, t, attribs);
                    if (findresult != null)
                        result.AddRange(findresult);
                }
            return result;
        }
        public HtmlTag FindInAll(string tagname, Dictionary<string, string> attribs = null)
        {
            return find_in_all(tagname, this,attribs);
        }
        public List<HtmlTag> FindAllInAll(string tagname, Dictionary<string, string> attribs = null)
        {
            return find_all_in_all(tagname, this, attribs);
        }


        public HtmlTag this[string tag] => Find(tag);
        public HtmlTag this[int index] => Tags[index];

    }
}
