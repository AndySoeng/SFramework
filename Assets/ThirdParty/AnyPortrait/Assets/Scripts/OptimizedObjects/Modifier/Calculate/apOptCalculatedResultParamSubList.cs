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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// ParamSetGroup에 해당하는 실행 데이터
	/// 런타임용 Optimized 버전이다.
	/// </summary>
	public class apOptCalculatedResultParamSubList
	{
		// Members
		//--------------------------------------------
		public apOptCalculatedResultParam _parentResultParam = null;
		public apOptParamSetGroup _keyParamSetGroup = null;

		public List<apOptCalculatedResultParam.OptParamKeyValueSet> _subParamKeyValues = new List<apOptCalculatedResultParam.OptParamKeyValueSet>();

		public apOptVertexRequest _vertexRequest = null;


		private int _nSubParamKeyValues = 0;

		//계산용
		public float _totalWeight = 0.0f;
		private const float _distBias = 0.01f;

		private bool _isAnim = false;
		public bool IsAnim { get { return _isAnim; } }

		public bool IsUpdatable
		{
			get
			{
				if (!_isAnim)
				{ return true; } //<<Anim타입이 아니면 항상 업데이트 된다.
				return _keyParamSetGroup.IsAnimEnabled;//<<Anim타입이라면 Clip 재생중에만 처리가 된다.
			}
		}


		// 개선된 Control Param 보간
		//Control Param 타입인 경우
		private List<apOptCalculatedLerpPoint> _cpLerpPoints = null;
		private List<apOptCalculatedLerpArea> _cpLerpAreas = null;

		//1D Lerp 처리를 위한 참조 변수
		private apOptCalculatedLerpPoint _cpLerpPoint_A = null;
		private apOptCalculatedLerpPoint _cpLerpPoint_B = null;

		//2D Lerp 처리를 위한 참조 변수
		private apOptCalculatedLerpArea _cpLerpAreaLastSelected = null;//빠른 처리를 위해 "이전에 참조된 Area"를 저장하자.

		private bool _isVertexLocalMorph = false;
		private bool _isVertexRigging = false;

		//추가 20.11.24 : 애니메이션의 빠른 키프레임 조회를 위해 만든 LUT 멤버와 거기서 연산된 두개(혹은 한개의 PKV)
		private apOptCalculatedAnimKeyLUT _animKeyLUT = null;
		
		public apOptCalculatedResultParam.OptParamKeyValueSet[] _resultAnimKeyPKVs = null;//결과 PKV (애니메이션)
		public int[] _resultAnimKeyPKVIndices = null;//결과 PKV의 인덱스(애니메이션)
		public int _nResultAnimKey = 0;

		//계산을 위한 임시 변수
		private apOptCalculatedResultParam.OptParamKeyValueSet _cal_resultAnimKeyPKV_A = null;
		private apOptCalculatedResultParam.OptParamKeyValueSet _cal_resultAnimKeyPKV_B = null;
		private apOptCalculatedAnimKeyLUT.LUTUnit _cal_targetLUT = null;
		private apAnimKeyframe _cal_keyframe_A = null;
		private apAnimKeyframe _cal_keyframe_B = null;

		// Init
		//--------------------------------------------
		public apOptCalculatedResultParamSubList(apOptCalculatedResultParam parentResultParam, bool isLocalMorph, bool isRigging)
		{
			_parentResultParam = parentResultParam;
			_subParamKeyValues.Clear();
			_nSubParamKeyValues = 0;

			_isVertexLocalMorph = isLocalMorph;
			_isVertexRigging = isRigging;
			if(_isVertexLocalMorph)
			{
				_vertexRequest = new apOptVertexRequest(apOptVertexRequest.REQUEST_TYPE.VertLocal);
			}
			else if(_isVertexRigging)
			{
				_vertexRequest = new apOptVertexRequest(apOptVertexRequest.REQUEST_TYPE.Rigging);
			}
		}

		public void SetParamSetGroup(apOptParamSetGroup paramSetGroup)
		{
			_keyParamSetGroup = paramSetGroup;

			if (_keyParamSetGroup._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
			{
				_isAnim = true;
			}
			else
			{
				_isAnim = false;
			}
		}



		// Add / Clear
		//--------------------------------------------
		public void ClearParams()
		{
			_subParamKeyValues.Clear();
			
			_nSubParamKeyValues = 0;
			if(_vertexRequest != null)
			{
				_vertexRequest.Clear();
			}
		}

		public void AddParamKeyValueSet(apOptCalculatedResultParam.OptParamKeyValueSet paramKeyValue)
		{
			if (_subParamKeyValues.Contains(paramKeyValue))
			{
				return;
			}
			_subParamKeyValues.Add(paramKeyValue);
			_nSubParamKeyValues = _subParamKeyValues.Count;


			

			if(_isVertexLocalMorph || _isVertexRigging)
			{
				if(_vertexRequest == null)
				{
					_vertexRequest = new apOptVertexRequest(_isVertexLocalMorph ? apOptVertexRequest.REQUEST_TYPE.VertLocal :  apOptVertexRequest.REQUEST_TYPE.Rigging);
				}
				//변경 19.5.24 : ModifiedMeshSet을 이용할지 여부
				if(paramKeyValue._modifiedMesh != null)
				{
					_vertexRequest.AddModMesh(paramKeyValue._modifiedMesh);
				}
				else if(paramKeyValue._modifiedMeshSet != null)
				{
					_vertexRequest.AddModMeshSet(paramKeyValue._modifiedMeshSet);
				}
				
			}
		}


		/// <summary>
		/// 입력된 Param Key Value를 보간하기 위한 보조 데이터를 만들어준다.
		/// Add ParamKeyValueSet을 모두 호출한 이후에 꼭 호출해야한다.
		/// </summary>
		public void MakeMetaData()
		{
			switch (_keyParamSetGroup._syncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					if (_keyParamSetGroup._keyControlParam != null)
					{
						//보간을 위한 Key Point와 Area를 만들자.
						if (_cpLerpPoints == null) { _cpLerpPoints = new List<apOptCalculatedLerpPoint>(); }
						if (_cpLerpAreas == null) { _cpLerpAreas = new List<apOptCalculatedLerpArea>(); }

						_cpLerpPoint_A = null;
						_cpLerpPoint_B = null;

						_cpLerpAreaLastSelected = null;

						MakeControlParamLerpAreas();
					}
					break;

				case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
					{
						if(_animKeyLUT == null)
						{
							_animKeyLUT = new apOptCalculatedAnimKeyLUT(_keyParamSetGroup._keyAnimClip, _keyParamSetGroup._keyAnimTimelineLayer);
						}

						_animKeyLUT.MakeLUT(this);//완성!

						if(_resultAnimKeyPKVs == null)
						{
							_resultAnimKeyPKVs = new apOptCalculatedResultParam.OptParamKeyValueSet[2];//배열의 최대 크기는 2
							_resultAnimKeyPKVIndices = new int[2];
						}
						_nResultAnimKey = 0;
					}
					break;
			}
		}

		// 계산
		//--------------------------------------------
		public void InitCalculate()
		{
			_totalWeight = 0.0f;
			for (int i = 0; i < _nSubParamKeyValues; i++)
			{
				_subParamKeyValues[i].ReadyToCalculate();
			}

			if(_isVertexLocalMorph || _isVertexRigging)
			{
				_vertexRequest.InitCalculate();
			}
		}


		public bool Calculate()
		{
			if (_keyParamSetGroup == null)
			{
				Debug.LogError("Null KeyParamSetGroup");
				return false;
			}

			if (!IsUpdatable)
			{
				return false;
			}

			_totalWeight = 0.0f;
			for (int i = 0; i < _nSubParamKeyValues; i++)
			{
				_subParamKeyValues[i].ReadyToCalculate();
			}

//#if UNITY_EDITOR
//			Profiler.BeginSample("Calcualte Result Param Sub List - Calculate");
//#endif
			switch (_keyParamSetGroup._syncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					//CalculateWeight_ControlParam();//구버전 - Lerp
//#if UNITY_EDITOR
//					Profiler.BeginSample("Controller");
//#endif
					CalculateWeight_ControlParam_WithMetaData();//변경 - Bi-Lerp
					
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;


				case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
//#if UNITY_EDITOR
//					Profiler.BeginSample("Keyframe");
//#endif
					CalculateWeight_KeyFrame();
					
//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif

					break;
				case apModifierParamSetGroup.SYNC_TARGET.Static:
//#if UNITY_EDITOR
//					Profiler.BeginSample("Static");
//#endif
					CalculateWeight_Static();

//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif
					break;


			}
//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			return true;
		}



		public bool Calculate_AnimMod()
		{
			//이 부분은 빠른 처리를 위해 제거
			//if (_keyParamSetGroup == null)
			//{
			//	Debug.LogError("Null KeyParamSetGroup");
			//	return false;
			//}

			//if (!IsUpdatable)
			//{
			//	return false;
			//}

			_totalWeight = 0.0f;
			//이거 안해도 된다. (초기화 안해도 결과에서 사용안함)
			//for (int i = 0; i < _nSubParamKeyValues; i++)
			//{
			//	_subParamKeyValues[i].ReadyToCalculate();
			//}

			//키프레임의 Weight 계산
			//TODO : 이 내부에 LookUpTable을 이용하여 처리를 가속화할 것
			//CalculateWeight_KeyFrame();/이전 함수
			CalculateWeight_KeyFrame_WithLUT();


			return true;
		}







		/// <summary>
		/// Animation인 경우 실행 순서와 Weight가 실시간으로 바뀐다.
		/// Weight는 상관없지만 재생 순서가 바뀐 경우 Sort를 다시 해야한다.
		/// Sort가 필요한 경우 True를 리턴한다.
		/// </summary>
		/// <returns></returns>
		public bool UpdateAnimLayer()
		{
			if (_keyParamSetGroup == null)
			{
				return false;
			}
			return _keyParamSetGroup.UpdateAnimLayer();
		}


		//---------------------------------------------------
		// 계산함수 - Control Param
		//---------------------------------------------------
		//계산에 사용할 변수를 멤버로 두자
		private apControlParam _cal_controlParam = null;
		//private float _cal_minDist = float.MaxValue;
		//private float _cal_maxDist = 0.0f;
		private float _cal_dist = 0.0f;
		private float _cal_sumDist = 0.0f;
		private float _cal_itp = 0.0f;
		private apOptCalculatedResultParam.OptParamKeyValueSet _cal_curParamKeyValue = null;
		private apOptCalculatedResultParam.OptParamKeyValueSet _cal_nextParamKeyValue = null;
		//private float _cal_deltaMinDist = 0.0f;
		//private float _cal_keepWeightRatio = 0.0f;
		//private float _cal_mulWeight = 0.0f;
		//private float _cal_revWeight = 0.0f;
		private const float _zeroBias = 0.00001f;

		private apControlParam _tmp_controlParam = null;
		apOptCalculatedResultParam.OptParamKeyValueSet _tmp_paramKeyValue = null;



		private void CalculateWeight_ControlParam()
		{
			if (_keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Null Key Control Param");
				return;
			}


			//Debug.Log("Update Control Param : " + _keyParamSetGroup._keyControlParam._keyName);

			_cal_controlParam = _keyParamSetGroup._keyControlParam;

			//_cal_minDist = float.MaxValue;
			//_cal_maxDist = 0.0f;
			_cal_dist = 0.0f;
			_cal_curParamKeyValue = null;
			_totalWeight = 0.0f;

			//Debug.Log(_cal_controlParam._keyName + " : " + _cal_controlParam._vec2_Cur);

			for (int i = 0; i < _nSubParamKeyValues; i++)
			{
				_cal_curParamKeyValue = _subParamKeyValues[i];
				_cal_dist = -10.0f;
				_cal_curParamKeyValue._isCalculated = false;

				switch (_cal_controlParam._valueType)
				{
					//case apControlParam.TYPE.Bool:
					//	if(_cal_curParamKeyValue._paramSet._conSyncValue_Bool == _cal_controlParam._bool_Cur)
					//	{
					//		_cal_curParamKeyValue._dist = 0.0f;
					//		_cal_curParamKeyValue._isCalculated = true;
					//	}
					//	else
					//	{
					//		_cal_curParamKeyValue._dist = -10.0f;
					//		_cal_curParamKeyValue._isCalculated = false;
					//	}
					//	break;

					case apControlParam.TYPE.Int:
						_cal_dist = _cal_controlParam.GetNormalizedDistance_Int(_cal_curParamKeyValue._paramSet._conSyncValue_Int);
						break;

					case apControlParam.TYPE.Float:
						_cal_dist = _cal_controlParam.GetNormalizedDistance_Float(_cal_curParamKeyValue._paramSet._conSyncValue_Float);
						break;

					case apControlParam.TYPE.Vector2:
						_cal_dist = _cal_controlParam.GetNormalizedDistance_Vector2(_cal_curParamKeyValue._paramSet._conSyncValue_Vector2);
						break;

						//case apControlParam.TYPE.Vector3:
						//	_cal_dist = _cal_controlParam.GetNormalizedDistance_Vector3(_cal_curParamKeyValue._paramSet._conSyncValue_Vector3);
						//	break;
				}
				if (_cal_dist < -1.0f)
				{
					_cal_curParamKeyValue._dist = -10.0f;
					_cal_curParamKeyValue._isCalculated = false;
					_cal_curParamKeyValue._weight = 0.0f;
					continue;
				}

				//주의 : Runtime에서는 Matched가 없다.

				_cal_curParamKeyValue._dist = _cal_dist;
				_cal_curParamKeyValue._isCalculated = true;
				_cal_curParamKeyValue._weight = 1.0f;
				_totalWeight += 1.0f;
			}

			//-----------------------------------------------
			// Weight 계산



			//선형 IDW 방식으로 계산한다.
			#region [미사용 코드] 역선형 보간 방식은 오류가 있다;
			//for (int i = 0; i < _nSubParamKeyvalues; i++)
			//{
			//	_cal_curParamKeyValue = _subParamKeyValues[i];
			//	if(!_cal_curParamKeyValue._isCalculated)
			//	{
			//		_cal_curParamKeyValue._weight = 0.0f;
			//		continue;
			//	}

			//	_cal_keepWeightRatio = Mathf.Clamp01((_cal_curParamKeyValue._dist - _cal_minDist) / (_cal_maxDist - _cal_minDist));
			//	_cal_mulWeight = (_cal_minDist * _cal_keepWeightRatio) + (1.0f - _cal_keepWeightRatio);
			//	//_cal_revWeight = (_cal_maxDist - _cal_curParamKeyValue._dist) * _cal_mulWeight;
			//	_cal_revWeight = (2.0f - _cal_curParamKeyValue._dist) * _cal_mulWeight;

			//	//_cal_revWeight = (1.0f / (_cal_curParamKeyValue._dist)) * _cal_mulWeight;

			//	_totalWeight += _cal_revWeight;
			//	_cal_curParamKeyValue._weight = _cal_revWeight;
			//} 
			#endregion

			if (_nSubParamKeyValues >= 2)
			{
				_totalWeight = 0.0f;
				for (int i = 0; i < _nSubParamKeyValues; i++)
				{
					_cal_curParamKeyValue = _subParamKeyValues[i];
					if (!_cal_curParamKeyValue._isCalculated || _cal_curParamKeyValue._weight < _zeroBias)
					{
						continue;
					}

					//다른 SubParam과의 Dist를 비교하여 내분 Weight를 하자
					if (i + 1 < _nSubParamKeyValues)
					{
						for (int j = i + 1; j < _nSubParamKeyValues; j++)
						{
							_cal_nextParamKeyValue = _subParamKeyValues[j];
							if (!_cal_nextParamKeyValue._isCalculated)
							{
								continue;
							}

							_cal_sumDist = _cal_curParamKeyValue._dist + _cal_nextParamKeyValue._dist;
							if (_cal_sumDist > _zeroBias)
							{
								_cal_itp = Mathf.Clamp01((_cal_sumDist - _cal_curParamKeyValue._dist) / _cal_sumDist);
								_cal_curParamKeyValue._weight *= _cal_itp;
								_cal_nextParamKeyValue._weight *= (1.0f - _cal_itp);
							}
						}
					}

					_totalWeight += _cal_curParamKeyValue._weight;
				}
			}



			if (_totalWeight > 0.0f)
			{
				for (int i = 0; i < _nSubParamKeyValues; i++)
				{
					_cal_curParamKeyValue = _subParamKeyValues[i];
					if (_cal_curParamKeyValue._isCalculated)
					{
						_cal_curParamKeyValue._weight /= _totalWeight;
					}
				}
			}

		}



		private void CalculateWeight_ControlParam_WithMetaData()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}

			//Value 타입에 따라 처리가 달라진다.
			switch (_keyParamSetGroup._keyControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
				case apControlParam.TYPE.Float:
					CalculateWeight_ControlParam_1D();
					break;

				case apControlParam.TYPE.Vector2:
					CalculateWeight_ControlParam_2D();

					break;
			}
		}

		private void CalculateWeight_ControlParam_1D()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}

			_tmp_controlParam = _keyParamSetGroup._keyControlParam;

			
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				_tmp_paramKeyValue = _subParamKeyValues[i];
				_tmp_paramKeyValue._weight = 0.0f;
				_tmp_paramKeyValue._isCalculated = false;//<<나중에 이것도 true로 올리자
			}
			if (_cpLerpPoints.Count == 0)
			{
				return;//처리 불가;
			}

			if (_cpLerpPoints.Count == 1)
			{
				_cpLerpPoints[0]._calculatedWeight = 1.0f;
				_cpLerpPoints[0].CalculateITPWeight();
			}
			else
			{

				//1) ITP를 계산할 두개의 Point (A, B)를 잡는다.
				//2) 두개의 포인트를 기준으로 ITP를 계산한다.
				//3) Total Weight 계산 후 적용
				bool isRefreshLerpPointRange = false;
				if (_cpLerpPoint_A == null || _cpLerpPoint_B == null)
				{
					isRefreshLerpPointRange = true;
				}
				else
				{
					if (_tmp_controlParam._valueType == apControlParam.TYPE.Int)
					{
						if (_tmp_controlParam._int_Cur < _cpLerpPoint_A._iPos ||
							_tmp_controlParam._int_Cur > _cpLerpPoint_B._iPos)
						{
							isRefreshLerpPointRange = true;
						}
					}
					else
					{
						if (_tmp_controlParam._float_Cur < _cpLerpPoint_A._pos.x ||
							_tmp_controlParam._float_Cur > _cpLerpPoint_B._pos.x)
						{
							isRefreshLerpPointRange = true;
						}
					}
				}

				if (isRefreshLerpPointRange)
				{
					//0..1..2.. [value]..3...4
					int iB = -1;
					if (_tmp_controlParam._valueType == apControlParam.TYPE.Int)
					{
						for (int i = 0; i < _cpLerpPoints.Count; i++)
						{
							if (_tmp_controlParam._int_Cur <= _cpLerpPoints[i]._iPos)
							{
								iB = i;
								break;
							}
						}
					}
					else
					{
						for (int i = 0; i < _cpLerpPoints.Count; i++)
						{
							if (_tmp_controlParam._float_Cur <= _cpLerpPoints[i]._pos.x)
							{
								iB = i;
								break;
							}
						}
					}
					if (iB < 0)
					{
						iB = _cpLerpPoints.Count - 1;
					}

					_cpLerpPoint_B = _cpLerpPoints[iB];
					if (iB == 0)
					{
						_cpLerpPoint_A = _cpLerpPoints[0];
					}
					else
					{
						_cpLerpPoint_A = _cpLerpPoints[iB - 1];
					}
				}

				if (_cpLerpPoint_A == null || _cpLerpPoint_B == null)
				{
					return;
				}

				if (_cpLerpPoint_A == _cpLerpPoint_B)
				{
					_cpLerpPoint_A._calculatedWeight = 1.0f;
					_cpLerpPoint_A.CalculateITPWeight();
				}
				else
				{
					float itp = 0.0f;
					if (_tmp_controlParam._valueType == apControlParam.TYPE.Int)
					{
						itp = 1.0f - Mathf.Clamp01((float)(_tmp_controlParam._int_Cur - _cpLerpPoint_A._iPos) / (float)(_cpLerpPoint_B._iPos - _cpLerpPoint_A._iPos));
					}
					else
					{
						itp = 1.0f - Mathf.Clamp01((float)(_tmp_controlParam._float_Cur - _cpLerpPoint_A._pos.x) / (float)(_cpLerpPoint_B._pos.x - _cpLerpPoint_A._pos.x));
					}

					_cpLerpPoint_A._calculatedWeight = itp;
					_cpLerpPoint_B._calculatedWeight = 1.0f - itp;

					_cpLerpPoint_A.CalculateITPWeight();
					_cpLerpPoint_B.CalculateITPWeight();

				}

				_totalWeight = 0.0f;

				for (int i = 0; i < _subParamKeyValues.Count; i++)
				{
					_tmp_paramKeyValue = _subParamKeyValues[i];

					if (!_tmp_paramKeyValue._isCalculated)
					{
						_tmp_paramKeyValue._weight = 0.0f;
						continue;
					}

					_totalWeight += _tmp_paramKeyValue._weight;
				}

				if (_totalWeight > 0.0f)
				{
					for (int i = 0; i < _subParamKeyValues.Count; i++)
					{
						_tmp_paramKeyValue = _subParamKeyValues[i];
						if (_tmp_paramKeyValue._isCalculated)
						{
							_tmp_paramKeyValue._weight /= _totalWeight;
						}
					}
				}
			}
		}



		private void CalculateWeight_ControlParam_2D()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}


			_tmp_controlParam = _keyParamSetGroup._keyControlParam;


			//1. Param의 Weight를 모두 0으로 세팅 (+ 연산으로 Weight를 추가하는 방식)
			//2. 어느 RectArea에 있는지 결정한다.
			//3. Rect 안에서 itp를 계산한다.
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				_tmp_paramKeyValue = _subParamKeyValues[i];
				_tmp_paramKeyValue._weight = 0.0f;
				_tmp_paramKeyValue._isCalculated = false;//<<나중에 이것도 true로 올리자
			}




			Vector2 curValue = _tmp_controlParam._vec2_Cur;



			if (_cpLerpAreaLastSelected == null || !_cpLerpAreaLastSelected.IsInclude(curValue))
			{
				_cpLerpAreaLastSelected = null;
				for (int i = 0; i < _cpLerpAreas.Count; i++)
				{
					if(_cpLerpAreas[i].IsInclude(curValue))
					{
						_cpLerpAreaLastSelected = _cpLerpAreas[i];
						break;
					}
				}
				//_cpLerpAreaLastSelected = _cpLerpAreas.Find(delegate (apOptCalculatedLerpArea a)
				//{
				//	return a.IsInclude(curValue);
				//});
			}




			if (_cpLerpAreaLastSelected == null)
			{
				Debug.LogError("No Lerp Area");
				return;//처리가 안되는데요;
			}



			_cpLerpAreaLastSelected.ReadyToCalculate();

			float itpX = 0.0f;
			float itpY = 0.0f;
			float rectPosX_Min = _cpLerpAreaLastSelected._posLT.x;
			float rectPosX_Max = _cpLerpAreaLastSelected._posRB.x;
			float rectPosY_Min = _cpLerpAreaLastSelected._posLT.y;
			float rectPosY_Max = _cpLerpAreaLastSelected._posRB.y;

			itpX = 1.0f - Mathf.Clamp01((curValue.x - rectPosX_Min) / (rectPosX_Max - rectPosX_Min));
			itpY = 1.0f - Mathf.Clamp01((curValue.y - rectPosY_Min) / (rectPosY_Max - rectPosY_Min));

			_cpLerpAreaLastSelected._pointLT._calculatedWeight = itpX * itpY;
			_cpLerpAreaLastSelected._pointRT._calculatedWeight = (1.0f - itpX) * itpY;
			_cpLerpAreaLastSelected._pointLB._calculatedWeight = itpX * (1.0f - itpY);
			_cpLerpAreaLastSelected._pointRB._calculatedWeight = (1.0f - itpX) * (1.0f - itpY);

			_cpLerpAreaLastSelected._pointLT.CalculateITPWeight();
			_cpLerpAreaLastSelected._pointRT.CalculateITPWeight();
			_cpLerpAreaLastSelected._pointLB.CalculateITPWeight();
			_cpLerpAreaLastSelected._pointRB.CalculateITPWeight();

			_totalWeight = 0.0f;

			// 여러개의 키값을 사용할 거라면
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				_tmp_paramKeyValue = _subParamKeyValues[i];

				if (!_tmp_paramKeyValue._isCalculated)
				{
					_tmp_paramKeyValue._weight = 0.0f;
					continue;
				}

				//변경
				//Weight 시작값이 기본 1이 아니라, 거리에 따른 가중치로 바뀐다.
				_totalWeight += _tmp_paramKeyValue._weight;
			}
			if (_totalWeight > 0.0f)
			{
				for (int i = 0; i < _subParamKeyValues.Count; i++)
				{
					_tmp_paramKeyValue = _subParamKeyValues[i];
					if (_tmp_paramKeyValue._isCalculated)
					{
						_tmp_paramKeyValue._weight /= _totalWeight;
					}
				}
			}



		}

		//---------------------------------------------------
		// 계산함수 - KeyFrame
		//---------------------------------------------------
		private void CalculateWeight_KeyFrame()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyAnimTimelineLayer == null)
			{
				return;
			}
			apAnimTimelineLayer timlineLayer = _keyParamSetGroup._keyAnimTimelineLayer;
			apOptCalculatedResultParam.OptParamKeyValueSet curParamKeyValue = null;
			
			//9.26 : 이거 수정해야한다.
			//Int형 프레임과 Float형 프레임을 둘다 사용한다.
			int curFrame = timlineLayer._parentAnimClip.CurFrame;
			float curFrameFloat = timlineLayer._parentAnimClip.CurFrameFloat;//<<이건 실수형

			bool isLoop = timlineLayer._parentAnimClip.IsLoop;
			//apAnimKeyframe firstKeyframe = timlineLayer._firstKeyFrame;
			//apAnimKeyframe lastKeyframe = timlineLayer._lastKeyFrame;
			

			_totalWeight = 0.0f;

			apAnimKeyframe curKeyframe = null;
			apAnimKeyframe prevKeyframe = null;
			apAnimKeyframe nextKeyframe = null;

			//int indexOffsetA = 0;
			//int indexOffsetB = 0;
			int lengthFrames = timlineLayer._parentAnimClip.EndFrame - timlineLayer._parentAnimClip.StartFrame;
			int tmpCurFrameInt = 0;
			float tmpCurFrameFloat = 0;
			

			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				curParamKeyValue = _subParamKeyValues[i];

				curParamKeyValue._dist = -10.0f;
				curParamKeyValue._isCalculated = false;
				curParamKeyValue._weight = 0.0f;

				//추가 12.5
				curParamKeyValue._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.NotCalculated;

				//유효하지 않은 키프레임이면 처리하지 않는다.
				if (curParamKeyValue._paramSet.SyncKeyframe == null ||
					!curParamKeyValue._paramSet.SyncKeyframe._isActive)
				{
					//Debug.Log("[" + i + "] Not Active or Null Keyframe");
					continue;
				}

				curKeyframe = curParamKeyValue._paramSet.SyncKeyframe;
				prevKeyframe = curParamKeyValue._paramSet.SyncKeyframe._prevLinkedKeyframe;
				nextKeyframe = curParamKeyValue._paramSet.SyncKeyframe._nextLinkedKeyframe;


				//1. 프레임이 같다. => 100%
				if (curFrame == curKeyframe._frameIndex ||
					((curKeyframe._isLoopAsStart || curKeyframe._isLoopAsEnd) && curFrame == curKeyframe._loopFrameIndex))
				{
					curParamKeyValue._dist = 0.0f;
					curParamKeyValue._isCalculated = true;
					curParamKeyValue._weight = 1.0f;
					_totalWeight += 1.0f;
					
					//추가 12.5 : AnimKeyPos : 동일 프레임
					curParamKeyValue._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.ExactKey;
				}
				//else if(curFrame >= curKeyframe._activeFrameIndexMin &&
				//		curFrame < curKeyframe._frameIndex)
				else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
				{
					//범위 안에 들었다. [Prev - Cur]
					if (prevKeyframe != null)
					{
						
						tmpCurFrameInt = curFrame;
						if (tmpCurFrameInt > curKeyframe._frameIndex)
						{
							tmpCurFrameInt -= lengthFrames;
						}

						tmpCurFrameFloat = curFrameFloat;
						if (tmpCurFrameFloat > curKeyframe._frameIndex)
						{
							tmpCurFrameFloat -= lengthFrames;
						}

						//[중요] 재생 프레임 -> 보간 가중치 계산 코드
						//_cal_itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrameInt, true);//기존 : Int
						_cal_itp = curKeyframe._curveKey.GetItp_Float(tmpCurFrameFloat, true, tmpCurFrameInt);//변경 : Float

						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = _cal_itp;
						_totalWeight += _cal_itp;
						

						//추가 : Rotation Bias
						//Prev와 연결되었다면 Prev 설정을 적용한다.
						if (curKeyframe._prevRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
						{
							curParamKeyValue.SetAnimRotationBias(curKeyframe._prevRotationBiasMode, curKeyframe._prevRotationBiasCount);
						}
						//Debug.Log("[" + i + "] [Prev ~ Cur] " + itp);
						//Debug.Log("Prev ~ Next : " + itp);

						//추가 12.5 : AnimKeyPos - Next 프레임으로서 Prev 프레임과 보간이 된다.
						curParamKeyValue._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.NextKey;
					}
					else
					{
						//연결된게 없다면 이게 100% 가중치를 갖는다.
						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = 1.0f;
						_totalWeight += 1.0f;
						//Debug.Log("[" + i + "] [Prev ?? ~ Cur] 1.0");


						//추가 12.5 : AnimKeyPos - 동일 프레임
						curParamKeyValue._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.ExactKey;
					}

				}
				//else if(curFrame > curKeyframe._frameIndex &&
				//		curFrame <= curKeyframe._activeFrameIndexMax)
				else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
				{
					//범위안에 들었다 [Cur - Next]
					if (nextKeyframe != null)
					{
						
						tmpCurFrameInt = curFrame;
						if (tmpCurFrameInt < curKeyframe._frameIndex)
						{
							tmpCurFrameInt += lengthFrames;
						}

						tmpCurFrameFloat = curFrameFloat;
						if (tmpCurFrameFloat < curKeyframe._frameIndex)
						{
							tmpCurFrameFloat += lengthFrames;
						}

						//[중요] 재생 프레임 -> 보간 가중치 계산 코드
						//_cal_itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrameInt, false);//기존
						_cal_itp = curKeyframe._curveKey.GetItp_Float(tmpCurFrameFloat, false, tmpCurFrameInt);//변경 : Float

						//itp = 1.0f - itp;//결과가 B에 맞추어지므로 여기서는 Reverse

						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = _cal_itp;
						_totalWeight += _cal_itp;

						//Debug.Log("[" + i + "] [Cur ~ Next] " + itp);

						//추가 : Rotation Bias
						//Next와 연결되었다면 Next 설정을 적용한다.
						if (curKeyframe._nextRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
						{
							curParamKeyValue.SetAnimRotationBias(curKeyframe._nextRotationBiasMode, curKeyframe._nextRotationBiasCount);
						}
						
						//추가 12.5 : AnimKeyPos - Prev 프레임으로서 Next 프레임과 보간이 된다.
						curParamKeyValue._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.PrevKey;
					}
					else
					{
						//연결된게 없다면 이게 100% 가중치를 갖는다.
						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = 1.0f;
						_totalWeight += 1.0f;

						//Debug.Log("[" + i + "] [Cur ~ Next ??] 1.0");

						//추가 12.5 : AnimKeyPos - 동일 프레임
						curParamKeyValue._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.ExactKey;
						
					}
				}

				

			}

			if (_totalWeight > 0.0f)
			{
				//Debug.Log("Result --------------------------------");
				for (int i = 0; i < _subParamKeyValues.Count; i++)
				{
					curParamKeyValue = _subParamKeyValues[i];

					if (curParamKeyValue._isCalculated)
					{
						curParamKeyValue._weight /= _totalWeight;
						//Debug.Log("[" + curParamKeyValue._weight + "]");
					}
					else
					{
						curParamKeyValue._weight = 0.0f;
					}
				}
				//Debug.Log("-------------------------------------");
			}

			
		}



		/// <summary>
		/// 추가 20.11.24 : 개선된 버전. LUT를 이용하여 for 없이 딱! 필요한 Keyframe들과 ParamKeyValueSet만 꺼낸다.
		/// 굳이 다른걸 false 필요는 없으며 대신 Modifier에서도 선택된 ParamKeyValue만 사용해야한다.
		/// </summary>
		private void CalculateWeight_KeyFrame_WithLUT()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyAnimTimelineLayer == null)
			{
				return;
			}
			apAnimClip animClip = _keyParamSetGroup._keyAnimClip;
			
			//9.26 : 이거 수정해야한다.
			//Int형 프레임과 Float형 프레임을 둘다 사용한다.
			int curFrame = animClip.CurFrame;
			float curFrameFloat = animClip.CurFrameFloat;//<<이건 실수형
			
			_cal_resultAnimKeyPKV_A = null;
			_cal_resultAnimKeyPKV_B = null;
			_totalWeight = 0.0f;

			

			_cal_targetLUT = _animKeyLUT.GetLUT(curFrame);//이부분이 핵심

			if(_cal_targetLUT == null)
			{
				//조회된 LUT Result가 없다.
				_resultAnimKeyPKVs[0] = null;
				_resultAnimKeyPKVs[1] = null;

				_resultAnimKeyPKVIndices[0] = -1;
				_resultAnimKeyPKVIndices[1] = -1;
				_nResultAnimKey = 0;
				return;
			}

			_cal_resultAnimKeyPKV_A = _cal_targetLUT._paramKeyValueSet_Cur;
			_cal_resultAnimKeyPKV_B = _cal_targetLUT._paramKeyValueSet_Next;

			int iResultAnimKeyPKV_A = _cal_targetLUT._iParamKeyValueSet_Cur;
			int iResultAnimKeyPKV_B = _cal_targetLUT._iParamKeyValueSet_Next;

			_cal_keyframe_A = _cal_targetLUT._keyframe_Cur;
			_cal_keyframe_B = _cal_targetLUT._keyframe_Next;

			int lengthFrames = animClip.EndFrame - animClip.StartFrame;

			if(_cal_resultAnimKeyPKV_A == _cal_resultAnimKeyPKV_B)
			{
				//A와 B가 같다 > 한개의 PKV의 영역에 들어와서 100%로 계산해야함
				_cal_resultAnimKeyPKV_A._dist = 0.0f;
				_cal_resultAnimKeyPKV_A._isCalculated = true;
				_cal_resultAnimKeyPKV_A._weight = 1.0f;
				_totalWeight += 1.0f;
				_cal_resultAnimKeyPKV_A._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.ExactKey;

				//1개의 결과
				_resultAnimKeyPKVs[0] = _cal_resultAnimKeyPKV_A;
				_resultAnimKeyPKVs[1] = _cal_resultAnimKeyPKV_A;

				_resultAnimKeyPKVIndices[0] = iResultAnimKeyPKV_A;
				_resultAnimKeyPKVIndices[1] = iResultAnimKeyPKV_A;

				_nResultAnimKey = 1;
			}
			else
			{
				//두개의 영역에 들어왔다.
				int frameInt_ForA = curFrame;
				int frameInt_ForB = curFrame;
				float frameFloat_ForA = curFrameFloat;
				float frameFloat_ForB = curFrameFloat;

				if(frameInt_ForA < _cal_keyframe_A._frameIndex)
				{
					frameInt_ForA += lengthFrames;
					frameFloat_ForA += lengthFrames;
				}

				if(frameInt_ForB > _cal_keyframe_B._frameIndex)
				{
					frameInt_ForB -= lengthFrames;
				}
				if(frameFloat_ForB > _cal_keyframe_B._frameIndex)
				{
					frameFloat_ForB -= lengthFrames;
				}

				//PKV A 계산
				_cal_resultAnimKeyPKV_A._dist = 0.0f;
				_cal_resultAnimKeyPKV_A._isCalculated = true;
				_cal_resultAnimKeyPKV_A._weight = _cal_keyframe_A._curveKey.GetItp_Float(frameFloat_ForA, false, frameInt_ForA);
				_cal_resultAnimKeyPKV_A._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.PrevKey;

				//Rotation Bias도 계산한다.
				if(_cal_keyframe_A._nextRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
				{
					_cal_resultAnimKeyPKV_A.SetAnimRotationBias(_cal_keyframe_A._nextRotationBiasMode, _cal_keyframe_A._nextRotationBiasCount);
				}
				else
				{
					_cal_resultAnimKeyPKV_A.TurnOffRotationBias();//[v1.4.3] 이게 해제되지 않으면 이전 Loop에서의 결과값이 남은 버그
				}

				//PKV B 계산
				_cal_resultAnimKeyPKV_B._dist = 0.0f;
				_cal_resultAnimKeyPKV_B._isCalculated = true;
				_cal_resultAnimKeyPKV_B._weight = _cal_keyframe_B._curveKey.GetItp_Float(frameFloat_ForB, true, frameInt_ForB);
				_cal_resultAnimKeyPKV_B._animKeyPos = apOptCalculatedResultParam.AnimKeyPos.NextKey;

				//Rotation Bias도 계산한다.
				if(_cal_keyframe_B._prevRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
				{
					_cal_resultAnimKeyPKV_B.SetAnimRotationBias(_cal_keyframe_B._prevRotationBiasMode, _cal_keyframe_B._prevRotationBiasCount);
				}
				else
				{
					_cal_resultAnimKeyPKV_B.TurnOffRotationBias();//[v1.4.3] 이게 해제되지 않으면 이전 Loop에서의 결과값이 남은 버그
				}

				//[v1.4.3 버그를 위한 테스트]
				////만약 둘다 None인데 Rotation Bias가 계산된 경우는?
				//if(_cal_keyframe_A._nextRotationBiasMode == apAnimKeyframe.ROTATION_BIAS.None
				//	&& _cal_keyframe_B._prevRotationBiasMode == apAnimKeyframe.ROTATION_BIAS.None)
				//{
				//	if(_cal_resultAnimKeyPKV_A._isAnimRotationBias || _cal_resultAnimKeyPKV_B._isAnimRotationBias)
				//	{
				//		Debug.Log("잘못된 Rotaion Bias [A:" + _cal_resultAnimKeyPKV_A._isAnimRotationBias + " / B:" + _cal_resultAnimKeyPKV_B._isAnimRotationBias + "]");
				//	}
				//}

				//Weight 계산
				_totalWeight += _cal_resultAnimKeyPKV_A._weight;
				_totalWeight += _cal_resultAnimKeyPKV_B._weight;

				if(_totalWeight > 0.0f)
				{
					_cal_resultAnimKeyPKV_A._weight /= _totalWeight;
					_cal_resultAnimKeyPKV_B._weight /= _totalWeight;
					_totalWeight = 1.0f;
				}

				//1개의 결과
				_resultAnimKeyPKVs[0] = _cal_resultAnimKeyPKV_A;
				_resultAnimKeyPKVs[1] = _cal_resultAnimKeyPKV_B;

				_resultAnimKeyPKVIndices[0] = iResultAnimKeyPKV_A;
				_resultAnimKeyPKVIndices[1] = iResultAnimKeyPKV_B;
				_nResultAnimKey = 2;
			}

		}

		//---------------------------------------------------
		// 계산함수 - Static
		//---------------------------------------------------
		private void CalculateWeight_Static()
		{

		}

		// Get / Set
		//--------------------------------------------


		//-------------------------------------------------------------------------------------------------
		//Control Param 보간 관련
		//--------------------------------------------------------------------------------------
		private void MakeControlParamLerpAreas()
		{
			//1. ParamSetKeyValue => Point를 만든다.

			_cpLerpAreas.Clear();
			_cpLerpPoints.Clear();

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;
			if (controlParam == null)
			{
				return;
			}

			List<float> fPosXList = new List<float>();
			List<float> fPosYList = new List<float>();

			float bias = 0.001f;

			if (controlParam._valueType == apControlParam.TYPE.Float)
			{
				bias = Mathf.Abs((controlParam._float_Max - controlParam._float_Min) * 0.05f);
				bias = Mathf.Clamp(bias, 0.0001f, 0.1f);
			}
			else if (controlParam._valueType == apControlParam.TYPE.Vector2)
			{
				bias = Mathf.Min(Mathf.Abs((controlParam._vec2_Max.x - controlParam._vec2_Min.x) * 0.05f),
									Mathf.Abs((controlParam._vec2_Max.y - controlParam._vec2_Min.y) * 0.05f));
				bias = Mathf.Clamp(bias, 0.0001f, 0.1f);
			}


			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				apOptCalculatedResultParam.OptParamKeyValueSet keyValueSet = _subParamKeyValues[i];

				if (keyValueSet._paramSet == null)
				{ continue; }

				apOptCalculatedLerpPoint newPoint = null;
				switch (controlParam._valueType)
				{
					case apControlParam.TYPE.Int:
						{
							int iPos = keyValueSet._paramSet._conSyncValue_Int;
							newPoint = new apOptCalculatedLerpPoint(iPos, true);
						}
						break;

					case apControlParam.TYPE.Float:
						{
							float fPos = keyValueSet._paramSet._conSyncValue_Float;
							newPoint = new apOptCalculatedLerpPoint(fPos, true);
						}
						break;

					case apControlParam.TYPE.Vector2:
						{
							Vector2 vPos = keyValueSet._paramSet._conSyncValue_Vector2;
							newPoint = new apOptCalculatedLerpPoint(vPos, true);

							//위치를 저장해둔다.
							AddLerpPos(vPos, fPosXList, fPosYList, bias);
						}
						break;
				}

				newPoint.AddPoint(keyValueSet, 1.0f);//실제 키는 Weight가 1이다.
				_cpLerpPoints.Add(newPoint);

			}


			//2-1 1차원 값이면 오름차순 정렬하는 걸로 끝
			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					_cpLerpPoints.Sort(delegate (apOptCalculatedLerpPoint a, apOptCalculatedLerpPoint b)
					{
						return a._iPos - b._iPos;
					});
					break;

				case apControlParam.TYPE.Float:
					_cpLerpPoints.Sort(delegate (apOptCalculatedLerpPoint a, apOptCalculatedLerpPoint b)
					{
						return (int)((a._pos.x - b._pos.x) * (1.0f / bias) * 100.0f);
					});
					break;
			}


			//2-2. (Vector2인 경우) Rect Area를 만들자.
			if (controlParam._valueType == apControlParam.TYPE.Vector2)
			{
				//1) Min, Max 위치에 대해서 확인 후 가상 포인트를 추가하자
				//2) X, Y 값에 대해서 정렬
				//3) X, Y 좌표를 순회하면서 "포인트가 없다면" 가상 포인트를 추가하자
				//4) X, Y 좌표 순회하면서 RectArea를 만들자.

				//1)
				float minX = controlParam._vec2_Min.x;
				float minY = controlParam._vec2_Min.y;
				float maxX = controlParam._vec2_Max.x;
				float maxY = controlParam._vec2_Max.y;

				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);
				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);
				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);
				MakeVirtualLerpPoint(new Vector2(minX, minY), bias);

				//Min/Max 위치를 추가로 저장해둔다.
				AddLerpPos(new Vector2(minX, minY), fPosXList, fPosYList, bias);
				AddLerpPos(new Vector2(minX, maxY), fPosXList, fPosYList, bias);
				AddLerpPos(new Vector2(maxX, minY), fPosXList, fPosYList, bias);
				AddLerpPos(new Vector2(maxX, maxY), fPosXList, fPosYList, bias);

				//2) 위치 정렬
				fPosXList.Sort(delegate (float a, float b)
				{
					return (int)((a - b) * (1.0f / bias) * 1000.0f);
				});

				fPosYList.Sort(delegate (float a, float b)
				{
					return (int)((a - b) * (1.0f / bias) * 1000.0f);
				});

				//3) 좌표 순회하면서 포인트 추가
				for (int iX = 0; iX < fPosXList.Count; iX++)
				{
					for (int iY = 0; iY < fPosYList.Count; iY++)
					{
						MakeVirtualLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY]), bias);
					}
				}

				apOptCalculatedLerpPoint pointLT = null;
				apOptCalculatedLerpPoint pointRT = null;
				apOptCalculatedLerpPoint pointLB = null;
				apOptCalculatedLerpPoint pointRB = null;

				//4) 좌표 순회하면서 RectArea 만들기
				for (int iX = 0; iX < fPosXList.Count - 1; iX++)
				{
					for (int iY = 0; iY < fPosYList.Count - 1; iY++)
					{
						pointLT = GetLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY]), bias);
						pointRT = GetLerpPoint(new Vector2(fPosXList[iX + 1], fPosYList[iY]), bias);
						pointLB = GetLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY + 1]), bias);
						pointRB = GetLerpPoint(new Vector2(fPosXList[iX + 1], fPosYList[iY + 1]), bias);

						apOptCalculatedLerpArea lerpArea = new apOptCalculatedLerpArea(pointLT, pointRT, pointLB, pointRB);

						_cpLerpAreas.Add(lerpArea);
					}
				}

			}
		}
		private apOptCalculatedLerpPoint GetLerpPoint(int iPos)
		{
			return _cpLerpPoints.Find(delegate (apOptCalculatedLerpPoint a)
			{
				return a._iPos == iPos;
			});
		}

		private apOptCalculatedLerpPoint GetLerpPoint(float fPos, float bias)
		{
			return _cpLerpPoints.Find(delegate (apOptCalculatedLerpPoint a)
			{
				return Mathf.Abs(a._pos.x - fPos) < bias;
			});
		}

		private apOptCalculatedLerpPoint GetLerpPoint(Vector2 vPos, float bias)
		{
			return _cpLerpPoints.Find(delegate (apOptCalculatedLerpPoint a)
			{
				return Mathf.Abs(a._pos.x - vPos.x) < bias &&
						Mathf.Abs(a._pos.y - vPos.y) < bias;
			});
		}

		private void AddLerpPos(Vector2 pos, List<float> posXList, List<float> posYList, float bias)
		{
			if (!posXList.Exists(delegate (float a)
			{ return Mathf.Abs(a - pos.x) < bias; }))
			{
				posXList.Add(pos.x);
			}

			if (!posYList.Exists(delegate (float a)
			{ return Mathf.Abs(a - pos.y) < bias; }))
			{
				posYList.Add(pos.y);
			}
		}

		/// <summary>
		/// 가상의 Lerp Point (Vector2)를 만든다.
		/// bias값을 이용하여 기존에 생성된 값이 있는지 확인한다.
		/// 기존에 생성된 값이나 새로 만든 값을 리턴한다.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="bias"></param>
		/// <returns></returns>
		private apOptCalculatedLerpPoint MakeVirtualLerpPoint(Vector2 pos, float bias)
		{
			apOptCalculatedLerpPoint existLerpPoint = GetLerpPoint(pos, bias);
			if (existLerpPoint != null)
			{
				return existLerpPoint;
			}
			apOptCalculatedLerpPoint newPoint = new apOptCalculatedLerpPoint(pos, false);
			_cpLerpPoints.Add(newPoint);

			//실제 Control Param Key를 입력해야한다.


			List<apOptCalculatedLerpPoint> realLerpPoints = _cpLerpPoints.FindAll(delegate (apOptCalculatedLerpPoint a)
			{
				return a._isRealPoint;
			});

			if (realLerpPoints.Count == 0)
			{
				return newPoint;
			}

			if (realLerpPoints.Count == 1)
			{
				newPoint.Addpoints(realLerpPoints[0], 1.0f);
				return newPoint;
			}


			//Pos를 기준으로 Lerp의 합을 계산한다.
			//전체 거리의 평균을 잡고, 그 평균 이내의 Point만 계산한다.

			List<float> distList = new List<float>();
			List<float> weightList = new List<float>();
			float totalDist = 0.0f;
			float totalWeight = 0.0f;

			apOptCalculatedLerpPoint lerpPoint = null;
			for (int i = 0; i < realLerpPoints.Count; i++)
			{
				lerpPoint = realLerpPoints[i];
				float dist = Vector2.Distance(pos, lerpPoint._pos);
				totalDist += dist;

				distList.Add(dist);
			}

			//float meanDist = totalDist / 2.0f;//<<이부분이 필요할까?

			for (int i = 0; i < realLerpPoints.Count; i++)
			{
				weightList.Add(1.0f);

				//if(distList[i] < meanDist)
				//{
				//	weightList.Add(1.0f);
				//}
				//else
				//{
				//	weightList.Add(-1.0f);
				//}
			}

			apOptCalculatedLerpPoint curPoint = null;
			apOptCalculatedLerpPoint nextpoint = null;

			for (int iCur = 0; iCur < realLerpPoints.Count - 1; iCur++)
			{
				curPoint = realLerpPoints[iCur];

				if (weightList[iCur] <= 0.0f)
				{
					continue;
				}

				float distCur = distList[iCur];

				for (int iNext = iCur + 1; iNext < realLerpPoints.Count; iNext++)
				{
					nextpoint = realLerpPoints[iNext];
					if (weightList[iNext] <= 0.0f)
					{
						continue;
					}

					float distNext = distList[iNext];

					float distSum = distCur + distNext;

					if (distSum <= 0.0f)
					{
						continue;
					}

					float itp = 1.0f - (distCur / distSum);
					weightList[iCur] *= itp;
					weightList[iNext] *= 1.0f - itp;
				}
			}


			for (int i = 0; i < realLerpPoints.Count - 1; i++)
			{
				if (weightList[i] < 0.0f)
				{
					weightList[i] = 0.0f;
				}
				else
				{
					totalWeight += weightList[i];
				}
			}

			if (totalWeight > 0.0f)
			{
				for (int i = 0; i < realLerpPoints.Count; i++)
				{
					lerpPoint = realLerpPoints[i];
					if (weightList[i] > 0.0f)
					{
						float pointWeight = weightList[i] / totalWeight;

						newPoint.Addpoints(lerpPoint, pointWeight);
					}
				}
			}

			return newPoint;
		}

	}

}