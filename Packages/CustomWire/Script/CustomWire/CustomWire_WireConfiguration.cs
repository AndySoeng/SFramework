/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWire_WireConfiguration.cs
  Author: 万剑飞       Version :1.0          Date: 2017年5月4日
  Description: 线的配置
************************************************************/

using UnityEngine;
using System;

public partial class CustomWire
{
    //曲线、折线
    public enum WireType
    {
        Curve,
        Linear
    }

    public WireType wireType = WireType.Curve;

    public LineRenderer lineRenderer;
    public bool closeWire; //true：闭合（首尾相连）  false：非闭合

    /// <summary>
    /// 根据节点设置线段
    /// </summary>
    public void SetWire()
    {
        if (lineRenderer == null)
        {
            Debug.Log("LineRenderer has not been assigned!");
            return;
        }

        //获取节点位置
        Vector3[] pos = GetNodesPosition();
        if (pos.Length > 0)
        {
            //如果是闭合线圈，则在末尾添加第一个元素
            if (closeWire)
            {
                Vector3[] tempPos = new Vector3[pos.Length + 1];
                Array.Copy(pos, tempPos, pos.Length);
                tempPos[tempPos.Length - 1] = tempPos[0];
                pos = new Vector3[tempPos.Length];
                Array.Copy(tempPos, pos, tempPos.Length);
            }

            switch (wireType)
            {
                case WireType.Curve:
                    //只有节点数量在3个及3个以上时才能绘制曲线，否则不绘制
                    if (pos.Length >= 3)
                        SetLineRenderer(GetSmoothCurve(pos));
                    else
                        ClearLineRenderer();
                    break;
                case WireType.Linear:
                    SetLineRenderer(pos);
                    break;
                default:
                    break;
            }
        }
        else //没有节点时不绘制
        {
            ClearLineRenderer();
        }
    }

    /// <summary>
    /// 设置线段
    /// </summary>
    /// <param name="v3s">线段点的位置</param>
    private void SetLineRenderer(Vector3[] v3s)
    {
        //lineRenderer.SetVertexCount(v3s.Length);
        lineRenderer.positionCount = v3s.Length;
        lineRenderer.SetPositions(v3s);
    }

    /// <summary>
    /// 清空线段
    /// </summary>
    private void ClearLineRenderer()
    {
        lineRenderer.SetPositions(new Vector3[] { });
        //lineRenderer.SetVertexCount(0);
        lineRenderer.positionCount = 0;
    }
}