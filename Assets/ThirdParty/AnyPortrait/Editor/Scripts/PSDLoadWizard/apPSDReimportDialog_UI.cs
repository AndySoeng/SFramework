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
using System.IO;
using System.Collections.Generic;
using Ntreev.Library.Psd;
using System.Threading;

using AnyPortrait;

namespace AnyPortrait
{
	//public enum RELOAD_STEP
	//	{
	//		Step1_SelectPSDSet,//PSD Set을 선택하거나 생성 (또는 삭제)
	//		Step2_FileLoadAndSelectMeshGroup,//PSD Set과 연결된 PSD 파일/MeshGroup/Atlas Texture Data를 선택 + 크기 오프셋 설정
	//		Step3_LinkLayerToTransform,//레이어 정보와 Mesh/MeshGroup Transform 연결 <GUI - 메시+레이어>
	//		Step4_ModifyOffset,//레이어의 위치 수정 <GUI - 메시+레이어>
	//		Step5_AtlasSetting,//아틀라스 굽기 (+PSD에서 삭제되면서 TextureData 갱신에 포함되지 않는 경우 포함) <GUI - Atlas>
	//	}
	public partial class apPSDReimportDialog : EditorWindow
	{
		// GUI - Center (Step 별로)
		
		//--------------------------------------------------------------------
		// Step 1 : PSD Set 선택하기
		//--------------------------------------------------------------------
		private void GUI_Center_1_SelectPSDSet(int width, int height, Rect centerRect)
		{
			//PSD Set을 선택하는 화면
			//좌우 2등분
			//왼쪽 : PSD 리스트와 생성 버튼
			//오른쪽 : 기본 정보와 파일 경로 + 삭제
			int margin = 4;
			int width_Half = (width - margin) / 2;

			Color prevColor = GUI.backgroundColor;
			
			//변경 19.11.20
			if(_guiContent_PSDSetIcon == null)
			{
				_guiContent_PSDSetIcon = apGUIContentWrapper.Make(_editor.ImageSet.Get(apImageSet.PRESET.PSD_Set));
			}

			if(_guiContent_PSDSecondaryIcon == null)
			{
				_guiContent_PSDSecondaryIcon = apGUIContentWrapper.Make(_editor.ImageSet.Get(apImageSet.PRESET.PSD_SetSecondary));
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Half, height), "");
			GUI.Box(new Rect(centerRect.xMin + width_Half + margin, centerRect.yMin, width_Half, height), "");

			//--------------------------------------
			// <1열 : PSD Set 리스트 또는 생성하기 >			
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Half), GUILayout.Height(height));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SelectPSDSet), GUILayout.Width(width_Half));//Select PSD Set

			GUILayout.Space(5);
			_scroll_Step1_Left = EditorGUILayout.BeginScrollView(_scroll_Step1_Left, false, true, GUILayout.Width(width_Half), GUILayout.Height(height - 90));
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Half - 20));

			int height_PSDSet = 20;
			GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
			guiStyle_Label.alignment = TextAnchor.MiddleLeft;
			guiStyle_Label.margin = GUI.skin.button.margin;

			GUIStyle guiStyle_TextBox = new GUIStyle(GUI.skin.textField);
			guiStyle_TextBox.alignment = TextAnchor.MiddleLeft;
			guiStyle_TextBox.margin = GUI.skin.button.margin;

			
			string str_NoFile = "< " + _editor.GetText(TEXT.DLG_PSD_NoFile) + " >";
			string str_NoPath = "< " + _editor.GetText(TEXT.DLG_PSD_NoPath) + " >";
			string str_InvalidFile = "< " + _editor.GetText(TEXT.DLG_PSD_InvalidFile) + " >";
			string str_InvalidPath = "< " + _editor.GetText(TEXT.DLG_PSD_InvalidPath) + " >";
			string str_Select = _editor.GetText(TEXT.DLG_PSD_Select);
			string str_Selected = _editor.GetText(TEXT.DLG_PSD_Selected);

			int nPSDSets = _portrait._bakedPsdSets != null ? _portrait._bakedPsdSets.Count : 0;
			int nPSDSecondary = _portrait._bakedPsdSecondarySet != null ? _portrait._bakedPsdSecondarySet.Count : 0;

			if (nPSDSets > 0)
			{
				apPSDSet psdSet = null;
				for (int i = 0; i < nPSDSets; i++)
				{
					psdSet = _portrait._bakedPsdSets[i];
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Half - 20), GUILayout.Height(height_PSDSet));
					GUILayout.Space(5);


					string fileName = str_NoFile;
					string filePath = str_NoPath;

					FileInfo fi = null;
					if (_psdSet2FileInfo.ContainsKey(psdSet))
					{
						fi = _psdSet2FileInfo[psdSet];

						if (fi.Exists)
						{
							fileName = fi.Name;
							filePath = fi.FullName;
						}
						else
						{
							fileName = str_InvalidFile;
							filePath = str_InvalidPath;
						}
					}
					
					EditorGUILayout.LabelField(_guiContent_PSDSetIcon.Content, guiStyle_Label, GUILayout.Width(height_PSDSet), GUILayout.Height(height_PSDSet));
					GUILayout.Space(5);
					EditorGUILayout.LabelField(fileName, guiStyle_Label, GUILayout.Width(200), GUILayout.Height(height_PSDSet));
					EditorGUILayout.TextField(filePath, guiStyle_TextBox, GUILayout.Width(width_Half - (5 + height_PSDSet + 5 + 220 + 5 + 100 + 20)), GUILayout.Height(height_PSDSet));
					GUILayout.Space(5);
					if (apEditorUtil.ToggledButton_2Side(str_Selected, str_Select, _selectedPSDSet == psdSet, true, 100, height_PSDSet))
					{
						if (psdSet != _selectedPSDSet)
						{
							//PSD Set을 교체한다
							SelectPSDSet(psdSet);
						}

					}
					EditorGUILayout.EndHorizontal();
				}
			}
			
			
			if(nPSDSets > 0 && nPSDSecondary > 0)
			{
				GUILayout.Space(20);
			}

			//추가 : 보조 PSD 정보를 출력하고 선택해야한다.
			if(nPSDSecondary > 0)
			{	
				apPSDSecondarySet psdSecondary = null;
				for (int i = 0; i < nPSDSecondary; i++)
				{
					psdSecondary = _portrait._bakedPsdSecondarySet[i];
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Half - 20), GUILayout.Height(height_PSDSet));
					GUILayout.Space(5);
					string fileName = str_NoFile;
					string filePath = str_NoPath;

					FileInfo fi = null;
					if (_psdSecondary2FileInfo.ContainsKey(psdSecondary))
					{
						fi = _psdSecondary2FileInfo[psdSecondary];

						if (fi.Exists)
						{
							fileName = fi.Name;
							filePath = fi.FullName;
						}
						else
						{
							fileName = str_InvalidFile;
							filePath = str_InvalidPath;
						}
					}


					EditorGUILayout.LabelField(_guiContent_PSDSecondaryIcon.Content, guiStyle_Label, GUILayout.Width(height_PSDSet), GUILayout.Height(height_PSDSet));
					GUILayout.Space(5);
					EditorGUILayout.LabelField(fileName, guiStyle_Label, GUILayout.Width(200), GUILayout.Height(height_PSDSet));
					EditorGUILayout.TextField(filePath, guiStyle_TextBox, GUILayout.Width(width_Half - (5 + height_PSDSet + 5 + 220 + 5 + 100 + 20)), GUILayout.Height(height_PSDSet));
					GUILayout.Space(5);
					if (apEditorUtil.ToggledButton_2Side(str_Selected, str_Select, _selectedPSDSecondary == psdSecondary, true, 100, height_PSDSet))
					{
						//PSD Secondary Set을 교체한다
						SelectPSDSecondary(psdSecondary, true);
					}
					EditorGUILayout.EndHorizontal();
				}
			}

			GUILayout.Space(height + 200);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AddNewPSDImportSet), GUILayout.Width(width_Half - 10), GUILayout.Height(35)))//"Add New PSD Import Set"
			{
				//_portrait._bakedPsdSets.Add(new apPSDSet())
				_editor.Controller.AddNewPSDSet(true);
			}

			EditorGUILayout.EndVertical();
			//--------------------------------------



			GUILayout.Space(margin);


			//----------------------------------------------------------			
			// <오른쪽 UI : 선택한 PSD Set 정보 + 파일 경로 설정 >
			//----------------------------------------------------------

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Half), GUILayout.Height(height));
			
			//(1) 선택한게 기본 PSD Set이라면
			if (_selectedPSDSet != null)
			{
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SelectedPSDSet), GUILayout.Width(width_Half));//Selected PSD Set
				GUILayout.Space(5);
				//- 파일 이름 / 경로 (변경 가능)
				//- 로드하기 버튼
				//- 이미지 크기 (저장된)
				//- 레이어 리스트
				//- Bake 정보
				//- 데이터 삭제

				FileInfo fi = null;
				if(_psdSet2FileInfo.ContainsKey(_selectedPSDSet))
				{
					fi = _psdSet2FileInfo[_selectedPSDSet];
				}

				string selectedPSDFileName = "< " + _editor.GetText(TEXT.DLG_PSD_NoPSDFile) + " >";//"< No PSD File >";
				string selectedPSDPath = "< " + _editor.GetText(TEXT.DLG_PSD_NoPSDFile) + " >"; //"< No PSD File >";
				if(fi != null && fi.Exists)
				{
					selectedPSDFileName = fi.Name;
					selectedPSDPath = fi.FullName;
				}

				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDFileName) + " : " + selectedPSDFileName, GUILayout.Width(width_Half));//PSD File Name
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Half), GUILayout.Height(20));
				GUILayout.Space(5);
				EditorGUILayout.TextField(selectedPSDPath, guiStyle_TextBox, GUILayout.Width(width_Half - (90 + 25)));
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(90), GUILayout.Height(20)))//Change
				{
					//PSD 여는 Dialog
					try
					{
						//변경 21.3.1 : 이전 파일이 있는 디렉토리 경로를 가져오자
						string prevFileDir = "";

						if (!string.IsNullOrEmpty(_selectedPSDSet._filePath))//추가 21.9.10 : 파일 경로가 비어있지 않은 경우에만 FileInfo 체크해야한다.
						{
							FileInfo prevFi = new FileInfo(_selectedPSDSet._filePath);//파일 경로 체크하도록 변경됨 (21.9.10)
							if (prevFi.Exists)
							{
								prevFileDir = prevFi.Directory.FullName;
							}
						}

						string filePath = EditorUtility.OpenFilePanel("Open PSD File", prevFileDir, "psd");//변경 21.3.1

						if (!string.IsNullOrEmpty(filePath))
						{
							//추가 21.7.3 : 이스케이프 문자 삭제
							filePath = apUtil.ConvertEscapeToPlainText(filePath);

							//LoadPsdFile(filePath, _selectedPSDSet);
							_selectedPSDSet.SetPSDFilePath(filePath);

							if (_selectedPSDSet.IsValidPSDFile)
							{
								bool isResult = _psdLoader.Step1_LoadPSDFile(
									_selectedPSDSet._filePath,
									(_selectedPSDSet._isLastBaked ? _selectedPSDSet._lastBakedAssetName : "")
									);
								if (isResult)
								{
									MakeRemappingList();
								}
							}
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("GUI_Center_FileLoad Exception : " + ex);
					}

					//경로에 관한 FileInfo 리스트도 다시 갱신한다.
					RefreshPSDFileInfo();
				}
				EditorGUILayout.EndHorizontal();
				

				//선택한 PSD Set의 정보를 적자
				//int width_Label = 150;
				//int width_Value = width_Half - (width_Label + 15);
				//int height_Value = 18;

				int imageSizeWidth = 0;
				int imageSizeHeight = 0;

				int lastImageSizeWidth = 0;
				int lastImageSizeHeight = 0;

				int layerCount = 0;
				int lastLayerCount = 0;

				//int lastBakeOption_Width = 0;
				//int lastBakeOption_Height = 0;
				string lastBakeDstPath = "";
				int lastBakePadding = 0;
				int lastBakeScale = 0;
				
				string strBakeProperties = "";
				int bakeInfoBoxHeight = 50;
				Color infoBoxColor_PSDSet = new Color(0.5f, 1.5f, 0.8f, 1.0f);
				Color infoBoxColor_PSDFile = new Color(0.5f, 0.8f, 1.5f, 1.0f);

				

				GUILayout.Space(15);


				
				if (_selectedPSDSet._isLastBaked)
				{
					lastImageSizeWidth = _selectedPSDSet._lastBaked_PSDImageWidth;
					lastImageSizeHeight = _selectedPSDSet._lastBaked_PSDImageHeight;

					lastLayerCount = _selectedPSDSet._layers.Count;

					//lastBakeOption_Width = _selectedPSDSet.GetBakeWidth();
					//lastBakeOption_Height = _selectedPSDSet.GetBakeHeight();
					lastBakeDstPath = _selectedPSDSet._bakeOption_DstFilePath;
					if (lastBakeDstPath.Length > 55)
					{
						//lastBakeDstPath = lastBakeDstPath.Substring(0, 55) + "..";
						//앞뒤로 끊자
						lastBakeDstPath = lastBakeDstPath.Substring(0, 25) + "  ..  " + lastBakeDstPath.Substring(lastBakeDstPath.Length - 30);

					}
					lastBakePadding = _selectedPSDSet._bakeOption_Padding;
					lastBakeScale = _selectedPSDSet._bakeScale100;
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					sb.Append(" - " + _editor.GetText(TEXT.DLG_PSD_ImageSize) + " : " + lastImageSizeWidth + " x " + lastImageSizeHeight);//Image Size
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_AssetPath) + " : " + lastBakeDstPath);//Asset Path
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Padding) + " : " + lastBakePadding);//Padding
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Scale) + " : " + lastBakeScale + "%");//Scale
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Layers) + " : " + lastLayerCount);//Layers

					strBakeProperties = sb.ToString();
					bakeInfoBoxHeight = 18 * 5;
				}
				else
				{
					infoBoxColor_PSDSet = new Color(1.5f, 0.6f, 0.6f, 1.0f);
					strBakeProperties = "[ " + _editor.GetText(TEXT.DLG_PSD_ThereAreNoBakeData) + " ]";//ThereAreNoBakeData
					bakeInfoBoxHeight = 30;
				}

				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BakeSettings), GUILayout.Width(width_Half));//Bake Settings

				

				GUI.backgroundColor = infoBoxColor_PSDSet;
				GUILayout.Box(strBakeProperties, _guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(bakeInfoBoxHeight));
				GUI.backgroundColor = prevColor;



				if(_psdLoader.IsFileLoaded)
				{
					imageSizeWidth = _psdLoader.PSDImageWidth;
					imageSizeHeight = _psdLoader.PSDImageHeight;
					layerCount = _psdLoader.PSDLayerDataList.Count;
				}

				SetGUIVisible("PSD File Properties", _selectedPSDSet != null && _psdLoader.IsFileLoaded);
				bool isPSDProperties = IsDelayedGUIVisible("PSD File Properties");

				if (isPSDProperties)
				{
					GUILayout.Space(20);
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDFileProperties), GUILayout.Width(width_Half));//PSD File Properties
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					sb.Append(" - " + _editor.GetText(TEXT.DLG_PSD_ImageSize) + " : " + imageSizeWidth + " x " + imageSizeHeight);//ImageSize
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Layers) + " : " + layerCount);//Layers

					GUI.backgroundColor = infoBoxColor_PSDFile;
					GUILayout.Box(sb.ToString(), _guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(50));
					GUI.backgroundColor = prevColor;
					
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width_Half - 30);
				GUILayout.Space(10);

				

				//추가 22.6.22 : 보조 텍스쳐 생성용 복제
				if(GUILayout.Button(_editor.GetText(TEXT.MakeSecondaryPSDSet), GUILayout.Width(width_Half - 30)))// "보조 텍스쳐 생성을 위한 데이터 복제"
				{
					if (_selectedPSDSet != null)
					{
						apPSDSecondarySet result = _editor.Controller.AddNewPSDSecondarySet(_selectedPSDSet);
						if(result == null)
						{
							//복제 실패
							EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_SecondaryPSDSetFailed_Title),
															_editor.GetText(TEXT.DLG_SecondaryPSDSetFailed_Body),
															_editor.GetText(TEXT.Okay));
						}
					}
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_RemovePSDImportSet), GUILayout.Width(width_Half - 30)))//Remove PSD Import Set
				{
					//bool isResult = EditorUtility.DisplayDialog(	
																	//"Remove PSD Import Set", 
																	//"Are you sure you want to remove the selected PSD Import Set? You can not undo deleted data.", 
																	//"Remove", 
																	//"Cancel");
					bool isResult = EditorUtility.DisplayDialog(
															_editor.GetText(TEXT.DLG_PSD_RemovePSDSet_Title),
															_editor.GetText(TEXT.DLG_PSD_RemovePSDSet_Body),
															_editor.GetText(TEXT.Remove),
															_editor.GetText(TEXT.Cancel)
															);
					if(isResult)
					{
						_portrait._bakedPsdSets.Remove(_selectedPSDSet);
						_selectedPSDSet = null;
						//_selectedPSDSetLayer = null;
						_selectedPSDLayerData = null;
						_selectedPSDBakeData = null;
						_selectedTextureData = null;
						_psdLoader.Clear();

						_isNeedBakeCheck = true;
						_isBakeWarning = false;
						_bakeWarningMsg = "";
						_loadKey_Calculated = null;
						_loadKey_Bake = null;
					}
				}
			}			
			else if(_selectedPSDSecondary != null)
			{
				//(2) 선택한게 Secondary Set이라면
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.SecondaryPSDSet), GUILayout.Width(width_Half));// "Secondary PSD 세트"
				GUILayout.Space(5);

				//- PSD 파일 이름 / 경로 (변경 가능)
				//- 원본 PSD Set 정보 (없으면 Invalid > Next 불가)
				//- Bake 설정 (자신)
				//- PSD 파일 정보
				//- 데이터 삭제

				//1. PSD 파일 이름/경로
				FileInfo fi = null;
				if(_psdSecondary2FileInfo.ContainsKey(_selectedPSDSecondary))
				{
					fi = _psdSecondary2FileInfo[_selectedPSDSecondary];
				}

				string selectedPSDFileName = "< " + _editor.GetText(TEXT.DLG_PSD_NoPSDFile) + " >";//"< No PSD File >";
				string selectedPSDPath = "< " + _editor.GetText(TEXT.DLG_PSD_NoPSDFile) + " >"; //"< No PSD File >";
				if(fi != null && fi.Exists)
				{
					selectedPSDFileName = fi.Name;
					selectedPSDPath = fi.FullName;
				}

				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDFileName) + " : " + selectedPSDFileName, GUILayout.Width(width_Half));//PSD File Name
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Half), GUILayout.Height(20));
				GUILayout.Space(5);
				EditorGUILayout.TextField(selectedPSDPath, guiStyle_TextBox, GUILayout.Width(width_Half - (90 + 25)));
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(90), GUILayout.Height(20)))//Change
				{
					//PSD 여는 Dialog
					try
					{
						//변경 21.3.1 : 이전 파일이 있는 디렉토리 경로를 가져오자
						string prevFileDir = "";

						if (!string.IsNullOrEmpty(_selectedPSDSecondary._psdFilePath))//추가 21.9.10 : 파일 경로가 비어있지 않은 경우에만 FileInfo 체크해야한다.
						{
							FileInfo prevFi = new FileInfo(_selectedPSDSecondary._psdFilePath);//파일 경로 체크하도록 변경됨 (21.9.10)
							if (prevFi.Exists)
							{
								prevFileDir = prevFi.Directory.FullName;
							}
						}

						string filePath = EditorUtility.OpenFilePanel("Open PSD File", prevFileDir, "psd");//변경 21.3.1

						if (!string.IsNullOrEmpty(filePath))
						{
							//추가 21.7.3 : 이스케이프 문자 삭제
							filePath = apUtil.ConvertEscapeToPlainText(filePath);

							_selectedPSDSecondary._psdFilePath = filePath;

							//다시 PSD 파일을 열자
							bool isResult = _psdLoader.Step1_LoadPSDFile(
									_selectedPSDSecondary._psdFilePath,
									(_selectedPSDSecondary._isLastBaked ? _selectedPSDSecondary._bakedTextureAssetName : "")
									);

							if (isResult)
							{
								MakeRemappingList();

								//Transform List는 사용하지 않는다.
								if(_targetTransformList == null)
								{
									_targetTransformList = new List<TargetTransformData>();
								}
								_targetTransformList.Clear();
							}
						}
					}
					catch (Exception ex)
					{
						Debug.LogError("GUI_Center_FileLoad Exception : " + ex);
					}

					//경로에 관한 FileInfo 리스트도 다시 갱신한다.
					RefreshPSDFileInfo();
				}
				EditorGUILayout.EndHorizontal();
				
				GUILayout.Space(15);

				//2. 원본 PSD Set 정보 (없으면 Invalid > Next 불가)
				EditorGUILayout.LabelField(_editor.GetText(TEXT.LinkedMainPSDSet), GUILayout.Width(width_Half));//"원본 PSD 세트"
				if(_selectedPSDSecondary._linkedMainSet != null)
				{
					// 원본이 되는 PSD Set이 있다.
					GUI.backgroundColor = new Color(1.0f, 1.0f, 0.2f, 1.0f);
					
					string strSrcPSDSet = "- Unknown File Path";
					if(_psdSet2FileInfo.ContainsKey(_selectedPSDSecondary._linkedMainSet))
					{
						FileInfo mainFI = _psdSet2FileInfo[_selectedPSDSecondary._linkedMainSet];
						if(mainFI != null && mainFI.Exists)
						{
							strSrcPSDSet = "- " + mainFI.Name;
						}
					}
					
					GUILayout.Box(strSrcPSDSet, _guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(50));
					GUI.backgroundColor = prevColor;
				}
				else
				{
					// 원본이 되는 PSD Set이 없어졌다.
					GUI.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
					GUILayout.Box("- " + _editor.GetText(TEXT.None), _guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(50));
					GUI.backgroundColor = prevColor;
				}


				GUILayout.Space(15);

				//3. 이전의 Bake 설정 (자신)

				string strBakeProperties = "";
				int bakeInfoBoxHeight = 50;
				Color infoBoxColor_PSDSet = new Color(0.5f, 1.5f, 0.8f, 1.0f);
				Color infoBoxColor_PSDFile = new Color(0.5f, 0.8f, 1.5f, 1.0f);

				if(_selectedPSDSecondary._isLastBaked)
				{
					string lastBakeDstPath = _selectedPSDSecondary._dstFilePath;
					if (lastBakeDstPath.Length > 55)
					{
						//앞뒤로 끊자
						lastBakeDstPath = lastBakeDstPath.Substring(0, 25) + "  ..  " + lastBakeDstPath.Substring(lastBakeDstPath.Length - 30);
					}

					int nLayers = _selectedPSDSecondary._layers != null ? _selectedPSDSecondary._layers.Count : 0;

					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					sb.Append(" - " + _editor.GetText(TEXT.DLG_PSD_ImageSize) + " : " + _selectedPSDSecondary._lastBaked_PSDImageWidth + " x " + _selectedPSDSecondary._lastBaked_PSDImageHeight);//Image Size
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_AssetPath) + " : " + lastBakeDstPath);//Asset Path
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Padding) + " : " + _selectedPSDSecondary._linkedMainSet._bakeOption_Padding);//Padding
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Scale) + " : " + _selectedPSDSecondary._lastBaked_Scale100 + "%");//Scale
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Layers) + " : " + nLayers);//Layers

					strBakeProperties = sb.ToString();
					bakeInfoBoxHeight = 18 * 5;
				}
				else
				{
					infoBoxColor_PSDSet = new Color(1.5f, 0.6f, 0.6f, 1.0f);
					strBakeProperties = "[ " + _editor.GetText(TEXT.DLG_PSD_ThereAreNoBakeData) + " ]";//ThereAreNoBakeData
					bakeInfoBoxHeight = 30;
				}
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BakeSettings), GUILayout.Width(width_Half));
				GUI.backgroundColor = infoBoxColor_PSDSet;
				GUILayout.Box(strBakeProperties, _guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(bakeInfoBoxHeight));
				GUI.backgroundColor = prevColor;

				
				//4. 로드된 PSD 파일 정보
				

				SetGUIVisible("PSD File Properties", _selectedPSDSecondary != null && _psdLoader.IsFileLoaded);
				bool isPSDProperties = IsDelayedGUIVisible("PSD File Properties");

				if (isPSDProperties && _psdLoader.IsFileLoaded)
				{
					GUILayout.Space(20);
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDFileProperties), GUILayout.Width(width_Half));//PSD File Properties
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					sb.Append(" - " + _editor.GetText(TEXT.DLG_PSD_ImageSize) + " : " + _psdLoader.PSDImageWidth + " x " + _psdLoader.PSDImageHeight);//ImageSize
					sb.Append("\n - " + _editor.GetText(TEXT.DLG_PSD_Layers) + " : " + _psdLoader.PSDLayerDataList.Count);//Layers

					GUI.backgroundColor = infoBoxColor_PSDFile;
					GUILayout.Box(sb.ToString(), _guiStyle_InfoBox, GUILayout.Width(width_Half - 30), GUILayout.Height(50));
					GUI.backgroundColor = prevColor;
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				//5. 데이터 삭제

				if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_RemovePSDImportSet), GUILayout.Width(width_Half - 30)))//Remove PSD Import Set
				{
					//bool isResult = EditorUtility.DisplayDialog(	
																	//"Remove PSD Import Set", 
																	//"Are you sure you want to remove the selected PSD Import Set? You can not undo deleted data.", 
																	//"Remove", 
																	//"Cancel");
					bool isResult = EditorUtility.DisplayDialog(
															_editor.GetText(TEXT.DLG_PSD_RemovePSDSet_Title),
															_editor.GetText(TEXT.DLG_PSD_RemovePSDSet_Body),
															_editor.GetText(TEXT.Remove),
															_editor.GetText(TEXT.Cancel)
															);
					if(isResult)
					{
						_portrait._bakedPsdSecondarySet.Remove(_selectedPSDSecondary);

						_selectedPSDSet = null;
						_selectedPSDSecondary = null;
						_selectedPSDLayerData = null;
						_selectedPSDBakeData = null;
						_selectedTextureData = null;

						_isNeedBakeCheck = true;
						_isBakeWarning = false;
						_bakeWarningMsg = "";
						_loadKey_Calculated = null;
						_loadKey_Bake = null;

						_psdLoader.Clear();
					}
				}
			}
			else
			{
				GUILayout.Space((height / 2) - 10);
				GUIStyle guiStyle_LabelCenter = new GUIStyle(GUI.skin.label);
				guiStyle_LabelCenter.alignment = TextAnchor.MiddleCenter;
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SelectPSDSet), guiStyle_LabelCenter, GUILayout.Width(width_Half));//Select PSD Import Set
			}




			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
			
		}



		//--------------------------------------------------------------------
		// Step 2 : 이미지 파일을 로드하고 메시그룹 선택하기
		// (미리보기와 기본 위치 설정)
		//--------------------------------------------------------------------
		private void GUI_Center_2_FileLoadAndSelectMeshGroup_Main(int width, int height, Rect centerRect)
		{
			//PSD의 설정, MeshGroup과 TextureData를 선택하는 화면
			//좌우 2개. 오른쪽이 크다
			//왼쪽 : 선택된 PSD Set의 설정과 연결된 MeshGroup, TextureData
			//오른쪽 : PSD, MeshGroup, TextureData를 볼 수 있는 GUI
			int margin = 4;
			int width_Left = 400;
			int width_Right = (width - (width_Left + margin));
			//int height_RightBottom = 40;
			int height_RightBottom = 70;
			int height_RightGUI = height - (height_RightBottom + margin);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Left, height), "");


			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightGUI), "");
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin + height_RightGUI + margin, width_Right, height_RightBottom), "");

			GUILayout.Space(5);

			//--------------------------------------
			// <1열 : PSD Set 설정 + MeshGroup, TextureData 연결하기 >

			//- MeshGroup 연결
			//- 덮어쓰기할 TextureData 연결
			//- 이전 크기 vs 현재 크기와 비교 (Box)
			int width_Icon = 40;
			int width_LeftInScroll = width_Left - (24 + 5);
			int width_ItemValue = width_Left - (width_Icon + 25 + 10);
			//int width_ItemValueInScroll = width_LeftInScroll - (width_Icon + 25 + 10);

			GUIStyle guiStyle_SmallBtn = new GUIStyle(GUI.skin.button);
			guiStyle_SmallBtn.margin = GUI.skin.box.margin;

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;

			GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
			guiStyle_Label.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_PMBtn = new GUIStyle(GUI.skin.button);
			guiStyle_PMBtn.margin = GUI.skin.textField.margin;

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height));

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDImportSetSettings));//PSD Import Set Settings
			GUILayout.Space(5);

			//1. MeshGroup
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(50));//H1
			GUILayout.Space(5);
			//아이콘
			EditorGUILayout.LabelField(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(width_Icon), GUILayout.Height(32));
			GUILayout.Space(5);
			//오른쪽 항목 (이름)
			EditorGUILayout.BeginVertical(GUILayout.Width(width_ItemValue), GUILayout.Height(50));//V3
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_TargetMeshGroup));//Target MeshGroup
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_ItemValue), GUILayout.Height(18));//H2
			string targetMeshGroupName = _editor.GetText(TEXT.DLG_PSD_NoMeshGroup);//No Mesh Group
			if (_selectedPSDSet._linkedTargetMeshGroup != null)
			{
				targetMeshGroupName = _selectedPSDSet._linkedTargetMeshGroup._name;
				if (targetMeshGroupName.Length > 25)
				{
					targetMeshGroupName = targetMeshGroupName.Substring(0, 25) + "..";
				}
				GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.5f, prevColor.b * 1.0f, 1.0f);
			}
			else
			{
				GUI.backgroundColor = new Color(prevColor.r * 1.5f, prevColor.g * 0.7f, prevColor.b * 0.7f, 1.0f);
			}
			GUILayout.Box(targetMeshGroupName, guiStyle_Box, GUILayout.Width(width_ItemValue - (90 - 13)), GUILayout.Height(18));
			GUI.backgroundColor = prevColor;

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), guiStyle_SmallBtn, GUILayout.Width(85), GUILayout.Height(18)))//Change
			{
				_loadKey_SelectMeshGroup = apDialog_SelectLinkedMeshGroup.ShowDialog(_editor, null, OnMeshGroupSelected);
			}
			EditorGUILayout.EndHorizontal();//H2
			EditorGUILayout.EndVertical();//V3

			EditorGUILayout.EndHorizontal();//H1


			//2. TextureData
			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BakedImages));//Baked Images
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AddImage), GUILayout.Width(width_Left - 15), GUILayout.Height(20)))//Add Image
			{
				_loadKey_SelectTextureData = apDialog_SelectTextureData.ShowDialog(_editor, null, OnTextureDataSelected);
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AddImagesAuto), GUILayout.Width(width_Left - 15), GUILayout.Height(30)))//Add Image Automatically
			{
				//_loadKey_SelectTextureData = apDialog_SelectTextureData.ShowDialog(_editor, null, OnTextureDataSelected);
				//자동으로 TextureData를 추가한다.
				if (_selectedPSDSet._linkedTargetMeshGroup != null)
				{
					AutoSelectTextureData(_selectedPSDSet);
				}
			}

			//GUIContent guiContent_TextureDataIcon = new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image));
			Texture2D img_TextureData = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);

			//이전
			//GUIContent guiContent_Delete = new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey));

			//변경
			if (_guiContent_Delete == null)
			{
				_guiContent_Delete = apGUIContentWrapper.Make(_editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey));
			}

			GUILayout.Space(5);
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + 161 + 10, width_Left, 150), "");
			GUI.backgroundColor = prevColor;
			_scroll_Step2_Left = EditorGUILayout.BeginScrollView(_scroll_Step2_Left, false, true, GUILayout.Width(width_Left - 5), GUILayout.Height(150));

			EditorGUILayout.BeginVertical(GUILayout.Width(width_LeftInScroll));//V1

			bool isRemoveTexData = false;
			int iRemoveTexData = -1;

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}
			GUILayout.Space(5);
			//Images
			GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.DLG_PSD_Images), _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)), guiStyle_None, GUILayout.Width(width_LeftInScroll - 10), GUILayout.Height(20));
			GUILayout.Space(4);

			for (int iTex = 0; iTex < _selectedPSDSet._targetTextureDataList.Count; iTex++)
			{
				apPSDSet.TextureDataSet texDataSet = _selectedPSDSet._targetTextureDataList[iTex];

				GUIStyle curGUIStyle = guiStyle_None;

				if (_selectedTextureData == texDataSet)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();

					#region [미사용 코드]
					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}
					//GUI.Box(new Rect(lastRect.x, lastRect.y + 2, width_Left, 24), "");
					//GUI.backgroundColor = prevColor; 
					#endregion


					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 2, width_Left - 2, 24, apEditorUtil.UNIT_BG_STYLE.Main);


					curGUIStyle = guiStyle_Selected;
				}

				string texDataName = "";
				if (texDataSet._linkedTextureData != null)
				{
					texDataName = texDataSet._linkedTextureData._name;
					if (texDataName.Length > 25)
					{
						texDataName = texDataName.Substring(0, 25) + "..";
					}
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_LeftInScroll), GUILayout.Height(20));
				GUILayout.Space(15);
				if (GUILayout.Button(new GUIContent(" " + texDataName, img_TextureData), curGUIStyle, GUILayout.Width(width_LeftInScroll - (15 + 30)), GUILayout.Height(20)))
				{
					_selectedTextureData = texDataSet;
				}
				GUILayout.Space(5);

				if (GUILayout.Button(_guiContent_Delete.Content, guiStyle_None, GUILayout.Width(20), GUILayout.Height(20)))
				{
					//TODO
					//bool isResult = EditorUtility.DisplayDialog("Detach Image", "Do you want to detach the Image?", "Detach", "Cancel");

					bool isResult = EditorUtility.DisplayDialog(
						_editor.GetText(TEXT.DLG_PSD_DetachImage_Title),
						_editor.GetText(TEXT.DLG_PSD_DetachImage_Body),
						_editor.GetText(TEXT.DLG_PSD_Detach),
						_editor.GetText(TEXT.DLG_Cancel));


					if (isResult)
					{
						isRemoveTexData = true;
						iRemoveTexData = iTex;
					}
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(4);
			}
			if (isRemoveTexData)
			{
				if (_selectedPSDSet._targetTextureDataList[iRemoveTexData] == _selectedTextureData)
				{
					_selectedTextureData = null;
				}
				_selectedPSDSet._targetTextureDataList.RemoveAt(iRemoveTexData);

			}

			GUILayout.Space(200);

			EditorGUILayout.EndVertical();//V1

			EditorGUILayout.EndScrollView();

			//3. PSD Bake 설정값들 (여기서 수정할 수 있는건 Bake된 비율. 위치를 맞추기 위해)
			GUILayout.Space(10);

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AtlasBakeSettingsForReimpot));//"Atlas Bake Settings for Reimporting"
			GUILayout.Space(5);
			if (!_selectedPSDSet._isLastBaked)
			{
				GUI.backgroundColor = new Color(1.5f, 0.6f, 0.6f, 1.0f);
				GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_ThereAreNoBakeData) + " ]", guiStyle_Box, GUILayout.Width(width_Left - 20), GUILayout.Height(30));//There are no bake data
				GUI.backgroundColor = prevColor;
				GUILayout.Space(5);
			}
			int width_InfoLabel = 100;
			int width_InfoValue = width_Left - (width_InfoLabel + 20);
			int height_Info = 18;

			if (_selectedPSDSet._isLastBaked)
			{
				//Width
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Width) + " : ", GUILayout.Width(width_InfoLabel));//Image Width
				EditorGUILayout.LabelField(_selectedPSDSet._lastBaked_PSDImageWidth.ToString(), GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				//Height
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Height) + " : ", GUILayout.Width(width_InfoLabel));//Image height
				EditorGUILayout.LabelField(_selectedPSDSet._lastBaked_PSDImageHeight.ToString(), GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();
			}
			GUILayout.Space(5);
			//Bake Scale
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BakeScale) + " (%) : ", GUILayout.Width(width_InfoLabel));//Bake Scale
			int nextBakeScale100 = EditorGUILayout.DelayedIntField(_selectedPSDSet._next_meshGroupScaleX100, GUILayout.Width(width_InfoValue - (28 * 6 + 40)));
			if (nextBakeScale100 != _selectedPSDSet._next_meshGroupScaleX100)
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(nextBakeScale100, 5, 1000);
			}
			GUILayout.Space(10);
			if (GUILayout.Button("-5", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 - 5, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if (GUILayout.Button("-2", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 - 2, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}

			if (GUILayout.Button("-1", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 - 1, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			GUILayout.Space(10);
			if (GUILayout.Button("+1", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 + 1, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if (GUILayout.Button("+2", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 + 2, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if (GUILayout.Button("+5", guiStyle_PMBtn, GUILayout.Width(26), GUILayout.Height(16)))
			{
				_selectedPSDSet._next_meshGroupScaleX100 = Mathf.Clamp(_selectedPSDSet._next_meshGroupScaleX100 + 5, 5, 1000);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			////추가 21.3.6
			////만약 BakeScale을 변경할 필요가 없는데 Canvas 사이즈가 변경되었다면 Offset을 수정하면 안된다.
			////이걸 명시해주자
			//if (_selectedPSDSet._lastBaked_PSDImageWidth != _psdLoader.PSDImageWidth &&
			//	_selectedPSDSet._lastBaked_PSDImageHeight != _psdLoader.PSDImageHeight)
			//{
			//	GUILayout.Space(5);
			//	//이미지 사이즈가 다르다.
			//	string strWarning = "Last PSD Size : " + _selectedPSDSet._lastBaked_PSDImageWidth + "x" + _selectedPSDSet._lastBaked_PSDImageHeight + "\n";
			//	strWarning += "Current PSD Size : " + _psdLoader.PSDImageWidth + "x" + _psdLoader.PSDImageHeight;
			//	GUILayout.Box(strWarning, apGUIStyleWrapper.I.Box_MiddleCenter, GUILayout.Height(50));
			//}




			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : PSD, MeshGroup, TextureData를 볼 수 있는 GUI >
			Rect guiRect = new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightGUI);
			UpdateAndDrawGUIBase(guiRect, new Vector2(6, 0.3f));
			
			//GL 렌더링
			//1. PSD
			
			if(_isRender_MeshGroup && _selectedPSDSet._linkedTargetMeshGroup != null)
			{
				DrawMeshGroup(_selectedPSDSet._linkedTargetMeshGroup);
			}
			if(_isRender_TextureData && _selectedTextureData != null)
			{
				DrawTextureData(_selectedTextureData._linkedTextureData, true);
			}
			if(_renderMode_PSD != RENDER_MODE.Hide)
			{
				DrawPSD(_renderMode_PSD == RENDER_MODE.Outline, null, _selectedPSDSet._nextBakeCenterOffsetDelta_X, _selectedPSDSet._nextBakeCenterOffsetDelta_Y, _selectedPSDSet._next_meshGroupScaleX100);
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height));
			GUILayout.Space(height_RightGUI + margin);
			//오른쪽 하단
			//렌더 필터 버튼들과 PSD 위치 이동
			//렌더 여부 버튼

			int btnSize = height_RightBottom - (24+8);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right), GUILayout.Height(height_RightBottom));
			GUILayout.Space(5);
			EditorGUILayout.BeginVertical(GUILayout.Width(220), GUILayout.Height(height_RightBottom));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_RenderingType), GUILayout.Width(150));//Rendering Type
			EditorGUILayout.BeginHorizontal(GUILayout.Width(220), GUILayout.Height(height_RightBottom - 24));

			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), _isRender_TextureData, true, 40, btnSize))
			{
				_isRender_TextureData = !_isRender_TextureData;
			}
			if (apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), _isRender_MeshGroup, true, 40, btnSize))
			{
				_isRender_MeshGroup = !_isRender_MeshGroup;
			}

			Texture2D img_PSDSet = null;
			bool isRenderPSD = _renderMode_PSD != RENDER_MODE.Hide;
			if(_renderMode_PSD == RENDER_MODE.Outline)
			{
				img_PSDSet = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetOutline);
			}
			else
			{
				img_PSDSet = _editor.ImageSet.Get(apImageSet.PRESET.PSD_Set);
			}

			if(apEditorUtil.ToggledButton_2Side(img_PSDSet, isRenderPSD, true, 40, btnSize))
			{
				//_isRender_PSD = !_isRender_PSD;
				switch (_renderMode_PSD)
				{
					case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
					case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
					case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
				}
			}
			//if(apEditorUtil.ToggledButton_2Side("Translucent", "Opaque", _isRender_PSDAlpha, true, 90, btnSize))
			//{
			//	_isRender_PSDAlpha = !_isRender_PSDAlpha;
			//}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.BeginVertical(GUILayout.Width(120), GUILayout.Height(height_RightBottom));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Offset), GUILayout.Width(120));//Offset
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(120), GUILayout.Height(20));
			EditorGUILayout.LabelField("X", GUILayout.Width(15));
			float nextOffsetPosX = EditorGUILayout.FloatField(_selectedPSDSet._nextBakeCenterOffsetDelta_X, GUILayout.Width(40));
			if(nextOffsetPosX != _selectedPSDSet._nextBakeCenterOffsetDelta_X)
			{
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = nextOffsetPosX;
			}
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Y", GUILayout.Width(15));
			float nextOffsetPosY = EditorGUILayout.FloatField(_selectedPSDSet._nextBakeCenterOffsetDelta_Y, GUILayout.Width(40));
			if(nextOffsetPosY != _selectedPSDSet._nextBakeCenterOffsetDelta_Y)
			{
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = nextOffsetPosY;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			//-------------------------------------------------------------------
			GUILayout.Space(10);

			//X,Y 제어 버튼

			//위치를 쉽게 제어하기 위해서 버튼으로 만들자
			//				Y:(+1, +10)
			//X:(-10, -1)					X:(+1, +10)
			//				Y:(-1, -10)
			int width_contBtn = 40;
			int height_contBtn = 22;
			int width_contBtnArea = width_contBtn * 2 + 4;
			int margin_contBtn = ((height_RightBottom - 4) - height_contBtn) / 2;
			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightBottom));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-X 버튼
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X - 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X - 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightBottom));
			//+-Y
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+Y 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y + 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y + 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(margin_contBtn / 2 - 2);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-Y 버튼
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y - 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_Y - 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightBottom));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+X 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				//X + 1
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X + 1);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{ 
				// X + 10
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = GetCorrectedFloat(_selectedPSDSet._nextBakeCenterOffsetDelta_X + 10);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();



			//-------------------------------------------------------------------
			

			EditorGUILayout.EndHorizontal();



			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}


		private void GUI_Center_2_FileLoadAndSelectMeshGroup_Secondary(int width, int height, Rect centerRect)
		{
			//Secondary 버전의 메시 그룹 연결과 대상 텍스쳐 에셋 선택 화면
			// 차이점
			//- 메시 그룹 보정이 필요 없다.
			//- 단, 이후의 비교를 위해 Scale은 조정해야한다.

			
			

			//PSD의 설정, MeshGroup과 TextureData를 선택하는 화면
			//좌우 2개. 오른쪽이 크다
			//왼쪽 : 선택된 PSD Set의 설정과 연결된 MeshGroup, TextureData
			//오른쪽 : PSD, MeshGroup, TextureData를 볼 수 있는 GUI
			int margin = 4;
			int width_Left = 400;
			int width_Right = (width - (width_Left + margin));
			//int height_RightBottom = 40;
			int height_RightBottom = 70;
			int height_RightGUI = height - (height_RightBottom + margin);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Left, height), "");


			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightGUI), "");
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin + height_RightGUI + margin, width_Right, height_RightBottom), "");

			GUILayout.Space(5);

			//--------------------------------------
			// <1열 : PSD Set 설정 + MeshGroup, TextureData 연결하기 >

			//- MeshGroup 연결
			//- 덮어쓰기할 TextureData 연결
			//- 이전 크기 vs 현재 크기와 비교 (Box)
			//int width_Icon = 40;
			int width_LeftInScroll = width_Left - (24 + 5);
			//int width_ItemValue = width_Left - (width_Icon + 25 + 10);
			//int width_ItemValueInScroll = width_LeftInScroll - (width_Icon + 25 + 10);

			GUIStyle guiStyle_SmallBtn = new GUIStyle(GUI.skin.button);
			guiStyle_SmallBtn.margin = GUI.skin.box.margin;

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;

			GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
			guiStyle_Label.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_PMBtn = new GUIStyle(GUI.skin.button);
			guiStyle_PMBtn.margin = GUI.skin.textField.margin;

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height));

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDImportSetSettings));//PSD Import Set Settings
			GUILayout.Space(5);

			//Secondary에서는 메시 그룹은 필요없고, 그냥 이전 Bake 정보에서 Texture Data들이 잘 있는지만 확인하면 된다.

			

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}

			Texture2D img_TextureData = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);


			//1. 원본이 되는 TextureData 리스트
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + 29, width_Left, 150), "");
			GUI.backgroundColor = prevColor;


			int nLinkedSrcTextureData = _selectedPSDSecondary._srcTextureDataInfoList != null ? _selectedPSDSecondary._srcTextureDataInfoList.Count : 0;

			_scroll_Step2_Left = EditorGUILayout.BeginScrollView(_scroll_Step2_Left, false, true, GUILayout.Width(width_Left - 5), GUILayout.Height(150));



			EditorGUILayout.BeginVertical(GUILayout.Width(width_LeftInScroll));//V1

			GUILayout.Space(5);
			//Images
			GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.DLG_PSD_Images), _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)), guiStyle_None, GUILayout.Width(width_LeftInScroll - 10), GUILayout.Height(20));
			GUILayout.Space(4);

			
			if(nLinkedSrcTextureData > 0)
			{
				for (int iTex = 0; iTex < nLinkedSrcTextureData; iTex++)
				{
					apPSDSecondarySet.SrcTextureData texDataInfo = _selectedPSDSecondary._srcTextureDataInfoList[iTex];
					GUIStyle curGUIStyle = guiStyle_None;

					
					if (_selectedSecondaryTexInfo == texDataInfo
							&& texDataInfo._linkedTextureData != null)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();


						#region [미사용 코드]
						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}
						//GUI.Box(new Rect(lastRect.x, lastRect.y + 2, width_Left, 24), "");
						//GUI.backgroundColor = prevColor; 
						#endregion


						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 2, width_Left - 2, 24, apEditorUtil.UNIT_BG_STYLE.Main);


						curGUIStyle = guiStyle_Selected;
					}

					string texDataName = "";
					if (texDataInfo._linkedTextureData != null)
					{
						texDataName = texDataInfo._linkedTextureData._name;
						if (texDataName.Length > 40)
						{
							texDataName = texDataName.Substring(0, 40) + "..";
						}
					}
					else
					{
						texDataName = "< " + _editor.GetText(TEXT.NoImage) + " >";
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_LeftInScroll), GUILayout.Height(20));
					GUILayout.Space(15);
					if (GUILayout.Button(new GUIContent(" " + texDataName, img_TextureData), curGUIStyle, GUILayout.Width(width_LeftInScroll - (15 + 10)), GUILayout.Height(20)))
					{
						_selectedSecondaryTexInfo = texDataInfo;
					}
				
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(4);
				}
			}

			GUILayout.Space(200);

			EditorGUILayout.EndVertical();//V1

			EditorGUILayout.EndScrollView();


			//2. Bake되었던 텍스쳐 에셋 리스트 (추가, 삭제는 되지 않는다. 의미가 없으므로)
			// (Main PSD Set에서는 오버라이드하는 Image(TextureData) 리스트가 나왔었다.


			GUILayout.Space(10);

			//4. 이전 Bake 기록
			
			EditorGUILayout.LabelField(_editor.GetText(TEXT.PreviousBakeInfo));//"이전의 Bake 기록"
			GUILayout.Space(5);
			
			if (_selectedPSDSecondary._isLastBaked)
			{
				//Bake되었다면
				int width_InfoLabel = 100;
				int width_InfoValue = width_Left - (width_InfoLabel + 20);
				int height_Info = 18;

				//Width
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Width) + " : ", GUILayout.Width(width_InfoLabel));//Image Width
				EditorGUILayout.LabelField(_selectedPSDSecondary._lastBaked_PSDImageWidth.ToString(), GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				//Height
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Left), GUILayout.Height(height_Info));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Height) + " : ", GUILayout.Width(width_InfoLabel));//Image height
				EditorGUILayout.LabelField(_selectedPSDSecondary._lastBaked_PSDImageHeight.ToString(), GUILayout.Width(width_InfoValue));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				EditorGUILayout.LabelField(_editor.GetText(TEXT.GeneratedSecondaryTextures));//"Bake 되었던 텍스쳐 에셋들"
				GUILayout.Space(5);
				GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
				GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + 289, width_Left, 150), "");
				GUI.backgroundColor = prevColor;
				_scroll_Step2_Left2 = EditorGUILayout.BeginScrollView(_scroll_Step2_Left2, false, true, GUILayout.Width(width_Left - 5), GUILayout.Height(150));

				EditorGUILayout.BeginVertical(GUILayout.Width(width_LeftInScroll));//V1
				{
				
					GUILayout.Space(5);
				
					//Images
					GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.TextureAssets), _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)), guiStyle_None, GUILayout.Width(width_LeftInScroll - 10), GUILayout.Height(20));
					GUILayout.Space(4);

					int nBakedImages = _selectedPSDSecondary._bakedTextures != null ? _selectedPSDSecondary._bakedTextures.Count : 0;
					if(nBakedImages > 0)
					{
						for (int iTex = 0; iTex < nBakedImages; iTex++)
						{
							Texture2D curTextureAsset = _selectedPSDSecondary._bakedTextures[iTex];

							//예전에 Bake 되었었던 해당되는 텍스쳐 에셋을 보여주자
							EditorGUILayout.BeginHorizontal(GUILayout.Width(width_LeftInScroll), GUILayout.Height(20));
							GUILayout.Space(15);
							EditorGUILayout.ObjectField(curTextureAsset, typeof(Texture2D), false, GUILayout.Width(width_LeftInScroll - 30));
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(4);
						}
					}
					GUILayout.Space(200);
				}
				EditorGUILayout.EndVertical();//V1

				EditorGUILayout.EndScrollView();
			}
			else
			{
				//Bake되지 않았다면
				GUI.backgroundColor = new Color(1.5f, 0.6f, 0.6f, 1.0f);
				GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_ThereAreNoBakeData) + " ]", guiStyle_Box, GUILayout.Width(width_Left - 20), GUILayout.Height(30));//There are no bake data
				GUI.backgroundColor = prevColor;
				GUILayout.Space(5);
			}

			


			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			
			// <2열 : PSD, MeshGroup, TextureData를 볼 수 있는 GUI >


			Rect guiRect = new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightGUI);
			UpdateAndDrawGUIBase(guiRect, new Vector2(6, 0.3f));
			
			//GL 렌더링
			//1. PSD
			

			//Secondary에서 Mesh Group은 보여주지 않는다.
			//if(_isRender_MeshGroup
			//	&& _selectedPSDSecondary._linkedMainSet != null
			//	&& _selectedPSDSecondary._linkedMainSet._linkedTargetMeshGroup != null)
			//{
			//	DrawMeshGroup(_selectedPSDSecondary._linkedMainSet._linkedTargetMeshGroup);
			//}

			if(_isRender_TextureData 
				&& _selectedSecondaryTexInfo != null
				&& _selectedSecondaryTexInfo._linkedTextureData != null)
			{
				DrawTextureData(_selectedSecondaryTexInfo._linkedTextureData, true);
			}

			if(_renderMode_PSD != RENDER_MODE.Hide)
			{
				DrawPSD(	_renderMode_PSD == RENDER_MODE.Outline, 
							null,
							0, 0, 100);
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height));
				GUILayout.Space(height_RightGUI + margin);
				//오른쪽 하단
				//렌더 필터 버튼들과 PSD 위치 이동
				//렌더 여부 버튼

				int btnSize = height_RightBottom - (24+8);

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right), GUILayout.Height(height_RightBottom));
					GUILayout.Space(5);
					EditorGUILayout.BeginVertical(GUILayout.Width(220), GUILayout.Height(height_RightBottom));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_RenderingType), GUILayout.Width(150));//Rendering Type
						EditorGUILayout.BeginHorizontal(GUILayout.Width(220), GUILayout.Height(height_RightBottom - 24));

							if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), _isRender_TextureData, true, 40, btnSize))
							{
								_isRender_TextureData = !_isRender_TextureData;
							}

							//Secondary에서 메시 그룹 보이기는 지원하지 않는다.
							//if (apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), _isRender_MeshGroup, true, 40, btnSize))
							//{
							//	_isRender_MeshGroup = !_isRender_MeshGroup;
							//}

							Texture2D img_PSDSet = null;
							bool isRenderPSD = _renderMode_PSD != RENDER_MODE.Hide;
							if(_renderMode_PSD == RENDER_MODE.Outline)
							{
								img_PSDSet = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetOutline);
							}
							else
							{
								img_PSDSet = _editor.ImageSet.Get(apImageSet.PRESET.PSD_Set);
							}

							if(apEditorUtil.ToggledButton_2Side(img_PSDSet, isRenderPSD, true, 40, btnSize))
							{
								//_isRender_PSD = !_isRender_PSD;
								switch (_renderMode_PSD)
								{
									case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
									case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
									case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
								}
							}
							//if(apEditorUtil.ToggledButton_2Side("Translucent", "Opaque", _isRender_PSDAlpha, true, 90, btnSize))
							//{
							//	_isRender_PSDAlpha = !_isRender_PSDAlpha;
							//}
						EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical();


			//-------------------------------------------------------------------


			EditorGUILayout.EndHorizontal();



			EditorGUILayout.EndVertical(); 
			
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}


		// 서브 이벤트 / 함수
		private object _loadKey_SelectMeshGroup = null;
		private void OnMeshGroupSelected(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip)
		{
			if(!isSuccess)
			{
				_loadKey_SelectMeshGroup = null;
				return;
			}
			if(_loadKey_SelectMeshGroup != loadKey || _loadKey_SelectMeshGroup == null || loadKey == null)
			{
				_loadKey_SelectMeshGroup = null;
				return;
			}

			if(_selectedPSDSet != null)
			{
				_selectedPSDSet._linkedTargetMeshGroup = meshGroup;
				if(_selectedPSDSet._linkedTargetMeshGroup != null)
				{
					_selectedPSDSet._targetMeshGroupID = meshGroup._uniqueID;
				}
				else
				{
					_selectedPSDSet._targetMeshGroupID = -1;
				}
			}
			_loadKey_SelectMeshGroup = null;
		}


		private void OnMeshGroupSelected_Secondary(bool isSuccess, object loadKey, apMeshGroup meshGroup, apAnimClip targetAnimClip)
		{
			if(!isSuccess)
			{
				_loadKey_SelectMeshGroup = null;
				return;
			}
			if(_loadKey_SelectMeshGroup != loadKey || _loadKey_SelectMeshGroup == null || loadKey == null)
			{
				_loadKey_SelectMeshGroup = null;
				return;
			}

			
			if(_selectedPSDSecondary != null
				&& _selectedPSDSecondary._linkedMainSet != null)
			{	
				if(meshGroup != null)
				{
					_selectedPSDSecondary._linkedMainSet._linkedTargetMeshGroup = meshGroup;
					_selectedPSDSecondary._linkedMainSet._targetMeshGroupID = meshGroup._uniqueID;
				}
				else
				{	
					_selectedPSDSecondary._linkedMainSet._linkedTargetMeshGroup = null;
					//연결이 안되었다고 해서 설정을 -1로 만들진 않는다.
				}
			}
			_loadKey_SelectMeshGroup = null;
		}

		private object _loadKey_SelectTextureData = null;
		private void OnTextureDataSelected(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData)
		{
			if(!isSuccess)
			{
				_loadKey_SelectTextureData = null;
				return;
			}
			if(_loadKey_SelectTextureData != loadKey || _loadKey_SelectTextureData == null || loadKey == null)
			{
				_loadKey_SelectTextureData = null;
				return;
			}
			if(_selectedPSDSet != null && resultTextureData != null)
			{
				//등록되지 않은 데이터라면
				if(!_selectedPSDSet._targetTextureDataList.Exists(delegate(apPSDSet.TextureDataSet a)
				{
					return a._textureDataID == resultTextureData._uniqueID;
				}))
				{
					apPSDSet.TextureDataSet newSet = new apPSDSet.TextureDataSet();
					newSet._textureDataID = resultTextureData._uniqueID;
					newSet._linkedTextureData = resultTextureData;
					_selectedPSDSet._targetTextureDataList.Add(newSet);
				}
			}
			_loadKey_SelectTextureData = null;
		}

		private void AutoSelectTextureData(apPSDSet psdSet)
		{
			if(psdSet == null || psdSet._linkedTargetMeshGroup == null)
			{
				return;
			}
			List<apTextureData> result = new List<apTextureData>();
			FindTextureDataRecursive(psdSet._linkedTargetMeshGroup, result);

			result.Sort(delegate(apTextureData a, apTextureData b)
			{
				return string.Compare(a._name, b._name);
			});

			for (int i = 0; i < result.Count; i++)
			{
				if(!psdSet._targetTextureDataList.Exists(delegate(apPSDSet.TextureDataSet a)
				{
					return a._textureDataID == result[i]._uniqueID;
				}))
				{
					apPSDSet.TextureDataSet newTexSet = new apPSDSet.TextureDataSet();
					newTexSet._textureDataID = result[i]._uniqueID;
					newTexSet._linkedTextureData = result[i];
					psdSet._targetTextureDataList.Add(newTexSet);
				}

			}
		}
		private void FindTextureDataRecursive(apMeshGroup meshGroup, List<apTextureData> resultList)
		{
			for (int i = 0; i < meshGroup._childMeshTransforms.Count; i++)
			{
				apMesh mesh = meshGroup._childMeshTransforms[i]._mesh;
				if(mesh != null)
				{
					if(mesh._textureData_Linked != null && !resultList.Contains(mesh._textureData_Linked))
					{
						resultList.Add(mesh._textureData_Linked);
					}
				}
			}
			if(meshGroup._childMeshGroupTransforms != null)
			{
				for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
				{
					apMeshGroup childMeshGroup = meshGroup._childMeshGroupTransforms[i]._meshGroup;
					if(childMeshGroup != null && childMeshGroup != meshGroup)
					{
						FindTextureDataRecursive(childMeshGroup, resultList);
					}
				}
			}
			
		}





		//--------------------------------------------------------------------
		// Step 3 : 레이어와 Transform 연결하기
		//--------------------------------------------------------------------
		private void GUI_Center_3_LinkLayerToTransform_Main(int width, int height, Rect centerRect)
		{
			//레이어, 트랜스폼을 연결하고, 선택된 레이어를 미리볼 수 있는 화면
			//1열 : 레이어 리스트
			//2열 : Transform 리스트 (Hierarchy)
			//3열 (넓음) : 선택된 Layer와 연결된 Mesh를 볼 수 있는 GUI
			//1, 2열 하단에는 매핑 도구가 있다.
			int margin = 4;
			int width_1 = 350;
			int width_2 = 300;
			int width_3 = (width - (width_1 + margin + width_2 + margin));
			int height_LeftLower = 36;
			int height_LeftUpper = height - (margin + height_LeftLower);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_1, height_LeftUpper), "");
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + 29, width_1, height_LeftUpper - 29), "");//리스트1 박스
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin, width_2, height_LeftUpper), "");


			if(_isLinkLayerToTransform)
			{
				GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.4f, prevColor.b * 0.7f, 1.0f);
			}
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin + 29, width_2, height_LeftUpper - 29), "");//리스트2 박스
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + height_LeftUpper + margin, width_1 + margin + width_2, height_LeftLower), "");
			
			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height), "");
			GUI.backgroundColor = prevColor;
			
			
			
			//--------------------------------------
			// <1열 + 2열 + 1, 2열 하단>
			int height_ListHeight = 26;

			GUIStyle guiStyle_BtnToggle = new GUIStyle(GUI.skin.button);
			guiStyle_BtnToggle.margin = GUI.skin.textField.margin;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Btn_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Btn_Selected.normal.textColor = Color.white;
			}
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_NotBake = new GUIStyle(GUIStyle.none);
			guiStyle_Btn_NotBake.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			guiStyle_Btn_NotBake.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;

			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);


			EditorGUILayout.BeginVertical(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height));
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height_LeftUpper));
			//GUILayout.Space(5);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_1), GUILayout.Height(height_LeftUpper));
			// <1열 : 레이어 리스트 >
			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_PSDLayers));//PSD Layers
			GUILayout.Space(5);
			_scroll_Step3_Line1 = EditorGUILayout.BeginScrollView(_scroll_Step3_Line1, false, true, GUILayout.Width(width_1), GUILayout.Height(height_LeftUpper - 30));

			int width_Line1InScroll = (width_1) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line1InScroll));
			GUILayout.Space(5);
			//레이어 리스트
			apPSDLayerData curPSDLayer = null;
			int iList = 0;

			Texture2D imgBakeEnabled = _editor.ImageSet.Get(apImageSet.PRESET.PSD_BakeEnabled);
			Texture2D imgBakeDisabled = _editor.ImageSet.Get(apImageSet.PRESET.PSD_BakeDisabled);

			for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
			{
				curPSDLayer = _psdLoader.PSDLayerDataList[i];
				//int level = curPSDLayer._hierarchyLevel;

				if (_selectedPSDLayerData == curPSDLayer)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();

					float yOffset = 0.0f;

					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}

					if (iList == 0)
					{
						//GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						yOffset = 4;
					}
					else
					{
						//GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						yOffset = height_ListHeight - 1;
					}

					//GUI.backgroundColor = prevColor;


					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, apEditorUtil.UNIT_BG_STYLE.Main);

				}
				else if(_isLinkGUIColoredList)
				{
					//_isLinkGUIColoredList 옵션이 켜지면 그 색상을 그냥 보여주자
					//연결 안되면 안보여줌
					if (curPSDLayer._isBakable && curPSDLayer._isRemapSelected)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						int yOffset = 0;

						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = curPSDLayer._randomGUIColor_Pro;
						//}
						//else
						//{
						//	GUI.backgroundColor = curPSDLayer._randomGUIColor;
						//}

						if (iList == 0)
						{
							//GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = 4;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = height_ListHeight - 1;
						}

						//GUI.backgroundColor = prevColor;

						//v1.4.2
						Color customBGColor = EditorGUIUtility.isProSkin ? curPSDLayer._randomGUIColor_Pro : curPSDLayer._randomGUIColor;
						//밝기가 일정 레벨을 넘어가면 제한하자 (새로운 리스트 배경은 White Texture를 사용하므로 기존보다 많이 밝다)
						float colorLuminous = (customBGColor.r * 0.3f) + (customBGColor.g * 0.6f) * (customBGColor.b * 0.1f);
						if(colorLuminous > 0.7f)
						{
							float lumCorrection = 0.7f / colorLuminous;
							customBGColor.r *= lumCorrection;
							customBGColor.g *= lumCorrection;
							customBGColor.b *= lumCorrection;
						}

						//변경 v1.4.2 (이건 커스텀색상)
						apEditorUtil.DrawListUnitBG_CustomColor(lastRect.x + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, customBGColor);
					}
				}
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line1InScroll), GUILayout.Height(height_ListHeight));
				
				int prefixMargin = 0;
				GUILayout.Space(5);
				
				prefixMargin = 5;
				if(curPSDLayer._isImageLayer)
				{
					if(curPSDLayer._isClipping)
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += (height_ListHeight / 2) + 4;
					}
					EditorGUILayout.LabelField(new GUIContent(curPSDLayer._image), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}
				else
				{
					EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}

				GUIStyle curGUIStyle = guiStyle_Btn;
				if (!curPSDLayer._isBakable)
				{
					curGUIStyle = guiStyle_Btn_NotBake;
				}
				else if (curPSDLayer == _selectedPSDLayerData)
				{
					curGUIStyle = guiStyle_Btn_Selected;
				}

				int btnWidth = width_Line1InScroll - (prefixMargin + 120);
				
				if (GUILayout.Button("  " + curPSDLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
				{
					_selectedPSDLayerData = curPSDLayer;
					_isLinkLayerToTransform = false;
					_linkSrcLayerData = null;
				}
				if(apEditorUtil.ToggledButton_2Side(imgBakeEnabled, imgBakeDisabled, curPSDLayer._isBakable, true, 20, height_ListHeight - 6))
				{
					curPSDLayer._isBakable = !curPSDLayer._isBakable;
					_isLinkLayerToTransform = false;
					_linkSrcLayerData = null;

					if(!curPSDLayer._isBakable)
					{
						//Bake를 끄는 경우 : 연결을 해제한다.
						UnlinkPSDLayer(curPSDLayer);
					}
				}

				bool isRemapSelected = false;
				bool isAvailable = true;
				string strRemapName = _editor.GetText(TEXT.DLG_PSD_NotSelected);//Not Selected
				if (curPSDLayer._isBakable)
				{
					if (curPSDLayer._isRemapSelected)
					{
						if (curPSDLayer._remap_MeshTransform != null)
						{
							strRemapName = curPSDLayer._remap_MeshTransform._nickName;
							isRemapSelected = true;
						}
						else if (curPSDLayer._remap_MeshGroupTransform != null)
						{
							strRemapName = curPSDLayer._remap_MeshGroupTransform._nickName;
							isRemapSelected = true;
						}
					}
				}
				else
				{
					isAvailable = false;
					strRemapName = _editor.GetText(TEXT.DLG_PSD_NotBakeable);//Not Bakeable
				}

				//만약 선택 중이라면
				if(_isLinkLayerToTransform)
				{
					if(curPSDLayer == _linkSrcLayerData)
					{
						isRemapSelected = true;
						strRemapName = ">>>";
						isAvailable = true;
					}
					else
					{
						isAvailable = false;
					}
				}

				if(apEditorUtil.ToggledButton_2Side(strRemapName, strRemapName, isRemapSelected, isAvailable, 85, height_ListHeight - 6))
				{
					//TODO.
					if(!_isLinkLayerToTransform)
					{
						//선택모드로 변경
						_linkSrcLayerData = curPSDLayer;
						_isLinkLayerToTransform = true;
						_selectedPSDLayerData = curPSDLayer;
					}
					else
					{
						//선택모드에서 해제
						_linkSrcLayerData = null;
						_isLinkLayerToTransform = false;
					}

					
					
				}
				

				EditorGUILayout.EndHorizontal();

				iList++;
			}
			

			GUILayout.Space(height_LeftUpper);
			EditorGUILayout.EndVertical();



			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(margin);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_2), GUILayout.Height(height_LeftUpper));
			// <2열 : Transform Hierarchy >
			GUILayout.Space(5);
			string meshGroupName = _selectedPSDSet._linkedTargetMeshGroup.name;
			if(meshGroupName.Length > 30)
			{
				meshGroupName = meshGroupName.Substring(0, 30) + "..";
			}
			EditorGUILayout.LabelField("  " + meshGroupName);
			GUILayout.Space(5);

			_scroll_Step3_Line2 = EditorGUILayout.BeginScrollView(_scroll_Step3_Line2, false, true, GUILayout.Width(width_2), GUILayout.Height(height_LeftUpper - 30));

			int width_Line2InScroll = (width_2) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line2InScroll));
			GUILayout.Space(5);
			//Transform 리스트

			Texture2D imgMeshTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			Texture2D imgMeshGroupTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);

			//apRenderUnit curRenderUnit = null;
			//for (int i = _selectedPSDSet._linkedTargetMeshGroup._renderUnits_All.Count - 1; i >= 0; i--)
			TargetTransformData curTransformData = null;
			iList = 0;
			for (int i = _targetTransformList.Count - 1; i >= 0; i--)
			{
				curTransformData = _targetTransformList[i];
				
				if(curTransformData._meshGroupTransform != null && 
					curTransformData._meshGroupTransform == _selectedPSDSet._linkedTargetMeshGroup._rootMeshGroupTransform)
				{
					continue;
				}

				bool isLinked = false;
				if (_selectedPSDLayerData != null &&
						_selectedPSDLayerData._isRemapSelected &&
						(
							(_selectedPSDLayerData._remap_MeshTransform != null && _selectedPSDLayerData._remap_MeshTransform == curTransformData._meshTransform)
							||
							(_selectedPSDLayerData._remap_MeshGroupTransform != null && _selectedPSDLayerData._remap_MeshGroupTransform == curTransformData._meshGroupTransform)
						)
					)
				{
					isLinked = true;
					Rect lastRect = GUILayoutUtility.GetLastRect();

					int yOffset = 0;

					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}

					if (iList == 0)
					{
						//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						yOffset = 4;
					}
					else
					{
						//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
						yOffset = height_ListHeight - 1;
					}

					//GUI.backgroundColor = prevColor;


					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1 + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, apEditorUtil.UNIT_BG_STYLE.Main);
				}

				if (_isLinkGUIColoredList && !isLinked)
				{
					//_isLinkGUIColoredList 옵션이 켜지면 그 색상을 그냥 보여주자
					apPSDLayerData linkedLayerData = null;
					if(curTransformData._meshTransform != null)
					{
						if (_meshTransform2PSDLayer.ContainsKey(curTransformData._meshTransform))
						{
							linkedLayerData = _meshTransform2PSDLayer[curTransformData._meshTransform];
						}
					}
					else if(curTransformData._meshGroupTransform != null)
					{
						if(_meshGroupTransform2PSDLayer.ContainsKey(curTransformData._meshGroupTransform))
						{
							linkedLayerData = _meshGroupTransform2PSDLayer[curTransformData._meshGroupTransform];
						}
					}
					if(linkedLayerData != null && linkedLayerData._isBakable)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						int yOffset = 0;

						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = linkedLayerData._randomGUIColor_Pro;
						//}
						//else
						//{
						//	GUI.backgroundColor = linkedLayerData._randomGUIColor;
						//}

						if (iList == 0)
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = 4;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = height_ListHeight - 1;
						}

						//GUI.backgroundColor = prevColor;

						//v1.4.2
						Color customBGColor = EditorGUIUtility.isProSkin ? linkedLayerData._randomGUIColor_Pro : linkedLayerData._randomGUIColor;

						//밝기가 일정 레벨을 넘어가면 제한하자 (새로운 리스트 배경은 White Texture를 사용하므로 기존보다 많이 밝다)
						float colorLuminous = (customBGColor.r * 0.3f) + (customBGColor.g * 0.6f) * (customBGColor.b * 0.1f);
						if(colorLuminous > 0.7f)
						{
							float lumCorrection = 0.7f / colorLuminous;
							customBGColor.r *= lumCorrection;
							customBGColor.g *= lumCorrection;
							customBGColor.b *= lumCorrection;
						}

						//변경 v1.4.2 (이건 커스텀색상)
						apEditorUtil.DrawListUnitBG_CustomColor(lastRect.x + 1 + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, customBGColor);
					}

				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line2InScroll), GUILayout.Height(height_ListHeight));
				GUILayout.Space(5);
				int prefixMargin = 5;
				bool isRemap = false;
				string remapPSDLayerName = _editor.GetText(TEXT.DLG_PSD_Select);//Select

				if(curTransformData._meshTransform != null)
				{
					if(curTransformData._meshTransform._isClipping_Child)
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += (height_ListHeight / 2) + 4;
					}
					EditorGUILayout.LabelField(new GUIContent(imgMeshTF), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;

					//연결된 PSD Layer를 체크하자
					if(_meshTransform2PSDLayer.ContainsKey(curTransformData._meshTransform))
					{
						isRemap = true;
						remapPSDLayerName = _meshTransform2PSDLayer[curTransformData._meshTransform]._name;
					}
					
				}
				else if(curTransformData._meshGroupTransform != null)
				{
					EditorGUILayout.LabelField(new GUIContent(imgMeshGroupTF), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;

					//연결된 PSD Layer를 체크하자
					if(_meshGroupTransform2PSDLayer.ContainsKey(curTransformData._meshGroupTransform))
					{
						isRemap = true;
						remapPSDLayerName = _meshGroupTransform2PSDLayer[curTransformData._meshGroupTransform]._name;
					}
				}
				
				GUIStyle curGUIStyle = guiStyle_Btn;
				if (curTransformData._isMeshTransform && !curTransformData._isValidMesh)
				{
					curGUIStyle = guiStyle_Btn_NotBake;
				}
				else if (isLinked)
				{
					curGUIStyle = guiStyle_Btn_Selected;
				}


				int btnWidth = width_Line2InScroll - (prefixMargin + 120);
				if (GUILayout.Button("  " + curTransformData.Name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
				{
					//_selectedPSDLayerData = curPSDLayer;
					//여기서..는 선택하지 않는다.
				}

				
				if ((_isLinkLayerToTransform && _linkSrcLayerData != null && _linkSrcLayerData._isImageLayer == curTransformData._isMeshTransform) 
					|| isRemap)
				{
					if(apEditorUtil.ToggledButton(remapPSDLayerName, !_isLinkLayerToTransform, 110, height_ListHeight - 6))
					{
						if (_isLinkLayerToTransform)
						{
							LinkPSDLayerAndTransform(_linkSrcLayerData, curTransformData);
							_isLinkLayerToTransform = false;
							_linkSrcLayerData = null;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				iList++;
			}
			
			


			GUILayout.Space(height_LeftUpper);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(margin);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height_LeftLower));
			// <1+2열 하단 : 매핑 툴>
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AutoMapping), GUILayout.Width(120), GUILayout.Height(height_LeftLower - 8)))//Auto Mapping
			{
				LinkTool_AutoMapping();
			}
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_EnableAll), GUILayout.Width(100), GUILayout.Height(height_LeftLower - 8)))//Enable All
			{
				LinkTool_EnableAll();
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_DisableAll), GUILayout.Width(100), GUILayout.Height(height_LeftLower - 8)))//Disable All
			{
				LinkTool_DisableAll();
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Reset), GUILayout.Width(110), GUILayout.Height(height_LeftLower - 8)))//Reset
			{
				LinkTool_Reset();
			}
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.PSD_LinkView), _isLinkGUIColoredList, true, 40, height_LeftLower - 8))
			{
				_isLinkGUIColoredList = !_isLinkGUIColoredList;
			}
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.PSD_Overlay), _isLinkOverlayColorRender, true, 40, height_LeftLower - 8))
			{
				_isLinkOverlayColorRender = !_isLinkOverlayColorRender;
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <3열 : 레이어, Mesh GUI >
			Rect guiRect = new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height);
			UpdateAndDrawGUIBase(guiRect, new Vector2(15.0f, 0.3f));

			float meshScale = (float)_selectedPSDSet._next_meshGroupScaleX100 / (float)_selectedPSDSet._prev_bakeScale100;
			//float meshScale = 100.0f / (float)_selectedPSDSet._prev_bakeScale100;

			//DrawPSD(false, null, 0, 0, 100, false);
			if(_selectedPSDLayerData != null)
			{
				if(_selectedPSDLayerData._image != null)
				{
					DrawPSDLayer(_selectedPSDLayerData, 0, 0, _selectedPSDSet._next_meshGroupScaleX100, _isLinkOverlayColorRender);
				}
				if(_selectedPSDLayerData._isRemapSelected
					&& _selectedPSDLayerData._isImageLayer
					&& _selectedPSDLayerData._remap_MeshTransform != null)
				{
					if(_isLinkOverlayColorRender)
					{
						//DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, true, _meshOverlayColor);
						DrawMeshToneColor(_selectedPSDLayerData._remap_MeshTransform._mesh, false, meshScale);
					}
					else
					{
						DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, false, meshScale);
					}
					
				}
			}


			EditorGUILayout.BeginVertical(GUILayout.Width(width_3), GUILayout.Height(height));
			GUILayout.Space(5);

			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}





		private void GUI_Center_3_LinkLayerToTransform_Secondary(int width, int height, Rect centerRect)
		{
			// Secondary 에서 PSD 레이어와 연결하는 화면
			// Main의 경우 PSD Layer Data -> Transform 연결이지만
			// Secondary의 경우엔 PSD Layer Data -> Secondary Layer를 연결한다.
			
			//레이어, 트랜스폼을 연결하고, 선택된 레이어를 미리볼 수 있는 화면
			//1열 : 레이어 리스트
			//2열 : Transform 리스트 (Hierarchy) >> Secondary Layer 리스트
			//3열 (넓음) : 선택된 Layer와 연결된 Atlas를 볼 수 있는 화면
			//1, 2열 하단에는 매핑 도구가 있다.
			int margin = 4;
			int width_1 = 350;
			int width_2 = 300;
			int width_3 = (width - (width_1 + margin + width_2 + margin));
			int height_LeftLower = 36;
			int height_LeftUpper = height - (margin + height_LeftLower);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_1, height_LeftUpper), "");
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + 29, width_1, height_LeftUpper - 29), "");//리스트1 박스
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin, width_2, height_LeftUpper), "");


			if(_isLinkLayerToTransform)
			{
				GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.4f, prevColor.b * 0.7f, 1.0f);
			}
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin + 29, width_2, height_LeftUpper - 29), "");//리스트2 박스
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin + height_LeftUpper + margin, width_1 + margin + width_2, height_LeftLower), "");
			
			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height), "");
			GUI.backgroundColor = prevColor;
			
			
			
			//--------------------------------------
			// <1열 + 2열 + 1, 2열 하단>
			int height_ListHeight = 26;

			GUIStyle guiStyle_BtnToggle = new GUIStyle(GUI.skin.button);
			guiStyle_BtnToggle.margin = GUI.skin.textField.margin;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Btn_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Btn_Selected.normal.textColor = Color.white;
			}
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_NotBake = new GUIStyle(GUIStyle.none);
			guiStyle_Btn_NotBake.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			guiStyle_Btn_NotBake.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;

			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);


			EditorGUILayout.BeginVertical(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height));
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height_LeftUpper));
			//GUILayout.Space(5);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_1), GUILayout.Height(height_LeftUpper));
			// <1열 : 레이어 리스트 >
			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_PSDLayers));//PSD Layers
			GUILayout.Space(5);
			_scroll_Step3_Line1 = EditorGUILayout.BeginScrollView(_scroll_Step3_Line1, false, true, GUILayout.Width(width_1), GUILayout.Height(height_LeftUpper - 30));

			int width_Line1InScroll = (width_1) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line1InScroll));
			GUILayout.Space(5);
			//레이어 리스트
			apPSDLayerData curPSDLayer = null;
			int iList = 0;

			Texture2D imgBakeEnabled = _editor.ImageSet.Get(apImageSet.PRESET.PSD_BakeEnabled);
			Texture2D imgBakeDisabled = _editor.ImageSet.Get(apImageSet.PRESET.PSD_BakeDisabled);

			//PSD 파일의 레이어를 "역순"으로 보여주자


			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;
			if (nPSDLayers > 0)
			{
				for (int i = nPSDLayers - 1; i >= 0; i--)
				{
					curPSDLayer = _psdLoader.PSDLayerDataList[i];

					if (_selectedPSDLayerData == curPSDLayer)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						int yOffset = 0;

						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}

						if (iList == 0)
						{
							//GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = 4;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = height_ListHeight - 1;
						}

						//GUI.backgroundColor = prevColor;

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, apEditorUtil.UNIT_BG_STYLE.Main);


					}
					else if (_isLinkGUIColoredList)
					{
						//_isLinkGUIColoredList 옵션이 켜지면 그 색상을 그냥 보여주자
						//연결 안되면 안보여줌
						//if (curPSDLayer._isBakable && curPSDLayer._isRemapSelected)//Main
						if (curPSDLayer._isBakable && curPSDLayer._linkedBakedInfo_Secondary != null)//Secondary
						{
							Rect lastRect = GUILayoutUtility.GetLastRect();
							int yOffset = 0;
							
							//if (EditorGUIUtility.isProSkin)
							//{
							//	GUI.backgroundColor = curPSDLayer._randomGUIColor_Pro;
							//}
							//else
							//{
							//	GUI.backgroundColor = curPSDLayer._randomGUIColor;
							//}

							if (iList == 0)
							{
								//GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
								yOffset = 4;
							}
							else
							{
								//GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
								yOffset = height_ListHeight - 1;
							}

							//GUI.backgroundColor = prevColor;

							//v1.4.2
							Color customBGColor = EditorGUIUtility.isProSkin ? curPSDLayer._randomGUIColor_Pro : curPSDLayer._randomGUIColor;
							//밝기가 일정 레벨을 넘어가면 제한하자 (새로운 리스트 배경은 White Texture를 사용하므로 기존보다 많이 밝다)
							float colorLuminous = (customBGColor.r * 0.3f) + (customBGColor.g * 0.6f) * (customBGColor.b * 0.1f);
							if(colorLuminous > 0.7f)
							{
								float lumCorrection = 0.7f / colorLuminous;
								customBGColor.r *= lumCorrection;
								customBGColor.g *= lumCorrection;
								customBGColor.b *= lumCorrection;
							}

							//변경 v1.4.2 (이건 커스텀색상)
							apEditorUtil.DrawListUnitBG_CustomColor(lastRect.x + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, customBGColor);
						}
					}
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line1InScroll), GUILayout.Height(height_ListHeight));

					int prefixMargin = 0;
					GUILayout.Space(5);

					prefixMargin = 5;
					if (curPSDLayer._isImageLayer)
					{
						if (curPSDLayer._isClipping)
						{
							EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
							prefixMargin += (height_ListHeight / 2) + 4;
						}
						EditorGUILayout.LabelField(new GUIContent(curPSDLayer._image), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += height_ListHeight - 5;
					}
					else
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += height_ListHeight - 5;
					}

					GUIStyle curGUIStyle = guiStyle_Btn;
					if (!curPSDLayer._isBakable)
					{
						curGUIStyle = guiStyle_Btn_NotBake;
					}
					else if (curPSDLayer == _selectedPSDLayerData)
					{
						curGUIStyle = guiStyle_Btn_Selected;
					}

					int btnWidth = width_Line1InScroll - (prefixMargin + 120);

					if (GUILayout.Button("  " + curPSDLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
					{
						_selectedPSDLayerData = curPSDLayer;
						_isLinkLayerToTransform = false;
						_linkSrcLayerData = null;
					}
					if (apEditorUtil.ToggledButton_2Side(imgBakeEnabled, imgBakeDisabled, curPSDLayer._isBakable, true, 20, height_ListHeight - 6))
					{
						curPSDLayer._isBakable = !curPSDLayer._isBakable;
						_isLinkLayerToTransform = false;
						_linkSrcLayerData = null;

						if (!curPSDLayer._isBakable)
						{
							//Bake를 끄는 경우 : 연결을 해제한다.
							UnlinkPSDLayer(curPSDLayer);
						}
					}

					bool isRemapSelected = false;
					bool isAvailable = true;
					string strRemapName = _editor.GetText(TEXT.DLG_PSD_NotSelected);//Not Selected
					if (curPSDLayer._isBakable)
					{
						//Main
						//if (curPSDLayer._isRemapSelected)
						//{
						//	if (curPSDLayer._remap_MeshTransform != null)
						//	{
						//		strRemapName = curPSDLayer._remap_MeshTransform._nickName;
						//		isRemapSelected = true;
						//	}
						//	else if (curPSDLayer._remap_MeshGroupTransform != null)
						//	{
						//		strRemapName = curPSDLayer._remap_MeshGroupTransform._nickName;
						//		isRemapSelected = true;
						//	}
						//}

						//Secondary
						if(curPSDLayer._linkedBakedInfo_Secondary != null)
						{
							strRemapName = curPSDLayer._linkedBakedInfo_Secondary._mainLayerName;
							isRemapSelected = true;
						}
					}
					else
					{
						isAvailable = false;
						strRemapName = _editor.GetText(TEXT.DLG_PSD_NotBakeable);//Not Bakeable
					}

					//만약 선택 중이라면
					if (_isLinkLayerToTransform)
					{
						if (curPSDLayer == _linkSrcLayerData)
						{
							isRemapSelected = true;
							strRemapName = ">>>";
							isAvailable = true;
						}
						else
						{
							isAvailable = false;
						}
					}

					if (apEditorUtil.ToggledButton_2Side(strRemapName, strRemapName, isRemapSelected, isAvailable, 85, height_ListHeight - 6))
					{
						//TODO.
						if (!_isLinkLayerToTransform)
						{
							//선택모드로 변경
							_linkSrcLayerData = curPSDLayer;
							_isLinkLayerToTransform = true;
							_selectedPSDLayerData = curPSDLayer;
						}
						else
						{
							//선택모드에서 해제
							_linkSrcLayerData = null;
							_isLinkLayerToTransform = false;
						}
					}


					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}
			
			

			GUILayout.Space(height_LeftUpper);
			EditorGUILayout.EndVertical();



			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(margin);



			// <2열 : Secondary Layer >


			EditorGUILayout.BeginVertical(GUILayout.Width(width_2), GUILayout.Height(height_LeftUpper));
			
			
			


			GUILayout.Space(5);


			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.PreviousLayers));//"기존 레이어 정보들"
			GUILayout.Space(5);

			_scroll_Step3_Line2 = EditorGUILayout.BeginScrollView(_scroll_Step3_Line2, false, true, GUILayout.Width(width_2), GUILayout.Height(height_LeftUpper - 30));

			int width_Line2InScroll = (width_2) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line2InScroll));
			GUILayout.Space(5);
			//Transform 리스트

			//Texture2D imgLayerInfo = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetSecondary);
			Texture2D imgLayerInfo = _editor.ImageSet.Get(apImageSet.PRESET.PSD_Set);


			int nSecondaryLayers = _selectedPSDSecondary._layers != null ? _selectedPSDSecondary._layers.Count : 0;
			apPSDSecondarySetLayer curSecondaryLayer = null;
			iList = 0;
			if (nSecondaryLayers > 0)
			{
				//역순으로
				for (int i = nSecondaryLayers - 1; i >= 0; i--)
				{
					curSecondaryLayer = _selectedPSDSecondary._layers[i];

					bool isLinked = false;
					if (_selectedPSDLayerData != null
						&& _selectedPSDLayerData._linkedBakedInfo_Secondary != null
						&& _selectedPSDLayerData._linkedBakedInfo_Secondary == curSecondaryLayer)
					{
						isLinked = true;
						Rect lastRect = GUILayoutUtility.GetLastRect();

						int yOffset = 0;

						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}

						if (iList == 0)
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = 4;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
							yOffset = height_ListHeight - 1;
						}

						//GUI.backgroundColor = prevColor;

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1 + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, apEditorUtil.UNIT_BG_STYLE.Main);
					}

					if (_isLinkGUIColoredList && !isLinked)
					{
						//_isLinkGUIColoredList 옵션이 켜지면 그 색상을 그냥 보여주자
						apPSDLayerData linkedLayerData = null;
						if(_secondaryLayer2PSDLayer.ContainsKey(curSecondaryLayer))
						{
							linkedLayerData = _secondaryLayer2PSDLayer[curSecondaryLayer];
						}


						if (linkedLayerData != null && linkedLayerData._isBakable)
						{
							Rect lastRect = GUILayoutUtility.GetLastRect();

							int yOffset = 0;

							//if (EditorGUIUtility.isProSkin)
							//{
							//	GUI.backgroundColor = linkedLayerData._randomGUIColor_Pro;
							//}
							//else
							//{
							//	GUI.backgroundColor = linkedLayerData._randomGUIColor;
							//}


							if (iList == 0)
							{
								//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 5 - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
								yOffset = 4;
							}
							else
							{
								//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + height_ListHeight - 1, width_Line1InScroll + 10, height_ListHeight + 2), "");
								yOffset = height_ListHeight - 1;
							}

							//GUI.backgroundColor = prevColor;

							//v1.4.2
							Color customBGColor = EditorGUIUtility.isProSkin ? linkedLayerData._randomGUIColor_Pro : linkedLayerData._randomGUIColor;
							//밝기가 일정 레벨을 넘어가면 제한하자 (새로운 리스트 배경은 White Texture를 사용하므로 기존보다 많이 밝다)
							float colorLuminous = (customBGColor.r * 0.3f) + (customBGColor.g * 0.6f) * (customBGColor.b * 0.1f);
							if(colorLuminous > 0.7f)
							{
								float lumCorrection = 0.7f / colorLuminous;
								customBGColor.r *= lumCorrection;
								customBGColor.g *= lumCorrection;
								customBGColor.b *= lumCorrection;
							}

							//변경 v1.4.2 (이건 커스텀색상)
							apEditorUtil.DrawListUnitBG_CustomColor(lastRect.x + 1, lastRect.y + yOffset, width_Line1InScroll + 10 - 2, height_ListHeight + 2, customBGColor);
						}

					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line2InScroll), GUILayout.Height(height_ListHeight));
					GUILayout.Space(5);
					int prefixMargin = 5;
					bool isRemap = false;
					string remapPSDLayerName = _editor.GetText(TEXT.DLG_PSD_Select);//Select

					EditorGUILayout.LabelField(new GUIContent(imgLayerInfo), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;

					//연결된 PSD Layer를 체크하자
					if (_secondaryLayer2PSDLayer.ContainsKey(curSecondaryLayer))
					{
						isRemap = true;
						remapPSDLayerName = _secondaryLayer2PSDLayer[curSecondaryLayer]._name;
					}

					GUIStyle curGUIStyle = guiStyle_Btn;
					if(curSecondaryLayer._bakedUniqueID < 0)
					{
						curGUIStyle = guiStyle_Btn_NotBake;
					}
					else if (isLinked)
					{
						curGUIStyle = guiStyle_Btn_Selected;
					}


					int btnWidth = width_Line2InScroll - (prefixMargin + 120);
					if (GUILayout.Button("  " + curSecondaryLayer._mainLayerName, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
					{
						//_selectedPSDLayerData = curPSDLayer;
						//여기서..는 선택하지 않는다.
					}

					//연결 중일때
					if ((_isLinkLayerToTransform 
						&& _linkSrcLayerData != null 
						//&& _linkSrcLayerData._isImageLayer == curTransformData._isMeshTransform
						&& curSecondaryLayer._bakedUniqueID >= 0
						)
						|| isRemap)
					{
						if (apEditorUtil.ToggledButton(remapPSDLayerName, !_isLinkLayerToTransform, 110, height_ListHeight - 6))
						{
							if (_isLinkLayerToTransform)
							{
								LinkPSDLayerAndSecondaryLayer(_linkSrcLayerData, curSecondaryLayer);
								_isLinkLayerToTransform = false;
								_linkSrcLayerData = null;
							}
						}
					}
					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}
			GUILayout.Space(height_LeftUpper + 100);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(margin);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_1 + margin + width_2), GUILayout.Height(height_LeftLower));
			// <1+2열 하단 : 매핑 툴>
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_AutoMapping), GUILayout.Width(120), GUILayout.Height(height_LeftLower - 8)))//Auto Mapping
			{
				LinkTool_AutoMapping();
			}
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_EnableAll), GUILayout.Width(100), GUILayout.Height(height_LeftLower - 8)))//Enable All
			{
				LinkTool_EnableAll();
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_DisableAll), GUILayout.Width(100), GUILayout.Height(height_LeftLower - 8)))//Disable All
			{
				LinkTool_DisableAll();
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Reset), GUILayout.Width(110), GUILayout.Height(height_LeftLower - 8)))//Reset
			{
				LinkTool_Reset();
			}
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.PSD_LinkView), _isLinkGUIColoredList, true, 40, height_LeftLower - 8))
			{
				_isLinkGUIColoredList = !_isLinkGUIColoredList;
			}
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.PSD_Overlay), _isLinkOverlayColorRender, true, 40, height_LeftLower - 8))
			{
				_isLinkOverlayColorRender = !_isLinkOverlayColorRender;
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <3열 : 레이어, Mesh GUI >
			Rect guiRect = new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height);
			UpdateAndDrawGUIBase(guiRect, new Vector2(15.0f, 0.3f));

			
			//메인
			//float meshScale = (float)_selectedPSDSet._next_meshGroupScaleX100 / (float)_selectedPSDSet._prev_bakeScale100;

			//Secondary
			//float meshScale = (float)_selectedPSDSecondary._next_meshGroupScaleX100 / (float)_selectedPSDSecondary._prev_bakeScale100;
			

			//DrawPSD(false, null, 0, 0, 100, false);
			if(_selectedPSDLayerData != null)
			{
				if(_selectedPSDLayerData._image != null)
				{
					DrawPSDLayer(_selectedPSDLayerData, 0, 0, 100, _isLinkOverlayColorRender);
				}

				//Main인 경우
				//if(_selectedPSDLayerData._isRemapSelected
				//	&& _selectedPSDLayerData._isImageLayer
				//	&& _selectedPSDLayerData._remap_MeshTransform != null)
				//{
				//	if(_isLinkOverlayColorRender)
				//	{
				//		//DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, true, _meshOverlayColor);
				//		DrawMeshToneColor(_selectedPSDLayerData._remap_MeshTransform._mesh, false, meshScale);
				//	}
				//	else
				//	{
				//		DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, false, meshScale);
				//	}
					
				//}

				//Secondary인 경우
				if(_selectedPSDLayerData._linkedBakedInfo_Secondary != null
					&& _selectedPSDLayerData._linkedBakedInfo_Secondary._linkedTextureData != null)
				{
					//출력 위치 기본적으로 UV가 (0.5, 0.5)인 중심을 가리키므로,
					//Atlas 이미지 영역의 중심이 화면의 중심이 되도록 보정을 해보자
					apTextureData textureData = _selectedPSDLayerData._linkedBakedInfo_Secondary._linkedTextureData;
					Vector2 centerPosOffset = Vector2.zero;
					float textureWidth_Half = textureData._width * 0.5f;
					float textureHeight_Half = textureData._height * 0.5f;

					int bakedPos_Left = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedImagePos_Left;
					int bakedPos_Top = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedImagePos_Top;
					int bakedPos_AreaWidth = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedWidth;
					int bakedPos_AreaHeight = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedHeight;

					//렌더링할 때 원점에 맞추기 위해 Size*0.5를 뺀다.
					//그럼 그만큼 미리 더한다.
					centerPosOffset.x += textureWidth_Half;
					centerPosOffset.y += textureHeight_Half;

					centerPosOffset.x -= (float)bakedPos_Left + ((float)bakedPos_AreaWidth * 0.5f);
					centerPosOffset.y -= (float)bakedPos_Top + ((float)bakedPos_AreaHeight * 0.5f);

					if (_isLinkOverlayColorRender)
					{
						DrawTextureData_Transparent(	_selectedPSDLayerData._linkedBakedInfo_Secondary._linkedTextureData,
											false,
											centerPosOffset);
					}
					else
					{
						DrawTextureData(	_selectedPSDLayerData._linkedBakedInfo_Secondary._linkedTextureData,
											false,
											centerPosOffset);
					}
					
				}
			}


			EditorGUILayout.BeginVertical(GUILayout.Width(width_3), GUILayout.Height(height));
			GUILayout.Space(5);

			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}





		//--------------------------------------------------------------------
		// Step 4 : 레이어별 위치 보정하기
		//--------------------------------------------------------------------
		private void GUI_Center_4_ModifyOffset_Main(int width, int height, Rect centerRect)
		{
			//레이어를 선택하고 새로운 이미지의 위치를 조정하는 과정
			//좌우 2개. 오른쪽이 크며, 오른쪽은 하단에 툴이 있다.
			//왼쪽 : 레이어 리스트
			//오른쪽 : 레이어 + 메시가 동시에 출력 (또는 전환되어 출력)되는 GUI와 하단의 위치 조정 툴
			int margin = 4;
			int width_Left = 200;
			int width_Right = (width - (width_Left + margin));
			int height_RightLower = 70;
			int height_RightUpper = height - (margin + height_RightLower);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Left, height), "");
			
			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightUpper), "");
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin + margin + height_RightUpper, width_Right, height_RightLower), "");

			//--------------------------------------
			// <1열 : 레이어 리스트 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_PSDLayers));//PSD Layers
			GUILayout.Space(5);
			
			
			_scroll_Step4_Left = EditorGUILayout.BeginScrollView(_scroll_Step4_Left, false, true, GUILayout.Width(width_Left), GUILayout.Height(height - 30));
			
			
			int width_LeftInScroll = (width_Left) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_LeftInScroll));
			GUILayout.Space(5);

			

			//PSD 레이어를 출력한다. (Bake안되는것은 나오지 않는다)
			apPSDLayerData curPSDLayer = null;
			int iList = 0;
			int height_ListHeight = 26;

			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);


			GUIStyle guiStyle_BtnToggle = new GUIStyle(GUI.skin.button);
			guiStyle_BtnToggle.margin = GUI.skin.textField.margin;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Btn_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Btn_Selected.normal.textColor = Color.white;
			}
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_NotBake = new GUIStyle(GUIStyle.none);
			guiStyle_Btn_NotBake.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			guiStyle_Btn_NotBake.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;



			for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
			{
				curPSDLayer = _psdLoader.PSDLayerDataList[i];
				//int level = curPSDLayer._hierarchyLevel;


				if (_selectedPSDLayerData == curPSDLayer)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();
					int yOffset = 0;

					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}

					if (iList == 0)
					{
						//GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_LeftInScroll + 10, height_ListHeight + 2), "");
						yOffset = 4;
					}
					else
					{
						//GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_LeftInScroll + 10, height_ListHeight + 2), "");
						yOffset = height_ListHeight - 1;
					}

					//GUI.backgroundColor = prevColor;

					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + yOffset, width_LeftInScroll + 10 - 2, height_ListHeight + 2, apEditorUtil.UNIT_BG_STYLE.Main);
				}
				
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_LeftInScroll), GUILayout.Height(height_ListHeight));
				
				int prefixMargin = 0;
				GUILayout.Space(5);
				
				prefixMargin = 5;
				if(curPSDLayer._isImageLayer)
				{
					if(curPSDLayer._isClipping)
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += (height_ListHeight / 2) + 4;
					}
					EditorGUILayout.LabelField(new GUIContent(curPSDLayer._image), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}
				else
				{
					EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
					prefixMargin += height_ListHeight - 5;
				}

				GUIStyle curGUIStyle = guiStyle_Btn;
				if (!curPSDLayer._isBakable)
				{
					curGUIStyle = guiStyle_Btn_NotBake;
				}
				else if (curPSDLayer == _selectedPSDLayerData)
				{
					curGUIStyle = guiStyle_Btn_Selected;
				}

				int btnWidth = width_LeftInScroll - (prefixMargin + 20);
				
				if (GUILayout.Button("  " + curPSDLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
				{
					_selectedPSDLayerData = curPSDLayer;
					_isLinkLayerToTransform = false;
					_linkSrcLayerData = null;

					apEditorUtil.ReleaseGUIFocus();

				}
				

				EditorGUILayout.EndHorizontal();
				


				iList++;
			}


			GUILayout.Space(height);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : 선택한 레이어의 GUI와 위치 조절 툴 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height));

			

			// <2열 상단 : 레이어와 메시의 GUI>
			Rect guiRect = new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightUpper);
			UpdateAndDrawGUIBase(guiRect, new Vector2(2.4f, 0.2f));
			if(_selectedPSDLayerData != null)
			{
				if (!_isRenderMesh2PSD)
				{
					//PSD -> [Mesh]
					if (_selectedPSDLayerData._image != null && _renderMode_PSD != RENDER_MODE.Hide)
					{	
						DrawPSDLayer(_selectedPSDLayerData, _selectedPSDLayerData._remapPosOffsetDelta_X, _selectedPSDLayerData._remapPosOffsetDelta_Y, _selectedPSDSet._next_meshGroupScaleX100, _renderMode_PSD == RENDER_MODE.Outline);
					}
				}
				
				
				float meshScale = (float)_selectedPSDSet._next_meshGroupScaleX100 / (float)_selectedPSDSet._prev_bakeScale100;
				//float meshScale = 100.0f / (float)_selectedPSDSet._prev_bakeScale100;

				if(_selectedPSDLayerData._isRemapSelected
					&& _selectedPSDLayerData._isImageLayer
					&& _selectedPSDLayerData._remap_MeshTransform != null
					&& _renderMode_Mesh != RENDER_MODE.Hide)
				{
					if(_renderMode_Mesh == RENDER_MODE.Normal)
					{
						//Normal
						DrawMesh(_selectedPSDLayerData._remap_MeshTransform._mesh, false, false, meshScale);
					}
					else
					{
						//Outline
						DrawMeshToneColor(_selectedPSDLayerData._remap_MeshTransform._mesh, false, meshScale);
					}
				}

				if (_isRenderMesh2PSD)
				{
					//[Mesh] -> PSD
					if (_selectedPSDLayerData._image != null)
					{
						if (_selectedPSDLayerData._image != null && _renderMode_PSD != RENDER_MODE.Hide)
						{
							DrawPSDLayer(_selectedPSDLayerData, _selectedPSDLayerData._remapPosOffsetDelta_X, _selectedPSDLayerData._remapPosOffsetDelta_Y, _selectedPSDSet._next_meshGroupScaleX100, _renderMode_PSD == RENDER_MODE.Outline);
						}
					}
				}
				
				if(_selectedPSDLayerData._isRemapSelected
					&& _selectedPSDLayerData._isImageLayer
					&& _selectedPSDLayerData._remap_MeshTransform != null)
				{
					DrawMeshEdgeOnly(_selectedPSDLayerData._remap_MeshTransform._mesh, meshScale);
				}


				//추가 v1.4.2 : 현재 선택된 객체들의 정보를 텍스트로 보여준다.
				_strWrapper_128.Clear();
				_strWrapper_128.Append("[ ", false);
				_strWrapper_128.Append(_selectedPSDLayerData._name, false);

				if (_selectedPSDLayerData._isImageLayer)
				{
					//Image Layer (MeshTF)인 경우
					if (_selectedPSDLayerData._remap_MeshTransform != null)
					{
						//연결될 TF가 있을 때
						_strWrapper_128.Append(" > ", false);
						_strWrapper_128.Append(_selectedPSDLayerData._remap_MeshTransform._nickName, false);
					}
					else
					{
						//연결될 TF가 없을 때
						_strWrapper_128.Append(" (New!)", false);
					}
				}
				else
				{
					//Image Layer가 아닌 경우
					if (_selectedPSDLayerData._remap_MeshGroupTransform != null)
					{
						//연결될 TF가 있을 때
						_strWrapper_128.Append(" > ", false);
						_strWrapper_128.Append(_selectedPSDLayerData._remap_MeshGroupTransform._nickName, false);
					}
					else
					{
						//연결될 TF가 없을 때
						_strWrapper_128.Append(" (New!)", false);
					}
				}
				
				_strWrapper_128.Append(" ]", true);
				DrawText(_strWrapper_128.ToString(), guiRect.x + 5.0f, guiRect.y + 5.0f);
			}


			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height_RightUpper));
			GUILayout.Space(height_RightUpper);
			EditorGUILayout.EndVertical();

			GUILayout.Space(margin);

			//EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height_RightLower));
			// <2열 하단 : 위치 조정 툴> 
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(130), GUILayout.Height(height_RightLower));
			//렌더링 순서와 렌더링 방식
			// Label
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_RenderingNOrder), GUILayout.Width(130));//Rendering & Order
			EditorGUILayout.BeginHorizontal(GUILayout.Width(130), GUILayout.Height(height_RightLower - 24));
			
			//- 1번 렌더링 모드, Switch, 2번 렌더링 모드 (기본은 Mesh 위에 PSD Layer), 

			Texture2D imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			Texture2D imgBtn_PSD = _editor.ImageSet.Get(apImageSet.PRESET.PSD_Set);
			
			if(_renderMode_Mesh == RENDER_MODE.Outline)
			{
				imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.PSD_MeshOutline);
			}
			if(_renderMode_PSD == RENDER_MODE.Outline)
			{
				imgBtn_PSD = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetOutline);
			}

			Texture2D imgBtn_Mod1 = (_isRenderMesh2PSD ? imgBtn_Mesh : imgBtn_PSD);
			Texture2D imgBtn_Mod2 = (_isRenderMesh2PSD ? imgBtn_PSD : imgBtn_Mesh);
			bool isMod1Selected = (_isRenderMesh2PSD ? (_renderMode_Mesh != RENDER_MODE.Hide) : (_renderMode_PSD != RENDER_MODE.Hide));
			bool isMod2Selected = (_isRenderMesh2PSD ? (_renderMode_PSD != RENDER_MODE.Hide) : (_renderMode_Mesh != RENDER_MODE.Hide));
			if(apEditorUtil.ToggledButton_2Side(imgBtn_Mod1, isMod1Selected, true, 46, height_RightLower - (24 + 8)))
			{
				if(_isRenderMesh2PSD)
				{
					//_isRenderOffset_Mesh = !_isRenderOffset_Mesh;
					switch (_renderMode_Mesh)
					{
						case RENDER_MODE.Hide:		_renderMode_Mesh = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_Mesh = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_Mesh = RENDER_MODE.Hide; break;
					}
				}
				else
				{
					//_isRenderOffset_PSD = !_isRenderOffset_PSD;
					switch (_renderMode_PSD)
					{
						case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
					}
				}
			}
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.PSD_Switch)), GUILayout.Width(32), GUILayout.Height(height_RightLower - (24 + 8))))
			{
				_isRenderMesh2PSD = !_isRenderMesh2PSD;
			}
			if(apEditorUtil.ToggledButton_2Side(imgBtn_Mod2, isMod2Selected, true, 46, height_RightLower - (24 + 8)))
			{
				if(_isRenderMesh2PSD)
				{
					//_isRenderOffset_PSD = !_isRenderOffset_PSD;
					switch (_renderMode_PSD)
					{
						case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
					}
				}
				else
				{
					//_isRenderOffset_Mesh = !_isRenderOffset_Mesh;
					switch (_renderMode_Mesh)
					{
						case RENDER_MODE.Hide:		_renderMode_Mesh = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_Mesh = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_Mesh = RENDER_MODE.Hide; break;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			
			//- 위치 보정 X값, Y값
			// 위치 보정 버튼들
			float offsetPosX = 0;
			float offsetPosY = 0;
			if(_selectedPSDLayerData != null)
			{
				offsetPosX = _selectedPSDLayerData._remapPosOffsetDelta_X;
				offsetPosY = _selectedPSDLayerData._remapPosOffsetDelta_Y;
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(120), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PositionOffset), GUILayout.Width(120));//Position Offset
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(120), GUILayout.Height(20));
			EditorGUILayout.LabelField("X", GUILayout.Width(15));
			float nextOffsetPosX = EditorGUILayout.FloatField(offsetPosX, GUILayout.Width(35));
			if(nextOffsetPosX != offsetPosX && _selectedPSDLayerData != null)
			{
				_selectedPSDLayerData._remapPosOffsetDelta_X = nextOffsetPosX;
			}
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Y", GUILayout.Width(15));
			float nextOffsetPosY = EditorGUILayout.FloatField(offsetPosY, GUILayout.Width(35));
			if(nextOffsetPosY != offsetPosY && _selectedPSDLayerData != null)
			{
				_selectedPSDLayerData._remapPosOffsetDelta_Y = nextOffsetPosY;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUILayout.Space(10);
			//X,Y 제어 버튼

			//위치를 쉽게 제어하기 위해서 버튼으로 만들자
			//				Y:(+1, +10)
			//X:(-10, -1)					X:(+1, +10)
			//				Y:(-1, -10)
			int width_contBtn = 40;
			int height_contBtn = 22;
			int width_contBtnArea = width_contBtn * 2 + 4;
			int margin_contBtn = ((height_RightLower - 4) - height_contBtn) / 2;
			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-X 버튼
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X - 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X - 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			//+-Y
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+Y 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y + 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y + 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(margin_contBtn / 2 - 2);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-Y 버튼
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y - 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y - 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+X 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				//X + 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X + 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{ 
				// X + 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X + 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(20);
			

			//"이전" Bake 크기
			EditorGUILayout.BeginVertical(GUILayout.Width(140), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PrevAtlasScale) + " (%)", GUILayout.Width(140));//Prev Atlas Scale
			GUILayout.Space(1);
			int next_PrevBakeScale = EditorGUILayout.DelayedIntField(_selectedPSDSet._prev_bakeScale100, GUILayout.Width(130));
			if(next_PrevBakeScale != _selectedPSDSet._prev_bakeScale100)
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(next_PrevBakeScale, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			int width_scaleBtnWidth = 28;
			int height_scaleBtnWidth = 18;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(140), GUILayout.Height(height_scaleBtnWidth));
			GUILayout.Space(4);
			if(GUILayout.Button("-5", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 -= 5, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 -= 1, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			GUILayout.Space(6);
			if(GUILayout.Button("+1", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 += 1, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+5", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSet._prev_bakeScale100 = Mathf.Clamp(_selectedPSDSet._prev_bakeScale100 += 5, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(20);

			EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height_RightLower));
			//Layer 전환
			// Label
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(100), GUILayout.Height(height_RightLower - 10));
			
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame)), GUILayout.Width(42), GUILayout.Height(height_RightLower - 16)))
			{
				SelectPSDLayer(false);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame)), GUILayout.Width(42), GUILayout.Height(height_RightLower - 16)))
			{
				SelectPSDLayer(true);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			//EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}




		private void GUI_Center_4_ModifyOffset_Secondary(int width, int height, Rect centerRect)
		{
			//레이어를 선택하고 새로운 이미지의 위치를 조정하는 과정
			//좌우 2개. 오른쪽이 크며, 오른쪽은 하단에 툴이 있다.
			//왼쪽 : 레이어 리스트
			//오른쪽 : 레이어 + 메시가 동시에 출력 (또는 전환되어 출력)되는 GUI와 하단의 위치 조정 툴
			int margin = 4;
			int width_Left = 200;
			int width_Right = (width - (width_Left + margin));
			int height_RightLower = 70;
			int height_RightUpper = height - (margin + height_RightLower);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_Left, height), "");
			
			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightUpper), "");
			GUI.backgroundColor = prevColor;

			GUI.Box(new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin + margin + height_RightUpper, width_Right, height_RightLower), "");

			//--------------------------------------
			// <1열 : 레이어 리스트 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height));
			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_PSDLayers));//PSD Layers
			GUILayout.Space(5);

			_scroll_Step4_Left = EditorGUILayout.BeginScrollView(_scroll_Step4_Left, false, true, GUILayout.Width(width_Left), GUILayout.Height(height - 30));
			
			int width_LeftInScroll = (width_Left) - (20);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_LeftInScroll));
			GUILayout.Space(5);
			
			//PSD 레이어를 출력한다. (Bake안되는것은 나오지 않는다)
			apPSDLayerData curPSDLayer = null;
			int iList = 0;
			int height_ListHeight = 26;

			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);


			GUIStyle guiStyle_BtnToggle = new GUIStyle(GUI.skin.button);
			guiStyle_BtnToggle.margin = GUI.skin.textField.margin;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Btn_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Btn_Selected.normal.textColor = Color.white;
			}
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_NotBake = new GUIStyle(GUIStyle.none);
			guiStyle_Btn_NotBake.normal.textColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			guiStyle_Btn_NotBake.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;


			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;
			if (nPSDLayers > 0)
			{
				for (int i = nPSDLayers - 1; i >= 0; i--)
				{
					curPSDLayer = _psdLoader.PSDLayerDataList[i];
					if(curPSDLayer._linkedBakedInfo_Secondary == null)
					{
						//연결 안된건 제외
						continue;
					}

					if (_selectedPSDLayerData == curPSDLayer)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();

						int yOffset = 0;

						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}

						if (iList == 0)
						{
							//GUI.Box(new Rect(lastRect.x, lastRect.y + 5 - 1, width_LeftInScroll + 10, height_ListHeight + 2), "");
							yOffset = 4;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x, lastRect.y + height_ListHeight - 1, width_LeftInScroll + 10, height_ListHeight + 2), "");
							yOffset = height_ListHeight - 1;
						}

						//GUI.backgroundColor = prevColor;

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + yOffset, width_LeftInScroll + 10 - 2, height_ListHeight + 2, apEditorUtil.UNIT_BG_STYLE.Main);
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_LeftInScroll), GUILayout.Height(height_ListHeight));

					int prefixMargin = 0;
					GUILayout.Space(5);

					prefixMargin = 5;
					if (curPSDLayer._isImageLayer)
					{
						if (curPSDLayer._isClipping)
						{
							EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(height_ListHeight / 2), GUILayout.Height(height_ListHeight - 5));
							prefixMargin += (height_ListHeight / 2) + 4;
						}
						EditorGUILayout.LabelField(new GUIContent(curPSDLayer._image), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += height_ListHeight - 5;
					}
					else
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(height_ListHeight - 5), GUILayout.Height(height_ListHeight - 5));
						prefixMargin += height_ListHeight - 5;
					}

					GUIStyle curGUIStyle = guiStyle_Btn;
					if (!curPSDLayer._isBakable)
					{
						curGUIStyle = guiStyle_Btn_NotBake;
					}
					else if (curPSDLayer == _selectedPSDLayerData)
					{
						curGUIStyle = guiStyle_Btn_Selected;
					}

					int btnWidth = width_LeftInScroll - (prefixMargin + 20);

					if (GUILayout.Button("  " + curPSDLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(height_ListHeight)))
					{
						_selectedPSDLayerData = curPSDLayer;
						_isLinkLayerToTransform = false;
						_linkSrcLayerData = null;

						apEditorUtil.ReleaseGUIFocus();

					}


					EditorGUILayout.EndHorizontal();



					iList++;
				}
			}

			GUILayout.Space(height);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : 선택한 레이어의 GUI와 위치 조절 툴 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height));

			

			// <2열 상단 : 레이어와 메시의 GUI>
			Rect guiRect = new Rect(centerRect.xMin + width_Left + margin, centerRect.yMin, width_Right, height_RightUpper);
			UpdateAndDrawGUIBase(guiRect, new Vector2(2.4f, 0.2f));
			if(_selectedPSDLayerData != null)
			{
				if (!_isRenderMesh2PSD)
				{
					//PSD -> [Mesh]
					if (_selectedPSDLayerData._image != null && _renderMode_PSD != RENDER_MODE.Hide)
					{	
						DrawPSDLayer(	_selectedPSDLayerData, 
										_selectedPSDLayerData._remapPosOffsetDelta_X, 
										_selectedPSDLayerData._remapPosOffsetDelta_Y, 
										//_selectedPSDSet._next_meshGroupScaleX100, 
										_selectedPSDSecondary._next_bakeScale100,
										_renderMode_PSD == RENDER_MODE.Outline);
					}
				}
				

				//Secondary인 경우
				if(_selectedPSDLayerData._linkedBakedInfo_Secondary != null)
				{
					if (_renderMode_Mesh != RENDER_MODE.Hide)
					{
						apTextureData textureData = _selectedPSDLayerData._linkedBakedInfo_Secondary._linkedTextureData;

						if (textureData != null)
						{
							Vector2 centerPosOffset = Vector2.zero;
							float textureWidth_Half = textureData._width * 0.5f;
							float textureHeight_Half = textureData._height * 0.5f;

							int bakedPos_Left = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedImagePos_Left;
							int bakedPos_Top = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedImagePos_Top;
							int bakedPos_AreaWidth = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedWidth;
							int bakedPos_AreaHeight = _selectedPSDLayerData._linkedBakedInfo_Secondary._bakedHeight;

							//렌더링할 때 원점에 맞추기 위해 Size*0.5를 뺀다.
							//그럼 그만큼 미리 더한다.
							centerPosOffset.x += textureWidth_Half;
							centerPosOffset.y += textureHeight_Half;

							centerPosOffset.x -= (float)bakedPos_Left + ((float)bakedPos_AreaWidth * 0.5f);
							centerPosOffset.y -= (float)bakedPos_Top + ((float)bakedPos_AreaHeight * 0.5f);

							if (_renderMode_Mesh == RENDER_MODE.Normal)
							{
								//Normal
								DrawTextureData(_selectedPSDLayerData._linkedBakedInfo_Secondary._linkedTextureData,
													false,
													centerPosOffset);
							}
							else
							{
								//Outline
								DrawTextureData_Transparent(_selectedPSDLayerData._linkedBakedInfo_Secondary._linkedTextureData,
																false,
																centerPosOffset);
							}
						}
					}
				}

				if (_isRenderMesh2PSD)
				{
					//[Mesh] -> PSD
					if (_selectedPSDLayerData._image != null)
					{
						if (_selectedPSDLayerData._image != null && _renderMode_PSD != RENDER_MODE.Hide)
						{
							DrawPSDLayer(	_selectedPSDLayerData, 
											_selectedPSDLayerData._remapPosOffsetDelta_X, 
											_selectedPSDLayerData._remapPosOffsetDelta_Y, 
											//_selectedPSDSet._next_meshGroupScaleX100, 
											_selectedPSDSecondary._next_bakeScale100,
											_renderMode_PSD == RENDER_MODE.Outline);
						}
					}
				}
				
				//if(_selectedPSDLayerData._isRemapSelected
				//	&& _selectedPSDLayerData._isImageLayer
				//	&& _selectedPSDLayerData._remap_MeshTransform != null)
				//{
				//	DrawMeshEdgeOnly(_selectedPSDLayerData._remap_MeshTransform._mesh, meshScale);
				//}
				
				//추가 v1.4.2 : 현재 선택된 객체들의 정보를 텍스트로 보여준다.
				_strWrapper_128.Clear();
				_strWrapper_128.Append("[ ", false);
				_strWrapper_128.Append(_selectedPSDLayerData._name, false);
				_strWrapper_128.Append(" ]", true);

				DrawText(_strWrapper_128.ToString(), guiRect.x + 5.0f, guiRect.y + 5.0f);
				
			}


			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height_RightUpper));
			GUILayout.Space(height_RightUpper);
			EditorGUILayout.EndVertical();

			GUILayout.Space(margin);

			//EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height_RightLower));
			// <2열 하단 : 위치 조정 툴> 
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Right), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(130), GUILayout.Height(height_RightLower));
			//렌더링 순서와 렌더링 방식
			// Label
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_RenderingNOrder), GUILayout.Width(130));//Rendering & Order
			EditorGUILayout.BeginHorizontal(GUILayout.Width(130), GUILayout.Height(height_RightLower - 24));
			
			//- 1번 렌더링 모드, Switch, 2번 렌더링 모드 (기본은 Mesh 위에 PSD Layer), 

			//Texture2D imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			Texture2D imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.PSD_LinkedMain);
			Texture2D imgBtn_PSD = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetSecondary);
			
			if(_renderMode_Mesh == RENDER_MODE.Outline)
			{
				//imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.PSD_MeshOutline);
				imgBtn_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.PSD_LinkedMainOutline);
			}
			if(_renderMode_PSD == RENDER_MODE.Outline)
			{
				imgBtn_PSD = _editor.ImageSet.Get(apImageSet.PRESET.PSD_SetSecondaryOutline);
			}

			Texture2D imgBtn_Mod1 = (_isRenderMesh2PSD ? imgBtn_Mesh : imgBtn_PSD);
			Texture2D imgBtn_Mod2 = (_isRenderMesh2PSD ? imgBtn_PSD : imgBtn_Mesh);
			bool isMod1Selected = (_isRenderMesh2PSD ? (_renderMode_Mesh != RENDER_MODE.Hide) : (_renderMode_PSD != RENDER_MODE.Hide));
			bool isMod2Selected = (_isRenderMesh2PSD ? (_renderMode_PSD != RENDER_MODE.Hide) : (_renderMode_Mesh != RENDER_MODE.Hide));
			if(apEditorUtil.ToggledButton_2Side(imgBtn_Mod1, isMod1Selected, true, 46, height_RightLower - (24 + 8)))
			{
				if(_isRenderMesh2PSD)
				{
					//_isRenderOffset_Mesh = !_isRenderOffset_Mesh;
					switch (_renderMode_Mesh)
					{
						case RENDER_MODE.Hide:		_renderMode_Mesh = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_Mesh = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_Mesh = RENDER_MODE.Hide; break;
					}
				}
				else
				{
					//_isRenderOffset_PSD = !_isRenderOffset_PSD;
					switch (_renderMode_PSD)
					{
						case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
					}
				}
			}
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.PSD_Switch)), GUILayout.Width(32), GUILayout.Height(height_RightLower - (24 + 8))))
			{
				_isRenderMesh2PSD = !_isRenderMesh2PSD;
			}
			if(apEditorUtil.ToggledButton_2Side(imgBtn_Mod2, isMod2Selected, true, 46, height_RightLower - (24 + 8)))
			{
				if(_isRenderMesh2PSD)
				{
					//_isRenderOffset_PSD = !_isRenderOffset_PSD;
					switch (_renderMode_PSD)
					{
						case RENDER_MODE.Hide:		_renderMode_PSD = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_PSD = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_PSD = RENDER_MODE.Hide; break;
					}
				}
				else
				{
					//_isRenderOffset_Mesh = !_isRenderOffset_Mesh;
					switch (_renderMode_Mesh)
					{
						case RENDER_MODE.Hide:		_renderMode_Mesh = RENDER_MODE.Normal; break;
						case RENDER_MODE.Normal:	_renderMode_Mesh = RENDER_MODE.Outline; break;
						case RENDER_MODE.Outline:	_renderMode_Mesh = RENDER_MODE.Hide; break;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			
			//- 위치 보정 X값, Y값
			// 위치 보정 버튼들
			float offsetPosX = 0;
			float offsetPosY = 0;
			if(_selectedPSDLayerData != null)
			{
				offsetPosX = _selectedPSDLayerData._remapPosOffsetDelta_X;
				offsetPosY = _selectedPSDLayerData._remapPosOffsetDelta_Y;
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(120), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PositionOffset), GUILayout.Width(120));//Position Offset
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(120), GUILayout.Height(20));
			EditorGUILayout.LabelField("X", GUILayout.Width(15));
			float nextOffsetPosX = EditorGUILayout.FloatField(offsetPosX, GUILayout.Width(35));
			if(nextOffsetPosX != offsetPosX && _selectedPSDLayerData != null)
			{
				_selectedPSDLayerData._remapPosOffsetDelta_X = nextOffsetPosX;
			}
			GUILayout.Space(5);
			EditorGUILayout.LabelField("Y", GUILayout.Width(15));
			float nextOffsetPosY = EditorGUILayout.FloatField(offsetPosY, GUILayout.Width(35));
			if(nextOffsetPosY != offsetPosY && _selectedPSDLayerData != null)
			{
				_selectedPSDLayerData._remapPosOffsetDelta_Y = nextOffsetPosY;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			GUILayout.Space(10);
			//X,Y 제어 버튼

			//위치를 쉽게 제어하기 위해서 버튼으로 만들자
			//				Y:(+1, +10)
			//X:(-10, -1)					X:(+1, +10)
			//				Y:(-1, -10)
			int width_contBtn = 40;
			int height_contBtn = 22;
			int width_contBtnArea = width_contBtn * 2 + 4;
			int margin_contBtn = ((height_RightLower - 4) - height_contBtn) / 2;
			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-X 버튼
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X - 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// X - 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X - 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			//+-Y
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+Y 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y + 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y + 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y + 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(margin_contBtn / 2 - 2);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//-Y 버튼
			if(GUILayout.Button("-1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y - 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				// Y - 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_Y = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_Y - 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_RightLower));
			GUILayout.Space(margin_contBtn);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_contBtnArea), GUILayout.Height(height_contBtn + 2));
			//+X 버튼
			if(GUILayout.Button("+1", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{
				//X + 1
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X + 1);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+10", GUILayout.Width(width_contBtn), GUILayout.Height(height_contBtn)))
			{ 
				// X + 10
				if(_selectedPSDLayerData != null)
				{
					_selectedPSDLayerData._remapPosOffsetDelta_X = GetCorrectedFloat(_selectedPSDLayerData._remapPosOffsetDelta_X + 10);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(20);



			//"이전" Bake 크기
			EditorGUILayout.BeginVertical(GUILayout.Width(140), GUILayout.Height(height_RightLower));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PrevAtlasScale) + " (%)", GUILayout.Width(140));//Prev Atlas Scale
			GUILayout.Space(1);


			int next_PrevBakeScale = EditorGUILayout.DelayedIntField(_selectedPSDSecondary._next_bakeScale100, GUILayout.Width(130));
			if(next_PrevBakeScale != _selectedPSDSecondary._next_bakeScale100)
			{
				_selectedPSDSecondary._next_bakeScale100 = Mathf.Clamp(next_PrevBakeScale, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}


			int width_scaleBtnWidth = 28;
			int height_scaleBtnWidth = 18;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(140), GUILayout.Height(height_scaleBtnWidth));
			GUILayout.Space(4);
			if(GUILayout.Button("-5", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSecondary._next_bakeScale100 = Mathf.Clamp(_selectedPSDSecondary._next_bakeScale100 -= 5, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("-1", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSecondary._next_bakeScale100 = Mathf.Clamp(_selectedPSDSecondary._next_bakeScale100 -= 1, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			GUILayout.Space(6);
			if(GUILayout.Button("+1", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSecondary._next_bakeScale100 = Mathf.Clamp(_selectedPSDSecondary._next_bakeScale100 += 1, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button("+5", GUILayout.Width(width_scaleBtnWidth), GUILayout.Height(height_scaleBtnWidth)))
			{
				_selectedPSDSecondary._next_bakeScale100 = Mathf.Clamp(_selectedPSDSecondary._next_bakeScale100 += 5, 5, 10000);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			GUILayout.Space(20);

			EditorGUILayout.BeginVertical(GUILayout.Width(100), GUILayout.Height(height_RightLower));
			//Layer 전환
			// Label
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(100), GUILayout.Height(height_RightLower - 10));
			
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame)), GUILayout.Width(42), GUILayout.Height(height_RightLower - 16)))
			{
				SelectPSDLayer(false);
				apEditorUtil.ReleaseGUIFocus();
			}
			if(GUILayout.Button(new GUIContent(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame)), GUILayout.Width(42), GUILayout.Height(height_RightLower - 16)))
			{
				SelectPSDLayer(true);
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			//EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();
			//--------------------------------------
			EditorGUILayout.EndHorizontal();
		}


		private void SelectPSDLayer(bool isNext)
		{
			//모드에 따라서 다르다
			bool isSecondaryMode = _selectedPSDSecondary != null;
			
			apPSDLayerData curLayer = null;
			if (_selectedPSDLayerData == null)
			{
				if (_psdLoader.PSDLayerDataList.Count > 0)
				{
					//맨 마지막 부터 역순으로 이동
					for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
					{
						curLayer = _psdLoader.PSDLayerDataList[i];
						if(curLayer._isBakable)
						{
							if((isSecondaryMode && curLayer._linkedBakedInfo_Secondary != null)
								|| !isSecondaryMode)
							{
								_selectedPSDLayerData = curLayer;
								return;
							}
						}
					}
				}
				//0부터 시작해서 Bake되는 레이어를 찾자
			}
			else
			{
				int curIndex = _psdLoader.PSDLayerDataList.IndexOf(_selectedPSDLayerData);
				if(curIndex < 0)
				{
					return;
				}
				if(isNext)
				{
					//Index를 줄여가면서 확인
					curIndex -= 1;
					while(true)
					{
						if(curIndex < 0)
						{
							return;
						}

						curLayer = _psdLoader.PSDLayerDataList[curIndex];
						if(curLayer._isBakable)
						{
							if ((isSecondaryMode && curLayer._linkedBakedInfo_Secondary != null)
								|| !isSecondaryMode)
							{
								_selectedPSDLayerData = curLayer;
								return;
							}
						}

						curIndex--;
					}
				}
				else
				{
					//Index를 늘려가면서 확인
					curIndex += 1;

					while(true)
					{
						if(curIndex >= _psdLoader.PSDLayerDataList.Count)
						{
							return;
						}

						curLayer = _psdLoader.PSDLayerDataList[curIndex];
						if(curLayer._isBakable)
						{
							if ((isSecondaryMode && curLayer._linkedBakedInfo_Secondary != null)
								|| !isSecondaryMode)
							{
								_selectedPSDLayerData = curLayer;
								return;
							}
						}
						curIndex++;
					}
				}
			}
				
		}





		//--------------------------------------------------------------------
		// Step 5 : 아틀라스 설정하고 굽기
		//--------------------------------------------------------------------
		
		private void GUI_Center_5_AtlasSetting_Main(int width, int height, Rect centerRect)
		{
			//텍스쳐 아틀라스 Bake 설정
			//PSD와 동일하게 3개의 열로 구성된다.
			//1열 : 생성된 Atlas 미리보기 GUI
			//2열 : 생성된 Atlas 리스트
			//3열 : Atlas 정보
			int margin = 4;
			
			int width_2 = 200;
			int width_3 = 300;
			int width_1 = width - (width_2 + margin + width_3 + margin);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_1, height), "");
			GUI.backgroundColor = prevColor;
			
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin, width_2, height), "");
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height), "");
			

			//--------------------------------------
			// <1열 : GUI : Atlas 미리보기 >
			Rect guiRect = new Rect(centerRect.xMin, centerRect.yMin, width_1, height);
			UpdateAndDrawGUIBase(guiRect, new Vector2(-3f, 3f));

			// Bake Atlas를 렌더링하자
			if (_psdLoader.BakeDataList.Count > 0 && !_psdLoader.IsImageBaking)
			{
				if (_selectedPSDBakeData == null)
				{
					apPSDBakeData curBakedData = null;
					//for (int i = 0; i < _bakeDataList.Count; i++)//이전 코드
					for (int i = 0; i < _psdLoader.BakeDataList.Count; i++)
					{
						//curBakedData = _bakeDataList[i];//이전 코드
						curBakedData = _psdLoader.BakeDataList[i];

						Vector2 imgPosOffset = new Vector2(curBakedData._width * i, 0);

						_gl.DrawTexture(curBakedData._bakedImage,
											new Vector2(curBakedData._width / 2, curBakedData._height / 2) + imgPosOffset,
											curBakedData._width, curBakedData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											false);
					}
				}
				else
				{
					_gl.DrawTexture(_selectedPSDBakeData._bakedImage,
											new Vector2(_selectedPSDBakeData._width / 2, _selectedPSDBakeData._height / 2),
											_selectedPSDBakeData._width, _selectedPSDBakeData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											true);
				}
			}



			EditorGUILayout.BeginVertical(GUILayout.Width(width_1), GUILayout.Height(height));
			GUILayout.Space(5);

			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : Atlas 리스트 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_2), GUILayout.Height(height));
			
			Texture2D icon_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);
			int itemHeight = 30;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)		{ guiStyle_Btn_Selected.normal.textColor = Color.cyan; }
			else								{ guiStyle_Btn_Selected.normal.textColor = Color.white; }
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;


			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_2));
			GUILayout.Space(5);
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Deselect), GUILayout.Width(width_2 - 7), GUILayout.Height(18)))//Deselect
			{
				if (IsGUIUsable)
				{
					_selectedPSDBakeData = null;
				}
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			_scroll_Step5_Left = EditorGUILayout.BeginScrollView(_scroll_Step5_Left, false, true, GUILayout.Width(width_2), GUILayout.Height(height - 41));
			
			int width_Line2InScroll = width_2 - 24;

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line2InScroll));

			GUILayout.Space(1);
			int iList = 0;
			if (_psdLoader.BakeDataList.Count > 0)
			{
				apPSDBakeData curBakeData = null;
				//for (int i = 0; i < _bakeDataList.Count; i++)//이전 코드
				for (int i = 0; i < _psdLoader.BakeDataList.Count; i++)
				{
					GUIStyle curGUIStyle = guiStyle_Btn;

					//curBakeData = _bakeDataList[i];//이전 코드
					curBakeData = _psdLoader.BakeDataList[i];

					if (_selectedPSDBakeData == curBakeData)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						int yOffset = 0;
						
						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}

						if (iList == 0)
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 1, width_Line2InScroll + 10, itemHeight), "");
							yOffset = 1;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 30, width_Line2InScroll + 10, itemHeight), "");
							yOffset = 30;
						}

						GUI.backgroundColor = prevColor;


						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1 + 1, lastRect.y + yOffset, width_Line2InScroll + 10 - 2, itemHeight, apEditorUtil.UNIT_BG_STYLE.Main);


						curGUIStyle = guiStyle_Btn_Selected;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line2InScroll), GUILayout.Height(itemHeight));

					GUILayout.Space(5);
					EditorGUILayout.LabelField(new GUIContent(icon_Image), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));


					if (GUILayout.Button("  " + curBakeData.Name, curGUIStyle, GUILayout.Width(width_Line2InScroll - (5 + itemHeight)), GUILayout.Height(itemHeight)))
					{
						if (IsGUIUsable)
						{
							_selectedPSDBakeData = curBakeData;
						}
					}

					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}

			GUILayout.Space(height + 20);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();


			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <3열 : Atlas Bake 설정 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_3), GUILayout.Height(height));

			width_3 -= 20;

			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AssetName), GUILayout.Width(width_3));//Asset Name
			EditorGUI.BeginChangeCheck();
			string next_fileNameOnly = EditorGUILayout.DelayedTextField(_psdLoader.FileName, GUILayout.Width(width_3));
			if(EditorGUI.EndChangeCheck())
			{
				if (IsGUIUsable)
				{
					//_fileNameOnly = next_fileNameOnly;
					_psdLoader.SetFileName(next_fileNameOnly);
				}
			}
			

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SavePath), GUILayout.Width(width_3));//Save Path
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));


			//string prev_bakeDstFilePath = _selectedPSDSet._bakeOption_DstFilePath;


			EditorGUI.BeginChangeCheck();

			string next_bakeDstFilePath = EditorGUILayout.DelayedTextField(_selectedPSDSet._bakeOption_DstFilePath, GUILayout.Width(width_3 - 64));

			if (EditorGUI.EndChangeCheck())
			{
				if (IsGUIUsable)
				{
					//이전
					//if (!string.Equals(_selectedPSDSet._bakeOption_DstFilePath, next_bakeDstFilePath))
					//{
					//	_selectedPSDSet._bakeOption_DstFilePath = next_bakeDstFilePath;

					//	int subStartLength = Application.dataPath.Length;

					//	_selectedPSDSet._bakeOption_DstFileRelativePath = "Assets";
					//	if (_selectedPSDSet._bakeOption_DstFilePath.Length > subStartLength)
					//	{
					//		_selectedPSDSet._bakeOption_DstFileRelativePath += _selectedPSDSet._bakeOption_DstFilePath.Substring(subStartLength);
					//	}
					//}

					//변경 22.7.1
					string resultPathFull = "";
					string resultPathRelative = "";
					bool isValidPath = apEditorUtil.MakeRelativeDirectoryPathFromAssets(next_bakeDstFilePath, ref resultPathFull, ref resultPathRelative);
					if (isValidPath)
					{
						_selectedPSDSet._bakeOption_DstFilePath = resultPathFull;
						_selectedPSDSet._bakeOption_DstFileRelativePath = resultPathRelative;
					}
					else
					{
						//유효하지 않다면, 상대 경로는 걍 Assets
						_selectedPSDSet._bakeOption_DstFilePath = next_bakeDstFilePath;
						_selectedPSDSet._bakeOption_DstFileRelativePath = "Assets";
					}
				}
			}
			
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Set), GUILayout.Width(60)))//Set
			{
				if (IsGUIUsable)
				{
					string defaultPath = _selectedPSDSet._bakeOption_DstFileRelativePath;
					if(string.IsNullOrEmpty(defaultPath))
					{
						defaultPath = "Assets";
					}

					//이전
					//_selectedPSDSet._bakeOption_DstFilePath = EditorUtility.SaveFolderPanel("Save Path Folder", defaultPath, "");
					//if (!_selectedPSDSet._bakeOption_DstFilePath.StartsWith(Application.dataPath))
					//{

					//	//EditorUtility.DisplayDialog("Bake Destination Path Error", "Bake Destination Path is have to be in Asset Folder", "Okay");
					//	EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_WrongDst),
					//									_editor.GetText(TEXT.PSDBakeError_Body_WrongDst),
					//									_editor.GetText(TEXT.Close)
					//									);

					//	_selectedPSDSet._bakeOption_DstFilePath = "";
					//	_selectedPSDSet._bakeOption_DstFileRelativePath = "";
					//}
					//else
					//{
					//	//앞의 걸 빼고 나면 (..../Assets) + ....가 된다.
					//	//Relatives는 "Assets/..."로 시작해야한다.
					//	int subStartLength = Application.dataPath.Length;
					//	_selectedPSDSet._bakeOption_DstFileRelativePath = "Assets";
					//	if (_selectedPSDSet._bakeOption_DstFilePath.Length > subStartLength)
					//	{
					//		_selectedPSDSet._bakeOption_DstFileRelativePath += _selectedPSDSet._bakeOption_DstFilePath.Substring(subStartLength);
					//	}

					//	//추가 21.7.3 : Escape 문자 삭제
					//	_selectedPSDSet._bakeOption_DstFilePath = apUtil.ConvertEscapeToPlainText(_selectedPSDSet._bakeOption_DstFilePath);
					//	_selectedPSDSet._bakeOption_DstFileRelativePath = apUtil.ConvertEscapeToPlainText(_selectedPSDSet._bakeOption_DstFileRelativePath);
					//}



					//변경 22.7.1					
					string nextPathFromDialog = EditorUtility.SaveFolderPanel("Save Path Folder", defaultPath, "");

					if (!string.IsNullOrEmpty(nextPathFromDialog))
					{
						string resultPathFull = "";
						string resultPathRelative = "";
						bool isValidPath = apEditorUtil.MakeRelativeDirectoryPathFromAssets(nextPathFromDialog, ref resultPathFull, ref resultPathRelative);
						if (isValidPath)
						{
							_selectedPSDSet._bakeOption_DstFilePath = resultPathFull;
							_selectedPSDSet._bakeOption_DstFileRelativePath = resultPathRelative;
						}
						else
						{
							//유효하지 않다면, 상대 경로는 걍 Assets
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_WrongDst),
															_editor.GetText(TEXT.PSDBakeError_Body_WrongDst),
															_editor.GetText(TEXT.Close)
															);

							_selectedPSDSet._bakeOption_DstFilePath = "";
							_selectedPSDSet._bakeOption_DstFileRelativePath = "Assets";
						}
					}
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);


			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AtlasBakingOption), GUILayout.Width(width_3));//Atlas Baking Option
			GUILayout.Space(10);


			apPSDLoader.BAKE_SIZE prev_bakeWidth = BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Width);
			apPSDLoader.BAKE_SIZE prev_bakeHeight = BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Height);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Atlas) + " " + _editor.GetText(TEXT.DLG_Width) + " : ", GUILayout.Width(120));//Atlas Width
			apPSDLoader.BAKE_SIZE next_bakeWidth = (apPSDLoader.BAKE_SIZE)EditorGUILayout.Popup((int)_selectedPSDSet._bakeOption_Width, _bakeDescription, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_Width = BakeSizeLoader2PSDSet(next_bakeWidth);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Atlas) + " " + _editor.GetText(TEXT.DLG_Height) + " : ", GUILayout.Width(120));//Atlas Height
			apPSDLoader.BAKE_SIZE next_bakeHeight = (apPSDLoader.BAKE_SIZE)EditorGUILayout.Popup((int)_selectedPSDSet._bakeOption_Height, _bakeDescription, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_Height = BakeSizeLoader2PSDSet(next_bakeHeight);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			int prev_bakeMaximumNumAtlas = _selectedPSDSet._bakeOption_MaximumNumAtlas;
			int prev_bakePadding = _selectedPSDSet._bakeOption_Padding;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_MaximumAtlas) + " : ", GUILayout.Width(120));//Maximum Atlas
			int next_bakeMaximumNumAtlas = EditorGUILayout.DelayedIntField(_selectedPSDSet._bakeOption_MaximumNumAtlas, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_MaximumNumAtlas = next_bakeMaximumNumAtlas;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Padding) + " : ", GUILayout.Width(120));//Padding
			int next_bakePadding = EditorGUILayout.DelayedIntField(_selectedPSDSet._bakeOption_Padding, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_Padding = next_bakePadding;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			bool prev_bakeBlurOption = _selectedPSDSet._bakeOption_BlurOption;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_FixBorderProblem) + " : ", GUILayout.Width(120));//Fix Border Problem
			bool next_bakeBlurOption = EditorGUILayout.Toggle(_selectedPSDSet._bakeOption_BlurOption, GUILayout.Width(width_3 - 124));
			if (IsGUIUsable)
			{
				_selectedPSDSet._bakeOption_BlurOption = next_bakeBlurOption;
			}
			EditorGUILayout.EndHorizontal();


			//이제 Bake 가능한지 체크하자
			GUILayout.Space(10);
			if (
				//prev_isBakeResizable != _isBakeResizable ||
				prev_bakeWidth != BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Width) ||
				prev_bakeHeight != BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Height) ||
				prev_bakeMaximumNumAtlas != _selectedPSDSet._bakeOption_MaximumNumAtlas ||
				prev_bakePadding != _selectedPSDSet._bakeOption_Padding ||
				//!string.Equals(prev_bakeDstFilePath, _selectedPSDSet._bakeOption_DstFilePath) ||//삭제 v1.4.2
				prev_bakeBlurOption != _selectedPSDSet._bakeOption_BlurOption)
			{
				_isNeedBakeCheck = true;
			}

			if (_isNeedBakeCheck)
			{
				//이전 코드
				//CheckBakable();

				//Calculate를 하자
				_psdLoader.Step2_Calculate(
					//_selectedPSDSet._bakeOption_DstFilePath, _selectedPSDSet._bakeOption_DstFileRelativePath,//삭제 v1.4.2
					 GetBakeIntSize(_selectedPSDSet._bakeOption_Width),
					 GetBakeIntSize(_selectedPSDSet._bakeOption_Height),
					_selectedPSDSet._bakeOption_MaximumNumAtlas, 
					_selectedPSDSet._bakeOption_Padding,
					_selectedPSDSet._bakeOption_BlurOption,
					OnCalculateResult
					);
			}

			
			GUIStyle guiStyle_Result = new GUIStyle(GUI.skin.box);
			guiStyle_Result.alignment = TextAnchor.MiddleLeft;
			guiStyle_Result.normal.textColor = apEditorUtil.BoxTextColor;

			//경로가 없을때도 오류 메시지가 나오도록
			if(string.IsNullOrEmpty(_selectedPSDSet._bakeOption_DstFilePath))
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;


				GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				//Warning
				GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Warning) + "\n[Save Path] is Empty", guiStyle_WarningBox, GUILayout.Width(width_3), GUILayout.Height(70));

				GUI.backgroundColor = prevColor;
			}
			else if (_isBakeWarning)
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;


				GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				//Warning
				GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Warning) + "\n" + _bakeWarningMsg, guiStyle_WarningBox, GUILayout.Width(width_3), GUILayout.Height(70));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Bake), GUILayout.Width(width_3), GUILayout.Height(40)))//Bake
				{
					_selectedPSDBakeData = null;//<< 선택된게 해제되어야 한다.

					if (IsGUIUsable)
					{
						//이전 코드
						//StartBake();

						_psdLoader.Step3_Bake(_loadKey_Bake, OnBakeResult, _loadKey_Calculated);
					}
				}
				if (_loadKey_Calculated != _loadKey_Bake)
				{
					GUILayout.Space(10);
					GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
					guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
					guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;

					GUI.backgroundColor = new Color(0.6f, 0.6f, 1.0f, 1.0f);

					//이전 : PSD Loader 사용 전
					//GUILayout.Box("[ Settings are changed ]"
					//				+ "\n  Expected Scale : " + _realBakeResizeX100 + " %"
					//				+ "\n  Expected Atlas : " + _realBakedAtlasCount,
					//				guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

					//GUILayout.Box("[ Settings are changed ]"
					//				+ "\n  Expected Scale : " + _psdLoader.CalculatedResizeX100 + " %"
					//				+ "\n  Expected Atlas : " + _psdLoader.CalculatedAtlasCount,
					//				guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));

					GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_SettingsAreChanged) + " ]"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedScale) + " : " + _psdLoader.CalculatedResizeX100 + " %"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedAtlas) + " : " + _psdLoader.CalculatedAtlasCount,
									guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));


					
					GUI.backgroundColor = prevColor;

				}


			}
			GUILayout.Space(10);
			if (_loadKey_Bake != null)
			{
				//Bake가 되었다면 => 그 정보를 넣어주자
				//이전 : PSD Loader 사용 전
				GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_BakeResult) + " ]"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ScalePercent) + " : " + _psdLoader.BakedResizeX100 + " %"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_Atlas) + " : " + _psdLoader.BakedAtlasCount,
								guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));
				
			}


			GUILayout.Space(20);

			if (IsProcessRunning)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();

				Rect barRect = new Rect(lastRect.x + 5, lastRect.y + 30, width_3 - 5, 20);

				//이전 : PSD Loader 사용 전
				//float barRatio = Mathf.Clamp01((float)_workProcess.ProcessX100 / 100.0f);
				//EditorGUI.ProgressBar(barRect, barRatio, _threadProcessName);

				float barRatio = _psdLoader.GetImageBakingRatio();
				EditorGUI.ProgressBar(barRect, barRatio, _psdLoader.GetProcessLabel());

			}


			EditorGUILayout.EndVertical();
			//--------------------------------------


			EditorGUILayout.EndHorizontal();
		}




		private void GUI_Center_5_AtlasSetting_Secondary(int width, int height, Rect centerRect)
		{
			//텍스쳐 아틀라스 Bake 설정
			//PSD와 동일하게 3개의 열로 구성된다.
			//1열 : 생성된 Atlas 미리보기 GUI
			//2열 : 생성된 Atlas 리스트
			//3열 : Atlas 정보
			int margin = 4;
			
			int width_2 = 200;
			int width_3 = 300;
			int width_1 = width - (width_2 + margin + width_3 + margin);
			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			GUI.backgroundColor = _glBackGroundColor;
			GUI.Box(new Rect(centerRect.xMin, centerRect.yMin, width_1, height), "");
			GUI.backgroundColor = prevColor;
			
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin, centerRect.yMin, width_2, height), "");
			GUI.Box(new Rect(centerRect.xMin + width_1 + margin + width_2 + margin, centerRect.yMin, width_3, height), "");
			

			//--------------------------------------
			// <1열 : GUI : Atlas 미리보기 >
			Rect guiRect = new Rect(centerRect.xMin, centerRect.yMin, width_1, height);
			UpdateAndDrawGUIBase(guiRect, new Vector2(-3f, 3f));

			// Bake Atlas를 렌더링하자
			if (_psdLoader.BakeDataList.Count > 0 && !_psdLoader.IsImageBaking)
			{
				if (_selectedPSDBakeData == null)
				{
					apPSDBakeData curBakedData = null;
					//for (int i = 0; i < _bakeDataList.Count; i++)//이전 코드
					for (int i = 0; i < _psdLoader.BakeDataList.Count; i++)
					{
						//curBakedData = _bakeDataList[i];//이전 코드
						curBakedData = _psdLoader.BakeDataList[i];

						Vector2 imgPosOffset = new Vector2(curBakedData._width * i, 0);

						_gl.DrawTexture(curBakedData._bakedImage,
											new Vector2(curBakedData._width / 2, curBakedData._height / 2) + imgPosOffset,
											curBakedData._width, curBakedData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											false);
					}
				}
				else
				{
					_gl.DrawTexture(_selectedPSDBakeData._bakedImage,
											new Vector2(_selectedPSDBakeData._width / 2, _selectedPSDBakeData._height / 2),
											_selectedPSDBakeData._width, _selectedPSDBakeData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											true);
				}
			}



			EditorGUILayout.BeginVertical(GUILayout.Width(width_1), GUILayout.Height(height));
			GUILayout.Space(5);

			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <2열 : Atlas 리스트 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_2), GUILayout.Height(height));
			
			Texture2D icon_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);
			int itemHeight = 30;

			GUIStyle guiStyle_Btn = new GUIStyle(GUIStyle.none);
			guiStyle_Btn.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Btn.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Btn_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)		{ guiStyle_Btn_Selected.normal.textColor = Color.cyan; }
			else								{ guiStyle_Btn_Selected.normal.textColor = Color.white; }
			guiStyle_Btn_Selected.alignment = TextAnchor.MiddleLeft;

			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;


			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_2));
			GUILayout.Space(5);
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Deselect), GUILayout.Width(width_2 - 7), GUILayout.Height(18)))//Deselect
			{
				if (IsGUIUsable)
				{
					_selectedPSDBakeData = null;
				}
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			_scroll_Step5_Left = EditorGUILayout.BeginScrollView(_scroll_Step5_Left, false, true, GUILayout.Width(width_2), GUILayout.Height(height - 41));
			
			int width_Line2InScroll = width_2 - 24;

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Line2InScroll));

			GUILayout.Space(1);
			int iList = 0;
			if (_psdLoader.BakeDataList.Count > 0)
			{
				apPSDBakeData curBakeData = null;
				
				
				for (int i = 0; i < _psdLoader.BakeDataList.Count; i++)
				{
					GUIStyle curGUIStyle = guiStyle_Btn;

					curBakeData = _psdLoader.BakeDataList[i];

					if (_selectedPSDBakeData == curBakeData)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						int yOffset = 0;

						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}


						//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
						if (iList == 0)
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 1, width_Line2InScroll + 10, itemHeight), "");
							yOffset = 1;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 30, width_Line2InScroll + 10, itemHeight), "");
							yOffset = 30;
						}

						//GUI.backgroundColor = prevColor;

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1 + 1, lastRect.y + yOffset, width_Line2InScroll + 10 - 2, itemHeight, apEditorUtil.UNIT_BG_STYLE.Main);

						curGUIStyle = guiStyle_Btn_Selected;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_Line2InScroll), GUILayout.Height(itemHeight));

					GUILayout.Space(5);
					EditorGUILayout.LabelField(new GUIContent(icon_Image), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));


					if (GUILayout.Button("  " + curBakeData.Name, curGUIStyle, GUILayout.Width(width_Line2InScroll - (5 + itemHeight)), GUILayout.Height(itemHeight)))
					{
						if (IsGUIUsable)
						{
							_selectedPSDBakeData = curBakeData;
						}
					}

					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}

			GUILayout.Space(height + 20);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();


			EditorGUILayout.EndVertical();
			//--------------------------------------

			GUILayout.Space(margin);

			//--------------------------------------
			// <3열 : Atlas Bake 설정 >
			EditorGUILayout.BeginVertical(GUILayout.Width(width_3), GUILayout.Height(height));

			width_3 -= 20;

			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AssetName), GUILayout.Width(width_3));//Asset Name
			string next_fileNameOnly = EditorGUILayout.DelayedTextField(_psdLoader.FileName, GUILayout.Width(width_3));
			//if (IsGUIUsable)
			{
				//_fileNameOnly = next_fileNameOnly;
				_psdLoader.SetFileName(next_fileNameOnly);
			}

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SavePath), GUILayout.Width(width_3));//Save Path
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			//string prev_bakeDstFilePath = _selectedPSDSet._bakeOption_DstFilePath;
			//string prev_bakeDstFilePath = _selectedPSDSecondary._dstFilePath;

			EditorGUI.BeginChangeCheck();
			string next_bakeDstFilePath = EditorGUILayout.DelayedTextField(_selectedPSDSecondary._dstFilePath, GUILayout.Width(width_3 - 64));

			if (EditorGUI.EndChangeCheck())
			{
				if (IsGUIUsable)
				{
					//이전
					//if (!string.Equals(_selectedPSDSecondary._dstFilePath, next_bakeDstFilePath))
					//{
					//	_selectedPSDSecondary._dstFilePath = next_bakeDstFilePath;

					//	int subStartLength = Application.dataPath.Length;

					//	_selectedPSDSecondary._dstFilePath_Relative = "Assets";
					//	if (_selectedPSDSecondary._dstFilePath.Length > subStartLength)
					//	{
					//		_selectedPSDSecondary._dstFilePath_Relative += _selectedPSDSecondary._dstFilePath.Substring(subStartLength);
					//	}
					//}

					//변경 22.7.1
					string resultPathFull = "";
					string resultPathRelative = "";
					bool isValidPath = apEditorUtil.MakeRelativeDirectoryPathFromAssets(next_bakeDstFilePath, ref resultPathFull, ref resultPathRelative);
					if (isValidPath)
					{
						_selectedPSDSecondary._dstFilePath = resultPathFull;
						_selectedPSDSecondary._dstFilePath_Relative = resultPathRelative;
					}
					else
					{
						//유효하지 않다면, 상대 경로는 걍 Assets
						_selectedPSDSecondary._dstFilePath = next_bakeDstFilePath;
						_selectedPSDSecondary._dstFilePath_Relative = "Assets";
					}
				}
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Set), GUILayout.Width(60)))//Set
			{
				if (IsGUIUsable)
				{
					string defaultPath = _selectedPSDSecondary._dstFilePath_Relative;
					if(string.IsNullOrEmpty(defaultPath))
					{
						defaultPath = "Assets";
					}

					//이전
					//_selectedPSDSecondary._dstFilePath = EditorUtility.SaveFolderPanel("Save Path Folder", defaultPath, "");

					//if (!_selectedPSDSecondary._dstFilePath.StartsWith(Application.dataPath))
					//{

					//	//EditorUtility.DisplayDialog("Bake Destination Path Error", "Bake Destination Path is have to be in Asset Folder", "Okay");
					//	EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_WrongDst),
					//									_editor.GetText(TEXT.PSDBakeError_Body_WrongDst),
					//									_editor.GetText(TEXT.Close)
					//									);

					//	_selectedPSDSecondary._dstFilePath = "";
					//	_selectedPSDSecondary._dstFilePath_Relative = "";
					//}
					//else
					//{
					//	//앞의 걸 빼고 나면 (..../Assets) + ....가 된다.
					//	//Relatives는 "Assets/..."로 시작해야한다.
					//	int subStartLength = Application.dataPath.Length;
					//	_selectedPSDSecondary._dstFilePath_Relative = "Assets";
					//	if (_selectedPSDSecondary._dstFilePath.Length > subStartLength)
					//	{
					//		_selectedPSDSecondary._dstFilePath_Relative += _selectedPSDSecondary._dstFilePath.Substring(subStartLength);
					//	}

					//	//추가 21.7.3 : Escape 문자 삭제
					//	_selectedPSDSecondary._dstFilePath = apUtil.ConvertEscapeToPlainText(_selectedPSDSecondary._dstFilePath);
					//	_selectedPSDSecondary._dstFilePath_Relative = apUtil.ConvertEscapeToPlainText(_selectedPSDSecondary._dstFilePath_Relative);
					//}

					//변경 22.7.1

					string nextPathFromDialog = EditorUtility.SaveFolderPanel("Save Path Folder", defaultPath, "");

					if (!string.IsNullOrEmpty(nextPathFromDialog))
					{
						string resultPathFull = "";
						string resultPathRelative = "";
						bool isValidPath = apEditorUtil.MakeRelativeDirectoryPathFromAssets(nextPathFromDialog, ref resultPathFull, ref resultPathRelative);
						if (isValidPath)
						{
							_selectedPSDSecondary._dstFilePath = resultPathFull;
							_selectedPSDSecondary._dstFilePath_Relative = resultPathRelative;
						}
						else
						{
							//유효하지 않다면, 상대 경로는 걍 Assets
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_WrongDst),
															_editor.GetText(TEXT.PSDBakeError_Body_WrongDst),
															_editor.GetText(TEXT.Close)
															);

							_selectedPSDSecondary._dstFilePath = "";
							_selectedPSDSecondary._dstFilePath_Relative = "Assets";
						}
					}
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);


			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AtlasBakingOption), GUILayout.Width(width_3));//Atlas Baking Option
			GUILayout.Space(10);


			//Secondary에서 Bake 옵션은 수정될 수 없다.
			//UV가 같아야 하기 때문
			


			//색상은 선택한다
			bool isBGChanged = false;
			Color nextBGColor = _selectedPSDSecondary._backgroundColor;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_3));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_BGColor) + " : ", GUILayout.Width(120));//Fix Border Problem

			EditorGUI.BeginChangeCheck();
			try
			{	
				nextBGColor = EditorGUILayout.ColorField(_selectedPSDSecondary._backgroundColor, GUILayout.Width(width_3 - 124));
			}
			catch(Exception)
			{
				//Debug.LogError("Color Ex : " + ex);
			}
			if(EditorGUI.EndChangeCheck())
			{
				if(IsGUIUsable)
				{
					_selectedPSDSecondary._backgroundColor = nextBGColor;
					isBGChanged = true;
				}
			}
			EditorGUILayout.EndHorizontal();


			//이제 Bake 가능한지 체크하자




			GUILayout.Space(10);

			if (
				//prev_isBakeResizable != _isBakeResizable ||
				//prev_bakeWidth != BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Width) ||
				//prev_bakeHeight != BakeSizePSDSet2Loader(_selectedPSDSet._bakeOption_Height) ||
				//prev_bakeMaximumNumAtlas != _selectedPSDSet._bakeOption_MaximumNumAtlas ||
				//prev_bakePadding != _selectedPSDSet._bakeOption_Padding ||
				//!string.Equals(prev_bakeDstFilePath, _selectedPSDSecondary._dstFilePath)//삭제 v1.4.2
				isBGChanged
				//|| prev_bakeBlurOption != _selectedPSDSet._bakeOption_BlurOption
				)
			{
				_isNeedBakeCheck = true;
			}

			if (_isNeedBakeCheck)
			{
				//이전 코드
				//CheckBakable();

				//Calculate를 하자
				_psdLoader.Step2_Calculate_Secondary(
					
					//삭제 v1.4.2
					//_selectedPSDSecondary._dstFilePath,
					//_selectedPSDSecondary._dstFilePath_Relative,

					 GetBakeIntSize(_selectedPSDSecondary._linkedMainSet._bakeOption_Width),
					 GetBakeIntSize(_selectedPSDSecondary._linkedMainSet._bakeOption_Height),
					//_selectedPSDSet._bakeOption_MaximumNumAtlas, 
					_selectedPSDSecondary._linkedMainSet._bakeOption_Padding,
					_selectedPSDSecondary._linkedMainSet._bakeOption_BlurOption,
					//_selectedPSDSecondary._linkedMainSet._prev_bakeScale100,
					_selectedPSDSecondary._next_bakeScale100,
					_selectedPSDSecondary._backgroundColor,
					OnCalculateResult
					);
			}
			
			GUIStyle guiStyle_Result = new GUIStyle(GUI.skin.box);
			guiStyle_Result.alignment = TextAnchor.MiddleLeft;
			guiStyle_Result.normal.textColor = apEditorUtil.BoxTextColor;

			_isBakeWarning = false;

			//오류 메시지는 별도로 체크
			if(string.IsNullOrEmpty(_selectedPSDSecondary._dstFilePath))
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				//Warning
				GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Warning) + "\n[Save Path] is Empty", guiStyle_WarningBox, GUILayout.Width(width_3), GUILayout.Height(70));

				GUI.backgroundColor = prevColor;
			}
			else if (_isBakeWarning)
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				//Warning
				GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Warning) + "\n" + _bakeWarningMsg, guiStyle_WarningBox, GUILayout.Width(width_3), GUILayout.Height(70));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Bake), GUILayout.Width(width_3), GUILayout.Height(40)))//Bake
				{
					_selectedPSDBakeData = null;//<< 선택된게 해제되어야 한다.

					if (IsGUIUsable)
					{
						//이전 코드
						//StartBake();

						_psdLoader.Step3_Bake_Secondary(_loadKey_Bake, OnBakeResult, _loadKey_Calculated);
					}
				}
				if (_loadKey_Calculated != _loadKey_Bake)
				{
					GUILayout.Space(10);
					GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
					guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
					guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;

					GUI.backgroundColor = new Color(0.6f, 0.6f, 1.0f, 1.0f);

					GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_SettingsAreChanged) + " ]"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedScale) + " : " + _psdLoader.CalculatedResizeX100 + " %"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedAtlas) + " : " + _psdLoader.CalculatedAtlasCount,
									guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));


					
					GUI.backgroundColor = prevColor;

				}


			}
			GUILayout.Space(10);
			if (_loadKey_Bake != null)
			{
				//Bake가 되었다면 => 그 정보를 넣어주자
				GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_BakeResult) + " ]"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ScalePercent) + " : " + _psdLoader.BakedResizeX100 + " %"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_Atlas) + " : " + _psdLoader.BakedAtlasCount,
								guiStyle_Result, GUILayout.Width(width_3), GUILayout.Height(60));
				
			}


			GUILayout.Space(20);

			if (IsProcessRunning)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();

				Rect barRect = new Rect(lastRect.x + 5, lastRect.y + 30, width_3 - 5, 20);

				//이전 : PSD Loader 사용 전
				//float barRatio = Mathf.Clamp01((float)_workProcess.ProcessX100 / 100.0f);
				//EditorGUI.ProgressBar(barRect, barRatio, _threadProcessName);

				float barRatio = _psdLoader.GetImageBakingRatio();
				EditorGUI.ProgressBar(barRect, barRatio, _psdLoader.GetProcessLabel());

			}


			EditorGUILayout.EndVertical();
			//--------------------------------------


			EditorGUILayout.EndHorizontal();
		}

	}
}