using System.Collections;
using DG.Tweening;
using FrostweepGames.Plugins.Native;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class TestAudioVisualization : MonoBehaviour
{
    public AudioSource _source;
    public SamplesVisualization samplesVisualization;

    void Start()
    {
        Debug.Log("DEMO:按O键开始，按P键结束");
        CustomMicrophone.RequestMicrophonePermission();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            CustomMicrophone.RefreshMicrophoneDevices();
            _source.clip = CustomMicrophone.Start(CustomMicrophone.devices[0], false, 100, 44100);
            StartCoroutine(Ex.ExAudio.GetCurrentVolumeData(_source.clip, samplesVisualization.OnSamplesChanged));
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            CustomMicrophone.End(CustomMicrophone.devices[0]);
            _source.Play();
        }
    }

    public static IEnumerator GetCurrentVolumeData(AudioClip recordingClip, UnityAction<float[]> OnGetVolumeData)
    {
        float[] volumeData = new float[256];

        int preOffset = 0;
        int offset = 0;
        while (true)
        {
            offset = CustomMicrophone.GetPosition(CustomMicrophone.devices[0]) - 256 + 1;
            //获取的offset超过总采样数则退出循环
            //Debug.Log(offset   +"\t"+recordingClip.frequency * recordingClip.length);
            if (offset >= recordingClip.frequency * recordingClip.length)
            {
                yield break;
            }

            if (preOffset != offset) //若offset相同，则获取到的数据时相同的，所以直接跳过输出
            {
                if (offset > 0)
                {
                    recordingClip.GetData(volumeData, offset);
                }

                OnGetVolumeData?.Invoke(volumeData);
                preOffset = offset;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}