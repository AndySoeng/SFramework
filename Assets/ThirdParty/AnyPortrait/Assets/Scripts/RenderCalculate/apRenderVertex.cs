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

	public class apRenderVertex
	{
		//Members
		//------------------------------------
		public apRenderUnit _parentRenderUnit = null;
		public apMeshGroup _parentMeshGroup = null;
		public apMesh _parentMesh = null;
		public int _indexOfRenderUnit = 0;

		public apVertex _vertex = null;

		/// <summary>
		/// 툴에서 사용하는 Weight. Editor 렌더링용
		/// </summary>
		public float _renderWeightByTool = 0.0f;

		/// <summary>
		/// 툴에서 사용하는 WeightColor. Bone이나 Physic 등
		/// </summary>
		public Color _renderColorByTool = Color.gray;

		//추가 19.7.30 : GUI에서 Rigging Weight를 표현할 때, 원으로 표현하기 위해서 추가된 파라미터
		public RigWeightParams _renderRigWeightParam = new RigWeightParams();
		

		//GUI 에서 사용하는 파라미터 추가 (ex.Physic에서 Main, Contraint 등)
		public int _renderParam = 0;



		//0. Rigging
		//리깅의 경우는 Additive없이 Weight, Pos로만 값을 가져온다.
		//레이어의 영향을 전혀 받지 않는다.
		//삭제 22.5.9 [v1.4.0] : 연산 최적화
		//public Vector2 _pos_Rigging = Vector2.zero;
		//public float _weight_Rigging = 0.0f;//0이면 Vertex Pos를 사용, 1이면 posRigging을 사용한다. 기본값은 0

		//수정된 리깅 : Pos가 아닌 Matrix를 받아봅시다.
		//Vert -> [Rigging] -> Local -> TF -> World 에서
		//Vert -> Local -> [Rigging] -> TF -> World 로 수정
		public apMatrix3x3 _matrix_Rigging = apMatrix3x3.identity;

		//최적화 계획 22.5.8
		//(1) _matrix_Static_Vert2Mesh :			삭제. _parent.VertToLocal 그대로 이용할 것
		//(2) _matrix_Cal_VertLocal :				Delta Pos만 받아서 계산. Matrix 계산도 분해해서 계산한다.
		//(3) _matrix_MeshTransform / Inv :			RenderUnit 단위로 저장
		//(4) _matrix_Rigging (C++용 PosRigging) :	Weight에 의해서 계산. 단 성분 나눠서 불필요한 할당은 줄인다.
		//(4) _matrix_Cal_VertWorld :				Delta Pos만 받아서 계산. Matrix 계산도 분해해서 계산한다.

		//행렬 계산의 경우 (연산 순서대로 적는다)
		//대부분 Gizmo 연산용이다.
		//(1) _matrix_ToWorld : 풀어서 직접 계산한다.
		//(2) _matrix_Cal_VertLocal * _matrix_Static_Vert2Mesh : Position만 설정할 것이므로, DeltaPos를 이용하여 연산하자.
		//(3) _matrix_Cal_VertWorld * _matrix_MeshTransform * _matrix_Rigging : 자주 사용하므로



		//< 이전 (삭제 v1.4.0) >
		//1. [Static] Vert -> Mesh (Pivot)
		//public apMatrix3x3 _matrix_Static_Vert2Mesh = apMatrix3x3.identity;

		//2. [Cal] Vert Local - Blended
		//public apMatrix3x3 _matrix_Cal_VertLocal = apMatrix3x3.identity;

		//3. [TF+Cal] 중첩된 Mesh/MeshGroup Transform (Parent / Local로 나뉨)
		//public apMatrix3x3 _matrix_MeshTransform = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_MeshTransform_Inv = apMatrix3x3.identity;


		//< 최적화 > [22.5.8 : v1.4.0]
		public Vector2 _deltaPos_VertLocal = Vector2.zero;
		public Vector2 _deltaPos_VertWorld = Vector2.zero;
		public Vector2 _deltaPos_Vert2Mesh_VertLocal = Vector2.zero;
		public apMatrix3x3 _matrix_Rigging_MeshTF_VertWorld = apMatrix3x3.identity;




		//4. [Cal] Vert World - Blended
		public apMatrix3x3 _matrix_Cal_VertWorld = apMatrix3x3.identity;

		//private Vector2 _cal_VertWorld = Vector2.zero;

		//변경
		//고정적인 Matrix가 아니라, Modifier의 실제 호출 주체의 계층 순위에 따라서 Stack을 다시 짜서 결과를 각각 저장해야한다.
		//public List<MatrixStack> _matrixStack = new List<MatrixStack>();

		// 계산 완료
		public apMatrix3x3 _matrix_ToWorld = apMatrix3x3.identity;
		//public apMatrix3x3 _matrix_ToVert = apMatrix3x3.identity;

		//결과
		//GL에 표시하거나 물리 계산에 사용할 World Position
		//public Vector3 _pos_World3 = Vector3.zero;
		public Vector2 _pos_World = Vector2.zero;

		//추가
		//GL 출력용 좌표
		//GL에서 직접 연산한다.
		public Vector2 _pos_GL = Vector2.zero;

		//Realtime의 Mesh에 포함시킬 Local Pos이다. (Mesh의 invWorldMatrix의 
		public Vector2 _pos_LocalOnMesh = Vector2.zero;


		//private bool _isCalculated = false;//삭제 22.5.9 [v1.4.0] : 사용하지 않음


		//Modifier가 적용되지 않은 World Pos (요청시에만 값이 계산된다)
		public Vector2 _pos_World_NoMod = Vector2.zero;



		// Init
		//------------------------------------
		public apRenderVertex(apRenderUnit parentRenderUnit, apMeshGroup parentMeshGroup, apMesh parentMesh, apVertex vertex, int indexOfRenderUnit)
		{
			_parentRenderUnit = parentRenderUnit;
			_parentMeshGroup = parentMeshGroup;
			_parentMesh = parentMesh;
			_vertex = vertex;
			_indexOfRenderUnit = indexOfRenderUnit;
			//_isCalculated = false;//삭제 22.5.9

			//삭제 22.5.9
			//_matrix_Static_Vert2Mesh = apMatrix3x3.identity;
			//_matrix_Cal_VertLocal = apMatrix3x3.identity;
			//_matrix_MeshTransform = apMatrix3x3.identity;
			//_matrix_MeshTransform_Inv = apMatrix3x3.identity;

			_matrix_Cal_VertWorld = apMatrix3x3.identity;
			_matrix_ToWorld = apMatrix3x3.identity;
			_pos_World = Vector2.zero;

			//삭제 22.5.9
			//_pos_Rigging = Vector2.zero;
			//_weight_Rigging = 0.0f;

			_matrix_Rigging = apMatrix3x3.identity;

			if(_renderRigWeightParam == null)
			{
				_renderRigWeightParam = new RigWeightParams();
			}


			//추가 22.5.9 [v1.4.0]
			_deltaPos_VertLocal = Vector2.zero;
			_deltaPos_VertWorld = Vector2.zero;
			_deltaPos_Vert2Mesh_VertLocal = Vector2.zero;
			_matrix_Rigging_MeshTF_VertWorld = apMatrix3x3.identity;
		}



		// Functions
		//------------------------------------
		public void ResetData()
		{
			//삭제 22.5.9
			//_matrix_Static_Vert2Mesh = apMatrix3x3.identity;
			//_matrix_Cal_VertLocal = apMatrix3x3.identity;
			//_matrix_MeshTransform = apMatrix3x3.identity;
			//_matrix_MeshTransform_Inv = apMatrix3x3.identity;

			_matrix_Cal_VertWorld = apMatrix3x3.identity;
			_matrix_ToWorld = apMatrix3x3.identity;
			_pos_World = Vector2.zero;

			//_pos_Rigging = Vector2.zero;
			//_weight_Rigging = 0.0f;

			_matrix_Rigging = apMatrix3x3.identity;

			//추가 22.5.9 [v1.4.0]
			_deltaPos_VertLocal = Vector2.zero;
			_deltaPos_VertWorld = Vector2.zero;
			_deltaPos_Vert2Mesh_VertLocal = Vector2.zero;
			_matrix_Rigging_MeshTF_VertWorld = apMatrix3x3.identity;

			_renderWeightByTool = 0.0f;
			_renderColorByTool = Color.gray;
			_renderRigWeightParam.Clear();
		}

		//이거 호출 안되네요
		//-> 호출하는 것으로 변경
		#region [미사용 코드] : 최적화되지 않은 코드 (v1.4.0 이전)
		//public void ReadyToCalculate()
		//{	
		//	_pos_Rigging = Vector2.zero;
		//	_weight_Rigging = 0.0f;
		//}


		//public void SetRigging_0_LocalPosWeight(Vector2 posRiggingResult, float weight, apMatrix3x3 matrix_rigging)
		//{
		//	_pos_Rigging = posRiggingResult;
		//	_weight_Rigging = weight;

		//	_matrix_Rigging.SetMatrixWithWeight(ref matrix_rigging, _weight_Rigging);

		//	//if(_vertex._index == 0)
		//	//{
		//	//	Debug.LogError("Rigging Matrix(" + _weight_Rigging + ") : \n" + matrix_rigging.ToString() + "\n>>\n" + _matrix_Rigging.ToString());
		//	//}
		//}


		////스텝별로 하나씩 세팅하자
		//public void SetMatrix_1_Static_Vert2Mesh(apMatrix3x3 matrix_Vert2Local)
		//{
		//	_matrix_Static_Vert2Mesh = matrix_Vert2Local;
		//}


		//public void SetMatrix_2_Calculate_VertLocal(Vector2 deltaPos)
		//{
		//	_matrix_Cal_VertLocal = apMatrix3x3.TRS(deltaPos);
		//}

		//public void SetMatrix_3_Transform_Mesh(apMatrix3x3 matrix_meshTransform, apMatrix3x3 matrix_meshTransformInv)
		//{
		//	_matrix_MeshTransform = matrix_meshTransform;
		//	_matrix_MeshTransform_Inv = matrix_meshTransformInv;
		//}

		//public void SetMatrix_4_Calculate_VertWorld(Vector2 deltaPos)
		//{
		//	_matrix_Cal_VertWorld = apMatrix3x3.TRS(deltaPos);
		//	//_cal_VertWorld = deltaPos;
		//}

		//public void Calculate()
		//{

		//	//역순으로 World Matrix를 계산하자
		//	//1. 기존 식
		//	//(Vert -> Rigging : ITP) -> (V2Mesh -> Local Morph -> Mesh TF -> World Morph : WorldMTX) 방식
		//	//_matrix_ToWorld = _matrix_Cal_VertWorld
		//	//				* _matrix_MeshTransform
		//	//				* _matrix_Cal_VertLocal
		//	//				* _matrix_Static_Vert2Mesh;

		//	//_pos_World = _matrix_ToWorld.MultiplyPoint(_vertex._pos * (1.0f - _weight_Rigging) + _pos_Rigging * _weight_Rigging);

		//	//2. 변경된 식
		//	//Vert -> (V2Mesh -> Local Morph -> RiggingMTX -> MeshTF -> World Morph : WorldMTX) 방식
		//	_matrix_ToWorld = _matrix_Cal_VertWorld
		//					* _matrix_MeshTransform
		//					* _matrix_Rigging//<<이게 추가
		//					* _matrix_Cal_VertLocal
		//					* _matrix_Static_Vert2Mesh;

		//	//기존
		//	//_pos_World = _matrix_ToWorld.MultiplyPoint(_vertex._pos);

		//	//테스트
		//	_pos_World = _matrix_Static_Vert2Mesh.MultiplyPoint(_vertex._pos);
		//	_pos_World = _matrix_Cal_VertLocal.MultiplyPoint(_pos_World);
		//	_pos_World = _matrix_Rigging.MultiplyPoint(_pos_World);
		//	_pos_World = _matrix_MeshTransform.MultiplyPoint(_pos_World);
		//	_pos_World = _matrix_Cal_VertWorld.MultiplyPoint(_pos_World);

		//	//World -> Local
		//	//_pos_LocalOnMesh = (_matrix_MeshTransform.inverse).MultiplyPoint(_pos_World);//일일이 inverse 계산
		//	_pos_LocalOnMesh = _matrix_MeshTransform_Inv.MultiplyPoint(_pos_World);//계산된 inverse 가져오기


		//	//삭제 22.5.9
		//	//_isCalculated = true;

		//} 
		#endregion



		//---------------------------------------------------------------------------

		//추가 22.5.9 : 최적화된 계산
		// Rigging + Local + World
		public void Calculate_All(	ref float riggingWeight, ref apMatrix3x3 matrix_rigging, //Step 0
									ref Vector2 deltaLocal, //Step2 Vert Local
									ref Vector2 deltaWorld //Step4 Vert World
									)
		{
			//1. Rigging
			_matrix_Rigging._m00 = (matrix_rigging._m00 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m01 = matrix_rigging._m01 * riggingWeight;
			_matrix_Rigging._m02 = matrix_rigging._m02 * riggingWeight;
			_matrix_Rigging._m10 = matrix_rigging._m10 * riggingWeight;
			_matrix_Rigging._m11 = (matrix_rigging._m11 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m12 = matrix_rigging._m12 * riggingWeight;
			
			//2. Delta Local/World
			_deltaPos_VertLocal = deltaLocal;
			_deltaPos_VertWorld = deltaWorld;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02 + _deltaPos_VertLocal.x;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12 + _deltaPos_VertLocal.y;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			float matrixTFVertWorld_X = _parentRenderUnit._matrix_TF._m02 + _deltaPos_VertWorld.x;
			float matrixTFVertWorld_Y = _parentRenderUnit._matrix_TF._m12 + _deltaPos_VertWorld.y;
			
			_matrix_Rigging_MeshTF_VertWorld._m00 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m10) + (matrixTFVertWorld_X * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m01 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m11) + (matrixTFVertWorld_X * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m02 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m12) + (matrixTFVertWorld_X * _matrix_Rigging._m22);

			_matrix_Rigging_MeshTF_VertWorld._m10 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m10) + (matrixTFVertWorld_Y * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m11 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m11) + (matrixTFVertWorld_Y * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m12 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m12) + (matrixTFVertWorld_Y * _matrix_Rigging._m22);

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}


		//Rigging - OFF / DeltaLocal - OFF / DeltaWorld - OFF
		public void Calculate_None()
		{
			//1. Rigging - Identity
			_matrix_Rigging._m00 = 1.0f;	_matrix_Rigging._m01 = 0.0f;	_matrix_Rigging._m02 = 0.0f;
			_matrix_Rigging._m10 = 0.0f;	_matrix_Rigging._m11 = 1.0f;	_matrix_Rigging._m12 = 0.0f;
			
			//2. Delta Local/World - Zero
			_deltaPos_VertLocal.x = 0.0f;
			_deltaPos_VertLocal.y = 0.0f;
			_deltaPos_VertWorld.x = 0.0f;
			_deltaPos_VertWorld.y = 0.0f;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			_matrix_Rigging_MeshTF_VertWorld._m00 = _parentRenderUnit._matrix_TF._m00;
			_matrix_Rigging_MeshTF_VertWorld._m01 = _parentRenderUnit._matrix_TF._m01;
			_matrix_Rigging_MeshTF_VertWorld._m02 = _parentRenderUnit._matrix_TF._m02;

			_matrix_Rigging_MeshTF_VertWorld._m10 = _parentRenderUnit._matrix_TF._m10;
			_matrix_Rigging_MeshTF_VertWorld._m11 = _parentRenderUnit._matrix_TF._m11;
			_matrix_Rigging_MeshTF_VertWorld._m12 = _parentRenderUnit._matrix_TF._m12;

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}





		// Rigging
		public void Calculate_Rigging(	ref float riggingWeight, ref apMatrix3x3 matrix_rigging)
		{
			//1. Rigging
			_matrix_Rigging._m00 = (matrix_rigging._m00 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m01 = matrix_rigging._m01 * riggingWeight;
			_matrix_Rigging._m02 = matrix_rigging._m02 * riggingWeight;
			_matrix_Rigging._m10 = matrix_rigging._m10 * riggingWeight;
			_matrix_Rigging._m11 = (matrix_rigging._m11 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m12 = matrix_rigging._m12 * riggingWeight;
			
			//2. Delta Local/World
			_deltaPos_VertLocal.x = 0.0f;
			_deltaPos_VertLocal.y = 0.0f;
			_deltaPos_VertWorld.x = 0.0f;
			_deltaPos_VertWorld.y = 0.0f;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			_matrix_Rigging_MeshTF_VertWorld._m00 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m10) + (_parentRenderUnit._matrix_TF._m02 * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m01 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m11) + (_parentRenderUnit._matrix_TF._m02 * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m02 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m12) + (_parentRenderUnit._matrix_TF._m02 * _matrix_Rigging._m22);

			_matrix_Rigging_MeshTF_VertWorld._m10 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m10) + (_parentRenderUnit._matrix_TF._m12 * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m11 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m11) + (_parentRenderUnit._matrix_TF._m12 * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m12 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m12) + (_parentRenderUnit._matrix_TF._m12 * _matrix_Rigging._m22);

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}


		// Rigging + Local
		public void Calculate_Rigging_VertLocal(	ref float riggingWeight, ref apMatrix3x3 matrix_rigging, //Step 0
													ref Vector2 deltaLocal//Step2 Vert Local
												)
		{
			//1. Rigging
			_matrix_Rigging._m00 = (matrix_rigging._m00 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m01 = matrix_rigging._m01 * riggingWeight;
			_matrix_Rigging._m02 = matrix_rigging._m02 * riggingWeight;
			_matrix_Rigging._m10 = matrix_rigging._m10 * riggingWeight;
			_matrix_Rigging._m11 = (matrix_rigging._m11 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m12 = matrix_rigging._m12 * riggingWeight;
			
			//2. Delta Local/World
			_deltaPos_VertLocal = deltaLocal;
			_deltaPos_VertWorld.x = 0.0f;
			_deltaPos_VertWorld.y = 0.0f;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02 + _deltaPos_VertLocal.x;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12 + _deltaPos_VertLocal.y;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			_matrix_Rigging_MeshTF_VertWorld._m00 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m10) + (_parentRenderUnit._matrix_TF._m02 * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m01 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m11) + (_parentRenderUnit._matrix_TF._m02 * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m02 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m12) + (_parentRenderUnit._matrix_TF._m02 * _matrix_Rigging._m22);

			_matrix_Rigging_MeshTF_VertWorld._m10 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m10) + (_parentRenderUnit._matrix_TF._m12 * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m11 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m11) + (_parentRenderUnit._matrix_TF._m12 * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m12 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m12) + (_parentRenderUnit._matrix_TF._m12 * _matrix_Rigging._m22);

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}



		// Rigging + Vert World
		public void Calculate_Rigging_VertWorld(	ref float riggingWeight, ref apMatrix3x3 matrix_rigging, //Step 0
													ref Vector2 deltaWorld //Step4 Vert World
												)
		{
			//1. Rigging
			_matrix_Rigging._m00 = (matrix_rigging._m00 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m01 = matrix_rigging._m01 * riggingWeight;
			_matrix_Rigging._m02 = matrix_rigging._m02 * riggingWeight;
			_matrix_Rigging._m10 = matrix_rigging._m10 * riggingWeight;
			_matrix_Rigging._m11 = (matrix_rigging._m11 * riggingWeight) + (1.0f - riggingWeight);
			_matrix_Rigging._m12 = matrix_rigging._m12 * riggingWeight;
			
			//2. Delta Local/World
			_deltaPos_VertLocal.x = 0.0f;
			_deltaPos_VertLocal.y = 0.0f;
			_deltaPos_VertWorld = deltaWorld;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			float matrixTFVertWorld_X = _parentRenderUnit._matrix_TF._m02 + _deltaPos_VertWorld.x;
			float matrixTFVertWorld_Y = _parentRenderUnit._matrix_TF._m12 + _deltaPos_VertWorld.y;
			
			_matrix_Rigging_MeshTF_VertWorld._m00 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m10) + (matrixTFVertWorld_X * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m01 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m11) + (matrixTFVertWorld_X * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m02 = (_parentRenderUnit._matrix_TF._m00 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m01 * _matrix_Rigging._m12) + (matrixTFVertWorld_X * _matrix_Rigging._m22);

			_matrix_Rigging_MeshTF_VertWorld._m10 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m00) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m10) + (matrixTFVertWorld_Y * _matrix_Rigging._m20);
			_matrix_Rigging_MeshTF_VertWorld._m11 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m01) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m11) + (matrixTFVertWorld_Y * _matrix_Rigging._m21);
			_matrix_Rigging_MeshTF_VertWorld._m12 = (_parentRenderUnit._matrix_TF._m10 * _matrix_Rigging._m02) + (_parentRenderUnit._matrix_TF._m11 * _matrix_Rigging._m12) + (matrixTFVertWorld_Y * _matrix_Rigging._m22);

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}




		// Local
		public void Calculate_VertLocal(ref Vector2 deltaLocal)
		{
			//1. Rigging - Identity
			_matrix_Rigging._m00 = 1.0f;	_matrix_Rigging._m01 = 0.0f;	_matrix_Rigging._m02 = 0.0f;
			_matrix_Rigging._m10 = 0.0f;	_matrix_Rigging._m11 = 1.0f;	_matrix_Rigging._m12 = 0.0f;
			
			//2. Delta Local/World
			_deltaPos_VertLocal = deltaLocal;
			_deltaPos_VertWorld.x = 0.0f;
			_deltaPos_VertWorld.y = 0.0f;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02 + _deltaPos_VertLocal.x;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12 + _deltaPos_VertLocal.y;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			_matrix_Rigging_MeshTF_VertWorld._m00 = _parentRenderUnit._matrix_TF._m00;
			_matrix_Rigging_MeshTF_VertWorld._m01 = _parentRenderUnit._matrix_TF._m01;
			_matrix_Rigging_MeshTF_VertWorld._m02 = _parentRenderUnit._matrix_TF._m02;

			_matrix_Rigging_MeshTF_VertWorld._m10 = _parentRenderUnit._matrix_TF._m10;
			_matrix_Rigging_MeshTF_VertWorld._m11 = _parentRenderUnit._matrix_TF._m11;
			_matrix_Rigging_MeshTF_VertWorld._m12 = _parentRenderUnit._matrix_TF._m12;

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}
		

		// Local + World
		public void Calculate_VertLocal_VertWorld(	ref Vector2 deltaLocal, //Step2 Vert Local
													ref Vector2 deltaWorld //Step4 Vert World
												)
		{
			//1. Rigging - Identity
			_matrix_Rigging._m00 = 1.0f;	_matrix_Rigging._m01 = 0.0f;	_matrix_Rigging._m02 = 0.0f;
			_matrix_Rigging._m10 = 0.0f;	_matrix_Rigging._m11 = 1.0f;	_matrix_Rigging._m12 = 0.0f;
			
			//2. Delta Local/World
			_deltaPos_VertLocal = deltaLocal;
			_deltaPos_VertWorld = deltaWorld;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02 + _deltaPos_VertLocal.x;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12 + _deltaPos_VertLocal.y;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			float matrixTFVertWorld_X = _parentRenderUnit._matrix_TF._m02 + _deltaPos_VertWorld.x;
			float matrixTFVertWorld_Y = _parentRenderUnit._matrix_TF._m12 + _deltaPos_VertWorld.y;
			
			_matrix_Rigging_MeshTF_VertWorld._m00 = _parentRenderUnit._matrix_TF._m00;
			_matrix_Rigging_MeshTF_VertWorld._m01 = _parentRenderUnit._matrix_TF._m01;
			_matrix_Rigging_MeshTF_VertWorld._m02 = matrixTFVertWorld_X;

			_matrix_Rigging_MeshTF_VertWorld._m10 = _parentRenderUnit._matrix_TF._m10;
			_matrix_Rigging_MeshTF_VertWorld._m11 = _parentRenderUnit._matrix_TF._m11;
			_matrix_Rigging_MeshTF_VertWorld._m12 = matrixTFVertWorld_Y;

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}


		// World
		public void Calculate_VertWorld(	ref Vector2 deltaWorld //Step4 Vert World
											)
		{
			//1. Rigging - Identity
			_matrix_Rigging._m00 = 1.0f;	_matrix_Rigging._m01 = 0.0f;	_matrix_Rigging._m02 = 0.0f;
			_matrix_Rigging._m10 = 0.0f;	_matrix_Rigging._m11 = 1.0f;	_matrix_Rigging._m12 = 0.0f;
			
			//2. Delta Local/World
			_deltaPos_VertLocal.x = 0.0f;
			_deltaPos_VertLocal.y = 0.0f;
			_deltaPos_VertWorld = deltaWorld;

			//3. 중간 Matrix
			//(1) _matrix_Vert2Mesh_VertLocal
			_deltaPos_Vert2Mesh_VertLocal.x = _parentMesh._matrix_VertToLocal._m02;
			_deltaPos_Vert2Mesh_VertLocal.y = _parentMesh._matrix_VertToLocal._m12;

			//(2) _matrix_Rigging_MeshTF_VertWorld
			_matrix_Rigging_MeshTF_VertWorld._m00 = _parentRenderUnit._matrix_TF._m00;
			_matrix_Rigging_MeshTF_VertWorld._m01 = _parentRenderUnit._matrix_TF._m01;
			_matrix_Rigging_MeshTF_VertWorld._m02 = _parentRenderUnit._matrix_TF._m02 + _deltaPos_VertWorld.x;

			_matrix_Rigging_MeshTF_VertWorld._m10 = _parentRenderUnit._matrix_TF._m10;
			_matrix_Rigging_MeshTF_VertWorld._m11 = _parentRenderUnit._matrix_TF._m11;
			_matrix_Rigging_MeshTF_VertWorld._m12 = _parentRenderUnit._matrix_TF._m12 + _deltaPos_VertWorld.y;

			//3. Matrix World
			//_matrix_ToWorld
			_matrix_ToWorld._m00 = _matrix_Rigging_MeshTF_VertWorld._m00;
			_matrix_ToWorld._m01 = _matrix_Rigging_MeshTF_VertWorld._m01;
			_matrix_ToWorld._m02 = _matrix_Rigging_MeshTF_VertWorld._m00 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m01 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m02;

			_matrix_ToWorld._m10 = _matrix_Rigging_MeshTF_VertWorld._m10;
			_matrix_ToWorld._m11 = _matrix_Rigging_MeshTF_VertWorld._m11;
			_matrix_ToWorld._m12 = _matrix_Rigging_MeshTF_VertWorld._m10 * _deltaPos_Vert2Mesh_VertLocal.x + _matrix_Rigging_MeshTF_VertWorld._m11 * _deltaPos_Vert2Mesh_VertLocal.y + _matrix_Rigging_MeshTF_VertWorld._m12;

			apMatrix3x3.MultiplyPoint(ref _pos_World, ref _matrix_ToWorld, ref _vertex._pos);
			apMatrix3x3.MultiplyPoint(ref _pos_LocalOnMesh, ref _parentRenderUnit._matrix_TF_Inv, ref _pos_World);
		}


		//------------------------------------------------------------------------------

		#region [미사용 코드]
		///// <summary>
		///// 추가 21.5.14 : Calculate 함수의 C++ DLL 버전.
		///// </summary>
		///// <param name="tDelta"></param>
		//public void Calculate_DLL(float tDelta)
		//{	
		//	//DLL 함수
		//	//RenderVertex_Calculate(	ref _matrix_ToWorld, 
		//	//						ref _pos_World,
		//	//						ref _pos_LocalOnMesh,
		//	//						ref _matrix_Static_Vert2Mesh,
		//	//						ref _matrix_Cal_VertLocal,
		//	//						ref _matrix_Rigging,
		//	//						ref _matrix_MeshTransform,
		//	//						ref _matrix_MeshTransform_Inv,
		//	//						ref _matrix_Cal_VertWorld,
		//	//						ref _vertex._pos);

		//	//삭제 22.5.9
		//	//_isCalculated = true;			
		//}




		//public void CalculateByComputeShader(Vector2 posWorld2, Vector2 posLocalOnMesh, apMatrix3x3 mtxWorld)
		//{
		//	_matrix_ToWorld = mtxWorld;

		//	//_pos_World3 = posWorld3;
		//	_pos_World = posWorld2;


		//	_pos_LocalOnMesh = posLocalOnMesh;

		//	_isCalculated = true;

		//	//_isUpdateMatrixForce = false;
		//	//_isMatrixChanged = false;

		//	//if(_cal_VertWorld.magnitude > 0.5f && _vertex._index == 4)
		//	//{
		//	//	Debug.Log("Editor Cal World : " + _cal_VertWorld + " >> VertWorld : " + _pos_World + " >> Mesh Transform : \r\n" + _matrix_MeshTransform.ToString());
		//	//}
		//}

		//public void CalculateByComputeShader_New(   //Vector3 posWorld3, 
		//											Vector2 posWorld2,
		//											Vector2 posLocalOnMesh,
		//											apMatrix3x3 mtxWorld,

		//											apMatrix3x3 mtxMeshWorldMatrix,
		//											Vector2 calLocalPos,
		//											Vector2 calWorldPos,
		//											float rigWeight,
		//											Vector2 rigPos)
		//{
		//	_matrix_ToWorld = mtxWorld;

		//	//_pos_World3 = posWorld3;
		//	_pos_World = posWorld2;


		//	_pos_LocalOnMesh = posLocalOnMesh;


		//	_pos_Rigging = rigPos;
		//	_weight_Rigging = rigWeight;
		//	_matrix_Cal_VertLocal = apMatrix3x3.TRS(calLocalPos, 0, Vector2.one);
		//	_matrix_MeshTransform = mtxMeshWorldMatrix;
		//	_matrix_Cal_VertWorld = apMatrix3x3.TRS(calWorldPos, 0, Vector2.one);

		//	_isCalculated = true;
		//} 
		#endregion


		/// <summary>
		/// Modifier가 적용되지 않은 World Position을 직접 계산한다.
		/// _pos_World_NoMod로 저장된다.
		/// </summary>
		/// <param name="matrix_Vert2Local"></param>
		/// <param name="matrix_meshTransformNoMod"></param>
		public void CalculateNotModified(apMatrix3x3 matrix_Vert2Local, apMatrix3x3 matrix_meshTransformNoMod)
		{
			apMatrix3x3 matrix_ToWorld = matrix_meshTransformNoMod * matrix_Vert2Local;
			Vector2 posW = matrix_ToWorld.MultiplyPoint(_vertex._pos);
			_pos_World_NoMod = posW;
			//_pos_World_NoMod.x = posW.x;
			//_pos_World_NoMod.y = posW.y;
		}


		// Get / Set
		//------------------------------------
		//삭제 22.5.9
		//public bool IsCalculated { get { return _isCalculated; } }


		//추가 19.7.30 : 그럴싸한 Bone Weight를 위해서 색상, 비율, 선택 여부를 리스트 형태로 저장한다.
		public class RigWeightParams
		{
			public List<Color> _colors = new List<Color>();
			public List<bool> _isSelecteds = new List<bool>();
			public List<float> _ratios = new List<float>();
			public int _nParam = 0;
			private float _totalWeight = 0.0f;

			public RigWeightParams()
			{
				_colors.Clear();
				_isSelecteds.Clear();
				_ratios.Clear();
				_nParam = 0;
				_totalWeight = 0.0f;
			}
			public void Clear()
			{
				_colors.Clear();
				_isSelecteds.Clear();
				_ratios.Clear();
				_nParam = 0;
				_totalWeight = 0.0f;
			}

			public void AddRigWeight(Color color, bool isSelected, float weight)
			{
				//유효한 것만 색으로 처리
				if (weight > 0.0f)
				{
					_colors.Add(color);
					_isSelecteds.Add(isSelected);
					_ratios.Add(weight);
					_nParam++;
					_totalWeight += weight;
				}
			}

			public void Normalize()
			{
				if(_nParam == 0)
				{
					return;
				}

				if(Mathf.Approximately(_totalWeight, 1.0f))
				{
					//이미 총합 1.0f으로 저장된 경우 리턴
					return;
				}

				for (int i = 0; i < _nParam; i++)
				{
					//값을 보정하자
					_ratios[i] /= _totalWeight;
				}

			}
		}
	}
}