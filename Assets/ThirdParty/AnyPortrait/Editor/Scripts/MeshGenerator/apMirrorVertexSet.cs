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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 메시 작업시 Mirror를 켰다면 미리 보기 및 실시간/일괄 복사 작업을 해야한다.
	/// apEditor의 멤버로 속하며, Mesh, VertController의 값을 받아서 갱신한다.
	/// Refresh 함수를 누르면 자동으로 갱신을 한다.
	/// </summary>
	public class apMirrorVertexSet
	{
		// Members
		//-----------------------------------------------
		private apEditor _editor = null;

		public class CloneVertex
		{
			private static int s_index = 0;
			public int _index = -1;
			public apVertex _srcVert = null;
			public apMeshEdge _srcSplitEdge = null;
			public Vector2 _pos = Vector2.zero;
			public bool _isOnAxis = false;//CrossOnAxis와 달리, 기존의 Vertex 자체가 축 위에 있는 것 (나중에 보정이 된다)
			public bool _isCrossOnAxis = false;//<<축에서 반전되기 위해서 생성된 점 (이게 True일 때에는 srcVert가 없다)

			public List<CloneVertex> _linkedCloneVerts = new List<CloneVertex>();
			public List<apVertex> _linkedSrcVerts = new List<apVertex>();//<<자신의 미러 소스를 제외한 나머지 apVertex

			public int _compareToMirror = 0;

			public static void ClearIndex()
			{
				s_index = 0;
			}

			public static CloneVertex MakeMirrorPoint(apVertex srcVert, Vector2 pos, int compare)
			{
				CloneVertex newCVert = new CloneVertex();
				newCVert._srcVert = srcVert;
				newCVert._pos = pos;
				newCVert._compareToMirror = compare;
				return newCVert;
			}

			public static CloneVertex MakePointOnAxis(apVertex srcVert)
			{
				CloneVertex newCVert = new CloneVertex();
				newCVert._srcVert = srcVert;
				newCVert._pos = srcVert._pos;
				newCVert._isOnAxis = true;
				newCVert._compareToMirror = 0;
				return newCVert;
			}

			public static CloneVertex MakeCrossPoint(Vector2 pos, apMeshEdge splitEdge)
			{
				CloneVertex newCVert = new CloneVertex();
				newCVert._pos = pos;
				newCVert._isCrossOnAxis = true;
				newCVert._srcSplitEdge = splitEdge;
				newCVert._compareToMirror = 0;
				return newCVert;
			}

			private CloneVertex()
			{
				_srcVert = null;
				_pos = Vector2.zero;
				_isCrossOnAxis = false;//<<축에서 반전되기 위해서 생성된 점 (이게 True일 때에는 srcVert가 없다)

				_linkedCloneVerts.Clear();
				_linkedSrcVerts.Clear();

				//인덱스 증가
				_index = s_index;
				s_index++;
			}

			public CloneVertex AddLinkedCloneVert(CloneVertex cloneVert)
			{
				_linkedCloneVerts.Add(cloneVert);
				return this;
			}

			public CloneVertex AddLinkedSrcVert(apVertex srcVert)
			{	
				_linkedSrcVerts.Add(srcVert);
				return this;
			}

			public void RemoveLinkedCloneVert(CloneVertex cloneVert)
			{
				if(_linkedCloneVerts.Contains(cloneVert))
				{
					_linkedCloneVerts.Remove(cloneVert);
				}
			}
		}

		public class CloneEdge
		{
			public CloneVertex _cloneVert1 = null;
			public CloneVertex _cloneVert2 = null;

			public CloneEdge(CloneVertex vert1, CloneVertex vert2)
			{
				_cloneVert1 = vert1;
				_cloneVert2 = vert2;
			}

			public bool IsSame(CloneVertex vert1, CloneVertex vert2)
			{
				return (_cloneVert1 == vert1 && _cloneVert2 == vert2) 
					|| (_cloneVert1 == vert2 && _cloneVert2 == vert1);
			}
		}


		public List<CloneVertex> _cloneVerts = new List<CloneVertex>();
		public List<CloneVertex> _crossVerts = new List<CloneVertex>();
		public List<CloneEdge> _cloneEdges = new List<CloneEdge>();

		private apMesh _mesh = null;
		private int _nSelectedVert = 0;
		private int _nTotalVert = 0;
		private int _nTotalEdge = 0;
		//private bool _isCleared = false;
		private float _offset = 0.0f;
		private bool _isAddOnRuler = false;
		private bool _isMirrorX = true;
		private float _mirrorPos = 0.0f;

		private List<apMeshEdge> _srcEdges = new List<apMeshEdge>();
		private List<apVertex> _srcVerts = new List<apVertex>();
		private Dictionary<apVertex, List<apVertex>> _vert2Verts = new Dictionary<apVertex, List<apVertex>>();
		private Dictionary<apVertex, CloneVertex> _vert2Clone = new Dictionary<apVertex, CloneVertex>();
		private Dictionary<CloneVertex, apVertex> _clone2Vert = new Dictionary<CloneVertex, apVertex>();

		//Make Mesh에서 Add Vert / Add Vert+Edge할 때의 변수
		//현재 작업중인 Mesh Work의 미러 위치들
		public enum MIRROR_MESH_WORK_TYPE
		{
			None,//입력된 값이 없다.
			New,//버텍스는 없으나 새로 생성해야 한다.
			ExistVertex,//버텍스가 있다. Start 지점에서 사용
			IsOnRuler,//Ruler에 위치함, 나중에 위치 보정해야한다.
			MergedVertex,//위치가 비슷해서 합쳐진다. 이 경우, 서로 연결하지 않는 한, Mirror쪽이 위치를 보정한다.

		}
		public MIRROR_MESH_WORK_TYPE _meshWork_TypePrev = MIRROR_MESH_WORK_TYPE.None;
		public apVertex _meshWork_VertPrev = null;
		public Vector2 _meshWork_PosPrev = Vector2.zero;//<<Mesh 좌표계 (World + meshOffset)
		

		public MIRROR_MESH_WORK_TYPE _meshWork_TypeNext = MIRROR_MESH_WORK_TYPE.None;
		public apVertex _meshWork_VertNext = null;
		public Vector2 _meshWork_PosNext = Vector2.zero;
		public bool _meshWork_SnapToAxis = false;
		public Vector2 _meshWork_PosNextSnapped = Vector2.zero;

		//버텍스 이동을 위한 버텍스 변수
		private apVertex _movedSrcVertex = null;
		private apVertex _movedMirrorVertex = null;

		// Init
		//-----------------------------------------------
		public apMirrorVertexSet(apEditor editor)
		{
			_editor = editor;
			//_isCleared = false;
			Clear();
		}

		public void Clear()
		{
			//if (_isCleared)
			//{
			//	return;
			//}
			_cloneVerts.Clear();
			_crossVerts.Clear();
			_cloneEdges.Clear();
			_mesh = null;
			_nSelectedVert = 0;
			_nTotalVert = 0;
			_nTotalEdge = 0;
			_offset = 0.0f;
			_isAddOnRuler = false;
			_isMirrorX = true;
			_mirrorPos = 0.0f;
			_srcEdges.Clear();
			_srcVerts.Clear();
			_vert2Verts.Clear();

			_vert2Clone.Clear();
			_clone2Vert.Clear();
			CloneVertex.ClearIndex();

			//_isCleared = true;
		}

		/// <summary>
		/// 마우스를 새로 클릭할 때 이 함수를 호출한다.
		/// </summary>
		public void ClearMovedVertex()
		{
			_movedSrcVertex = null;
			_movedMirrorVertex = null;
		}
		

		// Functions
		//-----------------------------------------------
		public void Refresh(apMesh mesh, bool isReset)
		{
			//리셋할지 말지 체크
			if(!isReset)
			{
				if(_mesh != mesh ||
					mesh == null ||
					_nSelectedVert != _editor.VertController.Vertices.Count ||
					_nTotalVert != mesh._vertexData.Count ||
					_nTotalEdge != mesh._edges.Count ||
					!Mathf.Approximately(_offset, _editor._meshTRSOption_MirrorOffset) ||
					_isAddOnRuler != _editor._meshTRSOption_MirrorSnapVertOnRuler
					)
				{
					isReset = true;
				}
				else
				{
					if(mesh != null)
					{
						if(_isMirrorX != mesh._isMirrorX)
						{
							isReset = true;
						}
						else if((mesh._isMirrorX && !Mathf.Approximately(_mirrorPos, mesh._mirrorAxis.x))
							|| (!mesh._isMirrorX && !Mathf.Approximately(_mirrorPos, mesh._mirrorAxis.y)))
						{
							isReset = true;
						}
					}
				}
			}


			if(!isReset)
			{
				//리스트를 돌면서
				//SrcVert의 위치가 미러 축의 (+, -, 0) 중 어디에 들어가는지 확인하자
				//하나라도 변경된게 있다면 Reset이다.
				CloneVertex checkClone = null;
				for (int i = 0; i < _cloneVerts.Count; i++)
				{
					checkClone = _cloneVerts[i];
					if(checkClone._compareToMirror != GetMirrorCompare(checkClone._srcVert._pos, true))
					{
						//리셋을 해야할 만큼 위치가 바뀌었다.
						//Debug.LogError("Mirror 변경됨");
						isReset = true;
						break;
					}
				}

			}

			if (isReset)
			{
				Clear();

				if (mesh == null)
				{
					return;
				}

				if (_editor.VertController.Vertices.Count == 0)
				{
					return;
				}

				List<apVertex> selectVerts = _editor.VertController.Vertices;
				List<apVertex> verts = mesh._vertexData;
				List<apMeshEdge> edges = mesh._edges;


				_mesh = mesh;
				_nSelectedVert = selectVerts.Count;
				_nTotalVert = verts.Count;
				_nTotalEdge = edges.Count;
				_offset = _editor._meshTRSOption_MirrorOffset;
				_isAddOnRuler = _editor._meshTRSOption_MirrorSnapVertOnRuler;
				_isMirrorX = mesh._isMirrorX;
				_mirrorPos = mesh._isMirrorX ? mesh._mirrorAxis.x : mesh._mirrorAxis.y;

				apMeshEdge curEdge = null;
				apVertex curVert1 = null;
				apVertex curVert2 = null;
				for (int iEdge = 0; iEdge < edges.Count; iEdge++)
				{
					curEdge = edges[iEdge];
					curVert1 = curEdge._vert1;
					curVert2 = curEdge._vert2;

					//하나라도 등록되어 있다면 "미러가 될 Edge"로 일단 등록
					bool isVert1InList = selectVerts.Contains(curVert1);
					bool isVert2InList = selectVerts.Contains(curVert2);

					if (isVert1InList || isVert2InList)
					{
						_srcEdges.Add(curEdge);

						if (isVert1InList)
						{
							if (!_srcVerts.Contains(curVert1))
							{
								_srcVerts.Add(curVert1);
							}

							if (!_vert2Verts.ContainsKey(curVert1))
							{
								_vert2Verts.Add(curVert1, new List<apVertex>());
							}
							if (!_vert2Verts[curVert1].Contains(curVert2))
							{
								_vert2Verts[curVert1].Add(curVert2);//<Vert1 -> Vert2가 이어져 있음을 등록
							}
						}
						if (isVert2InList)
						{
							if (!_srcVerts.Contains(curVert2))
							{
								_srcVerts.Add(curVert2);
							}
							if (!_vert2Verts.ContainsKey(curVert2))
							{
								_vert2Verts.Add(curVert2, new List<apVertex>());
							}
							if (!_vert2Verts[curVert2].Contains(curVert1))
							{
								_vert2Verts[curVert2].Add(curVert1);//<Vert2 -> Vert1가 이어져 있음을 등록
							}
						}

						//Edge중에서 축에 Cross되는 경우
						if (IsCrossAxis(curEdge._vert1._pos, curEdge._vert2._pos))
						{
							Vector2 crossPos = GetCrossPos(curEdge._vert1._pos, curEdge._vert2._pos);
							_crossVerts.Add(CloneVertex.MakeCrossPoint(crossPos, curEdge)
												.AddLinkedSrcVert(curEdge._vert1)
												.AddLinkedSrcVert(curEdge._vert2));

						}
					}
				}

				//등록된 Vertex 리스트를 바탕으로 Clone을 만들자
				//- 옵션에 따라 바뀐다.
				//- 일단 Clone간의 연결은 생략하고, Vert -> Clone만 만들자
				//- 위치 보정이 중요
				//- Cross가 되는 Clone은 여기서 만들지 않는다.
				apVertex srcVert = null;
				List<apVertex> linkedVerts = null;
				apVertex linkedVert = null;

				for (int iVert = 0; iVert < _srcVerts.Count; iVert++)
				{
					srcVert = _srcVerts[iVert];
					CloneVertex newCVert = null;
					if (IsOnAxis(srcVert._pos))
					{
						//Axis 위에 있다면..
						newCVert = CloneVertex.MakePointOnAxis(srcVert);
					}
					else
					{
						//그렇지 않다면
						Vector2 mirrorPos = GetMirrorPos(srcVert._pos);
						newCVert = CloneVertex.MakeMirrorPoint(srcVert, mirrorPos, GetMirrorCompare(srcVert._pos, false));
					}

					//이어진 Vertex 들을 입력하자
					linkedVerts = _vert2Verts[srcVert];
					for (int iLink = 0; iLink < linkedVerts.Count; iLink++)
					{
						linkedVert = linkedVerts[iLink];
						newCVert.AddLinkedSrcVert(linkedVert);
					}
					_cloneVerts.Add(newCVert);
					_clone2Vert.Add(newCVert, srcVert);
					_vert2Clone.Add(srcVert, newCVert);
				}

				//CloneVert간에 연결을 하자
				CloneVertex cloneVert = null;
				CloneVertex linkedCloneVert = null;
				for (int iClone = 0; iClone < _cloneVerts.Count; iClone++)
				{
					cloneVert = _cloneVerts[iClone];
					for (int iLinkedSrc = 0; iLinkedSrc < cloneVert._linkedSrcVerts.Count; iLinkedSrc++)
					{
						srcVert = cloneVert._linkedSrcVerts[iLinkedSrc];
						if (_vert2Clone.ContainsKey(srcVert))
						{
							linkedCloneVert = _vert2Clone[srcVert];
							cloneVert.AddLinkedCloneVert(linkedCloneVert);
						}
					}
				}

				//CrossVert를 이용해서 연결 관계를 바꾸자
				CloneVertex crossVert = null;
				CloneVertex linkedCloneVert1 = null;
				CloneVertex linkedCloneVert2 = null;
				for (int iCross = 0; iCross < _crossVerts.Count; iCross++)
				{
					crossVert = _crossVerts[iCross];
					curVert1 = crossVert._srcSplitEdge._vert1;
					curVert2 = crossVert._srcSplitEdge._vert2;

					linkedCloneVert1 = null;
					linkedCloneVert2 = null;
					if (_vert2Clone.ContainsKey(curVert1))
					{
						linkedCloneVert1 = _vert2Clone[curVert1];
						crossVert.AddLinkedCloneVert(linkedCloneVert1);
					}
					if (_vert2Clone.ContainsKey(curVert2))
					{
						linkedCloneVert2 = _vert2Clone[curVert2];
						crossVert.AddLinkedCloneVert(linkedCloneVert2);
					}

					if (linkedCloneVert1 != null)
					{
						linkedCloneVert1.AddLinkedCloneVert(crossVert);
						//만약 반대쪽이 연결된 상태라면 연결을 끊는다.
						if (linkedCloneVert2 != null)
						{
							linkedCloneVert1.RemoveLinkedCloneVert(linkedCloneVert2);
						}
					}

					if (linkedCloneVert2 != null)
					{
						linkedCloneVert2.AddLinkedCloneVert(crossVert);
						//만약 반대쪽이 연결된 상태라면 연결을 끊는다.
						if (linkedCloneVert1 != null)
						{
							linkedCloneVert2.RemoveLinkedCloneVert(linkedCloneVert1);
						}
					}
				}

				//CloneEdge를 완성한다. >> _cloneEdges
				//빠른 중복 
				CloneVertex cloneVert1 = null;
				CloneVertex cloneVert2 = null;
				Dictionary<CloneVertex, List<CloneVertex>> edgedVertPairs = new Dictionary<CloneVertex, List<CloneVertex>>();
				for (int iClone = 0; iClone < _cloneVerts.Count; iClone++)
				{
					cloneVert = _cloneVerts[iClone];
					for (int iLinkClone = 0; iLinkClone < cloneVert._linkedCloneVerts.Count; iLinkClone++)
					{
						linkedCloneVert = cloneVert._linkedCloneVerts[iLinkClone];
						if (cloneVert._index < linkedCloneVert._index)
						{
							cloneVert1 = cloneVert;
							cloneVert2 = linkedCloneVert;
						}
						else
						{
							cloneVert1 = linkedCloneVert;
							cloneVert2 = cloneVert;
						}

						bool isExisted = false;
						if (edgedVertPairs.ContainsKey(cloneVert1))
						{
							if (edgedVertPairs[cloneVert1].Contains(cloneVert2))
							{
								isExisted = true;
							}
							else
							{
								edgedVertPairs[cloneVert1].Add(cloneVert2);
							}
						}
						else
						{
							edgedVertPairs.Add(cloneVert1, new List<CloneVertex>());
							edgedVertPairs[cloneVert1].Add(cloneVert2);
						}
						if (!isExisted)
						{
							_cloneEdges.Add(new CloneEdge(cloneVert1, cloneVert2));
						}
					}
				}
			}
			else
			{
				//위치만 갱신하자
				CloneVertex curCloneVert = null;
				for (int iClone = 0; iClone < _cloneVerts.Count; iClone++)
				{
					curCloneVert = _cloneVerts[iClone];
					if(!curCloneVert._isOnAxis)
					{
						curCloneVert._pos = GetMirrorPos(curCloneVert._srcVert._pos);
					}
					else
					{
						curCloneVert._pos = curCloneVert._srcVert._pos;
					}
				}

				for (int iClone = 0; iClone < _crossVerts.Count; iClone++)
				{
					curCloneVert = _crossVerts[iClone];
					if(curCloneVert._linkedSrcVerts.Count < 2)
					{
						continue;
					}

					Vector2 crossPos = GetCrossPos(curCloneVert._linkedSrcVerts[0]._pos, 
												curCloneVert._linkedSrcVerts[1]._pos);
					curCloneVert._pos = crossPos;
				}
			}
		}


		private bool IsOnAxis(Vector2 pos)
		{
			if(_isMirrorX)
			{
				if(Mathf.Abs(pos.x - _mirrorPos) < _offset)
				{
					return true;
				}
			}
			else
			{
				if(Mathf.Abs(pos.y - _mirrorPos) < _offset)
				{
					return true;
				}
			}
			return false;
		}


		private bool IsCrossAxis(Vector2 posA, Vector2 posB)
		{
			if(_isMirrorX)
			{
				if((posA.x < _mirrorPos - _offset && posB.x >= _mirrorPos + _offset)
					|| (posB.x < _mirrorPos - _offset && posA.x >= _mirrorPos + _offset))
				{
					return true;
				}
				
			}
			else
			{
				if((posA.y <= _mirrorPos - _offset && posB.y >= _mirrorPos + _offset)
					|| (posB.y <= _mirrorPos - _offset && posA.y >= _mirrorPos + _offset))
				{
					return true;
				}
			}
			return false;
		}

		private Vector2 GetMirrorPos(Vector2 pos)
		{
			Vector2 result = pos;
			if(_isMirrorX)
			{
				result.x = _mirrorPos - (pos.x - _mirrorPos);
			}
			else
			{
				result.y = _mirrorPos - (pos.y - _mirrorPos);
			}
			return result;
		}

		private int GetMirrorCompare(Vector2 pos, bool isOffsetUse)
		{
			if (!isOffsetUse)
			{
				if (_isMirrorX)
				{
					if (pos.x > _mirrorPos)
					{
						return 1;
					}
				}
				else
				{
					if (pos.y > _mirrorPos)
					{
						return 1;
					}
				}
				//0인 경우는 위에서 체크
				//-1로 리턴
				return -1;
			}
			else
			{
				if (_isMirrorX)
				{
					if (pos.x >= _mirrorPos + _offset)
					{
						return 1;
					}
					if (pos.x <= _mirrorPos - _offset)
					{
						return -1;
					}
				}
				else
				{
					if (pos.y >= _mirrorPos + _offset)
					{
						return 1;
					}
					if (pos.y <= _mirrorPos - _offset)
					{
						return -1;
					}
				}

				return 0;
			}
			
		}

		private Vector2 GetCrossPos(Vector2 posA, Vector2 posB)
		{
			float dX = posB.x - posA.x;
			float dY = posB.y - posA.y;
			float t = 0.0f;
			if(_isMirrorX)
			{
				t = (_mirrorPos - posA.x) / dX;//위의 조건문으로 dX는 0이 되지 않는다.
			}
			else
			{
				t = (_mirrorPos - posA.y) / dY;//위의 조건문으로 dY는 0이 되지 않는다.
			}
			return new Vector2(dX * t + posA.x, dY * t + posA.y);
		}

		// Make Mesh에서 미리보기의 경우
		//----------------------------------------------------------------------------------
		
		

		public void RefreshMeshWork(apMesh mesh, apVertexController vertexController)
		{
			_meshWork_TypePrev = MIRROR_MESH_WORK_TYPE.None;
			_meshWork_VertPrev = null;
			_meshWork_PosPrev = Vector2.zero;

			_meshWork_TypeNext = MIRROR_MESH_WORK_TYPE.None;
			_meshWork_VertNext = null;
			_meshWork_PosNext = Vector2.zero;
			_meshWork_SnapToAxis = false;
			_meshWork_PosNextSnapped = Vector2.zero;

			if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
			{
				return;
			}

			if(_editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.None)
			{
				return;
			}

			//if (vertexController.Vertex == null)
			//{
			//	return;
			//}

			float mirrorOffset = _editor._meshTRSOption_MirrorOffset;
			//bool isMirrorSnap = _editor._meshTRSOption_MirrorSnapVertOnRuler;
			float mirrorPos = mesh._isMirrorX ? mesh._mirrorAxis.x : mesh._mirrorAxis.y;


			//1. Prev를 설정하자
			if(vertexController.Vertex != null)
			{
				_meshWork_TypePrev = MIRROR_MESH_WORK_TYPE.New;
				//_meshWork_TypePrev = MIRROR_MESH_WORK_TYPE.ExistVertex;
				//_meshWork_VertPrev = vertexController.Vertex;
				_meshWork_PosPrev = GetMirrorPosByMesh(vertexController.Vertex._pos, mesh);

				//위치를 확인하고 중점인지 보자
				if(_isMirrorX)
				{
					if(_meshWork_PosPrev.x > mirrorPos - mirrorOffset && 
						_meshWork_PosPrev.x < mirrorPos + mirrorOffset)
					{
						_meshWork_TypePrev = MIRROR_MESH_WORK_TYPE.IsOnRuler;
					}
				}
				else
				{
					if(_meshWork_PosPrev.y > mirrorPos - mirrorOffset && 
						_meshWork_PosPrev.y < mirrorPos + mirrorOffset)
					{
						_meshWork_TypePrev = MIRROR_MESH_WORK_TYPE.IsOnRuler;
					}
				}
			}

			//2. Next를 설정하자
			Vector2 mouseGL = vertexController.TmpEdgeWirePos;
			Vector2 mouseW = apGL.GL2World(mouseGL);
			
			_meshWork_TypeNext = MIRROR_MESH_WORK_TYPE.New;
			_meshWork_PosNext = GetMirrorPosByMesh(mouseW + mesh._offsetPos, mesh);

			//가까운게 있는가 // VertPrev / Next 모두 검사
			float clickableOffset = Mathf.Max(6.0f, mirrorOffset);//6은 기본 클릭 범위

			apVertex curVert = null;
			for (int iVert = 0; iVert < mesh._vertexData.Count; iVert++)
			{
				curVert = mesh._vertexData[iVert];
				//if(curVert == _meshWork_VertPrev)
				//{
				//	continue;
				//}
				if (_meshWork_VertPrev == null)
				{
					if (Mathf.Abs(curVert._pos.x - _meshWork_PosPrev.x) < clickableOffset
						&& Mathf.Abs(curVert._pos.y - _meshWork_PosPrev.y) < clickableOffset)
					{
						//선택할 수 있다.
						_meshWork_VertPrev = curVert;
						_meshWork_TypePrev = MIRROR_MESH_WORK_TYPE.MergedVertex;//<<합쳐질 수 있는 Vertex
					}
				}

				if (_meshWork_VertNext == null)
				{
					if (Mathf.Abs(curVert._pos.x - _meshWork_PosNext.x) < clickableOffset
						&& Mathf.Abs(curVert._pos.y - _meshWork_PosNext.y) < clickableOffset)
					{
						//선택할 수 있다.
						_meshWork_VertNext = curVert;
						_meshWork_TypeNext = MIRROR_MESH_WORK_TYPE.MergedVertex;//<<합쳐질 수 있는 Vertex
					}
				}
				if(_meshWork_VertPrev != null && _meshWork_VertNext != null)
				{
					//다 찾았다.
					break;
				}
			}
			// 다만, Next와 Prev가 같다면..
			if(_meshWork_VertPrev == _meshWork_VertNext)
			{
				_meshWork_VertNext = null;
				_meshWork_TypeNext = MIRROR_MESH_WORK_TYPE.New;
			}
			
			if(_editor._meshTRSOption_MirrorSnapVertOnRuler)
			{
				//스냅 옵션일 때
				//위치를 보정해주자
				Vector2 nextPos = _meshWork_PosNext;
				if(_meshWork_VertNext != null)
				{
					nextPos = _meshWork_VertNext._pos;
				}
				if(mesh._isMirrorX)
				{
					if(nextPos.x > mirrorPos - mirrorOffset && 
						nextPos.x < mirrorPos + mirrorOffset)
					{
						_meshWork_SnapToAxis = true;
						_meshWork_PosNextSnapped = nextPos;
						_meshWork_PosNextSnapped.x = mirrorPos;
					}
				}
				else
				{
					if(nextPos.y > mirrorPos - mirrorOffset && 
						nextPos.y < mirrorPos + mirrorOffset)
					{
						_meshWork_SnapToAxis = true;
						_meshWork_PosNextSnapped = nextPos;
						_meshWork_PosNextSnapped.y = mirrorPos;
					}
				}
				

			}
		}

		//----------------------------------------------------------------
		public void AddMirrorVertex(apVertex prevVert, apVertex addedVert, apMesh mesh, bool isAddEdge, bool isShift, bool isNewVert, bool isAddVertexWithSplit)
		{
			apVertex mirrorVert_Prev = null;
			apVertex mirrorVert_Next = null;

			//float mirrorOffset = _editor._meshTRSOption_MirrorOffset;
			//float clickableOffset = Mathf.Max(6.0f, mirrorOffset);//6은 기본 클릭 범위
			float clickableOffset = 6.0f;
			if(isNewVert)
			{
				//새로운 버텍스를 생성했다면 미러쪽도 새로 만드는 방향으로 해야한다.
				clickableOffset = 2.0f;
			}

			bool isSnap = _editor._meshTRSOption_MirrorSnapVertOnRuler;

			//추가된 버텍스가 있다 (각각 Prev, Next에 대해서 동일한 처리)
			//=> 1) 축에 있다면
			//		=> Snap 옵션이 켜졌다면 Mirror대신 축으로 위치를 보정한다. (미러 생성 안하고 mirrorVert를 이걸로 설정)
			//		=> Snap 옵션이 꺼졌다면 처리하지 않는다. (Mirror가 안됨)
			//=> 2) 미러 위치를 계산한다.
			//		=> 근처에 버텍스가 있다면 그걸 선택하고, 위치를 보정한다. (Prev와 Next는 같은 점을 공유할 수 없다.)
			//		=> 근처에 버텍스가 없다면 새로 생성한다.
			//=> isAddEdge가 True일 때, mirrorVert Prev, Next가 모두 있다면 Edge 생성. 단, 서로 교차된 경우는 생략한다.
			


			if (prevVert != null)
			{
				if(IsOnAxisByMesh(prevVert._pos, mesh))
				{
					//1) 축에 있는가
					if(isSnap)
					{
						//=> 위치만 보정한다.
						mirrorVert_Prev = prevVert;
						mirrorVert_Prev._pos = GetAxisPosToSnap(prevVert._pos, mesh);
					}
					else
					{
						//=> 위치 보정 없이 그냥 선택한다.
						mirrorVert_Prev = prevVert;
					}
				}
				else
				{
					//2) 축 바깥이라면 미러 위치를 계산한다.
					Vector2 mirrorPos = GetMirrorPosByMesh(prevVert._pos, mesh);
					
					//근처에 Vertex가 있는가
					apVertex nearestVert = FindNearestVertex(mirrorPos, mesh, clickableOffset);
					if(nearestVert != null)
					{
						//=> 이걸 선택하고 위치만 보정한다.
						mirrorVert_Prev = nearestVert;
						mirrorVert_Prev._pos = mirrorPos;
					}
					else
					{
						//=> 새로 생성하자
						mirrorVert_Prev = mesh.AddVertexAutoUV(mirrorPos);
					}
				}
			}

			if (addedVert != null && prevVert != addedVert)
			{
				if(IsOnAxisByMesh(addedVert._pos, mesh))
				{
					//1) 축에 있는가
					if(isSnap)
					{
						//=> 위치만 보정한다.
						mirrorVert_Next = addedVert;
						mirrorVert_Next._pos = GetAxisPosToSnap(addedVert._pos, mesh);
					}
					else
					{
						//=> 위치 보정 없이 그냥 선택한다.
						mirrorVert_Next = addedVert;
					}
				}
				else
				{
					//2) 축 바깥이라면 미러 위치를 계산한다.
					Vector2 mirrorPos = GetMirrorPosByMesh(addedVert._pos, mesh);
					
					//근처에 Vertex가 있는가 + Prev와 다른 Vertex인가
					apVertex nearestVert = FindNearestVertex(mirrorPos, mesh, clickableOffset);
					if(nearestVert != null && mirrorVert_Prev != nearestVert)
					{
						//=> 이걸 선택하고 위치만 보정한다.
						mirrorVert_Next = nearestVert;
						mirrorVert_Next._pos = mirrorPos;
					}
					else
					{
						//=> 새로 생성하자
						if (isAddVertexWithSplit)
						{
							//만약 원 소스가 Edge를 Split하고 Vertex를 만든 거라면,
							//Mirror도 주변의 Edge를 Split해야한다.
							apMeshEdge nearestEdge = GetMeshNearestEdge(mirrorPos, mesh, 3.0f);
							if(nearestEdge != null)
							{
								Vector2 splitPos = nearestEdge.GetNearestPosOnEdge(mirrorPos);
								if (Mathf.Abs(splitPos.x - nearestEdge._vert1._pos.x) < 1 && Mathf.Abs(splitPos.y - nearestEdge._vert1._pos.y) < 1)
								{
									//Vert1과 겹친다.
									mirrorVert_Next = nearestEdge._vert1;
								}
								else if (Mathf.Abs(splitPos.x - nearestEdge._vert2._pos.x) < 1 && Mathf.Abs(splitPos.y - nearestEdge._vert2._pos.y) < 1)
								{
									//Vert2와 겹친다.
									mirrorVert_Next = nearestEdge._vert2;
								}
								else
								{
									//겹치는게 없다.
									mirrorVert_Next = mesh.SplitEdge(nearestEdge, splitPos);
								}
							}
						}
						else
						{

							mirrorVert_Next = mesh.AddVertexAutoUV(mirrorPos);
						}
					}
				}
			}

			//if(!isAddEdge)
			//{
			//	Debug.LogError("Add Edge => False");
			//}
			if(isAddEdge && mirrorVert_Prev != null && mirrorVert_Next != null)
			{
				//Edge를 추가하자
				//단, 두개가 서로 Mirror된게 아니라면 생략
				if((mirrorVert_Prev == prevVert && mirrorVert_Next == addedVert)
					|| (mirrorVert_Next == prevVert && mirrorVert_Prev == addedVert))//<<또는 그 반대
				{
					//새로 만드는거 실패.
					//서로 교차되고 있었다.
					//Shift 키를 누른 상태라면 => Mirror 위치에 중점을 만들자.
					if(isShift && mirrorVert_Prev != mirrorVert_Next)
					{
						//Mirror의 양쪽에 위치한 경우
						bool isCounterSize = false;
						Vector2 centerPos = (mirrorVert_Prev._pos + mirrorVert_Next._pos) * 0.5f;
						if(mesh._isMirrorX)
						{
							if((mirrorVert_Prev._pos.x - mesh._mirrorAxis.x) * (mirrorVert_Next._pos.x - mesh._mirrorAxis.x) < 0.0f)
							{
								//축으로부터 X 변화량의 곱이 -1인 경우
								isCounterSize = true;
								centerPos.x = mesh._mirrorAxis.x;
							}
						}
						else
						{
							if((mirrorVert_Prev._pos.y - mesh._mirrorAxis.y) * (mirrorVert_Next._pos.y - mesh._mirrorAxis.y) < 0.0f)
							{
								//축으로부터 Y 변화량의 곱이 -1인 경우
								isCounterSize = true;
								centerPos.y = mesh._mirrorAxis.y;
							}
						}
						if (isCounterSize)
						{
							apMeshEdge existEdge = mesh._edges.Find(delegate (apMeshEdge a)
							{
								return a.IsSameEdge(mirrorVert_Prev, mirrorVert_Next);
							});

							if(existEdge != null)
							{
								//분할하자
								mesh.SplitEdge(existEdge, centerPos);
								//apVertex centerVert = mesh.AddVertexAutoUV(centerPos);
								//mesh.RemoveEdge(existEdge);

								//if (centerVert != null)
								//{
								//	mesh.MakeNewEdge(mirrorVert_Prev, centerVert, false);
								//	mesh.MakeNewEdge(mirrorVert_Next, centerVert, false);
								//}

								//Debug.Log("Split Edge");
							}
						}
					}
				}
				else
				{
					//새로운 Mirror Vertex를 만들자.
					mesh.MakeNewEdge(mirrorVert_Prev, mirrorVert_Next, isShift);

					//Debug.Log("Add Mirror Edge : " + isShift);
				}
			}

			if(mirrorVert_Prev != null)
			{
				mesh.RefreshVertexAutoUV(mirrorVert_Prev);
			}
			if(mirrorVert_Next != null)
			{
				mesh.RefreshVertexAutoUV(mirrorVert_Next);
			}
			

			ClearMovedVertex();
			
		}



		//----------------------------------------------------------------
		public void MoveMirrorVertex(apVertex movedVertex, Vector2 prevPos, Vector2 curPos, apMesh mesh)
		{
			if(movedVertex != _movedSrcVertex)
			{
				ClearMovedVertex();
				//새로 연결된 Mirror Vert를 찾자
				_movedSrcVertex = movedVertex;
				_movedMirrorVertex = FindNearestVertex(GetMirrorPosByMesh(prevPos, mesh), mesh, 3.0f, movedVertex);
			}
			 

			if(_movedMirrorVertex != null)
			{
				_movedMirrorVertex._pos = GetMirrorPosByMesh(curPos, mesh);
				mesh.RefreshVertexAutoUV(_movedMirrorVertex);
				
			}
		}


		public void RemoveMirrorVertex(apVertex removedVertex, apMesh mesh, bool isShift)
		{
			ClearMovedVertex();
			if(removedVertex == null)
			{
				return;
			}
			Vector2 mirrorPos = GetMirrorPosByMesh(removedVertex._pos, mesh);
			apVertex removableMirrorVert = FindNearestVertex(mirrorPos, mesh, 2.0f, removedVertex);//<<아주 좁은 범위
			if(removableMirrorVert != null)
			{
				mesh.RemoveVertex(removableMirrorVert, isShift);
			}
		}


		public void RemoveMirrorEdge(apMeshEdge removedEdge, apMesh mesh)
		{
			ClearMovedVertex();
			//두개의 Vertex => Mirror 위치 각자 계산 => mesh에서 검색
			if(removedEdge == null)
			{
				return;
			}
			apVertex srcVert1 = removedEdge._vert1;
			apVertex srcVert2 = removedEdge._vert2;
			if(srcVert1 == null || srcVert2 == null)
			{
				return;
			}

			Vector2 mirrorPos1 = GetMirrorPosByMesh(srcVert1._pos, mesh);
			Vector2 mirrorPos2 = GetMirrorPosByMesh(srcVert2._pos, mesh);

			apVertex mirrorVert1 = FindNearestVertex(mirrorPos1, mesh, 2.0f, srcVert1);
			apVertex mirrorVert2 = FindNearestVertex(mirrorPos2, mesh, 2.0f, srcVert2);

			if(mirrorVert1 == null || mirrorVert2 == null || mirrorVert1 == mirrorVert2
				|| (mirrorVert1 == srcVert1 && mirrorVert2 == srcVert2) 
				|| (mirrorVert1 == srcVert2 && mirrorVert2 == srcVert1)
				)
			{
				return;
			}

			//이제 대상이 되는 Edge를 찾자
			apMeshEdge mirrorEdge = mesh._edges.Find(delegate(apMeshEdge a)
			{
				return a.IsSameEdge(mirrorVert1, mirrorVert2);
			});

			if(mirrorEdge != null)
			{
				mesh.RemoveEdge(mirrorEdge);
			}
		}


		//-------------------------------------------------------------------------------------
		public apMeshEdge GetMeshNearestEdge(Vector2 pos, apMesh mesh, float offsetGL)
		{
			apMeshEdge curEdge = null;

			Vector2 posGL = apGL.World2GL(pos - mesh._offsetPos);
			
			Vector2 vPos1GL = Vector2.zero;
			Vector2 vPos2GL = Vector2.zero;
			float minX = 0.0f;
			float maxX = 0.0f;
			float minY = 0.0f;
			float maxY = 0.0f;
			float curDist = 0.0f;

			float minDist = 0.0f;
			apMeshEdge minEdge = null;

			//추가 v1.4.2 : AABB가 너무 타이트하게 들어가서 거리 비교가 아예 불가능한 버그 해결
			float aabbBias = (offsetGL * 1.5f) + 5.0f;

			for (int i = 0; i < mesh._edges.Count; i++)
			{
				curEdge = mesh._edges[i];

				if (curEdge._vert1 == null || curEdge._vert2 == null)
				{
					continue;
				}

				//기본 사각 범위안에 있는지 확인
				vPos1GL = apGL.World2GL(curEdge._vert1._pos - mesh._offsetPos);
				vPos2GL = apGL.World2GL(curEdge._vert2._pos - mesh._offsetPos);

				minX = Mathf.Min(vPos1GL.x, vPos2GL.x);
				maxX = Mathf.Max(vPos1GL.x, vPos2GL.x);
				minY = Mathf.Min(vPos1GL.y, vPos2GL.y);
				maxY = Mathf.Max(vPos1GL.y, vPos2GL.y);
				
				//이전 : 버그 (수직 수평선의 경우 X축 또는 Y축의 범위가 0에 수렴하여 OffsetGL과 비교하기도 전에 연산을 포기한다)
				//if(posGL.x < minX || maxX < posGL.x ||
				//	posGL.y < minY || maxY < posGL.y)
				//{
				//	continue;
				//}

				//수정 : 적절히 Bias를 둬서 AABB 체크가 빡세게 들어가지 않게 한다.
				if(posGL.x < (minX - aabbBias) || (maxX + aabbBias) < posGL.x ||
					posGL.y < (minY - aabbBias) || (maxY + aabbBias) < posGL.y)
				{
					continue;
				}
				
				curDist = apEditorUtil.DistanceFromLine(
					vPos1GL,
					vPos2GL,
					posGL);

				if (curDist < offsetGL)
				{
					if (minEdge == null || curDist < minDist)
					{
						minDist = curDist;
						minEdge = curEdge;
					}
				}

			}
			return minEdge;
		}

		//----------------------------------------------------------------
		public bool IsOnAxisByMesh(Vector2 pos, apMesh mesh)
		{
			if(mesh._isMirrorX)
			{
				return Mathf.Abs(pos.x - mesh._mirrorAxis.x) < _editor._meshTRSOption_MirrorOffset;
			}
			else
			{
				return Mathf.Abs(pos.y - mesh._mirrorAxis.y) < _editor._meshTRSOption_MirrorOffset;
			}
		}

		public Vector2 GetAxisPosToSnap(Vector2 pos, apMesh mesh)
		{
			if(mesh._isMirrorX)
			{
				pos.x = mesh._mirrorAxis.x;
			}
			else
			{
				pos.y = mesh._mirrorAxis.y;
			}
			return pos;
		}

		public Vector2 GetMirrorPosByMesh(Vector2 pos, apMesh mesh)
		{
			if(mesh._isMirrorX)
			{
				pos.x = mesh._mirrorAxis.x - (pos.x - mesh._mirrorAxis.x);
			}
			else
			{
				pos.y = mesh._mirrorAxis.y - (pos.y - mesh._mirrorAxis.y);
			}
			return pos;
		}

		public apVertex FindNearestVertex(Vector2 pos, apMesh mesh, float offset, apVertex exceptVert = null)
		{
			apVertex curVert = null;
			apVertex minVert = null;
			float curDist = 0.0f;
			float minDist = 0.0f;
			
			for (int iVert = 0; iVert < mesh._vertexData.Count; iVert++)
			{
				curVert = mesh._vertexData[iVert];
				if(exceptVert == curVert)
				{
					continue;
				}
				if (Mathf.Abs(curVert._pos.x - pos.x) < offset
						&& Mathf.Abs(curVert._pos.y - pos.y) < offset)
				{
					curDist = Mathf.Abs(curVert._pos.x - pos.x) + Mathf.Abs(curVert._pos.y - pos.y);//MHT Dist
					//선택할 수 있다.
					if(minVert == null || curDist < minDist)
					{
						minVert = curVert;
						minDist = curDist;
					}
					
				}
			}
			return minVert;
		}
	}
}