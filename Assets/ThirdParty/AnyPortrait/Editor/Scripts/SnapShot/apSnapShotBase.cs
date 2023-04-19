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

	public class apSnapShotBase
	{
		// Members
		//-----------------------------------------
		public object _target = null;
		public string _strParam = "";


		// Init
		//-----------------------------------------
		public apSnapShotBase()
		{

		}

		// Functions
		//-----------------------------------------
		public virtual void Clear()
		{

		}


		public virtual bool IsKeySyncable(object target)
		{
			return false;
		}

		public virtual bool Save(object target, string strParam)
		{
			_target = target;
			_strParam = strParam;
			return true;
		}

		public virtual bool Load(object targetObj)
		{
			return false;
		}

		//추가 22.7.10 : 속성을 지정하여 붙여넣는 경우
		public virtual bool LoadWithProperties(	object targetObj,
												bool isVerts,
												bool isPins,
												bool isTransform,
												bool isVisibility,
												bool isColor,
												bool isExtra,
												bool isSelectedOnly,
												List<apModifiedVertex> modVerts,
												List<apModifiedPin> modPins)
		{
			return false;
		}



		public virtual bool IsKeySyncable_MorphMod(object target)
		{
			return false;
		}

		public virtual bool IsKeySyncable_TFMod(object target)
		{
			return false;
		}

		




		// Get / Set
		//-----------------------------------------
	}

}