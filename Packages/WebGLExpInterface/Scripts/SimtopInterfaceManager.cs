using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace WebGLExpInterface
{
    public class SimtopInterfaceManager
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string LoadParams();
#endif

        public static string[] userInfo;


        // Start is called before the first frame update
        public static void GetParams(Action failureCallBack, Action successCallBack)
        {

            userInfo = new string[]
            {
                "",
                "",
                ""
            };
#if UNITY_WEBGL && !UNITY_EDITOR
      userInfo = LoadParams().Split('~');
#endif

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("登陆信息");
            for (int i = 0; i < userInfo.Length; i++)
            {
                sb.AppendLine(userInfo[i]);
            }

            Debug.Log(sb.ToString());


            if (userInfo[0] == "null" || userInfo[1] == "null" ||
                userInfo[2] == "null")
            {
                //ModalWindowPanelScreen.OpenModalWindowNoTabs("系统提示", "检测到当前用户未登录\n<color=red>请返回实验网站进行登录。</color>", false, null, false);
                failureCallBack?.Invoke();
            }
            else
            {
                successCallBack?.Invoke();
            }
        }
    }
}