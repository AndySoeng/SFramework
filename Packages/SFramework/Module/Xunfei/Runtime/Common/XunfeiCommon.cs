namespace Xunfei.Runtime.Common
{
    public class XunfeiCommon
    {
        #region 接口鉴权参数

        public static readonly string HOSTURL_SpeechDictationStreaming = "wss://iat-api.xfyun.cn/v2/iat";

        public static readonly string HOSTURL_SpeechSynthesisStreaming = "wss://tts-api.xfyun.cn/v2/tts";
        
        public static readonly string HOSTURL_SparkChat = "wss://spark-api.xf-yun.com/v2.1/chat";

        public static readonly string ALGORITHM = "hmac-sha256";

        public static readonly string HEADERS = "host date request-line";

        public static readonly string APPID = "";

        public static readonly string APISECRET = "";

        public static readonly string APIKEY = "";

        #endregion
    }

    public class RequestCommon
    {
        /// <summary>
        /// 在平台申请的APPID信息   必传
        /// </summary>
        public string app_id { get; set; }
    }
}