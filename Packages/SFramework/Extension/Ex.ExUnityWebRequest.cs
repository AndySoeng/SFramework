using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


namespace Ex
{
    public static class ExUnityWebRequest
    {
        public enum UnityWebRequestType
        {
            POST,
            GET
        }

        public static IEnumerator WebRequest(UnityWebRequestType type, string url, string jsonData, bool sendBase64,
            bool getBase64, UnityAction failureCallBack, UnityAction<string> successCallBack, string[] otherHeaderName,
            string[] otherHeaderValue)
        {
            //Debug.Log(type.ToString() + "\t" + url + "\t" + jsonData);
            if (sendBase64)
            {
                jsonData = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonData));
            }

            byte[] body = Encoding.UTF8.GetBytes(jsonData);

            using UnityWebRequest unityWeb = new UnityWebRequest(@url, type.ToString());

            unityWeb.uploadHandler = new UploadHandlerRaw(body);
            unityWeb.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            //unityWeb.SetRequestHeader("Authorization", "Bearer " + token);
            if (otherHeaderName != null)
            {
                for (int i = 0; i < otherHeaderName.Length; i++)
                {
                    unityWeb.SetRequestHeader(otherHeaderName[i], otherHeaderValue[i]);
                }
            }

            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            yield return unityWeb.SendWebRequest();

            if (unityWeb.result == UnityWebRequest.Result.ProtocolError|| unityWeb.result == UnityWebRequest.Result.ConnectionError|| unityWeb.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("failure:" + unityWeb.error);
                failureCallBack?.Invoke();
                yield break;
            }

            if (unityWeb.isDone)
            {
                string result = unityWeb.downloadHandler.text;
                if (getBase64)
                {
                    byte[] c = Convert.FromBase64String(result);
                    result = Encoding.UTF8.GetString(c);
                }

                //Debug.Log("success:" + result);
                successCallBack?.Invoke(result);
            }
        }


        public static IEnumerator WebRequestFrom(string url, string[] fromKey, string[] fromValue, bool sendBase64,
            bool getBase64, UnityAction failureCallBack, UnityAction<string> successCallBack)
        {
            WWWForm form = new WWWForm();
            if (fromKey != null)
            {
                for (int i = 0; i < fromKey.Length; i++)
                {
                    if (sendBase64)
                    {
                        fromValue[i] = Convert.ToBase64String(Encoding.UTF8.GetBytes(fromValue[i]));
                    }

                    //Debug.Log("表单添加字段：" + fromKey[i] + "\t" + fromValue[i]);
                    form.AddField(fromKey[i], fromValue[i]);
                }
            }

            using  UnityWebRequest unityWeb = UnityWebRequest.Post(@url, form);

            unityWeb.downloadHandler = new DownloadHandlerBuffer();
            yield return unityWeb.SendWebRequest();

            if (unityWeb.result == UnityWebRequest.Result.ProtocolError|| unityWeb.result == UnityWebRequest.Result.ConnectionError|| unityWeb.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("failure:" + unityWeb.error);
                failureCallBack?.Invoke();
                yield break;
            }
            
            if (unityWeb.isDone)
            {
                string result = unityWeb.downloadHandler.text;
                if (getBase64)
                {
                    byte[] c = Convert.FromBase64String(result);
                    result = Encoding.UTF8.GetString(c);
                }

                //Debug.Log("success:" + result);
                successCallBack?.Invoke(result);
            }
        }
    }
}