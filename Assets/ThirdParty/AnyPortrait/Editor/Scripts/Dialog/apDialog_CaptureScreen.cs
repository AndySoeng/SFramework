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

	public class apDialog_CaptureScreen : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_CaptureScreen s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		//private object _loadKey = null;

		private Vector2 _scroll = Vector2.zero;

		private apAnimClip _selectedAnimClip = null;
		private apRootUnit _curRootUnit = null;
		private List<apAnimClip> _animClips = new List<apAnimClip>();

		////private enum EXPORT_TYPE
		////{
		////	None,
		////	Thumbnail,
		////	PNG,
		////	GIFAnimation
		////}
		////private EXPORT_TYPE _exportRequestType = EXPORT_TYPE.None;
		////private EXPORT_TYPE _exportProcessType = EXPORT_TYPE.None;
		////private int _exportProcessX100 = -1;
		////private int _iProcess = 0;
		////private int _iProcessCount = 0;

		//private bool IsGUIUsable { get { return _exportProcessType == EXPORT_TYPE.None; } }

		//private string _prevFilePath = "";
		private string _prevFilePath_Directory = "";

		private enum CAPTURE_MODE
		{
			None,
			Capturing_Thumbnail,//<<썸네일 캡쳐중
			Capturing_ScreenShot,//<<ScreenShot 캡쳐중
			Capturing_GIF_Animation,//GIF 애니메이션 캡쳐중
		}
		private CAPTURE_MODE _captureMode = CAPTURE_MODE.None;
		private object _captureLoadKey = null;

		//GIF 애니메이션의 절차적 처리를 위한 변수
		private bool _isLoopAnimation = false;
		private bool _isAnimFirstFrame = false;
		private int _curAnimFrame = 0;
		private int _startAnimFrame = 0;
		private int _lastAnimFrame = 0;
		private int _curAnimLoop = 0;
		private int _animLoopCount = 0;
		private int _curAnimProcess = 0;
		private int _totalAnimProcess = 0;
		private int _gifAnimQuality = 0;

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apPortrait portrait)
		{
			//Debug.Log("Show Dialog - Portrait Setting");
			CloseDialog();


			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_CaptureScreen), true, "Capture", true);
			apDialog_CaptureScreen curTool = curWindow as apDialog_CaptureScreen;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);


				s_window.Init(editor, portrait, loadKey);

				return loadKey;
			}
			else
			{
				return null;
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
		public void Init(apEditor editor, apPortrait portrait, object loadKey)
		{
			_editor = editor;
			//_loadKey = loadKey;
			_targetPortrait = portrait;
			_selectedAnimClip = null;

			//_exportRequestType = EXPORT_TYPE.None;
			//_exportProcessType = EXPORT_TYPE.None;
			//_exportProcessX100 = 0;

			_captureMode = CAPTURE_MODE.None;
			_captureLoadKey = null;
		}

		// Update
		//------------------------------------------------------------------
		void Update()
		{
			if (Application.isPlaying)
			{
				return;
			}

			//Debug.Log("Update");
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				//Debug.LogError("Exit - Editor / Portrait is Null");
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자 + Overall Menu가 아니라면..
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait || _editor.Select.SelectionType != apSelection.SELECTION_TYPE.Overall)
			{
				//Debug.LogError("Exit - Editor / Portrait Missmatch");
				CloseDialog();
				return;
			}

			if(_captureMode != CAPTURE_MODE.None)
			{
				//캡쳐 중에는 다른 UI 제어 불가
				GUILayout.Space((height / 2) - 10);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space((width / 2) - 70);
				EditorGUILayout.LabelField("Please Wait a moment...");
				EditorGUILayout.EndHorizontal();

				if(_captureMode == CAPTURE_MODE.Capturing_GIF_Animation)
				{
					Rect barRect = new Rect((width / 2) - 100, height / 2 + 30, 200, 20);
					float barRatio = (float)(_curAnimProcess) / (float)(_totalAnimProcess);
					string barLabel = (int)(Mathf.Clamp01(barRatio) * 100.0f) + " %";
					EditorGUI.ProgressBar(barRect, barRatio, barLabel);
				}
				
				return;
			}

			#region [미사용 코드]
			//여기서 체크 및 실행하자
			//Request => Process => Process 처리
			//if (_exportProcessType == EXPORT_TYPE.None && _exportRequestType != EXPORT_TYPE.None)
			//{
			//	//_iProcess = 0;
			//	switch (_exportRequestType)
			//	{
			//		case EXPORT_TYPE.None:
			//			break;

			//		case EXPORT_TYPE.Thumbnail:
			//			_exportProcessType = EXPORT_TYPE.Thumbnail;
			//			break;
			//		case EXPORT_TYPE.PNG:
			//			_exportProcessType = EXPORT_TYPE.PNG;
			//			break;
			//		case EXPORT_TYPE.GIFAnimation:
			//			_exportProcessType = EXPORT_TYPE.GIFAnimation;
			//			break;
			//	}

			//	_exportRequestType = EXPORT_TYPE.None;

			//}
			//switch (_exportProcessType)
			//{
			//	case EXPORT_TYPE.None:
			//		break;
			//	case EXPORT_TYPE.Thumbnail:
			//		Process_MakeThumbnail();
			//		break;
			//	case EXPORT_TYPE.PNG:
			//		Process_PNGScreenShot();
			//		break;
			//	case EXPORT_TYPE.GIFAnimation:
			//		//Process_MakeGIF();//<<사용 안함
			//		break;
			//} 
			#endregion

			_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(width), GUILayout.Height(height));
			width -= 24;
			EditorGUILayout.BeginVertical(GUILayout.Width(width));

			int settingWidth = ((width - 10) / 3) - 4;
			int settingWidth_Label = 50;
			int settingWidth_Value = settingWidth - (50 + 8);
			int settingHeight = 70;
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Setting));//"Setting"
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(settingHeight));
			GUILayout.Space(5);

			//Position
			//------------------------
			EditorGUILayout.BeginVertical(GUILayout.Width(settingWidth), GUILayout.Height(settingHeight));


			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Position), GUILayout.Width(settingWidth));//"Position"
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("X", GUILayout.Width(settingWidth_Label));
			int posX = EditorGUILayout.DelayedIntField(_editor._captureFrame_PosX, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			EditorGUILayout.LabelField("Y", GUILayout.Width(settingWidth_Label));
			int posY = EditorGUILayout.DelayedIntField(_editor._captureFrame_PosY, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			//------------------------


			//Capture Size
			//------------------------
			EditorGUILayout.BeginVertical(GUILayout.Width(settingWidth), GUILayout.Height(settingHeight));

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_CaptureSize), GUILayout.Width(settingWidth));//"Capture Size"
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			//"Width"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Width), GUILayout.Width(settingWidth_Label));
			int srcSizeWidth = EditorGUILayout.DelayedIntField(_editor._captureFrame_SrcWidth, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			//"Height"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Height), GUILayout.Width(settingWidth_Label));
			int srcSizeHeight = EditorGUILayout.DelayedIntField(_editor._captureFrame_SrcHeight, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();


			if (srcSizeWidth < 8) { srcSizeWidth = 8; }
			if (srcSizeHeight < 8) { srcSizeHeight = 8; }

			EditorGUILayout.EndVertical();


			//------------------------

			//File Size
			//-------------------------------
			EditorGUILayout.BeginVertical(GUILayout.Width(settingWidth), GUILayout.Height(settingHeight));

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ImageSize), GUILayout.Width(settingWidth));//"Image Size"

			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			//"Width"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Width), GUILayout.Width(settingWidth_Label));
			int dstSizeWidth = EditorGUILayout.DelayedIntField(_editor._captureFrame_DstWidth, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(settingWidth));
			//"Height"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Height), GUILayout.Width(settingWidth_Label));
			int dstSizeHeight = EditorGUILayout.DelayedIntField(_editor._captureFrame_DstHeight, GUILayout.Width(settingWidth_Value));
			EditorGUILayout.EndHorizontal();

			if (dstSizeWidth < 8) { dstSizeWidth = 8; }
			if (dstSizeHeight < 8) { dstSizeHeight = 8; }



			EditorGUILayout.EndVertical();
			//-------------------------------

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			int setting2CompWidth = ((width - 10) / 2) - 8;

			//Color와 AspectRatio
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_BGColor), GUILayout.Width(80));//"BG Color"
			Color prevCaptureColor = _editor._captureFrame_Color;
			try
			{
				_editor._captureFrame_Color = EditorGUILayout.ColorField(_editor._captureFrame_Color, GUILayout.Width(setting2CompWidth - 86));
			}
			catch (Exception) { }


			GUILayout.Space(30);
			//"Aspect Ratio Fixed", "Aspect Ratio Not Fixed" -> "Fixed Aspect Ratio", "Not Fixed Aspect Ratio "
			if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_FixedAspectRatio), _editor.GetText(TEXT.DLG_NotFixedAspectRatio), _editor._isCaptureAspectRatioFixed, true, setting2CompWidth - 20, 20))
			{
				_editor._isCaptureAspectRatioFixed = !_editor._isCaptureAspectRatioFixed;

				if (_editor._isCaptureAspectRatioFixed)
				{
					//AspectRatio를 굳혔다.
					//Dst계열 변수를 Src에 맞춘다.
					//Height를 고정, Width를 맞춘다.
					_editor._captureFrame_DstWidth = GetAspectRatio_Width(_editor._captureFrame_DstHeight, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
					dstSizeWidth = _editor._captureFrame_DstWidth;
				}

				_editor.SaveEditorPref();


				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();


			//AspectRatio를 맞추어보자
			if (_editor._isCaptureAspectRatioFixed)
			{
				if (srcSizeWidth != _editor._captureFrame_SrcWidth)
				{
					//Width가 바뀌었다. => Height를 맞추자
					srcSizeHeight = GetAspectRatio_Height(srcSizeWidth, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
					//>> Dst도 바꾸자 => Width
					dstSizeWidth = GetAspectRatio_Width(dstSizeHeight, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
				}
				else if (srcSizeHeight != _editor._captureFrame_SrcHeight)
				{
					//Height가 바뀌었다. => Width를 맞추자
					srcSizeWidth = GetAspectRatio_Width(srcSizeHeight, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
					//>> Dst도 바꾸자 => Height
					dstSizeHeight = GetAspectRatio_Height(dstSizeWidth, _editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight);
				}
				else if (dstSizeWidth != _editor._captureFrame_DstWidth)
				{
					//Width가 바뀌었다. => Height를 맞추자
					dstSizeHeight = GetAspectRatio_Height(dstSizeWidth, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
					//>> Src도 바꾸다 => Width
					srcSizeWidth = GetAspectRatio_Width(srcSizeHeight, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
				}
				else if (dstSizeHeight != _editor._captureFrame_DstHeight)
				{
					//Height가 바뀌었다. => Width를 맞추자
					dstSizeWidth = GetAspectRatio_Width(dstSizeHeight, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
					//>> Dst도 바꾸자 => Height
					srcSizeHeight = GetAspectRatio_Height(srcSizeWidth, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);
				}
			}

			if (posX != _editor._captureFrame_PosX
				|| posY != _editor._captureFrame_PosY
				|| srcSizeWidth != _editor._captureFrame_SrcWidth
				|| srcSizeHeight != _editor._captureFrame_SrcHeight
				|| dstSizeWidth != _editor._captureFrame_DstWidth
				|| dstSizeHeight != _editor._captureFrame_DstHeight
				)
			{
				_editor._captureFrame_PosX = posX;
				_editor._captureFrame_PosY = posY;
				_editor._captureFrame_SrcWidth = srcSizeWidth;
				_editor._captureFrame_SrcHeight = srcSizeHeight;
				_editor._captureFrame_DstWidth = dstSizeWidth;
				_editor._captureFrame_DstHeight = dstSizeHeight;

				_editor.SaveEditorPref();
				apEditorUtil.ReleaseGUIFocus();
			}

			if (prevCaptureColor.r != _editor._captureFrame_Color.r
				|| prevCaptureColor.g != _editor._captureFrame_Color.g
				|| prevCaptureColor.b != _editor._captureFrame_Color.b)
			{
				_editor.SaveEditorPref();
				//색상은 GUIFocus를 null로 만들면 안되기에..
			}



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ThumbnailCapture));//"Thumbnail Capture"
			GUILayout.Space(5);

			string prev_ImageFilePath = _editor._portrait._imageFilePath_Thumbnail;

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_FilePath));//"File Path"
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(5);
			GUILayout.Box(_editor._portrait._thumbnailImage, GUI.skin.label, GUILayout.Width(50), GUILayout.Height(25));
			_editor._portrait._imageFilePath_Thumbnail = EditorGUILayout.TextField(_editor._portrait._imageFilePath_Thumbnail, GUILayout.Width(width - (130)));
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(60)))//"Change"
			{
				string fileName = EditorUtility.SaveFilePanelInProject("Thumbnail File Path", _editor._portrait.name + "_Thumb.png", "png", "Please Enter a file name to save Thumbnail to");
				if (!string.IsNullOrEmpty(fileName))
				{
					_editor._portrait._imageFilePath_Thumbnail = apUtil.ConvertEscapeToPlainText(fileName);//이스케이프 문자 삭제
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			EditorGUILayout.EndHorizontal();

			if(!_editor._portrait._imageFilePath_Thumbnail.Equals(prev_ImageFilePath))
			{
				//경로가 바뀌었다. -> 저장
				apEditorUtil.SetEditorDirty();
				
			}

			//"Make Thumbnail"
			//썸네일 만들기 버튼
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_MakeThumbnail), GUILayout.Width(width), GUILayout.Height(30)))
			{
				if (string.IsNullOrEmpty(_editor._portrait._imageFilePath_Thumbnail))
				{
					//EditorUtility.DisplayDialog("Thumbnail Creating Failed", "File Name is Empty", "Close");
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.ThumbCreateFailed_Title),
													_editor.GetText(TEXT.ThumbCreateFailed_Body_NoFile),
													_editor.GetText(TEXT.Close)
													);
				}
				else
				{
					//RequestExport(EXPORT_TYPE.Thumbnail);//<<이전 코드
					StartMakeThumbnail();//<<새로운 코드
					
				}
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//Screenshot을 찍자
			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ScreenshotCapture));//"Screenshot Capture"
			GUILayout.Space(5);
			//"Take a Screenshot"
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_TakeAScreenshot), GUILayout.Width(width), GUILayout.Height(30)))
			{
				//RequestExport(EXPORT_TYPE.PNG);//<<이전 코드
				StartTakeScreenShot();
			}

			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			//GIF Animation을 만들자
			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_GIFAnimation));//"GIF Animation"
			GUILayout.Space(5);

			apRootUnit curRootUnit = _editor.Select.RootUnit;

			if (_curRootUnit != curRootUnit)
			{
				//AnimList 리셋
				_animClips.Clear();

				_curRootUnit = curRootUnit;
				if (_curRootUnit != null)
				{
					for (int i = 0; i < _editor._portrait._animClips.Count; i++)
					{
						apAnimClip animClip = _editor._portrait._animClips[i];
						if (animClip._targetMeshGroup == _curRootUnit._childMeshGroup)
						{
							_animClips.Add(animClip);
						}
					}
				}

				_selectedAnimClip = null;
			}
			if (curRootUnit == null)
			{
				_selectedAnimClip = null;
			}
			else
			{
				if (_selectedAnimClip != null && _animClips.Count > 0)
				{
					if (!_animClips.Contains(_selectedAnimClip))
					{
						_selectedAnimClip = null;
					}
				}
				else
				{
					_selectedAnimClip = null;
				}
			}

			//string animName = "< Animation is not selected >";
			string animName = _editor.GetText(TEXT.DLG_NotAnimation);
			Color animBGColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
			if (_selectedAnimClip != null)
			{
				animName = _selectedAnimClip._name;
				animBGColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);
			}

			Color prevGUIColor = GUI.backgroundColor;
			GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
			guiStyleBox.alignment = TextAnchor.MiddleCenter;
			guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

			GUI.backgroundColor = animBGColor;

			GUILayout.Box(animName, guiStyleBox, GUILayout.Width(width), GUILayout.Height(30));

			GUI.backgroundColor = prevGUIColor;

			GUILayout.Space(5);
			int width_GIFSetting = (width - 32) / 2;

			//int gifQuality = 256 - _editor._captureFrame_GIFSampleQuality;
			int gifQuality = 128;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			string strQuality = "";
			if (gifQuality > 200)
			{
				//strQuality = "Quality [ High ]";
				strQuality = _editor.GetText(TEXT.DLG_QualityHigh);
			}
			else if (gifQuality > 120)
			{
				//strQuality = "Quality [ Medium ]";
				strQuality = _editor.GetText(TEXT.DLG_QualityMedium);
			}
			else
			{
				//strQuality = "Quality [ Low ]";
				strQuality = _editor.GetText(TEXT.DLG_QualityLow);
			}
			EditorGUILayout.LabelField(strQuality, GUILayout.Width(width_GIFSetting));
			GUILayout.Space(20);

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_LoopCount), GUILayout.Width(width_GIFSetting));//"Loop Count"

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			//10 ~ 256
			//246 ~ 0
			gifQuality = EditorGUILayout.IntSlider(gifQuality, 0, 246, GUILayout.Width(width_GIFSetting));

			gifQuality = 256 - gifQuality;
			//if (_editor._captureFrame_GIFSampleQuality != gifQuality)
			//{
			//	_editor._captureFrame_GIFSampleQuality = gifQuality;
			//	_editor.SaveEditorPref();
			//}

			GUILayout.Space(20);

			int loopCount = EditorGUILayout.DelayedIntField(_editor._captureFrame_GIFSampleLoopCount, GUILayout.Width(width_GIFSetting));

			if (loopCount != _editor._captureFrame_GIFSampleLoopCount)
			{
				loopCount = Mathf.Clamp(loopCount, 1, 10);
				_editor._captureFrame_GIFSampleLoopCount = loopCount;
				_editor.SaveEditorPref();
			}

			EditorGUILayout.EndHorizontal();

			//GUILayout.Space(10);

			//Rect lastRect_Progress = GUILayoutUtility.GetLastRect();

			//Rect barRect = new Rect(lastRect_Progress.x + 5, lastRect_Progress.y + 10, width - 5, 16);
			//float barRatio = 0.0f;
			//string strProcessName = "";
			//if(_exportProcessType == EXPORT_TYPE.GIFAnimation)
			//{
			//	barRatio = Mathf.Clamp01((float)_exportProcessX100 / 100.0f);
			//	strProcessName = "Exporting.. [ " + _exportProcessX100 + "% ]";
			//}

			////EditorGUI.ProgressBar(barRect, barRatio, "Convert PSD Data To Editor..");

			//EditorGUI.ProgressBar(barRect, barRatio, strProcessName);
			//GUILayout.Space(20);


			string strTakeAGIFAnimation = _editor.GetText(TEXT.DLG_TakeAGIFAnimation);
			//"Take a GIF Animation", "Take a GIF Animation"
			if (apEditorUtil.ToggledButton_2Side(strTakeAGIFAnimation, strTakeAGIFAnimation, false, (_selectedAnimClip != null), width, 30))
			{
				//RequestExport(EXPORT_TYPE.GIFAnimation);//리퀘스트 안할래..

				
				//string defFileName = "GIF_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".gif";
				//string saveFilePath = EditorUtility.SaveFilePanel("Save GIF Animation", _prevFilePath_Directory, defFileName, "gif");
				//if (!string.IsNullOrEmpty(saveFilePath))
				//{
				//	bool result = _editor.Exporter.MakeGIFAnimation(saveFilePath,
				//														_editor.Select.RootUnit._childMeshGroup,
				//														_selectedAnimClip, _editor._captureFrame_GIFSampleLoopCount,
				//														(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
				//														_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
				//														_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
				//														_editor._captureFrame_Color,
				//														_editor._captureFrame_GIFSampleQuality
				//													);
				//	if (result)
				//	{
				//		System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);

				//		Application.OpenURL("file://" + fi.Directory.FullName);
				//		Application.OpenURL("file://" + saveFilePath);

				//		_prevFilePath = _editor.Exporter.GIF_FilePath;
				//		_prevFilePath_Directory = fi.Directory.FullName;
				//	}
				//}

				StartGIFAnimation();
			}

			GUILayout.Space(10);

			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if(EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}
			
			//"Animation Clips"
			GUILayout.Button(_editor.GetText(TEXT.DLG_AnimationClips), guiStyle_None, GUILayout.Width(width), GUILayout.Height(20));//투명 버튼


			//애니메이션 클립 리스트를 만들어야 한다.
			if (_animClips.Count > 0)
			{

				Texture2D iconImage = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

				apAnimClip nextSelectedAnimClip = null;
				for (int i = 0; i < _animClips.Count; i++)
				{
					GUIStyle curGUIStyle = guiStyle_None;

					apAnimClip animClip = _animClips[i];

					if (animClip == _selectedAnimClip)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();

						#region [미사용 코드]
						//prevGUIColor = GUI.backgroundColor;

						//if(EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}

						//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
						//GUI.backgroundColor = prevGUIColor; 
						#endregion

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 20, width - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);


						curGUIStyle = guiStyle_Selected;
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
					GUILayout.Space(15);
					if (GUILayout.Button(new GUIContent(" " + animClip._name, iconImage), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
					{
						nextSelectedAnimClip = animClip;
					}

					EditorGUILayout.EndHorizontal();
				}

				if (nextSelectedAnimClip != null)
				{
					for (int i = 0; i < _editor._portrait._animClips.Count; i++)
					{
						_editor._portrait._animClips[i]._isSelectedInEditor = false;
					}

					nextSelectedAnimClip.LinkEditor(_editor._portrait);
					nextSelectedAnimClip.RefreshTimelines(null, null);
					nextSelectedAnimClip.SetFrame_Editor(nextSelectedAnimClip.StartFrame);
					nextSelectedAnimClip.Pause_Editor();
					nextSelectedAnimClip._isSelectedInEditor = true;
					_selectedAnimClip = nextSelectedAnimClip;

					_editor._portrait._animPlayManager.SetAnimClip_Editor(_selectedAnimClip);
				}
			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(500);

			EditorGUILayout.EndScrollView();


			//-----------------------------------------------------------------------------------------------------------------------
			//-----------------------------------------------------------------------------------------------------------------------
		}


		private int GetAspectRatio_Height(int srcWidth, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect
			//H = W / Aspect <<

			return (int)(((float)srcWidth / targetAspectRatio) + 0.5f);
		}

		private int GetAspectRatio_Width(int srcHeight, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect <<
			//H = W / Aspect

			return (int)(((float)srcHeight * targetAspectRatio) + 0.5f);
		}


		//-------------------------------------------------------------------------------------------------
		// Request / Response
		//-------------------------------------------------------------------------------------------------
		// 1. Make Thumbnail
		private void StartMakeThumbnail()
		{
			_captureMode = CAPTURE_MODE.Capturing_Thumbnail;

			//썸네일 크기
			int thumbnailWidth = 256;
			int thumbnailHeight = 128;

			float preferAspectRatio = (float)thumbnailWidth / (float)thumbnailHeight;

			float srcAspectRatio = (float)_editor._captureFrame_SrcWidth / (float)_editor._captureFrame_SrcHeight;
			//긴쪽으로 캡쳐 크기를 맞춘다.
			int srcThumbWidth = _editor._captureFrame_SrcWidth;
			int srcThumbHeight = _editor._captureFrame_SrcHeight;

			//AspectRatio = W / H
			if (srcAspectRatio < preferAspectRatio)
			{
				//가로가 더 길군요.
				//가로를 자릅시다.

				//H = W / AspectRatio;
				srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
			}
			else
			{
				//세로가 더 길군요.
				//세로를 자릅시다.
				//W = AspectRatio * H
				srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
			}

			
			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeScreenShot(OnThumbnailCaptured,
														_editor,
														_editor.Select.RootUnit._childMeshGroup,
														(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), 
														(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
														srcThumbWidth, srcThumbHeight,
														thumbnailWidth, thumbnailHeight,
														_editor._scroll_CenterWorkSpace, _editor._iZoomX100,
														_editor._captureFrame_Color, 0, "");

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			_editor.ScreenCaptureRequest(newRequest);
			_editor.SetRepaint();
			
		}

		private void OnThumbnailCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			_captureMode = CAPTURE_MODE.None;

			//Debug.Log("OnThumbnailCaptured / isSuccess : " + isSuccess + " / Image : " + (captureImage != null) + " / Step : " + iProcessStep);

			//우왕 왔당
			if (!isSuccess || captureImage == null)
			{
				//Debug.LogError("Failed..");
				if(captureImage != null)
				{
					DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				
				return;
			}
			if(_captureLoadKey != loadKey)
			{
				//Debug.LogError("LoadKey Mismatched");

				if(captureImage != null)
				{
					DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				return;
			}

			//이제 처리합시당 (Destroy도 포함되어있다)
			string filePathWOExtension = _editor._portrait._imageFilePath_Thumbnail.Substring(0, _editor._portrait._imageFilePath_Thumbnail.Length - 4);
			bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(captureImage, filePathWOExtension, true);

			if (isSaveSuccess)
			{
				AssetDatabase.Refresh();

				_editor._portrait._thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(_editor._portrait._imageFilePath_Thumbnail);
			}
		}



		//private bool RequestExport(EXPORT_TYPE exportType)
		//{
		//	if (_exportProcessType != EXPORT_TYPE.None || _exportRequestType != EXPORT_TYPE.None)
		//	{
		//		return false;
		//	}

		//	_exportRequestType = exportType;
		//	_exportProcessType = EXPORT_TYPE.None;
		//	return true;
		//}

		#region [미사용 코드]
		//private void Process_MakeThumbnail()
		//{
		//	//if (curEvent.type == EventType.Repaint)
		//	//{
		//	//	return;
		//	//}

		//	try
		//	{
		//		int thumbnailWidth = 256;
		//		int thumbnailHeight = 128;

		//		float preferAspectRatio = (float)thumbnailWidth / (float)thumbnailHeight;

		//		float srcAspectRatio = (float)_editor._captureFrame_SrcWidth / (float)_editor._captureFrame_SrcHeight;
		//		//긴쪽으로 캡쳐 크기를 맞춘다.
		//		int srcThumbWidth = _editor._captureFrame_SrcWidth;
		//		int srcThumbHeight = _editor._captureFrame_SrcHeight;
		//		//AspectRatio = W / H
		//		if (srcAspectRatio < preferAspectRatio)
		//		{
		//			//가로가 더 길군요.
		//			//가로를 자릅시다.

		//			//H = W / AspectRatio;
		//			srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
		//		}
		//		else
		//		{
		//			//세로가 더 길군요.
		//			//세로를 자릅시다.
		//			//W = AspectRatio * H
		//			srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
		//		}

		//		//_editor.Focus();
		//		Texture2D result = _editor.Exporter.RenderToTexture(_editor.Select.RootUnit._childMeshGroup,
		//														(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
		//														srcThumbWidth, srcThumbHeight,
		//														thumbnailWidth, thumbnailHeight,
		//														_editor._captureFrame_Color
		//														);

		//		//this.Focus();
		//		if (result != null)
		//		{
		//			//이미지를 저장하자
		//			//이건 Asset으로 자동 저장
		//			string filePathWOExtension = _editor._portrait._imageFilePath_Thumbnail.Substring(0, _editor._portrait._imageFilePath_Thumbnail.Length - 4);
		//			bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(result, filePathWOExtension, true);

		//			if (isSaveSuccess)
		//			{
		//				AssetDatabase.Refresh();

		//				_editor._portrait._thumbnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(_editor._portrait._imageFilePath_Thumbnail);
		//			}
		//		}

		//		//_exportRequestType = EXPORT_TYPE.None;
		//		//_exportProcessType = EXPORT_TYPE.None;//<<끝
		//		//_exportProcessX100 = 0;
		//		//_iProcess = 0;
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogError("Make Thumbnail Exception : " + ex);
		//	}
		//} 
		#endregion




		// 2. PNG 스크린샷
		private void StartTakeScreenShot()
		{
			try
			{
				string defFileName = "ScreenShot_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				string saveFilePath = EditorUtility.SaveFilePanel("Save Screenshot as PNG", _prevFilePath_Directory, defFileName, "png");
				if (!string.IsNullOrEmpty(saveFilePath))
				{
					_captureMode = CAPTURE_MODE.Capturing_ScreenShot;

					//Request를 만든다.
					apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
					_captureLoadKey = newRequest.MakeScreenShot(OnScreeenShotCaptured,
																_editor,
																_editor.Select.RootUnit._childMeshGroup,
																(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), 
																(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
																_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
																_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
																_editor._scroll_CenterWorkSpace, _editor._iZoomX100,
																_editor._captureFrame_Color, 0, saveFilePath);

					//에디터에 대신 렌더링해달라고 요청을 합시다.
					_editor.ScreenCaptureRequest(newRequest);
					_editor.SetRepaint();
				}
			}
			catch (Exception)
			{
				
			}
		}


		private void OnScreeenShotCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			_captureMode = CAPTURE_MODE.None;

			//우왕 왔당
			if (!isSuccess || captureImage == null || string.IsNullOrEmpty(filePath))
			{
				//Debug.LogError("Failed..");
				if(captureImage != null)
				{
					DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				
				return;
			}
			if(_captureLoadKey != loadKey)
			{
				//Debug.LogError("LoadKey Mismatched");

				if(captureImage != null)
				{
					DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				return;
			}

			//이제 파일로 저장하자
			try
			{
				string filePathWOExtension = filePath.Substring(0, filePath.Length - 4);

				//AutoDestroy = true
				bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(captureImage, filePathWOExtension, true);

				if (isSaveSuccess)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(filePath);//파일 경로 체크됨 (사용하지 않는 코드) (21.9.10)

					Application.OpenURL("file://" + fi.Directory.FullName);
					Application.OpenURL("file://" + filePath);

					//_prevFilePath = filePath;
					_prevFilePath_Directory = fi.Directory.FullName;
				}
			}
			catch(Exception)
			{

			}
		}



		//3. GIF 애니메이션 만들기
		private void StartGIFAnimation()
		{
			if (_selectedAnimClip == null || _editor.Select.RootUnit._childMeshGroup == null)
			{
				return;
			}
			
			string defFileName = "GIF_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".gif";
			string saveFilePath = EditorUtility.SaveFilePanel("Save GIF Animation", _prevFilePath_Directory, defFileName, "gif");
			if (!string.IsNullOrEmpty(saveFilePath))
			{
				//애니메이션 정보를 저장한다.
				_isLoopAnimation = _selectedAnimClip.IsLoop;
				_isAnimFirstFrame = true;
				_startAnimFrame = _selectedAnimClip.StartFrame;
				_lastAnimFrame = _selectedAnimClip.EndFrame;

				_animLoopCount = _editor._captureFrame_GIFSampleLoopCount;
				if (_animLoopCount < 1)
				{
					_animLoopCount = 1;
				}

				//_gifAnimQuality = _editor._captureFrame_GIFSampleQuality;

				if (_isLoopAnimation)
				{
					_lastAnimFrame--;//루프인 경우 마지막 프레임은 제외
				}

				if (_lastAnimFrame < _startAnimFrame)
				{
					_lastAnimFrame = _startAnimFrame;
				}

				_editor._portrait._animPlayManager.Stop_Editor();
				_editor._portrait._animPlayManager.SetAnimClip_Editor(_selectedAnimClip);

				_curAnimLoop = 0;
				_curAnimFrame = _startAnimFrame;

				_curAnimProcess = 0;
				_totalAnimProcess = (Mathf.Abs(_lastAnimFrame - _startAnimFrame) + 1) * _animLoopCount;


				//1. GIF 헤더를 만들고
				//2. 이제 프레임을 하나씩 렌더링하기 시작하자

				_captureMode = CAPTURE_MODE.Capturing_GIF_Animation;

				//GIF 헤더
				bool isHeaderResult = _editor.Exporter.MakeGIFHeader(saveFilePath, _selectedAnimClip, _editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight);

				if(!isHeaderResult)
				{
					//실패한 경우
					_captureMode = CAPTURE_MODE.None;
					return;
				}


				//첫번째 프레임
				//Request를 만든다.
				apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
				_captureLoadKey = newRequest.MakeAnimCapture(OnGIFFrameCaptured,
															_editor,
															_editor.Select.RootUnit._childMeshGroup,
															//true,
															_selectedAnimClip, _curAnimFrame,
															(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x),
															(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
															_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
															_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
															_editor._scroll_CenterWorkSpace, _editor._iZoomX100,
															_editor._captureFrame_Color, 
															_editor._captureFrame_IsPhysics,
															_curAnimProcess, saveFilePath);

				//에디터에 대신 렌더링해달라고 요청을 합시다.
				_editor.ScreenCaptureRequest(newRequest);
				_editor.SetRepaint();
			}
		}


		private void OnGIFFrameCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{

			//_captureMode = CAPTURE_MODE.None;

			//우왕 왔당
			if (!isSuccess
				|| captureImage == null
				|| string.IsNullOrEmpty(filePath)
				|| _captureMode != CAPTURE_MODE.Capturing_GIF_Animation
				|| _captureLoadKey != loadKey)
			{
				//Debug.LogError("프레임 오류");
				//Debug.LogError("Failed..");
				if (captureImage != null)
				{
					DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				//오류가 났다
				_editor.Exporter.EndGIF();
				_captureMode = CAPTURE_MODE.None;
				return;
			}

			_captureLoadKey = null;

			//이미지를 GIF 프레임으로 하나씩 넣자
			bool addFrameResult = _editor.Exporter.AddGIFFrame(captureImage, _isAnimFirstFrame, _gifAnimQuality);

			if (!addFrameResult)
			{
				//으잉 실패...
				if (captureImage != null)
				{
					DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				//오류가 났다
				_editor.Exporter.EndGIF();
				_captureMode = CAPTURE_MODE.None;
				return;
			}

			//이제 프레임 하나 증가
			_curAnimFrame++;
			_curAnimProcess++;
			_isAnimFirstFrame = false;

			if (_curAnimFrame > _lastAnimFrame)
			{
				//끝까지 갔네염
				//루프 카운트 증가
				_curAnimFrame = _startAnimFrame;
				_curAnimLoop++;

				if (_curAnimLoop >= _animLoopCount)
				{
					//으잉 루프 카운트가 끝났다.

					//끄으읕
					if (captureImage != null)
					{
						DestroyImmediate(captureImage);
					}
					_captureLoadKey = null;

					_editor.Exporter.EndGIF();
					_captureMode = CAPTURE_MODE.None;

					//완성된 파일을 열자
					System.IO.FileInfo fi = new System.IO.FileInfo(filePath);//파일 경로 체크됨 (사용되지 않는 코드) (21.9.10)

					Application.OpenURL("file://" + fi.Directory.FullName);
					Application.OpenURL("file://" + filePath);

					//_prevFilePath = filePath;
					_prevFilePath_Directory = fi.Directory.FullName;
					return;
				}
			}

			
			//다음 프레임을 렌더링하자
			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeAnimCapture(OnGIFFrameCaptured,
														_editor,
														_editor.Select.RootUnit._childMeshGroup,
														//true,
														_selectedAnimClip, _curAnimFrame,
														(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x),
														(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
														_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
														_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
														_editor._scroll_CenterWorkSpace, _editor._iZoomX100,
														_editor._captureFrame_Color, 
														_editor._captureFrame_IsPhysics,
														_curAnimProcess, filePath);

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			_editor.ScreenCaptureRequest(newRequest);
			_editor.SetRepaint();
			Repaint();

		}
		

		#region [미사용 코드]
		//private void Process_PNGScreenShot()
		//{
		//	try
		//	{
		//		string defFileName = "ScreenShot_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
		//		string saveFilePath = EditorUtility.SaveFilePanel("Save Screenshot as PNG", _prevFilePath_Directory, defFileName, "png");
		//		if (!string.IsNullOrEmpty(saveFilePath))
		//		{
		//			Texture2D result = _editor.Exporter.RenderToTexture(_editor.Select.RootUnit._childMeshGroup,
		//															(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
		//															_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
		//															_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
		//															_editor._captureFrame_Color
		//															);

		//			if (result != null)
		//			{
		//				//이미지를 저장하자
		//				//이건 Asset으로 자동 저장
		//				string filePathWOExtension = saveFilePath.Substring(0, saveFilePath.Length - 4);

		//				bool isSaveSuccess = _editor.Exporter.SaveTexture2DToPNG(result, filePathWOExtension, true);

		//				System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);

		//				Application.OpenURL("file://" + fi.Directory.FullName);
		//				Application.OpenURL("file://" + saveFilePath);

		//				_prevFilePath = saveFilePath;
		//				_prevFilePath_Directory = fi.Directory.FullName;
		//			}


		//		}

		//		//_exportRequestType = EXPORT_TYPE.None;
		//		//_exportProcessType = EXPORT_TYPE.None;//<<끝
		//		//_exportProcessX100 = 0;
		//		//_iProcess = 0;
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogError("PNG Screenshot Exception : " + ex);
		//	}
		//}

		//private void Process_MakeGIF()
		//{
		//	if (_iProcess == 0)
		//	{
		//		string defFileName = "GIF_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".gif";
		//		string saveFilePath = EditorUtility.SaveFilePanel("Save GIF Animation", _prevFilePath_Directory, defFileName, "gif");
		//		if (!string.IsNullOrEmpty(saveFilePath))
		//		{
		//			int result = _editor.Exporter.MakeGIFAnimation_Ready(saveFilePath,
		//																_editor.Select.RootUnit._childMeshGroup,
		//																_selectedAnimClip, _editor._captureFrame_GIFSampleLoopCount,
		//																(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x), (int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
		//																_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
		//																_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
		//																_editor._captureFrame_Color,
		//																_editor._captureFrame_GIFSampleQuality
		//															);

		//			if (result < 0)
		//			{
		//				//EditorUtility.DisplayDialog("Make GIF Creating Failed", "Request is rejected", "Close");
		//				EditorUtility.DisplayDialog(_editor.GetText(TEXT.GIFFailed_Title),
		//												_editor.GetText(TEXT.GIFFailed_Body_Reject),
		//												_editor.GetText(TEXT.Close));

		//				_exportProcessType = EXPORT_TYPE.None;
		//				_exportRequestType = EXPORT_TYPE.None;
		//				_exportProcessX100 = 0;
		//				return;
		//			}

		//			_iProcessCount = result;

		//		}
		//	}

		//	float processRatio = _editor.Exporter.MakeGIFAnimation_Step(_iProcess);
		//	if (processRatio < 0.0f)
		//	{
		//		//EditorUtility.DisplayDialog("Make GIF Creating Failed", "Request is rejected", "Close");

		//		EditorUtility.DisplayDialog(_editor.GetText(TEXT.GIFFailed_Title),
		//										_editor.GetText(TEXT.GIFFailed_Body_Reject),
		//										_editor.GetText(TEXT.Close));


		//		_exportProcessType = EXPORT_TYPE.None;
		//		_exportRequestType = EXPORT_TYPE.None;
		//		_exportProcessX100 = 0;
		//		return;
		//	}

		//	_exportProcessX100 = (int)(processRatio + 0.5f);

		//	_iProcess++;
		//	if (_iProcess >= _iProcessCount)
		//	{
		//		_exportProcessX100 = 100;
		//		_exportProcessType = EXPORT_TYPE.None;
		//		_exportRequestType = EXPORT_TYPE.None;

		//		System.IO.FileInfo fi = new System.IO.FileInfo(_editor.Exporter.GIF_FilePath);

		//		Application.OpenURL("file://" + fi.Directory.FullName);
		//		Application.OpenURL("file://" + _editor.Exporter.GIF_FilePath);

		//		_prevFilePath = _editor.Exporter.GIF_FilePath;
		//		_prevFilePath_Directory = fi.Directory.FullName;
		//	}
		//} 
		#endregion
	}

}