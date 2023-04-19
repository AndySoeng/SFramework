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
	/// MaterialLibray 다이얼로그
	/// </summary>
	public class apDialog_MaterialLibrary : EditorWindow
	{
		// Members
		//----------------------------------------------------------------------------
		private static apDialog_MaterialLibrary s_window = null;
		
		private apEditor _editor = null;
		private apPortrait _portrait = null;

		private Vector2 _scroll_LeftUpper = Vector2.zero;
		private Vector2 _scroll_LeftLower = Vector2.zero;
		private Vector2 _scroll_RightUpper = Vector2.zero;

		private apMaterialSet _selectedMaterialSet = null;
		private bool _isPreset = false;


		private Texture2D _img_FoldDown = null;
		private Texture2D _img_MaterialSet = null;
		private Texture2D _img_BasicSettings = null;
		private Texture2D _img_Shaders = null;
		private Texture2D _img_ShaderProperties = null;
		private Texture2D _img_Reserved = null;
		private Texture2D _img_LWRP = null;
		private Texture2D _img_VR = null;
		private Texture2D _img_URP = null;//<<추가 20.1.24
		private Texture2D _img_Merge = null;//<<추가 22.1.5

		private Dictionary<apMaterialSet.ICON, Texture2D> _img_MatTypeIcons = new Dictionary<apMaterialSet.ICON, Texture2D>();
		
		
		private bool _isInitGUIStyle = false;
		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_Selected = null;

		private object _loadKey_MakeNewMaterialSet = null;
		private object _loadKey_LinkPreset = null;



		//복제/삭제 요청
		private apMaterialSet _duplicateMatSet = null;
		private apMaterialSet _removeMatSet = null;

		private enum SHADER_TYPE
		{
			None,
			Default,
			LWRP,
			VR,
			URP,
			Mergeable,//추가 22.1.5
			//TODO..
		}

		private Dictionary<Shader, SHADER_TYPE> _shaderTypes = new Dictionary<Shader, SHADER_TYPE>();

		private string[] _packageNames = new string[] 
		{
			"Advanced Presets",//0
			"LWRP Unlit Preset",//1
			"LWRP 2D Lit Preset (Experimental)",//2
			"VR Presets",//3
			"KeepAlpha Presets",//4
			"Mergeable Presets",//5 : <추가 22.1.5
			"URP Presets",//<<6 : 추가 20.1.24
			"URP (2021) Presets",//7 : 추가 21.12.20
		};

		private int _packageIndex = 0;

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static void ShowDialog(apEditor editor, apPortrait portrait)
		{
			try
			{
				CloseDialog();

				if (editor == null || portrait == null)
				{
					return;
				}

				EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_MaterialLibrary), true, "Material Library", true);
				apDialog_MaterialLibrary curTool = curWindow as apDialog_MaterialLibrary;

				if (curTool != null && curTool != s_window)
				{
					int width = 900;
					int height = 700;
					s_window = curTool;
					s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
													(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
													width, height);
					s_window.Init(editor, portrait);
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("Material Library ShowDialog Exception : " + ex);
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

		private static void OnUndoRedoPerformed()
		{
			if(s_window == null)
			{
				//Debug.LogError("없어진 재질 다이얼로그창. UndoRedo 해제");
				Undo.undoRedoPerformed -= OnUndoRedoPerformed;
				return;
			}

			s_window.OnUndo();


		}

		// Init
		//------------------------------------------------------------------------
		public void Init(apEditor editor, apPortrait portrait)
		{
			_editor = editor;
			_portrait = portrait;
			_selectedMaterialSet = null;
			_isPreset = false;

			//이 다이얼로그를 열 때, Link를 한번 더 해주자.
			_editor.Controller.LinkMaterialSets(_portrait);

			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			_img_MaterialSet = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet);
			if(_img_MatTypeIcons != null)
			{
				_img_MatTypeIcons = new Dictionary<apMaterialSet.ICON, Texture2D>();
			}
			_img_MatTypeIcons.Clear();

			_img_MatTypeIcons.Add(apMaterialSet.ICON.Unlit, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Unlit));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.Lit, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Lit));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.LitSpecular, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitSpecular));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.LitSpecularEmission, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitSpecularEmission));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.LitRimlight, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitRim));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.LitRamp, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitRamp));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.Effect, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_FX));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.Cartoon, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Cartoon));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.Custom1, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom1));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.Custom2, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom2));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.Custom3, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom3));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.UnlitVR, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_UnlitVR));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.LitVR, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitVR));
			//추가 22.1.5:
			_img_MatTypeIcons.Add(apMaterialSet.ICON.UnlitMergeable, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_MergeableUnlit));
			_img_MatTypeIcons.Add(apMaterialSet.ICON.LitMergeable, _editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_MergeableLit));
		
			_img_BasicSettings = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_BasicSettings);
			_img_Shaders = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_Shaders);
			_img_ShaderProperties = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_ShaderProperties);
			_img_Reserved = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_Reserved);
			_img_LWRP = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_LWRP);
			_img_VR = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_VR);
			_img_URP = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_URP);//20.1.24
			_img_Merge = _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_Mergeable);//22.1.5


			_isInitGUIStyle = false;

			//기본 MaterialSet을 선택하자.
			if(_portrait._materialSets.Count > 0)
			{
				apMaterialSet matSet = null;
				//Default가 True인 Materisl Set을 찾자
				for (int i = 0; i < _portrait._materialSets.Count; i++)
				{
					matSet = _portrait._materialSets[i];
					if(matSet._isDefault)
					{
						_selectedMaterialSet = matSet;
						_isPreset = false;
						break;
					}
				}

				if(_selectedMaterialSet == null)
				{
					//Default가 없다면
					_selectedMaterialSet = _portrait._materialSets[0];
					_isPreset = false;
				}

			}
			if(_selectedMaterialSet == null)
			{
				//그게 아니라면 기본 Editor의 첫번째걸로 선택
				if(_editor.MaterialLibrary.Presets.Count > 0)
				{
					_selectedMaterialSet = _editor.MaterialLibrary.Presets[0];
					_isPreset = true;//<<프리셋이다.
				}
			}


			_loadKey_MakeNewMaterialSet = null;
			_loadKey_LinkPreset = null;

			if (_shaderTypes == null)
			{
				_shaderTypes = new Dictionary<Shader, SHADER_TYPE>();
			}
			_shaderTypes.Clear();


			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		public void OnUndo()
		{
			apEditorUtil.ReleaseGUIFocus();
			Repaint();
		}

		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			try
			{

				int width = (int)position.width;
				int height = (int)position.height;
				if (_editor == null || _portrait == null)
				{
					CloseDialog();
					return;
				}

				//레이아웃
				//---------------------------------
				//|      |                         |
				//|라이브|      재질 설정          |
				//|      |                         |
				//|      |                         |
				//|------|                         |
				//|      |                         |
				//|프리셋|-------------------------|
				//|      | 복사, 프리셋 저장, 삭제 |
				//|      |                         |
				//---------------------------------
				// 닫기
				//---------------------------------

				
				int bottomHeight = 45;
				int mainHeight = height - bottomHeight;

				int leftWidth = 250;
				int rightWidth = width - (leftWidth + 23) + 13;

				int leftHeight_Upper = (int)(mainHeight * 0.3f);
				int leftHeight_Lower = mainHeight - (leftHeight_Upper + 140 + (22 * 2));

				int rightHeight_Lower = 90;
				int rightHeight_Upper = mainHeight - (rightHeight_Lower + 15);


				bool isClose = false;

				//복제/삭제 요청 초기화
				_duplicateMatSet = null;
				_removeMatSet = null;
				

				Color prevColor = GUI.backgroundColor;

				GUI.Box(new Rect(5, 23, leftWidth, leftHeight_Upper), "");
				GUI.Box(new Rect(5, 23 + leftHeight_Upper + 76, leftWidth, leftHeight_Lower), "");

				//GUI.Box(new Rect(5 + leftWidth + 5, 5, rightWidth, rightHeight_Upper), "");
				

				//GUI Style을 초기화하자.
				if (!_isInitGUIStyle)
				{
					_isInitGUIStyle = true;

					_guiStyle_None = new GUIStyle(GUIStyle.none);
					_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

					_guiStyle_Selected = new GUIStyle(GUIStyle.none);
					if (EditorGUIUtility.isProSkin)
					{
						_guiStyle_Selected.normal.textColor = Color.cyan;
					}
					else
					{
						_guiStyle_Selected.normal.textColor = Color.white;
					}
				}

				EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
				
				//위쪽
				//------------------------------------------------------------------------------------
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(mainHeight));
				GUILayout.Space(5);

				//---------------------------------------------------
				// Left
				//---------------------------------------------------
				EditorGUILayout.BeginVertical(GUILayout.Width(leftWidth), GUILayout.Height(mainHeight));

				GUILayout.Space(5);

				// Left UI
				

				//Left Upper
				EditorGUILayout.BeginVertical(GUILayout.Width(leftWidth), GUILayout.Height(leftHeight_Upper));
				EditorGUILayout.LabelField(_editor.GetText(TEXT.MaterialSets));//"Material Sets"
				
				// Left Upper UI : 라이브 데이터 리스트
				_scroll_LeftUpper = EditorGUILayout.BeginScrollView(_scroll_LeftUpper, false, true, GUILayout.Width(leftWidth), GUILayout.Height(leftHeight_Upper));

				//EditorGUILayout.BeginVertical(GUILayout.Width(leftWidth - 20));
				EditorGUILayout.BeginVertical();

				// Left Upper GUI
				OnGUI_LeftUpper(leftWidth - 20);

				GUILayout.Space(leftHeight_Upper + 100);
				EditorGUILayout.EndVertical();

				
				

				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();

				//" Make Material Set"
				if(GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.MakeMaterialSet), _img_MaterialSet), GUILayout.Width(leftWidth - 10), GUILayout.Height(30)))
				{
					//다이얼로그를 만들자
					_loadKey_LinkPreset = null;
					//"Select a preset\nto create a Material Set"
					_loadKey_MakeNewMaterialSet = apDialog_SelectMaterialSet.ShowDialog(_editor, true, _editor.GetText(TEXT.SelectMatSetTitle_ToCreate), true, OnSelectMaterialSet_MakeMaterialSet, null);
				}

				GUILayout.Space(20);

				
				//Left Lower
				EditorGUILayout.BeginVertical(GUILayout.Width(leftWidth), GUILayout.Height(leftHeight_Lower));

				//"Material Presets"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.MaterialPresets));

				// Left Lower UI : 프리셋 데이터 리스트
				_scroll_LeftLower = EditorGUILayout.BeginScrollView(_scroll_LeftLower, false, true, GUILayout.Width(leftWidth), GUILayout.Height(leftHeight_Lower));

				//EditorGUILayout.BeginVertical(GUILayout.Width(leftWidth - 20));
				EditorGUILayout.BeginVertical();

				// Left Lower GUI
				OnGUI_LeftLower(leftWidth - 20);

				GUILayout.Space(leftHeight_Lower + 100);
				EditorGUILayout.EndVertical();
				
				EditorGUILayout.EndScrollView();

				EditorGUILayout.EndVertical();

				//" Make Material Preset"
				if(GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.MakeMaterialPreset), _img_MaterialSet), GUILayout.Width(leftWidth - 10), GUILayout.Height(30)))
				{
					//프리셋을 바로 만들자.
					//비어있는 상태로
					apMaterialSet newPreset = _editor.MaterialLibrary.AddNewPreset(null, false, null);
					_editor.MaterialLibrary.Save();

					//추가 22.1.11 : 생성된 프리셋 바로 선택
					if(newPreset != null)
					{
						_selectedMaterialSet = newPreset;
					}
					

					Repaint();
				}

				GUILayout.Space(2);

				//변경 19.8.5 : 패키지 설치하기 UI 변경
				//EditorGUILayout.LabelField("Preset Packages");
				_packageIndex = EditorGUILayout.Popup(_packageIndex, _packageNames, GUILayout.Width(leftWidth - 10));
				if(GUILayout.Button(_editor.GetText(TEXT.UnpackPreset), GUILayout.Width(leftWidth - 10), GUILayout.Height(20)))//"Unpack Preset"
				{
					string packagePath = "";
					string basePath = apPathSetting.I.CurrentPath;

					//기존 : 고정 경로
					//변경 20.4.21 : 가변 경로 가능. 기본 경로는 "Assets/AnyPortrait/"

					switch (_packageIndex)
					{
						case 0://Advanced Presets
							//packagePath = "Assets/AnyPortrait/Editor/Packages/Advanced Shaders.unitypackage";
							packagePath = basePath + "Editor/Packages/Advanced Shaders.unitypackage";
							break;

						case 1://LWRP Unlit Preset
							//packagePath = "Assets/AnyPortrait/Editor/Packages/LWRP Unlit Shaders.unitypackage";
							packagePath = basePath + "Editor/Packages/LWRP Unlit Shaders.unitypackage";
							break;

						case 2://LWRP 2D Lit Preset
							//packagePath = "Assets/AnyPortrait/Editor/Packages/LWRP 2D Lit Shaders.unitypackage";
							packagePath = basePath + "Editor/Packages/LWRP 2D Lit Shaders.unitypackage";
							break;

						case 3://VR Preset
							//packagePath = "Assets/AnyPortrait/Editor/Packages/VR Shaders.unitypackage";
							packagePath = basePath + "Editor/Packages/VR Shaders.unitypackage";
							break;

						case 4://KeepAlpha Preset
							//packagePath = "Assets/AnyPortrait/Editor/Packages/KeepAlpha Shaders.unitypackage";
							packagePath = basePath + "Editor/Packages/KeepAlpha Shaders.unitypackage";
							break;

							//추가 22.1.5
						case 5://Mergeable Presets
							packagePath = basePath + "Editor/Packages/Mergeable Shaders.unitypackage";
							break;

						//추가 20.1.24
						case 6://URP Presets
							//packagePath = "Assets/AnyPortrait/Editor/Packages/URP Shaders.unitypackage";
							packagePath = basePath + "Editor/Packages/URP Shaders.unitypackage";
							break;

						case 7://URP (2021) Presets (추가 21.12.20)
							packagePath = basePath + "Editor/Packages/URP 2021 Shaders.unitypackage";
							break;
					}

					AssetDatabase.ImportPackage(packagePath, false);
					AssetDatabase.Refresh();
					AssetDatabase.SaveAssets();

					switch (_packageIndex)
					{
						case 0://Advanced Presets
							_editor.MaterialLibrary.MakeReserved_Advanced(false);
							break;

						case 1://LWRP Unlit Preset
							_editor.MaterialLibrary.MakeReserved_LWRPUnlit(false);
							break;

						case 2://LWRP 2D Lit Preset
							_editor.MaterialLibrary.MakeReserved_LWRP_2DLit(false);
							break;

						case 3://VR Preset
							_editor.MaterialLibrary.MakeReserved_VR(false);
							break;

						case 4://KeepAlpha Preset
							_editor.MaterialLibrary.MakeReserved_KeepAlpha(false);
							break;

						//추가 22.1.5
						case 5://Mergeable Presets
							_editor.MaterialLibrary.MakeReserved_Mergeable(false);
							break;

						//추가 20.1.24
						case 6://URP Preset
							_editor.MaterialLibrary.MakeReserved_URP(false);
							break;

						//추가 21.12.20
						case 7://URP 21 Preset
							_editor.MaterialLibrary.MakeReserved_URP21(false);
							break;
					}

					_editor.MaterialLibrary.Save();
					Repaint();

					//재시작 요구
					EditorUtility.DisplayDialog(
						_editor.GetText(TEXT.DLG_UnpackMaterialPreset_Title),
						_editor.GetText(TEXT.DLG_UnpackMaterialPreset_Body),
						_editor.GetText(TEXT.Okay)
						);

					isClose = true;
				}

				#region [미사용 코드]
				////"Unpack Advanced Presets"
				//if(GUILayout.Button(_editor.GetText(TEXT.UnpackAdvancedPresets), GUILayout.Width(leftWidth - 10), GUILayout.Height(20)))
				//{
				//	//Advanced Shader 프리셋을 만들자
				//	AssetDatabase.ImportPackage("Assets/AnyPortrait/Editor/Packages/Advanced Shaders.unitypackage", false);
				//	AssetDatabase.Refresh();
				//	AssetDatabase.SaveAssets();

				//	_editor.MaterialLibrary.MakeReserved_Advanced(false);

				//	_editor.MaterialLibrary.Save();
				//	Repaint();

				//	//재시작 요구
				//	//"Restart Dialog", 
				//	//"The Dialog must be restarted to load the newly imported shaders.\nIf the Dialog is closed, please open it again.",
				//	EditorUtility.DisplayDialog(
				//		_editor.GetText(TEXT.DLG_UnpackMaterialPreset_Title), 
				//		_editor.GetText(TEXT.DLG_UnpackMaterialPreset_Body),
				//		_editor.GetText(TEXT.Okay)
				//		);

				//	isClose = true;
				//}

				////"Unpack LWRP Preset"
				//if(GUILayout.Button(_editor.GetText(TEXT.UnpackLWRPPreset), GUILayout.Width(leftWidth - 10), GUILayout.Height(20)))
				//{
				//	//LWRP용 프리셋을 만들자.

				//	AssetDatabase.ImportPackage("Assets/AnyPortrait/Editor/Packages/LWRP Unlit Shaders.unitypackage", false);
				//	AssetDatabase.Refresh();
				//	AssetDatabase.SaveAssets();
				//	bool isResult = _editor.MaterialLibrary.MakeReserved_LWRPUnlit(false);
				//	if (isResult)
				//	{
				//		_editor.MaterialLibrary.Save();
				//		Repaint();

				//		//재시작 요구
				//		//"Restart Dialog", 
				//		//"The Dialog must be restarted to load the newly imported shaders.\nIf the Dialog is closed, please open it again.",
				//		EditorUtility.DisplayDialog(
				//			_editor.GetText(TEXT.DLG_UnpackMaterialPreset_Title), 
				//			_editor.GetText(TEXT.DLG_UnpackMaterialPreset_Body),
				//			_editor.GetText(TEXT.Okay)
				//			);

				//		isClose = true;
				//	}
				//} 
				#endregion

				EditorGUILayout.EndVertical();

				//---------------------------------------------------
				GUILayout.Space(5);

				//---------------------------------------------------
				// Right
				//---------------------------------------------------

				EditorGUILayout.BeginVertical(GUILayout.Width(rightWidth), GUILayout.Height(mainHeight));

				GUILayout.Space(5);

				// Right UI
				

				EditorGUILayout.BeginVertical(GUILayout.Width(rightWidth), GUILayout.Height(rightHeight_Upper));
				// Right Upper UI : 선택된 재질 설정

				_scroll_RightUpper = EditorGUILayout.BeginScrollView(_scroll_RightUpper, false, true, GUILayout.Width(rightWidth), GUILayout.Height(rightHeight_Upper));

				EditorGUILayout.BeginVertical(GUILayout.Width(rightWidth - 30));

				// Right Upper GUI
				OnGUI_RightUpper(rightWidth - 30);

				GUILayout.Space(rightHeight_Upper + 100);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();

				
				GUILayout.Space(5);

				EditorGUILayout.BeginVertical(GUILayout.Width(rightWidth), GUILayout.Height(rightHeight_Lower));
				// Right Lower UI : 재질의 프리셋 설정
				OnGUI_RightLower(rightWidth, rightHeight_Lower);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndVertical();
				//---------------------------------------------------


				GUILayout.Space(5);
				EditorGUILayout.EndHorizontal();
				//-------------------------------------------------------------------------
				// Bottom
				GUILayout.Space(5);

				

				if(GUILayout.Button(_editor.GetText(TEXT.Close), GUILayout.Width(width - 10), GUILayout.Height(30)))
				{
					_editor.MaterialLibrary.Save();
					apEditorUtil.SetEditorDirty();
					isClose = true;
				}

				EditorGUILayout.EndVertical();



				// 복제 / 삭제 요청
				if(_duplicateMatSet != null)
				{
					apMaterialSet newMatSet = _editor.Controller.DuplicateMaterialSet(_duplicateMatSet);
					_duplicateMatSet = null;

					if(newMatSet != null)
					{
						_selectedMaterialSet = newMatSet;
					}
					Repaint();
				}

				if(_removeMatSet != null)
				{
					_editor.Controller.RemoveMaterialSet(_removeMatSet);
					_removeMatSet = null;
					_selectedMaterialSet = null;

					//가장 가까운 것을 선택하자.
					if(!_isPreset)
					{
						//일단 프리셋이 아닌 경우
						if(_portrait._materialSets.Count > 0)
						{
							_selectedMaterialSet = _portrait._materialSets[0];
							_isPreset = false;
						}
					}

					if(_selectedMaterialSet == null)
					{
						if(_editor.MaterialLibrary.Presets.Count > 0)
						{
							_selectedMaterialSet = _editor.MaterialLibrary.Presets[0];
							_isPreset = true;
						}
					}
					Repaint();
				}



				if(isClose)
				{
					Close();
				}


			}
			catch(Exception ex)
			{
				Debug.LogError("Material Library Exception : " + ex);

				//삭제 21.3.17 : MacOS 업데이트 이후 UI 에러가 빈번하게 발생하여 Close하게 만들면 안된다.
				//Close();
			}
		}

		private void OnDisable()
		{
			if(_editor != null && _editor.MaterialLibrary != null)
			{
				_editor.MaterialLibrary.Save();
				//Debug.LogError("Dialog 끝날때 자동 저장");
			}
		}

		// Sub GUI 함수들
		//-------------------------------------------------------------------------------
		private void OnGUI_LeftUpper(int width)
		{
			width += 20;

			//Portrait의 Material Set을 리스트로 표현하고 선택하자.
			GUILayout.Button(new GUIContent(_portrait.gameObject.name, _img_FoldDown), _guiStyle_None, GUILayout.Height(20));//<투명 버튼

			apMaterialSet curMatSet = null;
			for (int i = 0; i < _portrait._materialSets.Count; i++)
			{
				curMatSet = _portrait._materialSets[i];
				DrawMaterialSetInList(curMatSet, width, false);
			}

		}

		private void OnGUI_LeftLower(int width)
		{
			width += 20;

			//Editor의 Preset Material Set을 리스트로 표현하고 선택하자.
			
			GUILayout.Button(new GUIContent(_editor.GetUIWord(UIWORD.Presets), _img_FoldDown), _guiStyle_None, GUILayout.Height(20));//<투명 버튼

			apMaterialSet curMatSet = null;
			for (int i = 0; i < _editor.MaterialLibrary.Presets.Count; i++)
			{
				curMatSet = _editor.MaterialLibrary.Presets[i];
				DrawMaterialSetInList(curMatSet, width, true);
			}
		}



		private void DrawMaterialSetInList(apMaterialSet matSet, int width, bool isPreset)
		{
			GUIStyle curGUIStyle = _guiStyle_None;
			if (matSet == _selectedMaterialSet)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();

				#region [미사용 코드]
				//Color prevColor = GUI.backgroundColor;

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

				curGUIStyle = _guiStyle_Selected;
			}


			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(15);
			Texture2D iconImage = _img_MatTypeIcons[matSet._icon];
			if (GUILayout.Button(new GUIContent(" " + matSet._name, iconImage), curGUIStyle, /*GUILayout.Width(width - 35), */GUILayout.Height(20)))
			{
				_selectedMaterialSet = matSet;
				_isPreset = isPreset;

				_scroll_RightUpper = Vector2.zero;//<<스크롤 리셋
				apEditorUtil.ReleaseGUIFocus();
			}

			EditorGUILayout.EndHorizontal();
		}



		private void OnGUI_RightUpper(int width)
		{
			//선택된 객체의 속성을 보자
			if (_selectedMaterialSet == null)
			{
				return;
			}

			width -= 10;

			bool isNeedToEditorSave = false;
			
				

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;

			Color prevColor = GUI.backgroundColor;

			if (_isPreset)
			{
				//프리셋인 경우
				GUI.backgroundColor = new Color(1.0f, 0.8f, 0.8f, 1.0f);
			}
			else
			{
				//Portrait의 데이터인 경우
				GUI.backgroundColor = new Color(0.8f, 1.0f, 0.8f, 1.0f);
			}

			GUILayout.Space(10);
			GUILayout.Box(new GUIContent("  " + _selectedMaterialSet._name, _img_MatTypeIcons[_selectedMaterialSet._icon]),
				GUILayout.Width(width), GUILayout.Height(25));

			GUILayout.Space(10);

			GUI.backgroundColor = prevColor;

			//" Basic Properties"
			EditorGUILayout.LabelField(new GUIContent(" " + _editor.GetText(TEXT.BasicProperties), _img_BasicSettings), GUILayout.Height(28));
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(60));
			GUILayout.Space(5);

			int iconImgSize = 80;
			int width_BasicOption = width - (iconImgSize + 30);
			EditorGUILayout.BeginVertical(GUILayout.Width(width_BasicOption), GUILayout.Height(iconImgSize));
			//기본 설정들
			
			int width_BasicSetting_Label = width_BasicOption / 2;
			int width_BasicSetting_Value = width_BasicOption - (width_BasicSetting_Label + 10);

			//이름
			//string nextName = EditorGUILayout.DelayedTextField(_editor.GetUIWord(UIWORD.Name), _selectedMaterialSet._name);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_BasicOption));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Name), GUILayout.Width(width_BasicSetting_Label));
			string nextName = EditorGUILayout.DelayedTextField(_selectedMaterialSet._name, GUILayout.Width(width_BasicSetting_Value));
			EditorGUILayout.EndHorizontal();

			if (!string.Equals(nextName, _selectedMaterialSet._name))
			{
				//이름 바꾸기
				if (!_isPreset)
				{
					//Portrait 데이터 : Undo + 이름바꾸기
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_selectedMaterialSet._name = nextName;
				}
				else if(!_selectedMaterialSet._isReserved)
				{
					//Reserved가 아닌 프리셋 : 이름 바꾸고 Save
					_selectedMaterialSet._name = nextName;
					isNeedToEditorSave = true;
				}
				else
				{
					//Reserved인 경우 경고 메시지 보여주기
					ShowWarningIfReservedPresetChanged();
				}
				apEditorUtil.ReleaseGUIFocus();
			}

			//아이콘
			//apMaterialSet.ICON nextIcon = (apMaterialSet.ICON)EditorGUILayout.EnumPopup(_editor.GetText(TEXT.DLG_Icon), _selectedMaterialSet._icon);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_BasicOption));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Icon), GUILayout.Width(width_BasicSetting_Label));
			apMaterialSet.ICON nextIcon = (apMaterialSet.ICON)EditorGUILayout.EnumPopup(_selectedMaterialSet._icon, GUILayout.Width(width_BasicSetting_Value));
			EditorGUILayout.EndHorizontal();
			
			if (nextIcon != _selectedMaterialSet._icon)
			{
				if (!_isPreset)
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					//Portrait 데이터 : 아이콘 바꾸기
					_selectedMaterialSet._icon = nextIcon;
				}
				else if(!_selectedMaterialSet._isReserved)
				{
					//Reserved가 아닌 프리셋 : 아이콘 바꾸고 Save
					_selectedMaterialSet._icon = nextIcon;
					isNeedToEditorSave = true;
				}
				else
				{
					//Reserved인 경우 경고 메시지 보여주기
					ShowWarningIfReservedPresetChanged();
				}
				apEditorUtil.ReleaseGUIFocus();
			}

			//"Black Ambient Required"
			//bool isNextBlackColoredAmbient = EditorGUILayout.Toggle(_editor.GetText(TEXT.BlackAmbientRequired), _selectedMaterialSet._isNeedToSetBlackColoredAmbient);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width_BasicOption));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.BlackAmbientRequired), GUILayout.Width(width_BasicSetting_Label));
			bool isNextBlackColoredAmbient = EditorGUILayout.Toggle(_selectedMaterialSet._isNeedToSetBlackColoredAmbient, GUILayout.Width(width_BasicSetting_Value));
			EditorGUILayout.EndHorizontal();

			if(isNextBlackColoredAmbient != _selectedMaterialSet._isNeedToSetBlackColoredAmbient)
			{
				if (!_isPreset)
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					//Portrait 데이터 : 아이콘 바꾸기
					_selectedMaterialSet._isNeedToSetBlackColoredAmbient = isNextBlackColoredAmbient;
				}
				else if(!_selectedMaterialSet._isReserved)
				{
					//Reserved가 아닌 프리셋 : 아이콘 바꾸고 Save
					_selectedMaterialSet._isNeedToSetBlackColoredAmbient = isNextBlackColoredAmbient;
					isNeedToEditorSave = true;
				}
				else
				{
					//Reserved인 경우 경고 메시지 보여주기
					ShowWarningIfReservedPresetChanged();
				}
				apEditorUtil.ReleaseGUIFocus();
			}

			if(!_isPreset)
			{
				//프리셋이 아니라면,
				//- Default 설정 가능
				//"Default Material ON", "Default Material OFF"
				if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DefaultMaterialON), _editor.GetText(TEXT.DefaultMaterialOFF), _selectedMaterialSet._isDefault, true, width_BasicOption - 10, 24))
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_selectedMaterialSet._isDefault = !_selectedMaterialSet._isDefault;

					apMaterialSet otherMatSet = null;

					bool isNextDefaultMatSetAssigned = false;
					for (int i = 0; i < _portrait._materialSets.Count; i++)
					{
						otherMatSet = _portrait._materialSets[i];
						if (otherMatSet == _selectedMaterialSet)
						{
							continue;
						}

						if (_selectedMaterialSet._isDefault)
						{
							//Default가 true라면, 현재의 MaterialSet 이외의 Default 속성은 false가 된다.
							otherMatSet._isDefault = false;
						}
						else
						{
							//Default가 false라면, 현재의 MaterialSet 이외의 첫번째 MaterialSet이 자동으로 Default가 된다.
							if(!isNextDefaultMatSetAssigned)
							{
								otherMatSet._isDefault = true;
								isNextDefaultMatSetAssigned = true;
							}
							else
							{
								//이미 다른 Default Material Set이 설정되었다.
								otherMatSet._isDefault = false;
							}
						}
					}
				}
			}
			EditorGUILayout.EndVertical();


			EditorGUILayout.BeginVertical(GUILayout.Width(iconImgSize), GUILayout.Height(iconImgSize));
			//Icon
			GUILayout.Box(_img_MatTypeIcons[_selectedMaterialSet._icon], guiStyle_Box, GUILayout.Width(iconImgSize), GUILayout.Height(iconImgSize));
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();
			
			//쉐이더들
			Shader nextShader_Normal_AlphaBlend = _selectedMaterialSet._shader_Normal_AlphaBlend;
			Shader nextShader_Normal_Additive = _selectedMaterialSet._shader_Normal_Additive;
			Shader nextShader_Normal_SoftAdditive = _selectedMaterialSet._shader_Normal_SoftAdditive;
			Shader nextShader_Normal_Multiplicative = _selectedMaterialSet._shader_Normal_Multiplicative;
			Shader nextShader_Clipped_AlphaBlend = _selectedMaterialSet._shader_Clipped_AlphaBlend;
			Shader nextShader_Clipped_Additive = _selectedMaterialSet._shader_Clipped_Additive;
			Shader nextShader_Clipped_SoftAdditive = _selectedMaterialSet._shader_Clipped_SoftAdditive;
			Shader nextShader_Clipped_Multiplicative = _selectedMaterialSet._shader_Clipped_Multiplicative;
			Shader nextShader_L_Normal_AlphaBlend = _selectedMaterialSet._shader_L_Normal_AlphaBlend;
			Shader nextShader_L_Normal_Additive = _selectedMaterialSet._shader_L_Normal_Additive;
			Shader nextShader_L_Normal_SoftAdditive = _selectedMaterialSet._shader_L_Normal_SoftAdditive;
			Shader nextShader_L_Normal_Multiplicative = _selectedMaterialSet._shader_L_Normal_Multiplicative;
			Shader nextShader_L_Clipped_AlphaBlend = _selectedMaterialSet._shader_L_Clipped_AlphaBlend;
			Shader nextShader_L_Clipped_Additive = _selectedMaterialSet._shader_L_Clipped_Additive;
			Shader nextShader_L_Clipped_SoftAdditive = _selectedMaterialSet._shader_L_Clipped_SoftAdditive;
			Shader nextShader_L_Clipped_Multiplicative = _selectedMaterialSet._shader_L_Clipped_Multiplicative;
			Shader nextShader_AlphaMask = _selectedMaterialSet._shader_AlphaMask;





			GUILayout.Space(15);
			//" Shaders"
			EditorGUILayout.LabelField(new GUIContent(" " + _editor.GetText(TEXT.Shaders), _img_Shaders), GUILayout.Height(28));

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.ShaderInfo_1_CS_Gamma));//"1. Color Space : Gamma"
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.ShaderInfo_BasicRendering));//"  Basic Rendering"
			nextShader_Normal_AlphaBlend =		ShaderObjectField("   Alpha Blend", _selectedMaterialSet._shader_Normal_AlphaBlend, width);
			nextShader_Normal_Additive =		ShaderObjectField("   Additive", _selectedMaterialSet._shader_Normal_Additive, width);
			nextShader_Normal_SoftAdditive =	ShaderObjectField("   Soft Additive", _selectedMaterialSet._shader_Normal_SoftAdditive, width);
			nextShader_Normal_Multiplicative =	ShaderObjectField("   Multiplicative", _selectedMaterialSet._shader_Normal_Multiplicative, width);

			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.ShaderInfo_ClippedRendering));//Clipped Rendering
			nextShader_Clipped_AlphaBlend =		ShaderObjectField("   Alpha Blend", _selectedMaterialSet._shader_Clipped_AlphaBlend, width);
			nextShader_Clipped_Additive =		ShaderObjectField("   Additive", _selectedMaterialSet._shader_Clipped_Additive, width);
			nextShader_Clipped_SoftAdditive =	ShaderObjectField("   Soft Additive", _selectedMaterialSet._shader_Clipped_SoftAdditive, width);
			nextShader_Clipped_Multiplicative =	ShaderObjectField("   Multiplicative", _selectedMaterialSet._shader_Clipped_Multiplicative, width);

			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.ShaderInfo_2_CS_Linear));//"2. Color Space : Linear"
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.ShaderInfo_BasicRendering));//"  Basic Rendering"
			nextShader_L_Normal_AlphaBlend =		ShaderObjectField("   Alpha Blend", _selectedMaterialSet._shader_L_Normal_AlphaBlend, width);
			nextShader_L_Normal_Additive =			ShaderObjectField("   Additive", _selectedMaterialSet._shader_L_Normal_Additive, width);
			nextShader_L_Normal_SoftAdditive =		ShaderObjectField("   Soft Additive", _selectedMaterialSet._shader_L_Normal_SoftAdditive, width);
			nextShader_L_Normal_Multiplicative =	ShaderObjectField("   Multiplicative", _selectedMaterialSet._shader_L_Normal_Multiplicative, width);

			GUILayout.Space(5);
			EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.ShaderInfo_ClippedRendering));//Clipped Rendering
			nextShader_L_Clipped_AlphaBlend =		ShaderObjectField("   Alpha Blend", _selectedMaterialSet._shader_L_Clipped_AlphaBlend, width);
			nextShader_L_Clipped_Additive =			ShaderObjectField("   Additive", _selectedMaterialSet._shader_L_Clipped_Additive, width);
			nextShader_L_Clipped_SoftAdditive =		ShaderObjectField("   Soft Additive", _selectedMaterialSet._shader_L_Clipped_SoftAdditive, width);
			nextShader_L_Clipped_Multiplicative =	ShaderObjectField("   Multiplicative", _selectedMaterialSet._shader_L_Clipped_Multiplicative, width);

			GUILayout.Space(10);
			nextShader_AlphaMask = ShaderObjectField(_editor.GetText(TEXT.ShaderInfo_3_AlphaMask), _selectedMaterialSet._shader_AlphaMask, width);//"3. Alpha Mask"

			//편집이 가능할 때
			if (nextShader_Normal_AlphaBlend != _selectedMaterialSet._shader_Normal_AlphaBlend
				|| nextShader_Normal_Additive != _selectedMaterialSet._shader_Normal_Additive
				|| nextShader_Normal_SoftAdditive != _selectedMaterialSet._shader_Normal_SoftAdditive
				|| nextShader_Normal_Multiplicative != _selectedMaterialSet._shader_Normal_Multiplicative

				|| nextShader_Clipped_AlphaBlend != _selectedMaterialSet._shader_Clipped_AlphaBlend
				|| nextShader_Clipped_Additive != _selectedMaterialSet._shader_Clipped_Additive
				|| nextShader_Clipped_SoftAdditive != _selectedMaterialSet._shader_Clipped_SoftAdditive
				|| nextShader_Clipped_Multiplicative != _selectedMaterialSet._shader_Clipped_Multiplicative

				|| nextShader_L_Normal_AlphaBlend != _selectedMaterialSet._shader_L_Normal_AlphaBlend
				|| nextShader_L_Normal_Additive != _selectedMaterialSet._shader_L_Normal_Additive
				|| nextShader_L_Normal_SoftAdditive != _selectedMaterialSet._shader_L_Normal_SoftAdditive
				|| nextShader_L_Normal_Multiplicative != _selectedMaterialSet._shader_L_Normal_Multiplicative

				|| nextShader_L_Clipped_AlphaBlend != _selectedMaterialSet._shader_L_Clipped_AlphaBlend
				|| nextShader_L_Clipped_Additive != _selectedMaterialSet._shader_L_Clipped_Additive
				|| nextShader_L_Clipped_SoftAdditive != _selectedMaterialSet._shader_L_Clipped_SoftAdditive
				|| nextShader_L_Clipped_Multiplicative != _selectedMaterialSet._shader_L_Clipped_Multiplicative

				|| nextShader_AlphaMask != _selectedMaterialSet._shader_AlphaMask
				)
			{
				//Shader가 바뀐 것을 확인하자.
				bool isShaderChangable = false;
				if (!_isPreset)
				{
					//Portrait 데이터일 때 - Undo 등록
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					isShaderChangable = true;
				}
				else if(!_selectedMaterialSet._isReserved)
				{
					//프리셋일때 (Reserved는 아님)
					isNeedToEditorSave = true;
					isShaderChangable = true;
				}
				else
				{
					//Reserved일 때
					//편집 불가 + 경고 메시지
					isShaderChangable = false;
					ShowWarningIfReservedPresetChanged();
				}

				if (isShaderChangable)
				{
					//변경된 Shader 적용
					_selectedMaterialSet._shader_Normal_AlphaBlend = nextShader_Normal_AlphaBlend;
					_selectedMaterialSet._shader_Normal_Additive = nextShader_Normal_Additive;
					_selectedMaterialSet._shader_Normal_SoftAdditive = nextShader_Normal_SoftAdditive;
					_selectedMaterialSet._shader_Normal_Multiplicative = nextShader_Normal_Multiplicative;

					_selectedMaterialSet._shader_Clipped_AlphaBlend = nextShader_Clipped_AlphaBlend;
					_selectedMaterialSet._shader_Clipped_Additive = nextShader_Clipped_Additive;
					_selectedMaterialSet._shader_Clipped_SoftAdditive = nextShader_Clipped_SoftAdditive;
					_selectedMaterialSet._shader_Clipped_Multiplicative = nextShader_Clipped_Multiplicative;

					_selectedMaterialSet._shader_L_Normal_AlphaBlend = nextShader_L_Normal_AlphaBlend;
					_selectedMaterialSet._shader_L_Normal_Additive = nextShader_L_Normal_Additive;
					_selectedMaterialSet._shader_L_Normal_SoftAdditive = nextShader_L_Normal_SoftAdditive;
					_selectedMaterialSet._shader_L_Normal_Multiplicative = nextShader_L_Normal_Multiplicative;

					_selectedMaterialSet._shader_L_Clipped_AlphaBlend = nextShader_L_Clipped_AlphaBlend;
					_selectedMaterialSet._shader_L_Clipped_Additive = nextShader_L_Clipped_Additive;
					_selectedMaterialSet._shader_L_Clipped_SoftAdditive = nextShader_L_Clipped_SoftAdditive;
					_selectedMaterialSet._shader_L_Clipped_Multiplicative = nextShader_L_Clipped_Multiplicative;

					_selectedMaterialSet._shader_AlphaMask = nextShader_AlphaMask;


					//추가 22.7.11
					//쉐이더 에셋 변경 직후 Path가 변경되지 않은 버그 수정
					_selectedMaterialSet._shaderPath_Normal_AlphaBlend =		GetShaderPath(_selectedMaterialSet._shader_Normal_AlphaBlend);
					_selectedMaterialSet._shaderPath_Normal_Additive =			GetShaderPath(_selectedMaterialSet._shader_Normal_Additive);
					_selectedMaterialSet._shaderPath_Normal_SoftAdditive =		GetShaderPath(_selectedMaterialSet._shader_Normal_SoftAdditive);
					_selectedMaterialSet._shaderPath_Normal_Multiplicative =	GetShaderPath(_selectedMaterialSet._shader_Normal_Multiplicative);

					_selectedMaterialSet._shaderPath_Clipped_AlphaBlend =		GetShaderPath(_selectedMaterialSet._shader_Clipped_AlphaBlend);
					_selectedMaterialSet._shaderPath_Clipped_Additive =			GetShaderPath(_selectedMaterialSet._shader_Clipped_Additive);
					_selectedMaterialSet._shaderPath_Clipped_SoftAdditive =		GetShaderPath(_selectedMaterialSet._shader_Clipped_SoftAdditive);
					_selectedMaterialSet._shaderPath_Clipped_Multiplicative =	GetShaderPath(_selectedMaterialSet._shader_Clipped_Multiplicative);

					_selectedMaterialSet._shaderPath_L_Normal_AlphaBlend =		GetShaderPath(_selectedMaterialSet._shader_L_Normal_AlphaBlend);
					_selectedMaterialSet._shaderPath_L_Normal_Additive =		GetShaderPath(_selectedMaterialSet._shader_L_Normal_Additive);
					_selectedMaterialSet._shaderPath_L_Normal_SoftAdditive =	GetShaderPath(_selectedMaterialSet._shader_L_Normal_SoftAdditive);
					_selectedMaterialSet._shaderPath_L_Normal_Multiplicative =	GetShaderPath(_selectedMaterialSet._shader_L_Normal_Multiplicative);

					_selectedMaterialSet._shaderPath_L_Clipped_AlphaBlend =		GetShaderPath(_selectedMaterialSet._shader_L_Clipped_AlphaBlend);
					_selectedMaterialSet._shaderPath_L_Clipped_Additive =		GetShaderPath(_selectedMaterialSet._shader_L_Clipped_Additive);
					_selectedMaterialSet._shaderPath_L_Clipped_SoftAdditive =	GetShaderPath(_selectedMaterialSet._shader_L_Clipped_SoftAdditive);
					_selectedMaterialSet._shaderPath_L_Clipped_Multiplicative = GetShaderPath(_selectedMaterialSet._shader_L_Clipped_Multiplicative);

					_selectedMaterialSet._shaderPath_AlphaMask =				GetShaderPath(_selectedMaterialSet._shader_AlphaMask);
				}

				Repaint();
				//apEditorUtil.ReleaseGUIFocus();
			}



			//프로퍼티들
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			GUILayout.Space(15);
			//" Shader Properties"
			EditorGUILayout.LabelField(new GUIContent(" " + _editor.GetText(TEXT.ShaderProperties), _img_ShaderProperties), GUILayout.Height(28));

			GUILayout.Space(10);

			apMaterialSet.PropertySet propSet = null;
			apMaterialSet.PropertySet removePropSet = null;//<<삭제할 PropSet

			int height_1Line = 22;
			int height_1Line_Comp = 18;

			int size_2Line_Texture = 80;
			GUIStyle guiStyleProp_Label = new GUIStyle(GUI.skin.label);
			guiStyleProp_Label.margin = GUI.skin.button.margin;

			GUIStyle guiStyleProp_TextField = new GUIStyle(GUI.skin.textField);
			guiStyleProp_TextField.margin = GUI.skin.button.margin;

			GUIStyle guiStyle_LabelCenter = new GUIStyle(GUI.skin.label);
			guiStyle_LabelCenter.alignment = TextAnchor.MiddleCenter;

			GUIStyle guiStyle_LabelLeftCenter = new GUIStyle(GUI.skin.label);
			guiStyle_LabelLeftCenter.alignment = TextAnchor.MiddleLeft;
			
			
			//Color prevColor = GUI.backgroundColor;
			for (int iProp = 0; iProp < _selectedMaterialSet._propertySets.Count; iProp++)
			{
				propSet = _selectedMaterialSet._propertySets[iProp];

				//배경을 그리자
				int yOffset = 10;
				if(iProp == 0)
				{
					yOffset = 2;
				}

				//배경 박스의 크기와 색상
				Rect lastRect = GUILayoutUtility.GetLastRect();
				Color bgColor = GUI.backgroundColor;
				int propHeight = 40;
				if (propSet._isReserved || (_isPreset && _selectedMaterialSet._isReserved))
				{
					bgColor = new Color(bgColor.r * 0.9f, bgColor.g * 0.9f, bgColor.b * 0.9f, 1.0f);

					if(propSet._propType == apMaterialSet.SHADER_PROP_TYPE.Texture
						&& propSet._isOptionEnabled
						&& propSet._isCommonTexture
						&& !propSet._isReserved)
					{
						//텍스쳐 타입 + Common Texture인 경우 보여주기 위해 Height를 증가
						propHeight += size_2Line_Texture + 9;
					}
				}
				else if (!propSet._isOptionEnabled)
				{
					bgColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1.0f);
				}
				else
				{
					switch (propSet._propType)
					{
						case apMaterialSet.SHADER_PROP_TYPE.Float://초록색
							bgColor = new Color(bgColor.r * 0.7f, bgColor.g * 1.0f, bgColor.b * 0.7f, 1.0f);
							//if(propSet._isOptionEnabled)	{ propHeight = 64; }
							
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Int://노란색
							bgColor = new Color(bgColor.r * 1.0f, bgColor.g * 1.0f, bgColor.b * 0.7f, 1.0f);
							//if(propSet._isOptionEnabled)	{ propHeight = 64; }
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Vector://파란색
							bgColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.9f, bgColor.b * 1.0f, 1.0f);
							//if(propSet._isOptionEnabled)	{ propHeight = 64; }
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Texture://청록색
							bgColor = new Color(bgColor.r * 0.7f, bgColor.g * 1.0f, bgColor.b * 0.9f, 1.0f);
							if(propSet._isOptionEnabled)
							{
								if(propSet._isCommonTexture)
								{
									propHeight += size_2Line_Texture + 4 + 5;
								}
								else
								{
									if (!_isPreset)
									{
										propHeight += propSet._imageTexturePairs.Count * (size_2Line_Texture + 5 + 4) + 5;
									}
								}
							}
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Color://보라색
							bgColor = new Color(bgColor.r * 1.0f, bgColor.g * 0.7f, bgColor.b * 1.0f, 1.0f);
							//if(propSet._isOptionEnabled)	{ propHeight = 64; }
							break;
					}
				}

				//배경 박스
				GUI.backgroundColor = bgColor;
				GUI.Box(new Rect(lastRect.x, lastRect.y + yOffset, width, propHeight), "");
				GUI.backgroundColor = prevColor;
				
				//첫번째 줄
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_1Line));
				GUILayout.Space(5);

				//- Reserved 아이콘 또는 Enabled 토글
				if (propSet._isReserved || (_isPreset && _selectedMaterialSet._isReserved))
				{
					EditorGUILayout.LabelField(new GUIContent("", _img_Reserved), GUILayout.Width(30), GUILayout.Height(height_1Line_Comp));
				}
				else
				{
					bool nextEnabledOption = EditorGUILayout.Toggle(propSet._isOptionEnabled, GUILayout.Width(30), GUILayout.Height(height_1Line_Comp));
					if(nextEnabledOption != propSet._isOptionEnabled)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						propSet._isOptionEnabled = nextEnabledOption;
						apEditorUtil.ReleaseGUIFocus();
					}
				}

				GUILayout.Space(5);

				//- Property 이름
				string nextPropName = EditorGUILayout.DelayedTextField(propSet._name, guiStyleProp_TextField, GUILayout.Width(170), GUILayout.Height(height_1Line_Comp));
				if(!string.Equals(nextPropName, propSet._name))
				{
					if(!_isPreset)
					{
						//Portrait 데이터
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						propSet._name = nextPropName;
					}
					else if(!_selectedMaterialSet._isReserved)
					{
						//Reserved가 아닌 Preset
						propSet._name = nextPropName;
						isNeedToEditorSave = true;
					}
					else
					{
						apEditorUtil.ReleaseGUIFocus();

						nextPropName = propSet._name;

						//Reserved -> 경고 메시지
						ShowWarningIfReservedPresetChanged();
					}
					apEditorUtil.ReleaseGUIFocus();
				}

				apMaterialSet.SHADER_PROP_TYPE nextPropType = (apMaterialSet.SHADER_PROP_TYPE)EditorGUILayout.EnumPopup(propSet._propType, GUILayout.Width(80), GUILayout.Height(height_1Line_Comp));
				if(nextPropType != propSet._propType)
				{
					apMaterialSet.SHADER_PROP_TYPE prevPropType = propSet._propType;
					if(!_isPreset)
					{
						bool isChange = true;
						if(prevPropType == apMaterialSet.SHADER_PROP_TYPE.Texture)
						{
							//Texture > 다른 속성인 경우 한번 물어보자
							//"Change type", 
							//"If you change from a [Texture] type to a different type, the Texture Asset property is initialized.\nDo you want to change the type?",
							isChange = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_ShaderPropChangeWarning_Title), 
																	_editor.GetText(TEXT.DLG_ShaderPropChangeWarning_Body),
																	_editor.GetText(TEXT.Okay),
																	_editor.GetText(TEXT.Cancel)
																	);
						}

						if (isChange)
						{
							//Portrait 데이터
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
																_editor, 
																_portrait, 
																//_portrait, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							propSet._propType = nextPropType;

							//Type이 바뀌었다면 링크 한번더.
							_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);
							Repaint();
						}
					}
					else if(!_selectedMaterialSet._isReserved)
					{
						bool isChange = true;
						if(prevPropType == apMaterialSet.SHADER_PROP_TYPE.Texture)
						{
							//Texture > 다른 속성인 경우 한번 물어보자
							//"Change type", 
							//"If you change from a [Texture] type to a different type, the Texture Asset property is initialized.\nDo you want to change the type?",
							isChange = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_ShaderPropChangeWarning_Title), 
																	_editor.GetText(TEXT.DLG_ShaderPropChangeWarning_Body),
																	_editor.GetText(TEXT.Okay),
																	_editor.GetText(TEXT.Cancel)
																	);
						}

						if (isChange)
						{
							//Reserved가 아닌 Preset
							propSet._propType = nextPropType;
							isNeedToEditorSave = true;

							//Type이 바뀌었다면 링크 한번더.
							_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);
							Repaint();
						}
					}
					else
					{
						//Reserved -> 경고 메시지
						ShowWarningIfReservedPresetChanged();
					}

					apEditorUtil.ReleaseGUIFocus();
				}
				GUILayout.Space(5);


				//값을 넣자
				if (!propSet._isReserved)
				{
					GUILayout.Space(2);
					int width_Value = width - (365);

					if (propSet._isOptionEnabled)
					{
						

						switch (propSet._propType)
						{
							//1. Float 타입인 경우
							case apMaterialSet.SHADER_PROP_TYPE.Float:
								{
									float nextFloat = EditorGUILayout.DelayedFloatField(propSet._value_Float, GUILayout.Width(width_Value));
									if (Mathf.Abs(nextFloat - propSet._value_Float) > 0.0001f)
									{
										if (!_isPreset)
										{
											apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
																				_editor, 
																				_portrait, 
																				//_portrait, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);

											propSet._value_Float = nextFloat;
											apEditorUtil.ReleaseGUIFocus();
										}
										else if (!_selectedMaterialSet._isReserved)
										{
											propSet._value_Float = nextFloat;
											isNeedToEditorSave = true;
										}
									}
								}
								break;
							
							//2. Int 타입인 경우
							case apMaterialSet.SHADER_PROP_TYPE.Int:
								{
									int nextInt = EditorGUILayout.DelayedIntField(propSet._value_Int, GUILayout.Width(width_Value));
									if (nextInt != propSet._value_Int)
									{
										if (!_isPreset)
										{
											apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
																				_editor, 
																				_portrait, 
																				//_portrait, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);

											propSet._value_Int = nextInt;
											apEditorUtil.ReleaseGUIFocus();
										}
										else if (!_selectedMaterialSet._isReserved)
										{
											propSet._value_Int = nextInt;
											isNeedToEditorSave = true;
										}

									}
								}
								break;

							//3. Vector 타입인 경우
							case apMaterialSet.SHADER_PROP_TYPE.Vector:
								{
									int width_Value_V1 = (width_Value / 4) - 3;
									float vecX = EditorGUILayout.DelayedFloatField(propSet._value_Vector.x, GUILayout.Width(width_Value_V1));
									float vecY = EditorGUILayout.DelayedFloatField(propSet._value_Vector.y, GUILayout.Width(width_Value_V1));
									float vecZ = EditorGUILayout.DelayedFloatField(propSet._value_Vector.z, GUILayout.Width(width_Value_V1));
									float vecW = EditorGUILayout.DelayedFloatField(propSet._value_Vector.w, GUILayout.Width(width_Value_V1));

									if (Mathf.Abs(vecX - propSet._value_Vector.x) > 0.0001f
										|| Mathf.Abs(vecY - propSet._value_Vector.y) > 0.0001f
										|| Mathf.Abs(vecZ - propSet._value_Vector.z) > 0.0001f
										|| Mathf.Abs(vecW - propSet._value_Vector.w) > 0.0001f)
									{
										if (!_isPreset)
										{
											apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
																				_editor, 
																				_portrait, 
																				//_portrait, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);

											propSet._value_Vector.x = vecX;
											propSet._value_Vector.y = vecY;
											propSet._value_Vector.z = vecZ;
											propSet._value_Vector.w = vecW;
											apEditorUtil.ReleaseGUIFocus();
										}
										else if (!_selectedMaterialSet._isReserved)
										{
											propSet._value_Vector.x = vecX;
											propSet._value_Vector.y = vecY;
											propSet._value_Vector.z = vecZ;
											propSet._value_Vector.w = vecW;
											isNeedToEditorSave = true;
										}
									}
									
								}
								break;

							//4. Texture 타입인 경우
							case apMaterialSet.SHADER_PROP_TYPE.Texture:
								{
									//"Common Texture", "Texture per Image"
									bool nextCommonTexture = apEditorUtil.ToggledButton_2Side(	_editor.GetText(TEXT.CommonTexture),
																								_editor.GetText(TEXT.TexturePerImage), 
																								propSet._isCommonTexture, true, width_Value, height_1Line_Comp);

									if(nextCommonTexture)
									{
										if (!_isPreset)
										{
											apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
																				_editor, 
																				_portrait, 
																				//_portrait, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);

											propSet._isCommonTexture = !propSet._isCommonTexture;

											//Common Texture 타입이 바뀌었다면 링크 한번더.
											_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);
											apEditorUtil.ReleaseGUIFocus();
										}
										else if (!_selectedMaterialSet._isReserved)
										{
											propSet._isCommonTexture = !propSet._isCommonTexture;

											//Common Texture 타입이 바뀌었다면 링크 한번더.
											_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);

											isNeedToEditorSave = true;
										}
									}
								}
								break;

							//5. Color 타입인 경우
							case apMaterialSet.SHADER_PROP_TYPE.Color:
								{
									Color nextColor = propSet._value_Color;
									try
									{
										nextColor = EditorGUILayout.ColorField(propSet._value_Color, GUILayout.Width(width_Value));
									}
									catch (Exception) { }

									if (Mathf.Abs(nextColor.r - propSet._value_Color.r) > 0.001f
											|| Mathf.Abs(nextColor.g - propSet._value_Color.g) > 0.001f
											|| Mathf.Abs(nextColor.b - propSet._value_Color.b) > 0.001f
											|| Mathf.Abs(nextColor.a - propSet._value_Color.a) > 0.001f)
									{
										//색상은 그냥 대입
										if (!_isPreset)
										{
											//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.MaterialSetChanged, _editor, _portrait, _portrait, false);
											propSet._value_Color = nextColor;
											//apEditorUtil.ReleaseGUIFocus();
										}
										else if (!_selectedMaterialSet._isReserved)
										{
											propSet._value_Color = nextColor;
											//isNeedToEditorSave = true;
										}
									}
								}
								break;
						}

						
					}
					else
					{
						GUILayout.Space(width_Value + 4);
					}
					GUILayout.Space(4);
					
					
				}


				//삭제 버튼
				if(!propSet._isReserved && (!_isPreset || !_selectedMaterialSet._isReserved))
				{
					//Reserved Property가 아닌 경우
					//- 삭제 가능
					if(GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(height_1Line_Comp)))
					{
						//현재 파라미터 삭제
						//"Remove Property"
						//"Do you want to remove the property [" + propSet._name + "] ?"
						bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_RemoveMatSetProperty_Title), 
																	_editor.GetTextFormat(TEXT.DLG_RemoveMatSetProperty_Body, propSet._name), 
																	_editor.GetText(TEXT.Remove),
																	_editor.GetText(TEXT.Cancel));

						if(result)
						{
							removePropSet = propSet;
						}

						apEditorUtil.ReleaseGUIFocus();
					}
				}

				EditorGUILayout.EndHorizontal();


				//텍스쳐 타입이라면 1줄로 끝나지 않는다.
				if (!propSet._isReserved 
					&& propSet._isOptionEnabled 
					&& propSet._propType == apMaterialSet.SHADER_PROP_TYPE.Texture
					&& ((_isPreset && propSet._isCommonTexture) || !_isPreset)
					)
				{
					GUILayout.Space(5);
					if(propSet._isCommonTexture)
					{
						//1줄만 적용
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(size_2Line_Texture));
						GUILayout.Space(10);
						EditorGUILayout.BeginVertical(GUILayout.Width(240), GUILayout.Height(size_2Line_Texture));
						EditorGUILayout.LabelField(_editor.GetText(TEXT.CommonTexture), guiStyle_LabelLeftCenter, GUILayout.Width(240), GUILayout.Height(size_2Line_Texture));//"Common Texture"
						EditorGUILayout.EndVertical();
						
						GUILayout.Space(20);
						EditorGUILayout.LabelField(" >> ", guiStyle_LabelCenter, GUILayout.Width(width - 500), GUILayout.Height(size_2Line_Texture));
						GUILayout.Space(20);

						try
						{
							//EditorGUILayout.BeginVertical(GUILayout.Width(size_2Line_Texture), GUILayout.Height(size_2Line_Texture));
							Texture nextTextureAsset = EditorGUILayout.ObjectField(propSet._value_CommonTexture, typeof(Texture), false, GUILayout.Width(size_2Line_Texture), GUILayout.Height(size_2Line_Texture)) as Texture;
							//EditorGUILayout.EndVertical();

							

							if (nextTextureAsset != propSet._value_CommonTexture)
							{
								//이미지 대입하고 다시 Link
								if (!_isPreset)
								{
									//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.MaterialSetChanged, _editor, _portrait, _portrait, false);
									propSet._value_CommonTexture = nextTextureAsset;
									if(nextTextureAsset == null)
									{
										//아예 Path까지 날려서 복구를 못하게 해야한다.
										propSet._commonTexturePath = "";
									}

									//Common Texture 타입이 바뀌었다면 링크 한번더.
									_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);
									//apEditorUtil.ReleaseGUIFocus();
								}
								else if (!_selectedMaterialSet._isReserved)
								{
									propSet._value_CommonTexture = nextTextureAsset;
									if(nextTextureAsset == null)
									{
										//아예 Path까지 날려서 복구를 못하게 해야한다.
										propSet._commonTexturePath = "";
									}

									//Common Texture 타입이 바뀌었다면 링크 한번더.
									_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);

									//isNeedToEditorSave = true;
								}
							}
						}
						catch (Exception) { }
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						//Image 개수에 따라 적용
						apMaterialSet.PropertySet.ImageTexturePair pair = null;
						for (int iPair = 0; iPair < propSet._imageTexturePairs.Count; iPair++)
						{
							pair = propSet._imageTexturePairs[iPair];
							if(pair._targetTextureData == null)
							{
								continue;
							}

							EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(size_2Line_Texture));
							GUILayout.Space(10);
							EditorGUILayout.BeginVertical(GUILayout.Width(240), GUILayout.Height(size_2Line_Texture));
							EditorGUILayout.LabelField(pair._targetTextureData._name, GUILayout.Width(240), GUILayout.Height(20));
							EditorGUILayout.LabelField(new GUIContent(pair._targetTextureData._image, ""), GUILayout.Width(240), GUILayout.Height(size_2Line_Texture - 22));
							EditorGUILayout.EndVertical();

							GUILayout.Space(20);
							EditorGUILayout.LabelField(" >> ", guiStyle_LabelCenter, GUILayout.Width(width - 500), GUILayout.Height(size_2Line_Texture));
							GUILayout.Space(20);

							try
							{
								Texture nextPairTextureAsset = (Texture)EditorGUILayout.ObjectField(pair._textureAsset, typeof(Texture), true, GUILayout.Width(size_2Line_Texture), GUILayout.Height(size_2Line_Texture));

								if (nextPairTextureAsset != pair._textureAsset)
								{
									if (!_isPreset)
									{
										//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.MaterialSetChanged, _editor, _portrait, _portrait, false);
										pair._textureAsset = nextPairTextureAsset;

										if(nextPairTextureAsset == null)
										{
											//아예 Path까지 날려서 복구를 못하게 해야한다.
											pair._textureAssetPath = "";
										}

										//Common Texture 타입이 바뀌었다면 링크 한번더.
										_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);
										//apEditorUtil.ReleaseGUIFocus();
									}
									else if (!_selectedMaterialSet._isReserved)
									{
										pair._textureAsset = nextPairTextureAsset;

										if (nextPairTextureAsset == null)
										{
											//아예 Path까지 날려서 복구를 못하게 해야한다.
											pair._textureAssetPath = "";
										}

										//Common Texture 타입이 바뀌었다면 링크 한번더.
										_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, _isPreset, _portrait);

										//isNeedToEditorSave = true;
									}
								}
							}
							catch (Exception) { }

							EditorGUILayout.EndHorizontal();

							GUILayout.Space(5);
						}
					}
				}



				GUILayout.Space(20);
			}

			if (removePropSet != null && _selectedMaterialSet._propertySets.Contains(removePropSet) && !removePropSet._isReserved)
			{
				//Shader Property 삭제하자
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
													_editor, 
													_portrait, 
													//_portrait, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);


				_selectedMaterialSet._propertySets.Remove(removePropSet);
				removePropSet = null;
			}



			if (!_isPreset)
			{
				//프리셋이 아니라면 프로퍼티를 추가하자
				//" Add Property"
				if (GUILayout.Button(new GUIContent(" " + _editor.GetText(TEXT.AddProperty), _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_AddTransform)), GUILayout.Width(width), GUILayout.Height(25)))
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
														_editor, 
														_portrait, 
														//_portrait, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					apMaterialSet.PropertySet newProp = new apMaterialSet.PropertySet();
					newProp._name = "(New Property)";
					newProp._isReserved = false;
					newProp._isOptionEnabled = true;
					newProp._propType = apMaterialSet.SHADER_PROP_TYPE.Float;
					_selectedMaterialSet._propertySets.Add(newProp);
				}
			}
			

			if(isNeedToEditorSave)
			{
				_editor.MaterialLibrary.Save();
			}

		}



		private string GetShaderPath(Shader shaderAsset)
		{
			if(shaderAsset == null)
			{
				return "";
			}
			return AssetDatabase.GetAssetPath(shaderAsset);
		}


		private Shader ShaderObjectField(string label, Shader srcShader, int width)
		{
			Shader nextShader = srcShader;
			
			SHADER_TYPE shaderType = SHADER_TYPE.None;

			Color prevColor = GUI.backgroundColor;
			if(srcShader == null)
			{
				GUI.backgroundColor = new Color(prevColor.r * 1.0f, prevColor.g * 0.7f, prevColor.b * 0.7f, 1.0f);
			}
			else
			{
				if(_shaderTypes.ContainsKey(srcShader))
				{
					shaderType = _shaderTypes[srcShader];
				}
				else
				{
					//Shader를 분석하자.
					Material testMaterial = new Material(srcShader);
					
					
					//VR 계산을 위한 프로퍼티를 가지고 있는가
					bool isVRProperty = testMaterial.HasProperty("_MaskTex_L") && testMaterial.HasProperty("_MaskTex_R");

					//LWRP, URP 지원인가
					bool isLWRP = testMaterial.GetTag("RenderPipeline", false, "").Contains("Lightweight");
					bool isURP = testMaterial.GetTag("RenderPipeline", false, "").Contains("UniversalPipeline");

					//추가 22.1.5 : 병합 가능 재질
					bool isMergeable = testMaterial.HasProperty("_MergedTex1")
										&& testMaterial.HasProperty("_MergedTex2")
										&& testMaterial.HasProperty("_MergedTex3")
										&& testMaterial.HasProperty("_MergedTex4")
										&& testMaterial.HasProperty("_MergedTex5")
										&& testMaterial.HasProperty("_MergedTex6")
										&& testMaterial.HasProperty("_MergedTex7")
										&& testMaterial.HasProperty("_MergedTex8")
										&& testMaterial.HasProperty("_MergedTex9");

					if(isMergeable)
					{
						shaderType = SHADER_TYPE.Mergeable;//추가 22.1.5
					}
					else if(isVRProperty)
					{
						shaderType = SHADER_TYPE.VR;
					}
					else if(isLWRP)
					{
						shaderType = SHADER_TYPE.LWRP;
					}
					else if(isURP)
					{
						//추가 20.1.24
						shaderType = SHADER_TYPE.URP;
					}
					else
					{
						shaderType = SHADER_TYPE.Default;
					}
					
					_shaderTypes.Add(srcShader, shaderType);

					UnityEngine.Object.DestroyImmediate(testMaterial);
				}
			}

			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
			GUILayout.Space(5);
				
			try
			{
				nextShader = (Shader)EditorGUILayout.ObjectField(label, srcShader, typeof(Shader), false, GUILayout.Width(width - 35));
			}
			catch(Exception/* ex*/)
			{
				//Debug.LogError("Shader GUI Exception : " + ex);				
			}

			GUILayout.Space(5);
			if(srcShader != null)
			{
				if (shaderType == SHADER_TYPE.LWRP)
				{
					GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
					guiStyle_Icon.margin = new RectOffset();
					//LWRP인 경우 아이콘을 출력하자
					EditorGUILayout.LabelField(new GUIContent(_img_LWRP), guiStyle_Icon, GUILayout.Width(20), GUILayout.Height(16));
				}
				else if (shaderType == SHADER_TYPE.VR)
				{
					GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
					guiStyle_Icon.margin = new RectOffset();
					//VR인 경우 아이콘을 출력하자
					EditorGUILayout.LabelField(new GUIContent(_img_VR), guiStyle_Icon, GUILayout.Width(20), GUILayout.Height(16));
				}
				else if (shaderType == SHADER_TYPE.URP)
				{
					//추가 20.1.24
					GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
					guiStyle_Icon.margin = new RectOffset();
					//VR인 경우 아이콘을 출력하자
					EditorGUILayout.LabelField(new GUIContent(_img_URP), guiStyle_Icon, GUILayout.Width(20), GUILayout.Height(16));
				}
				else if(shaderType == SHADER_TYPE.Mergeable)
				{
					//추가 22.1.5
					GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
					guiStyle_Icon.margin = new RectOffset();
					//Mergeable인 경우 아이콘을 출력하자
					EditorGUILayout.LabelField(new GUIContent(_img_Merge), guiStyle_Icon, GUILayout.Width(20), GUILayout.Height(16));
				}
			}


			EditorGUILayout.EndHorizontal();
			

			GUI.backgroundColor = prevColor;
			return nextShader;
		}


		private void OnGUI_RightLower(int width, int height)
		{
			if(_selectedMaterialSet == null)
			{
				return;
			}

			int height_Line = 24;
			int height_Btn = 22;
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));

			GUILayout.Space(5);

			if(_isPreset)
			{
				//프리셋인 경우
				//<Reserved 인 경우>
				//1. 복제

				//<Reserved가 아닌 경우>
				//1. 복제
				//2. 삭제

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Line));
				GUILayout.Space(5);
				
				if(GUILayout.Button(_editor.GetUIWord(UIWORD.Duplicate), GUILayout.Width(width - 27), GUILayout.Height(height_Btn)))
				{
					//프리셋 복제
					_duplicateMatSet = _selectedMaterialSet;
				}
				EditorGUILayout.EndHorizontal();

				if (!_selectedMaterialSet._isReserved)
				{
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Line));
					GUILayout.Space(5);
					if (GUILayout.Button(_editor.GetText(TEXT.Remove), GUILayout.Width(width - 27), GUILayout.Height(height_Btn)))
					{
						//프리셋 삭제
						//"Remove Preset",
						//"Do you want to remove the Preset [" + _selectedMaterialSet._name + "]?\n(This action cannot be undone.)"
						bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_RemoveMatPreset_Title),
																_editor.GetTextFormat(TEXT.DLG_RemoveMatPreset_Body, _selectedMaterialSet._name),
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));
						if(result)
						{
							//삭제 요청하자
							_removeMatSet = _selectedMaterialSet;
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				//프리셋이 아닌 경우
				//1. 연결된 프리셋과 Select, Restore
				//2. 복제, 삭제
				apMaterialSet linkedPreset = null;
				if(_selectedMaterialSet._linkedPresetID >= 0)
				{
					linkedPreset = _selectedMaterialSet._linkedPresetMaterial;;
				}
				
				
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Line));
				GUILayout.Space(5);

				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				guiStyle_Box.margin = GUI.skin.button.margin;

				Color prevColor = GUI.backgroundColor;
				if(linkedPreset != null)
				{
					//푸른색
					GUI.backgroundColor = new Color(GUI.backgroundColor.r * 0.8f, GUI.backgroundColor.g * 1.0f, GUI.backgroundColor.b * 1.0f, 1.0f);

					GUILayout.Box(new GUIContent(" " + linkedPreset._name, _img_MatTypeIcons[linkedPreset._icon]), guiStyle_Box, GUILayout.Width(width - 255), GUILayout.Height(height_Btn));
				}
				else
				{
					//붉은색
					GUI.backgroundColor = new Color(GUI.backgroundColor.r * 1.0f, GUI.backgroundColor.g * 0.7f, GUI.backgroundColor.b * 0.7f, 1.0f);

					GUILayout.Box("No Preset", guiStyle_Box, GUILayout.Width(width - 255), GUILayout.Height(height_Btn));
				}

				GUI.backgroundColor = prevColor;

				GUILayout.Space(20);

				if(GUILayout.Button(_editor.GetUIWord(UIWORD.Change), GUILayout.Width(100), GUILayout.Height(height_Btn)))
				{
					_loadKey_MakeNewMaterialSet = null;
					//"Select a preset to link"
					_loadKey_LinkPreset = apDialog_SelectMaterialSet.ShowDialog(_editor, true, _editor.GetText(TEXT.SelectMatSetTitle_ToLink), true, OnSelectMaterialSet_Link, _selectedMaterialSet);
				}

				//"Restore"
				string strRestore = _editor.GetText(TEXT.Restore);

				if(apEditorUtil.ToggledButton_2Side(strRestore, strRestore, false, linkedPreset != null, 100, height_Btn))
				{
					if (linkedPreset != null)
					{
						//연결된 Preset의 설정으로 복구할지 여부 묻기
						//"Restore",
						//"Do you want to restore all properties to be the same as the linked preset?"
						bool result = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_RestoreMatSetProps_Title),
																	_editor.GetText(TEXT.DLG_RestoreMatSetProps_Body),
																	_editor.GetText(TEXT.Okay),
																	_editor.GetText(TEXT.Cancel));
						if (result)
						{
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.MaterialSetChanged, 
																_editor, 
																_portrait, 
																//_portrait, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							//값 복사
							//겹치는 PropertySet 중에서 "Texture per Image"인 Texture 속성이 있는 경우
							//이 값은 없애지 말자
							_editor.Controller.RestoreMaterialSetToPreset(_selectedMaterialSet);
							Repaint();
						}
					}
				}
				
				EditorGUILayout.EndHorizontal();

				int width_Half = ((width - 10) / 2) - 10;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Line));
				GUILayout.Space(5);
				if(GUILayout.Button(_editor.GetUIWord(UIWORD.Duplicate), GUILayout.Width(width_Half), GUILayout.Height(height_Btn)))
				{
					//Material Set 복제
					_duplicateMatSet = _selectedMaterialSet;
				}
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_RegistToPreset), GUILayout.Width(width_Half), GUILayout.Height(height_Btn)))
				{
					//일반 Material Set > Preset으로 복제

					//추가 22.7.11
					//Shader-Path 링크부터
					


					_editor.MaterialLibrary.AddNewPreset(_selectedMaterialSet, false, _selectedMaterialSet._name);
					Repaint();
				}
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Line));
				GUILayout.Space(5);
				if (GUILayout.Button(_editor.GetText(TEXT.Remove), GUILayout.Width(width - 27), GUILayout.Height(height_Btn)))
				{
					//"Remove Material Set",
					//"Do you want to remove the Material Set [" + _selectedMaterialSet._name + "]?"
					bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_RemoveMatSet_Title),
																_editor.GetTextFormat(TEXT.DLG_RemoveMatSet_Body, _selectedMaterialSet._name),
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));
					if(result)
					{
						//삭제 요청하자
						_removeMatSet = _selectedMaterialSet;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}


		/// <summary>
		/// Reserved 프리셋을 편집하는 경우 보여주는 메시지
		/// </summary>
		private void ShowWarningIfReservedPresetChanged()
		{
			//"Reserved Material Set", "This is a Material that can not be modified."
			EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_ReservedMatSetWarning_Title), 
										_editor.GetText(TEXT.DLG_ReservedMatSetWarning_Body),
										_editor.GetText(TEXT.Okay));
		}


		// 텍스트 길이
		//----------------------------------------------------------------------------------
		private string GetClippedText(string srcText, int length)
		{
			if(srcText.Length > length)
			{
				return srcText.Substring(0, length) + "..";
			}

			return srcText;
		}


		//속성 비교
		//----------------------------------------------------------------------------------

		


		// Event
		//----------------------------------------------------------------------------------
		private void OnSelectMaterialSet_MakeMaterialSet(bool isSuccess, object loadKey, apMaterialSet resultMaterialSet, bool isNoneSelected, object savedObject)
		{
			if(!isSuccess || resultMaterialSet == null || _loadKey_MakeNewMaterialSet != loadKey)
			{
				//처리 실패
				_loadKey_MakeNewMaterialSet = null;
				return;
			}

			_loadKey_MakeNewMaterialSet = null;

			if(_editor == null)
			{
				return;
			}

			apMaterialSet newMatSet = _editor.Controller.AddMaterialSet((isNoneSelected ? null : resultMaterialSet), true, false);

			//추가 22.1.9 : 생성된 재질 세트를 바로 선택
			if(newMatSet != null)
			{
				_selectedMaterialSet = newMatSet;
				_isPreset = false;

				_scroll_RightUpper = Vector2.zero;//<<스크롤 리셋
				apEditorUtil.ReleaseGUIFocus();

				//Debug.Log("생성된 MatSet 선택");
			}
			Repaint();
		}


		

		private void OnSelectMaterialSet_Link(bool isSuccess, object loadKey, apMaterialSet resultMaterialSet, bool isNoneSelected, object savedObject)
		{
			if(!isSuccess || resultMaterialSet == null || _loadKey_LinkPreset != loadKey)
			{
				_loadKey_LinkPreset = null;
				return;
			}
			_loadKey_LinkPreset = null;

			if(savedObject == null)
			{
				return;
			}

			apMaterialSet savedMatSet = savedObject as apMaterialSet;
			if(_selectedMaterialSet != savedMatSet || _isPreset)
			{
				return;
			}
			
			//프리셋과 연결을 하자
			if(isNoneSelected)
			{
				_selectedMaterialSet._linkedPresetID = -1;
				_selectedMaterialSet._linkedPresetMaterial = null;
			}
			else
			{
				_selectedMaterialSet._linkedPresetID = resultMaterialSet._uniqueID;
				_selectedMaterialSet._linkedPresetMaterial = resultMaterialSet;
			}

			_editor.Controller.LinkMaterialSetAssets(_selectedMaterialSet, false, _portrait);


			if (_selectedMaterialSet._linkedPresetMaterial != null)
			{
				//설정 동일화
				//"Set values"
				//"Do you want to set properties to be the same as the preset?"
				bool result = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_SetValueToPresetMatSet_Title),
															_editor.GetText(TEXT.DLG_SetValueToPresetMatSet_Body),
															_editor.GetText(TEXT.Okay),
															_editor.GetText(TEXT.Cancel));

				if(result)
				{
					_editor.Controller.RestoreMaterialSetToPreset(_selectedMaterialSet);
				}
			}

			Repaint();
		}
	}
}