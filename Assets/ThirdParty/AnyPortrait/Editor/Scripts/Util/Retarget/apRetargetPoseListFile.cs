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
	/// <summary>
	/// Pose를 저장할때, Pose의 파일 정보를 가진 클래스.
	/// 파일 이름, 정보, 실제 존재 여부, 저장 당시의 Scene과 Portrait, MeshGroup의 이름 등을 저장한다.
	/// 메타 파일의 역할을 한다.
	/// </summary>
	public class apRetargetPoseListFile
	{
		// Members
		//--------------------------------------------------------------
		public class FileMetaData
		{
			public string _filePath = "";
			public string _poseName = "";
			public string _description = "";
			public string _sceneName = "";
			public string _portraitName = "";
			public string _meshGroupName = "";
			public int _meshGroupUniqueID = -1;

			public int _nBones = 0;
			public string _boneNames = "";
			

			public FileMetaData(string filePath, string poseName, string description,
								string sceneName, string portraitName, string meshGroupName, int meshGroupUniqueID,
								int nBones, string boneNames)
			{
				_filePath = filePath;
				_poseName = poseName;
				_description = description;
				_sceneName = sceneName;
				_portraitName = portraitName;
				_meshGroupName = meshGroupName;
				_meshGroupUniqueID = meshGroupUniqueID;
				_nBones = nBones;
				_boneNames = boneNames;
			}

			
		}


		public List<FileMetaData> _metaDataList = new List<FileMetaData>();
		public bool _isFolderExist = false;

		

		// Init
		//--------------------------------------------------------------
		public apRetargetPoseListFile()
		{
			_isFolderExist = false;
		}

		public void Clear()
		{
			_metaDataList.Clear();
		}


		// Functions
		//--------------------------------------------------------------
		public void LoadList(string folderPath)
		{
			Clear();
			DirectoryInfo di = new DirectoryInfo(folderPath);
			if(!di.Exists)
			{
				//존재하지 않는 리스트
				_isFolderExist = false;
				return;
			}

			_isFolderExist = true;

			FileInfo[] fileInfos = di.GetFiles("*.pos", SearchOption.TopDirectoryOnly);

			if(fileInfos == null)
			{
				return;
			}

			FileInfo fi = null;
			for (int i = 0; i < fileInfos.Length; i++)
			{
				fi = fileInfos[i];

				FileStream fs = null;
				StreamReader sr = null;
				try
				{
					//이전
					//fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
					//sr = new StreamReader(fs);

					//변경 21.7.3 : 경로 문제와 인코딩 문제
					fs = new FileStream(apUtil.ConvertEscapeToPlainText(fi.FullName), FileMode.Open, FileAccess.Read);
					sr = new StreamReader(fs, System.Text.Encoding.UTF8, true);



					sr.ReadLine();//"----------------"
					sr.ReadLine();//" Pose Data		 "
					sr.ReadLine();//"----------------"

					string filePath = fi.FullName;
					string poseName = sr.ReadLine();
					string description = sr.ReadLine();
					string sceneName = sr.ReadLine();
					string portraitName = sr.ReadLine();
					string meshGroupName = sr.ReadLine();
					int meshGroupUniqueID = int.Parse(sr.ReadLine());
					int nBones = int.Parse(sr.ReadLine());
					string boneNames = sr.ReadLine();

					_metaDataList.Add(new FileMetaData(	filePath, 
														poseName, description, 
														sceneName, portraitName, meshGroupName, meshGroupUniqueID, 
														nBones, boneNames));

					sr.Close();
					fs.Close();

					sr = null;
					fs = null;

				}
				catch (Exception ex)
				{
					Debug.LogError("LoadList Exception : " + ex);

					if(sr != null)
					{
						sr.Close();
						sr = null;
					}

					if(fs != null)
					{
						fs.Close();
						fs = null;
					}
				}
			}
		}


		#region [미사용 코드]
		//public void Load(string folderPath)
		//{
		//	FileStream fs = null;
		//	StreamReader sr = null;


		//	try
		//	{
		//		Clear();

		//		fs = new FileStream(folderPath + "/PoseList.lst", FileMode.Open, FileAccess.Read);
		//		sr = new StreamReader(fs);

		//		sr.ReadLine();//"------"
		//		sr.ReadLine();//" Pose List "
		//		sr.ReadLine();//"------"

		//		while(true)
		//		{
		//			//하나씩 돌면서 메타 데이터를 넣자
		//			if(sr.Peek() < 0)
		//			{
		//				break;
		//			}

		//			string strData = sr.ReadLine();

		//			string strFilePath = GetDecodedString(strData);
		//			strData = strData.Substring(3 + strFilePath.Length);

		//			string strPoseName = GetDecodedString(strData);
		//			strData = strData.Substring(3 + strPoseName.Length);

		//			string strDesc = GetDecodedString(strData);
		//			strData = strData.Substring(3 + strDesc.Length);

		//			string strSceneName = GetDecodedString(strData);
		//			strData = strData.Substring(3 + strSceneName.Length);

		//			string strPortraitName = GetDecodedString(strData);
		//			strData = strData.Substring(3 + strPortraitName.Length);

		//			string strMeshGroupName = GetDecodedString(strData);

		//			FileMetaData newMeta = new FileMetaData(strFilePath, strPoseName, strDesc, strSceneName, strPortraitName, strMeshGroupName);

		//			//만약 이름이 겹치는게 있으면 스킵
		//			if(GetMetaData(newMeta._filePath) != null)
		//			{
		//				continue;
		//			}

		//			_metaDataList.Add(newMeta);
		//		}

		//		sr.Close();
		//		fs.Close();

		//		sr = null;
		//		fs = null;
		//	}
		//	catch (Exception ex)
		//	{
		//		if (sr != null)
		//		{
		//			sr.Close();
		//			sr = null;
		//		}

		//		if (fs != null)
		//		{
		//			fs.Close();
		//			fs = null;
		//		}

		//		Save(folderPath);
		//	}
		//}


		//public void Save(string folderPath)
		//{
		//	FileStream fs = null;
		//	StreamWriter sw = null;

		//	try
		//	{
		//		fs = new FileStream(folderPath + "/PoseList.lst", FileMode.Create, FileAccess.Write);
		//		sw = new StreamWriter(fs);

		//		sw.WriteLine("-------------------------------");
		//		sw.WriteLine(" Pose List ");
		//		sw.WriteLine("-------------------------------");
		//		for (int i = 0; i < _metaDataList.Count; i++)
		//		{
		//			FileMetaData metaUnit = _metaDataList[i];

		//			System.Text.StringBuilder sb = new System.Text.StringBuilder();
		//			sb.Append(GetEncodedString(metaUnit._filePath));
		//			sb.Append(GetEncodedString(metaUnit._poseName));
		//			sb.Append(GetEncodedString(metaUnit._description));
		//			sb.Append(GetEncodedString(metaUnit._sceneName));
		//			sb.Append(GetEncodedString(metaUnit._portraitName));
		//			sb.Append(GetEncodedString(metaUnit._meshGroupName));

		//			sw.WriteLine(sb.ToString());
		//		}


		//		sw.Close();
		//		fs.Close();

		//		sw = null;
		//		fs = null;
		//	}
		//	catch (Exception)
		//	{
		//		if (sw != null)
		//		{
		//			sw.Close();
		//			sw = null;
		//		}

		//		if (fs != null)
		//		{
		//			fs.Close();
		//			fs = null;
		//		}

		//	}
		//}


		///// <summary>
		///// 실제로 폴더에 있는 데이터를 확인하면서 
		///// </summary>
		//public void RefreshFileInfo(string folderPath)
		//{
		//	DirectoryInfo di = new DirectoryInfo(folderPath);

		//	for (int i = 0; i < _metaDataList.Count; i++)
		//	{
		//		_metaDataList[i]._isExist = false;
		//	}

		//	if(!di.Exists)
		//	{
		//		//존재하지 않는 폴더
		//		return;
		//	}

		//	FileInfo[] fileInfos = di.GetFiles("*.pos");
		//	if(fileInfos == null || fileInfos.Length == 0)
		//	{
		//		return;
		//	}

		//	for (int i = 0; i < fileInfos.Length; i++)
		//	{
		//		FileInfo fi = fileInfos[i];
		//		Debug.Log("[" + i + "] Full Name : " + fi.FullName);//이거 맞는지 체크하자
		//		//TODO

		//	}
		//} 
		#endregion


		public void RemoveFile(FileMetaData metaData)
		{
			if(metaData == null)
			{
				return;
			}

			//추가 21.9.10 : 경로가 공백인 경우
			if(string.IsNullOrEmpty(metaData._filePath))
			{
				return;
			}
			FileInfo fi = new FileInfo(metaData._filePath);//경로 체크 했음 (21.9.10)
			if(fi.Exists)
			{
				fi.Delete();
			}

		}
		// Get / Set
		//--------------------------------------------------------------
		public FileMetaData GetMetaData(string filePath)
		{
			return _metaDataList.Find(delegate (FileMetaData a)
			{
				return string.Equals(filePath, a._filePath);
			});
		}



		public string GetEncodedString(string strSrc)
		{
			if(string.IsNullOrEmpty(strSrc))
			{
				return "000";
			}

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if(strSrc.Length < 10)
			{
				sb.Append("00");
				sb.Append(strSrc.Length);
			}
			else if(strSrc.Length < 100)
			{
				sb.Append("0");
				sb.Append(strSrc.Length);
			}
			else if(strSrc.Length < 100)
			{
				sb.Append("0");
				sb.Append(strSrc.Length);
			}
			sb.Append(strSrc);

			return sb.ToString();
		}

		public string GetDecodedString(string strSrc)
		{
			int strLength = int.Parse(strSrc.Substring(0, 3));
			if(strLength == 0)
			{
				return "";
			}

			return strSrc.Substring(3, strLength);
		}
	}
}