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

	public class apDialog_SelectLinkedMeshGroup : EditorWindow
	{
		// Members
		//------------------------------------------------------------------------
		public delegate void FUNC_SELECT_MESHGROUP(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip);

		private static apDialog_SelectLinkedMeshGroup s_window = null;

		private apEditor _editor = null;
		private object _loadKey = null;

		private FUNC_SELECT_MESHGROUP _funcResult;
		private apAnimClip _targetAnimClip = null;

		private List<apMeshGroup> _selectableMeshGroups = new List<apMeshGroup>();
		private apMeshGroup _selectedMeshGroup = null;

		private Vector2 _scrollList = new Vector2();

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apAnimClip targetAnimClip, FUNC_SELECT_MESHGROUP funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectLinkedMeshGroup), true, "Select Mesh Group", true);
			apDialog_SelectLinkedMeshGroup curTool = curWindow as apDialog_SelectLinkedMeshGroup;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetAnimClip, funcResult);

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
		public void Init(apEditor editor, object loadKey, apAnimClip targetAnimGroup, FUNC_SELECT_MESHGROUP funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetAnimClip = targetAnimGroup;

			_selectedMeshGroup = null;
			_selectableMeshGroups.Clear();

			for (int i = 0; i < _editor._portrait._meshGroups.Count; i++)
			{
				_selectableMeshGroups.Add(_editor._portrait._meshGroups[i]);
			}

		}


		// GUI
		//------------------------------------------------------------------------
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
			GUI.Box(new Rect(0, 35, width, height - (90)), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			Texture2D iconMeshGroup = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);

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
			GUIStyle guiStyle_NotAvailable = new GUIStyle(GUIStyle.none);
			guiStyle_NotAvailable.normal.textColor = Color.red;
			

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			GUILayout.Button(_editor.GetText(TEXT.DLG_SelectMeshGroupToLink), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼//"Select Mesh Group to Link"
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (90)));

			
			GUILayout.Button(new GUIContent(_editor.GetText(TEXT.DLG_MeshGroups), iconImageCategory), guiStyle_None, GUILayout.Height(20));//<투명 버튼//"Mesh Groups"

			//GUILayout.Space(10);
			apMeshGroup curMeshGroup = null;
			for (int i = 0; i < _selectableMeshGroups.Count; i++)
			{
				GUIStyle curGUIStyle = guiStyle_None;
				curMeshGroup = _selectableMeshGroups[i];
				if (curMeshGroup == _selectedMeshGroup)
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
				if(_targetAnimClip != null && curMeshGroup._parentMeshGroup != null)
				{
					//애니메이션 클립과 연결된 메시 그룹을 선택할 때 > 자식 메시 그룹은 붉은 색으로 표시한다.
					curGUIStyle = guiStyle_NotAvailable;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
				GUILayout.Space(15);
				if (GUILayout.Button(new GUIContent(" " + curMeshGroup._name, iconMeshGroup), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
				{
					_selectedMeshGroup = curMeshGroup;
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
				if (_selectedMeshGroup != null)
				{
					_funcResult(true, _loadKey, _selectedMeshGroup, _targetAnimClip);
				}
				else
				{
					_funcResult(false, _loadKey, null, null);
				}
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, null, null);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}
	}

}