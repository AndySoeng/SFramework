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

using AnyPortrait;

namespace AnyPortrait
{

	public class apDialog_SelectTextureAsset : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		public delegate void FUNC_SELECT_TEXTUREASSET_RESULT(bool isSuccess, apTextureData targetTextureData, object loadKey, Texture2D resultTexture2D);

		private static apDialog_SelectTextureAsset s_window = null;

		private apEditor _editor = null;
		private apTextureData _targetTextureData = null;
		private object _loadKey = null;
		private FUNC_SELECT_TEXTUREASSET_RESULT _funcResult = null;

		private class TextureAssetData
		{
			public Texture2D _texture = null;
			public string _pathInUnity = "";

			public TextureAssetData(Texture2D texture, string pathInUnity)
			{
				_texture = texture;
				_pathInUnity = pathInUnity;
			}
		}


		private List<TextureAssetData> _texture2Ds_All = new List<TextureAssetData>();
		private List<TextureAssetData> _texture2Ds_Searched = new List<TextureAssetData>();//메인 리스트에 노출되는 리스트
		private List<Texture2D> _texture2DKeys = new List<Texture2D>();

		private Vector2 _scrollList = new Vector2();
		private TextureAssetData _curSelectedTexture2D = null;
		
		//private string _curSelectedPath = "";

		private bool _isSearched = false;
		private string _strSearchKeyword = "";

		//추가 20.1.22 : 한번에 텍스쳐를 찾는건 오래 걸리니 단계를 두고 로딩을 하자
		private enum LOADING_STEP
		{
			Step1_Loading,//준비 단계
			Step2_Complete//완료!
		}

		private LOADING_STEP _loadingStep = LOADING_STEP.Step1_Loading;
		private bool _isLoadCompleted = false;

		//Asset 내부의 폴더 정보
		private class DirectoryPathData
		{
			public DirectoryInfo _dirInfo;
			public string _pathInUnity = null;//유니티용 경로 (상대 경로)

			public DirectoryPathData(DirectoryInfo dirInfo, string pathInUnity)
			{
				_dirInfo = dirInfo;
				_pathInUnity = pathInUnity;
			}
		}
		private List<DirectoryPathData> _allAssetFolders = new List<DirectoryPathData>();

		private int _iLoadingFolder = 0;
		private int _nLoadingFolders = 0;
		private int _nLoadPerStep = 0;
		private System.Diagnostics.Stopwatch _updateTimer = new System.Diagnostics.Stopwatch();

		private List<string> _imageFileSearchPatterns = new List<string>();
		private string _assetRootPath = "";
		private GUIStyle _guiStyle_Box = null;
		private GUIStyle _guiStyle_Img = null;
		private GUIStyle _guiStyle_label = null;


		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apTextureData targetTextureData, FUNC_SELECT_TEXTUREASSET_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectTextureAsset), true, "Select Texture2D", true);
			apDialog_SelectTextureAsset curTool = curWindow as apDialog_SelectTextureAsset;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 630;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, targetTextureData, loadKey, funcResult);

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
		//------------------------------------------------------------------
		public void Init(apEditor editor, apTextureData targetTextureData, object loadKey, FUNC_SELECT_TEXTUREASSET_RESULT funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetTextureData = targetTextureData;

			_funcResult = funcResult;

			_curSelectedTexture2D = null;

			_isSearched = false;
			_strSearchKeyword = "";

			if(_texture2Ds_All == null)
			{
				_texture2Ds_All = new List<TextureAssetData>();
			}
			_texture2Ds_All.Clear();

			if(_texture2Ds_Searched == null)
			{
				_texture2Ds_Searched = new List<TextureAssetData>();
			}
			_texture2Ds_Searched.Clear();


			if(_texture2DKeys == null)
			{
				_texture2DKeys = new List<Texture2D>();
			}
			_texture2DKeys.Clear();

			_loadingStep = LOADING_STEP.Step1_Loading;
			_isLoadCompleted = false;

			if(_allAssetFolders == null)
			{
				_allAssetFolders = new List<DirectoryPathData>();
			}
			_allAssetFolders.Clear();


			if(_imageFileSearchPatterns == null)
			{
				_imageFileSearchPatterns = new List<string>();
			}
			_imageFileSearchPatterns.Clear();
			//이미지 파일 검색 패턴글자를 모두 저장하자.
			_imageFileSearchPatterns.Add("*.bmp");
			_imageFileSearchPatterns.Add("*.BMP");
			_imageFileSearchPatterns.Add("*.exr");
			_imageFileSearchPatterns.Add("*.EXR");
			_imageFileSearchPatterns.Add("*.gif");
			_imageFileSearchPatterns.Add("*.GIF");
			_imageFileSearchPatterns.Add("*.HDR");
			_imageFileSearchPatterns.Add("*.hdr");
			_imageFileSearchPatterns.Add("*.iff");
			_imageFileSearchPatterns.Add("*.IFF");
			_imageFileSearchPatterns.Add("*.jpg");
			_imageFileSearchPatterns.Add("*.JPG");
			_imageFileSearchPatterns.Add("*.jpeg");
			_imageFileSearchPatterns.Add("*.JPEG");
			_imageFileSearchPatterns.Add("*.pict");
			_imageFileSearchPatterns.Add("*.PICT");
			_imageFileSearchPatterns.Add("*.png");
			_imageFileSearchPatterns.Add("*.PNG");
			_imageFileSearchPatterns.Add("*.psd");
			_imageFileSearchPatterns.Add("*.PSD");
			_imageFileSearchPatterns.Add("*.tga");
			_imageFileSearchPatterns.Add("*.TGA");

			//경로를 모두 모으자
			try
			{
				string strAssetPath = Application.dataPath;
				DirectoryInfo di_Root = new DirectoryInfo(strAssetPath);
				if(!di_Root.Exists)
				{
					Debug.LogError("Invalid Path : " + di_Root.FullName);
					RefreshTextureAssetsOnce();
					return;
				}

				_assetRootPath = di_Root.FullName.Replace("\\", "/");//이게 중요.

				//Debug.Log(">> Asset Path : " + di_Root.FullName);
				
				//_allAssetFolders.Add(di_Root.FullName);
				//_allAssetFolders.Add("Assets");
				_allAssetFolders.Add(new DirectoryPathData(di_Root, "Assets"));

				DirectoryInfo[] childDirInfos = di_Root.GetDirectories("*", SearchOption.AllDirectories);
				if(childDirInfos != null && childDirInfos.Length > 0)
				{
					for (int i = 0; i < childDirInfos.Length; i++)
					{
						DirectoryInfo curDirInfo = childDirInfos[i];
						if(curDirInfo.Exists)
						{
							//Debug.Log(">> [" + i + "] : " + curDirInfo.FullName);
							//_allAssetFolders.Add(curDirInfo.FullName);
							//_allAssetFolders.Add(GetAssetRelativePath(strAbsAssetPath, curDirInfo));
							_allAssetFolders.Add(new DirectoryPathData(curDirInfo, GetAssetRelativePath(curDirInfo)));
						}
					}
				}

				//for (int i = 0; i < _allAssetFolders.Count; i++)
				//{
				//	Debug.Log("[" + i + "] : " + _allAssetFolders[i]);
				//}

				_iLoadingFolder = 0;
				//_nLoadingFolders = childDirInfos.Length;//마지막 폴더를 놓친다..
				_nLoadingFolders = _allAssetFolders.Count;//버그 잡음
				//한번에 몇개씩 업데이트를 해야할까
				//최대 20번의 스탭을 두자
				if(_nLoadingFolders > 20)
				{
					_nLoadPerStep = _nLoadingFolders / 20;
					_nLoadPerStep += 1;
					if(_nLoadPerStep < 2)
					{
						_nLoadPerStep = 2;
					}
				}
				else
				{
					_nLoadPerStep = 1;
				}
				

				_updateTimer.Reset();
				_updateTimer.Stop();
				_updateTimer.Start();
			}
			catch(Exception ex)
			{
				Debug.LogError("Exception : " + ex);
				RefreshTextureAssetsOnce();
				return;
			}
		}

		private string GetAssetRelativePath(DirectoryInfo targetDirInfo)
		{
			string strTargetPath = targetDirInfo.FullName.Replace("\\", "/");
			return "Assets" + strTargetPath.Replace(_assetRootPath, "");
		}

		private string GetAssetRelativePath(FileInfo targetFileInfo)
		{
			string strTargetPath = targetFileInfo.FullName.Replace("\\", "/");
			return "Assets" + strTargetPath.Replace(_assetRootPath, "");
		}



		private void RefreshTextureAssetsOnce()
		{
			Debug.LogError("RefreshTextureAssetsOnce");

			_texture2Ds_All.Clear();
			_texture2Ds_Searched.Clear();
			_texture2DKeys.Clear();
			//TODO : 너무 많은 시간이 걸리므로, 폴더를 분할해서 로딩바와 함께 천천히 검색하자>>>
			string[] guids = AssetDatabase.FindAssets("t:Texture2D");
			
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

				Texture2D textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
				if (textureAsset != null)
				{
					if (textureAsset.width <= 64 || textureAsset.height <= 64)
					{
						//너무 작은건 패스한다.
						continue;
					}

					//if (_isSearched)
					//{
					//	if (!textureAsset.name.Contains(_strSearchKeyword))
					//	{
					//		//검색이 되지 않는다면 패스
					//		continue;
					//	}
					//}

					_texture2Ds_All.Add(new TextureAssetData(textureAsset, assetPath));
					_texture2DKeys.Add(textureAsset);
				}
			}
			if (_curSelectedTexture2D != null)
			{
				if (!_texture2Ds_All.Contains(_curSelectedTexture2D))
				{
					_curSelectedTexture2D = null;
				}
			}
			RefreshMainList();
			_loadingStep = LOADING_STEP.Step2_Complete;//한번에 로딩이 끝났다.
		}

		private void RefreshMainList()
		{
			_texture2Ds_Searched.Clear();
			int nTextures = _texture2Ds_All.Count;

			TextureAssetData curData = null;

			if (_isSearched)
			{
				for (int i = 0; i < nTextures; i++)
				{
					curData = _texture2Ds_All[i];
					if(curData._texture.name.Contains(_strSearchKeyword))
					{
						//검색 키워드가 있는 경우에만
						_texture2Ds_Searched.Add(curData);
					}
				}
			}
			else
			{
				for (int i = 0; i < nTextures; i++)
				{
					curData = _texture2Ds_All[i];
					_texture2Ds_Searched.Add(curData);
				}
			}
			

		}


		// Update
		//------------------------------------------------------------------
		void Update()
		{
			if(_loadingStep != LOADING_STEP.Step1_Loading && _isLoadCompleted)
			{
				return;
			}

			if(_updateTimer.ElapsedMilliseconds > 10)//0.01초마다
			{
				_updateTimer.Reset();
				_updateTimer.Stop();
				_updateTimer.Start();

				Repaint();
			}
		}
		// GUI
		//------------------------------------------------------------------

		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}

			if (_guiStyle_Box == null)
			{
				_guiStyle_Box = new GUIStyle(GUI.skin.box);
				_guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				_guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;
			}

			if (_guiStyle_Img == null)
			{
				_guiStyle_Img = new GUIStyle(GUI.skin.box);
				_guiStyle_Img.alignment = TextAnchor.MiddleCenter;
			}

			
			if (_guiStyle_label == null)
			{
				_guiStyle_label = new GUIStyle(GUI.skin.label);
				_guiStyle_label.alignment = TextAnchor.UpperCenter;
				_guiStyle_label.wordWrap = true;

			}
			


			if(_loadingStep == LOADING_STEP.Step1_Loading)
			{
				DrawGUI_Loading(width, height);
			}
			else
			{
				DrawGUI_Loaded(width, height);
			}
		}
		private void DrawGUI_Loading(int width, int height)
		{
			//로딩바를 넣자
			int barWidth = (int)(width * 0.8f);
			int barHeight = 25;
			int posX = (width - barWidth) / 2 + 5;
			int posY = (height / 2) - (barHeight + 20);

			float loadingRatio = 0.0f;
			string loadingText = "";

			switch (_loadingStep)
			{
				case LOADING_STEP.Step1_Loading:
					{
						if(_nLoadingFolders > 0)
						{
							loadingRatio = (float)_iLoadingFolder / (float)_nLoadingFolders;
						}
						else
						{
							loadingRatio = 0.0f;
						}
						loadingText = "Finding texture assets.. (" + (Mathf.Clamp((int)(loadingRatio * 100.0f), 0, 100)) + "%)";
					}
					
					break;

				case LOADING_STEP.Step2_Complete:
					{
						loadingRatio = 1.0f;
						loadingText = "Wait..";
					}
					break;
			}


			EditorGUI.ProgressBar(new Rect(posX, posY, barWidth, barHeight), loadingRatio, loadingText);

			//로딩 처리를 하자
			try
			{
				LoadTexturePerStep();
			}
			catch(Exception ex)
			{
				Debug.LogError("Loading Exception : " + ex);
			}
			//Repaint();//<<바로 화면 갱신
		}



		private void LoadTexturePerStep()
		{
			if(Event.current.type != EventType.Repaint)
			{
				return;
			}
			//한번만 하는게 아니라 몇번 반복한다.
			if(_nLoadPerStep < 2)
			{
				_nLoadPerStep = 2;
			}
			for (int iLoop = 0; iLoop < _nLoadPerStep; iLoop++)
			{
				if (_iLoadingFolder >= _nLoadingFolders)
				{
					_loadingStep = LOADING_STEP.Step2_Complete;
					return;
				}



				DirectoryPathData dirPathData = _allAssetFolders[_iLoadingFolder];


				//직접 해당 폴더에 있는 파일들을 검색한다.
				//확장자 검색을 해야하는데, 이미지 포맷 확장자가 여러개라서 여러번 체크해야한다.
				int nPatterns = _imageFileSearchPatterns.Count;

				for (int iPattern = 0; iPattern < nPatterns; iPattern++)
				{
					FileInfo[] fileInfos = dirPathData._dirInfo.GetFiles(_imageFileSearchPatterns[iPattern], SearchOption.TopDirectoryOnly);
					if (fileInfos == null || fileInfos.Length == 0)
					{
						continue;
					}

					FileInfo curFileInfo = null;
					for (int iFile = 0; iFile < fileInfos.Length; iFile++)
					{
						curFileInfo = fileInfos[iFile];
						//경로를 변경한다.
						string pathInUnity = GetAssetRelativePath(curFileInfo);

						//경로에서 에셋을 로드해본다.
						Texture2D textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(pathInUnity);



						if (textureAsset != null)
						{
							//Debug.Log("Check Image Asset [" + pathInUnity + "]");

							if (textureAsset.width <= 64 || textureAsset.height <= 64)
							{
								//너무 작은건 패스한다.
								continue;
							}

							//if (_isSearched)
							//{
							//	if (!textureAsset.name.Contains(_strSearchKeyword))
							//	{
							//		//검색이 되지 않는다면 패스
							//		continue;
							//	}
							//}

							if (!_texture2DKeys.Contains(textureAsset))
							{
								_texture2DKeys.Add(textureAsset);//<중복을 막기 위한 작업
								_texture2Ds_All.Add(new TextureAssetData(textureAsset, pathInUnity));
							}
						}
						else
						{
							//Debug.LogError("Check Image Asset [" + pathInUnity + "] >> Failed");
						}
					}
				}

				#region [이전 방식] FindAsset을 이용한다.
				////string strPath = _allAssetFolders[_iLoadingFolder];

				////string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { strPath });
				//string[] guids = AssetDatabase.FindAssets("", new string[] { strPath });

				//for (int i = 0; i < guids.Length; i++)
				//{
				//	string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

				//	Texture2D textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
				//	if (textureAsset != null)
				//	{
				//		if (textureAsset.width <= 64 || textureAsset.height <= 64)
				//		{
				//			//너무 작은건 패스한다.
				//			continue;
				//		}

				//		if (_isSearched)
				//		{
				//			if (!textureAsset.name.Contains(_strSearchKeyword))
				//			{
				//				//검색이 되지 않는다면 패스
				//				continue;
				//			}
				//		}

				//		if(!_texture2Ds.Contains(textureAsset))
				//		{
				//			_texture2Ds.Add(textureAsset);
				//		}
				//	}
				//} 
				#endregion


				_iLoadingFolder++;
				if (_iLoadingFolder >= _nLoadingFolders)
				{
					//모두 로딩했다면
					if (_curSelectedTexture2D != null)
					{
						if (!_texture2Ds_All.Contains(_curSelectedTexture2D))
						{
							_curSelectedTexture2D = null;
						}
					}

					_texture2Ds_All.Sort(delegate (TextureAssetData a, TextureAssetData b)
					{
						return string.Compare(a._texture.name, b._texture.name);
					});

					RefreshMainList();//실제 노출될 리스트를 갱신한다.
					_loadingStep = LOADING_STEP.Step2_Complete;
					Repaint();
				}
			}
			
		}



		private void DrawGUI_Loaded(int width, int height)
		{
			int preferImageWidth = 120;
			int scrollWidth = width - 20;
			int nImagePerRow = (scrollWidth / preferImageWidth);
			if (nImagePerRow < 1)
			{
				nImagePerRow = 1;
			}
			int imageUnitWidth = (scrollWidth / nImagePerRow) - 14;

			int mainListHeight = height - (90 + 10 + 60 + 10);


			_isLoadCompleted = true;//로딩 완료

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			//GUI.Box(new Rect(0, 40, width, mainListHeight), "");
			GUI.Box(new Rect(0, 39, width, mainListHeight + 2), "");//범위를 1씩 늘렸다.
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			guiStyle.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			//EditorGUILayout.LabelField("Select Texture", guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(15));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Search), GUILayout.Width(80));//"Search"
			string prevSearchKeyword = _strSearchKeyword;
			//_strSearchKeyword = EditorGUILayout.DelayedTextField(_strSearchKeyword, GUILayout.Width(width - (100 + 110)), GUILayout.Height(15));
			_strSearchKeyword = EditorGUILayout.DelayedTextField(_strSearchKeyword, GUILayout.Width(width - (100)), GUILayout.Height(15));

			//변경 20.1.22 : 갱신 버튼 제거
			//if (GUILayout.Button(_editor.GetText(TEXT.DLG_Refresh), GUILayout.Width(100), GUILayout.Height(15)))//"Refresh"
			//{
			//	RefreshTextureAssetsOnce();
			//}

			if (prevSearchKeyword != _strSearchKeyword)
			{
				if (string.IsNullOrEmpty(_strSearchKeyword))
				{
					_isSearched = false;
				}
				else
				{
					_isSearched = true;
				}

				//RefreshTextureAssetsOnce();//이전
				RefreshMainList();//실제 노출될 리스트를 갱신한다. < 변경
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);

			int nTextures = _texture2Ds_Searched != null ? _texture2Ds_Searched.Count : 0;
			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(mainListHeight));

			GUILayout.Space(10);

			EditorGUILayout.LabelField(" " + _editor.GetText(TEXT.TextureAssets), GUILayout.Width(width - 30));

			GUILayout.Space(10);

			//int imageUnitHeight = 200;
			int imageUnitHeight = imageUnitWidth + 45;

			Rect lastRect = GUILayoutUtility.GetLastRect();
			int posY = (int)lastRect.y;

			//int scrollWidth = width - 16;
			//int imageUnitWidth = (scrollWidth / 3) - 12;
			for (int iRow = 0; iRow < nTextures; iRow += nImagePerRow)
			{
				int posX = 0;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(scrollWidth), GUILayout.Height(imageUnitHeight + 8));
				for (int iCol = 0; iCol < nImagePerRow; iCol++)
				{
					int iTex = iRow + iCol;
					if (iTex >= nTextures)
					{
						break;
					}

					GUILayout.Space(5);

					posX += 5;

					EditorGUILayout.BeginVertical(GUILayout.Width(imageUnitWidth), GUILayout.Height(imageUnitHeight));
					DrawTextureUnit(_texture2Ds_Searched[iTex], posX, posY, imageUnitWidth - 2, imageUnitHeight - 2);
					EditorGUILayout.EndVertical();

					posX += imageUnitWidth;

					if (iCol < nImagePerRow - 1)
					{
						GUILayout.Space(2);
						posX += 2;
					}

					posX += 6;
					
					
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(20);

				posY += imageUnitHeight + 8 + 20;
			}

			GUILayout.Space(height - 90);
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			//추가 : 선택된 텍스쳐의 정보를 표기하자

			//이전
			//if(_curSelectedTexture2D != null)
			//{
			//	GUILayout.Space(5);
			//	EditorGUILayout.LabelField("  " + _curSelectedTexture2D._texture.name);
			//	EditorGUILayout.LabelField("  " + _curSelectedTexture2D._pathInUnity);
			//	EditorGUILayout.LabelField("  " + _curSelectedTexture2D._texture.width + " x " + _curSelectedTexture2D._texture.height);
			//	GUILayout.Space(10);
			//}
			//else
			//{
			//	GUILayout.Space(69);
			//}

			//변경 22.1.8 : 여백 계산
			if(_curSelectedTexture2D != null)
			{
				GUILayout.Space(5);
				EditorGUILayout.LabelField("  " + _curSelectedTexture2D._texture.name, GUILayout.Height(20));
				EditorGUILayout.LabelField("  " + _curSelectedTexture2D._pathInUnity, GUILayout.Height(20));
				EditorGUILayout.LabelField("  " + _curSelectedTexture2D._texture.width + " x " + _curSelectedTexture2D._texture.height, GUILayout.Height(20));
				GUILayout.Space(5);
			}
			else
			{
				GUILayout.Space(5);
				EditorGUILayout.LabelField("  ", GUILayout.Height(20));
				EditorGUILayout.LabelField("  ", GUILayout.Height(20));
				EditorGUILayout.LabelField("  ", GUILayout.Height(20));
				GUILayout.Space(5);
			}

			EditorGUILayout.BeginHorizontal();
			bool isClose = false;
			//string strSelectBtn = "Set Texture";
			string strSelectBtn = _editor.GetText(TEXT.DLG_SetTexture);
			if (_curSelectedTexture2D != null)
			{
				if (_curSelectedTexture2D._texture.name.Length < 20)
				{
					//strSelectBtn = "Set [" + _curSelectedTexture2D.name + "]";
					strSelectBtn = string.Format("{0}\n[{1}]", _editor.GetText(TEXT.DLG_Select), _curSelectedTexture2D._texture.name);
				}
				else
				{
					//strSelectBtn = "Set [" + _curSelectedTexture2D.name.Substring(0, 15) + "..]";
					strSelectBtn = string.Format("{0}\n[{1}]", _editor.GetText(TEXT.DLG_Select), _curSelectedTexture2D._texture.name.Substring(0, 20) + "..");
				}

			}
			if (GUILayout.Button(strSelectBtn, GUILayout.Height(40), GUILayout.Width(width / 2 - 6)))
			{
				_funcResult(true, _targetTextureData, _loadKey, _curSelectedTexture2D._texture);
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(40), GUILayout.Width(width / 2 - 6)))//"Close"
			{
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}


		private void DrawTextureUnit(TextureAssetData textureAssetData, int posX, int posY, int width, int height)
		{
			Color prevColor = GUI.backgroundColor;

			int btnSize = width;
			
			bool isSelected = _curSelectedTexture2D == textureAssetData 
								&& textureAssetData != null
								&& textureAssetData._texture != null;

			if(isSelected)
			{
				//선택을 했다면, 뒤에 외곽선을 그리자
				if (EditorGUIUtility.isProSkin)
				{
					//Pro에서는 밝은 색상의 외곽선
					GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
				}
				else
				{
					//일반에서는 어두운 색상의 외곽선
					GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
				}
				GUI.Box(new Rect(posX, posY + 8, width + 8, width + 8), "", apEditorUtil.WhiteGUIStyle);
				GUI.backgroundColor = prevColor;
			}

			//float baseAspectRatio = (float)width / (float)imageSlotHeight;

			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			if (textureAssetData == null || textureAssetData._texture == null)
			{
				//_guiStyle_Box = new GUIStyle(GUI.skin.box);
				//_guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				//_guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box("Empty Image", _guiStyle_Box, GUILayout.Width(width), GUILayout.Height(height));
			}
			else
			{	
				if (isSelected)
				{
					Color boxColor = prevColor;
					if (EditorGUIUtility.isProSkin)
					{
						//변경 22.7.2 : Pro Skin인 경우엔 색을 더 다르게 하자
						boxColor.r = prevColor.r * 0.1f;
						boxColor.g = prevColor.g * 1.0f;
						boxColor.b = prevColor.b * 1.5f;
					}
					else
					{
						boxColor.r = prevColor.r * 0.8f;
						boxColor.g = prevColor.g * 0.8f;
						boxColor.b = prevColor.b * 1.2f;
					}

					GUI.backgroundColor = boxColor;

					isSelected = true;
				}
				
				if (GUILayout.Button(	textureAssetData._texture, 
										_guiStyle_Img, 
										GUILayout.Width(width), GUILayout.Height(btnSize)))
				{
					_curSelectedTexture2D = textureAssetData;
					EditorGUIUtility.PingObject(_curSelectedTexture2D._texture);
				}
				

				GUI.backgroundColor = prevColor;
			}
			

			//_guiStyle_label = new GUIStyle(GUI.skin.label);
			//_guiStyle_label.alignment = TextAnchor.MiddleCenter;

			string textureName = textureAssetData == null ? "<Empty>" : textureAssetData._texture.name;
			if(textureName.Length * 7 > width * 3)
			{
				//길이가 너무 길다면
				int maxLength = (int)((float)(width * 3) / 7.0f);
				if(maxLength < 10)
				{
					maxLength = 10;
				}
				if(textureName.Length > maxLength)
				{
					textureName = textureName.Substring(0, maxLength);
				}
			}
			EditorGUILayout.LabelField(textureName, _guiStyle_label, GUILayout.Width(width), GUILayout.Height(42));
			EditorGUILayout.EndVertical();
			//if(apEditorUtil.ToggledButton(textureData._name, textureData == _curSelectedTextureData, width, btnHeight))
			//{
			//	_curSelectedTextureData = textureData;
			//}
		}
		// 
		//------------------------------------------------------------------
	}


}