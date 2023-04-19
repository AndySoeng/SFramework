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
	/// apModifiedVertex와 유사하지만, 이동값이 아닌 Weight 값을 가진다.
	/// Weight 계열의 Modifier의 값을 가지고있다.
	/// (의외로?!) 데이터가 많고, 실제로 계산 후의 "위치값"을 따로 저장해야하므로 ModVert와 구분한다.
	/// </summary>
	[Serializable]
	public class apModifiedVertexWeight
	{
		// Members
		//------------------------------------------------------
		//기본 연동데이터
		[NonSerialized]
		public apRenderUnit _renderUnit = null;

		[NonSerialized]
		public apModifiedMesh _modifiedMesh = null;

		public int _vertexUniqueID = -1;
		public int _vertIndex = -1;

		[NonSerialized]
		public apMesh _mesh = null;

		[NonSerialized]
		public apVertex _vertex = null;

		[NonSerialized]
		public apRenderVertex _renderVertex = null;//RenderUnit과 연동된 경우 RenderVert도 넣어주자

		//계산을 위한 Weight
		public bool _isEnabled = false;
		public float _weight = 0.0f;

		[SerializeField]
		public bool _isPhysics = false;

		[SerializeField]
		public bool _isVolume = false;


		//Physics인 경우
		[SerializeField]
		public Vector2 _pos_World_NoMod = Vector2.zero;






		//물리 처리 방식 변경

		//<이전>
		//- Velocity 자체를 Pos 기록값으로부터 역추산하여 얻어낸다.
		//- 역으로 얻어낸 Velocity를 가지고 외력의 Acc를 구한다.
		//> 단점 : 내력과 외력을 구분할 수 없고(관성과 복원력의 제대로된 계산 불가), Velocity 계산 방식에 크게 의존한다.

		//<변경>
		//- 이전 프레임의 Velocity(=Velocity_Cur)를 유지한다.
		//- 기존의 "Acc = (V_cur - V_prev) / tDelta"의 공식은 버린다.
		//- 내력에 의한 속도는 그대로 유지
		//- "예상 위치"와 "실제 위치"의 차이가 "Delta Velocity"이다. "V_ex = (Pos_real - Pos_predict) / tDelta"
		//- "Acc = (V_ex - V_cur) / tDelta"
		//- Acc_ex = V_ex / tDelta
		//- Acc는 무조건 외력에 의해서만 발생한다. (외력 : 다른 Modifier 또는 Transform의 변화)
		//- 관성 : F_inertia = -1 * K * mass * Acc_ex
		//- 복원력 (장력의 절대값형) : F_recover = -1 * K * CalPos
		//- 장력 : F_stretch = Sum(-1 * K * deltaX_LinkedVert)
		//- 중력/바람 : F = mg, wind 절대값
		//- 저항력 : F_drag = -kmv

		//변경점
		//Velocity 계산 방식은 "위치 인식 방식"이 아니라 "외부에서 계산된(V += at) 처리 값"을 그대로 사용하는 방식
		//Pos_predict = Pos_prev + V_cur * tDelta
		//Pos_real = Pos_world
		//외력에 의한 실제 속도(이게 V_cur와 분리됨) = V_ex
		//V_ex = (Pos_real - Pos_predict) / tDelta
		//Acc_ex = (V_ex - V_cur) / tDelta

		//Pos Record 방식은 아예 사용하지 않는다. (일단은)


		//물리 처리용 지연 변수
		//처리 프레임 대비 -2F (Mod 계산 기준 -3F)의 값을 가진다.
		[NonSerialized]
		public bool _isPhysicsCalculatedPrevFrame = false;//<<이전 프레임에서 물리가 계산되지 않았다면, 처음엔 Damp를 해야한다.

		[NonSerialized]
		public Vector2 _pos_Real = Vector2.zero;

		[NonSerialized]
		public Vector2 _pos_1F = Vector2.zero;

		[NonSerialized]
		public Vector2 _pos_Predict = Vector2.zero;

		[NonSerialized]
		public float _tDelta_1F = 0.0f;

		////8Frame을 저장하여 Velocity를 샘플링한다.
		////Index는 0이 최신, 7(또는 6)이 가장 이전의 값
		//private const int NUM_POS_RECORD = 10;//60FPS 기준으로 최대 18프레임을 기록해야한다. 여유있게 25개 기록하자

		////private const float MAX_VALID_RECORD_TIME = 0.1f;//최대 0.3초전 기록을 가지고 샘플링을 한다.
		//private const int NUM_SAMPLING = 3;//3개의 데이터로 샘플링을 한다.


		//[NonSerialized]
		//public Vector2[] _pos_World_Records = new Vector2[NUM_POS_RECORD];

		////Pos 사이의 변위 시간
		////Pos[0] ~ tDelta[0] ~ Pos[1]
		//[NonSerialized]
		//public float[] _tDelta_Records = new float[NUM_POS_RECORD - 1];

		////Velocity도 기록하자
		//[NonSerialized]
		//public Vector2[] _velocity_Records = new Vector2[NUM_POS_RECORD];



		[NonSerialized]
		public Vector2 _velocity_1F = Vector2.zero;

		[NonSerialized]
		public Vector2 _acc_Ex = Vector2.zero;//현재 프레임에서 "외부 힘에 의한" Velocity 변경

		[NonSerialized]
		public Vector2 _velocity_Real = Vector2.zero;//외부 힘에 의한 Velocity

		[NonSerialized]
		public Vector2 _velocity_Next = Vector2.zero;//<<이건 계산용 변수

		[NonSerialized]
		public Vector2 _velocity_Real1F = Vector2.zero;

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

		/// <summary>
		/// DeltaPos의 값을 사용하는 힘의 노이즈를 막기 위해 Prev 값을 하나 둔다.
		/// </summary>
		[NonSerialized]
		public Vector2 _calculatedDeltaPos_Prev = Vector2.zero;

		[NonSerialized]
		public bool _isLimitPos = false;

		[NonSerialized]
		public float _limitScale = -1.0f;


		
		/// <summary>
		/// 자유롭게 움직일 수 있는 영역 (반지름)
		/// </summary>
		[SerializeField]
		public float _deltaPosRadius_Free = 0.0f;

		/// <summary>
		/// 움직일 수 있는 최대 영역 (반지름)
		/// </summary>
		[SerializeField]
		public float _deltaPosRadius_Max = 0.0f;


		[SerializeField]
		public apPhysicsVertParam _physicParam = new apPhysicsVertParam();


		////디버깅 용으로 이 값들을 가지고 있자
		//[NonSerialized]
		//public Vector2 _dbgF_gravity = Vector2.zero;

		//[NonSerialized]
		//public Vector2 _dbgF_wind = Vector2.zero;

		//[NonSerialized]
		//public Vector2 _dbgF_stretch = Vector2.zero;

		//[NonSerialized]
		//public Vector2 _dbgF_airDrag = Vector2.zero;

		//[NonSerialized]
		//public Vector2 _dbgF_recover = Vector2.zero;

		//[NonSerialized]
		//public Vector2 _dbgF_sum = Vector2.zero;


		// Init
		//------------------------------------------------------
		public apModifiedVertexWeight()
		{

		}

		public void Init(int vertUniqueID, apVertex vertex)
		{
			_vertexUniqueID = vertUniqueID;
			_vertex = vertex;
			_vertIndex = _vertex._index;

			_isEnabled = false;
			_weight = 0.0f;

			_calculatedDeltaPos = Vector2.zero;
			_calculatedDeltaPos_Prev = _calculatedDeltaPos;

			_isPhysics = false;
			_isVolume = false;

			if (_physicParam == null)
			{
				_physicParam = new apPhysicsVertParam();
			}

			_physicParam.Clear();
		}

		public void SetDataType(bool isPhysics, bool isVolume)
		{
			_isPhysics = isPhysics;
			_isVolume = isVolume;
		}




		public void Link(apModifiedMesh modifiedMesh, apMesh mesh, apVertex vertex)
		{
			_modifiedMesh = modifiedMesh;
			_mesh = mesh;
			_vertex = vertex;
			if (_vertex != null)
			{
				_vertIndex = _vertex._index;
			}
			else
			{
				_vertIndex = -1;
			}

			_renderVertex = null;
			if (modifiedMesh._renderUnit != null && _vertex != null)
			{
				//이전
				//_renderVertex = modifiedMesh._renderUnit._renderVerts.Find(delegate (apRenderVertex a)
				//{
				//	return a._vertex == _vertex;
				//});

				//변경 22.3.23 : Render Vertex가 배열로 바뀌면서 함수로 변경
				_renderVertex = modifiedMesh._renderUnit.FindRenderVertex(_vertex);
			}

			if (_physicParam == null)
			{
				_physicParam = new apPhysicsVertParam();
			}

			_physicParam.Link(modifiedMesh, this);

			//RefreshModMeshAndWeights(_modifiedMesh);//이전
			LinkModMeshAndWeights(_modifiedMesh);//변경 20.3.20
		}


		public void InitCalculatedValue()
		{
			_isPhysicsCalculatedPrevFrame = false;
			if (_isPhysics)
			{
				//Mod가 포함되지 않은 "초기 위치"
				if (_renderVertex != null)
				{
					_pos_World_NoMod = _renderVertex._pos_World_NoMod;
				}

				_pos_Real = Vector2.zero;
				_pos_1F = Vector2.zero;
				_pos_Predict = Vector2.zero;
				_tDelta_1F = -1.0f;


				_velocity_1F = Vector2.zero;
				_velocity_Next = Vector2.zero;
				_velocity_Real = Vector2.zero;
				_velocity_Real1F = Vector2.zero;

				//_acc_Cur = Vector2.zero;
				_acc_Ex = Vector2.zero;


				_F_inertia_Prev = Vector2.zero;
				_F_inertia_RecordMax = Vector2.zero;
				_isUsePrevInertia = false;
				_tReduceInertia = 0.0f;
			}
		}

		// Functions
		//------------------------------------------------------
		public void LinkModMeshAndWeights(apModifiedMesh modifiedMesh)
		{
			if (modifiedMesh != null)
			{
				if (_modifiedMesh != modifiedMesh
					|| _renderVertex == null
					|| _renderUnit != modifiedMesh._renderUnit)
				{
					_modifiedMesh = modifiedMesh;
					_renderUnit = modifiedMesh._renderUnit;
					if (_modifiedMesh != null && modifiedMesh._renderUnit != null && _vertex != null)
					{
						//이전
						//_renderVertex = modifiedMesh._renderUnit._renderVerts.Find(delegate (apRenderVertex a)
						//{
						//	return a._vertex == _vertex;
						//});

						//변경 22.3.23 : Render Vertex가 배열로 바뀌면서 함수로 변경
						_renderVertex = modifiedMesh._renderUnit.FindRenderVertex(_vertex);
					}

					if (_isPhysics)
					{
						_physicParam.Link(modifiedMesh, this);
					}
				}
			}

			_calculatedDeltaPos = Vector2.zero;

			//Debug.Log("Refresh Physic Param");
			if (_isPhysics)
			{
				//Mod가 포함되지 않은 "초기 위치"
				if (_renderVertex != null)
				{
					_pos_World_NoMod = _renderVertex._pos_World_NoMod;

					_pos_Real = Vector2.zero;
					_pos_1F = Vector2.zero;
					_pos_Predict = Vector2.zero;
					_tDelta_1F = -1.0f;

					//for (int i = 0; i < _pos_World_Records.Length; i++)
					//{
					//	_pos_World_Records[i] = Vector2.zero;
					//	_velocity_Records[i] = Vector2.zero;
					//}

					//for (int i = 0; i < _tDelta_Records.Length; i++)
					//{
					//	_tDelta_Records[i] = 0.0f;
					//}

					_velocity_1F = Vector2.zero;
					_velocity_Next = Vector2.zero;
					_velocity_Real = Vector2.zero;

					//_acc_Cur = Vector2.zero;
					_acc_Ex = Vector2.zero;


					_F_inertia_Prev = Vector2.zero;
					_F_inertia_RecordMax = Vector2.zero;
					_isUsePrevInertia = false;
					_tReduceInertia = 0.0f;
				}

				//이제 전체적으로 Enabled를 기준으로 Constraint를 지정해보자
				//Constraint 조건
				//- 자신은 Enabled = false
				//- 1-Level 중에 Enabled = true인게 1개 이상 있다.

				if (_isEnabled)
				{
					_physicParam._isConstraint = false;
				}
				else
				{
					//1-Level 중에서 하나라도 유효한 Vert가 연결되어 있으면
					//Constraint이다.
					bool isAnyEnabledLinkedVert = false;
					apPhysicsVertParam.LinkedVertex linkedVert = null;
					for (int i = 0; i < _physicParam._linkedVertices.Count; i++)
					{
						linkedVert = _physicParam._linkedVertices[i];
						if (linkedVert._level != 1)
						{
							continue;
						}

						if (linkedVert._modVertWeight._isEnabled)
						{
							isAnyEnabledLinkedVert = true;
							break;
						}
					}

					if (isAnyEnabledLinkedVert)
					{
						_physicParam._isConstraint = true;
					}
					else
					{
						_physicParam._isConstraint = false;//주변에 물리가 작동하는 Vert가 아예 없군염
					}
				}
			}
		}





		public void RefreshModMeshAndWeights_Check(apModifiedMesh modifiedMesh)
		{
			//Link 부분은 삭제
			//if (modifiedMesh != null)
			//{
			//	if (_modifiedMesh != modifiedMesh
			//		|| _renderVertex == null
			//		|| _renderUnit != modifiedMesh._renderUnit)
			//	{
			//		_modifiedMesh = modifiedMesh;
			//		_renderUnit = modifiedMesh._renderUnit;
			//		if (_modifiedMesh != null && modifiedMesh._renderUnit != null && _vertex != null)
			//		{
			//			_renderVertex = modifiedMesh._renderUnit._renderVerts.Find(delegate (apRenderVertex a)
			//		{
			//			return a._vertex == _vertex;
			//		});
			//		}

			//		if (_isPhysics)
			//		{
			//			_physicParam.Link(modifiedMesh, this);
			//		}
			//	}
			//}

			_calculatedDeltaPos = Vector2.zero;

			//Debug.Log("Refresh Physic Param");
			if (_isPhysics)
			{
				//Mod가 포함되지 않은 "초기 위치"
				if (_renderVertex != null)
				{
					_pos_World_NoMod = _renderVertex._pos_World_NoMod;

					_pos_Real = Vector2.zero;
					_pos_1F = Vector2.zero;
					_pos_Predict = Vector2.zero;
					_tDelta_1F = -1.0f;

					//for (int i = 0; i < _pos_World_Records.Length; i++)
					//{
					//	_pos_World_Records[i] = Vector2.zero;
					//	_velocity_Records[i] = Vector2.zero;
					//}

					//for (int i = 0; i < _tDelta_Records.Length; i++)
					//{
					//	_tDelta_Records[i] = 0.0f;
					//}

					_velocity_1F = Vector2.zero;
					_velocity_Next = Vector2.zero;
					_velocity_Real = Vector2.zero;

					//_acc_Cur = Vector2.zero;
					_acc_Ex = Vector2.zero;


					_F_inertia_Prev = Vector2.zero;
					_F_inertia_RecordMax = Vector2.zero;
					_isUsePrevInertia = false;
					_tReduceInertia = 0.0f;
				}

				//이제 전체적으로 Enabled를 기준으로 Constraint를 지정해보자
				//Constraint 조건
				//- 자신은 Enabled = false
				//- 1-Level 중에 Enabled = true인게 1개 이상 있다.

				if (_isEnabled)
				{
					_physicParam._isConstraint = false;
				}
				else
				{
					//1-Level 중에서 하나라도 유효한 Vert가 연결되어 있으면
					//Constraint이다.
					bool isAnyEnabledLinkedVert = false;
					apPhysicsVertParam.LinkedVertex linkedVert = null;
					for (int i = 0; i < _physicParam._linkedVertices.Count; i++)
					{
						linkedVert = _physicParam._linkedVertices[i];
						if (linkedVert._level != 1)
						{
							continue;
						}

						if (linkedVert._modVertWeight._isEnabled)
						{
							isAnyEnabledLinkedVert = true;
							break;
						}
					}

					if (isAnyEnabledLinkedVert)
					{
						_physicParam._isConstraint = true;
					}
					else
					{
						_physicParam._isConstraint = false;//주변에 물리가 작동하는 Vert가 아예 없군염
					}

				}


			}
		}









		public void RefreshLinkedVertex()
		{
			if (_isPhysics)
			{
				_physicParam.RefreshLinkedVertex();

				//영역을 체크하자
				//일단 최소 거리와 평균 거리를 구한다.
				float minDist = 0.0f;
				//float avgDist = 0.0f;
				int nDist = 0;

				for (int i = 0; i < _physicParam._linkedVertices.Count; i++)
				{
					apPhysicsVertParam.LinkedVertex linkVert = _physicParam._linkedVertices[i];
					float distW = (linkVert._modVertWeight._pos_World_NoMod - _pos_World_NoMod).magnitude;
					if (nDist == 0 || distW < minDist)
					{
						minDist = distW;
					}

					//avgDist += distW;
					nDist++;
				}
				//if(nDist > 0)
				//{
				//	avgDist /= nDist;
				//}

				_deltaPosRadius_Free = minDist * 0.5f;
				//_deltaPosRadius_Max = avgDist * 0.5f;
				_deltaPosRadius_Max = minDist;
				if (_deltaPosRadius_Max < _deltaPosRadius_Free)
				{
					_deltaPosRadius_Free = _deltaPosRadius_Max * 0.5f;
				}
			}

		}

		/// <summary>
		/// RenderVertex
		/// </summary>
		/// <param name="tDelta"></param>
		public void UpdatePhysicVertex(float tDelta, bool isValidFrame)
		{
			_velocity_Next = Vector2.zero;

			if (!_isPhysics || _renderVertex == null)
			{
				return;
			}

			//물리를 체크해야하는 유효한 프레임 : 위치를 기록하여 속도를 역추산한다. 이후 외부에서 계산한다.
			//물리 체크를 생략하는 중간 프레임 : 이전에 저장된 속도를 그대로 사용한다. (계산은 하지 않는다)
			if (isValidFrame)
			{
				//이전 프레임의 값을 저장하여 딜레이를 시키자
				if (tDelta > 0.0f)
				{
					//새로운 방식
					//Velocity_Cur에 의해 예상된 위치 (Predict)와 실제 위치(Real)
					_pos_1F = _pos_Real;
					_velocity_Real1F = _velocity_Real;
					_pos_Real = _renderVertex._pos_World;
					if (_tDelta_1F > 0.0f)
					{
						//이전 기록이 있다.
						_pos_Predict = _pos_1F + _velocity_1F * ((tDelta + _tDelta_1F) * 0.5f);

						//외력을 체크하자

						_velocity_Real = (_pos_Real - _pos_1F) / tDelta;
						//_velocity_Real = (_velocity_Real * 0.5f + _velocity_1F * 0.5f);
						_acc_Ex = (_velocity_Real - _velocity_1F) / tDelta;

					}
					else
					{
						//이전 기록이 없다.
						//그냥 Velocity는 0
						_pos_Predict = _pos_Real;
						_velocity_Real = (_pos_Real - _pos_1F) / tDelta;
						//_velocity_Real = (_velocity_Real * 0.5f + _velocity_1F * 0.5f);
						//_velocity_1F = Vector2.zero;
						//_acc_Ex = Vector2.zero;
					}

					_tDelta_1F = tDelta;
					//_pos_1F = _pos_Real;


					#region [미사용 코드] 이전 방식 계산
					//_velocity_Prev = _velocity_Cur;

					//_velocity_Cur = Vector2.zero;
					////float curWeight = 1.0f;
					////float totalWeight = 0.0f;
					//float totalTime = 0.0f;
					//float curTime = 0.0f;
					//int iLastRecord = 0;
					//for (int i = 0; i < NUM_POS_RECORD - 1; i++)
					//{
					//	curTime = _tDelta_Records[i];
					//	if (curTime > 0.0f)
					//	{	
					//		//curWeight = Mathf.Pow(Mathf.Clamp01((MAX_VALID_RECORD_TIME - curTime) / MAX_VALID_RECORD_TIME), 3);

					//		//_velocity_Cur += ((_pos_World_Records[i] - _pos_World_Records[i + 1]) / _tDelta_Records[i]) * curWeight;
					//		//totalWeight += curWeight;

					//		//0           ~    iLastRecord
					//		//Time : (iLastRecord - 1)
					//		if (totalTime + curTime > MAX_VALID_RECORD_TIME)
					//		{	
					//			break;
					//		}
					//		totalTime += curTime;
					//		iLastRecord = i;
					//	}
					//}

					//bool isDebug = _vertIndex == 0;
					//if (iLastRecord == 0)
					//{

					//	_velocity_Cur = Vector2.zero;

					//	//if (isDebug)
					//	//{	
					//	//	Debug.Log("No Record (tDelta : " + tDelta + ") [" +
					//	//		_tDelta_Records[0] + ", " + _tDelta_Records[1] + ", " + _tDelta_Records[2] + "]");
					//	//}
					//}
					//else
					//{

					//	_velocity_Cur = (_pos_World_Records[0] - _pos_World_Records[iLastRecord]) / totalTime;
					//	//if (isDebug)
					//	//{
					//	//	Debug.Log("Velocity Record [" + iLastRecord + ", Time : " + totalTime + "] => " + _velocity_Cur);
					//	//}
					//}

					//_velocity_Cur = (_pos_World - _pos_World_2F) / (_tDelta_1Fto0F + _tDelta_2Fto1F);
					//if (totalWeight > 0.0f)
					//{
					//	_velocity_Cur /= totalWeight;
					//} 
					#endregion

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
			//for (int i = 0; i < _pos_World_Records.Length; i++)
			//{
			//	_pos_World_Records[i] = _pos_World_Records[0];
			//	_velocity_Records[i] = _velocity_Records[0];
			//}

			//for (int i = 0; i < _tDelta_Records.Length; i++)
			//{
			//	_tDelta_Records[i] = 0.0f;
			//}

			_velocity_1F = Vector2.zero;
			_velocity_Next = Vector2.zero;
			_acc_Ex = Vector2.zero;
			_pos_Predict = _pos_Real;
			_pos_1F = _pos_Real;
			_tDelta_1F = -1.0f;
			//_velocity_Prev = Vector2.zero;

			//_acc_Cur = Vector2.zero;

			_F_inertia_Prev = Vector2.zero;
			_F_inertia_RecordMax = Vector2.zero;
			_tReduceInertia = 0.0f;
			_isUsePrevInertia = false;
		}

		// Get / Set
		//------------------------------------------------------
	}

}