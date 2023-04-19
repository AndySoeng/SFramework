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

	public class apDialog_ControlParamPreset : EditorWindow
	{
		// Members
		//----------------------------------------------------------------------------
		public delegate void FUNC_SELECT_CONTROLPARAM_PRESET(bool isSuccess, object loadKey, apControlParamPresetUnit controlParamPresetUnit, apControlParam controlParam);

		private static apDialog_ControlParamPreset s_window = null;



		private apEditor _editor = null;
		private object _loadKey = null;

		private FUNC_SELECT_CONTROLPARAM_PRESET _funcResult;
		private apControlParam _targetControlParam = null;


		private Vector2 _scrollList = new Vector2();
		private string _strAddParamName = "";
		private string _strValueInfo = "";
		private string _strDefaultInfo = "";

		private apControlParamPresetUnit _selectedUnit = null;

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apControlParam targetControlParam, FUNC_SELECT_CONTROLPARAM_PRESET funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ControlParamPreset), true, "Control Parameter Preset", true);
			apDialog_ControlParamPreset curTool = curWindow as apDialog_ControlParamPreset;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetControlParam, funcResult);

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
		//------------------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, apControlParam targetControlParam, FUNC_SELECT_CONTROLPARAM_PRESET funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetControlParam = targetControlParam;

			_strAddParamName = targetControlParam._keyName;
			_selectedUnit = null;

			switch (_targetControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					_strValueInfo = "Int : " + _targetControlParam._int_Min + " ~ " + _targetControlParam._int_Max;
					_strDefaultInfo = _targetControlParam._int_Def.ToString();
					break;

				case apControlParam.TYPE.Float:
					_strValueInfo = "Float : " + _targetControlParam._float_Min + " ~ " + _targetControlParam._float_Max;
					_strDefaultInfo = _targetControlParam._float_Def.ToString();
					break;

				case apControlParam.TYPE.Vector2:
					_strValueInfo = "Vector2 : " + 
						"[ X " + _targetControlParam._vec2_Min.x + " ~ " + _targetControlParam._vec2_Max.x + " , " +
						"[ Y " + _targetControlParam._vec2_Min.y + " ~ " + _targetControlParam._vec2_Max.y + " ]";
					_strDefaultInfo = _targetControlParam._vec2_Def.x + ", " + _targetControlParam._vec2_Def.y;
					break;
			}
		}
		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null || _targetControlParam == null)
			{
				CloseDialog();
				return;
			}

			int height_List = height - 450;
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 156, width, height_List), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_SelectedControlParamSetting));//"Selected Controller Parameter Setting"

			//현재 선택한 Physics Param을 등록하는 UI

			GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
			boxGUIStyle.alignment = TextAnchor.MiddleCenter;
			boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

			GUILayout.Box(_targetControlParam._keyName, boxGUIStyle, GUILayout.Width(width - 8), GUILayout.Height(20));

			GUILayout.Space(5);
			int width_Left = 90;
			int width_Right = width - 110;
			//Icon 이미지 | 이름, ValueType과 값 범위 (수정 불가)
			//            | 저장 버튼

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(70));
			GUILayout.Space(5);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(70));
			//Icon을 출력하자
			Texture2D addParamIcon = _editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(_targetControlParam._iconPreset));
			GUILayout.Box(addParamIcon, boxGUIStyle, GUILayout.Width(65), GUILayout.Height(65));
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(70));
			//이름, Icon 및 등록 버튼

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Range), GUILayout.Width(80));//"Range"
			EditorGUILayout.LabelField(_strValueInfo, GUILayout.Width(width_Right - 88));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Default), GUILayout.Width(80));//"Default"
			EditorGUILayout.LabelField(_strDefaultInfo, GUILayout.Width(width_Right - 88));
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right));
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_RegistToPreset), GUILayout.Width(150)))//"Regist To Preset"
			{
				//bool result = EditorUtility.DisplayDialog("Regist to Preset", "Regist Preset [" + _strAddParamName + "] ?", "Regist", "Cancel");
				bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ControlParamPreset_Regist_Title),
																_editor.GetTextFormat(TEXT.ControlParamPreset_Regist_Body, _strAddParamName),
																_editor.GetText(TEXT.ControlParamPreset_Regist_Okay),
																_editor.GetText(TEXT.Cancel));

				if (result)
				{
					//Control Param으로 추가하자
					_editor.ControlParamPreset.AddNewPreset(_targetControlParam);
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);


			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_NotSelectable = new GUIStyle(GUIStyle.none);
			guiStyle_NotSelectable.normal.textColor = Color.red;
			guiStyle_NotSelectable.alignment = TextAnchor.MiddleLeft;


			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;


			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height_List));

			//"Presets"
			GUILayout.Button(new GUIContent(_editor.GetText(TEXT.DLG_Presets), _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)), guiStyle, GUILayout.Height(20));//<투명 버튼

			for (int i = 0; i < _editor.ControlParamPreset.Presets.Count; i++)
			{
				//DrawBoneUnit(_boneUnits_Root[i], 0, width, iconImage_FoldDown, iconImage_FoldRight, guiContent_Bone, guiStyle, guiStyle_NotSelectable, _scrollList.x);
				DrawPresetUnit(_editor.ControlParamPreset.Presets[i], i, width - 18, _scrollList.x);
			}

			GUILayout.Space(310);

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			//TODO : 선택된 객체 정보 + 삭제(Reserved 아닌 경우) + 적용과 취소
			int width_Info = ((width - 10) / 2) - 10;
			int height_Info = 150;
			int selectedIconSize = 40;

			if (_selectedUnit != null)
			{
				GUILayout.Box("[" + _selectedUnit._keyName + "]", boxGUIStyle, GUILayout.Width(width - 8), GUILayout.Height(30));

				//아이콘, 카테고리, 값 타입
				int rightInfoWidth = width - (selectedIconSize + 20);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(selectedIconSize));
				Texture2D iconImage = _editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(_selectedUnit._iconPreset));
				GUILayout.Space(5);
				GUILayout.Box(iconImage, boxGUIStyle, GUILayout.Width(selectedIconSize), GUILayout.Height(selectedIconSize));

				GUILayout.Space(5);
				EditorGUILayout.BeginVertical(GUILayout.Width(rightInfoWidth), GUILayout.Height(selectedIconSize));

				//카테고리
				//값 타입
				GUILayout.Space(5);
				//"Category"
				EditorGUILayout.EnumPopup(_editor.GetText(TEXT.DLG_Category), _selectedUnit._category, GUILayout.Width(rightInfoWidth));
				//"Value Type"
				EditorGUILayout.EnumPopup(_editor.GetText(TEXT.DLG_ValueType), _selectedUnit._valueType, GUILayout.Width(rightInfoWidth));

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);



				//반반 나눠서 그려주자
				// Def    |  Label
				// Range  |  SnapSize

				int bottomInfoWidth = width_Info - 10;
				string strValueDef = "";
				string strLabel1 = "";
				string strLabel2 = "";

				int optLabelWidth = 50;
				int optValueWidth = bottomInfoWidth - (optLabelWidth + 10);
				int optValue2Width = (bottomInfoWidth - (optLabelWidth + 10)) / 2;

				switch (_selectedUnit._valueType)
				{
					case apControlParam.TYPE.Int:
						strValueDef = _selectedUnit._int_Def.ToString();
						//strLabel1 = "Min";
						//strLabel2 = "Max";
						strLabel1 = _editor.GetText(TEXT.DLG_Min);
						strLabel2 = _editor.GetText(TEXT.DLG_Max);
						break;

					case apControlParam.TYPE.Float:
						strValueDef = _selectedUnit._float_Def.ToString();
						//strLabel1 = "Min";
						//strLabel2 = "Max";
						strLabel1 = _editor.GetText(TEXT.DLG_Min);
						strLabel2 = _editor.GetText(TEXT.DLG_Max);
						break;

					case apControlParam.TYPE.Vector2:
						strValueDef = _selectedUnit._vec2_Def.ToString();
						//strLabel1 = "Axis 1";
						//strLabel2 = "Axis 2";
						strLabel1 = _editor.GetText(TEXT.DLG_Axis1);
						strLabel2 = _editor.GetText(TEXT.DLG_Axis2);
						break;
				}
				GUILayout.Space(5);

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.BeginVertical(GUILayout.Width(width_Info), GUILayout.Height(height_Info));

				//왼쪽 영역
				//기본 값
				//값 범위
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ValueRange), GUILayout.Width(width_Info));//"Value Range"
				GUILayout.Space(5);

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Default), GUILayout.Width(optLabelWidth));//"Default"
				EditorGUILayout.TextField(strValueDef, GUILayout.Width(optValueWidth));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);


				switch (_selectedUnit._valueType)
				{
					case apControlParam.TYPE.Int:
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Min), GUILayout.Width(optLabelWidth));//"Min"
						EditorGUILayout.IntField(_selectedUnit._int_Min, GUILayout.Width(optValueWidth));
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Max), GUILayout.Width(optLabelWidth));//"Max"
						EditorGUILayout.IntField(_selectedUnit._int_Max, GUILayout.Width(optValueWidth));
						EditorGUILayout.EndHorizontal();
						break;

					case apControlParam.TYPE.Float:
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Min), GUILayout.Width(optLabelWidth));//"Min"
						EditorGUILayout.FloatField(_selectedUnit._float_Min, GUILayout.Width(optValueWidth));
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Max), GUILayout.Width(optLabelWidth));//"Max"
						EditorGUILayout.FloatField(_selectedUnit._float_Max, GUILayout.Width(optValueWidth));
						EditorGUILayout.EndHorizontal();
						break;

					case apControlParam.TYPE.Vector2:
						{
							EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomInfoWidth));
							EditorGUILayout.LabelField("", GUILayout.Width(optLabelWidth));
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Min), GUILayout.Width(optValue2Width));//"Min"
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Max), GUILayout.Width(optValue2Width));//"Max"
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomInfoWidth));
							EditorGUILayout.LabelField("X", GUILayout.Width(optLabelWidth));
							EditorGUILayout.FloatField(_selectedUnit._vec2_Min.x, GUILayout.Width(optValue2Width));
							EditorGUILayout.FloatField(_selectedUnit._vec2_Max.x, GUILayout.Width(optValue2Width));
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomInfoWidth));
							EditorGUILayout.LabelField("Y", GUILayout.Width(optLabelWidth));
							EditorGUILayout.FloatField(_selectedUnit._vec2_Min.y, GUILayout.Width(optValue2Width));
							EditorGUILayout.FloatField(_selectedUnit._vec2_Max.y, GUILayout.Width(optValue2Width));
							EditorGUILayout.EndHorizontal();
						}

						break;
				}

				EditorGUILayout.EndVertical();

				GUILayout.Space(4);

				EditorGUILayout.BeginVertical(GUILayout.Width(width_Info), GUILayout.Height(height_Info));

				//오른쪽 영역
				//Label
				//SnapSize
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Label), GUILayout.Width(bottomInfoWidth));//"Label"
				GUILayout.Space(5);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomInfoWidth));
				EditorGUILayout.LabelField(strLabel1, GUILayout.Width(optLabelWidth));
				EditorGUILayout.TextField(_selectedUnit._label_Min, GUILayout.Width(optValueWidth));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(bottomInfoWidth));
				EditorGUILayout.LabelField(strLabel2, GUILayout.Width(optLabelWidth));
				EditorGUILayout.TextField(_selectedUnit._label_Max, GUILayout.Width(optValueWidth));
				EditorGUILayout.EndHorizontal();


				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_SnapSize), GUILayout.Width(bottomInfoWidth));//"Snap Size"
				EditorGUILayout.IntField(_selectedUnit._snapSize, GUILayout.Width(bottomInfoWidth));


				//Reserved가 아니면 삭제가능
				if (!_selectedUnit._isReserved)
				{
					//"Remove Preset"
					if (GUILayout.Button(_editor.GetText(TEXT.DLG_RemovePreset), GUILayout.Height(25)))
					{
						bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ControlParamPreset_Remove_Title),
																	_editor.GetTextFormat(TEXT.ControlParamPreset_Remove_Body, _selectedUnit._keyName),
																	_editor.GetText(TEXT.Remove),
																	_editor.GetText(TEXT.Cancel));

						if (result)
						{
							int targetID = _selectedUnit._uniqueID;
							_selectedUnit = null;

							_editor.ControlParamPreset.RemovePreset(targetID);
						}
					}
				}

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				





			}
			else
			{
				//"No Selected"
				GUILayout.Box(_editor.GetText(TEXT.DLG_NotSelected), boxGUIStyle, GUILayout.Width(width - 8), GUILayout.Height(30));

				GUILayout.Space(height_Info + selectedIconSize + 14);
			}

			bool isClose = false;
			bool isSelectBtnAvailable = _selectedUnit != null;
			EditorGUILayout.BeginHorizontal();
			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Apply), false, isSelectBtnAvailable, (width / 2) - 8, 30))//"Apply"
			{
				_funcResult(true, _loadKey, _selectedUnit, _targetControlParam);
				isClose = true;
			}
			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Close), false, true, (width / 2) - 8, 30))//"Close"
			{
				//_funcResult(false, _loadKey, null, null);
				_funcResult(false, _loadKey, null, _targetControlParam);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}

		private void DrawPresetUnit(apControlParamPresetUnit presetUnit, int index, int width, float scrollX)
		{

			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle.alignment = TextAnchor.MiddleLeft;

			int btnHeight = 32;
			int yOffset = 0;
			if (index == 0)
			{
				yOffset = -13;
			}
			if (presetUnit == _selectedUnit)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				//Color prevColor = GUI.backgroundColor;

				if(EditorGUIUtility.isProSkin)
				{
					//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					guiStyle.normal.textColor = Color.cyan;
				}
				else
				{
					//GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					guiStyle.normal.textColor = Color.white;
				}

				//GUI.Box(new Rect(lastRect.x, lastRect.y + btnHeight + yOffset, width + 4, btnHeight), "");
				//GUI.backgroundColor = prevColor;

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + btnHeight + yOffset, width + 4 - 2, btnHeight, apEditorUtil.UNIT_BG_STYLE.Main);
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 20), GUILayout.Height(btnHeight));
			GUILayout.Space(15);

			string strValueType = "";
			switch (presetUnit._valueType)
			{
				case apControlParam.TYPE.Int:
					strValueType = "  | Int (" + presetUnit._int_Min + " ~ " + presetUnit._int_Max + ")";
					break;

				case apControlParam.TYPE.Float:
					strValueType = "  | Float (" + presetUnit._float_Min + " ~ " + presetUnit._float_Max + ")";
					break;

				case apControlParam.TYPE.Vector2:
					strValueType = "  | Vector2 (" + presetUnit._vec2_Min.x + " ~ " + presetUnit._vec2_Max.x + " , " + presetUnit._vec2_Min.y + " ~ " + presetUnit._vec2_Max.y + ")";
					break;
			}

			if (GUILayout.Button(new GUIContent(
										"  " + presetUnit._keyName + strValueType, _editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(presetUnit._iconPreset))),
									guiStyle, GUILayout.Width(width - 20), GUILayout.Height(btnHeight)))
			{
				_selectedUnit = presetUnit;
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}