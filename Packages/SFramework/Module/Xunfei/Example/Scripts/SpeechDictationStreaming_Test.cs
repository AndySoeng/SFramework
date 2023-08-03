using Ex;
using FrostweepGames.Plugins.Native;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Xunfei.Runtime;

namespace Xunfei.Example
{
    public class SpeechDictationStreaming_Test : MonoBehaviour
    {
        //声明一个音效切片变量，用于存储录制的返回值
        public AudioClip clip;
        public AudioSource as_AudioSource;

        public Text txt_TimeLength;
        public Slider sd_TimeLength;
        public Button btn_Start;
        public Button btn_Stop;
        public InputField input_Result;

        public Button btn_To语音合成;

        private void Start()
        {
            CustomMicrophone.RefreshMicrophoneDevices();
            CustomMicrophone.RequestMicrophonePermission();

            txt_TimeLength.text = sd_TimeLength.value.ToString();
            sd_TimeLength.onValueChanged.AddListener((value) => { txt_TimeLength.text = value.ToString(); });

            btn_Start.interactable = true;
            btn_Stop.interactable = false;

            btn_Start.onClick.AddListener(() =>
            {
                btn_Start.interactable = false;
                btn_Stop.interactable = true;
                sd_TimeLength.interactable = false;
                input_Result.interactable = false;

                //开始录制
                //参数1 设备名，传空则使用默认值
                //参数2 超过录制长度后，是否重头录制
                //参数3 录制时长(单位 秒)
                //参数4 采样率
                clip =CustomMicrophone.Start(CustomMicrophone.devices[0], false, (int)sd_TimeLength.value, 16000); 
            });

            btn_Stop.onClick.AddListener(() =>
            {
                btn_Stop.interactable = false;

                //结束录制
                CustomMicrophone.End(CustomMicrophone.devices[0]);
                //把添加的这个AudioSource的切片音频文件 = 刚才录制的切片音频文件

                as_AudioSource.clip = clip;
                //播放
                as_AudioSource.Play();

                byte[] clipData = ExAudioClip.ClipToBytes(clip);


                SpeechDictationStreaming speechDictation = new SpeechDictationStreaming(this, clipData, (str) =>
                {
                    input_Result.text = str;
                    Debug.Log("接收到中间结果："+str);
                }, (str) =>
                {
                    Debug.Log("最终结果："+str);
                    btn_Start.interactable = true;
                    sd_TimeLength.interactable = true;
                    input_Result.interactable = true;
                    input_Result.text = str;
                });
            });
            
            btn_To语音合成.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("语音合成（流式版）");
            });
        }
    }
}