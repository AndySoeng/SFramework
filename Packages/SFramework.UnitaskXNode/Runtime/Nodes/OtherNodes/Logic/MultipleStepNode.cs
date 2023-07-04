using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnitaskXNode.Base;
using UnityEngine;
using UnityEngine.Events;

namespace UnitaskXNode
{
    namespace UnitaskXNode.Base
    {
        /// <summary>
        /// 并行任务
        /// </summary>
        [NodeTint("#9b59b6")]
        [CreateNodeMenu("逻辑/并行任务")]
        public class MultipleStepNode : BaseStepNode
        {
            /// <summary>
            /// 类型
            /// </summary>
            [LabelText("类型")] [LabelWidth(30)] public MultipleType multiple;


            /// <summary>
            /// 并行任务
            /// </summary>
            [LabelText("并行任务")] [Output(ShowBackingValue.Never, ConnectionType.Override, dynamicPortList = true)]
            public List<UnitaskPort> unitaskPorts;


            private List<UniTask> uniTasks = new List<UniTask>();
            public override async UniTask RunStepNode(UniTaskCompletionSource utcs)
            {
                uniTasks.Clear();
                
                foreach (var item in DynamicOutputs)
                {
                    if (item.IsConnected)
                    {
                        BaseStepNode stepNode = (BaseStepNode)item.Connection.node;
                        uniTasks.Add(stepNode.StartStepNode());
                    }
                }

                switch (multiple)
                {
                    case MultipleType.All:
                        await UniTask.WhenAll(uniTasks);
                        break;
                    case MultipleType.Any:
                        await UniTask.WhenAny(uniTasks);
                        break;
                }

                await EndStepNode(utcs);
            }
        }
    }
}