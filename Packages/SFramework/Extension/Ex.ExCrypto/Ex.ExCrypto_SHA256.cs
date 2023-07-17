using System.Security.Cryptography;
using System.Text;

namespace Ex
{
    /// <summary>
    /// SHA256 加密
    /// </summary>
    public class ExCrypto_SHA256
    {
        /// <summary>
        /// SHA256加密
        /// </summary>
        /// <param name="strIN">要加密的string字符串</param>
        /// <returns>SHA256加密之后的密文</returns>
        public static string SHA256Encrypt(string strIN)
        {
            byte[] tmpByte;
            SHA256 sha256 = new SHA256Managed();
            tmpByte = sha256.ComputeHash(GetKeyByteArray(strIN));

            StringBuilder rst = new StringBuilder();
            for (int i = 0; i < tmpByte.Length; i++)
            {
                rst.Append(tmpByte[i].ToString("x2"));
            }

            sha256.Clear();
            return rst.ToString();
        }

        /// <summary>
        /// 获取要加密的string字符串字节数组
        /// </summary>
        /// <param name="strKey">待加密字符串</param>
        /// <returns>加密数组</returns>
        private static byte[] GetKeyByteArray(string strKey)
        {
            UTF8Encoding Asc = new UTF8Encoding();
            int tmpStrLen = strKey.Length;
            byte[] tmpByte = new byte[tmpStrLen - 1];
            tmpByte = Asc.GetBytes(strKey);
            return tmpByte;
        }
    }
}