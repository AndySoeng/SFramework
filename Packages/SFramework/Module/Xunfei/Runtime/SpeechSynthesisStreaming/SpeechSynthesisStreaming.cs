using System;
using System.Collections.Generic;
using BestHTTP.WebSocket;
using Ex;
using Newtonsoft.Json;
using UnityEngine;
using Xunfei.Runtime.Common;

namespace Xunfei.Runtime
{
    public enum VCNName
    {
        xiaoyan,
        aisjiuxu,
        aisxping,
        aisjinger,
        aisbabyxu,
    }

    /// <summary>
    /// 语音合成流式接口将文字信息转化为声音信息，同时提供了众多极具特色的发音人（音库）供您选择，base64编码前最大长度需小于8000字节，约2000汉字。
    /// 
    /// 语音合成流式 WebAPI 接口调用示例 接口文档（必看）：https://www.xfyun.cn/doc/tts/online_tts/API.html
    /// 发音人使用方式：登陆开放平台https://www.xfyun.cn/后，到控制台-我的应用-语音合成-添加试用或购买发音人，添加后即显示该发音人参数值
    /// 错误码链接：https://www.xfyun.cn/document/error-code （code返回错误码时必看）
    /// 小语种需要传输小语种文本、使用小语种发音人vcn、tte=unicode以及修改文本编码方式
    /// </summary>
    public class SpeechSynthesisStreaming
    {


        /// <summary>
        /// Saved WebSocket instance
        /// </summary>
        WebSocket webSocket;


// 合成文本编码格式
        private string TTE = "UTF8"; // 小语种必须使用UNICODE编码作为值

        // 发音人参数。到控制台-我的应用-语音合成-添加试用或购买发音人，添加后即显示该发音人参数值，若试用未添加的发音人会报错11200
        private string VCN = "xiaoyan";

        private string synthesisText;

        private event Action<byte[]> OnResult;

        private List<byte> allReceivedData = new List<byte>();


        // Start is called before the first frame update
        public SpeechSynthesisStreaming(string synthesisText, Action<byte[]> result, VCNName vcnName = VCNName.xiaoyan)
        {
            allReceivedData.Clear();
            this.synthesisText = synthesisText;
            if (result != null)
                OnResult += result;
            this.VCN = vcnName.ToString();
            OnConnectButton();
        }


        private void OnConnectButton()
        {
            // Create the WebSocket instance
            this.webSocket = new WebSocket(new Uri(ExXunFei.AssembleAuthUrl(XunfeiCommon.HOSTURL_SpeechSynthesisStreaming,
                XunfeiCommon.APISECRET,
                XunfeiCommon.APIKEY,
                XunfeiCommon.ALGORITHM,
                XunfeiCommon.HEADERS)));

#if !UNITY_WEBGL || UNITY_EDITOR
            this.webSocket.StartPingThread = true;
#endif

            // Subscribe to the WS events
            this.webSocket.OnOpen += OnOpen;
            this.webSocket.OnMessage += OnMessageReceived;
            this.webSocket.OnClosed += OnClosed;
            this.webSocket.OnError += OnError;

            // Start connecting to the server
            this.webSocket.Open();

            //Debug.Log("Connecting...");
        }


        /// <summary>
        /// Called when the web socket is open, and we are ready to send and receive data
        /// </summary>
        void OnOpen(WebSocket ws)
        {
            //Debug.Log("WebSocket Open!");
            StartSendAudioData(synthesisText);
        }

        private void StartSendAudioData(string text)
        {
            SpeechSynthesisStreaming_Request request = new SpeechSynthesisStreaming_Request()
            {
                common = new RequestCommon()
                {
                    app_id = XunfeiCommon.APPID,
                },
                business = new SpeechSynthesisStreaming_RequestBusiness()
                {
                    aue = "raw",
                    sfl = 0,
                    auf = "audio/L16;rate=16000",
                    vcn = VCN,
                    speed = 50,
                    volume = 50,
                    pitch = 50,
                    bgs = 0,
                    tte = TTE,
                    reg = "0",
                    rdn = "0",

                },
                data = new SpeechSynthesisStreaming_RequestData()
                {
                    status = 2,
                    text = ExCrypto_Base64.Base64Encrypt(text),
                }
            };
            webSocket.Send(JsonConvert.SerializeObject(request));
        }

        /// <summary>
        /// Called when we received a text message from the server
        /// </summary>
        void OnMessageReceived(WebSocket ws, string message)
        {
            //Debug.Log(string.Format("Message received: <color=yellow>{0}</color>", message));
            HandleResponse(message);
        }


        private void HandleResponse(string message)
        {
            SpeechSynthesisStreaming_Response resp = JsonConvert.DeserializeObject<SpeechSynthesisStreaming_Response>(message);

            if (resp.code != 0)
            {
                Debug.LogError("code=>" + resp.code + " error=>" + resp.message + " sid=" + resp.sid + "\n错误码查询链接：https://www.xfyun.cn/document/error-code");
                SpeechSynthesisStreaming_Error.LogErrorByCode(resp.code);
            }

            if (resp.data != null)
            {
                byte[] textBase64Decode = Convert.FromBase64String(resp.data.audio);
                allReceivedData.AddRange(textBase64Decode);
                if (resp.data.status == 2)
                {
                    OnResult?.Invoke(allReceivedData.ToArray());
                    Debug.Log("Session end .本次合成sid：" + resp.sid);
                    // 可以关闭连接，释放资源
                    webSocket.Close();
                }
            }
        }

        /// <summary>
        /// Called when the web socket closed
        /// </summary>
        void OnClosed(WebSocket ws, UInt16 code, string message)
        {
            Debug.Log(string.Format("WebSocket closed! Code: {0} Message: {1}", code, message));

            webSocket = null;
        }

        /// <summary>
        /// Called when an error occured on client side
        /// </summary>
        void OnError(WebSocket ws, string error)
        {
            Debug.Log(string.Format("An error occured: <color=red>{0}</color>", error));

            webSocket = null;
        }

    }
}