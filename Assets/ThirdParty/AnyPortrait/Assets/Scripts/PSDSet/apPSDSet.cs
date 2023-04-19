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
	/// PSD 파일로부터 Atlas Texture와 Mesh를 만든 경우 만들어지는 데이터 세트
	/// Bake 데이터, Layer와 Atlas 데이터 등이 포함되어 있다.
	/// 이미 만들어진 데이터로부터 역으로 생성하는 것도 가능하다.
	/// Opt 버전은 없으며 apPortrait에 리스트로 저장된다.
	/// 이 정보는 Reimport를 위한 메타 데이터로만 저장, 사용된다.
	/// 이 정보는 백업에 저장되지 않는다.
	/// </summary>
	[Serializable]
	public class apPSDSet
	{
		// Members
		//----------------------------------
		public int _uniqueID = -1;
		public string _filePath = "";
		public string _fileNameOnly = "";
		public string _lastBakedAssetName = "";//<<마지막에 Asset으로 저장되는 이름
		public bool _isPSDFileExist = false;
		
		//public string _bakeDstPath = "";

		//이 PSD가 반영된 메시 그룹 ID
		public int _targetMeshGroupID = -1;

		[NonSerialized]
		public apMeshGroup _linkedTargetMeshGroup = null;

		//연결된 TextureData
		[Serializable]
		public class TextureDataSet
		{
			[SerializeField]
			public int _textureDataID = -1;
			[NonSerialized]
			public apTextureData _linkedTextureData = null;
		}
		
		[SerializeField]
		public List<TextureDataSet> _targetTextureDataList = new List<TextureDataSet>();
		

		[SerializeField]
		public List<apPSDSetLayer> _layers = new List<apPSDSetLayer>();

		//TODO : Atlas Bake 옵션 저장 (동일한 옵션으로 구워야 하므로)
		//마지막 Bake된 기록
		public bool _isLastBaked = false;
		public int _lastBaked_PSDImageWidth = -1;
		public int _lastBaked_PSDImageHeight = -1;
		public float _lastBaked_PSDCenterOffsetDelta_X = 0;
		public float _lastBaked_PSDCenterOffsetDelta_Y = 0;
		public int _lastBaked_MeshGroupScaleX100 = 100;
		

		//Atlas Bake 옵션
		[Serializable]
		public enum BAKE_SIZE
		{
			s256 = 0,
			s512 = 1,
			s1024 = 2,
			s2048 = 3,
			s4096 = 4
		}

		[SerializeField]
		public BAKE_SIZE _bakeOption_Width = BAKE_SIZE.s1024;
		[SerializeField]
		public BAKE_SIZE _bakeOption_Height = BAKE_SIZE.s1024;
		[SerializeField]
		public string _bakeOption_DstFilePath = "";//저장될 기본 경로 (폴더만 지정한다. 나머지는 파일 + 이미지 번호)
		[SerializeField]
		public string _bakeOption_DstFileRelativePath = "";
		public int _bakeOption_MaximumNumAtlas = 2;
		public int _bakeOption_Padding = 4;
		public bool _bakeOption_BlurOption = true;
		public int _bakeScale100 = 100;//<<Bake된 스케일 (100이 기본)
		
		
		[NonSerialized]
		public float _nextBakeCenterOffsetDelta_X = 0;
		[NonSerialized]
		public float _nextBakeCenterOffsetDelta_Y = 0;
		[NonSerialized]
		public int _next_meshGroupScaleX100 = 100;
		[NonSerialized]
		public int _prev_bakeScale100 = 100;//<<Bake된 스케일 (100이 기본)
		




		// Init
		//----------------------------------
		public apPSDSet()
		{
			//백업용
		}
		public apPSDSet(int uniqueID)
		{
			_uniqueID = uniqueID;
			Init();
		}

		public void Init()
		{
			_filePath = "";
			_fileNameOnly = "";
			_targetMeshGroupID = -1;
			_targetTextureDataList.Clear();
			if(_layers == null)
			{
				_layers = new List<apPSDSetLayer>();
			}
			_layers.Clear();

			_next_meshGroupScaleX100 = 100;
			_nextBakeCenterOffsetDelta_X = 0;
			_nextBakeCenterOffsetDelta_Y = 0;
		}

		
		// Functions
		//----------------------------------
		public void RefreshPSDFilePath()
		{
			_isPSDFileExist = false;
			if(!string.IsNullOrEmpty(_filePath))
			{
				FileInfo fi = new FileInfo(_filePath);//Path 빈 문자열 확인했음 (21.9.10)
				if(fi.Exists)
				{
					_isPSDFileExist = true;
					_fileNameOnly = fi.Name;
					if(_fileNameOnly.EndsWith(".psd") || _fileNameOnly.EndsWith(".PSD"))
					{
						_fileNameOnly = _fileNameOnly.Substring(0, _fileNameOnly.Length - 4);
					}
				}
			}
			if(!_isPSDFileExist)
			{
				_fileNameOnly = "";
			}
		}
		public void SetPSDFilePath(string PSDFilePath)
		{
			_filePath = PSDFilePath;
			RefreshPSDFilePath();
		}


		//Bake한 정보를 저장하자.
		public void SetPSDBakeData(	string filePath, string fileNameOnly,
									apMeshGroup targetMeshGroup,
									List<apTextureData> addedTextureData,
									int atlasImageWidth, int atlasImageHeight,
									int PSDImageWidth, int PSDImageHeight,
									float PSDCenterOffsetDelta_X, float PSDCenterOffsetDelta_Y,
									string bakeDstFilePath,string bakeDstFileRelativePath,
									int bakeMaximumNumAtlas, int bakePadding, bool bakeBlurOption,
									int bakeScale100, int meshGroupScale100
									)
		{
			_filePath = filePath;
			_fileNameOnly = fileNameOnly;
			_lastBakedAssetName = fileNameOnly;
			//_isPSDFileExist = isPSDFileExist;
		
			if(targetMeshGroup != null)
			{
				_targetMeshGroupID = targetMeshGroup._uniqueID;
				
			}
			else
			{
				_targetMeshGroupID = -1;
				_targetTextureDataList.Clear();
			}
			//Debug.Log("SetPSDBakeData > Layer Clear");
			_layers.Clear();//<<이건 다음 함수에서 순차적으로 설정
			
			_isLastBaked = true;
			_lastBaked_PSDImageWidth = PSDImageWidth;
			_lastBaked_PSDImageHeight = PSDImageHeight;
			_lastBaked_PSDCenterOffsetDelta_X = PSDCenterOffsetDelta_X;
			_lastBaked_PSDCenterOffsetDelta_Y = PSDCenterOffsetDelta_Y;
			_lastBaked_MeshGroupScaleX100 = meshGroupScale100;

			if (atlasImageWidth <= 256)			{ _bakeOption_Width = BAKE_SIZE.s256; }
			else if (atlasImageWidth <= 512)	{ _bakeOption_Width = BAKE_SIZE.s512; }
			else if (atlasImageWidth <= 1024)	{ _bakeOption_Width = BAKE_SIZE.s1024; }
			else if (atlasImageWidth <= 2048)	{ _bakeOption_Width = BAKE_SIZE.s2048; }
			else 								{ _bakeOption_Width = BAKE_SIZE.s4096; }

			if (atlasImageHeight <= 256)		{ _bakeOption_Height = BAKE_SIZE.s256; }
			else if (atlasImageHeight <= 512)	{ _bakeOption_Height = BAKE_SIZE.s512; }
			else if (atlasImageHeight <= 1024)	{ _bakeOption_Height = BAKE_SIZE.s1024; }
			else if (atlasImageHeight <= 2048)	{ _bakeOption_Height = BAKE_SIZE.s2048; }
			else 								{ _bakeOption_Height = BAKE_SIZE.s4096; }

			_bakeOption_DstFilePath = bakeDstFilePath;
			_bakeOption_DstFileRelativePath = bakeDstFileRelativePath;
			_bakeOption_MaximumNumAtlas = bakeMaximumNumAtlas;
			_bakeOption_Padding = bakePadding;
			_bakeOption_BlurOption = bakeBlurOption;
			_bakeScale100 = bakeScale100;

			if(addedTextureData != null && addedTextureData.Count > 0)
			{
				apTextureData curTexData = null;
				for (int i = 0; i < addedTextureData.Count; i++)
				{
					curTexData = addedTextureData[i];

					if(_targetTextureDataList.Exists(delegate(TextureDataSet a)
					{
						return a._textureDataID == curTexData._uniqueID;
					}))
					{
						//이미 존재한다면 패스
						continue;
					}
					TextureDataSet newSet = new TextureDataSet();
					newSet._textureDataID = curTexData._uniqueID;
					newSet._linkedTextureData = curTexData;
					_targetTextureDataList.Add(newSet);
				}
			}
		}

		public void SetPSDLayerData(int layerIndex,
									string name, 
									int width, 
									int height, 
									//int posOffset_Left, 
									//int posOffset_Top, 
									//int posOffset_Right, 
									//int posOffset_Bottom, 
									bool isImageLayer, 
									int transformID, 
									int textureDataID,
									float bakedLocalPosOffset_X,
									float bakedLocalPosOffset_Y,
									
									//추가 22.6.22
									int bakedAtlasIndex, 
									int bakedWidth, 
									int bakedHeight, 
									int bakedImagePos_Left, 
									int bakedImagePos_Top)
		{
			//Bake 시점에서 Unique ID를 만들자
			int bakedUniqueID = -1;

			int cnt = 0;
			bool isRandomKeyCompleted = false;
			while(true)
			{
				if(cnt > 20)
				{
					break;
				}

				bakedUniqueID = UnityEngine.Random.Range(1000, 99999999);
				bool isExist = _layers.Exists(delegate(apPSDSetLayer a)
				{
					return a._bakedUniqueID == bakedUniqueID;
				});

				if(!isExist)
				{
					//중복되지 않는다.
					isRandomKeyCompleted = true;
					break;
				}

				cnt += 1;
			}
			if(!isRandomKeyCompleted)
			{
				//랜덤으로 키가 생성되지 않았다.
				//고정값을 순서대로 체크해서 ID를 생성하자
				for (int iID = 100; iID < 1000; iID++)
				{
					bakedUniqueID = iID;
					bool isExist = _layers.Exists(delegate(apPSDSetLayer a)
					{
						return a._bakedUniqueID == bakedUniqueID;
					});

					if(!isExist)
					{
						//중복되지 않는다.
						isRandomKeyCompleted = true;
						break;
					}
				}
			}

			if(!isRandomKeyCompleted)
			{
				//실패..
				bakedUniqueID = -1;
			}

			apPSDSetLayer newLayer = new apPSDSetLayer();
			newLayer.SetBakeData(layerIndex,
									name, 
									width, 
									height, 
									//posOffset_Left, 
									//posOffset_Top, 
									//posOffset_Right, 
									//posOffset_Bottom, 
									isImageLayer, 
									//bakedWidth, 
									//bakedHeight, 
									//bakedImagePos_Left, 
									//bakedImagePos_Top, 
									transformID,
									textureDataID,
									bakedLocalPosOffset_X,
									bakedLocalPosOffset_Y,
									
									bakedAtlasIndex, 
									bakedWidth, 
									bakedHeight, 
									bakedImagePos_Left, 
									bakedImagePos_Top,
									bakedUniqueID
									);

			_layers.Add(newLayer);
		}

		public void SetNotBakedLayerData(int layerIndex, string name, bool isImageLayer)
		{
			apPSDSetLayer newLayer = new apPSDSetLayer();
			newLayer.SetNotBaked(layerIndex, name, isImageLayer);
			_layers.Add(newLayer);
		}



		// Get / Set
		//----------------------------------
		public bool IsValidPSDFile {  get {  return _isPSDFileExist; } }

		public int GetBakeWidth()
		{
			switch (_bakeOption_Width)
			{
				case BAKE_SIZE.s256:	return 256;
				case BAKE_SIZE.s512:	return 512;
				case BAKE_SIZE.s1024:	return 1024;
				case BAKE_SIZE.s2048:	return 2048;
				case BAKE_SIZE.s4096:	return 4096;
			}
			return 4096;
		}

		public int GetBakeHeight()
		{
			switch (_bakeOption_Height)
			{
				case BAKE_SIZE.s256:	return 256;
				case BAKE_SIZE.s512:	return 512;
				case BAKE_SIZE.s1024:	return 1024;
				case BAKE_SIZE.s2048:	return 2048;
				case BAKE_SIZE.s4096:	return 4096;
			}
			return 4096;
		}

		public apPSDSetLayer GetLayer(apTransform_Mesh meshTransform)
		{
			return _layers.Find(delegate(apPSDSetLayer a)
			{
				return a._transformID == meshTransform._transformUniqueID && a._isImageLayer;
			});
		}

		public apPSDSetLayer GetLayer(apTransform_MeshGroup meshGroupTransform)
		{
			return _layers.Find(delegate(apPSDSetLayer a)
			{
				return a._transformID == meshGroupTransform._transformUniqueID && !a._isImageLayer;
			});
		}
	}
}