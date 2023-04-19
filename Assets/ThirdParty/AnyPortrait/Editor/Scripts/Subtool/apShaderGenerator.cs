/*
*	Copyright (c) 2017-2023. RainyRizzle Inc. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee] of [RainyRizzle team].
*
*	It is illegal to download files from other than the Unity Asset Store and RainyRizzle homepage.
*	In that case, the act could be subject to legal sanctions.
*/

//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System;
//using System.Collections.Generic;
//using System.IO;

//using AnyPortrait;

//namespace AnyPortrait
//{
//	/// <summary>
//	/// LWRP Shader를 만들어주는 객체. 필요할때 생성하고 함수를 호출하면 된다.
//	/// </summary>
//	public class apShaderGenerator
//	{
//		// Members
//		//------------------------------------------------
//		public enum ShaderType
//		{
//			AlphaMask,
//			Transparent_Alpha,
//			Transparent_Add,
//			Transparent_Mul,
//			Transparent_SoftAdd,
//			Clipped_Alpha,
//			Clipped_Add,
//			Clipped_Mul,
//			Clipped_SoftAdd,
//			Linear_Transparent_Alpha,
//			Linear_Transparent_Add,
//			Linear_Transparent_Mul,
//			Linear_Transparent_SoftAdd,
//			Linear_Clipped_Alpha,
//			Linear_Clipped_Add,
//			Linear_Clipped_Mul,
//			Linear_Clipped_SoftAdd,
//		}
//		public class ShaderPair
//		{
//			public ShaderType _shaderType;
//			public Shader _shader_Normal;
//			public Shader _shader_LWRP;
//			public string _folderPath_Normal;
//			public string _folderPath_LWRP;
//			public string _fileName_Normal;
//			public string _fileName_LWRP;

//			public ShaderPair(ShaderType shaderType, 
//								string folderPath_Normal,
//								string fileName_Normal,
//								string folderPath_LWRP,
//								string fileName_LWRP)
//			{
//				_shaderType = shaderType;
//				_shader_Normal = null;
//				_shader_LWRP = null;
//				_folderPath_Normal = folderPath_Normal;
//				_folderPath_LWRP = folderPath_LWRP;
//				_fileName_Normal = fileName_Normal + ".shader";
//				_fileName_LWRP = fileName_LWRP + ".shader";
//			}

//			public void SetShader(Shader shader_Normal,	Shader shader_LWRP)
//			{
//				_shader_Normal = shader_Normal;
//				_shader_LWRP = shader_LWRP;
//			}
//		}

//		private Dictionary<ShaderType, ShaderPair> _shaderPairs = new Dictionary<ShaderType, ShaderPair>();

//		//하나라도 LWRP Shader가 없는 경우 True
//		private bool _isAnyMissingLWRP = false;


//		// Init
//		//------------------------------------------------
//		public apShaderGenerator()
//		{
//			_shaderPairs.Clear();
//			MakePair(ShaderType.AlphaMask,				ShaderPath, "apShader_AlphaMask",						ShaderPath_LWRP, "apShader_LWRP_AlphaMask");
//			MakePair(ShaderType.Transparent_Alpha,		ShaderPath, "apShader_Transparent",						ShaderPath_LWRP, "apShader_LWRP_Transparent");
//			MakePair(ShaderType.Transparent_Add,		ShaderPath, "apShader_Transparent_Additive",			ShaderPath_LWRP, "apShader_LWRP_Transparent_Additive");
//			MakePair(ShaderType.Transparent_Mul,		ShaderPath, "apShader_Transparent_Multiplicative",		ShaderPath_LWRP, "apShader_LWRP_Transparent_Multiplicative");
//			MakePair(ShaderType.Transparent_SoftAdd,	ShaderPath, "apShader_Transparent_SoftAdditive",		ShaderPath_LWRP, "apShader_LWRP_Transparent_SoftAdditive");
//			MakePair(ShaderType.Clipped_Alpha,			ShaderPath, "apShader_ClippedWithMask",					ShaderPath_LWRP, "apShader_LWRP_ClippedWithMask");
//			MakePair(ShaderType.Clipped_Add,			ShaderPath, "apShader_ClippedWithMask_Additive",		ShaderPath_LWRP, "apShader_LWRP_ClippedWithMask_Additive");
//			MakePair(ShaderType.Clipped_Mul,			ShaderPath, "apShader_ClippedWithMask_Multiplicative",	ShaderPath_LWRP, "apShader_LWRP_ClippedWithMask_Multiplicative");
//			MakePair(ShaderType.Clipped_SoftAdd,			ShaderPath, "apShader_ClippedWithMask_SoftAdditive",	ShaderPath_LWRP, "apShader_LWRP_ClippedWithMask_SoftAdditive");
//			MakePair(ShaderType.Linear_Transparent_Alpha,	ShaderPath_Linear, "apShader_L_Transparent",					ShaderPath_Linear_LWRP, "apShader_LWRP_L_Transparent");
//			MakePair(ShaderType.Linear_Transparent_Add,		ShaderPath_Linear, "apShader_L_Transparent_Additive",			ShaderPath_Linear_LWRP, "apShader_LWRP_L_Transparent_Additive");
//			MakePair(ShaderType.Linear_Transparent_Mul,		ShaderPath_Linear, "apShader_L_Transparent_Multiplicative",		ShaderPath_Linear_LWRP, "apShader_LWRP_L_Transparent_Multiplicative");
//			MakePair(ShaderType.Linear_Transparent_SoftAdd,	ShaderPath_Linear, "apShader_L_Transparent_SoftAdditive",		ShaderPath_Linear_LWRP, "apShader_LWRP_L_Transparent_SoftAdditive");
//			MakePair(ShaderType.Linear_Clipped_Alpha,		ShaderPath_Linear, "apShader_L_ClippedWithMask",				ShaderPath_Linear_LWRP, "apShader_LWRP_L_ClippedWithMask");
//			MakePair(ShaderType.Linear_Clipped_Add,			ShaderPath_Linear, "apShader_L_ClippedWithMask_Additive",		ShaderPath_Linear_LWRP, "apShader_LWRP_L_ClippedWithMask_Additive");
//			MakePair(ShaderType.Linear_Clipped_Mul,			ShaderPath_Linear, "apShader_L_ClippedWithMask_Multiplicative",	ShaderPath_Linear_LWRP, "apShader_LWRP_L_ClippedWithMask_Multiplicative");
//			MakePair(ShaderType.Linear_Clipped_SoftAdd,		ShaderPath_Linear, "apShader_L_ClippedWithMask_SoftAdditive",	ShaderPath_Linear_LWRP, "apShader_LWRP_L_ClippedWithMask_SoftAdditive");

//			Refresh();
//		}


//		// Functions
//		//------------------------------------------------
//		public void Refresh()
//		{
//			_isAnyMissingLWRP = false;
//			foreach (KeyValuePair<ShaderType, ShaderPair> shaderPair in _shaderPairs)
//			{
//				Shader shader_Normal = GetShaderAsset(shaderPair.Value._folderPath_Normal, shaderPair.Value._fileName_Normal);
//				Shader shader_LWRP = GetShaderAsset(shaderPair.Value._folderPath_LWRP, shaderPair.Value._fileName_LWRP);

//				shaderPair.Value.SetShader(shader_Normal, shader_LWRP);

//				if(shader_LWRP == null)
//				{
//					_isAnyMissingLWRP = true;
//				}
//			}
//		}


//		public void GenerateLWRPShaders()
//		{
//			foreach (KeyValuePair<ShaderType, ShaderPair> shaderPair in _shaderPairs)
//			{
//				GenerateShader(shaderPair.Key, true);
				
//			}
//			AssetDatabase.Refresh();
//			Refresh();
//		}


//		// Sub-Functions
//		//------------------------------------------------
//		private void MakePair(ShaderType shaderType, string folderPath_Normal, string fileName_Normal, string folderPath_LWRP, string fileName_LWRP)
//		{
//			ShaderPair newPair = new ShaderPair(shaderType, folderPath_Normal, fileName_Normal, folderPath_LWRP, fileName_LWRP);
//			_shaderPairs.Add(shaderType, newPair);
//		}


//		private Shader GetShaderAsset(string folderName, string fileName)
//		{
//			DirectoryInfo appDi = new DirectoryInfo(Application.dataPath);
//			string fullPath_Di = appDi.Parent.ToString() + "/" + folderName;

//			DirectoryInfo di = new DirectoryInfo(fullPath_Di);
//			if(!di.Exists)
//			{
//				return null;
//			}

//			FileInfo fi = new FileInfo(fullPath_Di + "/" + fileName);
//			if(!fi.Exists)
//			{
//				return null;
//			}

//			return AssetDatabase.LoadAssetAtPath<Shader>(folderName + "/" + fileName);
//		}


//		private bool GenerateShader(ShaderType shaderType, bool isOverwrite)
//		{
//			ShaderPair pair = _shaderPairs[shaderType];
//			Shader shader_Normal = pair._shader_Normal;
//			Shader shader_LWRP = pair._shader_LWRP;

//			if(shader_Normal == null)
//			{
//				//Source가 없다.
//				return false;
//			}
//			if(shader_LWRP != null && !isOverwrite)
//			{
//				//이미 있으며, 덮어쓸 수 없다.
//				return false;
//			}

//			DirectoryInfo appDi = new DirectoryInfo(Application.dataPath);
//			string projPath = appDi.Parent.ToString() + "/";

//			//1. 저장할 폴더가 있는지 확인. 없다면 만든다.
//			//2. 파일을 복사한다. 이미 있다면 삭제한다.
//			//3. 복사된 파일(LWRP)을 열고, 몇가지 코드를 수정한다.

//			FileStream fs_Src = null;
//			StreamReader sr_Src = null;
//			FileStream fs_Dst = null;
//			StreamWriter sw_Dst = null;

//			try
//			{
//				//폴더 확인 후 만들기
//				DirectoryInfo di_LWRP = new DirectoryInfo(projPath + pair._folderPath_LWRP);
//				if(!di_LWRP.Exists)
//				{
//					di_LWRP.Create();
//				}

//				FileInfo fi_Normal = new FileInfo(projPath + pair._folderPath_Normal + "/" + pair._fileName_Normal);
//				FileInfo fi_LWRP = new FileInfo(projPath + pair._folderPath_LWRP + "/" + pair._fileName_LWRP);
				
//				//fi_Normal.CopyTo(projPath + pair._folderPath_LWRP + "/" + pair._fileName_LWRP, isOverwrite);
//				fs_Src = new FileStream(fi_Normal.FullName, FileMode.Open, FileAccess.Read);
//				sr_Src = new StreamReader(fs_Src);

//				fs_Dst = new FileStream(fi_LWRP.FullName, FileMode.Create, FileAccess.Write);
//				sw_Dst = new StreamWriter(fs_Dst);
				
//				//하나씩 읽고 하나씩 적는다.
//				string strReadLine = null;
//				string strWriteLine = null;
//				while(true)
//				{
//					if(sr_Src.Peek() < 0)
//					{
//						break;
//					}

//					strReadLine = sr_Src.ReadLine();

//					//체크해야하는 것
//					//- Shader 이름 : Shader "AnyPortrait/Transparent => Shader "AnyPortrait/LWRP/Transparent
//					//- Tags : Tags{로 시작할 때 => 마지막 글자 } => [공백]"RenderPipeline" = "LightweightPipeline" "LightMode" = "LightweightForward"}
//					if(strReadLine.StartsWith("Shader ") && strReadLine.Contains("\"AnyPortrait/Transparent/")
//						)
//					{
//						//Shader 이름 부분이다.
//						strWriteLine = strReadLine.Replace("\"AnyPortrait/Transparent/", "\"AnyPortrait/LWRP/Transparent/");
//					}
//					else if((strReadLine.Contains("Tags {") || strReadLine.Contains("Tags{"))
//						&& strReadLine.Contains("\"RenderType\"")
//						&& strReadLine.Contains("}")
//						&& !strReadLine.Contains("LightweightPipeline")
//						&& !strReadLine.Contains("LightweightForward")
//						&& !strReadLine.Contains("//")
//						)
//					{
//						//Shader Tags 부분이다.
//						strWriteLine = strReadLine.Replace("}", " \"RenderPipeline\" = \"LightweightPipeline\" \"LightMode\" = \"LightweightForward\"}");
//					}
//					else
//					{
//						strWriteLine = strReadLine;//그대로 적용
//					}

//					sw_Dst.WriteLine(strWriteLine);
//				}

//				sw_Dst.Flush();

//				sr_Src.Close();
//				fs_Src.Close();
//				sw_Dst.Close();
//				fs_Dst.Close();
//				sr_Src = null;
//				fs_Src = null;
//				sw_Dst = null;
//				fs_Dst = null;
//			}
//			catch(Exception ex)
//			{
//				if(sr_Src != null)
//				{
//					sr_Src.Close();
//					sr_Src = null;
//				}
//				if(fs_Src != null)
//				{
//					fs_Src.Close();
//					fs_Src = null;
//				}

//				if(sw_Dst != null)
//				{
//					sw_Dst.Close();
//					sw_Dst = null;
//				}

//				if(fs_Dst != null)
//				{
//					fs_Dst.Close();
//					fs_Dst = null;
//				}
//				Debug.LogError("Generate Exception : " + ex);
//			}


//			return true;

//		}
//		// Get / Set
//		//------------------------------------------------
//		public static string ShaderPath	{ get { return "Assets/AnyPortrait/Assets/Shaders"; } }
//		public static string ShaderPath_LWRP	{ get { return "Assets/AnyPortrait/Assets/Shaders/LWRP"; } }
//		public static string ShaderPath_Linear	{ get { return "Assets/AnyPortrait/Assets/Shaders/Linear"; } }
//		public static string ShaderPath_Linear_LWRP	{ get { return "Assets/AnyPortrait/Assets/Shaders/Linear/LWRP"; } }

//		public bool IsAnyMissingLWRPShader {  get {  return _isAnyMissingLWRP; } }
//	}
//}
