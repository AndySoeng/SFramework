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
//using UnityEngine.Profiling;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	// apEditor의 멤버만 모은 스크립트 소스
	// 간단한 Get/Set도 포함되어 있다.
	public partial class apEditor : EditorWindow
	{
		//--------------------------------------------------------------------------------
		// 에디터 핵심 객체
		//--------------------------------------------------------------------------------
		private apSelection _selection = null;
		public apSelection Select { get { return _selection; } }


		private apEditorController _controller = null;
		public apEditorController Controller
		{
			get
			{
				if (_controller == null)
				{
					_controller = new apEditorController();
					_controller.SetEditor(this);
				}
				return _controller;
			}
		}

		private apVertexController _vertController = new apVertexController();
		public apVertexController VertController { get { return _vertController; } }

		private apGizmoController _gizmoController = new apGizmoController();
		public apGizmoController GizmoController { get { return _gizmoController; } }

		public apController ParamControl
		{
			get
			{
				if (_portrait == null) { return null; }
				else { return _portrait._controller; }
			}
		}

		// 기즈모
		private apGizmos _gizmos = null;
		public apGizmos Gizmos { get { return _gizmos; } }


		private apImageSet _imageSet = null;
		public apImageSet ImageSet { get { return _imageSet; } }


		// Hierarchy
		private apEditorHierarchy _hierarchy = null;
		public apEditorHierarchy Hierarchy { get { return _hierarchy; } }

		private apEditorMeshGroupHierarchy _hierarchy_MeshGroup = null;
		public apEditorMeshGroupHierarchy Hierarchy_MeshGroup { get { return _hierarchy_MeshGroup; } }

		private apEditorAnimClipTargetHierarchy _hierarchy_AnimClip = null;
		public apEditorAnimClipTargetHierarchy Hierarchy_AnimClip { get { return _hierarchy_AnimClip; } }


		// 입력 멤버들
		private apHotKey _hotKey = new apHotKey();
		public apHotKey HotKey { get { return _hotKey; } }

		//추가 20.11.30
		private apHotKeyMapping _hotKeyMap = new apHotKeyMapping();
		public apHotKeyMapping HotKeyMap { get { return _hotKeyMap; } }


		private apMouseSet _mouseSet = new apMouseSet();
		public apMouseSet Mouse { get { return _mouseSet; } }

		//--------------------------------------------------------------------------------
		// 에디터 편의 기능 요소
		//--------------------------------------------------------------------------------



		private apPhysicsPreset _physicsPreset = new apPhysicsPreset();
		public apPhysicsPreset PhysicsPreset { get { return _physicsPreset; } }

		//추가 22.6.13 : 애니메이션 이벤트 프리셋
		private apAnimEventPreset _animEventPreset = new apAnimEventPreset();
		public apAnimEventPreset AnimEventPreset { get { return _animEventPreset; } }

		private apControlParamPreset _controlParamPreset = new apControlParamPreset();
		public apControlParamPreset ControlParamPreset { get { return _controlParamPreset; } }

		private apExporter _exporter = null;
		public apExporter Exporter { get { if (_exporter == null) { _exporter = new apExporter(this); } return _exporter; } }

		private apSequenceExporter _sequenceExporter = null;
		public apSequenceExporter SeqExporter { get { if (_sequenceExporter == null) { _sequenceExporter = new apSequenceExporter(this); } return _sequenceExporter; } }

		private apLocalization _localization = new apLocalization();
		public apLocalization Localization { get { return _localization; } }


		private apBackup _backup = new apBackup();
		public apBackup Backup { get { return _backup; } }


		//추가 20.12.9 : 메시 생성기 V2
		private apMeshGeneratorV2 _meshGeneratorV2 = null;
		public apMeshGeneratorV2 MeshGeneratorV2 { get { if (_meshGeneratorV2 == null) { _meshGeneratorV2 = new apMeshGeneratorV2(this); } return _meshGeneratorV2; } }

		private apMaterialLibrary _materialLibrary = null;
		public apMaterialLibrary MaterialLibrary { get { return _materialLibrary; } }

		//[v1.4.2] 프로젝트 설정 데이터 (Bake 다이얼로그의 설정들)
		private apProjectSettingData _projectSettingData = null;
		public apProjectSettingData ProjectSettingData { get { return _projectSettingData; } }


		//--------------------------------------------------------------------------------
		// 화면 구성 요소
		//--------------------------------------------------------------------------------
		private apOnion _onion = new apOnion();
		public apOnion Onion { get { return _onion; } }


		//추가 20.4.13 : 오브젝트의 Show/Hide를 저장하고 열 수 있는 객체
		private apVisiblityController _visibilityController = null;
		public apVisiblityController VisiblityController { get { if (_visibilityController == null) { _visibilityController = new apVisiblityController(); } return _visibilityController; } }


		//추가 21.1.27 : 현재 선택된 VisibilityPreset과 Rule
		public bool _isAdaptVisibilityPreset = false;
		public apVisibilityPresets.RuleData _selectedVisibilityPresetRule = null;

		//추가 21.2.27
		private apRotoscoping _rotoscoping = new apRotoscoping();
		public apRotoscoping Rotoscoping { get { return _rotoscoping; } }

		public bool _isEnableRotoscoping = false;
		public apRotoscoping.ImageSetData _selectedRotoscopingData = null;
		public int _iRotoscopingImageFile = 0;
		//애니메이션과 동기화하여 로토스코핑을 제공하는 경우 : 마지막으로 동기화된 AnimClip 프레임과 동기화 여부 (동기화 이후에는 마음대로 이미지를 변경할 수 있다)
		private int _iSyncRotoscopingAnimClipFrame = -1;
		private bool _isSyncRotoscopingToAnimClipFrame = false;

		//추가 21.6.4 : 가이드라인
		public bool _isEnableGuideLine = false;



		//추가 21.1.18 : 제너릭 메뉴를 관리하는 클래스와 버튼들
		private apGUIMenu _guiMenu = null;
		public apGUIMenu GUIMenu { get { if (_guiMenu == null) { _guiMenu = new apGUIMenu(this); } return _guiMenu; } }

		private apGUIButton _guiButton_Menu = null;
		private apGUIButton _guiButton_RecordOnion = null;

		//추가 22.3.20 [v1.4.0]
		//Morph 모디파이어 편집시 Pin <-> Vertex 편집 모드를 변경하는 GUI Button
		private apGUIButton _guiButton_MorphEditVert = null;
		private apGUIButton _guiButton_MorphEditPin = null;

		//추가 21.2.18 : 현재 상태를 아이콘으로 통합적으로 표시하는 객체
		private apGUIStatBox _guiStatBox = null;
		private apGUIHowToUseTips _guiHowToUse = null;

		/// <summary>GUI의 정보과 Workspace의 가장자리와의 여백</summary>
		private const int GUI_STAT_MARGIN = 10;
		private const int GUI_STAT_ICON_SIZE = 28;
		private const int GUI_STAT_MENUBTN_SIZE = 32;
		private const int GUI_STAT_MENUBTN_MARGIN_SAMEGROUP = 2;//메뉴 버튼간의 간격 (같은 그룹일때)
		private const int GUI_STAT_MENUBTN_MARGIN_DIFGROUP = 6;//메뉴 버튼간의 간격 (다른 그룹일때)


		private apGUIRenderSettings _guiRenderSettings = null;
		public apGUIRenderSettings GUIRenderSettings { get { return _guiRenderSettings; } }


		//--------------------------------------------------------------------------------
		// GUI Repaint / 업데이트 관련 멤버
		//--------------------------------------------------------------------------------


		//----------------------------------------------------------------------------------
		// 에디터 환경 설정
		//----------------------------------------------------------------------------------
		//이전 버전에서의 Top 레이아웃의 View 버튼들을 보여줄지 여부
		public bool _option_ShowPrevViewMenuBtns = false;

		// 언어 설정
		public enum LANGUAGE
		{
			/// <summary>영어</summary>
			English = 0,
			/// <summary>한국어</summary>
			Korean = 1,
			/// <summary>프랑스어</summary>
			French = 2,
			/// <summary>독일어</summary>
			German = 3,
			/// <summary>스페인어</summary>
			Spanish = 4,
			/// <summary>이탈리아어</summary>
			Italian = 5,
			/// <summary>덴마크어</summary>
			Danish = 6,
			/// <summary>일본어</summary>
			Japanese = 7,
			/// <summary>중국어-번체</summary>
			Chinese_Traditional = 8,
			/// <summary>중국어-간체</summary>
			Chinese_Simplified = 9,
			/// <summary>폴란드어</summary>
			Polish = 10,
		}

		public LANGUAGE _language = LANGUAGE.English;



		//색상 옵션
		public Color _colorOption_Background = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		public Color _colorOption_GridCenter = new Color(0.7f, 0.7f, 0.3f, 1.0f);
		public Color _colorOption_Grid = new Color(0.3f, 0.3f, 0.3f, 1.0f);

		//추가 21.10.6 : 반전된 색상
		public Color _colorOption_InvertedBackground = new Color(0.9f, 0.9f, 0.9f, 1.0f);


		public Color _colorOption_MeshEdge = new Color(1.0f, 0.5f, 0.0f, 0.9f);
		public Color _colorOption_MeshHiddenEdge = new Color(1.0f, 1.0f, 0.0f, 0.7f);
		public Color _colorOption_Outline = new Color(0.0f, 0.5f, 1.0f, 0.7f);
		public Color _colorOption_TransformBorder = new Color(0.0f, 1.0f, 1.0f, 1.0f);
		public Color _colorOption_AtlasBorder = new Color(0.0f, 1.0f, 1.0f, 0.5f);

		public Color _colorOption_VertColor_NotSelected = new Color(0.0f, 0.85f, 1.0f, 0.95f);
		public Color _colorOption_VertColor_Selected = new Color(1.0f, 0.0f, 0.0f, 1.0f);

		public Color _colorOption_GizmoFFDLine = new Color(1.0f, 0.5f, 0.2f, 0.9f);
		public Color _colorOption_GizmoFFDInnerLine = new Color(1.0f, 0.7f, 0.2f, 0.7f);

		public Color _colorOption_OnionToneColor = new Color(0.1f, 0.43f, 0.5f, 0.7f);
		public Color _colorOption_OnionAnimPrevColor = new Color(0.5f, 0.2f, 0.1f, 0.7f);
		public Color _colorOption_OnionAnimNextColor = new Color(0.1f, 0.5f, 0.2f, 0.7f);
		public Color _colorOption_OnionBoneColor = new Color(0.4f, 1.0f, 1.0f, 0.9f);
		public Color _colorOption_OnionBonePrevColor = new Color(1.0f, 0.6f, 0.3f, 0.9f);
		public Color _colorOption_OnionBoneNextColor = new Color(0.3f, 1.0f, 0.6f, 0.9f);

		public bool _guiOption_isFPSVisible = true;
		public bool _guiOption_isStatisticsVisible = false;

		public bool _guiOption_isShowHowToEdit = true;

		//Onion 옵션
		public bool _onionOption_IsOutlineRender = true;//False일 때는 Solid 렌더
		public float _onionOption_OutlineThickness = 0.5f;
		public bool _onionOption_IsRenderOnlySelected = false;
		public bool _onionOption_IsRenderBehind = false;//뒤에 렌더링하기. false일 때에는 앞쪽에 렌더링
		public bool _onionOption_IsRenderAnimFrames = false;//True이면 마커가 아닌 프레임 단위로 렌더링
		public int _onionOption_PrevRange = 1;
		public int _onionOption_NextRange = 1;
		public int _onionOption_RenderPerFrame = 1;
		public float _onionOption_PosOffsetX = 0.0f;
		public float _onionOption_PosOffsetY = 0.0f;
		public bool _onionOption_IKCalculateForce = false;




		//추가 v1.4.2 : 버텍스 크기
		private int[] _vertGUISizeList_X100 = new int[]		{ 50, 80, 100, 120, 140, 160, 180, 200, 250, 300, 350, 400, 450, 500 };
		public string[] _vertGUISizeNameList = new string[] { "50%", "80%", "100%", "120%", "140%", "160%", "180%", "200%", "250%", "300%", "350%", "400%", "450%", "500%"};
		public const int VERT_GUI_SIZE_INDEX__DEFAULT = 2;
		public const int VERT_GUI_SIZE_INDEX__MIN = 0;
		public const int VERT_GUI_SIZE_INDEX__MAX = 13;
		public int _vertGUIOption_SizeRatio_Index = VERT_GUI_SIZE_INDEX__DEFAULT;

		/// <summary>GUI 상에서의 버텍스 크기 비율 (% 단위이며 기본 100)</summary>
		public int VertGUIOption_SizeRatioX100
		{
			get { return _vertGUISizeList_X100[Mathf.Clamp(_vertGUIOption_SizeRatio_Index, VERT_GUI_SIZE_INDEX__MIN, VERT_GUI_SIZE_INDEX__MAX)]; }
		}


		//추가 20.3.20 : 본 렌더링 옵션
		public enum BONE_DISPLAY_METHOD
		{
			Version1 = 0,
			Version2 = 1
		}

		private int[] _boneRigSizeRatioList_X100 = new int[]{ 40, 60, 80, 100, 120, 140, 160, 180, 200 };
		public string[] _boneRigSizeNameList = new string[]{ "40%", "60%", "80%", "100%", "120%", "140%", "160%", "180%", "200%" };
		public const int BONE_RIG_SIZE_INDEX__MIN = 0;
		public const int BONE_RIG_SIZE_INDEX__MAX = 8;
		public const int BONE_RIG_SIZE_INDEX__DEFAULT = 3;
		public const int BONE_RIG_SIZE_INDEX__DEFAULT_SELECTED = 4;

		public BONE_DISPLAY_METHOD _boneGUIOption_RenderType = BONE_DISPLAY_METHOD.Version2;//<<기본 값이 v2이다.

		public enum NEW_BONE_COLOR
		{
			SimilarColor = 0,
			DifferentColor = 1,
		}

		public enum NEW_BONE_PREVIEW : int
		{
			Line = 0,
			Ghost = 1
		}

		//본의 크기. 주로 원점의 Radius와 Width
		public int _boneGUIOption_SizeRatio_Index = BONE_RIG_SIZE_INDEX__DEFAULT;//크기. 인덱스로 저장한다.
		public bool _boneGUIOption_ScaledByZoom = false;//화면 확대에 따라 크기가 바뀌는가. 기존엔 true. 개선 후엔 false가 기본 값이다.
		public NEW_BONE_COLOR _boneGUIOption_NewBoneColor = NEW_BONE_COLOR.SimilarColor;
		public NEW_BONE_PREVIEW _boneGUIOption_NewBonePreview = NEW_BONE_PREVIEW.Line;
		


		public int BoneGUIOption_SizeRatioX100
		{
			get
			{
				_boneGUIOption_SizeRatio_Index = Mathf.Clamp(_boneGUIOption_SizeRatio_Index, BONE_RIG_SIZE_INDEX__MIN, BONE_RIG_SIZE_INDEX__MAX);
				return _boneRigSizeRatioList_X100[_boneGUIOption_SizeRatio_Index];
			}
		}



		public enum RIG_SELECTED_WEIGHT_GUI_TYPE
		{
			None = 0,
			Enlarged = 1,
			Flashing = 2,
			EnlargedAndFlashing = 3,
		}

		public enum NOLINKED_BONE_VISIBILITY : int
		{
			Opaque = 0,//불투명
			Translucent = 1,//반투명 [기본값]
			Hidden = 2,//안보임
		}


		public enum RIG_WEIGHT_GRADIENT_COLOR
		{
			Default = 0,//기존의 방식 [기본값]
			Vivid = 1,//채도가 높은 방식
		}

		//추가 20.3.20 리깅 GUI 관련 옵션
		public int _rigGUIOption_VertRatio_Index = BONE_RIG_SIZE_INDEX__DEFAULT;//원형 버텍스의 크기
		public bool _rigGUIOption_ScaledByZoom = false;//화면 확대에 따라 크기가 바뀌는가
		public int _rigGUIOption_VertRatio_Selected_Index = BONE_RIG_SIZE_INDEX__DEFAULT_SELECTED;//선택된 원형 버텍스의 크기
		public RIG_SELECTED_WEIGHT_GUI_TYPE _rigGUIOption_SelectedWeightGUIType = RIG_SELECTED_WEIGHT_GUI_TYPE.EnlargedAndFlashing;
		public NOLINKED_BONE_VISIBILITY _rigGUIOption_NoLinkedBoneVisibility = NOLINKED_BONE_VISIBILITY.Translucent;
		public RIG_WEIGHT_GRADIENT_COLOR _rigGUIOption_WeightGradientColor = RIG_WEIGHT_GRADIENT_COLOR.Default;

		public int RigGUIOption_SizeRatioX100
		{
			get
			{
				_rigGUIOption_VertRatio_Index = Mathf.Clamp(_rigGUIOption_VertRatio_Index, BONE_RIG_SIZE_INDEX__MIN, BONE_RIG_SIZE_INDEX__MAX);
				return _boneRigSizeRatioList_X100[_rigGUIOption_VertRatio_Index];
			}
		}

		public int RigGUIOption_SizeRatioX100_Selected
		{
			get
			{
				_rigGUIOption_VertRatio_Selected_Index = Mathf.Clamp(_rigGUIOption_VertRatio_Selected_Index, BONE_RIG_SIZE_INDEX__MIN, BONE_RIG_SIZE_INDEX__MAX);
				return _boneRigSizeRatioList_X100[_rigGUIOption_VertRatio_Selected_Index];
			}
		}



		//백업 옵션
		//자동 백업 옵션 처리
		public bool _backupOption_IsAutoSave = true;//자동 백업을 지원하는가
		public string _backupOption_BaseFolderName = "AnyPortraitBackup";//폴더를 지정해야한다. (프로젝트 폴더 기준 + 씬이름+에셋)
		public int _backupOption_Minute = 30;//기본은 30분마다 한번씩 저장한다.

		public string _bonePose_BaseFolderName = "AnyPortraitBonePose";


		//시작 화면 옵션
		//매번 시작할 것인가
		//마지막으로 열린 날짜 (날짜가 바뀌면 열린다.)
		public bool _startScreenOption_IsShowStartup = true;
		public int _startScreenOption_LastMonth = 0;
		public int _startScreenOption_LastDay = 0;

		public int _updateLogScreen_LastVersion = 0;

		//추가 19.12.25 : Mac OSX 안내 옵션
		//이 옵션이 켜진 상태에서는 Mac OSX일 경우의 팁이 항상 먼저 켜진다
		public bool _macOSXInfoScreenOption_IsShowStartup = true;
		public int _macOSXInfoScreenOption_LastMonth = 0;
		public int _macOSXInfoScreenOption_LastDay = 0;


		//>>> [삭제 1.4.2] Project Settings에 붙인다.
		////Bake시 Color Space를 어디에 맞출 것인가
		//public bool _isBakeColorSpaceToGamma = true;

		////RenderPipeline 옵션
		//public bool _isUseSRP = false;

		//Modifier Lock 옵션
		//변경 21.2.13 : ModLock/Unlock 개념이 사라지고, 단일 옵션만 남는다. 일부 옵션은 ExModObjOption으로 변경
		public bool _modLockOption_ColorPreview = false;//색상 미리보기
		public bool _modLockOption_BoneResultPreview = false;//본 결과 미리보기
		public bool _modLockOption_ModListUI = false;//모디파이어 리스트 미리보기

		public Color _modLockOption_BonePreviewColor = new Color(1.0f, 0.8f, 0.1f, 0.8f);


		
		//추가 3.22 : "선택 잠금" 옵션
		public bool _isSelectionLockOption_RiggingPhysics = true;
		public bool _isSelectionLockOption_Morph = true;
		public bool _isSelectionLockOption_Transform = true;
		public bool _isSelectionLockOption_ControlParamTimeline = true;


		//추가 21.2.10 : "모디파이어에 등록되지 않은 객체"에 대한 처리에 관한 옵션
		public bool _exModObjOption_UpdateByOtherMod = false;
		public bool _exModObjOption_ShowGray = false;
		public bool _exModObjOption_NotSelectable = false;





		//에디터가 유휴 상태일때는 프레임을 낮추자
		public bool _isLowCPUOption = false;
		
		//CPU 저하 옵션
		public enum LOW_CPU_STATUS
		{
			None,//Option이 꺼져있다.
			Full,//Option이 켜져있지만 해당 안됨
			LowCPU_Mid,//업데이트가 조금 제한된다.
			LowCPU_Low,//업데이트가 많이 제한된다.
		}

		private LOW_CPU_STATUS _lowCPUStatus = LOW_CPU_STATUS.None;
		//private Texture2D _imgLowCPUStatus = null;


		//추가 21.5.13 : C++ 플러그인을 활용할 수 있다.
		//DLL 유효성 검사 결과를 저장한다. (패키지 설치 등을 해야할 수 있다.)
		public apPluginUtil.VALIDATE_RESULT _cppPluginValidateResult = apPluginUtil.VALIDATE_RESULT.Unknown;
		public bool _cppPluginOption_UsePlugin = false;//플러그인을 사용한다. (단 유효성 검사를 통과해야한다.) 기본값 false : 설치해야하는게 문제가 된다.
		public bool IsUseCPPDLL { get { return _cppPluginOption_UsePlugin && _cppPluginValidateResult == apPluginUtil.VALIDATE_RESULT.Valid; } }
		//public bool IsUseCPPDLL { get { return false; } }


		//추가 3.29 : Ambient 자동 보정 옵션
		public bool _isAmbientCorrectionOption = true;

		//추가 19.6.28 : 자동으로 Controller 탭으로 전환할 지 여부 옵션 (Mod, AnimClip)
		public bool _isAutoSwitchControllerTab_Mod = true;
		public bool _isAutoSwitchControllerTab_Anim = false;

		//추가 19.6.28 : 메시의 작업용 보이기/숨기기를 작업 끝날때 자동으로 복원하기
		public bool _isRestoreTempMeshVisibilityWhenTaskEnded = true;

		//추가 20.7.6 : PSD 파일로부터 임포트를 할 때, 메시를 선택하면 버텍스를 삭제할지 물어보기
		//(기본값 : false)
		public bool _isNeedToAskRemoveVertByPSDImport = false;

		//추가 21.3.6 : 메시 생성시, 이미지가 한개면 자동으로 설정하기 (기본값 True)
		public bool _option_SetAutoImageToMeshIfOnlyOneImageExist = true;

		//추가 22.1.6 : URP 메시지 계속 보이기
		public bool _isShowURPWarningMsg = true;

		//추가 22.1.7 : Bake시 URP에 의한 렌더 파이프라인 옵션 체크
		public bool _option_CheckSRPWhenBake = true;


		//추가 22.4.17 : 핀 설정시, 가중치가 자동으로 갱신되게 만드는 옵션.
		public bool _pinOption_AutoWeightRefresh = true;


		//추가 22.5.18 : 컨트롤 파라미터 UI (Vector 타입)
		public enum CONTROL_PARAM_UI_SIZE_OPTION : int
		{
			Default = 0,
			Large = 1,//<추가됨. Vector 2 타입의 UI가 상하로 길어진다.
		}
		public CONTROL_PARAM_UI_SIZE_OPTION _controlParamUISizeOption = CONTROL_PARAM_UI_SIZE_OPTION.Default;


		//추가 22.7.13
		//가시성 프리셋을 매번 초기화할 지 여부
		public bool _option_TurnOffVisibilityPresetWhenSelectObject = false;


		//추가 21.3.7 : AutoKey를 초기화하는 옵션을 주고, 이게 Off인 경우 마지막 값을 유지한다. (그 값은 EdtiorPref에 저장한다. 별도의 변수로 두지 않음)
		public bool _option_IsTurnOffAnimAutoKey = true;

		//[1.4.2] 오브젝트 선택시 자동 스크롤
		public bool _option_AutoScrollWhenObjectSelected = true;

		//[1.4.2] 기즈모를 클릭하지 않아도 바로 이동 시킬 수 있는 옵션 (기본값 true)
		//원래 기본값이었던 기능인데 끌 수 있도록 옵션으로 만들었다.
		public bool _option_ObjMovableWithoutClickGizmo = true;


		//----------------------------------------------------------------------------------
		// 에디터 UI 변수
		//----------------------------------------------------------------------------------
		//컨트롤 파라미터 보여주는 필터
		public apControlParam.CATEGORY _curParamCategory = apControlParam.CATEGORY.Head |
															apControlParam.CATEGORY.Body |
															apControlParam.CATEGORY.Face |
															apControlParam.CATEGORY.Hair |
															apControlParam.CATEGORY.Equipment |
															apControlParam.CATEGORY.Force |
															apControlParam.CATEGORY.Etc;

		//왼쪽 탭
		public enum TAB_LEFT { Hierarchy = 0, Controller = 1 }
		private TAB_LEFT _tabLeft = TAB_LEFT.Hierarchy;
		public TAB_LEFT LeftTab { get { return _tabLeft; } }

		
		//Hierarchy 필터
		[Flags]
		public enum HIERARCHY_FILTER
		{
			None = 0,
			RootUnit = 1,
			Image = 2,
			Mesh = 4,
			MeshGroup = 8,
			Animation = 16,
			Param = 32,
			All = 63
		}
		public HIERARCHY_FILTER _hierarchyFilter = HIERARCHY_FILTER.All;



		// 에디터의 최신 레이아웃 크기 (자동 스크롤용)
		//private int _lastWindowWidth = 0;
		private int _lastWindowHeight = 0;
		private int _lastUIHeight_Right1Lower = 0;//오른쪽 하단 리스트

		//스크롤
		private Vector2 _scroll_Left_FirstPage = Vector2.zero;//왼쪽 : Portrait가 선택되기 전의 화면
		private Vector2 _scroll_Left_Hierarchy = Vector2.zero;//왼쪽 : Hierarchy 탭
		private Vector2 _scroll_Left_Controller = Vector2.zero;//왼쪽 : 컨트롤 파라미터들

		public Vector2 _scroll_CenterWorkSpace = Vector2.zero;//<<이건 사용하는 곳이 많아서 Public
		private Vector2 _scroll_Right1_Upper = Vector2.zero;
		private Vector2 _scroll_Right2 = Vector2.zero;
		//private Vector2 _scroll_Bottom = Vector2.zero;

		private Vector2 _scroll_Right1_Lower_MG_Mesh = Vector2.zero;
		private Vector2 _scroll_Right1_Lower_MG_Bone = Vector2.zero;
		private Vector2 _scroll_Right1_Lower_Anim_Mesh = Vector2.zero;
		private Vector2 _scroll_Right1_Lower_Anim_Bone = Vector2.zero;
		private Vector2 _scroll_Right1_Lower_Anim_ControlParam = Vector2.zero;

		private Vector2 _scroll_Right1_Lower_Dummy = Vector2.zero;//외부 입력에 따라 스크롤이 바뀌는데, 이벤트 타이밍상 더미가 필요하다


		public enum RIGHT_LOWER_SCROLL_TYPE
		{
			MeshGroup_Mesh,
			MeshGroup_Bone,
			Anim_Mesh,
			Anim_Bone,
			Anim_ControlParam
		}


		//GUI 줌
		public int _iZoomX100 = 36;//36 => 100
		public const int ZOOM_INDEX_DEFAULT = 36;
		public int[] _zoomListX100 = new int[] {    4,  6,  8,  10, 12, 14, 16, 18, 20, 22, //9
													24, 26, 28, 30, 32, 34, 36, 38, 40, 42, //19
													44, 46, 48, 50, 52, 54, 56, 58, 60, 65, //29
													70, 75, 80, 85, 90, 95, 100, //39
													105, 110, 115, 120, 125, 130, 140, 150, 160, 180, 200,
													220, 240, 260, 280, 300, 350, 400, 450,
													500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000,
													2100, 2200, 2300, 2400, 2500, 2600, 2700, 2800, 2900, 3000 };

		public string[] _zoomListX100_Label = new string[] {    "4%",  "6%",  "8%",  "10%", "12%", "14%", "16%", "18%", "20%", "22%",
																"24%", "26%", "28%", "30%", "32%", "34%", "36%", "38%", "40%", "42%",
																"44%", "46%", "48%", "50%", "52%", "54%", "56%", "58%", "60%", "65%",
																"70%", "75%", "80%", "85%", "90%", "95%", "100%",
																"105%", "110%", "115%", "120%", "125%", "130%", "140%", "150%", "160%", "180%", "200%",
																"220%", "240%", "260%", "280%", "300%", "350%", "400%", "450%",
																"500%", "600%", "700%", "800%", "900%", "1000%", "1100%", "1200%", "1300%", "1400%", "1500%", "1600%", "1700%", "1800%", "1900%", "2000%",
																"2100%", "2200%", "2300%", "2400%", "2500%", "2600%", "2700%", "2800%", "2900%", "3000%" };


		public Color _guiMainEditorColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		public Color _guiSubEditorColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

		//재질들
		public Material _mat_Color = null;
		public Material _mat_GUITexture = null;
		public Material[] _mat_Texture_Normal = null;//기본 White * Multiply (VColor 기본값이 White)
		public Material[] _mat_Texture_VertAdd = null;//Weight가 가능한 Add Vertex Color (VColor 기본값이 Black)
													  
		public Material[] _mat_Clipped = null;
		public Material _mat_MaskOnly = null;
		//Onion을 위한 ToneColor
		public Material _mat_ToneColor_Normal = null;
		public Material _mat_ToneColor_Clipped = null;
		public Material _mat_Alpha2White = null;
		public Material _mat_BoneV2 = null;
		public Material _mat_Texture_VColorMul = null;
		public Material _mat_RigCircleV2 = null;
		public Material _mat_Gray_Normal = null;
		public Material _mat_Gray_Clipped = null;
		public Material _mat_VertPin = null;


		// 본/메시 렌더링 여부 옵션들

		public enum BONE_RENDER_MODE { None, Render, RenderOutline, }
		
		/// <summary>Bone을 렌더링 하는가</summary>
		public BONE_RENDER_MODE _boneGUIRenderMode = BONE_RENDER_MODE.Render;

		public enum MESH_RENDER_MODE { None, Render, }
		public MESH_RENDER_MODE _meshGUIRenderMode = MESH_RENDER_MODE.Render;

		[Flags]
		public enum BONE_RENDER_TARGET
		{
			None = 0,
			AllBones = 1,//기본 본 렌더링
			SelectedOnly = 2,//기본 본은 렌더링 안하고 선택한 본만 렌더링 하는 경우
			SelectedOutline = 4,//선택한 본의 붉은 아웃라인을 표시하고자 하는 경우
			Default = 1 | 4
		}


		//추가 19.7.29 : Blur 브러시 인자를 Gizmo에서 Editor로 옮김
		public bool _blurEnabled = false;
		//public int _blurRadius = 50;//이전
		public int _blurRadiusIndex = apGizmos.DEFAULT_BRUSH_INDEX;//변경 22.1.9 : 인덱스 방식
		public int _blurIntensity = 50;



		//추가 19.7.31 : 리깅 GUI에 대한 옵션. 일부는 Selection에 있는 값을 가져왔다.
		public bool _rigViewOption_WeightOnly = false;
		public bool _rigViewOption_BoneColor = true;
		public bool _rigViewOption_CircleVert = false;


		//상단 버튼 탭 상태
		private enum GUITOP_TAB
		{
			Tab1_BakeAndSetting,
			Tab2_TRSTools,
			Tab3_Visibility,
			Tab4_FFD_Soft_Blur,
			Tab5_GizmoValue,
			Tab6_Capture
		}
		private Dictionary<GUITOP_TAB, bool> _guiTopTabStaus = new Dictionary<GUITOP_TAB, bool>();



		//각 화면 레이아웃의 숨기기 기능
		//추가 19.8.17 : UI의 숨기기 기능을 통합한다.
		// UI 숨기기 버튼 누른 결과 이벤트
		public enum UI_FOLD_BTN_RESULT
		{
			None,
			ToggleFold_Horizontal,
			ToggleFold_Vertical,
		}

		// UI가 어떻게 숨겨져있는지 여부
		public enum UI_FOLD_TYPE
		{
			Unfolded,
			Folded,
		}

		private UI_FOLD_TYPE _uiFoldType_Left = UI_FOLD_TYPE.Unfolded;
		private UI_FOLD_TYPE _uiFoldType_Right1 = UI_FOLD_TYPE.Unfolded;
		private UI_FOLD_TYPE _uiFoldType_Right1_Upper = UI_FOLD_TYPE.Unfolded;
		private UI_FOLD_TYPE _uiFoldType_Right1_Lower = UI_FOLD_TYPE.Unfolded;
		private UI_FOLD_TYPE _uiFoldType_Right2 = UI_FOLD_TYPE.Unfolded;



		///전체화면 기능
		public bool _isFullScreenGUI = false;//<<기본값은 false이다.

		//추가 21.10.6 : 배경 색상 반전
		public bool _isInvertBackgroundColor = false;



		//추가 3.28 : 메인 Hierarchy의 SortMode 기능
		//Hierarchy의 순서를 바꿀 수 있다.
		//SortMode를 켠 상태에서
		//- SortMode를 직접 끄거나
		//- 어떤 항목을 선택하거나
		//- Portrait가 바뀌거나
		//- 에디터가 리셋 될때
		//SortMode는 꺼진다.
		private bool _isHierarchyOrderEditEnabled = false;
		public void TurnOffHierarchyOrderEdit() { _isHierarchyOrderEditEnabled = false; }

		public enum HIERARCHY_SORT_MODE
		{
			RegOrder = 0,//등록된 순서
			AlphaNum = 1,//이름 순서
			Custom = 2,//변경된 순서
		}
		private HIERARCHY_SORT_MODE _hierarchySortMode = HIERARCHY_SORT_MODE.RegOrder;
		public HIERARCHY_SORT_MODE HierarchySortMode { get { return _hierarchySortMode; } }
		public void SetHierarchySortMode(HIERARCHY_SORT_MODE sortMode)
		{
			_hierarchySortMode = sortMode;
			SaveEditorPref();
			RefreshControllerAndHierarchy(false);
		}



		// Timeline GUI
		private apAnimClip _prevAnimClipForTimeline = null;
		private List<apTimelineLayerInfo> _timelineInfoList = new List<apTimelineLayerInfo>();
		public List<apTimelineLayerInfo> TimelineInfoList { get { return _timelineInfoList; } }

		public enum TIMELINE_INFO_SORT
		{
			Registered,//등록 순서대로..
			ABC,//가나다 순
			Depth,//깊이 순서대로 (RenderUnit 한정)
		}
		public TIMELINE_INFO_SORT _timelineInfoSortType = TIMELINE_INFO_SORT.Registered;//<<이게 기본값 (저장하자)
		



		public Rect _mainGUIRect = new Rect();


		
		// 추가 22.3.3 (v1.4.0)
		//GL에 보낼 렌더 타입 변수가 인스턴스로 변경됨
		private apGL.RenderTypeRequest _renderRequest_Normal = null;
		private apGL.RenderTypeRequest _renderRequest_Selected = null;


		//딜레이되어 동작하는 Window 호출 요청
		private enum DIALOG_SHOW_CALL { None, Setting, Bake, Capture, }
		private DIALOG_SHOW_CALL _dialogShowCall = DIALOG_SHOW_CALL.None;
		private EventType _curEventType = EventType.Ignore;




		// 프레임 업데이트 변수
		private enum FRAME_TIMER_TYPE
		{
			Update, Repaint, None
		}
		
		public float DeltaTime_UpdateAllFrame { get { return apTimer.I.DeltaTime_UpdateAllFrame; } }
		public float DeltaTime_Repaint { get { return apTimer.I.DeltaTime_Repaint; } }

		//변경 19.11.23 : 별도의 클래스를 이용
		private apFPSCounter _fpsCounter = new apFPSCounter();

		//private System.Text.StringBuilder _sb_FPSText = new System.Text.StringBuilder(16);
		private apStringWrapper _fpsString = null;
		private const string TEXT_FPS = "FPS ";



		// 단축키 관련 변수
		private bool _isHotKeyProcessable = true;
		private bool _isHotKeyEvent = false;
		private KeyCode _hotKeyCode = KeyCode.A;
		private bool _isHotKey_Ctrl = false;
		private bool _isHotKey_Alt = false;
		private bool _isHotKey_Shift = false;



		// 마지막으로 클릭한게 Hierarchy인가?를 판별하는 변수들
		// 마우스 클릭 이벤트에서 Hierarchy를 클릭했다면
		// "마지막에 Hierarchy를 클릭한 상태"를 유지한다.
		//만약 Hierarchy의 탭을 누르는 등의 행동을 해도 None으로 취소된다.
		public enum LAST_CLICKED_HIERARCHY
		{
			None,
			Main,
			MeshGroup_TF,
			MeshGroup_Bone,
			AnimClip_TF,
			AnimClip_Bone,
			AnimClip_ControlParam,
		}
		private LAST_CLICKED_HIERARCHY _lastClickedHierarchy = LAST_CLICKED_HIERARCHY.None;
		public LAST_CLICKED_HIERARCHY LastClickedHierarchy { get { return _lastClickedHierarchy; } }

		private bool _isReadyToCheckClickedHierarchy = false;//이게 true로 켜지면, Editor 종료시까지 
		private bool _isClickedHierarchyProcessed = false;//이게 true아니라면 Last Clicked는 None으로 강제된다.
		private EventType _curEventTypeBeforeAnyUsed = EventType.KeyUp;
		public EventType EventTypeBeforeAnyUsed { get { return _curEventTypeBeforeAnyUsed; } }



		// Mesh Edit 화면에서의 UI 변수들
		
		// Mesh Edit 모드
		public enum MESH_EDIT_MODE
		{
			Setting,
			MakeMesh,//AddVertex + LinkEdge
			Modify,
			//AddVertex,//삭제합니더 => 
			//LinkEdge,//삭제
			PivotEdit,
			//VolumeWeight,//>삭제합니더
			//PhysicWeight,//>>삭제합니더
			Pin,//<추가 22.2.28
		}


		public enum MESH_EDIT_MODE_MAKEMESH_TAB
		{
			AddTools, TRS, AutoGen
		}

		public enum MESH_EDIT_MODE_MAKEMESH_ADDTOOLS
		{
			//Add Sub Tab일때 (이 값중 하나면 Add SubTab이 켜진다.)
			VertexAndEdge, VertexOnly, EdgeOnly, Polygon,
		}

		public enum MESH_EDIT_RENDER_MODE
		{
			Normal, ZDepth
		}

		//추가 22.3.2 (v1.4.0) : 핀툴 모드
		public enum MESH_EDIT_PIN_TOOL_MODE { Select, Add, Link, Test }
		

		public MESH_EDIT_MODE _meshEditMode = MESH_EDIT_MODE.Setting;
		public MESH_EDIT_RENDER_MODE _meshEditZDepthView = MESH_EDIT_RENDER_MODE.Normal;
		public MESH_EDIT_MODE_MAKEMESH_TAB _meshEditeMode_MakeMesh_Tab = MESH_EDIT_MODE_MAKEMESH_TAB.AddTools;
		public MESH_EDIT_MODE_MAKEMESH_ADDTOOLS _meshEditeMode_MakeMesh_AddTool = MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge;

		public MESH_EDIT_PIN_TOOL_MODE _meshEditMode_Pin_ToolMode = MESH_EDIT_PIN_TOOL_MODE.Select;
		

		public enum MESH_EDIT_MIRROR_MODE
		{
			None,
			Mirror,
		}
		public MESH_EDIT_MIRROR_MODE _meshEditMirrorMode = MESH_EDIT_MIRROR_MODE.None;

		private apMirrorVertexSet _mirrorVertexSet = null;
		public apMirrorVertexSet MirrorSet { get { if (_mirrorVertexSet == null) { _mirrorVertexSet = new apMirrorVertexSet(this); } return _mirrorVertexSet; } }

		//추가 21.1.6 : Area 편집 중일때
		public bool _isMeshEdit_AreaEditing = false;


		//추가 20.7.6 : 메시를 처음 열때 PSD 파일로부터 열면 바로 Vertex를 리셋하는 기능이 있다.
		public bool _isRequestRemoveVerticesIfImportedFromPSD_Step1 = false;
		public bool _isRequestRemoveVerticesIfImportedFromPSD_Step2 = false;
		public apMesh _requestMeshRemoveVerticesIfImportedFromPSD = null; 
		


		// Root Unit 화면에서의 UI 변수들
		
		public enum ROOTUNIT_EDIT_MODE
		{
			Setting,
			Capture
		}
		public enum ROOTUNIT_CAPTURE_MODE
		{
			Thumbnail,
			ScreenShot,
			GIFAnimation,
			SpriteSheet
		}

		public ROOTUNIT_EDIT_MODE _rootUnitEditMode = ROOTUNIT_EDIT_MODE.Setting;
		public ROOTUNIT_CAPTURE_MODE _rootUnitCaptureMode = ROOTUNIT_CAPTURE_MODE.Thumbnail;



		// Mesh Group 화면에서의 UI 변수들

		// MeshGroup Edit 모드
		public enum MESHGROUP_EDIT_MODE
		{
			Setting,
			Bone,
			Modifier,
		}

		public MESHGROUP_EDIT_MODE _meshGroupEditMode = MESHGROUP_EDIT_MODE.Setting;



		// 애니메이션 화면에서의 UI 변수들
		public enum TIMELINE_LAYOUTSIZE
		{
			Size1, Size2, Size3,
		}
		public TIMELINE_LAYOUTSIZE _timelineLayoutSize = TIMELINE_LAYOUTSIZE.Size2;//<<기본값은 2

		public int[] _timelineZoomWPFPreset = new int[]
		//{ 1, 2, 5, 7, 10, 12, 15, 17, 20, 22, 25, 27, 30, 32, 35, 37, 40, 42, 45, 47, 50 };
		{ 50, 45, 40, 35, 30, 25, 22, 20, 17, 15, 12, 11, 10, 9, 8 };
		public const int DEFAULT_TIMELINE_ZOOM_INDEX = 10;
		public int _timelineZoom_Index = DEFAULT_TIMELINE_ZOOM_INDEX;
		public int WidthPerFrameInTimeline { get { return _timelineZoomWPFPreset[_timelineZoom_Index]; } }

		public bool _isAnimAutoScroll = true;
		public bool _isAnimAutoKey = false;

		

		//추가 20.1.21 : 애니메이션의 Transform 각도 제한 (+-180도) 설정 (기본값은 true = 잠금)
		public bool _isAnimRotation180Lock = true;
		public bool _isModRotation180Lock = true;//추가 21.9.2 : 모디파이어에도 회전 잠금이 포함된다.
		



		// 알림 메시지 (Notification)
		// 별개의 Balloon에 뜨는 일반 알림 메시지 시간
		private bool _isNotification = false;
		private float _tNotification = 0.0f;
		private const float NOTIFICATION_TIME_LONG = 4.5f;
		private const float NOTIFICATION_TIME_SHORT = 1.2f;

		// 작업 공간에 그려지는 작은 Noti를 출력하자
		private bool _isNotification_GUI = false;
		private float _tNotification_GUI = 0.0f;
		private string _strNotification_GUI = "";

		
		// HotKey 출력용 String Wrapper
		private apStringWrapper _hotKeyStringWrapper = null;
		private const string HOTKEY_NOTI_TEXT_1 = "[ ";
		private const string HOTKEY_NOTI_TEXT_2 = " ] - ";
		private const string HOTKEY_NOTI_TEXT_CUSTOMLABEL_1 = " (";
		private const string HOTKEY_NOTI_TEXT_CUSTOMLABEL_2 = ") ";
		private const string HOTKEY_NOTI_TEXT_Command = "Command+";
		private const string HOTKEY_NOTI_TEXT_Ctrl = "Ctrl+";
		private const string HOTKEY_NOTI_TEXT_Alt = "Alt+";
		private const string HOTKEY_NOTI_TEXT_Shift = "Shift+";

		private const string UNDO_REDO_TEXT = "Undo / Redo";





		//백업 GUI
		//GUI에 백업 처리를 하는 경우 업데이트를 하자
		private bool _isBackupProcessing = false;
		private float _tBackupProcessing_Label = 0.0f;
		private float _tBackupProcessing_Icon = 0.0f;
		private const float BACKUP_LABEL_TIME_LENGTH = 2.0f;
		private const float BACKUP_ICON_TIME_LENGTH = 0.8f;
		private Texture2D _imgBackupIcon_Frame1 = null;
		private Texture2D _imgBackupIcon_Frame2 = null;


		
		//서비스 중인 최신 버전 (옵션 포함)
		private bool _isCheckLiveVersion = false;
		private string _currentLiveVersion = "";
		private int _lastCheckLiveVersion_Month = 0;
		private int _lastCheckLiveVersion_Day = 0;
		public bool _isCheckLiveVersion_Option = false;
		
		//버전 체크 후 업데이트 알람 무시하기 기능
		private bool _isVersionNoticeIgnored = false;

		//변경 22.7.1 : 특정 버전은 계속 무시하기
		private int _versionNoticeIgnored_Ver = -1;


		// FFD 변수들
		private object _loadKey_FFDStart = null;
		private int _curFFDSizeX = 3;
		private int _curFFDSizeY = 3;



		// 추가 20.4.6 : 로딩바
		public delegate void FUNC_CANCEL_PROGRESS_POPUP();

		private bool _isProgressPopup = false;
		private bool _isProgressPopup_StartRequest = false;
		private bool _isProgressPopup_CompleteRequest = false;
		private float _proogressPopupRatio = 0.0f;
		private bool _isProogressPopup_Cancelable = false;
		private FUNC_CANCEL_PROGRESS_POPUP _funcProgressPopupCancel = null;
		private apStringWrapper _strProgressPopup_Title = null;
		private apStringWrapper _strProgressPopup_Info = null;

		

		//비동기 로딩 (로딩바 이용)
		private apPortrait _asyncLoading_Portrait = null;
		private static IEnumerator _asyncLoading_Coroutine = null;
		private static System.Diagnostics.Stopwatch _asyncLoading_CoroutineTimer = null;




		//-----------------------------------------------------------------------
		// 에디터 동작과 관련된 변수들
		//-----------------------------------------------------------------------
		
		// Repaint 관련 변수/함수

		private bool _isRepaintable = false;
		private bool _isUpdateWhenInputEvent = false;
		public void SetRepaint()
		{
			_isRepaintable = true;
			_isUpdateWhenInputEvent = true;
		}

		private bool _isRefreshClippingGL = false;//<<렌더링을 1프레임 쉰다. 모드 바뀔때 호출 필요
		public void RefreshClippingGL()
		{
			_isRefreshClippingGL = true;
		}


		/// <summary>
		/// Gizmo 등으로 강제로 업데이트를 한 경우에는 업데이트를 Skip한다.
		/// </summary>
		private bool _isUpdateSkip = false;

		public void SetUpdateSkip()
		{
			_isUpdateSkip = true;
			apTimer.I.ResetTime_Update();
		}


		private float _tMemGC = 0.0f;

		// 이게 True일때만 Repaint된다. Repaint하고나면 이 값은 False가 됨
		private bool _isRepaintTimerUsable = false;

		// 이게 True일때 OnGUI의 Repaint 이벤트가 "제어가능한 유효한 호출"임을 나타낸다.
		// (False일땐 유니티가 자체적으로 Repaint 한 것)
		private bool _isValidGUIRepaint = false;





		// 에디터가 켜진 상태에서 Unity에서 게임 실행 후 정지시 에디터 자동 리셋을 위한 변수
		private bool _isUpdateAfterEditorRunning = false;
				
		
		//첫 렌더링 감지 후 초기화
		public bool _isFirstOnGUI = false;


		/// <summary>
		/// RootUnit/MeshGroup/AnimClip을 선택한 상태에서 Control Parameter나 Frame이 변경되면 Bone Matrix를 임시로 업데이트 하는데,
		/// 이때 GUI와 맞지 않을 수 있다.
		/// 이 값이 True이고 IK가 켜져 있다면 업데이트를 강제로 한번 더 해야한다.
		/// </summary>
		private bool _isMeshGroupChangedByEditor = false;

		/// <summary>
		/// RootUnit/MeshGroup/AnimClip을 선택중일 때 "Control Parameter", "Frame"이 변경되었다면 이 함수를 호출한다.
		/// </summary>
		public void SetMeshGroupChanged()
		{
			_isMeshGroupChangedByEditor = true;
		}


		
		// CheckResource가 계속 호출되는 것을 막기 위한 변수.

		//단 Portrait를 교체하면 다시 호출하도록 하자
		private static bool s_isEditorResourcesLoaded = false;//Static으로 설정해서 OnEnabled보다 빠르게 호출하자

		/// <summary>이 함수를 호출하면 다음 OnGUI에 CheckResources를 다시 호출한다.</summary>
		public void SetEditorResourceReloadable() { s_isEditorResourcesLoaded = false; }

		

		// 화면 캡쳐 요청

		// 추가:맥버전은 매프레임마다 RenderTexture를 돌리면 안된다.
		// 카운트를 해야한다.
		private bool _isScreenCaptureRequest = false;
#if UNITY_EDITOR_OSX
		private bool _isScreenCaptureRequest_OSXReady = false;
		private int _screenCaptureRequest_Count = 0;
		private const int SCREEN_CAPTURE_REQUEST_OSX_COUNT = 5;
#endif
		private apScreenCaptureRequest _screenCaptureRequest = null;



		// 종료 요청 이후 카운트가 계속 늘어나면
		// 강제로 종료해야한다. (무한 루프에 빠질 수 있다)
		private bool _isLockOnEnable = false;



		//-----------------------------------------------------------------------
		// 기능별 특수 옵션들
		//-----------------------------------------------------------------------
		// 캡쳐 옵션
		public enum CAPTURE_GIF_QUALITY
		{
			Low = 0,
			Medium = 1,
			High = 2,
			Maximum = 3
		}
		public int _captureFrame_PosX = 0;
		public int _captureFrame_PosY = 0;
		public int _captureFrame_SrcWidth = 500;
		public int _captureFrame_SrcHeight = 500;
		public int _captureFrame_DstWidth = 500;
		public int _captureFrame_DstHeight = 500;
		public int _captureFrame_SpriteUnitWidth = 500;
		public int _captureFrame_SpriteUnitHeight = 500;
		public int _captureFrame_SpriteMargin = 0;
		public Color _captureFrame_Color = Color.black;
		public bool _captureFrame_IsPhysics = false;
		public bool _isShowCaptureFrame = true;//Capture Frame을 GUI에서 보여줄 것인가
		public bool _isCaptureAspectRatioFixed = true;
		//public int _captureFrame_GIFSampleQuality = 10;//낮을수록 용량이 높은거
		public CAPTURE_GIF_QUALITY _captureFrame_GIFQuality = CAPTURE_GIF_QUALITY.High;
		public int _captureFrame_GIFSampleLoopCount = 1;


		public enum CAPTURE_SPRITE_PACK_IMAGE_SIZE
		{
			s256 = 0,
			s512 = 1,
			s1024 = 2,
			s2048 = 3,
			s4096 = 4
		}
		public CAPTURE_SPRITE_PACK_IMAGE_SIZE _captureSpritePackImageWidth = CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024;
		public CAPTURE_SPRITE_PACK_IMAGE_SIZE _captureSpritePackImageHeight = CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024;

		public enum CAPTURE_SPRITE_TRIM_METHOD
		{
			/// <summary>설정했던 크기 그대로</summary>
			Fixed = 0,
			/// <summary>설정 크기를 기준으로 여백이 있으면 크기가 줄어든다.</summary>
			Compressed = 1,
		}
		public CAPTURE_SPRITE_TRIM_METHOD _captureSpriteTrimSize = CAPTURE_SPRITE_TRIM_METHOD.Fixed;
		public bool _captureSpriteMeta_XML = false;
		public bool _captureSpriteMeta_JSON = false;
		public bool _captureSpriteMeta_TXT = false;


		//캡쳐시 위치/줌을 고정할 수 있다. (수동)
		public Vector2 _captureSprite_ScreenPos = Vector2.zero;
		public int _captureSprite_ScreenZoom = ZOOM_INDEX_DEFAULT;




		
		//추가 8.27 : 메시 자동 생성 기능에 대한 옵션
		public float _meshTRSOption_MirrorOffset = 0.5f;
		public bool _meshTRSOption_MirrorSnapVertOnRuler = false;
		public bool _meshTRSOption_MirrorRemoved = false;

		//추가 21.1.4 : 메시 자동 생성 V2 옵션		
		public int _meshAutoGenV2Option_Inner_Density = 2;//이 값이 커질수록 내부의 점이 많이 생성된다. 기본값 2, 1~10의 값을 가진다.
		public int _meshAutoGenV2Option_OuterMargin = 10;//외부로의 여백
		public int _meshAutoGenV2Option_InnerMargin = 5;//내부로의 여백
		public bool _meshAutoGenV2Option_IsInnerMargin = false;//내부 여백 유무
		
		public int _meshAutoGenV2Option_QuickPresetType = 0;//Quick Generate를 할 때의 프리셋 방식 (Simple = 0)


		



		//---------------------------------------------------------------------------
		// 에디터 첫 페이지에서 사용되는 변수들
		//---------------------------------------------------------------------------
		private List<apPortrait> _portraitsInScene = new List<apPortrait>();
		private bool _isPortraitListLoaded = false;

		//Portrait를 생성한다는 요청이 있다. => 이 요청은 Repaint 이벤트가 아닐때 실행된다.
		private bool _isMakePortraitRequest = false;
		private string _requestedNewPortraitName = "";

		private bool _isMakePortraitRequestFromBackupFile = false;
		private string _requestedLoadedBackupPortraitFilePath = "";




		//---------------------------------------------------------------------------
		// OnGUI 딜레이 갱신 처리용 변수들
		//---------------------------------------------------------------------------
		// OnGUI 이벤트 성격을 체크하기 위한 변수
		private bool _isGUIEvent = false;
		//추가 19.11.23 : String대신 Enum 타입으로 변경한다.
		public enum DELAYED_UI_TYPE
		{
			None,
			Right2GUI,
			GUI_Top_Onion_Visible,
			Top_UI__Vertex_Transform,
			Top_UI__Position,
			Top_UI__Rotation,
			Top_UI__Scale,
			Top_UI__Depth,
			Top_UI__Color,
			Top_UI__Extra,
			Top_UI__BoneIKController,
			Top_UI__VTF_FFD,
			Top_UI__VTF_Soft,
			Top_UI__VTF_Blur,
			Top_UI__Overall,
			GUI_MeshGroup_Hierarchy_Delayed__Meshes,
			GUI_MeshGroup_Hierarchy_Delayed__Bones,
			GUI_Anim_Hierarchy_Delayed__Meshes,
			GUI_Anim_Hierarchy_Delayed__Bone,
			GUI_Anim_Hierarchy_Delayed__ControlParam,
			Capture_GIF_ProgressBar,
			Capture_GIF_Clips,
			Capture_Spritesheet_ProgressBar,
			Capture_Spritesheet_Settings,
			Mesh_Property_Modify_UI_Single,
			Mesh_Property_Modify_UI_Multiple,
			Mesh_Property_Modify_UI_No_Info,
			BoneEditMode__Editable,
			BoneEditMode__Select,
			BoneEditMode__Add,
			BoneEditMode__Link,
			Bottom2_Transform_Mod_Vert,
			Animation_Bottom_Property__MK,
			Animation_Bottom_Property__SK,
			Animation_Bottom_Property__ML,
			Animation_Bottom_Property__SL,
			Animation_Bottom_Property__T,
			Bottom_Right_Anim_Property__ControlParamUI,
			Bottom_Right_Anim_Property__ModifierUI,
			Bottom_Right_Anim_Property__BoneLayer,
			Anim_Property__SameKeyframe,
			AnimProperty_MultipleCurve__NoKey,
			AnimProperty_MultipleCurve__Sync,
			AnimProperty_MultipleCurve__NotSync,
			MeshGroupRight_Setting_ObjectSelected_SingleMeshTF,
			MeshGroupRight_Setting_ObjectSelected_SingleMeshGroupTF,
			MeshGroupRight_Setting_ObjectSelected_MultiMeshTF,
			MeshGroupRight_Setting_ObjectSelected_MultiMeshGroupTF,
			MeshGroupRight_Setting_ObjectSelected_Mixed,
			MeshGroupRight_Setting_ObjectNotSelected,
			Render_Unit_Detail_Status__MeshTransform,
			Render_Unit_Detail_Status__MeshGroupTransform,
			MeshGroup_Mesh_Setting__CustomShader,
			MeshGroup_Mesh_Setting__MaterialLibrary,
			MeshGroup_Mesh_Setting__MatLib_NotUseDefault,
			MeshGroup_Mesh_Setting__Same_Mesh,
			Mesh_Transform_Detail_Status__Clipping_Child,
			Mesh_Transform_Detail_Status__Clipping_Parent,
			Mesh_Transform_Detail_Status__Clipping_None,
			Update_Child_Bones,
			MeshGroupRight2_Bone_Single,
			MeshGroupRight2_Bone_Multiple,
			MeshGroup_Bone__Child_Bone_Drawable,
			Bone_Mirror_Axis_Option_Visible,
			MeshGroupBottom_Modifier,
			CP_Selected_ParamSetGroup,
			Modifier_Add_Transform_Check_Single,
			Modifier_Add_Transform_Check_Multiple,
			Modifier_Add_Transform_Check_Unselected,
			Modifier_Add_Transform_Check__Rigging,
			Rigging_UI_Info__MultipleVert,
			Rigging_UI_Info__SingleVert,
			Rigging_UI_Info__UnregRigData,
			Rigging_UI_Info__SameMode,
			Rig_Mod__RigDataCount_Refreshed,
			Modifier_Add_Transform_Check__Physic__Valid,
			Modifier_Add_Transform_Check__Physic__Invalid,
			Modifier_PhysicsPreset_Valid,
			Modifier_PhysicsPreset_Invalid,
			AnimationRight2GUI_AnimClip,
			AnimationRight2GUI_Timeline,
			AnimationRight2GUI_Timeline_Selected_Single,
			AnimationRight2GUI_Timeline_Selected_Multiple,
			AnimationRight2GUI_Timeline_Layers,
			Modifier_ControlParam_AddOrRemoveKeyButton,
			Modifier_ControlParam_NoAddKeyBtn,
		}
		private Dictionary<DELAYED_UI_TYPE, bool> _delayedGUIShowList = new Dictionary<DELAYED_UI_TYPE, bool>();
		private Dictionary<DELAYED_UI_TYPE, bool> _delayedGUIToggledList = new Dictionary<DELAYED_UI_TYPE, bool>();





		//---------------------------------------------------------------------------
		// 객체의 변경 사항을 기록하는 변수들
		//---------------------------------------------------------------------------
		private List<int> _recordList_TextureData = new List<int>();
		private List<int> _recordList_Mesh = new List<int>();
		//private List<int> _recordList_MeshGroup = new List<int>();//기존
		private List<int> _recordList_AnimClip = new List<int>();
		private List<int> _recordList_AnimTimeline = new List<int>();
		private List<int> _recordList_AnimTimelineLayer = new List<int>();
		private List<int> _recordList_ControlParam = new List<int>();
		private List<int> _recordList_Modifier = new List<int>();
		//private List<int> _recordList_Transform = new List<int>();//기존
		private List<int> _recordList_Bone = new List<int>();

		//변경 20.1.28 : Record 리스트 중에서 계층 구조인 MeshGroup > Transform (1단계만)는 단순 리스트가 아니라, Dictionary로 만들자
		//MeshGroup이 많지 않으므로 속도에 큰 차이가 없을 것
		private Dictionary<int, List<int>> _recordList_MeshGroupAndTransform = new Dictionary<int, List<int>>();
		//변경 20.3.19 : 구조 변경과 더불어서 AnimClip > MeshGroup 연결 정보도 저장한다.
		private Dictionary<int, int> _recordList_AnimClip2TargetMeshGroup = new Dictionary<int, int>();

		private bool _isRecordedStructChanged = false;




		//---------------------------------------------------------------------------
		// 핵심 편집 대상 < 중요 >
		//---------------------------------------------------------------------------
		public apPortrait _portrait = null;



		//---------------------------------------------------------------------------
		// 기타 변수들
		//---------------------------------------------------------------------------
		//현재 씬
		private UnityEngine.SceneManagement.Scene _currentScene;


		private bool _isGizmoGUIVisible_VTF_FFD_Prev = false;


		//렌더링시 사용되는 임시 변수들 (선택된 객체 리스트)
		//변경 20.5.28 : 메인 선택과 보조 선택을 구분한다.
		private apTransform_Mesh _tmpSelectedMainMeshTF = null;
		private List<apTransform_Mesh> _tmpSelectedSubMeshTFs = null;
		
		private apBone _tmpSelectedMainBone = null;
		private List<apBone> _tmpSelectedSubBones = null;
		

		private apRenderUnit _tmpSelected_MainRenderUnit = null;
		private List<apRenderUnit> _tmpSelected_SubRenderUnits = new List<apRenderUnit>();



		//---------------------------------------------------------------------------
		// GUI Content / Style 변수들 + Wrapper 객체들
		//---------------------------------------------------------------------------
		// 추가 19.11.20 : GUIContent들
		private apGUIContentWrapper _guiContent_Notification = null;
		private apGUIContentWrapper _guiContent_TopBtn_Setting = null;
		private apGUIContentWrapper _guiContent_TopBtn_Bake = null;
		private apGUIContentWrapper _guiContent_MainLeftUpper_MakeNewPortrait = null;
		private apGUIContentWrapper _guiContent_MainLeftUpper_RefreshToLoad = null;
		private apGUIContentWrapper _guiContent_MainLeftUpper_LoadBackupFile = null;
		private apGUIContentWrapper _guiContent_GUITopTab_Open = null;
		private apGUIContentWrapper _guiContent_GUITopTab_Folded = null;
		private apGUIContentWrapper _guiContent_Top_GizmoIcon_Move = null;
		private apGUIContentWrapper _guiContent_Top_GizmoIcon_Depth = null;
		private apGUIContentWrapper _guiContent_Top_GizmoIcon_Rotation = null;
		private apGUIContentWrapper _guiContent_Top_GizmoIcon_Scale = null;
		private apGUIContentWrapper _guiContent_Top_GizmoIcon_Color = null;
		private apGUIContentWrapper _guiContent_Top_GizmoIcon_Extra = null;

		//EditorController에서 사용될 GUIContent
		public apGUIContentWrapper _guiContent_EC_SetDefault = null;
		//public apGUIContentWrapper _guiContent_EC_EditParameter = null;//삭제 21.2.9 : 컨트롤 파라미터 편집 버튼은 삭제
		public apGUIContentWrapper _guiContent_EC_MakeKey_Editing = null;
		public apGUIContentWrapper _guiContent_EC_MakeKey_NotEdit = null;
		public apGUIContentWrapper _guiContent_EC_Select = null;
		public apGUIContentWrapper _guiContent_EC_RemoveKey = null;

		//주의 : GUIContent 추가시 ResetGUIContents() 함수에 리셋 코드 추가

		//GUIContent의 텍스트 생성용 StringWrapper
		private apStringWrapper _guiStringWrapper_32 = new apStringWrapper(32);
		private apStringWrapper _guiStringWrapper_64 = new apStringWrapper(64);
		private apStringWrapper _guiStringWrapper_128 = new apStringWrapper(128);
		private apStringWrapper _guiStringWrapper_256 = new apStringWrapper(256);



		// 추가 19.11.21 : GUIStyle Wrapper 객체
		private apGUIStyleWrapper _guiStyleWrapper = null;
		public apGUIStyleWrapper GUIStyleWrapper { get { return _guiStyleWrapper; } }

		// 추가 19.12.2 : String Factory / GUI Layout Option Factory
		private apStringFactory _stringFactory = null;
		public apStringFactory StringFactory { get { return _stringFactory; } }

		private apGUILOFactory _guiLOFactory = null;
		private apGUILOFactory GUILOFactory { get { return _guiLOFactory; } }

		private apStringWrapper _mainRightAnimHeaderTextWrapper = null;
	}
}