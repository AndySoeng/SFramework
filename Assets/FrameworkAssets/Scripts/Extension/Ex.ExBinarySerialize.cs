using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Ex
{
    public static class ExBinarySerialize
    {
        public static void SerializeFile<T>(T t, string path)
        {
            using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            var bf = new BinaryFormatter();
            bf.Serialize(fs, t);
        }

        public static T DeserializeFile<T>(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var bf = new BinaryFormatter();
            return (T) bf.Deserialize(fs);
        }

        public static byte[] SerializeBytes<T>(T t)
        {
            using MemoryStream ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, t);
            return ms.GetBuffer();
        }
      

        public static T DeserializeBytes<T>(byte[] bytes)
        {
            using MemoryStream ms = new MemoryStream(bytes);
            var bf = new BinaryFormatter();
            return (T) bf.Deserialize(ms);
        }
    }
}