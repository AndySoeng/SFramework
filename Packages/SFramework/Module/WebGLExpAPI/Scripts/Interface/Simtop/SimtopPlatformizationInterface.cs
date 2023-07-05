using System;
using System.Runtime.InteropServices;
using Ex;
using LitJson;
using UnityEngine;
using WebGLExpInterface.DTO;

namespace WebGLExpInterface
{
    public static class SimtopPlatformizationInterface
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);
#endif

        /// <summary>
        /// 是否为老师布置的实验
        /// </summary>
        private static int status;

        /// <summary>
        /// 该次实验的记录id 
        /// </summary>
        private static int submitId;

        /// <summary>
        /// 调用接口前缀
        /// </summary>
        private static string host;

        private static readonly string hostSuffix = "student/experiment/updateVirtualExperiment";

        private static bool init = false;

        public static void Init()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                host = getUrlParams("host");
                status = int.Parse(getUrlParams("status"));
                submitId = int.Parse(getUrlParams("submitId"));
                init = true;
            }
            catch (Exception e)
            {
                Debug.LogError("获取实验信息出错，请联系管理员。");
                init = false;
            }
#endif
        }

        public static void SendScore(this MonoBehaviour mono, int score, string[] keys, string[] values, Action failureCallBack,
            Action<string> successCallBack)
        {
            if (init == false)
            {
                return;
            }

            DTO_SimtopPlatformization.CXSYData data = new DTO_SimtopPlatformization.CXSYData(status, submitId, score, keys, values);
            string jsonData = JsonMapper.ToJson(data);
            string url = host + hostSuffix;
            mono.StartCoroutine(ExUnityWebRequest.WebRequest(ExUnityWebRequest.UnityWebRequestType.POST, url, jsonData, false,
                false, () => { failureCallBack?.Invoke(); }, (str) =>
                {
                    DTO_SimtopPlatformization.CXSYDataReply reply = JsonMapper.ToObject<DTO_SimtopPlatformization.CXSYDataReply>(str);
                    if (reply.code == 200)
                    {
                        successCallBack?.Invoke(reply.msg);
                    }
                    else
                    {
                        failureCallBack?.Invoke();
                    }
                }, null, null));

        }

   
    }
}