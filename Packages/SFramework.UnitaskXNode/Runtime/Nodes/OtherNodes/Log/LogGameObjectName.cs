
using Cysharp.Threading.Tasks;
using UnitaskXNode.Base;
using UnityEngine;

namespace UnitaskXNode
{ 
    [CreateNodeMenu("Log/输出游戏物体名称")]
    public class LogGameObjectName : BaseStepNode
    {

        [Input(ShowBackingValue.Unconnected,ConnectionType.Override)]
        public GameObject gameObject;

        
        public override async UniTask RunStepNode(UniTaskCompletionSource utcs)
        {
            Debug.Log(gameObject.name);
            await base.RunStepNode(utcs);
            
        }
    }
}