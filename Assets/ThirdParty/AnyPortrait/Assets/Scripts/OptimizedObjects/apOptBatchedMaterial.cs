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
	//Material Batch를 위해서 Material정보를 ID와 함께 저장해놓는다.
	//Bake 후에 다시 Link를 하면 이 정보를 참조하여 Material를 가져간다.
	//CustomShader를 포함하는 반면 Clipped Mesh는 예외로 둔다. (알아서 Shader로 만들 것)
	/// <summary>
	/// Material manager class for batch rendering
	/// This is done automatically, so it is recommended that you do not control it with scripts.
	/// </summary>
	[Serializable]
	public class apOptBatchedMaterial
	{
		// Members
		//----------------------------------------------------
		[Serializable]
		public class MaterialUnit
		{
			[SerializeField]
			public int _uniqueID = -1;

			//수정 : 직렬화 안되도록
			[NonSerialized]
			public Material _material = null;

			//추가 : 원래의 재질 속성을 저장하기 위한 원본 재질. 실제로 적용되진 않고, Reset 용도로 사용한다.
			[NonSerialized]
			public Material _material_Original = null;

			//일종의 키값이 되는 데이터
			[SerializeField]
			private Texture2D _texture = null;
			

			[SerializeField]
			public int _textureID = -1;

			[SerializeField]
			private Shader _shader = null;



			//추가 19.6.15 : Key값을 MaterialInfo로 설정하기
			//MaterialInfo를 이용하는 경우, 위의 _texture, _textureID, _shader는 사용되지 않는다.
			//Null값을 체크해야하므로 배열 형식을 이용한다.
			[SerializeField]
			private apOptMaterialInfo[] _materialInfo = null;

			public apOptMaterialInfo MaterialInfo
			{
				get
				{
					if(_materialInfo == null || _materialInfo.Length == 0) { return null; }
					return _materialInfo[0];
				}
			}

			public bool IsUseMaterialInfo
			{
				get { return MaterialInfo != null; }
			}

			


			[NonSerialized, NonBackupField]
			public List<apOptMesh> _linkedMeshes = new List<apOptMesh>();

			//추가 12.13 : 일괄 변형 요청 : 텍스쳐, 색상 등등
			//Custom이 속성 변경 요청이 아니라면, 기본값과 비교해서 자동 리셋할 수도 있다.
			[NonSerialized]
			public bool _isRequested_Texture = false;

			[NonSerialized]
			public bool _isRequested_Color = false;

			[NonSerialized]
			public bool _isRequested_Custom = false;
			
			[NonSerialized]
			private int _shaderID_MainTex = -1;
			[NonSerialized]
			private int _shaderID_Color = -1;
			

			public MaterialUnit()
			{
				_linkedMeshes.Clear();
			}

			public MaterialUnit(int uniqueID, Texture2D texture, int textureID, Shader shader)
			{
				_uniqueID = uniqueID;

				_texture = texture;
				_textureID = textureID;
				_shader = shader;

				//이전 방식에서 MaterialInfo는 Null
				_materialInfo = null;
			}


			//추가 19.6.15 : MaterialInfo를 이용한 생성
			public MaterialUnit(int uniqueID, apOptMaterialInfo srcMaterialInfo)
			{
				_uniqueID = uniqueID;

				//Material Info를 활용하는 경우 이 변수들은 사용하지 않는다.
				//_texture = texture;
				//_textureID = textureID;
				//_shader = shader;

				_materialInfo = new apOptMaterialInfo[1];
				_materialInfo[0] = new apOptMaterialInfo();
				_materialInfo[0].MakeFromSrc(srcMaterialInfo);

				_textureID = _materialInfo[0]._textureID;

			}


			/// <summary>
			/// 이 재질이 요구 사항에 맞는가?
			/// Material Info를 사용하지 않는 경우 (이전 버전)
			/// </summary>
			public bool IsEqualMaterial_Prev(Texture2D texture, int textureID, Shader shader)
			{
				if(IsUseMaterialInfo)
				{
					//Material Info를 사용한다면 이 재질은 요구사항에 맞지 않다.
					return false;
				}
				return _texture == texture
					&& _textureID == textureID
					&& _shader == shader;
			}


			/// <summary>
			/// 이 재질이 요구 사항에 맞는가?
			/// Material Info를 사용하지 않는 경우 (이전 버전)
			/// </summary>
			public bool IsEqualMaterial_MatInfo(apOptMaterialInfo matInfo)
			{
				if(!IsUseMaterialInfo)
				{
					//Material Info를 사용하지 않는다면 이 재질은 요구사항에 맞지 않다.
					return false;
				}
				return apOptMaterialInfo.IsSameInfo(MaterialInfo, matInfo);
			}


			

			public void MakeMaterial()
			{
				if (!IsUseMaterialInfo)
				{
					//이전 버전으로 만드는 경우
					_material = new Material(_shader);

					_shaderID_MainTex = Shader.PropertyToID("_MainTex");
					_shaderID_Color = Shader.PropertyToID("_Color");

					_material.SetTexture(_shaderID_MainTex, _texture);
					_material.SetColor(_shaderID_Color, new Color(0.5f, 0.5f, 0.5f, 1.0f));
				}
				else
				{
					//변경 19.6.15 : MaterialInfo를 이용하여 만드는 경우
					apOptMaterialInfo matInfo = MaterialInfo;

					_material = new Material(matInfo._shader);

					_shaderID_MainTex = Shader.PropertyToID("_MainTex");
					_shaderID_Color = Shader.PropertyToID("_Color");


					_material.SetTexture(_shaderID_MainTex, matInfo._mainTex);
					_material.SetColor(_shaderID_Color, new Color(0.5f, 0.5f, 0.5f, 1.0f));

					//속성대로 초기화
					matInfo.SetMaterialProperties(_material);
				}

				//복원용 재질
				_material_Original = new Material(_material);
				

				ResetRequestProperties();
			}

			public void LinkMesh(apOptMesh optMesh)
			{
				if(!_linkedMeshes.Contains(optMesh))
				{
					_linkedMeshes.Add(optMesh);
				}
			}

			//초기화시에는 연결된 메시 리스트를 날린다.
			public void ClearLinkedMeshes()
			{
				_linkedMeshes.Clear();
			}




			

			/// <summary>
			/// 변경 요청을 리셋한다.
			/// </summary>
			public void ResetRequestProperties()
			{
				_isRequested_Texture = false;
				_isRequested_Color = false;
				_isRequested_Custom = false;

				//원래 프로퍼티에서 복사를 하자.
				_material.CopyPropertiesFromMaterial(_material_Original);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_Reset(_material_Original);
				}
			}

			/// <summary>
			/// 기본 텍스쳐(_MainTex) 변경 요청. 기본값인 경우 요청을 초기화한다.
			/// </summary>
			/// <param name="texture"></param>
			public void RequestImage(Texture2D texture)
			{
				//원래의 Texture였는지 확인한다.
				//Material Info를 사용하는지에 따라서 다른 조건문에서 처리
				Texture2D defaultTexture = null;
				if (IsUseMaterialInfo)
				{
					defaultTexture = MaterialInfo._mainTex;
				}
				else
				{
					defaultTexture = _texture;
				}

				if (texture == defaultTexture)
				{
					//원래대로 돌아왔다.
					_isRequested_Texture = false;
				}
				else
				{
					_isRequested_Texture = true;
				}
				_material.SetTexture(_shaderID_MainTex, texture);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_Texture(texture);
				}

			}

			/// <summary>
			/// 기본 색상(_Color) 변경 요청. 기본값인 경우 요청을 초기화한다.
			/// </summary>
			/// <param name="color"></param>
			public void RequestColor(Color color)
			{
				bool isGray = Mathf.Abs(color.r - 0.5f) < 0.004f && 
								Mathf.Abs(color.g - 0.5f) < 0.004f && 
								Mathf.Abs(color.b - 0.5f) < 0.004f && 
								Mathf.Abs(color.a - 1.0f) < 0.004f;

				if(isGray)
				{
					_isRequested_Color = false;
					color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else
				{
					_isRequested_Color = true;
				}

				_material.SetColor(_shaderID_Color, color);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_Color(color);
				}
			}

			/// <summary>
			/// 기본 색상(_Color)의 알파 채널 변경 요청. 기본값인 경우 요청을 초기화한다.
			/// </summary>
			/// <param name="color"></param>
			public void RequestAlpha(float alpha)
			{
				Color color = _material.GetColor(_shaderID_Color);
				color.a = alpha;

				bool isGray = Mathf.Abs(color.r - 0.5f) < 0.004f && 
								Mathf.Abs(color.g - 0.5f) < 0.004f && 
								Mathf.Abs(color.b - 0.5f) < 0.004f && 
								Mathf.Abs(color.a - 1.0f) < 0.004f;

				if(isGray)
				{
					_isRequested_Color = false;
					color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
				else
				{
					_isRequested_Color = true;
				}

				_material.SetColor(_shaderID_Color, color);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_Color(color);
				}
			}

			/// <summary>
			/// 임의의 프로퍼티의 텍스쳐 속성을 변경한다. 호출 즉시 항상 Batched Material을 사용하게 된다.
			/// </summary>
			public void RequestCustomImage(Texture2D texture, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetTexture(propertyName, texture);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomTexture(texture, propertyName);
				}
			}

			//ID를 사용한 버전 [v1.4.3]
			public void RequestCustomImage(Texture2D texture, int propertyNameID)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetTexture(propertyNameID, texture);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomTexture(texture, propertyNameID);
				}
			}




			public void RequestCustomImageOffset(Vector2 offset, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetTextureOffset(propertyName, offset);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomTextureOffset(offset, propertyName);
				}
			}

			public void RequestCustomImageScale(Vector2 scale, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetTextureScale(propertyName, scale);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomTextureScale(scale, propertyName);
				}
			}





			/// <summary>
			/// 임의의 프로퍼티의 색상 속성을 변경한다. 호출 즉시 항상 Batched Material을 사용하게 된다.
			/// </summary>
			public void RequestCustomColor(Color color, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetColor(propertyName, color);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomColor(color, propertyName);
				}
			}

			//ID를 사용한 버전 [v1.4.3]
			public void RequestCustomColor(Color color, int propertyNameID)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetColor(propertyNameID, color);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomColor(color, propertyNameID);
				}
			}



			/// <summary>
			/// 임의의 프로퍼티의 투명도 속성을 변경한다. 호출 즉시 항상 Batched Material을 사용하게 된다.
			/// </summary>
			public void RequestCustomAlpha(float alpha, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				Color curColor = _material.GetColor(propertyName);
				curColor.a = alpha;
				_material.SetColor(propertyName, curColor);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomColor(curColor, propertyName);
				}
			}

			//ID를 사용한 버전 [v1.4.3]
			public void RequestCustomAlpha(float alpha, int propertyNameID)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				Color curColor = _material.GetColor(propertyNameID);
				curColor.a = alpha;
				_material.SetColor(propertyNameID, curColor);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomColor(curColor, propertyNameID);
				}
			}



			/// <summary>
			/// 임의의 프로퍼티의 Float 속성을 변경한다. 호출 즉시 항상 Batched Material을 사용하게 된다.
			/// </summary>
			public void RequestCustomFloat(float floatValue, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetFloat(propertyName, floatValue);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomFloat(floatValue, propertyName);
				}
			}

			//ID를 사용한 버전 [v1.4.3]
			public void RequestCustomFloat(float floatValue, int propertyNameID)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetFloat(propertyNameID, floatValue);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomFloat(floatValue, propertyNameID);
				}
			}



			/// <summary>
			/// 임의의 프로퍼티의 Int 속성을 변경한다. 호출 즉시 항상 Batched Material을 사용하게 된다.
			/// </summary>
			public void RequestCustomInt(int intValue, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetInt(propertyName, intValue);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomInt(intValue, propertyName);
				}
			}

			//ID를 사용한 버전 [v1.4.3]
			public void RequestCustomInt(int intValue, int propertyNameID)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetInt(propertyNameID, intValue);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomInt(intValue, propertyNameID);
				}
			}

			/// <summary>
			/// 임의의 프로퍼티의 Vector 속성을 변경한다. 호출 즉시 항상 Batched Material을 사용하게 된다.
			/// </summary>
			public void RequestCustomVector4(Vector4 vec4Value, string propertyName)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetVector(propertyName, vec4Value);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomVector4(vec4Value, propertyName);
				}
			}

			//ID를 사용한 버전 [v1.4.3]
			public void RequestCustomVector4(Vector4 vec4Value, int propertyNameID)
			{
				_isRequested_Custom = true;//<<항상 Request 사용 상태
				_material.SetVector(propertyNameID, vec4Value);

				apOptMesh curMesh = null;
				for (int i = 0; i < _linkedMeshes.Count; i++)
				{
					curMesh = _linkedMeshes[i];
					if (curMesh == null)
					{
						continue;
					}

					//각 메시의 Instanced Material도 Batch Material과 동일한 값을 가지도록 한다.
					curMesh.SyncMaterialPropertyByBatch_CustomVector4(vec4Value, propertyNameID);
				}
			}

			public bool IsAnyChanged
			{
				get
				{
					return _isRequested_Texture
						|| _isRequested_Color
						|| _isRequested_Custom;
				}
			}
		}

		//MaterialUnit 리스트 <중요!>
		[SerializeField]
		public List<MaterialUnit> _matUnits = new List<MaterialUnit>();

		//추가 22.6.8 : 빠른 접근을 위한 매핑
		[NonSerialized] private Dictionary<int, MaterialUnit> _mapping_MatUnit = null;
		[NonSerialized] private Dictionary<int, List<MaterialUnit>> _mapping_TextureID2MatUnits = null;



		//추가 19.10.28 : Clipped Child인 경우 Batched는 되지 않지만 일괄처리를 위해 런타임에는 등록할 필요가 있다.
		private class ClippedMatMeshSet
		{
			public apOptMaterialInfo _matInfo = null;
			public apOptMesh _clippedMesh = null;

			[NonSerialized]
			public Material _material_Original = null;

			public ClippedMatMeshSet(apOptMaterialInfo matInfo, apOptMesh clippedMesh, Material clippedMaterial)
			{
				_matInfo = matInfo;
				_clippedMesh = clippedMesh;

				//Reset용으로 등록 당시의 재질을 저장해야한다.
				//여기서 _MaskTex, _MaskTex_L, _MaskTex_R, _MaskScreenSpaceOffset은 복구해서는 안된다.
				_material_Original = new Material(clippedMaterial);
			}

			public bool IsValid()
			{
				return _matInfo != null && _clippedMesh != null;
			}

			public void ResetRequestProperties()
			{
				if(_clippedMesh == null)
				{
					return;
				}
				//리셋하자
				_clippedMesh.SetClippedMaterialPropertyByBatch_Reset(_material_Original);
			}
		}
		[NonSerialized]
		private Dictionary<apOptMesh, ClippedMatMeshSet> _clippedMesh2MatUnits = new Dictionary<apOptMesh, ClippedMatMeshSet>();

		[NonSerialized]
		private List<ClippedMatMeshSet> _clippedMatUnits = new List<ClippedMatMeshSet>();

		

		
		
		[NonSerialized]
		private apPortrait _parentPortrait = null;

		// Init
		//----------------------------------------------------
		public apOptBatchedMaterial()
		{

		}

		public void Link(apPortrait portrait)
		{
			_parentPortrait = portrait;

			//추가 22.6.8 : 빠른 참조를 위한 변수 초기화
			if(_mapping_MatUnit == null) { _mapping_MatUnit = new Dictionary<int, MaterialUnit>(); }
			if(_mapping_TextureID2MatUnits == null) { _mapping_TextureID2MatUnits = new Dictionary<int, List<MaterialUnit>>(); }
			_mapping_MatUnit.Clear();
			_mapping_TextureID2MatUnits.Clear();


			
			int nUnits = _matUnits != null ? _matUnits.Count : 0;
			if (nUnits > 0)
			{
				MaterialUnit curUnit = null;
				List<MaterialUnit> refMatUnits = null;
				for (int i = 0; i < nUnits; i++)
				{
					curUnit = _matUnits[i];
					curUnit._linkedMeshes.Clear();

					//추가 22.6.8 : 빠른 접근을 위한 매핑
					if(!_mapping_MatUnit.ContainsKey(curUnit._uniqueID))
					{
						_mapping_MatUnit.Add(curUnit._uniqueID, curUnit);
					}

					if(!_mapping_TextureID2MatUnits.ContainsKey(curUnit._textureID))
					{
						refMatUnits = new List<MaterialUnit>();
						refMatUnits.Add(curUnit);
						_mapping_TextureID2MatUnits.Add(curUnit._textureID, refMatUnits);
					}
					else
					{
						refMatUnits = _mapping_TextureID2MatUnits[curUnit._textureID];
						refMatUnits.Add(curUnit);
					}
				}
			}
			

			if(_clippedMatUnits == null)
			{
				_clippedMatUnits = new List<ClippedMatMeshSet>();
			}
			if(_clippedMesh2MatUnits == null)
			{
				_clippedMesh2MatUnits = new Dictionary<apOptMesh, ClippedMatMeshSet>();
			}
			_clippedMatUnits.Clear();
			_clippedMesh2MatUnits.Clear();
		}

		public void Clear(bool isDestroyMaterial)
		{
			//Debug.LogWarning("Batched Material 초기화");
			if (isDestroyMaterial)
			{
				MaterialUnit curUnit = null;
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					try
					{
						if (curUnit._material != null)
						{
							UnityEngine.Object.DestroyImmediate(curUnit._material);
						}

						if (curUnit._material_Original != null)
						{
							UnityEngine.Object.DestroyImmediate(curUnit._material_Original);
						}
					}
					catch (Exception) { }
				}
			}
			_matUnits.Clear();

			if(_mapping_MatUnit != null)
			{
				_mapping_MatUnit.Clear();
			}
			

			if(_clippedMatUnits == null)
			{
				_clippedMatUnits = new List<ClippedMatMeshSet>();
			}
			if(_clippedMesh2MatUnits == null)
			{
				_clippedMesh2MatUnits = new Dictionary<apOptMesh, ClippedMatMeshSet>();
			}
			_clippedMatUnits.Clear();
			_clippedMesh2MatUnits.Clear();
		}

		

		// Functions
		//----------------------------------------------------
		/// <summary>
		/// Batched Material을 만들거나, 동일한 Material를 리턴하는 함수.
		/// v1.1.6 또는 그 이전 버전의 함수이다.
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="textureID"></param>
		/// <param name="shader"></param>
		/// <returns></returns>
		public MaterialUnit MakeBatchedMaterial_Prev(Texture2D texture, int textureID, Shader shader)
		{
			MaterialUnit result = _matUnits.Find(delegate (MaterialUnit a)
			{
				return a.IsEqualMaterial_Prev(texture, textureID, shader);
			});
			if(result != null)
			{
				return result;
			}

			//새로 만들자
			int newID = _matUnits.Count + 1;

			result = new MaterialUnit(newID, texture, textureID, shader);
			_matUnits.Add(result);

			return result;
		}

		/// <summary>
		/// Batched Material을 만들거나, 동일한 Material를 리턴하는 함수.
		/// v1.1.7 또는 그 이후 버전을 위한 함수이며 Material Info를 이용한다.
		/// </summary>
		/// <param name="srcMatInfo"></param>
		/// <returns></returns>
		public MaterialUnit MakeBatchedMaterial_MatInfo(apOptMaterialInfo srcMatInfo)
		{
			MaterialUnit result = _matUnits.Find(delegate (MaterialUnit a)
			{
				return a.IsEqualMaterial_MatInfo(srcMatInfo);
			});
			if(result != null)
			{
				return result;
			}

			//새로 만들자
			int newID = _matUnits.Count + 1;

			result = new MaterialUnit(newID, srcMatInfo);
			_matUnits.Add(result);

			return result;
		}


		public MaterialUnit GetMaterialUnit(int materialID, apOptMesh optMesh)
		{
			MaterialUnit result = null;

			//추가 22.6.8 : 빠른 참조를 먼저 시도한다.
			if(_mapping_MatUnit != null)
			{
				_mapping_MatUnit.TryGetValue(materialID, out result);
			}

			if(result == null)
			{
				result = _matUnits.Find(delegate (MaterialUnit a)
				{
					return a._uniqueID == materialID;
				});
			}
			
			if(result == null)
			{
				return null;
			}

			if(result._material == null)
			{
				result.MakeMaterial();
			}

			result.LinkMesh(optMesh);

			return result;
		}


		//추가 19.10.28 : ClippedMesh는 따로 등록을 한다.
		public void LinkClippedMesh(apOptMesh clippedMesh, Material clippedMaterial)
		{
			if(_clippedMatUnits == null)
			{
				_clippedMatUnits = new List<ClippedMatMeshSet>();
			}
			if(_clippedMesh2MatUnits == null)
			{
				_clippedMesh2MatUnits = new Dictionary<apOptMesh, ClippedMatMeshSet>();
			}

			if(_clippedMesh2MatUnits.ContainsKey(clippedMesh))
			{
				return;
			}

			ClippedMatMeshSet clippedMatMeshSet = new ClippedMatMeshSet(clippedMesh.MaterialInfo, clippedMesh, clippedMaterial);
			_clippedMesh2MatUnits.Add(clippedMesh, clippedMatMeshSet);
			_clippedMatUnits.Add(clippedMatMeshSet);
		}
		


		//Shared Materal도 여기서 만들어야 한다.
		public Material GetSharedMaterial_Prev(Texture2D mainTex, Shader shader)
		{
			if(_parentPortrait == null)
			{
				//아직 연결이 안되었다.
				return null;
			}
			return apOptSharedMaterial.I.GetSharedMaterial_Prev(mainTex, shader, _parentPortrait);
		}

		public Material GetSharedMaterial_MatInfo(apOptMaterialInfo matInfo)
		{
			if(_parentPortrait == null)
			{
				//아직 연결이 안되었다.
				return null;
			}
			return apOptSharedMaterial.I.GetSharedMaterial_MatInfo(matInfo, _parentPortrait);
		}



		// 추가 12.13 : 일괄 수정 요청
		//---------------------------------------------------------------
		
		/// <summary>
		/// Batched Material 의 속성을 초기화한다. 이 함수 호출 후에는 Shared Material로 유도한다.
		/// </summary>
		public void ResetAllProperties()
		{
			MaterialUnit curUnit = null;
			//apOptMesh curMesh = null;
				
			for (int i = 0; i < _matUnits.Count; i++)
			{
				curUnit = _matUnits[i];
				curUnit.ResetRequestProperties();
				//curUnit.RefreshLinkedMeshes();
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					_clippedMatUnits[i].ResetRequestProperties();
				}
			}
		}

		public void ResetProperties(int textureID)
		{
			MaterialUnit curUnit = null;
			
			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.ResetRequestProperties();//<<이거 맞추기
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];

					//추가 19.10.28 : 이 코드가 빠지면 버그다.
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.ResetRequestProperties();
					//curUnit.RefreshLinkedMeshes();
				}
			}
			

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet.ResetRequestProperties();
				}
			}
		}

		public void SetMeshImageAll(int textureID, Texture2D newTexture)
		{
			MaterialUnit curUnit = null;
			bool isFastRef = false;


			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestImage(newTexture);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];

					//Debug.Log("[" + i + "] : " + curUnit._textureID);

					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestImage(newTexture);
				}
			}



			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_Texture(newTexture);
				}
			}
		}

		public void SetMeshColorAll(int textureID, Color color)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestColor(color);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestColor(color);
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_Color(color);
				}
			}
		}

		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용
		public void SetMeshColorAll_WithoutTextureID(Color color)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestColor(color);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_Color(color);
				}
			}
		}



		public void SetMeshAlphaAll(int textureID, float alpha)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestAlpha(alpha);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestAlpha(alpha);
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_Alpha(alpha);
				}
			}
		}


		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용
		public void SetMeshAlphaAll_WithoutTextureID(float alpha)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestAlpha(alpha);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_Alpha(alpha);
				}
			}
		}







		public void SetMeshCustomImageAll(int textureID, Texture2D newTexture, string propertyName)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomImage(newTexture, propertyName);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomImage(newTexture, propertyName);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomTexture(newTexture, propertyName);
				}
			}
		}


		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomImageAll(int textureID, Texture2D newTexture, int propertyNameID)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomImage(newTexture, propertyNameID);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomImage(newTexture, propertyNameID);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomTexture(newTexture, propertyNameID);
				}
			}
		}




		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용
		public void SetMeshCustomImageAll_WithoutTextureID(Texture2D newTexture, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomImage(newTexture, propertyName);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomTexture(newTexture, propertyName);
				}
			}
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomImageAll_WithoutTextureID(Texture2D newTexture, int propertyNameID)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomImage(newTexture, propertyNameID);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomTexture(newTexture, propertyNameID);
				}
			}
		}


		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용 안받는 UV Offset 함수
		public void SetMeshCustomImageOffsetAll_WithoutTextureID(Vector2 offset, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomImageOffset(offset, propertyName);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomTextureOffset(offset, propertyName);
				}
			}
		}

		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용 안받는 UV Scale 함수
		public void SetMeshCustomImageScaleAll_WithoutTextureID(Vector2 scale, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomImageScale(scale, propertyName);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomTextureScale(scale, propertyName);
				}
			}
		}








		public void SetMeshCustomColorAll(int textureID, Color color, string propertyName)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomColor(color, propertyName);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때

				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomColor(color, propertyName);
					//curUnit.RefreshLinkedMeshes();
				}
			}


			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomColor(color, propertyName);
				}
			}
		}


		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomColorAll(int textureID, Color color, int propertyNameID)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomColor(color, propertyNameID);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때

				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomColor(color, propertyNameID);
					//curUnit.RefreshLinkedMeshes();
				}
			}


			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomColor(color, propertyNameID);
				}
			}
		}




		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용
		public void SetMeshCustomColorAll_WithoutTextureID(Color color, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomColor(color, propertyName);
			}


			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomColor(color, propertyName);
				}
			}
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomColorAll_WithoutTextureID(Color color, int propertyNameID)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomColor(color, propertyNameID);
			}


			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomColor(color, propertyNameID);
				}
			}
		}




		public void SetMeshCustomAlphaAll(int textureID, float alpha, string propertyName)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomAlpha(alpha, propertyName);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomAlpha(alpha, propertyName);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomAlpha(alpha, propertyName);
				}
			}
		}


		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomAlphaAll(int textureID, float alpha, int propertyNameID)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomAlpha(alpha, propertyNameID);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomAlpha(alpha, propertyNameID);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomAlpha(alpha, propertyNameID);
				}
			}
		}




		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용
		public void SetMeshCustomAlphaAll_WithoutTextureID(float alpha, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomAlpha(alpha, propertyName);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomAlpha(alpha, propertyName);
				}
			}
		}

		public void SetMeshCustomAlphaAll_WithoutTextureID(float alpha, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomAlpha(alpha, propertyNameID);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomAlpha(alpha, propertyNameID);
				}
			}
		}



		public void SetMeshCustomFloatAll(int textureID, float floatValue, string propertyName)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomFloat(floatValue, propertyName);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomFloat(floatValue, propertyName);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomFloat(floatValue, propertyName);
				}
			}
		}


		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomFloatAll(int textureID, float floatValue, int propertyNameID)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomFloat(floatValue, propertyNameID);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomFloat(floatValue, propertyNameID);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomFloat(floatValue, propertyNameID);
				}
			}
		}



		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용
		public void SetMeshCustomFloatAll_WithoutTextureID(float floatValue, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomFloat(floatValue, propertyName);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomFloat(floatValue, propertyName);
				}
			}
		}


		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomFloatAll_WithoutTextureID(float floatValue, int propertyNameID)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomFloat(floatValue, propertyNameID);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomFloat(floatValue, propertyNameID);
				}
			}
		}





		public void SetMeshCustomIntAll(int textureID, int intValue, string propertyName)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomInt(intValue, propertyName);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomInt(intValue, propertyName);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomInt(intValue, propertyName);
				}
			}
		}


		//ID를 사용한 타입
		public void SetMeshCustomIntAll(int textureID, int intValue, int propertyNameID)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomInt(intValue, propertyNameID);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomInt(intValue, propertyNameID);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomInt(intValue, propertyNameID);
				}
			}
		}


		//추가 22.1.9 : 텍스쳐 ID를 안받고 모두 적용
		public void SetMeshCustomIntAll_WithoutTextureID(int intValue, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomInt(intValue, propertyName);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomInt(intValue, propertyName);
				}
			}
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomIntAll_WithoutTextureID(int intValue, int propertyNameID)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomInt(intValue, propertyNameID);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomInt(intValue, propertyNameID);
				}
			}
		}





		public void SetMeshCustomVector4All(int textureID, Vector4 vecValue, string propertyName)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomVector4(vecValue, propertyName);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomVector4(vecValue, propertyName);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomVector4(vecValue, propertyName);
				}
			}
		}



		//ID를 사용한 타입
		public void SetMeshCustomVector4All(int textureID, Vector4 vecValue, int propertyNameID)
		{
			MaterialUnit curUnit = null;

			bool isFastRef = false;

			//추가 22.6.8 : 빠른 참조로 딱 적당한 만큼만 조회하여 처리
			if(_mapping_TextureID2MatUnits != null)
			{
				List<MaterialUnit> targetUnits = null;
				_mapping_TextureID2MatUnits.TryGetValue(textureID, out targetUnits);
				if(targetUnits != null)
				{
					for (int i = 0; i < targetUnits.Count; i++)
					{
						curUnit = targetUnits[i];
						curUnit.RequestCustomVector4(vecValue, propertyNameID);
					}
					isFastRef = true;
				}
			}

			if (!isFastRef)
			{
				//빠른 조회가 실패했을 때
				for (int i = 0; i < _matUnits.Count; i++)
				{
					curUnit = _matUnits[i];
					if (curUnit._textureID != textureID)
					{
						continue;
					}

					curUnit.RequestCustomVector4(vecValue, propertyNameID);
					//curUnit.RefreshLinkedMeshes();
				}
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					if(curCMMSet._matInfo._textureID != textureID)
					{
						continue;
					}

					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomVector4(vecValue, propertyNameID);
				}
			}
		}



		//추가 22.1.9 : 텍스쳐 ID 안받고 모두 적용
		public void SetMeshCustomVector4All_WithoutTextureID(Vector4 vecValue, string propertyName)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomVector4(vecValue, propertyName);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomVector4(vecValue, propertyName);
				}
			}
		}


		//ID를 사용한 버전 [v1.4.3]
		public void SetMeshCustomVector4All_WithoutTextureID(Vector4 vecValue, int propertyNameID)
		{
			for (int i = 0; i < _matUnits.Count; i++)
			{
				_matUnits[i].RequestCustomVector4(vecValue, propertyNameID);
			}

			if(_clippedMatUnits != null && _clippedMatUnits.Count > 0)
			{
				ClippedMatMeshSet curCMMSet = null;
				for (int i = 0; i < _clippedMatUnits.Count; i++)
				{
					curCMMSet = _clippedMatUnits[i];
					if(!curCMMSet.IsValid())
					{
						continue;
					}
					curCMMSet._clippedMesh.SetClippedMaterialPropertyByBatch_CustomVector4(vecValue, propertyNameID);
				}
			}
		}

		// Get / Set
		//----------------------------------------------------
		

	}
}