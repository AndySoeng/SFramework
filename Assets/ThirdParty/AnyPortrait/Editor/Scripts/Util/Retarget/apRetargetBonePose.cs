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
	public class apRetargetBonePose
	{

		// Members
		//-----------------------------------------------------
		// Pose의 기본 정보를 저장한다.
		// 나중에 리스트의 메타 정보가 될 수 있다.
		public string _poseName = "";
		public string _description = "";
		public string _sceneName = "";
		public string _portraitName = "";
		public string _meshGroupName = "";
		public int _meshGroupUniqueID = -1;

		// Bone의 현재 정보들을 저장한다.
		//저장할 때에는 모든 Bone이 등록된다.
		//로드할 때에는 저장된 Bone만 보인다.
		public List<apRetargetBonePoseUnit> _bones = new List<apRetargetBonePoseUnit>();

		//로딩 정보
		//private bool _isLoaded = false;
		//private string _filePath = "";


		// Init
		//-----------------------------------------------------
		public apRetargetBonePose()
		{
			Clear();
		}

		public void Clear()
		{
			_poseName = "";
			_description = "";
			_sceneName = "";
			_portraitName = "";
			_meshGroupName = "";

			_bones.Clear();

			//_isLoaded = false;
			//_filePath = "";
		}


		// Functions
		//-----------------------------------------------------
		public void SetPose(apMeshGroup targetMeshGroup, apBone selectedBone, string sceneName)
		{
			Clear();

			_poseName = "<No Name Pose>";
			_description = "<No Description>";
			_sceneName = sceneName;
			_portraitName = targetMeshGroup._parentPortrait.name;
			_meshGroupName = targetMeshGroup._name;
			_meshGroupUniqueID = targetMeshGroup._uniqueID;

			List<apBone> rootBones = new List<apBone>();

			//<BONE_EDIT>
			//for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			//{
			//	rootBones.Add(targetMeshGroup._boneList_Root[i]);
			//}

			//>>Bone Set으로 변경
			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = targetMeshGroup._boneListSets[iSet];
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					rootBones.Add(boneSet._bones_Root[iRoot]);
				}
			}


			rootBones.Sort(delegate (apBone a, apBone b)
			{
				return b._depth - a._depth;
			});

			int curUnitID = 0;
			for (int i = 0; i < rootBones.Count; i++)
			{
				curUnitID = SetBonesRecursive(rootBones[i], selectedBone, curUnitID);
			}

			//for (int i = 0; i < targetMeshGroup._boneList_All.Count; i++)
			//{
			//	apBone srcBone = targetMeshGroup._boneList_All[i];

			//	apRetargetBonePoseUnit dstPoseBone = new apRetargetBonePoseUnit();

			//	dstPoseBone.SetBone(i, srcBone);

			//	if(selectedBone == srcBone)
			//	{
			//		//이건 일단 Export를 건다
			//		dstPoseBone._isExported = true;

			//	}
			//	_bones.Add(dstPoseBone);
			//}

			//Sort를 하자
			//_bones.Sort(delegate (apRetargetBonePoseUnit a, apRetargetBonePoseUnit b)
			//{
			//	if (a._targetBone._level == b._targetBone._level)
			//	{
			//		return b._targetBone._depth - a._targetBone._depth;
			//	}

			//	return a._targetBone._level * 1000 - b._targetBone._level * 1000;

			//	//return b._targetBone._depth - a._targetBone._depth;
			//});
		}

		private int SetBonesRecursive(apBone srcBone, apBone selectedBone, int unitID)
		{
			apRetargetBonePoseUnit dstPoseBone = new apRetargetBonePoseUnit();

			dstPoseBone.SetBone(unitID, srcBone);
			unitID++;

			if (selectedBone != null && selectedBone == srcBone)
			{
				//이건 일단 Export를 건다
				dstPoseBone._isExported = true;

			}
			_bones.Add(dstPoseBone);


			if(srcBone._childBones != null | srcBone._childBones.Count > 0)
			{
				List<apBone> childBones = new List<apBone>();
				for (int i = 0; i < srcBone._childBones.Count; i++)
				{
					childBones.Add(srcBone._childBones[i]);
				}

				childBones.Sort(delegate (apBone a, apBone b)
				{
					return b._depth - a._depth;
				});

				for (int i = 0; i < childBones.Count; i++)
				{
					unitID = SetBonesRecursive(childBones[i], selectedBone, unitID);
				}
				
			}

			return unitID;
		}




		public string SavePoseFile(string baseFolderPath)
		{
			FileStream fs = null;
			StreamWriter sw = null;

			try
			{
				//폴더가 없다면 만들자
				DirectoryInfo di = new DirectoryInfo(baseFolderPath);
				if(!di.Exists)
				{
					di.Create();
				}

				string fileName = _sceneName + "_" + _portraitName + "_" + _poseName;

				fileName = fileName.Replace("\\", "");
				fileName = fileName.Replace("/", "");
				fileName = fileName.Replace("*", "");
				fileName = fileName.Replace("?", "");
				fileName = fileName.Replace("\"", "");
				fileName = fileName.Replace("<", "");
				fileName = fileName.Replace(">", "");
				fileName = fileName.Replace("|", "");

				//파일이 겹친다면 넘버링을 붙이자.
				FileInfo fi = new FileInfo(baseFolderPath + "/" + fileName + ".pos");//파일 경로 문제가 발생하지 않는다. (21.9.10)
				if(fi.Exists)
				{
					//넘버링을 붙여서 루프를 돌자
					int iNumber = 1;
					while (true)
					{	
						string copyFileName = fileName + " (" + iNumber + ").pos";

						fi = new FileInfo(copyFileName);//파일 경로 문제가 발생하지 않는다. (21.9.10)
						if(!fi.Exists)
						{
							//어라 겹치는게 없네염
							fileName = fileName + " (" + iNumber + ")";
							break;
						}

						iNumber++;

						if(iNumber > 100)
						{
							EditorUtility.DisplayDialog("Save Error", "Pose Name is invalid", "Okay");
							return null;
						}
					}
				}

				//이전
				//fs = new FileStream(baseFolderPath + "/" + fileName + ".pos", FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(baseFolderPath + "/" + fileName + ".pos"), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);


				sw.WriteLine("--------------------------");
				sw.WriteLine(" Pose Data ");
				sw.WriteLine("--------------------------");
				sw.WriteLine(_poseName);
				sw.WriteLine(_description);
				sw.WriteLine(_sceneName);
				sw.WriteLine(_portraitName);
				sw.WriteLine(_meshGroupName);
				sw.WriteLine(_meshGroupUniqueID);

				//Bone 요약도 하자
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				int nBones = 0;
				for (int i = 0; i < _bones.Count; i++)
				{
					if (_bones[i]._isExported)
					{	
						if(nBones > 0)
						{
							sb.Append(", ");
						}
						sb.Append(_bones[i]._name);
						nBones++;
					}
				}

				sw.WriteLine(nBones);
				sw.WriteLine(sb.ToString());



				for (int i = 0; i < _bones.Count; i++)
				{
					if (_bones[i]._isExported)
					{
						sw.WriteLine(_bones[i].GetEncodingData());
					}
				}

				sw.Flush();

				sw.Close();
				fs.Close();

				sw = null;
				fs = null;

				return fileName + ".pos";

			}
			catch(Exception ex)
			{
				if(sw != null)
				{
					sw.Close();
					sw = null;
				}

				if (fs != null)
				{
					fs.Close();
					fs = null;
				}

				Debug.LogError("SavePoseFile Exception : " + ex);
			}

			return null;
		}


		public bool LoadPoseFile(string filePath)
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

				sr.ReadLine();//"-------------"
				sr.ReadLine();//"  Pose Data  "
				sr.ReadLine();//"-------------"
				_poseName = sr.ReadLine();
				_description = sr.ReadLine();
				_sceneName = sr.ReadLine();
				_portraitName = sr.ReadLine();
				_meshGroupName = sr.ReadLine();
				_meshGroupUniqueID = int.Parse(sr.ReadLine());

				sr.ReadLine();//Bone Count;
				sr.ReadLine();//Bone Names

				while(true)
				{
					if(sr.Peek() < 0)
					{
						break;
					}

					apRetargetBonePoseUnit newUnit = new apRetargetBonePoseUnit();
					newUnit.DecodeData(sr.ReadLine());

					_bones.Add(newUnit);
				}


				sr.Close();
				fs.Close();
			}
			catch (Exception ex)
			{
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
				Debug.LogError("LoadPoseFileException : " + ex);
				return false;
			}

			return true;
		}

		// Get / Set
		//-----------------------------------------------------

	}
}