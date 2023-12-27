using System.Runtime.InteropServices;
using Ex;
using LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using WebGLExpInterface.DTO;

namespace WebGLExpInterface
{
    /// <summary>
    /// 《国家虚拟仿真实验教学课程技术接口规范（2022版）》（简称2.1接口）
    /// </summary>
    public static class ILabInterface
    {
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);

        #region 固定参数(需配置为对应实验)
        private const int APPID = 0;
        private const string SECRET = "";
        private const string HOST = "http://www.ilab-x.com";
        #endregion

        #region 初始化参数

        private static string access_Token;
        private static string username;

        #endregion

        public static void Init(this MonoBehaviour mono)
        {
#if UNITY_EDITOR
            //复制getUrlParams输出的ticket(已UnEscapeURL解码)
            string ticket =
                "57eBKZVGMzYvuYMiWMbbiQf0/L7DBpFeuuDFPfCFVRKEneT0KGAqX89fejaQ/VF8BX0gQrNVT4ddkg5pXdTey+72oBTLXvnQcYiJ/0YTr1SPi2twwogV261Rfll2unoqY0mtCrLnvewdNjZi8aIacKV7jPlRAMysT0VDwz8HcVcMEVgM76ae8cUXv4cwUQ02";
            //若复制为地址栏的ticket，则解开以下注释
            //UnityWebRequest.UnEscapeURL(ticket);
#else
            string ticket = getUrlParams("ticket");
#endif
            string signature = ExCrypto_MD5.Md5Encrypt(ticket + APPID + SECRET).ToUpper();
            string access_TokenUrl = $"{HOST}/open/api/v2/token?ticket={UnityWebRequest.EscapeURL(ticket)}&appid={APPID.ToString()}&signature={signature}";
            mono.StartCoroutine(ExUnityWebRequest.WebRequest(ExUnityWebRequest.UnityWebRequestType.POST, access_TokenUrl, string.Empty, false, false, null, (arg0 =>
            {
                DTO_ILab.AccessTokenInfo accessTokenInfo = JsonMapper.ToObject<DTO_ILab.AccessTokenInfo>(arg0);
                if (accessTokenInfo.code == 0)
                {
                    access_Token = accessTokenInfo.access_token;
                    username = accessTokenInfo.un;
                }
            }), null, null));
        }

        public static void DataUpload(this MonoBehaviour mono, DTO_ILab dtoILab, UnityAction failureCallBack, UnityAction successCallBack)
        {
            if (string.IsNullOrEmpty(access_Token))
            {
                Debug.Log("未成功初始化接口获取access_Token，直接进行成功回调。");
                successCallBack?.Invoke();
                return;
            }

            dtoILab.appid = APPID.ToString();
            dtoILab.username = username;
            string uploadUrl = $"{HOST}/open/api/v2/data_upload?access_token={UnityWebRequest.EscapeURL(access_Token)}";
            mono.StartCoroutine(ExUnityWebRequest.WebRequest(ExUnityWebRequest.UnityWebRequestType.POST, uploadUrl, JsonMapper.ToJson(dtoILab), false, false,
                () => { failureCallBack?.Invoke(); }, (arg0 =>
                {
                    DTO_ILab.DataUploadRespon dataUploadRespon = JsonMapper.ToObject<DTO_ILab.DataUploadRespon>(arg0);
                    if (dataUploadRespon.code != 0)
                    {
                        Debug.Log($"错误返回,code={dataUploadRespon.code},msg={dataUploadRespon.msg}。");
                    }
                    else
                    {
                        successCallBack?.Invoke();
                    }
                }), null, null));
        }
    }
}