namespace XunfeiVoice.Runtime.Common
{
    public class XunfeiVoiceCommon
    {
        #region 接口鉴权参数

        public static readonly string HOSTURL_SpeechDictationStreaming = "wss://iat-api.xfyun.cn/v2/iat";

        public static readonly string HOSTURL_SpeechSynthesisStreaming = "wss://tts-api.xfyun.cn/v2/tts";

        public static readonly string ALGORITHM = "hmac-sha256";

        public static readonly string HEADERS = "host date request-line";

        public static readonly string APPID = "f1ffa0c7";

        public static readonly string APISECRET = "ZTQzZWJkNzk1YzU2Nzc5NDgzOWMxNGVm";

        public static readonly string APIKEY = "a2a6a3af40fac20f156a97bc1ec43530";

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