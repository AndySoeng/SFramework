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

	public class apMaskMesh : MonoBehaviour
	{

		// Members
		//-----------------------------------------------------------------
		private Renderer _renderer = null;
		private MeshFilter _meshFilter = null;
		private Mesh _mesh = null;
		private Material _material = null;

		private Vector3[] _vertices = null;
		private int[] _triangles = null;
		private Vector2[] _uvs = null;
		private Color[] _vertColors = null;

		public Transform _childMesh_Base = null;
		public Transform _childMesh_Clip1 = null;
		public Transform _childMesh_Clip2 = null;
		public Transform _childMesh_Clip3 = null;

		private bool _isVisible = false;


		private class SubMeshMap
		{
			public int _meshIndex = -1;//0일때 meshIndex는 Base이다. (RGB는 각각 1, 2,3)
			public Transform _transform = null;
			public MeshFilter _meshFilter = null;
			public Renderer _renderer = null;
			public Material _material = null;
			public Mesh _mesh = null;
			public Vector3[] _vertices = null;
			public int[] _triangles = null;
			public Vector2[] _uvs = null;

			public int _vertexIndexOffset = 0;//<이 Offset을 넣어야 Parent VertexIndex가 된다.
			public Color _color = Color.clear;
			public Texture _texture = null;
			public bool _isVisible = false;

			public SubMeshMap(int meshIndex, Transform childMesh)
			{
				_meshIndex = meshIndex;

				_transform = childMesh;
				_meshFilter = _transform.gameObject.GetComponent<MeshFilter>();
				_renderer = _transform.gameObject.GetComponent<MeshRenderer>();
				_material = _renderer.sharedMaterial;

				_mesh = _meshFilter.sharedMesh;

				_vertices = new Vector3[_mesh.vertices.Length];
				_triangles = new int[_mesh.triangles.Length];
				_uvs = new Vector2[_mesh.uv.Length];

				for (int i = 0; i < _mesh.vertices.Length; i++)
				{
					_vertices[i] = _mesh.vertices[i];
				}

				for (int i = 0; i < _mesh.triangles.Length; i++)
				{
					_triangles[i] = _mesh.triangles[i];
				}

				for (int i = 0; i < _mesh.uv.Length; i++)
				{
					_uvs[i] = _mesh.uv[i];
				}
				_color = _material.color;
				_texture = _material.mainTexture;
			}
			public void SetVertexIndexOffset(int vertexIndexOffset)
			{
				_vertexIndexOffset = vertexIndexOffset;
			}

			public void SetVisible(bool isVisible)
			{
				_isVisible = isVisible;
			}
		}


		private SubMeshMap[] _subMeshes = new SubMeshMap[4];
		private const int SUBMESH_BASE = 0;
		private const int SUBMESH_CLIP1 = 1;
		private const int SUBMESH_CLIP2 = 2;
		private const int SUBMESH_CLIP3 = 3;

		private Color VertexColor_Base = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		private Color VertexColor_Clip1 = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		private Color VertexColor_Clip2 = new Color(0.0f, 1.0f, 0.0f, 1.0f);
		private Color VertexColor_Clip3 = new Color(0.0f, 0.0f, 1.0f, 1.0f);



		// Init
		//-----------------------------------------------------------------
		void Awake()
		{

		}
		void Start()
		{
			try
			{
				ChildMeshesToMaskedMesh();
			}
			catch (Exception ex)
			{
				Debug.LogError("Mask Mesh Init Exception : " + ex);
			}
		}

		void Update()
		{
			if (_isVisible)
			{
				try
				{
					RefreshSubMesh();
				}
				catch (Exception ex)
				{
					Debug.LogError("Mask Mesh Update Exception : " + ex);
				}
			}
		}

		void LateUpdate()
		{

		}

		// Functions
		//-----------------------------------------------------------------
		// 1. Init
		private void ChildMeshesToMaskedMesh()
		{
			this.enabled = false;
			_isVisible = false;

			if (_renderer == null)
			{
				_renderer = gameObject.AddComponent<MeshRenderer>();
				_material = new Material(Shader.Find("AnyPortrait/Transparent/Masked Colored Texture (2X)"));
				_material.name = "AnyPortrait Material (Instance)";
				_material.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

				_renderer.material = _material;
			}

			if (_meshFilter == null)
			{
				_meshFilter = gameObject.AddComponent<MeshFilter>();
				_mesh = new Mesh();
				_meshFilter.mesh = _mesh;
			}

			for (int i = 0; i < _subMeshes.Length; i++)
			{
				_subMeshes[i] = null;
			}

			_vertices = null;
			_triangles = null;
			_uvs = null;
			_vertColors = null;



			if (_childMesh_Base == null)
			{
				return;//이게 없으면 끝입니더
			}

			_subMeshes[SUBMESH_BASE] = new SubMeshMap(SUBMESH_BASE, _childMesh_Base);
			if (_childMesh_Clip1 != null)
			{
				_subMeshes[SUBMESH_CLIP1] = new SubMeshMap(SUBMESH_CLIP1, _childMesh_Clip1);
			}

			if (_childMesh_Clip2 != null)
			{
				_subMeshes[SUBMESH_CLIP2] = new SubMeshMap(SUBMESH_CLIP2, _childMesh_Clip2);
			}

			if (_childMesh_Clip3 != null)
			{
				_subMeshes[SUBMESH_CLIP3] = new SubMeshMap(SUBMESH_CLIP3, _childMesh_Clip3);
			}

			//이제 Vertex List를 만들어주자
			List<Vector3> vertList = new List<Vector3>();
			List<int> triList = new List<int>();
			List<Vector2> uvList = new List<Vector2>();
			List<Color> vertColorList = new List<Color>();

			int curVertexIndexOffset = 0;
			for (int i = 0; i < _subMeshes.Length; i++)
			{
				if (_subMeshes[i] == null)
				{
					continue;
				}

				Transform subTransform = _subMeshes[i]._transform;
				_subMeshes[i].SetVertexIndexOffset(curVertexIndexOffset);

				Vector3[] subVerts = _subMeshes[i]._vertices;
				int[] subTris = _subMeshes[i]._triangles;
				Vector2[] subUVs = _subMeshes[i]._uvs;

				Color vertColor = Color.clear;
				switch (i)
				{
					case SUBMESH_BASE:
						vertColor = VertexColor_Base;
						break;
					case SUBMESH_CLIP1:
						vertColor = VertexColor_Clip1;
						break;
					case SUBMESH_CLIP2:
						vertColor = VertexColor_Clip2;
						break;
					case SUBMESH_CLIP3:
						vertColor = VertexColor_Clip3;
						break;
				}

				for (int iVert = 0; iVert < subVerts.Length; iVert++)
				{
					vertList.Add(transform.InverseTransformPoint(subTransform.TransformPoint(subVerts[iVert])));
					vertColorList.Add(vertColor);
				}

				for (int iTri = 0; iTri < subTris.Length; iTri++)
				{
					triList.Add(subTris[iTri] + curVertexIndexOffset);
				}

				for (int iUV = 0; iUV < subUVs.Length; iUV++)
				{
					uvList.Add(subUVs[iUV]);
				}

				//다음을 위해 Offset 추가
				curVertexIndexOffset += subVerts.Length;

				//Sub Mesh는 안보이게 하자
				_subMeshes[i]._renderer.enabled = false;
			}

			//만든 리스트를 Vertex 배열로 바꾸기
			_vertices = vertList.ToArray();
			_triangles = triList.ToArray();
			_uvs = uvList.ToArray();
			_vertColors = vertColorList.ToArray();

			//메시에 넣자
			_mesh.Clear();
			_mesh.vertices = _vertices;
			_mesh.triangles = _triangles;
			_mesh.uv = _uvs;
			_mesh.colors = _vertColors;

			_mesh.RecalculateNormals();
			_mesh.RecalculateBounds();

			//재질에도 넣자
			Color color_Base = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			Color color_Clip1 = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			Color color_Clip2 = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			Color color_Clip3 = new Color(0.0f, 0.0f, 0.0f, 0.0f);

			Texture texture_Base = null;
			Texture texture_Clip1 = null;
			Texture texture_Clip2 = null;
			Texture texture_Clip3 = null;


			if (_subMeshes[SUBMESH_BASE] != null)
			{
				color_Base = _subMeshes[SUBMESH_BASE]._color;
				texture_Base = _subMeshes[SUBMESH_BASE]._texture;
			}

			if (_subMeshes[SUBMESH_CLIP1] != null)
			{
				color_Clip1 = _subMeshes[SUBMESH_CLIP1]._color;
				texture_Clip1 = _subMeshes[SUBMESH_CLIP1]._texture;
			}

			if (_subMeshes[SUBMESH_CLIP2] != null)
			{
				color_Clip2 = _subMeshes[SUBMESH_CLIP2]._color;
				texture_Clip2 = _subMeshes[SUBMESH_CLIP2]._texture;
			}

			if (_subMeshes[SUBMESH_CLIP3] != null)
			{
				color_Clip3 = _subMeshes[SUBMESH_CLIP3]._color;
				texture_Clip3 = _subMeshes[SUBMESH_CLIP3]._texture;
			}

			_material.SetColor("_Color", color_Base);
			_material.SetColor("_Color1", color_Clip1);
			_material.SetColor("_Color2", color_Clip2);
			_material.SetColor("_Color3", color_Clip3);

			_material.SetTexture("_MainTex", texture_Base);
			_material.SetTexture("_ClipTexture1", texture_Clip1);
			_material.SetTexture("_ClipTexture2", texture_Clip2);
			_material.SetTexture("_ClipTexture3", texture_Clip3);



			_isVisible = false;
			Show();
		}




		//2. Update
		private void RefreshSubMesh()
		{
			for (int i = 0; i < _subMeshes.Length; i++)
			{
				if (_subMeshes[i] == null)
				{
					continue;
				}

				Transform subTransform = _subMeshes[i]._transform;
				Vector3[] subVerts = _subMeshes[i]._vertices;
				int vertexIndexOffset = _subMeshes[i]._vertexIndexOffset;

				for (int iVert = 0; iVert < subVerts.Length; iVert++)
				{
					_vertices[iVert + vertexIndexOffset] = transform.InverseTransformPoint(subTransform.TransformPoint(subVerts[iVert]));
				}
			}

			//메시에 넣자
			_mesh.Clear();
			_mesh.vertices = _vertices;
			_mesh.triangles = _triangles;
			_mesh.uv = _uvs;
			_mesh.colors = _vertColors;

			_mesh.RecalculateNormals();
			_mesh.RecalculateBounds();
		}


		//3. Show / Hide
		public void Show()
		{
			if (_isVisible || _renderer == null)
			{
				return;
			}

			_isVisible = true;
			_renderer.enabled = true;
			this.enabled = true;
		}

		public void Hide()
		{
			if (!_isVisible || _renderer == null)
			{
				return;
			}

			_isVisible = false;
			_renderer.enabled = false;
			this.enabled = false;
		}



		// Get / Set
		//-----------------------------------------------------------------
	}


}