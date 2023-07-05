using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnitaskXNode.Base;
using UnityEngine;
using XNode;
namespace UnitaskXNode
{
    /// <summary>
    /// 分解Vector3
    /// </summary>
    [CreateNodeMenu("数学/Vector3分解")]
    public class Vector3DecomposeNode : BaseOtherNode
    {
        /// <summary>
        /// 输入值
        /// </summary>
        [LabelText("Vector3")] [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
        public Vector3 inputValue;


        [Output(ShowBackingValue.Never, ConnectionType.Multiple)]
        public float x;

        [Output(ShowBackingValue.Never, ConnectionType.Multiple)]
        public float y;

        [Output(ShowBackingValue.Never, ConnectionType.Multiple)]
        public float z;

        public override object GetValue(NodePort port)
        {
            if (GetPort("inputValue").IsConnected)
            {
                inputValue = GetInputValue<Vector3>("inputValue");
            }

            switch (port.fieldName)
            {
                case "x":
                    return inputValue.x;
                case "y":
                    return inputValue.y;
                case "z":
                    return inputValue.z;
            }

            return null;
        }
    }
}