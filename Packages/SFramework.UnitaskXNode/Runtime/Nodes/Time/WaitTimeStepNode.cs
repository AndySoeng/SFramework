using System;
using Cysharp.Threading.Tasks;
using UnitaskXNode.Base;

namespace UnitaskXNode
{ 
    [CreateNodeMenu("时间/等待X秒")]
    public class WaitTimeStepNode : BaseStepNode
    {

        [Input(ShowBackingValue.Unconnected,ConnectionType.Override)]
        public float waitTime;

        public override async UniTask RunStepNode(UniTaskCompletionSource utcs)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(waitTime), ignoreTimeScale: false);
            await base.RunStepNode(utcs);
            
        }
    }
}