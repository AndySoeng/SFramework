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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// TimelineUI에서 여러개의 키프레임들을 선택했을 때, 한번에 "동기화된" 커브 편집을 위한 클래스
	/// 여러개의 키프레임들의 커브를 Prev/Next로 구분하여 동기화 여부를 테스트한 뒤,
	/// 편집을 위한 가상 커브를 제공한다.
	/// </summary>
	public class apTimelineCommonCurve
	{
		// Members
		//----------------------------------------------------
		public const int CURVE__PREV = 0;
		public const int CURVE__MID = 1;
		public const int CURVE__NEXT = 2;
		public const int NUM_CURVE_TYPE = 3;

		public enum SYNC_STATUS
		{
			/// <summary>키프레임들이 없다. (가능)</summary>
			NoKeyframes,
			/// <summary>키프레임은 있지만 동기화가 안되었다.</summary>
			NotSync,
			/// <summary>동기화가 된 상태</summary>
			Sync,
		}
		private SYNC_STATUS[] _syncStatusArr = new SYNC_STATUS[3];

		public List<apAnimKeyframe> _keyframes_All = new List<apAnimKeyframe>();
		//GL에 Curve 출력용
		private List<apAnimTimelineLayer> _timlineLayers = new List<apAnimTimelineLayer>();
		private List<apAnimCurveResult>[] _curveResultsArr = new List<apAnimCurveResult>[3];

		//두개의 키프레임을 저장하고, 두개의 커브(A, B)를 확인한다.
		//동기와 체크 및 동기화에 사용된다.
		public class CurveSet
		{
			public apAnimKeyframe _prevKeyframe = null;
			public apAnimKeyframe _nextKeyframe = null;

			public CurveSet(apAnimKeyframe prevKeyframe, apAnimKeyframe nextKeyframe)
			{
				_prevKeyframe = prevKeyframe;
				_nextKeyframe = nextKeyframe;
			}

			public bool IsSame(CurveSet target)
			{
				//Prev, Next간의 CurveResult의 값을 비교한다.
				apAnimCurve src_A = _prevKeyframe._curveKey;
				apAnimCurve src_B = _nextKeyframe._curveKey;

				apAnimCurve dst_A = target._prevKeyframe._curveKey;
				apAnimCurve dst_B = target._nextKeyframe._curveKey;

				//A의 Next와 B의 Prev끼리 묶어서 같은지 확인한다.
				if(src_A._nextTangentType != dst_A._nextTangentType ||
					src_B._prevTangentType != dst_B._prevTangentType)
				{
					return false;
				}

				apAnimCurve.TANGENT_TYPE srcTangentType = apAnimCurve.TANGENT_TYPE.Constant;
				apAnimCurve.TANGENT_TYPE dstTangentType = apAnimCurve.TANGENT_TYPE.Constant;

				//실제 탄젠트 타입 비교
				if(src_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Constant &&
					src_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Constant)
				{
					srcTangentType = apAnimCurve.TANGENT_TYPE.Constant;
				}
				else if(src_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Linear &&
					src_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Linear)
				{
					srcTangentType = apAnimCurve.TANGENT_TYPE.Linear;
				}
				else
				{
					srcTangentType = apAnimCurve.TANGENT_TYPE.Smooth;
				}

				if(dst_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Constant &&
					dst_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Constant)
				{
					dstTangentType = apAnimCurve.TANGENT_TYPE.Constant;
				}
				else if(dst_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Linear &&
					dst_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Linear)
				{
					dstTangentType = apAnimCurve.TANGENT_TYPE.Linear;
				}
				else
				{
					dstTangentType = apAnimCurve.TANGENT_TYPE.Smooth;
				}

				if(srcTangentType != dstTangentType)
				{
					return false;
				}

				if(srcTangentType == apAnimCurve.TANGENT_TYPE.Smooth &&
					dstTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
				{
					//Smooth의 경우, Smooth에 사용된 값도 같아야 한다.
					float bias = 0.001f;
					if(Mathf.Abs(src_A._nextSmoothX - dst_A._nextSmoothX) > bias ||
						Mathf.Abs(src_A._nextSmoothY - dst_A._nextSmoothY) > bias ||
						Mathf.Abs(src_B._prevSmoothX - dst_B._prevSmoothX) > bias ||
						Mathf.Abs(src_B._prevSmoothY - dst_B._prevSmoothY) > bias)
					{
						return false;
					}
				}

				return true;
			}
		}

		private List<CurveSet> _curveSets_All = new List<CurveSet>();
		private List<CurveSet>[] _curveSetsArr = new List<CurveSet>[3];

		private Dictionary<apAnimKeyframe, CurveSet> _prevKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();
		private Dictionary<apAnimKeyframe, CurveSet> _nextKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();

		//동기화된 커브 2개씩 세트 (배열로 묶음)
		private const int KEY_A = 0;
		private const int KEY_B = 1;

		public apAnimCurve[,] _syncAnimCurveArr = new apAnimCurve[3,2];

		public apAnimCurveResult GetSyncCurveResult(int iCurveType)
		{
			return _syncAnimCurveArr[iCurveType, KEY_A]._nextCurveResult;
		}

		public apAnimCurveResult SyncCurveResult_Prev
		{
			get { return _syncAnimCurveArr[CURVE__PREV, KEY_A]._nextCurveResult; }
		}
		public apAnimCurveResult SyncCurveResult_Mid
		{
			get { return _syncAnimCurveArr[CURVE__MID, KEY_A]._nextCurveResult; }
		}
		public apAnimCurveResult SyncCurveResult_Next
		{
			get { return _syncAnimCurveArr[CURVE__NEXT, KEY_A]._nextCurveResult; }
		}

		public apAnimCurve GetSyncCurve_Prev(int iCurveType)
		{
			return _syncAnimCurveArr[iCurveType, KEY_A];
		}
		public apAnimCurve GetSyncCurve_Next(int iCurveType)
		{
			return _syncAnimCurveArr[iCurveType, KEY_B];
		}
		//public apAnimCurve _syncCurve_Prev = new apAnimCurve();
		//public apAnimCurve _syncCurve_Next = new apAnimCurve();
		//public apAnimCurveResult SyncCurveResult
		//{
		//	get { return _syncCurve_Prev._nextCurveResult; }
		//}

		private bool _isAnyChangedRequest = false;


		// Init
		//----------------------------------------------------
		public apTimelineCommonCurve()
		{
			Clear();
		}


		public void Clear()
		{
			//이전
			//_syncStatus = SYNC_STATUS.NoKeyframes;

			//변경
			if(_syncStatusArr == null)
			{
				_syncStatusArr = new SYNC_STATUS[3];
			}
			_syncStatusArr[CURVE__PREV] = SYNC_STATUS.NoKeyframes;
			_syncStatusArr[CURVE__MID] = SYNC_STATUS.NoKeyframes;
			_syncStatusArr[CURVE__NEXT] = SYNC_STATUS.NoKeyframes;

			if (_keyframes_All == null)
			{
				_keyframes_All = new List<apAnimKeyframe>();
			}
			_keyframes_All.Clear();

			//타임라인 GL용
			if(_timlineLayers == null)
			{
				_timlineLayers = new List<apAnimTimelineLayer>();
			}
			_timlineLayers.Clear();
			
			if(_curveResultsArr == null)
			{
				_curveResultsArr = new List<apAnimCurveResult>[3];
			}
			if(_curveResultsArr[CURVE__PREV] == null) { _curveResultsArr[CURVE__PREV] = new List<apAnimCurveResult>(); }
			if(_curveResultsArr[CURVE__MID] == null) { _curveResultsArr[CURVE__MID] = new List<apAnimCurveResult>(); }
			if(_curveResultsArr[CURVE__NEXT] == null) { _curveResultsArr[CURVE__NEXT] = new List<apAnimCurveResult>(); }
			_curveResultsArr[CURVE__PREV].Clear();
			_curveResultsArr[CURVE__MID].Clear();
			_curveResultsArr[CURVE__NEXT].Clear();

			//이전
			//if (_curveSets == null)
			//{
			//	_curveSets = new List<CurveSet>();
			//}
			//_curveSets.Clear();
			
			//변경
			if(_curveSets_All == null)	{ _curveSets_All = new List<CurveSet>(); }
			_curveSets_All.Clear();

			if(_curveSetsArr == null)
			{
				_curveSetsArr = new List<CurveSet>[NUM_CURVE_TYPE];
			}

			for (int i = 0; i < NUM_CURVE_TYPE; i++)
			{
				if(_curveSetsArr[i] == null)
				{
					_curveSetsArr[i] = new List<CurveSet>();
				}
				_curveSetsArr[i].Clear();
			}
			
			if (_prevKey2CurveSet == null)
			{
				_prevKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();
			}
			if (_nextKey2CurveSet == null)
			{
				_nextKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();
			}
			_prevKey2CurveSet.Clear();
			_nextKey2CurveSet.Clear();

			_isAnyChangedRequest = false;

			//이전
			//_syncCurve_Prev.Init();
			//_syncCurve_Next.Init();

			//_syncCurve_Prev._keyIndex = 0;
			//_syncCurve_Next._keyIndex = 1;

			//_syncCurve_Prev._nextIndex = 1;
			//_syncCurve_Next._prevIndex = 0;

			//변경
			if(_syncAnimCurveArr == null)
			{
				_syncAnimCurveArr = new apAnimCurve[3, 2];
			}
			for (int iCurveType = 0; iCurveType < NUM_CURVE_TYPE; iCurveType++)
			{
				if(_syncAnimCurveArr[iCurveType, KEY_A] == null)
				{
					_syncAnimCurveArr[iCurveType, KEY_A] = new apAnimCurve();
				}
				if(_syncAnimCurveArr[iCurveType, KEY_B] == null)
				{
					_syncAnimCurveArr[iCurveType, KEY_B] = new apAnimCurve();
				}

				_syncAnimCurveArr[iCurveType, KEY_A].Init();
				_syncAnimCurveArr[iCurveType, KEY_B].Init();

				_syncAnimCurveArr[iCurveType, KEY_A]._keyIndex = 0;
				_syncAnimCurveArr[iCurveType, KEY_B]._keyIndex = 1;
				_syncAnimCurveArr[iCurveType, KEY_A]._nextIndex = 1;
				_syncAnimCurveArr[iCurveType, KEY_B]._prevIndex = 0;

				_syncAnimCurveArr[iCurveType, KEY_A]._nextCurveResult.Link(_syncAnimCurveArr[iCurveType, KEY_A], _syncAnimCurveArr[iCurveType, KEY_B], true);
				_syncAnimCurveArr[iCurveType, KEY_B]._prevCurveResult.Link(_syncAnimCurveArr[iCurveType, KEY_A], _syncAnimCurveArr[iCurveType, KEY_B], false);
				_syncAnimCurveArr[iCurveType, KEY_A].Refresh();
				_syncAnimCurveArr[iCurveType, KEY_B].Refresh();
			}

			//이전
			//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true, true);
			//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false, true);

			//변경 19.5.20 : MakeCurve를 항상 하는 걸로 변경
			//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true);
			//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false);

			//_syncCurve_Prev.Refresh();
			//_syncCurve_Next.Refresh();

			//다시 변경 19.12.30 : 3단계의 커브로 분리 (위쪽)
		}


		// Functions
		//----------------------------------------------------
		public void SetKeyframes(List<apAnimKeyframe> keyframes)
		{
			Clear();

			//키프레임들을 하나씩 돌면서 CurveSet에 넣자.
			//중복을 막기 위해서 Key2CurveSet 확인
			if(keyframes == null || keyframes.Count == 0)
			{
				return;
			}

			apAnimKeyframe srcKeyframe = null;
			apAnimKeyframe prevKey = null;
			apAnimKeyframe nextKey = null;
			for (int iKey = 0; iKey < keyframes.Count; iKey++)
			{
				srcKeyframe = keyframes[iKey];

				_keyframes_All.Add(srcKeyframe);
				if(!_timlineLayers.Contains(srcKeyframe._parentTimelineLayer))
				{
					_timlineLayers.Add(srcKeyframe._parentTimelineLayer);
				}

				prevKey = srcKeyframe._prevLinkedKeyframe;
				nextKey = srcKeyframe._nextLinkedKeyframe;

				srcKeyframe._curveKey.Refresh();

				if(prevKey != null && srcKeyframe != prevKey)
				{
					//Prev -> Src
					if(!_prevKey2CurveSet.ContainsKey(prevKey) && !_nextKey2CurveSet.ContainsKey(srcKeyframe))
					{
						//아직 등록되지 않은 키프레임들
						CurveSet newSet = new CurveSet(prevKey, srcKeyframe);
						_curveSets_All.Add(newSet);

						_prevKey2CurveSet.Add(prevKey, newSet);
						_nextKey2CurveSet.Add(srcKeyframe, newSet);
					}
				}

				if(nextKey != null && srcKeyframe != nextKey)
				{
					//Src -> Next
					if(!_prevKey2CurveSet.ContainsKey(srcKeyframe) && !_nextKey2CurveSet.ContainsKey(nextKey))
					{
						//아직 등록되지 않은 키프레임들
						CurveSet newSet = new CurveSet(srcKeyframe, nextKey);
						_curveSets_All.Add(newSet);

						_prevKey2CurveSet.Add(srcKeyframe, newSet);
						_nextKey2CurveSet.Add(nextKey, newSet);
					}
				}
			}

			if(_curveSets_All.Count <= 1)
			{
				Clear();
				return;
			}

			//추가 19.12.30 : 키프레임의 연결 상태를 보고 PREV, MID, NEXT를 구분하여 계산한다. (기존은 1개)
			CurveSet curCurveSet = null;
			apAnimKeyframe curCurvePrevKey = null;
			apAnimKeyframe curCurveNextKey = null;
			apAnimCurveResult curCurveResult = null;

			bool isKeyContain_Prev = false;
			bool isKeyContain_Next = false;

			for (int iSet = 0; iSet < _curveSets_All.Count; iSet++)
			{
				curCurveSet = _curveSets_All[iSet];
				curCurvePrevKey = curCurveSet._prevKeyframe;
				curCurveNextKey = curCurveSet._nextKeyframe;

				isKeyContain_Prev = false;
				isKeyContain_Next = false;

				if(curCurvePrevKey == null || curCurveNextKey == null)
				{
					continue;
				}

				curCurveResult = null;
				if(curCurvePrevKey._curveKey != null && curCurvePrevKey._curveKey._nextCurveResult != null)
				{
					curCurveResult = curCurvePrevKey._curveKey._nextCurveResult;
				}

				if(curCurvePrevKey != null)
				{
					if(_keyframes_All.Contains(curCurvePrevKey))
					{
						isKeyContain_Prev = true;
					}
				}

				if(curCurveNextKey != null)
				{
					if(_keyframes_All.Contains(curCurveNextKey))
					{
						isKeyContain_Next = true;
					}
				}

				if(isKeyContain_Prev && isKeyContain_Next)
				{
					//양쪽의 키프레임이 모두 선택된 상태 (|-|)
					//> 이 커브는 Mid 타입이다.
					_curveSetsArr[CURVE__MID].Add(curCurveSet);
					if(curCurveResult != null)
					{
						_curveResultsArr[CURVE__MID].Add(curCurveResult);
					}
					
				}
				else if(isKeyContain_Prev && !isKeyContain_Next)
				{
					//Prev 키프레임만 선택된 상태 (|->)
					//> 이 커브는 Next 타입이다.
					_curveSetsArr[CURVE__NEXT].Add(curCurveSet);
					if(curCurveResult != null)
					{
						_curveResultsArr[CURVE__NEXT].Add(curCurveResult);
					}
				}
				else if(!isKeyContain_Prev && isKeyContain_Next)
				{
					//Next 키프레임만 선택된 상태 (<-|)
					//> 이 커브는 Prev 타입이다.
					_curveSetsArr[CURVE__PREV].Add(curCurveSet);
					if(curCurveResult != null)
					{
						_curveResultsArr[CURVE__PREV].Add(curCurveResult);
					}
				}
				else
				{
					//키프레임이 둘다 없..응?
				}
			}

			//일단 동기화가 안됨
			//_syncStatus = SYNC_STATUS.NotSync;//이전
			for (int i = 0; i < NUM_CURVE_TYPE; i++)
			{
				_syncStatusArr[i] = SYNC_STATUS.NoKeyframes;
			}

			List<CurveSet> curCurveSetList = null;
			apAnimCurve curCurve_A = null;
			apAnimCurve curCurve_B = null;


			//변경 19.12.30 : 각각의 커브 타입에 대해서 동기화 여부 계산
			for (int iCurveType = 0; iCurveType < NUM_CURVE_TYPE; iCurveType++)
			{
				curCurveSetList = _curveSetsArr[iCurveType];
				if (curCurveSetList.Count == 0)
				{
					continue;
				}

				curCurve_A = _syncAnimCurveArr[iCurveType, KEY_A];
				curCurve_B = _syncAnimCurveArr[iCurveType, KEY_B];

				//커브값이 같은지 체크하자
				CurveSet firstSet = curCurveSetList[0];//<<첫번째것과 비교하자

				bool isAllSame = true;//<<모두 동일한 커브를 가졌는가

				CurveSet curSet = null;
				for (int i = 1; i < curCurveSetList.Count; i++)
				{
					curSet = curCurveSetList[i];
					if (!curSet.IsSame(firstSet))
					{
						//하나라도 다르다면
						isAllSame = false;
						break;
					}
				}

				if (isAllSame)
				{
					//오잉 모두 같았다.
					//공통 Curve를 만든다.
					_syncStatusArr[iCurveType] = SYNC_STATUS.Sync;

					//이전
					////공통 Curve를 만들자
					//_syncCurve_Prev._nextLinkedCurveKey = _syncCurve_Next;
					//_syncCurve_Next._prevLinkedCurveKey = _syncCurve_Prev;

					//_syncCurve_Prev._keyIndex = 0;
					//_syncCurve_Next._keyIndex = 1;

					//_syncCurve_Prev._nextIndex = 1;
					//_syncCurve_Next._prevIndex = 0;

					//apAnimCurve prevCurve = firstSet._prevKeyframe._curveKey;
					//apAnimCurve nextCurve = firstSet._nextKeyframe._curveKey;

					//_syncCurve_Prev._nextSmoothX = prevCurve._nextSmoothX;
					//_syncCurve_Prev._nextSmoothY = prevCurve._nextSmoothY;
					//_syncCurve_Prev._nextTangentType = prevCurve._nextTangentType;

					//_syncCurve_Next._prevSmoothX = nextCurve._prevSmoothX;
					//_syncCurve_Next._prevSmoothY = nextCurve._prevSmoothY;
					//_syncCurve_Next._prevTangentType = nextCurve._prevTangentType;

					////이전
					////_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true, true);
					////_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false, true);

					////변경 19.5.20 : MakeCurve를 항상 수행
					//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true);
					//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false);

					//_syncCurve_Prev.Refresh();
					//_syncCurve_Next.Refresh();

					//_syncCurve_Prev._nextCurveResult.MakeCurve();
					//_syncCurve_Next._prevCurveResult.MakeCurve();

					//변경
					//공통 Curve를 만들자
					curCurve_A._nextLinkedCurveKey = curCurve_B;
					curCurve_B._prevLinkedCurveKey = curCurve_A;

					curCurve_A._keyIndex = 0;
					curCurve_B._keyIndex = 1;

					curCurve_A._nextIndex = 1;
					curCurve_B._prevIndex = 0;

					apAnimCurve prevCurve = firstSet._prevKeyframe._curveKey;
					apAnimCurve nextCurve = firstSet._nextKeyframe._curveKey;

					curCurve_A._nextSmoothX = prevCurve._nextSmoothX;
					curCurve_A._nextSmoothY = prevCurve._nextSmoothY;
					curCurve_A._nextTangentType = prevCurve._nextTangentType;

					curCurve_B._prevSmoothX = nextCurve._prevSmoothX;
					curCurve_B._prevSmoothY = nextCurve._prevSmoothY;
					curCurve_B._prevTangentType = nextCurve._prevTangentType;

					//이전
					//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true, true);
					//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false, true);

					//변경 19.5.20 : MakeCurve를 항상 수행
					curCurve_A._nextCurveResult.Link(curCurve_A, curCurve_B, true);
					curCurve_B._prevCurveResult.Link(curCurve_A, curCurve_B, false);

					curCurve_A.Refresh();
					curCurve_B.Refresh();

					curCurve_A._nextCurveResult.MakeCurve();
					curCurve_B._prevCurveResult.MakeCurve();
				}
				else
				{
					//몇개가 다르다.
					_syncStatusArr[iCurveType] = SYNC_STATUS.NotSync;

					curCurve_A.Refresh();
					curCurve_B.Refresh();
				}
			}
			
		}

		//--------------------------------------------------------
		/// <summary>
		/// 커브의 탄젠트를 설정
		/// </summary>
		/// <param name="tangentType"></param>
		/// <param name="curveType">0 : Prev, 1 : Middle, 2 : Next</param>
		public void SetTangentType(apAnimCurve.TANGENT_TYPE tangentType, int iCurveType)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.Sync)
			{
				return;
			}


			GetSyncCurveResult(iCurveType).SetTangent(tangentType);

			SetChanged();
			ApplySync(iCurveType, true, false);
		}

		public void SetCurvePreset_Default(int iCurveType)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.Sync)
			{
				return;
			}

			GetSyncCurveResult(iCurveType).SetCurvePreset_Default();

			SetChanged();
			ApplySync(iCurveType, true, false);
		}

		public void SetCurvePreset_Hard(int iCurveType)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.Sync)
			{
				return;
			}

			GetSyncCurveResult(iCurveType).SetCurvePreset_Hard();

			SetChanged();
			ApplySync(iCurveType, true, false);
		}

		public void SetCurvePreset_Acc(int iCurveType)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.Sync)
			{
				return;
			}

			GetSyncCurveResult(iCurveType).SetCurvePreset_Acc();

			SetChanged();
			ApplySync(iCurveType, true, false);
		}

		public void SetCurvePreset_Dec(int iCurveType)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.Sync)
			{
				return;
			}

			GetSyncCurveResult(iCurveType).SetCurvePreset_Dec();

			SetChanged();
			ApplySync(iCurveType, true, false);
		}

		public void ResetSmoothSetting(int iCurveType)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.Sync)
			{
				return;
			}

			GetSyncCurveResult(iCurveType).ResetSmoothSetting();

			SetChanged();
			ApplySync(iCurveType, true, false);
		}
		//--------------------------------------------------------

		//설정이 바뀌었음을 알려준다.
		//이 함수가 호출되어야 ApplySync가 동작한다.
		public void SetChanged()
		{	
			_isAnyChangedRequest = true;
		}


		

		public void ApplySync(int iCurveType, bool isApplyForce, bool isMousePressed)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.Sync || !_isAnyChangedRequest)
			{
				return;
			}

			if(_curveSetsArr[iCurveType] == null || _curveSetsArr[iCurveType].Count == 0)
			{
				return;
			}

			//isApplyForce = true이거나
			//isMousePressed = false일때 Apply를 한다.
			if(!isApplyForce && isMousePressed)
			{
				return;
			}

			apEditorUtil.SetEditorDirty();
			
			List<CurveSet> curveSetList = _curveSetsArr[iCurveType];
			apAnimCurve animCurve_Prev = _syncAnimCurveArr[iCurveType, KEY_A];
			apAnimCurve animCurve_Next = _syncAnimCurveArr[iCurveType, KEY_B];

			animCurve_Prev._nextCurveResult.MakeCurve();
			animCurve_Next._prevCurveResult.MakeCurve();

			//동기화를 적용하자
			CurveSet curSet = null;
			apAnimCurve prevCurve = null;
			apAnimCurve nextCurve = null;
			for (int i = 0; i < curveSetList.Count; i++)
			{
				curSet = curveSetList[i];

				if(curSet._prevKeyframe == null || curSet._nextKeyframe == null)
				{
					continue;
				}
				prevCurve = curSet._prevKeyframe._curveKey;
				nextCurve = curSet._nextKeyframe._curveKey;

				//공통 커브와 동일하게 만들자.
				//Prev의 Next와 Next의 Prev.. 아 헷갈려;;
				prevCurve._nextTangentType = animCurve_Prev._nextTangentType;
				prevCurve._nextSmoothX = animCurve_Prev._nextSmoothX;
				prevCurve._nextSmoothY = animCurve_Prev._nextSmoothY;

				nextCurve._prevTangentType = animCurve_Next._prevTangentType;
				nextCurve._prevSmoothX = animCurve_Next._prevSmoothX;
				nextCurve._prevSmoothY = animCurve_Next._prevSmoothY;

				prevCurve.Refresh();
				nextCurve.Refresh();
			}

			_isAnyChangedRequest = false;
			
		}



		public void NotSync2SyncStatus(int iCurveType)
		{
			if(_syncStatusArr[iCurveType] != SYNC_STATUS.NotSync)
			{
				return;
			}


			_syncStatusArr[iCurveType] = SYNC_STATUS.Sync;

			//공통 Curve를 만들자
			apAnimCurve animCurve_Prev = _syncAnimCurveArr[iCurveType, KEY_A];
			apAnimCurve animCurve_Next = _syncAnimCurveArr[iCurveType, KEY_B];

			animCurve_Prev._nextLinkedCurveKey = animCurve_Next;
			animCurve_Next._prevLinkedCurveKey = animCurve_Prev;

			animCurve_Prev._keyIndex = 0;
			animCurve_Next._keyIndex = 1;

			animCurve_Prev._nextIndex = 1;
			animCurve_Next._prevIndex = 0;

			//이전
			//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true, true);
			//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false, true);

			//변경 19.5.20 : MakeCurve를 항상 수행
			animCurve_Prev._nextCurveResult.Link(animCurve_Prev, animCurve_Next, true);
			animCurve_Next._prevCurveResult.Link(animCurve_Prev, animCurve_Next, false);

			//Smooth 상태로 리셋한다.
			GetSyncCurveResult(iCurveType).ResetSmoothSetting();
			
			//동기화를 한다.
			SetChanged();
			ApplySync(iCurveType, true, false);
		}

		// Get / Set
		//----------------------------------------------------
		public SYNC_STATUS GetSyncStatus(int iCurveType)
		{
			return _syncStatusArr[iCurveType];
		}


		public bool IsCurveResultContains(apAnimCurveResult curveResult, int iCurveType)
		{
			if(_curveResultsArr[iCurveType].Count > 0)
			{
				return _curveResultsArr[iCurveType].Contains(curveResult);
			}
			return false;
		}

		public bool IsSelectedTimelineLayer(apAnimTimelineLayer timelineLayer)
		{
			if(_timlineLayers.Count > 0)
			{
				return _timlineLayers.Contains(timelineLayer);
			}
			return false;
		}
	}
}