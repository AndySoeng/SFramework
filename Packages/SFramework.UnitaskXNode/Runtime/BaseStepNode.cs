using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using XNode;


namespace UnitaskXNode.Base
{
    /// <summary>
    /// 基础动作节点
    /// </summary>
    [NodeTint("#3498db")]
    public abstract class BaseStepNode : Node
    {
        /// <summary>
        /// 上一个动作
        /// </summary>
        [Required] [Input(ShowBackingValue.Never)] [LabelText("上一个动作")]
        public UnitaskPort previous;
        
        

        /// <summary>
        /// 下一个动作
        /// </summary>
        [Output(ShowBackingValue.Never, connectionType = ConnectionType.Override)] [LabelText("下一个动作")] [ShowIf("showNext")]
        public UnitaskPort nextStepNode;

        /// <summary>
        /// 是否显示下一个接口
        /// </summary>
        private  bool showNext = true;

        public void SetNextShow(bool value)
        {
            showNext = value;
        }

        #region XNode

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);
            if (from.ValueType != to.ValueType) from.Disconnect(to);
        }

        public override object GetValue(NodePort port)
        {
            return null;
        }

        #endregion

        #region Unitask

        public async UniTask StartStepNode()
        {
            await RunStepNode(new UniTaskCompletionSource());
        }

        public virtual async UniTask RunStepNode(UniTaskCompletionSource utcs)
        {
            await EndStepNode(utcs);
        }

        protected  async UniTask EndStepNode(UniTaskCompletionSource utcs)
        {
            NodePort nodePort = GetPort("nextStepNode").Connection;
            if (nodePort!=null)
            {
                await ((BaseStepNode)nodePort.node).StartStepNode();
            }
            else
            {
                //await utcs.Task;
                utcs.TrySetResult();
            }

        }

        #endregion
    }
}