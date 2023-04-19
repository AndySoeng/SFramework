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
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apSnapShotStackUnit
	{
		// Members
		//-------------------------------------------
		private apStringWrapper _str_Name = null;
		
		public apSnapShotBase _snapShot = null;
		public bool _isDataSaved = false;

		public string Name
		{
			get
			{
				return _str_Name.ToString();
			}
		}

		// Init
		//-------------------------------------------
		public apSnapShotStackUnit()
		{
			_str_Name = new apStringWrapper(128);
			_str_Name.Clear();
			_isDataSaved = false;
			_snapShot = null;
		}

		public void Clear()
		{
			if(_snapShot != null)
			{
				_snapShot.Clear();
			}
			_isDataSaved = false;
			_str_Name.Clear();
		}

		public void SetName(string unitName)
		{
			_str_Name.SetText(unitName);
		}

		

		// Set Snapshot
		//---------------------------------------------------------------------------
		//삭제 21.3.19 : 안쓰는 복붙 함수들과 클래스는 삭제
		//public bool SetSnapShot_Mesh(apMesh mesh, string strParam)
		//{
		//	_snapShot = new apSnapShot_Mesh();
		//	return _snapShot.Save(mesh, strParam);
		//}

		//public bool SetSnapShot_MeshGroup(apMeshGroup meshGroup, string strParam)
		//{
		//	_snapShot = new apSnapShot_MeshGroup();
		//	return _snapShot.Save(meshGroup, strParam);
		//}

		//public bool SetSnapShot_Portrait(apPortrait portrait, string strParam)
		//{
		//	_snapShot = new apSnapShot_Portrait();
		//	return _snapShot.Save(portrait, strParam);
		//}

		public bool SetSnapShot_ModMesh(apModifiedMesh modMesh, string strParam)
		{
			//변경 21.3.19 : 매번 생성하는 코드에서 Clear로 재활용
			if(_snapShot == null)
			{
				_snapShot = new apSnapShot_ModifiedMesh();
			}
			else
			{
				_snapShot.Clear();
			}
			
			_isDataSaved = _snapShot.Save(modMesh, strParam);
			return _isDataSaved;
		}

		public bool SetSnapShot_Keyframe(apAnimKeyframe keyframe, string strParam)
		{
			if(_snapShot == null)
			{
				_snapShot = new apSnapShot_Keyframe();
			}
			else
			{
				_snapShot.Clear();
			}
			
			_isDataSaved = _snapShot.Save(keyframe, strParam);
			return _isDataSaved;
		}

		public bool SetSnapShot_VertRig(apModifiedVertexRig vertRig, string strParam)
		{
			if(_snapShot == null)
			{
				_snapShot = new apSnapShot_VertRig();
			}
			else
			{
				_snapShot.Clear();
			}
			
			_isDataSaved = _snapShot.Save(vertRig, strParam);
			return _isDataSaved;
		}

		public bool SetSnapShot_ModBone(apModifiedBone modBone, string strParam)
		{
			if(_snapShot == null)
			{
				_snapShot = new apSnapShot_ModifiedBone();
			}
			else
			{
				_snapShot.Clear();
			}
			
			_isDataSaved = _snapShot.Save(modBone, strParam);
			return _isDataSaved;
		}

		// Functions
		//-------------------------------------------
		/// <summary>
		/// Load / Paste가 가능한 "동기화 가능한" 객체인가
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool IsKeySyncable(object target)
		{
			if (_snapShot == null || !_isDataSaved)
			{
				return false;
			}

			return _snapShot.IsKeySyncable(target);
		}

		public bool IsKeySyncable_MorphMod(object target)
		{
			if (_snapShot == null || !_isDataSaved)
			{
				return false;
			}

			return _snapShot.IsKeySyncable_MorphMod(target);
		}

		public bool IsKeySyncable_TFMod(object target)
		{
			if (_snapShot == null || !_isDataSaved)
			{
				return false;
			}

			return _snapShot.IsKeySyncable_TFMod(target);
		}




		public bool Load(object targetObj)
		{
			if(_snapShot == null || !_isDataSaved)
			{
				return false;
			}
			return _snapShot.Load(targetObj);
		}

		public bool LoadWithProperties(object targetObj,
												bool isVerts,
												bool isPins,
												bool isTransform,
												bool isVisibility,
												bool isColor,
												bool isExtra,
												bool isSelectedOnly,
												List<apModifiedVertex> modVerts,
												List<apModifiedPin> modPins)
		{
			if(_snapShot == null || !_isDataSaved)
			{
				return false;
			}
			return _snapShot.LoadWithProperties(targetObj,
												isVerts,
												isPins,
												isTransform,
												isVisibility,
												isColor,
												isExtra,
												isSelectedOnly,
												modVerts,
												modPins);
		}



		// Get / Set
		//-------------------------------------------
	}

}