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

	public class apDialog_FFDSize : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_FFDSize s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		private object _loadKey = null;

		public delegate void FUNC_FFD_SIZE_RESULT(bool isSuccess, object loadKey, int numX, int numY);
		private FUNC_FFD_SIZE_RESULT _funcResult = null;

		private int _nX, _nY;

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait, FUNC_FFD_SIZE_RESULT funcResult, int curX, int curY)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_FFDSize), true, "Custom FFD Size", true);
			apDialog_FFDSize curTool = curWindow as apDialog_FFDSize;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 300;
				int height = 130;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, portrait, loadKey, funcResult, curX, curY);

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
		public void Init(apEditor editor, apPortrait portrait, object loadKey, FUNC_FFD_SIZE_RESULT funcResult, int curX, int curY)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetPortrait = portrait;
			_funcResult = funcResult;
			_nX = curX;
			_nY = curY;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null || _funcResult == null)
			{
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				CloseDialog();
				return;
			}

			//Bake 설정
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_SetSuctomFFDGridSize));//"Set Custom FFD Grid Size"
			//X, Y 개수를 표시
			int width_Label = 30;
			int width_Value = (width - 10) - 34;

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("X", GUILayout.Width(width_Label));
			_nX = EditorGUILayout.IntSlider(_nX, 2, 8, GUILayout.Width(width_Value));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Y", GUILayout.Width(width_Label));
			_nY = EditorGUILayout.IntSlider(_nY, 2, 8, GUILayout.Width(width_Value));
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			int width_Btn = ((width - 10) / 2) - 4;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_StartEdit), GUILayout.Width(width_Btn), GUILayout.Height(30)))//"Start Edit"
			{
				_funcResult(true, _loadKey, _nX, _nY);
				CloseDialog();
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width_Btn), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, _nX, _nY);
				CloseDialog();
			}
			EditorGUILayout.EndHorizontal();

		}

	}

}