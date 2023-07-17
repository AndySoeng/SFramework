using System;
using System.Text;

namespace Ex
{
    /// <summary>
    /// Base64加码解码，采用utf8编码
    /// </summary>
    public class ExCrypto_Base64
    {
        /// <summary>
        /// Base64编码，采用utf8编码
        /// </summary>
        /// <param name="strPath">待编码的明文</param>
        /// <returns>Base64编码后的字符串</returns>
        public static string Base64Encrypt(string strPath)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(strPath));
        }

        /// <summary>
        /// Base64解码，采用utf8编码方式解码
        /// </summary>
        /// <param name="strPath">待解码的密文</param>
        /// <returns>Base64解码的明文字符串</returns>
        public static string Base64Decrypt(string strPath)
        {
            byte[] c = Convert.FromBase64String(strPath);
            return Encoding.UTF8.GetString(c);
        }
    }
}