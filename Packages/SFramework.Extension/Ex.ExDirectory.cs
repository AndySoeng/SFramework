using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Ex
{
    public static class ExDirectory
    {
        public static string ExistOrCreateDir(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            return path;
        }

    }
}