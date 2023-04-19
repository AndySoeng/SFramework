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

#if UNITY_EDITOR
using UnityEditor;
#endif

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// MaterialLibrary에서 추가된 재질 설정들
	/// Preset 데이터 : Reserved와 Custom Preset이 있다.
	/// Portrait의 데이터 : Reserved + Preset으로 저장된다.
	/// </summary>
	[Serializable]
	public class apMaterialSet
	{
		// Members
		//-----------------------------------------------
		[SerializeField]
		public string _name = "";

		[SerializeField]
		public int _uniqueID = -1;//고유의 ID (Preset과 Portrait 데이터는 서로 다른 ID를 갖는다.)

		//기본 설정들
		
		
		//파일로 저장된 프리셋인 경우
		[NonSerialized]
		public bool _isReserved = false; //ID에 따라서 Reserved인지, 아니면 CustomPreset인지 알 수 있다.

		[SerializeField]
		public int _linkedPresetID = -1;//Portrait 데이터이면 : 연결된 프리셋의 ID

		

		//Portrait 데이터인 경우
		[NonSerialized]
		public apMaterialSet _linkedPresetMaterial = null;//연결된 프리셋

		[SerializeField]
		public bool _isDefault = true;//다른 설정이 없다면 이 재질이 전체에 적용되는가 (MeshTF에 재질 ID가 별도로 지정되지 않는 경우)

		
		public enum ICON : int
		{
			Unlit = 0,
			Lit = 1,
			LitSpecular = 2,
			LitSpecularEmission = 3,
			LitRimlight = 4,
			LitRamp = 5,
			Effect = 6,
			Cartoon = 7,
			Custom1 = 8,
			Custom2 = 9,
			Custom3 = 10,
			UnlitVR = 11,
			LitVR = 12,
			UnlitMergeable = 13,
			LitMergeable = 14
		}

		[SerializeField]
		public ICON _icon = ICON.Unlit;



		[SerializeField]
		public string _shaderPath_Normal_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_Normal_Additive = "";

		[SerializeField]
		public string _shaderPath_Normal_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_Normal_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_Clipped_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_Clipped_Additive = "";

		[SerializeField]
		public string _shaderPath_Clipped_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_Clipped_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_L_Normal_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_L_Normal_Additive = "";

		[SerializeField]
		public string _shaderPath_L_Normal_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_L_Normal_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_AlphaBlend = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_Additive = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_SoftAdditive = "";

		[SerializeField]
		public string _shaderPath_L_Clipped_Multiplicative = "";

		[SerializeField]
		public string _shaderPath_AlphaMask = "";

		//실제 Shader Asset들
		//파일에서 열때 > Path 위주
		//Portrait에서 저장할 때 > Shader 에셋과 Path 비교하여 결정
		[SerializeField, NonBackupField]
		public Shader _shader_Normal_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Normal_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Normal_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Normal_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_Clipped_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Normal_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_AlphaBlend = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_Additive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_SoftAdditive = null;

		[SerializeField, NonBackupField]
		public Shader _shader_L_Clipped_Multiplicative = null;

		[SerializeField, NonBackupField]
		public Shader _shader_AlphaMask = null;




		public enum SHADER_PROP_TYPE
		{
			Float = 0,
			Int = 1,
			Vector = 2,
			Texture = 3,
			Color = 4
		}


		[Serializable]
		public class PropertySet
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public bool _isReserved = false;//이게 True이면 값을 설정할 수 없다.

			[SerializeField]
			public bool _isOptionEnabled = true;

			[SerializeField]
			public SHADER_PROP_TYPE _propType = SHADER_PROP_TYPE.Float;

			[SerializeField]
			public float _value_Float = 0.0f;

			[SerializeField]
			public int _value_Int = 0;

			[SerializeField]
			public Vector4 _value_Vector = new Vector4(0, 0, 0, 0);

			[SerializeField]
			public Color _value_Color = new Color(0, 0, 0, 1);

			//텍스쳐는 조금 다르다.
			//공통된 하나의 텍스쳐를 이용할지, 이미지(TextureData)마다 다르게 텍스쳐를 입력할지 고를 수 있다.
			[SerializeField]
			public bool _isCommonTexture = true;

			[SerializeField]
			public string _commonTexturePath = "";//<<파일에서 읽을땐 이 경로를 이용한다.

			[SerializeField, NonBackupField]
			public Texture _value_CommonTexture = null;//<Texture2D가 아니라 Texture이다.

			//이 값은 파일로 저장되지 않는다.
			[Serializable]
			public class ImageTexturePair
			{
				[SerializeField]
				public int _textureDataID = -1;

				[NonSerialized]
				public apTextureData _targetTextureData = null;

				[SerializeField]
				public string _textureAssetPath = "";

				[SerializeField, NonBackupField]
				public Texture _textureAsset = null;

				public ImageTexturePair()
				{

				}

				public void CopyFromSrc(ImageTexturePair src)
				{
					_textureDataID = src._textureDataID;
					_targetTextureData = src._targetTextureData;
					_textureAssetPath = src._textureAssetPath;
					_textureAsset = src._textureAsset;
				}
			}

			[SerializeField, NonBackupField]
			public List<ImageTexturePair> _imageTexturePairs = new List<ImageTexturePair>();

			public PropertySet()
			{

			}

			public void CopyFromSrc(PropertySet srcProp)
			{
				_name = srcProp._name;
				_isReserved = srcProp._isReserved;
				_isOptionEnabled = srcProp._isOptionEnabled;
				_propType = srcProp._propType;

				_value_Float = srcProp._value_Float;
				_value_Int = srcProp._value_Int;
				_value_Vector = srcProp._value_Vector;
				_value_Color = srcProp._value_Color;
				_isCommonTexture = srcProp._isCommonTexture;

				_commonTexturePath = srcProp._commonTexturePath;
				_value_CommonTexture = srcProp._value_CommonTexture;

				for (int i = 0; i < srcProp._imageTexturePairs.Count; i++)
				{
					ImageTexturePair newImgTexPair = new ImageTexturePair();
					newImgTexPair.CopyFromSrc(srcProp._imageTexturePairs[i]);

					_imageTexturePairs.Add(newImgTexPair);
				}
				
			}


			public PropertySet SetFloat(float floatValue)
			{
				_value_Float = floatValue;
				return this;
			}

			public PropertySet SetInt(int intValue)
			{
				_value_Int = intValue;
				return this;
			}

			public PropertySet SetVector(Vector4 vectorValue)
			{
				_value_Vector = vectorValue;
				return this;
			}

			public PropertySet SetColor(Color colorValue)
			{
				_value_Color = colorValue;
				return this;
			}


		}

		[SerializeField]
		public List<PropertySet> _propertySets = new List<PropertySet>();
		

		[SerializeField]
		public bool _isNeedToSetBlackColoredAmbient = false;


		// Init
		//-----------------------------------------------
		public apMaterialSet()
		{
			//Material mat;
			//mat.Set
			
		}

		public void Init()
		{
			_name = "";
			_uniqueID = -1;
			_isReserved = false;
			_linkedPresetID = -1;
			_linkedPresetMaterial = null;
			_isDefault = false;

			_icon = ICON.Unlit;

			_shaderPath_Normal_AlphaBlend = "";
			_shaderPath_Normal_Additive = "";
			_shaderPath_Normal_SoftAdditive = "";
			_shaderPath_Normal_Multiplicative = "";
			_shaderPath_Clipped_AlphaBlend = "";
			_shaderPath_Clipped_Additive = "";
			_shaderPath_Clipped_SoftAdditive = "";
			_shaderPath_Clipped_Multiplicative = "";
			_shaderPath_L_Normal_AlphaBlend = "";
			_shaderPath_L_Normal_Additive = "";
			_shaderPath_L_Normal_SoftAdditive = "";
			_shaderPath_L_Normal_Multiplicative = "";
			_shaderPath_L_Clipped_AlphaBlend = "";
			_shaderPath_L_Clipped_Additive = "";
			_shaderPath_L_Clipped_SoftAdditive = "";
			_shaderPath_L_Clipped_Multiplicative = "";
			_shaderPath_AlphaMask = "";

			_shader_Normal_AlphaBlend = null;
			_shader_Normal_Additive = null;
			_shader_Normal_SoftAdditive = null;
			_shader_Normal_Multiplicative = null;
			_shader_Clipped_AlphaBlend = null;
			_shader_Clipped_Additive = null;
			_shader_Clipped_SoftAdditive = null;
			_shader_Clipped_Multiplicative = null;
			_shader_L_Normal_AlphaBlend = null;
			_shader_L_Normal_Additive = null;
			_shader_L_Normal_SoftAdditive = null;
			_shader_L_Normal_Multiplicative = null;
			_shader_L_Clipped_AlphaBlend = null;
			_shader_L_Clipped_Additive = null;
			_shader_L_Clipped_SoftAdditive = null;
			_shader_L_Clipped_Multiplicative = null;
			_shader_AlphaMask = null;

			_propertySets.Clear();

			_isNeedToSetBlackColoredAmbient = false;
		}

		// Link
		//-----------------------------------------------



		// Functions
		//-----------------------------------------------
		public apMaterialSet MakeReserved(	int uniqueID, 
											string name, 
											ICON icon,
											string shaderPath_Normal_AlphaBlend,
											string shaderPath_Normal_Additive,
											string shaderPath_Normal_SoftAdditive,
											string shaderPath_Normal_Multiplicative,
											string shaderPath_Clipped_AlphaBlend,
											string shaderPath_Clipped_Additive,
											string shaderPath_Clipped_SoftAdditive,
											string shaderPath_Clipped_Multiplicative,
											string shaderPath_L_Normal_AlphaBlend,
											string shaderPath_L_Normal_Additive,
											string shaderPath_L_Normal_SoftAdditive,
											string shaderPath_L_Normal_Multiplicative,
											string shaderPath_L_Clipped_AlphaBlend,
											string shaderPath_L_Clipped_Additive,
											string shaderPath_L_Clipped_SoftAdditive,
											string shaderPath_L_Clipped_Multiplicative,
											string shaderPath_AlphaMask,
											bool isNeedToSetBlackColoredAmbient
										)
		{
			_uniqueID = uniqueID;
			_name = name;

			if(_uniqueID < 10)
			{
				_isReserved = true;
			}
			else
			{
				_isReserved = false;
			}
			_linkedPresetID = -1;
			_linkedPresetMaterial = null;
			_isDefault = false;

			_icon = icon;


			_shaderPath_Normal_AlphaBlend = shaderPath_Normal_AlphaBlend;
			_shaderPath_Normal_Additive = shaderPath_Normal_Additive;
			_shaderPath_Normal_SoftAdditive = shaderPath_Normal_SoftAdditive;
			_shaderPath_Normal_Multiplicative = shaderPath_Normal_Multiplicative;
			_shaderPath_Clipped_AlphaBlend = shaderPath_Clipped_AlphaBlend;
			_shaderPath_Clipped_Additive = shaderPath_Clipped_Additive;
			_shaderPath_Clipped_SoftAdditive = shaderPath_Clipped_SoftAdditive;
			_shaderPath_Clipped_Multiplicative = shaderPath_Clipped_Multiplicative;
			_shaderPath_L_Normal_AlphaBlend = shaderPath_L_Normal_AlphaBlend;
			_shaderPath_L_Normal_Additive = shaderPath_L_Normal_Additive;
			_shaderPath_L_Normal_SoftAdditive = shaderPath_L_Normal_SoftAdditive;
			_shaderPath_L_Normal_Multiplicative = shaderPath_L_Normal_Multiplicative;
			_shaderPath_L_Clipped_AlphaBlend = shaderPath_L_Clipped_AlphaBlend;
			_shaderPath_L_Clipped_Additive = shaderPath_L_Clipped_Additive;
			_shaderPath_L_Clipped_SoftAdditive = shaderPath_L_Clipped_SoftAdditive;
			_shaderPath_L_Clipped_Multiplicative = shaderPath_L_Clipped_Multiplicative;
			_shaderPath_AlphaMask = shaderPath_AlphaMask;

			_propertySets.Clear();

			_isNeedToSetBlackColoredAmbient = isNeedToSetBlackColoredAmbient;

#if UNITY_EDITOR
			LoadShaderAssets();
#endif



			return this;
		}

#if UNITY_EDITOR
		private void LoadShaderAssets()
		{	
			_shader_Normal_AlphaBlend =			LoadShader(_shaderPath_Normal_AlphaBlend);
			_shader_Normal_Additive =			LoadShader(_shaderPath_Normal_Additive);
			_shader_Normal_SoftAdditive =		LoadShader(_shaderPath_Normal_SoftAdditive);
			_shader_Normal_Multiplicative =		LoadShader(_shaderPath_Normal_Multiplicative);
			_shader_Clipped_AlphaBlend =		LoadShader(_shaderPath_Clipped_AlphaBlend);
			_shader_Clipped_Additive =			LoadShader(_shaderPath_Clipped_Additive);
			_shader_Clipped_SoftAdditive =		LoadShader(_shaderPath_Clipped_SoftAdditive);
			_shader_Clipped_Multiplicative =	LoadShader(_shaderPath_Clipped_Multiplicative);
			_shader_L_Normal_AlphaBlend =		LoadShader(_shaderPath_L_Normal_AlphaBlend);
			_shader_L_Normal_Additive =			LoadShader(_shaderPath_L_Normal_Additive);
			_shader_L_Normal_SoftAdditive =		LoadShader(_shaderPath_L_Normal_SoftAdditive);
			_shader_L_Normal_Multiplicative =	LoadShader(_shaderPath_L_Normal_Multiplicative);
			_shader_L_Clipped_AlphaBlend =		LoadShader(_shaderPath_L_Clipped_AlphaBlend);
			_shader_L_Clipped_Additive =		LoadShader(_shaderPath_L_Clipped_Additive);
			_shader_L_Clipped_SoftAdditive =	LoadShader(_shaderPath_L_Clipped_SoftAdditive);
			_shader_L_Clipped_Multiplicative =	LoadShader(_shaderPath_L_Clipped_Multiplicative);
			_shader_AlphaMask =					LoadShader(_shaderPath_AlphaMask);
				
			
		}

		private Shader LoadShader(string path)
		{
			return AssetDatabase.LoadAssetAtPath<Shader>(path);
		}
#endif

		public PropertySet AddProperty(string name, bool isReserved, SHADER_PROP_TYPE propType)
		{
			PropertySet newPropertySet = new PropertySet();
			newPropertySet._name = name;
			newPropertySet._isReserved = isReserved;
			newPropertySet._isOptionEnabled = true;
			newPropertySet._propType = propType;

			_propertySets.Add(newPropertySet);

			return newPropertySet;
		}

		public PropertySet AddProperty_Texture(string name, bool isControlledByAnyPortrait, bool isCommonTexture)
		{
			PropertySet newProp = AddProperty(name, isControlledByAnyPortrait, SHADER_PROP_TYPE.Texture);

			newProp._isCommonTexture = isCommonTexture;

			return newProp;
		}
		
		/// <summary>
		/// MaterialSet으로 부터 생성한다.
		/// </summary>
		/// <param name="srcMat"></param>
		/// <param name="uniqueID"></param>
		/// <param name="isFromPreset"></param>
		/// <param name="isDefault"></param>
		public void CopyFromSrc(apMaterialSet srcMat, int uniqueID, bool isFromPreset, bool isPreset, bool isDefault)
		{
			_uniqueID = uniqueID;
			_name = srcMat._name;

			
			if (!isPreset)
			{
				//프리셋이 아닌 경우
				_isReserved = false;
				if (isFromPreset)
				{
					//Src가 Preset인 경우
					_linkedPresetID = srcMat._uniqueID;
					_linkedPresetMaterial = srcMat;
				}
				else
				{
					//Src가 일반 MaterialSet인 경우
					//같은 프리셋 공유
					_linkedPresetID = srcMat._linkedPresetID;
					_linkedPresetMaterial = srcMat._linkedPresetMaterial;
				}


				_isDefault = isDefault;
			}
			else
			{
				//프리셋인 경우
				_isReserved = false;
				_linkedPresetID = -1;
				_linkedPresetMaterial = null;
			}

			_icon = srcMat._icon;



			_shader_Normal_AlphaBlend = null;
			_shader_Normal_Additive = null;
			_shader_Normal_SoftAdditive = null;
			_shader_Normal_Multiplicative = null;

			_shader_Clipped_AlphaBlend = null;
			_shader_Clipped_Additive = null;
			_shader_Clipped_SoftAdditive = null;
			_shader_Clipped_Multiplicative = null;

			_shader_L_Normal_AlphaBlend = null;
			_shader_L_Normal_Additive = null;
			_shader_L_Normal_SoftAdditive = null;
			_shader_L_Normal_Multiplicative = null;

			_shader_L_Clipped_AlphaBlend = null;
			_shader_L_Clipped_Additive = null;
			_shader_L_Clipped_SoftAdditive = null;
			_shader_L_Clipped_Multiplicative = null;

			_shader_AlphaMask = null;


			//변경 : 22.7.11 : Shader 에셋도 복사
			if(srcMat._shader_Normal_AlphaBlend != null)		{ _shader_Normal_AlphaBlend =		srcMat._shader_Normal_AlphaBlend; }
			if(srcMat._shader_Normal_Additive != null)			{ _shader_Normal_Additive =			srcMat._shader_Normal_Additive; }
			if(srcMat._shader_Normal_SoftAdditive != null)		{ _shader_Normal_SoftAdditive =		srcMat._shader_Normal_SoftAdditive; }
			if(srcMat._shader_Normal_Multiplicative != null)	{ _shader_Normal_Multiplicative =	srcMat._shader_Normal_Multiplicative; }

			if(srcMat._shader_Clipped_AlphaBlend != null)		{ _shader_Clipped_AlphaBlend =		srcMat._shader_Clipped_AlphaBlend; }
			if(srcMat._shader_Clipped_Additive != null)			{ _shader_Clipped_Additive =		srcMat._shader_Clipped_Additive; }
			if(srcMat._shader_Clipped_SoftAdditive != null)		{ _shader_Clipped_SoftAdditive =	srcMat._shader_Clipped_SoftAdditive; }
			if(srcMat._shader_Clipped_Multiplicative != null)	{ _shader_Clipped_Multiplicative =	srcMat._shader_Clipped_Multiplicative; }

			if(srcMat._shader_L_Normal_AlphaBlend != null)		{ _shader_L_Normal_AlphaBlend =		srcMat._shader_L_Normal_AlphaBlend; }
			if(srcMat._shader_L_Normal_Additive != null)		{ _shader_L_Normal_Additive =		srcMat._shader_L_Normal_Additive; }
			if(srcMat._shader_L_Normal_SoftAdditive != null)	{ _shader_L_Normal_SoftAdditive =	srcMat._shader_L_Normal_SoftAdditive; }
			if(srcMat._shader_L_Normal_Multiplicative != null)	{ _shader_L_Normal_Multiplicative = srcMat._shader_L_Normal_Multiplicative; }

			if(srcMat._shader_L_Clipped_AlphaBlend != null)		{ _shader_L_Clipped_AlphaBlend =	srcMat._shader_L_Clipped_AlphaBlend; }
			if(srcMat._shader_L_Clipped_Additive != null)		{ _shader_L_Clipped_Additive =		srcMat._shader_L_Clipped_Additive; }
			if(srcMat._shader_L_Clipped_SoftAdditive != null)	{ _shader_L_Clipped_SoftAdditive =	srcMat._shader_L_Clipped_SoftAdditive; }
			if(srcMat._shader_L_Clipped_Multiplicative != null)	{ _shader_L_Clipped_Multiplicative = srcMat._shader_L_Clipped_Multiplicative; }

			if(srcMat._shaderPath_AlphaMask != null)			{ _shader_AlphaMask =				srcMat._shader_AlphaMask; }

			_shaderPath_Normal_AlphaBlend =		srcMat._shaderPath_Normal_AlphaBlend;
			_shaderPath_Normal_Additive =		srcMat._shaderPath_Normal_Additive;
			_shaderPath_Normal_SoftAdditive =	srcMat._shaderPath_Normal_SoftAdditive;
			_shaderPath_Normal_Multiplicative = srcMat._shaderPath_Normal_Multiplicative;
			_shaderPath_Clipped_AlphaBlend =	srcMat._shaderPath_Clipped_AlphaBlend;
			_shaderPath_Clipped_Additive =		srcMat._shaderPath_Clipped_Additive;
			_shaderPath_Clipped_SoftAdditive =	srcMat._shaderPath_Clipped_SoftAdditive;
			_shaderPath_Clipped_Multiplicative = srcMat._shaderPath_Clipped_Multiplicative;
			_shaderPath_L_Normal_AlphaBlend =	srcMat._shaderPath_L_Normal_AlphaBlend;
			_shaderPath_L_Normal_Additive =		srcMat._shaderPath_L_Normal_Additive;
			_shaderPath_L_Normal_SoftAdditive = srcMat._shaderPath_L_Normal_SoftAdditive;
			_shaderPath_L_Normal_Multiplicative =	srcMat._shaderPath_L_Normal_Multiplicative;
			_shaderPath_L_Clipped_AlphaBlend =	srcMat._shaderPath_L_Clipped_AlphaBlend;
			_shaderPath_L_Clipped_Additive =	srcMat._shaderPath_L_Clipped_Additive;
			_shaderPath_L_Clipped_SoftAdditive =	srcMat._shaderPath_L_Clipped_SoftAdditive;
			_shaderPath_L_Clipped_Multiplicative =	srcMat._shaderPath_L_Clipped_Multiplicative;
			_shaderPath_AlphaMask =				srcMat._shaderPath_AlphaMask;

			_propertySets.Clear();

			_isNeedToSetBlackColoredAmbient = srcMat._isNeedToSetBlackColoredAmbient;

#if UNITY_EDITOR
			LoadShaderAssets();
#endif

			//프로퍼티 복사
			for (int i = 0; i < srcMat._propertySets.Count; i++)
			{
				PropertySet newPropSet = new PropertySet();
				newPropSet.CopyFromSrc(srcMat._propertySets[i]);

				_propertySets.Add(newPropSet);
			}
		}



		// Save / Load
		//-----------------------------------------------
		public void Save(StreamWriter sw)
		{
			try
			{
				//KEY 4글자 + 값
				sw.WriteLine("NAME" + _name);
				sw.WriteLine("UNID" + _uniqueID);

				sw.WriteLine("ICON" + (int)_icon);

				//  G   /     L  +    N   /   C     +     AB    /   AD   /    SA      /    MP
				//Gamma / Linear + Noraml / Clipped + AlphaBlend/Additive/SoftAdditive/Multiplicative
				sw.WriteLine("GNAB" + _shaderPath_Normal_AlphaBlend);
				sw.WriteLine("GNAD" + _shaderPath_Normal_Additive);
				sw.WriteLine("GNSA" + _shaderPath_Normal_SoftAdditive);
				sw.WriteLine("GNMP" + _shaderPath_Normal_Multiplicative);
				sw.WriteLine("GCAB" + _shaderPath_Clipped_AlphaBlend);
				sw.WriteLine("GCAD" + _shaderPath_Clipped_Additive);
				sw.WriteLine("GCSA" + _shaderPath_Clipped_SoftAdditive);
				sw.WriteLine("GCMP" + _shaderPath_Clipped_Multiplicative);

				sw.WriteLine("LNAB" + _shaderPath_L_Normal_AlphaBlend);
				sw.WriteLine("LNAD" + _shaderPath_L_Normal_Additive);
				sw.WriteLine("LNSA" + _shaderPath_L_Normal_SoftAdditive);
				sw.WriteLine("LNMP" + _shaderPath_L_Normal_Multiplicative);
				sw.WriteLine("LCAB" + _shaderPath_L_Clipped_AlphaBlend);
				sw.WriteLine("LCAD" + _shaderPath_L_Clipped_Additive);
				sw.WriteLine("LCSA" + _shaderPath_L_Clipped_SoftAdditive);
				sw.WriteLine("LCMP" + _shaderPath_L_Clipped_Multiplicative);

				sw.WriteLine("MASK" + _shaderPath_AlphaMask);

				sw.WriteLine("AMBC" + (_isNeedToSetBlackColoredAmbient ? "true" : "false"));

				//Prop은.. 구분자로 개수 확인
				sw.WriteLine("PROP" + _propertySets.Count);

				for (int i = 0; i < _propertySets.Count; i++)
				{
					PropertySet propSet = _propertySets[i];

					sw.WriteLine("PNAM" + propSet._name);
					sw.WriteLine("PTYP" + (int)propSet._propType);
					sw.WriteLine("PTRV" + (propSet._isReserved ? "true" : "false"));
					sw.WriteLine("PVFL" + propSet._value_Float);
					sw.WriteLine("PVIT" + propSet._value_Int);
					sw.WriteLine("PVVX" + propSet._value_Vector.x);
					sw.WriteLine("PVVY" + propSet._value_Vector.y);
					sw.WriteLine("PVVZ" + propSet._value_Vector.z);
					sw.WriteLine("PVVW" + propSet._value_Vector.w);
					sw.WriteLine("PVCR" + propSet._value_Color.r);
					sw.WriteLine("PVCG" + propSet._value_Color.g);
					sw.WriteLine("PVCB" + propSet._value_Color.b);
					sw.WriteLine("PVCA" + propSet._value_Color.a);

					sw.WriteLine("PCMT" + (propSet._isCommonTexture ? "true" : "false"));
					sw.WriteLine("PCTP" + propSet._commonTexturePath);

					sw.WriteLine(">>>>>>>>");//구분자 : 이 구문을 만나면 파싱한 PropSet을 리스트에 넣자
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("MaterialPreset Write Exception : " + ex);
			}
		}


		public void Load(List<string> loadedStringSet)
		{

			_name = "";
			_uniqueID = -1;

			string strKey = "";
			string strValue = "";
			string strCur = "";

			_propertySets.Clear();
			int nPropSets = 0;

			PropertySet newPropSet = null;

			for (int i = 0; i < loadedStringSet.Count; i++)
			{
				strCur = loadedStringSet[i];
				if (strCur.Length < 4)
				{ continue; }

				//Key가 4글자
				//나머지가 Value
				strKey = strCur.Substring(0, 4);

				if (strCur.Length > 4)
				{
					strValue = strCur.Substring(4);
				}
				else
				{
					strValue = "";
				}

				try
				{
					if(strKey == "NAME") { _name = strValue; }
					else if(strKey == "UNID")
					{
						_uniqueID = int.Parse(strValue);
						if(_uniqueID < 10)
						{
							_isReserved = true;
						}
						else
						{
							_isReserved = false;
						}
					}
					else if(strKey == "ICON") { _icon = (ICON)(int.Parse(strValue)); }

					else if(strKey == "GNAB") { _shaderPath_Normal_AlphaBlend = strValue; }
					else if(strKey == "GNAD") { _shaderPath_Normal_Additive = strValue; }
					else if(strKey == "GNSA") { _shaderPath_Normal_SoftAdditive = strValue; }
					else if(strKey == "GNMP") { _shaderPath_Normal_Multiplicative = strValue; }
					else if(strKey == "GCAB") { _shaderPath_Clipped_AlphaBlend = strValue; }
					else if(strKey == "GCAD") { _shaderPath_Clipped_Additive = strValue; }
					else if(strKey == "GCSA") { _shaderPath_Clipped_SoftAdditive = strValue; }
					else if(strKey == "GCMP") { _shaderPath_Clipped_Multiplicative = strValue; }

					else if(strKey == "LNAB") { _shaderPath_L_Normal_AlphaBlend = strValue; }
					else if(strKey == "LNAD") { _shaderPath_L_Normal_Additive = strValue; }
					else if(strKey == "LNSA") { _shaderPath_L_Normal_SoftAdditive = strValue; }
					else if(strKey == "LNMP") { _shaderPath_L_Normal_Multiplicative = strValue; }
					else if(strKey == "LCAB") { _shaderPath_L_Clipped_AlphaBlend = strValue; }
					else if(strKey == "LCAD") { _shaderPath_L_Clipped_Additive = strValue; }
					else if(strKey == "LCSA") { _shaderPath_L_Clipped_SoftAdditive = strValue; }
					else if(strKey == "LCMP") { _shaderPath_L_Clipped_Multiplicative = strValue; }

					else if(strKey == "MASK") { _shaderPath_AlphaMask = strValue; }
					else if(strKey == "AMBC") { _isNeedToSetBlackColoredAmbient = strValue.Contains("true"); }

					else if(strKey == "PROP")
					{
						nPropSets = int.Parse(strValue);

						if(nPropSets > 0)
						{
							//새로운 PropSet을 만든다.
							newPropSet = new PropertySet();
						}
					}
					else if(strKey == "PNAM") { if(newPropSet != null) { newPropSet._name = strValue; } }
					else if(strKey == "PTYP") { if(newPropSet != null) { newPropSet._propType = (SHADER_PROP_TYPE)(int.Parse(strValue)); } }
					else if(strKey == "PTRV") { if(newPropSet != null) { newPropSet._isReserved = strValue.Contains("true"); } }
					else if(strKey == "PVFL") { if(newPropSet != null) { newPropSet._value_Float = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVIT") { if(newPropSet != null) { newPropSet._value_Int = int.Parse(strValue); } }
					else if(strKey == "PVVX") { if(newPropSet != null) { newPropSet._value_Vector.x = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVVY") { if(newPropSet != null) { newPropSet._value_Vector.y = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVVZ") { if(newPropSet != null) { newPropSet._value_Vector.z = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVVW") { if(newPropSet != null) { newPropSet._value_Vector.w = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCR") { if(newPropSet != null) { newPropSet._value_Color.r = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCG") { if(newPropSet != null) { newPropSet._value_Color.g = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCB") { if(newPropSet != null) { newPropSet._value_Color.b = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PVCA") { if(newPropSet != null) { newPropSet._value_Color.a = apUtil.ParseFloat(strValue); } }
					else if(strKey == "PCMT") { if(newPropSet != null) { newPropSet._isCommonTexture = strValue.Contains("true"); } }
					else if(strKey == "PCTP") { if(newPropSet != null) { newPropSet._commonTexturePath = strValue; } }
					else if(strKey == ">>>>")
					{
						//지금까지 만든 PropSet을 리스트에 넣자.
						if (newPropSet != null)
						{
							_propertySets.Add(newPropSet);
							newPropSet = new PropertySet();//새로운 PropSet 생성
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("MaterialSet Load Exception : " + ex);
				}
			}

			if(newPropSet != null)
			{
				newPropSet = null;
			}
		}


		// Get / Set
		//-----------------------------------------------
		public Shader GetShader(apPortrait.SHADER_TYPE shaderType, bool isClippedChild, bool isLinearSpace)
		{
			if(!isLinearSpace)
			{
				//Gamma Space
				switch (shaderType)
				{
					case apPortrait.SHADER_TYPE.AlphaBlend:
						if(!isClippedChild)		{ return _shader_Normal_AlphaBlend; }
						else					{ return _shader_Clipped_AlphaBlend;}

					case apPortrait.SHADER_TYPE.Additive:
						if(!isClippedChild)		{ return _shader_Normal_Additive; }
						else					{ return _shader_Clipped_Additive;}

					case apPortrait.SHADER_TYPE.SoftAdditive:
						if(!isClippedChild)		{ return _shader_Normal_SoftAdditive; }
						else					{ return _shader_Clipped_SoftAdditive;}

					case apPortrait.SHADER_TYPE.Multiplicative:
						if(!isClippedChild)		{ return _shader_Normal_Multiplicative; }
						else					{ return _shader_Clipped_Multiplicative;}
				}
			}
			else
			{
				//Linear Space
				switch (shaderType)
				{
					case apPortrait.SHADER_TYPE.AlphaBlend:
						if(!isClippedChild)		{ return _shader_L_Normal_AlphaBlend; }
						else					{ return _shader_L_Clipped_AlphaBlend;}

					case apPortrait.SHADER_TYPE.Additive:
						if(!isClippedChild)		{ return _shader_L_Normal_Additive; }
						else					{ return _shader_L_Clipped_Additive;}

					case apPortrait.SHADER_TYPE.SoftAdditive:
						if(!isClippedChild)		{ return _shader_L_Normal_SoftAdditive; }
						else					{ return _shader_L_Clipped_SoftAdditive;}

					case apPortrait.SHADER_TYPE.Multiplicative:
						if(!isClippedChild)		{ return _shader_L_Normal_Multiplicative; }
						else					{ return _shader_L_Clipped_Multiplicative;}
				}
			}

			return null;
		}

	}
}