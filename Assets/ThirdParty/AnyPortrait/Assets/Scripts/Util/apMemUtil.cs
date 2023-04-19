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
using System.Runtime.InteropServices;

namespace AnyPortrait
{
	/// <summary>
	/// 데이터 초기화시 빠르게 설정해주기 위한 메모리 더미가 포함된 객체.
	/// Array.Copy 방식으로 초기화할 경우, 배열의 크기, 데이터 타입을 요청하면 적절히 생성해서 리턴한다.
	/// 에디터, 게임에서 모두 사용한다.
	/// 만약 0으로 초기화한다면 그냥 Array.Clear를 사용하자.
	/// 이 객체는 0이 아닌 초기값을 갖는 경우 (예: 행렬의 Identity)에만 사용된다.
	/// </summary>
	public class apMemUtil
	{
		// Static Member
		//------------------------------------------
		private static apMemUtil s_instance = new apMemUtil();
		public static apMemUtil I { get { return s_instance; } }
		
		// Members
		//------------------------------------------
		private const int ARRSIZE_UNIT = 100;//크기는 100단위로 증가한다.

		//apMatrix3x3 초기화용
		private const int INIT_MATRIX3X3 = 1000;
		private int _nMatrix3x3 = -1;
		private apMatrix3x3[] _arrMatrix3x3 = null;



		// Init
		//------------------------------------------
		public apMemUtil()
		{
			//초기값 만큼 행렬을 생성하자
			MakeArr_Matrix3x3(INIT_MATRIX3X3);
		}


		private void MakeArr_Matrix3x3(int numMatrix)
		{
			_nMatrix3x3 = numMatrix;
			_arrMatrix3x3 = new apMatrix3x3[_nMatrix3x3];
			for (int i = 0; i < _nMatrix3x3; i++)
			{
				_arrMatrix3x3[i].SetIdentity();
			}
		}


		// Function
		//------------------------------------------
		/// <summary>
		/// SetIdentity()가 적용된 초기화된 행렬 배열을 리턴한다.
		/// </summary>
		/// <param name="nArr"></param>
		/// <returns></returns>
		public apMatrix3x3[] GetInitMatrix3x3(int nArr)
		{
			if(_nMatrix3x3 < nArr)
			{
				//현재 준비된 행렬 배열의 크기가 작다.
				//새로운 크기를 다시 계산하여 행렬을 다시 생성하자
				int nNewArr = (Mathf.Max((nArr / ARRSIZE_UNIT), 0) + 1) * ARRSIZE_UNIT;
				MakeArr_Matrix3x3(nNewArr);
			}
			return _arrMatrix3x3;
		}
	}
}