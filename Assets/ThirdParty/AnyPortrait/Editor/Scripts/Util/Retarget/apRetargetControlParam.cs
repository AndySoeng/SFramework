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
	/// Control Param의 리타겟 단위
	/// 연동만 할 것이므로 다른 데이터는 받지 않는다.
	/// </summary>
	public class apRetargetControlParam
	{
		// Members
		//-----------------------------------------------------
		public int _unitID = -1;
		public int _controlParamUniqueID = -1;
		public string _keyName = "";

		public apControlParam.TYPE _valueType = apControlParam.TYPE.Int;

		// Import 설정
		public bool _isImported = false;
		public apControlParam _targetControlParam = null;


		// Init
		//-----------------------------------------------------
		public apRetargetControlParam()
		{

		}

		// Functions
		//-----------------------------------------------------
		public void SetControlParam(int unitID, apControlParam controlParam)
		{
			_unitID = unitID;
			_controlParamUniqueID = controlParam._uniqueID;
			_keyName = controlParam._keyName;

			_valueType = controlParam._valueType;
		}


		public string GetEncodingData()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if(_keyName.Length < 10)
			{
				sb.Append("00");
				sb.Append(_keyName.Length);
			}
			else if(_keyName.Length < 100)
			{
				sb.Append("0");
				sb.Append(_keyName.Length);
			}
			else
			{
				sb.Append(_keyName.Length);
			}
			sb.Append(_keyName);	
			sb.Append(_unitID);					sb.Append("/");
			sb.Append(_controlParamUniqueID);	sb.Append("/");
			sb.Append((int)_valueType);			sb.Append("/");

			return sb.ToString();
		}



		public bool DecodeData(string strSrc)
		{
			try
			{
				int nName = int.Parse(strSrc.Substring(0, 3));
				if (nName == 0)
				{
					_keyName = "";
				}
				else
				{
					_keyName = strSrc.Substring(3, nName);
				}

				strSrc = strSrc.Substring(3 + nName);

				string[] strParse = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);

				_unitID = int.Parse(strParse[0]);
				_controlParamUniqueID = int.Parse(strParse[1]);
				_valueType = (apControlParam.TYPE)int.Parse(strParse[2]);
			}
			catch (Exception ex)
			{
				Debug.LogError("Decode Data Exception : " + ex);
				return false;
			}
			return true;
		}

		// Get / Set
		//-----------------------------------------------------
		public string LinkedName
		{
			get
			{
				if(!_isImported)
				{
					return "[ Not Imported ]";
				}
				if(_targetControlParam != null)
				{
					return _targetControlParam._keyName;
				}
				return "[ Not Selected ]";
			}
		}

		public bool IsLinked
		{
			get
			{
				return _targetControlParam != null;
			}
		}
	}
}