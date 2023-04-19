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

	//추가 : Vertex Work Weight
	//Transform-Mesh에서 작업한 VertMorph 내용이 100% 적용되는건 아니다
	//전체 중에서 "일부 Vertex만 적용"할 수 있도록 별도의 VertexWeight를 저장한다. (Layer가 0이 아닐 경우 적용)
	//값은 저장되며, 리스트 순서는 Vertex ID와 Index를 기준으로 배열 순서를 잘 맞춘다.
	//정렬은 자동이다.
	[Serializable]
	public class apModifierParamSetGroupVertWeight
	{
		// Members
		//---------------------------------------------
		[Serializable]
		public class WeightedVertex
		{
			/// <summary>Vertex의 UniqueID</summary>
			public int _uniqueID = -1;

			/// <summary>Vertex List에서의 순서 Index</summary>
			public int _vertIndex = -1;

			/// <summary>Vertex Modifier 처리시 일부만 적용하는 Weight</summary>
			public float _adaptWeight = 1.0f;

			/// <summary>
			/// 백업용 생성자
			/// </summary>
			public WeightedVertex()
			{

			}

			public WeightedVertex(int uniqueID, int vertIndex)
			{
				_uniqueID = uniqueID;
				_vertIndex = vertIndex;

				_adaptWeight = 1.0f;
			}
		}

		[SerializeField]
		public List<WeightedVertex> _weightedVerts = new List<WeightedVertex>();

		[SerializeField]
		public int _meshTransform_ID = -1;

		[NonSerialized]
		public apTransform_Mesh _meshTransform = null;

		[NonSerialized]
		public bool _isSync = false;

		// Init
		//---------------------------------------------
		/// <summary>
		/// 백업용 생성자
		/// </summary>
		public apModifierParamSetGroupVertWeight()
		{

		}

		public apModifierParamSetGroupVertWeight(apTransform_Mesh meshTransform)
		{
			_meshTransform = meshTransform;
			_meshTransform_ID = _meshTransform._transformUniqueID;
			_weightedVerts.Clear();

			if (_meshTransform._mesh != null)
			{
				List<apVertex> verts = _meshTransform._mesh._vertexData;
				for (int i = 0; i < verts.Count; i++)
				{
					_weightedVerts.Add(new WeightedVertex(verts[i]._uniqueID, i));
				}
			}
		}


		public void LinkMeshTransform(apTransform_Mesh meshTransform)
		{
			_meshTransform = meshTransform;
			if (_meshTransform == null)
			{
				_meshTransform_ID = -1;
			}

			RefreshVertexList();
		}

		public void RefreshVertexList()
		{
			//Vertex 리스트가 Mesh의 Vert 리스트와 동기화가 되었는지 확인한다.
			//동기화되지 않았다면 ID, 순서를 모두 다시 맞춰준다.
			bool isVertChanged = false;
			if (_meshTransform_ID < 0)
			{
				_weightedVerts.Clear();
				return;
			}

			if (_meshTransform == null || _meshTransform._mesh == null)
			{
				return;
			}

			List<apVertex> srcVerts = _meshTransform._mesh._vertexData;
			int nVerts = srcVerts.Count;

			if (srcVerts.Count != _weightedVerts.Count)
			{
				isVertChanged = true;
			}
			else
			{
				for (int i = 0; i < nVerts; i++)
				{
					if (srcVerts[i]._uniqueID != _weightedVerts[i]._uniqueID)
					{
						isVertChanged = true;
						break;
					}
				}
			}

			if (isVertChanged)
			{
				List<WeightedVertex> nextVerts = new List<WeightedVertex>();

				//Vertex가 동기화되지 않았다.
				//기존에 있는 값은 그대로 쓰고, 없는건 새로 만들자
				for (int i = 0; i < nVerts; i++)
				{
					int curUniqueID = srcVerts[i]._uniqueID;
					WeightedVertex existVert = GetWeightedVert(curUniqueID);
					if (existVert != null)
					{
						existVert._vertIndex = i;//<<인덱스는 바꾸고..
						nextVerts.Add(existVert);
					}
					else
					{
						nextVerts.Add(new WeightedVertex(curUniqueID, i));
					}
				}

				_weightedVerts.Clear();
				for (int i = 0; i < nVerts; i++)
				{
					_weightedVerts.Add(nextVerts[i]);
				}
			}

		}

		// Functions
		//---------------------------------------------
		public WeightedVertex GetWeightedVert(int vertUniqueID)
		{
			return _weightedVerts.Find(delegate (WeightedVertex a)
			{
				return a._uniqueID == vertUniqueID;
			});
		}
	}
}