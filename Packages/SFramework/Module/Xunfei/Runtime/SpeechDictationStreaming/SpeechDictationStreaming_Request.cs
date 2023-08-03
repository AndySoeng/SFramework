using Xunfei.Runtime.Common;

namespace Xunfei.Runtime
{
    public class SpeechDictationStreaming_Request
    {
        public RequestCommon common;
        public SpeechDictationStreaming_RequestBusiness business;
        public SpeechDictationStreaming_RequestData data;
    }


    public class SpeechDictationStreaming_RequestBusiness
    {
        /// <summary>
        /// 语种  必传
        /// zh_cn：中文（支持简单的英文识别）
        /// en_us：英文
        /// 其他小语种：可到控制台-语音听写（流式版）-方言/语种处添加试用或购买，添加后会显示该小语种参数值，若未授权无法使用会报错11200。
        /// 另外，小语种接口URL与中英文不同，详见接口要求。
        /// </summary>
        public string language { get; set; }

        /// <summary>
        /// 应用领域  必传
        /// iat：日常用语
        /// medical：医疗
        /// gov-seat-assistant：政务坐席助手
        /// seat-assistant：金融坐席助手
        /// gov-ansys：政务语音分析
        /// gov-nav：政务语音导航
        /// fin-nav：金融语音导航
        /// fin-ansys：金融语音分析
        /// 注：除日常用语领域外其他领域若未授权无法使用，可到控制台-语音听写（流式版）-高级功能处添加试用或购买；若未授权无法使用会报错11200。
        /// 坐席助手、语音导航、语音分析相关垂直领域仅适用于8k采样率的音频数据，另外三者的区别详见下方。
        /// </summary>
        public string domain { get; set; }

        /// <summary>
        /// 方言，当前仅在language为中文时，支持方言选择。  必传
        /// mandarin：中文普通话、其他语种
        /// 其他方言：可到控制台-语音听写（流式版）-方言/语种处添加试用或购买，添加后会显示该方言参数值；方言若未授权无法使用会报错11200。
        /// </summary>
        public string accent { get; set; }

        /// <summary>
        /// 用于设置端点检测的静默时间，单位是毫秒。
        /// 即静默多长时间后引擎认为音频结束。
        /// 默认2000（小语种除外，小语种不设置该参数默认为未开启VAD）。
        /// </summary>
        public int vad_eos { get; set; }

        /// <summary>
        /// （仅中文普通话支持）动态修正
        /// wpgs：开启流式结果返回功能
        /// 注：该扩展功能若未授权无法使用，可到控制台-语音听写（流式版）-高级功能处免费开通；若未授权状态下设置该参数并不会报错，但不会生效。
        /// </summary>
        public string dwa { get; set; }

        /// <summary>
        /// 	（仅中文支持）领域个性化参数
        /// game：游戏
        /// health：健康
        /// shopping：购物
        /// trip：旅行
        /// 注：该扩展功能若未授权无法使用，可到控制台-语音听写（流式版）-高级功能处添加试用或购买；若未授权状态下设置该参数并不会报错，但不会生效。
        /// </summary>
        public string pd { get; set; }

        /// <summary>
        /// （仅中文支持）是否开启标点符号添加
        /// 1：开启（默认值）
        /// 0：关闭
        /// </summary>
        public int ptt { get; set; }

        /// <summary>
        /// （仅中文支持）字体
        /// zh-cn :简体中文（默认值）
        /// zh-hk :繁体香港
        /// 注：该繁体功能若未授权无法使用，可到控制台-语音听写（流式版）-高级功能处免费开通；若未授权状态下设置为繁体并不会报错，但不会生效。
        /// </summary>
        public string rlang { get; set; }

        /// <summary>
        /// 返回子句结果对应的起始和结束的端点帧偏移值。端点帧偏移值表示从音频开头起已过去的帧长度。
        /// 0：关闭（默认值）
        /// 1：开启
        /// 开启后返回的结果中会增加data.result.vad字段，详见下方返回结果。
        /// 注：若开通并使用了动态修正功能，则该功能无法使用。
        /// </summary>
        public int vinfo { get; set; }

        /// <summary>
        /// （中文普通话和日语支持）将返回结果的数字格式规则为阿拉伯数字格式，默认开启
        /// 0：关闭
        /// 1：开启
        /// </summary>
        public int nunum { get; set; }

        /// <summary>
        /// speex音频帧长，仅在speex音频时使用
        /// 1 当speex编码为标准开源speex编码时必须指定
        /// 2 当speex编码为讯飞定制speex编码时不要设置
        /// 注：标准开源speex以及讯飞定制SPEEX编码工具请参考这里 speex编码 。
        /// </summary>
        public int speex_size { get; set; }

        /// <summary>
        /// 取值范围[1,5]，通过设置此参数，获取在发音相似时的句子多侯选结果。设置多候选会影响性能，响应时间延迟200ms左右。
        /// 注：该扩展功能若未授权无法使用，可到控制台-语音听写（流式版）-高级功能处免费开通；若未授权状态下设置该参数并不会报错，但不会生效。
        /// </summary>
        public int nbest { get; set; }

        /// <summary>
        /// 	取值范围[1,5]，通过设置此参数，获取在发音相似时的词语多侯选结果。设置多候选会影响性能，响应时间延迟200ms左右。
        /// 注：该扩展功能若未授权无法使用，可到控制台-语音听写（流式版）-高级功能处免费开通；若未授权状态下设置该参数并不会报错，但不会生效。
        /// </summary>
        public int wbest { get; set; }
    }

    public class SpeechDictationStreaming_RequestData
    {
        /// <summary>
        /// 音频的状态  必传
        /// 0 :第一帧音频
        /// 1 :中间的音频
        /// 2 :最后一帧音频，最后一帧必须要发送
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 音频的采样率支持16k和8k  必传
        /// 16k音频：audio/L16;rate=16000
        /// 8k音频：audio/L16;rate=8000
        /// </summary>
        public string format { get; set; }

        /// <summary>
        /// 音频数据格式  必传
        /// raw：原生音频（支持单声道的pcm）
        /// speex：speex压缩后的音频（8k）
        /// speex-wb：speex压缩后的音频（16k）
        /// 请注意压缩前也必须是采样率16k或8k单声道的pcm。
        /// lame：mp3格式（仅中文普通话和英文支持，方言及小语种暂不支持）
        /// 样例音频请参照音频样例
        /// </summary>
        public string encoding { get; set; }

        /// <summary>
        /// 音频内容，采用base64编码  必传
        /// </summary>
        public string audio { get; set; }
    }
}