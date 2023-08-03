
using System;
using System.Text;

namespace Ex
{
    /// <summary>
    /// 讯飞工具类
    /// </summary>
    public class ExXunFei
    {
        
        /// <summary>
        /// 接口鉴权,组装身份验证Url
        /// </summary>
        /// <returns></returns>
        public static string AssembleAuthUrl( string hostUrl,string APISecret,string APIKey,string algorithm="hmac-sha256",string headers="host date request-line")
        {
            Uri url = new Uri(hostUrl);
            string date = DateTime.UtcNow.ToString("R");
            string[] signString = new[] { "host: " + url.Host, "date: " + date, "GET " + url.LocalPath + " HTTP/1.1" };
            string sgin = string.Join("\n", signString);
            string sha = ExCrypto_HmacSHA256Base64.HmacWithShaTobase64(sgin, APISecret);
            string authUrl = $"api_key=\"{APIKey}\",algorithm=\"{algorithm}\",headers=\"{headers}\",signature=\"{sha}\"";
            string authorization = Ex.ExCrypto_Base64.Base64Encrypt(authUrl);
            string callUrl = hostUrl +
                             $"?authorization={authorization}" +
                             $"&date={date}" +
                             $"&host={url.Host}";
            return callUrl;
        }
        
        
    }
}