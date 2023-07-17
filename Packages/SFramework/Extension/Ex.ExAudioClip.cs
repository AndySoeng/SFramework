using System;
using UnityEngine;

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
        public static AudioClip BytesToClip(byte[] rawData, int frequency=16000)
        {
            float[] samples = new float[rawData.Length / 2];
            float rescaleFactor = 32767;
            short st = 0;
            float ft = 0;
 
            for(int i=0;i<rawData.Length;i+=2)
            {
                st = BitConverter.ToInt16(rawData,i);
                ft = st / rescaleFactor;
                samples[i / 2] = ft;
            }
        
            //AudioClip audioClip = AudioClip.Create("GenAudioClip", samples.Length, 1, 44100, false, false);
            AudioClip audioClip = AudioClip.Create("GenAudioClip", samples.Length, 1, frequency, false);
            audioClip.SetData(samples, 0);
 
            return audioClip;
        }
        
    }
}