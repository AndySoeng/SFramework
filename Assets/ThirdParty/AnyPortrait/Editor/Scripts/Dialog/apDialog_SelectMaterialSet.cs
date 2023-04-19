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
	public class apDialog_SelectMaterialSet : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_MATERIALSET_RESULT(bool isSuccess, object loadKey, apMaterialSet resultMaterialSet, bool isNoneSelected, object savedObject);

		private static apDialog_SelectMaterialSet s_window = null;
		

		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private object _loadKey = null;
		private FUNC_SELECT_MATERIALSET_RESULT _funcResult = null;

		private bool _isPresetTarget = false;

		private string _msg = "";
		private bool _isNoneSelectable = false;

		private List<apMaterialSet> _materialSets = new List<apMaterialSet>();
		private Vector2 _scrollList = new Vector2();
		private apMaterialSet _curSelectedMatSet = null;
		private object _savedObject = null;
		

		private Dictionary<apMaterialSet.ICON, Texture2D> _img_MatSetType = new Dictionary<apMaterialSet.ICON, Texture2D>();
		private apMaterialSet _noneMatSet = null;


		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(apEditor editor, bool isPresetTarget, string msg, bool isNoneSelectable, FUNC_SELECT_MATERIALSET_RESULT funcResult, object savedObject)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor.MaterialLibrary == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectMaterialSet), true, "Select Material Set", true);
			apDialog_SelectMaterialSet curTool = curWindow as apDialog_SelectMaterialSet;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, isPresetTarget, msg, isNoneSelectable, funcResult, savedObject);

				return loadKey;
			}
			else
			{
				return null;
			}

		}
		private static void CloseDialog()
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
		//--------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, bool isPresetTarget, string msg, bool isNoneSelectable, FUNC_SELECT_MATERIALSET_RESULT funcResult, object savedObject)
		{
			_editor = editor;
			_portrait = _editor._portrait;
			_loadKey = loadKey;
			_isPresetTarget = isPresetTarget;
			_funcResult = funcResult;

			_savedObject = savedObject;

			_msg = msg;
			_isNoneSelectable = isNoneSelectable;

			if(_materialSets == null)
			{
				_materialSets = new List<apMaterialSet>();
			}
			_materialSets.Clear();

			List<apMaterialSet> srcMatSets = null;
			if (isPresetTarget)
			{
				//프리셋이면 MaterialLibrary에서 가져오자
				srcMatSets = _editor.MaterialLibrary.Presets;
			}
			else
			{
				//그렇지 않다면 Portrait에서 가져오자
				srcMatSets = _portrait._materialSets;
			}

			if(_isNoneSelectable)
			{
				//None 타입을 추가하자
				_noneMatSet = new apMaterialSet();
				_noneMatSet._name = "(None)";
				_noneMatSet._icon = apMaterialSet.ICON.Unlit;
				_materialSets.Add(_noneMatSet);
			}

			for (int i = 0; i < srcMatSets.Count; i++)
			{
				_materialSets.Add(srcMatSets[i]);
			}
		
			_curSelectedMatSet = null;

			if (_img_MatSetType == null)
			{
				_img_MatSetType = new Dictionary<apMaterialSet.ICON, Texture2D>();
			}
			_img_MatSetType.Clear();

			_img_MatSetType.Add(apMaterialSet.ICON.Unlit, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Unlit));
			_img_MatSetType.Add(apMaterialSet.ICON.Lit, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Lit));
			_img_MatSetType.Add(apMaterialSet.ICON.LitSpecular, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitSpecular));
			_img_MatSetType.Add(apMaterialSet.ICON.LitSpecularEmission, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitSpecularEmission));
			_img_MatSetType.Add(apMaterialSet.ICON.LitRimlight, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitRim));
			_img_MatSetType.Add(apMaterialSet.ICON.LitRamp, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitRamp));
			_img_MatSetType.Add(apMaterialSet.ICON.Effect, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_FX));
			_img_MatSetType.Add(apMaterialSet.ICON.Cartoon, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Cartoon));
			_img_MatSetType.Add(apMaterialSet.ICON.Custom1, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom1));
			_img_MatSetType.Add(apMaterialSet.ICON.Custom2, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom2));
			_img_MatSetType.Add(apMaterialSet.ICON.Custom3, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom3));
			_img_MatSetType.Add(apMaterialSet.ICON.UnlitVR, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_UnlitVR));
			_img_MatSetType.Add(apMaterialSet.ICON.LitVR, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitVR));
			//추가 22.1.5
			_img_MatSetType.Add(apMaterialSet.ICON.UnlitMergeable, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_MergeableUnlit));
			_img_MatSetType.Add(apMaterialSet.ICON.LitMergeable, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_MergeableLit));

		}

		

		// GUI
		//--------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 45, width, height - 100), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();


			

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			
			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}
			

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			GUILayout.Button(_msg, guiStyle_Center, GUILayout.Width(width), GUILayout.Height(25));//<투명 버튼
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - 100));


			//TODO : 언어
			GUILayout.Button(new GUIContent((_isPresetTarget ? "Presets" : "Material Sets"), iconImageCategory), guiStyle_None, GUILayout.Height(20));//<투명 버튼

			apMaterialSet curMatSet = null;
			//GUILayout.Space(10);
			for (int i = 0; i < _materialSets.Count; i++)
			{
				curMatSet = _materialSets[i];
				GUIStyle curGUIStyle = guiStyle_None;
				if (curMatSet == _curSelectedMatSet)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();


					#region [미사용 코드]
					//prevColor = GUI.backgroundColor;

					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}

					//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
					//GUI.backgroundColor = prevColor; 
					#endregion

					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 20, width - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);

					curGUIStyle = guiStyle_Selected;
				}


				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
				GUILayout.Space(15);
				if (GUILayout.Button(new GUIContent(" " + curMatSet._name, _img_MatSetType[curMatSet._icon]), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
				{
					_curSelectedMatSet = curMatSet;
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Height(30)))//"Select"
			{
				_funcResult(true, _loadKey, _curSelectedMatSet, (_isNoneSelectable && _curSelectedMatSet != null && _curSelectedMatSet == _noneMatSet), _savedObject);
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}

		// Functions
		//--------------------------------------------------------------



		// Get / Set
		//--------------------------------------------------------------
	}
}