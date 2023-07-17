using UnityEngine;

namespace XunfeiVoice.Runtime
{
    public class SpeechDictationStreaming_Error
    {
        public static void LogErrorByCode(int errorCode)
        {
            switch (errorCode)
            {
                case 10005:
                    Debug.LogError("错误描述：licc fail	appid .\n" +
                                   "说明：授权失败。\n" +
                                   "处理方式：确认appid是否正确，是否开通了听写服务。");
                    break;
                case 10006:
                    Debug.LogError("错误描述：Get audio rate fail .\n" +
                                   "说明：获取某个参数失败。\n" +
                                   "处理方式：检查报错信息中的参数是否正确上传。");
                    break;
                case 10007:
                    Debug.LogError("错误描述：get invalid rate .\n" +
                                   "说明：参数值不合法。\n" +
                                   "处理方式：检查报错信息中的参数值是否在取值范围内。");
                    break;
                case 10010:
                    Debug.LogError("错误描述：AIGES_ERROR_NO_LICENSE .\n" +
                                   "说明：引擎授权不足。\n" +
                                   "处理方式：请到控制台提交工单联系技术人员。");
                    break;
                case 10014:
                    Debug.LogError("错误描述：AIGES_ERROR_TIME_OUT .\n" +
                                   "说明：会话超时。");
                    break;
                case 10019:
                    Debug.LogError("错误描述：service read buffer timeout, session timeout .\n" +
                                   "说明：session超时。\n" +
                                   "处理方式：检查是否数据发送完毕但未关闭连接。");
                    break;
                case 10043:
                    Debug.LogError("错误描述：Syscall AudioCodingDecode error .\n" +
                                   "说明：音频解码失败。\n" +
                                   "处理方式：检查aue参数，如果为speex，请确保音频是speex音频并分段压缩且与帧大小一致。");
                    break;
                case 10101:
                    Debug.LogError("错误描述：engine inavtive .\n" +
                                   "说明：引擎会话已结束。\n" +
                                   "处理方式：检查是否引擎已结束会话但客户端还在发送数据，比如音频数据虽然发送完毕但并未关闭websocket连接，还在发送空的音频等。");
                    break;
                case 10114:
                    Debug.LogError("错误描述：session timeout .\n" +
                                   "说明：会话超时。\n" +
                                   "处理方式：检查整个会话是否已经超过了60s。");
                    break;
                case 10139:
                    Debug.LogError("错误描述：invalid param .\n" +
                                   "说明：参数错误。\n" +
                                   "处理方式：引擎编解码错误。");
                    break;
                case 10313:
                    Debug.LogError("错误描述：appid cannot be empty .\n" +
                                   "说明：appid不能为空。\n" +
                                   "处理方式：检查common参数是否正确上传，或common中的app_id参数是否正确上传或是否为空。");
                    break;
                case 10317:
                    Debug.LogError("错误描述：invalid version .\n" +
                                   "说明：版本非法。\n" +
                                   "处理方式：联系技术人员。");
                    break;
                case 11200:
                    Debug.LogError("错误描述：auth no license .\n" +
                                   "说明：没有权限。\n" +
                                   "处理方式：检查是否使用了未授权的功能，或者总的调用次数已超越上限。");
                    break;
                case 11201:
                    Debug.LogError("错误描述：auth no enough license .\n" +
                                   "说明：日流控超限。\n" +
                                   "处理方式：可联系商务提高每日调用次数。");
                    break;
                case 10160:
                    Debug.LogError("错误描述：parse request json error .\n" +
                                   "说明：请求数据格式非法。\n" +
                                   "处理方式：检查请求数据是否是合法的json。");
                    break;
                case 10161:
                    Debug.LogError("错误描述：parse base64 string error .\n" +
                                   "说明：base64解码失败。\n" +
                                   "处理方式：检查发送的数据是否使用了base64编码。");
                    break;
                case 10163:
                    Debug.LogError("错误描述：param validate error:/common 'app_id' param is required .\n" +
                                   "说明：缺少必传参数，或者参数不合法。\n" +
                                   "处理方式：检查报错信息中的参数是否正确上传。");
                    break;
                case 10165:
                    Debug.LogError("错误描述：invalid handle .\n" +
                                   "说明：无效的句柄。\n" +
                                   "处理方式：检查下传入第一帧音频时，是否上传了status=0。");
                    break;
                case 10200:
                    Debug.LogError("错误描述：read data timeout .\n" +
                                   "说明：读取数据超时。\n" +
                                   "处理方式：检查是否累计10s未发送数据并且未关闭连接。");
                    break;
                default:
                    break;
            }
        }
    }
}