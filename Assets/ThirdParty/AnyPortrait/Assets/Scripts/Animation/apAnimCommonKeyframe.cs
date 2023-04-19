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

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using AnyPortrait;

namespace AnyPortrait
{

	// 에디터에서 전용으로 사용하는 클래스.
	// Keyframe을 일괄적으로 제어하기 위한 클래스이다.
	// Selection에서 관리한다.
	
	public class apAnimCommonKeyframe
	{
		// Members
		//---------------------------------------------
		public int _frameIndex = -1;

		/// <summary>
		/// 해당 프레임 인덱스에 포함된 키프레임들
		/// </summary>
		public List<apAnimKeyframe> _keyframes = new List<apAnimKeyframe>();

		public bool _isSelected = false;

		// Init
		//---------------------------------------------
		public apAnimCommonKeyframe(int frameIndex)
		{
			_frameIndex = frameIndex;
			_isSelected = false;
		}

		public void Clear()
		{
			_keyframes.Clear();
			_isSelected = false;
		}




		// Functions
		//---------------------------------------------
		public void ReadyToAdd()
		{
			Clear();
			_isSelected = true;
		}
		public void AddAnimKeyframe(apAnimKeyframe keyframe, bool isSelected)
		{
			_keyframes.Add(keyframe);
			if(!isSelected)
			{
				//하나라도 선택이 안되었다면 선택 안된걸로 처리
				_isSelected = false;
			}
		}
		


		// Get / Set
		//---------------------------------------------
	}

}