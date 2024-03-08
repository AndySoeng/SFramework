using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidScan3503M : MonoBehaviour
{
    public static AndroidScan3503M Ins
    {
        get
        {
            if (ins == null)
            {
                GameObject go = new GameObject("AndroidScan3503M");
                ins = go.AddComponent<AndroidScan3503M>();
                ins. JO_SCANLIB = new AndroidJavaObject("com.example.scanlib.MainActivity");
                ins. JO_SCANLIB.Call("RegisterScanBroadcastReceiver");
                DontDestroyOnLoad(go);
            }
            return ins;
        }
    }

    private static AndroidScan3503M ins;


    public AndroidJavaObject JO_SCANLIB { get; set; }

    

    public delegate void AndroidScanResultEvent(string result);

    #region Piravate Method

    private event AndroidScanResultEvent callBack_AndroidScanResult;

    private void CallBackScanResult(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            callBack_AndroidScanResult?.Invoke(value);
        }
    }

    private void Log(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            UnityEngine.Debug.Log(value);
        }
    }

    #endregion


    #region Public Method

    public void Subscribe(AndroidScanResultEvent callBack)
    {
        callBack_AndroidScanResult += callBack;
    }

    public void UnSubscribe(AndroidScanResultEvent callBack)
    {
        callBack_AndroidScanResult -= callBack;
    }

    public void OpenOrCloseScan(bool isOpen)
    {
        JO_SCANLIB.Call("OpenOrCloseScan", isOpen);
    }

    public void TriggerScan()
    {
        JO_SCANLIB.Call("TriggerScan");
    }

    #endregion
}