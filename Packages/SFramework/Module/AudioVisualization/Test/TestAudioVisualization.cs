using System.Collections;
using FrostweepGames.Plugins.Native;
using UnityEngine;
using UnityEngine.Events;

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
            _source.clip = CustomMicrophone.Start(CustomMicrophone.devices[0], false, 100, 16000);
            StartCoroutine(Ex.ExAudioClip.GetCurrentVolumeData(_source.clip, samplesVisualization.OnSamplesChanged,256));
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            CustomMicrophone.End(CustomMicrophone.devices[0]);
            _source.Play();
        }
    }

    
}