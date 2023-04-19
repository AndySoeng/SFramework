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
	//추가 21.9.14 : 본을 연결해서 실시간 리타게팅을 구현하자
	//본과 함께 부모-자식 관계를 가진다.
	public class apSyncSet_Bone
	{
		// Members
		//-----------------------------------------------
		public apOptBone _bone = null;
		public apOptBone _syncTargetBone = null;
		public bool _isSync = false;

		public apSyncSet_Bone _parentSyncSet = null;
		public List<apSyncSet_Bone> _childSyncSets = null;

		// Init
		//-----------------------------------------------
		public apSyncSet_Bone(apOptBone bone, apOptBone syncTargetBone)
		{
			_bone = bone;
			_syncTargetBone = syncTargetBone;
			_isSync = _syncTargetBone != null;

			_bone._syncBone = syncTargetBone;

			_parentSyncSet = null;
			_childSyncSets = null;

			//생성과 동시에 현재 본에 "동기화되었음"을 알려주자
		}

		public void SetParent(apSyncSet_Bone parentSyncSet)
		{
			if(parentSyncSet._childSyncSets == null)
			{
				parentSyncSet._childSyncSets = new List<apSyncSet_Bone>();
			}
			parentSyncSet._childSyncSets.Add(this);

			_parentSyncSet = parentSyncSet;
		}

		// Functions
		//-----------------------------------------------
		public void Unsync()
		{
			_bone._syncBone = null;
		}
		//public void Sync()
		//{
		//	if(!_isSync)
		//	{
		//		return;
		//	}

		//	//Sync 본의 Delta 데이터를 그대로 넘겨준다.
		//	//_bone.UpdateModifiedValue(_syncTargetBone._defaultMatrix)
			
		//	//그냥 값을 복사하자
		//	_bone.CopyWorldMatrixFromBone(_syncTargetBone);
		//}
	}
}