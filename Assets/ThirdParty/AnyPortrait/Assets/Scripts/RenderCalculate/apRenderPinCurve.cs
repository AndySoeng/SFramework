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
	/// Mesh Pin Curve의 Render 버전.
	/// Render Pin을 대상으로 연결된다.
	/// 기본적인 처리는 동일하다.
	/// </summary>
	public class apRenderPinCurve
	{
		// Members
		//---------------------------------------------------
		public apRenderPinGroup _parentRenderPinGroup = null;
		public apMeshPinCurve _srcCurve = null;

		public apRenderPin _prevPin;
		public apRenderPin _nextPin;

		// Init
		//---------------------------------------------------
		public apRenderPinCurve(apMeshPinCurve srcCurve, apRenderPinGroup parentRenderPinGroup)
		{
			_parentRenderPinGroup = parentRenderPinGroup;
			_srcCurve = srcCurve;

			_prevPin = null;
			_nextPin = null;
		}

		public void LinkData(apRenderPin prevPin, apRenderPin nextPin)
		{
			_prevPin = prevPin;
			_nextPin = nextPin;
		}

		// Get / Set
		//----------------------------------------------------
		public bool IsLinear()
		{
			return _prevPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp 
				&& _nextPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp;
		}

		public Vector2 GetCurvePosW(float lerp)
		{
			lerp = Mathf.Clamp01(lerp);

			Vector2 resultPos = Vector2.zero;
			//Default 위치를 바탕으로 커브를 계산한다.
			//양쪽의 Tangent 타입을 확인하자
			if(_prevPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//둘다 Sharp인 경우 > [직선]
				resultPos = (_prevPin._pos_World * (1.0f - lerp)) + (_nextPin._pos_World * lerp);
			}
			else if(_prevPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp
				&& _nextPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth)
			{
				//Prev가 Sharp + Next가 Smooth인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				resultPos = (_prevPin._pos_World * revLerp * revLerp)
							+ (2.0f * _nextPin._controlPointPos_Prev * revLerp * lerp)
							+ (_nextPin._pos_World * lerp * lerp);
			}
			else if(_prevPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Smooth
				&& _nextPin._srcPin._tangentType == apMeshPin.TANGENT_TYPE.Sharp)
			{
				//Prev가 Smooth + Next가 Sharp인 경우 : 컨트롤 파라미터 3개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				resultPos = (_prevPin._pos_World * revLerp * revLerp)
							+ (2.0f * _prevPin._controlPointPos_Next * revLerp * lerp)
							+ (_nextPin._pos_World * lerp * lerp);
			}
			else
			{
				//둘다 Smooth인 경우 : 컨트롤 파라미터 4개짜리 베지어 커브
				float revLerp = 1.0f - lerp;
				resultPos = (_prevPin._pos_World * revLerp * revLerp * revLerp)
							+ (3.0f * _prevPin._controlPointPos_Next * revLerp * revLerp * lerp)
							+ (3.0f * _nextPin._controlPointPos_Prev * revLerp * lerp * lerp)
							+ (_nextPin._pos_World * lerp * lerp * lerp);
			}

			return resultPos;
		}


	}
}