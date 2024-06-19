#if  UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using Ex;
using UnityEditor;
using UnityEngine;

namespace SFramework
{
    public static class GenProtoCs
    {
        public static string protoCsDir = "Assets/SDevelop/HotfixScripts/ProtoCS";


        public static string protogen = "Protoc_3.4.0_bin/tool/protoc.exe";
        
        public static string protoDir = "Protoc_3.4.0_bin";
        [MenuItem("Tools/STools/GenProtoCs/Gen(Win)")]
        public static void GenProtoCsFile()
        {

            string protogenPath = Directory.GetParent(UnityEngine.Application.dataPath) + "/" + protogen;
            string protoDirPath = Directory.GetParent(UnityEngine.Application.dataPath) + "/" + protoDir;
            string outDir = Directory.GetParent(Application.dataPath) + "/" +protoCsDir;
            
            List<string> cmds = new List<string>();
            if (!Directory.Exists(protoDirPath))
            {
                UnityEngine.Debug.LogError($"不存在Proto文件夹.{protoDirPath}");
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(protoDirPath); // Proto所在路径
            FileInfo[] files = folder.GetFiles("*.proto");
            if (!Directory.Exists(outDir)) // CSharp 输出路径
            {
                Directory.CreateDirectory(outDir);
            }

            foreach (FileInfo file in files)
            {
                string cmd = protogenPath + " --csharp_out=" + outDir + " -I " + protoDirPath + " " + file.FullName;
                cmds.Add(cmd);
            }

            cmds.Add("exit");


            ExCmd.Cmd(cmds);
            AssetDatabase.Refresh();
        }
    }
}
#endif
