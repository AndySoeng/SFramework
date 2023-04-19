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

	public class apCalculatedResultParam
	{
		// Members
		//--------------------------------------------------
		//입력 타입
		public apModifierParamSetGroup.SYNC_TARGET _inputType = apModifierParamSetGroup.SYNC_TARGET.Controller;

		//입력 키값에 따라 계산을 복합적으로 해야한다.
		//Param 값이 아닌 입력 키값을 따로 잡아야 한다.
		public class ParamInputKey
		{
			//입력 타입에 맞게 Key를 다르게 가져온다.
			//1. Control Param인 경우
			public apControlParam _controlParam = null;

			//2. 키프레임인 경우 : TODO
			//3. Static인 경우 -> 없음 (Input에 따라 구분되지 않는다.)

			public ParamInputKey(apControlParam controlParam)
			{
				_controlParam = controlParam;
			}
		}

		/// <summary>
		/// Modifier의 결과가 적용되는 결과 값이 종류
		/// 여러개를 동시에 적용할 수 있다.
		/// </summary>
		[Flags]
		public enum CALCULATED_VALUE_TYPE
		{
			VertexPos = 1,
			TransformMatrix = 2,
			Color = 4,
		}

		public CALCULATED_VALUE_TYPE _calculatedValueType = CALCULATED_VALUE_TYPE.VertexPos;

		/// <summary>
		/// 색상 처리 옵션은 기본값이 아니라 선택값이 아니어서 사용하지 않을 수도 있다.
		/// </summary>
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

		/// <summary>
		/// Modifier의 결과가 적용되는 Space
		/// 같은 Space에 대해서 + Modifier이 레이어 순서에 맞게 중첩하여 결과를 만들어낸다.
		/// </summary>
		public enum CALCULATED_SPACE
		{
			/// <summary>Local Transform 이전에 처리되는 단계이다. Vertex의 위치를 직접 바꿀때 사용</summary>
			Object = 0,
			/// <summary>MeshTransform의 첫 Transform에서 처리되는 단계이다.</summary>
			Local = 1,
			///// <summary>연결된 모디파이어의 MeshGroup에서 처리될때 같이 처리한다.</summary>
			//Parent,
			/// <summary>모든 처리후 World Transform을 만들때 처리된다.</summary>
			World = 2,
			/// <summary>
			/// Object보다 이전에 처리되는 단계. Bone Rigging의 경우 Vertex Morphing과 별도로 처리를 한다.
			/// </summary>
			Rigging = 3,

		}
		public CALCULATED_SPACE _calculatedSpace = CALCULATED_SPACE.Local;





		//연결된 모디파이어
		public apModifierBase _linkedModifier = null;

		//연결된 

		//타겟 렌더 유닛
		public apRenderUnit _targetRenderUnit = null;
		public apRenderUnit _ownerRenderUnit = null;

		//타겟 Bone
		public apBone _targetBone = null;

		//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
		//Vertex 가중치 적용 데이터
		//public apModifierParamSetGroupVertWeight _weightedVertexData = null;

		//결과값
		// 여기에 직접 처리를 해주자
		public Vector2[] _result_Positions = null;
		public apMatrix3x3[] _result_VertMatrices = null;//<<추가. 리깅용 결과 

		//추가 22.3.28 [v1.4.0] Pin의 월드 계산용 (미리보기용)
		public Vector2[] _result_PinPositions = null;

		//변경 3.26 : apMatrix에서 apMatrixCal로 변경
		//public apMatrix _result_Matrix = new apMatrix();
		public apMatrixCal _result_Matrix = new apMatrixCal();

		public Color _result_Color = new Color(0.5f, 0.5f, 0.5f, 1f);
		public bool _result_IsVisible = true;

		//>>> Bone IK Weight를 삭제. Control Param에 따르도록 한다.
		//변경 : Controller에 의한 Bone IK가 적용되는 경우의 값
		//public float _result_BoneIKWeight = 0.0f;
		//public bool _isBoneIKWeightCalculated = false;

		////처리를 위한 임시값도 만들자
		//public List<Vector2> _tmp_Positions = null;

		//지금 적용 가능한 파라미터인가 (등록은 해도 적용은 안할 수 있다.)
		//public bool _isAvailable = true;//이전
		public bool _isMainCalculated = true;//변경. 22.5.11 : Color / Extra를 제외한 처리가 발생했는가

		public bool _isColorCalculated = true;//Color 계산이 이루어졌는가

		//추가 11.29 : ExtraOption
		public bool _isExtra_DepthChanged = false;
		public bool _isExtra_TextureChanged = false;
		public int _extra_DeltaDepth = 0;
		public int _extra_TextureDataID = -1;
		public apTextureData _extra_TextureData = null;

		//변경
		//Weight를 Transform / Color 계열로 나누자
		//public float _totalParamSetGroupWeight = 0.0f;//<<이전
		public float _totalParamSetGroupWeight_Transform = 0.0f;
		public float _totalParamSetGroupWeight_Color = 0.0f;


		//추가 11.29 : 애니메이션에 의한 가중치 처리시, 현재 Param이 "이전 키프레임"인지 "다음 키프레임"인지 알려준다.
		//변수는 ParamKeyValueSet에 포함된다.
		public enum AnimKeyPos : byte
		{
			NotCalculated,
			PrevKey,
			NextKey,
			ExactKey
		}

		/// <summary>
		/// 키프레임에 해당하는 입력/변환값 Set
		/// </summary>
		public class ParamKeyValueSet
		{
			public apModifierParamSetGroup _keyParamSetGroup = null;
			public apModifierParamSet _paramSet = null;
			public apModifiedMesh _modifiedMesh = null;
			public apModifiedBone _modifiedBone = null;//<<추가
													   //TODO : ParamKey를 Mesh 기준이 아닌 Bone 기준으로도 등록 + 처리해야한다.

			//계산이 유효한 파라미터인가
			//public bool _isActive_InEditorExclusive = true;
			public bool IsActive
			{
				get
				{
					if (_keyParamSetGroup != null &&
						_keyParamSetGroup.IsCalculateEnabled)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}

			//계산시에 쓰이는 Weight값
			public float _dist = -1.0f;
			public float _weight = -1.0f;
			public float _weightBase = -1.0f;//전체 Weight를 계산하기 전에 "보간식 자체의 가중치"를 지정할때 사용하는 값
			public bool _isCalculated = false;

			public int _layerIndx = -1;

			//RotationBias를 계산한다. : 기본값 false
			public bool _isAnimRotationBias = false;//<<Animation Keyframe과 연동된 경우, "현재 처리"중에 회전 각도 Bias가 포함되어 있는가
			public int _animRotationBiasAngle = 0;
			public int _animRotationBiasAngle_Prev = 0;

			public apMatrix _animRotationBiasedMatrix = new apMatrix();

			//추가 11.29
			public AnimKeyPos _animKeyPos = AnimKeyPos.NotCalculated;


			public ParamKeyValueSet(apModifierParamSetGroup keyParamSetGroup, apModifierParamSet paramSet, apModifiedMesh modifiedValue)
			{
				_keyParamSetGroup = keyParamSetGroup;
				_paramSet = paramSet;
				_modifiedMesh = modifiedValue;
				_layerIndx = _paramSet._parentParamSetGroup._layerIndex;

				_modifiedBone = null;

				//추가 : RotationBias
				_isAnimRotationBias = false;
				_animRotationBiasAngle = 0;
				_animRotationBiasAngle_Prev = -1;
				_animRotationBiasedMatrix = new apMatrix();
			}
			public ParamKeyValueSet(apModifierParamSetGroup keyParamSetGroup, apModifierParamSet paramSet, apModifiedBone modifiedValue)
			{
				_keyParamSetGroup = keyParamSetGroup;
				_paramSet = paramSet;
				_modifiedMesh = null;
				_modifiedBone = modifiedValue;
				_layerIndx = _paramSet._parentParamSetGroup._layerIndex;

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

				//추가 11.29 : 애니메이션의 키프레임 중 어디에 속했는지 계산하여 이후에 Extra Option에 이용한다.
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
				//에디터에서는 최적화가 아니라 언제 바뀔지 모르므로 항상 갱신
				//if(_animRotationBiasAngle_Prev != _animRotationBiasAngle && _isAnimRotationBias)
				if(_isAnimRotationBias)
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
							false
						);
					}
					else if(_modifiedBone != null)
					{
						_animRotationBiasedMatrix.SetTRS(
							_modifiedBone._transformMatrix._pos,
							_modifiedBone._transformMatrix._angleDeg + _animRotationBiasAngle,
							_modifiedBone._transformMatrix._scale,
							false
						);
					}
					else
					{
						_animRotationBiasedMatrix.SetIdentity();
					}

					_animRotationBiasedMatrix.MakeMatrix();
					_animRotationBiasAngle_Prev = _animRotationBiasAngle;
				}
			}

			public apMatrix AnimRotationBiasedMatrix
			{
				get
				{
					return _animRotationBiasedMatrix;
				}
			}

			#region [미사용 코드] Debug + ParamSet 설계 일부 변경되어 ParamSetGroup에서 변수 가져와야함
			//public string GetDebugName
			//{
			//	get
			//	{
			//		return "[" + _paramSet._controlKeyName + " / " + _paramSet.ControlParamValue + " > " + _modfiedValue._transformUniqueID + "]";
			//	}
			//} 
			#endregion
		}


		public List<ParamKeyValueSet> _paramKeyValues = new List<ParamKeyValueSet>();
		public List<apCalculatedResultParamSubList> _subParamKeyValueList = new List<apCalculatedResultParamSubList>();
		//public List<ParamKeyValueSet> _paramKeyValues = new List<ParamKeyValueSet>();


		// Init
		//--------------------------------------------------
		public apCalculatedResultParam(CALCULATED_VALUE_TYPE calculatedValueType,
										CALCULATED_SPACE calculatedSpace,
										apModifierBase linkedModifier,
										apRenderUnit targetRenderUnit,
										apRenderUnit ownerRenderUnit,//<<추가 10.2 : Modifier를 가지고 있었던 RenderUnit
										apBone targetBone
										//apModifierParamSetGroupVertWeight weightedVertData//삭제 19.5.20
			)
		{
			_calculatedValueType = calculatedValueType;
			_calculatedSpace = calculatedSpace;

			_linkedModifier = linkedModifier;
			_targetRenderUnit = targetRenderUnit;
			_ownerRenderUnit = ownerRenderUnit;

			_targetBone = targetBone;

			//일단 초기화 (추가 22.3.28)
			_result_Positions = null;
			_result_VertMatrices = null;
			_result_PinPositions = null;

			//삭제 19.5.20 : _weightedVertexData 변수 삭제됨
			//_weightedVertexData = weightedVertData;

			

			//처리 타입이 Vertex 계열이면 Vertex List를 준비해야한다.
			if ((int)(_calculatedValueType & CALCULATED_VALUE_TYPE.VertexPos) != 0)
			{
				int nPos = 0;
				if (_targetRenderUnit._meshTransform != null && _targetRenderUnit._meshTransform._mesh != null)
				{
					nPos = _targetRenderUnit._meshTransform._mesh._vertexData.Count;
				}

				_result_Positions = new Vector2[nPos];
				//for (int i = 0; i < nPos; i++)
				//{
				//	_result_Positions[i] = Vector2.zero;
				//}
				
				//변경 22.3.28 [v1.4.0] 초기화 코드 변경
				Array.Clear(_result_Positions, 0, nPos);

				//추가 : 만약 리깅타입이면 Vertex 개수만큼의 Matrix를 만들어야 한다.
				if(_linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					_result_VertMatrices = new apMatrix3x3[nPos];

					//이전
					//for (int i = 0; i < nPos; i++)
					//{
					//	_result_VertMatrices[i] = apMatrix3x3.zero;
					//}

					//변경 22.3.28 [v1.4.0] 초기화 코드 변경
					Array.Clear(_result_VertMatrices, 0, nPos);
				}

				//추가 22.3.28 [v1.4.0] Pin 결과 미리보기
				//단, Morph 타입만 해당.
				if(_linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Morph
					|| _linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
				{
					int nPins = 0;
					if (_targetRenderUnit._meshTransform != null && _targetRenderUnit._meshTransform._mesh != null)
					{
						apMeshPinGroup pinGroup = _targetRenderUnit._meshTransform._mesh._pinGroup;
						if(pinGroup != null)
						{
							nPins = pinGroup.NumPins;
						}
					}
					if(nPins > 0)
					{
						_result_PinPositions = new Vector2[nPins];
						Array.Clear(_result_PinPositions, 0, nPins);
					}
					else
					{
						_result_PinPositions = null;
					}
					
				}
			}
		}

		/// <summary>
		/// 기존에 있는 ResultParam을 다시 Link하는 경우 Vertex 개수가 바뀌었는데 이를 갱신하는 부분이 없다.
		/// LinkModifierStackToRenderunitCaluclateStack 함수에서 이 함수를 호출해주자.
		/// 버텍스 모디파이어가 아니거나 변경 내역이 없다면 아무 일도 하지 않는다.
		/// </summary>
		public void RefreshResultVertices()
		{
			//처리 타입이 Vertex 계열이면 Vertex List를 준비해야한다.
			if ((int)(_calculatedValueType & CALCULATED_VALUE_TYPE.VertexPos) != 0)
			{
				int nPos = 0;
				if (_targetRenderUnit._meshTransform != null && _targetRenderUnit._meshTransform._mesh != null)
				{
					nPos = _targetRenderUnit._meshTransform._mesh._vertexData.Count;
				}

				if (_result_Positions == null || _result_Positions.Length != nPos)
				{
					_result_Positions = new Vector2[nPos];

					//for (int i = 0; i < nPos; i++)
					//{
					//	_result_Positions[i] = Vector2.zero;
					//}
					
					Array.Clear(_result_Positions, 0, nPos);//변경 22.3.28
				}


				//추가 : 만약 리깅타입이면 Vertex 개수만큼의 Matrix를 만들어야 한다.
				if (_linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					if (_result_VertMatrices == null || _result_VertMatrices.Length != nPos)
					{
						_result_VertMatrices = new apMatrix3x3[nPos];
						//for (int i = 0; i < nPos; i++)
						//{
						//	_result_VertMatrices[i] = apMatrix3x3.zero;
						//}
						Array.Clear(_result_VertMatrices, 0, nPos);//변경 22.3.28
					}
				}

				//추가 22.3.28 [v1.4.0] Pin 결과 미리보기
				//단, Morph 타입만 해당.
				if(_linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.Morph
					|| _linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
				{
					int nPins = 0;
					if (_targetRenderUnit._meshTransform != null && _targetRenderUnit._meshTransform._mesh != null)
					{
						apMeshPinGroup pinGroup = _targetRenderUnit._meshTransform._mesh._pinGroup;
						if(pinGroup != null)
						{
							nPins = pinGroup.NumPins;
						}
					}
					int nCurPins = _result_PinPositions != null ? _result_PinPositions.Length : 0;
					if (nPins != nCurPins)
					{
						if (nPins > 0)
						{
							_result_PinPositions = new Vector2[nPins];
							Array.Clear(_result_PinPositions, 0, nPins);
						}
						else
						{
							_result_PinPositions = null;
						}
					}
				}
			}
		}


		public void ClearParamKeyValueSets()
		{
			_paramKeyValues.Clear();
			_subParamKeyValueList.Clear();
		}


		//삭제 19.5.20
		//public void LinkWeightedVertexData(apModifierParamSetGroupVertWeight weightedVertData)
		//{
		//	_weightedVertexData = weightedVertData;
		//}

		/// <summary>
		/// ParamSet을 받아서 SubList와 연동한다.
		/// </summary>
		/// <param name="paramSet"></param>
		/// <param name="modifiedValue"></param>
		public void AddParamSetAndModifiedValue(apModifierParamSetGroup paramSetGroup,
												apModifierParamSet paramSet,
												apModifiedMesh modifiedMesh,
												apModifiedBone modifiedBone)
		{
			ParamKeyValueSet existSet = GetParamKeyValue(paramSet);
			if (existSet != null)
			{
				return;
			}


			//수정 : ModifiedBone을 넣어주자
			//새로운 KeyValueSet을 만들어서 리스트에 추가하자
			ParamKeyValueSet newKeyValueSet = null;
			if (modifiedMesh != null)
			{
				//ModMesh를 등록하는 경우
				newKeyValueSet = new ParamKeyValueSet(paramSetGroup, paramSet, modifiedMesh);
			}
			else if (modifiedBone != null)
			{
				//ModBone을 등록하는 경우
				newKeyValueSet = new ParamKeyValueSet(paramSetGroup, paramSet, modifiedBone);
			}

			_paramKeyValues.Add(newKeyValueSet);

			apCalculatedResultParamSubList targetSubList = null;

			//같이 묶여서 작업할 SubList가 있는가
			apCalculatedResultParamSubList existSubList = _subParamKeyValueList.Find(delegate (apCalculatedResultParamSubList a)
			{
			//return a._controlParam == paramSetGroup._keyControlParam;
			return a._keyParamSetGroup == paramSetGroup;
			});

			if (existSubList != null)
			{
				targetSubList = existSubList;
			}
			else
			{
				targetSubList = new apCalculatedResultParamSubList(this);
				targetSubList.SetParamSetGroup(paramSetGroup);
				//targetSubList.SetControlParam(paramSetGroup._keyControlParam);

				_subParamKeyValueList.Add(targetSubList);
				//Debug.LogError("AddParamSetAndModifiedMesh : Add New SubList");
			}


			#region [미사용 코드]
			//switch (paramSetGroup._syncTarget)
			//{
			//	case apModifierParamSetGroup.SYNC_TARGET.Controller:
			//		{
			//			if (paramSetGroup._keyControlParam != null)
			//			{
			//				//같이 묶여서 작업할 SubList가 있는가
			//				apCalculatedResultParamSubList existSubList = _subParamKeyValueList.Find(delegate (apCalculatedResultParamSubList a)
			//				{
			//					return a._controlParam == paramSetGroup._keyControlParam;
			//				});

			//				if (existSubList != null)
			//				{
			//					targetSubList = existSubList;
			//				}
			//				else
			//				{
			//					targetSubList = new apCalculatedResultParamSubList(this);
			//					targetSubList.SetParamSetGroup(paramSetGroup);
			//					//targetSubList.SetControlParam(paramSetGroup._keyControlParam);

			//					_subParamKeyValueList.Add(targetSubList);
			//				}
			//			}
			//		}
			//		break;

			//	case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
			//		{
			//			//...TODO
			//			Debug.LogError("TODO : KeyFrame 타입의 CalculateResultParam의 SubList 처리할 것");
			//			//??
			//		}
			//		break;

			//	case apModifierParamSetGroup.SYNC_TARGET.Static:
			//		{
			//			//Static은 1개만 있다.
			//			if(_subParamKeyValueList.Count == 0)
			//			{
			//				//새로 만들자.
			//				targetSubList = new apCalculatedResultParamSubList(this);
			//				targetSubList.SetStatic();

			//				_subParamKeyValueList.Add(targetSubList);
			//			}
			//			else
			//			{
			//				//있는거 사용하자
			//				targetSubList = _subParamKeyValueList[0];
			//			}
			//		}
			//		break;
			//} 
			#endregion

			//해당 SubList에 위에서 만든 KeyValueSet을 추가하자
			if (targetSubList != null)
			{
				//ParamKeyValueSet을 추가하자
				targetSubList.AddParamKeyValueSet(newKeyValueSet);
			}
		}




		public void SortSubList()
		{
			_subParamKeyValueList.Sort(delegate (apCalculatedResultParamSubList a, apCalculatedResultParamSubList b)
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

		// Functions
		//--------------------------------------------------
		public void InitCalculate()
		{
			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				_subParamKeyValueList[i].InitCalculate();
			}

			//_totalParamSetGroupWeight = 0.0f;//이전

			//추가 22.5.11
			_isMainCalculated = false;
			_isColorCalculated = false;
			_isExtra_DepthChanged = false;
			_isExtra_TextureChanged = false;

			//변경 : Transform / Color로 나눔
			_totalParamSetGroupWeight_Transform = 0.0f;
			_totalParamSetGroupWeight_Color = 0.0f;
			
		}
		public void Calculate()
		{
			//_totalParamSetGroupWeight = 0.0f;//<<이건 Modifier에서 넣어준다.

			//변경 : Transform / Color로 나눔
			_totalParamSetGroupWeight_Transform = 0.0f;
			_totalParamSetGroupWeight_Color = 0.0f;

			for (int i = 0; i < _subParamKeyValueList.Count; i++)
			{
				_subParamKeyValueList[i].Calculate();
			}

		}


		// 에디터 관련
		//-----------------------------------------------------------------------
		//이 함수들은 딱히 필요 없을듯
		//Modifier의 On/Off만 중요한 듯 하다
		//public void ActiveAllParamList()
		//{
		//	for (int i = 0; i < _paramKeyValues.Count; i++)
		//	{
		//		//수정.. 일단 이거 끕시다
		//		//_paramKeyValues[i]._isActive_InEditorExclusive = true;
		//	}
		//}

		//public void ActiveConditionParamList(List<apModifierParamSet> paramSetList)
		//{
		//	for (int i = 0; i < _paramKeyValues.Count; i++)
		//	{
		//		//수정... 일단 이거 끕시다
		//		//if (paramSetList.Contains(_paramKeyValues[i]._paramSet))
		//		//{
		//		//	_paramKeyValues[i]._isActive_InEditorExclusive = true;
		//		//}
		//		//else
		//		//{
		//		//	_paramKeyValues[i]._isActive_InEditorExclusive = false;
		//		//}

		//	}
		//}



		// Get
		//--------------------------------------------------
		public int ModifierLayer { get { return _linkedModifier._layer; } }
		public apModifierBase.BLEND_METHOD ModifierBlendMethod
		{
			get
			{
				//if (_linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
				if (_linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
					|| _linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
				{
					return apModifierBase.BLEND_METHOD.Interpolation;
				}
				return _linkedModifier._blendMethod;
			}
		}
		public float ModifierWeight_Transform
		{
			get
			{
				//return _linkedModifier._layerWeight;
				//if (_linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
				if (_linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
					|| _linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
				{
					return 1.0f;
				}

				//수정 >> 
				return Mathf.Clamp01(_linkedModifier._layerWeight * Mathf.Clamp01(_totalParamSetGroupWeight_Transform));
			}
		}
		public float ModifierWeight_Color
		{
			get
			{
				//return _linkedModifier._layerWeight;
				//if (_linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled)
				if (_linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit
					|| _linkedModifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background)
				{
					return 1.0f;
				}

				//수정 >> 
				return Mathf.Clamp01(_linkedModifier._layerWeight * Mathf.Clamp01(_totalParamSetGroupWeight_Color));
			}
		}

		//public bool IsModifierAvailable { get { return _isAvailable; } }



		public ParamKeyValueSet GetParamKeyValue(apModifierParamSet paramSet)
		{
			return _paramKeyValues.Find(delegate (ParamKeyValueSet a)
			{
				return a._paramSet == paramSet;
			});
		}
	}

}