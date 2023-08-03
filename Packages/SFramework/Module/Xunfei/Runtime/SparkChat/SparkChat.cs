using System;
using System.Collections.Generic;
using Ex;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xunfei.Runtime.Common;
using WebSocket = BestHTTP.WebSocket.WebSocket;

namespace Xunfei.Runtime
{
    /// <summary>
    /// 星火认知大模型 WebAPI 接口调用示例 接口文档（必看）：https://www.xfyun.cn/doc/spark/Web.htm
    /// 错误码链接：https://www.xfyun.cn/doc/spark/%E6%8E%A5%E5%8F%A3%E8%AF%B4%E6%98%8E.html （code返回错误码时必看）
    /// </summary>
    public class SparkChat
    {
        /// <summary>
        /// Saved WebSocket instance
        /// </summary>
        WebSocket webSocket;


        private string uid;
        private event Action<string> OnOnceResult;
        private List<SparkChat_Dto.Content> historyContent = new List<SparkChat_Dto.Content>();


        // Start is called before the first frame update
        public SparkChat(string uid, Action<string> onceResult = null)
        {
            if (uid.Length > 32)
            {
                Debug.LogError("uid长度不能超过32位");
            }

            if (onceResult != null)
                OnOnceResult += onceResult;
            OnConnectButton();
        }


        private void OnConnectButton()
        {
            // Create the WebSocket instance
            this.webSocket = new WebSocket(new Uri(ExXunFei.AssembleAuthUrl(XunfeiCommon.HOSTURL_SparkChat,
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
            SendChatImp();
        }

        public void SendChat(string userMessage)
        {
            historyContent.Add(new SparkChat_Dto.Content() { role = "user", content = userMessage });
            if (webSocket == null)
            {
                OnConnectButton();
            }
        }

        private void SendChatImp()
        {
            SparkChat_Dto.JsonRequest request = new SparkChat_Dto.JsonRequest();
            request.header = new SparkChat_Dto.Header()
            {
                app_id = XunfeiCommon.APPID,
                uid = "12345"
            };
            request.parameter = new SparkChat_Dto.Parameter()
            {
                chat = new SparkChat_Dto.Chat()
                {
                    domain = "general", //模型领域，默认为星火通用大模型
                    temperature = 0.5, //温度采样阈值，用于控制生成内容的随机性和多样性，值越大多样性越高；范围（0，1）
                    max_tokens = 1024, //生成内容的最大长度，范围（0，4096）
                }
            };
            request.payload = new SparkChat_Dto.Payload()
            {
                message = new SparkChat_Dto.Message()
                {
                    text = historyContent,
                }
            };

            string jsonString = JsonConvert.SerializeObject(request);
            //连接成功，开始发送数据


            byte[] frameData2 = System.Text.Encoding.UTF8.GetBytes(jsonString);


            webSocket.Send(frameData2);


            oneceResult = String.Empty;
        }


        /// <summary>
        /// Called when we received a text message from the server
        /// </summary>
        void OnMessageReceived(WebSocket ws, string message)
        {
            //Debug.Log(string.Format("Message received: <color=yellow>{0}</color>", message));
            HandleResponse(message);
        }

        private string oneceResult;

        private void HandleResponse(string message)
        {
            string receivedMessage = message;
            //将结果构造为json

            JObject jsonObj = JObject.Parse(receivedMessage);
            int code = (int)jsonObj["header"]["code"];
            
            // 返回code为错误码时，请查询https://www.xfyun.cn/document/error-code解决方案
            if (0 == code)
            {
                int status = (int)jsonObj["payload"]["choices"]["status"];


                JArray textArray = (JArray)jsonObj["payload"]["choices"]["text"];
                string content = (string)textArray[0]["content"];
                oneceResult += content;

                if (status != 2)
                {
                    //Debug.Log($"已接收到数据： {receivedMessage}");
                }
                else
                {
                    //Debug.Log($"最后一帧： {receivedMessage}");
                    int totalTokens = (int)jsonObj["payload"]["usage"]["text"]["total_tokens"];
                    string sid = (string)jsonObj["header"]["sid"];
                    Debug.Log("Session end .本次Chat sid：" + sid +
                              $"\n整体返回结果： {oneceResult}" +
                              $"\n本次消耗token数： {totalTokens}");
                    historyContent.Add(new SparkChat_Dto.Content() { role = "assistant", content = oneceResult });
                    OnOnceResult?.Invoke(oneceResult);
                    oneceResult = string.Empty;
                }
            }
            else
            {
                Debug.Log($"请求报错： {receivedMessage}");
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