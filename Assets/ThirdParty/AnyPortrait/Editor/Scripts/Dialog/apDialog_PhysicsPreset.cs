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

	public class apDialog_PhysicsPreset : EditorWindow
	{
		// Members
		//----------------------------------------------------------------------------
		public delegate void FUNC_SELECT_PHYSICS_PRESET(bool isSuccess, object loadKey, apPhysicsPresetUnit physicsUnit, apModifiedMesh targetModMesh);

		private static apDialog_PhysicsPreset s_window = null;



		private apEditor _editor = null;
		private object _loadKey = null;

		private FUNC_SELECT_PHYSICS_PRESET _funcResult;
		private apModifiedMesh _targetModMesh = null;


		private Vector2 _scrollList = new Vector2();
		private string _strAddParamName = "";
		private apPhysicsPresetUnit.ICON _addParamIcon = apPhysicsPresetUnit.ICON.Cloth1;

		private apPhysicsPresetUnit _selectedUnit = null;

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apModifiedMesh targetModMesh, FUNC_SELECT_PHYSICS_PRESET funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_PhysicsPreset), true, "Physics Preset", true);
			apDialog_PhysicsPreset curTool = curWindow as apDialog_PhysicsPreset;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetModMesh, funcResult);

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
		public void Init(apEditor editor, object loadKey, apModifiedMesh targetModMesh, FUNC_SELECT_PHYSICS_PRESET funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetModMesh = targetModMesh;

			_strAddParamName = "No Name Preset";
			_addParamIcon = apPhysicsPresetUnit.ICON.Cloth1;
			_selectedUnit = null;
		}


		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			try
			{
				int width = (int)position.width;
				int height = (int)position.height;
				if (_editor == null || _funcResult == null || _targetModMesh == null)
				{
					CloseDialog();
					return;
				}

				Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

				int height_List = height - 480;
				Color prevColor = GUI.backgroundColor;
				GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
				GUI.Box(new Rect(0, 156, width, height_List), "");
				GUI.backgroundColor = prevColor;

				EditorGUILayout.BeginVertical();

				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_SelectedPhysicsSetting));//"Selected Physics Setting"

				//현재 선택한 Physics Param을 등록하는 UI

				GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
				boxGUIStyle.alignment = TextAnchor.MiddleCenter;
				boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box(_targetModMesh._renderUnit.Name, boxGUIStyle, GUILayout.Width(width - 8), GUILayout.Height(20));

				GUILayout.Space(5);
				int width_Left = 90;
				int width_Right = width - 110;

				//Icon 이미지		|	이름	Icon 타입
				//					|	(저장)
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(70));
				GUILayout.Space(5);
				EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(70));
				//Icon을 출력하자
				Texture2D addParamIcon = _editor.ImageSet.Get(apEditorUtil.GetPhysicsPresetIconType(_addParamIcon));
				GUILayout.Box(addParamIcon, boxGUIStyle, GUILayout.Width(65), GUILayout.Height(65));
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(70));
				//이름, Icon 및 등록 버튼

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right));
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Name), GUILayout.Width(80));//"Name"
				_strAddParamName = EditorGUILayout.TextField(_strAddParamName, GUILayout.Width(width_Right - 88));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right));
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Icon), GUILayout.Width(80));//"Icon"
				_addParamIcon = (apPhysicsPresetUnit.ICON)EditorGUILayout.EnumPopup(_addParamIcon, GUILayout.Width(width_Right - 88));
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right));
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_RegistToPreset), GUILayout.Width(150)))//"Regist To Preset"
				{
					//TODO
					if (_targetModMesh.PhysicParam != null)
					{
						//bool result = EditorUtility.DisplayDialog("Regist to Preset", "Regist Preset [" + _strAddParamName + "] ?", "Regist", "Cancel");
						bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.PhysicPreset_Regist_Title),
																	_editor.GetTextFormat(TEXT.PhysicPreset_Regist_Body, _strAddParamName),
																	_editor.GetText(TEXT.PhysicPreset_Regist_Okay),
																	_editor.GetText(TEXT.Cancel));

						if (result)
						{
							_editor.PhysicsPreset.AddNewPreset(_targetModMesh.PhysicParam, _strAddParamName, _addParamIcon);
						}
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

				//GUIContent guiContent_Bone = new GUIContent(iconBone);

				GUILayout.Space(10);
				//GUILayout.Button("Select a Physics Preset", guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼


				_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height_List));

				//"Presets"
				GUILayout.Button(new GUIContent(_editor.GetText(TEXT.DLG_Presets), iconImageCategory), guiStyle, GUILayout.Height(20));//<투명 버튼

				for (int i = 0; i < _editor.PhysicsPreset.Presets.Count; i++)
				{
					//DrawBoneUnit(_boneUnits_Root[i], 0, width, iconImage_FoldDown, iconImage_FoldRight, guiContent_Bone, guiStyle, guiStyle_NotSelectable, _scrollList.x);
					DrawPresetUnit(_editor.PhysicsPreset.Presets[i], i, width - 18, _scrollList.x);
				}

				GUILayout.Space(310);

				EditorGUILayout.EndScrollView();

				EditorGUILayout.EndVertical();

				GUILayout.Space(10);



				//선택한 정보를 보여주자
				//string curName = "<Not Selected>";
				string curName = "<" + _editor.GetText(TEXT.DLG_NotSelected) + ">";
				Texture2D curIcon = null;

				float curMoveRange = 0.0f;
				//float curStretchRange_Min = 0.0f;
				float curStretchRange_Max = 0.0f;
				float curStretchK = 0.0f;
				float curInertiaK = 0.0f;
				float curDamping = 0.0f;
				float curMass = 100.0f;
				bool curIsRestrictMoveRange = false;
				bool curIsRestrictStretchRange = false;


				Vector2 curGravityConstValue = Vector2.zero;
				Vector2 curWindConstValue = Vector2.zero;
				Vector2 curWindRandomRange = Vector2.zero;

				float curAirDrag = 0.0f;
				float curViscosity = 0.0f;
				float curRestoring = 1.0f;

				if (_selectedUnit != null)
				{
					curName = _selectedUnit._name;
					curIcon = _editor.ImageSet.Get(apEditorUtil.GetPhysicsPresetIconType(_selectedUnit._icon));
					//curStretchRange_Min = _selectedUnit._stretchRange_Min;
					curStretchRange_Max = _selectedUnit._stretchRange_Max;

					curIsRestrictMoveRange = _selectedUnit._isRestrictMoveRange;
					curIsRestrictStretchRange = _selectedUnit._isRestrictStretchRange;

					curMoveRange = _selectedUnit._moveRange;
					curStretchK = _selectedUnit._stretchK;
					curInertiaK = _selectedUnit._inertiaK;
					curDamping = _selectedUnit._damping;
					curMass = _selectedUnit._mass;


					curGravityConstValue = _selectedUnit._gravityConstValue;
					curWindConstValue = _selectedUnit._windConstValue;
					curWindRandomRange = _selectedUnit._windRandomRange;

					curAirDrag = _selectedUnit._airDrag;
					curViscosity = _selectedUnit._viscosity;
					curRestoring = _selectedUnit._restoring;
				}

				Texture2D imgIcon_Stretch = _editor.ImageSet.Get(apImageSet.PRESET.Physic_Stretch);
				Texture2D imgIcon_Inertia = _editor.ImageSet.Get(apImageSet.PRESET.Physic_Inertia);
				Texture2D imgIcon_Restoring = _editor.ImageSet.Get(apImageSet.PRESET.Physic_Recover);
				Texture2D imgIcon_Viscosity = _editor.ImageSet.Get(apImageSet.PRESET.Physic_Viscosity);
				Texture2D imgIcon_Gravity = _editor.ImageSet.Get(apImageSet.PRESET.Physic_Gravity);
				Texture2D imgIcon_Wind = _editor.ImageSet.Get(apImageSet.PRESET.Physic_Wind);
				//반반 나눠서 그려주자
				int width_Info = ((width - 10) / 2) - 10;
				int height_Info = 270;
				int width_InfoLabel = 120;
				int width_InfoValue = (width_Info - width_InfoLabel) - 4;

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.BeginVertical(GUILayout.Width(width_Info), GUILayout.Height(height_Info));
				//Icon, Name, Basic Setting, Gravity, Wind를 보여주자
				if (curIcon == null)
				{
					EditorGUILayout.LabelField(curName, GUILayout.Width(width_Info), GUILayout.Height(26));
				}
				else
				{
					EditorGUILayout.LabelField(new GUIContent("  " + curName, curIcon), GUILayout.Width(width_Info), GUILayout.Height(26));
				}
				GUILayout.Space(5);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Mass), GUILayout.Width(width_InfoLabel));//"Mass"
				EditorGUILayout.FloatField(curMass, GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Damping), GUILayout.Width(width_InfoLabel));//"Damping"
				EditorGUILayout.FloatField(curDamping, GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.AirDrag), GUILayout.Width(width_InfoLabel));//"Air Drag"
				EditorGUILayout.FloatField(curAirDrag, GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();


				if (curIsRestrictMoveRange)
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
					EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.MoveRange), GUILayout.Width(width_InfoLabel));//"Move Range"
					EditorGUILayout.FloatField(curMoveRange, GUILayout.Width(width_InfoValue));
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
					EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.MoveRangeUnlimited), GUILayout.Width(width_Info));//"Move Range : (Unlimited)"
					EditorGUILayout.EndHorizontal();
				}




				GUILayout.Space(5);

				//"  Gravity"
				EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetUIWord(UIWORD.Gravity), imgIcon_Gravity), GUILayout.Width(width_Info), GUILayout.Height(25));
				apEditorUtil.DelayedVector2Field(curGravityConstValue, width_Info - 4);

				GUILayout.Space(5);
				//"  Wind"
				EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetUIWord(UIWORD.Wind), imgIcon_Wind), GUILayout.Width(width_Info), GUILayout.Height(25));
				apEditorUtil.DelayedVector2Field(curWindConstValue, width_Info - 4);
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.WindRandomRangeSize), GUILayout.Width(width_Info));//"Wind Random Size"
				apEditorUtil.DelayedVector2Field(curWindRandomRange, width_Info - 4);

				EditorGUILayout.EndVertical();

				GUILayout.Space(4);

				EditorGUILayout.BeginVertical(GUILayout.Width(width_Info), GUILayout.Height(height_Info));
				//Stretchiness, Inertia, Restoring, Viscosity를 보여주고, 삭제 버튼(또는 Reserved) 표시

				//"  Stretchiness"
				EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetUIWord(UIWORD.Stretchiness), imgIcon_Stretch), GUILayout.Width(width_Info), GUILayout.Height(25));
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.K_Value), GUILayout.Width(width_InfoLabel));//"K-Value"
				EditorGUILayout.FloatField(curStretchK, GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				if (curIsRestrictStretchRange)
				{
					//EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
					//EditorGUILayout.LabelField("Shorten Range", GUILayout.Width(width_InfoLabel));
					//EditorGUILayout.FloatField(curStretchRange_Min, GUILayout.Width(width_InfoValue));
					//EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
					EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.LengthenRatio), GUILayout.Width(width_InfoLabel));//"Lengthen Range"
					EditorGUILayout.FloatField(curStretchRange_Max, GUILayout.Width(width_InfoValue));
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					//EditorGUILayout.LabelField("Shorten Range : (Unlimited)", GUILayout.Width(width_Info));
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
					EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.LengthenRatioUnlimited), GUILayout.Width(width_Info));//"Lengthen Range : (Unlimited)"
					EditorGUILayout.EndHorizontal();
				}


				GUILayout.Space(5);

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				//"  Inertia"
				EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetUIWord(UIWORD.Inertia), imgIcon_Inertia), GUILayout.Width(width_InfoLabel), GUILayout.Height(25));
				EditorGUILayout.FloatField(curInertiaK, GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				//"  Restoring"
				EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetUIWord(UIWORD.Restoring), imgIcon_Restoring), GUILayout.Width(width_InfoLabel), GUILayout.Height(25));
				EditorGUILayout.FloatField(curRestoring, GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Info));
				//"  Viscosity"
				EditorGUILayout.LabelField(new GUIContent("  " + _editor.GetUIWord(UIWORD.Viscosity), imgIcon_Viscosity), GUILayout.Width(width_InfoLabel), GUILayout.Height(25));
				EditorGUILayout.FloatField(curViscosity, GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);
				if (_selectedUnit != null)
				{
					//삭제 가능한지 체크
					if (!_selectedUnit._isReserved)
					{
						//"Remove Preset"
						if (GUILayout.Button(_editor.GetText(TEXT.DLG_RemovePreset), GUILayout.Width(width_Info - 4)))
						{
							//bool result = EditorUtility.DisplayDialog("Remove Preset", "Remove Preset [" + _selectedUnit._name + "] ?", "Remove", "Cancel");
							bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.PhysicPreset_Remove_Title),
																		_editor.GetTextFormat(TEXT.PhysicPreset_Remove_Body, _selectedUnit._name),
																		_editor.GetText(TEXT.Remove),
																		_editor.GetText(TEXT.Cancel));

							if (result)
							{
								int targetID = _selectedUnit._uniqueID;
								_selectedUnit = null;

								_editor.PhysicsPreset.RemovePreset(targetID);
							}
						}
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				bool isClose = false;
				bool isSelectBtnAvailable = _selectedUnit != null;
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Apply), false, isSelectBtnAvailable, (width / 2) - 8, 30))//"Apply"
				{
					_funcResult(true, _loadKey, _selectedUnit, _targetModMesh);
					isClose = true;
				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Close), false, true, (width / 2) - 8, 30))//"Close"
				{
					//_funcResult(false, _loadKey, null, null);
					_funcResult(false, _loadKey, null, _targetModMesh);
					isClose = true;
				}
				EditorGUILayout.EndHorizontal();

				if (isClose)
				{
					CloseDialog();
				}
			}
			catch(Exception ex)
			{
				//추가 21.3.17 : Try-Catch 추가. Mac에서 에러가 발생하기 쉽다.
				Debug.LogError("AnyPortrait : Exception occurs : " + ex);
			}
		}

		private void DrawPresetUnit(apPhysicsPresetUnit presetUnit, int index, int width, float scrollX)
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
			if (GUILayout.Button(new GUIContent(
										"  " + presetUnit._name, _editor.ImageSet.Get(apEditorUtil.GetPhysicsPresetIconType(presetUnit._icon))),
									guiStyle, GUILayout.Width(width - 20), GUILayout.Height(btnHeight)))
			{
				_selectedUnit = presetUnit;
			}
			EditorGUILayout.EndHorizontal();
		}
		//private void DrawBoneUnit(BoneUnit boneUnit, int level, int width,
		//							Texture2D imgIcon_FoldDown, Texture2D imgIcon_FoldRight,
		//							GUIContent guiContent_Bone,
		//							GUIStyle guiStyle, GUIStyle guiStyle_NotSelectable,
		//							float scrollX)
		//{
		//	//Search 옵션에 따라 다르다.
		//	bool isRenderable = true;
		//	if (_isSearched)
		//	{
		//		if (boneUnit._bone != null)
		//		{
		//			if (boneUnit._name.Contains(_strSearchKeyword))
		//			{
		//				isRenderable = true;
		//			}
		//			else
		//			{
		//				isRenderable = false;
		//			}
		//		}
		//	}

		//	if (isRenderable)
		//	{
		//		bool isNotSelectable = !boneUnit._isSelectable || boneUnit._isTarget;
		//		if (boneUnit == _selectedBoneUnit)
		//		{
		//			Rect lastRect = GUILayoutUtility.GetLastRect();
		//			Color prevColor = GUI.backgroundColor;

		//			GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);

		//			//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
		//			GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + 20, width, 20), "");
		//			GUI.backgroundColor = prevColor;
		//		}

		//		if (_isSearched)
		//		{
		//			if (boneUnit._parentUnit != null)
		//			{
		//				if (boneUnit._parentUnit._bone != null && !boneUnit._parentUnit._name.Contains(_strSearchKeyword))
		//				{
		//					//Parent Unit이 검색에 포함되지 않는 경우
		//					//realLevel -= boneUnit._parentUnit._parentUnit._level;
		//					level = 0;
		//				}
		//			}
		//		}

		//		//if(_isSearched)
		//		//{
		//		//	if(boneUnit._parentUnit != null)
		//		//	{
		//		//		if(boneUnit._parentUnit._bone != null && !boneUnit._parentUnit._name.Contains(_strSearchKeyword))
		//		//		{
		//		//			//Parent Unit이 검색에 포함되지 않는 경우
		//		//			//realLevel -= boneUnit._parentUnit._parentUnit._level;
		//		//			level = 0;
		//		//		}
		//		//	}
		//		//	EditorGUILayout.BeginHorizontal(GUILayout.Width((width - 50)));
		//		//	GUILayout.Space(15);
		//		//}
		//		//else
		//		//{
		//		//	EditorGUILayout.BeginHorizontal(GUILayout.Width((width - 50) + level * 10));
		//		//	GUILayout.Space(15 + (level * 10));
		//		//}

		//		EditorGUILayout.BeginHorizontal(GUILayout.Width((width - 50) + level * 10));
		//		GUILayout.Space(15 + (level * 10));


		//		//Fold 관련
		//		if (boneUnit._isFoldable)
		//		{
		//			Texture2D foldIcon = imgIcon_FoldDown;
		//			if (boneUnit._isFolded)
		//			{
		//				foldIcon = imgIcon_FoldRight;
		//			}
		//			if (GUILayout.Button(foldIcon, guiStyle, GUILayout.Width(20), GUILayout.Height(20)))
		//			{
		//				boneUnit._isFolded = !boneUnit._isFolded;
		//			}
		//		}
		//		else
		//		{
		//			if (boneUnit._bone != null)
		//			{
		//				EditorGUILayout.LabelField(guiContent_Bone, guiStyle, GUILayout.Width(20), GUILayout.Height(20));
		//			}
		//			else
		//			{
		//				EditorGUILayout.LabelField("", guiStyle, GUILayout.Width(20), GUILayout.Height(20));
		//			}
		//		}

		//		GUIStyle guiStyleLabel = guiStyle;
		//		if (isNotSelectable)
		//		{
		//			guiStyleLabel = guiStyle_NotSelectable;
		//		}
		//		if (GUILayout.Button(boneUnit._name, guiStyleLabel, GUILayout.Width((width - 35) - 22), GUILayout.Height(20)))
		//		{
		//			//if(boneUnit._isSelectable && !boneUnit._isTarget)
		//			if (!isNotSelectable)
		//			{
		//				_selectedBoneUnit = boneUnit;
		//			}
		//		}

		//		EditorGUILayout.EndHorizontal();

		//	}
		//	if (!boneUnit._isFolded 
		//		//|| _isSearched
		//		)
		//	{
		//		for (int i = 0; i < boneUnit._childUnits.Count; i++)
		//		{
		//			DrawBoneUnit(boneUnit._childUnits[i], level + 1, width, imgIcon_FoldDown, imgIcon_FoldRight, guiContent_Bone, guiStyle, guiStyle_NotSelectable, scrollX);
		//		}
		//	}

		//	//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
		//	//	GUILayout.Space(15);
		//	//	if(GUILayout.Button(new GUIContent(" " + _selectableMeshGroups[i]._name, iconMeshGroup), guiStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
		//	//	{
		//	//		_selectedMeshGroup = _selectableMeshGroups[i];
		//	//	}

		//	//	EditorGUILayout.EndHorizontal();
		//	//}
		//}
	}

}