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
	/// apOptModifiedVertexWeight의 물리 전용으로 개선된 버전
	/// Serialize의 제한을 해결해야한다.
	/// </summary>
	[Serializable]
	public class apOptModifiedPhysicsVertex
	{
		// Members
		//------------------------------------------------------
		//기본 연동데이터
		[NonSerialized]
		public apOptTransform _optTransform = null;

		[NonSerialized]
		public apOptRenderVertex _vertex = null;

		//계산을 위한 Weight
		public bool _isEnabled = false;
		public float _weight = 0.0f;

		//Physics인 경우
		[SerializeField]
		public Vector2 _pos_World_NoMod = Vector2.zero;

		//물리 처리용 지연 변수
		//처리 프레임 대비 -2F (Mod 계산 기준 -3F)의 값을 가진다.
		[NonSerialized]
		public Vector2 _pos_Real = Vector2.zero;

		[NonSerialized]
		public Vector2 _pos_World_LocalTransform = Vector2.zero;

		[NonSerialized]
		public Vector2 _pos_1F = Vector2.zero;

		[NonSerialized]
		public Vector2 _pos_Predict = Vector2.zero;

		[NonSerialized]
		public float _tDelta_1F = 0.0f;


		[NonSerialized]
		public Vector2 _velocity_1F = Vector2.zero;

		[NonSerialized]
		public Vector2 _velocity_Real = Vector2.zero;

		[NonSerialized]
		public Vector2 _velocity_Real1F = Vector2.zero;

		[NonSerialized]
		public Vector2 _velocity_Next = Vector2.zero;

		[NonSerialized]
		public Vector2 _acc_Ex = Vector2.zero;

		

		[NonSerialized]
		public Vector2 _F_inertia_Prev = Vector2.zero;

		[NonSerialized]
		public Vector2 _F_inertia_RecordMax = Vector2.zero;

		[NonSerialized]
		public bool _isUsePrevInertia = false;

		[NonSerialized]
		public float _tReduceInertia = 0.0f;




		/// <summary>
		/// 계산된 DeltaPos
		/// CalculateResultParam에서 계산시 이 값을 deltaPos 처럼 사용한다.
		/// </summary>
		[NonSerialized]
		public Vector2 _calculatedDeltaPos = Vector2.zero;

		[NonSerialized]
		public Vector2 _calculatedDeltaPos_Prev = Vector2.zero;



		[NonSerialized]
		public bool _isLimitPos = false;

		[NonSerialized]
		public float _limitScale = -1.0f;

		//----------------------------------------------------------------------------------
		//이전
		//[SerializeField]
		//public apOptPhysicsVertParam _physicParam = new apOptPhysicsVertParam();

		//변경
		//PhysicVertParam을 별도의 클래스가 아니라 해체해서 직접 데이터를 멤버로 가진다.
		public bool _isConstraint = false;
		public bool _isMain = false;


		[Serializable]
		public class LinkedVertex
		{
			[SerializeField]
			public int _vertIndex = -1;

			[NonSerialized]
			public apOptRenderVertex _vertex = null;

			[NonSerialized]
			public apOptModifiedPhysicsVertex _modVertWeight = null;

			[SerializeField]
			public float _distWeight = 0.0f;

			[NonSerialized]
			public Vector2 _deltaPosToTarget_NoMod = Vector2.zero;

			public LinkedVertex(apPhysicsVertParam.LinkedVertex srcLinkedVertex)
			{
				_vertex = null;
				_vertIndex = srcLinkedVertex._vertex._index;
				_modVertWeight = null;
				_distWeight = srcLinkedVertex._distWeight;
			}


			public void Link(apOptRenderVertex linkedVertex, apOptModifiedPhysicsVertex linkedModPhysicsVertex)
			{
				_vertex = linkedVertex;
				_modVertWeight = linkedModPhysicsVertex;
			}
		}

		[SerializeField]
		public List<LinkedVertex> _linkedVertices = new List<LinkedVertex>();
		//----------------------------------------------------------------------------------

		//추가
		//같은 그룹 ID를 가진 Linked Vertex와 Velocity가 유사하게 바뀌는 "점성" 개념이 추가되었다.
		//그룹 ID를 추가한다.
		//플래그 개념으로 10개 까지 지원한다.
		//0 : None
		//1, 2, 4, 8, 16, 32...2^16
		[SerializeField]
		public int _viscosityGroupID = 0;
		

		

		//추가 : 당기는 힘을 추가한다.
		//[Touch ID, Weight] >> Weight 배열로 구현
		//링크된 개수는 고정
		//이벤트가 추가되었는지 여부는 ID 변동사항을 체크한다.
		[NonSerialized]
		public float[] _touchedWeight = new float[apForceManager.MAX_TOUCH_UNIT];

		[NonSerialized]
		public Vector2[] _touchedPosDelta = new Vector2[apForceManager.MAX_TOUCH_UNIT];


		// Init
		//------------------------------------------------------
		public apOptModifiedPhysicsVertex()
		{

		}

		public void Bake(apModifiedVertexWeight srcModVertWeight)
		{

			_isEnabled = srcModVertWeight._isEnabled;
			_weight = srcModVertWeight._weight;

			_pos_World_NoMod = srcModVertWeight._pos_World_NoMod;

			
			//PhysicsParam도 Bake
			apPhysicsVertParam srcVertParam = srcModVertWeight._physicParam;

			_isConstraint = srcVertParam._isConstraint;
			_isMain = srcVertParam._isMain;

			_linkedVertices.Clear();
			for (int i = 0; i < srcVertParam._linkedVertices.Count; i++)
			{
				apPhysicsVertParam.LinkedVertex srcLinkedVert = srcVertParam._linkedVertices[i];
				_linkedVertices.Add(new LinkedVertex(srcLinkedVert));//<<Add + Bake
			}

			_viscosityGroupID = srcVertParam._viscosityGroupID;
		}

		//개선된 버전 19.5.23
		public void Link(	apOptModifiedMesh_Physics modMeshPhysics,
							apOptModifiedMeshSet modMeshSet, 
							apOptTransform optTransform, 
							apOptRenderVertex vertex)
		{
			//>>19.5.23 : 삭제 (불필요)
			//_modifiedMesh = modifiedMesh;
			//_mesh = mesh;
			_vertex = vertex;
			_optTransform = optTransform;

			if (_linkedVertices == null)
			{
				_linkedVertices = new List<LinkedVertex>();
			}


			apOptMesh mesh = modMeshSet._targetMesh;

			//이미 Bake 되었으므로 바로 Link하면 된다.
			for (int i = 0; i < _linkedVertices.Count; i++)
			{
				LinkedVertex linkedVert = _linkedVertices[i];
				apOptRenderVertex renderVert = mesh.RenderVertices[linkedVert._vertIndex];
				apOptModifiedPhysicsVertex linkVert = modMeshSet.SubModMesh_Physics._vertWeights[linkedVert._vertIndex];
				linkedVert.Link(renderVert, linkVert);
			}

			DampPhysicVertex();

		}



		

		// Functions
		//------------------------------------------------------


		/// <summary>
		/// RenderVertex
		/// </summary>
		/// <param name="tDelta"></param>
		public void UpdatePhysicVertex(float tDelta, bool isValidFrame, bool isTeleportFrame)
		{
			_velocity_Next = Vector2.zero;

			if (_vertex == null)
			{
				return;
			}

			//물리를 체크해야하는 유효한 프레임 : 위치를 기록하여 속도를 역추산한다. 이후 외부에서 계산한다.
			//물리 체크를 생략하는 중간 프레임 : 이전에 저장된 속도를 그대로 사용한다. (계산은 하지 않는다)

			//bool isWorld = true;//이게 무조건 true인데 왜 변수를... (변수 삭제 및 조건문 삭제 (20.9.15)
			//코멘트.
			//아래의 코드 중, RootUnit의 Transform을 이용해서 실제 World 좌표를 참조하는 코드는 빌보드에 의해서 Root Unit이 회전할 때 오작동을 한다.
			//이전 프레임의 World 좌표를 RootUnit 위치로부터의 Offset (정사영 2D)로 저장하는 것으로 변경해야하는데, 그 작업을 할 경우 성능이 많이 떨어질 것이므로,
			//이 코드에서는 적용하지 않는다.
			//이와 관련된 코드는 apOptBone의 지글본 코드를 확인하자.

			if(isTeleportFrame)
			{
				//텔레포트 프레임이라면
				//World 좌표는 계산하지 않고, 로컬 좌표를 유지한다.
				//World 좌표는 거꾸로 연산해서 저장한다.
				//이전 프레임의 값을 저장하여 딜레이를 시키자
				//예측 위치는 모두 무효
				
				//Render Vertex의 위치는 그대로 가져온다.
				Vector2 vertPosWorld = _vertex._vertPos_World;

				//플립 테스트
				if ((_optTransform._rootUnit.IsFlippedX && !_optTransform._rootUnit.IsFlippedY)
					|| (!_optTransform._rootUnit.IsFlippedX && _optTransform._rootUnit.IsFlippedY))
				{
					if (_optTransform._rootUnit.IsFlippedX)
					{
						vertPosWorld.x *= -1;
					}
					if (_optTransform._rootUnit.IsFlippedY)
					{
						vertPosWorld.y *= -1;
					}				
				}

				_pos_Real = _optTransform._rootUnit._transform.TransformPoint(vertPosWorld); //<<World 방식

				//변경
				_pos_World_LocalTransform = vertPosWorld;

				//1Frame 위치 값들은 현재 위치로 모두 갱신한다. (차이를 두면 안된다.)
				//속도는 0
				_velocity_Real = Vector2.zero;
				_velocity_Real1F = Vector2.zero;
				_velocity_1F = Vector2.zero;
				_pos_1F = _pos_Real;
				_pos_Predict = _pos_Real;
				_acc_Ex = Vector2.zero;

				_tDelta_1F = tDelta;

				//관성도 없애자 [v1.4.2]
				_F_inertia_Prev = Vector2.zero;
				_F_inertia_RecordMax = Vector2.zero;
				_tReduceInertia = 0.0f;
				_isUsePrevInertia = false;

				_calculatedDeltaPos = Vector2.zero;
				_calculatedDeltaPos_Prev = _calculatedDeltaPos;



				if (_isUsePrevInertia)
				{
					_tReduceInertia += tDelta;
					if (_tReduceInertia < 1.0f)
					{
						_F_inertia_Prev = _F_inertia_RecordMax * (1.0f - _tReduceInertia);
					}
					else
					{
						_tReduceInertia = 0.0f;
						_F_inertia_Prev = Vector2.zero;
						_F_inertia_RecordMax = Vector2.zero;
						_isUsePrevInertia = false;
					}
				}
			}
			else if (isValidFrame && tDelta > 0.0f)
			{
				//이전 프레임의 값을 저장하여 딜레이를 시키자
				Vector2 vertPosWorld = _vertex._vertPos_World;

				//플립 테스트
				if ((_optTransform._rootUnit.IsFlippedX && !_optTransform._rootUnit.IsFlippedY)
					|| (!_optTransform._rootUnit.IsFlippedX && _optTransform._rootUnit.IsFlippedY))
				{
					if (_optTransform._rootUnit.IsFlippedX)
					{
						vertPosWorld.x *= -1;
					}
					if (_optTransform._rootUnit.IsFlippedY)
					{
						vertPosWorld.y *= -1;
					}
				}

				//새로운 방식
				//Velocity_Cur에 의해 예상된 위치 (Predict)와 실제 위치(Real)
				_pos_1F = _pos_Real;
				_velocity_Real1F = _velocity_Real;

					
				_pos_Real = _optTransform._rootUnit._transform.TransformPoint(vertPosWorld); //<<World 방식

				//변경
				_pos_World_LocalTransform = vertPosWorld;

				if (_tDelta_1F > 0.0f)
				{
					//이전 기록이 있다.
					Vector2 velWorld_1F = Vector2.zero;
					velWorld_1F = _optTransform._rootUnit._transform.TransformVector(_velocity_1F);
					_pos_Predict = _pos_1F + velWorld_1F * ((tDelta + _tDelta_1F) * 0.5f);

					//외력을 체크하자
					_velocity_Real = _optTransform._rootUnit._transform.InverseTransformVector(_pos_Real - _pos_1F) / tDelta;//<<World 방식
					_velocity_Real *= -1;//<<이거 확인할 것. 이거 왜 반대로 했어요?

					_acc_Ex = (_velocity_Real - _velocity_1F) / tDelta;

				}
				else
				{
					//이전 기록이 없다.
					//그냥 Velocity는 0
					_pos_Predict = _pos_Real;
					_velocity_Real = _optTransform._rootUnit._transform.InverseTransformVector(_pos_Real - _pos_1F) / tDelta;//World 방식
				}

				_tDelta_1F = tDelta;


				if (_isUsePrevInertia)
				{
					_tReduceInertia += tDelta;
					if (_tReduceInertia < 1.0f)
					{
						_F_inertia_Prev = _F_inertia_RecordMax * (1.0f - _tReduceInertia);
					}
					else
					{
						_tReduceInertia = 0.0f;
						_F_inertia_Prev = Vector2.zero;
						_F_inertia_RecordMax = Vector2.zero;
						_isUsePrevInertia = false;
					}
				}
			}
		}

		public void DampPhysicVertex()
		{
			//Debug.Log("Damp");
			_calculatedDeltaPos = Vector2.zero;
			
			_F_inertia_Prev = Vector2.zero;
			_F_inertia_RecordMax = Vector2.zero;
			_tReduceInertia = 0.0f;
			_isUsePrevInertia = false;


			_velocity_1F = Vector2.zero;
			//_velocity_Prev = Vector2.zero;

			_velocity_Next = Vector2.zero;
			_acc_Ex = Vector2.zero;
			_pos_Predict = _pos_Real;
			_pos_1F = _pos_Real;
			_tDelta_1F = -1.0f;

			_calculatedDeltaPos_Prev = _calculatedDeltaPos;

		}

		public void ClearTouchedWeight()
		{
			for (int i = 0; i < _touchedWeight.Length; i++)
			{
				_touchedWeight[i] = -1.0f;
			}
		}
	}
}