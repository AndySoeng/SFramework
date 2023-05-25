using Microsoft.Win32;

namespace Ex
{
    public class Ex_ExWebFireExe
    {
        /// <summary>
        /// Web拉起Exe前，进行注册表
        /// </summary>
        /// <param name="keyName">注册项名称（web使用此名称进行拉起）</param>
        /// <param name="keyValue">软件名称</param>
        /// <param name="appPath">软件路径</param>
        private static void Register(string keyName, string keyValue, string appPath)
        {
            string apppath = appPath;
            RegistryKey key;
            key = Registry.ClassesRoot.CreateSubKey(keyName);
            key.SetValue("", keyValue);
            key.SetValue("URL Protocol", appPath);
            RegistryKey iconkey;
            iconkey = key.CreateSubKey("DefaultIcon");
            iconkey.SetValue("", appPath);
            key = key.CreateSubKey("shell");
            key = key.CreateSubKey("open");
            key = key.CreateSubKey("command");
            key.SetValue("", apppath);
        }
    }
}