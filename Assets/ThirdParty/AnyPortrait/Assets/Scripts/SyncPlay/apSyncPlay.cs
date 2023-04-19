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
	/// Options for synchronizing bones
	/// </summary>
	public enum SYNC_BONE_OPTION
	{
		/// <summary>The structure and name must be the same from the Root bone.</summary>
		MatchFromRoot,//루트부터 구조가 같아야 한다.
		/// <summary>Even if it is not the Root bone, if the structure and name of the bones are the same, they are synchronized.</summary>
		MatchFromSubBones,//중간부터 구조가 같아도 된다.
	}

	





	public class apSyncPlay
	{
		// Members
		//--------------------------------------------
		private apPortrait _selfPortrait = null;
		private apPortrait _targetPortrait = null;

		private bool _isSync_Anim = false;
		private bool _isSync_ControlParam = false;
		
		//추가 21.9.14 : 본 리타겟
		private bool _isSync_Bone = false;

		//추가 21.9.21 : 루트 유닛 리타겟
		//private bool _isSync_RootUnit = false;
		private Dictionary<apOptRootUnit, apOptRootUnit> _syncMap_RootUnit = null;

		//연동된 데이터들
		public List<apSyncSet_AnimClip> _syncSet_AnimClip = null;
		public Dictionary<apAnimClip, apSyncSet_AnimClip> _animClip2SyncSet = null;
		public int _nSyncSet_AnimClip = 0;

		public List<apSyncSet_ControlParam> _sync_ControlParam = null;
		public Dictionary<apControlParam, apSyncSet_ControlParam> _controlParam2SyncSet = null;
		public int _nSyncSet_ControlParam = 0;

		//추가 21.9.14 : 본 리타겟
		//연결 조건과 함께 설정할 수 있다.
		//루트 유닛에 따라 다르다. (동작하지 않는 다른 루트 유닛에 대해서 동기화를 할 필요는 없다.)
		public class SyncBoneUnitPerRootUnit
		{
			//루트 유닛이 맞는 경우에만 동기화를 한다.
			public apOptRootUnit _key_TargetRootUnit = null;
			public apOptRootUnit _linked_SelfRootUnit = null;

			public List<apSyncSet_Bone> _sync_Bone_Root = null;
			public List<apSyncSet_Bone> _sync_Bone_All = null;
			public int _nSyncBone_Root = 0;
			public int _nSyncBone_All = 0;

			public bool _isValid = false;

			public SyncBoneUnitPerRootUnit(apOptRootUnit key_TargetRootUnit, apOptRootUnit linked_SelfRootUnit)
			{
				_key_TargetRootUnit = key_TargetRootUnit;
				_linked_SelfRootUnit = linked_SelfRootUnit;

				_sync_Bone_Root = new List<apSyncSet_Bone>();
				_sync_Bone_All = new List<apSyncSet_Bone>();
				_nSyncBone_Root = 0;
				_nSyncBone_All = 0;

				_isValid = false;
			}

			public void Validate()
			{
				_nSyncBone_Root = _sync_Bone_Root.Count;
				_nSyncBone_All = _sync_Bone_All.Count;

				_isValid = _nSyncBone_Root > 0 && _nSyncBone_All > 0;
			}

			public void UnsyncAll()
			{
				for (int i = 0; i < _nSyncBone_All; i++)
				{
					_sync_Bone_All[i].Unsync();
				}
			}
		}
		//public List<apSyncSet_Bone> _sync_Bone_Root = null;
		//public List<apSyncSet_Bone> _sync_Bone_All = null;
		private Dictionary<apOptRootUnit, SyncBoneUnitPerRootUnit> _sync_BoneUnitPerTargetRootUnit = null;
		private List<SyncBoneUnitPerRootUnit> _sync_BoneUnitsAll = null;

		//추가 21.9.21 : 루트 유닛 동기화
		private apOptRootUnit _selfRootUnit_Prev = null;
		private apOptRootUnit _selfRootUnit_Cur = null;
		private apOptRootUnit _targetRootUnit_Prev = null;
		private apOptRootUnit _targetRootUnit_Cur = null;
		
		
		// Init
		//--------------------------------------------
		public apSyncPlay(	apPortrait selfPortrait,
							apPortrait targetPortrait,
							bool isSyncAnim,
							bool isSyncControlParam,
							bool isSyncBone,
							SYNC_BONE_OPTION syncBoneOption,
							bool isSyncRootUnit
							)
		{
			_selfPortrait = selfPortrait;
			_targetPortrait = targetPortrait;
			_isSync_Anim = isSyncAnim;
			_isSync_ControlParam = isSyncControlParam;
			_isSync_Bone = isSyncBone;
			//_isSync_RootUnit = isSyncRootUnit;

			//연결을 하자
			if(_isSync_Anim 
				&& _selfPortrait._animClips != null
				&& _targetPortrait._animClips != null)
			{
				_syncSet_AnimClip = new List<apSyncSet_AnimClip>();
				_animClip2SyncSet = new Dictionary<apAnimClip, apSyncSet_AnimClip>();

				//전체 AnimClip을 돌면서 이름이 같은걸 찾자
				List<apAnimClip> animClips = _selfPortrait._animClips;
				List<apAnimClip> targetAnimClips = _targetPortrait._animClips;

				int nSrcAnimClips = animClips.Count;
				apAnimClip srcAnimClip = null;
				apAnimClip dstAnimClip = null;

				for (int iSrc = 0; iSrc < nSrcAnimClips; iSrc++)
				{
					srcAnimClip = animClips[iSrc];
					dstAnimClip = targetAnimClips.Find(delegate(apAnimClip a)
					{
						return string.Equals(srcAnimClip._name, a._name);
					});

					apSyncSet_AnimClip newSyncSet = new apSyncSet_AnimClip(srcAnimClip, dstAnimClip);
					_syncSet_AnimClip.Add(newSyncSet);
					_animClip2SyncSet.Add(srcAnimClip, newSyncSet);
				}

				_nSyncSet_AnimClip = _syncSet_AnimClip.Count;
			}
			else
			{
				_syncSet_AnimClip = null;
				_animClip2SyncSet = null;
				_nSyncSet_AnimClip = 0;
			}
			

			if(_isSync_ControlParam
				&& _selfPortrait._controller._controlParams != null
				&& _targetPortrait._controller._controlParams != null)
			{
				_sync_ControlParam = new List<apSyncSet_ControlParam>();
				_controlParam2SyncSet = new Dictionary<apControlParam, apSyncSet_ControlParam>();

				//전체 Control Param을 돌면서 이름이 같은걸 찾자
				List<apControlParam> controlParams = _selfPortrait._controller._controlParams;
				List<apControlParam> targetControlParams = _targetPortrait._controller._controlParams;

				int nSrcAnimClips = controlParams.Count;
				apControlParam srcAnimClip = null;
				apControlParam dstAnimClip = null;

				for (int iSrc = 0; iSrc < nSrcAnimClips; iSrc++)
				{
					srcAnimClip = controlParams[iSrc];
					dstAnimClip = targetControlParams.Find(delegate(apControlParam a)
					{
						return string.Equals(srcAnimClip._keyName, a._keyName)
								&& srcAnimClip._valueType == a._valueType;//타입도 같아야 한다.
					});

					apSyncSet_ControlParam newSyncSet = new apSyncSet_ControlParam(srcAnimClip, dstAnimClip);
					_sync_ControlParam.Add(newSyncSet);
					_controlParam2SyncSet.Add(srcAnimClip, newSyncSet);
				}
				_nSyncSet_ControlParam = _sync_ControlParam.Count;
			}
			else
			{
				_sync_ControlParam = null;
				_controlParam2SyncSet = null;
				_nSyncSet_ControlParam = 0;
			}


			//추가 21.9.17 : 본 동기화 (리타겟)

			if (_isSync_Bone)
			{	
				//타겟의 본들을 재귀적으로 찾으면서 이름이 같은걸 찾자
				//루트 본들을 찾자 (타겟, 자기 자신)
				//단, RootUnit당 각각 찾는다.
				List<List<apOptBone>> target_RootBonesPerRootUnit = new List<List<apOptBone>>();
				List<List<apOptBone>> self_RootBonesPerRootUnit = new List<List<apOptBone>>();

				List<apOptBone> findRootBones = null;
				apOptRootUnit curRootUnit = null;

				int nTargetRootBones = 0;
				int nSelfRootBones = 0;

				//타겟부터
				int nRootUnits_Target = targetPortrait._optRootUnitList != null ? targetPortrait._optRootUnitList.Count : 0;
				if (nRootUnits_Target > 0)
				{
					for (int iRootUnit = 0; iRootUnit < nRootUnits_Target; iRootUnit++)
					{
						curRootUnit = targetPortrait._optRootUnitList[iRootUnit];

						List<apOptBone> curRootBones = new List<apOptBone>();
						target_RootBonesPerRootUnit.Add(curRootBones);

						findRootBones = curRootUnit.GetRootBones();
						if(findRootBones == null || findRootBones.Count == 0)
						{
							continue;
						}

						//찾은걸 하나씩 넣어주자
						for (int iFind = 0; iFind < findRootBones.Count; iFind++)
						{	
							curRootBones.Add(findRootBones[iFind]);
						}

						nTargetRootBones += curRootBones.Count;//개수 증가
						
					}
				}
				
				//본인의 본도 찾자
				int nRootUnits_Self = selfPortrait._optRootUnitList != null ? selfPortrait._optRootUnitList.Count : 0;
				if (nRootUnits_Self > 0)
				{
					for (int iRootUnit = 0; iRootUnit < nRootUnits_Self; iRootUnit++)
					{
						curRootUnit = selfPortrait._optRootUnitList[iRootUnit];

						List<apOptBone> curRootBones = new List<apOptBone>();
						self_RootBonesPerRootUnit.Add(curRootBones);

						findRootBones = curRootUnit.GetRootBones();
						if(findRootBones == null || findRootBones.Count == 0)
						{
							continue;
						}

						//찾은걸 하나씩 넣어주자
						for (int iFind = 0; iFind < findRootBones.Count; iFind++)
						{	
							curRootBones.Add(findRootBones[iFind]);
						}

						nSelfRootBones += curRootBones.Count;//개수 증가
					}
				}
				

				//이제 연결을 하자
				//루트 유닛의 개수가 다를 수 있으므로
				//Min 개수로 맞춘다.
				int nRootUnit_Common = Mathf.Min(nRootUnits_Target, nRootUnits_Self);

				if (nTargetRootBones == 0 || nSelfRootBones == 0 || nRootUnit_Common == 0)
				{
					_isSync_Bone = false;
					//_sync_Bone_Root = null;
					//_sync_Bone_All = null;

					_sync_BoneUnitPerTargetRootUnit = null;
					_sync_BoneUnitsAll = null;
				}
				else
				{
					//- 옵션에 따라 다르다.
					//- 루트 유닛별로 찾아야 한다.
					
					
					_sync_BoneUnitPerTargetRootUnit = new Dictionary<apOptRootUnit, SyncBoneUnitPerRootUnit>();
					_sync_BoneUnitsAll = new List<SyncBoneUnitPerRootUnit>();

					//타겟 루트 유닛별로 본 동기화 유닛을 생성한다.
					for (int iRootUnitTarget = 0; iRootUnitTarget < nRootUnits_Target; iRootUnitTarget++)
					{
						apOptRootUnit targetRootUnit = targetPortrait._optRootUnitList[iRootUnitTarget];
						apOptRootUnit selfRootUnit = null;
						if(iRootUnitTarget < nRootUnits_Self)
						{
							selfRootUnit = selfPortrait._optRootUnitList[iRootUnitTarget];
						}
						SyncBoneUnitPerRootUnit newSyncUnit = new SyncBoneUnitPerRootUnit(targetRootUnit, selfRootUnit);
						_sync_BoneUnitPerTargetRootUnit.Add(targetRootUnit, newSyncUnit);
						_sync_BoneUnitsAll.Add(newSyncUnit);
					}


					
					List<apOptBone> targetRootBones = null;
					List<apOptBone> selfRootBones = null;
					SyncBoneUnitPerRootUnit curSyncUnit = null;
					for (int iRootUnit = 0; iRootUnit < nRootUnit_Common; iRootUnit++)
					{
						targetRootBones = target_RootBonesPerRootUnit[iRootUnit];
						selfRootBones = self_RootBonesPerRootUnit[iRootUnit];

						//루트 유닛을 키로 하여 동기화 유닛을 가져오고, 거기에 값을 넣는다.
						curSyncUnit = _sync_BoneUnitPerTargetRootUnit[targetPortrait._optRootUnitList[iRootUnit]];

						//int nRootBones = targetRootBones.Count;
						//for (int iRootBone = 0; iRootBone < nRootBones; iRootBone++)
						//{
						//	//FindSyncBone(null, targetRootBones[iRootBone], null, selfRootBones, syncBoneOption, curSyncUnit);
							
						//}
						int nSelfRootBonesOnList = selfRootBones != null ? selfRootBones.Count : 0;
						for (int iSelfRootBone = 0; iSelfRootBone < nSelfRootBonesOnList; iSelfRootBone++)
						{
							FindSyncBone_FirstLoop(selfRootBones[iSelfRootBone], targetRootBones, syncBoneOption, curSyncUnit);
						}
						

						//동기화 결과를 갱신한다.
						curSyncUnit.Validate();
					}
				}
			}
			else
			{
				_sync_BoneUnitPerTargetRootUnit = null;
				_sync_BoneUnitsAll = null;
			}

			//루트 유닛 동기화
			if(isSyncRootUnit)
			{
				_syncMap_RootUnit = new Dictionary<apOptRootUnit, apOptRootUnit>();

				_selfRootUnit_Prev = null;
				_selfRootUnit_Cur = null;
				_targetRootUnit_Prev = null;
				_targetRootUnit_Cur = null;

				int nRootUnits_Target = _targetPortrait._optRootUnitList != null ? _targetPortrait._optRootUnitList.Count : 0;
				int nRootUnits_Self = _selfPortrait._optRootUnitList != null ? _selfPortrait._optRootUnitList.Count : 0;

				//이 함수를 호출하기 전에 루트 유닛 개수로 유효성 검사를 했을 것.
				//그래도 혹시 모를 에러를 막기 위해 처리를 하자
				apOptRootUnit curSyncRootUnit = null;
				apOptRootUnit selfRootUnit = null;

				for (int i = 0; i < nRootUnits_Target; i++)
				{
					curSyncRootUnit = _targetPortrait._optRootUnitList[i];
					if(curSyncRootUnit == null || _syncMap_RootUnit.ContainsKey(curSyncRootUnit))
					{
						continue;
					}

					if(i < nRootUnits_Self)
					{
						selfRootUnit = _selfPortrait._optRootUnitList[i];
					}
					else if(nRootUnits_Self > 0)
					{
						//적절한 루트 유닛이 없다면 (에러지만)
						//첫번째 루트 유닛을 할당
						selfRootUnit = _selfPortrait._optRootUnitList[0];
					}
					else
					{
						//그마저도 없다면 null
						selfRootUnit = null;
					}

					_syncMap_RootUnit.Add(curSyncRootUnit, selfRootUnit);
				}


			}
		}

		#region [미사용 코드] 잘못짬
		//private bool FindSyncBone(	apSyncSet_Bone parentSyncSet, 
		//							apOptBone targetSyncBone, 
		//							apOptBone selfParentBone, 
		//							List<apOptBone> selfRootBones, 
		//							SYNC_BONE_OPTION syncBoneOption,
		//							SyncBoneUnitPerRootUnit resultUnit)
		//{
		//	//거꾸로 계산
		//	//Self의 루트들을 기준으로 이름이 같은걸 찾자

		//	//<1> 루트부터 같아야 하는 경우
		//	//[루트 본에 대해서]
		//	//1. 타겟의 루트 본을 하나 선택하고 이름이 같은 "루트 본" 있는지 확인한다.
		//	//2-1. 이름이 같은 본이 현재 본 중에 있다면 연결
		//	//2-2. 이름이 같은 본이 없다면 더 찾지 않는다. (다른 루트본으로 넘어감)
		//	//3. 이 자식 본에 대해서 재귀적으로 호출한다.

		//	//[자식 본에 대해서]
		//	//1. 자식 본 중 하나를 선택하고, 연결된 부모 본의 자식들 중에서 같은게 있는지 확인한다.
		//	//2-1. 이름이 같은 자식 본이 있다면 연결
		//	//2-2. 이름이 같은 본이 없다면 종료
		//	//3. 재귀적으로 계속 호출한다.


		//	//<2> 루트가 같지 않아도 되는 경우
		//	//[루트 본에 대해서]
		//	//1. 타겟의 루트 본을 하나 선택하고 이름이 같은 "루트 본" 있는지 확인한다.
		//	//2-1. 이름이 같은 본이 현재 본 중에 있다면 연결하고 <연결된 후 자식 본 찾기> 단계로 넘어간다.
		//	//2-2. 이름이 같은 본이 없다면 <연결되지 않은 상태로 자식 본 찾기> 단계로 넘어간다.

		//	//[연결되지 않은 상태로 자식 본에서 찾기]
		//	//1. 자식 본들 중 하나를 선택하고 이름이 같은게 있는지 확인한다.
		//	//2-1. 이름이 같은 "루트 본"이 있다면 연결하고 <연결된 후 자식 본 찾기> 단계로 넘어간다.
		//	//2-2. 이름이 같은 "루트 본"이 없다면 다음 자식 본에 대해서 반복한다.

		//	if(targetSyncBone == null)
		//	{
		//		return false;
		//	}


		//	apOptBone findBone = null;

		//	if(parentSyncSet == null)
		//	{
		//		//[새로 찾는 경우]

		//		//이름이 같은 본인의 루트 본이 있는가
		//		findBone = selfRootBones.Find(delegate(apOptBone a)
		//		{
		//			return string.Equals(targetSyncBone._name, a._name);
		//		});

		//		if(findBone != null)
		//		{
		//			//찾았다 > 새로운 SyncSet을 만들고, 자식으로 넘어간다.
		//			apSyncSet_Bone newSyncSet = new apSyncSet_Bone(findBone, targetSyncBone);

		//			resultUnit._sync_Bone_All.Add(newSyncSet);
		//			resultUnit._sync_Bone_Root.Add(newSyncSet);//루트에도 추가

		//			//자식 본에 대해서도 반복
		//			int nSyncChildBones = targetSyncBone._childBones != null ? targetSyncBone._childBones.Length : 0;
		//			for (int iChild = 0; iChild < nSyncChildBones; iChild++)
		//			{
		//				FindSyncBone(newSyncSet, targetSyncBone._childBones[iChild], findBone, selfRootBones, syncBoneOption, resultUnit);
		//			}
		//		}
		//		else
		//		{
		//			//찾지 못했다면 옵션에 따라서 다르다
		//			if(syncBoneOption == SYNC_BONE_OPTION.MatchFromRoot)
		//			{
		//				//루트부터 같아야 하는 경우 : 루트가 달랐으므로 처리 종료
		//				return false;
		//			}
		//			else
		//			{
		//				//루트가 달라도 되는 경우 : Sync 본의 자식 본과 본체의 Root 본들이 같은지 검사하자
		//				bool isAnySyncBone = false;

		//				int nSyncChildBones = targetSyncBone._childBones != null ? targetSyncBone._childBones.Length : 0;
		//				for (int iChild = 0; iChild < nSyncChildBones; iChild++)
		//				{
		//					if(FindSyncBone(null, targetSyncBone._childBones[iChild], null, selfRootBones, syncBoneOption, resultUnit))
		//					{
		//						isAnySyncBone = true;
		//					}
		//				}
		//				return isAnySyncBone;
		//			}
		//		}
		//	}
		//	else
		//	{
		//		//[부모로 부터 이어서 찾는 경우]
		//		if(selfParentBone == null)
		//		{
		//			//부모 본이 없다면 종료
		//			return false;
		//		}
		//		int nSelfChildBones = selfParentBone._childBones != null ? selfParentBone._childBones.Length : 0;
		//		if(nSelfChildBones == 0)
		//		{
		//			//새로 연결할 자식 본이 없어도 종료
		//			return false;
		//		}

		//		bool isAnySyncBone = false;

		//		apOptBone selfChildBone = null;
		//		for (int iSelfChildBone = 0; iSelfChildBone < nSelfChildBones; iSelfChildBone++)
		//		{
		//			selfChildBone = selfParentBone._childBones[iSelfChildBone];
		//			if(string.Equals(targetSyncBone._name, selfChildBone._name))
		//			{
		//				//동일한 이름의 자식 본을 찾았다.
		//				//SyncSet 생성 후 부모에 연결
		//				apSyncSet_Bone newSyncSet = new apSyncSet_Bone(selfChildBone, targetSyncBone);
		//				newSyncSet.SetParent(parentSyncSet);

		//				resultUnit._sync_Bone_All.Add(newSyncSet);

		//				//자식 본에 대해서 재귀 호출 후 Break;
		//				int nSyncChildBones = targetSyncBone._childBones != null ? targetSyncBone._childBones.Length : 0;
		//				for (int iTargetChild = 0; iTargetChild < nSyncChildBones; iTargetChild++)
		//				{
		//					if(FindSyncBone(newSyncSet, targetSyncBone._childBones[iTargetChild], selfChildBone, selfRootBones, syncBoneOption, resultUnit))
		//					{
		//						isAnySyncBone = true;
		//					}
		//					else if(syncBoneOption == SYNC_BONE_OPTION.MatchFromSubBones)
		//					{
		//						//Sync의 자식본과 
		//					}

		//				}

		//				break;
		//			}
		//		}

		//		//재귀 처리 종료
		//	}
		//} 
		#endregion


		private void FindSyncBone_FirstLoop(	apOptBone curRootBone_Self,
												List<apOptBone> rootBones_Target,
												SYNC_BONE_OPTION syncBoneOption,
												SyncBoneUnitPerRootUnit resultUnit)
		{
			//거꾸로 계산
			//Self의 루트들을 기준으로 이름이 같은걸 찾자
			// [루트본 찾기 단계]
			//- Self의 루트 본을 하나 선택한다. (외부)
			//- Target의 루트 본 중에 이름이 같은게 있는지 찾자
			// > 같은게 있다면 : "재귀적으로 이어서 연결"
			// > 같은게 없다면 + "루트가 같아야 하는 옵션" : 종료
			// > 같은게 없다면 + "루트가 달라도 되는 옵션" : 루트 본이 아닌 본 중에 이름이 같은걸 찾는다.

			if(curRootBone_Self == null || rootBones_Target == null)
			{
				return;
			}

			//타겟 루트 본 중에서 이름이 같은걸 찾자
			apOptBone findSyncRootBone = rootBones_Target.Find(delegate(apOptBone a)
			{
				return string.Equals(curRootBone_Self._name, a._name);
			});

			if (findSyncRootBone != null)
			{
				//1. 이름이 같은 루트 본을 찾았다.
				//SyncSet을 만들고 재귀적으로 자식 본으로 연결을 하자
				apSyncSet_Bone newSyncSet = new apSyncSet_Bone(curRootBone_Self, findSyncRootBone);

				resultUnit._sync_Bone_All.Add(newSyncSet);
				resultUnit._sync_Bone_Root.Add(newSyncSet);//루트에도 추가

				//각각의 자식본에 대해서 연결을 한다.
				int nChildBones_Self = curRootBone_Self._childBones != null ? curRootBone_Self._childBones.Length : 0;
				int nChildBones_Target = findSyncRootBone._childBones != null ? findSyncRootBone._childBones.Length : 0;

				if(nChildBones_Self > 0 && nChildBones_Target > 0)
				{
					for (int iChildSelf = 0; iChildSelf < nChildBones_Self; iChildSelf++)
					{
						//이어서 연결을 하자
						FindSyncBone_LinkContinuous(	newSyncSet, 
														curRootBone_Self._childBones[iChildSelf], 
														findSyncRootBone._childBones, resultUnit);
					}
				}
			}
			else
			{
				//2. 이름이 같은 루트 본을 찾지 못했다.
				//옵션에 다라서 처리가 다르다.
				if(syncBoneOption == SYNC_BONE_OPTION.MatchFromRoot)
				{
					//2-1. 루트부터 같아야하는 경우
					//> 여기서 종료
					return;
				}
				else
				{
					//2-2. 하위 본부터 같아도 되는 경우 (단 Self 본은 루트부터 적용되어야 한다.)
					//> 전체 본 중에서 이름이 같은 본을 찾는다. (재귀적으로 + 이름이 같다면 Depth가 적게 들어간게 우선순위)
					int nRootBones_Target = rootBones_Target.Count;
					apOptBone findSubRootBone_Target = null;
					apOptBone curResultRootBone = null;
					int resultFindLevel = -1;
					for (int i = 0; i < nRootBones_Target; i++)
					{
						int curFindLevel = -1;
						curResultRootBone = FindSyncBone_SubRootBone(curRootBone_Self, rootBones_Target[i], 0, ref curFindLevel);

						if(curResultRootBone != null && curFindLevel >= 0)
						{
							if(findSubRootBone_Target == null || curFindLevel < resultFindLevel)
							{
								//여태껏 찾은 루트 본이 없거나 루트에 더 가까운(Level 값이 낮은) 같은 이름의 본을 찾은 경우
								findSubRootBone_Target = curResultRootBone;
								resultFindLevel = curFindLevel;
							}
						}
					}
					
					if(findSubRootBone_Target != null)
					{
						//Sync의 서브 본 중 연결이 될만한 본을 찾았다.
						//찾은 본을 연결하고, 그 본을 기준으로 연결을 진행하자
						apSyncSet_Bone newSyncSet = new apSyncSet_Bone(curRootBone_Self, findSubRootBone_Target);

						resultUnit._sync_Bone_All.Add(newSyncSet);
						resultUnit._sync_Bone_Root.Add(newSyncSet);//루트에도 추가

						//각각의 자식본에 대해서 연결을 한다.
						int nChildBones_Self = curRootBone_Self._childBones != null ? curRootBone_Self._childBones.Length : 0;
						int nChildBones_Target = findSubRootBone_Target._childBones != null ? findSubRootBone_Target._childBones.Length : 0;

						if(nChildBones_Self > 0 && nChildBones_Target > 0)
						{
							for (int iChildSelf = 0; iChildSelf < nChildBones_Self; iChildSelf++)
							{
								//이어서 연결을 하자
								FindSyncBone_LinkContinuous(	newSyncSet, 
																curRootBone_Self._childBones[iChildSelf], 
																findSubRootBone_Target._childBones, resultUnit);
							}
						}
					}
					else
					{
						//연결이 될만한 본을 찾지 못했다. - 종료
						return;
					}

				}
			}
		}

		private void FindSyncBone_LinkContinuous(	apSyncSet_Bone parentSyncSet,
													apOptBone curBone_Self,
													apOptBone[] bones_Target,
													SyncBoneUnitPerRootUnit resultUnit)
		{
			//상위에서 본 연결이 된 경우, 계속해서 이름이 같은걸 찾아서 연결하자
			//연결이 일단 시작되면 본 옵션은 받지 않는다.
			apOptBone findSyncBone = null;
			int nBonesTarget = bones_Target.Length;
			apOptBone curTargetBone = null;
			for (int i = 0; i < nBonesTarget; i++)
			{
				curTargetBone = bones_Target[i];
				if(string.Equals(curBone_Self._name, curTargetBone._name))
				{
					findSyncBone = curTargetBone;
					break;
				}
			}
			
			if (findSyncBone != null)
			{
				//이름이 같은걸 발견				
				apSyncSet_Bone newSyncSet = new apSyncSet_Bone(curBone_Self, findSyncBone);
				newSyncSet.SetParent(parentSyncSet);

				resultUnit._sync_Bone_All.Add(newSyncSet);

				//계속해서 연결하자
				//각각의 자식본에 대해서 연결을 한다.
				int nChildBones_Self = curBone_Self._childBones != null ? curBone_Self._childBones.Length : 0;
				int nChildBones_Target = findSyncBone._childBones != null ? findSyncBone._childBones.Length : 0;

				if(nChildBones_Self > 0 && nChildBones_Target > 0)
				{
					for (int iChildSelf = 0; iChildSelf < nChildBones_Self; iChildSelf++)
					{
						//이어서 연결을 하자
						FindSyncBone_LinkContinuous(	newSyncSet, 
														curBone_Self._childBones[iChildSelf], 
														findSyncBone._childBones, resultUnit);
					}
				}
			}
			else
			{
				//이름이 같은게 없다. 종료
				return;
			}
		}

		private apOptBone FindSyncBone_SubRootBone(	apOptBone rootBone_Self,
													apOptBone bone_Target,
													int curLevel,
													ref int resultLevel)
		{
			if(string.Equals(rootBone_Self._name, bone_Target._name))
			{
				//이름이 같은걸 찾았다.
				resultLevel = curLevel;
				return bone_Target;
			}
			//그렇지 않다면 자식에서 찾아야 한다.
			int nChildBones_Target = bone_Target._childBones != null ? bone_Target._childBones.Length : 0;
			if(nChildBones_Target == 0)
			{
				return null;
			}

			apOptBone resultBone = null;
			for (int i = 0; i < nChildBones_Target; i++)
			{
				int subResultLevel = -1;
				resultBone = FindSyncBone_SubRootBone(rootBone_Self, bone_Target._childBones[i], curLevel+1, ref subResultLevel);
				if(resultBone != null && subResultLevel >= 0)
				{
					//찾았다.
					resultLevel = subResultLevel;
					return resultBone;
				}
			}

			return null;
		}



		// Functions
		//--------------------------------------------		
		public void SyncControlParams()
		{
			if(_nSyncSet_ControlParam == 0)
			{
				return;
			}

			for (int i = 0; i < _nSyncSet_ControlParam; i++)
			{
				_sync_ControlParam[i].Sync();
			}
		}

		public void SyncRootUnit()
		{
			//루트 유닛 인덱스를 확인하고 갱신한다.
			_selfRootUnit_Cur = null;
			_targetRootUnit_Cur = null;

			bool isAnyChanged = false;
			if(_selfPortrait._curPlayingOptRootUnit != null)
			{
				if(_selfRootUnit_Prev != _selfPortrait._curPlayingOptRootUnit)
				{
					//루트 유닛이 바뀌었다.
					_selfRootUnit_Cur = _selfPortrait._curPlayingOptRootUnit;
					_selfRootUnit_Prev = _selfRootUnit_Cur;
					
					isAnyChanged = true;
				}
				else
				{
					_selfRootUnit_Cur = _selfPortrait._curPlayingOptRootUnit;
				}
			}
			else
			{
				_selfRootUnit_Cur = null;
				if(_selfRootUnit_Prev != null)
				{
					isAnyChanged = true;
					_selfRootUnit_Prev = null;
				}
				
			}

			if(_targetPortrait._curPlayingOptRootUnit != null)
			{
				if(_targetRootUnit_Prev != _targetPortrait._curPlayingOptRootUnit)
				{
					_targetRootUnit_Cur = _targetPortrait._curPlayingOptRootUnit;
					_targetRootUnit_Prev = _targetRootUnit_Cur;

					isAnyChanged = true;
				}
				else
				{
					_targetRootUnit_Cur = _targetPortrait._curPlayingOptRootUnit;
				}
			}
			else
			{
				_targetRootUnit_Cur = null;
				if(_targetRootUnit_Prev != null)
				{
					isAnyChanged = true;
					_targetRootUnit_Prev = null;
				}
				
			}

			if(isAnyChanged)
			{
				//루트 유닛을 동기화한다.
				if(_targetRootUnit_Cur != null)
				{
					//연결될 타겟을 찾자
					_selfRootUnit_Cur = _syncMap_RootUnit[_targetRootUnit_Cur];
					if(_selfRootUnit_Cur == null)
					{
						_selfPortrait.Hide();
						//Debug.Log("Root Unit 변화 > 연결된게 없어서 Hide");
					}
					else
					{
						_selfPortrait.SwitchRootUnitWithoutPlayAnim(_selfRootUnit_Cur);
						//Debug.Log("Root Unit 변화 > " + _selfRootUnit_Cur.gameObject.name);
					}
				}
				else
				{
					_selfRootUnit_Cur = null;
					_selfPortrait.Hide();
					//Debug.Log("Root Unit 변화 > Hide");
				}

				_selfRootUnit_Prev = _selfRootUnit_Cur;
			}
		}


		public void Unsynchronize()
		{
			//if(_sync_Bone_All != null)
			//{
			//	int nSyncBones = _sync_Bone_All.Count;
			//	for (int i = 0; i < nSyncBones; i++)
			//	{
			//		_sync_Bone_All[i].Unsync();
			//	}
			//}
			if(_sync_BoneUnitsAll != null)
			{
				int nSyncUnits = _sync_BoneUnitsAll.Count;
				for (int iSyncUnit = 0; iSyncUnit < nSyncUnits; iSyncUnit++)
				{
					_sync_BoneUnitsAll[iSyncUnit].UnsyncAll();
				}

				_sync_BoneUnitPerTargetRootUnit = null;
				_sync_BoneUnitsAll = null;
			}

			_syncMap_RootUnit = null;
		}
	}
}