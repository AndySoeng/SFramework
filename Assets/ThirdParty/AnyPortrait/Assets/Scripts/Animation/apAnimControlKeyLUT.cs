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
	/// 추가 20.11.26 : 애니메이션 타임라인 중, "컨트롤 파라미터"를 제어하는 타임라인에 대한 LUT
	/// apAnimClip의 Opt 코드에서 동작하므로, 이 클래스도 apAnimClip에 속한다. 정확히는 ControlTimelineLayer
	/// 대부분의 코드는 apOptCalculatedAnimKeyLUT를 참고했다.
	/// </summary>
	public class apAnimControlKeyLUT
	{
		// Member
		//---------------------------------------------------
		public apAnimClip _linkedAnimClip = null;
		public apAnimTimelineLayer _linkedTimelineLayer = null;

		public class LUTUnit
		{
			public apAnimKeyframe _keyframe_Cur = null;
			public apAnimKeyframe _keyframe_Next = null;
			
			
			//먼저 Prev 위주로 생성
			public LUTUnit(apAnimKeyframe keyframe)
			{
				_keyframe_Cur = keyframe;
			}


			public void SetNextKeyframe(apAnimKeyframe keyframe_Next)
			{	
				_keyframe_Next = keyframe_Next;
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
		public apAnimControlKeyLUT(apAnimClip animClip, apAnimTimelineLayer timelineLayer)
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
		// Make LUT

		public void MakeLUT()
		{
			if(_linkedTimelineLayer._keyframes == null || _linkedTimelineLayer._keyframes.Count == 0)
			{
				_isLUTAvailable = false;

			}



			//일단 subList의 ParamSetValue를 모두 넣자
			List<LUTUnit> unitList = new List<LUTUnit>();

			apAnimKeyframe curKeyframe = null;
			for (int iKeyframe = 0; iKeyframe < _linkedTimelineLayer._keyframes.Count; iKeyframe++)
			{
				curKeyframe = _linkedTimelineLayer._keyframes[iKeyframe];
				if(!curKeyframe._isActive)
				{
					continue;
				}

				LUTUnit newUnit = new LUTUnit(curKeyframe);
				unitList.Add(newUnit);
			}

			if(unitList.Count == 0)
			{
				//Debug.Log("LUT가 없다.");
				return;
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

				curUnit.SetNextKeyframe(nextUnit._keyframe_Cur);
			}

			
			if (_isLoop)
			{
				//루프라면 Last > First로 묶기
				curUnit = unitList[unitList.Count - 1];
				nextUnit = unitList[0];

				curUnit.SetNextKeyframe(nextUnit._keyframe_Cur);
			}
			else
			{
				//루프가 아니면 Last 혼자서 Prev~Next 처리
				curUnit = unitList[unitList.Count - 1];
				curUnit.SetNextKeyframe(curUnit._keyframe_Cur);
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

				LUTUnit preUnit = new LUTUnit(firstUnit._keyframe_Cur);
				preUnit.SetNextKeyframe(firstUnit._keyframe_Cur);//자기 자신

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
		}


		// Get
		//---------------------------------------------------
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