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
using System.Text;
using System.IO;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	public class apDialog_PathSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_PathSetting s_window = null;

		private apEditor.LANGUAGE _language = apEditor.LANGUAGE.English;

		private string _text_SetEditorPath = "";
		private string _text_SetPathButton = "";
		private string _text_UseDefaultPathButton = "";
		private string _text_Close = "";
		private string _text_CurrentPah = "";
		private string _text_Info_Title = "";
		private string _text_Info_NoPackage = "";
		private string _text_Info_NoAssetFolder = "";
		private string _text_Info_Okay = "";

		
		// Show Window
		//------------------------------------------------------------------
		[MenuItem("Window/AnyPortrait/Change Installation Path", false, 22)]
		public static void ShowDialog()
		{
			CloseDialog();

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_PathSetting), true, "Installation Path", true);
			apDialog_PathSetting curTool = curWindow as apDialog_PathSetting;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				int height = 165;
				s_window = curTool;
				s_window.position = new Rect(100, 100, width, height);
				s_window.Init();
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
		public void Init()
		{	
			//이전
			//_text_CurrentPah = apPathSetting.I.Load();

			//변경 21.10.4 : 함수 변경
			_text_CurrentPah = apPathSetting.I.RefreshAndGetBasePath(true);//강제로 로드후 갱신

			
			_language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);

			switch (_language)
			{
				case apEditor.LANGUAGE.English:
					_text_SetEditorPath = "Installed Path of AnyPortrait Package";
					_text_SetPathButton = "Change Path";
					_text_UseDefaultPathButton = "Use Default Path";
					_text_Info_Title = "Invalid Path";
					_text_Info_NoPackage = "The selected path is not where the AnyPortrait package is located.";
					_text_Info_NoAssetFolder = "The path for AnyPortrait must be inside the Assets folder.";
					
					_text_Close = "Close";
					_text_Info_Okay = "Okay";
					break;

				case apEditor.LANGUAGE.Korean:
					_text_SetEditorPath = "AnyPortrait 패키지가 설치된 경로";
					_text_SetPathButton = "경로 변경";
					_text_UseDefaultPathButton = "기본 경로로 복구";
					_text_Info_Title = "잘못된 경로";
					_text_Info_NoPackage = "선택된 경로에 AnyPortrait가 설치되지 않았습니다.";
					_text_Info_NoAssetFolder = "AnyPortrait는 Assets 폴더의 하위에 위치해야 합니다.";

					_text_Close = "닫기";
					_text_Info_Okay = "확인";
					break;

				case apEditor.LANGUAGE.French:
					_text_SetEditorPath = "Chemin installé du package AnyPortrait";
					_text_SetPathButton = "Changer de chemin";
					_text_UseDefaultPathButton = "Utiliser le chemin par défaut";
					_text_Info_Title = "Chemin invalide";
					_text_Info_NoPackage = "Le chemin sélectionné n'est pas l'emplacement du package AnyPortrait.";
					_text_Info_NoAssetFolder = "Le chemin d'accès à AnyPortrait doit se trouver dans le dossier Assets.";

					_text_Close = "Fermer";
					_text_Info_Okay = "Oui";
					break;

				case apEditor.LANGUAGE.German:
					_text_SetEditorPath = "Installierter Pfad des AnyPortrait-Pakets";
					_text_SetPathButton = "Pfad ändern";
					_text_UseDefaultPathButton = "Standardpfad verwenden";
					_text_Info_Title = "Ungültigen Pfad";
					_text_Info_NoPackage = "Der ausgewählte Pfad befindet sich nicht dort, wo sich das AnyPortrait-Paket befindet.";
					_text_Info_NoAssetFolder = "Der Pfad für AnyPortrait muss sich im Ordner [Assets] befinden.";
					
					_text_Close = "Schließen";
					_text_Info_Okay = "Okay";
					break;

				case apEditor.LANGUAGE.Spanish:
					_text_SetEditorPath = "Ruta instalada del paquete AnyPortrait";
					_text_SetPathButton = "Cambiar ruta";
					_text_UseDefaultPathButton = "Usar ruta predeterminada";
					_text_Info_Title = "Ruta no válida";
					_text_Info_NoPackage = "La ruta seleccionada no es donde se encuentra el paquete AnyPortrait.";
					_text_Info_NoAssetFolder = "La ruta para AnyPortrait debe estar dentro de la carpeta Assets.";

					_text_Close = "Cerca";
					_text_Info_Okay = "Correcto";
					break;

				case apEditor.LANGUAGE.Italian:
					_text_SetEditorPath = "Percorso installato del pacchetto AnyPortrait";
					_text_SetPathButton = "Cambia percorso";
					_text_UseDefaultPathButton = "Usa percorso predefinito";
					_text_Info_Title = "Percorso non valido";
					_text_Info_NoPackage = "Il percorso selezionato non è dove si trova il pacchetto AnyPortrait.";
					_text_Info_NoAssetFolder = "Il percorso per AnyPortrait deve trovarsi nella cartella Assets.";

					_text_Close = "Vicino";
					_text_Info_Okay = "Va bene";
					break;

				case apEditor.LANGUAGE.Danish:
					_text_SetEditorPath = "Installeret sti af AnyPortrait-pakken";
					_text_SetPathButton = "Skift sti";
					_text_UseDefaultPathButton = "Brug standardsti";
					_text_Info_Title = "Ugyldig sti";
					_text_Info_NoPackage = "Den valgte sti er ikke, hvor AnyPortrait-pakken er placeret.";
					_text_Info_NoAssetFolder = "Stien til AnyPortrait skal være inde i mappen Assets.";

					_text_Close = "Tæt";
					_text_Info_Okay = "Okay";
					break;

				case apEditor.LANGUAGE.Japanese:
					_text_SetEditorPath = "AnyPortraitパッケージのインストールパス";
					_text_SetPathButton = "パスを変更";
					_text_UseDefaultPathButton = "デフォルトパスを使用";
					_text_Info_Title = "無効なパス";
					_text_Info_NoPackage = "選択したパスは、AnyPortraitパッケージの場所ではありません。";
					_text_Info_NoAssetFolder = "AnyPortraitのパスはAssetsフォルダー内になければなりません。";

					_text_Close = "閉じる";
					_text_Info_Okay = "はい";
					break;

				case apEditor.LANGUAGE.Chinese_Traditional:
					_text_SetEditorPath = "AnyPortrait軟件包的安裝路徑";
					_text_SetPathButton = "變更路徑";
					_text_UseDefaultPathButton = "使用默認路徑";
					_text_Info_Title = "無效的路徑";
					_text_Info_NoPackage = "所選路徑不在AnyPortrait程序包所在的位置。";
					_text_Info_NoAssetFolder = "AnyPortrait的路徑必須在Assets文件夾中。";

					_text_Close = "關閉";
					_text_Info_Okay = "確認";
					break;

				case apEditor.LANGUAGE.Chinese_Simplified:
					_text_SetEditorPath = "AnyPortrait软件包的安装路径";
					_text_SetPathButton = "变更路径";
					_text_UseDefaultPathButton = "使用默认路径";
					_text_Info_Title = "无效的路径";
					_text_Info_NoPackage = "所选路径不在AnyPortrait程序包所在的位置。";
					_text_Info_NoAssetFolder = "AnyPortrait的路径必须在Assets文件夹中。";

					_text_Close = "关闭";
					_text_Info_Okay = "确认";
					break;

				case apEditor.LANGUAGE.Polish:
					_text_SetEditorPath = "Zainstalowana ścieżka pakietu AnyPortrait";
					_text_SetPathButton = "Zmień ścieżkę";
					_text_UseDefaultPathButton = "Użyj domyślnej ścieżki";
					_text_Info_Title = "Niewłaściwa ścieżka";
					_text_Info_NoPackage = "Wybrana ścieżka nie znajduje się w miejscu, w którym znajduje się pakiet AnyPortrait.";
					_text_Info_NoAssetFolder = "Ścieżka do AnyPortrait musi znajdować się w folderze Assets.";

					_text_Close = "Zamknąć";
					_text_Info_Okay = "Tak";
					break;

				default:
					_text_SetEditorPath = "Installed Path of AnyPortrait Package";
					_text_SetPathButton = "Change Path";
					_text_UseDefaultPathButton = "Use Default Path";
					_text_Close = "Close";
					_text_Info_Title = "Invalid Path";
					_text_Info_NoPackage = "The selected path is not where the AnyPortrait package is located.";
					_text_Info_NoAssetFolder = "The path for AnyPortrait must be inside the Assets folder.";
					_text_Info_Okay = "Okay";
					break;
			}

			//_text_CurrentPah = apPathSetting.Load();
		}

		void OnGUI()
		{
			try
			{
				int width = (int)position.width;
				int height = (int)position.height;
				width -= 10;

				EditorGUILayout.LabelField(_text_SetEditorPath);
				GUILayout.Space(10);
				EditorGUILayout.TextField(_text_CurrentPah);

				GUILayout.Space(5);
				if (GUILayout.Button(_text_SetPathButton, GUILayout.Height(40)))
				{
					//TODO
					string strRootPath = EditorUtility.OpenFolderPanel("Set Root Folder of AnyPortrait", "", "");
					if (!string.IsNullOrEmpty(strRootPath))
					{
						DirectoryInfo di_AssetPath = new DirectoryInfo(Application.dataPath);
						DirectoryInfo di_SelectedPath = new DirectoryInfo(strRootPath);

						//Debug.LogError("Selected Path [" + strRootPath + "]");
						//Debug.LogError("DI Path > Assets : " + di_AssetPath.FullName);
						//Debug.LogError("DI Path > Selected : " + di_SelectedPath.FullName);

						string str_AssetPath = di_AssetPath.FullName;
						string str_SelectedPath = di_SelectedPath.FullName;

						str_AssetPath = str_AssetPath.Replace("\\", "/");
						if (!str_AssetPath.EndsWith("/"))
						{
							str_AssetPath += "/";
						}
						str_SelectedPath = str_SelectedPath.Replace("\\", "/");
						if (!str_SelectedPath.EndsWith("/"))
						{
							str_SelectedPath += "/";
						}

						if (!str_SelectedPath.EndsWith("/AnyPortrait/"))
						{
							//AnyPortrait 폴더가 아니다.
							//"Path Setting", "The selected folder is not where the AnyPortrait package is located.", "Okay"
							EditorUtility.DisplayDialog(_text_Info_Title, _text_Info_NoPackage, _text_Info_Okay);
						}
						else if (str_SelectedPath.Contains(str_AssetPath))
						{
							//경로가 포함된게 맞다.
							//상대 경로를 만들자
							string relativePath = str_SelectedPath.Substring(str_AssetPath.Length);
							relativePath = "Assets/" + relativePath;


							apPathSetting.I.Save(relativePath);
							
							//이전
							//_text_CurrentPah = apPathSetting.I.Load();
							//apEditorUtil.SetPackagePath(apPathSetting.I.CurrentPath);//추가 21.10.3 : 버그 수정. 루트 변경 후 에디터 유틸 변수 갱신해야한다.

							//변경 21.10.4 : 함수 변경
							_text_CurrentPah = apPathSetting.I.RefreshAndGetBasePath(true);//강제로 로드후 갱신

							//Debug.LogError("Relative Path : " + relativePath);
						}
						else
						{
							//Assets 폴더 내부에 있어야 한다.
							//"Path Setting", "The path for AnyPortrait must be inside the Assets folder.", "Okay"
							EditorUtility.DisplayDialog(_text_Info_Title, _text_Info_NoPackage, _text_Info_NoAssetFolder);
						}

					}

					apEditorUtil.ReleaseGUIFocus();

				}
				if (GUILayout.Button(_text_UseDefaultPathButton, GUILayout.Height(25)))
				{
					apPathSetting.I.SetDefaultPath();//기본값으로 변경 (저장도 한다.)

					//이전
					//_text_CurrentPah = apPathSetting.I.Load();
					//apEditorUtil.SetPackagePath(apPathSetting.I.CurrentPath);//추가 21.10.3 : 버그 수정. 루트 변경 후 에디터 유틸 변수 갱신해야한다.

					//변경 21.10.4 : 함수 변경
					_text_CurrentPah = apPathSetting.I.RefreshAndGetBasePath(true);//강제로 로드후 갱신

					apEditorUtil.ReleaseGUIFocus();
				}
				GUILayout.Space(10);
				if (GUILayout.Button(_text_Close, GUILayout.Height(25)))
				{
					CloseDialog();
				}
			}
			catch (Exception ex)
			{
				//추가 21.3.17 : Try-Catch를 추가했다. Mac에서 에러가 발생할 확률이 높기 때문
				Debug.LogError("AnyPortrait : Exception occurs : " + ex);
			}
		}
	}
}