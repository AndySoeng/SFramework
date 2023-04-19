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

	//Transform-Mesh에서 작업한 VertMorph 내용이 100% 적용되는건 아니다
	//전체 중에서 "일부 Vertex만 적용"할 수 있도록 별도의 VertexWeight를 저장한다. (Layer가 0이 아닐 경우 적용)
	//값은 저장되며, 리스트 순서는 Vertex ID와 Index를 기준으로 배열 순서를 잘 맞춘다.
	//정렬은 자동이다.
	[Serializable]
	public class apOptParamSetGroupVertWeight
	{
		// Members
		//---------------------------------------------
		[SerializeField]
		public List<float> _vertWeightList = new List<float>();//기존의 WeightedVertex와 달리 Weight값만 가진다. (동기화가 끝났으므로)

		[SerializeField]
		public int _meshTransform_ID = -1;

		[NonSerialized]
		public apOptTransform _optTransform = null;

		// Init
		//---------------------------------------------
		public apOptParamSetGroupVertWeight()
		{

		}

		public void Bake(apModifierParamSetGroupVertWeight srcWeightedVert)
		{
			_vertWeightList.Clear();

			for (int i = 0; i < srcWeightedVert._weightedVerts.Count; i++)
			{
				_vertWeightList.Add(Mathf.Clamp01(srcWeightedVert._weightedVerts[i]._adaptWeight));
			}


			_meshTransform_ID = srcWeightedVert._meshTransform_ID;
			_optTransform = null;
		}

		public void Link(apOptTransform optTransform)
		{
			_optTransform = optTransform;
		}

		// Functions
		//---------------------------------------------
	}
}