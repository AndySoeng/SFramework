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
	/// Modifier에 의해서 변동된 내역이 저장되는 클래스
	/// ParamSet이 키값이라면, 해당 키값에 해당하는 데이터이다.
	/// 키값 하나에 여러 대상에 대한 변동 내역이 저장되므로, 각각의 대상에 대해 작성된다.
	/// 이 클래스의 값을 이용하여 중간 보간을 한 뒤 Calculated 계열 인스턴스에게 값을 전달한다.
	/// </summary>

	[Serializable]
	public class apModifiedMesh
	{
		// Members
		//------------------------------------------
		public int _meshGroupUniqueID_Modifier = -1;


		// 적용되는 Child 객체
		public int _transformUniqueID = -1;
		public int _meshUniqueID = -1;
		//public int _boneUniqueID = -1;//<<이건 bone transform에 적용되는 것 (skinning에는 적용되지 않는다)

		public bool _isMeshTransform = true;



		/// <summary>ModMesh가 속한 Modifier의 MeshGroup</summary>
		[NonSerialized]
		public apMeshGroup _meshGroupOfModifier = null;

		/// <summary>Transform이 속한 MeshGroup (기본적으론 _meshGroup_ParentOfModifier와 동일하지만 하위 Transform인 경우 다르게 된다)</summary>
		[NonSerialized]
		public apMeshGroup _meshGroupOfTransform = null;

		[NonSerialized]
		public apRenderUnit _renderUnit = null;

		[NonSerialized]
		public apTransform_Mesh _transform_Mesh = null;

		[NonSerialized]
		public apTransform_MeshGroup _transform_MeshGroup = null;


		//추가
		//물리 파라미터
		[SerializeField]
		public bool _isUsePhysicParam = false;

		[SerializeField]
		private apPhysicsMeshParam _physicMeshParam = new apPhysicsMeshParam();

		public apPhysicsMeshParam PhysicParam
		{
			get
			{
				if (_isUsePhysicParam)
				{
					if (_physicMeshParam == null) { _physicMeshParam = new apPhysicsMeshParam(); }
					return _physicMeshParam;
				}
				return null;
			}
		}



		// 저장되는 값
		[Flags]
		public enum MOD_VALUE_TYPE
		{
			Unknown = 1,
			VertexPosList = 2,//Morph
			TransformMatrix = 4,
			Color = 8,
			BoneVertexWeightList = 16,//Bone Rigging Weight인 경우
			VertexWeightList_Physics = 32,// Physic / Volume인경우
			VertexWeightList_Volume = 64,// Physic / Volume인경우
			FFD = 128,//FFD 타입인 경우 ( 처리후에는 Vertex Pos 리스트가 된다.)
		}


		public MOD_VALUE_TYPE _modValueType = MOD_VALUE_TYPE.Unknown;

		//TODO : Bone > Modified Bone으로 대체한다.



		//추가
		//만약 이 ModMesh가 속한 Modifier의 MeshGroup에 속한 Transform이 아닌 
		//Child.. 또는 그 Child MeshGroup/Mesh Transform인 경우
		public bool _isRecursiveChildTransform = false;

		/// <summary>
		/// Modifier가 속한 MeshGroup이 아닌 다른 MeshGroup의 Transform인 경우(_isRecursiveChildTransform == true),
		/// 그때의 "원래 속한 Parent MeshGroup"의 ID
		/// </summary>
		public int _meshGroupUniqueID_Transform = -1;




		// Vertex Morph인 경우
		[SerializeField]
		public List<apModifiedVertex> _vertices = new List<apModifiedVertex>();

		//추가 22.3.20 [v1.4.0]
		[SerializeField]
		public List<apModifiedPin> _pins = new List<apModifiedPin>();

		
		// Mesh Transform / MeshGroup Transform 인 경우
		[SerializeField]
		public apMatrix _transformMatrix = new apMatrix();

		[SerializeField]
		public Color _meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		/// <summary>
		/// Mesh의 Visible 여부. False면 _meshColor의 알파를 강제로 0으로 둔다.
		/// True일땐 meshColor의 a를 그대로 사용한다.
		/// </summary>
		[SerializeField]
		public bool _isVisible = true;


		//BoneVertexWeightList 인 경우 (Rigging)
		[SerializeField]
		public List<apModifiedVertexRig> _vertRigs = new List<apModifiedVertexRig>();

		//Physics / FFD / Vertex Weight (FFD는 FFD Point와 Vertex Weight를 모두 가진다.)
		[SerializeField]
		public List<apModifiedVertexWeight> _vertWeights = new List<apModifiedVertexWeight>();


		//11.26 추가
		//일반적인 Transform/Color가 아닌 특별한 경우에 사용되는 값이다.
		//Depth 변화, 텍스쳐 변화 등이 포함되어 있다.
		//보간이 아예 안되며, "전환" 방식으로 계산된다.
		//따라서 "어느 가중치의 시점에서 전환될 것인지"에 대한 Offset이 있다. (일반/애니메이션 두종류)
		[Serializable]
		public class ExtraValue
		{
			
			//1. Depth Value
			[SerializeField]
			public bool _isDepthChanged = false;

			[SerializeField]
			public int _deltaDepth = 0;

			//2. Texture Value
			[SerializeField]
			public bool _isTextureChanged = false;

			[NonSerialized]
			public apTextureData _linkedTextureData = null;

			[SerializeField]
			public int _textureDataID = -1;

			//보간 관련 Cutout Offset
			//1이면 Weight가 1일때 (해당 Key값에 해당할 때)만 적용된다.
			//0이면 Weight가 0 초과 (약간의 값이라도 있을 경우)에 바로 적용된다.
			//즉, 숫자가 낮을 수록 이 값이 적용되는 범위가 넓다.
			//애니메이션의 경우, [이전 프레임]과 [다음 프레임]에 대해서의 Cutout Offset을 각각 설정할 수 있다.
			[SerializeField]
			public float _weightCutout = 0.5f;
			[SerializeField]
			public float _weightCutout_AnimPrev = 0.5f;
			[SerializeField]
			public float _weightCutout_AnimNext = 0.6f;//<<이건 예외적으로 0.5가 아니라 0.6이다. 서로 겹치게 만들기 위함

			public ExtraValue()
			{
				Init();
			}

			public void Init()
			{
				_isDepthChanged = false;
				_deltaDepth = 0;

				_isTextureChanged = false;
				_textureDataID = -1;

				_weightCutout = 0.5f;
				_weightCutout_AnimPrev = 0.5f;
				_weightCutout_AnimNext = 0.6f;
			}

			public void Reset()
			{
				_isDepthChanged = false;
				_deltaDepth = 0;

				_isTextureChanged = false;
				_textureDataID = -1;

				_weightCutout = 0.5f;
				_weightCutout_AnimPrev = 0.5f;
				_weightCutout_AnimNext = 0.6f;

				_linkedTextureData = null;
			}

			public void Link(apPortrait portrait)
			{
				if(_textureDataID >= 0)
				{
					_linkedTextureData = portrait.GetTexture(_textureDataID);
				}
			}

			public void CopyFromSrc(ExtraValue srcValue)
			{
				_isDepthChanged = srcValue._isDepthChanged;
				_deltaDepth = srcValue._deltaDepth;
				_isTextureChanged = srcValue._isTextureChanged;
				_textureDataID = srcValue._textureDataID;

				_weightCutout = srcValue._weightCutout;
				_weightCutout_AnimPrev = srcValue._weightCutout_AnimPrev;
				_weightCutout_AnimNext = srcValue._weightCutout_AnimNext;
			}
		}

		[SerializeField]
		public bool _isExtraValueEnabled = false;

		/// <summary>
		/// Depth, Texture 변환 같은 특수한 값
		/// </summary>
		[SerializeField]
		public ExtraValue _extraValue = new ExtraValue();
		

		// Init
		//------------------------------------------
		public apModifiedMesh()
		{

		}

		// Init - 값 넣기
		//--------------------------------------------------------
		public void Init(int meshGroupID_Modifier, int meshGroupID_Transform, MOD_VALUE_TYPE modValueType)
		{
			_meshGroupUniqueID_Modifier = meshGroupID_Modifier;
			_transformUniqueID = -1;
			_meshUniqueID = -1;
			//_boneUniqueID = -1;

			_modValueType = modValueType;

			_meshGroupUniqueID_Transform = meshGroupID_Transform;

			_isRecursiveChildTransform = (meshGroupID_Modifier != meshGroupID_Transform);

			_isUsePhysicParam = (int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Physics) != 0;
			_meshColor = Color.gray;
			_isVisible = true;

			_isExtraValueEnabled = false;
			if(_extraValue == null)
			{
				_extraValue = new ExtraValue();
			}
			_extraValue.Init();
		}

		public void SetTarget_MeshTransform(int meshTransformID, int meshID, Color meshColor_Default, bool isVisible_Default)
		{
			_transformUniqueID = meshTransformID;
			_meshUniqueID = meshID;
			_isMeshTransform = true;

			_meshColor = meshColor_Default;
			_isVisible = isVisible_Default;
		}

		public void SetTarget_MeshGroupTransform(int meshGroupTransformID, Color meshColor_Default, bool isVisible_Default)
		{
			_transformUniqueID = meshGroupTransformID;
			_isMeshTransform = false;

			_meshColor = meshColor_Default;
			_isVisible = isVisible_Default;
		}

		//public void SetTarget_Bone(int boneID)
		//{
		//	_boneUniqueID = boneID;
		//}

		#region [미사용 코드] 타입에 따른 초기화는 유연성이 떨어져서 패스.
		//public void Init_VertexMorph(int meshGroupID, int meshTransformID, int meshID)
		//{
		//	_targetType = TARGET_TYPE.VertexWithMeshTransform;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = meshTransformID;
		//	_meshUniqueID = meshID;
		//	_boneUniqueID = -1;


		//}

		//public void Init_MeshTransform(int meshGroupID, int meshTransformID, int meshID)
		//{
		//	_targetType = TARGET_TYPE.MeshTransformOnly;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = meshTransformID;
		//	_meshUniqueID = meshID;
		//	_boneUniqueID = -1;
		//}

		//public void Init_MeshGroupTransform(int meshGroupID, int meshGroupTransformID)
		//{
		//	_targetType = TARGET_TYPE.MeshGroupTransformOnly;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = meshGroupTransformID;
		//	_meshUniqueID = -1;
		//	_boneUniqueID = -1;
		//}

		//public void Init_BoneTransform(int meshGroupID, int boneID)
		//{
		//	_targetType = TARGET_TYPE.Bone;
		//	_meshGroupUniqueID = meshGroupID;
		//	_transformUniqueID = -1;
		//	_meshUniqueID = -1;
		//	_boneUniqueID = boneID;
		//} 
		#endregion


		// Init - ID에 맞게 세팅
		//--------------------------------------------------------
		//이건 날립니더
		//public void Link_VertexMorph(apMeshGroup meshGroup, apTransform_Mesh meshTransform, apRenderUnit renderUnit)
		//{
		//	_meshGroup = meshGroup;
		//	_transform_Mesh = meshTransform;
		//	_renderUnit = renderUnit;

		//	//RefreshVertices();

		//}

		/// <summary>
		/// MeshTransform과 ModMesh를 연결한다.
		/// </summary>
		/// <param name="meshGroupOfMod">Modifier가 속한 MeshGroup</param>
		///<param name="meshGroupOfTransform">Transform이 속한 MeshGroup</param>
		public void Link_MeshTransform(apMeshGroup meshGroupOfMod, apMeshGroup meshGroupOfTransform, apTransform_Mesh meshTransform, apRenderUnit renderUnit, apPortrait portrait)
		{

			_meshGroupOfModifier = meshGroupOfMod;
			_meshGroupOfTransform = meshGroupOfTransform;

			_transform_Mesh = meshTransform;
			_renderUnit = renderUnit;

			if (_isUsePhysicParam)
			{
				if (_physicMeshParam == null)
				{
					_physicMeshParam = new apPhysicsMeshParam();
				}
				_physicMeshParam.Link(portrait);
			}

			//Debug.Log("ModMesh Link RenderUnit");
			//RefreshModifiedValues(meshGroupOfMod._parentPortrait);//이전
			LinkValues(meshGroupOfMod._parentPortrait);//변경 20.3.30

			//추가 11.29 : Texture 때문에 ExtraPortrait를 링크한다.
			_extraValue.Link(portrait);
		}

		/// <summary>
		/// MeshGroupTransform과 ModMesh를 연결한다.
		/// </summary>
		/// <param name="meshGroupOfMod">Modifier가 속한 MeshGroup</param>
		/// <param name="meshGroupOfTransform">Transform이 속한 MeshGroup</param>
		public void Link_MeshGroupTransform(apMeshGroup meshGroupOfMod, apMeshGroup meshGroupOfTransform, apTransform_MeshGroup meshGroupTransform, apRenderUnit renderUnit)
		{
			_meshGroupOfModifier = meshGroupOfMod;
			_meshGroupOfTransform = meshGroupOfTransform;

			_transform_MeshGroup = meshGroupTransform;
			_renderUnit = renderUnit;


			//RefreshModifiedValues(meshGroupOfMod._parentPortrait);//이전
			LinkValues(meshGroupOfMod._parentPortrait);//변경 20.3.30

			
		}



		public void Link_Bone()
		{
			//?
		}



		// 데이터 리셋
		//------------------------------------------------------------------
		//수정 20.3.30 : Refresh Modified Values 함수가 "값 갱신"과 "Link" 모든 과정에서 호출된다. 이러니 느릴수밖에
		//Link 함수와 Refresh 함수를 완전히 분리한다.
		//>Opt도 적용
		public void LinkValues(apPortrait portrait)
		{
			if ((int)(_modValueType & MOD_VALUE_TYPE.VertexPosList) != 0)
			{
				LinkVertices();
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.TransformMatrix) != 0)
			{
				if(_transformMatrix == null)
				{
					_transformMatrix = new apMatrix();
				}
				_transformMatrix.MakeMatrix();
			}
			//else if ((int)(_modValueType & MOD_VALUE_TYPE.Color) != 0)
			//{
			//	//_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);//2X 칼라 초기화
			//}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.BoneVertexWeightList) != 0)
			{
				LinkVertexRigs(portrait);
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Physics) != 0)
			{
				//물리 타입의 Weight
				//RefreshVertexWeights(portrait, true, false);//이전
				LinkVertexWeights(portrait, true, false);//변경 20.3.30
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Volume) != 0)
			{
				//물리 값을 사용하지 않는 Weight
				//RefreshVertexWeights(portrait, false, true);//이전
				LinkVertexWeights(portrait, false, true);//변경 20.3.30
			}
			//else if ((int)(_modValueType & MOD_VALUE_TYPE.FFD) != 0)
			//{
			//	Debug.LogError("TODO : ModMesh FFD 타입 정의 필요");
			//}
			//else
			//{
			//	Debug.LogError("TODO : 알 수 없는 ModMesh Value Type : " + _modValueType);
			//}
		}

		/// <summary>
		/// 저장되는 ModifiedValue의 데이터들을 처리 준비하게 해준다.
		/// 값 초기화는 Reset에서 한다. 주의주의
		/// </summary>
		public void RefreshValues_Check(apPortrait portrait)
		{
			if ((int)(_modValueType & MOD_VALUE_TYPE.VertexPosList) != 0)
			{
				//RefreshVertices(); //> Refresh할 내용이 없다.
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.TransformMatrix) != 0)
			{
				_transformMatrix.MakeMatrix();
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.Color) != 0)
			{
				//_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);//2X 칼라 초기화
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.BoneVertexWeightList) != 0)
			{
				//RefreshVertexRigs(portrait);//Refresh할 내용이 없다.
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Physics) != 0)
			{
				//물리 타입의 Weight
				RefreshVertexWeights(portrait, true, false);
			}
			else if ((int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Volume) != 0)
			{
				//물리 값을 사용하지 않는 Weight
				RefreshVertexWeights(portrait, false, true);
			}
			//else if ((int)(_modValueType & MOD_VALUE_TYPE.FFD) != 0)
			//{
			//	Debug.LogError("TODO : ModMesh FFD 타입 정의 필요");
			//}
			//else
			//{
			//	Debug.LogError("TODO : 알 수 없는 ModMesh Value Type : " + _modValueType);
			//}
		}


		//추가 5.19
		//유효하지 않은 데이터를 삭제하는 기능
		public void CheckAndRemoveInvalidData(apModifierBase parentModifier)
		{
			//유효하지 않은 데이터가 있다면 삭제한다.
			if((int)(_modValueType & MOD_VALUE_TYPE.VertexPosList) == 0)
			{
				//1. Vertex 정보가 있는 경우 삭제
				if(_vertices != null && _vertices.Count > 0)
				{
					//Debug.LogError("Invalid Data [" + parentModifier.DisplayName + "] : 잘못된 Vertex 리스트");
					_vertices.Clear();
				}
				
				if(_pins != null && _pins.Count > 0)//추가 22.3.20 [v1.4.0]
				{
					_pins.Clear();
				}
			}
			if((int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Physics) == 0
				&& (int)(_modValueType & MOD_VALUE_TYPE.VertexWeightList_Volume) == 0)
			{
				//2. VertexWeight 정보가 있는 경우 삭제
				if(_vertWeights != null && _vertWeights.Count > 0)
				{
					//Debug.LogError("Invalid Data [" + parentModifier.DisplayName + "] : 잘못된 VertexWeight 리스트");
					_vertWeights.Clear();
				}
			}
			if((int)(_modValueType & MOD_VALUE_TYPE.BoneVertexWeightList) == 0)
			{
				//3. VertRig 정보가 있는 경우 삭제
				if(_vertRigs != null && _vertRigs.Count > 0)
				{
					//Debug.LogError("Invalid Data [" + parentModifier.DisplayName + "] : 잘못된 VertexRig 리스트");
					_vertRigs.Clear();
				}
			}
		}

		// 버텍스 Refresh / Link
		//---------------------------------------------------
		
		public void LinkVertices()//변경 20.3.30 : 내용이 Refresh보다는 Link에 해당한다.
		{
			if (_transform_Mesh._mesh != null)
			{
				bool isSameVerts = true;
				if (_vertices.Count == 0 || _vertices.Count != _transform_Mesh._mesh._vertexData.Count)
				{
					isSameVerts = false;
				}
				else
				{
					//전부 비교해볼까나..
					//빠르게 단순 링크를 시도해보고, 한번이라도 실패하면 다시 리스트를 만들어야한다.
					List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
					apVertex meshVert = null;
					apModifiedVertex modVert = null;
					for (int i = 0; i < meshVertList.Count; i++)
					{
						meshVert = meshVertList[i];
						modVert = _vertices[i];

						if (modVert._vertexUniqueID != meshVert._uniqueID)
						{
							//버텍스 리스트 갱신이 필요하다
							isSameVerts = false;
							break;
						}

						modVert.Link(this, _transform_Mesh._mesh, meshVert);
					}
				}

				if (!isSameVerts)
				{
					//유효한 Vertex만 찾아서 넣어준다.
					//유효하면 - Link
					//유효하지 않다면 - Pass (Link 안된거 삭제)
					//없는건 - Add
					//순서는.. Index를 넣어서



					//1. 일단 기존 데이터 복사 - 없어진 Vertex를 빼자
					if (_vertices.Count != 0)
					{
						apModifiedVertex modVert = null;
						for (int i = 0; i < _vertices.Count; i++)
						{
							modVert = _vertices[i];
							apVertex existVert = _transform_Mesh._mesh._vertexData.Find(delegate (apVertex a)
							{
								return a._uniqueID == modVert._vertexUniqueID;
							});

							if (existVert != null)
							{
								//유효하다면 Link
								modVert.Link(this, _transform_Mesh._mesh, existVert);
							}
							else
							{
								//유효하지 않다면.. Unlink -> 나중에 삭제됨
								modVert._vertex = null;
							}
						}

						//이제 존재하지 않는 Vertex에 대해서는 삭제
						_vertices.RemoveAll(delegate (apModifiedVertex a)
						{
							return a._vertex == null;
						});

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];
							//해당 Vertex가 있었는가
							bool isLinked = _vertices.Exists(delegate (apModifiedVertex a)
							{
								return a._vertex == meshVert;
							});

							//없으면 추가
							if (!isLinked)
							{
								apModifiedVertex newVert = new apModifiedVertex();
								newVert.Init(meshVert._uniqueID, meshVert);
								newVert.Link(this, _transform_Mesh._mesh, meshVert);

								_vertices.Add(newVert);//<<새로 추가할 리스트에 넣어준다.
							}
						}

						//Vertex Index에 맞게 정렬
						_vertices.Sort(delegate (apModifiedVertex a, apModifiedVertex b)
						{
							return a._vertIndex - b._vertIndex;
						});
					}
					else
					{
						//2. 아예 리스트가 없을 때
						_vertices.Clear();

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];

							apModifiedVertex newVert = new apModifiedVertex();
							newVert.Init(meshVert._uniqueID, meshVert);
							newVert.Link(this, _transform_Mesh._mesh, meshVert);

							_vertices.Add(newVert);//<<새로 추가할 리스트에 넣어준다.
						}
					}
				}

				//추가 22.3.20 [v1.4.0] Pin 링크
				bool isSamePins = true;
				int nModPins = _pins != null ? _pins.Count : 0;
				int nSrcPins = 0;
				if(_transform_Mesh._mesh._pinGroup != null)
				{
					nSrcPins = _transform_Mesh._mesh._pinGroup.NumPins;
				}
				if(nModPins != nSrcPins)
				{
					//개수가 다르다면
					isSamePins = false;
				}
				else if(nModPins > 0)
				{
					//개수가 같다면
					//바로 Link를 하되, 하나라도 다르다면 다시 전부 링크하자
					List<apMeshPin> srcPins = _transform_Mesh._mesh._pinGroup._pins_All;
					apMeshPin curSrcPin = null;
					apModifiedPin curModPin = null;

					for (int i = 0; i < nModPins; i++)
					{
						curSrcPin = srcPins[i];
						curModPin = _pins[i];

						if(curSrcPin._uniqueID != curModPin._pinUniqueID)
						{
							//서로 다른 ID. 리스트를 다시 갱신하자
							isSamePins = false;
							break;
						}

						//링크
						curModPin.Link(this, _transform_Mesh._mesh, curSrcPin);
					}
				}

				if(!isSamePins)
				{
					//개수가 다르다면 기존걸 백업하고 다시 링크
					if(nModPins > 0)
					{
						//이전 핀 데이터가 있다면 이전
						List<apModifiedPin> prevPins = _pins;
						_pins = new List<apModifiedPin>();

						if(nSrcPins > 0)
						{
							List<apMeshPin> srcPins = _transform_Mesh._mesh._pinGroup._pins_All;
							
							apMeshPin curSrcPin = null;
							apModifiedPin existModPin = null;

							for (int i = 0; i < nSrcPins; i++)
							{
								curSrcPin = srcPins[i];

								//이 핀에 해당되는 핀을 찾자
								existModPin = prevPins.Find(delegate(apModifiedPin a)
								{
									return a._pinUniqueID == curSrcPin._uniqueID;
								});
								if(existModPin != null)
								{
									//이전의 핀 데이터가 존재한다.
									existModPin.Link(this, _transform_Mesh._mesh, curSrcPin);
									_pins.Add(existModPin);
								}
								else
								{
									//새로운 핀 데이터를 만들어서 추가하자
									apModifiedPin newModPin = new apModifiedPin();
									newModPin.Init(curSrcPin._uniqueID, curSrcPin);
									newModPin.Link(this, _transform_Mesh._mesh, curSrcPin);
									_pins.Add(newModPin);
								}
							}
						}

					}
					else
					{
						//이전 핀 데이터가 없다.
						if(_pins == null)
						{
							_pins = new List<apModifiedPin>();
						}
						_pins.Clear();

						if(nSrcPins > 0)
						{
							List<apMeshPin> srcPins = _transform_Mesh._mesh._pinGroup._pins_All;
							
							apMeshPin curSrcPin = null;

							for (int i = 0; i < nSrcPins; i++)
							{
								curSrcPin = srcPins[i];

								//새로운 핀 데이터를 만들어서 추가하자
								apModifiedPin newModPin = new apModifiedPin();
								newModPin.Init(curSrcPin._uniqueID, curSrcPin);
								newModPin.Link(this, _transform_Mesh._mesh, curSrcPin);
								_pins.Add(newModPin);
							}
						}
					}
					
				}
			}
		}


		//public void RefreshVertexRigs(apPortrait portrait)
		public void LinkVertexRigs(apPortrait portrait)//변경 20.3.20 : 이 함수는 Refresh보다 Link에 해당한다.
		{
			//Debug.LogWarning("LinkVertexRigs");
			if (_transform_Mesh._mesh != null)
			{
				bool isSameVerts = true;
				if (_vertRigs.Count == 0 || _vertRigs.Count != _transform_Mesh._mesh._vertexData.Count)
				{
					isSameVerts = false;
				}
				else
				{
					//전부 비교해볼까나..
					//빠르게 단순 링크를 시도해보고, 한번이라도 실패하면 다시 리스트를 만들어야한다.
					List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
					apVertex meshVert = null;
					apModifiedVertexRig modVertRig = null;
					for (int i = 0; i < meshVertList.Count; i++)
					{
						meshVert = meshVertList[i];
						modVertRig = _vertRigs[i];

						if (modVertRig._vertexUniqueID != meshVert._uniqueID)
						{
							//버텍스 리스트 갱신이 필요하다
							isSameVerts = false;
							break;
						}

						//Debug.Log("Refresh ModVertRigs >> TODO"); > 이 부분이 계속 호출된다.
						modVertRig.Link(this, _transform_Mesh._mesh, meshVert);
						modVertRig.LinkWeightPair(portrait, _meshGroupOfModifier);
					}
				}

				if (!isSameVerts)
				{
					//<메시의 버텍스의 개수나 종류, 순서가 저장된것과 다른 경우>

					//유효한 Vertex만 찾아서 넣어준다.
					//유효하면 - Link
					//유효하지 않다면 - Pass (Link 안된거 삭제)
					//없는건 - Add
					//순서는.. Index를 넣어서



					//1. 일단 기존 데이터 복사 - 없어진 Vertex를 빼자
					if (_vertRigs.Count != 0)
					{
						apModifiedVertexRig modVertRig = null;
						for (int i = 0; i < _vertRigs.Count; i++)
						{
							modVertRig = _vertRigs[i];
							apVertex existVert = _transform_Mesh._mesh._vertexData.Find(delegate (apVertex a)
							{
								return a._uniqueID == modVertRig._vertexUniqueID;
							});

							if (existVert != null)
							{
								//유효하다면 Link
								modVertRig.Link(this, _transform_Mesh._mesh, existVert);
								modVertRig.LinkWeightPair(portrait, _meshGroupOfModifier);
							}
							else
							{
								//유효하지 않다면.. Unlink -> 나중에 삭제됨
								modVertRig._vertex = null;
							}
						}

						//이제 존재하지 않는 Vertex에 대해서는 삭제
						_vertRigs.RemoveAll(delegate (apModifiedVertexRig a)
						{
							return a._vertex == null;
						});

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];
							//해당 Vertex가 있었는가
							bool isLinked = _vertRigs.Exists(delegate (apModifiedVertexRig a)
							{
								return a._vertex == meshVert;
							});

							//없으면 추가
							if (!isLinked)
							{
								apModifiedVertexRig newVertRig = new apModifiedVertexRig();
								newVertRig.Init(meshVert._uniqueID, meshVert);
								newVertRig.Link(this, _transform_Mesh._mesh, meshVert);
								newVertRig.LinkWeightPair(portrait, _meshGroupOfModifier);

								_vertRigs.Add(newVertRig);//<<새로 추가할 리스트에 넣어준다.
							}
						}

						//Vertex Index에 맞게 정렬
						_vertRigs.Sort(delegate (apModifiedVertexRig a, apModifiedVertexRig b)
						{
							return a._vertIndex - b._vertIndex;
						});
					}
					else
					{
						//2. 아예 리스트가 없을 때
						_vertRigs.Clear();

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];

							apModifiedVertexRig newVertRig = new apModifiedVertexRig();
							newVertRig.Init(meshVert._uniqueID, meshVert);
							newVertRig.Link(this, _transform_Mesh._mesh, meshVert);
							newVertRig.LinkWeightPair(portrait, _meshGroupOfModifier);

							_vertRigs.Add(newVertRig);//<<새로 추가할 리스트에 넣어준다.
						}
					}

				}

				//int nRefreshedRenverVerts = 0;
				for (int i = 0; i < _vertRigs.Count; i++)
				{
					_vertRigs[i].CheckAndLinkModMeshAndRenderVertex(this);
					//bool isRefreshed = _vertRigs[i].CheckAndLinkModMeshAndRenderVertex(this);
					//if(isRefreshed)
					//{
					//	nRefreshedRenverVerts++;
					//}
				}

				//Debug.Log("[" + nRefreshedRenverVerts + "] 개의 RenderVert가 유효함");
			}
		}










		//Physics / Volume (폐기) 의 Link/Refresh 함수들
		public void LinkVertexWeights(apPortrait portrait, bool isPhysics, bool isVolume)
		{
			if (_renderUnit != null)
			{
				//"Modifier가 연산되기 전"의 WorldPosition을 미리 계산하자
				_renderUnit.CalculateWorldPositionWithoutModifier();
			}
			
			if (_transform_Mesh._mesh != null)
			{
				bool isSameVerts = true;
				if (_vertWeights.Count == 0 || _vertWeights.Count != _transform_Mesh._mesh._vertexData.Count)
				{
					isSameVerts = false;
				}
				else
				{
					//전부 비교해볼까나..
					//빠르게 단순 링크를 시도해보고, 한번이라도 실패하면 다시 리스트를 만들어야한다.
					List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
					apVertex meshVert = null;
					apModifiedVertexWeight modVertWeight = null;
					for (int i = 0; i < meshVertList.Count; i++)
					{
						meshVert = meshVertList[i];
						modVertWeight = _vertWeights[i];

						if (modVertWeight._vertexUniqueID != meshVert._uniqueID)
						{
							//버텍스 리스트 갱신이 필요하다
							isSameVerts = false;
							break;
						}
						modVertWeight.Link(this, _transform_Mesh._mesh, meshVert);
					}
				}

				if (!isSameVerts)
				{
					//유효한 Vertex만 찾아서 넣어준다.
					//유효하면 - Link
					//유효하지 않다면 - Pass (Link 안된거 삭제)
					//없는건 - Add
					//순서는.. Index를 넣어서

					//1. 일단 기존 데이터 복사 - 없어진 Vertex를 빼자
					if (_vertWeights.Count != 0)
					{
						apModifiedVertexWeight modVertWeight = null;
						for (int i = 0; i < _vertWeights.Count; i++)
						{
							modVertWeight = _vertWeights[i];
							apVertex existVert = _transform_Mesh._mesh._vertexData.Find(delegate (apVertex a)
							{
								return a._uniqueID == modVertWeight._vertexUniqueID;
							});

							if (existVert != null)
							{
								//유효하다면 Link
								modVertWeight.Link(this, _transform_Mesh._mesh, existVert);
							}
							else
							{
								//유효하지 않다면.. Unlink -> 나중에 삭제됨
								modVertWeight._vertex = null;
							}
						}

						//이제 존재하지 않는 Vertex에 대해서는 삭제
						_vertWeights.RemoveAll(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == null;
						});

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];
							//해당 Vertex가 있었는가
							bool isLinked = _vertWeights.Exists(delegate (apModifiedVertexWeight a)
							{
								return a._vertex == meshVert;
							});

							//없으면 추가
							if (!isLinked)
							{
								apModifiedVertexWeight newVertWeight = new apModifiedVertexWeight();
								newVertWeight.Init(meshVert._uniqueID, meshVert);
								newVertWeight.SetDataType(isPhysics, isVolume);//<<어떤 타입인지 넣는다.
																			   //TODO:Modifier에 따라 특성 추가
								newVertWeight.Link(this, _transform_Mesh._mesh, meshVert);

								_vertWeights.Add(newVertWeight);//<<새로 추가할 리스트에 넣어준다.
							}
						}

						//Vertex Index에 맞게 정렬
						_vertWeights.Sort(delegate (apModifiedVertexWeight a, apModifiedVertexWeight b)
						{
							return a._vertIndex - b._vertIndex;
						});
					}
					else
					{
						//2. 아예 리스트가 없을 때
						_vertWeights.Clear();

						List<apVertex> meshVertList = _transform_Mesh._mesh._vertexData;
						apVertex meshVert = null;

						for (int i = 0; i < meshVertList.Count; i++)
						{
							meshVert = meshVertList[i];

							apModifiedVertexWeight newVertWeight = new apModifiedVertexWeight();
							newVertWeight.Init(meshVert._uniqueID, meshVert);
							newVertWeight.SetDataType(isPhysics, isVolume);//<<어떤 타입인지 넣는다.
																		   //TODO:Modifier에 따라 특성 추가
							newVertWeight.Link(this, _transform_Mesh._mesh, meshVert);

							_vertWeights.Add(newVertWeight);//<<새로 추가할 리스트에 넣어준다.
						}
					}

				}

				for (int i = 0; i < _vertWeights.Count; i++)
				{
					//_vertWeights[i].RefreshModMeshAndWeights(this);//이전
					_vertWeights[i].LinkModMeshAndWeights(this);
				}
			}
			//물리 관련 Refresh를 한번 더 한다.
			if (isPhysics)
			{
				RefreshVertexWeight_Physics(true);
			}
		}

		//Physics/Volume 모디파이어의 Refresh 함수
		//Link 관련 내용은 제외한다. (20.3.30)
		public void RefreshVertexWeights(apPortrait portrait, bool isPhysics, bool isVolume)
		{
			if (_renderUnit != null)
			{
				//"Modifier가 연산되기 전"의 WorldPosition을 미리 계산하자
				_renderUnit.CalculateWorldPositionWithoutModifier();
			}
			//Debug.Log("<<< RefreshVertexWeights >>>");
			if (_transform_Mesh._mesh != null)
			{
				
				for (int i = 0; i < _vertWeights.Count; i++)
				{
					_vertWeights[i].RefreshModMeshAndWeights_Check(this);
				}
			}
			//물리 관련 Refresh를 한번 더 한다.
			if (isPhysics)
			{
				RefreshVertexWeight_Physics(true);
			}
		}

		/// <summary>
		/// ModVertexWeight를 사용하는 Modifier 중 Physics인 경우,
		/// Vertex의 Weight가 바뀌었다면 한번씩 이 함수를 호출해주자.
		/// Constraint(자동), isEnabled(자동), Main(수동) 등을 다시 세팅한다.
		/// </summary>
		private void RefreshVertexWeight_Physics(bool isForceRefresh)
		{
			if (_transform_Mesh == null || _transform_Mesh._mesh == null || _vertWeights.Count == 0)
			{
				return;
			}
			bool isAnyChanged = false;
			apModifiedVertexWeight vertWeight = null;
			float bias = 0.001f;
			for (int iVW = 0; iVW < _vertWeights.Count; iVW++)
			{
				vertWeight = _vertWeights[iVW];
				bool isNextEnabled = false;
				if (vertWeight._weight < bias)
				{
					isNextEnabled = false;
				}
				else
				{
					isNextEnabled = true;
				}
				//Weight 활성화 여부가 바뀌었는지 체크
				if (isNextEnabled != vertWeight._isEnabled)
				{
					vertWeight._isEnabled = isNextEnabled;
					isAnyChanged = true;
				}
			}
			if (!isAnyChanged && !isForceRefresh)
			{
				return;
			}

			for (int iVW = 0; iVW < _vertWeights.Count; iVW++)
			{
				vertWeight = _vertWeights[iVW];
				vertWeight.RefreshModMeshAndWeights_Check(this);
			}
			for (int iVW = 0; iVW < _vertWeights.Count; iVW++)
			{
				vertWeight = _vertWeights[iVW];
				vertWeight.RefreshLinkedVertex();
			}
		}

		// Functions
		//------------------------------------------
		public void ResetValues()
		{
			for (int i = 0; i < _vertices.Count; i++)
			{
				_vertices[i]._deltaPos = Vector2.zero;
			}			
			_transformMatrix.SetIdentity();
			_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_isVisible = true;

			//추가 22.3.20 [v1.4.0]			
			int nPins = _pins != null ? _pins.Count : 0;
			if (nPins > 0)
			{
				for (int i = 0; i < nPins; i++)
				{
					_pins[i]._deltaPos = Vector2.zero;
				}
			}
		}


		/// <summary>
		/// 추가 22.7.10 : 리셋시 특정 프로퍼티칸 리셋할 수 있다.
		/// </summary>
		/// <param name="isResetVerts"></param>
		/// <param name="isResetPins"></param>
		/// <param name="isResetTransform"></param>
		/// <param name="isResetVisibility"></param>
		/// <param name="isResetColor"></param>
		/// <param name="isResetExtra"></param>
		/// <param name="isSelectedVertPinOnly"></param>
		/// <param name="selectedModVerts"></param>
		/// <param name="selectedModPins"></param>
		public void ResetValues(	bool isResetVerts,
									bool isResetPins,
									bool isResetTransform,
									bool isResetVisibility,
									bool isResetColor,
									bool isResetExtra,
									bool isSelectedVertPinOnly,
									List<apModifiedVertex> selectedModVerts,
									List<apModifiedPin> selectedModPins)
		{
			if(isResetVerts)
			{
				//버텍스 리셋
				if(isSelectedVertPinOnly)
				{
					//선택된 버텍스만 리셋할 때
					if(selectedModVerts != null)
					{
						apModifiedVertex curModVert = null;
						for (int i = 0; i < _vertices.Count; i++)
						{
							curModVert = _vertices[i];
							if(selectedModVerts.Contains(curModVert))
							{
								curModVert._deltaPos = Vector2.zero;
							}
						}			
					}
				}
				else
				{
					//전체 버텍스를 리셋할 때
					for (int i = 0; i < _vertices.Count; i++)
					{
						_vertices[i]._deltaPos = Vector2.zero;
					}			
				}
			}

			int nPins = _pins != null ? _pins.Count : 0;
			if(isResetPins && nPins > 0)
			{
				if (isSelectedVertPinOnly)
				{
					if (selectedModPins != null)
					{
						apModifiedPin curModPin = null;
						for (int i = 0; i < nPins; i++)
						{
							curModPin = _pins[i];
							if(selectedModPins.Contains(curModPin))
							{
								curModPin._deltaPos = Vector2.zero;
							}
						}
					}
				}
				else
				{
					//전체 핀 리셋
					for (int i = 0; i < nPins; i++)
					{
						_pins[i]._deltaPos = Vector2.zero;
					}
				}
			}
			
			if(isResetTransform)
			{
				//Transform 초기화
				_transformMatrix.SetIdentity();
			}

			if(isResetVisibility)
			{
				//Visibility 초기화
				_isVisible = true;
			}

			if(isResetColor)
			{
				//Color 초기화
				_meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}

			if(isResetExtra)
			{
				//Extra를 리셋한다.
				_extraValue.Reset();
			}
		}



		public void UpdateBeforeBake(apPortrait portrait, apMeshGroup mainMeshGroup, apTransform_MeshGroup mainMeshGroupTransform)
		{
			//Bake 전에 업데이트할게 있으면 여기서 업데이트하자

			//1. VertRig의 LocalPos 갱신을 여기서 하자
			#region [미사용 코드]
			//if(_vertRigs != null && _vertRigs.Count > 0)
			//{
			//	apModifiedVertexRig vertRig = null;
			//	//기존 링크 말고, Bake 직전의 Transform 등을 검색하여 값을 넣어주자
			//	apTransform_Mesh meshTransform = mainMeshGroup.GetMeshTransformRecursive(_transformUniqueID);
			//	if (meshTransform != null)
			//	{
			//		apMesh mesh = meshTransform._mesh;
			//		if (mesh != null)
			//		{
			//			for (int iVR = 0; iVR < _vertRigs.Count; iVR++)
			//			{
			//				vertRig = _vertRigs[iVR];
			//				apVertex vert = vertRig._vertex;

			//				for (int iW = 0; iW < vertRig._weightPairs.Count; iW++)
			//				{
			//					apModifiedVertexRig.WeightPair weightPair = vertRig._weightPairs[iW];
			//					weightPair.CalculateLocalPos(vert._pos, mesh.Matrix_VertToLocal, meshTransform._matrix_TFResult_WorldWithoutMod, weightPair._bone._defaultMatrix);
			//				}

			//			}
			//		}
			//	}
			//} 
			#endregion
		}



		//추가 20.1.17 : 데이터 복사
		/// <summary>
		/// ModMesh 데이터를 복사한다. 연결 정보는 복사하지 않는다.
		/// 동일한 메시 그룹 내에서 복사할 때만 이 함수를 사용하자. (다른 메시 그룹으로 복사할때는 변환 정보가 더 필요하므로 이 함수를 사용하면 안된다.)
		/// </summary>
		/// <param name="srcModMesh"></param>
		public void CopyFromSrc(apModifiedMesh srcModMesh)
		{
			//값을 복사하자
			_isUsePhysicParam = srcModMesh._isUsePhysicParam;

			apPhysicsMeshParam srcPhysicsParam = srcModMesh.PhysicParam;
			apPhysicsMeshParam dstPhysicsParam = PhysicParam;
			if (dstPhysicsParam != null && srcPhysicsParam != null)
			{
				dstPhysicsParam.CopyFromSrc(srcPhysicsParam);
			}
			_modValueType = srcModMesh._modValueType;

			//Vertex Morph 정보를 복사하자
			int nModVerts = srcModMesh._vertices == null ? 0 : srcModMesh._vertices.Count;
			apModifiedVertex srcModVert = null;
			apModifiedVertex dstModVert = null;

			if (nModVerts > 0 && _vertices == null)
			{
				_vertices = new List<apModifiedVertex>();
			}

			for (int iSrcModVert = 0; iSrcModVert < nModVerts; iSrcModVert++)
			{
				srcModVert = srcModMesh._vertices[iSrcModVert];
				dstModVert = new apModifiedVertex();
				_vertices.Add(dstModVert);


				//값을 복사하자
				//동일한 메시를 참조하는 것일테므로, 별도로 검사하지는 말자
				dstModVert._vertexUniqueID = srcModVert._vertexUniqueID;
				dstModVert._vertIndex = srcModVert._vertIndex;
				dstModVert._deltaPos = srcModVert._deltaPos;
				dstModVert._overlapWeight = srcModVert._overlapWeight;
			}

			//추가 22.3.20 : 핀 정보 복사
			int nModPins = srcModMesh._pins != null ? srcModMesh._pins.Count : 0;
			apModifiedPin srcModPin = null;
			apModifiedPin dstModPin = null;
			
			if(_pins == null)
			{
				_pins = new List<apModifiedPin>();
			}
			_pins.Clear();

			for (int iSrcModPin = 0; iSrcModPin < nModPins; iSrcModPin++)
			{
				srcModPin = srcModMesh._pins[iSrcModPin];
				dstModPin = new apModifiedPin();

				//값 복사
				dstModPin._pinUniqueID = srcModPin._pinUniqueID;
				dstModPin._deltaPos = srcModPin._deltaPos;
			}
			


			//나머지 정보를 복사하자
			_transformMatrix.SetMatrix(srcModMesh._transformMatrix, true);
			_meshColor = srcModMesh._meshColor;
			_isVisible = srcModMesh._isVisible;


			//VertRigging을 복사하자
			int nModVertRig = srcModMesh._vertRigs == null ? 0 : srcModMesh._vertRigs.Count;
			if (nModVertRig > 0 && _vertRigs == null)
			{
				_vertRigs = new List<apModifiedVertexRig>();
			}

			apModifiedVertexRig srcModVertRig = null;
			apModifiedVertexRig dstModVertRig = null;

			for (int iSrcModVertRig = 0; iSrcModVertRig < nModVertRig; iSrcModVertRig++)
			{
				srcModVertRig = srcModMesh._vertRigs[iSrcModVertRig];
				dstModVertRig = new apModifiedVertexRig();
				_vertRigs.Add(dstModVertRig);

				//값을 복사하자.
				//본 정보는 참조를 해야함
				dstModVertRig._vertexUniqueID = srcModVertRig._vertexUniqueID;
				dstModVertRig._vertIndex = srcModVertRig._vertIndex;
				dstModVertRig._totalWeight = srcModVertRig._totalWeight;

				//Weight Pair를 복사하자
				int nWeightPair = srcModVertRig._weightPairs == null ? 0 : srcModVertRig._weightPairs.Count;
				if (nWeightPair > 0 && dstModVertRig._weightPairs == null)
				{
					dstModVertRig._weightPairs = new List<apModifiedVertexRig.WeightPair>();
				}

				apModifiedVertexRig.WeightPair srcWeightPair = null;
				apModifiedVertexRig.WeightPair dstWeightPair = null;
				for (int iWeightPair = 0; iWeightPair < nWeightPair; iWeightPair++)
				{
					srcWeightPair = srcModVertRig._weightPairs[iWeightPair];
					if (srcWeightPair == null)
					{
						continue;
					}
					//본을 찾아서 유효한지 보자
					if (srcWeightPair._bone != null)
					{
						dstWeightPair = new apModifiedVertexRig.WeightPair(srcWeightPair._bone);
						dstWeightPair._weight = srcWeightPair._weight;

						dstModVertRig._weightPairs.Add(dstWeightPair);
					}
				}
			}

			//VertWeight를 복사하자
			int nModVertWeight = srcModMesh._vertWeights == null ? 0 : srcModMesh._vertWeights.Count;
			if (nModVertWeight > 0 && _vertWeights == null)
			{
				_vertWeights = new List<apModifiedVertexWeight>();
			}

			apModifiedVertexWeight srcModVertWeight = null;
			apModifiedVertexWeight dstModVertWeight = null;

			for (int iModVertWeight = 0; iModVertWeight < nModVertWeight; iModVertWeight++)
			{
				srcModVertWeight = srcModMesh._vertWeights[iModVertWeight];
				dstModVertWeight = new apModifiedVertexWeight();

				dstModVertWeight._vertexUniqueID = srcModVertWeight._vertexUniqueID;
				dstModVertWeight._vertIndex = srcModVertWeight._vertIndex;

				dstModVertWeight._isEnabled = srcModVertWeight._isEnabled;
				dstModVertWeight._weight = srcModVertWeight._weight;
				dstModVertWeight._isPhysics = srcModVertWeight._isPhysics;
				dstModVertWeight._isVolume = srcModVertWeight._isVolume;
				dstModVertWeight._pos_World_NoMod = srcModVertWeight._pos_World_NoMod;

				dstModVertWeight._deltaPosRadius_Free = srcModVertWeight._deltaPosRadius_Free;
				dstModVertWeight._deltaPosRadius_Max = srcModVertWeight._deltaPosRadius_Max;

				if (dstModVertWeight._physicParam != null)
				{
					dstModVertWeight._physicParam = new apPhysicsVertParam();
				}
				if (srcModVertWeight._physicParam != null)
				{
					dstModVertWeight._physicParam.CopyFromSrc(srcModVertWeight._physicParam);
				}

				_vertWeights.Add(dstModVertWeight);
			}

			//Extra Value를 복사하자
			_isExtraValueEnabled = srcModMesh._isExtraValueEnabled;

			if (_extraValue == null)
			{
				_extraValue = new apModifiedMesh.ExtraValue();
			}
			if (srcModMesh._extraValue != null)
			{
				_extraValue.CopyFromSrc(srcModMesh._extraValue);
			}
		}



		// Get / Set
		//------------------------------------------
		public apModifiedVertexWeight GetVertexWeight(apVertex vertex)
		{
			if (vertex == null)
			{
				return null;
			}
			return _vertWeights.Find(delegate (apModifiedVertexWeight a)
			{
				return a._vertexUniqueID == vertex._uniqueID;
			});
		}

		// 비교 관련
		//------------------------------------------
		public bool IsContains_MeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform, apMesh mesh)
		{
			if (_meshGroupUniqueID_Modifier == meshGroup._uniqueID &&
				_transformUniqueID == meshTransform._transformUniqueID &&
				_transformUniqueID >= 0 &&
				_meshUniqueID == mesh._uniqueID &&
				_isMeshTransform
				)
			{
				return true;
			}
			return false;
		}

		public bool IsContains_MeshGroupTransform(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform)
		{
			if (_meshGroupUniqueID_Modifier == meshGroup._uniqueID &&
				_transformUniqueID == meshGroupTransform._transformUniqueID &&
				_transformUniqueID >= 0 &&
				!_isMeshTransform)
			{
				return true;
			}
			return false;
		}

		//public bool IsContains_Bone(apMeshGroup meshGroup, )
		//TODO : Bone
	}
}