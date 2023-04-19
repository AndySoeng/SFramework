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
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// FPS를 계산하는 카운터 객체
	/// 이전과 달리 구간별로 데이터를 모은 후에 바로 값을 반영한다. (실시간 평균 데이터 계산 방식에서 구간 계산 방식으로 변경)
	/// 프레임 격차가 클때의 정보도 알려준다.
	/// </summary>
	public class apFPSCounter
	{
		// Members
		//------------------------------------------------------
		//결과값 : 이전 구간에서의 결과
		private int _result_AvgFPS = 0;

		//삭제 21.7.17 : 기록하는건 좋지만 사용하지 않는다.
		//private int _result_HighFPS = 0;//최고 값
		//private int _result_LowFPS = 0;//최저 값

		//측정된 데이터
		//구간 : 1초가 넘거나 데이터 개수(100)가 넘었을 경우

		//이전 방식
		//private const int MAX_DATA = 100;
		//private const int MIN_DATA = 10;//최소 10개는 저장해야..
		//private const float MAX_TIME = 1.0f;

		//private int _nData = 0;
		//private float _curTime = 0.0f;

		//private int[] _dataList = null;

		//private int _cal_CurFPS = 0;
		//private int _cal_TotalFPS = 0;

		//변경 21.7.17 : 배열로 만들 필요 없이 1초 단위로 계산하자
		//프레임당 Delta 시간을 모두 누적하자
		private int _nRecords = 0;
		private float _tRecord = 0.0f;
		private const float RECORD_TIME_LENGTH = 1.0f;
		private float _totalFPS = 0.0f;


		// Init
		//------------------------------------------------------
		public apFPSCounter()
		{	
			_result_AvgFPS = 0;

			//삭제
			//_result_HighFPS = 0;
			//_result_LowFPS = 0;

			//이전
			//_dataList = new int[MAX_DATA];

			//변경
			_nRecords = 0;
			_tRecord = 0.0f;
			_totalFPS = 0.0f;

			//Reset();
		}


		// Functions
		//------------------------------------------------------
		#region [미사용 코드]
		//private void Reset()
		//{
		//	//_nData = 0;
		//	//_curTime = 0.0f;

		//	_nRecords = 0;
		//	_tRecord = 0.0f;
		//	_totalFPS = 0.0f;
		//} 
		#endregion

		//public void SetData(int FPS)//이전
		public void SetData(float deltaTime)//변경 21.7.17 : float 시간을 받자
		{
			#region [미사용 코드] : 이전 방식
			//if(FPS <= 0)
			//{
			//	return;
			//}
			//float frameTime = 1.0f / (float)FPS;

			////프레임 데이터를 저장
			//_curTime += frameTime;
			//_dataList[_nData] = FPS;
			//_nData++;

			////만약 구간에 꽉 찼다면, 계산한후 값을 리턴하자
			//if (_nData > MIN_DATA)
			//{
			//	if (_nData >= MAX_DATA || _curTime > MAX_TIME)
			//	{
			//		_result_AvgFPS = 0;
			//		//첫번째는 그냥 입력

			//		_cal_CurFPS = _dataList[0];
			//		_cal_TotalFPS = _cal_CurFPS;
			//		_result_HighFPS = _cal_CurFPS;
			//		_result_LowFPS = _cal_CurFPS;

			//		for (int i = 1; i < _nData; i++)
			//		{
			//			_cal_CurFPS = _dataList[i];

			//			_cal_TotalFPS += _cal_CurFPS;
			//			if(_cal_CurFPS < _result_LowFPS)
			//			{
			//				_result_LowFPS = _cal_CurFPS;
			//			}
			//			if(_cal_CurFPS > _result_HighFPS)
			//			{
			//				_result_HighFPS = _cal_CurFPS;
			//			}
			//		}

			//		_result_AvgFPS = (int)(((float)_cal_TotalFPS / (float)_nData) + 0.5f);

			//		Reset();
			//	}
			//} 
			#endregion

			//변경 21.7.17 : 더 간단한 방식으로 계산하자
			if(deltaTime > 0.0f)
			{
				_nRecords += 1;
				_tRecord += deltaTime;
				_totalFPS += 1.0f / deltaTime;

				if(_tRecord > RECORD_TIME_LENGTH)
				{
					_result_AvgFPS = (int)(_totalFPS / (float)_nRecords);

					_nRecords = 0;
					_tRecord = 0.0f;
					_totalFPS = 0.0f;
				}
			}
		}

		// Get
		//------------------------------------------------------
		public int AvgFPS {  get {  return _result_AvgFPS; } }
		//public int LowFPS {  get {  return _result_LowFPS; } }
		//public int HighFPS {  get {  return _result_HighFPS; } }
	}
}