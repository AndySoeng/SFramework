using System;
using System.Collections;
using System.IO;
using BestHTTP.WebSocket;
using Ex;
using Newtonsoft.Json;
using UnityEngine;
using Xunfei.Runtime.Common;

namespace Xunfei.Runtime
{
    public enum StreamingStatus
    {
        StatusFirstFrame,
        StatusContinueFrame,
        StatusLastFrame,
    }

    /// <summary>
    /// 语音听写流式接口，用于1分钟内的即时语音转文字技术，支持实时返回识别结果，达到一边上传音频一边获得识别文本的效果。
    ///
    /// 语音听写流式 WebAPI 接口调用示例 接口文档（必看）：https://doc.xfyun.cn/rest_api/语音听写（流式版）.html
    /// webapi 听写服务参考帖子（必看）：http://bbs.xfyun.cn/forum.php?mod=viewthread&tid=38947&extra=
    /// 语音听写流式WebAPI 服务，热词使用方式：登陆开放平台https://www.xfyun.cn/后，找到控制台--我的应用---语音听写---个性化热词，上传热词
    /// 注意：热词只能在识别的时候会增加热词的识别权重，需要注意的是增加相应词条的识别率，但并不是绝对的，具体效果以您测试为准。
    /// 错误码链接：https://www.xfyun.cn/document/error-code （code返回错误码时必看）
    /// 语音听写流式WebAPI 服务，方言或小语种试用方法：登陆开放平台https://www.xfyun.cn/后，在控制台--语音听写（流式）--方言/语种处添加
    /// 添加后会显示该方言/语种的参数值
    /// </summary>
    public class SpeechDictationStreaming
    {
        /// <summary>
        /// Saved WebSocket instance
        /// </summary>
        WebSocket webSocket;

        SpeechDictationStreaming_Decoder decoder = new SpeechDictationStreaming_Decoder();

        // 开始时间
        private DateTime dateBegin = DateTime.Now;

        // 结束时间
        private DateTime dateEnd = DateTime.Now;

        private event Action<string> OnIntermediateResult;
        private event Action<string> OnLastResult;

        private MonoBehaviour mono;
        private byte[] audioData;


        // Start is called before the first frame update
        public SpeechDictationStreaming(MonoBehaviour mono, byte[] audioData, Action<string> intermediateResult = null, Action<string> lastResult = null)
        {
            this.mono = mono;
            this.audioData = audioData;
            if (intermediateResult != null)
                OnIntermediateResult += intermediateResult;
            if (lastResult != null)
                OnLastResult += lastResult;
            OnConnectButton();
        }


        private void OnConnectButton()
        {
            // Create the WebSocket instance
            this.webSocket = new WebSocket(new Uri(ExXunFei.AssembleAuthUrl(XunfeiCommon.HOSTURL_SpeechDictationStreaming,
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
            mono.StartCoroutine(StartSendAudioData());
        }

        private IEnumerator StartSendAudioData()
        {
            //连接成功，开始发送数据
            int frameSize = 1280; //每一帧音频的大小,建议每 40ms 发送 122B
            int intervel = 40;
            int status = 0; // 音频的状态
            MemoryStream ms = new MemoryStream(audioData);
            byte[] buffer = new byte[frameSize];
            while (true)
            {
                int len = ms.Read(buffer);
                //Debug.Log(len);
                if (len == 0)
                {
                    status = (int)StreamingStatus.StatusLastFrame; //文件读完，改变status 为 2
                }

                switch (status)
                {
                    case (int)StreamingStatus.StatusFirstFrame: // 第一帧音频status = 0
                        //Debug.Log("第一帧音频:"+Convert.ToBase64String(ExArray.CopyOf(buffer, len)));
                        SpeechDictationStreaming_Request frame = new SpeechDictationStreaming_Request()
                        {
                            //第一帧必须发送
                            common = new RequestCommon() { app_id = XunfeiCommon.APPID },
                            //第一帧必须发送
                            business = new SpeechDictationStreaming_RequestBusiness()
                            {
                                language = "zh_cn",
                                domain = "iat",
                                accent = "mandarin", //中文方言请在控制台添加试用，添加后即展示相应参数值
                                dwa = "wpgs", //动态修正(若未授权不生效，在控制台可免费开通)
                                ptt = 1,
                            },
                            //每一帧都要发送
                            data = new SpeechDictationStreaming_RequestData()
                            {
                                status = (int)StreamingStatus.StatusFirstFrame,
                                format = "audio/L16;rate=16000",
                                encoding = "raw",
                                audio = Convert.ToBase64String(ExArray.CopyOf(buffer, len)),

                            }
                        };
                        webSocket?.Send(JsonConvert.SerializeObject(frame));
                        status = (int)StreamingStatus.StatusContinueFrame; // 发送完第一帧改变status 为 1
                        break;
                    case (int)StreamingStatus.StatusContinueFrame: //中间帧status = 1
                        //Debug.Log("中间帧:"+Convert.ToBase64String(ExArray.CopyOf(buffer, len)));
                        SpeechDictationStreaming_Request frame1 = new SpeechDictationStreaming_Request()
                        {
                            data = new SpeechDictationStreaming_RequestData()
                            {
                                status = (int)StreamingStatus.StatusContinueFrame,
                                format = "audio/L16;rate=16000",
                                encoding = "raw",
                                audio = Convert.ToBase64String(ExArray.CopyOf(buffer, len)),
                            }
                        };
                        webSocket?.Send(JsonConvert.SerializeObject(frame1));
                        break;
                    case (int)StreamingStatus.StatusLastFrame: // 最后一帧音频status = 2 ，标志音频发送结束
                        SpeechDictationStreaming_Request frame2 = new SpeechDictationStreaming_Request()
                        {
                            data = new SpeechDictationStreaming_RequestData()
                            {
                                status = (int)StreamingStatus.StatusLastFrame,
                                format = "audio/L16;rate=16000",
                                encoding = "raw",
                                audio = "",
                            }
                        };
                        webSocket?.Send(JsonConvert.SerializeObject(frame2));
                        break;
                }

                if (status == (int)StreamingStatus.StatusLastFrame)
                {
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(intervel / 1000f); //模拟音频采样延时
                }
            }
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
            SpeechDictationStreaming_Response resp = JsonConvert.DeserializeObject<SpeechDictationStreaming_Response>(message);

            if (resp == null)
            {
                Debug.LogError("未获取到回复结果。");
                return;
            }

            //Debug.Log("接收到回复");

            if (resp.code != 0)
            {
                Debug.LogError("code=>" + resp.code + " error=>" + resp.message + " sid=" + resp.sid + "\n错误码查询链接：https://www.xfyun.cn/document/error-code");
                SpeechDictationStreaming_Error.LogErrorByCode(resp.code);
                return;
            }
            
            //Debug.Log("code=0,正常");
            
            if (resp.data == null)
            {
                Debug.LogError("回复数据为NULL。");
                return;
            }
            
            //Debug.Log("回复数据不为NULL,正常");
            //Debug.Log("此次status:"+resp.data.status);
            
            if (resp.data.status == 0 || resp.data.status == 1)
                if (resp.data.result != null)
                {
                    //Debug.Log("中间结果不为NULL,正常");

                    SpeechDictationStreaming_ResponseResultDecoder te = resp.data.result.GetResultDecoder();
                    try
                    {
                        decoder.decode(te);
                        OnIntermediateResult?.Invoke(decoder.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message + "\n" + e.StackTrace);
                    }
                }

            if (resp.data.status == 2)
            {
                dateEnd = DateTime.Now;
                webSocket.Close(1000, "");
                Debug.Log("Session end .本次识别sid：" + resp.sid +
                          "\n最终识别结果：" + decoder.ToString() +
                          "\n开始时间：" + dateBegin.ToString("R") +
                          "\n结束时间：" + dateEnd.ToString("R") +
                          "\n耗时:" + new TimeSpan(dateEnd.Ticks - dateBegin.Ticks).TotalMilliseconds + "ms");
                OnLastResult?.Invoke(decoder.ToString());
                decoder.Discard();
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