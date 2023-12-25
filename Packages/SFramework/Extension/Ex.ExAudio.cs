using UnityEngine;


namespace Ex
{
    public static class ExAudio
    {
            
        /// <summary>
        /// 截取音频片段音效并播放
        /// </summary>
        /// <param name="audioClip">截取的音频</param>
        /// <param name="startTime">截取的开始时间</param>
        /// <param name="endTime">截取的结束时间</param>
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
    }
}
