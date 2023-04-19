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
	/// 기존의 optModifiedMesh에서 Physics 부분만 가져온 클래스
	/// 최적화를 위해서 분리가 되었다.
	/// 데이터가 훨씬 더 최적화되었다.
	/// </summary>
	[Serializable]
	public class apOptModifiedMesh_Physics
	{
		// Members
		//--------------------------------------------
		//[NonSerialized]
		//private apOptModifiedMeshSet _parentModMeshSet = null;

		[SerializeField]
		public apOptPhysicsMeshParam _physicMeshParam = null;

		[SerializeField]
		public int _nVertWeights = 0;
		
		[SerializeField]
		public apOptModifiedPhysicsVertex[] _vertWeights = null;

		// Init
		//--------------------------------------------
		public apOptModifiedMesh_Physics()
		{

		}

		public void Link(apPortrait portrait, apOptModifiedMeshSet parentModMeshSet)
		{
			//_parentModMeshSet = parentModMeshSet;

			if(_physicMeshParam != null)
			{
				_physicMeshParam.Link(portrait);
			}

			if (_nVertWeights > 0)
			{
				for (int i = 0; i < _nVertWeights; i++)
				{
					_vertWeights[i].Link(this,
											parentModMeshSet,
											parentModMeshSet._targetTransform,
											//_targetMesh.RenderVertices[_vertWeights[i]._vertIndex]//이전
											parentModMeshSet._targetMesh.RenderVertices[i]//<<변경 (이미 정렬되어 있으므로 i=vertIndex 이다.)
											);
				}
			}
			
		}

		// Init - Bake
		//--------------------------------------------
		public void Bake(apPhysicsMeshParam srcPhysicParam, List<apModifiedVertexWeight> modVertWeights, apPortrait portrait)
		{
			
			_nVertWeights = modVertWeights.Count;
			_vertWeights = new apOptModifiedPhysicsVertex[_nVertWeights];


			
			_physicMeshParam = new apOptPhysicsMeshParam();
			_physicMeshParam.Bake(srcPhysicParam);
			_physicMeshParam.Link(portrait);

			for (int i = 0; i < _nVertWeights; i++)
			{
				apOptModifiedPhysicsVertex optModPhysicVert = new apOptModifiedPhysicsVertex();
				apModifiedVertexWeight srcModVertWeight = modVertWeights[i];
				optModPhysicVert.Bake(srcModVertWeight);

				_vertWeights[i] = optModPhysicVert;
			}
		}



		

	}
}