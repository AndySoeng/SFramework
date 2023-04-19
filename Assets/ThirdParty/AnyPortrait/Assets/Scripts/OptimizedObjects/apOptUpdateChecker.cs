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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//#if UNITY_EDITOR
//using UnityEngine.Profiling;
//#endif
using System;

using AnyPortrait;


namespace AnyPortrait
{
	//Important를 끈 객체가 간헐적인 업데이트를 할 때, 
	public class apOptUpdateChecker
	{
		// Const
		//---------------------------------------------------
		//이전

		//private const int MAX_FPS = 60;
		//private const int MIN_FPS = 2;
		////private const int MAX_INV_LOW_FPS_RATIO = 4;

		//private const int REMOVABLE_COUNT = 100;


		//리뉴얼
		//토큰의 FPS 범위
		private const int MAX_TOKEN_FPS = 60;
		private const int MIN_TOKEN_FPS = 2;
		
		//이 카운트동안 Update 호출이 되지 않았다면, 이 토큰은 더이상 사용되지 않는 것
		private const int MAX_TOKEN_UNUSED_COUNT = 10;
		private const int MAX_TOKENLIST_UNUSED_COUNT = 60;

		// SubClass
		//---------------------------------------------------
		public class UpdateToken
		{
			//이전
			#region [미사용 코드]
			//private int _FPS = -1;
			//public int _delayedFrame = 0;


			//private bool _result = false;
			//private float _elapsedTime = 0.0f;
			//private float _updatableTimeLength = 0.0f;

			//private float _resultTime = 0.0f;
			//private bool _isOverDelayed = false;
			//private int _resultDelayedFrame = 0; 
			#endregion

			//리뉴얼 코드
			private int _FPS = -1;
			private int _unusedCount = 0;

			//LateUpdate에서 업데이트 가능한지 확인하기 위한 슬롯 정보

			private int _slotIndex = 0;
			private float _elapsedTime = 0.0f;
			private float _resultElapsedTime = 0.0f;
			
			private TokenList _parentTokenList = null;
			private bool _isUpdatable = false;




			public UpdateToken(int fps)
			{
				//이전
				#region [미사용 코드]
				//_FPS = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);
				//_delayedFrame = 0;
				//_result = false;
				//_elapsedTime = 0.0f;
				//_resultTime = 0.0f;

				//_updatableTimeLength = (1.0f / (float)_FPS) - 0.01f;//Bias만큼 약간 더 감소 
				#endregion

				//리뉴얼
				_FPS = fps;
				_unusedCount = 0;
				_slotIndex = -1;
				_elapsedTime = 0.0f;
				_resultElapsedTime = 0.0f;
				_parentTokenList = null;

				_isUpdatable = false;
			}

			public bool SetFPS(int fps)
			{
				#region [미사용 코드]
				//if(fps != _FPS)
				//{
				//	_FPS = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);
				//	_updatableTimeLength = (1.0f / (float)_FPS) - 0.01f;
				//} 
				#endregion

				bool isChanged = (_FPS != fps);
				_FPS = fps;
				return isChanged;
			}

			public void SetParentTokenList(TokenList tokenList)
			{
				_parentTokenList = tokenList;
			}

			public void Update(float deltaTime)
			{
				//_elapsedTime += deltaTime;
				_elapsedTime += deltaTime;
				_unusedCount = 0;//업데이트가 되었다면 카운트를 0으로 초기화

			}

			public void ReadyToUpdate()
			{
				_unusedCount++;//일단 카운트 1 올린다.
				_isUpdatable = false;
			}

			public void SetSlotIndex(int nextSlotIndex)
			{
				_slotIndex = nextSlotIndex;
			}

			public void SetUpdatable(bool isUpdatable)
			{
				_isUpdatable = isUpdatable;
				if(_isUpdatable)
				{
					//업데이트가 될 것이라면 경과 시간을 갱신한다.
					_resultElapsedTime = _elapsedTime;
					_elapsedTime = 0.0f;
				}
			}


			//Get
			public int SlotIndex { get { return _slotIndex; } }
			public TokenList ParentTokenList {  get { return _parentTokenList; } }
			public float ResultElapsedTime {  get { return _resultElapsedTime; } }

			//연속으로 업데이트 요청이 없었다면, 성능을 위해서 토큰을 리스트에서 삭제하자.
			public bool IsRemovable { get { return _unusedCount > MAX_TOKEN_UNUSED_COUNT; } }
			public bool IsUpdatable { get { return _isUpdatable; } }

			#region [미사용 코드]
			//public bool IsUpdatable()
			//{
			//	return _elapsedTime > _updatableTimeLength;
			//}

			//public bool IsUpdatableInLowSceneFPS(float lowFPSRatio)
			//{
			//	return _elapsedTime > (_updatableTimeLength / lowFPSRatio) - 0.01f;//Bias만큼 약간 더 감소
			//}

			//public void ReadyToCalculate()
			//{
			//	_result = false;
			//	_resultTime = 0.0f;
			//	_isOverDelayed = false;
			//	_resultDelayedFrame = 0;
			//}

			//public void SetSuccess(bool isOverDelayed)
			//{
			//	//_result = true;
			//	//_resultTime = _elapsedTime;
			//	//_elapsedTime = 0.0f;


			//	//_resultDelayedFrame = _delayedFrame;
			//	//_isOverDelayed = isOverDelayed;

			//	//_delayedFrame = 0;
			//}


			//public bool SetFailIfNotCalculated()
			//{
			//	if(_result)
			//	{
			//		return false;
			//	}

			//	_resultTime = 0.0f;
			//	_delayedFrame++;

			//	return true;
			//}

			//public bool IsSuccess { get { return _result; } }
			//public float ResultElapsedTime { get { return _resultTime; } }
			//public bool IsOverDelayed { get { return _isOverDelayed; } }
			//public int DelayedFrame {  get {  return _resultDelayedFrame; } } 
			#endregion
		}

		public class TokenList
		{
			public int _FPS = -1;//키값이 되는 FPS

			#region [미사용 코드]
			//private List<UpdateToken> _tokens = null;

			//private int _nRequest = 0;//<<요청 개수
			//private int _maxDelay = 0;//<<몇개의 그룹으로 분할해야 하는가
			////private int _maxDelayLowSceneFPS = 0;//<<몇개의 그룹으로 분할해야 하는가

			//private int _maxCount = -1;
			//private int _successCount = -1;

			//private Dictionary<int, List<UpdateToken>> _delayedTokens = null;//바로 처리되지 못하고 잠시 딜레이된 토큰들. 계산용

			////private float _tSceneFrame = 0.0f;//유니티 씬에서의 Frame 시간
			//private int _sceneFPS = 0;

			//private float _countBias = 1.0f;

			//private float _tCycle = 0.0f;
			//private float _tCycleLength = 0.0f;

			//private int _cycle_totalRequests = 0;
			//private int _cycle_totalFailed = 0;
			//private int _cycle_totalMargin = 0;

			////private int _prevBias = 0;

			////만약 SceneFPS가 목표된 FPS보다 낮은 경우 (이건 Cycle마다 체크한다)
			//private bool _isLowSceneFPS = false;
			//private int _lowSceneFPS = 0;

			////private int _lowFPS_Min = 0;

			//private int _lowFPS = 0;
			//private int _avgSceneFPS = 0;
			//private int _nSceneFPSCount = 0;
			//private float _lowFPSRatio = 0.0f;

			////삭제 가능성
			////긴 프레임동안 Request가 없었던 토큰 리스트는 삭제되어야 한다.
			//private bool _isRemovable = false;
			//private int _nNoRequestFrames = 0; 
			#endregion

			//전체 토큰 리스트
			private List<UpdateToken> _tokens_All = new List<UpdateToken>();

			//<슬롯 인덱스, 토큰 리스트>
			private Dictionary<int, List<UpdateToken>> _tokens_PerSlots = new Dictionary<int, List<UpdateToken>>();


			//게임의 FPS에 따라서 슬롯의 크기를 정한다.
			private int _slotSize = 0;

			//이것도 업데이트 호출이 안된 상태로 몇차례 시간이 지나면 자동으로 폐기한다.
			private int _unusedCount = 0;
			private List<UpdateToken> _removableTokens = new List<UpdateToken>();

			//"현재 업데이트될 슬롯 인덱스" > 커서
			//LateUpdate에서 하나씩 증가하며, 이 커서와 같은 경우에만 토큰이 업데이트 될 수 있다.
			private int _updateCursor = 0;


			public TokenList(int fps)
			{
				#region [미사용 코드]
				//_FPS = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);
				//_maxDelay = Mathf.Max((MAX_FPS / _FPS) + 1, 2);

				//_tokens = new List<UpdateToken>();
				//_delayedTokens = new Dictionary<int, List<UpdateToken>>();
				//for (int i = 0; i < _maxDelay; i++)
				//{
				//	_delayedTokens.Add(i, new List<UpdateToken>());
				//}

				//_tCycle = 0.0f;
				//_tCycleLength = 1.0f / (float)_FPS;

				//_isLowSceneFPS = false;
				//_lowSceneFPS = 0;
				//_avgSceneFPS = 0;
				//_nSceneFPSCount = 0;

				//_isRemovable = false;
				//_nNoRequestFrames = 0; 
				#endregion

				_FPS = fps;

				if(_tokens_All == null)
				{
					_tokens_All = new List<UpdateToken>();
				}
				if(_tokens_PerSlots == null)
				{
					_tokens_PerSlots = new Dictionary<int, List<UpdateToken>>();
				}
				if(_removableTokens == null)
				{
					_removableTokens = new List<UpdateToken>();
				}
				_slotSize = 0;
				_unusedCount = 0;
				_updateCursor = 0;
			}

			#region [미사용 코드]
			//public void Reset(float deltaTime)
			//{
			//	_nRequest = 0;
			//	_maxCount = 0;
			//	_successCount = 0;

			//	if(deltaTime > 0.0f)
			//	{
			//		_sceneFPS = (int)(1.0f / deltaTime);
			//	}
			//	else
			//	{
			//		_sceneFPS = MIN_FPS;
			//	}

			//	_avgSceneFPS += _sceneFPS;
			//	_nSceneFPSCount++;

			//	_tCycle += deltaTime;
			//	if(_tCycle > _tCycleLength)
			//	{
			//		// 이 토큰의 사이클이 한바퀴 돌았다
			//		//크기 보정 배수를 재계산하자
			//		if(_cycle_totalRequests == 0)
			//		{
			//			_countBias = 1.0f;

			//		}
			//		else
			//		{
			//			//보정값
			//			//처리 횟수
			//			//성공 + 실패 = 전체
			//			//maxSize대비 -> 
			//			//1 + ((실패 - 잉여) / 전체) + 0.5 (Bias)
			//			//단, 실패에 약간의 가중치가 더 붙는다.
			//			//float newCountBias = 1.5f + ((float)(_cycle_totalFailed * 1.5f - _cycle_totalMargin * 0.5f) / (float)_cycle_totalRequests );
			//			//float newCountBias = 1.0f;
			//			//float prevBias = _countBias;
			//			//float newCountBias = _countBias;
			//			if (_cycle_totalFailed > 0)
			//			{
			//				//newCountBias += ((float)(_cycle_totalFailed) / (float)_cycle_totalRequests);
			//				_countBias *= 1.1f;

			//				//Debug.LogError("[" + _FPS  + "] 배수 증가 : " + prevBias + " > " + _countBias);

			//			}
			//			else if (_cycle_totalMargin > 0)
			//			{
			//				//newCountBias -= ((float)(_cycle_totalMargin) / (float)_cycle_totalRequests);
			//				_countBias *= 0.95f;

			//				//Debug.LogWarning("[" + _FPS  + "] 배수 감소 : " + prevBias + " > " + _countBias);
			//			}
			//		}
			//		_cycle_totalRequests = 0;
			//		_cycle_totalFailed = 0;
			//		_cycle_totalMargin = 0;

			//		_tCycle = 0.0f;

			//		if(_nSceneFPSCount > 0)
			//		{
			//			_avgSceneFPS = _avgSceneFPS / _nSceneFPSCount;
			//			if(_avgSceneFPS / 2 < _FPS)
			//			{
			//				//실행중인 프레임이 매우 낮아서 실제 업데이트되는 FPS를 낮추어야 한다.
			//				//실제 FPS는 그 절반으로 낮추어야 한다.

			//				_lowSceneFPS = _avgSceneFPS;
			//				_lowFPS = _lowSceneFPS / 2;

			//				if(_lowFPS < MIN_FPS)
			//				{
			//					_lowFPS = MIN_FPS;
			//				}

			//				_lowFPSRatio = (float)_lowFPS / (float)_FPS;

			//				//if (!_isLowSceneFPS)
			//				//{
			//				//	Debug.LogWarning("[" + _FPS + "] Low Scene FPS : " + _FPS + " >> " + _lowFPS);
			//				//}

			//				_isLowSceneFPS = true;
			//			}
			//			else
			//			{	
			//				_lowSceneFPS = 0;
			//				_lowFPSRatio = 1.0f;
			//				_lowFPS = _FPS;

			//				//if (_isLowSceneFPS)
			//				//{
			//				//	Debug.Log("[" + _FPS + "] Recover Scene FPS");
			//				//}

			//				_isLowSceneFPS = false;
			//			}
			//		}
			//		else
			//		{
			//			_isLowSceneFPS = false;
			//			_lowSceneFPS = 0;
			//			_lowFPSRatio = 1.0f;
			//			_lowFPS = _FPS;
			//		}

			//		_avgSceneFPS = 0;
			//		_nSceneFPSCount = 0;
			//	}

			//	_tokens.Clear();

			//	for (int i = 0; i < _maxDelay; i++)
			//	{
			//		_delayedTokens[i].Clear();
			//	}
			//}

			//public void AddRequest(UpdateToken token)
			//{
			//	//token._result = false;//<<일단 False로 설정
			//	//token.ReadyToCalculate();//계산 준비 //<< AddRequest가 호출되기 전에 이미 호출되었다.

			//	if(_isLowSceneFPS)
			//	{
			//		//만약, 현재 Scene의 FPS가 낮다면,
			//		//이 토큰의 업데이트 여부를 한번더 체크해야한다.
			//		if(token.IsUpdatableInLowSceneFPS(_lowFPSRatio))
			//		{
			//			_nRequest++;
			//			_tokens.Add(token);
			//		}
			//	}
			//	else
			//	{
			//		_nRequest++;
			//		_tokens.Add(token);
			//	}


			//}

			//public bool Calculate()
			//{
			//	//이게 핵심. 
			//	//- maxCount 결정 + curCount = 0

			//	//- 요청된 토큰의 각각의 가중치를 보고 처리할 수 있는지 여부를 결정
			//	//1. DelayedFrame이 divide를 넘었으면 무조건 처리 => Sort 필요 없음
			//	//2. DelayedFrame이 큰것 부터 처리. curCount가 maxCount보다 크면 종료

			//	//- result가 true인 토큰은 delayedFrame을 0으로 초기화
			//	//- result가 false인 토큰은 delayedFrame을 1 증가

			//	if(_nRequest == 0)
			//	{
			//		if (!_isRemovable)
			//		{
			//			_nNoRequestFrames++;

			//			if (_nNoRequestFrames > REMOVABLE_COUNT)
			//			{
			//				_isRemovable = true;
			//			}
			//		}
			//		return _isRemovable;
			//	}

			//	_isRemovable = false;
			//	_nNoRequestFrames = 0;

			//	UpdateToken token = null;

			//	//프레임이 너무 낮은 경우 전부 OverDelayed를 하고 처리하자
			//	if(_sceneFPS < MIN_FPS)
			//	{
			//		for (int i = 0; i < _tokens.Count; i++)
			//		{
			//			token = _tokens[i];
			//			if (token == null)
			//			{
			//				return false;
			//			}

			//			token.SetSuccess(true);
			//			_successCount++;
			//		}

			//		_cycle_totalFailed += _nRequest;
			//		return false;
			//	}

			//	//Slot의 크기를 구하자
			//	//- 기본 : 전체 요청 / (현재 프레임 / FPS) => (전체 요청 * FPS) / 현재 프레임

			//	if(_isLowSceneFPS)
			//	{
			//		_maxCount = ((_nRequest * _lowFPS) / _sceneFPS) + 1;
			//	}
			//	else
			//	{
			//		_maxCount = ((_nRequest * _FPS) / _sceneFPS) + 1;
			//	}

			//	_maxCount = (int)(_maxCount * _countBias + 0.5f);//<<알아서 바뀌는 보정값으로 변경


			//	_successCount = 0;

			//	//1차로 : 무조건 처리해야하는거 찾기
			//	for (int i = 0; i < _tokens.Count; i++)
			//	{
			//		token = _tokens[i];
			//		if(token == null)
			//		{
			//			continue;
			//		}

			//		if (token._delayedFrame >= _maxDelay)
			//		{
			//			//한계치를 넘었다. > 무조건 성공
			//			//token._result = true;
			//			token.SetSuccess(true);
			//			_successCount++;
			//		}
			//		else
			//		{
			//			//일단 뒤로 미루자
			//			_delayedTokens[token._delayedFrame].Add(token);
			//		}
			//	}

			//	if(_successCount < _maxCount)
			//	{
			//		//아직 더 처리할 수 있다면
			//		//delayedFrame이 큰것부터 처리하자
			//		List<UpdateToken> delayedList = null;
			//		for (int iDelay = _maxDelay - 1; iDelay >= 0; iDelay--)
			//		{	
			//			delayedList = _delayedTokens[iDelay];
			//			for (int i = 0; i < delayedList.Count; i++)
			//			{
			//				token = delayedList[i];
			//				//token._result = true;
			//				token.SetSuccess(false);
			//				_successCount++;

			//				//처리가 모두 끝났으면 리턴
			//				if(_successCount >= _maxCount)
			//				{
			//					break;
			//				}
			//			}

			//			//처리가 모두 끝났으면 리턴
			//			if(_successCount >= _maxCount)
			//			{
			//				break;
			//			}
			//		}
			//	}

			//	int nFailed = 0;
			//	for (int i = 0; i < _tokens.Count; i++)
			//	{
			//		token = _tokens[i];
			//		//<<처리되지 않았다면 Fail 처리
			//		if(token.SetFailIfNotCalculated())
			//		{
			//			nFailed++;
			//		}
			//	}

			//	_cycle_totalRequests += _nRequest;

			//	//실패값
			//	if (_nRequest > _maxCount)
			//	{
			//		_cycle_totalFailed += _nRequest - _maxCount;
			//	}

			//	//잉여값
			//	if (_nRequest < _maxCount)
			//	{
			//		_cycle_totalMargin += _maxCount - _nRequest;
			//	}

			//	return false;
			//}

			//public bool IsRemovable
			//{
			//	get { return _isRemovable; }
			//} 
			#endregion

			/// <summary>
			/// Update의 첫 계산에서 호출된다.
			/// </summary>
			public void ReadyToUpdate()
			{
				UpdateToken curToken = null;
				for (int i = 0; i < _tokens_All.Count; i++)
				{
					curToken = _tokens_All[i];
					if(curToken == null)
					{
						//null이면 나중에 삭제를 해야한다.
						continue;
					}
					curToken.ReadyToUpdate();
				}
				_unusedCount++;//일단 카운트 1 올린다.
			}

			/// <summary>
			/// 게임의 FPS를 입력 받고 슬롯 크기를 계산한다.
			/// 기존의 슬롯 크기와 다르다면 슬롯을 초기화한다.
			/// </summary>
			/// <param name="gameFPS"></param>
			public void SetGameFPS(int gameFPS)
			{
				//슬롯 크기 계산하기
				//- 기본적으로 (게임 프레임 / 캐릭터 고정 프레임)으로 슬롯을 만들면 딱 맞는다.
				//- 슬롯이 많으면 애니메이션의 FPS가 줄어들며, 슬롯이 적으면 분산이 덜 되서 게임 성능에 영향을 더 준다.
				//- 게임 프레임이 60 밑으로 떨어진다면, 슬롯을 늘려서 성능을 보전해야한다.
				float gameOptParam = 1.0f;
				if(gameFPS < 60)
				{
					//게임 프레임이 60밑으로 떨어지면 강제로 슬롯을 늘려서 처리를 분산시키고 애니메이션 FPS를 줄인다.
					gameOptParam = 60.0f / (float)gameFPS;
				}

				int nextSlotSize = (int)(((float)gameFPS / (float)_FPS) + 0.5f);
				
				
				if(nextSlotSize < 2)
				{
					nextSlotSize = 2;//슬롯 최소 개수는 2이다. (최소 한번의 스킵은 있어야 한다.)
				}

				//게임 FPS가 떨어지면 강제로 애니메이션 FPS를 낮춰야 한다.
				nextSlotSize = (int)((nextSlotSize * gameOptParam) + 0.5f);

				//Debug.Log("Set Game FPS : " + gameFPS + " / Token FPS : " + _FPS + " / Next Slot Size : " + nextSlotSize);

				if(nextSlotSize != _slotSize)
				{
					//초기화
					//Debug.LogError("Slot Size Changed : " + _slotSize + " > " + nextSlotSize);

					_tokens_PerSlots.Clear();
					_slotSize = nextSlotSize;
					for (int i = 0; i < _slotSize; i++)
					{
						_tokens_PerSlots.Add(i, new List<UpdateToken>());
					}

					//< 슬롯 인덱스 재할당 >
					//존재하는 모든 토큰들의 슬롯 인덱스를 다시 설정해준다.
					//앞의 토큰부터 하나씩 0, 1, 2..를 할당한다.
					int curSlotIndex = 0;
					UpdateToken curToken = null;

					for (int i = 0; i < _tokens_All.Count; i++)
					{
						curToken = _tokens_All[i];
						
						if(curToken == null)
						{
							//null인 토큰은 나중에 삭제를 하자
							continue;
						}
						
						curToken.SetSlotIndex(curSlotIndex);//인덱스 변경
						_tokens_PerSlots[curSlotIndex].Add(curToken);//변경된 인덱스에 따른 슬롯 리스트에 재할당

						//슬롯 인덱스는 0, 1, 2... 0, 1, 2..식으로 반복된다.
						curSlotIndex++;
						if(curSlotIndex >= _slotSize)
						{
							curSlotIndex = 0;
						}
					}

					//커서가 슬롯 사이즈보다 크면 다시 0으로 초기화
					if(_updateCursor >= _slotSize)
					{
						_updateCursor = 0;
					}
				}
			}


			public void AddAndUpdateToken(UpdateToken token)
			{
				//추가하거나 이미 있다면 인덱스 체크 후 갱신
				
				bool isNeedToAssignIndex = false;//슬롯 인덱스를 할당해야 하는가
				if(!_tokens_All.Contains(token))
				{
					//새로 추가했다면
					_tokens_All.Add(token);

					isNeedToAssignIndex = true;//슬롯 인덱스도 만들어줘야한다.
				}
				else
				{
					//이미 있다면
					//인덱스가 유효한지 확인하자
					int prevSlotIndex = token.SlotIndex;
					if(prevSlotIndex < 0 || prevSlotIndex >= _slotSize)
					{
						//기존의 슬롯 인덱스가 유효하지 않다.
						isNeedToAssignIndex = true;
					}
					else if(!_tokens_PerSlots[prevSlotIndex].Contains(token))
					{
						//슬롯의 리스트에 존재하지 않다면 다시 할당하자.
						isNeedToAssignIndex = true;
					}

					if (isNeedToAssignIndex)
					{
						//슬롯을 재할당하기 전에, 이전의 슬롯에서 이걸 제거해야한다.
						//"전체 리스트"에는 있었고, "슬롯 리스트에서는 찾지 못한" 상황 ( = 에러)
						for (int i = 0; i < _slotSize; i++)
						{
							_tokens_PerSlots[i].Remove(token);
						}
					}
				}
				if(isNeedToAssignIndex)
				{
					//중요!
					//최적의 슬롯 인덱스를 찾아서 토크에 할당하고, 슬롯 리스트에도 추가한다.
					int optSlotIndex = GetOptimizedSlotIndex();

					token.SetSlotIndex(optSlotIndex);
					_tokens_PerSlots[optSlotIndex].Add(token);

				}


				token.SetParentTokenList(this);//Parent로 등록하자

				//토큰이 입력되었으니 업데이트 카운트를 초기화하자.
				_unusedCount = 0;
			}

			public void RemoveToken(UpdateToken token)
			{
				if (!_tokens_All.Contains(token))
				{
					return;
				}

				//전체 리스트와 슬롯별 리스트에서 모두 삭제하자.
				_tokens_All.Remove(token);
				for (int i = 0; i < _slotSize; i++)
				{
					_tokens_PerSlots[i].Remove(token);
				}
			}

			
			/// <summary>
			/// 토큰들을 상대로 업데이트 되어야 할지, 삭제될지를 정하고, 토큰들에 기록을 해준다.
			/// </summary>
			public void UpdateCursorAndRemoveInvalidTokens()
			{
				UpdateToken curToken = null;
				bool isAnyRemovable = false;
				bool isAnyNullToken = false;//<<하나라도 null이 있다면 검출해서 삭제하자.
				_removableTokens.Clear();
				for (int i = 0; i < _tokens_All.Count; i++)
				{
					curToken = _tokens_All[i];
					if(curToken == null)
					{
						//null토큰이 있다면 삭제를 해야한다.
						isAnyNullToken = true;
						continue;
					}
					if(curToken.IsRemovable)
					{
						isAnyRemovable = true;
						_removableTokens.Add(curToken);
					}
					else
					{
						//커서와 슬롯 인덱스가 같을 때에만 업데이트 된다.
						curToken.SetUpdatable(_updateCursor == curToken.SlotIndex);
					}
				}


				_updateCursor++;//커서 증가
				if(_updateCursor >= _slotSize)
				{
					_updateCursor = 0;
				}

				if(!isAnyRemovable && !isAnyNullToken)
				{
					return;
				}

				if(isAnyNullToken)
				{
					//Null 토큰을 삭제하자.
					//int nRemoved = _tokens_All.RemoveAll(delegate(UpdateToken a)
					_tokens_All.RemoveAll(delegate(UpdateToken a)
					{
						return a == null;
					});

					for (int iSlot = 0; iSlot < _slotSize; iSlot++)
					{
						_tokens_PerSlots[iSlot].RemoveAll(delegate(UpdateToken a)
						{
							return a == null;
						});
					}

					//Debug.LogError("Remove Null Tokens : " + nRemoved);
				}

				if (isAnyRemovable)
				{
					//수명이 다한 토큰을 삭제하자
					//int nRemovedUnusedTokens = _removableTokens.Count;
					for (int i = 0; i < _removableTokens.Count; i++)
					{
						curToken = _removableTokens[i];
						if(curToken == null)
						{
							continue;
						}
						_tokens_All.Remove(curToken);
						for (int iSlot = 0; iSlot < _slotSize; iSlot++)
						{
							_tokens_PerSlots[iSlot].Remove(curToken);
						}
					}

					_removableTokens.Clear();

					//Debug.LogError("Remove Unused Tokens : " + nRemovedUnusedTokens);
				}
			}

			//현재 "가장 적은 개수의 토큰을 보유하고 있는" 슬롯의 인덱스를 구한다.
			private int GetOptimizedSlotIndex()
			{
				int optSlotIndex = -1;
				int minNumTokens = -1;
				
				if(_slotSize == 0)
				{
					return -1;
				}

				int curTokens = 0;
				for (int i = 0; i < _slotSize; i++)
				{
					curTokens = _tokens_PerSlots[i].Count;
					if(minNumTokens < 0 || curTokens < minNumTokens)
					{
						//가장 적은 토큰 개수를 가진 슬롯의 인덱스를 반영한다.
						minNumTokens = curTokens;
						optSlotIndex = i;
					}
				}
				return optSlotIndex;
			}


			// Get
			//연속으로 업데이트가 없었다면 토큰 리스트 자체를 삭제한다.
			public bool IsRemovable { get { return _unusedCount >  MAX_TOKENLIST_UNUSED_COUNT; } }


			// Debug Text
			//현재 상태를 출력하자
#if UNITY_EDITOR			
			public string GetDebugText()
			{	
				string result = "[" + _FPS + " ( " + _slotSize + " Slots ) ] : ";
				for (int i = 0; i < _slotSize; i++)
				{
					result += _tokens_PerSlots[i].Count + " ";
				}
				return result;
			}
#endif
		}


		// Members
		//---------------------------------------------------
		private static apOptUpdateChecker _instance = new apOptUpdateChecker();
		public static apOptUpdateChecker I { get { return _instance; } }

		private Dictionary<int, TokenList> _fps2Tokens = new Dictionary<int, TokenList>();

		private enum STATE
		{
			Ready, Update, LateUpdate
		}
		private STATE _state = STATE.Ready;

		//리뉴얼
		//게임의 FPS를 계산하자.
		//DeltaTime 기록 배열이 모두 찰때마다 평균값을 새로 계산한다.
		private const int INIT_GAME_FPS = 60;
		private const int NUM_FPS_RECORDS = 300;//60FPS 기준으로 5초마다 평균 계산
		
		//실제 게임의 FPS 범위
		private const int MAX_GAME_FPS = 150;
		private const int MIN_GAME_FPS = 15;

		private const float MAX_VALID_DELTA_TIME = 0.2f;//5 FPS보다 느리면 유효하지 않은 DeltaTime이다.

		private int _curGameFPS = INIT_GAME_FPS;
		private float[] _deltaTimeRecords = null;
		private int _iDeltaTimeRecord = 0;


		private List<TokenList> _removableTokenLists = new List<TokenList>();//삭제 처리를 위한 리스트. 임시용이다.

#if UNITY_EDITOR
		private List<string> _debugTexts = new List<string>();
#endif

		// Init
		//---------------------------------------------------
		private apOptUpdateChecker()
		{
			//초기화
			_fps2Tokens.Clear();
			_removableTokenLists.Clear();

			_state = STATE.Ready;

			InitFPSRecords();
		}


		// Functions
		//---------------------------------------------------
		//Renew
		//프레임 계산
		private void InitFPSRecords()
		{
			_curGameFPS = INIT_GAME_FPS;
			if(_deltaTimeRecords == null)
			{
				_deltaTimeRecords = new float[NUM_FPS_RECORDS];
			}
			for (int i = 0; i < NUM_FPS_RECORDS; i++)
			{
				_deltaTimeRecords[i] = 0.0f;
			}
			_iDeltaTimeRecord = 0;
		}

		/// <summary>
		/// Update에서 "첫 업데이트"시 DeltaTime을 받아서 기록하거나 현재 게임의 FPS를 계산한다.
		/// FPS가 갱신되면 true를 리턴한다.
		/// </summary>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		private bool CalculateFPSRecord(float deltaTime)
		{

			if(deltaTime > MAX_VALID_DELTA_TIME)
			{
				//너무 느린 프레임 (DeltaTime이 크다)이라면
				//Spike일 것이므로 생략한다.
				return false;
			}


			if(_iDeltaTimeRecord < NUM_FPS_RECORDS)
			{
				//기록을 하자
				_deltaTimeRecords[_iDeltaTimeRecord] = deltaTime;
				_iDeltaTimeRecord++;
				return false;
			}
			else
			{
				//평균을 내서 FPS를 갱신하자.
				float totalDeltaTime = 0.0f;
				for (int i = 0; i < NUM_FPS_RECORDS; i++)
				{
					totalDeltaTime += _deltaTimeRecords[i];//Delta Time을 저장하고
					//_deltaTimeRecords[i] = 0.0f;//기록은 초기화 > 안해도 된다.
				}
				_iDeltaTimeRecord = 0;

				//평균값으로 FPS 계산하기
				float avgDeltaTime = totalDeltaTime / NUM_FPS_RECORDS;
				int prevFPS = _curGameFPS;
				if(avgDeltaTime > 0.0f)
				{
					_curGameFPS = (int)(1.0f / avgDeltaTime);
				}

				return _curGameFPS != prevFPS;//값이 바뀌었다면 true 리턴
			}
		}



		//Update에서 호출하는 함수와
		//LateUpdate에서 호출하는 함수 2개로 나뉜다.
		/// <summary>
		/// 이 함수를 Update에서 호출하자
		/// 토큰이 없다면 null로 하되, 리턴값을 멤버로 가지고 있자
		/// </summary>
		/// <param name="token"></param>
		/// <param name="fps"></param>
		/// <returns></returns>
		public UpdateToken AddRequest(UpdateToken token, int fps, float deltaTime)
		{
			//상태가 바뀌면 초기화를 해야한다.
			if(_state != STATE.Update)
			{
//#if UNITY_EDITOR
//				Profiler.BeginSample("AddRequest > Reset");
//#endif
				//_fps2Tokens.Clear();
				foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
				{
					//keyValuePair.Value.Reset(deltaTime);
					keyValuePair.Value.ReadyToUpdate();
				}

				_state = STATE.Update;


				//리뉴얼 : FPS를 계산한다.
				bool isFPSChanged = CalculateFPSRecord(deltaTime);
				if(isFPSChanged)
				{
					//FPS가 바뀌었다면 토큰 리스트의 슬롯 개수를 변경해야한다.
					foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
					{
						keyValuePair.Value.SetGameFPS(_curGameFPS);
					}
				}

//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
			}

			#region [미사용 코드 - 이전 로직]
			//fps = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);

			//if(token == null)
			//{
			//	token = new UpdateToken(fps);
			//}
			//else
			//{
			//	token.SetFPS(fps);
			//}
			//token.UpdateTime(deltaTime);

			//token.ReadyToCalculate();

			//if (token.IsUpdatable())
			//{
			//	//업데이트될 수 있다면 토큰을 리스트에 넣자
			//	if (_fps2Tokens.ContainsKey(fps))
			//	{
			//		_fps2Tokens[fps].AddRequest(token);
			//	}
			//	else
			//	{
			//		TokenList newTokenList = new TokenList(fps);

			//		newTokenList.Reset(deltaTime);
			//		_fps2Tokens.Add(fps, newTokenList);
			//		newTokenList.AddRequest(token);

			//		//Debug.Log("New Token List : " + fps);
			//	}
			//} 
			#endregion

			int tokenFPS = Mathf.Clamp(fps, MIN_TOKEN_FPS, MAX_TOKEN_FPS);

			if(token == null)
			{
				token = new UpdateToken(tokenFPS);
			}
			else
			{
				bool isChanged = token.SetFPS(tokenFPS);
				//이미 만들어진 토큰이다.
				//만약 FPS가 바뀌었다면, 기존에 슬롯에 저장되었다면, 해당 슬롯에서는 제거하자.
				if(isChanged && token.ParentTokenList != null)
				{
					//슬롯에서 제거하기 + 개수 변경하기
					token.ParentTokenList.RemoveToken(token);
				}
			}

			//입력될 토큰 리스트를 찾자
			
			if(_fps2Tokens.ContainsKey(tokenFPS))
			{
				_fps2Tokens[tokenFPS].AddAndUpdateToken(token);
			}
			else
			{
				//새로운 슬롯을 생성
				TokenList newTokenList = new TokenList(tokenFPS);
				newTokenList.SetGameFPS(_curGameFPS);//현재 기준으로 초기 크기도 지정해야한다.
				
				newTokenList.AddAndUpdateToken(token);

				_fps2Tokens.Add(tokenFPS, newTokenList);
			}

			//시간도 업데이트 해주자
			token.Update(deltaTime);

			return token;
		}

		/// <summary>
		/// 이 함수를 LateUpdate에서 호출하자. True면 업데이트 할 수 있다.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool GetUpdatable(UpdateToken token)
		{
			if(token == null)
			{
				return false;
			}
			if(_state != STATE.LateUpdate)
			{
//#if UNITY_EDITOR
//				Profiler.BeginSample("Calculate > Reset");
//#endif
				//LateUpdate의 첫 처리


				#region [미사용 코드]
				////_fps2Tokens.Clear();
				//bool isAnyRemovableList = false;
				//List<int> removalbeFPS = null;
				//foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
				//{
				//	if(keyValuePair.Value.Calculate())
				//	{
				//		//삭제할 게 있다.
				//		isAnyRemovableList = true;
				//		if(removalbeFPS == null)
				//		{
				//			removalbeFPS = new List<int>();
				//		}
				//		removalbeFPS.Add(keyValuePair.Key);
				//	}
				//}

				////삭제해야할 때도 있다.
				//if(isAnyRemovableList)
				//{
				//	for (int i = 0; i < removalbeFPS.Count; i++)
				//	{
				//		//Debug.Log("Token List 삭제 : " + removalbeFPS[i]);
				//		_fps2Tokens.Remove(removalbeFPS[i]);
				//	}
				//} 
				#endregion

				//Late Update의 첫 프레임에서는 "유효하지 않은 토큰"이나 "토큰 리스트"들을 삭제해야 한다.
				TokenList curTokenList = null;
				_removableTokenLists.Clear();
				bool isAnyRemoveTokenList = false;
				foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
				{
					curTokenList = keyValuePair.Value;
					if(curTokenList.IsRemovable)
					{
						//토큰 리스트가 삭제되어야 한다면
						isAnyRemoveTokenList = true;
						_removableTokenLists.Add(curTokenList);
					}
					else
					{
						//<중요!>
						//그렇지 않다면 토큰 리스트 내부의 유효하지 않은 토큰들을 찾아서 삭제하자
						//이 함수를 호출하면 토큰들의 "업데이트 여부"도 결정된다.
						curTokenList.UpdateCursorAndRemoveInvalidTokens();
					}
				}

				if(isAnyRemoveTokenList)
				{
					//삭제할 토큰리스트는 삭제하자
					int nRemovedList = _removableTokenLists.Count;
					for (int i = 0; i < _removableTokenLists.Count; i++)
					{
						_fps2Tokens.Remove(_removableTokenLists[i]._FPS);
					}

					_removableTokenLists.Clear();

					//Debug.LogError("Remove Token List : " + nRemovedList);;
				}

				_state = STATE.LateUpdate;

//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
			}

			return token.IsUpdatable;
		}


		// Get / Set
		//---------------------------------------------------
#if UNITY_EDITOR
		public List<string> GetDebugTexts()
		{
			if (_debugTexts == null)
			{
				_debugTexts = new List<string>();
			}
			_debugTexts.Clear();

			if (_fps2Tokens == null)
			{
				return _debugTexts;
			}

			_debugTexts.Add("Token Lists : " + _fps2Tokens.Count);

			TokenList curTokenList = null;
			foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
			{
				curTokenList = keyValuePair.Value;
				_debugTexts.Add(curTokenList.GetDebugText());
			}

			return _debugTexts;
		}
#endif

	}
}