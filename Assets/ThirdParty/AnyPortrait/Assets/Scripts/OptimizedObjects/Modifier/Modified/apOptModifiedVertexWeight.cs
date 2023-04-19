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
	/// apModifiedVertexWeight의 Opt 버전
	/// </summary>
	[Serializable]
	public class apOptModifiedVertexWeight
	{
		// Members
		//------------------------------------------------------
		//기본 연동데이터
		//[NonSerialized]
		//public apOptMesh _mesh = null; //>> 19.5.23 : 불필요해서 삭제

		[NonSerialized]
		public apOptTransform _optTransform = null;

		//[NonSerialized]
		//public apOptModifiedMesh _modifiedMesh = null; //>> 19.5.23 : 불필요해서 삭제

		// 19.5.23 : 불필요해서 삭제
		//public int _vertexUniqueID = -1;
		//public int _vertIndex = -1;


		[NonSerialized]
		public apOptRenderVertex _vertex = null;

		//계산을 위한 Weight
		public bool _isEnabled = false;
		public float _weight = 0.0f;

		// 19.5.23 : 삭제
		//[SerializeField]
		//public bool _isPhysics = false;

		//[SerializeField]
		//public bool _isVolume = false;


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


		//[NonSerialized]
		//public Vector2 _velocity_Prev = Vector2.zero;

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

		// 삭제 19.5.23 : 아래의 변수들은 삭제되지 않는다.
		///// <summary>
		///// 자유롭게 움직일 수 있는 영역 (반지름)
		///// </summary>
		//[SerializeField]
		//public float _deltaPosRadius_Free = 0.0f;

		///// <summary>
		///// 움직일 수 있는 최대 영역 (반지름)
		///// </summary>
		//[SerializeField]
		//public float _deltaPosRadius_Max = 0.0f;


		[SerializeField]
		public apOptPhysicsVertParam _physicParam = new apOptPhysicsVertParam();

		

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
		public apOptModifiedVertexWeight()
		{

		}

		public void Bake(apModifiedVertexWeight srcModVertWeight)
		{

			//>>19.5.23 : 삭제 (불필요)
			//_vertexUniqueID = srcModVertWeight._vertexUniqueID;
			//_vertIndex = srcModVertWeight._vertIndex;

			_isEnabled = srcModVertWeight._isEnabled;
			_weight = srcModVertWeight._weight;

			//>>19.5.23 : 삭제 (불필요)
			//_isPhysics = srcModVertWeight._isPhysics;
			//_isVolume = srcModVertWeight._isVolume;

			_pos_World_NoMod = srcModVertWeight._pos_World_NoMod;

			//>>19.5.23 : 삭제 (불필요)
			//_deltaPosRadius_Free = srcModVertWeight._deltaPosRadius_Free;
			//_deltaPosRadius_Max = srcModVertWeight._deltaPosRadius_Max;


			if (_physicParam == null)
			{
				_physicParam = new apOptPhysicsVertParam();
			}

			_physicParam.Bake(srcModVertWeight._physicParam);
		}

		public void Link(apOptModifiedMesh modifiedMesh, apOptTransform optTransform, apOptMesh mesh, apOptRenderVertex vertex)
		{
			//>>19.5.23 : 삭제 (불필요)
			//_modifiedMesh = modifiedMesh;
			//_mesh = mesh;
			_vertex = vertex;
			_optTransform = optTransform;

			if (_physicParam != null)
			{
				_physicParam.Link(modifiedMesh, this);
			}

			DampPhysicVertex();

		}

		

		// Functions
		//------------------------------------------------------


		/// <summary>
		/// RenderVertex
		/// </summary>
		/// <param name="tDelta"></param>
		public void UpdatePhysicVertex(float tDelta, bool isValidFrame)
		{
			_velocity_Next = Vector2.zero;

			if (
				//!_isPhysics || //>>19.5.23 : 삭제 (불필요)
				_vertex == null)
			{
				return;
			}

			//물리를 체크해야하는 유효한 프레임 : 위치를 기록하여 속도를 역추산한다. 이후 외부에서 계산한다.
			//물리 체크를 생략하는 중간 프레임 : 이전에 저장된 속도를 그대로 사용한다. (계산은 하지 않는다)

			bool isWorld = true;

			if (isValidFrame)
			{

				//이전 프레임의 값을 저장하여 딜레이를 시키자
				if (tDelta > 0.0f)
				{
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
					if (isWorld)
					{
						//이전
						//_pos_Real = _optTransform._rootUnit._transform.TransformPoint(_vertex._vertPos_World); //<<World 방식

						//변경
						_pos_Real = _optTransform._rootUnit._transform.TransformPoint(vertPosWorld); //<<World 방식

						
					}
					else
					{
						//이전
						//_pos_Real = _vertex._vertPos_World; //<<Local 방식

						//변경
						_pos_Real = vertPosWorld; //<<Local 방식
					}
					//이전
					//_pos_World_LocalTransform = _vertex._vertPos_World;

					//변경
					_pos_World_LocalTransform = vertPosWorld;

					if (_tDelta_1F > 0.0f)
					{
						//이전 기록이 있다.
						Vector2 velWorld_1F = Vector2.zero;
						if (isWorld)
						{
							velWorld_1F = _optTransform._rootUnit._transform.TransformVector(_velocity_1F);//<<World 방식
						}
						else
						{
							velWorld_1F = _velocity_1F;//<<Local 방식
						}

						_pos_Predict = _pos_1F + velWorld_1F * ((tDelta + _tDelta_1F) * 0.5f);

						//외력을 체크하자

						//_velocity_Real = (_pos_Real - _pos_1F) / tDelta;

						if (isWorld)
						{
							_velocity_Real = _optTransform._rootUnit._transform.InverseTransformVector(_pos_Real - _pos_1F) / tDelta;//<<World 방식
						}
						else
						{
							_velocity_Real = (_pos_Real - _pos_1F) / tDelta;//<<Local 방식
						}
						_velocity_Real *= -1;//<<이거 확인할 것. 이거 왜 반대로 했어요?

						
						
						//_velocity_Real = (_velocity_Real * 0.95f + _velocity_1F * 0.05f);//에러 보정 < 미사용

						//if(_vertIndex == 0 && _velocity_Real.sqrMagnitude > 0)
						//{
						//	Debug.Log("Velocity Real : " + _velocity_Real);
						//}
						

						_acc_Ex = (_velocity_Real - _velocity_1F) / tDelta;

					}
					else
					{
						//이전 기록이 없다.
						//그냥 Velocity는 0
						_pos_Predict = _pos_Real;
						//_velocity_Real = (_pos_Real - _pos_1F) / tDelta;

						if (isWorld)
						{
							_velocity_Real = _optTransform._rootUnit._transform.InverseTransformVector(_pos_Real - _pos_1F) / tDelta;//World 방식
						}
						else
						{
							_velocity_Real = (_pos_Real - _pos_1F) / tDelta;//Local 방식	
						}
						//_velocity_Real = (_velocity_Real * 0.5f + _velocity_1F * 0.5f);

						//_velocity_1F = Vector2.zero;
						//_acc_Ex = Vector2.zero;
					}

					_tDelta_1F = tDelta;


					////테스트
					//if(_optTransform._rootUnit.IsFlippedX)
					//{
					//	_velocity_Real.x *= -1;
					//}
					//if(_optTransform._rootUnit.IsFlippedY)
					//{
					//	_velocity_Real.y *= -1;
					//}


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
		}

		public void DampPhysicVertex()
		{
			//Debug.Log("Damp");
			_calculatedDeltaPos = Vector2.zero;


			//for (int i = 0; i < NUM_POS_RECORD; i++)
			//{
			//	_pos_World_Records[i] = _pos_Real;
			//	//_pos_RootWorld_Records[i] = _pos_RootWorld;//<<
			//}

			//for (int i = 0; i < NUM_POS_RECORD - 1; i++)
			//{
			//	_tDelta_Records[i] = 0.0f;
			//}

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

			//<<
			//_velocityRoot_Prev = Vector3.zero;
			//_velocityRoot_Cur = Vector3.zero;
			//_velocityRoot2_Prev = Vector2.zero;
			//_velocityRoot2_Cur = Vector2.zero;

			_calculatedDeltaPos_Prev = _calculatedDeltaPos;

		}

		public void ClearTouchedWeight()
		{
			for (int i = 0; i < _touchedWeight.Length; i++)
			{
				_touchedWeight[i] = -1.0f;
			}
		}

		// Get / Set
		//------------------------------------------------------
	}
}