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
	/// Play / PlayQueued 요청이 들어왔을때, 그 시간을 기록하여 Weight를 계산하거나 재생을 제어하는 역할을 하는 클래스
	/// Layer마다 지정되므로, 각각의 Layer에 대한 요청이 들어온 경우 이 객체를 여러개 생성한다.
	/// Layer별로 가장 먼저 요청된 Request부터 Weight를 계산한다.
	/// 이 데이터는 AnimPlayQueue에 포함된다.
	/// 만약 대기중인 종료 시점보다 먼저 종료되는게 뒤로 온 경우, 어색하지 않게 Weight를 급격히 낮추는 것도 중요.
	/// </summary>
	public class apAnimPlayRequest
	{
		// Members
		//--------------------------------------------------
		private apAnimPlayQueue _parentAnimPlayQueue = null;


		public int _order = -1;
		

		//시간 계산법. 호출한 시간으로부터 타이머가 작동을 한다. (절대 시간을 알 수 없으므로)
		//BlendTime이 있는 경우, NextPlayStart 타임이 바뀌는데
		//New 타입인 경우 : 
		//private float _tNextPlayStart = 0.0f;//다음에 플레이가 시작하는 시점 (Blend가 끝나는 시점이다.)
		private float _tBlend = 0.0f;//시간 간격
		private float _tLive = 0.0f;
		private bool _isNextUnitPlayed = false;
		private bool _isNextUnitFrameReset = false;

		private float _tActiveStart = 0.0f;//생성 시간을 0으로 둘때의 Active 시작 시점
		private float _tActiveEnd = 0.0f;//생성 시간을 0으로 둘때의 Active 끝 시점
		private float _tActiveMiddle = 0.0f;//생성 시간을 0으로 둘때의 Active의 중간 시점

		public enum STATUS
		{
			/// <summary>
			/// Queued 타입의 경우, 바로 Active하지 못하고 시작 시점을 기다려야한다.
			/// </summary>
			Ready,
			Active,
			End
		}
		private STATUS _status = STATUS.Ready;
		public STATUS Status {  get { return _status; } }
		public bool IsCancellable { get { return (_requestType == REQUEST_TYPE.Queued && _status != STATUS.Active); } }

		public enum REQUEST_TYPE
		{
			New,
			Queued,//<<시간으로 재는 것이 아니라, 현재 대기중인 PlayData의 재생 시간을 보고 결정한다.
			Stop,//New와 비슷하게 처리를 하지만, 다음에 재생되는 Unit은 없다.
		}
		private REQUEST_TYPE _requestType = REQUEST_TYPE.New;

		//추가 1.15 : 시작 프레임이 아닌 특정 프레임에서 재생되게 만들 경우
		private bool _isResetPlayAtStartFrame = true;//<<기본적으로는 재생을 시작할 때 ResetFrame을 한다. (True가 기본값)
		private int _frameToStart = -1;

		//다음에 플레이하게될 PlayUnit (필수)
		public apAnimPlayUnit _nextPlayUnit = null;

		//Queue에 들어간 PlayData 중 마지막 데이터.
		//만약 requestType이 Queued라면, Queue 상태로 저장된다. 
		private apAnimPlayUnit _prevWaitingPlayUnit = null;

		//요청이 들어왔을때, 그 직전에 Queue에 존재했던 Unit들..
		//TODO : 이걸 삭제하고, ParentQueue에 SetEnd 요청을 직접 하자.
		//AnimPlayUnit 뿐만 아니라 
		//private List<apAnimPlayUnit> _prevPlayUnits = new List<apAnimPlayUnit>();
		//AnimPlayUnit을 저장하면 갱신된 링크 정보를 알 수 없으니 LinkKey를 이용하자
		public List<int> _prevPlayUnitKeys = new List<int>();
		public List<float> _prevPlayUnitWeight = new List<float>();//<<추가 : Prev 등록 당시의 UnitWeight를 받는다.

		//Request 자체에 대한 Weight.
		//Request끼리 중첩되는 경우 (PlayUnit이 중첩되는게 아니라..)
		//Request간의 보간도 있어야 한다.
		//이때 Weight 주도권은 나중에 선언된 Request에 있다.
		private float _requestWeight = 1.0f;
		private float _nextUnitWeight = 1.0f;
		private float _prevUnitWeight = 1.0f;
		private float _nextUnitWeight_Overlap = 0.0f;


		/// <summary>새로 재생할 PlayUnit이 PrevUnit에 포함되어있는가? (포함된다면 해당 Unit의 Blend는 다르게 제어된다.</summary>
		private bool _isNextPlayUnitIsInPrevUnit = false;


		

		private const float BIAS_ZERO = 0.001f;


		//추가 : Queue에 의한 Chain 처리
		public apAnimPlayRequest _chainedRequest_Prev = null;
		public apAnimPlayRequest _chainedRequest_Next = null;


		//Pool 관련
		//-----------------------------------------------------------------



		// Init
		//--------------------------------------------------
		public apAnimPlayRequest()
		{
			Clear();
		}

		public void Clear()
		{
			_parentAnimPlayQueue = null;

			//_tNextPlayStart = 0.0f;
			_tBlend = 0.0f;
			_tLive = 0.0f;
			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.New;

			_nextPlayUnit = null;
			_prevWaitingPlayUnit = null;
			//_prevPlayUnits.Clear();
			_prevPlayUnitKeys.Clear();
			_prevPlayUnitWeight.Clear();

			_requestWeight = 1.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = 0.0f;
			_tActiveEnd = 0.0f;
			_tActiveMiddle = 0.0f;
			_status = STATUS.Ready;

			_isNextPlayUnitIsInPrevUnit = false;

			//추가 1.15 사용자가 지정한 시작 프레임을 쓸 것인지 여부 (True면 StartFrame으로 강제됨)
			_isResetPlayAtStartFrame = true;
			_frameToStart = -1;

			ReleaseChainedRequest();

		}


		public void ReleaseChainedRequest()
		{
			if(_chainedRequest_Prev != null)
			{
				if(_chainedRequest_Prev._chainedRequest_Next == this)
				{
					_chainedRequest_Prev._chainedRequest_Next = null;//<<서로 끊어주자
				}
			}
			_chainedRequest_Prev = null;

			if(_chainedRequest_Next != null)
			{
				if(_chainedRequest_Next._chainedRequest_Prev == this)
				{
					_chainedRequest_Next._chainedRequest_Prev = null;//<<서로 끊어주자
				}
			}

			_chainedRequest_Next = null;
		}


		//public void SetCurrentPlayedUnits(apAnimPlayQueue parentAnimPlayQueue, List<apAnimPlayUnit> prevPlayUnits)
		public void SetCurrentPlayedUnits(apAnimPlayQueue parentAnimPlayQueue)
		{
			_parentAnimPlayQueue = parentAnimPlayQueue;

			////리퀘스트가 들어왔는데, 대기->시작 단계를 거치기 전에 다른 Request가 들어와서 점유해버리면 재생이 되지 않는다.
			//_prevPlayUnits.Clear();

			//apAnimPlayUnit curUnit = null;


			//for (int i = 0; i < prevPlayUnits.Count; i++)
			//{
			//	curUnit = prevPlayUnits[i];
			//	curUnit.SetOwnerRequest_Prev(this);
			//		_prevPlayUnits.Add(curUnit);
			//}
			_prevPlayUnitKeys.Clear();
			_prevPlayUnitWeight.Clear();

		}
		public void AddPrevPlayUnitKeyLink(int prevKeyLink, float unitWeight)
		{
			_prevPlayUnitKeys.Add(prevKeyLink);
			_prevPlayUnitWeight.Add(unitWeight);
			
		}

		public void PlayNew(apAnimPlayUnit nextPlayUnit, float tBlend)
		{
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.New;
			_nextPlayUnit = nextPlayUnit;
			_nextPlayUnit.SetOwnerRequest_Next(this);

			_prevWaitingPlayUnit = null;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = 0.0f;
			_tActiveEnd = _tBlend;
			_tActiveMiddle = _tBlend * 0.5f;

			_status = STATUS.Active;//<<바로 시작

			_isNextPlayUnitIsInPrevUnit = false;

			_isNextPlayUnitIsInPrevUnit = _parentAnimPlayQueue.IsAlreadyAnimUnitPlayed(_nextPlayUnit);

			_nextUnitWeight_Overlap = 1.0f;

			//추가 1.15 사용자가 지정한 시작 프레임 > Off
			_isResetPlayAtStartFrame = true;
			_frameToStart = -1;

			
			if (_isNextPlayUnitIsInPrevUnit)
			{
				_nextUnitWeight_Overlap = _nextPlayUnit.UnitWeight;
				if (!_nextPlayUnit.IsLoop)
				{
					_nextPlayUnit.ResetPlay();
				}
			}
			else
			{
				_nextPlayUnit.ResetPlay();
			}
		}



		public void PlayNewAt(apAnimPlayUnit nextPlayUnit, int frame, float tBlend)
		{
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.New;
			_nextPlayUnit = nextPlayUnit;
			_nextPlayUnit.SetOwnerRequest_Next(this);

			_prevWaitingPlayUnit = null;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = 0.0f;
			_tActiveEnd = _tBlend;
			_tActiveMiddle = _tBlend * 0.5f;

			_status = STATUS.Active;//<<바로 시작

			_isNextPlayUnitIsInPrevUnit = false;

			_isNextPlayUnitIsInPrevUnit = _parentAnimPlayQueue.IsAlreadyAnimUnitPlayed(_nextPlayUnit);

			_nextUnitWeight_Overlap = 1.0f;

			//추가 1.15 사용자가 지정한 시작 프레임 > On
			_isResetPlayAtStartFrame = false;
			_frameToStart = frame;

			if (_isNextPlayUnitIsInPrevUnit)
			{
				_nextUnitWeight_Overlap = _nextPlayUnit.UnitWeight;
				if (!_nextPlayUnit.IsLoop)
				{
					_nextPlayUnit.ResetPlayAt(frame);//<<이게 바뀜
				}
			}
			else
			{
				_nextPlayUnit.ResetPlayAt(frame);//<<이게 바뀜
			}
		}

		public void PlayQueued(apAnimPlayUnit nextPlayUnit, apAnimPlayUnit prevLastPlayUnit, float tBlend)
		{
			//Debug.LogError(">> AnimRequest <Queued> : " + nextPlayUnit._linkedAnimClip._name + " >> 대기");

			//_tNextPlayStart = -1;//Queued 타입은 플레이 시간을 받지 않는다.
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.Queued;
			_nextPlayUnit = nextPlayUnit;
			_nextPlayUnit.SetOwnerRequest_Next(this);

			_prevWaitingPlayUnit = prevLastPlayUnit;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = -1.0f;
			_tActiveEnd = -1.0f;//<<알 수 없다.
			_tActiveMiddle = -1.0f;

			_status = STATUS.Ready;//<<일단 대기

			_isNextPlayUnitIsInPrevUnit = false;

			//추가 1.15 사용자가 지정한 시작 프레임 > Off
			_isResetPlayAtStartFrame = true;
			_frameToStart = -1;

			_nextUnitWeight_Overlap = 1.0f;

			
		}


		public void PlayQueuedAt(apAnimPlayUnit nextPlayUnit, apAnimPlayUnit prevLastPlayUnit, int frame, float tBlend)
		{
			//Debug.LogError(">> AnimRequest <Queued> : " + nextPlayUnit._linkedAnimClip._name + " >> 대기");

			//_tNextPlayStart = -1;//Queued 타입은 플레이 시간을 받지 않는다.
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.Queued;
			_nextPlayUnit = nextPlayUnit;
			_nextPlayUnit.SetOwnerRequest_Next(this);

			_prevWaitingPlayUnit = prevLastPlayUnit;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = -1.0f;
			_tActiveEnd = -1.0f;//<<알 수 없다.
			_tActiveMiddle = -1.0f;

			_status = STATUS.Ready;//<<일단 대기

			_isNextPlayUnitIsInPrevUnit = false;

			_nextUnitWeight_Overlap = 1.0f;

			//추가 1.15 사용자가 지정한 시작 프레임 > On
			_isResetPlayAtStartFrame = false;
			_frameToStart = frame;
		}

		public void Stop(float tBlend)
		{
			
			//_tNextPlayStart = -1;
			_tBlend = tBlend;
			_tLive = 0.0f;

			_isNextUnitPlayed = false;
			_isNextUnitFrameReset = false;

			_requestType = REQUEST_TYPE.Stop;
			_nextPlayUnit = null;
			_prevWaitingPlayUnit = null;

			_requestWeight = 0.0f;
			_nextUnitWeight = 1.0f;
			_prevUnitWeight = 1.0f;

			_tActiveStart = 0.0f;
			_tActiveEnd = _tBlend;
			_tActiveMiddle = _tBlend * 0.5f;
			_status = STATUS.Active;//<<바로 시작

			_isNextPlayUnitIsInPrevUnit = false;

		}

		// Update
		//--------------------------------------------------
		public void Update(float tDelta, int index)
		{
			//if (_nextPlayUnit != null)
			//{
			//	Debug.Log("[" + index +" - " + _requestType + "] Request Update : " + _nextPlayUnit._linkedAnimClip._name);
			//}
			//else
			//{
			//	Debug.Log("[" + index +" - " + _requestType + "] Request Update : Null");
			//}

			switch (_requestType)
			{
				case REQUEST_TYPE.New:
					{
						switch (_status)
						{
							case STATUS.Ready:
							case STATUS.Active:
								//Ready 상태가 없다. 있어도 Active로 처리
								if (!_isNextUnitPlayed)
								{
									if (_nextPlayUnit != null && _nextPlayUnit.NextOwnerRequest == this//<<일단 소유권 문제는 제외하자
										)
									{
										//Debug.LogError(">>>> [New] Play AnimClip : " + _nextPlayUnit._linkedAnimClip._name);
										_parentAnimPlayQueue.RefreshPlayOrderAll();//전체 Order를 갱신
										_nextPlayUnit.Play();
									}
									//else
									//{
									//	//Debug.LogError(">>>> [New] Play AnimClip : Failed");
									//	if(_nextPlayUnit == null)
									//	{
									//		Debug.LogError(">>>> 대상 PlayUnit이 없다.");
									//	}
									//	else if(_nextPlayUnit.NextOwnerRequest != this)
									//	{
									//		Debug.LogError(">>>> 대상 PlayUnit의 소유권이 Request에 없다.");
									//	}
									//}
									_isNextUnitPlayed = true;
									_tActiveStart = _tLive;
									_tActiveMiddle = (_tActiveStart + _tActiveEnd) * 0.5f;
								}

								_tLive += tDelta;

								float itpLive = (_tLive - _tActiveStart);
								float itpMiddle = (_tActiveMiddle - _tActiveStart);
								float itpEnd = (_tActiveEnd - _tActiveStart);


								if (_tLive >= _tActiveEnd || _tActiveEnd < BIAS_ZERO)
								{
									_status = STATUS.End;//끝!
									_nextUnitWeight = 1.0f;
									_prevUnitWeight = 0.0f;


									//for (int i = 0; i < _prevPlayUnits.Count; i++)
									//{
									//	if (_nextPlayUnit != _prevPlayUnits[i] 
									//		//&& _prevPlayUnits[i].PrevOwnerRequest == this
									//		)
									//	{
									//		_prevPlayUnits[i].SetEnd();
									//	}
									//}

									//변경 -> Queue가 간접적으로 End하도록 만든다.
									_parentAnimPlayQueue.SetEndByRequest(this);
								}
								else
								{
									//int calType = 0;
									if (_isNextPlayUnitIsInPrevUnit)
									//if(false)
									{
										//만약 Prev 유닛에 이미 재생중이었다면
										if (_tLive < _tActiveMiddle)
										{
											//절반 동안은 서서히 내려가고 (이미 재생중이었으므로)
											_nextUnitWeight = (1.0f - (itpLive / itpMiddle)) * _nextUnitWeight_Overlap;
											//_nextUnitWeight = 0.0f;

											//calType = 0;
										}
										else
										{
											//그 나머지는 1로 올라간다.
											_nextUnitWeight = ((itpLive - itpMiddle) / (itpEnd - itpMiddle));

											//calType = 1;
											if (!_isNextUnitFrameReset)
											{
												//프레임을 여기서 리셋한다.
												if (_nextPlayUnit != null && _nextPlayUnit.NextOwnerRequest == this)
												{
													_parentAnimPlayQueue.RefreshPlayOrderAll();//전체 Order를 갱신
													if (!_nextPlayUnit.IsLoop)
													{
														_nextPlayUnit.ResetPlay();
													}
													else
													{
														_nextPlayUnit.Resume();
													}
												}
												_isNextUnitFrameReset = true;
											}
										}
									}
									else
									{
										//새로운 NextUnit이 재생을 시작했다면 (기본)
										_nextUnitWeight = itpLive / itpEnd;

										//calType = 2;
									}

									_prevUnitWeight = 1.0f - (itpLive / itpEnd);
									//Debug.Log("Fade [" + _prevUnitWeight + " > " + _nextUnitWeight + " (" + (_prevUnitWeight + _nextUnitWeight) + ") ] - Overlap [" + _nextUnitWeight_Overlap + "]");
									//if((_prevUnitWeight + _nextUnitWeight) > 1.02f 
									//	|| (_prevUnitWeight + _nextUnitWeight) < 0.98f)
									//{
									//	Debug.Log("ITP Live : " + itpLive + " / ITP Middle : " + itpMiddle + " / ITP End : " + itpEnd + " / Cal Type : " + calType);
									//}
								}
								break;

							case STATUS.End:
								_nextUnitWeight = 1.0f;
								_prevUnitWeight = 0.0f;
								break;
						}
					}
					break;

				case REQUEST_TYPE.Queued:
					{
						switch (_status)
						{
							case STATUS.Ready:
								{
									//여기가 중요
									//대기중인 AnimPlayUnit의 종료를 기다린다.
									//if(_prevWaitingPlayUnit == null)
									//{
									//	_status = STATUS.End;
									//	_nextUnitWeight = 0.0f;
									//	break;
									//}

									_tLive += tDelta;
									float remainTime = 0.0f;

									if (_prevWaitingPlayUnit != null)
									{
										remainTime = _prevWaitingPlayUnit.RemainPlayTime;
									}

									if (remainTime <= _tBlend + BIAS_ZERO)
									{
										
										_status = STATUS.Active;
										// Blend 시간을 포함하여 다음 PlayUnit을 실행할 수 있게 되었다.
										//Debug.LogError("Queue Ready >> Active (Remain : " + remainTime + " / Blend Time : " + _tBlend + ")");

										//현재 시간을 기점으로 Start-End 시간을 만든다.
										_tActiveStart = _tLive;
										_tActiveEnd = _tActiveStart + _tBlend;
										_tActiveMiddle = (_tActiveStart + _tActiveEnd) * 0.5f;

										_nextUnitWeight = 0.0f;//<<아직은 0
										_prevUnitWeight = 1.0f;


										//이걸 플레이하는 시점에서 지정
										_isNextPlayUnitIsInPrevUnit = _parentAnimPlayQueue.IsAlreadyAnimUnitPlayed(_nextPlayUnit);


										_nextUnitWeight_Overlap = 1.0f;
										if (_isNextPlayUnitIsInPrevUnit)
										{
											//Debug.LogError("[" + _nextPlayUnit._linkedAnimClip._name + "] Overlap 상태로 Queue 시작 : 현재 Weight : " + _nextPlayUnit.UnitWeight);
											_nextUnitWeight_Overlap = _nextPlayUnit.UnitWeight;

										}
										else
										{
											_nextUnitWeight_Overlap = 0.0f;
										}
										
									}
									else
									{
										//대기..
										//Debug.Log("Queue Ready (Remain : " + remainTime + " / Blend Time : " + _tBlend + ")");
										_nextUnitWeight = 0.0f;
										_prevUnitWeight = 1.0f;
									}

								}
								break;

							case STATUS.Active:
								if (!_isNextUnitPlayed)
								{
									if (_nextPlayUnit != null && _nextPlayUnit.NextOwnerRequest == this)
									{
										//Debug.LogError(">>>> [Queued] Play AnimClip : " + _nextPlayUnit._linkedAnimClip._name);
										//여기서는 Order를 갱신하지 않는다.
										if(!_isNextPlayUnitIsInPrevUnit)
										{
											_parentAnimPlayQueue.RefreshPlayOrderAll();//전체 Order를 갱신
											//Debug.Log(">> Play (" + _isResetPlayAtStartFrame + " / " + _frameToStart + ")");
											if (_isResetPlayAtStartFrame)
											{
												_nextPlayUnit.Play();
											}
											else
											{
												_nextPlayUnit.PlayAt(_frameToStart);
												_isResetPlayAtStartFrame = true;//<<초기화
											}
										}
										
									}
									//else
									//{
									//	Debug.LogError(">>>> [Queued] Play AnimClip : Failed");
									//}
									//_tLive = 0.0f;
									_isNextUnitPlayed = true;
								}

								_tLive += tDelta;
								float itpLive = (_tLive - _tActiveStart);
								float itpMiddle = (_tActiveMiddle - _tActiveStart);
								float itpEnd = (_tActiveEnd - _tActiveStart);

								if (_tLive >= _tActiveEnd || _tActiveEnd < BIAS_ZERO)
								{
									_status = STATUS.End;//끝!
									_nextUnitWeight = 1.0f;
									_prevUnitWeight = 0.0f;

									//for (int i = 0; i < _prevPlayUnits.Count; i++)
									//{
									//	if (_nextPlayUnit != _prevPlayUnits[i] 
									//		//&& _prevPlayUnits[i].PrevOwnerRequest == this
									//		)
									//	{
									//		_prevPlayUnits[i].SetEnd();
									//	}
									//}

									//변경 -> Queue가 간접적으로 End하도록 만든다.
									_parentAnimPlayQueue.SetEndByRequest(this);
								}
								else
								{
									if (_isNextPlayUnitIsInPrevUnit)
									{
										//만약 Prev 유닛에 이미 재생중이었다면
										if (_tLive < _tActiveMiddle)
										{
											//절반 동안은 서서히 내려가고 (이미 재생중이었으므로)
											_nextUnitWeight = (1.0f - (itpLive / itpMiddle)) * _nextUnitWeight_Overlap;
										}
										else
										{
											//그 나머지는 1로 올라간다.
											_nextUnitWeight = ((itpLive - itpMiddle) / itpMiddle);
											if (!_isNextUnitFrameReset)
											{
												//프레임을 여기서 리셋한다.
												if (_nextPlayUnit != null && _nextPlayUnit.NextOwnerRequest == this)
												{
													_parentAnimPlayQueue.RefreshPlayOrderAll();//전체 Order를 갱신
													if (!_nextPlayUnit.IsLoop)
													{
														//Debug.LogError(">>> [Queued] Reset Play : " + _nextPlayUnit._linkedAnimClip._name);
														//Debug.Log(">> ResetPlay (" + _isResetPlayAtStartFrame + " / " + _frameToStart + ")");
														if (_isResetPlayAtStartFrame)
														{
															_nextPlayUnit.ResetPlay();
														}
														else
														{
															_nextPlayUnit.ResetPlayAt(_frameToStart);
															_isResetPlayAtStartFrame = true;
														}
													}
													else
													{
														_nextPlayUnit.Resume();
													}
												}
												_isNextUnitFrameReset = true;
											}
										}
									}
									else
									{
										//새로운 NextUnit이 재생을 시작했다면 (기본)
										_nextUnitWeight = Mathf.Clamp01(itpLive / itpEnd);
									}

									_prevUnitWeight = 1.0f - Mathf.Clamp01(itpLive / itpEnd);
									
								}
								break;

							case STATUS.End:
								_nextUnitWeight = 1.0f;
								_prevUnitWeight = 0.0f;
								break;
						}
					}
					break;

				case REQUEST_TYPE.Stop:
					{
						switch (_status)
						{
							case STATUS.Ready:
							case STATUS.Active:
								//Ready 상태가 없다. 있어도 Active로 처리


								_tLive += tDelta;
								if (_tLive >= _tActiveEnd || _tActiveEnd < BIAS_ZERO)
								{
									_status = STATUS.End;//끝!
									_nextUnitWeight = 1.0f;
									_prevUnitWeight = 0.0f;


									//for (int i = 0; i < _prevPlayUnits.Count; i++)
									//{
									//	if (_prevPlayUnits[i] != null && _prevPlayUnits[i].PrevOwnerRequest == this)
									//	{
									//		_prevPlayUnits[i].SetEnd();
									//	}
									//}

									//변경 -> Queue가 간접적으로 End하도록 만든다.
									_parentAnimPlayQueue.SetEndByRequest(this);
								}
								else
								{
									_nextUnitWeight = _tLive / _tActiveEnd;
									_prevUnitWeight = 1.0f - _nextUnitWeight;
								}
								break;

							case STATUS.End:
								_nextUnitWeight = 1.0f;
								_prevUnitWeight = 0.0f;
								break;
						}
					}
					break;

			}

		}






		// Functions
		//--------------------------------------------------
		public void AdaptWeightToPlayUnits()
		{
			//float weight2Next = _nextUnitWeight * _requestWeight;
			//float weight2Prev = (1 - _nextUnitWeight) * (_requestWeight);
			//float weight2Prev = _prevUnitWeight * _requestWeight;

			_parentAnimPlayQueue.SetRequestWeightToPlayUnits(this, _prevUnitWeight, _nextUnitWeight, _requestWeight);

			
		}

		public void ReleasePlayUnitLink()
		{
			if(_nextPlayUnit != null && _nextPlayUnit.NextOwnerRequest == this)
			{
				_nextPlayUnit.SetOwnerRequest_Next(null);
			}
			//이건 잘 생각해봐야할듯
			//for (int i = 0; i < _prevPlayUnits.Count; i++)
			//{
			//	if (_prevPlayUnits[i] != null && _prevPlayUnits[i].PrevOwnerRequest == this)
			//	{
			//		_prevPlayUnits[i].SetOwnerRequest_Prev(null);
			//	}
			//}
		}

		// Get / Set
		//--------------------------------------------------
		public bool IsLive { get { return _status == STATUS.Active; } }
		public bool IsEnded { get { return _status == STATUS.End; } }
		//public bool 
		//private float _tBlend = 0.0f;
		//private float _tLive = 0.0f;
		//private bool _isLive = false;
		//private bool _isFirstLive = false;

		//public enum REQUEST_TYPE
		//{
		//	New,
		//	Queued,//<<시간으로 재는 것이 아니라, 현재 대기중인 PlayData의 재생 시간을 보고 결정한다.
		//	Stop,//New와 비슷하게 처리를 하지만, 다음에 재생되는 Unit은 없다.
		//}
		//private REQUEST_TYPE _requestType = REQUEST_TYPE.New;

		////다음에 플레이하게될 PlayUnit (필수)
		//private apAnimPlayUnit _nextPlayUnit = null;

		////Queue에 들어간 PlayData 중 마지막 데이터.
		////만약 requestType이 Queued라면, Queue 상태로 저장된다. 
		//private apAnimPlayUnit _prevWaitingPlayUnit = null;




		////NextPlayUnit에 대한 Weight. 선형으로 계산된다.
		////이전 PlayUnit "전체"에 대해서는 (1-_playUnitWeight)의 값이 곱해진다.
		//private float _playUnitWeight = 0.0f;

		////Request 자체에 대한 Weight.
		////Request끼리 중첩되는 경우 (PlayUnit이 중첩되는게 아니라..)
		////Request간의 보간도 있어야 한다.
		////이때 Weight 주도권은 나중에 선언된 Request에 있다.
		//private float _requestWeight = 1.0f;

		public void SetRequestWeight(float requestWeight)
		{
			_requestWeight = requestWeight;
		}

		public void MultiplyRequestWeight(float decreaseRatio)
		{
			_requestWeight = Mathf.Clamp01(_requestWeight * decreaseRatio);
		}

		public float RequestWeight { get { return _requestWeight; } }
		public float Current2StartTime { get { return Mathf.Max(_tLive - _tActiveStart, 0); } }
		public float Current2EndTime { get { return Mathf.Max(_tActiveEnd - _tLive, 0); } }

		public REQUEST_TYPE RequestType { get { return _requestType; } }

	}
}