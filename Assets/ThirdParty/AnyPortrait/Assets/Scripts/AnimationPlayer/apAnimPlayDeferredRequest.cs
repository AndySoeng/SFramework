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
	/// 추가 22.5.17 [v1.4.0]
	/// 애니메이션 재생에 관련된 요청시, 한 프레임에 여러가지 요청이 온다면, 잘 정리할 필요가 있다.
	/// 그래서 스크립트로 애니메이션 재생 관련 함수가 들어올 때 바로 처리하지 않고, 일단 여기에 저장을 해둔다.
	/// 이후 일괄적으로 처리를 하면서 무시될 수 있는 요청들을 삭제하고 필요한 요청만 실제로 반영한다.
	/// (다중 루트 유닛에 대해서 모순된 Play 함수 호출시의 버그 해결)
	/// </summary>
	public class apAnimPlayDeferredRequest
	{
		// Members
		//------------------------------------------------------------
		public enum COMMAND_TYPE
		{
			Play,
			PlayQueued,
			CrossFade,
			CrossFadeQueued,
			PlayAt,
			PlayQueuedAt,
			CrossFadeAt,
			CrossFadeQueuedAt,
			StopLayer,
			StopAll,
			PauseLayer,
			PauseAll,
			ResumeLayer,
			ResumeAll,
		}

		//스크립트로 요청했던 것을 유닛 단위로 만들자
		//배열로 만들어서 빠른 큐로 만들자
		public class CommandUnit
		{
			public int _index = 0;//이건 커맨드 인덱스. 배열에 속하므로, 
			public COMMAND_TYPE _commandType = COMMAND_TYPE.Play;
			public bool _isValid = false;

			//Play 계열인 경우
			public apAnimPlayData _playData = null;
			public int _layer = 0;//Stop/Pause/Resume에서도 사용
			public apAnimPlayUnit.BLEND_METHOD _blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation;
			public apAnimPlayManager.PLAY_OPTION _playOption = apAnimPlayManager.PLAY_OPTION.StopSameLayer;
			public bool _isAutoEndIfNotloop = false;
			public float _fadeTime = 0.0f;//CrossFade / Stop
			public int _frame = 0;//At

			public CommandUnit(int index)
			{
				_index = index;
				_isValid = false;
			}

			// 각 스크립트 함수별로 만들자
			public void Play(	apAnimPlayData playData, 
								int layer, 
								apAnimPlayUnit.BLEND_METHOD blendMethod, 
								apAnimPlayManager.PLAY_OPTION playOption,
								bool isAutoEndIfNotloop = false)
			{
				_commandType = COMMAND_TYPE.Play;
				_isValid = true;

				_playData = playData;
				_layer = layer;
				_blendMethod = blendMethod;
				_playOption = playOption;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}

			public void PlayQueued(	apAnimPlayData playData,
									int layer,
									apAnimPlayUnit.BLEND_METHOD blendMethod,
									bool isAutoEndIfNotloop)
			{
				_commandType = COMMAND_TYPE.PlayQueued;
				_isValid = true;

				_playData = playData;
				_layer = layer;
				_blendMethod = blendMethod;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}


			public void CrossFade(	apAnimPlayData playData,
									int layer,
									apAnimPlayUnit.BLEND_METHOD blendMethod,
									float fadeTime, 
									apAnimPlayManager.PLAY_OPTION playOption,
									bool isAutoEndIfNotloop)
			{
				_commandType = COMMAND_TYPE.CrossFade;
				_isValid = true;

				_playData = playData;
				_layer = layer;
				_blendMethod = blendMethod;
				_fadeTime = fadeTime;
				_playOption = playOption;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}


			public void CrossFadeQueued(	apAnimPlayData playData,
											int layer,
											apAnimPlayUnit.BLEND_METHOD blendMethod,
											float fadeTime,
											bool isAutoEndIfNotloop)
			{
				_commandType = COMMAND_TYPE.CrossFadeQueued;
				_isValid = true;

				_playData = playData;
				_layer = layer;
				_blendMethod = blendMethod;
				_fadeTime = fadeTime;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}

			public void PlayAt(	apAnimPlayData playData,
								int frame,
								int layer,
								apAnimPlayUnit.BLEND_METHOD blendMethod,
								apAnimPlayManager.PLAY_OPTION playOption,
								bool isAutoEndIfNotloop)
			{
				_commandType = COMMAND_TYPE.PlayAt;
				_isValid = true;

				_playData = playData;
				_frame = frame;
				_layer = layer;
				_blendMethod = blendMethod;
				_playOption = playOption;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}


			public void PlayQueuedAt(	apAnimPlayData playData,
										int frame,
										int layer,
										apAnimPlayUnit.BLEND_METHOD blendMethod,
										bool isAutoEndIfNotloop)
			{
				_commandType = COMMAND_TYPE.PlayQueuedAt;
				_isValid = true;

				_playData = playData;
				_frame = frame;
				_layer = layer;
				_blendMethod = blendMethod;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}

			public void CrossFadeAt(	apAnimPlayData playData,
										int frame,
										int layer,
										apAnimPlayUnit.BLEND_METHOD blendMethod,
										float fadeTime,
										apAnimPlayManager.PLAY_OPTION playOption,
										bool isAutoEndIfNotloop)
			{
				_commandType = COMMAND_TYPE.CrossFadeAt;
				_isValid = true;

				_playData = playData;
				_frame = frame;
				_layer = layer;
				_blendMethod = blendMethod;
				_fadeTime = fadeTime;
				_playOption = playOption;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}


			public void CrossFadeQueuedAt(	apAnimPlayData playData,
											int frame,
											int layer,
											apAnimPlayUnit.BLEND_METHOD blendMethod,
											float fadeTime,
											bool isAutoEndIfNotloop)
			{
				_commandType = COMMAND_TYPE.CrossFadeQueuedAt;
				_isValid = true;

				_playData = playData;
				_frame = frame;
				_layer = layer;
				_blendMethod = blendMethod;
				_fadeTime = fadeTime;
				_isAutoEndIfNotloop = isAutoEndIfNotloop;
			}


			public void StopLayer(int layer, float fadeTime)
			{
				_commandType = COMMAND_TYPE.StopLayer;
				_isValid = true;

				_layer = layer;
				_fadeTime = fadeTime;
			}

			public void StopAll(float fadeTime)
			{
				_commandType = COMMAND_TYPE.StopAll;
				_isValid = true;

				_fadeTime = fadeTime;
			}

			public void PauseLayer(int layer)
			{
				_commandType = COMMAND_TYPE.PauseLayer;
				_isValid = true;

				_layer = layer;
			}

			public void PauseAll()
			{
				_commandType = COMMAND_TYPE.PauseAll;
				_isValid = true;
			}

			public void ResumeLayer(int layer)
			{
				_commandType = COMMAND_TYPE.ResumeLayer;
				_isValid = true;

				_layer = layer;
			}

			public void ResumeAll()
			{
				_commandType = COMMAND_TYPE.ResumeAll;
				_isValid = true;
			}
		}

		//배열로 만들어서 관리하자
		private const int INIT_COMMANDS = 20;
		private const int ADD_OVERFLOW_COMMANDS = 10;
		private int _nCommands = 0;
		private int _iCommand = 0;

		

		//배열로 만들자
		private CommandUnit[] _commands = null;
		private bool _isInit = false;

		private CommandUnit _curCmd = null;
		private apAnimPlayManager _animPlayManager = null;

		// Init
		//-----------------------------------------------------------
		public apAnimPlayDeferredRequest(apAnimPlayManager playManager)
		{
			_animPlayManager = playManager;

			if(!_isInit)
			{
				Init();
			}

			Ready();
		}

		public void Init()
		{
			if(_isInit)
			{
				return;
			}

			_commands = new CommandUnit[INIT_COMMANDS];
			_nCommands = INIT_COMMANDS;

			for (int i = 0; i < INIT_COMMANDS; i++)
			{
				_commands[i] = new CommandUnit(i);
			}

			_iCommand = 0;
			_isInit = true;
		}

		public void Ready()
		{
			//커서를 처음으로 돌린다.
			_iCommand = 0;
		}

		private CommandUnit GetNextCommand()
		{
			if(_iCommand >= _nCommands)
			{
				//배열이 꽉 찾다면 > 증가시키자
				CommandUnit[] prevCmds = _commands;
				int nPrev = _commands.Length;
				_nCommands += ADD_OVERFLOW_COMMANDS;//개수 증가
				_commands = new CommandUnit[_nCommands];//배열 새로 생성

				for (int i = 0; i < nPrev; i++)
				{
					//복사
					_commands[i] = prevCmds[i];
				}

				for (int i = nPrev; i < _nCommands; i++)
				{
					//새로 생성
					_commands[i] = new CommandUnit(i);
				}
			}
			//Debug.Log("Command : " + _iCommand);

			CommandUnit nextCmd = _commands[_iCommand];
			_iCommand += 1;

			return nextCmd;
		}


		// 애니메이션 재생 함수들
		//----------------------------------------------------------------
		public void Play(	apAnimPlayData playData, 
							int layer, 
							apAnimPlayUnit.BLEND_METHOD blendMethod, 
							apAnimPlayManager.PLAY_OPTION playOption,
							bool isAutoEndIfNotloop)
		{
			GetNextCommand().Play(playData, layer, blendMethod, playOption, isAutoEndIfNotloop);
		}

		public void PlayQueued(	apAnimPlayData playData,
								int layer,
								apAnimPlayUnit.BLEND_METHOD blendMethod,
								bool isAutoEndIfNotloop)
		{
			GetNextCommand().PlayQueued(playData, layer, blendMethod, isAutoEndIfNotloop);
		}


		public void CrossFade(	apAnimPlayData playData,
								int layer,
								apAnimPlayUnit.BLEND_METHOD blendMethod,
								float fadeTime, 
								apAnimPlayManager.PLAY_OPTION playOption,
								bool isAutoEndIfNotloop)
		{
			GetNextCommand().CrossFade(playData, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}


		public void CrossFadeQueued(	apAnimPlayData playData,
										int layer,
										apAnimPlayUnit.BLEND_METHOD blendMethod,
										float fadeTime,
										bool isAutoEndIfNotloop)
		{
			GetNextCommand().CrossFadeQueued(playData, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}

		public void PlayAt(	apAnimPlayData playData,
							int frame,
							int layer,
							apAnimPlayUnit.BLEND_METHOD blendMethod,
							apAnimPlayManager.PLAY_OPTION playOption,
							bool isAutoEndIfNotloop)
		{
			GetNextCommand().PlayAt(playData, frame, layer, blendMethod, playOption, isAutoEndIfNotloop);
		}


		public void PlayQueuedAt(	apAnimPlayData playData,
									int frame,
									int layer,
									apAnimPlayUnit.BLEND_METHOD blendMethod,
									bool isAutoEndIfNotloop)
		{
			GetNextCommand().PlayQueuedAt(playData, frame, layer, blendMethod, isAutoEndIfNotloop);
		}

		public void CrossFadeAt(	apAnimPlayData playData,
									int frame,
									int layer,
									apAnimPlayUnit.BLEND_METHOD blendMethod,
									float fadeTime,
									apAnimPlayManager.PLAY_OPTION playOption,
									bool isAutoEndIfNotloop)
		{
			GetNextCommand().CrossFadeAt(playData, frame, layer, blendMethod, fadeTime, playOption, isAutoEndIfNotloop);
		}


		public void CrossFadeQueuedAt(	apAnimPlayData playData,
										int frame,
										int layer,
										apAnimPlayUnit.BLEND_METHOD blendMethod,
										float fadeTime,
										bool isAutoEndIfNotloop)
		{
			GetNextCommand().CrossFadeQueuedAt(playData, frame, layer, blendMethod, fadeTime, isAutoEndIfNotloop);
		}


		public void StopLayer(int layer, float fadeTime)
		{
			GetNextCommand().StopLayer(layer, fadeTime);
		}

		public void StopAll(float fadeTime)
		{
			GetNextCommand().StopAll(fadeTime);
		}

		public void PauseLayer(int layer)
		{
			GetNextCommand().PauseLayer(layer);
		}

		public void PauseAll()
		{
			GetNextCommand().PauseAll();
		}

		public void ResumeLayer(int layer)
		{
			GetNextCommand().ResumeLayer(layer);
		}

		public void ResumeAll()
		{
			GetNextCommand().ResumeAll();
		}


		// 유효한 Command만 남기기
		//----------------------------------------------------------------
		public bool IsAnyRequests()
		{
			return _iCommand > 0;
		}

		
		public void ProcessRequests()
		{
			//1. 유효성 테스트
			Validate();

			//2. AnimPlayManager에 함수 전달. (순서대로)
			ExecuteCommands();

			//2. 종료
			Ready();

		}

		private void Validate()
		{
			if(_iCommand <= 0)
			{
				//요청이 없다.
				return;
			}

			//마지막 요청부터 거꾸로 체크한다.
			//- 마지막 Play 계열의 RootUnit을 체크하고, 그것과 다른 이전의 요청들은 무시한다.
			//- Queued의 경우, 가장 마지막에 플레이하는 애니메이션이 없거나 Loop라면 일반 Play로 간주하고 처리한다.

			_curCmd = null;
			apOptRootUnit requestedLastRootUnit = null;

			//Queued가 Play로 변환되는 조건
			//- 앞에 Play 계열의 요청이 있었어야 한다. (없으면 안됨)
			//- 마지막 Play 요청의 애니메이션이 루프여야 한다.
			//- Queued가 Play로 바뀐다.

			for (int i = _iCommand - 1; i >= 0; i--)
			{
				_curCmd = _commands[i];
				if (!_curCmd._isValid)
				{
					//이미 비활성화되었다.
					continue;
				}

				switch (_curCmd._commandType)
				{
					case COMMAND_TYPE.Play:
					case COMMAND_TYPE.CrossFade:
					case COMMAND_TYPE.PlayAt:
					case COMMAND_TYPE.CrossFadeAt:
						{
							//일반 Play 계열
							if(requestedLastRootUnit == null)
							{
								//Root Unit이 없다면 할당
								requestedLastRootUnit = _curCmd._playData._linkedOptRootUnit;
							}
							else if(requestedLastRootUnit != _curCmd._playData._linkedOptRootUnit)
							{
								//Root Unit이 다르다면 비활성화
								//이게 v1.4.0에서 고쳐진 버그
								//Debug.LogError("Root Unit 전환이 이미 될 예정이어서 요청 무시됨 [ " + _curCmd._playData.Name + " ]");
								_curCmd._isValid = false;
							}
						}
						break;

					case COMMAND_TYPE.PlayQueued:
					case COMMAND_TYPE.CrossFadeQueued:					
					case COMMAND_TYPE.PlayQueuedAt:					
					case COMMAND_TYPE.CrossFadeQueuedAt:
						{
							//Queued 계열
							//일반적으로는 대기되는 요청이므로 루트 유닛을 체크하진 않는다.
							//다음의 조건에서는 Play 함수처럼 사용되므로, 루트 유닛을 체크한다.
							//(스크립트 호출과 무관하게 현재 AnimPlayManager에서 플레이 중인 유닛들은 상관없다.)
							//- 
							//- 현재 플레이되는 애니메이션이 없을때
							//- 
						}
						break;

					case COMMAND_TYPE.StopLayer:
					case COMMAND_TYPE.StopAll:
					case COMMAND_TYPE.PauseLayer:
					case COMMAND_TYPE.PauseAll:
					case COMMAND_TYPE.ResumeLayer:
					case COMMAND_TYPE.ResumeAll:
						break;
				}
			}
		}


		private void ExecuteCommands()
		{
			_curCmd = null;

			for (int i = 0; i < _iCommand; i++)
			{
				_curCmd = _commands[i];
				if (!_curCmd._isValid)
				{
					//비활성화되었다.
					continue;
				}

				//각 커맨드대로 적용하자
				switch (_curCmd._commandType)
				{
					case COMMAND_TYPE.Play:
						{
							_animPlayManager.Play(	_curCmd._playData,
													_curCmd._layer,
													_curCmd._blendMethod,
													_curCmd._playOption,
													_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.CrossFade:
						{
							_animPlayManager.CrossFade(	_curCmd._playData,
														_curCmd._layer,
														_curCmd._blendMethod,
														_curCmd._fadeTime,
														_curCmd._playOption,
														_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.PlayAt:
						{
							_animPlayManager.PlayAt(	_curCmd._playData,
														_curCmd._frame,
														_curCmd._layer,
														_curCmd._blendMethod,
														_curCmd._playOption,
														_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.CrossFadeAt:
						{
							_animPlayManager.CrossFadeAt(	_curCmd._playData,
															_curCmd._frame,
															_curCmd._layer,
															_curCmd._blendMethod,
															_curCmd._fadeTime,
															_curCmd._playOption,
															_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.PlayQueued:
						{
							_animPlayManager.PlayQueued(	_curCmd._playData,
															_curCmd._layer,
															_curCmd._blendMethod,
															_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.CrossFadeQueued:
						{
							_animPlayManager.CrossFadeQueued(	_curCmd._playData,
																_curCmd._layer,
																_curCmd._blendMethod,
																_curCmd._fadeTime,
																_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.PlayQueuedAt:
						{
							_animPlayManager.PlayQueuedAt(	_curCmd._playData,
															_curCmd._frame,
															_curCmd._layer,
															_curCmd._blendMethod,
															_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.CrossFadeQueuedAt:
						{
							_animPlayManager.CrossFadeQueuedAt(	_curCmd._playData,
																_curCmd._frame,
																_curCmd._layer,
																_curCmd._blendMethod,
																_curCmd._fadeTime,
																_curCmd._isAutoEndIfNotloop);
						}
						break;

					case COMMAND_TYPE.StopLayer:
						{
							_animPlayManager.StopLayer(_curCmd._layer, _curCmd._fadeTime);
						}
						break;

					case COMMAND_TYPE.StopAll:
						{
							_animPlayManager.StopAll(_curCmd._fadeTime);
						}
						break;

					case COMMAND_TYPE.PauseLayer:
						{
							_animPlayManager.PauseLayer(_curCmd._layer);
						}
						break;

					case COMMAND_TYPE.PauseAll:
						{
							_animPlayManager.PauseAll();
						}
						break;

					case COMMAND_TYPE.ResumeLayer:
						{
							_animPlayManager.ResumeLayer(_curCmd._layer);
						}
						break;

					case COMMAND_TYPE.ResumeAll:
						{
							_animPlayManager.ResumeAll();
						}
						break;
				}
			}
		}

	}
}