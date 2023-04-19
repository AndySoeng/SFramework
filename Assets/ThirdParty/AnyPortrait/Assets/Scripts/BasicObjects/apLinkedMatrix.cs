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
	/// Matrix 연산을 추적하여 중간에 참조된 연산값과 World Matrix로의 자유로운 전환을 위한 객체
	/// 연산이 이루어지면 Linked 방식으로 연결이 된다.
	/// 객체 자체는 연산이 이루어지는 곳에서 가지고 있도록 하자
	/// World 연산은 필요한 경우에 하도록 하자
	/// 생성은 미리 해두고, 연산값 지정과 연산 순서는 매번 갱신하자
	/// 순서는 Child->Parent 순서로 하고, Child는 여러개 둘 수 있다. (연산 순서에 상관이 없는 경우)
	/// </summary>
	public class apLinkedMatrix
	{
		public enum VALUE_TYPE
		{
			VertPos,
			MatrixWrap,
			Matrix3x3,
		}

		/// <summary>
		/// 계산 과정 중 어느 단계에 해당하는가.
		/// 중간 단계인 CalculateData 단계에서는 Child->Parent 방식으로 계산이 진행되며,
		/// 결과 단계인 Result 에서는 0->4의 순서대로 작동을 한다. (Prev->Next 순서대로 작동)
		/// </summary>
		public enum LINK_STEP
		{
			/// <summary>CalculateResultStack 이전까지의 중간 계산 값이다.</summary>
			CalculateData,
			Result_0_Rigging,
			Result_1_StaticMesh,
			Result_2_VertLocal,
			Result_3_MeshTransform,
			Result_4_VertWorld,
		}

		// <[연산] {값}> -> <[연산] {값}> 인지
		// <{값} [연산]> -> <{값} [연산]> 인지 제대로 확인하자
		//Weight는 어디에 붙는가..> Child와의 연산용 Weight가 있고, Parent가 대신 사용할 Weight가 있다.
		//WeightAdded : Weight에 추가적으로 포함되는 값(Vertex마다 다르게 적용될 수 있다 / 표기 : A_weight)
		//Weight : Parent에서 값을 사용하기 전에 Result에 곱해지는 Weight<기본>. (표기 : C_weight)
		//Weight 계산은 C_weight * A_weight;
		//[연산]은 Child에 대해서
		//[Weight]는 자기 자신+Parent에 대해서 지정한다.

		public enum CALCULATE_TYPE
		{
			/// <summary>[ThisW] = [ChildW * weight] * (1-itp) + [ThisL] * (itp)</summary>
			Interpolation,
			/// <summary>[ThisW] = [ChildW * weight] * [ThisL]</summary>
			Add,
			/// <summary>[ThisW] = {[ThisL] x [ChildMatW]} * (weight) + {[ThisL] * (1-weight)}</summary>
			Multiply,
			/// <summary>[ThisW] = {[ThisL] rx [ChildMatW]} * (weight) + {[ThisL] * (1-weight)}</summary>
			RMultiply,
			/// <summary>[ThisW] = ChildW * weight (Child가 여러개인 경우 가중치 합으로 계산됨)</summary>
			PassAndMerge,


		}

		public VALUE_TYPE _valueType = VALUE_TYPE.VertPos;
		public CALCULATE_TYPE _calculateType = CALCULATE_TYPE.Add;

		public float _lerp = 0.0f;

		public int _nVerts = 0;
		public Vector2[] _vertPos = null;
		public apMatrix _matrixWrap = null;
		public apMatrix3x3 _matrix4x4 = apMatrix3x3.identity;

		public bool _isWeight = false;
		public float _weight = 1.0f;//자신의 값에 해당하는 Weight. Parent가 사용하게 된다.

		public float _weightCalAdded = 1.0f;//Child 연산에 직접 포함되는 Weight. 기본적으로는 1
		public bool _isWeightCalAdded = false;

		public object _keyObject = null;
		public string _keyName = "";

		public LINK_STEP _linkStep = LINK_STEP.CalculateData;

		public List<apLinkedMatrix> _childMatrix = new List<apLinkedMatrix>();
		public apLinkedMatrix _parentMatrix = null;


		public apLinkedMatrix _prevResultStep = null;
		public apLinkedMatrix _nextResultStep = null;

		public apLinkedMatrix(object keyObject, string keyName, LINK_STEP linkStep)
		{
			_keyObject = keyObject;
			_keyName = keyName;
			_linkStep = linkStep;
		}

		public apLinkedMatrix SetWeight(float weight)
		{
			_weight = weight;
			_isWeight = true;
			return this;
		}

		public apLinkedMatrix SetCalculateAddedWeight(float weightCalculateAdded)
		{
			_weightCalAdded = weightCalculateAdded;
			_isWeightCalAdded = true;
			return this;
		}

		public apLinkedMatrix SetVertPosition(Vector2[] vertPositions, CALCULATE_TYPE calculateType, float lerpIfInterpolation = 1.0f)
		{
			_valueType = VALUE_TYPE.VertPos;
			_nVerts = vertPositions.Length;
			if (_vertPos == null || _vertPos.Length != _nVerts)
			{
				_vertPos = new Vector2[_nVerts];
			}
			for (int i = 0; i < _nVerts; i++)
			{
				_vertPos[i] = vertPositions[i];
			}

			_calculateType = calculateType;
			if (_calculateType == CALCULATE_TYPE.Interpolation)
			{
				_lerp = lerpIfInterpolation;
			}

			_weight = 1.0f;
			_isWeight = false;

			_weightCalAdded = 1.0f;
			_isWeightCalAdded = false;

			return this;
		}


		public apLinkedMatrix SetVertPosition(List<apModifiedVertex> modVerts, CALCULATE_TYPE calculateType, float lerpIfInterpolation = 1.0f)
		{
			_valueType = VALUE_TYPE.VertPos;
			_nVerts = modVerts.Count;
			if (_vertPos == null || _vertPos.Length != _nVerts)
			{
				_vertPos = new Vector2[_nVerts];
			}
			for (int i = 0; i < _nVerts; i++)
			{
				_vertPos[i] = modVerts[i]._deltaPos;
			}

			_calculateType = calculateType;
			if (_calculateType == CALCULATE_TYPE.Interpolation)
			{
				_lerp = lerpIfInterpolation;
			}

			_weight = 1.0f;
			_isWeight = false;

			_weightCalAdded = 1.0f;
			_isWeightCalAdded = false;

			return this;
		}

		public apLinkedMatrix SetMatrixWrap(apMatrix matrixWrap, CALCULATE_TYPE calculateType, float lerpIfInterpolation = 1.0f)
		{
			_valueType = VALUE_TYPE.MatrixWrap;
			if (_matrixWrap == null)
			{
				_matrixWrap = new apMatrix(matrixWrap);
			}
			else
			{
				_matrixWrap.SetMatrix(matrixWrap, true);
			}
			_calculateType = calculateType;
			if (_calculateType == CALCULATE_TYPE.Interpolation)
			{
				_lerp = lerpIfInterpolation;
			}

			_weight = 1.0f;
			_isWeight = false;

			_weightCalAdded = 1.0f;
			_isWeightCalAdded = false;

			return this;
		}


		public apLinkedMatrix SetMatrix3x3(apMatrix3x3 matrix4x4, CALCULATE_TYPE calculateType, float lerpIfInterpolation = 1.0f)
		{
			_valueType = VALUE_TYPE.Matrix3x3;
			_matrix4x4 = matrix4x4;
			_calculateType = calculateType;
			if (_calculateType == CALCULATE_TYPE.Interpolation)
			{
				_lerp = lerpIfInterpolation;
			}

			_weight = 1.0f;
			_isWeight = false;

			_weightCalAdded = 1.0f;
			_isWeightCalAdded = false;

			return this;
		}


		public apLinkedMatrix SetPassAndMerge(VALUE_TYPE valueType)
		{
			_valueType = valueType;

			//값이 별도로 존재하지 않는다.
			_calculateType = CALCULATE_TYPE.PassAndMerge;

			_weight = 1.0f;
			_isWeight = false;

			_weightCalAdded = 1.0f;
			_isWeightCalAdded = false;

			return this;
		}


		public void ReadyToCalculate()
		{
			_childMatrix.Clear();
			_parentMatrix = null;

			_weight = 1.0f;
			_isWeight = false;

			_weightCalAdded = 1.0f;
			_isWeightCalAdded = false;
		}

		public void LinkToParent_CalculateData(apLinkedMatrix parentMatrix)
		{
			_parentMatrix = parentMatrix;
			if (!parentMatrix._childMatrix.Contains(this))
			{
				parentMatrix._childMatrix.Add(this);
			}
		}

		public void LinkToNext_Result(apLinkedMatrix nextResultMatrix)
		{
			_nextResultStep = nextResultMatrix;
			_nextResultStep._prevResultStep = this;
		}

		public bool IsValidKey(object keyObject, string keyName)
		{
			//public object _keyObject = null;
			//public string _keyName = "";
			//일단 오브젝트부터 검사.
			//오브젝트가 동일하면 바로 Okay
			if (keyObject != null)
			{
				if (_keyObject == keyObject)
				{
					return true;
				}
			}
			if (!string.IsNullOrEmpty(keyName))
			{
				if (string.Equals(_keyName, keyName))
				{
					return true;
				}
			}
			return false;
		}


	}


}