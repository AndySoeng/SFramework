using System;
using System.Security.Cryptography;
using System.Text;

namespace Ex
{
    /// <summary>
    /// HmacSHA256 Base64算法
    /// </summary>
    public class ExCrypto_HmacSHA256Base64
    {
        /// <summary>
        /// HmacSHA256 Base64算法,返回的结果始终是32位
        /// </summary>
        /// <param name="message">待加密的明文字符串</param>
        /// <param name="secret"></param>
        /// <returns>HmacSHA256算法加密之后的密文</returns>
        public static string HmacWithShaTobase64(string message, string secret)
        {
            byte[] keyByte = Encoding.GetEncoding("utf-8").GetBytes(secret);
            byte[] messageBytes = Encoding.GetEncoding("utf-8").GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}