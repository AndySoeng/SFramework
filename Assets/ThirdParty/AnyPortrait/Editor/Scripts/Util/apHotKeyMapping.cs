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
using System.Text;
//using System.Diagnostics;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 20.11.30 : 단축키를 매핑해서 사용할 수 있도록 레이어를 하나 더 둔다.
	/// 변경된 내용은 저장을 한다. (레지스트리를 적게 사용하도록 문자열 조합을 저장하자)
	/// </summary>
	public class apHotKeyMapping
	{
		/// <summary>
		/// 실제 사용되는 단축키.
		/// 단축키 추가시 1) enum을 추가하고, 2) DataUnit 초기화 코드를 작성하자
		/// </summary>
		public enum KEY_TYPE : int
		{
			//정말 공통
			Save,//RESV
			Undo,//RESV
			Redo,//RESV
			
			//기즈모
			Gizmo_Select,
			Gizmo_Move,
			Gizmo_Rotate,
			Gizmo_Scale,

			GizmoControl_Up,//RESV
			GizmoControl_Down,//RESV
			GizmoControl_Left,//RESV
			GizmoControl_Right,//RESV

			GizmoControl_x10_Up,//RESV
			GizmoControl_x10_Down,//RESV
			GizmoControl_x10_Left,//RESV
			GizmoControl_x10_Right,//RESV

			RemoveObject,//오브젝트 삭제 //RESV

			RenameObject,//오브젝트 이름 바꾸기 F2 //RESV

			//화면 설정
			ToggleWorkspaceSize,//전체 화면
			ToggleInvertBGColor,//배경 색상 반전 (21.10.6)

			ToggleOnionSkin,//Onion 단축키
			ToggleBoneVisibility,//Bone 가시성 단축키
			//가시성 단축키 추가
			ToggleMeshVisibility,//Mesh 가시성 단축키
			TogglePhysicsPreview,//물리 가시성 단축키
			TogglePresetVisibility,//프리셋 가시성 단축키
			PresetVisibilityCustomRule1,//커스텀 프리셋 규칙 1
			PresetVisibilityCustomRule2,//커스텀 프리셋 규칙 2
			PresetVisibilityCustomRule3,//커스텀 프리셋 규칙 3
			PresetVisibilityCustomRule4,//커스텀 프리셋 규칙 4
			PresetVisibilityCustomRule5,//커스텀 프리셋 규칙 5
			ToggleRotoscoping,//로토스코핑 가시성 단축키
			RotoscopingNext,//로토스코핑 다음 페이지
			RotoscopingPrev,//로토스코핑 이전 페이지

			ToggleGuidelines,//추가 21.6.4 : 가이드라인 보이기


			SwitchLeftTab,//왼쪽 탭 전환 (`)
			
			//메시 편집
			MakeMesh_SelectAllVertices,//RESV
			MakeMesh_RemovePolygon,//RESV
			MakeMesh_RemoveVertex,//RESV
			MakeMesh_RemoveVertex_KeepEdge,//RESV

			MakeMesh_MakePolygon,//RESV, Enter

			MakeMesh_Tab_1_Setting,//1
			MakeMesh_Tab_2_MakeMesh_Add,//2
			MakeMesh_Tab_2_MakeMesh_Edit,//3
			MakeMesh_Tab_2_MakeMesh_Auto,//4
			MakeMesh_Tab_3_Pivot,//5
			MakeMesh_Tab_4_Modify,//6
			MakeMesh_Tab_5_Pin,//7

			//모디파이어 / 애니메이션 편집
			ToggleEditingMode,
			ToggleSelectionLock,
			//ToggleModifierLock,//삭제 21.2.13 : 모디파이어 잠금 기능은 없어지고 그냥 편집 기능과 통합된다.
			//모디파이어 잠금 대신 다른 단축키가 추가된다.
			ExObj_UpdateByOtherMod,
			ExObj_ShowAsGray,
			ExObj_ToggleSelectionSemiLock,
			PreviewModBoneResult,
			PreviewModColorResult,
			ShowModifierListUI,
			ToggleMorphTarget,//모핑 타겟을 버텍스<->핀으로 전환한다.

			IncreaseModToolBrushSize,
			DecreaseModToolBrushSize,

			ApplyFFD,//RESV
			RevertFFD,//RESV

			SelectAllVertices_EditMod,//RESV
			

			//애니메이션
			Anim_PlayPause,
			Anim_MovePrevFrame,
			Anim_MoveNextFrame,
			Anim_MoveFirstFrame,
			Anim_MoveLastFrame,

			Anim_MovePrevKeyframe,//추가 : 단순히 프레임이 아닌, 키프레임의 위치를 찾아서 이동한다.
			Anim_MoveNextKeyframe,

			Anim_AddKeyframes,
			Anim_RemoveKeyframes,//RESV
			Anim_CopyKeyframes,//RESV
			Anim_PasteKeyframes,//RESV

			Anim_TimelineScrollUp,//기본 PageUp
			Anim_TimelineScrollDown,//기본 PageDown
			
			Anim_Curve_Linear,			//F7
			Anim_Curve_Constant,		//F8
			Anim_Curve_Smooth_Default,	//F9
			Anim_Curve_Smooth_AccAndDec,//F10
			Anim_Curve_Smooth_Acc,		//F11
			Anim_Curve_Smooth_Dec,		//F12
			

			Anim_ToggleAutoKey,		//N

			//리깅
			Rig_IncreaseWeight_02,
			Rig_DecreaseWeight_02,
			Rig_IncreaseWeight_05,
			Rig_DecreaseWeight_05,
			Rig_BrushMode_Add,
			Rig_BrushMode_Multiply,
			Rig_BrushMode_Blur,
			Rig_IncreaseBrushSize,
			Rig_DecreaseBrushSize,
			Rig_IncreaseBrushIntensity,
			Rig_DecreaseBrushIntensity,

		}


		/// <summary>
		/// 단축키가 발생하는 화면. 어떤 상태에서 단축키가 추가되는지 확인하고, 중복 체크를 할 수 있다.
		/// </summary>
		public enum SPACE : int
		{
			Common = 0,//공통
			MakeMesh = 1,//메시 제작시
			Modifier_Anim_Editing = 2,//편집모드
			AnimPlayBack = 3,//애니메이션 재생
			Rigging = 4,//리깅 편집
		}


		//KeyCode 중에서 단축키로 쓸만한 키코드만 모아둔 것
		//저장을 이걸로 하되, 변환을 해두자
		public enum EST_KEYCODE : int
		{
			//알파벳
			A = 0, B = 1, C = 2, D = 3, E = 4, F = 5, G = 6, H = 7, I = 8, J = 9, 
			K = 10, L = 11, M = 12, N = 13, O = 14, P = 15, Q = 16, R = 17, S = 18, T = 19, 
			U = 20, V = 21, W = 22, X = 23, Y = 24, Z = 25,
			//위 숫자
			Alpha0 = 26, Alpha1 = 27, Alpha2 = 28, Alpha3 = 29, Alpha4 = 30, 
			Alpha5 = 31, Alpha6 = 32, Alpha7 = 33, Alpha8 = 34, Alpha9 = 35, 
			//특수 문자들
			Minus = 36, Plus_Equal = 37, //-, +(=)
			LeftBracket = 38, RightBracket = 39, Backslash = 40, //[, ], \
			Semicolon = 41, Quote = 42, BackQuote = 43, //;, ', `
			Comma_Less = 44, Period_Greater = 45, Slash_Question = 46, //,(<), .(>), /(?)
			//키패드 글자
			Pad0 = 47, Pad1 = 48, Pad2 = 49, Pad3 = 50, Pad4 = 51, 
			Pad5 = 52, Pad6 = 53, Pad7 = 54, Pad8 = 55, Pad9 = 56,
			//특수키
			F1 = 57, F2 = 58, F3 = 59, F4 = 60, F5 = 61, 
			F6 = 62, F7 = 63, F8 = 64, F9 = 65, F10 = 66,
			F11 = 67, F12 = 68,
			Home = 69, End = 70, PageUp = 71, PageDown = 72, Space = 73,
						
			//여기서부터는 단축키로 지정될 수 없다.
			//방향키
			UpArrow = 74, DownArrow = 75, LeftArrow = 76, RightArrow = 77,
			
			//기본키
			Enter = 78, Esc = 79, Delete = 80,

			//지정될 수 없는 글자
			Unknown = 81,


		}

		public enum SPECIAL_KEY : int
		{
			None = 0,
			Ctrl = 1,
			Shift = 2,
			Alt = 3,
			Ctrl_Shift = 4,
			Ctrl_Alt = 5,
			Shift_Alt = 6,
			Ctrl_Shift_Alt = 7,
		}



		// Data Unit
		//---------------------------------------------------
		public class HotkeyMapUnit
		{	
			public string _ID = "";//저장이 되는 ID. 혹시라도 Enum을 쓸때 ID가 바뀔수 있으니 이렇게 조치한다.
			public KEY_TYPE _hotKeyType = KEY_TYPE.Gizmo_Select;
			public SPACE _eventSpace = SPACE.Common;

			//키코드 (단 필수 키코드만 이용)
			public EST_KEYCODE _keyCode_Cur = EST_KEYCODE.Unknown;//현재
			public EST_KEYCODE _keyCode_Def = EST_KEYCODE.Unknown;//기본 (복구용)

			public KeyCode _keyCode_Converted = KeyCode.None;

			//특수키 여부
			public bool _isIgnoreSpecialKey = false;//특수키 상관없이 동작 (변경 불가)

			public bool _isCtrl_Cur = false;
			public bool _isShift_Cur = false;
			public bool _isAlt_Cur = false;

			public bool _isCtrl_Def = false;
			public bool _isShift_Def = false;
			public bool _isAlt_Def = false;

			//변경 가능 여부
			public bool _isReserved = false;//이게 True면 변경 불가

			//설명에 나올 텍스트
			public apStringWrapper _label = null;
			public apStringWrapper _info = null;

			//단축키를 누를때 작업 공간에 나올 Label
			public apStringWrapper _label_Workspace = null;

			//단축키로 동작하는가 (대부분 true)
			public bool _isAvailable_Cur = false;
			public bool _isAvailable_Def = false;

			//충돌 여부 (충돌된 HotkeyMapUnit를 넣자. 경우에 따라 서로 넣을 수 있는데, Reserved의 경우는 제외)
			public HotkeyMapUnit _conflictedUnit = null;


			//초기값을 넣자
			public HotkeyMapUnit(	string ID,
									KEY_TYPE hotKeyType,
									SPACE eventSpace,
									EST_KEYCODE keyCode_Def,
									bool isIgnoreSpecialKey,
									bool isCtrl_Def,
									bool isShift_Def,
									bool isAlt_Def,
									bool isReserved,
									bool isAvailable,
									string label,
									string info,
									string label_WorkSpace)
			{
				_ID = ID;
				_hotKeyType = hotKeyType;
				_eventSpace = eventSpace;

				_keyCode_Cur = keyCode_Def;
				_keyCode_Def = keyCode_Def;

				_keyCode_Converted = apHotKeyMapping.Essential2KeyCode(_keyCode_Cur);

				_isIgnoreSpecialKey = isIgnoreSpecialKey;

				_isCtrl_Cur = isCtrl_Def;
				_isShift_Cur = isShift_Def;
				_isAlt_Cur = isAlt_Def;

				_isCtrl_Def = isCtrl_Def;
				_isShift_Def = isShift_Def;
				_isAlt_Def = isAlt_Def;

				_isReserved = isReserved;
				_isAvailable_Cur = isAvailable;
				_isAvailable_Def = isAvailable;
				

				//설명에 나올 텍스트
				_label = apStringWrapper.MakeStaticText(label);
				_info = apStringWrapper.MakeStaticText(info);

				//작업 공간에 나올 텍스트
				//null이거나 공백이라면 null로 처리
				if(string.IsNullOrEmpty(label_WorkSpace))
				{
					_label_Workspace = null;
				}
				else
				{
					_label_Workspace = apStringWrapper.MakeStaticText(label_WorkSpace);
				}
				


				_conflictedUnit = null;
			}

			//사용자에 의해 변경이 되었는가
			public bool IsChanged()
			{
				if(_isReserved)
				{
					return false;
				}
				return _keyCode_Cur != _keyCode_Def 
					|| _isCtrl_Cur != _isCtrl_Def
					|| _isShift_Cur != _isShift_Def
					|| _isAlt_Cur != _isAlt_Def
					|| _isAvailable_Cur != _isAvailable_Def;
				
			}

			public void Restore()
			{
				if (_isReserved)
				{
					return;
				}

				_keyCode_Cur = _keyCode_Def;
				_isCtrl_Cur = _isCtrl_Def;
				_isShift_Cur = _isShift_Def;
				_isAlt_Cur = _isAlt_Def;
				_isAvailable_Cur = _isAvailable_Def;

				_keyCode_Converted = apHotKeyMapping.Essential2KeyCode(_keyCode_Cur);
			}

			public void SetCurrentValue(EST_KEYCODE keyCode,
									bool isCtrl,
									bool isShift,
									bool isAlt,
									bool isAvailable)
			{
				if (_isReserved)
				{
					return;
				}

				_keyCode_Cur = keyCode;
				_isCtrl_Cur = isCtrl;
				_isShift_Cur = isShift;
				_isAlt_Cur = isAlt;
				_isAvailable_Cur = isAvailable;

				_keyCode_Converted = apHotKeyMapping.Essential2KeyCode(_keyCode_Cur);
			}

			public void SetValue_Available(bool isAvailable)
			{
				if (_isReserved)
				{
					return;
				}

				_isAvailable_Cur = isAvailable;
			}

			public void SetValue_SpecialKey(SPECIAL_KEY specialKeys)
			{
				if (_isReserved)
				{
					return;
				}

				switch (specialKeys)
				{
					case SPECIAL_KEY.None:				_isCtrl_Cur = false;	_isShift_Cur = false;	_isAlt_Cur = false;		break;
					case SPECIAL_KEY.Ctrl:				_isCtrl_Cur = true;		_isShift_Cur = false;	_isAlt_Cur = false;		break;
					case SPECIAL_KEY.Shift:				_isCtrl_Cur = false;	_isShift_Cur = true;	_isAlt_Cur = false;		break;
					case SPECIAL_KEY.Alt:				_isCtrl_Cur = false;	_isShift_Cur = false;	_isAlt_Cur = true;		break;
					case SPECIAL_KEY.Ctrl_Shift:		_isCtrl_Cur = true;		_isShift_Cur = true;	_isAlt_Cur = false;		break;
					case SPECIAL_KEY.Ctrl_Alt:			_isCtrl_Cur = true;		_isShift_Cur = false;	_isAlt_Cur = true;		break;
					case SPECIAL_KEY.Shift_Alt:			_isCtrl_Cur = false;	_isShift_Cur = true;	_isAlt_Cur = true;		break;
					case SPECIAL_KEY.Ctrl_Shift_Alt:	_isCtrl_Cur = true;		_isShift_Cur = true;	_isAlt_Cur = true;		break;
				}
			}

			public void SetValue_KeyCode(EST_KEYCODE keyCode)
			{
				if (_isReserved)
				{
					return;
				}

				_keyCode_Cur = keyCode;
				_keyCode_Converted = apHotKeyMapping.Essential2KeyCode(_keyCode_Cur);
			}

			public SPECIAL_KEY GetSpecialKeyComb()
			{
				if(_isIgnoreSpecialKey)
				{
					return SPECIAL_KEY.None;
				}
				if(_isCtrl_Cur)
				{
					if(_isShift_Cur)
					{
						if(_isAlt_Cur)
						{
							return SPECIAL_KEY.Ctrl_Shift_Alt;
						}
						else
						{
							return SPECIAL_KEY.Ctrl_Shift;
						}
					}
					else
					{
						if(_isAlt_Cur)
						{
							return SPECIAL_KEY.Ctrl_Alt;
						}
						else
						{
							return SPECIAL_KEY.Ctrl;
						}
					}
				}
				else
				{
					if(_isShift_Cur)
					{
						if(_isAlt_Cur)
						{
							return SPECIAL_KEY.Shift_Alt;
						}
						else
						{
							return SPECIAL_KEY.Shift;
						}
					}
					else
					{
						if(_isAlt_Cur)
						{
							return SPECIAL_KEY.Alt;
						}
						else
						{
							return SPECIAL_KEY.None;
						}
					}
				}
			}


			public bool IsInvalidKeyCode()
			{
				if(_keyCode_Cur == EST_KEYCODE.UpArrow
					|| _keyCode_Cur == EST_KEYCODE.DownArrow
					|| _keyCode_Cur == EST_KEYCODE.LeftArrow
					|| _keyCode_Cur == EST_KEYCODE.RightArrow
					|| _keyCode_Cur == EST_KEYCODE.Enter
					|| _keyCode_Cur == EST_KEYCODE.Esc
					|| _keyCode_Cur == EST_KEYCODE.Delete
					|| _keyCode_Cur == EST_KEYCODE.Unknown
					)
				{
					return true;
				}
				return false;
			}
		}


		// Members
		//---------------------------------------------------
		public bool _isLoaded = false;
		public List<HotkeyMapUnit> _units_All = new List<HotkeyMapUnit>();
		public Dictionary<KEY_TYPE, HotkeyMapUnit> _hotKeyType2Unit = new Dictionary<KEY_TYPE, HotkeyMapUnit>();
		public Dictionary<string, HotkeyMapUnit> _ID2Unit = new Dictionary<string, HotkeyMapUnit>();
		public Dictionary<SPACE, List<HotkeyMapUnit>> _eventSpace2UnitList = new Dictionary<SPACE, List<HotkeyMapUnit>>();

		private string[] _keycodeTexts = null;
		private string[] _specialTexts = null;



		// Init
		//---------------------------------------------------
		public apHotKeyMapping()
		{
			Init();
		}

		private void Init()
		{
			//전체 초기화
			_isLoaded = false;
			if(_units_All == null)
			{
				_units_All = new List<HotkeyMapUnit>();
			}
			_units_All.Clear();

			if(_hotKeyType2Unit == null)
			{
				_hotKeyType2Unit = new Dictionary<KEY_TYPE, HotkeyMapUnit>();
			}
			_hotKeyType2Unit.Clear();

			if(_ID2Unit == null)
			{
				_ID2Unit = new Dictionary<string, HotkeyMapUnit>();
			}	
			_ID2Unit.Clear();

			if(_eventSpace2UnitList == null)
			{
				_eventSpace2UnitList = new Dictionary<SPACE, List<HotkeyMapUnit>>();
			}
			_eventSpace2UnitList.Clear();

			_eventSpace2UnitList.Add(SPACE.Common, new List<HotkeyMapUnit>());
			_eventSpace2UnitList.Add(SPACE.MakeMesh, new List<HotkeyMapUnit>());
			_eventSpace2UnitList.Add(SPACE.Modifier_Anim_Editing, new List<HotkeyMapUnit>());
			_eventSpace2UnitList.Add(SPACE.AnimPlayBack, new List<HotkeyMapUnit>());
			_eventSpace2UnitList.Add(SPACE.Rigging, new List<HotkeyMapUnit>());


			//string으로도 추가
			_keycodeTexts = new string[]
			{
				"A", "B", "C", "D", "E", "F", "G", "H", "I", 
				"J", "K", "L", "M", "N", "O", "P", "Q", "R", 
				"S", "T", "U", "V", "W", "X", "Y", "Z",
			
				"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",

				"- _", "= +",
				"[ {", "] }", "\\ |",
				";", "' \"", "` ~",
				", <", ". >", "/ ?",
					
				"Numpad 0", "Numpad 1", "Numpad 2", "Numpad 3", "Numpad 4",
				"Numpad 5", "Numpad 6", "Numpad 7", "Numpad 8", "Numpad 9",
					
				"F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
				"Home", "End", "PageUp", "PageDown", "Space Bar",

				"▲ (Up Arrow)", "▼ (Down Arrow)", "◀ (Left Arrow)", "▶ (Right Arrow)",

				"Enter", "Esc", "Delete (Backspace)",

				"Unknown Key",
			};

			//특수키 표기는
			//Window는 Ctrl + Alt + Shift
			//OSX는 (Control) + Option + Shift + Command
			_specialTexts = new string[]
			{
#if UNITY_EDITOR_OSX
				"None",
				"Command",
				"Shift",
				"Option",
				"Shift+Command",
				"Option+Command",
				"Option+Shift",
				"Option+Shift+Command"
#else
				"None",
				"Ctrl",
				"Shift",
				"Alt",
				"Ctrl+Shift",
				"Ctrl+Alt",
				"Alt+Shift",
				"Ctrl+Alt+Shift"
#endif
			};


			//중요!
			//------------------------------------------------
			//여기에 직접 단축키들을 지정하자. TODO			
			

			AddHotKey_Reserved("SAVE", KEY_TYPE.Save, SPACE.Common, EST_KEYCODE.S, false, SPECIAL_KEY.Ctrl, "Save", "Save the current scene and work as a file.", "Save");
			AddHotKey_Reserved("UNDO", KEY_TYPE.Undo, SPACE.Common, EST_KEYCODE.Z, false, SPECIAL_KEY.Ctrl, "Undo", "Undo the current operation.", "Undo");
			AddHotKey_Reserved("REDO", KEY_TYPE.Redo, SPACE.Common, EST_KEYCODE.Y, false, SPECIAL_KEY.Ctrl, "Redo", "Redo the previous operation.", "Redo");

			AddHotKey_IgnoreSpecialKey("GIZMODE_SELECT",	KEY_TYPE.Gizmo_Select,	SPACE.Common, EST_KEYCODE.Q, true, "Gizmo : Select", "Switch Gizmo's mode to [Select].", "Select");
			AddHotKey_IgnoreSpecialKey("GIZMODE_MOVE",		KEY_TYPE.Gizmo_Move,	SPACE.Common, EST_KEYCODE.W, true, "Gizmo : Move", "Switch Gizmo's mode to [Move].", "Move");
			AddHotKey_IgnoreSpecialKey("GIZMODE_ROTATE",	KEY_TYPE.Gizmo_Rotate,	SPACE.Common, EST_KEYCODE.E, true, "Gizmo : Rotate", "Switch Gizmo's mode to [Rotate].", "Rotate");
			AddHotKey_IgnoreSpecialKey("GIZMODE_SCALE",		KEY_TYPE.Gizmo_Scale,	SPACE.Common, EST_KEYCODE.R, true, "Gizmo : Scale", "Switch Gizmo's mode to [Scale].", "Scale");

			AddHotKey_Reserved("GIZMO_UP",		KEY_TYPE.GizmoControl_Up,		SPACE.Common,	EST_KEYCODE.UpArrow,	true, SPECIAL_KEY.None, "Up : Move (1), Scale (0.01)", "Control the Gizmo to the Up direction.", "Up");
			AddHotKey_Reserved("GIZMO_DOWN",	KEY_TYPE.GizmoControl_Down,		SPACE.Common,	EST_KEYCODE.DownArrow,	true, SPECIAL_KEY.None, "Down : Move (1), Scale (0.01)", "Control the Gizmo to the Down direction." , "Down");
			AddHotKey_Reserved("GIZMO_LEFT",	KEY_TYPE.GizmoControl_Left,		SPACE.Common,	EST_KEYCODE.LeftArrow,	true, SPECIAL_KEY.None, "Left : Move (1), Rotate (1), Scale (0.01)", "Control the Gizmo to the Left direction.", "Left");
			AddHotKey_Reserved("GIZMO_RIGHT",	KEY_TYPE.GizmoControl_Right,	SPACE.Common,	EST_KEYCODE.RightArrow, true, SPECIAL_KEY.None, "Right : Move (1), Rotate (1), Scale (0.01)", "Control the Gizmo to the Right direction.", "Right");

			AddHotKey_Reserved("GIZMO_UP_X10",		KEY_TYPE.GizmoControl_x10_Up,		SPACE.Common,	EST_KEYCODE.UpArrow,		false, SPECIAL_KEY.Shift, "Up : Move (10), Scale (0.1)", "Control the Gizmo to the Up direction.", "Up");
			AddHotKey_Reserved("GIZMO_DOWN_X10",	KEY_TYPE.GizmoControl_x10_Down,		SPACE.Common,	EST_KEYCODE.DownArrow,		false, SPECIAL_KEY.Shift, "Down : Move (10), Scale (0.1)", "Control the Gizmo to the Down direction.", "Down");
			AddHotKey_Reserved("GIZMO_LEFT_X10",	KEY_TYPE.GizmoControl_x10_Left,		SPACE.Common,	EST_KEYCODE.LeftArrow,		false, SPECIAL_KEY.Shift, "Left : Move (10), Rotate (10), Scale (0.1)", "Control the Gizmo to the Left direction.", "Left");
			AddHotKey_Reserved("GIZMO_RIGHT_X10",	KEY_TYPE.GizmoControl_x10_Right,	SPACE.Common,	EST_KEYCODE.RightArrow,		false, SPECIAL_KEY.Shift, "Right : Move (10), Rotate (10), Scale (0.1)", "Control the Gizmo to the Right direction.", "Right");

			AddHotKey_Reserved("REMOVE_OBJECT",	KEY_TYPE.RemoveObject,	SPACE.Common,	EST_KEYCODE.Delete,		true,	SPECIAL_KEY.None, "Remove Objects", "Remove or detach the selected objects.", "Remove Objects");
			AddHotKey_Reserved("RENAME_OBJECT",	KEY_TYPE.RenameObject,	SPACE.Common,	EST_KEYCODE.F2,			false,	SPECIAL_KEY.None, "Rename", "Rename the selected object.", "Rename");

			AddHotKey("TOGGLE_WORKSPACE",	KEY_TYPE.ToggleWorkspaceSize,	SPACE.Common,	EST_KEYCODE.W,	SPECIAL_KEY.Alt,	true, "Maximize Workspace Size", "Maximize the size of the Workspace or restore it.", "Toggle Workspace Size");
			AddHotKey("TOGGLE_INVERT_BGCOLOR", KEY_TYPE.ToggleInvertBGColor, SPACE.Common, EST_KEYCODE.I, SPECIAL_KEY.Alt, true, "Invert Background Color", "Change the color of the background to the set inverted color.", "Toggle Background Color");

			AddHotKey("TOGGLE_ONIONSKIN",	KEY_TYPE.ToggleOnionSkin,		SPACE.Common,	EST_KEYCODE.O,	SPECIAL_KEY.None,	true, "Show Onion Skin", "Show or hide the Onion Skin.", "Toggle Onion Skin");
			AddHotKey("TOGGLE_BONEVISB",	KEY_TYPE.ToggleBoneVisibility,	SPACE.Common,	EST_KEYCODE.B,	SPECIAL_KEY.None,	true, "Show Bones", "Show or hide Bones.", "Toggle Bone Visibility");

			//가시성 단축키 추가 (21.1.21)
			AddHotKey("TOGGLE_MESHVISIB",		KEY_TYPE.ToggleMeshVisibility,			SPACE.Common,					EST_KEYCODE.M,	SPECIAL_KEY.None,		true, "Show Meshes", "Show or hide Meshes", "Toggle Mesh Visibility");//Mesh 가시성 단축키
			AddHotKey("TOGGLE_PHYSICSPREVIEW",	KEY_TYPE.TogglePhysicsPreview,			SPACE.Common,					EST_KEYCODE.P,	SPECIAL_KEY.None,		true, "Enable Physics Preview", "Enable the Preview of Physics Effects", "Toggle Physics");//물리 가시성 단축키
			AddHotKey("TOGGLE_PRESETVISIB",		KEY_TYPE.TogglePresetVisibility,		SPACE.Modifier_Anim_Editing,	EST_KEYCODE.I,		SPECIAL_KEY.None,	true, "Toggle Visibility Preset", "Turns the Visibility Preset on or off", "Toggle Visibility Preset");//프리셋 가시성 단축키
			AddHotKey("PRESETVISIB_RULE1",		KEY_TYPE.PresetVisibilityCustomRule1,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Alpha1,	SPECIAL_KEY.None,	true, "Visibility Rule 1", "Switch to Visibility Rule 1", "Visibility Rule 1");//커스텀 프리셋 규칙 1
			AddHotKey("PRESETVISIB_RULE2",		KEY_TYPE.PresetVisibilityCustomRule2,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Alpha2,	SPECIAL_KEY.None,	true, "Visibility Rule 2", "Switch to Visibility Rule 2", "Visibility Rule 2");//커스텀 프리셋 규칙 2
			AddHotKey("PRESETVISIB_RULE3",		KEY_TYPE.PresetVisibilityCustomRule3,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Alpha3,	SPECIAL_KEY.None,	true, "Visibility Rule 3", "Switch to Visibility Rule 3", "Visibility Rule 3");//커스텀 프리셋 규칙 3
			AddHotKey("PRESETVISIB_RULE4",		KEY_TYPE.PresetVisibilityCustomRule4,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Alpha4,	SPECIAL_KEY.None,	true, "Visibility Rule 4", "Switch to Visibility Rule 4", "Visibility Rule 4");//커스텀 프리셋 규칙 4
			AddHotKey("PRESETVISIB_RULE5",		KEY_TYPE.PresetVisibilityCustomRule5,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Alpha5,	SPECIAL_KEY.None,	true, "Visibility Rule 5", "Switch to Visibility Rule 5", "Visibility Rule 5");//커스텀 프리셋 규칙 5
			AddHotKey("TOGGLE_ROTOSCOPING",		KEY_TYPE.ToggleRotoscoping,				SPACE.Modifier_Anim_Editing,	EST_KEYCODE.O,		SPECIAL_KEY.Alt,	true, "Toggle Rotoscoping", "Shows an image of the external in the background of the workspace.", "Toggle Rotoscoping");//로토스코핑 가시성 단축키
			AddHotKey("ROTO_NEXT",				KEY_TYPE.RotoscopingNext,				SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Alpha0,	SPECIAL_KEY.None,	true, "Next image of Rotoscoping", "Switch to the next image of Rotoscoping", "Next Image");//로토스코핑 다음 페이지
			AddHotKey("ROTO_PREV",				KEY_TYPE.RotoscopingPrev,				SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Alpha9,	SPECIAL_KEY.None,	true, "Prev image of Rotoscoping", "Switch to the previous image of Rotoscoping", "Prev Image");//로토스코핑 이전 페이지

			AddHotKey("TOGGLE_GUIDELINES",	KEY_TYPE.ToggleGuidelines,				SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Semicolon,	SPECIAL_KEY.None,	true, "Show Guidelines", "Show or hide Guidelines", "Toggle Guidelines");//가이드라인 보이기


			AddHotKey("SWITCH_LEFT_TAB",	KEY_TYPE.SwitchLeftTab,	SPACE.Common,	EST_KEYCODE.BackQuote,	SPECIAL_KEY.None,	true, "Switch Left Tab", "Switch between the Hierarchy and Controller tabs on the left.", "Switch Left Tab");

			//메시 편집
			AddHotKey_Reserved("MESH_SELECT_ALL_VERTS",			KEY_TYPE.MakeMesh_SelectAllVertices,		SPACE.MakeMesh,		EST_KEYCODE.A,		false,	SPECIAL_KEY.Ctrl, "Select All Vertices", "Select all vertices during mesh modifying.", "Select All Vertices");
			AddHotKey_Reserved("MESH_REMOVE_POLYGONS",			KEY_TYPE.MakeMesh_RemovePolygon,			SPACE.MakeMesh,		EST_KEYCODE.Delete, true,	SPECIAL_KEY.None, "Remove Polygons", "Remove selected polygons which are faces surrounded by vertices.", "Remove Polygons");
			AddHotKey_Reserved("MESH_REMOVE_VERTEX",			KEY_TYPE.MakeMesh_RemoveVertex,				SPACE.MakeMesh,		EST_KEYCODE.Delete, false,	SPECIAL_KEY.None, "Remove Vertices", "Remove selected vertices.", "Remove Vertices");
			AddHotKey_Reserved("MESH_REMOVE_VERTEX_KEEPEDGE",	KEY_TYPE.MakeMesh_RemoveVertex_KeepEdge,	SPACE.MakeMesh,		EST_KEYCODE.Delete, false,	SPECIAL_KEY.Shift, "Remove Vertices (Preserving Edges)", "Remove vertices while preserving edges as much as possible", "Remove Vertices (Preserving Edges)");

			AddHotKey_Reserved("MESH_MAKE_POLYGON",				KEY_TYPE.MakeMesh_MakePolygon,				SPACE.MakeMesh,		EST_KEYCODE.Enter,	true,	SPECIAL_KEY.None, "Make Polygons", "Complete the editing and create the polygons of the mesh based on the information that the vertices are connected to.", "Make Polygons");

			AddHotKey("MESH_TAB_1_SETTING",		KEY_TYPE.MakeMesh_Tab_1_Setting,		SPACE.MakeMesh,	EST_KEYCODE.Alpha1,	SPECIAL_KEY.None,	true, "Mesh : Setting", "Switch to the [Setting] tab.", "Setting");
			AddHotKey("MESH_TAB_2_MAKE_ADD",	KEY_TYPE.MakeMesh_Tab_2_MakeMesh_Add,	SPACE.MakeMesh,	EST_KEYCODE.Alpha2,	SPECIAL_KEY.None,	true, "Mesh : Make Mesh - Add", "Switch to the [Add] tab of the [Make Mesh].", "Add");
			AddHotKey("MESH_TAB_2_MAKE_EDIT",	KEY_TYPE.MakeMesh_Tab_2_MakeMesh_Edit,	SPACE.MakeMesh,	EST_KEYCODE.Alpha3,	SPECIAL_KEY.None,	true, "Mesh : Make Mesh - Edit", "Switch to the [Edit] tab of the [Make Mesh].", "Edit");
			AddHotKey("MESH_TAB_2_MAKE_AUTO",	KEY_TYPE.MakeMesh_Tab_2_MakeMesh_Auto,	SPACE.MakeMesh,	EST_KEYCODE.Alpha4,	SPECIAL_KEY.None,	true, "Mesh : Make Mesh - Auto", "Switch to the [Auto] tab of the [Make Mesh].", "Auto");
			AddHotKey("MESH_TAB_3_PIVOT",		KEY_TYPE.MakeMesh_Tab_3_Pivot,			SPACE.MakeMesh,	EST_KEYCODE.Alpha5,	SPECIAL_KEY.None,	true, "Mesh : Pivot", "Switch to the [Pivot] tab.", "Pivot");
			AddHotKey("MESH_TAB_4_MODIFY",		KEY_TYPE.MakeMesh_Tab_4_Modify,			SPACE.MakeMesh,	EST_KEYCODE.Alpha6,	SPECIAL_KEY.None,	true, "Mesh : Modify", "Switch to the [Modify] tab.", "Modify");
			AddHotKey("MESH_TAB_5_PIN",			KEY_TYPE.MakeMesh_Tab_5_Pin,			SPACE.MakeMesh, EST_KEYCODE.Alpha7, SPECIAL_KEY.None,	true, "Mesh : Pin", "Switch to the [Pin] tab.", "Pin");


			//모디파이어 / 애니메이션 편집
			AddHotKey("TOGGLE_EDIT_MODE",		KEY_TYPE.ToggleEditingMode,		SPACE.Modifier_Anim_Editing,	EST_KEYCODE.A,	SPECIAL_KEY.None,	true, "Turn Editing Mode on/off", "Turn Editing Mode (or Binding Mode) on or off for editing modifiers or animations.", "Toggle Editing Mode");
			AddHotKey("TOGGLE_SELECTION_LOCK",	KEY_TYPE.ToggleSelectionLock,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.S,	SPECIAL_KEY.None,	true, "Turn Selection Lock on/off", "Turn Selection Lock on or off. When Selection Lock is on, you cannot select other objects in the workspace.", "Toggle Selection Lock");
			//[삭제]AddHotKey("TOGGLE_MODIFIER_LOCK",	KEY_TYPE.ToggleModifierLock,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.D,	SPECIAL_KEY.None,	true, "Turn Modifier Lock (Timeline Lock) on/off", "Turn Modifier Lock on or off. When the Modifier Lock is turned off, other Modifiers (or Timeline) are displayed with limited processing.", "Toggle Modifier Lock");


			AddHotKey("EXOBJ_UPDATE_BY_OTHERMOD",			KEY_TYPE.ExObj_UpdateByOtherMod,		SPACE.Modifier_Anim_Editing, EST_KEYCODE.D, SPECIAL_KEY.None, true, "Apply Multiple modifiers in edit mode", "Apply Multiple modifiers in edit mode if no conflict", "Multiple Modifiers");
			AddHotKey("EXOBJ_SHOW_AS_GRAY",					KEY_TYPE.ExObj_ShowAsGray,				SPACE.Modifier_Anim_Editing, EST_KEYCODE.G, SPECIAL_KEY.Alt, true, "Non-Edited : Show as Gray", "Show Non-edited objects as Gray", "Show as Gray");
			AddHotKey("EXOBJ_TOGGLE_SELECTION_SEMILOCK",	KEY_TYPE.ExObj_ToggleSelectionSemiLock,	SPACE.Modifier_Anim_Editing, EST_KEYCODE.D, SPECIAL_KEY.Alt, true, "Non-Edited : Selection Lock", "Selection lock for Non-edited objects", "Selection Lock for Non-Edited");

			AddHotKey("PREVIEW_MOD_BONE_RESULT",	KEY_TYPE.PreviewModBoneResult,	SPACE.Modifier_Anim_Editing, EST_KEYCODE.B, SPECIAL_KEY.Alt, true, "Preview Calculated Bones", "Preview bones with all modifiers and IK applied.", "Preview Calculated Bones");
			AddHotKey("PREVIEW_MOD_COLOR_RESULT",	KEY_TYPE.PreviewModColorResult, SPACE.Modifier_Anim_Editing, EST_KEYCODE.C, SPECIAL_KEY.Alt, true, "Preview Calculated Colors", "Preview colors with all modifiers applied.", "Preview Calculated Colors");
			AddHotKey("SHOW_MODIFIER_LISTUI",		KEY_TYPE.ShowModifierListUI,	SPACE.Modifier_Anim_Editing, EST_KEYCODE.M, SPECIAL_KEY.Alt, true, "Show Modifier List", "Displays the UI that allows you to see whether all modifiers are applied.", "show/Hide Modifier List");
			AddHotKey("TOGGLE_MORPH_TARGET",		KEY_TYPE.ToggleMorphTarget,		SPACE.Modifier_Anim_Editing, EST_KEYCODE.T, SPECIAL_KEY.Alt, true, "Switch Morph Target", "Switch the edit target of the Moph modifier to Vertex or Pin.", "Switch Morph Target");
			

			AddHotKey("MODTOOL_INC_BRUSHSIZE",	KEY_TYPE.IncreaseModToolBrushSize,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.RightBracket,	SPECIAL_KEY.None,	true, "Increase Brush Radius", "When editing vertices, increase the radius of the Soft Selection tool or Blur tool.", "Increase Brush Radius");
			AddHotKey("MODTOOL_DEC_BRUSHSIZE",	KEY_TYPE.DecreaseModToolBrushSize,	SPACE.Modifier_Anim_Editing,	EST_KEYCODE.LeftBracket,	SPECIAL_KEY.None,	true, "Decrease Brush Radius", "When editing vertices, decrease the radius of the Soft Selection tool or Blur tool.", "Decrease Brush Radius");
			AddHotKey_Reserved("APPLY_FFD",		KEY_TYPE.ApplyFFD,					SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Enter,		false, SPECIAL_KEY.None,	"Apply FFD", "Exit the FFD tool and apply the changes.", "Apply FFD");
			AddHotKey_Reserved("REVERT_FFD",	KEY_TYPE.RevertFFD,					SPACE.Modifier_Anim_Editing,	EST_KEYCODE.Esc,		false, SPECIAL_KEY.None,	"Revert FFD", "Exit the FFD tool and revert the changes.", "Revert FFD");

			AddHotKey_Reserved("MOD_SELECT_ALL_VERTS",	KEY_TYPE.SelectAllVertices_EditMod,		SPACE.Modifier_Anim_Editing,		EST_KEYCODE.A,		false,	SPECIAL_KEY.Ctrl, "Select All Vertices", "Select all vertices during editing modifiers or animations.", "Select All Vertices");

			//애니메이션
			AddHotKey("ANIM_PLAY_PAUSE",		KEY_TYPE.Anim_PlayPause,		SPACE.AnimPlayBack,	EST_KEYCODE.Space,			SPECIAL_KEY.None,	true, "Play / Pause", "Play or pause the animation.", "Play / Pause");
			AddHotKey("ANIM_MOVE_PREV_FRAME",	KEY_TYPE.Anim_MovePrevFrame,	SPACE.AnimPlayBack,	EST_KEYCODE.Comma_Less,		SPECIAL_KEY.None,	true, "Move to Previous Frame", "Move the time slider to the Previous frame.", "Previous Frame");
			AddHotKey("ANIM_MOVE_NEXT_FRAME",	KEY_TYPE.Anim_MoveNextFrame,	SPACE.AnimPlayBack,	EST_KEYCODE.Period_Greater,	SPECIAL_KEY.None,	true, "Move to Next Frame", "Move the time slider to the Next frame.", "Next Frame");
			AddHotKey("ANIM_MOVE_FIRST_FRAME",	KEY_TYPE.Anim_MoveFirstFrame,	SPACE.AnimPlayBack,	EST_KEYCODE.Comma_Less,		SPECIAL_KEY.Shift,	true, "Move to First Frame", "Move the time slider to the First frame.", "First Frame");
			AddHotKey("ANIM_MOVE_LAST_FRAME",	KEY_TYPE.Anim_MoveLastFrame,	SPACE.AnimPlayBack,	EST_KEYCODE.Period_Greater,	SPECIAL_KEY.Shift,	true, "Move to Last Frame", "Move the time slider to the Last frame.", "Last Frame");

			AddHotKey("ANIM_MOVE_PREV_KEYFRAME",	KEY_TYPE.Anim_MovePrevKeyframe,	SPACE.AnimPlayBack,	EST_KEYCODE.Comma_Less,		SPECIAL_KEY.Ctrl,	true, "Move to Previous Keyframe", "Move the time slider to the Nearest Previous Keyframe.", "Previous Keyframe");
			AddHotKey("ANIM_MOVE_NEXT_KEYFRAME",	KEY_TYPE.Anim_MoveNextKeyframe,	SPACE.AnimPlayBack,	EST_KEYCODE.Period_Greater,	SPECIAL_KEY.Ctrl,	true, "Move to Next Keyframe", "Move the time slider to the Nearest Next Keyframe.", "Next Keyframe");

			AddHotKey("ANIM_ADD_KEYS",				KEY_TYPE.Anim_AddKeyframes,		SPACE.AnimPlayBack,	EST_KEYCODE.F,		SPECIAL_KEY.None,	true, "Add New Keyframes", "In the current frame, create new keyframes for the selected timeline layers.", "Add Keyframes");
			AddHotKey_Reserved("ANIM_REMOVE_KEYS",	KEY_TYPE.Anim_RemoveKeyframes,	SPACE.AnimPlayBack,	EST_KEYCODE.Delete, false,	SPECIAL_KEY.None,	"Remove Keyframes", "Remove selected keyframes.", "Remove Keyframes");
			AddHotKey_Reserved("ANIM_COPY_KEYS",	KEY_TYPE.Anim_CopyKeyframes,	SPACE.AnimPlayBack,	EST_KEYCODE.C,		false,	SPECIAL_KEY.Ctrl,	"Copy Keyframes", "Copy selected keyframes.", "Copy Keyframes");
			AddHotKey_Reserved("ANIM_PASTE_KEYS",	KEY_TYPE.Anim_PasteKeyframes,	SPACE.AnimPlayBack,	EST_KEYCODE.V,		false,	SPECIAL_KEY.Ctrl,	"Paste Keyframes", "Paste copied keyframes based on the current frame.", "Paste Keyframes");

			AddHotKey("ANIM_TIMELINE_SCROLL_UP",	KEY_TYPE.Anim_TimelineScrollUp,		SPACE.AnimPlayBack,	EST_KEYCODE.PageUp,		SPECIAL_KEY.None, true,	"Scroll Timeline Up", "Scroll up the Timeline UI.", null);
			AddHotKey("ANIM_TIMELINE_SCROLL_DOWN",	KEY_TYPE.Anim_TimelineScrollDown,	SPACE.AnimPlayBack,	EST_KEYCODE.PageDown,	SPECIAL_KEY.None, true,	"Scroll Timeline Down", "Scroll down the Timeline UI.", null);

			AddHotKey("ANIM_CURVE_LINEAR",				KEY_TYPE.Anim_Curve_Linear,				SPACE.AnimPlayBack,	EST_KEYCODE.F7,		SPECIAL_KEY.None, true,	"Set Curve to Linear", "Set the animation curve to Linear.", "Linear");
			AddHotKey("ANIM_CURVE_CONSTANT",			KEY_TYPE.Anim_Curve_Constant,			SPACE.AnimPlayBack,	EST_KEYCODE.F8,		SPECIAL_KEY.None, true,	"Set Curve to Constant", "Set the animation curve to Constant.", "Constant");
			AddHotKey("ANIM_CURVE_SMOOTH_DEFAULT",		KEY_TYPE.Anim_Curve_Smooth_Default,		SPACE.AnimPlayBack,	EST_KEYCODE.F9,		SPECIAL_KEY.None, true,	"Set Curve to Smooth (Default)", "Set the animation curve to Smooth (Default).", "Smooth (Default)");
			AddHotKey("ANIM_CURVE_SMOOTH_ACCANDDEC",	KEY_TYPE.Anim_Curve_Smooth_AccAndDec,	SPACE.AnimPlayBack,	EST_KEYCODE.F10,	SPECIAL_KEY.None, true,	"Set Curve to Smooth (Mix)", "Set the animation curve to Smooth (Acceleration and Deceleration).", "Smooth (Default)");
			AddHotKey("ANIM_CURVE_SMOOTH_ACC",			KEY_TYPE.Anim_Curve_Smooth_Acc,			SPACE.AnimPlayBack,	EST_KEYCODE.F11,	SPECIAL_KEY.None, true,	"Set Curve to Smooth (Accel)", "Set the animation curve to Smooth (Acceleration).", "Smooth (Accel)");
			AddHotKey("ANIM_CURVE_SMOOTH_DEC",			KEY_TYPE.Anim_Curve_Smooth_Dec,			SPACE.AnimPlayBack,	EST_KEYCODE.F12,	SPECIAL_KEY.None, true,	"Set Curve to Smooth (Decel)", "Set the animation curve to Smooth (Deceleration).", "Smooth (Decel)");

			AddHotKey("ANIM_TOGGLE_AUTO_KEY", KEY_TYPE.Anim_ToggleAutoKey, SPACE.AnimPlayBack,	EST_KEYCODE.N,	SPECIAL_KEY.None, true,	"Toggle Auto-Key", "Turns Auto-Key on or off.", "Toggle Auto-Key");


			//리깅
			AddHotKey("RIG_INCREASE_WEIGHT_02",	KEY_TYPE.Rig_IncreaseWeight_02,		SPACE.Rigging,	EST_KEYCODE.X,		SPECIAL_KEY.None,	true, "Increase Weight (0.02)", "Increase the weight for the selected bone by 0.02.", "Increase Weight");
			AddHotKey("RIG_DECREASE_WEIGHT_02",	KEY_TYPE.Rig_DecreaseWeight_02,		SPACE.Rigging,	EST_KEYCODE.Z,		SPECIAL_KEY.None,	true, "Decrease Weight (0.02)", "Decrease the weight for the selected bone by 0.02.", "Decrease Weight");
			AddHotKey("RIG_INCREASE_WEIGHT_05",	KEY_TYPE.Rig_IncreaseWeight_05,		SPACE.Rigging,	EST_KEYCODE.X,		SPECIAL_KEY.Shift,	true, "Increase Weight (0.05)", "Increase the weight for the selected bone by 0.05.", "Increase Weight");
			AddHotKey("RIG_DECREASE_WEIGHT_05",	KEY_TYPE.Rig_DecreaseWeight_05,		SPACE.Rigging,	EST_KEYCODE.Z,		SPECIAL_KEY.Shift,	true, "Decrease Weight (0.05)", "Decrease the weight for the selected bone by 0.05.", "Decrease Weight");

			AddHotKey("RIG_BRUSHMODE_ADD",		KEY_TYPE.Rig_BrushMode_Add,			SPACE.Rigging,	EST_KEYCODE.J,		SPECIAL_KEY.None,	true, "Rigging Brush : [Add]", "Change the Rigging Brush mode to [Add].", "Brush Mode - Add");
			AddHotKey("RIG_BRUSHMODE_MULTIPLY",	KEY_TYPE.Rig_BrushMode_Multiply,	SPACE.Rigging,	EST_KEYCODE.K,		SPECIAL_KEY.None,	true, "Rigging Brush : [Multiply]", "Change the Rigging Brush mode to [Multiply].", "Brush Mode - Multiply");
			AddHotKey("RIG_BRUSHMODE_BLUR",		KEY_TYPE.Rig_BrushMode_Blur,		SPACE.Rigging,	EST_KEYCODE.L,		SPECIAL_KEY.None,	true, "Rigging Brush : [Blur]", "Change the Rigging Brush mode to [Blur].", "Brush Mode - Blur");

			AddHotKey("RIG_INC_BRUSH_SIZE",		KEY_TYPE.Rig_IncreaseBrushSize,		SPACE.Rigging,	EST_KEYCODE.RightBracket,	SPECIAL_KEY.None,	true, "Increase Rig-Brush Radius", "Increase the radius of the Rigging Brush.", "Increase Brush Radius");
			AddHotKey("RIG_DEC_BRUSH_SIZE",		KEY_TYPE.Rig_DecreaseBrushSize,		SPACE.Rigging,	EST_KEYCODE.LeftBracket,	SPECIAL_KEY.None,	true, "Decrease Rig-Brush Radius", "Decrease the radius of the Rigging Brush.", "Decrease Brush Radius");

			AddHotKey("RIG_INC_BRUSH_INTENSITY",	KEY_TYPE.Rig_IncreaseBrushIntensity,	SPACE.Rigging,	EST_KEYCODE.Period_Greater,	SPECIAL_KEY.None,	true, "Increase Rig-Brush Intensity",	"Increase the intensity of the Rigging Brush.", "Increase Brush Intensity");
			AddHotKey("RIG_DEC_BRUSH_INTENSITY",	KEY_TYPE.Rig_DecreaseBrushIntensity,	SPACE.Rigging,	EST_KEYCODE.Comma_Less,		SPECIAL_KEY.None,	true, "Decrease Rig-Brush Intensity",	"Decrease the intensity of the Rigging Brush.", "Decrease Brush Intensity");


			//TODO : 여기에 추가하자 (ID 고유하게 작성할 것)

			//------------------------------------------------
		}


		// Functions (Add Unit)
		//---------------------------------------------------
		private void AddHotKey(		string ID,
									KEY_TYPE hotKeyType, 
									SPACE eventSpace,
									EST_KEYCODE keyCode_Def,
									SPECIAL_KEY specialKeys,
									bool isAvailable,
									string label,
									string info,
									string label_WorkSpace)
		{
			HotkeyMapUnit newHotKey = new HotkeyMapUnit(	ID,
															hotKeyType, 
															eventSpace, 
															keyCode_Def, 
															false,
															IsCtrl_FromCombination(specialKeys),
															IsShift_FromCombination(specialKeys),
															IsAlt_FromCombination(specialKeys),
															false,
															isAvailable,
															label, info, label_WorkSpace);

			_units_All.Add(newHotKey);

			_hotKeyType2Unit.Add(hotKeyType, newHotKey);
			_ID2Unit.Add(ID, newHotKey);

			if(!_eventSpace2UnitList.ContainsKey(eventSpace))
			{
				_eventSpace2UnitList.Add(eventSpace, new List<HotkeyMapUnit>());
			}
			_eventSpace2UnitList[eventSpace].Add(newHotKey);
		}


		private void AddHotKey_IgnoreSpecialKey(	string ID,
													KEY_TYPE hotKeyType, 
													SPACE eventSpace,
													EST_KEYCODE keyCode_Def,
													bool isAvailable,
													string label,
													string info,
													string label_WorkSpace)
		{
			HotkeyMapUnit newHotKey = new HotkeyMapUnit(	ID,
															hotKeyType, 
															eventSpace, 
															keyCode_Def, 
															true,
															false,
															false,
															false,
															false,
															isAvailable,
															label, info, label_WorkSpace);

			_units_All.Add(newHotKey);

			_hotKeyType2Unit.Add(hotKeyType, newHotKey);
			_ID2Unit.Add(ID, newHotKey);

			if(!_eventSpace2UnitList.ContainsKey(eventSpace))
			{
				_eventSpace2UnitList.Add(eventSpace, new List<HotkeyMapUnit>());
			}
			_eventSpace2UnitList[eventSpace].Add(newHotKey);
		}


		private void AddHotKey_Reserved(	string ID,
											KEY_TYPE hotKeyType,
											SPACE eventSpace,
											EST_KEYCODE keyCode_Def,
											bool isIgnoreSpecialKey,
											SPECIAL_KEY specialKeys,
											string label,
											string info,
											string label_WorkSpace)
		{

			HotkeyMapUnit newHotKey = new HotkeyMapUnit(	ID,
															hotKeyType, 
															eventSpace, 
															keyCode_Def, 
															isIgnoreSpecialKey,
															IsCtrl_FromCombination(specialKeys),
															IsShift_FromCombination(specialKeys),
															IsAlt_FromCombination(specialKeys),
															true, true,
															label, info, label_WorkSpace);

			_units_All.Add(newHotKey);

			_hotKeyType2Unit.Add(hotKeyType, newHotKey);
			_ID2Unit.Add(ID, newHotKey);

			if(!_eventSpace2UnitList.ContainsKey(eventSpace))
			{
				_eventSpace2UnitList.Add(eventSpace, new List<HotkeyMapUnit>());
			}
			_eventSpace2UnitList[eventSpace].Add(newHotKey);
		}

		private bool IsCtrl_FromCombination(SPECIAL_KEY specialKeys)
		{
			return specialKeys == SPECIAL_KEY.Ctrl
				|| specialKeys == SPECIAL_KEY.Ctrl_Shift
				|| specialKeys == SPECIAL_KEY.Ctrl_Alt
				|| specialKeys == SPECIAL_KEY.Ctrl_Shift_Alt;
		}

		private bool IsShift_FromCombination(SPECIAL_KEY specialKeys)
		{
			return specialKeys == SPECIAL_KEY.Shift
				|| specialKeys == SPECIAL_KEY.Ctrl_Shift
				|| specialKeys == SPECIAL_KEY.Shift_Alt
				|| specialKeys == SPECIAL_KEY.Ctrl_Shift_Alt;
		}

		private bool IsAlt_FromCombination(SPECIAL_KEY specialKeys)
		{
			return specialKeys == SPECIAL_KEY.Alt
				|| specialKeys == SPECIAL_KEY.Ctrl_Alt
				|| specialKeys == SPECIAL_KEY.Shift_Alt
				|| specialKeys == SPECIAL_KEY.Ctrl_Shift_Alt;
		}
		

		// Functions (Save / Load / Restore)
		//---------------------------------------------------
		public void Save()
		{
			apStringWrapper strSave = new apStringWrapper(1000);
			//저장 양식
			//KeyType(int) : KeyCode(int) : Ctrl(T/F) + Shift(T/F) + Alt(T/F) + Available (T/F)
			//구분은 /

			
			string del1 = ":";
			string del2 = "/";
			string value_T = "T";
			string value_F = "F";
			
			HotkeyMapUnit curUnit = null;
			for (int i = 0; i < _units_All.Count; i++)
			{
				curUnit = _units_All[i];

				if(!curUnit.IsChanged())
				{
					//변경된게 없으면 패스
					//변경된 것만 저장하자
					continue;
				}

				//strSave.Append((int)curUnit._hotKeyType, false);
				strSave.Append(curUnit._ID, false);//ID로 변경
				strSave.Append(del1, false);
				strSave.Append((int)curUnit._keyCode_Cur, false);
				strSave.Append(del1, false);
				strSave.Append(curUnit._isCtrl_Cur ? value_T : value_F, false);
				strSave.Append(curUnit._isShift_Cur ? value_T : value_F, false);
				strSave.Append(curUnit._isAlt_Cur ? value_T : value_F, false);
				strSave.Append(curUnit._isAvailable_Cur ? value_T : value_F, false);
				strSave.Append(del2, false);
			}
			strSave.MakeString();

			EditorPrefs.SetString("AnyPortrait_HotKeys", strSave.ToString());
		}


		public void Load()
		{
			string loadedText = EditorPrefs.GetString("AnyPortrait_HotKeys", "");
			_isLoaded = true;


			//일단 전부 복구하자
			RestoreAll();

			//저장된 데이터가 없다.
			if(string.IsNullOrEmpty(loadedText))
			{	
				return;
			}

			//저장 양식
			//ID(Str) : KeyCode(int) : Ctrl(T/F) + Shift(T/F) + Alt(T/F) + Available (T/F)
			//구분은 /

			string del1 = ":";
			string[] arrDel1 = new string[] { del1 };
			string del2 = "/";
			char value_T = 'T';
			string[] strLoadTexts = loadedText.Split(new string[] { del2 }, StringSplitOptions.RemoveEmptyEntries);

			if(strLoadTexts == null || strLoadTexts.Length == 0)
			{
				//데이터가 뭔가 저장되었지만 유효하지 않다.
				//빈값으로 다시 저장
				EditorPrefs.SetString("AnyPortrait_HotKeys", "");
				return;
			}

			try
			{
				string curStr = null;
				HotkeyMapUnit targetUnit = null;
				for (int iStr = 0; iStr < strLoadTexts.Length; iStr++)
				{
					curStr = strLoadTexts[iStr];
					if (curStr.Length < 8 || !curStr.Contains(del1))
					{
						//파싱할 수가 없다면 패스
						continue;
					}

					string[] strProps = curStr.Split(arrDel1, StringSplitOptions.None);
					if(strProps == null || strProps.Length < 3)
					{
						//데이터가 이상하다
						continue;
					}

					//int iKeyType = int.Parse(strProps[0]);
					string ID = strProps[0];
					int iKeyCode = int.Parse(strProps[1]);
					string strSpecial = strProps[2];

					if(strSpecial.Length < 4)
					{
						//Special 데이터가 이상하다
						continue;
					}
					bool isCtrl = false;
					bool isShift = false;
					bool isAlt = false;
					bool isAvailable = false;

					//HOTKEY_TYPE hotKeyType = (HOTKEY_TYPE)iKeyType;
					EST_KEYCODE keyCode = (EST_KEYCODE)iKeyCode;
					isCtrl = strSpecial[0] == value_T;
					isShift = strSpecial[1] == value_T;
					isAlt = strSpecial[2] == value_T;
					isAvailable = strSpecial[3] == value_T;

					if(_ID2Unit.ContainsKey(ID))
					{
						targetUnit = _ID2Unit[ID];
					}

					if (targetUnit == null)
					{
						Debug.LogError("No ID in Dictionaray.. (" + ID + ")");

						//잉? ID가 없다고?
						targetUnit = _units_All.Find(delegate (HotkeyMapUnit a)
						{
							return string.Equals(a._ID, ID);
						});
					}
					//if(!_hotKeyType2Unit.ContainsKey(hotKeyType))
					//{
					//	Debug.LogError("AnyPortrait : There are uninitialized shortcut settings. [" + hotKeyType + "]");
					//	continue;
					//}

					//targetUnit = _hotKeyType2Unit[hotKeyType];

					if(targetUnit == null)
					{
						Debug.LogError("AnyPortrait : There are uninitialized shortcut settings. [" + ID + "]");
						continue;
					}

					

					targetUnit.SetCurrentValue(keyCode, isCtrl, isShift, isAlt, isAvailable);

				}
			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : Failed to load shortcut settings.\n" + ex.ToString());
				EditorPrefs.SetString("AnyPortrait_HotKeys", "");//실패했으니 초기화
				return;
			}
		}


		public void RestoreAll()
		{
			HotkeyMapUnit curUnit = null;
			for (int i = 0; i < _units_All.Count; i++)
			{
				curUnit = _units_All[i];
				curUnit.Restore();
			}
		}



		// Functions 
		//---------------------------------------------------
		public void CheckConflict()
		{
			HotkeyMapUnit curUnit = null;
			for (int i = 0; i < _units_All.Count; i++)
			{
				curUnit = _units_All[i];
				
				curUnit._conflictedUnit = null;
				if(curUnit._isReserved || !curUnit._isAvailable_Cur)
				{
					continue;
				}

				

				//단축키가 겹치는 경우를 찾자
				curUnit._conflictedUnit = _units_All.Find(delegate(HotkeyMapUnit a)
				{
					if(a == curUnit 
					|| !a._isAvailable_Cur
					|| curUnit._keyCode_Cur != a._keyCode_Cur)
					{
						return false;
					}

					//중복 대상을 찾자
					//Common의 경우, 다른 모든 단축키와 비교
					//그렇지 않을 경우, eventSpace가 같거나 Common인 경우만 체크
					if (curUnit._eventSpace != SPACE.Common)
					{
						if (a._eventSpace != SPACE.Common
							&& a._eventSpace != curUnit._eventSpace)
						{
							//비교 대상이 아니다.
							return false;
						}
					}

					//일단 키코드까진 같다.
					if (curUnit._isIgnoreSpecialKey)
					{
						//현재 Unit이 Special Key를 무시하는 경우
						//Special Key가 None인 경우만 체크
						if(a._isIgnoreSpecialKey
							|| (!a._isCtrl_Cur && !a._isShift_Cur && !a._isAlt_Cur)
							)
						{
							//동일한 단축키가 있다.
							return true;
						}
					}
					else
					{
						//Special Key가 필요하다면, Special Key가 동일해야한다.
						if(curUnit._isCtrl_Cur == a._isCtrl_Cur
						&& curUnit._isShift_Cur == a._isShift_Cur
						&& curUnit._isAlt_Cur == a._isAlt_Cur)
						{
							return true;
						}
					}

					return false;
				});
			}
		}


		/// <summary>
		/// UI에서 단축키 설명을 하고자 한다면 이 함수를 이용하여 단축키를 String에 추가할 수 있다.
		/// </summary>
		/// <param name="hotKeyType"></param>
		/// <param name="targetStrWrapper"></param>
		/// <param name="isIncludeBracket"></param>
		public void AddHotkeyTextToWrapper(KEY_TYPE hotKeyType, apStringWrapper targetStrWrapper, bool isIncludeBracket)
		{
			HotkeyMapUnit unit = GetHotkey(hotKeyType);
			if (!unit._isAvailable_Cur || unit._keyCode_Cur == EST_KEYCODE.Unknown)
			{
				return;
			}

			if (isIncludeBracket)
			{
				targetStrWrapper.Append(apStringFactory.I.Space1, false);
				targetStrWrapper.Append(apStringFactory.I.Bracket_1_L, false);
			}

			if (!unit._isIgnoreSpecialKey)
			{
				//Ctrl(Command) / Shift / Alt(Option) 표기
				if (unit._isCtrl_Cur || unit._isShift_Cur || unit._isAlt_Cur)
				{
					targetStrWrapper.Append(GetSpecialKeyText(unit.GetSpecialKeyComb()), false);
					targetStrWrapper.Append(apStringFactory.I.Plus, false);
				}
			}

			if (isIncludeBracket)
			{
				targetStrWrapper.Append(GetKeycodeText(unit._keyCode_Cur), false);
				targetStrWrapper.Append(apStringFactory.I.Bracket_1_R, true);
			}
			else
			{
				targetStrWrapper.Append(GetKeycodeText(unit._keyCode_Cur), true);
			}
		}







		// Get / Set
		//---------------------------------------------------
		public List<HotkeyMapUnit> Units
		{
			get
			{
				return _units_All;
			}
		}

		public Dictionary<SPACE, List<HotkeyMapUnit>> EventSpace2Units
		{
			get
			{
				return _eventSpace2UnitList;
			}
		}

		public HotkeyMapUnit GetHotkey(KEY_TYPE keyType)
		{
			return _hotKeyType2Unit[keyType];
		}

		public HotkeyMapUnit GetHotkeyByID(string ID)
		{
			return _units_All.Find(delegate(HotkeyMapUnit a)
			{
				return string.Equals(a._ID, ID);
			});
		}

		public bool IsHotkeyAvailable(KEY_TYPE keyType)
		{
			HotkeyMapUnit unit = _hotKeyType2Unit[keyType];
			return unit._isAvailable_Cur && unit._keyCode_Cur != EST_KEYCODE.Unknown;
		}



		public string GetKeycodeText(EST_KEYCODE keycode)
		{
			return _keycodeTexts[(int)keycode];
		}

		public string GetSpecialKeyText(SPECIAL_KEY specialKey)
		{
			return _specialTexts[(int)specialKey];
		}

		public string[] KeycodeTexts { get { return _keycodeTexts; } }
		public string[] SpecialKeyTexts { get { return _specialTexts; } }


		// 변환 함수
		//---------------------------------------------------------
		public static EST_KEYCODE KeyCode2Essential(KeyCode keyCode)
		{
			switch (keyCode)
			{
				case KeyCode.A: return EST_KEYCODE.A;
				case KeyCode.B: return EST_KEYCODE.B;
				case KeyCode.C: return EST_KEYCODE.C;
				case KeyCode.D: return EST_KEYCODE.D;
				case KeyCode.E: return EST_KEYCODE.E;
				case KeyCode.F: return EST_KEYCODE.F;
				case KeyCode.G: return EST_KEYCODE.G;
				case KeyCode.H: return EST_KEYCODE.H;
				case KeyCode.I: return EST_KEYCODE.I;
				case KeyCode.J: return EST_KEYCODE.J;
				case KeyCode.K: return EST_KEYCODE.K;
				case KeyCode.L: return EST_KEYCODE.L;
				case KeyCode.M: return EST_KEYCODE.M;
				case KeyCode.N: return EST_KEYCODE.N;
				case KeyCode.O: return EST_KEYCODE.O;
				case KeyCode.P: return EST_KEYCODE.P;
				case KeyCode.Q: return EST_KEYCODE.Q;
				case KeyCode.R: return EST_KEYCODE.R;
				case KeyCode.S: return EST_KEYCODE.S;
				case KeyCode.T: return EST_KEYCODE.T;
				case KeyCode.U: return EST_KEYCODE.U;
				case KeyCode.V: return EST_KEYCODE.V;
				case KeyCode.W: return EST_KEYCODE.W;
				case KeyCode.X: return EST_KEYCODE.X;
				case KeyCode.Y: return EST_KEYCODE.Y;
				case KeyCode.Z: return EST_KEYCODE.Z;

				//위 숫자
				case KeyCode.Alpha0: return EST_KEYCODE.Alpha0;
				case KeyCode.Alpha1: return EST_KEYCODE.Alpha1;
				case KeyCode.Alpha2: return EST_KEYCODE.Alpha2;
				case KeyCode.Alpha3: return EST_KEYCODE.Alpha3;
				case KeyCode.Alpha4: return EST_KEYCODE.Alpha4;
				case KeyCode.Alpha5: return EST_KEYCODE.Alpha5;
				case KeyCode.Alpha6: return EST_KEYCODE.Alpha6;
				case KeyCode.Alpha7: return EST_KEYCODE.Alpha7;
				case KeyCode.Alpha8: return EST_KEYCODE.Alpha8;
				case KeyCode.Alpha9: return EST_KEYCODE.Alpha9;

				//특수 문자들
				case KeyCode.Minus: return EST_KEYCODE.Minus;
				case KeyCode.Equals: return EST_KEYCODE.Plus_Equal;
				case KeyCode.LeftBracket: return EST_KEYCODE.LeftBracket;
				case KeyCode.RightBracket: return EST_KEYCODE.RightBracket;
				case KeyCode.Backslash: return EST_KEYCODE.Backslash;
				case KeyCode.Semicolon: return EST_KEYCODE.Semicolon;
				case KeyCode.Quote: return EST_KEYCODE.Quote;
				case KeyCode.BackQuote: return EST_KEYCODE.BackQuote;
				case KeyCode.Comma: return EST_KEYCODE.Comma_Less;
				case KeyCode.Period: return EST_KEYCODE.Period_Greater;
				case KeyCode.Slash: return EST_KEYCODE.Slash_Question;
				case KeyCode.Keypad0: return EST_KEYCODE.Pad0;
				case KeyCode.Keypad1: return EST_KEYCODE.Pad1;
				case KeyCode.Keypad2: return EST_KEYCODE.Pad2;
				case KeyCode.Keypad3: return EST_KEYCODE.Pad3;
				case KeyCode.Keypad4: return EST_KEYCODE.Pad4;
				case KeyCode.Keypad5: return EST_KEYCODE.Pad5;
				case KeyCode.Keypad6: return EST_KEYCODE.Pad6;
				case KeyCode.Keypad7: return EST_KEYCODE.Pad7;
				case KeyCode.Keypad8: return EST_KEYCODE.Pad8;
				case KeyCode.Keypad9: return EST_KEYCODE.Pad9;

					//특수키
				case KeyCode.F1: return EST_KEYCODE.F1;
				case KeyCode.F2: return EST_KEYCODE.F2;
				case KeyCode.F3: return EST_KEYCODE.F3;
				case KeyCode.F4: return EST_KEYCODE.F4;
				case KeyCode.F5: return EST_KEYCODE.F5;
				case KeyCode.F6: return EST_KEYCODE.F6;
				case KeyCode.F7: return EST_KEYCODE.F7;
				case KeyCode.F8: return EST_KEYCODE.F8;
				case KeyCode.F9: return EST_KEYCODE.F9;
				case KeyCode.F10: return EST_KEYCODE.F10;
				case KeyCode.F11: return EST_KEYCODE.F11;
				case KeyCode.F12: return EST_KEYCODE.F12;
				case KeyCode.Home: return EST_KEYCODE.Home;
				case KeyCode.End: return EST_KEYCODE.End;
				case KeyCode.PageUp: return EST_KEYCODE.PageUp;
				case KeyCode.PageDown: return EST_KEYCODE.PageDown;
				case KeyCode.Space: return EST_KEYCODE.Space;

					//방향키
				case KeyCode.UpArrow: return EST_KEYCODE.UpArrow;
				case KeyCode.DownArrow: return EST_KEYCODE.DownArrow;
				case KeyCode.LeftArrow: return EST_KEYCODE.LeftArrow;
				case KeyCode.RightArrow: return EST_KEYCODE.RightArrow;

				//기본키
				case KeyCode.Return: return  EST_KEYCODE.Enter;
				case KeyCode.Escape: return EST_KEYCODE.Esc;
				case KeyCode.Delete: return EST_KEYCODE.Delete;

				default:
					return EST_KEYCODE.Unknown;
			}
			
		}
		public static KeyCode Essential2KeyCode(EST_KEYCODE essentialKey)
		{
			switch (essentialKey)
			{
				case EST_KEYCODE.A: return KeyCode.A;
				case EST_KEYCODE.B: return KeyCode.B;
				case EST_KEYCODE.C: return KeyCode.C;
				case EST_KEYCODE.D: return KeyCode.D;
				case EST_KEYCODE.E: return KeyCode.E;
				case EST_KEYCODE.F: return KeyCode.F;
				case EST_KEYCODE.G: return KeyCode.G;
				case EST_KEYCODE.H: return KeyCode.H;
				case EST_KEYCODE.I: return KeyCode.I;
				case EST_KEYCODE.J: return KeyCode.J;
				case EST_KEYCODE.K: return KeyCode.K;
				case EST_KEYCODE.L: return KeyCode.L;
				case EST_KEYCODE.M: return KeyCode.M;
				case EST_KEYCODE.N: return KeyCode.N;
				case EST_KEYCODE.O: return KeyCode.O;
				case EST_KEYCODE.P: return KeyCode.P;
				case EST_KEYCODE.Q: return KeyCode.Q;
				case EST_KEYCODE.R: return KeyCode.R;
				case EST_KEYCODE.S: return KeyCode.S;
				case EST_KEYCODE.T: return KeyCode.T;
				case EST_KEYCODE.U: return KeyCode.U;
				case EST_KEYCODE.V: return KeyCode.V;
				case EST_KEYCODE.W: return KeyCode.W;
				case EST_KEYCODE.X: return KeyCode.X;
				case EST_KEYCODE.Y: return KeyCode.Y;
				case EST_KEYCODE.Z: return KeyCode.Z;

				//위 숫자
				case EST_KEYCODE.Alpha0: return KeyCode.Alpha0;
				case EST_KEYCODE.Alpha1: return KeyCode.Alpha1;
				case EST_KEYCODE.Alpha2: return KeyCode.Alpha2;
				case EST_KEYCODE.Alpha3: return KeyCode.Alpha3;
				case EST_KEYCODE.Alpha4: return KeyCode.Alpha4;
				case EST_KEYCODE.Alpha5: return KeyCode.Alpha5;
				case EST_KEYCODE.Alpha6: return KeyCode.Alpha6;
				case EST_KEYCODE.Alpha7: return KeyCode.Alpha7;
				case EST_KEYCODE.Alpha8: return KeyCode.Alpha8;
				case EST_KEYCODE.Alpha9: return KeyCode.Alpha9;

				//특수 문자들
				case EST_KEYCODE.Minus: return KeyCode.Minus;
				case EST_KEYCODE.Plus_Equal: return KeyCode.Equals;
				case EST_KEYCODE.LeftBracket: return KeyCode.LeftBracket;
				case EST_KEYCODE.RightBracket: return KeyCode.RightBracket;
				case EST_KEYCODE.Backslash: return KeyCode.Backslash;
				case EST_KEYCODE.Semicolon: return KeyCode.Semicolon;
				case EST_KEYCODE.Quote: return KeyCode.Quote;
				case EST_KEYCODE.BackQuote: return KeyCode.BackQuote;
				case EST_KEYCODE.Comma_Less: return KeyCode.Comma;
				case EST_KEYCODE.Period_Greater: return KeyCode.Period;
				case EST_KEYCODE.Slash_Question: return KeyCode.Slash;
				case EST_KEYCODE.Pad0: return KeyCode.Keypad0;
				case EST_KEYCODE.Pad1: return KeyCode.Keypad1;
				case EST_KEYCODE.Pad2: return KeyCode.Keypad2;
				case EST_KEYCODE.Pad3: return KeyCode.Keypad3;
				case EST_KEYCODE.Pad4: return KeyCode.Keypad4;
				case EST_KEYCODE.Pad5: return KeyCode.Keypad5;
				case EST_KEYCODE.Pad6: return KeyCode.Keypad6;
				case EST_KEYCODE.Pad7: return KeyCode.Keypad7;
				case EST_KEYCODE.Pad8: return KeyCode.Keypad8;
				case EST_KEYCODE.Pad9: return KeyCode.Keypad9;

				//특수키
				case EST_KEYCODE.F1: return KeyCode.F1;
				case EST_KEYCODE.F2: return KeyCode.F2;
				case EST_KEYCODE.F3: return KeyCode.F3;
				case EST_KEYCODE.F4: return KeyCode.F4;
				case EST_KEYCODE.F5: return KeyCode.F5;
				case EST_KEYCODE.F6: return KeyCode.F6;
				case EST_KEYCODE.F7: return KeyCode.F7;
				case EST_KEYCODE.F8: return KeyCode.F8;
				case EST_KEYCODE.F9: return KeyCode.F9;
				case EST_KEYCODE.F10: return KeyCode.F10;
				case EST_KEYCODE.F11: return KeyCode.F11;
				case EST_KEYCODE.F12: return KeyCode.F12;
				case EST_KEYCODE.Home: return KeyCode.Home;
				case EST_KEYCODE.End: return KeyCode.End;
				case EST_KEYCODE.PageUp: return KeyCode.PageUp;
				case EST_KEYCODE.PageDown: return KeyCode.PageDown;
				case EST_KEYCODE.Space: return KeyCode.Space;

				//방향키
				case EST_KEYCODE.UpArrow: return KeyCode.UpArrow;
				case EST_KEYCODE.DownArrow: return KeyCode.DownArrow;
				case EST_KEYCODE.LeftArrow: return KeyCode.LeftArrow;
				case EST_KEYCODE.RightArrow: return KeyCode.RightArrow;

				//기본키
				case EST_KEYCODE.Enter: return KeyCode.Return;
				case EST_KEYCODE.Esc: return KeyCode.Escape;
				case EST_KEYCODE.Delete: return KeyCode.Delete;

				default:
					return KeyCode.None;
			}
		}
	}
}