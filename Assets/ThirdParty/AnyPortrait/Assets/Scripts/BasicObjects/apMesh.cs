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

	public class apMesh : MonoBehaviour
	{
		// Members
		//----------------------------------------------
		public int _uniqueID = -1;//고유의 아이디를 만들자

		public string _name = "";
		// Components
		//private MeshFilter _meshFilter;
		//private MeshRenderer _meshRenderer;
		//private Material _material;
		//private Texture2D _texture;

		//// Mesh Variables
		//private Mesh _mesh;

		//수정. 
		/// <summary>
		/// 이 TextureData는 TextureData ID만 사용해야 한다.
		/// 외부에서는 이 값을 사용하지 말고, _textureData_Linked를 참조하자
		/// </summary>
		[SerializeField]
		private apTextureData _textureData = null;

		

		[NonSerialized]
		public apTextureData _textureData_Linked = null;



		// Vertex Data
		[SerializeField]
		public List<apVertex> _vertexData = new List<apVertex>();

		[SerializeField]
		public List<int> _indexBuffer = new List<int>();

		//private Vector3[] _vertices = null;
		//private Vector2[] _uvs = null;
		//private int[] _triangles = null;

		[NonSerialized]
		public bool _isEdgeWorking = false;//Edge 작업 후에 Tri를 일괄적으로 만든다.

		[NonSerialized]
		public bool _isEdgeWorkAnyChanged = false;

		[SerializeField]
		public List<apMeshEdge> _edges = new List<apMeshEdge>();//<<변경. 주로 이걸 위주로 작업하고, IndexBuffer는 렌더링 용으로만 사용

		[SerializeField]
		public List<apMeshPolygon> _polygons = new List<apMeshPolygon>();


		[NonSerialized]
		public apPortrait _portrait = null;

		//Matrix : Local : 
		//로컬 Offset에 좌표가 0,0이 아닌 이유
		//기본적으론 Texture 좌표계가 가장 밑이기 때문
		//Vertex의 Pos/UV : Texture 좌표계 <- Morph는 여기서 걸린다 / Rigging도..
		//Mesh의 좌표계 : Offset을 적용한 Mesh Local 좌표계 < 이게 메인
		//그 외에는 Parent ..... : 중첩된 좌표계 후 최종적으로 MeshGroup 좌표계
		//Root MeshGroup : MeshGroup 좌표계 = 기본적인 World 좌표계

		public Vector2 _offsetPos = Vector2.zero;//좌표가 아니라 Offset이다. 더하는게 아니라 빼야한다.


		//추가
		//PSD로 로드된 것이라면 Atlas로 만들어진 것이다.
		//Atlas 영역 정보를 받아오자
		[SerializeField, HideInInspector]
		public bool _isPSDParsed = false;//<<기본은 false

		[SerializeField, HideInInspector]
		public Vector2 _atlasFromPSD_LT = Vector2.zero;

		[SerializeField, HideInInspector]
		public Vector2 _atlasFromPSD_RB = Vector2.zero;

		//추가 : 9.7 Mirror 작업을 위한 저장용 변수. 백업은 안된다.
		[SerializeField, HideInInspector, NonBackupField]
		public Vector2 _mirrorAxis = Vector2.zero;

		[SerializeField, HideInInspector, NonBackupField]
		public bool _isMirrorX = true;

		//추가 20.7.6 : PSD로 부터 만들어진 경우 버텍스를 리셋할 건지 바로 물어보자.
		//옵션으로 켜고 끌 수 있으며, 다이얼로그 결과에 상관없이 한번 이 변수가 조회되면 무조건 false가 된다.
		//기본적으로는 false이지만, PSD로 생성된 메시만 true이다.
		[SerializeField, HideInInspector, NonBackupField]
		public bool _isNeedToAskRemoveVertByPSDImport = false;

		[NonSerialized, HideInInspector, NonBackupField]
		public apMatrix3x3 _matrix_VertToLocal = apMatrix3x3.identity;//변경 21.5.15 : Private에서 Public(NonSerialized)로 변경

		[NonSerialized, HideInInspector, NonBackupField]
		public apMatrix3x3 _matrix_VertToLocal_Inv = apMatrix3x3.identity;//변경 21.5.15 : Private에서 Public(NonSerialized)로 변경
		
		public void MakeOffsetPosMatrix()
		{
			_matrix_VertToLocal = apMatrix3x3.TRS(new Vector2(-_offsetPos.x, -_offsetPos.y), 0, Vector2.one);
			_matrix_VertToLocal_Inv = _matrix_VertToLocal.inverse;
		}

		public apMatrix3x3 Matrix_VertToLocal
		{
			get
			{
				//return apMatrix3x3.TRS(new Vector3(-_offsetPos.x, -_offsetPos.y, 0.0f), Quaternion.identity, Vector3.one);
				return _matrix_VertToLocal;
			}
		}

		//추가 21.5.16 : 역행렬 연산량을 줄이기 위해
		public apMatrix3x3 Matrix_VertToLocal_Inverse
		{
			get
			{
				return _matrix_VertToLocal_Inv;
			}
		}
		

		//추가 22.2.6 (v1.4.0) : Pin 그룹
		[SerializeField]
		public apMeshPinGroup _pinGroup;
		

		//private List<int> _vertexIDs = new List<int>();

		// Init
		//----------------------------------------------
		void Start()
		{
			//업데이트하지 않습니다.
			this.enabled = false;
		}
		
		// Update
		//----------------------------------------------
		
		// GUI / Gizmos
		//----------------------------------------------
		
		// Functions
		//----------------------------------------------
		// 0. Reset Vertex By Image Outline
		public void RefreshVertexAndPinIDs()
		{
			for (int iVert = 0; iVert < _vertexData.Count; iVert++)
			{
				//_portrait.RegistUniqueID_Vertex(_vertexData[iVert]._uniqueID);
				_portrait.RegistUniqueID(apIDManager.TARGET.Vertex, _vertexData[iVert]._uniqueID);
			}

			//추가 22.2.26 : Pin의 UniqueID도 이때 같이 넣자
			if(_pinGroup != null)
			{
				int nPins = _pinGroup._pins_All != null ? _pinGroup._pins_All.Count : 0;
				for (int i = 0; i < nPins; i++)
				{
					_portrait.RegistUniqueID(apIDManager.TARGET.MeshPin, _pinGroup._pins_All[i]._uniqueID);
				}
			}
		}


		public void ReadyToEdit(apPortrait portrait)
		{
			_portrait = portrait;

			//Mesh ID를 등록해준다.
			//_portrait.RegistUniqueID_Mesh(_uniqueID);
			_portrait.RegistUniqueID(apIDManager.TARGET.Mesh, _uniqueID);

			//Vertex ID를 등록해준다.
			RefreshVertexAndPinIDs();


			LinkEdgeAndVertex();
			

#if UNITY_EDITOR
			RefreshPolygonsToIndexBuffer();

			MakeOffsetPosMatrix();
#endif
		}


		public apVertex GetVertex(int index)
		{
			return _vertexData.Find(delegate (apVertex a)
				{
					return a._index == index;
				});
		}

		public apVertex GetVertexByUniqueID(int uniqueID)
		{
			return _vertexData.Find(delegate (apVertex a)
				{
					return a._uniqueID == uniqueID;
				});
		}


		public apMeshEdge GetEdgeByUniqueID(int vertUniqueID1, int vertUniqueID2)
		{
			return _edges.Find(delegate (apMeshEdge a)
			{
				return a.IsSameEdge(vertUniqueID1, vertUniqueID2);
			});
		}

		public void LinkEdgeAndVertex()
		{
			for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
			{
				int vID1 = _edges[iEdge]._vertID_1;
				int vID2 = _edges[iEdge]._vertID_2;

				_edges[iEdge]._vert1 = GetVertexByUniqueID(vID1);
				_edges[iEdge]._vert2 = GetVertexByUniqueID(vID2);
			}
			_edges.RemoveAll(delegate (apMeshEdge a)
			{
				return a._vert1 == null || a._vert2 == null;
			});

			for (int i = 0; i < _polygons.Count; i++)
			{
				_polygons[i].Link(this);
			}


			//추가 22.2.26 (1.4.0) : Pin도 링크하자
			if(_pinGroup == null)
			{
				_pinGroup = new apMeshPinGroup();
				_pinGroup.Clear();//완전히 초기화한다.
			}
			_pinGroup.Link(_portrait, this);

		}



		/// <summary>
		/// 요청한 Vertex와 인접한 Vertex들을 리턴한다.
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		public List<apVertex> GetLinkedVertex(apVertex vertex, bool is2Level)
		{
			if (!_vertexData.Contains(vertex))
			{
				return null;
			}

			List<apVertex> result = new List<apVertex>();

			apMeshEdge edge = null;
			for (int i = 0; i < _edges.Count; i++)
			{
				edge = _edges[i];
				if (edge._vert1 == vertex)
				{
					if (!result.Contains(edge._vert2))
					{
						result.Add(edge._vert2);
					}
				}
				else if (edge._vert2 == vertex)
				{
					if (!result.Contains(edge._vert1))
					{
						result.Add(edge._vert1);
					}
				}
			}

			if (is2Level && result.Count > 0)
			{
				List<apVertex> result2Level = new List<apVertex>();
				//현재 선택되었던 Vertex와 연결된 Vertex를 추가로 더 연결한다.
				for (int i = 0; i < _edges.Count; i++)
				{
					edge = _edges[i];
					if (result.Contains(edge._vert1) && !result.Contains(edge._vert2) && edge._vert2 != vertex)
					{
						if (!result2Level.Contains(edge._vert2))
						{
							result2Level.Add(edge._vert2);
						}
					}
					else if (result.Contains(edge._vert2) && !result.Contains(edge._vert1) && edge._vert1 != vertex)
					{
						if (!result2Level.Contains(edge._vert1))
						{
							result2Level.Add(edge._vert1);
						}
					}
				}
				for (int i = 0; i < result2Level.Count; i++)
				{
					result.Add(result2Level[i]);
				}
			}

			return result;
		}


		/// <summary>
		/// GetLinkedVertex의 Level값이 추가된 버전.
		/// 최대 Level 만큼 검색하여 연결된 Vertex 리스트를 리턴한다.
		/// Level이 많으면 속도가 좀 느리다.
		/// </summary>
		/// <param name="srcVertex"></param>
		/// <param name="maxLevel">검색할 연결 Level. 1 이상이어야 한다.</param>
		/// <returns></returns>
		public List<LinkedVertexResult> GetLinkedVertex(apVertex srcVertex, int maxLevel)
		{
			List<LinkedVertexResult> result = new List<LinkedVertexResult>();

			if (!_vertexData.Contains(srcVertex))
			{
				return result;
			}
			if (maxLevel <= 0)
			{
				return result;
			}

			Dictionary<int, List<apVertex>> linkedVertSet = new Dictionary<int, List<apVertex>>();

			apVertex vert = null;
			apMeshEdge edge = null;
			List<apVertex> notLinkedVerts = new List<apVertex>();//아직 연결 안된 버텍스
			for (int i = 0; i < _vertexData.Count; i++)
			{
				vert = _vertexData[i];
				if (vert == srcVertex)
				{
					continue;
				}
				notLinkedVerts.Add(vert);
			}

			//Level 0 : 자기 자신
			linkedVertSet.Add(0, new List<apVertex>());
			linkedVertSet[0].Add(srcVertex);

			List<apVertex> curSrcVerts = null;
			for (int iLevel = 1; iLevel <= maxLevel; iLevel++)
			{
				//연결 안된 Vertex 중에서 체크를 하자
				//Src Verts를 루프를 돌면서 "직접 연결된 Edge가 있는가"

				linkedVertSet.Add(iLevel, new List<apVertex>());//<<결과가 담길 리스트Set을 추가한다.
				curSrcVerts = linkedVertSet[iLevel - 1];//이전 레벨에서 체크를 시작한다.

				for (int iSrcVert = 0; iSrcVert < curSrcVerts.Count; iSrcVert++)
				{
					vert = curSrcVerts[iSrcVert];
					//연결 시킬만한 버텍스를 "연결안되고 남은 버텍스"에서 찾기 시작한다.
					//검색은 Edge에서 하자
					for (int i = 0; i < _edges.Count; i++)
					{
						edge = _edges[i];
						if (edge._vert1 == srcVertex)
						{
							if (notLinkedVerts.Contains(edge._vert2))
							{
								//아직 연결 안된 Vertex이다! -> 추가
								linkedVertSet[iLevel].Add(edge._vert2);
								notLinkedVerts.Remove(edge._vert2);
							}
						}
						else if (edge._vert2 == srcVertex)
						{
							if (notLinkedVerts.Contains(edge._vert1))
							{
								//아직 연결 안된 Vertex이다! -> 추가
								linkedVertSet[iLevel].Add(edge._vert1);
								notLinkedVerts.Remove(edge._vert1);
							}
						}
					}

				}
			}

			//완성된 리스트로 결과를 만들자

			for (int iLevel = 1; iLevel <= maxLevel; iLevel++)
			{
				//0레벨을 제외하고 리스트를 순회
				List<apVertex> lVerts = linkedVertSet[iLevel];

				for (int iVert = 0; iVert < lVerts.Count; iVert++)
				{
					result.Add(new LinkedVertexResult(lVerts[iVert], iLevel));
				}
			}

			return result;
		}

		public class LinkedVertexResult
		{
			public apVertex _vertex = null;
			public int _level = -1;
			public LinkedVertexResult(apVertex vertex, int level)
			{
				_vertex = vertex;
				_level = level;
			}
		}


		public bool IsOutlineVertex(apVertex vertex)
		{
			apMeshEdge edge = null;
			for (int i = 0; i < _edges.Count; i++)
			{
				edge = _edges[i];
				if (edge._vert1 == vertex || edge._vert2 == vertex)
				{
					if (edge._isOutline)
					{
						return true;
					}
				}
			}
			return false;
		}

		private apMeshEdge GetEdge(int vertIndex1, int vertIndex2)
		{
			return _edges.Find(delegate (apMeshEdge a)
			{
				return (a._vert1._index == vertIndex1 && a._vert2._index == vertIndex2)
				|| (a._vert2._index == vertIndex1 && a._vert1._index == vertIndex2);
			});
		}

		private apMeshEdge GetEdge(apVertex vert1, apVertex vert2)
		{
			return _edges.Find(delegate (apMeshEdge a)
			{
				return (a._vert1 == vert1 && a._vert2 == vert2)
				|| (a._vert1 == vert2 && a._vert2 == vert1);
			});
		}


#if UNITY_EDITOR
		public void ResetVerticesByImageOutline()
		{
			//if (_textureData == null)
			if (LinkedTextureData == null)
			{
				return;
			}
			//if (_textureData._image == null)
			if (LinkedTextureData._image == null)
			{
				return;
			}

			_vertexData.Clear();
			_indexBuffer.Clear();
			float width_Half = LinkedTextureData._width / 2;
			float height_Half = LinkedTextureData._height / 2;

			//	1		2
			//
			//	0		3


			AddVertex(new apVertex(0, _portrait.MakeUniqueID(apIDManager.TARGET.Vertex), new Vector3(-width_Half, -height_Half, 0), new Vector2(0, 0)));
			AddVertex(new apVertex(1, _portrait.MakeUniqueID(apIDManager.TARGET.Vertex), new Vector3(-width_Half, +height_Half, 0), new Vector2(0, 1)));
			AddVertex(new apVertex(2, _portrait.MakeUniqueID(apIDManager.TARGET.Vertex), new Vector3(+width_Half, +height_Half, 0), new Vector2(1, 1)));
			AddVertex(new apVertex(3, _portrait.MakeUniqueID(apIDManager.TARGET.Vertex), new Vector3(+width_Half, -height_Half, 0), new Vector2(1, 0)));

			_edges.Add(new apMeshEdge(_vertexData[0], _vertexData[1]));
			_edges.Add(new apMeshEdge(_vertexData[1], _vertexData[2]));
			_edges.Add(new apMeshEdge(_vertexData[2], _vertexData[3]));
			_edges.Add(new apMeshEdge(_vertexData[3], _vertexData[0]));

			//AddMeshIndex(0, 1, 3);
			//AddMeshIndex(1, 2, 3);

			SortVertexData();

			MakeEdgesToPolygonAndIndexBuffer();
			RefreshPolygonsToIndexBuffer();
			MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자
		}


		public void ResetVerticesByRect(Vector2 offsetPos, float posLeft, float posTop, float posRight, float posBottom)
		{
			if (LinkedTextureData == null)
			{
				return;
			}
			if (LinkedTextureData._image == null)
			{
				return;
			}

			_vertexData.Clear();
			_indexBuffer.Clear();
			//float width_Half = _textureData._width / 2;
			//float height_Half = _textureData._height / 2;

			//	1		2
			//
			//	0		3
			AddVertexAutoUV(new Vector2(posLeft, posBottom));
			AddVertexAutoUV(new Vector2(posLeft, posTop));
			AddVertexAutoUV(new Vector2(posRight, posTop));
			AddVertexAutoUV(new Vector2(posRight, posBottom));

			_edges.Add(new apMeshEdge(_vertexData[0], _vertexData[1]));
			_edges.Add(new apMeshEdge(_vertexData[1], _vertexData[2]));
			_edges.Add(new apMeshEdge(_vertexData[2], _vertexData[3]));
			_edges.Add(new apMeshEdge(_vertexData[3], _vertexData[0]));

			_offsetPos = offsetPos;

			SortVertexData();


			MakeEdgesToPolygonAndIndexBuffer();
			RefreshPolygonsToIndexBuffer();

			MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자
		}

		public void MoveVertexToRemappedAtlas(Vector2 nextOffsetPos, 
			//float nextPosLeft, float nextPosTop, float nextPosRight, float nextPosBottom, 
			float remapPosOffsetDeltaX, float remapPosOffsetDeltaY, Vector2 nextAtlasPSD_LT, Vector2 nextAtlasPSD_RB,
			//float deltaScaleRatio
			float prevAtlasScaleRatio,
			float nextAtlasScaleRatio
			)
		{
			if (LinkedTextureData == null)
			{
				return;
			}
			if (LinkedTextureData._image == null)
			{
				return;
			}

			//_vertexData.Clear();
			//_indexBuffer.Clear();

			//Vertex를 "Texture 좌표계의 Offset 차이 + 조정값"만큼 이동하자
			float atlasResizeRatio = (nextAtlasScaleRatio / prevAtlasScaleRatio);
			Vector2 deltaRemapOffset = new Vector2(remapPosOffsetDeltaX, remapPosOffsetDeltaY);

			//deltaRemapOffset /= atlasResizeRatio;
			deltaRemapOffset *= nextAtlasScaleRatio;

			Vector2 deltaOffset = (nextOffsetPos - _offsetPos);
			//Vector2 vertDeltaMove = deltaOffset - deltaRemapOffset;//기존
			

			float width = LinkedTextureData._width;
			float height = LinkedTextureData._height;
			float width_half = width * 0.5f;
			float height_half = height * 0.5f;
			


			Vector2 vLB = new Vector2(-width_half, -height_half);

			apVertex vert = null;
			for (int i = 0; i < _vertexData.Count; i++)
			{
				vert = _vertexData[i];
				//vert._pos = (((vert._pos + vertDeltaMove) - nextOffsetPos) * atlasResizeRatio) + nextOffsetPos;//기존 식
				vert._pos = ((((vert._pos + deltaOffset) - nextOffsetPos) * atlasResizeRatio) + nextOffsetPos) - deltaRemapOffset;//변경된 식

				
				//바뀐 위치에 맞게 UV 수정
				vert._uv = new Vector2(
						(vert._pos.x - vLB.x) / width,
						(vert._pos.y - vLB.y) / height);
			}

			
			_offsetPos = nextOffsetPos;

			_isPSDParsed = true;
			//이전
			//_atlasFromPSD_LT = nextAtlasPSD_LT;
			//_atlasFromPSD_RB = nextAtlasPSD_RB;

			//변경 21.3.4 : 크기에서 T, B에 뭔가 문제가 있는듯 하다.
			//T가 MaxY이다.
			_atlasFromPSD_LT.x = Mathf.Min(nextAtlasPSD_LT.x, nextAtlasPSD_RB.x);
			_atlasFromPSD_LT.y = Mathf.Max(nextAtlasPSD_LT.y, nextAtlasPSD_RB.y);
			_atlasFromPSD_RB.x = Mathf.Max(nextAtlasPSD_LT.x, nextAtlasPSD_RB.x);
			_atlasFromPSD_RB.y = Mathf.Min(nextAtlasPSD_LT.y, nextAtlasPSD_RB.y);

			SortVertexData();


			MakeEdgesToPolygonAndIndexBuffer();
			RefreshPolygonsToIndexBuffer();

			MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자
		}






#endif


		public void SetVertices(List<apVertex> vertexData, List<int> indexBuffer)
		{
			_vertexData = vertexData;
			_indexBuffer = indexBuffer;

			SortVertexData();
		}


		public void AddVertex(apVertex vertex)
		{
			_vertexData.Add(vertex);

			SortVertexData();
		}

		public apVertex AddVertexAutoUV(Vector2 pos)
		{
			//int nextUniqueID = _portrait.MakeUniqueID_Vertex();
			
			int nextUniqueID = _portrait.MakeUniqueID(apIDManager.TARGET.Vertex);

			if (nextUniqueID < 0)
			{
				return null;
			}
			apVertex vert = new apVertex(_vertexData.Count, nextUniqueID, pos, Vector2.zero);
			//vert._pos = pos;
			//vert._index = _vertexData.Count;
			//vert._uv = Vector2.zero;

			if (LinkedTextureData != null)
			{
				float width = LinkedTextureData._width;
				float height = LinkedTextureData._height;
				if (width > 1 && height > 1)
				{
					float width_half = width * 0.5f;
					float height_half = height * 0.5f;

					Vector2 vLB = new Vector2(-width_half, -height_half);

					Vector2 uv = new Vector2(
						(pos.x - vLB.x) / width,
						(pos.y - vLB.y) / height);

					vert._uv = uv;
				}
			}

			_vertexData.Add(vert);

			return vert;
		}
		public void RefreshVertexAutoUV(apVertex vertex)
		{
			vertex._uv = GetAutoUV(vertex._pos);
		}

		public Vector2 GetAutoUV(Vector2 pos)
		{
			if (LinkedTextureData != null)
			{
				float width = LinkedTextureData._width;
				float height = LinkedTextureData._height;
				if (width > 1 && height > 1)
				{
					float width_half = width * 0.5f;
					float height_half = height * 0.5f;

					Vector2 vLB = new Vector2(-width_half, -height_half);

					Vector2 uv = new Vector2(
						(pos.x - vLB.x) / width,
						(pos.y - vLB.y) / height);

					return uv;
				}
			}
			return Vector2.zero;
		}

		/// <summary>
		/// Vertex를 삭제한다.
		/// isTryKeepEdge를 True로 하면 Vertex를 공유하고 있는 Edge가 2개인 경우 Edge를 유지한다.
		/// </summary>
		/// <param name="vertex"></param>
		/// <param name="isTryKeepEdge"></param>
		public void RemoveVertex(apVertex vertex, bool isTryKeepEdge)
		{
			// 일단, 연결된 Edge는 모두 삭제한다.
			//나머지 Tri 중에서 
			// 이 Edge의 index를 기점으로,
			// 이 Index를 포함한 Tri는 삭제한다.

			//추가
			//연결된 Edge 삭제
			//연결된 Polygon 삭제
			//Refresh

			int iVert = -1;
			for (int i = 0; i < _vertexData.Count; i++)
			{
				if (_vertexData[i] == vertex)
				{
					iVert = i;
					break;
				}
			}

			if (iVert < 0)
			{
				return;
			}

			apVertex targetVert = _vertexData[iVert];

			int removedVertUniqueID = _vertexData[iVert]._uniqueID;
			//_portrait.PushUniqueID_Vertex(removedVertUniqueID);
			_portrait.PushUnusedID(apIDManager.TARGET.Vertex, removedVertUniqueID);


			#region [미사용 코드]
			////List<bool> _isValidBuffer = new List<bool>();
			//List<int> newIndexBuffer = new List<int>();
			//for (int i = 0; i < _indexBuffer.Count; i+=3)
			//{
			//	if(i + 2 >= _indexBuffer.Count)
			//	{
			//		// 3개를 만족하지 못하면 추가하지 않는다.
			//		break;
			//	}

			//	int index0 = _indexBuffer[i];
			//	int index1 = _indexBuffer[i + 1];
			//	int index2 = _indexBuffer[i + 2];

			//	if(index0 == iVert || index1 == iVert || index2 == iVert)
			//	{
			//		//하나라도 삭제할 Vertex를 포함한다.
			//	}
			//	else
			//	{
			//		//이 Tri는 유효하다
			//		//단, index 크기가 iVert보다 큰 건 1 감소시킨다.
			//		for (int iSub = 0; iSub < 3; iSub++)
			//		{
			//			int indexSub = _indexBuffer[i + iSub];
			//			if(indexSub > iVert)
			//			{
			//				newIndexBuffer.Add(indexSub - 1);
			//			}
			//			else
			//			{
			//				newIndexBuffer.Add(indexSub);
			//			}
			//		}
			//	}
			//}

			//_indexBuffer = newIndexBuffer; 
			#endregion




			_polygons.RemoveAll(delegate (apMeshPolygon a)
			{
				return a.IsVertexContain(vertex);
			});

			apVertex keepVert1 = null;
			apVertex keepVert2 = null;

			if (isTryKeepEdge)
			{
				List<apMeshEdge> linkedEdges = _edges.FindAll(delegate (apMeshEdge a)
				{
					return a._vert1 == targetVert || a._vert2 == targetVert;
				});

				if (linkedEdges.Count == 2)
				{
					//두개의 Edge인 경우
					if (linkedEdges[0]._vert1 == targetVert)
					{ keepVert1 = linkedEdges[0]._vert2; }
					else
					{ keepVert1 = linkedEdges[0]._vert1; }

					if (linkedEdges[1]._vert1 == targetVert)
					{ keepVert2 = linkedEdges[1]._vert2; }
					else
					{ keepVert2 = linkedEdges[1]._vert1; }
				}
			}
			_edges.RemoveAll(delegate (apMeshEdge a)
			{
				return (a._vert1 == vertex || a._vert2 == vertex);
			});

			if (isTryKeepEdge && keepVert1 != null && keepVert2 != null && keepVert1 != keepVert2)
			{
				apMeshEdge newKeepEdge = new apMeshEdge(keepVert1, keepVert2);
				_edges.Add(newKeepEdge);
			}

			_isEdgeWorkAnyChanged = true;

			_vertexData.RemoveAt(iVert);
#if UNITY_EDITOR
			RefreshPolygonsToIndexBuffer();
#endif
		}


		//public void AddMeshIndex(int index1, int index2, int index3)
		//{
		//	_indexBuffer.Add(index1);
		//	_indexBuffer.Add(index2);
		//	_indexBuffer.Add(index3);
		//}

		public void SortVertexData()
		{
			_vertexData.Sort(delegate (apVertex a, apVertex b)
			{
				return a._index - b._index;
			});

			for (int i = 0; i < _vertexData.Count; i++)
			{
				_vertexData[i]._index = i;
			}
		}


#if UNITY_EDITOR
		// Edge 작업
		//--------------------------------------------------------------------------------
		public void StartNewEdgeWork()
		{
			_isEdgeWorking = true;
			_isEdgeWorkAnyChanged = false;

			#region [미사용 코드] 현재의 Tri를 모두 Edge로 바꿔주자 -> 지금은 필요없다.
			////현재의 Tri를 모두 Edge로 바꿔주자
			//for (int i = 0; i < _indexBuffer.Count; i+=3)
			//{
			//	if(i + 2 > _indexBuffer.Count)
			//	{
			//		break;
			//	}

			//	if(_indexBuffer[i + 0] >= _vertexData.Count) { break; }
			//	if(_indexBuffer[i + 1] >= _vertexData.Count) { break; }
			//	if(_indexBuffer[i + 2] >= _vertexData.Count) { break; }

			//	apVertex vert0 = _vertexData[_indexBuffer[i + 0]];
			//	apVertex vert1 = _vertexData[_indexBuffer[i + 1]];
			//	apVertex vert2 = _vertexData[_indexBuffer[i + 2]];

			//	MakeNewEdge(vert0, vert1);
			//	MakeNewEdge(vert1, vert2);
			//	MakeNewEdge(vert2, vert0);
			//} 
			#endregion
		}

		/// <summary>
		/// Mesh에 Edge를 추가한다.
		/// isAddCrossPoint를 True로 할 경우, 관통된 다른 Edge에 추가로 Vertex를 생성한 뒤 Edge를 분할해가면서 생성한다.
		/// </summary>
		/// <param name="vert1"></param>
		/// <param name="vert2"></param>
		/// <param name="isAddCrossPoint"></param>
		/// <returns>생성된 Edge를 리턴한다. 여러개가 생성되었을 경우, 처음 한개만 리턴</returns>
		public apMeshEdge MakeNewEdge(apVertex vert1, apVertex vert2, bool isAddCrossPoint)
		{
			//여기서부터 작업
			if (vert1 == vert2)
			{
				return null;
			}

			bool isExist = _edges.Exists(delegate (apMeshEdge a)
							{
								return a.IsSameEdge(vert1, vert2);
							});

			if (!isExist)
			{
				if (isAddCrossPoint)
				{
					//만약 교차점 생성을 허용한다면..
					//해당 Edge를 Split하고, 그 지점을 Vertex로 저장한다.
					//시작점 -> 끝점을 기준으로 거리 순으로 정렬한 뒤,
					
					//>> TODO : 이거 문제인듯
					List<apMeshEdge> splitTargetEdge = new List<apMeshEdge>();
					List<Vector2> splitTargetCrossPos = new List<Vector2>();

					List<apVertex> addedVertices = new List<apVertex>();

					apMeshEdge resultEdge = null;

					for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
					{
						apMeshEdge otherEdge = _edges[iEdge];
						if (
							(otherEdge._vert1 == vert1 || otherEdge._vert2 == vert2)
							|| (otherEdge._vert1 == vert2 || otherEdge._vert2 == vert1)
							)
						{
							//Vertex를 하나라도 가진다면 교차되지는 않는다.
							continue;
						}

						apMeshEdge.CrossCheckResult crossResult = apMeshEdge.IsEdgeCrossWithResult(otherEdge, vert1._pos, vert2._pos);

						if (crossResult == null)
						{
							continue;
						}
						if (!crossResult._isCross)
						{
							continue;
						}
						apVertex overlapVertex = null;
						Vector2 crossPos = crossResult._posW;
						if (crossResult._isAnyPointSame)
						{
							//하나가 겹친다.
							//Split는 필요없다.
							overlapVertex = crossResult._overlapVert;
						}
						else
						{
							//교차된 점을 받고, 만약 그 점이 기존 Edge의 시작/끝 점과 가까우면 그냥 그 점으로 둔다.
							if (Vector2.Distance(otherEdge._vert1._pos, crossPos) <= 1.0f)
							{
								overlapVertex = otherEdge._vert1;
							}
							else if (Vector2.Distance(otherEdge._vert2._pos, crossPos) <= 1.0f)
							{
								overlapVertex = otherEdge._vert2;
							}
						}

						if (overlapVertex != null)
						{
							//선을 분할할 필요가 없다.
							addedVertices.Add(overlapVertex);
						}
						else
						{
							//선을 분할하자
							// => 바로 분할하지 말고 분할해야한다는 걸 저장한 뒤 나중에 한꺼번에 하자
							//바로 해버리면 For 도중에 Edge가 추가/삭제 되서 인덱스가 꼬임
							splitTargetEdge.Add(otherEdge);
							splitTargetCrossPos.Add(crossPos);
						}
					}

					for (int iSplit = 0; iSplit < splitTargetEdge.Count; iSplit++)
					{
						apVertex splitVert = SplitEdge(splitTargetEdge[iSplit], splitTargetCrossPos[iSplit]);
						if(splitVert == null)
						{
							Debug.LogError("Make New Edge > Split Failed");
							continue;
						}
						addedVertices.Add(splitVert);
					}

					//이제 중간에 추가된 Vert와 시작, 끝 Vert를 포함해서 "거리"순으로 정렬을 하자
					addedVertices.Add(vert1);
					addedVertices.Add(vert2);

					addedVertices.Sort(delegate (apVertex a, apVertex b)
					{
						float distA = Vector2.Distance(a._pos, vert1._pos);
						float distB = Vector2.Distance(b._pos, vert1._pos);

						return (int)((distA - distB) * 1000.0f);
					});



					//Vertex들을 하나씩 연결하자
					for (int iVert = 0; iVert < addedVertices.Count - 1; iVert++)
					{
						apVertex vertA = addedVertices[iVert];
						apVertex vertB = addedVertices[iVert + 1];

						//새로 Edge를 추가하자.
						apMeshEdge newEdge = new apMeshEdge(vertA, vertB);
						_edges.Add(newEdge);

						if(resultEdge == null)
						{
							resultEdge = newEdge;
						}
					}

					_isEdgeWorkAnyChanged = true;

					return resultEdge;
				}


				else
				{
					//새로 Edge를 추가해도 되겠다.
					apMeshEdge newEdge = new apMeshEdge(vert1, vert2);
					_edges.Add(newEdge);

					_isEdgeWorkAnyChanged = true;

					//추가
					//만약, 이 Edge가 어떤 Polygon의 두 점을 포함한다면, 그 Polygon은 삭제해야한다.
					_polygons.RemoveAll(delegate (apMeshPolygon a)
					{
						return (a._verts.Contains(vert1) && a._verts.Contains(vert2));
					});

					return newEdge;
				}
			}

			return null;
		}

		/// <summary>
		/// Edge의 일정 지점에 Vertex를 추가하고, edge를 두개의 edge로 분할한다.
		/// splitPoint는 Edge의 선상에 위치해야한다.(값 체크는 하지 않는다)
		/// 리턴은 분할 당시의 Vertex
		/// </summary>
		/// <param name="edge"></param>
		/// <param name="splitPoint"></param>
		public apVertex SplitEdge(apMeshEdge edge, Vector2 splitPointPosW)
		{
			//1. 일단 폴리곤은 걍 삭제. 귀찮아..
			apVertex vert1 = edge._vert1;
			apVertex vert2 = edge._vert2;

			//만약, 이 Edge가 어떤 Polygon의 두 점을 포함한다면, 그 Polygon은 삭제해야한다.
			_polygons.RemoveAll(delegate (apMeshPolygon a)
			{
				return (a._verts.Contains(vert1) && a._verts.Contains(vert2));
			});

			//2. Vertex를 추가한다.
			
			apVertex splitVert = AddVertexAutoUV(splitPointPosW);
			if(splitVert == null)
			{
				Debug.LogError("AnyPortrait : SplitEdge() Failed");
				return null;
			}

			//3. 새로운 Edge 2개를 만든다.
			apMeshEdge addedEdge1 = new apMeshEdge(vert1, splitVert);
			apMeshEdge addedEdge2 = new apMeshEdge(vert2, splitVert);

			_edges.Add(addedEdge1);
			_edges.Add(addedEdge2);

			//4. 기존의 Edge를 삭제한다.
			_edges.Remove(edge);

			_isEdgeWorkAnyChanged = true;

			return splitVert;
		}

		public void RemoveEdge(apMeshEdge edge)
		{
			//Edge를 선택하면
			//1. Edge가 포함된 모든 Polygon을 삭제해야 한다.
			//2. IndexBuffer 리프레시

			_polygons.RemoveAll(delegate (apMeshPolygon a)
			{
				return a.IsEdgeContain(edge);
			});

			_edges.Remove(edge);

			RefreshPolygonsToIndexBuffer();

			_isEdgeWorkAnyChanged = true;
		}

		public bool IsEdgeWorking() { return _isEdgeWorking; }

		public bool IsAnyWorkedEdge()
		{
			//return _edges.Count > 0;
			return _isEdgeWorkAnyChanged;
		}

		//public void CancelEdgeWork()
		//{
		//	//_edges.Clear();
		//	_isEdgeWorking = false;
		//	_isEdgeWorkAnyChanged = false;
		//}

		private bool IsValidPolygon(apMeshPolygon polygon)
		{
			//유효성 검사를 하자
			if (polygon._verts.Count > 3)
			{
				int nResultVerts = polygon._verts.Count;
				for (int iVert = 0; iVert < nResultVerts; iVert++)
				{
					apVertex vert1 = polygon._verts[iVert];

					int iVertEx_Prev = ((iVert - 1) + nResultVerts) % nResultVerts;
					int iVertEx_Next = (iVert + 1) % nResultVerts;

					for (int iNext = 0; iNext < nResultVerts; iNext++)
					{
						if (iNext == iVert || iNext == iVertEx_Prev || iNext == iVertEx_Next)
						{
							continue;
						}

						apVertex vert2 = polygon._verts[iNext];
						if (_edges.Exists(delegate (apMeshEdge a)
							{
								return a.IsSameEdge(vert1, vert2);
							}))
						{
							//Debug.LogError("[" + iResult + "] Poly Error : [" + iVert + " / " + iNext + "]  (" + nResultVerts + ")");
							return false;
						}
					}
				}
			}

			//현재 Tri 내부에서 다른 점이 등장하는가
			for (int iTri = 0; iTri < polygon._tris.Count; iTri++)
			{
				apMeshTri tri = polygon._tris[iTri];
				//이 Tri에 들어오는 다른 점이 있는가
				bool isExist = _vertexData.Exists(delegate (apVertex a)
				{
					if (a == tri._verts[0] || a == tri._verts[1] || a == tri._verts[2])
					{
						return false;
					}
					if (polygon._verts.Contains(a))
					{
						return false;
					}
					if (apMeshTri.IsPointInTri(a, tri._verts[0], tri._verts[1], tri._verts[2]))
					{
						return true;
					}
					return false;
				});

				if (isExist)
				{
					return false;
				}
			}

			return true;
		}

		public void MakeEdgesToPolygonAndIndexBuffer(List<PolygonExceptionSet> exceptionSet = null)
		{
			//Edge들을 연결해서
			//1. Polygon을 만든다.
			//2. Polygon들을 Tri로 만든다.
			//3. Tri를 Index Buffer에 넣는다.
			_isEdgeWorkAnyChanged = false;
			_isEdgeWorking = false;

			SortVertexData();

			_indexBuffer.Clear();
			//_polygons.Clear();//다 삭제하지 말자
			//Edge에 해당하지 않는 것만 삭제
			_polygons.RemoveAll(delegate (apMeshPolygon a)
			{
				for (int i = 0; i < a._edges.Count; i++)
				{
					bool isExist = _edges.Exists(delegate (apMeshEdge b)
					{
						return b.IsSameEdge(a._edges[i]);
					});

					if (!isExist)
					{
					//존재하지 않는 Edge가 있을 때
					return true;
					}
				}
				return false;
			//return IsValidPolygon(a);
			});


			for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
			{
				apMeshEdge curEdge = _edges[iEdge];
				apVertex endVert = curEdge._vert1;
				apVertex startVert = curEdge._vert2;
				
				// Polygon을 만드는 법
				// 1) Start Vert 를 결정
				// 2) Start Vert에서 하나씩 연결된 Edge를 찾는다 (여러개)
				// 3) 그중 한개의 Edge의 끝점을 Next Vert로 정한다. 다른 점은 Prev Vert
				// 4-1) 일단 Next, Prev, End 세개의 점으로 하는 가상의 Tri에 대해 다른 점이 속해있으면 => 루틴 취소
				// 4-2) Next -> End에 해당하는 Edge가 있으면 => Polygon 완성
				// 4-3) Next -> End에 해당하는 Edge가 없으면 Next Vert를 Start로 삼아서 2) 반복
				// <조건> 탐색 깊이가 8 이상이면 종료
				// 5) 완성된 Polygon이 복수개라면, 그 중에서 level이 낮은걸 선택한다.

				List<apMeshEdge> calculatedEdges = new List<apMeshEdge>();
				calculatedEdges.Add(curEdge);

				List<apVertex> calculateVert = new List<apVertex>();
				calculateVert.Add(endVert);
				calculateVert.Add(startVert);

				int maxLevel = 8;
				List<MakePolygonResult> results = RecursiveMakePolygon(endVert, startVert, curEdge, 1, calculatedEdges, calculateVert, maxLevel);
				if (results == null)
				{
					//폴리곤을 만들지 못했다.
					continue;
				}

				if (results.Count == 0)
				{
					continue;
				}

				//유효성 검사
				//일단 LTRB를 만들고
				//다른 모든 Vertex에 대해서, Polygon 내부에 들어가는 Vertex가 있다면 무효다.
				for (int iResult = 0; iResult < results.Count; iResult++)
				{
					MakePolygonResult curResult = results[iResult];

					curResult.MakeLTRB();
					curResult._isValid = true;

					if (!curResult.CheckValidation(maxLevel))
					{
						//리스트가 유효하지 않다.
						curResult._isValid = false;
						//Debug.Log("리스트 유효성 검사에서 발견");
					}
					else
					{
						//아직 유효할 때
						//외부 버텍스 체크
						apVertex curVert = null;
						for (int iVert = 0; iVert < _vertexData.Count; iVert++)
						{
							curVert = _vertexData[iVert];
							if (curResult.IsInPolygonResult(curVert))
							{
								//다른 점이 들어왔다 = 유효하지 않다.
								curResult._isValid = false;
								break;
							}

						}
					}
					//if(curResult._vertices.Count >= maxLevel)
					//{
					//	Debug.LogError("잘못된 Result : " + curResult._vertices.Count);
					//	curResult._isValid = false;
					//}
				}
				results.RemoveAll(delegate(MakePolygonResult a)
				{
					return !a._isValid;
				});
				//if (nRemoved > 0)
				//{
				//	Debug.LogError("[" + nRemoved + "] 개의 유효하지 않은 Polygon 조회 결과가 삭제됨");
				//}




				if (results.Count > 2)
				{
					//Debug.LogError("Make Polygon Error : Result Count is Over 2 (" + results.Count + ")");

					//기하학적으로 문제가 있다.
					//Edge 하나에 최대 폴리곤은 2개다.
					//혹시 모르니 일단 Sort를 한다.
					results.Sort(delegate (MakePolygonResult a, MakePolygonResult b)
					{
						return a._level - b._level;//오름차순 (낮은 레벨이 좋은거다)
				});
				}

				//Result를 받아서 폴리곤을 만들어보자
				int nResult = results.Count;
				//if(nResult > 2)
				//{
				//	nResult = 2;
				//}


				for (int iResult = 0; iResult < nResult; iResult++)
				{
					//if (existPolygon != null)
					//{
					//	//일종의 에러
					//	//다른 곳에서 이미 처리된 폴리곤이 있다면
					//	//여기서는 처리하지 않는다.
					//	if(existPolygon.IsSamePolygon(results[iResult]._vertices))
					//	{
					//		continue;
					//	}
					//}

					bool isExist = _polygons.Exists(delegate (apMeshPolygon a)
					{
					//return a.IsEdgeContain(curEdge);
					return a.IsSamePolygon(results[iResult]._vertices);
					});

					if (isExist)
					{
						continue;
					}

					if(exceptionSet != null)
					{
						//예외 목록에 포함되는지 확인
						bool isExcepted = false;
						for (int i = 0; i < exceptionSet.Count; i++)
						{
							if(exceptionSet[i].IsContains(results[iResult]._vertices))
							{
								//예외 목록에 포함된다.
								isExcepted = true;
								break;
							}
						}

						if(isExcepted)
						{
							//예외!
							continue;
						}
					}

					//유효성 검사를 하자
					bool isValid = true;
					if (results[iResult]._vertices.Count > 3)
					{
						int nResultVerts = results[iResult]._vertices.Count;
						for (int iVert = 0; iVert < nResultVerts; iVert++)
						{
							apVertex vert1 = results[iResult]._vertices[iVert];

							//apVertex vert2 = results[iResult]._vertices[(iVert + 2) % nResultVerts];
							//if (_edges.Exists(delegate (apMeshEdge a)
							//	{
							//		return a.IsSameEdge(vert1, vert2);
							//	}))
							//{
							//	isValid = false;
							//	break;
							//}

							int iVertEx_Prev = ((iVert - 1) + nResultVerts) % nResultVerts;
							int iVertEx_Next = (iVert + 1) % nResultVerts;

							for (int iNext = 0; iNext < nResultVerts; iNext++)
							{
								if (iNext == iVert || iNext == iVertEx_Prev || iNext == iVertEx_Next)
								{
									continue;
								}

								apVertex vert2 = results[iResult]._vertices[iNext];
								if (_edges.Exists(delegate (apMeshEdge a)
								 {
									 return a.IsSameEdge(vert1, vert2);
								 }))
								{
									//Debug.LogError("[" + iResult + "] Poly Error : [" + iVert + " / " + iNext + "]  (" + nResultVerts + ")");
									isValid = false;
									break;
								}
							}

							if (!isValid)
							{
								break;
							}
						}
					}

					if (isValid)
					{
						apMeshPolygon newPolygon = new apMeshPolygon();
						newPolygon.SetVertexAndEdges(results[iResult]._vertices, results[iResult]._edges);

						_polygons.Add(newPolygon);
					}
				}
			}


			_indexBuffer.Clear();

			_edges.RemoveAll(delegate (apMeshEdge a)
			{
				return a == null;
			});

			_edges.RemoveAll(delegate (apMeshEdge a)
			{
				return !_vertexData.Contains(a._vert1) || !_vertexData.Contains(a._vert2) || (a._vert1 == a._vert2);
			});


			_polygons.RemoveAll(delegate (apMeshPolygon a)
			{
				List<apMeshEdge> edges = a._edges;
				for (int i = 0; i < edges.Count; i++)
				{
					if (!_edges.Contains(edges[i]) || edges[i] == null)
					{
					//하나라도 삭제된 Edge를 포함한다면..
					return true;
					}
				}
				return false;

			});


			//추가 : Depth에 맞게 Polygon을 Sort한다.
			_polygons.Sort(delegate (apMeshPolygon a, apMeshPolygon b)
			{
				int depthA = (int)(a.GetDepth() * 1000.0f);
				int depthB = (int)(b.GetDepth() * 1000.0f);
				return depthA - depthB;
			});

			//폴리곤들이 만들어졌다.
			//이걸 이제 Tri들의 조합으로 만들어보자
			for (int iPoly = 0; iPoly < _polygons.Count; iPoly++)
			{
				_polygons[iPoly].MakeHiddenEdgeAndTri();

				for (int iTri = 0; iTri < _polygons[iPoly]._tris.Count; iTri++)
				{
					apMeshTri tri = _polygons[iPoly]._tris[iTri];
					_indexBuffer.Add(tri._verts[0]._index);
					_indexBuffer.Add(tri._verts[1]._index);
					_indexBuffer.Add(tri._verts[2]._index);

					//_indexBuffer.Add(tri._verts[2]._index);
					//_indexBuffer.Add(tri._verts[1]._index);
					//_indexBuffer.Add(tri._verts[0]._index);
				}
			}
			SortVertexData();
		}





		private class AutoLinkEdgeData
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
			public AutoLinkEdgeData(apVertex vert1, apVertex vert2)
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

		public void AutoLinkEdges()
		{
			//자동으로 Vertex를 서로 연결해준다.
			//조건
			//- 짧은 Edge 순서로 연결할 것
			//- 이미 3각, 4각 Polygon(또는 가능한 것)을 이루고 있다면 패스
			//- 이미 Polygon이 생성 되었다면 패스
			//- 다른 Edge를 관통한다면 패스. 단, HiddenEdge는 관통할 수 있다.

			SortVertexData();
			if (_vertexData.Count < 3)
			{
				return;
			}

			List<AutoLinkEdgeData> linkData = new List<AutoLinkEdgeData>();
			for (int iVert = 0; iVert < _vertexData.Count; iVert++)
			{
				apVertex srcVert = _vertexData[iVert];
				//연결되지 않은 Vertex도 찾아야 한다.
				for (int iTargetVert = 0; iTargetVert < _vertexData.Count; iTargetVert++)
				{
					apVertex targetVert = _vertexData[iTargetVert];

					if (targetVert == srcVert)
					{
						continue;
					}
					if (linkData.Exists(delegate (AutoLinkEdgeData a)
					 {
						 return (a._vert1 == srcVert && a._vert2 == targetVert)
						 || (a._vert2 == srcVert && a._vert1 == targetVert);
					 }))
					{
						continue;
					}

					//1) 이미 Edge가 있거나
					//2) 이미 두개의 Vert가 포함된 Polygon이 있거나
					//3) 다른 Edge를 관통하면
					//>> 패스
					if (GetEdge(srcVert, targetVert) != null)
					{
						continue;
					}
					if (_polygons.Exists(delegate (apMeshPolygon a)
					 {
						 return a.IsVertexContain(srcVert) && a.IsVertexContain(targetVert);
					 }))
					{
						continue;
					}

					bool isAnyCross = false;
					for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
					{
						apMeshEdge otherEdge = _edges[iEdge];
						if (otherEdge._vert1 == srcVert || otherEdge._vert1 == targetVert ||
							otherEdge._vert2 == srcVert || otherEdge._vert2 == targetVert)
						{
							continue;
						}
						if (apMeshEdge.IsEdgeCrossApprox(otherEdge._vert1, otherEdge._vert2, srcVert, targetVert))
						{
							isAnyCross = true;
							break;
						}
					}
					if (isAnyCross)
					{
						continue;
					}

					//링크 대상에 추가한다.
					AutoLinkEdgeData newLinkData = new AutoLinkEdgeData(srcVert, targetVert);

					//주변의 연결된 Edge를 찾아서, 각각의 Angle을 넣어준다.
					//MinAngle을 받아서 너무 작은 Angle을 가지는 Edge는 삭제한다.
					for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
					{
						apMeshEdge otherEdge = _edges[iEdge];
						if (otherEdge._vert1 == srcVert || otherEdge._vert1 == targetVert ||
							otherEdge._vert2 == srcVert || otherEdge._vert2 == targetVert)
						{
							float angle = Vector2.Angle(otherEdge._vert2._pos - otherEdge._vert1._pos, targetVert._pos - srcVert._pos);
							if (angle > 90.0f)
							{
								angle = 180.0f - angle;
							}
							newLinkData.SetEdgeAngle(angle);
						}
					}
					//Debug.Log("Link Data : Angle [" + newLinkData._minAngle + "], Length [" + newLinkData._length + "]");
					if (newLinkData._minAngle > 10.0f)
					{
						linkData.Add(newLinkData);
					}
				}
			}

			if (linkData.Count == 0)
			{
				return;
			}

			//짧은 순으로 정렬한다.
			linkData.Sort(delegate (AutoLinkEdgeData a, AutoLinkEdgeData b)
			{
				return b.Score - a.Score;
			});

			//Debug.Log("Link Data [" + linkData.Count + "]");

			//이제 하나씩 넣어보자
			//- 해당 Edge의 Vertex가 Polygon을 형성할 수 있는 가능성이 있고, 3, 4각형 Polygon이라면 생략한다.
			for (int iLinkData = 0; iLinkData < linkData.Count; iLinkData++)
			{
				AutoLinkEdgeData curLink = linkData[iLinkData];

				//Debug.Log("[" + iLinkData + "] >>");


				bool isAnyCross = false;
				for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
				{
					apMeshEdge otherEdge = _edges[iEdge];
					if (otherEdge.IsSameEdge(curLink._vert1, curLink._vert2))
					{
						isAnyCross = true;
						break;
					}
					if (otherEdge._vert1 == curLink._vert1 || otherEdge._vert1 == curLink._vert2 ||
						otherEdge._vert2 == curLink._vert1 || otherEdge._vert2 == curLink._vert2)
					{
						continue;
					}
					if (apMeshEdge.IsEdgeCrossApprox(otherEdge._vert1, otherEdge._vert2, curLink._vert1, curLink._vert2))
					{
						isAnyCross = true;
						break;
					}
				}
				if (isAnyCross)
				{
					//Debug.LogError("[" + iLinkData + "] >> Edge Cross");
					continue;
				}


				apMeshEdge tmpEdge = new apMeshEdge(curLink._vert1, curLink._vert2);

				List<apMeshEdge> calculatedEdges = new List<apMeshEdge>();
				calculatedEdges.Add(tmpEdge);

				List<apVertex> calculateVert = new List<apVertex>();
				calculateVert.Add(curLink._vert1);
				calculateVert.Add(curLink._vert2);

				int maxLevel = 8;
				List<MakePolygonResult> results = RecursiveMakePolygon(curLink._vert1, curLink._vert2, tmpEdge, 1, calculatedEdges, calculateVert, maxLevel);

				if (results != null)
				{
					//유효성 체크
					//유효성 검사
					//일단 LTRB를 만들고
					//다른 모든 Vertex에 대해서, Polygon 내부에 들어가는 Vertex가 있다면 무효다.
					for (int iResult = 0; iResult < results.Count; iResult++)
					{
						MakePolygonResult curResult = results[iResult];

						curResult.MakeLTRB();
						curResult._isValid = true;

						if (!curResult.CheckValidation(maxLevel))
						{
							//리스트가 유효하지 않다.
							curResult._isValid = false;
							//Debug.Log("리스트 유효성 검사에서 발견");
						}
						else
						{
							//아직 유효할 때
							//외부 버텍스 체크
							apVertex curVert = null;
							for (int iVert = 0; iVert < _vertexData.Count; iVert++)
							{
								curVert = _vertexData[iVert];
								if (curResult.IsInPolygonResult(curVert))
								{
									//다른 점이 들어왔다 = 유효하지 않다.
									curResult._isValid = false;
									break;
								}

							}
						}
						//if(curResult._vertices.Count >= maxLevel)
						//{
						//	Debug.LogError("잘못된 Result : " + curResult._vertices.Count);
						//	curResult._isValid = false;
						//}
					}
					results.RemoveAll(delegate (MakePolygonResult a)
					{
						return !a._isValid;
					});
				}

				bool isPolygon_3 = false;
				bool isPolygon_4 = false;
				bool isPolygon_5 = false;
				bool isPolygon_6 = false;

				if (results != null)
				{
					for (int iResult = 0; iResult < results.Count; iResult++)
					{
						int nVert = results[iResult]._vertices.Count;
						if (nVert == 3)
						{ isPolygon_3 = true; }
						if (nVert == 4)
						{ isPolygon_4 = true; }
						if (nVert == 5)
						{ isPolygon_5 = true; }
						if (nVert == 6)
						{ isPolygon_6 = true; }
					}
				}

				//- 4각형이 있다
				//- 5, 6각형이 있다 + 3각형을 생성하지는 않는다.

				if (isPolygon_4
					|| ((isPolygon_5 || isPolygon_6) && !isPolygon_3)
					)
				{
					//Debug.LogError("[" + iLinkData + "] >> Small Polygon");
					//continue;
					MakeNewEdge(curLink._vert1, curLink._vert2, false);
				}


			}
		}


		public void RefreshPolygonsToIndexBuffer()
		{

			_edges.RemoveAll(delegate (apMeshEdge a)
			{
				return a == null;
			});

			_edges.RemoveAll(delegate (apMeshEdge a)
			{
				return !_vertexData.Contains(a._vert1) || !_vertexData.Contains(a._vert2) || (a._vert1 == a._vert2);
			});


			_polygons.RemoveAll(delegate (apMeshPolygon a)
			{
				List<apMeshEdge> edges = a._edges;
				for (int i = 0; i < edges.Count; i++)
				{
					if (!_edges.Contains(edges[i]) || edges[i] == null)
					{
					//하나라도 삭제된 Edge를 포함한다면..
					return true;
					}
				}
				return false;

			});

			//추가 : Depth에 맞게 Polygon을 Sort한다.
			_polygons.Sort(delegate (apMeshPolygon a, apMeshPolygon b)
			{
				int depthA = (int)(a.GetDepth() * 1000.0f);
				int depthB = (int)(b.GetDepth() * 1000.0f);
				return depthA - depthB;
			});

			for (int i = 0; i < _polygons.Count; i++)
			{
				_polygons[i].SortTriByDepth();
			}

			for (int i = 0; i < _edges.Count; i++)
			{
				_edges[i]._isOutline = true;
				_edges[i]._nTri = 0;
			}




			_indexBuffer.Clear();

			for (int iPoly = 0; iPoly < _polygons.Count; iPoly++)
			{
				for (int iTri = 0; iTri < _polygons[iPoly]._tris.Count; iTri++)
				{
					apMeshTri tri = _polygons[iPoly]._tris[iTri];
					_indexBuffer.Add(tri._verts[0]._index);
					_indexBuffer.Add(tri._verts[1]._index);
					_indexBuffer.Add(tri._verts[2]._index);

					apMeshEdge edge0 = GetEdge(tri._verts[0], tri._verts[1]);
					apMeshEdge edge1 = GetEdge(tri._verts[1], tri._verts[2]);
					apMeshEdge edge2 = GetEdge(tri._verts[2], tri._verts[0]);

					if (edge0 != null)
					{
						edge0._nTri++;
						if (edge0._nTri >= 2)
						{ edge0._isOutline = false; }
					}

					if (edge1 != null)
					{
						edge1._nTri++;
						if (edge1._nTri >= 2)
						{ edge1._isOutline = false; }
					}

					if (edge2 != null)
					{
						edge2._nTri++;
						if (edge2._nTri >= 2)
						{ edge2._isOutline = false; }
					}

				}
			}

			SortVertexData();
		}

		public class MakePolygonResult
		{
			public List<apVertex> _vertices = new List<apVertex>();
			public List<apMeshEdge> _edges = new List<apMeshEdge>();
			public int _level = 0;
			private float _xMin = 0.0f;
			private float _xMax = 0.0f;
			private float _yMin = 0.0f;
			private float _yMax = 0.0f;
			private bool _isLTRBInit = false;
			public bool _isValid = true;

			public void MakeLTRB()
			{
				if(_vertices.Count == 0)
				{
					return;
				}
				apVertex curVert = null;
				curVert = _vertices[0];
				_xMin = curVert._pos.x;
				_xMax = curVert._pos.x;
				_yMin = curVert._pos.y;
				_yMax = curVert._pos.y;

				for (int i = 1; i < _vertices.Count; i++)
				{
					curVert = _vertices[i];
					_xMin = Mathf.Min(_xMin, curVert._pos.x);
					_xMax = Mathf.Max(_xMax, curVert._pos.x);
					_yMin = Mathf.Min(_yMin, curVert._pos.y);
					_yMax = Mathf.Max(_yMax, curVert._pos.y);
				}

				_isLTRBInit = true;
			}

			/// <summary>
			/// 외부의 Vertex가 이 PolygonResult 안에 있으면 True를 리턴한다.
			/// 유효하지 않거나 밖에 있다면 False를 리턴한다.
			/// </summary>
			/// <param name="vert"></param>
			/// <returns></returns>
			public bool IsInPolygonResult(apVertex vert)
			{
				if(_vertices.Contains(vert))
				{
					//이미 포함되어 있기 때문에 이건 체크 대상이 아니다.
					return false;
				}
				//폴리곤 안에 버텍스가 있는가

				//1. 일단 LTRB 안에 있는지 체크
				if(!_isLTRBInit)
				{
					MakeLTRB();
				}
				if(vert._pos.x < _xMin || vert._pos.x > _xMax || 
					vert._pos.y < _yMin || vert._pos.y > _yMax)
				{
					return false;
				}

				//2. 각도 체크
				//"Edge의 2개의 버텍스와 vert 위치가 이루는 각도"의 총 합이 360 (+bias) 라면 안에 있는 것이다.
				//360도 보다 크거나 작다면 밖에 있는 것이다.
				float totalAngle = 0.0f;
				apMeshEdge curEdge = null;
				for (int i = 0; i < _edges.Count; i++)
				{
					curEdge = _edges[i];
					totalAngle += Vector2.Angle(curEdge._vert1._pos - vert._pos, curEdge._vert2._pos - vert._pos);
				}

				//Bias : 2도
				float bias = 2.0f;
				if(totalAngle < 360.0f - bias || totalAngle > 360.0f + bias)
				{
					//밖에 있다.
					return false;
				}

				//선에 있거나 안에 있다.
				return true;
			}


			public bool CheckValidation(int maxLevel)
			{
				//1. 개수 체크
				if(_vertices.Count >= maxLevel)
				{
					//Debug.Log("유효성 검사 : Level 오버 : " + _vertices.Count + " / " + maxLevel);
					return false;
				}

				//2. 온전하게 저장된 Vertex 인가?
				//- 모든 Edge에 대해서 Vertex가 두번씩 등장하는가?
				//- Vertex Dictionary 개수가 Vertex 리스트 개수와 같은가?
				Dictionary<apVertex, int> vertCount = new Dictionary<apVertex, int>();
				apMeshEdge curEdge = null;
				apVertex vert_1 = null;
				apVertex vert_2 = null;
				for (int i = 0; i < _edges.Count; i++)
				{
					curEdge = _edges[i];
					vert_1 = curEdge._vert1;
					vert_2 = curEdge._vert2;
					if (!vertCount.ContainsKey(vert_1))	{ vertCount.Add(vert_1, 1); }
					else								{ vertCount[vert_1] = vertCount[vert_1] + 1; }

					if (!vertCount.ContainsKey(vert_2))	{ vertCount.Add(vert_2, 1); }
					else								{ vertCount[vert_2] = vertCount[vert_2] + 1; }

					if(!_vertices.Contains(vert_1) || !_vertices.Contains(vert_2))
					{
						//유효한 Vertex가 아니다.
						//Debug.Log("유효성 검사 : 리스트에 없는 버텍스");
						return false;
					}
				}
				if(vertCount.Count != _vertices.Count)
				{
					//Vertex에 대한 개수가 맞지 않다.
					//Debug.Log("유효성 검사 : Edge에서의 Vertex와 실제 Vertex 개수가 다름 [" + vertCount.Count + " / " + _vertices.Count + "]");
					return false;
				}

				foreach(KeyValuePair<apVertex, int> countUnit in vertCount)
				{
					if(countUnit.Value != 2)
					{
						//버텍스 개수가 2가 아니다.
						//Debug.Log("유효성 검사 : Edge 내에서 등장한 버텍스 개수가 2가 아니다.");
						return false;
					}
				}

				return true;
			}
		}
		private List<MakePolygonResult> RecursiveMakePolygon(	apVertex endVert, 
																apVertex prevVert, 
																apMeshEdge prevEdge, 
																int curLevel, 
																List<apMeshEdge> calculatedEdges, 
																List<apVertex> calculatedVertex, int maxLevel)
		{
			if (curLevel > maxLevel)
			{
				//Debug.LogError("Too Many Loop : " + curLevel);
				return null;
			}

			List<MakePolygonResult> results = new List<MakePolygonResult>();
			//MakePolygonResult optResult = null;

			//Prev Vert와 연결된 Edge를 모두 찾는다.
			for (int i = 0; i < _edges.Count; i++)
			{
				apMeshEdge nextEdge = _edges[i];
				if (calculatedEdges.Contains(nextEdge))
				{
					continue;
				}
				else
				{
					//calculatedEdges.Add(nextEdge);
				}

				if (nextEdge == prevEdge)
				{
					continue;
				}



				// 3) 그중 한개의 Edge의 끝점을 Next Vert로 정한다. 다른 점은 Prev Vert

				// endVert - .... [prev Vert] ~~ next Vert
				apVertex nextVert = null;
				if (nextEdge._vert1 == prevVert)
				{
					nextVert = nextEdge._vert2;
				}
				else if (nextEdge._vert2 == prevVert)
				{
					nextVert = nextEdge._vert1;
				}
				else
				{
					//연결되지 않았다.
					continue;
				}

				if (calculatedVertex.Contains(nextVert))
				{
					//이미 처리된 Vert이다.
					continue;
				}

				// 4-1) 일단 Next, Prev, End 세개의 점으로 하는 가상의 Tri에 대해 다른 점이 속해있으면 => 루틴 취소
				//Tri 체크
				bool isInTri = _vertexData.Exists(delegate (apVertex a)
				{
					if (a == endVert || a == prevVert || a == nextVert)
					{
						return false;
					}

					return apMeshTri.IsPointInTri(a, endVert, prevVert, nextVert);
					//return false;
				});

				if (isInTri)
				{
					//어떤 점이 해당 Tri (Polygon이 될..)에 들어있다.
					//Polygon이 성립되지 않는다.
					continue;
				}

				// 추가) 만약, 여태까지 지나온 Vertex들 중에서
				// endVert - .... [prev Vert] ~~ next Vert들을 제외한 "어떤 점"과 next를 이은 어떤 Edge가 발견되면
				// 취소
				if (curLevel > 1)
				{
					bool isOtherInnerEdge = false;

					for (int iCal = 0; iCal < calculatedVertex.Count; iCal++)
					{
						apVertex vert = calculatedVertex[iCal];
						if (vert == endVert || vert == nextVert || vert == prevVert)
						{
							continue;
						}

						if (_edges.Exists(delegate (apMeshEdge a)
											 {
												 return a.IsSameEdge(nextVert, vert);
											 }))
						{
							isOtherInnerEdge = true;
							break;
						}
					}

					if (isOtherInnerEdge)
					{
						//Debug.LogError("Inner Edge (" + curLevel + ")");
						continue;
					}
				}

				// 4-2) Next -> End에 해당하는 Edge가 있으면 => Polygon 완성
				// 4-3) Next -> End에 해당하는 Edge가 없으면 Next Vert를 Start로 삼아서 2) 반복

				apMeshEdge lastEdge = _edges.Find(delegate (apMeshEdge a)
				{
					return a.IsSameEdge(nextVert, endVert);
				});

				if (lastEdge != null)
				{
					if (curLevel >= 2)
					{
						//Debug.Log("Find Over Tri [" + curLevel + "]");
					}

					//폴리곤 완성
					//역순으로 넣어준다. //end도 넣자
					MakePolygonResult newResult = new MakePolygonResult();
					newResult._vertices.Add(endVert);
					newResult._vertices.Add(nextVert);
					//newResult._vertices.Add(prevVert);
					newResult._edges.Add(lastEdge);
					newResult._edges.Add(nextEdge);
					//newResult._edges.Add(prevEdge);
					newResult._level = curLevel;

					results.Add(newResult);
					//List<MakePolygonResult> directResult = new List<MakePolygonResult>();
					//directResult.Add(newResult);
					//return directResult;
				}
				else
				{
					List<apMeshEdge> newCalculatedEdges = new List<apMeshEdge>();
					for (int iCal = 0; iCal < calculatedEdges.Count; iCal++)
					{
						newCalculatedEdges.Add(calculatedEdges[iCal]);
					}
					newCalculatedEdges.Add(nextEdge);


					List<apVertex> newCalculatedVertex = new List<apVertex>();
					for (int iCal = 0; iCal < calculatedVertex.Count; iCal++)
					{
						newCalculatedVertex.Add(calculatedVertex[iCal]);
					}
					newCalculatedVertex.Add(nextVert);

					List<MakePolygonResult> nextResults = RecursiveMakePolygon(endVert, nextVert, nextEdge, curLevel++, newCalculatedEdges, newCalculatedVertex, maxLevel);
					if (nextResults != null)
					{
						for (int iResult = 0; iResult < nextResults.Count; iResult++)
						{
							if (nextResults[iResult] != null)
							{
								results.Add(nextResults[iResult]);
							}
						}
					}
				}
			}

			if (results.Count == 0)
			{
				return null;
			}

			for (int iResult = 0; iResult < results.Count; iResult++)
			{
				//현재 단계의 결과를 추가해주자
				results[iResult]._vertices.Add(prevVert);
				results[iResult]._edges.Add(prevEdge);
			}

			return results;

		}


		#region [미사용 코드] Edge가 임시 변수일 때의 코드
		//public void MakeEdgesToIndexBuffer()
		//{
		//	_isEdgeWorking = false;

		//	//일단, Edge 하나를 선택한다.
		//	//Vert1, Vert2를 기준으로, 연결된 걸 찾는다.
		//	//Vert마다 Level을 늘린다.
		//	_indexBuffer.Clear();

		//	List<TmpTri> tmpTris = new List<TmpTri>();

		//	for (int iEdge = 0; iEdge < _edges.Count; iEdge++)
		//	{
		//		Edge baseEdge = _edges[iEdge];


		//		//Edge와 Vertex를 한개 공유하고 있는 모든 Edge를 찾는다.
		//		List<Edge> linkedEdges = _edges.FindAll(delegate (Edge a)
		//		{
		//			bool isVert1Same = (a._vert1 == baseEdge._vert1 || a._vert1 == baseEdge._vert2);
		//			bool isVert2Same = (a._vert2 == baseEdge._vert1 || a._vert2 == baseEdge._vert2);
		//			if(isVert1Same == isVert2Same)
		//			{
		//				//둘다 맞거나 둘다 아니거나
		//				return false;
		//			}
		//			//둘중 하나만 연결된 경우
		//			return true;
		//		});

		//		List<Edge> remainedEdges = new List<Edge>();

		//		//링크로 연결된 Edge들 중 하나를 선택한다.
		//		for (int iLinked = 0; iLinked < linkedEdges.Count; iLinked++)
		//		{
		//			Edge linkedEdge = linkedEdges[iLinked];

		//			//이제 Base와 linkedEdge 두개와 연결된 Edge를 찾는다.
		//			//공유하고 있는 점/공유하지 않는 점(2개)를 찾자
		//			apVertex sharedVertex = null;
		//			apVertex findVert1 = null;
		//			apVertex findVert2 = null;
		//			if(baseEdge._vert1 == linkedEdge._vert1 || baseEdge._vert1 == linkedEdge._vert2)
		//			{
		//				sharedVertex = baseEdge._vert1;
		//				findVert1 = baseEdge._vert2;
		//			}
		//			else
		//			{
		//				sharedVertex = baseEdge._vert2;
		//				findVert1 = baseEdge._vert1;
		//			}

		//			if(linkedEdge._vert1 == sharedVertex)
		//			{
		//				findVert2 = linkedEdge._vert2;
		//			}
		//			else
		//			{
		//				findVert2 = linkedEdge._vert1;
		//			}

		//			//이제 findVert1, findVert2를 가진 Edge를 찾는다.
		//			//이 Edge를 포함하면 바로 Tri 완성
		//			Edge bridgeEdge = _edges.Find(delegate (Edge a)
		//			{
		//				return a.IsSameEdge(findVert1, findVert2);
		//			});


		//			//찾았다!
		//			if(bridgeEdge != null)
		//			{
		//				TmpTri newTri = new TmpTri();
		//				newTri.SetEdge(baseEdge, linkedEdge, bridgeEdge);

		//				//이제 기존의 Tri에 충돌하는지 확인해야한다.
		//				bool isExist = tmpTris.Exists(delegate (TmpTri a)
		//				{
		//					return a.IsSameTri(newTri);
		//				});

		//				if(!isExist)
		//				{
		//					tmpTris.Add(newTri);
		//				}
		//			}
		//			else
		//			{
		//				//만약 못찾았다면..
		//				//일단 못찾은 리스트에 넣자
		//				remainedEdges.Add(linkedEdge);
		//			}
		//		}

		//		#region [미처리된 Quad를 Mesh로 만들기. 이건 일단 빼자]
		//		//if(remainedEdges.Count > 0)
		//		//{
		//		//	//Tri가 아닌 상태에서 Quad가 있을지도 모른다.
		//		//	for (int iA = 0; iA < remainedEdges.Count - 1; iA++)
		//		//	{
		//		//		TmpEdge edgeA = remainedEdges[iA];
		//		//		apVertex vertA = null;
		//		//		apVertex vertA_Linked = null;
		//		//		if(edgeA._vert1 == baseEdge._vert1 ||edgeA._vert1 == baseEdge._vert2)
		//		//		{
		//		//			vertA = edgeA._vert2;
		//		//			vertA_Linked = edgeA._vert1;
		//		//		}
		//		//		else
		//		//		{
		//		//			vertA = edgeA._vert1;
		//		//			vertA_Linked = edgeA._vert2;
		//		//		}
		//		//		for (int iB = iA + 1; iB < remainedEdges.Count; iB++)
		//		//		{
		//		//			if(iA == iB)
		//		//			{
		//		//				continue;
		//		//			}
		//		//			TmpEdge edgeB = remainedEdges[iB];

		//		//			apVertex vertB = null;
		//		//			apVertex vertB_Linked = null;
		//		//			if(edgeB._vert1 == baseEdge._vert1 ||edgeB._vert1 == baseEdge._vert2)
		//		//			{
		//		//				vertB = edgeB._vert2;
		//		//				vertB_Linked = edgeB._vert1;
		//		//			}
		//		//			else
		//		//			{
		//		//				vertB = edgeB._vert1;
		//		//				vertB_Linked = edgeB._vert2;
		//		//			}

		//		//			//이 두 Edge를 연결할 BridgeEdge가 있나 체크해보자
		//		//			TmpEdge bridgeEdge = _tmpEdges.Find(delegate (TmpEdge a)
		//		//			{
		//		//				return a.IsSameEdge(vertA, vertB);
		//		//			});

		//		//			if(bridgeEdge != null)
		//		//			{
		//		//				apVertex[] quadVerts = new apVertex[] {
		//		//					baseEdge._vert1, baseEdge._vert2,
		//		//					null, null};

		//		//				bool isEdgeA_linkedToVert1 = true;

		//		//				if(vertA_Linked == baseEdge._vert1)
		//		//				{
		//		//					//EdgeA는 Vert1과 연결되어 있다.
		//		//					// => EdgeB는 Vert2와 연결되어 있다.
		//		//					quadVerts[2] = vertB;
		//		//					quadVerts[3] = vertA;

		//		//					isEdgeA_linkedToVert1 = true;
		//		//				}
		//		//				else
		//		//				{
		//		//					//EdgeA는 Vert2와 연결되어 있다.
		//		//					// => EdgeB는 Vert1과 연결되어 있다.
		//		//					quadVerts[2] = vertA;
		//		//					quadVerts[3] = vertB;

		//		//					isEdgeA_linkedToVert1 = false;

		//		//				}
		//		//				//Quad를 찾았다.
		//		//				//이제 문제는
		//		//				//Quad가 정상적인지, 아니면
		//		//				//뭔가를 교차하는 등의 형태인지를 파악해야한다.
		//		//				bool isAnyCross = false;
		//		//				for (int iOther = 0; iOther < _tmpEdges.Count; iOther++)
		//		//				{
		//		//					TmpEdge otherEdge = _tmpEdges[iOther];
		//		//					if(otherEdge == baseEdge ||
		//		//						otherEdge == edgeA ||
		//		//						otherEdge == edgeB ||
		//		//						otherEdge == bridgeEdge)
		//		//					{
		//		//						continue;
		//		//					}

		//		//					for (int iCheck = 0; iCheck < 4; iCheck++)
		//		//					{
		//		//						bool isCross = FasterLineSegmentIntersection3(
		//		//										quadVerts[iCheck]._pos,
		//		//										quadVerts[(iCheck + 1) % 4]._pos,
		//		//										otherEdge._vert1._pos,
		//		//										otherEdge._vert2._pos);
		//		//						if(isCross)
		//		//						{
		//		//							isAnyCross = true;
		//		//							break;
		//		//						}
		//		//					}

		//		//					if(isAnyCross)
		//		//					{
		//		//						break;
		//		//					}
		//		//				}

		//		//				if(isAnyCross)
		//		//				{
		//		//					//이건 안된다..
		//		//					continue;
		//		//				}

		//		//				//이건 Bridge로 적합하다!
		//		//				//두개의 Tri로 만들자

		//		//				if(isEdgeA_linkedToVert1)
		//		//				{
		//		//					//v1	---base---	v2
		//		//					//|					|
		//		//					//edgeA				edgeB
		//		//					//|					|
		//		//					//vertA	--bridge--	vertB
		//		//					TmpEdge newEdge = new TmpEdge(baseEdge._vert1, vertB);

		//		//					TmpTri newTri1 = new TmpTri();
		//		//					TmpTri newTri2 = new TmpTri();

		//		//					newTri1.SetEdge(edgeA, bridgeEdge, newEdge);
		//		//					newTri2.SetEdge(baseEdge, newEdge, edgeB);

		//		//					tmpTris.Add(newTri1);
		//		//					tmpTris.Add(newTri2);
		//		//				}
		//		//				else
		//		//				{
		//		//					//v1	---base---	v2
		//		//					//|					|
		//		//					//edgeB				edgeA
		//		//					//|					|
		//		//					//vertB	--bridge--	vertA

		//		//					TmpEdge newEdge = new TmpEdge(baseEdge._vert1, vertA);

		//		//					TmpTri newTri1 = new TmpTri();
		//		//					TmpTri newTri2 = new TmpTri();

		//		//					newTri1.SetEdge(edgeB, bridgeEdge, newEdge);
		//		//					newTri2.SetEdge(baseEdge, newEdge, edgeA);

		//		//					tmpTris.Add(newTri1);
		//		//					tmpTris.Add(newTri2);
		//		//				}
		//		//			}
		//		//		}
		//		//	}
		//		//} 
		//		#endregion
		//	}

		//	_indexBuffer.Clear();
		//	SortVertexData();

		//	//이제 TmpEdge / TmpTri 정보를 indexBuffer에 반영하자
		//	for (int i = 0; i < tmpTris.Count; i++)
		//	{
		//		TmpTri tri = tmpTris[i];

		//		apVertex vert1 = tri._verts[0];
		//		apVertex vert2 = tri._verts[1];
		//		apVertex vert3 = tri._verts[2];

		//		// 2
		//		// 
		//		// 1   3
		//		//<이거라면 1, 2, 3>

		//		// 1   3
		//		// 
		//		// 2
		//		//<이건 1, 3, 2>

		//		if(vert1 == null) { Debug.LogError("Vert1 is Null"); }
		//		if(vert2 == null) { Debug.LogError("Vert2 is Null"); }
		//		if(vert3 == null) { Debug.LogError("Vert3 is Null"); }

		//		Vector3 normal = Vector3.Cross(	(vert2._pos - vert1._pos),
		//										(vert3._pos - vert1._pos));
		//		if(normal.z > 0)
		//		{
		//			//_indexBuffer.Add(vert1._index);
		//			//_indexBuffer.Add(vert2._index);
		//			//_indexBuffer.Add(vert3._index);


		//			_indexBuffer.Add(vert3._index);
		//			_indexBuffer.Add(vert2._index);
		//			_indexBuffer.Add(vert1._index);
		//		}
		//		else
		//		{
		//			_indexBuffer.Add(vert1._index);
		//			_indexBuffer.Add(vert2._index);
		//			_indexBuffer.Add(vert3._index);

		//			//_indexBuffer.Add(vert3._index);
		//			//_indexBuffer.Add(vert2._index);
		//			//_indexBuffer.Add(vert1._index);
		//		}
		//	}

		//	_edges.Clear();
		//	_isEdgeWorking = false;
		//} 
		#endregion

		public static bool IsApproxVector2(Vector2 p1, Vector2 p2)
		{
			return (Mathf.Abs(p1.x - p2.x) < 0.01f) && (Mathf.Abs(p1.y - p2.y) < 0.01f);
		}
		public static bool FasterLineSegmentIntersection3(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
		{
			return FasterLineSegmentIntersection(new Vector2(p1.x, p1.y),
													new Vector2(p2.x, p2.y),
													new Vector2(p3.x, p3.y),
													new Vector2(p4.x, p4.y));
		}
		public static bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			bool isP1Same = IsApproxVector2(p1, p3) || IsApproxVector2(p1, p4);
			bool isP2Same = IsApproxVector2(p2, p3) || IsApproxVector2(p2, p4);

			if (isP1Same && isP2Same)
			{
				return true;
			}
			else if (isP1Same || isP2Same)
			{
				//하나만 공유하는 경우
				//이건 아예 처리를 하지 말자
				return false;
			}

			Vector2 a = p2 - p1;
			Vector2 b = p3 - p4;
			Vector2 c = p1 - p3;

			float alphaNumerator = b.y * c.x - b.x * c.y;
			float alphaDenominator = a.y * b.x - a.x * b.y;
			float betaNumerator = a.x * c.y - a.y * c.x;
			float betaDenominator = a.y * b.x - a.x * b.y;

			bool doIntersect = true;

			if (alphaDenominator == 0 || betaDenominator == 0)
			{
				doIntersect = false;
			}
			else
			{

				if (alphaDenominator > 0)
				{
					if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
					{
						doIntersect = false;

					}
				}
				else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
				{
					doIntersect = false;
				}

				if (doIntersect && betaDenominator > 0)
				{
					if (betaNumerator < 0 || betaNumerator > betaDenominator)
					{
						doIntersect = false;
					}
				}
				else if (betaNumerator > 0 || betaNumerator < betaDenominator)
				{
					doIntersect = false;
				}
			}

			return doIntersect;
		}

		



		// 추가 22.3.9 [v1.4.0]
		//Pin에 의한 가중치 업데이트
		//테스트 모드가 아닌 경우의 업데이트 (가중치 계산만 한다.
		public void UpdatePinDefaultMode(apMeshPin curSelectedPin)
		{
			int nVert = _vertexData != null ? _vertexData.Count : 0;
			if(nVert == 0)
			{
				return;
			}

			int nVertWeight = _pinGroup._vertWeights != null ? _pinGroup._vertWeights.Count : 0;

			if(nVertWeight != nVert)
			{	
				//다시 동기화를 한다.
				_pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
				nVertWeight = _pinGroup._vertWeights != null ? _pinGroup._vertWeights.Count : 0;
			}

			apVertex curVert = null;
			apMeshPinVertWeight curWeight = null;
			int nPins = _pinGroup != null ? _pinGroup.NumPins : 0;

			if(nPins == 0)
			{
				//핀이 없다면
				//원래의 위치를 그대로 이용
				for (int iVert = 0; iVert < nVert; iVert++)
				{
					curVert = _vertexData[iVert];
					curVert._pos_PinTest = curVert._pos;//현재는 사용 안하지만 Test 모드에서 사용할 좌표도 적용
					curVert._pinWeightRatio = 0.0f;
				}
			}
			else
			{
				//핀이 있다면
				//여기서 Pin과 Curve의 모든 Default World Matrix가 계산된다.
				_pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.Update_Default);

				//계산을 하자
				for (int iVert = 0; iVert < nVert; iVert++)
				{
					curVert = _vertexData[iVert];
					curVert._pinWeightRatio = 0.0f;

					if(curSelectedPin == null)
					{
						continue;
					}

					curWeight = _pinGroup._vertWeights[iVert];
					if(curWeight._nPairs == 0)
					{
						continue;
					}
					else
					{
						//가중치를 계산하자
						apMeshPinVertWeight.VertPinPair curPair = null;

						for (int iPair = 0; iPair < curWeight._nPairs; iPair++)
						{
							curPair = curWeight._vertPinPairs[iPair];

							//계산은
							//Weight * (Matrix World * DefaultMatrix-1 * Vert_Def)
							//즉 Pin 또는 Curve의 World Matrix를 가져와야 한다.
							if (!curPair._isCurveWeighted)
							{
								//현재 선택된 핀이라면 GUI 표시를 위해 가중치 표시
								if (curPair._linkedPin == curSelectedPin)
								{
									curVert._pinWeightRatio += curPair._weight * curWeight._totalWeight;
								}
							}
							else
							{
								//커브중 한쪽 Pin이
								//현재 선택된 핀이라면 GUI 표시를 위해 가중치 표시
								//if(curPair._linkedPin == null) { Debug.Log("curPair._linkedPin == null"); }
								//else if(curPair._linkedPin._nextCurve == null) { Debug.Log("curPair._linkedPin._nextCurve == null"); }
								//else if(curPair._linkedPin._nextCurve._prevPin == null) { Debug.Log("curPair._linkedPin._nextCurve._prevPin == null"); }

								if (curPair._linkedPin._nextCurve._prevPin == curSelectedPin)
								{
									curVert._pinWeightRatio += curPair._weight * curWeight._totalWeight * (1.0f - curPair._curveLerp);
								}
								else if (curPair._linkedPin._nextCurve._nextPin == curSelectedPin)
								{
									curVert._pinWeightRatio += curPair._weight * curWeight._totalWeight * curPair._curveLerp;
								}
							}

						}
					}
				}
			}
		}


		//Pin 편집 모드 중 [Test] 모드에서 호출된다. (에디터용 함수)
		public void UpdatePinTestMode(apMeshPin curSelectedPin)
		{
			int nVert = _vertexData != null ? _vertexData.Count : 0;
			if(nVert == 0)
			{
				return;
			}

			int nVertWeight = _pinGroup._vertWeights != null ? _pinGroup._vertWeights.Count : 0;

			if(nVertWeight != nVert)
			{	
				//다시 동기화를 한다.
				//_pinGroup.CalculateVertWeightAll();
				_pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
				nVertWeight = _pinGroup._vertWeights != null ? _pinGroup._vertWeights.Count : 0;
			}

			apVertex curVert = null;
			apMeshPinVertWeight curWeight = null;
			int nPins = _pinGroup != null ? _pinGroup.NumPins : 0;

			if(nPins == 0)
			{
				//핀이 없다면
				//원래의 위치를 그대로 이용
				for (int iVert = 0; iVert < nVert; iVert++)
				{
					curVert = _vertexData[iVert];
					curVert._pos_PinTest = curVert._pos;
					curVert._pinWeightRatio = 0.0f;
				}
			}
			else
			{
				//핀이 있다면
				//여기서 Pin과 Curve의 모든 Test_World Matrix가 계산된다.
				//_pinGroup.Test_UpdateCurves();//Test의 계산을 하자
				_pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.Update_Test);

				//계산을 하자
				for (int iVert = 0; iVert < nVert; iVert++)
				{
					curVert = _vertexData[iVert];
					curVert._pinWeightRatio = 0.0f;

					curWeight = _pinGroup._vertWeights[iVert];
					//TODO : 여기서 Weight를 다시 연산하여 Vert의 Test 변수 (Ratio와 위치)에 대입할 것
					if(curWeight._nPairs == 0)
					{
						curVert._pos_PinTest = curVert._pos;
					}
					else
					{
						//가중치를 이용하여 복사하자
						Vector2 sumWorldPos = Vector2.zero;
						Vector2 curWorldPos = Vector2.zero;
						apMatrix3x3 ver2WorldMatrix;
						apMeshPinVertWeight.VertPinPair curPair = null;
						
						for (int iPair = 0; iPair < curWeight._nPairs; iPair++)
						{
							curPair = curWeight._vertPinPairs[iPair];

							//계산은
							//Weight * (Matrix World * DefaultMatrix-1 * Vert_Def)
							//즉 Pin 또는 Curve의 World Matrix를 가져와야 한다.
							if(!curPair._isCurveWeighted)
							{
								//연결된 Pin의 World
								//curWorldPos = curPair._linkedPin._testVert2MeshMatrix.MultiplyPoint(curVert._pos);
								curWorldPos = curPair._linkedPin.TmpMultiplyVertPos(apMeshPin.TMP_VAR_TYPE.MeshTest, ref curVert._pos);

								//현재 선택된 핀이라면 GUI 표시를 위해 가중치 표시
								if(curPair._linkedPin == curSelectedPin)
								{
									curVert._pinWeightRatio += curPair._weight * curWeight._totalWeight;
								}

								//if(curPair._weight > 0.0f)
								//{
								//	Debug.Log("단일 Pin 연결 (가중치 : " + (curPair._weight + " / " + curWeight._totalWeight) + ")\n"
								//		+ "변환 행렬(Pin) : " + curPair._linkedPin._testVert2MeshMatrix + "\n"
								//		+ "위치 변환 : " + curVert._pos + " > " + curWorldPos);
								//}
							}
							else
							{
								//연결된 Curve의 World
								//ver2WorldMatrix = curPair._curveDefaultMatrix_Inv;
								ver2WorldMatrix = apMatrix3x3.identity;
								
								//apMatrix3x3 curveDefaultMatrix = curPair._linkedPin._nextCurve.GetCurveMatrix_Default(curPair._curveLerp);
								//apMatrix3x3 curveDefaultMatrix = curPair._curveDefaultMatrix;


								apMatrix3x3 curveDefaultMatrix_Inv = curPair._curveDefaultMatrix_Inv;
								apMatrix3x3 curveCurMatrix = curPair._linkedPin._nextCurve.GetCurveMatrix_Test(apMeshPin.TMP_VAR_TYPE.MeshTest, curPair._curveLerp);
								
								//ver2WorldMatrix = curveCurMatrix * curveDefaultMatrix.inverse;
								
								
								ver2WorldMatrix = curveCurMatrix * curveDefaultMatrix_Inv;
								curWorldPos = ver2WorldMatrix.MultiplyPoint(curVert._pos);

								//커브중 한쪽 Pin이
								//현재 선택된 핀이라면 GUI 표시를 위해 가중치 표시
								if(curPair._linkedPin._nextCurve._prevPin == curSelectedPin)
								{
									curVert._pinWeightRatio += curPair._weight * curWeight._totalWeight * (1.0f - curPair._curveLerp);
								}
								else if(curPair._linkedPin._nextCurve._nextPin == curSelectedPin)
								{
									curVert._pinWeightRatio += curPair._weight * curWeight._totalWeight * curPair._curveLerp;
								}

								//if(curPair._weight > 0.0f)
								//{
								//	Debug.Log("커브 연결 (가중치 : " + (curPair._weight + " / " + curWeight._totalWeight) + ") | Lerp : " + curPair._curveLerp + "\n"
								//		+ "변환 행렬(Curve) : \n" + ver2WorldMatrix + "\n"
								//		+ "> 기본 행렬 : \n" + curPair._curveDefaultMatrix + "\n"
								//		+ "> 재계산된 기본 행렬 : \n" + curveDefaultMatrix + "\n"
								//		+ "> 기본 역행렬 : \n" + curPair._curveDefaultMatrix_Inv + "\n"
								//		+ "> 현재 행렬 : \n" + curveCurMatrix + "\n"
								//		+ "위치 변환 : " + curVert._pos + " > " + curWorldPos);
								//}
							}
							sumWorldPos += curWorldPos * curPair._weight;//가중치를 곱해서 World좌표 계산하기

							
						}

						//Total Weight를 이용하여 선형 보간
						curVert._pos_PinTest = (curVert._pos * (1.0f - curWeight._totalWeight)) + (sumWorldPos * curWeight._totalWeight);


						//Debug.LogWarning(">> Pos PinTest : " + curVert._pos_PinTest + " (Weight : " + curWeight._totalWeight + ")");
					}
				}
			}
		}


		/// <summary>
		/// 추가 22.5.4 : 버텍스들의 간격의 최대치를 구한다. (Pin 생성용)
		/// </summary>
		/// <returns></returns>
		public float GetVerticesMaxRange()
		{
			float maxSize = 50.0f;//일단 최소치
			if(_isPSDParsed)
			{
				float size_X = Mathf.Abs(_atlasFromPSD_RB.x - _atlasFromPSD_LT.x);
				float size_Y = Mathf.Abs(_atlasFromPSD_RB.y - _atlasFromPSD_LT.y);

				if(maxSize < size_X)
				{
					maxSize = size_X;
				}
				if(maxSize < size_Y)
				{
					maxSize = size_Y;
				}

				//충분히 크다면 이걸로 결정
				if(maxSize > 200.0f)
				{
					return maxSize;
				}
			}

			int nVerts = _vertexData != null ? _vertexData.Count : 0;
			if(nVerts <= 1)
			{
				return maxSize;
			}

			//첫번째 위치를 정하자
			Vector2 minPos = _vertexData[0]._pos;
			Vector2 maxPos = minPos;

			apVertex curVert = null;
			for (int i = 1; i < nVerts; i++)
			{
				curVert = _vertexData[i];

				//최대 최소를 지정하자
				minPos.x = Mathf.Min(curVert._pos.x, minPos.x);
				minPos.y = Mathf.Min(curVert._pos.y, minPos.y);

				maxPos.x = Mathf.Max(curVert._pos.x, maxPos.x);
				maxPos.y = Mathf.Max(curVert._pos.y, maxPos.y);
			}

			float rangeX = maxPos.x - minPos.x;
			float rangeY = maxPos.y - minPos.y;
			
			if(maxSize < rangeX)
			{
				maxSize = rangeX;
			}

			if(maxSize < rangeY)
			{
				maxSize = rangeY;
			}

			return maxSize;
		}

		

#endif

		//---------------------------------------------------------------
		// 변경된 내용
		// TextureData를 직접 참조하면 안된다.
		public int LinkedTextureDataID
		{
			get
			{
				if(_textureData == null)
				{
					return -1;
				}
				return _textureData._uniqueID;
			}
		}

		public apTextureData LinkedTextureData
		{
			get
			{
				return _textureData_Linked;
			}
		}

		/// <summary>
		/// TextureData가 연결이 되었는가
		/// 연결이 안되었다면 false가 된다.
		/// </summary>
		public bool IsTextureDataLinked
		{
			get
			{	
				if(_textureData_Linked == null)
				{
					//연결된 것이 없다.
					return false;
				}
				int linkedTextureID = LinkedTextureDataID;
				if(_textureData_Linked._uniqueID != linkedTextureID)
				{
					//연결된게 있지만 ID가 다르다.
					return false;
				}

				if(linkedTextureID < 0)
				{
					//연결이 안되었다.
					return false;
				}
				//연결이 잘 되었다.
				return true;
			}
		}


		public void SetTextureData(apTextureData textureData)
		{
			_textureData_Linked = textureData;//<<이건 바로 링크

			

			//이건 ID만 이용한다.
			if(_textureData == null)
			{
				_textureData = new apTextureData();
			}

			//if (textureData == null)
			//{
			//	Debug.Log(">> Null 텍스쳐가 대입되었다 : ID " + _textureData._uniqueID);
			//}

			_textureData._uniqueID = -1;
			if(textureData != null)
			{
				_textureData._uniqueID = textureData._uniqueID;
			}
		}






		//-------------------------------------------------------------------------------
#if UNITY_EDITOR
		//Undo 관련 함수와 이벤트
		public void OnUndoPerformed()
		{
			//Undo를 하면 Edge의 Link가 풀리는 문제가 있다.
			LinkEdgeAndVertex();

			//일단 테스트를 하자
			//Debug.LogWarning("---- Mesh Undo [ " + _name + " ]----");
			//Debug.LogWarning("Vertex : " + _vertexData.Count);
			//Debug.LogWarning("IndexBuffer : " + _indexBuffer.Count);
			//Debug.LogWarning("Edges : " + _edges.Count);
			//Debug.LogWarning("Polygons : " + _polygons.Count);

			//Debug.Log("Edges..");
			//apMeshEdge curEdge = null;
			//string strData = "";
			//for (int i = 0; i < _edges.Count; i++)
			//{
			//	curEdge = _edges[i];
			//	strData = "> [" + i + "] ";
			//	strData += curEdge._vertID_1 + (curEdge._vert1 != null ? " (Exist)" : " (None)");
			//	strData += " ~ ";
			//	strData += curEdge._vertID_2 + (curEdge._vert2 != null ? " (Exist)" : " (None)");
			//	Debug.Log(strData);
			//}
		}
#endif


	}

}