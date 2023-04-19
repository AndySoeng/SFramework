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

	public class apCalculatedResultParamSubList
	{
		// Members
		//--------------------------------------------------
		public apCalculatedResultParam _parentResultParam = null;

		//이걸 그냥 Modifier의 Key값과 연계하는게 좋을지도
		//public apModifierParamSetGroup.SYNC_TARGET _keySyncType = apModifierParamSetGroup.SYNC_TARGET.Controller;
		public apModifierParamSetGroup _keyParamSetGroup = null;

		//Key
		//1. Control Param인 경우
		//public apControlParam _controlParam = null;

		//2. 키프레임인 경우 : TODO
		//3. Static인 경우 -> 없음 (Input에 따라 구분되지 않는다.)


		public List<apCalculatedResultParam.ParamKeyValueSet> _subParamKeyValues = new List<apCalculatedResultParam.ParamKeyValueSet>();

		//계산용
		public float _totalWeight = 0.0f;

		private const float _distBias = 0.01f;

		/// <summary>
		/// Control Param 타입인 경우에, "어떤 Key를 선택하여 Weight를 수행할 지를 결정"하도록 도와주는 클래스
		/// Input Control Param에 대해서
		/// 1) 입력된 1차원 영역 / 2차원 삼각 영역 / 2.5D 삼각 영역에 포함되는지 판단
		/// 2) 포함이 된다면 "이 영역에서 Weight를 같이 계산할 ParamSetKey들"을 리턴
		/// </summary>
		public class ControlParamArea
		{
			public int _iKeyA, _iKeyB;//Int 타입일때
			public Vector2 _keyPosA, _keyPosB, _keyPosC;
			public Vector2 _center;
			public float _radius = 0.0f;

		}

		// 개선된 Control Param 보간
		//Control Param 타입인 경우
		private List<apCalculatedLerpPoint> _cpLerpPoints = null;
		private List<apCalculatedLerpArea> _cpLerpAreas = null;

		//1D Lerp 처리를 위한 참조 변수
		private apCalculatedLerpPoint _cpLerpPoint_A = null;
		private apCalculatedLerpPoint _cpLerpPoint_B = null;

		//2D Lerp 처리를 위한 참조 변수
		private apCalculatedLerpArea _cpLerpAreaLastSelected = null;//빠른 처리를 위해 "이전에 참조된 Area"를 저장하자.




		// Init
		//--------------------------------------------------
		public apCalculatedResultParamSubList(apCalculatedResultParam parentResultParam)
		{
			_parentResultParam = parentResultParam;
			_subParamKeyValues.Clear();
		}

		public void SetParamSetGroup(apModifierParamSetGroup paramSetGroup)
		{
			_keyParamSetGroup = paramSetGroup;
			//_controlParam = _keyParamSetGroup._keyControlParam;
		}

		////1. Control Param 타입
		//public void SetControlParam(apControlParam controlParam)
		//{
		//	_controlParam = controlParam;
		//	_keySyncType = apModifierParamSetGroup.SYNC_TARGET.Controller;
		//}

		//public void SetKeyFrame()
		//{
		//	_keySyncType = apModifierParamSetGroup.SYNC_TARGET.KeyFrame;
		//}

		//public void SetStatic()
		//{
		//	_keySyncType = apModifierParamSetGroup.SYNC_TARGET.Static;
		//}

		////TODO : 키프레임/Static..나중에 추가


		// Add / Clear
		//--------------------------------------------------
		public void ClearParams()
		{
			_subParamKeyValues.Clear();
		}

		public void AddParamKeyValueSet(apCalculatedResultParam.ParamKeyValueSet paramKeyValue)
		{
			if (_subParamKeyValues.Contains(paramKeyValue))
			{
				return;
			}
			//Debug.Log("AddParamKeyValueSet");
			_subParamKeyValues.Add(paramKeyValue);


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
						if (_cpLerpPoints == null)
						{ _cpLerpPoints = new List<apCalculatedLerpPoint>(); }
						if (_cpLerpAreas == null)
						{ _cpLerpAreas = new List<apCalculatedLerpArea>(); }

						_cpLerpPoint_A = null;
						_cpLerpPoint_B = null;

						_cpLerpAreaLastSelected = null;

						MakeControlParamLerpAreas();
					}
					break;

			}
		}


		// 계산
		//--------------------------------------------------
		public void InitCalculate()
		{
			_totalWeight = 0.0f;
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				_subParamKeyValues[i].ReadyToCalculate();
			}
		}
		public void Calculate()
		{
			if (_keyParamSetGroup == null)
			{
				return;
			}

			_totalWeight = 0.0f;
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				_subParamKeyValues[i].ReadyToCalculate();
			}

			//Sync 타입에 따라서 ParamSet의 Weight를 계산한다. [중요!]
			switch (_keyParamSetGroup._syncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					//CalculateWeight_ControlParam();//구버전 Lerp
					CalculateWeight_ControlParam_WithMetaData();//신버전 Bilinear
					break;

				case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
					CalculateWeight_Keyframe();
					break;

				case apModifierParamSetGroup.SYNC_TARGET.Static:
					CalculateWeight_Static();
					break;
			}
		}


		//-----------------------------------------------------------------------------------
		// 중요
		//-----------------------------------------------------------------------------------

		/// <summary>
		/// ParamSet간의 Weight를 계산한다. [ControlParam이 입력값인 경우]
		/// </summary>
		private void CalculateWeight_ControlParam()
		{
			//if(_controlParam == null)
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyControlParam == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;

			//1. 현재 값에 따라서 Dist 값을 넣자
			float minDist = float.MaxValue;
			float maxDist = 0.0f;
			float dist = 0.0f;
			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			apCalculatedResultParam.ParamKeyValueSet nextParamKeyValue = null;

			//int nSubParamKeyValues = _subParamKeyValues.Count;
			_totalWeight = 0.0f;

			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				curParamKeyValue = _subParamKeyValues[i];
				dist = -10.0f;
				curParamKeyValue._isCalculated = false;

#if UNITY_EDITOR
				//if (!curParamKeyValue._isActive_InEditorExclusive)
				if (!curParamKeyValue.IsActive)
				{
					//에디터에서 제한한 Paramkey면
					curParamKeyValue._dist = -10.0f;
					curParamKeyValue._isCalculated = false;

					//bool isKeyNull = false;
					//bool isCalculateNotEnabled = false;
					//if (curParamKeyValue._keyParamSetGroup == null)
					//{
					//	isKeyNull = true;
					//}
					//else if (!curParamKeyValue._keyParamSetGroup.IsCalculateEnabled)
					//{
					//	//isCalculateNotEnabled = true;
					//}
					//Debug.LogError("CalResultParamSubList Weight Failed : " + _parentResultParam._targetRenderUnit + " / "
					//	+ "ParamSetGroup Is Null : " + (isKeyNull) + " / Calculate Enabled : " + isCalculateNotEnabled);
					continue;
				}
#endif

				//수식 1 : IDW 방식 (Inverse Distance Weighting)
				//-----------------------------------------------
				#region 수식 1 적용
				switch (controlParam._valueType)
				{
					//case apControlParam.TYPE.Bool:
					//	if (curParamKeyValue._paramSet._conSyncValue_Bool == controlParam._bool_Cur)
					//	{
					//		curParamKeyValue._dist = 0.0f;
					//		curParamKeyValue._isCalculated = true;
					//	}
					//	else
					//	{
					//		curParamKeyValue._dist = -10.0f;
					//		curParamKeyValue._isCalculated = false;
					//	}
					//	break;

					case apControlParam.TYPE.Int:
						dist = controlParam.GetNormalizedDistance_Int(curParamKeyValue._paramSet._conSyncValue_Int);
						break;

					case apControlParam.TYPE.Float:
						dist = controlParam.GetNormalizedDistance_Float(curParamKeyValue._paramSet._conSyncValue_Float);
						break;

					case apControlParam.TYPE.Vector2:
						dist = controlParam.GetNormalizedDistance_Vector2(curParamKeyValue._paramSet._conSyncValue_Vector2);
						break;

						//case apControlParam.TYPE.Vector3:
						//	dist = controlParam.GetNormalizedDistance_Vector3(curParamKeyValue._paramSet._conSyncValue_Vector3);
						//	break;

						//case apControlParam.TYPE.Color:
						//	break;
				}


				if (dist < -1.0f)
				{
					//계산 안함
					continue;
				}

				curParamKeyValue._dist = dist;
				curParamKeyValue._isCalculated = true;

				if (dist < minDist)
				{
					minDist = dist;//<<최소 값
				}
				if (dist > maxDist)
				{
					maxDist = dist;//최대값 (가장 Weight가 적게 걸리는 값)
				}


				#endregion
				//-----------------------------------------------
			}

			if (maxDist - minDist < 0.0001f)
			{
				maxDist = minDist + 0.0001f;
			}


			_totalWeight = 0.0f;
			// 여러개의 키값을 사용할 거라면


			#region [미사용 코드] 수식이 중복된다.
			//List<float> keepWeightRatios = new List<float>();


			//for (int i = 0; i < _subParamKeyValues.Count; i++)
			//{
			//	curParamKeyValue = _subParamKeyValues[i];
			//	if(curParamKeyValue._dist < -1.0f)
			//	{
			//		continue;
			//	}
			//	float keepWeightRatio = Mathf.Clamp01((curParamKeyValue._dist - minDist) / (maxDist - minDist));

			//	keepWeightRatios.Add(keepWeightRatio);
			//}

			//keepWeightRatios.Sort(delegate (float a, float b)
			//	{
			//		return (int)((a * 1000.0f) - (b * 1000.0f));
			//	});

			//bool isLimitedWeight = false;
			//float keepWeightRatio_Min = 0.0f;
			//float keepWeightRatio_Min2 = 0.0f;
			//float limitWeight = 1.0f;
			//if(keepWeightRatios.Count >= 2)
			//{
			//	isLimitedWeight = true;
			//	keepWeightRatio_Min = keepWeightRatios[0];
			//	keepWeightRatio_Min2 = keepWeightRatios[1];

			//	if(keepWeightRatio_Min2 - keepWeightRatio_Min < 0.0001f)
			//	{
			//		keepWeightRatio_Min2 = keepWeightRatio_Min + 0.0001f;
			//	}
			//	limitWeight = Mathf.Clamp01(0.5f - keepWeightRatio_Min) * 2;
			//} 
			#endregion

			#region [미사용 코드] 역 선형 수식이지만 오류가 있다.
			//for (int i = 0; i < _subParamKeyValues.Count; i++)
			//{
			//	curParamKeyValue = _subParamKeyValues[i];

			//	if (curParamKeyValue._dist < -1.0f)
			//	{
			//		curParamKeyValue._weight = 0.0f;
			//		curParamKeyValue._isCalculated = false;
			//		continue;
			//	}

			//	//ITP 계산
			//	//1 - Dist로 역 선형 보간을 사용한다.
			//	//가장 가까운 포인트에서 MinDist를 구한다.
			//	//Min Dist가 0일때 = 어느 점에 도달했을때
			//	//> 다른 Weight가 0이 되어야 하며, Min Point인 부분은 Weight가 보전되어야 한다.
			//	//"보전률" = deltaMinDist가 작을수록 크다 (max를 구해야겠네)
			//	//"Mul-Weight" : 보전률에 비례한다. 보전률이 0일땐 MinDist (Normalize)의 값을 가지고, 최대일땐 1의 값을 가진다.

			//	float keepWeightRatio = 1.0f - Mathf.Clamp01((curParamKeyValue._dist - minDist) / (maxDist - minDist));

			//	// 가까우면 = 1 (감소 없음) / 멀면 minDist (포인트에 근접할 수록 0에 수렴)

			//	float multiplyWeight = (1.0f * keepWeightRatio) + minDist * (1.0f - keepWeightRatio);

			//	//만약, minDist가 일정 값 이하로 떨어지면 0으로 multiplyWeight가 아예 수렴해야한다.



			//	//float revWeight = (maxDist - curParamKeyValue._dist) * multiplyWeight;
			//	float revWeight = (2.0f - curParamKeyValue._dist) * multiplyWeight;//<<수정 : MaxDist가 아니라 Normalize 영역 크기(-1 ~ 1 = 2)로 빼야 적절하게 나온다.


			//	_totalWeight += revWeight;
			//	curParamKeyValue._weight = revWeight;
			//	curParamKeyValue._isCalculated = true;
			//} 
			#endregion

			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				curParamKeyValue = _subParamKeyValues[i];

				if (curParamKeyValue._dist < -1.0f)
				{
					curParamKeyValue._weight = 0.0f;
					curParamKeyValue._weightBase = 0.0f;
					curParamKeyValue._isCalculated = false;
					continue;
				}

				//변경
				//Weight 시작값이 기본 1이 아니라, 거리에 따른 가중치로 바뀐다.
				curParamKeyValue._weight = 1.0f;

				if (_subParamKeyValues.Count <= 2)
				//if(true)
				{
					curParamKeyValue._weightBase = 1.0f;
				}
				else
				{
					curParamKeyValue._weightBase = controlParam.GetInterpolationWeight(curParamKeyValue._dist);
				}
				curParamKeyValue._isCalculated = true;
				//_totalWeight += 1.0f;
				_totalWeight += curParamKeyValue._weight;//변경!

			}

			if (_subParamKeyValues.Count >= 2)
			{
				_totalWeight = 0.0f;

				for (int i = 0; i < _subParamKeyValues.Count - 1; i++)
				{
					curParamKeyValue = _subParamKeyValues[i];
					if (!curParamKeyValue._isCalculated)
					{
						continue;
					}
					if (curParamKeyValue._weight < 0.00001f)
					{
						continue;
					}
					for (int j = i + 1; j < _subParamKeyValues.Count; j++)
					{
						nextParamKeyValue = _subParamKeyValues[j];
						if (!nextParamKeyValue._isCalculated)
						{
							continue;
						}

						float sumDist = curParamKeyValue._dist + nextParamKeyValue._dist;
						if (sumDist < 0.0001f)
						{
							curParamKeyValue._weight *= 1.0f;
							nextParamKeyValue._weight *= 1.0f;
						}
						else
						{
							float itp = Mathf.Clamp01((sumDist - curParamKeyValue._dist) / sumDist);
							//float baseWeight = (curParamKeyValue._weightBase + nextParamKeyValue._weightBase) * 0.5f;
							float baseWeight = Mathf.Clamp01(curParamKeyValue._weightBase + nextParamKeyValue._weightBase);
							//float baseWeight = curParamKeyValue._weightBase * nextParamKeyValue._weightBase;

							//float itp = apAnimCurve.GetSmoothInterpolation((sumDist - curParamKeyValue._dist) / sumDist);
							//curParamKeyValue._weight *= itp;
							//nextParamKeyValue._weight *= (1.0f - itp);
							curParamKeyValue._weight = curParamKeyValue._weight * ((1.0f - baseWeight) + itp * baseWeight);
							nextParamKeyValue._weight = nextParamKeyValue._weight * ((1.0f - baseWeight) + (1.0f - itp) * baseWeight);


							if (itp < 0.00001f)
							{
								break;
							}
						}
					}
				}

				for (int i = 0; i < _subParamKeyValues.Count; i++)
				{
					curParamKeyValue = _subParamKeyValues[i];
					if (curParamKeyValue._isCalculated)
					{
						_totalWeight += curParamKeyValue._weight;
					}
				}
			}

			//공통 부분
			if (_totalWeight > 0.0f)
			{
				for (int i = 0; i < _subParamKeyValues.Count; i++)
				{
					curParamKeyValue = _subParamKeyValues[i];

					if (curParamKeyValue._isCalculated)
					{
						curParamKeyValue._weight /= _totalWeight;
					}
					else
					{
						curParamKeyValue._weight = 0.0f;
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

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;

			//Value 타입에 따라 처리가 달라진다.
			switch (controlParam._valueType)
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

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;

			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				curParamKeyValue = _subParamKeyValues[i];
				curParamKeyValue._weight = 0.0f;
				curParamKeyValue._isCalculated = false;//<<나중에 이것도 true로 올리자
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
					if (controlParam._valueType == apControlParam.TYPE.Int)
					{
						if (controlParam._int_Cur < _cpLerpPoint_A._iPos ||
							controlParam._int_Cur > _cpLerpPoint_B._iPos)
						{
							isRefreshLerpPointRange = true;
						}
					}
					else
					{
						if (controlParam._float_Cur < _cpLerpPoint_A._pos.x ||
							controlParam._float_Cur > _cpLerpPoint_B._pos.x)
						{
							isRefreshLerpPointRange = true;
						}
					}
				}

				if (isRefreshLerpPointRange)
				{
					//0..1..2.. [value]..3...4
					int iB = -1;
					if (controlParam._valueType == apControlParam.TYPE.Int)
					{
						for (int i = 0; i < _cpLerpPoints.Count; i++)
						{
							if (controlParam._int_Cur <= _cpLerpPoints[i]._iPos)
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
							if (controlParam._float_Cur <= _cpLerpPoints[i]._pos.x)
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
					if (controlParam._valueType == apControlParam.TYPE.Int)
					{
						itp = 1.0f - Mathf.Clamp01((float)(controlParam._int_Cur - _cpLerpPoint_A._iPos) / (float)(_cpLerpPoint_B._iPos - _cpLerpPoint_A._iPos));
					}
					else
					{
						itp = 1.0f - Mathf.Clamp01((float)(controlParam._float_Cur - _cpLerpPoint_A._pos.x) / (float)(_cpLerpPoint_B._pos.x - _cpLerpPoint_A._pos.x));
					}

					_cpLerpPoint_A._calculatedWeight = itp;
					_cpLerpPoint_B._calculatedWeight = 1.0f - itp;

					_cpLerpPoint_A.CalculateITPWeight();
					_cpLerpPoint_B.CalculateITPWeight();

				}

				_totalWeight = 0.0f;

				for (int i = 0; i < _subParamKeyValues.Count; i++)
				{
					curParamKeyValue = _subParamKeyValues[i];

					if (!curParamKeyValue._isCalculated)
					{
						curParamKeyValue._weight = 0.0f;
						continue;
					}

					_totalWeight += curParamKeyValue._weight;
				}

				if (_totalWeight > 0.0f)
				{
					for (int i = 0; i < _subParamKeyValues.Count; i++)
					{
						curParamKeyValue = _subParamKeyValues[i];
						if (curParamKeyValue._isCalculated)
						{
							curParamKeyValue._weight /= _totalWeight;
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

			apControlParam controlParam = _keyParamSetGroup._keyControlParam;


			//1. Param의 Weight를 모두 0으로 세팅 (+ 연산으로 Weight를 추가하는 방식)
			//2. 어느 RectArea에 있는지 결정한다.
			//3. Rect 안에서 itp를 계산한다.
			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				curParamKeyValue = _subParamKeyValues[i];
				curParamKeyValue._weight = 0.0f;
				curParamKeyValue._isCalculated = false;//<<나중에 이것도 true로 올리자
			}

			Vector2 curValue = controlParam._vec2_Cur;


			

			if (_cpLerpAreaLastSelected == null || !_cpLerpAreaLastSelected.IsInclude(curValue))
			{
				//TODO : 이부분 성능 최적화 필요
				//여기서 delegate때문에 약간의 메모리 할당이 발생한다.
				//_cpLerpAreaLastSelected = _cpLerpAreas.Find(delegate (apCalculatedLerpArea a)
				//{
				//	return a.IsInclude(curValue);
				//});

				//성능은 떨어지지만 메모리 할당이 발생하지 않는 부분
				for (int i = 0; i < _cpLerpAreas.Count; i++)
				{
					if(_cpLerpAreas[i].IsInclude(curValue))
					{
						_cpLerpAreaLastSelected = _cpLerpAreas[i];
						break;
					}
				}
			}
			if (_cpLerpAreaLastSelected == null)
			{
				//잠깐 끕시더
				//Debug.LogError("No Lerp Area");
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
				curParamKeyValue = _subParamKeyValues[i];

				if (!curParamKeyValue._isCalculated)
				{
					curParamKeyValue._weight = 0.0f;
					continue;
				}

				//변경
				//Weight 시작값이 기본 1이 아니라, 거리에 따른 가중치로 바뀐다.
				_totalWeight += curParamKeyValue._weight;
			}
			if (_totalWeight > 0.0f)
			{
				for (int i = 0; i < _subParamKeyValues.Count; i++)
				{
					curParamKeyValue = _subParamKeyValues[i];
					if (curParamKeyValue._isCalculated)
					{
						curParamKeyValue._weight /= _totalWeight;
					}
				}
			}
		}



		/// <summary>
		/// ParamSet간의 Weight를 계산한다. [Keyframe이 입력값인 경우]
		/// </summary>
		private void CalculateWeight_Keyframe()
		{
			if (_keyParamSetGroup == null || _keyParamSetGroup._keyAnimTimelineLayer == null)
			{
				return;
			}

			bool isPlayedAnimClip = false;//<<이 코드가 추가됨
			if (_keyParamSetGroup._keyAnimClip._isSelectedInEditor)
			{
				_keyParamSetGroup._layerWeight = 1.0f;
				isPlayedAnimClip = true;
			}
			else
			{
				_keyParamSetGroup._layerWeight = 0.0f;
				isPlayedAnimClip = false;
			}

			apAnimTimelineLayer timlineLayer = _keyParamSetGroup._keyAnimTimelineLayer;
			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			int curFrame = timlineLayer._parentAnimClip.CurFrame;

			bool isLoop = timlineLayer._parentAnimClip.IsLoop;
			

			_totalWeight = 0.0f;

			apAnimKeyframe curKeyframe = null;
			apAnimKeyframe prevKeyframe = null;
			apAnimKeyframe nextKeyframe = null;

			int lengthFrames = timlineLayer._parentAnimClip.EndFrame - timlineLayer._parentAnimClip.StartFrame;
			int tmpCurFrame = 0;


			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				curParamKeyValue = _subParamKeyValues[i];
				curParamKeyValue._dist = -10.0f;
				curParamKeyValue._isCalculated = false;

				//추가 11.29 : Animation Key 위치 타입이 추가되었다.
				curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.NotCalculated;

				//유효하지 않은 키프레임이면 처리하지 않는다.
				if (curParamKeyValue._paramSet.SyncKeyframe == null ||
					!curParamKeyValue._paramSet.SyncKeyframe._isActive ||
					//!curParamKeyValue._isActive_InEditorExclusive
					!curParamKeyValue.IsActive ||
					!isPlayedAnimClip //<<애니메이션 재생 안될때는 여기서 생략이 되어야 하는데, 이게 왜 없었지;;
					)
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

					//추가 11.29 : AnimKeyPos - 동일 프레임
					curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;
				}
				//else if(curFrame >= curKeyframe._activeFrameIndexMin &&
				//		curFrame < curKeyframe._frameIndex)
				else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Prev))
				{
					//범위 안에 들었다. [Prev - Cur]
					if (prevKeyframe != null)
					{
						//indexOffsetA = 0;
						//indexOffsetB = 0;
						//if(prevKeyframe._frameIndex > curKeyframe._frameIndex)
						//{
						//	//Loop인 경우 Prev가 더 클 수 있다.
						//	indexOffsetA = -lengthFrames;
						//}

						tmpCurFrame = curFrame;
						if (tmpCurFrame > curKeyframe._frameIndex)
						{
							tmpCurFrame -= lengthFrames;
						}

						//float itp = apAnimCurve.GetCurvedRelativeInterpolation(prevKeyframe._curveKey, curKeyframe._curveKey, curFrame, curKeyframe._curveKey._isPrevKeyUseDummyIndex, false);
						//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, prevKeyframe._curveKey, tmpCurFrame, true);

						//>> 변경
						float itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, true);

						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = itp;
						_totalWeight += itp;

						//추가 : Rotation Bias
						//Prev와 연결되었다면 Prev 설정을 적용한다.
						if (curKeyframe._prevRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
						{
							curParamKeyValue.SetAnimRotationBias(curKeyframe._prevRotationBiasMode, curKeyframe._prevRotationBiasCount);
						}

						//Debug.Log("[" + i + "] [Prev ~ Cur] " + itp);
						//Debug.Log("Prev ~ Next : " + itp);

						//추가 11.29 : AnimKeyPos - Next 프레임으로서 Prev 프레임과 보간이 된다.
						curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.NextKey;
					}
					else
					{
						//연결된게 없다면 이게 100% 가중치를 갖는다.
						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = 1.0f;
						_totalWeight += 1.0f;
						//Debug.Log("[" + i + "] [Prev ?? ~ Cur] 1.0");

						//추가 11.29 : AnimKeyPos - 동일 프레임
						curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;
					}

				}
				//else if(curFrame > curKeyframe._frameIndex &&
				//		curFrame <= curKeyframe._activeFrameIndexMax)
				else if (curKeyframe.IsFrameIn(curFrame, apAnimKeyframe.LINKED_KEY.Next))
				{
					//범위안에 들었다 [Cur - Next]
					if (nextKeyframe != null)
					{
						//indexOffsetA = 0;
						//indexOffsetB = 0;
						//if(nextKeyframe._frameIndex < curKeyframe._frameIndex)
						//{
						//	//Loop인 경우 Next가 더 작을 수 있다.
						//	indexOffsetB = lengthFrames;
						//}

						tmpCurFrame = curFrame;
						if (tmpCurFrame < curKeyframe._frameIndex)
						{
							tmpCurFrame += lengthFrames;
						}

						//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, nextKeyframe._curveKey, curFrame, false, curKeyframe._curveKey._isNextKeyUseDummyIndex);
						//float itp = apAnimCurve.GetCurvedRelativeInterpolation(curKeyframe._curveKey, nextKeyframe._curveKey, tmpCurFrame, false);

						//>> 변경
						float itp = curKeyframe._curveKey.GetItp_Int(tmpCurFrame, false);

						//itp = 1.0f - itp;//결과가 B에 맞추어지므로 여기서는 Reverse

						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = itp;
						_totalWeight += itp;

						//추가 : Rotation Bias
						//Next와 연결되었다면 Next 설정을 적용한다.
						if (curKeyframe._nextRotationBiasMode != apAnimKeyframe.ROTATION_BIAS.None)
						{
							curParamKeyValue.SetAnimRotationBias(curKeyframe._nextRotationBiasMode, curKeyframe._nextRotationBiasCount);
						}

						//추가 11.29 : AnimKeyPos - Prev 프레임으로서 Next 프레임과 보간이 된다.
						curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.PrevKey;
					}
					else
					{
						//연결된게 없다면 이게 100% 가중치를 갖는다.
						curParamKeyValue._dist = 0.0f;
						curParamKeyValue._isCalculated = true;
						curParamKeyValue._weight = 1.0f;
						_totalWeight += 1.0f;

						//추가 11.29 : AnimKeyPos - 동일 프레임
						curParamKeyValue._animKeyPos = apCalculatedResultParam.AnimKeyPos.ExactKey;
					}
				}
			}

			if (_totalWeight > 0.0f)
			{
				//Debug.Log("Result --------------------------------");
				//float prevWeight = 0.0f;
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

		private void CalculateWeight_Static()
		{
			///계산할 필요가 없는데용...
			if (_keyParamSetGroup == null)
			{
				Debug.LogError("Key ParamSet Group is Null / Key Control Param Is null");
				return;
			}

			apCalculatedResultParam.ParamKeyValueSet curParamKeyValue = null;
			for (int i = 0; i < _subParamKeyValues.Count; i++)
			{
				curParamKeyValue = _subParamKeyValues[i];
				curParamKeyValue._weight = 1.0f;
				curParamKeyValue._isCalculated = true;//<<나중에 이것도 true로 올리자
			}
		}




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
				apCalculatedResultParam.ParamKeyValueSet keyValueSet = _subParamKeyValues[i];

				if (keyValueSet._paramSet == null)
				{ continue; }

				apCalculatedLerpPoint newPoint = null;
				switch (controlParam._valueType)
				{
					case apControlParam.TYPE.Int:
						{
							int iPos = keyValueSet._paramSet._conSyncValue_Int;
							newPoint = new apCalculatedLerpPoint(iPos, true);
						}
						break;

					case apControlParam.TYPE.Float:
						{
							float fPos = keyValueSet._paramSet._conSyncValue_Float;
							newPoint = new apCalculatedLerpPoint(fPos, true);
						}
						break;

					case apControlParam.TYPE.Vector2:
						{
							Vector2 vPos = keyValueSet._paramSet._conSyncValue_Vector2;
							newPoint = new apCalculatedLerpPoint(vPos, true);

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
					_cpLerpPoints.Sort(delegate (apCalculatedLerpPoint a, apCalculatedLerpPoint b)
					{
						return a._iPos - b._iPos;
					});
					break;

				case apControlParam.TYPE.Float:
					_cpLerpPoints.Sort(delegate (apCalculatedLerpPoint a, apCalculatedLerpPoint b)
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

				apCalculatedLerpPoint pointLT = null;
				apCalculatedLerpPoint pointRT = null;
				apCalculatedLerpPoint pointLB = null;
				apCalculatedLerpPoint pointRB = null;

				//4) 좌표 순회하면서 RectArea 만들기
				for (int iX = 0; iX < fPosXList.Count - 1; iX++)
				{
					for (int iY = 0; iY < fPosYList.Count - 1; iY++)
					{
						pointLT = GetLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY]), bias);
						pointRT = GetLerpPoint(new Vector2(fPosXList[iX + 1], fPosYList[iY]), bias);
						pointLB = GetLerpPoint(new Vector2(fPosXList[iX], fPosYList[iY + 1]), bias);
						pointRB = GetLerpPoint(new Vector2(fPosXList[iX + 1], fPosYList[iY + 1]), bias);

						apCalculatedLerpArea lerpArea = new apCalculatedLerpArea(pointLT, pointRT, pointLB, pointRB);

						_cpLerpAreas.Add(lerpArea);
					}
				}

			}
		}
		private apCalculatedLerpPoint GetLerpPoint(int iPos)
		{
			return _cpLerpPoints.Find(delegate (apCalculatedLerpPoint a)
			{
				return a._iPos == iPos;
			});
		}

		private apCalculatedLerpPoint GetLerpPoint(float fPos, float bias)
		{
			return _cpLerpPoints.Find(delegate (apCalculatedLerpPoint a)
			{
				return Mathf.Abs(a._pos.x - fPos) < bias;
			});
		}

		private apCalculatedLerpPoint GetLerpPoint(Vector2 vPos, float bias)
		{
			return _cpLerpPoints.Find(delegate (apCalculatedLerpPoint a)
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
		private apCalculatedLerpPoint MakeVirtualLerpPoint(Vector2 pos, float bias)
		{
			apCalculatedLerpPoint existLerpPoint = GetLerpPoint(pos, bias);
			if (existLerpPoint != null)
			{
				return existLerpPoint;
			}
			apCalculatedLerpPoint newPoint = new apCalculatedLerpPoint(pos, false);
			_cpLerpPoints.Add(newPoint);

			//실제 Control Param Key를 입력해야한다.


			List<apCalculatedLerpPoint> realLerpPoints = _cpLerpPoints.FindAll(delegate (apCalculatedLerpPoint a)
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

			apCalculatedLerpPoint lerpPoint = null;
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

			apCalculatedLerpPoint curPoint = null;
			apCalculatedLerpPoint nextpoint = null;

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