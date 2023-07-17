using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Ex
{
    /// <summary>
    /// AES加密解密
    /// </summary>
    public class ExCrypto_AES
    {
        /// <summary>
        /// 16位的加密秘钥
        /// </summary>
        private static string Key = "1123uyrlouhd@_Lq";

        /// <summary>
        /// 16位以上的默认向量
        /// </summary>
        private static string vector = "*abcdefghijklmnopqrst@";

        /// <summary>
        ///  AES base64 加密算法；Key 为16位
        /// </summary>
        /// <param name="Data">需要加密的字符串</param>
        /// <returns></returns>
        public static string RST_AesEncrypt_Base64(string Data)
        {
            if (string.IsNullOrEmpty(Data))
            {
                return null;
            }

            if (string.IsNullOrEmpty(Key))
            {
                return null;
            }

            string Vector = Key.Substring(0, 16);
            Byte[] plainBytes = Encoding.UTF8.GetBytes(Data);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);
            Byte[] bVector = new Byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);
            Byte[] Cryptograph = null; // 加密后的密文  
            Rijndael Aes = Rijndael.Create();
            //add 
            Aes.Mode = CipherMode.CBC; //兼任其他语言的des
            Aes.BlockSize = 128;
            Aes.Padding = PaddingMode.PKCS7;
            //add end
            try
            {
                // 开辟一块内存流  
                using (MemoryStream Memory = new MemoryStream())
                {
                    // 把内存流对象包装成加密流对象  
                    using (CryptoStream Encryptor = new CryptoStream(Memory,
                               Aes.CreateEncryptor(bKey, bVector),
                               CryptoStreamMode.Write))
                    {
                        // 明文数据写入加密流  
                        Encryptor.Write(plainBytes, 0, plainBytes.Length);
                        Encryptor.FlushFinalBlock();

                        Cryptograph = Memory.ToArray();
                    }
                }
            }
            catch
            {
                Cryptograph = null;
            }

            return Convert.ToBase64String(Cryptograph);
        }

        /// <summary>
        ///  AES base64 解密算法；Key为16位
        /// </summary>
        /// <param name="Data">需要解密的字符串</param>
        /// <param name="Key">Key为16位 密钥</param>
        /// <returns></returns>
        public static string RST_AesDecrypt_Base64(string Data)
        {
            try
            {
                if (string.IsNullOrEmpty(Data))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(Key))
                {
                    return null;
                }

                string Vector = Key.Substring(0, 16);
                Byte[] encryptedBytes = Convert.FromBase64String(Data);
                Byte[] bKey = new Byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);
                Byte[] bVector = new Byte[16];
                Array.Copy(Encoding.UTF8.GetBytes(Vector.PadRight(bVector.Length)), bVector, bVector.Length);
                Byte[] original = null; // 解密后的明文  
                Rijndael Aes = Rijndael.Create();
                //add 
                Aes.Mode = CipherMode.CBC; //兼任其他语言的des
                Aes.BlockSize = 128;
                Aes.Padding = PaddingMode.PKCS7;
                //add end
                try
                {
                    // 开辟一块内存流，存储密文  
                    using (MemoryStream Memory = new MemoryStream(encryptedBytes))
                    {
                        //把内存流对象包装成加密流对象  
                        using (CryptoStream Decryptor = new CryptoStream(Memory,
                                   Aes.CreateDecryptor(bKey, bVector),
                                   CryptoStreamMode.Read))
                        {
                            // 明文存储区  
                            using (MemoryStream originalMemory = new MemoryStream())
                            {
                                Byte[] Buffer = new Byte[1024];
                                Int32 readBytes = 0;
                                while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
                                {
                                    originalMemory.Write(Buffer, 0, readBytes);
                                }

                                original = originalMemory.ToArray();
                            }
                        }
                    }
                }
                catch
                {
                    original = null;
                }

                return Encoding.UTF8.GetString(original);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 密钥16位或者32位的AES base64加密
        /// </summary>
        /// <param name="value">需要进行加密的明文字符串</param>
        /// <param name="key">16位或者32位的密钥</param>
        /// <param name="iv">16位以上的向量；默认为："*Gc_Yy_Cq_@_Ztl_99*"</param>
        /// <returns>AES加密之后的密文</returns>
        public static string AesEncrypt(string value, string key, string iv = "")
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (string.IsNullOrEmpty(key))
            {
                key = Key;
            }

            if (key.Length < 16) throw new Exception("指定的密钥长度不能少于16位。");
            if (key.Length > 32) throw new Exception("指定的密钥长度不能多于32位。");
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) throw new Exception("指定的密钥长度不明确。");
            if (string.IsNullOrEmpty(iv))
            {
                iv = vector;
            }

            if (!string.IsNullOrEmpty(iv))
            {
                if (iv.Length < 16) throw new Exception("指定的向量长度不能少于16位。");
            }

            var _keyByte = Encoding.UTF8.GetBytes(key);
            var _valueByte = Encoding.UTF8.GetBytes(value);
            using (var aes = new RijndaelManaged())
            {
                aes.IV = !string.IsNullOrEmpty(iv) ? Encoding.UTF8.GetBytes(iv) : Encoding.UTF8.GetBytes(key.Substring(0, 16));
                aes.Key = _keyByte;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                var cryptoTransform = aes.CreateEncryptor();
                var resultArray = cryptoTransform.TransformFinalBlock(_valueByte, 0, _valueByte.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }

        /// <summary>
        /// 密钥16位或者32位的AES base64解密
        /// </summary>
        /// <param name="value">需要解密的密文</param>
        /// <param name="key">16位或者32位的密钥需和加密时的密钥保持一致</param>
        /// <param name="iv">16位以上的向量需和加密时的向量保持一致；默认为："*Gc_Yy_Cq_@_Ztl_99*"</param>
        /// <returns>AES解密之后的明文</returns>
        public static string AesDecrypt(string value, string key, string iv = "")
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (string.IsNullOrEmpty(key))
            {
                key = Key;
            }

            if (key.Length < 16) throw new Exception("指定的密钥长度不能少于16位。");
            if (key.Length > 32) throw new Exception("指定的密钥长度不能多于32位。");
            if (key.Length != 16 && key.Length != 24 && key.Length != 32) throw new Exception("指定的密钥长度不明确。");
            if (string.IsNullOrEmpty(iv))
            {
                iv = vector;
            }

            if (!string.IsNullOrEmpty(iv))
            {
                if (iv.Length < 16) throw new Exception("指定的向量长度不能少于16位。");
            }

            var _keyByte = Encoding.UTF8.GetBytes(key);
            var _valueByte = Convert.FromBase64String(value);
            using (var aes = new RijndaelManaged())
            {
                aes.IV = !string.IsNullOrEmpty(iv) ? Encoding.UTF8.GetBytes(iv) : Encoding.UTF8.GetBytes(key.Substring(0, 16));
                aes.Key = _keyByte;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                var cryptoTransform = aes.CreateDecryptor();
                var resultArray = cryptoTransform.TransformFinalBlock(_valueByte, 0, _valueByte.Length);
                return Encoding.UTF8.GetString(resultArray);
            }
        }
    }
}