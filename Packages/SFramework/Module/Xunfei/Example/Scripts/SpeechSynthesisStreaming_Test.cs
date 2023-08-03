using Ex;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Xunfei.Runtime;

namespace Xunfei.Example
{
    public class SpeechSynthesisStreaming_Test : MonoBehaviour
    {
        //声明一个音效切片变量，用于存储录制的返回值
        public AudioClip clip;
        public AudioSource as_AudioSource;

        public Button btn_Start;
        public InputField input_Result;

        public Button btn_To语音听写;
        private void Start()
        {
            btn_Start.onClick.AddListener(() =>
            {
                btn_Start.interactable = false;
                input_Result.interactable = false;
                SpeechSynthesisStreaming speechSynthesis = new SpeechSynthesisStreaming(input_Result.text, (bytes) =>
                {
                    btn_Start.interactable = true;
                    input_Result.interactable = true;

                    as_AudioSource.clip = ExAudioClip.BytesToClip(bytes);
                    //播放
                    as_AudioSource.Play();
                });
            });
            btn_To语音听写.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("语音听写（流式版）");
            });
        }
    }
}