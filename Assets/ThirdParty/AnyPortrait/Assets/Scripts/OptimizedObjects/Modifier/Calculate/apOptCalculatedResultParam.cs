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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apOptCalculatedResultParam
	{
		// Members
		//--------------------------------------------
		public apModifierParamSetGroup.SYNC_TARGET _inputType = apModifierParamSetGroup.SYNC_TARGET.Controller;

		public apCalculatedResultParam.CALCULATED_VALUE_TYPE _calculatedValueType = apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos;
		public apCalculatedResultParam.CALCULATED_SPACE _calculatedSpace = apCalculatedResultParam.CALCULATED_SPACE.Local;

		//연결된 모디파이어
		public apOptModifierUnitBase _linkedModifier = null;

		//타겟 Opt Transform
		public apOptTransform _targetOptTransform = null;
		public apOptTransform _ownerOptTransform = null;

		//타겟 Opt의 Child Mesh (존재한다면)
		public apOptMesh _targetOptMesh = null;

		//타겟 Bone
		public apOptBone _targetBone = null;


		//삭제 19.5.20 : 이 값을 사용하지 않음
		//Vertex 가중치 적용 데이터
		//public apOptParamSetGroupVertWeight _weightedVertexData = null;

		//결과값
		public Vector2[] _result_Positions = null;
		public apMatrix3x3[] _result_VertMatrices = null;//<<추가. 리깅용 결과 

		//변경 3.27 : apMatrix에서 apMatrixCal로 변경
		//public apMatrix _result_Matrix = new apMatrix();
		public apMatrixCal _result_Matrix = new apMatrixCal();


		public Color _result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		public bool _result_IsVisible = true;

		public float _resultWeight = 1.0f;

		//추가 : Controller에 의한 Bone IK가 적용되는 경우의 값
		//public float _result_BoneIKWeight = 0.0f;
		//public bool _isBoneIKWeightCalculated = false;

		//추가 : Vertex Morph 중 Physics (World)를 제외하고는 바로 계산하지 말고 별도의 파라미터를 둬서 계산한다.
		public List<apOptVertexRequest> _result_VertLocalPairs = null;

		//처리를 위한 임시값
		public Vector2[] _tmp_Positions = null;
		public apMatrix3x3[] _tmp_VertMatrices = null;

		public bool _isAvailable = true;

		public bool _isColorCalculated = true;//Color 계산이 이루어졌는가

		//추가 12.5 : ExtraOption
		public bool _isExtra_DepthChanged = false;
		public bool _isExtra_TextureChanged = false;
		public int _extra_DeltaDepth = 0;
		public int _extra_TextureDataID = -1;
		public apOptTextureData _extra_TextureData = null;



		public bool _isAnimModifier = false;
		private apAnimPlayMapping _linkedAnimPlayMapping = null;//추가 20.11.23 : 빠른 처리를 위해 Portrait의 AnimPlayMapping을 미리 연결하자

		public float _totalParamSetGroupWeight = 0.0f;

		public bool IsColorValueEnabled
		{
			get
			{
				if (_linkedModifier != null)
				{
					return _linkedModifier._isColorPropertyEnabled;
				}
				return false;
			}
		}


		//추가 12.5 : 애니메이션에 의한 가중치 처리시, 현재 Param이 "이전 키프레임"인지 "다음 키프레임"인지 알려준다.
		//변수는 ParamKeyValueSet에 포함된다.
		public enum AnimKeyPos : byte
		{
			NotCalculated,
			PrevKey,
			NextKey,
			ExactKey
		}

		/// <summary>
		/// 키프레임 / ControlParam에 따라 적용되는 데이터 Set
		/// 에디터에서의 ParamKeyValueSet에 해당한다.
		/// </summary>
		public class OptParamKeyValueSet
		{
			public apOptParamSetGroup _keyParamSetGroup = null;
			public apOptParamSet _paramSet = null;
			public apOptModifiedMesh _modifiedMesh = null;
			public apOptModifiedBone _modifiedBone = null;

			//변경 19.5.24 : ModifiedMesh대신 ModifiedMeshSet을 사용하는 것으로 변경
			public apOptModifiedMeshSet _modifiedMeshSet = null;

			public float _dist = -1.0f;
			public float _weight = -1.0f;
			public bool _isCalculated = false;
			public int _layerIndex = -1;

			//추가 : RotationBias를 계산한다. : 기본값 false
			public bool _isAnimRotationBias = false;//<<Animation Keyframe과 연동된 경우, "현재 처리"중에 회전 각도 Bias가 포함되어 있는가
			public int _animRotationBiasAngle = 0;
			public int _animRotationBiasAngle_Prev = 0;

			public apMatrix _animRotationBiasedMatrix = new apMatrix();

			//추가 11.29
			public AnimKeyPos _animKeyPos = AnimKeyPos.NotCalculated;

			//계산용 
			private apOptModifiedMesh_Transform _subModMesh_Transform = null;

			/// <summary>
			/// ModMesh와 연동되는 ParamKeyValue 생성
			/// </summary>
			public OptParamKeyValueSet(apOptParamSetGroup keyParamSetGroup, apOptParamSet paramSet, apOptModifiedMesh modifiedMesh)
			{
				_keyParamSetGroup = keyParamSetGroup;
				_paramSet = paramSet;
				_modifiedMesh = modifiedMesh;
				_modifiedMeshSet = null;
				_layerIndex = _keyParamSetGroup._layerIndex;

				_modifiedBone = null;

				//추가 : RotationBias
				_isAnimRotationBias = false;
				_animRotationBiasAngle = 0;
				_animRotationBiasAngle_Prev = -1;
				_animRotationBiasedMatrix = new apMatrix();
			}



			/// <summary>
			/// ModMeshSet과 연동되는 ParamKeyValue 생성 (19.5.24)
			/// </summary>
			public OptParamKeyValueSet(apOptParamSetGroup keyParamSetGroup, apOptParamSet paramSet, apOptModifiedMeshSet modifiedMeshSet)
			{
				_keyParamSetGroup = keyParamSetGroup;
				_paramSet = paramSet;
				_modifiedMesh = null;
				_modifiedMeshSet = modifiedMeshSet;
				_layerIndex = _keyParamSetGroup._layerIndex;

				_modifiedBone = null;

				//추가 : RotationBias
				_isAnimRotationBias = false;
				_animRotationBiasAngle = 0;
				_animRotationBiasAngle_Prev = -1;
				_animRotationBiasedMatrix = new apMatrix();
			}

			/// <summary>
			/// ModBone과 연동되는 ParamKeyValue 생성
			/// </summary>
			public OptParamKeyValueSet(apOptParamSetGroup keyParamSetGroup, apOptParamSet paramSet, apOptModifiedBone modifiedBone)
			{
				_keyParamSetGroup = keyParamSetGroup;
				_paramSet = paramSet;
				_modifiedMesh = null;
				_modifiedMeshSet = null;
				_layerIndex = _keyParamSetGroup._layerIndex;

				_modifiedBone = modifiedBone;

				//추가 : RotationBias
				_isAnimRotationBias = false;
				_animRotationBiasAngle = 0;
				_animRotationBiasAngle_Prev = -1;
				_animRotationBiasedMatrix = new apMatrix();
			}

			public void ReadyToCalculate()
			{
				_dist = -1.0f;
				_weight = -1.0f;
				_isCalculated = false;

				//RotationBias
				_isAnimRotationBias = false;

				//추가 12.5 : 애니메이션 키프레임 중 어디에 속했는지 계산
				_animKeyPos = AnimKeyPos.NotCalculated;
			}

			/// <summary>
			/// Keyframe에 Rotation Bias 설정이 있는 경우 관련 변수를 갱신한다.
			/// </summary>
			/// <param name="rotationBias"></param>
			/// <param name="rotationCount"></param>
			public void SetAnimRotationBias(apAnimKeyframe.ROTATION_BIAS rotationBias, int rotationCount)
			{
				
				if(rotationBias == apAnimKeyframe.ROTATION_BIAS.CW)
				{
					_isAnimRotationBias = true;
					_animRotationBiasAngle = -360 * rotationCount;
				}
				else if(rotationBias == apAnimKeyframe.ROTATION_BIAS.CCW)
				{
					_isAnimRotationBias = true;
					_animRotationBiasAngle = 360 * rotationCount;
				}
				else
				{
					_isAnimRotationBias = false;
					_animRotationBiasAngle = 0;
				}

				if(_animRotationBiasAngle_Prev != _animRotationBiasAngle && _isAnimRotationBias)
				{
					if(_animRotationBiasedMatrix == null)
					{
						_animRotationBiasedMatrix = new apMatrix();
					}

					if(_modifiedMesh != null)
					{
						_animRotationBiasedMatrix.SetTRS(
							_modifiedMesh._transformMatrix._pos,
							_modifiedMesh._transformMatrix._angleDeg + _animRotationBiasAngle,
							_modifiedMesh._transformMatrix._scale,
							false);
					}
					else if(_modifiedMeshSet != null)
					{
						//추가됨 19.5.24 : ModMeshSet으로 계산하기
						_subModMesh_Transform = _modifiedMeshSet.SubModMesh_Transform;
						if (_subModMesh_Transform != null)
						{
							_animRotationBiasedMatrix.SetTRS(
									_subModMesh_Transform._transformMatrix._pos,
									_subModMesh_Transform._transformMatrix._angleDeg + _animRotationBiasAngle,
									_subModMesh_Transform._transformMatrix._scale,
									false);
						}
						else
						{
							Debug.LogError("TODO : 에러 : 애니메이션에 SubModMeshSet_Transform이 없다.");
							_animRotationBiasedMatrix.SetIdentity();
						}
					}
					else if(_modifiedBone != null)
					{
						_animRotationBiasedMatrix.SetTRS(
							_modifiedBone._transformMatrix._pos,
							_modifiedBone._transformMatrix._angleDeg + _animRotationBiasAngle,
							_modifiedBone._transformMatrix._scale,
							false);
					}
					else
					{
						_animRotationBiasedMatrix.SetIdentity();
					}

					_animRotationBiasedMatrix.MakeMatrix();
					_animRotationBiasAngle_Prev = _animRotationBiasAngle;
				}
			}

			public void TurnOffRotationBias()
			{
				_isAnimRotationBias = false;
				_animRotationBiasAngle = 0;
			}

			public apMatrix AnimRotationBiasedMatrix
			{
				get
				{
					return _animRotationBiasedMatrix;
				}
			}
		}

		public List<OptParamKeyValueSet> _paramKeyValues = new List<OptParamKeyValueSet>();
		public List<apOptCalculatedResultParamSubList> _subParamKeyValueList = new List<apOptCalculatedResultParamSubList>();
		public apOptCalculatedResultParamSubList[] _subParamKeyValueList_AnimSync = null;//추가 20.11.23 : 애니메이션 모디파이어는 이걸 사용하자. AnimClip의 순서와 동일하게 생성된 배열이다. (중요)

		private bool _isVertexLocalMorph = false;
		private bool _isVertexRigging = false;

		// Init
		//--------------------------------------------
		public apOptCalculatedResultParam(apCalculatedResultParam.CALCULATED_VALUE_TYPE calculatedValueType,
											apCalculatedResultParam.CALCULATED_SPACE calculatedSpace,
											apOptModifierUnitBase linkedModifier,
											apOptTransform targetOptTranform,
											apOptTransform ownerOptTranform,
											apOptMesh targetOptMesh,
											apOptBone targetBone,
											apAnimPlayMapping linkedAnimPlayMapping//추가 20.11.23 : 빠른 애니메이션 처리를 위함
											//,apOptParamSetGroupVertWeight weightedVertData //<< 사용안함 19.5.20
											)
		{
			_calculatedValueType = calculatedValueType;
			_calculatedSpace = calculatedSpace;

			//TODO 여기서부터 작업하자
			_linkedModifier = linkedModifier;
			_targetOptTransform = targetOptTranform;
			_ownerOptTransform = ownerOptTranform;
			_targetOptMesh = targetOptMesh;
			_targetBone = targetBone;//<<추가

			_paramKeyValues.Clear();
			_subParamKeyValueList.Clear();
			_subParamKeyValueList_AnimSync = null;//애니메이션 리스트가 생기기 전까지는 null

			//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
			//_weightedVertexData = weightedVertData;

			_isVertexLocalMorph = false;
			_isVertexRigging = false;

			_linkedAnimPlayMapping = linkedAnimPlayMapping;

			//Vertex 데이터가 들어간 경우 Vert 리스트를 만들어주자
			if ((int)(_calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
			{
				int nPos = 0;
				//이전
				//if (_targetOptMesh.LocalVertPositions != null)
				//{
				//	nPos = _targetOptMesh.LocalVertPositions.Length;
				//}

				//변경 21.3.11 : 양면인 경우, RenderVertices와 LocalVertPositions가 다를 수 있다.
				if (_targetOptMesh.RenderVertices != null)
				{
					nPos = _targetOptMesh.RenderVertices.Length;
				}
				//if(_targetOptMesh.LocalVertPositions.Length != _targetOptMesh.RenderVertices.Length)
				//{
				//	Debug.LogError("양면 메시 발견 : Local Verts : " + _targetOptMesh.LocalVertPositions.Length + " / Render Verts : " + _targetOptMesh.RenderVertices.Length);
				//}

				_result_Positions = new Vector2[nPos];
				_tmp_Positions = new Vector2[nPos];

				for (int i = 0; i < nPos; i++)
				{
					_result_Positions[i] = Vector2.zero;
					_tmp_Positions[i] = Vector2.zero;
				}

				if (_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					_result_VertMatrices = new apMatrix3x3[nPos];
					_tmp_VertMatrices = new apMatrix3x3[nPos];

					for (int i = 0; i < nPos; i++)
					{
						_result_VertMatrices[i].SetIdentity();
						_tmp_VertMatrices[i].SetIdentity();
					}
				}

				//if(_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.Morph ||
				//	_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
				//{
				//	//최적화를 위한 VertLocalPair를 만든다. 개수는 ParamSetGroup 만큼
				//	_result_VertLocalPairs = new apOptVertexRequest[_linkedModifier._paramSetGroupList.Count];
				//	for (int iVLP = 0; iVLP < _result_VertLocalPairs.Length; iVLP++)
				//	{
				//		apOptVertexRequest newRequest = new apOptVertexRequest();
				//		newRequest.InitVertLocalPair(_linkedModifier._paramSetGroupList[iVLP]);

				//		_result_VertLocalPairs[iVLP] = newRequest;
				//	}
				//}

				if (_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph ||
						_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.Morph)
				{
					_isVertexLocalMorph = true;
				}
				else if (_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					_isVertexRigging = true;
				}
			}

		}

		//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
		//public void LinkWeightedVertexData(apOptParamSetGroupVertWeight weightedVertData)
		//{
		//	_weightedVertexData = weightedVertData;
		//}


		/// <summary>
		/// ParamSet을 받아서 SubList와 연동한다.
		/// </summary>
		/// <param name="paramSet"></param>
		/// <returns></returns>
		public void AddParamSetAndModifiedValue(apOptParamSetGroup paramSetGroup,
												apOptParamSet paramSet,
												apOptModifiedMesh modifiedMesh,
												apOptModifiedBone modifiedBone,
												apOptModifiedMeshSet modifiedMeshSet//<<추가됨 19.5.24
												
												)
		{
			OptParamKeyValueSet existSet = GetParamKeyValue(paramSet);

			if (existSet != null)
			{
				//이미 존재한 값이라면 패스
				return;
			}

			

			//새로운 KeyValueSet을 만들어서 리스트에 추가하자
			//Mod Mesh 또는 Mod Bone 둘중 하나를 넣어서 ParamKeyValueSet을 구성하자
			OptParamKeyValueSet newKeyValueSet = null;
			if (modifiedMesh != null)
			{
				newKeyValueSet = new OptParamKeyValueSet(paramSetGroup, paramSet, modifiedMesh);
			}
			else if (modifiedMeshSet != null)
			{
				newKeyValueSet = new OptParamKeyValueSet(paramSetGroup, paramSet, modifiedMeshSet);
			}
			else if (modifiedBone != null)
			{
				newKeyValueSet = new OptParamKeyValueSet(paramSetGroup, paramSet, modifiedBone);
			}
			else
			{
				Debug.LogError("AddParamSetAndModifiedMesh Error : ModifiedMesh와 ModifiedBone이 모두 Null이다.");
				return;
			}

			_paramKeyValues.Add(newKeyValueSet);

			apOptCalculatedResultParamSubList targetSubList = null;

			apOptCalculatedResultParamSubList existSubList = _subParamKeyValueList.Find(delegate (apOptCalculatedResultParamSubList a)
			{
				   return a._keyParamSetGroup == paramSetGroup;
			});

			//같이 묶여서 작업할 SubList가 있는가
			if (existSubList != null)
			{
				targetSubList = existSubList;
			}
			else
			{
				//없으면 만든다.
				targetSubList = new apOptCalculatedResultParamSubList(this, _isVertexLocalMorph, _isVertexRigging);
				targetSubList.SetParamSetGroup(paramSetGroup);

				_subParamKeyValueList.Add(targetSubList);

				if (_isVertexLocalMorph || _isVertexRigging)
				{
					//VertexRequest를 전체 리스트로 추가하여 관리하자
					if (_result_VertLocalPairs == null)
					{
						_result_VertLocalPairs = new List<apOptVertexRequest>();
					}

					_result_VertLocalPairs.Add(targetSubList._vertexRequest);
				}
			}

			//해당 SubList에 위에서 만든 KeyValueSet을 추가하자
			if (targetSubList != null)
			{
				targetSubList.AddParamKeyValueSet(newKeyValueSet);
			}

			_isAnimModifier = (paramSetGroup._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame);

			//추가 20.11.23 : 애니메이션 모디파이어라면 SubList를 AnimClip의 순서에 맞게 만들자.
			//만약 없다면 null로 두더라도, 순서를 유지하여 고정 배열로 만들어야 한다.
			//새로 생성하거나 추가될 SubList가 몇번째 Sync인지도 계산하자
			
			if (_isAnimModifier)
			{
				if (_subParamKeyValueList_AnimSync == null)
				{
					_subParamKeyValueList_AnimSync = new apOptCalculatedResultParamSubList[_linkedAnimPlayMapping._nAnimClips];
				}

				//애니메이션 클립의 인덱스를 가져오자
				int iAnimSync = _linkedAnimPlayMapping.GetAnimClipIndex(paramSetGroup._keyAnimClip);
				_subParamKeyValueList_AnimSync[iAnimSync] = targetSubList;//적절한 슬롯의 인덱스에 SubList를 할당한다.
			}
		}



		



		public void SortSubList()
		{
			_subParamKeyValueList.Sort(delegate (apOptCalculatedResultParamSubList a, apOptCalculatedResultParamSubList b)
			{
				if (a._keyParamSetGroup == null || b._keyParamSetGroup == null)
				{
					return 0;
				}

				return a._keyParamSetGroup._layerIndex - b._keyParamSetGroup._layerIndex;//오른차순 정렬
			});

			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				_subParamKeyValueList[i].MakeMetaData();
			}
		}


		public void ResetVerticesOnBake()
		{
			//추가 3.22 : 기존의 Vertex를 다시 갱신
			if ((int)(_calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
			{
				int nMeshVerts = 0;
				int nResultVerts = 0;
				int nResultMatrices = 0;


				//if (_targetOptMesh.LocalVertPositions != null)
				//{
				//	nMeshVerts = _targetOptMesh.LocalVertPositions.Length;
				//}

				//변경 21.3.11 : 양면 메시의 경우 RenderVertices를 이용해야한다.
				if (_targetOptMesh.RenderVertices != null)
				{
					nMeshVerts = _targetOptMesh.RenderVertices.Length;
				}

				if (_result_Positions != null)
				{
					nResultVerts = _result_Positions.Length;
				}
				if (_result_VertMatrices != null)
				{
					nResultMatrices = _result_VertMatrices.Length;
				}

				if (nMeshVerts != nResultVerts)
				{
					//Debug.LogError("갱신되는 CalResultParam에서 Mesh Vert 개수가 불일치함을 발견했다.");
					//Debug.Log("Mesh Vertex : " + nMeshVerts + " / Result Vertex : " + nResultVerts);

					_result_Positions = new Vector2[nMeshVerts];
					_tmp_Positions = new Vector2[nMeshVerts];

					for (int i = 0; i < nMeshVerts; i++)
					{
						_result_Positions[i] = Vector2.zero;
						_tmp_Positions[i] = Vector2.zero;
					}
				}

				if (nMeshVerts != nResultMatrices && _linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//Debug.LogError("갱신되는 CalResultParam에서 Rigging Matrix 개수가 불일치함을 발견했다.");
					//Debug.Log("Mesh Vertex : " + nMeshVerts + " / Result Matrix : " + nResultMatrices);

					_result_VertMatrices = new apMatrix3x3[nMeshVerts];
					_tmp_VertMatrices = new apMatrix3x3[nMeshVerts];

					for (int i = 0; i < nMeshVerts; i++)
					{
						_result_VertMatrices[i].SetIdentity();
						_tmp_VertMatrices[i].SetIdentity();
					}
				}


				if (_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph ||
						_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.Morph)
				{
					_isVertexLocalMorph = true;
				}
				else if (_linkedModifier._modifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					_isVertexRigging = true;
				}
			}
		}


		// Functions
		//--------------------------------------------
		public void InitCalculate()
		{
			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				_subParamKeyValueList[i].InitCalculate();
			}

			_totalParamSetGroupWeight = 0.0f;
		}

		/// <summary>
		/// Calculate Result 계산을 한다. (키프레임이나 컨트롤 파라미터 가중치)
		/// </summary>
		/// <returns>True : 이 CalResult는 업데이트 해야한다. / False : 모든 Sub ParamValue가 업데이트 되지 않는다.</returns>
		public bool Calculate()
		{
			bool isUpdatable = false;

			_totalParamSetGroupWeight = 0.0f;

//#if UNITY_EDITOR
//			Profiler.BeginSample("Calcualte Result Param - Calculate");
//#endif
			bool isResult = false;

			if (_isAnimModifier)
			{
				bool isNeedSort = false;
				//추가
				//애니메이션 타입인 경우
				//재정렬이 필요한지 체크한다.
				for (int i = 0; i < _subParamKeyValueList.Count; i++)
				{
					//여기서 애니메이션을 계산하고 UnitWeight를 LayerWeight로 저장한다.
					if (_subParamKeyValueList[i].UpdateAnimLayer())
					{
						//Layer의 변화가 있었다.
						//Sort를 하자
						isNeedSort = true;
					}
				}
				if (isNeedSort)
				{
					//Debug.Log("Reorder / AnimClip");
					//정렬을 다시 하자
					SortSubList();
				}
			}


			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				isResult = _subParamKeyValueList[i].Calculate();
				if (isResult)
				{
					isUpdatable = true;
				}
			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif

			return isUpdatable;
		}


		//추가 20.11.13 : 애니메이션용 Calculate 함수 > 최적화 코드가 추가되어 로직이 많이 사라졌다.
		public bool Calculate_AnimMod()
		{
			bool isUpdatable = false;

			_totalParamSetGroupWeight = 0.0f;


			bool isResult = false;

			//중요! 애니메이션 최적화에서 이 코드를 삭제할 수 있어야 한다.
			//if (_isAnimModifier)
			//{
			//	bool isNeedSort = false;
			//	//추가
			//	//애니메이션 타입인 경우
			//	//재정렬이 필요한지 체크한다.
			//	for (int i = 0; i < _subParamKeyValueList.Count; i++)
			//	{
			//		//여기서 애니메이션을 계산하고 UnitWeight를 LayerWeight로 저장한다.
			//		if (_subParamKeyValueList[i].UpdateAnimLayer())
			//		{
			//			//Layer의 변화가 있었다.
			//			//Sort를 하자
			//			isNeedSort = true;
			//		}
			//	}
			//	if (isNeedSort)
			//	{
			//		//Debug.Log("Reorder / AnimClip");
			//		//정렬을 다시 하자
			//		SortSubList();
			//	}
			//}


			//이전 : 모든 SubList를 Calculate
			//for (int i = 0; i < _subParamKeyValueList.Count; i++)
			//{
			//	isResult = _subParamKeyValueList[i].Calculate();
			//	if (isResult)
			//	{
			//		isUpdatable = true;
			//	}
			//}

			//변경 20.11.23
			//_linkedAnimPlayMapping를 이용해서 "재생 중인 애니메이션의 SubList"만 계산
			int iSubList = 0;
			apAnimPlayMapping.LiveUnit curUnit = null;
			
			for (int i = 0; i < _linkedAnimPlayMapping._nAnimClips; i++)
			{
				curUnit = _linkedAnimPlayMapping._liveUnits_Sorted[i];
				if(!curUnit._isLive)
				{
					//재생 종료
					//이 뒤는 모두 재생이 안되는 애니메이션이다.
					break;
				}
				iSubList = curUnit._animIndex;//현재 재생중인 AnimClip에 해당하는 SubList의 인덱스

				if(_subParamKeyValueList_AnimSync[iSubList] == null)
				{
					//이게 Null이라는 것은, 이 AnimClip에 대한 TimelineLayer와 Mod는 없다는 것
					continue;
				}

				//이제 SubList의 키프레임들을 계산하자. Anim전용 Calculate 함수를 이용할 것
				isResult = _subParamKeyValueList_AnimSync[iSubList].Calculate_AnimMod();
				

				if(isResult)
				{
					isUpdatable = true;
				}
				
			}
			
			

			return isUpdatable;
		}





		// Get / Set
		//--------------------------------------------
		public int ModifierLayer { get { return _linkedModifier._layer; } }
		public apModifierBase.BLEND_METHOD ModifierBlendMethod { get { return _linkedModifier._blendMethod; } }
		public float ModifierWeight
		{
			get
			{
				//return _linkedModifier._layerWeight;

				//수정 >> 
				return Mathf.Clamp01(_linkedModifier._layerWeight * Mathf.Clamp01(_totalParamSetGroupWeight));
			}
		}

		//추가 20.11.26 : Rigging 최적화를 위해 _totalParamSetGroupWeight를 계산하지 않는다. Modifier Weight만 계산할 것
		public float ModifierWeightForRigging
		{
			get
			{
				return Mathf.Clamp01(_linkedModifier._layerWeight);
			}
		}


		public bool IsModifierAvailable { get { return _isAvailable; } }

		public OptParamKeyValueSet GetParamKeyValue(apOptParamSet paramSet)
		{
			return _paramKeyValues.Find(delegate (OptParamKeyValueSet a)
			{
				return a._paramSet == paramSet;
			});
		}
	}

}