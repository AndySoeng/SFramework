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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AnyPortrait;

namespace AnyPortrait
{
	//RenderTexture를 하는 리퀘스트
	//Dialog_CaptureScreen -> Editor로 전달
	//렌더링 처리용 파라미터를 가지고 있으며 콜백 함수로 리턴
	//이 리퀘스트를 받은 Editor는 추가적인 렌더링 처리를 한다. (Repaint 이벤트에서)
	//순차적인 처리도 가능하므로 Step 리턴도 넣어주자
	public class apScreenCaptureRequest
	{
		// Callback Functions
		//----------------------------------------------------
		public delegate void FUNC_CAPTURE_RESULT(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey);


		// Members
		//----------------------------------------------------
		//결과
		public FUNC_CAPTURE_RESULT _funcCaptureResult = null;


		//에디터 윈도우
		public apEditor _editor = null;
		//public apDialog_CaptureScreen _dialogCaptureScreen = null;

		//타겟
		public apMeshGroup _meshGroup = null;

		public bool _isAnimClipRequest = false;
		public apAnimClip _animClip = null;
		public int _animFrame = -1;

		//캡쳐 당시 화면 스크롤
		public Vector2 _screenPosition = Vector2.zero;
		public int _screenZoomIndex = 0;

		//캡쳐 영역
		public int _winPosX;
		public int _winPosY;
		public int _srcSizeWidth;
		public int _srcSizeHeight;
		public int _dstSizeWidth;
		public int _dstSizeHeight;
		public Color _clearColor;
		public bool _isPhysics;

		//파일 경로
		public string _filePath = "";

		//로드키
		public object _loadKey = null;

		public int _iProcessStep = -1;

		
		
		// Init
		//----------------------------------------------------
		public apScreenCaptureRequest()
		{

		}

		public object MakeScreenShot(	FUNC_CAPTURE_RESULT funcCaptureResult,
										apEditor editor,
										apMeshGroup meshGroup,
										int winPosX,
										int winPosY,
										int srcSizeWidth,
										int srcSizeHeight,
										int dstSizeWidth,
										int dstSizeHeight,
										Vector2 screenPosition,
										int screenZoomIndex,
										Color clearColor,
										int iProcessStep,
										string filePath)
		{
			_funcCaptureResult = funcCaptureResult;
			_editor = editor;
			_meshGroup = meshGroup;
			_winPosX = winPosX;
			_winPosY = winPosY;
			_srcSizeWidth = srcSizeWidth;
			_srcSizeHeight = srcSizeHeight;
			_dstSizeWidth = dstSizeWidth;
			_dstSizeHeight = dstSizeHeight;
			_clearColor = clearColor;
			_iProcessStep = iProcessStep;
			_filePath = filePath;

			_isAnimClipRequest = false;
			_animClip = null;
			_animFrame = -1;

			_isPhysics = false;

			_screenPosition = screenPosition;
			_screenZoomIndex = screenZoomIndex;
			

			//로드키
			_loadKey = new object();

			return _loadKey;
		}

		public object MakeAnimCapture(	FUNC_CAPTURE_RESULT funcCaptureResult,
										apEditor editor,
										apMeshGroup meshGroup,
										//bool isAnimClipRequest,
										apAnimClip animClip,
										int animFrame,
										int winPosX,
										int winPosY,
										int srcSizeWidth,
										int srcSizeHeight,
										int dstSizeWidth,
										int dstSizeHeight,
										Vector2 screenPosition,
										int screenZoomIndex,
										Color clearColor,
										bool isPhysics,
										int iProcessStep,
										string filePath)
		{
			_funcCaptureResult = funcCaptureResult;
			_editor = editor;
			_meshGroup = meshGroup;
			_winPosX = winPosX;
			_winPosY = winPosY;
			_srcSizeWidth = srcSizeWidth;
			_srcSizeHeight = srcSizeHeight;
			_dstSizeWidth = dstSizeWidth;
			_dstSizeHeight = dstSizeHeight;
			_clearColor = clearColor;
			_iProcessStep = iProcessStep;
			_filePath = filePath;

			//_isAnimClipRequest = isAnimClipRequest;
			_isAnimClipRequest = true;
			_animClip = animClip;
			_animFrame = animFrame;

			_isPhysics = isPhysics;

			_screenPosition = screenPosition;
			_screenZoomIndex = screenZoomIndex;
			
			//로드키
			_loadKey = new object();

			return _loadKey;
		}
	}
}