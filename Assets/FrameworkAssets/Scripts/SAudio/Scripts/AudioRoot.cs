namespace SFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.Audio;

    public class AudioRoot : SerializedMonoBehaviour
    {
        public AudioMixer audioMixer;
        public Dictionary<SoundType, AudioSource> audioSourcesDic = new Dictionary<SoundType, AudioSource>();

        public AudioSource GetAudioSource(SoundType soundType)
        {
            if (audioSourcesDic.ContainsKey(soundType))
                return audioSourcesDic[soundType];
            else
            {
                Debug.LogError("不存在此AudioSource");
                return null;
            }
        }
    }
}