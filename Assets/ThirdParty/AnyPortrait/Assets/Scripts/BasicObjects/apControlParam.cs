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
	/// 키 프레임과 별도로 지정하는 값
	/// 이 값 자체는 헬퍼에 가깝다.
	/// 이 값에 의해 Morph와 Motion이 결정될 수 있다.
	/// 이 값이 KeyFrame에 저장될 수 있다.
	/// </summary>
	[Serializable]
	public class apControlParam
	{
		// Members
		//---------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		[Flags]
		public enum CATEGORY
		{
			Head = 1,
			Body = 2,
			Face = 4,
			Hair = 8,
			Equipment = 16,
			Force = 32,
			Etc = 64,
		}
		[SerializeField]
		public CATEGORY _category = CATEGORY.Etc;


		public enum ICON_PRESET
		{
			None = 0,
			Head = 1,
			Body = 2,
			Hand = 3,
			Face = 4,
			Eye = 5,
			Hair = 6,
			Equipment = 7,
			Cloth = 8,
			Force = 9,
			Etc = 10,

		}

		[SerializeField]
		public ICON_PRESET _iconPreset = ICON_PRESET.None;

		[SerializeField]
		public bool _isIconChanged = false;//이 값이 True이면 아이콘을 한번도 바꾼 적이 없다는 것

		//변경 : 키값으로 쓰기 위해선 무조건 Range여야 하고, Bool, Vector3, Color를 제외한다 <5.20>
		public enum TYPE
		{
			Int = 0,
			Float = 1,
			Vector2 = 2,
		}

		public TYPE _valueType = TYPE.Int;

		//값은 Min-Max-Default로 나뉜다.
		public int _int_Def = 0;
		public float _float_Def = 0.0f;
		public Vector2 _vec2_Def = Vector2.zero;

		public int _int_Min = 0;
		public int _int_Max = 0;

		public float _float_Min = 0;
		public float _float_Max = 0;

		public Vector2 _vec2_Min = Vector2.zero;
		public Vector2 _vec2_Max = Vector2.zero;

		//public Vector3 _vec3_Min = Vector3.zero;
		//public Vector3 _vec3_Max = Vector3.zero;

		//public bool _isRange = false;//<이건 필수로 바뀜
		public string _label_Min = "";
		public string _label_Max = "";
		//public string _label_Max3 = "";


		public int _uniqueID = -1;
		public string _keyName = "";

		public bool _isReserved = false;//예약된 것일 수 있다. 예약 값은 삭제할 수 없다.


		//추가
		//스냅 옵션을 주어서 GUI에서 Key를 선택하거나 생성할때 쉽게 하도록 한다.
		//1 => Min - Max
		//2 => Min - [0] - Max (0.5 간격)
		//..
		public int _snapSize = 4;//<기본값은 4이다.



		// 현재 값 (저장되지 않는다.)
		
		[NonSerialized]
		public int _int_Cur = 0;

		[NonSerialized]
		public float _float_Cur = 0;

		[NonSerialized]
		public Vector2 _vec2_Cur = Vector2.zero;


		// 외부에서 값을 제어할때 바로 Cur 값을 고칠게 아니라 별도의 변수를 두고 업데이트를 하자
		[NonSerialized]
		private bool _isExternalUpdate = false;

		[NonSerialized]
		private float _externalOverlapWeight = 0.0f;

		[NonSerialized]
		private int _int_Request = 0;

		[NonSerialized]
		private float _float_Request = 0.0f;

		[NonSerialized]
		private Vector2 _vec2_Request = Vector2.zero;

		//스크립트로 참조할때는 1Frame 이전의 값을 가져온다.
		//Request가 적용된 값이다.
		[NonSerialized]
		private int _int_PrevResult = 0;

		[NonSerialized]
		private float _float_PrevResult = 0.0f;

		[NonSerialized]
		private Vector2 _vec2_PrevResult = Vector2.zero;

		//Request를 받지 않은 Cur 값을 저장한다.
		[NonSerialized]
		private int _int_PrevResult_NoRequest = 0;

		[NonSerialized]
		private float _float_PrevResult_NoRequest = 0.0f;

		[NonSerialized]
		private Vector2 _vec2_PrevResult_NoRequest = Vector2.zero;




		//추가
		//Control Param을 애니메이션에서 업데이트 하는 경우에
		//애니메이션의 블렌딩 관련 처리를 할 때, 값을 지정하는 과정에서 "레이어"마다 중첩된 계산을 해야한다.
		//업데이트용 변수를 만들어서 "중첩 되는 값 처리"를 만들자
		//애니메이션이 아닌 API에 의한 값 변화는 직접 한다.

		/// <summary>
		/// 실시간 업데이트시, 블렌딩이 되는 상태이므로 처리가 시작 되었는지 여부가 중요하다.
		/// 한번이라도 처리가 되었다면 True (매 업데이트마다 False로 초기화를 하고 업데이트를 한다)
		/// </summary>
		[NonSerialized]
		private bool _isUpdated = false;

		//추가 22.1.9 : 블렌딩 버그 해결용 변수
		[NonSerialized]
		private bool _isUpdatedPrev = false;//이전 프레임에 업데이트를 하고 있었다면, 마지막 처리를 해야한다.

		[NonSerialized]
		private float _totalWeight = 0.0f;

		//[NonSerialized]
		//private bool _bool_CalculatedLayer = false;

		//이걸 레이어방식으로 만들자 20.4.19

		//기존
		//[NonSerialized]
		//private float _int_CalculatedLayer = 0.0f;//int에서 float로 변경 20.4.15

		[NonSerialized]
		private int _int_ClampedCalculatedLayer = 0;//추가 20.4.15 : _int_CalculatedLayer의 범위 Clamp된 버전이며 float > int로 변환된 계산 변수

		//[NonSerialized]
		//private float _float_CalculatedLayer = 0;

		//[NonSerialized]
		//private Vector2 _vec2_CalculatedLayer = Vector2.zero;

		//변형 20.4.19
		//애니메이션 레이어에 따라서 그룹을 나누고 따로 연산을 해야한다.
		public class LayeredRequest
		{
			//기본 데이터
			public int _layerIndex = 0;

			//입력된 데이터
			public List<apAnimControlParamResult> _params = new List<apAnimControlParamResult>();
			public int _nParams = 0;
			public float _totalWeight = 0.0f;
			public apAnimPlayUnit.BLEND_METHOD _blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation;

			public bool _isCalculated = false;

			private bool _isNeedToSort = false;
			private int _lastPlayOrder = -1;

			//계산값
			private float _cal_Value_1D = 0.0f;
			private Vector2 _cal_Value_2D = Vector2.zero;
			private apAnimControlParamResult _cal_CurParam = null;

			public LayeredRequest(int layer)
			{
				_layerIndex = layer;
				ReadyToUpdate();
			}

			public void ReadyToUpdate()
			{
				_params.Clear();
				_nParams = 0;
				_totalWeight = 0.0f;
				_blendMethod = apAnimPlayUnit.BLEND_METHOD.Interpolation;

				_isCalculated = false;

				_isNeedToSort = false;
				_lastPlayOrder = -1;
			}

			public void AddRequest(apAnimControlParamResult request)
			{
				if(!_isNeedToSort && _nParams > 0)
				{
					if(_lastPlayOrder >= request._animPlayOrder)
					{
						//오름차순으로 등록이 되어야 하는데, 그렇지 않다면 Sort를 해야한다.
						_isNeedToSort = true;
						//Debug.Log(">> Need to Sort");
					}
				}
				
				_params.Add(request);
				_nParams++;
				_isCalculated = true;
				_totalWeight += Mathf.Clamp01(request._animWeight);

				_lastPlayOrder = request._animPlayOrder;

				//Debug.Log(">> Add Request : " + _nParams);
			}

			public void CompleteCalculate()
			{
				if(_totalWeight > 1.0f)
				{
					//Normalize를 해야한다.
					float normalRatio = 1.0f / _totalWeight;

					for (int i = 0; i < _nParams; i++)
					{
						_params[i]._animWeight *= normalRatio;
					}

					_totalWeight = 1.0f;
				}

				if(_isNeedToSort)
				{
					_params.Sort(delegate(apAnimControlParamResult a, apAnimControlParamResult b)
					{
						return a._animPlayOrder - b._animPlayOrder;
					});
				}

				//첫번째 애니메이션의 블렌드 타입이 레이어의 블렌드 타입이다.
				_blendMethod = _params[0]._animBlendMethod;
			}

			/// <summary>Int형 값(중간 계산값이므로 Float)을 계산하여 리턴한다.</summary>
			public float GetCalculated_Int()
			{
				//첫번째는 그대로 할당하고, 그 다음부터 Interpolation한다.
				_cal_CurParam = _params[0];
				_cal_Value_1D = _cal_CurParam._value_Int;

				//string strDebug = "GetCalculated Int :\n" + "0 : " + _cal_CurParam._value_Int;
				
				if (_nParams > 1)
				{
					for (int i = 1; i < _params.Count; i++)
					{
						_cal_CurParam = _params[i];
						_cal_Value_1D = (_cal_Value_1D * (1.0f - _cal_CurParam._animWeight))
							+ (_cal_CurParam._value_Int * _cal_CurParam._animWeight);

						//strDebug += "\n" + i + " : " + _cal_CurParam._value_Int + "( " + _cal_CurParam._animWeight + ") > " + _cal_Value_1D;
					}
				}

				//strDebug += "\n >>> Result : " + _cal_Value_1D + " (" + _nParams + ")";
				//Debug.LogError(strDebug);

				return _cal_Value_1D;
			}

			/// <summary>
			/// Float형 값을 리턴한다.
			/// </summary>
			/// <returns></returns>
			public float GetCalculated_Float()
			{
				//첫번째는 그대로 할당하고, 그 다음부터 Interpolation한다.
				_cal_CurParam = _params[0];
				_cal_Value_1D = _cal_CurParam._value_Float;

				if (_nParams > 1)
				{
					for (int i = 1; i < _params.Count; i++)
					{
						_cal_CurParam = _params[i];
						_cal_Value_1D = (_cal_Value_1D * (1.0f - _cal_CurParam._animWeight))
							+ (_cal_CurParam._value_Float * _cal_CurParam._animWeight);
					}
				}

				return _cal_Value_1D;
			}

			/// <summary>
			/// Vector2형 값을 리턴한다.
			/// </summary>
			/// <returns></returns>
			public Vector2 GetCalculated_Vector2()
			{
				//첫번째는 그대로 할당하고, 그 다음부터 Interpolation한다.
				_cal_CurParam = _params[0];
				_cal_Value_2D = _cal_CurParam._value_Vec2;

				if (_nParams > 1)
				{
					for (int i = 1; i < _params.Count; i++)
					{
						_cal_CurParam = _params[i];
						
						_cal_Value_2D = (_cal_Value_2D * (1.0f - _cal_CurParam._animWeight))
										+ (_cal_CurParam._value_Vec2 * _cal_CurParam._animWeight);
					}
				}

				return _cal_Value_2D;
			}
		}

		[NonSerialized]
		private List<LayeredRequest> _layeredRequests = new List<LayeredRequest>();//풀의 역할을 하는 Request

		[NonSerialized]
		private LayeredRequest _curLayeredRequest = null;

		//레이어의 중간 계산 결과들
		//Int, Float, Vec2 모두 Float 형태로 중간 값을 계산하자
		[NonSerialized]
		private float _cal_Value_1D = 0.0f;

		[NonSerialized]
		private Vector2 _cal_Value_2D = Vector2.zero;




		[NonSerialized]
		private float _itpDistRange_Min = 0.0f;

		[NonSerialized]
		private float _itpDistRange_Max = 0.0f;

		private bool _isItpDistRange = false;

		//추가 22.5.16
		//애니메이션 전환시 기본적으로는 "Default"값을 사용한다.
		//그렇지만 이 옵션이 true라면, 저장되었던 마지막 연산 값을 베이스 값으로 사용한다.
		[NonSerialized]
		private bool _isKeepLastValueInAnimTransition = false;

		//마지막 Anim 결과값
		[NonSerialized] private int _lastValue_Int = 0;
		[NonSerialized] private float _lastValue_Float = 0.0f;
		[NonSerialized] private Vector2 _lastValue_Vec2 = Vector2.zero;



		//--------------------------------------------
		/// <summary>
		/// 백업용 생성자. 코드에서 호출하지는 말자
		/// </summary>
		public apControlParam()
		{

		}


		public apControlParam(int uniqueID, string keyName, bool isReserved, CATEGORY category)
		{
			_keyName = keyName;
			_isReserved = isReserved;
			_category = category;
			_uniqueID = uniqueID;

			//초기값 설정
			_valueType = TYPE.Float;
			_int_Def = 0;
			_float_Def = 0.0f;
			_vec2_Def = Vector2.zero;

			_int_Min = -1;
			_int_Max = 1;

			_float_Min = -1;
			_float_Max = 1;

			_vec2_Min = new Vector2(-1, -1);
			_vec2_Max = new Vector2(1, 1);


			_label_Min = "Label1";
			_label_Max = "Label2";

			_snapSize = 4;

			MakeInterpolationRange();
		}


		public void Ready(apPortrait portrait)
		{
			_portrait = portrait;
			//_portrait.RegistUniqueID_ControlParam(_uniqueID);
			_portrait.RegistUniqueID(apIDManager.TARGET.ControlParam, _uniqueID);

			CheckAndCorrectDefaultInRange();//추가 20.11.7

			MakeInterpolationRange();
		}


		/// <summary>
		/// 추가 20.11.7 : Default 값이 Range 안에 있는지 확인하고, 그렇지 않다면 가까운 값으로 변경한다.
		/// </summary>
		public void CheckAndCorrectDefaultInRange()
		{
			switch (_valueType)
			{
				case TYPE.Int:
					{
						if (_int_Min > _int_Max)
						{
							//Range가 잘못되었다.
							int nextMin = Mathf.Min(_int_Min, _int_Max);
							int nextMax = Mathf.Max(_int_Min, _int_Max);
							_int_Min = nextMin;
							_int_Max = nextMax;
						}

						_int_Def = Mathf.Clamp(_int_Def, _int_Min, _int_Max);
					}
					break;


				case TYPE.Float:
					{
						if (_float_Min > _float_Max)
						{
							//Range가 잘못되었다.
							float nextMin = Mathf.Min(_float_Min, _float_Max);
							float nextMax = Mathf.Max(_float_Min, _float_Max);
							_float_Min = nextMin;
							_float_Max = nextMax;
						}

						_float_Def = Mathf.Clamp(_float_Def, _float_Min, _float_Max);
					}
					break;

				case TYPE.Vector2:
					{
						if (_vec2_Min.x > _vec2_Max.x)
						{
							//Range (X)가 잘못되었다.
							float nextMin = Mathf.Min(_vec2_Min.x, _vec2_Max.x);
							float nextMax = Mathf.Max(_vec2_Min.x, _vec2_Max.x);
							_vec2_Min.x = nextMin;
							_vec2_Max.x = nextMax;
						}
						if (_vec2_Min.y > _vec2_Max.y)
						{
							//Range (Y)가 잘못되었다.
							float nextMin = Mathf.Min(_vec2_Min.y, _vec2_Max.y);
							float nextMax = Mathf.Max(_vec2_Min.y, _vec2_Max.y);
							_vec2_Min.y = nextMin;
							_vec2_Max.y = nextMax;
						}

						_vec2_Def.x = Mathf.Clamp(_vec2_Def.x, _vec2_Min.x, _vec2_Max.x);
						_vec2_Def.y = Mathf.Clamp(_vec2_Def.y, _vec2_Min.y, _vec2_Max.y);
					}
					break;
			}
			
		}


		/// <summary>
		/// 이 함수를 호출해야 적절한 보간 처리가 가능해진다.
		/// 파라미터 설정이 바뀌면 한번씩 호출해주자
		/// </summary>
		public void MakeInterpolationRange()
		{
			//if(!_isRange)
			//{
			//	_itpDistRange_Min = 0.0f;
			//	_itpDistRange_Max = 0.0f;
			//	_isItpDistRange = false;
			//	return;
			//}

			//Range Min : Dist가 이 값 이내면 Weight 초기값은 1로 시작한다.
			//Range Max : Dist가 Min-Max 사이면 Weight 초기값은 0 (Max) ~ 1 (Min) 사이 값으로 시작한다.
			//Range Max 이상 : Weight가 0부터 시작. 보간이 불가능한 거리이다.

			//적정 거리는 Def 값으로 부터 끝값 (축 대각선)까지의 거리.
			//Min은 적정 거리의 70%. Max는 적정 거리의 120%

			_isItpDistRange = true;

			float properDist = 0.0f;
			switch (_valueType)
			{
				//case TYPE.Bool:
				//	properDist = 1.0f;
				//	_isItpDistRange = false;
				//	break;

				case TYPE.Int:
					{
						properDist = Mathf.Max(Mathf.Abs(_int_Def - _int_Min), Mathf.Abs(_int_Def - _int_Max));
						if (_int_Max > _int_Min)
						{
							properDist /= (float)(_int_Max - _int_Min);
						}
						else
						{
							properDist = 0.0f;
						}
					}
					break;

				case TYPE.Float:
					{
						properDist = Mathf.Max(Mathf.Abs(_float_Def - _float_Min), Mathf.Abs(_float_Def - _float_Max));
						float size = _float_Max - _float_Min;
						if (size > 0.0f)
						{
							properDist /= size;
						}
						else
						{
							properDist = 0.0f;
						}
					}
					break;

				case TYPE.Vector2:
					{
						float distX = Mathf.Max(Mathf.Abs(_vec2_Def.x - _vec2_Min.x), Mathf.Abs(_vec2_Def.x - _vec2_Max.x));
						float distY = Mathf.Max(Mathf.Abs(_vec2_Def.y - _vec2_Min.y), Mathf.Abs(_vec2_Def.y - _vec2_Max.y));

						float sizeX = _vec2_Max.x - _vec2_Min.x;
						float sizeY = _vec2_Max.y - _vec2_Min.y;

						if (sizeX > 0.0f)
						{ distX /= sizeX; }
						else
						{ distX = 0.0f; }

						if (sizeY > 0.0f)
						{ distY /= sizeY; }
						else
						{ distY = 0.0f; }

						//properDist = Mathf.Sqrt((distX * distX) + (distY * distY));
						properDist = Mathf.Max(distX, distY);
					}
					break;

					//case TYPE.Vector3:
					//	{
					//		float distX = Mathf.Max(Mathf.Abs(_vec3_Def.x - _vec3_Min.x), Mathf.Abs(_vec3_Def.x - _vec3_Max.x));
					//		float distY = Mathf.Max(Mathf.Abs(_vec3_Def.y - _vec3_Min.y), Mathf.Abs(_vec3_Def.y - _vec3_Max.y));
					//		float distZ = Mathf.Max(Mathf.Abs(_vec3_Def.z - _vec3_Min.z), Mathf.Abs(_vec3_Def.z - _vec3_Max.z));

					//		float sizeX = _vec3_Max.x - _vec3_Min.x;
					//		float sizeY = _vec3_Max.y - _vec3_Min.y;
					//		float sizeZ = _vec3_Max.z - _vec3_Min.z;

					//		if(sizeX > 0.0f)	{ distX /= sizeX; }
					//		else				{ distX = 0.0f; }

					//		if(sizeY > 0.0f)	{ distY /= sizeY; }
					//		else				{ distY = 0.0f; }

					//		if(sizeZ > 0.0f)	{ distZ /= sizeZ; }
					//		else				{ distZ = 0.0f; }

					//		properDist = Mathf.Sqrt((distX * distX) + (distY * distY) + (distZ * distZ));
					//	}
					//	break;

					//case TYPE.Color:
					//	properDist = 0.5f;
					//	_isItpDistRange = false;
					//	break;
			}

			//제곱식인 경우
			//_itpDistRange_Min = properDist * 0.4f;
			//_itpDistRange_Max = properDist * 1.0f;

			//축 크기를 사용할 경우
			_itpDistRange_Min = properDist * 0.7f;
			_itpDistRange_Max = properDist * 1.2f;

			//Range에 큰 차이가 없다면..
			if (_itpDistRange_Max - _itpDistRange_Min < 0.0001f)
			{
				_isItpDistRange = false;
			}
		}

		/// <summary>
		/// 거리에 의한 기본 보간 시작 가중치를 리턴한다.
		/// </summary>
		/// <param name="dist"></param>
		/// <returns></returns>
		public float GetInterpolationWeight(float dist)
		{
			if (!_isItpDistRange)
			{
				return 1.0f;
			}

			if (dist < _itpDistRange_Min)
			{
				return 1.0f;
			}
			if (dist < _itpDistRange_Max)
			{
				return 1.0f - apAnimCurve.GetSmoothInterpolation(((dist - _itpDistRange_Min) / (_itpDistRange_Max - _itpDistRange_Min)));
				//return 1.0f - ((dist - _itpDistRange_Min) / (_itpDistRange_Max - _itpDistRange_Min));
			}
			return 0.0f;

		}

		// Default
		//--------------------------------------------
		public void SetDefault()
		{
			//_bool_Cur = _bool_Def;
			_int_Cur = _int_Def;
			_float_Cur = _float_Def;
			_vec2_Cur = _vec2_Def;
			//_vec3_Cur = _vec3_Def;
			//_color_Cur = _color_Def;

			MakeInterpolationRange();
		}







		// Int
		//---------------------------------------------
		public void SetInt(int defaultValue)
		{
			_valueType = TYPE.Int;
			_int_Def = defaultValue;
			_int_Cur = defaultValue;
			//_isRange = false;

			_int_Min = _int_Def - 1;
			_int_Max = _int_Def + 1;

			MakeInterpolationRange();
		}

		public void SetInt(int defaultValue, int min, int max, string label_Min, string label_Max)
		{
			_valueType = TYPE.Int;
			_int_Def = defaultValue;
			_int_Cur = defaultValue;

			//_isRange = true;
			_int_Min = Mathf.Min(min, max);
			_int_Max = Mathf.Max(min, max);

			if (string.IsNullOrEmpty(label_Min))	{ _label_Min = ""; }
			else									{ _label_Min = label_Min; }
			if (string.IsNullOrEmpty(label_Max))	{ _label_Max = ""; }
			else									{ _label_Max = label_Max; }

			MakeInterpolationRange();
		}

		// Float
		//----------------------------------------------
		public void SetFloat(float defaultValue)
		{
			_valueType = TYPE.Float;
			_float_Def = defaultValue;
			_float_Cur = defaultValue;
			//_isRange = false;

			_float_Min = _float_Def - 1.0f;
			_float_Max = _float_Def + 1.0f;

			MakeInterpolationRange();
		}

		public void SetFloat(float defaultValue, float min, float max, string label_Min, string label_Max)
		{
			_valueType = TYPE.Float;
			_float_Def = defaultValue;
			_float_Cur = defaultValue;

			//_isRange = true;
			_float_Min = Mathf.Min(min, max);
			_float_Max = Mathf.Max(min, max);

			if (string.IsNullOrEmpty(label_Min))	{ _label_Min = ""; }
			else									{ _label_Min = label_Min; }

			if (string.IsNullOrEmpty(label_Max))	{ _label_Max = ""; }
			else									{ _label_Max = label_Max; }

			MakeInterpolationRange();
		}


		// Vector2
		//----------------------------------------------
		public void SetVector2(Vector2 defaultValue)
		{
			_valueType = TYPE.Vector2;
			_vec2_Def = defaultValue;
			_vec2_Cur = defaultValue;
			//_isRange = false;

			_vec2_Min = new Vector2(_vec2_Def.x - 1.0f, _vec2_Def.y - 1.0f);
			_vec2_Max = new Vector2(_vec2_Def.x + 1.0f, _vec2_Def.y + 1.0f);

			MakeInterpolationRange();
		}

		public void SetVector2(Vector2 defaultValue, Vector2 min, Vector2 max, string label_Min, string label_Max)
		{
			_valueType = TYPE.Vector2;
			_vec2_Def = defaultValue;
			_vec2_Cur = defaultValue;

			//_isRange = true;
			_vec2_Min = new Vector2(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y));
			_vec2_Max = new Vector2(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));

			if (string.IsNullOrEmpty(label_Min))	{ _label_Min = ""; }
			else									{ _label_Min = label_Min; }

			if (string.IsNullOrEmpty(label_Max))	{ _label_Max = ""; }
			else									{ _label_Max = label_Max; }

			MakeInterpolationRange();
		}



		



		//---------------------------------------------------------
		public string CurValueString
		{
			get
			{
				switch (_valueType)
				{
					//case TYPE.Bool: return _bool_Cur.ToString();
					case TYPE.Int:
						return _int_Cur.ToString();
					case TYPE.Float:
						return _float_Cur.ToString();
					case TYPE.Vector2:
						return _vec2_Cur.ToString();
						//case TYPE.Vector3: return _vec3_Cur.ToString();
						//case TYPE.Color: return _color_Cur.ToString();
				}
				return "";
			}
		}

		public float GetNormalizedDistance_Float(float fValue)
		{
			float fSize = _float_Max - _float_Min;
			if (fSize <= 0.0f)
			{
				return 1.0f;
			}
			return Mathf.Abs(fValue - _float_Cur) / fSize;
		}

		public float GetNormalizedDistance_Int(int iValue)
		{
			float fSize = (float)(_int_Max - _int_Min);
			if (fSize <= 0.0f)
			{
				return 1.0f;
			}
			return (float)Mathf.Abs(iValue - _int_Cur) / fSize;
		}

		public float GetNormalizedDistance_Vector2(Vector2 vValue)
		{
			Vector2 vSize = _vec2_Max - _vec2_Min;
			if (vSize.x <= 0.0f || vSize.y <= 0.0f)
			{
				return 1.0f;
			}
			Vector2 norDist = new Vector2(
				Mathf.Abs(vValue.x - _vec2_Cur.x) / vSize.x,
				Mathf.Abs(vValue.y - _vec2_Cur.y) / vSize.y
				);
			return norDist.magnitude;
		}

		//public float GetNormalizedDistance_Vector3(Vector3 vValue)
		//{
		//	Vector3 vSize = _vec3_Max - _vec3_Min;
		//	if(vSize.x <= 0.0f || vSize.y <= 0.0f || vSize.z <= 0.0f)
		//	{
		//		return 1.0f;
		//	}
		//	Vector3 norDist = new Vector3(
		//		Mathf.Abs(vValue.x - _vec3_Cur.x) / vSize.x,
		//		Mathf.Abs(vValue.y - _vec3_Cur.y) / vSize.y,
		//		Mathf.Abs(vValue.z - _vec3_Cur.z) / vSize.z
		//		);

		//	return norDist.magnitude;
		//}


		// 런타임 업데이트
		//-----------------------------------------------------------------------------------
		/// <summary>
		/// 추가 22.5.16 : 애니메이션 전환시, 지정되지 않은 경우의 컨트롤 파라미터 기본값
		/// </summary>
		/// <param name="unspecifiedOption"></param>
		public void SetUnspecifiedValueInAnimOption(apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM unspecifiedOption)
		{
			if(unspecifiedOption == apPortrait.UNSPECIFIED_ANIM_CONTROL_PARAM.RevertToDefaultValue)
			{
				//기존 방식 : 기본값을 베이스 값으로 삼는다.
				_isKeepLastValueInAnimTransition = false;
			}
			else
			{
				//새로운 방식 : 마지막 결과값을 베이스값으로 삼는다.
				_isKeepLastValueInAnimTransition = true;
			}
		}

		public void ReadyToOptLayerUpdate()
		{
			_isUpdated = false;
			_totalWeight = 0.0f;

			//레이어 초기화
			if(_layeredRequests == null)
			{
				_layeredRequests = new List<LayeredRequest>();
			}

			for (int i = 0; i < _layeredRequests.Count; i++)
			{
				_layeredRequests[i].ReadyToUpdate();
			}
		}




		public void AddCalculated_Request(apAnimControlParamResult calculatedRequest, int layer)
		{
			//해당 레이어의 LayeredRequest가 있는지 확인하자.
			if(layer < 0)
			{
				return;
			}
			
			if(layer < _layeredRequests.Count)
			{
				_curLayeredRequest = _layeredRequests[layer];
			}
			else
			{
				//생성해야한다.
				//요청 레이어까지 계속 추가하자
				int nPrevLayers = _layeredRequests.Count;
				for (int iNewLayer = nPrevLayers; iNewLayer <= layer; iNewLayer++)
				{
					LayeredRequest newLayeredRequest = new LayeredRequest(iNewLayer);//생성자에 ReadyToUpdate가 포함되어 있다.
					_layeredRequests.Add(newLayeredRequest);
				}

				_curLayeredRequest = _layeredRequests[layer];
			}

			_curLayeredRequest.AddRequest(calculatedRequest);
			_isUpdated = true;
			
		}


		//....


		/// <summary>
		/// 애니메이션의 현재 레이어 업데이트를 끝내고 레이어 내에서 처리했던 값들을 옮겨놓자
		/// </summary>
		public void CompleteOptLayerUpdate()
		{
			if (!_isUpdated || _layeredRequests.Count == 0)
			{
				//Debug.LogError(">> Not Calculated");

				//추가 22.1.9 : 이전 프레임까지 업데이트되다가 업데이트가 멈추었다.
				if(_isUpdatedPrev)
				{
					//기본값 직접 대입
					//변경 22.5.16 : 옵션에 따라 애니메이션에 의한 할당 종료값이 다르다.
					//[기본값] 또는 [마지막 값]

					if (_isKeepLastValueInAnimTransition)
					{
						//마지막 값 사용 (옵션)
						switch (_valueType)
						{
							case TYPE.Int:
								{
									_cal_Value_1D = _lastValue_Int;
									_int_ClampedCalculatedLayer = _lastValue_Int;
									_int_Cur = _lastValue_Int;
								}

								break;

							case TYPE.Float:
								{
									_cal_Value_1D = _lastValue_Float;
									_float_Cur = _lastValue_Float;
								}
								break;

							case TYPE.Vector2:
								{
									_cal_Value_2D = _lastValue_Vec2;
									_vec2_Cur = _lastValue_Vec2;
								}
								break;
						}
					}
					else
					{
						//기본값 사용 (기존의 방식)
						switch (_valueType)
						{
							case TYPE.Int:
								{
									_cal_Value_1D = _int_Def;
									_int_ClampedCalculatedLayer = _int_Def;
									_int_Cur = _int_Def;
									_lastValue_Int = _int_Cur;
								}

								break;

							case TYPE.Float:
								{
									_cal_Value_1D = _float_Def;
									_float_Cur = _float_Def;
									_lastValue_Float = _float_Cur;
								}
								break;

							case TYPE.Vector2:
								{
									_cal_Value_2D = _vec2_Def;
									_vec2_Cur = _vec2_Def;
									_lastValue_Vec2 = _vec2_Cur;
								}
								break;
						}
					}
				}

				_isUpdatedPrev = false;

				return;
			}

			_isUpdated = false;
			_totalWeight = 0.0f;
			_isUpdatedPrev = true;

			//변경 22.5.16 : 옵션에 따라서 보간의 베이스값이 [기본값]일 수도 있고 [마지막 결과값]일 수 있다.


			if (_isKeepLastValueInAnimTransition)
			{
				//[마지막 결과값]을 베이스 값으로 사용 (v1.4.0 : 22.5.16)
				switch (_valueType)
				{
					case TYPE.Int:		_cal_Value_1D = _lastValue_Int;		break;
					case TYPE.Float:	_cal_Value_1D = _lastValue_Float;	break;
					case TYPE.Vector2:	_cal_Value_2D = _lastValue_Vec2;	break;
				}
			}
			else
			{
				//[기본값]을 베이스 값으로 사용
				switch (_valueType)
				{
					case TYPE.Int:		_cal_Value_1D = _int_Def;		break;
					case TYPE.Float:	_cal_Value_1D = _float_Def;		break;
					case TYPE.Vector2:	_cal_Value_2D = _vec2_Def;		break;
				}
			}
			

			//변경된 방식. 레이어를 순서대로 계산하여 값을 누적시킨다.
			int iCalculatedLayer = 0;
			for (int iLayer = 0; iLayer < _layeredRequests.Count; iLayer++)
			{
				_curLayeredRequest = _layeredRequests[iLayer];
				if(!_curLayeredRequest._isCalculated)
				{
					continue;
				}

				//계산된 레이어의 처리를 마무리한다.
				//- Weight가 1 이상인 경우 normalize해야한다.
				//- 필요한 경우 Sort를 해야한다.
				_curLayeredRequest.CompleteCalculate();
				
				_totalWeight += _curLayeredRequest._totalWeight;

				//첫 레이어면 Interpolation
				//그외에는 레이어의 속성에 따라서 결정한다.

				if(iCalculatedLayer == 0
					|| _curLayeredRequest._blendMethod == apAnimPlayUnit.BLEND_METHOD.Interpolation)
				{
					switch (_valueType)
					{
						case TYPE.Int:
							_cal_Value_1D = (_cal_Value_1D * (1.0f - _curLayeredRequest._totalWeight))
											+ (_curLayeredRequest.GetCalculated_Int() * _curLayeredRequest._totalWeight);
							break;

						case TYPE.Float:
							_cal_Value_1D = (_cal_Value_1D * (1.0f - _curLayeredRequest._totalWeight))
											+ (_curLayeredRequest.GetCalculated_Float() * _curLayeredRequest._totalWeight);
							break;

						case TYPE.Vector2:
							_cal_Value_2D = (_cal_Value_2D * (1.0f - _curLayeredRequest._totalWeight))
											+ (_curLayeredRequest.GetCalculated_Vector2() * _curLayeredRequest._totalWeight);
							break;
					}
				}
				else
				{
					switch (_valueType)
					{
						case TYPE.Int:
							_cal_Value_1D += _curLayeredRequest.GetCalculated_Int() * _curLayeredRequest._totalWeight;

							//Debug.Log("> Int Layer " + iCalculatedLayer + " : " + _curLayeredRequest.GetCalculated_Int() + " (" + _curLayeredRequest._totalWeight + ") >> " + _cal_Value_1D);
							break;

						case TYPE.Float:
							_cal_Value_1D += _curLayeredRequest.GetCalculated_Float() * _curLayeredRequest._totalWeight;
							//Debug.LogWarning("> Float Layer " + iCalculatedLayer + " : " + _curLayeredRequest.GetCalculated_Float() + " (" + _curLayeredRequest._totalWeight + ") >> " + _cal_Value_1D);
							break;

						case TYPE.Vector2:
							_cal_Value_2D += _curLayeredRequest.GetCalculated_Vector2() * _curLayeredRequest._totalWeight;
							break;
					}
				}
				
				
				//계산된 레이어 증가
				iCalculatedLayer++;

				
			}

			_totalWeight = Mathf.Clamp01(_totalWeight);
			
			//if(_valueType == TYPE.Int)
			//{
			//	Debug.Log("Int : " + _cal_Value_1D + " : Weight : " + _totalWeight);
			//}


			//처리 결과
			switch (_valueType)
			{
				case TYPE.Int:
					{
						//개선안 2 (20.4.15) : 보간 및 float->int 변환 버그 있어서 Mathf.RoundToInt 함수를 이용하는 것으로 변경
						_int_ClampedCalculatedLayer = Mathf.Clamp(Mathf.RoundToInt(_cal_Value_1D), _int_Min, _int_Max);

						//변경 22.1.9 : Weight 0에 대한 값은 이전값이 아닌 기본값이다.
						//변경의 변경 22.5.16 : 이것이 옵션에 따라 다르다.
						if(_isKeepLastValueInAnimTransition)
						{
							// [ 마지막 값 ] 을 보간의 베이스로 이용 (22.5.16)
							_int_Cur = Mathf.RoundToInt(((float)_lastValue_Int * (1.0f - _totalWeight)) + ((float)_int_ClampedCalculatedLayer * _totalWeight));
							_int_Cur = Mathf.Clamp(_int_Cur, _int_Min, _int_Max);
						}
						else
						{
							// [ 기본값 ] 을 보간의 베이스로 이용
							_int_Cur = Mathf.RoundToInt(((float)_int_Def * (1.0f - _totalWeight)) + ((float)_int_ClampedCalculatedLayer * _totalWeight));
							_int_Cur = Mathf.Clamp(_int_Cur, _int_Min, _int_Max);
						}
						
						_lastValue_Int = _int_Cur;
					}
					break;

				case TYPE.Float:
					{
						_cal_Value_1D = Mathf.Clamp(_cal_Value_1D, _float_Min, _float_Max);

						
						//변경 22.1.9 : Weight 0에 대한 값은 이전값이 아닌 기본값이다.
						//변경의 변경 22.5.16 : 이것이 옵션에 따라 다르다.
						if(_isKeepLastValueInAnimTransition)
						{
							// [ 마지막 값 ] 을 보간의 베이스로 이용 (22.5.16)
							_float_Cur = Mathf.Clamp((_lastValue_Float * (1.0f - _totalWeight)) + (_cal_Value_1D * _totalWeight), _float_Min, _float_Max);
						}
						else
						{
							// [ 기본값 ] 을 보간의 베이스로 이용
							_float_Cur = Mathf.Clamp((_float_Def * (1.0f - _totalWeight)) + (_cal_Value_1D * _totalWeight), _float_Min, _float_Max);
						}
						
						_lastValue_Float = _float_Cur;
					}
					break;

				case TYPE.Vector2:
					{
						_cal_Value_2D.x = Mathf.Clamp(_cal_Value_2D.x, _vec2_Min.x, _vec2_Max.x);
						_cal_Value_2D.y = Mathf.Clamp(_cal_Value_2D.y, _vec2_Min.y, _vec2_Max.y);

						//변경 22.1.9 : Weight 0에 대한 값은 이전값이 아닌 기본값이다.
						//변경의 변경 22.5.16 : 이것이 옵션에 따라 다르다.
						if (_isKeepLastValueInAnimTransition)
						{
							// [ 마지막 값 ] 을 보간의 베이스로 이용 (22.5.16)
							_vec2_Cur.x = Mathf.Clamp((_lastValue_Vec2.x * (1.0f - _totalWeight)) + (_cal_Value_2D.x * _totalWeight), _vec2_Min.x, _vec2_Max.x);
							_vec2_Cur.y = Mathf.Clamp((_lastValue_Vec2.y * (1.0f - _totalWeight)) + (_cal_Value_2D.y * _totalWeight), _vec2_Min.y, _vec2_Max.y);
						}
						else
						{
							// [ 기본값 ] 을 보간의 베이스로 이용
							_vec2_Cur.x = Mathf.Clamp((_vec2_Def.x * (1.0f - _totalWeight)) + (_cal_Value_2D.x * _totalWeight), _vec2_Min.x, _vec2_Max.x);
							_vec2_Cur.y = Mathf.Clamp((_vec2_Def.y * (1.0f - _totalWeight)) + (_cal_Value_2D.y * _totalWeight), _vec2_Min.y, _vec2_Max.y);
						}

						_lastValue_Vec2 = _vec2_Cur;
					}
					break;
			}
		}



		// 외부 스크립트에 의한 제어를 위해 별도의 업데이트를 한다.
		//--------------------------------------------------------------
		public void InitRuntime()
		{
			SetDefault();

			//추가 22.1.9 : 업데이트 변수 초기화
			_isUpdatedPrev = false;


			_lastValue_Int = _int_Cur;
			_lastValue_Float = _float_Cur;
			_lastValue_Vec2 = _vec2_Cur;
		}



		public void InitRequest()
		{
			_isExternalUpdate = false;

			switch (_valueType)
			{
				case TYPE.Int:
					_int_PrevResult = _int_Cur;
					_int_PrevResult_NoRequest = _int_Cur;
					break;

				case TYPE.Float:
					_float_PrevResult = _float_Cur;
					_float_PrevResult_NoRequest = _float_Cur;
					break;

				case TYPE.Vector2:
					_vec2_PrevResult.x = _vec2_Cur.x;
					_vec2_PrevResult.y = _vec2_Cur.y;

					_vec2_PrevResult_NoRequest.x = _vec2_Cur.x;
					_vec2_PrevResult_NoRequest.y = _vec2_Cur.y;
					break;
			}
		}

		/// <summary>
		/// 애니메이션 처리까지 끝난 후 LateUpdate에서 Modifier 업데이트 직전에 처리한다.
		/// 외부 제어가 요청이 있었다면 덮어씌운다.
		/// Prev 값도 갱신해준다.
		/// </summary>
		public void CompleteRequests()
		{
			switch (_valueType)
			{
				case TYPE.Int:
					_int_PrevResult_NoRequest = _int_Cur;
					break;

				case TYPE.Float:
					_float_PrevResult_NoRequest = _float_Cur;
					break;

				case TYPE.Vector2:
					_vec2_PrevResult_NoRequest.x = _vec2_Cur.x;
					_vec2_PrevResult_NoRequest.y = _vec2_Cur.y;
					break;
			}


			if(_isExternalUpdate)
			{
				_isExternalUpdate = false;

				//Request 값을 덮어 씌우자
				//추가 22.5.16 : Last Value도 갱신한다.
				switch (_valueType)
				{
					case TYPE.Int:
						{
							_int_Cur = Mathf.Clamp((int)(((float)_int_Cur * (1.0f - _externalOverlapWeight) + (float)_int_Request * _externalOverlapWeight) + 0.5f), _int_Min, _int_Max);
							_lastValue_Int = _int_Cur;
						}
						break;

					case TYPE.Float:
						{
							_float_Cur = Mathf.Clamp((_float_Cur * (1.0f - _externalOverlapWeight)) + (_float_Request * _externalOverlapWeight), _float_Min, _float_Max);
							_lastValue_Float = _float_Cur;
						}
						break;

					case TYPE.Vector2:
						{
							_vec2_Cur.x = Mathf.Clamp((_vec2_Cur.x * (1.0f - _externalOverlapWeight)) + (_vec2_Request.x * _externalOverlapWeight), _vec2_Min.x, _vec2_Max.x);
							_vec2_Cur.y = Mathf.Clamp((_vec2_Cur.y * (1.0f - _externalOverlapWeight)) + (_vec2_Request.y * _externalOverlapWeight), _vec2_Min.y, _vec2_Max.y);
							_lastValue_Vec2 = _vec2_Cur;
						}
						break;
				}
			}

			switch (_valueType)
			{
				case TYPE.Int:
					_int_PrevResult = _int_Cur;
					_int_Request = _int_Cur;
					break;

				case TYPE.Float:
					_float_PrevResult = _float_Cur;
					_float_Request = _float_Cur;
					break;

				case TYPE.Vector2:
					_vec2_PrevResult.x = _vec2_Cur.x;
					_vec2_PrevResult.y = _vec2_Cur.y;

					_vec2_Request.x = _vec2_Cur.x;
					_vec2_Request.y = _vec2_Cur.y;
					break;
			}
		}

		public void RequestSetValueInt(int intValue, float ovelapWeight)
		{
			_isExternalUpdate = true;
			_int_Request = intValue;
			_externalOverlapWeight = Mathf.Clamp01(ovelapWeight);
		}

		public void RequestSetValueFloat(float floatValue, float ovelapWeight)
		{
			_isExternalUpdate = true;
			_float_Request = floatValue;
			_externalOverlapWeight = Mathf.Clamp01(ovelapWeight);
		}

		public void RequestSetValueVector2(Vector2 vec2Value, float ovelapWeight)
		{
			_isExternalUpdate = true;
			_vec2_Request = vec2Value;
			_externalOverlapWeight = Mathf.Clamp01(ovelapWeight);
		}

		public int IntValue { get { return _int_PrevResult; } }
		public float FloatValue {  get { return _float_PrevResult; } }
		public Vector2 Vector2Value {  get { return _vec2_PrevResult; } }

		public int IntValueWithoutEditing { get { return _int_PrevResult_NoRequest; } }
		public float FloatValueWithoutEditing {  get { return _float_PrevResult_NoRequest; } }
		public Vector2 Vector2ValueWithoutEditing {  get { return _vec2_PrevResult_NoRequest; } }

		//--------------------------------------------------------------------
		// Bake용 복사
		//--------------------------------------------------------------------
		public void CopyFromControlParam(apControlParam srcParam)
		{
			_category = srcParam._category;
			_iconPreset = srcParam._iconPreset;
			_isIconChanged = srcParam._isIconChanged;

			_valueType = srcParam._valueType;

			_int_Def = srcParam._int_Def;
			_float_Def = srcParam._float_Def;
			_vec2_Def = srcParam._vec2_Def;

			_int_Min = srcParam._int_Min;
			_int_Max = srcParam._int_Max;

			_float_Min = srcParam._float_Min;
			_float_Max = srcParam._float_Max;

			_vec2_Min = srcParam._vec2_Min;
			_vec2_Max = srcParam._vec2_Max;


			_label_Min = srcParam._label_Min;
			_label_Max = srcParam._label_Max;

			_uniqueID = srcParam._uniqueID;
			_keyName = srcParam._keyName;

			_isReserved = srcParam._isReserved;

			_snapSize = srcParam._snapSize;
		}


		public void CopyFromControlParamWithoutName(apControlParam srcParam)
		{
			_category = srcParam._category;
			_iconPreset = srcParam._iconPreset;
			_isIconChanged = srcParam._isIconChanged;

			_valueType = srcParam._valueType;

			_int_Def = srcParam._int_Def;
			_float_Def = srcParam._float_Def;
			_vec2_Def = srcParam._vec2_Def;

			_int_Min = srcParam._int_Min;
			_int_Max = srcParam._int_Max;

			_float_Min = srcParam._float_Min;
			_float_Max = srcParam._float_Max;

			_vec2_Min = srcParam._vec2_Min;
			_vec2_Max = srcParam._vec2_Max;


			_label_Min = srcParam._label_Min;
			_label_Max = srcParam._label_Max;

			//_uniqueID = srcParam._uniqueID;
			//_keyName = srcParam._keyName;
			//_isReserved = srcParam._isReserved;

			_snapSize = srcParam._snapSize;
		}
	}

}