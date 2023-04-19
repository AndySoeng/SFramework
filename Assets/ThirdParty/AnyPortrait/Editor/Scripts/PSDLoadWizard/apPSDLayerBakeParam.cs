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
using System.Collections.Generic;
using System;
using System.IO;
using Ntreev.Library.Psd;

using AnyPortrait;

namespace AnyPortrait
{
	public class apPSDLayerBakeParam
	{
		public apPSDLayerData _targetLayer = null;
		public int _atlasIndex = 0;
		public int _posOffset_X = 0;
		public int _posOffset_Y = 0;
		public bool _isLeftover = false;//PSD로 부터 있는 이미지라면 false, PSD에 없는 이미지라면 True

		//Secondary맵을 만들때는 슬롯을 사용하지 않는다.
		public int _posOffset_SecondaryPixelX = 0;
		public int _posOffset_SecondaryPixelY = 0;

		public apPSDLayerBakeParam(apPSDLayerData targetLayer,
								int atlasIndex,
								int posOffset_X,
								int posOffset_Y,
								bool isLeftover
								)
		{
			_targetLayer = targetLayer;
			_atlasIndex = atlasIndex;
			_posOffset_X = posOffset_X;
			_posOffset_Y = posOffset_Y;
			_isLeftover = isLeftover;
		}

		/// <summary>
		/// Secondary 맵을 만들때 호출되는 생성자
		/// </summary>
		/// <param name="targetLayer"></param>
		/// <param name="atlasIndex"></param>
		/// <param name="posOffset_PixelX"></param>
		/// <param name="posOffset_PixelY"></param>
		public apPSDLayerBakeParam(apPSDLayerData targetLayer,
								int atlasIndex,
								int posOffset_PixelX,
								int posOffset_PixelY,
								int padding
								)
		{
			_targetLayer = targetLayer;
			_atlasIndex = atlasIndex;
			
			_posOffset_X = 0;
			_posOffset_Y = 0;
			_isLeftover = false;

			//Secondary맵을 만들때는 슬롯을 사용하지 않는다.
			_posOffset_SecondaryPixelX = posOffset_PixelX - padding;
			_posOffset_SecondaryPixelY = posOffset_PixelY - padding;
		}
	}
}
