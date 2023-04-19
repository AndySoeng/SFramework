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

	public class apPSDDialog : EditorWindow
	{
		// Menu
		//----------------------------------------------------------
		private static apPSDDialog s_window = null;



		public static object ShowWindow(apEditor editor, FUNC_PSD_LOAD_RESULT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apPSDDialog), false, "PSD Load");
			apPSDDialog curTool = curWindow as apPSDDialog;
			if (curTool != null && curTool != s_window)
			{

				int width = 1000;
				int height = 700;

				object loadKey = new object();

				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, funcResult, loadKey);

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
		private object _loadKey = null;
		private FUNC_PSD_LOAD_RESULT _funcResult = null;
		public delegate void FUNC_PSD_LOAD_RESULT(	bool isSuccess, object loadKey, 
													string fileName, string filePath, 
													List<apPSDLayerData> layerDataList, 
													//float atlasScaleRatio, float meshGroupScaleRatio,
													int atlasScaleRatioX100, int meshGroupScaleRatioX100,
													int totalWidth, int totalHeight, int padding, 
													int bakedTextureWidth, int bakedTextureHeight,
													int bakeMaximumNumAtlas, bool bakeBlurOption,
													string bakeDstFilePath,
													string bakeDstFileRelativePath
													);//<<나중에 처리 결과에 따라서 더 넣어주자



		public enum LOAD_STEP
		{
			Step1_FileLoad,
			Step2_LayerCheck,
			Step3_AtlasSetting,
		}

		private LOAD_STEP _step = LOAD_STEP.Step1_FileLoad;


		//파일 정보

		#region [미사용 코드 : PSD Loader로 이동]
		//private string _fileFullPath = "";
		//private string _fileNameOnly = "";

		//private int _imageWidth = -1;
		//private int _imageHeight = -1;
		//private Vector2 _imageCenterPosOffset = Vector2.zero; 

		//private List<apPSDLayerData> _layerDataList = new List<apPSDLayerData>();
		#endregion
		
		//레이어 리스트
		//이전 : 한개의 레이어 선택
		//private apPSDLayerData _selectedLayerData = null;

		//변경 : 여러개의 레이어를 선택할 수 있다.
		private List<apPSDLayerData> _selectedLayerDataList = null;


		private apPSDLoader _psdLoader = null;

		//Bake 정보
		//Bake 할때
		// 이미지 크기 + 이미지 개수 + Padding을 지정한다.
		// 리사이즈가 안되면 -> 이미지 개수가 부족할때 에러 (크기를 늘리거나 이미지 개수를 늘려야한다.)
		// 리사이즈가 되면 -> 자동으로 비율을 조절한다. 단, 더 늘리진 않음
		//private bool _isBakeResizable = false;//<<크기가 안맞으면 자동으로 리사이즈를 할 것인가 (이건 넓이 비교로 리사이즈를 하자)
		private apPSDLoader.BAKE_SIZE _bakeWidth = apPSDLoader.BAKE_SIZE.s1024;
		private apPSDLoader.BAKE_SIZE _bakeHeight = apPSDLoader.BAKE_SIZE.s1024;
		private string _bakeDstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)

		//private string _bakeDstFileRelativePath = "";
		private int _bakeMaximumNumAtlas = 2;
		private int _bakePadding = 4;
		private bool _isBakeWarning = false;
		private string _bakeWarningMsg = "";

		private bool _bakeBlurOption = true;
		//private int _bakeResizeRatioX100 = 100;
		//<<추가 : Atlas 말고, MeshGroup을 얼마나 Resize할 것인가
		//100이면 PSD의 픽셀 크기를 그대로 따른다. (Atlas의 비율은 무시한다.)
		//기본값은 100. 
		private int _bakeMeshGroupResizeX100 = 100;





		
		public string[] _bakeDescription = new string[] { "256", "512", "1024", "2048", "4096" };

		//Bake 리스트
		private apPSDBakeData _selectedBakeData = null;
		#region [미사용 코드 : PSD Loader로 리팩토링]
		//private List<apPSDBakeData> _bakeDataList = new List<apPSDBakeData>();


		////Bake Param
		////Bake 전에 어떻게 배치할 지 결정하는 파라미터
		////LayerData + PosOffset으로 구성되어 있다.
		////일단 Scale 상관없이 위치만 계산한다.
		//private class LayerBakeParam
		//{
		//	public apPSDLayerData _targetLayer = null;
		//	public int _atlasIndex = 0;
		//	public int _posOffset_X = 0;
		//	public int _posOffset_Y = 0;

		//	public LayerBakeParam(apPSDLayerData targetLayer,
		//							int atlasIndex,
		//							int posOffset_X,
		//							int posOffset_Y
		//							)
		//	{
		//		_targetLayer = targetLayer;
		//		_atlasIndex = atlasIndex;
		//		_posOffset_X = posOffset_X;
		//		_posOffset_Y = posOffset_Y;
		//	}
		//} 
		#endregion

		//Bake 처리 중에 사용되는 변수 => 이건 Bake 전에 결정된다.
		private bool _isNeedBakeCheck = true;

		#region [미사용 코드 : PSD Loader로 리팩토링]
		//private List<LayerBakeParam> _bakeParams = new List<LayerBakeParam>();
		//private int _realBakeSizePerIndex = 0;
		//private int _realBakedAtlasCount = 0;//실제로 Bake된 Atlas
		//private int _realBakeResizeX100 = 100; 
		#endregion
		private object _loadKey_Calculated = null;//<<체크가 끝났을때의 키
		private object _loadKey_Bake = null;//Bake가 끝났을 때의 키


		#region [미사용 코드 : PSD Loader로 리팩토링]
		//private int _resultAtlasCount = 0;
		//private int _resultBakeResizeX100 = 0;
		//private int _resultPadding = 0; 
		#endregion


		// GUI
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

		private Vector2 _scroll_MainCenter = Vector2.zero;

		private apPSDMouse _mouse = new apPSDMouse();
		private apPSDGL _gl = new apPSDGL();
		private bool _isCtrlAltDrag = false;

		private Color _glBackGroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

		private const int PSD_IMAGE_FILE_MAX_SIZE = 5000;

		// OnGUI 이벤트 성격을 체크하기 위한 변수
		private bool _isGUIEvent = false;
		private Dictionary<string, bool> _delayedGUIShowList = new Dictionary<string, bool>();
		private Dictionary<string, bool> _delayedGUIToggledList = new Dictionary<string, bool>();

		#region [미사용 코드 : PSD Loader로 리팩토링]
		//private string _threadProcessName = "";
		//private WorkProcess _workProcess = new WorkProcess();
		//private bool _isImageBaking = false; 
		#endregion


		#region [미사용 코드 : PSD Loader로 리팩토링]
		//private bool IsGUIUsable { get { return (!_workProcess.IsRunning); } }
		//private bool IsProcessRunning { get { return _workProcess.IsRunning; } } 
		#endregion

		//변경
		private bool IsGUIUsable { get { return (!_psdLoader.IsProcessRunning); } }
		private bool IsProcessRunning { get { return _psdLoader.IsProcessRunning; } }

		private bool _isRequestCloseDialog = false;
		private bool _isDialogEnded = false;
		

		//private Dictionary<string, bool> _delayedGUIEvent_Request = new Dictionary<string, bool>();
		//private Dictionary<string, bool> _delayedGUIEvent_Result = new Dictionary<string, bool>();

		#region [미사용 코드 : PSD Loader로 리팩토링]
		///// <summary>
		///// 스레드를 모방한 비동기 프로세스
		///// </summary>
		//public class WorkProcess
		//{
		//	public class WorkProcessUnit
		//	{
		//		public delegate bool FUNC_PROCESS_UNIT(int index);
		//		private FUNC_PROCESS_UNIT _funcUnit = null;
		//		public int _count = -1;

		//		public WorkProcessUnit(int count)
		//		{
		//			_count = count;
		//		}

		//		public void AddProcess(FUNC_PROCESS_UNIT funcProcess)
		//		{
		//			_funcUnit = funcProcess;
		//		}

		//		public bool Run(int index)
		//		{
		//			if (_funcUnit == null)
		//			{
		//				return false;
		//			}
		//			return _funcUnit(index);
		//		}
		//		public void ChangeCount(int count)
		//		{
		//			_count = count;
		//		}
		//	}
		//	private List<WorkProcessUnit> _units = new List<WorkProcessUnit>();
		//	private int _totalProcessCount = 0;
		//	private int _curProcessX100 = 0;

		//	private bool _isRunning = false;
		//	private bool _isSuccess = false;

		//	private int _iCurUnit = -1;
		//	private int _iSubProcess = -1;
		//	private int _iTotalProcess = -1;
		//	private string _strProcessLabel;


		//	public bool IsRunning { get { return _isRunning; } }
		//	public bool IsSuccess { get { return !_isRunning && _isSuccess; } }
		//	public int ProcessX100 { get { return _curProcessX100; } }




		//	public WorkProcess()
		//	{
		//		Clear();
		//	}

		//	public void Clear()
		//	{
		//		_units.Clear();

		//		_totalProcessCount = 0;

		//		_isRunning = false;
		//		_isSuccess = false;

		//		_iCurUnit = -1;
		//		_iSubProcess = -1;
		//		_iTotalProcess = 0;
		//		_curProcessX100 = 0;
		//	}

		//	public void Add(WorkProcessUnit.FUNC_PROCESS_UNIT funcProcess, int count)
		//	{
		//		WorkProcessUnit newUnit = new WorkProcessUnit(count);
		//		newUnit.AddProcess(funcProcess);
		//		_units.Add(newUnit);

		//		_totalProcessCount += count;//전체 카운트를 높인다. (나중에 퍼센트 체크를 위함)
		//	}

		//	public void ChangeCount(int workIndex, int count)
		//	{
		//		if (workIndex < 0 || workIndex >= _units.Count)
		//		{
		//			return;
		//		}
		//		_units[workIndex].ChangeCount(count);

		//		_totalProcessCount = 0;
		//		//전체 카운트 갱신
		//		for (int i = 0; i < _units.Count; i++)
		//		{
		//			_totalProcessCount += _units[i]._count;
		//		}
		//	}

		//	public void StartRun(string strProcessLabel)
		//	{
		//		_curProcessX100 = 0;

		//		_isRunning = true;
		//		_isSuccess = false;

		//		_iCurUnit = 0;
		//		_iSubProcess = 0;
		//		_iTotalProcess = 0;
		//		_strProcessLabel = strProcessLabel;
		//	}

		//	public void Run()
		//	{
		//		if (!_isRunning)
		//		{
		//			return;
		//		}
		//		if (_iCurUnit >= _units.Count)
		//		{
		//			//끝. 성공!
		//			_isRunning = false;
		//			_isSuccess = true;
		//			_curProcessX100 = 100;

		//			//Debug.Log("Process Success : " + _strProcessLabel);
		//			return;
		//		}
		//		WorkProcessUnit curUnit = _units[_iCurUnit];



		//		//실행하고 퍼센트를 높이자
		//		if (!curUnit.Run(_iSubProcess))
		//		{
		//			//실패 했네염..
		//			_isRunning = false;
		//			_isSuccess = false;
		//			_curProcessX100 = 0;
		//			Debug.LogError("AnyPortrait : PSD Process Failed : " + _strProcessLabel + " (Current Step : " + _iCurUnit + " / Sub Procss : " + _iSubProcess + ")");
		//			return;
		//		}

		//		_iTotalProcess++;
		//		_iSubProcess++;

		//		if (_iSubProcess >= curUnit._count)
		//		{
		//			_iSubProcess = 0;
		//			_iCurUnit++;
		//		}

		//		_curProcessX100 = (int)Mathf.Clamp((((float)_iTotalProcess * 100.0f) / (float)_totalProcessCount), 0, 100);
		//	}



		//} 
		#endregion

		// Init
		//----------------------------------------------------------
		private void Init(apEditor editor, FUNC_PSD_LOAD_RESULT funcResult, object loadKey)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_step = LOAD_STEP.Step1_FileLoad;

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
							editor._mat_VertPin.shader
							);

			wantsMouseMove = true;

			_isRequestCloseDialog = false;
			_isDialogEnded = false;

			#region [미사용 코드 : PSD Loader로 리팩토링]
			//_workProcess.Clear(); 
			#endregion

			//변경 6.23 : 별도의 PSD Loader를 둔다.
			_psdLoader = new apPSDLoader(editor);


			

			//_delayedGUIEvent_Request.Clear();
			//_delayedGUIEvent_Result.Clear();
		}

		public static void CloseDialog()
		{
			if (s_window != null)
			{
				//미사용 코드 : PSD Loader로 변경
				//s_window.CloseThread();

				try
				{	
					s_window._isRequestCloseDialog = false;
					s_window._isDialogEnded = true;
					s_window.Close();
					//Debug.Log("Close Dialog Ended");
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);

				}

				s_window = null;
			}
		}



		#region [미사용 코드 : PSD Loader로 변경]
		//public void CloseThread()
		//{
		//	try
		//	{
		//		//if (_thread != null)
		//		//{
		//		//	_thread.Abort();
		//		//}
		//		_workProcess.Clear();
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogError("CloseThread Exception : " + ex);
		//	}

		//	//_thread = null;
		//	//_threadWorkType = THREAD_WORK_TYPE.None;
		//	//_threadProcessX100 = 0;
		//	//_isThreadProcess = false;
		//	_isImageBaking = false;
		//}

		//void OnDestroy()
		//{
		//	//Debug.Log("PSD Dialog Destroy");
		//	CloseThread();
		//} 
		#endregion

		// Update
		//----------------------------------------------------------
		void Update()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			Repaint();


			#region [미사용 코드 : PSD Loader로 변경]
			//if (_workProcess.IsRunning)
			//{
			//	_workProcess.Run();
			//	if (!_workProcess.IsRunning)
			//	{
			//		_isImageBaking = false;
			//	}
			//} 
			#endregion

			//변경
			if (_psdLoader != null)
			{
				_psdLoader.Update();
			}

			if(_isRequestCloseDialog)
			{
				CloseDialog();
			}

		}


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

						_scroll_MainCenter.x -= moveDelta.x * sensative.x;
						_scroll_MainCenter.y -= moveDelta.y * sensative.y;


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


							_scroll_MainCenter.x -= moveDelta.x * sensative.x;
							_scroll_MainCenter.y -= moveDelta.y * sensative.y;

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
							Vector2 scroll = new Vector2(_scroll_MainCenter.x * 0.1f * _gl.WindowSize.x,
														_scroll_MainCenter.y * 0.1f * _gl.WindowSize.y);
							Vector2 guiCenterPos = _gl.WindowSizeHalf - scroll;

							Vector2 deltaMousePos = _mouse.PosLast - guiCenterPos;//>>이후
							Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

							//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자

							//>>변경
							float nextScrollX = ((nextDeltaMousePos.x - _mouse.PosLast.x) + _gl.WindowSizeHalf.x) / (0.1f * _gl.WindowSize.x);
							float nextScrollY = ((nextDeltaMousePos.y - _mouse.PosLast.y) + _gl.WindowSizeHalf.y) / (0.1f * _gl.WindowSize.y);

							nextScrollX = Mathf.Clamp(nextScrollX, -500.0f, 500.0f);
							nextScrollY = Mathf.Clamp(nextScrollY, -500.0f, 500.0f);

							_scroll_MainCenter.x = nextScrollX;
							_scroll_MainCenter.y = nextScrollY;

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


		// GUI
		//----------------------------------------------------------
		void OnGUI()
		{
			try
			{
				if(_isDialogEnded)
				{
					//Debug.Log("이미 죽은 Dialog입니다. [" + Event.current.type + "]");
					return;
				}
				if (_editor == null || _editor._portrait == null || _psdLoader == null)
				{
					CloseDialog();
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

				//UpdateDelayedGUIEvents(Event.current.type);


				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;

				int topHeight = 28;
				int bottomHeight = 46;
				int margin = 4;

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(windowHeight));

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

				//EditorGUILayout.BeginVertical();
				Rect centerRect = new Rect(0, topHeight + margin, windowWidth, centerHeight);

				int centerRectWidth_List = 170;
				int centerRectWidth_Property = 250;
				int centerRectWidth_Main = windowWidth - (margin + centerRectWidth_List + margin + centerRectWidth_Property);

				Rect centerRect_Main = new Rect(0, topHeight + margin, centerRectWidth_Main, centerHeight);
				Rect centerRect_List = new Rect(centerRectWidth_Main + margin, topHeight + margin, centerRectWidth_List, centerHeight);
				Rect centerRect_Property = new Rect(centerRectWidth_Main + margin + centerRectWidth_List + margin, topHeight + margin, centerRectWidth_Property, centerHeight);

				//GUILayout.BeginArea(centerRect);

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));
				//EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));


				MouseUpdate(centerRect_Main);

#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
				bool isCtrl = Event.current.control;
#endif
				bool isAlt = Event.current.alt;

				MouseScrollUpdate(centerRect_Main, isCtrl, isAlt);

				if (_step == LOAD_STEP.Step1_FileLoad)
				{
					GUI.Box(centerRect, "");
				}
				else
				{
					Color guiBasicColor = GUI.backgroundColor;

					GUI.backgroundColor = _glBackGroundColor;
					GUI.Box(centerRect_Main, "");

					GUI.backgroundColor = guiBasicColor;


					GUI.Box(centerRect_List, "");
					GUI.Box(centerRect_Property, "");

					int scrollHeightOffset = 32;
					_scroll_MainCenter.y = GUI.VerticalScrollbar(new Rect(centerRect_Main.width - 15, scrollHeightOffset, 20, centerRect_Main.height - 15), _scroll_MainCenter.y, 5.0f, -100.0f, 100.0f + 5.0f);
					_scroll_MainCenter.x = GUI.HorizontalScrollbar(new Rect(0, (centerRect_Main.height - 15) + scrollHeightOffset, centerRect_Main.width - 15, 20), _scroll_MainCenter.x, 5.0f, -100.0f, 100.0f + 5.0f);

					if (GUI.Button(new Rect(centerRect_Main.width - 15, (centerRect_Main.height - 15) + scrollHeightOffset, 15, 15), ""))
					{
						_scroll_MainCenter = Vector2.zero;
						_iZoomX100 = ZOOM_INDEX_DEFAULT;
					}

					_gl.SetWindowSize(
						(int)centerRect_Main.width, (int)centerRect_Main.height,
						_scroll_MainCenter, (float)(_zoomListX100[_iZoomX100]) * 0.01f,
						(int)centerRect_Main.x, (int)centerRect_Main.y,
						(int)position.width, (int)position.height);


				}

				switch (_step)
				{
					case LOAD_STEP.Step1_FileLoad:
						{
							//1개의 레이아웃
							EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));

							EditorGUILayout.BeginVertical();
							GUILayout.Space(10);

							GUI_Center_FileLoad(windowWidth - 10, centerHeight - 26);

							
							EditorGUILayout.EndVertical();

							EditorGUILayout.EndHorizontal();
						}
						break;

					case LOAD_STEP.Step2_LayerCheck:
						{
							//3개의 레이아웃
							EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Main.width), GUILayout.Height(centerHeight));
							//GUI_Center_LayerCheck_GUI(windowWidth - 10, centerHeight - 26);
							GUI_Center_LayerCheck_GUI((int)(centerRect_Main.width - 20), centerHeight - 26);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_List.width), GUILayout.Height(centerHeight - 6));
							//GUI_Center_LayerCheck_List((int)(centerRect_List.width - 4), centerHeight - 26);
							GUI_Center_LayerCheck_List((int)(centerRect_List.width), centerHeight - 6);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Property.width - 2), GUILayout.Height(centerHeight));
							//GUI_Center_LayerCheck_Property((int)(centerRect_Property.width - 4), centerHeight - 26);
							GUI_Center_LayerCheck_Property((int)(centerRect_Property.width - 2), centerHeight);
							EditorGUILayout.EndVertical();

							EditorGUILayout.EndHorizontal();
						}

						break;

					case LOAD_STEP.Step3_AtlasSetting:
						{
							//3개의 레이아웃
							EditorGUILayout.BeginHorizontal(GUILayout.Width(windowWidth), GUILayout.Height(centerHeight));

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Main.width), GUILayout.Height(centerHeight));
							//GUI_Center_LayerCheck_GUI(windowWidth - 10, centerHeight - 26);
							GUI_Center_AtlasSetting_GUI((int)(centerRect_Main.width - 20), centerHeight - 26);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_List.width), GUILayout.Height(centerHeight - 6));
							GUI_Center_AtlasSetting_List((int)(centerRect_List.width), centerHeight - 6);
							EditorGUILayout.EndVertical();

							GUILayout.Space(margin);

							EditorGUILayout.BeginVertical(GUILayout.Width(centerRect_Property.width - 2), GUILayout.Height(centerHeight));
							GUI_Center_AtlasSetting_Property((int)(centerRect_Property.width - 2), centerHeight);
							EditorGUILayout.EndVertical();

							EditorGUILayout.EndHorizontal();
						}
						break;
				}
				//EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();

				//if(bottomMargin > 0)
				//{
				//	GUILayout.Space(bottomMargin);
				//}

				GUILayout.Space(4);

				//GUILayout.EndArea();
				//EditorGUILayout.EndVertical();

				// Bottom UI : 스텝 이동/확인/취소를 제어할 수 있다.
				//--------------------------------------------
				GUI.Box(new Rect(0, topHeight + margin + centerHeight + margin, windowWidth, bottomHeight), "");
				GUILayout.Space(margin);
				//GUILayout.Space(margin - 2);

				EditorGUILayout.BeginVertical(GUILayout.Width(windowWidth), GUILayout.Height(bottomHeight));
				GUI_Bottom(windowWidth, bottomHeight - 12);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndVertical();


				//렌더링 중인 Pass를 종료
				_gl.EndPass();
				
				//if (Event.current.type == EventType.Repaint
				//	&& _isRequestCloseDialog)
				//{
				//	Debug.Log("apPSDDialog Close Dialog [" + Event.current.type + "]");
				//	CloseDialog();
				//	Debug.Log(">> Close Ended");
				//}
			}
			catch (Exception ex)
			{
				Debug.LogError("PSD Dialog Exception : " + ex);
				
				//Mac 코멘트 추가 : 에러가 발생해도 다이얼로그는 닫히지 않고, 코멘트 로그만 나오게 만들자
#if !UNITY_EDITOR_WIN
				Debug.Log("Comment: UI errors can often occur on Mac OS, but most can be ignored. Please ignore these logs if there is no problem with your work.");
#endif
				//삭제 21.3.17 : Mac의 문제로 에러가 줄기차게 발생할 수 있다. 그냥 에러가 나도 Close를 하지 말자
				//CloseDialog();
			}
			//if (_isDialogEnded)
			//{
			//	Debug.Log("Last OnGUI");
			//}
		}

		//void OnDestroy()
		//{
		//	Debug.Log("On Destroy");
		//}



		// Top
		private void GUI_Top(int width, int height)
		{
			int stepWidth = (width / 5) - 10;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			//GUILayout.Space(20);
			int totalContentWidth = (stepWidth + 2) + 50 + (stepWidth + 2) + 50 + (stepWidth + 2);
			GUILayout.Space((width / 2) - (totalContentWidth / 2));

			Color prevColor = GUI.backgroundColor;

			GUIStyle guiStyle_Center = GUI.skin.box;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			guiStyle_Center.normal.textColor = apEditorUtil.BoxTextColor;

			GUIStyle guiStyle_Next = GUI.skin.label;
			guiStyle_Next.alignment = TextAnchor.MiddleCenter;

			Color selectedColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
			Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			//Step 1
			if (_step == LOAD_STEP.Step1_FileLoad)	{ GUI.backgroundColor = selectedColor; }
			else									{ GUI.backgroundColor = unselectedColor; }
			//"Load"
			GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Load), guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));

			GUILayout.Space(10);
			GUILayout.Box(">>", guiStyle_Next, GUILayout.Width(30), GUILayout.Height(height));
			GUILayout.Space(10);

			//Step 2
			if (_step == LOAD_STEP.Step2_LayerCheck)	{ GUI.backgroundColor = selectedColor; }
			else										{ GUI.backgroundColor = unselectedColor; }

			//"Layers"
			GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Layers), guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));

			GUILayout.Space(10);
			GUILayout.Box(">>", guiStyle_Next, GUILayout.Width(30), GUILayout.Height(height));
			GUILayout.Space(10);

			//Step 3
			if (_step == LOAD_STEP.Step3_AtlasSetting)	{ GUI.backgroundColor = selectedColor; }
			else										{ GUI.backgroundColor = unselectedColor; }
			
			//"Atlas"
			GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Atlas), guiStyle_Center, GUILayout.Width(stepWidth), GUILayout.Height(height));


			EditorGUILayout.EndHorizontal();

			GUI.backgroundColor = prevColor;
		}

		private Vector2 _guiScroll_FileLoad = Vector2.zero;

		// Center - File Load
		private void GUI_Center_FileLoad(int width, int height)
		{
			_guiScroll_FileLoad = EditorGUILayout.BeginScrollView(_guiScroll_FileLoad, false, true, GUILayout.Width(width), GUILayout.Height(height));

			width -= 20;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			//if (!_isFileLoaded)//이전 코드
			//추가 8.31 : _psdLoader가 Null일 때가 있다.
			if(_psdLoader == null)
			{
				_psdLoader = new apPSDLoader(_editor);
				//원래는 이 코드가 실행되어서는 안된다.
			}
			if (!_psdLoader.IsFileLoaded)
			{
				int btnWidth = 200;
				int btnHeight = 40;

				GUILayout.Space((height / 2) - ((btnHeight * 2 + 4) / 2));
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(btnHeight));

				GUILayout.Space((width / 2) - (btnWidth / 2));

				//"Load PSD File"
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_LoadPSDFile), GUILayout.Width(btnWidth), GUILayout.Height(btnHeight)))
				{
					if (IsGUIUsable)
					{
						try
						{
							string filePath = EditorUtility.OpenFilePanel("Open PSD File", apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.PSD_ExternalFile), "psd");
							if (!string.IsNullOrEmpty(filePath))
							{
								//추가 21.7.3 : 이스케이프 문자 삭제
								filePath = apUtil.ConvertEscapeToPlainText(filePath);

								//이전 코드
								//LoadPsdFile(filePath);
								apEditorUtil.SetLastExternalOpenSaveFilePath(filePath, apEditorUtil.SAVED_LAST_FILE_PATH.PSD_ExternalFile);//21.3.1

								

								_psdLoader.Step1_LoadPSDFile(filePath, null);
							}
						}
						catch (Exception ex)
						{
							Debug.LogError("GUI_Center_FileLoad Exception : " + ex);
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(btnHeight));
				GUILayout.Space((width / 2) - (btnWidth / 2));
				//"Reload PSD File"
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_ReloadPSDFile), GUILayout.Width(btnWidth), GUILayout.Height(btnHeight)))
				{
					if (IsGUIUsable)
					{
						try
						{
							//추가 : Reimport / Reload PSD Dialog 호출
							_editor.Controller.ShowPSDReimportDialog();
							_isRequestCloseDialog = true;
						}
						catch (Exception ex)
						{
							Debug.LogError("GUI_Center_FileLoad Exception : " + ex);
						}
					}
				}

				EditorGUILayout.EndHorizontal();
			}
			else
			{


				width -= 20;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				GUILayout.Space(20);

				width -= 40;
				EditorGUILayout.BeginVertical(GUILayout.Width(width));

				//기본 정보를 보여준다.
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_PSDFileInformation), GUILayout.Width(width));//"PSD File Information"
				GUILayout.Space(10);

				EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_PSDFilePath) + " : " + _psdLoader.FileFullPath, GUILayout.Width(width));//"PSD File Path"
				GUILayout.Space(10);
				EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_ImageSize) + " : " + _psdLoader.PSDImageWidth + " x " + _psdLoader.PSDImageHeight, GUILayout.Width(width));//"Image Size"
				EditorGUILayout.LabelField("  " + _editor.GetText(TEXT.DLG_PSD_ImageLayers) + " : " + _psdLoader.PSDLayerDataList.Count, GUILayout.Width(width));//"Image Layers"

				GUILayout.Space(20);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Layers), GUILayout.Width(width));//"Layers"

				GUILayout.Space(10);
				//거꾸로 출력한다. (0번 백그라운드가 맨 밑으로)
				//if (_layerDataList.Count > 0)
				string strClipping = _editor.GetText(TEXT.DLG_PSD_Clipping);

				if (_psdLoader.PSDLayerDataList.Count > 0)
				{
					//for (int i = _layerDataList.Count - 1; i >= 0; i--)
					for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
					{
						//apPSDLayerData curLayer = _layerDataList[i];
						apPSDLayerData curLayer = _psdLoader.PSDLayerDataList[i];
						string strLayerInfo = "  [" + curLayer._layerIndex + "] : " + curLayer._name + "  ( " + curLayer._width + " x " + curLayer._height;
						if (curLayer._isClipping)
						{
							strLayerInfo += " / " + strClipping;//"Clipping"
						}
						strLayerInfo += " )";
						EditorGUILayout.LabelField(strLayerInfo, GUILayout.Width(width));
					}
				}

				GUILayout.Space(30);
				GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(4));
				GUILayout.Space(30);
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Reset), GUILayout.Width(width), GUILayout.Height(20)))//"Reset"
				{
					if (IsGUIUsable)
					{
						//bool result = EditorUtility.DisplayDialog("Reset", "Reset PSD Import Process? (Data is not Saved)", "Reset", "Cancel");
						bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ResetPSDImport_Title),
																	_editor.GetText(TEXT.ResetPSDImport_Body),
																	_editor.GetText(TEXT.ResetPSDImport_Okay),
																	_editor.GetText(TEXT.Cancel)
																	);
						if (result)
						{
							//ClearPsdFile();//이전 코드
							_psdLoader.Clear();
						}
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(height);


			}


			GUILayout.Space(height + 20);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

		}



		// Center - Layer Check
		private void GUI_Center_LayerCheck_GUI(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_gl.DrawGrid();
			//if (_layerDataList.Count > 0)
			if (_psdLoader.PSDLayerDataList.Count > 0)
			{
				apPSDLayerData curImageLayer = null;
				//TODO : 선택한 이미지는 Outline이 나오도록 (true) 하자
				//for (int i = 0; i < _layerDataList.Count; i++)
				for (int i = 0; i < _psdLoader.PSDLayerDataList.Count; i++)
				{
					//curImageLayer = _layerDataList[i];
					curImageLayer = _psdLoader.PSDLayerDataList[i];
					if (curImageLayer._image == null)
					{
						continue;
					}

					//이전
					//bool isOutline = (curImageLayer == _selectedLayerData);

					//변경 21.7.31 : 다중 선택 지원
					bool isOutline = false;
					if(_selectedLayerDataList != null && _selectedLayerDataList.Contains(curImageLayer))
					{
						isOutline = true;
					}


					_gl.DrawTexture(curImageLayer._image,
										//curImageLayer._posOffset - _imageCenterPosOffset,
										curImageLayer._posOffset - _psdLoader.PSDCenterOffset,
										curImageLayer._width, curImageLayer._height,
										curImageLayer._transparentColor2X,
										isOutline);
				}
			}
			EditorGUILayout.EndVertical();
		}

		private Vector2 _scroll_LayerCheckList = Vector2.zero;

		private void GUI_Center_LayerCheck_List(int width, int height)
		{
			//Texture2D icon_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);
			Texture2D icon_Clipping = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			Texture2D icon_Folder = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Folder);

			int itemHeight = 30;
			int levelMargin = 15;
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
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

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Deselect), GUILayout.Width(width - 7), GUILayout.Height(18)))//"Deselect"
			{
				if (IsGUIUsable)
				{
					//이전
					//_selectedLayerData = null;

					if(_selectedLayerDataList != null)
					{
						_selectedLayerDataList.Clear();
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			_scroll_LayerCheckList = EditorGUILayout.BeginScrollView(_scroll_LayerCheckList, false, true, GUILayout.Width(width), GUILayout.Height(height - 33));
			width -= 24;
			//EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(1);
			int iList = 0;

			//if (_layerDataList.Count > 0)
			if(_psdLoader.PSDLayerDataList.Count > 0)
			{

				apPSDLayerData curLayer = null;
				//for (int i = _layerDataList.Count - 1; i >= 0; i--)
				for (int i = _psdLoader.PSDLayerDataList.Count - 1; i >= 0; i--)
				{
					//curLayer = _layerDataList[i];
					curLayer = _psdLoader.PSDLayerDataList[i];


					//if (_selectedLayerData == curLayer)//이전

					//변경 21.7.3 : 다중 선택 지원
					bool isSelected = false;
					if(_selectedLayerDataList != null && _selectedLayerDataList.Contains(curLayer))
					{
						isSelected = true;
					}

					if(isSelected)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						int yOffset = 0;
						int xPos = (int)(_scroll_LayerCheckList.x + 0.5f);

						//Color prevColor = GUI.backgroundColor;

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
							//GUI.Box(new Rect(lastRect.x + 1 + xPos, lastRect.y + 1, width + 10, itemHeight + 1), "");
							yOffset = 1;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x + 1 + xPos, lastRect.y + 30, width + 10, itemHeight + 1), "");
							yOffset = 30;
						}

						//GUI.backgroundColor = prevColor;

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1 + xPos + 1, lastRect.y + yOffset, width + 10 - 2, itemHeight + 1, apEditorUtil.UNIT_BG_STYLE.Main);
					}

					int level = curLayer._hierarchyLevel;
					EditorGUILayout.BeginHorizontal(GUILayout.Width(width + (levelMargin * level)), GUILayout.Height(itemHeight));

					GUILayout.Space(5 + (levelMargin * level));


					bool isClipped = false;
					if (curLayer._isImageLayer)
					{
						isClipped = curLayer._isClipping;
						if (isClipped)
						{
							EditorGUILayout.LabelField(new GUIContent(icon_Clipping), guiStyle_Icon, GUILayout.Width(itemHeight / 2), GUILayout.Height(itemHeight - 5));
						}

						EditorGUILayout.LabelField(new GUIContent(curLayer._image), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));

					}
					else
					{
						EditorGUILayout.LabelField(new GUIContent(icon_Folder), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));
					}

					GUIStyle curGUIStyle = guiStyle_Btn;
					if (!curLayer._isBakable)
					{
						curGUIStyle = guiStyle_Btn_NotBake;
					}
					//else if (curLayer == _selectedLayerData)
					else if(isSelected)
					{
						curGUIStyle = guiStyle_Btn_Selected;
					}

					//if(GUILayout.Button(" [" + curLayer._layerIndex + "] " + curLayer._name, guiStyle_Btn, GUILayout.Width(width), GUILayout.Height(20)))
					int btnWidth = width - (5 + itemHeight);
					if (isClipped)
					{
						btnWidth -= (itemHeight / 2) + 2;
					}
					if (GUILayout.Button("  " + curLayer._name, curGUIStyle, GUILayout.Width(btnWidth), GUILayout.Height(itemHeight)))
					{
#if UNITY_EDITOR_OSX
						bool isCtrl = Event.current.command;
#else
						bool isCtrl = Event.current.control;
#endif
						bool isShift = Event.current.shift;

						if (IsGUIUsable)
						{
							//이전
							//_selectedLayerData = curLayer;

							if(_selectedLayerDataList == null)
							{
								_selectedLayerDataList = new List<apPSDLayerData>();
							}

							//변경 21.7.3 : 다중 선택
							if(isCtrl || isShift)
							{
								//선택된 상태라면 제외
								if(_selectedLayerDataList.Contains(curLayer))
								{
									//이미 선택된 상태다. > 제외
									_selectedLayerDataList.Remove(curLayer);
								}
								else
								{
									//선택되지 않았다. > 추가
									_selectedLayerDataList.Add(curLayer);
								}
							}
							else
							{
								//그냥 선택시 Clear후 선택
								_selectedLayerDataList.Clear();
								_selectedLayerDataList.Add(curLayer);
							}
						}
					}

					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}

			//EditorGUILayout.EndVertical();

			GUILayout.Space(height + 20);

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}


		private Vector2 _scroll_LayerCheckProperty = Vector2.zero;

		private void GUI_Center_LayerCheck_Property(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_scroll_LayerCheckProperty = EditorGUILayout.BeginScrollView(_scroll_LayerCheckProperty, false, true, GUILayout.Width(width), GUILayout.Height(height));
			width -= 24;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(10);

			int nSelectedLayerData = _selectedLayerDataList != null ? _selectedLayerDataList.Count : 0;//추가 21.7.3

			//if (_selectedLayerData != null)//이전
			if(nSelectedLayerData == 1)
			{
				//[ 1개 선택된 경우 ]
				apPSDLayerData selectedLayer = _selectedLayerDataList[0];

				//현재 레이어 정보를 표시한다.
				//적용 여부도 설정
				bool prev_isClipping = selectedLayer._isClipping;
				bool prev_isBakable = selectedLayer._isBakable;

				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Layer) + " " + selectedLayer._layerIndex, GUILayout.Width(width));//"Layer"

				GUILayout.Space(5);
				EditorGUILayout.LabelField(selectedLayer._name, GUILayout.Width(width));
				//"Rect", "Position", "Size"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Rect) + " (LBRT : " + selectedLayer._posOffset_Left + ", " + selectedLayer._posOffset_Top + ", " + selectedLayer._posOffset_Right + ", " + selectedLayer._posOffset_Bottom + " )", GUILayout.Width(width));
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Position) + " : " + selectedLayer._posOffset.x + ", " + selectedLayer._posOffset.y, GUILayout.Width(width));
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Size) + " : " + selectedLayer._width + " x " + selectedLayer._height, GUILayout.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Level) + " : " + selectedLayer._hierarchyLevel, GUILayout.Width(width));//"Level"
				if (selectedLayer._parentLayer != null)
				{
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Parent) + " : " + selectedLayer._parentLayer._name, GUILayout.Width(width));//"Parent"
				}



				GUILayout.Space(5);
				//이전
				//bool next_isClipping = EditorGUILayout.Toggle(_editor.GetText(TEXT.DLG_PSD_Clipping), selectedLayer._isClipping, GUILayout.Width(width));//"Clipping"
				
				//변경 21.7.3
				bool next_isClipping = selectedLayer._isClipping;
				if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_PSD_Clipping), next_isClipping, true, width, 25))
				{
					next_isClipping = !next_isClipping;
				}


				GUILayout.Space(5);
				//이전
				//bool next_isBakable = EditorGUILayout.Toggle(_editor.GetText(TEXT.DLG_PSD_BakeTarget), selectedLayer._isBakable, GUILayout.Width(width));//"Bake Target"

				//변경
				bool next_isBakable = selectedLayer._isBakable;
				if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_PSD_BakeTarget), next_isBakable, true, width, 30))
				{
					next_isBakable = !next_isBakable;
				}

				if (IsGUIUsable)
				{
					//스레드가 작동 안될때에만 적용하자
					selectedLayer._isClipping = next_isClipping;
					selectedLayer._isBakable = next_isBakable;
				}

				//클리핑 제한 없어짐
				//if (selectedLayer._isClipping && !selectedLayer._isClippingValid)
				//{
				//	GUILayout.Space(10);

				//	Color prevColor = GUI.backgroundColor;
				//	GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				//	GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				//	guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				//	guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;

				//	GUILayout.Box("Warning\nClipped Layers are Over 3", guiStyle_WarningBox, GUILayout.Width(width), GUILayout.Height(70));

				//	GUI.backgroundColor = prevColor;
				//}

				if (prev_isClipping != selectedLayer._isClipping
					|| prev_isBakable != selectedLayer._isBakable)
				{
					_isNeedBakeCheck = true;//<<다시 Bake할 수 있도록 하자

					//이전 코드
					//CheckClippingValidation();
					_psdLoader.CheckClippingValidation();//PSD Loader 이용
				}
			}
			else if(nSelectedLayerData > 1)
			{
				//[ 2개 이상 선택된 경우 ] (추가 21.7.3)
				bool isClipping_All = false;//모두 클리핑 되는가
				bool isBakable_All = false;//모두 Bake 대상인가
				bool isClipping_Sync = true;//클리핑 속성이 모두 동기화 되었는가
				bool isBakable_Sync = true;//Bakable 속성이 모두 동기화 되었는가

				apPSDLayerData curSelectedLayer = _selectedLayerDataList[0];

				//일단 첫번째 값을 설정
				isClipping_All = curSelectedLayer._isClipping;
				isBakable_All = curSelectedLayer._isBakable;

				isClipping_Sync = true;
				isBakable_Sync = true;

				for (int i = 1; i < nSelectedLayerData; i++)
				{
					curSelectedLayer = _selectedLayerDataList[i];
					if(isClipping_All != curSelectedLayer._isClipping)
					{
						//값이 다르다면
						isClipping_Sync = false;
					}

					if(isBakable_All != curSelectedLayer._isBakable)
					{
						//값이 다르다면
						isBakable_Sync = false;
					}

				}

				//현재 레이어 정보를 표시한다.
				//적용 여부도 설정

				//변경 21.7.3
				if(apEditorUtil.ToggledButton_2Side_Sync(_editor.GetText(TEXT.DLG_PSD_Clipping), _editor.GetText(TEXT.DLG_PSD_Clipping), isClipping_All, true, isClipping_Sync, width, 25))
				{
					if (IsGUIUsable)
					{
						bool isNextClipping = false;
						if (isClipping_Sync)
						{
							//동기화 되었다면 > 값의 반대
							isNextClipping = !isClipping_All;
						}
						else
						{
							//동기화 되지 않았다면 > 모두 Enable
							isNextClipping = true;
						}

						for (int i = 0; i < nSelectedLayerData; i++)
						{
							curSelectedLayer = _selectedLayerDataList[i];
							curSelectedLayer._isClipping = isNextClipping;
						}

						_isNeedBakeCheck = true;//<<다시 Bake할 수 있도록 하자
						_psdLoader.CheckClippingValidation();//PSD Loader 이용
					}
				}


				GUILayout.Space(5);
				
				//변경
				if(apEditorUtil.ToggledButton_2Side_Sync(_editor.GetText(TEXT.DLG_PSD_BakeTarget), _editor.GetText(TEXT.DLG_PSD_BakeTarget), isBakable_All, true, isBakable_Sync, width, 30))
				{
					if (IsGUIUsable)
					{
						bool isNextBakable = false;
						if (isBakable_Sync)
						{
							//동기화 되었다면 > 값의 반대
							isNextBakable = !isBakable_All;
						}
						else
						{
							//동기화 되지 않았다면 > 모두 Enable
							isNextBakable = true;
						}

						for (int i = 0; i < nSelectedLayerData; i++)
						{
							curSelectedLayer = _selectedLayerDataList[i];
							curSelectedLayer._isBakable = isNextBakable;
						}

						_isNeedBakeCheck = true;//<<다시 Bake할 수 있도록 하자
						_psdLoader.CheckClippingValidation();//PSD Loader 이용
					}
				}				
			}
			else
			{
				//이미지 전체 정보를 표시한다.
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_ImageName), GUILayout.Width(width));//"Image Name"
				//EditorGUILayout.LabelField(_fileNameOnly, GUILayout.Width(width));
				EditorGUILayout.LabelField(_psdLoader.FileName, GUILayout.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_FilePath), GUILayout.Width(width));//"File Path"
				//EditorGUILayout.TextField(_fileFullPath, GUILayout.Width(width));
				EditorGUILayout.TextField(_psdLoader.FileFullPath, GUILayout.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Size), GUILayout.Width(width));//"Size"
				//EditorGUILayout.LabelField(_imageWidth + " x " + _imageHeight, GUILayout.Width(width));
				EditorGUILayout.LabelField(_psdLoader.PSDImageWidth + " x " + _psdLoader.PSDImageHeight, GUILayout.Width(width));
			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(height + 20);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}



		// Center - Atlas Setting
		private void GUI_Center_AtlasSetting_GUI(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_gl.DrawGrid();

			//if (_bakeDataList.Count > 0 && !_isImageBaking)//이전 코드
			if (_psdLoader.BakeDataList.Count > 0 && !_psdLoader.IsImageBaking)
			{
				if (_selectedBakeData == null)
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
					_gl.DrawTexture(_selectedBakeData._bakedImage,
											new Vector2(_selectedBakeData._width / 2, _selectedBakeData._height / 2),
											_selectedBakeData._width, _selectedBakeData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f),
											true);
				}
			}
			EditorGUILayout.EndVertical();
		}


		private Vector2 _scroll_AtlasSettingList = Vector2.zero;
		private void GUI_Center_AtlasSetting_List(int width, int height)
		{
			Texture2D icon_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);

			int itemHeight = 30;
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));

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



			GUIStyle guiStyle_Icon = new GUIStyle(GUI.skin.label);
			guiStyle_Icon.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Deselect), GUILayout.Width(width - 7), GUILayout.Height(18)))//"Deselect"
			{
				if (IsGUIUsable)
				{
					_selectedBakeData = null;
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			_scroll_AtlasSettingList = EditorGUILayout.BeginScrollView(_scroll_AtlasSettingList, false, true, GUILayout.Width(width), GUILayout.Height(height - 33));
			width -= 24;
			//EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(1);
			int iList = 0;
			//if (_bakeDataList.Count > 0)//이전 코드
			if (_psdLoader.BakeDataList.Count > 0)
			{
				apPSDBakeData curBakeData = null;
				//for (int i = 0; i < _bakeDataList.Count; i++)//이전 코드
				for (int i = 0; i < _psdLoader.BakeDataList.Count; i++)
				{
					GUIStyle curGUIStyle = guiStyle_Btn;

					//curBakeData = _bakeDataList[i];//이전 코드
					curBakeData = _psdLoader.BakeDataList[i];

					if (_selectedBakeData == curBakeData)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();

						int yOffset = 0;

						//Color prevColor = GUI.backgroundColor;

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
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 1, width + 10, itemHeight), "");
							yOffset = 1;
						}
						else
						{
							//GUI.Box(new Rect(lastRect.x + 1, lastRect.y + 30, width + 10, itemHeight), "");
							yOffset = 30;
						}

						//GUI.backgroundColor = prevColor;


						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1 + 1, lastRect.y + yOffset, width + 10 - 2, itemHeight, apEditorUtil.UNIT_BG_STYLE.Main);

						curGUIStyle = guiStyle_Btn_Selected;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(itemHeight));

					GUILayout.Space(5);
					EditorGUILayout.LabelField(new GUIContent(icon_Image), guiStyle_Icon, GUILayout.Width(itemHeight - 5), GUILayout.Height(itemHeight - 5));


					if (GUILayout.Button("  " + curBakeData.Name, curGUIStyle, GUILayout.Width(width - (5 + itemHeight)), GUILayout.Height(itemHeight)))
					{
						if (IsGUIUsable)
						{
							_selectedBakeData = curBakeData;
						}
					}

					EditorGUILayout.EndHorizontal();

					iList++;
				}
			}

			GUILayout.Space(height + 20);

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}


		private Vector2 _scroll_AtlasSettingProperty = Vector2.zero;
		private void GUI_Center_AtlasSetting_Property(int width, int height)
		{
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height));
			_scroll_AtlasSettingProperty = EditorGUILayout.BeginScrollView(_scroll_AtlasSettingProperty, false, true, GUILayout.Width(width), GUILayout.Height(height));
			width -= 24;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(10);
			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AssetName), GUILayout.Width(width));//"Asset Name"
			
			EditorGUI.BeginChangeCheck();
			string next_fileNameOnly = EditorGUILayout.DelayedTextField(_psdLoader.FileName, GUILayout.Width(width));

			if (EditorGUI.EndChangeCheck())
			{
				if (IsGUIUsable)
				{
					//_fileNameOnly = next_fileNameOnly;
					_psdLoader.SetFileName(next_fileNameOnly);
				}
			}
			

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_SavePath), GUILayout.Width(width));//"Save path"
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			//string prev_bakeDstFilePath = _bakeDstFilePath;

			Color prevColor = GUI.backgroundColor;
			if(string.IsNullOrEmpty(_bakeDstFilePath))
			{
				GUI.backgroundColor = new Color(GUI.backgroundColor.r * 1.5f, 
					GUI.backgroundColor.g * 0.8f, 
					GUI.backgroundColor.b * 0.8f, 
					GUI.backgroundColor.a);
			}
			EditorGUI.BeginChangeCheck();
			string next_bakeDstFilePath = EditorGUILayout.DelayedTextField(_bakeDstFilePath, GUILayout.Width(width - 64));
			GUI.backgroundColor = prevColor;

			if (EditorGUI.EndChangeCheck())
			{
				if (IsGUIUsable)
				{
					//이전
					//_bakeDstFilePath = next_bakeDstFilePath;

					//int subStartLength = Application.dataPath.Length;
					//_bakeDstFileRelativePath = "Assets";
					//if (_bakeDstFilePath.Length > subStartLength)
					//{
					//	_bakeDstFileRelativePath += _bakeDstFilePath.Substring(subStartLength);
					//}

					//변경 22.7.1
					string resultPathFull = "";
					string resultPathRelative = "";
					bool isValidPath = apEditorUtil.MakeRelativeDirectoryPathFromAssets(next_bakeDstFilePath, ref resultPathFull, ref resultPathRelative);
					if (isValidPath)
					{
						_bakeDstFilePath = resultPathFull;
						//_bakeDstFileRelativePath = resultPathRelative;
					}
					else
					{
						//유효하지 않다면, 상대 경로는 걍 Assets
						_bakeDstFilePath = next_bakeDstFilePath;
						//_bakeDstFileRelativePath = "Assets";
					}
				}
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Set), GUILayout.Width(60)))//"Set"
			{
				if (IsGUIUsable)
				{
					//기본 경로
					//이전
					//string defaultPath = _bakeDstFileRelativePath;
					//if(string.IsNullOrEmpty(defaultPath))
					//{
					//	defaultPath = "Assets";
					//}

					//변경 v1.4.2 (상대 경로 변수를 삭제했으므로, 매번 절대 경롤르 바탕으로 상대 경로를 만든다.)

					string strFullPath = "";
					string strRelativePath = "";
					bool isValidDefaultPath = apEditorUtil.MakeRelativeDirectoryPathFromAssets(_bakeDstFilePath, ref strFullPath, ref strRelativePath);
					
					string defaultPath = "Assets";
					if(isValidDefaultPath && !string.IsNullOrEmpty(strRelativePath))
					{
						defaultPath = strRelativePath;
					}
					
					

					string nextPathFromDialog = EditorUtility.SaveFolderPanel("Save Path Folder", defaultPath, "");

					if (!string.IsNullOrEmpty(nextPathFromDialog))
					{
						string resultPathFull = "";
						string resultPathRelative = "";
						bool isValidPath = apEditorUtil.MakeRelativeDirectoryPathFromAssets(nextPathFromDialog, ref resultPathFull, ref resultPathRelative);
						if (isValidPath)
						{
							_bakeDstFilePath = resultPathFull;
							//_bakeDstFileRelativePath = resultPathRelative;//삭제 1.4.2
						}
						else
						{
							//유효하지 않다면, 상대 경로는 걍 Assets
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_WrongDst),
														_editor.GetText(TEXT.PSDBakeError_Body_WrongDst),
														_editor.GetText(TEXT.Close)
														);

							_bakeDstFilePath = "";
							//_bakeDstFileRelativePath = "Assets";//삭제 1.4.2
						}
					}
					apEditorUtil.ReleaseGUIFocus();
				}
				
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);

			//EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_AtlasBakingOption), GUILayout.Width(width));//"Atlas Baking Option"
			GUILayout.Space(10);

			
			apPSDLoader.BAKE_SIZE prev_bakeWidth = _bakeWidth;
			apPSDLoader.BAKE_SIZE prev_bakeHeight = _bakeHeight;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Atlas) + " " + _editor.GetText(TEXT.DLG_Width) + " : ", GUILayout.Width(120));//Atlas + Width
			apPSDLoader.BAKE_SIZE next_bakeWidth = (apPSDLoader.BAKE_SIZE)EditorGUILayout.Popup((int)_bakeWidth, _bakeDescription, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeWidth = next_bakeWidth;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Atlas) + " " + _editor.GetText(TEXT.DLG_Height) + " : ", GUILayout.Width(120));//Atlas + Height
			apPSDLoader.BAKE_SIZE next_bakeHeight = (apPSDLoader.BAKE_SIZE)EditorGUILayout.Popup((int)_bakeHeight, _bakeDescription, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeHeight = next_bakeHeight;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			int prev_bakeMaximumNumAtlas = _bakeMaximumNumAtlas;
			int prev_bakePadding = _bakePadding;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_MaximumAtlas) + " : ", GUILayout.Width(120));//Maximum Atlas
			int next_bakeMaximumNumAtlas = EditorGUILayout.DelayedIntField(_bakeMaximumNumAtlas, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeMaximumNumAtlas = next_bakeMaximumNumAtlas;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_Padding) + " : ", GUILayout.Width(120));//Padding
			int next_bakePadding = EditorGUILayout.DelayedIntField(_bakePadding, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakePadding = next_bakePadding;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			bool prev_bakeBlurOption = _bakeBlurOption;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_FixBorderProblem), GUILayout.Width(120)); //Fix Border Problem
			bool next_bakeBlurOption = EditorGUILayout.Toggle(_bakeBlurOption, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeBlurOption = next_bakeBlurOption;
			}
			EditorGUILayout.EndHorizontal();

			//추가 : MeshGroup의 Resize 결정
			GUILayout.Space(20);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_MeshGroupScaleOption), GUILayout.Width(width));//MeshGroup Scale Option
			GUILayout.Space(10);
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_ResizeRatio) + " (%) : ", GUILayout.Width(120));//Resize Ratio
			int next_bakeMeshGroupResizeX100 = EditorGUILayout.DelayedIntField(_bakeMeshGroupResizeX100, GUILayout.Width(width - 124));
			if (IsGUIUsable)
			{
				_bakeMeshGroupResizeX100 = Mathf.Clamp(next_bakeMeshGroupResizeX100, 1, 10000);
			}
			EditorGUILayout.EndHorizontal();
			

			//이제 Bake 가능한지 체크하자
			GUILayout.Space(20);

			if (
				//prev_isBakeResizable != _isBakeResizable ||
				prev_bakeWidth != _bakeWidth ||
				prev_bakeHeight != _bakeHeight ||
				prev_bakeMaximumNumAtlas != _bakeMaximumNumAtlas ||
				prev_bakePadding != _bakePadding ||
				//!string.Equals(prev_bakeDstFilePath, _bakeDstFilePath) ||//삭제 1.4.2
				prev_bakeBlurOption != _bakeBlurOption)
			{
				_isNeedBakeCheck = true;
			}

			if (_isNeedBakeCheck)
			{
				//이전 코드
				//CheckBakable();

				//Calculate를 하자
				_psdLoader.Step2_Calculate(
					//_bakeDstFilePath, _bakeDstFileRelativePath,//삭제 1.4.2
					 GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight),
					_bakeMaximumNumAtlas, _bakePadding,
					_bakeBlurOption,
					OnCalculateResult
					);
			}

			

			GUIStyle guiStyle_Result = new GUIStyle(GUI.skin.box);
			guiStyle_Result.alignment = TextAnchor.MiddleLeft;
			guiStyle_Result.normal.textColor = apEditorUtil.BoxTextColor;

			//경로가 지정되지 않은 경우 오류 메시지를 별도로 관리
			if(string.IsNullOrEmpty(_bakeDstFilePath))
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;


				GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Warning) + "\n[Save Path] is Empty", guiStyle_WarningBox, GUILayout.Width(width), GUILayout.Height(70));//Warning

				GUI.backgroundColor = prevColor;
			}
			else if (_isBakeWarning)
			{
				GUIStyle guiStyle_WarningBox = new GUIStyle(GUI.skin.box);
				guiStyle_WarningBox.alignment = TextAnchor.MiddleCenter;
				guiStyle_WarningBox.normal.textColor = apEditorUtil.BoxTextColor;


				GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);

				GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Warning) + "\n" + _bakeWarningMsg, guiStyle_WarningBox, GUILayout.Width(width), GUILayout.Height(70));//Warning

				GUI.backgroundColor = prevColor;
			}
			else
			{
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Bake), GUILayout.Width(width), GUILayout.Height(40)))//Bake
				{
					if (IsGUIUsable)
					{
						//이전 코드
						//StartBake();

						//추가 22.6.18 : Bake 버튼 누르면 선택된 Bake 결과가 해제되어야 한다.
						_selectedBakeData = null;

						_psdLoader.Step3_Bake(_loadKey_Bake, OnBakeResult, _loadKey_Calculated);
					}
				}

				//추가 22.6.21 (v1.4.0)
				//이미 저장된 Bake 정보를 이용해서 Bake를 할 수 있다.
				//if(GUILayout.Button("파일을 참조하여 Bake", GUILayout.Width(width)))
				//{
				//	if (IsGUIUsable)
				//	{
				//		string filePath = EditorUtility.OpenFilePanel(
				//							"Import Atlas information",
				//							apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.AtlasExport),
				//							"atl");

				//		if(!string.IsNullOrEmpty(filePath))
				//		{
				//			//추가 21.7.3 : 이스케이프 문자 삭제
				//			filePath = apUtil.ConvertEscapeToPlainText(filePath);

				//			apEditorUtil.SetLastExternalOpenSaveFilePath(filePath, apEditorUtil.SAVED_LAST_FILE_PATH.AtlasExport);//추가 21.3.1

				//			//다이얼로그를 열자
				//			_loadKey_ImportAtlas = apPSDAtlasImportDialog.ShowWindow(_editor, _psdLoader, filePath, OnAtlasImportResult);

				//		}
				//	}
				//}


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

					//이후
					//GUILayout.Box("[ Settings are changed ]"
					//				+ "\n  Expected Scale : " + _psdLoader.CalculatedResizeX100 + " %"
					//				+ "\n  Expected Atlas : " + _psdLoader.CalculatedAtlasCount,
					//				guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

					GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_SettingsAreChanged) + " ]"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedScale) + " : " + _psdLoader.CalculatedResizeX100 + " %"
									+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ExpectedAtlas) + " : " + _psdLoader.CalculatedAtlasCount,
									guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));


					
					GUI.backgroundColor = prevColor;

				}
			}
			GUILayout.Space(10);
			if (_loadKey_Bake != null)
			{
				//Bake가 되었다면 => 그 정보를 넣어주자
				//이전 : PSD Loader 사용 전
				//GUILayout.Box("[ Baked Result ]"
				//				+ "\n  Scale Percent : " + _resultBakeResizeX100 + " %"
				//				+ "\n  Atlas : " + _resultAtlasCount,
				//				guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

				//이후
				//GUILayout.Box("[ Baked Result ]"
				//				+ "\n  Scale Percent : " + _psdLoader.BakedResizeX100 + " %"
				//				+ "\n  Atlas : " + _psdLoader.BakedAtlasCount,
				//				guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

				GUILayout.Box("[ " + _editor.GetText(TEXT.DLG_PSD_BakeResult) + " ]"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_ScalePercent) + " : " + _psdLoader.BakedResizeX100 + " %"
								+ "\n  " + _editor.GetText(TEXT.DLG_PSD_Atlas) + " : " + _psdLoader.BakedAtlasCount,
								guiStyle_Result, GUILayout.Width(width), GUILayout.Height(60));

				

			}


			GUILayout.Space(20);

			if (IsProcessRunning)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();

				Rect barRect = new Rect(lastRect.x + 5, lastRect.y + 30, width - 5, 20);

				//이전 : PSD Loader 사용 전
				//float barRatio = Mathf.Clamp01((float)_workProcess.ProcessX100 / 100.0f);
				//EditorGUI.ProgressBar(barRect, barRatio, _threadProcessName);

				float barRatio = _psdLoader.GetImageBakingRatio();
				EditorGUI.ProgressBar(barRect, barRatio, _psdLoader.GetProcessLabel());

			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(height + 20);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}


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


		//private object _loadKey_ImportAtlas = null;
		//private void OnAtlasImportResult(apPSDAtlasImportDialog.RESULT result, apPSDAtlasImportDialog.AtlasFileMapping mapping, object loadKey)
		//{
		//	if(result != apPSDAtlasImportDialog.RESULT.Success
		//		|| _loadKey_ImportAtlas != loadKey
		//		|| mapping == null)
		//	{
		//		if(result == apPSDAtlasImportDialog.RESULT.Failed)
		//		{
		//			EditorUtility.DisplayDialog("Import Failed", "아틀라스 정보를 가져오는데 실패했습니다.", _editor.GetText(TEXT.Okay));
		//		}
		//		_loadKey_ImportAtlas = null;
		//		return;
		//	}

		//	_loadKey_ImportAtlas = null;

		//	//이제 이 정보(Mapping)를 이용해서 Bake를 하자
		//}






		// Bottom
		private void GUI_Bottom(int width, int height)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height));

			int btnWidth = 120;
			int btnWidth_Cancel = 100;
			int margin = width - (btnWidth * 2 + btnWidth_Cancel + 12 + 30 + 200 + 10 + 2 + 20);

			GUILayout.Space(10);
			EditorGUILayout.BeginVertical(GUILayout.Width(200));
			GUILayout.Space(2);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PSD_BackgroundColor), GUILayout.Width(200));//Background Color
			try
			{
				_glBackGroundColor = EditorGUILayout.ColorField(_glBackGroundColor, GUILayout.Width(80));
			}
			catch (Exception) { }

			EditorGUILayout.EndVertical();
			GUILayout.Space(margin);

			if (_step == LOAD_STEP.Step1_FileLoad)
			{
				GUILayout.Space(btnWidth + 4);
			}
			else
			{
				if (GUILayout.Button("< " +_editor.GetText(TEXT.DLG_PSD_Back), GUILayout.Width(btnWidth), GUILayout.Height(height)))//"Back"
				{
					if (IsGUIUsable)
					{
						MoveStep(false);
					}
				}
			}

			if (_step == LOAD_STEP.Step3_AtlasSetting)
			{
				bool isCompletable = (_loadKey_Bake != null
							&& _loadKey_Calculated != null
							&& _loadKey_Bake == _loadKey_Calculated);

				SetGUIVisible("Complete Button", isCompletable);
				if (IsDelayedGUIVisible("Complete Button"))
				{
					if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Complete), GUILayout.Width(btnWidth), GUILayout.Height(height)))//"Complete"
					{
						if (IsGUIUsable)
						{
							if (isCompletable)
							{
								//이전 코드
								//StartBakedImageSave();
								
								//처리 실패시 에러가 나온다. [v1.4.2]
								bool isSuccess = _psdLoader.Step4_ConvertToAnyPortrait(_bakeDstFilePath, OnConvertResult, null, null, null);
								
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
				}
				else
				{
					Color prevColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					GUILayout.Box(_editor.GetText(TEXT.DLG_PSD_Complete), GUILayout.Width(btnWidth), GUILayout.Height(height));//Complete

					GUI.backgroundColor = prevColor;
				}
			}
			else
			{
				//if (_isFileLoaded)//이전 코드
				if (_psdLoader.IsFileLoaded)
				{
					if (GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Next) + " >", GUILayout.Width(btnWidth), GUILayout.Height(height)))//Next
					{
						if (IsGUIUsable)
						{
							MoveStep(true);
						}
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
				if (IsGUIUsable)
				{
					//bool result = EditorUtility.DisplayDialog("Close", "Close PSD Load? (Data is Not Saved)", "Close", "Cancel");

					bool result = EditorUtility.DisplayDialog(_editor.GetText(TEXT.ClosePSDImport_Title),
																_editor.GetText(TEXT.ClosePSDImport_Body),
																_editor.GetText(TEXT.Close),
																_editor.GetText(TEXT.Cancel));

					if (result)
					{
						OnLoadComplete(false);
						//CloseDialog();
						_isRequestCloseDialog = true;//<<바로 Close하면 안된다.
					}
				}
			}



			EditorGUILayout.EndHorizontal();
		}



		private void OnConvertResult(bool isSuccess, List<Texture2D> resultTextures)
		{
			if(isSuccess)
			{
				OnLoadComplete(true);
			}
		}

		// Functions
		//----------------------------------------------------------
#region [미사용 : PSD Loader로 전환]
		//private void ClearPsdFile()
		//{
		//	_isFileLoaded = false;
		//	_fileFullPath = "";
		//	_fileNameOnly = "";
		//	_imageWidth = -1;
		//	_imageHeight = -1;
		//	_imageCenterPosOffset = Vector2.zero;

		//	_layerDataList.Clear();
		//	_selectedLayerData = null;

		//	_bakeDataList.Clear();
		//	_selectedBakeData = null;

		//	_bakeWidth = BAKE_SIZE.s1024;
		//	_bakeHeight = BAKE_SIZE.s1024;
		//	_bakeDstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)
		//	_bakeMaximumNumAtlas = 2;
		//	_bakePadding = 4;
		//	_bakeBlurOption = true;

		//	_isNeedBakeCheck = true;
		//	_bakeParams.Clear();

		//	_loadKey_CheckBake = null;
		//	_loadKey_Bake = null;

		//	_resultAtlasCount = 0;
		//	_resultBakeResizeX100 = 0;
		//	_resultPadding = 0;
		//}


		//private bool LoadPsdFile(string filePath)
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
		//		_fileFullPath = filePath;

		//		_fileNameOnly = "";

		//		if (_fileFullPath.Length > 4)
		//		{
		//			for (int i = _fileFullPath.Length - 5; i >= 0; i--)
		//			{
		//				string curChar = _fileFullPath.Substring(i, 1);
		//				if (curChar == "\\" || curChar == "/")
		//				{
		//					break;
		//				}
		//				_fileNameOnly = curChar + _fileNameOnly;
		//			}
		//		}
		//		_imageWidth = psdDoc.FileHeaderSection.Width;
		//		_imageHeight = psdDoc.FileHeaderSection.Height;
		//		_imageCenterPosOffset = new Vector2((float)_imageWidth * 0.5f, (float)_imageHeight * 0.5f);

		//		if (_imageWidth > PSD_IMAGE_FILE_MAX_SIZE || _imageHeight > PSD_IMAGE_FILE_MAX_SIZE)
		//		{
		//			//EditorUtility.DisplayDialog("PSD Load Failed", 
		//			//	"Image File is Too Large [ " + _imageWidth + " x " + _imageHeight + " ] (Maximum 5000 x 5000)", 
		//			//	"Okay");

		//			EditorUtility.DisplayDialog(_editor.GetText(TEXT.PSDBakeError_Title_Load),
		//											_editor.GetTextFormat(TEXT.PSDBakeError_Body_LoadSize, _imageWidth, _imageHeight),
		//											_editor.GetText(TEXT.Close)
		//											);
		//			ClearPsdFile();
		//			return false;
		//		}

		//		int curLayerIndex = 0;

		//		RecursiveAddLayer(psdDoc.Childs, 0, null, curLayerIndex);

		//		//클리핑이 가능한가 체크
		//		CheckClippingValidation();

		//		_isFileLoaded = true;

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


		//public void CheckBakable()
		//{
		//	_isNeedBakeCheck = false;
		//	_isBakeWarning = false;//<이게 True이면 Bake가 불가능하다.
		//	_bakeWarningMsg = "";
		//	//_bakeResizeRatioX100 = 100;
		//	_realBakeSizePerIndex = -1;
		//	_loadKey_CheckBake = null;
		//	_realBakedAtlasCount = 0;
		//	_realBakeResizeX100 = 100;

		//	CheckClippingValidation();

		//	if (_bakeMaximumNumAtlas <= 0)
		//	{
		//		_isBakeWarning = true;
		//		_bakeWarningMsg = "[Maximum Atlas] is less than 0";
		//		return;
		//	}

		//	if (_bakePadding < 0)
		//	{
		//		_isBakeWarning = true;
		//		_bakeWarningMsg = "[Padding] is less than 0";
		//		return;
		//	}

		//	//1. Path 미지정
		//	if (string.IsNullOrEmpty(_bakeDstFilePath))
		//	{
		//		_isBakeWarning = true;
		//		_bakeWarningMsg = "Save Path is empty";

		//		return;
		//	}

		//	//2. 크기를 비교하자
		//	//W,H 합계, 최대값, 최소값, 영역 전체의 합
		//	int nLayer = 0;
		//	int sumWidth = 0;
		//	int sumHeight = 0;
		//	int maxWidth = -1;
		//	int maxHeight = -1;
		//	int minWidth = -1;
		//	int minHeight = -1;
		//	double sumArea = 0;

		//	apPSDLayerData curLayer = null;
		//	List<apPSDLayerData> bakableLayersX = new List<apPSDLayerData>(); //X축 큰거부터 체크
		//	List<apPSDLayerData> bakableLayersY = new List<apPSDLayerData>(); //Y축 큰거부터 체크
		//	for (int i = 0; i < _layerDataList.Count; i++)
		//	{
		//		curLayer = _layerDataList[i];
		//		if (!curLayer._isBakable || curLayer._image == null)
		//		{
		//			continue;
		//		}
		//		bakableLayersX.Add(curLayer);
		//		bakableLayersY.Add(curLayer);
		//		nLayer++;
		//		int curWidth = curLayer._width + (_bakePadding * 2);
		//		int curHeight = curLayer._height + (_bakePadding * 2);

		//		sumWidth += curWidth;
		//		sumHeight += curHeight;

		//		if (maxWidth < 0 || curWidth > maxWidth)
		//		{
		//			maxWidth = curWidth;
		//		}
		//		if (maxHeight < 0 || curHeight > maxHeight)
		//		{
		//			maxHeight = curHeight;
		//		}
		//		if (minWidth < 0 || curWidth < minWidth)
		//		{
		//			minWidth = curWidth;
		//		}
		//		if (minHeight < 0 || curHeight < minHeight)
		//		{
		//			minHeight = curHeight;
		//		}
		//		sumArea += (curWidth * curHeight);
		//	}

		//	//_needBakeResizeX100 = 100;

		//	_bakeParams.Clear();

		//	if (sumWidth < 10 || sumHeight < 10)
		//	{
		//		_isBakeWarning = true;//<이게 True이면 Bake가 불가능하다.
		//		_bakeWarningMsg = "Too Small Image";
		//		return;
		//	}

		//	//이제 본격적으로 만들어보자
		//	//Slot이라는 개념을 만들자.
		//	//Slot의 최소 크기는 최소 W,H의 크기값을 기준으로 한다.
		//	//minWH의 1/10의 값을 기준으로 4~32의 값을 가진다.


		//	//시작 Resize 값을 결정한다. (기본 100)
		//	//만약 maxWH가 요청한 Bake사이즈보다 크다면 -> 그만큼 리사이즈를 먼저 한다.
		//	//반복 수행
		//	//만약 maxWH가 요청한 Bake 사이즈보다 작다면 리사이즈가 없다는 가정으로 정사각형으로 만든다. (최대 Atlas 결과값이 오버가 되면 리사이즈를 해서 다시 수행한다.)
		//	int curResizeX100 = 100;
		//	float curResizeRatio = 1.0f;
		//	int slotSize = Mathf.Clamp((Mathf.Min(minWidth, minHeight) / 10), 4, 32);
		//	_realBakeSizePerIndex = slotSize;//<<이 값을 곱해서 실제 위치를 구한다.

		//	int numSlotAxisX = GetBakeSize(_bakeWidth) / slotSize;
		//	int numSlotAxisY = GetBakeSize(_bakeHeight) / slotSize;

		//	bool isSuccess = false;

		//	float baseRatioW = (float)GetBakeSize(_bakeWidth) / (float)maxWidth;
		//	float baseRatioH = (float)GetBakeSize(_bakeHeight) / (float)maxHeight;
		//	int baseRatioX100 = (int)((Mathf.Max(baseRatioW, baseRatioH) + 0.5f) * 100.0f);

		//	if (baseRatioX100 % 5 != 0)
		//	{
		//		baseRatioX100 = ((baseRatioX100 + 5) / 5) * 5;
		//	}

		//	if (baseRatioX100 < 100)
		//	{
		//		//maxW 또는 maxH가 이미지 크기를 넘었다.
		//		//리사이즈를 해야한다.
		//		//스케일은 5단위로 한다.
		//		curResizeX100 = baseRatioX100;
		//	}

		//	List<int[,]> atlasSlots = new List<int[,]>();
		//	for (int i = 0; i < _bakeMaximumNumAtlas; i++)
		//	{
		//		atlasSlots.Add(new int[numSlotAxisX, numSlotAxisY]);
		//	}

		//	//크기가 큰 이미지부터 내림차순
		//	bakableLayersX.Sort(delegate (apPSDLayerData a, apPSDLayerData b)
		//	{
		//		return b._width - a._width;
		//	});

		//	bakableLayersY.Sort(delegate (apPSDLayerData a, apPSDLayerData b)
		//	{
		//		return b._height - a._height;
		//	});


		//	List<apPSDLayerData> checkLayersX = new List<apPSDLayerData>();
		//	List<apPSDLayerData> checkLayersY = new List<apPSDLayerData>();
		//	while (true)
		//	{
		//		curResizeRatio = (float)curResizeX100 / 100.0f;
		//		if (curResizeX100 < 10)
		//		{
		//			isSuccess = false;
		//			break;//실패다.
		//		}

		//		//일단 슬롯을 비워두자
		//		for (int iAtlas = 0; iAtlas < atlasSlots.Count; iAtlas++)
		//		{
		//			int[,] slots = atlasSlots[iAtlas];
		//			for (int iX = 0; iX < numSlotAxisX; iX++)
		//			{
		//				for (int iY = 0; iY < numSlotAxisY; iY++)
		//				{
		//					slots[iX, iY] = -1;//<인덱스 할당 정보 초기화
		//				}
		//			}
		//		}

		//		//계산할 LayerData를 다시 리셋하자.
		//		checkLayersX.Clear();
		//		checkLayersY.Clear();
		//		for (int i = 0; i < bakableLayersX.Count; i++)
		//		{
		//			checkLayersX.Add(bakableLayersX[i]);
		//			checkLayersY.Add(bakableLayersY[i]);
		//		}

		//		_bakeParams.Clear();

		//		bool isCheckX = true;
		//		//X, Y축을 번갈아가면서 체크한다.
		//		//X일땐 X축부터 체크하면서 빈칸을 채운다.


		//		apPSDLayerData nextLayer = null;

		//		isSuccess = false;
		//		while (true)
		//		{
		//			if (checkLayersX.Count == 0 || checkLayersY.Count == 0)
		//			{
		//				//다 넣었다.
		//				isSuccess = true;
		//				break;
		//			}

		//			//다음 Layer를 꺼내서 슬롯을 체크한다.
		//			if (isCheckX)
		//			{
		//				nextLayer = checkLayersX[0];
		//			}
		//			else
		//			{
		//				nextLayer = checkLayersY[0];
		//			}

		//			//꺼낸 값은 Layer에서 삭제한다.
		//			checkLayersX.Remove(nextLayer);
		//			checkLayersY.Remove(nextLayer);


		//			int layerIndex = nextLayer._layerIndex;
		//			//Slot Width, Height를 계산하자
		//			int slotWidth = (int)(((float)nextLayer._width * curResizeRatio) + (_bakePadding * 2)) / slotSize;
		//			int slotHeight = (int)(((float)nextLayer._height * curResizeRatio) + (_bakePadding * 2)) / slotSize;
		//			//이제 빈칸을 찾자!

		//			//Atlas 앞부터 시작해서
		//			//Check X인 경우는 : Y -> X순서
		//			//Check Y인 경우는 : X -> Y순서
		//			bool isAddedSuccess = false;
		//			int iAddedX = -1;
		//			int iAddedY = -1;
		//			int iAddedAtlas = -1;
		//			for (int iAtlas = 0; iAtlas < atlasSlots.Count; iAtlas++)
		//			{
		//				int[,] slots = atlasSlots[iAtlas];


		//				bool addResult = false;


		//				if (isCheckX)
		//				{
		//					//X먼저 계산할 때
		//					for (int iY = 0; iY < numSlotAxisY; iY++)
		//					{
		//						for (int iX = 0; iX < numSlotAxisX; iX++)
		//						{
		//							addResult = AddToSlot(iX, iY, slotWidth, slotHeight, slots, numSlotAxisX, numSlotAxisY, layerIndex);
		//							if (addResult)
		//							{
		//								iAddedX = iX;
		//								iAddedY = iY;
		//								iAddedAtlas = iAtlas;
		//								break;
		//							}
		//						}

		//						if (addResult)
		//						{ break; }
		//					}
		//				}
		//				else
		//				{
		//					//Y먼저 계산할 때
		//					for (int iX = 0; iX < numSlotAxisX; iX++)
		//					{
		//						for (int iY = 0; iY < numSlotAxisY; iY++)
		//						{
		//							addResult = AddToSlot(iX, iY, slotWidth, slotHeight, slots, numSlotAxisX, numSlotAxisY, layerIndex);
		//							if (addResult)
		//							{
		//								iAddedX = iX;
		//								iAddedY = iY;
		//								iAddedAtlas = iAtlas;
		//								break;
		//							}
		//						}

		//						if (addResult)
		//						{ break; }
		//					}
		//				}

		//				if (addResult)
		//				{
		//					isAddedSuccess = true;
		//					break;
		//				}
		//			}

		//			if (isAddedSuccess)
		//			{
		//				//적당히 넣었다.
		//				LayerBakeParam newBakeParam = new LayerBakeParam(nextLayer, iAddedAtlas, iAddedX, iAddedY);
		//				_bakeParams.Add(newBakeParam);

		//				//실제로 작성된 Atlas의 개수를 확장한다.
		//				if (iAddedAtlas + 1 > _realBakedAtlasCount)
		//				{
		//					_realBakedAtlasCount = iAddedAtlas + 1;
		//				}
		//			}
		//			else
		//			{
		//				//하나라도 실패하면 돌아간다.
		//				isSuccess = false;
		//				break;
		//			}


		//			isCheckX = !isCheckX;//토글!
		//								 //다음 이미지를 넣어보자 -> 루프
		//		}

		//		//모두 넣었다면
		//		if (isSuccess)
		//		{
		//			break;
		//		}

		//		curResizeX100 -= 5;
		//	}

		//	if (nLayer > 0 && _realBakedAtlasCount == 0)
		//	{
		//		isSuccess = false;
		//		_isBakeWarning = true;
		//		_bakeWarningMsg = "No Baked Atlas";
		//	}


		//	if (!isSuccess)
		//	{
		//		_isBakeWarning = true;//<이게 True이면 Bake가 불가능하다.
		//		_bakeWarningMsg = "Need to increase [Number of Maximum Atlas]";

		//		//if(!_isBakeResizable)
		//		//{
		//		//	_bakeWarningMsg = "Need to turn on [Auto Resize] \n or [Increase Number of Maximum Atlas]";
		//		//}
		//		//else
		//		//{
		//		//	_bakeWarningMsg = "Need to increase [Number of Maximum Atlas]";
		//		//}
		//		return;
		//	}

		//	_realBakeResizeX100 = curResizeX100;
		//	_loadKey_CheckBake = new object();//마지막으로 Bake Check가 끝났다는 Key를 만들어주자
		//}



		////슬롯에 레이어를 넣을 수 있는지 확인하자
		//private bool AddToSlot(int startPosX, int startPosY, int slotWidth, int slotHeight, int[,] targetSlot, int slotSizeX, int slotSizeY, int addedLayerIndex)
		//{
		//	if (targetSlot[startPosX, startPosY] >= 0)
		//	{
		//		//시작점에 뭔가가 있다.
		//		return false;
		//	}

		//	if (startPosX + slotWidth >= slotSizeX ||
		//		startPosY + slotHeight >= slotSizeY)
		//	{
		//		//영역을 벗어난다.
		//		return false;
		//	}

		//	for (int iX = startPosX; iX <= startPosX + slotWidth; iX++)
		//	{
		//		for (int iY = startPosY; iY <= startPosY + slotHeight; iY++)
		//		{
		//			if (targetSlot[iX, iY] >= 0)
		//			{
		//				return false;//뭔가가 있다.
		//			}
		//		}
		//	}

		//	//넣어봤는데 괜찮네요
		//	for (int iX = startPosX; iX <= startPosX + slotWidth; iX++)
		//	{
		//		for (int iY = startPosY; iY <= startPosY + slotHeight; iY++)
		//		{
		//			targetSlot[iX, iY] = addedLayerIndex;
		//		}
		//	}
		//	return true;
		//}



		////중요! Bake!
		//private bool StartBake()
		//{
		//	if (_loadKey_CheckBake == null)
		//	{
		//		return false;
		//	}
		//	if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
		//	{
		//		return false;
		//	}

		//	_selectedBakeData = null;

		//	CloseThread();

		//	_threadProcessName = "Bake Atlas..";

		//	_bakeDataList.Clear();

		//	_workProcess.Add(Work_Bake_1, _realBakedAtlasCount);
		//	_workProcess.Add(Work_Bake_2, _bakeParams.Count);
		//	_workProcess.Add(Work_Bake_3, _bakeDataList.Count);
		//	_workProcess.Add(Work_Bake_4, 1);

		//	_workProcess.StartRun("Bake Atlas");
		//	//_thread = new Thread(new ThreadStart(Thread_Bake));
		//	//_thread.Start();

		//	_isImageBaking = true;

		//	return true;
		//}

		//private bool Work_Bake_1(int index)
		//{
		//	if (_loadKey_CheckBake == null)
		//	{
		//		return false;
		//	}
		//	if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
		//	{
		//		return false;
		//	}
		//	if (index >= _realBakedAtlasCount)
		//	{
		//		Debug.LogError("Work_Bake_1 Exception : Index Over (" + index + " / " + _realBakedAtlasCount + ")");
		//		return false;
		//	}

		//	apPSDBakeData newBakeData = new apPSDBakeData(index, GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight));
		//	newBakeData.ReadyToBake();
		//	_bakeDataList.Add(newBakeData);

		//	//WorkProcess 갱신
		//	_workProcess.ChangeCount(2, _bakeDataList.Count);
		//	return true;

		//}

		//private bool Work_Bake_2(int index)
		//{
		//	if (_loadKey_CheckBake == null)
		//	{
		//		return false;
		//	}
		//	if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
		//	{
		//		return false;
		//	}
		//	if (index >= _bakeParams.Count)
		//	{
		//		Debug.LogError("Work_Bake_2 Exception : Index Over (" + index + " / " + _bakeParams.Count + ")");
		//		return false;
		//	}

		//	float bakeResizeRatio = Mathf.Clamp01(((float)_realBakeResizeX100 / 100.0f));

		//	LayerBakeParam bakeParam = _bakeParams[index];
		//	apPSDLayerData targetLayer = bakeParam._targetLayer;
		//	if (targetLayer._image == null)
		//	{
		//		Debug.LogError("Work_Bake_2 : No Image");
		//		return true;
		//	}

		//	//일단 레이어에 Bake 정보를 입력하자
		//	targetLayer._bakedAtalsIndex = bakeParam._atlasIndex;
		//	targetLayer._bakedImagePos_Left = bakeParam._posOffset_X * _realBakeSizePerIndex;
		//	targetLayer._bakedImagePos_Top = bakeParam._posOffset_Y * _realBakeSizePerIndex;
		//	targetLayer._bakedWidth = (int)((float)targetLayer._width * bakeResizeRatio + 0.5f);
		//	targetLayer._bakedHeight = (int)((float)targetLayer._height * bakeResizeRatio + 0.5f);

		//	//Bake Image에 값을 넣자
		//	apPSDBakeData targetBakeData = _bakeDataList[bakeParam._atlasIndex];
		//	bool isResult = targetBakeData.AddImage(targetLayer,
		//												targetLayer._bakedImagePos_Left,
		//												targetLayer._bakedImagePos_Top,
		//												bakeResizeRatio,
		//												targetLayer._bakedWidth,
		//												targetLayer._bakedHeight,
		//												_bakePadding);

		//	//Debug.Log("Bake [AddImage] : " + index + " >> " + bakeParam._atlasIndex);

		//	return isResult;
		//}

		//private bool Work_Bake_3(int index)
		//{
		//	if (_loadKey_CheckBake == null)
		//	{
		//		return false;
		//	}
		//	if (_realBakeSizePerIndex <= 0 || _realBakeResizeX100 <= 0)
		//	{
		//		return false;
		//	}
		//	if (index >= _bakeDataList.Count)
		//	{
		//		Debug.LogError("Work_Bake_3 Exception : Index Over (" + index + " / " + _bakeDataList.Count + ")");
		//		return false;
		//	}

		//	//이제 실제로 Texture2D로 바꾸어주자
		//	_bakeDataList[index].EndToBake(_bakeBlurOption, _bakePadding);
		//	//Debug.Log("EndToBake : " + index);
		//	return true;
		//}

		//private bool Work_Bake_4(int index)
		//{
		//	_loadKey_Bake = _loadKey_CheckBake;//체크했던 Bake 값이 같음을 설정해주자
		//	_resultAtlasCount = _realBakedAtlasCount;
		//	_resultBakeResizeX100 = _realBakeResizeX100;
		//	_resultPadding = _bakePadding;
		//	return true;
		//}




		//// Thread
		////-------------------------------------------------------------------
		//private void StartBakedImageSave()
		//{
		//	CloseThread();

		//	//Debug.Log("Start Baked Image Save");
		//	_threadProcessName = "Convert PSD Data To Editor..";

		//	_workProcess.Add(Work_BakedImageSave_1, _bakeDataList.Count);
		//	_workProcess.Add(Work_BakdImageSave_2, 1);
		//	_workProcess.StartRun("Convert PSD Data To Editor");
		//}

		//private bool Work_BakedImageSave_1(int index)
		//{
		//	if (_bakeDataList.Count == 0 || _loadKey_Bake == null)
		//	{
		//		return false;
		//	}
		//	if (string.IsNullOrEmpty(_bakeDstFilePath) || string.IsNullOrEmpty(_fileNameOnly) || string.IsNullOrEmpty(_bakeDstFileRelativePath))
		//	{
		//		return false;
		//	}
		//	if (index >= _bakeDataList.Count)
		//	{
		//		Debug.LogError("Work BakedImageSave - 1 : Index Over (" + index + " / " + _bakeDataList.Count + ")");
		//		return false;
		//	}

		//	SaveBakeImage(index);
		//	ReimportBakedImage(index);

		//	return true;
		//}

		//private bool Work_BakdImageSave_2(int index)
		//{
		//	OnLoadComplete(true);
		//	return true;
		//}




		//private void SaveBakeImage(int iBakeDataList)
		//{
		//	try
		//	{
		//		apPSDBakeData curBakeData = _bakeDataList[iBakeDataList];
		//		byte[] data = curBakeData._bakedImage.EncodeToPNG();

		//		//F:/MainWorks/UnityProjects/AnyPortrait/AnyPortrait/Assets/Sample
		//		string path = _bakeDstFilePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";
		//		string relPath = _bakeDstFileRelativePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";

		//		for (int iLayer = 0; iLayer < curBakeData._bakedLayerData.Count; iLayer++)
		//		{
		//			curBakeData._bakedLayerData[iLayer]._textureAssetPath = relPath;
		//			curBakeData._bakedLayerData[iLayer]._bakedData = curBakeData;
		//		}

		//		File.WriteAllBytes(path, data);


		//		//AssetDatabase.CreateAsset(curBakeData._bakedImage, relPath);
		//		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

		//	}
		//	catch (Exception)
		//	{
		//		return;
		//	}
		//}

		//private void ReimportBakedImage(int iBakeDataList)
		//{
		//	try
		//	{
		//		//string path = _bakeDstFilePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";
		//		string relPath = _bakeDstFileRelativePath + "/" + _fileNameOnly + "_" + iBakeDataList + ".png";

		//		AssetDatabase.SaveAssets();

		//		//AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceSynchronousImport);

		//		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);


		//		//-------------------------------------------------------------------
		//		// Unity 5.5부터는 TextureImporter를 호출하기 전에
		//		// AssetDatabase에서 한번 열어서 Apply를 해줘야 한다.
		//		//-------------------------------------------------------------------
		//		Texture2D tex2D = AssetDatabase.LoadAssetAtPath<Texture2D>(relPath);
		//		if (tex2D != null)
		//		{
		//			try
		//			{
		//				//tex2D.Apply(false, true);
		//				AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceUpdate);

		//				tex2D.Apply();
		//			}
		//			catch(Exception)
		//			{
		//				//Debug.LogError("Sub Exception : Texture Apply : " + ex2);
		//			}
		//		}
		//		//-------------------------------------------------------------------

		//		//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);


		//		TextureImporter ti = TextureImporter.GetAtPath(relPath) as TextureImporter;


		//		if (ti == null)
		//		{
		//			Debug.LogError("Bake Error : Path : " + relPath);
		//		}
		//		else
		//		{
		//			//TextureImporterSettings tiSetting = new TextureImporterSettings();
		//			//tiSetting.filterMode = FilterMode.Bilinear;
		//			//tiSetting.mipmapEnabled = false;
		//			////tiSetting.textureFormat = TextureImporterFormat.RGBA32;//Deprecated
		//			//tiSetting.wrapMode = TextureWrapMode.Clamp;
		//			//tiSetting.alphaSource = TextureImporterAlphaSource.FromInput;


		//			//ti.SetTextureSettings(tiSetting);
		//			//ti.maxTextureSize = 4096;
		//			//ti.textureCompression = TextureImporterCompression.Uncompressed;

		//			//Debug.Log("현재 Color Space : " + apEditorUtil.GetColorSpace());
		//			if(apEditorUtil.IsGammaColorSpace())
		//			{
		//				ti.sRGBTexture = true;//Gamma인 경우 sRGB를 사용한다.
		//			}
		//			else
		//			{
		//				ti.sRGBTexture = false;//Linear인 경우 sRGB를 사용하지 않는다.
		//			}

		//			ti.SaveAndReimport();
		//			AssetDatabase.Refresh();
		//		}

		//	}
		//	catch (Exception)
		//	{
		//		//Debug.LogError("Reimport Exception : " + ex);
		//	}
		//} 
#endregion


		private int GetBakeSize(apPSDLoader.BAKE_SIZE bakeSize)
		{
			switch (bakeSize)
			{
				case apPSDLoader.BAKE_SIZE.s256:
					return 256;
				case apPSDLoader.BAKE_SIZE.s512:
					return 512;
				case apPSDLoader.BAKE_SIZE.s1024:
					return 1024;
				case apPSDLoader.BAKE_SIZE.s2048:
					return 2048;
				case apPSDLoader.BAKE_SIZE.s4096:
					return 4096;
			}
			return 4096;
		}
		// Return Event
		//----------------------------------------------------------


		public void OnLoadComplete(bool isResult)
		{
			if (_funcResult != null)
			{
				//상대 경로를 다시 계산한다. [v1.4.2]
				string strFullPath = "";
				string strRelativePath = "";
				apEditorUtil.MakeRelativeDirectoryPathFromAssets(_bakeDstFilePath, ref strFullPath, ref strRelativePath);


				if (isResult)
				{
					//이전 코드
					//_funcResult(isResult, _loadKey, _fileNameOnly, _layerDataList, (float)_resultBakeResizeX100 / 100.0f, _imageWidth, _imageHeight, _resultPadding, GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight));

					_funcResult(	isResult, 
									_loadKey, 
									_psdLoader.FileName, _psdLoader.FileFullPath,
									_psdLoader.PSDLayerDataList, 
									//(float)_psdLoader.BakedResizeX100 / 100.0f, 
									//(float)_bakeMeshGroupResizeX100 / 100.0f,
									_psdLoader.BakedResizeX100,
									_bakeMeshGroupResizeX100,
									_psdLoader.PSDImageWidth, _psdLoader.PSDImageHeight,
									_psdLoader.BakedPadding, 
									GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight),
									_bakeMaximumNumAtlas,
									_bakeBlurOption,
									_bakeDstFilePath,

									//_bakeDstFileRelativePath//이전
									strRelativePath//변경 v1.4.2
									);
				}
				else
				{
					//이전 코드
					//_funcResult(isResult, _loadKey, _fileNameOnly, null, (float)_resultBakeResizeX100 / 100.0f, _imageWidth, _imageHeight, _resultPadding, GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight));

					_funcResult(isResult, 
						_loadKey, 
						_psdLoader.FileName, _psdLoader.FileFullPath,
						null, 
						//(float)_psdLoader.BakedResizeX100 / 100.0f, 
						//(float)_bakeMeshGroupResizeX100 / 100.0f,
						_psdLoader.BakedResizeX100,
						_bakeMeshGroupResizeX100,
						_psdLoader.PSDImageWidth, _psdLoader.PSDImageHeight,
						_psdLoader.BakedPadding, 
						GetBakeSize(_bakeWidth), GetBakeSize(_bakeHeight),
						_bakeMaximumNumAtlas,
						_bakeBlurOption,
						_bakeDstFilePath,
						
						//_bakeDstFileRelativePath//이전
						strRelativePath//변경 v1.4.2
						);
				}
			}
			//CloseDialog();
			_isRequestCloseDialog = true;
		}

		public void MoveStep(bool isMoveNext)
		{
			LOAD_STEP nextStep = _step;
			switch (_step)
			{
				case LOAD_STEP.Step1_FileLoad:
					if (!isMoveNext)
					{ nextStep = LOAD_STEP.Step1_FileLoad; }
					else
					{ nextStep = LOAD_STEP.Step2_LayerCheck; }
					break;

				case LOAD_STEP.Step2_LayerCheck:
					if (!isMoveNext)
					{ nextStep = LOAD_STEP.Step1_FileLoad; }
					else
					{ nextStep = LOAD_STEP.Step3_AtlasSetting; }
					break;

				case LOAD_STEP.Step3_AtlasSetting:
					if (!isMoveNext)
					{ nextStep = LOAD_STEP.Step2_LayerCheck; }
					else
					{ nextStep = LOAD_STEP.Step3_AtlasSetting; }
					break;
			}

			if (isMoveNext)
			{
				if (nextStep == LOAD_STEP.Step2_LayerCheck)
				{

				}
				else if (nextStep == LOAD_STEP.Step3_AtlasSetting)
				{

				}
			}

			_step = nextStep;
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

		//-----------------------------------------------------------------------------------------------
		private bool IsNextButtonAvailable()
		{
			switch (_step)
			{
				case LOAD_STEP.Step1_FileLoad:
					if(_psdLoader.IsFileLoaded)
					{
						return true;
					}
					break;

				case LOAD_STEP.Step2_LayerCheck:
					return true;
					//break;

				case LOAD_STEP.Step3_AtlasSetting:
					if(_psdLoader.IsCalculated && _psdLoader.IsBaked)
					{
						return true;
					}
					break;

			}
			return false;
		}
		//--------------------------------------------------------------------------
		//private void SetDelayedGUIEvent(string keyName, bool value)
		//{
		//	if(!_delayedGUIEvent_Request.ContainsKey(keyName))
		//	{
		//		_delayedGUIEvent_Request.Add(keyName, value);
		//		_delayedGUIEvent_Result.Add(keyName, false);//<<Result의 기본값은 False이다.
		//	}
		//	else
		//	{
		//		_delayedGUIEvent_Request[keyName] = value;
		//	}
		//	//private Dictionary<string, bool> _delayedGUIEvent_Request = new Dictionary<string, bool>();
		////private Dictionary<string, bool> _delayedGUIEvent_Result = new Dictionary<string, bool>();
		//}

		//private void UpdateDelayedGUIEvents(EventType eventType)
		//{
		//	if(eventType == EventType.Layout)
		//	{
		//		foreach (KeyValuePair<string, bool> guiEvent in _delayedGUIEvent_Request)
		//		{
		//			//Layout일 때에는 Key가 갱신된다.
		//			_delayedGUIEvent_Result[guiEvent.Key] = guiEvent.Value;
		//		}
		//	}
		//	else
		//	{
		//		foreach (KeyValuePair<string, bool> guiEvent in _delayedGUIEvent_Request)
		//		{
		//			//Layout이 아닐 때에는 False 요청일 때에만 갱신된다.
		//			if (!guiEvent.Value)
		//			{
		//				_delayedGUIEvent_Result[guiEvent.Key] = guiEvent.Value;
		//			}
		//		}
		//	}
		//}

		//private bool IsDelayedGUIEvent(string keyName)
		//{
		//	if(_delayedGUIEvent_Result.ContainsKey(keyName))
		//	{
		//		return _delayedGUIEvent_Result[keyName];
		//	}
		//	return false;
		//}

	}

	

}