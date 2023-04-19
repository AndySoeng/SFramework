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
	public class apDialog_MacMetalInfo : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_MacMetalInfo s_window = null;

		private apEditor _editor = null;
		
		private apEditor.LANGUAGE _language = apEditor.LANGUAGE.English;
		
		private Texture2D _img_Info = null;
		private string _str_Info = null;
		private string _str_Close = null;
		private string _str_Webpage = null;
		private string _str_ShowStartup = null;
		//private bool _isShowStartup = false;
		private string _url_LeranMore = null;

		private GUIStyle _guiStyle_InfoImg = null;
		private GUIStyle _guiStyle_InfoText = null;

		

		// Show Window
		//------------------------------------------------------------------
		public static void ShowDialog(apEditor editor, apEditor.LANGUAGE language)
		{
			
			CloseDialog();

			if (editor == null)
			{
				return;
			}

			string strTitle = "Troubleshooting on Mac OSX";
			
			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_MacMetalInfo), true, strTitle, true);
			apDialog_MacMetalInfo curTool = curWindow as apDialog_MacMetalInfo;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 520;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, editor._language);
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
		public void Init(apEditor editor, apEditor.LANGUAGE language)
		{
			_editor = editor;
			
			_language = language;
			string basePath = apPathSetting.I.CurrentPath;
			
			//_img_Info = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AnyPortrait/Editor/Images/MacTurnOffMetalEditor.png");//기존
			_img_Info = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath + "Editor/Images/MacTurnOffMetalEditor.png");//변경 20.4.21

			switch (_language)
			{
				case apEditor.LANGUAGE.English:
				default:
					_str_Info = "If the performance of AnyPortrait drops on Mac OSX,\nturn off [Player Settings > Metal Editor Support].";
					_str_Webpage = "Learn more";
					_str_Close = "Close";
					_str_ShowStartup = "Show this Screen at startup";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.Korean:
					_str_Info = "Mac OSX에서 AnyPortrait의 성능이 떨어질 경우,\n[Player Settings > Metal Editor Support]를 해제하세요.";
					_str_Webpage = "자세한 내용 보기";
					_str_Close = "닫기";
					_str_ShowStartup = "시작할 때마다 항상 보임";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-kor";
					break;

				case apEditor.LANGUAGE.French:
					_str_Info = "Si les performances d'AnyPortrait diminuent sous Mac OSX\ndésactivez [Player Settings > Metal Editor Support].";
					_str_Webpage = "Apprendre encore plus";
					_str_Close = "Fermer";
					_str_ShowStartup = "Afficher cet écran au démarrage";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.German:
					_str_Info = "Wenn die Leistung von AnyPortrait unter Mac OS X sinkt,\ndeaktivieren Sie [Player Settings > Metal Editor Support].";
					_str_Webpage = "Erfahren Sie mehr";
					_str_Close = "Schließen";
					_str_ShowStartup = "Zeigen Sie dies beim Start an";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.Spanish:
					_str_Info = "Si el rendimiento de AnyPortrait cae en Mac OSX,\napague [Player Settings > Metal Editor Support].";
					_str_Webpage = "Aprende más";
					_str_Close = "Cerca";
					_str_ShowStartup = "Mostrar esta pantalla al inicio";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.Italian:
					_str_Info = "Se le prestazioni di AnyPortrait diminuiscono su Mac OSX,\ndisattivare [Player Settings > Metal Editor Support].";
					_str_Webpage = "Per saperne di più";
					_str_Close = "Vicino";
					_str_ShowStartup = "Mostra questa schermata all'avvio";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.Danish:
					_str_Info = "Hvis ydelsen til AnyPortrait falder på Mac OSX,\nskal du slukke for [Player Settings > Metal Editor Support].";
					_str_Webpage = "Lær mere";
					_str_Close = "Tæt";
					_str_ShowStartup = "Vis denne skærm i starten";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.Japanese:
					_str_Info = "Mac OSXでAnyPortraitのパフォーマンスが低下する場合は、\n[Player Settings > Metal Editor Support]をオフにします。";
					_str_Webpage = "もっと詳しく知る";
					_str_Close = "閉じる";
					_str_ShowStartup = "起動時にこの画面を表示する";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-jp";
					break;

				case apEditor.LANGUAGE.Chinese_Traditional:
					_str_Info = "如果AnyPortrait在Mac OSX上的表現不佳，\n請關閉[Player Settings > Metal Editor Support].";
					_str_Webpage = "學到更多";
					_str_Close = "關閉";
					_str_ShowStartup = "在啟動時顯示此屏幕";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.Chinese_Simplified:
					_str_Info = "如果AnyPortrait在Mac OSX上无法正常运行，\n请关闭[Player Settings > Metal Editor Support].";
					_str_Webpage = "学到更多";
					_str_Close = "关闭";
					_str_ShowStartup = "在启动时显示此屏幕";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;

				case apEditor.LANGUAGE.Polish:
					_str_Info = "Jeśli wydajność AnyPortrait spada w systemie Mac OSX,\nwyłącz [Player Settings > Metal Editor Support].";
					_str_Webpage = "Ucz się więcej";
					_str_Close = "Zamknąć";
					_str_ShowStartup = "Pokaż to przy starcie";
					_url_LeranMore = "https://www.rainyrizzle.com/apam-macstuttering-eng";
					break;
			}
			
			//_isShowStartup = _editor._macOSXInfoScreenOption_IsShowStartup;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			
			int width = (int)position.width;
			int height = (int)position.height;
			width -= 10;

			int logoWidth = _img_Info.width;
			int logoHeight = _img_Info.height;
			int boxHeight = (int)((float)width * ((float)logoHeight / (float)logoWidth));
			Color prevColor = GUI.backgroundColor;

			//레이아웃
			//- 그림
			//- 설명글
			//- 더 알아보기 버튼
			//- 닫기 버튼
			//- 계속 보기 토글

			bool isClose = false;


			if(_guiStyle_InfoImg == null)
			{
				_guiStyle_InfoImg = new GUIStyle(GUI.skin.box);
				_guiStyle_InfoImg.alignment = TextAnchor.MiddleCenter;
			}

			GUI.backgroundColor = Color.black;
			GUILayout.Box(_img_Info, _guiStyle_InfoImg, GUILayout.Width(width), GUILayout.Height(boxHeight));

			GUI.backgroundColor = prevColor;
			GUILayout.Space(5);

			if(_guiStyle_InfoText == null)
			{
				_guiStyle_InfoText = new GUIStyle(GUI.skin.box);
				_guiStyle_InfoText.alignment = TextAnchor.MiddleCenter;
			}

			//Info 텍스트
			GUILayout.Box(_str_Info, _guiStyle_InfoText, GUILayout.Width(width), GUILayout.Height(40));

			//더 알아보기 버튼
			if(GUILayout.Button(_str_Webpage, GUILayout.Height(20)))
			{	
				Application.OpenURL(_url_LeranMore);
				isClose = true;
			}

			//닫기 버튼
			if(GUILayout.Button(_str_Close, GUILayout.Height(25)))
			{	
				isClose = true;
			}

			//항상 열기 버튼
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_str_ShowStartup, GUILayout.Width(width - (12 + 20)), GUILayout.Height(20));
			bool isShow = EditorGUILayout.Toggle(_editor._macOSXInfoScreenOption_IsShowStartup, GUILayout.Width(20), GUILayout.Height(20));
			if(_editor._macOSXInfoScreenOption_IsShowStartup != isShow)
			{
				_editor._macOSXInfoScreenOption_IsShowStartup = isShow;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.EndHorizontal();


			if(isClose)
			{
				CloseDialog();
			}
		}
	}
}