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

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// PSD에 들어가는 레이어 데이터이다.
	/// Import에서 사용되는 apPSDLayerData와 유사하다. 헷갈리지 말자.
	/// Import할때 저장되는 데이터 정보를 저장하는 역할
	/// Texture Atlas와 Mesh / MeshGroup과 연결한다.
	/// 색상 정보는 저장하지 않는다.
	/// Reimport를 위한 것이므로, 그 외의 정보는 저장되지 않는다. (레이어 속성은 처음 Bake시에 정해진다)
	/// </summary>
	[Serializable]
	public class apPSDSetLayer
	{
		// Members
		//---------------------------------------------
		public int _layerIndex = -1;
		public string _name = "";
		public int _width = -1;
		public int _height = -1;
		//public int _posOffset_Left = 0;
		//public int _posOffset_Top = 0;
		//public int _posOffset_Right = 0;
		//public int _posOffset_Bottom = 0;

		//_isImageLayer가 True이면 : TextureData + Mesh (Atlas) + MeshTransform 정보가 포함된다.
		//_isImageLayer가 False이면 : MeshGroupTransform 정보만 포함된다.
		public bool _isImageLayer = false;

		//1. ImageLayer인 경우
		//Bake된 정보
		//[v1.4.0에서 추가]
		//이전 버전과 호환되지 않는 부분이다.
		//호환 여부를 체크하기 위해 모두 -1로 초기화된다.
		public int _bakedAtlasIndex = -1;
		public int _bakedWidth = -1;
		public int _bakedHeight = -1;
		public int _bakedImagePos_Left = -1;
		public int _bakedImagePos_Top = -1;
		//Secondary와의 참조를 위해 Unique ID를 기록한다.
		public int _bakedUniqueID = -1;

		public int _transformID = -1;
		public int _textureDataID = -1;//이것도 기록하자 [v1.4.0]

		public float _bakedLocalPosOffset_X = 0;
		public float _bakedLocalPosOffset_Y = 0;

		public bool _isBaked = false;

		

	

		// Init
		//---------------------------------------------
		public apPSDSetLayer()
		{

		}


		// Functions
		//---------------------------------------------
		public void SetBakeData(	int layerIndex,
									string name, 
									int width, 
									int height, 
									//int posOffset_Left, 
									//int posOffset_Top, 
									//int posOffset_Right, 
									//int posOffset_Bottom, 
									bool isImageLayer,
									//int bakedWidth,
									//int bakedHeight,
									//int bakedImagePos_Left, 
									//int bakedImagePos_Top, 
									int transformID, 
									int textureDataID,//추가22.6.27

									float bakedLocalPosOffset_X,
									float bakedLocalPosOffset_Y,
									
									//추가 22.6.22
									int bakedAtlasIndex,
									int bakedWidth,
									int bakedHeight,
									int bakedPos_Left,
									int bakedPos_Top,
									int bakedUniqueID
									
									)
		{
			_layerIndex = layerIndex;
			_name = name;
			_width = width;
			_height = height;
			//_posOffset_Left = posOffset_Left;
			//_posOffset_Top = posOffset_Top;
			//_posOffset_Right = posOffset_Right;
			//_posOffset_Bottom = posOffset_Bottom;
			_isImageLayer = isImageLayer;

			//_bakedWidth = bakedWidth;
			//_bakedHeight = bakedHeight;
			//_bakedImagePos_Left = bakedImagePos_Left;
			//_bakedImagePos_Top = bakedImagePos_Top;

			_transformID = transformID;
			_bakedLocalPosOffset_X = bakedLocalPosOffset_X;
			_bakedLocalPosOffset_Y = bakedLocalPosOffset_Y;

			_bakedAtlasIndex = bakedAtlasIndex;
			_bakedWidth = bakedWidth;
			_bakedHeight = bakedHeight;
			_bakedImagePos_Left = bakedPos_Left;
			_bakedImagePos_Top = bakedPos_Top;

			_textureDataID = textureDataID;
			_bakedUniqueID = bakedUniqueID;

			_isBaked = true;
		}

		public void SetNotBaked(int layerIndex, string name, bool isImageLayer)
		{
			_layerIndex = layerIndex;
			_name = name;
			_isImageLayer = isImageLayer;

			_bakedLocalPosOffset_X = 0;
			_bakedLocalPosOffset_Y = 0;

			_bakedAtlasIndex = -1;
			_bakedWidth = 0;
			_bakedHeight = 0;
			_bakedImagePos_Left = -1;
			_bakedImagePos_Top = -1;

			_bakedUniqueID = -1;


			_isBaked = false;
		}


		// Get / Set
		//---------------------------------------------
	}
}