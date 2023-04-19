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
	/// apModifiedVertexRig의 Opt 버전
	/// </summary>
	[Serializable]
	public class apOptModifiedVertexRig
	{
		// Members
		//-----------------------------------------------
		public int _vertexUniqueID = -1;
		public int _vertIndex = -1;

		public apOptMesh _mesh = null;

		[Serializable]
		public class OptWeightPair
		{
			public int _boneID = -1;

			[SerializeField]
			public apOptBone _bone = null;

			public int _meshGroupID = -1;

			[SerializeField]
			public apOptTransform _meshGroup = null;

			public float _weight = 0.0f;

			public OptWeightPair(apModifiedVertexRig.WeightPair srcWeightPair)
			{
				_boneID = srcWeightPair._boneID;
				_meshGroupID = srcWeightPair._meshGroupID;
				_weight = srcWeightPair._weight;
			}

			public bool Link(apPortrait portrait)
			{
				_meshGroup = portrait.GetOptTransformAsMeshGroup(_meshGroupID);
				if (_meshGroup == null)
				{
					Debug.LogError("VertRig Bake 실패 : MeshGroup을 찾을 수 없다. [" + _meshGroupID + "]");
					return false;
				}

				_bone = _meshGroup.GetBone(_boneID);
				if (_bone == null)
				{
					Debug.LogError("VertRig Bake 실패 : Bone을 찾을 수 없다. [" + _boneID + "]");
					return false;
				}

				return true;
			}
		}

		[SerializeField]
		public OptWeightPair[] _weightPairs = null;



		// Init
		//-----------------------------------------------------------
		public apOptModifiedVertexRig()
		{

		}

		public bool Bake(apModifiedVertexRig srcModVert, apOptMesh mesh, apPortrait portrait)
		{
			_vertexUniqueID = srcModVert._vertexUniqueID;
			_vertIndex = srcModVert._vertIndex;
			_mesh = mesh;

			//변경 : 8.2 유효한 Weight Pair만 전달하자
			List<apModifiedVertexRig.WeightPair> validSrcWeightPairs = new List<apModifiedVertexRig.WeightPair>();
			List<OptWeightPair> validOptWeightPairs = new List<OptWeightPair>();//추가 (20.3.30 : 오류 추가 검증용)
			for (int i = 0; i < srcModVert._weightPairs.Count; i++)
			{
				apModifiedVertexRig.WeightPair srcWeightPair = srcModVert._weightPairs[i];
				if(srcWeightPair == null)
				{
					continue;
				}
				if(srcWeightPair._weight <= 0.00001f)
				{
					continue;;
				}
				validSrcWeightPairs.Add(srcWeightPair);
			}

			
			

			for (int i = 0; i < validSrcWeightPairs.Count; i++)
			{
				//apModifiedVertexRig.WeightPair srcWeightPair = srcModVert._weightPairs[i];
				apModifiedVertexRig.WeightPair srcWeightPair = validSrcWeightPairs[i];
				
				//추가 : 유효한 Weight만 추가 > 이 코드가 버그를 발생시킨다.
				//if(srcWeightPair._weight <= 0.00001f)
				//{
				//	continue;
				//}
				OptWeightPair optWeightPair = new OptWeightPair(srcWeightPair);
				bool isValid = optWeightPair.Link(portrait);

				//_weightPairs[i] = optWeightPair;//이전
				if(!isValid)
				{
					//링크 오류 추가 검증
					continue;
				}
				validOptWeightPairs.Add(optWeightPair);
			}


			//유효한 리스트를 복사한다.
			_weightPairs = new OptWeightPair[validOptWeightPairs.Count];
			for (int i = 0; i < validOptWeightPairs.Count; i++)
			{
				_weightPairs[i] = validOptWeightPairs[i];
			}

			float totalWeight = 0.0f;

			for (int i = 0; i < _weightPairs.Length; i++)
			{
				totalWeight += _weightPairs[i]._weight;
			}
			//Noamlize는 1 이상일 때에만
			if (totalWeight > 1.0f)
			{
				for (int i = 0; i < _weightPairs.Length; i++)
				{
					_weightPairs[i]._weight /= totalWeight;
				}
			}
			
			return true;
		}
	}
}