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
	/// ModParamSetGroup이 동일한 AnimClip에 대해서 각각의 Layer로 정의되므로
	/// 이를 묶어서 AnimClip 단위로 한번에 참조하고 싶을때 사용하는 클래스
	/// 저장되지 않으며, Animation에서만 사용하는 클래스이다.
	/// 실제 렌더링 파이프라인에서는 사용하지 않으며 UI에서 주로 사용 (직렬화가 되지 않는다)
	/// OPT 버전은 없다.
	/// </summary>
	public class apModifierParamSetGroupAnimPack
	{
		// Members
		//--------------------------------------------------------------
		[NonSerialized]
		private apModifierBase _parentModifier = null;

		[NonSerialized]
		private apAnimClip _linkedAnimClip = null;

		[NonSerialized]
		private List<apModifierParamSetGroup> _paramSetGroups = new List<apModifierParamSetGroup>();

		// Init
		//--------------------------------------------------------------
		public apModifierParamSetGroupAnimPack(apModifierBase modifier, apAnimClip animClip)
		{
			_parentModifier = modifier;
			_linkedAnimClip = animClip;

			_paramSetGroups.Clear();
		}

		// Functions
		//--------------------------------------------------------------
		public void Clear()
		{
			_paramSetGroups.Clear();
		}

		public void AddParamSetGroup(apModifierParamSetGroup paramSetGroup)
		{
			if (!_paramSetGroups.Contains(paramSetGroup))
			{
				_paramSetGroups.Add(paramSetGroup);
			}
		}

		public void RemoveInvalidParamSetGroup(List<apModifierParamSetGroup> paramSetGroupList)
		{
			_paramSetGroups.RemoveAll(delegate (apModifierParamSetGroup a)
			{
				if (!paramSetGroupList.Contains(a))
				{
					return true;
				}
				return false;
			});
		}


		// Get / Set
		//--------------------------------------------------------------
		public apModifierBase ParentModifier { get { return _parentModifier; } }
		public apAnimClip LinkedAnimClip { get { return _linkedAnimClip; } }
		public List<apModifierParamSetGroup> ParamSetGroups { get { return _paramSetGroups; } }
	}

}