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
	public class apRetargetAnimEvent
	{
		// Members
		//-----------------------------------------------
		public int _frameIndex = -1;
		public int _frameIndex_End = -1;//Continuous 타입일 때

		public string _eventName = "";

		public apAnimEvent.CALL_TYPE _callType = apAnimEvent.CALL_TYPE.Once;

		public class SubParameter
		{
			public apAnimEvent.PARAM_TYPE _paramType = apAnimEvent.PARAM_TYPE.Integer;

			public bool _boolValue = false;//<<이것도 보간이 안된다.
			public int _intValue = 0;
			public float _floatValue = 0.0f;
			public Vector2 _vec2Value = Vector2.zero;
			public string _strValue = "";//<<이건 보간이 안된다.

			public int _intValue_End = 0;
			public float _floatValue_End = 0.0f;
			public Vector2 _vec2Value_End = Vector2.zero;

			public SubParameter()
			{
				_paramType = apAnimEvent.PARAM_TYPE.Integer;

				_boolValue = false;
				_intValue = 0;
				_floatValue = 0.0f;
				_vec2Value = Vector2.zero;
				_strValue = "";//<<이건 보간이 안된다.

				_intValue_End = 0;
				_floatValue_End = 0.0f;
				_vec2Value_End = Vector2.zero;
			}
		}

		public List<SubParameter> _subParams = new List<SubParameter>();


		// Import 여부
		public bool _isImported = false;


		// Init
		//-----------------------------------------------
		public apRetargetAnimEvent()
		{

		}


		// Functions
		//-----------------------------------------------
		// Event -> File
		//--------------------------------------------------------------
		public void SetAnimationEvent(apAnimEvent animEvent)
		{
			_frameIndex = animEvent._frameIndex;
			_frameIndex_End = animEvent._frameIndex_End;

			_eventName = animEvent._eventName;

			_callType = animEvent._callType;


			_subParams.Clear();
			for (int i = 0; i < animEvent._subParams.Count; i++)
			{
				SubParameter newSubParam = new SubParameter();
				apAnimEvent.SubParameter srcParam = animEvent._subParams[i];

				newSubParam._paramType = srcParam._paramType;

				newSubParam._boolValue = srcParam._boolValue;
				newSubParam._intValue = srcParam._intValue;
				newSubParam._floatValue = srcParam._floatValue;
				newSubParam._vec2Value = srcParam._vec2Value;
				newSubParam._strValue = srcParam._strValue;

				newSubParam._intValue_End = srcParam._intValue_End;
				newSubParam._floatValue_End = srcParam._floatValue_End;
				newSubParam._vec2Value_End = srcParam._vec2Value_End;

				_subParams.Add(newSubParam);
			}
		}

		public bool EncodeToFile(StreamWriter sw)
		{
			//AnimEvent의 기본 정보를 넣고
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			if(_eventName.Length < 10)
			{
				sb.Append("00"); sb.Append(_eventName.Length);
			}
			else if(_eventName.Length < 100)
			{
				sb.Append("0"); sb.Append(_eventName.Length);
			}
			else
			{
				sb.Append(_eventName.Length);
			}
			if (_eventName.Length > 0)
			{
				sb.Append(_eventName);
			}

			sb.Append(_frameIndex); sb.Append("/");
			sb.Append(_frameIndex_End); sb.Append("/");
			sb.Append((int)_callType); sb.Append("/");
			sb.Append(_subParams.Count); sb.Append("/");

			sw.WriteLine(sb.ToString());

			//SubParam 정보를 한줄씩 넣자
			for (int i = 0; i < _subParams.Count; i++)
			{
				SubParameter subParam = _subParams[i];

				System.Text.StringBuilder sb_sub = new System.Text.StringBuilder();
				if(subParam._strValue.Length == 0)
				{
					sb_sub.Append("000");
				}
				else if(subParam._strValue.Length < 10)
				{
					sb_sub.Append("00");
					sb_sub.Append(subParam._strValue.Length);
				}
				else if(subParam._strValue.Length < 100)
				{
					sb_sub.Append("0");
					sb_sub.Append(subParam._strValue.Length);
				}
				else
				{
					sb_sub.Append(subParam._strValue.Length);
				}
				if(subParam._strValue.Length > 0)
				{
					sb_sub.Append(subParam._strValue);
				}

				sb_sub.Append((int)subParam._paramType);			sb_sub.Append("/");

				sb_sub.Append((subParam._boolValue ? "1" : "0"));	sb_sub.Append("/");
				sb_sub.Append(subParam._intValue);					sb_sub.Append("/");
				sb_sub.Append(subParam._floatValue);				sb_sub.Append("/");
				sb_sub.Append(subParam._vec2Value.x);				sb_sub.Append("/");
				sb_sub.Append(subParam._vec2Value.y);				sb_sub.Append("/");

				sb_sub.Append(subParam._intValue_End);				sb_sub.Append("/");
				sb_sub.Append(subParam._floatValue_End);			sb_sub.Append("/");
				sb_sub.Append(subParam._vec2Value_End.x);			sb_sub.Append("/");
				sb_sub.Append(subParam._vec2Value_End.y);			sb_sub.Append("/");

				sw.WriteLine(sb_sub.ToString());
				
			}


			return true;
		}



		public bool DecodeData(StreamReader sr)
		{
			try
			{
				string strSrc = sr.ReadLine();
				int nNameLength = int.Parse(strSrc.Substring(0, 3));
				if(nNameLength > 0)
				{
					_eventName = strSrc.Substring(3, nNameLength);
				}
				else
				{
					_eventName = "";
				}
				strSrc = strSrc.Substring(3 + nNameLength);

				string[] strParse = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);

				_frameIndex = int.Parse(strParse[0]);
				_frameIndex_End = int.Parse(strParse[1]);
				_callType = (apAnimEvent.CALL_TYPE)int.Parse(strParse[2]);
				int nSubParam = int.Parse(strParse[3]);

				_subParams.Clear();

				for (int i = 0; i < nSubParam; i++)
				{
					SubParameter subParam = new SubParameter();
					strSrc = sr.ReadLine();

					int nStrValue = int.Parse(strSrc.Substring(0, 3));
					if(nStrValue > 0)
					{
						subParam._strValue = strSrc.Substring(3, nStrValue);
					}
					else
					{
						subParam._strValue = "";
					}
					strSrc = strSrc.Substring(3 + nStrValue);

					strParse = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);

					subParam._paramType =	(apAnimEvent.PARAM_TYPE)int.Parse(strParse[0]);
					subParam._boolValue =	(int.Parse(strParse[1]) == 1 ? true : false);
					subParam._intValue =	int.Parse(strParse[2]);
					subParam._floatValue =	apUtil.ParseFloat(strParse[3]);
					subParam._vec2Value.x = apUtil.ParseFloat(strParse[4]);
					subParam._vec2Value.y = apUtil.ParseFloat(strParse[5]);

					subParam._intValue_End =	int.Parse(strParse[6]);
					subParam._floatValue_End =	apUtil.ParseFloat(strParse[7]);
					subParam._vec2Value_End.x = apUtil.ParseFloat(strParse[8]);
					subParam._vec2Value_End.y = apUtil.ParseFloat(strParse[9]);

					_subParams.Add(subParam);
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
		//-----------------------------------------------
	}
}