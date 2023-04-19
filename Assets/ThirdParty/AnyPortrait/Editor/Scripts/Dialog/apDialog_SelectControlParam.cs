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

	public class apDialog_SelectControlParam : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_CONTROLPARAM_RESULT(bool isSuccess, object loadKey, apControlParam resultControlParam, object savedObject);

		private static apDialog_SelectControlParam s_window = null;

		[Flags]
		public enum PARAM_TYPE
		{
			Bool = 1,
			Int = 2,
			Float = 4,
			Vector2 = 8,
			Vector3 = 16,
			Color = 32,
			All = 63,

		}

		private apEditor _editor = null;
		private object _loadKey = null;
		//private PARAM_TYPE _paramTypeFilter = PARAM_TYPE.All;
		private FUNC_SELECT_CONTROLPARAM_RESULT _funcResult = null;


		private List<apControlParam> _controlParams = new List<apControlParam>();
		private Vector2 _scrollList = new Vector2();
		private apControlParam _curSelectedParam = null;
		private object _savedObject = null;



		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(apEditor editor, PARAM_TYPE paramTypeFilter, FUNC_SELECT_CONTROLPARAM_RESULT funcResult, object savedObject)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectControlParam), true, "Select Control Param", true);
			apDialog_SelectControlParam curTool = curWindow as apDialog_SelectControlParam;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, paramTypeFilter, funcResult, savedObject);

				return loadKey;
			}
			else
			{
				return null;
			}

		}

		//추가 3.22 : 리스트를 받아서 출력하자. 모디파이어용
		public static object ShowDialogWithList(apEditor editor, List<apControlParam> controlParamList, FUNC_SELECT_CONTROLPARAM_RESULT funcResult, object savedObject)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectControlParam), true, "Select Control Param", true);
			apDialog_SelectControlParam curTool = curWindow as apDialog_SelectControlParam;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.InitWithList(editor, loadKey, controlParamList, funcResult, savedObject);

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
		public void Init(apEditor editor, object loadKey, PARAM_TYPE paramTypeFilter, FUNC_SELECT_CONTROLPARAM_RESULT funcResult, object savedObject)
		{
			_editor = editor;
			_loadKey = loadKey;
			//_paramTypeFilter = paramTypeFilter;



			_funcResult = funcResult;

			_savedObject = savedObject;

			_controlParams.Clear();

			List<apControlParam> cParams = _editor._portrait._controller._controlParams;

			for (int i = 0; i < cParams.Count; i++)
			{
				apControlParam.TYPE paramType = cParams[i]._valueType;
				bool isAdded = false;
				switch (paramType)
				{
					//case apControlParam.TYPE.Bool:
					//	if((int)(paramTypeFilter & PARAM_TYPE.Bool) != 0) { isAdded = true; }
					//	break;

					case apControlParam.TYPE.Int:
						if ((int)(paramTypeFilter & PARAM_TYPE.Int) != 0)
						{ isAdded = true; }
						break;

					case apControlParam.TYPE.Float:
						if ((int)(paramTypeFilter & PARAM_TYPE.Float) != 0)
						{ isAdded = true; }
						break;

					case apControlParam.TYPE.Vector2:
						if ((int)(paramTypeFilter & PARAM_TYPE.Vector2) != 0)
						{ isAdded = true; }
						break;

						//case apControlParam.TYPE.Vector3:
						//	if((int)(paramTypeFilter & PARAM_TYPE.Vector3) != 0) { isAdded = true; }
						//	break;

						//case apControlParam.TYPE.Color:
						//	if((int)(paramTypeFilter & PARAM_TYPE.Color) != 0) { isAdded = true; }
						//	break;
				}

				if (isAdded)
				{
					_controlParams.Add(cParams[i]);
				}

			}
		}



		public void InitWithList(apEditor editor, object loadKey, List<apControlParam> controlParamList, FUNC_SELECT_CONTROLPARAM_RESULT funcResult, object savedObject)
		{
			_editor = editor;
			_loadKey = loadKey;
			


			_funcResult = funcResult;

			_savedObject = savedObject;

			_controlParams.Clear();

			for (int i = 0; i < controlParamList.Count; i++)
			{
				if(!_controlParams.Contains(controlParamList[i]))
				{
					_controlParams.Add(controlParamList[i]);
				}
			}
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
			GUI.Box(new Rect(0, 35, width, height - 90), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			Texture2D iconImage = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);

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
			GUILayout.Button(_editor.GetText(TEXT.DLG_SelectControlParemeter), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼//"Select Control Param"
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - 90));


			//"Control Parameters"
			GUILayout.Button(new GUIContent(_editor.GetText(TEXT.DLG_ControlParameters), iconImageCategory), guiStyle_None, GUILayout.Height(20));//<투명 버튼

			//GUILayout.Space(10);
			for (int i = 0; i < _controlParams.Count; i++)
			{
				GUIStyle curGUIStyle = guiStyle_None;
				if (_controlParams[i] == _curSelectedParam)
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
				if (GUILayout.Button(new GUIContent(" " + _controlParams[i]._keyName, iconImage), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
				{
					_curSelectedParam = _controlParams[i];
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
				_funcResult(true, _loadKey, _curSelectedParam, _savedObject);
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