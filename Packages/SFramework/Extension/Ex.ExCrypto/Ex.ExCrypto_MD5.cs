using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


/*      //测试代码
        string test = "你好，世界！Hello World!";
        string sercet = "1123uyrlouhd@_Lq";
        string encrypt = ExCrypto_MD5.Md5Encrypt(test, sercet);
        Debug.Log(encrypt);
        string decrypt = ExCrypto_MD5.Md5Decrypt(encrypt, sercet);
        Debug.Log(decrypt);
 */


namespace Ex
{
    /// <summary>
    /// MD5加密解密
    /// </summary>
    public class ExCrypto_MD5
    {
        /// <summary>
        ///  Md5密钥加密
        /// </summary>
        /// <param name="pToEncrypt">要加密的string字符串</param>
        /// <param name="secret">8位加密密钥</param>
        /// <returns></returns>
        public static string Md5Encrypt(string pToEncrypt, string secret)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
            des.Key = Encoding.ASCII.GetBytes(secret);
            des.IV = Encoding.ASCII.GetBytes(secret);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }

            var s = ret.ToString();
            return s;
        }
        
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="pToEncrypt">要加密的string字符串</param>
        /// <returns></returns>
        public static string Md5Encrypt(string pToEncrypt)
        {
            var md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(pToEncrypt));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                builder.Append(data[i].ToString("x2")); // 将每个byte转换为十六进制并添加到StringBuilder对象中
            return builder.ToString();
        }

        /// <summary>
        ///  Md5解密
        /// </summary>
        /// <param name="pToDecrypt">解密string</param>.
        /// <param name="secret">8位加密密钥</param>
        /// <returns></returns>
        public static string Md5Decrypt(string pToDecrypt, string secret)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = Encoding.ASCII.GetBytes(secret);
            des.IV = Encoding.ASCII.GetBytes(secret);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

    }
}