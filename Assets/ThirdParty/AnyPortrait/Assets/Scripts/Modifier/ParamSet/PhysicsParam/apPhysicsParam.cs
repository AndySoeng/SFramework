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

	// 코드의 위쪽은 MeshParam. 아래에는 VertParam의 두개의 클래스가 정의되어있다.

	[Serializable]
	public class apPhysicsMeshParam
	{
		// Members
		//-------------------------------------
		/// <summary>기존 위치에서 이동하는 것을 제한하는가 (기본 False)</summary>
		[SerializeField]
		public bool _isRestrictMoveRange = false;

		/// <summary>절대값으로서 Vertex가 움직일 수 있는 범위. 이게 0이면 아예 움직이질 않는다.</summary>
		[SerializeField]
		public float _moveRange = 0.0f;


		/// <summary>[장력]에 의한 신축의 길이를 제한하는가 (기본 False)</summary>
		[SerializeField]
		public bool _isRestrictStretchRange = false;

		//이게 켜져있으면 너무 제한적으로 움직인다.
		///// <summary>장력에 의해 길이가 압축 가능한 비율. 0이면 압축이 되지 않으며 1인 경우 최대 압축.</summary>
		//[SerializeField]
		//public float _stretchRangeRatio_Min = 0.0f;

		/// <summary>장력에 의해 길이가 늘어나는 비율. 0이면 늘어나지 않으며 1이면 기존 길이 100% 만큼 늘어난다. </summary>
		[SerializeField]
		public float _stretchRangeRatio_Max = 0.0f;

		/// <summary>K상수 - Strech (0~1)</summary>
		[SerializeField]
		public float _stretchK = 0.0f;




		[SerializeField]
		public float _inertiaK = 0.0f;


		/// <summary>너무 낮은 속도의 움직임이 계산된 경우 0으로 만든다.</summary>
		[SerializeField]
		public float _damping = 0.0f;


		/// <summary>질량값. Vertex의 질량은 (전체 질량 / Vertex 개수)로 계산된다.</summary>
		[SerializeField]
		public float _mass = 100.0f;

		//외부 변수 : 중력과 바람
		public enum ExternalParamType
		{
			/// <summary>고정된 값을 사용한다.</summary>
			Constant = 0,
			/// <summary>Control Param을 변수값으로 사용한다. (컨트롤 가능)</summary>
			ControlParam = 1,
		}

		/// <summary>중력 변수 타입</summary>
		[SerializeField]
		public ExternalParamType _gravityParamType = ExternalParamType.Constant;

		/// <summary>중력 값 (고정)</summary>
		[SerializeField]
		public Vector2 _gravityConstValue = Vector2.zero;

		/// <summary>중력 값에 연결된 Control Param의 ID</summary>
		[SerializeField]
		public int _gravityControlParamID = -1;

		/// <summary>중력 값에 연결된 Control Param</summary>
		[NonSerialized]
		public apControlParam _gravityControlParam = null;//<<Link해야한다.

		[SerializeField]
		public ExternalParamType _windParamType = ExternalParamType.Constant;

		[SerializeField]
		public Vector2 _windConstValue = Vector2.zero;

		[SerializeField]
		public int _windControlParamID = -1;

		[NonSerialized]
		public apControlParam _windControlParam = null;//<<Link해야한다.

		[SerializeField]
		public Vector2 _windRandomRange = Vector2.zero;

		/// <summary>공기 저항 계수</summary>
		[SerializeField]
		public float _airDrag = 0.0f;


		[NonSerialized]
		private Vector2 _windRandItp = Vector2.zero;

		[NonSerialized]
		private Vector2 _windRandItpLength = new Vector2(2.7f, 3.1f);

		//추가
		//같은 그룹끼리 velocity가 비슷해지는 "점성"을 추가한다.
		[SerializeField]
		public float _viscosity = 0.0f;

		//복원력
		//원래 모양으로 돌아가려는 힘
		[SerializeField]
		public float _restoring = 1.0f;


		//에디터에서 편하게 보기 위한 프리셋 옵션
		[SerializeField]
		public int _presetID = -1;



		// Init
		//-------------------------------------
		public apPhysicsMeshParam()
		{
			Clear();
		}

		public void Clear()
		{
			_isRestrictStretchRange = false;
			_isRestrictMoveRange = false;
			_moveRange = 0.0f;
			//_stretchRangeRatio_Min = 0.0f;
			_stretchRangeRatio_Max = 0.0f;
			_stretchK = 0.0f;
			//_bendRange = 0.0f;
			//_bendK = 0.0f;

			_damping = 0.0f;
			_mass = 100.0f;

			_gravityParamType = ExternalParamType.Constant;
			_gravityConstValue = Vector2.zero;
			_gravityControlParamID = -1;
			_gravityControlParam = null;

			_windParamType = ExternalParamType.Constant;
			_windConstValue = Vector2.zero;
			_windControlParamID = -1;
			_windControlParam = null;//<<Link해야한다.
			_windRandomRange = Vector2.zero;

			_airDrag = 0.0f;

			_viscosity = 0.0f;
			_restoring = 1.0f;
		}

		public void Link(apPortrait portrait)
		{
			//ControlParam을 연결하자
			//1. Gravity
			if (_gravityControlParamID > 0)
			{
				_gravityControlParam = portrait._controller.FindParam(_gravityControlParamID);
				if (_gravityControlParam == null)
				{
					_gravityControlParamID = -1;
				}
			}
			else
			{
				_gravityControlParam = null;
			}

			//2. Wind
			if (_windControlParamID > 0)
			{
				_windControlParam = portrait._controller.FindParam(_windControlParamID);
				if (_windControlParam == null)
				{
					_windControlParamID = -1;
				}
			}
			else
			{
				_windControlParam = null;
			}
		}

		// Get
		//----------------------------------------
		public Vector2 GetGravityAcc()
		{
			if (_gravityParamType == ExternalParamType.Constant)
			{
				return _gravityConstValue;
			}
			else
			{
				if (_gravityControlParam == null)
				{
					return Vector2.zero;
				}
				return _gravityControlParam._vec2_Cur;
			}
		}

		public Vector2 GetWindAcc(float tDelta)
		{
			_windRandItp.x += tDelta;
			_windRandItp.y += tDelta;

			if (_windRandItp.x > _windRandItpLength.x)
			{
				_windRandItp.x -= _windRandItpLength.x;
			}

			if (_windRandItp.y > _windRandItpLength.y)
			{
				_windRandItp.y -= _windRandItpLength.y;
			}

			if (_windParamType == ExternalParamType.Constant)
			{
				return new Vector2(
					_windConstValue.x + (Mathf.Sin(2.0f * Mathf.PI * _windRandItp.x / _windRandItpLength.x) * _windRandomRange.x),
					_windConstValue.y + (Mathf.Sin(2.0f * Mathf.PI * _windRandItp.y / _windRandItpLength.y) * _windRandomRange.y)
					);
			}
			else
			{
				if (_windControlParam == null)
				{
					return Vector2.zero;
				}
				//return _windControlParam._vec2_Cur;
				return new Vector2(
					_windControlParam._vec2_Cur.x + (Mathf.Sin(2.0f * Mathf.PI * _windRandItp.x / _windRandItpLength.x) * _windRandomRange.x),
					_windControlParam._vec2_Cur.y + (Mathf.Sin(2.0f * Mathf.PI * _windRandItp.y / _windRandItpLength.y) * _windRandomRange.y)
					);
			}
		}

		// Duplicate
		//------------------------------------------------------------------------
		public void CopyFromSrc(apPhysicsMeshParam srcMeshParam)
		{
			_isRestrictMoveRange = srcMeshParam._isRestrictMoveRange;
			_moveRange = srcMeshParam._moveRange;
			_isRestrictStretchRange = srcMeshParam._isRestrictStretchRange;
			_stretchRangeRatio_Max = srcMeshParam._stretchRangeRatio_Max;
			
			_stretchK = srcMeshParam._stretchK;
			_inertiaK = srcMeshParam._inertiaK;

			_damping = srcMeshParam._damping;
			_mass = srcMeshParam._mass;

			_gravityParamType = srcMeshParam._gravityParamType;
			_gravityConstValue = srcMeshParam._gravityConstValue;
			_gravityControlParamID = srcMeshParam._gravityControlParamID;
			_gravityControlParam = srcMeshParam._gravityControlParam;

			_windParamType = srcMeshParam._windParamType;
			_windConstValue = srcMeshParam._windConstValue;
			_windControlParamID = srcMeshParam._windControlParamID;
			_windControlParam = srcMeshParam._windControlParam;
			_windRandomRange = srcMeshParam._windRandomRange;

			_airDrag = srcMeshParam._airDrag;
			_viscosity = srcMeshParam._viscosity;
			_restoring = srcMeshParam._restoring;
			_presetID = srcMeshParam._presetID;
		}
	}

	/// <summary>
	/// apModifiedVertexWeight에 들어가는 파라미터
	/// isEnabled, Weight는 ModVertWeight에서 정의되어있다.
	/// 계산을 위해서 "연결된 이웃한 Vertex"들의 정보를 가지고 있다. (Link 필요)
	/// </summary>
	[Serializable]
	public class apPhysicsVertParam
	{
		// Members
		//-------------------------------------
		/// <summary>
		/// Enabled가 False Vertex 중에서
		/// </summary>
		public bool _isConstraint = false;
		public bool _isMain = false;


		[NonSerialized]
		private apModifiedVertexWeight _parentModVertWeight = null;

		[NonSerialized]
		private apModifiedMesh _parentModMesh = null;

		//연결된 Vertex와 거리에 다른 Weight를 저장하자 ((1 - 길이 / 전체 길이)의 Normalize)


		[Serializable]
		public class LinkedVertex
		{
			[SerializeField]
			public int _vertUniqueID = -1;

			[NonSerialized]
			public apVertex _vertex = null;

			[NonSerialized]
			public apModifiedVertexWeight _modVertWeight = null;

			[SerializeField]
			public Vector2 _deltaPosLocalLinkToTarget = Vector2.zero;

			[SerializeField]
			public float _distLocal = 0.0f;

			[SerializeField]
			public float _distWeight = 0.0f;

			[SerializeField]
			public int _level = -1;


			[NonSerialized]
			public Vector2 _deltaPosToTarget_NoMod = Vector2.zero;


			/// <summary>
			/// 백어	용 생성자. 코드에서 사용하지 말것
			/// </summary>
			public LinkedVertex()
			{

			}

			public LinkedVertex(apVertex vertex, int level)
			{
				_vertex = vertex;
				_vertUniqueID = vertex._uniqueID;

				_modVertWeight = null;

				_deltaPosLocalLinkToTarget = Vector2.zero;
				_distLocal = 0.0f;
				_distWeight = 0.0f;

				_level = level;
			}

			public void Link(apVertex linkedVertex, apModifiedVertexWeight linkedModVertWeight)
			{
				_vertex = linkedVertex;
				_modVertWeight = linkedModVertWeight;
			}

			public void SetDistance(Vector2 deltaPosLinkToTarget)
			{
				_deltaPosLocalLinkToTarget = deltaPosLinkToTarget;
				_distLocal = _deltaPosLocalLinkToTarget.magnitude;
			}

			public void SetWeight(float distWeight)
			{
				_distWeight = distWeight;
			}
		}

		[SerializeField]
		public List<LinkedVertex> _linkedVertices = new List<LinkedVertex>();


		//추가
		//같은 그룹 ID를 가진 Linked Vertex와 Velocity가 유사하게 바뀌는 "점성" 개념이 추가되었다.
		//그룹 ID를 추가한다.
		//플래그 개념으로 10개 까지 지원한다.
		//0 : None
		//1, 2, 4, 8, 16, 32...2^16
		[SerializeField]
		public int _viscosityGroupID = 0;

		// Init
		//-------------------------------------
		public apPhysicsVertParam()
		{
			Clear();
		}

		public void Clear()
		{
			_isConstraint = false;
			_isMain = false;
			if (_linkedVertices == null)
			{
				_linkedVertices = new List<LinkedVertex>();
			}
			_linkedVertices.Clear();
		}

		/// <summary>
		/// Linked Vertex의 객체를 Link한다.
		/// </summary>
		/// <param name="parentModMesh"></param>
		public void Link(apModifiedMesh parentModMesh, apModifiedVertexWeight parentModVertWeight)
		{
			_parentModMesh = parentModMesh;
			_parentModVertWeight = parentModVertWeight;

			if (_linkedVertices == null)
			{
				_linkedVertices = new List<LinkedVertex>();
			}
			if (parentModMesh._transform_Mesh == null || parentModMesh._transform_Mesh._mesh == null)
			{
				Debug.LogError("Physics Param Link Error : MeshTransform is Null of Mesh is Null");
				return;
			}

			//????
			//이게 왜 Link에 있지??
			//이거 많이 느림
			//데이터 저장 후 ID를 통해서 바로 Link해야하는데 매번 조회 후 생성+연결하려고 함.
			//이 부분을 고쳐서 Physic.LinkedVertex를 미리 생성하고 여기서는 바로 연결해야한다. 로딩 느릴수밖에 없네..
			//TODO
			//...대체 방법이 없는데요..


			apMesh mesh = parentModMesh._transform_Mesh._mesh;
			//int maxVertLevel = 3;
			int maxVertLevel = 1;
			//연결된 apVertex를 받아오자 (최대 3레벨)
			//수정 > 1레벨로도 충분할 듯
			List<apMesh.LinkedVertexResult> linkedVerticesOnMesh = mesh.GetLinkedVertex(_parentModVertWeight._vertex, maxVertLevel);

			//두개의 리스트를 비교하여
			//- 같으면 단순 Link
			//- 없으면 추가해서 Link
			//- 연결되지 않았으면 제외

			//+Contraint 체크를 위해서, 1-Level 을 따로 분류 (또는 Level 값을 넣자)


			//1. 추가할 것을 체크하자
			apMesh.LinkedVertexResult srcVert = null;
			apModifiedVertexWeight modVertWeight = null;
			for (int iSrcVert = 0; iSrcVert < linkedVerticesOnMesh.Count; iSrcVert++)
			{
				srcVert = linkedVerticesOnMesh[iSrcVert];
				modVertWeight = _parentModMesh.GetVertexWeight(srcVert._vertex);

				LinkedVertex linkedVert = _linkedVertices.Find(delegate (LinkedVertex a)
				{
					return a._vertUniqueID == srcVert._vertex._uniqueID;
				});

				if (linkedVert == null)
				{
					//새로 추가해야하는 Linked Vert
					LinkedVertex newLinkedVert = new LinkedVertex(srcVert._vertex, srcVert._level);
					newLinkedVert.Link(srcVert._vertex, modVertWeight);


					_linkedVertices.Add(newLinkedVert);
				}
				else
				{
					//이미 추가되었으니 Link만 하자
					linkedVert.Link(srcVert._vertex, modVertWeight);
				}
			}

			//2. 이제 제거할 것을 체크하자
			//연결 안된건 지우자
			_linkedVertices.RemoveAll(delegate (LinkedVertex a)
			{
				if (a._vertex == null || a._modVertWeight == null)
				{
					return true;
				}
				if (!linkedVerticesOnMesh.Exists(delegate (apMesh.LinkedVertexResult b)
			 {
					return b._vertex == a._vertex;
				}))
				{
				//연결된게 아니다. 찾을 수 없다.
				return true;
				}

				return false;
			});

			//전체 길이를 체크하여 역 Weight를 걸자




			RefreshLinkedVertex();
		}

		/// <summary>
		/// Linked Vertex의 Dist, Weight을 갱신하자
		/// LinkedVertex는 Link에서 미리 만들자
		/// </summary>
		public void RefreshLinkedVertex()
		{
			if (_parentModMesh._transform_Mesh == null ||
				_parentModMesh._transform_Mesh._mesh == null ||
				_parentModVertWeight._vertex == null)
			{
				return;
			}

			float totalDist = 0.0f;
			for (int i = 0; i < _linkedVertices.Count; i++)
			{
				LinkedVertex linkVert = _linkedVertices[i];
				linkVert.SetDistance(_parentModVertWeight._vertex._pos - linkVert._vertex._pos);
				totalDist += linkVert._distLocal;
			}
			if (totalDist > 0.0f)
			{
				if (_linkedVertices.Count > 1)
				{
					float totalWeight = totalDist * (_linkedVertices.Count - 1);

					for (int i = 0; i < _linkedVertices.Count; i++)
					{
						LinkedVertex linkVert = _linkedVertices[i];

						linkVert.SetWeight((totalDist - linkVert._distLocal) / totalWeight);//Normalized된 Weight를 넣자
					}
				}
				else if (_linkedVertices.Count == 0)
				{
					_linkedVertices[0].SetWeight(1.0f);
				}
			}


		}


		// 복사하기
		public void CopyFromSrc(apPhysicsVertParam srcVertParam)
		{
			_isConstraint = srcVertParam._isConstraint;
			_isMain = srcVertParam._isMain;

			int nLinkedVertex = srcVertParam._linkedVertices == null ? 0 : srcVertParam._linkedVertices.Count;
			if(_linkedVertices == null)
			{
				_linkedVertices = new List<LinkedVertex>();
			}
			_linkedVertices.Clear();


			LinkedVertex srcLVert = null;
			LinkedVertex dstLVert = null;
			for (int iSrc = 0; iSrc < nLinkedVertex; iSrc++)
			{
				srcLVert = srcVertParam._linkedVertices[iSrc];
				if(srcLVert == null || srcLVert._vertex == null)
				{
					continue;
				}
				dstLVert = new LinkedVertex(srcLVert._vertex, srcLVert._level);
				dstLVert._deltaPosLocalLinkToTarget = srcLVert._deltaPosLocalLinkToTarget;
				dstLVert._distLocal = srcLVert._distLocal;
				dstLVert._distWeight = srcLVert._distWeight;

				_linkedVertices.Add(dstLVert);
			}

			_viscosityGroupID = srcVertParam._viscosityGroupID;
		}


	}

}