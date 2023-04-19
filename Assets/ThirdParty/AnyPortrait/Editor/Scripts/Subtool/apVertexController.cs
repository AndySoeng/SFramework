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

	public class apVertexController
	{
		// Members
		//-----------------------------------------------
		private apMesh _mesh = null;
		private apVertex _curVertex = null;
		private List<apVertex> _curVertices = new List<apVertex>();
		private apMeshPolygon _curPolygon = null;

		//linke 연결 용
		private apVertex _nextVertex = null;
		private bool _isTmpEdgeWire = false;
		private bool _isTmpEdgeWireCrossEdge = false;
		private bool _isTmpEdgeWireCrossEdge_Multiple = false;
		private Vector2 _tmpEdgeWire_MousePos = Vector2.zero;
		private Vector2 _tmpEdgeWireCrossPoint = Vector2.zero;
		private List<Vector2> _tmpEdgeWireMultipleCrossPoints = new List<Vector2>();
		private bool _isTmpEdgeWire_SnapToEdge = false;
		private Vector2 _tmpEdgeWire_SnapToEdge = Vector2.zero;
		private bool _isTmpEdgeWire_SnapCheck_PrevShift = false;
		private bool _isTmpEdgeWire_SnapCheck_PrevCtrl = false;
		private Vector2 _isTmpEdgeWire_SnapCheck_MousePos = Vector2.zero;


		public apMesh Mesh { get { return _mesh; } }
		public apVertex Vertex { get { return _curVertex; } }
		public List<apVertex> Vertices {  get { return _curVertices; } }
		public apMeshPolygon Polygon { get { return _curPolygon; } }

		public bool IsTmpEdgeWire { get { return _isTmpEdgeWire; } }
		public Vector2 TmpEdgeWirePos { get { return _tmpEdgeWire_MousePos; } }
		public apVertex LinkedNextVertex { get { return _nextVertex; } }

		public bool IsTmpSnapToEdge {  get {  return _isTmpEdgeWire_SnapToEdge; } }
		public Vector2 TmpSnapToEdgePos {  get {  return _tmpEdgeWire_SnapToEdge; } }

		private apBone _curBone = null;
		public apBone Bone { get { return _curBone; } }

		// Init
		//-----------------------------------------------
		public apVertexController()
		{
			Init();
		}

		public void Init()
		{
			_mesh = null;
			_curVertex = null;
			_curVertices.Clear();
			_curPolygon = null;

			_nextVertex = null;
			_isTmpEdgeWire = false;
			_tmpEdgeWire_MousePos = Vector2.zero;

			_isTmpEdgeWire_SnapToEdge = false;
			_tmpEdgeWire_SnapToEdge = Vector2.zero;
		}

		// Functions
		//-----------------------------------------------
		public void SetMesh(apMesh mesh)
		{
			if (_mesh != mesh)
			{
				_mesh = mesh;
				_curVertex = null;
				_curVertices.Clear();
				_curPolygon = null;

				UnselectNextVertex();

				_isTmpEdgeWire = false;
				_isTmpEdgeWireCrossEdge = false;
				_isTmpEdgeWireCrossEdge_Multiple = false;
				_tmpEdgeWire_MousePos = Vector2.zero;
				_tmpEdgeWireMultipleCrossPoints.Clear();

				_isTmpEdgeWire_SnapToEdge = false;
				_tmpEdgeWire_SnapToEdge = Vector2.zero;
			}
		}

		public void SelectVertex(apVertex vertex)
		{
			_curVertex = vertex;
			_curVertices.Clear();
			_curVertices.Add(_curVertex);//한개만 선택

			UnselectNextVertex();

			_isTmpEdgeWire = false;
			_isTmpEdgeWireCrossEdge = false;
			//_tmpEdgeWire_MousePos = Vector2.zero;
			_isTmpEdgeWireCrossEdge_Multiple = false;
			_tmpEdgeWireMultipleCrossPoints.Clear();

			//_isTmpEdgeWire_SnapToEdge = false;
			//_tmpEdgeWire_SnapToEdge = Vector2.zero;

			_curPolygon = null;
		}

		public void SelectVertices(List<apVertex> vertices, apGizmos.SELECT_TYPE selectType)
		{
			if(selectType == apGizmos.SELECT_TYPE.New)
			{
				_curVertex = null;
				_curVertices.Clear();
			}

			switch (selectType)
			{
				case apGizmos.SELECT_TYPE.Add:
				case apGizmos.SELECT_TYPE.New:

					for (int i = 0; i < vertices.Count; i++)
					{
						if(!_curVertices.Contains(vertices[i]))
						{
							_curVertices.Add(vertices[i]);
						}
					}
					break;

				case apGizmos.SELECT_TYPE.Subtract:
					for (int i = 0; i < vertices.Count; i++)
					{
						if(_curVertices.Contains(vertices[i]))
						{
							_curVertices.Remove(vertices[i]);
						}
					}
					break;
			}

			if (_curVertices.Count > 0)
			{
				if(_curVertex == null || !_curVertices.Contains(_curVertex))
				{
					_curVertex = _curVertices[0];
				}
			}
			else
			{
				_curVertex = null;
			}



			UnselectNextVertex();

			_isTmpEdgeWire = false;
			_isTmpEdgeWireCrossEdge = false;
			//_tmpEdgeWire_MousePos = Vector2.zero;
			_isTmpEdgeWireCrossEdge_Multiple = false;
			_tmpEdgeWireMultipleCrossPoints.Clear();
			//_isTmpEdgeWire_SnapToEdge = false;
			//_tmpEdgeWire_SnapToEdge = Vector2.zero;

			_curPolygon = null;
		}


		public void UnselectVertex()
		{
			_curVertex = null;
			_curVertices.Clear();

			UnselectNextVertex();

			_isTmpEdgeWire = false;
			_isTmpEdgeWireCrossEdge = false;
			//_tmpEdgeWire_MousePos = Vector2.zero;
			_isTmpEdgeWireCrossEdge_Multiple = false;
			_tmpEdgeWireMultipleCrossPoints.Clear();
			//_isTmpEdgeWire_SnapToEdge = false;
			//_tmpEdgeWire_SnapToEdge = Vector2.zero;

			_curPolygon = null;
		}

		public void SelectPolygon(apMeshPolygon polygon)
		{
			_curPolygon = polygon;

			_curVertex = null;
			_curVertices.Clear();

			UnselectNextVertex();

			_isTmpEdgeWire = false;
			_isTmpEdgeWireCrossEdge = false;
			//_tmpEdgeWire_MousePos = Vector2.zero;
			_isTmpEdgeWireCrossEdge_Multiple = false;
			_tmpEdgeWireMultipleCrossPoints.Clear();
			_isTmpEdgeWire_SnapToEdge = false;
			_tmpEdgeWire_SnapToEdge = Vector2.zero;
		}



		public void UnselectNextVertex()
		{
			_nextVertex = null;
		}

		public void SelectNextVertex(apVertex vertex)
		{
			_nextVertex = vertex;

		}

		/// <summary>
		/// MeshEdit중 "이전 위치 > 마우스 위치"로의 임시 Edge를 생성하여 GUI에 반영할 수 있도록 하는 함수.
		/// 교차 처리도 여기서 한다.
		/// Shift를 누르면 교차 처리가 Vertex 생성으로 이루어지므로 여러개의 교차점을 다른 색으로 표시한다.
		/// </summary>
		/// <param name="mousePos"></param>
		/// <param name="isShift"></param>
		public void UpdateEdgeWire(Vector2 mousePos, bool isShift, bool isCtrl)
		{
			if (!_isTmpEdgeWire || Vector2.SqrMagnitude(_tmpEdgeWire_MousePos - mousePos) > 1.0f)
			{
				//처음 Update를 하거나 마우스가 움직였을때 체크

				_isTmpEdgeWire_SnapToEdge = false;
				//_tmpEdgeWire_SnapToEdge = Vector2.zero;

				if (_curVertex == null)
				{
					_isTmpEdgeWireCrossEdge = false;
					if (_isTmpEdgeWireCrossEdge_Multiple)
					{
						_tmpEdgeWireMultipleCrossPoints.Clear();
					}
					_isTmpEdgeWireCrossEdge_Multiple = false;


				}
				else if (_mesh != null)
				{
					Vector2 mousePosW = apGL.GL2World(mousePos);
					Vector2 mousePosLocal2 = _mesh.Matrix_VertToLocal.inverse.MultiplyPoint(mousePosW);

					if (isShift)
					{
						//Shift가 눌리면 여러개의 VertexPos를 가져와야 한다.
						_isTmpEdgeWireCrossEdge = false;
						_tmpEdgeWireCrossPoint = Vector2.zero;

						_isTmpEdgeWireCrossEdge_Multiple = true;
						//_tmpEdgeWireMultipleCrossPoints.Clear();

						IsAnyCrossEdgeMultiple(_curVertex, null, mousePosLocal2);
						if (_tmpEdgeWireMultipleCrossPoints.Count == 0)
						{
							_isTmpEdgeWireCrossEdge_Multiple = false;
						}
					}
					else
					{
						if (_isTmpEdgeWireCrossEdge_Multiple)
						{
							_tmpEdgeWireMultipleCrossPoints.Clear();
						}
						_isTmpEdgeWireCrossEdge_Multiple = false;


						_isTmpEdgeWireCrossEdge = IsAnyCrossEdge(_curVertex, null, mousePosLocal2);
						if (_isTmpEdgeWireCrossEdge)
						{
							_tmpEdgeWireCrossPoint = _crossPoint._pos;

							//시작점이나 끝점에 가까우면 무시한다.
							if (Vector2.Distance(_curVertex._pos, _tmpEdgeWireCrossPoint) < 4.0f
								|| Vector2.Distance(mousePosLocal2, _tmpEdgeWireCrossPoint) < 4.0f)
							{
								_isTmpEdgeWireCrossEdge = false;
								_tmpEdgeWireCrossPoint = Vector2.zero;
							}
						}
					}
				}

				//if (isShift && _mesh != null && !isCtrl && _curVertex == null)
				//{
				//	//추가
				//	//가장 가까운 Edge의 점을 찾는다.
				//	apMeshEdge nearestEdge = GetMeshNearestEdge(mousePos, _mesh, 3.0f);//<<기본적인 5가 아닌 3이다. 제한적임
				//	if(nearestEdge != null)
				//	{
				//		_isTmpEdgeWire_SnapToEdge = true;
				//		_tmpEdgeWire_SnapToEdge = nearestEdge.GetNearestPosOnEdge(apGL.GL2World(mousePos) + _mesh._offsetPos);
				//	}
				//}

				_tmpEdgeWire_MousePos = mousePos;
			}

			_isTmpEdgeWire = true;
			
		}

		

		public void StopEdgeWire()
		{
			_isTmpEdgeWire = false;
			_isTmpEdgeWireCrossEdge = false;

			//_tmpEdgeWire_MousePos = Vector2.zero;


			if (_isTmpEdgeWireCrossEdge_Multiple)
			{
				_tmpEdgeWireMultipleCrossPoints.Clear();
			}
			_isTmpEdgeWireCrossEdge_Multiple = false;
			//_isTmpEdgeWire_SnapToEdge = false;
			//_tmpEdgeWire_SnapToEdge = Vector2.zero;
		}

		public void UpdateSnapEdgeGUIOnly(Vector2 mousePos, bool isShift, bool isCtrl, bool isPressed, float snappableDistGL)
		{
			if (Vector2.SqrMagnitude(_isTmpEdgeWire_SnapCheck_MousePos - mousePos) > 1.0f
				|| _isTmpEdgeWire_SnapCheck_PrevShift != isShift
				|| _isTmpEdgeWire_SnapCheck_PrevCtrl != isCtrl)
			{
				
				_isTmpEdgeWire_SnapToEdge = false;
				_tmpEdgeWire_SnapToEdge = Vector2.zero;

				//if (isShift && _mesh != null && !isCtrl && _curVertex == null)
				if (isShift && _mesh != null && !isCtrl && !isPressed)
				{
					//스냅이 될만한 마우스 위치에서 가장 가까운 Edge의 점을 찾는다.

					apMeshEdge nearestEdge = GetMeshNearestEdge(mousePos, _mesh, snappableDistGL);//<<기본적인 클릭 범위(5)보다 좁은 3이다. Snap 미리보기가 
					if (nearestEdge != null)
					{
						_isTmpEdgeWire_SnapToEdge = true;
						_tmpEdgeWire_SnapToEdge = nearestEdge.GetNearestPosOnEdge(apGL.GL2World(mousePos) + _mesh._offsetPos);
					}
				}

				//Debug.Log("UpdateSnapEdgeGUIOnly >> " + _isTmpEdgeWire_SnapToEdge);

				_isTmpEdgeWire_SnapCheck_MousePos = mousePos;
				_isTmpEdgeWire_SnapCheck_PrevShift = isShift;
				_isTmpEdgeWire_SnapCheck_PrevCtrl = isCtrl;
			}
			//_tmpEdgeWire_MousePos = mousePos;
		}

		public bool IsEdgeWireRenderable()
		{
			return _curVertex != null && _isTmpEdgeWire;
		}

		public bool IsEdgeWireCross()
		{
			return _curVertex != null && _isTmpEdgeWire && _isTmpEdgeWireCrossEdge;
		}
		public Vector2 EdgeWireCrossPoint()
		{
			return _tmpEdgeWireCrossPoint;
		}
		public bool IsEdgeWireMultipleCross()
		{
			return _curVertex != null && _isTmpEdgeWire && _isTmpEdgeWireCrossEdge_Multiple;
		}
		public List<Vector2> EdgeWireMultipleCrossPoints()
		{
			return _tmpEdgeWireMultipleCrossPoints;
		}


		private bool IsAnyCrossEdge(apVertex vert1, apVertex vert2, Vector2 vert2PosIfNull)
		{
			if (_mesh == null)
			{
				return false;
			}
			int nVert = _mesh._vertexData.Count;
			int nEdge = _mesh._edges.Count;

			Vector2 vert1Local = vert1._pos;
			Vector2 vert2Local = vert2PosIfNull;
			if (vert2 != null)
			{
				vert2Local = vert2._pos;
			}


			apMeshEdge edge = null;
			apVertex edgeVert1 = null;
			apVertex edgeVert2 = null;
			//Vector2 crossPos = Vector2.zero;

			for (int i = 0; i < nEdge; i++)
			{
				edge = _mesh._edges[i];
				edgeVert1 = edge._vert1;
				edgeVert2 = edge._vert2;

				//if (vert2 != null)
				//{
				//	if ((vert1 == edgeVert1 && vert2 == edgeVert2) ||
				//		(vert1 == edgeVert2 && vert2 == edgeVert1))
				//	{
				//		//겹치면 : 같은 선분이 있네염
				//		return true;
				//	}
				//}
				//else
				//{
				//	if(vert1 == edgeVert1 || vert1 == edgeVert2)
				//	{
				//		//하나만 겹쳐도 해당 Edge와 교차되지는 않는다.
				//		return false;
				//	}
				//}

				CheckLineIntersetion(edgeVert1._pos, edgeVert2._pos, vert1Local, vert2Local);
				if (_crossPoint._isIntersetion)
				{
					//교차되었다.
					//단, 교차 포인트가 어느 점 근처라면 SameLine이 아닌 이상 일단 넘어간다.
					if (_crossPoint._isAnyPointSame)
					{
						if (_crossPoint._isSameLine)
						{
							return true;//<아예 겹친다.
						}
						//패스
					}
					else
					{
						//교차점이 선분 내부에 있다.
						//만약 교차점이 어느 다른 점과 가까이 있다면 일단 무시할 수 있다.
						if (_nextVertex != null)
						{
							if (Vector2.Distance(_nextVertex._pos, _crossPoint._pos) < nearBias)
							{
								continue;
							}
						}

						//교차점이 목표한 선분 내부에 없다면 패스
						Vector2 vec2Cross = _crossPoint._pos - vert1Local;
						Vector2 vec2Req = vert2Local - vert1Local;
						if (vec2Req.sqrMagnitude > 1.0f && vec2Cross.sqrMagnitude > 1.0f)
						{
							float dotProduct = Vector2.Dot(vec2Req, vec2Cross);
							//if(dotProduct < 0.0f || dotProduct > 1.0f)
							//{
							//	continue;//벡터 안에 있는게 아닌것 같다.
							//}

							float angle = Vector2.Angle(vec2Req, vec2Cross);

							if (dotProduct < 0.0f || angle > 30.0f)
							{
								//Debug.Log("Cross [Dot : " + dotProduct + " / Angle : " + angle + "]");
								continue;
							}
							if (dotProduct < 0.0f)
							{
								//Debug.LogError("Cross Out [Dot : " + dotProduct + "]");
								continue;
							}
							if (vec2Cross.sqrMagnitude > vec2Req.sqrMagnitude)
							{
								//Debug.LogError("Cross Out [Length Over : " + vec2Req.magnitude + " >> " + vec2Cross.magnitude + "]");
								continue;
							}

							//if(angle > 5.0f)
							//{
							//	//각도도 다르네요...
							//	continue;
							//}
						}

						return true;
					}
				}
				else
				{
					//교차되지 않았다.
					//다음 계산
				}
			}

			return false;
		}



		private void IsAnyCrossEdgeMultiple(apVertex vert1, apVertex vert2, Vector2 vert2PosIfNull)
		{
			if (_mesh == null)
			{
				return;
			}
			int nVert = _mesh._vertexData.Count;
			int nEdge = _mesh._edges.Count;

			Vector2 vert1Local = vert1._pos;
			Vector2 vert2Local = vert2PosIfNull;
			if (vert2 != null)
			{
				vert2Local = vert2._pos;
			}

			apMeshEdge edge = null;
			apVertex edgeVert1 = null;
			apVertex edgeVert2 = null;
			//Vector2 crossPos = Vector2.zero;

			_tmpEdgeWireMultipleCrossPoints.Clear();

			for (int i = 0; i < nEdge; i++)
			{
				edge = _mesh._edges[i];
				edgeVert1 = edge._vert1;
				edgeVert2 = edge._vert2;

			

				CheckLineIntersetion(edgeVert1._pos, edgeVert2._pos, vert1Local, vert2Local);
				if (_crossPoint._isIntersetion)
				{
					//교차되었다.
					//단, 교차 포인트가 어느 점 근처라면 SameLine이 아닌 이상 일단 넘어간다.
					if (_crossPoint._isAnyPointSame)
					{
						if (_crossPoint._isSameLine)
						{
							//return true;//<아예 겹친다.
							if (Vector2.Distance(_crossPoint._pos, vert1Local) > 4.0f &&
								Vector2.Distance(_crossPoint._pos, vert2Local) > 4.0f)
							{
								_tmpEdgeWireMultipleCrossPoints.Add(_crossPoint._pos);
							}
							//return;
						}
						//패스
					}
					else
					{
						//교차점이 선분 내부에 있다.
						//만약 교차점이 어느 다른 점과 가까이 있다면 일단 무시할 수 있다.
						if (_nextVertex != null)
						{
							if (Vector2.Distance(_nextVertex._pos, _crossPoint._pos) < nearBias)
							{
								continue;
							}
						}

						//교차점이 목표한 선분 내부에 없다면 패스
						Vector2 vec2Cross = _crossPoint._pos - vert1Local;
						Vector2 vec2Req = vert2Local - vert1Local;
						if (vec2Req.sqrMagnitude > 1.0f && vec2Cross.sqrMagnitude > 1.0f)
						{
							float dotProduct = Vector2.Dot(vec2Req, vec2Cross);
							//if(dotProduct < 0.0f || dotProduct > 1.0f)
							//{
							//	continue;//벡터 안에 있는게 아닌것 같다.
							//}

							//float angle = Vector2.Angle(vec2Req, vec2Cross);

							//if (dotProduct < 0.0f || angle > 5.0f)
							//{
							//	Debug.Log("Cross [Dot : " + dotProduct + " / Angle : " + angle + "]");
							//}
							if (dotProduct < 0.0f)
							{
								//Debug.LogError("Cross Out [Dot : " + dotProduct + "]");
								continue;
							}
							if (vec2Cross.sqrMagnitude > vec2Req.sqrMagnitude)
							{
								//Debug.LogError("Cross Out [Length Over : " + vec2Req.magnitude + " >> " + vec2Cross.magnitude + "]");
								continue;
							}

							//if(angle > 5.0f)
							//{
							//	//각도도 다르네요...
							//	continue;
							//}
						}

						if (Vector2.Distance(_crossPoint._pos, vert1Local) > 4.0f &&
								Vector2.Distance(_crossPoint._pos, vert2Local) > 4.0f)
						{
							_tmpEdgeWireMultipleCrossPoints.Add(_crossPoint._pos);
						}
						//return true;
					}
				}
				else
				{
					//교차되지 않았다.
					//다음 계산
				}
			}

			//return false;
		}



		private apMeshEdge GetMeshNearestEdge(Vector2 posGL, apMesh mesh, float offsetGL)
		{
			apMeshEdge curEdge = null;

			//Vector2 posW = apGL.GL2World(posGL) + mesh._offsetPos;
			
			Vector2 vPos1GL = Vector2.zero;
			Vector2 vPos2GL = Vector2.zero;
			float minX = 0.0f;
			float maxX = 0.0f;
			float minY = 0.0f;
			float maxY = 0.0f;
			float curDist = 0.0f;

			float minDist = 0.0f;
			apMeshEdge minEdge = null;

			int nEdges = mesh._edges != null ? mesh._edges.Count : 0;
			if(nEdges == 0)
			{
				return null;
			}

			//추가 v1.4.2 : AABB가 너무 타이트하게 들어가서 거리 비교가 아예 불가능한 버그 해결
			float aabbBias = (offsetGL * 1.5f) + 5.0f;

			for (int i = 0; i < nEdges; i++)
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





		private const float zeroBias = 1f;
		private const float nearBias = 8.0f;

		public class CrossPoint
		{
			public bool _isIntersetion = false;
			public bool _isSameLine = false;//<<교차점이 아니라 일정 구간 아예 겹친다.
			public Vector2 _pos = Vector2.zero;
			public bool _isAnyPointSame = false;
			public CrossPoint()
			{
				Init();
			}

			public void Init()
			{
				_isIntersetion = false;
				_isSameLine = false;
				_isAnyPointSame = false;
				_pos = Vector2.zero;
			}
		}
		private CrossPoint _crossPoint = new CrossPoint();


		private void CheckLineIntersetion(Vector2 edge1A, Vector2 edge1B, Vector2 edge2A, Vector2 edge2B)
		{
			_crossPoint.Init();

			//만약 어떤 점이 겹친 상태라면 일단 겹친 점에 대한 정보를 넣어준다.
			if (Vector2.Distance(edge1A, edge2A) < zeroBias || Vector2.Distance(edge1A, edge2B) < zeroBias)
			{
				_crossPoint._isAnyPointSame = true;
				_crossPoint._pos = edge1A;
			}
			else if (Vector2.Distance(edge1B, edge2A) < zeroBias || Vector2.Distance(edge1B, edge2B) < zeroBias)
			{
				_crossPoint._isAnyPointSame = true;
				_crossPoint._pos = edge1B;
			}


			float dX_1 = edge1B.x - edge1A.x;
			float dY_1 = edge1B.y - edge1A.y;
			float dX_2 = edge2B.x - edge2A.x;
			float dY_2 = edge2B.y - edge2A.y;

			float a1 = 0.0f;
			float a2 = 0.0f;
			float b1 = 0.0f;
			float b2 = 0.0f;

			float x1_Min = Mathf.Min(edge1A.x, edge1B.x);
			float x1_Max = Mathf.Max(edge1A.x, edge1B.x);
			float y1_Min = Mathf.Min(edge1A.y, edge1B.y);
			float y1_Max = Mathf.Max(edge1A.y, edge1B.y);

			float x2_Min = Mathf.Min(edge2A.x, edge2B.x);
			float x2_Max = Mathf.Max(edge2A.x, edge2B.x);
			float y2_Min = Mathf.Min(edge2A.y, edge2B.y);
			float y2_Max = Mathf.Max(edge2A.y, edge2B.y);



			//이전 방식

			//수직/수평에 따라 다르게 처리
			//if (Mathf.Abs(dX_1) < zeroBias * 0.01f)
			if (Mathf.Approximately(dX_1, 0.0f))
			{
				//Line 1이 수직일 때

				float X1 = (edge1A.x + edge1B.x) * 0.5f;

				//if (Mathf.Abs(dX_2) < zeroBias * 0.01f)
				if (Mathf.Approximately(dX_2, 0.0f))
				{
					//Line 2도 같이 수직일 때
					//수직 + 수직
					//x가 같으면 [겹침] (y범위 비교)
					//그 외에는 [평행]

					float X2 = (edge2A.x + edge2B.x) * 0.5f;

					//if (Mathf.Abs(X1 - X2) < zeroBias * 0.01f)
					if (Mathf.Approximately(X1, X2))
					{
						//Y 영역이 겹치는가 [Y영역이 겹치면 겹침]
						if (IsAreaIntersection(y1_Min, y1_Max, y2_Min, y2_Max))
						{
							_crossPoint._isIntersetion = true;
							_crossPoint._isSameLine = true;//<<이건 교차점 대신 아예 겹친다.

							//이전
							//_crossPoint._pos = edge1A;

							//변경 21.3.14 : Y는 겹치는 중점을 설정
							float commonY_Min = Mathf.Max(y1_Min, y2_Min);
							float commonY_Max = Mathf.Min(y1_Max, y2_Max);
							float commonY_Avg = commonY_Min * 0.5f + commonY_Max * 0.5f;

							_crossPoint._pos = new Vector2(X1 * 0.5f + X2 * 0.5f, commonY_Avg);
							return;
						}
					}
				}
				else if (Mathf.Approximately(dY_2, 0.0f))
				{
					//Line2가 수평일 때
					float Y2 = (edge2A.y + edge2B.y) * 0.5f;

					//서로가 범위 안에 들어가야 한다.
					if (y1_Min <= Y2 && Y2 <= y1_Max
						&& x2_Min <= X1 && X1 <= x2_Max)
					{
						//[교차] : 수직1 + 수평2
						_crossPoint._isIntersetion = true;
						_crossPoint._pos = new Vector2(X1, Y2);
						return;
					}
				}
				else
				{
					//Line 2는 수평이나 기울기가 있을 때
					//Line1의 x 범위에서 y 안에 들면 [교차]
					//Line1의 x 범위 밖이거나 y 범위 밖에 있으면 [교차하지 않음]
					if (x2_Min <= X1 && X1 <= x2_Max)
					{
						a2 = dY_2 / dX_2;
						b2 = edge2A.y - edge2A.x * a2;

						float Yresult = a2 * X1 + b2;
						if (y1_Min <= Yresult && Yresult <= y1_Max)
						{
							//[교차]
							_crossPoint._isIntersetion = true;
							_crossPoint._pos = new Vector2(X1, Yresult);
							return;
						}

					}
				}
			}
			else if (Mathf.Approximately(dY_1, 0.0f))
			{
				//Line 1이 수평일 때
				float Y1 = (edge1A.y + edge1B.y) * 0.5f;
				if (Mathf.Approximately(dX_2, 0.0f))
				{
					//Line 2가 수직일 때
					//수평 + 수직
					//교차점 비교
					float X2 = (edge2A.x + edge2B.x) * 0.5f;

					if (y2_Min <= Y1 && Y1 <= y2_Max
						&& x1_Min <= X2 && X2 <= x1_Max)
					{
						//[교차] : 수평1 + 수직2
						_crossPoint._isIntersetion = true;
						_crossPoint._pos = new Vector2(X2, Y1);
						return;
					}
				}
				else if (Mathf.Approximately(dY_2, 0.0f))
				{
					//Line 2가 수평일 때
					//수평 + 수평
					//Y가 같고 X 범위가 겹쳐야 함 Same
					float Y2 = (edge2A.y + edge2B.y) * 0.5f;

					if (Mathf.Approximately(Y1, Y2))
					{
						if (IsAreaIntersection(x1_Min, x1_Max, x2_Min, x2_Max))
						{
							//[겹침] : 수평1 + 수평2
							_crossPoint._isIntersetion = true;
							_crossPoint._isSameLine = true;//<<이건 교차점 대신 아예 겹친다.
							_crossPoint._pos = (edge1A + edge1B) * 0.5f;
							return;
						}
					}
				}
				else
				{
					//Line 2가 기울기가 있을 때
					//범위와 교점 체크
					//수평 + 기울기
					a1 = 0.0f;
					b1 = edge1A.y;

					a2 = dY_2 / dX_2;
					b2 = edge2A.y - edge2A.x * a2;

					float XResult = (b1 - b2) / a2;

					if (x1_Min <= XResult && XResult <= x1_Max
						&& x2_Min <= XResult && XResult <= x2_Max)
					{
						//Line 1, 2의 X 범위 안에 들어간다.
						_crossPoint._isIntersetion = true;
						_crossPoint._pos = new Vector2(XResult, b1);
						return;
					}
				}
			}
			else
			{
				//Line 1이 기울기가 있을 때
				a1 = dY_1 / dX_1;
				b1 = edge1A.y - edge1A.x * a1;

				//if (Mathf.Abs(dX_2) < zeroBias * 0.01f)
				if (Mathf.Approximately(dX_2, 0.0f))
				{
					//Line 2가 수직일 때
					//Line2를 기준으로 x, y범위 비교후 Y 체크 [교차]
					//범위 밖이면 [교차하지 않음]

					float X2 = (edge2A.x + edge2B.x) * 0.5f;

					if (x1_Min <= X2 && X2 <= x1_Max)
					{
						float Yresult = a1 * X2 + b1;
						if (y2_Min <= Yresult && Yresult <= y2_Max)
						{
							//[교차]
							_crossPoint._isIntersetion = true;
							_crossPoint._pos = new Vector2(X2, Yresult);
							return;
						}
					}
				}
				else if (Mathf.Approximately(dY_2, 0.0f))
				{
					//Line 2가 수평일 때
					//기울기 + 수평
					a2 = 0.0f;
					b2 = edge2A.y;

					float XResult = (b2 - b1) / a1;

					if (x1_Min <= XResult && XResult <= x1_Max
						&& x2_Min <= XResult && XResult <= x2_Max)
					{
						//Line 1, 2의 X 범위 안에 들어간다.
						_crossPoint._isIntersetion = true;
						_crossPoint._pos = new Vector2(XResult, b2);
						return;
					}
				}
				else
				{
					//Line 2는 수평이나 기울기가 있을 때
					//X 범위 비교후
					//대입법 이용하여 체크하면 [교차]

					if (IsAreaIntersection(x1_Min, x1_Max, x2_Min, x2_Max))
					{
						a1 = dY_1 / dX_1;
						b1 = edge1A.y - edge1A.x * a1;

						a2 = dY_2 / dX_2;
						b2 = edge2A.y - edge2A.x * a2;

						float Yparam1 = a2 - a1;
						float Yparam2 = (a2 * b1) - (a1 * b2);

						//if (Mathf.Abs(Yparam1) < zeroBias * 0.01f)
						if (Mathf.Approximately(Yparam1, 0.0f))
						{
							//기울기가 같을때
							//b도 같아야한다.
							//if (Mathf.Abs(Yparam2) < zeroBias * 0.01f)
							if (Mathf.Approximately(b1, b2))
							{
								//[일치]
								_crossPoint._isIntersetion = true;
								_crossPoint._isSameLine = true;
								_crossPoint._pos = edge1A;
								return;
							}
						}
						else
						{
							//기울기가 다를때
							float Yresult = Yparam2 / Yparam1;

							//교차점의 위치를 확인한다.
							if (y1_Min <= Yresult && Yresult <= y1_Max &&
								y2_Min <= Yresult && Yresult <= y2_Max)
							{
								float Xresult = (Yresult - b1) / a1;
								_crossPoint._isIntersetion = true;
								_crossPoint._pos = new Vector2(Xresult, Yresult);
								return;
							}
						}
					}
				}
			}
			
		}

		private bool IsAreaIntersection(float area1Min, float area1Max, float area2Min, float area2Max)
		{
			//[ 1 ] .. [ 2 ] 이거나 [ 2 ] .. [ 1 ]으로 서로 겹쳐지지 않을 때
			if (area1Max < area2Min || area2Max < area1Min)
			{
				//
				return false;
			}
			return true;
		}
	}
}