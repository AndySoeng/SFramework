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
	/// IK Controller의 OPT 버전
	/// </summary>
	[Serializable]
	public class apOptBoneIKController
	{
		// Members
		//---------------------------------------------------
		public enum CONTROLLER_TYPE
		{
			None = 0,
			Position = 1,
			LookAt = 2
		}
		[SerializeField]
		public CONTROLLER_TYPE _controllerType = CONTROLLER_TYPE.None;

		[SerializeField]
		public apOptBone _parentBone = null;//<<Opt Bone은 직렬화로 저장한다.

		[SerializeField]
		public int _effectorBoneID = -1;

		[SerializeField]
		public apOptBone _effectorBone = null;

		
		[SerializeField]
		public float _defaultMixWeight = 0.0f;//>>>> Default는 0이다. IK를 항상 켜놓지는 말자

		//추가 : Control Param으로 Weight를 제어할 수 있다.
		[SerializeField]
		public bool _isWeightByControlParam = false;

		[SerializeField]
		public int _weightControlParamID = -1;

		[NonSerialized]
		public apControlParam _weightControlParam = null;
		

		// Init
		//---------------------------------------------------
		public apOptBoneIKController()
		{
			_controllerType = CONTROLLER_TYPE.None;
		}


		public void Bake(apOptBone parentBone, 
							int effectorBoneID, apBoneIKController.CONTROLLER_TYPE controllerType, 
							float defaultMixWeight, bool isWeightByControlParam, int weightControlParamID)
		{
			_parentBone = parentBone;
			_effectorBoneID = effectorBoneID;

			switch (controllerType)
			{
				case apBoneIKController.CONTROLLER_TYPE.None:
					_controllerType = CONTROLLER_TYPE.None;
					break;

				case apBoneIKController.CONTROLLER_TYPE.Position:
					_controllerType = CONTROLLER_TYPE.Position;
					break;

				case apBoneIKController.CONTROLLER_TYPE.LookAt:
					_controllerType = CONTROLLER_TYPE.LookAt;
					break;
			}
			_defaultMixWeight = defaultMixWeight;

			_effectorBone = null;

			_isWeightByControlParam = isWeightByControlParam;
			_weightControlParamID = weightControlParamID;
			_weightControlParam = null;
		}

		public void LinkEffector(apOptBone effectorBone)
		{
			//if(_controllerType != CONTROLLER_TYPE.None)
			//{
			//	Debug.Log("Link Effector : " + _parentBone.name + " : " + (effectorBone != null ? effectorBone.name : " <None>"));
			//}
			
			_effectorBone = effectorBone;
			
			if(_effectorBone == null)
			{
				_effectorBoneID = -1;
			}
			else
			{
				_effectorBoneID = _effectorBone._uniqueID;
			}
			if(_effectorBone == null)
			{
				//Effector Bone이 없으면 처리 무효
				_controllerType = CONTROLLER_TYPE.None;
			}
		}

		/// <summary>
		/// 게임이 시작되면 portrait로부터 ControlParam을 연결해야한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void Link(apPortrait portrait)
		{
			
			if(_isWeightByControlParam && _weightControlParamID >= 0)
			{
				if (_weightControlParam == null)
				{
					_weightControlParam = portrait.GetControlParam(_weightControlParamID);
				}

				//if(_weightControlParam == null)
				//{
				//	//_weightControlParamID = -1;
				//	//_isWeightByControlParam = false;
				//}
			}
			else
			{
				_weightControlParam = null;
			}
		}

		// Functions
		//---------------------------------------------------
		

	}
}