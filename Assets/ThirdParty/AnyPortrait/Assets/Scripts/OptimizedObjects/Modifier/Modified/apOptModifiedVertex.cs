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

	[Serializable]
	public class apOptModifiedVertex
	{
		// Members
		//--------------------------------------------
		//버텍스 비교 없이 바로 Index 접근을 한다. (Bake 된 상태이므로 불필요한 체크 필요 없음)
		public int _vertUniqueID = -1;
		public int _vertIndex = -1;

		[SerializeField]
		public apOptMesh _mesh = null;

		public Vector2 _deltaPos = Vector2.zero;



		// Init
		//--------------------------------------------
		public apOptModifiedVertex()
		{

		}

		//public void Init(int vertUniqueID, int vertIndex, apOptMesh mesh, Vector2 deltaPos)
		//{
		//	_vertUniqueID = vertUniqueID;
		//	_vertIndex = vertIndex;
		//	_mesh = mesh;
		//	_deltaPos = deltaPos;
		//}

		public void Bake(apModifiedVertex srcModVert, apOptMesh mesh)
		{
			_vertUniqueID = srcModVert._vertexUniqueID;
			_vertIndex = srcModVert._vertIndex;
			_mesh = mesh;
			_deltaPos = srcModVert._deltaPos;
		}

		//추가 22.4.11 [v1.4.0]
		/// <summary>
		/// 핀 등에 의해서 추가적으로 움직이는 경우, DeltaPos에 값을 더하자
		/// </summary>
		/// <param name="deltaPos"></param>
		public void AddDeltaPos(Vector2 deltaPos)
		{
			_deltaPos += deltaPos;
		}

		// Functions
		//--------------------------------------------


		// Get / Set
		//--------------------------------------------
	}
}