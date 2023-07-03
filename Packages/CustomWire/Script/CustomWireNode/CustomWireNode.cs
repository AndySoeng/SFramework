/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWireNode.cs
  Author: 万剑飞       Version :1.0          Date: 2017年5月4日
  Description: 线段控制节点
************************************************************/

using UnityEngine;

[ExecuteInEditMode]
public class CustomWireNode : MonoBehaviour {

    [HideInInspector]
    public CustomWire wire;//线段控制
    
    /// <summary>
    /// 当前节点在列表中的位置索引号
    /// </summary>
    public int IndexAtNodes
    {
        get
        {
            return wire.Nodes.IndexOf(this);
        }
    }

    /// <summary>
    /// 前置节点
    /// </summary>
    public CustomWireNode FrontNode
    {
        get
        {
            if (IndexAtNodes == -1)
                return null;

            if (IndexAtNodes == 0)
                if (wire.closeWire)//闭合则第一个节点的前直节点为最后一个节点
                    return wire.Nodes[wire.Nodes.Count - 1];
                else 
                    return null;
            else
                return wire.Nodes[IndexAtNodes - 1];
        }
    }

    /// <summary>
    /// 后置节点
    /// </summary>
    public CustomWireNode BackNode
    {
        get
        {
            if (IndexAtNodes == -1)
                return null;

            if (IndexAtNodes == wire.Nodes.Count - 1)
                if (wire.closeWire)//闭合则最后一个节点的后置节点为第一个节点
                    return wire.Nodes[0];
                else
                    return null;
            else
                return wire.Nodes[IndexAtNodes + 1];
        }
    }
    
    /// <summary>
    /// 在当前节点前/后添加节点
    /// </summary>
    /// <param name="atFront">true：添加前置， false：添加后置</param>
    public void AddNode(bool atFront)
    {
        wire.InsertNode(IndexAtNodes, atFront);
    }
}
