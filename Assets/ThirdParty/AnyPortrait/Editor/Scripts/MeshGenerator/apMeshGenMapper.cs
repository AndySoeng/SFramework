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
//using UnityEditor;
//using System.Collections;
//using System.Collections.Generic;
//using System;


//using AnyPortrait;

//namespace AnyPortrait
//{
//	/// <summary>
//	/// MeshGenerator에 포함되어 있는 Mapper 클래스
//	/// Scan 이후에 Preview/Generate 단계에서 사용된다.
//	/// 일부 정보는 Editor에 저장된다.
//	/// </summary>
//	public class apMeshGenMapper
//	{
//		// Members
//		//-------------------------------------------------------------------------------
//		public apEditor _editor = null;
//		public apMeshGenerator _meshGenerator = null;


//		//자동 생성에 필요한 값들
//		public enum MapperShape : int
//		{
//			/// <summary>사각형 방식. 컨트롤 포인트를 추가적으로 넣을 수 없는 가장 심플한 방법</summary>
//			Quad = 0,
//			/// <summary>Quad의 응용판. "X,Y 축" 별로 컨트롤 포인트를 추가할 수 있다.</summary>
//			ComplexQuad = 1,
//			/// <summary>원형이며, 내부는 방사형. "둘레"에 컨트롤 포인트를 추가할 수 있다.</summary>
//			Circle = 2,
//			/// <summary>고리 형태이며, 방사형과 비슷하지만 처리는 ComplexQuad에 가깝다. "둘레"에 컨트롤 포인트를 추가할 수 있다.</summary>
//			Ring = 3
//		}
//		public MapperShape _shape = MapperShape.Quad;
		
//		public class ControlPoint
//		{
//			public Vector2 _pos_Org = Vector2.zero;
//			public Vector2 _pos_Cur = Vector2.zero;

//			public bool _isOuter = false;
//			//public bool _isMain = false;

//			//Quad 타입인 경우
//			public int _quad_iX = 0;
//			public int _quad_iY = 0;
//			public float _quad_LerpX = 0.0f;
//			public float _quad_LerpY = 0.0f;

//			//Circle 타입인 경우
//			public int _circle_Index = 0;
//			public float _circle_Angle = 0;
//			public float _circle_Lerp = 0.0f;

//			//GUI를 위한 연결 관계
//			public List<ControlPoint> _linkedOuterPoints = new List<ControlPoint>();
//			public List<ControlPoint> _linkedInnerOrCrossPoints = new List<ControlPoint>();
			
			
//			public ControlPoint()
//			{
//				_pos_Org = Vector2.zero;
//				_pos_Cur = Vector2.zero;

//				_isOuter = false;
				

//				_quad_iX = 0;
//				_quad_iY = 0;

//				_quad_LerpX = 0.0f;
//				_quad_LerpY = 0.0f;

//				_circle_Index = 0;
//				_circle_Angle = 0;

//				_circle_Lerp = 0.0f;

//				_linkedOuterPoints.Clear();
//				_linkedInnerOrCrossPoints.Clear();
//			}

//			public static ControlPoint MakeQuadPoint(Vector2 pos, int iX, int iY, float lerpX, float lerpY, bool isOuter)
//			{
//				ControlPoint newPoint = new ControlPoint();
//				newPoint._pos_Org = pos;
//				newPoint._pos_Cur = pos;
//				newPoint._quad_iX = iX;
//				newPoint._quad_iY = iY;
//				newPoint._isOuter = isOuter;
//				//newPoint._isMain = isMain;

//				newPoint._quad_LerpX = lerpX;
//				newPoint._quad_LerpY = lerpY; 

				

//				return newPoint;
//			}

//			public static ControlPoint MakeCircleOuterPoint(Vector2 pos, int index, float angle, float angleLerp)
//			{
//				ControlPoint newPoint = new ControlPoint();
//				newPoint._pos_Org = pos;
//				newPoint._pos_Cur = pos;
//				newPoint._isOuter = true;
//				newPoint._circle_Index = index;
//				newPoint._circle_Angle = angle;
//				//newPoint._isMain = isMain;

//				newPoint._circle_Lerp = angleLerp;

//				return newPoint;
//			}

//			public static ControlPoint MakeCircleCenterPoint(Vector2 pos)
//			{
//				ControlPoint newPoint = new ControlPoint();
//				newPoint._pos_Org = pos;
//				newPoint._pos_Cur = pos;
//				newPoint._isOuter = false;
				
//				return newPoint;
//			}

//			public static ControlPoint MakeRingPoint(Vector2 pos, int index, float angle, float angleLerp, bool isOuter)
//			{
//				ControlPoint newPoint = new ControlPoint();
//				newPoint._pos_Org = pos;
//				newPoint._pos_Cur = pos;
//				newPoint._isOuter = isOuter;
//				newPoint._circle_Index = index;
//				newPoint._circle_Angle = angle;

//				newPoint._circle_Lerp = angleLerp;

//				return newPoint;
//			}
			
//		}
		


//		//각 Shape별로 포인트를 동시에 가지고 있는다.
//		//전체 리스트와 참조를 위한 배열 및 별도의 변수로 가지고 있는다.
//		private ControlPoint[,] _controlPointArr_Quad = null;
//		private List<ControlPoint> _controlPointList_Quad = new List<ControlPoint>();

//		private ControlPoint[,] _controlPointArr_ComplexQuad = null;
//		private List<ControlPoint> _controlPointList_ComplexQuad = new List<ControlPoint>();

//		private ControlPoint[] _controlPointArr_CircleOuter = null;
//		private ControlPoint _controlPoint_CircleCenter = null;
//		private List<ControlPoint> _controlPointList_Circle = new List<ControlPoint>();

//		private ControlPoint[] _controlPointArr_RingOuter = null;
//		private ControlPoint[] _controlPointArr_RingInner = null;
//		private List<ControlPoint> _controlPointList_Ring = new List<ControlPoint>();

//		//초기화 되었는가
//		private bool _isPointCreated = false;

//		//Make 파라미터
//		private Vector2 _areaLT = Vector2.zero;
//		private Vector2 _areaRB = Vector2.zero;
//		private Vector2 _areaCenter = Vector2.zero;
//		private int _numPoint_ComplexQuadX = 3;
//		private int _numPoint_ComplexQuadY = 3;
//		private int _numPoint_CircleRing = 3;

//		// Init
//		//-------------------------------------------------------------------------------
//		public apMeshGenMapper(apEditor editor, apMeshGenerator meshGenerator)
//		{
//			_editor = editor;
//			_meshGenerator = meshGenerator;
//			Clear();
//		}

//		public void Clear()
//		{
//			_controlPointArr_Quad = null;
//			_controlPointList_Quad.Clear();

//			_controlPointArr_ComplexQuad = null;
//			_controlPointList_ComplexQuad.Clear();

//			_controlPointArr_CircleOuter = null;
//			_controlPoint_CircleCenter = null;
//			_controlPointList_Circle.Clear();

//			_controlPointArr_RingOuter = null;
//			_controlPointArr_RingInner = null;
//			_controlPointList_Ring.Clear();

//			_isPointCreated = false;
//		}

//		// Functions
//		//-------------------------------------------------------------------------------
//		public void Make(Vector2 posLT, Vector2 posRB)
//		{
//			Clear();//혹시 모르니

//			Vector2 offset = Vector2.one * 5.0f;
//			_areaLT = new Vector2(Mathf.Min(posLT.x, posRB.x), Mathf.Min(posLT.y, posRB.y)) - offset;
//			_areaRB = new Vector2(Mathf.Max(posLT.x, posRB.x), Mathf.Max(posLT.y, posRB.y)) + offset;
//			_areaCenter = (_areaLT + _areaRB) * 0.5f;

//			_numPoint_ComplexQuadX = Mathf.Max(_editor._meshAutoGenOption_numControlPoint_ComplexQuad_X, 3);
//			_numPoint_ComplexQuadY = Mathf.Max(_editor._meshAutoGenOption_numControlPoint_ComplexQuad_Y, 3);
//			_numPoint_CircleRing = Mathf.Max(_editor._meshAutoGenOption_numControlPoint_CircleRing, 4);
			
//			MakeQuadPoints();
//			MakeCircleRingPoints();
//		}



//		public void RefreshOption()
//		{
//			//TODO : 속성만 바뀌었을 때
//			//최대한 Lerp 값을 이용해서 위치를 추적해서 포인트를 만들자
//		}
		



//		private void MakeQuadPoints()
//		{
//			_controlPointArr_Quad = new ControlPoint[3, 3];
//			_controlPointList_Quad.Clear();

//			_controlPointArr_ComplexQuad = new ControlPoint[_numPoint_ComplexQuadX, _numPoint_ComplexQuadY];
//			_controlPointList_ComplexQuad.Clear();

//			//일반 Quad
//			for (int iX = 0; iX < 3; iX++)
//			{
//				for (int iY = 0; iY < 3; iY++)
//				{
//					float lerpX = (float)iX / 2.0f;
//					float lerpY = (float)iY / 2.0f;
//					Vector2 pos = GetQuadLerpPos(lerpX, lerpY);
//					bool isOuter = (iX != 1) || (iY != 1);

//					ControlPoint newControlPoint = ControlPoint.MakeQuadPoint(pos, iX, iY, lerpX, lerpY, isOuter);
//					_controlPointArr_Quad[iX, iY] = newControlPoint;
//					_controlPointList_Quad.Add(newControlPoint);
//				}
//			}

//			//다중 Quad
//			for (int iX = 0; iX < _numPoint_ComplexQuadX; iX++)
//			{
//				float lerpX = (float)iX / (float)(_numPoint_ComplexQuadX - 1);
//				for (int iY = 0; iY < _numPoint_ComplexQuadY; iY++)
//				{
//					float lerpY = (float)iY / (float)(_numPoint_ComplexQuadY - 1);
//					Vector2 pos = GetQuadLerpPos(lerpX, lerpY);
//					bool isOuter = (iX == 0) || (iX == _numPoint_ComplexQuadX - 1)
//								|| (iY == 0) || (iY == _numPoint_ComplexQuadY - 1);

//					ControlPoint newControlPoint = ControlPoint.MakeQuadPoint(pos, iX, iY, lerpX, lerpY, isOuter);
//					_controlPointArr_ComplexQuad[iX, iY] = newControlPoint;
//					_controlPointList_ComplexQuad.Add(newControlPoint);
//				}
//			}

//			//GUI 용으로 연결을 하자
//			//Quad의 경우, +X, +Y 방향으로 연결을 한다.
//			ControlPoint curPoint = null;
//			ControlPoint nextPoint = null;
//			for (int iX = 0; iX < 3; iX++)
//			{
//				for (int iY = 0; iY < 3; iY++)
//				{
//					curPoint = _controlPointArr_Quad[iX, iY];
//					if(iX < 2)
//					{
//						nextPoint = _controlPointArr_Quad[iX + 1, iY];
//						if(nextPoint._isOuter && curPoint._isOuter)
//						{
//							curPoint._linkedOuterPoints.Add(nextPoint);
//						}
//						else
//						{
//							curPoint._linkedInnerOrCrossPoints.Add(nextPoint);
//						}
//					}
//					if(iY < 2)
//					{
//						nextPoint = _controlPointArr_Quad[iX, iY + 1];
//						if(nextPoint._isOuter && curPoint._isOuter)
//						{
//							curPoint._linkedOuterPoints.Add(nextPoint);
//						}
//						else
//						{
//							curPoint._linkedInnerOrCrossPoints.Add(nextPoint);
//						}
//					}
//				}
//			}

//			for (int iX = 0; iX < _numPoint_ComplexQuadX; iX++)
//			{
//				for (int iY = 0; iY < _numPoint_ComplexQuadY; iY++)
//				{
//					curPoint = _controlPointArr_ComplexQuad[iX, iY];
//					if(iX < _numPoint_ComplexQuadX - 1)
//					{
//						nextPoint = _controlPointArr_ComplexQuad[iX + 1, iY];
//						if(nextPoint._isOuter && curPoint._isOuter)
//						{
//							curPoint._linkedOuterPoints.Add(nextPoint);
//						}
//						else
//						{
//							curPoint._linkedInnerOrCrossPoints.Add(nextPoint);
//						}
//					}
//					if(iY < _numPoint_ComplexQuadY - 1)
//					{
//						nextPoint = _controlPointArr_ComplexQuad[iX, iY + 1];
//						if(nextPoint._isOuter && curPoint._isOuter)
//						{
//							curPoint._linkedOuterPoints.Add(nextPoint);
//						}
//						else
//						{
//							curPoint._linkedInnerOrCrossPoints.Add(nextPoint);
//						}
//					}
//				}
//			}
//		}



//		private void MakeCircleRingPoints()
//		{
//			_controlPointArr_CircleOuter = new ControlPoint[_numPoint_CircleRing];
//			_controlPoint_CircleCenter = null;
//			_controlPointList_Circle.Clear();

//			_controlPointArr_RingOuter = new ControlPoint[_numPoint_CircleRing];
//			_controlPointArr_RingInner = new ControlPoint[_numPoint_CircleRing];
//			_controlPointList_Ring.Clear();

//			float radius = Mathf.Max(Mathf.Abs(_areaLT.x - _areaRB.x) / 2.0f, Mathf.Abs(_areaLT.y - _areaRB.y) / 2.0f);
//			float radiusRingInner = radius / 2.0f;

//			for (int i = 0; i < _numPoint_CircleRing; i++)
//			{
//				float angleLerp = (float)i / (float)(_numPoint_CircleRing);
//				float angle = angleLerp * 360.0f;
//				ControlPoint newPoint_Circle = ControlPoint.MakeCircleOuterPoint(GetCircleLerpPos(angle, radius, _areaCenter), i, angle, angleLerp);
//				ControlPoint newPoint_OuterRing = ControlPoint.MakeCircleOuterPoint(GetCircleLerpPos(angle, radius, _areaCenter), i, angle, angleLerp);
//				ControlPoint newPoint_InnerRing = ControlPoint.MakeCircleOuterPoint(GetCircleLerpPos(angle, radiusRingInner, _areaCenter), i, angle, angleLerp);

//				_controlPointArr_CircleOuter[i] = newPoint_Circle;
//				_controlPointArr_RingOuter[i] = newPoint_OuterRing;
//				_controlPointArr_RingInner[i] = newPoint_InnerRing;

//				_controlPointList_Circle.Add(newPoint_Circle);
//				_controlPointList_Ring.Add(newPoint_OuterRing);
//				_controlPointList_Ring.Add(newPoint_InnerRing);
//			}

//			//Circle Center
//			ControlPoint newPoint_CircleCenter = ControlPoint.MakeCircleCenterPoint(_areaCenter);
//			_controlPoint_CircleCenter = newPoint_CircleCenter;
//			_controlPointList_Circle.Add(_controlPoint_CircleCenter);


//			//GUI 용으로 연결을 하자
//			//Circle의 경우, Index가 증가하는 방향으로 + Center로 Inner 연결
//			//Ring은 Outer->Inner
//			ControlPoint curPoint = null;
//			ControlPoint nextPoint = null;
			
//			for (int i = 0; i < _numPoint_CircleRing; i++)
//			{
//				//Circle 먼저
//				curPoint = _controlPointArr_CircleOuter[i];
//				nextPoint = _controlPointArr_CircleOuter[(i + 1) % _numPoint_CircleRing];
//				curPoint._linkedOuterPoints.Add(nextPoint);
//				curPoint._linkedInnerOrCrossPoints.Add(_controlPoint_CircleCenter);

//				//Ring
//				curPoint = _controlPointArr_RingOuter[i];
//				nextPoint = _controlPointArr_RingOuter[(i + 1) % _numPoint_CircleRing];
//				curPoint._linkedOuterPoints.Add(nextPoint);

//				nextPoint = _controlPointArr_RingInner[i];
//				curPoint._linkedInnerOrCrossPoints.Add(nextPoint);

//				curPoint = _controlPointArr_RingInner[i];
//				nextPoint = _controlPointArr_RingInner[(i + 1) % _numPoint_CircleRing];
//				curPoint._linkedOuterPoints.Add(nextPoint);
//			}
//		}




//		private Vector2 GetQuadLerpPos(float lerpX, float lerpY)
//		{
//			//float lerpX = (float)iX / (float)(nX - 1);
//			//float lerpY = (float)iY / (float)(nY - 1);

//			return new Vector2(
//				_areaLT.x * (1.0f - lerpX) + _areaRB.x * lerpX,
//				_areaLT.y * (1.0f - lerpY) + _areaRB.y * lerpY
//				);
//		}

//		private Vector2 GetCircleLerpPos(float angle, float radius, Vector2 center)
//		{
//			//float angle = ((float)iAngle / (float)(nAngle - 1)) * 360.0f;
//			return new Vector2(
//				(Mathf.Sin(angle * Mathf.Deg2Rad) * radius) + center.x,
//				(Mathf.Cos(angle * Mathf.Deg2Rad) * radius) + center.y
//				);
//		}

//		// Get / Set
//		//-------------------------------------------------------------------------------
//		public bool IsPointCreated {  get {  return _isPointCreated; } }
//		public List<ControlPoint> ControlPoints
//		{
//			get
//			{
//				switch (_shape)
//				{
//					case MapperShape.Quad:	return _controlPointList_Quad;
//					case MapperShape.ComplexQuad: return _controlPointList_ComplexQuad;
//					case MapperShape.Circle: return _controlPointList_Circle;
//					case MapperShape.Ring: return _controlPointList_Ring;
//				}
//				return null;
//			}
//		}

//		public ControlPoint[,] GetQuadControlPointArr(bool isComplex)
//		{
//			if(isComplex)
//			{
//				return _controlPointArr_ComplexQuad;
//			}
//			return _controlPointArr_Quad;
//		}

//		public ControlPoint[] GetCircleControlPointArr()
//		{
//			return _controlPointArr_CircleOuter;
//		}

//		public ControlPoint GetCircleCenterControlPoint()
//		{
//			return _controlPoint_CircleCenter;
//		}

//		public ControlPoint[] GetRingControlPointArr(bool isOuter)
//		{
//			if(isOuter)
//			{
//				return _controlPointArr_RingOuter;
//			}
			
//			return _controlPointArr_RingInner;
//		}
		
//		public List<ControlPoint> GetControlPointList(MapperShape shape)
//		{
//			switch (shape)
//			{
//				case MapperShape.Quad:	return _controlPointList_Quad;
//				case MapperShape.ComplexQuad: return _controlPointList_ComplexQuad;
//				case MapperShape.Circle: return _controlPointList_Circle;
//				case MapperShape.Ring: return _controlPointList_Ring;
//			}
//			return null;
//		}
		
//	}
//}