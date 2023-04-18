using System.IO;
using Unity.IO.Compression;

namespace Ex
{
    public static class ExGZip
    {
        public static byte[] Compress(string str)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                using (var writer = new StreamWriter(gzipStream))
                {
                    writer.Write(str);
                }

                return memoryStream.ToArray();
            }
        }

        public static string Decompress(byte[] bytes)
        {
            using var memoryStream = new MemoryStream(bytes);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzipStream);
            return reader.ReadToEnd();
        }
    }
}