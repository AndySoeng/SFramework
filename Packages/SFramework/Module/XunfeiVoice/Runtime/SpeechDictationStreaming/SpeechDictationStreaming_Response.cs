using System.Text;

namespace XunfeiVoice.Runtime
{
    public class SpeechDictationStreaming_Response
    {
        /// <summary>
        /// 本次会话的id，只在握手成功后第一帧请求时返回
        /// </summary>
        public string sid { get; set; }

        /// <summary>
        /// 返回码，0表示成功，其它表示异常，详情请参考错误码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 听写结果信息
        /// </summary>
        public SpeechDictationStreaming_ResponseData data;
    }

    /// <summary>
    /// 听写结果信息
    /// </summary>
    public class SpeechDictationStreaming_ResponseData
    {
        /// <summary>
        /// 识别结果是否结束标识：
        /// 0：识别的第一块结果
        /// 1：识别中间结果
        /// 2：识别最后一块结果
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 听写识别结果
        /// </summary>
        public SpeechDictationStreaming_ResponseResult result;
    }

    /// <summary>
    /// 听写识别结果
    /// </summary>
    public class SpeechDictationStreaming_ResponseResult
    {
        /// <summary>
        /// 返回结果的序号
        /// </summary>
        public int sn { get; set; }

        /// <summary>
        /// 是否是最后一片结果
        /// </summary>
        public bool ls { get; set; }

        /// <summary>
        /// 保留字段，无需关心
        /// </summary>
        public int bg { get; set; }

        /// <summary>
        /// 保留字段，无需关心
        /// </summary>
        public int ed { get; set; }

        /// <summary>
        /// 听写结果
        /// </summary>
        public ResponseWS[] ws;


        #region 动态修正返回参数

        //若开通了动态修正功能并设置了dwa=wpgs（仅中文支持），还有如下字段返回：
        //注：动态修正结果解析可参考页面下方的java demo。

        /// <summary>
        /// 开启wpgs会有此字段
        /// 取值为 "apd"时表示该片结果是追加到前面的最终结果；取值为"rpl" 时表示替换前面的部分结果，替换范围为rg字段
        /// </summary>
        public string pgs { get; set; }


        /// <summary>
        /// 替换范围，开启wpgs会有此字段
        /// 假设值为[2,5]，则代表要替换的是第2次到第5次返回的结果
        /// </summary>
        public int[] rg;

        #endregion


        #region vinfo返回参数

        //若设置了vinfo=1，还有如下字段返回（若同时开通并设置了dwa=wpgs，则vinfo失效）：

        /// <summary>
        /// 端点帧偏移值信息
        /// </summary>
        public SpeechDictationStreaming_ResponseVAD vad;

        #endregion

        public SpeechDictationStreaming_ResponseResultDecoder GetResultDecoder()
        {
            SpeechDictationStreaming_ResponseResultDecoder speechDictationStreamingResponseResultDecoder = new SpeechDictationStreaming_ResponseResultDecoder();
            StringBuilder sb = new StringBuilder();

            foreach (ResponseWS ws in ws)
            {
                sb.Append(ws.cw[0].w);
            }

            speechDictationStreamingResponseResultDecoder.sn = this.sn;
            speechDictationStreamingResponseResultDecoder.text = sb.ToString();
            speechDictationStreamingResponseResultDecoder.sn = this.sn;
            speechDictationStreamingResponseResultDecoder.rg = this.rg;
            speechDictationStreamingResponseResultDecoder.pgs = this.pgs;
            speechDictationStreamingResponseResultDecoder.bg = this.bg;
            speechDictationStreamingResponseResultDecoder.ed = this.ed;
            speechDictationStreamingResponseResultDecoder.ls = this.ls;
            speechDictationStreamingResponseResultDecoder.vad = this.vad == null ? null : this.vad;
            return speechDictationStreamingResponseResultDecoder;
        }
    }


    /// <summary>
    /// 端点帧偏移值结果
    /// </summary>
    public class ResponseWS
    {
        /// <summary>
        /// 起始的端点帧偏移值，单位：帧（1帧=10ms）
        /// 注：以下两种情况下bg=0，无参考意义：
        /// 1)返回结果为标点符号或者为空；2)本次返回结果过长。
        /// </summary>
        public int bg { get; set; }

        /// <summary>
        /// 中文分词
        /// </summary>
        public SpeechDictationStreaming_ResponseCW[] cw;
    }

    /// <summary>
    /// 中文分词
    /// </summary>
    public class SpeechDictationStreaming_ResponseCW
    {
        /// <summary>
        /// 字词
        /// </summary>
        public string w { get; set; }

        //其他字段均为保留字段，无需关心
    }

    /// <summary>
    /// 端点帧偏移值信息
    /// </summary>
    public class SpeechDictationStreaming_ResponseVAD
    {
        /// <summary>
        /// 端点帧偏移值结果
        /// </summary>
        public ResponseWS[] ws;

        /// <summary>
        /// 起始的端点帧偏移值，单位：帧（1帧=10ms）
        /// </summary>
        public int bg { get; set; }

        /// <summary>
        /// 结束的端点帧偏移值，单位：帧（1帧=10ms）
        /// </summary>
        public int ed { get; set; }
    }
}