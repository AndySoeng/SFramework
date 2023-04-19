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
	/// 추가 20.11.23 : 
	/// 최적화를 위해서 AnimPlayUnit > Modifier-CalResultParam-SubList까지 이어지는 매핑 정보 제공.
	/// 이건 Portrait와 Modifier에서 바로 접근 가능해야한다.
	/// index를 기준으로 AnimPlayUnit (AnimClip)에 바로 접근할 수 있어야 한다.
	/// - AnimClip ~ index 전환
	/// - 플레이/레이어 정보가 포함된 AnimClip 메타 정보
	/// </summary>
	public class apAnimPlayMapping
	{
		// Members
		//---------------------------------------------------------
		public apPortrait _portrait = null;

		//애니메이션 클립 연결 정보
		public Dictionary<int, apAnimClip> _index2AnimClip = null;
		public Dictionary<apAnimClip, int> _animClip2Index = null;
		public int _nAnimClips = 0;

		//플레이 정보 (전체 / Live)
		public class LiveUnit
		{
			//고정 데이터
			public int _animIndex = -1;
			public apAnimClip _linkedAnimClip = null;

			//라이브 데이터
			public bool _isLive = false;//재생중인가
			public int _playIndex = -1;//SubList의 _layerIndex에 해당한다. Sort용
			public float _playWeight = 0.0f;//SubList의 _layerWeight에 해당한다.
			public apModifierParamSetGroup.BLEND_METHOD _blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;//SubList의 _blendMethod
			
			public LiveUnit(int animIndex, apAnimClip linkedAnimClip)
			{
				_animIndex = animIndex;
				_linkedAnimClip = linkedAnimClip;

				Init();
			}

			private void Init()
			{
				_isLive = false;//재생중인가
				_playIndex = -10;//SubList의 _layerIndex에 해당한다. Sort용
				_playWeight = 0.0f;//SubList의 _layerWeight에 해당한다.
				_blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;//SubList의 _blendMethod
			}

			/// <summary>
			/// 재생중이 아님을 설정한다.
			/// 정보가 변동되어 정렬이 필요하면 true 리턴
			/// </summary>
			/// <returns></returns>
			public bool SetPlayData_NoLive()
			{
				if(_isLive || _playIndex != -10)
				{
					//이전에 플레이 중이었다면
					_isLive = false;
					_playIndex = -10;//재생이 아닐땐 Index가 -10이다.
					_playWeight = 0.0f;
					return true;
				}

				//계속 대기 상태
				return false;
			}
			/// <summary>
			/// 재생 중일때의 정보를 설정한다.
			/// 정보가 변동되어 정렬이 필요하면 true 리턴
			/// </summary>
			/// <returns></returns>
			public bool SetPlayData_Live(int playIndex, float playWeight, apModifierParamSetGroup.BLEND_METHOD blendMethod)
			{
				//일단 값 적용
				_playWeight = playWeight;
				_blendMethod = blendMethod;
				
				if(!_isLive || _playIndex != playIndex)
				{
					//플레이 상태나 순서가 바뀌었다. > 정렬 필요
					_isLive = true;
					_playIndex = playIndex;
					return true;
				}

				return false;//계속 플레이 중 > 정렬 필요 없다.
			}
		}

		//재생 정보를 담은 배열
		//전체 데이터는 크기와 위치가 고정이며, "정렬된 데이터"를 별도로 둔다.
		private LiveUnit[] _liveUnits_All = null;
		public LiveUnit[] _liveUnits_Sorted = null;


		// Init
		//---------------------------------------------------------
		public apAnimPlayMapping(apPortrait portrait)
		{
			Link(portrait);
		}

		public void Link(apPortrait portrait)
		{
			_portrait = portrait;

			//애니메이션 클립을 이용해서 데이터 정리
			_index2AnimClip = new Dictionary<int, apAnimClip>();
			_animClip2Index = new Dictionary<apAnimClip, int>();

			_nAnimClips = _portrait._animClips != null ? _portrait._animClips.Count : 0;

			//재생 정보 배열도 함께 만들기
			_liveUnits_All = new LiveUnit[_nAnimClips];
			_liveUnits_Sorted = new LiveUnit[_nAnimClips];

			apAnimClip curAnimClip = null;
			for (int i = 0; i < _nAnimClips; i++)
			{
				curAnimClip = _portrait._animClips[i];
				_index2AnimClip.Add(i, curAnimClip);
				_animClip2Index.Add(curAnimClip, i);

				LiveUnit newLiveUnit = new LiveUnit(i, curAnimClip);
				_liveUnits_All[i] = newLiveUnit;
				_liveUnits_Sorted[i] = newLiveUnit;
			}
		}



		// Functions
		//---------------------------------------------------------
		public void Update()
		{
			//플레이 정보에 따라서 업데이트를 하자 (apOptParamSetGroup의 UpdateAnimLayer의 함수의 내용을 참고하자)
			LiveUnit curUnit = null;
			apAnimClip curAnimClip = null;
			bool isNeedSort = false;
			
			for (int i = 0; i < _nAnimClips; i++)
			{
				curUnit = _liveUnits_All[i];
				curAnimClip = curUnit._linkedAnimClip;

				if(curAnimClip._parentPlayUnit == null
					|| !curAnimClip._parentPlayUnit.IsUpdatable)
				{
					//플레이를 할 수 있는 상태가 아니다.
					if(curUnit.SetPlayData_NoLive())
					{
						isNeedSort = true;//정렬 필요
					}
				}
				else
				{
					//플레이 중이다.
					if(curUnit.SetPlayData_Live(	(curAnimClip._parentPlayUnit._layer * 100) + curAnimClip._parentPlayUnit._playOrder,//Index
													Mathf.Clamp01(curAnimClip._parentPlayUnit.UnitWeight),//Weight
													(curAnimClip._parentPlayUnit.BlendMethod == apAnimPlayUnit.BLEND_METHOD.Interpolation)
															? apModifierParamSetGroup.BLEND_METHOD.Interpolation : apModifierParamSetGroup.BLEND_METHOD.Additive
												))
					{
						isNeedSort = true;//정렬 필요
					}
				}
			}

			if(isNeedSort && _nAnimClips > 1)
			{
				//정렬을 하자
				//가장 빠른 Bubble Sort..
				LiveUnit prevUnit = null;
				LiveUnit nextUnit = null;
				
				//Bubble Sort로 하자. 애니메이션들은 대체로 양이 적으니까 이게 더 빠름


				//n : 3
				//i = 2, 1
				//i = 2일때 : j = 0~1, 1~2 비교
				//i = 1일때 : j = 0~1 비교
				for (int i = _nAnimClips - 1; i > 0; i--)
				{
					for (int j = 0; j < i; j++)
					{
						prevUnit = _liveUnits_Sorted[j];
						nextUnit = _liveUnits_Sorted[j+1];

						//1) 플레이 중이 아닌게 뒤로 간다. (앞 Unit이 플레이중이 아니라면 스왑)
						//2) 둘다 플레이 중이라면 오름차순
						//그 외에는 가만히
						if (prevUnit._isLive && nextUnit._isLive)
						{
							if (prevUnit._playIndex > nextUnit._playIndex)
							{
								//오름차순이 아니라면
								_liveUnits_Sorted[j] = nextUnit;
								_liveUnits_Sorted[j + 1] = prevUnit;
							}
						}
						else if (!prevUnit._isLive && nextUnit._isLive)
						{
							//앞 Unit이 재생되지 않고, 뒤 Unit이 재생 중일때
							_liveUnits_Sorted[j] = nextUnit;
							_liveUnits_Sorted[j + 1] = prevUnit;
						}
					}
				}

				//Debug.LogError("Sort Anim");
				//for (int i = 0; i < _nAnimClips; i++)
				//{
				//	curUnit = _liveUnits_Sorted[i];
				//	if(curUnit._isLive)
				//	{
				//		Debug.Log("[" + i + "] : " + curUnit._linkedAnimClip._name + " <" + curUnit._playIndex + " Playing>");
				//	}
				//	else
				//	{
				//		Debug.LogWarning("[" + i + "] : " + curUnit._linkedAnimClip._name + " <" + curUnit._playIndex + ">");
				//	}
				//}
				
			}
		}


		public int GetAnimClipIndex(apAnimClip animClip)
		{
			return _animClip2Index[animClip];
		}

		public apAnimClip GetAnimClip(int animIndex)
		{
			return _index2AnimClip[animIndex];
		}
	}
}