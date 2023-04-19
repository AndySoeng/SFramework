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

	public class apEditorTroubleShooting : EditorWindow
	{
		//private static apEditor2 s_window = null;

		//--------------------------------------------------------------
		//우선순위가 10 이상 차이가 나면 구분자가 생긴다.

		[MenuItem("Window/AnyPortrait/Reset Editor", false, 21)]
		public static void ShowWindow()
		{
			apEditor.CloseEditor();
			Debug.LogWarning("AnyPortrait Editor is Closed.");

			bool isCurCppPlugin = EditorPrefs.GetBool("AnyPortrait_UseCPPPlugin", true);
			if(isCurCppPlugin)
			{
				EditorPrefs.SetBool("AnyPortrait_UseCPPPlugin", false);
				Debug.LogWarning("The editor's Update mode has been changed from [Accelerated Mode] to a [Compatible Mode].");
			}
		}

		//추가 22.7.13 : 씬의 전체 apPortrait의 Mesh를 갱신하는 메뉴
		//--------------------------------------------------------------
		[MenuItem("Window/AnyPortrait/Refresh All Meshes", false, 41)]
		public static void RefrehsAllScenePortraits()
		{
			try
			{
				string strTitle = "";
				string strBody = "";
				string strOkay = "";
				string strCancel = "";
				if (Application.isPlaying || apEditor.IsOpen())
				{
					//게임이 실행중이거나 에디터가 열려 있을 때
					GetRefreshMeshesText(false, 0, ref strTitle, ref strBody, ref strOkay, ref strCancel);

					//실행 불가 안내
					EditorUtility.DisplayDialog(strTitle, strBody, strOkay);
				}
				else
				{
					apPortrait[] portraits = FindObjectsOfType<apPortrait>();
					int nPortraits = portraits != null ? portraits.Length : 0;

					GetRefreshMeshesText(true, nPortraits, ref strTitle, ref strBody, ref strOkay, ref strCancel);

					if(nPortraits > 0)
					{
						bool isResult = EditorUtility.DisplayDialog(strTitle, strBody, strOkay, strCancel);
						if(isResult)
						{
							for (int i = 0; i < nPortraits; i++)
							{
								portraits[i].OnMeshResetInEditor();
							}
						}
					}
					else
					{
						//대상이 없다.
						EditorUtility.DisplayDialog(strTitle, strBody, strOkay);
					}
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : Failed to refresh\n" + ex.ToString());
			}
		}

		private static void GetRefreshMeshesText(	bool isAvailable,
													int nPortraits,
													ref string strTitle,
													ref string strBody,
													ref string strOkay,
													ref string strCancel)
		{
			

			apEditor.LANGUAGE language = GetLanguage();

			//기본 확인, 취소 + 타이틀
			switch (language)
			{
				case apEditor.LANGUAGE.English://영어
					strTitle = "Refresh All Meshes";
					strOkay = "Okay";
					strCancel = "Cancel";
					break;

				case apEditor.LANGUAGE.Korean://한국어
					strTitle = "모든 메시 갱신";
					strOkay = "확인";
					strCancel = "취소";
					break;
				case apEditor.LANGUAGE.French://프랑스어
					strTitle = "Actualiser tous les maillages";
					strOkay = "Oui";
					strCancel = "Annuler";
					break;

				case apEditor.LANGUAGE.German://독일어
					strTitle = "Alle Netze aktualisieren";
					strOkay = "Okay";
					strCancel = "Stornieren";
					break;

				case apEditor.LANGUAGE.Spanish://스페인어
					strTitle = "Actualizar todas las mallas";
					strOkay = "Correcto";
					strCancel = "Anular";
					break;

				case apEditor.LANGUAGE.Italian://이탈리아어
					strTitle = "Aggiorna tutte le mesh";
					strOkay = "Va bene";
					strCancel = "Annulla";
					break;

				case apEditor.LANGUAGE.Danish://덴마크어
					strTitle = "Opdater alle masker";
					strOkay = "Okay";
					strCancel = "Afbestille";
					break;

				case apEditor.LANGUAGE.Japanese://일본어
					strTitle = "すべてのメッシュを更新";
					strOkay = "はい";
					strCancel = "いいえ";
					break;

				case apEditor.LANGUAGE.Chinese_Traditional://중국어-번체
					strTitle = "刷新所有網格";
					strOkay = "確認";
					strCancel = "取消";
					break;

				case apEditor.LANGUAGE.Chinese_Simplified://중국어-간체
					strTitle = "刷新所有网格";
					strOkay = "确认";
					strCancel = "取消";
					break;

				case apEditor.LANGUAGE.Polish://폴란드어
					strTitle = "Odśwież wszystkie siatki";
					strOkay = "Tak";
					strCancel = "Anuluj";
					break;

				default:
					strTitle = "Refresh the meshes";
					strOkay = "Okay";
					strCancel = "Cancel";
					break;
			}

			if (isAvailable)
			{
				if (nPortraits > 0)
				{
					switch (language)
					{
						case apEditor.LANGUAGE.English:		strBody = "Do you want to refresh meshes of " + nPortraits + " Portraits existing in the current scene in a batch?"; break;
						case apEditor.LANGUAGE.Korean:		strBody = "현재 씬에 존재하는 " + nPortraits + "개의 Portrait의 메시들을 일괄적으로 갱신하겠습니까?"; break;
						case apEditor.LANGUAGE.French:		strBody = "Souhaitez-vous rafraîchir les maillages de " + nPortraits + " portraits existant dans la scène actuelle dans un lot ?"; break;
						case apEditor.LANGUAGE.German:		strBody = "Möchten Sie Meshes von " + nPortraits + " Porträts, die in der aktuellen Szene vorhanden sind, in einem Stapel aktualisieren?"; break;
						case apEditor.LANGUAGE.Spanish:		strBody = "¿Desea actualizar mallas de " + nPortraits + " Retratos existentes en la escena actual en un lote?"; break;
						case apEditor.LANGUAGE.Italian:		strBody = "Vuoi aggiornare le mesh di " + nPortraits + " Ritratti esistenti nella scena corrente in un batch?"; break;
						case apEditor.LANGUAGE.Danish:		strBody = "Vil du opdatere masker af " + nPortraits + " portrætter, der findes i den aktuelle scene i en batch?"; break;
						case apEditor.LANGUAGE.Japanese:	strBody = "現在のシーンに存在する" + nPortraits + "つのポートレートのメッシュをバッチで更新しますか？"; break;
						case apEditor.LANGUAGE.Chinese_Traditional:	strBody = "是否要批量刷新當前場景中存在的" + nPortraits + "幅肖像的網格？"; break;
						case apEditor.LANGUAGE.Chinese_Simplified:	strBody = "是否要批量刷新当前场景中存在的" + nPortraits + "幅肖像的网格？"; break;
						case apEditor.LANGUAGE.Polish:		strBody = "Czy chcesz odświeżyć siatki " + nPortraits + " portretów istniejących w bieżącej scenie w partii?"; break;
						default:							strBody = "Do you want to refresh meshes of " + nPortraits + " Portraits existing in the current scene in a batch?"; break;
					}
				}
				else
				{
					switch (language)
					{
						case apEditor.LANGUAGE.English:		strBody = "The target Portrait cannot be found in the current scene."; break;
						case apEditor.LANGUAGE.Korean:		strBody = "대상이 되는 Portrait를 현재 씬에서 찾을 수 없습니다."; break;
						case apEditor.LANGUAGE.French:		strBody = "Le portrait cible est introuvable dans la scène actuelle."; break;
						case apEditor.LANGUAGE.German:		strBody = "Das Zielporträt kann in der aktuellen Szene nicht gefunden werden."; break;
						case apEditor.LANGUAGE.Spanish:		strBody = "El retrato de destino no se puede encontrar en la escena actual."; break;
						case apEditor.LANGUAGE.Italian:		strBody = "Non è possibile trovare il ritratto di destinazione nella scena corrente."; break;
						case apEditor.LANGUAGE.Danish:		strBody = "Målportrættet kan ikke findes i den aktuelle scene."; break;
						case apEditor.LANGUAGE.Japanese:	strBody = "現在のシーンでターゲットのポートレートが見つかりません。"; break;
						case apEditor.LANGUAGE.Chinese_Traditional:		strBody = "當前場景中找不到目標人像。"; break;
						case apEditor.LANGUAGE.Chinese_Simplified:		strBody = "当前场景中找不到目标人像。"; break;
						case apEditor.LANGUAGE.Polish:		strBody = "Nie można znaleźć docelowego Portretu w bieżącej scenie."; break;
						default:							strBody = "The target Portrait cannot be found in the current scene."; break;
					}
				}
			}
			else
			{
				//실행 불가능하다
				switch (language)
					{
					case apEditor.LANGUAGE.English:		strBody = "This function cannot be executed while the game is running or AnyPortrait editor is running."; break;
					case apEditor.LANGUAGE.Korean:		strBody = "게임이 실행 중이거나 AnyPortrait 에디터가 실행 중일때는 이 기능을 실행할 수 없습니다."; break;
					case apEditor.LANGUAGE.French:		strBody = "Cette fonction ne peut pas être exécutée lorsque le jeu est en cours d'exécution ou que l'éditeur AnyPortrait est en cours d'exécution."; break;
					case apEditor.LANGUAGE.German:		strBody = "Diese Funktion kann nicht ausgeführt werden, während das Spiel läuft oder der AnyPortrait-Editor läuft."; break;
					case apEditor.LANGUAGE.Spanish:		strBody = "Esta función no se puede ejecutar mientras se ejecuta el juego o el editor AnyPortrait."; break;
					case apEditor.LANGUAGE.Italian:		strBody = "Questa funzione non può essere eseguita mentre il gioco è in esecuzione o l'editor AnyPortrait è in esecuzione."; break;
					case apEditor.LANGUAGE.Danish:		strBody = "Denne funktion kan ikke udføres, mens spillet kører, eller AnyPortrait-editoren kører."; break;
					case apEditor.LANGUAGE.Japanese:	strBody = "この関数は、ゲームの実行中またはAnyPortraitエディターの実行中は実行できません。"; break;
					case apEditor.LANGUAGE.Chinese_Traditional:	strBody = "此功能在遊戲運行或 AnyPortrait 編輯器運行時無法執行。"; break;
					case apEditor.LANGUAGE.Chinese_Simplified:	strBody = "此功能在游戏运行或 AnyPortrait 编辑器运行时无法执行。"; break;
					case apEditor.LANGUAGE.Polish:		strBody = "Tej funkcji nie można wykonać, gdy gra jest uruchomiona lub edytor AnyPortrait jest uruchomiony."; break;
					default:							strBody = "This function cannot be executed while the game is running or AnyPortrait editor is running."; break;
				}
			}
		}



		//--------------------------------------------------------------
		public static apEditor.LANGUAGE GetLanguage()
		{
			return (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
		}
		//public static bool IsKorean()
		//{
		//	apEditor.LANGUAGE language = (apEditor.LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)apEditor.LANGUAGE.English);
		//	return language == apEditor.LANGUAGE.Korean;
		//}



		[MenuItem("Window/AnyPortrait/Homepage", false, 61)]
		public static void OpenHomepage()
		{
			Application.OpenURL("https://www.rainyrizzle.com/");
		}

		[MenuItem("Window/AnyPortrait/Getting Started", false, 62)]
		public static void OpenGettingStarted()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				//url = "https://www.rainyrizzle.com/ap-gettingstarted-kor";
				url = "https://rainyrizzle.github.io/kr/GettingStarted.html";//주소 변경
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				//url = "https://www.rainyrizzle.com/ap-gettingstarted-jp";
				url = "https://rainyrizzle.github.io/jp/GettingStarted.html";//주소 변경
			}
			else
			{
				//url = "https://www.rainyrizzle.com/ap-gettingstarted-eng";
				url = "https://rainyrizzle.github.io/en/GettingStarted.html";//주소 변경
			}
			Application.OpenURL(url);
		}


		[MenuItem("Window/AnyPortrait/Manual", false, 63)]
		public static void OpenAdvancedManul()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				//url = "https://www.rainyrizzle.com/ap-advanced-kor";
				url = "https://rainyrizzle.github.io/kr/AdManual.html";//주소 변경
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				//url = "https://www.rainyrizzle.com/ap-advanced-jp";
				url = "https://rainyrizzle.github.io/jp/AdManual.html";//주소 변경
			}
			else
			{
				//url = "https://www.rainyrizzle.com/ap-advanced-eng";
				url = "https://rainyrizzle.github.io/en/AdManual.html";//주소 변경
			}
			Application.OpenURL(url);
		}

		[MenuItem("Window/AnyPortrait/Scripting", false, 64)]
		public static void OpenScripting()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				//url = "https://www.rainyrizzle.com/ap-scripting-kor";
				url = "https://rainyrizzle.github.io/kr/Script.html";//주소 변경
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				//url = "https://www.rainyrizzle.com/ap-scripting-jp";
				url = "https://rainyrizzle.github.io/jp/Script.html";//주소 변경				
			}
			else
			{
				//url = "https://www.rainyrizzle.com/ap-scripting-eng";
				url = "https://rainyrizzle.github.io/en/Script.html";//주소 변경
			}
			Application.OpenURL(url);
		}

		//추가 21.10.11
		[MenuItem("Window/AnyPortrait/Video Tutorials", false, 65)]
		public static void OpenVideoTutorials()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				url = "https://www.rainyrizzle.com/ap-videotutorial-kor";
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				url = "https://www.rainyrizzle.com/ap-videotutorial-jp";
			}
			else
			{
				url = "https://www.rainyrizzle.com/ap-videotutorial-eng";
			}
			Application.OpenURL(url);
		}

		//이 기능은 뺍시다.
		//[MenuItem("Window/AnyPortrait/Submit a Survey (Demo)", false, 81)]
		//public static void OpenSubmitASurvey()
		//{
		//	Application.OpenURL("https://goo.gl/forms/xZqTaXTesYq6v1Ba2");
		//}


		[MenuItem("Window/AnyPortrait/Report a Bug or Suggestion", false, 101)]
		public static void OpenReportABug()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-kor";
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-jp";
			}
			else
			{
				url = "https://www.rainyrizzle.com/anyportrait-report-eng";
			}
			Application.OpenURL(url);
		}

		[MenuItem("Window/AnyPortrait/Ask a Question", false, 102)]
		public static void OpenAskQuestion()
		{
			string url = "";

			apEditor.LANGUAGE language = GetLanguage();

			if(language == apEditor.LANGUAGE.Korean)
			{
				url = "https://www.rainyrizzle.com/anyportrait-qna-kor";
			}
			else if(language == apEditor.LANGUAGE.Japanese)
			{
				url = "https://www.rainyrizzle.com/anyportrait-qna-jp";
			}
			else
			{
				url = "https://www.rainyrizzle.com/anyportrait-qna-eng";
			}
			Application.OpenURL(url);
		}

		[MenuItem("Window/AnyPortrait/Forum", false, 103)]
		public static void OpenForum()
		{
			Application.OpenURL("https://www.rainyrizzle.com/ap-forum");
		}


		[MenuItem("Window/AnyPortrait/Open Asset Store Page", false, 121)]
		public static void OpenAssetStorePage()
		{
			apEditorUtil.OpenAssetStorePage();
		}
		//[MenuItem("Window/AnyPortrait/Restart Editor", false, 101)]
		//public static void RestartEditor()
		//{
		//	EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
		//}

		//[MenuItem("Window/AnyPortrait/Install Package Force", false, 102)]
		//public static void RestartEditor()
		//{
		//	//EditorApplication.OpenProject(System.IO.Directory.GetCurrentDirectory());
		//	apPluginUtil.I.CheckAndInstallCPPDLLPackage(true);

		//}
	}

}