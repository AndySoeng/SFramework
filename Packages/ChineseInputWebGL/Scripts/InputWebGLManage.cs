using System.Collections.Generic;
using UnityEngine;

public class InputWebGLManage : MonoBehaviour
{
    private static InputWebGLManage instance;

    public static InputWebGLManage Instance
    {
        get
        {
            if (instance == null)
            {
                InputWebGLManage inputWebGLManage = GameObject.FindObjectOfType<InputWebGLManage>();
                if (inputWebGLManage == null)
                {
                    GameObject o = new GameObject("InputWebGLManage");
                    DontDestroyOnLoad(o);
                    inputWebGLManage = o.AddComponent<InputWebGLManage>();
                }
                instance = inputWebGLManage;
                
            }
            return instance;
        }
    }

    public Dictionary<int, InputField_WebGL> GameObjectID_InputField = new Dictionary<int, InputField_WebGL>();

    public void InputShow(string inputID, string text, string fontsize, string indexStr, string inputRectStr)
    {
        ChineseInputWebGL.InputShow(gameObject.name, inputID, text, fontsize, indexStr, inputRectStr);
    }

    /// <summary>
    /// 注册
    /// </summary>
    public void Register(int id, InputField_WebGL inputFieldT)
    {
        if (!GameObjectID_InputField.ContainsKey(id))
        {
            GameObjectID_InputField.Add(id, inputFieldT);
        }
        else
        {
            GameObjectID_InputField[id] = inputFieldT;
        }
    }

    public InputField_WebGL GetInputField(int id)
    {
        if (GameObjectID_InputField.ContainsKey(id))
        {
            return GameObjectID_InputField[id];
        }
        return null;
    }
#if UNITY_WEBGL && !UNITY_EDITOR
#region WebGL回调
    public void OnInputText(string text)
    {
        string[] strs = text.Split('|');
        int inputID = ParseInputId(strs[0]);
        InputField_WebGL inputFieldT = GetInputField(inputID);
        if (inputFieldT)
        {
            inputFieldT.OnInputText(strs[1], strs[2], strs[3]);
        }
    }
    public void OnInputEnd(string inputIDStr)
    {
        int inputID = ParseInputId(inputIDStr);
        InputField_WebGL inputFieldT = GetInputField(inputID);
        if (inputFieldT)
        {
            inputFieldT.OnInputEnd();
        }
    }

    public void SelectAll(string inputIDStr)
    {
        int inputID = ParseInputId(inputIDStr);
        InputField_WebGL inputFieldT = GetInputField(inputID);
        if (inputFieldT)
        {
            inputFieldT.SelectAll();
        }
    }

    private int ParseInputId(string inputIDStr)
    {
        try
        {
            return int.Parse(inputIDStr);
        }
        catch (Exception e)
        {
            Debug.LogError("InputWebGLManage的InputID:" + e.Message);
            return 0;
        }
    }
#endregion
#endif
}
