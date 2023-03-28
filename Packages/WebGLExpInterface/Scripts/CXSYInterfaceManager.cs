using System;
using System.Runtime.InteropServices;
using LitJson;
using UnityEngine;

namespace ExpInterface
{
    public static class CXSYInterfaceManager
    {
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);


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

        public static void CXSYInterface_SendScore(this MonoBehaviour mono, int score, string[] keys, string[] values, Action failureCallBack,
            Action<string> successCallBack)
        {
            if (init == false)
            {
                return;
            }

            CXSYData data = new CXSYData(status, submitId, score, keys, values);
            string jsonData = JsonMapper.ToJson(data);
            string url = host + hostSuffix;
            mono.StartCoroutine(ExpInterfaceBase.WebRequest(UnityWebRequestType.POST, url, jsonData, false,
                false, () => { failureCallBack?.Invoke(); }, (str) =>
                {
                    CXSYDataReply reply = JsonMapper.ToObject<CXSYDataReply>(str);
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

        public class CXSYData
        {
            public int status;
            public int submitId;
            public int virtualExperimentScore;

            public string[] keys;
            public string[] values;

            public CXSYData(int status, int submitId, int virtualExperimentScore, string[] keys, string[] values)
            {
                this.status = status;
                this.submitId = submitId;
                this.virtualExperimentScore = virtualExperimentScore;
                this.keys = keys;
                this.values = values;
            }

            public CXSYData()
            {
            }
        }

        public class CXSYDataReply
        {
            public int code;
            public string msg;
        }
    }
}