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
using System.Text;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 20.11.24 : 애니메이션 키프레임 조회 성능을 높이기 위한 클래스
	/// 이전과 달리 for를 최소한으로 사용하여 바로 키프레임 (=ParamKeyValue) 1개 혹은 2개를 리턴한다.
	/// AnimClip의 int형 모든 프레임마다 미리 ParamKeyValue 세트를 저장한다.
	/// 메모리를 차지하지만 많아봐야 MB 단위가 될 것
	/// </summary>
	public class apOptCalculatedAnimKeyLUT
	{
		// Members
		//---------------------------------------------------
		public apAnimClip _linkedAnimClip = null;
		public apAnimTimelineLayer _linkedTimelineLayer = null;


		public class LUTUnit
		{
			public apAnimKeyframe _keyframe_Cur = null;
			public apAnimKeyframe _keyframe_Next = null;
			public apOptCalculatedResultParam.OptParamKeyValueSet _paramKeyValueSet_Cur = null;
			public apOptCalculatedResultParam.OptParamKeyValueSet _paramKeyValueSet_Next = null;
			//모디파이어에 저장된 PKV의 인덱스
			public int _iParamKeyValueSet_Cur = -1;
			public int _iParamKeyValueSet_Next = -1;

			//먼저 Prev 위주로 생성
			public LUTUnit(apAnimKeyframe keyframe, apOptCalculatedResultParam.OptParamKeyValueSet paramKeyValue, int iParamKeyValue)
			{
				_keyframe_Cur = keyframe;
				_paramKeyValueSet_Cur = paramKeyValue;
				_iParamKeyValueSet_Cur = iParamKeyValue;
			}


			public void SetNextKeyframe(apAnimKeyframe keyframe_Next, apOptCalculatedResultParam.OptParamKeyValueSet paramKeyValue_Next, int iParamKeyValue_Next)
			{	
				_keyframe_Next = keyframe_Next;
				_paramKeyValueSet_Next = paramKeyValue_Next;
				_iParamKeyValueSet_Next = iParamKeyValue_Next;
			}
		}


		private LUTUnit[] _LUT = null;//배열로 만들어야 빠르다.
		private int _startFrame = 0;//애니메이션의 시작 프레임을 알아야 Offset을 구한다.
		private int _endFrame = 0;//애니메이션의 시작 프레임을 알아야 Offset을 구한다.
		private int _lutLength = 0;
		private bool _isLoop = false;
		
		private bool _isLUTAvailable = false;//데이터가 없을 수 있다.




		// Init
		//---------------------------------------------------
		public apOptCalculatedAnimKeyLUT(apAnimClip animClip, apAnimTimelineLayer timelineLayer)
		{
			_linkedAnimClip = animClip;
			_linkedTimelineLayer = timelineLayer;

			//LUT 배열을 만들자 (일단 비어있을 것)
			_startFrame = _linkedAnimClip.StartFrame;
			_endFrame = _linkedAnimClip.EndFrame;
			_isLoop = _linkedAnimClip.IsLoop;

			_lutLength = (_endFrame - _startFrame) + 1;

			_LUT = new LUTUnit[_lutLength];
		}

		// Functions
		//---------------------------------------------------
		
		//Make LUT

		//이 함수는 SubList의 MakeMetaData에서 만들자
		public void MakeLUT(apOptCalculatedResultParamSubList subList)
		{
			if(subList._subParamKeyValues.Count == 0)
			{
				//만약 키프레임들이 없다면
				_isLUTAvailable = false;
				return;
			}
			



			//일단 subList의 ParamSetValue를 모두 넣자
			List<LUTUnit> unitList = new List<LUTUnit>();

			apOptCalculatedResultParam.OptParamKeyValueSet curParamKeyValueSet = null;
			apAnimKeyframe curKeyframe = null;
			for (int i = 0; i < subList._subParamKeyValues.Count; i++)
			{
				curParamKeyValueSet = subList._subParamKeyValues[i];
				curKeyframe = curParamKeyValueSet._paramSet.SyncKeyframe;

				if(!curKeyframe._isActive)
				{
					//유효하지 않은 키프레임은 LUT에서 제외
					continue;
				}

				LUTUnit newUnit = new LUTUnit(curKeyframe, curParamKeyValueSet, i);
				unitList.Add(newUnit);
			}

			//정렬을 한다. (키프레임 위치에 따라 오름차순)
			unitList.Sort(delegate (LUTUnit a, LUTUnit b)
			{
				return a._keyframe_Cur._frameIndex - b._keyframe_Cur._frameIndex;
			});

			//일단 앞뒤로 연결을 하자.
			LUTUnit curUnit = null;
			LUTUnit nextUnit = null;
			for (int i = 0; i < unitList.Count - 1; i++)
			{
				curUnit = unitList[i];
				nextUnit = unitList[i + 1];

				curUnit.SetNextKeyframe(nextUnit._keyframe_Cur, nextUnit._paramKeyValueSet_Cur, nextUnit._iParamKeyValueSet_Cur);
			}

			
			if (_isLoop)
			{
				//루프라면 Last > First로 묶기
				curUnit = unitList[unitList.Count - 1];
				nextUnit = unitList[0];

				curUnit.SetNextKeyframe(nextUnit._keyframe_Cur, nextUnit._paramKeyValueSet_Cur, nextUnit._iParamKeyValueSet_Cur);
			}
			else
			{
				//루프가 아니면 Last 혼자서 Prev~Next 처리
				curUnit = unitList[unitList.Count - 1];
				curUnit.SetNextKeyframe(curUnit._keyframe_Cur, curUnit._paramKeyValueSet_Cur, curUnit._iParamKeyValueSet_Cur);
			}

			//이제 배열에 LUT를 넣자
			
			int iLUT_Start = 0;
			int iLUT_End = 0;

			//일단 마지막 전 LUT Unit까지 체크
			for (int i = 0; i < unitList.Count - 1; i++)
			{
				curUnit = unitList[i];
				iLUT_Start = curUnit._keyframe_Cur._frameIndex - _startFrame;
				iLUT_End = curUnit._keyframe_Next._frameIndex - _startFrame;

				for (int iLUT = iLUT_Start; iLUT < iLUT_End; iLUT++)
				{
					if (iLUT < 0 || iLUT >= _lutLength)
					{
						continue;
					}

					_LUT[iLUT] = curUnit;
				}
			}

			//이제 "<~첫 키프레임" / "마지막 키프레임~>"을 계산하자
			LUTUnit lastUnit = unitList[unitList.Count - 1];
			LUTUnit firstUnit = unitList[0];

			int iListFirstLUT = firstUnit._keyframe_Cur._frameIndex - _startFrame;
			int iListLastLUT = lastUnit._keyframe_Cur._frameIndex - _startFrame;
			
			if (_isLoop)
			{
				//루프라면, "첫 키프레임 이전"과 "마지막 키프레임 이후"에 마지막 LUT Unit을 넣는다.
				for (int iLUT = 0; iLUT < iListFirstLUT; iLUT++)
				{
					if (iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = lastUnit;
				}

				for (int iLUT = iListLastLUT; iLUT < _lutLength; iLUT++)
				{
					if (iLUT < 0 || iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = lastUnit;
				}
			}
			else
			{
				//루프가 아니라면
				//"첫 키프레임 이전"에는 "첫 키프레임"으로만 구성된 LUT Unit 생성 후 대입
				//"마지막 키프레임 이후"에는 마지막 LUT Unit을 넣는다.

				LUTUnit preUnit = new LUTUnit(firstUnit._keyframe_Cur, firstUnit._paramKeyValueSet_Cur, firstUnit._iParamKeyValueSet_Cur);
				preUnit.SetNextKeyframe(firstUnit._keyframe_Cur, firstUnit._paramKeyValueSet_Cur, firstUnit._iParamKeyValueSet_Cur);//자기 자신

				for (int iLUT = 0; iLUT < iListFirstLUT; iLUT++)
				{
					if (iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = preUnit;
				}

				for (int iLUT = iListLastLUT; iLUT < _lutLength; iLUT++)
				{
					if (iLUT < 0 || iLUT >= _lutLength) { continue; }

					_LUT[iLUT] = lastUnit;
				}
			}

			_isLUTAvailable = true;

			//디버그
			//Debug.LogError("LUT Make : (" + _linkedAnimClip._name + " > " + _linkedTimelineLayer.DisplayName + ")");
			
			//StringBuilder sb = new StringBuilder(1000);
			//string space = " ";
			//for (int i = 0; i < _lutLength; i++)
			//{	
			//	sb.Append(_LUT[i]._keyframe_Cur._frameIndex);
			//	sb.Append(space);
			//}
			//Debug.Log(sb.ToString());
		}


		/// <summary>
		/// LUT를 리턴한다. 매우! 빠름! (순회따위.. ㅂㄷㅂㄷ)
		/// </summary>
		/// <param name="curAnimFrame"></param>
		/// <returns></returns>
		public LUTUnit GetLUT(int curAnimFrame)
		{
			if(!_isLUTAvailable)
			{
				return null;
			}

			return _LUT[(curAnimFrame - _startFrame) % _lutLength];
		}
	}
}