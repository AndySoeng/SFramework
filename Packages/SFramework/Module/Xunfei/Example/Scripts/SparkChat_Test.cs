using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xunfei.Runtime;

namespace Xunfei.Example
{
    public class SparkChat_Test : MonoBehaviour
    {
        public string userName = "123456";
        public Text txt_Result;
        public InputField input_Chat;
        public Button input_NewChat;
        public Button btn_SendChat;
        public Button input_ClearChat;

        private SparkChat _sparkChat;

        // Start is called before the first frame update
        void Start()
        {
            btn_SendChat.onClick.AddListener(() =>
            {
                if (_sparkChat == null)
                {
                    _sparkChat = new SparkChat(userName, (result) =>
                    {
                        txt_Result.text += "SparkChat:" + result + "\n";

                        btn_SendChat.interactable = true;
                    });
                }


                txt_Result.text += $"User({userName}):" + input_Chat.text + "\n";
                _sparkChat.SendChat(input_Chat.text);
                input_Chat.text = String.Empty;

                btn_SendChat.interactable = false;
            });
            input_NewChat.onClick.AddListener(() =>
            {
                _sparkChat = null;
                txt_Result.text=String.Empty;
            });

            input_ClearChat.onClick.AddListener(() =>
            {
                txt_Result.text=String.Empty;
            });
        }
    }
}