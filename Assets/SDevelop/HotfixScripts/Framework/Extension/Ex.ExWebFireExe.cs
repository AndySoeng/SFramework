using System;
using Microsoft.Win32;
using UnityEngine;

namespace Ex
{
    public class Ex_ExWebFireExe
    {
        /// <summary>
        /// Web拉起Exe前，进行注册表
        /// </summary>
        /// <param name="strPrimaryKey">注册项名称（web使用此名称进行拉起）</param>
        /// <param name="strExePathName">软件路径</param>
        public static void Register(string strPrimaryKey, string strExePathName)
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot;
                RegistryKey regPrimaryKey = key.CreateSubKey(strPrimaryKey);
                regPrimaryKey.SetValue("", strPrimaryKey + " Protocol");
                regPrimaryKey.SetValue("URL Protocol", "");
                RegistryKey regDefaultIconKey = key.CreateSubKey(strPrimaryKey + "\\DefaultIcon");
                regDefaultIconKey.SetValue("", strExePathName + ",1");
                RegistryKey regshellKey = key.CreateSubKey(strPrimaryKey + "\\shell");
                RegistryKey regshellopenKey = key.CreateSubKey(strPrimaryKey + "\\shell\\open");
                RegistryKey regshellopencommandKey = key.CreateSubKey(strPrimaryKey + "\\shell\\open\\command");
                regshellopencommandKey.SetValue("", string.Format("\"{0}\" \"%1\"", strExePathName));
                key.Close();
            }
            catch (Exception ex)
            {
                Debug.Log(strPrimaryKey + "生成注册表失败：" + ex.Message);
            }
        }


        public static void UnRegister(string strPrimaryKey)
        {
            try
            {
                RegistryKey delKey = Registry.ClassesRoot;
                RegistryKey regPrimaryKey = delKey.OpenSubKey(strPrimaryKey, true);
                //判断要删除的regPrimaryKey是否存在
                if (regPrimaryKey != null)
                {
                    delKey.DeleteSubKeyTree(strPrimaryKey);
                }

                delKey.Close();
                Debug.Log("删除注册表成功！");
            }
            catch (Exception ex)
            {
                Debug.Log(strPrimaryKey + "删除注册表失败：" + ex.Message);
            }
        }
    }
}