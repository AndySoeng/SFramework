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

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using AnyPortrait;

namespace AnyPortrait
{
	public class apRetargetBaseFile
	{
		// Members
		//-----------------------------------------------
		public List<apRetargetBoneUnit> _boneUnits = new List<apRetargetBoneUnit>();
		private bool _isLoaded = false;
		private string _filePath = "";

		// Init
		//-----------------------------------------------
		public apRetargetBaseFile()
		{
			Clear();
		}


		public void Clear()
		{
			_isLoaded = false;
			_filePath = "";
			_boneUnits.Clear();
		}

		// Functions
		//-----------------------------------------------
		public static bool SaveBaseStruct(apMeshGroup meshGroup, string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return false;
			}
			if (meshGroup == null)
			{
				return false;
			}

			if (meshGroup._boneList_Root == null || meshGroup._boneList_Root.Count == 0)
			{
				return false;
			}

			//Bone UnitqueID -> ReatargetUnitID로 변환하는 맵을 만들자
			Dictionary<int, int> boneID2UnitID = new Dictionary<int, int>();
			int curUnitID = 0;
			for (int i = 0; i < meshGroup._boneList_All.Count; i++)
			{
				int boneID = meshGroup._boneList_All[i]._uniqueID;
				boneID2UnitID.Add(boneID, curUnitID);
				curUnitID++;
			}

			//ID Map을 이용해서 하나씩 만들어보자
			List<apRetargetBoneUnit> boneUnits = new List<apRetargetBoneUnit>();

			for (int i = 0; i < meshGroup._boneList_All.Count; i++)
			{
				apBone srcBone = meshGroup._boneList_All[i];
				apRetargetBoneUnit newBoneUnit = new apRetargetBoneUnit();
				int boneUnitID = boneID2UnitID[srcBone._uniqueID];

				newBoneUnit.SetBone(boneUnitID, srcBone, boneID2UnitID);

				boneUnits.Add(newBoneUnit);
			}

			FileStream fs = null;
			StreamWriter sw = null;
			//파일로 저장하자
			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				sw.WriteLine("----------------------------------------------");
				sw.WriteLine("Base Bone Structure");
				sw.WriteLine(boneUnits.Count.ToString());
				sw.WriteLine("----------------------------------------------");

				for (int i = 0; i < boneUnits.Count; i++)
				{
					sw.WriteLine(boneUnits[i].GetEncodingData());
				}

				sw.Flush();
				sw.Close();
				fs.Close();
				sw = null;
				fs = null;

				
			}
			catch (Exception ex)
			{
				Debug.LogError("Save Base Struct Exception : " + ex);

				if(sw != null)
				{
					sw.Close();
					sw = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}

				return false;
			}
			


			return true;
		}


		public bool LoadBaseStruct(string filePath)
		{
			Clear();

			FileStream fs = null;
			StreamReader sr = null;
			try
			{
				//이전
				//fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				//sr = new StreamReader(fs);

				//변경 21.7.3 : 경로 문제와 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(filePath), FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);

				sr.ReadLine();//"-----";
				sr.ReadLine();//"Base Bone Structure";
				int nBones = int.Parse(sr.ReadLine());
				sr.ReadLine();//"-----";

				while(true)
				{
					if(sr.Peek() < 0)
					{
						break;
					}
					if(_boneUnits.Count > nBones)
					{
						break;
					}

					string strData = sr.ReadLine();
					apRetargetBoneUnit newBoneUnit = new apRetargetBoneUnit();
					newBoneUnit.DecodeData(strData);
					_boneUnits.Add(newBoneUnit);
				}


				sr.Close();
				fs.Close();

				sr = null;
				fs = null;

				_filePath = filePath;
				_isLoaded = true;
			}
			catch (Exception ex)
			{
				Debug.LogError("Load Base Struct Exception : " + ex);

				if (sr != null)
				{
					sr.Close();
					sr = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}
				return false;
			}

			return true;
		}


		// Get / Set
		//-----------------------------------------------
		public bool IsLoaded {  get { return _isLoaded; } }
		public string LoadFilePath { get { return _filePath; } }
	}
}