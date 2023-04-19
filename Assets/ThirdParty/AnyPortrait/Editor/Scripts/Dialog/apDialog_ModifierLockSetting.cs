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


	//변경 21.2.13 : 모디파이어 잠금 > 모디파이어 설정으로 변경한다.
	//GUIMenu와 Setting Dialog에서 열 수 있다.
	//모디파이어 잠금과는 관련 없고, 바로 옵션을 켜거나 끌 수 있다.
	//단축키(D)는 다른 기능들로 따로 지정될 수 있다.
	//다른곳에 있었던 옵션들이 여기로 추가된다.

	public class apDialog_ModifierLockSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_ModifierLockSetting s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		

		private const int LEFT_MARGIN = 5;
		//private const int LABEL_WIDTH = 250;
		private const int LABEL_WIDTH = 400;
		private const int LAYOUT_HEIGHT = 18;

		private GUIStyle _guiStyle_Label_Default = null;
		private GUIStyle _guiStyle_Label_Changed = null;

		

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



			//EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ModifierLockSetting), true, "Modifier Lock", true);
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ModifierLockSetting), true, "Edit Mode Options", true);
			apDialog_ModifierLockSetting curTool = curWindow as apDialog_ModifierLockSetting;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				//이전
				//int width = 620;
				//int height = 470;

				//변경 21.2.13
				//int width = 400;
				int width = 500;
				int height = 520;

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
			
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			bool isChanged = false;

			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				//Debug.LogError("Exit - Editor / Portrait is Null");
				CloseDialog();
				return;
			}

			width -= 10;

			
			//기본값을 비교하여, 기본값과 다르면 다른 색으로 표시하자.
			if(_guiStyle_Label_Default == null)
			{
				_guiStyle_Label_Default = new GUIStyle(GUI.skin.label);
				_guiStyle_Label_Default.alignment = TextAnchor.UpperLeft;
			}
			if(_guiStyle_Label_Changed == null)
			{
				_guiStyle_Label_Changed = new GUIStyle(GUI.skin.label);
				_guiStyle_Label_Changed.alignment = TextAnchor.UpperLeft;
				if(EditorGUIUtility.isProSkin)
				{
					//어두운 색이면 > 노란색
					_guiStyle_Label_Changed.normal.textColor = Color.yellow;
				}
				else
				{
					//밝은 색이면 진한 보라색
					_guiStyle_Label_Changed.normal.textColor = new Color(1.0f, 0.0f, 0.8f, 1.0f);
				}
			}

			//변경된 UI
			//1. 편집모드가 시작될 때 Selection Lock이 시작되는지 옵션
			//2. 편집 중이 아닌 객체들에 대한 옵션들
			//3. 미리보기 (색상과 Bone) (단축키 보여주기)			
			//4. 복구/닫기
			
			GUILayout.Space(10);

			//1. 편집모드가 시작될 때 Selection Lock이 시작되는지 옵션
			//EditorGUILayout.LabelField("Whether Selection Lock is turned on in Edit Mode");
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting_EnableSelectionLockEditMode));
			GUILayout.Space(5);

			
			bool prevSelectionEnableOption_Morph = _editor._isSelectionLockOption_Morph;
			bool prevSelectionEnableOption_Transform = _editor._isSelectionLockOption_Transform;
			bool prevSelectionEnableOption_RigPhy = _editor._isSelectionLockOption_RiggingPhysics;
			bool prevSelectionEnableOption_ControlTimeline = _editor._isSelectionLockOption_ControlParamTimeline;
			
			_editor._isSelectionLockOption_Morph = Layout_Toggle("- Morph " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_Morph, apEditor.DefaultSelectionLockOption_Morph);
			_editor._isSelectionLockOption_Transform = Layout_Toggle("- Transform " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_Transform, apEditor.DefaultSelectionLockOption_Transform);
			_editor._isSelectionLockOption_RiggingPhysics = Layout_Toggle("- Rigging/Physic " + _editor.GetText(TEXT.DLG_Modifier), _editor._isSelectionLockOption_RiggingPhysics, apEditor.DefaultSelectionLockOption_RiggingPhysics);
			_editor._isSelectionLockOption_ControlParamTimeline = Layout_Toggle("- Control Parameter " + _editor.GetUIWord(UIWORD.Timeline), _editor._isSelectionLockOption_ControlParamTimeline, apEditor.DefaultSelectionLockOption_ControlParamTimeline);

			if(prevSelectionEnableOption_Morph != _editor._isSelectionLockOption_Morph ||
				prevSelectionEnableOption_Transform != _editor._isSelectionLockOption_Transform ||
				prevSelectionEnableOption_RigPhy != _editor._isSelectionLockOption_RiggingPhysics ||
				prevSelectionEnableOption_ControlTimeline != _editor._isSelectionLockOption_ControlParamTimeline)
			{
				//값 저장
				_editor.SaveEditorPref();
				isChanged = true;
				apEditorUtil.SetEditorDirty();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//2. 다른 모디파이어 적용 여부
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_EditModeMultipleModOption_Title));
			GUILayout.Space(5);

			bool prevExModObjOpt_UpdateByOtherMod = _editor._exModObjOption_UpdateByOtherMod;
			_editor._exModObjOption_UpdateByOtherMod = Layout_Toggle(TEXT.Setting_EditModeMultipleModOption_MultipleMod, _editor._exModObjOption_UpdateByOtherMod, apEditor.DefaultExModObjOption_UpdateByOtherMod);

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//3. 편집 중이 아닌 객체들에 대한 옵션들
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Setting_ExModObjOption_Title));
			GUILayout.Space(5);
			
			bool prevExModObjOpt_ShowGray = _editor._exModObjOption_ShowGray;
			bool prevExModObjOpt_NotSelect = _editor._exModObjOption_NotSelectable;
			
			_editor._exModObjOption_ShowGray = Layout_Toggle(TEXT.Setting_ExModObjOption_Gray, _editor._exModObjOption_ShowGray, apEditor.DefaultExModObjOption_ShowGray);
			_editor._exModObjOption_NotSelectable = Layout_Toggle(TEXT.Setting_ExModObjOption_NotSelect, _editor._exModObjOption_NotSelectable, apEditor.DefaultExModObjOption_NotSelectable);

			if(prevExModObjOpt_UpdateByOtherMod != _editor._exModObjOption_UpdateByOtherMod || 
				prevExModObjOpt_ShowGray != _editor._exModObjOption_ShowGray || 
				prevExModObjOpt_NotSelect != _editor._exModObjOption_NotSelectable)
			{
				//값 저장
				_editor.SaveEditorPref();

				//FFD 모드는 취소한다.
				if(_editor.Gizmos.IsFFDMode)
				{
					_editor.Gizmos.RevertFFDTransformForce();
				}

				if(_editor.Select != null)
				{
					//Debug.Log("EXFlag 변경");
					_editor.Select.RefreshMeshGroupExEditingFlags(true);
				}

				isChanged = true;
				apEditorUtil.SetEditorDirty();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			//3. 미리보기 (색상과 Bone) (단축키 보여주기)
			EditorGUILayout.LabelField(_editor.GetText(TEXT.EditModeSetting_PreviewResult));
			GUILayout.Space(5);
			bool prevColorPreview = _editor._modLockOption_ColorPreview;
			bool prevBoneResultPreview = _editor._modLockOption_BoneResultPreview;
			Color prevBonePreviewColor = _editor._modLockOption_BonePreviewColor;
			bool prevModListUI = _editor._modLockOption_ModListUI;

			_editor._modLockOption_ColorPreview = Layout_Toggle(TEXT.DLG_ModLockRenderCalculatedColors, _editor._modLockOption_ColorPreview, false);
			_editor._modLockOption_BoneResultPreview = Layout_Toggle(TEXT.DLG_ModLockPreviewCalculatedBones, _editor._modLockOption_BoneResultPreview, false);
			_editor._modLockOption_BonePreviewColor = Layout_Color(TEXT.DLG_ModLockPreviewColor, _editor._modLockOption_BonePreviewColor, apEditor.DefauleColor_ModLockOpt_BonePreview);

			GUILayout.Space(5);
			_editor._modLockOption_ModListUI = Layout_Toggle(TEXT.DLG_ModLockShowModifierList, _editor._modLockOption_ModListUI, false);


			if(prevColorPreview != _editor._modLockOption_ColorPreview ||
				prevBoneResultPreview != _editor._modLockOption_BoneResultPreview ||
				!IsSameColor(prevBonePreviewColor, _editor._modLockOption_BonePreviewColor) ||
				prevModListUI != _editor._modLockOption_ModListUI)
			{
				//값 저장
				_editor.SaveEditorPref();

				if(_editor.Select != null)
				{
					//Debug.Log("EXFlag 변경");
					_editor.Select.RefreshMeshGroupExEditingFlags(true);
				}

				isChanged = true;
				apEditorUtil.SetEditorDirty();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//4. 복구/닫기
			//"Restore Settings"
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_ModLockRestoreSettings), GUILayout.Height(20)))
			{
				
				_editor._isSelectionLockOption_RiggingPhysics = apEditor.DefaultSelectionLockOption_RiggingPhysics;
				_editor._isSelectionLockOption_Morph = apEditor.DefaultSelectionLockOption_Morph;
				_editor._isSelectionLockOption_Transform = apEditor.DefaultSelectionLockOption_Transform;
				_editor._isSelectionLockOption_ControlParamTimeline = apEditor.DefaultSelectionLockOption_ControlParamTimeline;

				_editor._exModObjOption_UpdateByOtherMod = apEditor.DefaultExModObjOption_UpdateByOtherMod;
				_editor._exModObjOption_ShowGray = apEditor.DefaultExModObjOption_ShowGray;
				_editor._exModObjOption_NotSelectable = apEditor.DefaultExModObjOption_NotSelectable;

				_editor._modLockOption_ColorPreview = false;
				_editor._modLockOption_BoneResultPreview = false;
				_editor._modLockOption_BonePreviewColor = apEditor.DefauleColor_ModLockOpt_BonePreview;
				_editor._modLockOption_ModListUI = false;

				isChanged = true;
				_editor.SaveEditorPref();
			}


			if (isChanged)
			{
				if(_editor.Select != null)
				{
					//삭제
					//_editor.Select.RefreshModifierExclusiveEditing();
					//_editor.Select.RefreshAnimEditingLayerLock();

					//변경 22.5.14
					_editor.Select.AutoRefreshModifierExclusiveEditing();
					
				}
			}

			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(35)))
			{
				_editor.SaveEditorPref();
				CloseDialog();
			}

			#region [미사용 코드]


			////이전 UI
			////int height_top = 30;
			////int height_bottom = 35 + 40;
			////int height_middle = height - (height_top + height_bottom);
			//int height_middle = 370;
			//GUILayout.Space(5);
			//EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ModLockSettings), GUILayout.Height(25));//"Modifier Lock Settings"



			//GUI.Box(new Rect(0, 30, width / 2, 300), "");
			//GUI.Box(new Rect(width / 2 - 1, 30, width / 2 + 1, 300), "");


			//Texture2D img_Lock = _editor.ImageSet.Get(apImageSet.PRESET.Edit_ModLock);
			//Texture2D img_Unlock = _editor.ImageSet.Get(apImageSet.PRESET.Edit_ModUnlock);

			//GUIStyle guiStyle_Title = new GUIStyle(GUI.skin.label);
			//guiStyle_Title.alignment = TextAnchor.MiddleCenter;

			//GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			//guiStyle_Box.alignment = TextAnchor.MiddleCenter;

			//int width_half = (width - 10) / 2;
			//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height_middle));

			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(40));

			//GUILayout.Space(5);
			////Lock Mode / Unlock Mode
			//EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetText(TEXT.DLG_ModLockMode), img_Lock), guiStyle_Title, GUILayout.Width(width_half), GUILayout.Height(40));
			//EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetText(TEXT.DLG_ModUnlockMode), img_Unlock), guiStyle_Title, GUILayout.Width(width_half), GUILayout.Height(40));
			//EditorGUILayout.EndHorizontal();

			//GUILayout.Space(5);

			//Color prevColor = GUI.backgroundColor;

			////1. 설명

			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(60));
			//GUILayout.Space(5);
			//GUI.backgroundColor = new Color(prevColor.r * 1.8f, prevColor.g * 0.7f, prevColor.b * 0.7f, 1.0f);
			////"Other than the selected Modifier\nwill not be executed."
			//GUILayout.Box(_editor.GetText(TEXT.DLG_ModLockDescription), guiStyle_Box, GUILayout.Width(width_half - 5), GUILayout.Height(60));

			//GUILayout.Space(5);

			//GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.8f, prevColor.b * 1.8f, 1.0f);
			////"Except Modifiers which are of the same type\nor can not be edited at the same time,\nothers are executed."
			//GUILayout.Box(_editor.GetText(TEXT.DLG_ModUnlockDescription), guiStyle_Box, GUILayout.Width(width_half - 5), GUILayout.Height(60));

			//GUI.backgroundColor = prevColor;

			//EditorGUILayout.EndHorizontal();

			//GUILayout.Space(5);


			////2. 어떤 객체가 Modifier나 Timeline Layer에 등록 안된 경우, 배타적인 계산을 하지 않는다.
			////Lock On인 경우 없다.
			////"Calculating transformations\nof unregistered objects"
			//string strOpt_CalculateIfNotAddedOther_On = _editor.GetText(TEXT.DLG_ModLockCalculateUnregisteredObj);

			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(40));
			//GUILayout.Space(5 + width_half + 5);
			//if (apEditorUtil.ToggledButton_2Side(strOpt_CalculateIfNotAddedOther_On,
			//									strOpt_CalculateIfNotAddedOther_On,
			//									_editor._modLockOption_CalculateIfNotAddedOther,
			//									true,
			//									width_half - 5, 40))
			//{
			//	_editor._modLockOption_CalculateIfNotAddedOther = !_editor._modLockOption_CalculateIfNotAddedOther;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//EditorGUILayout.EndHorizontal();

			//GUILayout.Space(5);


			////3. Color Preview
			////"Render Calculated Colors"
			//string strOpt_ColorPreview = _editor.GetText(TEXT.DLG_ModLockRenderCalculatedColors);
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			//GUILayout.Space(5);
			//if (apEditorUtil.ToggledButton_2Side(strOpt_ColorPreview, strOpt_ColorPreview,
			//									_editor._modLockOption_ColorPreview_Lock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_ColorPreview_Lock = !_editor._modLockOption_ColorPreview_Lock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//GUILayout.Space(5);

			//if (apEditorUtil.ToggledButton_2Side(strOpt_ColorPreview, strOpt_ColorPreview,
			//									_editor._modLockOption_ColorPreview_Unlock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_ColorPreview_Unlock = !_editor._modLockOption_ColorPreview_Unlock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//EditorGUILayout.EndHorizontal();

			//GUILayout.Space(5);

			////4. Bone Preview
			////"Preview Calculated Bones"
			//string strOpt_BonePreview = _editor.GetText(TEXT.DLG_ModLockPreviewCalculatedBones);
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			//GUILayout.Space(5);
			//if (apEditorUtil.ToggledButton_2Side(strOpt_BonePreview, strOpt_BonePreview,
			//									_editor._modLockOption_BonePreview_Lock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_BonePreview_Lock = !_editor._modLockOption_BonePreview_Lock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//GUILayout.Space(5);

			//if (apEditorUtil.ToggledButton_2Side(strOpt_BonePreview, strOpt_BonePreview,
			//									_editor._modLockOption_BonePreview_Unlock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_BonePreview_Unlock = !_editor._modLockOption_BonePreview_Unlock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//EditorGUILayout.EndHorizontal();

			//GUILayout.Space(5);


			////5. Mesh Preview
			////string strOpt_MeshPreview = "Preview Calculated Meshes";
			////EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			////GUILayout.Space(5);
			////if(apEditorUtil.ToggledButton_2Side(strOpt_MeshPreview, strOpt_MeshPreview,
			////									_editor._modLockOption_MeshPreview_Lock,
			////									true,
			////									width_half - 5, 30))
			////{
			////	_editor._modLockOption_MeshPreview_Lock = !_editor._modLockOption_MeshPreview_Lock;
			////	isChanged = true;
			////	_editor.SaveEditorPref();
			////}

			////GUILayout.Space(5);

			////if(apEditorUtil.ToggledButton_2Side(strOpt_MeshPreview, strOpt_MeshPreview,
			////									_editor._modLockOption_MeshPreview_Unlock,
			////									true,
			////									width_half - 5, 30))
			////{
			////	_editor._modLockOption_MeshPreview_Unlock = !_editor._modLockOption_MeshPreview_Unlock;
			////	isChanged = true;
			////	_editor.SaveEditorPref();
			////}

			////EditorGUILayout.EndHorizontal();

			////GUILayout.Space(5);


			////6. Modifier List UI
			////"Show Modifier List"
			//string strOpt_ModifierListUI = _editor.GetText(TEXT.DLG_ModLockShowModifierList);
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			//GUILayout.Space(5);
			//if (apEditorUtil.ToggledButton_2Side(strOpt_ModifierListUI, strOpt_ModifierListUI,
			//									_editor._modLockOption_ModListUI_Lock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_ModListUI_Lock = !_editor._modLockOption_ModListUI_Lock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//GUILayout.Space(5);

			//if (apEditorUtil.ToggledButton_2Side(strOpt_ModifierListUI, strOpt_ModifierListUI,
			//									_editor._modLockOption_ModListUI_Unlock,
			//									true,
			//									width_half - 5, 30))
			//{
			//	_editor._modLockOption_ModListUI_Unlock = !_editor._modLockOption_ModListUI_Unlock;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}

			//EditorGUILayout.EndHorizontal();

			//GUILayout.Space(15);




			//EditorGUILayout.EndVertical();


			////"Restore Settings"
			//if (GUILayout.Button(_editor.GetText(TEXT.DLG_ModLockRestoreSettings), GUILayout.Height(20)))
			//{
			//	//TODO
			//	_editor._modLockOption_CalculateIfNotAddedOther = false;
			//	_editor._modLockOption_ColorPreview_Lock = false;
			//	_editor._modLockOption_ColorPreview_Unlock = true;//<< True 기본값
			//	_editor._modLockOption_BonePreview_Lock = false;
			//	_editor._modLockOption_BonePreview_Unlock = true;//<< True 기본값
			//													 //_editor._modLockOption_MeshPreview_Lock =		false;
			//													 //_editor._modLockOption_MeshPreview_Unlock =		false;
			//	_editor._modLockOption_ModListUI_Lock = false;
			//	_editor._modLockOption_ModListUI_Unlock = false;

			//	//_editor._modLockOption_MeshPreviewColor = apEditor.DefauleColor_ModLockOpt_MeshPreview;
			//	_editor._modLockOption_BonePreviewColor = apEditor.DefauleColor_ModLockOpt_BonePreview;
			//	isChanged = true;
			//	_editor.SaveEditorPref();
			//}


			//if (isChanged)
			//{
			//	if(_editor.Select != null)
			//	{
			//		_editor.Select.RefreshModifierExclusiveEditing();
			//		_editor.Select.RefreshAnimEditingLayerLock();
			//	}
			//}

			//if(GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(35)))
			//{
			//	_editor.SaveEditorPref();
			//	CloseDialog();
			//} 
			#endregion


		}

		private bool IsSameColor(Color colorA, Color colorB)
		{
			float bias = 0.001f;
			if(Mathf.Abs(colorA.r - colorB.r) < bias &&
				Mathf.Abs(colorA.g - colorB.g) < bias &&
				Mathf.Abs(colorA.b - colorB.b) < bias &&
				Mathf.Abs(colorA.a - colorB.a) < bias)
			{
				return true;
			}
			return false;
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

		private bool Layout_Toggle(string strLabel, bool isValue, bool defaultValue)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(strLabel, (isValue == defaultValue ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));
			bool result = EditorGUILayout.Toggle(isValue);
			EditorGUILayout.EndHorizontal();

			return result;
		}

		private Color Layout_Color(TEXT label, Color colorValue, Color defaultValue)
		{
			bool isColorSame = IsSameColor(colorValue, defaultValue);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(LAYOUT_HEIGHT));
			GUILayout.Space(LEFT_MARGIN);
			EditorGUILayout.LabelField(_editor.GetText(label), (isColorSame ? _guiStyle_Label_Default : _guiStyle_Label_Changed), GUILayout.Width(LABEL_WIDTH));

			Color result = colorValue;

			try
			{
				result = EditorGUILayout.ColorField(colorValue);
			}
			catch (Exception) { }

			EditorGUILayout.EndHorizontal();

			return result;
		}
	}
}