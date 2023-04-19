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
	/// 메시의 "선택된" 버텍스들을 복사하고 붙여넣는 클래스.
	/// 버텍스와 선분을 복사한다. (Hidden Edge)는 복사하지 않는다.
	/// 가능한 Key 값을 사용하지 않는다.
	/// </summary>
	public class apSnapShot_Mesh : apSnapShotBase
	{
		// Sub Class
		//--------------------------------------------
		public class VertData
		{
			//위치, Z값을 저장한다.
			//UV는 저장하지 않는다. (자동으로 보정하므로)
			public Vector2 _pos = Vector2.zero;
			public float _zDepth = 0.0f;

			public VertData(apVertex srcVert)
			{
				_pos = srcVert._pos;
				_zDepth = srcVert._zDepth;
			}
		}

		public class EdgeData
		{
			//연결 정보
			public VertData _vert1 = null;
			public VertData _vert2 = null;

			public EdgeData(VertData vert1, VertData vert2)
			{
				_vert1 = vert1;
				_vert2 = vert2;
			}
		}

		// Members
		//--------------------------------------------
		private int _nVerts = 0;
		private List<VertData> _verts = null;

		private int _nEdges = 0;
		private List<EdgeData> _edges = null;

		//원본 메시의 OffsetPos를 저장해야한다.
		private Vector2 _srcMeshOffsetPos = Vector2.zero;


		// Init
		//--------------------------------------------
		public apSnapShot_Mesh()
		{
			Clear();
		}

		public override void Clear()
		{
			if(_verts == null)
			{
				_verts = new List<VertData>();
			}
			_verts.Clear();

			if(_edges == null)
			{
				_edges = new List<EdgeData>();
			}
			_edges.Clear();

			_nVerts = 0;
			_nEdges = 0;

			_srcMeshOffsetPos = Vector2.zero;
		}

		// Functions
		//--------------------------------------------
		public bool IsPastable()
		{
			//저장된 값이 있어야 한다.
			if(_nVerts == 0 || _nEdges == 0 || _verts == null || _edges == null)
			{
				return false;
			}

			//특별한 조건은 없다.
			return true;
		}

		public bool Copy(List<apVertex> selectedVerts, apMesh srcMesh)
		{
			if(selectedVerts == null
				|| selectedVerts.Count == 0
				|| srcMesh == null)
			{
				return false;
			}

			Clear();

			//선택된 버텍스 + 선택된 버텍스를 모두 포함하는 Edge들을 리스트로 옮긴다.
			//변환 맵 필요
			Dictionary<apVertex, VertData> src2DstVert = new Dictionary<apVertex, VertData>();
			
			int nSrcVerts = selectedVerts.Count;
			apVertex srcVert = null;
			for (int iSrcVert = 0; iSrcVert < nSrcVerts; iSrcVert++)
			{
				srcVert = selectedVerts[iSrcVert];

				if(src2DstVert.ContainsKey(srcVert))
				{
					continue;
				}
				VertData newVertData = new VertData(srcVert);
				_verts.Add(newVertData);

				src2DstVert.Add(srcVert, newVertData);
			}

			_nVerts = _verts.Count;

			//이제 Edge 짝을 연결해서 저장하자
			//선택된 Vert를 모두 가지고 있는 Edge를 모두 찾자
			int nEdges = srcMesh._edges != null ? srcMesh._edges.Count : 0;
			if(nEdges > 0)
			{
				apMeshEdge srcEdge = null;
				for (int iEdge = 0; iEdge < nEdges; iEdge++)
				{
					srcEdge = srcMesh._edges[iEdge];

					if(src2DstVert.ContainsKey(srcEdge._vert1)
						&& src2DstVert.ContainsKey(srcEdge._vert2)
						&& srcEdge._vert1 != srcEdge._vert2)
					{
						//유효한 경우
						VertData linkedVertData_1 = src2DstVert[srcEdge._vert1];
						VertData linkedVertData_2 = src2DstVert[srcEdge._vert2];
						EdgeData newEdgeData = new EdgeData(linkedVertData_1, linkedVertData_2);
						_edges.Add(newEdgeData);
					}
				}
			}

			_nEdges = _edges.Count;


			_srcMeshOffsetPos = srcMesh._offsetPos;
			

			return true;
		}



		public List<apVertex> Paste(apMesh targetMesh, apDialog_CopyMeshVertPin.POSITION_SPACE posSpace)
		{
			if(_nVerts == 0 
				|| _nEdges == 0 
				|| _verts == null 
				|| _edges == null
				|| targetMesh == null)
			{
				return null;
			}

			//붙여넣자
			VertData curVertData = null;
			List<apVertex> newVerts = new List<apVertex>();
			Dictionary<VertData, apVertex> data2CopiedVert = new Dictionary<VertData, apVertex>();

			//피벗에 대한 오프셋
			Vector2 deltaPivot = targetMesh._offsetPos - _srcMeshOffsetPos;


			for (int i = 0; i < _nVerts; i++)
			{
				curVertData = _verts[i];

				//버텍스를 추가한다.
				//옵션에 따라 위치를 변환해야할 수도 있다.
				Vector2 pastePos = curVertData._pos;
				if(posSpace == apDialog_CopyMeshVertPin.POSITION_SPACE.RelativeToPivot)
				{
					//Pivot에 대한 상대 위치
					pastePos += deltaPivot;
				}
				apVertex copiedVert = targetMesh.AddVertexAutoUV(pastePos);
				copiedVert._zDepth = curVertData._zDepth;

				//Edge를 위해 변환 정보 저장
				data2CopiedVert.Add(curVertData, copiedVert);

				//리턴을 위해서 리스트에 넣자
				newVerts.Add(copiedVert);
			}

			EdgeData curEdgeData = null;
			for (int i = 0; i < _nEdges; i++)
			{
				curEdgeData = _edges[i];

				//변환 정보를 통해서 Edge를 찾자
				apVertex copiedVert1 = null;
				apVertex copiedVert2 = null;

				if(data2CopiedVert.ContainsKey(curEdgeData._vert1))
				{
					copiedVert1 = data2CopiedVert[curEdgeData._vert1];
				}

				if(data2CopiedVert.ContainsKey(curEdgeData._vert2))
				{
					copiedVert2 = data2CopiedVert[curEdgeData._vert2];
				}

				if(copiedVert1 == null || copiedVert2 == null)
				{
					continue;
				}

				targetMesh.MakeNewEdge(copiedVert1, copiedVert2, false);
			}

			return newVerts;

		}



		// Get / Set
		//--------------------------------------------
	}

}