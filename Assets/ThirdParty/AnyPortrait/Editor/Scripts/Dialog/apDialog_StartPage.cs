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
	//[데모용] Start Page 다이얼로그
	//에디터 시작시 나온다.
	//데모 버전 : 매 시작시마다 나온다.
	
	//내용
	//풀 버전 : 로고 / 버전 / 홈페이지 /  닫기 / "다시 보이지 않음" (Toogle)
	//데모 버전 : 로고 / 버전 / 데모와 정품 차이 안내<- 이거 한글만 되어있다. / 닫기 / AssetStore 페이지

	public class apDialog_StartPage : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_StartPage s_window = null;

		private apEditor _editor = null;
		private Texture2D _img_Logo = null;

		//메뉴 구성 변경
		//(1) Gettting Started   (2) Video Tutorials
		//(3) Manual             (4) Forum

		//Hompage
		//Close
		private Texture2D _img_Icon_GettingStarted = null;
		private Texture2D _img_Icon_VideoTutorials = null;
		private Texture2D _img_Icon_Manual = null;
		private Texture2D _img_Icon_Forum = null;

		private bool _isFullVersion = false;


		private GUIContent _guiContent_GettingStarted = null;
		private GUIContent _guiContent_VideoTutorial = null;
		private GUIContent _guiContent_Manual = null;
		private GUIContent _guiContent_Forum = null;
		
		private GUIStyle _guiStyle_Box = null;

		private const int WINDOW_WIDTH = 700;
		private const int WINDOW_HEIGHT_FULL = 410;
		private const int WINDOW_HEIGHT_DEMO = 430;

		// Show Window
		//------------------------------------------------------------------
		public static void ShowDialog(	apEditor editor, 
										Texture2D img_Logo, 
										Texture2D img_Icon_GettingStarted,
										Texture2D img_Icon_VideoTutorials,
										Texture2D img_Icon_Manual,
										Texture2D img_Icon_Forum,
										bool isFullVersion)
		{
			
			CloseDialog();

			if (editor == null)
			{
				return;
			}

			string strTitle = "Welcome!";
			if(!isFullVersion)
			{
				strTitle = "Demo Start Page";
			}

			
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_StartPage), true, strTitle, true);
			apDialog_StartPage curTool = curWindow as apDialog_StartPage;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = WINDOW_WIDTH;
				int height = isFullVersion ? WINDOW_HEIGHT_FULL : WINDOW_HEIGHT_DEMO;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(	editor, 
								img_Logo, 
								img_Icon_GettingStarted,
								img_Icon_VideoTutorials,
								img_Icon_Manual,
								img_Icon_Forum,
								isFullVersion);
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
		public void Init(	apEditor editor, 
							Texture2D img_Logo, 
							Texture2D img_Icon_GettingStarted,
							Texture2D img_Icon_VideoTutorials,
							Texture2D img_Icon_Manual,
							Texture2D img_Icon_Forum,
							bool isFullVersion)
		{
			_editor = editor;
			_img_Logo = img_Logo;

			_img_Icon_GettingStarted = img_Icon_GettingStarted;
			_img_Icon_VideoTutorials = img_Icon_VideoTutorials;
			_img_Icon_Manual = img_Icon_Manual;
			_img_Icon_Forum = img_Icon_Forum;

			_isFullVersion = isFullVersion;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			int targetHeight = _isFullVersion ? WINDOW_HEIGHT_FULL : WINDOW_HEIGHT_DEMO;
			if(width != WINDOW_WIDTH
				|| height != targetHeight)
			{
				Rect resizedRect = position;
				resizedRect.width = WINDOW_WIDTH;
				resizedRect.height = targetHeight;

				position = resizedRect;
			}

			//width -= 10;
			//if (_editor == null)
			//{
			//	CloseDialog();
			//	return;
			//}

			////만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			//if (_editor != apEditor.CurrentEditor)
			//{
			//	CloseDialog();
			//	return;
			//}


			//1. 로고
			//2. 버전
			//3. 데모 기능 제한 확인하기

			if(_guiStyle_Box == null)
			{
				_guiStyle_Box = new GUIStyle(GUI.skin.box);
				_guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				_guiStyle_Box.margin = new RectOffset(0, 0, 0, 0);
				_guiStyle_Box.padding = new RectOffset(0, 0, 0, 0);
			}

			bool isClose = false;

			int logoWidth = _img_Logo.width;
			int logoHeight = _img_Logo.height;
			int boxHeight = (int)((float)width * ((float)logoHeight / (float)logoWidth));
			Color prevColor = GUI.backgroundColor;

			//GUI.backgroundColor = Color.black;
			GUI.backgroundColor = new Color(0, 0, 0, 0);
			GUILayout.Box(_img_Logo, _guiStyle_Box, GUILayout.Width(width), GUILayout.Height(boxHeight));

			GUI.backgroundColor = prevColor;
			GUILayout.Space(5);

			if (_isFullVersion)
			{
				EditorGUILayout.LabelField(string.Format("Build : {0}", apVersion.I.APP_VERSION));
			}
			else
			{ 
				//"Demo Version : " + apVersion.I.APP_VERSION
				EditorGUILayout.LabelField(string.Format("{0} : {1}", _editor.GetText(TEXT.DLG_DemoVersion), apVersion.I.APP_VERSION));
			}
			GUILayout.Space(10);

			if(_guiContent_GettingStarted == null)
			{
				_guiContent_GettingStarted = new GUIContent("  " + _editor.GetText(TEXT.StartPage_GettingStarted), _img_Icon_GettingStarted);
			}
			if(_guiContent_VideoTutorial == null)
			{
				_guiContent_VideoTutorial = new GUIContent("  " + _editor.GetText(TEXT.StartPage_VideoTutorials), _img_Icon_VideoTutorials);
			}
			if(_guiContent_Manual == null)
			{
				_guiContent_Manual = new GUIContent("  " + _editor.GetText(TEXT.StartPage_Manual), _img_Icon_Manual);
			}
			if(_guiContent_Forum == null)
			{
				_guiContent_Forum = new GUIContent("  " + _editor.GetText(TEXT.StartPage_Forum), _img_Icon_Forum);
			}


			//추가 21.10.11 : 첫 사용자용 버튼
			int width_Half = ((width - 10) / 2) - 1;
			int height_4Btn = 40;

			//버튼 두개씩

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height_4Btn));
			GUILayout.Space(4);
			if(GUILayout.Button(_guiContent_GettingStarted, GUILayout.Width(width_Half), GUILayout.Height(height_4Btn)))
			{
				//Getting Started : 시작하기
				if(_editor._language == apEditor.LANGUAGE.Korean)//한국어
				{
					Application.OpenURL("https://rainyrizzle.github.io/kr/GettingStarted.html");
				}
				else if(_editor._language == apEditor.LANGUAGE.Japanese)//일본어
				{
					Application.OpenURL("https://rainyrizzle.github.io/jp/GettingStarted.html");
				}
				else//영어 + 기타
				{
					Application.OpenURL("https://rainyrizzle.github.io/en/GettingStarted.html");
				}
			}
			if(GUILayout.Button(_guiContent_VideoTutorial, GUILayout.Width(width_Half), GUILayout.Height(height_4Btn)))
			{
				//Video Tutorial : 동영상 튜토리얼
				if(_editor._language == apEditor.LANGUAGE.Korean)//한국어
				{
					Application.OpenURL("https://www.rainyrizzle.com/ap-videotutorial-kor");
				}
				else if(_editor._language == apEditor.LANGUAGE.Japanese)//일본어
				{
					Application.OpenURL("https://www.rainyrizzle.com/ap-videotutorial-jp");
				}
				else//영어 + 기타
				{
					Application.OpenURL("https://www.rainyrizzle.com/ap-videotutorial-eng");
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Height(height_4Btn));
			GUILayout.Space(4);
			if(GUILayout.Button(_guiContent_Manual, GUILayout.Width(width_Half), GUILayout.Height(height_4Btn)))
			{
				//Manual : 메뉴얼
				if(_editor._language == apEditor.LANGUAGE.Korean)//한국어
				{
					Application.OpenURL("https://rainyrizzle.github.io/kr/AdManual.html");
				}
				else if(_editor._language == apEditor.LANGUAGE.Japanese)//일본어
				{
					Application.OpenURL("https://rainyrizzle.github.io/jp/AdManual.html");
				}
				else//영어 + 기타
				{
					Application.OpenURL("https://rainyrizzle.github.io/en/AdManual.html");
				}
			}
			if(GUILayout.Button(_guiContent_Forum, GUILayout.Width(width_Half), GUILayout.Height(height_4Btn)))
			{
				//Forum : 포럼
				Application.OpenURL("https://www.rainyrizzle.com/ap-forum");
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			//풀 버전 : 로고 / 버전 / 홈페이지 /  닫기 / "다시 보이지 않음" (Toogle)
			//데모 버전 : 로고 / 버전 / 데모와 정품 차이 안내<- 이거 한글만 되어있다. / 닫기 / AssetStore 페이지
			
			if (_isFullVersion)
			{
				//홈페이지
				//데모 다운로드 안내
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_StartPage_Hompage), GUILayout.Height(25)))//"Check Limitations"
				{
					//홈페이지로 갑시다.
					Application.OpenURL("https://www.rainyrizzle.com");
				}
			}
			else
			{
				//데모 다운로드 안내
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_CheckLimitations), GUILayout.Height(25)))//"Check Limitations"
				{
					//홈페이지로 갑시다.
					if(_editor._language == apEditor.LANGUAGE.Korean)
					{
						Application.OpenURL("https://www.rainyrizzle.com/ap-demodownload-kor");
					}
					else
					{
						Application.OpenURL("https://www.rainyrizzle.com/ap-demodownload-eng");
					}
					
					isClose = true;
				}
			}
			
			GUILayout.Space(5);

			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(25)))//"Close"
			{
				isClose = true;
			}

			//왼쪽 : 에셋 스토어 또는 계속 시작

			GUILayout.Space(10);
			if(_isFullVersion)			
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_StartPage_AlawysOn), GUILayout.Width(width - (12 + 20)), GUILayout.Height(20));
				bool isShow = EditorGUILayout.Toggle(_editor._startScreenOption_IsShowStartup, GUILayout.Width(20), GUILayout.Height(20));
				if(_editor._startScreenOption_IsShowStartup != isShow)
				{
					_editor._startScreenOption_IsShowStartup = isShow;
					_editor.SaveEditorPref();
				}

				EditorGUILayout.EndHorizontal();
			}
			else
			{
				if(GUILayout.Button("Asset Store", GUILayout.Height(25)))
				{
					//AssetStore 등록하면 여기에 넣자
					Application.OpenURL("http://u3d.as/16c7");
					isClose = true;
				}
			}
			

			if(isClose)
			{
				CloseDialog();
			}
		}
	}

}