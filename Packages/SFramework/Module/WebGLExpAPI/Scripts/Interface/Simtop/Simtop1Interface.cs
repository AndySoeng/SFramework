using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Ex;
using LitJson;
using UnityEngine;
using UnityEngine.Events;
using WebGLExpInterface.DTO;

namespace WebGLExpInterface
{
    /// <summary>
    /// 根据康亚威20240131提供的文档进行实现的提交接口
    /// </summary>
    public static class Simtop1Interface
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string getUrlParams(string name);
#endif
        private static string access_token;

        private static string aesKey = "TH5J2B6I6H6SVV7I";

        private static bool init = false;

        public static void Init()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                access_token = getUrlParams("access_token");
                SplitAccessToken(ExCrypto_AES.Decrypt(access_token, CipherMode.ECB, PaddingMode.PKCS7, aesKey));
                init = true;
            }
            catch (Exception e)
            {
                Debug.LogError("获取实验信息出错，请联系管理员。");
                init = false;
            }
# else
            //编辑器测试用
            access_token =
                "VPP1jIlZcfw8Yrv4mefUm/DePojRLLiyqqvuVkOk9pgPChRBHrVyse0W1y4lAsg9EVo01Wj1d7MxxTgTdepQncKs9+4VwwPC9DTQk7eBbTOD4MyhpDg8iVUbn/EF1hfR3r9rowPuPr9CbjfZVuUtbxKsRrVCsFPRUoawcG9r9wa7XDgPeXJaBJ1NB+eKTD8Cm+HBfGysHEM7uu8eLwxaqgheMqNs03pgXbKOJ9wah5BxejNrQxBDg3GvRpdMX0TePOdhfwp1IGtaE7UXkzVRwxxwNwuooQSTK1HRdFPeQvzjRLod7yoPIzCPjBf/wFBZQ1ORXarWZA16LT5i7y5pMQVHaf0FgX4IisVc/B66B4hhdg9nNKiKFw20rx8pu55el9ZafZV2McV5ZJGJqnrHBGiApOS9D1c3qs0a1REUML267hBRpRCdtAzANzx2qy0iaDGOsIFDyssajdDtP2cxZmGv4P7mjmRz0ww527WEB8zncvilthEUaRbrCZlrvY2OX/HzpzdDMwvGrfS88UAMVPnf7TxohKxYoiMCoMhbHkzeMSrj0ZJa4mu6P1GkH6HntYM7OyBuZcn5A9ir0KIYon42A35wXzlT4+kerZEuaISbGSRFEZuiomz5u9VhCZeL";
            SplitAccessToken(ExCrypto_AES.Decrypt(access_token, CipherMode.ECB, PaddingMode.PKCS7, aesKey));
            init = true;
#endif
        }

        // AccessToken解密后：
        // token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzY2hvb2wiOiLmtZnmsZ_moJHkurrlpKflraYiLCJ1cGRhdGVQYXNzd29yZFRpbWUiOm51bGwsInJvbGVJZCI6Mywib3BlbklkIjpudWxsLCJsb2dpbk5hbWUiOiJwc3poMTcwNjc2ODk3MTYzMSIsImlkIjoxNDAsImFjY2Vzc1Rva2VuIjpudWxsLCJleHAiOjE3MDY3NzYxNzE2NjMsInVzZXJuYW1lIjoi6K-E5a6h6LSm5oi3In0.mT6zXl-0EJPPhNRSCa4NUESYg4aec-SSbIcFnW5PToo;
        // salt=FW2TAMAV;
        // loginName=pszh1706768971631;
        // username=评审账户;
        // proveid=4590;
        // host=http://192.168.0.144:8105/Field/experiment/SubmitGrades

        private static Dictionary<string, string> ACCESSTOKEN = new Dictionary<string, string>();

        private static void SplitAccessToken(string decryptAccessToken)
        {
            string[] accessTokenArray = decryptAccessToken.Split(';');
            for (int i = 0; i < accessTokenArray.Length; i++)
            {
                int firstIndex = accessTokenArray[i].IndexOf('=');
                string key = accessTokenArray[i].Substring(0, firstIndex);
                string value = accessTokenArray[i].Substring(firstIndex + 1, accessTokenArray[i].Length - firstIndex - 1);
                ACCESSTOKEN.Add(key, value);
            }
        }


        public static void DataUpload(this MonoBehaviour mono, DTO_Simtop1 dtoSimtop1, UnityAction failureCallBack, UnityAction successCallBack)
        {
            if (init == false)
            {
                Debug.LogError("未初始化成功。");
                return;
            }

            dtoSimtop1.serialNumber = ExCrypto_MD5.Md5Encrypt(ACCESSTOKEN["salt"] + "_" + ACCESSTOKEN["proveid"].ToUpper());
            dtoSimtop1.salt = ACCESSTOKEN["salt"];


            mono.StartCoroutine(ExUnityWebRequest.WebRequest(ExUnityWebRequest.UnityWebRequestType.POST, ACCESSTOKEN["host"], JsonMapper.ToJson(dtoSimtop1), false,
                false, () => { failureCallBack?.Invoke(); }, (str) =>
                {
                    DTO_Simtop1.DataUploadRespon dataUploadRespon = JsonMapper.ToObject<DTO_Simtop1.DataUploadRespon>(str);
                    if (dataUploadRespon.status != 0)
                    {
                        Debug.Log($"错误返回,code={dataUploadRespon.status},data={dataUploadRespon.data}。");
                    }
                    else
                    {
                        successCallBack?.Invoke();
                    }
                }, new[] { "Authorization" }, new[] { "Bearer " + ACCESSTOKEN["token"] }));
        }
    }
}