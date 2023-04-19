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
	//Merge 요청을 하면 생성되는 클래스
	//Merged인 경우엔 여러개의 Portrait들의 재질을 합칠 수 있어서 Shared와 유사하다.
	//성격상 Batched와 Shared의 중간
	//Main에서 생성되지만 Sub Portrait들도 연결되어 있다.
	//Mesh당 연결 정보와 채널 ID를 할당한다.
	//재질은 최소화한다.
	public class apOptMergedMaterial
	{
		public enum MERGED_CHANNEL
		{
			/// <summary>Not Merged</summary>
			None,
			/// <summary>_MainTex and _Color</summary>
			Main,
			/// <summary>_MergedTex1 and _MergedColor1</summary>
			Merged1,
			/// <summary>_MergedTex2 and _MergedColor2</summary>
			Merged2,
			/// <summary>_MergedTex3 and _MergedColor3</summary>
			Merged3,
			/// <summary>_MergedTex4 and _MergedColor4</summary>
			Merged4,
			/// <summary>_MergedTex5 and _MergedColor5</summary>
			Merged5,
			/// <summary>_MergedTex6 and _MergedColor6</summary>
			Merged6,
			/// <summary>_MergedTex7 and _MergedColor7</summary>
			Merged7,
			/// <summary>_MergedTex8 and _MergedColor8</summary>
			Merged8,
			/// <summary>_MergedTex9 and _MergedColor9</summary>
			Merged9,
		}

		// Sub Class
		//-------------------------------------------------------
		public class MeshInfo
		{
			public apPortrait _linkedPortrait;
			public apOptMesh _linkedOptMesh;
			public bool _isMerged = false;//조건이 안되면 Merged 되지 않는다.

			public ShaderChannelInfo _linkedShaderChannelInfo = null;
			public MERGED_CHANNEL _channel = MERGED_CHANNEL.None;

			//값 저장용 Material을 생성한다.
			private Material _material_Original = null;

			
			public MeshInfo(apPortrait linkedPortrait,
										apOptMesh linkedOptMesh,
										bool isMerged
										)
			{
				_linkedPortrait = linkedPortrait;
				_linkedOptMesh = linkedOptMesh;
				_isMerged = isMerged;
				_channel = MERGED_CHANNEL.None;
				_material_Original = null;

				Material curMaterial = _linkedOptMesh.GetMaterialBeforeMerge();
				if(curMaterial == null)
				{
					//Debug.LogError("병합 백업용 Material 가져오기 실패 [" + _linkedOptMesh.gameObject.name + "]");
					Debug.LogError("AnyPortrait : The material for merging has not yet been created. Initialize() of apPortrait must be called first.");
				}
				else
				{
					_material_Original = new Material(curMaterial.shader);
					_material_Original.CopyPropertiesFromMaterial(curMaterial);
				}
			}

			public void SetChannel(ShaderChannelInfo linkedShaderChannelInfo,
									MERGED_CHANNEL channel)
			{
				_linkedShaderChannelInfo = linkedShaderChannelInfo;
				_channel = channel;
			}



			// 재질 제어 함수들
			//------------------------------------------------------
			//리셋하기
			public void ResetMaterialToMerge()
			{
				//다시 Merged될 수 있게 초기화한다.
				_linkedOptMesh.SyncMergedMaterial_Reset(_material_Original);
			}

			/// <summary>_Color 동기화</summary>
			public void SetColor(ref Color color2X)
			{
				if(_isMerged)
				{	
					_linkedOptMesh.SyncMergedMaterial_Color(ref color2X);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetMeshColor(color2X);//직접 할당
				}
			}

			public void SetCustomImage(Texture2D texture, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomImage(texture, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomTexture(texture, propertyName);//직접 할당
				}
			}

			public void SetCustomImage(Texture2D texture, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomImage(texture, propertyNameID);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomTexture(texture, propertyNameID);//직접 할당
				}
			}

			public void SetCustomImageOffset(ref Vector2 offset, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomImageOffset(ref offset, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomTextureOffset(offset, propertyName);//직접 할당
				}
			}

			public void SetCustomImageScale(ref Vector2 scale, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomImageScale(ref scale, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomTextureScale(scale, propertyName);//직접 할당
				}
			}

			public void SetCustomFloat(float floatValue, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomFloat(floatValue, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomFloat(floatValue, propertyName);//직접 할당
				}
			}

			public void SetCustomFloat(float floatValue, int propertyNameID)//ID를 사용한 버전
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomFloat(floatValue, propertyNameID);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomFloat(floatValue, propertyNameID);//직접 할당
				}
			}

			public void SetCustomInt(int intValue, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomInt(intValue, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomInt(intValue, propertyName);//직접 할당
				}
			}

			public void SetCustomInt(int intValue, int propertyNameID)//ID를 사용한 버전
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomInt(intValue, propertyNameID);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomInt(intValue, propertyNameID);//직접 할당
				}
			}

			public void SetCustomVector4(ref Vector4 vec4Value, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomVector4(ref vec4Value, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomVector4(vec4Value, propertyName);//직접 할당
				}
			}

			public void SetCustomVector4(ref Vector4 vec4Value, int propertyNameID)//ID를 사용한 버전
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomVector4(ref vec4Value, propertyNameID);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomVector4(vec4Value, propertyNameID);//직접 할당
				}
			}

			public void SetCustomColor(ref Color color, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomColor(ref color, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomColor(color, propertyName);//직접 할당
				}
			}

			public void SetCustomColor(ref Color color, int propertyNameID)//ID를 사용한 버전
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomColor(ref color, propertyNameID);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomColor(color, propertyNameID);//직접 할당
				}
			}

			public void SetCustomAlpha(float alpha, ref string propertyName)
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomAlpha(alpha, ref propertyName);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomAlpha(alpha, propertyName);//직접 할당
				}
			}

			public void SetCustomAlpha(float alpha, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
			{
				if(_isMerged)
				{
					_linkedOptMesh.SyncMergedMaterial_CustomAlpha(alpha, propertyNameID);//동기화 함수
				}
				else
				{
					_linkedOptMesh.SetCustomAlpha(alpha, propertyNameID);//직접 할당
				}
			}
		}



		//Shader와 MainTex당 채널이 어떻게 저장되는지에 대한 정보
		public class ShaderChannelInfo
		{
			public Shader _shader = null;

			public List<Texture2D> _textures = null;
			public Dictionary<Texture2D, MERGED_CHANNEL> _tex2Channel = null;
			public Material _material = null;
			public Material _material_Original = null;//원래의 값.

			private apOptMergedMaterial _parentMergedMat = null;

			public ShaderChannelInfo(Shader shader, apOptMergedMaterial parentMergedMaterial)
			{
				_parentMergedMat = parentMergedMaterial;
				_shader = shader;

				_textures = new List<Texture2D>();
				_tex2Channel = new Dictionary<Texture2D, MERGED_CHANNEL>();

				if(_material == null || _material_Original == null)
				{
					_material = new Material(_shader);
					_material_Original = new Material(_shader);//값 복구용 재질
				}
			}

			public void OnDestroy()
			{
				if(_material != null)
				{
					UnityEngine.Object.Destroy(_material);
				}
				if(_material_Original != null)
				{
					UnityEngine.Object.Destroy(_material_Original);
				}
			}

			public bool IsTextureConatins(Texture2D targetTex)
			{
				if(targetTex == null)
				{
					return false;
				}
				return _tex2Channel.ContainsKey(targetTex);
			}

			/// <summary>
			/// 아직 추가할만한 슬롯이 남아있다.
			/// </summary>
			/// <returns></returns>
			public bool IsEmptySlotRemained()
			{
				int nAddedTextures = _textures.Count;
				//현재 0개 > 다음에 넣으면 Main 슬롯에 넣는다.
				//현재 1개 > 다음에 넣으면 Merged1 슬롯에 넣는다.
				//...
				//현재 9개 > 다음에 넣으면 Merged9 슬롯에 넣는다.
				//즉 현재 9개 이하면 더 넣을 수 있다.
				return (nAddedTextures <= 9);
			}

			/// <summary>
			/// 텍스쳐를 추가한다. 추가당시의 순서에 따라 채널을 지정한다.
			/// </summary>
			/// <param name="targetTex"></param>
			public void AddTexture(Texture2D targetTex)
			{
				int nAddedTextures = _textures.Count;
				MERGED_CHANNEL nextChannel = MERGED_CHANNEL.None;

				switch (nAddedTextures)
				{
					case 0: nextChannel = MERGED_CHANNEL.Main; break;
					case 1: nextChannel = MERGED_CHANNEL.Merged1; break;
					case 2: nextChannel = MERGED_CHANNEL.Merged2; break;
					case 3: nextChannel = MERGED_CHANNEL.Merged3; break;
					case 4: nextChannel = MERGED_CHANNEL.Merged4; break;
					case 5: nextChannel = MERGED_CHANNEL.Merged5; break;
					case 6: nextChannel = MERGED_CHANNEL.Merged6; break;
					case 7: nextChannel = MERGED_CHANNEL.Merged7; break;
					case 8: nextChannel = MERGED_CHANNEL.Merged8; break;
					case 9: nextChannel = MERGED_CHANNEL.Merged9; break;

				}

				//Debug.Log("Texture : " + targetTex.name + " > " + nextChannel);

				_tex2Channel.Add(targetTex, nextChannel);

				_textures.Add(targetTex);
			}

			public MERGED_CHANNEL GetChannel(Texture2D targetTex)
			{
				return _tex2Channel[targetTex];
			}

			/// <summary>
			/// 병합 정보를 바탕으로 재질에 값을 넣는다.
			/// </summary>
			public void AdaptMaterial()
			{
				_material.SetColor(_parentMergedMat._propID__COLOR, new Color(0.5f, 0.5f, 0.5f, 1.0f));

				foreach (KeyValuePair<Texture2D, MERGED_CHANNEL> tex2ChannelItem in _tex2Channel)
				{
					switch (tex2ChannelItem.Value)
					{
						case MERGED_CHANNEL.Main: _material.SetTexture(_parentMergedMat._propID__MAIN_TEX, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged1: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_1, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged2: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_2, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged3: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_3, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged4: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_4, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged5: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_5, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged6: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_6, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged7: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_7, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged8: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_8, tex2ChannelItem.Key); break;
						case MERGED_CHANNEL.Merged9: _material.SetTexture(_parentMergedMat._propID__MERGED_TEX_9, tex2ChannelItem.Key); break;
					}
				}

				//복구용 재질에 복사
				_material_Original.CopyPropertiesFromMaterial(_material);
			}



			// 재질 제어
			// 개별 메시를 제어하는게 아니라 아예 Merged 재질을 제어하여 일괄 편집할 수 있다.
			// 이미지 변경은 불가. 오직 색상이나 다른 것들만 가능하다.
			//--------------------------------------------------------------------
			public void ResetProperties()
			{
				//재질을 복구한다.
				_material.CopyPropertiesFromMaterial(_material_Original);
			}

			/// <summary>
			/// 색상을 지정한다. (_Color)
			/// 병합된 재질의 속성을 변경하되, 연결된 메시들
			/// </summary>
			/// <param name="color2X"></param>
			public void SetColor(ref Color color2X)
			{
				_material.SetColor(_parentMergedMat._propID__COLOR, color2X);
			}

			public void SetCustomImage(Texture2D texture, ref string propertyName)
			{
				_material.SetTexture(propertyName, texture);
			}

			public void SetCustomImage(Texture2D texture, int propertyNameID)//ID 버전 [v1.4.3]
			{
				_material.SetTexture(propertyNameID, texture);
			}

			public void SetCustomImageOffset(ref Vector2 offset, ref string propertyName)
			{
				_material.SetTextureOffset(propertyName, offset);
			}

			public void SetCustomImageScale(ref Vector2 scale, ref string propertyName)
			{
				_material.SetTextureScale(propertyName, scale);
			}

			public void SetCustomFloat(float floatValue, ref string propertyName)
			{
				_material.SetFloat(propertyName, floatValue);
			}

			public void SetCustomFloat(float floatValue, int propertyNameID)//ID 버전 [v1.4.3]
			{
				_material.SetFloat(propertyNameID, floatValue);
			}

			public void SetCustomInt(int intValue, ref string propertyName)
			{
				_material.SetInt(propertyName, intValue);
			}

			public void SetCustomInt(int intValue, int propertyNameID)//ID 버전 [v1.4.3]
			{
				_material.SetInt(propertyNameID, intValue);
			}

			public void SetCustomVector4(ref Vector4 vec4Value, ref string propertyName)
			{
				_material.SetVector(propertyName, vec4Value);
			}

			public void SetCustomVector4(ref Vector4 vec4Value, int propertyNameID)//ID 버전 [v1.4.3]
			{
				_material.SetVector(propertyNameID, vec4Value);
			}

			public void SetCustomColor(ref Color color, ref string propertyName)
			{
				_material.SetColor(propertyName, color);
			}

			public void SetCustomColor(ref Color color, int propertyNameID)//ID 버전 [v1.4.3]
			{
				_material.SetColor(propertyNameID, color);
			}

			public void SetCustomAlpha(float alpha, ref string propertyName)
			{
				if(!_material.HasProperty(propertyName))
				{
					return;
				}
				Color curColor = _material.GetColor(propertyName);
				curColor.a = alpha;
				_material.SetColor(propertyName, curColor);
			}

			public void SetCustomAlpha(float alpha, int propertyNameID)//ID 버전 [v1.4.3]
			{
				if(!_material.HasProperty(propertyNameID))
				{
					return;
				}
				Color curColor = _material.GetColor(propertyNameID);
				curColor.a = alpha;
				_material.SetColor(propertyNameID, curColor);
			}
			
		}

		//텍스쳐당 Channel 정보를 저장하자
		//같은 텍스쳐에 다른 Shader에 의해서 하나의 텍스쳐가 여러개의 채널 정보를 가질 수도 있다.
		//만약 Clipped Mask/Child라면 직접 설정하게 됨
		public class TextureChannelInfo
		{
			//키값이 되는 텍스쳐
			public Texture2D _keyTexture = null;

			//연결된 채널들
			//private int _nChannelInfos = 0;
			private List<ShaderChannelInfo> _channelInfos = null;

			//병합이 되지 않은 경우엔 직접 Mesh를 제어한다
			//private int _nNotMergedMeshes = 0;
			private List<apOptMesh> _notMergedMeshes = null;

			public TextureChannelInfo(Texture2D keyTexture)
			{
				_keyTexture = keyTexture;

				//_nChannelInfos = 0;
				_channelInfos = new List<ShaderChannelInfo>();

				//_nNotMergedMeshes = 0;
				_notMergedMeshes = new List<apOptMesh>();
			}

			public void AddChannelInfo(ShaderChannelInfo channelInfo)
			{
				if(!_channelInfos.Contains(channelInfo))
				{
					//Debug.LogError("이미 이 텍스쳐에 대한 채널이 존재한다.");
					return;
				}

				_channelInfos.Add(channelInfo);
				//_nChannelInfos = _channelInfos.Count;
			}

			public void AddNotMergedMesh(apOptMesh mesh)
			{
				_notMergedMeshes.Add(mesh);
				//_nNotMergedMeshes = _notMergedMeshes.Count;
			}
		}
		private List<TextureChannelInfo> _textureChannelInfoList = null;
		private Dictionary<Texture2D, TextureChannelInfo> _textureID2TexChannelInfo = null;




		private const string SHADER_PROP__COLOR = "_Color";
		private const string SHADER_PROP__MAIN_TEX = "_MainTex";

		private const string SHADER_PROP__MERGED_TEX_1 = "_MergedTex1";
		private const string SHADER_PROP__MERGED_TEX_2 = "_MergedTex2";
		private const string SHADER_PROP__MERGED_TEX_3 = "_MergedTex3";
		private const string SHADER_PROP__MERGED_TEX_4 = "_MergedTex4";
		private const string SHADER_PROP__MERGED_TEX_5 = "_MergedTex5";
		private const string SHADER_PROP__MERGED_TEX_6 = "_MergedTex6";
		private const string SHADER_PROP__MERGED_TEX_7 = "_MergedTex7";
		private const string SHADER_PROP__MERGED_TEX_8 = "_MergedTex8";
		private const string SHADER_PROP__MERGED_TEX_9 = "_MergedTex9";

		private Dictionary<MERGED_CHANNEL, Color> _channel2Color = null;

		

		public int _propID__COLOR = -1;
		public int _propID__MAIN_TEX = -1;

		public int _propID__MERGED_TEX_1 = -1;
		public int _propID__MERGED_TEX_2 = -1;
		public int _propID__MERGED_TEX_3 = -1;
		public int _propID__MERGED_TEX_4 = -1;
		public int _propID__MERGED_TEX_5 = -1;
		public int _propID__MERGED_TEX_6 = -1;
		public int _propID__MERGED_TEX_7 = -1;
		public int _propID__MERGED_TEX_8 = -1;
		public int _propID__MERGED_TEX_9 = -1;

		//공통된 속성 (색상)
		private Color _commonProp_Color = Color.gray;


		// Members
		//-------------------------------------------------------
		private List<MeshInfo> _meshInfoList = null;
		private Dictionary<apOptMesh, MeshInfo> _mesh2Info = null;
		private int _nMeshInfoList = 0;
		
		//통합된 재질
		private List<Material> _mergedMaterials = null;

		//Shader별로 개수를 모은다.
		//Shader 하나당 채널이 10개를 넘어가면 Info가 부족해진다.
		//따라서 Shader당 n개의 병합 정보가 있을 수 있다.
		private List<ShaderChannelInfo> _shaderChannelInfoList = null;
		private Dictionary<Shader, List<ShaderChannelInfo>> _shader2ChannelInfos = null;
		private int _nShaderChannelInfos = 0;

		//각 채널별로 버텍스 최대 크기를 저장한다.
		//색상 지정을 최소한으로 하기 위함
		private int _vertMaxCount__None = 0;//<<None은 복구할 때 White 색상으로 돌려놓기 위함. 나머지 모든 채널의 최대값이다.
		private int _vertMaxCount__Main = 0;
		private int _vertMaxCount__Merged1 = 0;
		private int _vertMaxCount__Merged2 = 0;
		private int _vertMaxCount__Merged3 = 0;
		private int _vertMaxCount__Merged4 = 0;
		private int _vertMaxCount__Merged5 = 0;
		private int _vertMaxCount__Merged6 = 0;
		private int _vertMaxCount__Merged7 = 0;
		private int _vertMaxCount__Merged8 = 0;
		private int _vertMaxCount__Merged9 = 0;

		private Dictionary<MERGED_CHANNEL, Color[]> _channel2VertColors = null;
		

		// Init
		//-------------------------------------------------------
		public apOptMergedMaterial()
		{
			_meshInfoList = new List<MeshInfo>();
			_mesh2Info = new Dictionary<apOptMesh, MeshInfo>();
			_mergedMaterials = new List<Material>();

			_shaderChannelInfoList = new List<ShaderChannelInfo>();
			_shader2ChannelInfos = new Dictionary<Shader, List<ShaderChannelInfo>>();

			_textureChannelInfoList = new List<TextureChannelInfo>();
			_textureID2TexChannelInfo = new Dictionary<Texture2D, TextureChannelInfo>();

			//채널당 버텍스 색상을 지정하자
			_channel2Color = new Dictionary<MERGED_CHANNEL, Color>();
			_channel2Color.Add(MERGED_CHANNEL.Main, new Color(1.0f, 1.0f, 1.0f, 1.0f));//R = White
			_channel2Color.Add(MERGED_CHANNEL.Merged1, new Color(0.9f, 0.5f, 0.5f, 1.0f));//R
			_channel2Color.Add(MERGED_CHANNEL.Merged2, new Color(0.8f, 0.5f, 0.5f, 1.0f));//R
			_channel2Color.Add(MERGED_CHANNEL.Merged3, new Color(0.7f, 0.5f, 0.5f, 1.0f));//R
			_channel2Color.Add(MERGED_CHANNEL.Merged4, new Color(0.5f, 0.9f, 0.5f, 1.0f));//G
			_channel2Color.Add(MERGED_CHANNEL.Merged5, new Color(0.5f, 0.8f, 0.5f, 1.0f));//G
			_channel2Color.Add(MERGED_CHANNEL.Merged6, new Color(0.5f, 0.7f, 0.5f, 1.0f));//G
			_channel2Color.Add(MERGED_CHANNEL.Merged7, new Color(0.5f, 0.5f, 0.9f, 1.0f));//B
			_channel2Color.Add(MERGED_CHANNEL.Merged8, new Color(0.5f, 0.5f, 0.8f, 1.0f));//B
			_channel2Color.Add(MERGED_CHANNEL.Merged9, new Color(0.5f, 0.5f, 0.7f, 1.0f));//B
			_channel2Color.Add(MERGED_CHANNEL.None, new Color(1.0f, 1.0f, 1.0f, 1.0f));//None 타입은 흰색 (기본)

			_propID__COLOR = Shader.PropertyToID(SHADER_PROP__COLOR);
			_propID__MAIN_TEX = Shader.PropertyToID(SHADER_PROP__MAIN_TEX);

			_propID__MERGED_TEX_1 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_1);
			_propID__MERGED_TEX_2 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_2);
			_propID__MERGED_TEX_3 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_3);
			_propID__MERGED_TEX_4 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_4);
			_propID__MERGED_TEX_5 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_5);
			_propID__MERGED_TEX_6 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_6);
			_propID__MERGED_TEX_7 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_7);
			_propID__MERGED_TEX_8 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_8);
			_propID__MERGED_TEX_9 = Shader.PropertyToID(SHADER_PROP__MERGED_TEX_9);
		}


		// Destroy
		//-------------------------------------------------------
		/// <summary>
		/// 중요! 삭제시에는 꼭 이 함수를 호출해야한다. (생성된 재질들 삭제)
		/// </summary>
		public void OnDestroy()
		{
			int nInfo = _shaderChannelInfoList != null ? _shaderChannelInfoList.Count : 0;
			if(nInfo > 0)
			{
				for (int i = 0; i < nInfo; i++)
				{
					//재질들을 삭제한다.
					_shaderChannelInfoList[i].OnDestroy();
				}

				_shaderChannelInfoList.Clear();
			}

			_shaderChannelInfoList = null;
		}


		// Functions
		//-------------------------------------------------------
		public bool MakeMergeMaterials(List<apPortrait> portraits)
		{
			if (_meshInfoList == null)
			{
				_meshInfoList = new List<MeshInfo>();
			}
			if (_mesh2Info == null)
			{
				_mesh2Info = new Dictionary<apOptMesh, MeshInfo>();
			}
			if (_mergedMaterials == null)
			{
				_mergedMaterials = new List<Material>();
			}
			if (_shaderChannelInfoList == null)
			{
				_shaderChannelInfoList = new List<ShaderChannelInfo>();
			}
			if(_shader2ChannelInfos == null)
			{
				_shader2ChannelInfos = new Dictionary<Shader, List<ShaderChannelInfo>>();
			}
			if(_textureChannelInfoList == null)
			{
				_textureChannelInfoList = new List<TextureChannelInfo>();
			}
			if(_textureID2TexChannelInfo == null)
			{
				_textureID2TexChannelInfo = new Dictionary<Texture2D, TextureChannelInfo>();
			}
			

			_meshInfoList.Clear();
			_mesh2Info.Clear();
			_mergedMaterials.Clear();
			_shaderChannelInfoList.Clear();
			_shader2ChannelInfos.Clear();
			_textureChannelInfoList.Clear();
			_textureID2TexChannelInfo.Clear();

			_nMeshInfoList = 0;
			_nShaderChannelInfos = 0;


			//최소 크기는 50
			int initVertCount = 50;
			_vertMaxCount__Main = initVertCount;
			_vertMaxCount__Merged1 = initVertCount;
			_vertMaxCount__Merged2 = initVertCount;
			_vertMaxCount__Merged3 = initVertCount;
			_vertMaxCount__Merged4 = initVertCount;
			_vertMaxCount__Merged5 = initVertCount;
			_vertMaxCount__Merged6 = initVertCount;
			_vertMaxCount__Merged7 = initVertCount;
			_vertMaxCount__Merged8 = initVertCount;
			_vertMaxCount__Merged9 = initVertCount;
			_vertMaxCount__None = initVertCount;//할당 안되는 경우도 만들자 병합 해제시 사용하기 위해서

			if(_channel2VertColors == null)
			{
				_channel2VertColors = new Dictionary<MERGED_CHANNEL, Color[]>();
			}
			_channel2VertColors.Clear();



			int nPortraits = portraits != null ? portraits.Count : 0;
			if(nPortraits == 0)
			{
				return false;
			}

			apPortrait curPortrait = null;
			apOptMesh curMesh = null;

			Shader curShader = null;
			Texture2D curTexture = null;

			List<ShaderChannelInfo> curChannelInfoList = null;
			for (int iPortrait = 0; iPortrait < nPortraits; iPortrait++)
			{
				curPortrait = portraits[iPortrait];
				if(curPortrait == null)
				{
					continue;
				}

				int nMeshes = curPortrait._optMeshes != null ? curPortrait._optMeshes.Count : 0;
				if(nMeshes == 0)
				{
					continue;
				}


				for (int iMesh = 0; iMesh < nMeshes; iMesh++)
				{
					curMesh = curPortrait._optMeshes[iMesh];
					
					bool isMergableMesh = CheckMergable(curMesh);

					if(!isMergableMesh)
					{
						//Merge 불가
						MeshInfo newMergeInfo = new MeshInfo(curPortrait, curMesh, false);
						_meshInfoList.Add(newMergeInfo);
						_mesh2Info.Add(curMesh, newMergeInfo);

						if(curMesh._texture != null)
						{
							//Merged가 안된 메시는 별도로 저장한다.
							TextureChannelInfo targetTexChannelInfo = null;
							if(!_textureID2TexChannelInfo.ContainsKey(curMesh._texture))
							{
								targetTexChannelInfo = new TextureChannelInfo(curMesh._texture);

								_textureID2TexChannelInfo.Add(curMesh._texture, targetTexChannelInfo);
								_textureChannelInfoList.Add(targetTexChannelInfo);
							}
							else
							{
								targetTexChannelInfo = _textureID2TexChannelInfo[curMesh._texture];
							}
							targetTexChannelInfo.AddNotMergedMesh(curMesh);//메시 추가
						}

						continue;
					}

					int curVertCount = curMesh._mesh.vertices.Length;

					curShader = curMesh.MaterialInfo._shader;
					curTexture = curMesh.MaterialInfo._mainTex;

					//이 Shader가 들어갈만한 재질과 Channel을 찾자
					//일단 이미 추가된 건 아닌지 찾자
					ShaderChannelInfo existChannelInfo = null;
					if(_shader2ChannelInfos.ContainsKey(curShader))
					{
						curChannelInfoList = _shader2ChannelInfos[curShader];
						int nChannelInfoList = curChannelInfoList.Count;
						for (int iChannelInfo = 0; iChannelInfo < nChannelInfoList; iChannelInfo++)
						{
							ShaderChannelInfo curChannelInfo = curChannelInfoList[iChannelInfo];
							if(curChannelInfo.IsTextureConatins(curTexture))
							{
								//해당 Shader+Texture는 이미 채널로서 추가되었다.
								existChannelInfo = curChannelInfo;
								break;
							}
						}
					}

					MERGED_CHANNEL channel = MERGED_CHANNEL.None;

					if(existChannelInfo != null)
					{
						//이미 병합 정보가 추가되었다면
						MeshInfo newMergeInfo = new MeshInfo(curPortrait, curMesh, true);
						channel = existChannelInfo.GetChannel(curTexture);
						newMergeInfo.SetChannel(	existChannelInfo, 
													channel);

						_meshInfoList.Add(newMergeInfo);
						_mesh2Info.Add(curMesh, newMergeInfo);
					}
					else
					{
						//병합 정보가 없다면 새로 만들자
						ShaderChannelInfo targetChannelInfo = null;
						if(!_shader2ChannelInfos.ContainsKey(curShader))
						{
							//Shader가 등록되지 않았다면
							curChannelInfoList = new List<ShaderChannelInfo>();
							_shader2ChannelInfos.Add(curShader, curChannelInfoList);
						}
						else
						{
							curChannelInfoList = _shader2ChannelInfos[curShader];
						}

						//이제 추가할만한 Channel이 있는지 찾자
						int nCurChannelList = curChannelInfoList.Count;
						if(nCurChannelList > 0)
						{
							ShaderChannelInfo curEmptyChannelInfo = null;
							for (int iChannel = 0; iChannel < nCurChannelList; iChannel++)
							{
								curEmptyChannelInfo = curChannelInfoList[iChannel];
								if(curEmptyChannelInfo.IsEmptySlotRemained())
								{
									//비어있는걸 찾았다.
									targetChannelInfo = curEmptyChannelInfo;
									break;
								}
							}
						}

						//추가할만한 것을 못찾았다면 새로 만들어야 한다.
						if(targetChannelInfo == null)
						{
							targetChannelInfo = new ShaderChannelInfo(curShader, this);							
							curChannelInfoList.Add(targetChannelInfo);
							_shaderChannelInfoList.Add(targetChannelInfo);
						}

						//채널 정보에 텍스쳐를 추가하자
						targetChannelInfo.AddTexture(curTexture);

						//새로운 정보를 바탕으로 병합 정보를 만들자
						MeshInfo newMergeInfo = new MeshInfo(curPortrait, curMesh, true);
						channel = targetChannelInfo.GetChannel(curTexture);
						newMergeInfo.SetChannel(	targetChannelInfo, 
													channel);

						_meshInfoList.Add(newMergeInfo);
						_mesh2Info.Add(curMesh, newMergeInfo);

						//새로 생성된 Texture+ShaderChannelInfo 조합을 Texture의 입장에서 저장하자
						TextureChannelInfo targetTexChannelInfo = null;
						if(!_textureID2TexChannelInfo.ContainsKey(curMesh._texture))
						{
							targetTexChannelInfo = new TextureChannelInfo(curMesh._texture);

							_textureID2TexChannelInfo.Add(curMesh._texture, targetTexChannelInfo);
							_textureChannelInfoList.Add(targetTexChannelInfo);
						}
						else
						{
							targetTexChannelInfo = _textureID2TexChannelInfo[curMesh._texture];
						}
						targetTexChannelInfo.AddChannelInfo(targetChannelInfo);//채널 저장
					}



					//채널당 버텍스 개수를 갱신한다.
					switch (channel)
					{
						case MERGED_CHANNEL.Main: _vertMaxCount__Main = Mathf.Max(curVertCount, _vertMaxCount__Main); break;
						case MERGED_CHANNEL.Merged1: _vertMaxCount__Merged1 = Mathf.Max(curVertCount, _vertMaxCount__Merged1); break;
						case MERGED_CHANNEL.Merged2: _vertMaxCount__Merged2 = Mathf.Max(curVertCount, _vertMaxCount__Merged2); break;
						case MERGED_CHANNEL.Merged3: _vertMaxCount__Merged3 = Mathf.Max(curVertCount, _vertMaxCount__Merged3); break;
						case MERGED_CHANNEL.Merged4: _vertMaxCount__Merged4 = Mathf.Max(curVertCount, _vertMaxCount__Merged4); break;
						case MERGED_CHANNEL.Merged5: _vertMaxCount__Merged5 = Mathf.Max(curVertCount, _vertMaxCount__Merged5); break;
						case MERGED_CHANNEL.Merged6: _vertMaxCount__Merged6 = Mathf.Max(curVertCount, _vertMaxCount__Merged6); break;
						case MERGED_CHANNEL.Merged7: _vertMaxCount__Merged7 = Mathf.Max(curVertCount, _vertMaxCount__Merged7); break;
						case MERGED_CHANNEL.Merged8: _vertMaxCount__Merged8 = Mathf.Max(curVertCount, _vertMaxCount__Merged8); break;
						case MERGED_CHANNEL.Merged9: _vertMaxCount__Merged9 = Mathf.Max(curVertCount, _vertMaxCount__Merged9); break;
					}
					
					//채널에 상관없이 None 타입은 최대값을 가진다.
					_vertMaxCount__None = Mathf.Max(curVertCount, _vertMaxCount__None);
				}
			}

			//병합된 채널 정보를 바탕으로 재질을 모두 만들자
			_nShaderChannelInfos = _shaderChannelInfoList != null ? _shaderChannelInfoList.Count : 0;
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					//재질들을 완성한다.
					_shaderChannelInfoList[i].AdaptMaterial();
				}
			}

			//버텍스 색상을 적용하기 위해 배열을 만들자 (이 과정 오래 걸릴듯)
			MakeVertexColorArray(MERGED_CHANNEL.Main, _vertMaxCount__Main);
			MakeVertexColorArray(MERGED_CHANNEL.Merged1, _vertMaxCount__Merged1);
			MakeVertexColorArray(MERGED_CHANNEL.Merged2, _vertMaxCount__Merged2);
			MakeVertexColorArray(MERGED_CHANNEL.Merged3, _vertMaxCount__Merged3);
			MakeVertexColorArray(MERGED_CHANNEL.Merged4, _vertMaxCount__Merged4);
			MakeVertexColorArray(MERGED_CHANNEL.Merged5, _vertMaxCount__Merged5);
			MakeVertexColorArray(MERGED_CHANNEL.Merged6, _vertMaxCount__Merged6);
			MakeVertexColorArray(MERGED_CHANNEL.Merged7, _vertMaxCount__Merged7);
			MakeVertexColorArray(MERGED_CHANNEL.Merged8, _vertMaxCount__Merged8);
			MakeVertexColorArray(MERGED_CHANNEL.Merged9, _vertMaxCount__Merged9);
			MakeVertexColorArray(MERGED_CHANNEL.None, _vertMaxCount__None);


			//각 채널당 메시를 돌면서 버텍스 색상을 지정한다.
			_nMeshInfoList = _meshInfoList.Count;
			if(_nMeshInfoList > 0)
			{
				MeshInfo curMergeInfo = null;
				MERGED_CHANNEL channel;
				Material curMergedMaterial = null;
				for (int iInfo = 0; iInfo < _nMeshInfoList; iInfo++)
				{
					curMergeInfo = _meshInfoList[iInfo];
					if(!curMergeInfo._isMerged)
					{
						continue;
					}

					channel = curMergeInfo._channel;
					curMergedMaterial = curMergeInfo._linkedShaderChannelInfo._material;
					curMergeInfo._linkedOptMesh.SetMergedMaterial(	_channel2VertColors[channel], 
																	_channel2VertColors[MERGED_CHANNEL.None], 
																	curMergedMaterial);
				}
			}

			//공통 속성을 변수로 저장
			_commonProp_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			//Debug.Log("병합된 Channel Info : " + _shaderChannelInfoList.Count);

			return true;
		}


		//병합 가능한지 확인한다.
		private bool CheckMergable(apOptMesh mesh)
		{
			//Batch가 되지 않는 재질은 병합도 안된다.
			if(!mesh._isBatchedMaterial)
			{
				return false;
			}

			//재질 정보가 없는 이전 버전의 메시는 병합이 안된다.
			if(!mesh.IsUseMaterialInfo)
			{
				return false;
			}

			//재질 정보가 없다면 병합 불가
			if(mesh.MaterialInfo == null)
			{
				return false;
			}

			//Shader나 MainTex가 없으면 안된다.
			if(mesh.MaterialInfo._shader == null
				|| mesh.MaterialInfo._mainTex == null)
			{
				return false;
			}

			//알파블랜드가 아니면 안됨
			if(mesh._shaderType != apPortrait.SHADER_TYPE.AlphaBlend)
			{
				return false;
			}

			//마스크 자식/부모면 안된다.
			if(mesh._isMaskChild
				|| mesh._isMaskParent //부모는 괜찮지 않을까
				)
			{
				return false;
			}
			//재질 정보에서 병합 불가라고 했다면
			if(!mesh.MaterialInfo._isMergable)
			{
				return false;
			}

			//메시가 없거나 버텍스가 없다면
			if(mesh._mesh == null
				|| mesh._mesh.vertices.Length == 0)
			{
				return false;
			}
			//모든 필터를 통과했다.
			return true;
		}

		private void MakeVertexColorArray(MERGED_CHANNEL channel, int vertCount)
		{
			//약간 더 올린다
			vertCount += 20;
			Color[] vertColors = new Color[vertCount];
			Color channelColor = _channel2Color[channel];
			
			for (int i = 0; i < vertCount; i++)
			{
				vertColors[i] = channelColor;
			}

			_channel2VertColors.Add(channel, vertColors);
		}


		// 색상 변경 함수들
		//-------------------------------------------------------------
		//Merged인 경우는 하나의 세트로 바꿀 수 있다.
		public void ResetAllProperties()
		{
			//먼저 쉐이더당 채널 재질을 초기화 한다.
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].ResetProperties();
				}
			}

			//메시들도 초기화
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].ResetMaterialToMerge();
				}
			}

			//재질 변수도 초기화
			_commonProp_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		}

		public void SetColor(Color color2X)
		{
			//공통 변수
			_commonProp_Color = color2X;

			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetColor(ref _commonProp_Color);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetColor(ref _commonProp_Color);
				}
			}
		}

		public void SetAlpha(float alpha)
		{
			//공통 속성
			_commonProp_Color.a = alpha;

			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetColor(ref _commonProp_Color);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetColor(ref _commonProp_Color);
				}
			}
		}

		//커스텀 재질 속성
		public void SetCustomImage(Texture2D texture, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomImage(texture, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomImage(texture, ref propertyName);
				}
			}
		}

		public void SetCustomImage(Texture2D texture, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomImage(texture, propertyNameID);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomImage(texture, propertyNameID);
				}
			}
		}

		public void SetCustomImageOffset(Vector2 uvOffset, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomImageOffset(ref uvOffset, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomImageOffset(ref uvOffset, ref propertyName);
				}
			}
		}

		public void SetCustomImageScale(Vector2 uvScale, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomImageScale(ref uvScale, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomImageScale(ref uvScale, ref propertyName);
				}
			}
		}

		public void SetCustomFloat(float floatValue, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomFloat(floatValue, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomFloat(floatValue, ref propertyName);
				}
			}
		}

		public void SetCustomFloat(float floatValue, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomFloat(floatValue, propertyNameID);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomFloat(floatValue, propertyNameID);
				}
			}
		}

		public void SetCustomInt(int intValue, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomInt(intValue, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomInt(intValue, ref propertyName);
				}
			}
		}

		public void SetCustomInt(int intValue, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomInt(intValue, propertyNameID);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomInt(intValue, propertyNameID);
				}
			}
		}

		public void SetCustomVector4(Vector4 vec4Value, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomVector4(ref vec4Value, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomVector4(ref vec4Value, ref propertyName);
				}
			}
		}

		public void SetCustomVector4(Vector4 vec4Value, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomVector4(ref vec4Value, propertyNameID);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomVector4(ref vec4Value, propertyNameID);
				}
			}
		}

		public void SetCustomColor(Color color, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomColor(ref color, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomColor(ref color, ref propertyName);
				}
			}
		}

		public void SetCustomColor(Color color, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomColor(ref color, propertyNameID);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomColor(ref color, propertyNameID);
				}
			}
		}

		public void SetCustomAlpha(float alpha, ref string propertyName)
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomAlpha(alpha, ref propertyName);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomAlpha(alpha, ref propertyName);
				}
			}
		}

		public void SetCustomAlpha(float alpha, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			//먼저 쉐이더당 색상 지정
			if(_nShaderChannelInfos > 0)
			{
				for (int i = 0; i < _nShaderChannelInfos; i++)
				{
					_shaderChannelInfoList[i].SetCustomAlpha(alpha, propertyNameID);
				}
			}

			//메시들의 색상을 지정. 이건 동기화에 가깝다
			if(_nMeshInfoList > 0)
			{
				for (int i = 0; i < _nMeshInfoList; i++)
				{
					_meshInfoList[i].SetCustomAlpha(alpha, propertyNameID);
				}
			}
		}
	}
}