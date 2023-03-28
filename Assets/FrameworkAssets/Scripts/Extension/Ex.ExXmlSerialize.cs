using System.IO;
using System.Xml.Serialization;

namespace Ex
{
    public static class ExXmlSerialize
    {
        public static void SerializeFile<T>(T t, string path)
        {
            using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            var xs = new XmlSerializer(t.GetType());
            xs.Serialize(sw, t);
        }

        public static T DeserializeFile<T>(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            var xs = new XmlSerializer(typeof(T));
            return (T) xs.Deserialize(fs);
        }
        
        
        public static byte[] SerializeBytes<T>(T t)
        {
            using MemoryStream ms = new MemoryStream();
            var xs = new XmlSerializer(t.GetType());
            xs.Serialize(ms, t);
            return ms.GetBuffer();
        }
      

        public static T DeserializeBytes<T>(byte[] bytes)
        {
            using MemoryStream ms = new MemoryStream(bytes);
            var xs = new XmlSerializer(typeof(T));
            return (T) xs.Deserialize(ms);
        }
    }
}