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
	/// MeshGroup의 Mesh/MeshGroup Transform과 Bone에 대한 기초 정보를 가지고 있다.
	/// Anim 리타겟의 기본 유닛이 된다.
	/// UnitID와 UniqueID를 모두 가지며 판별시 사용된다.
	/// TimelineLayer와 연동하기 위함이므로 자세한 데이터는 들어가지 않는다.
	/// </summary>
	public class apRetargetSubUnit
	{
		// Members
		//----------------------------------------------
		public enum TYPE
		{
			MeshTransform,
			MeshGroupTransform,
			Bone
		}

		public TYPE _type = TYPE.MeshTransform;

		public int _unitID = -1;
		public int _uniqueID = -1;

		public string _name = "";

		public apMatrix _defaultMatrix = new apMatrix();
		public Color _defaultColor = Color.white;
		public bool _isVisible = false;

		public int _parentUnitID = -1;
		public List<int> _childUnitIDs = new List<int>();

		public apRetargetSubUnit _parentUnit = null;
		public List<apRetargetSubUnit> _childUnits = new List<apRetargetSubUnit>();

		public int _sortIndex = -1;

		//Import 설정
		public bool _isImported = false;
		public apTransform_Mesh _targetMeshTransform = null;
		public apTransform_MeshGroup _targetMeshGroupTransform = null;
		public apBone _targetBone = null;



		// Init
		//----------------------------------------------
		public apRetargetSubUnit()
		{
			_isImported = false;
			_targetMeshTransform = null;
			_targetMeshGroupTransform = null;
			_targetBone = null;
		}
		

		// Functions
		//----------------------------------------------
		// TF/Bone -> File
		//----------------------------------------------
		public void SetSubData(int unitID, 
								apTransform_Mesh meshTransform, 
								apTransform_MeshGroup meshGroupTransform,
								apBone bone,
								apRetargetSubUnit parentRetargetUnit)
		{
			_unitID = unitID;

			_parentUnitID = -1;
			_childUnitIDs.Clear();

			_parentUnit = null;
			_childUnits.Clear();

			if(meshTransform != null)
			{
				_type = TYPE.MeshTransform;
				_uniqueID = meshTransform._transformUniqueID;

				_name = meshTransform._nickName;

				_defaultMatrix.SetMatrix(meshTransform._matrix, true);
				_defaultColor = meshTransform._meshColor2X_Default;
				_isVisible = meshTransform._isVisible_Default;
			}
			else if(meshGroupTransform != null)
			{
				_type = TYPE.MeshGroupTransform;
				_uniqueID = meshGroupTransform._transformUniqueID;

				_name = meshGroupTransform._nickName;

				_defaultMatrix.SetMatrix(meshGroupTransform._matrix, true);
				_defaultColor = meshGroupTransform._meshColor2X_Default;
				_isVisible = meshGroupTransform._isVisible_Default;
			}
			else if(bone != null)
			{
				_type = TYPE.Bone;
				_uniqueID = bone._uniqueID;

				_name = bone._name;

				_defaultMatrix.SetMatrix(bone._defaultMatrix, true);
				_defaultColor = Color.white;
				_isVisible = true;
			}
			else
			{
				Debug.LogError("Wrong Sub Unit");
				return;
			}

			if(parentRetargetUnit != null)
			{
				//Parent와 연결한다.

				_parentUnitID = parentRetargetUnit._unitID;
				_parentUnit = parentRetargetUnit;

				parentRetargetUnit._childUnitIDs.Add(_unitID);
				parentRetargetUnit._childUnits.Add(this);
			}
		}

		public string GetEncodingData()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if(_name.Length < 10)
			{
				sb.Append("00");	sb.Append(_name.Length);
			}
			else if(_name.Length < 100)
			{
				sb.Append("0");	sb.Append(_name.Length);
			}
			else
			{
				sb.Append(_name.Length);
			}
			sb.Append(_name);

			sb.Append(_unitID); sb.Append("/");
			sb.Append((int)_type); sb.Append("/");
			sb.Append(_uniqueID); sb.Append("/");

			sb.Append(_defaultMatrix._pos.x); sb.Append("/");
			sb.Append(_defaultMatrix._pos.y); sb.Append("/");
			sb.Append(_defaultMatrix._angleDeg); sb.Append("/");
			sb.Append(_defaultMatrix._scale.x); sb.Append("/");
			sb.Append(_defaultMatrix._scale.y); sb.Append("/");

			sb.Append(_defaultColor.r); sb.Append("/");
			sb.Append(_defaultColor.g); sb.Append("/");
			sb.Append(_defaultColor.b); sb.Append("/");
			sb.Append(_defaultColor.a); sb.Append("/");

			sb.Append((_isVisible ? "1" : "0")); sb.Append("/");

			sb.Append(_parentUnitID); sb.Append("/");
			sb.Append(_childUnitIDs.Count); sb.Append("/");

			if(_childUnitIDs.Count > 0)
			{
				for (int i = 0; i < _childUnitIDs.Count; i++)
				{
					sb.Append(_childUnitIDs[i]); sb.Append("/");
				}
			}

			return sb.ToString();
		}


		//일단 파싱만 하고 계층 설정은 나중에
		public bool DecodeData(string strSrc)
		{
			try
			{
				int nameLength = int.Parse(strSrc.Substring(0, 3));
				_name = strSrc.Substring(3, nameLength);

				strSrc = strSrc.Substring(3 + nameLength);

				string[] strParse = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);

				_unitID = int.Parse(strParse[0]);
				_type = (TYPE)int.Parse(strParse[1]);
				_uniqueID = int.Parse(strParse[2]);
				
				_defaultMatrix._pos.x = apUtil.ParseFloat(strParse[3]);
				_defaultMatrix._pos.y = apUtil.ParseFloat(strParse[4]);
				_defaultMatrix._angleDeg = apUtil.ParseFloat(strParse[5]);
				_defaultMatrix._scale.x = apUtil.ParseFloat(strParse[6]);
				_defaultMatrix._scale.y = apUtil.ParseFloat(strParse[7]);

				_defaultMatrix.MakeMatrix();

				_defaultColor.r = apUtil.ParseFloat(strParse[8]);
				_defaultColor.g = apUtil.ParseFloat(strParse[9]);
				_defaultColor.b = apUtil.ParseFloat(strParse[10]);
				_defaultColor.a = apUtil.ParseFloat(strParse[11]);

				_isVisible = (int.Parse(strParse[12]) == 1 ? true : false);

				_parentUnitID = int.Parse(strParse[13]);
				int nChild = int.Parse(strParse[14]);
				_childUnitIDs.Clear();

				for (int i = 0; i < nChild; i++)
				{
					_childUnitIDs.Add(int.Parse(strParse[15 + i]));
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("DecodeData Exception : " + ex);
				return false;
			}

			return true;
		}



		// Get / Set
		//----------------------------------------------
		public string LinkedName
		{
			get
			{
				if(!_isImported)
				{
					return "[ Not Imported ]";
				}
				if(_type == TYPE.MeshTransform && _targetMeshTransform != null)
				{
					return _targetMeshTransform._nickName;
				}
				else if(_type == TYPE.MeshGroupTransform && _targetMeshGroupTransform != null)
				{
					return _targetMeshGroupTransform._nickName;
				}
				else if(_type == TYPE.Bone && _targetBone != null)
				{
					return _targetBone._name;
				}
				return "[ Not Selected ]";
			}
		}

		public bool IsLinked
		{
			get
			{
				return (_type == TYPE.MeshTransform && _targetMeshTransform != null) ||
					(_type == TYPE.MeshGroupTransform && _targetMeshGroupTransform != null) ||
					(_type == TYPE.Bone && _targetBone != null);
			}
		}
	}
}
