using System;
using System.IO;
using System.Security.Cryptography;

/*
        string test = "你好，世界！Hello World!";
        string sercet = "1123uyrlouhd@_Lq";
        string encrypt = ExCrypto_DES.DESEncrypt(test);
        Debug.Log(encrypt);
        string decrypt = ExCrypto_DES.DESDecrypt(encrypt);
        Debug.Log(decrypt);
 */

namespace Ex
{
    /// <summary>
    /// DES加密解密
    /// </summary>
    public class ExCrypto_DES
    {
        /// <summary>
        /// DES密钥
        /// </summary>
        private static byte[] _KEY = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        /// <summary>
        /// DES向量
        /// </summary>
        private static byte[] _IV = new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };

        /// <summary>
        /// DES加密操作
        /// </summary>
        /// <param name="normalTxt">需要加密的明文字符串</param>
        /// <returns>返回DES加密的密文字符串</returns>
        public static string DESEncrypt(string normalTxt)
        {
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(_KEY, _IV), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(normalTxt);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            string strRet = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            return strRet;
        }

        /// <summary>
        /// DES解密操作
        /// </summary>
        /// <param name="securityTxt">需要解密的密文字符串</param>
        /// <returns>返回DES解密之后的明文字符串</returns>
        public static string DESDecrypt(string securityTxt) //解密  
        {
            byte[] byEnc;
            try
            {
                securityTxt.Replace("_%_", "/");
                securityTxt.Replace("-%-", "#");
                byEnc = Convert.FromBase64String(securityTxt);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(_KEY, _IV), CryptoStreamMode.Read);
            StreamReader sr = new StreamReader(cst);
            return sr.ReadToEnd();
        }
    }
}