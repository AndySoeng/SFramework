using System;
using UnityEngine;

namespace Ex
{
    public class ExWebFireExeParam
    {
        /// <summary>
        /// 用来接收HTML发来的数据
        /// web传参样例：https://xxx.xxx.xxxx?host=https://www.xxx.com/upscore/upscore&&token=aisrhgiu&*^%*%Tiuhdfiguh
        /// 调用样例：  (bool result,string host ,string token)= ExWebFireExeParam.GetWebParam();
        /// </summary>
        /// <returns>(是否获取到web传参，成绩上传host,成绩使用的token)</returns>
        public static (bool result, string host, string token) GetWebParam()
        {
            //用来接收HTML发来的数据
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length < 2 || commandLineArgs[1] == "")
            {
                Debug.Log("没有接收到参数");
                return (false, "", "");
            }

            string host, token = "";
            string[] webParams = commandLineArgs[1].Split('&');
            host = webParams[0].Replace("host=", "");
            if (webParams.Length == 2)
                token = webParams[1].Replace("token=", "");

            return (true, host, token);
        }
    }
}