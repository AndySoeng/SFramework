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
	/// IK 계산시 입력을 받아서 계산을 수행할 때 사용되는 클래스
	/// Chaining 정보 입력 -> 각 관절의 각도 리턴을 해준다.
	/// 기본적으로 World좌표계로 계산이 되며,
	/// 얼마나 각도를 회전해야하는지 알려준다.
	/// 각 유닛은 Bone에 속하며, Tail->Head 방향으로 움직이며 계산한다.
	/// </summary>
	public class apBoneIKChainUnit
	{
		//Members
		//-------------------------------------------------
		public apBoneIKChainUnit _parentChainUnit = null;
		public apBoneIKChainUnit _childChainUnit = null;

		public apBoneIKChainUnit _tailChainUnit = null;


		public apBone _parentBone = null;//<Parent를 넣어야 IK
		public apBone _baseBone = null;
		public apBone _targetBone = null;

		//좌표계 값을 저장해야하는 것
		//(Parent) ------ [ (Cur Bone) ------------> ] (Target)
		//                World Pos <갱신됨>
		//                            Dir Length
		//                      () Local Angle <Prev, Next> - World Angle (갱신됨)

		//참조
		//Parent Pos / Dir / Angle (World Pos 갱신용 / Angle Constraint용)


		//Head인 경우)
		//[(Parent) ------------------- (Cur Bone) ------------> ] (Target)
		// Parent World Pos(연산 시작점)
		//     Parent Dir Vector (World)

		//Tail인 경우)
		//(Parent) ------ [ (Cur Bone) ------------> (Target) ]
		//                                       Target World Pos (계산하여 갱신한다)

		//연산 순서

		// Loop 연산 순서)
		// 1. (계산된) Target Pos W를 기준으로 
		//    [Cur Bone -> Target Angle]에서 [Cur Bone -> Request IK Angle]으로 변하는 Delta Angle을 구한다.
		//    Delta Angle을 현재의 Angle Next에 더한다.
		//    Angle Constraint가 적용되고 있는 중이라면) 
		//    -> Local Angle Next의 범위를 Lower / Upper로 제한한다.
		//    -> Loop 1회차에서는 "계산된 Angle"과 "Preferred Angle"의 중간값을 사용한다.

		// 2. Local Angle (Next)가 갱신되었으므로 "재귀적으로" Child Unit에게 World Matrix 갱신을 요청한다.
		//    (World Matrix 연산시에는 Local Angle + Parent World Angle => World Angle을 활용한다)
		//    마지막 Tail에서 연산을 마무리하면 Target Pos W가 완성된다.

		// 3. Parent로 이동하여 1.을 반복한다.





		//<현재 World Pos>
		//Bone들의 World 좌표
		//연산하면서 위치가 계속 바뀌지만, World Matrix로 반영하지 않고 별도로 계산하기 위해
		public Vector2 _bonePosW = Vector2.zero;

		//만약 Head라면 이 값을 사용해야한다.
		public Vector2 _parentPosW = Vector2.zero;
		public float _angleWorld_Parent = 0.0f;

		//만약 Tail이라면 완성된 World 값을 이용해서 Target Pos W를 구해야한다.
		public Vector2 _targetPosW = Vector2.zero;

		public float _lengthBoneToTarget = 0.0f;


		/// <summary>
		/// 계산 전의 벡터 각도(World). 
		/// 순수하게 Pos로만 계산된 각도이다.
		/// 계산 후에는 -90를 해서 Bone에 넣어줘야한다. (이 값은 -90 처리를 하지 않았다)
		/// </summary>
		public float _angleWorld_Prev = 0.0f;


		/// <summary>
		/// 계산 후의 벡터 각도(Parent 대비 상대값). 
		/// 순수하게 Pos로만 계산된 각도이다.
		/// 기본 연산에 의한 결과
		/// </summary>
		public float _angleLocal_Next = 0.0f;


		public float _angleWorld_Next = 0.0f;
		public float _angleLocal_Prev = 0.0f;//<<이건 계산용 값이다. 업데이트시에 매번 바뀜
		public float _angleLocal_Delta = 0.0f;//<<이건 계산용 값이다. 업데이트시에 매번 바뀜


		public bool _isAngleContraint = false;

		public float _angleParentToBase_Offset = 0.0f;
		public float _angleDir_Preferred = 0.0f;
		public float _angleDir_Lower = 0.0f;
		public float _angleDir_Upper = 0.0f;
		//public bool _isAngleDir_Plus = true;

		public bool _isPreferredAngleAdapted = false;


		// Functions
		//-------------------------------------------------
		public apBoneIKChainUnit(apBone baseBone, apBone targetBone, apBone parentBone)
		{
			_baseBone = baseBone;
			_targetBone = targetBone;
			_parentBone = parentBone;

			_parentChainUnit = null;
			_childChainUnit = null;

			_isPreferredAngleAdapted = false;
		}


		public void SetParent(apBoneIKChainUnit parentUnit)
		{
			_parentChainUnit = parentUnit;
		}

		public void SetChild(apBoneIKChainUnit childUnit)
		{
			_childChainUnit = childUnit;
		}

		public void SetTail(apBoneIKChainUnit tailChainUnit)
		{
			_tailChainUnit = tailChainUnit;
		}


		private float Lerp(float A, float B, float itp, float length)
		{
			itp = Mathf.Clamp(itp, 0.0f, length);
			return (A * (length - itp) + B * itp) / length;
		}

		// Update
		//-----------------------------------------------------------
		/// <summary>
		/// IK 시뮬레이션하기전에 호출하는 함수
		/// 위치값을 모두 넣어주고, baseBone의 설정을 복사한다.
		/// 이 함수를 호출한 후, Head에서 CalculateWorldRecursive를 호출하자
		/// </summary>
		public void ReadyToSimulate(bool isUseIKMatrix = false)
		{
			_isAngleContraint = _baseBone._isIKAngleRange;

			
			//현재의 Bone의 Pos World를 이용해서 Local 정보를 만들자

			//<<삭제 (20.8.8) 플립은 아래에서 따로 계산한다.
			//bool isXReverse = false;
			if (!isUseIKMatrix)
			{
				//IK가 적용된 WorldMatrix를 사용하지 않을 때

				//이전
				//_bonePosW = _baseBone._worldMatrix._pos;
				//_targetPosW = _targetBone._worldMatrix._pos;
				
				//변경 20.8.17 : 래핑
				//_bonePosW = _baseBone._worldMatrix.Pos;
				//_targetPosW = _targetBone._worldMatrix.Pos;

				//변경 20.8.25 : IK용 좌표계의 값을 이용하자
				_bonePosW = _baseBone._worldMatrix.Pos_IKSpace;
				_targetPosW = _targetBone._worldMatrix.Pos_IKSpace;

				//isXReverse = _baseBone._worldMatrix._scale.x < 0.0f;//삭제 20.8.8
			}
			else
			{
				//IK가 적용된 WorldMatrix를 사용할 때
				
				//이전
				//_bonePosW = _baseBone._worldMatrix_IK._pos;
				//_targetPosW = _targetBone._worldMatrix_IK._pos;

				//변경 20.8.17 : 래핑
				//_bonePosW = _baseBone._worldMatrix_IK.Pos;
				//_targetPosW = _targetBone._worldMatrix_IK.Pos;

				//변경 20.8.25 : IK용 좌표계의 값을 이용하자
				_bonePosW = _baseBone._worldMatrix_IK.Pos_IKSpace;
				_targetPosW = _targetBone._worldMatrix_IK.Pos_IKSpace;

				//isXReverse = _baseBone._worldMatrix_IK._scale.x < 0.0f;//삭제 20.8.8
			}

			if (_parentBone != null)
			{
				if (!isUseIKMatrix)
				{
					//_parentPosW = _parentBone._worldMatrix._pos;
					//_parentPosW = _parentBone._worldMatrix.Pos;//래핑 20.8.17
					_parentPosW = _parentBone._worldMatrix.Pos_IKSpace;//IK 좌표계 사용 20.8.25
				}
				else
				{
					//_parentPosW = _parentBone._worldMatrix_IK._pos;
					//_parentPosW = _parentBone._worldMatrix_IK.Pos;//래핑 20.8.17
					_parentPosW = _parentBone._worldMatrix_IK.Pos_IKSpace;//IK 좌표계 사용 20.8.25
				}


				//Angle : Parent -> Base
				_angleWorld_Parent = Vector2Angle(_bonePosW - _parentPosW);

				//Parent에서 Base로의 상대 각도가 Default와 같은지 체크
				//Offset만큼 위치가 맞지 않는다.

				if (!isUseIKMatrix)
				{
					//_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix._angleDeg) - 90);
					//_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix.Angle) - 90);//래핑 20.8.17
					_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix.Angle_IKSpace) - 90);//IK 좌표계 사용 20.8.25
				}
				else
				{
					//_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix_IK._angleDeg) - 90);
					_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix_IK.Angle_IKSpace) - 90);//IK 좌표계 사용 20.8.25
				}
			}
			else
			{
				//이전 : RootBone에서는 AngleConstraint가 지정되지 않는다.
				//_parentPosW = Vector2.zero;
				//_isAngleContraint = false;
				//_angleWorld_Parent = 0.0f;
				//_angleParentToBase_Offset = 0.0f;

				//>>>변경
				//RootBone에서도 AngleConstraint를 적용할 수 있다.
				_parentPosW = Vector2.zero;
				//_isAngleContraint = false;
				_angleWorld_Parent = 0.0f;
				_angleParentToBase_Offset = -90.0f;//<<실제 각도와 IK각도가 90만큼 차이가 있기 때문

				//TODO : 이거 우쨔... Def가 아닌 Skew에서는 IKSpace에서 이 값이 무시되어야 한다.
				//>> 이게 문제가 아닌가??
				if (_baseBone._renderUnit != null)
				{
					//RenderUnit과의 계산
					_angleWorld_Parent = _baseBone._renderUnit.WorldMatrixWrap._angleDeg;
				}
			}
			//이전
			//float defaultAngle180 = apUtil.AngleTo180(_baseBone._defaultMatrix._angleDeg - _angleParentToBase_Offset);

			//변경 20.8.8 [Flipped Scale 문제]
			float baseBoneDefaultAngle = _baseBone._defaultMatrix._angleDeg;

			//Flip을 체크하는 방식이 두가지가 있다.
			
			//래핑 20.8.17
			bool isFliped_X = false;
			bool isFliped_Y = false;

			if(_baseBone._rootBoneScaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				isFliped_X = _baseBone._worldMatrix.Scale.x < 0.0f;
				isFliped_Y = _baseBone._worldMatrix.Scale.y < 0.0f;
			}
			else
			{
				isFliped_X = _baseBone._worldMatrix._mtx_Skew._scale_Step1.x < 0.0f;
				isFliped_Y = _baseBone._worldMatrix._mtx_Skew._scale_Step1.y < 0.0f;
			}
			bool is1AxisFlipped = isFliped_X != isFliped_Y;//1개의 축만 뒤집힌 경우
			bool isReversedYFromParent = isFliped_Y;

			//스케일 조건이 Default일 때는 AngleConstraint 계산시 영역 반전을 해야한다.
			if (_baseBone._rootBoneScaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				//defaultAngle은 부모 본/부모 렌더 유닛의 스케일의 영향을 받는다. [20.10.6]
				//부모 본의 크기 반전과 부모 렌더 유닛의 크기 반전은 조건이 다르다.
				if (_baseBone._parentBone != null)
				{
					//Y
					if (_baseBone._parentBone._worldMatrix.Scale.y < 0.0f)
					{
						baseBoneDefaultAngle = 180.0f - baseBoneDefaultAngle;
						isReversedYFromParent = !isFliped_Y;
					}
					else
					{
						isReversedYFromParent = isFliped_Y;
					}
					
					//X
					if (_baseBone._parentBone._worldMatrix.Scale.x < 0.0f)
					{
						baseBoneDefaultAngle = -baseBoneDefaultAngle;
					}
				}
				else if (_baseBone._renderUnit != null)
				{
					//원래는 Scale되는 (반전 뿐만 아니라) 벡터를 이용해서 "찌그러진 벡터"까지 계산해야한다.
					//하지만 반전 여부만 확인
					//Y
					if (_baseBone._renderUnit.WorldMatrixWrap._scale.y < 0.0f)
					{
						baseBoneDefaultAngle = 180.0f - baseBoneDefaultAngle;
						isReversedYFromParent = !isFliped_Y;
					}
					else
					{
						isReversedYFromParent = isFliped_Y;
					}
					
					//X
					if (_baseBone._renderUnit.WorldMatrixWrap._scale.x < 0.0f)
					{
						baseBoneDefaultAngle = -baseBoneDefaultAngle;
					}
				}
			}
			else
			{
				if (_baseBone._parentBone != null)
				{
					//Y
					if (_baseBone._parentBone._worldMatrix._mtx_Skew._scale_Step1.y < 0.0f)
					{
						baseBoneDefaultAngle = 180.0f - baseBoneDefaultAngle;
						isReversedYFromParent = !isFliped_Y;
					}
					else
					{
						isReversedYFromParent = isFliped_Y;
					}
					//X
					if (_baseBone._parentBone._worldMatrix._mtx_Skew._scale_Step1.x < 0.0f)
					{
						baseBoneDefaultAngle = -baseBoneDefaultAngle;
					}
				}
			}

			float defaultAngle180 = apUtil.AngleTo180(baseBoneDefaultAngle - _angleParentToBase_Offset);

			float IKAngleRange_1 = 0.0f;
			float IKAngleRange_2 = 0.0f;

			//부모와 다른 Y 좌표계를 가지면 아예 기준 축을 뒤집어야 한다.
			if(isReversedYFromParent)
			{
				defaultAngle180 = apUtil.AngleTo180(180 + defaultAngle180);
			}

			//한개의 축만 뒤집힌 경우 범위가 변경된다.
			if(is1AxisFlipped)
			{
				//이 부분이 기존의 [X축이 뒤집힌 경우]에서 [1개의 축이 뒤집힌 경우]로 변경되었다. (20.8.8)
				_angleDir_Preferred = defaultAngle180 - _baseBone._IKAnglePreferred;
				IKAngleRange_1 = defaultAngle180 - _baseBone._IKAngleRange_Upper;
				IKAngleRange_2 = defaultAngle180 - _baseBone._IKAngleRange_Lower;
			}
			else
			{
				_angleDir_Preferred = defaultAngle180 + _baseBone._IKAnglePreferred;
				IKAngleRange_1 = defaultAngle180 + _baseBone._IKAngleRange_Lower;
				IKAngleRange_2 = defaultAngle180 + _baseBone._IKAngleRange_Upper;
			}

			_angleDir_Lower = Mathf.Min(IKAngleRange_1, IKAngleRange_2);
			_angleDir_Upper = Mathf.Max(IKAngleRange_1, IKAngleRange_2);


			_lengthBoneToTarget = Vector2.Distance(_targetPosW, _bonePosW);
			_angleWorld_Prev = Vector2Angle(_targetPosW - _bonePosW);
			_angleWorld_Next = _angleWorld_Prev;

			_angleLocal_Next = _angleWorld_Next - _angleWorld_Parent;

			if (isUseIKMatrix)
			{
				//주의
				//IK Matrix를 사용한다면, Prev값은 WorldMatrix(WorldMatrixIK가 아님)상에서의 벡터 각도를 사용해야한다.
				_angleWorld_Prev = Vector2Angle(_targetBone._worldMatrix.Pos_IKSpace - _baseBone._worldMatrix.Pos_IKSpace);//IK 좌표계 20.8.25
			}
		}


		/// <summary>
		/// 현재 호출한 Bone Unit을 시작으로 Tail 방향으로 World를 갱신한다.
		/// Parent의 PosW, AngleWorld가 갱신되었어야 한다.
		/// IK의 핵심이 되는 _angleLocal_Next가 계산된 상태여야 한다.
		/// </summary>
		public void CalculateWorldRecursive()
		{
			if (_parentChainUnit != null)
			{
				//Parent 기준으로 Pos를 갱신한다.
				_parentPosW = _parentChainUnit._bonePosW;
				_angleWorld_Parent = _parentChainUnit._angleWorld_Next;

				_bonePosW.x = _parentPosW.x + _parentChainUnit._lengthBoneToTarget * Mathf.Cos(_angleWorld_Parent * Mathf.Deg2Rad);
				_bonePosW.y = _parentPosW.y + _parentChainUnit._lengthBoneToTarget * Mathf.Sin(_angleWorld_Parent * Mathf.Deg2Rad);
			}

			//Local Angle에 따라 World Angle을 갱신한다.
			_angleWorld_Next = _angleLocal_Next + _angleWorld_Parent;

			//Child Unit도 같이 갱신해주자
			if (_childChainUnit != null)
			{
				_childChainUnit.CalculateWorldRecursive();
			}
			else
			{
				//엥 여기가 Tail인가염
				_targetPosW.x = _bonePosW.x + _lengthBoneToTarget * Mathf.Cos(_angleWorld_Next * Mathf.Deg2Rad);
				_targetPosW.y = _bonePosW.y + _lengthBoneToTarget * Mathf.Sin(_angleWorld_Next * Mathf.Deg2Rad);
			}
		}



		/// <summary>
		/// IK를 요청한다.
		/// </summary>
		/// <param name="requestIKPosW"></param>
		/// <param name="isContinuous"></param>
		/// <returns></returns>
		public void RequestIK(Vector2 requestIKPosW, bool isContinuous
								//, int loopCount, int totalCount
								)
		{
			
			_angleLocal_Prev = _angleLocal_Next;

			//회전해야하는 World 각도
			float angleIK_Bone2IKtarget = Vector2Angle(requestIKPosW - _bonePosW);
			float angleIK_Bone2Tail = Vector2Angle(_tailChainUnit._targetPosW - _bonePosW);

			//현재 각도에서 빼자
			float angleIK_Delta = apUtil.AngleTo180(angleIK_Bone2IKtarget - angleIK_Bone2Tail);

			
			//Local에 더해주자
			_angleLocal_Next = apUtil.AngleTo360(_angleLocal_Next + angleIK_Delta);

			//Angle Constraint에 걸리나
			if (_isAngleContraint)
			{
				//기존
				//if (_angleLocal_Next < _angleDir_Lower)
				//{
				//	_angleLocal_Next = _angleDir_Lower;
				//}
				//else if (_angleLocal_Next > _angleDir_Upper)
				//{
				//	_angleLocal_Next = _angleDir_Upper;
				//}

				//변경 20.10.9 : Clamp시 각도 보정을 해준다.
				_angleLocal_Next = apUtil.AngleClamp360(_angleLocal_Next, _angleDir_Lower, _angleDir_Upper);
			}

			//연속적인 처리에서 너무 차이가 크다면 IK가 점프해버리는 문제가 있다.
			//if(isContinuous)
			//{	
			//	if(Mathf.Abs(_angleLocal_Prev - _angleLocal_Next) > 90.0f)
			//	{
			//		_angleLocal_Next = _angleLocal_Prev;
			//	}
			//}

			//이후에 Calculate를 외부에서 호출해주자
			_angleLocal_Delta = _angleLocal_Next - _angleLocal_Prev;
		}


		public void RequestIK_Weighted(Vector2 requestIKPosW, bool isContinuous, float weight)
		{
			
			_angleLocal_Prev = _angleLocal_Next;

			//회전해야하는 World 각도
			float angleIK_Bone2IKtarget = Vector2Angle(requestIKPosW - _bonePosW);
			float angleIK_Bone2Tail = Vector2Angle(_tailChainUnit._targetPosW - _bonePosW);

			//현재 각도에서 빼자
			float angleIK_Delta = apUtil.AngleTo180(angleIK_Bone2IKtarget - angleIK_Bone2Tail);//<<<이부분이 수정되었다.
			angleIK_Delta *= weight;//<<추가 : 가중치를 두어서 제한을 한다.

			
			//Local에 더해주자
			_angleLocal_Next = apUtil.AngleTo360(_angleLocal_Next + angleIK_Delta);

			//Angle Constraint에 걸리나
			if (_isAngleContraint)
			{
				//이전
				//if (_angleLocal_Next < _angleDir_Lower)
				//{
				//	_angleLocal_Next = _angleDir_Lower;
				//}
				//else if (_angleLocal_Next > _angleDir_Upper)
				//{
				//	_angleLocal_Next = _angleDir_Upper;
				//}

				//변경 20.10.9 : Clamp시 각도 보정을 해준다.
				_angleLocal_Next = apUtil.AngleClamp360(_angleLocal_Next, _angleDir_Lower, _angleDir_Upper);
			}

			//연속적인 처리에서 너무 차이가 크다면 IK가 점프해버리는 문제가 있다.
			//if(isContinuous)
			//{	
			//	if(Mathf.Abs(_angleLocal_Prev - _angleLocal_Next) > 90.0f)
			//	{
			//		_angleLocal_Next = _angleLocal_Prev;
			//	}
			//}

			//이후에 Calculate를 외부에서 호출해주자
			_angleLocal_Delta = _angleLocal_Next - _angleLocal_Prev;
		}


		// 계산 함수들
		//----------------------------------------------------------------------------------
		/// <summary>
		/// Dir Vector의 Angle (Degree)를 리턴한다.
		/// </summary>
		/// <param name="dirVec"></param>
		/// <returns></returns>
		public static float Vector2Angle(Vector2 dirVec)
		{
			return Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// 두개의 좌표계에서, Origin Pos를 기준으로 Target Pos를 회전하고, 그 위치를 리턴한다.
		/// 각도는 변환 뒤의 절대값이다. (Degree)
		/// </summary>
		/// <param name="originPos"></param>
		/// <param name="targetPos"></param>
		/// <param name="nextAngle"></param>
		/// <returns></returns>
		public static Vector2 RotateAngle(Vector2 originPos, Vector2 targetPos, float nextAngle)
		{
			float dist = Vector2.Distance(targetPos, originPos);

			return new Vector2(originPos.x + dist * Mathf.Cos(nextAngle * Mathf.Deg2Rad),
								originPos.y + dist * Mathf.Sin(nextAngle * Mathf.Deg2Rad)
								);
		}




	}

}