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
	//AnimClip을 감싸고 Runtime에서 재생이 되는 유닛.
	//Layer 정보를 가지고 블렌딩의 기준이 된다.
	//Queue의 실행 순서에 따라서 대기->페이드인(재생)->재생->페이드아웃(재생)->끝의 생명 주기를 가진다.
	//"자동 재생 종료"옵션에 따라 "Loop가 아닌 AnimClip"은 자동으로 재생이 끝나기도 한다.
	/// <summary>
	/// A class that is the unit for playing animation according to Animation Clip information.
	/// It is controlled by the "apAnimPlayQueue" and is updated.
	/// (It is recommended to use "apAnimPlayManager" to control, and it is possible to read play state.)
	/// </summary>
	public class apAnimPlayUnit
	{
		// Members
		//-----------------------------------------------
		/// <summary>[Please do not use it] Parent apAnimPlayQueue</summary>
		public apAnimPlayQueue _parentQueue = null;

		/// <summary>[Please do not use it] Linked Animation Clip</summary>
		public apAnimClip _linkedAnimClip = null;

		/// <summary>[Please do not use it] Linked Target Root Unit</summary>
		public apOptRootUnit _targetRootUnit = null;//렌더링 대상이 되는 루트 유닛

		//최종적으로 제어하고 있는 Request를 저장한다.
		//Weight 지정은 여러 Request에서 중첩적으로 하지만, 스테이트 제어는 마지막에 생성된 Request만 가능하다.
		private apAnimPlayRequest _ownerRequest_Prev = null;
		private apAnimPlayRequest _ownerRequest_Next = null;

		/// <summary>[Please do not use it] Animation Request (Prev)</summary>
		public apAnimPlayRequest PrevOwnerRequest {  get { return _ownerRequest_Prev; } }

		/// <summary>[Please do not use it] Animation Request (Next)</summary>
		public apAnimPlayRequest NextOwnerRequest {  get { return _ownerRequest_Next; } }

		/// <summary>[Please do not use it] Animation Layer</summary>
		public int _layer = -1;

		/// <summary>[Please do not use it] Animation Play Order in a layer</summary>
		public int _playOrder = -1;//<<이게 재생 순서. (Layer에 따라 증가하며, 동일 Layer에서는 Queue의 재생 순서에 따라 매겨진다.

		/// <summary>[Please do not use it] Parent Request Order</summary>
		public int _requestedOrder = -1;//재생순서와 달리, Request의 순서에 따라 매겨진다. List의 인덱스와 다를 수 있다.

		/// <summary>
		/// 대기/페이드/플레이 상태
		/// (Pause는 별도의 변수로 체크하며 여기서는 Play에 포함된다)
		/// </summary>
		public enum PLAY_STATUS
		{
			/// <summary>Ready : 등록만 되고 아무런 처리가 되지 않았다. Queue 대기 상태인 경우</summary>
			Ready = 0,
			/// <summary>Play : 플레이가 되고 있는 중</summary>
			Play = 1,
			/// <summary>End : 플레이가 모두 끝났다. (삭제 대기)</summary>
			End = 2
		}

		private PLAY_STATUS _playStatus = PLAY_STATUS.Ready;

		/// <summary>Play Status (Ready -> Play -> End)</summary>
		public PLAY_STATUS PlayStatus { get { return _playStatus; } }

		/// <summary>[Please do not use it] Pause Status</summary>
		public bool _isPause = false;


		
		public enum BLEND_METHOD
		{
			Interpolation = 0,
			Additive = 1
		}

		private BLEND_METHOD _blendMethod = BLEND_METHOD.Interpolation;

		/// <summary>[Please do not use it] Blend Method</summary>
		public BLEND_METHOD BlendMethod { get { return _blendMethod; } }

		//AnimClip이 Loop 타입이 아니라면 자동으로 종료한다.
		/// <summary>End Automatically (If it is not loop animation)</summary>
		private bool _isAutoEnd = false;

		////배속 비율 (기본값 1)
		///// <summary>Animation Play Speed Ratio (Default : 1.0)</summary>
		//private float _speedRatio = 1.0f;



		// 내부 스테이트 처리 변수
		private PLAY_STATUS _nextPlayStatus = PLAY_STATUS.Ready;
		private bool _isFirstFrame = false;

		//추가 1.14 : 프레임 리셋 여부를 자동으로 설정하지 않고, 변수를 통해 제어한다.
		private bool _isResetFrameOnReadyStatus = true;//<<기본적으로는 Ready 스테이트일때 프레임을 리셋한다.
		//private float _tFade = 0.0f;

		//총 재생 시간.
		//private float _tAnimClipLength = 0.0f;

		private float _unitWeight = 0.0f;
		//private bool _isWeightCalculated = false;
		private float _totalRequestWeights = 0.0f;

		
		/// <summary>
		/// [Please do not use it] Blend Weight of Requests
		/// </summary>
		public float TotalRequestWeights {  get { return _totalRequestWeights; } }
		
		/// <summary>
		/// [Please do not use it] Blend Weight
		/// </summary>
		public float UnitWeight
		{
			get
			{
				if (_playStatus != PLAY_STATUS.Play)
				{
					return 0.0f;
				}

				if (_totalRequestWeights > 0.0f)
				{
					return _unitWeight / _totalRequestWeights;
				}
				//if (_isWeightCalculated)
				//{
				//	//일단 빼자
				//	//Debug.LogError("Calculated가 된 Play Unit : " + _unitWeight + " / Total : " + _totalRequestWeights);
				//}

				return 1.0f;
			}
		}

		private bool _tmpIsEnd = false;
		private bool _tmpIsControlParamUpdatable = false;

		private bool _isLoop = false;

		private bool _isPlayStartEventCalled = false;
		private bool _isEndEventCalled = false;

		//public float FadeInTime { get { return _fadeInTime; } }
		//public float FadeOutTime { get { return _fadeOutTime; } }
		//public float DelayToPlayTime { get { return _delayToPlayTime; } }
		//public float DelayToEndTime {  get { return _delayToEndTime; } }


		private int _linkKey = -1;

		/// <summary>[Please do not use it] Request Key</summary>
		public int LinkKey { get { return _linkKey; }  }

		//삭제 3.1 : 1.1.4, 1.1.5에서 작성된 변수인데 1.1.6에서 일단 안씀. 버그 생기면 확인 바람
		//[NonSerialized]
		//private float _timeRatioPrev = 0.0f;

		[NonSerialized]
		private float _mecanimTime = 0.0f;

		[NonSerialized]
		private float _mecanimTimePrev = 0.0f;

		[NonSerialized]
		private float _mecanimTimeLength = 0.0f;

		[NonSerialized]
		private float _mecanimDeltaTime = 0.0f;

		[NonSerialized]
		private bool _isMecanimResetable = false;

		////1.17 추가 : 이벤트 초기화 관련하여 정밀한 처리 위해 변수 추가됨
		//[NonSerialized]
		//private int _mecanimTimeRatioLoopCount = -1;

		//[NonSerialized]
		//private bool _isMecanimLoopingFrame = false;

		



		// Init
		//-----------------------------------------------
		public apAnimPlayUnit(apAnimPlayQueue parentQueue, int requestedOrder, int linkKey)
		{
			_parentQueue = parentQueue;
			_requestedOrder = requestedOrder;
			_linkKey = linkKey;

			//_isMecanimPlayUnit = false;
		}

		public void SetMecanimPlayUnit()
		{
			//_isMecanimPlayUnit = true;
			_mecanimTime = 0.0f;
			_mecanimTimePrev = 0.0f;
			//_timeRatioPrev = 0.0f;//<<1.1.6에서 삭제
		}

		

		

		/// <summary>
		/// [Please do not use it] Set AnimClip to get data
		/// </summary>
		/// <param name="playData"></param>
		/// <param name="layer"></param>
		/// <param name="blendMethod"></param>
		/// <param name="isAutoEndIfNotLoop"></param>
		/// <param name="isEditor"></param>
		public void SetAnimClip(apAnimPlayData playData, int layer, BLEND_METHOD blendMethod, bool isAutoEndIfNotLoop, bool isEditor)
		{
			_linkedAnimClip = playData._linkedAnimClip;
			_targetRootUnit = playData._linkedOptRootUnit;

			//추가
			if (_linkedAnimClip._parentPlayUnit != null
				&& _linkedAnimClip._parentPlayUnit != this)
			{
				//이미 다른 PlayUnit이 사용중이었다면..
				_linkedAnimClip._parentPlayUnit.SetEnd();
				//_linkedAnimClip._parentPlayUnit._linkedAnimClip = null;
			}
			_linkedAnimClip._parentPlayUnit = this;

			_layer = layer;

			_isLoop = _linkedAnimClip.IsLoop;
			_isAutoEnd = isAutoEndIfNotLoop;
			if (_isLoop)
			{
				_isAutoEnd = false;//<<Loop일때 AutoEnd는 불가능하다
			}


			_blendMethod = blendMethod;

			_isPause = false;
			_playStatus = PLAY_STATUS.Ready;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;

			
			//_speedRatio = 1.0f;

			_isFirstFrame = true;
			_nextPlayStatus = _playStatus;

			//추가 1.14 
			_isResetFrameOnReadyStatus = true;

			if (isEditor)
			{
				_linkedAnimClip.Stop_Editor(false);//Stop은 하되 업데이트는 하지 않는다. (false)
			}
			else
			{
				_linkedAnimClip.Stop_Opt(false);
			}

			_unitWeight = 0.0f;
			_totalRequestWeights = 0.0f;

			
		}


		/// <summary>
		/// [Please do not use it] Set Playing option
		/// </summary>
		/// <param name="blendMethod"></param>
		/// <param name="isAutoEndIfNotLoop"></param>
		/// <param name="newRequestedOrder"></param>
		/// <param name="newLinkKey"></param>
		public void SetSubOption(BLEND_METHOD blendMethod, bool isAutoEndIfNotLoop, int newRequestedOrder, int newLinkKey)
		{
			_blendMethod = blendMethod;
			_isAutoEnd = isAutoEndIfNotLoop;
			_requestedOrder = newRequestedOrder;
			_linkKey = newLinkKey;

			if (_isLoop)
			{
				_isAutoEnd = false;//<<Loop일때 AutoEnd는 불가능하다
			}
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="request"></param>
		public void SetOwnerRequest_Prev(apAnimPlayRequest request)
		{
			_ownerRequest_Prev = request;
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="request"></param>
		public void SetOwnerRequest_Next(apAnimPlayRequest request)
		{
			_ownerRequest_Next = request;
		}

		// Update
		//-----------------------------------------------
		#region [미사용 코드] UnitWeight를 계산하는건 외부에서 일괄적으로 한다. 자체적으로 하면 문제가 많다.
		///// <summary>
		///// Update 직전에 UnitWeight를 계산한다.
		///// 유효하지 않을 경우 -1 리턴.
		///// 꼭 Update 직전에 호출해야한다.
		///// 실제 Clip 업데이트 전에 타이머/스테이트 처리등을 수행한다.
		///// </summary>
		///// <returns></returns>
		//public float CalculateUnitWeight(float tDelta)
		//{
		//	_tmpIsEnd = false;

		//	if(_linkedAnimClip._parentPlayUnit != this)
		//	{
		//		return -1.0f;
		//	}

		//	PLAY_STATUS requestedNextPlayStatus = _nextPlayStatus;

		//	switch (_playStatus)
		//	{
		//		case PLAY_STATUS.Ready:
		//			{
		//				if (_isFirstFrame)
		//				{
		//					_unitWeight = 0.0f;
		//					//_prevUnitWeight = 0.0f;
		//				}
		//				//if (!_isPause)
		//				//{
		//				//	if (_isDelayIn)
		//				//	{
		//				//		//딜레이 후에 플레이된다.
		//				//		_tDelay += tDelta;
		//				//		if (_tDelay > _delayToPlayTime)
		//				//		{
		//				//			_unitWeight = 0.0f;
		//				//			_isDelayIn = false;
		//				//			ChangeNextStatus(PLAY_STATUS.PlayWithFadeIn);//<<플레이 된다.
		//				//		}
		//				//	}
		//				//}
		//			}
		//			break;


		//		//case PLAY_STATUS.PlayWithFadeIn:
		//		//	{
		//		//		if(_isFirstFrame)
		//		//		{
		//		//			_tFade = 0.0f;
		//		//			_prevUnitWeight = _unitWeight;
		//		//		}
		//		//		if (!_isPause)
		//		//		{
		//		//			_tFade += tDelta;

		//		//			if (_tFade < _fadeInTime)
		//		//			{
		//		//				_unitWeight = (_prevUnitWeight * (_fadeInTime - _tFade) + 1.0f * _tFade) / _fadeInTime;
		//		//			}
		//		//			else
		//		//			{
		//		//				_unitWeight = 1.0f;
		//		//				//Fade가 끝났으면 Play
		//		//				ChangeNextStatus(PLAY_STATUS.Play);
		//		//			}
		//		//		}
		//		//	}
		//		//	break;

		//		case PLAY_STATUS.Play:
		//			{
		//				if(_isFirstFrame)
		//				{
		//					_unitWeight = 1.0f;
		//					//_prevUnitWeight = 1.0f;
		//				}

		//				if (!_isPause)
		//				{
		//					//if (_isDelayOut)
		//					//{
		//					//	//딜레이 후에 FadeOut된다.
		//					//	_tDelay += tDelta;
		//					//	if (_tDelay > _delayToEndTime)
		//					//	{
		//					//		_isDelayOut = false;
		//					//		_unitWeight = 1.0f;
		//					//		ChangeNextStatus(PLAY_STATUS.PlayWithFadeOut);//<<플레이 종료를 위한 FadeOut
		//					//	}
		//					//}
		//				}
		//			}
		//			break;

		//		case PLAY_STATUS.PlayWithFadeOut:
		//			{
		//				if(_isFirstFrame)
		//				{
		//					_tFade = 0.0f;
		//					_prevUnitWeight = _unitWeight;
		//				}

		//				if (!_isPause)
		//				{
		//					_tFade += tDelta;

		//					if (_tFade < _fadeOutTime)
		//					{
		//						_unitWeight = (_prevUnitWeight * (_fadeOutTime - _tFade) + 0.0f * _tFade) / _fadeOutTime;
		//					}
		//					else
		//					{
		//						_unitWeight = 0.0f;
		//						ChangeNextStatus(PLAY_STATUS.End);
		//					}
		//				}
		//			}
		//			break;


		//		case PLAY_STATUS.End:
		//			{
		//				//아무것도 안합니더
		//				if(_isFirstFrame)
		//				{
		//					//Debug.Log("End");
		//					_unitWeight = 0.0f;
		//				}

		//			}
		//			break;
		//	}

		//	if(_playOrder == 0)
		//	{
		//		return 1.0f;
		//	}
		//	return _unitWeight;
		//} 
		#endregion

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="isCalculated"></param>
		public void SetWeight(float weight, bool isCalculated)
		{
			//외부에서 Weight를 지정한다.
			_unitWeight = weight;
			//_isWeightCalculated = isCalculated;
			_totalRequestWeights = 0.0f;

			//_debugWeight1 = _unitWeight;
			//_debugWeight2 = _unitWeight;
			//_debugWeight3 = _unitWeight;
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="multiplyUnitWeight"></param>
		/// <param name="requestWeight"></param>
		public void AddWeight(float multiplyUnitWeight, float requestWeight)
		{
			//외부에서 Weight를 지정한다.
			//_unitWeight = Mathf.Clamp01(_unitWeight * multiplyRatio);
			//_unitWeight = Mathf.Clamp01((_unitWeight * multiplyUnitWeight * requestWeight) + (_unitWeight * (1-requestWeight)));
			_unitWeight = _unitWeight + (multiplyUnitWeight * requestWeight);
			_totalRequestWeights += requestWeight;

			//_isWeightCalculated = true;
			//Debug.Log(">> Weight [" + _linkedAnimClip._name + "] : " + _unitWeight + " / Request Weight : " + _totalRequestWeights);
			
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="normalizeWeight"></param>
		public void NormalizeWeight(float normalizeWeight)
		{
			_unitWeight *= normalizeWeight;
		}

		/// <summary>
		/// [Please do not use it] Update Animation
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update(float tDelta)
		{
			_tmpIsEnd = false;
			_tmpIsControlParamUpdatable = false;

			
			if (_linkedAnimClip._parentPlayUnit != this)
			{
				//PlayUnit이 더이상 이 AnimClip을 제어할 수 없게 되었다
				//Link Release를 하고 업데이트도 막는다.
				
				ReleaseLink();
				return;
			}

			PLAY_STATUS requestedNextPlayStatus = _nextPlayStatus;
			float speedRatio = _linkedAnimClip._speedRatio;

			switch (_playStatus)
			{
				case PLAY_STATUS.Ready:
					{
						if (_isFirstFrame)
						{
							//_unitWeight = 0.0f;
							//_prevUnitWeight = 0.0f;
							_linkedAnimClip.SetPlaying_Opt(false);
							if (_isResetFrameOnReadyStatus)
							{
								_linkedAnimClip.SetFrame_Opt(_linkedAnimClip.StartFrame, false);
							}
							_isResetFrameOnReadyStatus = true;//True가 기본값
						}
						
					}
					break;


				

				case PLAY_STATUS.Play:
					{
						if (_isFirstFrame)
						{
							//_unitWeight = 1.0f;
							//_prevUnitWeight = 1.0f;

							//플레이 시작했다고 알려주자
							if (!_isPlayStartEventCalled)
							{
								_parentQueue.OnAnimPlayUnitPlayStart(this);
								_isPlayStartEventCalled = true;
							}
							//Debug.Log("Play");
							_linkedAnimClip.SetPlaying_Opt(true);
						}

						if (!_isPause)
						{
							_linkedAnimClip.SetPlaying_Opt(true);
							_tmpIsEnd = _linkedAnimClip.Update_Opt(tDelta * speedRatio);
							_tmpIsControlParamUpdatable = true;
							
						}
						else
						{
							_linkedAnimClip.SetPlaying_Opt(false);

							//변경 22.1.9 : Pause 상태에서도 컨트롤 파라미터가 업데이트 되어야 한다.
							_tmpIsControlParamUpdatable = true;
						}
					}
					break;
					
				case PLAY_STATUS.End:
					{
						//아무것도 안합니더
						if (_isFirstFrame)
						{
							ReleaseLink();
						}

					}
					break;
			}

			if (_tmpIsEnd && _isAutoEnd)
			{
				//종료가 되었다면 (일단 Loop는 아니라는 것)
				//조건에 따라 End로 넘어가자
				SetEnd();
			}

			//스테이트 처리
			//if(_nextPlayStatus != _playStatus)
			if (requestedNextPlayStatus != _playStatus)
			{
				//-----------------------------------------------
				//수정 21.4.1 : 애니메이션 재생시 루트유닛이 바뀐다면 다음 프레임에 OnAnimPlayUnitPlayStart를 호출하지 말고 바로 여기서 호출하자
				if (requestedNextPlayStatus == PLAY_STATUS.Play)
				{
					//플레이 시작했다고 알려주자
					if (!_isPlayStartEventCalled)
					{
						_parentQueue.OnAnimPlayUnitPlayStart(this);
						_isPlayStartEventCalled = true;
						//추가 > 컨트롤 파라미터가 Ready > Play로 넘어가는 1프레임 업데이트 안되는 문제가 있다.
						_tmpIsControlParamUpdatable = true;//업데이트 할 수 있게 만들자
					}
				}
				//--------------------------------------------------

				_playStatus = requestedNextPlayStatus;
				_nextPlayStatus = _playStatus;
				_isFirstFrame = true;

			}
			else if (_isFirstFrame)
			{
				_isFirstFrame = false;
			}
		}





		/// <summary>
		/// [Please do not use it] Update Animation in Inspector
		/// </summary>
		/// <param name="tDelta"></param>
		public void Update_InspectorPreview()
		{
			_tmpIsEnd = false;
			_tmpIsControlParamUpdatable = false;

			
			if (_linkedAnimClip._parentPlayUnit != this)
			{
				//PlayUnit이 더이상 이 AnimClip을 제어할 수 없게 되었다
				//Link Release를 하고 업데이트도 막는다.
				ReleaseLink();
				return;
			}

			PLAY_STATUS requestedNextPlayStatus = _nextPlayStatus;
			
			switch (_playStatus)
			{
				case PLAY_STATUS.Ready:
					{
						if (_isFirstFrame)
						{
							_linkedAnimClip.SetPlaying_Opt(false);
							if (_isResetFrameOnReadyStatus)
							{
								_linkedAnimClip.SetFrame_Opt(_linkedAnimClip.StartFrame, false);
							}
							_isResetFrameOnReadyStatus = true;//True가 기본값
						}
					}
					break;
				

				case PLAY_STATUS.Play:
					{
						if (_isFirstFrame)
						{
							//플레이 시작했다고 알려주자
							if (!_isPlayStartEventCalled)
							{
								_parentQueue.OnAnimPlayUnitPlayStart(this);
								_isPlayStartEventCalled = true;
							}
							//Debug.Log("Play");
							_linkedAnimClip.SetPlaying_Opt(true);
						}

						if (!_isPause)
						{
							_linkedAnimClip.SetPlaying_Opt(true);
							//_tmpIsEnd = _linkedAnimClip.Update_Opt(0.0f);
							_tmpIsEnd = false;//<< 이게 인스펙터용으로 바뀜
							_tmpIsControlParamUpdatable = true;
						}
						else
						{
							_linkedAnimClip.SetPlaying_Opt(false);

							//변경 22.1.9 : Pause 상태에서도 컨트롤 파라미터가 업데이트 되어야 한다.
							_tmpIsControlParamUpdatable = true;
						}
					}
					break;
					
				case PLAY_STATUS.End:
					{
						//아무것도 안합니더
						if (_isFirstFrame)
						{
							ReleaseLink();
						}

					}
					break;
			}

			if (_tmpIsEnd && _isAutoEnd)
			{
				//종료가 되었다면 (일단 Loop는 아니라는 것)
				//조건에 따라 End로 넘어가자
				SetEnd();
			}

			//스테이트 처리
			//if(_nextPlayStatus != _playStatus)
			if (requestedNextPlayStatus != _playStatus)
			{
				//-----------------------------------------------
				//수정 21.4.1 : 애니메이션 재생시 루트유닛이 바뀐다면 다음 프레임에 OnAnimPlayUnitPlayStart를 호출하지 말고 바로 여기서 호출하자
				if (requestedNextPlayStatus == PLAY_STATUS.Play)
				{
					//플레이 시작했다고 알려주자
					if (!_isPlayStartEventCalled)
					{
						_parentQueue.OnAnimPlayUnitPlayStart(this);
						_isPlayStartEventCalled = true;
						//추가 > 컨트롤 파라미터가 Ready > Play로 넘어가는 1프레임 업데이트 안되는 문제가 있다.
						_tmpIsControlParamUpdatable = true;//업데이트 할 수 있게 만들자
					}
				}
				//--------------------------------------------------

				_playStatus = requestedNextPlayStatus;
				_nextPlayStatus = _playStatus;
				_isFirstFrame = true;

			}
			else if (_isFirstFrame)
			{
				_isFirstFrame = false;
			}
		}




		//Control Param은 약간 지연되어 다른 타이밍에 업데이트 되어야 한다.
		public void UpdateControlParamOpt()
		{
			if(!_tmpIsControlParamUpdatable)
			{
				return;
			}

			_linkedAnimClip.UpdateControlParamOpt();
		}



		private void ChangeNextStatus(PLAY_STATUS nextStatus)
		{
			_nextPlayStatus = nextStatus;
		}


		





		// Functions
		//-----------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// "Play()" is called by apAnimPlayManager
		/// </summary>
		public void Play()
		{
			if (_playStatus == PLAY_STATUS.Ready)
			{
				_isPause = false;
				_unitWeight = 0.0f;
				
				_isPlayStartEventCalled = false;
				_isEndEventCalled = false;

				//바로 시작
				ChangeNextStatus(PLAY_STATUS.Play);
			}
		}

		//1.15 추가 : 시작시 프레임을 설정할 수 있다.
		/// <summary>
		/// [Please do not use it]
		/// "PlayAt()" is called by apAnimPlayManager
		/// </summary>
		public void PlayAt(int frame)
		{
			if (_playStatus == PLAY_STATUS.Ready)
			{
				_isPause = false;
				_unitWeight = 0.0f;
				
				_isPlayStartEventCalled = false;
				_isEndEventCalled = false;

				
				_linkedAnimClip.SetFrame_Opt(frame, true);//<<여기가 바뀜 + 자동으로 Clamp

				//바로 시작
				ChangeNextStatus(PLAY_STATUS.Play);
			}
		}

		//일반적인 Play와 달리 강제로 재시작을 한다.
		
		/// <summary>
		/// [Please do not use it]
		/// "ResetPlay()" is called by apAnimPlayManager
		/// </summary>
		public void ResetPlay()
		{
			_isPause = false;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.Play);
			_isFirstFrame = true;

			_linkedAnimClip.SetFrame_Opt(_linkedAnimClip.StartFrame, false);
		}

		//추가 1.4 : 특정 프레임에서 시작을 하도록 초기화
		/// <summary>
		/// [Please do not use it]
		/// "ResetPlayAt()" is called by apAnimPlayManager
		/// </summary>
		public void ResetPlayAt(int frame)
		{
			_isPause = false;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.Play);
			_isFirstFrame = true;

			//변경 사항 1.14
			_isResetFrameOnReadyStatus = false;//<<이걸 False로 해야 처음 시작시 프레임이 초기화되는 걸 막을 수 있다.

			_linkedAnimClip.SetFrame_Opt(frame, true);//<<여기가 바뀜 + 자동으로 Clamp
			
		}

		/// <summary>
		/// [Please do not use it]
		/// ResetFrame() without other processing
		/// </summary>
		public void ResetFrame()
		{
			_linkedAnimClip.ResetFrame();
		}

		/// <summary>
		/// [Please do not use it]
		/// "Resume" is called by apAnimPlayManager
		/// </summary>
		public void Resume()
		{
			_isPause = false;
			
		}

		/// <summary>
		/// [Please do not use it]
		/// "Pause()" is called by apAnimPlayManager
		/// </summary>
		public void Pause()
		{
			_isPause = true;
		}

		//삭제 : 이 함수를 호출할 수 없다.
		///// <summary>
		///// Set SpeedRatio (Defulat : 1.0)
		///// </summary>
		///// <param name="speedRatio"></param>
		//public void SetSpeed(float speedRatio)
		//{
		//	_speedRatio = speedRatio;
		//}


		
		/// <summary>
		/// [Please do not use it]
		/// "SetEnd()" is called by apAnimPlayManager
		/// </summary>
		public void SetEnd(bool isDirectStateChange = false)
		{
			_unitWeight = 0.0f;
			
			_totalRequestWeights = 1.0f;
			//_isWeightCalculated = true;

			_isPlayStartEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.End);

			if (isDirectStateChange)
			{
				if (_nextPlayStatus != _playStatus)
				{
					_playStatus = _nextPlayStatus;
					_isFirstFrame = true;
				}
				else if (_isFirstFrame)
				{
					_isFirstFrame = false;
				}
			}
		}


		// SetEnd와 비슷하지만 PlayStatus를 Ready로 바꾼다.
		/// <summary>
		/// [Please do not use it] Initialize Weight
		/// </summary>
		public void SetWeightZero()
		{
			_unitWeight = 0.0f;
			_totalRequestWeights = 1.0f;
			//_isWeightCalculated = true;

			_isPlayStartEventCalled = false;
			ChangeNextStatus(PLAY_STATUS.Ready);
		}


		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void ReleaseLink()
		{
			//연결된 Calculate와 연동을 끊는다.
			if (!_isEndEventCalled)
			{
				_parentQueue.OnAnimPlayUnitEnded(this);
				_isEndEventCalled = true;
				_playStatus = PLAY_STATUS.End;
			}
		}


		// 메카님 전용 함수
		//---------------------------------------------------------------------
		
		public void Mecanim_Link(apAnimClip animClip)
		{
			_playStatus = PLAY_STATUS.Play;

			_linkedAnimClip = animClip;
			animClip._parentPlayUnit = this;

			_isPause = false;
			_isPlayStartEventCalled = false;
			_isEndEventCalled = false;
			_isFirstFrame = true;
			_mecanimTime = 0.0f;
			_mecanimTimePrev = 0.0f;
			_mecanimTimeLength = animClip.TimeLength;
			//_timeRatioPrev = 0.0f;//<<1.1.6에서 삭제
			
			
			if (!animClip.IsLoop)
			{
				animClip.Stop_Opt(false);
				
				
			}
			
		}
		public void Mecanim_Unlink()
		{
			_playStatus = PLAY_STATUS.End;
			if(_linkedAnimClip != null)
			{
				_linkedAnimClip.Stop_Opt(false);
				

				_linkedAnimClip._parentPlayUnit = null;
				_linkedAnimClip = null;
			}

			_unitWeight = 0.0f;
			_totalRequestWeights = 1.0f;

			_totalRequestWeights = 1.0f;
			//_isWeightCalculated = true;
			_isPlayStartEventCalled = false;

			_mecanimTime = 0.0f;
			_mecanimTimePrev = 0.0f;
			_mecanimTimeLength = 1.0f;
			//_timeRatioPrev = 0.0f;//<<1.1.6에서 삭제

			
		}

		public void Mecanim_Update(float weight, float timeRatio, int playOrder, int playLayer, BLEND_METHOD blendMethod, float speed)
		{
			if(_linkedAnimClip == null)
			{
				//Debug.LogError("No AnimClip");
				return;
			}
			_unitWeight = weight;
			_totalRequestWeights = 1.0f;

			_blendMethod = blendMethod;
			_playOrder = playOrder;
			_layer = playLayer;
			
			_isMecanimResetable = false;
			//_isMecanimLoopingFrame = false;

			//2.28 일단 이거 제외 [1.1.6]
			//1.1.4 / 1.1.5에서 메카님 음수 speed에 적용하려고 만든 것 같은데, 오작동을 일으킨다.
			//Debug.Log("Mecanim [" + timeRatio + " ( " + speed + " )]");
			//if((speed > 0.0f && timeRatio < _timeRatioPrev) ||
			//	(speed < 0.0f && timeRatio > _timeRatioPrev))
			//{
			//	//Debug.LogWarning("[" + _linkedAnimClip._name + "] 메카님 Speed와 TimeRatio 증감이 반대 : " + (timeRatio - _timeRatioPrev) + " / Speed : " + speed);
			//	//Debug.LogWarning("[" + _linkedAnimClip._name + "] 메카님 Speed와 TimeRatio 증감이 반대 : " + _timeRatioPrev + " >> " + timeRatio + " / Speed : " + speed);
			//	//Speed와 맞지 않은 증감폭이다.
				
			//	//이전
			//	//timeRatio = _timeRatioPrev - (timeRatio - _timeRatioPrev);<<이게 문제.. 근데 왜..
			//}
			//_timeRatioPrev = timeRatio;//<<1.1.6에서 삭제


			//Loop가 아닌 경우, 첫번째 프레임/ 마지막 프레임을 감지해야한다.
			bool isOverLastFrame = false;
			bool isOverFirstFrame = false;

			
			if(_linkedAnimClip.IsLoop)
			{
				//int curTimeRatioLoopCount = (int)timeRatio;
				//if(_mecanimTimeRatioLoopCount != curTimeRatioLoopCount)
				//{
				//	Debug.LogError("[Index Changed] _isMecanimLoopingFrame > True : " + timeRatio + " (" + _mecanimTimeRatioLoopCount + " >> " + curTimeRatioLoopCount + ")");
				//	//루프 카운트가 바뀌었다. > //이번 프레임에서 루프가 된다.
				//	_isMecanimLoopingFrame = true;
				//	_mecanimTimeRatioLoopCount = curTimeRatioLoopCount;
				//}
				//else
				//{
				//	if(
				//		(speed > 0.0f && timeRatio < 0.0f) ||
				//		(speed < 0.0f && timeRatio > 0.0f)
				//		)
				//	{
				//		Debug.LogError("[Speed And Time Inverted] _isMecanimLoopingFrame > True : " + timeRatio + " (Speed : " + speed + ")");
				//		//앞으로 진행하는데, timeRatio가 뒤로 가거나 또는 그 반대
				//		//Loop가 발생한 것이다.
				//		_isMecanimLoopingFrame = true;
				//	}
				//}

				if(timeRatio > 1.0f)
				{	
					timeRatio -= (int)timeRatio;
				}
				else if(timeRatio < 0.0f)
				{
					timeRatio = (1.0f - Mathf.Abs(timeRatio) - (int)Mathf.Abs(timeRatio));
				}

				
			}
			else
			{
				if(timeRatio > 1.0f)
				{
					//timeRatio = 2.0f;//이전					
					timeRatio = 1.0f;//변경

					isOverLastFrame = true;//마지막 프레임을 지나갔다.
				}
				else if(timeRatio < 0.0f)
				{
					//timeRatio = -1.0f;//이전
					timeRatio = 0.0f;//변경

					isOverFirstFrame = true;//첫번째 프레임을 지나갔다 (음수로)
				}
				else
				{
					//Time Ratio가 0~1의 값을 가지면 Reset이 가능하다.
					_isMecanimResetable = true;
				}
			}
			
			_mecanimTime = timeRatio * _mecanimTimeLength;

			

			bool isResetFrame = false;


			//루프가 아닌데 재생 시간이 재생 방향의 반대로 설정된 경우
			if (!_linkedAnimClip.IsLoop && _isMecanimResetable)
			{
				if (speed > 0.0f)
				{
					if (_mecanimTime < _mecanimTimePrev)
					{
						isResetFrame = true;
						_linkedAnimClip.ResetEvents();
						_linkedAnimClip.ResetFrame();
					}
				}
				else if (speed < 0.0f)
				{
					if (_mecanimTime > _mecanimTimePrev)
					{
						isResetFrame = true;
						_linkedAnimClip.ResetEvents();
						_linkedAnimClip.ResetFrame();
					}
				}
			}
			//if(_isMecanimLoopingFrame)
			//{
			//	Debug.LogError(">> Reset Event");
			//	_linkedAnimClip.ResetEvents();
			//}

			//TODO : 여기서 tDelta로만 Forward/Backward를 결정하는게 아니라, 메카님의 Speed로 방향을 인식하도록 바꾸어야 한다.
			if(!isResetFrame)
			{
				_mecanimDeltaTime = _mecanimTime - _mecanimTimePrev;
				if (_mecanimTimeLength > 0.0f)
				{
					if (_mecanimDeltaTime < 0.0f && speed > 0.0f)
					{
						//진행 방향과 시간이 반대라면
						while (_mecanimDeltaTime < 0.0f)
						{
							_mecanimDeltaTime += _mecanimTimeLength;
						}
					}
					else if (_mecanimDeltaTime > 0.0f && speed < 0.0f)
					{
						//진행 방향과 시간이 반대라면
						while (_mecanimDeltaTime > 0.0f)
						{
							_mecanimDeltaTime -= _mecanimTimeLength;
						}
					}
				}
				
				//변경 1.17 : 일반 Update_Opt에서 UpdateMecanim_Opt로 변경한다.
				//_linkedAnimClip.UpdateMecanim_Opt(_mecanimTime - _mecanimTimePrev, speed);
			}
			else
			{
				_mecanimDeltaTime = _mecanimTime;
				//변경 1.17 : 일반 Update_Opt에서 UpdateMecanim_Opt로 변경한다.
				//_linkedAnimClip.UpdateMecanim_Opt(_mecanimTime, speed);
			}

			
			_linkedAnimClip.UpdateMecanim_Opt(_mecanimDeltaTime, speed, isOverLastFrame, isOverFirstFrame);
			
			_mecanimTimePrev = _mecanimTime;
		}




		//추가 21.6.10 : 메카님과 유사하지만 "다른 애니메이션 클립과 동기화되어서 동작하는 경우"
		public void Sync_Update(apAnimPlayUnit syncPlayUnit, apAnimClip syncAnimClip)
		{
			if(_linkedAnimClip == null) { return; }

			//대부분의 값은 동기화 대상과 동일하다
			_unitWeight = syncPlayUnit._unitWeight;
			_totalRequestWeights = syncPlayUnit._totalRequestWeights;

			_blendMethod = syncPlayUnit._blendMethod;
			_playOrder = syncPlayUnit._playOrder;
			_layer = syncPlayUnit._layer;
			
			_playStatus = syncPlayUnit._playStatus;//재생 상태 동기화

			_isMecanimResetable = false;

			
			
			_linkedAnimClip.UpdateSync_Opt(syncAnimClip);
			
			_mecanimTimePrev = _mecanimTime;
		}





		// Get / Set
		//-----------------------------------------------
		// 재생이 끝나고 삭제를 해야하는가
		/// <summary>
		/// [Please do not use it] Is Ended
		/// </summary>
		public bool IsRemovable { get { return _playStatus == PLAY_STATUS.End; } }

		/// <summary>
		/// [Please do not use it] Is Updatable status (Ready or Play)
		/// </summary>
		public bool IsUpdatable
		{
			get
			{
				return _playStatus == PLAY_STATUS.Ready ||
					//_playStatus == PLAY_STATUS.PlayWithFadeIn ||
					_playStatus == PLAY_STATUS.Play;
				//_playStatus == PLAY_STATUS.PlayWithFadeOut;
			}
		}

		/// <summary>
		/// Is Loop Animation
		/// </summary>
		public bool IsLoop { get { return _isLoop; } }


		//PlayUnit이 자동으로 종료가 되는가. 이게 True여야 Queued Play가 가능하다
		//[Loop가 아니어야 하며, isAutoEndIfNotLoop = true여야 한다]
		/// <summary>
		/// Is it not a Loop animation and has an automatic end request?
		/// </summary>
		public bool IsEndAutomaticallly
		{
			get
			{
				if (_isLoop)
				{
					return false;
				}
				return _isAutoEnd;
			}
		}

		/// <summary>
		/// Remaining playing time. (Return -1 if it is loop animation).
		/// </summary>
		public float RemainPlayTime
		{
			get
			{
				if (_isLoop)
				{
					return -1.0f;
				}
				return _linkedAnimClip.TimeLength - _linkedAnimClip.TotalPlayTime;
			}
		}

		/// <summary>
		/// Total played time
		/// </summary>
		public float TotalPlayTime
		{
			get
			{
				return _linkedAnimClip.TotalPlayTime;
			}
		}

		/// <summary>
		/// Animation Time Length
		/// </summary>
		public float TimeLength
		{
			get
			{
				return _linkedAnimClip.TimeLength;
			}
		}

		public int Frame
		{
			get
			{
				return _linkedAnimClip.CurFrame;
			}
		}
		public int StartFrame
		{
			get
			{
				return _linkedAnimClip.StartFrame;
			}
		}
		public int EndFrame
		{
			get
			{
				return _linkedAnimClip.EndFrame;
			}
		}

		/// <summary>
		/// [Please do not use it] Set Play Order
		/// </summary>
		/// <param name="playOrder"></param>
		public void SetPlayOrder(int playOrder)
		{
			_playOrder = playOrder;
		}
	}

}