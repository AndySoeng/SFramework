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
	/// AnimCurve가 Key 데이터를 가진다면, CurveResult는 두개의 CurveKey를 바탕으로 실제로 어떤 식으로 계산되는지를 결정한다.
	/// 링크된 두개의 Key값에 따라서 조합된 설정을 가져온다.
	/// 각각의 Key에 이 레퍼런스를 가지며, 양쪽 어느 Key에서 참조하더라도 같은 결과가 나온다. (단 보간값은 어디서 호출했냐에 따라 다르다)
	/// 보간처리는 이 클래스의 "조합 결과"를 기반으로 각각 처리한다.
	/// 기본 계산 데이터는 저장되며, 참조 키만 레퍼런스로 링크된다.
	/// </summary>
	[Serializable]
	public class apAnimCurveResult
	{
		// Members
		//---------------------------------------
		[NonSerialized]
		public apAnimCurve _curveKeyA = null;

		[NonSerialized]
		public apAnimCurve _curveKeyB = null;
		


		[SerializeField]
		private int _frameIndex_A = -1;

		[SerializeField]
		private int _frameIndex_B = -1;

		[SerializeField]
		private apAnimCurve.TANGENT_TYPE _tangentType = apAnimCurve.TANGENT_TYPE.Linear;

		[SerializeField]
		private bool _isTargetIsA = true;

		[SerializeField]
		private float _smoothX_A = 0.0f;

		[SerializeField]
		private float _smoothY_A = 0.0f;

		[SerializeField]
		private float _smoothX_B = 0.0f;

		[SerializeField]
		private float _smoothY_B = 0.0f;

		



		// Init
		//---------------------------------------
		public apAnimCurveResult()
		{

		}

		/// <summary>
		/// CurveKey를 Link한다.
		/// A가 Prev, B가 Next여야 한다. (반대로 하면 처리 잘못됨)
		/// </summary>
		/// <param name="curveKeyA"></param>
		/// <param name="curveKeyB"></param>
		/// <param name="isTargetIsA">이 Result를 소유하고 있는 CurveKey가 Prev면 True, Next면 False</param>
		/// <param name="isMakeCurve">Link와 함께 MakeCurve를 할 것인가</param>
		//public void Link(apAnimCurve curveKeyA, apAnimCurve curveKeyB, bool isTargetIsA, bool isMakeCurve)
		public void Link(apAnimCurve curveKeyA, apAnimCurve curveKeyB, bool isTargetIsA) //isMakeCurve 삭제 : 19.5.20
		{
			//bool isMakeCurveForce = true;
			//if(_curveKeyA == curveKeyA && _curveKeyB == curveKeyB)
			//{
			//	isMakeCurveForce = false;
			//}

			_curveKeyA = curveKeyA;
			_curveKeyB = curveKeyB;

			_isTargetIsA = isTargetIsA;

			//이전
			//if (isMakeCurve)
			//{
			//	MakeCurve();
			//}

			//변경 : 19.5.20 최적화 작업
			MakeCurve();
		}


		public void ResetSmoothSetting()
		{
			if (_curveKeyA == null || _curveKeyB == null)
			{
				return;
			}

			_curveKeyA._nextSmoothX = apAnimCurve.CONTROL_POINT_X_OFFSET;
			_curveKeyA._nextSmoothY = 0.0f;

			_curveKeyB._prevSmoothX = apAnimCurve.CONTROL_POINT_X_OFFSET;
			_curveKeyB._prevSmoothY = 0.0f;

			MakeCurve();
		}

		public void CopyCurve(apAnimCurveResult srcCurveResult)
		{
			if (_curveKeyA == null || _curveKeyB == null)
			{
				return;
			}

			_tangentType = srcCurveResult._tangentType;

			_curveKeyA._nextTangentType = srcCurveResult._curveKeyA._nextTangentType;
			_curveKeyA._nextSmoothX = srcCurveResult._curveKeyA._nextSmoothX;
			_curveKeyA._nextSmoothY = srcCurveResult._curveKeyA._nextSmoothY;

			_curveKeyB._prevTangentType = srcCurveResult._curveKeyB._prevTangentType;
			_curveKeyB._prevSmoothX = srcCurveResult._curveKeyB._prevSmoothX;
			_curveKeyB._prevSmoothY = srcCurveResult._curveKeyB._prevSmoothY;

			MakeCurve();
		}

		//public void MakeCurve(bool isForce = false)
		public void MakeCurve()
		{
			if (_curveKeyA == null || _curveKeyB == null)
			{
				_frameIndex_A = -1;
				_frameIndex_B = -1;
				_tangentType = apAnimCurve.TANGENT_TYPE.Linear;
				return;
			}


			_smoothX_A = _curveKeyA._nextSmoothX;
			_smoothY_A = _curveKeyA._nextSmoothY;

			_smoothX_B = _curveKeyB._prevSmoothX;
			_smoothY_B = _curveKeyB._prevSmoothY;

			if (_isTargetIsA)
			{
				_frameIndex_A = _curveKeyA._keyIndex;
				_frameIndex_B = _curveKeyA._nextIndex;
			}
			else
			{
				_frameIndex_A = _curveKeyB._prevIndex;
				_frameIndex_B = _curveKeyB._keyIndex;
			}
			//Debug.Log("Make Curve [ A : " + _frameIndex_A + " / B : " + _frameIndex_B + " ]");



			if (_curveKeyA._nextTangentType == apAnimCurve.TANGENT_TYPE.Constant ||
				_curveKeyB._prevTangentType == apAnimCurve.TANGENT_TYPE.Constant)
			{
				_tangentType = apAnimCurve.TANGENT_TYPE.Constant;
			}
			else if (_curveKeyA._nextTangentType == apAnimCurve.TANGENT_TYPE.Linear &&
				_curveKeyB._prevTangentType == apAnimCurve.TANGENT_TYPE.Linear)
			{
				_tangentType = apAnimCurve.TANGENT_TYPE.Linear;
			}
			else
			{
				_tangentType = apAnimCurve.TANGENT_TYPE.Smooth;
			}

			
		}


		

		private Vector2 GetSmoothItp(Vector2 controlPointA, Vector2 controlPointB, float itp)
		{
			float revItp = (1.0f - itp);
			return (Vector2.zero * revItp * revItp * revItp) +
					(3.0f * controlPointA * revItp * revItp * itp) +
					(3.0f * controlPointB * revItp * itp * itp) +
					(Vector2.one * itp * itp * itp);
		}

		private static float GetSmoothItp(float controlPointA_X, float controlPointB_X, float itp)
		{
			float revItp = (1.0f - itp);
			return (3.0f * controlPointA_X * revItp * revItp * itp) +
					(3.0f * controlPointB_X * revItp * itp * itp) +
					(itp * itp * itp);
		}

		public float GetInterpolation(float curKeyIndex, int iCurKeyIndex)
		{
			if (_curveKeyA == null || _curveKeyB == null)
			{
				return 0.0f;
			}

			if (_frameIndex_A == _frameIndex_B)
			{
				return 0.0f;
			}

			float keyRatio = Mathf.Clamp01((float)(curKeyIndex - (float)_frameIndex_A) / (float)(_frameIndex_B - _frameIndex_A));

			float resultItp = 0.0f;

			switch (_tangentType)
			{
				case apAnimCurve.TANGENT_TYPE.Constant:
					//변경전 : 0.5를 기준으로 변동
					//변경후 : 1을 기준으로 변동
					//if (keyRatio < 0.5f)
					//{
					//	resultItp = 0.0f;
					//}
					//else
					//{
					//	resultItp = 1.0f;
					//}

					if(iCurKeyIndex < _frameIndex_B)
					{
						resultItp = 0.0f;
					}
					else
					{
						resultItp = 1.0f;
					}

					break;

				case apAnimCurve.TANGENT_TYPE.Linear:
					resultItp = keyRatio;
					break;

				case apAnimCurve.TANGENT_TYPE.Smooth:
					{
						float cpXRatio = 0.3f;
						Vector2 controlPointA = new Vector2(Mathf.Clamp01(_smoothX_A), Mathf.Clamp01(_smoothY_A));
						Vector2 controlPointB = new Vector2(Mathf.Clamp01(1.0f - _smoothX_B), Mathf.Clamp01(1.0f - _smoothY_B));

						float revT = 1.0f - keyRatio;
						float linearItp = Mathf.Clamp01(
							((1.0f - Mathf.Sqrt(_smoothX_A * _smoothX_A + _smoothY_A * _smoothY_A)) * revT 
							+ (1.0f - Mathf.Sqrt(_smoothX_B * _smoothX_B + _smoothY_B * _smoothY_B)) * keyRatio)
							);

						float convertTItp = (0.0f * revT * revT * revT) +
											(3.0f * ((1.0f - controlPointA.x) * cpXRatio) * revT * revT * keyRatio) +
											(3.0f * (1.0f - (controlPointB.x * cpXRatio)) * revT * keyRatio * keyRatio) +
											(1.0f * keyRatio * keyRatio * keyRatio);

						//float convertTItp = (0.0f * revT * revT * revT) +
						//					(3.0f * (controlPointA.x) * revT * revT * keyRatio) +
						//					(3.0f * (controlPointB.x) * revT * keyRatio * keyRatio) +
						//					(1.0f * keyRatio * keyRatio * keyRatio);

						convertTItp = convertTItp * (1.0f - linearItp) + keyRatio * linearItp;

						resultItp = GetSmoothItp(controlPointA.y, controlPointB.y, convertTItp);
						resultItp = resultItp * (1.0f - linearItp) + convertTItp * linearItp;
						
					}
					break;
			}
			if (_isTargetIsA)
			{
				resultItp = 1.0f - resultItp;
			}

			return resultItp;
		}


		//public float GetInterpolation_Int(int curKeyIndex)
		//{
		//	if(_curveKeyA == null || _curveKeyB == null)
		//	{
		//		return 0.0f;
		//	}

		//	if(_frameIndex_A == _frameIndex_B)
		//	{
		//		return 0.0f;
		//	}

		//	float keyRatio = (float)(curKeyIndex - _frameIndex_A) / (float)(_frameIndex_B - _frameIndex_A);
		//	float resultItp = 0.0f;
		//	switch (_tangentType)
		//	{
		//		case apAnimCurve.TANGENT_TYPE.Constant:
		//			if (keyRatio < 0.5f)
		//			{
		//				resultItp = 0.0f;
		//			}
		//			else
		//			{
		//				resultItp = 1.0f;
		//			}
		//			break;

		//		case apAnimCurve.TANGENT_TYPE.Linear:
		//			resultItp = keyRatio;
		//			break;

		//		case apAnimCurve.TANGENT_TYPE.Smooth:
		//			{
		//				int iParam = -1;
		//				// A ~ itp ~ B
		//				for (int i = 0; i < _smoothParams.Count; i++)
		//				{
		//					if(keyRatio == _smoothParams[i]._frameIndex && _smoothParams[i]._isFrameParam)
		//					{
		//						iParam = i;
		//						break;
		//					}
		//				}
		//				if(iParam < 0)
		//				{
		//					resultItp = 1.0f;
		//					break;
		//				}
		//				if(iParam == 0)
		//				{
		//					resultItp = 0.0f;
		//					break;
		//				}
		//				resultItp = _smoothParams[iParam]._yItp;
		//			}
		//			break;
		//	}
		//	if(_isTargetIsA)
		//	{
		//		resultItp = 1.0f - resultItp;
		//	}
		//	return resultItp;
		//}



		// Get / Set
		//---------------------------------------
		public apAnimCurve.TANGENT_TYPE CurveTangentType
		{
			get
			{
				//if(_curveKeyA == null || _curveKeyB == null)
				//{
				//	return apAnimCurve.TANGENT_TYPE.Linear;
				//}

				////Constant : 하나라도 Constant라면 성립 (가장 강력)
				////Linear : 둘다 Linear일 경우에만 (가장 약함)
				////Smooth : 그 나머지 모두

				//if(_curveKeyA._nextTangentType == apAnimCurve.TANGENT_TYPE.Constant || 
				//	_curveKeyB._prevTangentType == apAnimCurve.TANGENT_TYPE.Constant)
				//{
				//	return apAnimCurve.TANGENT_TYPE.Constant;
				//}

				//if(_curveKeyA._nextTangentType == apAnimCurve.TANGENT_TYPE.Linear &&
				//	_curveKeyB._prevTangentType == apAnimCurve.TANGENT_TYPE.Linear)
				//{
				//	return apAnimCurve.TANGENT_TYPE.Linear;
				//}

				//return apAnimCurve.TANGENT_TYPE.Smooth;
				return _tangentType;
			}
		}

		public void SetTangent(apAnimCurve.TANGENT_TYPE tangentType)
		{
			if (_curveKeyA == null || _curveKeyB == null)
			{
				return;
			}
			_curveKeyA._nextTangentType = tangentType;
			_curveKeyB._prevTangentType = tangentType;

			_curveKeyA.Refresh();
			_curveKeyB.Refresh();

			MakeCurve();
		}

		public void SetCurvePreset_Default()
		{
			if (_curveKeyA == null || _curveKeyB == null || _tangentType != apAnimCurve.TANGENT_TYPE.Smooth)
			{
				return;
			}
			_curveKeyA._nextSmoothX = apAnimCurve.CONTROL_POINT_X_OFFSET;
			_curveKeyA._nextSmoothY = 0.0f;

			_curveKeyB._prevSmoothX = apAnimCurve.CONTROL_POINT_X_OFFSET;
			_curveKeyB._prevSmoothY = 0.0f;

			_curveKeyA.Refresh();
			_curveKeyB.Refresh();

			MakeCurve();
		}

		public void SetCurvePreset_Hard()
		{
			if (_curveKeyA == null || _curveKeyB == null || _tangentType != apAnimCurve.TANGENT_TYPE.Smooth)
			{
				return;
			}

			_curveKeyA._nextSmoothX = 1.0f;
			_curveKeyA._nextSmoothY = 0.0f;

			_curveKeyB._prevSmoothX = 1.0f;
			_curveKeyB._prevSmoothY = 0.0f;

			_curveKeyA.Refresh();
			_curveKeyB.Refresh();

			MakeCurve();
		}

		public void SetCurvePreset_Acc()
		{
			if (_curveKeyA == null || _curveKeyB == null || _tangentType != apAnimCurve.TANGENT_TYPE.Smooth)
			{
				return;
			}

			_curveKeyA._nextSmoothX = 0.8f;
			_curveKeyA._nextSmoothY = 0.0f;

			_curveKeyB._prevSmoothX = 0.0f;
			_curveKeyB._prevSmoothY = 0.8f;

			_curveKeyA.Refresh();
			_curveKeyB.Refresh();

			MakeCurve();
		}

		public void SetCurvePreset_Dec()
		{
			if (_curveKeyA == null || _curveKeyB == null || _tangentType != apAnimCurve.TANGENT_TYPE.Smooth)
			{
				return;
			}

			_curveKeyA._nextSmoothX = 0.0f;
			_curveKeyA._nextSmoothY = 0.8f;

			_curveKeyB._prevSmoothX = 0.8f;
			_curveKeyB._prevSmoothY = 0.0f;

			_curveKeyA.Refresh();
			_curveKeyB.Refresh();

			MakeCurve();
		}

		// 계산용 Static 함수. 에디터에서만 사용하자
		//-------------------------------------------------------------------------------------
		/// <summary>
		/// CurveKey 두개에 대한 임시 보간을 계산한다.
		/// 애니메이션 프레임을 직접 넣어줘야 한다.
		/// ITP 값은 B에 대한 값이 나온다. A는 (1-Result) 를 하자
		/// 0이면 A에 가까운 값. 1이면 B에 가까운 값
		/// </summary>
		/// <param name="curKeyIndex"></param>
		/// <param name="frameIndex_A"></param>
		/// <param name="frameIndex_B"></param>
		/// <param name="curveA"></param>
		/// <param name="curveB"></param>
		/// <returns></returns>
		public static float CalculateInterpolation_Float(	float curKeyIndex, int iCurKeyframe,
															int frameIndex_A, int frameIndex_B, 
															apAnimCurve curveA, apAnimCurve curveB)
		{
			if (frameIndex_A == frameIndex_B)
			{
				return 0.0f;
			}

			float keyRatio = Mathf.Clamp01((float)(curKeyIndex - (float)frameIndex_A) / (float)(frameIndex_B - frameIndex_A));

			float resultItp = 0.0f;


			apAnimCurve.TANGENT_TYPE tangentType = apAnimCurve.TANGENT_TYPE.Linear;

			if (curveA._nextTangentType == apAnimCurve.TANGENT_TYPE.Constant ||
				curveB._prevTangentType == apAnimCurve.TANGENT_TYPE.Constant)
			{
				tangentType = apAnimCurve.TANGENT_TYPE.Constant;
			}
			else if (curveA._nextTangentType == apAnimCurve.TANGENT_TYPE.Linear &&
				curveB._prevTangentType == apAnimCurve.TANGENT_TYPE.Linear)
			{
				tangentType = apAnimCurve.TANGENT_TYPE.Linear;
			}
			else
			{
				tangentType = apAnimCurve.TANGENT_TYPE.Smooth;
			}


			float smoothX_A = curveA._nextSmoothX;
			float smoothY_A = curveA._nextSmoothY;

			float smoothX_B = curveB._prevSmoothX;
			float smoothY_B = curveB._prevSmoothY;


			switch (tangentType)
			{
				case apAnimCurve.TANGENT_TYPE.Constant:
					//변경전 : 0.5를 기준으로 바뀜
					//변경후 : 1을 기준으로 바뀜 (int형 키프레임 받아와야함)
					//if (keyRatio < 0.5f)
					//{
					//	resultItp = 0.0f;
					//}
					//else
					//{
					//	resultItp = 1.0f;
					//}
					if(iCurKeyframe < frameIndex_B)
					{
						resultItp = 0.0f;
					}
					else
					{
						resultItp = 1.0f;
					}
					break;

				case apAnimCurve.TANGENT_TYPE.Linear:
					resultItp = keyRatio;
					break;

				case apAnimCurve.TANGENT_TYPE.Smooth:
					{
						float cpXRatio = 0.3f;
						Vector2 controlPointA = new Vector2(Mathf.Clamp01(smoothX_A), Mathf.Clamp01(smoothY_A));
						Vector2 controlPointB = new Vector2(Mathf.Clamp01(1.0f - smoothX_B), Mathf.Clamp01(1.0f - smoothY_B));

						float revT = 1.0f - keyRatio;
						float linearItp = Mathf.Clamp01(
							((1.0f - Mathf.Sqrt(smoothX_A * smoothX_A + smoothY_A * smoothY_A)) * revT 
							+ (1.0f - Mathf.Sqrt(smoothX_B * smoothX_B + smoothY_B * smoothY_B)) * keyRatio)
							);

						float convertTItp = (0.0f * revT * revT * revT) +
											(3.0f * ((1.0f - controlPointA.x) * cpXRatio) * revT * revT * keyRatio) +
											(3.0f * (1.0f - (controlPointB.x * cpXRatio)) * revT * keyRatio * keyRatio) +
											(1.0f * keyRatio * keyRatio * keyRatio);

						//float convertTItp = (0.0f * revT * revT * revT) +
						//					(3.0f * (controlPointA.x) * revT * revT * keyRatio) +
						//					(3.0f * (controlPointB.x) * revT * keyRatio * keyRatio) +
						//					(1.0f * keyRatio * keyRatio * keyRatio);

						convertTItp = convertTItp * (1.0f - linearItp) + keyRatio * linearItp;

						resultItp = GetSmoothItp(controlPointA.y, controlPointB.y, convertTItp);
						resultItp = resultItp * (1.0f - linearItp) + convertTItp * linearItp;
						
					}
					break;
			}
			
			return resultItp;
		}

	}

}