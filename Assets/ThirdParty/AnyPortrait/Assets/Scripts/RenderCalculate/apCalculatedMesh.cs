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

	public class apCalculatedMesh
	{
		// Members
		//--------------------------------------
		//키값
		public apRenderUnit _renderUnit = null;
		public apTransform_Mesh _transformMesh = null;//<<이게 키값
		public apMesh _mesh;

		//Mesh의 Transform 정보가 있는가
		public bool _isMeshTransformCalculated = false;
		public apMatrix3x3 _meshTransformMatrix = apMatrix3x3.identity;

		//Mesh의 Visible 변경 정보가 있는가
		public bool _isMeshVisibleCalculated = false;
		public bool _isMeshVisible = true;//키애니메이션/컨트롤러에 따라 Visible 여부가 결정된다.

		//Mesh의 Color 정보가 있다
		public bool _isColorCalculated = false;
		public Color _color2X = Color.black;//칼라도 결정된다.

		//Vert -> Mesh Space
		public bool _isVertLocalMatrixCalculated = false;
		public Dictionary<apVertex, apMatrix3x3> _localMatrixPerVertex = new Dictionary<apVertex, apMatrix3x3>();

		//Mesh Space -> WorldSpace (이건 MeshTransfrom 이후)
		public bool _isVertWorldMatrixCalculated = false;
		public Dictionary<apVertex, apMatrix3x3> _worldMatrixPerVertex = new Dictionary<apVertex, apMatrix3x3>();

		// Init
		//--------------------------------------
		public apCalculatedMesh(apMesh mesh)
		{
			_mesh = mesh;
			_localMatrixPerVertex.Clear();
			for (int i = 0; i < _mesh._vertexData.Count; i++)
			{
				_localMatrixPerVertex.Add(_mesh._vertexData[i], apMatrix3x3.identity);
				_worldMatrixPerVertex.Add(_mesh._vertexData[i], apMatrix3x3.identity);
			}

		}


		public void Clear()
		{
			_isMeshTransformCalculated = false;
			_meshTransformMatrix = apMatrix3x3.identity;

			//Mesh의 Visible 변경 정보가 있는가
			_isMeshVisibleCalculated = false;
			_isMeshVisible = true;//키애니메이션/컨트롤러에 따라 Visible 여부가 결정된다.

			//Mesh의 Color 정보가 있다
			_isColorCalculated = false;
			_color2X = Color.black;//칼라도 결정된다.

			//Vert -> Mesh Space
			_isVertLocalMatrixCalculated = false;
			//_localMatrixPerVertex.Clear();

			//Mesh Space -> WorldSpace (이건 MeshTransfrom 이후)
			_isVertWorldMatrixCalculated = false;
			//_worldMatrixPerVertex.Clear();
		}

		public void ReadyToUpdate()
		{
			_isMeshTransformCalculated = false;
			_meshTransformMatrix = apMatrix3x3.identity;

			_isMeshVisibleCalculated = false;
			_isMeshVisible = true;//키애니메이션/컨트롤러에 따라 Visible 여부가 결정된다.

			_isColorCalculated = false;
			_color2X = Color.black;//칼라도 결정된다.

			//Vert -> Mesh Space
			_isVertLocalMatrixCalculated = false;

			//Mesh Space -> WorldSpace (이건 MeshTransfrom 이후)
			_isVertWorldMatrixCalculated = false;

			for (int i = 0; i < _mesh._vertexData.Count; i++)
			{
				_localMatrixPerVertex[_mesh._vertexData[i]] = apMatrix3x3.identity;
				_worldMatrixPerVertex[_mesh._vertexData[i]] = apMatrix3x3.identity;
			}
		}


		// Functions
		//--------------------------------------
		public void SetMeshTransform(apMatrix3x3 meshTransformMatrix)
		{
			_isMeshTransformCalculated = true;
			_meshTransformMatrix = meshTransformMatrix;
		}

		public void SetMeshVisible(bool isVisible)
		{
			_isMeshVisibleCalculated = true;
			_isMeshVisible = isVisible;
		}

		public void SetColor(Color color2X)
		{
			_isColorCalculated = true;
			_color2X = color2X;
		}

		public void AddVertLocalMatrix(apVertex vert, apMatrix3x3 localMatrix)
		{
			_isVertLocalMatrixCalculated = true;
			_localMatrixPerVertex[vert] = localMatrix;
		}

		public void AddVertWorldMatrix(apVertex vert, apMatrix3x3 worldMatrix)
		{
			_isVertWorldMatrixCalculated = true;
			_worldMatrixPerVertex[vert] = worldMatrix;
		}


		// Get
		//--------------------------------------
	}
}