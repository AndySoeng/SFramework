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
using System.Runtime.InteropServices;

namespace AnyPortrait
{

	[Serializable]
	public class apTransform_MeshGroup
	{
		// Members
		//--------------------------------------------
		[SerializeField]
		public int _meshGroupUniqueID = -1;

		[SerializeField]
		public int _transformUniqueID = -1;

		[SerializeField]
		public string _nickName = "";

		[NonSerialized]
		public apMeshGroup _meshGroup = null;

		[SerializeField]
		public apMatrix _matrix = new apMatrix();//이건 기본 Static Matrix

		[SerializeField]
		public Color _meshColor2X_Default = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[SerializeField]
		public bool _isVisible_Default = true;

		[SerializeField]
		public int _depth = 0;

		//삭제 [v1.4.2]
		//[SerializeField]
		//public int _level = 0;//Parent부터 내려오는 Level


		//추가 : Socket
		//Bake할때 소켓을 생성한다.
		[SerializeField]
		public bool _isSocket = false;


		//[SerializeField]
		//public Color _color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[NonSerialized]
		public apMatrix _matrix_TF_ParentWorld = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_ToParent = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TF_LocalModified = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_World = new apMatrix();

		[NonSerialized]
		public apMatrix _matrix_TFResult_WorldWithoutMod = new apMatrix();

		//계산용 변수
		///// <summary>Parent로부터 누적된 WorldMatrix. 자기 자신의 Matrix는 포함되지 않는다.</summary>
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_Parent = apMatrix3x3.identity;

		////추가
		///// <summary>누적되지 않은 기본 Pivot Transform + Modifier 결과만 가지고 있는 값이다.</summary>
		//[NonSerialized]
		//public apMatrix3x3 _matrix_TF_Cal_Local = apMatrix3x3.identity;


		//private apMatrix _calculateTmpMatrix = new apMatrix();
		//public apMatrix CalculatedTmpMatrix {  get { return _calculateTmpMatrix; } }

		//private apMatrix _calculateTmpMatrix_Local = new apMatrix();


		[NonSerialized]
		public apRenderUnit _linkedRenderUnit = null;

		//[NonSerialized]
		//public bool _isVisible_TmpWork = true;//<<이값이 false이면 아예 렌더링이 안된다. 작업용. 허용되는 경우 외에는 항상 True

		// Init
		//--------------------------------------------
		/// <summary>
		/// 백업용 생성자. 코드에서 호출하지 말자
		/// </summary>
		public apTransform_MeshGroup()
		{
			
		}
		public apTransform_MeshGroup(int uniqueID)
		{
			_transformUniqueID = uniqueID;
		}

		public void RegistIDToPortrait(apPortrait portrait)
		{
			portrait.RegistUniqueID(apIDManager.TARGET.Transform, _transformUniqueID);
		}

		// Functions
		//--------------------------------------------
		public void ReadyToCalculate()
		{
			_matrix.MakeMatrix();

			//_matrix_TF_Cal_Parent = apMatrix3x3.identity;
			//_matrix_TF_Cal_Local = _matrix.MtrxToSpace;

			//if(_calculateTmpMatrix == null)
			//{
			//	_calculateTmpMatrix = new apMatrix();
			//}
			//_calculateTmpMatrix.SetIdentity();
			//_calculateTmpMatrix.SetMatrix(_matrix);

			//if(_calculateTmpMatrix_Local == null)
			//{
			//	_calculateTmpMatrix_Local = new apMatrix();
			//}
			//_calculateTmpMatrix_Local.SetIdentity();
			//_calculateTmpMatrix_Local.SetMatrix(_matrix);

			//변경
			//[Parent World x To Parent x Local TF] 조합으로 변경

			if (_matrix_TF_ParentWorld == null)				{ _matrix_TF_ParentWorld = new apMatrix(); }
			if (_matrix_TF_ToParent == null)				{ _matrix_TF_ToParent = new apMatrix(); }
			if (_matrix_TF_LocalModified == null)			{ _matrix_TF_LocalModified = new apMatrix(); }
			if (_matrix_TFResult_World == null)				{ _matrix_TFResult_World = new apMatrix(); }
			if (_matrix_TFResult_WorldWithoutMod == null)	{ _matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			_matrix_TF_ParentWorld.SetIdentity();
			_matrix_TF_ToParent.SetIdentity();
			_matrix_TF_LocalModified.SetIdentity();

			//ToParent는 Pivot이므로 고정
			_matrix_TF_ToParent.SetMatrix(_matrix, true);

			_matrix_TFResult_World.SetIdentity();
			_matrix_TFResult_WorldWithoutMod.SetIdentity();
		}

		public void SetModifiedTransform(apMatrix matrix_modified)
		{
			////_calculateTmpMatrix_Local.SRMultiply(matrix_modified, true);//Parent
			//_calculateTmpMatrix_Local.SRMultiply(matrix_modified, false);//Child

			//_matrix_TF_Cal_Local = _calculateTmpMatrix_Local.MtrxToSpace;

			_matrix_TF_LocalModified.SetMatrix(matrix_modified, false);
		}


		/// <summary>
		/// Parent의 Matrix를 추가한다. (Parent x This)
		/// </summary>
		/// <param name="matrix_parentTransform"></param>
		//public void AddWorldMatrix_Parent(apMatrix3x3 matrix_parentTransform)
		public void AddWorldMatrix_Parent(apMatrix matrix_parentTransform)
		{
			//_matrix_TF_Cal_Parent = matrix_parentTransform.MtrxToSpace * _matrix_TF_Cal_Parent;
			////_calculateTmpMatrix.SRMultiply(matrix_parentTransform, true);
			////_matrix_TF_Cal_ToWorld = _calculateTmpMatrix.MtrxToSpace;

			_matrix_TF_ParentWorld.SetMatrix(matrix_parentTransform, false);
		}


		public void MakeTransformMatrix()
		{

			//중요! 계산에 적용되는 Matrix를 여기서 Make하자
			//_matrix_TF_ToParent.MakeMatrix();
			_matrix_TF_LocalModified.MakeMatrix();
			_matrix_TF_ParentWorld.MakeMatrix();

			//1) SR Multiply로 만드는 경우
			//[SR]
			//_matrix_TFResult_World.SRMultiply(_matrix_TF_LocalModified, true);
			//_matrix_TFResult_World.SRMultiply(_matrix_TF_ToParent, true);
			//_matrix_TFResult_World.SRMultiply(_matrix_TF_ParentWorld, true);

			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ToParent, true);
			//_matrix_TFResult_WorldWithoutMod.SRMultiply(_matrix_TF_ParentWorld, true);

			//[R]
			//추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//위치 이동 20.10.28
			//_matrix_TFResult_World.OnBeforeRMultiply();
			//_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();

			//_matrix_TFResult_World.RMultiply(_matrix_TF_ToParent, false);//이전
			_matrix_TFResult_World.SetMatrix(_matrix_TF_ToParent, false);//20.10.28 : 첫 계산이므로 Set으로 변경
			_matrix_TFResult_World.OnBeforeRMultiply();//20.10.28 : 위치 이동 (버그때문에)
			

			_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified, false);
			_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld, true);

			//_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ToParent, false);//이전
			_matrix_TFResult_WorldWithoutMod.SetMatrix(_matrix_TF_ToParent, false);//20.10.28 : 첫 계산이므로 Set으로 변경
			_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();//20.10.28 : 위치 이동 (버그때문에)


			_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorld, true);
		}


#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void MatrixWrap_MakeMatrix(	ref Vector2 pos, float angleDeg, ref Vector2 scale, bool isInverseCalculate,
													ref apMatrix3x3 dstMtrxToSpace, ref apMatrix3x3 dstOnlyRotation,
													ref apMatrix3x3 dstMtrxToLowerSpace, ref apMatrix3x3 dstOnlyRotationInv,
													ref bool dstIsInverseCalculated_Space, ref bool dstIsInverseCalculated_OnlyRotation);
		
		#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void MatrixWrap_Set_OnBefore_RMul1(ref Vector2 dst_pos, ref float dst_angleDeg, ref Vector2 dst_scale,
																ref bool dst_isInitScalePositive_X, ref bool dst_isInitScalePositive_Y, ref bool dst_isAngleFlipped,
																ref apMatrix3x3 dst_mtrxToSpace, ref apMatrix3x3 dst_onlyRotation,
																ref bool dst_isInverseCalculated_Space, ref bool dst_isInverseCalculated_OnlyRotation,
																ref Vector2 srcSet_pos, float srcSet_angleDeg, ref Vector2 srcSet_scale,
																ref Vector2 srcRMul_pos, float srcRMul_angleDeg, ref Vector2 srcRMul_scale, ref apMatrix3x3 srcRMul_onlyRotation);

		#if UNITY_EDITOR_WIN
		[DllImport("AnyPortrait_Editor_Win64")]
#else
		[DllImport("AnyPortrait_Editor_MAC")]
#endif
		private static extern void MatrixWrap_Set_OnBefore_RMul2(ref Vector2 dst_pos, ref float dst_angleDeg, ref Vector2 dst_scale,
																ref bool dst_isInitScalePositive_X, ref bool dst_isInitScalePositive_Y, ref bool dst_isAngleFlipped,
																ref apMatrix3x3 dst_mtrxToSpace, ref apMatrix3x3 dst_onlyRotation,
																ref bool dst_isInverseCalculated_Space, ref bool dst_isInverseCalculated_OnlyRotation,
																ref Vector2 srcSet_pos, float srcSet_angleDeg, ref Vector2 srcSet_scale,
																ref Vector2 srcRMul1_pos, float srcRMul1_angleDeg, ref Vector2 srcRMul1_scale, ref apMatrix3x3 srcRMul1_onlyRotation,
																ref Vector2 srcRMul2_pos, float srcRMul2_angleDeg, ref Vector2 srcRMul2_scale, ref apMatrix3x3 srcRMul2_onlyRotation
																);


		/// <summary>
		/// MakeTransformMatrix의 C++ DLL 버전
		/// </summary>
		public void MakeTransformMatrix_DLL()
		{

			//중요! 계산에 적용되는 Matrix를 여기서 Make하자
			//기존 코드
			//_matrix_TF_LocalModified.MakeMatrix();
			//_matrix_TF_ParentWorld.MakeMatrix();

			//< C++ DLL >
			MatrixWrap_MakeMatrix(	ref _matrix_TF_LocalModified._pos, _matrix_TF_LocalModified._angleDeg, ref _matrix_TF_LocalModified._scale, 
									false,
									ref _matrix_TF_LocalModified._mtrxToSpace, ref _matrix_TF_LocalModified._mtrxOnlyRotation,
									ref _matrix_TF_LocalModified._mtrxToLowerSpace, ref _matrix_TF_LocalModified._mtrxOnlyRotationInv,
									ref _matrix_TF_LocalModified._isInverseCalculated_Space, ref _matrix_TF_LocalModified._isInverseCalculated_OnlyRotation);

			MatrixWrap_MakeMatrix(	ref _matrix_TF_ParentWorld._pos, _matrix_TF_ParentWorld._angleDeg, ref _matrix_TF_ParentWorld._scale, 
									false,
									ref _matrix_TF_ParentWorld._mtrxToSpace, ref _matrix_TF_ParentWorld._mtrxOnlyRotation,
									ref _matrix_TF_ParentWorld._mtrxToLowerSpace, ref _matrix_TF_ParentWorld._mtrxOnlyRotationInv,
									ref _matrix_TF_ParentWorld._isInverseCalculated_Space, ref _matrix_TF_ParentWorld._isInverseCalculated_OnlyRotation);


			//기존 코드
			//_matrix_TFResult_World.SetMatrix(_matrix_TF_ToParent, false);//20.10.28 : 첫 계산이므로 Set으로 변경
			//_matrix_TFResult_World.OnBeforeRMultiply();//20.10.28 : 위치 이동 (버그때문에)
			
			//_matrix_TFResult_World.RMultiply(_matrix_TF_LocalModified, false);
			//_matrix_TFResult_World.RMultiply(_matrix_TF_ParentWorld, true);

			//< C++ DLL >
			MatrixWrap_Set_OnBefore_RMul2(ref _matrix_TFResult_World._pos, ref _matrix_TFResult_World._angleDeg, ref _matrix_TFResult_World._scale,
																ref _matrix_TFResult_World._isInitScalePositive_X, ref _matrix_TFResult_World._isInitScalePositive_Y, ref _matrix_TFResult_World._isAngleFlipped,
																ref _matrix_TFResult_World._mtrxToSpace, ref _matrix_TFResult_World._mtrxOnlyRotation,
																ref _matrix_TFResult_World._isInverseCalculated_Space, ref _matrix_TFResult_World._isInverseCalculated_OnlyRotation,
																ref _matrix_TF_ToParent._pos, _matrix_TF_ToParent._angleDeg, ref _matrix_TF_ToParent._scale,
																ref _matrix_TF_LocalModified._pos, _matrix_TF_LocalModified._angleDeg, ref _matrix_TF_LocalModified._scale, ref _matrix_TF_LocalModified._mtrxOnlyRotation,
																ref _matrix_TF_ParentWorld._pos, _matrix_TF_ParentWorld._angleDeg, ref _matrix_TF_ParentWorld._scale, ref _matrix_TF_ParentWorld._mtrxOnlyRotation
																);

			//기존
			//_matrix_TFResult_WorldWithoutMod.SetMatrix(_matrix_TF_ToParent, false);//20.10.28 : 첫 계산이므로 Set으로 변경
			//_matrix_TFResult_WorldWithoutMod.OnBeforeRMultiply();//20.10.28 : 위치 이동 (버그때문에)
			//_matrix_TFResult_WorldWithoutMod.RMultiply(_matrix_TF_ParentWorld, true);

			//< C++ DLL >
			MatrixWrap_Set_OnBefore_RMul1(ref _matrix_TFResult_WorldWithoutMod._pos, ref _matrix_TFResult_WorldWithoutMod._angleDeg, ref _matrix_TFResult_WorldWithoutMod._scale,
										ref _matrix_TFResult_WorldWithoutMod._isInitScalePositive_X, ref _matrix_TFResult_WorldWithoutMod._isInitScalePositive_Y, ref _matrix_TFResult_WorldWithoutMod._isAngleFlipped,
										ref _matrix_TFResult_WorldWithoutMod._mtrxToSpace, ref _matrix_TFResult_WorldWithoutMod._mtrxOnlyRotation,
										ref _matrix_TFResult_WorldWithoutMod._isInverseCalculated_Space, ref _matrix_TFResult_WorldWithoutMod._isInverseCalculated_OnlyRotation,
										ref _matrix_TF_ToParent._pos, _matrix_TF_ToParent._angleDeg, ref _matrix_TF_ToParent._scale,
										ref _matrix_TF_ParentWorld._pos, _matrix_TF_ParentWorld._angleDeg, ref _matrix_TF_ParentWorld._scale, ref _matrix_TF_ParentWorld._mtrxOnlyRotation);
		}

		

		// Get / Set
		//--------------------------------------------
	}
}