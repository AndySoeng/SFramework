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
	/// apMatrix와 apMatrix3x3 등의 연산 순서를 기록하는 클래스.
	/// 연산 과정을 기록한뒤 Inverse를 하거나 특정 연산만 포함시킬 수 있다.
	/// 키값은 오브젝트나 string으로 한다.
	/// 연산 속도상 에디터 코드에서만 수행한다.
	/// </summary>
	public class apStackMatrix
	{
		// Unit Class
		//---------------------------------------------------------
		/// <summary>
		/// 연산 유닛의 종류. 어떤 연산이 들어가는가.
		/// </summary>
		public enum UnitValueType
		{
			VertPos,
			MatrixWrap,
			Matrix3x3
		}

		public enum CalculateType
		{
			Interpolation,
			Add,
			Multiply,
			RMultiply,
		}



		public class MatrixUnit
		{
			public UnitValueType _valueType = UnitValueType.Matrix3x3;
			public CalculateType _calculateType = CalculateType.Add;

			public int _nVert = 0;
			public Vector2[] _val_VertPos = null;
			public apMatrix _val_MatrixWrap = null;
			public apMatrix3x3 _val_Matrix3x3 = apMatrix3x3.identity;

			public bool _isWeighted = false;
			public float _weight = 1.0f;

			//private bool _isStringKey = false;
			public object _keyObj = null;
			public string _keyString = "";

			//앞 뒤로 연결된 Unit을 저장한다.
			public MatrixUnit _prevUnit = null;
			public MatrixUnit _nextUnit = null;


			public bool _isCalculated = false;


			public MatrixUnit(object keyObj)
			{
				_keyObj = keyObj;
				//_isStringKey = false;
				_isCalculated = true;
			}

			public MatrixUnit(string keyString)
			{
				_keyString = keyString;
				//_isStringKey = false;
				_isCalculated = true;
			}

			public void SetPosition(Vector2[] vertPositions, CalculateType calculateType, bool isWeighted, float weight)
			{
				_valueType = UnitValueType.VertPos;
				if (_val_VertPos == null || _val_VertPos.Length != vertPositions.Length)
				{
					_val_VertPos = new Vector2[vertPositions.Length];
				}
				for (int i = 0; i < vertPositions.Length; i++)
				{
					_val_VertPos[i] = vertPositions[i];
				}

				_calculateType = calculateType;
				_isWeighted = isWeighted;
				_weight = weight;
			}




			public void SetMatrixWrap(apMatrix matrixWrap, CalculateType calculateType, bool isWeighted, float weight)
			{
				_valueType = UnitValueType.MatrixWrap;
				if (_val_MatrixWrap == null)
				{
					_val_MatrixWrap = new apMatrix(matrixWrap);//<<복사를 하자
				}
				else
				{
					_val_MatrixWrap.SetMatrix(matrixWrap, true);
				}
				_calculateType = calculateType;
				_isWeighted = isWeighted;
				_weight = weight;
			}

			public void SetMatrix3x3(apMatrix3x3 matrix4x4, CalculateType calculateType, bool isWeighted, float weight)
			{
				_valueType = UnitValueType.Matrix3x3;
				_val_Matrix3x3 = matrix4x4;
				_calculateType = calculateType;
				_isWeighted = isWeighted;
				_weight = weight;
			}
		}


		// Members
		//---------------------------------------------------------
		public enum STACK_LEVEL
		{
			L0_Rigging,
			L1_VertLocal,
			L2_Transform,
			L3_VertWorld,
		}
		//private Dictionary<int, List<MatrixUnit>> _matrixUnit = new List<MatrixUnit>();
		//private apMatrix3x3 _resultMatrix = apMatrix3x3.identity;

		// Init
		//---------------------------------------------------------
		public apStackMatrix()
		{
			Clear();
		}

		public void Clear()
		{
			//_matrixUnit.Clear();
			//_resultMatrix = apMatrix3x3.identity;
		}



		// Functions - Add Matrix
		//---------------------------------------------------------




		// Get / Set
		//---------------------------------------------------------
	}

}