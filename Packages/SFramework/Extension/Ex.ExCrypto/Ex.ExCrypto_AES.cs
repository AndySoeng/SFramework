using System;
using System.Security.Cryptography;
using System.Text;


namespace Ex
{
    /// <summary>
    /// AES加密解密
    /// </summary>
    public class ExCrypto_AES
    {
        public static string Encrypt(string encryptStr, CipherMode cipherMode, PaddingMode paddingMode, string secretkey, string iv = "")
        {
            using (var aes = new AesManaged())
            {
                int[] digitlist = { 16, 24, 32 };
                if (string.IsNullOrEmpty(encryptStr) || string.IsNullOrEmpty(secretkey) || Array.IndexOf(digitlist, secretkey.Length) < 0)
                    return null;

                if (!string.IsNullOrEmpty(iv))
                    aes.IV = Encoding.UTF8.GetBytes(iv);

                aes.Key = Encoding.UTF8.GetBytes(secretkey);
                aes.Mode = cipherMode;
                aes.Padding = paddingMode;

                var encryptor = aes.CreateEncryptor();
                var cipherBytes = Encoding.UTF8.GetBytes(encryptStr);
                var encryptedData = encryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return Convert.ToBase64String(encryptedData);
            }
        }

        public static string Decrypt(string decryptStr, CipherMode cipherMode, PaddingMode paddingMode, string secretkey, string iv = "")
        {
            using (var aes = new AesManaged())
            {
                int[] digitlist = { 16, 24, 32 };
                if (string.IsNullOrEmpty(decryptStr) || string.IsNullOrEmpty(secretkey) || Array.IndexOf(digitlist, secretkey.Length) < 0)
                    return null;

                if (!string.IsNullOrEmpty(iv))
                    aes.IV = Encoding.UTF8.GetBytes(iv);

                aes.Key = Encoding.UTF8.GetBytes(secretkey);
                aes.Mode = cipherMode;
                aes.Padding = paddingMode;

                var decryptor = aes.CreateDecryptor();
                var cipherBytes = Convert.FromBase64String(decryptStr);
                var decryptedData = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }
    }
}