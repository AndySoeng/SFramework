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
using System.IO;

namespace AnyPortrait
{

	public class apDialog_PortraitSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_PortraitSetting s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		//private object _loadKey = null;


		private enum TAB
		{
			PortriatSetting,
			EditorSetting,
			About
		}

		private TAB _tab = TAB.PortriatSetting;
		private Vector2 _scroll = Vector2.zero;

		private int _width = 0;
		private int _height = 0;


		private string[] _strLanguageName = new string[]
		{
			"English",//"English" 0
			"한국어",//Korean 1
			"Français",//French 2
			"Deutsch",//German 3
			"Español",//Spanish 4
			"Dansk",//Danish 6
			"日本語",//Japanese 7
			"繁體中文",//Chinese_Traditional 8
			"簡體中文",//Chinese_Simplified 9
			"Italiano",//Italian 5 -> 현재 미지원
			"Polski",//Polish 10 -> 현재 미지원

		};

		//실제로 지원하는 언어 인덱스를 적는다.
		//0 -> 0 (English) 이런 방식
		//현재 Italian (5), Polish (10) 제외됨
		private int[] _validLanguageIndex = new int[]
		{
			0,	//English
			1,	//Korean
			2,	//French
			3,	//German
			4,	//Spanish
			6,	//Danish
			7,	//Japanese
			8,	//Chinese-Trad
			9,	//Chinese-Simp
			5,	//Italian
			10,	//Polish
		};
		private apGUIContentWrapper _guiContent_IsImportant = null;
		private apGUIContentWrapper _guiContent_FPS = null;

		
		private string[] _strBoneGUIRenderTypeNames = new string[]
		{
			"Arrowhead (v1)",
			"Needle (v2)"
		};



		private string[] _rootBoneScaleOptionLabel = new string[] { "Default", "Non-Uniform Scale"};

		private string[] _editorUpdateModeLabel = new string[] {"Accelerated Mode (Native Plugin)", "Compatible Mode" };

		private GUIStyle _guiStyle_WrapLabel_Default = null;
		private GUIStyle _guiStyle_WrapLabel_Changed = null;

		//기본값을 비교하여, 기본값과 다르면 다른 색으로 표시하자.
		private GUIStyle _guiStyle_Label_Default = null;
		private GUIStyle _guiStyle_Label_Changed = null;

		private GUIStyle _guiStyle_Text_About = null;

		private GUIStyle _guiStyle_Box = null;

		private string _aboutText_1_PSD = null;
		private string _aboutText_2_NGif = null;
		private string _aboutText_3_Font = null;


		private string[] _strCustomHierarchyIconModes = new string[]
		{
			"Hide Icons", "Show Icons (Left)", "Show Icons (Right)"
		};

		private int _iCustomHierarchyIconOption = 0;


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			//Debug.Log("Show Dialog - Portrait Setting");
			CloseDialog();


			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_PortraitSetting), true, "Setting", true);
			apDialog_PortraitSetting curTool = curWindow as apDialog_PortraitSetting;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				//이전 크기
				//int width = 400;
				//int height = 500;

				//변경 20.3.26
				int width = 500;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);


				s_window.Init(editor, portrait, loadKey);

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
		public void Init(apEditor editor, apPortrait portrait, object loadKey)
		{
			_editor = editor;
			//_loadKey = loadKey;
			_targetPortrait = portrait;

			_aboutText_1_PSD = null;
			_aboutText_2_NGif = null;
			_aboutText_3_Font = null;

			//추가 21.8.22 : Unity 에디터상에서의 옵션 (AnyPortrait 에디터와는 상관없다.)
			bool isCustomHierarchy = EditorPrefs.GetBool("AnyPortrait_ShowCustomHierarchyIcon", true);
			bool isIconDrawLeft = EditorPrefs.GetBool("AnyPortrait_ShowCustomIconLeft", true);
			if(!isCustomHierarchy)
			{
				_iCustomHierarchyIconOption = 0;//None
			}
			else
			{
				if(isIconDrawLeft)
				{
					_iCustomHierarchyIconOption = 1;//Left
				}
				else
				{
					_iCustomHierarchyIconOption = 2;//Right
				}
			}


			MakeAboutText();
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			try
			{
				_width = (int)position.width;
				_height = (int)position.height;

				if (_editor == null || _targetPortrait == null)
				{
					//Debug.LogError("Exit - Editor / Portrait is Null");
					CloseDialog();
					return;
				}

				//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
				if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
				{
					//Debug.LogError("Exit - Editor / Portrait Missmatch");
					CloseDialog();
					return;

				}

				if (_guiStyle_WrapLabel_Default == null)
				{
					_guiStyle_WrapLabel_Default = new GUIStyle(GUI.skin.label);
					_guiStyle_WrapLabel_Default.wordWrap = true;
					_guiStyle_WrapLabel_Default.alignment = TextAnchor.MiddleLeft;
				}


				if (_guiStyle_WrapLabel_Changed == null)
				{
					_guiStyle_WrapLabel_Changed = new GUIStyle(GUI.skin.label);
					_guiStyle_WrapLabel_Changed.wordWrap = true;
					_guiStyle_WrapLabel_Changed.alignment = TextAnchor.MiddleLeft;
					if (EditorGUIUtility.isProSkin)
					{
						//어두운 색이면 > 노란색
						_guiStyle_WrapLabel_Changed.normal.textColor = Color.yellow;
					}
					else
					{
						//밝은 색이면 보라색
						_guiStyle_WrapLabel_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.5f, 1.0f);
					}
				}


				//기본값을 비교하여, 기본값과 다르면 다른 색으로 표시하자.
				if (_guiStyle_Label_Default == null)
				{
					_guiStyle_Label_Default = new GUIStyle(GUI.skin.label);
					_guiStyle_Label_Default.alignment = TextAnchor.UpperLeft;
				}
				if (_guiStyle_Label_Changed == null)
				{
					_guiStyle_Label_Changed = new GUIStyle(GUI.skin.label);
					_guiStyle_Label_Changed.alignment = TextAnchor.UpperLeft;
					if (EditorGUIUtility.isProSkin)
					{
						//어두운 색이면 > 노란색
						_guiStyle_Label_Changed.normal.textColor = Color.yellow;
					}
					else
					{
						//밝은 색이면 진한 보라색
						_guiStyle_Label_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.5f, 1.0f);
					}
				}
				if(_guiStyle_Box == null)
				{
					_guiStyle_Box = new GUIStyle(GUI.skin.box);
					_guiStyle_Box.alignment = TextAnchor.MiddleCenter;
					_guiStyle_Box.wordWrap = true;
				}


				//탭
				int tabBtnHeight = 25;
				int tabBtnWidth = ((_width - 10) / 3) - 4;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(_width), GUILayout.Height(tabBtnHeight));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Portrait), _tab == TAB.PortriatSetting, tabBtnWidth, tabBtnHeight))//"Portrait"
				{
					_tab = TAB.PortriatSetting;
				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Editor), _tab == TAB.EditorSetting, tabBtnWidth, tabBtnHeight))//"Editor"
				{
					_tab = TAB.EditorSetting;
				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_About), _tab == TAB.About, tabBtnWidth, tabBtnHeight))//"About"
				{
					_tab = TAB.About;
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				int scrollHeight = _height - 40;
				_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(_width), GUILayout.Height(scrollHeight));
				_width -= 25;
				GUILayout.BeginVertical(GUILayout.Width(_width));

				if (_guiContent_IsImportant == null)
				{
					_guiContent_IsImportant = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_IsImportant), false, "When this setting is on, it always updates and the physics effect works.");
				}
				if (_guiContent_FPS == null)
				{
					_guiContent_FPS = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Setting_FPS), false, "This setting is used when <Important> is off");
				}


				switch (_tab)
				{
					case TAB.PortriatSetting:
						{
							DrawUI_PortraitTab();
						}
						break;

					case TAB.EditorSetting:
						{
							DrawUI_SettingTab();
						}
						break;

					case TAB.About:
						{	
							DrawUI_AboutTab();
						}
						break;
				}



				GUILayout.Space(_height);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndScrollView();
			}
			catch(Exception ex)
			{
				//추가 21.3.17 : Try-Catch 추가. Mac에서 에러가 발생하기 쉽다.
				Debug.LogError("AnyPortrait : Exception occurs : " + ex);
			}
		}





		// Portrait 탭 UI
		//------------------------------------------------------------------
		private void DrawUI_PortraitTab()
		{
			//Portrait 설정
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PortraitSetting));//"Portrait Settings"


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			string nextName = EditorGUILayout.DelayedTextField(_editor.GetText(TEXT.DLG_Name), _targetPortrait.name);//"Name"
			if (nextName != _targetPortrait.name)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait.name = nextName;

			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			//"Is Important"
			bool nextImportant = EditorGUILayout.Toggle(_guiContent_IsImportant.Content, _targetPortrait._isImportant);
			if (nextImportant != _targetPortrait._isImportant)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._isImportant = nextImportant;
			}

			//"FPS (Important Off)"
			int nextFPS = EditorGUILayout.DelayedIntField(_guiContent_FPS.Content, _targetPortrait._FPS);
			if (_targetPortrait._FPS != nextFPS)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				if (nextFPS < 10)
				{
					nextFPS = 10;
				}
				_targetPortrait._FPS = nextFPS;
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			//추가 20.9.2 : 스케일 옵션을 넣자
			//추가 20.8.14 : 루트 본의 스케일 옵션을 직접 정한다. [Skew 문제]
			int iNextRootBoneScaleOption = EditorGUILayout.Popup(_editor.GetText(TEXT.Setting_ScaleOfRootBone), (int)_targetPortrait._rootBoneScaleMethod, _rootBoneScaleOptionLabel);
			if (iNextRootBoneScaleOption != (int)_targetPortrait._rootBoneScaleMethod)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_SettingChanged,
													_editor,
													_targetPortrait,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				_targetPortrait._rootBoneScaleMethod = (apPortrait.ROOT_BONE_SCALE_METHOD)iNextRootBoneScaleOption;

				//모든 본에 대해서 ScaleOption을 적용해야한다.
				_editor.Controller.RefreshBoneScaleMethod_Editor();
			}





			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			//수동으로 백업하기
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Setting_ManualBackUp), GUILayout.Height(30)))//"Save Backup (Manual)"
			{
				if (_editor.Backup.IsAutoSaveWorking())
				{
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.BackupError_Title),
												_editor.GetText(TEXT.BackupError_Body),
												_editor.GetText(TEXT.Okay));
				}
				else
				{
					string defaultBackupFileName = _targetPortrait.name + "_backup_" + apBackup.GetCurrentTimeString();
					string savePath = EditorUtility.SaveFilePanel("Backup File Path",
																	apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BackupFile), defaultBackupFileName,
																	"bck");
					if (string.IsNullOrEmpty(savePath))
					{
						_editor.Notification("Backup Canceled", true, false);
					}
					else
					{
						//추가 21.7.3 : 이스케이프 문자 삭제
						savePath = apUtil.ConvertEscapeToPlainText(savePath);

						_editor.Backup.SaveBackupManual(savePath, _targetPortrait);
						_editor.Notification("Backup Saved [" + savePath + "]", false, true);

						apEditorUtil.SetLastExternalOpenSaveFilePath(savePath, apEditorUtil.SAVED_LAST_FILE_PATH.BackupFile);//추가 21.3.1
					}

					CloseDialog();
				}
			}
		}



		// Setting 탭 UI
		//------------------------------------------------------------------
		private void DrawUI_SettingTab()
		{
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_EditorSetting));//"Editor Settings"

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);



			apEditor.LANGUAGE prevLanguage = _editor._language;
			apEditor.LANGUAGE defLanguage = apEditor.DefaultLanguage;
			int prevLangIndex = -1;
			int defLangIndex = -1;
			for (int i = 0; i < _validLanguageIndex.Length; i++)
			{
				if (_validLanguageIndex[i] == (int)prevLanguage)
				{
					prevLangIndex = i;
				}

				if (_validLanguageIndex[i] == (int)defLanguage)
				{
					defLangIndex = i;
				}
			}
			if (prevLangIndex < 0)	{ prevLangIndex = 0; }//English 강제
			if (defLangIndex < 0)	{ defLangIndex = 0; }//English 강제



			bool prevGUIFPS = _editor._guiOption_isFPSVisible;
			bool prevGUIStatistics = _editor._guiOption_isStatisticsVisible;
			bool prevUseCPPDLL = _editor._cppPluginOption_UsePlugin;

			Color prevColor_Background = _editor._colorOption_Background;
			Color prevColor_GridCenter = _editor._colorOption_GridCenter;
			Color prevColor_Grid = _editor._colorOption_Grid;
			Color prevColor_InvertedBackground = _editor._colorOption_InvertedBackground;//추가 21.10.6

			Color prevColor_MeshEdge = _editor._colorOption_MeshEdge;
			Color prevColor_MeshHiddenEdge = _editor._colorOption_MeshHiddenEdge;
			Color prevColor_Outline = _editor._colorOption_Outline;
			Color prevColor_TFBorder = _editor._colorOption_TransformBorder;
			Color prevColor_VertNotSelected = _editor._colorOption_VertColor_NotSelected;
			Color prevColor_VertSelected = _editor._colorOption_VertColor_Selected;

			Color prevColor_GizmoFFDLine = _editor._colorOption_GizmoFFDLine;
			Color prevColor_GizmoFFDInnerLine = _editor._colorOption_GizmoFFDInnerLine;

			//Color prevColor_ToneColor = _editor._colorOption_OnionToneColor;//<<이거 빠집니더


			bool prevBackup_IsAutoSave = _editor._backupOption_IsAutoSave;
			string prevBackup_Path = _editor._backupOption_BaseFolderName;
			int prevBackup_Time = _editor._backupOption_Minute;

			int prevVertGUIOption_SizeRatio_Index = _editor._vertGUIOption_SizeRatio_Index;

			apEditor.BONE_DISPLAY_METHOD prevBoneGUIOption_RenderType = _editor._boneGUIOption_RenderType;
			int prevBoneGUIOption_SizeRatio_Index = _editor._boneGUIOption_SizeRatio_Index;
			bool prevBoneGUIOption_ScaledByZoom = _editor._boneGUIOption_ScaledByZoom;
			apEditor.NEW_BONE_COLOR prevBoneGUIOption_NewBoneColor = _editor._boneGUIOption_NewBoneColor;
			apEditor.NEW_BONE_PREVIEW prevBoneGUIOption_NewBonePreview = _editor._boneGUIOption_NewBonePreview;

			int prevRigGUIOption_VertRatio_Index = _editor._rigGUIOption_VertRatio_Index;
			bool prevRigGUIOption_ScaledByZoom = _editor._rigGUIOption_ScaledByZoom;
			int prevRigGUIOption_VertRatio_Selected_Index = _editor._rigGUIOption_VertRatio_Selected_Index;
			apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE prevRigGUIOption_SelectedWeightType = _editor._rigGUIOption_SelectedWeightGUIType;
			apEditor.NOLINKED_BONE_VISIBILITY prevRigGUIOption_NoLinkedBoneVisibility = _editor._rigGUIOption_NoLinkedBoneVisibility;
			apEditor.RIG_WEIGHT_GRADIENT_COLOR prevRigGUIOption_WeightGradientColor = _editor._rigGUIOption_WeightGradientColor;

			string prevBonePose_Path = _editor._bonePose_BaseFolderName;

			apEditor.CONTROL_PARAM_UI_SIZE_OPTION prevControlParamUISizeOption = _editor._controlParamUISizeOption;


			//1. 기본 설정 (언어, FPS, Statistics)
			//"Language"
			int nextLangIndex = Layout_Popup(TEXT.DLG_Setting_Language, prevLangIndex, _strLanguageName, defLangIndex);
			_editor._language = (apEditor.LANGUAGE)_validLanguageIndex[nextLangIndex];

			GUILayout.Space(5);
			_editor._guiOption_isFPSVisible = Layout_Toggle(TEXT.DLG_Setting_ShowFPS, _editor._guiOption_isFPSVisible, apEditor.DefaultGUIOption_ShowFPS);//"Show FPS"
			_editor._guiOption_isStatisticsVisible = Layout_Toggle(TEXT.DLG_Setting_ShowStatistics, _editor._guiOption_isStatisticsVisible, apEditor.DefaultGUIOption_ShowStatistics);// "Show Statistics"

			GUILayout.Space(5);
			_editor._controlParamUISizeOption = (apEditor.CONTROL_PARAM_UI_SIZE_OPTION)Layout_EnumPopup(TEXT.Setting_SizeOfControlParameterUI, _editor._controlParamUISizeOption, apEditor.DefaultControlParamUISizeOption);

			//TODO : C++ Plugin 사용 여부 설정 UI 만들기 (토글대신 드롭다운, 경고문, Validation)
			GUILayout.Space(5);
			int iCPPPluginMode = Layout_Popup(TEXT.Setting_UpdateMode, _editor._cppPluginOption_UsePlugin ? 0 : 1, _editorUpdateModeLabel, apEditor.DefaultEditorOption_UsePlugin ? 0 : 1);
			_editor._cppPluginOption_UsePlugin = (iCPPPluginMode == 0);

			//만약 플러그인 사용 안함 > 사용함인 경으 플러그인 호환성을 다시 테스트한다.
			if (_editor._cppPluginOption_UsePlugin
				&& (!prevUseCPPDLL || _editor._cppPluginValidateResult == apPluginUtil.VALIDATE_RESULT.Unknown))
			{
				_editor._cppPluginValidateResult = apPluginUtil.I.ValidateDLL();
			}
			else if (prevUseCPPDLL && !_editor._cppPluginOption_UsePlugin)
			{
				//만약 플러그인 비활성화했다면 설치 요청을 해제한다.
				apPluginUtil.I.ReleaseAllInstallRequests();
			}


			//CPP Plugin을 사용할때 에러가 발생하는지 체크
			if ((_editor._cppPluginOption_UsePlugin && _editor._cppPluginValidateResult != apPluginUtil.VALIDATE_RESULT.Valid))
			{
				Color prevGUIColor = GUI.color;
				GUI.color = new Color(prevGUIColor.r * 1.2f, prevGUIColor.g * 0.7f, prevGUIColor.b * 0.7f, 1.0f);

				TEXT warningMsg = TEXT.Setting_WarningAccPlugin_NotInstalled;
				int height_warningBox = 30;//짧은 경우
				switch (_editor._cppPluginValidateResult)
				{
					case apPluginUtil.VALIDATE_RESULT.Unknown:
					case apPluginUtil.VALIDATE_RESULT.NotSupported:
					case apPluginUtil.VALIDATE_RESULT.InstalledButInvalid:
						warningMsg = TEXT.Setting_WarningAccPlugin_NotSupported;
						break;

					case apPluginUtil.VALIDATE_RESULT.NotInstalled:
						warningMsg = TEXT.Setting_WarningAccPlugin_NotInstalled;
						//height_warningBox = 50;//이건 줄이 길다
						break;

					case apPluginUtil.VALIDATE_RESULT.InstallationRequested:
						warningMsg = TEXT.Setting_WarningAccPlugin_InstallReserved;
						break;

					case apPluginUtil.VALIDATE_RESULT.InstalledButOldVersion:
						warningMsg = TEXT.Setting_WarningAccPlugin_PrevVersion;
						//height_warningBox = 50;//이건 줄이 길다
						break;
				}
				GUILayout.Box(_editor.GetText(warningMsg), _guiStyle_Box, GUILayout.Width(_width), GUILayout.Height(height_warningBox));
				GUI.color = prevGUIColor;

				//설치 버튼
				if (_editor._cppPluginValidateResult == apPluginUtil.VALIDATE_RESULT.NotInstalled
					|| _editor._cppPluginValidateResult == apPluginUtil.VALIDATE_RESULT.InstalledButOldVersion)
				{
					//설치가 안되었거나 이전 버전이라면
					//지금 설치하기
					if (GUILayout.Button(_editor.GetText(TEXT.Setting_AccPlugin_Install), GUILayout.Height(20)))
					{
						bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_AccPluginInstall_Title),
										_editor.GetText(TEXT.DLG_AccPluginInstall_Body),
										_editor.GetText(TEXT.Okay),
										_editor.GetText(TEXT.Cancel));

						if (isResult)
						{
							//설치 요청 + 다시 Validate
							apPluginUtil.I.RequestInstall();
							_editor._cppPluginValidateResult = apPluginUtil.I.ValidateDLL();
						}
					}

				}
			}



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			//2. 자동 백업 설정

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_AutoBackupSetting));//"Auto Backup Option"
			GUILayout.Space(10);

			_editor._backupOption_IsAutoSave = Layout_Toggle(TEXT.DLG_Setting_AutoBackup, _editor._backupOption_IsAutoSave, apEditor.DefaultBackupOption_IsAutoSave);//"Auto Backup"

			if (_editor._backupOption_IsAutoSave)
			{
				//경로와 시간
				//"Time (Min)"
				_editor._backupOption_Minute = Layout_IntField(TEXT.DLG_Setting_BackupTime, _editor._backupOption_Minute, apEditor.DefaultBackupOption_Minute);

				//이전 방식
				
				//변경된 방식 20.3.27
				string nextBackupOptionBaseFolderName = null;
				bool isBackupPathButtonDown = false;

				Layout_TextFieldAndButton(TEXT.DLG_Setting_BackupPath, _editor._backupOption_BaseFolderName, apEditor.DefaultBackupOption_BaseFolderName, TEXT.DLG_Change, 90, out nextBackupOptionBaseFolderName, out isBackupPathButtonDown);
				_editor._backupOption_BaseFolderName = string.IsNullOrEmpty(nextBackupOptionBaseFolderName) ? "" : nextBackupOptionBaseFolderName;
				if (isBackupPathButtonDown)
				{
					string pathResult = EditorUtility.SaveFolderPanel("Set the Backup Folder", _editor._backupOption_BaseFolderName, "");
					if (!string.IsNullOrEmpty(pathResult))
					{
						//Debug.Log("백업 폴더 경로 [" + pathResult + "] - " + Application.dataPath);
						Uri targetUri = new Uri(pathResult);
						Uri baseUri = new Uri(Application.dataPath);

						string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();

						_editor._backupOption_BaseFolderName = apUtil.ConvertEscapeToPlainText(relativePath);//변경 21.7.3 : 이스케이프 문자 삭제

						//Debug.Log("상대 경로 [" + relativePath + "]");
						apEditorUtil.SetEditorDirty();

					}
				}

			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			//3. 포즈 저장 옵션

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_PoseSnapshotSetting));//"Pose Snapshot Option"
			GUILayout.Space(10);

			
			string nextBonePoseBaseFolderName = null;
			bool isBonePoseFolderChangeButtonDown = false;

			//변경된 방식
			//"Save Path"
			Layout_TextFieldAndButton(TEXT.DLG_Setting_BackupPath, _editor._bonePose_BaseFolderName, apEditor.DefaultBonePoseOption_BaseFolderName, TEXT.DLG_Change, 90, out nextBonePoseBaseFolderName, out isBonePoseFolderChangeButtonDown);
			_editor._bonePose_BaseFolderName = string.IsNullOrEmpty(nextBonePoseBaseFolderName) ? "" : nextBonePoseBaseFolderName;

			if (isBonePoseFolderChangeButtonDown)//"Change"
			{
				string pathResult = EditorUtility.SaveFolderPanel("Set the Pose Folder", _editor._bonePose_BaseFolderName, "");
				if (!string.IsNullOrEmpty(pathResult))
				{
					Uri targetUri = new Uri(pathResult);
					Uri baseUri = new Uri(Application.dataPath);

					string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();

					_editor._bonePose_BaseFolderName = apUtil.ConvertEscapeToPlainText(relativePath);//이스케이프 문자 삭제

					apEditorUtil.SetEditorDirty();

				}
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			//4. 색상 옵션
			try
			{
				//int width_Btn = 65;
				//int width_Color = width - (width_Btn + 8);

				//int height_Color = 18;
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_BackgroundColors));//"Background Colors"
				GUILayout.Space(10);

				//"Background"
				_editor._colorOption_Background = ColorUI(TEXT.DLG_Setting_Background, _editor._colorOption_Background, apEditor.DefaultColor_Background);

				//추가 21.10.6 : 반전된 배경색상
				_editor._colorOption_InvertedBackground = ColorUI(TEXT.DLG_Setting_InvertedBackground, _editor._colorOption_InvertedBackground, apEditor.DefaultColor_InvertedBackground);

				//"Grid Center"
				_editor._colorOption_GridCenter = ColorUI(TEXT.DLG_Setting_GridCenter, _editor._colorOption_GridCenter, apEditor.DefaultColor_GridCenter);

				//"Grid"
				_editor._colorOption_Grid = ColorUI(TEXT.DLG_Setting_Grid, _editor._colorOption_Grid, apEditor.DefaultColor_Grid);

				//"Atlas Border"
				_editor._colorOption_AtlasBorder = ColorUI(TEXT.DLG_Setting_AtlasBorder, _editor._colorOption_AtlasBorder, apEditor.DefaultColor_AtlasBorder);


				GUILayout.Space(15);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_MeshGUIColors));//"Mesh GUI Colors"
				GUILayout.Space(10);

				//"Mesh Edge"
				_editor._colorOption_MeshEdge = ColorUI(TEXT.DLG_Setting_MeshEdge, _editor._colorOption_MeshEdge, apEditor.DefaultColor_MeshEdge);

				//"Mesh Hidden Edge"
				_editor._colorOption_MeshHiddenEdge = ColorUI(TEXT.DLG_Setting_MeshHiddenEdge, _editor._colorOption_MeshHiddenEdge, apEditor.DefaultColor_MeshHiddenEdge);

				//"Outline"
				_editor._colorOption_Outline = ColorUI(TEXT.DLG_Setting_Outline, _editor._colorOption_Outline, apEditor.DefaultColor_Outline);

				//"Transform Border"
				_editor._colorOption_TransformBorder = ColorUI(TEXT.DLG_Setting_TransformBorder, _editor._colorOption_TransformBorder, apEditor.DefaultColor_TransformBorder);

				//"Vertex"
				_editor._colorOption_VertColor_NotSelected = ColorUI(TEXT.DLG_Setting_Vertex, _editor._colorOption_VertColor_NotSelected, apEditor.DefaultColor_VertNotSelected);

				//"Selected Vertex"
				_editor._colorOption_VertColor_Selected = ColorUI(TEXT.DLG_Setting_SelectedVertex, _editor._colorOption_VertColor_Selected, apEditor.DefaultColor_VertSelected);


				GUILayout.Space(15);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_GizmoColors));//"Gizmo Colors"
				GUILayout.Space(10);

				//"FFD Line"
				_editor._colorOption_GizmoFFDLine = ColorUI(TEXT.DLG_Setting_FFDLine, _editor._colorOption_GizmoFFDLine, apEditor.DefaultColor_GizmoFFDLine);

				//"FFD Inner Line"
				_editor._colorOption_GizmoFFDInnerLine = ColorUI(TEXT.DLG_Setting_FFDInnerLine, _editor._colorOption_GizmoFFDInnerLine, apEditor.DefaultColor_GizmoFFDInnerLine);

			}
			catch (Exception)
			{

			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			//추가 v1.4.2 : 버텍스의 외형 (크기 등)
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_VertOpt_Appearance));
			GUILayout.Space(10);
			_editor._vertGUIOption_SizeRatio_Index = Layout_Popup(	TEXT.Setting_SizeRatio, _editor._vertGUIOption_SizeRatio_Index, _editor._vertGUISizeNameList, apEditor.DefaultVertGUIOption_SizeRatio_Index);


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			//추가 20.3.20 : 
			//5. 본의 GUI 렌더링 옵션

			EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_BoneOpt_Appearance));//"Appearance of Bones"
			GUILayout.Space(10);

			_editor._boneGUIOption_RenderType = (apEditor.BONE_DISPLAY_METHOD)Layout_Popup(TEXT.Setting_BoneOpt_DisplayMethod, (int)_editor._boneGUIOption_RenderType, _strBoneGUIRenderTypeNames, (int)apEditor.DefaultBoneGUIOption_RenderType);//"Display Method"
			_editor._boneGUIOption_SizeRatio_Index = Layout_Popup(TEXT.Setting_SizeRatio, _editor._boneGUIOption_SizeRatio_Index, _editor._boneRigSizeNameList, apEditor.DefaultBoneGUIOption_SizeRatio_Index);//"Size (%)"
			_editor._boneGUIOption_ScaledByZoom = Layout_Toggle(TEXT.Setting_ScaledByZoom, _editor._boneGUIOption_ScaledByZoom, apEditor.DefaultBoneGUIOption_ScaedByZoom);//"Scaled by the Zoom"
			_editor._boneGUIOption_NewBoneColor = (apEditor.NEW_BONE_COLOR)Layout_EnumPopup(TEXT.Setting_NewBoneColor, _editor._boneGUIOption_NewBoneColor, apEditor.DefaultBoneGUIOption_NewBoneColor);
			_editor._boneGUIOption_NewBonePreview = (apEditor.NEW_BONE_PREVIEW)Layout_EnumPopup(TEXT.Setting_PreviewNewBones, _editor._boneGUIOption_NewBonePreview, apEditor.DefaultBoneGUIOption_NewBonePreview);


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);



			//추가 20.3.20 : 
			//6. 리깅에서의 GUI 렌더링 옵션

			EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_RigOpt));//"Appearance of Vertices during Rigging"
			GUILayout.Space(10);

			_editor._rigGUIOption_VertRatio_Index = Layout_Popup(TEXT.Setting_RigOpt_SizeCirVert, _editor._rigGUIOption_VertRatio_Index, _editor._boneRigSizeNameList, apEditor.DefaultRigGUIOption_VertRatio_Index);//"Size (%)"
			_editor._rigGUIOption_VertRatio_Selected_Index = Layout_Popup(TEXT.Setting_RigOpt_SizeSelectedCirVert, _editor._rigGUIOption_VertRatio_Selected_Index, _editor._boneRigSizeNameList, apEditor.DefaultRigGUIOption_VertRatio_Selected_Index);//Size of selected circular vertices
			_editor._rigGUIOption_ScaledByZoom = Layout_Toggle(TEXT.Setting_RigOpt_ScaledCirVertByZoom, _editor._rigGUIOption_ScaledByZoom, apEditor.DefaultRigGUIOption_ScaledByZoom);//"Scaled by the Zoom"
																																													   //_editor._rigGUIOption_SelectedWeightGUIType = (apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE)Layout_EnumMaskPopup(TEXT.Setting_RigOpt_DisplaySelectedWeight, _editor._rigGUIOption_SelectedWeightGUIType, (int)_editor._rigGUIOption_SelectedWeightGUIType, (int)apEditor.DefaultRigGUIOption_SelectedWeightGUIType);
			_editor._rigGUIOption_SelectedWeightGUIType = (apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE)Layout_EnumPopup(TEXT.Setting_RigOpt_DisplaySelectedWeight, _editor._rigGUIOption_SelectedWeightGUIType, apEditor.DefaultRigGUIOption_SelectedWeightGUIType);
			//_editor._rigGUIOption_NoLinkedBoneVisibility = (apEditor.RIG_NOLINKED_BONE_VISIBILITY)Layout_EnumPopup(TEXT.Setting_RigOpt_DisplayNoRiggedBones, _editor._rigGUIOption_NoLinkedBoneVisibility);//>>이 옵션은 리깅 화면으로 이동
			_editor._rigGUIOption_WeightGradientColor = (apEditor.RIG_WEIGHT_GRADIENT_COLOR)Layout_EnumPopup(TEXT.Setting_RigOpt_GradientColor, _editor._rigGUIOption_WeightGradientColor, apEditor.DefaultRigGUIOption_WeightGradientColor);




			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			//추가 21.8.22
			// 유니티 에디터상에서의 옵션 (Hierarchy 아이콘)
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_UnityEditorUI));
			GUILayout.Space(10);

			int iNextHierarchyIcon = Layout_Popup(TEXT.Setting_HierarchyIcon, _iCustomHierarchyIconOption, _strCustomHierarchyIconModes, 1);
			if (iNextHierarchyIcon != _iCustomHierarchyIconOption)
			{
				//이건 AnyPortrait 에디터에 영향을 주지 않으므로 별도로 저장한다.
				_iCustomHierarchyIconOption = iNextHierarchyIcon;
				bool isCustomHierarchyIcon = false;
				bool isIconDrawLeft = false;

				switch (_iCustomHierarchyIconOption)
				{
					case 0:
						isCustomHierarchyIcon = false;
						isIconDrawLeft = true;
						break;

					case 1:
						isCustomHierarchyIcon = true;
						isIconDrawLeft = true;
						break;

					case 2:
					default:
						isCustomHierarchyIcon = true;
						isIconDrawLeft = false;
						break;
				}

				EditorPrefs.SetBool("AnyPortrait_ShowCustomHierarchyIcon", isCustomHierarchyIcon);
				EditorPrefs.SetBool("AnyPortrait_ShowCustomIconLeft", isIconDrawLeft);
				apEditorUtil.SetEditorDirty();

				apCustomHierarchy.CheckEventOptions();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			//7. 단축키 설정창 열기 (추가 20.11.30)
			//TODO 언어
			if (GUILayout.Button(_editor.GetText(TEXT.Setting_ShortcutsSettings), apGUILOFactory.I.Height(25)))//"Customize Shortcuts"
			{
				apDialog_HotkeyMapping.ShowDialog(_editor);
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			//추가 21.2.13 : 모디파이어 편집 옵션 열기
			if (GUILayout.Button(_editor.GetUIWord(UIWORD.GUIMenu_EditModeOptions), apGUILOFactory.I.Height(25)))
			{
				apDialog_ModifierLockSetting.ShowDialog(_editor, _editor._portrait);
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			//8. 고급 옵션들

			//변경 3.22 : 이하는 고급 옵션으로 분리한다.
			//텍스트를 길게 작성할 수 있게 만든다.
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_Advanced));
			GUILayout.Space(10);


			//int labelWidth = _width - (30 + 20);
			//int toggleWidth = 20;

			bool prevStartupScreen = _editor._startScreenOption_IsShowStartup;
			_editor._startScreenOption_IsShowStartup = Layout_Toggle_AdvOpt(TEXT.DLG_Setting_ShowStartPageOn, _editor._startScreenOption_IsShowStartup, apEditor.DefaultStartScreenOption_IsShowStartup);

			//GUILayout.Space(10);
			bool prevCheckVersion = _editor._isCheckLiveVersion_Option;
			_editor._isCheckLiveVersion_Option = Layout_Toggle_AdvOpt(TEXT.CheckLatestVersionOption, _editor._isCheckLiveVersion_Option, apEditor.DefaultCheckLiverVersionOption);

			//추가 3.1 : 유휴 상태에서는 업데이트 빈도를 낮춤
			bool prevLowCPUOption = _editor._isLowCPUOption;
			_editor._isLowCPUOption = Layout_Toggle_AdvOpt(TEXT.DLG_Setting_LowCPU, _editor._isLowCPUOption, apEditor.DefaultLowCPUOption);

			//추가 3.29 : Ambient 자동으로 보정하기 기능
			bool prevAmbientCorrection = _editor._isAmbientCorrectionOption;

			_editor._isAmbientCorrectionOption = Layout_Toggle_AdvOpt(TEXT.DLG_Setting_AmbientColorCorrection, _editor._isAmbientCorrectionOption, apEditor.DefaultAmbientCorrectionOption);

			//추가 19.6.28 : 자동으로 Controller Tab으로 전환할 지 여부 (Mod, Anim)
			bool prevAutoSwitchController_Mod = _editor._isAutoSwitchControllerTab_Mod;
			_editor._isAutoSwitchControllerTab_Mod = Layout_Toggle_AdvOpt(TEXT.Setting_SwitchContTab_Mod, _editor._isAutoSwitchControllerTab_Mod, apEditor.DefaultAutoSwitchControllerTab_Mod);

			bool prevAutoSwitchController_Anim = _editor._isAutoSwitchControllerTab_Anim;
			_editor._isAutoSwitchControllerTab_Anim = Layout_Toggle_AdvOpt(TEXT.Setting_SwitchContTab_Anim, _editor._isAutoSwitchControllerTab_Anim, apEditor.DefaultAutoSwitchControllerTab_Anim);

			//추가 19.6.28 : 작업 종료시 메시의 작업용 보이기/숨기기를 초기화 할 지 여부
			bool prevTempMeshVisibility = _editor._isRestoreTempMeshVisibilityWhenTaskEnded;
			_editor._isRestoreTempMeshVisibilityWhenTaskEnded = Layout_Toggle_AdvOpt(TEXT.Setting_TempVisibilityMesh, _editor._isRestoreTempMeshVisibilityWhenTaskEnded, apEditor.DefaultRestoreTempMeshVisibiilityWhenTaskEnded);

			//추가 20.7.6 : PSD 파일로부터 연 경우, 해당 메시를 선택할때 버텍스 초기화를 할 지 물어본다.
			bool prevRemoveVertsIfImportedFromPSD = _editor._isNeedToAskRemoveVertByPSDImport;
			_editor._isNeedToAskRemoveVertByPSDImport = Layout_Toggle_AdvOpt(TEXT.Setting_AskRemoveVerticesImportedFromPSD, _editor._isNeedToAskRemoveVertByPSDImport, apEditor.DefaultNeedToAskRemoveVertByPSDImport);



			//추가 19.8.13 : 리깅 관련 옵션 > 다른 변수로 변경 (20.3.26)
			#region [미사용 코드]
			//bool prevRigOpt_ColorLikeParent = _editor._rigOption_NewChildBoneColorIsLikeParent;
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//GUILayout.Space(5);
			//_editor._rigOption_NewChildBoneColorIsLikeParent = EditorGUILayout.Toggle(_editor._rigOption_NewChildBoneColorIsLikeParent, GUILayout.Width(toggleWidth));
			//GUILayout.Space(5);
			//EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_RigOpt_ColorLikeParent), guiStyle_WrapLabel, GUILayout.Width(labelWidth));
			//EditorGUILayout.EndHorizontal(); 
			#endregion


			//추가 21.1.20 : View 버튼들이 기본적으로 없어지는데, 이걸 다시 보이게 만들 수 있다.
			bool prevShowViewBtns = _editor._option_ShowPrevViewMenuBtns;
			_editor._option_ShowPrevViewMenuBtns = Layout_Toggle_AdvOpt(TEXT.Setting_ShowPrevViewMenuBtns, _editor._option_ShowPrevViewMenuBtns, apEditor.DefaultShowPrevViewMenuBtns);

			//추가 21.3.6 : 이미지가 한개인 경우 메시 생성시 자동으로 이미지 할당하기
			bool prevSetAutoImageToMesh = _editor._option_SetAutoImageToMeshIfOnlyOneImageExist;
			_editor._option_SetAutoImageToMeshIfOnlyOneImageExist = Layout_Toggle_AdvOpt(TEXT.Setting_AutoImageSetToMeshCreation, _editor._option_SetAutoImageToMeshIfOnlyOneImageExist, apEditor.DefaultSetAutoImageToMeshIfOnlyOneImageExist);

			//추가 21.3.7 : 애니메이션의 AutoKey를 항상 초기화하는지 여부
			bool prevIsTurnOffAnimAutoKey = _editor._option_IsTurnOffAnimAutoKey;
			_editor._option_IsTurnOffAnimAutoKey = Layout_Toggle_AdvOpt(TEXT.Setting_InitAutoKeyframeOption, _editor._option_IsTurnOffAnimAutoKey, apEditor.DefaultIsTurnOffAnimAutoKey);


#if UNITY_2020_1_OR_NEWER
			//추가 22.1.7 : Bake시 SRP를 체크한다. (2020부터)
			bool prevIsCheckSRPOption = _editor._option_CheckSRPWhenBake;
			_editor._option_CheckSRPWhenBake = Layout_Toggle_AdvOpt(TEXT.Setting_CheckScriptableRenderPipelineWhenBake, _editor._option_CheckSRPWhenBake, apEditor.DefaultCheckSRPWhenBake);
#endif

			//추가 22.7.13 : 보기 프리셋의 자동 해제 여부
			bool prevIsTurnOffVisibilityPreset = _editor._option_TurnOffVisibilityPresetWhenSelectObject;
			_editor._option_TurnOffVisibilityPresetWhenSelectObject = Layout_Toggle_AdvOpt(TEXT.Setting_TurnOffVisibilityPresetoption, _editor._option_TurnOffVisibilityPresetWhenSelectObject, apEditor.DefaultVisibilityTurnOffOption);


			//추가 22.12.17 [v1.4.2] : 오브젝트 선택시 자동 스크롤
			bool prevIsAutoScrollWhenObjSelect = _editor._option_AutoScrollWhenObjectSelected;
			_editor._option_AutoScrollWhenObjectSelected = Layout_Toggle_AdvOpt(TEXT.Setting_AutoScrollListObjSelected, _editor._option_AutoScrollWhenObjectSelected, apEditor.DefaultAutoScrollListWhenObjSelected);

			//추가 23.1.9 [v1.4.2] : 객체 클릭 직후 드래그로 기즈모를 통한 편집을 막기
			bool prevIsObjMovableWithoutClickGizmo = _editor._option_ObjMovableWithoutClickGizmo;
			_editor._option_ObjMovableWithoutClickGizmo = Layout_Toggle_AdvOpt(TEXT.Setting_ObjectMovableWithoutClickingGizmoUI, _editor._option_ObjMovableWithoutClickGizmo, apEditor.DefaultObjMovableWithoutClickGizmo);

			GUILayout.Space(10);

			//선택 잠금에 대해서 > 이거를 다른 메뉴로 치환한다. 21.2.13 > 그냥 여기도 두자. 두군데 있으면 좋지 뭐
			EditorGUILayout.BeginHorizontal(GUILayout.Width(_width));
			GUILayout.Space(34);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_EnableSelectionLockEditMode), _guiStyle_WrapLabel_Default);
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);

			bool prevSelectionEnableOption_RigPhy = _editor._isSelectionLockOption_RiggingPhysics;
			bool prevSelectionEnableOption_Morph = _editor._isSelectionLockOption_Morph;
			bool prevSelectionEnableOption_Transform = _editor._isSelectionLockOption_Transform;
			bool prevSelectionEnableOption_ControlTimeline = _editor._isSelectionLockOption_ControlParamTimeline;
			_editor._isSelectionLockOption_Morph = Layout_Toggle_AdvOpt("- Morph " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_Morph, apEditor.DefaultSelectionLockOption_Morph);
			_editor._isSelectionLockOption_Transform = Layout_Toggle_AdvOpt("- Transform " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_Transform, apEditor.DefaultSelectionLockOption_Transform);
			_editor._isSelectionLockOption_RiggingPhysics = Layout_Toggle_AdvOpt("- Rigging/Physic " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_RiggingPhysics, apEditor.DefaultSelectionLockOption_RiggingPhysics);
			_editor._isSelectionLockOption_ControlParamTimeline = Layout_Toggle_AdvOpt("- Control Parameter " + _editor.GetUIWord(UIWORD.Timeline), _editor._isSelectionLockOption_ControlParamTimeline, apEditor.DefaultSelectionLockOption_ControlParamTimeline);



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);


			// < 코멘트!!! >
			// 에디터 설정을 추가했다면, "파일로 내보내기" / "파일에서 가져오기" 코드도 작성해야한다.


			//8. 기본값으로 복원

			//"Restore Editor Default Setting"
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Setting_RestoreDefaultSetting), GUILayout.Height(20)))
			{
				//추가 20.4.1 : 설정을 복구할 것인지 물어보자
				bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_RestoreEditorSetting_Title),
															_editor.GetText(TEXT.DLG_RestoreEditorSetting_Body),
															_editor.GetText(TEXT.Okay),
															_editor.GetText(TEXT.Cancel));
				if (result)
				{
					_editor.RestoreEditorPref();

					//추가 21.8.22 : 유니티 에디터용 변수는 별도로 초기화
					RestoreUnityEditorOptions();
				}
			}


			if (prevLanguage != _editor._language ||
				prevGUIFPS != _editor._guiOption_isFPSVisible ||
				prevGUIStatistics != _editor._guiOption_isStatisticsVisible ||

				prevControlParamUISizeOption != _editor._controlParamUISizeOption ||

				prevUseCPPDLL != _editor._cppPluginOption_UsePlugin ||
				prevColor_Background != _editor._colorOption_Background ||
				prevColor_InvertedBackground != _editor._colorOption_InvertedBackground || //추가
				prevColor_GridCenter != _editor._colorOption_GridCenter ||
				prevColor_Grid != _editor._colorOption_Grid ||

				prevColor_MeshEdge != _editor._colorOption_MeshEdge ||
				prevColor_MeshHiddenEdge != _editor._colorOption_MeshHiddenEdge ||
				prevColor_Outline != _editor._colorOption_Outline ||
				prevColor_TFBorder != _editor._colorOption_TransformBorder ||
				prevColor_VertNotSelected != _editor._colorOption_VertColor_NotSelected ||
				prevColor_VertSelected != _editor._colorOption_VertColor_Selected ||

				prevColor_GizmoFFDLine != _editor._colorOption_GizmoFFDLine ||
				prevColor_GizmoFFDInnerLine != _editor._colorOption_GizmoFFDInnerLine ||
				//prevColor_ToneColor != _editor._colorOption_OnionToneColor ||
				prevBackup_IsAutoSave != _editor._backupOption_IsAutoSave ||
				!prevBackup_Path.Equals(_editor._backupOption_BaseFolderName) ||
				prevBackup_Time != _editor._backupOption_Minute ||
				!prevBonePose_Path.Equals(_editor._bonePose_BaseFolderName) ||

				prevStartupScreen != _editor._startScreenOption_IsShowStartup ||
				prevCheckVersion != _editor._isCheckLiveVersion_Option ||
				prevLowCPUOption != _editor._isLowCPUOption ||
				prevAmbientCorrection != _editor._isAmbientCorrectionOption ||
				prevAutoSwitchController_Mod != _editor._isAutoSwitchControllerTab_Mod ||
				prevAutoSwitchController_Anim != _editor._isAutoSwitchControllerTab_Anim ||

				prevTempMeshVisibility != _editor._isRestoreTempMeshVisibilityWhenTaskEnded ||
				//prevRigOpt_ColorLikeParent != _editor._rigOption_NewChildBoneColorIsLikeParent ||//>>_editor._boneGUIOption_NewBoneColor으로 변경
				prevRemoveVertsIfImportedFromPSD != _editor._isNeedToAskRemoveVertByPSDImport ||

				prevShowViewBtns != _editor._option_ShowPrevViewMenuBtns ||
				prevSetAutoImageToMesh != _editor._option_SetAutoImageToMeshIfOnlyOneImageExist ||
				prevIsTurnOffAnimAutoKey != _editor._option_IsTurnOffAnimAutoKey ||

				prevSelectionEnableOption_RigPhy != _editor._isSelectionLockOption_RiggingPhysics ||
				prevSelectionEnableOption_Morph != _editor._isSelectionLockOption_Morph ||
				prevSelectionEnableOption_Transform != _editor._isSelectionLockOption_Transform ||
				prevSelectionEnableOption_ControlTimeline != _editor._isSelectionLockOption_ControlParamTimeline ||

				prevVertGUIOption_SizeRatio_Index != _editor._vertGUIOption_SizeRatio_Index ||

				prevBoneGUIOption_RenderType != _editor._boneGUIOption_RenderType ||
				prevBoneGUIOption_SizeRatio_Index != _editor._boneGUIOption_SizeRatio_Index ||
				prevBoneGUIOption_ScaledByZoom != _editor._boneGUIOption_ScaledByZoom ||
				prevBoneGUIOption_NewBoneColor != _editor._boneGUIOption_NewBoneColor ||
				prevBoneGUIOption_NewBonePreview != _editor._boneGUIOption_NewBonePreview ||
				prevRigGUIOption_VertRatio_Index != _editor._rigGUIOption_VertRatio_Index ||
				prevRigGUIOption_ScaledByZoom != _editor._rigGUIOption_ScaledByZoom ||
				prevRigGUIOption_VertRatio_Selected_Index != _editor._rigGUIOption_VertRatio_Selected_Index ||
				prevRigGUIOption_SelectedWeightType != _editor._rigGUIOption_SelectedWeightGUIType ||
				prevRigGUIOption_NoLinkedBoneVisibility != _editor._rigGUIOption_NoLinkedBoneVisibility ||
				prevRigGUIOption_WeightGradientColor != _editor._rigGUIOption_WeightGradientColor ||
				prevIsTurnOffVisibilityPreset != _editor._option_TurnOffVisibilityPresetWhenSelectObject ||
				prevIsAutoScrollWhenObjSelect != _editor._option_AutoScrollWhenObjectSelected ||
				prevIsObjMovableWithoutClickGizmo != _editor._option_ObjMovableWithoutClickGizmo
#if UNITY_2020_1_OR_NEWER
								||
								prevIsCheckSRPOption != _editor._option_CheckSRPWhenBake
#endif

					)
			{
				bool isLanguageChanged = (prevLanguage != _editor._language);

				_editor.SaveEditorPref();
				apEditorUtil.SetEditorDirty();

				//apGL.SetToneColor(_editor._colorOption_OnionToneColor);

				if (isLanguageChanged)
				{

					_editor.SetEditorResourceReloadable();//추가 21.8.3 : 에디터 리소스 중 텍스트 리소스를 다시 로드할 필요가 있다.

					_editor.ResetHierarchyAll();

					//이전
					//_editor.RefreshTimelineLayers(true);
					//_editor.RefreshControllerAndHierarchy();

					//변경 19.5.21
					_editor.RefreshControllerAndHierarchy(true);//<<True를 넣으면 RefreshTimelineLayer 함수가 같이 호출된다.
				}

				//apEditorUtil.ReleaseGUIFocus();
			}

			GUILayout.Space(5);

			//v1.4.2 : 외부로 내보내기
			if (GUILayout.Button(_editor.GetText(TEXT.Export), GUILayout.Height(20)))
			{
				string defaultEditorPref = "AnyPortraitEditorSettings";
				string savePath = EditorUtility.SaveFilePanel(	"Save Editor Settings",
																apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.EditorPreferences),
																defaultEditorPref,
																"apref");

				if(!string.IsNullOrEmpty(savePath))
				{
					//이스케이프 문자 삭제
					savePath = apUtil.ConvertEscapeToPlainText(savePath);

					//저장하기
					bool isSaved = SaveEditorSettings(savePath);

					if(isSaved)
					{
						_editor.Notification("Editor Settings are Saved [" + savePath + "]", false, false);
					}
					
					apEditorUtil.SetLastExternalOpenSaveFilePath(savePath, apEditorUtil.SAVED_LAST_FILE_PATH.EditorPreferences);
				}
			}

			//v1.4.2 : 외부에서 가져오기
			if (GUILayout.Button(_editor.GetText(TEXT.Import), GUILayout.Height(20)))
			{
				apEditorUtil.ReleaseGUIFocus();

				string strPath = EditorUtility.OpenFilePanel(	"Load Editor Settings",
																apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.EditorPreferences),
																"apref");

				if (!string.IsNullOrEmpty(strPath))
				{
					//추가 21.7.3 : 이스케이프 문자 삭제
					strPath = apUtil.ConvertEscapeToPlainText(strPath);

					//에디터 설정을 파일에서 가져오기
					bool isLoaded = LoadEditorSettings(strPath);

					if(isLoaded)
					{
						_editor.Notification("Editor Settings are Loaded", false, false);
					}
					
					
					apEditorUtil.SetLastExternalOpenSaveFilePath(strPath, apEditorUtil.SAVED_LAST_FILE_PATH.EditorPreferences);
				}
			}
		}

		// About 탭 UI
		//------------------------------------------------------------------
		private void DrawUI_AboutTab()
		{
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_About));//"About"

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			EditorGUILayout.LabelField("[AnyPortrait]");
			EditorGUILayout.LabelField("Build : " + apVersion.I.APP_VERSION_NUMBER_ONLY);



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			EditorGUILayout.LabelField("[Open Source Library License]");
			GUILayout.Space(20);


			//변경 : TextArea 사용
			if (_guiStyle_Text_About == null)
			{
				_guiStyle_Text_About = new GUIStyle(GUI.skin.label);
				_guiStyle_Text_About.richText = true;
				_guiStyle_Text_About.wordWrap = true;
				_guiStyle_Text_About.alignment = TextAnchor.UpperLeft;
			}

			if (_aboutText_1_PSD == null ||
				_aboutText_2_NGif == null ||
				_aboutText_3_Font == null)
			{
				MakeAboutText();
			}

			EditorGUILayout.TextArea(_aboutText_1_PSD, _guiStyle_Text_About, GUILayout.Width(_width - 25));

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			EditorGUILayout.TextArea(_aboutText_2_NGif, _guiStyle_Text_About, GUILayout.Width(_width - 25));

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(_width);//구분선
			GUILayout.Space(10);

			EditorGUILayout.TextArea(_aboutText_3_Font, _guiStyle_Text_About, GUILayout.Width(_width - 25));
		}







		// 기타 함수들
		//-----------------------------------------------------------------
		private void RestoreUnityEditorOptions()
		{
			EditorPrefs.DeleteKey("AnyPortrait_ShowCustomHierarchyIcon");
			EditorPrefs.DeleteKey("AnyPortrait_ShowCustomIconLeft");
			_iCustomHierarchyIconOption = 1;

			apCustomHierarchy.CheckEventOptions();
		}


		//--------------------------------------------------------------------------------------------------
		// UI 함수 래핑 (레이블 길이때문에..)
		//--------------------------------------------------------------------------------------------------
		private const int LEFT_MARGIN = 5;
		private const int LABEL_WIDTH = 250;
		private const int LAYOUT_HEIGHT = 18;

		private int Layout_Popup(TEXT label, int index, string[] names, int defaultIndex)
		{	
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (index == defaultIndex ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			int result = EditorGUILayout.Popup(index, names);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private bool Layout_Toggle(TEXT label, bool isValue, bool defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (isValue == defaultValue ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			bool result = EditorGUILayout.Toggle(isValue);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private int Layout_IntField(TEXT label, int intValue, int defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (intValue == defaultValue ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			int result = EditorGUILayout.IntField(intValue);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private string Layout_TextField(TEXT label, string strValue, string defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (string.Equals(strValue, defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			string result = EditorGUILayout.TextField(strValue);
			EditorGUILayout.EndHorizontal();

			return result;
		}

		//TextField + Button
		private void Layout_TextFieldAndButton(TEXT label, string strValue, string defaultValue, TEXT buttonName, int buttonWidth, out string strResult, out bool isButtonDown)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (string.Equals(strValue, defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			strResult = EditorGUILayout.TextField(strValue, GUILayout.Width(_width - (LABEL_WIDTH + buttonWidth + 10)));
			isButtonDown = GUILayout.Button(_editor.GetText(buttonName), GUILayout.Width(buttonWidth), GUILayout.Height(LAYOUT_HEIGHT));

			EditorGUILayout.EndHorizontal();
		}



		private Color ColorUI(TEXT label, Color srcColor, Color defaultColor)
		{
			int width_Btn = 65;
			int width_Color = _width - (LABEL_WIDTH + width_Btn + 10);

			bool isDefaultColor =	Mathf.Abs(srcColor.r - defaultColor.r) < 0.005f
								&& Mathf.Abs(srcColor.g - defaultColor.g) < 0.005f
								&& Mathf.Abs(srcColor.b - defaultColor.b) < 0.005f
								&& Mathf.Abs(srcColor.a - defaultColor.a) < 0.005f;


			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			Color result = srcColor;
			try
			{
				EditorGUILayout.LabelField(_editor.GetText(label), (isDefaultColor ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
				result = EditorGUILayout.ColorField(srcColor, GUILayout.Width(width_Color), GUILayout.Height(LAYOUT_HEIGHT));
			}
			catch (Exception)
			{
			}

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Default), GUILayout.Width(width_Btn), GUILayout.Height(LAYOUT_HEIGHT)))//"Default"
			{
				result = defaultColor;
			}
			EditorGUILayout.EndHorizontal();
			return result;
		}

		


		private Enum Layout_EnumPopup(TEXT label, Enum selected, Enum defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (Enum.Equals(selected, defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			Enum result = EditorGUILayout.EnumPopup(selected);
			EditorGUILayout.EndHorizontal();

			return result;
		}


		private Enum Layout_EnumMaskPopup(TEXT label, Enum selected, int intSelected, int defaultValue)
		{	
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), ((intSelected == defaultValue) ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));

#if UNITY_2017_3_OR_NEWER
			Enum result = EditorGUILayout.EnumFlagsField(selected);
#else
			Enum result = EditorGUILayout.EnumMaskPopup("", selected);
#endif
			
			EditorGUILayout.EndHorizontal();

			return result;
		}



		private bool Layout_Toggle_AdvOpt(TEXT label, bool isValue, bool defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(_width));
			GUILayout.Space(LEFT_MARGIN);
			bool result = EditorGUILayout.Toggle(isValue, GUILayout.Width(20));//고급 옵션은 토글이 앞쪽
			EditorGUILayout.LabelField(_editor.GetText(label), (isValue == defaultValue ? _guiStyle_WrapLabel_Default : _guiStyle_WrapLabel_Changed), GUILayout.Width(_width - (50)));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);//여백이 아예 포함됨다.

			return result;
		}

		private bool Layout_Toggle_AdvOpt(string strLabel, bool isValue, bool defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(_width));
			GUILayout.Space(LEFT_MARGIN);
			bool result = EditorGUILayout.Toggle(isValue, GUILayout.Width(20));//고급 옵션은 토글이 앞쪽
			EditorGUILayout.LabelField(strLabel, (isValue == defaultValue ? _guiStyle_WrapLabel_Default : _guiStyle_WrapLabel_Changed), GUILayout.Width(_width - (50)));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);//여백이 아예 포함됨다.

			return result;
		}

		//텍스트 만들기
		//----------------------------------------------------------------------------------------
		private void MakeAboutText()
		{
			_aboutText_1_PSD = null;
			_aboutText_2_NGif = null;
			_aboutText_3_Font = null;

			_aboutText_1_PSD = 
				"[PSD File Import Library]"
				+ "\n\nNtreev Photoshop Document Parser for .Net"
				+ "\n(https://github.com/NtreevSoft/psd-parser)"
				+ "\n\nCopyright (c) 2015 Ntreev Soft co., Ltd."
				+ "\n\nReleased under the MIT License."
				+ "\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:"
				+ "\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software."
				+ "\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";

			_aboutText_2_NGif = 
				"[GIF Export Library]"
				+ "\n\nNGif, Animated GIF Encoder for .NET"
				+ "\n(https://www.codeproject.com/Articles/11505/NGif-Animated-GIF-Encoder-for-NET)"
				+ "\n\nReleased under the CPOL 1.02."
				+ "\n\nThe Code Project Open License (CPOL) is intended to provide developers who choose to share their code with a license that protects them and provides users of their code with a clear statement regarding how the code can be used."
				+ "\n\nThe CPOL is our gift to the community. We encourage everyone to use this license if they wish regardless of whether the code is posted on CodeProject.com."
				+ "\n\n> Detailed License Information : "
				+ "\nhttps://www.codeproject.com/info/cpol10.aspx";

			_aboutText_3_Font = 
				"[Font for Demo Scenes]"
				+ "\n\nSpoqa Han Sans"
				+ "\n(https://spoqa.github.io/spoqa-han-sans/en-US/)"
				+ "\n\nSpoqa Han Sans fonts are open source. All Spoqa Han Sans fonts are published under the SIL Open Font License, Version 1.1."
				+ "\n\nSIL Open Font License (OFL) 1.1"
				+ "\n\n> Detailed License Information : "
				+ "\nhttp://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL";
		}




		
		//------------------------------------------------------------------------
		// 에디터 설정 Export/Import 함수 (데이터 저장 없이 대행)
		//------------------------------------------------------------------------
		private bool SaveEditorSettings(string saveFilePath)
		{
			if(string.IsNullOrEmpty(saveFilePath))
			{
				return false;
			}

			FileStream fs = null;
			StreamWriter sw = null;

			try
			{
				fs = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs);

				//파일 저장 양식
				//키 + : + 값

				//-------------------------------
				// 하나씩 저장하자
				SavePref_Int(sw, "Language", (int)_editor._language);				
				SavePref_Bool(sw, "ShowFPS", _editor._guiOption_isFPSVisible);
				SavePref_Bool(sw, "ShowStatistics", _editor._guiOption_isStatisticsVisible);
				
				SavePref_Int(sw, "ControlParamUISizeOption", (int)_editor._controlParamUISizeOption);
				
				SavePref_Bool(sw, "UseCPPPlugin", _editor._cppPluginOption_UsePlugin);

				SavePref_Bool(sw, "IsAutoBackup", _editor._backupOption_IsAutoSave);
				SavePref_Int(sw, "AutoBackupTime", _editor._backupOption_Minute);
				SavePref_String(sw, "AutoBackupFolderName", _editor._backupOption_BaseFolderName);

				SavePref_String(sw, "PoseFolderName", _editor._bonePose_BaseFolderName);
				
				//색상들
				SavePref_Color(sw, "Color_BG", _editor._colorOption_Background);
				SavePref_Color(sw, "Color_InvertBG", _editor._colorOption_InvertedBackground);
				SavePref_Color(sw, "Color_GridCenter", _editor._colorOption_GridCenter);
				SavePref_Color(sw, "Color_Grid", _editor._colorOption_Grid);
				SavePref_Color(sw, "Color_AtlasBorder", _editor._colorOption_AtlasBorder);
				SavePref_Color(sw, "Color_MeshEdge", _editor._colorOption_MeshEdge);
				SavePref_Color(sw, "Color_MeshHiddenEdge", _editor._colorOption_MeshHiddenEdge);
				SavePref_Color(sw, "Color_Outline", _editor._colorOption_Outline);
				SavePref_Color(sw, "Color_TransformBorder", _editor._colorOption_TransformBorder);
				SavePref_Color(sw, "Color_VertNotSelected", _editor._colorOption_VertColor_NotSelected);
				SavePref_Color(sw, "Color_VertSelected", _editor._colorOption_VertColor_Selected);
				SavePref_Color(sw, "Color_GizmoFFDLine", _editor._colorOption_GizmoFFDLine);
				SavePref_Color(sw, "Color_GizmoFFDInnerLine", _editor._colorOption_GizmoFFDInnerLine);

				//버텍스 렌더링 옵션
				SavePref_Int(sw, "VertSizeRatioIndex", _editor._vertGUIOption_SizeRatio_Index);

				//본 렌더링 옵션
				SavePref_Int(sw, "BoneRenderType", (int)_editor._boneGUIOption_RenderType);
				SavePref_Int(sw, "BoneSizeRatioIndex", _editor._boneGUIOption_SizeRatio_Index);
				SavePref_Bool(sw, "BoneScaledByZoom", _editor._boneGUIOption_ScaledByZoom);
				SavePref_Int(sw, "BoneNewBoneColor", (int)_editor._boneGUIOption_NewBoneColor);
				SavePref_Int(sw, "BoneNewBonePreview", (int)_editor._boneGUIOption_NewBonePreview);

				//리깅 옵션
				SavePref_Int(sw, "RigVertRatioIndex", _editor._rigGUIOption_VertRatio_Index);
				SavePref_Int(sw, "RigVertRatioSelectedIndex", _editor._rigGUIOption_VertRatio_Selected_Index);
				SavePref_Bool(sw, "RigScaledByZoom", _editor._rigGUIOption_ScaledByZoom);
				SavePref_Int(sw, "RigSelectedWeightGUIType", (int)_editor._rigGUIOption_SelectedWeightGUIType);
				SavePref_Int(sw, "RigWeightGradientColor", (int)_editor._rigGUIOption_WeightGradientColor);
				
				//유니티 에디터내의 아이콘 옵션
				//이건 변수가 아닌 레지 값을 받아서 저장하자
				SavePref_Bool(sw, "CustomHierarchyIcon", EditorPrefs.GetBool("AnyPortrait_ShowCustomHierarchyIcon", true));
				SavePref_Bool(sw, "CustomHierarchyIconLeft", EditorPrefs.GetBool("AnyPortrait_ShowCustomIconLeft", true));

				//고급 옵션들
				SavePref_Bool(sw, "StartScreenOnStartUp", _editor._startScreenOption_IsShowStartup);
				SavePref_Bool(sw, "CheckLiveVersion", _editor._isCheckLiveVersion_Option);
				SavePref_Bool(sw, "LowCPUOption", _editor._isLowCPUOption);
				SavePref_Bool(sw, "AmbientCorrection", _editor._isAmbientCorrectionOption);
				SavePref_Bool(sw, "AutoSwitchControllerTab_Mod", _editor._isAutoSwitchControllerTab_Mod);
				SavePref_Bool(sw, "AutoSwitchControllerTab_Anim", _editor._isAutoSwitchControllerTab_Anim);
				SavePref_Bool(sw, "RestoreTempMeshVisibilityWhenTaskEnded", _editor._isRestoreTempMeshVisibilityWhenTaskEnded);
				SavePref_Bool(sw, "AskRemoveVertByPSDImport", _editor._isNeedToAskRemoveVertByPSDImport);
				SavePref_Bool(sw, "ShowPrevViewBtns", _editor._option_ShowPrevViewMenuBtns);
				SavePref_Bool(sw, "SetAutoOneImage", _editor._option_SetAutoImageToMeshIfOnlyOneImageExist);
				SavePref_Bool(sw, "TurnOffAnimAutoKey", _editor._option_IsTurnOffAnimAutoKey);
				SavePref_Bool(sw, "CheckSRPWhenBake", _editor._option_CheckSRPWhenBake);
				SavePref_Bool(sw, "TurnOffVisibPresetWhenSelectObj", _editor._option_TurnOffVisibilityPresetWhenSelectObject);
				SavePref_Bool(sw, "AutoScrollWhenSelect", _editor._option_AutoScrollWhenObjectSelected);
				SavePref_Bool(sw, "ObjMovableWithoutClickGizmo", _editor._option_ObjMovableWithoutClickGizmo);

				//고급-선택잠금 세부 옵션
				SavePref_Bool(sw, "SelectionLock_Morph", _editor._isSelectionLockOption_Morph);
				SavePref_Bool(sw, "SelectionLock_TF", _editor._isSelectionLockOption_Transform);
				SavePref_Bool(sw, "SelectionLock_RiggingPhysics", _editor._isSelectionLockOption_RiggingPhysics);
				SavePref_Bool(sw, "SelectionLock_ControlParamTimeline", _editor._isSelectionLockOption_ControlParamTimeline);

				//모디파이어 설정
				SavePref_Bool(sw, "ExModUpdateByOtherMod", _editor._exModObjOption_UpdateByOtherMod);
				SavePref_Bool(sw, "ExModShowGray", _editor._exModObjOption_ShowGray);
				SavePref_Bool(sw, "ExModNotSelectable", _editor._exModObjOption_NotSelectable);
				SavePref_Bool(sw, "ExModColorPreview", _editor._modLockOption_ColorPreview);
				SavePref_Bool(sw, "ExModBonePreview", _editor._modLockOption_BoneResultPreview);
				SavePref_Color(sw, "ExModBonePreviewColor", _editor._modLockOption_BonePreviewColor);
				SavePref_Bool(sw, "ExModModListUI", _editor._modLockOption_ModListUI);


				//단축키
				List<apHotKeyMapping.HotkeyMapUnit> hotKeyUnits = _editor.HotKeyMap.Units;
				int nHotKeyUnits = hotKeyUnits != null ? hotKeyUnits.Count : 0;
				if(nHotKeyUnits > 0)
				{
					for (int i = 0; i < nHotKeyUnits; i++)
					{
						SavePref_HotKey(sw, hotKeyUnits[i]);
					}
				}

				//-------------------------------


				sw.Flush();
				sw.Close();
				fs.Close();

				sw = null;
				fs = null;

				return true;

			}
			catch(Exception ex)
			{
				if(sw != null)
				{
					sw.Close();
					sw = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}

				Debug.LogError("AnyPortrait : Save Editor Settings Failed : " + ex);

				return false;
			}
		}

		private void SavePref_Bool(StreamWriter sw, string key, bool value)
		{
			sw.WriteLine("K" + key);
			sw.WriteLine("V" + (value ? "TRUE" : "FALSE"));
		}
		private void SavePref_Int(StreamWriter sw, string key, int value)
		{
			sw.WriteLine("K" + key);
			sw.WriteLine("V" + value);
		}
		private void SavePref_Float(StreamWriter sw, string key, float value)
		{
			sw.WriteLine("K" + key);
			sw.WriteLine("V" + (value.ToString()).Replace(',', '.'));//소수점 이슈
		}
		private void SavePref_Color(StreamWriter sw, string key, Color value)
		{
			//소수점을 ,로 적는 문화권에선 .로 저장되도록 만든다.
			string strColorR = (value.r.ToString()).Replace(',', '.');
			string strColorG = (value.g.ToString()).Replace(',', '.');
			string strColorB = (value.b.ToString()).Replace(',', '.');
			string strColorA = (value.a.ToString()).Replace(',', '.');

			sw.WriteLine("K" + key);
			sw.WriteLine("V" + strColorR + ":" + strColorG + ":" + strColorB + ":" + strColorA);
		}
		private void SavePref_String(StreamWriter sw, string key, string value)
		{
			sw.WriteLine("K" + key);
			if(string.IsNullOrEmpty(value))
			{
				sw.WriteLine("V");
			}
			else
			{
				sw.WriteLine("V" + value);
			}
		}
		private void SavePref_HotKey(StreamWriter sw, apHotKeyMapping.HotkeyMapUnit hotkey)
		{
			sw.WriteLine("K" + "HOTKEY_" + hotkey._ID);
			
			string strModKey = "-";
			if(hotkey._isCtrl_Cur) { strModKey += "C"; }
			if(hotkey._isShift_Cur) { strModKey += "S"; }
			if(hotkey._isAlt_Cur) { strModKey += "A"; }
			if(hotkey._isAvailable_Cur) { strModKey += "V"; }

			sw.WriteLine("V" + (int)hotkey._keyCode_Cur + ":" + strModKey);
		}

		public bool LoadEditorSettings(string loadFilePath)
		{
			if(string.IsNullOrEmpty(loadFilePath))
			{
				return false;
			}

			FileStream fs = null;
			StreamReader sr = null;

			try
			{
				fs = new FileStream(loadFilePath, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(fs);


				//데이터는
				//K + 키
				//V + 값 순서로 이루어져 있다.
				//V가 파싱되면 키가 로드되어있는 경우 다같이 파싱한다.

				bool isKeyLoaded = false;
				bool isHotKeyData = false;
				string strKey = null;
				string strHotKeyID = null;
				
				//언어 변경 체크용
				apEditor.LANGUAGE prevLanguage = _editor._language;


				while (true)
				{
					if(sr.Peek() < 0)
					{
						//파일 끝에 도달했다.
						break;
					}

					string strRead = sr.ReadLine();
					if(strRead.Length < 1)
					{
						//너무 짧은 문장
						continue;
					}

					if(strRead.StartsWith("K"))
					{
						//이 문장은 Key 값을 의미한다.
						if(strRead.Length > 1)
						{
							isKeyLoaded = true;							
							strKey = strRead.Substring(1);

							if(strKey.Length > 7 && strKey.StartsWith("HOTKEY_"))
							{
								//이 키는 단축키 데이터다
								isHotKeyData = true;
								strHotKeyID = strKey.Substring(7);//실제 HotKey정보
							}
							else
							{
								isHotKeyData = false;
							}
							
						}
					}
					else if(strRead.StartsWith("V"))
					{
						//이 문장은 Value 값을 의미한다.
						//만약 Key가 없는 상태라면 무시한다.
						if(!isKeyLoaded || string.IsNullOrEmpty(strKey))
						{
							continue;
						}

						string strValue = "";
						
						if(strRead.Length > 1)
						{
							strValue = strRead.Substring(1);
						}

						//키값에 따라 파싱하자
						//단축키 여부에 따라서 코드가 조금 다르다.
						if(!isHotKeyData)
						{
							//단축키가 아닌 일반 키 데이터인 경우
							if(string.Equals(strKey, "Language"))						{ _editor._language = (apEditor.LANGUAGE)LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "ShowFPS"))					{ _editor._guiOption_isFPSVisible = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ShowStatistics"))			{ _editor._guiOption_isStatisticsVisible = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ControlParamUISizeOption"))	{ _editor._controlParamUISizeOption = (apEditor.CONTROL_PARAM_UI_SIZE_OPTION)LoadPref_Int(strValue); }

							else if(string.Equals(strKey, "UseCPPPlugin"))			{ _editor._cppPluginOption_UsePlugin = LoadPref_Bool(strValue); }

							else if(string.Equals(strKey, "IsAutoBackup"))			{ _editor._backupOption_IsAutoSave = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "AutoBackupTime"))		{ _editor._backupOption_Minute = LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "AutoBackupFolderName"))	{ _editor._backupOption_BaseFolderName = strValue; }

							else if(string.Equals(strKey, "PoseFolderName"))		{ _editor._bonePose_BaseFolderName = strValue; }
				
							//색상들
							else if(string.Equals(strKey, "Color_BG"))				{ _editor._colorOption_Background = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_InvertBG"))		{ _editor._colorOption_InvertedBackground = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_GridCenter"))		{ _editor._colorOption_GridCenter = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_Grid"))			{ _editor._colorOption_Grid = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_AtlasBorder"))		{ _editor._colorOption_AtlasBorder = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_MeshEdge"))		{ _editor._colorOption_MeshEdge = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_MeshHiddenEdge"))	{ _editor._colorOption_MeshHiddenEdge = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_Outline"))			{ _editor._colorOption_Outline = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_TransformBorder")) { _editor._colorOption_TransformBorder = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_VertNotSelected")) { _editor._colorOption_VertColor_NotSelected = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_VertSelected"))	{ _editor._colorOption_VertColor_Selected = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_GizmoFFDLine"))	{ _editor._colorOption_GizmoFFDLine = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "Color_GizmoFFDInnerLine")) { _editor._colorOption_GizmoFFDInnerLine = LoadPref_Color(strValue); }

							//버텍스 렌더링 옵션
							else if(string.Equals(strKey, "VertSizeRatioIndex"))		{ _editor._vertGUIOption_SizeRatio_Index = LoadPref_Int(strValue); }

							//본 렌더링 옵션
							else if(string.Equals(strKey, "BoneRenderType"))		{ _editor._boneGUIOption_RenderType = (apEditor.BONE_DISPLAY_METHOD)LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "BoneSizeRatioIndex"))	{ _editor._boneGUIOption_SizeRatio_Index = LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "BoneScaledByZoom"))		{ _editor._boneGUIOption_ScaledByZoom = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "BoneNewBoneColor"))		{ _editor._boneGUIOption_NewBoneColor = (apEditor.NEW_BONE_COLOR)LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "BoneNewBonePreview"))	{ _editor._boneGUIOption_NewBonePreview = (apEditor.NEW_BONE_PREVIEW)LoadPref_Int(strValue); }

							//리깅 옵션
							else if(string.Equals(strKey, "RigVertRatioIndex"))			{ _editor._rigGUIOption_VertRatio_Index = LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "RigVertRatioSelectedIndex")) { _editor._rigGUIOption_VertRatio_Selected_Index = LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "RigScaledByZoom"))			{ _editor._rigGUIOption_ScaledByZoom = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "RigSelectedWeightGUIType"))	{ _editor._rigGUIOption_SelectedWeightGUIType = (apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE)LoadPref_Int(strValue); }
							else if(string.Equals(strKey, "RigWeightGradientColor"))	{ _editor._rigGUIOption_WeightGradientColor = (apEditor.RIG_WEIGHT_GRADIENT_COLOR)LoadPref_Int(strValue); }
				
							//유니티 에디터내의 아이콘 옵션
							//이건 변수가 아닌 레지 값을 받아서 저장하자
							else if(string.Equals(strKey, "CustomHierarchyIcon"))		{ EditorPrefs.SetBool("AnyPortrait_ShowCustomHierarchyIcon", LoadPref_Bool(strValue)); }
							else if(string.Equals(strKey, "CustomHierarchyIconLeft"))	{ EditorPrefs.SetBool("AnyPortrait_ShowCustomIconLeft", LoadPref_Bool(strValue)); }

							//고급 옵션들
							else if(string.Equals(strKey, "StartScreenOnStartUp"))				{ _editor._startScreenOption_IsShowStartup = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "CheckLiveVersion"))					{ _editor._isCheckLiveVersion_Option = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "LowCPUOption"))						{ _editor._isLowCPUOption = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "AmbientCorrection"))					{ _editor._isAmbientCorrectionOption = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "AutoSwitchControllerTab_Mod"))		{ _editor._isAutoSwitchControllerTab_Mod = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "AutoSwitchControllerTab_Anim"))		{ _editor._isAutoSwitchControllerTab_Anim = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "RestoreTempMeshVisibilityWhenTaskEnded"))	{ _editor._isRestoreTempMeshVisibilityWhenTaskEnded = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "AskRemoveVertByPSDImport"))			{ _editor._isNeedToAskRemoveVertByPSDImport = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ShowPrevViewBtns"))					{ _editor._option_ShowPrevViewMenuBtns = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "SetAutoOneImage"))					{ _editor._option_SetAutoImageToMeshIfOnlyOneImageExist = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "TurnOffAnimAutoKey"))				{ _editor._option_IsTurnOffAnimAutoKey = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "CheckSRPWhenBake"))					{ _editor._option_CheckSRPWhenBake = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "TurnOffVisibPresetWhenSelectObj"))	{ _editor._option_TurnOffVisibilityPresetWhenSelectObject = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "AutoScrollWhenSelect"))				{ _editor._option_AutoScrollWhenObjectSelected = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ObjMovableWithoutClickGizmo"))		{ _editor._option_ObjMovableWithoutClickGizmo = LoadPref_Bool(strValue); }

							//고급-선택잠금 세부 옵션
							else if(string.Equals(strKey, "SelectionLock_Morph"))					{ _editor._isSelectionLockOption_Morph = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "SelectionLock_TF"))						{ _editor._isSelectionLockOption_Transform = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "SelectionLock_RiggingPhysics"))			{ _editor._isSelectionLockOption_RiggingPhysics = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "SelectionLock_ControlParamTimeline"))	{ _editor._isSelectionLockOption_ControlParamTimeline = LoadPref_Bool(strValue); }

							//모디파이어 설정
							else if(string.Equals(strKey, "ExModUpdateByOtherMod"))		{ _editor._exModObjOption_UpdateByOtherMod = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ExModShowGray"))				{ _editor._exModObjOption_ShowGray = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ExModNotSelectable"))		{ _editor._exModObjOption_NotSelectable = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ExModColorPreview"))			{ _editor._modLockOption_ColorPreview = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ExModBonePreview"))			{ _editor._modLockOption_BoneResultPreview = LoadPref_Bool(strValue); }
							else if(string.Equals(strKey, "ExModBonePreviewColor"))		{ _editor._modLockOption_BonePreviewColor = LoadPref_Color(strValue); }
							else if(string.Equals(strKey, "ExModModListUI"))			{ _editor._modLockOption_ModListUI = LoadPref_Bool(strValue); }

						}
						else
						{
							//단축키 데이터인 경우. 이때는 strHotKeyID를 strKey 대신 이용하자
							apHotKeyMapping.HotkeyMapUnit hotkeyUnit = _editor.HotKeyMap.GetHotkeyByID(strHotKeyID);
							if(hotkeyUnit != null)
							{
								apHotKeyMapping.EST_KEYCODE keyCode = apHotKeyMapping.EST_KEYCODE.A;
								bool isCtrl = false;
								bool isShift = false;
								bool isAlt = false;
								bool isAvailable = false;

								bool isLoaded = LoadPref_HotKey(	strValue, 
																	out keyCode, out isCtrl, out isShift, out isAlt, out isAvailable);
								if(isLoaded)
								{
									hotkeyUnit.SetCurrentValue(keyCode, isCtrl, isShift, isAlt, isAvailable);
								}
							}
						}
						
						//파싱이 끝났다면, 저장된 키값은 제거
						isKeyLoaded = false;
						strKey = null;
					}
				}
				


				sr.Close();
				fs.Close();

				sr = null;
				fs = null;

				//후처리도 하자
				//1. 에디터 정보를 저장
				_editor.SaveEditorPref();

				//2. 단축키 정보를 저장
				_editor.HotKeyMap.Save();

				//3. Hierarchy 아이콘 변수 갱신
				bool isCustomHierarchy = EditorPrefs.GetBool("AnyPortrait_ShowCustomHierarchyIcon", true);
				bool isIconDrawLeft = EditorPrefs.GetBool("AnyPortrait_ShowCustomIconLeft", true);
				if(!isCustomHierarchy)
				{
					_iCustomHierarchyIconOption = 0;//None
				}
				else
				{
					if(isIconDrawLeft)
					{
						_iCustomHierarchyIconOption = 1;//Left
					}
					else
					{
						_iCustomHierarchyIconOption = 2;//Right
					}
				}

				//4. 언어가 바뀌었다면
				if (prevLanguage != _editor._language)
				{
					//에디터 리소스 중 텍스트 리소스를 다시 로드할 필요가 있다.
					_editor.SetEditorResourceReloadable();

					_editor.ResetHierarchyAll();

					//변경 19.5.21
					_editor.RefreshControllerAndHierarchy(true);//<<True를 넣으면 RefreshTimelineLayer 함수가 같이 호출된다.
				}


				return true;
			}
			catch(Exception ex)
			{
				if(sr != null)
				{
					sr.Close();
					sr = null;
				}

				if(fs != null)
				{
					fs.Close();
					fs = null;
				}

				Debug.LogError("AnyPortrait : Load Editor Settings Failed : " + ex);
				return false;
			}

		}




		private bool LoadPref_Bool(string strValue)
		{
			return strValue.Contains("TRUE");
		}
		private int LoadPref_Int(string strValue)
		{
			return int.Parse(strValue);
		}
		private float LoadPref_Float(string strValue)
		{
			return apUtil.ParseFloat(strValue);//소수점 파싱 (, ,)이슈때문에 별도의 함수 이용
		}
		private Color LoadPref_Color(string strValue)
		{	
			Color result = Color.white;
			if(!string.IsNullOrEmpty(strValue))
			{
				string[] strColors = strValue.Split(new char[] { ':' }, StringSplitOptions.None);
				int nStrColors = strColors != null ? strColors.Length : 0;
				if(nStrColors >= 4)
				{
					result.r = Mathf.Clamp01(apUtil.ParseFloat(strColors[0]));
					result.g = Mathf.Clamp01(apUtil.ParseFloat(strColors[1]));
					result.b = Mathf.Clamp01(apUtil.ParseFloat(strColors[2]));
					result.a = Mathf.Clamp01(apUtil.ParseFloat(strColors[3]));
				}
			}
			return result;
		}
		
		private bool LoadPref_HotKey(	string strValue, 
										out apHotKeyMapping.EST_KEYCODE keyCode, out bool isCtrl, out bool isShift, out bool isAlt, out bool isAvailable)
		{
			if(string.IsNullOrEmpty(strValue))
			{
				//처리 실패
				keyCode = apHotKeyMapping.EST_KEYCODE.A;
				isCtrl = false;
				isShift = false;
				isAlt = false;
				isAvailable = false;

				return false;
			}

			string[] strData = strValue.Split(new char[] {':'}, StringSplitOptions.None);
			if(strData.Length < 2)
			{
				//처리 실패
				keyCode = apHotKeyMapping.EST_KEYCODE.A;
				isCtrl = false;
				isShift = false;
				isAlt = false;
				isAvailable = false;

				return false;
			}

			keyCode = (apHotKeyMapping.EST_KEYCODE)int.Parse(strData[0]);
			string strModKey = strData[1];
			isCtrl = strModKey.Contains("C");
			isShift = strModKey.Contains("S");
			isAlt = strModKey.Contains("A");
			isAvailable = strModKey.Contains("V");

			return true;
		}
		
	}


}