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
	/// 오직 AsyncInitialize를 위한 함수
	/// </summary>
	public class apAsyncTimer
	{
		// Members
		//------------------------------------------------
		private long _timePerYield = 0;
		private const int MIN_TIME = 10; // 100FPS
		private const int MAX_TIME = 1000; // 1 FPS

		private System.Diagnostics.Stopwatch _timer = null;
		//private bool _isDebug = false;
		//private int _yieldCount = 0;

		// Init
		//------------------------------------------------
		public apAsyncTimer(int timePerYield
			//, bool isDebug = false
			)
		{
			_timePerYield = (long)Mathf.Clamp(timePerYield, MIN_TIME, MAX_TIME);
			//_isDebug = isDebug;
			//if(_isDebug)
			//{
			//	Debug.Log("Time Per Yield : " + _timePerYield);
			//}
			
			//_yieldCount = 0;

			_timer = new System.Diagnostics.Stopwatch();
			_timer.Reset();
			_timer.Start();
		}


		// Functions
		//------------------------------------------------
		public bool IsYield()
		{
			long elpasedTimeMS = _timer.ElapsedMilliseconds;


			if(elpasedTimeMS > _timePerYield)
			{
				//if(_isDebug)
				//{
				//	_yieldCount++;
				//	Debug.LogWarning("Yield! > " + elpasedTimeMS + " (" + _timePerYield + ") - " + _yieldCount);
				//}
				_timer.Stop();
				_timer.Reset();
				return true;
			}

			//if(_isDebug)
			//{
			//	Debug.Log("Pass > " + elpasedTimeMS);
			//}
			return false;
		}

		public IEnumerator WaitAndRestart()
		{
			_timer.Stop();

			//yield return new WaitForEndOfFrame();
			yield return null;

			_timer.Stop();
			_timer.Reset();
			_timer.Start();
		}

		public void OnCompleted()
		{
			_timer.Stop();
		}
		// Get / Set
		//------------------------------------------------

	}
}
