using System;
using UnityEngine;
using XNode;
using XNodeEditor;
using Object = UnityEngine.Object;

namespace UnitaskXNode.Base.Editor
{
    [CustomNodeGraphEditor(typeof(StepGraph))]
    public class StepGraphEditor : NodeGraphEditor
    {
        public override string GetNodeMenuName(Type type)
        {
            switch (type.Name)
            {
                case "BaseStepNode":
                    return null;
                case "BaseOtherNode":
                    return null;
                default:
                    break;
            }
            return base.GetNodeMenuName(type);
        }


        public override void OnDropObjects(Object[] objects)
        {
            //解决无法拖拽非NodeGraph的对象到NodeGraph上的问题
            //base.OnDropObjects(objects);
        }

        public override Node CreateNode(Type type, Vector2 position)
        {
            Node node = base.CreateNode(type, position);

            //修改节点名称便于阅读
            switch (type.Name)
            {
                case "StartGraphStep":
                    node.name = "开始步骤";
                    break;
                case "LocalScriptStepNode":
                    node.name = "本地脚本";
                    break;
                case "InstantiationStepNode":
                    node.name = "实例化对象";
                    break;
                case "LoadSceneStepNode":
                    node.name = "加载场景";
                    break;
                case "LoadStepGraphStepNode":
                    node.name = "运行步骤图";
                    break;


                case "WaitTimeStepNode":
                    node.name = "等待X秒";
                    break;
                case "PlayAudioStepNode":
                    node.name = "播放其他声音";
                    break;
                case "PlayBgmStepNode":
                    node.name = "播放背景音乐";
                    break;
                case "PlayVoiceStepNode":
                    node.name = "播放提示声音";
                    break;
                case "StopAudioStepNode":
                    node.name = "停止其他声音";
                    break;
                case "StopBgmStepNode":
                    node.name = "停止背景音乐";
                    break;
                case "StopVoiceStepNode":
                    node.name = "停止提示声音";
                    break;

                case "AnimationPlayStepNode":
                    node.name = "播放动画";
                    break;
                case "AnimationStopStepNode":
                    node.name = "停止动画";
                    break;
                case "AnimationPlayForgetStepNode":
                    node.name = "播放动画(不等待)";
                    break;

                case "GameObjectStepNode":
                    node.name = "游戏对象设置";
                    break;
                case "GetGameObjectNode":
                    node.name = "获取游戏对象(唯一)";
                    break;

                case "SetTransformStepNode":
                    node.name = "设置变换";
                    break;
                case "GetTransformStepNode":
                    node.name = "获取变换";
                    break;
                case "MoveToPositionStepNode":
                    node.name = "移动到目标";
                    break;
                case "RotateToEulerAngleStepNode":
                    node.name = "旋转到角度";
                    break;
                case "ScaleToVector3StepNode":
                    node.name = "缩放到数值";
                    break;


                case "Vector3CombineNode":
                    node.name = "Vector3合并";
                    break;
                case "Vector3DecomposeNode":
                    node.name = "Vector3分解";
                    break;

                case "SetParameterStepNode":
                    node.name = "设置参数";
                    break;
                case "GetParameterStepNode":
                    node.name = "获取参数";
                    break;


                case "IfStepNode":
                    node.name = "if分支";
                    break;
                case "MultipleStepNode":
                    node.name = "并行任务";
                    break;
            }

            if (type == typeof(IfStepNode))
            {
                var baseStepNode = node as BaseStepNode;
                baseStepNode.SetNextShow(false);
            }
            
            return node;
        }
       

        public override Color GetPortColor(NodePort port)
        {
            if (port.ValueType == typeof(UnitaskPort))
            {
                return new Color(0.18f, 0.8f, 0.443f);
            }
            else
            {
                return new Color(0.945f, 0.768f, 0.06f);
            }
        }

        public override Color GetTypeColor(Type type)
        {
            if (type == typeof(UnitaskPort))
            {
                return new Color(0.18f, 0.8f, 0.443f);
            }
            else
            {
                return new Color(0.945f, 0.768f, 0.06f);
            }
        }
    }
}