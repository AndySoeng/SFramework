using System.Collections.Generic;
using System.IO;
using Ex;
using UnityEditor;
using UnityEngine;

namespace SFramework
{
    public static class GenProtoCs
    {
        [MenuItem("Tools/STools/GenProtoCs/Gen(Win)")]
        public static void GenProtoCsFile()
        {
            SUpdateConfig sUpdateConfig = SUpdateConfig.GetConfig();

            string protogen = Directory.GetParent(UnityEngine.Application.dataPath) + "/" + sUpdateConfig.protogen;
            string protoDir = Directory.GetParent(UnityEngine.Application.dataPath) + "/" + sUpdateConfig.protoDir;
            string outDir = Directory.GetParent(Application.dataPath) + "/" + sUpdateConfig.protoCsDir;
            
            List<string> cmds = new List<string>();
            if (!Directory.Exists(protoDir))
            {
                UnityEngine.Debug.LogError($"不存在Proto文件夹.{protoDir}");
                return;
            }

            DirectoryInfo folder = new DirectoryInfo(protoDir); // Proto所在路径
            FileInfo[] files = folder.GetFiles("*.proto");
            if (!Directory.Exists(outDir)) // CSharp 输出路径
            {
                Directory.CreateDirectory(outDir);
            }

            foreach (FileInfo file in files)
            {
                string cmd = protogen + " --csharp_out=" + outDir + " -I " + protoDir + " " + file.FullName;
                cmds.Add(cmd);
            }

            cmds.Add("exit");


            ExCmd.Cmd(cmds);
            AssetDatabase.Refresh();
        }
    }
}