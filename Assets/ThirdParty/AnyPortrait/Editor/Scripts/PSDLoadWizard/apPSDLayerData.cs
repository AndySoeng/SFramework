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
using Ntreev.Library.Psd;

using AnyPortrait;

namespace AnyPortrait
{
	public class apPSDLayerData
	{
		// Members
		//---------------------------------------------------
		public int _layerIndex = -1;
		public string _name = "";
		public bool _isClipping = false;
		public int _width = -1;
		public int _height = -1;

		public bool _isImageLayer = true;//ImageLayer가 아니면 Group 레이어이다.

		public byte[] _colorData = null;
		//public Color[] _colors = null;

		public int _posOffset_Left = 0;
		public int _posOffset_Top = 0;
		public int _posOffset_Right = 0;
		public int _posOffset_Bottom = 0;

		public Vector2 _posOffset = Vector2.zero;

		public Vector2 _posOffsetLocal = Vector2.zero;

		public float _opacity = 1.0f;
		public Color _transparentColor2X = Color.black;
		public bool _isVisible = true;

		public Texture2D _image = null;
		public Texture2D _image_Nearest = null;//추가 v1.4.2 : Nearest 필터링 이미지도 만든다.

		public bool _isBakable = false;


		public apPSDLayerData _parentLayer = null;
		public List<apPSDLayerData> _childLayers = null;
		public int _hierarchyLevel = 0;//0이 Parent가 없을때. 그 외에는 1, 2, 3으로 늘어난다.

		//Bake 이후에 결정되는 값
		public int _bakedAtalsIndex = -1;
		public int _bakedWidth = 0;
		public int _bakedHeight = 0;
		public int _bakedImagePos_Left = 0;
		public int _bakedImagePos_Top = 0;

		//public IPsdLayer _srcPsdLayer = null;

		//Clipping이 가능한가
		public bool _isClippingValid = false;

		//Bake 결과
		public string _textureAssetPath = "";
		public apPSDBakeData _bakedData = null;


		//추가
		//Remap을 하고자 하는 경우
		public bool _isRemapSelected = false;
		public int _remap_TransformID = -1;

		public apTransform_Mesh _remap_MeshTransform = null;
		public apTransform_MeshGroup _remap_MeshGroupTransform = null;
		
		public Color _randomGUIColor = Color.black;
		public Color _randomGUIColor_Pro = Color.black;
		public float _remapPosOffsetDelta_X = 0;
		public float _remapPosOffsetDelta_Y = 0;
		public bool _isRemapPosOffset_Initialized = false;//<<Transform과 연결후, 또는 PSD Set Layer의 값으로 부터 초기화가 되었는가


		//추가 22.6.25 [v1.4.0]
		//Secondary 맵을 만들때는 이전 Bake 기록이 있어야 한다.
		public apPSDSecondarySetLayer _linkedBakedInfo_Secondary = null;
		
		

		// Init
		//---------------------------------------------------
		public apPSDLayerData(int layerIndex, IPsdLayer psdLayer, int imageTotalWidth, int imageTotalHeight)
		{
			_layerIndex = layerIndex;

			_name = psdLayer.Name;
			_isClipping = psdLayer.IsClipping;
			_isBakable = true;
			//_srcPsdLayer = psdLayer;
			_isClippingValid = true;//일단 가능하다고 체크

			_isVisible = true;

			if (psdLayer.HasImage)
			{
				//1. 이미지 타입의 레이어
				_isImageLayer = true;

				_width = psdLayer.Width;
				_height = psdLayer.Height;
				_posOffset_Left = psdLayer.Left;
				_posOffset_Top = imageTotalHeight - psdLayer.Top;//좌표계 특성상 반전하자

				_posOffset_Right = psdLayer.Right;
				_posOffset_Bottom = imageTotalHeight - psdLayer.Bottom;

				_posOffset = new Vector2(
					(float)(_posOffset_Left + _posOffset_Right) * 0.5f,
					(float)(_posOffset_Top + _posOffset_Bottom) * 0.5f
					);

				_opacity = psdLayer.Opacity;
				_transparentColor2X = new Color(0.5f, 0.5f, 0.5f, _opacity);
				_isVisible = psdLayer.IsVisible;

				//Debug.Log("이미지 [" + _name + "] Visible : " + _isVisible);

				_colorData = new byte[_width * _height * 4];//W x H x RGBA(4)
															//_colors = new Color[_width * _height];

				int subDataLength = _width * _height;
				int totalDataLength = imageTotalWidth * imageTotalHeight;

				//if(subDataLength == 0)
				//{
				//	UnityEngine.Debug.LogError("데이터 길이가 0");
				//}

				byte[] colorData_R = new byte[subDataLength];
				byte[] colorData_G = new byte[subDataLength];
				byte[] colorData_B = new byte[subDataLength];
				byte[] colorData_A = new byte[subDataLength];
				byte[] colorData_Mask = new byte[subDataLength];

				bool isMask = false;

				for (int i = 0; i < subDataLength; i++)
				{
					colorData_R[i] = 0;
					colorData_G[i] = 0;
					colorData_B[i] = 0;
					colorData_A[i] = 255;
					colorData_Mask[i] = 255;
				}

				
				
				for (int iChannel = 0; iChannel < psdLayer.Channels.Length; iChannel++)
				{
					IChannel curChannel = psdLayer.Channels[iChannel];
					byte[] curColorData = null;

					bool isValidChannel = true;
					if (curChannel.Type == ChannelType.Mask)
					{
						continue;
					}

					switch (curChannel.Type)
					{
						case ChannelType.Red:
							curColorData = colorData_R;
							break;

						case ChannelType.Green:
							curColorData = colorData_G;
							break;

						case ChannelType.Blue:
							curColorData = colorData_B;
							break;

						case ChannelType.Alpha:
							curColorData = colorData_A;
							break;

						case ChannelType.Mask:
							//마스크는 무시한다.
							curColorData = colorData_Mask;
							isMask = true;
							break;

							//추가 4.19 : 알수없는 채널이 들어오는 경우가 있다.
						default:
							//UnityEngine.Debug.LogError("알 수 없는 채널 : " + curChannel.Type);
							isValidChannel = false;
							break;
					}
					//유효하지 않은 채널
					if(!isValidChannel)
					{
						continue;
					}

					int dataLength = curChannel.Data.Length;
					if (subDataLength != dataLength)
					{
						//저장되었어야할 데이터와 실제 데이터가 다르다.
						bool isError = true;
						if (curChannel.Type == ChannelType.Mask)
						{
							isMask = false;

							//만약 -> Mask의 경우
							//이미지 전체가 들어올 수는 있다.
							//확장된 데이터 사이즈와 비교를 하자
							if (dataLength == totalDataLength)
							{
								isError = false;
								isMask = true;

								//데이터가 Height가 거꾸로 되어있다.
								//X, Y의 오프셋을 적용해야한다.
								//Debug.Log("Mask Image : Total : " + dataLength + "( " + imageTotalWidth + " x " + imageTotalHeight + " )");
								//Debug.Log("X : " + _posOffset_Left + " ~ " + _posOffset_Right);
								//Debug.Log("Y : " + +_posOffset_Top + " ~ " + _posOffset_Bottom);

							}
						}

						if (isError)
						{
							Debug.LogError("Data Length is not correct : " + _name + " [ Channel : " + curChannel.Type + " ]");
							//Debug.LogError("Data Size : " + curChannel.Data.Length + " (Expected : " + totalDataLength + " / Sub : " + subDataLength + ")");
							//Debug.Log("Mask Image : Total : " + dataLength + "( " + imageTotalWidth + " x " + imageTotalHeight + " )");
							//Debug.Log("X : " + _posOffset_Left + " ~ " + _posOffset_Right);
							//Debug.Log("Y : " + +_posOffset_Top + " ~ " + _posOffset_Bottom);
							continue;
						}
					}
					else
					{
						//칼라값을 복사하자
						Array.Copy(curChannel.Data, curColorData, dataLength);
					}
				}

				//이제 마지막으로 byte 배열을 만들고 Texture로 구성하자
				int iMainColor = 0;
				int iX = 0;
				int iY = 0;
				if (!isMask)
				{
					//Debug.Log("Image : " + layerIndex + " [ No Mask ]");
					//마스크가 없는 경우
					for (int iColor = 0; iColor < subDataLength; iColor++)
					{
						//iColor = y * Width + x
						//RevYColor = ((Height - Y) * Width) + X
						//iMainColor = iColor * 4;
						iY = iColor / _width;
						iX = iColor % _width;
						iMainColor = ((((_height - 1) - iY) * _width) + iX) * 4;
						_colorData[iMainColor + 0] = colorData_R[iColor];
						_colorData[iMainColor + 1] = colorData_G[iColor];
						_colorData[iMainColor + 2] = colorData_B[iColor];
						_colorData[iMainColor + 3] = colorData_A[iColor];

						//_colors[iColor] = ByteToColor(
						//	_colorData[iMainColor + 0],
						//	_colorData[iMainColor + 1],
						//	_colorData[iMainColor + 2],
						//	_colorData[iMainColor + 3]
						//	);
					}
				}
				else
				{
					//Debug.Log("Image : " + layerIndex + " [ Mask ]");
					//마스크가 있는 경우
					for (int iColor = 0; iColor < subDataLength; iColor++)
					{
						//iMainColor = iColor * 4;
						//iColor = y * Width + x
						//RevYColor = ((Height - Y) * Width) + X
						//iMainColor = iColor * 4;
						iY = iColor / _width;
						iX = iColor % _width;
						iMainColor = ((((_height - 1) - iY) * _width) + iX) * 4;

						_colorData[iMainColor + 0] = GetMaskedColor(colorData_R[iColor], colorData_Mask[iColor]);
						_colorData[iMainColor + 1] = GetMaskedColor(colorData_G[iColor], colorData_Mask[iColor]);
						_colorData[iMainColor + 2] = GetMaskedColor(colorData_B[iColor], colorData_Mask[iColor]);
						_colorData[iMainColor + 3] = GetMaskedColor(colorData_A[iColor], colorData_Mask[iColor]);

						//_colors[iColor] = ByteToColor(
						//	_colorData[iMainColor + 0],
						//	_colorData[iMainColor + 1],
						//	_colorData[iMainColor + 2],
						//	_colorData[iMainColor + 3]
						//	);
					}
				}

				_image = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
				//_image.SetPixels(_colors);
				_image.LoadRawTextureData(_colorData);
				_image.wrapMode = TextureWrapMode.Clamp;
				_image.filterMode = FilterMode.Bilinear;
				_image.Apply();


				_image_Nearest = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
				_image_Nearest.LoadRawTextureData(_colorData);
				_image_Nearest.wrapMode = TextureWrapMode.Clamp;
				_image_Nearest.filterMode = FilterMode.Point;//Nearest 샘플링
				_image_Nearest.Apply();

			}
			else
			{
				_isImageLayer = false;

				_image = null;
				_image_Nearest = null;
				_width = 0;
				_height = 0;
				_colorData = null;

				_width = psdLayer.Width;
				_height = psdLayer.Height;
				_posOffset_Left = psdLayer.Left;
				_posOffset_Top = imageTotalHeight - psdLayer.Top;//좌표계 특성상 반전하자

				_posOffset_Right = psdLayer.Right;
				_posOffset_Bottom = imageTotalHeight - psdLayer.Bottom;

				_posOffset = new Vector2(
					(float)(_posOffset_Left + _posOffset_Right) * 0.5f,
					(float)(_posOffset_Top + _posOffset_Bottom) * 0.5f
					);

				_opacity = 1.0f;
				_transparentColor2X = Color.black;
				_isVisible = psdLayer.IsVisible;

				//Debug.Log("그룹 [" + _name + "] Visible : " + _isVisible);
			}

			_posOffsetLocal = _posOffset;

			_parentLayer = null;
			_childLayers = null;

			_isRemapSelected = false;
			_remap_TransformID = -1;

			_remap_MeshTransform = null;
			_remap_MeshGroupTransform = null;

			_remapPosOffsetDelta_X = 0;
			_remapPosOffsetDelta_Y = 0;
			_isRemapPosOffset_Initialized = false;

			_linkedBakedInfo_Secondary = null;//이거 연결해둘것

			//추가 : GUI를 위해서 랜덤한 색상을 정하자
			MakeRandomGUIColor();

		}

		// Set Hierarcy
		//---------------------------------------------------
		public void SetLevel(int level)
		{
			_hierarchyLevel = level;
		}

		public void AddChildLayer(apPSDLayerData childLayer)
		{
			if (_childLayers == null)
			{
				_childLayers = new List<apPSDLayerData>();
			}
			_childLayers.Add(childLayer);
			childLayer._parentLayer = this;

			//World 값을 Parent 값을 이용해서 계싼
			//childLayer._posOffsetLocal = childLayer._posOffset - _posOffsetLocal;
			childLayer._posOffsetLocal = childLayer._posOffset - _posOffset;
		}

		// Functions
		//---------------------------------------------------
		private byte GetMaskedColor(byte colorValue, byte maskValue)
		{
			int iValue = (int)((((float)colorValue / 255.0f) * ((float)maskValue / 255.0f) * 255.0f) + 0.5f);

			if (iValue < 0)
			{ return 0; }
			if (iValue > 255)
			{ return 255; }
			return (byte)iValue;
		}


		private Color ByteToColor(byte byteR, byte byteG, byte byteB, byte byteA)
		{
			return new Color((float)byteR / 255.0f,
								(float)byteG / 255.0f,
								(float)byteB / 255.0f,
								(float)byteA / 255.0f);
		}

		private void MakeRandomGUIColor()
		{
			float colorR = UnityEngine.Random.Range(0.7f, 1.2f);
			float colorG = UnityEngine.Random.Range(0.7f, 1.2f);
			float colorB = UnityEngine.Random.Range(0.7f, 1.2f);
			//float rum = UnityEngine.Random.Range(0.6f, 1.0f);
			//float colorSum = colorR + colorG + colorB;
			//colorR = (colorR / colorSum) * rum;
			//colorG = (colorG / colorSum) * rum;
			//colorB = (colorB / colorSum) * rum;

			_randomGUIColor = new Color(colorR, colorG, colorB, 1.0f);

			colorR = UnityEngine.Random.Range(0.1f, 1.2f);
			colorG = UnityEngine.Random.Range(0.1f, 1.2f);
			colorB = UnityEngine.Random.Range(0.1f, 1.2f);
			_randomGUIColor_Pro = new Color(colorR, colorG, colorB, 1.0f);
		}
	}
}