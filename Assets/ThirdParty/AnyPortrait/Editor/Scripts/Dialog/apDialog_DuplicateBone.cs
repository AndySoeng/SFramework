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
	public class apDialog_DuplicateBone : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_DuplicateBone s_window = null;

		private apEditor _editor = null;
		private apBone _targetBone = null;
		private object _loadKey = null;

		public delegate void FUNC_DUPLICATE_BONE_RESULT(bool isSuccess, apBone targetBone, object loadKey, float offsetX, float offsetY, bool isDuplicateChildren);
		private FUNC_DUPLICATE_BONE_RESULT _funcResult = null;

		private float _offsetX = 0.0f;
		private float _offsetY = 0.0f;
		private bool _isDuplicateChildren = true;


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apBone bone, FUNC_DUPLICATE_BONE_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null || bone == null || funcResult == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_DuplicateBone), true, "Duplicate", true);
			apDialog_DuplicateBone curTool = curWindow as apDialog_DuplicateBone;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 300;
				int height = 150;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, bone, loadKey, funcResult);

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
		public void Init(apEditor editor, apBone targetBone, object loadKey, FUNC_DUPLICATE_BONE_RESULT funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetBone = targetBone;
			_offsetX = 0.0f;
			_offsetY = 0.0f;
			_isDuplicateChildren = true;
			_funcResult = funcResult;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetBone == null || _funcResult == null)
			{
				CloseDialog();
				return;
			}
			
			//오프셋과 자식 복사 여부
			int width_Label = 30;
			int width_Value = (width - 10) - 34;
			
			GUILayout.Space(5);

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PositionOffset));
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("X", GUILayout.Width(width_Label));
			_offsetX = EditorGUILayout.FloatField(_offsetX, GUILayout.Width(width_Value));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Y", GUILayout.Width(width_Label));
			_offsetY = EditorGUILayout.FloatField(_offsetY, GUILayout.Width(width_Value));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.DuplicateWithChildBones), GUILayout.Width(width - (10 + 30)));//"Duplicate with Child Bones"
			_isDuplicateChildren = EditorGUILayout.Toggle(_isDuplicateChildren, GUILayout.Width(30));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			int width_Btn = ((width - 10) / 2) - 4;

			bool isCloseDialog = false;

			if (GUILayout.Button(_editor.GetUIWord(UIWORD.Duplicate), GUILayout.Width(width_Btn), GUILayout.Height(30)))//"Start Edit"
			{
				_funcResult(true, _targetBone, _loadKey, _offsetX, _offsetY, _isDuplicateChildren);
				isCloseDialog = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.Cancel), GUILayout.Width(width_Btn), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _targetBone, null, _offsetX, _offsetY, _isDuplicateChildren);
				isCloseDialog = true;
			}
			EditorGUILayout.EndHorizontal();

			if(isCloseDialog)
			{
				CloseDialog();
			}
		}
	}
}