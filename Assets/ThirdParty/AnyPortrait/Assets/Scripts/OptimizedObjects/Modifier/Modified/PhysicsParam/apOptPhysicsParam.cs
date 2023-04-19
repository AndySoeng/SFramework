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


	[Serializable]
	public class apOptPhysicsMeshParam
	{
		//Members
		//-----------------------------------------------------
		/// <summary>기존 위치에서 이동하는 것을 제한하는가 (기본 False)</summary>
		[SerializeField]
		public bool _isRestrictMoveRange = false;

		/// <summary>절대값으로서 Vertex가 움직일 수 있는 범위. 이게 0이면 아예 움직이질 않는다.</summary>
		[SerializeField]
		public float _moveRange = 0.0f;


		/// <summary>[장력]에 의한 신축의 길이를 제한하는가 (기본 False)</summary>
		[SerializeField]
		public bool _isRestrictStretchRange = false;

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

		//[SerializeField]
		//public float _optPhysicScale = 1.0f;

		//추가
		//같은 그룹끼리 velocity가 비슷해지는 "점성"을 추가한다.
		[SerializeField]
		public float _viscosity = 0.0f;

		//복원력
		//원래 모양으로 돌아가려는 힘
		[SerializeField]
		public float _restoring = 1.0f;

		// Init
		//-----------------------------------------------------------------
		public apOptPhysicsMeshParam()
		{
			//Clear();
		}

		public void Bake(apPhysicsMeshParam srcPhysicsParam)
		{

			_isRestrictMoveRange = srcPhysicsParam._isRestrictMoveRange;
			_isRestrictStretchRange = srcPhysicsParam._isRestrictStretchRange;

			_stretchRangeRatio_Max = srcPhysicsParam._stretchRangeRatio_Max;//<<MinMax+ MoveRange

			_moveRange = srcPhysicsParam._moveRange;

			_stretchK = srcPhysicsParam._stretchK;

			_inertiaK = srcPhysicsParam._inertiaK;
			_damping = srcPhysicsParam._damping;


			_mass = srcPhysicsParam._mass;

			switch (srcPhysicsParam._gravityParamType)
			{
				case apPhysicsMeshParam.ExternalParamType.Constant:
					_gravityParamType = ExternalParamType.Constant;
					break;

				case apPhysicsMeshParam.ExternalParamType.ControlParam:
					_gravityParamType = ExternalParamType.ControlParam;
					break;

			}
			//_optPhysicScale = physicScale;
			_gravityConstValue = srcPhysicsParam._gravityConstValue;

			_gravityControlParamID = srcPhysicsParam._gravityControlParamID;
			_gravityControlParam = null;

			switch (srcPhysicsParam._windParamType)
			{
				case apPhysicsMeshParam.ExternalParamType.Constant:
					_windParamType = ExternalParamType.Constant;
					break;

				case apPhysicsMeshParam.ExternalParamType.ControlParam:
					_windParamType = ExternalParamType.ControlParam;
					break;
			}

			_windConstValue = srcPhysicsParam._windConstValue;
			_windControlParamID = srcPhysicsParam._windControlParamID;

			_windControlParam = null;//<<Link해야한다.

			_windRandomRange = srcPhysicsParam._windRandomRange;
			_airDrag = srcPhysicsParam._airDrag;
			_windRandItp = Vector2.zero;
			_windRandItpLength = new Vector2(2.7f, 3.1f);

			_viscosity = srcPhysicsParam._viscosity;
			_restoring = srcPhysicsParam._restoring;
		}

		//public void Clear()
		//{
		//	_stretchRange = 0.0f;
		//	_stretchK = 0.0f;
		//	//_bendRange = 0.0f;
		//	//_bendK = 0.0f;

		//	_damping = 0.0f;
		//	_mass = 100.0f;

		//	_gravityParamType = ExternalParamType.Constant;
		//	_gravityConstValue = Vector2.zero;
		//	_gravityControlParamID = -1;
		//	_gravityControlParam = null;

		//	_windParamType = ExternalParamType.Constant;
		//	_windConstValue = Vector2.zero;
		//	_windControlParamID = -1;
		//	_windControlParam = null;//<<Link해야한다.
		//	_windRandomRange = Vector2.zero;

		//	_airDrag = 0.0f;

		//	_viscosity = 0.0f;
		//	_restoring = 1.0f;
		//}

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
	}






	/// <summary>
	/// apModifiedVertexWeight에 들어가는 파라미터의 Opt 버전
	/// isEnabled, Weight는 OptModVertWeight에서 정의되어있다.
	/// 계산을 위해서 "연결된 이웃한 Vertex"들의 정보를 가지고 있다. (Link 필요)
	/// </summary>
	[Serializable]
	public class apOptPhysicsVertParam
	{
		// Members
		//-------------------------------------
		/// <summary>
		/// Enabled가 False Vertex 중에서
		/// </summary>
		public bool _isConstraint = false;
		public bool _isMain = false;


		//[NonSerialized]
		//private apOptModifiedVertexWeight _parentModVertWeight = null;

		//[NonSerialized]
		//private apOptModifiedMesh _parentModMesh = null;

		//연결된 Vertex와 거리에 다른 Weight를 저장하자 ((1 - 길이 / 전체 길이)의 Normalize)


		[Serializable]
		public class OptLinkedVertex
		{
			//19.5.23 : 사용되지 않는 변수
			//[SerializeField]
			//public int _vertUniqueID = -1;

			[SerializeField]
			public int _vertIndex = -1;

			[NonSerialized]
			public apOptRenderVertex _vertex = null;

			[NonSerialized]
			public apOptModifiedVertexWeight _modVertWeight = null;

			//19.5.23 : 사용되지 않는 변수
			//[SerializeField]
			//public Vector2 _deltaPosLocal = Vector2.zero;

			//19.5.23 : 사용되지 않는 변수
			//[SerializeField]
			//public float _distLocal = 0.0f;

			[SerializeField]
			public float _distWeight = 0.0f;

			//19.5.23 : 사용되지 않는 변수
			//[SerializeField]
			//public int _level = -1;

			[NonSerialized]
			public Vector2 _deltaPosToTarget_NoMod = Vector2.zero;

			public OptLinkedVertex(apPhysicsVertParam.LinkedVertex srcLinkedVertex)
			{
				//_vertex = vertex;
				_vertex = null;
				//_vertUniqueID = srcLinkedVertex._vertUniqueID;//>>19.5.23 : 삭제 (불필요)
				_vertIndex = srcLinkedVertex._vertex._index;

				_modVertWeight = null;

				//_deltaPosLocal = srcLinkedVertex._deltaPosLocalLinkToTarget;//>>19.5.23 : 삭제 (불필요)
				//_distLocal = srcLinkedVertex._distLocal;//>>19.5.23 : 삭제 (불필요)
				_distWeight = srcLinkedVertex._distWeight;

				//_level = srcLinkedVertex._level;//>>19.5.23 : 삭제 (불필요)
			}





			public void Link(apOptRenderVertex linkedVertex, apOptModifiedVertexWeight linkedModVertWeight)
			{
				_vertex = linkedVertex;
				_modVertWeight = linkedModVertWeight;
			}


		}

		[SerializeField]
		public List<OptLinkedVertex> _linkedVertices = new List<OptLinkedVertex>();


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
		public apOptPhysicsVertParam()
		{
			Clear();
		}

		public void Clear()
		{
			_isConstraint = false;
			_isMain = false;
			if (_linkedVertices == null)
			{
				_linkedVertices = new List<OptLinkedVertex>();
			}
			_linkedVertices.Clear();
		}

		public void Bake(apPhysicsVertParam srcVertParam)
		{
			_isConstraint = srcVertParam._isConstraint;
			_isMain = srcVertParam._isMain;

			_linkedVertices.Clear();
			for (int i = 0; i < srcVertParam._linkedVertices.Count; i++)
			{
				apPhysicsVertParam.LinkedVertex srcLinkedVert = srcVertParam._linkedVertices[i];
				_linkedVertices.Add(new OptLinkedVertex(srcLinkedVert));//<<Add + Bake
			}

			_viscosityGroupID = srcVertParam._viscosityGroupID;
		}

		/// <summary>
		/// Linked Vertex의 객체를 Link한다.
		/// </summary>
		/// <param name="parentModMesh"></param>
		public void Link(apOptModifiedMesh parentModMesh, apOptModifiedVertexWeight parentModVertWeight)
		{
			//_parentModMesh = parentModMesh;
			//_parentModVertWeight = parentModVertWeight;

			if (_linkedVertices == null)
			{
				_linkedVertices = new List<OptLinkedVertex>();
			}


			apOptMesh mesh = parentModMesh._targetMesh;

			//이미 Bake 되었으므로 바로 Link하면 된다.
			for (int i = 0; i < _linkedVertices.Count; i++)
			{
				OptLinkedVertex linkedVert = _linkedVertices[i];
				apOptRenderVertex renderVert = mesh.RenderVertices[linkedVert._vertIndex];
				apOptModifiedVertexWeight linkVertWeight = parentModMesh._vertWeights[linkedVert._vertIndex];
				linkedVert.Link(renderVert, linkVertWeight);
			}
		}

		
	}

}