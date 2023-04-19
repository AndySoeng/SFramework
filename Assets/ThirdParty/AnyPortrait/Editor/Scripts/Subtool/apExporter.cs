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
using NGIFforUnity;

#if UNITY_2017_4_OR_NEWER
using UnityEditor.Media;
#endif

namespace AnyPortrait
{

	/// <summary>
	/// Editor에 포함되어서 Export를 담당한다.
	/// Texture Render / GIF Export / 백업용 Txt
	
	/// </summary>
	public class apExporter
	{
		// Members
		//----------------------------------------------
		private apEditor _editor = null;
		private RenderTexture _renderTexture = null;
		private RenderTexture _renderTexture_GrayscaleAlpha = null;

		private apGL.WindowParameters _glWindowParam = new apGL.WindowParameters();
		
		//Step 전용 변수
		private apNGIFforUnity _ngif = new apNGIFforUnity();
		private string _gif_FilePath = "";
		
		public string GIF_FilePath { get { return _gif_FilePath; } }

		private FileStream _gifFileStream = null;

		//추가 : 11.5
#if UNITY_2017_4_OR_NEWER
		private MediaEncoder _mediaEncoder = null;
		private VideoTrackAttributes _videoAttr;

		private int _dstMP4SizeWidth = 0;
		private int _dstMP4SizeHeight = 0;
#endif

		// Init
		//----------------------------------------------
		public apExporter(apEditor editor)
		{
			_editor = editor;
			
		}

		public void Clear()
		{
#if UNITY_2017_4_OR_NEWER
			if(_mediaEncoder != null)
			{
				try
				{
					_mediaEncoder.Dispose();
					_mediaEncoder = null;
				}
				catch(Exception ex)
				{
					Debug.LogError("AnyPortrait : MovieExporter Clear Exception : " + ex);
				}
				_mediaEncoder = null;
			}
#endif
		}

		// Functions
		//----------------------------------------------
		public Texture2D RenderToTexture(apMeshGroup meshGroup,
										int winPosX, int winPosY,
										int srcSizeWidth, int srcSizeHeight,
										int dstSizeWidth, int dstSizeHeight,
										Color clearColor)
		{
			if (_editor == null)
			{
				return null;
			}

			// 1. 렌더링을 한다.
			//--------------------------------------------------------------------

			//기존 방식 : 전체 화면을 기준으로 모두 렌더링을 하고(>RenderTexture) -> Texture2D에 클리핑해서 적용한다.
			//변경 방식 : 미리 클리핑 영역으로 화면 포커스를 이동한 후, Dst만큼 확대한다 -> 그대로 Texture2D에 적용한다.
			//apGL의 Window Size를 바꾸어준다.

			//-------------------->> 기존 방식

			#region [미사용 코드]
			//int rtSizeWidth = ((int)_editor.position.width);
			//int rtSizeHeight = ((int)_editor.position.height);

			////winPosY -= 10;
			//int guiOffsetX = apGL._posX_NotCalculated;
			//int guiOffsetY = apGL._posY_NotCalculated;

			//int clipPosX = winPosX - (srcSizeWidth / 2);
			//int clipPosY = winPosY - (srcSizeHeight / 2);

			//clipPosX += guiOffsetX;
			//clipPosY += guiOffsetY + 15;

			//int clipPosX_Right = clipPosX + srcSizeWidth;
			//int clipPosY_Bottom = clipPosY + srcSizeHeight;

			//if (clipPosX < 0)		{ clipPosX = 0; }
			//if (clipPosY < 0)		{ clipPosY = 0; }
			//if (clipPosX_Right > rtSizeWidth)	{ clipPosX_Right = rtSizeWidth; }
			//if (clipPosY_Bottom > rtSizeHeight)	{ clipPosY_Bottom = rtSizeHeight; }

			//int clipWidth = (clipPosX_Right - clipPosX);
			//int clipHeight = (clipPosY_Bottom - clipPosY);
			//if (clipWidth <= 0 || clipHeight <= 0)
			//{
			//	Debug.LogError("RenderToTexture Failed : Clip Area is over Screen");
			//	return null;
			//}

			//meshGroup.RefreshForce();
			//meshGroup.UpdateRenderUnits(0.0f, true);



			////Pass-1. 일반 + MaskParent를 Alpha2White 렌더링. 이걸로 나중에 알파 채널용 텍스쳐를 만든다.
			////--------------------------------------------------------------------------------------------------------
			//_renderTexture_GrayscaleAlpha = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture_GrayscaleAlpha.antiAliasing = 1;
			//_renderTexture_GrayscaleAlpha.wrapMode = TextureWrapMode.Clamp;

			//RenderTexture.active = null;
			//RenderTexture.active = _renderTexture_GrayscaleAlpha;

			////기본 
			//Color maskClearColor = new Color(clearColor.a, clearColor.a, clearColor.a, 1.0f);
			//GL.Clear(false, true, maskClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			//apGL.DrawBoxGL(Vector2.zero, 50000, 50000, maskClearColor, false, true);//<<이걸로 배경을 깔자
			//GL.Flush();

			////System.Threading.Thread.Sleep(50);

			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
			//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			//	{
			//		if (renderUnit._meshTransform != null)
			//		{
			//			if (renderUnit._meshTransform._isClipping_Parent)
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					//RenderTexture.active = _renderTexture_GrayscaleAlpha;
			//					apGL.DrawRenderUnit_Basic_Alpha2White(renderUnit);
			//				}
			//			}
			//			else if (renderUnit._meshTransform._isClipping_Child)
			//			{
			//				//Pass
			//				//Alpha 렌더링에서 Clipping Child는 제외한다. 어차피 Parent의 Alpha보다 많을 수 없으니..
			//			}
			//			else
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					//RenderTexture.active = _renderTexture_GrayscaleAlpha;
			//					apGL.DrawRenderUnit_Basic_Alpha2White(renderUnit);
			//				}
			//			}
			//		}
			//	}
			//}

			//System.Threading.Thread.Sleep(5);

			//Texture2D resultTex_SrcSize_Alpha = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			//resultTex_SrcSize_Alpha.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);
			//resultTex_SrcSize_Alpha.Apply();


			////Pass-2. 기본 렌더링
			////--------------------------------------------------------------------------------------------------------
			////1. Clip Parent의 MaskTexture를 미리 구워서 Dictionary에 넣는다.
			//Dictionary<apRenderUnit, Texture2D> bakedClipMaskTextures = new Dictionary<apRenderUnit, Texture2D>();


			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
			//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			//	{
			//		if (renderUnit._meshTransform != null)
			//		{
			//			if (renderUnit._meshTransform._isClipping_Parent)
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					Texture2D clipMaskTex = apGL.GetMaskTexture_ClippingParent(renderUnit);
			//					if (clipMaskTex != null)
			//					{
			//						bakedClipMaskTextures.Add(renderUnit, clipMaskTex);
			//					}
			//					else
			//					{
			//						Debug.LogError("Clip Testure Bake Failed");
			//					}

			//				}
			//			}
			//		}
			//	}
			//}

			//System.Threading.Thread.Sleep(5);


			//_renderTexture = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture.antiAliasing = 1;
			//_renderTexture.wrapMode = TextureWrapMode.Clamp;

			//RenderTexture.active = null;
			//RenderTexture.active = _renderTexture;

			//Color opaqueClearColor = new Color(clearColor.r * clearColor.a, clearColor.g * clearColor.a, clearColor.b * clearColor.a, 1.0f);

			////GL.Clear(true, true, clearColor, -100.0f);//이전
			//GL.Clear(false, true, opaqueClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			//apGL.DrawBoxGL(Vector2.zero, 50000, 50000, opaqueClearColor, false, true);//<<이걸로 배경을 깔자
			//GL.Flush();

			////System.Threading.Thread.Sleep(50);

			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];
			//	if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			//	{
			//		if (renderUnit._meshTransform != null)
			//		{
			//			if (renderUnit._meshTransform._isClipping_Parent)
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					if (bakedClipMaskTextures.ContainsKey(renderUnit))
			//					{
			//						apGL.DrawRenderUnit_ClippingParent_Renew_WithoutRTT(renderUnit,
			//									renderUnit._meshTransform._clipChildMeshes,
			//									bakedClipMaskTextures[renderUnit]);
			//					}


			//					////RenderTexture.active = _renderTexture;//<<클리핑 뒤에는 다시 연결해줘야한다.
			//				}
			//			}
			//			else if (renderUnit._meshTransform._isClipping_Child)
			//			{
			//				//Pass
			//			}
			//			else
			//			{
			//				if (renderUnit._isVisible)
			//				{
			//					RenderTexture.active = _renderTexture;
			//					apGL.DrawRenderUnit_Basic(renderUnit);
			//				}
			//			}
			//		}
			//	}
			//}

			//System.Threading.Thread.Sleep(5);


			//Texture2D resultTex_SrcSize = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			//resultTex_SrcSize.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);

			//resultTex_SrcSize.Apply(); 
			#endregion

			//--------------------<< 기존 방식
			

			//-------------------->> 새로운 방식2

			apGL.GetWindowParameters(_glWindowParam);

			int rtSizeWidth = ((int)_editor.position.width);
			int rtSizeHeight = ((int)_editor.position.height);

			//winPosY -= 10;
			int guiOffsetX = apGL._posX_NotCalculated;
			int guiOffsetY = apGL._posY_NotCalculated;

			int clipPosX = winPosX - (srcSizeWidth / 2);
			int clipPosY = winPosY - (srcSizeHeight / 2);

			clipPosX += guiOffsetX;
			clipPosY += guiOffsetY + 15;

			int clipPosX_Right = clipPosX + srcSizeWidth;
			int clipPosY_Bottom = clipPosY + srcSizeHeight;

			if (clipPosX < 0)	{ clipPosX = 0; }
			if (clipPosY < 0)	{ clipPosY = 0; }
			if (clipPosX_Right > rtSizeWidth)	{ clipPosX_Right = rtSizeWidth; }
			if (clipPosY_Bottom > rtSizeHeight)	{ clipPosY_Bottom = rtSizeHeight; }

			int clipWidth = (clipPosX_Right - clipPosX);
			int clipHeight = (clipPosY_Bottom - clipPosY);
			if (clipWidth <= 0 || clipHeight <= 0)
			{
				Debug.LogError("RenderToTexture Failed : Clip Area is over Screen");
				return null;
			}

			clipPosY += 2;
			clipHeight = (int)(clipHeight * 0.979f);//<<TODO 이게 비율인지 절대값인지 확인 필요
			srcSizeHeight = (int)(srcSizeHeight * 0.979f);

			if(dstSizeWidth != srcSizeWidth || dstSizeHeight != srcSizeHeight)
			{
				float resizeRatio_X = (float)dstSizeWidth / (float)srcSizeWidth;
				float resizeRatio_Y = (float)dstSizeHeight / (float)srcSizeHeight;
				float resizeRatio = 1.0f;
				if(resizeRatio_X > 1.0f || resizeRatio_Y > 1.0f)
				{
					//하나라도 증가하는 방향이면, 더 크게 확대하는 방향으로 설정
					resizeRatio = Mathf.Max(resizeRatio_X, resizeRatio_Y, 1.0f);
				}
				else
				{
					//둘다 감소(혹은 유지)라면 더 많이 축소하는 방향으로 설정
					resizeRatio = Mathf.Min(resizeRatio_X, resizeRatio_Y, 1.0f);
				}
				
				//Debug.Log("Resize Ratio : " + resizeRatio);

				//크기 제한이 있다.
				//너무 크거나 너무 작아진다면 제한해야한다.
				int expectedRtSizeWidth = (int)(rtSizeWidth * resizeRatio);
				int expectedRtSizeHeight = (int)(rtSizeHeight * resizeRatio);
				if(resizeRatio > 1.0f)
				{
					float limitedRatioX = resizeRatio;
					float limitedRatioY = resizeRatio;
					bool isSizeLimited = false;
					if(expectedRtSizeWidth > 4096)
					{
						//Debug.Log("Width is Over 4096");
						limitedRatioX = (4096.0f / (float)rtSizeWidth);
						isSizeLimited = true;
					}
					if (expectedRtSizeHeight > 4096)
					{
						//Debug.Log("Height is Over 4096");
						limitedRatioY = (4096.0f / (float)rtSizeHeight);
						isSizeLimited = true;
					}

					if (isSizeLimited)
					{
						//확대되어 제한된 만큼, 작은 쪽으로 ResizeRatio를 맞춘다.
						resizeRatio = Mathf.Min(limitedRatioX, limitedRatioY);
						//Debug.Log("Limited Ratio : " + resizeRatio);
					}
				}
				else if(resizeRatio < 1.0f)
				{
					float limitedRatioX = resizeRatio;
					float limitedRatioY = resizeRatio;
					bool isSizeLimited = false;

					if(expectedRtSizeWidth < 256)
					{
						//Debug.Log("Width is Under 256");
						limitedRatioX = (256.0f / (float)rtSizeWidth);
						isSizeLimited = true;
					}
					if (expectedRtSizeHeight < 256)
					{
						//Debug.Log("Height is Under 256");
						limitedRatioY = (256.0f / (float)rtSizeHeight);
						isSizeLimited = true;
					}
					if (isSizeLimited)
					{
						//축소되어 제한된 만큼, 큰 쪽으로 ResizeRatio를 맞춘다.
						resizeRatio = Mathf.Max(limitedRatioX, limitedRatioY);
						//Debug.Log("Limited Ratio : " + resizeRatio);
					}
				}
				

				//Debug.Log("Rescale Ratio : " + resizeRatio);

				Vector2 scrollPos_Prev = apGL._windowScroll;
				Vector2 scrollPos_InEditor = scrollPos_Prev + new Vector2(apGL._posX_NotCalculated, apGL._posY_NotCalculated) + apGL.WindowSizeHalf;
				scrollPos_InEditor *= resizeRatio;
				scrollPos_InEditor -= (new Vector2(apGL._posX_NotCalculated, apGL._posY_NotCalculated) + apGL.WindowSizeHalf);

				//apGL._windowScroll = scrollPos_InEditor;
				//apGL._zoom /= resizeRatio;

				//apGL._windowWidth = (int)(apGL._windowWidth * resizeRatio);
				//apGL._windowHeight = (int)(apGL._windowHeight * resizeRatio);
				apGL.SetScreenClippingSizeTmp(new Vector4(-100, -100, 200, 200));
				rtSizeWidth = (int)(rtSizeWidth * resizeRatio);
				rtSizeHeight = (int)(rtSizeHeight * resizeRatio);
				apGL._posX_NotCalculated = (int)(apGL._posX_NotCalculated * resizeRatio);
				apGL._posY_NotCalculated = (int)(apGL._posY_NotCalculated * resizeRatio);

				clipPosX = (int)(clipPosX * resizeRatio);
				clipPosY = (int)(clipPosY * resizeRatio);
				clipWidth = (int)(clipWidth * resizeRatio);
				clipHeight = (int)(clipHeight * resizeRatio);

				srcSizeWidth = (int)(srcSizeWidth * resizeRatio);
				srcSizeHeight = (int)(srcSizeHeight * resizeRatio);
			}


			meshGroup.RefreshForce();
			meshGroup.UpdateRenderUnits(0.0f, true);

			//int newRtSizeWidth = GetProperRenderTextureSize(rtSizeWidth);
			//int newRtSizeHeight = GetProperRenderTextureSize(rtSizeHeight);
			//clipPosX += (newRtSizeWidth - rtSizeWidth) / 2;
			//clipPosY += (newRtSizeHeight - rtSizeHeight) / 2;
			//rtSizeWidth = newRtSizeWidth;
			//rtSizeHeight = newRtSizeHeight;

			//Pass-1. 일반 + MaskParent를 Alpha2White 렌더링. 이걸로 나중에 알파 채널용 텍스쳐를 만든다.
			//--------------------------------------------------------------------------------------------------------
			_renderTexture_GrayscaleAlpha = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture_GrayscaleAlpha.antiAliasing = 1;
			_renderTexture_GrayscaleAlpha.isPowerOfTwo = false;
			_renderTexture_GrayscaleAlpha.wrapMode = TextureWrapMode.Clamp;

			RenderTexture.active = null;
			RenderTexture.active = _renderTexture_GrayscaleAlpha;

			//기본 
			Color maskClearColor = new Color(clearColor.a, clearColor.a, clearColor.a, 1.0f);
			GL.Clear(false, true, maskClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, maskClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			//System.Threading.Thread.Sleep(50);

			//변경
			List<apRenderUnit> renderUnits = meshGroup.SortedBuffer.SortedRenderUnits;
			int nRenderUnits = renderUnits.Count;

			//기존
			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];

			//변경 19.11.23 : ExtraOption-Depth에 의한 순서 변경도 적용
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White_ForExport(renderUnit);
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
							//Alpha 렌더링에서 Clipping Child는 제외한다. 어차피 Parent의 Alpha보다 많을 수 없으니..
						}
						else
						{
							if (renderUnit._isVisible)
							{
								//RenderTexture.active = _renderTexture_GrayscaleAlpha;
								apGL.DrawRenderUnit_Basic_Alpha2White_ForExport(renderUnit);
							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);

			Texture2D resultTex_SrcSize_Alpha = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			resultTex_SrcSize_Alpha.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);
			resultTex_SrcSize_Alpha.Apply();


			//Pass-2. 기본 렌더링
			//--------------------------------------------------------------------------------------------------------
			//1. Clip Parent의 MaskTexture를 미리 구워서 Dictionary에 넣는다.
			Dictionary<apRenderUnit, Texture2D> bakedClipMaskTextures = new Dictionary<apRenderUnit, Texture2D>();

			

			//기존
			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];

			//변경 19.11.23 : ExtraOption-Depth인 경우 렌더링 순서가 바뀌어야한다.
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								Texture2D clipMaskTex = apGL.GetMaskTexture_ClippingParent(renderUnit);
								if (clipMaskTex != null)
								{
									bakedClipMaskTextures.Add(renderUnit, clipMaskTex);
								}
								else
								{
									Debug.LogError("Clip Testure Bake Failed");
								}

							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);


			_renderTexture = RenderTexture.GetTemporary(rtSizeWidth, rtSizeHeight, 8, RenderTextureFormat.ARGB32);
			//_renderTexture.antiAliasing = 1;
			_renderTexture.isPowerOfTwo = false;
			_renderTexture.wrapMode = TextureWrapMode.Clamp;

			RenderTexture.active = null;
			RenderTexture.active = _renderTexture;

			Color opaqueClearColor = new Color(clearColor.r * clearColor.a, clearColor.g * clearColor.a, clearColor.b * clearColor.a, 1.0f);

			//GL.Clear(true, true, clearColor, -100.0f);//이전
			GL.Clear(false, true, opaqueClearColor, 0.0f);//변경 : Mac에서도 작동 하려면..
			apGL.DrawBoxGL(Vector2.zero, 50000, 50000, opaqueClearColor, false, true);//<<이걸로 배경을 깔자
			GL.Flush();

			//System.Threading.Thread.Sleep(50);

			//기존
			//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)
			//{
			//	apRenderUnit renderUnit = meshGroup._renderUnits_All[iUnit];

			//변경 19.11.23 : ExtraOption-Depth인 경우 렌더링 순서가 바뀌어야한다.
			for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
			{
				apRenderUnit renderUnit = renderUnits[iUnit];

				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
				{
					if (renderUnit._meshTransform != null)
					{
						if (renderUnit._meshTransform._isClipping_Parent)
						{
							if (renderUnit._isVisible)
							{
								if (bakedClipMaskTextures.ContainsKey(renderUnit))
								{
									apGL.DrawRenderUnit_ClippingParent_Renew_WithoutRTT(renderUnit,
												renderUnit._meshTransform._clipChildMeshes,
												bakedClipMaskTextures[renderUnit]);
								}


								////RenderTexture.active = _renderTexture;//<<클리핑 뒤에는 다시 연결해줘야한다.
							}
						}
						else if (renderUnit._meshTransform._isClipping_Child)
						{
							//Pass
						}
						else
						{
							if (renderUnit._isVisible)
							{
								RenderTexture.active = _renderTexture;
								apGL.DrawRenderUnit_Basic_ForExport(renderUnit);
							}
						}
					}
				}
			}

			System.Threading.Thread.Sleep(5);


			Texture2D resultTex_SrcSize = new Texture2D(srcSizeWidth, srcSizeHeight, TextureFormat.ARGB32, false);
			resultTex_SrcSize.ReadPixels(new Rect(clipPosX, clipPosY, clipWidth, clipHeight), 0, 0);

			resultTex_SrcSize.Apply();

			//윈도우 크기 복구
			apGL.RecoverWindowSize(_glWindowParam);
			//--------------------<< 새로운 방식2


			RenderTexture.active = null;

			RenderTexture.ReleaseTemporary(_renderTexture_GrayscaleAlpha);
			RenderTexture.ReleaseTemporary(_renderTexture);
			
			// 2. 렌더링된 이미지를 가공한다.
			//--------------------------------------------------------------------


			_renderTexture = null;
			_renderTexture_GrayscaleAlpha = null;
			Texture2D resultTex_DstSize = null;

			//추가 : 가장자리 알파 문제를 수정하자
			//int blurSize = Mathf.Max(4, (dstSizeWidth / srcSizeWidth) + 1, (dstSizeHeight / srcSizeHeight) + 1);

			//Texture2D resultTex_FixAlphaBorder = MakeBlurImage(resultTex_SrcSize, resultTex_SrcSize_Alpha, blurSize);//<<이거 빼자

			if (dstSizeWidth != srcSizeWidth || dstSizeHeight != srcSizeHeight)
			{
				//기존 Resize
				//Texture2D resultTex_DstSize = new Texture2D(dstSizeWidth, dstSizeHeight, TextureFormat.ARGB32, false);
				//Color color_RGB = Color.black;
				//Color color_A = Color.black;

				//for (int iY = 0; iY < dstSizeHeight; iY++)
				//{
				//	for (int iX = 0; iX < dstSizeWidth; iX++)
				//	{
				//		float u = (float)iX / (float)dstSizeWidth;
				//		float v = (float)iY / (float)dstSizeHeight;

				//		color_RGB = resultTex_SrcSize.GetPixelBilinear(u, v);
				//		color_A = resultTex_SrcSize_Alpha.GetPixelBilinear(u, v);
				//		resultTex_DstSize.SetPixel(iX, iY, new Color(color_RGB.r, color_RGB.g, color_RGB.b, color_A.r));
				//		//resultTex_DstSize.SetPixel(iX, iY, new Color(color_A.r, color_A.g, color_A.b, 1));
				//	}
				//}
				//resultTex_DstSize.Apply();
				if(SystemInfo.supportsComputeShaders)
				{
					resultTex_DstSize = ResizeTextureWithComputeShader(resultTex_SrcSize, resultTex_SrcSize_Alpha, /*resultTex_FixAlphaBorder,*/ dstSizeWidth, dstSizeHeight);
				}

				if(resultTex_DstSize == null)
				{
					resultTex_DstSize = ResizeTexture(resultTex_SrcSize, resultTex_SrcSize_Alpha, /*resultTex_FixAlphaBorder,*/ dstSizeWidth, dstSizeHeight);
				}
				
				
			}
			else
			{
				
				if(SystemInfo.supportsComputeShaders)
				{
					resultTex_DstSize = MergeAlphaChannelWithComputeShader(resultTex_SrcSize, resultTex_SrcSize_Alpha);
				}
				if(resultTex_DstSize == null)
				{
					resultTex_DstSize = MergeAlphaChannel(resultTex_SrcSize, resultTex_SrcSize_Alpha/*, resultTex_FixAlphaBorder*/);
				}
				
			}

			System.Threading.Thread.Sleep(5);
			//기존 크기의 이미지는 삭제
			UnityEngine.Object.DestroyImmediate(resultTex_SrcSize);
			UnityEngine.Object.DestroyImmediate(resultTex_SrcSize_Alpha);
			//UnityEngine.Object.DestroyImmediate(resultTex_FixAlphaBorder);

			return resultTex_DstSize;

		}


		public bool SaveTexture2DToPNG(Texture2D srcTexture2D, string filePathWithExtension, bool isAutoDestroy)
		{
			try
			{
				if (srcTexture2D == null)
				{
					return false;
				}

				File.WriteAllBytes(filePathWithExtension + ".png", srcTexture2D.EncodeToPNG());

				if (isAutoDestroy)
				{
					UnityEngine.Object.DestroyImmediate(srcTexture2D);
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("SaveTexture2DToPNG Exception : " + ex);

				if (isAutoDestroy)
				{
					UnityEngine.Object.Destroy(srcTexture2D);
				}
				return false;
			}
		}

		//------------------------------------------------------------------------------------------
		public bool MakeGIFHeader(	string filePath,
									apAnimClip animClip,
									int dstSizeWidth, int dstSizeHeight)
		{
			//일단 파일 스트림을 꺼준다.
			if(_gifFileStream != null)
			{
				try
				{
					_gifFileStream.Close();
				}
				catch(Exception) { }
				_gifFileStream = null;
			}

			float secPerFrame = 1.0f / (float)animClip.FPS;

			//파일 스트림을 만들고 GIF 헤더 작성
			try
			{
				_gifFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

				_ngif.WriteHeader(_gifFileStream);
				_ngif.SetGIFSetting((int)((secPerFrame * 100.0f) + 0.5f), 0, dstSizeWidth, dstSizeHeight);
			}
			catch(Exception)
			{
				if(_gifFileStream != null)
				{
					_gifFileStream.Close();
				}
				_gifFileStream = null;
				return false;
			}

			return true;
		}


		public bool AddGIFFrame(Texture2D frameImage, bool isFirstFrame, int quality)
		{
			if(_gifFileStream == null)
			{
				//이미 처리가 끝났네요.
				return false;
			}

			try
			{
				_ngif.AddFrame(frameImage, _gifFileStream, isFirstFrame, quality);
			}
			catch(Exception)
			{

			}
			UnityEngine.Object.DestroyImmediate(frameImage);
			return true;
		}

		public void EndGIF()
		{
			if(_gifFileStream == null)
			{
				//이미 처리가 끝났네요.
				return;
			}

			try
			{
				_ngif.Finish(_gifFileStream);

				_gifFileStream.Close();
				_gifFileStream = null;
			}
			catch(Exception)
			{
				if (_gifFileStream != null)
				{
					_gifFileStream.Close();
				}
				_gifFileStream = null;
			}
		}
		


		// MP4 함수들
		//-------------------------------------------------------------------------------
		public bool MakeMP4Animation(	string filePath, 
										apAnimClip animClip,
										int dstSizeWidth, int dstSizeHeight
										)
		{
			Clear();

			if (_editor == null || _editor._portrait == null || animClip == null)
			{
				return false;
			}

#if UNITY_2017_4_OR_NEWER

			_dstMP4SizeWidth = dstSizeWidth;
			_dstMP4SizeHeight = dstSizeHeight;
			
			int frameRate = animClip.FPS;
			
			_videoAttr.frameRate = new MediaRational(frameRate);
			_videoAttr.width = (uint)_dstMP4SizeWidth;
			_videoAttr.height = (uint)_dstMP4SizeHeight;
			_videoAttr.includeAlpha = false;
			
#if UNITY_2018_1_OR_NEWER
			//2018에서는 더 고화질의 영상을 뽑을 수 있다.
			_videoAttr.bitRateMode = UnityEditor.VideoBitrateMode.High;
#endif

			try
			{
				_mediaEncoder = new MediaEncoder(filePath, _videoAttr);
			}
			catch(Exception)
			{
				Clear();
				return false;
			}

			return true;
#else
			return false;
#endif
		}


		public bool AddMP4Frame(Texture2D frameImage)
		{
#if UNITY_2017_4_OR_NEWER
			if(_mediaEncoder == null)
			{
				return false;
			}

			try
			{
				//사이즈는 이미 조절되서 들어온다.
				////사이즈가 안맞다면 리사이즈
				//if(frameImage.width != _dstMP4SizeWidth || 
				//	frameImage.height != _dstMP4SizeHeight)
				//{
				//	Texture2D newTex = ResizeTexture(frameImage, _dstMP4SizeWidth, _dstMP4SizeHeight);
				//	UnityEngine.Object.DestroyImmediate(frameImage);
				//	frameImage = newTex;
				//}
				
				//포맷을 바꿔야 한다.
				Texture2D newTex = new Texture2D(frameImage.width, frameImage.height, TextureFormat.RGBA32, false);
				newTex.SetPixels(frameImage.GetPixels());
				newTex.Apply();

				UnityEngine.Object.DestroyImmediate(frameImage);
				frameImage = newTex;

				_mediaEncoder.AddFrame(frameImage);
			}
			catch(Exception)
			{

			}
			UnityEngine.Object.DestroyImmediate(frameImage);
			return true;			
#else
			return false;
#endif
		}


		public void EndMP4()
		{
			Clear();
		}


		// Sub Functions
		//---------------------------------------------------------------------------------
		/// <summary>
		/// Fix를 위해서 Blur 이미지를 만든다.
		/// Fix용이므로 만약 Alpha 채널에 해당하는 값이라면 Blur에 포함되지 않는것이 특징
		/// 매우 강한 색상으로 블러가 된다.
		/// </summary>
		/// <param name="srcTex"></param>
		/// <param name="srcAlphaTex"></param>
		/// <param name="blurSize"></param>
		/// <returns></returns>
		private Texture2D MakeBlurImage(Texture2D srcTex, Texture2D srcAlphaTex, int blurSize)
		{
			
			Color[] srcColorArr = srcTex.GetPixels(0);
			Color[] srcAlphaArr = srcAlphaTex.GetPixels(0);

			float width = srcTex.width;
			float height = srcTex.height;
 
			//Make New
			Texture2D resultTex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
 
			//Make destination array
			int length = (int)width * (int)height;
			Color[] resultColorArr = new Color[length];
			float[] resultWeightArr = new float[length];
			for (int i = 0; i < length; i++)
			{
				resultColorArr[i] = Color.clear;
				resultWeightArr[i] = 0.0f;
			}


			if(blurSize < 2)
			{
				blurSize = 2;
			}

			//Color resultColor = Color.black;
			Color curColor = Color.black;
			float curAlpha = 0.0f;
			Color curSubColor = Color.black;
			int iSubTex = 0;

			//Dst -> Sample 방식이 아니라
			//Src -> 결과 누적 방식
			//조금 더 빠를 것이다.
			for (int i = 0; i < length; i++)
			{	
				float iX = (float)i % width;
				float iY = Mathf.Floor((float)i / width);

				curColor = srcColorArr[i];
				curAlpha = srcAlphaArr[i].r;

				//resultColor = Color.clear;

				if(curAlpha < 0.8f)
				{
					//0.5 이하는 블러처리 하지 않는다.
					//자기 자신에만 값 넣는다.
					resultColorArr[i] += curColor;
					resultWeightArr[i] += 1.0f;
				}
				else
				{
					//0.5 이상은 주변에 블러 샘플링을 넣어준다.
					int iX_A = (int)Mathf.Max(iX - blurSize, 0);
					int iX_B = (int)Mathf.Min(iX + blurSize, width - 1);
					int iY_A = (int)Mathf.Max(iY - blurSize, 0);
					int iY_B = (int)Mathf.Min(iY + blurSize, height - 1);

					for (int iSubX = iX_A; iSubX <= iX_B; iSubX++)
					{
						for (int iSubY = iY_A; iSubY <= iY_B; iSubY++)
						{
							iSubTex = iSubX + (int)(iSubY * width);
							resultColorArr[iSubTex] += curColor;
							resultWeightArr[iSubTex] += 1.0f;
						}
					}
				}
			}

			for (int i = 0; i < length; i++)
			{
				resultColorArr[i] /= resultWeightArr[i];
			}

			resultTex.SetPixels(resultColorArr);
			resultTex.Apply();

			
 
			//*** Return
			return resultTex;
		}



		public Texture2D ResizeTexture(	Texture2D srcColorTex, 
										Texture2D srcAlphaTex, 
										//Texture2D srcBlurTex, 
										int dstWidth, int dstHeight)
		{
			Color[] srcColorArr = srcColorTex.GetPixels(0);
			Color[] srcAlphaArr = srcAlphaTex.GetPixels(0);
			//Color[] srcBlurArr = srcBlurTex.GetPixels(0);

			int iSrcWidth = srcColorTex.width;
			int iSrcHeight = srcColorTex.height;

			float fSrcWidth = iSrcWidth;
			float fSrcHeight = iSrcHeight;
 
			//New Size
			float fDstWidth = dstWidth;
			float fDstHeight = dstHeight;
 
			//Make New
			Texture2D resultTex = new Texture2D(dstWidth, dstHeight, TextureFormat.ARGB32, false);
 
			//Make destination array
			int srcLength = srcColorTex.width * srcColorTex.height;
			int dstLength = dstWidth * dstHeight;

			Color[] resultColorArr = new Color[dstLength];
			float[] resultWeightArr = new float[dstLength];

			for (int i = 0; i < dstLength; i++)
			{
				resultColorArr[i] = Color.clear;
				resultWeightArr[i] = 0.0f;
			}
			
			
			Color resultColor = new Color();
			Color curColor;
			float curAlpha = 0.0f;

			//Offset_Src : 이미지가 작아질 때 1보다 크다
			//Offset_Dst : 이미지가 커질때 1보다 크다

			float offsetSrcX = fSrcWidth / fDstWidth;
			float offsetSrcY = fSrcHeight / fDstHeight;
			float offsetDstX = fDstWidth / fSrcWidth;
			float offsetDstY = fDstHeight / fSrcHeight;

			float offsetSrcX_Half = Mathf.Max(offsetSrcX * 0.5f, 0);
			float offsetSrcY_Half = Mathf.Max(offsetSrcY * 0.5f, 0);
			float offsetDstX_Half = Mathf.Max(offsetDstX * 0.5f, 0);
			float offsetDstY_Half = Mathf.Max(offsetDstY * 0.5f, 0);

			//이미지가 커질땐 조금 더 오버 샘플링을 해야한다.
			if (dstWidth > iSrcWidth)
			{
				//offsetSrcX_Half += 1;
				offsetDstX_Half += 1;
			}
			if (dstHeight > iSrcHeight)
			{
				//offsetSrcY_Half += 1;
				offsetDstY_Half += 1;
			}


			float maxDiff_SrcX = offsetSrcX_Half + 1;
			float maxDiff_SrcY = offsetSrcY_Half + 1;
			float maxDiff_DstX = offsetDstX_Half + 1;
			float maxDiff_DstY = offsetDstY_Half + 1;

			
			//Dst -> Sample에서
			//Src -> 결과 누적 방식으로 변경
			for (int iSrc = 0; iSrc < srcLength; iSrc++)
			{
				int iSrcX = iSrc % iSrcWidth;
				int iSrcY = iSrc / iSrcWidth;

				curColor = srcColorArr[iSrc];
				curAlpha = srcAlphaArr[iSrc].r;

				resultColor = curColor;
				resultColor.a = curAlpha;

				float fDstX_Expect = (iSrcX * fDstWidth) / fSrcWidth;
				float fDstY_Expect = (iSrcY * fDstHeight) / fSrcHeight;

				int iSrcX_Min = iSrcX - (int)offsetSrcX_Half;
				int iSrcY_Min = iSrcY - (int)offsetSrcY_Half;
				int iSrcX_Max = iSrcX + (int)offsetSrcX_Half;
				int iSrcY_Max = iSrcY + (int)offsetSrcY_Half;

				int iDstX_Min = (int)Mathf.Max(Mathf.Floor((iSrcX_Min * fDstWidth) / fSrcWidth - offsetDstX_Half), 0);
				int iDstX_Max = (int)Mathf.Min(Mathf.Ceil((iSrcX_Max * fDstWidth) / fSrcWidth + offsetDstX_Half), dstWidth - 1);
				int iDstY_Min = (int)Mathf.Max(Mathf.Floor((iSrcY_Min * fDstHeight) / fSrcHeight - offsetDstY_Half), 0);
				int iDstY_Max = (int)Mathf.Min(Mathf.Ceil((iSrcY_Max * fDstHeight) / fSrcHeight + offsetDstY_Half), dstHeight - 1);

				float fSrcX_Expect = 0.0f;
				float fSrcY_Expect = 0.0f;

				float dstDiffX = 0.0f;
				float dstDiffY = 0.0f;

				float srcDiffX = 0.0f;
				float srcDiffY = 0.0f;

				float srcWeight = 0.0f;
				float dstWeight = 0.0f;
				float curWeight = 0.0f;
				int iDst = 0;

				for (int iDstX = iDstX_Min; iDstX <= iDstX_Max; iDstX++)
				{
					for (int iDstY = iDstY_Min; iDstY <= iDstY_Max; iDstY++)
					{
						//1. dstDiff : 원래 이 Src가 원하던 Dst와 현재 Dst와의 차이
						//2. srdDiff : 현재 Dst의 Src와 현재 Src와의 차이
						fSrcX_Expect = (iDstX * fSrcWidth) / fDstWidth;
						fSrcY_Expect = (iDstY * fSrcHeight) / fDstHeight;

						dstDiffX = Mathf.Abs(fDstX_Expect - iDstX);
						dstDiffY = Mathf.Abs(fDstY_Expect - iDstY);

						srcDiffX = Mathf.Abs(fSrcX_Expect - iSrcX);
						srcDiffY = Mathf.Abs(fSrcY_Expect - iSrcY);

						//float srcWeight = Mathf.Pow(1.0f - (srcDiffX / maxDiff_SrcX), 2) * Mathf.Pow(1.0f - (srcDiffY / maxDiff_SrcY), 2);
						//float dstWeight = Mathf.Pow(1.0f - (dstDiffX / maxDiff_DstX), 2) * Mathf.Pow(1.0f - (dstDiffY / maxDiff_DstY), 2);
						srcWeight = (1.0f - (srcDiffX / maxDiff_SrcX)) * (1.0f - (srcDiffY / maxDiff_SrcY));
						dstWeight = (1.0f - (dstDiffX / maxDiff_DstX)) * (1.0f - (dstDiffY / maxDiff_DstY));

						curWeight = srcWeight * dstWeight * (curAlpha + 0.1f);//0.1은 Bias

						iDst = (iDstY * dstWidth) + iDstX;

						resultColorArr[iDst] += resultColor * curWeight;
						resultWeightArr[iDst] += curWeight;
					}
				}
			}

			

			for (int i = 0; i < dstLength; i++)
			{
				resultColorArr[i] /= resultWeightArr[i];
			}
			
			//*** Set Pixels
			resultTex.SetPixels(resultColorArr);
			resultTex.Apply();

			
			
			//*** Return
			return resultTex;
		}



		public Texture2D MergeAlphaChannel(	Texture2D srcColorTex, 
											Texture2D srcAlphaTex 
											//Texture2D srcBlurTex
											)
		{
 			Color[] srcColorArr = srcColorTex.GetPixels(0);
			Color[] srcAlphaArr = srcAlphaTex.GetPixels(0);
			//Color[] srcBlurArr = srcBlurTex.GetPixels(0);
			
			//New Size
			float width = srcColorTex.width;
			float height = srcColorTex.height;
 
			//Make New
			Texture2D resultTex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
 
			//Make destination array
			int length = (int)width * (int)height;
			Color[] resultColorArr = new Color[length];
 
			Color resultColor = Color.clear;
			Color curColor;
			//Color curBlur;
			float curAlpha = 0.0f;

			for (int i = 0; i < length; i++)
			{	
				resultColor = Color.clear;
				curColor = srcColorArr[i];
				curAlpha = srcAlphaArr[i].r;
				//curBlur = srcBlurArr[i];

				//if (curAlpha > 0.0f && curAlpha < 1.0f)
				//{
				//	resultColor = curBlur * (1.0f - curAlpha) + curColor * curAlpha;
				//	resultColor.a = curAlpha;
				//}
				//else
				//{
				//	resultColor = curColor;
				//	resultColor.a = curAlpha;
				//}
				resultColor = curColor;
				resultColor.a = curAlpha;

				//resultColor = curBlurTemp;

				resultColorArr[i] = resultColor;
			}
 
			//*** Set Pixels
			resultTex.SetPixels(resultColorArr);
			resultTex.Apply();
 
			//*** Return
			return resultTex;
		}
		

		private Texture2D BlurTextureWithComputeShader(	Texture2D srcColorTex, 
														Texture2D srcAlphaTex, int blurSize)
		{
			//추가 20.4.21
			string basePath = "Assets/AnyPortrait/";
			if(_editor != null)
			{
				basePath = apPathSetting.I.CurrentPath;
			}
			//ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/AnyPortrait/Editor/Scripts/Subtool/CShader/apCShader_Blur.compute");
			ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(basePath + "Editor/Scripts/Subtool/CShader/apCShader_Blur.compute");

			if(cShader == null)
			{
				//Debug.LogError("No Compute Shader");
				return null;
			}

			int kernel = cShader.FindKernel("CSMain");
			if(kernel < 0)
			{
				//커널 찾기에 실패했다.
				return null;
			}

			int srcWidth = srcColorTex.width;
			int srcHeight = srcColorTex.height;

			RenderTexture resultTex = RenderTexture.GetTemporary(srcWidth, srcHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			resultTex.enableRandomWrite = true;
			resultTex.Create();

			cShader.SetTexture(kernel, "Result", resultTex);
			
			cShader.SetTexture(kernel, "SrcColorTex", srcColorTex);
			cShader.SetTexture(kernel, "SrcAlphaTex", srcAlphaTex);

			cShader.SetInt("srcWidth", srcWidth);
			cShader.SetInt("srcHeight", srcHeight);
			cShader.SetInt("blurSize", blurSize);

			cShader.Dispatch(kernel, srcWidth / 8 + 1, srcHeight / 8 + 1, 1);

			RenderTexture.active = resultTex;
			Texture2D blurredTex = new Texture2D(srcWidth, srcHeight, TextureFormat.ARGB32, false);
			blurredTex.ReadPixels(new Rect(0, 0, resultTex.width, resultTex.height), 0, 0);
			blurredTex.Apply();

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(resultTex);
			

			return blurredTex;
		}


		private Texture2D ResizeTextureWithComputeShader(	Texture2D srcColorTex, 
															Texture2D srcAlphaTex,
															int dstWidth, int dstHeight)
		{
			int srcWidth = srcColorTex.width;
			int srcHeight = srcColorTex.height;

			int blurSize = 4;
			if(dstWidth < srcWidth)
			{
				blurSize = Mathf.Max(blurSize, srcWidth / dstWidth);
			}
			if(dstHeight < srcHeight)
			{
				blurSize = Mathf.Max(blurSize, srcHeight / dstHeight);
			}

			Texture2D blurTexture = BlurTextureWithComputeShader(srcColorTex, srcAlphaTex, blurSize);
			if(blurTexture == null)
			{
				//Debug.LogError("No Pre-Texture");
				return null;
			}

			//추가 20.4.21
			string basePath = "Assets/AnyPortrait/";
			if(_editor != null)
			{
				basePath = apPathSetting.I.CurrentPath;
			}

			//ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/AnyPortrait/Editor/Scripts/Subtool/CShader/apCShader_ResizeTexture.compute");
			ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(basePath + "Editor/Scripts/Subtool/CShader/apCShader_ResizeTexture.compute");

			if(cShader == null)
			{
				//Debug.LogError("No Compute Shader");
				return null;
			}

			int kernel = cShader.FindKernel("CSMain");
			if(kernel < 0)
			{
				//커널 찾기에 실패했다.
				return null;
			}

			

			float offsetSrcX = (float)srcWidth / (float)dstWidth;
			float offsetSrcY = (float)srcHeight / (float)dstHeight;
			float offsetDstX = (float)dstWidth / (float)srcWidth;
			float offsetDstY = (float)dstHeight / (float)srcHeight;
			int offsetSrcX_Half = (int)Mathf.Max(offsetSrcX * 0.5f, 1);
			int offsetSrcY_Half = (int)Mathf.Max(offsetSrcY * 0.5f, 1);
			int offsetDstX_Half = (int)Mathf.Max(offsetDstX * 0.5f, 1);
			int offsetDstY_Half = (int)Mathf.Max(offsetDstY * 0.5f, 1);
			
			//이미지가 커질땐 조금 더 오버 샘플링을 해야한다.
			if(dstWidth > srcWidth)
			{
				offsetSrcX_Half += 2;
				offsetDstX_Half += 1;
			}
			if(dstHeight > srcHeight)
			{
				offsetSrcY_Half += 2;
				offsetDstY_Half += 1;
			}
			
			RenderTexture resultTex = RenderTexture.GetTemporary(dstWidth, dstHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			resultTex.enableRandomWrite = true;
			resultTex.Create();

			cShader.SetTexture(kernel, "Result", resultTex);
			
			cShader.SetTexture(kernel, "SrcColorTex", srcColorTex);
			cShader.SetTexture(kernel, "SrcAlphaTex", srcAlphaTex);
			cShader.SetTexture(kernel, "SrcBlurTex", blurTexture);
			

			cShader.SetInt("srcWidth", srcWidth);
			cShader.SetInt("srcHeight", srcHeight);
			cShader.SetInt("dstWidth", dstWidth);
			cShader.SetInt("dstHeight", dstHeight);
			cShader.SetInt("srcOffsetX", offsetSrcX_Half);
			cShader.SetInt("srcOffsetY", offsetSrcY_Half);
			cShader.SetInt("dstOffsetX", offsetDstX_Half);
			cShader.SetInt("dstOffsetY", offsetDstY_Half);

			cShader.Dispatch(kernel, dstWidth / 8 + 1, dstHeight / 8 + 1, 1);

			RenderTexture.active = resultTex;
			Texture2D resizeTex = new Texture2D(dstWidth, dstHeight, TextureFormat.ARGB32, false);
			resizeTex.ReadPixels(new Rect(0, 0, resultTex.width, resultTex.height), 0, 0);
			resizeTex.Apply();

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(resultTex);
			UnityEngine.GameObject.DestroyImmediate(blurTexture);

			return resizeTex;
		}



		public Texture2D MergeAlphaChannelWithComputeShader(Texture2D srcColorTex, Texture2D srcAlphaTex)
		{
 			int width = srcColorTex.width;
			int height = srcColorTex.height;
			int blurSize = 4;

			Texture2D blurTexture = BlurTextureWithComputeShader(srcColorTex, srcAlphaTex, blurSize);
			if(blurTexture == null)
			{
				//Debug.LogError("No Pre-Texture");
				return null;
			}

			//추가 20.4.21
			string basePath = "Assets/AnyPortrait/";
			if(_editor != null)
			{
				basePath = apPathSetting.I.CurrentPath;
			}

			//ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/AnyPortrait/Editor/Scripts/Subtool/CShader/apCShader_MergeChannels.compute");
			ComputeShader cShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(basePath + "Editor/Scripts/Subtool/CShader/apCShader_MergeChannels.compute");


			if(cShader == null)
			{
				//Debug.LogError("No Compute Shader");
				return null;
			}

			int kernel = cShader.FindKernel("CSMain");
			if(kernel < 0)
			{
				//커널 찾기에 실패했다.
				return null;
			}
			
			
			RenderTexture resultTex = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			resultTex.enableRandomWrite = true;
			resultTex.Create();

			cShader.SetTexture(kernel, "Result", resultTex);
			
			cShader.SetTexture(kernel, "SrcColorTex", srcColorTex);
			cShader.SetTexture(kernel, "SrcAlphaTex", srcAlphaTex);
			cShader.SetTexture(kernel, "SrcBlurTex", blurTexture);
			
			cShader.Dispatch(kernel, width / 8 + 1, height / 8 + 1, 1);

			RenderTexture.active = resultTex;
			Texture2D resizeTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
			resizeTex.ReadPixels(new Rect(0, 0, resultTex.width, resultTex.height), 0, 0);
			resizeTex.Apply();

			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(resultTex);

			UnityEngine.GameObject.DestroyImmediate(blurTexture);

			return resizeTex;
		}

		//GIF 함수
		//----------------------------------------------


		// Get / Set
		//----------------------------------------------


		//-------------------------------------------------------------------------------
		private int GetProperRenderTextureSize(int size)
		{
			if(size < 256)			{ return 256; }
			else if(size < 512)		{ return 512; }
			else if(size < 1024)	{ return 1024; }
			else if(size < 2048)	{ return 2048; }
			else { return 4096; }
		}
	}

}