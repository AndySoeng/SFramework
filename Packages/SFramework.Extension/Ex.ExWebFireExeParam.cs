using System;
using UnityEngine;

namespace Ex
{
    public class ExWebFireExeParam
    {
        /// <summary>
        /// 用来接收HTML发来的数据
        /// Web传入参数样例：triagerescue://host=http://192.168.0.138:8101/psychology/SubmitScore/UpscTriager&token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1aWQiOjEsInVuYW1lIjoi566h55CG5ZGYIiwicm5hbWUiOiJhZG1pbiIsInVhZG1pbiI6ImFkbWluIiwicmlkIjoxLCJleHAiOjE2ODczMTU5MjIyOTgsImFjY2Vzc1Rva2VuIjoiZXlKMGVYQWlPaUpLVjFRaUxDSmhiR2NpT2lKSVV6STFOaUo5LmV5SjFhV1FpT2pFc0luVnVZVzFsSWpvaTU2Nmg1NUNHNVpHWUlpd2ljbTVoYldVaU9pSmhaRzFwYmlJc0luVmhaRzFwYmlJNkltRmtiV2x1SWl3aWNtbGtJam94TENKbGVIQWlPakUyT0Rjek1UVTVNakl5T1RoOS45MGgySEhZZnRUbC1MSEl3SF9oemMzX3NTblRkQUg2QjBmTThncC1zVVlBIn0.Va_ALVf0PXv_Oy3vjUuwJjr-qZa4q5bRuaD5dxc0Gjc
        /// 调用样例：  (bool result,string host ,string token)= ExWebFireExeParam.GetWebParam();
        /// </summary>
        /// <returns>(是否获取到web传参，成绩上传host,成绩使用的token)</returns>
        public static (bool result, string host, string token) GetWebParam()
        {
#if UNITY_EDITOR
            return (false, "", "");
#endif

            //用来接收HTML发来的数据
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length < 2 || commandLineArgs[1] == "")
            {
                Debug.Log("没有接收到参数");
                return (false, "", "");
            }

            //获取命令行传参
            string webParam = commandLineArgs[1];
            //截取拉起命令
            webParam = webParam.Substring(webParam.IndexOf("://") + 3);
            //如果拉起参数最后有'/'则截取
            if (webParam[webParam.Length - 1] == '/')
                webParam = webParam.Substring(0, webParam.Length - 1);

            //截取所需参数
            string host, token = "";
            string[] webParams = webParam.Split('&');
            host = webParams[0].Replace("host=", "");
            if (webParams.Length == 2)
                token = webParams[1].Replace("token=", "");

            return (true, host, token);
        }
    }
}