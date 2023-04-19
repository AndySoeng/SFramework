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
	/// AnimClip 데이터 계산 후, 어떤 Control Param을 컨트롤하여 어떤 값을 가지게 할지 결정하는 데이터
	/// Keyframe 보간 계산 결과값이 들어간다.
	/// 멤버는 Sort시 미리 만든다.
	/// </summary>
	public class apAnimControlParamResult
	{
		// Members
		//--------------------------------------------------------------------
		public apControlParam _targetControlParam = null;
		public bool _isCalculated = false;
		//public int _value_Int = 0;//기존 : Int형
		public float _value_Int = 0;//변경 : Float형
		public float _value_Float = 0.0f;
		public Vector2 _value_Vec2 = Vector2.zero;
		
		//추가 20.4.19
		//애니메이션의 정보를 추가한다.
		public apAnimClip _parentAnimClip = null;
		public int _animPlayOrder = 0;
		public float _animWeight = 0.0f;
		public apAnimPlayUnit.BLEND_METHOD _animBlendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation;
		
		// Init
		//--------------------------------------------------------------------
		public apAnimControlParamResult(apControlParam targetControlParam, apAnimClip parentAnimClip)
		{
			_targetControlParam = targetControlParam;
			_parentAnimClip = parentAnimClip;
		}


		//--------------------------------------------------------------------
		public void Init()
		{
			//_weight = 0.0f;
			_isCalculated = false;
			//_value_Bool = false;
			_value_Int = 0.0f;
			_value_Float = 0.0f;
			_value_Vec2 = Vector2.zero;
			//_value_Vec3 = Vector3.zero;
			//_value_Color = Color.black;
		}


		// Set Calculated Value
		//--------------------------------------------------------------------
		public void SetKeyframeResult(apAnimKeyframe keyframe, float weight)
		{
			//_weight = Mathf.Clamp01(_weight + weight);


			switch (_targetControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					//_value_Int += (int)(keyframe._conSyncValue_Int * weight + 0.5f);//기존 : 버그 있다.
					//일단 이게 없어야 함 + Float형으로 바뀌었다.

					//이전 : 버그
					//_value_Int += (int)(keyframe._conSyncValue_Int * weight);

					//버그 수정 : 20.4.15
					_value_Int += (float)keyframe._conSyncValue_Int * weight;
					//Debug.Log("Int Key Value : " + keyframe._conSyncValue_Int + " (" + weight + ") >> " + _value_Int);
					break;


				case apControlParam.TYPE.Float:
					_value_Float += keyframe._conSyncValue_Float * weight;
					//Debug.LogWarning("Float Key Value : " + keyframe._conSyncValue_Float + " (" + weight + ") >> " + _value_Float);
					break;

				case apControlParam.TYPE.Vector2:
					_value_Vec2 += keyframe._conSyncValue_Vector2 * weight;
					break;
			}

			_isCalculated = true;
		}


		public void AdaptToControlParam()
		{
			if (!_isCalculated)
			{
				return;
			}

			switch (_targetControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					_targetControlParam._int_Cur = Mathf.RoundToInt(_value_Int);//버그 수정 20.4.15
					break;

				case apControlParam.TYPE.Float:
					_targetControlParam._float_Cur = _value_Float;
					break;

				case apControlParam.TYPE.Vector2:
					_targetControlParam._vec2_Cur = _value_Vec2;
					break;
			}

		}


		public void AdaptToControlParam_Opt(float weight, int layer, int playOrder, apAnimPlayUnit.BLEND_METHOD blendMethod)
		{
			if (!_isCalculated)
			{
				//Debug.LogError("계산되지 않았다. [" + _targetControlParam._keyName + "]");
				return;
			}


			//이전방식 (레이어가 없다.)

			//변경된 방식 (20.4.19 : 레이어 처리를 위해 아예 이 인스턴스를 넘겨주자)
			if(playOrder < 0 || weight <= 0.0f)
			{
				//Debug.LogError("Order/Weight 이상 [" + _targetControlParam._keyName + "] / Play Order : " + playOrder + " / Weight : " + weight);
				return;
			}
			_animPlayOrder = playOrder;
			_animWeight = weight;
			_animBlendMethod = blendMethod;
			_targetControlParam.AddCalculated_Request(this, layer);
		}
	}

}