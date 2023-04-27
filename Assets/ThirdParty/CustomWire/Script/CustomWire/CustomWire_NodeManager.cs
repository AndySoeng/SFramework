/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWire_NodeManager.cs
  Author: 万剑飞       Version :1.0          Date: 2017年5月4日
  Description: 节点管理
************************************************************/

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 节点管理
/// </summary>
public partial class CustomWire {

    [HideInInspector]
    [SerializeField]
    //节点列表，私有，提供一个只读属性
    private List<CustomWireNode> nodes = new List<CustomWireNode>();
    public List<CustomWireNode> Nodes
    {
        get { return new List<CustomWireNode>(nodes); }
    }
    
    /// <summary>
    /// 获取节点
    /// </summary>
    /// <param name="index">节点索引</param>
    /// <returns>节点</returns>
    public CustomWireNode GetNode(int index)
    {
        if (index < 0 || index > nodes.Count - 1)
            return null;
        else 
            return nodes[index];
    }
}

/// <summary>
/// 创建节点
/// </summary>
public partial class CustomWire
{
    /// <summary>
    /// 在末尾添加新节点
    /// </summary>
    public void AddNode()
    {
        //没有节点时在第一个位置创建
        if (nodes.Count == 0)
            InsertNode(0, true);
        //有节点时在最后一个节点的后面创建
        else
            InsertNode(nodes.Count - 1, false);
    }

    /// <summary>
    /// 在末尾添加指定节点
    /// </summary>
    /// <param name="node">指定节点</param>
    public void AddNode(CustomWireNode node)
    {
        if (node == null)
            return;

        //不重复添加
        if (nodes.Contains(node))
            return;
        
        nodes.Add(node);
        
        UpdateWire();
    }

    /// <summary>
    /// 添加节点集合
    /// </summary>
    /// <param name="nodeCollection">节点集合</param>
    public void AddNodeRange(CustomWireNode[] nodeCollection)
    {
        if (nodeCollection == null)
            return;

        nodes.AddRange(nodeCollection);
    }

    /// <summary>
    /// 添加节点集合
    /// </summary>
    /// <param name="goCollection">GameObject集合</param>
    public void AddNodeRange(GameObject[] goCollection)
    {
        InsertNodeRange(nodes.Count, goCollection);        
    }

    /// <summary>
    /// 添加节点集合
    /// </summary>
    /// <param name="tfCollection">Transform集合</param>
    public void AddNodeRange(Transform[] tfCollection)
    {
        InsertNodeRange(nodes.Count, tfCollection);
    }

    /// <summary>
    /// 添加节点集合
    /// </summary>
    /// <param name="v3Collection">Vector3数组集合</param>
    public void AddNodeRange(Vector3[] v3Collection)
    {
        InsertNodeRange(nodes.Count, v3Collection);
    }
}

/// <summary>
/// 插入节点
/// </summary>
public partial class CustomWire
{
    /// <summary>
    /// 在指定位置插入新节点
    /// </summary>
    /// <param name="index">指定位置索引</param>
    public void InsertNode(int index)
    {
        InsertNode(index, true);
    }

    /// <summary>
    /// 在指定位置插入前置/后置节点
    /// 添加前置节点则在指定位置添加新节点，添加后置节点则在指定位置后一位添加
    /// </summary>
    /// <param name="index">指定位置索引</param>
    /// <param name="atFront">true：新节点作为指定位置节点的前置节点， false：新节点作为指定位置节点的后置节点</param>
    public void InsertNode(int index, bool atFront)
    {
        //没有节点，则只能在0位置添加
        if (nodes.Count == 0)
        {
            if (atFront && index != 0)
            {
                Debug.Log("The node list is empty, please add node at first place");
                return;
            }
            else if (!atFront)
            {
                Debug.Log("Can't add node at back, because the node list is empty");
                return;
            }
        }
        //有节点，则必须在有效范围内
        else
        {
            if (index < 0 || index > nodes.Count - 1)
            {
                Debug.Log("The insert index out of range");
                return;
            }
        }

        //设置节点名字
        string namePattern = "Wire Node ";
        string nodeName = namePattern;
        int i = 0;
        for (i = 0; i < nodes.Count; i++)
            if (!nodes.Exists(p => p.name.Equals(namePattern + i)))
            {
                nodeName = namePattern + i;
                break;
            }
        if (i == nodes.Count)
            nodeName = namePattern + i;

        //创建节点物体
        GameObject nodeObj = new GameObject(nodeName);

        //新节点作为子物体
        nodeObj.transform.parent = transform;

        //设置新节点位置在索引为addIndex节点位置上，若没有则（0，0，0）
        if (nodes.Count > 0)
        {
            nodeObj.transform.position = nodes[index].transform.position;
        }
        else
        {
            nodeObj.transform.localPosition = Vector3.zero;
        }

        //添加节点脚本
        CustomWireNode nodeComponent = nodeObj.AddComponent<CustomWireNode>();

        //设置节点脚本
        nodeComponent.wire = this;

        //添加节点到列表
        nodes.Insert(atFront ? index : index + 1, nodeComponent);

        //更新线段
        UpdateWire();
    }

    /// <summary>
    /// 在指定位置插入节点
    /// </summary>
    /// <param name="index">指定位置索引</param>
    /// <param name="node">需要插入的节点</param>
    public void InsertNode(int index, CustomWireNode node)
    {
        if (index < 0 || index > nodes.Count)
            return;

        if (node == null)
            return;

        //不重复添加
        if (nodes.Contains(node))
            return;

        nodes.Insert(index, node);

        UpdateWire();
    }

    /// <summary>
    /// 插入节点集合
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="nodeCollection">节点集合</param>
    public void InsertNodeRange(int index, CustomWireNode[] nodeCollection)
    {
        if (nodeCollection == null)
            return;

        if (index < 0 || index > nodes.Count)
            return;

        nodes.InsertRange(index, nodeCollection);

        UpdateWire();
    }

    /// <summary>
    /// 插入节点集合
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="goCollection">GameObject集合</param>
    public void InsertNodeRange(int index, GameObject[] goCollection)
    {
        CustomWireNode[] nodeCollection = new CustomWireNode[goCollection.Length];

        for (int i = 0; i < goCollection.Length; i++)
        {
            CustomWireNode node = goCollection[i].GetComponent<CustomWireNode>();
            if (node == null)
                node = goCollection[i].AddComponent<CustomWireNode>();
            node.wire = this;
            nodeCollection[i] = node;
        }

        InsertNodeRange(index, nodeCollection);
    }

    /// <summary>
    /// 插入节点集合
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="tfCollection">Transform集合</param>
    public void InsertNodeRange(int index, Transform[] tfCollection)
    {
        GameObject[] gos = new GameObject[tfCollection.Length];

        for (int i = 0; i < tfCollection.Length; i++)
            gos[i] = tfCollection[i].gameObject;

        InsertNodeRange(index, gos);
    }

    /// <summary>
    /// 插入节点集合
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="v3Collection">Vector3集合</param>
    public void InsertNodeRange(int index, Vector3[] v3Collection)
    {
        GameObject[] gos = new GameObject[v3Collection.Length];

        for (int i = 0; i < gos.Length; i++)
        {
            gos[i] = new GameObject();
            gos[i].transform.position = v3Collection[i];
        }

        InsertNodeRange(index, gos);
    }
}

/// <summary>
/// 删除节点
/// </summary>
public partial class CustomWire
{
    /// <summary>
    /// 根据位置索引删除节点
    /// </summary>
    /// <param name="index">指定位置索引</param>
    public void RemoveNode(int index, bool destroyNodeGameObject = true)
    {
        //检查index是否在有效范围内
        if (index < 0 || index > nodes.Count - 1)
            return;

        RemoveNode(nodes[index], destroyNodeGameObject);
    }

    /// <summary>
    /// 删除指定节点
    /// </summary>
    /// <param name="node">需要删除的节点</param>
    public void RemoveNode(CustomWireNode node, bool destroyNodeGameObject = true)
    {
        if (node == null)
            return;

        //从列表中删除节点
        nodes.Remove(node);

        //删除节点物体，编辑模式下使用Undo可进行撤销操作    
        if (destroyNodeGameObject)
        {
            DestroyImmediate(node.gameObject);
        }

        //更新列表
        UpdateWire();
    }

    /// <summary>
    /// 删除所有节点
    /// </summary>
    public void RemoveAllNodes(bool destroyNodeGameObject = true)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            CustomWireNode node = nodes[i];
            if (node)
            {
                nodes.Remove(node);

                if (destroyNodeGameObject)
                    DestroyImmediate(node.gameObject);

                //删除当前节点，则后续节点前移一位，强制i-1，保证下一次循环指向下一个节点
                i--;
            }
        }

        //更新列表
        UpdateWire();
    }
}

/// <summary>
/// 移动节点，线段根据节点在列表中的位置依次计算，移动节点顺序可改变线段点连接方式
/// </summary>
public partial class CustomWire
{
    /// <summary>
    /// 节点顺序前移
    /// </summary>
    /// <param name="index">指定节点的索引</param>
    public void NodeMoveUp(int index)
    {
        if (index < nodes.Count && index > 0)
        {
            CustomWireNode temp = nodes[index - 1];
            nodes[index - 1] = nodes[index];
            nodes[index] = temp;

            //更新线段
            UpdateWire();
        }
    }

    /// <summary>
    /// 节点顺序后移
    /// </summary>
    /// <param name="index">指定节点的索引</param>
    public void NodeMoveDown(int index)
    {
        if (index < nodes.Count - 1 && index >= 0)
        {
            CustomWireNode temp = nodes[index + 1];
            nodes[index + 1] = nodes[index];
            nodes[index] = temp;

            //更新线段
            UpdateWire();
        }
    }
}

/// <summary>
/// 其他
/// </summary>
public partial class CustomWire
{
    /// <summary>
    /// 获取所有节点的位置坐标
    /// </summary>
    /// <returns></returns>
    private Vector3[] GetNodesPosition()
    {
        if (nodes.Count <= 0)
            return new Vector3[] { };

        Vector3[] v3s = new Vector3[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            v3s[i] = nodes[i].transform.position;
        }

        return v3s;
    }

    /// <summary>
    /// 检查节点列表，若节点为空，则删除节点
    /// </summary>
    private void CheckNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == null)
            {
                nodes.RemoveAt(i);
                i--;
            }
        }
    }
}
