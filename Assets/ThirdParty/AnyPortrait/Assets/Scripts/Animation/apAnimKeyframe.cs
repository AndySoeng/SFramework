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

	[Serializable]
	public class apAnimKeyframe
	{
		// Members
		//-----------------------------------------------------------------------
		public int _uniqueID = -1;//<<키프레임마다 unique ID가 있다.

		public int _frameIndex = -1;//<<어디에 배치되는가 (겹치면 안되며, 겹치면 하나가 삭제되어야함)

		[NonSerialized]
		public apAnimTimelineLayer _parentTimelineLayer = null;

		[SerializeField]
		public apAnimCurve _curveKey = new apAnimCurve();

		[NonSerialized]
		public apAnimKeyframe _prevLinkedKeyframe = null;

		[NonSerialized]
		public apAnimKeyframe _nextLinkedKeyframe = null;

		/// <summary>애니메이션 보간을 위해 연동된 값을 입력했는가 (그렇지 않다면 상대적인 보간 처리가 들어간다)</summary>
		public bool _isKeyValueSet = false;

		/// <summary>
		/// 이 키프레임은 활성화되어있는가 [AnimClip의 재생 영역 밖이면 비활성화되며 링크되지 않는다]
		/// </summary>
		public bool _isActive = false;

		//루프 양쪽의 프레임인 경우
		//해당 프레임은 더미 프레임으로 설정될 수 있다.
		//(Start Frame은) OverEndDummy로 설정된다. -> DummyIndex가 EndIndex 또는 그 이상에 붙는다.
		//(End Frame은) UnderStartDummy로 설정된다. -> DummyIndex가 StartIndex 또는 그 이하에 붙는다.
		public bool _isLoopAsStart = false;
		public bool _isLoopAsEnd = false;


		public int _loopFrameIndex = -1;

		// 변경 19.5.20 : 용량 최적화를 위해서 activeFrame 변수들은 NonSerialized로 변경
		//값이 적용되는 프레임 범위
		[NonSerialized, NonBackupField]
		public int _activeFrameIndexMin = 0;
		[NonSerialized, NonBackupField]
		public int _activeFrameIndexMax = 0;

		[NonSerialized, NonBackupField]
		public int _activeFrameIndexMin_Dummy = 0;
		[NonSerialized, NonBackupField]
		public int _activeFrameIndexMax_Dummy = 0;



		//추가 : Transform + 회전 중 360도 회전을 지원해야한다.
		/// <summary>
		/// Transform + 회전시 실제 값에 추가되는 회전 값.
		/// 시계 방향, 반대방향을 추가할 수 있다.
		/// 각 키 별로 Prev, Next를 두어서 처리한다.
		/// </summary>
		public enum ROTATION_BIAS
		{
			None = 0,
			/// <summary>시계 방향 회전 +360 x n</summary>
			CW = 1,
			/// <summary>반시계 방향 회전 -360 x n</summary>
			CCW = 2
		}

		[SerializeField]
		public ROTATION_BIAS _prevRotationBiasMode = ROTATION_BIAS.None;

		[SerializeField]
		public ROTATION_BIAS _nextRotationBiasMode = ROTATION_BIAS.None;

		[SerializeField]
		public int _prevRotationBiasCount = 0;

		[SerializeField]
		public int _nextRotationBiasCount = 0;


		//Control Param 타입이면 
		//Control Param의 어떤 값에 동기화되는가
		public int _conSyncValue_Int = 0;
		public float _conSyncValue_Float = 0.0f;
		public Vector2 _conSyncValue_Vector2 = Vector2.zero;


		//에디터용 빠른 접근 위한 변수
		[NonSerialized]
		public apModifierParamSet _linkedParamSet_Editor = null;

		[NonSerialized]
		public apModifiedMesh _linkedModMesh_Editor = null;

		[NonSerialized]
		public apModifiedBone _linkedModBone_Editor = null;


		public enum LINKED_KEY
		{
			Prev = 0, Next = 1
		}

		// Init
		//-----------------------------------------------------------------------
		public apAnimKeyframe()
		{

		}

		//추가 22.1.8 : 키프레임 생성시 컨트롤 파라미터의 기본값이 무시되는 버그를 해결하기 위해 Init 함수에서 기본값을 아예 넣어주자
		public void Init_ControlParam(int uniqueID, int frameIndex, bool isIntControlParamLayer, apControlParam targetControlParam)
		{
			Init(uniqueID, frameIndex, isIntControlParamLayer);

			//기본값을 할당
			_conSyncValue_Int = targetControlParam._int_Def;
			_conSyncValue_Float = targetControlParam._float_Def;
			_conSyncValue_Vector2 = targetControlParam._vec2_Def;
		}


		public void Init_Modifier(int uniqueID, int frameIndex, bool isIntControlParamLayer)
		{
			Init(uniqueID, frameIndex, isIntControlParamLayer);
		}

		private void Init(int uniqueID, int frameIndex, bool isIntControlParamLayer)
		{
			_uniqueID = uniqueID;
			_frameIndex = frameIndex;

			_isLoopAsStart = false;
			_isLoopAsEnd = false;
			_loopFrameIndex = -1;

			//_conSyncValue_Bool = false;
			_conSyncValue_Int = 0;
			_conSyncValue_Float = 0.0f;
			_conSyncValue_Vector2 = Vector2.zero;
			//_conSyncValue_Vector3 = Vector3.zero;
			//_conSyncValue_Color = Color.black;

			_linkedParamSet_Editor = null;
			_linkedModMesh_Editor = null;
			_linkedModBone_Editor = null;

			if(isIntControlParamLayer)
			{
				_curveKey.SetTangentType(apAnimCurve.TANGENT_TYPE.Constant, apAnimCurve.KEY_POS.NEXT);
				_curveKey.SetTangentType(apAnimCurve.TANGENT_TYPE.Constant, apAnimCurve.KEY_POS.PREV);
			}

			_prevRotationBiasMode = ROTATION_BIAS.None;
			_nextRotationBiasMode = ROTATION_BIAS.None;
			_prevRotationBiasCount = 0;
			_nextRotationBiasCount = 0;
		}





		public void LinkModMesh_Editor(apModifierParamSet paramSet, apModifiedMesh modMesh)
		{
			_linkedParamSet_Editor = paramSet;
			_linkedModMesh_Editor = modMesh;
			_linkedModBone_Editor = null;
		}

		public void LinkModBone_Editor(apModifierParamSet paramSet, apModifiedBone modBone)
		{
			_linkedParamSet_Editor = paramSet;
			_linkedModMesh_Editor = null;
			_linkedModBone_Editor = modBone;
		}


		public void Link(apAnimTimelineLayer parentTimelineLayer)
		{
			_parentTimelineLayer = parentTimelineLayer;
			_parentTimelineLayer._parentAnimClip._portrait.RegistUniqueID(apIDManager.TARGET.AnimKeyFrame, _uniqueID);
		}

		public void SetInactive()
		{
			_isActive = false;
			_prevLinkedKeyframe = null;
			_nextLinkedKeyframe = null;
			_curveKey.SetLinkedCurveKey(null, null, _frameIndex, _frameIndex);


			_isLoopAsStart = false;
			_isLoopAsEnd = false;

			_loopFrameIndex = -1;
		}


		//public void SetLinkedKeyframes(apAnimKeyframe prevKeyframe, apAnimKeyframe nextKeyframe, bool isPrevDummyIndex, bool isNextDummyIndex)
		public void SetLinkedKeyframes(apAnimKeyframe prevKeyframe, apAnimKeyframe nextKeyframe, int prevFrameIndex, int nextFrameIndex)
		{
			_isActive = true;
			_prevLinkedKeyframe = prevKeyframe;
			_nextLinkedKeyframe = nextKeyframe;

			apAnimCurve prevCurveKey = null;
			apAnimCurve nextCurveKey = null;

			if (_prevLinkedKeyframe != null)
			{
				prevCurveKey = _prevLinkedKeyframe._curveKey;
			}
			if (_nextLinkedKeyframe != null)
			{
				nextCurveKey = _nextLinkedKeyframe._curveKey;
			}

			//_isLoopAsStart = false;
			//_isLoopAsEnd = false;
			//_loopFrameIndex = -1;

			//이전
			//_curveKey.SetLinkedCurveKey(prevCurveKey, nextCurveKey, prevFrameIndex, nextFrameIndex, isMakeCurveForce);

			//변경 19.5.20
			_curveKey.SetLinkedCurveKey(prevCurveKey, nextCurveKey, prevFrameIndex, nextFrameIndex);
		}


		/// <summary>
		/// 해당 프레임은 루프의 양쪽에 위치하여 더미프레임이 생성된다.
		/// StartFrame은 OverEnd 더미를 생성한다. (파라미터 True이며 인덱스를 +Length한다.
		/// EndFrame은 UnderStart 더미를 생성한다. (파라미터 False이며 인덱스를 -Length한다.
		/// </summary>
		/// <param name="isLoopAsStart"></param>
		/// <param name="dummyFrameIndex"></param>
		public void SetLoopFrame(bool isLoopAsStart, int dummyFrameIndex)
		{
			if (isLoopAsStart)
			{
				_isLoopAsStart = true;
				_isLoopAsEnd = false;
			}
			else
			{
				_isLoopAsStart = false;
				_isLoopAsEnd = true;
			}

			_loopFrameIndex = dummyFrameIndex;
			//if(isLoopAsStart)
			//{
			//	Debug.Log("Loop Start [" + _frameIndex + " > " + _loopFrameIndex + " ]");
			//}
			//if(_isLoopAsEnd)
			//{
			//	Debug.Log("Loop End [" + _frameIndex + " > " + _loopFrameIndex + " ]");
			//}

			//_curveKey.SetKeyIndex(_frameIndex, _loopFrameIndex);
			_curveKey.SetKeyIndex(_frameIndex);
		}

		public void SetDummyDisable()
		{
			_isLoopAsStart = false;
			_isLoopAsEnd = false;
			_loopFrameIndex = _frameIndex;
		}

		public bool IsFrameIn(int curFrame, LINKED_KEY linkedType)
		{
			if (linkedType == LINKED_KEY.Prev)
			{
				if (_activeFrameIndexMin <= curFrame && curFrame <= _frameIndex)
				{
					return true;
				}
				if (_isLoopAsStart || _isLoopAsEnd)
				{
					if (_activeFrameIndexMin_Dummy <= curFrame && curFrame <= _loopFrameIndex)
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				if (_frameIndex <= curFrame && curFrame <= _activeFrameIndexMax)
				{
					return true;
				}
				if (_isLoopAsStart || _isLoopAsEnd)
				{
					if (_loopFrameIndex <= curFrame && curFrame <= _activeFrameIndexMax_Dummy)
					{
						return true;
					}
				}
				return false;
			}
			//return false;
		}



		// Functions
		//-----------------------------------------------------------------------
		// 키프레임에서 "연동된 데이터"의 표면적인 값을 넣거나 상대적 처리임을 명시해주자
		public void SetKeyValue(float keyValue)
		{
			//_curveKey.SetKeyValue()
		}
		public void SetKeyValueRelative()
		{
			_isKeyValueSet = false;
		}



		public void RefreshCurveKey()
		{
			//int dummyFrameIndex = _frameIndex;
			//if (_isLoopAsStart || _isLoopAsEnd)
			//{
			//	dummyFrameIndex = _loopFrameIndex;
			//}
			//_curveKey.SetKeyIndex(_frameIndex, dummyFrameIndex);
			_curveKey.SetKeyIndex(_frameIndex);
			//_curveKey.CalculateSmooth();
		}
		// Get / Set
		//-----------------------------------------------------------------------


		//-----------------------------------------------------------------------------------
		// Copy For Bake
		//-----------------------------------------------------------------------------------
		public void CopyFromKeyframe(apAnimKeyframe srcKeyframe)
		{
			_uniqueID = srcKeyframe._uniqueID;

			_frameIndex = srcKeyframe._frameIndex;
			_curveKey = new apAnimCurve(srcKeyframe._curveKey, srcKeyframe._frameIndex);

			_isKeyValueSet = srcKeyframe._isKeyValueSet;

			_isActive = srcKeyframe._isActive;

			_isLoopAsStart = srcKeyframe._isLoopAsStart;
			_isLoopAsEnd = srcKeyframe._isLoopAsEnd;

			_loopFrameIndex = srcKeyframe._loopFrameIndex;

			_activeFrameIndexMin = srcKeyframe._activeFrameIndexMin;
			_activeFrameIndexMax = srcKeyframe._activeFrameIndexMax;

			_activeFrameIndexMin_Dummy = srcKeyframe._activeFrameIndexMin_Dummy;
			_activeFrameIndexMax_Dummy = srcKeyframe._activeFrameIndexMax_Dummy;

			_conSyncValue_Int = srcKeyframe._conSyncValue_Int;
			_conSyncValue_Float = srcKeyframe._conSyncValue_Float;
			_conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;


			//[v1.4.3] 버그 수정 (코드가 누락되어있었다.)
			_prevRotationBiasMode = srcKeyframe._prevRotationBiasMode;
			_nextRotationBiasMode = srcKeyframe._nextRotationBiasMode;
			_prevRotationBiasCount = srcKeyframe._prevRotationBiasCount;
			_nextRotationBiasCount = srcKeyframe._nextRotationBiasCount;
			
		}
	}

}