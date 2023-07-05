using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnitaskXNode.Base;
using UnityEngine;

namespace UnitaskXNode
{
    /// <summary>
    /// 播放提示声音
    /// </summary>
    [CreateNodeMenu("声音/播放提示声音")]
    public class PlayVoiceStepNode : BaseStepNode
    {
        /// <summary>
        /// 要播放的声音片段
        /// </summary>
        [LabelText("声音")] [LabelWidth(30)] [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
        public AudioClip audioClip;

        public override async UniTask RunStepNode(UniTaskCompletionSource utcs)
        {
            if (GetInputPort("audioClip").IsConnected) audioClip = GetInputValue<AudioClip>("audioClip");

            Debug.LogError("未指定声音管理器播放");
            //await AudioManager.Instance.PlayVoice(audioClip);
            await EndStepNode(utcs);
        }
    }
}