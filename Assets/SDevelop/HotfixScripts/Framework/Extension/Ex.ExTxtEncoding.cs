using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace Ex
{
    public static class ExTxtEncoding
    {
        public static string CovertUnicode2UTF8(string unicodeStr)
        {
            Regex reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            unicodeStr = reg.Replace(unicodeStr,
                delegate(Match m) { return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });
            byte[] postBytes = Encoding.UTF8.GetBytes(unicodeStr);
            Encoding gb2312 = Encoding.GetEncoding("UTF-8");
            return gb2312.GetString(postBytes);
        }
    }
}