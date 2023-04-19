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
	/// <summary>
	/// The class that controls Modifiers. 
	/// It has a user-definable "Control Parameters" inside.
	/// If the user controls this class directly, it can cause malfunction due to Monobehaviour's circular manipulation rules.
	/// It is recommended to use the function in "apPortrait".
	/// </summary>
	[Serializable]
	public class apController
	{
		// Members
		//-----------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		[SerializeField]
		public List<apControlParam> _controlParams = new List<apControlParam>();


		// Init
		//-----------------------------------------------
		public apController()
		{

		}


		public void Ready(apPortrait portrait)
		{
			_portrait = portrait;

			_controlParams.RemoveAll(delegate(apControlParam a)
			{
				return a == null;
			});

			for (int i = 0; i < _controlParams.Count; i++)
			{
				_controlParams[i].Ready(portrait);
			}
		}

		public void SetDefaultAll()
		{
			for (int i = 0; i < _controlParams.Count; i++)
			{
				_controlParams[i].SetDefault();
			}
		}

		


		// Functions - Editor
		//-----------------------------------------------
		public void MakeReservedParams()
		{
			//얼굴
			AddParam_Vector2("Head Angle", true, apControlParam.CATEGORY.Head, Vector2.zero, new Vector2(-1, -1), new Vector2(1, 1), "Dir X", "Dir Y");
			AddParam_Vector2("Body Angle", true, apControlParam.CATEGORY.Body, Vector2.zero, new Vector2(-1, -1), new Vector2(1, 1), "Dir X", "Dir Y");

			//표정
			AddParam_Float("Eye L Open", true, apControlParam.CATEGORY.Face, 0.0f, -1.0f, 1.0f, "Close", "Open");
			AddParam_Float("Eye R Open", true, apControlParam.CATEGORY.Face, 0.0f, -1.0f, 1.0f, "Close", "Open");

			AddParam_Float("Eye L Smile", true, apControlParam.CATEGORY.Face, 1.0f, 0.0f, 1.0f, "Sad", "Angry");
			AddParam_Float("Eye R Smile", true, apControlParam.CATEGORY.Face, 1.0f, 0.0f, 1.0f, "Sad", "Angry");

			AddParam_Float("EyeBall Size", true, apControlParam.CATEGORY.Face, 0.0f, -1.0f, 1.0f, "Small", "Large");

			AddParam_Vector2("EyeBall L LookAt", true, apControlParam.CATEGORY.Face, Vector2.zero, new Vector2(-1, -1), new Vector2(1, 1), "Dir X", "Dir Y");
			AddParam_Vector2("EyeBall R LookAt", true, apControlParam.CATEGORY.Face, Vector2.zero, new Vector2(-1, -1), new Vector2(1, 1), "Dir X", "Dir Y");

			AddParam_Float("Brow L Shape", true, apControlParam.CATEGORY.Face, 0.0f, -1.0f, 1.0f, "Sad", "Angry");
			AddParam_Float("Brow R Shape", true, apControlParam.CATEGORY.Face, 0.0f, -1.0f, 1.0f, "Sad", "Angry");

			AddParam_Int("Mouth Type", true, apControlParam.CATEGORY.Face, 0);
			AddParam_Float("Mouth Open", true, apControlParam.CATEGORY.Face, 0.0f, 0.0f, 1.0f, "Close", "Open");
			AddParam_Vector2("Mouth Form", true, apControlParam.CATEGORY.Face, Vector2.zero, new Vector2(-1, -1), new Vector2(1, 1), "I-E-O", "A-E-U");

			AddParam_Float("Breath", true, apControlParam.CATEGORY.Body, 0.0f, 0.0f, 1.0f, "Default", "Breath");
		}

		//public apControlParam AddParam_Bool(string keyName, bool isReserved, apControlParam.CATEGORY category, bool defaultValue)
		//{
		//	apControlParam newParam = MakeNewParam(keyName, isReserved, category);
		//	if(newParam == null) { return null; }
		//	newParam.SetBool(defaultValue);

		//	return newParam;
		//}

		public apControlParam AddParam_Int(string keyName, bool isReserved, apControlParam.CATEGORY category, int defaultValue)
		{
			apControlParam newParam = MakeNewParam(keyName, isReserved, category);
			if (newParam == null)
			{ return null; }
			newParam.SetInt(defaultValue);
			return newParam;
		}

		public apControlParam AddParam_Int(string keyName, bool isReserved, apControlParam.CATEGORY category, int defaultValue, int min, int max, string label_Min, string label_Max)
		{
			apControlParam newParam = MakeNewParam(keyName, isReserved, category);
			if (newParam == null)
			{ return null; }
			newParam.SetInt(defaultValue, min, max, label_Min, label_Max);
			return newParam;
		}

		public apControlParam AddParam_Float(string keyName, bool isReserved, apControlParam.CATEGORY category, float defaultValue)
		{
			apControlParam newParam = MakeNewParam(keyName, isReserved, category);
			if (newParam == null)
			{ return null; }
			newParam.SetFloat(defaultValue);
			return newParam;
		}

		public apControlParam AddParam_Float(string keyName, bool isReserved, apControlParam.CATEGORY category, float defaultValue, float min, float max, string label_Min, string label_Max)
		{
			apControlParam newParam = MakeNewParam(keyName, isReserved, category);
			if (newParam == null)
			{ return null; }
			newParam.SetFloat(defaultValue, min, max, label_Min, label_Max);
			return newParam;
		}

		public apControlParam AddParam_Vector2(string keyName, bool isReserved, apControlParam.CATEGORY category, Vector2 defaultValue)
		{
			apControlParam newParam = MakeNewParam(keyName, isReserved, category);
			if (newParam == null)
			{ return null; }
			newParam.SetVector2(defaultValue);
			return newParam;
		}

		public apControlParam AddParam_Vector2(string keyName, bool isReserved, apControlParam.CATEGORY category, Vector2 defaultValue, Vector2 min, Vector2 max, string label_Axis1, string label_Axis2)
		{
			apControlParam newParam = MakeNewParam(keyName, isReserved, category);
			if (newParam == null)
			{ return null; }
			newParam.SetVector2(defaultValue, min, max, label_Axis1, label_Axis2);
			return newParam;
		}

		//public apControlParam AddParam_Vector3(string keyName, bool isReserved, apControlParam.CATEGORY category, Vector3 defaultValue)
		//{
		//	apControlParam newParam = MakeNewParam(keyName, isReserved, category);
		//	if(newParam == null) { return null; }
		//	newParam.SetVector3(defaultValue);
		//	return newParam;
		//}

		//public apControlParam AddParam_Vector3(string keyName, bool isReserved, apControlParam.CATEGORY category, Vector3 defaultValue, Vector3 min, Vector3 max, string label_Axis1, string label_Axis2, string label_Axis3)
		//{
		//	apControlParam newParam = MakeNewParam(keyName, isReserved, category);
		//	if(newParam == null) { return null; }
		//	newParam.SetVector3(defaultValue, min, max, label_Axis1, label_Axis2, label_Axis3);
		//	return newParam;
		//}

		//public apControlParam AddParam_Color(string keyName, bool isReserved, apControlParam.CATEGORY category, Color defaultValue)
		//{
		//	apControlParam newParam = MakeNewParam(keyName, isReserved, category);
		//	if(newParam == null) { return null; }
		//	newParam.SetColor(defaultValue);
		//	return newParam;
		//}

		public apControlParam MakeNewParam(string keyName, bool isReserved, apControlParam.CATEGORY category)
		{
			//Debug.Log("Make New Param <" + keyName + ">");
			//apControlParam existParam = FindParam(keyName);
			//if (existParam != null)
			//{
			//	Debug.LogError("키 겹침 문제");
			//	return null;
			//}
			//겹침은 ID로만

			//int nextID = _portrait.MakeUniqueID_ControlParam();
			int nextID = _portrait.MakeUniqueID(apIDManager.TARGET.ControlParam);
			if (nextID < 0)
			{
				Debug.LogError("ID Creating Failed");
				return null;
			}

			apControlParam newParam = new apControlParam(nextID, keyName, isReserved, category);
			_controlParams.Add(newParam);
			return newParam;
		}


		public apControlParam FindParam(string keyName)
		{
			return _controlParams.Find(delegate (apControlParam a)
			{
				return a._keyName.Equals(keyName);
			});
		}

		public apControlParam FindParam(int uniqueID)
		{
			return _controlParams.Find(delegate (apControlParam a)
			{
				return a._uniqueID == uniqueID;
			});
		}


		// Function - Realtime
		//-----------------------------------------------
		//추가 20.7.5
		/// <summary>
		/// 인게임에서 Portrait 초기화시 호출되어야 한다.
		/// </summary>
		public void InitRuntime(apPortrait portrait)
		{
			apControlParam curParam = null;
			for (int i = 0; i < _controlParams.Count; i++)
			{
				curParam = _controlParams[i];
				curParam.InitRuntime();
				curParam.SetUnspecifiedValueInAnimOption(portrait._unspecifiedAnimControlParamOption);//추가 22.5.16
			}
		}

		public void ReadyToLayerUpdate()
		{
			for (int i = 0; i < _controlParams.Count; i++)
			{
				_controlParams[i].ReadyToOptLayerUpdate();
			}
		}


		public void CompleteLayerUpdate()
		{
			for (int i = 0; i < _controlParams.Count; i++)
			{
				_controlParams[i].CompleteOptLayerUpdate();
			}
		}

		/// <summary>
		/// 외부 스크립트에서 제어하기 전에 초기화를 해야한다.
		/// </summary>
		public void InitRequest()
		{	
			for (int i = 0; i < _controlParams.Count; i++)
			{
				_controlParams[i].InitRequest();
			}
		}

		/// <summary>
		/// 외부 스크립트에서 제어된 값을 적용한다.
		/// </summary>
		public void CompleteRequests()
		{
			for (int i = 0; i < _controlParams.Count; i++)
			{
				_controlParams[i].CompleteRequests();
			}
		}
	}


}