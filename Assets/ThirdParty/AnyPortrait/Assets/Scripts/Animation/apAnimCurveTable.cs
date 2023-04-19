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
	/// Bezier 보간을 해주기 위한 "t"값이 저장된 테이블
	/// 직렬화되지는 않고, 로드 및 링크 후에 갱신해줘야 한다.
	/// 생성시 : T를 기준으로 X(ITP)를 만들어준다.
	/// 참조시 : X(ITP)를 넣으면 적절한 T를 리턴한다.
	/// </summary>
	public class apAnimCurveTable
	{
		// Members
		//--------------------------------------------------------------------
		[NonSerialized]
		public apAnimCurve _parentAnimCurve = null;

		[NonSerialized]
		public apAnimCurve _linkedAnimCurve = null;

		private const int TABLE_SIZE = 21; // 0 ~ 1 + (0.1, 0.2..0.9)
										   //private const float T_UNIT_SIZE = 0.1f;

		public float[] _tList = new float[TABLE_SIZE];
		public float[] _xList = new float[TABLE_SIZE];

		//private int _tmpIndex = 0;
		//private int _iA = 0;
		//private int _iB = 0;
		//private float _lengthX = 0.0f;

		// Init
		//--------------------------------------------------------------------
		public apAnimCurveTable(apAnimCurve parentAnimCurve)
		{
			_parentAnimCurve = parentAnimCurve;

			_linkedAnimCurve = null;
			Init();
		}
		public void Init()
		{
			for (int i = 0; i < TABLE_SIZE; i++)
			{
				_tList[i] = (i * (1.0f / (float)(TABLE_SIZE - 1)));
				_xList[i] = 0.0f;
			}
		}

		// Link
		//--------------------------------------------------------------------
		public bool LinkAnimCurve(apAnimCurve animCurve)
		{
			bool isChanged = _linkedAnimCurve != animCurve;
			_linkedAnimCurve = animCurve;
			return isChanged;
		}

		// Make
		//--------------------------------------------------------------------
		public void MakeTable()
		{
			Init();

			if (_linkedAnimCurve == null)
			{
				return;
			}

			//Debug.Log("--------------------MakeTable----------------");
			for (int i = 0; i < TABLE_SIZE; i++)
			{
				_xList[i] = CalculateX(_tList[i]);
				//Debug.Log(" X : " + _xList[i] + " >> T : " + _tList[i]);
			}
			//Debug.Log("----------------------------------------------");

		}



		// Get T
		//--------------------------------------------------------------------
		private float CalculateX(float t)
		{
			if (_linkedAnimCurve == null)
			{
				return -50.0f;
			}
			t = Mathf.Clamp01(t);
			float revT = Mathf.Clamp01(1.0f - t);

			if (_linkedAnimCurve._keyIndex < _parentAnimCurve._keyIndex)
			{
				//Link [0] -> Parent [1]

				return (0.0f * revT * revT * revT) +
						(3.0f * revT * revT * t * Mathf.Clamp01(_linkedAnimCurve._nextSmoothX)) +
						(3.0f * revT * t * t * (1.0f - Mathf.Clamp01(_parentAnimCurve._prevSmoothX))) +
						(1.0f * t * t * t);
			}
			else if (_linkedAnimCurve._keyIndex > _parentAnimCurve._keyIndex)
			{
				//Parent [0] -> Link [1]

				return (0.0f * revT * revT * revT) +
						(3.0f * revT * revT * t * Mathf.Clamp01(_parentAnimCurve._nextSmoothX)) +
						(3.0f * revT * t * t * (1.0f - Mathf.Clamp01(_linkedAnimCurve._prevSmoothX))) +
						(1.0f * t * t * t);
			}
			return 0.0f;
		}

		//private float _prevITPX = 0.0f;
		public float GetT(float itpX)
		{
			return CalculateX(itpX);

			

			//_tmpIndex = -1;
			////요청된 itpX가 포함된 xList를 찾는다.
			////(_tmpIndex) < itpX < (_tmpIndex + 1)

			//for (int i = 0; i < TABLE_SIZE; i++)
			//{
			//	if (itpX >= _xList[i])
			//	{
			//		_tmpIndex = i;
			//		break;
			//	}
			//}
			//if (_tmpIndex < 0 || _tmpIndex + 1 >= TABLE_SIZE)
			//{
			//	//아마 1이어서 못찾았을 듯
			//	return 1.0f;
			//}
			////_tList[_tmpIndex] ~ _tList[_tmpIndex + 1] 사이의 결과값이 나와야한다.
			////만약, _xA와 _xB의 크기가 오름차순이 아니라면 _xA만 리턴한다 (그래프 에러)
			//_iA = _tmpIndex;
			//_iB = _tmpIndex + 1;
			//if (_xList[_iA] >= _xList[_iB])
			//{
			//	return _tList[_iA];
			//}
			//_lengthX = _xList[_iB] - _xList[_iA];
			//itpX -= _xList[_iA];



			//return (_tList[_iA] * (_lengthX - itpX) + _tList[_iB] * itpX) / _lengthX;
		}
	}

}