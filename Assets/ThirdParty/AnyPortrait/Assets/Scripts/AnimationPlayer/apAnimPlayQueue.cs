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
	/// A class that is stored in Queue type and determines Play order
	/// (This is done automatically, so scripted control is not recommended.)
	/// </summary>
	public class apAnimPlayQueue
	{
		// Members
		//----------------------------------------------
		public apPortrait _portrait = null;
		public apAnimPlayManager _playManager = null;

		public List<apAnimPlayUnit> _animPlayUnits = new List<apAnimPlayUnit>();

		//TODO : 블렌드하자 + Request 받고 천천히 처리 방식

		private int _layer = -1;
		public int _nPlayedUnit = 0;
		
		private bool _isInitPlayUnit = false;


		private bool _isAnyUnitChanged = false;

		private apAnimPlayUnit _tmpCurPlayUnit = null;
		//private float _totalWeight = 0.0f;

		public List<apAnimPlayRequest> _requests_Live = new List<apAnimPlayRequest>();

		//RequestPool을 만들어서 관리한다.
		private List<apAnimPlayRequest> _requests_Total = new List<apAnimPlayRequest>();
		private List<apAnimPlayRequest> _requests_Remained = new List<apAnimPlayRequest>();

		//"마지막 Request"를 저장한 뒤, 여기서 모든 End 시점을 파악한다.
		//private apAnimPlayRequest _lastRequest = null;

		private const int NUM_REQUEST_POOL_SIZE = 20;//기본 20개 사이즈를 가진다.

		private const float DEFAULT_FADE_TIME = 0.2f;


		private int _nextRequestLinkKey = 0;
		private int GetNextRequestLinkKey()
		{
			int resultLinkKey = _nextRequestLinkKey;
			_nextRequestLinkKey++;
			if(_nextRequestLinkKey > 100000)
			{
				//적당히 이정도면 Anim이 전부 블렌드 되고 없어졌겠지..
				_nextRequestLinkKey = 0;
			}
			return resultLinkKey;
		}


		private const int QUEUE_CHAIN__1_LINK_CHAIN = 1;
		private const int QUEUE_CHAIN__2_PLAY = 2;
		private const int QUEUE_CHAIN__3_SKIP = 3;

		// Init
		//----------------------------------------------
		public apAnimPlayQueue(int layer, apPortrait portrait, apAnimPlayManager playManager)
		{
			_layer = layer;
			_portrait = portrait;
			_playManager = playManager;


			Clear();
			_isAnyUnitChanged = false;

			_requests_Live.Clear();
			_requests_Total.Clear();
			_requests_Remained.Clear();

			AddRequestPool();//<<Pool을 만든다.

		}


		// 초기화의 Clear이다. 
		// 애니메이션을 정지시킬때는 Stop 함수를 써야한다. (그래야 Modifier에서 인식을 한다)
		/// <summary>
		/// Clear Data
		/// </summary>
		public void Clear()
		{
			_animPlayUnits.Clear();
			_nPlayedUnit = 0;
			_isInitPlayUnit = false;

			PushAllRequests();

			//Order를 갱신한다.
			RefreshOrder();
		}

		// Functions
		//----------------------------------------------
		// 기본 함수들
		private void AddRequestPool()
		{
			for (int i = 0; i < NUM_REQUEST_POOL_SIZE; i++)
			{
				apAnimPlayRequest newRequest = new apAnimPlayRequest();
				_requests_Total.Add(newRequest);
				_requests_Remained.Add(newRequest);
			}
		}

		private void PushAllRequests()
		{
			apAnimPlayRequest curRequest = null;
			for (int i = 0; i < _requests_Live.Count; i++)
			{
				curRequest = _requests_Live[i];
				curRequest.Clear();

				if (!_requests_Total.Contains(curRequest))
				{
					_requests_Total.Add(curRequest);
				}
				if (!_requests_Remained.Contains(curRequest))
				{
					_requests_Remained.Add(curRequest);
				}
			}

			_requests_Live.Clear();
		}

		private apAnimPlayRequest PopRequest()
		{
			apAnimPlayRequest popRequest = null;
			if (_requests_Remained.Count == 0)
			{
				AddRequestPool();//<<Pool Size를 늘리자
			}
			popRequest = _requests_Remained[0];

			//Remained -> Live
			_requests_Remained.RemoveAt(0);
			_requests_Live.Add(popRequest);

			popRequest.Clear();
			

			return popRequest;
		}

		private void PushRequest(apAnimPlayRequest request)
		{
			request.ReleasePlayUnitLink();
			request.Clear();
			_requests_Live.Remove(request);

			if (!_requests_Total.Contains(request))
			{
				_requests_Total.Add(request);
			}
			if (!_requests_Remained.Contains(request))
			{
				_requests_Remained.Add(request);
			}
		}

		/// <summary>
		/// 활성화되지 않은 Request를 미리 해제한다.
		/// </summary>
		private void PushAllNoActiveRequests()
		{
			//Debug.LogError("실행 대기 중인 Request를 해제한다.");
			List<apAnimPlayRequest> targetRequests = new List<apAnimPlayRequest>();
			apAnimPlayRequest curPlayRequest = null;
			for (int i = 0; i < _requests_Live.Count; i++)
			{
				curPlayRequest = _requests_Live[i];
				if(curPlayRequest.IsCancellable)
				{
					targetRequests.Add(curPlayRequest);
				}
			}

			for (int i = 0; i < targetRequests.Count; i++)
			{
				PushRequest(targetRequests[i]);
			}
		}


		private apAnimPlayUnit MakePlayUnit(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop)
		{
			//새로 만들고
			//그 전에..
			//재생중인 PlayUnit이 있으면 그걸 사용하자
			//레이어는 같아야 한다.
			apAnimPlayUnit existPlayUnit = null;
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				if (_animPlayUnits[i]._linkedAnimClip == playData._linkedAnimClip
					&& _animPlayUnits[i].IsUpdatable

					)
				{
					existPlayUnit = _animPlayUnits[i];
					break;
				}
			}
			if (existPlayUnit != null)
			{


				//Debug.Log("아직 재생중인 PlayUnit을 다시 재생하는 요청이 왔다. [" + existPlayUnit._linkedAnimClip._name + "]");
				existPlayUnit.SetSubOption(blendMethod, isAutoEndIfNotloop, GetNextPlayUnitRequestOrder(), GetNextRequestLinkKey());

				_nPlayedUnit = _animPlayUnits.Count;
				return existPlayUnit;
			}

			apAnimPlayUnit newPlayUnit = new apAnimPlayUnit(this, GetNextPlayUnitRequestOrder(), GetNextRequestLinkKey());
			newPlayUnit.SetAnimClip(playData, _layer, blendMethod, isAutoEndIfNotloop, false);

			//if(!newPlayUnit._linkedAnimClip.IsPlaying)
			//{
			//	if(newPlayUnit.Frame != newPlayUnit.StartFrame)
			//	{
			//		Debug.Log("새로운 재생 요청 - 프레임이 초기화되지 않음 [" + newPlayUnit._linkedAnimClip._name + "]");
			//	}
			//	newPlayUnit.ResetPlay();
			//}

			//리스트에 넣자
			_animPlayUnits.Add(newPlayUnit);

			_nPlayedUnit = _animPlayUnits.Count;
			_isInitPlayUnit = false;
			return newPlayUnit;
		}

		//----------------------------------------------------
		// 재생/정지 요청 함수들
		//----------------------------------------------------

		//AnimClip을 PlayUnit에 담아서 재생한다.
		//Queue에 저장된 모든 클립은 무시되며 블렌드되지 않는다.
		public apAnimPlayUnit Play(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, float blendTime = 0.0f, bool isAutoEndIfNotloop = true)
		{
			//현재 상태에서 실행되지 않은 Queued 애니메이션 재생 요청은 삭제한다.
			PushAllNoActiveRequests();

			//Request를 생성한다.
			apAnimPlayRequest request = PopRequest();
			//request.SetCurrentPlayedUnits(this, _animPlayUnits);
			request.SetCurrentPlayedUnits(this);

			//현재 플레이 중인 AnimPlayUnit들의 LinkKey를 넣어준다.
			//Debug.Log("Add Play Unit Link Key");

			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				//Debug.Log(_animPlayUnits[i]._linkedAnimClip._name + " : " + _animPlayUnits[i].LinkKey + " / " + _animPlayUnits[i].PlayStatus);
				request.AddPrevPlayUnitKeyLink(_animPlayUnits[i].LinkKey, _animPlayUnits[i].UnitWeight);
			}

			apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);

			//newPlayUnit.Play();
			newPlayUnit.Resume();//Pause가 걸려있으면 풀어주자


			//Play 명령을 준다.
			
			request.PlayNew(newPlayUnit, blendTime);

			//이때, 만약 PlayQueued 타입이며 newPlayUnit을 타겟으로 하는게 있으면 처리할 때 무력화시켜야 한다.
			apAnimPlayRequest overlapQueuedRequest = _requests_Live.Find(delegate (apAnimPlayRequest a)
			{
				return a != request && a.RequestType == apAnimPlayRequest.REQUEST_TYPE.Queued && a._nextPlayUnit == newPlayUnit;
			});
			if(overlapQueuedRequest != null)
			{
				//Debug.Log("겹치는 Queue Request를 그냥 바로 삭제");
				PushRequest(overlapQueuedRequest);
			}

			#region [미사용 코드]
			//TODO : 이 AnimClip을 CalculatedParam에 연결해야한다.
			//Debug.LogError("TODO : 이 AnimClip을 CalculatedParam에 연결해야한다");

			////플레이 유닛은 플레이 시작
			////나머지는 End로 만든다.
			//for (int i = 0; i < _animPlayUnits.Count; i++)
			//{
			//	if (newPlayUnit != _animPlayUnits[i])
			//	{
			//		_animPlayUnits[i].SetEnd();
			//	}
			//} 
			#endregion

			_nPlayedUnit = _animPlayUnits.Count;


			//Order를 갱신한다.
			RefreshOrder();

			

			//Debug.Log("Next Play Units [" + _nPlayedUnit + "]");
			return newPlayUnit;
		}

		

		//AnimClip을 PlayUnit에 담아서 재생한다.
		//Queue에 저장된 클립들이 모두 끝나면 블렌드 없이 바로 실행된다.
		public apAnimPlayUnit PlayQueued(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, float blendTime = 0.0f, bool isAutoEndIfNotloop = true)
		{
			//현재 재생되는 플레이 유닛 중에서 "가장 많이 남은 플레이 시간"을 기준으로 타이머를 잡자
			//Fade 타임은 없고, 자동 삭제 타이머 + 자동 재생 대기 타이머를 지정

			//현재 Queue에 있는 객체가 없다면 Play와 동일하다

			//수정 : 
			//이전에는 Queue 조건을 PlayUnit이 재생되는가..로 판별했다. -> 그러면 Stop 도중의 Unit도 Queue로 인식해버림
			//이젠 이전에 존재하는 Request가 있는지 확인하고 Chain으로 연결한다.
			//Chain 연결시에는 연결된 양쪽이 서로를 알고 있어야 한다.
			//Request가 없는 경우에 한해서 PlayUnit에 연결한다.
			//마지막 연결이 
			apAnimPlayRequest lastRequest = null;

			if (_requests_Live.Count > 0)
			{
				//1. Request가 있는 경우/
				//Chain을 시도한다.
				//마지막 Request를 찾는다. (End가 아닌거면 다 됨)
				
				for (int i = _requests_Live.Count - 1; i >= 0; i--)
				{
					if(_requests_Live[i].Status != apAnimPlayRequest.STATUS.End)
					{
						lastRequest = _requests_Live[i];
						break;
					}
				}

				//아직 처리중인 마지막 Request가 있을 때
				//Request 타입과 해당 AnimPlayUnit에 따라 
				//1) Chain+Queued을 할지, 2) 그냥 Play를 할 지, 3) 처리를 포기할 지
				//결정을 한다.
				if (lastRequest != null)
				{
					int chainType = -1;
					switch (lastRequest.RequestType)
					{
						case apAnimPlayRequest.REQUEST_TYPE.New:
						case apAnimPlayRequest.REQUEST_TYPE.Queued:
							{
								//대기 중이거나 재생 중이다.
								//1. 같은 PlayUnit이라면 -> Loop 타입이라면 취소, Once 타입이라면 Chain 처리
								//2. Loop 타입이라면 -> 그냥 Play
								//3. 그외 -> Chain 처리
								if (lastRequest._nextPlayUnit._linkedAnimClip == playData._linkedAnimClip)
								{
									//1. 같은 PlayUnit이다.
									if (lastRequest._nextPlayUnit.IsLoop)
									{
										//Loop 타입이라면 취소
										chainType = QUEUE_CHAIN__3_SKIP;
									}
									else
									{
										//Once 타입이라면 Chain 처리
										//chainType = QUEUE_CHAIN__1_LINK_CHAIN;

										//변경 : 이미 같은 애니메이션에 대하여 New, Queued가 등록된 요청이다.
										//처리하지 않는걸로 하자
										chainType = QUEUE_CHAIN__3_SKIP;
									}
								}
								else if (lastRequest._nextPlayUnit.IsLoop)
								{
									//2. 마지막 PlayUnit이 Loop 타입이다.
									// -> 그냥 Play
									chainType = QUEUE_CHAIN__2_PLAY;
								}
								else
								{
									//3. 그외 -> Chain 처리
									chainType = QUEUE_CHAIN__1_LINK_CHAIN;
								}
							}
							break;

						case apAnimPlayRequest.REQUEST_TYPE.Stop:
							{
								//정지 요청이 있다.
								//Chain 없이 Play를 시도한다. (Stop 이후이므로)
								chainType = QUEUE_CHAIN__2_PLAY;
							}
							break;
					}

					switch (chainType)
					{
						case QUEUE_CHAIN__1_LINK_CHAIN:
							{
								//Last Unit과 Chain으로 연결한다.
							}
							break;

						case QUEUE_CHAIN__2_PLAY:
							//Debug.LogError("Play Queued -> 2) Play");
							return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);

						case QUEUE_CHAIN__3_SKIP:
							//Debug.LogError("Play Queued -> 3) Skip");
							return null;
					}
				}
			}

			apAnimPlayUnit lastPlayUnit = null;

			if(lastRequest != null)
			{
				//Chain 처리가 되었다면
				//-> LastPlayUnit은 Chain된 Request의 PlayUnit으로 삼는다.
				lastPlayUnit = lastRequest._nextPlayUnit;

			}
			else
			{
				//Chain 처리가 안되었을 때
				//마지막 PlayUnit을 비교하여 처리한다.
				//1. 마지막 PlayUnit이 없는 경우 -> 바로 Play
				//2. 마지막 PlayUnit이 Loop 타입이어서 기다릴 수 없는 경우 -> 바로 Play

				if (_nPlayedUnit == 0)
				{
					//Debug.LogError("현재 남은 PlayUnit이 없으므로 바로 실행 [" + playData._animClipName + "]");
					return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);
				}

				//마지막 PlayUnit을 가져오자
				lastPlayUnit = _animPlayUnits[_animPlayUnits.Count - 1];

				if (lastPlayUnit.IsLoop)
				{
					//만약 마지막 PlayUnit이 Loop라면 => Queued 되지 않는다. 자동으로 [Play]로 바뀜
					//Debug.LogError("마지막 PlayUnit [" + lastPlayUnit._linkedAnimClip._name + "] 이 Loop 타입이어서 그냥 무시하고 CrossFade로 실행");
					return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);
				}
			}
			
			if(lastPlayUnit != null && lastPlayUnit._linkedAnimClip == playData._linkedAnimClip)
			{
				//Debug.LogError("이미 재생중인 애니메이션이다.");
				return null;
			}
			

			
			//Request를 생성한다.
			apAnimPlayRequest request = PopRequest();
			//request.SetCurrentPlayedUnits(this, _animPlayUnits);
			request.SetCurrentPlayedUnits(this);
			

			//현재 플레이 중인 AnimPlayUnit들의 LinkKey를 넣어준다.
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				request.AddPrevPlayUnitKeyLink(_animPlayUnits[i].LinkKey, _animPlayUnits[i].UnitWeight);
			}
			
			apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
			
			newPlayUnit.Pause();

			
			//Play Queued 명령을 준다.
			request.PlayQueued(newPlayUnit, lastPlayUnit, blendTime);


			//추가 : Chain 처리를 해주자
			if(lastRequest != null)
			{
				//if(lastRequest._chainedRequest_Next != null)
				//{
				//	Debug.LogError("마지막 Unit을 Chain하려고 했으나 이미 연결되어 있다;;;");
				//}

				//LastRequest.Next <-> Request.Prev

				lastRequest._chainedRequest_Next = request;
				request._chainedRequest_Prev = lastRequest;
			}
			


			_nPlayedUnit = _animPlayUnits.Count;

			//Order를 갱신한다.
			RefreshOrder();
			
			

			return newPlayUnit;
		}


		//---------------------------------------------------------------------------------------------
		public apAnimPlayUnit PlayAt(apAnimPlayData playData, int frame, apAnimPlayUnit.BLEND_METHOD blendMethod, float blendTime = 0.0f, bool isAutoEndIfNotloop = true)
		{
			//현재 상태에서 실행되지 않은 Queued 애니메이션 재생 요청은 삭제한다.
			PushAllNoActiveRequests();

			//Request를 생성한다.
			apAnimPlayRequest request = PopRequest();
			request.SetCurrentPlayedUnits(this);

			//현재 플레이 중인 AnimPlayUnit들의 LinkKey를 넣어준다.
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				request.AddPrevPlayUnitKeyLink(_animPlayUnits[i].LinkKey, _animPlayUnits[i].UnitWeight);
			}

			apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);

			newPlayUnit.Resume();//Pause가 걸려있으면 풀어주자

			//Play 명령을 준다.
			request.PlayNewAt(newPlayUnit, frame, blendTime);//<<수정됨

			//이때, 만약 PlayQueued 타입이며 newPlayUnit을 타겟으로 하는게 있으면 처리할 때 무력화시켜야 한다.
			apAnimPlayRequest overlapQueuedRequest = _requests_Live.Find(delegate (apAnimPlayRequest a)
			{
				return a != request && a.RequestType == apAnimPlayRequest.REQUEST_TYPE.Queued && a._nextPlayUnit == newPlayUnit;
			});
			if(overlapQueuedRequest != null)
			{
				PushRequest(overlapQueuedRequest);
			}

			_nPlayedUnit = _animPlayUnits.Count;
			
			//Order를 갱신한다.
			RefreshOrder();

			return newPlayUnit;
		}


		public apAnimPlayUnit PlayQueuedAt(apAnimPlayData playData, int frame, apAnimPlayUnit.BLEND_METHOD blendMethod, float blendTime = 0.0f, bool isAutoEndIfNotloop = true)
		{
			apAnimPlayRequest lastRequest = null;

			if (_requests_Live.Count > 0)
			{
				//1. Request가 있는 경우/
				//Chain을 시도한다.
				//마지막 Request를 찾는다. (End가 아닌거면 다 됨)
				
				for (int i = _requests_Live.Count - 1; i >= 0; i--)
				{
					if(_requests_Live[i].Status != apAnimPlayRequest.STATUS.End)
					{
						lastRequest = _requests_Live[i];
						break;
					}
				}

				//아직 처리중인 마지막 Request가 있을 때
				//Request 타입과 해당 AnimPlayUnit에 따라 
				//1) Chain+Queued을 할지, 2) 그냥 Play를 할 지, 3) 처리를 포기할 지
				//결정을 한다.
				if (lastRequest != null)
				{
					int chainType = -1;
					switch (lastRequest.RequestType)
					{
						case apAnimPlayRequest.REQUEST_TYPE.New:
						case apAnimPlayRequest.REQUEST_TYPE.Queued:
							{
								//대기 중이거나 재생 중이다.
								//1. 같은 PlayUnit이라면 -> Loop 타입이라면 취소, Once 타입이라면 Chain 처리
								//2. Loop 타입이라면 -> 그냥 Play
								//3. 그외 -> Chain 처리
								if (lastRequest._nextPlayUnit._linkedAnimClip == playData._linkedAnimClip)
								{
									//1. 같은 PlayUnit이다.
									if (lastRequest._nextPlayUnit.IsLoop)
									{
										//Loop 타입이라면 취소
										chainType = QUEUE_CHAIN__3_SKIP;
									}
									else
									{
										//Once 타입이라면 Chain 처리
										//chainType = QUEUE_CHAIN__1_LINK_CHAIN;

										//변경 : 이미 같은 애니메이션에 대하여 New, Queued가 등록된 요청이다.
										//처리하지 않는걸로 하자
										chainType = QUEUE_CHAIN__3_SKIP;
									}
								}
								else if (lastRequest._nextPlayUnit.IsLoop)
								{
									//2. 마지막 PlayUnit이 Loop 타입이다.
									// -> 그냥 Play
									chainType = QUEUE_CHAIN__2_PLAY;
								}
								else
								{
									//3. 그외 -> Chain 처리
									chainType = QUEUE_CHAIN__1_LINK_CHAIN;
								}
							}
							break;

						case apAnimPlayRequest.REQUEST_TYPE.Stop:
							{
								//정지 요청이 있다.
								//Chain 없이 Play를 시도한다. (Stop 이후이므로)
								chainType = QUEUE_CHAIN__2_PLAY;
							}
							break;
					}

					switch (chainType)
					{
						case QUEUE_CHAIN__1_LINK_CHAIN:
							{
								//Last Unit과 Chain으로 연결한다.
							}
							break;

						case QUEUE_CHAIN__2_PLAY:
							//Debug.LogError("Play Queued -> 2) Play");
							//return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);//<<일반
							return PlayAt(playData, frame, blendMethod, blendTime, isAutoEndIfNotloop);//<<시작 프레임 제어

						case QUEUE_CHAIN__3_SKIP:
							//Debug.LogError("Play Queued -> 3) Skip");
							return null;
					}
				}
			}

			apAnimPlayUnit lastPlayUnit = null;

			if(lastRequest != null)
			{
				//Chain 처리가 되었다면
				//-> LastPlayUnit은 Chain된 Request의 PlayUnit으로 삼는다.
				lastPlayUnit = lastRequest._nextPlayUnit;

			}
			else
			{
				//Chain 처리가 안되었을 때
				//마지막 PlayUnit을 비교하여 처리한다.
				//1. 마지막 PlayUnit이 없는 경우 -> 바로 Play
				//2. 마지막 PlayUnit이 Loop 타입이어서 기다릴 수 없는 경우 -> 바로 Play

				if (_nPlayedUnit == 0)
				{
					//return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);//일반
					return PlayAt(playData, frame, blendMethod, blendTime, isAutoEndIfNotloop);//시작 프레임 제어
				}

				//마지막 PlayUnit을 가져오자
				lastPlayUnit = _animPlayUnits[_animPlayUnits.Count - 1];

				if (lastPlayUnit.IsLoop)
				{
					//만약 마지막 PlayUnit이 Loop라면 => Queued 되지 않는다. 자동으로 [Play]로 바뀜
					//return Play(playData, blendMethod, blendTime, isAutoEndIfNotloop);//일반
					return PlayAt(playData, frame, blendMethod, blendTime, isAutoEndIfNotloop);//시작 프레임 제어
				}
			}
			
			if(lastPlayUnit != null && lastPlayUnit._linkedAnimClip == playData._linkedAnimClip)
			{
				//Debug.LogError("이미 재생중인 애니메이션이다.");
				return null;
			}
			
			//Request를 생성한다.
			apAnimPlayRequest request = PopRequest();
			//request.SetCurrentPlayedUnits(this, _animPlayUnits);
			request.SetCurrentPlayedUnits(this);
			

			//현재 플레이 중인 AnimPlayUnit들의 LinkKey를 넣어준다.
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				request.AddPrevPlayUnitKeyLink(_animPlayUnits[i].LinkKey, _animPlayUnits[i].UnitWeight);
			}
			
			apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
			
			newPlayUnit.Pause();

			
			//Play Queued 명령을 준다.
			//request.PlayQueued(newPlayUnit, lastPlayUnit, blendTime);//일반
			request.PlayQueuedAt(newPlayUnit, lastPlayUnit, frame, blendTime);//시작 프레임 지정


			//추가 : Chain 처리를 해주자
			if(lastRequest != null)
			{
				lastRequest._chainedRequest_Next = request;
				request._chainedRequest_Prev = lastRequest;
			}
			
			_nPlayedUnit = _animPlayUnits.Count;

			//Order를 갱신한다.
			RefreshOrder();
			
			

			return newPlayUnit;
		}

		//---------------------------------------------------------------------------------------------


		#region [미사용 코드] CrossFade 대신 Play에서 BlendTime을 넣자
		///// <summary>
		///// AnimClip을 PlayUnit에 담아서 바로 재생한다.
		///// Queue에 저장된 모든 클립에 바로 FadeOut을 지정하여 자연스럽게 종료하도록 한다.
		///// </summary>
		///// <param name="blendMethod"></param>
		///// <param name="isAutoEndIfNotloop">True이면 Clip의 재생 후 자동으로 종료한다. (Loop일 경우 무시됨)</param>
		//public apAnimPlayUnit CrossFade(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop, float fadeTime)
		//{
		//	apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
		//	newPlayUnit.Play(fadeTime, 0.0f);

		//	//TODO : 이 AnimClip을 CalculatedParam에 연결해야한다.
		//	//Debug.LogError("TODO : 이 AnimClip을 CalculatedParam에 연결해야한다");

		//	//플레이 유닛은 플레이 시작
		//	//나머지는 End로 만든다.
		//	for (int i = 0; i < _animPlayUnits.Count; i++)
		//	{
		//		if (newPlayUnit != _animPlayUnits[i])
		//		{
		//			_animPlayUnits[i].FadeOut(fadeTime);
		//		}
		//	}

		//	_nPlayedUnit = _animPlayUnits.Count;

		//	return newPlayUnit;
		//}


		///// <summary>
		///// AnimClip을 PlayUnit에 담아서 기다린 뒤 재생한다.
		///// Queue에 저장된 클립들이 모두 끝나면 Fade Time만큼 섞어서 재생한다.
		///// </summary>
		///// <param name="animClip"></param>
		///// <param name="blendMethod"></param>
		///// <param name="isAutoEndIfNotloop">True이면 Clip의 재생 후 자동으로 종료한다. (Loop일 경우 무시됨)</param>
		///// <returns></returns>
		//public apAnimPlayUnit CrossFadeQueued(apAnimPlayData playData, apAnimPlayUnit.BLEND_METHOD blendMethod, bool isAutoEndIfNotloop, float fadeTime)
		//{
		//	//현재 재생되는 플레이 유닛 중에서 "가장 많이 남은 플레이 시간"을 기준으로 타이머를 잡자
		//	//Fade 타임은 없고, 자동 삭제 타이머 + 자동 재생 대기 타이머를 지정

		//	//현재 Queue에 있는 객체가 없다면 CrossFade와 동일하다
		//	if(_nPlayedUnit == 0)
		//	{	
		//		return CrossFade(playData, blendMethod, isAutoEndIfNotloop, fadeTime);
		//	}

		//	float maxRemainPlayTime = -1.0f;
		//	float curRemainPlayTime = 0.0f;
		//	bool isAnyOnceAnimClip = false;
		//	for (int i = 0; i < _nPlayedUnit; i++)
		//	{
		//		_tmpCurPlayUnit = _animPlayUnits[i];
		//		if(_tmpCurPlayUnit.IsLoop)
		//		{
		//			//하나라도 루프이면 실패다. > 수정
		//			//루프는 무시하고 Queue 시간을 잡자
		//			//만약 Loop를 만나고 Queue가 있다면 그냥 기본값인 0.5초를 Queue 시간으로 쓴다.
		//			//Queue에 넣어도 작동하지 않는다.
		//			//Debug.LogError("PlayQueued Failed : Any Clip has Loop Option. Adding to Queue will be ignored");
		//			//return null;
		//			continue;
		//		}

		//		isAnyOnceAnimClip = true;
		//		curRemainPlayTime = _tmpCurPlayUnit.GetRemainPlayTime;
		//		if(maxRemainPlayTime < curRemainPlayTime)
		//		{
		//			maxRemainPlayTime = curRemainPlayTime;
		//		}
		//	}

		//	if(!isAnyOnceAnimClip)
		//	{
		//		maxRemainPlayTime = 0.5f;
		//	}
		//	if(maxRemainPlayTime < 0.0f)
		//	{
		//		maxRemainPlayTime = 0.0f;
		//	}

		//	//딜레이 시간 = 최대 "남은 시간" - "페이드아웃 시간"
		//	//-----------------..............--->
		//	//[    딜레이    ] + [ 페이드아웃 ] 

		//	//Debug.Log("------------------------------------------------------------");
		//	//Debug.Log("CrossFadeQueued Request [" + playData._animClipName + "]");
		//	float delayTime = maxRemainPlayTime - fadeTime;

		//	//Debug.Log("Max Remain Time : " + maxRemainPlayTime);
		//	//Debug.Log("Fade Time : " + fadeTime);
		//	//Debug.Log("Delay Time : " + delayTime);

		//	if(delayTime < 0.0f)
		//	{
		//		// 만약 남은 시간이 적어서 Delay 시간이 음수가 된다면
		//		//Delay Time = 0으로 두고
		//		//남은 시간이 모두 FadeTime이다.
		//		fadeTime = maxRemainPlayTime;
		//		delayTime = 0.0f;

		//		//Debug.LogError("Adjusted > Fade Time : " + fadeTime + " / Delay Time : 0");
		//	}

		//	//Debug.Log("------------------------------------------------------------");

		//	//최대 RemainPlayTime 만큼 Delay한다.
		//	// Delay후 신규 플레이 또는 플레이 종료를 한다.
		//	//Fade 시간은 0

		//	apAnimPlayUnit newPlayUnit = MakePlayUnit(playData, blendMethod, isAutoEndIfNotloop);
		//	newPlayUnit.Play(fadeTime, delayTime);

		//	//Debug.LogError("TODO : 이 AnimClip을 CalculatedParam에 연결해야한다");

		//	for (int i = 0; i < _nPlayedUnit; i++)
		//	{
		//		_tmpCurPlayUnit = _animPlayUnits[i];
		//		if (newPlayUnit != _tmpCurPlayUnit)
		//		{
		//			_tmpCurPlayUnit.FadeOut(fadeTime, delayTime);
		//		}
		//	}

		//	_nPlayedUnit = _animPlayUnits.Count;

		//	return newPlayUnit;
		//}

		#endregion

		//모든 PlayUnit을 종료한다. Clear와 달리 blendTime을 지원한다.
		//이 프레임에서 바로 종료하는게 아니므로, 만약 바로 정리를 하고자 한다면 ReleaseForce를 호출하자
		public void StopAll(float blendTime)
		{
			//현재 상태에서 실행되지 않은 Queued 애니메이션 재생 요청은 삭제한다.
			PushAllNoActiveRequests();
			
			//Stop을 하면서 서서히 줄어드는 걸 요청한다.
			apAnimPlayRequest request = PopRequest();
			//request.SetCurrentPlayedUnits(this, _animPlayUnits);
			request.SetCurrentPlayedUnits(this);
			

			//현재 플레이 중인 AnimPlayUnit들의 LinkKey를 넣어준다.
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				request.AddPrevPlayUnitKeyLink(_animPlayUnits[i].LinkKey, _animPlayUnits[i].UnitWeight);

				if (blendTime < 0.0001f)
				{
					if (_animPlayUnits[i]._linkedAnimClip != null)
					{
						if (!_animPlayUnits[i]._linkedAnimClip.IsPlaying_Opt)
						{
							_animPlayUnits[i]._linkedAnimClip.ResetFrame();
						}
					}
				}
			}

			request.Stop(blendTime);

			


			//Order를 갱신한다.
			RefreshOrder();

			
			
		}

		//각 AnimClip을 강제로 Stop시킴과 동시에 Calculated와의 연동을 바로 끊어버린다.
		//StopAll과 유사하지만 연동을 바로 끊는 점에서 강제력이 있고 업데이트시 처리에 문제가 있을 수 있음
		public void ReleaseForce()
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_animPlayUnits[i].ReleaseLink();
			}

			_animPlayUnits.Clear();
			_nPlayedUnit = _animPlayUnits.Count;
			_isInitPlayUnit = false;

			//Order를 갱신한다.
			RefreshOrder();
		}


		//AnimClip중에 요청된 RootUnit에 대한 것이 아니면 강제로 종료한다.
		public void StopWithInvalidRootUnit(apOptRootUnit usingRootUnit)
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_tmpCurPlayUnit = _animPlayUnits[i];
				
				if (_tmpCurPlayUnit._targetRootUnit != usingRootUnit)
				{
					//단, Queued 타입은 제외해야한다. [22.5.18 : v1.4.0]
					if (_tmpCurPlayUnit.NextOwnerRequest != null)
					{
						if (_tmpCurPlayUnit.NextOwnerRequest.RequestType == apAnimPlayRequest.REQUEST_TYPE.Queued)
						{
							//Debug.Log("Queued 타입은 Root Unit이 달라도 대기한다. [" + _tmpCurPlayUnit._linkedAnimClip._name + "]");
							continue;
						}
					}
					_tmpCurPlayUnit.SetEnd(true);//true : 바로 여기서 스테이트 전환
				}
			}
		}

		//추가 21.4.3 : Portrait Hide시 강제로 모든 PlayUnit을 1프레임안에 종료시켜야한다.
		public void SetAllPlayUnitEnd()
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_animPlayUnits[i].SetEnd(true);
			}
		}


		public void Pause()
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_tmpCurPlayUnit = _animPlayUnits[i];
				_tmpCurPlayUnit.Pause();
			}
		}

		public void Resume()
		{
			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_tmpCurPlayUnit = _animPlayUnits[i];
				_tmpCurPlayUnit.Resume();
			}
		}

		// Sort / SetOrder
		//----------------------------------------------
		//Request와 PlayUnit의 Order를 갱신한다.
		//Request와 PlayUnit의 생성/삭제시 한번씩 호출한다.
		private void RefreshOrder()
		{
			int minOrder = -1;
			//가장 작은 Order가 0이 되도록 해야한다.
			//일단 가장 작은 Order를 찾는다.
			//(순서대로 Sort가 되지 않을 수 있다. 주의할 것)
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{	
				if(_animPlayUnits[i]._requestedOrder < minOrder || minOrder < 0)
				{
					minOrder = _animPlayUnits[i]._requestedOrder;
				}
			}
			//minOrder -> 0으로 Order를 모두 변경해야한다.
			if (minOrder != 0)
			{
				int orderOffset = (0 - minOrder);

				for (int i = 0; i < _animPlayUnits.Count; i++)
				{
					_animPlayUnits[i]._requestedOrder += orderOffset;
				}
			}

			for (int i = 0; i < _requests_Live.Count; i++)
			{
				_requests_Live[i]._order = i;
			}
		}

		public void RefreshPlayOrderAll()
		{
			_playManager.RefreshPlayOrders();
		}

		//PlayUnit을 만들때 RequestOrder를 만들어주자
		private int GetNextPlayUnitRequestOrder()
		{
			int maxOrder = -1;
			for (int i = 0; i < _animPlayUnits.Count; i++)
			{
				if(_animPlayUnits[i]._requestedOrder > maxOrder)
				{
					maxOrder = _animPlayUnits[i]._requestedOrder;
				}
			}
			if(maxOrder < 0)
			{
				return 0;
			}
			else
			{
				return maxOrder + 1;
			}
		}


		public apAnimPlayUnit GetPlayUnitByLinkKey(int linkKey)
		{
			if(_animPlayUnits.Count == 0)
			{
				return null;
			}
			return _animPlayUnits.Find(delegate (apAnimPlayUnit a)
			{
				return a.LinkKey == linkKey;
			});
		}

		// Update
		//----------------------------------------------
		public bool _isUpdated = false;
		public void Update(float tDelta)
		{
			//현재 재생중인 유닛이 있다면 시작
			_isUpdated = false;
			if (_nPlayedUnit > 0)
			{
				//업데이트..
				//Debug.Log("Update Queue");
				for (int i = 0; i < _nPlayedUnit; i++)
				{
					_tmpCurPlayUnit = _animPlayUnits[i];
					_tmpCurPlayUnit.SetWeight(0.0f, false);//<<일단 Weight를 0으로 둔다.
					_tmpCurPlayUnit.Update(tDelta);

					_isUpdated = true;
					if (_tmpCurPlayUnit.IsRemovable)
					{
						//TODO : 이 객체와 연결된 CalculatedParam에 AnimClip이 사라졌음을 알려야한다.
						//Debug.LogError("TODO : 이 객체와 연결된 CalculatedParam에 AnimClip이 사라졌음을 알려야한다");
						_tmpCurPlayUnit.SetWeight(0.0f, true);
						_isAnyUnitChanged = true;
					}
				}

				_isInitPlayUnit = false;
			}
			else
			{
				//만약 _nPlayedUnit = 0인 상태로 한번도 초기화를 안했다면..
				if(!_isInitPlayUnit)
				{
					_isInitPlayUnit = true;
				}
			}

			apAnimPlayRequest curRequest = null;
			

			if (_requests_Live.Count > 0)
			{
				//Debug.Log("----------------- Request Update[" + _requests_Live.Count + "] ---------------------");
				//Request를 업데이트하고
				//각 Request별로 연관된 PlayUnit의 Weight를 지정해주자
				for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
				{
					curRequest = _requests_Live[iCur];
					if (curRequest.IsEnded)
					{
						continue;
					}
					curRequest.Update(tDelta, iCur);
				}
				//Debug.Log("------------------------------------------------------");
			}



			//이제 Weight를 지정해준다.
			//1. Request를 자체적으로 업데이트하여 UnitWeight를 계산한다.

			//업데이트 방식
			//- 앞에서부터 설정한다.
			//- 일단 Weight를 1로 설정
			//- 이전 Prev 시간 영역 (-Start ~ +End)을 비교하여 겹치는 시간이 BlendTime보다 긴지 짧은지 판별한다.
			//- 겹친 시간계산하고, 현재의 ITP를 구한다.
			//- 현재 Request에 ITP를 곱하고, 이전 "모든 Weight"에 (1-ITP)를 곱한다.
			// 겹친 시간 : [ tStart <- tCur ] + [ tCur -> tEnd ]

			//2. 현재 시점에서 중복된 Request들 간의 RequestWeight를 계산한다.
			//3. Request를 돌면서 Prev/Next에 대해서 Weight를 지정해준다.


			float prevCurrent2EndTime = -1.0f;
			float tmpOverlapTime = 0.0f;
			float tmpOverlapITP = 0.0f;

			bool isAnyRequestChanged = false;

			for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
			{
				curRequest = _requests_Live[iCur];
				//Ready => Weight : 1
				//End => Weight : 0
				if (!curRequest.IsLive)
				{

					if (curRequest.IsEnded)
					{
						curRequest.SetRequestWeight(0.0f);
						isAnyRequestChanged = true;
					}
					else
					{
						curRequest.SetRequestWeight(1.0f);
					}
					continue;
				}

				curRequest.SetRequestWeight(1.0f);//일단 1을 넣는다.

				if (iCur == 0)
				{
					prevCurrent2EndTime = curRequest.Current2EndTime;
					continue;
				}

				//BlendTime보다 짧다면 Overlap 시간이 짧아진다.
				//CurTime을 기준으로 [tStart <- tCur] 시간과 [tCur -> tEnd] 시간을 나누어 더하여 계산하는데,
				//[tCur -> tEnd] 시간은 이전 Request와 길이를 비교한다.
				tmpOverlapTime = curRequest.Current2StartTime + Mathf.Min(prevCurrent2EndTime, curRequest.Current2EndTime);
				if (tmpOverlapTime < 0.001f)
				{
					tmpOverlapITP = 1.0f;
				}
				else
				{
					//기존 : 선형
					//tmpOverlapITP = curRequest.Current2StartTime / tmpOverlapTime;

					//변경 20.4.18 : SmoothStep을 이용해서 부드럽게
					tmpOverlapITP = Mathf.SmoothStep(0.0f, 1.0f, curRequest.Current2StartTime / tmpOverlapTime);
				}

				for (int iPrev = 0; iPrev < iCur; iPrev++)
				{
					_requests_Live[iPrev].MultiplyRequestWeight(1.0f - tmpOverlapITP);
				}
				curRequest.MultiplyRequestWeight(tmpOverlapITP);


				prevCurrent2EndTime = curRequest.Current2EndTime;
			}


			//마지막으로 다시 돌면서 Request에서 계산된 UnitWeight * RequestWeight를 넣어서 완성
			float totalRequestWeight = 0.0f;
			int nLiveRequest = 0;
			for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
			{
				curRequest = _requests_Live[iCur];
				//curRequest.AdaptWeightToPlayUnits();

				if (!curRequest.IsLive)
				{
					continue;
				}

				curRequest.AdaptWeightToPlayUnits();
				totalRequestWeight += curRequest.RequestWeight;
				nLiveRequest++;
			}

			if (nLiveRequest > 0 && totalRequestWeight > 0.0f)
			{
				if (totalRequestWeight > 1.0f)
				{
					totalRequestWeight = 1.0f;
				}

				//추가
				//전체 PlayUnit의 Weight가 totalRequestWeight 또는 1을 넘어가거나 모자르다면 거기에 맞게 Normalize한다.
				float totalPlayUnitWeight = 0.0f;
				int nPlayUnit = 0;
				for (int iPlayUnit = 0; iPlayUnit < _animPlayUnits.Count; iPlayUnit++)
				{
					totalPlayUnitWeight += _animPlayUnits[iPlayUnit].UnitWeight;
					if (totalPlayUnitWeight > 0.0f)
					{
						nPlayUnit++;
					}
				}
				if (totalPlayUnitWeight > 0.0f && nPlayUnit > 1)
				{
					float normalizeWeight = totalRequestWeight / totalPlayUnitWeight;

					for (int iPlayUnit = 0; iPlayUnit < _animPlayUnits.Count; iPlayUnit++)
					{
						_animPlayUnits[iPlayUnit].NormalizeWeight(normalizeWeight);
					}
				}
			}

			//변경 20.2.24 : Control Param 업데이트를 여기서 일괄적으로 해야한다.
			for (int iPlayUnit = 0; iPlayUnit < _animPlayUnits.Count; iPlayUnit++)
			{
				_animPlayUnits[iPlayUnit].UpdateControlParamOpt();
			}

			bool isOrderRefreshable = _isAnyUnitChanged || isAnyRequestChanged;

			//변화값이 있으면 삭제 여부를 판단하자
			if (_isAnyUnitChanged)
			{
				_animPlayUnits.RemoveAll(delegate (apAnimPlayUnit a)
				{
					return a.IsRemovable;
				});

				_isAnyUnitChanged = false;
				_nPlayedUnit = _animPlayUnits.Count;


				_playManager.OnAnyAnimPlayUnitEnded();
			}


			if (isAnyRequestChanged)
			{
				//끝난 Request를 Pool에 돌려놓는다.
				List<apAnimPlayRequest> endedRequests = new List<apAnimPlayRequest>();
				for (int i = 0; i < _requests_Live.Count; i++)
				{
					if (_requests_Live[i].IsEnded)
					{
						endedRequests.Add(_requests_Live[i]);
					}
				}

				for (int i = 0; i < endedRequests.Count; i++)
				{
					PushRequest(endedRequests[i]);
				}
			}


			if(isOrderRefreshable)
			{
				//Order를 갱신한다.
				RefreshOrder();
			}
		}








		//---------------------------------------------------
		// 인스펙터 미리보기용 업데이트 함수
		public void Update_InspectorPreview()
		{
			//현재 재생중인 유닛이 있다면 시작
			_isUpdated = false;
			if (_nPlayedUnit > 0)
			{
				//업데이트..
				//Debug.Log("Update Queue");
				for (int i = 0; i < _nPlayedUnit; i++)
				{
					_tmpCurPlayUnit = _animPlayUnits[i];
					_tmpCurPlayUnit.SetWeight(0.0f, false);//<<일단 Weight를 0으로 둔다.
					_tmpCurPlayUnit.Update_InspectorPreview();//이게 인스펙터용으로 바뀜 (애니메이션 이벤트 발생 안함)

					_isUpdated = true;
					if (_tmpCurPlayUnit.IsRemovable)
					{
						_tmpCurPlayUnit.SetWeight(0.0f, true);
						_isAnyUnitChanged = true;
					}
				}

				_isInitPlayUnit = false;
			}
			else
			{
				//만약 _nPlayedUnit = 0인 상태로 한번도 초기화를 안했다면..
				if(!_isInitPlayUnit)
				{
					_isInitPlayUnit = true;
				}
			}

			apAnimPlayRequest curRequest = null;
			

			if (_requests_Live.Count > 0)
			{
				for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
				{
					curRequest = _requests_Live[iCur];
					if (curRequest.IsEnded)
					{
						continue;
					}
					curRequest.Update(0.0f, iCur);
				}
			}

			float prevCurrent2EndTime = -1.0f;
			float tmpOverlapTime = 0.0f;
			float tmpOverlapITP = 0.0f;

			bool isAnyRequestChanged = false;

			for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
			{
				curRequest = _requests_Live[iCur];
				//Ready => Weight : 1
				//End => Weight : 0
				if (!curRequest.IsLive)
				{

					if (curRequest.IsEnded)
					{
						curRequest.SetRequestWeight(0.0f);
						isAnyRequestChanged = true;
					}
					else
					{
						curRequest.SetRequestWeight(1.0f);
					}
					continue;
				}

				curRequest.SetRequestWeight(1.0f);//일단 1을 넣는다.

				if (iCur == 0)
				{
					prevCurrent2EndTime = curRequest.Current2EndTime;
					continue;
				}

				//BlendTime보다 짧다면 Overlap 시간이 짧아진다.
				//CurTime을 기준으로 [tStart <- tCur] 시간과 [tCur -> tEnd] 시간을 나누어 더하여 계산하는데,
				//[tCur -> tEnd] 시간은 이전 Request와 길이를 비교한다.
				tmpOverlapTime = curRequest.Current2StartTime + Mathf.Min(prevCurrent2EndTime, curRequest.Current2EndTime);
				if (tmpOverlapTime < 0.001f)
				{
					tmpOverlapITP = 1.0f;
				}
				else
				{
					//기존 : 선형
					//tmpOverlapITP = curRequest.Current2StartTime / tmpOverlapTime;

					//변경 20.4.18 : SmoothStep을 이용해서 부드럽게
					tmpOverlapITP = Mathf.SmoothStep(0.0f, 1.0f, curRequest.Current2StartTime / tmpOverlapTime);
				}

				for (int iPrev = 0; iPrev < iCur; iPrev++)
				{
					_requests_Live[iPrev].MultiplyRequestWeight(1.0f - tmpOverlapITP);
				}
				curRequest.MultiplyRequestWeight(tmpOverlapITP);


				prevCurrent2EndTime = curRequest.Current2EndTime;
			}


			//마지막으로 다시 돌면서 Request에서 계산된 UnitWeight * RequestWeight를 넣어서 완성
			float totalRequestWeight = 0.0f;
			int nLiveRequest = 0;
			for (int iCur = 0; iCur < _requests_Live.Count; iCur++)
			{
				curRequest = _requests_Live[iCur];
				//curRequest.AdaptWeightToPlayUnits();

				if (!curRequest.IsLive)
				{
					continue;
				}

				curRequest.AdaptWeightToPlayUnits();
				totalRequestWeight += curRequest.RequestWeight;
				nLiveRequest++;
			}

			if (nLiveRequest > 0 && totalRequestWeight > 0.0f)
			{
				if (totalRequestWeight > 1.0f)
				{
					totalRequestWeight = 1.0f;
				}

				//추가
				//전체 PlayUnit의 Weight가 totalRequestWeight 또는 1을 넘어가거나 모자르다면 거기에 맞게 Normalize한다.
				float totalPlayUnitWeight = 0.0f;
				int nPlayUnit = 0;
				for (int iPlayUnit = 0; iPlayUnit < _animPlayUnits.Count; iPlayUnit++)
				{
					totalPlayUnitWeight += _animPlayUnits[iPlayUnit].UnitWeight;
					if (totalPlayUnitWeight > 0.0f)
					{
						nPlayUnit++;
					}
				}
				if (totalPlayUnitWeight > 0.0f && nPlayUnit > 1)
				{
					float normalizeWeight = totalRequestWeight / totalPlayUnitWeight;

					for (int iPlayUnit = 0; iPlayUnit < _animPlayUnits.Count; iPlayUnit++)
					{
						_animPlayUnits[iPlayUnit].NormalizeWeight(normalizeWeight);
					}
				}
			}

			//변경 20.2.24 : Control Param 업데이트를 여기서 일괄적으로 해야한다.
			for (int iPlayUnit = 0; iPlayUnit < _animPlayUnits.Count; iPlayUnit++)
			{
				_animPlayUnits[iPlayUnit].UpdateControlParamOpt();
			}

			bool isOrderRefreshable = _isAnyUnitChanged || isAnyRequestChanged;

			//변화값이 있으면 삭제 여부를 판단하자
			if (_isAnyUnitChanged)
			{
				_animPlayUnits.RemoveAll(delegate (apAnimPlayUnit a)
				{
					return a.IsRemovable;
				});

				_isAnyUnitChanged = false;
				_nPlayedUnit = _animPlayUnits.Count;


				_playManager.OnAnyAnimPlayUnitEnded();
			}


			if (isAnyRequestChanged)
			{
				//끝난 Request를 Pool에 돌려놓는다.
				List<apAnimPlayRequest> endedRequests = new List<apAnimPlayRequest>();
				for (int i = 0; i < _requests_Live.Count; i++)
				{
					if (_requests_Live[i].IsEnded)
					{
						endedRequests.Add(_requests_Live[i]);
					}
				}

				for (int i = 0; i < endedRequests.Count; i++)
				{
					PushRequest(endedRequests[i]);
				}
			}


			if(isOrderRefreshable)
			{
				//Order를 갱신한다.
				RefreshOrder();
			}
		}









		//---------------------------------------------------

		public int RefreshPlayOrders(int startOrder)
		{
			//이게 호출되면 Request 순으로 갱신해야한다.
			_animPlayUnits.Sort(delegate (apAnimPlayUnit a, apAnimPlayUnit b)
			{
				return a._requestedOrder - b._requestedOrder;
			});

			for (int i = 0; i < _nPlayedUnit; i++)
			{
				_animPlayUnits[i].SetPlayOrder(startOrder);
				startOrder++;
			}
			return startOrder;
		}


		//----------------------------------------------------------------------------------

		//Queue에서 재생하고자 하는 AnimPlayUnit이 실행 중인지 확인하는 함수.
		public bool IsAlreadyAnimUnitPlayed(apAnimPlayUnit playUnit)
		{
			//Debug.LogError("TODO : IsArleadyAnimUnitPlayed");
			if(playUnit == null)
			{
				return false;
			}

			if(_animPlayUnits.Contains(playUnit))
			{
				if(playUnit.PlayStatus == apAnimPlayUnit.PLAY_STATUS.Play && playUnit.UnitWeight > 0.001f)
				{
					return true;
				}
			}

			return false;
		}

		// Request -> Queue 요청
		//Request의 애니메이션의 Fade가 끝나면서 이전에 플레이되던 AnimPlayUnit을 종료하는 함수
		//AnimPlayRequest가 호출한다.
		//직접 SetEnd하는 것이 아니라 Queue에서 확인하여 처리한다.
		public void SetEndByRequest(apAnimPlayRequest request)
		{
			//Debug.Log("----- Check Set End -------------");
			bool isStopRequest = request.RequestType == apAnimPlayRequest.REQUEST_TYPE.Stop;
			bool isLastRequest = false;
			if (_requests_Live.Count > 0)
			{
				int iLastLiveRequest = -1;
				//Live 중의 마지막 Request를 찾는다.
				//대기중인 것은 의미 없음
				for (int i = _requests_Live.Count - 1; i >= 0; i--)
				{
					if (_requests_Live[i].IsLive || _requests_Live[i].IsEnded)
					{
						iLastLiveRequest = i;
						break;
					}
				}

				//Debug.Log("iLastRequest : " + iLastLiveRequest);
				if (iLastLiveRequest >= 0)
				{
					isLastRequest = (_requests_Live[iLastLiveRequest] == request);
				}
				
			}

			//Debug.Log("Is Stop Request [" + isStopRequest + "] / Is Last Request [" + isLastRequest + "]");

			//TODO
			//이 상태에서
			//A -> Play B -> PlayQueued A -> A를 바로 호출시
			//[A -> Play B]를 진행하면서 A -> 0 -> End/Ready로 만들고 싶지만
			//[PlayQueued A]를 호출하면서 A의 LinkKey가 바뀌면서 [Play B]에서 Weight를 조절할 수 없게 되었다...
			

			//if (isStopRequest || isLastRequest)
			//그냥 해볼까
			{
				//Debug.LogError("!!!!! Set End !!!!!");
				//이전의 모든 Request를 모두 End한다.
				apAnimPlayUnit playUnit = null;
				for (int i = 0; i < request._prevPlayUnitKeys.Count; i++)
				{
					playUnit = GetPlayUnitByLinkKey(request._prevPlayUnitKeys[i]);
					if (playUnit != null)
					{
						//Debug.Log("Set End - " + playUnit._linkedAnimClip._name + " / " + playUnit.LinkKey);
						if (isStopRequest || isLastRequest)
						{
							playUnit.SetEnd();
						}
						else
						{
							playUnit.SetWeightZero();
						}
					}
					//else
					//{
					//	Debug.LogError("Is Null Unit [" + request._prevPlayUnitKeys[i] + "]");
					//}
				}
			}

			//추가
			//만약 Chained 된 상태이고
			//Next Chained된 Request가 아직 Ready라면
			//SetWeightZero를 해야한다.
			//이건 반복해서 처리한다.
			//AdaptWeight도 마찬가지로 처리
			SetWeightZeroToNextChained(request);


			//Debug.Break();

		}

		//SetEndByRequest 함수 호출시, Prev Anim PlayUnit은 아니지만, Chained가 되어서
		//다음에 연결되어 대기중인 Anim PlayUnit의 Weight를 0으로 하고 대기시켜야 하는 경우 호출하는 함수.
		//Chained가 이어져 있다면 계속 호출한다.
		private void SetWeightZeroToNextChained(apAnimPlayRequest request)
		{
			if(request._chainedRequest_Next == null)
			{
				return;
			}
			if(!request._chainedRequest_Next.IsLive)
			{
				if(request._chainedRequest_Next._nextPlayUnit != null)
				{
					request._chainedRequest_Next._nextPlayUnit.SetWeightZero();
				}
			}

			//이어서 계속 호출한다.
			SetWeightZeroToNextChained(request._chainedRequest_Next);
		}

		//Request의 AdaptWeightToPlayUnits에서 호출하는 함수
		public void SetRequestWeightToPlayUnits(apAnimPlayRequest request, float prevUnitWeight, float nextUnitWeight, float requestWeight)
		{
			//TODO. AdaptWeightToPlayUnits 함수의 내용을 적자
			//단, Prev 지점 확실히 해야함
			//Debug.LogError("TODO : SetRequestWeightToPlayUnits");

			if(request._nextPlayUnit != null)
			{
				//Weight가 적용되는 방식 1) To Next
				request._nextPlayUnit.AddWeight(nextUnitWeight, requestWeight);
			}

			//이전 유닛에 대해서 Weight 지정
			apAnimPlayUnit playUnit = null;

			

			//테스트 코드 : 뒤에서부터 체크해서 "유효한" 한개의 PrevUnit만 Weight를 적용
			if (request._prevPlayUnitKeys.Count > 0)
			{
				for (int i = request._prevPlayUnitKeys.Count - 1; i >= 0; i--)
				{
					playUnit = GetPlayUnitByLinkKey(request._prevPlayUnitKeys[i]);
					//prevUnitWeight 총 합이 1이 되도록 분할한다.
					if (playUnit != null)
					{
						//Weight가 적용되는 방식 2) To Prev
						playUnit.AddWeight(prevUnitWeight, requestWeight);
						break;
					}
				}
			}



			//Prev에 있었지만 다음 Request로 인해 연결이 끊겨버린 
			//Next Chained에 해당하는 AnimPlayUnit에 대해서도 Weight를 지정해주자
			SetRequestWeightToNextChained(request, prevUnitWeight, requestWeight);
		}

		//SetRequestWeightToPlayUnits 함수 호출시, Prev Anim PlayUnit은 아니지만, Chained가 되어서
		//다음에 연결되어 대기중인 Anim PlayUnit의 Weight를 지정하고자 하는 경우.
		//Weight는 PrevWeight를 적용한다.
		//Chained가 이어져 있다면 계속 호출한다.
		private void SetRequestWeightToNextChained(apAnimPlayRequest request, float prevUnitWeight, float requestWeight)
		{
			if(request._chainedRequest_Next == null)
			{
				return;
			}
			if(!request._chainedRequest_Next.IsLive)
			{
				if(request._chainedRequest_Next._nextPlayUnit != null)
				{
					//Weight가 적용되는 방식 3) To Next chaind
					request._chainedRequest_Next._nextPlayUnit.AddWeight(prevUnitWeight, requestWeight);
				}
			}

			//이어서 계속 호출한다.
			SetRequestWeightToNextChained(request._chainedRequest_Next, prevUnitWeight, requestWeight);
		}


		// Event
		//-----------------------------------------------------------------
		public void OnAnimPlayUnitPlayStart(apAnimPlayUnit playUnit)
		{
			//Play Unit이 재생을 시작했다.
			//Delay 이후에 업데이트되는 첫 프레임에 이 이벤트가 호출된다.

			// > Root Unit이 바뀔 수 있으므로 Play Manager에도 신고를 해야한다.
			_playManager.OnAnimPlayUnitPlayStart(playUnit, this);
		}


		public void OnAnimPlayUnitEnded(apAnimPlayUnit playUnit)
		{
			_playManager.OnAnimPlayUnitEnded(playUnit, this);
		}

		// Get / Set
		//-----------------------------------------------------------------
		public apAnimPlayUnit GetPlayUnit(string animClipName)
		{
			return _animPlayUnits.Find(delegate (apAnimPlayUnit a)
			{
				return (a._linkedAnimClip != null) && string.Equals(a._linkedAnimClip._name, animClipName);
			});
		}

		public apAnimPlayUnit GetPlayUnit(apAnimClip animClip)
		{
			return _animPlayUnits.Find(delegate (apAnimPlayUnit a)
			{
				return (a._linkedAnimClip == animClip);
			});
		}


		//디버그용
		public List<apAnimPlayUnit> PlayUnitList
		{
			get
			{
				return _animPlayUnits;
			}
		}
	}
}