using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ex
{
    public class ExCrypto_AES
    {
     
        /// <summary>
        /// 加密字符串
        /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="encryptStr">待加密密文</param>
        /// <param name="EncryptKey">加密密钥</param>
        public static byte[] AESEncrypt(string encryptStr, string key)
        {
            return AESEncrypt(Encoding.UTF8.GetBytes(encryptStr), key);
        }

        /// <summary>
        /// 加密字节数组
        /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="EncryptString">待加密密文</param>
        /// <param name="EncryptKey">加密密钥</param>
        public static byte[] AESEncrypt(byte[] encryptByte,string key)
        {
            if (encryptByte.Length == 0)
            {
                throw (new Exception("明文不得为空"));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw (new Exception("密钥不得为空"));
            }

            byte[] m_strEncrypt;
            byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
            Rijndael m_AESProvider = Rijndael.Create();
            try
            {
                MemoryStream m_stream = new MemoryStream();
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(key, m_salt);
                ICryptoTransform transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
                CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                m_csstream.Write(encryptByte, 0, encryptByte.Length);
                m_csstream.FlushFinalBlock();
                m_strEncrypt = m_stream.ToArray();
                m_stream.Close();
                m_stream.Dispose();
                m_csstream.Close();
                m_csstream.Dispose();
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                m_AESProvider.Clear();
            }

            return m_strEncrypt;
        }


        /// <summary>
        /// 解密字符串
        /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="decryptStr">待解密密文</param>
        /// <param name="DecryptKey">解密密钥</param>
        public static byte[] AESDecrypt(string decryptStr,string key)
        {
            string[] encryptArray = decryptStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] bytes = new byte[encryptArray.Length];
            for (int i = 0; i < encryptArray.Length; i++)
            {
                bytes[i] = Convert.ToByte(encryptArray[i], 16);
            }

            return AESDecrypt(bytes,key);
        }

        /// <summary>
        /// 解密字节数组
        /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="DecryptString">待解密密文</param>
        /// <param name="DecryptKey">解密密钥</param>
        public static byte[] AESDecrypt(byte[] decryptByte,string key)
        {
            if (decryptByte.Length == 0)
            {
                throw (new Exception("密文不得为空"));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw (new Exception("密钥不得为空"));
            }

            byte[] m_strDecrypt;
            byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
            Rijndael m_AESProvider = Rijndael.Create();
            try
            {
                MemoryStream m_stream = new MemoryStream();
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(key, m_salt);
                ICryptoTransform transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
                CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                m_csstream.Write(decryptByte, 0, decryptByte.Length);
                m_csstream.FlushFinalBlock();
                m_strDecrypt = m_stream.ToArray();
                m_stream.Close();
                m_stream.Dispose();
                m_csstream.Close();
                m_csstream.Dispose();
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                m_AESProvider.Clear();
            }

            return m_strDecrypt;
        }
    }
}