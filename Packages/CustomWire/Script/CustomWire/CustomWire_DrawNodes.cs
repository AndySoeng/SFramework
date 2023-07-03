/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWire_DrawNodes.cs
  Author: 万剑飞       Version :1.0          Date: 2017年5月4日
  Description: 绘制节点
************************************************************/

using UnityEngine;

public partial class CustomWire {

    public bool drawNodes;//true：绘制节点
    public float nodeRadius = 0.01f;//表示节点的球体的半径

    void OnDrawGizmos()
    {
        if (drawNodes)
        {
            Gizmos.color = Color.red;
            
            foreach (var node in nodes)
            {
                Vector3 pos = node.transform.position;
                Gizmos.DrawSphere(pos, nodeRadius);
            }
        }
    }
}
