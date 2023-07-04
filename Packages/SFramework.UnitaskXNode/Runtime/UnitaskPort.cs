using System;
using System.Reflection;
using XNode;

namespace UnitaskXNode.Base
{
    [Serializable]
    public class UnitaskPort : NodePort
    {
        public UnitaskPort(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        public UnitaskPort(NodePort nodePort, Node node) : base(nodePort, node)
        {
        }

        public UnitaskPort(string fieldName, Type type, IO direction, Node.ConnectionType connectionType, Node.TypeConstraint typeConstraint, Node node) : base(fieldName, type, direction, connectionType, typeConstraint, node)
        {
        }
    }
}