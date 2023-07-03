/************************************************************
  Copyright (C), 2007-2017,BJ Rainier Tech. Co., Ltd.
  FileName: CustomWireGUI.cs
  Author: 万剑飞       Version :1.0          Date: 2017年5月4日
  Description: GUI通用设置
************************************************************/

using UnityEngine;
using UnityEditor;

public static class CustomWireGUI {

    public static GUIStyle ButtonStyle()
    {
        GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
        style.stretchWidth = false;
        style.fixedWidth = 22f;

        return style;
    }
}
