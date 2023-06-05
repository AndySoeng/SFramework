using System;
using UnityEngine;

namespace WebGLExpInterface
{
    public static class CXInterfaceManager
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);
#endif
        
        private static string host
        {
            get => "http://123.60.156.121:8001/remote_java/experiment/addExperiment/";
        }

        public static void AddExperiment(this MonoBehaviour mono, string ilabJson, Action failureCallBack,
            Action<StatusInfo> successCallBack)
        {
            string token = String.Empty;
#if UNITY_WEBGL && !UNITY_EDITOR
            token = getUrlParams("token");
#endif

            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("token不存在");
                failureCallBack?.Invoke();
                return;
            }

            DTO_AddExperiment addExperiment = new DTO_AddExperiment(token, ilabJson);
            string jsonData = LitJson.JsonMapper.ToJson(addExperiment);
            mono.StartCoroutine(ExpInterfaceBase.WebRequest(UnityWebRequestType.POST, host, jsonData, false, false, () => { }, (result) =>
            {
                StatusInfo statusInfo = LitJson.JsonMapper.ToObject<StatusInfo>(result);
                successCallBack?.Invoke(statusInfo);
            }, null, null));
        }


        #region DTO

        public class StatusInfo
        {
            public int code;
            public string message;
            public int data;
        }

        public class DTO_AddExperiment
        {
            /// <summary>
            /// 客户名
            /// </summary>
            public string customName;

            /// <summary>
            /// 用户账号类型
            /// </summary>
            public string accountType;

            /// <summary>
            /// 用户账号
            /// </summary>
            public string accountNumber;

            /// <summary>
            /// 用户名
            /// </summary>
            public string userName;

            /// <summary>
            /// token
            /// </summary>
            public string accessToken;

            /// <summary>
            /// 实验信息json
            /// </summary>
            public string contextJson;

            /// <summary>
            /// 实验平台的URL
            /// </summary>
            public string remoteUrl;

            /// <summary>
            /// 类型（1：普通学生实验保存  2：评审实验保存）
            /// </summary>
            public string type;

            public DTO_AddExperiment(string accessToken, string contextJson)
            {
                this.accessToken = accessToken;
                this.contextJson = contextJson;
            }
        }

        #endregion
    }
}