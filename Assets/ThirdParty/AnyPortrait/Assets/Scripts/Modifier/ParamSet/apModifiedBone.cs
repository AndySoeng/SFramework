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
	/// Modifier에 의해서 변동된 내역이 저장되는 클래스
	/// ParamSet에 포함되며, ModifiedMesh와 동등한 레벨에서 처리된다.
	/// MeshGroup 내의 Bone 한개에 대한 정보를 가지고 있다.
	/// </summary>
	[Serializable]
	public class apModifiedBone
	{
		// Members
		//------------------------------------------
		public int _meshGroupUniqueID_Modifier = -1;
		public int _meshGropuUniqueID_Bone = -1;

		public int _transformUniqueID = -1;//_meshGropuUniqueID_Bone의 MeshGroupTransform이다.

		[NonSerialized]
		public apMeshGroup _meshGroup_Modifier = null;

		[NonSerialized]
		public apMeshGroup _meshGroup_Bone = null;

		/// <summary>
		/// 선택된 Bone의 MeshGroup(RenderUnit)이 포함된 루트 MeshGroupTransform
		/// </summary>
		[NonSerialized]
		public apTransform_MeshGroup _meshGroupTransform = null;

		public int _boneID = -1;

		[NonSerialized]
		public apBone _bone = null;

		[NonSerialized]
		public apRenderUnit _renderUnit = null;//Parent MeshGroup의 RenderUnit

		/// <summary>
		/// Bone 제어 정보.
		/// </summary>
		[SerializeField]
		public apMatrix _transformMatrix = new apMatrix();

		//>>> 이거 다시 없앰. Control Param의 값을 따르는 것으로 변경.. 으으..
		////추가 : 5.9
		////본 + IK Controller인 경우
		////Position Controller와 LookAt Controller의 Mix Weight를 설정할 수 있다.
		//[SerializeField]
		//public float _boneIKController_MixWeight = 0.0f;

		// Init
		//------------------------------------------
		public apModifiedBone()
		{

		}
		public void Init(int meshGroupID_Modifier, int meshGroupID_Bone, int meshGroupTransformID, apBone bone)
		{
			_meshGroupUniqueID_Modifier = meshGroupID_Modifier;
			_meshGropuUniqueID_Bone = meshGroupID_Bone;
			_transformUniqueID = meshGroupTransformID;

			_bone = bone;
			_boneID = bone._uniqueID;
		}

		//TODO Link 등등
		//에디터에서 제대로 Link를 해야한다.
		public void Link(apMeshGroup meshGroup_Modifier, apMeshGroup meshGroup_Bone, apBone bone, apRenderUnit renderUnit, apTransform_MeshGroup meshGroupTransform)
		{
			_meshGroup_Modifier = meshGroup_Modifier;
			_meshGroup_Bone = meshGroup_Bone;
			_bone = bone;
			_renderUnit = renderUnit;

			
			_meshGroupTransform = meshGroupTransform;


			//if (_meshGroup_Bone != meshGroup_Modifier)
			//{
			//	//Debug.Log(" ------------Sub Bone의 Link ------------");
			//	if (_renderUnit == null)
			//	{
			//		//Debug.LogError("<<< Render Unit이 Null이다. >>> ");
			//	}
			//	Debug.Log("meshGroup_Modifier : " + (meshGroup_Modifier == null ? "NULL" : meshGroup_Modifier._name));
			//	Debug.Log("_meshGroup_Bone : " + (_meshGroup_Bone == null ? "NULL" : _meshGroup_Bone._name));
			//	Debug.Log("_bone : " + (_bone == null ? "NULL" : _bone._name));
			//	Debug.Log("_meshGroupTransform : " + (_meshGroupTransform == null ? "NULL" : _meshGroupTransform._nickName));
			//	Debug.Log("_transformUniqueID : " + _transformUniqueID);
			//}
			
		}




		// Functions
		//------------------------------------------
		public void UpdateBeforeBake(apPortrait portrait, apMeshGroup mainMeshGroup, apTransform_MeshGroup mainMeshGroupTransform)
		{

		}


		// Get / Set
		//------------------------------------------

	}
}