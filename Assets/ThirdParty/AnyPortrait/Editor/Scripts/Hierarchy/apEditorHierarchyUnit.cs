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

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apEditorHierarchyUnit
	{
		// Member
		//--------------------------------------------------------------------------
		public delegate void FUNC_UNIT_CLICK(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isCtrl, bool isShift);
		public delegate void FUNC_UNIT_CLICK_VISIBLE(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isVisible, bool isPostfixIcon);
		public delegate void FUNC_UNIT_CLICK_ORDER_CHANGED(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isOrderUp);
		public delegate void FUNC_UNIT_CLICK_RESTORE_TMPWORK();

		//추가 22.12.12 : 선택된 Hierarchy인지 조회
		public delegate bool FUNC_CHECK_SELECTED_HIERARCHY(apEditorHierarchyUnit unit);


		//추가 21.6.12 : 우클릭은 따로 체크한다.
		public delegate void FUNC_UNIT_RIGHTCLICK(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj);


		public enum UNIT_TYPE
		{
			Label,
			ToggleButton,
			ToggleButton_Visible,
			OnlyButton,
		}
		public UNIT_TYPE _unitType = UNIT_TYPE.Label;
		public Texture2D _icon = null;
		public apStringWrapper _text = new apStringWrapper(128);

		public int _level = 0;
		public int _savedKey = -1;
		public object _savedObj = null;

		public enum VISIBLE_TYPE
		{
			None,//<<안보입니더
			NoKey,//MoKey는 없지만 출력은 됩니다.
			NoKeyDisabled,//NoKey와 다르지만 옵션 등에 의해 클릭한다고 해서 바로 Key가 생성되지 않음
			Current_Visible,
			Current_NonVisible,
			TmpWork_Visible,
			TmpWork_NonVisible,
			ModKey_Visible,
			ModKey_NonVisible,
			Default_Visible,
			Default_NonVisible,
			Rule_Visible,//추가 21.1.27
			Rule_NonVisible

		}
		public VISIBLE_TYPE _visibleType_Prefix = VISIBLE_TYPE.None;//Visible 속성이 붙은 경우는 이것도 세팅해야한다.
		public VISIBLE_TYPE _visibleType_Postfix = VISIBLE_TYPE.None;//Visible 속성이 붙은 경우는 이것도 세팅해야한다.

		//추가 19.11.24 : VisiblePrefix를 실시간으로 갱신해야할 필요가 있는데, 매번 RefreshUnit을 호출할 순 없다.
		//함수 대리자를 이용해서 실시간 갱신을 하도록 하자
		public delegate void FUNC_REFRESH_VISIBLE_PREFIX(apEditorHierarchyUnit unit);
		private FUNC_REFRESH_VISIBLE_PREFIX _funcRefreshVisiblePreFix = null;

		//추가 19.6.29 : VisiblePrefix를 리셋하는 버튼을 Label에 추가할 수 있다.
		public bool _isRestoreTmpWorkVisibleBtn = false;
		private bool _isTmpWorkAnyChanged = false;

		public apEditorHierarchyUnit _parentUnit = null;
		public List<apEditorHierarchyUnit> _childUnits = new List<apEditorHierarchyUnit>();

		private bool _isFoldOut = true;
		
		private bool _isSelected = false;
		private bool _isSubSelected = false;//추가 20.5.24 : 여러개를 선택하면 메인 + 서브들로 구분된다. 선택 색상이 바뀜
		public bool IsSelected { get { return _isSelected; } }


		//추가 20.5.28 : 다중 선택 가능 여부 > 이게 true이면 선택된 상태에서도 Label이 아닌 Button이 보여진다.
		private bool _isMultiSelectable = false;

		private bool _isModRegistered = false;//추가) 현재 선택한 Mod 등에서 등록된
		private bool _isAvailable = true;//선택 가능한가. 기본적으로는 True. 예외적으로 False를 설정해야한다.

		public void SetFoldOut(bool isFoldOut) { _isFoldOut = isFoldOut; }

		//변경 20.5.24 : 선택이 [메인 / 서브]로 세분화된다.
		public void SetSelected(bool isSelected, bool isMain)
		{
			if(!isSelected)
			{
				_isSelected = false;
				_isSubSelected = false;
			}
			else
			{
				if(isMain)
				{
					//실제
					_isSelected = true;
					_isSubSelected = false;

					//테스트
					//_isSelected = false;
					//_isSubSelected = true;
				}
				else
				{
					_isSelected = false;
					_isSubSelected = true;
				}
			}
		}
		
		public void SetModRegistered(bool isModRegistered)
		{
			_isModRegistered = isModRegistered;
			RefreshPrevRender();
		}
		public void SetAvailable(bool isAvailable) {  _isAvailable = isAvailable; }


		public bool IsFoldOut { get { return _isFoldOut; } }
		public bool IsSelected_Main { get { return _isSelected; } }
		public bool IsSelected_Sub { get { return _isSubSelected; } }
		
		public bool IsAvailable {  get {  return _isAvailable; } }

		private FUNC_UNIT_CLICK _funcClick = null;
		private FUNC_UNIT_CLICK_VISIBLE _funcClickVisible = null;
		private FUNC_UNIT_CLICK_ORDER_CHANGED _funcClickOrderChanged = null;
		private FUNC_UNIT_RIGHTCLICK _funcRightClick = null;
		private FUNC_CHECK_SELECTED_HIERARCHY _funcCheckSelectedHierarchy = null;

		//이전
		//private GUIContent _guiContent_Text = new GUIContent();
		//private GUIContent _guiContent_Icon = new GUIContent();

		//변경 19.11.16
		private apGUIContentWrapper _guiContent_Text = new apGUIContentWrapper();
		private apGUIContentWrapper _guiContent_Text_Short = new apGUIContentWrapper();
		private apGUIContentWrapper _guiContent_Icon = new apGUIContentWrapper();

		//Level에 따라서 Short Text의 길이가 다르다.
		private const int SHORT_TEX_LENGTH_LV01 = 18;
		private const int SHORT_TEX_LENGTH_LV23 = 16;
		private const int SHORT_TEX_LENGTH_LV4_MORE = 14;
		private const string TEX_DOT2 = "..";

		//private GUIContent _guiContent_Folded = new GUIContent();
		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_Selected = null;
		private GUIStyle _guiStyle_NoAvailable = null;
		private GUIStyle _guiStyle_ModIcon = null;
		//private Color _guiColor_TextColor_None;
		//private Color _guiColor_TextColor_Selected;
		//private bool _isGUIStyleCreated = false;

		//이전
		//private GUIContent _guiContent_FoldDown = new GUIContent();
		//private GUIContent _guiContent_FoldRight = new GUIContent();

		//변경 19.11.16
		private apGUIContentWrapper _guiContent_FoldDown = new apGUIContentWrapper();
		private apGUIContentWrapper _guiContent_FoldRight = new apGUIContentWrapper();

		private enum VISIBLE_ICON
		{
			Current,
			TmpWork,
			Default,
			ModKey,
			Rule//추가 21.1.27
		}
		
		//변경 19.11.16
		private apGUIContentWrapper _guiContent_NoKey = null;
		private apGUIContentWrapper _guiContent_NoKeyDisabled = null;
		private apGUIContentWrapper[] _guiContent_Visible = new apGUIContentWrapper[5];
		private apGUIContentWrapper[] _guiContent_Nonvisible = new apGUIContentWrapper[5];

		private apGUIContentWrapper _guiContent_ModRegisted = new apGUIContentWrapper();

		private apGUIContentWrapper _guiContent_OrderUp = new apGUIContentWrapper();
		private apGUIContentWrapper _guiContent_OrderDown = new apGUIContentWrapper();


		private bool _isOrderChangable = false;

		public int _indexPerParent = -1;
		private int _indexCountForChild = 0;

		


		//추가 19.6.29 : RestoreTmpWorkVisible 버튼
		//private GUIContent _guiContent_RestoreTmpWorkVisible_ON = null;
		//private GUIContent _guiContent_RestoreTmpWorkVisible_OFF = null;

		//변경 19.11.16
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_ON = null;
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_OFF = null;


		//추가된 내용
		//일부 버튼들은 나오거나 안나올 수 있다.
		//GUIEvent (Layout)을 기준으로 모두 갱신되는데,
		//그 외의 이벤트에서는 "이전 프레임의 기록"을 따라야 한다.
		//이전 프레임에서 렌더링 되었을때 -> 현재 안될때
		// >> 더미 렌더링을 한다. 클릭 이벤트는 발생하지 않는다.
		//이전 프레임에서 렌더링 안되고 -> 현재 될때
		// >> 렌더링하지 않는다.

		//리셋할때 체크한다.
		//GUIEvent에서는 이 변수를 무시한다. (렌더 여부를 갱신한다.)
		private bool _isPrev_ModRegBox = false;
		private bool _isPrev_VisiblePrefix = false;
		private bool _isPrev_VisiblePostfix = false;
		private bool _isPrev_FoldBtn = false;

		private bool _isNext_ModRegBox;
		private bool _isNext_VisiblePrefix;
		private bool _isNext_FoldBtn;
		private bool _isNext_VisiblePostfix;

		private bool _isCur_ModRegBox;
		private bool _isCur_VisiblePrefix;
		private bool _isCur_FoldBtn;
		private bool _isCur_VisiblePostfix;

		//추가 19.6.29 : RestoreTmpWorkVisible 버튼
		private bool _isPrev_RestoreTmpWorkVisible = false;
		private bool _isNext_RestoreTmpWorkVisible = false;
		private bool _isCur_RestoreTmpWorkVisible = false;

		private FUNC_UNIT_CLICK_RESTORE_TMPWORK _funcClickRestoreTmpWorkVisible = null;

		private const string NO_NAME = " <No Name>  ";
		private const string NO_TEXT = "";

		public const int HEIGHT = 20;
		private const int WIDTH_FOLD_BTN = HEIGHT - 4;
		private const int WIDTH_ICON = HEIGHT - 6;

		private GUILayoutOption _layoutOption_H_Height = null;
		private GUILayoutOption _layoutOption_W_8 = null;
		private GUILayoutOption _layoutOption_W_20 = null;
		private GUILayoutOption _layoutOption_W_12 = null;
		private GUILayoutOption _layoutOption_W_FOLD_BTN = null;
		private GUILayoutOption _layoutOption_W_ICON = null;

		private bool _isRenderable = false;
		private int _leftSpaceWidth = 0;
		private int _cursorX = 0;
		private int _lastRenderedPosY = -1;



		//추가 20.7.4 : 선형 인덱스. 이게 정해지면 Shift키로 여러개를 선택할 수 있다.
		//재귀적으로 정해지는 _indexPerParent와는 다르다.
		private int _linearIndex = 0;
		public int LinearIndex { get { return _linearIndex; } }




		// Init
		//--------------------------------------------------------------------------
		public static apEditorHierarchyUnit MakeUnit()
		{
			return new apEditorHierarchyUnit();
		}


		private apEditorHierarchyUnit()
		{
			_isSelected = false;
			_isSubSelected = false;
			//_isGUIStyleCreated = false;
			
			_indexPerParent = -1;

			_isAvailable = true;

			if(_text == null)
			{
				_text = new apStringWrapper(128);
			}

			_linearIndex = -1;

			_lastRenderedPosY = -1;

			InitPrevRender();
		}

		private void InitPrevRender()
		{
			_isPrev_ModRegBox = false;
			_isPrev_VisiblePrefix = false;
			_isPrev_VisiblePostfix = false;
			_isPrev_FoldBtn = false;

			_isPrev_RestoreTmpWorkVisible = false;

			_funcRefreshVisiblePreFix = null;
			_isRenderable = true;

			//변경 19.12.24 : GUI_Render가 아닌 여기서 호출
			CheckAndCreateGUIStyle();
		}


		//추가 20.3.18 : 기존의 생성, 삭제 방식에서 pool 방식으로 바꾸려면 데이터를 초기화하는 과정이 필요하다.
		public void Clear()
		{
			_unitType = UNIT_TYPE.Label;
			_icon = null;
			if(_text == null)
			{
				_text = new apStringWrapper(128);
			}

			_level = 0;
			_savedKey = -1;
			_savedObj = null;

			_visibleType_Prefix = VISIBLE_TYPE.None;
			_visibleType_Postfix = VISIBLE_TYPE.None;

			_funcRefreshVisiblePreFix = null;

			_isRestoreTmpWorkVisibleBtn = false;
			_isTmpWorkAnyChanged = false;

			_parentUnit = null;
			if(_childUnits == null)
			{
				_childUnits = new List<apEditorHierarchyUnit>();
			}
			_childUnits.Clear();

			_isFoldOut = true;
			_isSelected = false;
			_isSubSelected = false;
			_isModRegistered = false;
			_isAvailable = true;

			_isMultiSelectable = false;

			_funcClick = null;
			_funcClickVisible = null;
			_funcClickOrderChanged = null;
			_funcRightClick = null;//추가 21.6.12
			_funcCheckSelectedHierarchy = null;//추가 22.12.12

			if(_guiContent_Text == null)
			{
				_guiContent_Text = new apGUIContentWrapper();
			}
			if(_guiContent_Text_Short == null)
			{
				_guiContent_Text_Short = new apGUIContentWrapper();
			}
			if (_guiContent_Icon == null)
			{
				_guiContent_Icon = new apGUIContentWrapper();
			}
			_guiContent_Text.ClearAll();
			_guiContent_Text_Short.ClearAll();
			_guiContent_Icon.ClearAll();

			_isOrderChangable = false;

			_indexPerParent = -1;
			_indexCountForChild = 0;
		
			_isPrev_ModRegBox = false;
			_isPrev_VisiblePrefix = false;
			_isPrev_VisiblePostfix = false;
			_isPrev_FoldBtn = false;

			_isNext_ModRegBox = false;
			_isNext_VisiblePrefix = false;
			_isNext_FoldBtn = false;
			_isNext_VisiblePostfix = false;

			_isCur_ModRegBox = false;
			_isCur_VisiblePrefix = false;
			_isCur_FoldBtn = false;
			_isCur_VisiblePostfix = false;

			_isPrev_RestoreTmpWorkVisible = false;
			_isNext_RestoreTmpWorkVisible = false;
			_isCur_RestoreTmpWorkVisible = false;

			_funcClickRestoreTmpWorkVisible = null;

			_isRenderable = false;
			_leftSpaceWidth = 0;
			_cursorX = 0;

			_linearIndex = -1;
		}

		//필수! 꼭 초기화 함수를 실행하자
		public void Init(int level)
		{
			_level = level;
		}
		
		public void SetLinearIndex(int linearIndex)
		{
			_linearIndex = linearIndex;
		}

		//추가
		/// <summary>
		/// GUIStyle이 있는지 체크하고 없으면 생성합니다.
		/// </summary>
		private void CheckAndCreateGUIStyle()
		{	
			_guiStyle_None = apGUIStyleWrapper.I.None_MiddleLeft_Margin0_Black2LabelColor;
			_guiStyle_Selected = apGUIStyleWrapper.I.None_MiddleLeft_Margin0_White2Cyan;
			_guiStyle_ModIcon = apGUIStyleWrapper.I.None_MiddleCenter_Margin0;

			
			_guiStyle_NoAvailable = apGUIStyleWrapper.I.None_MiddleLeft_Margin0_GrayColor;

			//색상을 바꾸는 것은 안되고, 아예 Available이 false인 경우에 대한 guiStyle을 만들어야 함
			//_guiColor_TextColor_None = _guiStyle_None.normal.textColor;
			//_guiColor_TextColor_Selected = _guiStyle_Selected.normal.textColor;

			//추가 : 19.12.24 : GUILayout.Width / Height를 미리 만들자. 이것도 성능 차이 많음
			_layoutOption_H_Height = apGUILOFactory.I.Height(HEIGHT);
			_layoutOption_W_8 = apGUILOFactory.I.Width(8);
			_layoutOption_W_20 = apGUILOFactory.I.Width(20);
			_layoutOption_W_12 = apGUILOFactory.I.Width(12);
			_layoutOption_W_FOLD_BTN = apGUILOFactory.I.Width(WIDTH_FOLD_BTN);
			_layoutOption_W_ICON = apGUILOFactory.I.Width(WIDTH_ICON);

			
		}


		public void ReloadResources()
		{
			//bool isPrevNull = _guiStyle_None == null;
			
			CheckAndCreateGUIStyle();

			//if(isPrevNull && _guiStyle_None != null)
			//{
			//	Debug.Log("GUI 리소스가 나중에 다시 로드됨");
			//}
		}

		// Common
		//--------------------------------------------------------------------------

		public void SetBasicIconImg(	apGUIContentWrapper guiContent_imgFoldDown, 
										apGUIContentWrapper guiContent_imgFoldRight, 
										apGUIContentWrapper guiContent_imgModRegisted)//변경 19.11.16 : 공유할 수 있게
		{
			_guiContent_FoldDown = guiContent_imgFoldDown;
			_guiContent_FoldRight = guiContent_imgFoldRight;
			_guiContent_ModRegisted = guiContent_imgModRegisted;
		}

		public void SetBasicIconImg(	apGUIContentWrapper guiContent_imgFoldDown, 
										apGUIContentWrapper guiContent_imgFoldRight, 
										apGUIContentWrapper guiContent_imgModRegisted, 
										apGUIContentWrapper guiContent_imgOrderUp, 
										apGUIContentWrapper guiContent_imgOrderDown)//변경 19.11.16 : 공유할 수 있게
		{
			_guiContent_FoldDown = guiContent_imgFoldDown;
			_guiContent_FoldRight = guiContent_imgFoldRight;
			_guiContent_ModRegisted = guiContent_imgModRegisted;

			_guiContent_OrderUp = guiContent_imgOrderUp;
			_guiContent_OrderDown = guiContent_imgOrderDown;
		}


		//Visible 속성이 붙은 경우는 이걸 호출해서 세팅해줘야 한다.
		public void SetVisibleIconImage(	apGUIContentWrapper guiVisible_Current, apGUIContentWrapper guiNonVisible_Current,
											apGUIContentWrapper guiVisible_TmpWork, apGUIContentWrapper guiNonVisible_TmpWork,
											apGUIContentWrapper guiVisible_Default, apGUIContentWrapper guiNonVisible_Default,
											apGUIContentWrapper guiVisible_ModKey, apGUIContentWrapper guiNonVisible_ModKey,
											apGUIContentWrapper guiVisible_Rule, apGUIContentWrapper guiNonVisible_Rule,
											apGUIContentWrapper gui_NoKey, apGUIContentWrapper gui_NoKeyDisabled,
											FUNC_REFRESH_VISIBLE_PREFIX funcVisiblePrePostFix
											)
		{
			if (_guiContent_Visible == null)
			{
				_guiContent_Visible = new apGUIContentWrapper[5];
			}
			if (_guiContent_Nonvisible == null)
			{
				_guiContent_Nonvisible = new apGUIContentWrapper[5];
			}

			_guiContent_Visible[(int)VISIBLE_ICON.Current] = guiVisible_Current;
			_guiContent_Visible[(int)VISIBLE_ICON.TmpWork] = guiVisible_TmpWork;
			_guiContent_Visible[(int)VISIBLE_ICON.Default] = guiVisible_Default;
			_guiContent_Visible[(int)VISIBLE_ICON.ModKey] = guiVisible_ModKey;
			_guiContent_Visible[(int)VISIBLE_ICON.Rule] = guiVisible_Rule;

			_guiContent_Nonvisible[(int)VISIBLE_ICON.Current] = guiNonVisible_Current;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork] = guiNonVisible_TmpWork;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.Default] = guiNonVisible_Default;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey] = guiNonVisible_ModKey;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.Rule] = guiNonVisible_Rule;

			_guiContent_NoKey = gui_NoKey;
			_guiContent_NoKeyDisabled = gui_NoKeyDisabled;

			_funcRefreshVisiblePreFix = funcVisiblePrePostFix;//추가 19.11.24
		}

		public void SetEvent(FUNC_UNIT_CLICK funcUnitClick,
							FUNC_CHECK_SELECTED_HIERARCHY funcCheckSelectedHierarchy)
		{
			_funcClick = funcUnitClick;
			_funcClickVisible = null;
			_funcClickOrderChanged = null;
			_isOrderChangable = false;
			_funcCheckSelectedHierarchy = funcCheckSelectedHierarchy;//추가 22.12.12
		}

		//Visible 속성이 붙은 경우는 위 함수(SetEvent)대신 이걸 호출해야한다.
		public void SetEvent(	FUNC_UNIT_CLICK funcUnitClick,
								FUNC_UNIT_CLICK_VISIBLE funcClickVisible,
								FUNC_CHECK_SELECTED_HIERARCHY funcCheckSelectedHierarchy,
								FUNC_UNIT_CLICK_ORDER_CHANGED funcClickOrderChanged = null)
		{
			_funcClick = funcUnitClick;
			_funcClickVisible = funcClickVisible;
			_funcClickOrderChanged = funcClickOrderChanged;
			_isOrderChangable = _funcClickOrderChanged != null;
			_funcCheckSelectedHierarchy = funcCheckSelectedHierarchy;//추가 22.12.12
		}

		//추가 21.6.12 : 우클릭을 입력하자
		public void SetRightClickEvent(FUNC_UNIT_RIGHTCLICK funcRightClick)
		{
			_funcRightClick = funcRightClick;
		}


		public void SetParent(apEditorHierarchyUnit parentUnit)
		{
			_parentUnit = parentUnit;
		}

		public void AddChild(apEditorHierarchyUnit childUnit)
		{
			childUnit._indexPerParent = _indexCountForChild;
			_indexCountForChild++;

			_childUnits.Add(childUnit);

			RefreshPrevRender();
		}

		//추가 Label에서 TmpWorkVisible을 초기화하는 버튼을 추가할 수 있다.
		//public void SetRestoreTmpWorkVisible(Texture2D btnIcon_ON, Texture2D btnIcon_OFF, FUNC_UNIT_CLICK_RESTORE_TMPWORK funcRestoreClick)//이전
		public void SetRestoreTmpWorkVisible(	apGUIContentWrapper guiContent_btnIcon_ON, 
												apGUIContentWrapper guiContent_btnIcon_OFF, 
												FUNC_UNIT_CLICK_RESTORE_TMPWORK funcRestoreClick)//변경 19.11.16 : 공유할 수 있게
		{
			_isRestoreTmpWorkVisibleBtn = true;
			_isTmpWorkAnyChanged = false;
			//이전
			//_guiContent_RestoreTmpWorkVisible_ON = new GUIContent(btnIcon_ON);
			//_guiContent_RestoreTmpWorkVisible_OFF = new GUIContent(btnIcon_OFF);

			//변경
			_guiContent_RestoreTmpWorkVisible_ON = guiContent_btnIcon_ON;
			_guiContent_RestoreTmpWorkVisible_OFF = guiContent_btnIcon_OFF;

			_funcClickRestoreTmpWorkVisible = funcRestoreClick;
		}

		public void SetRestoreTmpWorkVisibleAnyChanged(bool isAnyChanged)
		{
			_isTmpWorkAnyChanged = isAnyChanged;
		}


		// Set
		//--------------------------------------------------------------------------
		public void ChangeText(string text)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = NO_TEXT;
			}
			_text.SetText(text);
			MakeGUIContent();
		}
		public void ChangeIcon(Texture2D icon)
		{
			_icon = icon;
			MakeGUIContent();
		}

		public void SetLabel(Texture2D icon, string text, int savedKey, object savedObj)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = NO_TEXT;
			}

			_unitType = UNIT_TYPE.Label;
			_icon = icon;
			_text.SetText(text);
			_savedKey = savedKey;
			_savedObj = savedObj;

			_isRestoreTmpWorkVisibleBtn = false;

			MakeGUIContent();
		}

		public void SetToggleButton(Texture2D icon, string text, int savedKey, object savedObj, bool isMultiSelectable)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = NO_TEXT;
			}

			_unitType = UNIT_TYPE.ToggleButton;
			_icon = icon;
			_text.SetText(text);
			_savedKey = savedKey;
			_savedObj = savedObj;
			_isMultiSelectable = isMultiSelectable;

			MakeGUIContent();
		}

		public void SetToggleButton_Visible(Texture2D icon, string text, int savedKey, object savedObj, 
											bool isMultiSelectable,
											VISIBLE_TYPE visibleType_Prefix, VISIBLE_TYPE visibleType_Postfix)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = NO_TEXT;
			}

			_unitType = UNIT_TYPE.ToggleButton_Visible;
			_icon = icon;
			_text.SetText(text);
			_savedKey = savedKey;
			_savedObj = savedObj;

			_isMultiSelectable = isMultiSelectable;

			_visibleType_Prefix = visibleType_Prefix;
			_visibleType_Postfix = visibleType_Postfix;

			MakeGUIContent();
		}

		public void SetOnlyButton(Texture2D icon, string text, int savedKey, object savedObj)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = NO_TEXT;
			}

			_unitType = UNIT_TYPE.OnlyButton;
			_icon = icon;
			_text.SetText(text);
			_savedKey = savedKey;
			_savedObj = savedObj;

			MakeGUIContent();
		}

		private void MakeGUIContent()
		{
			if (_icon != null)
			{
				if(_guiContent_Icon == null)
				{
					_guiContent_Icon = apGUIContentWrapper.Make(_icon);
				}
				else
				{
					_guiContent_Icon.SetImage(_icon);
				}
				//_guiContent_Icon = new GUIContent(_icon);//이전
			}
			else
			{
				//_guiContent_Icon = null;//null로 만들진 않는다.
				_guiContent_Icon.SetVisible(false);
			}

			//이전
			//if (!string.IsNullOrEmpty(_text))
			//{
			//	_guiContent_Text = new GUIContent(" " + _text + "  ");
			//}
			//else
			//{
			//	_guiContent_Text = new GUIContent(" <No Name>  ");
			//}

			//변경
			if(_guiContent_Text == null)
			{
				_guiContent_Text = new apGUIContentWrapper();
			}

			if(_guiContent_Text_Short == null)
			{
				_guiContent_Text_Short = new apGUIContentWrapper();
			}

			if (!string.IsNullOrEmpty(_text.ToString()))
			{
				//공백(1) + 텍스트 + 공백(2)
				_guiContent_Text.ClearText(false);
				_guiContent_Text.AppendSpaceText(1, false);
				_guiContent_Text.AppendText(_text.ToString(), false);
				_guiContent_Text.AppendSpaceText(2, true);

				//이름의 길이를 18자 이내로 줄인 버전도 준비한다.
				_guiContent_Text_Short.ClearText(false);
				_guiContent_Text_Short.AppendSpaceText(1, false);
				
				int shortTextLength = 0;
				if(_level <= 1)			{ shortTextLength = SHORT_TEX_LENGTH_LV01; }
				else if(_level <= 3)	{ shortTextLength = SHORT_TEX_LENGTH_LV23; }
				else					{ shortTextLength = SHORT_TEX_LENGTH_LV4_MORE; }
				
				if(_text.ToString().Length <= shortTextLength)
				{
					_guiContent_Text_Short.AppendText(_text.ToString(), false);
				}
				else
				{
					_guiContent_Text_Short.AppendText(_text.ToString().Substring(0, shortTextLength-2), false);
					_guiContent_Text_Short.AppendText(TEX_DOT2, false);
				}
				_guiContent_Text_Short.AppendSpaceText(2, true);
			}
			else
			{
				_guiContent_Text.SetText(NO_NAME);
				_guiContent_Text_Short.SetText(NO_NAME);
			}

			RefreshPrevRender();
		}



		public void RefreshPrevRender()
		{
			//Debug.LogWarning("Refresh Prev [" + Event.current.type + "]");

			//이전 방식
			//_isPrevRender_ModRegBox = _isModRegistered;
			//_isPrevRender_VisiblePrefix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None);
			//_isPrevRender_VisiblePostfix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Postfix != VISIBLE_TYPE.None);
			//_isPrevRender_Fold = (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label));

			////추가 19.6.29 : TmpWork
			//_isPrevRender_RestoreTmpWorkVisible = _unitType == UNIT_TYPE.Label && _isRestoreTmpWorkVisibleBtn ;

			_isPrev_ModRegBox = _isCur_ModRegBox;
			_isPrev_VisiblePrefix = _isCur_VisiblePrefix;
			_isPrev_VisiblePostfix = _isCur_VisiblePostfix;
			_isPrev_FoldBtn = _isCur_FoldBtn;
			_isPrev_RestoreTmpWorkVisible = _isCur_RestoreTmpWorkVisible;
		}

		// GUI
		//--------------------------------------------------------------------------
		public void GUI_Render(	int posY,
								int width,
								Vector2 scroll,
								int scrollLayoutHeight,
								bool isGUIEvent,
								int level,
								bool isOrderButtonVisible = false)
		{
			//Level에 따른 여백
			//_leftSpaceWidth = level * 10;
			
			if(level < 2)
			{
				_leftSpaceWidth = 0;
			}
			else
			{
				_leftSpaceWidth = (level - 1) * 10;
			}
			_cursorX = 0;

			//마지막 렌더 위치를 저장하자
			_lastRenderedPosY = posY;

			//추가 19.11.22
			//만약 렌더링하지 않아도 된다면 렌더링하지 않고 여백만 주고 넘어가야한다.
			//다만, 이 과정에서 Layout - Repaint 비동기 문제로 GUI 에러가 발생한다.
			if (Event.current.type != EventType.Layout)
			{
				//Repaint일때는 지속
			}
			else
			{
				//Repaint가 아닐때는 스크롤 여부를 확인하여 변경하자
				_isRenderable = apEditorUtil.IsItemInScroll(posY, HEIGHT, scroll, scrollLayoutHeight);
			}


			//if(!apEditorUtil.IsItemInScroll(posY, HEIGHT, scroll, scrollLayoutHeight))//이전
			if (!_isRenderable)//변경
			{
				//기존
				GUILayout.Space(HEIGHT);
				return;
			}

			Rect lastRect = GUILayoutUtility.GetLastRect();

			//배경 렌더링
			if (_isSelected || _isSubSelected)
			{
				apEditorUtil.UNIT_BG_STYLE bgStyle = apEditorUtil.UNIT_BG_STYLE.Main;

				//Color prevColor = GUI.backgroundColor;

				//현재 Hierarchy가 선택되어 있다면 반짝거리게 하자
				if (_isSelected)
				{
					if(_funcCheckSelectedHierarchy != null
						&& _funcCheckSelectedHierarchy(this))
					{
						//현재 선택된 Hierarchy라면
						//GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightHierarchyUnitColor();

						//v1.4.2 : 반짝반짝한 색상
						bgStyle = apEditorUtil.UNIT_BG_STYLE.MainAnimated;
					}
					else
					{
						//현재 선택된 Hierarchy가 아니라면
						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}

						//v1.4.2 일반 색상
						bgStyle = apEditorUtil.UNIT_BG_STYLE.Main;
					}
					
				}
				else
				{
					////서브 선택인 경우, 녹색 계열로 보여주자
					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(1.0f, 0.4f, 0.4f, 1.0f);
					//}

					//v1.4.2 서브 선택
					bgStyle = apEditorUtil.UNIT_BG_STYLE.Sub;
				}
				

				//GUI.Box(new Rect(lastRect.x + scroll.x + 1, lastRect.y + (HEIGHT - 1), width + 10, HEIGHT + 1), NO_TEXT, apEditorUtil.WhiteGUIStyle_Box);
				//GUI.backgroundColor = prevColor;

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + scroll.x + 1, lastRect.y + (HEIGHT - 1), width + 10, HEIGHT + 1, bgStyle);
			}

			//EditorGUILayout.BeginHorizontal(GUILayout.Height(HEIGHT));
			EditorGUILayout.BeginHorizontal(_layoutOption_H_Height);//<<


			GUILayout.Space(2);

			//추가 : 19.11.24 : Pre/Post Fix 아이콘을 여기서 실시간으로 갱신할 수 있다.
			if(isGUIEvent)
			{
				if(_funcRefreshVisiblePreFix != null)
				{
					//체크 : 만약 값이 바뀌었다면 DebugLog (에러를 잡았당!)
					//VISIBLE_TYPE debug_Prefix = _visibleType_Prefix;
					_funcRefreshVisiblePreFix(this);
					//if(debug_Prefix != _visibleType_Prefix)
					//{
					//	Debug.LogError("Prefix 미갱신 오류가 보정되었다! : " + _guiContent_Text.Content.text);
					//}
				}
			}

			_isNext_ModRegBox = _isModRegistered;
			_isNext_VisiblePrefix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None);
			_isNext_VisiblePostfix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Postfix != VISIBLE_TYPE.None);
			_isNext_FoldBtn = (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label));
			_isNext_RestoreTmpWorkVisible = (_unitType == UNIT_TYPE.Label && _isRestoreTmpWorkVisibleBtn);

			if (isGUIEvent)
			{
				//GUIEvent에서는 Prev를 무시한다.
				_isCur_ModRegBox = _isNext_ModRegBox;
				_isCur_VisiblePrefix = _isNext_VisiblePrefix;
				_isCur_VisiblePostfix = _isNext_VisiblePostfix;
				_isCur_FoldBtn = _isNext_FoldBtn;
				_isCur_RestoreTmpWorkVisible = _isNext_RestoreTmpWorkVisible;

				RefreshPrevRender();
			}
			else
			{
				//GUIEvent가 아닐 때에는 True > Prev, False > Cur (+Prev 갱신)의 값을 따른다.
				if (_isNext_ModRegBox)
				{
					_isCur_ModRegBox = _isPrev_ModRegBox;
				}
				else
				{
					_isCur_ModRegBox = _isNext_ModRegBox;
					_isPrev_ModRegBox = _isNext_ModRegBox;
				}

				if (_isNext_VisiblePrefix)
				{
					_isCur_VisiblePrefix = _isPrev_VisiblePrefix;
				}
				else
				{
					_isCur_VisiblePrefix = _isNext_VisiblePrefix;
					_isPrev_VisiblePrefix = _isNext_VisiblePrefix;
				}

				if (_isNext_VisiblePostfix)
				{
					_isCur_VisiblePostfix = _isPrev_VisiblePostfix;
				}
				else
				{
					_isCur_VisiblePostfix = _isNext_VisiblePostfix;
					_isPrev_VisiblePostfix = _isNext_VisiblePostfix;
				}

				if (_isNext_FoldBtn)
				{
					_isCur_FoldBtn = _isPrev_FoldBtn;
				}
				else
				{
					_isCur_FoldBtn = _isNext_FoldBtn;
					_isPrev_FoldBtn = _isNext_FoldBtn;
				}

				if (_isNext_RestoreTmpWorkVisible)
				{
					_isCur_RestoreTmpWorkVisible = _isPrev_RestoreTmpWorkVisible;
				}
				else
				{
					_isCur_RestoreTmpWorkVisible = _isNext_RestoreTmpWorkVisible;
					_isPrev_RestoreTmpWorkVisible = _isNext_RestoreTmpWorkVisible;
				}
				
			}
			
			//이제 렌더링을 하자

			//1) Modifier 등록 박스 아이콘 (또는 여백8. 왼쪽 여백과 별도로 추가된다.)
			if(_isCur_ModRegBox)
			{
				//이전
				//GUILayout.Box(_guiContent_ModRegisted, _guiStyle_ModIcon, GUILayout.Width(8), GUILayout.Height(height));

				//변경
				GUILayout.Box(_guiContent_ModRegisted.Content, _guiStyle_ModIcon, _layoutOption_W_8, _layoutOption_H_Height);
			}
			else
			{
				GUILayout.Space(8);
			}
			_cursorX += 8;



			// 2) 앞쪽의 "보기" 버튼 (왼쪽 여백을 소모시킴)
			if(_isCur_VisiblePrefix)
			{
				//앞쪽에도 Visible Button을 띄워야겠다면
				apGUIContentWrapper visibleGUIContent = null;

				if (!_isNext_VisiblePrefix)
				{
					//만약 더미를 렌더링 하는 경우
					visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current];
				}
				else
				{
					//정식 렌더링인 경우
					switch (_visibleType_Prefix)
					{
						case VISIBLE_TYPE.Current_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Current]; break;
						case VISIBLE_TYPE.Current_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current]; break;
						case VISIBLE_TYPE.TmpWork_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.TmpWork]; break;
						case VISIBLE_TYPE.TmpWork_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork]; break;
						case VISIBLE_TYPE.Default_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Default]; break;
						case VISIBLE_TYPE.Default_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Default]; break;
						case VISIBLE_TYPE.ModKey_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.ModKey]; break;
						case VISIBLE_TYPE.ModKey_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey]; break;
						case VISIBLE_TYPE.Rule_Visible:			visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Rule]; break;
						case VISIBLE_TYPE.Rule_NonVisible:		visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Rule]; break;
						case VISIBLE_TYPE.NoKey:				visibleGUIContent = _guiContent_NoKey; break;
						case VISIBLE_TYPE.NoKeyDisabled:		visibleGUIContent = _guiContent_NoKeyDisabled; break;
						

					}
				}

				if (GUILayout.Button(visibleGUIContent.Content, _guiStyle_None, _layoutOption_W_20, _layoutOption_H_Height))
				{
					if (_isNext_VisiblePrefix)
					{
						if (_funcClickVisible != null)
						{
							_funcClickVisible(this, _savedKey, _savedObj,
								_visibleType_Prefix == VISIBLE_TYPE.Current_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.Default_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.TmpWork_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.ModKey_Visible, true);
						}
					}
				}
				//_leftSpaceWidth -= 22;
				//if (_leftSpaceWidth < 0)
				//{
				//	_leftSpaceWidth = 0;
				//	//leftWidth = level * 5;
				//}

				_cursorX += 20;
			}

			// 3) 레이어 순서 변경 버튼 (메인 하이라키 전용) 또는 여백
			if(isOrderButtonVisible && _isOrderChangable)
			{
				if(GUILayout.Button(_guiContent_OrderUp.Content, _guiStyle_None, _layoutOption_W_12, _layoutOption_H_Height))
				{
					if(_funcClickOrderChanged != null)
					{
						_funcClickOrderChanged(this, _savedKey, _savedObj, true);
					}
				}
				if(GUILayout.Button(_guiContent_OrderDown.Content, _guiStyle_None, _layoutOption_W_12, _layoutOption_H_Height))
				{
					if(_funcClickOrderChanged != null)
					{
						_funcClickOrderChanged(this, _savedKey, _savedObj, false);
					}
				}

				_leftSpaceWidth -= 30;
				if (_leftSpaceWidth < 0)
				{
					_leftSpaceWidth = 0;
					//leftWidth = level * 5;
				}
				//GUILayout.Space(leftSpaceWidth);//<<이전 (20.3.17)

				_cursorX += 24;
			}
			//else
			//{
			//	//기본 여백
			//	//GUILayout.Space(Mathf.Max(leftSpaceWidth, level * 10));//<<이전 (20.3.17)
			//}

			//4) TmpVisible 일괄 복구 버튼 (맨 위)
			if(_isCur_RestoreTmpWorkVisible)
			{
				if (GUILayout.Button((_isTmpWorkAnyChanged ? _guiContent_RestoreTmpWorkVisible_ON.Content : _guiContent_RestoreTmpWorkVisible_OFF.Content), _guiStyle_None, _layoutOption_W_FOLD_BTN, _layoutOption_H_Height))
				{
					if (_funcClickRestoreTmpWorkVisible != null)
					{
						_funcClickRestoreTmpWorkVisible();
					}
				}
				GUILayout.Space(2);

				_cursorX += WIDTH_FOLD_BTN + 2;
			}
			

			// 5) 여백 < 필수
			//이전 방법
			//GUILayout.Space(leftSpaceWidth);//방법 1
			//GUILayout.Space(Mathf.Max(leftSpaceWidth, level * 10));//방법 2
			
			//변경 : 20.3.17
			//_isNextRender_VisiblePostFix이 true인 경우 (Mod에 의한 Visible 아이콘이 보여지는 경우)는 가로 스크롤이 되지 않도록 고정 길이로 제한된다.
			//level에 따른 여백의 길이도 줄인다. (변경전 10 > 변경후 6)
			//여백의 최대값을 정한다.
			if(_leftSpaceWidth > 0)
			{
				//여백을 그려야 할 때
				if(_isCur_VisiblePostfix)
				{
					if(_leftSpaceWidth > 24)//level이 4일때 24이므로 그게 최고 레벨
					{
						_leftSpaceWidth = 24;
					}
				}
				GUILayout.Space(_leftSpaceWidth);

				_cursorX += _leftSpaceWidth;
			}
			
			

			// 6) Fold 버튼 (또는 _layoutOption_W_FOLD_BTN 만큼의 고정 길이)
			//맨 앞에 ▼/▶ 아이콘을 보이고, 작동시킬지를 결정
			if(_isCur_FoldBtn)
			{
				//Fold 아이콘을 출력하고 Button 기능을 추가한다.
				GUIContent btnContent = _guiContent_FoldDown.Content;
				if (!_isFoldOut)
				{
					btnContent = _guiContent_FoldRight.Content;
				}

				if(_guiStyle_None == null)
				{
					Debug.LogError("GUIStyle이 Null이다.");
				}
				if (GUILayout.Button(btnContent, _guiStyle_None, _layoutOption_W_FOLD_BTN, _layoutOption_H_Height))
				{
					if (_isNext_FoldBtn)
					{
						//정식 렌더링인 경우에 바꿔주자
						_isFoldOut = !_isFoldOut;
					}
				}
				

				_cursorX += WIDTH_FOLD_BTN;
			}
			else if(isOrderButtonVisible && _isOrderChangable)
			{
				GUILayout.Space(2);

				_cursorX += 2;
			}
			else
			{
				GUILayout.Space(WIDTH_FOLD_BTN);

				_cursorX += WIDTH_FOLD_BTN;
			}


			// 기본 아이콘
			if (_guiContent_Icon != null && _guiContent_Icon.IsVisible)
			{
				if (GUILayout.Button(_guiContent_Icon.Content, _guiStyle_None, _layoutOption_W_ICON, _layoutOption_H_Height))
				{
					if (_unitType == UNIT_TYPE.Label)
					{
						//if (isFoldVisible)
						if(_isCur_FoldBtn && _isNext_FoldBtn)
						{
							_isFoldOut = !_isFoldOut;
						}
					}
					else
					{
						apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.

						if(Event.current.button == 1 && _funcRightClick != null)
						{
							//우클릭이라면 (추가 21.6.12)
							_funcRightClick(this, _savedKey, _savedObj);
						}
						else if (_funcClick != null)
						{
							//좌클릭이라면
							_funcClick(this, _savedKey, _savedObj,
#if UNITY_EDITOR_OSX
								Event.current.command
#else
								Event.current.control
#endif
								,
								Event.current.shift
								);
						}
					}
				}

				_cursorX += WIDTH_ICON;
			}

			
			//유닛의 타입에 따라 다르게 출력한다.
			switch (_unitType)
			{
				//Label : 별도의 버튼 기능 없이 아이콘+텍스트만 보인다.
				//길이는 고정. 앞의 여백으로부터 Width를 뺀 길이를 이용한다.
				//대신 텍스트의 Short 버전은 이용하지 않는다. 그냥 스크롤 없이 UI 밖으로 글자가 나간다.
				case UNIT_TYPE.Label:
					if(_isCur_FoldBtn)
					{
						//만약, Fold가 가능한 경우 버튼으로 바뀌는데, Fold Toggle에 사용된다.
						
						if (GUILayout.Button(_guiContent_Text.Content, _isAvailable ? _guiStyle_None : _guiStyle_NoAvailable, apGUILOFactory.I.Width(width - (_cursorX + 10)), _layoutOption_H_Height))
						{
							if (_isCur_FoldBtn && _isNext_FoldBtn)
							{
								_isFoldOut = !_isFoldOut;
								apEditorUtil.ReleaseGUIFocus();
							}
						}
					}
					else
					{
						EditorGUILayout.LabelField(_guiContent_Text.Content, apGUILOFactory.I.Width(width - (_cursorX + 10)), _layoutOption_H_Height);
					}
					break;

				//OnlyButton : Toggle 기능 없이 항상 버튼의 역할을 한다.
				case UNIT_TYPE.OnlyButton:
					if (GUILayout.Button(_guiContent_Text.Content, _isAvailable ? _guiStyle_None : _guiStyle_NoAvailable, _layoutOption_H_Height))
					{
						apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.

						if(Event.current.button == 1 && _funcRightClick != null)
						{
							//우클릭이라면 (추가 21.6.12)
							_funcRightClick(this, _savedKey, _savedObj);
						}
						if (_funcClick != null)
						{	
							_funcClick(this, _savedKey, _savedObj,
#if UNITY_EDITOR_OSX
								Event.current.command
#else
								Event.current.control
#endif
								, Event.current.shift
								);
						}
					}
					break;

				//ToggleButton : Off된 상태에서는 On하기 위한 버튼이며, On이 된 경우는 단순히 아이콘+텍스트만 출력한다.
				//변경 20.5.28 : 다중 선택이 가능한 경우엔 항상 버튼을 출력하고 누를 수 있다.
				case UNIT_TYPE.ToggleButton:
					if ((!_isSelected && !_isSubSelected) || _isMultiSelectable)
					{
						if (GUILayout.Button(_guiContent_Text.Content, 
							_isAvailable ? ((_isSelected || _isSubSelected) ? _guiStyle_Selected : _guiStyle_None) : _guiStyle_NoAvailable, 
							_layoutOption_H_Height))
						{
							apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.

							if(Event.current.button == 1 && _funcRightClick != null)
							{
								//우클릭이라면 (추가 21.6.12)
								_funcRightClick(this, _savedKey, _savedObj);
							}
							else if (_funcClick != null)
							{	
								_funcClick(this, _savedKey, _savedObj,
#if UNITY_EDITOR_OSX
											Event.current.command
#else
											Event.current.control
#endif
											, Event.current.shift
								);
							}
						}
					}
					else
					{
						if(_funcRightClick != null)
						{
							//변경 21.6.13 : 우클릭이 있다면 선택된 상태에서도 Button으로 출력해야한다.
							if (GUILayout.Button(	_guiContent_Text.Content,
													_isAvailable ? _guiStyle_Selected : _guiStyle_NoAvailable,
													_layoutOption_H_Height))
							{
								apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.

								if (Event.current.button == 1 && _funcRightClick != null)
								{
									//우클릭이라면 (추가 21.6.12)
									_funcRightClick(this, _savedKey, _savedObj);
								}
							}
						}
						else
						{
							GUILayout.Label(_guiContent_Text.Content, _isAvailable ? _guiStyle_Selected : _guiStyle_NoAvailable, _layoutOption_H_Height);
						}
						
					}

					break;

				//ToggleButton + PostFix Visible
				//조건에 따라 PostFix 아이콘이 추가되며, 텍스트 길이가 고정된다. Short 버전의 텍스트를 이용한다.
				case UNIT_TYPE.ToggleButton_Visible:
					if ((!_isSelected && !_isSubSelected) || _isMultiSelectable)
					{
						bool isBtnClick = false;
						if(_isCur_VisiblePostfix)
						{
							isBtnClick = GUILayout.Button(_guiContent_Text_Short.Content, 
								_isAvailable ? ((_isSelected || _isSubSelected) ? _guiStyle_Selected : _guiStyle_None) : _guiStyle_NoAvailable, 
								apGUILOFactory.I.Width(width - (_cursorX + 30)), _layoutOption_H_Height);
						}
						else
						{
							isBtnClick = GUILayout.Button(_guiContent_Text.Content, 
								_isAvailable ? ((_isSelected || _isSubSelected) ? _guiStyle_Selected : _guiStyle_None) : _guiStyle_NoAvailable, 
								_layoutOption_H_Height);
						}
						if (isBtnClick)
						{
							apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.

							if(Event.current.button == 1 && _funcRightClick != null)
							{
								//우클릭이라면 (추가 21.6.12)
								_funcRightClick(this, _savedKey, _savedObj);
							}
							else if (_funcClick != null)
							{	
								_funcClick(this, _savedKey, _savedObj,
#if UNITY_EDITOR_OSX
									Event.current.command
#else
									Event.current.control
#endif
									, Event.current.shift
									);
							}
						}
					}
					else
					{
						if (_funcRightClick != null)
						{
							bool isBtnClick = false;
							//변경 21.6.13 : 우클릭이 있다면 선택된 상태에서도 Button으로 출력해야한다.
							if (_isCur_VisiblePostfix)
							{
								isBtnClick = GUILayout.Button(_guiContent_Text_Short.Content, _isAvailable ? _guiStyle_Selected : _guiStyle_NoAvailable, apGUILOFactory.I.Width(width - (_cursorX + 30)), _layoutOption_H_Height);
							}
							else
							{
								isBtnClick = GUILayout.Button(_guiContent_Text.Content, _isAvailable ? _guiStyle_Selected : _guiStyle_NoAvailable, _layoutOption_H_Height);
							}
							if (isBtnClick)
							{
								apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.

								if (Event.current.button == 1 && _funcRightClick != null)
								{
									//우클릭이라면 (추가 21.6.12)
									_funcRightClick(this, _savedKey, _savedObj);
								}
							}
						}
						else
						{
							if (_isCur_VisiblePostfix)
							{
								GUILayout.Label(_guiContent_Text_Short.Content, _isAvailable ? _guiStyle_Selected : _guiStyle_NoAvailable, apGUILOFactory.I.Width(width - (_cursorX + 30)), _layoutOption_H_Height);
							}
							else
							{
								GUILayout.Label(_guiContent_Text.Content, _isAvailable ? _guiStyle_Selected : _guiStyle_NoAvailable, _layoutOption_H_Height);
							}
						}
						
					}


					//if (_visibleType_Postfix != VISIBLE_TYPE.None)
					if(_isCur_VisiblePostfix)
					{
						apGUIContentWrapper visibleGUIContent = null;

						if (!_isNext_VisiblePostfix)
						{
							//더미 렌더링이라면
							visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current];
						}
						else
						{
							switch (_visibleType_Postfix)
							{
								case VISIBLE_TYPE.Current_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Current]; break;
								case VISIBLE_TYPE.Current_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current]; break;
								case VISIBLE_TYPE.TmpWork_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.TmpWork]; break;
								case VISIBLE_TYPE.TmpWork_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork]; break;
								case VISIBLE_TYPE.Default_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Default]; break;
								case VISIBLE_TYPE.Default_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Default]; break;
								case VISIBLE_TYPE.ModKey_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.ModKey]; break;
								case VISIBLE_TYPE.ModKey_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey]; break;
								case VISIBLE_TYPE.Rule_Visible:			visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Rule]; break;
								case VISIBLE_TYPE.Rule_NonVisible:		visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Rule]; break;
								case VISIBLE_TYPE.NoKey:				visibleGUIContent = _guiContent_NoKey; break;
								case VISIBLE_TYPE.NoKeyDisabled:		visibleGUIContent = _guiContent_NoKeyDisabled; break;

							}
						}

						if (GUILayout.Button(visibleGUIContent.Content, _guiStyle_None, _layoutOption_W_20, _layoutOption_H_Height))
						{
							if (_isNext_VisiblePostfix)
							{
								if (_funcClickVisible != null)
								{

									_funcClickVisible(this, _savedKey, _savedObj,
										_visibleType_Postfix == VISIBLE_TYPE.Current_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.Default_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.TmpWork_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.ModKey_Visible, false);
								}
							}
						}
					}
					break;
			}

			EditorGUILayout.EndHorizontal();

			//if(!_isAvailable)
			//{
			//	//GUI.contentColor = _guiColor_ContentColor;
			//	_guiStyle_None.normal.textColor = _guiColor_TextColor_None;
			//	_guiStyle_Selected.normal.textColor = _guiColor_TextColor_Selected;
			//}

			//if (isGUIEvent)
			//{
			//	//이전 프레임과 렌더링 동기화
			//	//설정에 의해 Prev를 갱신합니다.
			//	RefreshPrevRender();
			//}
		}


		// Functions
		//--------------------------------------------------------------------------
		/// <summary>
		/// 부모 UI로 가면서 Fold를 해제한다. Fold가 하나라도 되어 있었다면 true를 리턴한다.
		/// </summary>
		public bool UnfoldAllParent()
		{
			bool isAnyUnfolded = false;
			apEditorHierarchyUnit curUnit = this;//시작은 자기 자신(this)
			
			while(true)
			{
				if(curUnit == null)
				{
					break;
				}

				if (curUnit != this)
				{
					//본인 제외 foldOut을 체크

					if (!curUnit._isFoldOut)
					{
						//접혀있다면 펴자
						isAnyUnfolded = true;
						curUnit._isFoldOut = true;
					}
				}
				

				//부모로 이동
				if(curUnit._parentUnit == null
					|| curUnit._parentUnit == this)
				{
					break;
				}

				curUnit = curUnit._parentUnit;//한칸 위로 이동
			}

			return isAnyUnfolded;
		}

		public int GetLastPosY()
		{
			return _lastRenderedPosY;
		}


	}



	/// <summary>
	/// 추가 20.3.18 : HierarchyUnit을 Pool 형태로 관리하자
	/// </summary>
	public class apEditorHierarchyUnitPool
	{
		// Members
		//--------------------------------------------
		private List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		private List<apEditorHierarchyUnit> _units_Live = new List<apEditorHierarchyUnit>();
		private List<apEditorHierarchyUnit> _units_Ready = new List<apEditorHierarchyUnit>();

		//초기 개수와 증가 개수를 정하자
		private const int NUM_INIT = 200;
		private const int NUM_INCREASE = 50;

		private bool _isInitialized = false;

		public bool IsInitialized { get { return _isInitialized; } }


		// Init
		//--------------------------------------------
		public apEditorHierarchyUnitPool()
		{
			if(_units_All == null)
			{
				_units_All = new List<apEditorHierarchyUnit>();
			}
			if(_units_Live == null)
			{
				_units_Live = new List<apEditorHierarchyUnit>();
			}
			if(_units_Ready == null)
			{
				_units_Ready = new List<apEditorHierarchyUnit>();
			}
			_units_All.Clear();
			_units_Live.Clear();
			_units_Ready.Clear();

			_isInitialized = false;
			
		}

		/// <summary>
		/// 초기화. 별건 아니고, 풀의 크기가 0이라면, 초기 개수만큼 늘린다.
		/// </summary>
		public void Init()
		{
			if(_units_All.Count < NUM_INIT)
			{
				//초기 개수만큼 준비하자
				CreateUnits(NUM_INIT);
			}
			_isInitialized = true;
		}

		// Functions
		//--------------------------------------------
		private void CreateUnits(int nUnits)
		{
			for (int i = 0; i < nUnits; i++)
			{
				apEditorHierarchyUnit newUnit = apEditorHierarchyUnit.MakeUnit();
				newUnit.Clear();

				_units_All.Add(newUnit);
				_units_Ready.Add(newUnit);
			}
		}

		//만약 리소스를 다시 로드했다면 이 함수를 호출해서 모든 유닛들에 포함된 리소스를 갱신하자
		public void CheckAndReInitUnit()
		{
			for (int i = 0; i < _units_All.Count; i++)
			{
				_units_All[i].ReloadResources();
			}
		}


		/// <summary>
		/// 새롭게 Unit을 꺼내자
		/// </summary>
		/// <returns></returns>
		public apEditorHierarchyUnit PullUnit(int level)
		{
			if (_units_Ready.Count == 0)
			{
				//Debug.LogError("Pool 증가 [" + _units_All.Count + " > " + (_units_All.Count + NUM_INCREASE) + "]");

				//개수가 부족하다면 추가를 하자
				CreateUnits(NUM_INCREASE);

				
			}

			//맨앞 한개를 꺼내자 > Live에 넣자
			apEditorHierarchyUnit pullUnit = _units_Ready[0];
			_units_Ready.RemoveAt(0);
			
			_units_Live.Add(pullUnit);
			
			//데이터 비우고 리턴
			pullUnit.Clear();
			pullUnit.Init(level);

			return pullUnit;
		}

		/// <summary>
		/// 제거할 Unit을 반납하자.
		/// </summary>
		/// <param name="pushUnit"></param>
		public void PushUnit(apEditorHierarchyUnit pushUnit)
		{
			//Debug.LogWarning("반납 [" + pushUnit._text.ToString() + "]");
			pushUnit.Clear();
			
			//Live에서 빼서 Ready에 넣는다.
			_units_Live.Remove(pushUnit);

			if(!_units_Ready.Contains(pushUnit))
			{
				_units_Ready.Add(pushUnit);
			}

			
		}

		/// <summary>
		/// 모든 객체들을 Ready상태로 되돌린다.
		/// </summary>
		public void PushAll()
		{
			//Live는 모두 비우고, 모든 유닛들을 Ready로 넣는다. (동기화 과정)
			_units_Live.Clear();
			_units_Ready.Clear();

			int nUnits = _units_All.Count;
			apEditorHierarchyUnit curUnit = null;

			for (int i = 0; i < _units_All.Count; i++)
			{
				curUnit = _units_All[i];
				curUnit.Clear();

				_units_Ready.Add(curUnit);
			}

			//Debug.LogWarning("모두 반납 [" + _units_All.Count + "]");
		}
	}

}