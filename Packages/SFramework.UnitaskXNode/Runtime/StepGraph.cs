using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace UnitaskXNode.Base
{
    [RequireNode(typeof(StartGraphStep))]
    [Serializable]
    public class StepGraph : NodeGraph
    {
        /// <summary>
        /// 起始节点
        /// </summary>
        [ReadOnly]
        [SerializeField]
        public StartGraphStep startGraphStep;

        /// <summary>
        /// 开始步骤
        /// </summary>
        public async UniTask StartGraph()
        {
            await startGraphStep.RunStepNode();
        }

        #region XNode

        public override Node AddNode(Type type)
        {
            Node.graphHotfix = this;
            Node node = CreateInstance(type) as Node;
            node.graph = this;
            nodes.Add(node);
            if (node.GetType() == typeof(StartGraphStep))
            {
                startGraphStep = (StartGraphStep)node;
            }

            return node;
        }

      
        #endregion
    }
}