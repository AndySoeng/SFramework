
using System.Runtime.InteropServices;
using UnityEngine;


namespace Ex
{
    public static class ExBrowser
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void OpenPage(string str);
#endif


        public static void OpenURL(string url)
        {
#if UNITY_EDITOR
            Application.OpenURL(url);
#elif UNITY_WEBGL
            OpenPage(url);
#else
            Application.OpenURL(url);
#endif
        }
    }
}
