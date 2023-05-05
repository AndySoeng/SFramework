#if UNITY_EDITOR


using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SFramework
{
    public static class CleanSameMaterials
    {
        [MenuItem("Tools/STools/Clean Same Materials")]
        static void CleanMaterials()
        {
            MaterialInfo[] matInfos = GetAllAssetsMaterialInfos();
            Debug.Log("Assets内材质共：" + matInfos.Length);
            List<List<MaterialInfo>> classifyMatInfos = DistinguishDuplicateMaterialInfos(matInfos);
            Debug.Log($"共分组{classifyMatInfos.Count}组,重复材质数量{matInfos.Length - classifyMatInfos.Count}个");
            DebugDistinguishDuplicateMaterialInfos(classifyMatInfos);
            MergeMaterialDependencies(classifyMatInfos);
            ClearnDuplicateMaterial(classifyMatInfos);
            classifyMatInfos = null;
            matInfos = null;
            Resources.UnloadUnusedAssets();
        }


        public static MaterialInfo[] GetAllAssetsMaterialInfos()
        {
            return AssetDatabase.FindAssets("t:Material").Select(AssetDatabase.GUIDToAssetPath).Where(x => x.StartsWith("Assets/") && x.EndsWith(".mat"))
                .Select(x => new MaterialInfo(x)).ToArray();
        }

        public static List<List<MaterialInfo>> DistinguishDuplicateMaterialInfos(MaterialInfo[] matInfos)
        {
            List<List<MaterialInfo>> classifyMatInfos = new List<List<MaterialInfo>>();
            for (int i = 0; i < matInfos.Length; i++)
            {
                bool isAdd = false;
                for (int j = 0; j < classifyMatInfos.Count; j++)
                {
                    if (matInfos[i].matSerializeMat == classifyMatInfos[j][0].matSerializeMat)
                    {
                        classifyMatInfos[j].Add(matInfos[i]);
                        isAdd = true;
                        break;
                    }
                }

                if (!isAdd)
                {
                    List<MaterialInfo> temp = new List<MaterialInfo>();
                    temp.Add(matInfos[i]);
                    classifyMatInfos.Add(temp);
                }
            }

            return classifyMatInfos;
        }

        private static void DebugDistinguishDuplicateMaterialInfos(List<List<MaterialInfo>> classifyMatInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("------------重复情况------------");
            for (int i = 0; i < classifyMatInfos.Count; i++)
            {
                if (classifyMatInfos[i].Count > 1)
                {
                    sb.AppendLine($"{classifyMatInfos[i].Count}个相同材质");
                    for (int j = 0; j < classifyMatInfos[i].Count; j++)
                    {
                        sb.AppendLine(classifyMatInfos[i][j].assetPath);
                    }

                    sb.AppendLine("---------------------------");
                }
            }

            Debug.Log(sb.ToString());
        }

        private static void MergeMaterialDependencies(List<List<MaterialInfo>> classifyMatInfos)
        {
            var prefabPath = AssetDatabase.FindAssets("t:Prefab").Select(AssetDatabase.GUIDToAssetPath).Where(x => x.StartsWith("Assets/")).ToArray(); // 获取所有Prefab的GUID

            for (int i = 0; i < classifyMatInfos.Count; i++)
            {
                if (classifyMatInfos[i].Count <= 1)
                {
                    classifyMatInfos[i].Clear();
                    continue;
                }

                List<MaterialInfo> duplicateMatInfos = classifyMatInfos[i];
                MaterialInfo mainInfo = GetMainMaterial(duplicateMatInfos);

                for (int j = 0; j < prefabPath.Length; j++)
                {
                    string prefabSerializeOrigin = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), prefabPath[j]));
                    string prefabSerializeModify = prefabSerializeOrigin;
                    for (int k = 0; k < duplicateMatInfos.Count; k++)
                    {
                        prefabSerializeModify = prefabSerializeModify.Replace(duplicateMatInfos[k].guid, mainInfo.guid);
                    }

                    if (prefabSerializeModify != prefabSerializeOrigin)
                    {
                        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), prefabPath[j]), prefabSerializeModify);
                    }
                }

            }
        }

        private static MaterialInfo GetMainMaterial(List<MaterialInfo> materialInfos)
        {
            MaterialInfo mainMat = null;
            for (int i = 0; i < materialInfos.Count; i++)
            {
                if (materialInfos[i].mat.mainTexture == null)
                {
                    continue;
                }

                if (String.Compare(materialInfos[i].mat.mainTexture.name, materialInfos[i].mat.name, StringComparison.Ordinal) == 0)
                {
                    mainMat = materialInfos[i];
                    break;
                }
            }

            if (mainMat == null)
            {
                mainMat = materialInfos[0];
            }

            mainMat.isMainMat = true;
            return mainMat;
        }

        private static void ClearnDuplicateMaterial(List<List<MaterialInfo>> classifyMatInfos)
        {
            for (int i = 0; i < classifyMatInfos.Count; i++)
            {
                for (int j = 0; j < classifyMatInfos[i].Count; j++)
                {
                    if (classifyMatInfos[i][j].isMainMat != true)
                    {
                        Resources.UnloadAsset(classifyMatInfos[i][j].mat);
                        AssetDatabase.DeleteAsset(classifyMatInfos[i][j].assetPath);
                    }
                }
            }
        }

        public class MaterialInfo
        {
            public string guid;
            public string matName;
            public string assetPath;
            public Material mat;
            public string matSerializeMat;
            public bool isMainMat = false;

            public MaterialInfo(string assetPath)
            {
                matName = Path.GetFileNameWithoutExtension(assetPath);
                this.assetPath = assetPath;
                mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                matSerializeMat = SetSerializeMat();
                guid = AssetDatabase.AssetPathToGUID(assetPath);
            }

            private string SetSerializeMat()
            {
                string temp = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
                int statIndex = temp.IndexOf("Material:", StringComparison.Ordinal);
                temp = temp.Substring(statIndex);
                temp = temp.Replace($"  m_Name: {matName}", "");
                int endIndex = temp.IndexOf("---", StringComparison.Ordinal);
                if (endIndex != -1)
                    temp = temp.Substring(0, endIndex);
                return temp;
            }
        }
    }
}
#endif 