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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

using AnyPortrait;


namespace AnyPortrait
{
	[Serializable]
	public class apUnityEvent
	{
		// Members
		//------------------------------------------------------
		//빠른 Link를 위한 ID를 발급하자
		[SerializeField]
		public int _uniqueID = -1;

		//키값 비교를 위한 파라미터 타입과 이름
		[SerializeField]
		public string _eventName = null;

		[SerializeField]
		public apAnimEvent.PARAM_TYPE[] _paramTypes = null;

		public int _nParams = 0;

		public bool _isMultipleEvent = false;

#if UNITY_EDITOR
		//[NonSerialized]
		//private SerializedProperty _eventProperty = null;

		[NonSerialized]
		private string _guiLabel = null;
		
#endif


		//설정된 유니티 이벤트 (타입에 따라 다르다)
		public enum UNITY_EVENT_TYPE : int
		{
			None = 0,
			Bool = 1,
			Integer = 2,
			Float = 3,
			Vector2 = 4,
			String = 5,
			MultipleObjects = 6,//다중인 경우
		}

		[SerializeField]
		public UNITY_EVENT_TYPE _unityEventType = UNITY_EVENT_TYPE.Bool;


		//유니티 이벤트들 > 사용 안함.
		//커스텀을 이용하자
		//[SerializeField]
		//public apUnityEvent_None _unityEvent_None = null;

		//[SerializeField]
		//public apUnityEvent_Bool _unityEvent_Bool = null;

		//[SerializeField]
		//public apUnityEvent_Int _unityEvent_Int = null;

		//[SerializeField]
		//public apUnityEvent_Float _unityEvent_Float = null;

		//[SerializeField]
		//public apUnityEvent_Vector2 _unityEvent_Vector2 = null;

		//[SerializeField]
		//public apUnityEvent_String _unityEvent_String = null;

		//[SerializeField]
		//public apUnityEvent_Objects _unityEvent_Objects = null;

		[Serializable]
		public class TargetMethodSet
		{
			[SerializeField]
			public MonoBehaviour _target = null;

			[SerializeField]
			public string _methodName = null;

			//Action (타입에 따라서)
			[NonSerialized]
			private System.Action _action_None = null;
			
			[NonSerialized]
			private System.Action<bool> _action_Bool = null;

			[NonSerialized]
			private System.Action<int> _action_Int = null;

			[NonSerialized]
			private System.Action<float> _action_Float = null;

			[NonSerialized]
			private System.Action<Vector2> _action_Vector2 = null;

			[NonSerialized]
			private System.Action<string> _action_String = null;

			[NonSerialized]
			private System.Action<object[]> _action_Objects = null;

			

			public TargetMethodSet()
			{
			}

			/// <summary>유효성 테스트</summary>
			public bool Validate(apUnityEvent.UNITY_EVENT_TYPE eventType, bool isShowLog = true)
			{
				_action_None = null;
				_action_Bool = null;
				_action_Int = null;
				_action_Float = null;
				_action_Vector2 = null;
				_action_String = null;
				_action_Objects = null;

				if(_target == null || string.IsNullOrEmpty(_methodName))
				{	
					return false;
				}

				System.Type type_Target = _target.GetType();

				MethodInfo methodInfo = null;
				try
				{
					methodInfo = type_Target.GetMethod(_methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				}
				catch(System.Reflection.AmbiguousMatchException)
				{
					//오버로딩된 함수를 참조하고자 했다.
					if(isShowLog)
					{
						Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (Overloaded function)");
					}
					
					return false;
				}
				catch(Exception ex)
				{
					//예외가 발생할 수 있다.
					if(isShowLog)
					{
						Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid.\n" + ex.ToString());
					}					
					return false;
				}
				

				if(methodInfo == null)
				{
					if(isShowLog)
					{
						Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (Unknown function)");
					}
					return false;
				}



				//리턴 체크 (void 인지 체크한다.)
				
				if(!System.Type.Equals(methodInfo.ReturnType, typeof(void)))
				{
					if (isShowLog)
					{
						Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (Return type is not void.)");
					}
					return false;
				}




				//파라미터 체크
				//- None : 파라미터가 없어야 한다.
				//- 기본 : 파라미터 1개에 타입이 맞아야 한다.
				//- 다중 : 파라미터가 2개 이상이어야 한다. 각각 테스트도 해야함
				// > 그냥 가져오고 null 체크만 하면 될듯

				#region [미사용 코드]
				//ParameterInfo[] paramInfos = methodInfo.GetParameters();
				//int nParams = paramInfos != null ? paramInfos.Length : 0;

				//switch (eventType)
				//{
				//	case apUnityEvent.UNITY_EVENT_TYPE.None:
				//		if(nParams > 0)
				//		{
				//			Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (There should be no arguments.)");
				//			return false;
				//		}
				//		break;

				//	case apUnityEvent.UNITY_EVENT_TYPE.Bool:
				//		{
				//			if (nParams != 1)
				//			{
				//				Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (There must be one bool argument.)");
				//				return false;
				//			}
				//			if (!Type.Equals(paramInfos[0].ParameterType, typeof(bool)))
				//			{
				//				Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (There must be one bool argument.)");
				//				return false;
				//			}
				//		}
				//		break;

				//	case apUnityEvent.UNITY_EVENT_TYPE.Integer:
				//		{
				//			if (nParams != 1)
				//			{
				//				Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (There must be one int argument.)");
				//				return false;
				//			}
				//			if (!Type.Equals(paramInfos[0].ParameterType, typeof(int)))
				//			{
				//				Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (There must be one int argument.)");
				//				return false;
				//			}
				//		}
				//		break;

				//	case apUnityEvent.UNITY_EVENT_TYPE.Float:
				//		{
				//			if (nParams != 1)
				//			{
				//				Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (There must be one float argument.)");
				//				return false;
				//			}
				//			if (!Type.Equals(paramInfos[0].ParameterType, typeof(float)))
				//			{
				//				Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (There must be one float argument.)");
				//				return false;
				//			}
				//		}
				//		break;

				//	case apUnityEvent.UNITY_EVENT_TYPE.Vector2:
				//		{

				//		}
				//		break;

				//	case apUnityEvent.UNITY_EVENT_TYPE.String:
				//	case apUnityEvent.UNITY_EVENT_TYPE.MultipleObjects:
				//		break;

				//} 
				#endregion


				switch (eventType)
				{
					case apUnityEvent.UNITY_EVENT_TYPE.None:
						_action_None = Delegate.CreateDelegate(typeof(System.Action), _target, methodInfo, false) as System.Action;
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Bool:
						_action_Bool = Delegate.CreateDelegate(typeof(System.Action<bool>), _target, methodInfo, false) as System.Action<bool>;
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Integer:
						_action_Int = Delegate.CreateDelegate(typeof(System.Action<int>), _target, methodInfo, false) as System.Action<int>;
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Float:
						_action_Float = Delegate.CreateDelegate(typeof(System.Action<float>), _target, methodInfo, false) as System.Action<float>;
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Vector2:
						_action_Vector2 = Delegate.CreateDelegate(typeof(System.Action<Vector2>), _target, methodInfo, false) as System.Action<Vector2>;
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.String:
						_action_String = Delegate.CreateDelegate(typeof(System.Action<string>), _target, methodInfo, false) as System.Action<string>;
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.MultipleObjects:
						_action_Objects = Delegate.CreateDelegate(typeof(System.Action<object[]>), _target, methodInfo, false) as System.Action<object[]>;
						break;
				}


				if(_action_None == null
					&& _action_Bool == null
					&& _action_Int == null
					&& _action_Float == null
					&& _action_Vector2 == null
					&& _action_String == null
					&& _action_Objects == null)
				{
					if (isShowLog)
					{
						Debug.LogError("AnyPortrait : The callback function [" + _methodName + "] for animation events is not valid. (Action creation is failed.)");
					}
					return false;
				}
				
				return true;
			}

			public void Invoke_None()
			{
				if(_target == null || _action_None == null) { return; }
				_action_None();
			}

			public void Invoke_Bool(bool param)
			{
				if(_target == null || _action_Bool == null) { return; }
				_action_Bool(param);
			}

			public void Invoke_Int(int param)
			{
				if(_target == null || _action_Int == null) { return; }
				_action_Int(param);
			}

			public void Invoke_Float(float param)
			{
				if(_target == null || _action_Float == null) { return; }
				_action_Float(param);
			}

			public void Invoke_Vector2(Vector2 param)
			{
				if(_target == null || _action_Vector2 == null) { return; }
				_action_Vector2(param);
			}

			public void Invoke_String(string param)
			{
				if(_target == null || _action_String == null) { return; }
				_action_String(param);
			}

			public void Invoke_Objects(object[] param)
			{
				if(_target == null || _action_Objects == null) { return; }
				_action_Objects(param);
			}
		}

		//저장된 리스트
		[SerializeField]
		public List<TargetMethodSet> _targetMethods = null;

		//실행 리스트 (빠른 처리를 위해)
		[NonSerialized]
		private TargetMethodSet[] _runtimeTargetMethods = null;

		[NonSerialized]
		private int _nRuntimeTargetMethods = 0;


		// Init
		//------------------------------------------------------
		public apUnityEvent()
		{
			
		}

		public void SetUniqueID(int uniqueID)
		{
			_uniqueID = uniqueID;
		}


		/// <summary>
		/// Bake시 애니메이션 이벤트 정보를 넣어주자. (srcAnimEvent의 데이터만 받고, 실제로 연결되지는 않는다.)
		/// </summary>
		/// <param name="srcAnimEvent"></param>
		public void SetSrcEventData_Bake(apAnimEvent srcAnimEvent)
		{
			_eventName = srcAnimEvent._eventName;
			_nParams = srcAnimEvent._subParams != null ? srcAnimEvent._subParams.Count : 0;

			if (_nParams <= 0)
			{
				//파라미터가 없다면
				_paramTypes = null;
				_isMultipleEvent = false;
				_unityEventType = UNITY_EVENT_TYPE.None;//파라미터가 없다.
			}
			else if (_nParams == 1)
			{
				//파라미터가 1개
				_paramTypes = new apAnimEvent.PARAM_TYPE[1];
				_isMultipleEvent = false;

				_paramTypes[0] = srcAnimEvent._subParams[0]._paramType;

				//파라미터 타입에 따라 유니티 이벤트의 타입을 정하자
				switch (_paramTypes[0])
				{
					case apAnimEvent.PARAM_TYPE.Bool:
						_unityEventType = UNITY_EVENT_TYPE.Bool;
						break;

					case apAnimEvent.PARAM_TYPE.Integer:
						_unityEventType = UNITY_EVENT_TYPE.Integer;
						break;

					case apAnimEvent.PARAM_TYPE.Float:
						_unityEventType = UNITY_EVENT_TYPE.Float;
						break;

					case apAnimEvent.PARAM_TYPE.Vector2:
						_unityEventType = UNITY_EVENT_TYPE.Vector2;
						break;

					case apAnimEvent.PARAM_TYPE.String:
					default:
						_unityEventType = UNITY_EVENT_TYPE.String;
						break;
				}

			}
			else
			{
				//파라미터가 여러개
				_paramTypes = new apAnimEvent.PARAM_TYPE[_nParams];
				_isMultipleEvent = true;//다중 이벤트

				//파라미터들을 복제하자
				for (int iParam = 0; iParam < _nParams; iParam++)
				{
					_paramTypes[iParam] = srcAnimEvent._subParams[iParam]._paramType;
				}

				_unityEventType = UNITY_EVENT_TYPE.MultipleObjects;
			}

			//유니티 이벤트 타입에 따라서 이벤트를 생성/삭제하자
			//_unityEvent_None = null;
			//_unityEvent_Bool = null;
			//_unityEvent_Int = null;
			//_unityEvent_Float = null;
			//_unityEvent_Vector2 = null;
			//_unityEvent_String = null;
			//_unityEvent_Objects = null;

			//switch (_unityEventType)
			//{
			//	case UNITY_EVENT_TYPE.None:					_unityEvent_None = new apUnityEvent_None(); break;
			//	case UNITY_EVENT_TYPE.Bool:					_unityEvent_Bool = new apUnityEvent_Bool(); break;
			//	case UNITY_EVENT_TYPE.Integer:				_unityEvent_Int = new apUnityEvent_Int(); break;
			//	case UNITY_EVENT_TYPE.Vector2:				_unityEvent_Vector2 = new apUnityEvent_Vector2(); break;
			//	case UNITY_EVENT_TYPE.String:				_unityEvent_String = new apUnityEvent_String(); break;
			//	case UNITY_EVENT_TYPE.MultipleObjects:		_unityEvent_Objects = new apUnityEvent_Objects(); break;
			//}

			if(_targetMethods == null)
			{
				_targetMethods = new List<TargetMethodSet>();
			}

#if UNITY_EDITOR
			//_eventProperty = null;

			
#endif
		}

		// Functions
		//------------------------------------------------------
		/// <summary>
		/// 해당 이벤트와 래핑 조건이 맞는가.
		/// </summary>
		/// <param name="animEvent"></param>
		/// <returns></returns>
		public bool IsWrappableEvent(apAnimEvent animEvent)
		{
			//이름, 파라미터 타입을 검사한다.
			if(!string.Equals(_eventName, animEvent._eventName))
			{
				//이름이 다르다
				return false;
			}

			//타입을 보자
			int nParamsAnimEvent = animEvent._subParams != null ? animEvent._subParams.Count : 0;
			if(_nParams != nParamsAnimEvent)
			{
				//파라미터의 개수가 다르다.
				return false;
			}


			if(nParamsAnimEvent == 0)
			{
				//파라미터가 없다면 더 체크하지 않고 true 리턴
				return true;
			}

			//하나라도 다르면 false 리턴

			for (int iParam = 0; iParam < nParamsAnimEvent; iParam++)
			{
				//파라미터가 다르다.
				if(_paramTypes[iParam] != animEvent._subParams[iParam]._paramType)
				{
					return false;
				}
			}

			//동일하다.
			return true;
		}





		/// <summary>
		/// 유효성을 테스트한다. Link시 호출할 것
		/// </summary>
		public void Validate()
		{
			int nSrcTMs = _targetMethods != null ? _targetMethods.Count : 0;
			if(nSrcTMs == 0)
			{
				_runtimeTargetMethods = null;
				_nRuntimeTargetMethods = 0;
				return;
			}

			TargetMethodSet curTM = null;
			List<TargetMethodSet> validTMs = new List<TargetMethodSet>();
			for (int iTM = 0; iTM < nSrcTMs; iTM++)
			{
				curTM = _targetMethods[iTM];
				if(curTM.Validate(_unityEventType))
				{
					//유효한 리스트에 넣자
					validTMs.Add(curTM);
				}
			}

			if(validTMs.Count == 0)
			{
				//유효한게 없다.
				_runtimeTargetMethods = null;
				_nRuntimeTargetMethods = 0;
				return;
			}

			_nRuntimeTargetMethods = validTMs.Count;
			_runtimeTargetMethods = new TargetMethodSet[_nRuntimeTargetMethods];
			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i] = validTMs[i];
			}
		}



		//에디터용 유효성 검사 함수
		/// <summary>에디터에서 유효성을 테스트한다. 유효하지 않다면 false를 리턴한다. 변경되는 건 없다.</summary>
		/// <returns></returns>
		public bool ValidateInEditor()
		{
			int nSrcTMs = _targetMethods != null ? _targetMethods.Count : 0;
			if(nSrcTMs == 0)
			{
				return true;
			}

			TargetMethodSet curTM = null;
			for (int iTM = 0; iTM < nSrcTMs; iTM++)
			{
				curTM = _targetMethods[iTM];
				if(!curTM.Validate(_unityEventType, false))
				{
					//유효하지 않은게 하나라도 있다면
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// 유효하지 않은 메소드 세트를 삭제한다.
		/// </summary>
		public void RemoveInvalidMethodSetInEditor()
		{
			int nSrcTMs = _targetMethods != null ? _targetMethods.Count : 0;
			if(nSrcTMs == 0)
			{
				return;
			}

			TargetMethodSet curTM = null;
			List<TargetMethodSet> validTMs = new List<TargetMethodSet>();
			bool isAnyInvalid = false;
			for (int iTM = 0; iTM < nSrcTMs; iTM++)
			{
				curTM = _targetMethods[iTM];
				if(curTM.Validate(_unityEventType, false))
				{
					//유효한 리스트에 넣자
					validTMs.Add(curTM);
				}
				else
				{
					//유효하지 않은게 있다.
					isAnyInvalid = true;
				}	
			}

			if(!isAnyInvalid)
			{
				//모두 유효하다.
				return;
			}
			if(validTMs.Count == 0)
			{
				//유효한게 없다.
				_targetMethods.Clear();
				return;
			}

			//유효하지 않았던 것을 모두 삭제한다.
			_targetMethods.RemoveAll(delegate(TargetMethodSet a)
			{
				//유효한 리스트에 없는걸 모두 삭제한다.
				return !validTMs.Contains(a);
			});
			
		}


		/// <summary>
		/// 입력된 모노 타겟을 메소드 세트에 할당한다.
		/// 이미 해당 모노 타겟을 하나라도 대상으로 하는 메소드 세트가 있다면 생략한다.
		/// </summary>
		/// <param name="targetMono"></param>
		public void AssignCommonMonoTarget(MonoBehaviour targetMono)
		{
			if(targetMono == null)
			{
				return;
			}

			//먼저 이미 할당된게 있는지 확인하자
			int nTMS = _targetMethods != null ? _targetMethods.Count : 0;

			
			if(nTMS > 0)
			{
				TargetMethodSet curTMS = null;
				for (int i = 0; i < nTMS; i++)
				{
					curTMS = _targetMethods[i];
					if(curTMS._target == targetMono)
					{
						//이미 할당되어 있다면 생략
						return;
					}
				}
			}

			//추가하자
			TargetMethodSet newTMS = new TargetMethodSet();
			newTMS._target = targetMono;
			newTMS._methodName = "";

			if(_targetMethods == null)
			{
				_targetMethods = new List<TargetMethodSet>();
			}
			_targetMethods.Add(newTMS);
		}


		//Invoke 함수들
		//-----------------------------------------------------------------------------
		public void Invoke(apAnimEvent animEvent)
		{
			switch (_unityEventType)
			{
				case UNITY_EVENT_TYPE.None:
					//if(_unityEvent_None != null) { _unityEvent_None.Invoke(); }
					Invoke_None();
					break;

				case UNITY_EVENT_TYPE.Bool:
					//if(_unityEvent_Bool != null) { _unityEvent_Bool.Invoke((bool)animEvent.GetCalculatedParam()); }
					Invoke_Bool((bool)animEvent.GetCalculatedParam());
					break;

				case UNITY_EVENT_TYPE.Integer:
					//if(_unityEvent_Int != null) { _unityEvent_Int.Invoke((int)animEvent.GetCalculatedParam()); }
					Invoke_Int((int)animEvent.GetCalculatedParam());
					break;

				case UNITY_EVENT_TYPE.Float:
					//if(_unityEvent_Float != null) { _unityEvent_Float.Invoke((float)animEvent.GetCalculatedParam()); }
					Invoke_Float((float)animEvent.GetCalculatedParam());
					break;

				case UNITY_EVENT_TYPE.Vector2:
					//if(_unityEvent_Vector2 != null) { _unityEvent_Vector2.Invoke((Vector2)animEvent.GetCalculatedParam()); }
					Invoke_Vector2((Vector2)animEvent.GetCalculatedParam());
					break;

				case UNITY_EVENT_TYPE.String:
					//if(_unityEvent_String != null) { _unityEvent_String.Invoke((string)animEvent.GetCalculatedParam()); }
					Invoke_String((string)animEvent.GetCalculatedParam());
					break;

				case UNITY_EVENT_TYPE.MultipleObjects:
					//if(_unityEvent_Objects != null) { _unityEvent_Objects.Invoke((object[])animEvent.GetCalculatedParam()); }
					Invoke_Objects((object[])animEvent.GetCalculatedParam());
					break;
			}
		}


		private void Invoke_None()
		{
			if(_nRuntimeTargetMethods == 0) { return; }

			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i].Invoke_None();
			}
		}

		private void Invoke_Bool(bool param)
		{
			if(_nRuntimeTargetMethods == 0) { return; }

			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i].Invoke_Bool(param);
			}
		}

		private void Invoke_Int(int param)
		{
			if(_nRuntimeTargetMethods == 0) { return; }

			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i].Invoke_Int(param);
			}
		}

		private void Invoke_Float(float param)
		{
			if(_nRuntimeTargetMethods == 0) { return; }

			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i].Invoke_Float(param);
			}
		}

		private void Invoke_Vector2(Vector2 param)
		{
			if(_nRuntimeTargetMethods == 0) { return; }

			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i].Invoke_Vector2(param);
			}
		}

		private void Invoke_String(string param)
		{
			if(_nRuntimeTargetMethods == 0) { return; }

			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i].Invoke_String(param);
			}
		}

		private void Invoke_Objects(object[] param)
		{
			if(_nRuntimeTargetMethods == 0) { return; }

			for (int i = 0; i < _nRuntimeTargetMethods; i++)
			{
				_runtimeTargetMethods[i].Invoke_Objects(param);
			}
		}





		


		#region [미사용 코드]
		//// Add Listener
		////------------------------------------------------------
		//public void AddListener(UnityAction action_NoParam)
		//{
		//	if(_unityEventType != UNITY_EVENT_TYPE.None || _unityEvent_None == null)
		//	{
		//		Debug.LogError("AnyPortrait : [AddListener Failed] The listener type is not appropriate. (None > " + _unityEventType + ")");
		//		return;
		//	}

		//	_unityEvent_None.AddListener(action_NoParam);
		//}

		//public void AddListener(UnityAction<bool> action_BoolParam)
		//{
		//	if(_unityEventType != UNITY_EVENT_TYPE.Bool || _unityEvent_Bool == null)
		//	{
		//		Debug.LogError("AnyPortrait : [AddListener Failed] The listener type is not appropriate. (Bool > " + _unityEventType + ")");
		//		return;
		//	}

		//	_unityEvent_Bool.AddListener(action_BoolParam);
		//}

		//public void AddListener(UnityAction<int> action_IntParam)
		//{
		//	if(_unityEventType != UNITY_EVENT_TYPE.Integer || _unityEvent_Int == null)
		//	{
		//		Debug.LogError("AnyPortrait : [AddListener Failed] The listener type is not appropriate. (Integer > " + _unityEventType + ")");
		//		return;
		//	}

		//	_unityEvent_Int.AddListener(action_IntParam);
		//}

		//public void AddListener(UnityAction<float> action_FloatParam)
		//{
		//	if(_unityEventType != UNITY_EVENT_TYPE.Float || _unityEvent_Float == null)
		//	{
		//		Debug.LogError("AnyPortrait : [AddListener Failed] The listener type is not appropriate. (Float > " + _unityEventType + ")");
		//		return;
		//	}

		//	_unityEvent_Float.AddListener(action_FloatParam);
		//}

		//public void AddListener(UnityAction<Vector2> action_Vector2Param)
		//{
		//	if(_unityEventType != UNITY_EVENT_TYPE.Vector2 || _unityEvent_Vector2 == null)
		//	{
		//		Debug.LogError("AnyPortrait : [AddListener Failed] The listener type is not appropriate. (Vector2 > " + _unityEventType + ")");
		//		return;
		//	}

		//	_unityEvent_Vector2.AddListener(action_Vector2Param);
		//}

		//public void AddListener(UnityAction<string> action_StringParam)
		//{
		//	if(_unityEventType != UNITY_EVENT_TYPE.String || _unityEvent_String == null)
		//	{
		//		Debug.LogError("AnyPortrait : [AddListener Failed] The listener type is not appropriate. (String > " + _unityEventType + ")");
		//		return;
		//	}

		//	_unityEvent_String.AddListener(action_StringParam);
		//}

		//public void AddListener(UnityAction<object[]> action_MultipleParams)
		//{
		//	if(_unityEventType != UNITY_EVENT_TYPE.MultipleObjects || _unityEvent_Objects == null)
		//	{
		//		Debug.LogError("AnyPortrait : [AddListener Failed] The listener type is not appropriate. (Multiple Objects > " + _unityEventType + ")");
		//		return;
		//	}

		//	_unityEvent_Objects.AddListener(action_MultipleParams);
		//}


		//// Remove Listener
		////------------------------------------------------------
		//public void RemoveListener(UnityAction action_NoParam)
		//{
		//	if(_unityEvent_None == null) { return; }
		//	_unityEvent_None.RemoveListener(action_NoParam);
		//}

		//public void RemoveListener(UnityAction<bool> action_BoolParam)
		//{
		//	if(_unityEvent_Bool == null) { return; }
		//	_unityEvent_Bool.RemoveListener(action_BoolParam);
		//}

		//public void RemoveListener(UnityAction<int> action_IntParam)
		//{
		//	if(_unityEvent_Int == null) { return; }
		//	_unityEvent_Int.RemoveListener(action_IntParam);
		//}

		//public void RemoveListener(UnityAction<float> action_FloatParam)
		//{
		//	if(_unityEvent_Float == null) { return; }
		//	_unityEvent_Float.RemoveListener(action_FloatParam);
		//}

		//public void RemoveListener(UnityAction<Vector2> action_Vector2Param)
		//{
		//	if(_unityEvent_Vector2 == null) { return; }
		//	_unityEvent_Vector2.RemoveListener(action_Vector2Param);
		//}

		//public void RemoveListener(UnityAction<string> action_StringParam)
		//{
		//	if(_unityEvent_String == null) { return; }
		//	_unityEvent_String.RemoveListener(action_StringParam);
		//}

		//public void RemoveListener(UnityAction<object[]> action_MultipleParams)
		//{
		//	if(_unityEvent_Objects == null) { return; }
		//	_unityEvent_Objects.RemoveListener(action_MultipleParams);
		//}

		//public void RemoveAllListeners()
		//{
		//	if(_unityEvent_None != null)		{ _unityEvent_None.RemoveAllListeners(); }
		//	if(_unityEvent_Bool != null)		{ _unityEvent_Bool.RemoveAllListeners(); }
		//	if(_unityEvent_Int != null)			{ _unityEvent_Int.RemoveAllListeners(); }
		//	if(_unityEvent_Float != null)		{ _unityEvent_Float.RemoveAllListeners(); }
		//	if(_unityEvent_Vector2 != null)		{ _unityEvent_Vector2.RemoveAllListeners(); }
		//	if(_unityEvent_String != null)		{ _unityEvent_String.RemoveAllListeners(); }
		//	if(_unityEvent_Objects != null)		{ _unityEvent_Objects.RemoveAllListeners(); }
		//} 
		#endregion

#if UNITY_EDITOR

		//public SerializedProperty GetSerializedProperty(SerializedObject serializedObject, int index)
		//{
		//	// Disposed일 수 있다.
		//	// 체크할 것
		//	if(_eventProperty != null)
		//	{
		//		try
		//		{
		//			if(!_eventProperty.editable)
		//			{
		//				//Debug.Log("편집 불가능한 Property 발견 (" + index + ")");
		//				_eventProperty = null;
		//			}
		//		}
		//		catch(ObjectDisposedException)
		//		{
		//			//Disposed된 것이다.
		//			//Debug.Log("Disposed된 Property 발견 (" + index + ")");
		//			_eventProperty = null;
		//		}
		//		catch(NullReferenceException)
		//		{
		//			//Disposed된 것이다.
		//			//Debug.Log("Disposed/Null된 Property 발견 (" + index + ")");
		//			_eventProperty = null;
		//		}
				
		//	}
		//	if (_eventProperty == null)
		//	{
		//		try
		//		{
		//			SerializedProperty prop_Events = serializedObject.FindProperty("_unityEventWrapper._unityEvents");
		//			SerializedProperty prop_Self = prop_Events.GetArrayElementAtIndex(index);

		//			switch (_unityEventType)
		//			{
		//				case UNITY_EVENT_TYPE.None:
		//					_eventProperty = prop_Self.FindPropertyRelative("_unityEvent_None");
		//					break;

		//				case UNITY_EVENT_TYPE.Bool:
		//					_eventProperty = prop_Self.FindPropertyRelative("_unityEvent_Bool");
		//					break;

		//				case UNITY_EVENT_TYPE.Integer:
		//					_eventProperty = prop_Self.FindPropertyRelative("_unityEvent_Int");
		//					break;

		//				case UNITY_EVENT_TYPE.Float:
		//					_eventProperty = prop_Self.FindPropertyRelative("_unityEvent_Float");
		//					break;

		//				case UNITY_EVENT_TYPE.Vector2:
		//					_eventProperty = prop_Self.FindPropertyRelative("_unityEvent_Vector2");
		//					break;

		//				case UNITY_EVENT_TYPE.String:
		//					_eventProperty = prop_Self.FindPropertyRelative("_unityEvent_String");
		//					break;

		//				case UNITY_EVENT_TYPE.MultipleObjects:
		//					_eventProperty = prop_Self.FindPropertyRelative("_unityEvent_Objects");
		//					break;
		//			}
		//		}
		//		catch(Exception ex)
		//		{
		//			Debug.LogError("GetSerializedProperty Exception : " + ex);
		//			Debug.LogError("Index : " + index);
		//		}
		//	}
		//	return _eventProperty;
		//}

		
		public string GetGUILabel()
		{
			if(string.IsNullOrEmpty(_guiLabel))
			{	
				_guiLabel = _eventName;
				if(_nParams > 0)
				{
					_guiLabel += " : ";

					if(_nParams > 1)
					{
						_guiLabel += "(object[]) ";
					}
					for (int i = 0; i < _nParams; i++)
					{
						if(i > 0)
						{
							_guiLabel += ", ";
						}

						switch (_paramTypes[i])
						{
							case apAnimEvent.PARAM_TYPE.Bool:		_guiLabel += "bool"; break;
							case apAnimEvent.PARAM_TYPE.Integer:	_guiLabel += "int"; break;
							case apAnimEvent.PARAM_TYPE.Float:		_guiLabel += "float"; break;
							case apAnimEvent.PARAM_TYPE.Vector2:	_guiLabel += "Vector2"; break;
							case apAnimEvent.PARAM_TYPE.String:		_guiLabel += "string"; break;
						}
					}
				}
			}

			return _guiLabel;
		}


		public static System.Type GetParamType(apAnimEvent.PARAM_TYPE paramType)
		{
			switch (paramType)
			{
				case apAnimEvent.PARAM_TYPE.Bool:		return typeof(bool);
				case apAnimEvent.PARAM_TYPE.Integer:	return typeof(int);
				case apAnimEvent.PARAM_TYPE.Float:		return typeof(float);
				case apAnimEvent.PARAM_TYPE.Vector2:	return typeof(Vector2);
				case apAnimEvent.PARAM_TYPE.String:		return typeof(string);
			}
			return typeof(void);
		}
#endif
	}

	

	//유니티 이벤트값을 멤버로 저장하기 위한 오버라이드 클래스
	//> 유니티 이벤트는 호환이 맞지 않는다. 직접 만들자.
	//설명 https://docs.unity3d.com/kr/530/ScriptReference/Events.UnityEvent_1.html
	//[Serializable]
	//public class apUnityEvent_None : UnityEvent { }

	//[Serializable]
	//public class apUnityEvent_Bool : UnityEvent<bool> { }
	
	//[Serializable]
	//public class apUnityEvent_Int : UnityEvent<int> { }

	//[Serializable]
	//public class apUnityEvent_Float : UnityEvent<float> { }

	//[Serializable]
	//public class apUnityEvent_Vector2 : UnityEvent<Vector2> { }

	//[Serializable]
	//public class apUnityEvent_String : UnityEvent<string> { }

	//[Serializable]
	//public class apUnityEvent_Objects : UnityEvent<object[]> { }
}