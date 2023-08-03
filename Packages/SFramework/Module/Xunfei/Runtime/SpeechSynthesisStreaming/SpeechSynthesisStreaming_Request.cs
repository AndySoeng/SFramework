using Xunfei.Runtime.Common;

namespace Xunfei.Runtime
{
    public class SpeechSynthesisStreaming_Request
    {
        public RequestCommon common;
        public SpeechSynthesisStreaming_RequestBusiness business;
        public SpeechSynthesisStreaming_RequestData data;
    }



    public class SpeechSynthesisStreaming_RequestBusiness
    {
        /// <summary>
        /// 音频编码，可选值：
        /// raw：未压缩的pcm
        /// lame：mp3 (当aue=lame时需传参sfl=1)
        /// speex-org-wb;7： 标准开源speex（for speex_wideband，即16k）数字代表指定压缩等级（默认等级为8）
        /// speex-org-nb;7： 标准开源speex（for speex_narrowband，即8k）数字代表指定压缩等级（默认等级为8）
        /// speex;7：压缩格式，压缩等级1~10，默认为7（8k讯飞定制speex）
        /// speex-wb;7：压缩格式，压缩等级1~10，默认为7（16k讯飞定制speex）
        /// </summary>
        public string aue { get; set; }

        /// <summary>
        /// 需要配合aue=lame使用，开启流式返回
        /// mp3格式音频
        /// 取值：1 开启
        /// </summary>
        public int sfl { get; set; }

        /// <summary>
        /// 音频采样率，可选值：
        /// audio/L16;rate=8000：合成8K 的音频
        /// audio/L16;rate=16000：合成16K 的音频
        /// auf不传值：合成16K 的音频
        /// </summary>
        public string auf { get; set; }

        /// <summary>
        /// 发音人，可选值：请到控制台添加试用或购买发音人，添加后即显示发音人参数值
        /// </summary>
        public string vcn { get; set; }

        /// <summary>
        /// 语速，可选值：[0-100]，默认为50
        /// </summary>
        public int speed { get; set; }

        /// <summary>
        /// 音量，可选值：[0-100]，默认为50
        /// </summary>
        public int volume { get; set; }

        /// <summary>
        /// 音高，可选值：[0-100]，默认为50
        /// </summary>
        public int pitch { get; set; }

        /// <summary>
        /// 合成音频的背景音
        /// 0:无背景音（默认值）
        /// 1:有背景音
        /// </summary>
        public int bgs { get; set; }

        /// <summary>
        /// 	文本编码格式
        /// GB2312
        /// GBK
        /// BIG5
        /// UNICODE(小语种必须使用UNICODE编码，合成的文本需使用utf16小端的编码方式，详见java示例demo)
        /// GB18030
        /// UTF8（小语种）
        /// </summary>
        public string tte { get; set; }

        /// <summary>
        /// 设置英文发音方式：
        /// 0：自动判断处理，如果不确定将按照英文词语拼写处理（缺省）
        /// 1：所有英文按字母发音
        /// 2：自动判断处理，如果不确定将按照字母朗读
        /// 默认按英文单词发音
        /// </summary>
        public string reg { get; set; }

        /// <summary>
        /// 合成音频数字发音方式
        /// 0：自动判断（默认值）
        /// 1：完全数值
        /// 2：完全字符串
        /// 3：字符串优先
        /// </summary>
        public string rdn { get; set; }
    }

    public class SpeechSynthesisStreaming_RequestData
    {
        /// <summary>
        /// 文本内容，需进行base64编码；
        /// base64编码前最大长度需小于8000字节，约2000汉字
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 数据状态，固定为2
        /// 注：由于流式合成的文本只能一次性传输，不支持多次分段传输，此处status必须为2。
        /// </summary>
        public int status { get; set; }



    }
}