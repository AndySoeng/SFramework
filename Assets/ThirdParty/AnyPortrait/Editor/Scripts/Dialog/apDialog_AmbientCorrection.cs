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
	/// 추가 3.31 : Ambient를 자동으로 보정할 것인지 물어보는 다이얼로그
	/// Bake를 진행한 이후에 뜬다.
	/// </summary>
	public class apDialog_AmbientCorrection : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_AmbientCorrection s_window = null;

		private apEditor _editor = null;
		private Texture2D _img_Info = null;

		private string _str_Info = "";
		private string _str_Convert = "";
		private string _str_Close = "";
		private string _str_DoNotShow = "";
		private apGUIContentWrapper _guiContent_ImgInfo = null;

		// Show Window
		//------------------------------------------------------------------
		public static void ShowDialog(apEditor editor, int parentX, int parentY)
		{
			
			CloseDialog();

			if (editor == null)
			{
				return;
			}
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_AmbientCorrection), true, "Ambient Correction", true);
			apDialog_AmbientCorrection curTool = curWindow as apDialog_AmbientCorrection;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 540;
				int height = 430;
				s_window = curTool;
				int basePosX = parentX - 50;
				int basePosY = parentY + 50;
				//s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
				//								(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
				//								width, height);

				s_window.position = new Rect(Mathf.Max(basePosX - (width / 2), 10),
												Mathf.Max(basePosY - (height / 2), 10),
												width, height);

				s_window.Init(editor);
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
		public void Init(apEditor editor)
		{
			_editor = editor;
			//_img_Info = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + "AmbientInfo.png");
			_img_Info = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.MakePath_Icon("AmbientInfo", false));//이 함수에 png를 자동으로 붙여준다.

			_str_Info = _editor.GetText(TEXT.AmbientCorrection_Info);
			_str_Convert = _editor.GetText(TEXT.AmbientCorrection_Convert);
			_str_Close = _editor.GetText(TEXT.Close);
			_str_DoNotShow = _editor.GetText(TEXT.AmbientCorrection_Ignore);
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			if (_editor == null)
			{
				CloseDialog();
				return;
			}

			int width = (int)position.width;
			int height = (int)position.height;
			width -= 10;
			bool isClose = false;

			GUILayout.Space(5);
			GUIStyle guiStyle_Center = new GUIStyle(GUI.skin.label);
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			guiStyle_Center.wordWrap = true;

			if (_guiContent_ImgInfo == null)
			{
				_guiContent_ImgInfo = new apGUIContentWrapper();
				_guiContent_ImgInfo.SetImage(_img_Info);
			}
			

			//EditorGUILayout.LabelField(new GUIContent("", _img_Info), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(256));
			EditorGUILayout.LabelField(_guiContent_ImgInfo.Content, guiStyle_Center, GUILayout.Width(width), GUILayout.Height(256));

			GUILayout.Space(10);
			//"Ambient Color가 검은색이 아니면 결과물이\n원본보다 밝게 보여질 수 있습니다."
			EditorGUILayout.LabelField(_str_Info, guiStyle_Center, GUILayout.Height(40));
			GUILayout.Space(5);

			//"Ambient Color를 검은색으로 변경하기"
			if(GUILayout.Button(_str_Convert, GUILayout.Height(30)))
			{
				MakeAmbientLightToBlack();
				isClose = true;
			}

			//"닫기"
			if(GUILayout.Button(_str_Close, GUILayout.Height(30)))
			{
				isClose = true;
			}
			GUILayout.Space(5);
			
			//"이 메시지 더이상 보지 않기"
			//이부분 버그
			bool doNotShowAmbientDialog = !_editor._isAmbientCorrectionOption;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(10);
			//bool isAmbientOption = EditorGUILayout.Toggle(_editor._isAmbientCorrectionOption, GUILayout.Width(30));
			bool nextDoNotShowAmbient = EditorGUILayout.Toggle(doNotShowAmbientDialog, GUILayout.Width(30));
			GUILayout.Space(5);
			//if(isAmbientOption != _editor._isAmbientCorrectionOption)
			if(nextDoNotShowAmbient != doNotShowAmbientDialog)
			{
				//_editor._isAmbientCorrectionOption = isAmbientOption;
				_editor._isAmbientCorrectionOption = !nextDoNotShowAmbient;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.LabelField(_str_DoNotShow, GUILayout.Width(300));
			

			EditorGUILayout.EndHorizontal();

			if(isClose)
			{
				CloseDialog();
			}
		}

		private void MakeAmbientLightToBlack()
		{	
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.ambientLight = Color.black;
			apEditorUtil.SetEditorDirty();
		}
	}
}