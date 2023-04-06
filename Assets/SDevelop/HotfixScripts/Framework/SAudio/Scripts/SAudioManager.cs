using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace SFramework
{
    using UnityEngine;

    public class SAudioManager : MonoSingleton<SAudioManager>
    {
        private AudioRoot audioRoot;

        protected override async Task OnInit()
        {
            // 初始化UI根节点
            var goAudioRoot= await  Addressables.InstantiateAsync("AudioRoot.prefab", transform).Task;
            audioRoot = goAudioRoot.GetComponent<AudioRoot>();
            clickSoundClip  = await  Addressables.LoadAssetAsync<AudioClip>("AudioClick.mp3").Task;
        }

        private AudioClip clickSoundClip;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                if (!GetAudioSource(SoundType.UISound).isPlaying)
                {
                    PlayUISound(clickSoundClip);
                }
            }
        }

        public void PlayBackgroundMusic(AudioClip audioClip)
        {
            audioRoot.GetAudioSource(SoundType.BackgroundMusic).clip = audioClip;
            audioRoot.GetAudioSource(SoundType.BackgroundMusic).Play();
        }

        public void PauseBackgroundMusic()
        {
            if (audioRoot.GetAudioSource(SoundType.BackgroundMusic).isPlaying)
                audioRoot.GetAudioSource(SoundType.BackgroundMusic).Pause();
        }

        public void StopBackgroundMusic()
        {
            audioRoot.GetAudioSource(SoundType.BackgroundMusic).Stop();
            audioRoot.GetAudioSource(SoundType.BackgroundMusic).clip = null;
        }

        public float GetBackgroundMusicVolume()
        {
            return audioRoot.audioSourcesDic[SoundType.BackgroundMusic].volume;
        }

        public void SetBackgroundMusicVolume(float volume)
        {
            audioRoot.audioSourcesDic[SoundType.BackgroundMusic].volume = volume;
        }

        public AudioSource GetAudioSource(SoundType soundType)
        {
            return audioRoot.GetAudioSource(SoundType.UISound);
        }

        public void PlayUISound(AudioClip audioClip)
        {
            audioRoot.GetAudioSource(SoundType.UISound).clip = audioClip;
            audioRoot.GetAudioSource(SoundType.UISound).Play();
        }

        public void PauseUISound()
        {
            if (audioRoot.GetAudioSource(SoundType.UISound).isPlaying)
                audioRoot.GetAudioSource(SoundType.UISound).Pause();
        }

        public void StopUISound()
        {
            audioRoot.GetAudioSource(SoundType.UISound).Stop();
            audioRoot.GetAudioSource(SoundType.UISound).clip = null;
        }

        public void SetUISoundVolume(float volume)
        {
            audioRoot.audioSourcesDic[SoundType.UISound].volume = volume;
        }

        public float GetUISoundVolume()
        {
            return audioRoot.audioSourcesDic[SoundType.UISound].volume;
        }
    }

    public enum SoundType
    {
        BackgroundMusic,
        UISound,
    }
}