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
	/// 애니메이션 및 컨트롤러 보간에서 사용하는 보간용 커브
	/// 값 자체는 커브에 해당하는 한쪽의 키값을 저장한다.
	/// Static 함수나 멤버 함수로 "다른 키와의 보간값"을 리턴해준다.
	/// </summary>
	[Serializable]
	public class apAnimCurve
	{
		// Members
		//-------------------------------------------------
		/// <summary>
		/// 한쪽의 사이드에 대한 보간 탄젠트 타입
		/// </summary>
		public enum TANGENT_TYPE
		{
			/// <summary>A->B가 직선으로 이어지며 컨트롤 포인트가 사라진다.</summary>
			Linear = 0,
			/// <summary>A->B에 컨트롤 포인트가 포함된다. 커브 형태로 생성된다.</summary>
			Smooth = 1,
			/// <summary>A.... > B 방식으로 A가 유지되다가 B로 갑자기 바뀐다. 한쪽이 Constant면 무조건 Constant가 된다.</summary>
			Constant = 2
		}

		//Curve 자체에
		//  <-   ㅁ   -> 양쪽에 대한 값을 가진다.
		//(Prev) ㅁ (Next)

		//컨트롤 포인트의 값
		//X ( 연결된 다른 Curve 까지의 차이에 대한 비율 (0~1). 기본값 0.3 )
		//Y ( 기본값은 0. 0일수록 해당 ITP의 결과에 가깝게 되고, 1이면 연결된 다른 Curve Point 


		[SerializeField]
		public TANGENT_TYPE _prevTangentType = TANGENT_TYPE.Smooth;

		public float _prevSmoothX = 0.3f;
		public float _prevSmoothY = 0.0f;

		[NonSerialized]
		public apAnimCurve _prevLinkedCurveKey = null;

		//[NonSerialized]
		//public bool _isPrevKeyUseDummyIndex = false;//<Prev CurveKey가 더미인덱스를 사용하는가


		[SerializeField]
		public TANGENT_TYPE _nextTangentType = TANGENT_TYPE.Smooth;

		public float _nextSmoothX = 0.3f;
		public float _nextSmoothY = 0.0f;



		#region [미사용 코드]
		//public Vector4 _nextSmoothAngle = Vector4.zero;
		//public Vector4 _nextSmoothYOffset = Vector4.zero;
		//public float _nextDeltaX = 1.0f;
		//public Vector4 _nextDeltaY = Vector4.zero; 
		#endregion

		[NonSerialized]
		public apAnimCurve _nextLinkedCurveKey = null;

		#region [미사용 코드]
		//[NonSerialized]
		//public bool _isNextKeyUseDummyIndex = false;

		//[SerializeField]
		//public KEY_ITP_TYPE _keyItpType = KEY_ITP_TYPE.AutoSmooth; 
		#endregion

		[SerializeField]
		public int _keyIndex = 0;

		//[SerializeField]
		//public float _dummyKeyIndex = 0.0f;

		//변경
		//기존의 인덱스를 [기본] / [더미]로 쓰던걸
		//그냥 Prev / Index / Next로 바꾸고
		//그 값을 바로 쓸 수 있도록 외부에서 설정해서 가져올 것
		[SerializeField]
		public int _prevIndex = 0;

		[SerializeField]
		public int _nextIndex = 0;


		#region [미사용 코드]
		//[SerializeField]
		//public Vector4 _keyValue = Vector4.zero;

		//[SerializeField]
		//public int _dimension = 1;

		//추가
		//Morph같이 다중 키값이 들어오면 보간시 "데이터 값"을 정의할 수 없다.
		//public bool _isRelativeKeyValue = false;

		//이 경우는 Smooth 값은 별도로 가진다. (Weight 형식으로 가지며 0 ~ 0.2의 값을 가진다.)
		//여기선 Smooth Weight만 처리한다.
		//public float _smoothRelativeWeight = 0.0f;


		//[SerializeField]
		//private float _loopSize = 0.0f;


		#endregion


		//public const float CONTROL_POINT_X_OFFSET = 0.3f;
		//public const float CONTROL_POINT_X_OFFSET = 1.0f;//꽤 부드러운 커브를 기본값으로 한다.
		public const float CONTROL_POINT_X_OFFSET = 0.5f;//1로 하니 너무 빡세게 부드럽다. 절충하자
		
		public const float MIN_DELTA_X = 0.0001f;
		//public const float MIN_ANGLE = -89.5f;
		//public const float MAX_ANGLE = 89.5f;

		public const float MIN_RELATIVE_WEIGHT = 0.0f;
		public const float MAX_RELATIVE_WEIGHT = 0.2f;


		public enum KEY_POS
		{
			PREV = 0, NEXT = 1
		}

		//변경 19.5.22 : 용량 최적화를 위해서 AnimCurveResult를 NonSerialized로 변경
		//[SerializeField]
		[NonSerialized, NonBackupField]
		public apAnimCurveResult _prevCurveResult = new apAnimCurveResult();

		//[SerializeField]
		[NonSerialized, NonBackupField]
		public apAnimCurveResult _nextCurveResult = new apAnimCurveResult();

		//[NonSerialized]
		//private apAnimCurveTable _prevTable = null;

		//[NonSerialized]
		//private apAnimCurveTable _nextTable = null;

		

		// Init
		//-------------------------------------------------
		public apAnimCurve()
		{
			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}
			Init();
		}


		public apAnimCurve(apAnimCurve srcCurve, int keyIndex)
		{
			_prevTangentType = srcCurve._prevTangentType;

			_prevSmoothX = srcCurve._prevSmoothX;
			_prevSmoothY = srcCurve._prevSmoothY;

			#region [미사용 코드]
			//_prevSmoothAngle = srcCurve._prevSmoothAngle;
			//_prevSmoothYOffset = srcCurve._prevSmoothYOffset;
			//_prevDeltaX = srcCurve._prevDeltaX;
			//_prevDeltaY = srcCurve._prevDeltaY; 
			#endregion

			_prevLinkedCurveKey = srcCurve._prevLinkedCurveKey;
			_nextTangentType = srcCurve._nextTangentType;

			_nextSmoothX = srcCurve._nextSmoothX;
			_nextSmoothY = srcCurve._nextSmoothY;

			#region [미사용 코드]
			//_nextSmoothAngle = srcCurve._nextSmoothAngle;
			//_nextSmoothYOffset = srcCurve._nextSmoothYOffset;
			//_nextDeltaX = srcCurve._nextDeltaX;
			//_nextDeltaY = srcCurve._nextDeltaY; 
			#endregion

			_nextLinkedCurveKey = srcCurve._nextLinkedCurveKey;
			//_keyItpType = srcCurve._keyItpType;

			_keyIndex = keyIndex;//<키 인덱스만 따로 분리해서 복사한다.

			_prevIndex = keyIndex;
			_nextIndex = keyIndex;

			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}
			#region [미사용 코드]
			//_keyValue = srcCurve._keyValue;

			//_dimension = srcCurve._dimension;
			//_isRelativeKeyValue = srcCurve._isRelativeKeyValue;

			//_smoothRelativeWeight = srcCurve._smoothRelativeWeight; 
			#endregion
		}


		public apAnimCurve(apAnimCurve srcCurve_Prev, apAnimCurve srcCurve_Next, int keyIndex)
		{
			//Prev는 Next의 Prev를 사용한다.
			_prevTangentType = srcCurve_Next._prevTangentType;

			_prevSmoothX = srcCurve_Next._prevSmoothX;
			_prevSmoothY = srcCurve_Next._prevSmoothY;

			

			_prevLinkedCurveKey = srcCurve_Next._prevLinkedCurveKey;

			//Next는 Prev의 Next를 사용한다.
			_nextTangentType = srcCurve_Prev._nextTangentType;

			_nextSmoothX = srcCurve_Prev._nextSmoothX;
			_nextSmoothY = srcCurve_Prev._nextSmoothY;

			

			_nextLinkedCurveKey = srcCurve_Prev._nextLinkedCurveKey;
			//_keyItpType = srcCurve._keyItpType;

			_keyIndex = keyIndex;//<키 인덱스만 따로 분리해서 복사한다.

			_prevIndex = keyIndex;
			_nextIndex = keyIndex;

			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}
			
		}

		public void Init()
		{

			//_keyItpType = KEY_ITP_TYPE.AutoSmooth;
			_prevTangentType = TANGENT_TYPE.Smooth;
			_nextTangentType = TANGENT_TYPE.Smooth;

			_prevSmoothX = CONTROL_POINT_X_OFFSET;
			_prevSmoothY = 0.0f;

			_nextSmoothX = CONTROL_POINT_X_OFFSET;
			_nextSmoothY = 0.0f;

			#region [미사용 코드]
			//SetSmoothAngle(Vector4.zero, KEY_POS.PREV);
			//SetSmoothAngle(Vector4.zero, KEY_POS.NEXT); 
			#endregion

			_prevLinkedCurveKey = null;
			_nextLinkedCurveKey = null;

			#region [미사용 코드]
			//_prevDeltaX = 1.0f;
			//_prevDeltaY = Vector4.zero;

			//_nextDeltaX = 1.0f;
			//_nextDeltaY = Vector4.zero;

			//_isRelativeKeyValue = false;
			//_smoothRelativeWeight = MIN_RELATIVE_WEIGHT; 
			#endregion

			//_prevTable = new apAnimCurveTable(this);
			//_nextTable = new apAnimCurveTable(this);

			if (_prevCurveResult == null)
			{
				_prevCurveResult = new apAnimCurveResult();
			}
			if (_nextCurveResult == null)
			{
				_nextCurveResult = new apAnimCurveResult();
			}

			
		}



		// Functions
		//-------------------------------------------------

		// Set Key Value (값 타입에 맞게 오버로드)
		//-------------------------------------------------------------------------------
		#region [미사용 코드]
		//public void SetLoopSize(int loopSize)
		//{
		//	_loopSize = loopSize;
		//} 
		#endregion

		/// <summary>
		/// 이 커브 정보가 입력될 위치값을 지정한다(키프레임)
		/// </summary>
		/// <param name="keyIndex"></param>
		public void SetKeyIndex(int keyIndex)
		{
			_keyIndex = keyIndex;
			//_dummyKeyIndex = dummyKeyIndex;

			Refresh();
		}

		/// <summary>
		/// Curve 정보가 바뀌면 항상 호출해야하는 함수
		/// 보간 처리에 필요한 Table을 갱신한다.
		/// </summary>
		public void Refresh()
		{
			//_prevTable.MakeTable();
			//_nextTable.MakeTable();
			_prevCurveResult.MakeCurve();
			_nextCurveResult.MakeCurve();
		}



		//-------------------------------------------------------------------------------
		public void SetLinkedIndex(int prevIndex, int nextIndex)
		{
			_prevIndex = prevIndex;
			_nextIndex = nextIndex;

			Refresh();
		}

		public void SetLinkedCurveKey(apAnimCurve prevLinkedCurveKey, apAnimCurve nextLinkedCurveKey,
										int prevIndex, int nextIndex
			
										//bool isMakeCurveForce = false//<< 삭제 19.5.20 : 이게 문제가 아닌데??
									//, bool isPrevDummyIndex, bool isNextDummyIndex
									)
		{
			_prevLinkedCurveKey = prevLinkedCurveKey;
			_nextLinkedCurveKey = nextLinkedCurveKey;

			_prevIndex = prevIndex;
			_nextIndex = nextIndex;

			#region [미사용 코드]
			//_isPrevKeyUseDummyIndex = isPrevDummyIndex;
			//_isNextKeyUseDummyIndex = isNextDummyIndex;

			//Delta값은 항상 B-A 방식으로 한다.

			//float prevKeyIndex = _keyIndex;
			//float nextKeyIndex = _keyIndex;

			//if (_prevLinkedCurveKey == null)
			//{
			//	_prevDeltaX = 1.0f;
			//	_prevDeltaY = Vector4.zero;
			//}
			//else
			//{
			//	//prevKeyIndex = _prevLinkedCurveKey.GetIndex(_isPrevKeyUseDummyIndex);

			//	_prevDeltaX = (_keyIndex - _prevIndex);
			//	_prevDeltaY = (_keyValue - _prevLinkedCurveKey._keyValue);
			//}


			//if(_nextLinkedCurveKey == null)
			//{
			//	_nextDeltaX = 1.0f;
			//	_nextDeltaY = Vector4.zero;
			//}
			//else
			//{
			//	//nextKeyIndex = _nextLinkedCurveKey.GetIndex(_isNextKeyUseDummyIndex);

			//	//_nextDeltaX = (nextKeyIndex - _keyIndex);
			//	_nextDeltaX = (_nextIndex - _keyIndex);
			//	_nextDeltaY = (_nextLinkedCurveKey._keyValue - _keyValue);
			//}

			//_prevDeltaX = Mathf.Max(_prevDeltaX, MIN_DELTA_X);
			//_nextDeltaX = Mathf.Max(_nextDeltaX, MIN_DELTA_X);

			//각도를 가지고 YOffset을 다시 계산하자
			//CalculateSmooth(); 
			#endregion

			//이전
			//_prevCurveResult.Link(prevLinkedCurveKey, this, false, !(Application.isPlaying) || isMakeCurveForce);
			//_nextCurveResult.Link(this, nextLinkedCurveKey, true, !(Application.isPlaying) || isMakeCurveForce);

			//변경 : 19.5.20 : 최적화 작업
			_prevCurveResult.Link(prevLinkedCurveKey, this, false);
			_nextCurveResult.Link(this, nextLinkedCurveKey, true);

			//Debug.Log("Key 연결 : Prev : " + prevIndex + " | Next : " + nextIndex);

			
		}




		public void SetTangentType(TANGENT_TYPE tangentType, KEY_POS keyPos)
		{
			if (keyPos == KEY_POS.PREV)
			{
				//Prev
				_prevTangentType = tangentType;
			}
			else
			{
				//Next
				_nextTangentType = tangentType;
			}

			Refresh();
		}

		// Get / Set
		//-------------------------------------------------
		public TANGENT_TYPE PrevTangent
		{
			get
			{
				return _prevTangentType;
			}
		}

		public TANGENT_TYPE NextTangent
		{
			get
			{
				return _nextTangentType;
				//if(_keyItpType == KEY_ITP_TYPE.Broken)	{ return ; }
				//else									{ return TANGENT_TYPE.Smooth; }
			}
		}

		public float GetItp_Float(float curKeyframe, bool isWithPrevKey, int iCurKeyframe)
		{
			if (isWithPrevKey)
			{
				return _prevCurveResult.GetInterpolation(curKeyframe, iCurKeyframe);
			}
			else
			{
				return _nextCurveResult.GetInterpolation(curKeyframe, iCurKeyframe);
			}
		}

		public float GetItp_Int(int curKeyframe, bool isWithPrevKey)
		{
			if (isWithPrevKey)
			{
				//return _prevCurveResult.GetInterpolation_Int(curKeyframe);
				return _prevCurveResult.GetInterpolation((float)curKeyframe, curKeyframe);
			}
			else
			{
				//return _nextCurveResult.GetInterpolation_Int(curKeyframe);
				return _nextCurveResult.GetInterpolation((float)curKeyframe, curKeyframe);
			}
		}

		//private static float _tmpDeltaX = 0.0f;
		//private static float _tmpItp = 0.0f;
		private static float _tmpRevItp = 0.0f;

		//private static float _tmpT = 0.0f;
		//private static float _tmpRevT = 0.0f;

		//private static apAnimCurve _tmpKeyA = null;
		//private static apAnimCurve _tmpKeyB = null;

		


		/// <summary>
		/// 일반적으로 부드러운 Interpolation 변형 (Key 없이 사용할 수 있다)
		/// [A의 입장에서 Interpolation을 꺼내고자 한다면 (1-결과값)을 해야한다.]
		/// </summary>
		/// <param name="itp"></param>
		/// <returns></returns>
		public static float GetSmoothInterpolation(float itp)
		{
			_tmpRevItp = 1.0f - itp;
			return  //0.0f * (_tmpRevItp * _tmpRevItp * _tmpRevItp) +
					//0.0f * (3.0f * _tmpRevItp * _tmpRevItp * itp) +
					1.0f * (3.0f * _tmpRevItp * itp * itp) +
					1.0f * (itp * itp * itp);
		}


	}
}