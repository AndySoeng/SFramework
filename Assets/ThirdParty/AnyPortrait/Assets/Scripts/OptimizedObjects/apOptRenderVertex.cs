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
	public class apOptRenderVertex
	{
		// Members
		//----------------------------------------------
		//Parent MonoBehaviour
		public apOptTransform _parentTransform = null;
		public apOptMesh _parentMesh = null;


		//Vertex의 값에 해당하는 apVertex가 없으므로 바로 Index 접근을 한다.
		//기본 데이터
		public int _uniqueID = -1;
		public int _index;

		public Vector2 _pos_Local = Vector2.zero;
		//public Vector3 _pos3_Local = Vector3.zero;

		public Vector2 _uv = Vector2.zero;

		public float _zDepth = 0.0f;

		//업데이트 데이터
		//public Vector3 _vertPos3_LocalUpdated = Vector3.zero;//삭제 21.5.23 : 최적화

		public Vector2 _vertPos_World = Vector2.zero;
		//public Vector3 _vertPos3_World = Vector3.zero;

		// Transform 데이터들
		//0. Rigging
		//리깅의 경우는 Additive없이 Weight, Pos로만 값을 가져온다.
		//레이어의 영향을 전혀 받지 않는다.
		//구버전 코드 : 
		//public Vector2 _pos_Rigging = Vector2.zero;
		//public float _weight_Rigging = 0.0f;//0이면 Vertex Pos를 사용, 1이면 posRigging을 사용한다. 기본값은 0

		//수정된 코드 : Rigging Matrix로 수정
		//public apMatrix3x3 _matrix_Rigging = apMatrix3x3.identity;//삭제 21.5.24 : 외부의 배열에서 일괄 계산하는 걸로 변경


		//1. [Static] Vert -> Mesh (Pivot)
		[SerializeField]
		public apMatrix3x3 _matrix_Static_Vert2Mesh = apMatrix3x3.identity;

		[SerializeField]
		public apMatrix3x3 _matrix_Static_Vert2Mesh_Inverse = apMatrix3x3.identity;


		//최적화 21.5.23 : 외부에서 계산되는 변수들 삭제. 값 할당하는 시간이 많이 든다.
		////2. [Cal] Vert Local - Blended
		//public apMatrix3x3 _matrix_Cal_VertLocal = apMatrix3x3.identity;

		////3. [TF+Cal] 중첩된 Mesh/MeshGroup Transform
		//public apMatrix3x3 _matrix_MeshTransform = apMatrix3x3.identity;
		

		////4. [Cal] Vert World - Blended
		//public apMatrix3x3 _matrix_Cal_VertWorld = apMatrix3x3.identity;

		////private Vector2 _cal_VertWorld = Vector2.zero;
		
		////5. [TF] Mesh의 Perspective -> Ortho를 위한 변환 매트릭스
		//[NonSerialized]
		//public apMatrix3x3 _matrix_MeshOrthoCorrection = apMatrix3x3.identity;

		//[NonSerialized]
		//public bool _isMeshOrthoCorrection = false;

		////추가 2.25
		////6. [Flip] Flip Multiply 값
		//[NonSerialized]
		//private float _flipWeight_X = 1.0f;
		//[NonSerialized]
		//private float _flipWeight_Y = 1.0f;



		// 계산 완료
		public apMatrix3x3 _matrix_ToWorld = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_ToVert = apMatrix3x3.identity;


		//계산 관련 변수
		[NonSerialized]
		private bool _isCalculated = false;

		[NonSerialized]
		private Vector2 _cal_posLocalUpdated2 = Vector2.zero;

		//TODO : 물리 관련 지연 변수 추가 필요




		// Init
		//----------------------------------------------
		public apOptRenderVertex(apOptTransform parentTransform, apOptMesh parentMesh,
									int vertUniqueID, int vertIndex, Vector2 vertPosLocal,
									Vector2 vertUV, float zDepth)
		{
			_parentTransform = parentTransform;
			_parentMesh = parentMesh;
			_uniqueID = vertUniqueID;
			_index = vertIndex;
			_pos_Local = vertPosLocal;
			_uv = vertUV;
			_zDepth = zDepth;

			//삭제 21.5.23 : 최적화
			//_vertPos3_LocalUpdated.x = _pos_Local.x;
			//_vertPos3_LocalUpdated.y = _pos_Local.y;
			//_vertPos3_LocalUpdated.z = 0;

			_isCalculated = false;

			//_matrix_Rigging = apMatrix3x3.identity;//삭제 21.5.25

			//삭제 21.5.23 : 최적화
			////추가 : 2.25 
			//_flipWeight_X = 1.0f;
			//_flipWeight_Y = 1.0f;

			//_matrix_MeshOrthoCorrection = apMatrix3x3.identity;
			//_isMeshOrthoCorrection = false;
		}

		// Functions
		//----------------------------------------------
		// 준비 + Matrix/Delta Pos 입력
		//---------------------------------------------------------
		//삭제 21.5.22 : 사용되지 않는 초기화 코드
		//public void ReadyToCalculate()
		//{
		//	_matrix_Static_Vert2Mesh = apMatrix3x3.identity;
		//	_matrix_Static_Vert2Mesh_Inverse = apMatrix3x3.identity;

		//	_matrix_Cal_VertLocal = apMatrix3x3.identity;
		//	_matrix_MeshTransform = apMatrix3x3.identity;

		//	_matrix_Cal_VertWorld = apMatrix3x3.identity;
		//	_matrix_ToWorld = apMatrix3x3.identity;
		//	//_matrix_ToVert = apMatrix3x3.identity;
		//	_vertPos_World = Vector2.zero;

		//	//_cal_VertWorld = Vector2.zero;

		//	_vertPos3_LocalUpdated.x = _pos_Local.x;
		//	_vertPos3_LocalUpdated.y = _pos_Local.y;
		//	_vertPos3_LocalUpdated.z = 0;

		//	//추가 : 2.25 
		//	_flipWeight_X = 1.0f;
		//	_flipWeight_Y = 1.0f;

		//	//_pos_Rigging = Vector2.zero;
		//	//_weight_Rigging = 0.0f;

		//	_matrix_Rigging = apMatrix3x3.identity;
		//	_isMeshOrthoCorrection = false;//<<추가
		//}

		//삭제 21.5.25 : LUT 이후로 사용하지 않는 함수
		//public void SetRigging_0_LocalPosWeight(apMatrix3x3 matrix_Rigging, float weight)
		//{
		//	//_pos_Rigging = posRiggingResult;
		//	//_weight_Rigging = weight;

		//	_matrix_Rigging.SetMatrixWithWeight(ref matrix_Rigging, weight);
		//}

		//이 함수는 Bake때 호출된다.
		public void SetMatrix_1_Static_Vert2Mesh(apMatrix3x3 matrix_Vert2Local)
		{
			_matrix_Static_Vert2Mesh = matrix_Vert2Local;
			_matrix_Static_Vert2Mesh_Inverse = _matrix_Static_Vert2Mesh.inverse;
		}


		//삭제 21.5.23
		////삭제 예정
		//public void SetMatrix_2_Calculate_VertLocal(Vector2 deltaPos)
		//{
		//	_matrix_Cal_VertLocal = apMatrix3x3.TRS(deltaPos, 0, Vector2.one);
		//}

		////삭제 예정
		//public void SetMatrix_3_Transform_Mesh(apMatrix3x3 matrix_meshTransform)
		//{
		//	_matrix_MeshTransform = matrix_meshTransform;
		//}

		
		////삭제 예정
		//public void SetMatrix_4_Calculate_VertWorld(Vector2 deltaPos)
		//{
		//	_matrix_Cal_VertWorld = apMatrix3x3.TRS(deltaPos, 0, Vector2.one);
		//	//_cal_VertWorld = deltaPos;
		//}

		////삭제 예정
		//public void SetMatrix_5_OrthoCorrection(apMatrix3x3 matrix_orthoCorrection)
		//{
		//	_matrix_MeshOrthoCorrection = matrix_orthoCorrection;
		//	_isMeshOrthoCorrection = true;
		//}

		////삭제 예정
		//public void SetMatrix_6_FlipWeight(float flipWeightX, float flipWeightY)
		//{
		//	//추가 : 2.25 
		//	_flipWeight_X = flipWeightX;
		//	_flipWeight_Y = flipWeightY;
		//}

		// Calculate
		//---------------------------------------------------------
		//삭제 21.5.23 : 계산 값을 일일이 RenderVertex에 할당한 후 계산하는 방식
		#region [미사용 코드 : 고전적인 방식]
		//public void Calculate()
		//{
		//	//역순으로 World Matrix를 계산하자
		//	//Rigging이 포함된 코드
		//	//_matrix_ToWorld = _matrix_Cal_VertWorld // T
		//	//				* _matrix_MeshTransform // TRS
		//	//				* _matrix_Rigging//<<추가 // TRS
		//	//				* _matrix_Cal_VertLocal // T
		//	//				* _matrix_Static_Vert2Mesh; // T


		//	//단축식을 만들자
		//	//1. MR 00, 01, 10, 11
		//	_matrix_ToWorld._m00 = (_matrix_MeshTransform._m00 * _matrix_Rigging._m00) + (_matrix_MeshTransform._m01 * _matrix_Rigging._m10);
		//	_matrix_ToWorld._m01 = (_matrix_MeshTransform._m00 * _matrix_Rigging._m01) + (_matrix_MeshTransform._m01 * _matrix_Rigging._m11);
		//	_matrix_ToWorld._m10 = (_matrix_MeshTransform._m10 * _matrix_Rigging._m00) + (_matrix_MeshTransform._m11 * _matrix_Rigging._m10);
		//	_matrix_ToWorld._m11 = (_matrix_MeshTransform._m10 * _matrix_Rigging._m01) + (_matrix_MeshTransform._m11 * _matrix_Rigging._m11);

		//	////추가 2.25 : Flip
		//	//_matrix_ToWorld._m00 *= _flipWeight_X;
		//	//_matrix_ToWorld._m11 *= _flipWeight_Y;


		//	//2.
		//	//x=02, y=12
		//	// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
		//	// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
		//	_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (_matrix_Cal_VertLocal._m02 + _matrix_Static_Vert2Mesh._m02)
		//						+ _matrix_ToWorld._m01 * (_matrix_Cal_VertLocal._m12 + _matrix_Static_Vert2Mesh._m12)
		//						+ _matrix_MeshTransform._m00 * _matrix_Rigging._m02
		//						+ _matrix_MeshTransform._m01 * _matrix_Rigging._m12
		//						+ _matrix_Cal_VertWorld._m02
		//						+ _matrix_MeshTransform._m02;

		//	_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (_matrix_Cal_VertLocal._m02 + _matrix_Static_Vert2Mesh._m02)
		//						+ _matrix_ToWorld._m11 * (_matrix_Cal_VertLocal._m12 + _matrix_Static_Vert2Mesh._m12)
		//						+ _matrix_MeshTransform._m10 * _matrix_Rigging._m02
		//						+ _matrix_MeshTransform._m11 * _matrix_Rigging._m12
		//						+ _matrix_Cal_VertWorld._m12
		//						+ _matrix_MeshTransform._m12;

		//	_matrix_ToWorld._m20 = 0;
		//	_matrix_ToWorld._m21 = 0;
		//	_matrix_ToWorld._m22 = 1;



		//	//_matrix_ToVert = _matrix_ToWorld.inverse;

		//	//이전 식
		//	//_vertPos3_World = _matrix_ToWorld.MultiplyPoint3x4(_pos3_Local);

		//	//리깅 포함한 식으로 변경

		//	//리깅 변경 이전 코드
		//	//_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local * (1.0f - _weight_Rigging) + _pos_Rigging * _weight_Rigging);

		//	//리깅 변경 후 코드
		//	_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

		//	//추가 2.26 : Flip
		//	_vertPos_World.x *= _flipWeight_X;
		//	_vertPos_World.y *= _flipWeight_Y;

		//	//_vertPos_World.x = _vertPos3_World.x;
		//	//_vertPos_World.y = _vertPos3_World.y;

		//	if(_isMeshOrthoCorrection)
		//	{
		//		//추가 : Pers -> Ortho Correction을 적용한다.
		//		_cal_posLocalUpdated2 = (_matrix_MeshOrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
		//	}
		//	else
		//	{
		//		_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
		//	}

		//	_vertPos3_LocalUpdated.x = _cal_posLocalUpdated2.x;
		//	_vertPos3_LocalUpdated.y = _cal_posLocalUpdated2.y;
		//	_vertPos3_LocalUpdated.z = _zDepth * 0.01f;

		//	_isCalculated = true;
		//} 
		#endregion


		//변경 21.5.23 : 내부의 변수 할당을 안하고 계산하는 함수
		//조건에 따라 함수들이 조금 다르다.
		//- Vert Local 여부
		//- Vert World 여부
		//- Rigging 여부
		//- OrthoCorrection 여부



		/// <summary>
		/// Calculate [(None)]
		/// </summary>
		public void Calculate_None(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
									float srcFlipWeightX, float srcFlipWeightY,
									ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//단축식을 만들자
			//Rigging이 없다면 > 00, 11, 22 > 1 | 그 외에는 0
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								//+ srcMatx_MeshTFWorld._m00 * 0//리깅이 없으면 삭제
								//+ srcMatx_MeshTFWorld._m01 * 0
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								//+ srcMatx_MeshTFWorld._m10 * 0//리깅이 없으면 삭제
								//+ srcMatx_MeshTFWorld._m11 * 0
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}

		/// <summary>
		/// Calculate [OrthoCorrection]
		/// </summary>
		public void Calculate_OrthoCorrection(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
												ref apMatrix3x3 srcMatx_OrthoCorrection,
												float srcFlipWeightX, float srcFlipWeightY,
												ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//이전
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}



		

		/// <summary>
		/// Calculate [Rigging]
		/// </summary>
		public void Calculate_Rigging(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
										ref apMatrix3x3 srcMatx_Rigging,
										float srcFlipWeightX, float srcFlipWeightY,
										ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


		/// <summary>
		/// Calculate [Rigging, OrthoCorrection]
		/// </summary>
		public void Calculate_Rigging_OrthoCorrection(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
														ref apMatrix3x3 srcMatx_Rigging,
														ref apMatrix3x3 srcMatx_OrthoCorrection,
														float srcFlipWeightX, float srcFlipWeightY,
														ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


			

		/// <summary>
		/// Calculate [VertWorld]
		/// </summary>
		public void Calculate_World(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
										ref Vector2 srcVec2_VertWorld,
										float srcFlipWeightX, float srcFlipWeightY,
										ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


		/// <summary>
		/// Calculate [VertWorld, OrthoCorrection]
		/// </summary>
		public void Calculate_World_OrthoCorrection(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
														ref Vector2 srcVec2_VertWorld,
														ref apMatrix3x3 srcMatx_OrthoCorrection,
														float srcFlipWeightX, float srcFlipWeightY,
														ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}



		/// <summary>
		/// Calculate [VertWorld, Rigging]
		/// </summary>
		public void Calculate_World_Rigging(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
												ref Vector2 srcVec2_VertWorld,
												ref apMatrix3x3 srcMatx_Rigging,
												float srcFlipWeightX, float srcFlipWeightY,
												ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


		/// <summary>
		/// Calculate [VertWorld, Rigging, OrthoCorrection]
		/// </summary>
		public void Calculate_World_Rigging_OrthoCorrection(	ref apMatrix3x3 srcMatx_MeshTFWorld, 
																ref Vector2 srcVec2_VertWorld,
																ref apMatrix3x3 srcMatx_Rigging,
																ref apMatrix3x3 srcMatx_OrthoCorrection,
																float srcFlipWeightX, float srcFlipWeightY,
																ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m01 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * _matrix_Static_Vert2Mesh._m02
								+ _matrix_ToWorld._m11 * _matrix_Static_Vert2Mesh._m12
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}



		//===

		/// <summary>
		/// Calculate [VertLocal]
		/// </summary>
		public void Calculate_Local(	ref Vector2 srcVec2_VertLocal, 
										ref apMatrix3x3 srcMatx_MeshTFWorld, 
										float srcFlipWeightX, float srcFlipWeightY,
										ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}

		/// <summary>
		/// Calculate [VertLocal, OrthoCorrection]
		/// </summary>
		public void Calculate_Local_OrthoCorrection(	ref Vector2 srcVec2_VertLocal, 
														ref apMatrix3x3 srcMatx_MeshTFWorld, 
														ref apMatrix3x3 srcMatx_OrthoCorrection,
														float srcFlipWeightX, float srcFlipWeightY,
														ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}



		

		/// <summary>
		/// Calculate [VertLocal, Rigging]
		/// </summary>
		public void Calculate_Local_Rigging(	ref Vector2 srcVec2_VertLocal, 
												ref apMatrix3x3 srcMatx_MeshTFWorld, 
												ref apMatrix3x3 srcMatx_Rigging,
												float srcFlipWeightX, float srcFlipWeightY,
												ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


		/// <summary>
		/// Calculate [VertLocal, Rigging, OrthoCorrection]
		/// </summary>
		public void Calculate_Local_Rigging_OrthoCorrection(	ref Vector2 srcVec2_VertLocal, 
																ref apMatrix3x3 srcMatx_MeshTFWorld, 
																ref apMatrix3x3 srcMatx_Rigging,
																ref apMatrix3x3 srcMatx_OrthoCorrection,
																float srcFlipWeightX, float srcFlipWeightY,
																ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


			

		/// <summary>
		/// Calculate [VertLocal, VertWorld]
		/// </summary>
		public void Calculate_Local_World(	ref Vector2 srcVec2_VertLocal, 
											ref apMatrix3x3 srcMatx_MeshTFWorld, 
											ref Vector2 srcVec2_VertWorld,
											float srcFlipWeightX, float srcFlipWeightY,
											ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


		/// <summary>
		/// Calculate [VertLocal, VertWorld, OrthoCorrection]
		/// </summary>
		public void Calculate_Local_World_OrthoCorrection(	ref Vector2 srcVec2_VertLocal, 
															ref apMatrix3x3 srcMatx_MeshTFWorld, 
															ref Vector2 srcVec2_VertWorld,
															ref apMatrix3x3 srcMatx_OrthoCorrection,
															float srcFlipWeightX, float srcFlipWeightY,
															ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			//리깅이 없는 경우 : 그대로 복사
			_matrix_ToWorld._m00 = srcMatx_MeshTFWorld._m00;
			_matrix_ToWorld._m01 = srcMatx_MeshTFWorld._m01;
			_matrix_ToWorld._m10 = srcMatx_MeshTFWorld._m10;
			_matrix_ToWorld._m11 = srcMatx_MeshTFWorld._m11;

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}



		/// <summary>
		/// Calculate [VertLocal, VertWorld, Rigging]
		/// </summary>
		public void Calculate_Local_World_Rigging(	ref Vector2 srcVec2_VertLocal, 
													ref apMatrix3x3 srcMatx_MeshTFWorld, 
													ref Vector2 srcVec2_VertWorld,
													ref apMatrix3x3 srcMatx_Rigging,
													float srcFlipWeightX, float srcFlipWeightY,
													ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//이 함수에서는 srcMatx_OrthoCorrection가 없다.
			_cal_posLocalUpdated2 = (_matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}


		/// <summary>
		/// Calculate [VertLocal, VertWorld, Rigging, OrthoCorrection]
		/// </summary>
		public void Calculate_Local_World_Rigging_OrthoCorrection(	ref Vector2 srcVec2_VertLocal, 
																	ref apMatrix3x3 srcMatx_MeshTFWorld, 
																	ref Vector2 srcVec2_VertWorld,
																	ref apMatrix3x3 srcMatx_Rigging,
																	ref apMatrix3x3 srcMatx_OrthoCorrection,
																	float srcFlipWeightX, float srcFlipWeightY,
																	ref Vector3 dstVertResult)
		{
			//역순으로 World Matrix를 계산하자
			//Rigging이 포함된 코드
			

			//단축식을 만들자
			//1. MR 00, 01, 10, 11
			_matrix_ToWorld._m00 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m01 = (srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m11);
			_matrix_ToWorld._m10 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m00) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m10);
			_matrix_ToWorld._m11 = (srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m01) + (srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m11);

			//2.
			//x=02, y=12
			// X : MR00(Lx+Px) + MR01(Ly+Py) + M00Rx + M01Ry + Wx + Mx
			// Y : MR10(Lx+Px) + MR11(Ly+Py) + M10Rx + M11Ry + Wy + My
			_matrix_ToWorld._m02 = _matrix_ToWorld._m00 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m01 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m00 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m01 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.x
								+ srcMatx_MeshTFWorld._m02;

			_matrix_ToWorld._m12 = _matrix_ToWorld._m10 * (srcVec2_VertLocal.x + _matrix_Static_Vert2Mesh._m02)
								+ _matrix_ToWorld._m11 * (srcVec2_VertLocal.y + _matrix_Static_Vert2Mesh._m12)
								+ srcMatx_MeshTFWorld._m10 * srcMatx_Rigging._m02
								+ srcMatx_MeshTFWorld._m11 * srcMatx_Rigging._m12
								+ srcVec2_VertWorld.y
								+ srcMatx_MeshTFWorld._m12;

			_matrix_ToWorld._m20 = 0;
			_matrix_ToWorld._m21 = 0;
			_matrix_ToWorld._m22 = 1;

			_vertPos_World = _matrix_ToWorld.MultiplyPoint(_pos_Local);

			//추가 2.26 : Flip
			_vertPos_World.x *= srcFlipWeightX;
			_vertPos_World.y *= srcFlipWeightY;

			//[OrthoCorrection] : 이 함수에서는 srcMatx_OrthoCorrection를 적용한다.
			//_cal_posLocalUpdated2 = (srcMatx_OrthoCorrection * _matrix_Static_Vert2Mesh_Inverse).MultiplyPoint(_vertPos_World);
			//변경 21.5.25
			apMatrix3x3.MultiplyPointToMultipliedMatrix(	ref _cal_posLocalUpdated2, 
															ref srcMatx_OrthoCorrection, 
															ref _matrix_Static_Vert2Mesh_Inverse,
															ref _vertPos_World);
			
			dstVertResult.x = _cal_posLocalUpdated2.x;
			dstVertResult.y = _cal_posLocalUpdated2.y;
			dstVertResult.z = _zDepth * 0.01f;
			
			_isCalculated = true;
		}
		
		// Get / Set
		//----------------------------------------------
		public bool IsCalculated { get { return _isCalculated; } }
	}
}