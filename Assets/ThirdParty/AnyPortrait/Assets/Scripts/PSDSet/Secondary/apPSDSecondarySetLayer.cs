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
using System.IO;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 메인 레이어로부터 추가적인 보정이 들어가는 보조 레이어
	/// </summary>
	[Serializable]
	public class apPSDSecondarySetLayer
	{
		// Members
		//----------------------------------------------
		//원본 레이어의 인덱스+이름
		public int _mainLayerIndex = -1;
		public string _mainLayerName = "";

		//연결된 소스 레이어
		//평소에는 null 상태이며, 선택을 하면 찾아서 연결을 한다.
		[NonSerialized] public apPSDSetLayer _linkedMainLayer = null;
		[NonSerialized] public apTextureData _linkedTextureData = null;

		//Bake를 위한 UV 정보
		[SerializeField] public int _bakedAtlasIndex = -1;
		[SerializeField] public int _bakedWidth = -1;
		[SerializeField] public int _bakedHeight = -1;
		[SerializeField] public int _bakedImagePos_Left = -1;
		[SerializeField] public int _bakedImagePos_Top = -1;
		[SerializeField] public int _bakedUniqueID = -1;

		//Src와 비교하여 추가적인 Offset
		[SerializeField] public float _lastBakedDeltaPixelPosOffsetX = 0;
		[SerializeField] public float _lastBakedDeltaPixelPosOffsetY = 0;

		// Init
		//----------------------------------------------
		public apPSDSecondarySetLayer()
		{
			
		}

		public void Init()
		{
			_mainLayerIndex = -1;
			_mainLayerName = "";

			_lastBakedDeltaPixelPosOffsetX = 0;
			_lastBakedDeltaPixelPosOffsetY = 0;

			_bakedAtlasIndex = -1;
			_bakedWidth = -1;
			_bakedHeight = -1;
			_bakedImagePos_Left = -1;
			_bakedImagePos_Top = -1;
			_bakedUniqueID = -1;
		}

		public void CopyFromMainLayer(apPSDSetLayer srcLayer)
		{
			_mainLayerIndex = srcLayer._layerIndex;
			_mainLayerName = srcLayer._name;

			_bakedAtlasIndex = srcLayer._bakedAtlasIndex;
			_bakedWidth = srcLayer._bakedWidth;
			_bakedHeight = srcLayer._bakedHeight;
			_bakedImagePos_Left = srcLayer._bakedImagePos_Left;
			_bakedImagePos_Top = srcLayer._bakedImagePos_Top;
			_bakedUniqueID = srcLayer._bakedUniqueID;
		}

	}
}