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
	/// 추가 22.1.6 : URP 환경에서 Bake를 했을 때 Clipping이 되지 않을 수 있다.
	/// URP 환경 중 일부 세팅을 바꿔야 할 수 있음을 알려주자
	/// </summary>
	public class apDialog_URPSettingNotice : EditorWindow
	{
		// Members
		//----------------------------------------------------------------------
		private static apDialog_URPSettingNotice s_window = null;

		private apEditor _editor = null;
		private Texture2D _img_Info = null;

		private string _str_Info = "";
		private string _str_Close = "";
		private string _str_DoNotShow = "";
		private apGUIContentWrapper _guiContent_ImgInfo = null;
		
		private GUIStyle _guiStyle_Center = null;
		private GUIStyle _guiStyle_MultilineLabel = null;


		// Show Window
		//------------------------------------------------------------------
		public static void ShowDialog(apEditor editor, int parentX, int parentY)
		{
			
			CloseDialog();

			if (editor == null)
			{
				return;
			}
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_URPSettingNotice), true, "URP Setting", true);
			apDialog_URPSettingNotice curTool = curWindow as apDialog_URPSettingNotice;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 540;
				int height = 420;
				s_window = curTool;
				int basePosX = parentX - 40;
				int basePosY = parentY + 40;
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
			_img_Info = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.MakePath_Icon("URP2DForemostSettingInfo", false));//이 함수에 png를 자동으로 붙여준다.

			//URP 경고 메시지
			//"URP의 2D Renderer를 사용하는 경우 클리핑 메시가 렌더링 되지 않는다면, 2D Renderer의 [Camera Sorting Layer Texture > Foremost Sorting Layer]를 [Disabled]가 아닌 값으로 변경해주세요.";
			_str_Info = _editor.GetText(TEXT.URPWarningMsgInfo);
			_str_Close = _editor.GetText(TEXT.Close);
			_str_DoNotShow = _editor.GetText(TEXT.AmbientCorrection_Ignore);
			//_str_DoNotShow = "메시지 더이상 보지 않기";
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
			if (_guiStyle_Center == null)
			{
				_guiStyle_Center = new GUIStyle(GUI.skin.label);
				_guiStyle_Center.alignment = TextAnchor.MiddleCenter;
				_guiStyle_Center.wordWrap = true;
			}
			

			if (_guiContent_ImgInfo == null)
			{
				_guiContent_ImgInfo = new apGUIContentWrapper();
				_guiContent_ImgInfo.SetImage(_img_Info);
			}
			
			

			//EditorGUILayout.LabelField(new GUIContent("", _img_Info), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(256));
			EditorGUILayout.LabelField(_guiContent_ImgInfo.Content, _guiStyle_Center, GUILayout.Width(width), GUILayout.Height(256));


			GUILayout.Space(10);
			

			//클리핑 옵션 메시지			

			if(_guiStyle_MultilineLabel == null)
			{
				_guiStyle_MultilineLabel = new GUIStyle(GUI.skin.label);
				_guiStyle_MultilineLabel.padding = new RectOffset(10, 10, 5, 5);
				_guiStyle_MultilineLabel.wordWrap = true;
				_guiStyle_MultilineLabel.alignment = TextAnchor.MiddleLeft;
			}

			EditorGUILayout.LabelField(_str_Info, _guiStyle_MultilineLabel, GUILayout.Height(60));
			GUILayout.Space(5);

			//"닫기"
			if(GUILayout.Button(_str_Close, GUILayout.Height(30)))
			{
				isClose = true;
			}
			GUILayout.Space(5);
			
			//"이 메시지 더이상 보지 않기"
			//이부분 버그
			//_isShowURPWarningMsg 이게 True면 계속 보임
			bool doNotShowAmbientDialog = !_editor._isShowURPWarningMsg;//

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(10);
			//bool isAmbientOption = EditorGUILayout.Toggle(_editor._isAmbientCorrectionOption, GUILayout.Width(30));
			bool nextDoNotShowAmbient = EditorGUILayout.Toggle(doNotShowAmbientDialog, GUILayout.Width(30));
			GUILayout.Space(5);
			//if(isAmbientOption != _editor._isAmbientCorrectionOption)
			if(nextDoNotShowAmbient != doNotShowAmbientDialog)
			{
				//_editor._isAmbientCorrectionOption = isAmbientOption;
				_editor._isShowURPWarningMsg = !nextDoNotShowAmbient;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.LabelField(_str_DoNotShow, GUILayout.Width(300));
			

			EditorGUILayout.EndHorizontal();

			if(isClose)
			{
				CloseDialog();
			}
		}
	}
}