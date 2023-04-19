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
using System.IO;

using AnyPortrait;
using System.Xml;

namespace AnyPortrait
{
	/// <summary>
	/// Editor에 포함되어서 Export와 같이 이미지 캡쳐를 담당한다.
	/// 연속 이미지(GIF, AnimClip)를 캡쳐할 때에는 이 클래스를 이용한다.
	/// Editor, Selection에서는 이 클래스가 작동 중인 정보를 UI에 출력한다.
	/// 화면 스크롤 정보도 가지고 있는다.
	/// </summary>
	public class apSequenceExporter
	{
		// Members
		//---------------------------------------------------------
		private apEditor _editor = null;

		public enum SEQUENCE_MODE
		{
			None,
			GIF,
			MP4,
			Spritesheet_Capture,
			Spritesheet_Pack
		}
		private SEQUENCE_MODE _sequenceMode = SEQUENCE_MODE.None;
		private bool _isRequestStop = false;

		//공통
		private apRootUnit _targetRootUnit = null;
		private Vector2 _frameScrollPosition = Vector2.zero;
		private int _frameZoomIndex = 0;
		
		private int _srcPosX = 0;
		private int _srcPosY = 0;
		private int _srcWidth = 0;
		private int _srcHeight = 0;
		private int _dstWidth = 0;
		private int _dstHeight = 0;
		private int _spriteUnitWidth = 0;
		private int _spriteUnitHeight = 0;
		private int _spriteImageWidth = 0;
		private int _spriteImageHeight = 0;
		private Color _bgColor = Color.black;
		private string _saveFilePath = "";

		private object _captureLoadKey = null;

		//GIF 애니메이션의 절차적 처리를 위한 변수
		private apAnimClip _GIF_animClip = null;
		private bool _GIF_IsLoopAnimation = false;
		private bool _GIF_IsAnimFirstFrame = false;
		private int _GIF_CurAnimFrame = 0;
		private int _GIF_StartAnimFrame = 0;
		private int _GIF_LastAnimFrame = 0;
		private int _GIF_CurAnimLoop = 0;
		private int _GIF_AnimLoopCount = 0;
		private int _GIF_CurAnimProcess = 0;
		private int _GIF_TotalAnimProcess = 0;
		private int _GIF_GifAnimQuality = 0;


		//MP4 애니메이션의 절차적 처리를 위한 변수
#if UNITY_2017_4_OR_NEWER
		private apAnimClip _MP4_animClip = null;

		private bool _MP4_IsLoopAnimation = false;

		private int _MP4_CurAnimFrame = 0;
		private int _MP4_StartAnimFrame = 0;
		private int _MP4_LastAnimFrame = 0;
		private int _MP4_CurAnimLoop = 0;
		private int _MP4_AnimLoopCount = 0;
		private int _MP4_CurAnimProcess = 0;
		private int _MP4_TotalAnimProcess = 0;
#endif		
		

		//SpriteSheet 절차적 처리를 위한 변수
		private List<apAnimClip> _Sprite_AnimClips = new List<apAnimClip>();
		private apAnimClip _Sprite_CurAnimClip = null;
		private int _Sprite_CurAnimClipIndex = 0;
		private int _Sprite_CurFrame = 0;
		private int _Sprite_StartFrame = 0;
		private int _Sprite_LastFrame = 0;
		private bool _Sprite_IsLoopAnimation = false;
		private int _Sprite_TotalAnimFrames = 0;
		private int _Sprite_CurAnimFrameOnTotal = 0;
		private bool _Sprite_IsSizeCompressed = false;
		private bool _Sprite_IsSequenceFiles = false;
		private string _Sprite_FirstSequenceFilePath = "";
		private bool _Sprite_IsFirstSequenceFile = false;

		private int _Sprite_Margin = 0;
		private bool _Sprite_IsMeta_XML = false;
		private bool _Sprite_IsMeta_JSON = false;
		private bool _Sprite_IsMeta_TXT = false;

		public delegate void FUNC_SEQUENCE_RESULT(bool isSuccess);
		private FUNC_SEQUENCE_RESULT _funcSequenceResut = null;


		private class SpriteUnit
		{
			public Texture2D _frameImage = null;
			public apAnimClip _animClip = null;
			public int _frameIndex = 0;

			//리사이즈 전
			public int _width = 0;
			public int _height = 0;
			public int _centerX = 0;
			public int _centerY = 0;
			//리사이즈 후
			public int _compOffset_Left = 0;
			public int _compOffset_Right = 0;
			public int _compOffset_Top = 0;
			public int _compOffset_Bottom = 0;
			//배치
			public bool _isBaked = false;
			public int _bakeTextureIndex = -1;
			public int _bakePosX = 0;
			public int _bakePosY = 0;
			public int _bakeWidth = 0;
			public int _bakeHeight = 0;

			private int _minX = 0;
			private int _maxX = 0;
			private int _minY = 0;
			private int _maxY = 0;

			//시퀸스 파일인 경우
			public string _sequenceFileName = "";
			

			public SpriteUnit()
			{
				_frameImage = null;
				_animClip = null;
				_frameIndex = 0;

				_width = 0;
				_height = 0;
				_centerX = 0;
				_centerY = 0;
			
				_compOffset_Left = 0;
				_compOffset_Right = 0;
				_compOffset_Top = 0;
				_compOffset_Bottom = 0;

				_isBaked = false;
				_bakeTextureIndex = -1;
				_bakePosX = 0;
				_bakePosY = 0;
				_bakeWidth = 0;
				_bakeHeight = 0;
			}

			public void SetTexture(Texture2D frameImage, apAnimClip animClip, int frameIndex, int width, int height, int centerX, int centerY)
			{
				_frameImage = frameImage;
				_animClip = animClip;
				_frameIndex = frameIndex;

				_width = width;
				_height = height;
				_centerX = centerX;
				_centerY = centerY;

				_compOffset_Left = 0;
				_compOffset_Right = 0;
				_compOffset_Top = 0;
				_compOffset_Bottom = 0;

				_bakeWidth = _width;
				_bakeHeight = _height;
			}

			public void SetSequenceFileName(string fileName)
			{
				_sequenceFileName = fileName;
			}

			public void Optimize(Color clearColor)
			{
				_compOffset_Left = 0;
				_compOffset_Right = 0;
				_compOffset_Top = 0;
				_compOffset_Bottom = 0;

				_bakeWidth = _width;
				_bakeHeight = _height;
				
				
				_minX = _bakeWidth;
				_maxX = 0;
				_minY = _bakeHeight;
				_maxY = 0;

				//Step 0
				//전체를 1/10씩 체크하여 Clear Color와 다른 곳을 찾는다.
				bool isAnyValidColor = CheckColors(0, _bakeWidth, 0, _bakeHeight, 10, clearColor);
				

				if(!isAnyValidColor)
				{
					//못찾았다.
					//압축 가능한 최소 크기는 XY로 1/2
					_minX = (int)((float)_bakeWidth * 0.25f);
					_maxX = (int)((float)_bakeWidth * 0.75f);
					_minY = (int)((float)_bakeHeight * 0.25f);
					_maxY = (int)((float)_bakeHeight * 0.75f);
				}

				//이제 3x3 분할로 나눠가면서 체크를 해야한다.
				//중심에서는 체크를 하지 않는다.
				//각각 0 ~ minXY / minXY ~ maxXY / maxXY ~ WidthHeight
				//분할 크기는 기본 1/5
				//만약 변동이 없었다면 분할 크기가 2배로 늘어난다.
				//분할 크기가 20 이상이거나 Step이 5이 넘어가면 나중엔 2픽셀 단위로 체크를 한다.

				int pickScale = 5;

				int curMinX = _minX;
				int curMaxX = _maxX;
				int curMinY = _minY;
				int curMaxY = _maxY;
				for (int iStep = 0; iStep < 5; iStep++)
				{
					//0		minX		maxX		Width
					//
					//minY
					//
					//maxY
					//
					//Height

					curMinX = _minX;
					curMaxX = _maxX;
					curMinY = _minY;
					curMaxY = _maxY;

					isAnyValidColor = false;

					//1) LT
					if(CheckColors(0, curMinX, 0, curMinY, pickScale, clearColor)) { isAnyValidColor = true; }

					//2) T
					if(CheckColors(curMinX, curMaxX, 0, curMinY, pickScale, clearColor)) { isAnyValidColor = true; }

					//3) RT
					if(CheckColors(curMaxX, _bakeWidth, 0, curMinY, pickScale, clearColor)) { isAnyValidColor = true; }

					//4) L
					if(CheckColors(0, curMinX, curMinY, curMaxY, pickScale, clearColor)) { isAnyValidColor = true; }

					//5) R
					if(CheckColors(curMaxX, _bakeWidth, curMinY, curMaxY, pickScale, clearColor)) { isAnyValidColor = true; }

					//6) LT
					if(CheckColors(0, curMinX, curMaxY, _bakeHeight, pickScale, clearColor)) { isAnyValidColor = true; }

					//7) T
					if(CheckColors(curMinX, curMaxX, curMaxY, _bakeHeight, pickScale, clearColor)) { isAnyValidColor = true; }

					//8) RT
					if(CheckColors(curMaxX, _bakeWidth, curMaxY, _bakeHeight, pickScale, clearColor)) { isAnyValidColor = true; }

					if(!isAnyValidColor)
					{
						pickScale *= 2;
					}

					if(pickScale > 20)
					{
						//너무 세세하게 잡으면 나간다.
						//굳이 더 체크할 필요가 없다.
						break;
					}

				}
				
				//마지막 8공간 체크

				curMinX = _minX;
				curMaxX = _maxX;
				curMinY = _minY;
				curMaxY = _maxY;
				CheckColorsByPixel2(0,			curMinX,	0,			curMinY,		clearColor);
				CheckColorsByPixel2(curMinX,	curMaxX,	0,			curMinY,		clearColor);
				CheckColorsByPixel2(curMaxX,	_bakeWidth, 0,			curMinY,		clearColor);
				CheckColorsByPixel2(0,			curMinX,	curMinY,	curMaxY,		clearColor);
				CheckColorsByPixel2(curMinX,	curMaxX,	curMinY,	curMaxY,		clearColor);
				CheckColorsByPixel2(curMaxX,	_bakeWidth, curMinY,	curMaxY,		clearColor);
				CheckColorsByPixel2(0,			curMinX,	curMaxY,	_bakeHeight,	clearColor);
				CheckColorsByPixel2(curMinX,	curMaxX,	curMaxY,	_bakeHeight,	clearColor);
				CheckColorsByPixel2(curMaxX,	_bakeWidth, curMaxY,	_bakeHeight,	clearColor);

				//여유있게 각각 2픽셀 더 벌리자
				_minX -= 2;
				_maxX += 2;
				_minY -= 2;
				_maxY += 2;

				if(_minX < 0) { _minX = 0; }
				if(_maxX > _bakeWidth) { _maxX = _bakeWidth; }
				if(_minY < 0) { _minY = 0; }
				if(_maxY > _bakeHeight) { _maxY = _bakeHeight; }

				_compOffset_Left = _minX;
				_compOffset_Right = _bakeWidth - _maxX;
				_compOffset_Bottom = _minY;
				_compOffset_Top = _bakeHeight - _maxY;
			}

			public void SetOptimizedSize(int compOffset_X, int compOffset_Y)
			{
				_compOffset_Left = compOffset_X;
				_compOffset_Right = compOffset_X;
				_compOffset_Top = compOffset_Y;
				_compOffset_Bottom = compOffset_Y;

				_bakeWidth = _width - (_compOffset_Left + _compOffset_Right);
				_bakeHeight = _height - (_compOffset_Top + _compOffset_Bottom);
			}

			public void SetBakeOption(int textureIndex, int bakeX, int bakeY)
			{
				_isBaked = true;
				_bakeTextureIndex = textureIndex;
				_bakePosX = bakeX;
				_bakePosY = bakeY;
				
			}

			//두 색이 다르면 True
			private bool IsValidColor(Color color, Color clearColor)
			{
				if(clearColor.a < 0.008f)
				{
					//Alpha만 비교하자
					return (color.a > 0.008f);
				}
				int iR = (int)((color.r - clearColor.r) * 255.0f);
				int iG = (int)((color.g - clearColor.g) * 255.0f);
				int iB = (int)((color.b - clearColor.b) * 255.0f);
				int iA = (int)((color.a - clearColor.a) * 255.0f);
				return (iR < -1 || iR > 1 
					|| iG < -1 || iG > 1 
					|| iB < -1 || iB > 1 
					|| iA < -1 || iA > 1);
			}

			private bool CheckColors(int startX, int endX, int startY, int endY, int pickScale, Color clearColor)
			{
				bool isAnyValidColor = false;
				for (int iPickX = 0; iPickX <= pickScale; iPickX++)
				{
					int iX = (int)(((float)(endX - startX) / (float)pickScale) * iPickX) + startX;

					for (int iPickY = 0; iPickY <= pickScale; iPickY++)
					{
						int iY = (int)(((float)(endY - startY) / (float)pickScale) * iPickY) + startY;

						if(IsValidColor(_frameImage.GetPixel(iX, iY), clearColor))
						{
							//유효한 색상이 있다.
							if(iX < _minX) { _minX = iX; }
							if(iX > _maxX) { _maxX = iX; }
							if(iY < _minY) { _minY = iY; }
							if(iY > _maxY) { _maxY = iY; }

							isAnyValidColor = true;
						}
					}
				}
				return isAnyValidColor;
			}

			private void CheckColorsByPixel2(int startX, int endX, int startY, int endY, Color clearColor)
			{
				for (int iX = startX; iX <= endX; iX+=2)
				{
					for (int iY = startY; iY <= endY; iY+=2)
					{
						if(IsValidColor(_frameImage.GetPixel(iX, iY), clearColor))
						{
							//유효한 색상이 있다.
							if(iX < _minX) { _minX = iX; }
							if(iX > _maxX) { _maxX = iX; }
							if(iY < _minY) { _minY = iY; }
							if(iY > _maxY) { _maxY = iY; }
						}
					}
				}
			}
		}

		private Dictionary<apAnimClip, List<SpriteUnit>> _spriteUnits = new Dictionary<apAnimClip, List<SpriteUnit>>();


		// Init
		//---------------------------------------------------------
		public apSequenceExporter(apEditor editor)
		{
			_editor = editor;
		}

		public void Clear()
		{
			_sequenceMode = SEQUENCE_MODE.None;
			_isRequestStop = false;

			//공통
			_targetRootUnit = null;
			_frameScrollPosition = Vector2.zero;
			_frameZoomIndex = 0;
			

			_srcPosX = 0;
			_srcPosY = 0;
			_srcWidth = 0;
			_srcHeight = 0;
			_dstWidth = 0;
			_dstHeight = 0;
			_spriteUnitWidth = 0;
			_spriteUnitHeight = 0;
			_spriteImageWidth = 0;
			_spriteImageHeight = 0;

			_bgColor = Color.black;

			_captureLoadKey = null;

			//GIF 애니메이션의 절차적 처리를 위한 변수
			_GIF_animClip = null;
			_GIF_IsLoopAnimation = false;
			_GIF_IsAnimFirstFrame = false;
			_GIF_CurAnimFrame = 0;
			_GIF_StartAnimFrame = 0;
			_GIF_LastAnimFrame = 0;
			_GIF_CurAnimLoop = 0;
			_GIF_AnimLoopCount = 0;
			_GIF_CurAnimProcess = 0;
			_GIF_TotalAnimProcess = 0;
			_GIF_GifAnimQuality = 0;

#if UNITY_2017_4_OR_NEWER
			//MP4 애니메이션의 절차적 처리를 위한 변수
			_MP4_animClip = null;
			_MP4_IsLoopAnimation = false;
			_MP4_CurAnimFrame = 0;
			_MP4_StartAnimFrame = 0;
			_MP4_LastAnimFrame = 0;
			_MP4_CurAnimLoop = 0;
			_MP4_AnimLoopCount = 0;
			_MP4_CurAnimProcess = 0;
			_MP4_TotalAnimProcess = 0;
#endif


			//SpriteSheet 절차적 처리를 위한 변수
			_Sprite_AnimClips.Clear();
			_Sprite_CurAnimClip = null;
			_Sprite_CurAnimClipIndex = 0;
			_Sprite_CurFrame = 0;
			_Sprite_StartFrame = 0;
			_Sprite_LastFrame = 0;
			_Sprite_IsLoopAnimation = false;
			//_Sprite_CurFPS = 0;
			_Sprite_TotalAnimFrames = 0;
			_Sprite_CurAnimFrameOnTotal = 0;
			_Sprite_IsSizeCompressed = false;
			_spriteUnits.Clear();
		}


		// Functions
		//---------------------------------------------------------
		public void StopAll()
		{
			if (_spriteUnits.Count > 0)
			{
				//Debug.Log("StopAll");
				foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> units in _spriteUnits)
				{

					for (int i = 0; i < units.Value.Count; i++)
					{
						if (units.Value[i]._frameImage != null)
						{
							UnityEngine.GameObject.DestroyImmediate(units.Value[i]._frameImage);
						}

					}
				}

			}
			Clear();
		}

		//---------------------------------------------------------------------------------------
		// Functions : GIF Animation
		//---------------------------------------------------------------------------------------
		public bool StartGIFAnimation(apRootUnit rootUnit, apAnimClip animClip, 
										int loopCount, int gifQulity, 
										string saveFilePath, 
										FUNC_SEQUENCE_RESULT funcSequenceResut)
		{
			if (rootUnit == null || animClip == null)
			{
				StopAll();
				return false;
			}

			//혹시 모를 처리 일단 중단
			StopAll();

			_srcPosX = _editor._captureFrame_PosX;
			_srcPosY = _editor._captureFrame_PosY;
			_srcWidth = _editor._captureFrame_SrcWidth;
			_srcHeight = _editor._captureFrame_SrcHeight;
			_dstWidth = _editor._captureFrame_DstWidth;
			_dstHeight = _editor._captureFrame_DstHeight;
			_bgColor = _editor._captureFrame_Color;
			_saveFilePath = saveFilePath;

			_frameScrollPosition = _editor._scroll_CenterWorkSpace;
			_frameZoomIndex = _editor._iZoomX100;

			_targetRootUnit = rootUnit;
			_GIF_animClip = animClip;

			_GIF_IsLoopAnimation = _GIF_animClip.IsLoop;
			_GIF_IsAnimFirstFrame = true;

			_GIF_StartAnimFrame = _GIF_animClip.StartFrame;
			_GIF_LastAnimFrame = _GIF_animClip.EndFrame;

			_GIF_AnimLoopCount = loopCount;

			if (_GIF_AnimLoopCount < 1)
			{
				_GIF_AnimLoopCount = 1;
			}
			_GIF_GifAnimQuality = gifQulity;

			if (_GIF_IsLoopAnimation)
			{
				//루프인 경우 마지막 프레임은 제외
				_GIF_LastAnimFrame--;
			}
			if (_GIF_LastAnimFrame < _GIF_StartAnimFrame)
			{
				_GIF_LastAnimFrame = _GIF_StartAnimFrame;
			}
			_GIF_CurAnimFrame = _GIF_StartAnimFrame;
			_GIF_CurAnimLoop = 0;
			_GIF_CurAnimProcess = 0;

			_GIF_TotalAnimProcess = (Mathf.Abs(_GIF_LastAnimFrame - _GIF_StartAnimFrame) + 1) * _GIF_AnimLoopCount;

			_sequenceMode = SEQUENCE_MODE.GIF;
			_isRequestStop = false;
			_funcSequenceResut = funcSequenceResut;


			//1. GIF 헤더를 만들고
			//2. 이제 프레임을 하나씩 렌더링하기 시작하자

			//GIF 헤더
			bool isHeaderResult = _editor.Exporter.MakeGIFHeader(saveFilePath, _GIF_animClip, _dstWidth, _dstHeight);

			if (!isHeaderResult)
			{
				//실패한 경우
				//_captureMode = CAPTURE_MODE.None;
				StopAll();
				return false;
			}


			//첫번째 프레임
			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeAnimCapture(OnGIFFrameCaptured,
														_editor,
														_targetRootUnit._childMeshGroup,
														//true,
														_GIF_animClip, _GIF_CurAnimFrame,
														(int)(_srcPosX + apGL.WindowSizeHalf.x),
														(int)(_srcPosY + apGL.WindowSizeHalf.y),
														_srcWidth, _srcHeight,
														_dstWidth, _dstHeight,
														_frameScrollPosition, _frameZoomIndex,
														_bgColor, 
														_editor._captureFrame_IsPhysics,
														_GIF_CurAnimProcess, _saveFilePath);

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			_editor.ScreenCaptureRequest(newRequest);
			_editor.SetRepaint();

			return true;
		}




		
		private void OnGIFFrameCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			//우왕 왔당
			if (!isSuccess
				|| captureImage == null
				|| string.IsNullOrEmpty(filePath)
				|| _sequenceMode != SEQUENCE_MODE.GIF
				|| _captureLoadKey != loadKey
				|| _isRequestStop)
			{
				//Debug.LogError("프레임 오류");
				//Debug.LogError("Failed..");
				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;
				_isRequestStop = false;

				//오류가 났다
				_editor.Exporter.EndGIF();
				_sequenceMode = SEQUENCE_MODE.None;
				StopAll();

				if (_funcSequenceResut != null)
				{
					_funcSequenceResut(false);//실패
					_funcSequenceResut = null;
				}
				
				return;
			}

			_captureLoadKey = null;

			//이미지를 GIF 프레임으로 하나씩 넣자
			bool addFrameResult = _editor.Exporter.AddGIFFrame(captureImage, _GIF_IsAnimFirstFrame, _GIF_GifAnimQuality);

			if (!addFrameResult)
			{
				//으잉 실패...
				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				//오류가 났다
				_editor.Exporter.EndGIF();
				_sequenceMode = SEQUENCE_MODE.None;
				StopAll();
				if(_funcSequenceResut != null)
				{
					_funcSequenceResut(false);
					_funcSequenceResut = null;
				}
				return;
			}

			//이제 프레임 하나 증가
			_GIF_CurAnimFrame++;
			_GIF_CurAnimProcess++;
			_GIF_IsAnimFirstFrame = false;

			if (_GIF_CurAnimFrame > _GIF_LastAnimFrame)
			{
				//끝까지 갔네염
				//루프 카운트 증가
				_GIF_CurAnimFrame = _GIF_StartAnimFrame;
				_GIF_CurAnimLoop++;

				if (_GIF_CurAnimLoop >= _GIF_AnimLoopCount)
				{
					//으잉 루프 카운트가 끝났다.

					//끄으읕
					if (captureImage != null)
					{
						UnityEngine.GameObject.DestroyImmediate(captureImage);
					}
					_captureLoadKey = null;

					_editor.Exporter.EndGIF();
					_sequenceMode = SEQUENCE_MODE.None;

					//완성된 파일을 열자
					System.IO.FileInfo fi = new System.IO.FileInfo(filePath);//Path 빈 문자열 확인했음 (21.9.10)

					Application.OpenURL("file://" + fi.Directory.FullName);
					Application.OpenURL("file://" + filePath);

					if(_funcSequenceResut != null)
					{
						_funcSequenceResut(true);//성공!
						_funcSequenceResut = null;
					}
					StopAll();
					return;
				}
			}


			//다음 프레임을 렌더링하자
			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeAnimCapture(OnGIFFrameCaptured,
														_editor,
														_targetRootUnit._childMeshGroup,
														//true,
														_GIF_animClip, _GIF_CurAnimFrame,
														(int)(_srcPosX + apGL.WindowSizeHalf.x),
														(int)(_srcPosY + apGL.WindowSizeHalf.y),
														_srcWidth, _srcHeight,
														_dstWidth, _dstHeight,
														_frameScrollPosition, _frameZoomIndex,
														_bgColor, 
														_editor._captureFrame_IsPhysics,
														_GIF_CurAnimProcess, _saveFilePath);
			

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			_editor.ScreenCaptureRequest(newRequest);
			_editor.SetRepaint();
		}


		//---------------------------------------------------------------------------------------
		// Functions : MP4 Animation
		//---------------------------------------------------------------------------------------
		public bool StartMP4Animation(apRootUnit rootUnit, apAnimClip animClip, 
										int loopCount, 
										string saveFilePath, 
										FUNC_SEQUENCE_RESULT funcSequenceResut)
		{
#if UNITY_2017_4_OR_NEWER
			if (rootUnit == null || animClip == null)
			{
				StopAll();
				return false;
			}

			//혹시 모를 처리 일단 중단
			StopAll();

			_srcPosX = _editor._captureFrame_PosX;
			_srcPosY = _editor._captureFrame_PosY;
			_srcWidth = _editor._captureFrame_SrcWidth;
			_srcHeight = _editor._captureFrame_SrcHeight;
			_dstWidth = _editor._captureFrame_DstWidth;
			_dstHeight = _editor._captureFrame_DstHeight;
			_bgColor = _editor._captureFrame_Color;
			_bgColor.a = 1.0f;//<<MP4는 투명을 지원하지 않는다.
			_saveFilePath = saveFilePath;

			_frameScrollPosition = _editor._scroll_CenterWorkSpace;
			_frameZoomIndex = _editor._iZoomX100;

			_targetRootUnit = rootUnit;
			

			_MP4_animClip = animClip;

			_MP4_IsLoopAnimation = _MP4_animClip.IsLoop;
			
			_MP4_StartAnimFrame = _MP4_animClip.StartFrame;
			_MP4_LastAnimFrame = _MP4_animClip.EndFrame;

			_MP4_AnimLoopCount = loopCount;

			if (_MP4_AnimLoopCount < 1)
			{
				_MP4_AnimLoopCount = 1;
			}
			
			if (_MP4_IsLoopAnimation)
			{
				//루프인 경우 마지막 프레임은 제외
				_MP4_LastAnimFrame--;
			}
			if (_MP4_LastAnimFrame < _MP4_StartAnimFrame)
			{
				_MP4_LastAnimFrame = _MP4_StartAnimFrame;
			}
			_MP4_CurAnimFrame = _MP4_StartAnimFrame;
			_MP4_CurAnimLoop = 0;
			_MP4_CurAnimProcess = 0;

			_MP4_TotalAnimProcess = (Mathf.Abs(_MP4_LastAnimFrame - _MP4_StartAnimFrame) + 1) * _MP4_AnimLoopCount;

			_sequenceMode = SEQUENCE_MODE.MP4;
			_isRequestStop = false;
			_funcSequenceResut = funcSequenceResut;


			//1. MP4 파일을 만들고
			//2. 이제 프레임을 하나씩 렌더링하기 시작하자

			//GIF 헤더
			bool isHeaderResult = _editor.Exporter.MakeMP4Animation(saveFilePath, _MP4_animClip, _dstWidth, _dstHeight);

			if (!isHeaderResult)
			{
				//실패한 경우
				//_captureMode = CAPTURE_MODE.None;
				StopAll();
				return false;
			}


			//첫번째 프레임
			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeAnimCapture(OnMP4FrameCaptured,
														_editor,
														_targetRootUnit._childMeshGroup,
														//true,
														_MP4_animClip, _MP4_CurAnimFrame,
														(int)(_srcPosX + apGL.WindowSizeHalf.x),
														(int)(_srcPosY + apGL.WindowSizeHalf.y),
														_srcWidth, _srcHeight,
														_dstWidth, _dstHeight,
														_frameScrollPosition, _frameZoomIndex,
														_bgColor, 
														_editor._captureFrame_IsPhysics,
														_MP4_CurAnimProcess, _saveFilePath);

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			_editor.ScreenCaptureRequest(newRequest);
			_editor.SetRepaint();

			return true;
#else
			return false;
#endif
		}




		
		private void OnMP4FrameCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
#if UNITY_2017_4_OR_NEWER
			//우왕 왔당
			if (!isSuccess
				|| captureImage == null
				|| string.IsNullOrEmpty(filePath)
				|| _sequenceMode != SEQUENCE_MODE.MP4
				
				|| _captureLoadKey != loadKey
				|| _isRequestStop)
			{
				//Debug.LogError("프레임 오류");
				//Debug.LogError("Failed..");
				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				//오류가 났다
				_editor.Exporter.EndMP4();
				_sequenceMode = SEQUENCE_MODE.None;
				StopAll();

				if (_funcSequenceResut != null)
				{
					_funcSequenceResut(false);//실패
					_funcSequenceResut = null;
				}
				return;
			}

			_captureLoadKey = null;

			//이미지를 MP4 프레임으로 하나씩 넣자
			bool addFrameResult = _editor.Exporter.AddMP4Frame(captureImage);

			if (!addFrameResult)
			{
				//으잉 실패...
				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				//오류가 났다
				_editor.Exporter.EndMP4();
				_sequenceMode = SEQUENCE_MODE.None;
				StopAll();
				if(_funcSequenceResut != null)
				{
					_funcSequenceResut(false);
					_funcSequenceResut = null;
				}
				return;
			}

			//이제 프레임 하나 증가
			_MP4_CurAnimFrame++;
			_MP4_CurAnimProcess++;

			if (_MP4_CurAnimFrame > _MP4_LastAnimFrame)
			{
				//끝까지 갔네염
				//루프 카운트 증가
				_MP4_CurAnimFrame = _MP4_StartAnimFrame;
				_MP4_CurAnimLoop++;

				if (_MP4_CurAnimLoop >= _MP4_AnimLoopCount)
				{
					//으잉 루프 카운트가 끝났다.

					//끄으읕
					if (captureImage != null)
					{
						UnityEngine.GameObject.DestroyImmediate(captureImage);
					}
					_captureLoadKey = null;

					_editor.Exporter.EndMP4();
					_sequenceMode = SEQUENCE_MODE.None;

					//완성된 파일을 열자
					System.IO.FileInfo fi = new System.IO.FileInfo(filePath);

					Application.OpenURL("file://" + fi.Directory.FullName);
					Application.OpenURL("file://" + filePath);

					if(_funcSequenceResut != null)
					{
						_funcSequenceResut(true);//성공!
						_funcSequenceResut = null;
					}
					StopAll();
					return;
				}
			}


			//다음 프레임을 렌더링하자
			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeAnimCapture(OnMP4FrameCaptured,
														_editor,
														_targetRootUnit._childMeshGroup,
														//true,
														_MP4_animClip, _MP4_CurAnimFrame,
														(int)(_srcPosX + apGL.WindowSizeHalf.x),
														(int)(_srcPosY + apGL.WindowSizeHalf.y),
														_srcWidth, _srcHeight,
														_dstWidth, _dstHeight,
														_frameScrollPosition, _frameZoomIndex,
														_bgColor, 
														_editor._captureFrame_IsPhysics,
														_MP4_CurAnimProcess, _saveFilePath);
			

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			_editor.ScreenCaptureRequest(newRequest);
			_editor.SetRepaint();
#else
			if (captureImage != null)
			{
				UnityEngine.GameObject.DestroyImmediate(captureImage);
			}
			_captureLoadKey = null;

			//오류가 났다
			_sequenceMode = SEQUENCE_MODE.None;
			StopAll();

			if (_funcSequenceResut != null)
			{
				_funcSequenceResut(false);//실패
				_funcSequenceResut = null;
			}
#endif
			
		}

		//---------------------------------------------------------------------------------------
		// Functions : Sprite Sheet
		//---------------------------------------------------------------------------------------
		public bool StartSpritesheet(apRootUnit rootUnit, List<apAnimClip> animClips, List<bool> animClipFlags, 
										string saveFilePath, 
										bool isSizeCompressed,
										bool isSequenceFiles,
										int margin,
										bool isMeta_XML,
										bool isMeta_JSON,
										bool isMeta_TXT,
										FUNC_SEQUENCE_RESULT funcSequenceResut)
		{

			//1. 애니메이션을 한번씩 돌리고, 결과를 SpriteUnit으로 텍스쳐와 애니메이션+프레임 정보, 이미지의 크기와 오프셋 등을 저장하여 리스트로 가진다.
			//2. Pack 방식에 따라서 한꺼번에 합친다.

			if (rootUnit == null || animClips == null || animClips.Count == 0)
			{
				StopAll();
				return false;
			}
			
			//혹시 모를 처리 일단 중단
			StopAll();

			//처리해야할 전체 AnimClip을 리스트로 받아온다.
			_Sprite_TotalAnimFrames = 0;
			_Sprite_CurAnimFrameOnTotal = 0;

			_Sprite_AnimClips.Clear();
			for (int i = 0; i < animClips.Count; i++)
			{
				if(animClipFlags[i])
				{
					_Sprite_AnimClips.Add(animClips[i]);
					int frames = 0;
					if(animClips[i].IsLoop)
					{
						frames = Mathf.Max(animClips[i].EndFrame - animClips[i].StartFrame, 1);
					}
					else
					{
						frames = Mathf.Max((animClips[i].EndFrame - animClips[i].StartFrame) + 1, 1);
					}
					_Sprite_TotalAnimFrames += frames;
				}
			}

			if(_Sprite_TotalAnimFrames < 1)
			{
				_Sprite_TotalAnimFrames = 1;
			}
			if(_Sprite_AnimClips.Count == 0)
			{
				StopAll();
				return false;
			}


			

			_srcPosX = _editor._captureFrame_PosX;
			_srcPosY = _editor._captureFrame_PosY;
			_srcWidth = _editor._captureFrame_SrcWidth;
			_srcHeight = _editor._captureFrame_SrcHeight;
			_spriteUnitWidth = _editor._captureFrame_SpriteUnitWidth;
			_spriteUnitHeight = _editor._captureFrame_SpriteUnitHeight;

			_spriteImageWidth = 0;
			_spriteImageHeight = 0;
			switch (_editor._captureSpritePackImageWidth)
			{
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256: _spriteImageWidth = 256; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s512: _spriteImageWidth = 512; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024: _spriteImageWidth = 1024; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s2048: _spriteImageWidth = 2048; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096: _spriteImageWidth = 4096; break;
			}
			switch (_editor._captureSpritePackImageHeight)
			{
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256: _spriteImageHeight = 256; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s512: _spriteImageHeight = 512; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024: _spriteImageHeight = 1024; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s2048: _spriteImageHeight = 2048; break;
				case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096: _spriteImageHeight = 4096; break;
			}

			_bgColor = _editor._captureFrame_Color;
			_saveFilePath = saveFilePath;


			_frameScrollPosition = _editor._scroll_CenterWorkSpace;
			_frameZoomIndex = _editor._iZoomX100;

			_targetRootUnit = rootUnit;

			_Sprite_IsSizeCompressed = isSizeCompressed;
			_Sprite_IsSequenceFiles = isSequenceFiles;//<<이게 True라면 Spritesheet는 만들지 않고 SequenceFile만 만든다.
			_Sprite_FirstSequenceFilePath = "";
			_Sprite_IsFirstSequenceFile = true;

			//추가 정보를 넣자
			_Sprite_Margin = margin;
			_Sprite_IsMeta_XML = isMeta_XML;
			_Sprite_IsMeta_JSON = isMeta_JSON;
			_Sprite_IsMeta_TXT = isMeta_TXT;

			//margin은 최대 크기가 있다.
			if(_Sprite_Margin > _spriteUnitWidth / 4) { _Sprite_Margin = _spriteUnitWidth / 4; }
			if(_Sprite_Margin > _spriteUnitHeight / 4) { _Sprite_Margin = _spriteUnitHeight / 4; }

			//첫번째 AnimClip 선택
			_Sprite_CurAnimClip = _Sprite_AnimClips[0];
			_Sprite_CurAnimClipIndex = 0;

			_Sprite_CurFrame = 0;
			_Sprite_StartFrame = _Sprite_CurAnimClip.StartFrame;
			_Sprite_LastFrame = _Sprite_CurAnimClip.EndFrame;
			_Sprite_IsLoopAnimation = _Sprite_CurAnimClip.IsLoop;
			if(_Sprite_IsLoopAnimation)
			{
				_Sprite_LastFrame--;
			}
			if(_Sprite_LastFrame < _Sprite_StartFrame)
			{
				_Sprite_LastFrame = _Sprite_StartFrame;
			}
			//_Sprite_CurFPS = _Sprite_CurAnimClip.FPS;

			_sequenceMode = SEQUENCE_MODE.Spritesheet_Capture;
			_funcSequenceResut = funcSequenceResut;

			_spriteUnits.Clear();

			for (int i = 0; i < _editor._portrait._animClips.Count; i++)
			{
				_editor._portrait._animClips[i]._isSelectedInEditor = false;
			}

			_Sprite_CurAnimClip.LinkEditor(_editor._portrait);
			_Sprite_CurAnimClip.RefreshTimelines(null, null);//모든 타임라인 Refresh
			_Sprite_CurAnimClip.SetFrame_Editor(_Sprite_CurAnimClip.StartFrame);
			_Sprite_CurAnimClip.Pause_Editor();
			_Sprite_CurAnimClip._isSelectedInEditor = true;
			_editor._portrait._animPlayManager.SetAnimClip_Editor(_Sprite_CurAnimClip);
			
			//첫번째 프레임
			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeAnimCapture(OnSpriteUnitCaptured,
														_editor,
														_targetRootUnit._childMeshGroup,
														//true,
														_Sprite_CurAnimClip, _Sprite_CurFrame,
														(int)(_srcPosX + apGL.WindowSizeHalf.x),
														(int)(_srcPosY + apGL.WindowSizeHalf.y),
														_srcWidth, _srcHeight,
														_spriteUnitWidth, _spriteUnitHeight,
														_frameScrollPosition, _frameZoomIndex,
														_bgColor, 
														_editor._captureFrame_IsPhysics,
														_Sprite_CurAnimFrameOnTotal, _saveFilePath);

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			_editor.ScreenCaptureRequest(newRequest);
			_editor.SetRepaint();

			return true;
		}
		
		private void OnSpriteUnitCaptured(bool isSuccess, Texture2D captureImage, int iProcessStep, string filePath, object loadKey)
		{
			if (!isSuccess
				|| captureImage == null
				|| string.IsNullOrEmpty(filePath)
				|| _sequenceMode != SEQUENCE_MODE.Spritesheet_Capture
				|| _captureLoadKey != loadKey
				|| _isRequestStop)
			{
				if (captureImage != null)
				{
					UnityEngine.GameObject.DestroyImmediate(captureImage);
				}
				_captureLoadKey = null;

				//오류가 났다
				_sequenceMode = SEQUENCE_MODE.None;
				StopAll();

				if (_funcSequenceResut != null)
				{
					_funcSequenceResut(false);//실패
					_funcSequenceResut = null;
				}
				return;
			}


			//시퀸스 파일 옵션을 추가하고 나중에 다시 넣자
			string strSequenceFileName = "";
			if (_Sprite_IsSequenceFiles)
			{
				
				//시퀸스 옵션이 있다면 바로 저장한다.
				try
				{
					string filePathWOExtension = filePath.Substring(0, filePath.Length - 4) + "_" + _Sprite_CurAnimClip._name + "__" + _Sprite_CurFrame;

					System.IO.FileInfo fi = new System.IO.FileInfo(filePathWOExtension + ".png");//Path 빈 문자열 확인했음 (21.9.10)
					strSequenceFileName = fi.Name;

					//AutoDestroy = true > False
					_editor.Exporter.SaveTexture2DToPNG(captureImage, filePathWOExtension, false);
					if (_Sprite_IsFirstSequenceFile)
					{
						_Sprite_FirstSequenceFilePath = filePathWOExtension + ".png";
						_Sprite_IsFirstSequenceFile = false;
					}

				}
				catch (Exception)
				{

				}
			}


			_captureLoadKey = null;

			//프레임 리스트에 SpriteUnit을 하나씩 넣자
			if(!_spriteUnits.ContainsKey(_Sprite_CurAnimClip))
			{
				_spriteUnits.Add(_Sprite_CurAnimClip, new List<SpriteUnit>());
			}

			SpriteUnit spriteUnit = new SpriteUnit();
			spriteUnit.SetTexture(captureImage, _Sprite_CurAnimClip, _Sprite_CurFrame, captureImage.width, captureImage.height, captureImage.width / 2, captureImage.height / 2);
			spriteUnit.SetSequenceFileName(strSequenceFileName);

			_spriteUnits[_Sprite_CurAnimClip].Add(spriteUnit);

			//이제 프레임을 하나씩 증가
			_Sprite_CurFrame++;
			_Sprite_CurAnimFrameOnTotal++;
			if (_Sprite_CurFrame > _Sprite_LastFrame)
			{
				//마지막 프레임을 지났다.
				//1. 다음 AnimClip이 있다면 전환
				//2. 이게 마지막 애니메이션이었다면 Pack 진행

				_Sprite_CurAnimClipIndex++;
				_Sprite_CurAnimClip = null;
				if (_Sprite_CurAnimClipIndex < _Sprite_AnimClips.Count)
				{
					//Debug.Log("Next Animation Clip");
					//1. 다음 AnimClip으로 전환
					_Sprite_CurAnimClip = _Sprite_AnimClips[_Sprite_CurAnimClipIndex];

					_Sprite_CurFrame = 0;
					_Sprite_StartFrame = _Sprite_CurAnimClip.StartFrame;
					_Sprite_LastFrame = _Sprite_CurAnimClip.EndFrame;
					_Sprite_IsLoopAnimation = _Sprite_CurAnimClip.IsLoop;
					if (_Sprite_IsLoopAnimation)
					{
						_Sprite_LastFrame--;
					}
					if (_Sprite_LastFrame < _Sprite_StartFrame)
					{
						_Sprite_LastFrame = _Sprite_StartFrame;
					}
					//_Sprite_CurFPS = _Sprite_CurAnimClip.FPS;

					for (int i = 0; i < _editor._portrait._animClips.Count; i++)
					{
						_editor._portrait._animClips[i]._isSelectedInEditor = false;
					}

					_Sprite_CurAnimClip.LinkEditor(_editor._portrait);
					_Sprite_CurAnimClip.RefreshTimelines(null, null);
					_Sprite_CurAnimClip.SetFrame_Editor(_Sprite_CurAnimClip.StartFrame);
					_Sprite_CurAnimClip.Pause_Editor();
					_Sprite_CurAnimClip._isSelectedInEditor = true;
					_editor._portrait._animPlayManager.SetAnimClip_Editor(_Sprite_CurAnimClip);

					//다음 렌더링 요청
					apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
					_captureLoadKey = newRequest.MakeAnimCapture(OnSpriteUnitCaptured,
																_editor,
																_targetRootUnit._childMeshGroup,
																//true,
																_Sprite_CurAnimClip, _Sprite_CurFrame,
																(int)(_srcPosX + apGL.WindowSizeHalf.x),
																(int)(_srcPosY + apGL.WindowSizeHalf.y),
																_srcWidth, _srcHeight,
																_spriteUnitWidth, _spriteUnitHeight,
																_frameScrollPosition, _frameZoomIndex,
																_bgColor, 
																_editor._captureFrame_IsPhysics,
																_Sprite_CurAnimFrameOnTotal, _saveFilePath);

					//에디터에 대신 렌더링해달라고 요청을 합시다.
					_editor.ScreenCaptureRequest(newRequest);
					_editor.SetRepaint();
				}
				else
				{
					//Debug.Log("End Animation Clip");
					//2. 마지막 애니메이션이었다.
					PackSpritesToImageFiles();
				}
			}
			else
			{
				//Debug.Log("Next Frame : " + _Sprite_CurFrame);
				//아직 프레임이 남았다.
				//3. 다음 프레임 렌더링
				//다음 렌더링 요청
				apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
				_captureLoadKey = newRequest.MakeAnimCapture(OnSpriteUnitCaptured,
															_editor,
															_targetRootUnit._childMeshGroup,
															//true,
															_Sprite_CurAnimClip, _Sprite_CurFrame,
															(int)(_srcPosX + apGL.WindowSizeHalf.x),
															(int)(_srcPosY + apGL.WindowSizeHalf.y),
															_srcWidth, _srcHeight,
															_spriteUnitWidth, _spriteUnitHeight,
															_frameScrollPosition, _frameZoomIndex,
															_bgColor, 
															_editor._captureFrame_IsPhysics,
															_Sprite_CurAnimFrameOnTotal, _saveFilePath);

				//에디터에 대신 렌더링해달라고 요청을 합시다.
				_editor.ScreenCaptureRequest(newRequest);
				_editor.SetRepaint();
			}
			

		}


		private void PackSpritesToImageFiles()
		{
			

			//Pack으로 묶고
			

			//일단 순서대로 이미지를 배치해보자.
			//1. 배치할 위치를 계산한다.
			//2. 텍스쳐를 준비해서 Clear 이후에 하나씩 색을 채워 넣는다.
			
			List<SpriteUnit> spriteUnitsToBake = new List<SpriteUnit>();
			foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> spriteUnitPair in _spriteUnits)
			{
				for (int i = 0; i < spriteUnitPair.Value.Count; i++)
				{
					spriteUnitsToBake.Add(spriteUnitPair.Value[i]);
				}
			}

			if (_Sprite_IsSequenceFiles)
			{
				//만약 시퀸스 타입이면
				//이미지 삭제하고 그냥 종료한다.

				//Meta파일 저장
				if(_Sprite_IsMeta_XML)
				{
					//XML 파일 저장
					ExportToXML(_saveFilePath, _spriteUnits, _Sprite_IsSequenceFiles, _spriteImageHeight);
				}
				if(_Sprite_IsMeta_JSON)
				{
					//JSON 파일 저장
					ExportToJSON(_saveFilePath, _spriteUnits, _Sprite_IsSequenceFiles, _spriteImageHeight);
				}
				if(_Sprite_IsMeta_TXT)
				{
					//TXT 파일 저장
					ExportToTXT(_saveFilePath, _spriteUnits, _Sprite_IsSequenceFiles, _spriteImageHeight);
				}

				//이미지들을 모두 삭제
				foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> spriteUnitPair in _spriteUnits)
				{
					for (int i = 0; i < spriteUnitPair.Value.Count; i++)
					{
						UnityEngine.GameObject.DestroyImmediate(spriteUnitPair.Value[i]._frameImage);
					}
				}
				//해당 경로를 열어보자
				if (!string.IsNullOrEmpty(_Sprite_FirstSequenceFilePath))
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(_Sprite_FirstSequenceFilePath);//Path 빈 문자열 확인했음 (21.9.10)
					Application.OpenURL("file://" + fi.Directory.FullName);
					Application.OpenURL("file://" + _Sprite_FirstSequenceFilePath);
				}

				if (_funcSequenceResut != null)
				{
					_funcSequenceResut(true);
					_funcSequenceResut = null;
				}

				StopAll();

				return;
			}
			//_spriteImageWidth x _spriteImageHeight

			int bakeTextureIndex = 0;
			int bakeCurX = 0;
			int bakeCurY = 0;
			int margin = _Sprite_Margin;

			bakeCurX = margin;
			bakeCurY = _spriteImageHeight - margin;

			//Bake 옵션
			//- 기본일 때 : 그냥 순서대로
			//- 크기 최적화 : 각 애니메이션 별로 크기 최적화를 한다. (같은 애니메이션은 동일한 크기를 가진다)
			SpriteUnit nextUnit = null;

			
			if (_Sprite_IsSizeCompressed)
			{
				//일단 전체 Optimize를 계산한다.
				for (int i = 0; i < spriteUnitsToBake.Count; i++)
				{
					nextUnit = spriteUnitsToBake[i];
					nextUnit.Optimize(_bgColor);
				}

				//각각 AnimClip별로 compOffset LR, BT를 가장 작은 (렌더링 영역이 가장 크게) 값을 구한다.
				
				foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> spriteUnitPair in _spriteUnits)
				{
					int minCompOffset_X = int.MaxValue;
					int minCompOffset_Y = int.MaxValue;
					for (int i = 0; i < spriteUnitPair.Value.Count; i++)
					{
						nextUnit = spriteUnitPair.Value[i];
						if(nextUnit._compOffset_Left < minCompOffset_X) { minCompOffset_X = nextUnit._compOffset_Left; }
						if(nextUnit._compOffset_Right < minCompOffset_X) { minCompOffset_X = nextUnit._compOffset_Right; }
						if(nextUnit._compOffset_Bottom < minCompOffset_Y) { minCompOffset_Y = nextUnit._compOffset_Bottom; }
						if(nextUnit._compOffset_Top < minCompOffset_Y) { minCompOffset_Y = nextUnit._compOffset_Top; }
					}
					minCompOffset_X = Mathf.Clamp(minCompOffset_X, 0, _spriteUnitWidth);
					minCompOffset_Y = Mathf.Clamp(minCompOffset_Y, 0, _spriteUnitHeight);
					//Debug.Log("Anim Optimized Offset : " + spriteUnitPair.Key._name + " / " + minCompOffset_X + " , " + minCompOffset_Y);

					//이제 다시 OptSize를 넣어주자
					for (int i = 0; i < spriteUnitPair.Value.Count; i++)
					{
						nextUnit = spriteUnitPair.Value[i];
						nextUnit.SetOptimizedSize(minCompOffset_X, minCompOffset_Y);
					}
				}
			}
			else
			{
				for (int i = 0; i < spriteUnitsToBake.Count; i++)
				{
					nextUnit = spriteUnitsToBake[i];
					nextUnit.SetOptimizedSize(0, 0);
				}
			}
			
			apAnimClip prevAnimClip = spriteUnitsToBake[0]._animClip;
			for (int i = 0; i < spriteUnitsToBake.Count; i++)
			{
				nextUnit = spriteUnitsToBake[i];
				if(nextUnit._isBaked)
				{
					continue;
				}



				//현재 위치에서 X축으로 추가 가능한지 확인하자
				//또는 AnimClip이 바뀌었을 때
				if(bakeCurX + nextUnit._bakeWidth + margin > _spriteImageWidth
					|| prevAnimClip != nextUnit._animClip)
				{
					//X축으로 더이상 넣을 수 없다.
					//X는 Left, Y는 Bottom으로 증가한다.
					bakeCurX = margin;
					bakeCurY -= nextUnit._bakeHeight + margin;
				}
				
				//Y로 넣을 수 없다면?
				if(bakeCurY - (nextUnit._bakeHeight + margin) < 0)
				{
					//다음 텍스쳐로 넘어가야 한다.
					bakeCurX = margin;
					bakeCurY = _spriteImageHeight - margin;
					bakeTextureIndex++;
				}

				//Bake Option을 지정한다.
				nextUnit.SetBakeOption(bakeTextureIndex, bakeCurX, bakeCurY - nextUnit._bakeHeight);

				//그리고 X로 하나 증가
				bakeCurX += nextUnit._bakeWidth + margin;

				prevAnimClip = nextUnit._animClip;
			}

			//이제 실제로 텍스쳐를 만들고 굽자
			int nTexture = bakeTextureIndex + 1;
			Texture2D[] packTextures = new Texture2D[nTexture];

			for (int i = 0; i < nTexture; i++)
			{
				Texture2D newTexture = new Texture2D(_spriteImageWidth, _spriteImageHeight, TextureFormat.ARGB32, false, false);
				//기본 색 부터 깔자
				for (int iX = 0; iX < _spriteImageWidth; iX++)
				{
					for (int iY = 0; iY < _spriteImageHeight; iY++)
					{
						newTexture.SetPixel(iX, iY, _bgColor);
					}
				}
				newTexture.Apply();
				packTextures[i] = newTexture;
			}

			Texture2D targetTexture = null;
			for (int i = 0; i < spriteUnitsToBake.Count; i++)
			{
				nextUnit = spriteUnitsToBake[i];
				if(!nextUnit._isBaked)
				{
					continue;
				}
				targetTexture = packTextures[nextUnit._bakeTextureIndex];

				for (int iX = 0; iX < nextUnit._bakeWidth; iX++)
				{
					for (int iY = 0; iY < nextUnit._bakeHeight; iY++)
					{
						targetTexture.SetPixel(
							iX + nextUnit._bakePosX, iY + nextUnit._bakePosY,
							nextUnit._frameImage.GetPixel(iX + nextUnit._compOffset_Left, iY + nextUnit._compOffset_Top)
							);
					}
				}
				targetTexture.Apply();
			}

			string strFirstImagePath = "";

			for (int i = 0; i < nTexture; i++)
			{
				Texture2D packTexture = packTextures[i];

				//저장된 Pack 이미지를 파일로 저장하자
				try
				{
					string filePathWOExtension = _saveFilePath.Substring(0, _saveFilePath.Length - 4) + "__" + (i);
					if(i == 0)
					{
						strFirstImagePath = filePathWOExtension + ".png";
					}
					//AutoDestroy = true
					_editor.Exporter.SaveTexture2DToPNG(packTexture, filePathWOExtension, false);
				}
				catch (Exception)
				{

				}
			}

			//Meta파일 저장
			if(_Sprite_IsMeta_XML)
			{
				//XML 파일 저장
				ExportToXML(_saveFilePath, _spriteUnits, _Sprite_IsSequenceFiles, _spriteImageHeight);
			}
			if(_Sprite_IsMeta_JSON)
			{
				//JSON 파일 저장
				ExportToJSON(_saveFilePath, _spriteUnits, _Sprite_IsSequenceFiles, _spriteImageHeight);
			}
			if (_Sprite_IsMeta_TXT)
			{
				//TXT 파일 저장
				ExportToTXT(_saveFilePath, _spriteUnits, _Sprite_IsSequenceFiles, _spriteImageHeight);
			}

			//이미지들을 모두 삭제
			foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> spriteUnitPair in _spriteUnits)
			{
				for (int i = 0; i < spriteUnitPair.Value.Count; i++)
				{
					UnityEngine.GameObject.DestroyImmediate(spriteUnitPair.Value[i]._frameImage);
				}
			}

			for (int i = 0; i < nTexture; i++)
			{
				Texture2D packTexture = packTextures[i];
				UnityEngine.GameObject.DestroyImmediate(packTexture);
			}

			//해당 경로를 열어보자
			if (!string.IsNullOrEmpty(strFirstImagePath))
			{
				System.IO.FileInfo fi = new System.IO.FileInfo(strFirstImagePath);//Path 빈 문자열 확인했음 (21.9.10)
				Application.OpenURL("file://" + fi.Directory.FullName);
				Application.OpenURL("file://" + strFirstImagePath);
			}

			if(_funcSequenceResut != null)
			{
				_funcSequenceResut(true);
				_funcSequenceResut = null;
			}


			
			StopAll();

			
			
		}

		private class CheckResult
		{
			public bool _isResult = false;
			public int _bakeX = 0;
			public int _bakeY = 0;
			public int _bakeX_Index = 0;
			public int _bakeY_Index = 0;
			public int _bakeWidth_Index = 0;
			public int _bakeHeight_Index = 0;
			
			public CheckResult()
			{
				Clear();
			}
			public void Clear()
			{
				_isResult = false;
				_bakeX = 0;
				_bakeY = 0;
				_bakeX_Index = 0;
				_bakeY_Index = 0;
				_bakeWidth_Index = 0;
				_bakeHeight_Index = 0;
			}
		}
		//private CheckResult _checkResult = new CheckResult();
		//private CheckResult CheckBakeToPack(bool[,] tileMap, 
		//								int unitWidth, int unitHeight, 
		//								int packImageWidth, int packImageHeight,
		//								int packTileSize, int margin)
		//{
		//	_checkResult.Clear();

		//	int unitWidht_Index = (int)(((float)(unitWidth + margin) + 0.5f) / (float)packTileSize);
		//	int unitHeight_Index = (int)(((float)(unitHeight + margin) + 0.5f) / (float)packTileSize);


		//	return _checkResult;
		//}



		// Get / Set
		//---------------------------------------------------------
		public float ProcessRatio
		{
			get
			{
				switch (_sequenceMode)
				{
					case SEQUENCE_MODE.None:
						return 0.0f;

					case SEQUENCE_MODE.GIF:
						return (float)(_GIF_CurAnimProcess) / (float)(_GIF_TotalAnimProcess);

					case SEQUENCE_MODE.MP4:
#if UNITY_2017_4_OR_NEWER
						return (float)(_MP4_CurAnimProcess) / (float)(_MP4_TotalAnimProcess);
#else
						return 0.0f;
#endif

					case SEQUENCE_MODE.Spritesheet_Capture:

						return ((float)(_Sprite_CurAnimFrameOnTotal) / (float)(_Sprite_TotalAnimFrames)) * 0.95f;

					case SEQUENCE_MODE.Spritesheet_Pack:
						return 0.95f;//<<TODO : Pack 과정도 출력하자
				}
				return 0.0f;
			}
		}

		public void RequestStop()
		{
			_isRequestStop = true;
		}

		//-------------------------------------------------------------------------------------------------
		// Text Export
		//-------------------------------------------------------------------------------------------------
		private void ExportToXML(string filePath, Dictionary<apAnimClip, List<SpriteUnit>> spriteUnits, bool isSequenceFiles, int spriteImageHeight)
		{
			string xmlFilePath = filePath.Substring(0, filePath.Length - 4) + "__XML.xml";
			XmlDocument xmlDoc = new XmlDocument();

			xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));

			try
			{
				//AnimtionClip이 먼저
				//인덱스, 이름, FPS, Frame개수, Loop 여부
				//FrameImage
				//Spritesheet : AnimClip 인덱스, 프레임, X, Y, Width, Height, 
				//Sequence : AnimClip 인덱스, 프레임, 파일 이름
				XmlNode rootNode = xmlDoc.CreateNode(XmlNodeType.Element, "Content", string.Empty);
				XmlNode animClipNode = xmlDoc.CreateNode(XmlNodeType.Element, "AnimationClip", string.Empty);
				XmlNode frameImageNode = xmlDoc.CreateNode(XmlNodeType.Element, "FrameImage", string.Empty);

				xmlDoc.AppendChild(rootNode);
				rootNode.AppendChild(animClipNode);
				rootNode.AppendChild(frameImageNode);

				Dictionary<apAnimClip, int> animClipAndIndex = new Dictionary<apAnimClip, int>();
				
				apAnimClip animClip = null;
				List<SpriteUnit> spriteUnitList = null;
				SpriteUnit unit = null;
				int curAnimClipIndex = 0;
				foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> spriteUnitPair in spriteUnits)
				{
					animClip = spriteUnitPair.Key;

					//먼저 AnimClip Element를 추가하자
					if (!animClipAndIndex.ContainsKey(animClip))
					{
						animClipAndIndex.Add(animClip, curAnimClipIndex);
						curAnimClipIndex++;
					}
					int animClipIndex = animClipAndIndex[animClip];

					XmlElement animClipElement = xmlDoc.CreateElement("Clip");
					animClipElement.SetAttribute("Index", animClipIndex.ToString());
					animClipElement.SetAttribute("Name", animClip._name);
					animClipElement.SetAttribute("FPS", animClip.FPS.ToString());
					animClipElement.SetAttribute("Frames", spriteUnitPair.Value.Count.ToString());

					animClipNode.AppendChild(animClipElement);


					spriteUnitList = spriteUnitPair.Value;
					for (int iUnit = 0; iUnit < spriteUnitList.Count; iUnit++)
					{
						unit = spriteUnitList[iUnit];

						//각각 프레임에 따라 Frame Element를 넣자.
						//주의 : isSequenceFile여부에 따라 데이터가 다르다.
						//Spritesheet : AnimClip 인덱스, 프레임, X, Y, Width, Height, 
						//Sequence : AnimClip 인덱스, 프레임, 파일 이름

						XmlElement frameElement = xmlDoc.CreateElement("Image");
						frameElement.SetAttribute("AnimationClipIndex", animClipIndex.ToString());
						frameElement.SetAttribute("Frame", unit._frameIndex.ToString());

						if(isSequenceFiles)
						{
							frameElement.SetAttribute("FileName", unit._sequenceFileName);
						}
						else
						{
							frameElement.SetAttribute("SpriteIndex", unit._bakeTextureIndex.ToString());
							frameElement.SetAttribute("X", unit._bakePosX.ToString());
							frameElement.SetAttribute("Y", ((spriteImageHeight - unit._bakePosY) - unit._bakeHeight).ToString());
							frameElement.SetAttribute("Width", unit._bakeWidth.ToString());
							frameElement.SetAttribute("Height", unit._bakeHeight.ToString());
						}

						frameImageNode.AppendChild(frameElement);
					}
					
				}

				xmlDoc.Save(xmlFilePath);
				
			}
			catch(Exception ex)
			{
				Debug.LogError("ExportToXML Exception : " + ex);
			}
		}

		private void ExportToJSON(string filePath, Dictionary<apAnimClip, List<SpriteUnit>> spriteUnits, bool isSequenceFiles, int spriteImageHeight)
		{
			string jsonFilePath = filePath.Substring(0, filePath.Length - 4) + "__JSON.json";
			FileStream fs = null;
			StreamWriter sw = null;
				 
			try
			{
				//이전
				//fs = new FileStream(jsonFilePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(jsonFilePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				//AnimtionClip이 먼저
				//인덱스, 이름, FPS, Frame개수, Loop 여부
				//FrameImage
				//Spritesheet : AnimClip 인덱스, 프레임, X, Y, Width, Height, 
				//Sequence : AnimClip 인덱스, 프레임, 파일 이름
				Dictionary<apAnimClip, int> animClipAndIndex = new Dictionary<apAnimClip, int>();
				
				apAnimClip animClip = null;
				List<SpriteUnit> spriteUnitList = null;
				SpriteUnit unit = null;
				int curAnimClipIndex = 0;

				List<string> strAnimClipBodyList = new List<string>();
				List<string> strFrameBodyList = new List<string>();

				foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> spriteUnitPair in spriteUnits)
				{
					animClip = spriteUnitPair.Key;
					spriteUnitList = spriteUnitPair.Value;
					//먼저 AnimClip Element를 추가하자
					if (!animClipAndIndex.ContainsKey(animClip))
					{
						animClipAndIndex.Add(animClip, curAnimClipIndex);
						curAnimClipIndex++;
					}
					int animClipIndex = animClipAndIndex[animClip];

					//AnimClip 정보 추가
					string animClipBody = "\t{ ";
					animClipBody += "\"Index\":" + animClipIndex + ", ";
					animClipBody += "\"Name\":\"" + animClip._name + "\", ";
					animClipBody += "\"FPS\":" + animClip.FPS + ", ";
					animClipBody += "\"Frames\":" + spriteUnitPair.Value.Count + " }";
					strAnimClipBodyList.Add(animClipBody);

					for (int iUnit = 0; iUnit < spriteUnitList.Count; iUnit++)
					{
						unit = spriteUnitList[iUnit];

						//각각 프레임에 따라 Frame Element를 넣자.
						//주의 : isSequenceFile여부에 따라 데이터가 다르다.
						//Spritesheet : AnimClip 인덱스, 프레임, X, Y, Width, Height, 
						//Sequence : AnimClip 인덱스, 프레임, 파일 이름

						string frameBody = "\t{ ";
						frameBody += "\"AnimationClipIndex\":" + animClipIndex + ", ";
						frameBody += "\"Frame\":" + unit._frameIndex + ", ";
						if(isSequenceFiles)
						{
							frameBody += "\"FileName\":\"" + unit._sequenceFileName + "\"";
						}
						else
						{
							frameBody += "\"SpriteIndex\":" + unit._bakeTextureIndex + ", ";
							frameBody += "\"X\":" + unit._bakePosX + ", ";
							frameBody += "\"Y\":" + ((spriteImageHeight - unit._bakePosY) - unit._bakeHeight) + ", ";
							frameBody += "\"Width\":" + unit._bakeWidth + ", ";
							frameBody += "\"Height\":" + unit._bakeHeight;
						}
						frameBody += " }";
						strFrameBodyList.Add(frameBody);
					}
				}

				sw.WriteLine("{");

				//AnimationClip Body들
				sw.WriteLine("\t\"AnimationClip\":[");
				for (int i = 0; i < strAnimClipBodyList.Count; i++)
				{
					if(i < strAnimClipBodyList.Count - 1)
					{
						sw.WriteLine(strAnimClipBodyList[i] + ",");
					}
					else
					{
						sw.WriteLine(strAnimClipBodyList[i]);
					}
				}
				sw.WriteLine("\t],");

				//Frame Body들
				sw.WriteLine("\t\"FrameImage\":[");
				for (int i = 0; i < strFrameBodyList.Count; i++)
				{
					if(i < strFrameBodyList.Count - 1)
					{
						sw.WriteLine(strFrameBodyList[i] + ",");
					}
					else
					{
						sw.WriteLine(strFrameBodyList[i]);
					}
				}
				sw.WriteLine("\t]");
				sw.WriteLine("}");

				sw.Flush();

				sw.Close();
				fs.Close();
				sw = null;
				fs = null;
			}
			catch(Exception ex)
			{
				Debug.LogError("ExportToJSON Exception : " + ex);

				if(sw != null)
				{
					sw.Close();
				}
				if(fs != null)
				{
					fs.Close();
				}
				sw = null;
				fs = null;
			}
		}

		private void ExportToTXT(string filePath, Dictionary<apAnimClip, List<SpriteUnit>> spriteUnits, bool isSequenceFiles, int spriteImageHeight)
		{
			string txtFilePath = filePath.Substring(0, filePath.Length - 4) + "__TXT.txt";
			FileStream fs = null;
			StreamWriter sw = null;
				 
			try
			{
				//이전
				//fs = new FileStream(txtFilePath, FileMode.Create, FileAccess.Write);
				//sw = new StreamWriter(fs);

				//변경 21.7.3 : 경로 + 인코딩 문제
				fs = new FileStream(apUtil.ConvertEscapeToPlainText(txtFilePath), FileMode.Create, FileAccess.Write);
				sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

				//AnimtionClip이 먼저
				//인덱스, 이름, FPS, Frame개수, Loop 여부
				//FrameImage
				//Spritesheet : AnimClip 인덱스, 프레임, X, Y, Width, Height, 
				//Sequence : AnimClip 인덱스, 프레임, 파일 이름
				Dictionary<apAnimClip, int> animClipAndIndex = new Dictionary<apAnimClip, int>();
				
				apAnimClip animClip = null;
				List<SpriteUnit> spriteUnitList = null;
				SpriteUnit unit = null;
				int curAnimClipIndex = 0;

				List<string> strAnimClipBodyList = new List<string>();
				List<string> strFrameBodyList = new List<string>();

				foreach (KeyValuePair<apAnimClip, List<SpriteUnit>> spriteUnitPair in spriteUnits)
				{
					animClip = spriteUnitPair.Key;
					spriteUnitList = spriteUnitPair.Value;
					//먼저 AnimClip Element를 추가하자
					if (!animClipAndIndex.ContainsKey(animClip))
					{
						animClipAndIndex.Add(animClip, curAnimClipIndex);
						curAnimClipIndex++;
					}
					int animClipIndex = animClipAndIndex[animClip];

					//AnimClip 정보 추가
					string animClipBody = animClipIndex + "\t" + animClip._name + "\t\t" + animClip.FPS + "\t" + spriteUnitPair.Value.Count;
					strAnimClipBodyList.Add(animClipBody);

					for (int iUnit = 0; iUnit < spriteUnitList.Count; iUnit++)
					{
						unit = spriteUnitList[iUnit];

						//각각 프레임에 따라 Frame Element를 넣자.
						//주의 : isSequenceFile여부에 따라 데이터가 다르다.
						//Spritesheet : AnimClip 인덱스, 프레임, X, Y, Width, Height, 
						//Sequence : AnimClip 인덱스, 프레임, 파일 이름

						string frameBody = animClipIndex + "\t\t" + unit._frameIndex + "\t";
						if(isSequenceFiles)
						{
							frameBody += unit._sequenceFileName;
						}
						else
						{
							frameBody += unit._bakeTextureIndex + "\t\t";
							frameBody += unit._bakePosX + "\t";
							frameBody += ((spriteImageHeight - unit._bakePosY) - unit._bakeHeight) + "\t";
							frameBody += unit._bakeWidth + "\t";
							frameBody += unit._bakeHeight;
						}
						strFrameBodyList.Add(frameBody);
					}
				}

				//AnimationClip Body들
				sw.WriteLine("=========================================================================");
				sw.WriteLine("                      AnyPortrait Sprite Sheet Data                      ");
				sw.WriteLine("=========================================================================");
				sw.WriteLine("AnimationClip : " + strAnimClipBodyList.Count);
				sw.WriteLine("Index\tName\t\tFPS\tFrames");
				for (int i = 0; i < strAnimClipBodyList.Count; i++)
				{
					sw.WriteLine(strAnimClipBodyList[i]);
				}
				sw.WriteLine("=========================================================================");
				sw.WriteLine("FrameImage : " + strFrameBodyList.Count);
				if(isSequenceFiles)
				{
					sw.WriteLine("ClipIndex\tFrame\tFileName");
				}
				else
				{
					sw.WriteLine("ClipIndex\tFrame\tSpriteIndex\tX\tY\tWidth\tHeight");
				}
				for (int i = 0; i < strFrameBodyList.Count; i++)
				{
					sw.WriteLine(strFrameBodyList[i]);
				}
				sw.WriteLine("=========================================================================");
				
				sw.Flush();
				
				sw.Close();
				fs.Close();
				sw = null;
				fs = null;
			}
			catch(Exception ex)
			{
				Debug.LogError("ExportToJSON Exception : " + ex);

				if(sw != null)
				{
					sw.Close();
				}
				if(fs != null)
				{
					fs.Close();
				}
				sw = null;
				fs = null;
			}
		}
	}
}