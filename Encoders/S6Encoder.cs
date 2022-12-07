using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ObisoftNet.Encoders
{
    public class S6Encoder
    {
        public static char CryptoChar(char value)
        {
            string map = "@./=#$%&:,;_-|0123456789abcd3fghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (map.Contains(value))
            {
               int i = map.IndexOf(value);
               return map[map.Length - 1 - i];
            }
            return value;
        }
        private static string _enc(string text)
        {
            string result = "";
            foreach(var ch in text)
            {
                result += CryptoChar(ch);
            }
            return result;
        }
        public static string encrypt(string text, string key = "keytoenc")
        {
            if (key.Length > 10)
                throw new Exception("LA KEY SOLO PUEDE TENER 9 CARACTERES");
            string result = _enc(text);
            string keyenc = _enc(key);
            string keylenenc = _enc(keyenc.Length.ToString());
            return keylenenc + result + keyenc;
        }
        public static string decrypt(string text, string key = "keytoenc")
        {
            if (key.Length > 10)
                throw new Exception("LA KEY SOLO PUEDE TENER 9 CARACTERES");
            try
            {
                int keylen = int.Parse(CryptoChar(text[0]).ToString());
                int indexkey = text.Length - keylen;
                string keyintext = _enc(text.Substring(indexkey));
                if (keyintext == key)
                {
                    return _enc(text.Remove(indexkey).Substring(1));
                }
                else
                {
                    throw new Exception("Key No Valida!");
                }
            }
            catch { }
            return text;
        }
    }
}
