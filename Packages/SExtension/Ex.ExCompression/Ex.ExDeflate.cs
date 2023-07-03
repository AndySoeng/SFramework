
using System.IO;
using Unity.IO.Compression;

namespace Ex
{
    public static class ExDeflate
    {
        public static byte[] Compress(string str)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                using (var writer = new StreamWriter(deflateStream))
                {
                    writer.Write(str);
                }
                return memoryStream.ToArray();
            }
        }

        public static string Decompress(byte[] bytes)
        {
            using var memoryStream = new MemoryStream(bytes);
            using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
            using var reader = new StreamReader(deflateStream);
            return reader.ReadToEnd();
        }
    }
}