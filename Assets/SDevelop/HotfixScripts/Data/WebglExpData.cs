using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ExpMoudle{
    实验基础,
    知识考核,
}
/// <summary>
/// 当前实验的缓存数据类
/// </summary>
public static class WebglExpData
{
    public static DateTime firstEnterTime;

    public static Dictionary<ExpMoudle, int> ExpMoudleScore = new Dictionary<ExpMoudle, int>();
    public static Dictionary<ExpMoudle, bool> ExpMoudleFineshed = new Dictionary<ExpMoudle, bool>();
}
