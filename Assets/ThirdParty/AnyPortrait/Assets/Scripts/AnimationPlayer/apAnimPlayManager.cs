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

	//애니메이션을 통합하여 관리하는 매니저
	//AnimClip을 재생할 때, 레이어와 블렌딩을 수행한다.
	//MeshGroup에 포함된게 아니라 Portrait에 속하는 것이다.
	//어떤 MeshGroup이 출력할지도 이 매니저가 결정한다.
	/// <summary>
	/// The Manager class that controls animations.
	/// This determines which a Root Unit is to be displayed according to the animation being played back.
	/// (You can refer to this in your script, but we do not recommend using it directly.)
	/// </summary>
	[Serializable]
	public class apAnimPlayManager
	{
		// Members
		//-------------------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;


		//런타임)
		// [Play Data]
		//      +
		// [Play Unit] -> [Play Queue] -> [Play Layer] -> 여기서 병합하여 AnimClip 각각의 Blend, Layer Index, Weight를 결정한다.
		[SerializeField]
		public List<apAnimPlayData> _animPlayDataList = new List<apAnimPlayData>();


		//추가 22.6.8 : 이름으로 참조할 때 더 빠르게 참조
		[NonSerialized] private Dictionary<string, apAnimPlayData> _mapping_AnimPlayData_ByName = null;
		[NonSerialized] private Dictionary<apAnimClip, apAnimPlayData> _mapping_AnimPlayData_ByClip = null;


		//에디터)
		//에디터에서는
		//단일 RootUnit 선택 + AnimClip 선택 정도
		//블렌딩은 없으며, 선택된 RootUnit과 AnimClip의 재생을 대신 하는 정도다.
		[NonSerialized]
		private apRootUnit _curRootUnitInEditor = null;

		[NonSerialized]
		private apAnimClip _curAnimClipInEditor = null;


		public bool IsPlaying_Editor
		{
			get
			{
				if (_curAnimClipInEditor == null)
				{
					return false;
				}
				return _curAnimClipInEditor.IsPlaying_Editor;
			}
		}

		[NonSerialized]
		private List<apAnimPlayQueue> _animPlayQueues = new List<apAnimPlayQueue>();

		public const int MIN_LAYER_INDEX = 0;
		public const int MAX_LAYER_INDEX = 20;//<<20까지나 필요할까나..


		public enum PLAY_OPTION
		{
			// 플레이 시작시, 같은 레이어의 AnimClip만 중단시킨다.
			// Fade에서도 정상 종료된다.
			/// <summary>
			/// When the animation is played, it stops other animations on the same layer.
			/// </summary>
			StopSameLayer = 0,

			// 플레이 시작시, 다른 레이어의 AnimClip을 모두 중단시킨다.
			// 요청된 레이어를 기준으로 Delay, Fade시간을 계산하고, 다른 레이어에 적용한다.
			/// <summary>
			/// When the animation is played, it stops other animations on all layers.
			/// </summary>
			StopAllLayers = 1,
		}


		// <플레이에 대한 주석>
		// 각 애니메이션 클립은 루트 유닛에 연결된다.
		// "같은 루트 유닛"에서는 Queue, Layer가 정상적으로 작동을 한다.
		// "다른 루트 유닛"에서는, 가장 마지막에 호출된 루트 유닛을 기준으로 재생되며, 그 외에는 바로 Stop되며 무시된다.
		///// <summary>
		///// Currently playing Root Unit
		///// </summary>
		//[NonSerialized]
		//public apOptRootUnit _curPlayedRootUnit = null;
		private bool _isInitAndLink = false;


		//추가 : 메카님 연동용 멤버
		[NonSerialized]
		public apAnimPlayMecanim _mecanim = null;
		private bool _isMecanim = false;


		//추가 22.5.18 : 업데이트 과정에서 Portrait의 Root Unit이 바뀌었는지 확인하는 플래그 변수
		private bool _isRootUnitChanged = false;


		// Init
		//-------------------------------------------------------
		public apAnimPlayManager()
		{
			_isInitAndLink = false;
		}

		//리스트를 초기화하자.
		/// <summary>
		/// Initialize
		/// </summary>
		public void InitAndLink()
		{
			if (_animPlayDataList == null)
			{
				_animPlayDataList = new List<apAnimPlayData>();
			}
			//_animPlayDataList.Clear();

			if (_animPlayQueues == null)
			{
				_animPlayQueues = new List<apAnimPlayQueue>();
			}
			else
			{
				//일단 모두 Release를 한다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					_animPlayQueues[i].ReleaseForce();
				}

				_animPlayQueues.Clear();
			}

			_animPlayQueues.Clear();
			for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
			{
				apAnimPlayQueue newPlayQueue = new apAnimPlayQueue(i, _portrait, this);
				_animPlayQueues.Add(newPlayQueue);
			}

			if(_mecanim == null)
			{
				_mecanim = new apAnimPlayMecanim();
				_isMecanim = false;
			}



			

			_isInitAndLink = true;
		}


		// [런타임에서] Portrait를 연결하고, Portrait를 검색하여 animPlayData를 세팅한다.
		// 다른 Link가 모두 끝난뒤에 호출하자
		/// <summary>
		/// Connect to Portrait and initialize it for runtime processing.
		/// </summary>
		/// <param name="portrait"></param>
		public void LinkPortrait(apPortrait portrait)
		{
			_portrait = portrait;

			InitAndLink();

			if (_animPlayDataList == null)
			{
				_animPlayDataList = new List<apAnimPlayData>();
			}



			//추가 22.6.8 : 빠른 애니메이션 참조용 Dictionary
			if(_mapping_AnimPlayData_ByName == null) { _mapping_AnimPlayData_ByName = new Dictionary<string, apAnimPlayData>(); }
			if(_mapping_AnimPlayData_ByClip == null) { _mapping_AnimPlayData_ByClip = new Dictionary<apAnimClip, apAnimPlayData>(); }
			_mapping_AnimPlayData_ByName.Clear();
			_mapping_AnimPlayData_ByClip.Clear();



			apAnimPlayData animPlayData = null;

			for (int i = 0; i < _animPlayDataList.Count; i++)
			{
				animPlayData = _animPlayDataList[i];
				animPlayData._isValid = false;//일단 유효성 초기화 (나중에 값 넣으면 자동으로 true)

				apAnimClip animClip = _portrait.GetAnimClip(animPlayData._animClipID);


				apOptRootUnit rootUnit = _portrait._optRootUnitList.Find(delegate (apOptRootUnit a)
				{
					if (a._rootOptTransform != null)
					{
						if (a._rootOptTransform._meshGroupUniqueID == animPlayData._meshGroupID)
						{
							return true;
						}
					}
					return false;
				});

				if (animClip != null && rootUnit != null)
				{
					animPlayData.Link(animClip, rootUnit);

					//추가 : 여기서 ControlParamResult를 미리 만들어서 이후에 AnimClip이 미리 만들수 있게 해주자
					animClip.MakeAndLinkControlParamResults();
				}

				//추가 22.6.8 : 빠른 참조를 위해 연동
				if (!string.IsNullOrEmpty(animPlayData._animClipName))
				{
					if (!_mapping_AnimPlayData_ByName.ContainsKey(animPlayData._animClipName))
					{
						_mapping_AnimPlayData_ByName.Add(animPlayData._animClipName, animPlayData);
					}
				}

				if (animPlayData._linkedAnimClip != null)
				{
					if (!_mapping_AnimPlayData_ByClip.ContainsKey(animPlayData._linkedAnimClip))
					{
						_mapping_AnimPlayData_ByClip.Add(animPlayData._linkedAnimClip, animPlayData);
					}
				}
			}

			//추가 : 메카님 연동
			_mecanim.LinkPortrait(portrait, this);
			

			if(portrait._isUsingMecanim && portrait._animator != null)
			{
				_isMecanim = true;
			}
			else
			{
				_isMecanim = false;
			}


		}




		// [런타임에서] Portrait를 연결하고, Portrait를 검색하여 animPlayData를 세팅한다.
		// 다른 Link가 모두 끝난뒤에 호출하자
		/// <summary>
		/// Connect to Portrait and initialize it for runtime processing.
		/// </summary>
		/// <param name="portrait"></param>
		public IEnumerator LinkPortraitAsync(apPortrait portrait, apAsyncTimer asyncTimer)
		{
			_portrait = portrait;

			InitAndLink();

			//Async Wait
			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}



			if (_animPlayDataList == null)
			{
				_animPlayDataList = new List<apAnimPlayData>();
			}

			//추가 22.6.8 : 빠른 애니메이션 참조용 Dictionary
			if(_mapping_AnimPlayData_ByName == null) { _mapping_AnimPlayData_ByName = new Dictionary<string, apAnimPlayData>(); }
			if(_mapping_AnimPlayData_ByClip == null) { _mapping_AnimPlayData_ByClip = new Dictionary<apAnimClip, apAnimPlayData>(); }
			_mapping_AnimPlayData_ByName.Clear();
			_mapping_AnimPlayData_ByClip.Clear();


			apAnimPlayData animPlayData = null;

			for (int i = 0; i < _animPlayDataList.Count; i++)
			{
				animPlayData = _animPlayDataList[i];
				animPlayData._isValid = false;//일단 유효성 초기화 (나중에 값 넣으면 자동으로 true)

				apAnimClip animClip = _portrait.GetAnimClip(animPlayData._animClipID);


				apOptRootUnit rootUnit = _portrait._optRootUnitList.Find(delegate (apOptRootUnit a)
				{
					if (a._rootOptTransform != null)
					{
						if (a._rootOptTransform._meshGroupUniqueID == animPlayData._meshGroupID)
						{
							return true;
						}
					}
					return false;
				});

				if (animClip != null && rootUnit != null)
				{
					animPlayData.Link(animClip, rootUnit);

					//추가 : 여기서 ControlParamResult를 미리 만들어서 이후에 AnimClip이 미리 만들수 있게 해주자
					animClip.MakeAndLinkControlParamResults();
				}

				//추가 22.6.8 : 빠른 참조를 위해 연동
				if(!_mapping_AnimPlayData_ByName.ContainsKey(animPlayData._animClipName))
				{
					_mapping_AnimPlayData_ByName.Add(animPlayData._animClipName, animPlayData);
				}

				if(!_mapping_AnimPlayData_ByClip.ContainsKey(animPlayData._linkedAnimClip))
				{
					_mapping_AnimPlayData_ByClip.Add(animPlayData._linkedAnimClip, animPlayData);
				}

				//Async Wait
				if (asyncTimer.IsYield())
				{
					yield return asyncTimer.WaitAndRestart();
				}
			}

			//추가 : 메카님 연동
			_mecanim.LinkPortrait(portrait, this);
			

			//Async Wait
			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}

			if(portrait._isUsingMecanim && portrait._animator != null)
			{
				_isMecanim = true;
			}
			else
			{
				_isMecanim = false;
			}

		}


		// [Runtime] Functions
		//-------------------------------------------------------
		// 제어 함수
		// 업데이트를 한다
		// 1차적으로 키프레임을 업데이트하고, 2차로 컨트롤 Param을 업데이트 한다.
		/// <summary>
		/// Update the animation. Calculate the keyframes and update the Control Parameters.
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update(float tDelta)
		{
			if (!_isInitAndLink)
			{
				//Debug.LogError("AnyPortrait : Not Initialized AnimPlayManager");
				return;
			}

			//컨트롤러 초기화 먼저
			_portrait._controller.ReadyToLayerUpdate();

			_isRootUnitChanged = false;//추가 22.5.18


			//변경 : 메카님 여부에 따라 업데이트 방식이 다르다
			//> 1. 기본 방식 : AnimQueue를 업데이트 한다.
			//> 2. 메카님 : 메카님을 업데이트 한다.
			if (!_isMecanim)
			{
				for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
				{
					//Play Queue 업데이트
					_animPlayQueues[i].Update(tDelta);
				}
			}
			else
			{
				//메카님을 업데이트한다.
				_mecanim.Update();
			}

			//추가 22.5.18 : 루트 유닛이 바뀌었다면 후속 처리를 해야한다.
			if(_isRootUnitChanged)
			{
				//Root Unit이 맞지 않은 
				StopPlayUnitsInvalidRootUnits();
			}


			//컨트롤러 적용
			_portrait._controller.CompleteLayerUpdate();
		}

		//추가 21.6.8 : 동기화된 경우, 
		public void UpdateAsSyncChild(float tDelta, apSyncPlay syncPlay)
		{
			//컨트롤러 초기화 먼저
			_portrait._controller.ReadyToLayerUpdate();


			if(syncPlay._nSyncSet_AnimClip > 0)
			{
				apSyncSet_AnimClip curSyncSet = null;
				for (int i = 0; i < syncPlay._nSyncSet_AnimClip; i++)
				{
					curSyncSet = syncPlay._syncSet_AnimClip[i];
					if(curSyncSet._animClip == null)
					{
						continue;
					}

					//동기화 및 업데이트를 한다.
					curSyncSet.SyncAndUpdate();					
				}
			}


			//컨트롤러 적용
			_portrait._controller.CompleteLayerUpdate();
		}





		//추가 22.6.17 : 에디터 미리보기용 업데이트 함수
		public void Update_InspectorPreview()
		{
			if (!_isInitAndLink)
			{
				return;
			}

			//컨트롤러 초기화 먼저
			_portrait._controller.ReadyToLayerUpdate();

			_isRootUnitChanged = false;//추가 22.5.18

			//메카님은 여기서 동작하지 않는다.
			//바로 동작함

			for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
			{
				//Play Queue 업데이트
				_animPlayQueues[i].Update_InspectorPreview();//<<이게 인스펙터용으로 바뀜
			}


			//추가 22.5.18 : 루트 유닛이 바뀌었다면 후속 처리를 해야한다.
			if(_isRootUnitChanged)
			{
				//Root Unit이 맞지 않은 
				StopPlayUnitsInvalidRootUnits();
			}

			//컨트롤러 적용
			_portrait._controller.CompleteLayerUpdate();
		}






		//AnimPlayData가 생성 또는 삭제 되었을 때, PlayOrder를 다시 매겨준다.
		/// <summary>
		/// Recalculate the processing order of the animation.
		/// </summary>
		public void RefreshPlayOrders()
		{
			int playOrder = 0;
			for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
			{
				playOrder = _animPlayQueues[i].RefreshPlayOrders(playOrder);
			}
		}


		//추가 : AnimPlayData로 PlayQueued를 바로 실행하는 함수가 나오면서, 이름으로 검색하는 건 오버로드로 뺌
		public apAnimPlayData Play(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false, bool isDebugMsg = true)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				if (isDebugMsg)
				{
					Debug.LogError("Play Failed : No AnimClip [" + animClipName + "]");
				}
				return null;
			}
			return Play(playData, layer, blendMethod, playOption, isAutoEndIfNotloop, isDebugMsg);
		}

		


		/// <summary>
		/// Play the animation
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData Play(apAnimPlayData playData, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false, bool isDebugMsg = true)
		{
			if (playData == null)
			{
				if (isDebugMsg)
				{
					Debug.LogError("Play Failed : Unknown AnimPlayData");
				}
				return null;
			}
			
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				if (isDebugMsg)
				{
					Debug.LogError("Play Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				}
				return null;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.Play(playData, blendMethod, 0.0f, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{
				return null;
			}

			

			if (playOption == PLAY_OPTION.StopAllLayers)
			{
				//다른 레이어를 모두 정지시킨다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					if (i == layer) { continue; }
					_animPlayQueues[i].StopAll(0.0f);
				}
			}

			RefreshPlayOrders();

			return playData;
		}



		


		//추가 : AnimPlayData로 PlayQueued를 바로 실행하는 함수가 나오면서, 이름으로 검색하는 건 오버로드로 뺌
		public apAnimPlayData PlayQueued(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("PlayQueued Failed : No AnimClip [" + animClipName + "]");
				return null;
			}
			return PlayQueued(playData, layer, blendMethod, isAutoEndIfNotloop);
		}

		/// <summary>
		/// Wait for the previous animation to finish, then play it.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueued(apAnimPlayData playData, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop = false)
		{
			if (playData == null)
			{
				Debug.LogError("PlayQueued Failed : Unknown AnimPlayData");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("PlayQueued Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayQueued(playData, blendMethod, 0.0f, isAutoEndIfNotloop);

			if (resultPlayUnit == null) { return null; }


			

			//if (playOption == PLAY_OPTION.StopAllLayers)
			//{
			//	//다른 레이어를 모두 정지시킨다. - 단, 딜레이를 준다. 
			//	for (int i = 0; i < _animPlayQueues.Count; i++)
			//	{
			//		if (i == layer)
			//		{ continue; }
			//		_animPlayQueues[i].StopAll(delayTime);
			//	}
			//}

			RefreshPlayOrders();

			return playData;
		}


		//추가 : AnimPlayData로 PlayQueued를 바로 실행하는 함수가 나오면서, 이름으로 검색하는 건 오버로드로 뺌
		public apAnimPlayData CrossFade(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("CrossFade Failed : No AnimClip [" + animClipName + "]");
				return null;
			}
			return CrossFade(playData, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Play the animation smoothly.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFade(apAnimPlayData playData, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false)
		{
			if (playData == null)
			{
				Debug.LogError("CrossFade Failed : Unknown AnimPlayData");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("CrossFade Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			if (fadeTime < 0.0f)
			{
				fadeTime = 0.0f;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.Play(playData, blendMethod, fadeTime, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{ return null; }


			//float fadeInTime = resultPlayUnit.FadeInTime;

			if (playOption == PLAY_OPTION.StopAllLayers)
			{
				//다른 레이어를 모두 정지시킨다. - 단, 딜레이를 준다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					if (i == layer)
					{ continue; }
					_animPlayQueues[i].StopAll(fadeTime);
				}
			}

			RefreshPlayOrders();

			return playData;
		}

		//추가 : AnimPlayData로 PlayQueued를 바로 실행하는 함수가 나오면서, 이름으로 검색하는 건 오버로드로 뺌
		public apAnimPlayData CrossFadeQueued(string animClipName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("CrossFade Failed : No AnimClip [" + animClipName + "]");
				return null;
			}
			return CrossFadeQueued(playData, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}
		/// <summary>
		/// Wait for the previous animation to finish, then play it smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueued(apAnimPlayData playData, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, bool isAutoEndIfNotloop = false)
		{
			if (playData == null)
			{
				Debug.LogError("CrossFadeQueued Failed : Unknown AnimPlayData");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("CrossFadeQueued Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			if (fadeTime < 0.0f)
			{
				fadeTime = 0.0f;
			}

			//Debug.Log("CrossFadeQueued [" + animClipName + "]");

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayQueued(playData, blendMethod, fadeTime, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{

				return null;
			}

			//float delayTime = resultPlayUnit.DelayToPlayTime;
			//float delayTime = Mathf.Clamp01(resultPlayUnit.RemainPlayTime - fadeTime);

			//if (playOption == PLAY_OPTION.StopAllLayers)
			//{
			//	//다른 레이어를 모두 정지시킨다. - 단, 딜레이를 준다.
			//	for (int i = 0; i < _animPlayQueues.Count; i++)
			//	{
			//		if (i == layer)
			//		{ continue; }
			//		_animPlayQueues[i].StopAll(delayTime);
			//	}
			//}

			RefreshPlayOrders();

			return playData;
		}

		//----------------------------------------------------------------------------------------
		public apAnimPlayData PlayAt(string animClipName, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false, bool isDebugMsg = true)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				if (isDebugMsg)
				{
					Debug.LogError("PlayAt Failed : No AnimClip [" + animClipName + "]");
				}
				return null;
			}
			return PlayAt(playData, frame, layer, blendMethod, playOption, isAutoEndIfNotloop, isDebugMsg);
		}

		public apAnimPlayData PlayAt(apAnimPlayData playData, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false, bool isDebugMsg = true)
		{
			if (playData == null)
			{
				if (isDebugMsg)
				{
					Debug.LogError("PlayAt Failed : Unknown AnimPlayData");
				}
				return null;
			}
			
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				if (isDebugMsg)
				{
					Debug.LogError("PlayAt Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				}
				return null;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayAt(playData, frame, blendMethod, 0.0f, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{	
				return null;
			}

			if (playOption == PLAY_OPTION.StopAllLayers)
			{
				//다른 레이어를 모두 정지시킨다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					if (i == layer) { continue; }
					_animPlayQueues[i].StopAll(0.0f);
				}
			}

			RefreshPlayOrders();

			return playData;
		}



		public apAnimPlayData PlayQueuedAt(string animClipName, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("PlayQueuedAt Failed : No AnimClip [" + animClipName + "]");
				return null;
			}
			return PlayQueuedAt(playData, frame, layer, blendMethod, isAutoEndIfNotloop);
		}

		/// <summary>
		/// Wait for the previous animation to finish, then play it.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData PlayQueuedAt(apAnimPlayData playData, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop = false)
		{
			if (playData == null)
			{
				Debug.LogError("PlayQueuedAt Failed : Unknown AnimPlayData");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("PlayQueuedAt Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayQueuedAt(playData, frame, blendMethod, 0.0f, isAutoEndIfNotloop);

			if (resultPlayUnit == null) { return null; }
			
			RefreshPlayOrders();

			return playData;
		}



		public apAnimPlayData CrossFadeAt(string animClipName, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("CrossFade Failed : No AnimClip [" + animClipName + "]");
				return null;
			}
			return CrossFadeAt(playData, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}


		/// <summary>
		/// Play the animation smoothly.
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="playOption">How to stop which animations</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeAt(apAnimPlayData playData, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, PLAY_OPTION playOption = PLAY_OPTION.StopSameLayer, bool isAutoEndIfNotloop = false)
		{
			if (playData == null)
			{
				Debug.LogError("CrossFadeAt Failed : Unknown AnimPlayData");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("CrossFadeAt Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			if (fadeTime < 0.0f)
			{
				fadeTime = 0.0f;
			}

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayAt(playData, frame, blendMethod, fadeTime, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{ return null; }


			//float fadeInTime = resultPlayUnit.FadeInTime;

			if (playOption == PLAY_OPTION.StopAllLayers)
			{
				//다른 레이어를 모두 정지시킨다. - 단, 딜레이를 준다.
				for (int i = 0; i < _animPlayQueues.Count; i++)
				{
					if (i == layer)
					{ continue; }
					_animPlayQueues[i].StopAll(fadeTime);
				}
			}

			RefreshPlayOrders();

			return playData;
		}



		public apAnimPlayData CrossFadeQueuedAt(string animClipName, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, bool isAutoEndIfNotloop = false)
		{
			apAnimPlayData playData = GetAnimPlayData_Opt(animClipName);
			if (playData == null)
			{
				Debug.LogError("CrossFadeQueuedAt Failed : No AnimClip [" + animClipName + "]");
				return null;
			}
			return CrossFadeQueuedAt(playData, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}
		/// <summary>
		/// Wait for the previous animation to finish, then play it smoothly.
		/// (If the previously playing animation is a loop animation, it will not be executed.)
		/// </summary>
		/// <param name="animClipName">Name of the Animation Clip</param>
		/// <param name="fadeTime">Fade Time</param>
		/// <param name="layer">The layer to which the animation is applied. From 0 to 20</param>
		/// <param name="blendMethod">How it is blended with the animation of the lower layers</param>
		/// <param name="isAutoEndIfNotloop">If True, animation that does not play repeatedly is automatically terminated.</param>
		/// <returns>Animation data to be played. If it fails, null is returned.</returns>
		public apAnimPlayData CrossFadeQueuedAt(apAnimPlayData playData, int frame, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod, float fadeTime, bool isAutoEndIfNotloop = false)
		{
			if (playData == null)
			{
				Debug.LogError("CrossFadeQueuedAt Failed : Unknown AnimPlayData");
				return null;
			}

			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{
				Debug.LogError("CrossFadeQueuedAt Failed : Layer " + layer + " is invalid. Layer must be between " + MIN_LAYER_INDEX + " ~ " + MAX_LAYER_INDEX);
				return null;
			}

			if (fadeTime < 0.0f)
			{
				fadeTime = 0.0f;
			}

			//Debug.Log("CrossFadeQueued [" + animClipName + "]");

			apAnimPlayQueue playQueue = _animPlayQueues[layer];
			apAnimPlayUnit resultPlayUnit = playQueue.PlayQueuedAt(playData, frame, blendMethod, fadeTime, isAutoEndIfNotloop);

			if (resultPlayUnit == null)
			{

				return null;
			}

			RefreshPlayOrders();

			return playData;
		}
		//----------------------------------------------------------------------------------------

		/// <summary>
		/// End all animations playing on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		/// <param name="fadeTime">Fade Time</param>
		public void StopLayer(int layer, float fadeTime = 0.0f)
		{
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{ return; }

			_animPlayQueues[layer].StopAll(fadeTime);
		}

		/// <summary>
		/// End all animations.
		/// </summary>
		/// <param name="fadeTime">Fade Time</param>
		public void StopAll(float fadeTime = 0.0f)
		{
			for (int i = 0; i < _animPlayQueues.Count; i++)
			{
				_animPlayQueues[i].StopAll(fadeTime);
			}
		}


		/// <summary>
		/// Pause all animations playing on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		public void PauseLayer(int layer)
		{
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{ return; }
			_animPlayQueues[layer].Pause();
		}

		/// <summary>
		/// Pause all animations.
		/// </summary>
		public void PauseAll()
		{
			for (int i = 0; i < _animPlayQueues.Count; i++)
			{
				_animPlayQueues[i].Pause();
			}
		}

		/// <summary>
		/// Resume all animations paused on the target layer.
		/// </summary>
		/// <param name="layer">Target Layer (From 0 to 20)</param>
		public void ResumeLayer(int layer)
		{
			if (layer < MIN_LAYER_INDEX || layer > MAX_LAYER_INDEX)
			{ return; }
			_animPlayQueues[layer].Resume();
		}


		/// <summary>
		/// Resume all animations.
		/// </summary>
		public void ResumeAll()
		{
			for (int i = 0; i < _animPlayQueues.Count; i++)
			{
				_animPlayQueues[i].Resume();
			}
		}



		//----------------------------------------------------------------
		//추가 21.4.3 : Hide를 하면 Stop요청을 하더라도 업데이트가 안되서 PlayUnit이 종료되지 않고 "동면"에 빠진다.
		//Portrait가 종료될 때 강제로 모든 PlayUnit들이 End에 넘어가서 종료되는걸 만들자.
		/// <summary>
		/// [Do not use this function]
		/// </summary>
		public void ReleaseAllPlayUnitAndQueues()
		{
			if (!_isInitAndLink)
			{
				//Debug.LogError("AnyPortrait : Not Initialized AnimPlayManager");
				return;
			}

			//컨트롤러 초기화 먼저
			_portrait._controller.ReadyToLayerUpdate();

			_isRootUnitChanged = false;

			//변경 : 메카님 여부에 따라 업데이트 방식이 다르다
			//> 1. 기본 방식 : AnimQueue를 업데이트 한다.
			//> 2. 메카님 : 메카님을 업데이트 한다.
			if (!_isMecanim)
			{
				for (int i = MIN_LAYER_INDEX; i <= MAX_LAYER_INDEX; i++)
				{
					//Play Queue 업데이트
					_animPlayQueues[i].SetAllPlayUnitEnd();//중요 > Ended 상태로 만든다.
					_animPlayQueues[i].Update(0.0f);
				}
			}
			else
			{
				//메카님을 업데이트한다.
				_mecanim.Update();
			}
			

			//추가 22.5.18 : 루트 유닛이 바뀌었다면 후속 처리를 해야한다.
			if(_isRootUnitChanged)
			{
				//Root Unit이 맞지 않은 
				StopPlayUnitsInvalidRootUnits();
			}

			//컨트롤러 적용
			_portrait._controller.CompleteLayerUpdate();
		}



		//----------------------------------------------------------------
		/// <summary>
		/// Get Animation Data in runtime
		/// </summary>
		/// <param name="animClipName">Name of Animation Clip</param>
		/// <returns></returns>
		public apAnimPlayData GetAnimPlayData_Opt(string animClipName)
		{	
			//이전
			//return _animPlayDataList.Find(delegate (apAnimPlayData a)
			//{
			//	return string.Equals(a._animClipName, animClipName);
			//});

			//변경 22.6.8 : 코드 최적화
			apAnimPlayData result = null;
			if(_mapping_AnimPlayData_ByName != null)
			{
				_mapping_AnimPlayData_ByName.TryGetValue(animClipName, out result);
			}
			if(result == null)
			{
				result = _animPlayDataList.Find(delegate (apAnimPlayData a)
				{
					return string.Equals(a._animClipName, animClipName);
				});
			}
			return result;
		}

		/// <summary>
		/// Get Animation Data in runtime
		/// </summary>
		/// <param name="animClip">Linked Animation Clip</param>
		/// <returns></returns>
		public apAnimPlayData GetAnimPlayData_Opt(apAnimClip animClip)
		{
			//이전
			//return _animPlayDataList.Find(delegate (apAnimPlayData a)
			//{
			//	return a._linkedAnimClip == animClip;
			//});

			//변경 22.6.8 : 코드 최적화
			apAnimPlayData result = null;
			if(_mapping_AnimPlayData_ByClip != null)
			{
				_mapping_AnimPlayData_ByClip.TryGetValue(animClip, out result);
			}
			if (result == null)
			{
				result = _animPlayDataList.Find(delegate (apAnimPlayData a)
				{
					return a._linkedAnimClip == animClip;
				});
			}
			return result;
			
		}


		public bool IsPlaying(string animClipName)
		{
			apAnimPlayData animPlayData = GetAnimPlayData_Opt(animClipName);
			if(animPlayData == null)
			{
				Debug.LogError("No AnimCip : " + animClipName);
				return false;
			}
			return animPlayData._linkedAnimClip.IsPlaying_Opt;
		}

		public apAnimPlayData.AnimationPlaybackStatus GetAnimationPlaybackStatus(string animClipName)
		{
			apAnimPlayData animPlayData = GetAnimPlayData_Opt(animClipName);
			if(animPlayData == null)
			{
				Debug.LogError("No AnimCip : " + animClipName);
				return apAnimPlayData.AnimationPlaybackStatus.None;
			}
			return animPlayData.PlaybackStatus;
		}

		//애니메이션 속도 설정
		public void SetAnimSpeed(string animClipName, float speed)
		{
			apAnimPlayData animPlayData = GetAnimPlayData_Opt(animClipName);
			if(animPlayData == null)
			{
				Debug.LogError("No AnimCip : " + animClipName);
				return;
			}
			//animPlayData._linkedAnimClip._speedRatio = speed;//이전
			animPlayData._linkedAnimClip.SetSpeed(speed);//이후
			
			
		}

		public void SetAnimSpeed(float speed)
		{
			apAnimClip animClip = null;
			for (int i = 0; i < _animPlayDataList.Count; i++)
			{
				animClip = _animPlayDataList[i]._linkedAnimClip;
				if(animClip != null)
				{
					//animClip._speedRatio = speed;//이전
					animClip.SetSpeed(speed);//이후
				}
			}
		}

		public void ResetAnimSpeed()
		{
			SetAnimSpeed(1.0f);
		}


		public int GetAnimationCurrentFrame(string animClipName)
		{
			apAnimPlayData animPlayData = GetAnimPlayData_Opt(animClipName);
			if(animPlayData == null)
			{
				Debug.LogError("No AnimCip : " + animClipName);
				return -1;
			}
			return animPlayData.CurrentFrame;
		}

		public int GetAnimationStartFrame(string animClipName)
		{
			apAnimPlayData animPlayData = GetAnimPlayData_Opt(animClipName);
			if(animPlayData == null)
			{
				Debug.LogError("No AnimCip : " + animClipName);
				return -1;
			}
			return animPlayData.StartFrame;
		}

		public int GetAnimationEndFrame(string animClipName)
		{
			apAnimPlayData animPlayData = GetAnimPlayData_Opt(animClipName);
			if(animPlayData == null)
			{
				Debug.LogError("No AnimCip : " + animClipName);
				return -1;
			}
			return animPlayData.EndFrame;
		}

		public float GetAnimationNormalizedTime(string animClipName)
		{
			apAnimPlayData animPlayData = GetAnimPlayData_Opt(animClipName);
			if(animPlayData == null)
			{
				Debug.LogError("No AnimCip : " + animClipName);
				return -1.0f;
			}
			return animPlayData.NormalizedTime;
		}




		/// <summary>
		/// Set Playing Root Unit.
		/// </summary>
		/// <param name="rootUnit"></param>
		public void SetOptRootUnit(apOptRootUnit rootUnit)
		{
			if (_portrait._curPlayingOptRootUnit != rootUnit)//변경 21.4.3 : CurPlayedRootUnit 삭제 후 Portrait의 변수를 직접 이용
			{
				//Root Unit 전환
				_portrait.ShowRootUnit(rootUnit);

				//이전 : 여기서 바로 전체 End를 할게 아니라, 
				////AnimQueue를 돌면서 해당 RootUnit이 아닌 PlayUnit은 강제 종료한다.
				//for (int i = 0; i < _animPlayQueues.Count; i++)
				//{
				//	_animPlayQueues[i].StopWithInvalidRootUnit(_portrait._curPlayingOptRootUnit);
				//}

				//변경 22.5.18
				//플래그를 이용하여 이후에 StopPlayUnitsInvalidRootUnits() 함수를 일괄 호출하도록 하자.
				_isRootUnitChanged = true;

				//Debug.Log("루트 유닛 변경 요청 : " + (rootUnit != null ? rootUnit.name : "None"));
			}
		}

		/// <summary>
		/// 추가 22.5.18 : 다중 루트유닛일 때, 현재의 루트 유닛에 맞지 않는 Play Unit은 모두 종료한다.
		/// (기존에는 매번 호출되었다.)
		/// </summary>
		private void StopPlayUnitsInvalidRootUnits()
		{
			//Debug.Log("<< 루트 유닛이 변경되었다. >>");
			//if(_portrait._curPlayingOptRootUnit != null)
			//{
			//	Debug.Log("[ " + _portrait._curPlayingOptRootUnit.name + " ]");
			//}

			//AnimQueue를 돌면서 해당 RootUnit이 아닌 PlayUnit은 강제 종료한다.
			for (int i = 0; i < _animPlayQueues.Count; i++)
			{
				_animPlayQueues[i].StopWithInvalidRootUnit(_portrait._curPlayingOptRootUnit);
			}
			_isRootUnitChanged = false;
			
			//Debug.Break();
		}


		/// <summary>
		/// Event : Animation is started
		/// </summary>
		/// <param name="playUnit"></param>
		/// <param name="playQueue"></param>
		public void OnAnimPlayUnitPlayStart(apAnimPlayUnit playUnit, apAnimPlayQueue playQueue)
		{
			//Play Unit이 재생을 시작했다.
			//Delay 이후에 업데이트되는 첫 프레임에 이 이벤트가 호출된다.

			// > Root Unit이 바뀔 수 있으므로 Play Manager에도 신고를 해야한다.
			SetOptRootUnit(playUnit._targetRootUnit);
		}

		/// <summary>
		/// Event : Animation is ended
		/// </summary>
		/// <param name="playUnit"></param>
		/// <param name="playQueue"></param>
		public void OnAnimPlayUnitEnded(apAnimPlayUnit playUnit, apAnimPlayQueue playQueue)
		{
			//Play Unit이 재생을 종료했다
			//1. apAnimPlayUnit을 사용하고 있던 Modifier와의 연동을 해제한다.
			//??

		}

		/// <summary>
		/// Event : Any Animation is ended
		/// </summary>
		public void OnAnyAnimPlayUnitEnded()
		{
			//Debug.Log("Anim End And Refresh Order");
			RefreshPlayOrders();
		}

		//추가 3.8 : 타임라인 관련 함수들
		//Timeline이 유니티 2017의 기능이므로 그 전에는 막혀있다.
#if UNITY_2017_1_OR_NEWER
		public bool AddTimelineTrack(UnityEngine.Playables.PlayableDirector playableDirector, string trackName, int layer, apAnimPlayUnit.BLEND_METHOD blendMethod)
		{
			if (!_isMecanim)
			{
				Debug.LogError("AnyPortrait : AddTimelineTrack() is Failed. Mecanim is not activated.");
				return false;
			}
			return _mecanim.AddTimelineTrack(playableDirector, trackName, layer, blendMethod);
		}

		public void RemoveInvalidTimelineTracks()
		{
			if (!_isMecanim)
			{
				Debug.LogError("AnyPortrait : RemoveInvalidTimelineTracks() is Failed. Mecanim is not activated.");
				return;
			}
			_mecanim.RemoveInvalidTimelineTracks();
		}

		public void RemoveAllTimelineTracks()
		{
			if (!_isMecanim)
			{
				Debug.LogError("AnyPortrait : RemoveAllTimelineTracks() is Failed. Mecanim is not activated.");
				return;
			}
			_mecanim.RemoveAllTimelineTracks();
		}

		public void UnlinkTimelinePlayableDirector(UnityEngine.Playables.PlayableDirector playableDirector)
		{
			if (!_isMecanim)
			{
				Debug.LogError("AnyPortrait : UnlinkTimelinePlayableDirector() is Failed. Mecanim is not activated.");
				return;
			}
			_mecanim.UnlinkTimelinePlayableDirector(playableDirector);
		}

		public void SetTimelineEnable(bool isEnabled)
		{
			if (!_isMecanim)
			{
				Debug.LogError("AnyPortrait : SetTimelineEnable() is Failed. Mecanim is not activated.");
				return;
			}
			_mecanim.SetTimelineEnable(isEnabled);
		}

#endif







		// [에디터] Functions
		//-------------------------------------------------------
		// 업데이트 함수
#if UNITY_EDITOR
		/// <summary>
		/// [Please do not use it] Update in Editor
		/// </summary>
		/// <param name="tDelta"></param>
		/// <returns></returns>
		public bool Update_Editor(float tDelta)
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return false;
			}

			int curFrame = _curAnimClipInEditor.CurFrame;
			_curAnimClipInEditor.Update_Editor(tDelta, false, false, false, false);

			return curFrame != _curAnimClipInEditor.CurFrame;
		}
#endif

		// 제어 함수
		/// <summary>
		/// [Please do not use it] Set Root Unit in Editor
		/// </summary>
		/// <param name="rootUnit"></param>
		public void SetRootUnit_Editor(apRootUnit rootUnit)
		{
			bool isChanged = (_curRootUnitInEditor != rootUnit);
			_curRootUnitInEditor = rootUnit;

			if (isChanged)
			{
				_curAnimClipInEditor = null;
			}
		}

		/// <summary>
		/// [Please do not use it] Set Anim Clip in Editor
		/// </summary>
		/// <param name="animClip"></param>
		/// <returns></returns>
		public bool SetAnimClip_Editor(apAnimClip animClip)
		{
			if (_curRootUnitInEditor == null)
			{
				//선택한 RootUnit이 없다.
				return false;
			}

			if (_curRootUnitInEditor._childMeshGroup != animClip._targetMeshGroup)
			{
				//이 RootUnit을 위한 AnimClip이 아니다.
				return false;
			}
			_curAnimClipInEditor = animClip;

			_curAnimClipInEditor.Stop_Editor();
			return true;
		}


		/// <summary>
		/// [Please do not use it] Play in Editor
		/// </summary>
		public void Play_Editor()
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			_curAnimClipInEditor.Play_Editor();
		}

		/// <summary>
		/// [Please do not use it] Pause in Editor
		/// </summary>
		public void Pause_Editor()
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			_curAnimClipInEditor.Pause_Editor();
		}

		/// <summary>
		/// [Please do not use it] Stop in Editor
		/// </summary>
		public void Stop_Editor()
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			_curAnimClipInEditor.Stop_Editor();
		}


		/// <summary>
		/// [Please do not use it] Set Frame in Editor
		/// </summary>
		/// <param name="frame"></param>
		public void SetFrame_Editor(int frame)
		{
			if (_curRootUnitInEditor == null || _curAnimClipInEditor == null)
			{
				return;
			}

			int startFrame = _curAnimClipInEditor.StartFrame;
			int endFrame = _curAnimClipInEditor.EndFrame;

			int nextFrame = Mathf.Clamp(frame, startFrame, endFrame);

			_curAnimClipInEditor.SetFrame_Editor(nextFrame);
		}


		/// <summary>
		/// [Please do not use it] Current Played Frame Index (Float) in Editor
		/// </summary>
		public float CurAnimFrameFloat_Editor
		{
			get
			{
				if (_curAnimClipInEditor == null)
				{ return 0.0f; }
				return _curAnimClipInEditor.CurFrameFloat;
			}
		}

		/// <summary>
		/// [Please do not use it] Current Played Animation Clip in Editor
		/// </summary>
		public apAnimClip CurAnimClip_Editor { get { return _curAnimClipInEditor; } }

		/// <summary>
		/// [Please do not use it] Current Played Root Unit in Editor
		/// </summary>
		public apRootUnit CurRootUnit_Editor { get { return _curRootUnitInEditor; } }



		// Get / Set
		//-------------------------------------------------------
		/// <summary>
		/// [Please do not use it] Play Queue List
		/// </summary>
		public List<apAnimPlayQueue> PlayQueueList
		{
			get
			{
				return _animPlayQueues;
			}
		}

		/// <summary>
		/// [Please do not use it] Play Data List
		/// </summary>
		public List<apAnimPlayData> PlayDataList
		{
			get
			{
				return _animPlayDataList;
			}
		}
	}

}