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


        public enum OpenTypeURLWebGL
        {
            NONE,

            /// <summary>
            /// 浏览器会另开一个新窗口显示链接
            /// </summary>
            _blank,

            /// <summary>
            /// 在同一框架或窗口中打开所链接的文档。此参数为默认值，通常不用指定。
            /// </summary>
            _self,

            /// <summary>
            /// 将链接的文件载入含有该链接框架的父框架集或父窗口中。如果含有该链接的框架不是嵌套的，则在浏览器全屏窗口中载入链接的文件，就象_self参数一样。
            /// </summary>
            _parent,

            /// <summary>
            /// 在当前的整个浏览器窗口中打开所链接的文档，因而会删除所有框架
            /// </summary>
            _top,

            /// <summary>
            /// 在浏览器的搜索区装载文档，注意，这个功能只在Internet Explorer 5 或者更高版本中适用。
            /// </summary>
            _search,
        }

        public static void OpenURL(string url, OpenTypeURLWebGL openType = OpenTypeURLWebGL.NONE)
        {
#if UNITY_EDITOR
            Application.OpenURL(url);
#elif UNITY_WEBGL
            if (OpenTypeURLWebGL.NONE == openType)
                OpenPage(url);
            else
                Application.ExternalEval("window.open('" + url + $"','{openType.ToString()}')");
#else
            Application.OpenURL(url);
#endif
        }
    }
}