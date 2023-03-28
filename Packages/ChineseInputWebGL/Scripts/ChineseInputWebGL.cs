using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public static class ChineseInputWebGL
{

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void InputShow(string GameObjectName, string InputID, string text, string fontsize, string indexStr, string inputRectStr);
 //   [DllImport("__Internal")]
	//public static extern void InputEnd ();    

#else
    public static void InputShow(string GameObjectName, string InputID, string text, string fontsize, string indexStr, string inputRectStr) { }
    //public static void InputEnd() { }
#endif

}
