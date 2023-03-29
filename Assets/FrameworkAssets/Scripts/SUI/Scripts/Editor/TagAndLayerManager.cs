namespace SFramework
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;

#if UNITY_EDITOR
    public class TagAndLayerManager
    {
        #region SortingLayer

        [MenuItem("Tools/UITagAndLayer管理器/SortingLayer")]
        public static void AddSortingLayer()
        {
            // 先遍历枚举拿到枚举的字符串
            List<string> lstSceenPriority = new List<string>();
            foreach (int v in Enum.GetValues(typeof(ESceenPriority)))
            {
                lstSceenPriority.Add(Enum.GetName(typeof(ESceenPriority), v));
            }

            // 清除数据
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            if (tagManager == null)
            {
                Debug.LogError("未能序列化tagManager！！！！！！");
                return;
            }

            SerializedProperty it = tagManager.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.name != "m_SortingLayers")
                {
                    continue;
                }

                // 先删除所有
                while (it.arraySize > 0)
                {
                    it.DeleteArrayElementAtIndex(0);
                }

                // 重新插入
                // 将枚举字符串生成到 sortingLayer
                foreach (var s in lstSceenPriority)
                {
                    it.InsertArrayElementAtIndex(it.arraySize);
                    SerializedProperty dataPoint = it.GetArrayElementAtIndex(it.arraySize - 1);

                    while (dataPoint.NextVisible(true))
                    {
                        if (dataPoint.name == "name")
                        {
                            dataPoint.stringValue = s;
                        }
                        else if (dataPoint.name == "uniqueID")
                        {
                            dataPoint.intValue = (int) Enum.Parse(typeof(ESceenPriority), s);
                        }
                    }
                }
            }

            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public static bool IsHaveSortingLayer(string sortingLayer)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Tagmanager.asset")[0]);
            if (tagManager == null)
            {
                Debug.LogError("未能序列化tagManager！！！！！！ IsHaveSortingLayer");
                return true;
            }

            SerializedProperty it = tagManager.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.name != "m_SortingLayers")
                {
                    continue;
                }

                for (int i = 0; i < it.arraySize; i++)
                {
                    SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                    while (dataPoint.NextVisible(true))
                    {
                        if (dataPoint.name != "name")
                        {
                            continue;
                        }

                        if (dataPoint.stringValue == sortingLayer)
                        {
                            return true;
                        }
                    }
                }
            }


            return false;
        }

        //public static void AddSortingLayer(string sortingLayer)
        //{
        //    if(IsHaveSortingLayer(sortingLayer))
        //    {
        //        return;
        //    }
        //    SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Tagmanager.asset")[0]);
        //    SerializedProperty it = tagManager.GetIterator();
        //    while (it.NextVisible(true))
        //    {
        //        if (it.name != "m_SortingLayers")
        //        {
        //            continue;
        //        }
        //        it.InsertArrayElementAtIndex(it.arraySize);
        //        SerializedProperty dataPoint = it.GetArrayElementAtIndex(it.arraySize - 1);
        //        while (dataPoint.NextVisible(true))
        //        {
        //            if (dataPoint.name != "name")
        //            {
        //                continue;
        //            }
        //            dataPoint.stringValue = sortingLayer;
        //            tagManager.ApplyModifiedProperties();
        //        }
        //    }
        //}

        #endregion



    }
#endif
}