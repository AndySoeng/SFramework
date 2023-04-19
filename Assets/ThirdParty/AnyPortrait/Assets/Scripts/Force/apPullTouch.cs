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

	// Force와 달리, 일종의 터치 이벤트에 대해서 지원하는 기능
	// 터치한 순간 PhysicsModifier를 지원하는 Vertex가 "잡힌 상태가 되며", 다른 힘보다 우선하여 끌려다닌다.
	// "위치", "범위", "인덱스"만 저장하며, 처리는 Modifier에서 한다.
	// 처리 특성항 OptModifier에서만 작동 (Editor에서도 코드는 작동하지만 제어할 수 없다)
	// 처리 자체는 힘이 아닌 위치 보간의 방식
	
	/// <summary>
	/// A class that implements the effect of being touched by a touch or mouse.
	/// "TouchID" can be used to give a continuous pull effect.
	/// (We recommend using "apPortrait" functions rather than using this class directly.)
	/// </summary>
	public class apPullTouch
	{
		// Members
		//----------------------------------------------------
		private int _touchID = -1;
		private Vector2 _posW = Vector2.zero;
		private float _radius = 0.0f;
		private bool _isLive = false;

		/// <summary>
		/// Touch ID
		/// </summary>
		public int TouchID { get { return _touchID; } }
		public Vector2 Position { get { return _posW; } }
		public float Radius { get { return _radius; } }
		public bool IsLive { get { return _isLive; } }

		// Init
		//----------------------------------------------------
		public apPullTouch(int touchID)
		{
			_touchID = touchID;
			_isLive = false;
		}

		// Functions
		//----------------------------------------------------
		public void SetPos(Vector2 posW)
		{
			_posW = posW;
		}

		public void SetDisable()
		{
			_isLive = false;
		}

		public void SetEnable(Vector2 posW, float radius)
		{
			_posW = posW;
			_radius = radius;
			_isLive = true;
		}

		// Get
		//----------------------------------------------------
		// 실제로 끌려가는 위치를 가져온다.
		// 처음 터치했을 때 Weight를 저장해야한다.
		public Vector2 GetPulledPos(Vector2 targetPosW, float weight)
		{
			return (_posW * weight) + (targetPosW * (1.0f - weight));
		}

		// 처음 터치를 했을 때, 끌려가는 가중치(Pulled Weight)를 계산하여 리턴하는 함수.
		// 이 함수를 호출한 후, touchID와 weight를 저장해야한다.
		public float GetTouchedWeight(Vector2 curTargetPosW)
		{
			float dist = Vector2.Distance(curTargetPosW, _posW);
			if (dist > _radius)
			{
				return 0.0f;
			}
			if (_radius < 0.001f)
			{
				return 0.0f;
			}

			return Mathf.Clamp01(1.0f - (dist / _radius));
		}
	}
}