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
	
	public class apOptSharedMaterial
	{

		//싱글톤이다.
		private static apOptSharedMaterial _instance = new apOptSharedMaterial();
		public static apOptSharedMaterial I { get { return _instance; } }

		// Members
		//-----------------------------------------------------------------------------
		/// <summary>
		/// Key - Value로 작동하는 재질 세트
		/// Key : Texture + Shader
		/// Value : Material
		/// 추가로, 사용량도 확인할 수 있다.
		/// </summary>
		public class MaterialUnit
		{
			//이전 방식
			public Texture _texture = null;
			public Shader _shader = null;

			//변경된 방식 19.6.16 : Material Info를 이용한다.
			public bool _isUseMaterialInfo = false;

			public apOptMaterialInfo _materialInfo = null;

			[NonSerialized]
			public Material _material = null;

			public List<apPortrait> _linkedPortraits = new List<apPortrait>();

			public MaterialUnit(Texture texture, Shader shader)
			{
				//구형 버전이다. (Material Info 사용 안함)
				_texture = texture;
				_shader = shader;

				_isUseMaterialInfo = false;
				_materialInfo = null;

				_material = new Material(_shader);
				_material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1.0f));
				_material.SetTexture("_MainTex", _texture);

				if(_linkedPortraits == null)
				{
					_linkedPortraits = new List<apPortrait>();
				}
				_linkedPortraits.Clear();

				//Debug.Log("구형 Shared Material 생성");
			}


			public MaterialUnit(apOptMaterialInfo srcMatInfo)
			{
				_texture = null;
				_shader = null;

				_isUseMaterialInfo = true;
				_materialInfo = new apOptMaterialInfo();
				_materialInfo.MakeFromSrc(srcMatInfo);

				//_texture = _materialInfo._mainTex;
				//_shader = _materialInfo._shader;

				_material = new Material(_materialInfo._shader);
				_material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 1.0f));
				_material.SetTexture("_MainTex", _materialInfo._mainTex);

				//Material Info에 저장된 초기 설정값을 적용한다.
				_materialInfo.SetMaterialProperties(_material);



				if(_linkedPortraits == null)
				{
					_linkedPortraits = new List<apPortrait>();
				}
				_linkedPortraits.Clear();

				//Debug.Log("신형 Shared Material 생성");
			}

			public bool IsEqualMaterialInfo(apOptMaterialInfo matInfo)
			{
				if(!_isUseMaterialInfo)
				{
					//Material Info를 사용하지 않는다.
					return false;
				}

				return apOptMaterialInfo.IsSameInfo(_materialInfo, matInfo);
			}

			public void LinkPortrait(apPortrait portrait)
			{
				if(_linkedPortraits.Contains(portrait))
				{
					return;
				}
				_linkedPortraits.Add(portrait);
			}

			

			public void RemoveMaterial()
			{
				if(_material == null)
				{
					UnityEngine.Object.Destroy(_material);
					_material = null;
				}
			}

			/// <summary>
			/// Portrait의 등록을 해제한다.
			/// 등록된 모든 Portrait가 삭제되었다면, True를 리턴한다. (삭제하도록)
			/// </summary>
			/// <param name="portrait"></param>
			/// <returns></returns>
			public bool RemovePortrait(apPortrait portrait)
			{
				if(_linkedPortraits.Contains(portrait))
				{
					_linkedPortraits.Remove(portrait);
				}

				return (_linkedPortraits.Count == 0);
			}

		}

		//이전 버전 : MaterialInfo를 사용하지 않는 경우
		private Dictionary<Texture, Dictionary<Shader, MaterialUnit>> _matUnits_Prev = new Dictionary<Texture, Dictionary<Shader, MaterialUnit>>();
		private Dictionary<apPortrait, List<MaterialUnit>> _portrait2MatUnits_Prev = new Dictionary<apPortrait, List<MaterialUnit>>();

		//변경 19.6.16 : MaterialInfo에 의해서 리스트가 복잡해짐
		private Dictionary<Texture, Dictionary<Shader, List<MaterialUnit>>> _matUnits_MatInfo = new Dictionary<Texture, Dictionary<Shader, List<MaterialUnit>>>();
		private Dictionary<apPortrait, List<MaterialUnit>> _portrait2MatUnits_MatInfo = new Dictionary<apPortrait, List<MaterialUnit>>();
		
		

		// Init
		//-----------------------------------------------------------------------------
		private apOptSharedMaterial()
		{
			Clear();
		}

		public void Clear()
		{	
			//리스트를 클리어 하기 전에 Material을 삭제해야한다.
			if (_matUnits_Prev == null)
			{
				_matUnits_Prev = new Dictionary<Texture, Dictionary<Shader, MaterialUnit>>();
			}
			else
			{
				foreach (KeyValuePair<Texture, Dictionary<Shader, MaterialUnit>> shaderMatUnit in _matUnits_Prev)
				{
					foreach (KeyValuePair<Shader, MaterialUnit> matSet in shaderMatUnit.Value)
					{
						//재질 삭제
						matSet.Value.RemoveMaterial();
					}
				}
				_matUnits_Prev.Clear();
			}
			_portrait2MatUnits_Prev.Clear();

			//MatInfo 리스트도 초기화
			if (_matUnits_MatInfo == null)
			{
				_matUnits_MatInfo = new Dictionary<Texture, Dictionary<Shader, List<MaterialUnit>>>();
			}
			else
			{
				foreach (KeyValuePair<Texture, Dictionary<Shader, List<MaterialUnit>>> shaderMatUnit in _matUnits_MatInfo)
				{
					foreach (KeyValuePair<Shader, List<MaterialUnit>> matUnitList in shaderMatUnit.Value)
					{
						//재질 삭제
						for (int iUnit = 0; iUnit < matUnitList.Value.Count; iUnit++)
						{
							matUnitList.Value[iUnit].RemoveMaterial();
						}
						
					}
				}
				_matUnits_MatInfo.Clear();
			}
			_portrait2MatUnits_MatInfo.Clear();
		}


		// Functions
		//-----------------------------------------------------------------------------

		//Shared Material을 가져오기

		//Material Info를 사용하지 않는 이전 방식
		public Material GetSharedMaterial_Prev(Texture texture, Shader shader, apPortrait portrait)
		{
#if UNITY_EDITOR
			if(UnityEditor.BuildPipeline.isBuildingPlayer)
			{
				return null;
			}
#endif
			MaterialUnit matUnit = null;

			//Debug.LogWarning("Shared Material - Get Shared Material [ " + texture.name + " / " + shader.name + " / " + portrait.name + " ]");
			if(_matUnits_Prev.ContainsKey(texture))
			{
				if(_matUnits_Prev[texture].ContainsKey(shader))
				{
					matUnit = _matUnits_Prev[texture][shader];
				}
				else
				{
					//새로운 Material Set 생성
					matUnit = new MaterialUnit(texture, shader);

					//Shader 키와 함께 등록
					_matUnits_Prev[texture].Add(shader, matUnit);

					//Debug.Log(">> (!) 새로운 Material 리턴");
				}

			}
			else
			{
				//새로운 Material Set 생성
				matUnit = new MaterialUnit(texture, shader);

				//Texture 키와 리스트 생성
				_matUnits_Prev.Add(texture, new Dictionary<Shader, MaterialUnit>());

				//Shader 키와 함께 등록
				_matUnits_Prev[texture].Add(shader, matUnit);

				//Debug.Log(">> (!) 새로운 Material 리턴");
			}

			//Portrait 등록
			matUnit.LinkPortrait(portrait);
			List<MaterialUnit> matUnitList = null;
			if(!_portrait2MatUnits_Prev.ContainsKey(portrait))
			{
				_portrait2MatUnits_Prev.Add(portrait, new List<MaterialUnit>());
			}
			matUnitList = _portrait2MatUnits_Prev[portrait];
			if(!matUnitList.Contains(matUnit))
			{
				matUnitList.Add(matUnit);
			}

			//Shader Material 반환
			return matUnit._material;
		}



		/// <summary>
		/// Material Info를 이용하여 Shared Material을 가져오거나 만드는 함수 (v1.1.7)
		/// </summary>
		/// <param name="portrait"></param>
		/// <returns></returns>
		public Material GetSharedMaterial_MatInfo(apOptMaterialInfo matInfo, apPortrait portrait)
		{
#if UNITY_EDITOR
			if(UnityEditor.BuildPipeline.isBuildingPlayer)
			{
				return null;
			}
#endif
			
			MaterialUnit matUnit = null;

			if(_matUnits_MatInfo.ContainsKey(matInfo._mainTex))
			{
				if(_matUnits_MatInfo[matInfo._mainTex].ContainsKey(matInfo._shader))
				{
					List<MaterialUnit> matUnitList = _matUnits_MatInfo[matInfo._mainTex][matInfo._shader];

					matUnit = matUnitList.Find(delegate(MaterialUnit a)
					{
						return a.IsEqualMaterialInfo(matInfo);
					});
				}
			}

			//새로 만들어야 한다.
			if(matUnit == null)
			{
				//새로운 Material Unit 생성
				matUnit = new MaterialUnit(matInfo);

				List<MaterialUnit> matUnitList = null;
				Dictionary<Shader, List<MaterialUnit>> shader2MatUnitList = null;

				if(_matUnits_MatInfo.ContainsKey(matInfo._mainTex))
				{
					shader2MatUnitList = _matUnits_MatInfo[matInfo._mainTex];
				}
				else
				{
					shader2MatUnitList = new Dictionary<Shader, List<MaterialUnit>>();
					_matUnits_MatInfo.Add(matInfo._mainTex, shader2MatUnitList);
				}

				if(shader2MatUnitList.ContainsKey(matInfo._shader))
				{
					matUnitList = shader2MatUnitList[matInfo._shader];
				}
				else
				{
					matUnitList = new List<MaterialUnit>();
					shader2MatUnitList.Add(matInfo._shader, matUnitList);
				}

				matUnitList.Add(matUnit);

				//Debug.Log(">> (!) 새로운 Material 리턴");
			}

			//Portrait 등록
			matUnit.LinkPortrait(portrait);

			List<MaterialUnit> matUnitList_InPortrait = null;
			if(!_portrait2MatUnits_MatInfo.ContainsKey(portrait))
			{
				_portrait2MatUnits_MatInfo.Add(portrait, new List<MaterialUnit>());
			}
			matUnitList_InPortrait = _portrait2MatUnits_MatInfo[portrait];
			if(!matUnitList_InPortrait.Contains(matUnit))
			{
				matUnitList_InPortrait.Add(matUnit);
			}

			//Shader Material 반환
			return matUnit._material;
		}

		


		// Event
		//-----------------------------------------------------------------------------
		/// <summary>
		/// apPortrait가 없어질때 호출해야한다.
		/// 가져다쓴 Material이 있다면 최적화를 위해 등록을 해제하고 삭제 여부를 결정한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void OnPortraitDestroyed(apPortrait portrait)
		{
			//Debug.LogError("Shared Material - OnPortraitDestroyed : " + portrait.name);
			OnPortraitDestroyed_Prev(portrait);
			OnPortraitDestroyed_MatInfo(portrait);
		}



		private void OnPortraitDestroyed_Prev(apPortrait portrait)
		{
			if(!_portrait2MatUnits_Prev.ContainsKey(portrait))
			{
				return;
			}

			List<MaterialUnit> optMatUnitList = _portrait2MatUnits_Prev[portrait];
			MaterialUnit curMatSet = null;

			List<Texture> removedTextureKey = new List<Texture>();
			List<Shader> removedShaderKey = new List<Shader>();
			int nRemoved = 0;

			for (int i = 0; i < optMatUnitList.Count; i++)
			{
				curMatSet = optMatUnitList[i];

				//Mat Set에서 Portrait를 삭제한다.
				if(curMatSet.RemovePortrait(portrait))
				{
					//모든 Portrait가 삭제되었다.
					//여기서 바로 재질 삭제
					curMatSet.RemoveMaterial();

					//리스트에서 삭제할 준비
					removedTextureKey.Add(curMatSet._texture);
					removedShaderKey.Add(curMatSet._shader);
					nRemoved++;
				}
			}

			//Debug.LogError(">> " + nRemoved + "개의 쓸모없는 Shared material이 삭제된다.");

			if(nRemoved > 0)
			{
				Texture curTex = null;
				Shader curShader = null;
				for (int i = 0; i < nRemoved; i++)
				{
					curTex = removedTextureKey[i];
					curShader = removedShaderKey[i];

					if(!_matUnits_Prev.ContainsKey(curTex))
					{
						continue;
					}

					//Texture + Shader에 해당하는 Material Set를 삭제한다.
					_matUnits_Prev[curTex].Remove(curShader);

					//만약 이 Texture 키에 대한 Shader-Set 리스트에 아무런 데이터가 없다면
					//이 텍스쳐에 대한 Set도 삭제
					if(_matUnits_Prev[curTex].Count == 0)
					{
						_matUnits_Prev.Remove(curTex);
					}
				}
			}

			//마지막으로 연결 리스트도 삭제
			_portrait2MatUnits_Prev.Remove(portrait);
		}

		private void OnPortraitDestroyed_MatInfo(apPortrait portrait)
		{
			if(!_portrait2MatUnits_MatInfo.ContainsKey(portrait))
			{
				return;
			}

			List<MaterialUnit> optMatUnitList = _portrait2MatUnits_MatInfo[portrait];
			MaterialUnit curMatUnit = null;

			List<Texture> removedTextureKey = new List<Texture>();
			List<Shader> removedShaderKey = new List<Shader>();
			List<MaterialUnit> removedMatUnit = new List<MaterialUnit>();
			int nRemoved = 0;

			for (int i = 0; i < optMatUnitList.Count; i++)
			{
				curMatUnit = optMatUnitList[i];

				//Mat Set에서 Portrait를 삭제한다.
				if(curMatUnit.RemovePortrait(portrait))
				{
					//모든 Portrait가 삭제되었다. > 연결된 Portrait가 0
					//여기서 바로 재질 삭제
					curMatUnit.RemoveMaterial();

					//리스트에서 삭제할 준비
					removedTextureKey.Add(curMatUnit._materialInfo._mainTex);
					removedShaderKey.Add(curMatUnit._materialInfo._shader);
					removedMatUnit.Add(curMatUnit);
					nRemoved++;
				}
			}

			//Debug.LogError(">> " + nRemoved + "개의 쓸모없는 Shared material이 삭제된다.");

			if(nRemoved > 0)
			{
				Texture curTex = null;
				Shader curShader = null;
				//MaterialUnit curMatUnit = null;
				
				List<MaterialUnit> curMatUnits = null;
				for (int i = 0; i < nRemoved; i++)
				{
					curTex = removedTextureKey[i];
					curShader = removedShaderKey[i];
					curMatUnit = removedMatUnit[i];

					if(!_matUnits_MatInfo.ContainsKey(curTex))
					{
						continue;
					}

					//Texture + Shader에 해당하는 Material Set를 삭제한다.
					if(!_matUnits_MatInfo[curTex].ContainsKey(curShader))
					{
						continue;
					}

					curMatUnits = _matUnits_MatInfo[curTex][curShader];

					curMatUnits.Remove(curMatUnit);

					//리스트 > Shader > Texture 순으로 Count가 0이면 삭제
					if(_matUnits_MatInfo[curTex][curShader].Count == 0)
					{
						_matUnits_MatInfo[curTex].Remove(curShader);

						if(_matUnits_MatInfo[curTex].Count == 0)
						{
							_matUnits_MatInfo.Remove(curTex);
						}
					}
				}
			}

			//마지막으로 연결 리스트도 삭제
			_portrait2MatUnits_MatInfo.Remove(portrait);
		}

		// Get / Set
		//-----------------------------------------------------------------------------
		public void DebugAllMaterials()
		{
			
			Debug.LogError("Shared Materials");
			int index = 0;
			foreach (KeyValuePair<Texture, Dictionary<Shader, MaterialUnit>> tex2ShaderMat in _matUnits_Prev)
			{
				Debug.LogWarning("Texture : " + tex2ShaderMat.Key.name);
				foreach (KeyValuePair<Shader, MaterialUnit> shader2Mat in tex2ShaderMat.Value)
				{
					Debug.Log(" [" + index + "] : " + shader2Mat.Key.name + " (" + shader2Mat.Value._linkedPortraits.Count + ")");
					
					index++;
				}
			}
		}
	}
}