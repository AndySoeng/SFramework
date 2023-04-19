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
	/// <summary>
	/// v1.4.2에 추가된 다이얼로그. 메시의 버텍스, 핀을 복사하여 붙여넣을때 나온다.
	/// 붙여넣기 방식을 결정할 수 있다.
	/// </summary>
	public class apDialog_CopyMeshVertPin : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_CopyMeshVertPin s_window = null;

		private apEditor _editor = null;
		private apMesh _dstMesh = null;

		private object _loadKey = null;

		public enum POSITION_SPACE : int
		{
			/// <summary>절대 위치</summary>
			SourceValueAsIs = 0,
			/// <summary>피벗의 위치에 따른 상대 위치</summary>
			RelativeToPivot = 1,
		}

		public delegate void FUNC_ONCOPYMESHPIN(bool isSuccess,
												object loadKey, 
												apMesh dstMesh,
												POSITION_SPACE positionSpace);

		private FUNC_ONCOPYMESHPIN _funcResult = null;
		private POSITION_SPACE _positionSpace = POSITION_SPACE.SourceValueAsIs;

		private const string PREF_POS_SPACE = "AnyPortrait_MeshCopyProp_PosSpace";


		public static object ShowDialog(	apEditor editor,
											apMesh mesh,
											FUNC_ONCOPYMESHPIN funcOnCopy,
											bool isPastePins)
		{
			CloseDialog();

			if (editor == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(	typeof(apDialog_CopyMeshVertPin),
																true,
																isPastePins ? "Paste Pins" : "Paste Vertices",
																true);
			apDialog_CopyMeshVertPin curTool = curWindow as apDialog_CopyMeshVertPin;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 300;
				int height = 85;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				object loadKey = new object();

				s_window.Init(	editor,
								mesh,
								funcOnCopy,
								loadKey);

				return loadKey;
			}

			return null;
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
		public void Init(	apEditor editor,
							apMesh mesh,
							FUNC_ONCOPYMESHPIN funcOnCopy,
							object loadKey)
		{
			_editor = editor;
			_dstMesh = mesh;

			_loadKey = loadKey;
			_funcResult = funcOnCopy;

			//선택지는 옵션으로 마지막 값이 저장되어 있다.
			_positionSpace = (POSITION_SPACE)EditorPrefs.GetInt(PREF_POS_SPACE, (int)POSITION_SPACE.SourceValueAsIs);
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;

			if(_editor == null)
			{
				CloseDialog();
				return;
			}

			int width_2Btn = ((width - 10) / 2) - 2;

			GUILayout.Space(5);

			//위치값
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Position));
			POSITION_SPACE nextSpace = (POSITION_SPACE)EditorGUILayout.EnumPopup(_positionSpace);

			if(nextSpace != _positionSpace)
			{
				_positionSpace = nextSpace;
				SetPref(PREF_POS_SPACE, (int)_positionSpace, (int)POSITION_SPACE.SourceValueAsIs);
			}

			GUILayout.Space(10);

			bool isClose = false;

			EditorGUILayout.BeginHorizontal(GUILayout.Height(34));

			GUILayout.Space(5);


			if (GUILayout.Button(_editor.GetUIWord(UIWORD.Paste), GUILayout.Width(width_2Btn), GUILayout.Height(32)))
			{
				if (_funcResult != null)
				{
					_funcResult(true,
								_loadKey,
								_dstMesh,
								_positionSpace);
				}
				isClose = true;
			}

			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Cancel), GUILayout.Width(width_2Btn), GUILayout.Height(32)))
			{
				if (_funcResult != null)
				{
					_funcResult(false,
								_loadKey,
								null,
								POSITION_SPACE.SourceValueAsIs);
				}
				isClose = true;
			}

			EditorGUILayout.EndHorizontal();

			if(isClose)
			{
				CloseDialog();
			}
		}


		private void SetPref(string strPref, bool curValue, bool defaultValue)
		{
			if(curValue == defaultValue)
			{
				//기본값이라면 삭제
				EditorPrefs.DeleteKey(strPref);
			}
			else
			{
				EditorPrefs.SetBool(strPref, curValue);
			}
		}

		private void SetPref(string strPref, int curValue, int defaultValue)
		{
			if(curValue == defaultValue)
			{
				//기본값이라면 삭제
				EditorPrefs.DeleteKey(strPref);
			}
			else
			{
				EditorPrefs.SetInt(strPref, curValue);
			}
		}
	}
}