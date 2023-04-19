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
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.1.18 : Unity Generic Menu를 래핑한 클래스
	/// 필요한 타입별로 미리 만들고 호출하자 (Enum 이용)
	/// </summary>
	public class apGUIMenu
	{
		// Members
		//----------------------------------------------
		//클릭한 메뉴의 콜백 이벤트
		public delegate void FUNC_MENU_CALLBACK(object menuIndex);

		public apEditor _editor = null;

		//각 타입별로 서브 메뉴
		//private MenuWrapper _menu_GUIView = null;
		public enum MENU_ITEM__GUIVIEW
		{
			FPS,
			Statistics,			
			MaximizeWorkspace,
			InvertBackground,//추가 21.10.6
			

			Mesh,
			Bone_Show,
			Bone_Outline,
			Physics,
			ToggleOnionSkin,
			RecordOnion,
			OnionSkinSetting,

			ModEditingSettings,
			ExModObj_UpdateByOtherModifiers,
			ExModObj_ShowAsGray,
			ExModObj_NotSelectable,
			ExModObj_PreviewColorResult,
			ExModObj_PreviewBoneResult,
			ExModObj_ShowModifierList,

			ToggleVisibilityPreset,
			VisibilityPresetSettings,
			VisibilityRule,
			ToggleRotoscoping,
			PrevRotoscopingImage,
			NextRotoscopingImage,
			RotoscopingSettings,
			RotoscopingData,

			HowToEdit,

			ToggleGuidelines,
			GuideLinesSettings,

			
		}
		public class MenuCallBackParam
		{
			public MENU_ITEM__GUIVIEW _menuType;
			public object _objParam = null;
			public MenuCallBackParam() { }

			public void SetParam(MENU_ITEM__GUIVIEW menuType, object objParam)
			{
				_menuType = menuType;
				_objParam = objParam;
			}
		}


		private List<MenuCallBackParam> _callBackParams = new List<MenuCallBackParam>();
		private int _iCallBackParams = 0;
		private MenuCallBackParam _cal_CallBackParam = null;

		private apStringWrapper _strWrapper = null;
		private List<apGUIContentWrapper> _guiContents = null;
		private int _iGUIContent = 0;
		private apGUIContentWrapper _cal_CurWrapper = null;

		private const string STR_EMPTY = "";
		private const string STR_NONAME = "<No Name>";
		private const string STR_DOTSPACE = ". ";

		private apStringWrapper _strWrapper_Rule = null;

		

		// Init
		//----------------------------------------------
		public apGUIMenu(apEditor editor)
		{
			_editor = editor;
			_strWrapper = new apStringWrapper(128);
			_strWrapper_Rule = new apStringWrapper(128);
		}


		

		// Functions
		//----------------------------------------------
		public void ShowMenu_GUIView(GenericMenu.MenuFunction2 callback, Rect rect)
		{	
			//일단 테스트로 직접 메뉴 입력
			//GUI View의 메뉴 순서
			//- FPS 보이기
			//- Statistics 보이기
			//- Mesh 보이기, 숨기기
			//- Bone 보이기, 숨기기, 외곽선만 보이기
			//- 물리 효과 활성, 비활성
			//- Onion Skin 보이기, 숨기기, 설정
			
			GenericMenu newGenericMenu = new GenericMenu();
			
			//GUI 만들 준비
			ReadyGUIContentsAndParams();




			//- FPS 보이기
			//- Statistics 보이기
			//"Show FPS"
			newGenericMenu.AddItem(	MakeText(_editor.GetUIWord(UIWORD.GUIMenu_ShowFPS)), 
									_editor._guiOption_isFPSVisible, 
									callback, 
									MakeParam(MENU_ITEM__GUIVIEW.FPS));
			//"Show Statistics"
			newGenericMenu.AddItem(	MakeText(_editor.GetUIWord(UIWORD.GUIMenu_ShowStatistics)), 
									_editor._guiOption_isStatisticsVisible, 
									callback, 
									MakeParam(MENU_ITEM__GUIVIEW.Statistics));

			
			//Show How to Use
			newGenericMenu.AddItem(	MakeText(_editor.GetUIWord(UIWORD.GUIMenu_ShowHowToEdit)),
									_editor._guiOption_isShowHowToEdit,
									callback, 
									MakeParam(MENU_ITEM__GUIVIEW.HowToEdit));

			newGenericMenu.AddSeparator(STR_EMPTY);

			//"Maximize Workspace (Alt+W)"
			newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleWorkspaceSize, _editor.GetUIWord(UIWORD.GUIMenu_MaximizeWorkspace)), 
									_editor._isFullScreenGUI, 
									callback, 
									MakeParam(MENU_ITEM__GUIVIEW.MaximizeWorkspace));

			//배경색 전환 (추가 21.10.6)
			newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleInvertBGColor, _editor.GetUIWord(UIWORD.GUIMenu_InvertBGColor)), 
									_editor._isInvertBackgroundColor, 
									callback, 
									MakeParam(MENU_ITEM__GUIVIEW.InvertBackground));
			

			//- Mesh 보이기, 숨기기
			//- Bone 보이기, 숨기기, 외곽선만 보이기
			//- 물리 효과 활성, 비활성
			if(_editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
				|| _editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation
				|| _editor.Select.SelectionType == apSelection.SELECTION_TYPE.Overall)
			{
				newGenericMenu.AddSeparator(STR_EMPTY);
				//"Show Meshes"
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleMeshVisibility, _editor.GetUIWord(UIWORD.GUIMenu_ShowMeshes)), 
										_editor._meshGUIRenderMode == apEditor.MESH_RENDER_MODE.Render, 
										callback, 
										MakeParam(MENU_ITEM__GUIVIEW.Mesh));

				newGenericMenu.AddSeparator(STR_EMPTY);
				//"Show Bones (B)"
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleBoneVisibility, _editor.GetUIWord(UIWORD.GUIMenu_ShowBones)), 
										_editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.Render, 
										callback, 
										MakeParam(MENU_ITEM__GUIVIEW.Bone_Show));
				//"Show Bones' Outline (B)"
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleBoneVisibility, _editor.GetUIWord(UIWORD.GUIMenu_ShowBonesOutline)), 
										_editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.RenderOutline, 
										callback, 
										MakeParam(MENU_ITEM__GUIVIEW.Bone_Outline));

				if(_editor._portrait != null)
				{
					newGenericMenu.AddSeparator(STR_EMPTY);

					//"Enable Physics (Alt+B)"
					newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.TogglePhysicsPreview, _editor.GetUIWord(UIWORD.GUIMenu_EnablePhysics)), 
											_editor._portrait._isPhysicsPlay_Editor, 
											callback, 
											MakeParam(MENU_ITEM__GUIVIEW.Physics));
				}
			}
			//- Onion Skin / 보이기 프리셋 / 로토스코핑 보이기, 숨기기, 설정
			if (_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation ||
					_editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				newGenericMenu.AddSeparator(STR_EMPTY);

				//"Onion Skin/Show Onion Skin"
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleOnionSkin, _editor.GetUIWord(UIWORD.GUIMenu_OnionSkin), _editor.GetUIWord(UIWORD.GUIMenu_ShowOnionSkin)),
										_editor.Onion.IsVisible,
										callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ToggleOnionSkin));
				//"Onion Skin/"
				newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_OnionSkin)));
				//"Onion Skin/Settings"
				newGenericMenu.AddItem(	MakeText(_editor.GetUIWord(UIWORD.GUIMenu_OnionSkin), _editor.GetUIWord(UIWORD.GUIMenu_Settings)),
										false, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.OnionSkinSetting));


				newGenericMenu.AddSeparator(STR_EMPTY);


				//Editing Settings
				newGenericMenu.AddItem(	MakeText(_editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), _editor.GetUIWord(UIWORD.GUIMenu_Settings)),
										false, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ModEditingSettings));

				newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions)));

				//Edit 옵션
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ExObj_UpdateByOtherMod, _editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), _editor.GetUIWord(UIWORD.GUIMenu_ExMod_ApplyOtherMod)),
										_editor._exModObjOption_UpdateByOtherMod, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ExModObj_UpdateByOtherModifiers));

				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ExObj_ShowAsGray, _editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), _editor.GetUIWord(UIWORD.GUIMenu_ExMod_ShowAsGray)),
										_editor._exModObjOption_ShowGray, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ExModObj_ShowAsGray));

				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ExObj_ToggleSelectionSemiLock, _editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), _editor.GetUIWord(UIWORD.GUIMenu_ExMod_SelectionLock)),
										_editor._exModObjOption_NotSelectable, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ExModObj_NotSelectable));


				newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions)));

				//Preview 옵션
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.PreviewModBoneResult, _editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), _editor.GetText(TEXT.DLG_ModLockPreviewCalculatedBones)),
										_editor._modLockOption_BoneResultPreview, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ExModObj_PreviewBoneResult));

				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.PreviewModColorResult, _editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), _editor.GetText(TEXT.DLG_ModLockRenderCalculatedColors)),
										_editor._modLockOption_ColorPreview, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ExModObj_PreviewColorResult));

				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ShowModifierListUI, _editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), _editor.GetText(TEXT.DLG_ModLockShowModifierList)),
										_editor._modLockOption_ModListUI, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.ExModObj_ShowModifierList));




				newGenericMenu.AddSeparator(STR_EMPTY);

				int nRules = 0;
				if (_editor._portrait != null)
				{
					nRules = _editor._portrait.VisiblePreset._rules != null ? _editor._portrait.VisiblePreset._rules.Count : 0;
				}

				//"Visibility Presets/Enable Preset"
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.TogglePresetVisibility, _editor.GetUIWord(UIWORD.GUIMenu_VisibilityPresets), _editor.GetUIWord(UIWORD.GUIMenu_EnablePreset)),
										_editor._isAdaptVisibilityPreset,
										nRules > 0 ? callback : null,
										MakeParam(MENU_ITEM__GUIVIEW.ToggleVisibilityPreset));
				
				//"Visibility Presets/"
				newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_VisibilityPresets)));

				//"Visibility Presets/Settings"
				newGenericMenu.AddItem(	MakeText(_editor.GetUIWord(UIWORD.GUIMenu_VisibilityPresets), _editor.GetUIWord(UIWORD.GUIMenu_Settings)),
										false, callback, 
										MakeParam(MENU_ITEM__GUIVIEW.VisibilityPresetSettings));
				

				//"Visibility Presets/Rule 1.."
				//이건 실제로 Preset들을 보여줘야 한다.
				if(_editor._portrait != null && nRules > 0)
				{
					//"Visibility Presets/"
					newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_VisibilityPresets)));

					//int nRules = _editor._portrait.VisiblePreset._rules != null ? _editor._portrait.VisiblePreset._rules.Count : 0;
					apVisibilityPresets.RuleData curRule = null;
					for (int iRule = 0; iRule < nRules; iRule++)
					{
						curRule = _editor._portrait.VisiblePreset._rules[iRule];
						bool isSelected = curRule == _editor._selectedVisibilityPresetRule;

						//규칙 이름을 설정하자
						_strWrapper_Rule.Clear();
						_strWrapper_Rule.Append((iRule + 1), false);
						_strWrapper_Rule.Append(STR_DOTSPACE, false);
						if(string.IsNullOrEmpty(curRule._name))
						{
							_strWrapper_Rule.Append(STR_NONAME, true);
						}
						else
						{
							_strWrapper_Rule.Append(curRule._name, true);
						}


						if(curRule._hotKey == apVisibilityPresets.HOTKEY.None)
						{
							//단축키가 없는 경우
							newGenericMenu.AddItem(
										MakeText(	_editor.GetUIWord(UIWORD.GUIMenu_VisibilityPresets), 
													_strWrapper_Rule.ToString()),
										isSelected,
										callback, 
										MakeParam(MENU_ITEM__GUIVIEW.VisibilityRule, curRule));
						}
						else
						{
							//단축키가 있는 경우
							apHotKeyMapping.KEY_TYPE linkedHotKey = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule1;
							switch (curRule._hotKey)
							{
								case apVisibilityPresets.HOTKEY.Hotkey1: linkedHotKey = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule1; break;
								case apVisibilityPresets.HOTKEY.Hotkey2: linkedHotKey = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule2; break;
								case apVisibilityPresets.HOTKEY.Hotkey3: linkedHotKey = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule3; break;
								case apVisibilityPresets.HOTKEY.Hotkey4: linkedHotKey = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule4; break;
								case apVisibilityPresets.HOTKEY.Hotkey5: linkedHotKey = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule5; break;
							}


							newGenericMenu.AddItem(
										MakeTextHotkey(	linkedHotKey, _editor.GetUIWord(UIWORD.GUIMenu_VisibilityPresets), 
														_strWrapper_Rule.ToString()),
										isSelected,
										callback, 
										MakeParam(MENU_ITEM__GUIVIEW.VisibilityRule, curRule));
						}
					}
				}
			}

			if (_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation ||
					_editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
					_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Overall)
			{
				newGenericMenu.AddSeparator(STR_EMPTY);


				//"Rotoscoping/Show Background Images"
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleRotoscoping, _editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping), _editor.GetUIWord(UIWORD.GUIMenu_EnableRotoscopingImages)),
										_editor._isEnableRotoscoping,
										callback, MakeParam(MENU_ITEM__GUIVIEW.ToggleRotoscoping));

				//"Rotoscoping/"
				newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping)));

				//이전 이미지 / 다음 이미지
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.RotoscopingPrev, _editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping), _editor.GetUIWord(UIWORD.GUIMenu_PrevImage)),//"Previous Image"
										false,
										callback, MakeParam(MENU_ITEM__GUIVIEW.PrevRotoscopingImage));

				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.RotoscopingNext, _editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping), _editor.GetUIWord(UIWORD.GUIMenu_NextImage)),//"Next Image"
										false,
										callback, MakeParam(MENU_ITEM__GUIVIEW.NextRotoscopingImage));


				//"Rotoscoping/"
				newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping)));

				//"Rotoscoping/Settings"
				newGenericMenu.AddItem(MakeText(_editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping), _editor.GetUIWord(UIWORD.GUIMenu_Settings)), 
										false, callback, MakeParam(MENU_ITEM__GUIVIEW.RotoscopingSettings));

				int nImageSetData = _editor.Rotoscoping._imageSetDataList != null ? _editor.Rotoscoping._imageSetDataList.Count : 0;
				if(nImageSetData > 0)
				{
					//"Rotoscoping/"
					newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping)));

					//로토스코핑 데이터들
					apRotoscoping.ImageSetData curImageSetData = null;

					for (int iISD = 0; iISD < nImageSetData; iISD++)
					{
						curImageSetData = _editor.Rotoscoping._imageSetDataList[iISD];

						bool isSelected = curImageSetData == _editor._selectedRotoscopingData;

						//규칙 이름을 설정하자
						_strWrapper_Rule.Clear();
						_strWrapper_Rule.Append((iISD + 1), false);
						_strWrapper_Rule.Append(STR_DOTSPACE, false);
						if(string.IsNullOrEmpty(curImageSetData._name))
						{
							_strWrapper_Rule.Append(STR_NONAME, true);
						}
						else
						{
							_strWrapper_Rule.Append(curImageSetData._name, true);
						}

						newGenericMenu.AddItem(
										MakeText(	_editor.GetUIWord(UIWORD.GUIMenu_Rotoscoping), 
													_strWrapper_Rule.ToString()),
										isSelected,
										callback, 
										MakeParam(MENU_ITEM__GUIVIEW.RotoscopingData, curImageSetData));
					}
				}


				//추가 21.6.4 : 가이드라인
				newGenericMenu.AddSeparator(STR_EMPTY);


				//"Guidelines/Show Guidelines"
				newGenericMenu.AddItem(	MakeTextHotkey(apHotKeyMapping.KEY_TYPE.ToggleGuidelines, _editor.GetUIWord(UIWORD.GUIMenu_Guidelines), _editor.GetUIWord(UIWORD.GUIMenu_ShowGuidelines)),
										_editor._isEnableGuideLine,
										callback, MakeParam(MENU_ITEM__GUIVIEW.ToggleGuidelines));

				//"Guidelines/"
				newGenericMenu.AddSeparator(MakeSeparatorPath(_editor.GetUIWord(UIWORD.GUIMenu_Guidelines)));

				//"Rotoscoping/Settings"
				newGenericMenu.AddItem(MakeText(_editor.GetUIWord(UIWORD.GUIMenu_Guidelines), _editor.GetUIWord(UIWORD.GUIMenu_Settings)), 
										false, callback, MakeParam(MENU_ITEM__GUIVIEW.GuideLinesSettings));
			}

			
			


			newGenericMenu.DropDown(rect);
			
			//if(Event.current != null)
			//{
			//	Event.current.Use();
			//}
		}



		// Sub-Functions
		//----------------------------------------------
		private void ReadyGUIContentsAndParams()
		{
			if(_guiContents == null)
			{
				_guiContents = new List<apGUIContentWrapper>();
			}
			_iGUIContent = 0;

			if(_callBackParams == null)
			{
				_callBackParams = new List<MenuCallBackParam>();
			}
			
			_iCallBackParams = 0;
		}

		private GUIContent MakeText(string path1, string path2 = null)
		{
			_strWrapper.Clear();
			if(path2 == null)
			{
				_strWrapper.Append(path1, true);
			}
			else
			{
				_strWrapper.Append(path1, false);
				_strWrapper.Append(apStringFactory.I.Slash, false);
				_strWrapper.Append(path2, true);
			}
			
			//현재 커서에 맞는 Conent에 값 입력
			if(_iGUIContent < _guiContents.Count)
			{
				_cal_CurWrapper = _guiContents[_iGUIContent];
			}
			else
			{
				_cal_CurWrapper = new apGUIContentWrapper();
				_guiContents.Add(_cal_CurWrapper);
			}
			
			_cal_CurWrapper.SetText(_strWrapper.ToString());

			_iGUIContent++;
			return _cal_CurWrapper.Content;
		}

		private GUIContent MakeTextHotkey(apHotKeyMapping.KEY_TYPE hotkey, string path1, string path2 = null)
		{
			_strWrapper.Clear();
			if(path2 == null)
			{
				_strWrapper.Append(path1, false);
			}
			else
			{
				_strWrapper.Append(path1, false);
				_strWrapper.Append(apStringFactory.I.Slash, false);
				_strWrapper.Append(path2, false);
			}
			_editor.HotKeyMap.AddHotkeyTextToWrapper(hotkey, _strWrapper, true);
			
			//현재 커서에 맞는 Conent에 값 입력
			if(_iGUIContent < _guiContents.Count)
			{
				_cal_CurWrapper = _guiContents[_iGUIContent];
			}
			else
			{
				_cal_CurWrapper = new apGUIContentWrapper();
				_guiContents.Add(_cal_CurWrapper);
			}

			_cal_CurWrapper.SetText(_strWrapper.ToString());

			_iGUIContent++;
			return _cal_CurWrapper.Content;
		}
		
		private string MakeSeparatorPath(string path)
		{
			_strWrapper.Clear();
			_strWrapper.Append(path, false);
			_strWrapper.Append(apStringFactory.I.Slash, true);
			return _strWrapper.ToString();
		}


		private MenuCallBackParam MakeParam(MENU_ITEM__GUIVIEW menuType, object param2 = null)
		{
			
			if(_iCallBackParams >= _callBackParams.Count)
			{
				_callBackParams.Add(new MenuCallBackParam());
			}
			_cal_CallBackParam = _callBackParams[_iCallBackParams];
			_iCallBackParams++;

			_cal_CallBackParam.SetParam(menuType, param2);

			return _cal_CallBackParam;

		}
	}
}