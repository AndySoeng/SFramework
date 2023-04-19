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
//using System.Diagnostics;


using AnyPortrait;

namespace AnyPortrait
{
	

	/// <summary>
	/// 에디터의 단축키를 처리하는 객체.
	/// 단축키 처리는 OnGUI이 후반부에 해야하는데,
	/// UI별로 단축키에 대한 처리 요구가 임의의 위치에서 이루어지므로, 이를 대신 받아서 지연시키는 객체.
	/// 모든 함수 요청은 OnGUI마다 리셋되고 다시 받는다.
	/// 이벤트에 따라 묵살될 수 있다.
	/// </summary>
	public class apHotKey
	{
		//변경 21.12.8 : 처리 결과를 Return 받아야 한다.
		//싱글톤/Static 함수로 처리한다.
		//이게 리턴되는것 자체가 성공이다.
		public class HotKeyResult
		{
			private static HotKeyResult _instance = new HotKeyResult();

			public string _customLabel = null;

			//외부 생성 불가
			private HotKeyResult() { }

			public static HotKeyResult MakeResult(string customLabel = null)
			{
				if(_instance == null)
				{
					_instance = new HotKeyResult();
				}
				_instance._customLabel = customLabel;
				return _instance;
			}
		}


		public delegate HotKeyResult FUNC_HOTKEY_EVENT(object paramObject);
		public delegate HotKeyResult FUNC_RESV_HOTKEY_EVENT(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject);

		//변경 20.1.26 : Label을 string으로 받지 말고 enum으로 받은뒤, 미리 정의된 Label을 출력하자
		public enum LabelText
		{
			None,
			ToggleWorkspaceSize,
			Select,
			Move,
			Rotate,
			Scale,
			OnionSkinToggle,
			ChangeBoneVisiblity,
			IncreaseBrushSize,
			DecreaseBrushSize,
			RemovePolygon,
			SelectAllVertices,
			RemoveVertices,
			ToggleEditingMode,
			ToggleSelectionLock,
			ToggleModifierLock,
			ToggleLayerLock,
			AddNewKeyframe,
			RemoveKeyframe,
			RemoveKeyframes,
			PlayPause,
			PreviousFrame,
			NextFrame,
			FirstFrame,
			LastFrame,
			CopyKeyframes,
			PasteKeyframes,
			IncreaseBrushRadius,
			DecreaseBrushRadius,
			BrushMode_Add,
			BrushMode_Multiply,
			BrushMode_Blur,
			IncreaseBrushIntensity,
			DecreaseBrushIntensity,
			IncreaseRigWeight,
			DecreaseRigWeight,
			RemoveBone,
			DetachTrasnforms,

			//TODO
		}

		// Unit Class
		public class HotKeyEvent
		{
			public KeyCode _keyCode;
			//public string _label;//이전
			//public LabelText _labelType = LabelText.None;//이전
			public apStringWrapper _labelText = null;//변경 20.12.3 : Wrapper 방식으로 직접 지정
			public bool _isShift;
			public bool _isAlt;
			public bool _isCtrl;

			//추가 : Ctrl/Shift에 무관한 경우가 있다.
			public bool _isIgnoreCtrlShift = false;

			public object _paramObject;
			public FUNC_HOTKEY_EVENT _funcEvent;
			public bool _isCombination;

			public HotKeyEvent()
			{
				_keyCode = KeyCode.Space;
				//_label;
				_isShift = false;
				_isAlt = false;
				_isCtrl = false;
				_paramObject = null;
				_funcEvent = null;
				_isCombination = false;

				_isIgnoreCtrlShift = false;
			}

			//public void SetEvent(	FUNC_HOTKEY_EVENT funcEvent, 
			//						LabelText labelType,
			//						KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, 
			//						object paramObject)
			//{
			//	_funcEvent = funcEvent;
			//	//_label = label;//이전
			//	_labelType = labelType;//변경 20.1.26
			//	_keyCode = keyCode;
			//	_isShift = isShift;
			//	_isAlt = isAlt;
			//	_isCtrl = isCtrl;

			//	_isIgnoreCtrlShift = false;

			//	_isCombination = _isShift || _isAlt || _isCtrl;

			//	_paramObject = paramObject;
			//}

			////추가 20.7.19
			////단일 키 입력이지만, Ctrl/Shift인 경우에도 동작할 때.
			////다만 이 이벤트는 다른 단축키 입력이 해당되지 않을때 동작한다.
			//public void SetEventIgnoreCtrlShift(	FUNC_HOTKEY_EVENT funcEvent, 
			//										LabelText labelType,
			//										KeyCode keyCode, 
			//										object paramObject)
			//{
			//	_funcEvent = funcEvent;
			//	//_label = label;//이전
			//	_labelType = labelType;//변경 20.1.26
			//	_keyCode = keyCode;
			//	_isShift = false;
			//	_isAlt = false;
			//	_isCtrl = false;

			//	_isIgnoreCtrlShift = true;//<<이거

			//	_isCombination = false;

			//	_paramObject = paramObject;
			//}

			//변경 20.12.3
			//HotKeyUnit을 이용한 것으로 변경
			public void SetEvent(FUNC_HOTKEY_EVENT funcEvent,
									apHotKeyMapping.HotkeyMapUnit hotKeyUnit,
									object paramObject)
			{
				_funcEvent = funcEvent;
				
				_labelText = hotKeyUnit._label_Workspace;
				_keyCode = hotKeyUnit._keyCode_Converted;

				if(hotKeyUnit._isIgnoreSpecialKey)
				{
					_isShift = false;
					_isAlt = false;
					_isCtrl = false;
					_isIgnoreCtrlShift = true;
				}
				else
				{
					_isShift = hotKeyUnit._isShift_Cur;
					_isAlt = hotKeyUnit._isAlt_Cur;
					_isCtrl = hotKeyUnit._isCtrl_Cur;
					_isIgnoreCtrlShift = false;
				}
				
				_isCombination = _isShift || _isAlt || _isCtrl;

				_paramObject = paramObject;
			}
		}

		//추가 20.1.26 : 특정 단축키는 미리 저장했다가 사용한다.
		//복수개의 키 타입을 한개의 이벤트에 매핑할 수 있다.
		//KeyCode가 아닌 별도의 Enum을 이용한다.
		//PopEvent가 아니며, 고정 크기의 배열을 이용한다. (최대 개수가 정해져있음)
		//이 키들은 일반적이므로 Label이 없다.
		public enum RESERVED_KEY
		{
			None,
			Arrow,
			Arrow_Shift,
			Arrow_Ctrl,
			EnterOrEscape,
			Escape,
			Enter,
		}
		public class ReservedHotKeyEvent
		{
			public RESERVED_KEY _reservedHotkey = RESERVED_KEY.None;
			public bool _isShift = false;
			public bool _isCtrl = false;
			public bool _isAlt = false;
			public bool _isCombination = false;

			public FUNC_RESV_HOTKEY_EVENT _funcEvent = null;
			public object _paramObject = null;

			public bool _isRegistered = false;

			public ReservedHotKeyEvent(RESERVED_KEY keyType, bool isShift, bool isCtrl, bool isAlt)
			{
				_reservedHotkey = keyType;
				_isShift = isShift;
				_isCtrl = isCtrl;
				_isAlt = isAlt;

				_isCombination = _isShift || _isCtrl || _isAlt;

				_funcEvent = null;
				_paramObject = null;

				_isRegistered = false;
			}

			public void SetEvent(FUNC_RESV_HOTKEY_EVENT funcEvent, object paramObject)
			{
				_isRegistered = true;
				_funcEvent = funcEvent;
				_paramObject = paramObject;
			}

			public void ClearEvent()
			{
				_funcEvent = null;
				_paramObject = null;
				_isRegistered = false;
			}
		}


		//특수키의 딜레이 처리
		//특수키는 Up 이벤트 발생 후에도 아주 짧은 시간(0.3초)동안 Down 이벤트로 기록해야한다.
		public enum SPECIAL_KEY
		{
			Ctrl, Shift, Alt
		}

		//public enum SPECIAL_KEY_BUTTON_STATUS
		//{
		//	Pressed,
		//	Released,
		//	ReleasedButDelayed,//Released 상태이지만 Pressed 상태에서 얼마 지나지 않았다.
		//}

		private class SpecialKeyProcess
		{
			public SPECIAL_KEY _key;
			public bool _isPressed_Input;
			public bool _isPressed_Delayed;
			public System.Diagnostics.Stopwatch _timer = new System.Diagnostics.Stopwatch();

			//private const long DELAY_TIME_MSEC = 300;//0.3초
			private const long DELAY_TIME_MSEC = 150;//0.15초

			public SpecialKeyProcess(SPECIAL_KEY specialKey)
			{
				_key = specialKey;
				_isPressed_Input = false;
				_isPressed_Delayed = false;
			}

			public void OnKeyDown()
			{
				if (!_isPressed_Input)
				{
					_isPressed_Input = true;
					_isPressed_Delayed = true;
					_timer.Reset();
					_timer.Start();
				}
			}

			public void OnKeyUp()
			{
				if (_isPressed_Input)
				{
					_isPressed_Input = false;
					_isPressed_Delayed = true;//일단은 True

					_timer.Stop();
					_timer.Reset();
					_timer.Start();
					//Debug.LogWarning("Up > " + _key + " > 딜레이 체크 시작 : " + _timer.ElapsedMilliseconds);
				}
				else if(_isPressed_Delayed)
				{
					//딜레이 중이라면
					if (_timer.ElapsedMilliseconds > DELAY_TIME_MSEC)
					{
						_isPressed_Delayed = false;
						_timer.Stop();

						//Debug.LogError("Up > " + _key + " > 딜레이 끝 : " + _timer.ElapsedMilliseconds);
					}
					//else
					//{
					//	Debug.Log("Up > " + _key + " > 딜레이 중 : " + _timer.ElapsedMilliseconds + " (~" + DELAY_TIME_MSEC + ")");
					//}
				}
			}

			//미사용 코드
			public bool IsPressed()
			{
				if (_isPressed_Input)
				{
					//실제로 눌린 상태
					return true;
				}
				else
				{
					if (_isPressed_Delayed)
					{
						//일단은 딜레이 중이다.
						if (_timer.ElapsedMilliseconds > DELAY_TIME_MSEC)
						{
							_isPressed_Delayed = false;
							_timer.Stop();

							//Debug.LogError(_key + " > 딜레이 끝 : " + _timer.ElapsedMilliseconds);
						}
						//else
						//{
						//	Debug.Log(_key + " > 딜레이 중 : " + _timer.ElapsedMilliseconds);
						//}
						
					}

					return _isPressed_Delayed;

				}
			}


			public void ResetTimer()
			{
				if(!_isPressed_Input)
				{
					_isPressed_Delayed = false;
					_timer.Reset();
					_timer.Stop();
				}
			}
		}

		public enum EVENT_RESULT
		{
			None, NormalEvent, ReservedEvent
		}

		public class HotKeyCheck
		{
			public HotKeyEvent _checkedEvent = null;
			public HotKeyResult _checkedResult = null;

			public HotKeyCheck()
			{
				Clear();
			}

			public void Clear()
			{
				_checkedEvent = null;
				_checkedResult = null;
			}

			public void SetResult(HotKeyEvent checkedEvent, HotKeyResult checkedResult)
			{
				_checkedEvent = checkedEvent;
				_checkedResult = checkedResult;
			}
		}

		// Members
		//---------------------------------------------
		private int _nEvent = 0;
		private const int NUM_INIT_EVENT_POOL = 20;
		private List<HotKeyEvent> _hotKeyEvents = new List<HotKeyEvent>();

		//추가 20.7.19 : 단일 키 중에서 Ctrl, Shift가 눌려도 동작하는 단일키는 별도로 체크한다.
		private int _nEvent_woCS = 0;
		private List<HotKeyEvent> _hotKeyEvents_woCS = new List<HotKeyEvent>();

		//추가 20.1.27 : Reserved HotKey를 추가
		private Dictionary<RESERVED_KEY, ReservedHotKeyEvent> _reservedHotKeyEvents_Mapping = new Dictionary<RESERVED_KEY, ReservedHotKeyEvent>();
		private List<ReservedHotKeyEvent> _reservedHotKeyEvents = new List<ReservedHotKeyEvent>();
		private int _nReservedHotKeyEvent = 0;


		private bool _isAnyEvent_Normal = false;
		private bool _isAnyEvent_Reserved = false;


		private KeyCode _prevKey = KeyCode.None;
		private bool _prevCtrl = false;
		private bool _prevShift = false;
		private bool _prevAlt = false;

		private Dictionary<SPECIAL_KEY, SpecialKeyProcess> _specialKeys = new Dictionary<SPECIAL_KEY, SpecialKeyProcess>();


		//추가 20.1.26 : 최적화 코드
		//Label을 String 타입으로 받지 말고 미리 만든뒤 재활용하자
		private Dictionary<LabelText, apStringWrapper> _labels = new Dictionary<LabelText, apStringWrapper>();

		//추가 20.1.27 : 입력 연산 후 바로 리턴하지 말고, 결과값을 변수로 가지고 있자
		private HotKeyCheck _checkedResult = null;
		private HotKeyEvent _resultHotKeyEvent = null;
		private HotKeyResult _eventResult = null;
		private ReservedHotKeyEvent _resultReservedHotKeyEvent = null;

		// Init
		//---------------------------------------------
		public apHotKey()
		{
			_isAnyEvent_Normal = false;
			_isAnyEvent_Reserved = false;

			_resultHotKeyEvent = null;
			_eventResult = null;
			_resultReservedHotKeyEvent = null;

			_checkedResult = new HotKeyCheck();
			


			if (_hotKeyEvents == null)
			{
				_hotKeyEvents = new List<HotKeyEvent>();
			}
			_hotKeyEvents.Clear();

			for (int i = 0; i < NUM_INIT_EVENT_POOL; i++)
			{
				_hotKeyEvents.Add(new HotKeyEvent());
			}
			_nEvent = 0;


			//추가 20.7.19 : Ctrl과 Shift의 영향을 받지 않는 단축키를 위한 리스트
			if (_hotKeyEvents_woCS == null)
			{
				_hotKeyEvents_woCS = new List<HotKeyEvent>();
			}
			_hotKeyEvents_woCS.Clear();

			for (int i = 0; i < NUM_INIT_EVENT_POOL; i++)
			{
				_hotKeyEvents_woCS.Add(new HotKeyEvent());
			}
			_nEvent_woCS = 0;




			//추가 20.1.27 : 예약된 시스템 단축키 초기화
			InitReservedHotKeys();


			//_isLock = false;
			//_isSpecialKey_Ctrl = false;
			//_isSpecialKey_Shift = false;
			//_isSpecialKey_Alt = false;
			_prevKey = KeyCode.None;
			_prevCtrl = false;
			_prevShift = false;
			_prevAlt = false;

			if(_specialKeys == null)
			{
				_specialKeys = new Dictionary<SPECIAL_KEY, SpecialKeyProcess>();
			}
			_specialKeys.Clear();

			_specialKeys.Add(SPECIAL_KEY.Ctrl, new SpecialKeyProcess(SPECIAL_KEY.Ctrl));
			_specialKeys.Add(SPECIAL_KEY.Alt, new SpecialKeyProcess(SPECIAL_KEY.Alt));
			_specialKeys.Add(SPECIAL_KEY.Shift, new SpecialKeyProcess(SPECIAL_KEY.Shift));

			InitLabels();
		}




		//추가 : 단축키 Label을 여기서 만들자
		private void InitLabels()
		{
			if(_labels == null)
			{
				_labels = new Dictionary<LabelText, apStringWrapper>();
			}
			_labels.Clear();

			AddLabelText(LabelText.None, "None");
			AddLabelText(LabelText.ToggleWorkspaceSize, "Toggle Workspace Size");
			AddLabelText(LabelText.Select, "Select");
			AddLabelText(LabelText.Move, "Move");
			AddLabelText(LabelText.Rotate, "Rotate");
			AddLabelText(LabelText.Scale, "Scale");
			AddLabelText(LabelText.OnionSkinToggle, "Onion Skin Toggle");
			AddLabelText(LabelText.ChangeBoneVisiblity, "Change Bone Visiblity");
			AddLabelText(LabelText.IncreaseBrushSize, "Increase Brush Size");
			AddLabelText(LabelText.DecreaseBrushSize, "Decrease Brush Size");
			AddLabelText(LabelText.RemovePolygon, "Remove Polygon");
			AddLabelText(LabelText.SelectAllVertices, "Select All Vertices");
			AddLabelText(LabelText.RemoveVertices, "Remove Vertices");
			AddLabelText(LabelText.ToggleEditingMode, "Toggle Editing Mode");
			AddLabelText(LabelText.ToggleSelectionLock, "Toggle Selection Lock");
			AddLabelText(LabelText.ToggleModifierLock, "Toggle Modifier Lock");
			AddLabelText(LabelText.ToggleLayerLock,"Toggle Layer Lock");
			AddLabelText(LabelText.AddNewKeyframe, "Add New Keyframe");
			AddLabelText(LabelText.RemoveKeyframe, "Remove Keyframe");
			AddLabelText(LabelText.RemoveKeyframes, "Remove Keyframes");
			AddLabelText(LabelText.PlayPause, "Play/Pause");
			AddLabelText(LabelText.PreviousFrame, "Previous Frame");
			AddLabelText(LabelText.NextFrame, "Next Frame");
			AddLabelText(LabelText.FirstFrame, "First Frame");
			AddLabelText(LabelText.LastFrame, "Last Frame");
			AddLabelText(LabelText.CopyKeyframes, "Copy Keyframes");
			AddLabelText(LabelText.PasteKeyframes, "Paste Keyframes");
			AddLabelText(LabelText.IncreaseBrushRadius, "Increase Brush Radius");
			AddLabelText(LabelText.DecreaseBrushRadius, "Decrease Brush Radius");
			AddLabelText(LabelText.BrushMode_Add, "Brush Mode - Add");
			AddLabelText(LabelText.BrushMode_Multiply, "Brush Mode - Multiply");
			AddLabelText(LabelText.BrushMode_Blur, "Brush Mode - Blur");
			AddLabelText(LabelText.IncreaseBrushIntensity, "Increase Brush Intensity");
			AddLabelText(LabelText.DecreaseBrushIntensity, "Decrease Brush Intensity");
			AddLabelText(LabelText.IncreaseRigWeight, "Increase Weight");
			AddLabelText(LabelText.DecreaseRigWeight, "Decrease Weight");
			AddLabelText(LabelText.RemoveBone, "Remove Bone");
			AddLabelText(LabelText.DetachTrasnforms, "Detach Objects");
			
			//TODO : Label을 추가합니다.
		}

		private void AddLabelText(LabelText labelType, string text)
		{
			_labels.Add(labelType, apStringWrapper.MakeStaticText(text));
		}


		private void InitReservedHotKeys()
		{
			if(_reservedHotKeyEvents_Mapping == null)
			{
				_reservedHotKeyEvents_Mapping = new Dictionary<RESERVED_KEY, ReservedHotKeyEvent>();
			}
			if(_reservedHotKeyEvents == null)
			{
				_reservedHotKeyEvents = new List<ReservedHotKeyEvent>();
			}
			
			_reservedHotKeyEvents_Mapping.Clear();
			_reservedHotKeyEvents.Clear();

			AddReservedHotKeyEvent(RESERVED_KEY.None, false, false, false);
			AddReservedHotKeyEvent(RESERVED_KEY.Arrow, false, false, false);
			AddReservedHotKeyEvent(RESERVED_KEY.Arrow_Shift, true, false, false);
			AddReservedHotKeyEvent(RESERVED_KEY.Arrow_Ctrl, false, true, false);
			AddReservedHotKeyEvent(RESERVED_KEY.EnterOrEscape, false, false, false);
			AddReservedHotKeyEvent(RESERVED_KEY.Escape, false, false, false);
			AddReservedHotKeyEvent(RESERVED_KEY.Enter, false, false, false);
			

			//TODO : 만약 특수 시스템키가 추가되면 여기에 더 추가하자
			
			
			_nReservedHotKeyEvent = _reservedHotKeyEvents.Count;
		}

		private void AddReservedHotKeyEvent(RESERVED_KEY hotKeyType, bool isShift, bool isCtrl, bool isAlt)
		{
			ReservedHotKeyEvent newHotKeyEvent = new ReservedHotKeyEvent(hotKeyType, isShift, isCtrl, isAlt);
			newHotKeyEvent.ClearEvent();
			_reservedHotKeyEvents.Add(newHotKeyEvent);
			_reservedHotKeyEvents_Mapping.Add(hotKeyType, newHotKeyEvent);
		}


		/// <summary>
		/// OnGUI 초기에 호출해주자
		/// </summary>
		public void Clear()
		{
			if (_isAnyEvent_Normal)
			{
				_isAnyEvent_Normal = false;
				//_hotKeyEvents_Live.Clear();
				_nEvent = 0;
				_nEvent_woCS = 0;
			}

			if(_isAnyEvent_Reserved)
			{
				_isAnyEvent_Reserved = false;
				for (int i = 0; i < _nReservedHotKeyEvent; i++)
				{
					_reservedHotKeyEvents[i].ClearEvent();
				}
			}

			_resultHotKeyEvent = null;
			_eventResult = null;
			_resultReservedHotKeyEvent = null;
		}


		// Input Event
		//------------------------------------------------------------------------------
		//이전 : 바로 결과 HotKeyEvent 리턴
		//public apHotKey.HotKeyEvent OnKeyEvent(KeyCode keyCode, bool isCtrl, bool isShift, bool isAlt, bool isPressed)
		
		//변경 20.1.27 : 리턴값이 바로 안나오고, GetResultEvent / GetReservedResultEvent 함수로 가져오게 변경 (뭔가 결과가 발생하는지 체크 위한 bool 리턴)
		public EVENT_RESULT OnKeyEvent(KeyCode keyCode, bool isCtrl, bool isShift, bool isAlt, bool isPressed)
		{
			//if(isPressed)
			//{
			//	Debug.Log("Pressed : " + keyCode + "(Ctrl : " + isCtrl + " / Shift : " + isShift + " / Alt : " + isAlt + ")");
			//}
			//else
			//{
			//	Debug.LogWarning("Released : " + keyCode + "(Ctrl : " + isCtrl + " / Shift : " + isShift + " / Alt : " + isAlt + ")");
			//}

			//21.2.9 : 키코드가 특수키라면, bool로 변환하고 keyCode는 None으로 변환
			switch (keyCode)
			{
#if UNITY_EDITOR_OSX
				case KeyCode.LeftCommand:
				case KeyCode.RightCommand:
#else
				case KeyCode.LeftControl:
				case KeyCode.RightControl:
#endif
					isCtrl = isPressed;
					keyCode = KeyCode.None;
					break;

				case KeyCode.LeftShift:
				case KeyCode.RightShift:
					isShift = isPressed;
					keyCode = KeyCode.None;
					break;

				case KeyCode.LeftAlt:
				case KeyCode.RightAlt:
					isAlt = isPressed;
					keyCode = KeyCode.None;
					break;
			}

			//이건 삭제 21.2.9
			//if(keyCode == KeyCode.None)
			//{
			//	return EVENT_RESULT.None;
			//}
			
			
			if (isPressed)
			{
				//Pressed 이벤트의 경우, 너무 잦은 이벤트 호출이 문제다.
				//if (_isLock)
				//{
				//	Debug.LogWarning("키 입력 되었으나 Lock [" + keyCode + "]");
				//	return null;
				//}

				if (_prevKey == keyCode
					//스페셜키 조건 추가 21.2.9
					&& _prevCtrl == isCtrl
					&& _prevShift == isShift
					&& _prevAlt == isAlt)
				{
					//Debug.Log(">> 이전 키와 같음 : " + keyCode);
					return EVENT_RESULT.None;
				}

				_prevKey = keyCode;
			}
			else
			{
				_prevKey = KeyCode.None;

				//Up 이벤트에서도 특수키는 해제하지 않는다. (보조키 형태로 이벤트가 있었다면)
			}

			//특수키 반영
			_prevCtrl = isCtrl;
			_prevShift = isShift;
			_prevAlt = isAlt;



			//추가적으로, 유니티에서 제공하는 값에 따라서도 변동
			//이거 이상한데.
			//if (isPressed)
			//{
			//	//Pressed인 경우 False > True로만 보정
			//	if (isCtrl)
			//	{
			//		_specialKeys[SPECIAL_KEY.Ctrl].OnKeyDown();
			//	}
			//	if (isShift)
			//	{
			//		_specialKeys[SPECIAL_KEY.Shift].OnKeyDown();
			//	}
			//	if (isAlt)
			//	{
			//		_specialKeys[SPECIAL_KEY.Alt].OnKeyDown();
			//	}
			//}
			//else
			//{
			//	//Released인 경우 False > True로만 보정
			//	if (!isCtrl)
			//	{
			//		_specialKeys[SPECIAL_KEY.Ctrl].OnKeyUp();
			//	}
			//	if (!isShift)
			//	{
			//		_specialKeys[SPECIAL_KEY.Shift].OnKeyUp();
			//	}
			//	if (!isAlt)
			//	{
			//		_specialKeys[SPECIAL_KEY.Alt].OnKeyUp();
			//	}
			//}

			//변경 21.2.9 : 특수키는 Pressed 상관없이 각각 처리해야한다.
			if(isCtrl)	{ _specialKeys[SPECIAL_KEY.Ctrl].OnKeyDown(); }
			else		{ _specialKeys[SPECIAL_KEY.Ctrl].OnKeyUp(); }

			if(isShift)	{ _specialKeys[SPECIAL_KEY.Shift].OnKeyDown(); }
			else		{ _specialKeys[SPECIAL_KEY.Shift].OnKeyUp(); }

			if(isAlt)	{ _specialKeys[SPECIAL_KEY.Alt].OnKeyDown(); }
			else		{ _specialKeys[SPECIAL_KEY.Alt].OnKeyUp(); }


			//이전
			#region [미사용 코드]
			//			switch (keyCode)
			//			{		
			//				// Special Key
			//#if UNITY_EDITOR_OSX
			//				case KeyCode.LeftCommand:
			//				case KeyCode.RightCommand:
			//#else
			//				case KeyCode.LeftControl:
			//				case KeyCode.RightControl:
			//#endif
			//					if(isPressed)
			//					{
			//						_specialKeys[SPECIAL_KEY.Ctrl].OnKeyDown();
			//					}
			//					else
			//					{
			//						_specialKeys[SPECIAL_KEY.Ctrl].OnKeyUp();
			//					}
			//					break;

			//				case KeyCode.LeftShift:
			//				case KeyCode.RightShift:
			//					if(isPressed)
			//					{
			//						_specialKeys[SPECIAL_KEY.Shift].OnKeyDown();
			//					}
			//					else
			//					{
			//						_specialKeys[SPECIAL_KEY.Shift].OnKeyUp();
			//					}
			//					break;

			//				case KeyCode.LeftAlt:
			//				case KeyCode.RightAlt:
			//					if(isPressed)
			//					{
			//						_specialKeys[SPECIAL_KEY.Alt].OnKeyDown();
			//					}
			//					else
			//					{
			//						_specialKeys[SPECIAL_KEY.Alt].OnKeyUp();
			//					}
			//					break;

			//				default:
			//					//그 외의 키값이라면..
			//					//Up 이벤트에만 반응하자 > 변경
			//					//특수키가 있는 단축키 => Up 이벤트에서만 적용
			//					//특수키가 없는 단축키 => Down 이벤트에서만 적용
			//					//if (!isPressed)
			//					{
			//						//해당하는 이벤트가 있는가?
			//						//추가 20.1.27 : ReservedHotKeyEvent 먼저 체크한다.
			//						if (_isAnyEvent_Reserved)
			//						{
			//							ReservedHotKeyEvent reservedHotKeyEvent = CheckReservedHotKeyEvent(keyCode,
			//																			_specialKeys[SPECIAL_KEY.Shift].IsPressed(),
			//																			_specialKeys[SPECIAL_KEY.Alt].IsPressed(),
			//																			_specialKeys[SPECIAL_KEY.Ctrl].IsPressed(),
			//																			isPressed);

			//							if(reservedHotKeyEvent != null)
			//							{
			//								//Reserved 이벤트가 먼저 발생했다.
			//								_resultReservedHotKeyEvent = reservedHotKeyEvent;
			//							}
			//						}

			//						if (_resultReservedHotKeyEvent == null && _isAnyEvent_Normal)
			//						{
			//							//Reserved 이벤트가 발생하지 않고, 기본 이벤트가 등록되었다면
			//							HotKeyCheck checkedEvent = CheckHotKeyEvent(keyCode,
			//																		_specialKeys[SPECIAL_KEY.Shift].IsPressed(),
			//																		_specialKeys[SPECIAL_KEY.Alt].IsPressed(),
			//																		_specialKeys[SPECIAL_KEY.Ctrl].IsPressed(),
			//																		isPressed);

			//							if (checkedEvent != null)
			//							{
			//								//이벤트가 발생했다.
			//								_resultHotKeyEvent = checkedEvent._checkedEvent;
			//								_eventResult = checkedEvent._checkedResult;

			//								//////일단 이 메인 키를 누른 상태에서 Lock을 건다.
			//								////_isLock = true;
			//								//return hotkeyEvent;
			//							}
			//						}



			//						//딜레이 처리가 끝났다면 특수키 타이머를 리셋한다.
			//						_specialKeys[SPECIAL_KEY.Shift].ResetTimer();
			//						_specialKeys[SPECIAL_KEY.Alt].ResetTimer();
			//						_specialKeys[SPECIAL_KEY.Ctrl].ResetTimer();


			//					}

			//					break;
			//			} 
			#endregion


			//변경 21.2.9

			//if (isPressed)
			//{
			//	Debug.Log("Pressed : " + keyCode + " (Ctrl : " + isCtrl + " / Shift : " + isShift + " / Alt : " + isAlt + ")");
			//}
			//else
			//{
			//	Debug.LogWarning("Released : " + keyCode + " (Ctrl : " + isCtrl + " / Shift : " + isShift + " / Alt : " + isAlt + ")");
			//}

			if (keyCode != KeyCode.None)
			{
				
				//특수키가 있는 단축키 => Up 이벤트에서만 적용
				//특수키가 없는 단축키 => Down 이벤트에서만 적용
				//if (!isPressed)
				{
					//해당하는 이벤트가 있는가?
					//추가 20.1.27 : ReservedHotKeyEvent 먼저 체크한다.
					if (_isAnyEvent_Reserved)
					{
						ReservedHotKeyEvent reservedHotKeyEvent = CheckReservedHotKeyEvent(keyCode,
																		_specialKeys[SPECIAL_KEY.Shift].IsPressed(),
																		_specialKeys[SPECIAL_KEY.Alt].IsPressed(),
																		_specialKeys[SPECIAL_KEY.Ctrl].IsPressed(),
																		isPressed);

						if (reservedHotKeyEvent != null)
						{
							//Reserved 이벤트가 먼저 발생했다.
							_resultReservedHotKeyEvent = reservedHotKeyEvent;
						}
					}

					if (_resultReservedHotKeyEvent == null && _isAnyEvent_Normal)
					{
						//Reserved 이벤트가 발생하지 않고, 기본 이벤트가 등록되었다면
						HotKeyCheck checkedEvent = CheckHotKeyEvent(keyCode,
																	_specialKeys[SPECIAL_KEY.Shift].IsPressed(),
																	_specialKeys[SPECIAL_KEY.Alt].IsPressed(),
																	_specialKeys[SPECIAL_KEY.Ctrl].IsPressed(),
																	isPressed);

						if (checkedEvent != null)
						{
							//이벤트가 발생했다.
							_resultHotKeyEvent = checkedEvent._checkedEvent;
							_eventResult = checkedEvent._checkedResult;

							//////일단 이 메인 키를 누른 상태에서 Lock을 건다.
							////_isLock = true;
							//return hotkeyEvent;
						}
					}

					//딜레이 처리가 끝났다면 특수키 타이머를 리셋한다.
					_specialKeys[SPECIAL_KEY.Shift].ResetTimer();
					_specialKeys[SPECIAL_KEY.Alt].ResetTimer();
					_specialKeys[SPECIAL_KEY.Ctrl].ResetTimer();
				}
			}



			if(_resultReservedHotKeyEvent != null)
			{
				return EVENT_RESULT.ReservedEvent;
			}
			if(_resultHotKeyEvent != null)
			{
				return EVENT_RESULT.NormalEvent;
			}
			return EVENT_RESULT.None;
		}

		public void OnShiftKeyEvent(bool isShift)
		{
			if(_prevShift != isShift)
			{
				_prevShift = isShift;

				//Debug.LogError("Shift 키는 여기서 따로 처리한다. - " + isShift);
				if(isShift)	{ _specialKeys[SPECIAL_KEY.Shift].OnKeyDown(); }
				else		{ _specialKeys[SPECIAL_KEY.Shift].OnKeyUp(); }
			}
		}

		public HotKeyEvent GetResultEvent()
		{
			return _resultHotKeyEvent;
		}

		public HotKeyResult GetResultAfterCallback()
		{
			return _eventResult;
		}

		public ReservedHotKeyEvent GetResultReservedEvent()
		{
			return _resultReservedHotKeyEvent;
		}
		
			



		#region [미사용 코드]
		//		public apHotKey.HotKeyEvent OnKeyDown(KeyCode keyCode, bool isCtrl, bool isShift, bool isAlt)
		//		{
		//			Debug.Log("OnKeyDown : " + keyCode);
		//			if(keyCode == KeyCode.None)
		//			{
		//				return null;
		//			}

		//			if(_isLock)
		//			{
		//				Debug.LogWarning("키 입력 되었으나 Lock [" + keyCode + "]");
		//				return null;
		//			}

		//			if(_prevKey == keyCode)
		//			{
		//				Debug.Log(">> 이전 키와 같음 : " + keyCode);
		//				return null;
		//			}

		//			_prevKey = keyCode;

		//			Debug.LogWarning("Key Down : " + keyCode);


		//			//추가적으로, 유니티에서 제공하는 값에 따라서도 변동
		//			if(isCtrl)
		//			{
		//				_isSpecialKey_Ctrl = true;
		//			}
		//			if(isShift)
		//			{
		//				_isSpecialKey_Shift = true;
		//			}
		//			if (isAlt)
		//			{
		//				_isSpecialKey_Alt = true;
		//			}

		//			switch (keyCode)
		//			{		
		//				// Special Key
		//#if UNITY_EDITOR_OSX
		//				case KeyCode.LeftCommand:
		//				case KeyCode.RightCommand:
		//#else
		//				case KeyCode.LeftControl:
		//				case KeyCode.RightControl:
		//#endif
		//					_isSpecialKey_Ctrl = true;
		//					break;

		//				case KeyCode.LeftShift:
		//				case KeyCode.RightShift:
		//					_isSpecialKey_Shift = true;
		//					break;

		//				case KeyCode.LeftAlt:
		//				case KeyCode.RightAlt:
		//					_isSpecialKey_Alt = true;
		//					break;

		//				default:
		//					//그 외의 키값이라면..
		//					_mainKey = keyCode;

		//					//해당하는 이벤트가 있는가?
		//					apHotKey.HotKeyEvent hotkeyEvent = CheckHotKeyEvent(_mainKey, _isSpecialKey_Shift, _isSpecialKey_Alt, _isSpecialKey_Ctrl);
		//					if(hotkeyEvent != null)
		//					{
		//						//일단 이 메인 키를 누른 상태에서 Lock을 건다.
		//						_isLock = true;

		//						return hotkeyEvent;
		//					}
		//					break;
		//			}

		//			return null;

		//		}

		//		public void OnKeyUp(KeyCode keyCode, bool isCtrl, bool isShift, bool isAlt)
		//		{
		//			if(keyCode == KeyCode.None)
		//			{
		//				return;
		//			}

		//			//추가적으로, 유니티에서 제공하는 값에 따라서도 변동 (False로만)
		//			if(!isCtrl)
		//			{
		//				_isSpecialKey_Ctrl = false;
		//			}
		//			if(!isShift)
		//			{
		//				_isSpecialKey_Shift = false;
		//			}
		//			if (!isAlt)
		//			{
		//				_isSpecialKey_Alt = false;
		//			}

		//			Debug.LogError("Key Up : " + keyCode);
		//			_prevKey = KeyCode.None;

		//			//Lock을 풀 수 있을까
		//			switch (keyCode)
		//			{		
		//				// Special Key
		//#if UNITY_EDITOR_OSX
		//				case KeyCode.LeftCommand:
		//				case KeyCode.RightCommand:
		//#else
		//				case KeyCode.LeftControl:
		//				case KeyCode.RightControl:
		//#endif
		//					_isSpecialKey_Ctrl = false;
		//					break;

		//				case KeyCode.LeftShift:
		//				case KeyCode.RightShift:
		//					_isSpecialKey_Shift = false;
		//					break;

		//				case KeyCode.LeftAlt:
		//				case KeyCode.RightAlt:
		//					_isSpecialKey_Alt = false;
		//					break;

		//				default:
		//					if(keyCode == _mainKey)
		//					{
		//						//Lock을 풀자
		//						_isLock = false;

		//						Debug.Log("[" + _mainKey + "] 단축키 Lock 해제됨");
		//						_mainKey = KeyCode.ScrollLock;
		//					}
		//					break;
		//			}

		//		} 
		#endregion

		// Functions
		//------------------------------------------------------------------------------
		//1) 일반 단축키의 Add, Pop, Check 함수들
		
		//함수 인자 변경 (20.12.3)
		//public void AddHotKeyEvent(FUNC_HOTKEY_EVENT funcEvent, LabelText labelType, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		public void AddHotKeyEvent(FUNC_HOTKEY_EVENT funcEvent, apHotKeyMapping.HotkeyMapUnit hotKeyUnit, object paramObject)
		{
			//_hotKeyEvents_Live.Add(PopEvent(funcEvent, labelType, keyCode, isShift, isAlt, isCtrl, paramObject));

			//변경 20.7.19 : Pool 방식에서 인덱스 추가 방식
			if(_nEvent >= _hotKeyEvents.Count)
			{
				//리스트를 확장하자
				for (int i = 0; i < 5; i++)
				{
					_hotKeyEvents.Add(new HotKeyEvent());
				}
			}
			HotKeyEvent nextEvent = _hotKeyEvents[_nEvent];
			
			//이전
			//nextEvent.SetEvent(funcEvent, labelType, keyCode, isShift, isAlt, isCtrl, paramObject);

			//변경 20.12.3
			nextEvent.SetEvent(funcEvent, hotKeyUnit, paramObject);

			_nEvent++;
			_isAnyEvent_Normal = true;
		}

		//함수 인자 변경 (20.12.3)
		//public void AddHotKeyEventIgnoreCtrlShift(FUNC_HOTKEY_EVENT funcEvent, LabelText labelType, KeyCode keyCode, object paramObject)
		public void AddHotKeyEventIgnoreCtrlShift(FUNC_HOTKEY_EVENT funcEvent, apHotKeyMapping.HotkeyMapUnit hotKeyUnit, object paramObject)
		{
			//_hotKeyEvents_Live.Add(PopEventIgnoreCtrlShift(funcEvent, labelType, keyCode, paramObject));

			//변경 20.7.19 : Pool 방식에서 인덱스 추가 방식
			if(_nEvent_woCS >= _hotKeyEvents_woCS.Count)
			{
				//리스트를 확장하자
				for (int i = 0; i < 5; i++)
				{
					_hotKeyEvents_woCS.Add(new HotKeyEvent());
				}
			}
			HotKeyEvent nextEvent = _hotKeyEvents_woCS[_nEvent_woCS];
			
			//이전
			//nextEvent.SetEventIgnoreCtrlShift(funcEvent, labelType, keyCode, paramObject);

			//변경 20.12.3
			nextEvent.SetEvent(funcEvent, hotKeyUnit, paramObject);

			_nEvent_woCS++;

			_isAnyEvent_Normal = true;//이건 동일
		}

		#region [미사용 코드]
		//// 변경 3.25 : 매번 생성하는 방식에서 Pop 방식으로 변경
		//private HotKeyEvent PopEvent(FUNC_HOTKEY_EVENT funcEvent, LabelText labelType, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		//{
		//	if(_iEvent >= _hotKeyEvents_Pool.Count)
		//	{
		//		//두개씩 늘리자
		//		for (int i = 0; i < 2; i++)
		//		{
		//			_hotKeyEvents_Pool.Add(new HotKeyEvent());
		//		}

		//		//Debug.Log("입력 풀 부족 : " + _hotKeyEvents_Pool.Count + " [" + label + "]");
		//	}

		//	HotKeyEvent result = _hotKeyEvents_Pool[_iEvent];
		//	_iEvent++;
		//	result.SetEvent(funcEvent, labelType, keyCode, isShift, isAlt, isCtrl, paramObject);
		//	return result;
		//}



		//private HotKeyEvent PopEventIgnoreCtrlShift(FUNC_HOTKEY_EVENT funcEvent, LabelText labelType, KeyCode keyCode, object paramObject)
		//{
		//	if(_iEvent >= _hotKeyEvents_Pool.Count)
		//	{
		//		//두개씩 늘리자
		//		for (int i = 0; i < 2; i++)
		//		{
		//			_hotKeyEvents_Pool.Add(new HotKeyEvent());
		//		}

		//		//Debug.Log("입력 풀 부족 : " + _hotKeyEvents_Pool.Count + " [" + label + "]");
		//	}

		//	HotKeyEvent result = _hotKeyEvents_Pool[_iEvent];
		//	_iEvent++;
		//	result.SetEventIgnoreCtrlShift(funcEvent, labelType, keyCode, paramObject);
		//	return result;
		//} 
		#endregion



		//2) 예약된 단축키의 Add, Check
		public void AddReservedHotKey(FUNC_RESV_HOTKEY_EVENT funcEvent, RESERVED_KEY keyType, object paramObject)
		{
			_reservedHotKeyEvents_Mapping[keyType].SetEvent(funcEvent, paramObject);
			_isAnyEvent_Reserved = true;
		}



		/// <summary>
		/// OnGUI 후반부에 체크해준다.
		/// Event가 used가 아니라면 호출 가능
		/// </summary>
		/// <param name=""></param>
		public HotKeyCheck CheckHotKeyEvent(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, bool isPressed)
		{
			if (!_isAnyEvent_Normal)
			{
				return null;
			}

			_checkedResult.Clear();

			
			HotKeyEvent hkEvent = null;
			bool isValidSystemKey = false;
			bool isValidMainKey = false;

			if (_nEvent > 0)
			{
				//for (int i = 0; i < _hotKeyEvents_Live.Count; i++)//이전
				for (int i = 0; i < _nEvent; i++)//변경
				{
					//hkEvent = _hotKeyEvents_Live[i];//이전
					hkEvent = _hotKeyEvents[i];//변경

					//Pressed 이벤트 = 단일 키
					//Released 이벤트 = 조합 키
					// 위조건이 안맞으면 continue
					if ((isPressed && hkEvent._isCombination)
						|| (!isPressed && !hkEvent._isCombination))
					{
						//조합키인데 Pressed 이벤트이거나
						//단일키인데 Released 이벤트라면
						//패스
						continue;
					}

					isValidSystemKey = hkEvent._isShift == isShift &&
										hkEvent._isAlt == isAlt &&
										hkEvent._isCtrl == isCtrl;

					if (!isValidSystemKey)
					{
						continue;
					}

					isValidMainKey = hkEvent._keyCode == keyCode;

					if (!isValidMainKey)
					{
						//추가 20.4.1 : 입력이 유효하지 않은 경우, 일부 키는 한번 더 체크를 할 수 있다.
						isValidMainKey = CheckSecondaryKeyProcess(hkEvent._keyCode, keyCode);
					}

					if (!isValidMainKey)
					{
						continue;
					}

					try
					{
						//저장된 이벤트를 실행하자
						HotKeyResult eventResult = hkEvent._funcEvent(hkEvent._paramObject);
						
						_checkedResult.SetResult(hkEvent, eventResult);
						return _checkedResult;
					}
					catch (Exception ex)
					{
						Debug.LogError("HotKey Event Exception : " + ex);
						return null;
					}
				}
			}



			//추가 20.7.19 : 일반 단축키 이벤트가 발생하지 않으면, Ctrl/Shift키에 무관한 단축키 이벤트를 체크하자
			//단, Alt키가 눌렸다면 모두 소용없다.
			//Ctrl/Shift 무관 키는 무조건 Pressed 이벤트에서 동작한다.
			if (_nEvent_woCS > 0 && !isAlt && isPressed)
			{
				for (int i = 0; i < _nEvent_woCS; i++)//변경
				{
					hkEvent = _hotKeyEvents_woCS[i];//변경

					isValidMainKey = hkEvent._keyCode == keyCode;

					if (!isValidMainKey)
					{
						//추가 20.4.1 : 입력이 유효하지 않은 경우, 일부 키는 한번 더 체크를 할 수 있다.
						isValidMainKey = CheckSecondaryKeyProcess(hkEvent._keyCode, keyCode);
					}

					if (!isValidMainKey)
					{
						continue;
					}

					try
					{
						//저장된 이벤트를 실행하자
						HotKeyResult eventResult = hkEvent._funcEvent(hkEvent._paramObject);

						_checkedResult.SetResult(hkEvent, eventResult);
						return _checkedResult;
					}
					catch (Exception ex)
					{
						Debug.LogError("HotKey Event Exception : " + ex);
						return null;
					}
				}
			}


			return null;
		}


		











		public ReservedHotKeyEvent CheckReservedHotKeyEvent(
			KeyCode keyCode, 
			bool isShift, bool isAlt, bool isCtrl, 
			bool isPressed)
		{
			if(!_isAnyEvent_Reserved)
			{
				return null;
			}

			ReservedHotKeyEvent hkEvent = null;
			bool isValidInput = false;
			for (int i = 0; i < _nReservedHotKeyEvent; i++)
			{
				hkEvent = _reservedHotKeyEvents[i];
				
				if(!hkEvent._isRegistered)
				{
					//이벤트가 등록 안되었으면 패스
					continue;
				}

				if((isPressed && hkEvent._isCombination)
					|| (!isPressed && !hkEvent._isCombination))
				{
					//조합키인데 Pressed 이벤트이거나
					//단일키인데 Released 이벤트라면
					//패스
					continue;
				}

				

				//키 입력 체크는 여기서 직접 한다.
				//TODO : 키가 추가되면 여기에 코드를 추가하자
				isValidInput = false;

				switch (hkEvent._reservedHotkey)
				{
					case RESERVED_KEY.Arrow:
						if(
							(keyCode == KeyCode.DownArrow || keyCode == KeyCode.UpArrow || keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow)
							&& !isShift && !isAlt && !isCtrl
							)
						{
							//방향키 + 특수키 없음
							isValidInput = true;
						}
						break;

					case RESERVED_KEY.Arrow_Ctrl:
						if(
							(keyCode == KeyCode.DownArrow || keyCode == KeyCode.UpArrow || keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow)
							&& !isShift && !isAlt && isCtrl
							)
						{
							//방향키 + Ctrl
							isValidInput = true;
						}
						break;

					case RESERVED_KEY.Arrow_Shift:
						if(
							(keyCode == KeyCode.DownArrow || keyCode == KeyCode.UpArrow || keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow)
							&& isShift && !isAlt && !isCtrl
							)
						{
							//방향키 + Shift
							isValidInput = true;
						}
						break;

					case RESERVED_KEY.EnterOrEscape:
						if(keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter || keyCode == KeyCode.Escape)
						{
							//Enter or Escape
							isValidInput = true;
						}
						break;
						
					case RESERVED_KEY.Escape:
						if(keyCode == KeyCode.Escape)
						{
							//Esc
							isValidInput = true;
						}
						break;

					case RESERVED_KEY.Enter:
						if(keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter)
						{
							//Enter
							isValidInput = true;
						}
						break;

					case RESERVED_KEY.None:
						//>> 이건 아무 처리도 안한다.
						break;
				}

				if(isValidInput)
				{
					//저장된 이벤트를 실행하자
					try
					{
						hkEvent._funcEvent(keyCode, isShift, isAlt, isCtrl, hkEvent._paramObject);

						return hkEvent;
					}
					catch(Exception ex)
					{
						Debug.LogError("AnyPortrait : HotKey Event Exception : " + ex);
						return null;
					}
				}
			}
			return null;
		}



		//추가 20.4.2 : 특정 키들은 등록된 이벤트와 달라도 대체 키들이 같다면 인식이 되어야 한다.
		public bool CheckSecondaryKeyProcess(KeyCode srcKey, KeyCode inputKey)
		{
			switch (srcKey)
			{	
				//Delete키가 없는 Mac을 고려하여 Backspace도 같이 생각해봐야 한다.
				case KeyCode.Delete: return inputKey == KeyCode.Backspace;
				case KeyCode.Backspace: return inputKey == KeyCode.Delete;
			}

			return false;
		}


		// Get / Set
		//---------------------------------------------
		//삭제 20.12.3
		//public apStringWrapper GetText(HotKeyEvent hotkeyEvent)
		//{
		//	return _labels[hotkeyEvent._labelType];
		//}
	}

}