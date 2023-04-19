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
	/// 본 리타겟 데이터/파일을 저장하거나 여는 클래스.
	/// 1) 기본 구조, 2) 단일 포즈, 3) 애니메이션 파일 정보를 각각 저장할 수 있다.
	/// 1) 파일 저장과 2) 파일 열고 리타겟 매핑후 붙여넣기 정보를 가진다.
	/// 리타겟 매핑 정보도 저장한다.
	/// </summary>
	public class apRetarget
	{
		// Members
		//---------------------------------------------
		// 단일 포즈에 대한 데이터
		public apRetargetBaseFile _baseFile = null;
		public float _importScale = 1.0f;

		public apRetargetAnimFile _animFile = null;


		public apRetargetBonePose _singlePose = null;
		public apRetargetPoseListFile _singlePoseList = null;

		// Init
		//---------------------------------------------
		public apRetarget()
		{

		}


		// Functions
		//---------------------------------------------
		// 1. 기본 구조의 저장과 로드
		//---------------------------------------------
		public static bool SaveBaseStruct(apMeshGroup meshGroup, string filePath)
		{
			return apRetargetBaseFile.SaveBaseStruct(meshGroup, filePath);
		}

		public bool LoadBaseStruct(string filePath)
		{
			if(_baseFile == null)
			{
				_baseFile = new apRetargetBaseFile();
			}

			return _baseFile.LoadBaseStruct(filePath);
		}

		public bool IsBaseFileLoaded
		{
			get
			{
				if(_baseFile != null) { return _baseFile.IsLoaded; }
				else { return false; }
			}
		}


		//------------------------------------------------------
		// 2. 애니메이션의 저장과 로드
		//------------------------------------------------------
		public static bool SaveAnimClip(apAnimClip animClip, string filePath)
		{
			return apRetargetAnimFile.SaveAnimFile(animClip, filePath);
		}


		public bool LoadAnimClip(string filePath)
		{
			if(_animFile == null)
			{
				_animFile = new apRetargetAnimFile();
			}

			return _animFile.LoadAnimClip(filePath);
		}

		public bool IsAnimFileLoaded
		{
			get
			{
				if(_animFile != null) { return _animFile.IsLoaded; }
				return false;
			}
		}


		//---------------------------------------------------------
		// 3. 단일 포즈의 저장과 로드
		//---------------------------------------------------------
		public void SetSinglePose(apMeshGroup meshGroup, apBone selectedBone, string sceneName)
		{
			if(_singlePose == null)
			{
				_singlePose = new apRetargetBonePose();
			}

			_singlePose.Clear();
			_singlePose.SetPose(meshGroup, selectedBone, sceneName);
		}

		public string SaveSinglePose(string baseFolderPath)
		{
			if(_singlePose == null)
			{
				return null;
			}

			return _singlePose.SavePoseFile(baseFolderPath);

			//Application.OpenURL("file://" + baseFolderPath);
		}

		public bool LoadSinglePose(string filePath)
		{
			if(_singlePose == null)
			{
				_singlePose = new apRetargetBonePose();
			}

			return _singlePose.LoadPoseFile(filePath);
		}

		public void LoadSinglePoseFileList(apEditor editor)
		{
			if(_singlePoseList == null)
			{
				_singlePoseList = new apRetargetPoseListFile();
			}

			_singlePoseList.LoadList(Application.dataPath + "/../" + editor._bonePose_BaseFolderName);
		}
		


		public apRetargetPoseListFile SinglePoseList
		{
			get
			{
				if(_singlePoseList == null)
				{
					_singlePoseList = new apRetargetPoseListFile();
				}
				return _singlePoseList;
			}
		}


		// Get / Set
		//---------------------------------------------
		public List<apRetargetBoneUnit> BaseBoneUnits
		{
			get
			{
				if(IsBaseFileLoaded)
				{
					return _baseFile._boneUnits;
				}
				return null;
			}
		}

		public string BaseLoadedFilePath
		{
			get
			{
				if(IsBaseFileLoaded)
				{
					return _baseFile.LoadFilePath;
				}
				return "";

			}
		}



		public apRetargetAnimFile AnimFile
		{
			get
			{
				if(IsAnimFileLoaded)
				{
					return _animFile;
				}
				return null;
			}
		}

		public string AnimLoadedFilePath
		{
			get
			{
				if(IsAnimFileLoaded)
				{
					return _animFile.LoadFilePath;
				}
				return "";
			}
		}



		public apRetargetBonePose SinglePoseFile
		{
			get
			{
				return _singlePose;
			}
		}
		
	}
}