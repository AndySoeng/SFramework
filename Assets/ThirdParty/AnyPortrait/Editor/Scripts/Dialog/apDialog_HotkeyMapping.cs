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
using System;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{
	//추가 20.11.30
	//단축키를 매핑하는 설정화면. Setting 다이얼로그로부터 열린다.
	public class apDialog_HotkeyMapping : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_HotkeyMapping s_window = null;

		private apEditor _editor = null;
		private Vector2 _scroll = Vector2.zero;

		private int _width = 0;
		private int _height = 0;

		//중간 리스트
		//- Common : 공통
		//- Make Mesh : 메시 제작시
		//- Editing : 편집시
		//- AnimPlayBack : 애니메이션 재생
		//- Rigging : 리깅
		//+ 추가 구현시 여기에 표시

		
		private apGUIContentWrapper _guiContent_Name = null;
		private apGUIContentWrapper _guiContent_RestoreBtn = null;
		
		private apGUIContentWrapper _guiContent_Group__Common = null;
		private apGUIContentWrapper _guiContent_Group__MakeMesh = null;
		private apGUIContentWrapper _guiContent_Group__ModifierAnimEditing = null;
		private apGUIContentWrapper _guiContent_Group__AnimPlayBack = null;
		private apGUIContentWrapper _guiContent_Group__Rigging = null;

		private GUIStyle _guiStyle_Label_Default = null;
		private GUIStyle _guiStyle_Label_Reserved = null;
		private GUIStyle _guiStyle_Label_Changed = null;
		private GUIStyle _guiStyle_Label_Warning = null;
		

		private string[] _enumLabels_SpecialKeys = null;
		private string[] _enumLabels_KeyCodes = null;

		
		


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor)
		{
			//Debug.Log("Show Dialog - Portrait Setting");
			CloseDialog();


			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_HotkeyMapping), true, "Shortcuts", true);
			apDialog_HotkeyMapping curTool = curWindow as apDialog_HotkeyMapping;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				//이전 크기
				//int width = 400;
				//int height = 500;

				//변경 20.3.26
				int width = 650;
				int height = 750;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);


				s_window.Init(editor);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);
				}
				s_window = null;
			}
		}

		// Init
		//------------------------------------------------------------------
		public void Init(apEditor editor)
		{
			_editor = editor;

			_editor.HotKeyMap.CheckConflict();//중복 여부를 체크하자
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			_width = (int)position.width;
			_height = (int)position.height;

			if (_editor == null)
			{
				//Debug.LogError("Exit - Editor / Portrait is Null");
				CloseDialog();
				return;
			}
			

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor)
			{
				//Debug.LogError("Exit - Editor / Portrait Missmatch");
				CloseDialog();
				return;
			}
			
			//Enum Label을 만들자
			CheckAndMakeGUIContentAndLabels();
			


			

			


			Dictionary<apHotKeyMapping.SPACE, List<apHotKeyMapping.HotkeyMapUnit>> eventSpace2Units = _editor.HotKeyMap.EventSpace2Units;

			int scrollHeight = _height - 85;
			int listWidth = _width - 24;
			int listHeight = 20;

			
			int width_CheckBox = 15;
			int width_KeyCode = 100;
			int width_SpecialKey = 100;

			int width_RestoreBtn = 65;
			int width_Name = listWidth - (5 + width_CheckBox + 5 + (width_KeyCode + 2) + (width_SpecialKey + 2) + 10 + (width_RestoreBtn + 2) + 24);
			

			_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(_width), GUILayout.Height(scrollHeight));
			GUILayout.BeginVertical(GUILayout.Width(listWidth));

			apHotKeyMapping.HotkeyMapUnit curHotKey = null;
			

			
			GUILayout.Space(10);

			//내용은
			//체크박스 / 이름 (Tooltip) / KeyCode / Ctrl / Alt / Shift / (복구 버튼) 순서이다.
			//Reserved인 경우 체크박스 생략하고 이름 색상이 다르다.
			//Reserved인 경우 : 체크박스가 없다 / 텍스트 색상이 다르다
			//Special Key를 무시하는 경우 : Special Key 부분이 비어있다.
			//(Reserved가 아닐때) 값이 변경된 경우 : 이름의 색상이 다르다

			bool isAnyChanged = false;
			GUIStyle curGuiStyle = null;
			apGUIContentWrapper curGroupContent = null;

			
			foreach (KeyValuePair<apHotKeyMapping.SPACE, List<apHotKeyMapping.HotkeyMapUnit>> hotKeyUnits in eventSpace2Units)
			{
				
				List<apHotKeyMapping.HotkeyMapUnit> hotKeys = hotKeyUnits.Value;

				//그룹 전에 이름을 보여주자

				switch (hotKeyUnits.Key)
				{
					case apHotKeyMapping.SPACE.Common:			curGroupContent = _guiContent_Group__Common; break;
					case apHotKeyMapping.SPACE.MakeMesh:		curGroupContent = _guiContent_Group__MakeMesh; break;
					case apHotKeyMapping.SPACE.Modifier_Anim_Editing: curGroupContent = _guiContent_Group__ModifierAnimEditing; break;
					case apHotKeyMapping.SPACE.AnimPlayBack:	curGroupContent = _guiContent_Group__AnimPlayBack; break;
					case apHotKeyMapping.SPACE.Rigging:		curGroupContent = _guiContent_Group__Rigging; break;
				}

				if(hotKeyUnits.Key != apHotKeyMapping.SPACE.Common)
				{
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(listWidth);
					GUILayout.Space(10);
				}

				EditorGUILayout.LabelField(curGroupContent.Content);
				GUILayout.Space(10);


				//리스트들
				for (int i = 0; i < hotKeys.Count; i++)
				{
					curHotKey = hotKeys[i];

					//TODO : isOverlappedWarning 체크

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(listWidth), apGUILOFactory.I.Height(listHeight));
					GUILayout.Space(5);

					_guiContent_Name.ClearAll();
					_guiContent_Name.SetTextImageToolTip(curHotKey._label.ToString(), null, curHotKey._info.ToString());
					//_guiContent_Name.SetTextImageToolTip("테스트..", null, curHotKey._info.ToString());

					if (curHotKey._isReserved)
					{
						//Reserved라면 편집 불가
						curGuiStyle = _guiStyle_Label_Reserved;
					}
					else if (curHotKey._conflictedUnit != null)
					{
						//다른 단축키와 겹쳤다면..
						curGuiStyle = _guiStyle_Label_Warning;
					}
					else if (curHotKey.IsChanged())
					{
						//변경이 되었다면
						curGuiStyle = _guiStyle_Label_Changed;
					}
					else
					{
						//기본
						curGuiStyle = _guiStyle_Label_Default;
					}

					if (curHotKey._isReserved)
					{
						//Reserved라면 체크박스를 표시하지 않는다.
						GUILayout.Space(width_CheckBox + 4);
					}
					else
					{
						//Reserved가 아니면 체크박스를 표시한다.
						bool nextAvailable = EditorGUILayout.Toggle(curHotKey._isAvailable_Cur, apGUILOFactory.I.Width(width_CheckBox), apGUILOFactory.I.Height(listHeight));
						if (nextAvailable != curHotKey._isAvailable_Cur)
						{
							//- Available 변경하기
							curHotKey.SetValue_Available(nextAvailable);
							isAnyChanged = true;
						}
					}

					EditorGUILayout.LabelField(_guiContent_Name.Content, curGuiStyle, apGUILOFactory.I.Width(width_Name), apGUILOFactory.I.Height(listHeight));
					GUILayout.Space(5);

					//Special Key를 사용하지 않는 단축키의 경우
					if (curHotKey._isIgnoreSpecialKey)
					{
						GUILayout.Space(width_SpecialKey + 4);
					}
					else
					{
						apHotKeyMapping.SPECIAL_KEY prevSpecialKey = curHotKey.GetSpecialKeyComb();
						apHotKeyMapping.SPECIAL_KEY nextSpecialKey = (apHotKeyMapping.SPECIAL_KEY)EditorGUILayout.Popup((int)prevSpecialKey, _enumLabels_SpecialKeys, apGUILOFactory.I.Width(width_SpecialKey));

						if (prevSpecialKey != nextSpecialKey && !curHotKey._isReserved)
						{
							curHotKey.SetValue_SpecialKey(nextSpecialKey);
							isAnyChanged = true;
						}
					}


					apHotKeyMapping.EST_KEYCODE prevKeyCode = curHotKey._keyCode_Cur;
					apHotKeyMapping.EST_KEYCODE nextKeyCode = (apHotKeyMapping.EST_KEYCODE)EditorGUILayout.Popup((int)(curHotKey._keyCode_Cur), _enumLabels_KeyCodes, apGUILOFactory.I.Width(width_KeyCode));

					if (prevKeyCode != nextKeyCode && !curHotKey._isReserved)
					{
						curHotKey.SetValue_KeyCode(nextKeyCode);
						isAnyChanged = true;
					}

					GUILayout.Space(10);

					if (!curHotKey._isReserved)
					{
						//Reserved가 아니라면 Default 버튼
						if (GUILayout.Button(_guiContent_RestoreBtn.Content, apGUILOFactory.I.Width(width_RestoreBtn)))
						{
							curHotKey.Restore();
							isAnyChanged = true;
						}
					}
					

					EditorGUILayout.EndHorizontal();

					if (!curHotKey._isReserved)
					{
						if (curHotKey._conflictedUnit != null)
						{
							//만약 중복이 되었다면
							string strConflictUnitName = curHotKey._conflictedUnit._label.ToString();
							if (strConflictUnitName.Length > 30)
							{
								strConflictUnitName = strConflictUnitName.Substring(0, 28) + "..";
							}
							Color prevColor = GUI.backgroundColor;
							GUI.backgroundColor = new Color(GUI.backgroundColor.r * 2.0f, GUI.backgroundColor.g * 0.5f, GUI.backgroundColor.b * 0.4f, 1.0f);
							//"It is conflicted with [ " + strConflictUnitName + " ]."
							GUILayout.Box(_editor.GetTextFormat(TEXT.ShortcutWarning_Conflict, strConflictUnitName), apGUILOFactory.I.Width(listWidth), apGUILOFactory.I.Height(20));
							GUI.backgroundColor = prevColor;
							GUILayout.Space(10);
						}
						else if (curHotKey.IsInvalidKeyCode())
						{
							//만약 선택될 수 없는 키를 선택했다면
							Color prevColor = GUI.backgroundColor;
							GUI.backgroundColor = new Color(GUI.backgroundColor.r * 2.0f, GUI.backgroundColor.g * 0.5f, GUI.backgroundColor.b * 0.4f, 1.0f);
							//"The selected key [ " + _enumLabels_KeyCodes[(int)(curHotKey._keyCode_Cur)] + " ] cannot be used."
							GUILayout.Box(_editor.GetTextFormat(TEXT.ShortcutWarning_KeyLimit, _enumLabels_KeyCodes[(int)(curHotKey._keyCode_Cur)]), apGUILOFactory.I.Width(listWidth), apGUILOFactory.I.Height(20));
							GUI.backgroundColor = prevColor;
							GUILayout.Space(10);
						}
					}

					GUILayout.Space(5);
				}

				GUILayout.Space(10);
			}
			


			GUILayout.Space(_height + 500);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			

			if(isAnyChanged)
			{
				_editor.HotKeyMap.Save();//저장!
				_editor.HotKeyMap.CheckConflict();//중복 여부도 체크하자
			}

			if(GUILayout.Button(_editor.GetText(TEXT.RestoreAllShortcuts), apGUILOFactory.I.Height(20)))//"Restore All Shortcuts"
			{
				//경고 문구
				bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_RestoreAllShortcuts_Title),//"Restore all shortcuts"
											_editor.GetText(TEXT.DLG_RestoreAllShortcuts_Body),//"Do you want to restore all shortcut settings? (This operation cannot be undone.)"
											_editor.GetText(TEXT.Restore),
											_editor.GetText(TEXT.Cancel));

				if (result)
				{
					_editor.HotKeyMap.RestoreAll();
					_editor.HotKeyMap.CheckConflict();
					_editor.HotKeyMap.Save();
				}
			}
			if(GUILayout.Button(_editor.GetText(TEXT.OpenShortcutsPage), apGUILOFactory.I.Height(20)))//"Open Shortcuts Manual Page"
			{
				//단축키 관련 매뉴얼 페이지를 열자
				if(_editor._language == apEditor.LANGUAGE.Korean)
				{
					Application.OpenURL("https://rainyrizzle.github.io/kr/AdvancedManual/AD_Shortcuts.html");
				}
				else if(_editor._language == apEditor.LANGUAGE.Japanese)
				{
					Application.OpenURL("https://rainyrizzle.github.io/jp/AdvancedManual/AD_Shortcuts.html");
				}
				else
				{
					Application.OpenURL("https://rainyrizzle.github.io/en/AdvancedManual/AD_Shortcuts.html");
				}
			}
			if(GUILayout.Button(_editor.GetText(TEXT.Close), apGUILOFactory.I.Height(25)))
			{
				CloseDialog();
			}
		}

		private void CheckAndMakeGUIContentAndLabels()
		{
			if(_enumLabels_SpecialKeys == null)
			{
				#region [미사용 코드]
				//				//apHotKeyMapping.SPECIAL_KEY_COMBINATION의 순서대로 정의
				//				//OSX에선 다르게 정의
				//#if UNITY_EDITOR_OSX
				//				_enumLabels_SpecialKeys = new string[]
				//				{
				//					"None",
				//					"Command",
				//					"Shift",
				//					"Option",
				//					"Command+Shift",
				//					"Command+Option",
				//					"Shift+Option",
				//					"Command+Shift+Option"
				//				};
				//#else
				//				_enumLabels_SpecialKeys = new string[]
				//				{
				//					"None",
				//					"Ctrl",
				//					"Shift",
				//					"Alt",
				//					"Ctrl+Shift",
				//					"Ctrl+Alt",
				//					"Shift+Alt",
				//					"Ctrl+Shift+Alt"
				//				};
				//#endif 
				#endregion

				//변경. 있는거 사용하자
				_enumLabels_SpecialKeys = _editor.HotKeyMap.SpecialKeyTexts;
			}
			
			if(_enumLabels_KeyCodes == null)
			{
				#region [미사용 코드]
				////apHotKeyMapping.ESSENTIAL_KEYCODE의 순서대로 정의
				//_enumLabels_KeyCodes = new string[]
				//{
				//	"A", "B", "C", "D", "E", "F", "G", "H", "I", 
				//	"J", "K", "L", "M", "N", "O", "P", "Q", "R", 
				//	"S", "T", "U", "V", "W", "X", "Y", "Z",

				//	"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",

				//	"- _", "= +",
				//	"[ {", "] }", "\\ |",
				//	"; :", "' \"", "` ~",
				//	", <", ". >", "/ ?",

				//	"Numpad 0", "Numpad 1", "Numpad 2", "Numpad 3", "Numpad 4",
				//	"Numpad 5", "Numpad 6", "Numpad 7", "Numpad 8", "Numpad 9",

				//	"F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
				//	"Home", "End", "PageUp", "PageDown", "Space Bar",

				//	"▲ (Up Arrow)", "▼ (Down Arrow)", "◀ (Left Arrow)", "▶ (Right Arrow)",

				//	"Enter", "Esc", "Delete (Backspace)",

				//	"Unknown Key",
				//}; 
				#endregion

				//변경. 있는거 사용하자
				_enumLabels_KeyCodes = _editor.HotKeyMap.KeycodeTexts;
			}

			
			if(_guiStyle_Label_Default == null)
			{
				_guiStyle_Label_Default = new GUIStyle(GUI.skin.label);
			}
			if(_guiStyle_Label_Reserved == null)
			{
				_guiStyle_Label_Reserved = new GUIStyle(GUI.skin.label);

				if (EditorGUIUtility.isProSkin)
				{
					//어두운 색이면 > 살짝 밝은 회색
					_guiStyle_Label_Reserved.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
				}
				else
				{
					//밝은 색이면 > 어두운 회색
					_guiStyle_Label_Reserved.normal.textColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
				}
				
			}

			if (_guiStyle_Label_Changed == null)
			{
				_guiStyle_Label_Changed = new GUIStyle(GUI.skin.label);
				if (EditorGUIUtility.isProSkin)
				{
					//어두운 색이면 > 노란색
					_guiStyle_Label_Changed.normal.textColor = Color.yellow;
				}
				else
				{
					//밝은 색이면 > 진한 보라색
					_guiStyle_Label_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.8f, 1.0f);
				}
			}
			
			if(_guiStyle_Label_Warning == null)
			{
				_guiStyle_Label_Warning = new GUIStyle(GUI.skin.label);
				if (EditorGUIUtility.isProSkin)
				{
					//어두운 색이면 > 밝은 주황색
					_guiStyle_Label_Warning.normal.textColor = new Color(1.0f, 0.7f, 0.3f, 1.0f);
				}
				else
				{
					//밝은 색이면 > 붉은색에 가까운 주황색
					_guiStyle_Label_Warning.normal.textColor = new Color(1.0f, 0.2f, 0.0f, 1.0f);
				}
			}


			if(_guiContent_RestoreBtn == null)
			{
				//_guiContent_RestoreBtn = apGUIContentWrapper.Make(_editor.ImageSet.Get(apImageSet.PRESET.Controller_Default));
				_guiContent_RestoreBtn = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Default), false);
			}
			if(_guiContent_Name == null)
			{
				_guiContent_Name = new apGUIContentWrapper(false);
			}

			if(_guiContent_Group__Common == null)
			{
				_guiContent_Group__Common = apGUIContentWrapper.Make(_editor.GetText(TEXT.ShortcutSpace_Common), false);//"Common"
			}			
			if(_guiContent_Group__MakeMesh == null)
			{
				_guiContent_Group__MakeMesh = apGUIContentWrapper.Make(_editor.GetText(TEXT.ShortcutSpace_MakeMesh), false);//"Making Meshes"
			}
			if(_guiContent_Group__ModifierAnimEditing == null)
			{
				_guiContent_Group__ModifierAnimEditing = apGUIContentWrapper.Make(_editor.GetText(TEXT.ShortcutSpace_EditModAnim), true);//"Editing Modifiers or Animations"
			}
			if(_guiContent_Group__AnimPlayBack == null)
			{
				_guiContent_Group__AnimPlayBack = apGUIContentWrapper.Make(_editor.GetText(TEXT.ShortcutSpace_Anim), false);//"Controlling Animation"
			}
			if(_guiContent_Group__Rigging == null)
			{
				_guiContent_Group__Rigging = apGUIContentWrapper.Make(_editor.GetText(TEXT.ShortcutSpace_Rigging), true);//"Editing the Rigging Modifier"
			}

			
		}
	}
}