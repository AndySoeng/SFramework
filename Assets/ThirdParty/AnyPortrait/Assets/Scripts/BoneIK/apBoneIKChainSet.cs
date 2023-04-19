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
	/// IK가 활성된 Bone이 가지는 IK 처리 클래스
	/// BoneIKChainUnit을 리스트로 가지고 있다.
	/// IK 요청시 현재의 World Matrix를 복사하여 시뮬레이션한 뒤, 적절한 값(Delta Angle)을 알려준다.
	/// IKTail인 Bone에만 등록된다.
	/// </summary>
	public class apBoneIKChainSet
	{
		// Members
		//------------------------------------------------------------
		public apBone _bone = null;

		/// <summary>
		/// Tail -> Head로 이어지는 ChainUnit
		/// IK 특성상 이 ChainSet을 소유하고 있는 Bone이 베이스가 되는 ChainUnit은 없다.
		/// </summary>
		public List<apBoneIKChainUnit> _chainUnits = new List<apBoneIKChainUnit>();

		private apBoneIKChainUnit _headChainUnit = null;
		private apBoneIKChainUnit _tailChainUnit = null;

		public Vector2 _requestedTargetPosW = Vector2.zero;
		public Vector2 _requestedBonePosW = Vector2.zero;


		/// <summary>
		/// 계산을 반복하는 Loop의 최대 횟수
		/// </summary>
		public const int MAX_CALCULATE_LOOP_EDITOR = 30;
		//public const int MAX_CALCULATE_LOOP_EDITOR = 10;
		public const int MAX_CALCULATE_LOOP_RUNTIME = 20;
		public const int MAX_TOTAL_UNIT_CALCULATE = 100;

		public const float CONTINUOUS_TARGET_POS_BIAS = 5.0f;
		public const float CONTINUOUS_ANGLE_JUMP_LIMIT = 30.0f;

		/// <summary>
		/// 연산이 종료되는 거리값 오차 허용값
		/// </summary>
		public const float BIAS_TARGET_POS_MATCH = 0.1f * 0.1f;//sqr Distance 값이다.
		


		private int _nLoop = 0;

		private Vector2 _prevTargetPosW = Vector2.zero;
		private bool _isContinuousPrevPos = false;

		public Vector2 _tailBoneNextPosW = Vector2.zero;

		// Init
		//------------------------------------------------------------
		public apBoneIKChainSet(apBone bone)
		{
			_bone = bone;
			_nLoop = 0;
		}

		// Functions
		//------------------------------------------------------------
		/// <summary>
		/// Bone Hierarchy에 맞추어서 다시 Chain을 만든다.
		/// </summary>
		public void RefreshChain()
		{
			_chainUnits.Clear();
			_headChainUnit = null;
			_tailChainUnit = null;

			if (!_bone._isIKTail)
			{
				return;
			}


			//Bone으로부터 Head가 나올때까지 Chain을 추가하자
			//[Parent] .... [Cur Bone] ----> [Target Bone < 이것 부터 시작]
			apBone parentBone = null;
			apBone curBone = null;
			apBone targetBone = _bone;

			//int curLevel = 0;
			while (true)
			{
				curBone = targetBone._parentBone;
				if (curBone == null)
				{
					//? 왜 여기서 끊기지..
					break;
				}

				parentBone = curBone._parentBone;//<<이건 Null 일 수 있다.

				//apBoneIKChainUnit newUnit = new apBoneIKChainUnit(curBone, targetBone, parentBone, curLevel);
				apBoneIKChainUnit newUnit = new apBoneIKChainUnit(curBone, targetBone, parentBone);

				_chainUnits.Add(newUnit);

				//끝났당
				if (curBone == _bone._IKHeaderBone)
				{
					break;
				}

				//하나씩 위로 탐색하자
				targetBone = curBone;
				//curLevel++;
			}
			if (_chainUnits.Count == 0)
			{
				return;
			}
			
			//앞쪽이 Tail이다.
			_tailChainUnit = _chainUnits[0];
			_headChainUnit = _chainUnits[_chainUnits.Count - 1];

			//Chain Unit간의 연결을 한다.
			apBoneIKChainUnit curUnit = null;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				curUnit = _chainUnits[i];

				if (i > 0)
				{
					curUnit.SetChild(_chainUnits[i - 1]);
				}

				if (i < _chainUnits.Count - 1)
				{
					curUnit.SetParent(_chainUnits[i + 1]);
				}

				curUnit.SetTail(_tailChainUnit);
			}

			if (_chainUnits.Count == 0)
			{
				_nLoop = 0;
			}
			else
			{
				// 얼마나 연산을 반복할 것인지 결정 (연산 횟수는 루프 단위로 결정한다)
				_nLoop = MAX_CALCULATE_LOOP_EDITOR;
				if (_chainUnits.Count * _nLoop > MAX_TOTAL_UNIT_CALCULATE)
				{
					//전체 계산 횟수 (Unit * Loop)가 제한을 넘겼을 때
					_nLoop = MAX_TOTAL_UNIT_CALCULATE / _chainUnits.Count;
					if (_nLoop < 2)
					{
						_nLoop = 2;
					}
				}
			}

			_isContinuousPrevPos = false;
		}



		/// <summary>
		/// IK를 시뮬레이션한다.
		/// 요청한 Bone을 Tail로 하여 Head까지 처리한다.
		/// 결과값은 Delta Angle로 나오며, 이 값을 참조하여 결정한다. (Matrix 중 어디에 쓸지는 외부에서 결정)
		/// </summary>
		/// <param name="targetPosW"></param>
		public bool SimulateIK(Vector2 targetPosW, bool isContinuous, bool isUseIKMatrix = false)
		{
			if (!_bone._isIKTail)
			{
				//Debug.Log("IK Failed : Not Tail");
				return false;
			}

			if (_chainUnits.Count == 0)
			{
				//Debug.Log("IK Failed : Chain Count 0");
				return false;
			}

			apBoneIKChainUnit chainUnit = null;

			//[Tail] .....[] .... [Head]
			//Tail에 가까운(인덱스가 가장 작은) Constraint가 적용된 Bone을 구한다.
			//Head에 가까운(인덱스가 가장 큰) Constraint가 적용된 Bone을 구한다.

			float lengthTotal = 0.0f;

			//1. Simulate 준비
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit.ReadyToSimulate(isUseIKMatrix);

				lengthTotal += chainUnit._lengthBoneToTarget;
			}


			// 전체 Loop 연산 순서
			// 1. 바로 CCD를 결정할 것인지, 아니면 각을 비튼 후에 CCD를 시작할 것인지 결정
			//    체크 방법은, 
			//    - Head Pos -> Target Pos의 Dir를 구하고
			//    - Request IK가 Dir 내부에 위치하는지(길이와 각도 범위) 계산

			// 1-2. 만약 Dir 내부에 Request IK가 위치한다면)
			//    - Head 를 기준으로
			//      "Head의 + Angle (90 또는 Upper)과 나머지 Unit의 - Anlge (-90 또는 Lower)합이 비슷한지, 또는 그 반대인지 결정
			//      (이건 Ready To Simulate에서 미리 연산한다)
			//      (그외의 - 각도) >= (Head 각도) * 0.5 이면 된다.
			//    - 둘다 가능하면) 현재 Head의 Local 각도가 +인지 -인지에 따라서 미리 상한/하한치까지 구부리고 시작
			//    - 가능한게 하나라면) 그 각도로 비틀고 시작하자
			//    - 가능한게 없다면) (그외의 - 각도)

			// 2. (계산된) Target Pos W의 위치가 Request IK Pos와 오차 범위 내에 위치한다 -> 종료
			// 2. Loop 연산 횟수가 끝났다 -> 종료

			// 3. Tail -> Head 순서로 Loop를 진행한다.
			// 4. 마지막으로 Tail에서 한번 더 수행하고 끝
			// 5. 2)로 돌아가서 더 처리할지 체크하고 반복한다.

			//1. CCD 전에 각도를 왜곡해야하는지 판별
			//수정) 이 작업은 각 마디에서 해야한다.

			//다시 수정
			//(Unit이 2개 이상인 경우)
			//"현재 Bone 좌표를 기준"으로
			//Head -> Target 길이가 Head -> Tail 길이보다 짧은 경우
			//CCD는 내부로 들어오지 못하여 에러가 발생한다.
			//압축 처리를 해야한다.
			//Angle Constraint를 이용하자
			//압축된 거리 비례를 dLenRatio라고 할때 (30% 줄어들면 0.7)

			//Constraint가 없을때)
			//압축되어야 하는 각도는 Cos(X) = 0.7
			//X = ACos(0.7)이다.
			//Head와 Tail은 X에서 X의 0.8~1.5 이내의 값을 가진다.
			//그 외의 유닛은 제한을 두지 않는다. (초기값만 줄 뿐)
			//Head와 Tail의 지정 각도는 X와 -X (나머지는 0으로 지정한다.

			//Constraint가 있을 때)
			//압축 권장 각도는 X = ACos(0.7)
			//Pref가 지정된 Bone에서는 Pref가 +일때, -일때를 구분한다.
			//지정 각도는 Pref이며, X가 포함되도록 영역 크기를 잡는다.
			//지정 각도는 Pref이다.
			//Head와 Tail의 지정각도는 X이며, +인지, -인지는 가장 가까운 Pref Bone의 값을 이용한다.
			//주의) Pref가 0인 경우는 Limit 중에서 더 넓은 쪽의 부호값을 사용하며, 이때는 X를 초기값으로 한다.

			//초기값으로 회전한 후에, 동적 Angle Constraint를 적용한다.

			//1. 길이 확인 후 압축을 해야하는지 적용
			float length2Target = (targetPosW - _headChainUnit._bonePosW).magnitude;

			float length2Tail = (_tailChainUnit._targetPosW - _headChainUnit._bonePosW).magnitude;
			if (length2Tail == 0.0f)
			{
				//Debug.Log("IK Failed : Length 0");
				return false;
			}

			float beforSqrDist = (targetPosW - _tailChainUnit._bonePosW).sqrMagnitude;

			apBoneIKChainUnit curBoneUnit = null;

			if (length2Target < lengthTotal)
			{
				//압축을 해야한다.
				//float compressRatio = Mathf.Clamp01(length2Target / lengthTotal);//<<이전에 사용하던 변수
				
				//알고리즘 다시 수정
				//"직선 내에 있으면 탄력적으로 Pref 방향으로 움직인다라는 것으로 변경
				//Pref 값과 그 방향을 지정해준다.
				//기본적으로 +compressAngle 값을 지정
				//_tailChainUnit이 있다면 처음엔 반대, 그 이후엔 Constraint의 부호를 따른다.
				//bool curPlusX = true;
				//if(_tailChainUnit != null)
				//{
				//	curPlusX = !_tailChainUnit._isAngleDir_Plus;
				//}

				//Vector2 dirHeadToTarget = targetPosW - _headChainUnit._bonePosW;
				for (int i = 0; i < _chainUnits.Count; i++)
				{
					curBoneUnit = _chainUnits[i];
					//if(curBoneUnit._isAngleContraint && !curBoneUnit._isPreferredAngleAdapted)
					if (curBoneUnit._isAngleContraint)
					{

						//curBoneUnit._angleLocal_Next = curBoneUnit._angleDir_Preferred * (1.0f - compressRatio) + curBoneUnit._angleLocal_Next + compressRatio;//구형

						//이전
						//curBoneUnit._angleLocal_Next = curBoneUnit._angleDir_Preferred;//신형

						//변경 20.10.9 : _angleDir_Preferred이 _angleLocal_Next와 360가까이 차이나는걸 막기 위함
						curBoneUnit._angleLocal_Next = apUtil.GetNearestLoopedAngle360(curBoneUnit._angleDir_Preferred, curBoneUnit._angleLocal_Next);

						//Preferred를 적용했다는 것을 알려주자
						curBoneUnit._isPreferredAngleAdapted = true;
					}					
				}

				_headChainUnit.CalculateWorldRecursive();
			}
			else if (length2Target > lengthTotal + 1.0f)//Bias 추가해서 플래그 리셋
			{
				for (int i = 0; i < _chainUnits.Count; i++)
				{
					_chainUnits[i]._isPreferredAngleAdapted = false;
				}
			}

			curBoneUnit = null;
			int nCalculate = 1;
			//float weight = 0.0f;
			int curIndex = 0;
			
			for (int i = 0; i < _nLoop; i++)
			{
				// 2. (계산된) Target Pos W의 위치가 Request IK Pos와 오차 범위 내에 위치한다 -> 종료
				// 2. Loop 연산 횟수가 끝났다 -> 종료

				// 3. Tail -> Head 순서로 Loop를 진행한다.
				// 4. 마지막으로 Tail에서 한번 더 수행하고 끝
				// 5. 2)로 돌아가서 더 처리할지 체크하고 반복한다.
				//if ((_tailChainUnit._targetPosW - targetPosW).sqrMagnitude < BIAS_TARGET_POS_MATCH)
				//{
				//	break;
				//}

				curBoneUnit = _tailChainUnit;

				//totalDeltaAngle = 0.0f;
				while (true)
				{
					//루프를 돕시다.
					//curBoneUnit.RequestIK(targetPosW, i, _nLoop);
					if(i == 0)
					{
						//가중치가 포함된 것으로 돈다.
						curBoneUnit.RequestIK_Weighted(targetPosW, isContinuous, Mathf.Clamp01((float)(curIndex + 1) / (float)_chainUnits.Count) * 0.7f);
						//curBoneUnit.RequestIK(targetPosW, isContinuous);//구형
						curIndex++;
					}
					else
					{
						curBoneUnit.RequestIK(targetPosW, isContinuous);
					}
					

					curBoneUnit.CalculateWorldRecursive();

					//매번 위치체크
					//if ((_tailChainUnit._targetPosW - targetPosW).sqrMagnitude < BIAS_TARGET_POS_MATCH)
					//{
					//	Debug.Log("IK가 끝남 : " +i + " / " + _nLoop);
					//	break;
					//}

					if (curBoneUnit._parentChainUnit != null)
					{
						curBoneUnit = curBoneUnit._parentChainUnit;
					}
					else
					{
						break;
					}
				}

				//마지막으로 Tail에서 처리 한번더
				curBoneUnit = _tailChainUnit;
				//curBoneUnit.RequestIK(targetPosW, i, _nLoop);
				curBoneUnit.RequestIK(targetPosW, isContinuous);
				curBoneUnit.CalculateWorldRecursive();

				nCalculate++;
			}


			//만약 Continuous 모드에서 각도가 너무 많이 차이가 나면 실패한 처리다.
			//이전 요청 좌표와 거리가 적은 경우 유효

			if (isContinuous)
			{
				if (_isContinuousPrevPos)
				{
					float distTargetDelta = Vector2.Distance(_prevTargetPosW, targetPosW);
					if (distTargetDelta < CONTINUOUS_TARGET_POS_BIAS)
					{

						//연속된 위치 입력인 경우
						//전체의 각도 크기를 구하자
						float totalDeltaAngle = 0.0f;
						for (int i = 0; i < _chainUnits.Count; i++)
						{
							totalDeltaAngle += Mathf.Abs(_chainUnits[i]._angleLocal_Delta);
						}
						//Debug.Log("Cont Move : " + distTargetDelta + " / Delta Angle : " + totalDeltaAngle);
						if (totalDeltaAngle > CONTINUOUS_ANGLE_JUMP_LIMIT)
						{
							//너무 많이 움직였다.
							_isContinuousPrevPos = true;
							_prevTargetPosW = targetPosW;
							//Debug.LogError("Angle Jump Error : Total Angle : " + totalDeltaAngle + " / Delta Target : " + distTargetDelta);
							//Debug.Log("IK Failed : 각도가 점프함");
							return false;
						}
					}
				}
				_isContinuousPrevPos = true;
				_prevTargetPosW = targetPosW;
			}
			else
			{
				_isContinuousPrevPos = false;
			}

			if (isContinuous && length2Target < lengthTotal)
			{
				float afterSqrdist = (_tailChainUnit._targetPosW - targetPosW).sqrMagnitude;
				if (beforSqrDist * 1.2f < afterSqrdist)
				{
					//오히려 더 멀어졌다.
					return false;
				}
			}

			_requestedTargetPosW = _tailChainUnit._targetPosW;
			_requestedBonePosW = _tailChainUnit._bonePosW;

			return true;
		}




		/// <summary>
		/// IK를 시뮬레이션한다.
		/// 요청한 Bone을 Tail로 하여 Head까지 처리해야하나, SimulateIK()와 달리, lastCheckUnitf를 실질적인 Head로 보고 처리한다.
		/// ChainUnit보다 짧은 제한적인 요청시 호출되는 함수.
		/// lastCheckUnit이 없거나 Chain 내에 없으면 SimulateIK와 동일한 결과를 만든다.
		/// 결과값은 Delta Angle로 나오며, 이 값을 참조하여 결정한다. (Matrix 중 어디에 쓸지는 외부에서 결정)
		/// </summary>
		/// <param name="targetPosW"></param>
		public bool SimulateIK_Limited(Vector2 targetPosW, bool isContinuous, apBoneIKChainUnit lastCheckUnit)
		{

			if (!_bone._isIKTail)
			{
				return false;
			}

			if (_chainUnits.Count == 0)
			{
				return false;
			}
			if (!_chainUnits.Contains(lastCheckUnit))
			{
				return SimulateIK(targetPosW, isContinuous);//<<입력값인 targetPosW가 IK 좌표계의 값이다.
			}

			//주석을 대부분 날릴테니 SimulateIK 함수를 참고하자

			apBoneIKChainUnit chainUnit = null;

			//[Tail] .....[] .... [Head]


			float lengthTotal = 0.0f;

			int iLastUnit = -1;
			//1. Simulate 준비
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit.ReadyToSimulate();

				if (iLastUnit < 0)
				{
					//아직 제한 범위가 검색 되기 전이라면 계산에 포함
					lengthTotal += chainUnit._lengthBoneToTarget;
				}

				//여기까지만 처리하도록 하자
				if (chainUnit == lastCheckUnit)
				{
					iLastUnit = i;
				}
			}

			int nLimitedChain = iLastUnit + 1;

			if (nLimitedChain == 0)
			{
				return false;
			}

			//<Head 대신 LastChainUnit을 사용한다>

			//전체 루프 횟수도 변경 (기존에는 _nLoop)
			int nLimitedLoop = MAX_CALCULATE_LOOP_EDITOR;

			if (nLimitedChain * nLimitedLoop > MAX_TOTAL_UNIT_CALCULATE)
			{
				//전체 계산 횟수 (Unit * Loop)가 제한을 넘겼을 때
				nLimitedLoop = MAX_TOTAL_UNIT_CALCULATE / nLimitedChain;
				if (nLimitedLoop < 2)
				{
					nLimitedLoop = 2;
				}
			}

			//1. 길이 확인 후 압축을 해야하는지 적용
			float length2Target = (targetPosW - lastCheckUnit._bonePosW).magnitude;
			float length2Tail = (_tailChainUnit._targetPosW - lastCheckUnit._bonePosW).magnitude;

			if (length2Tail == 0.0f)
			{
				return false;
			}

			float beforSqrDist = (targetPosW - _tailChainUnit._bonePosW).sqrMagnitude;

			apBoneIKChainUnit curBoneUnit = null;

			if (length2Target < lengthTotal)
			{
				//압축을 해야한다.
				float compressRatio = Mathf.Clamp01(length2Target / lengthTotal);
				
				for (int i = 0; i < nLimitedChain; i++)//<<전체 개수가 아닌 제한적인 개수만 돌린다.
				{
					curBoneUnit = _chainUnits[i];
					
					if (curBoneUnit._isAngleContraint)
					{
						//이전
						//curBoneUnit._angleLocal_Next = curBoneUnit._angleDir_Preferred * (1.0f - compressRatio) + curBoneUnit._angleLocal_Next + compressRatio;

						//변경 20.10.9 : _angleDir_Preferred의 값이 _angleLocal_Next와 360도 가까이 차이나는걸 막기 위함
						curBoneUnit._angleLocal_Next = 
							apUtil.GetNearestLoopedAngle360(curBoneUnit._angleDir_Preferred, curBoneUnit._angleLocal_Next) * (1.0f - compressRatio) 
							+ curBoneUnit._angleLocal_Next + compressRatio;

						//Preferred를 적용했다는 것을 알려주자
						curBoneUnit._isPreferredAngleAdapted = true;
					}
				}

				lastCheckUnit.CalculateWorldRecursive();
			}
			else if (length2Target > lengthTotal + 1.0f)//Bias 추가해서 플래그 리셋
			{
				for (int i = 0; i < nLimitedChain; i++)//제한된 개수만 돌린다.
				{
					_chainUnits[i]._isPreferredAngleAdapted = false;
				}
			}

			curBoneUnit = null;
			int nCalculate = 1;
			//for (int i = 0; i < _nLoop; i++)
			for (int i = 0; i < nLimitedLoop; i++)//_nLoop대신 제한된 Loop횟수로 변경
			{
				curBoneUnit = _tailChainUnit;

				//totalDeltaAngle = 0.0f;
				while (true)
				{
					//루프를 돕시다.
					curBoneUnit.RequestIK(targetPosW, isContinuous);

					curBoneUnit.CalculateWorldRecursive();

					//현재 Bone이 Last였으면 break;
					if (curBoneUnit == lastCheckUnit)
					{
						break;
					}

					if (curBoneUnit._parentChainUnit != null)
					{
						curBoneUnit = curBoneUnit._parentChainUnit;
					}
					else
					{
						break;
					}
				}

				//마지막으로 Tail에서 처리 한번더
				curBoneUnit = _tailChainUnit;
				
				curBoneUnit.RequestIK(targetPosW, isContinuous);
				curBoneUnit.CalculateWorldRecursive();

				nCalculate++;
			}


			//만약 Continuous 모드에서 각도가 너무 많이 차이가 나면 실패한 처리다.
			//이전 요청 좌표와 거리가 적은 경우 유효

			if (isContinuous)
			{
				if (_isContinuousPrevPos)
				{
					float distTargetDelta = Vector2.Distance(_prevTargetPosW, targetPosW);
					if (distTargetDelta < CONTINUOUS_TARGET_POS_BIAS)
					{
						//연속된 위치 입력인 경우
						//전체의 각도 크기를 구하자
						float totalDeltaAngle = 0.0f;
						for (int i = 0; i < _chainUnits.Count; i++)
						{
							totalDeltaAngle += Mathf.Abs(_chainUnits[i]._angleLocal_Delta);
						}
						
						if (totalDeltaAngle > CONTINUOUS_ANGLE_JUMP_LIMIT)
						{
							//너무 많이 움직였다.
							_isContinuousPrevPos = true;
							_prevTargetPosW = targetPosW;
							
							return false;
						}
					}
				}
				_isContinuousPrevPos = true;
				_prevTargetPosW = targetPosW;
			}
			else
			{
				_isContinuousPrevPos = false;
			}

			if (isContinuous && length2Target < lengthTotal)
			{
				float afterSqrdist = (_tailChainUnit._targetPosW - targetPosW).sqrMagnitude;
				if (beforSqrDist * 1.2f < afterSqrdist)
				{
					//오히려 더 멀어졌다.
					//Debug.LogError("다시 멀어졌다");
					return false;
				}
			}

			_requestedTargetPosW = _tailChainUnit._targetPosW;
			_requestedBonePosW = _tailChainUnit._bonePosW;

			return true;
		}

		/// <summary>
		/// IK 결과값을 일단 각 Bone에게 넣어준다.
		/// 적용된 값이 아니라 변수로 저장하는 것이므로, 
		/// 각 Bone에 있는 _IKRequestAngleResult를 참조하자
		/// </summary>
		/// <param name="weight"></param>
		public void AdaptIKResultToBones(float weight)
		{
			apBoneIKChainUnit chainUnit = null;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit._baseBone.AddIKAngle((chainUnit._angleWorld_Next - chainUnit._angleWorld_Prev) * weight);
			}
		}


		/// <summary>
		/// AdaptIKResultToBones()의 IKController 버전
		/// </summary>
		/// <param name="weight"></param>
		public void AdaptIKResultToBones_ByController(float weight)
		{
			apBoneIKChainUnit chainUnit = null;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				chainUnit._baseBone.AddIKAngle_Controlled(chainUnit._angleWorld_Next - chainUnit._angleWorld_Prev, weight);
			}
		}



		
		/// <summary>
		/// LookAt IK를 시뮬레이션한다.
		/// 요청한 Bone을 Tail로 하여 Head까지 처리한다.
		/// 결과값은 Delta Angle로 나오며, 이 값을 참조하여 결정한다. (Matrix 중 어디에 쓸지는 외부에서 결정)
		/// </summary>
		/// <param name="targetPosW"></param>
		public bool SimulateLookAtIK(Vector2 defaultLookAtPosW, Vector2 lookAtPosW, bool isContinuous, bool isUseIKMatrix = false)
		{

			//기본적인 계산은 SimulateIK를 이용한다.
			//여기서는 SimulateIK 전후로 추가적인 작업을 한다.
			//계산 전)
			//- 각각의 ChainUnit을 기준으로 Look At Dir를 계산하여 "평균적으로"적절한 targetPosW를 계산한다.
			//계산 후)
			//-이 본의 예상 위치를 계산하여 마지막으로 바라보는 각도를 계산한다. (적용은 나중에)
			if (!_bone._isIKTail)
			{
				return false;
			}

			if (_chainUnits.Count == 0)
			{
				return false;
			}

			apBoneIKChainUnit chainUnit = null;

			Vector2 targetPosW = Vector2.zero;
			int nCalculated = 0;
			for (int i = 0; i < _chainUnits.Count; i++)
			{
				chainUnit = _chainUnits[i];
				if (chainUnit._baseBone == null)
				{
					continue;
				}

				//이전
				//Vector2 dirBone2LookAt = lookAtPosW - chainUnit._baseBone._worldMatrix._pos;
				//Vector2 dirBone2DefaultLookAt = defaultLookAtPosW - chainUnit._baseBone._worldMatrix._pos;//<<기본 위치 기준
				//float deltaAngle = apUtil.AngleTo180(apBoneIKChainUnit.Vector2Angle(dirBone2LookAt) - apBoneIKChainUnit.Vector2Angle(dirBone2DefaultLookAt));
				
				//Vector2 dirBone2TailBone = _tailChainUnit._targetBone._worldMatrix._pos - chainUnit._baseBone._worldMatrix._pos;
				//Vector2 dirBone2ExpectedTargetPos = apBoneIKChainUnit.RotateAngle(
				//											chainUnit._baseBone._worldMatrix._pos, 
				//											_tailChainUnit._targetBone._worldMatrix._pos, 
				//											apBoneIKChainUnit.Vector2Angle(dirBone2TailBone) + deltaAngle);

				//변경 20.8.17 : 래핑
				//Vector2 dirBone2LookAt = lookAtPosW - chainUnit._baseBone._worldMatrix.Pos;
				//Vector2 dirBone2DefaultLookAt = defaultLookAtPosW - chainUnit._baseBone._worldMatrix.Pos;//<<기본 위치 기준
				//float deltaAngle = apUtil.AngleTo180(apBoneIKChainUnit.Vector2Angle(dirBone2LookAt) - apBoneIKChainUnit.Vector2Angle(dirBone2DefaultLookAt));
				
				//Vector2 dirBone2TailBone = _tailChainUnit._targetBone._worldMatrix.Pos - chainUnit._baseBone._worldMatrix.Pos;
				//Vector2 dirBone2ExpectedTargetPos = apBoneIKChainUnit.RotateAngle(
				//											chainUnit._baseBone._worldMatrix.Pos, 
				//											_tailChainUnit._targetBone._worldMatrix.Pos, 
				//											apBoneIKChainUnit.Vector2Angle(dirBone2TailBone) + deltaAngle);

				//변경 20.8.26 : IKSpace의 값을 이용
				Vector2 dirBone2LookAt = lookAtPosW - chainUnit._baseBone._worldMatrix.Pos_IKSpace;
				Vector2 dirBone2DefaultLookAt = defaultLookAtPosW - chainUnit._baseBone._worldMatrix.Pos_IKSpace;//<<기본 위치 기준
				float deltaAngle = apUtil.AngleTo180(apBoneIKChainUnit.Vector2Angle(dirBone2LookAt) - apBoneIKChainUnit.Vector2Angle(dirBone2DefaultLookAt));
				
				Vector2 dirBone2TailBone = _tailChainUnit._targetBone._worldMatrix.Pos_IKSpace - chainUnit._baseBone._worldMatrix.Pos_IKSpace;
				Vector2 dirBone2ExpectedTargetPos = apBoneIKChainUnit.RotateAngle(
															chainUnit._baseBone._worldMatrix.Pos_IKSpace, 
															_tailChainUnit._targetBone._worldMatrix.Pos_IKSpace, 
															apBoneIKChainUnit.Vector2Angle(dirBone2TailBone) + deltaAngle);

				targetPosW += dirBone2ExpectedTargetPos;
				nCalculated++;
			}

			if(nCalculated == 0)
			{
				targetPosW = lookAtPosW;
			}
			else
			{
				targetPosW.x /= nCalculated;
				targetPosW.y /= nCalculated;
			}

			bool result = SimulateIK(targetPosW, isContinuous, isUseIKMatrix);//targetPosW > 이건 IKSpace로 계산된 것이다.
			if(!result)
			{
				return false;
			}

			_tailBoneNextPosW = _tailChainUnit._targetPosW;

			return true;
		}
	}

}