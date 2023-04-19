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
	/// v1.1.7에 추가된 Material Library/Set의 Opt로 Bake된 클래스
	/// 재질의 기본 설정들을 담고 있다.
	/// 실제 재질은 없으며, 이 정보를 바탕으로 재질 초기화 및 Batched/Shared를 처리한다.
	/// 이 클래스의 정보로 Key가 된다.
	/// 이 값이 없거나 Baked되지 않았다면 기존의 값 (Texture, Shader_Normal)만으로만 Key를 만들어야 한다.
	/// Custom Shader도 적용 가능하며, 만약 Clipped Child여도 재질 초기화를 위해서 필요하다 (Batched가 안되더라도..)
	/// </summary>
	[Serializable]
	public class apOptMaterialInfo
	{
		// Members
		//---------------------------------------------------
		//값이 Bake되었는가
		[SerializeField]
		public bool _isBaked = false;//<<이게 false라면 v1.1.7 이전의 방식으로 저장해야한다.

		//기본 키값
		[SerializeField, NonBackupField]
		public Texture2D _mainTex = null;

		[SerializeField, NonBackupField]
		public int _textureID = -1;

		[SerializeField, NonBackupField]
		public Shader _shader = null;
		
		//추가된 속성 정보
		//이거때문에 결국 v1.1.7과 이전 버전이 갈린 것이다.
		[Serializable]
		public class Property_Float
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public float _value = 0.0f;

			public Property_Float() { }
			public Property_Float(string name, float value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Int
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public int _value = 0;

			public Property_Int() { }
			public Property_Int(string name, int value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Vector
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public Vector4 _value = Vector4.zero;

			public Property_Vector() { }
			public Property_Vector(string name, Vector4 value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Texture
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public Texture _value = null;

			public Property_Texture() { }
			public Property_Texture(string name, Texture value)
			{
				_name = name;
				_value = value;
			}
		}

		[Serializable]
		public class Property_Color
		{
			[SerializeField]
			public string _name = "";

			[SerializeField]
			public Color _value = Color.clear;

			public Property_Color() { }
			public Property_Color(string name, Color value)
			{
				_name = name;
				_value = value;
			}
		}



		[SerializeField, NonBackupField]
		public Property_Float[] _props_Float = null;

		[SerializeField, NonBackupField]
		public Property_Int[] _props_Int = null;

		[SerializeField, NonBackupField]
		public Property_Vector[] _props_Vector = null;

		[SerializeField, NonBackupField]
		public Property_Texture[] _props_Texture = null;

		[SerializeField]
		public Property_Color[] _props_Color = null;

		//변경 21.12.22 : 예약된 프로퍼티를 const 변수로 빼자
		private const string RESERVED_PROP__COLOR = "_Color";
		private const string RESERVED_PROP__MAIN_TEX = "_MainTex";
		private const string RESERVED_PROP__MASK_TEX = "_MaskTex";
		private const string RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET = "_MaskScreenSpaceOffset";

		//추가 21.12.22 : 9개의 병합용 Tex를 더 받을 수 있다. 하나는 MainTex를 사용하여 총 10개
		private const string RESERVED_PROP__MERGED_TEX_1 = "_MergedTex1";
		private const string RESERVED_PROP__MERGED_TEX_2 = "_MergedTex2";
		private const string RESERVED_PROP__MERGED_TEX_3 = "_MergedTex3";
		private const string RESERVED_PROP__MERGED_TEX_4 = "_MergedTex4";
		private const string RESERVED_PROP__MERGED_TEX_5 = "_MergedTex5";
		private const string RESERVED_PROP__MERGED_TEX_6 = "_MergedTex6";
		private const string RESERVED_PROP__MERGED_TEX_7 = "_MergedTex7";
		private const string RESERVED_PROP__MERGED_TEX_8 = "_MergedTex8";
		private const string RESERVED_PROP__MERGED_TEX_9 = "_MergedTex9";


		//추가 21.12.24 : 병합 가능한지 여부 : 병합용 텍스쳐가 있어야 하며, 알파블렌딩이어야 한다.
		[SerializeField]
		public bool _isMergable = false;


		// Init
		//---------------------------------------------------
		public apOptMaterialInfo()
		{

		}

		public void Clear()
		{
			_isBaked = false;
			_mainTex = null;
			_textureID = -1;
			_shader = null;
			
			_props_Float = null;
			_props_Int = null;
			_props_Vector = null;
			_props_Texture = null;
			_props_Color = null;
		}

		


		// Bake
		//---------------------------------------------------
#if UNITY_EDITOR
		public void Bake(apTransform_Mesh srcMeshTransform, apPortrait portrait, bool isLinearSpace, int textureDataID, int srcTextureDataID, apMaterialLibrary materialLibrary)
		{
			Clear();

			//1. 기본 텍스쳐를 만든다.
			Texture2D mainTex = null;

			if(srcMeshTransform._mesh != null
				&& srcMeshTransform._mesh.LinkedTextureData != null)
			{
				mainTex = srcMeshTransform._mesh.LinkedTextureData._image;
			}

			if(mainTex == null)
			{
				return;
			}

			//2. Shader를 연결한다.
			Shader targetShader = null;
			apMaterialSet srcMatSet = null;

			apMaterialSet defaultMatSet = portrait.GetDefaultMaterialSet();
			apMaterialSet libraryMatSet = materialLibrary.Presets[0];//<<Library의 첫번째 프리셋

			bool isClippedChild = srcMeshTransform._isClipping_Child;

			if (srcMeshTransform._isCustomShader)
			{
				//2-1. Custom Shader를 사용하는 경우
				targetShader = srcMeshTransform._customShader;
			}
			else
			{
				srcMatSet = srcMeshTransform._linkedMaterialSet;
				if (srcMatSet == null)
				{
					srcMatSet = defaultMatSet;
				}

				if(srcMatSet == null)
				{
					srcMatSet = libraryMatSet;
				}

				if (srcMatSet == null)
				{
					return;
				}

				//조건에 맞는 Shader를 받아오자
				targetShader = srcMatSet.GetShader(srcMeshTransform._shaderType, isClippedChild, isLinearSpace);

				//만약 현재 Material Set에 Shader가 없다면, Default Shader에서 가져오자.
				if(targetShader == null)
				{
					targetShader = defaultMatSet.GetShader(srcMeshTransform._shaderType, isClippedChild, isLinearSpace);
				}
			}

			if(targetShader == null)
			{
				//Shader가 없으면 Material Liabrary에서 가져오자
				targetShader = libraryMatSet.GetShader(srcMeshTransform._shaderType, isClippedChild, isLinearSpace);
			}

			
			if(targetShader == null || mainTex == null)
			{
				return;
			}

			// 일단 MainTex와 Shader를 연결하자
			_isBaked = true;
			_mainTex = mainTex;
			_textureID = textureDataID;
			_shader = targetShader;
			
			//3. 프로퍼티
			List<Property_Float> list_Float = new List<Property_Float>();
			List<Property_Int> list_Int = new List<Property_Int>();
			List<Property_Vector> list_Vector = new List<Property_Vector>();
			List<Property_Texture> list_Texture = new List<Property_Texture>();
			List<Property_Color> list_Color = new List<Property_Color>();

			//테스트를 위해서 Material을 생성
			Material mat_Test = new Material(_shader);

			


			//3-1. 먼저 MaterialSet의 정보를 리스트로 저장한다.
			if (srcMatSet != null)
			{
				for (int iSrcProp = 0; iSrcProp < srcMatSet._propertySets.Count; iSrcProp++)
				{
					apMaterialSet.PropertySet srcProp = srcMatSet._propertySets[iSrcProp];
					if (srcProp._isReserved ||
						!srcProp._isOptionEnabled)
					{
						continue;
					}

					//이전
					//if(	string.Equals(srcProp._name, "_Color") ||
					//	string.Equals(srcProp._name, "_MainTex") ||
					//	string.Equals(srcProp._name, "_MaskTex") ||
					//	string.Equals(srcProp._name, "_MaskScreenSpaceOffset") ||
					//	string.IsNullOrEmpty(srcProp._name))

					//변경 21.12.22 : const 변수로 바뀌었으며, "병합용" 텍스쳐가 추가되었다.
					if(	string.Equals(srcProp._name, RESERVED_PROP__COLOR) ||
						string.Equals(srcProp._name, RESERVED_PROP__MAIN_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET) ||

						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_1) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_2) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_3) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_4) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_5) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_6) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_7) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_8) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_9) ||

						string.IsNullOrEmpty(srcProp._name))

					{
						//이 값은 사용할 수 없다. Reserved임
						continue;
					}

					//프로퍼티가 있는 경우에만
					bool isHasProp = mat_Test.HasProperty(srcProp._name);
					if (!isHasProp)
					{
						//없는 Property.
						continue;
					}


					switch (srcProp._propType)
					{
						case apMaterialSet.SHADER_PROP_TYPE.Float:
							AddProperty_Float(list_Float, srcProp._name, srcProp._value_Float);
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Int:
							AddProperty_Int(list_Int, srcProp._name, srcProp._value_Int);
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Vector:
							AddProperty_Vector(list_Vector, srcProp._name, srcProp._value_Vector);
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Texture:
							if (srcProp._isCommonTexture)
							{
								//공통 텍스쳐인 경우
								AddProperty_Texture(list_Texture, srcProp._name, srcProp._value_CommonTexture);
							}
							else
							{
								//TextureData에 해당하는 정보가 있는지 확인하자.
								//없어도 null값을 넣는다. (null로 초기화하고 싶을 때도 있겠징...)
								//Debug.Log("Bake > [" + textureDataID + "]");
								apMaterialSet.PropertySet.ImageTexturePair imgTexPair = srcProp._imageTexturePairs.Find(delegate (apMaterialSet.PropertySet.ImageTexturePair a)
								{	
									//이전 [버그 : ImageTexturePair는 랜덤 ID로 구성된 반면, 파라미터인 textureDataID는 0, 1, 2로 증가하는 Opt ID이다.]
									//return a._textureDataID == textureDataID;
									return a._textureDataID == srcTextureDataID;
								});
								if (imgTexPair != null)
								{
									//Debug.Log(">> 제대로 적용됨 [" + srcProp._name + "] >> " + (imgTexPair._textureAsset != null ? imgTexPair._textureAsset.name : "Null"));
									AddProperty_Texture(list_Texture, srcProp._name, imgTexPair._textureAsset);
								}
								else
								{
									//Debug.LogError("Null 값이 적용되었다 [" + srcProp._name + "]");
									AddProperty_Texture(list_Texture, srcProp._name, null);
								}
							}
							break;

						case apMaterialSet.SHADER_PROP_TYPE.Color:
							AddProperty_Color(list_Color, srcProp._name, srcProp._value_Color);
							break;
					}

				}
			}


			//3-2. MeshTransform의 속성에 Overwrite 설정이 있을 수도 있다.
			if(srcMeshTransform._customMaterialProperties != null &&
				srcMeshTransform._customMaterialProperties.Count > 0)
			{
				for (int iSrcProp = 0; iSrcProp < srcMeshTransform._customMaterialProperties.Count; iSrcProp++)
				{
					apTransform_Mesh.CustomMaterialProperty srcProp = srcMeshTransform._customMaterialProperties[iSrcProp];

					//이전
					//if(	string.Equals(srcProp._name, "_Color") ||
					//	string.Equals(srcProp._name, "_MainTex") ||
					//	string.Equals(srcProp._name, "_MaskTex") ||
					//	string.Equals(srcProp._name, "_MaskScreenSpaceOffset") ||
					//	string.IsNullOrEmpty(srcProp._name))

					//변경 21.12.22 : const 변수로 바뀌었으며, "병합용" 텍스쳐가 추가되었다.
					if(	string.Equals(srcProp._name, RESERVED_PROP__COLOR) ||
						string.Equals(srcProp._name, RESERVED_PROP__MAIN_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_TEX) ||
						string.Equals(srcProp._name, RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_1) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_2) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_3) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_4) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_5) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_6) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_7) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_8) ||
						string.Equals(srcProp._name, RESERVED_PROP__MERGED_TEX_9) ||
						string.IsNullOrEmpty(srcProp._name))
					{
						//이 값은 사용할 수 없다. Reserved임
						continue;
					}

					//프로퍼티가 있는 경우에만
					bool isHasProp = mat_Test.HasProperty(srcProp._name);
					if(!isHasProp)
					{
						//없는 Property.
						continue;
					}

					//이름을 비교하여, 기존의 값이 있다면 덮어 씌우고, 없으면 새로 만들기
					switch (srcProp._propType)
					{
						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Float:
							AddProperty_Float(list_Float, srcProp._name, srcProp._value_Float);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Int:
							AddProperty_Int(list_Int, srcProp._name, srcProp._value_Int);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Vector:
							AddProperty_Vector(list_Vector, srcProp._name, srcProp._value_Vector);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Texture:
							AddProperty_Texture(list_Texture, srcProp._name, srcProp._value_Texture);
							break;

						case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Color:
							AddProperty_Color(list_Color, srcProp._name, srcProp._value_Color);
							break;
					}
				}
			}

			


			//리스트의 값을 변수로 저장한다.
			if(list_Float.Count > 0)
			{
				//정렬부터 
				list_Float.Sort(delegate(Property_Float a, Property_Float b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Float = new Property_Float[list_Float.Count];
				for (int i = 0; i < list_Float.Count; i++)
				{
					_props_Float[i] = list_Float[i];
				}
			}
			
			if(list_Int.Count > 0)
			{
				//정렬부터 
				list_Int.Sort(delegate(Property_Int a, Property_Int b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Int = new Property_Int[list_Int.Count];
				for (int i = 0; i < list_Int.Count; i++)
				{
					_props_Int[i] = list_Int[i];
				}
			}

			if(list_Vector.Count > 0)
			{
				//정렬부터 
				list_Vector.Sort(delegate(Property_Vector a, Property_Vector b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Vector = new Property_Vector[list_Vector.Count];
				for (int i = 0; i < list_Vector.Count; i++)
				{
					_props_Vector[i] = list_Vector[i];
				}
			}

			if(list_Texture.Count > 0)
			{
				//정렬부터 
				list_Float.Sort(delegate(Property_Float a, Property_Float b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Texture = new Property_Texture[list_Texture.Count];
				for (int i = 0; i < list_Texture.Count; i++)
				{
					_props_Texture[i] = list_Texture[i];
				}
			}

			if(list_Color.Count > 0)
			{
				//정렬부터 
				list_Float.Sort(delegate(Property_Float a, Property_Float b)
				{
					return string.Compare(a._name, b._name);
				});
				
				//값 복사
				_props_Color = new Property_Color[list_Color.Count];
				for (int i = 0; i < list_Color.Count; i++)
				{
					_props_Color[i] = list_Color[i];
				}
			}


			//추가 21.12.24
			//이게 병합 가능한 재질인지 확인하자
			//기본 속성은 있어야 한다.
			//클리핑 속성은 없어야 한다.
			//병합 속성 9개 모두 있어야 한다.
			_isMergable = false;
			if(mat_Test.HasProperty(RESERVED_PROP__COLOR)
				&& mat_Test.HasProperty(RESERVED_PROP__MAIN_TEX)
				&& !mat_Test.HasProperty(RESERVED_PROP__MASK_TEX)
				&& !mat_Test.HasProperty(RESERVED_PROP__MASK_SCREEN_SPACE_OFFSET)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_1)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_2)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_3)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_4)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_5)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_6)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_7)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_8)
				&& mat_Test.HasProperty(RESERVED_PROP__MERGED_TEX_9)
				)
			{
				_isMergable = true;
			}

			UnityEngine.Object.DestroyImmediate(mat_Test);
		}
		
#endif
		//Property 값들을 추가하자.
		//이름을 기준으로 겹체는게 있으면 값을 덮어씌움.
		public void AddProperty_Float(List<Property_Float> propList, string name, float value)
		{
			Property_Float existProp = propList.Find(delegate(Property_Float a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Float(name, value)); }
		}

		public void AddProperty_Int(List<Property_Int> propList, string name, int value)
		{
			Property_Int existProp = propList.Find(delegate(Property_Int a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Int(name, value)); }
		}

		public void AddProperty_Vector(List<Property_Vector> propList, string name, Vector4 value)
		{
			Property_Vector existProp = propList.Find(delegate(Property_Vector a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Vector(name, value)); }
		}

		public void AddProperty_Texture(List<Property_Texture> propList, string name, Texture value)
		{
			Property_Texture existProp = propList.Find(delegate(Property_Texture a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Texture(name, value)); }
		}

		public void AddProperty_Color(List<Property_Color> propList, string name, Color value)
		{
			Property_Color existProp = propList.Find(delegate(Property_Color a)
			{
				return string.Equals(a._name, name);
			});

			if(existProp != null)	{ existProp._value = value; }
			else					{ propList.Add(new Property_Color(name, value)); }
		}

		// Make from Src : Batched / Shared Material에서 만들때는 이 함수를 이용하자
		//---------------------------------------------------
		public void MakeFromSrc(apOptMaterialInfo srcMatInfo)
		{
			Clear();

			_isBaked = true;
			_mainTex = srcMatInfo._mainTex;
			_textureID = srcMatInfo._textureID;
			_shader = srcMatInfo._shader;
			
			
			if(srcMatInfo.NumProp_Float > 0)
			{
				_props_Float = new Property_Float[srcMatInfo.NumProp_Float];
				for (int i = 0; i < srcMatInfo.NumProp_Float; i++)
				{
					Property_Float srcProp = srcMatInfo._props_Float[i];
					_props_Float[i] = new Property_Float(srcProp._name, srcProp._value);
				}
			}

			if(srcMatInfo.NumProp_Int > 0)
			{
				_props_Int = new Property_Int[srcMatInfo.NumProp_Int];
				for (int i = 0; i < srcMatInfo.NumProp_Int; i++)
				{
					Property_Int srcProp = srcMatInfo._props_Int[i];
					_props_Int[i] = new Property_Int(srcProp._name, srcProp._value);
				}
			}

			if(srcMatInfo.NumProp_Vector > 0)
			{
				_props_Vector = new Property_Vector[srcMatInfo.NumProp_Vector];
				for (int i = 0; i < srcMatInfo.NumProp_Vector; i++)
				{
					Property_Vector srcProp = srcMatInfo._props_Vector[i];
					_props_Vector[i] = new Property_Vector(srcProp._name, srcProp._value);
				}
			}

			if(srcMatInfo.NumProp_Texture > 0)
			{
				_props_Texture = new Property_Texture[srcMatInfo.NumProp_Texture];
				for (int i = 0; i < srcMatInfo.NumProp_Texture; i++)
				{
					Property_Texture srcProp = srcMatInfo._props_Texture[i];
					_props_Texture[i] = new Property_Texture(srcProp._name, srcProp._value);
				}
			}

			if(srcMatInfo.NumProp_Color > 0)
			{
				_props_Color = new Property_Color[srcMatInfo.NumProp_Color];
				for (int i = 0; i < srcMatInfo.NumProp_Color; i++)
				{
					Property_Color srcProp = srcMatInfo._props_Color[i];
					_props_Color[i] = new Property_Color(srcProp._name, srcProp._value);
				}
			}
		}


		// Functions
		//---------------------------------------------------
		public static bool IsSameInfo(apOptMaterialInfo infoA, apOptMaterialInfo infoB)
		{
			if(	infoA._mainTex != infoB._mainTex ||
				infoA._textureID != infoB._textureID ||
				infoA._shader != infoB._shader ||
				
				infoA.NumProp_Float != infoB.NumProp_Float ||
				infoA.NumProp_Int != infoB.NumProp_Int ||
				infoA.NumProp_Vector != infoB.NumProp_Vector ||
				infoA.NumProp_Texture != infoB.NumProp_Texture ||
				infoA.NumProp_Color != infoB.NumProp_Color)
			{
				//기본 속성에서 차이가 있다.
				return false;
			}

			//이제 상세 설정이 모두 동일한지 확인해야한다.
			//하나라도 다르면 패스
			//이름이나 속성을 순서대로 비교해서 하나라도 다르면 다른 것이다.
			//정렬을 했기 때문에 순서대로 비교하면 된다.

			int numFloat = infoA.NumProp_Float;
			int numInt = infoA.NumProp_Int;
			int numVector = infoA.NumProp_Vector;
			int numTexture = infoA.NumProp_Texture;
			int numColor = infoA.NumProp_Color;

			//1. Float
			if(numFloat > 0)
			{
				Property_Float propA = null;
				Property_Float propB = null;
				for (int i = 0; i < numFloat; i++)
				{
					propA = infoA._props_Float[i];
					propB = infoB._props_Float[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(Mathf.Abs(propA._value - propB._value) > 0.0001f)
					{
						return false;
					}
				}
			}

			//2. Int
			if(numInt > 0)
			{
				Property_Int propA = null;
				Property_Int propB = null;
				for (int i = 0; i < numInt; i++)
				{
					propA = infoA._props_Int[i];
					propB = infoB._props_Int[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(propA._value != propB._value)
					{
						return false;
					}
				}
			}

			//3. Vector
			if(numVector > 0)
			{
				Property_Vector propA = null;
				Property_Vector propB = null;
				for (int i = 0; i < numVector; i++)
				{
					propA = infoA._props_Vector[i];
					propB = infoB._props_Vector[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(	Mathf.Abs(propA._value.x - propB._value.x) > 0.0001f ||
						Mathf.Abs(propA._value.y - propB._value.y) > 0.0001f ||
						Mathf.Abs(propA._value.z - propB._value.z) > 0.0001f ||
						Mathf.Abs(propA._value.w - propB._value.w) > 0.0001f)
					{
						return false;
					}
				}
			}

			//4. Texture
			if(numTexture > 0)
			{
				Property_Texture propA = null;
				Property_Texture propB = null;
				for (int i = 0; i < numTexture; i++)
				{
					propA = infoA._props_Texture[i];
					propB = infoB._props_Texture[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(propA._value != propB._value)
					{
						return false;
					}
				}
			}


			//5. Color
			if(numColor > 0)
			{
				Property_Color propA = null;
				Property_Color propB = null;
				for (int i = 0; i < numColor; i++)
				{
					propA = infoA._props_Color[i];
					propB = infoB._props_Color[i];
					if(!string.Equals(propA._name, propB._name))
					{
						return false;
					}

					if(	Mathf.Abs(propA._value.r - propB._value.r) > 0.001f ||
						Mathf.Abs(propA._value.g - propB._value.g) > 0.001f ||
						Mathf.Abs(propA._value.b - propB._value.b) > 0.001f ||
						Mathf.Abs(propA._value.a - propB._value.a) > 0.001f)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// 기본 재질 속성을 제외한 사용자 정의 속성들을 입력된 Material에 입력한다.
		/// (Float, Int, Vector, Texture, Color)
		/// </summary>
		/// <param name="targetMaterial"></param>
		public void SetMaterialProperties(Material targetMaterial)
		{
			if(targetMaterial == null)
			{
				return;
			}

			//1. Float
			if(NumProp_Float > 0)
			{
				for (int i = 0; i < NumProp_Float; i++)
				{
					targetMaterial.SetFloat(_props_Float[i]._name, _props_Float[i]._value);
				}
			}

			//2. Int
			if(NumProp_Int > 0)
			{
				for (int i = 0; i < NumProp_Int; i++)
				{
					targetMaterial.SetInt(_props_Int[i]._name, _props_Int[i]._value);
				}
			}

			//3. Vector
			if(NumProp_Vector > 0)
			{
				for (int i = 0; i < NumProp_Vector; i++)
				{
					targetMaterial.SetVector(_props_Vector[i]._name, _props_Vector[i]._value);
				}
			}

			//4. Texture
			if(NumProp_Texture > 0)
			{
				for (int i = 0; i < NumProp_Texture; i++)
				{
					targetMaterial.SetTexture(_props_Texture[i]._name, _props_Texture[i]._value);
				}
			}

			//5. Color
			if(NumProp_Color > 0)
			{
				for (int i = 0; i < NumProp_Color; i++)
				{
					targetMaterial.SetColor(_props_Color[i]._name, _props_Color[i]._value);
				}
			}
		}

		// Get / Set
		//---------------------------------------------------
		public int NumProp_Float
		{
			get
			{
				if(_props_Float == null) { return 0; }
				return _props_Float.Length;
			}
		}

		public int NumProp_Int
		{
			get
			{
				if(_props_Int == null) { return 0; }
				return _props_Int.Length;
			}
		}

		public int NumProp_Vector
		{
			get
			{
				if(_props_Vector == null) { return 0; }
				return _props_Vector.Length;
			}
		}

		public int NumProp_Texture
		{
			get
			{
				if(_props_Texture == null) { return 0; }
				return _props_Texture.Length;
			}
		}

		public int NumProp_Color
		{
			get
			{
				if(_props_Color == null) { return 0; }
				return _props_Color.Length;
			}
		}
	}
}