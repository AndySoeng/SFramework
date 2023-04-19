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
	/// Mecanim을 설정했을때 업데이트되는 매니저 클래스
	/// 에디터에서는 동장하지 않으며, 실시간 플레이에서만 동작한다.
	/// AnimClip의 프레임, 레이어(호출 순서), Blend 방식을 설정하면 Modifier에서 자동으로 수행할 것이다.
	/// 초기화에서 Animator를 인식하고, 레이어 정보를 받는다.
	/// 레이어 정보가 비어있다면 임의로 처리한다.
	/// AnimPlayManager 내부에 포함된다.
	/// </summary>
	public class apAnimPlayMecanim
	{
		// Members
		//---------------------------------------------
		private apAnimPlayManager _animPlayManager = null;
		private apPortrait _portrait = null;

		private Animator _animator = null;
		private RuntimeAnimatorController _animController = null;

		private bool _isValidAnimator = false;

		public class MecanimLayer
		{
			public int _index = 0;
			public apAnimPlayUnit.BLEND_METHOD _blendType = apAnimPlayUnit.BLEND_METHOD.Interpolation;
		}

		private List<MecanimLayer> _layers = new List<MecanimLayer>();
		private int _nLayers = 0;

		

		

		private Dictionary<apAnimClip, AnimationClip> _animClip2Asset = new Dictionary<apAnimClip, AnimationClip>();
		private Dictionary<AnimationClip, apAnimClip> _asset2AnimClip = new Dictionary<AnimationClip, apAnimClip>();

		//TODO
		// Clip Asset에 따라 AnimClip / PlayUnit / 재생 여부 데이터를 관리해야한다.
		public class MecanimClipData
		{
			public AnimationClip _clipAsset = null;
			public apAnimClip _animClip = null;
			public apOptRootUnit _linkedRootUnit = null;
			public bool _isCalculated = false;
			public bool _isCalculatedPrev = false;
			public bool _isPlaying = false;
			public int _playedLayer = 0;
			public int _playOrder = 0;
			public apAnimPlayUnit.BLEND_METHOD _blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation;
			public float _speed = 0.0f;
			public float _weight = 0.0f;
			public float _timeRatio = 0.0f;
			


			public apAnimPlayUnit _playUnit = null;//<<미리 만들어놓고, 실행중일 때에는 연결, 그렇지 않을 때에는 해제한다.

			public MecanimClipData(AnimationClip clipAsset, apAnimClip animClip, apOptRootUnit linkedRootUnit)
			{
				_clipAsset = clipAsset;
				_animClip = animClip;
				_linkedRootUnit = linkedRootUnit;

				//Mecanim용 PlayUnit을 만든다.
				_playUnit = new apAnimPlayUnit(null, -1, -1);
				_playUnit.SetMecanimPlayUnit();

				_isCalculatedPrev = false;
				ReadyToUpdate();
				Unlink();
			}

			public void ReadyToUpdate()
			{
				_isCalculated = false;
				_isPlaying = false;
				_playedLayer = 0;
				_playOrder = 0;
				_blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation;
				_speed = 0.0f;
				_weight = 0.0f;
				_timeRatio = 0.0f;
			}

			public void SetData(	int playedLayer, 
									int playOrder,
									apAnimPlayUnit.BLEND_METHOD blendMethod,
									float speed,
									float speedMultiplier,
									float weight,
									float timeRatio)
			{
				_isCalculated = true;
				_isPlaying = true;
				_playedLayer = playedLayer;
				_playOrder = playOrder;
				_blendMethod = blendMethod;
				//_speed = speed;
				_speed = speed * speedMultiplier;
				_weight = weight;
				_timeRatio = timeRatio;
			}


			public void Link()
			{
				_playUnit.Mecanim_Link(_animClip);
				
			}

			public void Unlink()
			{
				//Debug.LogError("Unlink [" + _animClip._name + "]");
				_playUnit.Mecanim_Unlink();
			}

			

			public void UpdateAnimClipAndPlayUnit()
			{
				_playUnit.Mecanim_Update(_weight, _timeRatio, _playOrder, _playedLayer, _blendMethod, _speed);
			}
		}
		private List<MecanimClipData> _clipData = new List<MecanimClipData>();
		//빠른 접근용
		private Dictionary<AnimationClip, MecanimClipData> _clipDataByAsset = new Dictionary<AnimationClip, MecanimClipData>();
		private Dictionary<apAnimClip, MecanimClipData> _clipDataByAnimClip = new Dictionary<apAnimClip, MecanimClipData>();
		private int _nClipData = 0;


		//계산용 변수
		private MecanimLayer _curMecanimLayer = null;
		private AnimatorStateInfo _curStateInfo;
		private AnimatorStateInfo _nextStateInfo;
		private AnimatorClipInfo[] _curClipInfos = null;
		private AnimatorClipInfo[] _nextClipInfos = null;
		//private bool _isInTransition = false;
		private float _curLayerWeight = 0.0f;
		private float _curNormalizedTime = 0.0f;
		private float _nextNormalizedTime = 0.0f;
		private AnimationClip _curClipAsset = null;
		private AnimationClip _nextClipAsset = null;
		private MecanimClipData _curClipData = null;
		private MecanimClipData _nextClipData = null;
		private int _curOrder = 0;
	
		//추가 3.5 : Unity 2017부터 추가된 "Timeline"기능에 대한 연동 객체이다.
#if UNITY_2017_1_OR_NEWER
		private apAnimPlayTimeline _timlinePlay = null;

		private enum PLAY_TYPE
		{
			Mecanim,
			Timeline
		}

		private PLAY_TYPE _playType = PLAY_TYPE.Mecanim;
#endif

		// Init
		//---------------------------------------------
		public apAnimPlayMecanim()
		{
#if UNITY_2017_1_OR_NEWER
			_playType = PLAY_TYPE.Mecanim;
#endif
		}

		public void LinkPortrait(apPortrait portrait, apAnimPlayManager animPlayManager)
		{
			_portrait = portrait;
			_animPlayManager = animPlayManager;

			_animator = _portrait._animator;
			_animController = null;
			_isValidAnimator = false;

			if(_animator == null)
			{
				//Debug.LogError("No Animator");

				return;
			}

			_animController = _animator.runtimeAnimatorController;

#if UNITY_2017_1_OR_NEWER
			//추가 3.5 : Unity 2017부터 추가된 "Timeline"기능에 대한 연동 객체이다.
			if(_timlinePlay == null)
			{
				_timlinePlay = new apAnimPlayTimeline();
			}

			_timlinePlay.Link(portrait, animPlayManager, this);
			
#endif

			if(_animController == null)
			{
				//변경 : 타임라인을 위해서 Mecanim을 사용하는 경우, RuntimeAnimatorController가 필요없을 수 있다.
				//Debug.LogError("AnyPortrait : No RuntimeAnimatorController on Animator");
				return;
			}
			
			_isValidAnimator = true;

			_layers.Clear();
			_clipData.Clear();
			_clipDataByAsset.Clear();
			_clipDataByAnimClip.Clear();

			//레이어 정보를 담자
			_nLayers = _animator.layerCount;

			//저장된 AnimClip 체크를 위해서 리스트를 만들자
			//유효하지 않은 AnimClip은 제외해야한다.
			List<AnimationClip> allClips = new List<AnimationClip>();
			List<AnimationClip> validClips = new List<AnimationClip>();
			
			
			for (int iLayer = 0; iLayer < _animator.layerCount; iLayer++)
			{
				MecanimLayer newLayer = new MecanimLayer();
				newLayer._index = iLayer;

				newLayer._blendType = apAnimPlayUnit.BLEND_METHOD.Interpolation;

				if(iLayer < portrait._animatorLayerBakedData.Count)
				{
					if(portrait._animatorLayerBakedData[iLayer]._blendType == apAnimMecanimData_Layer.MecanimLayerBlendType.Additive)
					{
						//미리 설정된 데이터가 있고, Additive라면 Additive로 설정
						newLayer._blendType = apAnimPlayUnit.BLEND_METHOD.Additive;
					}
				}

				_layers.Add(newLayer);
				
			}

			//어떤 AnimClip이 있는지 체크하자
			for (int i = 0; i < _animController.animationClips.Length; i++)
			{
				if (!allClips.Contains(_animController.animationClips[i]))
				{
					allClips.Add(_animController.animationClips[i]);
				}
			}
			
		
			//apAnimClip <-> AnimationClip Asset을 서로 연결하자
			_animClip2Asset.Clear();
			_asset2AnimClip.Clear();

			//Debug.Log("animPlayManager._animPlayDataList Count : " + animPlayManager._animPlayDataList.Count);

			for (int i = 0; i < animPlayManager._animPlayDataList.Count; i++)
			{
				apAnimPlayData playData = animPlayManager._animPlayDataList[i];
				//Debug.Log("[" + i + "] : " + playData._animClipName);
				apAnimClip animClip = playData._linkedAnimClip;
				if(animClip == null)
				{
					//Debug.LogError("[" + i + "] : " + playData._animClipName + " >> Linked Anim Clip is Null");
					continue;
				}
				AnimationClip assetClip = animClip._animationClipForMecanim;

				if(assetClip != null)
				{
					_animClip2Asset.Add(animClip, assetClip);
					_asset2AnimClip.Add(assetClip, animClip);

					MecanimClipData newClipData = new MecanimClipData(assetClip, animClip, playData._linkedOptRootUnit);
					_clipData.Add(newClipData);
					_clipDataByAsset.Add(assetClip, newClipData);
					_clipDataByAnimClip.Add(animClip, newClipData);

					if (!validClips.Contains(assetClip))
					{
						validClips.Add(assetClip);
					}
					//Debug.Log("[" + i + "] : " + playData._animClipName + " >> " + assetClip.name);
				}
				else
				{
					if (Application.isPlaying)
					{
						Debug.LogError("[" + i + "] : " + playData._animClipName + " >> AnimAsset is Null");
					}
				}

			}

			_nClipData = _clipData.Count;

			//여기서 유효성 체크
			//(유효성 체크는 Application 실행 시에)
			//if (Application.isPlaying)
			{
				for (int i = 0; i < allClips.Count; i++)
				{
					if (!validClips.Contains(allClips[i]))
					{

						if (allClips[i] != _portrait._emptyAnimClipForMecanim)
						{
							//유효하지 않은 Clip이 발견되었다.
							Debug.LogError("AnyPortrait : ( Caution! ) Contains an invalid AnimationClip. An error may occur when playing. [" + allClips[i].name + " < " + _portrait.gameObject.name + "]");
							Debug.LogError("Valid Clips : " + validClips.Count);
						}
					}
				}
			}

			
		}


		// Functions
		//---------------------------------------------
		public void Update()
		{

#if UNITY_2017_1_OR_NEWER
			//추가 3.5 : Timeline의 PlayerDirector가 연결되어 있다면 재생할 수 있는지 확인하자.
			//재생되는 타임라인이 있다면, 메카님 컨트롤러는 무시하고 타임라인의 제어를 받자
			if(_timlinePlay != null)
			{
				//변경 20.3.1 : 게임이 실행 중이 아닐때, TimelinePlay가 재생중이지 않더라도 Timeline이 시뮬레이션 되도록 해보자
#if UNITY_EDITOR
				if(_timlinePlay.IsAnyPlaying || !Application.isPlaying)
#else
				if(_timlinePlay.IsAnyPlaying)
#endif		
				{
					if(_playType == PLAY_TYPE.Mecanim)
					{
						//Mecanim > Timeline
						_playType = PLAY_TYPE.Timeline;

						//메카님에서 재생중이던 클립들을 모두 Unlink해야한다.
						UnlinkAllMecanimClips();
					}

					//타임라인으로 재생을 한다.
					_timlinePlay.Update();


					if(_timlinePlay.IsLastPlayedTimelineClipChanged)
					{
						//프레임 보정>>
						//마지막으로 재생된 TimelineClip과, 현재 Animator에서 백그라운드에서 재생중인 State간의 시간을 보정한다.
						//- 0번 레이어야 한다.
						//- 재생 중인 State여야 한다.
						if(_timlinePlay.LastPlayedTimelineClip != null
							&& _animator != null 
							&& _isValidAnimator 
							&& _nLayers > 0)
						{
							_curStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
							_curClipInfos = _animator.GetCurrentAnimatorClipInfo(0);

							UnityEngine.Timeline.TimelineClip lastTimelineClip = _timlinePlay.LastPlayedTimelineClip;
							float lastLocalTime = _timlinePlay.LastPlayedLocalTime;

							if(_curClipInfos != null && _curClipInfos.Length > 0)
							{
								if(_curClipInfos[0].clip == lastTimelineClip.animationClip)
								{
									//현재 "재생중"인 "0번"레이어의 "현재 클립"과 동일하다
									//시간을 보정하자
									_animator.PlayInFixedTime(0, 0, lastLocalTime);
								}
							}
						}
					}

					//더이상 처리를 하지 않는다.
					return;
				}

				

				if (_playType == PLAY_TYPE.Timeline)
				{
					//Timeline > Mecanim
					_playType = PLAY_TYPE.Mecanim;

					//Timeline에서 재생중이던 클립들을 모두 Unlink해야한다.
					_timlinePlay.UnlinkAll();
				}
			}
#endif

			if(!_isValidAnimator)
			{
				//변경 : Timeline을 위해서 설정된 경우 RuntimeAnimatorController가 필요 없을 수 있다.
				//Debug.LogError("AnyPortrait : IsValidAnimator = false");
				return;
			}

			//1. 일단 Clip Data를 초기화
			for (int i = 0; i < _nClipData; i++)
			{
				_clipData[i].ReadyToUpdate();
			}

			_curOrder = 0;
			//2. 레이어를 돌면서 각 클립에 대해 플레이, 시간, 배속(정방향/역방향)을 갱신한다.
			//AnimPlayUnit에 대해서 값을 넣는다.
			for (int iLayer = 0; iLayer < _nLayers; iLayer++)
			{
				
				_curMecanimLayer = _layers[iLayer];
				if(iLayer == 0)
				{
					//Layer 0에서는 GetLayerWeight의 값이 0이 리턴된다. (항상 1이어야함)
					_curLayerWeight = 1.0f;
				}
				else
				{
					_curLayerWeight = _animator.GetLayerWeight(iLayer);
				}
				

				//_isInTransition = _animator.IsInTransition(iLayer);
				_curStateInfo = _animator.GetCurrentAnimatorStateInfo(iLayer);
				_curClipInfos = _animator.GetCurrentAnimatorClipInfo(iLayer);

				//Debug.Log("Cur Clip Info : "+ _curClipInfos.Length);

				//Debug.Log("Mecanim Layer [" + iLayer + "] / _isInTransition : " + _isInTransition + " / _curLayerWeight : " + _curLayerWeight);

				_curNormalizedTime = _curStateInfo.normalizedTime;
				//if(_curNormalizedTime > 1.0f)
				//{
				//	_curNormalizedTime -= (int)_curNormalizedTime;
				//}


				//"현재 Clip" 먼저 처리
				if (_curClipInfos != null && _curClipInfos.Length > 0)
				{
					for (int iClip = 0; iClip < _curClipInfos.Length; iClip++)
					{
						_curClipAsset = _curClipInfos[iClip].clip;
						if(_curClipAsset == _portrait._emptyAnimClipForMecanim ||
							_curClipAsset == null)
						{
							continue;
						}
						_curClipData = _clipDataByAsset[_curClipAsset];

						if(_curClipData._isCalculated)
						{
							//이미 처리가 되었다면 패스
							continue;
						}
						//업데이트를 하자 <Current>
						
						_curClipData.SetData(	iLayer, 
												_curOrder, 
												_curMecanimLayer._blendType, 
												_curStateInfo.speed,
												_curStateInfo.speedMultiplier,
												_curLayerWeight * _curClipInfos[iClip].weight,
												_curNormalizedTime);

						_curOrder++;
					}
				}

				_nextStateInfo = _animator.GetNextAnimatorStateInfo(iLayer);
				_nextClipInfos = _animator.GetNextAnimatorClipInfo(iLayer);

				

				//"다음 Clip" 처리
				if (_nextClipInfos != null && _nextClipInfos.Length > 0)
				{
					//1. 전환되는 Clip이 존재한다.
					_nextNormalizedTime = _nextStateInfo.normalizedTime;
					//if (_nextNormalizedTime > 1.0f)
					//{
					//	_nextNormalizedTime -= (int)_nextNormalizedTime;
					//}

					for (int iClip = 0; iClip < _nextClipInfos.Length; iClip++)
					{	
						_nextClipAsset = _nextClipInfos[iClip].clip;
						if(_nextClipAsset == _portrait._emptyAnimClipForMecanim
							|| _nextClipAsset == null)
						{
							continue;
						}
						_nextClipData = _clipDataByAsset[_nextClipAsset];

						if(_nextClipData._isCalculated)
						{
							//이미 처리가 되었다면 패스
							continue;
						}
						//업데이트를 하자 <Next>
						_nextClipData.SetData(	iLayer, 
												_curOrder, 
												_curMecanimLayer._blendType, 
												//apAnimPlayUnit.BLEND_METHOD.Additive,
												_nextStateInfo.speed,
												_nextStateInfo.speedMultiplier,
												_curLayerWeight * _nextClipInfos[iClip].weight,
												_nextNormalizedTime);

						_curOrder++;
					}
				}
				

				
			}


			//3. 플레이 데이터 확인하여 PlayUnit 생성/삭제하여 연결한다.
			//프레임이나 설정, 이벤트 등을 갱신하고 호출한다.
			//0번 레이어를 기준으로 RootUnit을 결정한다.
			for (int i = 0; i < _nClipData; i++)
			{
				_curClipData = _clipData[i];
				
				if(!_curClipData._isCalculated)
				{
					//계산이 안되었으면 Release를 한다.
					if(_curClipData._isCalculatedPrev)
					{
						//이전 프레임에서는 계산이 되었던 ClipData이다.
						_curClipData.Unlink();
					}
				}
				else
				{
					//계산이 되었으면 연결을 하고, 프레임과 Weight를 업데이트한다.
					if(!_curClipData._isCalculatedPrev)
					{
						//이전 프레임에서는 계산이 안된 ClipData이다.
						_curClipData.Link();
						
						//Layer = 0인 경우에 Link가 발생하는 경우 => RootUnit을 설정해주자
						if(_curClipData._playedLayer == 0)
						{
							_animPlayManager.SetOptRootUnit(_curClipData._linkedRootUnit);
						}
					}

					//이제 업데이트를 합시다.
					_curClipData.UpdateAnimClipAndPlayUnit();
				}

				//Prev를 갱신한다
				_curClipData._isCalculatedPrev = _curClipData._isCalculated;
			}
		}


		// Functions
		//---------------------------------------------
		// 외부 제어 함수들 - 주로 Timeline
#if UNITY_2017_1_OR_NEWER
		public bool AddTimelineTrack(UnityEngine.Playables.PlayableDirector playableDirector, string trackName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod)
		{
			if(_timlinePlay == null)
			{
				Debug.LogError("AnyPortrait : AddTimelineTrack() is Failed. It is not ready to be associated with the Timeline. Please try again in the next frame.");
				return false;
			}
			
			return _timlinePlay.AddTrack(playableDirector, trackName, layer, blendMethod);
		}

		public void RemoveInvalidTimelineTracks()
		{
			if(_timlinePlay == null)
			{
				return;
			}
			_timlinePlay.RemoveInvalidTracks();
		}

		public void RemoveAllTimelineTracks()
		{
			if(_timlinePlay == null)
			{
				return;
			}
			_timlinePlay.ClearTracks();
		}

		public void UnlinkTimelinePlayableDirector(UnityEngine.Playables.PlayableDirector playableDirector)
		{
			if(_timlinePlay == null)
			{
				return;
			}

			_timlinePlay.UnlinkPlayableDirector(playableDirector);
		}

		public void SetTimelineEnable(bool isEnabled)
		{
			if(_timlinePlay == null)
			{
				return;
			}

			_timlinePlay.SetEnable(isEnabled);
		}

#endif


		// 추가 3.7 : Unlink 처리
		private void UnlinkAllMecanimClips()
		{
			for (int i = 0; i < _nClipData; i++)
			{
				_curClipData = _clipData[i];
				_curClipData.Unlink();
				_curClipData._isCalculated = false;
				_curClipData._isCalculatedPrev = false;
			}
		}

		// Get / Set
		//---------------------------------------------
	}
}