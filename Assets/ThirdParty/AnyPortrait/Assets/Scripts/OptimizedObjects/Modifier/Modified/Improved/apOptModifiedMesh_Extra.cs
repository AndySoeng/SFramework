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
	/// 기존의 optModifiedMesh에서 Extra 부분만 가져온 클래스
	/// 최적화를 위해서 분리가 되었다.
	/// 데이터가 훨씬 더 최적화되었다.
	/// </summary>
	[Serializable]
	public class apOptModifiedMesh_Extra
	{
		// Members
		//--------------------------------------------
		//[NonSerialized]
		//private apOptModifiedMeshSet _parentModMeshSet = null;

		/// <summary>
		/// Depth, Texture 변환 같은 특수한 값
		/// </summary>
		[SerializeField]
		public OptExtraValue _extraValue = new OptExtraValue();
		

		// Init
		//--------------------------------------------
		public apOptModifiedMesh_Extra()
		{

		}

		public void Link(apPortrait portrait, apOptModifiedMeshSet parentModMeshSet)
		{
			//_parentModMeshSet = parentModMeshSet;

			_extraValue.Link(portrait);
		}

		// Init - Bake
		//--------------------------------------------
		public void Bake(apModifiedMesh srcModMesh)
		{
			_extraValue.Bake(srcModMesh._extraValue);
		}



		// Sub Class
		//------------------------------------------------------------
		//12.05 추가 : Depth와 텍스쳐를 전환하는 설정
		[Serializable]
		public class OptExtraValue
		{
			[SerializeField]
			public bool _isDepthChanged = false;

			[SerializeField]
			public int _deltaDepth = 0;

			[SerializeField]
			public bool _isTextureChanged = false;

			[SerializeField]
			public int _textureDataID = -1;

			[NonSerialized]
			public apOptTextureData _linkedTextureData = null;

			[SerializeField]
			public float _weightCutout = 0.5f;
			[SerializeField]
			public float _weightCutout_AnimPrev = 0.5f;
			[SerializeField]
			public float _weightCutout_AnimNext = 0.6f;

			public OptExtraValue()
			{
				Init();
			}

			public void Init()
			{
				_isDepthChanged = false;
				_deltaDepth = 0;

				_isTextureChanged = false;
				_textureDataID = -1;
				_linkedTextureData = null;

				_weightCutout = 0.5f;
				_weightCutout_AnimPrev = 0.5f;
				_weightCutout_AnimNext = 0.6f;
			}

			public void Bake(apModifiedMesh.ExtraValue srcValue)
			{
				Init();

				_isDepthChanged = srcValue._isDepthChanged;
				_deltaDepth = srcValue._deltaDepth;
				if(_deltaDepth == 0)
				{
					_isDepthChanged = false;
				}

				_isTextureChanged = srcValue._isTextureChanged;
				_textureDataID = srcValue._textureDataID;
				//_linkedTextureData = null;//이건 나중에 Link
				if(_textureDataID < 0)
				{
					_isTextureChanged = false;
				}

				_weightCutout = srcValue._weightCutout;
				_weightCutout_AnimPrev = srcValue._weightCutout_AnimPrev;
				_weightCutout_AnimNext = srcValue._weightCutout_AnimNext;
			}

			public void Link(apPortrait portrait)
			{
				if(_textureDataID >= 0 && _isTextureChanged)
				{
					apOptTextureData linkedOptTextureData = portrait._optTextureData.Find(delegate(apOptTextureData a)
					{
						return a._srcUniqueID == _textureDataID;
					});

					if(linkedOptTextureData != null)
					{
						_linkedTextureData = linkedOptTextureData;
					}
					else
					{
						_linkedTextureData = null;
						_textureDataID = -1;
						_isTextureChanged = false;
					}
				}
			}
		}
	}
}