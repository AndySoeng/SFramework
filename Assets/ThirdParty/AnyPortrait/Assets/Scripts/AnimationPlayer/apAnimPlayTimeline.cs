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
#if UNITY_2017_1_OR_NEWER
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 유니티 2017의 타임라인을 연동한 기능
	/// 이 기능이 동작하는 동안에는 AnimPlayMecanim 대신 동작한다.
	/// 다만, AnimPlayMecanim의 업데이트를 이용하므로, 외부에서는 AnimPlayMecanim이 동작하는 것으로 보인다.
	/// PlayableDirector+PlayableAsset+TrackName을 등록하거나 미리 설정하여 연동을 시킬 수 있다.
	/// [PlayableDirector+PlayableAsset+TrackName]에 따라서 Track과 TimelineClip이 다르므로 따로 저장해야한다.
	/// 이 점이 메카님과 다르다.
	/// </summary>
	public class apAnimPlayTimeline
	{
		// SubClass
		//-------------------------------------------------

		public class TimelineClipData
		{
#if UNITY_2017_1_OR_NEWER
			public TimelineClip _timelineClip = null;
#endif
			public AnimationClip _clipAsset = null;
			public apAnimClip _animClip = null;
			public apOptRootUnit _linkedRootUnit = null;
			public bool _isCalculated = false;
			public bool _isCalculatedPrev = false;
			public bool _isPlaying = false;
			public int _playOrder = 0;
			
			public float _weight = 0.0f;
			public float _timeRatio = 0.0f;

			private apAnimPlayUnit.BLEND_METHOD _blendMethod = apAnimPlayUnit.BLEND_METHOD.Additive;
			private int _layerIndex = 0;

			public apAnimPlayUnit _playUnit = null;

			public TimelineClipData(
#if UNITY_2017_1_OR_NEWER
				TimelineClip timelineClip, 
#endif
				apAnimClip animClip, apOptRootUnit linkedRootUnit)
			{
#if UNITY_2017_1_OR_NEWER
				_timelineClip = timelineClip;
				_clipAsset = timelineClip.animationClip;
#endif
				_animClip = animClip;
				_linkedRootUnit = linkedRootUnit;
				_isCalculated = false;
				_isCalculatedPrev = false;
				_isPlaying = false;
				
				//Mecanim용 PlayUnit을 만든다.
				_playUnit = new apAnimPlayUnit(null, -1, -1);
				_playUnit.SetMecanimPlayUnit();

				ReadyToUpdate();
				Unlink();
			}

			public void ReadyToUpdate()
			{
				_isCalculated = false;
				_isPlaying = false;
				_playOrder = 0;
				//_speed = 0.0f;
				_weight = 0.0f;
				_timeRatio = 0.0f;
			}

			public void SetData(bool isClipA, float weight, float localPlayedTime, apAnimPlayUnit.BLEND_METHOD blendMethod, int layerIndex)
			{
				_isCalculated = true;
				_isPlaying = true;
				if(isClipA)
				{
					_playOrder = 0;
				}
				else
				{
					_playOrder = 1;
				}
				//TimelineClip의 Speed는 LocalTime 계산시 포함되어있다.
				//_speed = speed;
				_weight = weight;

				_timeRatio = localPlayedTime / _animClip.TimeLength;

				_blendMethod = blendMethod;
				_layerIndex = layerIndex;
			}

			public void Link()
			{
				_playUnit.Mecanim_Link(_animClip);
			}

			public void Unlink()
			{
				_playUnit.Mecanim_Unlink();
			}

			public void UpdateTimelineClipAndPlayUnit()
			{
				_playUnit.Mecanim_Update(_weight, _timeRatio, _playOrder, _layerIndex, _blendMethod, 1.0f);
			}
		}

		//트랙 한개가 레이어이다.
		//트랙의 이름과 레이어 번호를 지정한다.
		public class TimelineTrackData
		{
			//키값
			public string _trackName = "";
			public int _layerIndex = 0;//<<이 값은 요청된 Layer이다.

#if UNITY_2017_1_OR_NEWER
			private int _layerOrder = 0;//<<정렬 이후의 Layer 값이다. 0부터 증가한다.
#endif
			public apAnimPlayUnit.BLEND_METHOD _blendMethod = apAnimPlayUnit.BLEND_METHOD.Additive;


			//연결된 트랙과 Director
#if UNITY_2017_1_OR_NEWER
			private DirectorTrackSet _parentTrackSet = null;

			private AnimationTrack _animationTrack = null;
#endif


			// 리스트
#if UNITY_2017_1_OR_NEWER
			private List<TimelineClip> _timelineClips = null;
#endif
			private List<TimelineClipData> _clipData = null;
#if UNITY_2017_1_OR_NEWER
			private Dictionary<TimelineClip, TimelineClipData> _clipDataByTrack = null;
#endif
			private int _nClipData = 0;

			// 계산용 변수
#if UNITY_2017_1_OR_NEWER
			private double _c_curTime;
			private TimelineClipData _c_curClipData = null;
			private TimelineClipData _c_clipA = null;
			private TimelineClipData _c_clipB = null;

			private double _c_localTimeA = 0.0;
			private double _c_localTimeB = 0.0;
			private float _c_weightA = 0.0f;
			private float _c_weightB = 0.0f;

			private int _c_nPlayClips = 0;
			private double _c_localStart = 0;
			private double _c_localEnd = 0;
#endif

			//마지막으로 플레이된 타임라인 클립을 저장한다.
#if UNITY_2017_1_OR_NEWER
			public TimelineClip _lastPlayedTimelineClip = null;
#endif
			public float _lastPlayedLocalTime = 0.0f;

			//초기화
			public TimelineTrackData(DirectorTrackSet parentTrackSet)
			{
#if UNITY_2017_1_OR_NEWER
				_parentTrackSet = parentTrackSet;
#endif
			}

			public void SetLayerOrder(int layerOrder)
			{
#if UNITY_2017_1_OR_NEWER
				_layerOrder = layerOrder;
#endif
			}


			public bool SetTrack(string trackName, int layerIndex, apAnimPlayUnit.BLEND_METHOD blendMethod, apPortrait portrait, apAnimPlayManager animPlayManager)
			{
#if UNITY_2017_1_OR_NEWER
				//PlayableDirector playDirector = _parentTrackSet._playableDirector;
				PlayableAsset playAsset = _parentTrackSet._playableAsset;
#endif

				_trackName = trackName;
				_layerIndex = layerIndex;
#if UNITY_2017_1_OR_NEWER
				_layerOrder = _layerIndex;
#endif
				_blendMethod = blendMethod;

				if(string.IsNullOrEmpty(_trackName))
				{
					//이름이 빈칸이거나 null이다.
					return false;
				}

				_clipData = new List<TimelineClipData>();
#if UNITY_2017_1_OR_NEWER
				_timelineClips = new List<TimelineClip>();
				_clipDataByTrack = new Dictionary<TimelineClip, TimelineClipData>();
#endif

				_nClipData = 0;

				//연결을 하자
				bool isFind = false;
#if UNITY_2017_1_OR_NEWER
				_animationTrack = null;
#endif

				//그 전에 AnimClip <-> AnimationClipAsset을 서로 연결해야한다.
				apAnimClip curAnimClip = null;
				AnimationClip curAnimationClipAsset = null;
				Dictionary<AnimationClip, apAnimClip> animClipAsset2AnimClip = new Dictionary<AnimationClip, apAnimClip>();

				for (int i = 0; i < portrait._animClips.Count; i++)
				{
					curAnimClip = portrait._animClips[i];
					if(curAnimClip == null)
					{
						continue;
					}
					curAnimationClipAsset = curAnimClip._animationClipForMecanim;
					if(curAnimationClipAsset == null)
					{
						continue;
					}

					if(animClipAsset2AnimClip.ContainsKey(curAnimationClipAsset))
					{
						continue;
					}

					animClipAsset2AnimClip.Add(curAnimationClipAsset, curAnimClip);
				}

				//apAnimClip에 해당하는 apOptRootUnit을 알아야 한다.
				//apAnimPlayData에 그 정보가 저장되어 있으니 참조하자
				apAnimPlayData curPlayData = null;
				Dictionary<apAnimClip, apOptRootUnit> animClip2RootUnit = new Dictionary<apAnimClip, apOptRootUnit>();

				for (int i = 0; i < animPlayManager._animPlayDataList.Count; i++)
				{
					curPlayData = animPlayManager._animPlayDataList[i];
					if(curPlayData == null)
					{
						continue;
					}
					if(curPlayData._linkedAnimClip == null || curPlayData._linkedOptRootUnit == null)
					{
						continue;
					}
					
					if(animClip2RootUnit.ContainsKey(curPlayData._linkedAnimClip))
					{
						continue;
					}

					//apAnimClip -> apOptRootUnit으로 연결 데이터 추가
					animClip2RootUnit.Add(curPlayData._linkedAnimClip, curPlayData._linkedOptRootUnit);
				}


#if UNITY_2017_1_OR_NEWER
				foreach (PlayableBinding playableBinding in playAsset.outputs)
				{
#endif
#if UNITY_2018_1_OR_NEWER
					bool isAnimTrack = playableBinding.sourceObject != null && playableBinding.sourceObject is AnimationTrack;
					
#elif UNITY_2017_1_OR_NEWER
					bool isAnimTrack = playableBinding.streamType == DataStreamType.Animation;

#endif
					//if (playableBinding.streamType != DataStreamType.Animation)
#if UNITY_2017_1_OR_NEWER
					if(!isAnimTrack)
					{
						//애니메이션 타입이 아니라면 패스
						continue;
					}

					AnimationTrack animTrack = playableBinding.sourceObject as AnimationTrack;
					if(animTrack == null)
					{
						continue;
					}
					//if(animTrack.isEmpty)
					//{
					//	//클립이 아예 없는데용
					//	continue;
					//}

					if(!animTrack.name.Equals(_trackName))
					{
						//이름이 다르다.
						continue;
					}

					if (animTrack.isEmpty)
					{
						//클립이 아예 없는데용
						//continue;
						Debug.LogWarning("AnyPortrait : ( Warning ) No Clip in the requested track. [ " + trackName + " ]");
						//일단 처리는 하자
					}

					//이름도 같고 유효한 트랙을 찾았다!
					isFind = true;
					_animationTrack = animTrack;
					

					AnimationClip animClipInTrack = null;
					apAnimClip targetAnimClip = null;
					apOptRootUnit targetRootUnit = null;

					foreach (TimelineClip timelineClip in _animationTrack.GetClips())
					{
						//Track의 TimelineClip 중에서 유효한 AnimationClip만 선택한다.
						animClipInTrack = timelineClip.animationClip;

						if(!animClipAsset2AnimClip.ContainsKey(animClipInTrack))
						{
							//유효한 AnimationClip이 아니다.
							continue;
						}

						targetAnimClip = animClipAsset2AnimClip[animClipInTrack];

						if(targetAnimClip == null)
						{
							//animClip이 비어있다.
							continue;
						}

						if(!animClip2RootUnit.ContainsKey(targetAnimClip))
						{
							//apAnimClip -> apOptRootUnit을 조회할 수 없다.
							continue;
						}
						
						targetRootUnit = animClip2RootUnit[targetAnimClip];
						if(targetRootUnit == null)
						{
							//RootUnit이 null이다.
							continue;
						}

						
						TimelineClipData newClipData = new TimelineClipData(timelineClip, targetAnimClip, targetRootUnit);
						_clipData.Add(newClipData);

						_timelineClips.Add(timelineClip);
						_clipDataByTrack.Add(timelineClip, newClipData);
						//_clipDataByAnimClip.Add(targetAnimClip, newClipData);
						_nClipData++;

						
					}

					//Debug.Log("Track [" + trackName + "] Added");

					//<<추가>> 시간대에 따라서 Sort
					_clipData.Sort(delegate(TimelineClipData a, TimelineClipData b)
					{
						return (int)((a._timelineClip.start - b._timelineClip.start) * 100.0);
					});

					if(isFind)
					{
						break;
					}
				}
#endif
				if(!isFind)
				{
					Debug.LogError("AnyPortrait : No track with the requested name. [" + trackName + "]");
				}
				return isFind;
			}


			public void Update(apAnimPlayManager animPlayManager)
			{
#if UNITY_2017_1_OR_NEWER
				_c_curTime = _parentTrackSet._playableDirector.time;

				_lastPlayedTimelineClip = null;
#endif
				_lastPlayedLocalTime = 0.0f;

				//1. 전체 Reday To Update
				for (int i = 0; i < _nClipData; i++)
				{
					_clipData[i].ReadyToUpdate();
				}

#if UNITY_2017_1_OR_NEWER
				//2. 범위 체크해서 업데이트 여부 결정과 Set Data
				_c_curClipData = null;
				_c_clipA = null;
				_c_clipB = null;

				_c_localTimeA = 0.0;
				_c_localTimeB = 0.0;
				_c_weightA = 0.0f;
				_c_weightB = 0.0f;

				_c_nPlayClips = 0;

				for (int i = 0; i < _nClipData; i++)
				{
					_c_curClipData = _clipData[i];

					if(_c_curTime < _c_curClipData._timelineClip.start || 
						_c_curTime > _c_curClipData._timelineClip.end)
					{
						continue;
					}

					if(_c_clipA == null)
					{
						_c_clipA = _c_curClipData;
						_c_nPlayClips = 1;

						_c_localTimeA = _c_clipA._timelineClip.ToLocalTimeUnbound(_c_curTime);
					}
					else if(_c_clipB == null)
					{
						//단, A와 AnimClip이 겹치면 안된다.
						if (_c_clipA._animClip != _c_curClipData._animClip)
						{
							_c_clipB = _c_curClipData;
							_c_nPlayClips = 2;

							_c_localTimeB = _c_clipB._timelineClip.ToLocalTimeUnbound(_c_curTime);
							break;//B까지 연결했으면 끝
						}
					}
				}

				if(_c_nPlayClips == 1)
				{
					//PlayClip이 1개일 때
					if (_c_localTimeA < _c_clipA._timelineClip.easeInDuration)
					{
						_c_weightA = GetWeightIn(_c_localTimeA, _c_clipA._timelineClip.easeInDuration);
					}
					else if (_c_localTimeA < _c_clipA._timelineClip.duration - _c_clipA._timelineClip.easeOutDuration)
					{
						_c_weightA = 1.0f;
					}
					else
					{
						_c_weightA = GetWeightOut(_c_localTimeA, _c_clipA._timelineClip.easeOutDuration, _c_clipA._timelineClip.duration);
					}

					_c_clipA.SetData(true, _c_weightA, (float)_c_localTimeA, _blendMethod, _layerOrder);

					//Clip A가 마지막에 계산되었다.
					_lastPlayedTimelineClip = _c_clipA._timelineClip;
					_lastPlayedLocalTime = (float)_c_localTimeA;
				}
				else if(_c_nPlayClips == 2)
				{
					//PlayClip이 2개일 때 (무조건 Blend)
					_c_weightA = GetWeightOut(_c_localTimeA, _c_clipA._timelineClip.blendOutDuration, _c_clipA._timelineClip.duration);
					_c_weightB = 1.0f - _c_weightA;

					_c_clipA.SetData(true, _c_weightA, (float)_c_localTimeA, _blendMethod, _layerOrder);
					_c_clipB.SetData(false, _c_weightB, (float)_c_localTimeB, _blendMethod, _layerOrder);

					//Clip B가 마지막에 계산되었다.
					_lastPlayedTimelineClip = _c_clipB._timelineClip;
					_lastPlayedLocalTime = (float)_c_localTimeB;
				}



				//3. 클립데이터 확인하여 Link/Unlink 및 Update
				for (int i = 0; i < _nClipData; i++)
				{
					_c_curClipData = _clipData[i];
					if(!_c_curClipData._isCalculated)
					{
						//업데이트가 안되었다면 Release
						if(_c_curClipData._isCalculatedPrev)
						{
							//이전 프레임에서는 계산이 되었던 ClipData다.
							_c_curClipData.Unlink();
						}
					}
					else
					{
						//업데이트가 되었으면 연결을 하고, 프레임과 Weight를 업데이트 한다.
						if(!_c_curClipData._isCalculatedPrev)
						{
							//이전 프레임에서는 계산이 안되었다.
							_c_curClipData.Link();

							//Root Unit을 갱신할 수도 있다.
							animPlayManager.SetOptRootUnit(_c_curClipData._linkedRootUnit);
						}

						//업데이트 <중요!>
						_c_curClipData.UpdateTimelineClipAndPlayUnit();
					}

					//Prev 갱신
					_c_curClipData._isCalculatedPrev = _c_curClipData._isCalculated;
				}
#endif
			}

			//블렌드되는 가중치. 선형으로 처리한다.
			private float GetWeightIn(double localTime, double easeIn)
			{
#if UNITY_2017_1_OR_NEWER
				//앞쪽에서 0~1
				if(easeIn < 0.01)
				{
					//바로 시작
					return 1;
				}
				_c_localStart = 0;
				_c_localEnd = easeIn;
				
				//기존
				//return Mathf.Clamp01((float)((localTime - _c_localStart) / (_c_localEnd - _c_localStart)));

				//변경 (SmoothStep이용)
				return Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01((float)((localTime - _c_localStart) / (_c_localEnd - _c_localStart))));
#else
				return 0.0f;
#endif
			}
			private float GetWeightOut(double localTime, double easeOut, double clipLength)
			{
#if UNITY_2017_1_OR_NEWER
				if(easeOut < 0.01)
				{
					//아직 1
					return 1;
				}

				if(easeOut > clipLength)
				{
					easeOut = clipLength;
				}

				//뒤쪽에서 1~0
				_c_localStart = clipLength - easeOut;
				_c_localEnd = clipLength;
				
				//기존
				//return 1.0f - Mathf.Clamp01((float)((localTime - _c_localStart) / (_c_localEnd - _c_localStart)));

				//변경 (SmoothStep이용)
				return 1.0f - Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01((float)((localTime - _c_localStart) / (_c_localEnd - _c_localStart))));
#else
				return 1.0f;
#endif
			}

			public void UnlinkAll()
			{
#if UNITY_2017_1_OR_NEWER
				for (int i = 0; i < _nClipData; i++)
				{
					_c_curClipData = _clipData[i];
					_c_curClipData.Unlink();
					_c_curClipData._isCalculated = false;
					_c_curClipData._isCalculatedPrev = false;
				}
#endif
			}


		}



		/// <summary>
		/// 연결된 PlayableDirector와 Track 정보
		/// 다만, 인스턴스 정보이므로, 이 클래스의 변수들 게임이 실행되고 생성되어 설정된다.
		/// (Bake되지 않음)
		/// </summary>
		public class DirectorTrackSet
		{
			// 키값
#if UNITY_2017_1_OR_NEWER
			public PlayableDirector _playableDirector = null;
			public PlayableAsset _playableAsset = null;
#endif

			//트랙 리스트 (이름, 인덱스)로 레이어 방식으로 구성된다.
			//List로 구성되어 있으며, 0부터 오름차순으로 결정된다.
			private List<TimelineTrackData> _trackData = null;
			private int _nTrackData = 0;

			
			//계산용 변수
			private TimelineTrackData _curTrackData = null;

			//마지막으로 계산된 TimelineClip. 0번 레이어만 저장한다.
#if UNITY_2017_1_OR_NEWER
			public TimelineClip _lastPlayedTimelineClip = null;
#endif
			public float _lastPlayedLocalTime = 0.0f;

			//초기화
			public DirectorTrackSet()
			{
			}

			//트랙을 추가할 수 있는지 검사 (Static)
			public static bool CheckTrackValidation(
#if UNITY_2017_1_OR_NEWER
				PlayableDirector playableDirector, PlayableAsset playableAsset, 
#endif
				string trackName
				)
			{
#if UNITY_2017_1_OR_NEWER
				if(playableDirector == null)
				{
					return false;
				}

				if(playableAsset == null)
				{
					return false;
				}

				if(playableDirector.playableAsset != playableAsset)
				{
					return false;
				}

				foreach (PlayableBinding playableBinding in playableAsset.outputs)
				{
#endif
#if UNITY_2018_1_OR_NEWER
					bool isAnimTrack = playableBinding.sourceObject != null && playableBinding.sourceObject is AnimationTrack;
					
#elif UNITY_2017_1_OR_NEWER
					bool isAnimTrack = playableBinding.streamType == DataStreamType.Animation;

#endif
					//if (playableBinding.streamType != DataStreamType.Animation)
#if UNITY_2017_1_OR_NEWER
					if(!isAnimTrack)
					{
						//애니메이션 타입이 아니라면 패스
						continue;
					}

					AnimationTrack animTrack = playableBinding.sourceObject as AnimationTrack;
					if (animTrack == null)
					{
						continue;
					}

					if(!animTrack.name.Equals(trackName))
					{
						//이름이 다르다.
						continue;
					}

					//찾았당
					return true;
				}
#endif
				//못찾음
				return false;

			}



			public void SetPlayableDirector(
#if UNITY_2017_1_OR_NEWER
				PlayableDirector playableDirector, PlayableAsset playableAsset
#endif
				)
			{
#if UNITY_2017_1_OR_NEWER
				_playableDirector = playableDirector;
				_playableAsset = playableAsset;
#endif
				_trackData = new List<TimelineTrackData>();
				_nTrackData = 0;
			}

			//트랙을 추가한다.
			public bool AddTrack(string trackName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, apPortrait portrait, apAnimPlayManager animPlayManager)
			{
				//이미 적용된 트랙이 있다면, 에러 메시지를 보낸다.
				bool isExist = _trackData.Exists(delegate(TimelineTrackData a)
				{
					return a._trackName.Equals(trackName);
				});
				if(isExist)
				{
					Debug.LogError("AnyPortrait : A track with the same name has already been registered. [ " + trackName + " ]");
					return false;
				}

				TimelineTrackData newTrackData = new TimelineTrackData(this);
				bool isResult = newTrackData.SetTrack(trackName, layer, blendMethod, portrait, animPlayManager);
				if(!isResult)
				{
					return false;
				}

				//리스트에 트랙 추가
				_trackData.Add(newTrackData);
				_nTrackData = _trackData.Count;

				//트랙을 추가했으면, Layer에 맞게 정렬한다. (오름차순)
				_trackData.Sort(delegate(TimelineTrackData a, TimelineTrackData b)
				{
					return a._layerIndex - b._layerIndex;
				});

				for (int i = 0; i < _trackData.Count; i++)
				{
					_trackData[i].SetLayerOrder(i);//<<레이어의 순서를 다시 지정한다.
				}

				return true;
			}
			


			//업데이트
			public void Update(apAnimPlayManager animPlayManager)
			{
#if UNITY_2017_1_OR_NEWER
				_lastPlayedTimelineClip = null;
#endif

				if(_nTrackData == 0)
				{
					return;
				}

				for (int i = 0; i < _nTrackData; i++)
				{
					_curTrackData = _trackData[i];
					_curTrackData.Update(animPlayManager);
				}

				//0번 레이어의 마지막으로 재생된 클립 정보를 받자
#if UNITY_2017_1_OR_NEWER
				_lastPlayedTimelineClip = _trackData[0]._lastPlayedTimelineClip;
#endif
				_lastPlayedLocalTime = _trackData[0]._lastPlayedLocalTime;
				
			}

			//초기화를 위해서 모든 트랙 데이터를 초기화
			public void UnlinkAll()
			{
				for (int i = 0; i < _nTrackData; i++)
				{
					_curTrackData = _trackData[i];
					_curTrackData.UnlinkAll();
				}
			}

			//Get
			public bool IsValid
			{
				get
				{
#if UNITY_2017_1_OR_NEWER
					return _playableDirector != null && _playableAsset != null;
#else
					return false;
#endif
				}
			}

			public bool IsPlaying
			{
				get
				{
#if UNITY_2017_1_OR_NEWER
					if(_playableDirector == null)
					{
						return false;
					}
					if(_playableAsset == null || _playableDirector.playableAsset != _playableAsset)
					{
						return false;
					}
					
					return _playableDirector.state != PlayState.Paused;//Delay / Playing 상태이면 True
#else
					return false;
#endif
				}
			}

			
		}



		// Members
		//-------------------------------------------------
		private List<DirectorTrackSet> _trackSets = null;
#if UNITY_2017_1_OR_NEWER
		private apPortrait _portrait = null;
#endif
		private apAnimPlayManager _animPlayManager = null;
#if UNITY_2017_1_OR_NEWER
		//private apAnimPlayMecanim _animPlayMecanim = null;
#endif

		private int _nTrackSets = 0;

		private bool _isEnabled = true;//<<이게 켜져 있으면 재생시 자동으로 연결된다. Disabled 상태라면 재생되는 타임라인이 있어도 처리되지 않는다.

		//계산용 변수
		private DirectorTrackSet _curTrackSet = null;

		//이전 프레임에서 0번 레이어에서 플레이된 Timeline Clip을 저장한다.
#if UNITY_2017_1_OR_NEWER
		private TimelineClip _lastPlayedTimelineClip = null;
		private TimelineClip _lastPlayedTimelineClipPrev = null;
#endif
		
		private float _lastPlayedLocalTime = 0.0f;
		
		// Init
		//-------------------------------------------------
		public apAnimPlayTimeline()
		{
			_trackSets = new List<DirectorTrackSet>();
			//_trackKey2TrackSets = new Dictionary<PlayableDirector, Dictionary<PlayableAsset, DirectorTrackSet>>();

			ClearTracks();

			_isEnabled = true;
		}

		public void Link(apPortrait portrait, apAnimPlayManager animPlayManager, apAnimPlayMecanim animPlayMecanim)
		{
			ClearTracks();
#if UNITY_2017_1_OR_NEWER
			_portrait = portrait;
#endif
			_animPlayManager = animPlayManager;
#if UNITY_2017_1_OR_NEWER
			//_animPlayMecanim = animPlayMecanim;
#endif

			//Inspector에서 지정된 트랙 정보가 있다면 먼저 등록한다.
#if UNITY_2017_1_OR_NEWER
			if(portrait._timelineTrackSets != null && portrait._timelineTrackSets.Length > 0)
			{
				apPortrait.TimelineTrackPreset trackSetData = null;
				for (int i = 0; i < portrait._timelineTrackSets.Length; i++)
				{
					trackSetData = portrait._timelineTrackSets[i];

					AddTrack(trackSetData._playableDirector, trackSetData._trackName, trackSetData._layer, trackSetData._blendMethod);
				}
			}
#endif
		}


		public void ClearTracks()
		{
			if(_trackSets == null)
			{
				_trackSets = new List<DirectorTrackSet>();
			}

			_trackSets.Clear();
			_nTrackSets = 0;

#if UNITY_2017_1_OR_NEWER
			_lastPlayedTimelineClip = null;
			_lastPlayedTimelineClipPrev = null;
#endif
			_lastPlayedLocalTime = 0.0f;
		}






		// Functions
		//-------------------------------------------------
		public bool AddTrack(
#if UNITY_2017_1_OR_NEWER
			PlayableDirector playableDirector, 
#endif
			string trackName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod)
		{
#if UNITY_2017_1_OR_NEWER
			if(playableDirector == null)
			{
				Debug.LogError("AnyPortrait : AddTimelineTrack() is Failed. PlayableDirector is Null.");
				return false;
			}
			if(playableDirector.playableAsset == null)
			{
				Debug.LogError("AnyPortrait : AddTimelineTrack() is Failed. PlayableAsset(Timeline) is Null.");
				return false;
			}
#endif
			if(string.IsNullOrEmpty(trackName))
			{
				Debug.LogError("AnyPortrait : AddTimelineTrack() is Failed. Track Name is empty.");
				return false;
			}

#if UNITY_2017_1_OR_NEWER
			bool isValidTrack = DirectorTrackSet.CheckTrackValidation(playableDirector, playableDirector.playableAsset, trackName);
			if(!isValidTrack)
			{
				Debug.LogError("AnyPortrait : AddTimelineTrack() is Failed. The requested track could not be found.");
				return false;
			}
			
			//이미 등록된 Director에 대하여 새로운 Track을 추가하는 경우일 수 있다.
			//없다면 새로 Director를 만들고, 있으면 재활용

			DirectorTrackSet targetDirectorSet = _trackSets.Find(delegate(DirectorTrackSet a)
			{
				return a._playableDirector == playableDirector && a._playableAsset == playableDirector.playableAsset;
			});

			if(targetDirectorSet == null)
			{
				//새로운 Director로 등록해야 한다.
				targetDirectorSet = new DirectorTrackSet();
				targetDirectorSet.SetPlayableDirector(playableDirector, playableDirector.playableAsset);
				_trackSets.Add(targetDirectorSet);

				_nTrackSets = _trackSets.Count;
			}

			bool result = targetDirectorSet.AddTrack(trackName, layer, blendMethod, _portrait, _animPlayManager);

			return result;
#else
			return false;
#endif

		}


		/// <summary>
		/// 유효하지 않은 트랙을 삭제한다.
		/// </summary>
		public void RemoveInvalidTracks()
		{
			//유효하지 않은 트랙들을 지워야 한다.
			_trackSets.RemoveAll(delegate(DirectorTrackSet a)
			{
				if(!a.IsValid)
				{
					return true;
				}
#if UNITY_2017_1_OR_NEWER
				if(a._playableDirector.gameObject == null)
				{
					return true;
				}
#endif
				return false;
			});
			
			_nTrackSets = _trackSets.Count;
		}


		/// <summary>
		/// 특정 PlayableDirector와의 연결을 제거한다.
		/// </summary>
		/// <param name="playableDirector"></param>
		public void UnlinkPlayableDirector(
#if UNITY_2017_1_OR_NEWER
			PlayableDirector playableDirector
#endif
			)
		{
			_trackSets.RemoveAll(delegate(DirectorTrackSet a)
			{
				if(!a.IsValid)//<<이참에 유효하지 않은 것도 삭제
				{
					return true;
				}
#if UNITY_2017_1_OR_NEWER
				if(a._playableDirector.gameObject == null)
				{
					return true;
				}
				//요청한 PlayableDirector를 가지고 있다면 삭제
				if(a._playableDirector == playableDirector)
				{
					return true;
				}
#endif
				return false;
			});
			
			_nTrackSets = _trackSets.Count;
		}

		

		// 업데이트
		//--------------------------------------------------------------------
		public void Update()
		{
			//플레이중인 TrackSet을 찾고 바로 업데이트
			_curTrackSet = null;

#if UNITY_2017_1_OR_NEWER
			_lastPlayedTimelineClipPrev = _lastPlayedTimelineClip;

			_lastPlayedTimelineClip = null;
#endif
			_lastPlayedLocalTime = 0.0f;

			for (int i = 0; i < _nTrackSets; i++)
			{
				_curTrackSet = _trackSets[i];
				//변경 20.3.1 : 게임 중이 아닐땐 재생중이 아니더라도 실행되게 하자
#if UNITY_EDITOR
				if(_curTrackSet.IsPlaying || (!Application.isPlaying && _curTrackSet.IsValid))

#else
				if(_curTrackSet.IsPlaying)
#endif
				
				{
					_curTrackSet.Update(_animPlayManager);

					//마지막으로 계산된 클립을 저장한다.
#if UNITY_2017_1_OR_NEWER
					_lastPlayedTimelineClip = _curTrackSet._lastPlayedTimelineClip;
#endif
					_lastPlayedLocalTime = _curTrackSet._lastPlayedLocalTime;

					
					return;
				}
			}
		}

		// 종료시 모두 초기화
		public void UnlinkAll()
		{
			_curTrackSet = null;
			for (int i = 0; i < _nTrackSets; i++)
			{
				_curTrackSet = _trackSets[i];
				_curTrackSet.UnlinkAll();
			}
		}




		// Get / Set
		//-------------------------------------------------
		public int TrackCount
		{
			get {  return _nTrackSets; }
		}

#if UNITY_2017_1_OR_NEWER
		public TimelineClip LastPlayedTimelineClip
		{
			get {  return _lastPlayedTimelineClip; }
		}
#endif
		public float LastPlayedLocalTime
		{
			get {  return _lastPlayedLocalTime; }
		}
#if UNITY_2017_1_OR_NEWER
		public bool IsLastPlayedTimelineClipChanged
		{
			get {  return _lastPlayedTimelineClipPrev != _lastPlayedTimelineClip && _lastPlayedTimelineClip != null; }
		}
#endif

		/// <summary>
		/// 하나라도 플레이 중이라면 True
		/// </summary>
		/// <returns></returns>
		public bool IsAnyPlaying
		{
			get
			{
				if(!_isEnabled)
				{
					return false;
				}

				if (_nTrackSets == 0)
				{
					return false;
				}

				for (int i = 0; i < _nTrackSets; i++)
				{
					_curTrackSet = _trackSets[i];
					if (_curTrackSet.IsPlaying)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void SetEnable(bool isEnabled)
		{
			_isEnabled = isEnabled;
		}

	}
}
