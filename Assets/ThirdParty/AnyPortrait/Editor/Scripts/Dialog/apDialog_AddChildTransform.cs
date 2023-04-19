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

	public class apDialog_AddChildTransform : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_TRANSFORM_RESULT(bool isSuccess, object loadKey, apMesh mesh, apMeshGroup meshGroup);

		private static apDialog_AddChildTransform s_window = null;

		public enum TARGET
		{
			Mesh,
			MeshGroup,
		}

		private TARGET _target = TARGET.Mesh;
		private apEditor _editor = null;
		private object _loadKey = null;
		private FUNC_SELECT_TRANSFORM_RESULT _funcResult = null;

		//private apMeshGroup _srcMeshGroup = null;
		private Vector2 _scrollList = Vector2.zero;
		private apGUIContentWrapper _guiContent_Category = null;
		private List<apMesh> _meshes = new List<apMesh>();
		private List<apMeshGroup> _meshGroups = new List<apMeshGroup>();

		private apMesh _curSelectedMesh = null;
		private apMeshGroup _curSelectedMeshGroup = null;
		private apGUIContentWrapper _guiContent_MeshItem = null;
		private apGUIContentWrapper _guiContent_MeshGroupItem = null;

		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apMeshGroup srcMeshGroup, FUNC_SELECT_TRANSFORM_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_AddChildTransform), true, "Add Mesh/MeshGroup", true);
			apDialog_AddChildTransform curTool = curWindow as apDialog_AddChildTransform;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				s_window = curTool;
				s_window.position = new Rect(100, 100, 250, 400);
				s_window.Init(editor, loadKey, srcMeshGroup, funcResult);

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
		//--------------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, apMeshGroup srcMeshGroup, FUNC_SELECT_TRANSFORM_RESULT funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			//_srcMeshGroup = srcMeshGroup;
			_funcResult = funcResult;

			//타겟을 검색하자
			_meshes.Clear();
			_meshGroups.Clear();

			for (int i = 0; i < _editor._portrait._meshes.Count; i++)
			{
				_meshes.Add(_editor._portrait._meshes[i]);
			}


			for (int i = 0; i < _editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = _editor._portrait._meshGroups[i];
				if (meshGroup == srcMeshGroup)
				{
					continue;
				}
				//재귀적으로 이미 포함된 MeshGroup인지 판단한다.
				//추가 12.03 : 그 반대도 포함해야 한다.
				apTransform_MeshGroup childMeshGroupTransform = srcMeshGroup.FindChildMeshGroupTransform(meshGroup);
				apTransform_MeshGroup childMeshGroupTransform_Rev = meshGroup.FindChildMeshGroupTransform(srcMeshGroup);
				if (childMeshGroupTransform == null && childMeshGroupTransform_Rev == null)
				{
					//child가 아닐때
					_meshGroups.Add(meshGroup);
				}
			}
		}


		// GUI
		//--------------------------------------------------------------------
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
			GUI.Box(new Rect(0, 41, width, height - 90), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			//1. Tab
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));

			string strCategory = "";

			if (_target == TARGET.Mesh)
			{
				//strCategory = "Meshes";
				strCategory = _editor.GetText(TEXT.DLG_Meshes);
			}
			else
			{
				//strCategory = "Mesh Groups";
				strCategory = _editor.GetText(TEXT.DLG_MeshGroups);
			}

			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Mesh), (_target == TARGET.Mesh), (width / 2) - 2, 25))//"Mesh"
			{
				_target = TARGET.Mesh;
			}

			if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_MeshGroup), (_target == TARGET.MeshGroup), (width / 2) - 2, 25))//"MeshGroup"
			{
				_target = TARGET.MeshGroup;
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			
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

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - 90));

			if(_guiContent_Category == null)
			{
				_guiContent_Category = apGUIContentWrapper.Make(strCategory, false, _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown));
			}
			
			if(_guiContent_MeshItem == null)
			{
				_guiContent_MeshItem = new apGUIContentWrapper();
				_guiContent_MeshItem.SetImage(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh));
				_guiContent_MeshItem.ClearText(true);
			}
			
			if(_guiContent_MeshGroupItem == null)
			{
				_guiContent_MeshGroupItem = new apGUIContentWrapper();
				_guiContent_MeshGroupItem.SetImage(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup));
				_guiContent_MeshGroupItem.ClearText(true);
			}
			
			
			GUILayout.Button(_guiContent_Category.Content, guiStyle_None, GUILayout.Height(20));//<투명 버튼

			if (_target == TARGET.Mesh)
			{
				for (int i = 0; i < _meshes.Count; i++)
				{
					GUIStyle curGUIStyle = guiStyle_None;
					apMesh mesh = _meshes[i];
					if (mesh == _curSelectedMesh)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();

						//이전
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

					_guiContent_MeshItem.ClearText(false);
					_guiContent_MeshItem.AppendSpaceText(1, false);
					_guiContent_MeshItem.AppendText(mesh._name, true);
					
					if (GUILayout.Button(_guiContent_MeshItem.Content, curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
					{
						_curSelectedMesh = mesh;
					}

					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				for (int i = 0; i < _meshGroups.Count; i++)
				{
					GUIStyle curGUIStyle = guiStyle_None;
					apMeshGroup meshGroup = _meshGroups[i];
					if (meshGroup == _curSelectedMeshGroup)
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

					_guiContent_MeshGroupItem.ClearText(false);
					_guiContent_MeshGroupItem.AppendSpaceText(1, false);
					_guiContent_MeshGroupItem.AppendText(meshGroup._name, true);

					if (GUILayout.Button(_guiContent_MeshGroupItem.Content, curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
					{
						_curSelectedMeshGroup = meshGroup;
					}

					EditorGUILayout.EndHorizontal();
				}
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Add), GUILayout.Height(30)))//"Add"
			{
				if (_target == TARGET.Mesh)
				{
					_funcResult(true, _loadKey, _curSelectedMesh, null);
				}
				else
				{
					_funcResult(true, _loadKey, null, _curSelectedMeshGroup);
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