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
	public partial class apPSDReimportDialog : EditorWindow
	{
		// Menu
		//----------------------------------------------------------
		private static apPSDReimportDialog s_window = null;

		public static object ShowWindow(apEditor editor, FUNC_PSD_REIMPORT_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apPSDReimportDialog), false, "PSD Reload");
			apPSDReimportDialog curTool = curWindow as apPSDReimportDialog;
			if (curTool != null && curTool != s_window)
			{

				int width = 1100;
				int height = 700;

				object loadKey = new object();

				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, editor._portrait, funcResult, loadKey);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		// Members
		//----------------------------------------------------------
		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private object _loadKey = null;
		private FUNC_PSD_REIMPORT_RESULT _funcResult = null;
		public delegate void FUNC_PSD_REIMPORT_RESULT(	bool isSuccess, 
														object loadKey, string fileName, string filePath,
														List<apPSDLayerData> layerDataList, 
														int atlasScaleRatioX100, int meshGroupScaleRatioX100, int prevAtlasScaleRatioX100,
														int totalWidth, int totalHeight, 
														int padding, 
														int bakedTextureWidth, int bakedTextureHeight, int bakeMaximumNumAtlas, bool bakeBlurOption,
														float centerOffsetDeltaX, float centerOffsetDeltaY,
														string bakeDstFilePath, string bakeDstFileRelativePath,
														apPSDSet psdSet
														//float deltaScaleRatio
														);//<<나중에 처리 결과에 따라서 더 넣어주자


		private bool _isGUIEvent = false;
		private Dictionary<string, bool> _delayedGUIShowList = new Dictionary<string, bool>();
		private Dictionary<string, bool> _delayedGUIToggledList = new Dictionary<string, bool>();

		public enum RELOAD_STEP
		{
			Step1_SelectPSDSet,//PSD Set을 선택하거나 생성 (또는 삭제)
			Step2_FileLoadAndSelectMeshGroup,//PSD Set과 연결된 PSD 파일/MeshGroup/Atlas Texture Data를 선택 + 크기 오프셋 설정
			Step3_LinkLayerToTransform,//레이어 정보와 Mesh/MeshGroup Transform 연결 <GUI - 메시+레이어>
			Step4_ModifyOffset,//레이어의 위치 수정 <GUI - 메시+레이어>
			Step5_AtlasSetting,//아틀라스 굽기 (+PSD에서 삭제되면서 TextureData 갱신에 포함되지 않는 경우 포함) <GUI - Atlas>
		}

		private RELOAD_STEP _step = RELOAD_STEP.Step1_SelectPSDSet;

		private Color _glBackGroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);




		private apPSDSet _selectedPSDSet = null;
		private apPSDSet.TextureDataSet _selectedTextureData = null;

		//추가 22.6.22 : 보조 텍스쳐 생성을 위한 변수
		private apPSDSecondarySet _selectedPSDSecondary = null;
		private apPSDSecondarySet.SrcTextureData _selectedSecondaryTexInfo = null;

		//편집 모드
		public enum EDIT_TARGET
		{
			Unknown,
			Main,
			Secondary
		}
		private EDIT_TARGET EditTarget
		{
			get
			{
				if(_selectedPSDSet != null) { return EDIT_TARGET.Main; }
				if(_selectedPSDSecondary != null) { return EDIT_TARGET.Secondary; }
				return EDIT_TARGET.Unknown;
			}
		}


		private const int PSD_IMAGE_FILE_MAX_SIZE = 5000;

		//PSD Loader
		private apPSDLoader _psdLoader = null;
		private apPSDLayerData _selectedPSDLayerData = null;
		private apPSDBakeData _selectedPSDBakeData = null;

		private apPSDLayerData _linkSrcLayerData = null;
		private bool _isLinkLayerToTransform = false;

		//MeshTransform, MeshGroupTransform -> apPSDLayerData으로 참조하는 데이터를 알 수 있어야 한다.
		public class TargetTransformData
		{
			public bool _isMeshTransform;
			public apMesh _mesh = null;
			public apTransform_Mesh _meshTransform = null;
			public apTransform_MeshGroup _meshGroupTransform = null;
			public bool _isClipped = false;
			public bool _isValidMesh = false;
			public int _transformID = -1;
			

			public TargetTransformData(apTransform_Mesh meshTransform, bool isValidMesh)
			{
				_isMeshTransform = true;
				_meshTransform = meshTransform;
				_mesh = meshTransform._mesh;
				_meshGroupTransform = null;
				_isValidMesh = isValidMesh;
				_transformID = meshTransform._transformUniqueID;
				_isClipped = _meshTransform._isClipping_Child;
			}

			public TargetTransformData(apTransform_MeshGroup meshGroupTransform)
			{
				_isMeshTransform = false;
				_meshTransform = null;
				_mesh = null;
				_meshGroupTransform = meshGroupTransform;
				_isValidMesh = true;
				_transformID = meshGroupTransform._transformUniqueID;
				_isClipped = false;
			}

			public string Name
			{
				get
				{
					if(_meshTransform != null)
					{
						return _meshTransform._nickName;
					}
					else if(_meshGroupTransform != null)
					{
						return _meshGroupTransform._nickName;
					}
					return "";
				}
			}
		}
		private List<TargetTransformData> _targetTransformList = new List<TargetTransformData>();
		private Dictionary<apTransform_Mesh, apPSDLayerData> _meshTransform2PSDLayer = new Dictionary<apTransform_Mesh, apPSDLayerData>();
		private Dictionary<apTransform_MeshGroup, apPSDLayerData> _meshGroupTransform2PSDLayer = new Dictionary<apTransform_MeshGroup, apPSDLayerData>();
		private Dictionary<apPSDSecondarySetLayer, apPSDLayerData> _secondaryLayer2PSDLayer = new Dictionary<apPSDSecondarySetLayer, apPSDLayerData>();

		//PSD Remap List
		//private List<apPSDRemapData> _remapList = new List<apPSDRemapData>();
		//private Dictionary<apPSDLayerData, apPSDRemapData> _remapList_Psd2Map = new Dictionary<apPSDLayerData, apPSDRemapData>();

		// GUI
		//public int _iZoomX100 = 11;//11 => 100
		//public const int ZOOM_INDEX_DEFAULT = 11;
		//public int[] _zoomListX100 = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, 85, 90, 95, 100/*(11)*/, 105, 110, 120, 140, 160, 180, 200, 250, 300, 350, 400, 450, 500 };
		public int _iZoomX100 = 36;//36 => 100
		public const int ZOOM_INDEX_DEFAULT = 36;
		public int[] _zoomListX100 = new int[] {    4,	6,	8,	10, 12, 14, 16, 18, 20, 22, //9
													24, 26, 28, 30, 32, 34, 36, 38, 40, 42, //19
													44, 46, 48, 50, 52, 54, 56, 58, 60, 65, //29
													70, 75, 80, 85, 90, 95, 100, //39
													105, 110, 115, 120, 125, 130, 140, 150, 160, 180, 200,
													220, 240, 260, 280, 300, 350, 400, 450,
													500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000,
													2100, 2200, 2300, 2400, 2500, 2600, 2700, 2800, 2900, 3000 };

		private Vector2 _scroll_GUI = Vector2.zero;

		private apPSDMouse _mouse = new apPSDMouse();
		private apPSDGL _gl = new apPSDGL();

		private bool _isCtrlAltDrag = false;

		//private bool _isRender_PSD = true;
		private bool _isRender_MeshGroup = false;
		private bool _isRender_TextureData = false;
		private enum RENDER_MODE
		{
			Normal,
			Outline,
			Hide,
		}
		//private bool _isRender_PSDAlpha = true;
		private RENDER_MODE _renderMode_PSD = RENDER_MODE.Normal;
		private RENDER_MODE _renderMode_Mesh = RENDER_MODE.Normal;

		//private int _psdRenderPosOffset_X = 0;
		//private int _psdRenderPosOffset_Y = 0;

		private bool _isLinkGUIColoredList = false;
		private bool _isLinkOverlayColorRender = false;
		private Color _meshOverlayColor = new Color(1.0f, 0.0f, 0.0f, 0.85f);
		private Color _psdOverlayColor = new Color(0.0f, 1.0f, 0.8f, 0.85f);

		//private bool _isRenderOffset_PSD = true;
		//private bool _isRenderOffset_Mesh = true;
		private bool _isRenderMesh2PSD = true;

		// Scroll
		private Vector2 _scroll_Step1_Left = Vector2.zero;
		private Vector2 _scroll_Step2_Left2 = Vector2.zero;
		private Vector2 _scroll_Step2_Left = Vector2.zero;
		private Vector2 _scroll_Step3_Line1 = Vector2.zero;
		private Vector2 _scroll_Step3_Line2 = Vector2.zero;
		private Vector2 _scroll_Step4_Left = Vector2.zero;
		private Vector2 _scroll_Step5_Left = Vector2.zero;



		private bool _isRequestCloseDialog = false;
		private bool _isDialogEnded = false;

		
		private bool IsGUIUsable { get { return (!_psdLoader.IsProcessRunning); } }
		private bool IsProcessRunning { get { return _psdLoader.IsProcessRunning; } }

		private string[] _bakeDescription = new string[] { "256", "512", "1024", "2048", "4096" };
		private bool _isNeedBakeCheck = false;
		private bool _isBakeWarning = false;
		private string _bakeWarningMsg = "";
		private object _loadKey_Calculated = null;
		private object _loadKey_Bake = null;

		private Dictionary<apPSDSet, FileInfo> _psdSet2FileInfo = null;
		private Dictionary<apPSDSecondarySet, FileInfo> _psdSecondary2FileInfo = null;


		private GUIStyle _guiStyle_InfoBox = null;

		private GUIStyle _guiStyle_GLText = null;
		private GUIStyle _guiStyle_GLTextWarning = null;
		private apStringWrapper _strWrapper_128 = null;


		//Nearest 필터링이 들어간 텍스쳐를 생성한다. (Portrait의 모든 텍스쳐를 지정하자)
		private Dictionary<Texture2D, Texture2D> _meshTexture2NearestTex = null;

		private enum TEXTURE_SAMPLING
		{
			Default,
			Nearest
		}
		private TEXTURE_SAMPLING _samplingMethod = TEXTURE_SAMPLING.Default;

		
		// Init
		//----------------------------------------------------------
		private void Init(apEditor editor, apPortrait portrait, FUNC_PSD_REIMPORT_RESULT funcResult, object loadKey)
		{
			_editor = editor;
			_portrait = portrait;
			_loadKey = loadKey;
			_funcResult = funcResult;

			_step = RELOAD_STEP.Step1_SelectPSDSet;

			Shader[] shaderSet_Normal = new Shader[4];
			Shader[] shaderSet_VertAdd = new Shader[4];
			//Shader[] shaderSet_Mask = new Shader[4];
			Shader[] shaderSet_Clipped = new Shader[4];
			for (int i = 0; i < 4; i++)
			{
				shaderSet_Normal[i] = editor._mat_Texture_Normal[i].shader;
				shaderSet_VertAdd[i] = editor._mat_Texture_VertAdd[i].shader;
				//shaderSet_Mask[i] = editor._mat_MaskedTexture[i].shader;
				shaderSet_Clipped[i] = editor._mat_Clipped[i].shader;
			}

			//_gl.SetMaterial(editor._mat_Color, editor._mat_Texture, editor._mat_MaskedTexture);
			_gl.SetShader(editor._mat_Color.shader,
							shaderSet_Normal,
							shaderSet_VertAdd,
							//shaderSet_Mask,
							editor._mat_MaskOnly.shader,
							shaderSet_Clipped,
							editor._mat_GUITexture.shader,
							editor._mat_ToneColor_Normal.shader,
							editor._mat_ToneColor_Clipped.shader,
							editor._mat_Alpha2White.shader,
							editor._mat_BoneV2.shader,
							editor._mat_Texture_VColorMul.shader,
							editor._mat_RigCircleV2.shader,
							editor._mat_Gray_Normal.shader,
							editor._mat_Gray_Clipped.shader,
							editor._mat_VertPin.shader);

			wantsMouseMove = true;

			//값 초기화
			_selectedPSDSet = null;
			//_selectedPSDSetLayer = null;
			_selectedTextureData = null;
			_selectedSecondaryTexInfo = null;

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			if(_targetTransformList == null) { _targetTransformList = new List<TargetTransformData>(); }
			if(_meshTransform2PSDLayer == null) { _meshTransform2PSDLayer = new Dictionary<apTransform_Mesh, apPSDLayerData>(); }
			if(_meshGroupTransform2PSDLayer == null) { _meshGroupTransform2PSDLayer = new Dictionary<apTransform_MeshGroup, apPSDLayerData>(); }
			if(_secondaryLayer2PSDLayer == null) { _secondaryLayer2PSDLayer = new Dictionary<apPSDSecondarySetLayer, apPSDLayerData>(); }

			_targetTransformList.Clear();
			_meshTransform2PSDLayer.Clear();
			_meshGroupTransform2PSDLayer.Clear();
			_secondaryLayer2PSDLayer.Clear();

			//for (int i = 0; i < _portrait._bakedPsdSets.Count; i++)
			//{
			//	_portrait._bakedPsdSets[i].ReadyToLoad();
			//}

			_isRequestCloseDialog = false;
			_isDialogEnded = false;

			_psdLoader = new apPSDLoader(editor);
			_selectedPSDLayerData = null;
			_selectedPSDBakeData = null;

			//_isRender_PSD = true;
			_isRender_MeshGroup = true;//<<두개가 보인다.
			_isRender_TextureData = false;
			//_isRender_PSDAlpha = true;//기본 알파
			_isLinkGUIColoredList = false;
			_isLinkOverlayColorRender = false;

			//_isRenderOffset_PSD = true;
			//_isRenderOffset_Mesh = true;
			_isRenderMesh2PSD = true;

			_isNeedBakeCheck = true;
			_isBakeWarning = false;
			_bakeWarningMsg = "";
			_loadKey_Calculated = null;
			_loadKey_Bake = null;
			
			_renderMode_PSD = RENDER_MODE.Normal;
			_renderMode_Mesh = RENDER_MODE.Normal;


			_psdSet2FileInfo = new Dictionary<apPSDSet, FileInfo>();
			_psdSecondary2FileInfo = new Dictionary<apPSDSecondarySet, FileInfo>();
			_psdSet2FileInfo.Clear();
			_psdSecondary2FileInfo.Clear();

			//Portrait의 이미지들을 복제하여 Nearest 샘플링이 들어간 텍스쳐 리스트를 만든다.
			_meshTexture2NearestTex = new Dictionary<Texture2D, Texture2D>();
			_meshTexture2NearestTex.Clear();
			int nTextureData = _portrait._textureData != null ? _portrait._textureData.Count : 0;
			if(nTextureData > 0)
			{
				apTextureData curTextureData = null;
				for (int i = 0; i < nTextureData; i++)
				{
					curTextureData = _portrait._textureData[i];
					if(curTextureData == null) { continue; }
					if(curTextureData._image == null) { continue; }

					Texture2D srcTexture = curTextureData._image;
					Texture2D nearestTex = null;
					if(srcTexture != null)
					{
						_meshTexture2NearestTex.TryGetValue(srcTexture, out nearestTex);
					}

					if(nearestTex == null)
					{
						//변환 정보가 없다면 새로 변환하자
						nearestTex = new Texture2D(srcTexture.width, srcTexture.height, TextureFormat.RGBA32, false);
						nearestTex.LoadRawTextureData(srcTexture.GetRawTextureData());
						nearestTex.wrapMode = TextureWrapMode.Clamp;
						nearestTex.filterMode = FilterMode.Point;
						nearestTex.Apply();

						_meshTexture2NearestTex.Add(srcTexture, nearestTex);
					}
				}
			}

			


			RefreshPSDFileInfo();
		}


		public static void CloseDialog()
		{
			if (s_window != null)
			{
				//s_window.CloseThread();

				try
				{
					s_window._isRequestCloseDialog = false;
					s_window._isDialogEnded = true;
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);

				}

				s_window = null;
			}
		}

		//?
		void Update()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			Repaint();

			if (_psdLoader != null)
			{
				_psdLoader.Update();
			}

			if(_isRequestCloseDialog)
			{
				CloseDialog();
			}
		}

		void OnGUI()
		{
			try
			{
				if (_editor == null || _editor._portrait == null)
				{
					CloseDialog();
					return;
				}

				if(_isDialogEnded)
				{
					return;
				}

				//GUI 이벤트인지 판별
				if (Event.current.type != EventType.Layout
					&& Event.current.type != EventType.Repaint)
				{
					_isGUIEvent = false;
				}
				else
				{
					_isGUIEvent = true;
				}


				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;

				int topHeight = 28;
				int bottomHeight = 46;
				int margin = 4;

				if(_guiStyle_InfoBox == null)
				{
					_guiStyle_InfoBox = new GUIStyle(GUI.skin.box);
					_guiStyle_InfoBox.alignment = TextAnchor.MiddleLeft;
				}

				if(_guiStyle_GLText == null)
				{
					_guiStyle_GLText = new GUIStyle(GUI.skin.label);
					_guiStyle_GLText.normal.textColor = Color.yellow;
					_guiStyle_GLText.alignment = TextAnchor.UpperLeft;
				}

				if(_guiStyle_GLTextWarning == null)
				{
					_guiStyle_GLTextWarning = new GUIStyle(GUI.skin.label);
					_guiStyle_GLTextWarning.normal.textColor = Color.red;
					_guiStyle_GLTextWarning.alignment = TextAnchor.UpperLeft;
				}

				if(_strWrapper_128 == null)
				{
					_strWrapper_128 = new apStringWrapper(128);
					_strWrapper_128.Clear();
				}

				

				if (_selectedPSDSet == null 
					&& _selectedPSDSecondary == null
					&& _step != RELOAD_STEP.Step1_SelectPSDSet)
				{
					//만약 에러가 나서 PSDSet 선택이 해제되면, 강제로 첫 화면으로 돌아가야한다.
					_step = RELOAD_STEP.Step1_SelectPSDSet;

					RefreshPSDFileInfo();//파일 정보 리플레시
				}

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));
				//-------------------------------------------------------------------------------------------------------
				int centerHeight = windowHeight - (topHeight + bottomHeight + margin * 2);

				// Top UI : Step을 차례로 보여준다.
				//-----------------------------------------------
				GUI.Box(new Rect(0, 0, windowWidth, topHeight), "");

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(topHeight));

				GUI_Top(windowWidth, topHeight - 6);

				EditorGUILayout.EndVertical();
				// Center UI : Step별 설정을 보여준다.
				//-----------------------------------------------
				GUILayout.Space(margin);

				// Center UI : 메인 에디터
				//-----------------------------------------------

				//EditorGUILayout.BeginVertical();
				Rect centerRect = new Rect(0, topHeight + margin - 2, windowWidth, centerHeight + 2);
				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));
				//Mouse Update
				//Mouse ScrollUpdate
				//GUI..
				GUI_Center(windowWidth, centerHeight, centerRect);
				

				EditorGUILayout.EndVertical();

				GUILayout.Space(4);

				// Bottom UI : 스텝 이동/확인/취소를 제어할 수 있다.
				//--------------------------------------------
				GUI.Box(new Rect(0, topHeight + margin + centerHeight + margin, windowWidth, bottomHeight), "");
				GUILayout.Space(margin);

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(bottomHeight));
				GUI_Bottom(windowWidth, bottomHeight - 12);
				EditorGUILayout.EndVertical();
				//-------------------------------------------------------------------------------------------------------
				EditorGUILayout.EndVertical();

				//if(_isRequestCloseDialog)
				//{
				//	CloseDialog();
				//}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		// Top
		private void GUI_Top(int width, int height)
		{
			int stepWidth = (width / 8) - 10;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			//GUILayout.Space(20);
			int totalContentWidth = ((stepWidth + 2) * 5) + (50 * 4);
			GUILayout.Space((width / 2) - (totalContentWidth / 2));

			Color prevColor = GUI.backgroundColor;

			GUIStyle guiStyle_Center = GUI.skin.box;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			guiStyle_Center.normal.textColor = apEditorUtil.BoxTextColor;

			GUIStyle guiStyle_Next = GUI.skin.label;
			guiStyle_Next.alignment = TextAnchor.MiddleCenter;

			Color selectedColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
			Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			for (int iStep = 0; iStep < 5; iStep++)
			{
				RELOAD_STEP stepType = (RELOAD_STEP)iStep;
				if (_step == stepType)	{ GUI.backgroundColor = selectedColor; }
				else								{ GUI.backgroundColor = unselectedColor; }
				string strLabel = "";
				switch (stepType)
				{
					case RELOAD_STEP.Step1_SelectPSDSet:
						strLabel = _editor.GetText(TEXT.DLG_PSD_PSDSet);//PSD Set
						break;

					case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup:
						strLabel = _editor.GetText(TEXT.DLG_PSD_BasicSetting);//BasicSetting
						break;

					case RELOAD_STEP.Step3_LinkLayerToTransform:
						strLabel = _editor.GetText(TEXT.DLG_PSD_Mapping);//Mapping
						break;

					case RELOAD_STEP.Step4_ModifyOffset:
						strLabel = _editor.GetText(TEXT.DLG_PSD_Adjust);//Adjust
						break;

					case RELOAD_STEP.Step5_AtlasSetting:
						strLabel = _editor.GetText(TEXT.DLG_PSD_Atlas);//Atlas
						break;
				}
				GUILayout.Box(strLabel, guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));

				if(iStep < 4)
				{
					GUILayout.Space(10);
					GUILayout.Box(">>", guiStyle_Next, GUILayout.Width(30), GUILayout.Height(height));
					GUILayout.Space(10);
				}
			}

			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = prevColor;
		}


		// Bottom
		private void GUI_Bottom(int width, int height)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			int colorWidth = 155;
			int samplingBtnWidth = height + 20;
			int btnWidth = 120;
			int btnWidth_Cancel = 100;
			int margin = width - (btnWidth * 2 + btnWidth_Cancel + 12 + 30 + (colorWidth * 3) + samplingBtnWidth + 10 + 2 + 20 + 4);

			GUILayout.Space(10);


			//Background Color / PSD Line Color / Mesh Line Color
			EditorGUILayout.BeginVertical(GUILayout.Width(colorWidth));
			GUILayout.Space(2);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BackgroundColor), GUILayout.Width(colorWidth));//Background Color
			try
			{
				_glBackGroundColor = EditorGUILayout.ColorField(_glBackGroundColor, GUILayout.Width(colorWidth - 20));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();


			EditorGUILayout.BeginVertical(GUILayout.Width(colorWidth));
			GUILayout.Space(2);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDOutlineColor), GUILayout.Width(colorWidth));//PSD Outline Color
			try
			{
				_psdOverlayColor = EditorGUILayout.ColorField(_psdOverlayColor, GUILayout.Width(colorWidth - 20));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();


			EditorGUILayout.BeginVertical(GUILayout.Width(colorWidth));
			GUILayout.Space(2);

			if(_selectedPSDSecondary != null)
			{
				//Secondary인 경우엔 텍스트가 다르다.
				//"이전 기록 색상"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.PreviousLayerColor), GUILayout.Width(colorWidth));
			}
			else
			{
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_MeshOutlineColor), GUILayout.Width(colorWidth));//Mesh Outline Color
			}
			
			try
			{
				_meshOverlayColor = EditorGUILayout.ColorField(_meshOverlayColor, GUILayout.Width(colorWidth - 20));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();

			//추가 v1.4.2 : 텍스쳐 샘플링 방식
			if(apEditorUtil.ToggledButton_2Side(	_editor.ImageSet.Get(apImageSet.PRESET.PSD_TextureSampling),
												_samplingMethod == TEXTURE_SAMPLING.Nearest,
												true,
												samplingBtnWidth, height))
			{
				if(_samplingMethod == TEXTURE_SAMPLING.Default)
				{
					_samplingMethod = TEXTURE_SAMPLING.Nearest;
				}
				else
				{
					_samplingMethod = TEXTURE_SAMPLING.Default;
				}
			}
				


			GUILayout.Space(margin);

			if (_step == RELOAD_STEP.Step1_SelectPSDSet)
			{
				GUILayout.Space(btnWidth + 4);
			}
			else
			{
				if (GUILayout.Button("< " + _editor.GetText(TEXT.DLG_PSD_Back), GUILayout.Width(btnWidth), GUILayout.Height(height)))//Back
				{
					//TODO
					//if (IsGUIUsable)
					//{
					//	MoveStep(false);
					//}
					MovePrev();
				}
			}

			if (_step == RELOAD_STEP.Step5_AtlasSetting)
			{
				//TODO : Secondary에 대해서도 작성할 것
				bool isCompletable = false;
				if(_selectedPSDSet != null)
				{
					// PSD Set을 선택했을 때
					if (_selectedPSDSet._targetMeshGroupID >= 0
						&& _selectedPSDSet._linkedTargetMeshGroup != null
						&& _psdLoader.IsFileLoaded
						&& _psdLoader.IsCalculated
						&& _psdLoader.IsBaked)
					{
						isCompletable = true;

						if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Complete), GUILayout.Width(btnWidth), GUILayout.Height(height)))//Complete
						{
							bool isSuccess = _psdLoader.Step4_ConvertToAnyPortrait(	_selectedPSDSet._bakeOption_DstFilePath,
																					OnConvertResult,
																					_portrait,
																					_selectedPSDSet,
																					_meshTransform2PSDLayer);

							if(!isSuccess)
							{
								//에러가 발생했다. (에러는 경로에 의해서만 발생한다.)
								//경로가 유효하지 않다면 다시 설정해달라는 경고
								EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_PSD_InvalidPath_Title),
																_editor.GetText(TEXT.DLG_PSD_InvalidPath_Body),
																_editor.GetText(TEXT.Okay));
							}
						}
					}
				}
				else if(_selectedPSDSecondary != null)
				{
					// PSD Secondary Set을 선택했을 때
					if(_psdLoader.IsFileLoaded					
						&& _psdLoader.IsCalculated
						&& _psdLoader.IsBaked)
					{
						isCompletable = true;

						if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Complete), GUILayout.Width(btnWidth), GUILayout.Height(height)))//Complete
						{
							bool isSuccess = _psdLoader.Step4_ConvertToAnyPortrait_Secondary(	_selectedPSDSecondary._dstFilePath,
																								OnConvertResult_Secondary,
																								_selectedPSDSecondary);

							if(!isSuccess)
							{
								//에러가 발생했다. (에러는 경로에 의해서만 발생한다.)
								//경로가 유효하지 않다면 다시 설정해달라는 경고
								EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_PSD_InvalidPath_Title),
																_editor.GetText(TEXT.DLG_PSD_InvalidPath_Body),
																_editor.GetText(TEXT.Okay));
							}
						}
					}
				}

				
				if(!isCompletable)
				{
					Color prevColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Complete), GUILayout.Width(btnWidth), GUILayout.Height(height));//Complete

					GUI.backgroundColor = prevColor;
				}
			}
			else
			{
				bool isNextAvailable = false;

				if (_psdLoader.IsFileLoaded)
				{
					//공통적으로 PSD 파일이 로드된 상태여야 한다.
					if (_selectedPSDSet != null)
					{
						//1. PSD Set을 선택했다면 
						if (_step == RELOAD_STEP.Step1_SelectPSDSet)
						{
							isNextAvailable = true;
						}
						else
						{
							if (_selectedPSDSet._targetMeshGroupID >= 0
							&& _selectedPSDSet._linkedTargetMeshGroup != null)
							{
								isNextAvailable = true;
							}
						}
					}
					else if (_selectedPSDSecondary != null)
					{
						//2. PSD Secondary를 선택했다면
						if (_selectedPSDSecondary._linkedMainSet != null)
						{
							if (_step == RELOAD_STEP.Step1_SelectPSDSet)
							{
								isNextAvailable = true;
							}
							else
							{
								if (_selectedPSDSecondary._linkedMainSet._targetMeshGroupID >= 0
									&& _selectedPSDSecondary._linkedMainSet._linkedTargetMeshGroup != null)
								{
									isNextAvailable = true;
								}
							}
						}
					}
				}
				
				if(isNextAvailable)
				{
					if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Next) + " >", GUILayout.Width(btnWidth), GUILayout.Height(height)))//Next
					{
						MoveNext();
					}
				}
				else
				{
					Color prevColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Next) + " >", GUILayout.Width(btnWidth), GUILayout.Height(height));//Next

					GUI.backgroundColor = prevColor;
				}
			}

			GUILayout.Space(30);

			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(btnWidth_Cancel), GUILayout.Height(height)))//Close
			{
				//TODO.
				//if (IsGUIUsable)
				//{
				//	//bool result = EditorUtility.DisplayDialog("Close", "Close PSD Load? (Data is Not Saved)", "Close", "Cancel");

				//	bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ClosePSDImport_Title),
				//												_editor.GetText(TEXT.ClosePSDImport_Body),
				//												_editor.GetText(TEXT.Close),
				//												_editor.GetText(TEXT.Cancel));

				//	if (result)
				//	{
				//		OnLoadComplete(false);
				//		//CloseDialog();
				//		_isRequestCloseDialog = true;//<<바로 Close하면 안된다.
				//	}
				//}
				if(IsGUIUsable)
				{
					bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ClosePSDImport_Title),
															_editor.GetText(TEXT.ClosePSDImport_Body),
															_editor.GetText(TEXT.Close),
															_editor.GetText(TEXT.Cancel));

					if (result)
					{
						_isRequestCloseDialog = true;
					}
				}
				
			}



			EditorGUILayout.EndHorizontal();
		}

		//Center
		private void GUI_Center(int width, int height, Rect centerRect)
		{
			
			
			switch (_step)
			{
				case RELOAD_STEP.Step1_SelectPSDSet://GUI가 없다.
					GUI_Center_1_SelectPSDSet(width, height, centerRect);
					break;

				case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup://설정 + GUI (PSD / TextureData / MeshGroup)
					{
						if(_selectedPSDSet != null)
						{
							//PSD Set
							GUI_Center_2_FileLoadAndSelectMeshGroup_Main(width, height, centerRect);
						}
						else if(_selectedPSDSecondary != null)
						{
							//Secondary Set
							GUI_Center_2_FileLoadAndSelectMeshGroup_Secondary(width, height, centerRect);
						}
					}
					
					break;

				case RELOAD_STEP.Step3_LinkLayerToTransform://레이어 + TF Hierarchy + GUI (Layer + Mesh) , 하단에 매핑 툴
					{
						if(_selectedPSDSet != null)
						{
							//PSD Set
							GUI_Center_3_LinkLayerToTransform_Main(width, height, centerRect);
						}
						else if(_selectedPSDSecondary != null)
						{
							//Secondary Set
							GUI_Center_3_LinkLayerToTransform_Secondary(width, height, centerRect);
						}
					}
					
					break;

				case RELOAD_STEP.Step4_ModifyOffset://레이어 + GUI (Layer + Mesh) + 하단에 위치 보정 툴
					{
						if(_selectedPSDSet != null)
						{
							//PSD Set
							GUI_Center_4_ModifyOffset_Main(width, height, centerRect);
						}
						else if(_selectedPSDSecondary != null)
						{
							//Secondary Set
							GUI_Center_4_ModifyOffset_Secondary(width, height, centerRect);
						}
					}
					break;

				case RELOAD_STEP.Step5_AtlasSetting://GUI + Atlas 세팅
					{
						if (_selectedPSDSet != null)
						{
							//PSD Set
							GUI_Center_5_AtlasSetting_Main(width, height, centerRect);
						}
						else if (_selectedPSDSecondary != null)
						{
							//Secondary Set
							GUI_Center_5_AtlasSetting_Secondary(width, height, centerRect);
						}
					}
					
					break;
			}

			//추가 21.5.19 : 렌더링중인 Pass를 종료한다.
			_gl.EndPass();
		}


		private apGUIContentWrapper _guiContent_PSDSetIcon = null;
		private apGUIContentWrapper _guiContent_PSDSecondaryIcon = null;
		
		private apGUIContentWrapper _guiContent_Delete = null;



		// GUI Center 1 : Select PSD Set
		//-------------------------------------------------------------------------------
		



		
		


		


		private apPSDLoader.BAKE_SIZE BakeSizePSDSet2Loader(apPSDSet.BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case apPSDSet.BAKE_SIZE.s256:	return apPSDLoader.BAKE_SIZE.s256;
				case apPSDSet.BAKE_SIZE.s512:	return apPSDLoader.BAKE_SIZE.s512;
				case apPSDSet.BAKE_SIZE.s1024:	return apPSDLoader.BAKE_SIZE.s1024;
				case apPSDSet.BAKE_SIZE.s2048:	return apPSDLoader.BAKE_SIZE.s2048;
				case apPSDSet.BAKE_SIZE.s4096:	return apPSDLoader.BAKE_SIZE.s4096;
			}
			return apPSDLoader.BAKE_SIZE.s4096;
		}

		private apPSDSet.BAKE_SIZE BakeSizeLoader2PSDSet(apPSDLoader.BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case apPSDLoader.BAKE_SIZE.s256:	return apPSDSet.BAKE_SIZE.s256;
				case apPSDLoader.BAKE_SIZE.s512:	return apPSDSet.BAKE_SIZE.s512;
				case apPSDLoader.BAKE_SIZE.s1024:	return apPSDSet.BAKE_SIZE.s1024;
				case apPSDLoader.BAKE_SIZE.s2048:	return apPSDSet.BAKE_SIZE.s2048;
				case apPSDLoader.BAKE_SIZE.s4096:	return apPSDSet.BAKE_SIZE.s4096;
			}
			return apPSDSet.BAKE_SIZE.s4096;
		}

		private int GetBakeIntSize(apPSDSet.BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case apPSDSet.BAKE_SIZE.s256:	return 256;
				case apPSDSet.BAKE_SIZE.s512:	return 512;
				case apPSDSet.BAKE_SIZE.s1024:	return 1024;
				case apPSDSet.BAKE_SIZE.s2048:	return 2048;
				case apPSDSet.BAKE_SIZE.s4096:	return 4096;
			}
			return 4096;

		}
		// Bake Events
		//------------------------------------------------------------------------------------------------------
		private void OnCalculateResult(bool isSuccess, object loadKey, bool isWarning, string warningMsg)
		{

			if (isSuccess)
			{
				_loadKey_Calculated = loadKey;
				_isBakeWarning = false;
				_bakeWarningMsg = "";
			}
			else
			{
				if (isWarning)
				{
					_isBakeWarning = true;
					_bakeWarningMsg = warningMsg;
				}
				else
				{
					_isBakeWarning = false;
					_bakeWarningMsg = "";
				}
				_loadKey_Calculated = null;
			}

			_isNeedBakeCheck = false;
		}

		private void OnBakeResult(bool isSuccess, object loadKey)
		{
			if (isSuccess)
			{
				_loadKey_Bake = loadKey;
			}
			else
			{
				_loadKey_Bake = null;
			}
		}


		private void OnConvertResult(bool isSuccess, List<Texture2D> resultTextures)
		{
			if(isSuccess)
			{
				OnLoadComplete(true);
			}
		}


		//Atlas 생성에 성공했다.
		private void OnConvertResult_Secondary(bool isSuccess, List<Texture2D> resultTextures)
		{
			if(isSuccess)
			{
				if(_selectedPSDSecondary != null)
				{
					//Bake 기록을 저장하자
					_selectedPSDSecondary._isLastBaked = true;
					_selectedPSDSecondary._lastBaked_PSDImageWidth = _psdLoader.PSDImageWidth;
					_selectedPSDSecondary._lastBaked_PSDImageHeight = _psdLoader.PSDImageHeight;
					_selectedPSDSecondary._lastBaked_Scale100 = _psdLoader.BakedResizeX100;

					//Debug.Log("Convert Result");

					//레이어 Offset도 저장해두자
					//동일한 PSD 파일을 열때를 대비해서
					int nPSDBakedLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;
					if(nPSDBakedLayers > 0)
					{
						apPSDLayerData curPSDLayer = null;
						for (int i = 0; i < nPSDBakedLayers; i++)
						{
							curPSDLayer = _psdLoader.PSDLayerDataList[i];
							if(curPSDLayer._linkedBakedInfo_Secondary != null)
							{
								curPSDLayer._linkedBakedInfo_Secondary._lastBakedDeltaPixelPosOffsetX = curPSDLayer._remapPosOffsetDelta_X;
								curPSDLayer._linkedBakedInfo_Secondary._lastBakedDeltaPixelPosOffsetY = curPSDLayer._remapPosOffsetDelta_Y;

								//Debug.Log("Layer 기록 저장 : " + curPSDLayer._name + " (" + curPSDLayer._remapPosOffsetDelta_X + ", " + curPSDLayer._remapPosOffsetDelta_Y + ")");
							}
						}
					}

					EditorUtility.SetDirty(_portrait);
				}

				//Secondary의 경우 에디터 내에서는 변화가 없으므로, 별도의 안내창이 나와야 한다.
				if(resultTextures != null && resultTextures.Count > 0)
				{
					//텍스쳐 중 하나를 선택하자
					Selection.activeObject = resultTextures[0];

					string strTextures = "";
					for (int i = 0; i < resultTextures.Count; i++)
					{
						strTextures += "- " + resultTextures[i].name + "\n";
					}

					EditorUtility.DisplayDialog(
						_editor.GetText(TEXT.DLG_SecondaryTextureCreated_Title),
						strTextures + _editor.GetText(TEXT.DLG_SecondaryTextureCreated_Body),
						_editor.GetText(TEXT.Okay));
				}

				_isRequestCloseDialog = true;
			}
		}


		public void OnLoadComplete(bool isResult)
		{
			if (_funcResult != null && _selectedPSDSet != null)
			{
				if (isResult)
				{
					//float deltaScaleRatio = ((float)_psdLoader.BakedResizeX100 / (float)_selectedPSDSet._nextBakeScale100);//<<이전 Bake 확대 비율 대비 현재 Bake 확대 비율
					//Debug.Log("크기 변환 비율 : " + deltaScaleRatio + "(" + _psdLoader.BakedResizeX100 + " / " + _selectedPSDSet._nextBakeScale100 + ")");
					_funcResult(	isResult, 
									_loadKey, 
									_psdLoader.FileName, _psdLoader.FileFullPath,
									_psdLoader.PSDLayerDataList, 
									_psdLoader.BakedResizeX100, 
									_selectedPSDSet._next_meshGroupScaleX100,
									_selectedPSDSet._prev_bakeScale100,
									_psdLoader.PSDImageWidth, _psdLoader.PSDImageHeight,
									_psdLoader.BakedPadding, 
									GetBakeIntSize(_selectedPSDSet._bakeOption_Width),
									GetBakeIntSize(_selectedPSDSet._bakeOption_Height),
									_selectedPSDSet._bakeOption_MaximumNumAtlas,
									_selectedPSDSet._bakeOption_BlurOption,
									_selectedPSDSet._nextBakeCenterOffsetDelta_X,
									_selectedPSDSet._nextBakeCenterOffsetDelta_Y,
									_psdLoader._bakeDstPath,
									_psdLoader._bakeDstPathRelative,
									_selectedPSDSet 
									//deltaScaleRatio
									);
				}
				else
				{
					_funcResult(isResult, 
						_loadKey, 
						_psdLoader.FileName, _psdLoader.FileFullPath,
						null, 
						_psdLoader.BakedResizeX100, 
						_selectedPSDSet._next_meshGroupScaleX100,
						_selectedPSDSet._prev_bakeScale100,
						_psdLoader.PSDImageWidth, _psdLoader.PSDImageHeight,
						_psdLoader.BakedPadding, 
						GetBakeIntSize(_selectedPSDSet._bakeOption_Width),
						GetBakeIntSize(_selectedPSDSet._bakeOption_Height),
						_selectedPSDSet._bakeOption_MaximumNumAtlas,
						_selectedPSDSet._bakeOption_BlurOption,
						_selectedPSDSet._nextBakeCenterOffsetDelta_X,
						_selectedPSDSet._nextBakeCenterOffsetDelta_Y,
						_psdLoader._bakeDstPath,
						_psdLoader._bakeDstPathRelative,
						_selectedPSDSet
						//1.0f
						);
				}
			}
			//CloseDialog();
			_isRequestCloseDialog = true;
		}

		// GUI Base
		//--------------------------------------------------------------------------------------------------
		private void UpdateAndDrawGUIBase(Rect guiRect, Vector2 centerOffset)
		{
			MouseUpdate(guiRect);

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif
			bool isAlt = Event.current.alt;

			MouseScrollUpdate(guiRect, isCtrl, isAlt);

			//int scrollHeightOffset = 32;
			_scroll_GUI.y = GUI.VerticalScrollbar(new Rect(guiRect.xMin + guiRect.width - 15, guiRect.yMin, 20, guiRect.height - 15), _scroll_GUI.y, 5.0f, -100.0f, 100.0f + 5.0f);
			_scroll_GUI.x = GUI.HorizontalScrollbar(new Rect(guiRect.xMin, guiRect.yMin + (guiRect.height - 15), guiRect.width - 15, 20), _scroll_GUI.x, 5.0f, -100.0f, 100.0f + 5.0f);

			if (GUI.Button(new Rect(guiRect.xMin + guiRect.width - 15, guiRect.yMin + (guiRect.height - 15), 15, 15), ""))
			{
				_scroll_GUI = Vector2.zero;
				_iZoomX100 = ZOOM_INDEX_DEFAULT;
			}

			//Vector2 centerOffset = new Vector2(6, 0.3f);
			_gl.SetWindowSize(
						(int)guiRect.width, (int)guiRect.height,
						_scroll_GUI - centerOffset, (float)(_zoomListX100[_iZoomX100]) * 0.01f,
						(int)guiRect.x, (int)guiRect.y,
						(int)position.width, (int)position.height);

			

			_gl.DrawGrid();
			//_gl.DrawBoldLine(Vector2.zero, new Vector2(10, 10), 5, Color.yellow, true);
		}


		// Mouse Update
		//--------------------------------------------------------------------------------------------------
		private bool IsMouseInGUI(Vector2 mousePos, Rect mainGUIRect)
		{
			if (mousePos.x < 0 || mousePos.x > mainGUIRect.width
				|| mousePos.y < 0 || mousePos.y > mainGUIRect.height)
			{
				return false;
			}
			return true;
		}

		private void MouseUpdate(Rect mainGUIRect)
		{
			bool isMouseEvent = Event.current.rawType == EventType.ScrollWheel ||
				Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag ||
				Event.current.rawType == EventType.MouseMove ||
				Event.current.rawType == EventType.MouseUp;

			if (!isMouseEvent)
			{
				return;
			}

			Vector2 mousePos = Event.current.mousePosition - new Vector2(mainGUIRect.x, mainGUIRect.y);
			_mouse.SetMousePos(mousePos, Event.current.mousePosition);
			_mouse.ReadyToUpdate();

			if (Event.current.rawType == EventType.ScrollWheel)
			{
				Vector2 deltaValue = Event.current.delta;
				_mouse.Update_Wheel((int)(deltaValue.y * 10.0f));

			}
			else
			{
				int iMouse = -1;
				switch (Event.current.button)
				{
					case 0://Left
						iMouse = 0;
						break;

					case 1://Right
						iMouse = 1;
						break;

					case 2://Middle
						iMouse = 2;
						break;
				}


				if (iMouse >= 0)
				{
					_mouse.SetMouseBtn(iMouse);

					//GUI 기준 상대 좌표
					switch (Event.current.rawType)
					{
						case EventType.MouseDown:
							{
								if (IsMouseInGUI(mousePos, mainGUIRect))
								{
									//Editor._mouseBtn[iMouse].Update_Pressed(mousePos);
									_mouse.Update_Pressed();
								}
							}
							break;

						case EventType.MouseUp:
							{
								//Editor._mouseBtn[iMouse].Update_Released(mousePos);
								_mouse.Update_Released();
							}
							break;

						case EventType.MouseMove:
						case EventType.MouseDrag:
							{
								//Editor._mouseBtn[iMouse].Update_Moved(deltaValue);
								_mouse.Update_Moved();

							}
							break;

							//case EventType.ScrollWheel:
							//	{

							//	}
							//break;
					}

					_mouse.EndUpdate();
				}
			}
		}

		private bool MouseScrollUpdate(Rect mainGUIRect, bool isCtrl, bool isAlt)
		{
			if (_mouse.Wheel != 0)
			{
				//if(IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
				if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
				{
					if (_mouse.Wheel > 0)
					{
						//줌 아웃 = 인덱스 감소
						_iZoomX100--;
						if (_iZoomX100 < 0)
						{ _iZoomX100 = 0; }
					}
					else if (_mouse.Wheel < 0)
					{
						//줌 인 = 인덱스 증가
						_iZoomX100++;
						if (_iZoomX100 >= _zoomListX100.Length)
						{
							_iZoomX100 = _zoomListX100.Length - 1;
						}
					}

					//Editor.Repaint();
					//SetRepaint();
					//Debug.Log("Zoom [" + _zoomListX100[_iZoomX100] + "]");

					_mouse.UseWheel();
					_isCtrlAltDrag = false;
					return true;
				}
			}

			if (_mouse.ButtonIndex == 2)
			{
				if (_mouse.Status == apPSDMouse.MouseBtnStatus.Down ||
					_mouse.Status == apPSDMouse.MouseBtnStatus.Pressed)
				{
					//if (IsMouseInGUI(Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosLast))
					if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
					{
						Vector2 moveDelta = _mouse.PosDelta;
						//RealX = scroll * windowWidth * 0.1

						Vector2 sensative = new Vector2(
							1.0f / (mainGUIRect.width * 0.1f),
							1.0f / (mainGUIRect.height * 0.1f));

						_scroll_GUI.x -= moveDelta.x * sensative.x;
						_scroll_GUI.y -= moveDelta.y * sensative.y;


						_mouse.UseMouseDrag();
						_isCtrlAltDrag = false;
						return true;
					}
				}
			}


			//추가 : Ctrl+Alt 누르고 제어하기
			//Ctrl+Alt+좌클릭 드래그 : 화면 이동
			//Ctrl+Alt+우클릭 드래그 : 화면 확대
			if (isCtrl && isAlt)
			{
				if (_mouse.Status == apPSDMouse.MouseBtnStatus.Down ||
					_mouse.Status == apPSDMouse.MouseBtnStatus.Pressed)
				{
					if (IsMouseInGUI(_mouse.PosLast, mainGUIRect))
					{
						Vector2 moveDelta = _mouse.PosDelta;
						if(!_isCtrlAltDrag)
						{
							moveDelta = Vector2.zero;
						}
						if (_mouse.ButtonIndex == 0)
						{
							//Ctrl+Alt로 화면 이동
							Vector2 sensative = new Vector2(
							1.0f / (mainGUIRect.width * 0.1f),
							1.0f / (mainGUIRect.height * 0.1f));


							_scroll_GUI.x -= moveDelta.x * sensative.x;
							_scroll_GUI.y -= moveDelta.y * sensative.y;

							//처리 끝
							_mouse.UseMouseDrag();
							_isCtrlAltDrag = true;
							return true;
						}
						else if (_mouse.ButtonIndex == 1)
						{
							//Ctrl+Alt로 화면 확대/축소
							float wheelOffset = 0.0f;
							if(Mathf.Abs(moveDelta.x) * 1.5f > Mathf.Abs(moveDelta.y))
							{
								wheelOffset = moveDelta.x;
							}
							else
							{
								wheelOffset = moveDelta.y;
							}
							float zoomPrev = _zoomListX100[_iZoomX100] * 0.01f;
							if (wheelOffset < -1.3f)
							{
								//줌 아웃 = 인덱스 감소
								_iZoomX100--;
								if (_iZoomX100 < 0)	{ _iZoomX100 = 0; }
							}
							else if (wheelOffset > 1.3f)
							{
								//줌 인 = 인덱스 증가
								_iZoomX100++;
								if (_iZoomX100 >= _zoomListX100.Length)
								{
									_iZoomX100 = _zoomListX100.Length - 1;
								}
							}


							//마우스의 World 좌표는 같아야 한다.
							float zoomNext = _zoomListX100[_iZoomX100] * 0.01f;

							//중심점의 위치를 구하자 (Editor GL 기준)
							Vector2 scroll = new Vector2(_scroll_GUI.x * 0.1f * _gl.WindowSize.x,
														_scroll_GUI.y * 0.1f * _gl.WindowSize.y);
							Vector2 guiCenterPos = _gl.WindowSizeHalf - scroll;

							Vector2 deltaMousePos = _mouse.PosLast - guiCenterPos;//>>이후
							Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

							//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자

							//>>변경
							float nextScrollX = ((nextDeltaMousePos.x - _mouse.PosLast.x) + _gl.WindowSizeHalf.x) / (0.1f * _gl.WindowSize.x);
							float nextScrollY = ((nextDeltaMousePos.y - _mouse.PosLast.y) + _gl.WindowSizeHalf.y) / (0.1f * _gl.WindowSize.y);

							nextScrollX = Mathf.Clamp(nextScrollX, -500.0f, 500.0f);
							nextScrollY = Mathf.Clamp(nextScrollY, -500.0f, 500.0f);

							_scroll_GUI.x = nextScrollX;
							_scroll_GUI.y = nextScrollY;

							//처리 끝
							_mouse.UseMouseDrag();
							_isCtrlAltDrag = true;
							return true;
						}
						else
						{
							_isCtrlAltDrag = false;
						}
					}
				}
				
			}

			_isCtrlAltDrag = false;
			return false;
		}


		//------------------------------------------------------------------------------------
		private void MoveNext()
		{
			if (!IsGUIUsable)
			{
				return;
			}
			switch (_step)
			{
				case RELOAD_STEP.Step1_SelectPSDSet:				_step = RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup;	break;
				case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup:	_step = RELOAD_STEP.Step3_LinkLayerToTransform;	break;
				case RELOAD_STEP.Step3_LinkLayerToTransform:		_step = RELOAD_STEP.Step4_ModifyOffset;	break;
				case RELOAD_STEP.Step4_ModifyOffset:				_step = RELOAD_STEP.Step5_AtlasSetting;	break;
				case RELOAD_STEP.Step5_AtlasSetting:				_step = RELOAD_STEP.Step5_AtlasSetting;	break;
			}


			if(_step == RELOAD_STEP.Step3_LinkLayerToTransform)
			{
				//다음이 Step3라면
				//_selectedPSDSet._linkedTargetMeshGroup.ResetRenderUnits();
				RefreshTransform2PSDLayer();
				MakeTransformDataList();

				_isLinkGUIColoredList = false;
				_isLinkOverlayColorRender = false;

				if (_psdLoader.PSDLayerDataList != null && _psdLoader.PSDLayerDataList.Count > 0)
				{
					_selectedPSDLayerData = _psdLoader.PSDLayerDataList[_psdLoader.PSDLayerDataList.Count - 1];
				}
				else
				{
					_selectedPSDLayerData = null;
				}
			}
			else if(_step == RELOAD_STEP.Step4_ModifyOffset)
			{
				RefreshTransform2PSDLayer();
				_selectedPSDLayerData = null;

				int nPSDLayerData = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;
				if (nPSDLayerData > 0)
				{
					_selectedPSDLayerData = _psdLoader.PSDLayerDataList[nPSDLayerData - 1];

					
					//버그 v1.4.2
					//Secondary 편집시 만약 PSD Set Layer중에 Secondary의 대상이 아닌 경우가 있다면,
					//해당 레이어를 선택하지 말고, Secondary중 가장 최상단(뒤쪽)에 위치한 것을 찾아서 선택해야한다.
					if(_selectedPSDSecondary != null
						&& _selectedPSDLayerData._linkedBakedInfo_Secondary == null)
					{
						//Debug.Log("첫번째 선택된 레이어가 Secondary의 대상이 아니다.");
						_selectedPSDLayerData = null;//일단 다시 선택 해제

						//뒤에서부터 찾자
						for (int iLayer = nPSDLayerData - 1; iLayer >= 0; iLayer--)
						{
							apPSDLayerData curLayerData = _psdLoader.PSDLayerDataList[iLayer];
							if(curLayerData._linkedBakedInfo_Secondary != null)
							{
								//유효한 걸 찾았다.
								_selectedPSDLayerData = curLayerData;
								break;
							}
						}
					}
				}
				else
				{
					_selectedPSDLayerData = null;
				}

				
			}

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			_isNeedBakeCheck = true;
			_isBakeWarning = false;
			_bakeWarningMsg = "";
			_loadKey_Calculated = null;
			_loadKey_Bake = null;

			_renderMode_PSD = RENDER_MODE.Normal;
			_renderMode_Mesh = RENDER_MODE.Normal;

			if(_step == RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup)
			{
				_renderMode_PSD = RENDER_MODE.Outline;//<<첫 화면은 Outline 모드
			}

			apEditorUtil.ReleaseGUIFocus();
		}

		private void MovePrev()
		{
			if (!IsGUIUsable)
			{
				return;
			}

			switch (_step)
			{
				case RELOAD_STEP.Step1_SelectPSDSet:				_step = RELOAD_STEP.Step1_SelectPSDSet;	break;
				case RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup:	_step = RELOAD_STEP.Step1_SelectPSDSet;	break;
				case RELOAD_STEP.Step3_LinkLayerToTransform:		_step = RELOAD_STEP.Step2_FileLoadAndSelectMeshGroup;	break;
				case RELOAD_STEP.Step4_ModifyOffset:				_step = RELOAD_STEP.Step3_LinkLayerToTransform;	break;
				case RELOAD_STEP.Step5_AtlasSetting:				_step = RELOAD_STEP.Step4_ModifyOffset;	break;
			}

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			_isNeedBakeCheck = true;
			_isBakeWarning = false;
			_bakeWarningMsg = "";
			_loadKey_Calculated = null;
			_loadKey_Bake = null;

			_renderMode_PSD = RENDER_MODE.Normal;
			_renderMode_Mesh = RENDER_MODE.Normal;

			if(_step == RELOAD_STEP.Step1_SelectPSDSet)
			{
				//이전으로 돌아갈땐 PSD 정보 갱신
				RefreshPSDFileInfo();
			}

			apEditorUtil.ReleaseGUIFocus();
		}



		//-----------------------------------------------------------------------
		private void SetGUIVisible(string keyName, bool isVisible)
		{
			if (_delayedGUIShowList.ContainsKey(keyName))
			{
				if (_delayedGUIShowList[keyName] != isVisible)
				{
					_delayedGUIShowList[keyName] = isVisible;//Visible 조건 값

					//_delayedGUIToggledList는 "Visible 값이 바뀌었을때 그걸 바로 GUI에 적용했는지"를 저장한다.
					//바뀐 순간엔 GUI에 적용 전이므로 "Visible Toggle이 완료되었는지"를 저장하는 리스트엔 False를 넣어둔다.
					_delayedGUIToggledList[keyName] = false;
				}
			}
			else
			{
				_delayedGUIShowList.Add(keyName, isVisible);
				_delayedGUIToggledList.Add(keyName, false);
			}
		}

		public bool IsDelayedGUIVisible(string keyName)
		{
			//GUI Layout이 출력되려면
			//1. Visible 값이 True여야 한다.
			//2-1. GUI Event가 Layout/Repaint 여야 한다.
			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다.


			//1-1. GUI Layout의 Visible 여부를 결정하는 값이 없다면 -> False
			if (!_delayedGUIShowList.ContainsKey(keyName))
			{
				return false;
			}

			//1-2. GUI Layout의 Visible 값이 False라면 -> False
			if (!_delayedGUIShowList[keyName])
			{
				return false;
			}

			//2. (Toggle 처리가 완료되지 않은 상태에서..)
			if (!_delayedGUIToggledList[keyName])
			{
				//2-1. GUI Event가 Layout/Repaint라면 -> 다음 OnGUI까지 일단 보류합니다. False
				if (!_isGUIEvent)
				{
					return false;
				}

				// GUI Event가 유효하다면 Visible이 가능하다고 바꿔줍니다.
				//_delayedGUIToggledList [False -> True]
				_delayedGUIToggledList[keyName] = true;
			}

			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다. -> True
			return true;
		}


		// GL
		//-------------------------------------------------------------------------------------------------
		private void DrawPSD(bool isDrawToneOutline, apPSDLayerData selectedLayerData, float posX, float posY, int bakeScale100)
		{
			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}
			if(_psdLoader.PSDLayerDataList.Count == 0)
			{
				return;
			}
			Vector2 renderOffset = new Vector2(posX, posY);
			Vector2 renderScale = new Vector2((float)bakeScale100 * 0.01f, (float)bakeScale100 * 0.01f);

			apPSDLayerData curImageLayer = null;
			apMatrix worldMat = new apMatrix();
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				curImageLayer = _psdLoader.PSDLayerDataList[i];
				if(curImageLayer._image == null)
				{
					continue;
				}

				//bool isSelected = (curImageLayer == selectedLayerData);
				//apMatrix3x3 worldMat = apMatrix3x3.TRS((curImageLayer._posOffset - _psdLoader.PSDCenterOffset) + renderOffset, 0.0f, renderScale);
				//apMatrix3x3 worldMat = new apMatrix3x3();
				worldMat.SetIdentity();
				worldMat.SetPos((curImageLayer._posOffset - _psdLoader.PSDCenterOffset) + renderOffset, false);
				worldMat.RMultiply(Vector2.zero, 0.0f, renderScale, false);
				worldMat.MakeMatrix();
				
				Color meshColor = curImageLayer._transparentColor2X;
				//if(isAlpha)
				//{
				//	meshColor.a *= 0.5f;
				//}
				if(isDrawToneOutline)
				{
					meshColor = _psdOverlayColor;
					meshColor.a = 0.8f;
				}
				_gl.DrawTexture(curImageLayer._image,
										worldMat.MtrxToSpace,
										curImageLayer._width, curImageLayer._height,
										meshColor,
										0.0f, isDrawToneOutline);

				//_gl.DrawTexture(curImageLayer._image,
				//						//curImageLayer._posOffset - _imageCenterPosOffset,
				//						(curImageLayer._posOffset - _psdLoader.PSDCenterOffset) + renderOffset,
				//						curImageLayer._width, curImageLayer._height,
				//						curImageLayer._transparentColor2X,
				//						isSelected);
			}
		}

		private void DrawMeshGroup(apMeshGroup meshGroup)
		{
			if(meshGroup == null)
			{
				return;
			}

			//중요 > meshGroup의 RootUnit의 Transform을 역으로 만들어야 한다.

			meshGroup.RefreshForce();
			apMatrix rootMatrix = null;
			if(meshGroup._rootMeshGroupTransform != null)
			{
				//Debug.Log("Root Matrix : " + meshGroup._rootMeshGroupTransform._matrix.ToString());
				rootMatrix = meshGroup._rootMeshGroupTransform._matrix;
				
			}
			
			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				renderUnit.CalculateWorldPositionWithoutModifier();//<NoMod Pos를 계산한다.
			}

			for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			{
				apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
				
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						//if(!renderUnit._meshTransform._isVisible_Default)
						//{
						//	continue;
						//}

						if (renderUnit._meshTransform._isClipping_Parent)
						{
							//Profiler.BeginSample("Render - Mask Unit");

							if (renderUnit._meshTransform._isVisible_Default)
							{
								_gl.DrawRenderUnit_ClippingParent_Renew(renderUnit, renderUnit._meshTransform._clipChildMeshes, null, rootMatrix);
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//렌더링은 생략한다.
						}
						else
						{
							//Profiler.BeginSample("Render - Normal Unit");

							if (renderUnit._meshTransform._isVisible_Default)
							{
								_gl.DrawRenderUnit(renderUnit, rootMatrix);
							}
						}

					}
				}
			}

			//Debug.Log("Render MeshGroup : " + nRendered);
		}

		private void DrawTextureData(apTextureData textureData, bool isDrawOutline)
		{
			if(textureData == null || textureData._image == null)
			{
				return;
			}

			_gl.DrawTexture(textureData._image,
										Vector2.zero,
										textureData._width, textureData._height,
										new Color(0.5f, 0.5f, 0.5f, 1.0f),
										isDrawOutline);
		}


		private void DrawTextureData(apTextureData textureData, bool isDrawOutline, Vector2 centerPosOffset)
		{
			if(textureData == null || textureData._image == null)
			{
				return;
			}

			if(_samplingMethod == TEXTURE_SAMPLING.Default)
			{
				//기본 텍스쳐로 렌더링
				_gl.DrawTexture(textureData._image,
										centerPosOffset,
										textureData._width, textureData._height,
										new Color(0.5f, 0.5f, 0.5f, 1.0f),
										isDrawOutline);
			}
			else
			{
				//Nearest 텍스쳐로 렌더링
				//Nearest 텍스쳐 사용
				Texture2D defaultTexture = textureData._image;
				Texture2D nearestTexture = null;
				if (_meshTexture2NearestTex != null)
				{
					_meshTexture2NearestTex.TryGetValue(defaultTexture, out nearestTexture);
				}

				if (nearestTexture != null)
				{
					_gl.DrawTexture(nearestTexture,
										centerPosOffset,
										textureData._width, textureData._height,
										new Color(0.5f, 0.5f, 0.5f, 1.0f),
										isDrawOutline);
				}
				else
				{
					_gl.DrawTexture(textureData._image,
										centerPosOffset,
										textureData._width, textureData._height,
										new Color(0.5f, 0.5f, 0.5f, 1.0f),
										isDrawOutline);
				}
			}
			
		}

		private void DrawTextureData_Transparent(apTextureData textureData, bool isDrawOutline, Vector2 centerPosOffset)
		{
			if(textureData == null || textureData._image == null)
			{
				return;
			}

			if (_samplingMethod == TEXTURE_SAMPLING.Default)
			{
				//기본 텍스쳐로 렌더링
				_gl.DrawTexture(textureData._image,
											centerPosOffset,
											textureData._width, textureData._height,
											_meshOverlayColor,
											isDrawOutline,
											true);
			}
			else
			{
				//Nearest 텍스쳐로 렌더링
				//Nearest 텍스쳐 사용
				Texture2D defaultTexture = textureData._image;
				Texture2D nearestTexture = null;
				if (_meshTexture2NearestTex != null)
				{
					_meshTexture2NearestTex.TryGetValue(defaultTexture, out nearestTexture);
				}

				if (nearestTexture != null)
				{
					_gl.DrawTexture(nearestTexture,
											centerPosOffset,
											textureData._width, textureData._height,
											_meshOverlayColor,
											isDrawOutline,
											true);
				}
				else
				{
					_gl.DrawTexture(textureData._image,
											centerPosOffset,
											textureData._width, textureData._height,
											_meshOverlayColor,
											isDrawOutline,
											true);
				}
			}
		}





		private void DrawMesh(apMesh mesh, bool isShowAllTexture, bool isDrawEdge, float scale)
		{
			if(mesh == null
				|| mesh.LinkedTextureData == null
				|| mesh.LinkedTextureData._image == null)
			{
				return;
			}

			if (_samplingMethod == TEXTURE_SAMPLING.Default)
			{
				//기본 텍스쳐 사용
				_gl.DrawMesh(mesh,
							apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)),
							new Color(0.5f, 0.5f, 0.5f, 1.0f),
							isShowAllTexture, true, isDrawEdge, false);
			}
			else
			{
				//Nearest 텍스쳐 사용
				Texture2D defaultTexture = mesh.LinkedTextureData._image;
				Texture2D nearestTexture = null;
				if (_meshTexture2NearestTex != null)
				{
					_meshTexture2NearestTex.TryGetValue(defaultTexture, out nearestTexture);
				}

				if (nearestTexture != null)
				{
					//Nearest Texture 샘플링으로 출력한다.
					_gl.DrawMesh(mesh,
									apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)),
									new Color(0.5f, 0.5f, 0.5f, 1.0f),
									isShowAllTexture, true, isDrawEdge, false,
									nearestTexture);
				}
				else
				{
					//기본 텍스쳐로 출력한다.
					_gl.DrawMesh(mesh,
									apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)),
									new Color(0.5f, 0.5f, 0.5f, 1.0f),
									isShowAllTexture, true, isDrawEdge, false);
				}
			}
			

			
		}


		private void DrawMeshToneColor(apMesh mesh, bool isShowAllTexture, float scale)
		{
			if(mesh == null || mesh.LinkedTextureData == null)
			{
				return;
			}

			if (_samplingMethod == TEXTURE_SAMPLING.Default)
			{
				//기본 텍스쳐 사용
				_gl.DrawMesh(mesh,
							apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)),
							_meshOverlayColor,
							isShowAllTexture,
							true, false,
							true, //ToneColor
							null);
			}
			else
			{
				//Nearest 텍스쳐 사용
				Texture2D defaultTexture = mesh.LinkedTextureData._image;
				Texture2D nearestTexture = null;
				if (_meshTexture2NearestTex != null)
				{
					_meshTexture2NearestTex.TryGetValue(defaultTexture, out nearestTexture);
				}

				if (nearestTexture != null)
				{
					//Nearest Texture 샘플링으로 출력한다.
					_gl.DrawMesh(mesh,
									apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)),
									_meshOverlayColor,
									isShowAllTexture, true, false, true,
									nearestTexture);
				}
				else
				{
					//기본 텍스쳐로 출력한다.
					_gl.DrawMesh(mesh,
									apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)),
									_meshOverlayColor,
									isShowAllTexture, true, false, true);
				}
			}

			
		}

		//private void DrawMesh(apMesh mesh, bool isShowAllTexture, bool isDrawEdge, Color color)
		//{
		//	if(mesh == null || mesh.LinkedTextureData == null)
		//	{
		//		return;
		//	}
		//	_gl.DrawMesh(mesh, apMatrix3x3.identity, color, isShowAllTexture, true, isDrawEdge);
		//}
		private void DrawMeshEdgeOnly(apMesh mesh, float scale)
		{
			if(mesh == null || mesh.LinkedTextureData == null)
			{
				return;
			}
			_gl.DrawMeshEdgeOnly(mesh, apMatrix3x3.TRS(Vector2.zero, 0.0f, new Vector2(scale, scale)));
		}


		private void DrawPSDLayer(	apPSDLayerData layerData,
									float posX,
									float posY,
									int bakeScale100,
									bool isDrawOutline)
		{
			Vector2 renderOffset = new Vector2(posX, posY);
			Vector2 renderScale = new Vector2((float)bakeScale100 * 0.01f, (float)bakeScale100 * 0.01f);

			apMatrix worldMat = new apMatrix();

			worldMat.SetIdentity();
			//worldMat.SetPos((layerData._posOffset - _psdLoader.PSDCenterOffset) + renderOffset);
			worldMat.SetPos(renderOffset, false);
			worldMat.RMultiply(Vector2.zero, 0.0f, renderScale, false);
			worldMat.MakeMatrix();
				
			Color meshColor = layerData._transparentColor2X;
			if(isDrawOutline)
			{
				meshColor = _psdOverlayColor;
			}

			if(_samplingMethod == TEXTURE_SAMPLING.Default)
			{
				_gl.DrawTexture(layerData._image,
							worldMat.MtrxToSpace,
							layerData._width, layerData._height,
							meshColor,
							0.0f, isDrawOutline);
			}
			else
			{
				//Nearest Texture를 사용한다.
				_gl.DrawTexture(layerData._image_Nearest,
							worldMat.MtrxToSpace,
							layerData._width, layerData._height,
							meshColor,
							0.0f, isDrawOutline);
			}
		}


		//private void DrawPSDLayer(apPSDLayerData layerData, float posX, float posY, int bakeScale100, Color color2X)
		//{
		//	Vector2 renderOffset = new Vector2(posX, posY);
		//	Vector2 renderScale = new Vector2((float)bakeScale100 * 0.01f, (float)bakeScale100 * 0.01f);

		//	apMatrix worldMat = new apMatrix();

		//	worldMat.SetIdentity();
		//	//worldMat.SetPos((layerData._posOffset - _psdLoader.PSDCenterOffset) + renderOffset);
		//	worldMat.SetPos(renderOffset);
		//	worldMat.RMultiply(Vector2.zero, 0.0f, renderScale);
				
		//	_gl.DrawTexture(layerData._image,
		//					worldMat.MtrxToSpace,
		//					layerData._width, layerData._height,
		//					color2X,
		//					0.0f);
		//}


		private void DrawText(string strText, float posX, float posY)
		{
			if(string.IsNullOrEmpty(strText))
			{
				return;
			}
			int textWidth = (strText.Length * 24);
			GUI.Label(new Rect(posX, posY, textWidth + 50, 30.0f), strText, _guiStyle_GLText);
		}

		private void DrawTextWarning(string strText, float posX, float posY)
		{
			if(string.IsNullOrEmpty(strText))
			{
				return;
			}
			int textWidth = (strText.Length * 24);
			GUI.Label(new Rect(posX, posY, textWidth + 50, 30.0f), strText, _guiStyle_GLTextWarning);
		}

		//----------------------------------------------------------------------------------------------
		private void SelectPSDSet(apPSDSet psdSet)
		{
			_selectedPSDSecondary = null;

			if(_selectedPSDSet == psdSet)
			{
				return;
			}
			_selectedPSDSet = psdSet;
			_selectedPSDSecondary = null;//Secondary는 선택 해제
			_psdLoader.Clear();//<<PSD Loader 초기화
			_selectedTextureData = null;
			_selectedSecondaryTexInfo = null;
			//_selectedPSDSetLayer = null;

			//MeshGroup/TextureData와 연결을 해주자
			if(_selectedPSDSet._targetMeshGroupID < 0)
			{
				_selectedPSDSet._linkedTargetMeshGroup = null;
			}
			else
			{
				_selectedPSDSet._linkedTargetMeshGroup = _portrait.GetMeshGroup(_selectedPSDSet._targetMeshGroupID);
				if(_selectedPSDSet._linkedTargetMeshGroup == null)
				{
					_selectedPSDSet._targetMeshGroupID = -1;
				}
			}

			if(_selectedPSDSet._targetTextureDataList == null)
			{
				_selectedPSDSet._targetTextureDataList = new List<apPSDSet.TextureDataSet>();
			}

			for (int iTex = 0; iTex < _selectedPSDSet._targetTextureDataList.Count; iTex++)
			{
				apPSDSet.TextureDataSet texDataSet = _selectedPSDSet._targetTextureDataList[iTex];
				if(texDataSet._textureDataID >= 0)
				{
					texDataSet._linkedTextureData = _portrait.GetTexture(texDataSet._textureDataID);
					if(texDataSet._linkedTextureData == null)
					{
						texDataSet._textureDataID = -1;
					}
				}
				else
				{
					texDataSet._linkedTextureData = null;
				}
			}
			_selectedPSDSet._targetTextureDataList.RemoveAll(delegate(apPSDSet.TextureDataSet a)
			{
				return a._textureDataID < 0;
			});


			//PSD File을 열자
			_selectedPSDSet.RefreshPSDFilePath();
			if (_selectedPSDSet.IsValidPSDFile)
			{
				//유효한 PSD File인 경우 PSD Loader로 열자
				_psdLoader.Step1_LoadPSDFile(_selectedPSDSet._filePath, 
					(_selectedPSDSet._isLastBaked ? _selectedPSDSet._lastBakedAssetName : "")
					);
			}

			if (_selectedPSDSet._isLastBaked)
			{
				_selectedPSDSet._next_meshGroupScaleX100 = _selectedPSDSet._lastBaked_MeshGroupScaleX100;//<<Bake 크기를 지정
				_selectedPSDSet._prev_bakeScale100 = _selectedPSDSet._bakeScale100;
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = _selectedPSDSet._lastBaked_PSDCenterOffsetDelta_X;
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = _selectedPSDSet._lastBaked_PSDCenterOffsetDelta_Y;
			}
			else
			{
				_selectedPSDSet._next_meshGroupScaleX100 = 100;
				_selectedPSDSet._prev_bakeScale100 = 100;
				_selectedPSDSet._nextBakeCenterOffsetDelta_X = 0;
				_selectedPSDSet._nextBakeCenterOffsetDelta_Y = 0;
			}
			

			

			//LoadKey 모두 초기화
			_loadKey_SelectMeshGroup = null;
			_loadKey_SelectTextureData = null;

			//_psdRenderPosOffset_X = 0;
			//_psdRenderPosOffset_Y = 0;

			//_selectedPSDSetLayer = null;
			_selectedTextureData = null;
			_selectedSecondaryTexInfo = null;
			_selectedPSDLayerData = null;

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;

			MakeRemappingList();
			MakeTransformDataList();
		}


		//보조 PSD Set을 선택한다.
		//원본이 되는 PSD Set도 연결해야한다.
		private void SelectPSDSecondary(apPSDSecondarySet psdSecondary, bool isForce)
		{
			_selectedPSDSet = null;
			if(_selectedPSDSecondary == psdSecondary && !isForce)
			{
				return;
			}
			_selectedPSDSecondary = psdSecondary;
			_psdLoader.Clear();//<<PSD Loader 초기화
			_selectedTextureData = null;
			_selectedSecondaryTexInfo = null;

			//원본이 되는 PSD Set을 찾자
			apPSDSet linkedPSDSet = null;
			if(_portrait._bakedPsdSets != null)
			{
				linkedPSDSet = _portrait._bakedPsdSets.Find(delegate(apPSDSet a)
				{
					return a._uniqueID == _selectedPSDSecondary._mainPSDSetID;
				});
			}

			if(linkedPSDSet == null)
			{
				//메인을 찾지 못했다면
				_selectedPSDSecondary._linkedMainSet = null;
				return;
			}

			if(_selectedPSDSecondary._isLastBaked)
			{
				//_selectedPSDSecondary._nextBakeCenterOffsetDelta_X = _selectedPSDSecondary._lastBaked_PSDCenterOffsetDelta_X;
				//_selectedPSDSecondary._nextBakeCenterOffsetDelta_Y = _selectedPSDSecondary._lastBaked_PSDCenterOffsetDelta_Y;
				//_selectedPSDSecondary._next_meshGroupScaleX100 = _selectedPSDSecondary._lastBaked_MeshGroupScaleX100;
				_selectedPSDSecondary._next_bakeScale100 = _selectedPSDSecondary._lastBaked_Scale100;
			}
			else
			{
				//_selectedPSDSecondary._nextBakeCenterOffsetDelta_X = 0;
				//_selectedPSDSecondary._nextBakeCenterOffsetDelta_Y = 0;
				//_selectedPSDSecondary._next_meshGroupScaleX100 = 100;

				if(linkedPSDSet._isLastBaked)
				{
					_selectedPSDSecondary._next_bakeScale100 = linkedPSDSet._bakeScale100;//Bake기록이 없다면 연결된 PSD Set의 기록을 가져온다.
				}
				else
				{
					_selectedPSDSecondary._next_bakeScale100 = 100;
				}
			}

			////Bake Scale은 원본을 따른다.
			//if(linkedPSDSet._isLastBaked)
			//{
			//	_selectedPSDSecondary._prev_bakeScale100 = linkedPSDSet._bakeScale100;
			//}
			//else
			//{
			//	_selectedPSDSecondary._prev_bakeScale100 = 100;
			//}
			
			


			//원본을 연결하자 (레이어 포함)
			_selectedPSDSecondary._linkedMainSet = linkedPSDSet;

			_selectedPSDSecondary.ClearTextureDataInfo();

			int nLayers = _selectedPSDSecondary._layers != null ? _selectedPSDSecondary._layers.Count : 0;
			int nSrcLayers = linkedPSDSet._layers != null ? linkedPSDSet._layers.Count : 0;

			if (nLayers > 0)
			{
				apPSDSecondarySetLayer curLayer = null;
				for (int i = 0; i < nLayers; i++)
				{
					curLayer = _selectedPSDSecondary._layers[i];
					if(nSrcLayers > 0)
					{
						//연결할 것을 찾자
						curLayer._linkedMainLayer = linkedPSDSet._layers.Find(delegate(apPSDSetLayer a)
						{
							return a._bakedUniqueID > 0 && curLayer._bakedUniqueID == a._bakedUniqueID;

							//return a._layerIndex == curLayer._mainLayerIndex
							//		&& string.Equals(a._name, curLayer._mainLayerName);
						});

						if(curLayer._linkedMainLayer != null)
						{
							//Texture Data도 연결하자
							curLayer._linkedTextureData = _portrait.GetTexture(curLayer._linkedMainLayer._textureDataID);

							//어떤 TextureData를 참조하는지 기록한다.
							_selectedPSDSecondary.AddTextureDataInfo(curLayer._linkedMainLayer._textureDataID, curLayer._linkedTextureData);
						}
					}
					else
					{
						curLayer._linkedMainLayer = null;//연결 불가
						curLayer._linkedTextureData = null;
					}
				}
			}
			

			//소스 PSD Set 연결을 다시 해준다. (필요한 만큼만)

			//MeshGroup/TextureData와 연결을 해주자
			if(linkedPSDSet._targetMeshGroupID < 0)
			{
				linkedPSDSet._linkedTargetMeshGroup = null;
			}
			else
			{
				linkedPSDSet._linkedTargetMeshGroup = _portrait.GetMeshGroup(linkedPSDSet._targetMeshGroupID);
			}

			//(텍스쳐 데이터는 연결하지 않는다.)
			
			//PSD File을 열자
			
			//LoadKey 모두 초기화
			_loadKey_SelectMeshGroup = null;
			_loadKey_SelectTextureData = null;

			_selectedTextureData = null;
			_selectedSecondaryTexInfo = null;
			_selectedPSDLayerData = null;

			_linkSrcLayerData = null;
			_isLinkLayerToTransform = false;


			//PSD Loader로 열자
			if(!string.IsNullOrEmpty(_selectedPSDSecondary._psdFilePath))
			{
				FileInfo fi = new FileInfo(_selectedPSDSecondary._psdFilePath);
				if (fi.Exists)
				{
					//PSD Load로 Secondary를 위한 PSD 파일을 열자
					_psdLoader.Step1_LoadPSDFile(	_selectedPSDSecondary._psdFilePath,
													(_selectedPSDSecondary._isLastBaked ? _selectedPSDSecondary._bakedTextureAssetName : "")
												);
				}
			}
			

			MakeRemappingList();
			//MakeTransformDataList();

			//Secondary에 맞게 연결을 하자

			//Transform List는 사용하지 않는다.
			if(_targetTransformList == null)
			{
				_targetTransformList = new List<TargetTransformData>();
			}
			_targetTransformList.Clear();
		}







		private void MakeRemappingList()
		{
			//_remapList.Clear();
			//_remapList_Psd2Map.Clear();

			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				//둘다 선택되지 않았다면
				return;
			}

			apPSDLayerData psdLayer = null;

			//PSD 파일로부터 생성된 Layer 데이터
			//일단 모두 초기화
			for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
			{
				psdLayer = _psdLoader.PSDLayerDataList[i];

				psdLayer._isRemapSelected = false;
				psdLayer._isBakable = true;
				psdLayer._remap_TransformID = -1;
				psdLayer._remap_MeshTransform = null;
				psdLayer._remap_MeshGroupTransform = null;

				psdLayer._linkedBakedInfo_Secondary = null;
			}


			//1. 기본 (PSD Set을 선택했을때)

			if (_selectedPSDSet != null)
			{
				if (_selectedPSDSet._isLastBaked && _selectedPSDSet._linkedTargetMeshGroup != null)
				{
					//Debug.Log("PSD Set Layer Remap [" + _selectedPSDSet._layers.Count + "]");
					//1. 만약 Bake된게 있다면
					//- PSDSetLayer에 따라서 찾자
					//- 이름과 레이어 번호를 기준으로 하자
					//- 검색 순서는 PSDLayerData <- PSDSetLayer 역으로
					apPSDSetLayer setLayer = null;
					int nLayers = _selectedPSDSet._layers != null ? _selectedPSDSet._layers.Count : 0;

					for (int iSetLayer = 0; iSetLayer < nLayers; iSetLayer++)
					{
						setLayer = _selectedPSDSet._layers[iSetLayer];

						if (!setLayer._isBaked)
						{
							//일단 Baked된 것부터 찾고 연결하자.
							continue;
						}

						//Bake된적이 있는 레이어를 대상으로 적절한 PSD Layer Data를 찾자
						apPSDLayerData srcLayerData = _psdLoader.FindMatchedLayerData_Baked(setLayer);

						#region [미사용 코드] FindMatchedLayerData_Baked 함수에 이 코드가 들어갔다.
						////1. 이름과 레이어가 같은 PSD LayerData를 찾자
						//apPSDLayerData srcLayerData = _psdLoader.PSDLayerDataList.Find(delegate (apPSDLayerData a)
						//{
						//	return a._layerIndex == setLayer._layerIndex
						//			&& string.Equals(a._name, setLayer._name)
						//			&& a._isImageLayer == setLayer._isImageLayer;

						//});

						//if (srcLayerData == null)
						//{
						//	//2. 없다면> 이름만이라도 같은게 있으면 오케이
						//	//- 1개라면 > 그것을 선택
						//	//- 크기가 같은거 선택
						//	//- 레이어 인덱스의 차이가 가장 작은거 선택
						//	List<apPSDLayerData> srcLayerDataList = _psdLoader.PSDLayerDataList.FindAll(delegate (apPSDLayerData a)
						//	{
						//		return string.Equals(a._name, setLayer._name)
						//		&& a._isImageLayer == setLayer._isImageLayer;
						//	});

						//	if (srcLayerDataList != null && srcLayerDataList.Count > 0)
						//	{
						//		//2-1. 1개인 경우
						//		if (srcLayerDataList.Count == 1)
						//		{
						//			srcLayerData = srcLayerDataList[0];
						//		}

						//		//2-2. 크기가 같은게 1개 있다면 선택
						//		if (srcLayerData == null)
						//		{
						//			List<apPSDLayerData> srcLayerDataList_SameSize = srcLayerDataList.FindAll(delegate (apPSDLayerData a)
						//			{
						//				return a._width == setLayer._width && a._height == setLayer._height;
						//			});

						//			if (srcLayerDataList_SameSize != null && srcLayerDataList_SameSize.Count == 1)
						//			{
						//				srcLayerData = srcLayerDataList_SameSize[0];
						//			}
						//		}

						//		//레이어 인덱스 차이가 가장 작은거 선택
						//		if (srcLayerData == null)
						//		{
						//			int minLayerIndexDiff = 20;//<<최대치
						//			int iMinLayer = -1;
						//			for (int iSubLayer = 0; iSubLayer < srcLayerDataList.Count; iSubLayer++)
						//			{
						//				apPSDLayerData subLayer = srcLayerDataList[iSubLayer];
						//				int indexDiff = Mathf.Abs(subLayer._layerIndex - setLayer._layerIndex);
						//				if (indexDiff < minLayerIndexDiff)
						//				{
						//					minLayerIndexDiff = indexDiff;
						//					iMinLayer = iSubLayer;
						//				}
						//			}
						//			if (iMinLayer >= 0)
						//			{
						//				srcLayerData = srcLayerDataList[iMinLayer];
						//			}
						//		}

						//	}
						//} 
						#endregion

						if (srcLayerData != null)
						{
							//Debug.Log("PSD Set Layer >> Source Layer Data Recovered : " + setLayer._transformID);
							//Transform이 존재한다면연결을 해주자
							if (srcLayerData._isImageLayer)
							{
								//apTransform_Mesh meshTransform = _selectedPSDSet._linkedTargetMeshGroup.GetMeshTransform(setLayer._transformID);//이전
								apTransform_Mesh meshTransform = _selectedPSDSet._linkedTargetMeshGroup.GetMeshTransformRecursive(setLayer._transformID);//버그 수정
								if (meshTransform != null)
								{
									srcLayerData._isRemapSelected = true;
									srcLayerData._isBakable = true;
									srcLayerData._remap_TransformID = setLayer._transformID;

									srcLayerData._remap_MeshTransform = meshTransform;
									srcLayerData._remap_MeshGroupTransform = null;
								}
							}
							else
							{
								apTransform_MeshGroup meshGroupTransform = _selectedPSDSet._linkedTargetMeshGroup.GetMeshGroupTransformRecursive(setLayer._transformID);//버그 수정
								if (meshGroupTransform != null)
								{
									srcLayerData._isRemapSelected = true;
									srcLayerData._isBakable = true;
									srcLayerData._remap_TransformID = setLayer._transformID;

									srcLayerData._remap_MeshTransform = null;
									srcLayerData._remap_MeshGroupTransform = meshGroupTransform;
								}
							}
						}
					}

					//Baked가 안된 레이어 정보를 찾아서 연결한다.
					for (int iSetLayer = 0; iSetLayer < nLayers; iSetLayer++)
					{
						setLayer = _selectedPSDSet._layers[iSetLayer];

						if (setLayer._isBaked)
						{
							continue;
						}

						#region [미사용 코드] FindMatchedLayerData_NotBaked 함수에 해당 코드를 넣었다.
						////1. 이름과 레이어가 같은 PSD LayerData를 찾자 + Remap이 안된 것
						//apPSDLayerData srcLayerData = _psdLoader.PSDLayerDataList.Find(delegate (apPSDLayerData a)
						//{
						//	return a._layerIndex == setLayer._layerIndex
						//			&& string.Equals(a._name, setLayer._name)
						//			&& a._isImageLayer == setLayer._isImageLayer
						//			&& !a._isRemapSelected
						//			&& a._isBakable;

						//});

						////2. 레이어 번호가 같은게 없다면, 이름이라도 같은걸 찾자
						//if (srcLayerData == null)
						//{
						//	srcLayerData = _psdLoader.PSDLayerDataList.Find(delegate (apPSDLayerData a)
						//	{
						//		return string.Equals(a._name, setLayer._name)
						//			&& a._isImageLayer == setLayer._isImageLayer
						//			&& !a._isRemapSelected
						//			&& a._isBakable;

						//	});
						//} 
						#endregion

						//Baked되지 않은 레이어인 경우 적절한 PSD 데이터를 찾자
						apPSDLayerData srcLayerData = _psdLoader.FindMatchedLayerData_NotBaked(setLayer);

						

						if (srcLayerData != null)
						{
							//다른건 없고 그냥 Bakable을 끈다.
							srcLayerData._isBakable = false;
						}
					}
				}
			}
			else if(_selectedPSDSecondary != null)
			{
				//2. Secondary를 선택했을때 연결
				apPSDSet linkdPSDSet = _selectedPSDSecondary._linkedMainSet;

				if (linkdPSDSet != null
					&& linkdPSDSet._isLastBaked
					//&& linkdPSDSet._linkedTargetMeshGroup != null
					)
				{
					//1. 만약 Bake된게 있다면
					//- PSDSetLayer에 따라서 찾자
					//- 이름과 레이어 번호를 기준으로 하자
					//- 검색 순서는 PSDLayerData <- PSDSetLayer 역으로
					
					apPSDSecondarySetLayer curSecondLayer = null;
					apPSDSetLayer linkedMainLayer = null;
					int nSecondLayers = _selectedPSDSecondary._layers != null ? _selectedPSDSecondary._layers.Count : 0;
					
					for (int iLayer = 0; iLayer < nSecondLayers; iLayer++)
					{
						curSecondLayer = _selectedPSDSecondary._layers[iLayer];
						linkedMainLayer = curSecondLayer._linkedMainLayer;

						if(linkedMainLayer == null)
						{
							continue;
						}

						if (!linkedMainLayer._isBaked)
						{
							//일단 Baked된 것부터 찾고 연결하자.
							continue;
						}

						//이 레이어에 해당하는 PSD 레이어를 찾자 (Bake된 레이어 기준)
						apPSDLayerData srcLayerData = _psdLoader.FindMatchedLayerData_Baked(linkedMainLayer);

						if (srcLayerData != null)
						{
							//Debug.Log("PSD Set Layer >> Source Layer Data Recovered : " + setLayer._transformID);
							//Transform은 가볍게만 연결하되 제일 중요한건 Secondary Layer와 연결하는 것

							//Transform이 존재한다면연결을 해주자
							if (srcLayerData._isImageLayer)
							{
								apTransform_Mesh meshTransform = linkdPSDSet._linkedTargetMeshGroup.GetMeshTransformRecursive(linkedMainLayer._transformID);//버그 수정
								if (meshTransform != null)
								{
									srcLayerData._isRemapSelected = true;
									srcLayerData._isBakable = true;
									srcLayerData._remap_TransformID = linkedMainLayer._transformID;

									srcLayerData._remap_MeshTransform = meshTransform;
									srcLayerData._remap_MeshGroupTransform = null;
								}
							}
							else
							{
								apTransform_MeshGroup meshGroupTransform = linkdPSDSet._linkedTargetMeshGroup.GetMeshGroupTransformRecursive(linkedMainLayer._transformID);//버그 수정
								if (meshGroupTransform != null)
								{
									srcLayerData._isRemapSelected = true;
									srcLayerData._isBakable = true;
									srcLayerData._remap_TransformID = linkedMainLayer._transformID;

									srcLayerData._remap_MeshTransform = null;
									srcLayerData._remap_MeshGroupTransform = meshGroupTransform;

									
								}
							}

							//Secondary Layer 연결
							srcLayerData._linkedBakedInfo_Secondary = curSecondLayer;

							if(_selectedPSDSecondary._isLastBaked)
							{
								//이전의 기록이 있다면
								srcLayerData._remapPosOffsetDelta_X = curSecondLayer._lastBakedDeltaPixelPosOffsetX;
								srcLayerData._remapPosOffsetDelta_Y = curSecondLayer._lastBakedDeltaPixelPosOffsetY;

								//Debug.Log("> Remap : " + srcLayerData._name + " (" + srcLayerData._remapPosOffsetDelta_X + ", " + srcLayerData._remapPosOffsetDelta_Y + ")");
							}
						}
					}

					//Baked가 안된 레이어 정보를 찾아서 연결한다.
					for (int iLayer = 0; iLayer < nSecondLayers; iLayer++)
					{
						curSecondLayer = _selectedPSDSecondary._layers[iLayer];
						linkedMainLayer = curSecondLayer._linkedMainLayer;

						if(linkedMainLayer == null)
						{
							continue;
						}

						if (linkedMainLayer._isBaked)
						{
							continue;
						}

						//이 레이어에 해당하는 PSD 레이어를 찾자 (Bake 안된 레이어 기준)
						apPSDLayerData srcLayerData = _psdLoader.FindMatchedLayerData_NotBaked(linkedMainLayer);

						if (srcLayerData != null)
						{
							//다른건 없고 그냥 Bakable을 끈다.
							srcLayerData._isBakable = false;
						}
					}
				}
			}


			
			RefreshTransform2PSDLayer();

			//PSD Set 선택 / PSD Secondary Set 선택 여부에 따라서 코드가 조금 다르다.
			apPSDSet targetPSDSet = null;
			if(_selectedPSDSet != null)
			{
				targetPSDSet = _selectedPSDSet;
			}
			//else if(_selectedPSDSecondary != null)
			//{
			//	targetPSDSet = _selectedPSDSecondary._linkedMainSet;
			//}


			//연결이 되었다면 위치를 보정해주자
			//Load 후 단 한번만
			if (targetPSDSet != null)
			{
				//대상 PSD Set이 존재해야한다.
				float meshGroupScaleRatio = (float)targetPSDSet._next_meshGroupScaleX100 * 0.01f;

				int nSrcPSDLayerData = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;
				for (int i = 0; i < nSrcPSDLayerData; i++)
				{
					psdLayer = _psdLoader.PSDLayerDataList[i];

					if (!psdLayer._isRemapSelected)
					{
						continue;
					}
					if (psdLayer._isRemapPosOffset_Initialized)
					{
						//이미 위치가 초기화 되었으면 패스
					}

					float prevLocalPos_X = 0;
					float prevLocalPos_Y = 0;
					float prevPosOffset_X = 0.0f;
					float prevPosOffset_Y = 0.0f;
					bool isLocalPosCalculatable = false;

					if (targetPSDSet._isLastBaked)
					{
						//1. 이전에 Bake된 적이 있다면
						// Bake되었을 때의 PosOffset을 기본적으로 사용하자

						apPSDSetLayer psdSetLayer = null;
						if (psdLayer._remap_MeshTransform != null)
						{
							psdSetLayer = targetPSDSet.GetLayer(psdLayer._remap_MeshTransform);
						}
						else if (psdLayer._remap_MeshGroupTransform != null)
						{
							psdSetLayer = targetPSDSet.GetLayer(psdLayer._remap_MeshGroupTransform);
						}
						if (psdSetLayer != null)
						{
							//prevPosOffset_X = psdSetLayer._bakedLocalPosOffset_X;
							//prevPosOffset_Y = psdSetLayer._bakedLocalPosOffset_Y;
							prevPosOffset_X = 0;
							prevPosOffset_Y = 0;
							prevLocalPos_X = 0.0f;
							prevLocalPos_Y = 0.0f;
							isLocalPosCalculatable = true;
						}
					}
					else
					{
						//2. 이전에 Bake된 적이 없었다면
						//현재 LocalPos를 기준으로
						//Offset = Cur Local Pos - Prev Local Pos이다.

						if (psdLayer._remap_MeshTransform != null)
						{
							if (psdLayer._hierarchyLevel == 0)
							{
								prevLocalPos_X = (psdLayer._remap_MeshTransform._matrix._pos.x / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.x + targetPSDSet._nextBakeCenterOffsetDelta_X);
								prevLocalPos_Y = (psdLayer._remap_MeshTransform._matrix._pos.y / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.y + targetPSDSet._nextBakeCenterOffsetDelta_Y);
							}
							else
							{
								prevLocalPos_X = psdLayer._remap_MeshTransform._matrix._pos.x / meshGroupScaleRatio;
								prevLocalPos_Y = psdLayer._remap_MeshTransform._matrix._pos.y / meshGroupScaleRatio;
							}
							prevPosOffset_X = psdLayer._posOffsetLocal.x;
							prevPosOffset_Y = psdLayer._posOffsetLocal.y;
							isLocalPosCalculatable = true;
						}
						else if (psdLayer._remap_MeshGroupTransform != null)
						{
							if (psdLayer._hierarchyLevel == 0)
							{
								prevLocalPos_X = (psdLayer._remap_MeshGroupTransform._matrix._pos.x / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.x + targetPSDSet._nextBakeCenterOffsetDelta_X);
								prevLocalPos_Y = (psdLayer._remap_MeshGroupTransform._matrix._pos.y / meshGroupScaleRatio) - (_psdLoader.PSDCenterOffset.y + targetPSDSet._nextBakeCenterOffsetDelta_Y);
							}
							else
							{
								prevLocalPos_X = psdLayer._remap_MeshGroupTransform._matrix._pos.x / meshGroupScaleRatio;
								prevLocalPos_Y = psdLayer._remap_MeshGroupTransform._matrix._pos.y / meshGroupScaleRatio;
							}
							prevPosOffset_X = psdLayer._posOffsetLocal.x;
							prevPosOffset_Y = psdLayer._posOffsetLocal.y;
							isLocalPosCalculatable = true;
						}
						//psdLayer._remapPosOffsetDelta_X = psdLayer._posOffsetLocal.x;
					}

					if (isLocalPosCalculatable)
					{
						psdLayer._remapPosOffsetDelta_X = prevPosOffset_X - prevLocalPos_X;
						psdLayer._remapPosOffsetDelta_Y = prevPosOffset_Y - prevLocalPos_Y;
						psdLayer._isRemapPosOffset_Initialized = true;
					}
					else
					{
						psdLayer._remapPosOffsetDelta_X = 0;
						psdLayer._remapPosOffsetDelta_Y = 0;
						//psdLayer._isRemapPosOffset_Initialized = true;
					}

				}
			}
			
			
			
		}

		private void MakeTransformDataList()
		{
			_targetTransformList.Clear();

			


			if(_selectedPSDSet == null ||
				_selectedPSDSet._linkedTargetMeshGroup == null)
			{
				return;
			}

			List<apMesh> meshes = new List<apMesh>();//<<MeshTransform의 중복 메시를 막기 위해


			MakeTransformDataListRecursive(_selectedPSDSet._linkedTargetMeshGroup._rootRenderUnit, _selectedPSDSet._linkedTargetMeshGroup._rootRenderUnit, meshes);
			
		}
		private void MakeTransformDataListRecursive(apRenderUnit curRenderUnit, apRenderUnit rootUnit, List<apMesh> uniqueMeshes)
		{
			if (curRenderUnit._childRenderUnits != null)
			{
				for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
				{
					//자식을 먼저 넣자
					MakeTransformDataListRecursive(curRenderUnit._childRenderUnits[i], rootUnit, uniqueMeshes);
				}
			}

			if(curRenderUnit != rootUnit)
			{
				if(curRenderUnit._meshTransform != null)
				{
					if(curRenderUnit._meshTransform._mesh != null)
					{
						bool isValidMesh = !uniqueMeshes.Contains(curRenderUnit._meshTransform._mesh);//유니크한 Mesh일때

						_targetTransformList.Add(new TargetTransformData(curRenderUnit._meshTransform, isValidMesh));

						if(isValidMesh)
						{
							uniqueMeshes.Add(curRenderUnit._meshTransform._mesh);
						}
					}
				}
				else if(curRenderUnit._meshGroupTransform != null)
				{
					//Root가 아니라면
					if (_selectedPSDSet._linkedTargetMeshGroup._rootMeshGroupTransform != curRenderUnit._meshGroupTransform)
					{
						_targetTransformList.Add(new TargetTransformData(curRenderUnit._meshGroupTransform));
					}
				}
			}

			
		}


		private void RefreshTransform2PSDLayer()
		{	
			_meshTransform2PSDLayer.Clear();
			_meshGroupTransform2PSDLayer.Clear();
			_secondaryLayer2PSDLayer.Clear();

			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}

			apPSDLayerData psdLayer = null;

			if (_selectedPSDSet != null)
			{
				//Main인 경우 Transform과 연결
				for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
				{
					psdLayer = _psdLoader.PSDLayerDataList[i];
					if (!psdLayer._isRemapSelected)
					{
						continue;
					}
					if (psdLayer._isImageLayer)
					{
						if (psdLayer._remap_MeshTransform != null)
						{
							if (!_meshTransform2PSDLayer.ContainsKey(psdLayer._remap_MeshTransform))
							{
								_meshTransform2PSDLayer.Add(psdLayer._remap_MeshTransform, psdLayer);
							}
							else
							{
								//이미 존재한다면!?
								//다른곳에 이미 연결되었을 것이므로 이 데이터를 날린다.
								psdLayer._isRemapSelected = false;
								psdLayer._remap_TransformID = -1;
								psdLayer._remap_MeshTransform = null;
								psdLayer._remap_MeshGroupTransform = null;
							}
						}
					}
					else
					{
						if (psdLayer._remap_MeshGroupTransform != null)
						{
							if (!_meshGroupTransform2PSDLayer.ContainsKey(psdLayer._remap_MeshGroupTransform))
							{
								_meshGroupTransform2PSDLayer.Add(psdLayer._remap_MeshGroupTransform, psdLayer);
							}
							else
							{
								//이미 존재한다면!?
								//다른곳에 이미 연결되었을 것이므로 이 데이터를 날린다.
								psdLayer._isRemapSelected = false;
								psdLayer._remap_TransformID = -1;
								psdLayer._remap_MeshTransform = null;
								psdLayer._remap_MeshGroupTransform = null;
							}
						}
					}
				}
			}
			else if(_selectedPSDSecondary != null)
			{
				//Secondary인 경우에는 Secondary Layer와 연결
				for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
				{
					psdLayer = _psdLoader.PSDLayerDataList[i];
					if(psdLayer._linkedBakedInfo_Secondary == null)
					{
						continue;
					}

					if(_secondaryLayer2PSDLayer.ContainsKey(psdLayer._linkedBakedInfo_Secondary))
					{
						//이미 존재한다면, 하나의 Secondary에 두개의 PSD가 연결된 셈.
						//이 설정을 날린다.
						psdLayer._linkedBakedInfo_Secondary = null;
						continue;
					}

					_secondaryLayer2PSDLayer.Add(psdLayer._linkedBakedInfo_Secondary, psdLayer);
				}
			}
			
		}



		private void LinkPSDLayerAndTransform(apPSDLayerData psdLayerData, TargetTransformData transformData)
		{
			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}
			if(!_psdLoader.IsFileLoaded || psdLayerData == null || transformData == null)
			{
				return;
			}

			//일단, 이 TransformData (또는 공유하는 Mesh)를 참고하고 있는 것이 있다면 연결 해제
			apPSDLayerData curPSDLayer = null;
			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count :0;

			for (int i = 0; i < nPSDLayers; i++)
			{
				curPSDLayer = _psdLoader.PSDLayerDataList[i];
				if(!curPSDLayer._isRemapSelected)
				{
					continue;
				}
				bool isRelease = false;
				if(transformData._meshTransform != null)
				{
					if(curPSDLayer._remap_MeshTransform != null)
					{
						//1. 같은거라면 해제
						//2. Mesh가 같아도 해제
						if(curPSDLayer._remap_MeshTransform == transformData._meshTransform)
						{
							isRelease = true;
						}
						else if(curPSDLayer._remap_MeshTransform._mesh == transformData._meshTransform._mesh)
						{
							isRelease = true;
						}
					}
				}
				else if(transformData._meshGroupTransform != null)
				{
					if(curPSDLayer._remap_MeshGroupTransform != null)
					{
						//같은거 해제
						if(curPSDLayer._remap_MeshGroupTransform == transformData._meshGroupTransform)
						{
							isRelease = true;
						}
					}
				}
				if (isRelease)
				{
					curPSDLayer._isRemapSelected = false;
					curPSDLayer._remap_MeshTransform = null;
					curPSDLayer._remap_TransformID = -1;
					curPSDLayer._remap_MeshGroupTransform = null;
				}
			}

			//선택된걸 연결하자
			if(psdLayerData._isImageLayer && transformData._meshTransform != null)
			{
				psdLayerData._isRemapSelected = true;
				psdLayerData._remap_MeshTransform = transformData._meshTransform;
				psdLayerData._remap_TransformID = transformData._meshTransform._transformUniqueID;
				psdLayerData._remap_MeshGroupTransform = null;
			}
			else if(!psdLayerData._isImageLayer && transformData._meshGroupTransform != null)
			{
				psdLayerData._isRemapSelected = true;
				psdLayerData._remap_MeshTransform = null;
				psdLayerData._remap_TransformID = transformData._meshGroupTransform._transformUniqueID;
				psdLayerData._remap_MeshGroupTransform = transformData._meshGroupTransform;
			}
			
			RefreshTransform2PSDLayer();
		}



		private void LinkPSDLayerAndSecondaryLayer(apPSDLayerData psdLayerData, apPSDSecondarySetLayer secondaryLayer)
		{
			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}
			if(!_psdLoader.IsFileLoaded || psdLayerData == null || secondaryLayer == null)
			{
				return;
			}

			//일단, 이 TransformData (또는 공유하는 Mesh)를 참고하고 있는 것이 있다면 연결 해제
			apPSDLayerData curPSDLayer = null;
			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count :0;

			for (int i = 0; i < nPSDLayers; i++)
			{
				curPSDLayer = _psdLoader.PSDLayerDataList[i];

				//같은걸 참조하고 있는건 모두 해제
				if(curPSDLayer._linkedBakedInfo_Secondary == secondaryLayer)
				{
					curPSDLayer._isRemapSelected = false;
					curPSDLayer._remap_MeshTransform = null;
					curPSDLayer._remap_TransformID = -1;
					curPSDLayer._remap_MeshGroupTransform = null;
					curPSDLayer._linkedBakedInfo_Secondary = null;
					curPSDLayer._remapPosOffsetDelta_X = 0.0f;
					curPSDLayer._remapPosOffsetDelta_Y = 0.0f;
				}
			}

			//선택된걸 연결하자
			psdLayerData._linkedBakedInfo_Secondary = secondaryLayer;
			psdLayerData._remapPosOffsetDelta_X = 0.0f;
			psdLayerData._remapPosOffsetDelta_Y = 0.0f;

			//만약 이전의 Bake 기록이 있다면
			if(_selectedPSDSecondary._isLastBaked)
			{
				psdLayerData._remapPosOffsetDelta_X = secondaryLayer._lastBakedDeltaPixelPosOffsetX;
				psdLayerData._remapPosOffsetDelta_Y = secondaryLayer._lastBakedDeltaPixelPosOffsetY;
			}
			
			RefreshTransform2PSDLayer();
		}


		private void UnlinkPSDLayer(apPSDLayerData psdLayerData)
		{
			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}
			if(!_psdLoader.IsFileLoaded || psdLayerData == null)
			{
				return;
			}

			psdLayerData._isRemapSelected = false;
			psdLayerData._remap_MeshTransform = null;
			psdLayerData._remap_TransformID = -1;
			psdLayerData._remap_MeshGroupTransform = null;
			psdLayerData._linkedBakedInfo_Secondary = null;
			
			RefreshTransform2PSDLayer();
		}

		private void LinkTool_AutoMapping()
		{
			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}
			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Auto Mapping", "Do you want to automatically link layers to Mesh Group?", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			RefreshTransform2PSDLayer();

			//연결이 안된 것들을 순회
			//연결안된 리스트를 취합하자
			//- Main인 경우엔 Transform과 연결

			apPSDLayerData curLayer = null;
			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;

			if (_selectedPSDSet != null)
			{
				//Main인 경우
				TargetTransformData curTransform = null;
				List<apPSDLayerData> notLinkedPSDLayers = new List<apPSDLayerData>();
				List<TargetTransformData> notLinkedTransforms = new List<TargetTransformData>();

				int nTransforms = _targetTransformList != null ? _targetTransformList.Count : 0;

				for (int i = 0; i < nPSDLayers; i++)
				{
					curLayer = _psdLoader.PSDLayerDataList[i];
					if (curLayer._isBakable)
					{
						if (!curLayer._isRemapSelected
							|| (curLayer._remap_MeshTransform == null && curLayer._remap_MeshGroupTransform == null)
							)
						{
							notLinkedPSDLayers.Add(curLayer);
						}
					}
				}

				for (int i = 0; i < nTransforms; i++)
				{
					curTransform = _targetTransformList[i];
					if (curTransform._meshTransform != null && curTransform._isValidMesh)
					{
						if (!_meshTransform2PSDLayer.ContainsKey(curTransform._meshTransform))
						{
							notLinkedTransforms.Add(curTransform);
						}
					}
					else if (curTransform._meshGroupTransform != null)
					{
						if (!_meshGroupTransform2PSDLayer.ContainsKey(curTransform._meshGroupTransform))
						{
							notLinkedTransforms.Add(curTransform);
						}
					}

				}

				//이제 연결을 하자
				//이름을 기준으로 처리
				//각 단계별로 처리 후 리스트에서 제외하자
				//1. 이름이 같은게 1개 있다면 그것을 선택
				//2. 이름이 같은 레이어가 여러개 있다면 인덱스 차이가 가장 적은것을 선택
				//3. 이름이 같은 레이어가 없는건 이름 문자열 비교해서 차이가 가장 적은 것을 선택
				TargetTransformData resultTransformData = null;
				for (int iPSD = 0; iPSD < notLinkedPSDLayers.Count; iPSD++)
				{
					curLayer = notLinkedPSDLayers[iPSD];

					List<TargetTransformData> sameNameTargets = notLinkedTransforms.FindAll(delegate (TargetTransformData a)
					{
						return string.Equals(curLayer._name, a.Name)
								&& curLayer._isImageLayer == a._isMeshTransform;
					});

					resultTransformData = null;
					if (sameNameTargets.Count == 1)
					{
						//1. 같은게 한개가 있다. > 바로 연결
						resultTransformData = sameNameTargets[0];

					}
					else if (sameNameTargets.Count > 1)
					{
						//2. 같은게 여러개 있다. > 레이어 인덱스가 가장 적게 차이나는 것 선택
						int iMinIndex = -1;
						int minIndexDiff = 100;
						for (int iSub = 0; iSub < sameNameTargets.Count; iSub++)
						{
							int diff = Mathf.Abs(curLayer._layerIndex - notLinkedTransforms.IndexOf(sameNameTargets[iSub]));
							if (diff < minIndexDiff)
							{
								minIndexDiff = diff;
								iMinIndex = iSub;
							}
						}
						if (iMinIndex >= 0)
						{
							resultTransformData = sameNameTargets[iMinIndex];
						}
					}

					if (resultTransformData != null)
					{
						curLayer._isRemapSelected = true;
						curLayer._remap_MeshTransform = resultTransformData._meshTransform;
						curLayer._remap_MeshGroupTransform = resultTransformData._meshGroupTransform;
						if (curLayer._remap_MeshTransform != null)
						{
							curLayer._remap_TransformID = curLayer._remap_MeshTransform._transformUniqueID;
						}
						else
						{
							curLayer._remap_TransformID = curLayer._remap_MeshGroupTransform._transformUniqueID;
						}

						notLinkedTransforms.Remove(resultTransformData);//<<선택한건 리스트에서 제외
					}
				}

				//다음 처리를 위해 연결이 된 것들은 제외하자
				notLinkedPSDLayers.RemoveAll(delegate (apPSDLayerData a)
				{
					return a._isRemapSelected;
				});

				//이름이 같은걸 못찾았으니 "유사한 이름 순으로 찾자"
				//유사도가 가장 높은걸 선택
				for (int iPSD = 0; iPSD < notLinkedPSDLayers.Count; iPSD++)
				{
					curLayer = notLinkedPSDLayers[iPSD];
					int iMaxSim = -1;
					int maxSim = -100;
					resultTransformData = null;
					for (int iTD = 0; iTD < notLinkedTransforms.Count; iTD++)
					{
						curTransform = notLinkedTransforms[iTD];
						if (curLayer._isImageLayer != curTransform._isMeshTransform)
						{
							continue;
						}
						int sim = GetNameSimilarity(curLayer._name, curTransform.Name);
						if (sim > 0)
						{
							if (iMaxSim < 0 || sim > maxSim)
							{
								iMaxSim = iTD;
								sim = maxSim;
							}
						}
					}
					if (iMaxSim >= 0)
					{
						resultTransformData = notLinkedTransforms[iMaxSim];
					}

					if (resultTransformData != null)
					{
						curLayer._isRemapSelected = true;
						curLayer._remap_MeshTransform = resultTransformData._meshTransform;
						curLayer._remap_MeshGroupTransform = resultTransformData._meshGroupTransform;
						if (curLayer._remap_MeshTransform != null)
						{
							curLayer._remap_TransformID = curLayer._remap_MeshTransform._transformUniqueID;
						}
						else
						{
							curLayer._remap_TransformID = curLayer._remap_MeshGroupTransform._transformUniqueID;
						}

						notLinkedTransforms.Remove(resultTransformData);//<<선택한건 리스트에서 제외
					}
				}
			}
			else if (_selectedPSDSecondary != null)
			{
				//Secondary인 경우
				//- 연결 안된 것을 모아서 자동으로 연결한다.
				apPSDSecondarySetLayer curSecondaryLayer = null;
				List<apPSDLayerData> notLinkedPSDLayers = new List<apPSDLayerData>();
				List<apPSDSecondarySetLayer> notLinkedSecondaryLayers = new List<apPSDSecondarySetLayer>();

				int nSecondaryLayers = _selectedPSDSecondary._layers != null ? _selectedPSDSecondary._layers.Count : 0;

				for (int i = 0; i < nPSDLayers; i++)
				{
					curLayer = _psdLoader.PSDLayerDataList[i];
					if (curLayer._isBakable)
					{
						if(curLayer._linkedBakedInfo_Secondary == null)
						{
							//연결되지 않은 PSD 레이어다.
							notLinkedPSDLayers.Add(curLayer);
						}
					}
				}

				if(nSecondaryLayers > 0)
				{
					for (int i = 0; i < nSecondaryLayers; i++)
					{
						curSecondaryLayer = _selectedPSDSecondary._layers[i];
						if(!_secondaryLayer2PSDLayer.ContainsKey(curSecondaryLayer))
						{
							//연결이 되지 않았다.
							notLinkedSecondaryLayers.Add(curSecondaryLayer);
						}
					}
				}

				//연결 안된것을 찾았다.
				//이름을 기준으로 처리한다.
				//1. 이름이 같은게 1개 있다면 그것을 선택
				//2. 이름이 같은 레이어가 여러개 있다면 인덱스 차이가 가장 적은것을 선택
				//3. 이름이 같은 레이어가 없는건 이름 문자열 비교해서 차이가 가장 적은 것을 선택

				if (notLinkedPSDLayers.Count > 0 && notLinkedSecondaryLayers.Count > 0)
				{
					for (int i = 0; i < notLinkedPSDLayers.Count; i++)
					{
						curLayer = _psdLoader.PSDLayerDataList[i];

						apPSDSecondarySetLayer resultSecondaryLayer = null;
						List<apPSDSecondarySetLayer> sameNameTargets = notLinkedSecondaryLayers.FindAll(delegate(apPSDSecondarySetLayer a)
						{
							return string.Equals(curLayer._name, a._mainLayerName);
						});

						if(sameNameTargets.Count == 1)
						{
							//이름 같은게 하나 있다 > 바로 연결
							resultSecondaryLayer = sameNameTargets[0];
						}
						else if(sameNameTargets.Count > 1)
						{
							//이름 같은개 여러개 있다. > 레이어 인덱스 차이가 가장 적게 나는 것을 선택
							int iMinIndex = -1;
							int minIndexDiff = 100;
							for (int iSub = 0; iSub < sameNameTargets.Count; iSub++)
							{
								int diff = Mathf.Abs(curLayer._layerIndex - notLinkedSecondaryLayers.IndexOf(sameNameTargets[iSub]));
								if (diff < minIndexDiff)
								{
									minIndexDiff = diff;
									iMinIndex = iSub;
								}
							}
							if (iMinIndex >= 0)
							{
								resultSecondaryLayer = sameNameTargets[iMinIndex];
							}
						}

						if (resultSecondaryLayer != null)
						{
							curLayer._linkedBakedInfo_Secondary = resultSecondaryLayer;
							if(_selectedPSDSecondary._isLastBaked)
							{
								curLayer._remapPosOffsetDelta_X = resultSecondaryLayer._lastBakedDeltaPixelPosOffsetX;
								curLayer._remapPosOffsetDelta_Y = resultSecondaryLayer._lastBakedDeltaPixelPosOffsetY;
							}

							notLinkedSecondaryLayers.Remove(resultSecondaryLayer);//선택한건 리스트에서 제외
						}
					}
					//다음 처리를 위해 연결이 된 것들은 제외하자
					notLinkedPSDLayers.RemoveAll(delegate(apPSDLayerData a)
					{
						return a._linkedBakedInfo_Secondary != null;
					});


					//이름이 같은걸 못찾았으니 "유사한 이름 순으로 찾자"
					//유사도가 가장 높은걸 선택
					for (int i = 0; i < notLinkedPSDLayers.Count; i++)
					{
						curLayer = _psdLoader.PSDLayerDataList[i];

						int iMaxSim = -1;
						int maxSim = -100;

						apPSDSecondarySetLayer resultSecondaryLayer = null;
						for (int iTD = 0; iTD < notLinkedSecondaryLayers.Count; iTD++)
						{
							apPSDSecondarySetLayer curSecLayer = notLinkedSecondaryLayers[iTD];
							int sim = GetNameSimilarity(curLayer._name, curSecLayer._mainLayerName);
							if (sim > 0)
							{
								if (iMaxSim < 0 || sim > maxSim)
								{
									iMaxSim = iTD;
									sim = maxSim;
								}
							}
						}
						if (iMaxSim >= 0)
						{
							resultSecondaryLayer = notLinkedSecondaryLayers[iMaxSim];
						}

						if (resultSecondaryLayer != null)
						{
							curLayer._linkedBakedInfo_Secondary = resultSecondaryLayer;

							if(_selectedPSDSecondary._isLastBaked)
							{
								curLayer._remapPosOffsetDelta_X = resultSecondaryLayer._lastBakedDeltaPixelPosOffsetX;
								curLayer._remapPosOffsetDelta_Y = resultSecondaryLayer._lastBakedDeltaPixelPosOffsetY;
							}

							notLinkedSecondaryLayers.Remove(resultSecondaryLayer);//선택한건 리스트에서 제외
						}
					}
				}
			}
			

			RefreshTransform2PSDLayer();
		}


		private int GetNameSimilarity(string strA, string strB)
		{
			//1. 가장 길게 동일한 글자를 찾자
			int lengthSame = 0;
			if(strA.Length > strB.Length)
			{
				//처리를 위해서 A가 더 짧아야 한다.
				string strTmp = strA;
				strA = strB;
				strB = strTmp;
			}
			
			char curA;
			char curB;
			for (int iStartA = 0; iStartA < strA.Length; iStartA++)
			{
				for (int iStartB = 0; iStartB < strB.Length; iStartB++)
				{
					curA = strA[iStartA];
					curB = strB[iStartB];

					if(curA == curB)
					{
						//시작지점이 같다면 카운트 시작
						int iCount = 1;
						while(true)
						{
							if(iStartA + iCount >= strA.Length)
							{
								break;
							}
							if(iStartB + iCount >= strB.Length)
							{
								break;
							}

							curA = strA[iStartA + iCount];
							curB = strB[iStartB + iCount];
							if(curA != curB)
							{
								break;
							}
							iCount++;
						}

						if(iCount > lengthSame)
						{
							//동일한 글자 길이가 더 길면 갱신
							lengthSame = iCount;
						}
					}
				}
			}
			if(lengthSame == 1)
			{
				return -1;
			}

			

			return (lengthSame * 100) - Mathf.Abs(string.Compare(strA, strB));

		}


		


		private void LinkTool_EnableAll()
		{
			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}

			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Enable All", "Do you want to enable all layers?", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			apPSDLayerData curLayer = null;
			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;

			for (int i = 0; i < nPSDLayers; i++)
			{
				curLayer = _psdLoader.PSDLayerDataList[i];
				curLayer._isBakable = true;
			}

			RefreshTransform2PSDLayer();
			
		}

		private void LinkTool_DisableAll()
		{
			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}

			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Disable All", "Do you want to disable all layers?\n(All links will be disconnected.)", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			apPSDLayerData curLayer = null;
			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;

			for (int i = 0; i < nPSDLayers; i++)
			{
				curLayer = _psdLoader.PSDLayerDataList[i];
				curLayer._isBakable = false;
				//Bakable가 False라면 연결 모두 해제
				curLayer._isRemapSelected = false;
				curLayer._remap_MeshGroupTransform = null;
				curLayer._remap_MeshTransform = null;
				curLayer._remap_TransformID = -1;
				curLayer._linkedBakedInfo_Secondary = null;
			}

			RefreshTransform2PSDLayer();
		}

		private void LinkTool_Reset()
		{
			if(_selectedPSDSet == null && _selectedPSDSecondary == null)
			{
				return;
			}

			if(!_psdLoader.IsFileLoaded)
			{
				return;
			}

			//Dialog
			bool isResult = EditorUtility.DisplayDialog("Reset", "Do you want to reset all layers?", "Okay", "Cancel");
			if(!isResult)
			{
				return;
			}

			apPSDLayerData curLayer = null;
			int nPSDLayers = _psdLoader.PSDLayerDataList != null ? _psdLoader.PSDLayerDataList.Count : 0;

			for (int i = 0; i < nPSDLayers; i++)
			{
				curLayer = _psdLoader.PSDLayerDataList[i];
				curLayer._isRemapSelected = false;
				curLayer._remap_TransformID = -1;
				curLayer._remap_MeshTransform = null;
				curLayer._remap_MeshGroupTransform = null;
				curLayer._linkedBakedInfo_Secondary = null;
			}

			RefreshTransform2PSDLayer();
		}

		

		private float GetCorrectedFloat(float value)
		{
			return (float)((int)(value * 1000.0f)) * 0.001f;
		}

		//-----------------------------------------------------------------------------------------
		private void RefreshPSDFileInfo()
		{
			if(_psdSet2FileInfo == null)
			{
				_psdSet2FileInfo = new Dictionary<apPSDSet, FileInfo>();
			}
			_psdSet2FileInfo.Clear();

			if(_psdSecondary2FileInfo == null)
			{
				_psdSecondary2FileInfo = new Dictionary<apPSDSecondarySet, FileInfo>();
			}
			_psdSecondary2FileInfo.Clear();

			if(_portrait == null)
			{
				return;
			}

			int nPSDSets = _portrait._bakedPsdSets != null ? _portrait._bakedPsdSets.Count : 0;
			int nPSDSecondaries = _portrait._bakedPsdSecondarySet != null ? _portrait._bakedPsdSecondarySet.Count : 0;

			if(nPSDSets > 0)
			{
				apPSDSet curPSDSet = null;
				for (int i = 0; i < nPSDSets; i++)
				{
					curPSDSet = _portrait._bakedPsdSets[i];

					if(!string.IsNullOrEmpty(curPSDSet._filePath))
					{
						_psdSet2FileInfo.Add(curPSDSet, new FileInfo(curPSDSet._filePath));
					}
				}
			}

			if(nPSDSecondaries > 0)
			{
				apPSDSecondarySet curPSDSecondary = null;
				for (int i = 0; i < nPSDSecondaries; i++)
				{
					curPSDSecondary = _portrait._bakedPsdSecondarySet[i];
					if(!string.IsNullOrEmpty(curPSDSecondary._psdFilePath))
					{
						_psdSecondary2FileInfo.Add(curPSDSecondary, new FileInfo(curPSDSecondary._psdFilePath));
					}
				}
			}
		}

		//----------------------------------------------------------------------------------------------
		//private bool LoadPsdFile(string filePath, apPSDSet psdSet)
		//{

		//	PsdDocument psdDoc = null;
		//	try
		//	{
		//		ClearPsdFile();

		//		psdDoc = PsdDocument.Create(filePath);
		//		if (psdDoc == null)
		//		{
		//			//EditorUtility.DisplayDialog("PSD Load Failed", "No File Loaded [" + filePath + "]", "Okay");
		//			EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
		//											_editor.GetTextFormat(TEXT.PSDBakeError_Body_LoadPath, filePath),
		//											_editor.GetText(TEXT.Close)
		//											);
		//			return false;
		//		}
		//		psdSet._filePath = filePath;
		//		psdSet._fileNameOnly = "";

		//		if (psdSet._filePath.Length > 4)
		//		{
		//			for (int i = psdSet._filePath.Length - 5; i >= 0; i--)
		//			{
		//				string curChar = psdSet._filePath.Substring(i, 1);
		//				if (curChar == "\\" || curChar == "/")
		//				{
		//					break;
		//				}
		//				psdSet._fileNameOnly = curChar + psdSet._fileNameOnly;
		//			}
		//		}
				
		//		psdSet._imageWidth = psdDoc.FileHeaderSection.Width;
		//		psdSet._imageHeight = psdDoc.FileHeaderSection.Height;
		//		psdSet._imageCenterPosOffset = new Vector2((float)psdSet._imageWidth * 0.5f, (float)psdSet._imageHeight * 0.5f);

		//		if (psdSet._imageWidth > PSD_IMAGE_FILE_MAX_SIZE || psdSet._imageHeight > PSD_IMAGE_FILE_MAX_SIZE)
		//		{
		//			//EditorUtility.DisplayDialog("PSD Load Failed", 
		//			//	"Image File is Too Large [ " + _imageWidth + " x " + _imageHeight + " ] (Maximum 5000 x 5000)", 
		//			//	"Okay");

		//			EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
		//											_editor.GetTextFormat(TEXT.PSDBakeError_Body_LoadSize, psdSet._imageWidth, psdSet._imageHeight),
		//											_editor.GetText(TEXT.Close)
		//											);
		//			ClearPsdFile();
		//			return false;
		//		}

				

		//		int curLayerIndex = 0;

		//		RecursiveAddLayer(psdDoc.Childs, 0, null, curLayerIndex);

		//		//클리핑이 가능한가 체크
		//		CheckClippingValidation();

		//		//파일 로드 성공
		//		psdSet._isFileLoaded = true;

		//		psdDoc.Dispose();
		//		psdDoc = null;
		//		System.GC.Collect();

		//		return true;
		//	}
		//	catch (Exception ex)
		//	{
		//		ClearPsdFile();

		//		if (psdDoc != null)
		//		{
		//			psdDoc.Dispose();
		//			System.GC.Collect();
		//		}

		//		Debug.LogError("Load PSD File Exception : " + ex);

		//		//EditorUtility.DisplayDialog("PSD Load Failed", "Error Occured [" + ex.ToString() + "]", "Okay");
		//		EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
		//										_editor.GetTextFormat(TEXT.PSDBakeError_Body_ErrorCode, ex.ToString()),
		//										_editor.GetText(TEXT.Close)
		//										);

		//	}

		//	return false;
		//}

		//private void ClearPsdFile(apPSDSet psdSet)
		//{
		//	psdSet.ReadyToLoad();
		//	//psdSet._isFileLoaded = false;
		//	//_fileFullPath = "";
		//	//_fileNameOnly = "";
		//	//_imageWidth = -1;
		//	//_imageHeight = -1;
		//	//_imageCenterPosOffset = Vector2.zero;

		//	//_layerDataList.Clear();
		//	//_selectedLayerData = null;

		//	//_bakeDataList.Clear();
		//	//_selectedBakeData = null;

		//	////_isBakeResizable = false;//<<크기가 안맞으면 자동으로 리사이즈를 할 것인가 (이건 넓이 비교로 리사이즈를 하자)
		//	//_bakeWidth = BAKE_SIZE.s1024;
		//	//_bakeHeight = BAKE_SIZE.s1024;
		//	//_bakeDstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)
		//	//_bakeMaximumNumAtlas = 2;
		//	//_bakePadding = 4;
		//	//_bakeBlurOption = true;

		//	//_isNeedBakeCheck = true;
		//	////_needBakeResizeX100 = 100;
		//	//_bakeParams.Clear();

		//	//_loadKey_CheckBake = null;
		//	//_loadKey_Bake = null;

		//	//_resultAtlasCount = 0;
		//	//_resultBakeResizeX100 = 0;
		//	//_resultPadding = 0;
		//}

		//private int RecursiveAddLayer(IPsdLayer[] layers, int level, apPSDLayerData parentLayerData, int curLayerIndex)
		//{
		//	for (int i = 0; i < layers.Length; i++)
		//	{
		//		IPsdLayer curLayer = layers[i];
		//		if (curLayer == null)
		//		{
		//			continue;
		//		}

		//		apPSDLayerData newLayerData = new apPSDLayerData(curLayerIndex, curLayer, _imageWidth, _imageHeight);
		//		newLayerData.SetLevel(level);
		//		if (parentLayerData != null)
		//		{
		//			parentLayerData.AddChildLayer(newLayerData);
		//		}

		//		curLayerIndex++;

		//		//재귀 호출을 하자
		//		if (curLayer.Childs != null && curLayer.Childs.Length > 0)
		//		{
		//			curLayerIndex = RecursiveAddLayer(curLayer.Childs, level + 1, newLayerData, curLayerIndex);
		//		}

		//		_layerDataList.Add(newLayerData);
		//	}
		//	return curLayerIndex;
		//}

		//private void MakePosOffsetLocals(List<apPSDLayerData> layerList, int curLevel, apPSDLayerData parentLayer)
		//{
		//	for (int i = 0; i < layerList.Count; i++)
		//	{
		//		apPSDLayerData curLayer = layerList[i];
		//		if (curLayer._hierarchyLevel != curLevel)
		//		{
		//			continue;
		//		}

		//		if (parentLayer != null)
		//		{
		//			curLayer._posOffsetLocal = curLayer._posOffset - parentLayer._posOffset;
		//		}
		//		else
		//		{
		//			curLayer._posOffsetLocal = curLayer._posOffset;
		//		}
		//	}
		//}

		//private void CheckClippingValidation()
		//{
		//	//Debug.Log("CheckClippingValidation");
		//	//클리핑이 가능한가 체크
		//	//어떤 클리핑 옵션이 나올때
		//	//"같은 레벨에서" ㅁ CC[C] 까지는 Okay / ㅁCCC..[C]는 No
		//	for (int i = 0; i < _layerDataList.Count; i++)
		//	{
		//		apPSDLayerData curLayerData = _layerDataList[i];
		//		curLayerData._isClippingValid = true;

		//		if (curLayerData._isImageLayer && curLayerData._isClipping)
		//		{
		//			//앞으로 체크해보자.
		//			int curLevel = curLayerData._hierarchyLevel;

		//			apPSDLayerData prev1_Layer = null;
		//			apPSDLayerData prev2_Layer = null;
		//			apPSDLayerData prev3_Layer = null;

		//			if (i - 1 >= 0)
		//			{ prev1_Layer = _layerDataList[i - 1]; }
		//			if (i - 2 >= 0)
		//			{ prev2_Layer = _layerDataList[i - 2]; }
		//			if (i - 3 >= 0)
		//			{ prev3_Layer = _layerDataList[i - 3]; }

		//			bool isValiePrev1 = (prev1_Layer != null && prev1_Layer._isBakable && prev1_Layer._isImageLayer && !prev1_Layer._isClipping && prev1_Layer._hierarchyLevel == curLevel);
		//			bool isValiePrev2 = (prev2_Layer != null && prev2_Layer._isBakable && prev2_Layer._isImageLayer && !prev2_Layer._isClipping && prev2_Layer._hierarchyLevel == curLevel);
		//			bool isValiePrev3 = (prev3_Layer != null && prev3_Layer._isBakable && prev3_Layer._isImageLayer && !prev3_Layer._isClipping && prev3_Layer._hierarchyLevel == curLevel);
		//			if (isValiePrev1 || isValiePrev2 || isValiePrev3)
		//			{
		//				curLayerData._isClippingValid = true;
		//			}
		//			else
		//			{
		//				//Clipping의 대상이 없다면 문제가 있다.
		//				//Debug.LogError("Find Invalid Clipping [" + curLayerData._name + "]");
		//				curLayerData._isClippingValid = false;
		//			}
		//		}
		//	}
		//}
	}
}