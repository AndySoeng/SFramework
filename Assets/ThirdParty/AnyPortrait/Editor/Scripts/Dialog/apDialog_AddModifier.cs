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

	public class apDialog_AddModifier : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_ADD_MODIFIER_RESULT(bool isSuccess, object loadKey, apModifierBase.MODIFIER_TYPE modifierType, apMeshGroup targetMeshGroup, int validationKey);

		private static apDialog_AddModifier s_window = null;

		private apEditor _editor = null;
		private object _loadKey = null;

		private FUNC_ADD_MODIFIER_RESULT _funcResult;
		private apMeshGroup _targetMeshGroup = null;

		private class ModInfo
		{
			public apModifierBase.MODIFIER_TYPE _modType;
			public string _name = "";
			public int _validationKey = 0;
			public Texture2D _iconImage = null;
			public ModInfo(apModifierBase.MODIFIER_TYPE modType, string name, Texture2D iconImage, int validationKey)
			{
				_modType = modType;
				_name = name;
				_iconImage = iconImage;
				_validationKey = validationKey;
			}
		}
		private List<ModInfo> _modifiers = new List<ModInfo>();
		private ModInfo _curSelectedMod = null;

		private Vector2 _scrollList = new Vector2();

		private apGUIContentWrapper _guiContent_ModifierText = null;
		private apGUIContentWrapper _guiContent_Item = null;

		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apMeshGroup targetMeshGroup, FUNC_ADD_MODIFIER_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_AddModifier), true, "Select Modifier", true);
			apDialog_AddModifier curTool = curWindow as apDialog_AddModifier;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetMeshGroup, funcResult);

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
		public void Init(apEditor editor, object loadKey, apMeshGroup targetMeshGroup, FUNC_ADD_MODIFIER_RESULT funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetMeshGroup = targetMeshGroup;
			_funcResult = funcResult;

			_modifiers.Clear();
			//int nMod = Enum.GetValues(typeof(apModifierBase.MODIFIER_TYPE)).Length;

			//어떤 Modifier를 추가할지 타입을 배열로 정리
			apVersion.I.RequestAddableModifierTypes(OnAddableModifierResult);

			_curSelectedMod = null;
		}

		private void OnAddableModifierResult(int[] modifierTypes, int[] validationKey, string[] names)
		{
			_modifiers.Clear();
			for (int i = 0; i < modifierTypes.Length; i++)
			{
				_modifiers.Add(
					new ModInfo(
									(apModifierBase.MODIFIER_TYPE)modifierTypes[i],
									names[i],
									GetModifierIcon((apModifierBase.MODIFIER_TYPE)modifierTypes[i]),
									validationKey[i]
									)
								);
			}
		}

		// GUI
		//----------------------------------------------------------------------
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
			GUI.Box(new Rect(0, 35, width, height - (90 + 58)), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
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
			//"Select Modifier"
			GUILayout.Button(_editor.GetText(TEXT.DLG_SelectModifier), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (90 + 58)));


			//"Modifiers"
			if(_guiContent_ModifierText == null)
			{
				_guiContent_ModifierText = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_Modifiers), false, iconImageCategory);
			}

			if(_guiContent_Item == null)
			{
				_guiContent_Item = new apGUIContentWrapper();
			}
			
			

			GUILayout.Button(_guiContent_ModifierText.Content, guiStyle_None, GUILayout.Height(20));//<투명 버튼

			//GUILayout.Space(10);
			for (int i = 0; i < _modifiers.Count; i++)
			{
				GUIStyle curGUIStyle = guiStyle_None;
				if (_modifiers[i] == _curSelectedMod)
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

				//이전
				//if (GUILayout.Button(new GUIContent(" " + _modifiers[i]._name, _modifiers[i]._iconImage), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))

				//변경
				_guiContent_Item.ClearText(false);
				_guiContent_Item.AppendSpaceText(1, false);
				_guiContent_Item.AppendText(_modifiers[i]._name, true);

				_guiContent_Item.SetImage(_modifiers[i]._iconImage);
				if (GUILayout.Button(_guiContent_Item.Content, curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
				{
					_curSelectedMod = _modifiers[i];
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(5);

			GUIStyle guiStyle_InfoBox = new GUIStyle(GUI.skin.box);
			guiStyle_InfoBox.alignment = TextAnchor.MiddleCenter;
			guiStyle_InfoBox.normal.textColor = apEditorUtil.BoxTextColor;

			GUILayout.Box(GetModInfo(), guiStyle_InfoBox, GUILayout.Width(width - 6), GUILayout.Height(54));
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal();


			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Height(30)))//"Select"
			{
				if (_curSelectedMod != null)
				{
					//if(_curSelectedMod._modType == apModifierBase.MODIFIER_TYPE.Physic && apVersion.I.IsDemo)
					if(!apVersion.I.IsAddableModifier((int)_curSelectedMod._modType, _curSelectedMod._validationKey))
					{
						//데모 버전 + 물리 Modifier는 데모 버전에서 선택할 수 없다.
						EditorUtility.DisplayDialog(
							_editor.GetText(TEXT.DemoLimitation_Title),
							_editor.GetText(TEXT.DemoLimitation_Body),
							_editor.GetText(TEXT.Okay));
					}
					else
					{
						_funcResult(true, _loadKey, _curSelectedMod._modType, _targetMeshGroup, _curSelectedMod._validationKey);
						isClose = true;
					}
				}
				else
				{
					_funcResult(false, _loadKey, apModifierBase.MODIFIER_TYPE.Base, _targetMeshGroup, -1);
					isClose = true;
				}
				
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, apModifierBase.MODIFIER_TYPE.Base, _targetMeshGroup, -1);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}

		private string GetModInfo()
		{
			if (_curSelectedMod == null)
			{
				return _editor.GetText(TEXT.DLG_SelectModifier);//"Select Modifier"
			}
			if(!apVersion.I.IsAddableModifier((int)_curSelectedMod._modType, _curSelectedMod._validationKey))
			{
				return _editor.GetText(TEXT.DLG_ModInfo_NotSelectableInDemo);//"It is not selectable\nin the [Demo version]";
			}
			switch (_curSelectedMod._modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return "Unhandled type";
				case apModifierBase.MODIFIER_TYPE.Volume:
					return "Changing shape automatically\nwhen you enter the [Volume]";
				case apModifierBase.MODIFIER_TYPE.Morph:
					return _editor.GetText(TEXT.DLG_ModInfo_Morph);//"Changing shape freely\naccording to [Controller]";
				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return _editor.GetText(TEXT.DLG_ModInfo_AnimatedMorph);//"Changing shape freely\naccording to [Animation]";
				case apModifierBase.MODIFIER_TYPE.Rigging:
					return _editor.GetText(TEXT.DLG_ModInfo_Rigging);//"Adding [Bones]\nto make joint animation";
				case apModifierBase.MODIFIER_TYPE.Physic:
					return _editor.GetText(TEXT.DLG_ModInfo_Physic);//"Entering [Physical Information]\nto give inertia";
					
				case apModifierBase.MODIFIER_TYPE.TF:
					return _editor.GetText(TEXT.DLG_ModInfo_TF);//"Changing shape\nwith basic transformation\naccording to [Controller]";
				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return _editor.GetText(TEXT.DLG_ModInfo_AnimatedTF);//"Changing shape\nwith basic transformation\naccording to [Animation]";
				case apModifierBase.MODIFIER_TYPE.FFD:
					return "Changing shape\nwith free-form deformation\naccording to [Controller]";
				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return "Changing shape\nwith free-form deformation\naccording to [Animation]";

				//추가 21.7.20
				case apModifierBase.MODIFIER_TYPE.ColorOnly:					
					return _editor.GetText(TEXT.DLG_ModInfo_ColorOnly);
				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:					
					return _editor.GetText(TEXT.DLG_ModInfo_AnimatedColorOnly);

			}
			return "?? Unknown Type : " + _curSelectedMod._modType;
		}

		private Texture2D GetModifierIcon(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Volume:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_Volume);

				case apModifierBase.MODIFIER_TYPE.Morph:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_Morph);

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_AnimatedMorph);

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);

				case apModifierBase.MODIFIER_TYPE.Physic:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_Physic);

				case apModifierBase.MODIFIER_TYPE.TF:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_TF);

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_AnimatedTF);

				case apModifierBase.MODIFIER_TYPE.FFD:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_FFD);

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_AnimatedFFD);

				//추가 21.7.20 : 색상 모디파이어
				case apModifierBase.MODIFIER_TYPE.ColorOnly:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorOnly);

				case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
					return _editor.ImageSet.Get(apImageSet.PRESET.Modifier_AnimatedColorOnly);

				default:
					Debug.LogError("TODO : 알 수 없는 Modifier [" + modType + "]");
					break;
			}
			return null;
		}
		
	}

}