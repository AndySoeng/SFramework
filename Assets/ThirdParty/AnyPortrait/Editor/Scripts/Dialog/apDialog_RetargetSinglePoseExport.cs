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
	public class apDialog_RetargetSinglePoseExport : EditorWindow
	{
		// Members
		//---------------------------------------------------------------------------
		private static apDialog_RetargetSinglePoseExport s_window = null;

		private apEditor _editor = null;
		//private object _loadKey = null;
		private apMeshGroup _targetMeshGroup = null;

		private apRetarget _retarget = new apRetarget();

		private Vector2 _scrollList = new Vector2();

		private Texture2D _imgIcon_Bone = null;


		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apMeshGroup targetMeshGroup, apBone selectedBone)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_RetargetSinglePoseExport), true, "Export Pose", true);
			apDialog_RetargetSinglePoseExport curTool = curWindow as apDialog_RetargetSinglePoseExport;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetMeshGroup, selectedBone);

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
		public void Init(apEditor editor, object loadKey, apMeshGroup targetMeshGroup, apBone selectedBone)
		{
			_editor = editor;
			//_loadKey = loadKey;
			_targetMeshGroup = targetMeshGroup;


			_imgIcon_Bone = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Bone);
			

			UnityEngine.SceneManagement.Scene curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			string sceneName = "Unknown Scene";
			//if(curScene != null)
			//{
			//	sceneName = curScene.name;
			//}

			sceneName = curScene.name;

			_retarget.SetSinglePose(targetMeshGroup, selectedBone, sceneName);
		}


		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetMeshGroup == null)
			{
				CloseDialog();
				return;
			}

			try
			{

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

				//1. 다이얼로그 타이틀
				GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
				guiStyleBox.alignment = TextAnchor.MiddleCenter;
				guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUIStyle guiStyleBox_Left = new GUIStyle(GUI.skin.textField);
				guiStyleBox_Left.alignment = TextAnchor.MiddleLeft;

				//"  Export Pose"
				GUILayout.Box(new GUIContent("  " + _editor.GetText(TEXT.DLG_ExportPose), _editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad)), guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Space(5);

				//2. 기본 정보
				int widthLabel = 120;
				int widthValue = width - (widthLabel + 10 + 5);

				EditorGUILayout.LabelField(_retarget.SinglePoseFile._portraitName + " - " + _retarget.SinglePoseFile._meshGroupName);
				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				GUILayout.Space(5);

				//"Pose Name..
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PoseName) + " : ", GUILayout.Width(widthLabel));
				_retarget.SinglePoseFile._poseName = EditorGUILayout.TextField(_retarget.SinglePoseFile._poseName, GUILayout.Width(widthValue));

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				GUILayout.Space(5);

				//"Description
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Description) + " : ", GUILayout.Width(widthLabel));
				_retarget.SinglePoseFile._description = EditorGUILayout.TextField(_retarget.SinglePoseFile._description, GUILayout.Width(widthValue));

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				//3. Export할 Bone 선택
				int listHeight = height - 240;

				Rect lastRect = GUILayoutUtility.GetLastRect();

				GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
				GUI.Box(new Rect(0, lastRect.y + 5, width + 10, listHeight), "");

				List<apRetargetBonePoseUnit> bonePoseUnits = _retarget.SinglePoseFile._bones;
				int itemWidth = width - 20;
				int itemHeight = 20;

				GUIStyle guiStyle_ItemLabel = new GUIStyle(GUI.skin.label);
				guiStyle_ItemLabel.alignment = TextAnchor.MiddleLeft;

				_scrollList = EditorGUILayout.BeginScrollView(_scrollList, false, true, GUILayout.Width(width + 10), GUILayout.Height(listHeight));
				EditorGUILayout.BeginVertical(GUILayout.Width(width - 10));

				for (int i = 0; i < bonePoseUnits.Count; i++)
				{
					apRetargetBonePoseUnit boneUnit = bonePoseUnits[i];

					EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
					GUILayout.Space(5);

					EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Bone), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
					EditorGUILayout.LabelField(boneUnit._name, guiStyle_ItemLabel, GUILayout.Width(200), GUILayout.Height(itemHeight));

					GUILayout.Space(10);

					//"Export", "Export"

					if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_Export), _editor.GetText(TEXT.DLG_Export), boneUnit._isExported, true, 120, itemHeight))
					{
						boneUnit._isExported = !boneUnit._isExported;
					}

					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(listHeight + 100);

				EditorGUILayout.EndScrollView();

				GUILayout.Space(10);


				// Select All / Deselect All
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width + 10), GUILayout.Height(25));
				GUILayout.Space(5);
				//"Select All"
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_SelectAll), GUILayout.Width((width - 6) / 2), GUILayout.Height(25)))
				{
					for (int i = 0; i < bonePoseUnits.Count; i++)
					{
						apRetargetBonePoseUnit boneUnit = bonePoseUnits[i];
						boneUnit._isExported = true;
					}
				}
				//"Deselect All"
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_DeselectAll), GUILayout.Width((width - 6) / 2), GUILayout.Height(25)))
				{
					for (int i = 0; i < bonePoseUnits.Count; i++)
					{
						apRetargetBonePoseUnit boneUnit = bonePoseUnits[i];
						boneUnit._isExported = false;
					}
				}

				GUILayout.Space(10);

				EditorGUILayout.EndHorizontal();


				string strExportBtn = " " + _editor.GetText(TEXT.DLG_ExportPose);
				if (apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), 
													strExportBtn, strExportBtn, 
													false, !string.IsNullOrEmpty(_retarget.SinglePoseFile._poseName), width, 30))
				{
					//TODO.
					string fileFolderPath = Application.dataPath + "/../" + _editor._bonePose_BaseFolderName;
					string savedFileName = _retarget.SaveSinglePose(fileFolderPath);

					if(string.IsNullOrEmpty(savedFileName))
					{
						_editor.Notification("Pose Save Failed", false, true);
					}
					else
					{
						_editor.Notification("Pose Saved [" + savedFileName + "]", false, false);
					}
					CloseDialog();
					
				}
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width), GUILayout.Height(25)))//"Close"
				{
					CloseDialog();
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("Exception : " + ex);
			}
		}


	}
}