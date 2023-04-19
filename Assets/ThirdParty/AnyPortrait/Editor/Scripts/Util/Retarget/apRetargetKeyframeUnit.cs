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
	/// 애니메이션 리타겟을 위한 "키프레임"의 데이터 단위
	/// Modifier 데이터를 모두 저장한다.
	/// 커브키..도? ㅜㅜ
	/// </summary>
	public class apRetargetKeyframeUnit
	{
		// Members
		//---------------------------------------------------------
		// 저장되는 데이터
		public int _unitID = -1;

		public int _keyframeUniqueID = -1;
		public apAnimKeyframe _linkedKeyframe = null;

		public int _frameIndex = -1;
		public bool _isKeyValueSet = false;

		public bool _isActive = false;

		public bool _isLoopAsStart = false;
		public bool _isLoopAsEnd = false;

		public int _loopFrameIndex = -1;

		public int _activeFrameIndexMin = 0;
		public int _activeFrameIndexMax = 0;

		public int _activeFrameIndexMin_Dummy = 0;
		public int _activeFrameIndexMax_Dummy = 0;

		//Curve 정보
		public apAnimCurve.TANGENT_TYPE _curve_PrevTangentType = apAnimCurve.TANGENT_TYPE.Smooth;
		public float _curve_PrevSmoothX = 0.3f;
		public float _curve_PrevSmoothY = 0.0f;
		public apAnimCurve.TANGENT_TYPE _curve_NextTangentType = apAnimCurve.TANGENT_TYPE.Smooth;
		public float _curve_NextSmoothX = 0.3f;
		public float _curve_NextSmoothY = 0.0f;

		//Control Param 타입이면 
		//Control Param의 어떤 값에 동기화되는가
		public int _conSyncValue_Int = 0;
		public float _conSyncValue_Float = 0.0f;
		public Vector2 _conSyncValue_Vector2 = Vector2.zero;

		//Modifier Mesh 타입일때
		public bool _isModMeshType = false;
		//>> Transform만 저장한다.
		public apMatrix _modTransformMatrix = new apMatrix();
		public Color _modMeshColor = Color.black;
		public bool _modVisible = false;



		//ModBone 타입일때
		public bool _isModBoneType = false;
		//>>Transform Matrix는 위 변수 이용

		//-------------------------------------------------------------------


		// Init
		//---------------------------------------------------------
		public apRetargetKeyframeUnit()
		{

		}



		// Functions
		//---------------------------------------------------------
		// Keyframe -> File
		public void SetAnimKeyframe(int unitID, apAnimKeyframe animKeyframe)
		{
			_unitID = unitID;

			_keyframeUniqueID = animKeyframe._uniqueID;
			_linkedKeyframe = animKeyframe;

			_frameIndex = animKeyframe._frameIndex;
			_isKeyValueSet = animKeyframe._isKeyValueSet;

			_isActive = animKeyframe._isActive;

			_isLoopAsStart = animKeyframe._isLoopAsStart;
			_isLoopAsEnd = animKeyframe._isLoopAsEnd;

			_loopFrameIndex = animKeyframe._loopFrameIndex;

			_activeFrameIndexMin = animKeyframe._activeFrameIndexMin;
			_activeFrameIndexMax = animKeyframe._activeFrameIndexMax;

			_activeFrameIndexMin_Dummy = animKeyframe._activeFrameIndexMin_Dummy;
			_activeFrameIndexMax_Dummy = animKeyframe._activeFrameIndexMax_Dummy;

			//Curve 정보
			_curve_PrevTangentType = animKeyframe._curveKey._prevTangentType;
			_curve_PrevSmoothX = animKeyframe._curveKey._prevSmoothX;
			_curve_PrevSmoothY = animKeyframe._curveKey._prevSmoothY;
			_curve_NextTangentType = animKeyframe._curveKey._nextTangentType;
			_curve_NextSmoothX = animKeyframe._curveKey._nextSmoothX;
			_curve_NextSmoothY = animKeyframe._curveKey._nextSmoothY;

			//Control Param 타입이면 
			//Control Param의 어떤 값에 동기화되는가
			_conSyncValue_Int = animKeyframe._conSyncValue_Int;
			_conSyncValue_Float = animKeyframe._conSyncValue_Float;
			_conSyncValue_Vector2 = animKeyframe._conSyncValue_Vector2;

			//Modifier Mesh 타입일때
			_isModMeshType = animKeyframe._linkedModMesh_Editor != null;
			_isModBoneType = animKeyframe._linkedModBone_Editor != null;

			//>> Transform만 저장한다.
			_modTransformMatrix = new apMatrix();
			_modMeshColor = Color.black;
			_modVisible = true;

			if (_isModMeshType)
			{
				_modTransformMatrix.SetMatrix(animKeyframe._linkedModMesh_Editor._transformMatrix, true);
				_modMeshColor = animKeyframe._linkedModMesh_Editor._meshColor;
				_modVisible = animKeyframe._linkedModMesh_Editor._isVisible;
			}
			else if (_isModBoneType)
			{
				_modTransformMatrix.SetMatrix(animKeyframe._linkedModBone_Editor._transformMatrix, true);
			}

		}

		public string GetEncodingData()
		{
			//Delimeter "/"를 기준으로 작성한다.

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append(_unitID);				sb.Append("/");
			sb.Append(_frameIndex);			sb.Append("/");
			sb.Append((_isKeyValueSet ? "1" : "0"));	sb.Append("/");
			sb.Append((_isActive ? "1" : "0"));	sb.Append("/");

			sb.Append((_isLoopAsStart ? "1" : "0"));	sb.Append("/");
			sb.Append((_isLoopAsEnd ? "1" : "0"));		sb.Append("/");

			sb.Append(_loopFrameIndex);				sb.Append("/");
			sb.Append(_activeFrameIndexMin);		sb.Append("/");
			sb.Append(_activeFrameIndexMax);		sb.Append("/");
			sb.Append(_activeFrameIndexMin_Dummy);	sb.Append("/");
			sb.Append(_activeFrameIndexMax_Dummy);	sb.Append("/");

			//Curve 정보
			sb.Append((int)_curve_PrevTangentType);	sb.Append("/");
			sb.Append(_curve_PrevSmoothX);			sb.Append("/");
			sb.Append(_curve_PrevSmoothY);			sb.Append("/");
			sb.Append((int)_curve_NextTangentType);	sb.Append("/");
			sb.Append(_curve_NextSmoothX);			sb.Append("/");
			sb.Append(_curve_NextSmoothY);			sb.Append("/");

			//Control Param 타입이면 
			//Control Param의 어떤 값에 동기화되는가
			sb.Append(_conSyncValue_Int);			sb.Append("/");
			sb.Append(_conSyncValue_Float);			sb.Append("/");
			sb.Append(_conSyncValue_Vector2.x);		sb.Append("/");
			sb.Append(_conSyncValue_Vector2.y);		sb.Append("/");

			//Modifier Mesh 타입일때
			sb.Append((_isModMeshType ? "1" : "0"));	sb.Append("/");
			sb.Append((_isModBoneType ? "1" : "0"));	sb.Append("/");

			//>> Transform만 저장한다.
			sb.Append(_modTransformMatrix._pos.x);		sb.Append("/");
			sb.Append(_modTransformMatrix._pos.y);		sb.Append("/");
			sb.Append(_modTransformMatrix._angleDeg);	sb.Append("/");
			sb.Append(_modTransformMatrix._scale.x);	sb.Append("/");
			sb.Append(_modTransformMatrix._scale.y);	sb.Append("/");

			sb.Append(_modMeshColor.r);		sb.Append("/");
			sb.Append(_modMeshColor.g);		sb.Append("/");
			sb.Append(_modMeshColor.b);		sb.Append("/");
			sb.Append(_modMeshColor.a);		sb.Append("/");

			sb.Append((_modVisible ? "1" : "0"));		sb.Append("/");

			return sb.ToString();
		}


		// File -> Keyframe
		//-----------------------------------------------------------
		public bool DecodeData(string strSrc)
		{
			try
			{
				string[] strUnits = strSrc.Split(new string[] { "/" }, StringSplitOptions.None);

				_unitID =		int.Parse(strUnits[0]);
				_frameIndex =	int.Parse(strUnits[1]);
				_isKeyValueSet =	(int.Parse(strUnits[2]) == 1) ? true : false;
				_isActive =			(int.Parse(strUnits[3]) == 1) ? true : false;

				_isLoopAsStart =	(int.Parse(strUnits[4]) == 1) ? true : false;
				_isLoopAsEnd =		(int.Parse(strUnits[5]) == 1) ? true : false;
				
				_loopFrameIndex =				int.Parse(strUnits[6]);
				_activeFrameIndexMin =			int.Parse(strUnits[7]);
				_activeFrameIndexMax =			int.Parse(strUnits[8]);
				_activeFrameIndexMin_Dummy =	int.Parse(strUnits[9]);
				_activeFrameIndexMax_Dummy =	int.Parse(strUnits[10]);
				
				_curve_PrevTangentType = (apAnimCurve.TANGENT_TYPE)int.Parse(strUnits[11]);
				_curve_PrevSmoothX = apUtil.ParseFloat(strUnits[12]);
				_curve_PrevSmoothY = apUtil.ParseFloat(strUnits[13]);
				_curve_NextTangentType = (apAnimCurve.TANGENT_TYPE)int.Parse(strUnits[14]);
				_curve_NextSmoothX = apUtil.ParseFloat(strUnits[15]);
				_curve_NextSmoothY = apUtil.ParseFloat(strUnits[16]);
				
				_conSyncValue_Int = int.Parse(strUnits[17]);
				_conSyncValue_Float = apUtil.ParseFloat(strUnits[18]);
				_conSyncValue_Vector2.x = apUtil.ParseFloat(strUnits[19]);
				_conSyncValue_Vector2.y = apUtil.ParseFloat(strUnits[20]);
				
				_isModMeshType = (int.Parse(strUnits[21]) == 1) ? true : false;
				_isModBoneType = (int.Parse(strUnits[22]) == 1) ? true : false;

				_modTransformMatrix._pos.x = apUtil.ParseFloat(strUnits[23]);
				_modTransformMatrix._pos.y = apUtil.ParseFloat(strUnits[24]);
				_modTransformMatrix._angleDeg = apUtil.ParseFloat(strUnits[25]);
				_modTransformMatrix._scale.x = apUtil.ParseFloat(strUnits[26]);
				_modTransformMatrix._scale.y = apUtil.ParseFloat(strUnits[27]);
				
				_modMeshColor.r = apUtil.ParseFloat(strUnits[28]);
				_modMeshColor.g = apUtil.ParseFloat(strUnits[29]);
				_modMeshColor.b = apUtil.ParseFloat(strUnits[30]);
				_modMeshColor.a = apUtil.ParseFloat(strUnits[31]);

				_modVisible = (int.Parse(strUnits[32]) == 1) ? true : false;
			}
			catch (Exception ex)
			{
				Debug.LogError("Decode Exception : " + ex);
				return false;
			}

			return true;
		}

		// Get / Set
		//---------------------------------------------------------
	}
}