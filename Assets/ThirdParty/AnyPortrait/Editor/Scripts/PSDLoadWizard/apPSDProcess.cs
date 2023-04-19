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
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Ntreev.Library.Psd;

using AnyPortrait;

namespace AnyPortrait
{
	public class apPSDProcess
	{
		// Members
		//---------------------------------------
		public delegate bool FUNC_PROCESS_UNIT(int index);

		public class ProcessUnit
		{
			private FUNC_PROCESS_UNIT _funcUnit = null;
			public int _count = -1;

			public ProcessUnit(int count)
			{
				_count = count;
			}

			public void AddProcess(FUNC_PROCESS_UNIT funcProcess)
			{
				_funcUnit = funcProcess;
			}

			public bool Run(int index)
			{
				if(_funcUnit == null)
				{
					return false;
				}
				return _funcUnit(index);
			}

			public void ChangeCount(int count)
			{
				_count = count;
			}
		}
		
		private List<ProcessUnit> _units = new List<ProcessUnit>();
		private int _totalProcessCount = 0;
		private int _curProcessX100 = 0;

		private bool _isRunning = false;
		private bool _isSuccess = false;

		private int _iCurUnit = -1;
		private int _iSubProcess = -1;
		private int _iTotalProcess = -1;
		private string _strProcessLabel;


		public bool IsRunning { get { return _isRunning; } }
		public bool IsSuccess { get { return !_isRunning && _isSuccess; } }
		public int ProcessX100 { get { return _curProcessX100; } }
		

		// Init
		//---------------------------------------
		public apPSDProcess()
		{
			Clear();
		}

		public void Clear()
		{
			_units.Clear();

			_totalProcessCount = 0;

			_isRunning = false;
			_isSuccess = false;

			_iCurUnit = -1;
			_iSubProcess = -1;
			_iTotalProcess = 0;
			_curProcessX100 = 0;
		}

		// Functions
		//---------------------------------------
		public void Add(FUNC_PROCESS_UNIT funcProcess, int count)
			{
				ProcessUnit newUnit = new ProcessUnit(count);
				newUnit.AddProcess(funcProcess);
				_units.Add(newUnit);

				_totalProcessCount += count;//전체 카운트를 높인다. (나중에 퍼센트 체크를 위함)
			}

			public void ChangeCount(int workIndex, int count)
			{
				if (workIndex < 0 || workIndex >= _units.Count)
				{
					return;
				}
				_units[workIndex].ChangeCount(count);

				_totalProcessCount = 0;
				//전체 카운트 갱신
				for (int i = 0; i < _units.Count; i++)
				{
					_totalProcessCount += _units[i]._count;
				}
			}

			public void StartRun(string strProcessLabel)
			{
				_curProcessX100 = 0;

				_isRunning = true;
				_isSuccess = false;

				_iCurUnit = 0;
				_iSubProcess = 0;
				_iTotalProcess = 0;
				_strProcessLabel = strProcessLabel;
			}

			public void Run()
			{
				if (!_isRunning)
				{
					return;
				}
				if (_iCurUnit >= _units.Count)
				{
					//끝. 성공!
					_isRunning = false;
					_isSuccess = true;
					_curProcessX100 = 100;

					//Debug.Log("Process Success : " + _strProcessLabel);
					return;
				}
				ProcessUnit curUnit = _units[_iCurUnit];



				//실행하고 퍼센트를 높이자
				if (!curUnit.Run(_iSubProcess))
				{
					//실패 했네염..
					_isRunning = false;
					_isSuccess = false;
					_curProcessX100 = 0;
					Debug.LogError("AnyPortrait : PSD Process Failed : " + _strProcessLabel + " (Current Step : " + _iCurUnit + " / Sub Procss : " + _iSubProcess + ")");
					return;
				}

				_iTotalProcess++;
				_iSubProcess++;

				if (_iSubProcess >= curUnit._count)
				{
					_iSubProcess = 0;
					_iCurUnit++;
				}

				_curProcessX100 = (int)Mathf.Clamp((((float)_iTotalProcess * 100.0f) / (float)_totalProcessCount), 0, 100);
			}
	}
}