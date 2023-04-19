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

	public class apCalculatedRenderUnit
	{
		// Members
		//--------------------------------------
		public apRenderUnit _renderUnit = null;//<<이게 키값
		public apMeshGroup _meshGroup;


		//Calculated 멤버들
		//리스트로 만들어서 빨리 순회하도록 한다.
		//2. Mesh Transform
		public List<apCalculatedMesh> _calMeshes = new List<apCalculatedMesh>();

		//3. Mesh Group
		public List<apCalculatedMeshGroup> _calMeshGroups = new List<apCalculatedMeshGroup>();


		//public Dictionary<apMesh, apModifiedResult_Mesh> _meshResults = new Dictionary<apMesh, apModifiedResult_Mesh>();

		// Init
		//--------------------------------------
		public apCalculatedRenderUnit()
		{

		}


		public void SetMeshGroup(apMeshGroup meshGroup)
		{
			_meshGroup = meshGroup;
		}

		public void Clear()
		{
			//_meshResults.Clear();

		}

		//public void AddMesh(apMesh mesh)
		//{
		//	if(!_meshResults.ContainsKey(mesh))
		//	{
		//		_meshResults.Add(mesh, new apModifiedResult_Mesh(mesh));
		//	}
		//}

		// Functions
		//--------------------------------------
		public void ReadyToCalculate()
		{
			//foreach (KeyValuePair<apMesh, apModifiedResult_Mesh> meshResult in _meshResults)
			//{
			//	meshResult.Value.ReadyToUpdate();
			//}
		}

		//public void SetMeshTransform(apMesh mesh, apMatrix3x3 meshTransformMatrix)
		//{
		//	_meshResults[mesh].SetMeshTransform(meshTransformMatrix);
		//}

		//public void SetMeshVisible(apMesh mesh, bool isVisible)
		//{
		//	_meshResults[mesh].SetMeshVisible(isVisible);
		//}

		//public void SetColor(apMesh mesh, Color color2X)
		//{
		//	_meshResults[mesh].SetColor(color2X);
		//}

		//public void AddVertLocalMatrix(apMesh mesh, apVertex vert, apMatrix3x3 localMatrix)
		//{
		//	_meshResults[mesh].AddVertLocalMatrix(vert, localMatrix);
		//}

		//public void AddVertWorldMatrix(apMesh mesh, apVertex vert, apMatrix3x3 worldMatrix)
		//{
		//	_meshResults[mesh].AddVertWorldMatrix(vert, worldMatrix);
		//}

		// Get
		//--------------------------------------
		//public bool IsMeshContain(apMesh mesh) { return _meshResults.ContainsKey(mesh); }
		//public bool IsMeshTransformCalculated(apMesh mesh) { return _meshResults[mesh]._isMeshTransformCalculated; }
		//public bool IsMeshVisibleCalculated(apMesh mesh) { return _meshResults[mesh]._isMeshVisibleCalculated; }
		//public bool IsMeshColorCalculated(apMesh mesh) { return _meshResults[mesh]._isColorCalculated; }
		//public bool IsVertLocalMatrixCalculated(apMesh mesh) { return _meshResults[mesh]._isVertLocalMatrixCalculated; }
		//public bool IsVertWorldMatrixCalculated(apMesh mesh) { return _meshResults[mesh]._isVertWorldMatrixCalculated; }

		//public apMatrix3x3 GetMeshTransformMatrix(apMesh mesh) { return _meshResults[mesh]._meshTransformMatrix; }
		//public bool GetMeshVisible(apMesh mesh) { return _meshResults[mesh]._isMeshVisible; }
		//public Color GetMeshColor(apMesh mesh) { return _meshResults[mesh]._color2X; }
		//public apMatrix3x3 GetVertLocalMatrix(apMesh mesh, apVertex vert) { return _meshResults[mesh]._localMatrixPerVertex[vert]; }
		//public apMatrix3x3 GetVertWorldMatrix(apMesh mesh, apVertex vert) { return _meshResults[mesh]._worldMatrixPerVertex[vert]; }

	}

}