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
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
//using UnityEngine.Profiling;

#if UNITY_2017_1_OR_NEWER
using UnityEngine.Timeline;
using UnityEngine.Playables;
#endif

using AnyPortrait;

namespace AnyPortrait
{

	[CustomEditor(typeof(apPortrait))]
	public class apInspector_Portrait : Editor
	{
		private apPortrait _targetPortrait = null;
		private apControlParam.CATEGORY _curControlCategory = apControlParam.CATEGORY.Etc;
		private bool _showBaseInspector = false;
		private List<apControlParam> _controlParams = null;

		//private bool _isFold_BasicSettings = false;
		private bool _isFold_RootPortraits = false;
		private bool _isFold_AnimationClips = false;
		private bool _isFold_AnimationEvents = false;
		private bool _isFold_Sockets = false;
		private bool _isFold_Images = false;

		//추가 3.4
#if UNITY_2017_1_OR_NEWER
		private bool _isFold_Timeline = false;
		private int _nTimelineTrackSet = 0;
#endif
		//private bool _isFold_ConrolParameters = false;

		//3.7 추가 : 이미지들
		private bool _isImageLoaded = false;
		private Texture2D _img_EditorIsOpen = null;

		private Texture2D _img_OpenEditor = null;
		private Texture2D _img_QuickBake = null;
		private Texture2D _img_RefreshMeshes = null;
		private Texture2D _img_PrefabAsset = null;

		private Texture2D _img_BasicSettings = null;
		private Texture2D _img_Prefab = null;
		private Texture2D _img_RootPortraits = null;
		private Texture2D _img_AnimationSettings = null;
		private Texture2D _img_Mecanim = null;
#if UNITY_2017_1_OR_NEWER
		private Texture2D _img_Timeline = null;
#endif
		private Texture2D _img_ControlParams = null;
		private Texture2D _img_Objects = null;

		//추가 21.8.2 : 애니메이션 이벤트도 Inspector에 표시하자
		private Texture2D _img_AnimEvents = null;
		private Texture2D _img_AnimEvents_UnityVer = null;
		private Texture2D _img_Copy = null;
		//private Texture2D _img_CopyAll = null;
		private Texture2D _img_Sockets = null;

		private Texture2D _img_AddButton = null;
		private Texture2D _img_RemoveButton = null;

		private Texture2D _img_AnimPreview_Play = null;
		private Texture2D _img_AnimPreview_Stop = null;
		private Texture2D _img_AnimAutoPlay = null;

		private Texture2D _img_Images = null;

		private GUIContent _guiContent_EditorIsOpen = null;
		private GUIContent _guiContent_OpenEditor = null;
		private GUIContent _guiContent_QuickBake = null;
		private GUIContent _guiContent_RefreshMeshes = null;
		private GUIContent _guiContent_PrefabAsset = null;

		private GUIContent _guiContent_BasicSettings = null;
		private GUIContent _guiContent_Prefab = null;
		private GUIContent _guiContent_RootPortraits = null;
		private GUIContent _guiContent_AnimationSettings = null;

		private GUIContent _guiContent_Mecanim = null;
#if UNITY_2017_1_OR_NEWER
		private GUIContent _guiContent_Timeline = null;
#endif

		private GUIContent _guiContent_ControlParams = null;

		private GUIContent _guiContent_Images = null;

		private GUIContent _guiContent_AnimEvents = null;
		private GUIContent _guiContent_AnimEvents_UnityVer = null;
		private GUIContent _guiContent_Copy = null;
		//private GUIContent _guiContent_CopyAll = null;
		private GUIContent _guiContent_Sockets = null;

		private GUIContent _guiContent_AddButton = null;
		private GUIContent _guiContent_RemoveButton = null;

		private GUIContent _guiContent_AnimAutoPlay = null;


		private GUIStyle _guiStyle_buttonIcon = null;
		private GUIStyle _guiStyle_subTitle_Msg = null;
		private GUIStyle _guiStyle_subTitle_Title = null;

		private GUIStyle _guiStyle_subBox = null;
		private GUIStyle _guiStyle_button_NoMargin = null;
		private GUIStyle _guiStyle_button_Margin6 = null;
		private GUIStyle _guiStyle_WhiteText = null;
		private GUIStyle _guiStyle_Label_Middle = null;
		private GUIStyle _guiStyle_TextBox_Margin4 = null;

		private GUIContent _guiContent_Category = null;


		//추가 20.4.21 : 이미지 경로
		private string _basePath = "";


		//추가 20.9.14 : 대상 Portrait의 프리팹 여부
		private bool _isPrefabAsset = false;
		private bool _isPrefabInstance = false;
		private apEditorUtil.PREFAB_STATUS _prefabStatus = apEditorUtil.PREFAB_STATUS.NoPrefab;
		private UnityEngine.Object _srcPrefabObject = null;
		private GameObject _rootGameObjAsPrefabInstance = null;

#if UNITY_2021_1_OR_NEWER
		//추가 v1.4.2 : 유니티 2021.1부터 지원되는 값들
		private bool _isPrefabEditingScreen = false;//프리팹 편집 화면(Isolation Mode 등)인가.
#endif

		//추가 21.8.21 : 소켓 정보들
		private enum SOCKET_TYPE
		{
			Transform,
			Bone
		}
		private class SocketInfo
		{
			public string _name = null;
			public Transform _targetTransform = null;
			public SOCKET_TYPE _type = SOCKET_TYPE.Transform;

			public SocketInfo(apOptTransform socketParentTransform)
			{
				_name = socketParentTransform._name;
				_targetTransform = socketParentTransform._socketTransform;
				_type = SOCKET_TYPE.Transform;
			}

			public SocketInfo(apOptBone socketParentBone)
			{
				_name = socketParentBone._name;
				_targetTransform = socketParentBone._socketTransform;
				_type = SOCKET_TYPE.Bone;
			}
		}

		private List<SocketInfo> _socketInfos = new List<SocketInfo>();


		private int _nTotalAnimEvents = 0;
		private List<AnimEventInfo> _animEventInfos = null;


		// 변경 22.6.15 : 화면을 탭으로 구분한다.
		//다른 Portrait를 눌러도 화면을 유지하도록 EditorPref에 저장하자
		private enum TAB : int
		{
			Tab_1_BasicSettings = 0,//기본 설정들
			Tab_2_Components = 1,//루트 유닛 / 텍스쳐 (추가) / 소켓들
			Tab_3_Animations = 2,//애니메이션 설정과 클립들 (프리뷰 가능) / 타임라인 설정 포함
			Tab_4_AnimEvents = 3,//애니메이션 이벤트
			Tab_5_ControlParams = 4,//컨트롤 파라미터
		}
		private TAB _tab = TAB.Tab_1_BasicSettings;
		private const string PREF_KEY_TAB = "AnyPortrait_Inspector_Tab";



		//탭 GUIContent.
		//길이에 따라서 아이콘만 있거나, 텍스트 + 아이콘이 모두 있는 방식으로 구성된다.
		private GUIContent[] _guiContents_Tabs_IconOnly = null;
		private GUIContent[] _guiContents_Tabs_IconAndTexts = null;


		//애니메이션 프리뷰
		private apAnimClip _previewAnimClip = null;
		private int _previewFrame = 0;


		//애니메이션 이벤트 리스너 할당
		private MonoBehaviour _targetBatchMonoListener = null;


		void OnEnable()
		{
			_targetPortrait = null;

			//_isFold_BasicSettings = true;
			_isFold_RootPortraits = true;
			_isFold_AnimationClips = true;
			//_isFold_ConrolParameters = true;

			_isFold_AnimationEvents = true;
			_isFold_Sockets = true;
			_isFold_Images = true;

			

			//추가 3.4
#if UNITY_2017_1_OR_NEWER
			_isFold_Timeline = true;//<<
			_nTimelineTrackSet = 0;
#endif

			//Debug.Log("OnEnable - Inspector");
			_tab = (TAB)EditorPrefs.GetInt(PREF_KEY_TAB, (int)TAB.Tab_1_BasicSettings);
		}




		private void LoadImages()
		{

			if (_isImageLoaded)
			{
				return;
			}

			//이전
			//_basePath = apPathSetting.I.Load();

			//변경 21.10.4 : 함수 변경
			_basePath = apPathSetting.I.RefreshAndGetBasePath(true);//강제로 로드후 갱신

			_img_EditorIsOpen = LoadImage("InspectorIcon_EditorIsOpen");

			_img_OpenEditor = LoadImage("InspectorIcon_OpenEditor");
			_img_QuickBake = LoadImage("InspectorIcon_QuickBake");
			_img_RefreshMeshes = LoadImage("InspectorIcon_RefreshMeshes");
			_img_PrefabAsset = LoadImage("InspectorIcon_PrefabAsset");

			_img_BasicSettings = LoadImage("InspectorIcon_BasicSettings");
			_img_Prefab = LoadImage("InspectorIcon_Prefab");
			_img_RootPortraits = LoadImage("InspectorIcon_RootPortraits");
			_img_AnimationSettings = LoadImage("InspectorIcon_AnimationSettings");
			_img_Mecanim = LoadImage("InspectorIcon_Mecanim");
#if UNITY_2017_1_OR_NEWER
			_img_Timeline = LoadImage("InspectorIcon_Timeline");
#endif
			_img_ControlParams = LoadImage("InspectorIcon_ControlParams");
			_img_Objects = LoadImage("InspectorIcon_Objects");

			_img_AnimEvents = LoadImage("InspectorIcon_AnimEvent");
			_img_AnimEvents_UnityVer = LoadImage("InspectorIcon_AnimEvent_UnityVer");
			_img_Copy = LoadImage("InspectorIcon_Copy");
			//_img_CopyAll = LoadImage("InspectorIcon_CopyAll");
			_img_Sockets = LoadImage("InspectorIcon_Sockets");

			_img_AddButton = LoadImage("InspectorIcon_AddButton");
			_img_RemoveButton = LoadImage("InspectorIcon_RemoveButton");

			_img_AnimPreview_Play = LoadImage("InspectorIcon_AnimPreview_Play");
			_img_AnimPreview_Stop = LoadImage("InspectorIcon_AnimPreview_Stop");
			_img_AnimAutoPlay = LoadImage("InspectorIcon_AnimAutoPlay");

			_img_Images = LoadImage("InspectorIcon_Image");




			_guiContent_EditorIsOpen = new GUIContent("  Editor is opened", _img_EditorIsOpen);
			_guiContent_OpenEditor = new GUIContent(_img_OpenEditor);
			_guiContent_QuickBake = new GUIContent(_img_QuickBake);
			_guiContent_RefreshMeshes = new GUIContent(_img_RefreshMeshes);
			_guiContent_PrefabAsset = new GUIContent("  Prefab Asset is selected", _img_PrefabAsset);

			_guiContent_BasicSettings = new GUIContent("  General Properties", _img_BasicSettings);
			_guiContent_Prefab = new GUIContent("  Prefab", _img_Prefab);
			_guiContent_RootPortraits = new GUIContent("  Root Portraits", _img_RootPortraits);
			_guiContent_Images = new GUIContent("  Images", _img_Images);
			_guiContent_AnimationSettings = new GUIContent("  Animation Settings", _img_AnimationSettings);

			_guiContent_Mecanim = new GUIContent("  Mecanim Settings", _img_Mecanim);
#if UNITY_2017_1_OR_NEWER
			_guiContent_Timeline = new GUIContent("  Timeline Settings", _img_Timeline);
#endif

			_guiContent_ControlParams = new GUIContent("  Control Parameters", _img_ControlParams);
			_guiContent_AnimEvents = new GUIContent("  Animation Events", _img_AnimEvents);
			_guiContent_AnimEvents_UnityVer = new GUIContent("  Animation Events", _img_AnimEvents_UnityVer);
			_guiContent_Copy = new GUIContent(_img_Copy);
			//_guiContent_CopyAll = new GUIContent(_img_CopyAll);
			_guiContent_Sockets = new GUIContent("  Sockets", _img_Sockets);

			_guiContent_AddButton = new GUIContent(_img_AddButton);
			_guiContent_RemoveButton = new GUIContent(_img_RemoveButton);

			_guiContent_AnimAutoPlay = new GUIContent(_img_AnimAutoPlay);


			_guiContent_Category = new GUIContent("Category");

			_guiStyle_buttonIcon = new GUIStyle(GUI.skin.label);
			_guiStyle_buttonIcon.alignment = TextAnchor.MiddleCenter;
			_guiStyle_buttonIcon.padding = new RectOffset(0, 0, 0, 0);


			_guiStyle_button_NoMargin = new GUIStyle(GUI.skin.button);
			_guiStyle_button_NoMargin.alignment = TextAnchor.MiddleCenter;
			_guiStyle_button_NoMargin.margin = new RectOffset(0, 0, 2, 2);
			_guiStyle_button_NoMargin.padding = new RectOffset(0, 0, 1, 1);

			_guiStyle_button_Margin6 = new GUIStyle(GUI.skin.button);
			_guiStyle_button_Margin6.alignment = TextAnchor.MiddleCenter;
			_guiStyle_button_Margin6.margin = new RectOffset(0, 0, 6, 6);
			_guiStyle_button_Margin6.padding = new RectOffset(0, 0, 1, 1);

			_guiStyle_subTitle_Msg = new GUIStyle(GUI.skin.box);
			_guiStyle_subTitle_Msg.alignment = TextAnchor.MiddleCenter;
			_guiStyle_subTitle_Msg.margin = new RectOffset(0, 0, 0, 0);
			_guiStyle_subTitle_Msg.padding = new RectOffset(0, 0, 0, 0);

			_guiStyle_subTitle_Title = new GUIStyle(GUI.skin.box);
			_guiStyle_subTitle_Title.alignment = TextAnchor.MiddleCenter;
			_guiStyle_subTitle_Title.margin = new RectOffset(0, 0, 0, 0);
			_guiStyle_subTitle_Title.padding = new RectOffset(0, 0, 0, 0);
			_guiStyle_subTitle_Title.normal.textColor = Color.white;



			_guiStyle_subBox = new GUIStyle(GUI.skin.box);
			_guiStyle_subBox.alignment = TextAnchor.MiddleCenter;
			_guiStyle_subBox.padding = new RectOffset(0, 0, 2, 2);

			_guiStyle_WhiteText = new GUIStyle(GUI.skin.label);
			_guiStyle_WhiteText.normal.textColor = Color.white;

			_guiStyle_Label_Middle = new GUIStyle(GUI.skin.label);
			_guiStyle_Label_Middle.alignment = TextAnchor.MiddleLeft;

			_guiStyle_TextBox_Margin4 = new GUIStyle(GUI.skin.textField);
			_guiStyle_TextBox_Margin4.margin = new RectOffset(	_guiStyle_TextBox_Margin4.margin.left,
																_guiStyle_TextBox_Margin4.margin.right,
																5, 5);
			_guiStyle_TextBox_Margin4.alignment = TextAnchor.MiddleLeft;

			//탭 GUI Content
			_guiContents_Tabs_IconOnly = new GUIContent[5];
			_guiContents_Tabs_IconAndTexts = new GUIContent[5];

			_guiContents_Tabs_IconOnly[0] = new GUIContent(_img_BasicSettings);
			_guiContents_Tabs_IconAndTexts[0] = new GUIContent(" General", _img_BasicSettings);

			_guiContents_Tabs_IconOnly[1] = new GUIContent(_img_Objects);
			_guiContents_Tabs_IconAndTexts[1] = new GUIContent(" Objects", _img_Objects);

			_guiContents_Tabs_IconOnly[2] = new GUIContent(_img_AnimationSettings);
			_guiContents_Tabs_IconAndTexts[2] = new GUIContent(" Animations", _img_AnimationSettings);

			_guiContents_Tabs_IconOnly[3] = new GUIContent(_img_AnimEvents);
			_guiContents_Tabs_IconAndTexts[3] = new GUIContent(" Events", _img_AnimEvents);

			_guiContents_Tabs_IconOnly[4] = new GUIContent(_img_ControlParams);
			_guiContents_Tabs_IconAndTexts[4] = new GUIContent(" Control", _img_ControlParams);


			_isImageLoaded = true;
		}

		public override void OnInspectorGUI()
		{
			//return;
			LoadImages();


			//base.OnInspectorGUI();
			apPortrait targetPortrait = target as apPortrait;

			if (targetPortrait != _targetPortrait)
			{
				_targetPortrait = targetPortrait;
				Init();

				FindSockets();
				RefreshAnimEventInfo();
			}


			if (_targetPortrait == null)
			{
				//Profiler.EndSample();
				return;
			}

			//Profiler.BeginSample("anyPortrait Inspector GUI");

			int width_Inspector = (int)EditorGUIUtility.currentViewWidth - 36;//제공되는 값보다 좀 많이 줄여야 한다.
			//int width_Inspector = (int)EditorGUIUtility.currentViewWidth - 46;//제공되는 값보다 좀 많이 줄여야 한다.
			Color prevColor = GUI.backgroundColor;

			


			//0. 에디터가 작동중일 때는 모든 메뉴가 안보인다.
			//return;
			if (apEditor.IsOpen())
			{
				//에디터가 작동중에는 안보이도록 하자
				//EditorGUILayout.LabelField("Editor is opened");
				GUILayout.Space(10);

				EditorGUILayout.LabelField(_guiContent_EditorIsOpen, GUILayout.Height(40));

				//Profiler.EndSample();

				return;
			}


			EditorGUILayout.BeginVertical(GUILayout.Width(width_Inspector));

			width_Inspector -= 10;

			try
			{
				bool request_OpenEditor = false;
				bool request_QuickBake = false;
				bool request_RefreshMeshes = false;
				
				
				//bool prevImportant = _targetPortrait._isImportant;
				//int prevSortingLayerID = _targetPortrait._sortingLayerID;
				//apPortrait.SORTING_ORDER_OPTION prevSortingOrderOption = _targetPortrait._sortingOrderOption;
				//int prevSortingOrder = _targetPortrait._sortingOrder;
				//int prevOrderPerDepth = _targetPortrait._sortingOrderPerDepth;//추가 21.1.31

				//1. 메시지와 에디터 열기 메뉴
				int height_MainMsg = 40;

				if (_isPrefabAsset)
				{
					//추가 20.9.15 : 만약 프리팹 에셋이라면 에디터를 열 수 없다.
					GUILayout.Space(10);

					EditorGUILayout.LabelField(_guiContent_PrefabAsset, GUILayout.Height(40));

					GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);

					GUILayout.Box("Prefab Assets cannot be edited.\nPlace the Prefab in the Scene as an Instance.",
									_guiStyle_subTitle_Msg,
									GUILayout.Width(width_Inspector),
									GUILayout.Height(height_MainMsg));

					GUI.backgroundColor = prevColor;
				}
#if UNITY_2021_1_OR_NEWER
				else if(_isPrefabEditingScreen)
				{
					//(Unity 2021에서)
					//현재 화면이 프리팹 편집 화면이라면 에디터를 열 수 없다.
					GUILayout.Space(10);

					EditorGUILayout.LabelField(_guiContent_PrefabAsset, GUILayout.Height(40));

					GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);

					GUILayout.Box("Editing is not possible in the prefab editing screen.\nPlace the instance in the normal scene.",
									_guiStyle_subTitle_Msg,
									GUILayout.Width(width_Inspector),
									GUILayout.Height(height_MainMsg));

					GUI.backgroundColor = prevColor;
				}
#endif
				else if (!EditorApplication.isPlaying)
				{
					//일반 메뉴 버튼 3개가 나온다.
					//단 플레이중이면 안된다.
					int iconWidth = 34;
					int iconHeight = 36;
					int buttonHeight = 36;
					//int buttonWidth = width_Inspector - (iconWidth + 4);

					//v1.1.7의 용량 최적화 기능이 수행되지 않았거나, 아직 Bake되지 않았다면
					if (!_targetPortrait._isSizeOptimizedV117)
					{
						GUILayout.Space(10);

						GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);

						GUILayout.Box("[Bake] was not executed.\nExecute the [Bake] again.",
										_guiStyle_subTitle_Msg,
										GUILayout.Width(width_Inspector), 
										GUILayout.Height(height_MainMsg));

						GUI.backgroundColor = prevColor;
					}

					if (!_targetPortrait._isOptimizedPortrait)
					{
						GUILayout.Space(10);

						EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(_guiContent_OpenEditor, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						if (GUILayout.Button("Open Editor", GUILayout.Height(buttonHeight)))
						{
							request_OpenEditor = true;
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(_guiContent_QuickBake, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						if (GUILayout.Button("Quick Bake", GUILayout.Height(buttonHeight)))
						{
							request_QuickBake = true;
						}
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						GUILayout.Space(10);

						EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(_guiContent_OpenEditor, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
						GUILayout.Space(5);
						if (GUILayout.Button("Open Editor (Not Editable)", GUILayout.Height(buttonHeight)))
						{
							//열기만 하고 선택은 못함
							request_OpenEditor = true;
						}
						EditorGUILayout.EndHorizontal();
					}

					//추가 12.18 : Mesh를 리프레시 하자

					EditorGUILayout.BeginHorizontal(GUILayout.Height(iconHeight));
					GUILayout.Space(5);
					EditorGUILayout.LabelField(_guiContent_RefreshMeshes, _guiStyle_buttonIcon, GUILayout.Width(iconWidth), GUILayout.Height(iconHeight));
					GUILayout.Space(5);
					if (GUILayout.Button("Refresh Meshes", GUILayout.Height(buttonHeight)))
					{
						request_RefreshMeshes = true;
					}
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(10);

				//추가 22.6.15 : 탭 메뉴
				int curTab = (int)_tab;
				int nextTab = curTab;
				if ((width_Inspector - 10) / 5 < 120)
				{
					nextTab = GUILayout.Toolbar(curTab, _guiContents_Tabs_IconOnly, GUILayout.Width(width_Inspector), GUILayout.Height(25));
				}
				else
				{
					nextTab = GUILayout.Toolbar(curTab, _guiContents_Tabs_IconAndTexts, GUILayout.Width(width_Inspector), GUILayout.Height(25));
				}

				if (nextTab != curTab)
				{
					_tab = (TAB)nextTab;
					GUI.FocusControl(null);

					//현재 탭을 저장하자
					EditorPrefs.SetInt(PREF_KEY_TAB, (int)_tab);
				}

				//탭에 따라 다른 화면을 보여주자
				GUILayout.Space(10);

				//1. 탭에 따른 제목 + UI 그리기

				//int subTitleWidth = width_Inspector - 44;
				//int subTitleWidth = width_Inspector;
				//int subTitleHeight = 26;


				bool isChanged = false;

				switch (_tab)
				{
					case TAB.Tab_1_BasicSettings:
						{
							if(GUI_Tab_1_BasicSettings(width_Inspector))
							{
								isChanged = true;
							}
						}
						break;

					case TAB.Tab_2_Components:
						{
							if(GUI_Tab_2_Components(width_Inspector))
							{
								isChanged = true;
							}
						}
						break;

					case TAB.Tab_3_Animations:
						{
							if(GUI_Tab_3_Animations(width_Inspector))
							{
								isChanged = true;
							}
						}
						break;

					case TAB.Tab_4_AnimEvents:
						{
							if(GUI_Tab_4_AnimEvents(width_Inspector))
							{
								isChanged = true;
							}
						}
						break;

					case TAB.Tab_5_ControlParams:
						{
							if(GUI_Tab_5_ControlParams(width_Inspector))
							{
								isChanged = true;
							}
						}
						break;
				}

				
				GUILayout.Space(30);

				//2. 토글 버튼을 두어서 기본 Inspector 출력 여부를 결정하자.
				string strBaseButton = "Show All Properties";
				if (_showBaseInspector)
				{
					strBaseButton = "Hide Properties";
				}

				if (GUILayout.Button(strBaseButton, GUILayout.Height(20)))
				{
					_showBaseInspector = !_showBaseInspector;
				}

				if (_showBaseInspector)
				{
					base.OnInspectorGUI();
				}

				
				if (!Application.isPlaying && isChanged)
				{
					//플레이 중이 아닐때 업데이트를 하자
					//플레이 중이라면 자동으로 업데이트 될 것이다.
					//Debug.Log("Inspector 플레이");
					
					//_targetPortrait.IgnoreAnimAutoPlayOption();

					if(_targetPortrait.InitializationStatus == apPortrait.INIT_STATUS.Ready)
					{
						//강제로 초기화
						_targetPortrait.Initialize();

						//자동시작되는 애니메이션이 있다면 종료시키기 위함
						_targetPortrait._animPlayManager.StopAll();
					}

					if(_previewAnimClip != null)
					{
						//애니메이션 프리뷰 요청
						apAnimPlayData animPlayData = _targetPortrait.GetAnimationPlayData(_previewAnimClip._name);

						if (animPlayData == null)
						{
							Debug.LogError("AnyPortrait : Animation cannot be played. Please run Bake again.");
							_previewAnimClip = null;
						}
						else
						{
							if (animPlayData.PlaybackStatus == apAnimPlayData.AnimationPlaybackStatus.Playing
							|| animPlayData.PlaybackStatus == apAnimPlayData.AnimationPlaybackStatus.Paused)
							{
								animPlayData._linkedAnimClip.SetFrame_Opt(_previewFrame, false);
							}
							else
							{

								_targetPortrait._animPlayManager.StopAll();
								_targetPortrait._animPlayManager.PlayAt(
									_previewAnimClip._name,
									_previewFrame,
									0,
									apAnimPlayUnit.BLEND_METHOD.Interpolation);
							}
						}
					}
					else
					{
						_targetPortrait._animPlayManager.StopAll();
					}

					_targetPortrait.UpdateForce();
				}

				if (_targetPortrait != null)
				{
					if (request_OpenEditor)
					{
						if (_targetPortrait._isOptimizedPortrait)
						{
							RequestDelayedOpenEditor(_targetPortrait, REQUEST_TYPE.Open);
						}
						else
						{
							RequestDelayedOpenEditor(_targetPortrait, REQUEST_TYPE.OpenAndSet);
						}
						//apEditor anyPortraitEditor = apEditor.ShowWindow();
						//if (anyPortraitEditor != null && !_targetPortrait._isOptimizedPortrait)
						//{
						//	anyPortraitEditor.SetPortraitByInspector(_targetPortrait, false);
						//}
					}
					else if (request_QuickBake)
					{
						RequestDelayedOpenEditor(_targetPortrait, REQUEST_TYPE.QuickBake);
						//apEditor anyPortraitEditor = apEditor.ShowWindow();
						//if (anyPortraitEditor != null)
						//{
						//	anyPortraitEditor.SetPortraitByInspector(_targetPortrait, true);

						//	Selection.activeObject = _targetPortrait.gameObject;
						//}
					}
					else if (request_RefreshMeshes)
					{
						_targetPortrait.OnMeshResetInEditor();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("apInspector_Portrait Exception");
				Debug.LogException(ex);
			}

			//Profiler.EndSample();

			EditorGUILayout.EndVertical();
		}



		private void Init()
		{
			_curControlCategory = apControlParam.CATEGORY.Head |
									apControlParam.CATEGORY.Body |
									apControlParam.CATEGORY.Face |
									apControlParam.CATEGORY.Hair |
									apControlParam.CATEGORY.Equipment |
									apControlParam.CATEGORY.Force |
									apControlParam.CATEGORY.Etc;

			_showBaseInspector = false;

			//_isFold_BasicSettings = true;
			//_isFold_BasicSettings = true;
			_isFold_RootPortraits = true;
			//_isFold_AnimationSettings = true;
			_isFold_AnimationClips = true;
			//_isFold_ConrolParameters = true;

			_isFold_AnimationEvents = true;
			_isFold_Sockets = true;
			_isFold_Images = true;

			_controlParams = null;
			if (_targetPortrait._controller != null)
			{
				_controlParams = _targetPortrait._controller._controlParams;
			}


			_requestPortrait = null;
			_requestType = REQUEST_TYPE.None;
			_coroutine = null;

#if UNITY_2017_1_OR_NEWER
			_nTimelineTrackSet = (_targetPortrait._timelineTrackSets == null) ? 0 :_targetPortrait._timelineTrackSets.Length;
#endif

			EditorApplication.update -= ExecuteCoroutine;


			if (_socketInfos == null)
			{
				_socketInfos = new List<SocketInfo>();
			}
			_socketInfos.Clear();

			_tab = (TAB)EditorPrefs.GetInt(PREF_KEY_TAB, (int)TAB.Tab_1_BasicSettings);

			_previewAnimClip = null;
			_previewFrame = 0;

			_targetBatchMonoListener = null;

			RefreshPrefabStatus();


		}



		// GUI
		//--------------------------------------------------------------------------------------

		//----------------------------------------------------
		// 1. 탭1 : 기본 설정 (Basic Settings)
		//----------------------------------------------------
		private bool GUI_Tab_1_BasicSettings(int width)
		{
			bool isChanged = false;

			Color prevColor = GUI.backgroundColor;

			//1. 기본 설정들
			DrawSubTitle(_guiContent_BasicSettings, width, new Color(0.0f, 0.8f, 1.0f));



			EditorGUI.BeginChangeCheck();
			
			bool next_isImportant = EditorGUILayout.Toggle("Is Important", _targetPortrait._isImportant);

			if(EditorGUI.EndChangeCheck())
			{
				if (next_isImportant != _targetPortrait._isImportant)
				{
					RecordUndo();
					_targetPortrait._isImportant = next_isImportant;
				}
			}

			GUILayout.Space(10);


			//2. Sorting Layer 설정
			string[] sortingLayerName = new string[SortingLayer.layers.Length];
			int layerIndex = -1;
			for (int i = 0; i < SortingLayer.layers.Length; i++)
			{
				sortingLayerName[i] = SortingLayer.layers[i].name;
				if (SortingLayer.layers[i].id == _targetPortrait._sortingLayerID)
				{
					layerIndex = i;
				}
			}

			EditorGUI.BeginChangeCheck();
			int nextLayerIndex = EditorGUILayout.Popup("Sorting Layer", layerIndex, sortingLayerName);
			apPortrait.SORTING_ORDER_OPTION nextSortingOption = (apPortrait.SORTING_ORDER_OPTION)EditorGUILayout.EnumPopup("Sorting Order Option", _targetPortrait._sortingOrderOption);
			if(EditorGUI.EndChangeCheck())
			{
				
				if (nextLayerIndex != layerIndex)
				{
					RecordUndo();

					//Sorting Layer를 바꾸자
					if (nextLayerIndex >= 0 && nextLayerIndex < SortingLayer.layers.Length)
					{
						string nextLayerName = SortingLayer.layers[nextLayerIndex].name;
						_targetPortrait.SetSortingLayer(nextLayerName);
					}
				}

				if (nextSortingOption != _targetPortrait._sortingOrderOption)
				{
					RecordUndo();

					//Sorting Order를 바꾸자
					_targetPortrait._sortingOrderOption = nextSortingOption;
					//변경된 Sorting Order Option에 따라서 바로 Sorting을 해야한다.
					_targetPortrait.ApplySortingOptionToOptRootUnits();

					switch (_targetPortrait._sortingOrderOption)
					{
						case apPortrait.SORTING_ORDER_OPTION.SetOrder:
							_targetPortrait.SetSortingOrder(_targetPortrait._sortingOrder);
							break;

						case apPortrait.SORTING_ORDER_OPTION.DepthToOrder:
						case apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder:
							_targetPortrait.SetSortingOrderChangedAutomatically(true);
							_targetPortrait.RefreshSortingOrderByDepth();
							break;
					}
				}
			}


			if (_targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
			{
				EditorGUI.BeginChangeCheck();

				int nextLayerOrder = EditorGUILayout.DelayedIntField("Sorting Order", _targetPortrait._sortingOrder);

				if (EditorGUI.EndChangeCheck())
				{
					if (nextLayerOrder != _targetPortrait._sortingOrder)
					{
						RecordUndo();

						_targetPortrait.SetSortingOrder(nextLayerOrder);
					}

					GUI.FocusControl(null);
				}
			}
			else if (_targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder
				|| _targetPortrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
			{
				EditorGUI.BeginChangeCheck();

				//추가 21.1.31 : Depth To Order일때, 1씩만 증가하는게 아닌 더 큰값으로 증가할 수도 있게 만들자
				int nextOrderPerDepth = EditorGUILayout.DelayedIntField("Order Per Depth", _targetPortrait._sortingOrderPerDepth);

				if (EditorGUI.EndChangeCheck())
				{
					if (nextOrderPerDepth != _targetPortrait._sortingOrderPerDepth)
					{
						RecordUndo();

						if (nextOrderPerDepth < 1)
						{
							nextOrderPerDepth = 1;
						}

						_targetPortrait._sortingOrderPerDepth = nextOrderPerDepth;

						//변경된 Sorting Order Option에 따라서 바로 Sorting을 해야한다.
						_targetPortrait.ApplySortingOptionToOptRootUnits();
					}

					GUI.FocusControl(null);
				}
			}
			GUILayout.Space(10);




			//3. 빌보드

			EditorGUI.BeginChangeCheck();

			apPortrait.BILLBOARD_TYPE nextBillboard = (apPortrait.BILLBOARD_TYPE)EditorGUILayout.EnumPopup("Billboard Type", _targetPortrait._billboardType);

			if (EditorGUI.EndChangeCheck())
			{
				if (nextBillboard != _targetPortrait._billboardType)
				{
					RecordUndo();

					_targetPortrait._billboardType = nextBillboard;
				}
			}

			GUILayout.Space(20);

			//4. 프리팹 (선택)
			if (_isPrefabInstance)
			{
				DrawSubTitle(_guiContent_Prefab, width, new Color(0.0f, 0.5f, 1.0f));

				//연결 상태를 보여주자
				string strStatus = null;
				switch (_prefabStatus)
				{
					case apEditorUtil.PREFAB_STATUS.Connected:
						GUI.backgroundColor = new Color(0.7f, 1.0f, 1.0f, 1.0f);
						strStatus = "Source Prefab";
						break;

					case apEditorUtil.PREFAB_STATUS.Disconnected:
						GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
						strStatus = "(Disconnected)";
						break;

					case apEditorUtil.PREFAB_STATUS.Asset://v1.4.2
						GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
						strStatus = "(Not Instance)";
						break;

					case apEditorUtil.PREFAB_STATUS.Missing:
					default:
						GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
						strStatus = "(Missing)";
						break;
				}
				EditorGUILayout.ObjectField(strStatus, _srcPrefabObject, typeof(UnityEngine.Object), false);
				EditorGUILayout.ObjectField("Root GameObject", _rootGameObjAsPrefabInstance, typeof(GameObject), false);
				GUI.backgroundColor = prevColor;

				int width_PrefabButtons = (width / 2) - 4;

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(5);
				if (GUILayout.Button("Apply", GUILayout.Width(width_PrefabButtons)))
				{
					//프리팹 변경 내용을 저장하자
					apEditorUtil.ApplyPrefab(_targetPortrait);
					RefreshPrefabStatus();
				}
				if (GUILayout.Button("Refresh", GUILayout.Width(width_PrefabButtons)))
				{
					//프리팹 연결 정보를 갱신한다.
					RefreshPrefabStatus();
				}


				EditorGUILayout.EndHorizontal();

				//Disconnect를 할 수 있다.
				//Legacy : 단순 Disconnect를 할 수 있다.
				//2018.3 : Disconnect를 한 후, 복원 정보를 모두 삭제할 수 있다.
#if UNITY_2018_3_OR_NEWER
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(5);
					
				if (GUILayout.Button("Disconnect", GUILayout.Width(width_PrefabButtons)))
				{
					//Disconnect를 하되, 연결 정보는 남겨둔다.
					if(EditorUtility.DisplayDialog(
											"Disconnecting from Prefab", 
											"Are you sure you want to disconnect this Portrait from the Prefab Asset?", 
											"Disconnect", "Cancel"))
					{
						apEditorUtil.CheckAndRefreshPrefabInfo(_targetPortrait);
						apEditorUtil.DisconnectPrefab(_targetPortrait);
						RefreshPrefabStatus();
					}
						
				}

				if (GUILayout.Button("Clear", GUILayout.Width(width_PrefabButtons)))
				{
					//Disconnect를 하고, 연결 정보를 삭제한다.
					if(EditorUtility.DisplayDialog(
											"Disconnecting from Prefab", 
											"Are you sure you want to disconnect this Portrait from the Prefab Asset?\nThis completely deletes the connection data with the Prefab.", 
											"Disconnect and Clear", "Cancel"))
					{
						apEditorUtil.DisconnectPrefab(_targetPortrait, true);
						RefreshPrefabStatus();
					}
				}
				EditorGUILayout.EndHorizontal();
#else
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(5);
				
				if (GUILayout.Button("Disconnect", GUILayout.Width(width - 4)))
				{
					//Disconnect를 하되, 연결 정보는 남겨둔다.
					if (EditorUtility.DisplayDialog(
											"Disconnecting from Prefab",
											"Are you sure you want to disconnect this Portrait from the Prefab Asset?",
											"Disconnect", "Cancel"))
					{
						apEditorUtil.CheckAndRefreshPrefabInfo(_targetPortrait);
						apEditorUtil.DisconnectPrefab(_targetPortrait);
						RefreshPrefabStatus();
					}
				}
				EditorGUILayout.EndHorizontal();
#endif

				GUILayout.Space(20);
			}
			


			
			return isChanged;
		}





		//----------------------------------------------------
		// 2. 컴포넌트 (루트 유닛, 텍스쳐, 소켓 등)
		//----------------------------------------------------
		private bool GUI_Tab_2_Components(int width)
		{
			bool isChanged = false;


			//1. Root Units
			DrawSubTitle(_guiContent_RootPortraits, width, new Color(0.0f, 1.0f, 0.5f));

			_isFold_RootPortraits = EditorGUILayout.Foldout(_isFold_RootPortraits, "Root Units");
			if (_isFold_RootPortraits)
			{
				int width_Label = 50;
				int width_ObjectField = width - (width_Label + 2);

				int nRootUnits = _targetPortrait._optRootUnitList != null ? _targetPortrait._optRootUnitList.Count : 0;

				if (nRootUnits > 0)
				{
					for (int i = 0; i < nRootUnits; i++)
					{
						apOptRootUnit rootUnit = _targetPortrait._optRootUnitList[i];

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
						
						EditorGUILayout.LabelField("[" + i + "]", GUILayout.Width(width_Label));
						EditorGUILayout.ObjectField(rootUnit, typeof(apOptRootUnit), true, GUILayout.Width(width_ObjectField));

						EditorGUILayout.EndHorizontal();
					}
				}
			}

			GUILayout.Space(20);


			//2. 이미지들
			DrawSubTitle(_guiContent_Images, width, new Color(1.0f, 0.5f, 0.0f));


			_isFold_Images = EditorGUILayout.Foldout(_isFold_Images, "Images");
			if(_isFold_Images)
			{
				int width_Label = 50;
				int width_TextureName = (int)((float)width * 0.4f);
				int width_ObjectField = width - (width_Label + width_TextureName + 6);

				int nImages = _targetPortrait._optTextureData != null ? _targetPortrait._optTextureData.Count : 0;
				if(nImages > 0)
				{
					apOptTextureData curImg = null;
					for (int i = 0; i < nImages; i++)
					{
						curImg = _targetPortrait._optTextureData[i];

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
						
						EditorGUILayout.LabelField("[" + i + "]", GUILayout.Width(width_Label));
						EditorGUILayout.TextField(curImg._name, GUILayout.Width(width_TextureName));
						EditorGUILayout.ObjectField(curImg._texture, typeof(Texture2D), false, GUILayout.Width(width_ObjectField));

						EditorGUILayout.EndHorizontal();
					}

					GUILayout.Space(5);
					if (GUILayout.Button("Copy Names of Images to Clipboard"))
					{
						//이미지 이름 복사하기
						CopyImageNames();
					}
				}
			}


			GUILayout.Space(20);

			//추가 21.8.21 : 소켓이 있다면 정보들을 출력한다.
			// Sockets (선택)
			
			DrawSubTitle(_guiContent_Sockets, width, new Color(1.0f, 1.0f, 0.3f));
			_isFold_Sockets = EditorGUILayout.Foldout(_isFold_Sockets, "Sockets");
			if (_isFold_Sockets)
			{
				if (_socketInfos != null && _socketInfos.Count > 0)
				{
					//GUILayout.Box(_guiContent_Sockets, _guiStyle_subTitle, GUILayout.Width(subTitleWidth), GUILayout.Height(subTitleHeight));
					int nSockets = _socketInfos.Count;
					SocketInfo curInfo = null;
					for (int iInfo = 0; iInfo < nSockets; iInfo++)
					{
						curInfo = _socketInfos[iInfo];

						//이름 / Transform / 이름 복사

						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(5);

						EditorGUILayout.EnumPopup(curInfo._type, GUILayout.Width(80));

						EditorGUILayout.TextField(curInfo._name, GUILayout.Width(width - (5 + 120 + 6 + 80)));

						EditorGUILayout.ObjectField(curInfo._targetTransform, typeof(Transform), true, GUILayout.Width(120));

						EditorGUILayout.EndHorizontal();
					}
				}
				GUILayout.Space(5);
				if(GUILayout.Button("Refresh Sockets", GUILayout.Height(25)))
				{
					FindSockets();
				}
				
				if (GUILayout.Button("Copy Names of Sockets to Clipboard"))
				{
					//소켓 이름 복사하기
					CopySocketNames(_socketInfos);
				}

			}

			GUILayout.Space(20);

			//TODO : 텍스쳐들




			GUILayout.Space(20);

			return isChanged;

		}


		//----------------------------------------------------
		// 3. 애니메이션 (프리뷰, 메카님, 타임라인 등)
		//----------------------------------------------------
		private bool GUI_Tab_3_Animations(int width)
		{

			bool isChanged = false;

			//애니메이션 설정들
			DrawSubTitle(_guiContent_AnimationSettings, width, new Color(1.0f, 0.0f, 0.8f));

			_isFold_AnimationClips = EditorGUILayout.Foldout(_isFold_AnimationClips, "Animation Clips");

			int nAnimClips = _targetPortrait._animClips != null ? _targetPortrait._animClips.Count : 0;

			if (_isFold_AnimationClips)
			{
				if (nAnimClips > 0)
				{
					//보여줘야 하는 정보
					//- 애니메이션 이름
					//- 자동 실행 여부
					//- 프리뷰 활성 버튼 / 프리뷰 슬라이더
					//- (메카님) 연결된 애니메이션 클립

					int width_AutoPlay = 25;
					int width_PreviewBtn = 30;
					int height_AnimInfo = 24;

					int width_AnimName = 0;
					int width_LinkedClip = 0;
					bool isShowPreviewBtn = false;
					bool isShowLinkedAnimClip = false;
						
					
					if (!_targetPortrait._isUsingMecanim)
					{
						//메카님을 사용하지 않는다면
						if (Application.isPlaying)
						{
							//플레이 중이라면 (프리뷰 없음)
							width_AnimName = width - (12 + width_AutoPlay);
							isShowPreviewBtn = false;
							
						}
						else
						{
							//에디터 중이라면 (프리뷰 있음)
							width_AnimName = width - (12 + width_AutoPlay + width_PreviewBtn);
							isShowPreviewBtn = true;
						}

						isShowLinkedAnimClip = false;
						width_LinkedClip = 0;
					}
					else
					{
						//메카님을 사용한다면
						width_LinkedClip = (int)((float)width * 0.4f);
						isShowLinkedAnimClip = true;

						if (Application.isPlaying)
						{
							//플레이 중이라면 (프리뷰 없음)
							width_AnimName = width - (12 + width_AutoPlay + width_LinkedClip + 4);
							isShowPreviewBtn = false;
						}
						else
						{
							//에디터 중이라면 (프리뷰 있음)
							width_AnimName = width - (12 + width_AutoPlay + width_PreviewBtn + width_LinkedClip + 4);
							isShowPreviewBtn = true;
						}
					}


					apAnimClip curAnimClip = null;

					for (int i = 0; i < nAnimClips; i++)
					{
						//- 자동 시작 아이콘, 이름, 복사 버튼, (메카님시) 연결된 링크 클립, 프리뷰 활성 버튼 (토글 방식)
						
						curAnimClip = _targetPortrait._animClips[i];

						bool isAutoStarted = curAnimClip._uniqueID == _targetPortrait._autoPlayAnimClipID;

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 2), GUILayout.Height(height_AnimInfo));
						GUILayout.Space(5);
						
						if (isAutoStarted)
						{
							EditorGUILayout.LabelField(_guiContent_AnimAutoPlay, _guiStyle_Label_Middle, GUILayout.Width(width_AutoPlay), GUILayout.Height(height_AnimInfo));
						}
						else
						{
							EditorGUILayout.LabelField("[" + i + "]", _guiStyle_Label_Middle, GUILayout.Width(width_AutoPlay), GUILayout.Height(height_AnimInfo));
						}

						

						EditorGUILayout.BeginVertical(GUILayout.Width(width_AnimName), GUILayout.Height(22));
						GUILayout.Space(5);
						EditorGUILayout.TextField(curAnimClip._name, GUILayout.Width(width_AnimName - 4), GUILayout.Height(18));
						EditorGUILayout.EndVertical();

						if(isShowLinkedAnimClip)
						{
							EditorGUILayout.BeginVertical(GUILayout.Width(width_LinkedClip), GUILayout.Height(22));
							GUILayout.Space(5);

							try
							{	
								AnimationClip nextAnimationClip = EditorGUILayout.ObjectField(	curAnimClip._animationClipForMecanim,
																								typeof(AnimationClip), 
																								false
																								, GUILayout.Width(width_LinkedClip - 4)
																								) as AnimationClip;
								
								if (nextAnimationClip != curAnimClip._animationClipForMecanim)
								{
									UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
									Undo.IncrementCurrentGroup();
									Undo.RegisterCompleteObjectUndo(_targetPortrait, "Animation Changed");

									curAnimClip._animationClipForMecanim = nextAnimationClip;
								}
							}
							catch (Exception)
							{ }

							EditorGUILayout.EndVertical();
						}


						

						if (isShowPreviewBtn)
						{
							if (ToggleButton(	_img_AnimPreview_Stop, _img_AnimPreview_Play,
												curAnimClip == _previewAnimClip,
												width_PreviewBtn, 20))
							{
								if(curAnimClip != _previewAnimClip)
								{
									//선택하기
									_previewAnimClip = curAnimClip;
									_previewFrame = _previewAnimClip.StartFrame;
								}
								else
								{
									//이미 선택됨 > 선택 해제하기
									_previewAnimClip = null;
									_previewFrame = 0;
								}

								//중요 : 버튼을 누를때 Initialize가 일부만 되어있는 상태일 수 있다. (에러 상황)
								//이 경우를 방지하기 위해 Initialize를 호출하도록 만든다.
								_targetPortrait.SetFirstInitializeAfterBake();
								_targetPortrait.Initialize();

								isChanged = true;
							}
						}
						
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(2);
						
					}
					
					//먄약 프리뷰 대상인 애니메이션이 있다면
					if(_previewAnimClip != null && !Application.isPlaying)
					{
						GUILayout.Space(10);

						EditorGUILayout.LabelField("Preview : " + _previewAnimClip._name + " ( " + _previewAnimClip.StartFrame + " ~ " + _previewAnimClip.EndFrame + " )");
						GUILayout.Space(2);
						int nextPreviewFrame = EditorGUILayout.IntSlider(_previewFrame, _previewAnimClip.StartFrame, _previewAnimClip.EndFrame);
						if(nextPreviewFrame != _previewFrame)
						{
							_previewFrame = nextPreviewFrame;
							isChanged = true;
						}
						
					}

					GUILayout.Space(5);
					//애니메이션 이름 모두 복사하기
					if (GUILayout.Button("Copy Names of Animations to Clipboard"))
					{	
						CopyAnimationNames();
					}
				}
			}


			GUILayout.Space(20);


			//메카님 설정
			//EditorGUILayout.LabelField("Mecanim Settings");
			//EditorGUILayout.LabelField(_guiContent_Mecanim, GUILayout.Height(24));
			DrawSubTitle(_guiContent_Mecanim, width, new Color(0.2f, 1.0f, 0.2f));


			bool isNextUsingMecanim = EditorGUILayout.Toggle("Use Mecanim", _targetPortrait._isUsingMecanim);
			if (_targetPortrait._isUsingMecanim != isNextUsingMecanim)
			{
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
				Undo.IncrementCurrentGroup();
				Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

				_targetPortrait._isUsingMecanim = isNextUsingMecanim;
			}


			if (_targetPortrait._isUsingMecanim)
			{
				AnimationClip nextEmptyAnimClip = EditorGUILayout.ObjectField("Empty Anim Clip", _targetPortrait._emptyAnimClipForMecanim, typeof(AnimationClip), false) as AnimationClip;
				if (nextEmptyAnimClip != _targetPortrait._emptyAnimClipForMecanim)
				{
					UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
					Undo.IncrementCurrentGroup();
					Undo.RegisterCompleteObjectUndo(_targetPortrait, "Animation Changed");

					_targetPortrait._emptyAnimClipForMecanim = nextEmptyAnimClip;
				}

				//GUILayout.Space(10);
				try
				{
					Animator nextAnimator = EditorGUILayout.ObjectField("Animator", _targetPortrait._animator, typeof(Animator), true) as Animator;
					if (nextAnimator != _targetPortrait._animator)
					{
						//하위에 있는 Component일 때에만 변동 가능
						if (nextAnimator == null)
						{
							_targetPortrait._animator = null;
						}
						else
						{
							if (nextAnimator == _targetPortrait.GetComponent<Animator>())
							{
								_targetPortrait._animator = nextAnimator;
							}
							else
							{
								EditorUtility.DisplayDialog("Invalid Animator", "Invalid Animator. Only the Animator, which is its own component, is valid.", "Okay");

							}
						}

					}
				}
				catch (Exception)
				{

				}
				if (_targetPortrait._animator == null)
				{
					//1. Animator가 없다면
					// > 생성하기
					// > 생성되어 있다면 다시 링크
					GUIStyle guiStyle_WarningText = new GUIStyle(GUI.skin.label);
					guiStyle_WarningText.normal.textColor = Color.red;
					EditorGUILayout.LabelField("Warning : No Animator!", guiStyle_WarningText);
					GUILayout.Space(5);

					if (GUILayout.Button("Add / Check Animator", GUILayout.Height(25)))
					{
						UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
						Undo.IncrementCurrentGroup();
						Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

						Animator animator = _targetPortrait.gameObject.GetComponent<Animator>();
						if (animator == null)
						{
							animator = _targetPortrait.gameObject.AddComponent<Animator>();
						}
						_targetPortrait._animator = animator;
					}
				}
				else
				{
					//2. Animator가 있다면
					if (GUILayout.Button("Refresh Layers", GUILayout.Height(25)))
					{
						UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
						Undo.IncrementCurrentGroup();
						Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

						//Animator의 Controller가 있는지 체크해야한다.

						if (_targetPortrait._animator.runtimeAnimatorController == null)
						{
							//AnimatorController가 없다면 Layer는 초기화
							_targetPortrait._animatorLayerBakedData.Clear();
						}
						else
						{
							//AnimatorController가 있다면 레이어에 맞게 설정
							_targetPortrait._animatorLayerBakedData.Clear();
							UnityEditor.Animations.AnimatorController animatorController = _targetPortrait._animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

							if (animatorController != null && animatorController.layers.Length > 0)
							{
								for (int iLayer = 0; iLayer < animatorController.layers.Length; iLayer++)
								{
									apAnimMecanimData_Layer newLayerData = new apAnimMecanimData_Layer();
									newLayerData._layerIndex = iLayer;
									newLayerData._layerName = animatorController.layers[iLayer].name;
									newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Unknown;
									switch (animatorController.layers[iLayer].blendingMode)
									{
										case UnityEditor.Animations.AnimatorLayerBlendingMode.Override:
											newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Override;
											break;

										case UnityEditor.Animations.AnimatorLayerBlendingMode.Additive:
											newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Additive;
											break;
									}

									_targetPortrait._animatorLayerBakedData.Add(newLayerData);
								}
							}
						}
					}
					GUILayout.Space(5);
					EditorGUILayout.LabelField("Animator Controller Layers");
					for (int i = 0; i < _targetPortrait._animatorLayerBakedData.Count; i++)
					{
						apAnimMecanimData_Layer layer = _targetPortrait._animatorLayerBakedData[i];
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(5);
						EditorGUILayout.LabelField("[" + layer._layerIndex + "]", GUILayout.Width(50));
						EditorGUILayout.TextField(layer._layerName);
						apAnimMecanimData_Layer.MecanimLayerBlendType nextBlendType = (apAnimMecanimData_Layer.MecanimLayerBlendType)EditorGUILayout.EnumPopup(layer._blendType);
						EditorGUILayout.EndHorizontal();

						if (nextBlendType != layer._blendType)
						{
							UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
							Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(_targetPortrait, "Mecanim Setting Changed");

							_targetPortrait._animatorLayerBakedData[i]._blendType = nextBlendType;
						}
					}
				}

			}


			GUILayout.Space(20);


			//추가 3.4 : 타임라인 설정
#if UNITY_2017_1_OR_NEWER

			DrawSubTitle(_guiContent_Timeline, width, new Color(0.3f, 0.3f, 0.0f));

			_isFold_Timeline = EditorGUILayout.Foldout(_isFold_Timeline, "Track Data");
			if(_isFold_Timeline)
			{
					
				int nextTimelineTracks = EditorGUILayout.DelayedIntField("Size", _nTimelineTrackSet);
				if(nextTimelineTracks != _nTimelineTrackSet)
				{
					//TimelineTrackSet의 개수가 바뀌었다. 
					UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
					Undo.IncrementCurrentGroup();
					Undo.RegisterCompleteObjectUndo(_targetPortrait, "Track Setting Changed");
					_nTimelineTrackSet = nextTimelineTracks;
					if(_nTimelineTrackSet < 0)
					{
						_nTimelineTrackSet = 0;
					}

					//일단 이전 개수만큼 복사를 한다.
					int nPrev = 0;
					List<apPortrait.TimelineTrackPreset> prevSets = new List<apPortrait.TimelineTrackPreset>();
					if(_targetPortrait._timelineTrackSets != null && _targetPortrait._timelineTrackSets.Length > 0)
					{
						for (int i = 0; i < _targetPortrait._timelineTrackSets.Length; i++)
						{
							prevSets.Add(_targetPortrait._timelineTrackSets[i]);
						}
						nPrev = _targetPortrait._timelineTrackSets.Length;
					}
						
					//배열을 새로 만들자
					_targetPortrait._timelineTrackSets = new apPortrait.TimelineTrackPreset[_nTimelineTrackSet];

					//가능한 이전 소스를 복사한다.
					for (int i = 0; i < _nTimelineTrackSet; i++)
					{
						if(i < nPrev)
						{
							_targetPortrait._timelineTrackSets[i] = new apPortrait.TimelineTrackPreset();
							_targetPortrait._timelineTrackSets[i]._playableDirector = prevSets[i]._playableDirector;
							_targetPortrait._timelineTrackSets[i]._trackName = prevSets[i]._trackName;
							_targetPortrait._timelineTrackSets[i]._layer = prevSets[i]._layer;
							_targetPortrait._timelineTrackSets[i]._blendMethod = prevSets[i]._blendMethod;
						}
						else
						{
							_targetPortrait._timelineTrackSets[i] = new apPortrait.TimelineTrackPreset();
						}
					}


					apEditorUtil.ReleaseGUIFocus();
						
				}

				GUILayout.Space(5);

				if(_targetPortrait._timelineTrackSets != null)
				{
					apPortrait.TimelineTrackPreset curTrackSet = null;
					for (int i = 0; i < _targetPortrait._timelineTrackSets.Length; i++)
					{
						//트랙을 하나씩 적용
						curTrackSet = _targetPortrait._timelineTrackSets[i];
							
						EditorGUILayout.LabelField("[" + i + "] : " + (curTrackSet._playableDirector == null ? "<None>" : curTrackSet._playableDirector.name));

						EditorGUI.BeginChangeCheck();

						PlayableDirector nextDirector = EditorGUILayout.ObjectField("Director", curTrackSet._playableDirector, typeof(PlayableDirector), true) as PlayableDirector;
						string nextTrackName = EditorGUILayout.DelayedTextField("Track Name", curTrackSet._trackName);
						int nextLayer = EditorGUILayout.DelayedIntField("Layer", curTrackSet._layer);
						apAnimPlayUnit.BLEND_METHOD nextBlendMethod = (apAnimPlayUnit.BLEND_METHOD)EditorGUILayout.EnumPopup("Blend", curTrackSet._blendMethod);

						if (EditorGUI.EndChangeCheck())
						{
							if (nextDirector != curTrackSet._playableDirector
								|| !string.Equals(nextTrackName, curTrackSet._trackName)
								|| nextLayer != curTrackSet._layer
								|| nextBlendMethod != curTrackSet._blendMethod
								)
							{
								RecordUndo();
								//UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
								//Undo.IncrementCurrentGroup();
								//Undo.RegisterCompleteObjectUndo(_targetPortrait, "Track Setting Changed");

								curTrackSet._playableDirector = nextDirector;
								curTrackSet._trackName = nextTrackName;
								curTrackSet._layer = nextLayer;
								curTrackSet._blendMethod = nextBlendMethod;

								
							}
							apEditorUtil.ReleaseGUIFocus();
						}

						GUILayout.Space(5);
					}
				}
			}

			GUILayout.Space(20);
#endif

			return isChanged;

		}


		//----------------------------------------------------
		// 4. 애니메이션 이벤트
		//----------------------------------------------------
		private bool GUI_Tab_4_AnimEvents(int width)
		{
			bool isChanged = false;

			//제목 : 설정에 따라 다르다.
			if(_targetPortrait._animEventCallMode == apPortrait.ANIM_EVENT_CALL_MODE.SendMessage)
			{
				DrawSubTitle(_guiContent_AnimEvents, width, new Color(1.0f, 1.0f, 0.0f));
			}
			else
			{
				DrawSubTitle(_guiContent_AnimEvents_UnityVer, width, new Color(0.2f, 0.2f, 0.1f));
			}

			//TODO : 이거 매번 갱신하니 느려진다.
			//int nAnimClips = _targetPortrait._animClips != null ? _targetPortrait._animClips.Count : 0;
			Color prevColor = GUI.backgroundColor;




			//옵션 : 이벤트 리스너 모드부터 설정 (21.9.25)
			apPortrait.ANIM_EVENT_CALL_MODE nextAnimCallMode = (apPortrait.ANIM_EVENT_CALL_MODE)EditorGUILayout.EnumPopup("Event Method", _targetPortrait._animEventCallMode);
			if (nextAnimCallMode != _targetPortrait._animEventCallMode)
			{
				Undo.IncrementCurrentGroup();
				Undo.RegisterCompleteObjectUndo(_targetPortrait, "Animation Setting Changed");

				_targetPortrait._animEventCallMode = nextAnimCallMode;

				//변경 되었을 때도 Event Wrapper Bake를 해야한다.
				if (_targetPortrait._unityEventWrapper == null)
				{
					_targetPortrait._unityEventWrapper = new apUnityEventWrapper();
				}
				_targetPortrait._unityEventWrapper.Bake(_targetPortrait);

				apEditorUtil.SetEditorDirty();
			}

			if (_targetPortrait._animEventCallMode == apPortrait.ANIM_EVENT_CALL_MODE.SendMessage)
			{
				GUILayout.Space(5);

				//이벤트 모드 1 : SendMessage 방식
				MonoBehaviour nextEventListener = (MonoBehaviour)EditorGUILayout.ObjectField("Event Listener", _targetPortrait._optAnimEventListener, typeof(MonoBehaviour), true);

				if (nextEventListener != _targetPortrait._optAnimEventListener)
				{
					Undo.IncrementCurrentGroup();
					Undo.RegisterCompleteObjectUndo(_targetPortrait, "Animation Setting Changed");

					_targetPortrait._optAnimEventListener = nextEventListener;

					apEditorUtil.SetEditorDirty();
				}

				//애니메이션 이벤트가 존재하는 경우
				if (_nTotalAnimEvents > 0 && _animEventInfos != null)
				{
					GUILayout.Space(5);

					_isFold_AnimationEvents = EditorGUILayout.Foldout(_isFold_AnimationEvents, "Events");
					if (_isFold_AnimationEvents)
					{
						for (int iInfo = 0; iInfo < _nTotalAnimEvents; iInfo++)
						{
							apAnimEvent curEvent = _animEventInfos[iInfo]._animEvent;

							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(5);

							EditorGUILayout.TextField(_animEventInfos[iInfo]._strEventName, GUILayout.Width(width - (5 + 35)));

							if (GUILayout.Button(_guiContent_Copy, _guiStyle_button_NoMargin, GUILayout.Width(35), GUILayout.Height(16)))
							{
								CopyAnimEventAsScript(curEvent);
							}

							EditorGUILayout.EndHorizontal();
						}
						if (GUILayout.Button("Copy All Events to Clipboard"))
						{
							//모든 이벤트 복사하기
							CopyAnimEventsAsScript(_animEventInfos);
						}
					}
				}

				GUILayout.Space(20);
			}
			else
			{
				//이벤트 모드 2 : Unity Event 방식
				int nUnityEvents = 0;
				if (_targetPortrait._unityEventWrapper != null)
				{
					nUnityEvents = _targetPortrait._unityEventWrapper._unityEvents != null ? _targetPortrait._unityEventWrapper._unityEvents.Count : 0;
				}

				GUILayout.Space(5);

				apUnityEvent curUnityEvent = null;

				_isFold_AnimationEvents = EditorGUILayout.Foldout(_isFold_AnimationEvents, "Events");
				if (_isFold_AnimationEvents && nUnityEvents > 0)
				{
					GUILayout.Space(5);
					
					//SerializedProperty curEventProperty = null;
					apUnityEvent.TargetMethodSet curTMSet = null;
					apUnityEvent.TargetMethodSet removeTMSet = null;
					for (int iUEvent = 0; iUEvent < nUnityEvents; iUEvent++)
					{
						curUnityEvent = _targetPortrait._unityEventWrapper._unityEvents[iUEvent];

						//유니티 이벤트들을 GUI에 출력하자
						

						//직접 UI를 만들자
						//배경

						if (curUnityEvent._targetMethods == null)
						{
							curUnityEvent._targetMethods = new List<apUnityEvent.TargetMethodSet>();
						}
						int nTMs = curUnityEvent._targetMethods.Count;
						removeTMSet = null;//삭제 확인용

						Rect lastRect = GUILayoutUtility.GetLastRect();


						GUI.backgroundColor = new Color(prevColor.r * 0.5f, prevColor.g * 0.5f, prevColor.b * 0.5f, 1.0f);

						int offsetY = 10;
						if (iUEvent == 0)
						{
							offsetY -= 5;

						}
						//뒤에 그리는게 조금 이상하다.
						int backBGHeight = 47 + (nTMs * 24);
						GUI.Box(new Rect(lastRect.x - 5, lastRect.y + offsetY, width + 5, backBGHeight), "");
						

						GUI.backgroundColor = prevColor;


						EditorGUILayout.LabelField(curUnityEvent.GetGUILabel(), _guiStyle_WhiteText, GUILayout.Height(20));

						int width_TM_Target = Mathf.Clamp((int)((float)width * 0.4f), 80, 250);
						int width_TM_Remove = 20;
						int width_TM_Method = width - (30 + width_TM_Target + width_TM_Remove);


						for (int iTM = 0; iTM < nTMs; iTM++)
						{
							curTMSet = curUnityEvent._targetMethods[iTM];

							EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
							GUILayout.Space(5);
							try
							{
								MonoBehaviour nextMonoTarget = curTMSet._target;
								nextMonoTarget = EditorGUILayout.ObjectField(curTMSet._target, typeof(MonoBehaviour), true, GUILayout.Width(width_TM_Target)) as MonoBehaviour;
								if (nextMonoTarget != curTMSet._target)
								{
									//Undo 등록
									//메소드 초기화 및 리스트 작성									
									Undo.IncrementCurrentGroup();
									Undo.RegisterCompleteObjectUndo(_targetPortrait, "Event Changed");

									curTMSet._target = nextMonoTarget;
									curTMSet._methodName = "";

									apEditorUtil.SetEditorDirty();
								}
							}
							catch (Exception) { }

							//Method들을 보여주자
							string methodName = string.IsNullOrEmpty(curTMSet._methodName) ? "" : curTMSet._methodName;


							if (GUILayout.Button(methodName, GUILayout.Width(width_TM_Method), GUILayout.Height(18)))
							{
								ShowMethodsOfUnityEvent(curUnityEvent, curTMSet);
							}

							if (GUILayout.Button(_guiContent_RemoveButton, _guiStyle_buttonIcon, GUILayout.Width(width_TM_Remove), GUILayout.Height(20)))
							{
								removeTMSet = curTMSet;
							}
							EditorGUILayout.EndHorizontal();

						}
						//타겟, 함수, 삭제
						//추가
						EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
						GUILayout.Space(width - (37));
						if (GUILayout.Button(_guiContent_AddButton, _guiStyle_buttonIcon, GUILayout.Width(width_TM_Remove), GUILayout.Height(20)))
						{
							Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(_targetPortrait, "Event Changed");

							curUnityEvent._targetMethods.Add(new apUnityEvent.TargetMethodSet());

							apEditorUtil.SetEditorDirty();
						}
						EditorGUILayout.EndHorizontal();

						if (removeTMSet != null)
						{
							Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(_targetPortrait, "Event Changed");

							curUnityEvent._targetMethods.Remove(removeTMSet);

							apEditorUtil.SetEditorDirty();

							removeTMSet = null;
						}


						GUILayout.Space(10);
					}
				}

				if (nUnityEvents > 0)
				{
					//일괄 리스너 연결
					EditorGUILayout.LabelField("Assign Listener to All Events");

					EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
					GUILayout.Space(4);
					_targetBatchMonoListener = (MonoBehaviour)EditorGUILayout.ObjectField("Target", _targetBatchMonoListener, typeof(MonoBehaviour), true, GUILayout.Width(width - (2 + 80)));
					if (GUILayout.Button("Assign", GUILayout.Width(80)))
					{
						//하나씩 대상을 입력하자.
						if (_targetBatchMonoListener != null)
						{
							Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(_targetPortrait, "Event Changed");

							for (int iUEvent = 0; iUEvent < nUnityEvents; iUEvent++)
							{
								curUnityEvent = _targetPortrait._unityEventWrapper._unityEvents[iUEvent];
								curUnityEvent.AssignCommonMonoTarget(_targetBatchMonoListener);
							}

							apEditorUtil.SetEditorDirty();
						}
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(4);

					//유효성 검사 버튼
					if (GUILayout.Button("Validate Events"))
					{
						//하나라도 유효하지 않은게 있는지 체크한다.
						bool isAnyInvalid = false;
						for (int iUEvent = 0; iUEvent < nUnityEvents; iUEvent++)
						{
							curUnityEvent = _targetPortrait._unityEventWrapper._unityEvents[iUEvent];

							bool isValid = curUnityEvent.ValidateInEditor();
							if (!isValid)
							{
								//유효하지 않은거 발견
								isAnyInvalid = true;
								break;
							}
						}

						if (isAnyInvalid)
						{
							//유효하지 않은걸 모두 삭제하자
							Undo.IncrementCurrentGroup();
							Undo.RegisterCompleteObjectUndo(_targetPortrait, "Event Changed");

							for (int iUEvent = 0; iUEvent < nUnityEvents; iUEvent++)
							{
								curUnityEvent = _targetPortrait._unityEventWrapper._unityEvents[iUEvent];
								curUnityEvent.RemoveInvalidMethodSetInEditor();
							}

							apEditorUtil.SetEditorDirty();

							Debug.Log("AnyPortrait : Invalid event methods have been removed.");
						}
					}
				}
				

				GUILayout.Space(20);

			}

			return isChanged;
		}


		//----------------------------------------------------
		// 5. 컨트롤 파라미터 (프리뷰)
		//----------------------------------------------------
		private bool GUI_Tab_5_ControlParams(int width)
		{
			bool isChanged = false;

			DrawSubTitle(_guiContent_ControlParams, width, new Color(1.0f, 0.0f, 0.2f));


			

			
#if UNITY_2017_3_OR_NEWER
			_curControlCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumFlagsField(_guiContent_Category, _curControlCategory);
#else
			_curControlCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumMaskPopup(_guiContent_Category, _curControlCategory);
#endif

			EditorGUILayout.Space();
			//1. 컨르롤러를 제어할 수 있도록 하자

			if (_controlParams != null)
			{
				for (int i = 0; i < _controlParams.Count; i++)
				{
					if ((int)(_controlParams[i]._category & _curControlCategory) != 0)
					{
						if (GUI_ControlParam(_controlParams[i]))
						{
							//만약 재생중인 애니메이션이 있다면 해제
							_previewAnimClip = null;
							_previewFrame = 0;
							isChanged = true;
						}
					}
				}

				GUILayout.Space(5);
				if (GUILayout.Button("Copy Control Parameters to Clipboard"))
				{
					//컨트롤 파라미터 이름 클립보드로 복사하기
					CopyControlParamNames();
				}
			}

			return isChanged;
		}



		private void DrawSubTitle(GUIContent guiContent, int width, Color baseColor)
		{
			Color prevColor = GUI.backgroundColor;

			GUILayout.Space(5);
			
			EditorGUILayout.BeginHorizontal(/*GUILayout.Width(width), */GUILayout.Height(28));
			GUILayout.Space(5);

			//색상을 받아서 밝기를 조절한다.
			float color_H = 1.0f;
			float color_S = 1.0f;
			float color_V = 1.0f;
			Color.RGBToHSV(baseColor, out color_H, out color_S, out color_V);

			if (EditorGUIUtility.isProSkin)
			{
				//Pro에서는 조금 더 어두워야 한다.
				//GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
				color_V *= 0.2f;
			}
			else
			{
				//GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
				color_V *= 0.3f;
			}

			GUI.backgroundColor = Color.HSVToRGB(color_H, color_S, color_V);
			

			GUILayout.Box(guiContent, _guiStyle_subTitle_Title, GUILayout.Width(width - 2), GUILayout.Height(26));
			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = prevColor;
			//GUILayout.Space(10);
		}


		private bool ToggleButton(	Texture2D selectedImg, 
									Texture2D notSelectedImg, 
									bool isSelected, 
									int width, 
									int height)
		{
			if(isSelected)
			{
				Color prevColor = GUI.backgroundColor;

				if (EditorGUIUtility.isProSkin)
				{
					//밝은 파랑 + 하늘색
					//textColor = Color.cyan;
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//청록색 + 흰색
					//textColor = Color.white;
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				bool isBtn = GUILayout.Button(selectedImg, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtn;
			}
			else
			{
				return GUILayout.Button(notSelectedImg, GUILayout.Width(width), GUILayout.Height(height));
			}
		}



		// Sub Functions
		//--------------------------------------------------------------------------------------

		private void RefreshPrefabStatus()
		{
			//추가 20.9.14 : 프리팹 여부 체크하기
			_isPrefabAsset = false;
			_isPrefabInstance = false;
			_srcPrefabObject = null;
			_rootGameObjAsPrefabInstance = null;

#if UNITY_2021_1_OR_NEWER
			//추가 v1.4.2 : 유니티 2021.1부터 지원되는 값들

			//프리팹 편집 화면(Isolation Mode 등)인가.
			_isPrefabEditingScreen = PrefabStageUtility.GetCurrentPrefabStage() != null;
#endif


			//프리팹 에셋이면 아무것도 편집 불가
			_isPrefabAsset = apEditorUtil.IsPrefabAsset(_targetPortrait.gameObject);
			if(_isPrefabAsset)
			{
				return;
			}

#if UNITY_2021_1_OR_NEWER
			//프리팹 편집 화면에서는 편집이 불가능하다.
			if(_isPrefabEditingScreen)
			{
				return;
			}
#endif

			_prefabStatus = apEditorUtil.GetPrefabStatus(_targetPortrait.gameObject);

			switch (_prefabStatus)
			{
				case apEditorUtil.PREFAB_STATUS.NoPrefab:
#if UNITY_2018_3_OR_NEWER
					//이 상태에서 2018.3버전의 프리팹인 경우, 복원 정보가 저장되어 있다면 Disconnected로 변경한다.
					//만약 Disconnected라면 이 정보가 없을 수 있다. (Connected라면 연결되어 있을까?)
					//복원용 정보를 입력하고 Disconnected 상태로 만들자.
					_srcPrefabObject = _targetPortrait._srcPrefabAssetForRestore;
					_rootGameObjAsPrefabInstance = _targetPortrait._rootGameObjectAsPrefabInstanceForRestore;

					if(_srcPrefabObject != null && _rootGameObjAsPrefabInstance != null)
					{
						//둘다 있는 경우에 한해서 Disconnected로 변경
						_isPrefabInstance = true;
						_prefabStatus = apEditorUtil.PREFAB_STATUS.Disconnected;
					}
					else
					{
						//그렇지 않다면 초기화
						_srcPrefabObject = null;
						_rootGameObjAsPrefabInstance = null;
					}
#endif
					break;

				case apEditorUtil.PREFAB_STATUS.Connected:
				case apEditorUtil.PREFAB_STATUS.Disconnected:
					_isPrefabInstance = true;
					_srcPrefabObject = apEditorUtil.GetPrefabObject(_targetPortrait.gameObject);
					_rootGameObjAsPrefabInstance = apEditorUtil.GetRootGameObjectAsPrefabInstance(_targetPortrait.gameObject);
					
					//만약 Disconnected라면 이 정보가 없을 수 있다. (Connected라면 연결되어 있을까?)
					//복원용 정보를 입력하고 Disconnected 상태로 만들자.
					if(_srcPrefabObject == null)
					{
						_srcPrefabObject = _targetPortrait._srcPrefabAssetForRestore;
						_prefabStatus = apEditorUtil.PREFAB_STATUS.Disconnected;
					}
					if(_rootGameObjAsPrefabInstance == null)
					{
						_rootGameObjAsPrefabInstance = _targetPortrait._rootGameObjectAsPrefabInstanceForRestore;
						_prefabStatus = apEditorUtil.PREFAB_STATUS.Disconnected;
					}
					break;

				case apEditorUtil.PREFAB_STATUS.Missing:
					_isPrefabInstance = true;
					break;

				case apEditorUtil.PREFAB_STATUS.Asset://v1.4.2 추가
					_isPrefabAsset = true;
					_isPrefabInstance = false;
					break;
			}
		}

		private void FindSockets()
		{
			if(_socketInfos == null)
			{
				_socketInfos = new List<SocketInfo>();
			}
			_socketInfos.Clear();

			if(_targetPortrait == null)
			{
				return;
			}

			int nRootUnits = _targetPortrait._optRootUnitList != null ? _targetPortrait._optRootUnitList.Count : 0;
			apOptRootUnit curRootUnit = null;
			apOptTransform curTF = null;
			apOptBone curBone = null;

			//소켓을 가진 Transform이나 Bone을 찾자
			for (int iRoot = 0; iRoot < nRootUnits; iRoot++)
			{
				curRootUnit = _targetPortrait._optRootUnitList[iRoot];
				if(curRootUnit == null)
				{
					continue;
				}
				int nTransforms = curRootUnit.OptTransforms != null ? curRootUnit.OptTransforms.Count : 0;
				int nBones = curRootUnit.OptBones != null ? curRootUnit.OptBones.Count : 0;

				if (nTransforms > 0)
				{
					for (int iTransform = 0; iTransform < nTransforms; iTransform++)
					{
						curTF = curRootUnit.OptTransforms[iTransform];
						if(curTF._socketTransform != null)
						{
							_socketInfos.Add(new SocketInfo(curTF));
						}
					}
				}

				if (nBones > 0)
				{
					for (int iBone = 0; iBone < nBones; iBone++)
					{
						curBone = curRootUnit.OptBones[iBone];
						if(curBone._socketTransform != null)
						{
							_socketInfos.Add(new SocketInfo(curBone));
						}
					}
				}
			}
			

		}


		private void RefreshAnimEventInfo()
		{
			_nTotalAnimEvents = 0;
			if(_animEventInfos == null)
			{
				_animEventInfos = new List<AnimEventInfo>();
			}
			_animEventInfos.Clear();
			
			List<string> animEventNames = null;
			
			if (_targetPortrait == null)
			{
				return;
			}

			int nAnimClips = _targetPortrait._animClips != null ? _targetPortrait._animClips.Count : 0;

			if(nAnimClips == 0)
			{
				return;
			}

			List<apAnimEvent> curEvents = null;
			for (int i = 0; i < nAnimClips; i++)
			{
				curEvents = _targetPortrait._animClips[i]._animEvents;
				int nCurEvents = curEvents != null ? curEvents.Count : 0;
				if (nCurEvents == 0)
				{
					continue;
				}

				if (_animEventInfos == null)
				{
					_animEventInfos = new List<AnimEventInfo>();
				}
				if (animEventNames == null)
				{
					animEventNames = new List<string>();
				}

				for (int iEvent = 0; iEvent < nCurEvents; iEvent++)
				{
					apAnimEvent curEvent = curEvents[iEvent];
					AnimEventInfo eventInfo = MakeAnimEventInfo(curEvent);

					//해당 이름이 이미 있는지 확인하고, 없으면 이벤트 등록
					if (!animEventNames.Contains(eventInfo._strEventName))
					{
						_animEventInfos.Add(eventInfo);
						animEventNames.Add(eventInfo._strEventName);
						_nTotalAnimEvents += 1;
					}
				}
			}
		}

		private bool GUI_ControlParam(apControlParam controlParam)
		{
			if (controlParam == null)
			{ return false; }

			bool isChanged = false;

			EditorGUILayout.LabelField(controlParam._keyName);

			switch (controlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		bool bPrev = controlParam._bool_Cur;
				//		controlParam._bool_Cur = EditorGUILayout.Toggle(controlParam._bool_Cur);
				//		if(bPrev != controlParam._bool_Cur)
				//		{
				//			isChanged = true;
				//		}
				//	}
				//	break;

				case apControlParam.TYPE.Int:
					{
						int iPrev = controlParam._int_Cur;
						controlParam._int_Cur = EditorGUILayout.IntSlider(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);

						if (iPrev != controlParam._int_Cur)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float fPrev = controlParam._float_Cur;
						controlParam._float_Cur = EditorGUILayout.Slider(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);

						if (Mathf.Abs(fPrev - controlParam._float_Cur) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 v2Prev = controlParam._vec2_Cur;
						controlParam._vec2_Cur.x = EditorGUILayout.Slider(controlParam._vec2_Cur.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						controlParam._vec2_Cur.y = EditorGUILayout.Slider(controlParam._vec2_Cur.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);

						if (Mathf.Abs(v2Prev.x - controlParam._vec2_Cur.x) > 0.0001f ||
							Mathf.Abs(v2Prev.y - controlParam._vec2_Cur.y) > 0.0001f)
						{
							isChanged = true;
						}
					}
					break;

			}

			GUILayout.Space(5);

			return isChanged;
		}


		private apPortrait _requestPortrait = null;
		private enum REQUEST_TYPE
		{
			None,
			Open,
			OpenAndSet,
			QuickBake
		}
		private REQUEST_TYPE _requestType = REQUEST_TYPE.None;
		private IEnumerator _coroutine = null;
		

		private void RequestDelayedOpenEditor(apPortrait portrait, REQUEST_TYPE requestType)
		{
			if(_coroutine != null)
			{
				return;
			}

			_requestPortrait = portrait;
			_requestType = requestType;
			_coroutine = Crt_RequestEditor();

			EditorApplication.update -= ExecuteCoroutine;
			EditorApplication.update += ExecuteCoroutine;
		}







		private void ExecuteCoroutine()
		{
			if(_coroutine == null)
			{
				_requestType = REQUEST_TYPE.None;
				_requestPortrait = null;

				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}

			//Debug.Log("Update Coroutine");
			bool isResult = _coroutine.MoveNext();
			
			if(!isResult)
			{
				_coroutine = null;
				_requestType = REQUEST_TYPE.None;
				_requestPortrait = null;
				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}
		}
		private IEnumerator Crt_RequestEditor()
		{
			yield return new WaitForEndOfFrame();
			Selection.activeObject = null;

			yield return new WaitForEndOfFrame();

			if (_requestPortrait != null)
			{	
				try
				{	
					apEditor anyPortraitEditor = apEditor.ShowWindow();
					if (_requestType == REQUEST_TYPE.OpenAndSet)
					{
						anyPortraitEditor.SetPortraitByInspector(_requestPortrait, false);
					}
					else if (_requestType == REQUEST_TYPE.QuickBake)
					{
						anyPortraitEditor.SetPortraitByInspector(_requestPortrait, true);
						Selection.activeObject = _requestPortrait.gameObject;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Open Editor Error : " + ex);
				}
			}
			_requestType = REQUEST_TYPE.None;
			_requestPortrait = null;
		}




		private Texture2D LoadImage(string iconName)
		{
			//이전
			//return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AnyPortrait/Editor/Images/Inspector/" + iconName + ".png");
			//변경 20.4.21 : 경로 변경 설정을 따른다.
			//기본 경로는 "Assets/AnyPortrait/"이므로
			//경로면은 _basePath + "Editor/Images/Inspector/" + iconName + ".png"가 된다.
			return AssetDatabase.LoadAssetAtPath<Texture2D>(_basePath + "Editor/Images/Inspector/" + iconName + ".png");
		}





		// 추가 22.6.17
		//모든 애니메이션 이름들을 클립보드로 복사한다.
		private void CopyAnimationNames()
		{
			if(_targetPortrait == null)
			{
				return;
			}
			int nAnimClips = _targetPortrait._animClips != null ? _targetPortrait._animClips.Count : 0;
			if(nAnimClips == 0)
			{
				return;
			}

			string strNames = "";

			apAnimClip curAnimClip = null;
			int nCopied = 0;
			for (int i = 0; i < nAnimClips; i++)
			{
				curAnimClip = _targetPortrait._animClips[i];
				if(string.IsNullOrEmpty(curAnimClip._name))
				{
					continue;
				}


				strNames += curAnimClip._name;
				if(i < nAnimClips - 1)
				{
					strNames += "\n";
				}

				nCopied += 1;
			}

			if(nCopied == 0)
			{
				return;
			}

			EditorGUIUtility.systemCopyBuffer = strNames;
			
			if(nCopied > 1)
			{
				Debug.Log("AnyPortrait : The names of " + nCopied + " animation clips have been copied to the clipboard.");
			}
			else
			{
				Debug.Log("AnyPortrait : The name of 1 animation clip has been copied to the clipboard.");
			}
			
		}



		private void CopySocketNames(List<SocketInfo> socketInfo)
		{	
			int nInfos = socketInfo != null ? socketInfo.Count : 0;
			if(nInfos == 0)
			{
				return;
			}

			string scriptCode = "";
			SocketInfo info = null;
			List<SocketInfo> infos_TF = new List<SocketInfo>();
			List<SocketInfo> infos_Bone = new List<SocketInfo>();

			for (int i = 0; i < nInfos; i++)
			{
				info = socketInfo[i];
				if(info._type == SOCKET_TYPE.Transform)
				{
					infos_TF.Add(info);
				}
				else
				{
					infos_Bone.Add(info);
				}
			}

			int nInfo_TF = infos_TF.Count;
			int nInfo_Bone = infos_Bone.Count;

			if(nInfo_TF == 0 && nInfo_Bone == 0)
			{
				return;
			}

			if (nInfo_TF > 0)
			{
				scriptCode += "-- Transform Sockets ( " + nInfo_TF + " ) --\n";
				for (int i = 0; i < nInfo_TF; i++)
				{
					info = infos_TF[i];
					scriptCode += info._name;

					if(i < nInfo_TF - 1)
					{
						scriptCode += "\n";
					}
				}
				if(nInfo_Bone > 0)
				{
					//한칸 더 비우기
					scriptCode += "\n\n\n";
				}
			}

			if (nInfo_Bone > 0)
			{
				scriptCode += "-- Bone Sockets ( " + nInfo_Bone + " ) --\n";
				for (int i = 0; i < nInfo_Bone; i++)
				{
					info = infos_Bone[i];
					scriptCode += info._name;

					if(i < nInfo_Bone - 1)
					{
						scriptCode += "\n";
					}
				}
			}
			

			EditorGUIUtility.systemCopyBuffer = scriptCode;
			
			Debug.Log("AnyPortrait : The names of " + nInfo_TF + " Transform Sockets and " + nInfo_Bone + " Bone Sockets have been copied to the clipboard.");
		}




		private void CopyImageNames()
		{	
			int nImages = _targetPortrait._optTextureData != null ? _targetPortrait._optTextureData.Count : 0;
			if(nImages == 0)
			{
				return;
			}

			string scriptCode = "";
			apOptTextureData curImage = null;

			for (int i = 0; i < nImages; i++)
			{
				curImage = _targetPortrait._optTextureData[i];
				scriptCode += curImage._name;
				
				if(i < nImages - 1)
				{
					scriptCode += "\n";
				}
			}


			EditorGUIUtility.systemCopyBuffer = scriptCode;
			
			Debug.Log("AnyPortrait : The names of " + nImages + " Images have been copied to the clipboard.");
		}




		private void CopyControlParamNames()
		{	
			if(_targetPortrait == null
				|| _targetPortrait._controller == null)
			{
				return;
			}

			int nControlParams = _targetPortrait._controller._controlParams != null ? _targetPortrait._controller._controlParams.Count : 0;
			
			if(nControlParams == 0)
			{
				return;
			}

			string scriptCode = "";
			apControlParam curCP = null;

			for (int i = 0; i < nControlParams; i++)
			{
				curCP = _targetPortrait._controller._controlParams[i];
				if(curCP == null)
				{
					continue;
				}
				if(string.IsNullOrEmpty(curCP._keyName))
				{
					continue;
				}
				scriptCode += curCP._keyName + " (";
				
				//타입도 추가하자
				switch (curCP._valueType)
				{
					case apControlParam.TYPE.Int:
						{
							scriptCode += "Int";
						}
						break;

					case apControlParam.TYPE.Float:
						{
							scriptCode += "Float";
						}
						break;

					case apControlParam.TYPE.Vector2:
						{
							scriptCode += "Vector2";
						}
						break;
				}
				scriptCode += ")";

				if(i < nControlParams - 1)
				{
					scriptCode += "\n";
				}
			}


			EditorGUIUtility.systemCopyBuffer = scriptCode;
			
			if(nControlParams > 1)
			{
				Debug.Log("AnyPortrait : The names of " + nControlParams + " Control Paramters have been copied to the clipboard.");
			}
			else
			{
				Debug.Log("AnyPortrait : The name of 1 Control Paramter has been copied to the clipboard.");
			}
			
		}



		//추가 21.8.3 : 애니메이션 이벤트 정보들을 UI에 보여주자

		private struct AnimEventInfo
		{
			public apAnimEvent _animEvent;
			public string _strEventName;
		}

		private AnimEventInfo MakeAnimEventInfo(apAnimEvent animEvent)
		{
			string strAnimEventName = animEvent._eventName;
			
			int nSubParams = animEvent._subParams != null ? animEvent._subParams.Count : 0;
			
			if(nSubParams > 0)
			{
				strAnimEventName += " : ";

				if(nSubParams > 1)
				{
					strAnimEventName += "(object[]) ";
				}

				apAnimEvent.SubParameter subParam = null;
				for (int i = 0; i < nSubParams; i++)
				{
					if(i != 0)
					{
						strAnimEventName += ", ";
					}
					subParam = animEvent._subParams[i];
					switch(subParam._paramType)
					{
						case apAnimEvent.PARAM_TYPE.Bool:		strAnimEventName += "bool";			break;
						case apAnimEvent.PARAM_TYPE.Integer:	strAnimEventName += "int";			break;
						case apAnimEvent.PARAM_TYPE.Float:		strAnimEventName += "float";		break;
						case apAnimEvent.PARAM_TYPE.Vector2:	strAnimEventName += "Vector2";		break;
						case apAnimEvent.PARAM_TYPE.String:		strAnimEventName += "string";		break;
					}
				}
			}

			AnimEventInfo newInfo = new AnimEventInfo();
			newInfo._animEvent = animEvent;
			newInfo._strEventName = strAnimEventName;
			return newInfo;
		}


		//추가 21.8.3 : 애니메이션 이벤트를 복사한다. (스크립트에 붙여넣기 좋게)
		private void CopyAnimEventAsScript(apAnimEvent animEvent)
		{
			string scriptCode = "private void ";
			scriptCode += animEvent._eventName;
			scriptCode += "(";
			
			//파라미터가 있는 경우
			int nSubParams = animEvent._subParams != null ? animEvent._subParams.Count : 0;
			if(nSubParams == 1)
			{
				//한개인 경우
				apAnimEvent.SubParameter subParam = animEvent._subParams[0];
				
				switch (subParam._paramType)
				{	
					case apAnimEvent.PARAM_TYPE.Bool:
						scriptCode += " bool boolValue ";
						break;

					case apAnimEvent.PARAM_TYPE.Integer:
						scriptCode += " int intValue ";
						break;

					case apAnimEvent.PARAM_TYPE.Float:
						scriptCode += " float floatValue ";
						break;

					case apAnimEvent.PARAM_TYPE.Vector2:						
						scriptCode += " Vector2 vecValue ";
						break;

					case apAnimEvent.PARAM_TYPE.String:
						scriptCode += " string strValue ";
						break;
				}
			}
			else if(nSubParams > 1)
			{
				scriptCode += " object[] multipleParams ";
			}
			scriptCode += ")";
			if(nSubParams > 1)
			{
				//파라미터가 여러개인 경우
				scriptCode += "\n{\n";
				apAnimEvent.SubParameter subParam = null;
				for (int i = 0; i < nSubParams; i++)
				{
					subParam = animEvent._subParams[i];
					switch (subParam._paramType)
					{
						case apAnimEvent.PARAM_TYPE.Bool:
							scriptCode += "\tbool boolValue = (bool)multipleParams[" + i + "];\n";
							break;

						case apAnimEvent.PARAM_TYPE.Integer:
							scriptCode += "\tint intValue = (int)multipleParams[" + i + "];\n";
							break;

						case apAnimEvent.PARAM_TYPE.Float:
							scriptCode += "\tfloat floatValue = (float)multipleParams[" + i + "];\n";
							break;

						case apAnimEvent.PARAM_TYPE.Vector2:
							scriptCode += "\tVector2 vecValue = (Vector2)multipleParams[" + i + "];\n";
							break;

						case apAnimEvent.PARAM_TYPE.String:
							scriptCode += "\tstring strValue = (string)multipleParams[" + i + "];\n";
							break;
					}
				}
				scriptCode += "}\n";
			}
			else
			{
				//그외의 경우
				scriptCode += " { }\n";
			}

			EditorGUIUtility.systemCopyBuffer = scriptCode;
			
			Debug.Log("AnyPortrait : The Animation event (" + animEvent._eventName + ") was copied to the clipboard as c# script format.");
		}

		private void CopyAnimEventsAsScript(List<AnimEventInfo> infos)
		{
			int nInfos = infos != null ? infos.Count : 0;
			if(nInfos == 0)
			{
				return;
			}

			string scriptCode = "";
			apAnimEvent animEvent = null;
			for (int iInfo = 0; iInfo < nInfos; iInfo++)
			{
				animEvent = infos[iInfo]._animEvent;

				scriptCode += "private void ";
				scriptCode += animEvent._eventName;
				scriptCode += "(";

				//파라미터가 있는 경우
				int nSubParams = animEvent._subParams != null ? animEvent._subParams.Count : 0;
				if (nSubParams == 1)
				{
					//한개인 경우
					apAnimEvent.SubParameter subParam = animEvent._subParams[0];

					switch (subParam._paramType)
					{
						case apAnimEvent.PARAM_TYPE.Bool:
							scriptCode += " bool boolValue ";
							break;

						case apAnimEvent.PARAM_TYPE.Integer:
							scriptCode += " int intValue ";
							break;

						case apAnimEvent.PARAM_TYPE.Float:
							scriptCode += " float floatValue ";
							break;

						case apAnimEvent.PARAM_TYPE.Vector2:
							scriptCode += " Vector2 vecValue ";
							break;

						case apAnimEvent.PARAM_TYPE.String:
							scriptCode += " string strValue ";
							break;
					}
				}
				else if (nSubParams > 1)
				{
					scriptCode += " object[] multipleParams ";
				}
				scriptCode += ")";
				if (nSubParams > 1)
				{
					//파라미터가 여러개인 경우
					scriptCode += "\n{\n";
					apAnimEvent.SubParameter subParam = null;
					for (int i = 0; i < nSubParams; i++)
					{
						subParam = animEvent._subParams[i];
						switch (subParam._paramType)
						{
							case apAnimEvent.PARAM_TYPE.Bool:
								scriptCode += "\tbool boolValue = (bool)multipleParams[" + i + "];\n";
								break;

							case apAnimEvent.PARAM_TYPE.Integer:
								scriptCode += "\tint intValue = (int)multipleParams[" + i + "];\n";
								break;

							case apAnimEvent.PARAM_TYPE.Float:
								scriptCode += "\tfloat floatValue = (float)multipleParams[" + i + "];\n";
								break;

							case apAnimEvent.PARAM_TYPE.Vector2:
								scriptCode += "\tVector2 vecValue = (Vector2)multipleParams[" + i + "];\n";
								break;

							case apAnimEvent.PARAM_TYPE.String:
								scriptCode += "\tstring strValue = (string)multipleParams[" + i + "];\n";
								break;
						}
					}
					scriptCode += "}\n";
				}
				else
				{
					//그외의 경우
					scriptCode += " { }\n";
				}

				if(iInfo < infos.Count - 1)
				{
					scriptCode += "\n";
				}
			}

			EditorGUIUtility.systemCopyBuffer = scriptCode;
			
			if(nInfos > 1)
			{
				Debug.Log("AnyPortrait : " + infos.Count + " Animation events were copied to the clipboard as c# script format.");
			}
			else
			{
				Debug.Log("AnyPortrait : " + infos.Count + " Animation event was copied to the clipboard as c# script format.");
			}
			
		}



		//추가 21.9.26 : 유니티 이벤트처럼 해당 객체의 함수를 보여준다.
		//타입을 비교해서 처리
		//UnityEvent에 Target Mono를 연결해둘것
		private void ShowMethodsOfUnityEvent(apUnityEvent targetUnityEvent, apUnityEvent.TargetMethodSet methodSet)
		{
			if(targetUnityEvent == null || methodSet == null)
			{
				return;
			}
			GenericMenu newMenu = new GenericMenu();
			if(methodSet._target == null)
			{
				//메소드를 찾을 수 없다.
				newMenu.AddItem(new GUIContent("<No Target>"), false, OnMethodOfUnitEventSelected, null);
			}
			else
			{
				//메소드들을 찾자
				//해당 Mono 뿐만아니라, 그 GameObject의 다른 Monobehaviour도 찾는다.
				//0. 현재의 메소드 이름에 맞는 메소드 Info를 찾자
				bool isAnyMethod = false;

				MethodInfo selectedMethodInfo = GetMethodInfoOfEvent(targetUnityEvent, methodSet);

				//1. 해당 MonoTarget의 메소드들을 찾자.
				List<MethodInfo> methodInfos = GetMethodInfosOfEvent(methodSet._target, targetUnityEvent);
				int nMethodInfos = methodInfos != null ? methodInfos.Count : 0;

				MethodInfo curMethodInfo = null;

				if(nMethodInfos > 0)
				{
					for (int i = 0; i < nMethodInfos; i++)
					{
						curMethodInfo = methodInfos[i];

						//메소드들을 추가한다.
						newMenu.AddItem(	new GUIContent(curMethodInfo.Name),
											selectedMethodInfo != null && MethodInfo.Equals(curMethodInfo, selectedMethodInfo),
											OnMethodOfUnitEventSelected,
											new object[] { targetUnityEvent, methodSet, methodSet._target, curMethodInfo});
						isAnyMethod = true;
					}
				}

				//2. 선택된 Mono의 GameObject에 다른 Mono가 있는 경우
				GameObject parentGameObject = methodSet._target.gameObject;
				if(parentGameObject != null)
				{
					MonoBehaviour[] monos = parentGameObject.GetComponents<MonoBehaviour>();
					int nMonos = monos != null ? monos.Length : 0;

					if(nMonos > 1)
					{
						//다른 Mono가 있는 것 같다.
						MonoBehaviour otherMono = null;
						for (int iMono = 0; iMono < nMonos; iMono++)
						{
							otherMono = monos[iMono];
							if(otherMono == methodSet._target)
							{
								//동일하면 패스
								continue;
							}

							methodInfos = GetMethodInfosOfEvent(otherMono, targetUnityEvent);
							nMethodInfos = methodInfos != null ? methodInfos.Count : 0;

							curMethodInfo = null;

							if (nMethodInfos > 0)
							{
								newMenu.AddSeparator("");

								for (int i = 0; i < nMethodInfos; i++)
								{
									curMethodInfo = methodInfos[i];

									//메소드들을 추가한다.
									newMenu.AddItem(new GUIContent(otherMono.GetType().Name + "/" + curMethodInfo.Name), false, OnMethodOfUnitEventSelected, new object[] { targetUnityEvent, methodSet, otherMono, curMethodInfo });
									isAnyMethod = true;
								}
							}
						}
					}
				}

				if(!isAnyMethod)
				{
					//Method가 하나도 없다면
					newMenu.AddItem(new GUIContent("<No Valid Method>"), false, OnMethodOfUnitEventSelected, null);
				}
			}

			newMenu.ShowAsContext();
			Event.current.Use();
		}



		//애니메이션 이벤트 > 콜백 방식인 경우 메소드를 찾아서 연결한다.
		private void OnMethodOfUnitEventSelected(object param)
		{
			if(param == null)
			{
				return;
			}
			if(!(param is object[]))
			{
				return;
			}
			object[] arrParams = param as object[];
			if(arrParams == null || arrParams.Length != 4)
			{
				return;
			}

			apUnityEvent targetEvent = arrParams[0] as apUnityEvent;
			apUnityEvent.TargetMethodSet targetMethodSet = arrParams[1] as apUnityEvent.TargetMethodSet;
			MonoBehaviour monoObj = arrParams[2] as MonoBehaviour;
			MethodInfo methodInfo = arrParams[3] as MethodInfo;

			//하나라도 null이면 종료
			if(targetEvent == null
				|| targetMethodSet == null
				|| monoObj == null
				|| methodInfo == null)
			{
				return;
			}

			Undo.IncrementCurrentGroup();
			Undo.RegisterCompleteObjectUndo(_targetPortrait, "Event Changed");

			targetMethodSet._target = monoObj;
			targetMethodSet._methodName = methodInfo.Name;

			apEditorUtil.SetEditorDirty();
			apEditorUtil.ReleaseGUIFocus();
			if(Event.current != null)
			{
				Event.current.Use();
			}
		}




		private MethodInfo GetMethodInfoOfEvent(apUnityEvent targetUnityEvent, apUnityEvent.TargetMethodSet methodSet)
		{
			if(methodSet._target == null || string.IsNullOrEmpty(methodSet._methodName))
			{
				return null;
			}

			System.Type type_Mono = methodSet._target.GetType();
			MethodInfo resultMI = null;
			try
			{
				resultMI = type_Mono.GetMethod(methodSet._methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			}
			catch(Exception)
			{
				//[v1.4.1] 에러가 발생하여 메소드를 찾을 수 없다.
				return null;
			}

			
			if(resultMI == null)
			{
				return null;
			}

			//리턴이 맞아야 한다. (void)
			if(!Type.Equals(resultMI.ReturnType, typeof(void)))
			{
				return null;
			}

			//파라미터 타입이 맞아야 한다.
			ParameterInfo[] paramInfos = resultMI.GetParameters();
			int nParamInfos = paramInfos != null ? paramInfos.Length : 0;

			switch (targetUnityEvent._unityEventType)
			{
				case apUnityEvent.UNITY_EVENT_TYPE.None:
					if(nParamInfos != 0)
					{
						return null;
					}
					break;

				case apUnityEvent.UNITY_EVENT_TYPE.Bool:
					if(nParamInfos != 1 || !Type.Equals(paramInfos[0].ParameterType, typeof(bool)))
					{
						return null;
					}
					break;

				case apUnityEvent.UNITY_EVENT_TYPE.Integer:
					if(nParamInfos != 1 || !Type.Equals(paramInfos[0].ParameterType, typeof(int)))
					{
						return null;
					}
					break;

				case apUnityEvent.UNITY_EVENT_TYPE.Float:
					if(nParamInfos != 1 || !Type.Equals(paramInfos[0].ParameterType, typeof(float)))
					{
						return null;
					}
					break;

				case apUnityEvent.UNITY_EVENT_TYPE.Vector2:
					if(nParamInfos != 1 || !Type.Equals(paramInfos[0].ParameterType, typeof(Vector2)))
					{
						return null;
					}
					break;

				case apUnityEvent.UNITY_EVENT_TYPE.String:
					if(nParamInfos != 1 || !Type.Equals(paramInfos[0].ParameterType, typeof(string)))
					{
						return null;
					}
					break;

				case apUnityEvent.UNITY_EVENT_TYPE.MultipleObjects:
					if(nParamInfos != 1 || !Type.Equals(paramInfos[0].ParameterType, typeof(object[])))
					{
						return null;
					}
					break;
			}

			return resultMI;
		}


		private List<MethodInfo> GetMethodInfosOfEvent(MonoBehaviour targetMono, apUnityEvent targetUnityEvent)
		{			
			System.Type type_Mono = targetMono.GetType();
			MethodInfo[] methods = type_Mono.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			//전체 Method 중에서 해당 유니티 이벤트에 맞는 유니티 이벤트를 찾자
			int nMethods = methods != null ? methods.Length : 0;
			if(nMethods == 0)
			{
				return null;
			}

			MethodInfo curMethod = null;
			MethodInfo otherMethod = null;
			
			//오버로딩은 제거한다.
			//이름이 똑같은 메서드는 조건에 상관없이 무조건 제외
			List<MethodInfo> uniqueMethods = new List<MethodInfo>();

			for (int iMethod = 0; iMethod < nMethods; iMethod++)
			{
				curMethod = methods[iMethod];
				//이름이 중복되진 않았는지 확인하자
				bool isAnySameName = false;

				for (int iOther = 0; iOther < nMethods; iOther++)
				{
					if(iMethod == iOther)
					{
						continue;
					}
					otherMethod = methods[iOther];

					if(string.Equals(curMethod.Name, otherMethod.Name))
					{
						//이름이 같은 함수가 있었다.
						isAnySameName = true;
						break;
					}
				}

				if(isAnySameName)
				{
					//동일한 이름의 메소드가 있다면 제외시킨다.
					continue;
				}
				
				uniqueMethods.Add(curMethod);
			}
			nMethods = uniqueMethods.Count;



			List<MethodInfo> resultMIs = new List<MethodInfo>();

			
			for (int iMethod = 0; iMethod < nMethods; iMethod++)
			{
				curMethod = uniqueMethods[iMethod];

				//리턴이 맞아야 한다. (void)
				if(!Type.Equals(curMethod.ReturnType, typeof(void)))
				{
					continue;
				}

				//파라미터 타입이 맞아야 한다.
				ParameterInfo[] paramInfos = curMethod.GetParameters();
				int nParamInfos = paramInfos != null ? paramInfos.Length : 0;

				bool isValidParam = false;
				switch (targetUnityEvent._unityEventType)
				{
					case apUnityEvent.UNITY_EVENT_TYPE.None:
						if(nParamInfos == 0)
						{
							isValidParam = true;
						}
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Bool:
						if(nParamInfos == 1 && Type.Equals(paramInfos[0].ParameterType, typeof(bool)))
						{
							isValidParam = true;
						}
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Integer:
						if(nParamInfos == 1 && Type.Equals(paramInfos[0].ParameterType, typeof(int)))
						{
							isValidParam = true;
						}
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Float:
						if(nParamInfos == 1 && Type.Equals(paramInfos[0].ParameterType, typeof(float)))
						{
							isValidParam = true;
						}
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.Vector2:
						if(nParamInfos == 1 && Type.Equals(paramInfos[0].ParameterType, typeof(Vector2)))
						{
							isValidParam = true;
						}
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.String:
						if(nParamInfos == 1 && Type.Equals(paramInfos[0].ParameterType, typeof(string)))
						{
							isValidParam = true;
						}
						break;

					case apUnityEvent.UNITY_EVENT_TYPE.MultipleObjects:
						if(nParamInfos == 1 && Type.Equals(paramInfos[0].ParameterType, typeof(object[])))
						{
							isValidParam = true;
						}
						break;
				}


				if(isValidParam)
				{
					resultMIs.Add(curMethod);
				}
			}

			return resultMIs;
		}


		private void RecordUndo()
		{
			if(_targetPortrait != null)
			{
				Undo.RecordObject(_targetPortrait, "Portrait Changed");
				EditorUtility.SetDirty(_targetPortrait);
			}
			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
			
			
		}

		








		////Event 방식 처리를 위한 배열 UI용 서브 클래스
		//public class EventMethodNames
		//{
		//	//연결된 유니티 이벤트
		//	public apUnityEvent linkedUnityEvent = null;

		//	//해당 Mono의 
		//	public MonoBehaviour targetMono = null;
		//	public MethodInfo[] methodInfos = null;
		//	public string[] methodNames = null;
		//}
	}


	
	

}