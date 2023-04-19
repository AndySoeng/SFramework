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
	/// 동기화된 애니메이션 처리를 위해서 "대상 애니메이션"과 "동기화의 대상인 애니메이션"을 저장한다.
	/// </summary>
	public class apSyncSet_AnimClip
	{
		// Members
		//-----------------------------------------
		public apAnimClip _animClip = null;
		public apAnimClip _syncTargetAnimClip = null;
		public bool _isSync = false;

		//동기화 방식은 apAnimPlayMecanim.MecanimClipData와 유사하게 동작한다.
		//미리 PlayUnit을 만들고, 실행중일때는 연결, 그렇지 않을때는 해제한다.
		public apAnimPlayUnit _playUnit = null;
		

		// Init
		//-----------------------------------------
		public apSyncSet_AnimClip(apAnimClip animClip, apAnimClip syncTargetAnimClip)
		{
			_animClip = animClip;
			_syncTargetAnimClip = syncTargetAnimClip;
			_isSync = _syncTargetAnimClip != null;

			_playUnit = new apAnimPlayUnit(null, -1, -1);
			_playUnit.SetMecanimPlayUnit();

			_animClip.Stop_Opt(false);
			_animClip._parentPlayUnit = null;
		}


		// Function
		//-----------------------------------------
		/// <summary>
		/// 동기화 후 애니메이션 업데이트를 한다.
		/// </summary>
		public void SyncAndUpdate()
		{
			if (_syncTargetAnimClip != null)
			{
				//1. 동기화로 연결된게 있다면
				//- 상태를 동기화한다.
				if (_syncTargetAnimClip._parentPlayUnit != null)
				{
					//1-1. PlayUnit이 존재하여 동기화되었다면
					//- PlayUnit이 없다면 생성
					//- 동기화된 객체는 Mecanim과 같은 방식으로 동작한다.
					if(_animClip._parentPlayUnit != _playUnit)
					{
						//연결부터 한다.
						_playUnit.Mecanim_Link(_animClip);
					}

					//동기화를 하면서 업데이트를 한다.
					_playUnit.Sync_Update(_syncTargetAnimClip._parentPlayUnit, _syncTargetAnimClip);
				}
				else
				{
					//1-2. PlayUnit이 존재하지 않는 상태라면
					if (_playUnit._linkedAnimClip != null
						|| _animClip._parentPlayUnit != null)
					{
						//연결된게 없는데 PlayUnit이 있으면 해제한다.
						_playUnit.Mecanim_Unlink();
						_animClip._parentPlayUnit = null;
						
					}
				}
			}
			else
			{
				//2. 동기화로 연결된게 없다면
				if (_playUnit._linkedAnimClip != null
						|| _animClip._parentPlayUnit != null)
				{
					//연결된게 없는데 PlayUnit이 있으면 해제한다.
					_playUnit.Mecanim_Unlink();
					_animClip._parentPlayUnit = null;
				}
			}
		}
	}
}