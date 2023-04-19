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
	public class apModifiedVertex
	{
		// Members
		//------------------------------------------
		[NonSerialized]
		public apModifiedMesh _modifiedMesh = null;


		public int _vertexUniqueID = -1;
		public int _vertIndex = -1;

		[NonSerialized]
		public apMesh _mesh = null;

		[NonSerialized]
		public apVertex _vertex = null;

		[SerializeField]
		public Vector2 _deltaPos = Vector2.zero;


		//상위 레이어에서 이미 적용된 값을 Overlap하는 경우 모두 동일한 Layer Weight를 적용하는게 아니라
		//다른 레이어값을 적용한다.
		//단, 레이어 0일땐 Weight를 적용하지 않는다.
		[SerializeField]
		public float _overlapWeight = 1.0f;






		// Init
		//------------------------------------------
		public apModifiedVertex()
		{

		}

		public void Init(int vertUniqueID, apVertex vertex)
		{
			_vertexUniqueID = vertUniqueID;
			_vertex = vertex;
			_deltaPos = Vector2.zero;

			_vertIndex = _vertex._index;
		}

		public void Link(apModifiedMesh modifiedMesh, apMesh mesh, apVertex vertex)
		{
			_modifiedMesh = modifiedMesh;
			_mesh = mesh;
			_vertex = vertex;
			if (_vertex != null)
			{
				_vertIndex = _vertex._index;
			}
			else
			{
				_vertIndex = -1;
			}

			//테스트 : 랜덤값을 넣자
			//_deltaPos = new Vector2(UnityEngine.Random.Range(-20.0f, 20.0f),
			//							UnityEngine.Random.Range(-20.0f, 20.0f));
		}

		// Functions
		//------------------------------------------

		// Get / Set
		//------------------------------------------
	}
}