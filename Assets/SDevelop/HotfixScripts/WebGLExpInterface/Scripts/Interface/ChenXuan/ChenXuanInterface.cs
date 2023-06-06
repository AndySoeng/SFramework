using System;
using Ex;
using UnityEngine;
using WebGLExpInterface.DTO;

namespace WebGLExpInterface
{
    public static class ChenXuanInterface
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);
#endif


        private static readonly string hostSuffix = "/experiment/addExperiment/";


        public static void AddExperiment(this MonoBehaviour mono, string ilabJson, Action failureCallBack,
            Action<DTO_ChenXuan.StatusInfo> successCallBack)
        {
            mono.StartCoroutine(ExUnityWebRequest.WebRequest(ExUnityWebRequest.UnityWebRequestType.GET, Application.streamingAssetsPath + "/PLAINTXTCONFIG", "", false, false,
                () =>
                {
                    Debug.LogError("未获取到配置文件");
                    failureCallBack.Invoke();
                }, (arg0 =>
                {
                    string hostPrefix = String.Empty;
                    string[] line = arg0.Split('\n');
                    foreach (var l in line)
                    {
                        if (l.StartsWith("chenxuanHostPrefix="))
                        {
                            hostPrefix = l.Replace("chenxuanHostPrefix=", "");
                        }
                    }

                    string host = hostPrefix + hostSuffix;
                    AddExperimentImp(mono, host, ilabJson, failureCallBack, successCallBack);
                }), null, null));
        }

        private static void AddExperimentImp(this MonoBehaviour mono, string host, string ilabJson, Action failureCallBack,
            Action<DTO_ChenXuan.StatusInfo> successCallBack)
        {
            string token = String.Empty;
#if UNITY_EDITOR
            token = "84561605125232068622526788021623?type=1";
#elif UNITY_WEBGL&& !UNITY_EDITOR
            token = getUrlParams("token");
#endif

            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("token不存在");
                failureCallBack?.Invoke();
                return;
            }

            DTO_ChenXuan.DTO_AddExperiment addExperiment = new DTO_ChenXuan.DTO_AddExperiment(token, ilabJson);
            string jsonData = LitJson.JsonMapper.ToJson(addExperiment);
            mono.StartCoroutine(ExUnityWebRequest.WebRequest(ExUnityWebRequest.UnityWebRequestType.POST, host, jsonData, false, false, () => { }, (result) =>
            {
                DTO_ChenXuan.StatusInfo statusInfo = LitJson.JsonMapper.ToObject<DTO_ChenXuan.StatusInfo>(result);
                successCallBack?.Invoke(statusInfo);
            }, null, null));
        }


        #region DTO

      

        #endregion
    }
}