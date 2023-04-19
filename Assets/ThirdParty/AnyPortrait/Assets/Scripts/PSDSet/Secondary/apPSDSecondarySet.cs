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
	/// 일반적인 PSDSet과 달리, 메시 그룹에 적용되지 않고 오직 보조 텍스쳐 에셋만 생성하는 정보.
	/// 아틀라스 동기화를 위해 메인 PSD Set과 완성된 텍스쳐 에셋만 저장한다.
	/// 기본적으로는 PSD Set과 유사하다.
	/// </summary>
	[Serializable]
	public class apPSDSecondarySet
	{
		// Members
		//------------------------------------
		public int _uniqueID = -1;
		public int _mainPSDSetID = -1;//참조하는 메인 PSD Set

		[NonSerialized]
		public apPSDSet _linkedMainSet = null;

		//여기서 사용하는 보조용 PSD 파일
		public string _psdFilePath = "";

		//저장한 에셋의 기본 이름
		public string _dstFilePath = "";
		public string _dstFilePath_Relative = "";

		public string _bakedTextureAssetName = "";

		//Secondary의 경우, 배경색이 꼭 투명이 아닐 수 있다.
		[SerializeField]
		public Color _backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		

		//완성된 에셋
		[SerializeField]
		public List<Texture2D> _bakedTextures = new List<Texture2D>();

		[SerializeField]
		public List<apPSDSecondarySetLayer> _layers = new List<apPSDSecondarySetLayer>();

		//이전 Bake 정보
		public bool _isLastBaked = false;
		public int _lastBaked_PSDImageWidth = -1;
		public int _lastBaked_PSDImageHeight = -1;
		//public float _lastBaked_PSDCenterOffsetDelta_X = 0;
		//public float _lastBaked_PSDCenterOffsetDelta_Y = 0;
		//public int _lastBaked_MeshGroupScaleX100 = 100;//Secondary는 메시그룹의 크기는 알 필요가 없다.
		public int _lastBaked_Scale100 = 100;//PSD > Atlas의 크기 배율.

		//[NonSerialized]
		//public float _nextBakeCenterOffsetDelta_X = 0;
		//[NonSerialized]
		//public float _nextBakeCenterOffsetDelta_Y = 0;
		//[NonSerialized]
		//public int _next_meshGroupScaleX100 = 100;
		//[NonSerialized]
		//public int _next_BakeScale100 = 100;

		

		[NonSerialized]
		public int _next_bakeScale100 = 100;//<<Bake된 스케일 (100이 기본)
		//Atlas Bake 옵션은 메인과 동일하다.

		//미리보기 및 유효성 테스트를 위해 Main의 텍스쳐 데이터를 가져온다.
		public class SrcTextureData
		{
			public int _textureDataID = -1;
			public apTextureData _linkedTextureData = null;

			public SrcTextureData(int textureDataID, apTextureData linkedTextureData)
			{
				_textureDataID = textureDataID;
				_linkedTextureData = linkedTextureData;
			}
		}

		[NonSerialized]
		public Dictionary<int, SrcTextureData> _srcTextureDataInfoMap = null;
		[NonSerialized]
		public List<SrcTextureData> _srcTextureDataInfoList = null;

		// Init
		//------------------------------------
		public apPSDSecondarySet()
		{
			//_nextBakeCenterOffsetDelta_X = 0;
			//_nextBakeCenterOffsetDelta_Y = 0;
			//_next_meshGroupScaleX100 = 100;

			_dstFilePath = "";
			_dstFilePath_Relative = "";

			_backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		}

		public void SetUniqueID(int uniqueID)
		{
			_uniqueID = uniqueID;
		}

		public void CopyFromPSDSet(apPSDSet srcMainSet)
		{	
			_mainPSDSetID = srcMainSet._uniqueID;

			if(_bakedTextures == null)
			{
				_bakedTextures = new List<Texture2D>();
			}
			_bakedTextures.Clear();

			if(_layers == null)
			{
				_layers = new List<apPSDSecondarySetLayer>();
			}
			_layers.Clear();

			_psdFilePath = "";

			//이전 Bake 정보
			_isLastBaked = false;
			_lastBaked_PSDImageWidth = -1;
			_lastBaked_PSDImageHeight = -1;
			//_lastBaked_PSDCenterOffsetDelta_X = 0;
			//_lastBaked_PSDCenterOffsetDelta_Y = 0;
			_lastBaked_Scale100 = srcMainSet._bakeScale100;

			//_lastBaked_MeshGroupScaleX100 = 100;

			//레이어 정보를 복사해야한다.
			int nSrcLayer = srcMainSet._layers != null ? srcMainSet._layers.Count : 0;
			if(nSrcLayer > 0)
			{
				apPSDSetLayer curSrcLayer = null;
				apPSDSecondarySetLayer newSecLayer = null;
				for (int i = 0; i < nSrcLayer; i++)
				{
					curSrcLayer = srcMainSet._layers[i];
					if(!curSrcLayer._isBaked
						|| !curSrcLayer._isImageLayer)
					{
						//불필요한 레이어는 제외한다.
						//- Bake된 적이 없는 레이어
						//- 이미지가 아닌 레이어
						continue;
					}
					newSecLayer = new apPSDSecondarySetLayer();
					newSecLayer.Init();
					newSecLayer.CopyFromMainLayer(curSrcLayer);

					_layers.Add(newSecLayer);
				}
			}
		}

		

		// Link
		//----------------------------------
		public void ClearTextureDataInfo()
		{
			if(_srcTextureDataInfoMap == null)
			{
				_srcTextureDataInfoMap = new Dictionary<int, SrcTextureData>();
			}
			if(_srcTextureDataInfoList == null)
			{
				_srcTextureDataInfoList = new List<SrcTextureData>();
			}
			_srcTextureDataInfoMap.Clear();
			_srcTextureDataInfoList.Clear();
		}


		public void AddTextureDataInfo(int textureDataID, apTextureData linkedTextureData)
		{
			if(_srcTextureDataInfoMap.ContainsKey(textureDataID))
			{
				return;
			}

			SrcTextureData texInfo = new SrcTextureData(textureDataID, linkedTextureData);
			_srcTextureDataInfoMap.Add(textureDataID, texInfo);
			_srcTextureDataInfoList.Add(texInfo);
		}


		// Functions
		//----------------------------------
	}
}