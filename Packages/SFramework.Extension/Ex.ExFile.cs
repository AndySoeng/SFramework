using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Ex
{
    public static class ExFile
    {
        public static void ExistOrDelete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

    }
}