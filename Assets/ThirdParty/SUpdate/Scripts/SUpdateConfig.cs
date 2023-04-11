using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SFramework
{
    /// <summary>
    /// 此文件仅在整包打包前可修改，打包后仅可修改ScriptableObject asset
    /// </summary>
    public  class SUpdateConfig : ScriptableObject
    {
#if UNITY_EDITOR
        public static SUpdateConfig GetConfig()
        {
            string [] paths= UnityEditor.AssetDatabase.FindAssets("SUpdateConfig");
            if (paths.Length==0)
            {
                Debug.LogError("未找到SUpdateConfig");
                return null;
            }
            
            SUpdateConfig sf=   UnityEditor.AssetDatabase.LoadAssetAtPath<SUpdateConfig>(UnityEditor.AssetDatabase.GUIDToAssetPath(paths[0]));
            return sf;
        }
        
        
        #region Proto
        [Header("Proto(Editor Only))")]
        [FolderPath] public string protoCsDir = "Assets/SDevelop/HotfixScripts/ProtoCS";

        [FilePath] public string protogen = "Protoc_3.4.0_bin/tool/protoc.exe";
        [FolderPath]
        public string protoDir = "Protoc_3.4.0_bin";
        #endregion
#endif
        
        
        #region HybridCLR
        [Header("HybridCLR")]
        [FolderPath] public string aotAndHotFixAssembliesDstDir = "Assets/SDevelop/Dll";


        public  List<string> hotfixAssemblyFiles = new List<string>()
        {
            "Hotfix.Develop.dll",
        };
        public List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
            "UniTask.dll",
            "LitJson.dll",
            "Google.Protobuf.dll"
        };
        #endregion
       

    }
}