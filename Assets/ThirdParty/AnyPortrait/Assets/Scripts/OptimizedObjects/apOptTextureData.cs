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

	//실제로 저장될 Texture Data의 Set
	//Material 정보가 저장되서 실제 리소스로 바뀌어야 한다.
	//존재하는 Material이라면 이후 사용자가 Shader를 바꾸는 등의 작업도 가능하다.
	[Serializable]
	/// <summary>
	/// A data class that stores texture information and meshes that use it.
	/// It is possible to collectively control the shaders of meshes using the same texture.
	/// </summary>
	public class apOptTextureData
	{
		// Members
		//---------------------------------------
		/// <summary>Linked Opt-Meshes</summary>
		[SerializeField]
		public List<apOptMesh> _linkedOptMeshes = new List<apOptMesh>();//이 텍스쳐를 사용하는 OptMesh들

		/// <summary>Texture Asset</summary>
		[SerializeField]
		public Texture2D _texture = null;

		/// <summary>[Please do not use it] UniqueID</summary>
		public int _textureID = -1;

		/// <summary>
		/// [Please do not use it] UniqueID used in Editor
		/// </summary>
		public int _srcUniqueID = -1;

		/// <summary>
		/// Texture Data Name used in Editor
		/// </summary>
		public string _name = "";



		// Init
		//---------------------------------------
		public apOptTextureData()
		{

		}

		// Bake
		//--------------------------------------------------
		/// <summary>
		/// [Please do not use it] Bake Function
		/// </summary>
		/// <param name="textureID"></param>
		/// <param name="textureData"></param>
		public void Bake(int textureID, apTextureData textureData)
		{
			if (_linkedOptMeshes == null)
			{
				_linkedOptMeshes = new List<apOptMesh>();
			}

			_linkedOptMeshes.Clear();
			_texture = textureData._image;

			_textureID = textureID;
			_srcUniqueID = textureData._uniqueID;//
			_name = textureData._name;
		}
		
		
		// Functions
		//---------------------------------------
		/// <summary>
		/// [Please do not use it] Initialization
		/// </summary>
		/// <param name="optMesh"></param>
		public void AddLinkOptMesh(apOptMesh optMesh)
		{
			if(!_linkedOptMeshes.Contains(optMesh))
			{
				_linkedOptMeshes.Add(optMesh);
			}
		}


		/// <summary>
		/// Set the Main Texture of all linked Opt-Meshes
		/// </summary>
		/// <param name="texture"></param>
		public void SetMeshTextureAll(Texture2D texture)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetTexture("_MainTex", texture);
				_linkedOptMeshes[i].SetMeshTexture(texture);
			}
		}

		

		//기타 자세한 재질 일괄 제어
		/// <summary>
		/// Set the Texture of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="propertyName"></param>
		public void SetCustomImageAll(Texture2D texture, string propertyName)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetTexture(paramName, texture);
				_linkedOptMeshes[i].SetCustomTexture(texture, propertyName);
			}
		}


		/// <summary>
		/// Set the Texture of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="propertyNameID">Shader Property ID (Use Shader.PropertyToID)</param>
		public void SetCustomImageAll(Texture2D texture, int propertyNameID)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetTexture(paramName, texture);
				_linkedOptMeshes[i].SetCustomTexture(texture, propertyNameID);
			}
		}

		/// <summary>
		/// Set the Main Color (2X) of all linked Opt-Meshes
		/// </summary>
		/// <param name="color"></param>
		public void SetMeshColorAll(Color color)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetColor(paramName, color2X);
				_linkedOptMeshes[i].SetMeshColor(color);
			}
		}


		/// <summary>
		/// Set the Alpha of Main Color (2X) of all linked Opt-Meshes
		/// </summary>
		public void SetMeshAlphaAll(float alpha)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				_linkedOptMeshes[i].SetMeshAlpha(alpha);
			}
		}

		/// <summary>
		/// Set the Color of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyName"></param>
		public void SetCustomColorAll(Color color, string propertyName)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetColor(paramName, color2X);
				_linkedOptMeshes[i].SetCustomColor(color, propertyName);
			}
		}


		/// <summary>
		/// Set the Color of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyNameID">Shader Property ID (Use Shader.PropertyToID)</param>
		public void SetCustomColorAll(Color color, int propertyNameID)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetColor(paramName, color2X);
				_linkedOptMeshes[i].SetCustomColor(color, propertyNameID);
			}
		}

		/// <summary>
		/// Set the Color's Alpha of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyName"></param>
		public void SetCustomAlphaAll(float alpha, string propertyName)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetColor(paramName, color2X);
				_linkedOptMeshes[i].SetCustomAlpha(alpha, propertyName);
			}
		}


		/// <summary>
		/// Set the Color's Alpha of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyNameID">Shader Property ID (Use Shader.PropertyToID)</param>
		public void SetCustomAlphaAll(float alpha, int propertyNameID)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				//_linkedOptMeshes[i]._material.SetColor(paramName, color2X);
				_linkedOptMeshes[i].SetCustomAlpha(alpha, propertyNameID);
			}
		}

		/// <summary>
		/// Set the Float Value of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="floatValue"></param>
		/// <param name="propertyName"></param>
		public void SetCustomFloatAll(float floatValue, string propertyName)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				_linkedOptMeshes[i].SetCustomFloat(floatValue, propertyName);
			}
		}


		/// <summary>
		/// Set the Float Value of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="floatValue"></param>
		/// <param name="propertyNameID">Shader Property ID (Use Shader.PropertyToID)</param>
		public void SetCustomFloatAll(float floatValue, int propertyNameID)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				_linkedOptMeshes[i].SetCustomFloat(floatValue, propertyNameID);
			}
		}

		/// <summary>
		/// Set the Int Value of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="intValue"></param>
		/// <param name="propertyName"></param>
		public void SetCustomIntAll(int intValue, string propertyName)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				_linkedOptMeshes[i].SetCustomInt(intValue, propertyName);
			}
		}

		/// <summary>
		/// Set the Int Value of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="intValue"></param>
		/// <param name="propertyNameID">Shader Property ID (Use Shader.PropertyToID)</param>
		public void SetCustomIntAll(int intValue, int propertyNameID)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				_linkedOptMeshes[i].SetCustomInt(intValue, propertyNameID);
			}
		}

		/// <summary>
		/// Set the Vector4 Value of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="vector4Value"></param>
		/// <param name="propertyName"></param>
		public void SetCustomVector4All(Vector4 vector4Value, string propertyName)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				_linkedOptMeshes[i].SetCustomVector4(vector4Value, propertyName);
			}
		}



		/// <summary>
		/// Set the Vector4 Value of all linked Opt-Meshes with shader property
		/// </summary>
		/// <param name="vector4Value"></param>
		/// <param name="propertyNameID">Shader Property ID (Use Shader.PropertyToID)</param>
		public void SetCustomVector4All(Vector4 vector4Value, int propertyNameID)
		{
			if(_linkedOptMeshes == null || _linkedOptMeshes.Count == 0) { return; }

			for (int i = 0; i < _linkedOptMeshes.Count; i++)
			{
				_linkedOptMeshes[i].SetCustomVector4(vector4Value, propertyNameID);
			}
		}

		// Get / Set
		//---------------------------------------
	}
}