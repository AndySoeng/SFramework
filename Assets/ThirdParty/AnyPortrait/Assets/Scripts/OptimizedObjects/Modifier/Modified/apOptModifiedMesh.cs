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
	/// Modifier에서 작성된 "변경 사항" 중 Mesh에 대한 데이터가 저장되는 클래스
	/// Modifier Vertex를 가지고 있어서 계산시 데이터를 제공한다.
	/// RealTime 전용
	/// </summary>
	[Serializable]
	public class apOptModifiedMesh
	{
		// Members
		//--------------------------------------------
		public apPortrait _portrait = null;

		//public apModifiedMesh.TARGET_TYPE _targetType = apModifiedMesh.TARGET_TYPE.MeshTransformOnly;
		public apModifiedMesh.MOD_VALUE_TYPE _modValueType = apModifiedMesh.MOD_VALUE_TYPE.Unknown;

		//적용 대상
		//에디터와 달리 바로 Monobehaviour를 저장하자.
		public apOptMesh _targetMesh = null;
		public apOptTransform _targetTransform = null;
		public apOptTransform _rootTransform = null;

		public int _rootMeshGroupUniqueID = -1;

		public int _meshUniqueID = -1;
		public int _transformUniqueID = -1;

		//public int _boneUniqueID = -1;

		public bool _isMeshTransform = true;

		//TODO : Bone

		//1. Mesh 타입인 경우
		//-> Vertex 리스트 (배열로 한다)
		public int _nVerts = 0;
		public int _nVertRigs = 0;
		public int _nVertWeights = 0;

		[SerializeField]
		public apOptModifiedVertex[] _vertices = null;

		[SerializeField]
		public apOptModifiedVertexRig[] _vertRigs = null;

		[SerializeField]
		public apOptModifiedVertexWeight[] _vertWeights = null;


		//2. Transform 타입인 경우
		//-> Transform 변동사항
		[SerializeField]
		public apMatrix _transformMatrix = new apMatrix();

		[SerializeField]
		public Color _meshColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[SerializeField]
		public bool _isVisible = true;


		//물리 파라미터
		[SerializeField]
		public bool _isUsePhysicParam = false; //<<Bake하자

		[SerializeField]
		private apOptPhysicsMeshParam _physicMeshParam = new apOptPhysicsMeshParam();

		public apOptPhysicsMeshParam PhysicParam
		{
			get
			{
				if (_isUsePhysicParam)
				{
					if (_physicMeshParam == null)
					{ _physicMeshParam = new apOptPhysicsMeshParam(); }
					return _physicMeshParam;
				}
				return null;
			}
		}


		//12.05 추가 : Depth와 텍스쳐를 전환하는 설정
		[Serializable]
		public class OptExtraValue
		{
			[SerializeField]
			public bool _isDepthChanged = false;

			[SerializeField]
			public int _deltaDepth = 0;

			[SerializeField]
			public bool _isTextureChanged = false;

			[SerializeField]
			public int _textureDataID = -1;

			[NonSerialized]
			public apOptTextureData _linkedTextureData = null;

			[SerializeField]
			public float _weightCutout = 0.5f;
			[SerializeField]
			public float _weightCutout_AnimPrev = 0.5f;
			[SerializeField]
			public float _weightCutout_AnimNext = 0.6f;

			public OptExtraValue()
			{
				Init();
			}

			public void Init()
			{
				_isDepthChanged = false;
				_deltaDepth = 0;

				_isTextureChanged = false;
				_textureDataID = -1;
				_linkedTextureData = null;

				_weightCutout = 0.5f;
				_weightCutout_AnimPrev = 0.5f;
				_weightCutout_AnimNext = 0.6f;
			}

			public void Bake(apModifiedMesh.ExtraValue srcValue)
			{
				Init();

				_isDepthChanged = srcValue._isDepthChanged;
				_deltaDepth = srcValue._deltaDepth;
				if(_deltaDepth == 0)
				{
					_isDepthChanged = false;
				}

				_isTextureChanged = srcValue._isTextureChanged;
				_textureDataID = srcValue._textureDataID;
				//_linkedTextureData = null;//이건 나중에 Link
				if(_textureDataID < 0)
				{
					_isTextureChanged = false;
				}

				_weightCutout = srcValue._weightCutout;
				_weightCutout_AnimPrev = srcValue._weightCutout_AnimPrev;
				_weightCutout_AnimNext = srcValue._weightCutout_AnimNext;
			}

			public void Link(apPortrait portrait)
			{
				if(_textureDataID >= 0 && _isTextureChanged)
				{
					apOptTextureData linkedOptTextureData = portrait._optTextureData.Find(delegate(apOptTextureData a)
					{
						return a._srcUniqueID == _textureDataID;
					});

					if(linkedOptTextureData != null)
					{
						_linkedTextureData = linkedOptTextureData;
					}
					else
					{
						_linkedTextureData = null;
						_textureDataID = -1;
						_isTextureChanged = false;
					}
				}
			}
		}

		[SerializeField]
		public bool _isExtraValueEnabled = false;

		/// <summary>
		/// Depth, Texture 변환 같은 특수한 값
		/// </summary>
		[SerializeField]
		public OptExtraValue _extraValue = new OptExtraValue();



		// Init
		//--------------------------------------------
		public apOptModifiedMesh()
		{

		}

		public void Link(apPortrait portrait)
		{
			//Portrait를 기준으로 Link를 해야한다.
			_portrait = portrait;

			//필요한 경우 Link 추가

			if (_physicMeshParam != null && _isUsePhysicParam)
			{
				_physicMeshParam.Link(_portrait);
			}

			if (_nVertWeights > 0)
			{
				for (int i = 0; i < _nVertWeights; i++)
				{
					_vertWeights[i].Link(this,
											_targetTransform,
											_targetMesh,
											//_targetMesh.RenderVertices[_vertWeights[i]._vertIndex]//이전
											_targetMesh.RenderVertices[i]//<<변경 (이미 정렬되어 있으므로 i=vertIndex 이다.)
											);
				}
			}

			//추가 : Extra Option
			if(_isExtraValueEnabled)
			{
				_extraValue.Link(portrait);
			}

			//추가 20.11.5 : Transform Matrix의 MakeMatrix가 실행된 코드가 하나도 없었다!! ㄷㄷ
			_transformMatrix.MakeMatrix();
		}

		// Init - Bake
		//--------------------------------------------
		public bool Bake(apModifiedMesh srcModMesh, apPortrait portrait)
		{
			_portrait = portrait;
			_rootMeshGroupUniqueID = srcModMesh._meshGroupUniqueID_Modifier;

			_meshUniqueID = srcModMesh._meshUniqueID;
			_transformUniqueID = srcModMesh._transformUniqueID;

			//_boneUniqueID = srcModMesh._boneUniqueID;

			_isMeshTransform = srcModMesh._isMeshTransform;

			apOptTransform rootTransform = _portrait.GetOptTransformAsMeshGroup(_rootMeshGroupUniqueID);
			apOptTransform targetTransform = _portrait.GetOptTransform(_transformUniqueID);

			if (targetTransform == null)
			{
				Debug.LogError("Bake 실패 : 찾을 수 없는 연결된 OptTransform [" + _transformUniqueID + "]");
				Debug.LogError("이미 삭제된 객체에 연결된 ModMesh가 아닌지 확인해보세염");
				return false;
			}
			apOptMesh targetMesh = null;
			if (targetTransform._unitType == apOptTransform.UNIT_TYPE.Mesh)
			{
				targetMesh = targetTransform._childMesh;
			}



			if (rootTransform == null)
			{
				Debug.LogError("ModifiedMesh 연동 에러 : 알수 없는 RootTransform");
				return false;
			}

			//_targetType = srcModMesh._targetType;
			_modValueType = srcModMesh._modValueType;

			//switch (srcModMesh._targetType)
			Color meshColor = srcModMesh._meshColor;
			if (!srcModMesh._isVisible)
			{
				meshColor.a = 0.0f;
			}

			_isUsePhysicParam = srcModMesh._isUsePhysicParam;
			if (_isUsePhysicParam)
			{
				_physicMeshParam = new apOptPhysicsMeshParam();
				_physicMeshParam.Bake(srcModMesh.PhysicParam);
				_physicMeshParam.Link(_portrait);
			}

			//추가 ExtraOption
			_isExtraValueEnabled = srcModMesh._isExtraValueEnabled;
			_extraValue.Bake(srcModMesh._extraValue);
			if(!_extraValue._isDepthChanged && !_extraValue._isTextureChanged)
			{
				_isExtraValueEnabled = false;
			}

			//Modifier Value에 맞게 Bake를 하자
			if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0)
			{

				Bake_VertexMorph(rootTransform,
									targetTransform,
									targetMesh,
									srcModMesh,
									srcModMesh._vertices,
									srcModMesh._pins,
									meshColor,
									srcModMesh._isVisible);
			}
			else if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.TransformMatrix) != 0)
			{
				if (srcModMesh._isMeshTransform)
				{
					Bake_MeshTransform(rootTransform,
										targetTransform,
										targetMesh,
										srcModMesh._transformMatrix,
										meshColor,
										srcModMesh._isVisible);
				}
				else
				{
					Bake_MeshGroupTransform(rootTransform,
											targetTransform,
											srcModMesh._transformMatrix,
											meshColor,
											srcModMesh._isVisible);
				}
			}
			else if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.BoneVertexWeightList) != 0)
			{
				//추가 : VertRig 데이터를 넣는다.
				bool isValidVertexRigsData = Bake_VertexRigs(rootTransform, targetTransform, targetMesh, srcModMesh._vertRigs);
				if(!isValidVertexRigsData)
				{
					//유효하지 않은 Rigging의 ModMesh가 있다.
					//Rig 데이터가 아무것도 없는 ModMesh는 Opt에서 오작동을 한다. (오류 검사를 안하므로)
					//Debug.LogError("Rig 데이터가 없는 ModMesh");
					string strTransform = rootTransform._name + " / " + targetTransform._name;
					Debug.LogError("AnyPortrait : Vertices with missing rigging data have been detected. Rigging for [" + strTransform + "] is not Bake.");
					return false;
				}
			}
			else if ((int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0
				|| (int)(srcModMesh._modValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Volume) != 0)
			{
				Bake_VertexWeights(rootTransform, targetTransform, targetMesh, srcModMesh._vertWeights);
			}
			else
			{
				Debug.LogError("연동 에러 : 알 수 없는 ModifierMesh 타입 : " + srcModMesh._modValueType);
				return false;
			}
			

			

			return true;
		}

		// Init - 값 넣기 (값복사)
		//--------------------------------------------
		//연동을 해주자 (apModifiedMesh에서 Init/Link 계열 함수)
		public void Bake_VertexMorph(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, 
										apModifiedMesh srcModMesh,
										List<apModifiedVertex> modVerts, 
										List<apModifiedPin> modPins,//추가 22.4.11
										Color meshColor, bool isVisible)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform;
			//Debug.LogError("Bake_VertexMorph");

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			if (_targetMesh == null)
			{
				Debug.LogError("Vert Morph인데 Target Mesh가 Null");
				Debug.LogError("Target Transform [" + _targetTransform.transform.name + "]");
			}

			_nVerts = modVerts.Count;
			_vertices = new apOptModifiedVertex[_nVerts];

			for (int i = 0; i < _nVerts; i++)
			{
				apOptModifiedVertex optModVert = new apOptModifiedVertex();
				apModifiedVertex srcModVert = modVerts[i];
				optModVert.Bake(srcModVert, _targetMesh);

				_vertices[i] = optModVert;
			}

			_meshColor = meshColor;
			_isVisible = isVisible;


			//추가 22.4.11 [v1.4.0]
			// Pin이 있는 경우엔 이를 포함한 모핑을 계산해야한다.
			int nModPins = modPins != null ? modPins.Count : 0;
			apMeshPinGroup pinGroup = null;
			if(srcModMesh._transform_Mesh != null
				&& srcModMesh._transform_Mesh._mesh != null)
			{
				pinGroup = srcModMesh._transform_Mesh._mesh._pinGroup;
			}
			//Debug.Log("핀 체크 결과 : 핀 개수 : " + nModPins + " / 핀 그룹 여부 : " + (pinGroup != null));

			if (nModPins > 0 && pinGroup != null)
			{
				//핀이 있는 경우 : Morph 모디파이어의 로직을 이용하여 위치 보정 (apModifierBase:1495를 참고하자)
				//Debug.Log("Opt ModMesh Bake > Pin 적용");

				apModifiedPin curModPin = null;
				apMeshPin curPin = null;
				
				//(1) 핀의 변경된 위치를 계산한다.
				for (int i = 0; i < nModPins; i++)
				{
					curModPin = modPins[i];
					curPin = curModPin._pin;
					//임시 변수 (Mod-Mid)에 Delta Pos를 적용
					curPin.SetTmpPos_ModMid(curPin._defaultPos + curModPin._deltaPos);
				}

				//(2) Mod Mid 값을 기준으로 커브 갱신
				pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.Update_ModMid);

				//(3) 버텍스 돌면서 Pin에 의한 위치 계산
				apModifiedVertex curModVert = null;
				apVertex curVert = null;

				//대상 OptModVert
				apOptModifiedVertex curOptModVert = null;
				
				apMeshPinVertWeight curPinVertWeight = null;
				apMeshPinVertWeight.VertPinPair curWeightPair = null;


				Vector2 vertPos_WOPin = Vector2.zero;

				Vector2 totalVertPosPinWeight = Vector2.zero;//가중치에 의한 버텍스 이동 결과 합
				Vector2 curVertPinWeightedPos = Vector2.zero;
				apMatrix3x3 curveMatrix = apMatrix3x3.identity;
				apMatrix3x3 curveVert2WorldMatrix = apMatrix3x3.identity;

				Vector2 vertPos_ResultWeighted = Vector2.zero;
				for (int i = 0; i < _nVerts; i++)
				{
					curOptModVert = _vertices[i];
					curModVert = modVerts[i];
					curVert = curModVert._vertex;
					
					//핀 그룹에서 가중치 정보를 가져운다.
					curPinVertWeight = pinGroup._vertWeights[i];
					int nPairs = curPinVertWeight._nPairs;
					if(nPairs == 0)
					{
						//이 버텍스는 핀과 연결되지 않았다.
						continue;
					}

					vertPos_WOPin = curVert._pos;

					totalVertPosPinWeight = Vector2.zero;//가중치에 의한 버텍스 이동 결과 합
					curVertPinWeightedPos = Vector2.zero;
					

					for (int iPair = 0; iPair < nPairs; iPair++)
					{
						curWeightPair = curPinVertWeight._vertPinPairs[iPair];

						if(!curWeightPair._isCurveWeighted)
						{
							//단일 핀과의 연결인 경우
							curVertPinWeightedPos = curWeightPair._linkedPin.TmpMultiplyVertPos(apMeshPin.TMP_VAR_TYPE.ModMid, ref vertPos_WOPin);
						}
						else
						{
							//핀과 핀 사이의 커브와 연결된 경우
							//커브 행렬
							curveMatrix = curWeightPair._linkedPin._nextCurve.GetCurveMatrix_Test(apMeshPin.TMP_VAR_TYPE.ModMid, curWeightPair._curveLerp);
							curveVert2WorldMatrix = curveMatrix * curWeightPair._curveDefaultMatrix_Inv;
							curVertPinWeightedPos = curveVert2WorldMatrix.MultiplyPoint(vertPos_WOPin);
						}

						//가중치를 이용하여 Weight에 의한 위치 합
						totalVertPosPinWeight += curVertPinWeightedPos * curWeightPair._weight;
					}

					//Total Weight를 이용하여 최종 위치를 계산하자
					vertPos_ResultWeighted = (vertPos_WOPin * (1.0f - curPinVertWeight._totalWeight)) + (totalVertPosPinWeight * curPinVertWeight._totalWeight);
									
					//기본 Pos를 빼서 Opt에 추가한다.
					Vector2 deltaPos = vertPos_ResultWeighted - curVert._pos;
					//Debug.Log("[" + i + "] Pin Delta Pos : " + deltaPos);
					curOptModVert.AddDeltaPos(deltaPos);
				}

			}
			

			
		}

		public void Bake_MeshTransform(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, apMatrix transformMatrix, Color meshColor, bool isVisible)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.MeshTransformOnly;

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			_transformMatrix = new apMatrix(transformMatrix);
			_meshColor = meshColor;
			_isVisible = isVisible;
		}

		public void Bake_MeshGroupTransform(apOptTransform rootTransform, apOptTransform targetTransform,
												apMatrix transformMatrix, Color meshColor, bool isVisible)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.MeshGroupTransformOnly;

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;

			_transformMatrix = new apMatrix(transformMatrix);

			_meshColor = meshColor;
			_isVisible = isVisible;
		}


		
		public bool Bake_VertexRigs(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, List<apModifiedVertexRig> modVertRigs)
		{
			//_targetType = apModifiedMesh.TARGET_TYPE.VertexWithMeshTransform;

			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			if (_targetMesh == null)
			{
				Debug.LogError("Vert Rig인데 Target Mesh가 Null");
				Debug.LogError("Target Transform [" + _targetTransform.transform.name + "]");
			}

			_nVertRigs = modVertRigs.Count;

			if(_nVertRigs == 0)
			{
				return false;
			}

			_vertRigs = new apOptModifiedVertexRig[_nVertRigs];
			for (int i = 0; i < _nVertRigs; i++)
			{
				apOptModifiedVertexRig optModVertRig = new apOptModifiedVertexRig();
				apModifiedVertexRig srcModVertRig = modVertRigs[i];
				bool rigVertBakeResult = optModVertRig.Bake(srcModVertRig, _targetMesh, _portrait);
				if(!rigVertBakeResult)
				{	
					return false;
				}

				_vertRigs[i] = optModVertRig;
			}

			_meshColor = Color.gray;
			_isVisible = true;

			return true;
		}


		public void Bake_VertexWeights(apOptTransform rootTransform, apOptTransform targetTransform,
										apOptMesh targetMesh, List<apModifiedVertexWeight> modVertWeights)
		{
			_rootTransform = rootTransform;
			_targetTransform = targetTransform;
			_targetMesh = targetMesh;

			if (_targetMesh == null)
			{
				Debug.LogError("Vert Rig인데 Target Mesh가 Null");
				Debug.LogError("Target Transform [" + _targetTransform.transform.name + "]");
			}

			_nVertWeights = modVertWeights.Count;
			_vertWeights = new apOptModifiedVertexWeight[_nVertWeights];
			for (int i = 0; i < _nVertWeights; i++)
			{
				apOptModifiedVertexWeight optModVertWeight = new apOptModifiedVertexWeight();
				apModifiedVertexWeight srcModVertWeight = modVertWeights[i];
				optModVertWeight.Bake(srcModVertWeight);

				_vertWeights[i] = optModVertWeight;
			}

			_meshColor = Color.gray;
			_isVisible = true;

		}

		// Functions
		//--------------------------------------------



		// Get / Set
		//--------------------------------------------
	}

}