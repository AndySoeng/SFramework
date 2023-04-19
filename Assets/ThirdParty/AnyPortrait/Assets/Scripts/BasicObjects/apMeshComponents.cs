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

	// Mesh에 들어가는 하위 요소들 
	[Serializable]
	public class apMeshEdge
	{
		//변경 : Edge에서 Vertex는 외부에 저장되므로,
		//여기서는 연결만 한다.

		public int _vertID_1 = -1;
		public int _vertID_2 = -1;

		[NonSerialized]
		public apVertex _vert1 = null;

		[NonSerialized]
		public apVertex _vert2 = null;

		[SerializeField]
		public bool _isHidden = false;

		[SerializeField, HideInInspector]
		public bool _isOutline = false;

		[NonSerialized, HideInInspector]
		public int _nTri = 0;


		/// <summary>
		/// 백업 파싱용 생성자. 그 외에는 호출하지 않도록 한다.
		/// </summary>
		public apMeshEdge()
		{

		}


		public apMeshEdge(apVertex vert1, apVertex vert2)
		{
			_vertID_1 = vert1._uniqueID;
			_vertID_2 = vert2._uniqueID;
			_vert1 = vert1;
			_vert2 = vert2;
			_isHidden = false;
			_isOutline = false;
		}

		public void Link(apVertex vert1, apVertex vert2)
		{
			_vert1 = vert1;
			_vert2 = vert2;
		}

		public bool IsSameEdge(apVertex vert1, apVertex vert2)
		{
			return (_vert1 == vert1 && _vert2 == vert2)
				|| (_vert1 == vert2 && _vert2 == vert1);
		}

		public bool IsSameEdge(int vertUniqueID1, int vertUniqueID2)
		{
			return (_vertID_1 == vertUniqueID1 && _vertID_2 == vertUniqueID2)
				|| (_vertID_1 == vertUniqueID2 && _vertID_2 == vertUniqueID1);
		}

		public bool IsSameEdge(apMeshEdge edge)
		{
			return IsSameEdge(edge._vert1, edge._vert2);
		}

		public bool IsLinkedEdge(apMeshEdge edge)
		{
			bool isORCond = (_vert1 == edge._vert1 || _vert1 == edge._vert2) ||
							(_vert2 == edge._vert1 || _vert2 == edge._vert2);

			return isORCond && !IsSameEdge(edge);//
		}


		public static bool IsEdgeCross(apMeshEdge edge1, apMeshEdge edge2)
		{
			return IsEdgeCross(edge1._vert1, edge1._vert2, edge2._vert1, edge2._vert2);
		}

		public static bool IsEdgeCross(apVertex vert1_A, apVertex vert1_B, apVertex vert2_A, apVertex vert2_B)
		{
			Vector2 v1_A = vert1_A._pos;
			Vector2 v1_B = vert1_B._pos;
			Vector2 v2_A = vert2_A._pos;
			Vector2 v2_B = vert2_B._pos;

			

			//return doIntersect;

			// Get A,B,C of first line - points : ps1 to pe1
			float A1 = v1_B.y - v1_A.y;
			float B1 = v1_A.x - v1_B.x;
			float C1 = A1 * v1_A.x + B1 * v1_A.y;

			// Get A,B,C of second line - points : ps2 to pe2
			float A2 = v2_B.y - v2_A.y;
			float B2 = v2_A.x - v2_B.x;
			float C2 = A2 * v2_A.x + B2 * v2_A.y;

			// Get delta and check if the lines are parallel
			float delta = A1 * B2 - A2 * B1;
			if (delta == 0)
			{
				return false;
			}

			Vector2 intersection = new Vector2((B2 * C1 - B1 * C2) / delta,
												(A1 * C2 - A2 * C1) / delta
											  );

			float p1 = (intersection - v1_A).magnitude / (v1_B - v1_A).magnitude;
			float p2 = (intersection - v2_A).magnitude / (v2_B - v2_A).magnitude;
			float bias = 0.3f;

			bool bHit_1 = false;
			bool bHit_2 = false;
			if (p1 < bias * 0.5f)
			{
				bHit_1 = false;
			}
			else if (p1 < bias)
			{
				//A랑 같으면 무시, 아니면 true
				if (vert1_A != vert2_A && vert1_A != vert2_B)
				{
					//다르다 -> 충돌
					bHit_1 = true;
				}
				else
				{
					//같다 -> 예외
					bHit_1 = false;
				}
			}
			else if (p1 < 1.0f - bias)
			{
				//충돌
				bHit_1 = true;
			}
			else if (p1 < 1.0f - bias * 0.5f)
			{
				//B랑 같으면 무시, 아니면 false
				if (vert1_B != vert2_A && vert1_B != vert2_B)
				{
					//다르다 -> 충돌
					bHit_1 = true;
				}
				else
				{
					//같다 -> 예외
					bHit_1 = false;
				}
			}
			else
			{
				bHit_1 = false;
			}




			if (p2 < bias * 0.5f)
			{
				bHit_2 = false;
			}
			else if (p2 < bias)
			{
				//A랑 같으면 무시, 아니면 false
				if (vert2_A != vert1_A && vert2_A != vert1_B)
				{
					// 충돌
					bHit_2 = true;
				}
				else
				{
					bHit_2 = false;
				}
			}
			else if (p2 < 1.0f - bias)
			{
				bHit_2 = true;
			}
			else if (p2 < 1.0f - bias * 0.5f)
			{
				//B랑 같으면 무시, 아니면 false
				if (vert2_B != vert1_A && vert2_B != vert1_B)
				{
					bHit_2 = true;
				}
				else
				{
					bHit_2 = false;
				}
			}
			else
			{
				bHit_2 = false;
			}

			if (bHit_1 && bHit_2)
			{
				//Debug.LogError("Line Cross : " + p1 + " / " + p2);
			}

			return bHit_1 && bHit_2;
		}

		/// <summary>
		/// IsEdgeCross의 "적당히" 더 검사를 하는 함수.
		/// 유사한 Edge가 있으면 충돌식이 False라 하더라도 True를 리턴한다.
		/// </summary>
		/// <param name="vert1_A"></param>
		/// <param name="vert1_B"></param>
		/// <param name="vert2_A"></param>
		/// <param name="vert2_B"></param>
		/// <returns></returns>
		public static bool IsEdgeCrossApprox(apVertex vert1_A, apVertex vert1_B, apVertex vert2_A, apVertex vert2_B)
		{
			if (IsEdgeCross(vert1_A, vert1_B, vert2_A, vert2_B))
			{
				return true;
			}
			Vector2 v1_A = vert1_A._pos;
			Vector2 v1_B = vert1_B._pos;
			Vector2 v2_A = vert2_A._pos;
			Vector2 v2_B = vert2_B._pos;

			float bias = 4f;
			float biasSqr = bias * bias;

			//1. [시작-끝] 좌표가 유사하면 교차
			if ((v1_A - v2_A).sqrMagnitude < biasSqr
				|| (v1_A - v2_B).sqrMagnitude < biasSqr
				|| (v1_B - v2_A).sqrMagnitude < biasSqr
				|| (v1_B - v2_B).sqrMagnitude < biasSqr
				)
			{
				return true;
			}

			//2. [내부의 점]이 서로 유사하다면 교차
			int nPoints_Line1 = (int)((v1_A - v1_B).magnitude / (bias)) + 1;
			int nPoints_Line2 = (int)((v2_A - v2_B).magnitude / (bias)) + 1;

			Vector2[] points_Line1 = new Vector2[nPoints_Line1];
			Vector2[] points_Line2 = new Vector2[nPoints_Line2];

			for (int i = 0; i < nPoints_Line1; i++)
			{
				points_Line1[i] = v1_A + (((v1_B - v1_A) / (nPoints_Line1 - 1)) * (i));
			}
			for (int i = 0; i < nPoints_Line2; i++)
			{
				points_Line2[i] = v2_A + (((v2_B - v2_A) / (nPoints_Line2 - 1)) * (i));
			}

			//다 체크해봅시다.
			for (int iPoint1 = 0; iPoint1 < nPoints_Line1; iPoint1++)
			{
				for (int iPoint2 = 0; iPoint2 < nPoints_Line2; iPoint2++)
				{
					if ((points_Line1[iPoint1] - points_Line2[iPoint2]).magnitude <= bias)
					{
						return true;
					}
				}
			}

			return false;
		}

		public class CrossCheckResult
		{
			private static CrossCheckResult _instance = new CrossCheckResult();
			public static CrossCheckResult I { get { return _instance; } }

			public bool _isCross = false;
			public Vector2 _posW = Vector2.zero;
			//만약 어떤 Vertex에서 겹친다면, 해당 Vertex를 리턴한다.
			public bool _isAnyPointSame = false;
			public apVertex _overlapVert = null;
			public bool _isSameLine = false;

			private CrossCheckResult()
			{
				Init();
			}
			public void Init()
			{
				_isCross = false;
				_posW = Vector2.zero;

				_isAnyPointSame = false;
				_overlapVert = null;
				_isSameLine = false;
			}
			public void SetCrossResult(Vector2 crossPos)
			{
				_isCross = true;
				_posW = crossPos;
			}
			public void SetSameVertexResult(apVertex vertex)
			{
				_isAnyPointSame = true;
				_overlapVert = vertex;
				_isCross = true;
				_posW = _overlapVert._pos;
			}
			public void SetSameLine(Vector2 crossPos)
			{
				_isCross = true;
				_posW = crossPos;
				_isSameLine = true;
			}
		}

		private const float zeroBias = 1f;
		private const float nearBias = 8.0f;

		public static CrossCheckResult IsEdgeCrossWithResult(apMeshEdge edge, Vector2 edge2A, Vector2 edge2B)
		{
			CrossCheckResult.I.Init();

			Vector2 edge1A = edge._vert1._pos;
			Vector2 edge1B = edge._vert2._pos;

			//만약 어떤 점이 겹친 상태라면 일단 겹친 점에 대한 정보를 넣어준다.
			if (Vector2.Distance(edge1A, edge2A) < zeroBias || Vector2.Distance(edge1A, edge2B) < zeroBias)
			{
				//Vert1 에 겹친다.
				CrossCheckResult.I.SetSameVertexResult(edge._vert1);
				return CrossCheckResult.I;
			}
			else if (Vector2.Distance(edge1B, edge2A) < zeroBias || Vector2.Distance(edge1B, edge2B) < zeroBias)
			{
				//Vert2 에 겹친다.
				CrossCheckResult.I.SetSameVertexResult(edge._vert2);
				return CrossCheckResult.I;
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

			//수직/수평에 따라 다르게 처리
			//if (Mathf.Abs(dX_1) < zeroBias * 0.01f)
			if(Mathf.Approximately(dX_1, 0.0f))
			{
				//Line 1이 수직일 때
				float X1 = (edge1A.x + edge1B.x) * 0.5f;

				
				//if (Mathf.Abs(dX_2) < zeroBias * 0.01f)
				if(Mathf.Approximately(dX_2, 0.0f))
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
							//[겹침] : 수직1 + 수직2
							CrossCheckResult.I.SetSameLine((edge1A + edge1B) * 0.5f);
							return CrossCheckResult.I;
						}
					}
				}
				else if(Mathf.Approximately(dY_2, 0.0f))
				{
					//Line2가 수평일 때
					float Y2 = (edge2A.y + edge2B.y) * 0.5f;

					//서로가 범위 안에 들어가야 한다.
					if(y1_Min <= Y2 && Y2 <= y1_Max
						&& x2_Min <= X1 && X1 <= x2_Max)
					{
						//[교차] : 수직1 + 수평2
						CrossCheckResult.I.SetCrossResult(new Vector2(X1, Y2));
						return CrossCheckResult.I;
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
							//[교차] : 수직1 + 기울기2
							CrossCheckResult.I.SetCrossResult(new Vector2(X1, Yresult));
							return CrossCheckResult.I;
						}

					}
				}
			}
			else if(Mathf.Approximately(dY_1, 0.0f))
			{
				//Line 1이 수평일 때
				float Y1 = (edge1A.y + edge1B.y) * 0.5f;
				if (Mathf.Approximately(dX_2, 0.0f))
				{
					//Line 2가 수직일 때
					//수평 + 수직
					//교차점 비교
					float X2 = (edge2A.x + edge2B.x) * 0.5f;

					if(y2_Min <= Y1 && Y1 <= y2_Max
						&& x1_Min <= X2 && X2 <= x1_Max)
					{
						//[교차] : 수평1 + 수직2
						CrossCheckResult.I.SetCrossResult(new Vector2(X2, Y1));
						return CrossCheckResult.I;
					}
				}
				else if (Mathf.Approximately(dY_2, 0.0f))
				{
					//Line 2가 수평일 때
					//수평 + 수평
					//Y가 같고 X 범위가 겹쳐야 함 Same
					float Y2 = (edge2A.y + edge2B.y) * 0.5f;

					if(Mathf.Approximately(Y1, Y2))
					{
						if (IsAreaIntersection(x1_Min, x1_Max, x2_Min, x2_Max))
						{
							//[겹침] : 수평1 + 수평2
							CrossCheckResult.I.SetSameLine((edge1A + edge1B) * 0.5f);
							return CrossCheckResult.I;
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

					if(x1_Min <= XResult && XResult <= x1_Max
						&& x2_Min <= XResult && XResult <= x2_Max)
					{
						//Line 1, 2의 X 범위 안에 들어간다.
						CrossCheckResult.I.SetCrossResult(new Vector2(XResult, b1));
						return CrossCheckResult.I;
					}
				}
			}
			else
			{
				//Line 1이 기울기가 있을 때
				//if (Mathf.Abs(dX_2) < zeroBias * 0.01f)
				a1 = dY_1 / dX_1;
				b1 = edge1A.y - edge1A.x * a1;

				if(Mathf.Approximately(dX_2, 0.0f))
				{
					//Line 2가 수직일 때
					//기울기 + 수직
					//Line2를 기준으로 x, y범위 비교후 Y 체크 [교차]
					//범위 밖이면 [교차하지 않음]

					float X2 = (edge2A.x + edge2B.x) * 0.5f;

					if (x1_Min <= X2 && X2 <= x1_Max)
					{
						float Yresult = a1 * X2 + b1;
						if (y2_Min <= Yresult && Yresult <= y2_Max)
						{
							//[교차]
							CrossCheckResult.I.SetCrossResult(new Vector2(X2, Yresult));
							return CrossCheckResult.I;
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

					if(x1_Min <= XResult && XResult <= x1_Max
						&& x2_Min <= XResult && XResult <= x2_Max)
					{
						//Line 1, 2의 X 범위 안에 들어간다.
						CrossCheckResult.I.SetCrossResult(new Vector2(XResult, b2));
						return CrossCheckResult.I;
					}
				}
				else
				{
					//Line 2는 수평이나 기울기가 있을 때
					//X 범위 비교후
					//대입법 이용하여 체크하면 [교차]

					if (IsAreaIntersection(x1_Min, x1_Max, x2_Min, x2_Max))
					{	

						a2 = dY_2 / dX_2;
						b2 = edge2A.y - edge2A.x * a2;

						float Yparam1 = a2 - a1;
						float Yparam2 = (a2 * b1) - (a1 * b2);

						//if (Mathf.Abs(Yparam1) < zeroBias * 0.01f)
						if(Mathf.Approximately(Yparam1, 0.0f))
						{
							//기울기가 같을때
							//b도 같아야한다.
							//if (Mathf.Abs(Yparam2) < zeroBias * 0.01f)
							if (Mathf.Approximately(b1, b2))
							{
								//[일치]
								CrossCheckResult.I.SetSameLine((edge1A + edge1B) * 0.5f);
								return CrossCheckResult.I;
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

								CrossCheckResult.I.SetCrossResult(new Vector2(Xresult, Yresult));
								return CrossCheckResult.I;
							}
						}
					}
				}
			}


			//return _crossPoint;
			return null;
		}

		private static bool IsAreaIntersection(float area1Min, float area1Max, float area2Min, float area2Max)
		{
			//[ 1 ] .. [ 2 ] 이거나 [ 2 ] .. [ 1 ]으로 서로 겹쳐지지 않을 때
			if (area1Max < area2Min || area2Max < area1Min)
			{
				//
				return false;
			}
			return true;
		}

		public static apVertex[] Get3VerticesOf2Edges(apMeshEdge edge1, apMeshEdge edge2)
		{
			apVertex[] verts = new apVertex[3];
			verts[0] = edge1._vert1;
			verts[1] = edge1._vert2;

			if (edge2._vert1 == edge1._vert1 || edge2._vert1 == edge1._vert2)
			{
				verts[2] = edge2._vert2;
			}
			else
			{
				verts[2] = edge2._vert1;
			}

			return verts;
		}

		public static apVertex GetSharedVertex(apMeshEdge edge1, apMeshEdge edge2)
		{
			if (edge1._vert1 == edge2._vert1 || edge1._vert1 == edge2._vert2)
			{
				return edge1._vert1;
			}
			else if (edge1._vert2 == edge2._vert1 || edge1._vert2 == edge2._vert2)
			{
				return edge1._vert2;
			}
			return null;
		}

		public static apVertex[] GetNoSharedVertex(apMeshEdge edge1, apMeshEdge edge2)
		{
			if (edge1._vert1 == edge2._vert1)
			{ return new apVertex[] { edge1._vert2, edge2._vert2 }; }
			if (edge1._vert1 == edge2._vert2)
			{ return new apVertex[] { edge1._vert2, edge2._vert1 }; }
			if (edge1._vert2 == edge2._vert1)
			{ return new apVertex[] { edge1._vert1, edge2._vert2 }; }
			if (edge1._vert2 == edge2._vert2)
			{ return new apVertex[] { edge1._vert1, edge2._vert1 }; }
			return null;
		}

		public Vector2 GetNearestPosOnEdge(Vector2 pos)
		{
			float dist = Vector2.Distance(pos, _vert1._pos);
			if(Mathf.Approximately(dist, 0))
			{
				return _vert1._pos;
			}
			float totalDist = Vector2.Distance(_vert2._pos, _vert1._pos);
			if(Mathf.Approximately(totalDist, 0))
			{
				return _vert1._pos;
			}

			float distProj = Vector2.Dot(pos - _vert1._pos, (_vert2._pos - _vert1._pos).normalized);
			if(distProj < 0.0f)
			{
				return _vert1._pos;
			}
			
			if(distProj > totalDist)
			{
				return _vert2._pos;
			}

			float lerp = distProj / totalDist;
			return _vert1._pos * (1.0f - lerp) + _vert2._pos * lerp;
		}
	}

	[Serializable]
	public class apMeshTri
	{
		[SerializeField]
		public int[] _vertIDs = new int[3];

		[NonSerialized]
		public apVertex[] _verts = new apVertex[3];

		//[SerializeField, HideInInspector]
		//public apMeshEdge[] _edges = new apMeshEdge[3];

		public apMeshTri()
		{
			for (int i = 0; i < 3; i++)
			{
				_vertIDs[i] = -1;
				_verts[i] = null;
				//_edges[i] = null;
			}
		}

		public void Link(apVertex vert1, apVertex vert2, apVertex vert3)
		{
			_verts[0] = vert1;
			_verts[1] = vert2;
			_verts[2] = vert3;
		}

		public void SetVertices(apVertex vert1, apVertex vert2, apVertex vert3)
		{
			Vector3 normal = Vector3.Cross((vert2._pos - vert1._pos),
												(vert3._pos - vert1._pos));
			if (normal.z > 0)
			{
				_vertIDs[0] = vert3._uniqueID;
				_vertIDs[1] = vert2._uniqueID;
				_vertIDs[2] = vert1._uniqueID;

				_verts[0] = vert3;
				_verts[1] = vert2;
				_verts[2] = vert1;
			}
			else
			{
				_vertIDs[0] = vert1._uniqueID;
				_vertIDs[1] = vert2._uniqueID;
				_vertIDs[2] = vert3._uniqueID;

				_verts[0] = vert1;
				_verts[1] = vert2;
				_verts[2] = vert3;
			}
		}

		public float GetDepth()
		{
			return _verts[0]._zDepth + _verts[1]._zDepth + _verts[2]._zDepth;
		}



		public bool IsIncludeEdge(apMeshEdge edge)
		{
			return (edge._vert1 == _verts[0] || edge._vert1 == _verts[1] || edge._vert1 == _verts[2]) &&
					(edge._vert2 == _verts[0] || edge._vert2 == _verts[1] || edge._vert2 == _verts[2]);
		}

		public bool IsSameTri(apMeshTri tri)
		{
			//return IsSameTri(tri._edges[0], tri._edges[1], tri._edges[2]);
			return IsSameTri(tri._verts[0], tri._verts[1], tri._verts[2]);
		}

		public bool IsSameTri(apVertex vert1, apVertex vert2, apVertex vert3)
		{
			int nSame = 0;
			if (vert1 == _verts[0] || vert1 == _verts[1] || vert1 == _verts[2])
			{ nSame++; }
			if (vert2 == _verts[0] || vert2 == _verts[1] || vert2 == _verts[2])
			{ nSame++; }
			if (vert3 == _verts[0] || vert3 == _verts[1] || vert3 == _verts[2])
			{ nSame++; }

			return (nSame >= 3);
		}


		public static bool IsPointInTri(apVertex vertPoint, apVertex vertT1, apVertex vertT2, apVertex vertT3)
		{
			return IsPointInTri(vertPoint._pos, vertT1._pos, vertT2._pos, vertT3._pos);
		}

		public static bool IsPointInTri(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
		{
			float s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
			float t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

			if ((s < 0) != (t < 0))
			{
				return false;
			}

			var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;
			if (A < 0.0)
			{
				s = -s;
				t = -t;
				A = -A;
			}
			return s > 0 && t > 0 && (s + t) <= A;

		}

		public bool IsPointInTri(Vector2 pos)
		{
			return IsPointInTri(pos, _verts[0]._pos, _verts[1]._pos, _verts[2]._pos);
		}


	}




	[Serializable]
	public class apMeshPolygon
	{
		//Vert, Edge를 Link해야하는 대상으로 변경
		//HiddenEdge, Tri는 Polygon 내부에 존재하므로 Serialized를 유지
		[SerializeField]
		public List<int> _vertIDs = new List<int>();

		[Serializable]
		public class IDPair
		{
			[SerializeField]
			public int _ID1 = -1;

			[SerializeField]
			public int _ID2 = -1;

			/// <summary>
			/// 백업 파싱용 생성자
			/// </summary>
			public IDPair()
			{

			}

			public IDPair(int ID1, int ID2)
			{
				_ID1 = ID1;
				_ID2 = ID2;
			}
		}
		[SerializeField]
		public List<IDPair> _edgeIDs = new List<IDPair>();

		[NonSerialized]
		public List<apVertex> _verts = new List<apVertex>();

		[NonSerialized]
		public List<apMeshEdge> _edges = new List<apMeshEdge>();

		[SerializeField]
		public List<apMeshEdge> _hidddenEdges = new List<apMeshEdge>();

		[SerializeField]
		public List<apMeshTri> _tris = new List<apMeshTri>();

		[NonSerialized]
		private Color _debugColor = Color.black;

		public Color DebugColor
		{
			get
			{
				if (_debugColor.r < 0.1f && _debugColor.g < 0.1f && _debugColor.b < 0.1f)
				{
					int iRGB = UnityEngine.Random.Range(0, 30);
					if (iRGB % 3 == 0)
					{
						_debugColor = new Color(
								UnityEngine.Random.Range(0.7f, 1.0f),
								UnityEngine.Random.Range(0.1f, 0.4f),
								UnityEngine.Random.Range(0.1f, 0.4f),
								1.0f);
					}
					else if (iRGB % 3 == 1)
					{
						_debugColor = new Color(
								UnityEngine.Random.Range(0.1f, 0.4f),
								UnityEngine.Random.Range(0.7f, 1.0f),
								UnityEngine.Random.Range(0.1f, 0.4f),
								1.0f);
					}
					else
					{
						_debugColor = new Color(
								UnityEngine.Random.Range(0.1f, 0.4f),
								UnityEngine.Random.Range(0.1f, 0.4f),
								UnityEngine.Random.Range(0.7f, 1.0f),
								1.0f);
					}

				}
				return _debugColor;
			}
		}


		private Vector3 _randOffset = new Vector3(-100.0f, -100.0f, 0);
		public Vector3 RandOffset
		{
			get
			{
				if (_randOffset.x < -20)
				{
					_randOffset.x = UnityEngine.Random.Range(-10.0f, 10.0f);
					_randOffset.y = UnityEngine.Random.Range(-10.0f, 10.0f);
				}

				return _randOffset;
			}
		}

		public apMeshPolygon()
		{
			//Clear();

		}

		public void Clear()
		{
			_vertIDs.Clear();
			_edgeIDs.Clear();
			_verts.Clear();
			_edges.Clear();
			_hidddenEdges.Clear();
			_tris.Clear();
		}

		public void Link(apMesh mesh)
		{
			//Mesh에 속한 Vert와 Edge는 초기화 후 ID와 연결해준다.
			//그 외에는 내부 Link를 호출한다.
			_verts.Clear();
			_edges.Clear();

			for (int i = 0; i < _vertIDs.Count; i++)
			{
				apVertex vert = mesh.GetVertexByUniqueID(_vertIDs[i]);
				if (vert != null)
				{
					_verts.Add(vert);
				}
				else
				{
					//?? 없는 Vert다.
					_vertIDs[i] = -1;
				}
			}
			_vertIDs.RemoveAll(delegate (int a)
			{
				return a < 0;
			});

			for (int i = 0; i < _edgeIDs.Count; i++)
			{
				apMeshEdge edge = mesh.GetEdgeByUniqueID(_edgeIDs[i]._ID1, _edgeIDs[i]._ID2);
				if (edge != null)
				{
					_edges.Add(edge);
				}
				else
				{
					_edgeIDs[i]._ID1 = -1;
					_edgeIDs[i]._ID2 = -1;
				}
			}
			_edgeIDs.RemoveAll(delegate (IDPair a)
			{
				return a._ID1 < 0 || a._ID2 < 0;
			});

			//나머지도 링크를 하자
			for (int i = 0; i < _hidddenEdges.Count; i++)
			{
				apMeshEdge hiddenEdge = _hidddenEdges[i];
				apVertex vert1 = mesh.GetVertexByUniqueID(hiddenEdge._vertID_1);
				apVertex vert2 = mesh.GetVertexByUniqueID(hiddenEdge._vertID_2);
				hiddenEdge.Link(vert1, vert2);
			}
			_hidddenEdges.RemoveAll(delegate (apMeshEdge a)
			{
				return a._vert1 == null || a._vert2 == null;
			});

			for (int i = 0; i < _tris.Count; i++)
			{
				apMeshTri tri = _tris[i];
				apVertex vert1 = mesh.GetVertexByUniqueID(tri._vertIDs[0]);
				apVertex vert2 = mesh.GetVertexByUniqueID(tri._vertIDs[1]);
				apVertex vert3 = mesh.GetVertexByUniqueID(tri._vertIDs[2]);
				tri.Link(vert1, vert2, vert3);
			}
			_tris.RemoveAll(delegate (apMeshTri a)
			{
				return a._verts[0] == null || a._verts[1] == null || a._verts[2] == null;
			});

		}

		public bool IsSamePolygon(List<apVertex> verts)
		{
			int nSameVerts = 0;

			for (int i = 0; i < verts.Count; i++)
			{
				if (_verts.Contains(verts[i]))
				{
					nSameVerts++;
				}
			}

			return nSameVerts >= _verts.Count;
		}

		public bool IsEdgeContain(apMeshEdge edge)
		{
			return _edges.Contains(edge);
		}

		public bool IsVertexContain(apVertex vert)
		{
			return _verts.Contains(vert);
		}

		public void SetVertexAndEdges(List<apVertex> verts, List<apMeshEdge> edges)
		{
			Clear();
			for (int i = 0; i < verts.Count; i++)
			{
				_vertIDs.Add(verts[i]._uniqueID);//ID를 기본 저장
				_verts.Add(verts[i]);
			}

			for (int i = 0; i < edges.Count; i++)
			{
				_edgeIDs.Add(new IDPair(edges[i]._vertID_1, edges[i]._vertID_2));
				_edges.Add(edges[i]);
			}
		}

		public class AutoHiddenEdgeData
		{
			public apVertex _vert1, _vert2;
			public float _length = 0.0f;
			public float _minAngle = 90.0f;
			public int Score
			{
				get
				{
					return (int)(Mathf.Clamp01(500.0f - _length) / 5.0f + Mathf.Clamp01(_minAngle / 90.0f) * 200.0f);
				}
			}
			public AutoHiddenEdgeData(apVertex vert1, apVertex vert2)
			{
				_vert1 = vert1;
				_vert2 = vert2;
				_length = Vector3.Distance(_vert1._pos, _vert2._pos);
				_minAngle = 90.0f;
			}
			public void SetEdgeAngle(float angle)
			{
				if (angle < _minAngle)
				{
					_minAngle = angle;
				}
			}
		}


		private class LinkedVert
		{
			public apVertex _vert = null;
			public LinkedVert _prevData = null;
			public LinkedVert _nextData = null;
			public List<apMeshEdge> _linkedEdges = null;

			public float _angleToPrev = 0.0f;
			public float _angleToNext = 0.0f;

			public bool _isIndent = false;//이게 true면 Concave를 만드는 버텍스이다.
			public LinkedVert(apVertex vert)
			{
				_vert = vert;
				_linkedEdges = new List<apMeshEdge>();
			}

			public void Reverse()
			{
				LinkedVert tmpData = _prevData;
				_prevData = _nextData;
				_nextData = tmpData;

				float tmpAngle = _angleToPrev;
				_angleToPrev = _angleToNext;
				_angleToNext = tmpAngle;

				//이건 360도에 맞게
				if(_angleToPrev > _angleToNext)
				{
					_angleToPrev -= 360.0f;
				}

				_isIndent = !_isIndent;//예각, 둔각 토글
			}
		}


		public void MakeHiddenEdgeAndTri()
		{
			int nVert = _verts.Count;
			int nNeedHiddenEdge = nVert - 3;
			int nNeedTri = nVert - 2;


			if (_hidddenEdges.Count == nNeedHiddenEdge && _tris.Count == nNeedTri)
			{
				return;
			}

			_hidddenEdges.Clear();


			//Debug.Log("Verts : " + nVert + " / Hidden : " + nNeedHiddenEdge + " / Tri : " + nNeedTri);
			List<AutoHiddenEdgeData> hiddenData = new List<AutoHiddenEdgeData>();

			if (nNeedHiddenEdge > 0)
			{

				//추가 21.1.9 : 버텍스들의 "예각", "둔각" 여부를 따로 저장한다.
				//- 버텍스들을 임의의 연결 순서대로 Start > End로 연결한다.
				//- 두개의 Edge로의 각도를 Atan2로 계산한 후 "현재 방향"에서의 각도를 저장한다. (180도 비교 여부도 저장)
				//- 모든 버텍스의 각도의 합과 그 반대의 각도를 합친 결과를 비교하여, 반대의 각도보다 작으면 정방향, 그렇지 않으면 역방향
				//- 역방향이라면, 순서를 바꾸고, 예각/둔각 상태를 초기화한다.
				//이후 HiddenEdge를 지정할 때, LinkedVert를 이용하여 만든다.
				LinkedVert startLVert = null;
				LinkedVert curLVert = null;

				List<LinkedVert> linkedVerts = new List<LinkedVert>();
				Dictionary<apVertex, LinkedVert> vert2LinkedData = new Dictionary<apVertex, LinkedVert>();

				//Edge를 돌면서 버텍스와 연결된 Edge들을 리스트로 저장하자
				apMeshEdge curEdge = null;
				for (int i = 0; i < _edges.Count; i++)
				{
					curEdge = _edges[i];
					apVertex vertA = curEdge._vert1;
					apVertex vertB = curEdge._vert2;

					LinkedVert linkedVertA = null;
					LinkedVert linkedVertB = null;

					if(vert2LinkedData.ContainsKey(vertA)) { linkedVertA = vert2LinkedData[vertA]; }
					else
					{
						linkedVertA = new LinkedVert(vertA);
						vert2LinkedData.Add(vertA, linkedVertA);
						linkedVerts.Add(linkedVertA);
					}
					linkedVertA._linkedEdges.Add(curEdge);

					if(vert2LinkedData.ContainsKey(vertB)) { linkedVertB = vert2LinkedData[vertB]; }
					else
					{
						linkedVertB = new LinkedVert(vertB);
						vert2LinkedData.Add(vertB, linkedVertB);
						linkedVerts.Add(linkedVertB);
					}
					linkedVertB._linkedEdges.Add(curEdge);
				}

				startLVert = linkedVerts[0];
				curLVert = startLVert;
				//이제 순서대로 연결을 하자
				List<LinkedVert> sortedLinkedVerts = new List<LinkedVert>();
				List<apMeshEdge> usedEdges = new List<apMeshEdge>();
				sortedLinkedVerts.Add(startLVert);

				//연결 불가 리스트를 만들자
				//각도에 의해서 연결 불가능한 조합을 만들어두자
				Dictionary<apVertex, Dictionary<apVertex, bool>> preventableCondsByAngle = null;

				bool isAnyError = false;
				while(true)
				{
					//연결된 두개의 Edge중 아직 사용 안된 Edge를 꺼내자
					apMeshEdge nextEdge = null;
					for (int iEdge = 0; iEdge < curLVert._linkedEdges.Count; iEdge++)
					{
						apMeshEdge linkedEdge = curLVert._linkedEdges[iEdge];
						if(!usedEdges.Contains(linkedEdge))
						{
							//아직 사용안된 Edge 발견
							nextEdge = linkedEdge;
							break;
						}
					}
					if(nextEdge == null)
					{
						//연결 실패
						isAnyError = true;
						break;
					}
					usedEdges.Add(nextEdge);

					apVertex nextVert = (nextEdge._vert1 == curLVert._vert) ? nextEdge._vert2 : nextEdge._vert1;
					LinkedVert nextLVert = vert2LinkedData[nextVert];

					//두개를 연결한다.
					curLVert._nextData = nextLVert;
					nextLVert._prevData = curLVert;

					if(sortedLinkedVerts.Contains(nextLVert)
						|| sortedLinkedVerts.Count >= linkedVerts.Count)
					{
						//한바퀴 다 돌았다.
						break;
					}

					sortedLinkedVerts.Add(nextLVert);
					curLVert = nextLVert;
				}
				
				//bool isDebug = false;
				if(!isAnyError)
				{
					//에러가 없다면,
					//이제 연결된 각도들을 Prev > Next 각도를 계산한다.
					float totalNormalAngles = 0.0f;
					float totalReversedAngles = 0.0f;
					bool isAnyIndent = false;
					for (int i = 0; i < sortedLinkedVerts.Count; i++)
					{
						curLVert = sortedLinkedVerts[i];
						LinkedVert prevLVert = curLVert._prevData;
						LinkedVert nextLVert = curLVert._nextData;

						Vector2 cur2Prev = prevLVert._vert._pos - curLVert._vert._pos;
						Vector2 cur2Next = nextLVert._vert._pos - curLVert._vert._pos;

						float prevAngle = apUtil.AngleTo360(Mathf.Atan2(cur2Prev.y, cur2Prev.x) * Mathf.Rad2Deg);
						float nextAngle = apUtil.AngleTo360(Mathf.Atan2(cur2Next.y, cur2Next.x) * Mathf.Rad2Deg);

						//크기는 항상 nextAngle이 더 커야한다.
						if(nextAngle < prevAngle)
						{
							nextAngle += 360.0f;
						}	

						float deltaAngle = apUtil.AngleTo360(nextAngle - prevAngle);
						float reverseAngle = 360.0f - deltaAngle;

						//일단 각도를 넣자
						curLVert._angleToPrev = prevAngle;
						curLVert._angleToNext = nextAngle;

						//현재 방향에서 각도가 180도 보다 작다면 예각, 그렇지 않으면 둔각
						if(deltaAngle < 180.0f)
						{
							curLVert._isIndent = false;//False가 예각
						}
						else
						{
							curLVert._isIndent = true;
							isAnyIndent = true;
							//isDebug = true;
						}

						//방향이 정확한지 확인하기 위한 각도 확인
						totalNormalAngles += deltaAngle;
						totalReversedAngles += reverseAngle;
					}

					//if(isDebug)
					//{
					//	Debug.LogWarning("둔각이 있는 폴리곤 발견");
					//	for (int i = 0; i < sortedLinkedVerts.Count; i++)
					//	{
					//		curLVert = sortedLinkedVerts[i];
					//		if(!curLVert._isIndent)
					//		{
					//			Debug.Log("[" + i + "] : " + curLVert._vert._pos + " / Angle : " + curLVert._angleToPrev + " ~ " + curLVert._angleToNext);
					//		}
					//		else
					//		{
					//			Debug.LogError("[" + i + " - 둔각] : " + curLVert._vert._pos + " / Angle : " + curLVert._angleToPrev + " ~ " + curLVert._angleToNext);
					//		}
							
					//	}
					//	Debug.Log("---");
					//}

					//만약 전체 Prev->Next 각도의 합이 Reverse보다 더 크다면, 순서가 반대가 되어야 한다.
					if(totalNormalAngles > totalReversedAngles)
					{
						//if(isDebug)
						//{
						//	Debug.LogError("역방향 : 내부 각도 : " + totalNormalAngles + " / 외부 각도 : " + totalReversedAngles);
						//}

						isAnyIndent = false;
						for (int i = 0; i < sortedLinkedVerts.Count; i++)
						{
							curLVert = sortedLinkedVerts[i];
							curLVert.Reverse();//뒤집자

							if(curLVert._isIndent)
							{
								isAnyIndent = true;
							}

							//if(!curLVert._isIndent)
							//{
							//	Debug.Log("[" + i + "] : " + curLVert._vert._pos + " / Angle : " + curLVert._angleToPrev + " ~ " + curLVert._angleToNext);
							//}
							//else
							//{
							//	Debug.LogError("[" + i + " - 둔각] : " + curLVert._vert._pos + " / Angle : " + curLVert._angleToPrev + " ~ " + curLVert._angleToNext);
							//}
						}
						//Debug.Log("---");
					}
					//else
					//{
					//	if(isDebug)
					//	{
					//		Debug.Log("정방향 : 내부 각도 : " + totalNormalAngles + " / 외부 각도 : " + totalReversedAngles);
					//	}
						
					//}

					//연결 불가능한 리스트를 만들자
					//단, 둔각이 하나라도 있는 경우에
					if(isAnyIndent)
					{
						//if(isDebug)
						//{
						//	Debug.LogWarning("둔각이 있으니 연결 불가 버텍스 조합 만들기");
						//}

						preventableCondsByAngle = new Dictionary<apVertex, Dictionary<apVertex, bool>>();

						LinkedVert otherLVert = null;
						for (int i = 0; i < sortedLinkedVerts.Count - 1; i++)
						{
							curLVert = sortedLinkedVerts[i];

							float minAngle_Cur = apUtil.AngleTo360(curLVert._angleToPrev);
							float maxAngle_Cur = apUtil.AngleTo360(curLVert._angleToNext);
							if(maxAngle_Cur < minAngle_Cur)
							{
								maxAngle_Cur += 360.0f;
							}

							//다른 모든 점들을 체크한다.
							for (int iOther = i + 1; iOther < sortedLinkedVerts.Count; iOther++)
							{
								otherLVert = sortedLinkedVerts[iOther];

								float minAngle_Other = apUtil.AngleTo360(otherLVert._angleToPrev);
								float maxAngle_Other = apUtil.AngleTo360(otherLVert._angleToNext);
								if(maxAngle_Other < minAngle_Other)
								{
									maxAngle_Other += 360.0f;
								}

								//if(isDebug)
								//{
								//	Debug.Log("> 선 연결 테스트 " + i + " > " + iOther);
								//	if (curLVert._isIndent)
								//	{
								//		Debug.LogWarning(">> From : " + curLVert._vert._pos + " Angle : " + minAngle_Cur + " ~ " + maxAngle_Cur);
								//	}
								//	else
								//	{
								//		Debug.Log(">> From : " + curLVert._vert._pos + " Angle : " + minAngle_Cur + " ~ " + maxAngle_Cur);
								//	}
									
								//	if(otherLVert._isIndent)
								//	{
								//		Debug.LogWarning(">> To : " + otherLVert._vert._pos + " Angle : " + minAngle_Other + " ~ " + maxAngle_Other);
								//	}
								//	else
								//	{
								//		Debug.Log(">> To : " + otherLVert._vert._pos + " Angle : " + minAngle_Other + " ~ " + maxAngle_Other);
								//	}
								//}


								//선분을 연결한다.
								Vector2 cur2Other = otherLVert._vert._pos - curLVert._vert._pos;

								//벡터의 각도를 구하자
								float edgeAngle_FromCur = apUtil.AngleTo360(Mathf.Atan2(cur2Other.y, cur2Other.x) * Mathf.Rad2Deg);
								float edgeAngle_FromOther = apUtil.AngleTo360(edgeAngle_FromCur + 180.0f);

								//if (isDebug)
								//{
								//	Debug.Log(">> 연결 선 각도(변환 전) : " + edgeAngle_FromCur + " / R: " + edgeAngle_FromOther);
								//}

								edgeAngle_FromCur -= 720.0f;
								edgeAngle_FromOther -= 720.0f;


								
								//이제 edgeAngle_FromCur이 Cur의 Min-Max 범위 밖에 있거나,
								//edgeAngle_FromOther이 Other의 Min-Max 범위 밖에 있으면, 이 점들은 연결해서는 안된다.
								while(edgeAngle_FromCur < minAngle_Cur)
								{
									edgeAngle_FromCur += 360.0f;//360도 보정을 하고
								}
								while(edgeAngle_FromOther < minAngle_Other)
								{
									edgeAngle_FromOther += 360.0f;//360도 보정을 하고
								}

								//if (isDebug)
								//{
								//	Debug.Log(">> 연결 선 각도 : " + edgeAngle_FromCur + " / R: " + edgeAngle_FromOther);
								//}

								if(edgeAngle_FromCur > maxAngle_Cur || edgeAngle_FromOther > maxAngle_Other)
								{
									//이 두 점은 연결해서는 안된다.
									if(!preventableCondsByAngle.ContainsKey(curLVert._vert))
									{
										preventableCondsByAngle.Add(curLVert._vert, new Dictionary<apVertex, bool>());
									}

									if(!preventableCondsByAngle.ContainsKey(otherLVert._vert))
									{
										preventableCondsByAngle.Add(otherLVert._vert, new Dictionary<apVertex, bool>());
									}

									preventableCondsByAngle[curLVert._vert].Add(otherLVert._vert, false);
									preventableCondsByAngle[otherLVert._vert].Add(curLVert._vert, false);

									//if (isDebug)
									//{
									//	Debug.LogError(">> 연결 불가 --------");
									//}
								}
								//else
								//{
								//	if (isDebug)
								//	{
								//		Debug.Log(">> 연결 가능 --------");
								//	}
								//}

							}
						}
					}
				}




				bool isAnyPreventedPair = preventableCondsByAngle != null && preventableCondsByAngle.Count > 0;
				//if (isAnyPreventedPair)
				//{
				//	Debug.Log("금지된 조합이 있다. [" + (preventableCondsByAngle.Count / 2) + "]");
				//}




				//Debug.Log("히든 엣지 필요 개수 : " + nNeedHiddenEdge);
				for (int iBaseVert = 0; iBaseVert < nVert; iBaseVert++)
				{
					apVertex baseVert = _verts[iBaseVert];

					//직접 연결되지 않은 Edge를 찾자
					for (int iNext = 0; iNext < nVert; iNext++)
					{
						apVertex nextVert = _verts[iNext];
						if (nextVert == baseVert)
						{
							continue;
						}

						//Edge가 있는가
						bool isExistEdge = _edges.Exists(delegate (apMeshEdge a)
						{
							return a.IsSameEdge(baseVert, nextVert);
						});

						if (isExistEdge)
						{
							continue;
						}

						//이 조합이 금지되어 있는가 (21.1.9)
						if(isAnyPreventedPair)
						{
							if(preventableCondsByAngle.ContainsKey(baseVert))
							{
								if(preventableCondsByAngle[baseVert].ContainsKey(nextVert))
								{
									//Debug.Log("금지된 버텍스 조합을 연결하려고 했다. : " + baseVert._pos + " > " + nextVert._pos);
									continue;
								}
							}
						}

						if (hiddenData.Exists(delegate (AutoHiddenEdgeData a)
						 {
							 return (a._vert1 == baseVert && a._vert2 == nextVert)
								 || (a._vert2 == baseVert && a._vert1 == nextVert);
						 }))
						{
							continue;
						}

						//없다 -> HiddenEdge 대상이다.
						//다른 HiddenEdge와 겹치지 않는지 체크한다.
						bool isAnyCross = false;
						for (int iHide = 0; iHide < _hidddenEdges.Count; iHide++)
						{
							apMeshEdge hiddenEdge = _hidddenEdges[iHide];

							if (hiddenEdge.IsSameEdge(baseVert, nextVert))
							{
								isAnyCross = true;
								break;
							}
							if (apMeshEdge.IsEdgeCross(hiddenEdge._vert1, hiddenEdge._vert2, baseVert, nextVert))
							{
								isAnyCross = true;
								break;
							}
						}

						//추가 21.1.9 : HiddenEdge와 연결된 버텍스의 양쪽의 Edge의 합과 같아야 한다.

						if (!isAnyCross)
						{
							//교차되는 Hidden Edge가 없다.
							//Hidden Edge를 만들쟈!
							//수정 => 일단 Hidden Edge "대상"에 넣자

							AutoHiddenEdgeData newHiddenData = new AutoHiddenEdgeData(baseVert, nextVert);

							//다른 Edge와 비교하여 Angle을 넣어주자
							for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
							{
								apMeshEdge edge = _edges[iEdge];
								float angle = Vector2.Angle(edge._vert2._pos - edge._vert1._pos, nextVert._pos - baseVert._pos);
								if (angle > 90.0f)
								{
									angle = 180.0f - angle;
								}
								newHiddenData.SetEdgeAngle(angle);
							}

							hiddenData.Add(newHiddenData);
							//if(isAnyPreventedPair)
							//{
							//	Debug.Log("Make Hidden Edge");
							//}
						}
					}
				}

				hiddenData.Sort(delegate (AutoHiddenEdgeData a, AutoHiddenEdgeData b)
				{
					return b.Score - a.Score;
				});

				for (int iData = 0; iData < hiddenData.Count; iData++)
				{
					AutoHiddenEdgeData data = hiddenData[iData];

					//다른 HiddenEdge와 겹치지 않는지 체크한다.
					bool isAnyCross = false;
					for (int iHide = 0; iHide < _hidddenEdges.Count; iHide++)
					{
						apMeshEdge hiddenEdge = _hidddenEdges[iHide];

						if (hiddenEdge.IsSameEdge(data._vert1, data._vert2))
						{
							isAnyCross = true;
							break;
						}

						if (apMeshEdge.IsEdgeCross(hiddenEdge._vert1, hiddenEdge._vert2, data._vert1, data._vert2))
						{
							isAnyCross = true;
							break;
						}
					}
					if (isAnyCross)
					{
						continue;
					}

					apMeshEdge newHiddenEdge = new apMeshEdge(data._vert1, data._vert2);
					newHiddenEdge._isHidden = true;

					_hidddenEdges.Add(newHiddenEdge);
					if (_hidddenEdges.Count >= nNeedHiddenEdge)
					{
						break;
					}
				}

				if (_hidddenEdges.Count < nNeedHiddenEdge)
				{
					//오잉? 다 입력되지 않았네요. 그냥 한번 넣어봅시다.
					for (int iBaseVert = 0; iBaseVert < nVert; iBaseVert++)
					{
						apVertex baseVert = _verts[iBaseVert];

						//직접 연결되지 않은 Edge를 찾자
						for (int iNext = 0; iNext < nVert; iNext++)
						{
							apVertex nextVert = _verts[iNext];
							if (nextVert == baseVert)
							{
								continue;
							}

							//Edge가 있는가
							bool isExistEdge = _edges.Exists(delegate (apMeshEdge a)
							{
								return a.IsSameEdge(baseVert, nextVert);
							});

							if (isExistEdge)
							{
								continue;
							}


							//이 조합이 금지되어 있는가 (21.1.9)
							//bool isPreventedPair = false;
							if(isAnyPreventedPair)
							{
								if(preventableCondsByAngle.ContainsKey(baseVert))
								{
									if(preventableCondsByAngle[baseVert].ContainsKey(nextVert))
									{
										//Debug.Log("<추가> 금지된 버텍스 조합을 연결하려고 했다. : " + baseVert._pos + " > " + nextVert._pos);
										//isPreventedPair = true;
										continue;
									}
								}
							}


							//없다 -> HiddenEdge 대상이다.
							//다른 HiddenEdge와 겹치지 않는지 체크한다.
							bool isAnyCross = false;
							for (int iHide = 0; iHide < _hidddenEdges.Count; iHide++)
							{
								apMeshEdge hiddenEdge = _hidddenEdges[iHide];

								if (hiddenEdge.IsSameEdge(baseVert, nextVert))
								{
									isAnyCross = true;
									break;
								}

								if (apMeshEdge.IsEdgeCross(hiddenEdge._vert1, hiddenEdge._vert2, baseVert, nextVert))
								{
									isAnyCross = true;
									break;
								}
							}

							if (isAnyCross)
							{
								continue;
							}

							//if(isPreventedPair)
							//{
							//	Debug.LogWarning("잘못된 조합이지만 그냥 추가");
							//}
							apMeshEdge newHiddenEdge = new apMeshEdge(baseVert, nextVert);
							newHiddenEdge._isHidden = true;

							_hidddenEdges.Add(newHiddenEdge);
							if (_hidddenEdges.Count >= nNeedHiddenEdge)
							{
								break;
							}
						}
					}
				}



				//Debug.LogWarning("만들어진 히든 엣지 : " + _hidddenEdges.Count);
			}


			//Hidden Edge까지 추가했다면..
			//이제 내부에 Tri를 만들어주자
			//이건 겹치는건 체크하지 않는다.
			//Hidden을 제외하고는
			//1Edge -> 1Tri이며,
			//이미 Tri에 포함된 Edge는 제외한다.
			_tris.Clear();
			if (nNeedTri > 0)
			{
				MakeTriangles();
			}
		}

		private void MakeTriangles()
		{
			_tris.Clear();
			List<apMeshEdge> allEdges = new List<apMeshEdge>();
			for (int i = 0; i < _edges.Count; i++)
			{
				allEdges.Add(_edges[i]);
			}

			for (int i = 0; i < _hidddenEdges.Count; i++)
			{
				allEdges.Add(_hidddenEdges[i]);
			}


			for (int iEdge = 0; iEdge < allEdges.Count; iEdge++)
			{
				apMeshEdge baseEdge = allEdges[iEdge];

				if (!baseEdge._isHidden)
				{
					//Hidden이 아닌 경우
					//한개의 Edge는 한개의 Tri에만 들어간다.
					bool isExistTri = _tris.Exists(delegate (apMeshTri a)
					{
						return a.IsIncludeEdge(baseEdge);
					});

					if (isExistTri)
					{
						//이미 Tri 계산에 사용된 Edge이다.
						continue;
					}
				}

				//이제 여기에 연결된 Edge 하나를 찾는다. (두번째 Edge)
				//처음엔 "기본 Edge", 여기서 못찾으면 "Hidden Edge"에서 찾자

				//한개의 Edge에 대해서 최대 2개의 Tri만 나온다.
				//2개를 만들면 더이상 처리하지 말자
				int nCreatedTri = 0;

				for (int iNext = 0; iNext < allEdges.Count; iNext++)
				{
					apMeshEdge nextEdge = allEdges[iNext];
					if (baseEdge == nextEdge)
					{
						//같은거다
						continue;
					}

					if (!nextEdge.IsLinkedEdge(baseEdge))
					{
						//연결되지 않았다.
						continue;
					}

					//이 둘을 연결할 선분은 있는지 체크
					//공유하고 있지 않은 버텍스 두개를 구하자
					apVertex[] noSharedVerts = apMeshEdge.GetNoSharedVertex(baseEdge, nextEdge);
					if (noSharedVerts == null)
					{
						continue;
					}

					//해당 Edge를 포함하는 Edge가 있는가
					apMeshEdge thirdEdge = allEdges.Find(delegate (apMeshEdge a)
					{
						return a.IsSameEdge(noSharedVerts[0], noSharedVerts[1]);
					});

					if (thirdEdge == null)
					{
						continue;
					}

					apVertex[] allVerts = apMeshEdge.Get3VerticesOf2Edges(baseEdge, nextEdge);

					//base, next, third 완성
					bool isExistTri = _tris.Exists(delegate (apMeshTri a)
					{
						return a.IsSameTri(allVerts[0], allVerts[1], allVerts[2]);
					});

					if (!isExistTri)
					{
						//겹치는 Tri가 없다.
						//만들자
						apMeshTri newTri = new apMeshTri();
						newTri.SetVertices(allVerts[0], allVerts[1], allVerts[2]);

						_tris.Add(newTri);

						nCreatedTri++;
					}

					if (nCreatedTri >= 2)
					{
						//처리 끝
						break;
					}
				}

				//if (_tris.Count >= nNeedTri)
				//{
				//	//이미 이 폴리곤에서 만들 수 있는 최대의 Tri를 만들었다.
				//	break;
				//}
			}

			//마지막에 Sort
			SortTriByDepth();
		}


		public bool TurnHiddenEdge(apMeshEdge hiddenEdge)
		{
			if (!_hidddenEdges.Contains(hiddenEdge))
			{
				Debug.LogError("Not Contains Hidden Edge");
				return false;
			}

			//List<apMeshEdge> allEdges = new List<apMeshEdge>();
			//for (int i = 0; i < _edges.Count; i++)
			//{
			//	allEdges.Add(_edges[i]);
			//}

			//for (int i = 0; i < _hidddenEdges.Count; i++)
			//{
			//	allEdges.Add(_hidddenEdges[i]);
			//}

			List<apMeshTri> containTris = _tris.FindAll(delegate (apMeshTri a)
			{
				return a.IsIncludeEdge(hiddenEdge);
			});

			if (containTris.Count != 2)
			{
				//회전이 불가능하다
				Debug.LogError("Tri Count is Not 2 : " + containTris.Count);
				return false;
			}

			apVertex[] newVerts = new apVertex[2];

			for (int i = 0; i < 2; i++)
			{
				apMeshTri curTri = containTris[i];

				if (curTri._verts[0] != hiddenEdge._vert1 && curTri._verts[0] != hiddenEdge._vert2)
				{
					newVerts[i] = curTri._verts[0];
				}
				else if (curTri._verts[1] != hiddenEdge._vert1 && curTri._verts[1] != hiddenEdge._vert2)
				{
					newVerts[i] = curTri._verts[1];
				}
				else if (curTri._verts[2] != hiddenEdge._vert1 && curTri._verts[2] != hiddenEdge._vert2)
				{
					newVerts[i] = curTri._verts[2];
				}
				else
				{
					newVerts[i] = null;
				}
			}


			if (newVerts[0] != null && newVerts[1] != null && newVerts[0] != newVerts[1])
			{
				//새로운 엣지를 넣고
				apMeshEdge newHidden = new apMeshEdge(newVerts[0], newVerts[1]);
				newHidden._isHidden = true;
				_hidddenEdges.Add(newHidden);

				//Debug.LogWarning("Vertex Turn [" 
				//	+ hiddenEdge._vert1._index + ", " + hiddenEdge._vert2._index + "] -> ["
				//	+ newHidden._vert1._index + ", " + newHidden._vert2._index + "]");

				//현재 엣지를 지우자
				_hidddenEdges.Remove(hiddenEdge);

				MakeTriangles();
			}
			else
			{
				Debug.LogError("Vertex is Error");
			}


			return true;
		}

		public float GetDepth()
		{
			if (_verts == null || _verts.Count == 0)
			{
				return 0.0f;
			}

			float totalDepth = 0.0f;
			for (int i = 0; i < _verts.Count; i++)
			{
				totalDepth += _verts[i]._zDepth;
			}

			return totalDepth / _verts.Count;
		}

		public void SortTriByDepth()
		{
			if (_tris == null || _tris.Count == 0)
			{
				return;
			}

			_tris.Sort(delegate (apMeshTri a, apMeshTri b)
			{
				return (int)(a.GetDepth() * 1000.0f) - (int)(b.GetDepth() * 1000.0f);
			});
		}

		/// <summary>
		/// 추가 20.1.5 : 외부에서 HiddenEdge를 만들어서 대입하고 Triangle을 만들고자 하는 경우
		/// </summary>
		/// <param name="srcHiddenEdges"></param>
		/// <returns></returns>
		public bool SetHiddenEdgesAndMakeTri(List<apMeshEdge> srcHiddenEdges)
		{
			int nVert = _verts.Count;
			int nNeedHiddenEdge = nVert - 3;
			int nNeedTri = nVert - 2;

			//HiddenEdge가 필요한 개수와 다르다 > 실패
			if (srcHiddenEdges.Count != nNeedHiddenEdge)
			{
				return false;
			}

			_hidddenEdges.Clear();
			apMeshEdge srcHiddenEdge = null;
			for (int i = 0; i < srcHiddenEdges.Count; i++)
			{
				srcHiddenEdge = srcHiddenEdges[i];
				
				if(!_verts.Contains(srcHiddenEdge._vert1)
					|| !_verts.Contains(srcHiddenEdge._vert2))
				{
					//폴리곤에 없는 버텍스를 가졌다면 실패
					return false;
				}

				apMeshEdge newHidden = new apMeshEdge(srcHiddenEdge._vert1, srcHiddenEdge._vert2);
				newHidden._isHidden = true;
				_hidddenEdges.Add(newHidden);
			}

			//Tri를 만들자
			_tris.Clear();
			if (nNeedTri > 0)
			{
				MakeTriangles();
			}

			return true;
		}
	}

	



	/// <summary>
	/// 폴리곤을 생성할 때, 이 버텍스들간의 짝은 폴리곤으로 생성하지 않는다.
	/// </summary>
	public class PolygonExceptionSet
	{
		public List<apVertex> _vertices = new List<apVertex>();

		public PolygonExceptionSet()
		{
			_vertices.Clear();
		}

		public void AddVertex(apVertex vert)
		{
			if(!_vertices.Contains(vert))
			{
				_vertices.Add(vert);
			}
		}

		public bool IsContains(apVertex vert1, apVertex vert2, apVertex vert3)
		{
			return _vertices.Contains(vert1) && _vertices.Contains(vert2) && _vertices.Contains(vert3);
		}

		public bool IsContains(List<apVertex> vertices)
		{
			for (int i = 0; i < vertices.Count; i++)
			{
				if(!_vertices.Contains(vertices[i]))
				{
					//하나라도 포함되지 않으면 False
					return false;
				}
			}
			return true;
		}
	}
}