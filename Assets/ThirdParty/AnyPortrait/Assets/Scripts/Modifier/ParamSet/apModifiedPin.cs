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
	/// apModifiedVertex에 상응하는 Pin에 대한 데이터
	/// </summary>
	[Serializable]
	public class apModifiedPin
	{
		// Members
		//----------------------------------------------------------
		[NonSerialized]
		public apModifiedMesh _modifiedMesh = null;

		public int _pinUniqueID = -1;

		[NonSerialized]
		public apMesh _mesh = null;

		[NonSerialized]
		public apMeshPin _pin = null;

		[SerializeField]
		public Vector2 _deltaPos = Vector2.zero;


		// Init
		//----------------------------------------------------------
		public apModifiedPin() { }

		public void Init(int pinUniqueID, apMeshPin pin)
		{
			_pinUniqueID = pinUniqueID;

			_pin = pin;
			_deltaPos = Vector2.zero;
		}

		public void Link(apModifiedMesh modifiedMesh, apMesh mesh, apMeshPin pin)
		{
			_modifiedMesh = modifiedMesh;
			_mesh = mesh;
			_pin = pin;
		}
	}
}