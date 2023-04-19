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
	public class apSyncSet_ControlParam
	{
		// Members
		//-----------------------------------------
		public apControlParam _controlParam = null;
		public apControlParam _syncTargetControlParam = null;
		public bool _isSync = false;

		// Init
		//-----------------------------------------
		public apSyncSet_ControlParam(apControlParam controlParam, apControlParam syncTargetControlParam)
		{
			_controlParam = controlParam;
			_syncTargetControlParam = syncTargetControlParam;
			_isSync = _syncTargetControlParam != null;
		}


		// Function
		//-----------------------------------------
		public void Sync()
		{
			if(!_isSync)
			{
				return;
			}

			switch (_controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					_controlParam._int_Cur = Mathf.Clamp(_syncTargetControlParam._int_Cur, _controlParam._int_Min, _controlParam._int_Max);
					break;

				case apControlParam.TYPE.Float:
					_controlParam._float_Cur = Mathf.Clamp(_syncTargetControlParam._float_Cur, _controlParam._float_Min, _controlParam._float_Max);
					break;

				case apControlParam.TYPE.Vector2:
					_controlParam._vec2_Cur.x = Mathf.Clamp(_syncTargetControlParam._vec2_Cur.x, _controlParam._vec2_Min.x, _controlParam._vec2_Max.x);
					_controlParam._vec2_Cur.y = Mathf.Clamp(_syncTargetControlParam._vec2_Cur.y, _controlParam._vec2_Min.y, _controlParam._vec2_Max.y);
					break;
			}
			
			
			
		}
	}
}