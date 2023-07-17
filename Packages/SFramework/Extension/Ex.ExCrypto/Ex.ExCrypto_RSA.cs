// using System;
// using System.IO;
// using System.Security.Cryptography;
// using System.Text;
//
// namespace Ex
// {
//     /// <summary>
//     /// RSA加密解密：采用公钥，私钥的模式
//     /// </summary>
//     public class ExCrypto_RSA
//     {
//         private static CspParameters Param;
//
//         /// <summary>
//         /// RSA加密/解密的默认公钥
//         /// </summary>
//         private static string _publicKey =
//             @"<RSAKeyValue><Modulus>5m9m14XH3oqLJ8bNGw9e4rGpXpcktv9MSkHSVFVMjHbfv+SJ5v0ubqQxa5YjLN4vc49z7SVju8s0X4gZ6AzZTn06jzWOgyPRV54Q4I0DCYadWW4Ze3e+BOtwgVU1Og3qHKn8vygoj40J6U85Z/PTJu3hN1m75Zr195ju7g9v4Hk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
//
//         /// <summary>
//         /// RSA解密/加密的默认私钥
//         /// </summary>
//         private static string _privateKey =
//             @"<RSAKeyValue><Modulus>5m9m14XH3oqLJ8bNGw9e4rGpXpcktv9MSkHSVFVMjHbfv+SJ5v0ubqQxa5YjLN4vc49z7SVju8s0X4gZ6AzZTn06jzWOgyPRV54Q4I0DCYadWW4Ze3e+BOtwgVU1Og3qHKn8vygoj40J6U85Z/PTJu3hN1m75Zr195ju7g9v4Hk=</Modulus><Exponent>AQAB</Exponent><P>/hf2dnK7rNfl3lbqghWcpFdu778hUpIEBixCDL5WiBtpkZdpSw90aERmHJYaW2RGvGRi6zSftLh00KHsPcNUMw==</P><Q>6Cn/jOLrPapDTEp1Fkq+uz++1Do0eeX7HYqi9rY29CqShzCeI7LEYOoSwYuAJ3xA/DuCdQENPSoJ9KFbO4Wsow==</Q><DP>ga1rHIJro8e/yhxjrKYo/nqc5ICQGhrpMNlPkD9n3CjZVPOISkWF7FzUHEzDANeJfkZhcZa21z24aG3rKo5Qnw==</DP><DQ>MNGsCB8rYlMsRZ2ek2pyQwO7h/sZT8y5ilO9wu08Dwnot/7UMiOEQfDWstY3w5XQQHnvC9WFyCfP4h4QBissyw==</DQ><InverseQ>EG02S7SADhH1EVT9DD0Z62Y0uY7gIYvxX/uq+IzKSCwB8M2G7Qv9xgZQaQlLpCaeKbux3Y59hHM+KpamGL19Kg==</InverseQ><D>vmaYHEbPAgOJvaEXQl+t8DQKFT1fudEysTy31LTyXjGu6XiltXXHUuZaa2IPyHgBz0Nd7znwsW/S44iql0Fen1kzKioEL3svANui63O3o5xdDeExVM6zOf1wUUh/oldovPweChyoAdMtUzgvCbJk1sYDJf++Nr0FeNW1RB1XG30=</D></RSAKeyValue>";
//
//
//
//         #region RSA加密解密：采用公钥，私钥的模式
//
//         #region 私钥加密，公钥解密
//
//         /// <summary>
//         /// RSA私钥加密
//         /// </summary>
//         /// <param name="privateKey">Java格式的RSA私钥 base64格式</param>
//         /// <param name="contentData">待加密的数据；调用方法Encoding.GetEncoding("UTF-8").GetBytes(contentData)</param>
//         /// <param name="algorithm">加密算法</param>
//         /// <returns>RSA私钥加密之后的密文</returns>
//         public static string EncryptWithPrivateKey(string privateKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
//         {
//             return Convert.ToBase64String(EncryptWithPrivateKey(Convert.FromBase64String(privateKey), contentData, algorithm));
//         }
//
//         private static byte[] Transform(AsymmetricKeyParameter key, byte[] contentData, string algorithm, bool forEncryption)
//         {
//             var c = CipherUtilities.GetCipher(algorithm);
//             c.Init(forEncryption, new ParametersWithRandom(key));
//             return c.DoFinal(contentData);
//         }
//
//         /// <summary>
//         /// RSA私钥加密
//         /// </summary>
//         /// <param name="privateKey">Java格式的RSA私钥</param>
//         /// <param name="contentData">待加密的数据；调用方法Encoding.GetEncoding("UTF-8").GetBytes(contentData)</param>
//         /// <param name="algorithm">加密算法</param>
//         /// <returns>RSA私钥加密之后的密文</returns>
//         public static byte[] EncryptWithPrivateKey(byte[] privateKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
//         {
//             RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(privateKey);
//             return Transform(privateKeyParam, contentData, algorithm, true);
//         }
//
//         /// <summary>
//         /// RSA公钥解密
//         /// </summary>
//         /// <param name="publicKey">Java格式的RSA公钥  base64格式</param>
//         /// <param name="content">待解密数据 base64格式</param>
//         /// <param name="encoding">解密出来的数据编码格式，默认UTF-8</param>
//         /// <param name="algorithm">加密算法</param>
//         /// <returns>RSA私钥解密之后的明文</returns>
//         public static string DecryptWithPublicKey(string publicKey, string content, string encoding = "UTF-8", string algorithm = "RSA/ECB/PKCS1Padding")
//         {
//             return Encoding.GetEncoding(encoding).GetString(DecryptWithPublicKey(Convert.FromBase64String(publicKey), Convert.FromBase64String(content), algorithm));
//         }
//
//         /// <summary>
//         /// RSA公钥解密
//         /// </summary>
//         /// <param name="publicKey">Java格式的RSA公钥</param>
//         /// <param name="contentData">待解密数据</param>
//         /// <param name="algorithm">加密算法</param>
//         /// <returns>RSA私钥解密之后的明文</returns>
//         public static byte[] DecryptWithPublicKey(byte[] publicKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
//         {
//             RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(publicKey);
//             return Transform(publicKeyParam, contentData, algorithm, false);
//         }
//
//         #endregion
//
//         #region 公钥加密，私钥解密
//
//         /// <summary>
//         /// RSA公钥加密
//         /// </summary>
//         /// <param name="xmlPublicKey">加密公钥；为空则默认系统公钥</param>
//         /// <param name="enptStr">需要加密的明文字符串</param>
//         /// <param name="encoding">编码格式；默认：UTF-8</param>
//         /// <returns>RSA公钥加密的密文</returns>
//         public static string RSAEncrypt_Public(string xmlPublicKey, string enptStr, string encoding = "UTF-8")
//         {
//             if (string.IsNullOrEmpty(xmlPublicKey))
//             {
//                 xmlPublicKey = _publicKey;
//             }
//
//             using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
//             {
//                 byte[] cipherbytes;
//                 rsa.FromXmlString(xmlPublicKey);
//                 cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(enptStr), false);
//                 return Convert.ToBase64String(cipherbytes);
//             }
//         }
//
//         /// <summary>
//         /// RSA私钥解密
//         /// </summary>
//         /// <param name="xmlPrivateKey">解密私钥；为空则默认系统公钥</param>
//         /// <param name="enptStr">需要加密的明文字符串</param>
//         /// <param name="encoding">编码格式；默认：UTF-8</param>
//         /// <returns>RSA私钥解密的明文</returns>
//         public static string RSADecrypt_Private(string xmlPrivateKey, string enptStr, string encoding = "UTF-8")
//         {
//             if (string.IsNullOrEmpty(xmlPrivateKey))
//             {
//                 xmlPrivateKey = _privateKey;
//             }
//
//             using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
//             {
//                 byte[] cipherbytes;
//                 rsa.FromXmlString(xmlPrivateKey);
//                 cipherbytes = rsa.Decrypt(Convert.FromBase64String(enptStr), false);
//                 return Encoding.GetEncoding(encoding).GetString(cipherbytes);
//             }
//         }
//
//         #endregion
//
//         #region 使用同一容器的名称进行RSA加密解密
//
//         /// <summary>
//         /// 进行 RSA 加密
//         /// </summary>
//         /// <param name="sourceStr">源字符串</param>
//         /// <returns>加密后字符串</returns>
//         public static string RsaEncrypt(string sourceStr)
//         {
//             Param = new CspParameters();
//             //密匙容器的名称，保持加密解密一致才能解密成功
//             Param.KeyContainerName = "Navis";
//             using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(Param))
//             {
//                 //将要加密的字符串转换成字节数组
//                 byte[] plaindata = Encoding.Default.GetBytes(sourceStr);
//                 //通过字节数组进行加密
//                 byte[] encryptdata = rsa.Encrypt(plaindata, false);
//                 //将加密后的字节数组转换成字符串
//                 return Convert.ToBase64String(encryptdata);
//             }
//         }
//
//         /// <summary>
//         /// 通过RSA 加密方式进行解密
//         /// </summary>
//         /// <param name="codingStr">加密字符串</param>
//         /// <returns>解密后字符串</returns>
//         public static string RsaDesEncrypt(string codingStr)
//         {
//             Param = new CspParameters();
//             Param.KeyContainerName = "Navis";
//             using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(Param))
//             {
//                 byte[] encryptdata = Convert.FromBase64String(codingStr);
//                 byte[] decryptdata = rsa.Decrypt(encryptdata, false);
//                 return Encoding.Default.GetString(decryptdata);
//             }
//         }
//
//         #endregion
//
//         #region RSA分段加密：待加密的字符串拆开，每段长度都小于等于限制长度，然后分段加密
//
//         /// <summary>
//         /// RSA分段加密
//         /// </summary>
//         /// <param name="xmlPublicKey">RSA C#公钥</param>
//         /// <param name="enptStr">需要进行RSA加密的长字符串</param>
//         /// <returns>返回RSA加密后的密文</returns>
//         public static String SubRSAEncrypt(string xmlPublicKey, string enptStr)
//         {
//             RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
//             provider.FromXmlString(xmlPublicKey);
//             Byte[] bytes = Encoding.Default.GetBytes(enptStr);
//             int MaxBlockSize = provider.KeySize / 8 - 11; //加密块最大长度限制
//
//             if (bytes.Length <= MaxBlockSize)
//                 return Convert.ToBase64String(provider.Encrypt(bytes, false));
//
//             using (MemoryStream PlaiStream = new MemoryStream(bytes))
//             using (MemoryStream CrypStream = new MemoryStream())
//             {
//                 Byte[] Buffer = new Byte[MaxBlockSize];
//                 int BlockSize = PlaiStream.Read(Buffer, 0, MaxBlockSize);
//
//                 while (BlockSize > 0)
//                 {
//                     Byte[] ToEncrypt = new Byte[BlockSize];
//                     Array.Copy(Buffer, 0, ToEncrypt, 0, BlockSize);
//
//                     Byte[] Cryptograph = provider.Encrypt(ToEncrypt, false);
//                     CrypStream.Write(Cryptograph, 0, Cryptograph.Length);
//
//                     BlockSize = PlaiStream.Read(Buffer, 0, MaxBlockSize);
//                 }
//
//                 return Convert.ToBase64String(CrypStream.ToArray(), Base64FormattingOptions.None);
//             }
//         }
//
//         /// <summary>
//         /// RSA分段解密，应对长字符串
//         /// </summary>
//         /// <param name="xmlPrivateKey">RSA C#私钥</param>
//         /// <param name="enptStr">需要解密的长字符串</param>
//         /// <returns>返回RSA分段解密的明文</returns>
//         public static String SubRSADecrypt(string xmlPrivateKey, string enptStr)
//         {
//             RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
//             provider.FromXmlString(xmlPrivateKey);
//             Byte[] bytes = Convert.FromBase64String(enptStr);
//             int MaxBlockSize = provider.KeySize / 8; //解密块最大长度限制
//
//             if (bytes.Length <= MaxBlockSize)
//                 return Encoding.Default.GetString(provider.Decrypt(bytes, false));
//
//             using (MemoryStream CrypStream = new MemoryStream(bytes))
//             using (MemoryStream PlaiStream = new MemoryStream())
//             {
//                 Byte[] Buffer = new Byte[MaxBlockSize];
//                 int BlockSize = CrypStream.Read(Buffer, 0, MaxBlockSize);
//
//                 while (BlockSize > 0)
//                 {
//                     Byte[] ToDecrypt = new Byte[BlockSize];
//                     Array.Copy(Buffer, 0, ToDecrypt, 0, BlockSize);
//
//                     Byte[] Plaintext = provider.Decrypt(ToDecrypt, false);
//                     PlaiStream.Write(Plaintext, 0, Plaintext.Length);
//
//                     BlockSize = CrypStream.Read(Buffer, 0, MaxBlockSize);
//                 }
//
//                 return Encoding.Default.GetString(PlaiStream.ToArray());
//             }
//         }
//
//         #endregion
//
//         #endregion
//     }
// }