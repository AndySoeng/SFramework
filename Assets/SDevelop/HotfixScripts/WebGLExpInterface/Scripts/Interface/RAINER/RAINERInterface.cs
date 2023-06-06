using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Ex;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using WebGLExpInterface.DTO;

namespace WebGLExpInterface
{
    public static class RAINERInterface
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);
#endif

        public static DTO_RAINER.UserInfo userInfo;

        /// <summary>
        /// 测试平台标记
        /// </summary>
        private static bool IsTestingPlatform = false;

        private static string host
        {
            get => IsTestingPlatform ? "http://dj.owvlab.net/virexp/" : "https://xfpt.sues.edu.cn/virexp/";
        }


        /// <summary>
        /// 获取用户信息接口
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="failureCallBack"></param>
        /// <param name="successCallBack"></param>
        public static void GetUserInfo(this MonoBehaviour mono, Action failureCallBack,
            Action<DTO_RAINER.UserInfo> successCallBack)
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

            DTO_RAINER.Token tokenData = new DTO_RAINER.Token();
            tokenData.token = token;
            string jsonData = LitJson.JsonMapper.ToJson(tokenData);
            string url = host + "outer/getMessageByToken";
            mono.StartCoroutine(ExUnityWebRequest.WebRequestFrom(url, new[] { "param" }, new[] { jsonData },
                true, true, () => { failureCallBack?.Invoke(); }, (result) =>
                {
                    userInfo = LitJson.JsonMapper.ToObject<DTO_RAINER.UserInfo>(result);

                    if (userInfo.status == "909")
                    {
                        Debug.LogWarning("token失效");
                        failureCallBack?.Invoke();
                        return;
                    }

                    successCallBack?.Invoke(userInfo);
                }));
        }

        /// <summary>
        /// 发送成绩接口
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="score"></param>
        /// <param name="failureCallBack"></param>
        /// <param name="successCallBack"></param>
        public static void SendExpScore(this MonoBehaviour mono, string score, Action failureCallBack,
            Action<DTO_RAINER.StatusInfo> successCallBack)
        {
            DTO_RAINER.SendExpScore send = new DTO_RAINER.SendExpScore() { eid = userInfo.eId, expScore = score };
            string scoreData = LitJson.JsonMapper.ToJson(send);
            string url = host + "outer/intelligent/!expScoreSave";
            mono.StartCoroutine(ExUnityWebRequest.WebRequestFrom(url, new[] { "param" },
                new[] { scoreData }, false, false, () => { failureCallBack?.Invoke(); }, (result) =>
                {
                    DTO_RAINER.StatusInfo statusInfo = LitJson.JsonMapper.ToObject<DTO_RAINER.StatusInfo>(result);
                    successCallBack?.Invoke(statusInfo);
                }));
        }


        /// <summary>
        /// 实验报告对接接口（自定义程度高，需要按需修改）
        /// 需要在外部进行List<Text_chan>()的初始化工作，并赋值给对应的text,如text不足，则手动添加即可
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="failureCallBack"></param>
        /// <param name="successCallBack"></param>
        public static void SendReportInfo(this MonoBehaviour mono, Action<DTO_RAINER.ReportInfo> InitReportInfo, Action failureCallBack,
            Action<DTO_RAINER.StatusInfo> successCallBack)
        {
            DTO_RAINER.ReportInfo report = new DTO_RAINER.ReportInfo() { eid = userInfo.eId };
            InitReportInfo.Invoke(report);
            // report.text1 = new List<Text_chan>(); //
            // Text_chan text = new Text_chan();
            // text.text = 传值.instance.简答题题干.text;
            // text.color = "red";
            // report.text1.Add(text);
            //
            //
            // report.text2 = new List<Text_chan>(); //
            // text = new Text_chan();
            // text.text = 传值.instance.简答题记录;
            // text.color = "blue";
            // report.text2.Add(text);
            //
            // report.text3 = new List<Text_chan>(); //
            // text = new Text_chan();
            // text.text = 传值.instance.实验评价记录; //***********
            // text.color = "blue";
            // report.text3.Add(text);
            string reportData = LitJson.JsonMapper.ToJson(report);
            string url = host + "outer/report/!reportEdit";
            mono.StartCoroutine(ExUnityWebRequest.WebRequestFrom(url, new[] { "param" }, new[] { reportData },
                false, false, () => { failureCallBack?.Invoke(); }, (result) =>
                {
                    DTO_RAINER.StatusInfo statusInfo = LitJson.JsonMapper.ToObject<DTO_RAINER.StatusInfo>(result);
                    successCallBack?.Invoke(statusInfo);
                }));
        }
    }

}