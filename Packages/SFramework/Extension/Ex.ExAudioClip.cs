using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ex
{
    /// <summary>
    /// AudioClip扩展
    /// </summary>
    public class ExAudioClip
    {
        /// <summary>
        /// 将AudioClip转换为byte数组
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public static byte[] ClipToBytes(AudioClip audioClip)
        {
            float[] samples = new float[audioClip.samples];

            audioClip.GetData(samples, 0);

            return AudioClipSamplesToBytes(samples);
        }


        
        public static  byte[] AudioClipSamplesToBytes(float []  samples)
        {
            short[] intData = new short[samples.Length];

            byte[] bytesData = new byte[samples.Length * 2];

            int rescaleFactor = 32767;

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }


        /// <summary>
        /// 将byte数组转换为AudioClip
        /// 如果对接讯飞语音合成接口，则需aue = "raw",sfl=0,
        /// 若对接讯飞语音合成接口， aue = "lame",sfl=1,则可将获取的语音数据写入本地，通过UnityWebRequestMultimedia读取本地mp3
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static AudioClip BytesToClip(byte[] rawData, int frequency = 16000)
        {
            float[] samples = new float[rawData.Length / 2];
            float rescaleFactor = 32767;
            short st = 0;
            float ft = 0;

            for (int i = 0; i < rawData.Length; i += 2)
            {
                st = BitConverter.ToInt16(rawData, i);
                ft = st / rescaleFactor;
                samples[i / 2] = ft;
            }

            //AudioClip audioClip = AudioClip.Create("GenAudioClip", samples.Length, 1, 44100, false, false);
            AudioClip audioClip = AudioClip.Create("GenAudioClip", samples.Length, 1, frequency, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }

        /// <summary>
        /// 截取音频片段音效并播放
        /// </summary>
        /// <param name="audioClip">截取的音频</param>
        /// <param name="startTime">截取的开始时间</param>
        /// <param name="endTime">截取的结束时间</param>
        /// <returns></returns>
        public static AudioClip PlayAudioClipSegment(AudioClip audioClip, float startTime, float endTime)
        {
            // 计算截取片段的长度
            float clipLength = endTime - startTime;
            // 创建一个新的截取剪辑
            AudioClip segment = AudioClip.Create("TempAudio", Mathf.RoundToInt(clipLength * audioClip.frequency), audioClip.channels, audioClip.frequency, false);

            // 计算截取的样本数
            int startSample = Mathf.RoundToInt(startTime * audioClip.frequency);
            int endSample = Mathf.RoundToInt(endTime * audioClip.frequency);
            int segmentLength = endSample - startSample;

            // 从原始音频剪辑中复制样本到截取剪辑
            float[] data = new float[segmentLength * audioClip.channels];
            audioClip.GetData(data, startSample);

            //数据设置进新的片段里
            segment.SetData(data, 0);

            return segment;
        }
        
        
        /// <summary>
        /// 此方法获取的每帧数据可能会导致讯飞语音听写失败。
        /// </summary>
        /// <param name="recordingClip"></param>
        /// <param name="OnGetVolumeData"></param>
        /// <param name="getDataCount">-1时返回每帧的所有采样数据，其他参数则返回指定数量的采样数据，当每帧的采样数据数量小于给定值时也返回所有每帧所有采样数据</param>
        /// <returns></returns>
        public static IEnumerator GetCurrentVolumeData(AudioClip recordingClip, UnityAction< float[]> OnGetVolumeData,int getDataCount=-1)
        {
            //最小每帧提取后256个采样数据（因为音频可视化使用的为256个）
            float[] volumeData = new float[256];

            int preOffset = 0;
            int offset = 0;
            while (true)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                offset = FrostweepGames.Plugins.Native.CustomMicrophone.GetPosition(FrostweepGames.Plugins.Native.CustomMicrophone.devices[0]) - 256 + 1;
#else
                offset = Microphone.GetPosition(Microphone.devices[0]);
#endif
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
                        //当getDataCount<0或每帧的采样数据小于getDataCount时，则返回每帧的所有采样数据
                        if (getDataCount<0|| offset - preOffset<=getDataCount)
                        {
                            volumeData = new float[offset - preOffset];
                            recordingClip.GetData(volumeData, offset-preOffset);
                        }
                        else
                        {
                            recordingClip.GetData(volumeData, offset-getDataCount);
                        }
                       
                    }

                    OnGetVolumeData?.Invoke( volumeData);
                    preOffset = offset;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }
}