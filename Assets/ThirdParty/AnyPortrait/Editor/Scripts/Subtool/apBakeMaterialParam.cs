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

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;

//using AnyPortrait;

//namespace AnyPortrait
//{
//	/// <summary>
//	/// Bake하는 과정에서 "공통의 Material을 만들어서 Batch 유도"를 하게 만들기 위한 Material 파라미터.
//	/// 같은 텍스쳐, 동일한 Shader 요청인 경우 만들어진 Material를 리턴한다.
//	/// MaskTexture를 받아야 하는 Clipped Mesh는 Batch 대상이 아니다.
//	/// </summary>
//	public class apBakeMaterialParam
//	{
//		// Members
//		//---------------------------------------------
//		private class MaterialUnit
//		{
//			public Material _material = null;
//			public Texture2D _texture = null;
//			public int _textureID = -1;
//			public Shader _shader = null;

//			public MaterialUnit(	Material material,
//									Texture2D texture,
//									int textureID,
//									Shader shader)
//			{
//				_material = material;
//				_texture = texture;
//				_textureID = textureID;
//				_shader = shader;
//			}
//		}

//		private List<MaterialUnit> _materialUnits = new List<MaterialUnit>();
		

//		// Init
//		//---------------------------------------------
//		public apBakeMaterialParam()
//		{

//		}

//		public void Clear()
//		{
//			_materialUnits.Clear();
//		}
		
//		// Functions
//		//---------------------------------------------
//		public Material GetMaterial(Texture2D texture,
//									int textureID,
//									Shader shader)
//		{
//			MaterialUnit result = _materialUnits.Find(delegate (MaterialUnit a)
//			{
//				return a._texture == texture &&
//						a._textureID == textureID &&
//						a._shader == shader;

//			});

//			if(result != null)
//			{
//				return result._material;
//			}

//			//Material newMat = new Material()
//			return null;
//		}


//		// Get / Set
//		//---------------------------------------------
//	}
//}