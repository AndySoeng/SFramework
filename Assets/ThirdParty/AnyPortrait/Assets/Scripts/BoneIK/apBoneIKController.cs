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
	/// 본에 IK Controller를 추가할 때 사용되는 데이터 클래스
	/// 이후에 IKControllerRequest에서 이 정보와 Runtime 정보를 받아서 연산을 한다.
	/// 데이터 저장 용도이며 apBone에 포함된다.
	/// 나중에 apOptBone에도 포함된다.
	/// </summary>
	[Serializable]
	public class apBoneIKController
	{
		// Members
		//---------------------------------------------------
		public enum CONTROLLER_TYPE
		{
			None = 0,
			Position = 1,
			LookAt = 2
		}
		[SerializeField]
		public CONTROLLER_TYPE _controllerType = CONTROLLER_TYPE.None;

		[NonSerialized, NonBackupField]
		public apBone _parentBone = null;

		[SerializeField]
		public int _effectorBoneID = -1;

		[NonSerialized, NonBackupField]
		public apBone _effectorBone = null;

		//StartBone은 LookAt타입일때만 적용
		//[SerializeField]
		//public int _startBoneID = -1;

		//[NonSerialized]
		//public apBone _startBone = null;


		[SerializeField]
		public float _defaultMixWeight = 0.0f;//>>>> Default는 0이다. IK를 항상 켜놓지는 말자

		//추가 : Control Param으로 Weight를 제어할 수 있다.
		[SerializeField]
		public bool _isWeightByControlParam = false;

		[SerializeField]
		public int _weightControlParamID = -1;

		[NonSerialized]
		public apControlParam _weightControlParam = null;

		#region [미사용 코드 : Bone Chain으로 대체한다]
		////[SerializeField]
		////public float _lookWeight = 1.0f;//<<StartBone의 Weight를 SubChainedBone에 넣을 것이다.

		////LookAt인 경우 중간의 Chained Bone에서의 SubWeight를 지정하여 조금씩 이동하게 만드는게 가능하다
		////Bone 추가/삭제 시에 갱신해야한다.
		////ChainedBone은 리스트에 무조건 들어간다.
		////Parent ~ TargetBone 사이의 Bone들이 들어간다.
		////IsEnable가 꺼져있거나 targetBone이 없으면 갱신되지 않는다.
		////SubWeight는 Normalize로 계산된다. (AutoNormalize가 켜져있다면)
		//[Serializable]
		//public class SubChainedBone
		//{
		//	[SerializeField]
		//	public int _subBoneID = -1;

		//	[NonSerialized]
		//	public apBone _subBone = null;

		//	[NonSerialized]
		//	public SubChainedBone _childChainedBone = null;

		//	[NonSerialized]
		//	public apBone _childBone = null;

		//	[NonSerialized]
		//	public SubChainedBone _parentChainedBone = null;

		//	[SerializeField]
		//	public float _subWeight = 0.0f;

		//	//계산용 변수
		//	[NonSerialized]
		//	public Vector2 _pos_Prev = Vector2.zero;

		//	[NonSerialized]
		//	public Vector2 _pos_Next = Vector2.zero;

		//	[NonSerialized]
		//	public float _angleWorld_Prev = 0.0f;

		//	[NonSerialized]
		//	public float _angleWorld_Next = 0.0f;

		//	[NonSerialized]
		//	public float _lengthToParent = 0.0f;

		//	[NonSerialized]
		//	public float _angleWorld_Max = 0.0f;

		//	[NonSerialized]
		//	public float _angleWorld_Min = 0.0f;




		//	public SubChainedBone()
		//	{

		//	}

		//	public void SetNewBone(apBone bone)
		//	{
		//		_subBoneID = bone._uniqueID;
		//		_subBone = bone;
		//		_subWeight = 0.0f;
		//	}

		//	public void CopyFromChainedBone(SubChainedBone srcChainedBone)
		//	{
		//		_subBoneID = srcChainedBone._subBoneID;
		//		_subBone = srcChainedBone._subBone;
		//		_subWeight = srcChainedBone._subWeight;
		//	}

		//	public void ReadyToUpdate()
		//	{
		//		_pos_Prev = _subBone._worldMatrix._pos;
		//		_pos_Next = _pos_Prev;

		//		if(_childBone == null)
		//		{
		//			_angleWorld_Prev = _subBone._worldMatrix._angleDeg;
		//		}
		//		else
		//		{
		//			_angleWorld_Prev = Vector2Angle(_childBone._worldMatrix._pos - _pos_Prev);
		//		}
		//		//_angleWorld_Prev = _subBone._worldMatrix._angleDeg;
		//		//수정

		//		_angleWorld_Next = _angleWorld_Prev;
		//		if(_subBone._isIKAngleRange && _subBone._parentBone != null)
		//		{
		//			//TODO : 이거 Default Angle 위주로 바꿔야함
		//			_angleWorld_Min = _subBone._parentBone._worldMatrix._angleDeg + _subBone._IKAngleRange_Lower;
		//			_angleWorld_Max = _subBone._parentBone._worldMatrix._angleDeg + _subBone._IKAngleRange_Upper;
		//		}
		//		else
		//		{
		//			_angleWorld_Min = _angleWorld_Prev;
		//			_angleWorld_Max = _angleWorld_Prev;
		//		}


		//		if(_subBone._parentBone != null)
		//		{
		//			_lengthToParent = Vector2.Distance(_subBone._parentBone._worldMatrix._pos, _pos_Prev);
		//		}
		//		else
		//		{
		//			_lengthToParent = 0.0f;
		//		}
		//	}

		//	public void CalculateWorldRecursive()
		//	{
		//		if(_parentChainedBone != null)
		//		{
		//			_pos_Next = _parentChainedBone._pos_Next + Angle2Vector2(_parentChainedBone._angleWorld_Next) * _lengthToParent;
		//			_angleWorld_Next = _angleWorld_Prev + apUtil.AngleTo180(_parentChainedBone._angleWorld_Next - _parentChainedBone._angleWorld_Prev);
		//		}
		//		if(_childChainedBone != null)
		//		{
		//			_childChainedBone.CalculateWorldRecursive();
		//		}

		//	}


		//}

		//[SerializeField]
		//public List<SubChainedBone> _subChainedBones = new List<SubChainedBone>();

		////Look At 계산용
		//[NonSerialized]
		//public Vector2 _lookAtNextPosW = Vector2.zero;

		//[NonSerialized]
		//public float _lookAtNextAngleW = 0.0f; 
		#endregion

		// Init
		//---------------------------------------------------
		public apBoneIKController()
		{
			_controllerType = CONTROLLER_TYPE.None;
		}

		
		public void Link(apBone parentBone, apMeshGroup meshGroup, apPortrait portrait)
		{
			_parentBone = parentBone;

			if(_effectorBoneID < 0)
			{
				_effectorBone = null;
			}
			else
			{
				_effectorBone = meshGroup.GetBone(_effectorBoneID);

				if(_effectorBone == null)
				{
					_effectorBoneID = -1;
				}
			}

			//추가 : Control Param으로 Weight를 제어할 수 있다.
			if(_isWeightByControlParam)
			{
				_weightControlParam = null;
				if(_weightControlParamID >= 0)
				{
					_weightControlParam = portrait.GetControlParam(_weightControlParamID);
				}
				if(_weightControlParam == null)
				{
					_weightControlParamID = -1;
				}
			}
			else
			{
				_weightControlParam = null;
			}

			#region [미사용 코드 : Bone Chain으로 대체한다]
			//if(_startBoneID < 0)
			//{
			//	_startBone = null;
			//}
			//else
			//{
			//	_startBone = meshGroup.GetBone(_startBoneID);
			//	if(_startBone == null)
			//	{
			//		_startBoneID = -1;
			//	}
			//}

			//if(_subChainedBones == null)
			//{
			//	_subChainedBones = new List<SubChainedBone>();
			//}
			//if(_effectorBone == null)
			//{
			//	_subChainedBones.Clear();
			//}

			////Child > Parent 순으로 순회한다.
			//apBone curChildBone = _parentBone;//<<현재 본이 가장 Child

			//for (int i = 0; i < _subChainedBones.Count; i++)
			//{
			//	_subChainedBones[i]._subBone = meshGroup.GetBone(_subChainedBones[i]._subBoneID);
			//	_subChainedBones[i]._childBone = curChildBone;
			//	if(i - 1 >= 0)
			//	{
			//		_subChainedBones[i]._childChainedBone = _subChainedBones[i - 1];
			//	}
			//	else
			//	{
			//		_subChainedBones[i]._childChainedBone = null;
			//	}
			//	if(i + 1 < _subChainedBones.Count)
			//	{
			//		_subChainedBones[i]._parentChainedBone = _subChainedBones[i + 1];
			//	}
			//	else
			//	{
			//		_subChainedBones[i]._parentChainedBone = null;
			//	}
			//	curChildBone = _subChainedBones[i]._subBone;
			//} 
			#endregion


			//_childChainBone
		}

		// Functions
		//---------------------------------------------------
		/// <summary>
		/// 선택된 Bone들이 유효한 것들인지 체크한다.
		/// RefreshSubChainedBones()함수를 포함한다.
		/// </summary>
		public void CheckValidation()
		{
			if (_effectorBone != null)
			{

				//Effector 본이 존재할 때
				//Effector 본은
				//- 자기 자신 제외
				//- 자신의 IK Header가 있다면, IK Header의 모든 Child 제외
				//- Parent 제외
				//- 자신의 모든 Child 제외 (Chain 상관없이)
				//위 조건을 만족해야한다.

				List<apBone> exBones = new List<apBone>();
				exBones.Add(_parentBone);

				if (_parentBone._IKHeaderBone != null)
				{
					List<apBone> childBonesOfIKHeader = _parentBone._IKHeaderBone.GetAllChildBones();
					if (childBonesOfIKHeader != null)
					{
						for (int i = 0; i < childBonesOfIKHeader.Count; i++)
						{
							exBones.Add(childBonesOfIKHeader[i]);
						}
					}
				}

				if (_parentBone._parentBone != null)
				{
					exBones.Add(_parentBone._parentBone);
				}

				List<apBone> childBones = _parentBone.GetAllChildBones();
				if (childBones != null)
				{
					for (int i = 0; i < childBones.Count; i++)
					{
						exBones.Add(childBones[i]);
					}
				}

				if (exBones.Contains(_effectorBone))
				{
					//선택될 수 없는 EffectBone이다.
					Debug.LogError("유효하지 않은 Position Controller Effector Bone이다.");
					_effectorBoneID = -1;
					_effectorBone = null;
				}
			}

			#region [미사용 코드]
			//if(_startBone != null)
			//{
			//	//EndBone이 존재할 때
			//	//EndBone 조건
			//	//-자기 자신을 제외한 Chained의 모든 자식 본들만 선택가능

			//	//EndBone -> Start Bone으로 변경
			//	//Start Bone이 Parent Bone 중에 하나여야 하며, Chain으로 연결된 상태여야 한다.
			//	//

			//	List<apBone> parentBones = _parentBone.GetAllChainedParentBones();
			//	if(parentBones == null)
			//	{
			//		_startBoneID = -1;
			//		_startBone = null;
			//	}
			//	else if(!parentBones.Contains(_startBone))
			//	{
			//		_startBoneID = -1;
			//		_startBone = null;
			//	}
			//	//TODO
			//	//List<apBone> childChainedBones = _parentBone.GetAllChainedChildBones();
			//	//if(childChainedBones == null)
			//	//{
			//	//	//리스트가 사라졌다.
			//	//	_endBoneID = -1;
			//	//	_endBone = null;
			//	//}
			//	//else if(!childChainedBones.Contains(_endBone))
			//	//{
			//	//	//선택될 수 없는 EndBone이다.
			//	//	Debug.LogError("유효하지 않은 Position Controller End Bone ID이다.");
			//	//	_endBoneID = -1;
			//	//	_endBone = null;
			//	//}
			//}

			//RefreshSubChainedBones(); 
			#endregion
		}

		#region [미사용 코드]
		//public void RefreshSubChainedBones()
		//{
		//	//EndBone > StartBone으로 수정하자
		//	if(_startBone == null)
		//	{
		//		_subChainedBones.Clear();
		//		return;
		//	}


		//	List<apBone> parentBones = _parentBone.GetAllChainedParentBones();
		//	if(parentBones == null)
		//	{
		//		_subChainedBones.Clear();
		//		return;
		//	}
		//	else if(!parentBones.Contains(_startBone))
		//	{
		//		_subChainedBones.Clear();
		//		return;
		//	}
		//	//이제 하나씩 비교하면서 체크하자
		//	//Start Bone이 포함된 Chain Bone 리스트
		//	List<apBone> chainBones = new List<apBone>();
		//	for (int i = 0; i < parentBones.Count; i++)
		//	{
		//		chainBones.Add(parentBones[i]);
		//		if(parentBones[i] == _startBone)
		//		{
		//			break;
		//		}
		//	}


		//	//1. Chain이 유효한지 테스트
		//	bool isValid = false;
		//	if(_subChainedBones.Count == chainBones.Count)
		//	{
		//		isValid = true;
		//		for (int i = 0; i < _subChainedBones.Count; i++)
		//		{
		//			if(_subChainedBones[i]._subBone != chainBones[i])
		//			{
		//				//다른게 나타났다!
		//				isValid = false;
		//				break;
		//			}
		//		}
		//	}

		//	if(isValid)
		//	{
		//		//변경된게 없네용
		//		return;
		//	}

		//	//2. 변경된게 있을 경우
		//	//리스트를 복제 + Clear하고 하나씩 넣자
		//	Dictionary<apBone, SubChainedBone> prevChainedBones = new Dictionary<apBone, SubChainedBone>();
		//	for (int i = 0; i < _subChainedBones.Count; i++)
		//	{
		//		SubChainedBone subChainBone = _subChainedBones[i];
		//		if(subChainBone._subBone != null && !prevChainedBones.ContainsKey(subChainBone._subBone))
		//		{
		//			prevChainedBones.Add(subChainBone._subBone, subChainBone);
		//		}
		//	}

		//	//리스트로 하나씩 넣자.
		//	//이전데이터가 있다면 복사를 한다.
		//	_subChainedBones.Clear();
		//	for (int i = 0; i < chainBones.Count; i++)
		//	{
		//		SubChainedBone newChainedBone = new SubChainedBone();
		//		apBone bone = chainBones[i];
		//		if(prevChainedBones.ContainsKey(bone))
		//		{
		//			newChainedBone.CopyFromChainedBone(prevChainedBones[bone]);
		//		}
		//		else
		//		{
		//			newChainedBone.SetNewBone(bone);
		//		}

		//		_subChainedBones.Add(newChainedBone);
		//	}

		//	#region [미사용 코드]
		//	////만약 EndBone이 SubChained의 마지막 Bone의 자식 본이 아니라면 해제해야한다.
		//	////> Check Validation에서 이미 다 적용한 이후이다.
		//	//if(_endBone == null)
		//	//{
		//	//	_subChainedBones.Clear();
		//	//	return;
		//	//}

		//	////Parent > EndBone까지의 Chain을 검사하면서 _subChainedBone 리스트가 적절한지 테스트한다.
		//	////가급적 기존 데이터를 이용한다. (Weight 때문)

		//	////전체 Chain 본들을 가져오고,
		//	////End까지의 리스트를 만든다.
		//	//List<apBone> chainBones = _parentBone.GetAllChainedChildBones();
		//	//if(chainBones == null)
		//	//{
		//	//	//Chain이 없는데용..
		//	//	_subChainedBones.Clear();
		//	//	return;
		//	//}
		//	//if(!chainBones.Contains(_endBone))
		//	//{
		//	//	//ChainBone에 EndBone이 없는데용..
		//	//	_subChainedBones.Clear();
		//	//	return;
		//	//}
		//	//List<apBone> chainBonesBeforeEndBone = new List<apBone>();
		//	//for (int i = 0; i < chainBones.Count; i++)
		//	//{
		//	//	if(chainBones[i] == _endBone)
		//	//	{
		//	//		//End를 만나면 중단
		//	//		break;
		//	//	}

		//	//	chainBonesBeforeEndBone.Add(chainBones[i]);
		//	//}

		//	////1. Chain이 유효한지 테스트
		//	//bool isValid = false;
		//	//if(_subChainedBones.Count == chainBonesBeforeEndBone.Count)
		//	//{
		//	//	isValid = true;
		//	//	for (int i = 0; i < _subChainedBones.Count; i++)
		//	//	{
		//	//		if(_subChainedBones[i]._subBone != chainBonesBeforeEndBone[i])
		//	//		{
		//	//			//다른게 나타났다!
		//	//			isValid = false;
		//	//			break;
		//	//		}
		//	//	}
		//	//}

		//	//if(isValid)
		//	//{
		//	//	//변경된게 없네용
		//	//	return;
		//	//}

		//	////2. 변경된게 있을 경우
		//	////리스트를 복제 + Clear하고 하나씩 넣자
		//	//Dictionary<apBone, SubChainedBone> prevChainedBones = new Dictionary<apBone, SubChainedBone>();
		//	//for (int i = 0; i < _subChainedBones.Count; i++)
		//	//{
		//	//	SubChainedBone subChainBone = _subChainedBones[i];
		//	//	if(subChainBone._subBone != null && !prevChainedBones.ContainsKey(subChainBone._subBone))
		//	//	{
		//	//		prevChainedBones.Add(subChainBone._subBone, subChainBone);
		//	//	}
		//	//}

		//	////리스트로 하나씩 넣자.
		//	////이전데이터가 있다면 복사를 한다.
		//	//_subChainedBones.Clear();
		//	//for (int i = 0; i < chainBonesBeforeEndBone.Count; i++)
		//	//{
		//	//	SubChainedBone newChainedBone = new SubChainedBone();
		//	//	apBone bone = chainBonesBeforeEndBone[i];
		//	//	if(prevChainedBones.ContainsKey(bone))
		//	//	{
		//	//		newChainedBone.CopyFromChainedBone(prevChainedBones[bone]);
		//	//	}
		//	//	else
		//	//	{
		//	//		newChainedBone.SetNewBone(bone);
		//	//	}

		//	//	_subChainedBones.Add(newChainedBone);
		//	//} 
		//	#endregion
		//}


		//public void NormalizeLookWeights()
		//{
		//	//모든 Weight의 합이 1 또는 0 (모두 0일때)이 되게 만든다.
		//	float totalWeight = 0.0f;
		//	//totalWeight += _lookWeight;
		//	for (int i = 0; i < _subChainedBones.Count; i++)
		//	{
		//		totalWeight += _subChainedBones[i]._subWeight;
		//	}

		//	if(totalWeight > 0.0f)
		//	{
		//		//모두 totalWeight로 나누어준다.
		//		//_lookWeight /= totalWeight;

		//		for (int i = 0; i < _subChainedBones.Count; i++)
		//		{
		//			_subChainedBones[i]._subWeight /= totalWeight;
		//		}
		//	}
		//	else
		//	{
		//		//모두 0이다.
		//		//Base만 1.0으로 한다.
		//		//_lookWeight = 1.0f;
		//		for (int i = 0; i < _subChainedBones.Count; i++)
		//		{
		//			_subChainedBones[i]._subWeight = 0.0f;
		//		}
		//	}
		//} 
		#endregion

		#region [미사용 코드]
		////Update - Look At은 여기서 별도로 처리한다.
		////-----------------------------------------------------------------
		//public bool UpdateLookAt(float weight)
		//{
		//	if(_effectorBone == null || _controllerType != CONTROLLER_TYPE.LookAt)
		//	{
		//		return false;
		//	}

		//	//Target에 대해서 정의를 해야한다.
		//	Vector2 targetPos_Origin = _effectorBone._worldMatrix_NonModified._pos;
		//	Vector2 targetPos = _effectorBone._worldMatrix._pos;
		//	Vector2 endPos_Origin = _parentBone._worldMatrix_NonModified._pos;
		//	Vector2 endPos = _parentBone._worldMatrix._pos;

		//	float lengthEndToTarget = Vector2.Distance(targetPos_Origin, endPos_Origin);
		//	Vector2 dirTargetToEnd = _parentBone._worldMatrix._pos - targetPos;
		//	Vector2 expectedEndPos = dirTargetToEnd.normalized * lengthEndToTarget + targetPos;//<<EndBone(ParentBone)이 이 위치로 이동하도록 노력해보자

		//	float remainedWeight = 1.0f;//<< 계산을 위한 남은 Weight
		//	float totalDeltaAngle = 0.0f;//<<변형된 각도를 누적하자.

		//	//각 Chain Unit별로 현재 위치와 각도를 계산하자
		//	//일단 초기화
		//	int nSubBoneCount = _subChainedBones.Count;

		//	_lookAtNextPosW = _parentBone._worldMatrix._pos;

		//	if (nSubBoneCount > 0)
		//	{
		//		for (int i = 0; i < _subChainedBones.Count; i++)
		//		{
		//			_subChainedBones[i].ReadyToUpdate();
		//		}


		//		SubChainedBone curChainedBone = null;
		//		SubChainedBone prevChainedBone = null;
		//		SubChainedBone lastChainedBone = _subChainedBones[0];
		//		float legthEndToParent = Vector2.Distance(endPos, lastChainedBone._pos_Prev);

		//		//Root (마지막 본)부터 아래로 LookAt을 계산한다.

		//		for (int i = nSubBoneCount - 1; i >= 0; i--)
		//		{
		//			curChainedBone = _subChainedBones[i];

		//			//현재 EndPos와 ExpectedEndPos간의 각도 차이를 계산하자
		//			float deltaAngle = apUtil.AngleTo180(
		//									Vector2Angle(expectedEndPos - curChainedBone._pos_Next) 
		//									- Vector2Angle(endPos - curChainedBone._pos_Next)
		//									);

		//			//float weightedDeltaAngle = deltaAngle * curChainedBone._subWeight;
		//			float weightedDeltaAngle = deltaAngle;

		//			//TODO : 제한된 각도

		//			//각도를 넣어주자
		//			curChainedBone._angleWorld_Next += weightedDeltaAngle;

		//			//자식 본들의 World 좌표를 다시 갱신하자
		//			if(curChainedBone._childChainedBone != null)
		//			{
		//				curChainedBone._childChainedBone.CalculateWorldRecursive();
		//			}

		//			//마지막 자식 본의 위치를 기준으로 EndPos 계산하자
		//			//endPos = lastChainedBone._pos_Next + Angle2Vector2(lastChainedBone._angleWorld_Next) * legthEndToParent;


		//			//if (prevChainedBone != null)
		//			//{
		//			//	//이전에 처리된 "부모 본"이 있다면
		//			//	//World 좌표를 갱신해야한다.
		//			//	curChainedBone._angleWorld_Next += totalDeltaAngle;
		//			//	curChainedBone._angleWorld_Min += totalDeltaAngle;
		//			//	curChainedBone._angleWorld_Max += totalDeltaAngle;

		//			//	//위치와 각도를 Parent에 맞게 갱신한다.
		//			//	curChainedBone._pos_Next = prevChainedBone._pos_Next +
		//			//		Angle2Vector2(prevChainedBone._angleWorld_Next) * curChainedBone._lengthToParent;


		//			//}


		//			//if(curChainedBone._subWeight > 0.0f)
		//			//{
		//			//	//이제 각도를 계산하자
		//			//	Vector2 lookVector = targetPos - curChainedBone._pos_Next;
		//			//	if(lookVector.sqrMagnitude > 0.0f)
		//			//	{
		//			//		//동일한 벡터가 아니라면 통과

		//			//		//각도를 계산
		//			//		float lookAngle = Vector2Angle(lookVector);

		//			//		float deltaLookAngle = apUtil.AngleTo180(lookAngle - curChainedBone._angleWorld_Next);

		//			//		//이 각도를 그대로 적용할 수 있는 건 아니다.
		//			//		//Weight만큼 제한
		//			//		//TODO : 각도 제한

		//			//		float weightedDeltaLookAngle = deltaLookAngle * curChainedBone._subWeight;
		//			//		remainedWeight -= curChainedBone._subWeight;
		//			//		totalDeltaAngle += weightedDeltaLookAngle;
		//			//		curChainedBone._angleWorld_Next += weightedDeltaLookAngle;

		//			//	}


		//			//}

		//			prevChainedBone = curChainedBone;
		//		}

		//		//마지막으로 현재 본의 위치와 각도도 회전한다.
		//		//prevChainedBone = _subChainedBones[0];

		//		//_lookAtNextPosW = prevChainedBone._pos_Next +
		//		//			Angle2Vector2(prevChainedBone._angleWorld_Next) * Vector2.Distance(prevChainedBone._pos_Prev, _parentBone._worldMatrix._pos);




		//	}

		//	//이건 바로 바라보도록 설정
		//	//_lookAtNextAngleW = Vector2Angle(targetPos - _lookAtNextPosW);
		//	_lookAtNextAngleW = Vector2Angle(targetPos - endPos);

		//	//이제 결과값을 적용하자
		//	if (nSubBoneCount > 0)
		//	{
		//		for (int i = 0; i < _subChainedBones.Count; i++)
		//		{
		//			//_subChainedBones[i]._subBone.AddIKAngle_Controlled(apUtil.AngleTo180(_subChainedBones[i]._angleWorld_Next) + 90, weight);
		//			_subChainedBones[i]._subBone.AddIKAngle_Controlled(apUtil.AngleTo180(_subChainedBones[i]._angleWorld_Next - _subChainedBones[i]._angleWorld_Prev), weight);
		//		}
		//	}

		//	//_parentBone.AddIKAngle_Controlled(_lookAtNextAngleW + 90, weight);//내부에서 -90 bias가 있어서 여기서는 맞는 좌표계임에도 +90을 한다.
		//	_parentBone.AddIKAngle_Controlled(_lookAtNextAngleW - _parentBone._worldMatrix._angleDeg, weight);//내부에서 -90 bias가 있어서 여기서는 맞는 좌표계임에도 +90을 한다.

		//	return true;
		//}

		//private static float Vector2Angle(Vector2 dirVec)
		//{
		//	return Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg - 90;
		//}

		//private static Vector2 Angle2Vector2(float angle)
		//{
		//	return new Vector2(Mathf.Cos((angle + 90) * Mathf.Rad2Deg), Mathf.Sin((angle + 90) * Mathf.Rad2Deg));
		//} 
		#endregion


		// Get / Set
		//---------------------------------------------------
	}
}