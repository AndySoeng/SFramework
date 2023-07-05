using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using XNode;

namespace UnitaskXNode.Base
{
    [DisallowMultipleNodes()]
    [CreateNodeMenu("开始步骤")]
    [NodeTint(0.18f, 0.8f, 0.443f)]
    public class StartGraphStep : Node
    {
        [Output(ShowBackingValue.Never, connectionType = ConnectionType.Override)] [LabelText("下一个动作")] 
        public UnitaskPort nextStepNode;

        public async UniTask RunStepNode()
        {
            await ((BaseStepNode)GetPort( "nextStepNode").Connection.node).StartStepNode();
        }
    }
}