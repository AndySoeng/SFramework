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
	/// 추가 22.3.20 [v1.4.0] apRenderVertex에 상응하는 Pin에 대한 데이터
	/// </summary>
	public class apRenderPin
	{
		// Members
		//---------------------------------------------------------
		public apMeshPin _srcPin = null;

		public apRenderUnit _parentRenderUnit = null;
		public apMesh _parentMesh = null;

		public apRenderPinGroup _parentRenderPinGroup;
		
		public apRenderPinCurve _prevRenderCurve = null;
		public apRenderPinCurve _nextRenderCurve = null;
		public apRenderPin _prevRenderPin = null;
		public apRenderPin _nextRenderPin = null;

		public Vector2 _pos_World = Vector2.zero;
		public Vector2 _pos_LocalOnMesh = Vector2.zero;
		
		//기본 매트릭스는 필요하다.
		//public apMatrix3x3 _matrix_Static_Vert2Mesh = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_Cal_PinLocal = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_MeshTransform = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_MeshTransform_Inv = apMatrix3x3.identity;

		public Vector2 _deltaPos_PinLocal = Vector2.zero;
		public Vector2 _deltaPos_Pin2Mesh_PinLocal = Vector2.zero;
		public apMatrix3x3 _matrix_ToWorld = apMatrix3x3.identity;//World Matrix가 아닌 World Transformation Matrix다.

		//커브에 의해서 계산된 행렬과 컨트롤 포인트
		//Render Pin을 대상으로 버텍스가 움직이지는 않으므로, 컨트롤 포인트를 계산할 정도의 행렬만 있으면 된다.
		public float _angleCurved = 0.0f;
		public apMatrix3x3 _matrixCurved = apMatrix3x3.identity;
		public Vector2 _controlPointPos_Prev = Vector2.zero;
		public Vector2 _controlPointPos_Next = Vector2.zero;

		//에디터용
		public float _renderWeightByTool = 0.0f;





		// Init
		//---------------------------------------------------------
		public apRenderPin(	apMeshPin pin, 
							apRenderPinGroup parentRenderPinGroup, 
							apRenderUnit parentRenderUnit,
							apMesh parentMesh)
		{
			_srcPin = pin;
			_parentRenderPinGroup = parentRenderPinGroup;
			_parentRenderUnit = parentRenderUnit;
			_parentMesh = parentMesh;


			_pos_World = Vector2.zero;
			_pos_LocalOnMesh = Vector2.zero;

			//_matrix_Static_Vert2Mesh = apMatrix3x3.identity;
			//_matrix_Cal_PinLocal = apMatrix3x3.identity;
			//_matrix_MeshTransform = apMatrix3x3.identity;
			//_matrix_MeshTransform_Inv = apMatrix3x3.identity;

			_deltaPos_PinLocal = Vector2.zero;
			_deltaPos_Pin2Mesh_PinLocal = Vector2.zero;

			_matrix_ToWorld = apMatrix3x3.identity;
		}

		public void LinkData(	apRenderPinCurve prevRenderCurve,
								apRenderPinCurve nextRenderCurve,
								apRenderPin prevRenderPin,
								apRenderPin nextRenderPin)
		{
			_prevRenderCurve = prevRenderCurve;
			_nextRenderCurve = nextRenderCurve;
			_prevRenderPin = prevRenderPin;
			_nextRenderPin = nextRenderPin;
		}


		// Functions ( 모디파이어에 의한 위치 계산 )
		//---------------------------------------------------------
		public void ResetData()
		{
			_pos_World = Vector2.zero;
			_pos_LocalOnMesh = Vector2.zero;

			//_matrix_Static_Vert2Mesh = apMatrix3x3.identity;
			//_matrix_Cal_PinLocal = apMatrix3x3.identity;
			//_matrix_MeshTransform = apMatrix3x3.identity;
			//_matrix_MeshTransform_Inv = apMatrix3x3.identity;

			_deltaPos_PinLocal = Vector2.zero;
			_deltaPos_Pin2Mesh_PinLocal = Vector2.zero;

			_matrix_ToWorld = apMatrix3x3.identity;

			_renderWeightByTool = 0.0f;
		}

		
		// 업데이트
		//public void SetMatrix_1_Static_Vert2Mesh(apMatrix3x3 matrix_Vert2Local)
		//{
		//	_matrix_Static_Vert2Mesh = matrix_Vert2Local;
		//}
		//public void SetMatrix_2_Calculate_PinLocal(Vector2 deltaPos)
		//{
		//	_matrix_Cal_PinLocal = apMatrix3x3.TRS(deltaPos, 0, Vector2.one);
		//}
		//public void SetMatrix_3_Transform_Mesh(apMatrix3x3 matrix_meshTransform, apMatrix3x3 matrix_meshTransformInv)
		//{
		//	_matrix_MeshTransform = matrix_meshTransform;
		//	_matrix_MeshTransform_Inv = matrix_meshTransformInv;
		//}

		////계산!
		//public void Calculate()
		//{
		//	_matrix_ToWorld = _matrix_MeshTransform
		//					* _matrix_Cal_PinLocal
		//					* _matrix_Static_Vert2Mesh;

		//	_pos_World = _matrix_ToWorld.MultiplyPoint(_srcPin._defaultPos);
		//	_pos_LocalOnMesh = _matrix_MeshTransform_Inv.MultiplyPoint(_pos_World);
		//	_isCalculated = true;
		//}



		public void Calculate_PinLocal(ref Vector2 deltaLocal)
		{
			_deltaPos_PinLocal = deltaLocal;
			_deltaPos_Pin2Mesh_PinLocal.x = _parentMesh._matrix_VertToLocal._m02 + _deltaPos_PinLocal.x;
			_deltaPos_Pin2Mesh_PinLocal.y = _parentMesh._matrix_VertToLocal._m12 + _deltaPos_PinLocal.y;
			
			_matrix_ToWorld._m00 = _parentRenderUnit._matrix_TF._m00;
			_matrix_ToWorld._m01 = _parentRenderUnit._matrix_TF._m01;
			_matrix_ToWorld._m02 =	(_parentRenderUnit._matrix_TF._m00 * _deltaPos_Pin2Mesh_PinLocal.x)
									+ (_parentRenderUnit._matrix_TF._m01 * _deltaPos_Pin2Mesh_PinLocal.y)
									+ _parentRenderUnit._matrix_TF._m02;

			_matrix_ToWorld._m10 = _parentRenderUnit._matrix_TF._m10;
			_matrix_ToWorld._m11 = _parentRenderUnit._matrix_TF._m11;
			_matrix_ToWorld._m12 =	(_parentRenderUnit._matrix_TF._m10 * _deltaPos_Pin2Mesh_PinLocal.x)
									+ (_parentRenderUnit._matrix_TF._m11 * _deltaPos_Pin2Mesh_PinLocal.y)
									+ _parentRenderUnit._matrix_TF._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _srcPin._defaultPos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}

		public void Calculate_None()
		{
			_deltaPos_PinLocal.x = 0.0f;
			_deltaPos_PinLocal.y = 0.0f;
			_deltaPos_Pin2Mesh_PinLocal.x = _parentMesh._matrix_VertToLocal._m02;
			_deltaPos_Pin2Mesh_PinLocal.y = _parentMesh._matrix_VertToLocal._m12;
			
			_matrix_ToWorld._m00 = _parentRenderUnit._matrix_TF._m00;
			_matrix_ToWorld._m01 = _parentRenderUnit._matrix_TF._m01;
			_matrix_ToWorld._m02 =	(_parentRenderUnit._matrix_TF._m00 * _deltaPos_Pin2Mesh_PinLocal.x)
									+ (_parentRenderUnit._matrix_TF._m01 * _deltaPos_Pin2Mesh_PinLocal.y)
									+ _parentRenderUnit._matrix_TF._m02;

			_matrix_ToWorld._m10 = _parentRenderUnit._matrix_TF._m10;
			_matrix_ToWorld._m11 = _parentRenderUnit._matrix_TF._m11;
			_matrix_ToWorld._m12 =	(_parentRenderUnit._matrix_TF._m10 * _deltaPos_Pin2Mesh_PinLocal.x)
									+ (_parentRenderUnit._matrix_TF._m11 * _deltaPos_Pin2Mesh_PinLocal.y)
									+ _parentRenderUnit._matrix_TF._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _srcPin._defaultPos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}


		// Functions ( 커브식에 의한 회전 계산 )
		//---------------------------------------------------------
		/// <summary>
		/// 커브 계산 결과에 따른 각도를 지정한다.
		/// 컨트롤 포인트를 계산하기 위한 Matrix도 생성된다.
		/// </summary>
		/// <param name="resultWorldAngle"></param>
		public void SetAngleByCurve(float resultWorldAngle)
		{
			_angleCurved = resultWorldAngle;
			_matrixCurved.SetTRS(_pos_World, _angleCurved, Vector2.one);
		}

		/// <summary>
		/// 컨트롤 포인트를 계산한다. SetAngleByCurve가 호출된 이후여야 한다.
		/// </summary>
		public void CalculateControlPoints()
		{
			float distToPrev = 10.0f;
			float distToNext = 10.0f;
			if(_prevRenderPin != null)
			{
				distToPrev = Vector2.Distance(_prevRenderPin._pos_World, _pos_World);
			}

			if(_nextRenderPin != null)
			{
				distToNext = Vector2.Distance(_nextRenderPin._pos_World, _pos_World);
			}
			
			if(distToPrev < apMeshPin.CONTROL_POINT_LENGTH_MIN)
			{
				distToPrev = apMeshPin.CONTROL_POINT_LENGTH_MIN;
			}

			if(distToNext < apMeshPin.CONTROL_POINT_LENGTH_MIN)
			{
				distToNext = apMeshPin.CONTROL_POINT_LENGTH_MIN;
			}

			distToPrev *= apMeshPin.CONTROL_POINT_LENGTH_RATIO;
			distToNext *= apMeshPin.CONTROL_POINT_LENGTH_RATIO;

			
			_controlPointPos_Prev = _matrixCurved.MultiplyPoint(new Vector2(-distToPrev, 0.0f));
			_controlPointPos_Next = _matrixCurved.MultiplyPoint(new Vector2(distToNext, 0.0f));
		}
	}
}