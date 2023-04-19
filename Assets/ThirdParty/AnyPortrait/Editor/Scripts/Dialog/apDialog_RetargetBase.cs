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

	public class apDialog_RetargetBase : EditorWindow
	{

		// Members
		//----------------------------------------------------------------------------
		public delegate void FUNC_LOAD_RETARGET(bool isSuccess, object loadKey, apRetarget retargetData, apMeshGroup targetMeshGroup);

		private static apDialog_RetargetBase s_window = null;



		private apEditor _editor = null;
		private object _loadKey = null;
		private apMeshGroup _targetMeshGroup = null;

		private FUNC_LOAD_RETARGET _funcResult;

		private apRetarget _retargetData = new apRetarget();
		

		private Vector2 _scrollList = new Vector2();

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apMeshGroup targetMeshGroup, FUNC_LOAD_RETARGET funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_RetargetBase), true, "Export/Import Bones", true);
			apDialog_RetargetBase curTool = curWindow as apDialog_RetargetBase;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 800;
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
		//------------------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, apMeshGroup targetMeshGroup, FUNC_LOAD_RETARGET funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetMeshGroup = targetMeshGroup;
			
			
		}


		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			try
			{
				int width = (int)position.width;
				int height = (int)position.height;
				if (_editor == null || _funcResult == null || _targetMeshGroup == null)
				{
					CloseDialog();
					return;
				}

				Color prevColor = GUI.backgroundColor;

				//레이아웃 구조
				//1. Save
				// - 저장 버튼
				//2. Load
				// - 로드 버튼
				// - 본 정보 리스트
				//   - <색상> 인덱스, 이름 -> 적용 여부 + IK + 색상 로드
				// - 전체 선택 / 해제
				// - 전체 IK 포함 여부, 
				// - 옵션 : 크기

				width -= 10;

				//1. Save
				GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
				guiStyleBox.alignment = TextAnchor.MiddleCenter;
				guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUIStyle guiStyleBox_Left = new GUIStyle(GUI.skin.textField);
				guiStyleBox_Left.alignment = TextAnchor.MiddleLeft;

				//"  Export Bone Structure"
				GUILayout.Box(new GUIContent("  " + _editor.GetText(TEXT.DLG_ExportBoneStructure), _editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad)), guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Space(5);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);

				//<BONE_EDIT> : 그대로 이용
				int nBones = _targetMeshGroup._boneList_All.Count;
				////>>Bone Set 이용
				//int nBones = 0;
				//for (int iSet = 0; iSet < _targetMeshGroup._boneListSets.Count; iSet++)
				//{
				//	nBones += _targetMeshGroup._boneListSets[iSet]._bones_All.Count;

				//}


				string strBones = "";
				if (nBones == 0)
				{
					//strBones = "No Bones to Export";
					strBones = _editor.GetText(TEXT.DLG_NoBonesToExport);
				}
				else if (nBones == 1)
				{
					//strBones = "1 Bone to Export";
					strBones = _editor.GetText(TEXT.DLG_1BoneToExport);
				}
				else
				{
					//strBones = nBones + " Bones to Export";
					strBones = _editor.GetTextFormat(TEXT.DLG_NBonesToExport, nBones);
				}
				if (nBones > 0)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.8f, prevColor.g * 1.5f, prevColor.b * 1.5f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(prevColor.r * 1.5f, prevColor.g * 0.8f, prevColor.b * 0.8f, 1.0f);
				}
				GUILayout.Box(strBones, guiStyleBox, GUILayout.Width(width - 120), GUILayout.Height(25));
				GUI.backgroundColor = prevColor;

				//" Export"
				if (apEditorUtil.ToggledButton(_editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), " " + _editor.GetText(TEXT.Export), false, (nBones > 0), 115, 25))
				{
					string saveFilePath = EditorUtility.SaveFilePanel("Save Bone Structure",
																		apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport),
																		"", "apb");
					if (!string.IsNullOrEmpty(saveFilePath))
					{
						//추가 21.7.3 : 이스케이프 문자 삭제
						saveFilePath = apUtil.ConvertEscapeToPlainText(saveFilePath);

						//Save를 하자
						bool isResult = apRetarget.SaveBaseStruct(_targetMeshGroup, saveFilePath);

						apEditorUtil.SetLastExternalOpenSaveFilePath(saveFilePath, apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport);//추가 21.3.1

						if (isResult)
						{
							_editor.Notification("[" + saveFilePath + "] is Saved", false, false);
							//Debug.Log("[" + saveFilePath + "] is Saved");
						}
						else
						{
							//Debug.LogError("File Save Failed");
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				//2. Load
				// - 로드 버튼
				// - 본 정보 리스트
				//   - <색상> 인덱스, 이름 -> 적용 여부 + IK + 색상 로드
				// - 전체 선택 / 해제
				// - 전체 IK 포함 여부, 
				// - 옵션 : 크기
				//"  Import Bone Structure"
				GUILayout.Box(new GUIContent("  " + _editor.GetText(TEXT.DLG_ImportBoneStructure), _editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones)), guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Space(5);

				//로드한 파일 정보
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				//TODO : 
				bool isFileLoaded = _retargetData.IsBaseFileLoaded;
				string strFileName = _retargetData.BaseLoadedFilePath;
				if (isFileLoaded)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.8f, prevColor.g * 2.0f, prevColor.b * 0.8f, 1.0f);
				}
				else
				{
					//strFileName = "No File is Imported";
					strFileName = _editor.GetText(TEXT.DLG_NoFileIsImported);
					GUI.backgroundColor = new Color(prevColor.r * 1.5f, prevColor.g * 0.8f, prevColor.b * 0.8f, 1.0f);
				}

				EditorGUILayout.TextField(strFileName, guiStyleBox_Left, GUILayout.Width(width - 120), GUILayout.Height(25));
				GUI.backgroundColor = prevColor;

				//"Load File"
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_LoadFile), false, true, 115, 25))
				{
					string loadFilePath = EditorUtility.OpenFilePanel("Open Bone Structure",
																		apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport), "apb");
					if (!string.IsNullOrEmpty(loadFilePath))
					{
						//추가 21.7.3 : 이스케이프 문자 삭제
						loadFilePath = apUtil.ConvertEscapeToPlainText(loadFilePath);

						bool loadResult = _retargetData.LoadBaseStruct(loadFilePath);
						if (loadResult)
						{
							_editor.Notification("[" + loadFilePath + "] is Loaded", false, false);
							//Debug.Log("[" + loadFilePath + "] is Loaded");
						}
						apEditorUtil.SetLastExternalOpenSaveFilePath(loadFilePath, apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport);//추가 21.3.1
					}
				}

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);
				int listHeight = height - 450;

				Rect lastRect = GUILayoutUtility.GetLastRect();

				GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
				GUI.Box(new Rect(0, lastRect.y + 5, width + 10, listHeight), "");
				GUI.backgroundColor = prevColor;


				List<apRetargetBoneUnit> baseBoneUnits = _retargetData.BaseBoneUnits;

				_scrollList = EditorGUILayout.BeginScrollView(_scrollList, false, true, GUILayout.Width(width + 10), GUILayout.Height(listHeight));
				EditorGUILayout.BeginVertical(GUILayout.Width(width - 20));

				if (baseBoneUnits != null)
				{
					GUIStyle guiStyle_ItemLabel = new GUIStyle(GUI.skin.label);
					guiStyle_ItemLabel.alignment = TextAnchor.MiddleLeft;

					GUIStyle guiStyle_ItemTextBox = new GUIStyle(GUI.skin.textField);
					guiStyle_ItemTextBox.alignment = TextAnchor.MiddleLeft;

					//   - <색상> 인덱스, 이름 -> 적용 여부 + IK + 색상 로드
					apRetargetBoneUnit boneUnit = null;
					int itemWidth = width - 20;
					int itemHeight = 20;

					string strImport = _editor.GetText(TEXT.DLG_Import);
					string strNoImport = _editor.GetText(TEXT.DLG_NoImport);
					string strIK = "IK";//<<이건 고유명사
					string strNoIK = _editor.GetText(TEXT.DLG_NoIK);
					string strShape = _editor.GetText(TEXT.DLG_Shape);
					string strNoShape = _editor.GetText(TEXT.DLG_NoShape);

					for (int i = 0; i < baseBoneUnits.Count; i++)
					{
						boneUnit = baseBoneUnits[i];
						EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
						GUILayout.Space(10);
						GUI.backgroundColor = boneUnit._color;
						GUILayout.Box("", apEditorUtil.WhiteGUIStyle_Box, GUILayout.Width(16), GUILayout.Height(16));
						GUI.backgroundColor = prevColor;

						EditorGUILayout.LabelField(boneUnit._unitID.ToString(), guiStyle_ItemLabel, GUILayout.Width(30), GUILayout.Height(itemHeight));
						boneUnit._name = EditorGUILayout.TextField(boneUnit._name, guiStyle_ItemTextBox, GUILayout.Width(120), GUILayout.Height(itemHeight));

						GUILayout.Space(20);

						//"Import", "Not Import"
						if (apEditorUtil.ToggledButton_2Side(strImport, strNoImport, boneUnit._isImportEnabled, true, 100, itemHeight))
						{
							boneUnit._isImportEnabled = !boneUnit._isImportEnabled;
						}
						GUILayout.Space(10);
						//"IK", "No IK"
						if (apEditorUtil.ToggledButton_2Side(strIK, strNoIK, boneUnit._isIKEnabled, boneUnit._isImportEnabled, 70, itemHeight))
						{
							boneUnit._isIKEnabled = !boneUnit._isIKEnabled;
						}
						//"Shape", "No Shape"
						if (apEditorUtil.ToggledButton_2Side(strShape, strNoShape, boneUnit._isShapeEnabled, boneUnit._isImportEnabled, 70, itemHeight))
						{
							boneUnit._isShapeEnabled = !boneUnit._isShapeEnabled;
						}

						EditorGUILayout.EndHorizontal();
					}
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(listHeight + 100);
				EditorGUILayout.EndScrollView();
				GUILayout.Space(20);

				// - 전체 선택 / 해제
				// - 전체 IK 포함 여부, 
				// - 옵션 : 크기
				int widthHalf = (width / 2) - 4;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_EnableAllBones), false, isFileLoaded, widthHalf, 25))//"Enable All Bones"
				{
					if (baseBoneUnits != null)
					{
						for (int i = 0; i < baseBoneUnits.Count; i++)
						{
							baseBoneUnits[i]._isImportEnabled = true;
						}
					}

				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_DisableAllBones), false, isFileLoaded, widthHalf, 25))//"Disable All Bones"
				{
					if (baseBoneUnits != null)
					{
						for (int i = 0; i < baseBoneUnits.Count; i++)
						{
							baseBoneUnits[i]._isImportEnabled = false;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_EnableAllIK), false, isFileLoaded, widthHalf, 25))//"Enable All IK"
				{
					if (baseBoneUnits != null)
					{
						for (int i = 0; i < baseBoneUnits.Count; i++)
						{
							baseBoneUnits[i]._isIKEnabled = true;
						}
					}

				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_DisableAllIK), false, isFileLoaded, widthHalf, 25))//"Disable All IK"
				{
					if (baseBoneUnits != null)
					{
						for (int i = 0; i < baseBoneUnits.Count; i++)
						{
							baseBoneUnits[i]._isIKEnabled = false;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_EnableAllShape), false, isFileLoaded, widthHalf, 25))//"Enable All Shape"
				{
					if (baseBoneUnits != null)
					{
						for (int i = 0; i < baseBoneUnits.Count; i++)
						{
							baseBoneUnits[i]._isShapeEnabled = true;
						}
					}

				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_DisableAllShape), false, isFileLoaded, widthHalf, 25))//"Disable All Shape"
				{
					if (baseBoneUnits != null)
					{
						for (int i = 0; i < baseBoneUnits.Count; i++)
						{
							baseBoneUnits[i]._isShapeEnabled = false;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				int widthLabel = 150;
				int widthValue = width - 155;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
				GUILayout.Space(5);

				//"Import Scale"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ImportScale), GUILayout.Width(widthLabel));
				_retargetData._importScale = EditorGUILayout.FloatField(_retargetData._importScale, GUILayout.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);
				bool isClose = false;
				bool isSelectBtnAvailable = _retargetData.IsBaseFileLoaded;//<<TODO : 파일을 연게 있다면 이게 true

				//"  Import to [" + _targetMeshGroup._name + "]"
				if (apEditorUtil.ToggledButton(_editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones),
					"  " + _editor.GetTextFormat(TEXT.DLG_ImportToMeshGroup, _targetMeshGroup._name), false, isSelectBtnAvailable, width, 30))
				{
					_funcResult(true, _loadKey, _retargetData, _targetMeshGroup);
					isClose = true;
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Close), false, true, width, 30))//"Close"
				{
					//_funcResult(false, _loadKey, null, null);
					_funcResult(false, _loadKey, null, _targetMeshGroup);
					isClose = true;
				}

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
	}
}