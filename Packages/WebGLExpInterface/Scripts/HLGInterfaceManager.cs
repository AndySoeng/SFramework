using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ExpInterface
{
    public static class HLGInterfaceManager
    {
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);

        public static UserInfo userInfo;

        /// <summary>
        /// 测试平台标记
        /// </summary>
        private  static bool IsTestingPlatform = false;
        
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
        public static void HLGInterface_GetUserInfo(this MonoBehaviour mono, Action failureCallBack,
            Action<UserInfo> successCallBack)
        {
            string token = getUrlParams("token");

            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("token不存在");
                failureCallBack?.Invoke();
                return;
            }

            Token tokenData = new Token();
            tokenData.token = token;
            string jsonData = LitJson.JsonMapper.ToJson(tokenData);
            string url = host + "outer/getMessageByToken";
            mono.StartCoroutine(ExpInterfaceBase.WebRequestFrom(url, new[] {"param"}, new[] {jsonData},
                true, true, () =>
                {
                    failureCallBack?.Invoke();
                }, (result) =>
                {
                    userInfo = LitJson.JsonMapper.ToObject<UserInfo>(result);
                    
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
        public static void HLGInterface_SendExpScore(this MonoBehaviour mono, string score, Action failureCallBack,
            Action<StatusInfo> successCallBack)
        {
            SendExpScore send = new SendExpScore() {eid = userInfo.eId, expScore = score};
            string scoreData = LitJson.JsonMapper.ToJson(send);
            string url = host + "outer/intelligent/!expScoreSave";
            mono.StartCoroutine(ExpInterfaceBase.WebRequestFrom(url, new[] {"param"},
                new[] {scoreData}, false, false, () =>
                {
                    failureCallBack?.Invoke();
                }, (result) =>
                {
                    StatusInfo statusInfo = LitJson.JsonMapper.ToObject<StatusInfo>(result);
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
        public static void HLGInterface_SendReportInfo(this MonoBehaviour mono,Action<ReportInfo> InitReportInfo, Action failureCallBack,
            Action<StatusInfo> successCallBack)
        {
            ReportInfo report = new ReportInfo() {eid = userInfo.eId};
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
            mono.StartCoroutine(ExpInterfaceBase.WebRequestFrom(url, new[] {"param"}, new[] {reportData},
                false, false, () =>
                {
                    failureCallBack?.Invoke();
                }, (result) =>
                {
                    StatusInfo statusInfo = LitJson.JsonMapper.ToObject<StatusInfo>(result);
                    successCallBack?.Invoke(statusInfo);
                }));
        }



    }


   

    public class UserInfo
    {
        public string status { get; set; }
        public string eId { get; set; }
        public string userId { get; set; }
        public string numberId { get; set; }
        public string name { get; set; }
        public string groupName { get; set; }
        public string host { get; set; }
        public string role { get; set; }

        public string statusMessage { get; set; }
        public override string ToString()
        {
            return status + "\t" + eId + "\t" + userId + "\t" + numberId + "\t" + name + "\t" + groupName + "\t" +
                   host + "\t" + role+ "\t" + statusMessage;
        }
    }

//{"status":"000",
//"eId":"8a8091397217a91a01722705b0b80fb4",
//"userId":"ff8080817048d4830170624769800cde",
//"numberId":"20202020",
//"groupName":"无",
//"name":"333",
//"host":"http://zhang.xb.owvlab.net/virexp",
//"role":"student"}


    public class Token
    {
        public string token { get; set; }
    }


    public class SendExpScore
    {
        public string eid { get; set; }
        public string expScore { get; set; }
    }

    public class StatusInfo
    {
        // 000	成功
        // 101	数据库异常
        // 其他	系统错误
        public string status { get; set; }
        public string statusMessage { get; set; }


        public override string ToString()
        {
            return status + "\t" + statusMessage;
        }
    }


    public class ReportInfo
    {
        public string eid { get; set; }
        public List<Text_chan> text1 { get; set; }
        public List<Text_chan> text2 { get; set; }
        public List<Text_chan> text3 { get; set; }


        public override string ToString()
        {
            return text1[0].text + "\t" + text1[0].color + "\t" + text2[0].text + "\t" + text2[0].color + "\t" +
                   text3[0].text + "\t" + text3[0].color;
        }
    }

    public class Text_chan
    {
        public string text { get; set; }
        public string color { get; set; }
        public bool enabled { get; internal set; }
        public Font font { get; internal set; }
        public int fontSize { get; internal set; }
        public TextAnchor alignment { get; internal set; }
        public int preferredWidth { get; internal set; }
    }
}