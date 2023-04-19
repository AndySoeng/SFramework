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

	[Serializable]
	public class apTextureData
	{
		// Members
		//-------------------------------------------
		public int _uniqueID = -1;

		public string _name = "";

		[SerializeField]
		public Texture2D _image = null;
		
		//이미지 크기 값에 대한 변경 : 이게 실제 메시의 Pixel 위치값이며, 이미지 에셋이 바뀌더라도 유지되어야 한다.
		//초기값 이후로는 변경되어서는 안된다.
		public int _width = 0;
		public int _height = 0;

		public string _assetFullPath = "";

		//public bool _isPSDFile = false;//<<이거 삭제


		// Init
		//-------------------------------------------
		/// <summary>
		/// 백업 로드시에만 사용되는 생성자
		/// </summary>
		public apTextureData()
		{
			
		}
		public apTextureData(int index)
		{
			_uniqueID = index;
		}

		public void ReadyToEdit(apPortrait portrait)
		{
			portrait.RegistUniqueID(apIDManager.TARGET.Texture, _uniqueID);
		}

		// Get / Set
		//-------------------------------------------
		public void SetImage(Texture2D image, int width, int height)
		{
			_image = image;
			_name = image.name;

			_width = width;
			_height = height;
		}


		//추가 21.9.11 : 이미지 에셋의 크기를 비교하여 비율을 리턴한다.
		/// <summary>
		/// 실제 에셋으로의 이미지 크기와 현재의 이미지 크기가 다르면 true 리턴
		/// </summary>
		/// <returns></returns>
		public bool IsResized()
		{
			if(_image == null)
			{
				return false;
			}
			return _image.width != _width || _image.height != _height;
		}

		public int GetTextureAssetWidth()
		{
			return _image != null ? _image.width : 0;
		}

		public int GetTextureAssetHeight()
		{
			return _image != null ? _image.height : 0;
		}
		


		
	}

}