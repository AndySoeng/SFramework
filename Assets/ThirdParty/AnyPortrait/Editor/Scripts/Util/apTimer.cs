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
using System.Diagnostics;
using System;

using AnyPortrait;
using UnityEditor;

namespace AnyPortrait
{

	public class apTimer
	{
		// SingleTone
		//----------------------------------------
		private static apTimer _instance = new apTimer();
		public static apTimer I
		{
			get
			{
				return _instance;
			}
		}

		// Members
		//----------------------------------------

		public enum TIMER_TYPE
		{
			Update = 0,//최대 60FPS를 지원하는 동기화된 프레임의 시간 [동기화 처리를 한다.]
			UpdateAllFrame = 1,//모든 Update 프레임 시간을 계산하여 매번 연산된 시간
			Repaint = 2//Repaint 이벤트마다 계산된 시간
		}

		private const int NUM_TIME_TYPE = 3;
		private const int UPDATE = 0;
		private const int UPDATE_ALL_FRAME = 1;
		private const int REPAINT = 2;

		//이전 : Stopwatch 이용. 이건 심플한 방식으로 바꾸자
		
		//추가 21.7.17 : EditorApplication.timeSinceStartup를 이용해서 시간을 측정할 수 있다.
		private double [] _prevEditorTime = new double[NUM_TIME_TYPE];
		private double[] _deltaTimeCount2 = new double[NUM_TIME_TYPE];
		private double[] _lastDeltaTime2 = new double[NUM_TIME_TYPE];

		
		//private const long MIN_UPDATE_DELTA_TIME = 16;//(16)60FPS보다 작으면 강제 업데이트 갱신을 막아야 한다.
		private const long MIN_UPDATE_DELTA_TIME = 8;//변경 21.7.17. 기준 FPS를 60에서 120으로 증가

		private const long MIN_UPDATE_DELTA_TIME_LOWCPU_LOW = 500;//(500 = 0.5초) LowCPU_Low 모드에서는 2FPS 기준으로 동작
		private const long MIN_UPDATE_DELTA_TIME_LOWCPU_MID = 50;//(50 = 0.05초) LowCPU_Mid 모드에서는 20FPS 기준으로 동작

		//포커스가 이탈하여 에디터가 멈추면, 그동안 시간이 누적되어 DeltaTime이 크게 증가한다.
		//Max값을 정해야한다.
		private const double MAX_DELTA_TIME = 1.0;//1초가 넘으면 에디터가 멈춘 것으로 판단.
		private const double MAX_DELTA_TIME_LOWCPU_LOW = 2.0;//LOW > 2초
		private const double MAX_DELTA_TIME_LOWCPU_MID = 2.0;//MID > 2초

		private bool _isValidFrame2 = false;

		private bool _isLostFocus = false;

		

		// Init
		//----------------------------------------
		private apTimer()
		{

			#region [미사용 코드]
			//if(_stopWatch == null || _stopWatch.Length != NUM_TIME_TYPE)				{ _stopWatch = new Stopwatch[NUM_TIME_TYPE]; }			
			//if(_deltaTimeCount == null || _deltaTimeCount.Length != NUM_TIME_TYPE)		{ _deltaTimeCount = new long[NUM_TIME_TYPE]; }
			//if(_prevDeltaTime == null || _prevDeltaTime.Length != NUM_TIME_TYPE)		{ _prevDeltaTime = new double[NUM_TIME_TYPE]; }			 
			#endregion

			//추가 21.7.17 : 에디터 API를 이용한다.
			if(_prevEditorTime == null || _prevEditorTime.Length != NUM_TIME_TYPE)		{ _prevEditorTime = new double[NUM_TIME_TYPE]; }
			if(_deltaTimeCount2 == null || _deltaTimeCount2.Length != NUM_TIME_TYPE)	{ _deltaTimeCount2 = new double[NUM_TIME_TYPE]; }
			if(_lastDeltaTime2 == null || _lastDeltaTime2.Length != NUM_TIME_TYPE)		{ _lastDeltaTime2 = new double[NUM_TIME_TYPE]; }


			for (int i = 0; i < NUM_TIME_TYPE; i++)
			{
				#region [미사용 코드]
				//_stopWatch[i] = new Stopwatch();
				//_stopWatch[i].Start();
				//_deltaTimeCount[i] = 0;
				//_prevDeltaTime[i] = 0.0; 
				#endregion

				//추가 21.7.17
				_prevEditorTime[i] = EditorApplication.timeSinceStartup;
				_deltaTimeCount2[i] = 0;
				_lastDeltaTime2[i] = 0;

				//_subTimer[i] = new SubTimer();
			}

			_isLostFocus = false;
			//_fps = 0;
		}

		private void ResetTime()
		{
			if(_prevEditorTime == null || _prevEditorTime.Length != NUM_TIME_TYPE)		{ _prevEditorTime = new double[NUM_TIME_TYPE]; }
			if(_deltaTimeCount2 == null || _deltaTimeCount2.Length != NUM_TIME_TYPE)	{ _deltaTimeCount2 = new double[NUM_TIME_TYPE]; }
			if(_lastDeltaTime2 == null || _lastDeltaTime2.Length != NUM_TIME_TYPE)		{ _lastDeltaTime2 = new double[NUM_TIME_TYPE]; }


			for (int i = 0; i < NUM_TIME_TYPE; i++)
			{
				_prevEditorTime[i] = EditorApplication.timeSinceStartup;
				_deltaTimeCount2[i] = 0;
				_lastDeltaTime2[i] = 0;
			}
		}

		// Functions
		//----------------------------------------
		//윈도우가 포커스를 잃었다.
		public void OnLostFocus()
		{
			_isLostFocus = true;
		}

		//윈도우가 포커스를 회복했다.
		public void OnRecoverFocus()
		{
			if(!_isLostFocus)
			{
				return;
			}

			//포커스를 잃은 적이 있다면
			//모든 Delta Time을 리셋해야한다.
			_isLostFocus = false;

			ResetTime();
		}


		public bool CheckTime_Update(apEditor.LOW_CPU_STATUS lowCPUStatus)
		{
			_isValidFrame2 = false;

			//이전 코드
			#region [미사용 코드]
			//_stopWatch[UPDATE].Stop();
			//_stopWatch[UPDATE_ALL_FRAME].Stop();

			//long deltaTime_Update = _stopWatch[UPDATE].ElapsedMilliseconds;
			//long deltaTime_UpdateAllFrame = _stopWatch[UPDATE_ALL_FRAME].ElapsedMilliseconds;

			////Sub Time도 계산
			//_subTimer[UPDATE].UpdateTime(deltaTime_Update);
			//_subTimer[UPDATE_ALL_FRAME].UpdateTime(deltaTime_UpdateAllFrame);


			//_deltaTimeCount[UPDATE] += deltaTime_Update;
			//_deltaTimeCount[UPDATE_ALL_FRAME] += deltaTime_UpdateAllFrame; 
			#endregion


			//추가 21.7.17 : 에디터의 시간 이용			
			_deltaTimeCount2[UPDATE] += (EditorApplication.timeSinceStartup - _prevEditorTime[UPDATE]);
			_prevEditorTime[UPDATE] = EditorApplication.timeSinceStartup;


			//Update는 60FPS -> 120FPS 보다 높으면(해당 시간보다 낮으면) 프레임 스킵을 해야한다.
			//그 적당한 시간이 지난 경우에 ValidFrame 설정

			if(lowCPUStatus == apEditor.LOW_CPU_STATUS.LowCPU_Low)
			{
				//약 2FPS (0.5초)
				//이전 방식
				#region [미사용 코드]
				//if (_deltaTimeCount[UPDATE] > MIN_UPDATE_DELTA_TIME_LOWCPU_LOW)
				//{
				//	_prevDeltaTime[UPDATE] = (_deltaTimeCount[UPDATE] / 1000.0);
				//	_deltaTimeCount[UPDATE] = 0;
				//	_isValidFrame = true;
				//} 
				#endregion

				//추가 21.7.17 : 에디터 시간
				if((long)(_deltaTimeCount2[UPDATE] * 1000.0) > MIN_UPDATE_DELTA_TIME_LOWCPU_LOW)
				{
					_lastDeltaTime2[UPDATE] = _deltaTimeCount2[UPDATE];
					if(_lastDeltaTime2[UPDATE] > MAX_DELTA_TIME_LOWCPU_LOW)
					{
						_lastDeltaTime2[UPDATE] = MAX_DELTA_TIME_LOWCPU_LOW;
					}
					_deltaTimeCount2[UPDATE] = 0.0;
					_isValidFrame2 = true;
				}
			}
			else if(lowCPUStatus == apEditor.LOW_CPU_STATUS.LowCPU_Mid)
			{
				//약 20FPS
				//이전 코드
				#region [미사용 코드]
				//if (_deltaTimeCount[UPDATE] > MIN_UPDATE_DELTA_TIME_LOWCPU_MID)
				//{
				//	_prevDeltaTime[UPDATE] = (_deltaTimeCount[UPDATE] / 1000.0);
				//	_deltaTimeCount[UPDATE] = 0;
				//	_isValidFrame = true;
				//} 
				#endregion

				//추가 21.7.17 : 에디터 시간
				if((long)(_deltaTimeCount2[UPDATE] * 1000.0) > MIN_UPDATE_DELTA_TIME_LOWCPU_MID)
				{
					_lastDeltaTime2[UPDATE] = _deltaTimeCount2[UPDATE];
					if(_lastDeltaTime2[UPDATE] > MAX_DELTA_TIME_LOWCPU_MID)
					{
						_lastDeltaTime2[UPDATE] = MAX_DELTA_TIME_LOWCPU_MID;
					}
					_deltaTimeCount2[UPDATE] = 0.0;
					_isValidFrame2 = true;
				}
			}
			else
			{
				//정상적인 FPS
				//이전 코드
				#region [미사용 코드]
				//if (_deltaTimeCount[UPDATE] > MIN_UPDATE_DELTA_TIME)
				//{
				//	_prevDeltaTime[UPDATE] = (_deltaTimeCount[UPDATE] / 1000.0);
				//	_deltaTimeCount[UPDATE] = 0;
				//	_isValidFrame = true;
				//} 
				#endregion

				//추가 21.7.17 : 에디터 시간
				if((long)(_deltaTimeCount2[UPDATE] * 1000.0) > MIN_UPDATE_DELTA_TIME)
				{
					_lastDeltaTime2[UPDATE] = _deltaTimeCount2[UPDATE];
					if(_lastDeltaTime2[UPDATE] > MAX_DELTA_TIME)
					{
						_lastDeltaTime2[UPDATE] = MAX_DELTA_TIME;
					}
					_deltaTimeCount2[UPDATE] = 0.0;
					_isValidFrame2 = true;
				}
			}



			//이전 코드
			#region [미사용 코드]
			////Update All Frame은 프레임 스킵 없이 매번 경과 시간을 리턴한다.
			//_prevDeltaTime[UPDATE_ALL_FRAME] = (_deltaTimeCount[UPDATE_ALL_FRAME] / 1000.0);
			//_deltaTimeCount[UPDATE_ALL_FRAME] = 0;


			//_stopWatch[UPDATE].Reset();
			//_stopWatch[UPDATE].Start();

			//_stopWatch[UPDATE_ALL_FRAME].Reset();
			//_stopWatch[UPDATE_ALL_FRAME].Start(); 
			#endregion

			//추가 21.7.17
			//Update All Frame은 프레임 스킵 없이 매번 경과 시간을 리턴한다.
			_deltaTimeCount2[UPDATE_ALL_FRAME] += (EditorApplication.timeSinceStartup - _prevEditorTime[UPDATE_ALL_FRAME]);
			_prevEditorTime[UPDATE_ALL_FRAME] = EditorApplication.timeSinceStartup;
			_lastDeltaTime2[UPDATE_ALL_FRAME] = _deltaTimeCount2[UPDATE_ALL_FRAME];
			_deltaTimeCount2[UPDATE_ALL_FRAME] = 0.0;

			//return _isValidFrame;
			return _isValidFrame2;
		}

		public void CheckTime_Repaint(apEditor.LOW_CPU_STATUS lowCPUStatus)
		{
			//if(apVersion.I.IsDemoViolation)
			//{
			//	//재생할 수 없다.
			//	return;
			//}

			//Repaint 상에서의 경과 시간을 리턴한다.
			//이전 코드
			#region [미사용 코드]
			//_stopWatch[REPAINT].Stop();

			//long deltaTime_Repaint = _stopWatch[REPAINT].ElapsedMilliseconds;

			////SubTimer 갱신
			//_subTimer[REPAINT].UpdateTime(deltaTime_Repaint);

			//_deltaTimeCount[REPAINT] = deltaTime_Repaint;
			//_prevDeltaTime[REPAINT] = (_deltaTimeCount[REPAINT] / 1000.0f); 
			#endregion


			//추가 21.7.17 : 에디터 API
			_deltaTimeCount2[REPAINT] += (EditorApplication.timeSinceStartup - _prevEditorTime[REPAINT]);
			_prevEditorTime[REPAINT] = EditorApplication.timeSinceStartup;

			_lastDeltaTime2[REPAINT] = _deltaTimeCount2[REPAINT];

			if(_lastDeltaTime2[REPAINT] > MAX_DELTA_TIME)
			{
				_lastDeltaTime2[REPAINT] = MAX_DELTA_TIME;
			}


			_deltaTimeCount2[REPAINT] = 0.0;



			#region [미사용 코드]
			//if (lowCPUStatus == apEditor.LOW_CPU_STATUS.LowCPU_Low || lowCPUStatus == apEditor.LOW_CPU_STATUS.LowCPU_Mid)
			//{
			//	//Low CPU에서는 Repaint가 아주 낮은 단위로 호출되는데,
			//	//이때 중간중간에 2프레임간 연속으로 실행되면서 FPS가 높은걸로 나타난다.
			//	//따라서 일정값의 FPS보다 높다면 (=시간 간격이 짧다면)
			//	//그 프레임은 무시해야한다.
			//	//이전 코드
			//	//if (_prevDeltaTime[REPAINT] > 0.01f)
			//	//{
			//	//	_fps = (int)(1.0f / (_prevDeltaTime[REPAINT] * _subTimer[REPAINT].TimeMultiply));
			//	//}				
			//}
			//else
			//{
			//	//이전 코드
			//	//if (_prevDeltaTime[REPAINT] > 0.0f)
			//	//{
			//	//	_fps = (int)(1.0f / (_prevDeltaTime[REPAINT] * _subTimer[REPAINT].TimeMultiply));
			//	//}				
			//} 
			#endregion


			//이전 코드
			//_stopWatch[REPAINT].Reset();
			//_stopWatch[REPAINT].Start();

			if (lowCPUStatus == apEditor.LOW_CPU_STATUS.LowCPU_Low || lowCPUStatus == apEditor.LOW_CPU_STATUS.LowCPU_Mid)
			{
				//LowCPU 모드일때는
				//Repaint직후에 불필요한 Update를 막고자 Update용 StopWatch를 리셋한다
				//이전 코드
				//_stopWatch[UPDATE].Reset();
				//_stopWatch[UPDATE].Start();

				//추가 21.7.17 : 에디터 API
				_prevEditorTime[UPDATE] = EditorApplication.timeSinceStartup;
			}
		}

		//강제로 다른 곳에서 업데이트를 하면 초기화한다.
		public void ResetTime_Update()
		{
			//매번 연산하는 값이 아닌 누적 연산을 하는 Update 타입은 외부에서 중복 처리시 타이머를 리셋할 수 있다.
			//이전 코드
			#region [미사용 코드]
			//_stopWatch[UPDATE].Stop();
			//_stopWatch[UPDATE].Reset();
			//_stopWatch[UPDATE].Start();

			//_deltaTimeCount[UPDATE] = 0;
			//_prevDeltaTime[UPDATE] = 0.0f; 
			#endregion


			//추가 21.7.17 : 에디터 API
			_prevEditorTime[UPDATE] = EditorApplication.timeSinceStartup;
			_deltaTimeCount2[UPDATE] = 0.0;
			_lastDeltaTime2[UPDATE] = 0.0;
		}


		// Get / Set
		//----------------------------------------
		//타입1 : Stopwatch를 이용한 방식
		#region [미사용 코드]
		//public float DeltaTime_Update { get { return (float)(_prevDeltaTime[UPDATE] * _subTimer[UPDATE].TimeMultiply); } }
		//public float DeltaTime_UpdateAllFrame { get { return (float)(_prevDeltaTime[UPDATE_ALL_FRAME] * _subTimer[UPDATE_ALL_FRAME].TimeMultiply); } }
		//public float DeltaTime_Repaint { get { return (float)(_prevDeltaTime[REPAINT] * _subTimer[REPAINT].TimeMultiply); } }
		//public int FPS { get { return _fps; } } 
		#endregion

		//타입2 : EditorAPI를 이용한 방식
		//public float DeltaTime_Update { get { return (float)(_lastDeltaTime2[UPDATE]); } }//이 값은 사용되지는 않는다. (Update 함수의 리턴 값으로 조절)
		public float DeltaTime_UpdateAllFrame { get { return (float)(_lastDeltaTime2[UPDATE_ALL_FRAME]); } }
		public float DeltaTime_Repaint { get { return (float)(_lastDeltaTime2[REPAINT]); } }
		//public int FPS { get { return _fps2; } }//삭제 21.7.17 : FPS 계산 방식 변경으로 

		//디버그용 (기존 방식과 변경된 방식의 시간 차이 > "거의 없더라")
		//public int GapMsec { get { return Mathf.Abs((int)(((_prevDeltaTime[UPDATE_ALL_FRAME] * _subTimer[UPDATE_ALL_FRAME].TimeMultiply) - _prevDeltaTime2[UPDATE_ALL_FRAME]) * 1000.0)); } }
	}

}