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
//	/// 메시를 자동으로 만들 때, 각종 파라미터와 단계가 필요하여 코드가 길어졌당.
//	/// 메시를 입력하면 마법사가 실행된다.
//	/// 3단계가 있으며 미리보기 등을 지원한다.
//	/// apEditor에 멤버로 포함된다.
//	/// </summary>
//	public class apMeshGenerator
//	{
//		// Members
//		//----------------------------------------------------
//		public apEditor _editor = null;
//		public enum STEP
//		{
//			Ready,
//			Scanned,
//			Previewed,
//			Completed
//		}
//		private STEP _step = STEP.Ready;
//		private apMesh _targetMesh = null;
//		private apTextureData _targetTextureData = null;
//		private Texture2D _targetImage = null;
//		private TextureImporter _textureImporter = null;


		
//		public enum TileType
//		{
//			Empty,
//			Filled,
//			Outline,
//		}
//		public class ScanTile
//		{
//			public int _iX = 0;
//			public int _iY = 0;
//			public int _posX = 0;
//			public int _posY = 0;
//			public TileType _tileType = TileType.Empty;

//			public List<ScanTile> _linkedOutlineTiles = null;
//			public ScanTile _linkedOutlineTile_Left = null;
//			public ScanTile _linkedOutlineTile_Right = null;
//			public ScanTile _linkedOutlineTile_Up = null;//Top를 +Y로 하자
//			public ScanTile _linkedOutlineTile_Down = null;//Bottom을 -Y로 하자

//			public ScanTileGroup _parentGroup = null;

//			public ScanTile(int iX, int iY, int posX, int posY)
//			{
//				_iX = iX;
//				_iY = iY;
//				_posX = posX;
//				_posY = posY;

//				_tileType = TileType.Empty;
//				_linkedOutlineTiles = null;
//				_linkedOutlineTile_Left = null;
//				_linkedOutlineTile_Right = null;
//				_linkedOutlineTile_Up = null;
//				_linkedOutlineTile_Down = null;
//				_parentGroup = null;
//			}

//			public void AddLinkedOutlineTile(ScanTile linkedTile)
//			{
//				if(_linkedOutlineTiles == null)
//				{
//					_linkedOutlineTiles = new List<ScanTile>();
//				}
//				_linkedOutlineTiles.Add(linkedTile);
//				if(linkedTile._iX == _iX - 1 && linkedTile._iY == _iY)
//				{
//					_linkedOutlineTile_Left = linkedTile;
//					linkedTile._linkedOutlineTile_Right = this;//서로 연결
//				}
//				else if(linkedTile._iX == _iX + 1 && linkedTile._iY == _iY)
//				{
//					_linkedOutlineTile_Right = linkedTile;
//					linkedTile._linkedOutlineTile_Left = this;//서로 연결
//				}
//				else if(linkedTile._iX == _iX && linkedTile._iY == _iY - 1)
//				{
//					_linkedOutlineTile_Down = linkedTile;
//					linkedTile._linkedOutlineTile_Up = this;//서로 연결
//				}
//				else if(linkedTile._iX == _iX && linkedTile._iY == _iY + 1)
//				{
//					_linkedOutlineTile_Up = linkedTile;
//					linkedTile._linkedOutlineTile_Down = this;//서로 연결
//				}
//			}
//		}

//		public ScanTile[,] _scanTiles = null;//(X, Y)
//		private int _scan_PosMinX = 0;
//		private int _scan_PosMaxX = 0;
//		private int _scan_PosMinY = 0;
//		private int _scan_PosMaxY = 0;
//		private int _scan_TileSize = 0;
//		private int _scan_NumTileX = 0;
//		private int _scan_NumTileY = 0;
//		private int _scan_Width = 0;
//		private int _scan_Height = 0;

		
//		public class OuterPoint
//		{
//			public int _iX = 0;
//			public int _iY = 0;
//			public Vector2 _pos;
//			public OuterPoint _prevPoint = null;
//			public OuterPoint _nextPoint = null;

//			public bool _isNeedShapeCheck = false;
//			public bool _isEmptyTile_LU = false;
//			public bool _isEmptyTile_RU = false;
//			public bool _isEmptyTile_LD = false;
//			public bool _isEmptyTile_RD = false;

//			//기울기
//			public Vector2 _slopeVec2_Prev = Vector2.zero;
//			public Vector2 _slopeVec2_Next = Vector2.zero;
//			public int _slopeY_Prev = 0;
//			public int _slopeX_Prev = 0;
//			public int _slopeY_Next = 0;
//			public int _slopeX_Next = 0;
//			public Vector2 _normal_Prev = Vector2.zero;
//			public Vector2 _normal_Next = Vector2.zero;
//			public Vector2 _normal_Avg = Vector2.zero;
//			public float _slopeAngle_Prev = 0.0f;
//			public float _slopeAngle_Next = 0.0f;
//			public float _slopeAngle_Avg = 0.0f;
//			public bool _isInversedNormal_Prev = false;
//			public bool _isInversedNormal_Next = false;

//			public bool _isSplitPoint = false;
			
			
//			public OuterPoint(OuterPoint src)
//			{
//				_isSplitPoint = false;
//				Copy(src);
//				//_pos += _normal_Avg.normalized * margin;//Margin 만큼 이동
//			}
//			public OuterPoint(int iX, int iY, Vector2 pos)
//			{
//				_iX = iX;
//				_iY = iY;
//				_pos = pos;
//				_prevPoint = null;
//				_nextPoint = null;
//				_isNeedShapeCheck = false;
//				_isSplitPoint = false;
//			}

//			public OuterPoint()
//			{

//			}


//			public void SetTileInformation(	bool isEmptyTile_LU, 
//											bool isEmptyTile_RU, 
//											bool isEmptyTile_LD, 
//											bool isEmptyTile_RD)
//			{
//				_isEmptyTile_LU = isEmptyTile_LU;
//				_isEmptyTile_RU = isEmptyTile_RU;
//				_isEmptyTile_LD = isEmptyTile_LD;
//				_isEmptyTile_RD = isEmptyTile_RD;
//			}

//			public void SetShapeCheckFlag()
//			{
//				//이게 True라면
//				//Linear / Concave / Convex 체크할 때
//				//타일만 할게 아니라 인접한 포인트와 별도로 체크를 해야한다.
//				_isNeedShapeCheck = true;
//			}

//			/// <summary>
//			/// 직선의 중간에 위치한 포인트인가
//			/// </summary>
//			/// <returns></returns>
//			public bool IsLinearPoint()
//			{
//				bool isLinearTile = (_isEmptyTile_LU && _isEmptyTile_RU && !_isEmptyTile_RD && !_isEmptyTile_LD)
//						|| (!_isEmptyTile_LU && _isEmptyTile_RU && _isEmptyTile_RD && !_isEmptyTile_LD)
//						|| (!_isEmptyTile_LU && !_isEmptyTile_RU && _isEmptyTile_RD && _isEmptyTile_LD)
//						|| (_isEmptyTile_LU && !_isEmptyTile_RU && !_isEmptyTile_RD && _isEmptyTile_LD);
//				if (!_isNeedShapeCheck)
//				{
//					return isLinearTile;
//				}
//				else
//				{
//					//인접한 것들과 체크해야한다.
//					//if(isLinearTile)
//					{
//						Vector2 deltaPos_Prev = _prevPoint._pos - _pos;
//						Vector2 deltaPos_Next = _nextPoint._pos - _pos;
//						float angleNormal2Prev = Vector2.Angle(_normal_Avg, deltaPos_Prev);
//						float angleNormal2Next = Vector2.Angle(_normal_Avg, deltaPos_Next);
//						float angleSum = angleNormal2Prev + angleNormal2Next;

//						float bias = 0.2f;
//						//if(
//						//	(Mathf.Abs(deltaPos_Prev.x) < bias || Mathf.Abs(deltaPos_Prev.y) < bias)
//						//	&& (Mathf.Abs(deltaPos_Next.x) < bias || Mathf.Abs(deltaPos_Next.y) < bias)
//						//	)
//						//{
//						//	return true;
//						//}
//						if(angleSum > 180.0f - bias && angleSum < 180.0f + bias)
//						{
//							return true;
//						}
//					}
//					return false;

//				}
//			}

//			/// <summary>
//			/// 오목한 포인트인가 (1면만 Empty)
//			/// </summary>
//			/// <returns></returns>
//			public bool IsConcavePoint()
//			{
//				if (!_isNeedShapeCheck)
//				{
//					return (_isEmptyTile_LU && !_isEmptyTile_RU && !_isEmptyTile_RD && !_isEmptyTile_LD)
//						|| (!_isEmptyTile_LU && _isEmptyTile_RU && !_isEmptyTile_RD && !_isEmptyTile_LD)
//						|| (!_isEmptyTile_LU && !_isEmptyTile_RU && _isEmptyTile_RD && !_isEmptyTile_LD)
//						|| (!_isEmptyTile_LU && !_isEmptyTile_RU && !_isEmptyTile_RD && _isEmptyTile_LD);
//				}
//				else
//				{
//					float angleNormalToNext = Vector2.Angle(_normal_Avg, _nextPoint._pos - _pos);
//					float angleNormalToPrev = Vector2.Angle(_normal_Avg, _prevPoint._pos - _pos);
//					return (angleNormalToNext + angleNormalToPrev) < 180.0f;
//				}
//			}

//			/// <summary>
//			/// 볼록한 포인트인가 (3면이 Empty)
//			/// </summary>
//			/// <returns></returns>
//			public bool IsConvexPoint()
//			{
//				if (!_isNeedShapeCheck)
//				{
//					return (!_isEmptyTile_LU && _isEmptyTile_RU && _isEmptyTile_RD && _isEmptyTile_LD)
//						|| (_isEmptyTile_LU && !_isEmptyTile_RU && _isEmptyTile_RD && _isEmptyTile_LD)
//						|| (_isEmptyTile_LU && _isEmptyTile_RU && !_isEmptyTile_RD && _isEmptyTile_LD)
//						|| (_isEmptyTile_LU && _isEmptyTile_RU && _isEmptyTile_RD && !_isEmptyTile_LD);
//				}
//				else
//				{
//					float angleNormalToNext = Vector2.Angle(_normal_Avg, _nextPoint._pos - _pos);
//					float angleNormalToPrev = Vector2.Angle(_normal_Avg, _prevPoint._pos - _pos);
//					return (angleNormalToNext + angleNormalToPrev) > 180.0f;
//				}
//			}


//			public void Copy(OuterPoint src)
//			{
//				_iX = src._iX;
//				_iY = src._iY;
//				_pos = src._pos;
				

//				_prevPoint = null;
//				_nextPoint = null;

//				_isNeedShapeCheck = src._isNeedShapeCheck;
//				_isEmptyTile_LU = src._isEmptyTile_LU;
//				_isEmptyTile_RU = src._isEmptyTile_RU;
//				_isEmptyTile_LD = src._isEmptyTile_LD;
//				_isEmptyTile_RD = src._isEmptyTile_RD;

//				_slopeVec2_Prev = src._slopeVec2_Prev;
//				_slopeVec2_Next = src._slopeVec2_Next;
//				_slopeY_Prev = src._slopeY_Prev;
//				_slopeX_Prev = src._slopeX_Prev;
//				_slopeY_Next = src._slopeY_Next;
//				_slopeX_Next = src._slopeX_Next;
//				_normal_Prev = src._normal_Prev;
//				_normal_Next = src._normal_Next;
//				_normal_Avg = src._normal_Avg;
//				_slopeAngle_Prev = src._slopeAngle_Prev;
//				_slopeAngle_Next = src._slopeAngle_Next;
//				_slopeAngle_Avg = src._slopeAngle_Avg;
//				_isInversedNormal_Prev = src._isInversedNormal_Prev;
//				_isInversedNormal_Next = src._isInversedNormal_Next;
//			}

//			public void CopyLink(OuterPoint src, Dictionary<OuterPoint, OuterPoint> mappingList)
//			{
//				_prevPoint = src._prevPoint != null ? mappingList[src._prevPoint] : null;
//				_nextPoint = src._nextPoint != null ? mappingList[src._nextPoint] : null;
//			}

//			public void SetSplit(OuterPoint pointA, OuterPoint pointB, int iSplit, int nSplit)
//			{	
//				float lerp = (float)(iSplit + 1) / (float)(nSplit + 1);
//				_pos = pointA._pos * (1.0f - lerp) + pointB._pos * lerp;
//				_normal_Avg = pointA._normal_Avg * (1.0f - lerp) + pointB._normal_Avg * lerp;
//				_isNeedShapeCheck = true;
//				_isSplitPoint = true;
//			}
			
//		}


//		public class ScanTileGroup
//		{
//			public List<ScanTile> _tiles = new List<ScanTile>();
//			public List<OuterPoint> _outerPoints = new List<OuterPoint>();
//			public bool _isEnabled = true;//<<이 그룹을 사용할 것인가

//			public ScanTileGroup()
//			{
//				_tiles.Clear();
//				_outerPoints.Clear();
//				_isEnabled = true;
//			}

//			public void AddTile(ScanTile tile)
//			{
//				_tiles.Add(tile);
//				tile._parentGroup = this;
//			}
//			public bool IsContains(ScanTile tile)
//			{
//				return _tiles.Contains(tile);
//			}
//			public Vector4 GetLTRB()
//			{	
//				if(_outerPoints.Count == 0)
//				{
//					return Vector4.zero;
//				}
//				Vector4 ltrb = Vector4.zero;
//				ltrb.x = _outerPoints[0]._pos.x;
//				ltrb.y = _outerPoints[0]._pos.y;
//				ltrb.z = _outerPoints[0]._pos.x;
//				ltrb.w = _outerPoints[0]._pos.y;

//				OuterPoint curOutPoint = null;
//				for (int i = 1; i < _outerPoints.Count; i++)
//				{
//					curOutPoint = _outerPoints[i];
//					ltrb.x = Mathf.Min(ltrb.x, curOutPoint._pos.x);//L
//					ltrb.y = Mathf.Min(ltrb.y, curOutPoint._pos.y);//T
//					ltrb.z = Mathf.Max(ltrb.z, curOutPoint._pos.x);//R
//					ltrb.w = Mathf.Max(ltrb.w, curOutPoint._pos.y);//B
//				}


//				//mesh._offsetPos + imageHalfOffset
				
//				return ltrb;
//			}
//		}
//		public List<ScanTileGroup> _outlineGroups = new List<ScanTileGroup>();
//		public List<OuterPoint> _outerPoints_Preview = new List<OuterPoint>();

//		public enum INNER_MOVE_LOCK
//		{
//			None,
//			AxisLimited,
//			Locked
//		}

//		public class InnerPoint
//		{
//			public Vector2 _pos = Vector2.zero;
//			public float _lerp_Axis1 = 0.0f;
//			public float _lerp_Axis2 = 0.0f;
//			public Vector2 _normalIfOuter = Vector2.zero;//외곽의 InnerPoint인 경우의 Normal
//			public float _properDistIfOuter = 0.0f;//외곽의 InnerPoint인 경우, Normal 방향으로 OuterPoint와의 적절한 거리 (너무 멀어도 안되고, 가까워도 안된다)
//			public bool _isOuter = false;
//			public bool _isDirToInnerRadius = false;

//			public List<InnerPoint> _linkedPoint = new List<InnerPoint>();
//			public List<InnerPoint> _linkedPoint_GUI = new List<InnerPoint>();//<<GUI용. 일부만 저장된다.
//			public List<OuterPoint> _linkedOuterPoints = new List<OuterPoint>();

//			public Vector2 _relaxForce = Vector2.zero;
//			//Relax 계산 우선순위. 숫자가 낮을 수록 먼저 계산된다. 외곽선일 수록 먼저 계산된다.
//			//시작값은 1(0은 값이 적용되지 않았다)
//			public int _priority = 0;

//			public List<InnerPoint> _linkedPoint_EndOut_Axis1 = new List<InnerPoint>();
//			public List<InnerPoint> _linkedPoint_EndOut_Axis2 = new List<InnerPoint>();

//			//외곽에서의 위치보정
//			public bool _isNeedCorrectOuterPos = false;
//			public Vector2 _correctedPos = Vector2.zero;

//			//Releax 제한
//			public INNER_MOVE_LOCK _moveLock = INNER_MOVE_LOCK.None;
//			public Vector2 _limitedPosA = Vector2.zero;
//			public Vector2 _limitedPosB = Vector2.zero;
			

//			public InnerPoint(Vector2 pos, float lerpAxis1, float lerpAxis2)
//			{
//				_pos = pos;
//				_lerp_Axis1 = lerpAxis1;
//				_lerp_Axis2 = lerpAxis2;
//				_linkedPoint.Clear();
//				_linkedPoint_EndOut_Axis1.Clear();
//				_linkedPoint_EndOut_Axis2.Clear();
//				_linkedPoint_GUI.Clear();
//				_linkedOuterPoints.Clear();
//				_priority = 0;

//				_moveLock = INNER_MOVE_LOCK.None;
//			}
//			//public void LinkPoint(InnerPoint innerPoint)
//			//{
//			//	if(!_linkedPoint.Contains(innerPoint))
//			//	{
//			//		_linkedPoint.Add(innerPoint);
//			//	}
//			//}
//		}
//		public List<InnerPoint> _innerPoints = new List<InnerPoint>();
//		public int _maxInnerPointPriority = 0;


//		//Mesh Gen Mapper
//		private apMeshGenMapper _mapper = null;
//		public apMeshGenMapper Mapper {  get {  return _mapper; } }


//		/// <summary>작업 중일 때 Gizmo에서 제어하고 있는 컨트롤 포인트들</summary>
//		public List<apMeshGenMapper.ControlPoint> _selectedControlPoints = new List<apMeshGenMapper.ControlPoint>();
//		/// <summary>
//		/// Scan 단계에서 선택한 Outer Group
//		/// 여기에서 선택된 OuterGroup의 Normal을 전환하거나 삭제할 수 있다.
//		/// </summary>
//		public ScanTileGroup _selectedOuterGroup = null;
//		public int _selectedOuterGroupIndex = -1;


//		//Mesh 를 만들때 사용되는 임시 클래스
//		public class RawDataVertexPair
//		{
//			public apVertex _vert = null;
//			public OuterPoint _outerPoint = null;
//			public InnerPoint _innerPoint = null;
//			public List<RawDataVertexPair> _nextPairs = new List<RawDataVertexPair>();

//			public RawDataVertexPair(apVertex vert, OuterPoint outPoint)
//			{
//				_vert = vert;
//				_outerPoint = outPoint;
//				_innerPoint = null;
//				_nextPairs.Clear();
//			}

//			public RawDataVertexPair(apVertex vert, InnerPoint inPoint)
//			{
//				_vert = vert;
//				_outerPoint = null;
//				_innerPoint = inPoint;
//				_nextPairs.Clear();
//			}
//		}

//		// Init
//		//----------------------------------------------------
//		public apMeshGenerator(apEditor editor)
//		{
//			_editor = editor;
//			_mapper = new apMeshGenMapper(editor, this);
//			Clear();
//		}

//		public void Clear()
//		{
//			_step = STEP.Ready;
//			_targetMesh = null;
//			_targetTextureData = null;
//			_targetImage = null;
//			_textureImporter = null;
//			_scanTiles = null;

//			Mapper.Clear();
//			_innerPoints.Clear();
//			_outerPoints_Preview.Clear();
//			_selectedControlPoints.Clear();
//			_selectedOuterGroup = null;
//			_selectedOuterGroupIndex = -1;
//		}

//		// Functions
//		//----------------------------------------------------

//		public void CheckAndSetMesh(apMesh mesh)
//		{
//			if (_targetMesh == mesh)
//			{
//				//동일하다면 그대로 한다.
//				//이미지 설정은 확인을 하자
//				bool isAnyTextureChanged = false;
//				if (_targetTextureData != mesh._textureData_Linked)
//				{
//					isAnyTextureChanged = true;
//				}
//				else
//				{
//					if (_targetImage != _targetTextureData._image)
//					{
//						isAnyTextureChanged = true;
//					}
//				}

//				if (isAnyTextureChanged)
//				{
//					_targetTextureData = null;
//					_targetImage = null;
//					_textureImporter = null;

//					_targetTextureData = mesh._textureData_Linked;
//					if (_targetTextureData != null)
//					{
//						_targetImage = _targetTextureData._image;
//					}
//					if (_targetImage != null)
//					{
//						string path = AssetDatabase.GetAssetPath(_targetImage);
//						_textureImporter = TextureImporter.GetAtPath(path) as TextureImporter;
//					}
//				}

//				return;
//			}
//			Clear();
//			_targetMesh = mesh;
//			_targetTextureData = mesh._textureData_Linked;
//			if (_targetTextureData != null)
//			{
//				_targetImage = _targetTextureData._image;
//			}
//			if (_targetImage != null)
//			{
//				string path = AssetDatabase.GetAssetPath(_targetImage);
//				_textureImporter = TextureImporter.GetAtPath(path) as TextureImporter;

//			}

//		}

//		public void SetTextureReadWriteEnableToggle()
//		{
//			if (_textureImporter == null)
//			{
//				return;
//			}
//			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Image_SettingChanged, _editor, _editor._portrait, _targetTextureData, false);
//			_textureImporter.isReadable = !_textureImporter.isReadable;
//			_textureImporter.SaveAndReimport();
//			AssetDatabase.Refresh();
//		}

//		//---------------------------------------------------------------

//		//단계 1 - 스캔
//		public void Step1_Scan()
//		{
//			if(!IsScanable())
//			{
//				return;
//			}
//			//스캔해봅시다.
//			try
//			{
//				//Vector2 offset = new Vector2(_targetTextureData._width * 0.5f, _targetTextureData._height * 0.5f) - _targetMesh._offsetPos;
//				Vector2 offset = new Vector2(_targetTextureData._width * 0.5f, _targetTextureData._height * 0.5f);
//				//1. 타일 생성
//				_scan_PosMinX = (int)(Mathf.Min(_targetMesh._atlasFromPSD_LT.x, _targetMesh._atlasFromPSD_RB.x) + offset.x);
//				_scan_PosMaxX = (int)(Mathf.Max(_targetMesh._atlasFromPSD_LT.x, _targetMesh._atlasFromPSD_RB.x) + 0.5f + offset.x);
//				_scan_PosMinY = (int)(Mathf.Min(_targetMesh._atlasFromPSD_LT.y, _targetMesh._atlasFromPSD_RB.y) + offset.y);
//				_scan_PosMaxY = (int)(Mathf.Max(_targetMesh._atlasFromPSD_LT.y, _targetMesh._atlasFromPSD_RB.y) + 0.5f + offset.y);

				
//				_scan_Width = (_scan_PosMaxX - _scan_PosMinX);
//				_scan_Height = (_scan_PosMaxY - _scan_PosMinY);

//				_scan_TileSize = 0;

//				_scan_TileSize = Mathf.Min(5, _scan_Width / 20, _scan_Height / 20);
//				if (_scan_TileSize < 2)
//				{
//					_scan_TileSize = 2;
//				}

//				_scan_NumTileX = _scan_Width / _scan_TileSize;
//				_scan_NumTileY = _scan_Height / _scan_TileSize;
//				//if (_scan_Width % _scan_TileSize != 0)
//				//{
//				//	//딱 나누어 떨어지지 않는다면 한칸 더
//				//	_scan_NumTileX++;
//				//}
//				//if (_scan_Height % _scan_TileSize != 0)
//				//{
//				//	//딱 나누어 떨어지지 않는다면 한칸 더
//				//	_scan_NumTileY++;
//				//}

//				_scanTiles = new ScanTile[_scan_NumTileX, _scan_NumTileY];

//				for (int iX = 0; iX < _scan_NumTileX; iX++)
//				{
//					for (int iY = 0; iY < _scan_NumTileY; iY++)
//					{
//						_scanTiles[iX, iY] = new ScanTile(
//							iX, iY,
//							(iX * _scan_TileSize) + _scan_PosMinX,
//							(iY * _scan_TileSize) + _scan_PosMinY);
//					}
//				}

//				//2. 투명도 비교하여 비어있는 타일인지 색이 있는지 비교
//				ScanTile curTile = null;
//				Color curColor;
//				int halfSize = _scan_TileSize / 2;
				

//				//float alphaOffset = 2.0f / 256.0f;//
//				float alphaOffset = _editor._meshAutoGenOption_AlphaCutOff;

//				bool isChecked = false;
//				for (int iX = 0; iX < _scan_NumTileX; iX++)
//				{
//					for (int iY = 0; iY < _scan_NumTileY; iY++)
//					{
//						curTile = _scanTiles[iX, iY];
//						//(1) 중심 위치 체크
//						curColor = _targetImage.GetPixel(	curTile._posX + halfSize,
//															curTile._posY + halfSize
//															);

//						if(curColor.a > alphaOffset)
//						{
//							curTile._tileType = TileType.Filled;//중심점에 채색이 되어있다.
//							continue;
//						}

//						//(2) 만약 비어 있다면 : 2px 간격으로 다시 체크
//						isChecked = false;
//						for (int subX = 0; subX < _scan_TileSize; subX += 2)
//						{
//							for (int subY = 0; subY < _scan_TileSize; subY += 2)
//							{
//								curColor = _targetImage.GetPixel(	curTile._posX + subX,
//																	curTile._posY + subY);

//								if (curColor.a > alphaOffset)
//								{
//									curTile._tileType = TileType.Filled;//중심점에 채색이 되어있다.
//									isChecked = true;
//									break;
//								}
//							}
//							if(isChecked)
//							{
//								break;
//							}
//						}
//					}
//				}

//				//추가
//				//만약 Empty 중에서 옆에 Filled가 있는 경우에는 1px 단위로 다시 체크를 하자
//				for (int iX = 0; iX < _scan_NumTileX; iX++)
//				{
//					for (int iY = 0; iY < _scan_NumTileY; iY++)
//					{
//						curTile = _scanTiles[iX, iY];
//						if(curTile._tileType != TileType.Empty)
//						{
//							continue;
//						}
//						bool isLeftFilled = (iX >= 1 ? _scanTiles[iX - 1, iY]._tileType == TileType.Filled : false);
//						bool isRightFilled = (iX < _scan_NumTileX - 1 ? _scanTiles[iX + 1, iY]._tileType == TileType.Filled : false);
//						bool isUpFilled = (iY >= 1 ? _scanTiles[iX, iY - 1]._tileType == TileType.Filled : false);
//						bool isDownFilled = (iY < _scan_NumTileY - 1 ? _scanTiles[iX, iY + 1]._tileType == TileType.Filled : false);

//						if (isLeftFilled || isRightFilled || isUpFilled || isDownFilled)
//						{
//							//1픽셀 간격으로 체크하자
//							isChecked = false;
//							for (int subX = 0; subX < _scan_TileSize; subX++)
//							{
//								for (int subY = 0; subY < _scan_TileSize; subY++)
//								{
//									curColor = _targetImage.GetPixel(curTile._posX + subX,
//																		curTile._posY + subY);

//									if (curColor.a > alphaOffset)
//									{
//										curTile._tileType = TileType.Filled;//중심점에 채색이 되어있다.
//										isChecked = true;
//										break;
//									}
//								}
//								if (isChecked)
//								{
//									break;
//								}
//							}
//						}
//					}
//				}

//				//Filled / Empty 타입으로 나뉘었다.
//				//3. Filled 타입 중에서 주변이 없거나 Empty 타입이면 Outline으로 설정한다.

//				List<ScanTile> outlineTiles = new List<ScanTile>();
//				List<ScanTile> remainedOutlineTiles = new List<ScanTile>();

//				for (int iX = 0; iX < _scan_NumTileX; iX++)
//				{
//					for (int iY = 0; iY < _scan_NumTileY; iY++)
//					{
//						curTile = _scanTiles[iX, iY];
//						if(curTile._tileType != TileType.Filled)
//						{
//							continue;
//						}

//						//외곽이라면 Outline 설정
//						if(iX == 0 || iX == _scan_NumTileX - 1 ||
//							iY == 0 || iY == _scan_NumTileY - 1)
//						{	
//							curTile._tileType = TileType.Outline;
//							outlineTiles.Add(curTile);
//							continue;
//						}

//						//-X, +X, -Y, +Y (대각 포함) 8개의 경우 하나라도 Empty라면 Outline
//						if(	_scanTiles[iX - 1	,	iY - 1	]._tileType == TileType.Empty ||
//							_scanTiles[iX - 1	,	iY		]._tileType == TileType.Empty ||
//							_scanTiles[iX - 1	,	iY + 1	]._tileType == TileType.Empty ||
//							_scanTiles[iX		,	iY - 1	]._tileType == TileType.Empty ||
//							_scanTiles[iX		,	iY		]._tileType == TileType.Empty ||
//							_scanTiles[iX		,	iY + 1	]._tileType == TileType.Empty ||
//							_scanTiles[iX + 1	,	iY - 1	]._tileType == TileType.Empty ||
//							_scanTiles[iX + 1	,	iY		]._tileType == TileType.Empty ||
//							_scanTiles[iX + 1	,	iY + 1	]._tileType == TileType.Empty)
//						{
//							curTile._tileType = TileType.Outline;
//							outlineTiles.Add(curTile);
//							continue;
//						}
//					}
//				}

//				//4. Outline 리스트를 돌면서 일단 인접한 타일의 정보를 넣어주자
//				ScanTile leftTile = null;
//				ScanTile rightTile = null;
//				ScanTile upTile = null;
//				ScanTile downTile = null;
//				for (int i = 0; i < outlineTiles.Count; i++)
//				{
//					curTile = outlineTiles[i];
//					remainedOutlineTiles.Add(curTile);

//					//상/하/좌/우를 살피자
//					if(curTile._iX >= 1)
//					{
//						leftTile = _scanTiles[curTile._iX - 1, curTile._iY];
//						if(leftTile._tileType == TileType.Outline)
//						{
//							curTile.AddLinkedOutlineTile(leftTile);
//						}
//					}
//					if(curTile._iX < _scan_NumTileX - 1)
//					{
//						rightTile = _scanTiles[curTile._iX + 1, curTile._iY];
//						if(rightTile._tileType == TileType.Outline)
//						{
//							curTile.AddLinkedOutlineTile(rightTile);
//						}
//					}
//					if(curTile._iY >= 1)
//					{
//						downTile = _scanTiles[curTile._iX, curTile._iY - 1];
//						if(downTile._tileType == TileType.Outline)
//						{
//							curTile.AddLinkedOutlineTile(downTile);
//						}
//					}
//					if(curTile._iY < _scan_NumTileY - 1)
//					{
//						upTile = _scanTiles[curTile._iX, curTile._iY + 1];
//						if(upTile._tileType == TileType.Outline)
//						{
//							curTile.AddLinkedOutlineTile(upTile);
//						}
//					}
//				}

//				//5. Outline을 돌면서 인접한 것끼리 연결을 하고 그룹을 짓자
//				//Recursive로 하면 오류가 날 것
//				//Pop 방식으로 처리
//				ScanTile startTile = null;
//				_outlineGroups.Clear();

//				List<ScanTile> linkedTiles = new List<ScanTile>();
//				List<ScanTile> processedTiles = new List<ScanTile>();
//				ScanTile linkTile = null;

//				while(remainedOutlineTiles.Count > 0)
//				{
//					//하나를 꺼낸다.
//					startTile = remainedOutlineTiles[0];
//					remainedOutlineTiles.RemoveAt(0);

//					//처음 꺼낸걸 새로운 그룹에 넣는다.
//					ScanTileGroup curGroup = new ScanTileGroup();
//					_outlineGroups.Add(curGroup);
//					curGroup.AddTile(startTile);

//					processedTiles.Add(startTile);
					
//					if (startTile._linkedOutlineTiles != null)
//					{
//						for (int iLink = 0; iLink < startTile._linkedOutlineTiles.Count; iLink++)
//						{
//							linkTile = startTile._linkedOutlineTiles[iLink];
//							if (!processedTiles.Contains(linkTile)
//								&& !linkedTiles.Contains(linkTile))
//							{
//								linkedTiles.Add(linkTile);
//							}
//						}
//					}
					
//					//이제 하나씩 연결된 것을 증가/감소 하면서 Group에 추가하자
//					while(linkedTiles.Count > 0)
//					{
//						//연결된 리스트 중 하나를 꺼낸다.
//						curTile = linkedTiles[0];
//						linkedTiles.RemoveAt(0);

//						//처리 했으니
//						processedTiles.Add(curTile);
//						remainedOutlineTiles.Remove(curTile);

//						//그룹에 넣고
//						curGroup.AddTile(curTile);

//						//서브 리스트를 다시 생성
//						if (curTile._linkedOutlineTiles != null)
//						{
//							for (int iLink = 0; iLink < curTile._linkedOutlineTiles.Count; iLink++)
//							{
//								linkTile = curTile._linkedOutlineTiles[iLink];
//								if (!processedTiles.Contains(linkTile)
//									&& !linkedTiles.Contains(linkTile))
//								{
//									linkedTiles.Add(linkTile);
//								}
//							}
//						}
//					}
//				}

				
				
//				//6. 점 + 외곽선을 만들어주자.
//				//먼저 사각형 점을 기준으로 변화량이 바뀌는 부분에 점을 만들자.
//				for (int iGroup = 0; iGroup < _outlineGroups.Count; iGroup++)
//				{
//					ScanTileGroup curGroup = _outlineGroups[iGroup];
//					if(curGroup._tiles.Count == 0)
//					{	
//						continue;
//					}

//					curTile = curGroup._tiles[0];
//					//LT, RT, LB, RB 중에서 가능한거 하나 더 찾자

//					int iCurPointX = -1;
//					int iCurPointY = -1;
//					if(IsOuterPoint(curTile._iX, curTile._iY))
//					{
//						//왼쪽 위
//						iCurPointX = curTile._iX;
//						iCurPointY = curTile._iY;
//					}
//					else if(IsOuterPoint(curTile._iX + 1, curTile._iY))
//					{
//						//오른쪽 위
//						iCurPointX = curTile._iX + 1;
//						iCurPointY = curTile._iY;
//					}
//					else if(IsOuterPoint(curTile._iX, curTile._iY + 1))
//					{
//						//왼쪽 아래
//						iCurPointX = curTile._iX;
//						iCurPointY = curTile._iY + 1;
//					}
//					else if(IsOuterPoint(curTile._iX + 1, curTile._iY + 1))
//					{
//						//오른쪽 아래
//						iCurPointX = curTile._iX + 1;
//						iCurPointY = curTile._iY + 1;
//					}
//					else
//					{
//						//이 루틴은 실패당
//						continue;
//					}

//					//시작점은 추가되었다.
//					OuterPoint startPoint = new OuterPoint(iCurPointX, iCurPointY, GetTilePos(iCurPointX, iCurPointY));
//					SetTileInformationToPoint(startPoint);
//					curGroup._outerPoints.Add(startPoint);

//					OuterPoint prevPoint = null;
//					OuterPoint curPoint = startPoint;
//					OuterPoint nextPoint = null;

//					while(true)
//					{
//						//이제 계속 순회하면서 체크한다.
//						//- 체크 순서는 상 -> 우 -> 하 -> 좌
//						//- Prev가 있을 때, Prev와 같은 방향은 무시
//						//- 그 방향으로 한면은 Outline Tile, 다른 면은 Empty Tile이어야 한다.
//						//- 그 방향으로 이동했을 때, 범위를 벗어난다면 패스
//						//- 성공한 결과는 무조건 1개여야 한다. (0개이면 에러)
//						//- 처리 후 한칸 이동. 도착 지점이 startPoint라면 종결한다.

//						nextPoint = null;
//						//1) Top
//						if (IsOuterPointPair(curPoint._iX, curPoint._iY, curPoint._iX, curPoint._iY - 1)
//							&& !(prevPoint != null && prevPoint._iY < curPoint._iY))
//						{
//							//위쪽으로 설정가능
//							if(startPoint._iX == curPoint._iX && startPoint._iY == curPoint._iY - 1)
//							{
//								nextPoint = startPoint;
//							}
//							else
//							{
//								nextPoint = new OuterPoint(curPoint._iX, curPoint._iY - 1, GetTilePos(curPoint._iX, curPoint._iY - 1));
//							}
							
//						}
//						else if (IsOuterPointPair(curPoint._iX, curPoint._iY, curPoint._iX + 1, curPoint._iY)
//							&& !(prevPoint != null && prevPoint._iX > curPoint._iX))
//						{
//							//오른쪽으로 설정가능
//							if (startPoint._iX == curPoint._iX + 1 && startPoint._iY == curPoint._iY)
//							{
//								nextPoint = startPoint;
//							}
//							else
//							{
//								nextPoint = new OuterPoint(curPoint._iX + 1, curPoint._iY, GetTilePos(curPoint._iX + 1, curPoint._iY));
//							}
//						}
//						else if (IsOuterPointPair(curPoint._iX, curPoint._iY, curPoint._iX, curPoint._iY + 1)
//							&& !(prevPoint != null && prevPoint._iY > curPoint._iY))
//						{
//							//아래로 설정가능
//							if (startPoint._iX == curPoint._iX && startPoint._iY == curPoint._iY + 1)
//							{
//								nextPoint = startPoint;
//							}
//							else
//							{
//								nextPoint = new OuterPoint(curPoint._iX, curPoint._iY + 1, GetTilePos(curPoint._iX, curPoint._iY + 1));
//							}
//						}
//						else if (IsOuterPointPair(curPoint._iX, curPoint._iY, curPoint._iX - 1, curPoint._iY)
//							&& !(prevPoint != null && prevPoint._iX < curPoint._iX))
//						{
//							//왼쪽으로 설정가능
//							if (startPoint._iX == curPoint._iX - 1 && startPoint._iY == curPoint._iY)
//							{
//								nextPoint = startPoint;
//							}
//							else
//							{
//								nextPoint = new OuterPoint(curPoint._iX - 1, curPoint._iY, GetTilePos(curPoint._iX - 1, curPoint._iY));
//							}
//						}
//						else
//						{
//							break;
//						}

//						//새로운 포인트와 지금 포인트를 연결
//						curPoint._nextPoint = nextPoint;
//						nextPoint._prevPoint = curPoint;

//						//리스트에 추가
//						if (nextPoint != startPoint)
//						{
//							curGroup._outerPoints.Add(nextPoint);
//							SetTileInformationToPoint(nextPoint);
//						}

//						prevPoint = curPoint;
//						curPoint = nextPoint;

//						if(curPoint == startPoint)
//						{
//							//한바퀴 도착
//							break;
//						}

//					}
//				}

				
//				//7. 외곽선을 이루는 점 중에서 불필요한 점을 삭제한다.
//				//루틴0) Concave 중에서 1칸짜리가 있다면 삭제한다.
//				//루틴1) 직선 점은 삭제한다.
//				//루틴2) 오목 코너를 찾아서 생략 가능한지 체크하자
//				//루틴3) 각각의 선분의 기울기(float Vector와 index X/Y차이)와 Normal을 체크한다. (Normal은 외곽을 향하도록 한다)
//				//루틴4) Normal 차이가 큰 "상대적 볼록 Vertex"를 시작점으로 하여, 비슷한 기울기를 가지며 상대적으로 가까운 버텍스들을 묶는다.
//				int nRemoved = 0;
//				for (int iGroup = 0; iGroup < _outlineGroups.Count; iGroup++)
//				{
//					ScanTileGroup curGroup = _outlineGroups[iGroup];
//					if(curGroup._outerPoints.Count == 0)
//					{
//						continue;
//					}

//					OuterPoint startPoint = null;//<<Start Point는 Linear Point가 아니어야 한다.
//					OuterPoint curPoint = null;
//					OuterPoint nextPoint = null;

//					nRemoved = 0;

//					bool isRemoved = false;

//					//-----------------------------------------------------------------------------
//					//루틴 전)
//					//기본 Normal을 계산한다.
//					//이 Normal을 기준으로 갱신을 한다. (여기서는 타일값을 기준으로 Normal을 만든ㄷ)
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						CalculatePointSlopeAndNormal(curPoint);
//					}

//					//-----------------------------------------------------------------------------
//					//루틴0) Concave 중에서 1칸짜리가 있다면 삭제한다.
//					//여기서 상호 연결된 포인트들은 단순 속성 비교를 하면 안된다.
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						if(!curPoint.IsConcavePoint())
//						{
//							startPoint = curPoint;
//							break;
//						}
//					}
//					if (startPoint != null)
//					{
//						curPoint = startPoint;
//						//bool isRemoved = false;
//						while (true)
//						{
//							isRemoved = false;
//							if (curPoint.IsConcavePoint())
//							{
//								//오목 버텍스인 경우
//								if(!curPoint._prevPoint.IsConcavePoint()
//									&& !curPoint._nextPoint.IsConcavePoint())
//								{
//									//양쪽은 오목 버텍스가 아닐 때
//									int prevDist = GetPointDistanceMHT(curPoint._prevPoint, curPoint);
//									int nextDist = GetPointDistanceMHT(curPoint._nextPoint, curPoint);
//									if(prevDist <= 2 && nextDist <= 2)
//									{
//										//이전과 이후를 연결할 수 있는지 체크
//										//if(IsLineTroughEmptyTiles(curPoint._prevPoint._pos, curPoint._nextPoint._pos))
//										{
//											isRemoved = true;
//										}
//									}
//								}
//							}

//							if(isRemoved)
//							{
//								curPoint._prevPoint._nextPoint = curPoint._nextPoint;
//								curPoint._nextPoint._prevPoint = curPoint._prevPoint;

//								curPoint._nextPoint.SetShapeCheckFlag();
//								curPoint._prevPoint.SetShapeCheckFlag();

//								nextPoint = curPoint._nextPoint;

//								//포인트를 삭제한다.
//								curGroup._outerPoints.Remove(curPoint);
//								nRemoved++;

//								curPoint = nextPoint;
//							}
//							else
//							{
//								nextPoint = curPoint._nextPoint;
//								curPoint = nextPoint;
//							}

//							if (nextPoint == startPoint)
//							{
//								//한바퀴 다 돌았다.
//								break;
//							}
//						}

						
//					}


//					//-----------------------------------------------------------------------------
//					//루틴1) 직선 점은 삭제한다.
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						if(!curPoint.IsLinearPoint())
//						{
//							startPoint = curPoint;
//							break;
//						}
//					}
//					if(startPoint == null)
//					{
//						//? 뭐지 최적화를 할수가 없는뎅?
//						continue;
//					}
					
//					//이제 루틴을 돌면서 Linear인 Point를 삭제하자
//					curPoint = startPoint;

//					nRemoved = 0;
					
//					while(true)
//					{
//						if(curPoint.IsLinearPoint())
//						{
//							//양쪽 포인트를 연결해준다.
//							curPoint._prevPoint._nextPoint = curPoint._nextPoint;
//							curPoint._nextPoint._prevPoint = curPoint._prevPoint;

//							nextPoint = curPoint._nextPoint;
							
//							//Linear 포인트는 삭제한다.
//							curGroup._outerPoints.Remove(curPoint);
//							nRemoved++;

//							curPoint = nextPoint;
//						}
//						else
//						{
//							nextPoint = curPoint._nextPoint;
//							curPoint = nextPoint;
//						}

//						if(nextPoint == startPoint)
//						{
//							//한바퀴 다 돌았다.
//							break;
//						}
//					}

					

//					//-----------------------------------------------------------------------------
//					//루틴2) 오목 코너를 찾아서 생략 가능한지 체크하자
//					//양쪽으로 3칸 이내에 포인트가 나오는 오목(Concave) 포인트를 찾자
//					//- 만약 오목 포인트가 연속으로 붙어있다면 2개의 오목 포인트까지 묶어서 생략해야한다.
//					//- (-3) Convex ~ [Concave, (Concave2)] ~ Convex (+3) 조건외에는 묶이지 최적화되지 않는다.

//					//루프를 돌기 위한 시작점은 Convex여야 한다. (삭제되면 안되므로)
//					startPoint = null;
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						if(!curPoint.IsConcavePoint())
//						{
//							startPoint = curPoint;
//							break;
//						}
//					}

//					//루프를 돌자
//					curPoint = startPoint;
//					nextPoint = null;
//					OuterPoint prevConvex = null;
//					OuterPoint nextConvex = null;
//					OuterPoint nextConcave = null;
//					OuterPoint checkPoint = null;

//					int distToPrev = 0;
//					int distToNearConcave = 0;
//					int distToNext = 0;
					

//					nRemoved = 0;

//					if (startPoint != null)
//					{
//						while (true)
//						{
//							//조건 체크
//							prevConvex = null;
//							nextConvex = null;
//							nextConcave = null;
//							distToPrev = 0;
//							distToNearConcave = 0;
//							distToNext = 0;
//							isRemoved = false;

//							if (curPoint.IsConcavePoint())
//							{
//								//오목한 Point 발견!
//								//이전 3 영역 이내에서 Convex가 발견되는지 확인 (Concave가 발견되면 취소)
//								checkPoint = curPoint._prevPoint;
//								if (checkPoint.IsConvexPoint())
//								{
//									prevConvex = checkPoint;
//									distToPrev = GetPointDistanceMHT(checkPoint, curPoint);
//								}

//								//다음 3 영역 내에서 Convex가 발견되는지 확인
//								//다음 포인트가 Concave인 경우는 별도로 처리
//								checkPoint = curPoint._nextPoint;
//								if (checkPoint.IsConcavePoint())
//								{
//									//Concave가 연달아 나온다.
//									distToNearConcave = GetPointDistanceMHT(checkPoint, curPoint);
//									nextConcave = checkPoint;
//									checkPoint = checkPoint._nextPoint;
//								}

//								if (checkPoint.IsConvexPoint())
//								{
//									//Concave가 없다.
//									distToNext = GetPointDistanceMHT(checkPoint, curPoint);
//									nextConvex = checkPoint;
//								}

//								//거리 체크를 하자 (실패하면 데이터를 null로 바꿈)
//								if (prevConvex != null && nextConvex != null && prevConvex != nextConvex)
//								{
//									//허용 조건
//									//- Concave가 있다면 2 이하여야 하며, Prev + Next는 10 이하
//									//- 그외)
//									//- 둘다 각각 5 이하일 때
//									//- 하나가 3이하일 때, 다른 하나가 10 이하
//									if (nextConcave != null)
//									{
//										if (distToNearConcave <= 2 && distToPrev + distToNext <= 10)
//										{
//											isRemoved = true;
//										}
//									}
//									else
//									{
//										if ((distToPrev <= 5 && distToNext <= 5)
//											|| (distToPrev <= 3 && distToNext <= 10)
//											|| (distToPrev <= 10 && distToNext <= 3)
//											|| (distToPrev <= 2 && distToNext <= 15)
//											|| (distToPrev <= 15 && distToNext <= 2)
//											)
//										{
//											isRemoved = true;
//										}
//									}
//								}

//							}

//							//if(prevConvex != null && nextConvex != null && prevConvex != nextConvex)
//							if (isRemoved)
//							{
//								//둘다 연결할 수 있다.
//								prevConvex._nextPoint = nextConvex;
//								nextConvex._prevPoint = prevConvex;

//								//포인트는 삭제
//								curGroup._outerPoints.Remove(curPoint);
//								nRemoved++;
//								if (nextConcave != null)
//								{
//									curGroup._outerPoints.Remove(nextConcave);
//									nRemoved++;
//								}

//								curPoint = nextConvex;
//							}
//							else
//							{
//								curPoint = curPoint._nextPoint;
//							}


//							if (curPoint == startPoint)
//							{
//								//한바퀴 다 돌았다.
//								break;
//							}
//						}
//					}

					
//					//----------------------------------------------------------------------------------
//					//루틴3) 각각의 선분의 기울기(float Vector와 index X/Y차이)와 Normal을 체크한다. (Normal은 외곽을 향하도록 한다)
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						ReCalculatePointSlopeAndNormal(curPoint);//이미 한번 계산되었으므로 ReCalculate
//						curPoint.SetShapeCheckFlag();//<<이제 각도를 직접 체크해서 Linear/Concave/Convex를 구분해야함을 알려준다.
//					}

//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						ReCalculatePointSlopeAndNormal(curPoint);
//					}

//					//----------------------------------------------------------------------------------
//					//루틴4) Convex로 부터 Normal->각도 증감을 비교하여 묶기를 시도한다.
//					startPoint = null;
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						if(curPoint.IsConvexPoint())
//						{
//							//5도 이상 차이가 난다면 꺾인 부분
//							startPoint = curPoint;
//							break;
//						}
//					}
//					if(startPoint == null)
//					{
//						startPoint = curGroup._outerPoints[0];
//					}

//					curPoint = startPoint;
//					nextPoint = null;
//					OuterPoint subCheckPoint = curPoint;
//					float farLimitDist = _scan_TileSize * 10.0f;
//					//float nearLimitDist = _scan_TileSize * 2.0f;
//					bool isTryOptimize = false;
//					List<OuterPoint> optimizablePoints = new List<OuterPoint>();
//					float subCheckAngle = 0.0f;
//					float subMinAngle = 0.0f;
//					float curAngle = 0.0f;
//					int count = 0;
					
//					nRemoved = 0;

//					while (true)
//					{
//						isTryOptimize = false;
//						if (curPoint != subCheckPoint)
//						{
//							if (curPoint == subCheckPoint._nextPoint)
//							{
//								//Next (+1) 인 경우
//								subCheckAngle = Vector2.Angle(subCheckPoint._normal_Avg, curPoint._pos - subCheckPoint._pos);
//								subMinAngle = subCheckAngle;
//								count++;
								
//							}
//							else
//							{
//								//Next의 이후 (+2, 3..)인 경우
//								float dist = Vector2.Distance(subCheckPoint._pos, curPoint._pos);
//								if (dist > farLimitDist)
//								{
//									isTryOptimize = true;
									
//								}
//								else
//								{
//									curAngle = Vector2.Angle(subCheckPoint._normal_Avg, curPoint._pos - subCheckPoint._pos);
//									if (curAngle <= subMinAngle + 0.0001f && curAngle > subCheckAngle - 80.0f)
//									{
//										//한칸 이동하면서 각도가 줄어든다. (점점 펼쳐진다.)
//										subMinAngle = curAngle;
//										count++;
										
//									}
//									else
//									{
//										//각도가 다시 벌어졌다.
//										//꺾인 부분을 발견했다.
//										isTryOptimize = true;
										
//									}
//								}
//							}
//						}
//						else
//						{
//							//그 외에는 처리하지 않는다.
//							count = 0;

//						}

//						if (isTryOptimize)
//						{
//							if (optimizablePoints.Count > 2)
//							{
//								//TODO.
//								//2개 이상의 버텍스들이 발견되었다.
//								//합칠 필요가 있다.
//								OuterPoint pointA = optimizablePoints[0];
//								OuterPoint pointB = optimizablePoints[optimizablePoints.Count - 1];

//								//처리후 삭제하자
//								pointA._nextPoint = pointB;
//								pointB._prevPoint = pointA;
//								for (int iOpt = 1; iOpt < optimizablePoints.Count - 1; iOpt++)
//								{
//									//A 다음부터 마지막 전까지 삭제
//									curGroup._outerPoints.Remove(optimizablePoints[iOpt]);
//									nRemoved++;
//								}
//								//합칠 필요가 없어도 일단 클리어
//								curPoint = pointB;

//								optimizablePoints.Clear();
//								optimizablePoints.Add(curPoint);
//								subCheckPoint = curPoint;
//								curPoint = curPoint._nextPoint;
//							}
//							else if (optimizablePoints.Count == 2)
//							{
//								curPoint = optimizablePoints[optimizablePoints.Count - 1];

//								optimizablePoints.Clear();//이제 클리어
//								optimizablePoints.Add(curPoint);//<<이번껀 새롭게 넣자
//								subCheckPoint = curPoint;//<<여기서부터 다시 시작
								
//								curPoint = curPoint._nextPoint;
//							}
//							else if (optimizablePoints.Count == 1)
//							{
//								//제자리에서 다시 체크 시작
//								optimizablePoints.Clear();//이제 클리어
//								optimizablePoints.Add(curPoint);//<<이번껀 새롭게 넣자
//								subCheckPoint = curPoint;//<<여기서부터 다시 시작
								
//								//curPoint = curPoint._nextPoint;//<Next로 이동하지 않는다.
//							}
//							else
//							{
//								//합칠 필요가 없어도 일단 클리어
//								optimizablePoints.Clear();//이제 클리어
//								optimizablePoints.Add(curPoint);//<<이번껀 새롭게 넣자
//								subCheckPoint = curPoint;//<<여기서부터 다시 시작
//								curPoint = curPoint._nextPoint;
//							}
//						}
//						else
//						{
//							//리스트를 리셋하지 않아도 된다. (유사한 포인트)
//							if (!optimizablePoints.Contains(curPoint))
//							{
//								optimizablePoints.Add(curPoint);
//							}
//							curPoint = curPoint._nextPoint;
//						}


//						if (curPoint == startPoint)
//						{
//							break;
//						}
//					}


					

//					//-----------------------------------------------------------------------------
//					//루틴5) 직선 점은 한번 더 삭제한다.
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						curPoint.SetShapeCheckFlag();
//					}

//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						if(!curPoint.IsLinearPoint())
//						{
//							startPoint = curPoint;
//							break;
//						}
//					}
//					if(startPoint == null)
//					{
//						//? 뭐지 최적화를 할수가 없는뎅?
//						continue;
//					}
					
//					//이제 루틴을 돌면서 Linear인 Point를 삭제하자
//					curPoint = startPoint;

//					nRemoved = 0;
					
//					while(true)
//					{
//						if(curPoint.IsLinearPoint())
//						{
//							//양쪽 포인트를 연결해준다.
//							curPoint._prevPoint._nextPoint = curPoint._nextPoint;
//							curPoint._nextPoint._prevPoint = curPoint._prevPoint;

//							nextPoint = curPoint._nextPoint;
							
//							//Linear 포인트는 삭제한다.
//							curGroup._outerPoints.Remove(curPoint);
//							nRemoved++;

//							curPoint = nextPoint;
//						}
//						else
//						{
//							nextPoint = curPoint._nextPoint;
//							curPoint = nextPoint;
//						}

//						if(nextPoint == startPoint)
//						{
//							//한바퀴 다 돌았다.
//							break;
//						}
//					}
					
					

//					//----------------------------------------------------------------------------------
				
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						ResetIndexByPos(curPoint);
//					}
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						ReCalculatePointSlopeAndNormal(curPoint);
//					}
//				}

//				//마지막 단계
//				//Mapping을 위한 Mapper의 컨트롤 포인트들을 만들자.
//				Vector2 point_LT = Vector2.zero;
//				Vector2 point_RB = Vector2.zero;
//				bool isMinMaxPointCalculated = false;
//				for (int iGroup = 0; iGroup < _outlineGroups.Count; iGroup++)
//				{
//					ScanTileGroup curGroup = _outlineGroups[iGroup];
//					if (curGroup._outerPoints.Count == 0)
//					{
//						continue;
//					}
//					OuterPoint curPoint = null;
//					for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//					{
//						curPoint = curGroup._outerPoints[iPoint];
//						if(!isMinMaxPointCalculated)
//						{
//							point_LT = curPoint._pos;
//							point_RB = curPoint._pos;
//							isMinMaxPointCalculated = true;
//						}
//						else
//						{
//							point_LT.x = Mathf.Min(point_LT.x, curPoint._pos.x);
//							point_LT.y = Mathf.Min(point_LT.y, curPoint._pos.y);
//							point_RB.x = Mathf.Max(point_RB.x, curPoint._pos.x);
//							point_RB.y = Mathf.Max(point_RB.y, curPoint._pos.y);
//						}
						
//					}
//				}
//				Mapper.Make(point_LT, point_RB);

//				_innerPoints.Clear();
//				_selectedControlPoints.Clear();
//				_selectedOuterGroup = null;
//				_selectedOuterGroupIndex = -1;
				

//				if(_outlineGroups.Count > 0)
//				{
//					_selectedOuterGroupIndex = 0;
//					_selectedOuterGroup = _outlineGroups[_selectedOuterGroupIndex];
//				}

//				_step = STEP.Scanned;
//			}
//			catch(Exception ex)
//			{
//				Debug.LogError("AnyPortrait : Mesh Scan Failed : " + ex);
//			}
//		}

//		//---------------------------------------------------------------
		
//		//단계 2 - Preview
//		public void Step2_Preview()
//		{
//			if(!IsScanned)
//			{
//				return;
//			}
//			try
//			{
//				_innerPoints.Clear();
//				_selectedControlPoints.Clear();
//				_selectedOuterGroup = null;
//				_selectedOuterGroupIndex = -1;
//				switch (Mapper._shape)
//				{
//					case apMeshGenMapper.MapperShape.Quad:
//						//Quad방식에 맞게 Inner Point를 생성한다.
//						CreateInnerPoints_Quad(false);
//						break;

//					case apMeshGenMapper.MapperShape.ComplexQuad:
//						//Complex Quad 방식에 맞게 Inner Point를 생성한다.
//						CreateInnerPoints_Quad(true);
//						break;
						

//					case apMeshGenMapper.MapperShape.Circle:
//						//Circle 방식에 맞게 Inner Point를 생성한다.
//						CreateInnerPoints_Circle();
//						break;

//					case apMeshGenMapper.MapperShape.Ring:
//						//Ring 방식에 맞게 Inner Point를 생성한다.
//						CreateInnerPoints_Ring();
//						break;
//				}

//				if(_outlineGroups.Count > 0)
//				{
//					_selectedOuterGroupIndex = 0;
//					_selectedOuterGroup = _outlineGroups[_selectedOuterGroupIndex];
//				}

//				_step = STEP.Previewed;
//			}
//			catch(Exception ex)
//			{
//				Debug.LogError("AnyPortrait : Mesh Preview Failed : " + ex);
//			}
//		}



//		private void CreateInnerPoints_Quad(bool isComplex)
//		{
//			//1. Divide 개수를 이용해서 대략 버텍스의 간격을 추측하자.
//			//2. 외곽 선분들 중에서 그 간격보다 너무 긴 선분은 미리 쪼개주자 (별도의 플래그를 걸고 삭제될 수 있게)
//			//- 동시에 플래그가 있었던 Tmp OuterPoint는 미리 삭제한다.
//			//3. 2D 테이블에 Inner Point를 생성한다.
//			//- 내부 교차점에서는 "이동 제한이 걸린" Inner Point를 생성한다. <옵션에 따라서>
//			//4. OuterPoint와 가장자리의 InnerPoint를 연결한다.
//			//- 빠짐없이 최소 거리로 각각 최소 1개씩 연결한다.
//			//5. Relax를 조금 실행한다.
			
//			apMeshGenMapper.ControlPoint[,] controlPointArr = Mapper.GetQuadControlPointArr(isComplex);
//			List<apMeshGenMapper.ControlPoint> controlPointList = Mapper.GetControlPointList(isComplex ? apMeshGenMapper.MapperShape.ComplexQuad : apMeshGenMapper.MapperShape.Quad);

//			apMeshGenMapper.ControlPoint curPoint = null;
//			apMeshGenMapper.ControlPoint nextPoint = null;

//			_outerPoints_Preview.Clear();
			
			

//			//1. Divide 개수를 이용해서 대략 버텍스의 간격을 추측하자.
//			int nDivide = Mathf.Max(_editor._meshAutoGenOption_GridDivide, 1);

//			//각 Control Point간의 거리의 평균을 구하자
//			float dist = 0.0f;
//			float totalDist = 0.0f;
//			int nDistCalculated = 0;
//			for (int iPoint = 0; iPoint < controlPointList.Count; iPoint++)
//			{
//				curPoint = controlPointList[iPoint];
//				if(curPoint._linkedInnerOrCrossPoints.Count > 0)
//				{
//					for (int iNext = 0; iNext < curPoint._linkedInnerOrCrossPoints.Count; iNext++)
//					{
//						nextPoint = curPoint._linkedInnerOrCrossPoints[iNext];
//						dist = Vector2.Distance(nextPoint._pos_Cur, curPoint._pos_Cur);
//						totalDist += dist;
//						nDistCalculated++;
//					}
//				}
//				if(curPoint._linkedOuterPoints.Count > 0)
//				{
//					for (int iNext = 0; iNext < curPoint._linkedOuterPoints.Count; iNext++)
//					{
//						nextPoint = curPoint._linkedOuterPoints[iNext];
//						dist = Vector2.Distance(nextPoint._pos_Cur, curPoint._pos_Cur);
//						totalDist += dist;
//						nDistCalculated++;
//					}
//				}
//			}

//			float avgDist = 0.0f;
//			if(nDistCalculated > 0)
//			{
//				avgDist = totalDist / nDistCalculated;
//			}
//			else
//			{
//				avgDist = 500;
//			}
//			avgDist /= nDivide;
//			if(avgDist < 5)
//			{
//				avgDist = 5;
//			}

//			//2. 외곽 선분들 중에서 그 간격보다 너무 긴 선분은 미리 쪼개주자 (별도의 플래그를 걸고 삭제될 수 있게)
//			//- 동시에 플래그가 있었던 Tmp OuterPoint는 미리 삭제한다.

//			//OuterPoint 리스트를 통합으로 만들자.
//			Dictionary<OuterPoint, OuterPoint> outerPointSrc2Copy = new Dictionary<OuterPoint, OuterPoint>();
//			Dictionary<OuterPoint, OuterPoint> outerPointCopy2Src = new Dictionary<OuterPoint, OuterPoint>();

//			ScanTileGroup curGroup = null;
//			OuterPoint curOutPoint = null;

//			float margin = Mathf.Max(_editor._meshAutoGenOption_Margin, 0);
//			for (int iGroup = 0; iGroup < _outlineGroups.Count; iGroup++)
//			{
//				curGroup = _outlineGroups[iGroup];
//				if(!curGroup._isEnabled)
//				{	
//					continue;//비활성화 되었다면 생략
//				}
//				for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//				{
//					curOutPoint = curGroup._outerPoints[iPoint];
//					OuterPoint newPoint = new OuterPoint(curOutPoint);

//					_outerPoints_Preview.Add(newPoint);

//					outerPointSrc2Copy.Add(curOutPoint, newPoint);
//					outerPointCopy2Src.Add(newPoint, curOutPoint);
//				}
//			}
			
//			//Next/Prev를 Mapping을 이용해서 연결하자
//			OuterPoint srcOutPoint = null;
//			for (int iPoint = 0; iPoint < _outerPoints_Preview.Count; iPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iPoint];
//				srcOutPoint = outerPointCopy2Src[curOutPoint];

//				curOutPoint.CopyLink(srcOutPoint, outerPointSrc2Copy);
//			}

//			//Margin 만큼 확장하자. (일부 포인트는 삭제된다)
//			ExtendOuterPointsPreview(margin, outerPointSrc2Copy, outerPointCopy2Src);
			
//			//리스트를 돌면서 Next와의 거리가 멀다면 나누어주자 (일단 별도의 리스트를 만들어서 나중에 참조)
//			List<OuterPoint> splitOutPoints = new List<OuterPoint>();
//			OuterPoint nextOutPoint = null;
//			for (int iPoint = 0; iPoint < _outerPoints_Preview.Count; iPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iPoint];
//				if(curOutPoint._nextPoint == null || splitOutPoints.Contains(curOutPoint._nextPoint))
//				{
//					continue;
//				}

//				nextOutPoint = curOutPoint._nextPoint;
//				dist = Vector2.Distance(curOutPoint._pos, nextOutPoint._pos);
//				if(dist > avgDist * 1.5f)
//				{
//					//너무 멀리 떨어져있다.
//					int nSplit = Mathf.Max((int)(dist / (avgDist * 1.2f)), 1);
//					OuterPoint prevOutPoint = curOutPoint;
//					for (int iSplit = 0; iSplit < nSplit; iSplit++)
//					{
//						OuterPoint newSplitOutPoint = new OuterPoint();
//						newSplitOutPoint.SetSplit(curOutPoint, nextOutPoint, iSplit, nSplit);

//						newSplitOutPoint._prevPoint = prevOutPoint;
//						prevOutPoint._nextPoint = newSplitOutPoint;

//						prevOutPoint = newSplitOutPoint;

//						splitOutPoints.Add(newSplitOutPoint);
//					}

//					//마지막 끝 점 처리
//					nextOutPoint._prevPoint = prevOutPoint;
//					prevOutPoint._nextPoint = nextOutPoint;
//				}
//			}

//			if(splitOutPoints.Count > 0)
//			{
//				//분할된 포인트를 리스트에 합쳐준다.
//				for (int iSplit = 0; iSplit < splitOutPoints.Count; iSplit++)
//				{
//					_outerPoints_Preview.Add(splitOutPoints[iSplit]);
//				}

//			}


//			//3. 2D 테이블에 Inner Point를 생성한다.
//			//- 내부 교차점에서는 "이동 제한이 걸린" Inner Point를 생성한다.
//			_innerPoints.Clear();
//			_maxInnerPointPriority = 0;
//			bool isLockAxis = _editor._meshAutoGenOption_IsLockAxis;

//			int controlPointNumX = Mathf.Max(controlPointArr.GetLength(0), 2);
//			int controlPointNumY = Mathf.Max(controlPointArr.GetLength(1), 2);
//			int innerPointNumX = controlPointNumX + (nDivide - 1) * (controlPointNumX - 1);
//			int innerPointNumY = controlPointNumY + (nDivide - 1) * (controlPointNumY - 1);

//			float minDistControlPointAxis_X = 20.0f;
//			float minDistControlPointAxis_Y = 20.0f;
//			for (int iCX = 0; iCX < controlPointArr.GetLength(0) - 1; iCX++)
//			{
				
//				for (int iCY = 0; iCY < controlPointArr.GetLength(1) - 1; iCY++)
//				{
//					apMeshGenMapper.ControlPoint cp_A = controlPointArr[iCX, iCY];
//					apMeshGenMapper.ControlPoint cp_X = controlPointArr[iCX + 1, iCY];
//					apMeshGenMapper.ControlPoint cp_Y = controlPointArr[iCX, iCY + 1];

//					float ctDstX = Vector2.Distance(cp_A._pos_Cur, cp_X._pos_Cur);
//					float ctDstY = Vector2.Distance(cp_A._pos_Cur, cp_Y._pos_Cur);

//					if(ctDstX < minDistControlPointAxis_X)
//					{
//						minDistControlPointAxis_X = ctDstX;
//					}
//					if(ctDstY < minDistControlPointAxis_Y)
//					{
//						minDistControlPointAxis_Y = ctDstY;
//					}
//				}
//			}
//			minDistControlPointAxis_X /= ((innerPointNumX - 1) + 3);
//			minDistControlPointAxis_Y /= ((innerPointNumY - 1) + 3);

//			InnerPoint[,] innerPointArr = new InnerPoint[innerPointNumX, innerPointNumY];
//			for (int iX = 0; iX < innerPointNumX; iX++)
//			{
//				int iControlPointX_A = iX / nDivide;
//				int iControlPointX_B = iControlPointX_A + 1;
//				if(iControlPointX_A == controlPointNumX - 1)
//				{
//					iControlPointX_B = iControlPointX_A;
//				}
//				bool isOnAxisX = false;
//				if(iX % nDivide == 0 && iX != 0 && iX != innerPointNumX - 1)
//				{
//					//중간 점이 아니고 (나머지 = 0)
//					//처음과 끝이 아니라면
//					//컨트롤 포인트를 잇는 선상에 위치한다.
//					isOnAxisX = true;
//				}
//				float lerpX = (iX % nDivide) / (float)nDivide;
//				float lerpXReal = lerpX;

//				if(iControlPointX_A == 0)
//				{
//					//맨 왼쪽인 경우 일부 축소
//					//0~1 => 0.5~1
//					lerpXReal = (lerpX * 0.5f) + 0.5f;
//				}
//				else if(iControlPointX_A == controlPointNumX - 2)
//				{
//					//맨 오른쪽인 경우 일부 축소
//					//0~1 => 0~0.5
//					lerpXReal = (lerpX * 0.5f);
//				}
//				else if(iControlPointX_A == controlPointNumX - 1)
//				{
//					//끝점이면 이전 점을 찾아서 둘다 축소
//					lerpXReal = 0.5f;
//				}


//				for (int iY = 0; iY < innerPointNumY; iY++)
//				{
//					int iControlPointY_A = iY / nDivide;
//					int iControlPointY_B = iControlPointY_A + 1;
//					if(iControlPointY_A == controlPointNumY - 1)
//					{
//						iControlPointY_B = iControlPointY_A;
//					}
//					bool isOnAxisY = false;
//					if(iY % nDivide == 0 && iY != 0 && iY != innerPointNumY - 1)
//					{
//						isOnAxisY = true;
//					}
//					float lerpY = (iY % nDivide) / (float)nDivide;
//					float lerpYReal = lerpY;
//					if(iControlPointY_A == 0)
//					{
//						//맨 위쪽인 경우 일부 축소
//						//0~1 => 0.5~1
//						lerpYReal = (lerpY * 0.5f) + 0.5f;
//					}
//					else if(iControlPointY_A == controlPointNumY - 2)
//					{
//						//맨 아래쪽인 경우 일부 축소
//						//0~1 => 0~0.5
//						lerpYReal = lerpYReal * 0.5f;
//					}
//					else if(iControlPointY_A == controlPointNumY - 1)
//					{
//						//끝점이면 이전 점을 찾아서 둘다 축소
//						//0.5
//						lerpYReal = 0.5f;
//					}

//					Vector2 posLT = Vector2.zero;
//					Vector2 posRT = Vector2.zero;
//					Vector2 posLB = Vector2.zero;
//					Vector2 posRB = Vector2.zero;

//					if(iControlPointX_A == controlPointNumX - 1)
//					{
//						iControlPointX_A = iControlPointX_B-1;
//					}

//					if(iControlPointY_A == controlPointNumY - 1)
//					{
//						iControlPointY_A = iControlPointY_B-1;
//					}

//					apMeshGenMapper.ControlPoint cp_LT = controlPointArr[iControlPointX_A, iControlPointY_A];
//					apMeshGenMapper.ControlPoint cp_RT = controlPointArr[iControlPointX_B, iControlPointY_A];
//					apMeshGenMapper.ControlPoint cp_LB = controlPointArr[iControlPointX_A, iControlPointY_B];
//					apMeshGenMapper.ControlPoint cp_RB = controlPointArr[iControlPointX_B, iControlPointY_B];

					

//					posLT = cp_LT._pos_Cur;
//					posRT = cp_RT._pos_Cur;
//					posLB = cp_LB._pos_Cur;
//					posRB = cp_RB._pos_Cur;
					
					

//					Vector2 posT = posLT * (1.0f - lerpXReal) + posRT * lerpXReal;
//					Vector2 posB = posLB * (1.0f - lerpXReal) + posRB * lerpXReal;
//					Vector2 pos = posT * (1.0f - lerpYReal) + posB * lerpYReal;
//					InnerPoint newPoint = new InnerPoint(pos, lerpX, lerpY);

//					//이동 제한
//					if (isLockAxis)
//					{
//						if(isOnAxisX && isOnAxisY)
//						{
//							//둘다 포함일 때 => Locked
//							newPoint._moveLock = INNER_MOVE_LOCK.Locked;
//						}
//						else if (isOnAxisX)
//						{
//							//X 축에 있을 때 => Limited + Y로만 이동
//							newPoint._moveLock = INNER_MOVE_LOCK.AxisLimited;
//							newPoint._limitedPosA = posLT;
//							newPoint._limitedPosB = posLB;
//						}
//						else if (isOnAxisY)
//						{
//							//Y 축에 있을 때 => Limited + X로만 이동
//							newPoint._moveLock = INNER_MOVE_LOCK.AxisLimited;
//							newPoint._limitedPosA = posLT;
//							newPoint._limitedPosB = posRT;
//						}
//					}

//					innerPointArr[iX, iY] = newPoint;
//				}
//			}

//			//이제 연결을 하자
//			InnerPoint curInnerPoint = null;
//			InnerPoint leftInnerPoint = null;
//			InnerPoint rightInnerPoint = null;
//			InnerPoint upInnerPoint = null;
//			InnerPoint downInnerPoint = null;
//			List<InnerPoint> outerInnerPoints = new List<InnerPoint>();
//			for (int iX = 0; iX < innerPointNumX; iX++)
//			{
//				for (int iY = 0; iY < innerPointNumY; iY++)
//				{
//					curInnerPoint = innerPointArr[iX, iY];

//					leftInnerPoint = null;
//					rightInnerPoint = null;
//					upInnerPoint = null;
//					downInnerPoint = null;

//					if(iX - 1 >= 0)
//					{
//						leftInnerPoint = innerPointArr[iX - 1, iY];
//					}
//					if(iX + 1 < innerPointNumX)
//					{
//						rightInnerPoint = innerPointArr[iX + 1, iY];
//					}
//					if(iY - 1 >= 0)
//					{
//						upInnerPoint = innerPointArr[iX, iY - 1];
//					}
//					if(iY + 1 < innerPointNumY)
//					{
//						downInnerPoint = innerPointArr[iX, iY + 1];
//					}

//					if(leftInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(leftInnerPoint);
//					}
//					if(rightInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(rightInnerPoint);
//						curInnerPoint._linkedPoint_GUI.Add(rightInnerPoint);
//					}
//					if(upInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(upInnerPoint);
//					}
//					if(downInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(downInnerPoint);
//						curInnerPoint._linkedPoint_GUI.Add(downInnerPoint);
//					}

//					//마지막으로 리스트에 넣자
//					_innerPoints.Add(curInnerPoint);

//					if(iX == 0 || iX == innerPointNumX - 1
//						|| iY == 0 || iY == innerPointNumY - 1)
//					{
//						//InnerPoint 중에 외곽에 해당한다면 리스트에 추가
//						outerInnerPoints.Add(curInnerPoint);

//						//Normal도 계산한다.
//						//+ Normal 방향으로 Outer Point와의 적절한 거리도 계산한다.
//						//X, Y 축으로 각각 계산
//						Vector2 normalVec = Vector2.zero;
//						float properDist = 0.0f;
//						bool isProperDist_X = false;
//						bool isProperDist_Y = false;
						
//						if(iX == 0)
//						{
//							normalVec.x = -1;
//							isProperDist_X = true;
//						}
//						else if(iX == innerPointNumX - 1)
//						{
//							normalVec.x = 1;
//							isProperDist_X = true;
//						}

//						if(iY == 0)
//						{
//							normalVec.y = -1;
//							isProperDist_Y = true;
//						}
//						else if(iY == innerPointNumY - 1)
//						{
//							normalVec.y = 1;
//							isProperDist_Y = true;
//						}

//						if(isProperDist_X && isProperDist_Y)
//						{
//							properDist = (minDistControlPointAxis_X + minDistControlPointAxis_Y) * 0.25f;
//						}
//						else if(isProperDist_X)
//						{
//							properDist = minDistControlPointAxis_X * 0.5f;
//						}
//						else if(isProperDist_Y)
//						{
//							properDist = minDistControlPointAxis_Y * 0.5f;
//						}
						


//						curInnerPoint._normalIfOuter = normalVec.normalized;
//						curInnerPoint._isOuter = true;
//						curInnerPoint._priority = 1;//<<외곽이면 우선 순위 1 (최상)
//						curInnerPoint._properDistIfOuter = properDist;
//					}
//				}
//			}

//			//우선순위 계산
//			//_maxInnerPointPriority = 1;
//			MakePriority();


//			//추가 >> 각 지점에서 축에 대한 끝점을 서로 연결해야한다.
//			for (int iX = 0; iX < innerPointNumX; iX++)
//			{
//				for (int iY = 0; iY < innerPointNumY; iY++)
//				{
//					curInnerPoint = innerPointArr[iX, iY];
//					InnerPoint leftEnd = innerPointArr[0, iY];
//					InnerPoint rightEnd = innerPointArr[innerPointNumX - 1, iY];
//					InnerPoint upEnd = innerPointArr[iX, 0];
//					InnerPoint downEnd = innerPointArr[iX, innerPointNumY - 1];
//					if (iY > 0 && iY < innerPointNumY - 1)
//					{
//						if (leftEnd != curInnerPoint)
//						{
//							curInnerPoint._linkedPoint_EndOut_Axis1.Add(leftEnd);
//						}
//						if (rightEnd != curInnerPoint)
//						{
//							curInnerPoint._linkedPoint_EndOut_Axis1.Add(rightEnd);
//						}
//					}
//					if (iX > 0 && iX < innerPointNumX - 1)
//					{
//						if (upEnd != curInnerPoint)
//						{
//							curInnerPoint._linkedPoint_EndOut_Axis2.Add(upEnd);
//						}
//						if (downEnd != curInnerPoint)
//						{
//							curInnerPoint._linkedPoint_EndOut_Axis2.Add(downEnd);
//						}
//					}
//				}
//			}


			
//			//4. OuterPoint와 가장자리의 InnerPoint를 연결한다.
//			//- 빠짐없이 최소 거리로 각각 최소 1개씩 연결한다.
//			//- InnerPoint의 Normal과 Inner -> Out의 벡터의 각도를 계산해서, 가중치를 넣는다.

//			List<InnerPoint> addedInnerPoints = new List<InnerPoint>();

//			for (int iOutPoint = 0; iOutPoint < _outerPoints_Preview.Count; iOutPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iOutPoint];

//				//가장 가까운 InnerPoint를 찾자
//				float minDist = float.MaxValue;
//				InnerPoint minPoint = null;
//				float deltaAngle = 0.0f;
//				float angleWeight = 0.0f;

//				for (int iInnerPoint = 0; iInnerPoint < outerInnerPoints.Count; iInnerPoint++)
//				{
//					curInnerPoint = outerInnerPoints[iInnerPoint];
//					float curDist = Vector2.Distance(curOutPoint._pos, curInnerPoint._pos);
//					if (curDist > 0.0f)
//					{
//						deltaAngle = Vector2.Angle(curInnerPoint._normalIfOuter, curOutPoint._pos - curInnerPoint._pos);
//						angleWeight = (Mathf.Clamp(deltaAngle, 0.0f, 120.0f) / 120.0f);
//						//0 : 0도 ~ 1 : 120도
//						//>> 0 : 0도 ~ 4 : 120도
//						//>> 1 : 0도 ~ 5 : 120도
//						angleWeight = (angleWeight * 4) + 1;
//						curDist *= angleWeight;
//					}


//					if(curDist < minDist || minPoint == null)
//					{
//						minPoint =  curInnerPoint;
//						minDist = curDist;
//					}
//				}

//				//최단 거리의 Inner Point와 연결하자
//				if(minPoint != null)
//				{
//					minPoint._linkedOuterPoints.Add(curOutPoint);
//					addedInnerPoints.Add(minPoint);
//				}
//			}

//			//반대로 처리가 안된 InnerPoint 기준으로 최단 거리의 Outer Point를 연결하자
//			for (int iInnerPoint = 0; iInnerPoint < outerInnerPoints.Count; iInnerPoint++)
//			{
//				curInnerPoint = outerInnerPoints[iInnerPoint];
//				if(addedInnerPoints.Contains(curInnerPoint))
//				{
//					continue;
//				}
//				float minDist = float.MaxValue;
//				OuterPoint minPoint = null;
//				float deltaAngle = 0.0f;
//				float angleWeight = 0.0f;

//				for (int iOutPoint = 0; iOutPoint < _outerPoints_Preview.Count; iOutPoint++)
//				{
//					curOutPoint = _outerPoints_Preview[iOutPoint];
//					float curDist = Vector2.Distance(curOutPoint._pos, curInnerPoint._pos);
//					if (curDist > 0.0f)
//					{
//						deltaAngle = Vector2.Angle(curInnerPoint._normalIfOuter, curOutPoint._pos - curInnerPoint._pos);
//						angleWeight = (Mathf.Clamp(deltaAngle, 0.0f, 120.0f) / 120.0f);
//						//0 : 0도 ~ 1 : 120도
//						//>> 0 : 0도 ~ 4 : 120도
//						//>> 1 : 0도 ~ 5 : 120도
//						angleWeight = (angleWeight * 4) + 1;
//						curDist *= angleWeight;
//					}

//					if(curDist < minDist || minPoint == null)
//					{
//						minPoint =  curOutPoint;
//						minDist = curDist;
//					}
//				}

//				//최단 거리의 Inner Point와 연결하자
//				if(minPoint != null)
//				{
//					curInnerPoint._linkedOuterPoints.Add(minPoint);
//				}
//			}

//			//5. Relax를 조금 실행한다. (1회)
//			RelaxInnerPoints(1);
			
//		}

//		private void CreateInnerPoints_Circle()
//		{
//			//Circle 방식
//			//외곽 > Center로 이어지는 방식이다.
//			//각 좌표계(각 + 거리)로 이루어져있다.
			

//			//1. Divide 개수를 이용해서 대략 버텍스의 간격을 추측하자.
//			//2. 외곽 선분들 중에서 그 간격보다 너무 긴 선분은 미리 쪼개주자 (별도의 플래그를 걸고 삭제될 수 있게)
//			//3. 각 좌표계에 따른 Inner Point를 생성한다.
//			//- 내부 교차점에서는 "이동 제한이 걸린" Inner Point를 생성한다. <옵션에 따라서>
//			//4. OuterPoint와 가장자리의 InnerPoint를 연결한다.
//			//- 빠짐없이 최소 거리로 각각 최소 1개씩 연결한다.
//			//5. Relax를 조금 실행한다.
			
//			apMeshGenMapper.ControlPoint[] controlPointOuterArr = Mapper.GetCircleControlPointArr();
//			apMeshGenMapper.ControlPoint controlPoint_Center = Mapper.GetCircleCenterControlPoint();
//			//List<apMeshGenMapper.ControlPoint> controlPointList = Mapper.GetControlPointList(apMeshGenMapper.MapperShape.Circle);
			
//			apMeshGenMapper.ControlPoint curPoint = null;
//			apMeshGenMapper.ControlPoint nextPoint = null;

//			_outerPoints_Preview.Clear();
			

//			//1. Divide 개수를 이용해서 대략 버텍스의 간격을 추측하자.
//			int nDivide = Mathf.Max(_editor._meshAutoGenOption_GridDivide, 1);

//			//각 Control Point간의 거리의 평균을 구하자
//			//Outer 사이의 간격만 구한다.
//			float dist = 0.0f;
//			float totalDist = 0.0f;
//			int nDistCalculated = 0;
//			for (int iPoint = 0; iPoint < controlPointOuterArr.Length; iPoint++)
//			{
//				curPoint = controlPointOuterArr[iPoint];
//				if(curPoint._linkedOuterPoints.Count > 0)
//				{
//					for (int iNext = 0; iNext < curPoint._linkedOuterPoints.Count; iNext++)
//					{
//						nextPoint = curPoint._linkedOuterPoints[iNext];
//						dist = Vector2.Distance(nextPoint._pos_Cur, curPoint._pos_Cur);
//						totalDist += dist;
//						nDistCalculated++;
//					}
//				}
//			}

//			float avgDist = 0.0f;
//			if(nDistCalculated > 0)
//			{
//				avgDist = totalDist / nDistCalculated;
//			}
//			else
//			{
//				avgDist = 500;
//			}
//			avgDist /= nDivide;
//			if(avgDist < 5)
//			{
//				avgDist = 5;
//			}

			
//			//2. 외곽 선분들 중에서 그 간격보다 너무 긴 선분은 미리 쪼개주자 (별도의 플래그를 걸고 삭제될 수 있게)
//			//- 동시에 플래그가 있었던 Tmp OuterPoint는 미리 삭제한다.

//			//OuterPoint 리스트를 통합으로 만들자.
//			Dictionary<OuterPoint, OuterPoint> outerPointSrc2Copy = new Dictionary<OuterPoint, OuterPoint>();
//			Dictionary<OuterPoint, OuterPoint> outerPointCopy2Src = new Dictionary<OuterPoint, OuterPoint>();

//			ScanTileGroup curGroup = null;
//			OuterPoint curOutPoint = null;
//			float margin = Mathf.Max(_editor._meshAutoGenOption_Margin, 0);
//			for (int iGroup = 0; iGroup < _outlineGroups.Count; iGroup++)
//			{
//				curGroup = _outlineGroups[iGroup];
//				if(!curGroup._isEnabled)
//				{	
//					continue;//비활성화 되었다면 생략
//				}
//				for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//				{
//					curOutPoint = curGroup._outerPoints[iPoint];
//					OuterPoint newPoint = new OuterPoint(curOutPoint);

//					_outerPoints_Preview.Add(newPoint);

//					outerPointSrc2Copy.Add(curOutPoint, newPoint);
//					outerPointCopy2Src.Add(newPoint, curOutPoint);
//				}
//			}
			
//			//Next/Prev를 Mapping을 이용해서 연결하자
//			OuterPoint srcOutPoint = null;
//			for (int iPoint = 0; iPoint < _outerPoints_Preview.Count; iPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iPoint];
//				srcOutPoint = outerPointCopy2Src[curOutPoint];

//				curOutPoint.CopyLink(srcOutPoint, outerPointSrc2Copy);
//			}

//			//Margin 만큼 확장한다.
//			ExtendOuterPointsPreview(margin, outerPointSrc2Copy, outerPointCopy2Src);
			
//			//리스트를 돌면서 Next와의 거리가 멀다면 나누어주자 (일단 별도의 리스트를 만들어서 나중에 참조)
//			List<OuterPoint> splitOutPoints = new List<OuterPoint>();
//			OuterPoint nextOutPoint = null;
//			for (int iPoint = 0; iPoint < _outerPoints_Preview.Count; iPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iPoint];
//				if(curOutPoint._nextPoint == null || splitOutPoints.Contains(curOutPoint._nextPoint))
//				{
//					continue;
//				}

//				nextOutPoint = curOutPoint._nextPoint;
//				dist = Vector2.Distance(curOutPoint._pos, nextOutPoint._pos);
//				if(dist > avgDist * 1.5f)
//				{
//					//너무 멀리 떨어져있다.
//					int nSplit = Mathf.Max((int)(dist / (avgDist * 1.2f)), 1);
//					OuterPoint prevOutPoint = curOutPoint;
//					for (int iSplit = 0; iSplit < nSplit; iSplit++)
//					{
//						OuterPoint newSplitOutPoint = new OuterPoint();
//						newSplitOutPoint.SetSplit(curOutPoint, nextOutPoint, iSplit, nSplit);

//						newSplitOutPoint._prevPoint = prevOutPoint;
//						prevOutPoint._nextPoint = newSplitOutPoint;

//						prevOutPoint = newSplitOutPoint;

//						splitOutPoints.Add(newSplitOutPoint);
//					}

//					//마지막 끝 점 처리
//					nextOutPoint._prevPoint = prevOutPoint;
//					prevOutPoint._nextPoint = nextOutPoint;
//				}
//			}

//			if(splitOutPoints.Count > 0)
//			{
//				//분할된 포인트를 리스트에 합쳐준다.
//				for (int iSplit = 0; iSplit < splitOutPoints.Count; iSplit++)
//				{
//					_outerPoints_Preview.Add(splitOutPoints[iSplit]);
//				}

//			}


//			//3. 각 좌표계에 따른 Inner Point를 생성한다.
//			//- 내부 교차점에서는 "이동 제한이 걸린" Inner Point를 생성한다. <옵션에 따라서>
//			_innerPoints.Clear();
//			_maxInnerPointPriority = 0;

//			bool isLockAxis = _editor._meshAutoGenOption_IsLockAxis;

//			int controlPointNumAngle = controlPointOuterArr.Length;
//			int controlPointNumRadius = 2;//Center ~ Outer 2개밖에 없다.
//			int innerPointNumAngle = controlPointNumAngle + (nDivide - 1) * (controlPointNumAngle);
//			int innerPointNumRadius = controlPointNumRadius + (nDivide - 1) * (controlPointNumRadius - 1);

//			//Control Point의 간격을 정한다. 나중에 외곽에서의 여백에 사용될 것
//			float minDistControlPointAxis_Radius = 20.0f;
//			for (int iCAngle = 0; iCAngle < controlPointOuterArr.Length; iCAngle++)
//			{
//				apMeshGenMapper.ControlPoint cp_A = controlPointOuterArr[iCAngle];
//				float ctDist = Vector2.Distance(cp_A._pos_Cur, controlPoint_Center._pos_Cur);
//				if(ctDist < minDistControlPointAxis_Radius)
//				{
//					minDistControlPointAxis_Radius = ctDist;
//				}
//			}
//			minDistControlPointAxis_Radius /= ((innerPointNumRadius - 1) + 3);


//			InnerPoint[,] innerPointArr = new InnerPoint[innerPointNumAngle, innerPointNumRadius];
//			InnerPoint innerPointCenter = null;

//			Vector2 posCenter = controlPoint_Center._pos_Cur;

//			for (int iAngle = 0; iAngle < innerPointNumAngle; iAngle++)
//			{
//				int iControlPointAngle_A = iAngle / nDivide;
//				int iControlPointAngle_B = (iControlPointAngle_A + 1) % controlPointNumAngle;
//				bool isOnAxisAngle = false;
//				if (iAngle % nDivide == 0)
//				{
//					//중간 점이 아니면 (나머지 = 0)
//					isOnAxisAngle = true;
//				}
//				float lerpAngle = (iAngle % nDivide) / (float)nDivide;
//				for (int iRadius = 0; iRadius < innerPointNumRadius; iRadius++)
//				{
//					float lerpRad = (float)iRadius / (float)(innerPointNumRadius - 1);
//					//이걸 수정해서 0.5에 몰리도록 하자
//					//0~1 -> 0~0.2 -> 0.4~0.6
//					float realLerpRad = lerpRad;
//					lerpRad = (lerpRad * 0.2f) + 0.4f;

//					Vector2 posOut_L = Vector2.zero;
//					Vector2 posOut_R = Vector2.zero;

//					apMeshGenMapper.ControlPoint cp_OutL = controlPointOuterArr[iControlPointAngle_A];
//					apMeshGenMapper.ControlPoint cp_OutR = controlPointOuterArr[iControlPointAngle_B];

//					posOut_L = cp_OutL._pos_Cur;
//					posOut_R = cp_OutR._pos_Cur;

//					//Vector2 dirL = posOut_L - posCenter;
//					//Vector2 dirR = posOut_R - posCenter;

//					Vector2 pos_L = posCenter * (1.0f - lerpRad) + posOut_L * lerpRad;
//					Vector2 pos_R = posCenter * (1.0f - lerpRad) + posOut_R * lerpRad;
					

//					Vector2 pos = pos_L * (1.0f - lerpAngle) + pos_R * lerpAngle;
//					InnerPoint newPoint = new InnerPoint(pos, lerpAngle, realLerpRad);

//					//이동 제한
//					if (isLockAxis)
//					{
//						if (isOnAxisAngle)
//						{
//							newPoint._moveLock = INNER_MOVE_LOCK.AxisLimited;
//							newPoint._limitedPosA = posCenter;
//							newPoint._limitedPosB = posOut_L;
//						}
//					}

//					innerPointArr[iAngle, iRadius] = newPoint;
//				}
//			}

//			//추가:Center 위치에 InnerPoint를 별도로 추가하자
//			innerPointCenter = new InnerPoint(posCenter, 0.0f, 0.0f);
//			//Move Lock
//			if(isLockAxis)
//			{
//				innerPointCenter._moveLock = INNER_MOVE_LOCK.Locked;
//			}

//			//이제 연결을 하자
//			InnerPoint curInnerPoint = null;
//			InnerPoint leftInnerPoint = null;
//			InnerPoint rightInnerPoint = null;
//			InnerPoint upInnerPoint = null;
//			InnerPoint downInnerPoint = null;
//			List<InnerPoint> outerInnerPoints = new List<InnerPoint>();
//			for (int iAngle = 0; iAngle < innerPointNumAngle; iAngle++)
//			{
//				for (int iRad = 0; iRad < innerPointNumRadius; iRad++)
//				{
//					curInnerPoint = innerPointArr[iAngle, iRad];

//					leftInnerPoint = null;
//					rightInnerPoint = null;
//					upInnerPoint = null;
//					downInnerPoint = null;

//					int iLeft = iAngle - 1;
//					if(iLeft < 0)
//					{
//						iLeft = (iLeft + innerPointNumAngle) % innerPointNumAngle;
//					}
//					int iRight = (iAngle + 1) % innerPointNumAngle;

//					leftInnerPoint = innerPointArr[iLeft, iRad];
//					rightInnerPoint = innerPointArr[iRight, iRad];

					
//					if (iRad - 1 >= 0)
//					{
//						upInnerPoint = innerPointArr[iAngle, iRad - 1];
//					}
//					else
//					{
//						upInnerPoint = innerPointCenter;//가운데 점과 연결한다.
//					}
//					if (iRad + 1 < innerPointNumRadius)
//					{
//						downInnerPoint = innerPointArr[iAngle, iRad + 1];
//					}
					

//					//Left 추가
//					curInnerPoint._linkedPoint.Add(leftInnerPoint);

//					//Right 추가
//					curInnerPoint._linkedPoint.Add(rightInnerPoint);
//					curInnerPoint._linkedPoint_GUI.Add(rightInnerPoint);
					
//					if (upInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(upInnerPoint);
//						if(upInnerPoint == innerPointCenter)
//						{
//							//Center에서 방사형으로 나가도록 연결
//							innerPointCenter._linkedPoint.Add(curInnerPoint);
//							innerPointCenter._linkedPoint_GUI.Add(curInnerPoint);
//						}
//					}
//					if (downInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(downInnerPoint);
//						curInnerPoint._linkedPoint_GUI.Add(downInnerPoint);
//					}

//					//마지막으로 리스트에 넣자
//					_innerPoints.Add(curInnerPoint);

//					if (iRad == innerPointNumRadius - 1)
//					{
//						//InnerPoint 중에 외곽에 해당한다면 리스트에 추가
//						outerInnerPoints.Add(curInnerPoint);

//						//Normal도 계산한다.
//						curInnerPoint._isOuter = true;
//						curInnerPoint._normalIfOuter = (curInnerPoint._pos - posCenter).normalized;
//						curInnerPoint._properDistIfOuter = minDistControlPointAxis_Radius * 0.5f;

//						//외곽이면 우선 순위가 2이다.
//						curInnerPoint._priority = 2;
//					}
//					else if(iRad == 0)
//					{
//						//Center 바로 인근이면 1
//						curInnerPoint._priority = 1;
//					}
//				}
//			}

//			_innerPoints.Add(innerPointCenter);
			
//			//우선순위 계산
//			MakePriority();

//			//추가 >> 각 지점에서 축에 대한 끝점을 서로 연결해야한다.
//			for (int iAngle = 0; iAngle < innerPointNumAngle; iAngle++)
//			{
//				for (int iRad = 0; iRad < innerPointNumRadius; iRad++)
//				{
//					curInnerPoint = innerPointArr[iAngle, iRad];

//					InnerPoint outEnd = innerPointArr[iAngle, innerPointNumRadius - 1];
//					if(curInnerPoint != outEnd)
//					{
//						curInnerPoint._linkedPoint_EndOut_Axis2.Add(outEnd);
//					}
//					curInnerPoint._linkedPoint_EndOut_Axis2.Add(innerPointCenter);//<<Center는 자동으로 넣는다.
//				}
//			}
//			//반대로 Center에는 맞은편 모든 외곽점을 연결한다.
//			for (int iAngle = 0; iAngle < innerPointNumAngle; iAngle++)
//			{
//				innerPointCenter._linkedPoint_EndOut_Axis2.Add(innerPointArr[iAngle, innerPointNumRadius - 1]);
//			}



//			//4. OuterPoint와 가장자리의 InnerPoint를 연결한다.
//			//- 빠짐없이 최소 거리로 각각 최소 1개씩 연결한다.
//			//- InnerPoint의 Normal과 Inner -> Out의 벡터의 각도를 계산해서, 가중치를 넣는다.

//			List<InnerPoint> addedInnerPoints = new List<InnerPoint>();

//			for (int iOutPoint = 0; iOutPoint < _outerPoints_Preview.Count; iOutPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iOutPoint];
				

//				//가장 가까운 InnerPoint를 찾자
//				float minDist = float.MaxValue;
//				InnerPoint minPoint = null;
//				float deltaAngle = 0.0f;
//				float angleWeight = 0.0f;

//				for (int iInnerPoint = 0; iInnerPoint < outerInnerPoints.Count; iInnerPoint++)
//				{
//					curInnerPoint = outerInnerPoints[iInnerPoint];

//					float curDist = Vector2.Distance(curOutPoint._pos, curInnerPoint._pos);
//					if (curDist > 0.0f)
//					{
//						deltaAngle = Vector2.Angle(curInnerPoint._normalIfOuter, curOutPoint._pos - curInnerPoint._pos);
//						angleWeight = (Mathf.Clamp(deltaAngle, 0.0f, 120.0f) / 120.0f);
//						//0 : 0도 ~ 1 : 120도
//						//>> 0 : 0도 ~ 4 : 120도
//						//>> 1 : 0도 ~ 5 : 120도
//						angleWeight = (angleWeight * 4) + 1;
//						curDist *= angleWeight;
//					}


//					if (curDist < minDist || minPoint == null)
//					{
//						minPoint = curInnerPoint;
//						minDist = curDist;
//					}
//				}

//				//최단 거리의 Inner Point와 연결하자
//				if (minPoint != null)
//				{
//					minPoint._linkedOuterPoints.Add(curOutPoint);
//					addedInnerPoints.Add(minPoint);
//				}
//			}

//			//반대로 처리가 안된 InnerPoint 기준으로 최단 거리의 Outer Point를 연결하자
//			for (int iInnerPoint = 0; iInnerPoint < outerInnerPoints.Count; iInnerPoint++)
//			{
//				curInnerPoint = outerInnerPoints[iInnerPoint];
//				if (addedInnerPoints.Contains(curInnerPoint))
//				{
//					continue;
//				}

//				//바깥쪽을 향한다면
//				float minDist = float.MaxValue;
//				OuterPoint minPoint = null;
//				float deltaAngle = 0.0f;
//				float angleWeight = 0.0f;

//				for (int iOutPoint = 0; iOutPoint < _outerPoints_Preview.Count; iOutPoint++)
//				{
//					curOutPoint = _outerPoints_Preview[iOutPoint];
//					float curDist = Vector2.Distance(curOutPoint._pos, curInnerPoint._pos);
//					if (curDist > 0.0f)
//					{
//						deltaAngle = Vector2.Angle(curInnerPoint._normalIfOuter, curOutPoint._pos - curInnerPoint._pos);
//						angleWeight = (Mathf.Clamp(deltaAngle, 0.0f, 120.0f) / 120.0f);
//						//0 : 0도 ~ 1 : 120도
//						//>> 0 : 0도 ~ 4 : 120도
//						//>> 1 : 0도 ~ 5 : 120도
//						angleWeight = (angleWeight * 4) + 1;
//						curDist *= angleWeight;
//					}

//					if (curDist < minDist || minPoint == null)
//					{
//						minPoint = curOutPoint;
//						minDist = curDist;
//					}
//				}

//				//최단 거리의 Inner Point와 연결하자
//				if (minPoint != null)
//				{
//					curInnerPoint._linkedOuterPoints.Add(minPoint);
//				}

//			}

//			//5. Relax를 조금 실행한다. (1회)
//			RelaxInnerPoints(1);
//		}





//		private void CreateInnerPoints_Ring()
//		{
//			//Ring 방식
//			//외경 > 내경로 이어지는 방식이다.
//			//각 좌표계(각 + 거리)로 이루어져있다.
//			//Circle과 거의 동일하지만, Center의 점 1개가 아닌 내경의 OuterPoint가 있으므로 잘 연결해야한다.
			

//			//1. Divide 개수를 이용해서 대략 버텍스의 간격을 추측하자.
//			//2. 외곽 선분들 중에서 그 간격보다 너무 긴 선분은 미리 쪼개주자 (별도의 플래그를 걸고 삭제될 수 있게)
//			//3. 각 좌표계에 따른 Inner Point를 생성한다.
//			//- 내부 교차점에서는 "이동 제한이 걸린" Inner Point를 생성한다. <옵션에 따라서>
//			//4. OuterPoint와 가장자리의 InnerPoint를 연결한다.
//			//- 빠짐없이 최소 거리로 각각 최소 1개씩 연결한다.
//			//5. Relax를 조금 실행한다.
			
//			apMeshGenMapper.ControlPoint[] controlPointOuterArr = Mapper.GetRingControlPointArr(true);
//			apMeshGenMapper.ControlPoint[] controlPointInnerArr = Mapper.GetRingControlPointArr(false);
//			//List<apMeshGenMapper.ControlPoint> controlPointList = Mapper.GetControlPointList(apMeshGenMapper.MapperShape.Ring);
			
//			apMeshGenMapper.ControlPoint curPoint = null;
//			apMeshGenMapper.ControlPoint nextPoint = null;

//			_outerPoints_Preview.Clear();
			

//			//1. Divide 개수를 이용해서 대략 버텍스의 간격을 추측하자.
//			int nDivide = Mathf.Max(_editor._meshAutoGenOption_GridDivide, 1);

//			//각 Control Point간의 거리의 평균을 구하자
//			//Outer 사이의 간격만 구한다.
//			float dist = 0.0f;
//			float totalDist = 0.0f;
//			int nDistCalculated = 0;
//			for (int iPoint = 0; iPoint < controlPointOuterArr.Length; iPoint++)
//			{
//				curPoint = controlPointOuterArr[iPoint];
//				if(curPoint._linkedOuterPoints.Count > 0)
//				{
//					for (int iNext = 0; iNext < curPoint._linkedOuterPoints.Count; iNext++)
//					{
//						nextPoint = curPoint._linkedOuterPoints[iNext];
//						dist = Vector2.Distance(nextPoint._pos_Cur, curPoint._pos_Cur);
//						totalDist += dist;
//						nDistCalculated++;
//					}
//				}
//			}

//			float avgDist = 0.0f;
//			if(nDistCalculated > 0)
//			{
//				avgDist = totalDist / nDistCalculated;
//			}
//			else
//			{
//				avgDist = 500;
//			}
//			avgDist /= nDivide;
//			if(avgDist < 5)
//			{
//				avgDist = 5;
//			}

//			//2. 외곽 선분들 중에서 그 간격보다 너무 긴 선분은 미리 쪼개주자 (별도의 플래그를 걸고 삭제될 수 있게)
//			//- 동시에 플래그가 있었던 Tmp OuterPoint는 미리 삭제한다.

//			//OuterPoint 리스트를 통합으로 만들자.
//			Dictionary<OuterPoint, OuterPoint> outerPointSrc2Copy = new Dictionary<OuterPoint, OuterPoint>();
//			Dictionary<OuterPoint, OuterPoint> outerPointCopy2Src = new Dictionary<OuterPoint, OuterPoint>();

//			ScanTileGroup curGroup = null;
//			OuterPoint curOutPoint = null;
//			float margin = Mathf.Max(_editor._meshAutoGenOption_Margin, 0);
//			for (int iGroup = 0; iGroup < _outlineGroups.Count; iGroup++)
//			{
//				curGroup = _outlineGroups[iGroup];
//				if(!curGroup._isEnabled)
//				{	
//					continue;//비활성화 되었다면 생략
//				}
//				for (int iPoint = 0; iPoint < curGroup._outerPoints.Count; iPoint++)
//				{
//					curOutPoint = curGroup._outerPoints[iPoint];
//					OuterPoint newPoint = new OuterPoint(curOutPoint);

//					_outerPoints_Preview.Add(newPoint);

//					outerPointSrc2Copy.Add(curOutPoint, newPoint);
//					outerPointCopy2Src.Add(newPoint, curOutPoint);
//				}
//			}

			

//			//Next/Prev를 Mapping을 이용해서 연결하자
//			OuterPoint srcOutPoint = null;
//			for (int iPoint = 0; iPoint < _outerPoints_Preview.Count; iPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iPoint];
//				srcOutPoint = outerPointCopy2Src[curOutPoint];

//				curOutPoint.CopyLink(srcOutPoint, outerPointSrc2Copy);
//			}

//			//Margin 만큼 확장한다.
//			ExtendOuterPointsPreview(margin, outerPointSrc2Copy, outerPointCopy2Src);
			
//			//리스트를 돌면서 Next와의 거리가 멀다면 나누어주자 (일단 별도의 리스트를 만들어서 나중에 참조)
//			List<OuterPoint> splitOutPoints = new List<OuterPoint>();
//			OuterPoint nextOutPoint = null;
//			for (int iPoint = 0; iPoint < _outerPoints_Preview.Count; iPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iPoint];
//				if(curOutPoint._nextPoint == null || splitOutPoints.Contains(curOutPoint._nextPoint))
//				{
//					continue;
//				}

//				nextOutPoint = curOutPoint._nextPoint;
//				dist = Vector2.Distance(curOutPoint._pos, nextOutPoint._pos);
//				if(dist > avgDist * 1.5f)
//				{
//					//너무 멀리 떨어져있다.
//					int nSplit = Mathf.Max((int)(dist / (avgDist * 1.2f)), 1);
//					OuterPoint prevOutPoint = curOutPoint;
//					for (int iSplit = 0; iSplit < nSplit; iSplit++)
//					{
//						OuterPoint newSplitOutPoint = new OuterPoint();
//						newSplitOutPoint.SetSplit(curOutPoint, nextOutPoint, iSplit, nSplit);

//						newSplitOutPoint._prevPoint = prevOutPoint;
//						prevOutPoint._nextPoint = newSplitOutPoint;

//						prevOutPoint = newSplitOutPoint;

//						splitOutPoints.Add(newSplitOutPoint);
//					}

//					//마지막 끝 점 처리
//					nextOutPoint._prevPoint = prevOutPoint;
//					prevOutPoint._nextPoint = nextOutPoint;
//				}
//			}

//			if(splitOutPoints.Count > 0)
//			{
//				//분할된 포인트를 리스트에 합쳐준다.
//				for (int iSplit = 0; iSplit < splitOutPoints.Count; iSplit++)
//				{
//					_outerPoints_Preview.Add(splitOutPoints[iSplit]);
//				}
//			}
			

//			//3. 각 좌표계에 따른 Inner Point를 생성한다.
//			//- 내부 교차점에서는 "이동 제한이 걸린" Inner Point를 생성한다. <옵션에 따라서>
//			_innerPoints.Clear();
//			_maxInnerPointPriority = 0;

//			bool isLockAxis = _editor._meshAutoGenOption_IsLockAxis;


//			int controlPointNumAngle = controlPointOuterArr.Length;
//			int controlPointNumRadius = 2;//Center ~ Outer 2개밖에 없다.
//			int innerPointNumAngle = controlPointNumAngle + (nDivide - 1) * (controlPointNumAngle);
//			int innerPointNumRadius = controlPointNumRadius + (nDivide - 1) * (controlPointNumRadius - 1);

//			//Control Point간의 최소 거리를 구한다.
//			//외곽의 InnerPoint의 여백에 이용된다.
//			float minDistControlPointAxis_Radius = 20.0f;
//			for (int iCAngle = 0; iCAngle < controlPointOuterArr.Length; iCAngle++)
//			{
//				apMeshGenMapper.ControlPoint cp_Out = controlPointOuterArr[iCAngle];
//				apMeshGenMapper.ControlPoint cp_In = controlPointInnerArr[iCAngle];

//				float ctDst = Vector2.Distance(cp_In._pos_Cur, cp_Out._pos_Cur);

//				if(ctDst < minDistControlPointAxis_Radius)
//				{
//					minDistControlPointAxis_Radius = ctDst;
//				}
//			}
//			minDistControlPointAxis_Radius /= ((innerPointNumRadius - 1) + 3);


//			InnerPoint[,] innerPointArr = new InnerPoint[innerPointNumAngle, innerPointNumRadius];
			
			
//			for (int iAngle = 0; iAngle < innerPointNumAngle; iAngle++)
//			{
//				int iControlPointAngle_A = iAngle / nDivide;
//				int iControlPointAngle_B = (iControlPointAngle_A + 1) % controlPointNumAngle;
//				bool isOnAxisAngle = false;
//				if (iAngle % nDivide == 0)
//				{
//					//중간 점이 아니면 (나머지 = 0)
//					isOnAxisAngle = true;
//				}
//				float lerpAngle = (iAngle % nDivide) / (float)nDivide;
//				for (int iRadius = 0; iRadius < innerPointNumRadius; iRadius++)
//				{
//					float lerpRad = (float)iRadius / (float)(innerPointNumRadius - 1);
//					//이걸 수정해서 0.5에 몰리도록 하자
//					//0~1 -> 0~0.2 -> 0.4~0.6
//					float realLerpRad = lerpRad;
//					lerpRad = (lerpRad * 0.2f) + 0.4f;

//					Vector2 posOut_L = Vector2.zero;
//					Vector2 posOut_R = Vector2.zero;
//					Vector2 posIn_L = Vector2.zero;
//					Vector2 posIn_R = Vector2.zero;

//					apMeshGenMapper.ControlPoint cp_OutL = controlPointOuterArr[iControlPointAngle_A];
//					apMeshGenMapper.ControlPoint cp_OutR = controlPointOuterArr[iControlPointAngle_B];
//					apMeshGenMapper.ControlPoint cp_InL = controlPointInnerArr[iControlPointAngle_A];
//					apMeshGenMapper.ControlPoint cp_InR = controlPointInnerArr[iControlPointAngle_B];

//					posIn_L = cp_InL._pos_Cur;
//					posIn_R = cp_InR._pos_Cur;
//					posOut_L = cp_OutL._pos_Cur;
//					posOut_R = cp_OutR._pos_Cur;

//					Vector2 dirL = posOut_L - posIn_L;
//					Vector2 dirR = posOut_R - posIn_R;
//					//float radiusL = dirL.magnitude;
//					//float radiusR = dirR.magnitude;
//					Vector2 pos_L = posIn_L * (1.0f - lerpRad) + posOut_L * lerpRad;
//					Vector2 pos_R = posIn_R * (1.0f - lerpRad) + posOut_R * lerpRad;
					

//					Vector2 pos = pos_L * (1.0f - lerpAngle) + pos_R * lerpAngle;
//					InnerPoint newPoint = new InnerPoint(pos, lerpAngle, realLerpRad);

//					newPoint._normalIfOuter = (dirL.normalized * (1.0f - lerpAngle) + dirR.normalized * lerpAngle).normalized;

//					//이동 제한
//					if (isLockAxis)
//					{
//						if (isOnAxisAngle)
//						{
//							newPoint._moveLock = INNER_MOVE_LOCK.AxisLimited;
//							newPoint._limitedPosA = posIn_L;
//							newPoint._limitedPosB = posOut_L;
//						}
//					}

//					innerPointArr[iAngle, iRadius] = newPoint;
//				}
//			}

			
//			//이제 연결을 하자
//			InnerPoint curInnerPoint = null;
//			InnerPoint leftInnerPoint = null;
//			InnerPoint rightInnerPoint = null;
//			InnerPoint upInnerPoint = null;
//			InnerPoint downInnerPoint = null;
//			List<InnerPoint> outerInnerPoints = new List<InnerPoint>();
//			for (int iAngle = 0; iAngle < innerPointNumAngle; iAngle++)
//			{
//				for (int iRad = 0; iRad < innerPointNumRadius; iRad++)
//				{
//					curInnerPoint = innerPointArr[iAngle, iRad];

//					leftInnerPoint = null;
//					rightInnerPoint = null;
//					upInnerPoint = null;
//					downInnerPoint = null;

//					int iLeft = iAngle - 1;
//					if(iLeft < 0)
//					{
//						iLeft = (iLeft + innerPointNumAngle) % innerPointNumAngle;
//					}
//					int iRight = (iAngle + 1) % innerPointNumAngle;

//					leftInnerPoint = innerPointArr[iLeft, iRad];
//					rightInnerPoint = innerPointArr[iRight, iRad];

					
//					if (iRad - 1 >= 0)
//					{
//						upInnerPoint = innerPointArr[iAngle, iRad - 1];
//					}
//					if (iRad + 1 < innerPointNumRadius)
//					{
//						downInnerPoint = innerPointArr[iAngle, iRad + 1];
//					}
					

//					//Left 추가
//					curInnerPoint._linkedPoint.Add(leftInnerPoint);

//					//Right 추가
//					curInnerPoint._linkedPoint.Add(rightInnerPoint);
//					curInnerPoint._linkedPoint_GUI.Add(rightInnerPoint);
					
//					if (upInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(upInnerPoint);
//					}
//					if (downInnerPoint != null)
//					{
//						curInnerPoint._linkedPoint.Add(downInnerPoint);
//						curInnerPoint._linkedPoint_GUI.Add(downInnerPoint);
//					}

//					//마지막으로 리스트에 넣자
//					_innerPoints.Add(curInnerPoint);

//					if(iRad == 0)
//					{
//						//안쪽으로 향한다면
//						outerInnerPoints.Add(curInnerPoint);
//						curInnerPoint._isOuter = true;
//						curInnerPoint._isDirToInnerRadius = true;
//						//계산된 Normal을 뒤집자
//						curInnerPoint._normalIfOuter *= -1;
//						curInnerPoint._priority = 1;//<<내경이면 우선순위 1
//						curInnerPoint._properDistIfOuter = minDistControlPointAxis_Radius * 0.5f;
//					}
//					else if (iRad == innerPointNumRadius - 1)
//					{
//						//바깥쪽으로 향한다면
//						//InnerPoint 중에 외곽에 해당한다면 리스트에 추가
//						outerInnerPoints.Add(curInnerPoint);

//						//Normal도 계산한다. > 여기선 이미 계산되어있다.
//						curInnerPoint._isOuter = true;
//						curInnerPoint._isDirToInnerRadius = false;
//						curInnerPoint._priority = 2;//<<외곽이면 우선순위 2(최상)
//						curInnerPoint._properDistIfOuter = minDistControlPointAxis_Radius * 0.5f;
//					}
//				}
//			}

//			//_maxInnerPointPriority = 2;
			
//			//우선순위 계산
//			//_maxInnerPointPriority = 1;
//			MakePriority();
			
//			//추가 >> 각 지점에서 축에 대한 끝점을 서로 연결해야한다.
//			for (int iAngle = 0; iAngle < innerPointNumAngle; iAngle++)
//			{
//				for (int iRad = 0; iRad < innerPointNumRadius; iRad++)
//				{
//					curInnerPoint = innerPointArr[iAngle, iRad];

//					InnerPoint outEnd = innerPointArr[iAngle, innerPointNumRadius - 1];
//					InnerPoint inEnd = innerPointArr[iAngle, 0];
//					if(curInnerPoint != outEnd)
//					{
//						curInnerPoint._linkedPoint_EndOut_Axis2.Add(outEnd);
//					}
//					if(curInnerPoint != inEnd)
//					{
//						curInnerPoint._linkedPoint_EndOut_Axis2.Add(inEnd);
//					}
//				}
//			}


//			//4. OuterPoint와 가장자리의 InnerPoint를 연결한다.
//			//- 빠짐없이 최소 거리로 각각 최소 1개씩 연결한다.
//			//- InnerPoint의 Normal과 Inner -> Out의 벡터의 각도를 계산해서, 가중치를 넣는다.

//			List<InnerPoint> addedInnerPoints = new List<InnerPoint>();

//			for (int iOutPoint = 0; iOutPoint < _outerPoints_Preview.Count; iOutPoint++)
//			{
//				curOutPoint = _outerPoints_Preview[iOutPoint];
				

//				//가장 가까운 InnerPoint를 찾자
//				float minDist = float.MaxValue;
//				InnerPoint minPoint = null;
//				float deltaAngle = 0.0f;
//				float angleWeight = 0.0f;

//				for (int iInnerPoint = 0; iInnerPoint < outerInnerPoints.Count; iInnerPoint++)
//				{
//					curInnerPoint = outerInnerPoints[iInnerPoint];

//					float curDist = Vector2.Distance(curOutPoint._pos, curInnerPoint._pos);
//					if (curDist > 0.0f)
//					{
//						deltaAngle = Vector2.Angle(curInnerPoint._normalIfOuter, curOutPoint._pos - curInnerPoint._pos);
//						angleWeight = (Mathf.Clamp(deltaAngle, 0.0f, 120.0f) / 120.0f);
//						//0 : 0도 ~ 1 : 120도
//						//>> 0 : 0도 ~ 4 : 120도
//						//>> 1 : 0도 ~ 5 : 120도
//						angleWeight = (angleWeight * 4) + 1;
//						curDist *= angleWeight;
//					}


//					if (curDist < minDist || minPoint == null)
//					{
//						minPoint = curInnerPoint;
//						minDist = curDist;
//					}
//				}

//				//최단 거리의 Inner Point와 연결하자
//				if (minPoint != null)
//				{
//					minPoint._linkedOuterPoints.Add(curOutPoint);
//					addedInnerPoints.Add(minPoint);
//				}
//			}

//			//반대로 처리가 안된 InnerPoint 기준으로 최단 거리의 Outer Point를 연결하자
//			for (int iInnerPoint = 0; iInnerPoint < outerInnerPoints.Count; iInnerPoint++)
//			{
//				curInnerPoint = outerInnerPoints[iInnerPoint];
//				if (addedInnerPoints.Contains(curInnerPoint))
//				{
//					continue;
//				}

//				//바깥쪽을 향한다면
//				float minDist = float.MaxValue;
//				OuterPoint minPoint = null;
//				float deltaAngle = 0.0f;
//				float angleWeight = 0.0f;

//				for (int iOutPoint = 0; iOutPoint < _outerPoints_Preview.Count; iOutPoint++)
//				{
//					curOutPoint = _outerPoints_Preview[iOutPoint];
//					float curDist = Vector2.Distance(curOutPoint._pos, curInnerPoint._pos);
//					if (curDist > 0.0f)
//					{
//						deltaAngle = Vector2.Angle(curInnerPoint._normalIfOuter, curOutPoint._pos - curInnerPoint._pos);
//						angleWeight = (Mathf.Clamp(deltaAngle, 0.0f, 120.0f) / 120.0f);
//						//0 : 0도 ~ 1 : 120도
//						//>> 0 : 0도 ~ 4 : 120도
//						//>> 1 : 0도 ~ 5 : 120도
//						angleWeight = (angleWeight * 4) + 1;
//						curDist *= angleWeight;
//					}

//					if (curDist < minDist || minPoint == null)
//					{
//						minPoint = curOutPoint;
//						minDist = curDist;
//					}
//				}

//				//최단 거리의 Inner Point와 연결하자
//				if (minPoint != null)
//				{
//					curInnerPoint._linkedOuterPoints.Add(minPoint);
//				}

//			}

//			//5. Relax를 조금 실행한다. (1회)
//			RelaxInnerPoints(1);
//		}



//		//---------------------------------------------------------------
		
//		//단계 3 - Generate
//		public void Step3_Generate(bool isRemovePreviousVertices)
//		{
//			if(!IsScanned || !IsPreviewed || !IsScanable())
//			{
//				return;
//			}
//			if(_targetImage == null || _targetMesh == null)
//			{
//				return;
//			}
//			try
//			{
//				//Undo에 등록
//				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AutoGen, _editor, _targetMesh, _targetMesh, false);

//				if (isRemovePreviousVertices)
//				{
//					//버텍스 모두 삭제

//					_targetMesh._vertexData.Clear();
//					_targetMesh._indexBuffer.Clear();
//					_targetMesh._edges.Clear();
//					_targetMesh._polygons.Clear();

//					_targetMesh.MakeEdgesToPolygonAndIndexBuffer();

//					_editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

//					_editor.VertController.UnselectVertex();
//					_editor.VertController.UnselectNextVertex();

//				}

//				//_outerPoints_Preview//<<이거랑
//				//_innerPoints//<<이거를 서로 연결합시당

//				//아래의 함수를 사용합시다.
				
//				List<RawDataVertexPair> rawDataVertPairs = new List<RawDataVertexPair>();
//				Dictionary<apVertex, RawDataVertexPair> vertex2RawData = new Dictionary<apVertex, RawDataVertexPair>();
//				Dictionary<OuterPoint, RawDataVertexPair> outPoint2RawData = new Dictionary<OuterPoint, RawDataVertexPair>();
//				Dictionary<InnerPoint, RawDataVertexPair> inPoint2RawData = new Dictionary<InnerPoint, RawDataVertexPair>();
//				Dictionary<RawDataVertexPair, List<RawDataVertexPair>> linkedRawDataVertPairs = new Dictionary<RawDataVertexPair, List<RawDataVertexPair>>();

//				Vector2 imageHalfOffset = new Vector2(_targetTextureData._width * 0.5f, _targetTextureData._height * 0.5f);
//				//Vector2 meshOffset = _targetMesh._offsetPos;

//				//Vector2 worldOffset = meshOffset + imageHalfOffset;
//				Vector2 worldOffset = imageHalfOffset;

//				OuterPoint curOutPoint = null;
//				InnerPoint curInPoint = null;
//				RawDataVertexPair curPair = null;
//				OuterPoint nextOutPoint = null;
//				InnerPoint nextInPoint = null;
//				RawDataVertexPair nextPair = null;

//				Vector2 posW = Vector2.zero;
//				int nResult = 0;

//				//OuterPoint -> Vertex 추가
//				for (int iOut = 0; iOut < _outerPoints_Preview.Count; iOut++)
//				{
//					curOutPoint = _outerPoints_Preview[iOut];
//					posW = curOutPoint._pos - worldOffset;

//					apVertex vert = _targetMesh.AddVertexAutoUV(posW);
//					if(vert != null)
//					{
//						RawDataVertexPair newPair = new RawDataVertexPair(vert, curOutPoint);
//						rawDataVertPairs.Add(newPair);
//						vertex2RawData.Add(vert, newPair);
//						outPoint2RawData.Add(curOutPoint, newPair);
//						linkedRawDataVertPairs.Add(newPair, new List<RawDataVertexPair>());
//					}
//				}

//				//InnerPoint -> Vertex 추가
//				for (int iIn = 0; iIn < _innerPoints.Count; iIn++)
//				{
//					curInPoint = _innerPoints[iIn];
//					posW = curInPoint._pos - worldOffset;

//					apVertex vert = _targetMesh.AddVertexAutoUV(posW);
//					if(vert != null)
//					{
//						RawDataVertexPair newPair = new RawDataVertexPair(vert, curInPoint);
//						rawDataVertPairs.Add(newPair);
//						vertex2RawData.Add(vert, newPair);
//						inPoint2RawData.Add(curInPoint, newPair);
//						linkedRawDataVertPairs.Add(newPair, new List<RawDataVertexPair>());
//					}
//				}


//				//RawData -> Edge 추가
//				for (int iPair = 0; iPair < rawDataVertPairs.Count; iPair++)
//				{
//					curPair = rawDataVertPairs[iPair];
//					nextPair = null;
//					if(curPair._outerPoint != null)
//					{
//						//1. Outer Point
//						curOutPoint = curPair._outerPoint;

//						nextOutPoint = curOutPoint._nextPoint;
//						if(nextOutPoint == null)
//						{
//							continue;
//						}

//						nextPair = outPoint2RawData[nextOutPoint];

//						//Edge 생성
//						bool isResult = MakeEdgeBetwenToPairs(curPair, nextPair, linkedRawDataVertPairs);
//						if(isResult)
//						{
//							nResult++;
//						}
//					}
//					else if(curPair._innerPoint != null)
//					{
//						//2. Inner Point
//						curInPoint = curPair._innerPoint;


//						//Inner -> Outer
//						for (int iNext = 0; iNext < curInPoint._linkedOuterPoints.Count; iNext++)
//						{
//							nextOutPoint = curInPoint._linkedOuterPoints[iNext];
//							if(nextOutPoint == null)
//							{
//								continue;
//							}

//							nextPair = outPoint2RawData[nextOutPoint];

//							//Edge 생성
//							bool isResult = MakeEdgeBetwenToPairs(curPair, nextPair, linkedRawDataVertPairs);
//							if(isResult)
//							{
//								nResult++;
//							}
//						}

//						//Inner -> Inner
//						for (int iNext = 0; iNext < curInPoint._linkedPoint.Count; iNext++)
//						{
//							nextInPoint = curInPoint._linkedPoint[iNext];
//							if(nextInPoint == null)
//							{
//								continue;
//							}

//							nextPair = inPoint2RawData[nextInPoint];

//							//Edge 생성
//							bool isResult = MakeEdgeBetwenToPairs(curPair, nextPair, linkedRawDataVertPairs);
//							if(isResult)
//							{
//								nResult++;
//							}
//						}
						
//					}

					
//				}

//				//Debug.Log("Added Edges : " + nResult);

//				//Outer Point 끼리는 폴리곤으로 생성되면 안된다.
//				PolygonExceptionSet exceptionSet = new PolygonExceptionSet();
//				for (int iOut = 0; iOut < _outerPoints_Preview.Count; iOut++)
//				{
//					exceptionSet.AddVertex(outPoint2RawData[_outerPoints_Preview[iOut]]._vert);
//				}
//				List<PolygonExceptionSet> exceptionSetList = new List<PolygonExceptionSet>();
//				exceptionSetList.Add(exceptionSet);

//				_step = STEP.Completed;

//				//Make Polygon
//				_targetMesh.MakeEdgesToPolygonAndIndexBuffer(exceptionSetList);
//				_targetMesh.RefreshPolygonsToIndexBuffer();
//				_editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
//				_editor.SetRepaint();

//			}
//			catch(Exception ex)
//			{
//				Debug.LogError("AnyPortrait : Mesh Generate Exception : " + ex);
//			}
//		}

//		private bool MakeEdgeBetwenToPairs(RawDataVertexPair curPair, RawDataVertexPair nextPair, Dictionary<RawDataVertexPair, List<RawDataVertexPair>> linkedRawDataVertPairs)
//		{
//			//이미 Edge로 등록 되었는지 확인
//			bool isEdgeExisted = false;
//			if(linkedRawDataVertPairs[curPair].Contains(nextPair))
//			{
//				isEdgeExisted = true;
//			}
//			else if(linkedRawDataVertPairs[nextPair].Contains(curPair))
//			{
//				isEdgeExisted = true;
//			}
//			if(isEdgeExisted)
//			{
//				return false;
//			}

//			//Edge 추가
//			_targetMesh.MakeNewEdge(curPair._vert, nextPair._vert, false);
//			linkedRawDataVertPairs[curPair].Add(nextPair);
//			linkedRawDataVertPairs[nextPair].Add(curPair);

//			return true;
//		}

//		//---------------------------------------------------------------
//		/// <summary>
//		/// 우선 순위를 만든다.
//		/// </summary>
//		private void MakePriority()
//		{
//			InnerPoint curInPoint = null;
//			InnerPoint nextInPoint = null;

//			_maxInnerPointPriority = 0;
//			bool isAnyOuter = false;

//			for (int i = 0; i < _innerPoints.Count; i++)
//			{
//				curInPoint = _innerPoints[i];
//				curInPoint._priority = 0;
//				//처음의 Outer만 계산
//				if(curInPoint._isOuter)
//				{
//					curInPoint._priority = 1;
//					isAnyOuter = true;
//				}
//			}
//			if(!isAnyOuter)
//			{
//				//Outer가 없다면 ??
//				//모두 1
//				for (int i = 0; i < _innerPoints.Count; i++)
//				{
//					curInPoint = _innerPoints[i];
//					curInPoint._priority = 1;
//				}
//				_maxInnerPointPriority = 1;
//				//끝
//				return;
//			}

//			int curPriority = 2;
//			_maxInnerPointPriority = 2;
//			while(true)
//			{
//				bool isAnyNotCalculated = false;
//				bool isAnyCalculated = false;
				
				
//				for (int i = 0; i < _innerPoints.Count; i++)
//				{
//					curInPoint = _innerPoints[i];
//					if(curInPoint._priority > 0)
//					{
//						//이미 계산되었다.
//						//패스
//						continue;
//					}
//					if(curInPoint._linkedPoint.Count > 0)
//					{
//						//주변에 계산된게 하나 있다면
//						//>> 현재 우선 순위를 지정받을 수 있다.
//						bool isAnyNearCalculated = false;
//						for (int iNext = 0; iNext < curInPoint._linkedPoint.Count; iNext++)
//						{
//							nextInPoint = curInPoint._linkedPoint[iNext];
//							if(nextInPoint._priority > 0)
//							{
//								isAnyNearCalculated = true;
//								break;
//							}
//						}

//						if(isAnyNearCalculated)
//						{
//							curInPoint._priority = curPriority;
//						}
//						else
//						{
//							//연결된게 있지만, 아직 계산할 수 없다.
//							isAnyNotCalculated = true;
//							continue;
//						}
//					}
//					else
//					{
//						//? 연결된게 없네요. 그냥 받으셈
//						curInPoint._priority = curPriority;
//					}
//					isAnyCalculated = true;
//					_maxInnerPointPriority = curPriority;
//				}

//				if(isAnyNotCalculated)
//				{
//					//만약 계산되지 않은게 하나라도 있다면
//					//다음 루프로 이동
//					curPriority++;
//				}
//				else
//				{
//					//만약 계산되지 않은게 없다면 (모두 계산되었다면?)
//					if(isAnyCalculated)
//					{
//						//하나라도 계산되었다면
//						//끝
//						break;
//					}
//					else
//					{
//						//계산된 것도 없다?
//						//에러지만 끝
//						break;
//					}
//				}
//			}

//		}

//		/// <summary>
//		/// Preview 스텝 이후로 Inner Point를 펼쳐서 메시 모양에 맞춘다.
//		/// 입력값은 계산 횟수이다. 최소 1 이상 입력할 것.
//		/// </summary>
//		public void RelaxInnerPoints(int relaxCount, float moveRatio = 0.3f)
//		{
//			if(_innerPoints.Count == 0)
//			{
//				return;
//			}
//			//float moveRatio = Mathf.Clamp01(deltaTime * 0.3f);
//			if(relaxCount < 1)
//			{
//				relaxCount = 1;
//			}

//			InnerPoint curInnerPoint = null;

//			//평균 길이를 가져오자 (단 Outer와 연결된 경우 Normal이 예각이어야 함)
//			//평균 길이를 넘는 선분은 제한된다. (최대 힘이 결정됨)
//			float avgLinkDist = 0.0f;
//			int nDist = 0;

//			for (int iPoint = 0; iPoint < _innerPoints.Count; iPoint++)
//			{
//				curInnerPoint = _innerPoints[iPoint];

//				if (curInnerPoint._linkedOuterPoints.Count > 0)
//				{
//					Vector2 dirToOuter = Vector2.zero;
//					OuterPoint linkedOuterPoint = null;

//					for (int iLink = 0; iLink < curInnerPoint._linkedOuterPoints.Count; iLink++)
//					{
//						linkedOuterPoint = curInnerPoint._linkedOuterPoints[iLink];
//						dirToOuter = linkedOuterPoint._pos - curInnerPoint._pos;

//						if (Vector2.Dot(linkedOuterPoint._normal_Avg, dirToOuter) > 0)
//						{
//							avgLinkDist += dirToOuter.magnitude;

//						}
//					}

					
//				}

//				if (curInnerPoint._linkedPoint.Count > 0)
//				{
//					for (int iLink = 0; iLink < curInnerPoint._linkedPoint.Count; iLink++)
//					{
//						avgLinkDist += (curInnerPoint._linkedPoint[iLink]._pos - curInnerPoint._pos).magnitude;
//						nDist++;
//					}
//				}
//			}
//			if (nDist > 0)
//			{
//				avgLinkDist /= nDist;
//			}
			
//			moveRatio *= 0.2f;
//			relaxCount *= 2;


//			//지정된 횟수대로 실행한다.
//			//우선 순위 순서대로 한다.
//			for (int iCount = 0; iCount < relaxCount; iCount++)
//			{
//				//float moveRatio = 0.6f;

//				//일단 힘 합력을 계산해보자
//				for (int iPriority = 1; iPriority <= _maxInnerPointPriority; iPriority++)
//				{
//					int randomStart = UnityEngine.Random.Range(0, _innerPoints.Count * 3);
//					//int nCalculated = 0;
//					//bool isOverOutline = false;
//					for (int iPoint = 0; iPoint < _innerPoints.Count; iPoint++)
//					{
//						curInnerPoint = _innerPoints[(iPoint + randomStart) % _innerPoints.Count];
//						if (curInnerPoint._priority != iPriority)
//						{
//							continue;
//						}
//						//curInnerPoint._relaxForce = Vector2.zero;
//						curInnerPoint._relaxForce = CalculateRelaxForce(curInnerPoint, avgLinkDist, (iPriority == 0));
//					}

//					//힘의 합력대로 이동한다.
//					for (int iPoint = 0; iPoint < _innerPoints.Count; iPoint++)
//					{
//						curInnerPoint = _innerPoints[iPoint];
//						if (curInnerPoint._priority != iPriority)
//						{
//							continue;
//						}

//						Vector2 nextPos = curInnerPoint._pos + curInnerPoint._relaxForce * moveRatio;
//						if(!curInnerPoint._isNeedCorrectOuterPos)
//						{
//							//위치 보정이 필요 없는 경우
//						}
//						else
//						{
//							float dotVec = Vector2.Dot(nextPos - curInnerPoint._correctedPos, curInnerPoint._normalIfOuter);
							
//							//이게 0보다 크면, 밖으로 나갔다는 것
//							if(dotVec > 0.0f)
//							{
//								nextPos = curInnerPoint._correctedPos;
//							}
							
//						}
//						//이동 제한 옵션
//						if(curInnerPoint._moveLock == INNER_MOVE_LOCK.Locked)
//						{
//							//적용되지 않는다.
//						}
//						else if(curInnerPoint._moveLock == INNER_MOVE_LOCK.None)
//						{
//							//그냥 적용한다.
//							curInnerPoint._pos = nextPos;
//						}
//						else
//						{
//							//일부만 적용한다.
//							Vector2 limitA = curInnerPoint._limitedPosA;
//							Vector2 limitB = curInnerPoint._limitedPosB;
//							float distA2B = (limitB - limitA).magnitude;
//							if (distA2B > 0.0f)
//							{
//								Vector2 norA2B = (limitB - limitA).normalized;
//								float t = Vector2.Dot(nextPos - limitA, norA2B);
//								if (t < 0.0f)
//								{
//									//A 전에 위치한다.
//									nextPos = limitA;
//								}
//								else if (t < distA2B)
//								{
//									nextPos = (norA2B * t) + limitA;
//								}
//								else
//								{
//									//B 뒤로 넘어갔다.
//									nextPos = limitB;
//								}
//							}
//							else
//							{
//								nextPos = limitA;
//							}

//							curInnerPoint._pos = nextPos;
//						}
//					}
//				}
				
//			}
			
//		}

//		private Vector2 CalculateRelaxForce(InnerPoint innerPoint, float avgLinkDist, bool is2StepCheck)
//		{
//			Vector2 resultForce = Vector2.zero;
//			bool isOverOutline = false;
//			int nCalculated = 0;
//			float outerBias = 5.0f;
//			Vector2 dirToOuter = Vector2.zero;

//			innerPoint._isNeedCorrectOuterPos = false;

//			if (innerPoint._linkedOuterPoints.Count > 0)
//			{
//				//Vector2 outerForce = Vector2.zero;
//				//만약 OuterPoint의 Normal과 Inner2Outer가 둔각 (반대 방향)이면 안으로 넣을 동력이 없어진다.
//				//Inner2Outer의 거리가 짧아도 동력이 사라진다.
//				//Bias 만큼 뒤에서 잡아 당겨야 한다.

//				//추가 : 외곽의 점과 연결이 되었다면 > 위치를 보정해야한다.
//				//innerPoint._isNeedCorretOuterPos = true;
//				float properDist = Mathf.Max(innerPoint._properDistIfOuter, 2f);
//				innerPoint._correctedPos = Vector2.zero;
//				float totalCorrectionWeight = 0.0f;
				
//				OuterPoint linkedOuterPoint = null;
//				for (int iLink = 0; iLink < innerPoint._linkedOuterPoints.Count; iLink++)
//				{
//					linkedOuterPoint = innerPoint._linkedOuterPoints[iLink];
//					dirToOuter = linkedOuterPoint._pos - innerPoint._pos;

//					float correctWeight = Mathf.Abs(Mathf.Cos(Vector2.Angle(dirToOuter, innerPoint._normalIfOuter) * Mathf.Deg2Rad));

//					if (dirToOuter.sqrMagnitude < outerBias * outerBias
//						|| Vector2.Dot(linkedOuterPoint._normal_Avg, dirToOuter) < 0)
//					{
//						resultForce += -linkedOuterPoint._normal_Avg * avgLinkDist;
//						isOverOutline = true;//<<Outline 밖으로 나갔다.
//					}
//					else
//					{
//						//안에서 밖으로 나가는 일반적인 경우 (약간 더 힘이 가해진다)
//						resultForce += GetDistLimitedVector(dirToOuter, avgLinkDist) * 1.5f;
//					}

//					innerPoint._correctedPos += ((-1.0f * linkedOuterPoint._normal_Avg.normalized) * properDist + linkedOuterPoint._pos) * correctWeight;
//					totalCorrectionWeight += correctWeight;
					
//					nCalculated++;
//				}

//				if(totalCorrectionWeight > 0.0f)
//				{
//					innerPoint._isNeedCorrectOuterPos = true;
//					innerPoint._correctedPos /= totalCorrectionWeight;
//				}
//				else if(innerPoint._isOuter)
//				{
//					//Outer인데 Weight가 0인 경우 > 다들 선상에 있어서 90도였나보다.
//					innerPoint._isNeedCorrectOuterPos = true;
//					innerPoint._correctedPos = innerPoint._pos - (innerPoint._normalIfOuter.normalized * properDist);
//				}
//			}

//			//if(innerPoint._isOuter && !innerPoint._isNeedCorrectOuterPos)
//			//{
//			//	Debug.Log("Outer / Correct 문제 발생 (True / False) : " + innerPoint._linkedOuterPoints.Count);
//			//}

//			float interWeight = 0.8f;
//			if(isOverOutline)
//			{
//				interWeight = 0.3f;
//			}

//			InnerPoint linkedInnerPoint = null;
//			if (innerPoint._linkedPoint.Count > 0)
//			{
//				for (int iLink = 0; iLink < innerPoint._linkedPoint.Count; iLink++)
//				{
//					linkedInnerPoint = innerPoint._linkedPoint[iLink];
//					resultForce += GetDistLimitedVector(linkedInnerPoint._pos - innerPoint._pos, avgLinkDist) * interWeight;
//					nCalculated++;

//					//만약 2Step이 켜져 있다면,
//					//해당 LinkedPoint에 연결된 다른 Point (자신 제외)에 대해서 Relax를 체크해야한다.
//					if(is2StepCheck)
//					{
//						OuterPoint link2StepOutPoint = null;
//						InnerPoint link2StepInPoint = null;
//						//Out 먼저 (그래도 가중치가 높다)
//						float weight2Step_Out = 0.5f;
//						float weight2Step_In = 0.2f;
//						if(linkedInnerPoint._linkedOuterPoints.Count > 0)
//						{
							
//							for (int iOut = 0; iOut < linkedInnerPoint._linkedOuterPoints.Count; iOut++)
//							{
//								link2StepOutPoint = linkedInnerPoint._linkedOuterPoints[iOut];
//								if(innerPoint._linkedOuterPoints.Contains(link2StepOutPoint))
//								{
//									continue;
//								}

//								dirToOuter = link2StepOutPoint._pos - innerPoint._pos;

//								if (dirToOuter.sqrMagnitude < outerBias * outerBias
//									|| Vector2.Dot(link2StepOutPoint._normal_Avg, dirToOuter) < 0)
//								{
//									resultForce += -link2StepOutPoint._normal_Avg * avgLinkDist * weight2Step_Out;
//									isOverOutline = true;//<<Outline 밖으로 나갔다.
//								}
//								else
//								{
//									//안에서 밖으로 나가는 일반적인 경우 (약간 더 힘이 가해진다)
//									resultForce += GetDistLimitedVector(dirToOuter, avgLinkDist) * 1.5f * weight2Step_Out;
//								}

//								nCalculated++;
//							}
//						}

//						if(linkedInnerPoint._linkedPoint.Count > 0)
//						{
//							for (int iIn = 0; iIn < linkedInnerPoint._linkedPoint.Count; iIn++)
//							{
//								link2StepInPoint = linkedInnerPoint._linkedPoint[iIn];
//								if(link2StepInPoint == innerPoint)
//								{
//									continue;
//								}
//								if(innerPoint._linkedPoint.Contains(link2StepInPoint))
//								{
//									continue;
//								}

//								resultForce += GetDistLimitedVector(link2StepInPoint._pos - innerPoint._pos, avgLinkDist) * interWeight * weight2Step_In;
//								nCalculated++;
//							}
//						}
//					}
//				}
//			}


//			float axisWeight = 0.5f;
//			if (innerPoint._linkedPoint_EndOut_Axis1.Count > 0)
//			{
//				for (int iLink = 0; iLink < innerPoint._linkedPoint_EndOut_Axis1.Count; iLink++)
//				{
//					linkedInnerPoint = innerPoint._linkedPoint_EndOut_Axis1[iLink];
//					resultForce += GetDistLimitedVector(linkedInnerPoint._pos - innerPoint._pos, avgLinkDist) * axisWeight;
//					nCalculated++;
//				}
//			}
//			if (innerPoint._linkedPoint_EndOut_Axis2.Count > 0)
//			{
//				for (int iLink = 0; iLink < innerPoint._linkedPoint_EndOut_Axis2.Count; iLink++)
//				{
//					linkedInnerPoint = innerPoint._linkedPoint_EndOut_Axis2[iLink];
//					resultForce += GetDistLimitedVector(linkedInnerPoint._pos - innerPoint._pos, avgLinkDist) * axisWeight;
//					nCalculated++;
//				}
//			}

//			if (nCalculated > 0)
//			{
//				resultForce /= nCalculated;
//			}
//			return resultForce;
//		}

//		private Vector2 GetDistLimitedVector(Vector2 srcVector, float targetDist)
//		{
//			if(srcVector.sqrMagnitude > targetDist * targetDist)
//			{
//				return srcVector.normalized * (targetDist);
//			}
//			else
//			{
//				return srcVector;
//			}
//			//return srcVector.normalized * (srcVector.magnitude - targetDist);
//		}




//		/// <summary>
//		/// Margin 값 만큼 OuterPoint를 확장한다.
//		/// OuterPoint_Preview를 생성한 직후 (분할 전) 이 함수를 호출하자
//		/// </summary>
//		private void ExtendOuterPointsPreview(float marginExtended,	Dictionary<OuterPoint, OuterPoint> outerPointSrc2Copy,	Dictionary<OuterPoint, OuterPoint> outerPointCopy2Src)
//		{
//			//한번에 확장하는게 아니라
//			//최대 5차례에 거쳐서 확장을 한다.
//			//기본은 2픽셀씩 확장
//			int nExtend = (int)(marginExtended / 2.0f);
//			float remainedMargin = marginExtended;
//			float curMargin = 2.0f;
//			if(nExtend > 5)
//			{
//				curMargin = marginExtended / 5.0f;//(5번 증가할 정도)
//			}
//			for (int iExtend = 0; iExtend < nExtend; iExtend++)
//			{
//				if (iExtend < nExtend - 1)
//				{
//					remainedMargin -= curMargin;
//				}
//				else
//				{
//					//마지막은 남은 Margin 만큼 모두 이동
//					curMargin = remainedMargin;
//					remainedMargin = 0.0f;
//				}

//				//----------------------------------------------------
//				if (_outerPoints_Preview.Count == 0)
//				{
//					return;
//				}
//				OuterPoint startPoint = _outerPoints_Preview[0];
//				OuterPoint curOutPoint = null;
//				OuterPoint nextOutPoint = null;
//				//증가 방향을 정하자
//				//기본적으로는 그냥 Normal 방향으로 설정한다.
//				//만약 margin 범위 내에서 서로 교차되는 것이 있다면, 중점 + 중간 Normal에 대한 새로운 포인트 생성 (둘다 생성하는게 아니라, Next를 연속으로 삭제하고, 현재 포인트를 중점으로 이동)
//				List<OuterPoint> removablePoints = new List<OuterPoint>();
//				List<OuterPoint> nearPoints = new List<OuterPoint>();

//				int nCnt = 0;
//				int nPoints = _outerPoints_Preview.Count;
//				float dist_Org = 0.0f;
//				float dist_Moved = 0.0f;
//				float dist_HalfMoved = 0.0f;
//				Vector2 curOutPoint_ExtendedPos = Vector2.zero;
//				Vector2 nextOutPoint_ExtendedPos = Vector2.zero;

//				curOutPoint = startPoint;


//				while (true)
//				{

//					nearPoints.Clear();
//					nextOutPoint = curOutPoint._nextPoint;
//					curOutPoint_ExtendedPos = curOutPoint._pos + curOutPoint._normal_Avg.normalized * curMargin;

//					//거리가 가까운 것을 nearPoint에 모두 묶자
//					//만약 "조건에 해당하지 않는게 나오면 패스"
//					float biasRatio = 1.2f;

//					while (true)
//					{
//						if (nextOutPoint == startPoint || nextOutPoint == curOutPoint)
//						{
//							break;
//						}

//						nextOutPoint_ExtendedPos = nextOutPoint._pos + nextOutPoint._normal_Avg.normalized * curMargin;
//						dist_Org = Vector2.Distance(nextOutPoint._pos, curOutPoint._pos);
//						dist_Moved = Vector2.Distance(nextOutPoint_ExtendedPos, curOutPoint_ExtendedPos);
//						dist_HalfMoved = Vector2.Distance(nextOutPoint_ExtendedPos * 0.5f + nextOutPoint._pos * 0.5f, curOutPoint_ExtendedPos * 0.5f + curOutPoint._pos * 0.5f);
//						if (dist_Org > marginExtended * biasRatio && dist_Moved > marginExtended * biasRatio && dist_HalfMoved > marginExtended * biasRatio)
//						{
//							//모든 지점이 범위에서 벗어남
//							break;
//						}
//						//영역 안에 들어간다.
//						nearPoints.Add(nextOutPoint);

//						//다음으로 이동
//						nextOutPoint = nextOutPoint._nextPoint;
//					}

//					if (nearPoints.Count > 0)
//					{
//						//합쳐야 한다.
//						Vector2 mergedPos = Vector2.zero;
//						Vector2 mergedNormal = Vector2.zero;
//						int nMerged = 1 + nearPoints.Count; //Next + Cur

//						mergedPos = curOutPoint._pos;
//						mergedNormal = curOutPoint._normal_Avg.normalized;
//						OuterPoint lastNextPoint = null;

//						for (int i = 0; i < nearPoints.Count; i++)
//						{
//							nextOutPoint = nearPoints[i];
//							mergedPos += nextOutPoint._pos;
//							mergedNormal += nextOutPoint._normal_Avg.normalized;

//							lastNextPoint = nextOutPoint._nextPoint;

//							//삭제 목록에도 넣자
//							removablePoints.Add(nextOutPoint);
//						}

//						mergedPos /= nMerged;
//						mergedNormal /= nMerged;
//						mergedNormal.Normalize();

//						//이제 위치를 조정하자
//						curOutPoint._pos = mergedPos;
//						curOutPoint._normal_Avg = mergedNormal;

//						//합쳐지는거 패스하고 다음으로 이동
//						//패스하는 곳과 연결하자
//						lastNextPoint._prevPoint = curOutPoint;
//						curOutPoint._nextPoint = lastNextPoint;

//						curOutPoint = lastNextPoint;
//					}
//					else
//					{
//						//다음으로 이동
//						curOutPoint = curOutPoint._nextPoint;
//					}

//					nCnt++;

//					if (curOutPoint == startPoint || nCnt >= nPoints)
//					{
//						break;
//					}

//				}

//				OuterPoint srcPoint = null;
//				for (int i = 0; i < removablePoints.Count; i++)
//				{
//					curOutPoint = removablePoints[i];
//					if (_outerPoints_Preview.Contains(curOutPoint))
//					{
//						_outerPoints_Preview.Remove(curOutPoint);
//					}

//					if (outerPointCopy2Src.ContainsKey(curOutPoint))
//					{
//						//Copy => Src에 대해서
//						srcPoint = outerPointCopy2Src[curOutPoint];

//						//Dictionary에서도 삭제
//						outerPointCopy2Src.Remove(curOutPoint);
//						outerPointSrc2Copy.Remove(srcPoint);
//					}

//				}

//				//이제 Normal 방향으로 증가시키자
//				for (int i = 0; i < _outerPoints_Preview.Count; i++)
//				{
//					curOutPoint = _outerPoints_Preview[i];
//					curOutPoint._pos += curOutPoint._normal_Avg.normalized * curMargin;
//				}

//				//Normal을 다시 계산해야한다.
//				OuterPoint prevOutPoint = null;
//				Vector3 dirUp = new Vector3(0, 0, 1);
//				for (int i = 0; i < _outerPoints_Preview.Count; i++)
//				{
//					curOutPoint = _outerPoints_Preview[i];
//					prevOutPoint = curOutPoint._prevPoint;
//					nextOutPoint = curOutPoint._nextPoint;
//					Vector3 dir2Prev = new Vector3(prevOutPoint._pos.x - curOutPoint._pos.x, prevOutPoint._pos.y - curOutPoint._pos.y, 0);
//					Vector3 dir2Next = new Vector3(nextOutPoint._pos.x - curOutPoint._pos.x, nextOutPoint._pos.y - curOutPoint._pos.y, 0);

//					Vector3 normalPrev3 = Vector3.Cross(dirUp, dir2Prev);
//					Vector3 normalNext3 = Vector3.Cross(dir2Next, dirUp);
//					Vector2 normalPrev = new Vector2(normalPrev3.x, normalPrev3.y);
//					Vector2 normalNext = new Vector2(normalNext3.x, normalNext3.y);

//					//만약 Prev/Next 방향이 반대라면 전환
//					if(Vector2.Dot(normalPrev, curOutPoint._normal_Prev) < 0.0f)
//					{
//						normalPrev *= -1;
//					}
//					if(Vector2.Dot(normalNext, curOutPoint._normal_Next) < 0.0f)
//					{
//						normalNext *= -1;
//					}
//					Vector2 normalAvg = (normalPrev.normalized + normalNext.normalized).normalized;
//					if(Vector2.Dot(normalAvg, curOutPoint._normal_Avg) < 0)
//					{
//						normalAvg *= -1;
//					}
//					curOutPoint._normal_Avg = normalAvg;
//				}
//				//----------------------------------------------------
//			}
			
//		}

//		//---------------------------------------------------------------
//		private bool IsOuterPoint(int iX, int iY)
//		{
//			int iLeft = iX - 1;
//			int iRight = iX;
//			int iUp = iY - 1;
//			int iDown = iY;
//			//하나라도 범위 밖이면 Outer Point이다.
//			if(iLeft < 0 ||
//				iRight >= _scan_NumTileX ||
//				iUp < 0 ||
//				iDown >= _scan_NumTileY
//				)
//			{
//				return true;
//			}

//			if(_scanTiles[iLeft, iUp]._tileType == TileType.Empty ||
//				_scanTiles[iLeft, iDown]._tileType == TileType.Empty ||
//				_scanTiles[iRight, iUp]._tileType == TileType.Empty ||
//				_scanTiles[iRight, iDown]._tileType == TileType.Empty)
//			{
//				return true;
//			}
//			return false;
//		}

//		private bool IsOuterPointPair(int iX_src, int iY_src, int iX_dst, int iY_dst)
//		{
//			if(iX_dst < iX_src)
//			{
//				//Left
//				if(iX_dst < 0)
//				{
//					return false;
//				}
//				//위쪽과 아래쪽을 비교 : 한쪽"만" 밖으로 나갔거나 Empty이면 True
//				bool isUpperEmpty = false;
//				bool isLowerEmpty = false;

//				if (iY_src - 1 < 0)	{	isUpperEmpty = true;	}
//				else				{	isUpperEmpty = (_scanTiles[iX_dst, iY_src - 1]._tileType == TileType.Empty);	}

//				if(iY_src >= _scan_NumTileY)	{	isLowerEmpty = true;	}
//				else							{	isLowerEmpty = (_scanTiles[iX_dst, iY_src]._tileType == TileType.Empty);	}

//				return (isUpperEmpty != isLowerEmpty);
//			}
//			else if(iY_dst < iY_src)
//			{
//				//Top
//				if(iY_dst < 0)
//				{
//					return false;
//				}
//				//왼쪽과 오른쪽을 비교 : 한쪽"만" 밖으로 나갔거나 Empty이면 True
//				bool isLeftEmpty = false;
//				bool isRightEmpty = false;

//				if (iX_src - 1 < 0)	{	isLeftEmpty = true;	}
//				else				{	isLeftEmpty = (_scanTiles[iX_src - 1, iY_dst]._tileType == TileType.Empty);	}

//				if(iX_src >= _scan_NumTileX)	{	isRightEmpty = true;	}
//				else							{	isRightEmpty = (_scanTiles[iX_src, iY_dst]._tileType == TileType.Empty);	}

//				return (isLeftEmpty != isRightEmpty);

//			}
//			else if(iX_dst > iX_src)
//			{
//				//Right
//				if(iX_dst > _scan_NumTileX)
//				{
//					return false;
//				}
//				//위쪽과 아래쪽을 비교 : 한쪽"만" 밖으로 나갔거나 Empty이면 True
//				bool isUpperEmpty = false;
//				bool isLowerEmpty = false;

//				if (iY_src - 1 < 0)	{	isUpperEmpty = true;	}
//				else				{	isUpperEmpty = (_scanTiles[iX_src, iY_src - 1]._tileType == TileType.Empty);	}

//				if(iY_src >= _scan_NumTileY)	{	isLowerEmpty = true;	}
//				else							{	isLowerEmpty = (_scanTiles[iX_src, iY_src]._tileType == TileType.Empty);	}

//				return (isUpperEmpty != isLowerEmpty);
//			}
//			else
//			{
//				//Down
//				if(iY_dst > _scan_NumTileY)
//				{
//					return false;
//				}

//				//왼쪽과 오른쪽을 비교 : 한쪽"만" 밖으로 나갔거나 Empty이면 True
//				bool isLeftEmpty = false;
//				bool isRightEmpty = false;

//				if (iX_src - 1 < 0)	{	isLeftEmpty = true;	}
//				else				{	isLeftEmpty = (_scanTiles[iX_src - 1, iY_src]._tileType == TileType.Empty);	}

//				if(iX_src >= _scan_NumTileX)	{	isRightEmpty = true;	}
//				else							{	isRightEmpty = (_scanTiles[iX_src, iY_src]._tileType == TileType.Empty);	}

//				return (isLeftEmpty != isRightEmpty);
//			}
//		}

//		private void SetTileInformationToPoint(OuterPoint point)
//		{
//			bool isEmptyTile_LU = false;
//			bool isEmptyTile_RU = false;
//			bool isEmptyTile_LD = false;
//			bool isEmptyTile_RD = false;

			
//			if(point._iX <= 0)
//			{
//				isEmptyTile_LU = true;
//				isEmptyTile_LD = true;

//				if (point._iY <= 0)
//				{
//					isEmptyTile_RU = true;
//					isEmptyTile_RD = (_scanTiles[point._iX, point._iY]._tileType == TileType.Empty);
//				}
//				else if (point._iY >= _scan_NumTileY)
//				{
//					isEmptyTile_RU = (_scanTiles[point._iX, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_RD = true;
//				}
//				else
//				{
//					isEmptyTile_RU = (_scanTiles[point._iX, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_RD = (_scanTiles[point._iX, point._iY]._tileType == TileType.Empty);
//				}
//			}
//			else if(point._iX >= _scan_NumTileX)
//			{
//				isEmptyTile_RU = true;
//				isEmptyTile_RD = true;

//				if (point._iY <= 0)
//				{
//					isEmptyTile_LU = true;
//					isEmptyTile_LD = (_scanTiles[point._iX - 1, point._iY]._tileType == TileType.Empty);
//				}
//				else if (point._iY >= _scan_NumTileY)
//				{
//					isEmptyTile_LU = (_scanTiles[point._iX - 1, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_LD = true;
//				}
//				else
//				{
//					isEmptyTile_LU = (_scanTiles[point._iX - 1, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_LD = (_scanTiles[point._iX - 1, point._iY]._tileType == TileType.Empty);
//				}
//			}
//			else
//			{
//				if (point._iY <= 0)
//				{
//					isEmptyTile_LU = true;
//					isEmptyTile_RU = true;
//					isEmptyTile_LD = (_scanTiles[point._iX - 1, point._iY]._tileType == TileType.Empty);
//					isEmptyTile_RD = (_scanTiles[point._iX, point._iY]._tileType == TileType.Empty);
//				}
//				else if (point._iY >= _scan_NumTileY)
//				{
//					isEmptyTile_LU = (_scanTiles[point._iX - 1, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_RU = (_scanTiles[point._iX, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_LD = true;
//					isEmptyTile_RD = true;
//				}
//				else
//				{
//					isEmptyTile_LU = (_scanTiles[point._iX - 1, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_RU = (_scanTiles[point._iX, point._iY - 1]._tileType == TileType.Empty);
//					isEmptyTile_LD = (_scanTiles[point._iX - 1, point._iY]._tileType == TileType.Empty);
//					isEmptyTile_RD = (_scanTiles[point._iX, point._iY]._tileType == TileType.Empty);
//				}
//			}

//			point.SetTileInformation(isEmptyTile_LU, isEmptyTile_RU, isEmptyTile_LD, isEmptyTile_RD);
//		}

//		private int GetPointDistanceMHT(OuterPoint pointA, OuterPoint pointB)
//		{
//			return Mathf.Abs(pointA._iX - pointB._iX) + Mathf.Abs(pointA._iY - pointB._iY);
//		}

//		private void CalculatePointSlopeAndNormal(OuterPoint point)
//		{
//			OuterPoint nextPoint = point._nextPoint;
//			OuterPoint prevPoint = point._prevPoint;

//			//기울기를 계산하자
//			//Prev -> Cur -> Next (Cur -> Prev가 아니라 Prev -> Cur이다)

//			point._slopeX_Next = nextPoint._iX - point._iX;
//			point._slopeY_Next = nextPoint._iY - point._iY;
//			point._slopeX_Prev = point._iX - prevPoint._iX;
//			point._slopeY_Prev = point._iY - prevPoint._iY;

//			if(point._slopeX_Next == 0)
//			{
//				point._slopeY_Next = (point._slopeY_Next > 0 ? 1 : -1);
//			}
//			else if(point._slopeY_Next == 0)
//			{
//				point._slopeX_Next = (point._slopeX_Next > 0 ? 1 : -1);
//			}
//			else if(Mathf.Abs(point._slopeY_Next) % Mathf.Abs(point._slopeX_Next) == 0)
//			{
//				point._slopeY_Next /= point._slopeX_Next;
//				point._slopeX_Next = 0;
//			}

//			if(point._slopeX_Prev == 0)
//			{
//				point._slopeY_Prev = (point._slopeY_Prev > 0 ? 1 : -1);
//			}
//			else if(point._slopeY_Prev == 0)
//			{
//				point._slopeX_Prev = (point._slopeX_Prev > 0 ? 1 : -1);
//			}
//			else if(Mathf.Abs(point._slopeY_Prev) % Mathf.Abs(point._slopeX_Prev) == 0)
//			{
//				point._slopeY_Prev /= point._slopeX_Prev;
//				point._slopeX_Prev = 0;
//			}

//			point._slopeVec2_Prev = (point._pos - prevPoint._pos).normalized;
//			point._slopeVec2_Next = (nextPoint._pos - point._pos).normalized;

//			point._normal_Prev = new Vector2(point._slopeVec2_Prev.y, -point._slopeVec2_Prev.x);
//			point._normal_Next = new Vector2(point._slopeVec2_Next.y, -point._slopeVec2_Next.x);

//			//노멀 벡터를 점검하자
//			//벡터가 있는 방향은 Empty여야 한다.
//			if(IsInversedVector(point._normal_Prev, point._isEmptyTile_LU, point._isEmptyTile_RU, point._isEmptyTile_LD, point._isEmptyTile_RD))
//			{
//				point._normal_Prev *= -1.0f;
//				point._isInversedNormal_Prev = true;
//			}
//			else
//			{
//				point._isInversedNormal_Prev = false;
//			}
			
//			if(IsInversedVector(point._normal_Next, point._isEmptyTile_LU, point._isEmptyTile_RU, point._isEmptyTile_LD, point._isEmptyTile_RD))
//			{
//				point._normal_Next *= -1.0f;
//				point._isInversedNormal_Next = true;
//			}
//			else
//			{
//				point._isInversedNormal_Next = false;
//			}
//			//point._isInversedNormal_Next = isVectorInversed;

//			point._normal_Avg = (point._normal_Prev + point._normal_Next).normalized;
			
//			point._slopeAngle_Prev = Mathf.Atan2(point._normal_Prev.y, point._normal_Prev.x) * Mathf.Rad2Deg;
//			point._slopeAngle_Next = Mathf.Atan2(point._normal_Next.y, point._normal_Next.x) * Mathf.Rad2Deg;
//			point._slopeAngle_Avg = Mathf.Atan2(point._normal_Avg.y, point._normal_Avg.x) * Mathf.Rad2Deg;
//		}


//		/// <summary>
//		/// CalculatePointSlopeAndNormal 함수 이후에 다시 계산하는 함수.
//		/// 타일값은 따지지 않으며, "이전에 계산했던 값"을 기준으로 약간 바꾸는 정도이다.
//		/// </summary>
//		/// <param name="point"></param>
//		private void ReCalculatePointSlopeAndNormal(OuterPoint point)
//		{
//			OuterPoint nextPoint = point._nextPoint;
//			OuterPoint prevPoint = point._prevPoint;

//			//기울기를 계산하자
//			//Prev -> Cur -> Next (Cur -> Prev가 아니라 Prev -> Cur이다)

//			point._slopeX_Next = nextPoint._iX - point._iX;
//			point._slopeY_Next = nextPoint._iY - point._iY;
//			point._slopeX_Prev = point._iX - prevPoint._iX;
//			point._slopeY_Prev = point._iY - prevPoint._iY;

//			if(point._slopeX_Next == 0)
//			{
//				point._slopeY_Next = (point._slopeY_Next > 0 ? 1 : -1);
//			}
//			else if(point._slopeY_Next == 0)
//			{
//				point._slopeX_Next = (point._slopeX_Next > 0 ? 1 : -1);
//			}
//			else if(Mathf.Abs(point._slopeY_Next) % Mathf.Abs(point._slopeX_Next) == 0)
//			{
//				point._slopeY_Next /= point._slopeX_Next;
//				point._slopeX_Next = 0;
//			}

//			if(point._slopeX_Prev == 0)
//			{
//				point._slopeY_Prev = (point._slopeY_Prev > 0 ? 1 : -1);
//			}
//			else if(point._slopeY_Prev == 0)
//			{
//				point._slopeX_Prev = (point._slopeX_Prev > 0 ? 1 : -1);
//			}
//			else if(Mathf.Abs(point._slopeY_Prev) % Mathf.Abs(point._slopeX_Prev) == 0)
//			{
//				point._slopeY_Prev /= point._slopeX_Prev;
//				point._slopeX_Prev = 0;
//			}

//			point._slopeVec2_Prev = (point._pos - prevPoint._pos).normalized;
//			point._slopeVec2_Next = (nextPoint._pos - point._pos).normalized;

//			//Vector2 prevNormal_Prev = point._normal_Prev;
//			//Vector2 prevNormal_Next = point._normal_Next;

//			Vector2 prevNormal_Prev = point._normal_Prev + point._prevPoint._normal_Avg;
//			Vector2 prevNormal_Next = point._normal_Next + point._nextPoint._normal_Avg;
//			point._normal_Prev = new Vector2(point._slopeVec2_Prev.y, -point._slopeVec2_Prev.x);
//			point._normal_Next = new Vector2(point._slopeVec2_Next.y, -point._slopeVec2_Next.x);

//			//노멀 벡터를 점검하자
//			//이전 값과 비교해서 각도가 반대가 되면 반전해야한다.
//			//전/후의 버텍스의 노멀과 비교
//			if(Vector2.Dot(point._normal_Prev, prevNormal_Prev) < 0.0f)
//			{
//				point._normal_Prev *= -1.0f;
//				point._isInversedNormal_Prev = true;
//			}
//			else
//			{
//				point._isInversedNormal_Prev = false;
//			}

//			if(Vector2.Dot(point._normal_Next, prevNormal_Next) < 0.0f)
//			{
//				point._normal_Next *= -1.0f;
//				point._isInversedNormal_Next = true;
//			}
//			else
//			{
//				point._isInversedNormal_Next = false;
//			}
			

//			point._normal_Avg = (point._normal_Prev + point._normal_Next).normalized;
			
//			point._slopeAngle_Prev = Mathf.Atan2(point._normal_Prev.y, point._normal_Prev.x) * Mathf.Rad2Deg;
//			point._slopeAngle_Next = Mathf.Atan2(point._normal_Next.y, point._normal_Next.x) * Mathf.Rad2Deg;
//			point._slopeAngle_Avg = Mathf.Atan2(point._normal_Avg.y, point._normal_Avg.x) * Mathf.Rad2Deg;
//		}


//		/// <summary>
//		/// 해당 방향의 방향 벡터가 비어있는 칸이 아닐 경우 벡터가 반전되어야 한다.
//		/// </summary>
//		/// <param name="vec2"></param>
//		/// <param name="isEmpty_LU"></param>
//		/// <param name="isEmpty_RU"></param>
//		/// <param name="isEmpty_LD"></param>
//		/// <param name="isEmpty_RD"></param>
//		/// <returns></returns>
//		private bool IsInversedVector(Vector2 vec2, bool isEmpty_LU, bool isEmpty_RU, bool isEmpty_LD, bool isEmpty_RD)
//		{
//			if(vec2.x < 0.0f)
//			{
//				if(vec2.y < 0.0f)
//				{
//					//LU
//					return (!isEmpty_LU);
//				}
//				else if(vec2.y > 0.0f)
//				{
//					//LD
//					return (!isEmpty_LD);
//				}
//				else
//				{
//					//Left
//					//좌우 비어있는 칸 수를 비교한다.
//					//Left 방향의 비어있는 칸 수가 적으면 Invese
//					int nLeftEmpty = (isEmpty_LU ? 1 : 0) + (isEmpty_LD ? 1 : 0);
//					int nRightEmpty = (isEmpty_RU ? 1 : 0) + (isEmpty_RD ? 1 : 0);
//					return (nLeftEmpty < 2 && nLeftEmpty < nRightEmpty);
//				}
//			}
//			else if(vec2.x > 0.0f)
//			{
//				if (vec2.y < 0.0f)
//				{
//					//RU
//					return (!isEmpty_RU);
//				}
//				else if (vec2.y > 0.0f)
//				{
//					//RD
//					return (!isEmpty_RD);
//				}
//				else
//				{
//					//Right
//					//좌우 비어있는 칸 수를 비교한다.
//					//Right 방향의 비어있는 칸 수가 적으면 Invese
//					int nLeftEmpty = (isEmpty_LU ? 1 : 0) + (isEmpty_LD ? 1 : 0);
//					int nRightEmpty = (isEmpty_RU ? 1 : 0) + (isEmpty_RD ? 1 : 0);
//					return (nRightEmpty < 2 && nRightEmpty < nLeftEmpty);
//				}
//			}
//			else
//			{
//				//상하 비어있는 칸 수를 비교한다.
//				int nUpEmpty = (isEmpty_LU ? 1 : 0) + (isEmpty_RU ? 1 : 0);
//				int nDownEmpty = (isEmpty_LD ? 1 : 0) + (isEmpty_RD ? 1 : 0);
					

//				if (vec2.y < 0.0f)
//				{
//					//Up
//					//Up 방향의 비어있는 칸 수가 적으면 Invese
//					return (nUpEmpty < 2 && nUpEmpty < nDownEmpty);
//				}
//				else if (vec2.y > 0.0f)
//				{
//					//Down
//					//Down 방향의 비어있는 칸 수가 적으면 Invese
//					return (nDownEmpty < 2 && nDownEmpty < nUpEmpty);
//				}
//				else
//				{
//					//? 원점
//					return false;
//				}
//			}
//			//return false;
//		}


//		private float GetDistanceFromLineByNormal(OuterPoint linePointA, OuterPoint linePointB, OuterPoint targetPoint)
//		{
//			if(Vector2.Dot(targetPoint._pos - linePointA._pos, linePointA._normal_Avg) < 0.0f)
//			{
//				//노멀 방향으로 이동한 점이 아니다.
//				return -1.0f;
//			}

//			float pA = linePointA._pos.y - linePointB._pos.y;
//			float pB = linePointB._pos.x - linePointA._pos.x;
//			float pC = linePointA._pos.x * (linePointB._pos.y - linePointA._pos.y) - linePointA._pos.y * (linePointB._pos.x - linePointA._pos.x);
//			return Mathf.Abs(targetPoint._pos.x * pA + targetPoint._pos.y * pB + pC) / Mathf.Sqrt(pA * pA + pB * pB);
//		}


//		private bool IsPointOuterFromNextLine(OuterPoint linePointAUsingNext, OuterPoint targetPoint)
//		{
//			if(Vector2.Dot(targetPoint._pos - linePointAUsingNext._pos, linePointAUsingNext._normal_Avg) < 0.0f)
//			{
//				//노멀 방향으로 이동한 점이 아니다.
//				return false;
//			}
//			return true;
//		}

//		private float GetDistanceFromLineByNormal(OuterPoint linePointA, OuterPoint targetPoint)
//		{
//			if(Vector2.Dot(targetPoint._pos - linePointA._pos, linePointA._normal_Avg) < 0.0f)
//			{
//				//노멀 방향으로 이동한 점이 아니다.
//				return -1.0f;
//			}
//			Vector2 linePointB_Pos = linePointA._pos + linePointA._slopeVec2_Next * 10.0f;
//			float pA = linePointA._pos.y - linePointB_Pos.y;
//			float pB = linePointB_Pos.x - linePointA._pos.x;
//			float pC = linePointA._pos.x * (linePointB_Pos.y - linePointA._pos.y) - linePointA._pos.y * (linePointB_Pos.x - linePointA._pos.x);
//			return Mathf.Abs(targetPoint._pos.x * pA + targetPoint._pos.y * pB + pC) / Mathf.Sqrt(pA * pA + pB * pB);
//		}

//		/// <summary>
//		/// 지정된 Line이 Empty 영역을 지나가는지 체크
//		/// Outline 타일에 걸쳐있으면 외곽선에 위치하면 된다.
//		/// 밖으로 나갔으면 무조건 Empty
//		/// 외곽선을 그을 수 있으면 True이다.
//		/// </summary>
//		/// <param name="posA"></param>
//		/// <param name="posB"></param>
//		/// <returns></returns>
//		private bool IsLineTroughEmptyTiles(Vector2 posA, Vector2 posB)
//		{
//			float length = Vector2.Distance(posA, posB);
//			int nUnits = (int)((length / (_scan_TileSize / 5.0f)) + 5);//타일 사이즈의 1/4 만큼의 간격 만큼 체크를 한다.
//			if(nUnits < 20)
//			{
//				nUnits = 20;
//			}
//			//nUnits += 1;//<시작점, 끝점 모두 체크해야하니 + 1
//			Vector2 curPos = Vector2.zero;
//			int iX = 0;
//			int iY = 0;
//			float lerp = 0.0f;
//			//ScanTile scanTile = null;
//			int iX_Prev = 0;
//			int iY_Prev = 0;
//			for (int i = 0; i <= nUnits; i++)
//			{
//				lerp = (float)i / (float)nUnits;
//				curPos = posA * (1.0f - lerp) + posB * lerp;
//				iX = (int)((curPos.x - _scan_PosMinX) / (float)_scan_TileSize);
//				iY = (int)((curPos.y - _scan_PosMinY) / (float)_scan_TileSize);

//				if(i != 0)
//				{
//					if(Mathf.Abs(iX - iX_Prev) + Mathf.Abs(iY - iY_Prev) > 1)
//					{
//						//두칸 이상 이동해버렸다;
//						//매우 적은 간격으로 다시 테스트
//						float prevLerp = (float)(i - 1) / (float)nUnits;
//						Vector2 prevPos = posA * (1.0f - prevLerp) + posB * prevLerp;
//						for (int iSub = 0; iSub <= 10; iSub++)
//						{
//							float subLerp = (float)iSub / 10.0f;
//							Vector2 subPos = prevPos * (1.0f - subLerp) + curPos * subLerp;
//							int subiX = (int)((subPos.x - _scan_PosMinX) / (float)_scan_TileSize);
//							int subiY = (int)((subPos.y - _scan_PosMinY) / (float)_scan_TileSize);

//							if (!IsEmptyPos(subPos, subiX, subiY))
//							{
//								//유효하지 않은 타일을 만나버렸다.
//								return false;
//							}
//						}
//					}
//				}

//				if(!IsEmptyPos(curPos, iX, iY))
//				{
//					//유효하지 않은 타일을 만나버렸다.
//					return false;
//				}

//				iX_Prev = iX;
//				iY_Prev = iY;
//			}
//			//하나도 Filled Tile에 걸리지 않았다.
//			return true;
//		}

//		private bool IsEmptyPos(Vector2 pos, int iX, int iY)
//		{
//			if (iX < 0 || iX >= _scan_NumTileX
//					|| iY < 0 || iY >= _scan_NumTileY)
//			{
//				return true;
//			}

//			ScanTile scanTile = _scanTiles[iX, iY];
//			if (scanTile._tileType == TileType.Empty)
//			{
//				return true;
//			}
//			if (scanTile._tileType == TileType.Filled)
//			{
//				return false;
//			}

//			//Outline인 경우
//			//이 점이 외곽선이어야 한다.
//			float bias = 0.0001f;
//			float left = (iX * _scan_TileSize) + _scan_PosMinX;
//			float right = ((iX + 1) * _scan_TileSize) + _scan_PosMinX;
//			float up = (iY * _scan_TileSize) + _scan_PosMinY;
//			float down = ((iY + 1) * _scan_TileSize) + _scan_PosMinY;
//			if (pos.x < left + bias || pos.x > right - bias
//				|| pos.y < up + bias || pos.y > down - bias)
//			{
//				//외곽선에 위치한다.
//				//return true;
//			}
//			//Outline 타일이지만, 경계가 아니어서 충돌될 수 있다.
//			return false;
//		}

//		private void ResetIndexByPos(OuterPoint point)
//		{
//			point._iX = (int)((point._pos.x - _scan_PosMinX) / (float)_scan_TileSize);
//			point._iY = (int)((point._pos.y - _scan_PosMinY) / (float)_scan_TileSize);

//			SetTileInformationToPoint(point);
//		}

//		public Vector2 ConvertWorld2Tex(Vector2 worldPos)
//		{
//			if(_targetMesh == null || _targetTextureData == null)
//			{
//				return worldPos;
//			}
//			Vector2 imageHalfSize = new Vector2(_targetTextureData._width * 0.5f, _targetTextureData._height * 0.5f);
//			//Vector2 meshOffset = _targetMesh._offsetPos;
//			//return worldPos + (meshOffset - imageHalfSize);
//			//return worldPos + (_targetMesh._offsetPos);
//			return worldPos - imageHalfSize;
//		}

//		//---------------------------------------------------------------

//		// Get / Set
//		//----------------------------------------------------
//		public apMesh TargetMesh
//		{
//			get { return _targetMesh; }
//		}

//		public STEP Step
//		{
//			get { return _step; }
//		}

//		public bool IsScanned
//		{
//			get { return _step == STEP.Scanned || _step == STEP.Previewed || _step == STEP.Completed; }
//		}

//		public bool IsPreviewed
//		{
//			get { return _step == STEP.Previewed || _step == STEP.Completed; }
//		}

//		public bool IsCompleted
//		{
//			get { return _step == STEP.Completed; }
//		}

//		public bool IsValidTexture()
//		{
//			return _textureImporter != null;
//		}

//		public bool IsTextureReadWriteEnabled()
//		{
//			if (_textureImporter == null)
//			{
//				return false;
//			}
//			return _textureImporter.isReadable;
//		}

//		public bool IsValidAtlasArea()
//		{
//			if(_targetMesh == null)
//			{
//				return false;
//			}
//			if(!_targetMesh._isPSDParsed)
//			{
//				return false;
//			}
//			int width = (int)(Mathf.Abs(_targetMesh._atlasFromPSD_RB.x - _targetMesh._atlasFromPSD_LT.x));
//			int height = (int)(Mathf.Abs(_targetMesh._atlasFromPSD_RB.y - _targetMesh._atlasFromPSD_LT.y));
//			if(width <= 20 || height <= 20)
//			{
//				return false;
//			}
//			return true;
//		}

//		public bool IsScanable()
//		{
//			if (_targetMesh == null || _targetImage == null || _targetTextureData == null)
//			{
//				return false;
//			}
//			return _targetMesh._isPSDParsed && IsValidAtlasArea() && IsTextureReadWriteEnabled();
//		}

//		public Vector2 GetTilePos(int iX, int iY)
//		{
//			return new Vector2(	(iX * _scan_TileSize) + _scan_PosMinX,
//								(iY * _scan_TileSize) + _scan_PosMinY);
//		}

//		public Vector2 GetTilePos_Min(int iX, int iY)
//		{
//			return new Vector2(	(iX * _scan_TileSize) + _scan_PosMinX,
//								(iY * _scan_TileSize) + _scan_PosMinY);
//		}
//		public Vector2 GetTilePos_Max(int iX, int iY)
//		{
//			return new Vector2(	((iX + 1) * _scan_TileSize) + _scan_PosMinX,
//								((iY + 1) * _scan_TileSize) + _scan_PosMinY);
//		}
//	}
//}