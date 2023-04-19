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
using UnityEngine.Profiling;//테스트
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// (Root)MeshGroup -> ModiferStack -> Modifier -> ParamSet...으로 이어지는 단계중 [Modifier]의 리얼타임
	/// </summary>
	[Serializable]
	public class apOptModifierUnitBase
	{
		// Members
		//--------------------------------------------
		[NonSerialized]
		public apPortrait _portrait = null;

		

		[NonSerialized]
		public apOptTransform _parentOptTransform = null;

		//고유 ID. 모디파이어도 고유 아이디를 갖는다.
		public int _uniqueID = -1;

		//레이어
		public int _layer = -1;//낮을수록 먼저 처리된다. (오름차순으로 배열)

		//레이어 병합시 가중치 (0~1)
		public float _layerWeight = 0.0f;

		public string _name = "";

		[NonSerialized]
		public bool _isActive = true;

		[SerializeField]
		public apCalculatedResultParam.CALCULATED_VALUE_TYPE _calculatedValueType = apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos;

		[SerializeField]
		public apCalculatedResultParam.CALCULATED_SPACE _calculatedSpace = apCalculatedResultParam.CALCULATED_SPACE.Object;

		public apModifierBase.BLEND_METHOD _blendMethod = apModifierBase.BLEND_METHOD.Interpolation;

		[SerializeField]
		public List<apOptParamSetGroup> _paramSetGroupList = new List<apOptParamSetGroup>();


		[NonSerialized]
		public Dictionary<apVertex, apMatrix3x3> _vertWorldMatrix = new Dictionary<apVertex, apMatrix3x3>();

		//각 RenderUnit으로 계산 결과를 보내주는 Param들
		[NonSerialized]
		public List<apOptCalculatedResultParam> _calculatedResultParams = new List<apOptCalculatedResultParam>();


		[SerializeField]
		public bool _isColorPropertyEnabled = true;

		//Editor에는 없는 변수
		//Color 타입의 CalculateValueType + _isColorPropertyEnabled일때 True이다.
		[SerializeField]
		public bool _isColorProperty = false;


		//추가 12.5 : Extra Option
		[SerializeField]
		public bool _isExtraPropertyEnabled = false;//<<이건 기본값이 False

		//다형성이 안되니
		//Enum을 넣고 아예 다 로직을 여기에 넣자
		[SerializeField]
		public apModifierBase.MODIFIER_TYPE _modifierType = apModifierBase.MODIFIER_TYPE.Base;


		//계산용 변수
		private Vector2[] posList = null;
		private Vector2[] tmpPosList = null;

		//private apMatrix3x3[] vertMatrixList = null;
		//private apMatrix3x3[] tmpVertMatrixList = null;

		//이전 : apMatrix를 이용하는 경우
		//private apMatrix tmpMatrix = new apMatrix();

		//변경 3.27 : apMatrixCal을 이용하는 것으로 변경
		private apMatrixCal tmpMatrix = new apMatrixCal();


		private List<apOptCalculatedResultParamSubList> subParamGroupList = null;
		private List<apOptCalculatedResultParam.OptParamKeyValueSet> subParamKeyValueList = null;
		private float layerWeight = 0.0f;
		private apOptParamSetGroup keyParamSetGroup = null;
		//추가 20.4.18 : 애니메이션 모디파이어를 위한 계산 변수
		private apAnimClip _keyAnimClip = null;
		private apAnimPlayUnit _keyAnimPlayUnit = null;



		private apOptCalculatedResultParamSubList curSubList = null;
		private apOptCalculatedResultParam.OptParamKeyValueSet paramKeyValue = null;

		private List<apOptVertexRequest> calParamVertRequestList = null;
		private apOptVertexRequest vertRequest = null;
		private apOptVertexRequest tmpVertRequest = null;


		//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
		//private apOptParamSetGroupVertWeight weightedVertData = null;

		private Color tmpColor = Color.clear;
		private bool tmpVisible = false;
		//private float tmpBoneIKWeight = 0.0f;
		private int nColorCalculated = 0;
		private float tmpTotalParamSetWeight = 0.0f;

		private int iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
		private bool tmpIsColoredKeyParamSetGroup = false;
		private Color tmpParamColor = Color.black;

		//추가 20.2.24 : Show/Hide 토글을 할 수 있다.
		private bool tmpIsToggleShowHideOption = false;


		//추가 21.9.2 : 회전 보정
		private bool _cal_isRotation180Correction = false;
		private float _cal_Rotation180Correction_DeltaAngle = 0.0f;
		private Vector2 _cal_Rotation180Correction_SumVector = Vector2.zero;
		private Vector2 _cal_Rotation180Correction_CurVector = Vector2.zero;
				
		private bool tmpToggleOpt_IsAnyKey_Shown = false;
		private float tmpToggleOpt_TotalWeight_Shown = 0.0f;
		private float tmpToggleOpt_MaxWeight_Shown = 0.0f;
		private float tmpToggleOpt_KeyIndex_Shown = 0.0f;
		private bool tmpToggleOpt_IsAny_Hidden = false;
		private float tmpToggleOpt_TotalWeight_Hidden = 0.0f;
		private float tmpToggleOpt_MaxWeight_Hidden = 0.0f;
		private float tmpToggleOpt_KeyIndex_Hidden = 0.0f;
		private float tmpToggleOpt_KeyIndex_Cal = 0.0f;

		
		private apOptModifiedVertexWeight tmpModVertWeight = null;
		private apOptPhysicsVertParam tmpPhysicVertParam = null;
		private apOptPhysicsMeshParam tmpPhysicMeshParam = null;
		private int tmpNumVert = 0;
		private float tmpMass = 0.0f;

		//추가 : ModMeshSet의 PhysicsVertex
		private apOptModifiedPhysicsVertex tmpModPhysicsVert = null;
		private apOptModifiedPhysicsVertex.LinkedVertex tmpLinkedPhysicVert = null;


		private Vector2 tmpF_gravity = Vector2.zero;
		private Vector2 tmpF_wind = Vector2.zero;
		private Vector2 tmpF_stretch = Vector2.zero;
		//private Vector2 tmpF_airDrag = Vector2.zero;
		//private Vector2 tmpF_inertia = Vector2.zero;
		private Vector2 tmpF_recover = Vector2.zero;
		private Vector2 tmpF_ext = Vector2.zero;
		private Vector2 tmpF_sum = Vector2.zero;

		private apOptPhysicsVertParam.OptLinkedVertex tmpLinkedVert = null;
		private bool tmpIsViscosity = false;

		private Vector2 tmpNextVelocity = Vector2.zero;
		private float tmpLinkedViscosityWeight = 0.0f;
		//private Vector2 tmpLinkedViscosityNextVelocity = Vector2.zero;

		private Vector2 tmpSrcVertPos_NoMod = Vector2.zero;
		private Vector2 tmpLinkVertPos_NoMod = Vector2.zero;
		private Vector2 tmpSrcVertPos_Cur = Vector2.zero;
		private Vector2 tmpLinkVertPos_Cur = Vector2.zero;
		private Vector2 tmpDeltaVec_0 = Vector2.zero;
		private Vector2 tmpDeltaVec_Cur = Vector2.zero;
		private Vector2 tmpNextCalPos = Vector2.zero;
		private Vector2 tmpLinkedTotalCalPos = Vector2.zero;

		//터치에 의한 외력을 계산하기 위한 코드 변수
		[NonSerialized]
		private int _tmpTouchProcessCode = 0;


		//추가 12.5 : Extra Option 계산용
		private bool tmpExtra_DepthChanged = false;
		private bool tmpExtra_TextureChanged = false;
		private int tmpExtra_DeltaDepth = 0;
		private int tmpExtra_TextureDataID = 0;
		private apOptTextureData tmpExtra_TextureData = null;
		private float tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
		private float tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


		//private apOptModifiedMesh_Vertex		tmpSubModMesh_Vertex = null;
		private apOptModifiedMesh_Transform		tmpSubModMesh_Transform = null;
		//private apOptModifiedMesh_VertexRig		tmpSubModMesh_Rigging = null;
		private apOptModifiedMesh_Physics		tmpSubModMesh_Physics = null;
		private apOptModifiedMesh_Color			tmpSubModMesh_Color = null;
		private apOptModifiedMesh_Extra			tmpSubModMesh_Extra = null;


		private static Color _defaultColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		//Modifier의 설정들을 저장하자
		[SerializeField]
		private bool _isTarget_Bone = false;

		[SerializeField]
		private bool _isTarget_MeshTransform = false;

		[SerializeField]
		private bool _isTarget_MeshGroupTransform = false;

		[SerializeField]
		private bool _isTarget_ChildMeshTransform = false;

		[SerializeField]
		private bool _isAnimated = false;

		[SerializeField]
		private bool _isPreUpdate = false;

		public bool IsTarget_MeshTransform { get { return _isTarget_MeshTransform; } }
		public bool IsTarget_MeshGroupTransform { get { return _isTarget_MeshGroupTransform; } }
		public bool IsTarget_Bone { get { return _isTarget_Bone; } }
		public bool IsTarget_ChildMeshTransform { get { return _isTarget_ChildMeshTransform; } }
		public bool IsAnimated { get { return _isAnimated; } }
		public bool IsPreUpdate { get { return _isPreUpdate; } }

		//ParamSetWeight를 사용하는가
		[SerializeField]
		private bool _isUseParamSetWeight = false;

		[NonSerialized]
		protected float _tDeltaFixed = 0.0f;

		private const float PHYSIC_DELTA_TIME = 0.033f;//20FPS (0.05), 30FPS (0.033), 15FPS (0.067), 40FPS (0.025)		: 이걸 바꾸면 물리 재질이 더 과하게 움직일 수 있으니 건들지 말자

		//private System.Diagnostics.Stopwatch _stopWatch = null;//삭제 21.7.7

		//추가 19.5.23 : 최적화를 위해 개선된 ModMeshSet (< ModMesh)을 사용할지 여부. v1.1.7 이후에 Bake를 하면 항상 true이다.
		[SerializeField]
		public bool _isUseModMeshSet = false;

		//추가 20.11.23 : 빠른 애니메이션 처리를 위한 매핑 클래스 (Portrait에 속한걸 연결함)
		[NonSerialized]
		private apAnimPlayMapping _animPlayMapping = null;

		// 추가 20.4.18
		//애니메이션이 블렌딩될 때 (연속 또는 레이어로) 기존의 방식으로는 제대로 처리가 되지 않아서
		//PSG 데이터를 바로 CalParam으로 넘기지 않고, 1차적으로 그룹에 값을 저장한뒤, 그룹 단위로 값을 CalParam에 넘기자.
		public class AnimLayeredParam
		{
			//레이어 정보
			public int _layerIndex = 0;

			//계산 정보
			public bool _isCalculated = false;
			public int _iCurAnimClip = 0;
			public float _totalWeight = 0.0f;

			//블렌딩 정보 > 첫 애니메이션 클립의 블렌딩 방식이 레이어 전체의 블렌딩 방식이다
			public apModifierParamSetGroup.BLEND_METHOD _blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;

			//계산되는 항목들
			private bool _isMorphMod = true;//이게 Morph 타입인가
			private bool _isTFMod = true;//이게 TF 타입인가 (추가 21.7.20)

			//1. Morph 타입일 때
			//이 레이어에서 계산된 vertRequest들을 리스트 형태로 저장한다.
			public List<apOptVertexRequest> _cal_VertRequests = null;
			public List<float> _cal_VertWeight = null;
			public int _cal_NumVertRequest = 0;


			//2. TF 타입일 때
			public apMatrixCal _cal_Matrix = null;

			//공통 속성들
			public bool _isCal_Color = false;
			public int _nCal_Color = 0;
			public Color _cal_Color = Color.clear;
			public bool _cal_Visible = false;

			public bool _isCal_ExtraDepth = false;
			public int _cal_ExtraDepth = 0;

			public bool _isCal_ExtraTexture = false;
			public apOptTextureData _cal_ExtraTexture = null;
			public int _cal_ExtraTextureID = -1;


			private AnimLayeredParam(int layer, bool isMorph, bool isTF)
			{
				_layerIndex = layer;

				_isMorphMod = isMorph;
				_isTFMod = isTF;
				
				if(_isMorphMod)
				{
					_cal_VertRequests = new List<apOptVertexRequest>();
					_cal_VertWeight = new List<float>();
				}

				if(_isTFMod)
				{
					_cal_Matrix = new apMatrixCal();
				}

				
				ReadyToUpdate();
			}

			public static AnimLayeredParam Make_Morph(int layer)
			{
				return new AnimLayeredParam(layer, true, false);
			}

			public static AnimLayeredParam Make_TF(int layer)
			{
				return new AnimLayeredParam(layer, false, true);
			}

			public static AnimLayeredParam Make_Color(int layer)
			{
				return new AnimLayeredParam(layer, false, false);
			}


			public void ReadyToUpdate()
			{
				_isCalculated = false;
				_iCurAnimClip = 0;
				_totalWeight = 0.0f;

				_blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;

				if(_isMorphMod)
				{
					_cal_VertRequests.Clear();
					_cal_VertWeight.Clear();
					_cal_NumVertRequest = 0;
				}

				if(_isTFMod)
				{
					_cal_Matrix.SetZero();
				}
				
				

				_isCal_Color = false;
				_nCal_Color = 0;
				_cal_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				_cal_Visible = false;

				_isCal_ExtraDepth = false;
				_cal_ExtraDepth = 0;

				_isCal_ExtraTexture = false;
				_cal_ExtraTexture = null;
				_cal_ExtraTextureID = -1;
			}
		}

		private List<AnimLayeredParam> _animLayeredParams = null;
		private AnimLayeredParam _curAnimLayeredParam = null;
		private int _nAnimLayeredParams = 0;
		private int _curAnimLayer = -1;
		private int _iColoredAnimLayeredParam = 0;
		private float _layeredAnimWeightClamped = 0.0f;
		private bool _isAnimAnyColorCalculated = false;
		private bool _isAnimAnyExtraCalculated = false;

		//계산을 위한 추가 변수 20.11.25

		

		// Init
		//--------------------------------------------
		public apOptModifierUnitBase()
		{
			Init();
		}

		public virtual void Init()
		{

		}

		public void Link(apPortrait portrait, apOptTransform parentOptTransform)
		{
			_portrait = portrait;
			_animPlayMapping = _portrait._animPlayMapping;
			_parentOptTransform = parentOptTransform;

			for (int i = 0; i < _paramSetGroupList.Count; i++)
			{
				_paramSetGroupList[i].LinkPortrait(portrait, this);
			}

			if(_isAnimated)
			{
				_animLayeredParams = new List<AnimLayeredParam>();
			}
		}

		public IEnumerator LinkAsync(apPortrait portrait, apOptTransform parentOptTransform, apAsyncTimer asyncTimer)
		{
			_portrait = portrait;
			_animPlayMapping = _portrait._animPlayMapping;
			_parentOptTransform = parentOptTransform;

			if(_isAnimated)
			{
				_animLayeredParams = new List<AnimLayeredParam>();
			}

			for (int i = 0; i < _paramSetGroupList.Count; i++)
			{
				yield return _paramSetGroupList[i].LinkPortraitAsync(portrait, this, asyncTimer);
			}

			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}
		}
		           
		public void Bake(apModifierBase srcModifier, apPortrait portrait, bool isUseModMeshSet)
		{
			_uniqueID = srcModifier._uniqueID;
			_layer = srcModifier._layer;
			_layerWeight = srcModifier._layerWeight;
			_isActive = srcModifier._isActive;

			_blendMethod = srcModifier._blendMethod;
			_calculatedValueType = srcModifier.CalculatedValueType;
			_calculatedSpace = srcModifier.CalculatedSpace;
			_modifierType = srcModifier.ModifierType;

			_name = srcModifier.DisplayName;

			_isColorPropertyEnabled = srcModifier._isColorPropertyEnabled;

			//추가 21.7.20 : 색상 모디파이어에서는 색상 옵션 강제로 true
			if(srcModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedColorOnly
				|| srcModifier.ModifierType == apModifierBase.MODIFIER_TYPE.ColorOnly)
			{
				_isColorPropertyEnabled = true;
			}

			//이 부분이 추가되었다. 실제로 Color 연산을 하는지는 이 변수를 활용하자
			_isColorProperty = (int)(_calculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0
								&& _isColorPropertyEnabled;

			//추가 12.5 : Extra Option
			_isExtraPropertyEnabled = srcModifier._isExtraPropertyEnabled;


			//Modifier의 설정을 저장하자
			_isTarget_Bone = srcModifier.IsTarget_Bone;
			_isTarget_MeshTransform = srcModifier.IsTarget_MeshTransform;
			_isTarget_MeshGroupTransform = srcModifier.IsTarget_MeshGroupTransform;
			_isTarget_ChildMeshTransform = srcModifier.IsTarget_ChildMeshTransform;
			_isAnimated = srcModifier.IsAnimated;


			_isPreUpdate = srcModifier.IsPreUpdate;
			//Debug.LogError(">>> Bake - [" + srcModifier.ModifierType + "] Is Pre Update : " + _isPreUpdate);

			//ParamSetWeight를 사용하는가
			_isUseParamSetWeight = srcModifier.IsUseParamSetWeight;

			_paramSetGroupList.Clear();
			for (int i = 0; i < srcModifier._paramSetGroup_controller.Count; i++)
			{
				apModifierParamSetGroup srcParamSetGroup = srcModifier._paramSetGroup_controller[i];

				apOptParamSetGroup optParamSetGroup = new apOptParamSetGroup();
				optParamSetGroup.Bake(portrait, this, srcParamSetGroup, _isAnimated, isUseModMeshSet);

				_paramSetGroupList.Add(optParamSetGroup);
				
			}

			//추가 19.5.23 : ModMeshSet을 사용하도록 하자
			_isUseModMeshSet = isUseModMeshSet;
		}

		// Functions
		//--------------------------------------------
		public void InitCalcualte(float tDelta)
		{
			//계산이 불가능한 상황일 때, 계산 값만 초기화한다.
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			apOptCalculatedResultParam calParam = null;
			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				calParam.InitCalculate();
				calParam._isAvailable = false;
			}

			//_tDeltaFixed = 0.0f;

		}

		public void Calculate(float tDelta)
		{
			//Calculate 최적화 : 19.5.24
			if (_isUseModMeshSet)
			{
				//ModMeshSet을 사용하는 버전의 Calculate
				switch (_modifierType)
				{
					case apModifierBase.MODIFIER_TYPE.Base:
						break;

					case apModifierBase.MODIFIER_TYPE.Volume:
						break;

					case apModifierBase.MODIFIER_TYPE.Morph:
						Calculate_Morph_UseModMeshSet(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
						Calculate_Morph_UseModMeshSet_Animation(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.Rigging:
						//Calculate_Rigging_UseModMeshSet(tDelta);//이거 안해도 되지 않나??
						break;

					case apModifierBase.MODIFIER_TYPE.Physic:
						Calculate_Physics_UseModMeshSet(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.TF:
						Calculate_TF_UseModMeshSet(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.AnimatedTF:
						Calculate_TF_UseModMeshSet_Animation(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.FFD:
						break;

					case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
						break;

					//추가 21.7.20 : 색상 제어 모디파이어
					case apModifierBase.MODIFIER_TYPE.ColorOnly:
						Calculate_ColorOnly_UseModMeshSet(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
						Calculate_ColorOnly_UseModMeshSet_Animation(tDelta);
						break;
				}
			}
			else
			{
				//이전 버전의 Calculate
				switch (_modifierType)
				{
					case apModifierBase.MODIFIER_TYPE.Base:
						break;

					case apModifierBase.MODIFIER_TYPE.Volume:
						break;

					case apModifierBase.MODIFIER_TYPE.Morph:
						Calculate_Morph(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
						Calculate_Morph(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.Rigging:
						Calculate_Rigging(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.Physic:
						Calculate_Physics(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.TF:
						Calculate_TF(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.AnimatedTF:
						Calculate_TF(tDelta);
						break;

					case apModifierBase.MODIFIER_TYPE.FFD:
						break;

					case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
						break;

					//추가 21.7.20 : 색상 제어 모디파이어 > 이전 버전에서는 지원하지 않는다.
					case apModifierBase.MODIFIER_TYPE.ColorOnly:	
					case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
						break;
				}
			}
			
		}


		//--------------------------------------------------------------------------
		// Sub 로직들
		//--------------------------------------------------------------------------


		//--------------------------------------------------------------------------
		// Morph
		//--------------------------------------------------------------------------
		private void Calculate_Morph(float tDelta)
		{

			
//#if UNITY_EDITOR
//			Profiler.BeginSample("Modifier - Calculate Morph");
//#endif

			//bool isFirstDebug = true;
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;
			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				
				//1. 계산 [중요]
				isUpdatable = calParam.Calculate();
				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//추가 : 색상 처리 초기화
				calParam._isColorCalculated = false;


				//계산 결과를 Vertex에 넣어줘야 한다.
				//구버전
				//posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;

				//신버전
				calParamVertRequestList = calParam._result_VertLocalPairs;


				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;

				//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
				//weightedVertData = calParam._weightedVertexData;

				//일단 초기화
				//구버전
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//신버전
				for (int iVR = 0; iVR < calParamVertRequestList.Count; iVR++)
				{
					calParamVertRequestList[iVR].InitCalculate();
				}


				if (_isColorProperty)
				{
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					calParam._result_IsVisible = false;//Alpha와 달리 Visible 값은 false -> OR 연산으로 작동한다.
				}
				else
				{
					calParam._result_IsVisible = true;
				}


				//추가 12.5 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;





				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;



				//추가 12.5 : Extra Option 계산 값				
				tmpExtra_DepthChanged = false;
				tmpExtra_TextureChanged = false;
				tmpExtra_DeltaDepth = 0;
				tmpExtra_TextureDataID = 0;
				tmpExtra_TextureData = null;
				tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					paramKeyValue = null;

					
					keyParamSetGroup = curSubList._keyParamSetGroup;
				
					//추가 20.4.2 : 애니메이션 모디파이어일때.
					if(_isAnimated && !keyParamSetGroup.IsAnimEnabled)
					{	
						//실행되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
						//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
						continue;
					}
					
					//레이어 내부의 임시 데이터를 먼저 초기화
					//구버전
					//for (int iPos = 0; iPos < posList.Length; iPos++)
					//{
					//	tmpPosList[iPos] = Vector2.zero;
					//}

					//신버전
					//Vertex Pos 대신 Vertex Requst를 보간하자
					vertRequest = curSubList._vertexRequest;
					//vertRequest.SetCalculated();//<<이 코드가 여기 있으면 안된다.


					tmpColor = Color.clear;
					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled;

					//#if UNITY_EDITOR
					//					Profiler.BeginSample("Modifier - Calculate Morph > Add Pos List");
					//#endif

					//-------------------------------------------
					// 여기가 과부하가 가장 심한 곳이다! 우오오오
					//-------------------------------------------

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						if (!paramKeyValue._isCalculated)
						{ continue; }

						tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

						//#if UNITY_EDITOR
						//						Profiler.BeginSample("Modifier - Calculate Morph > 2. Pos List Loop");
						//#endif

						//최적화해야할 부분 1)
						//구버전)
						//Pos를 일일이 돌게 아니라 VertexRequst의 Weight만 지정하자
						////---------------------------- Pos List
						//for (int iPos = 0; iPos < posList.Length; iPos++)
						//{
						//	//calculatedValue = paramKeyValue._modifiedValue._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//	tmpPosList[iPos] += paramKeyValue._modifiedMesh._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//}
						////---------------------------- Pos List

						//>> 최적화 코드)
						vertRequest._modWeightPairs[iPV].SetWeight(paramKeyValue._weight);



						//---------------------------- Color
						if (tmpIsColoredKeyParamSetGroup)
						{
							if (paramKeyValue._modifiedMesh._isVisible)
							{
								tmpColor += paramKeyValue._modifiedMesh._meshColor * paramKeyValue._weight;
								tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
							}
							else
							{
								tmpParamColor = paramKeyValue._modifiedMesh._meshColor;
								tmpParamColor.a = 0.0f;
								tmpColor += tmpParamColor * paramKeyValue._weight;
							}
							//paramKeyValue._modifiedValue._isMeshTransform
						}

						nColorCalculated++;
						//---------------------------- Color

						//---------------------------- Extra Option
						//추가 12.5 : Extra Option
						if(_isExtraPropertyEnabled)
						{
							//1. Modifier의 Extra Property가 켜져 있어야 한다.
							//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
							//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
							//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
							if (paramKeyValue._modifiedMesh._isExtraValueEnabled
								&& (paramKeyValue._modifiedMesh._extraValue._isDepthChanged || paramKeyValue._modifiedMesh._extraValue._isTextureChanged)
								)
							{
								//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
								float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
								float bias = 0.0001f;
								float overlapBias = 0.01f;
								float cutOut = 0.0f;
								bool isExactWeight = false;
								if (IsAnimated)
								{
									switch (paramKeyValue._animKeyPos)
									{
										case apOptCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
										case apOptCalculatedResultParam.AnimKeyPos.NextKey: cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
										case apOptCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
									}
								}
								else
								{
									cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout;
								}

								cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

								if (isExactWeight)
								{
									extraWeight = 10000.0f;
								}
								else if (cutOut < bias)
								{
									//정확하면 최대값
									//아니면 적용안함
									if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
									else { extraWeight = -1.0f; }
								}
								else
								{
									if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
									else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
								}

								if (extraWeight > 0.0f)
								{
									if (paramKeyValue._modifiedMesh._extraValue._isDepthChanged)
									{
										//2-1. Depth 이벤트
										if(extraWeight > tmpExtra_DepthMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_DepthMaxWeight = extraWeight;
											tmpExtra_DepthChanged = true;
											tmpExtra_DeltaDepth = paramKeyValue._modifiedMesh._extraValue._deltaDepth;
										}

									}
									if (paramKeyValue._modifiedMesh._extraValue._isTextureChanged)
									{
										//2-2. Texture 이벤트
										if(extraWeight > tmpExtra_TextureMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_TextureMaxWeight = extraWeight;
											tmpExtra_TextureChanged = true;
											tmpExtra_TextureData = paramKeyValue._modifiedMesh._extraValue._linkedTextureData;
											tmpExtra_TextureDataID = paramKeyValue._modifiedMesh._extraValue._textureDataID;
										}
									}
								}
							}
						}
						//---------------------------- Extra Option


//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif
					}//--- Params

//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.
					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));
					}


					if (layerWeight < 0.001f)
					{
						continue;
					}

					vertRequest.SetCalculated();//<<일단 계산하기 위해 참조 했음을 알린다.

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					if (nColorCalculated == 0)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
					}

					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)
					{
						//구버전 : Vertex Pos를 직접 수정
						//for (int iPos = 0; iPos < posList.Length; iPos++)
						//{
						//	posList[iPos] = tmpPosList[iPos] * layerWeight;
						//}

						//신버전 : VertexRequest에 넣자
						vertRequest.MultiplyWeight(layerWeight);
					}
					else
					{

//#if UNITY_EDITOR
//						Profiler.BeginSample("Modifier - Calculate Morph > Overlap Pos List");
//#endif

						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//구버전
									//if (weightedVertData != null)
									//{
									//	//Vertex 가중치가 추가되었다.
									//	float vertWeight = 0.0f;
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

									//		posList[iPos] += tmpPosList[iPos] * vertWeight;
									//	}
									//}
									//else
									//{
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		posList[iPos] += tmpPosList[iPos] * layerWeight;
									//	}
									//}

									//신버전 : VertexRequest에 넣자
									//Additive : Prev + Next * weight이므로
									//Next에만 weight를 곱한다.
									vertRequest.MultiplyWeight(layerWeight);

								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//if (weightedVertData != null)
									//{
									//	//Vertex 가중치가 추가되었다.
									//	float vertWeight = 0.0f;
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

									//		posList[iPos] = (posList[iPos] * (1.0f - vertWeight)) +
									//						(tmpPosList[iPos] * vertWeight);
									//	}
									//}
									//else
									//{
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		posList[iPos] = (posList[iPos] * (1.0f - layerWeight)) +
									//						(tmpPosList[iPos] * layerWeight);
									//	}
									//}

									//신버전 : VertexRequest에 넣자
									//Interpolation : Prev * (1-weight) + Next * weight이므로
									//Prev에 1-weight
									//Next에 weight
									//단, 계산 안한건 제외한다.
									float invWeight = 1.0f - layerWeight;
									for (int iVR = 0; iVR < calParamVertRequestList.Count; iVR++)
									{
										tmpVertRequest = calParamVertRequestList[iVR];
										if(!tmpVertRequest._isCalculated)
										{
											//아직 계산 안한건 패스
											continue;
										}
										if(tmpVertRequest == vertRequest)
										{
											//Next엔 * weight
											tmpVertRequest.MultiplyWeight(layerWeight);
										}
										else
										{
											//Prev엔 * (1-weight)
											tmpVertRequest.MultiplyWeight(invWeight);
										}
									}
								}
								break;

							default:
								Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif

					}


					if (tmpIsColoredKeyParamSetGroup)
					{
						if (iColoredKeyParamSetGroup == 0 || keyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						iColoredKeyParamSetGroup++;
						calParam._isColorCalculated = true;
					}


					//추가 12.5 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(tmpExtra_DepthChanged)
						{
							calParam._isExtra_DepthChanged = true;
							calParam._extra_DeltaDepth = tmpExtra_DeltaDepth;
						}

						if(tmpExtra_TextureChanged)
						{
							calParam._isExtra_TextureChanged = true;
							calParam._extra_TextureData = tmpExtra_TextureData;
							calParam._extra_TextureDataID = tmpExtra_TextureDataID;
						}
					}


					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)




				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;

					//Morph에서는 이 과정이 필요없는데?
					////이전 : apMatrix로 계산된 경우
					////calParam._result_Matrix.MakeMatrix();

					////변경 3.27 : apMatrixCal로 계산된 경우
					//calParam._result_Matrix.CalculateScale_FromLerp();

				}

			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif	
		}





		//--------------------------------------------------------------------------
		// TF (Transform)
		//--------------------------------------------------------------------------
		private void Calculate_TF(float tDelta)
		{
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;

			//추가 : Bone을 대상으로 하는가
			//Bone대상이면 ModBone을 사용해야한다.
			bool isBoneTarget = false;
			//bool isBoneIKControllerUsed = false;//<<Bone IK 추가

			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				if (calParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					isBoneTarget = true;
					//if(calParam._targetBone._IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
					//{
					//	isBoneIKControllerUsed = true;//<<추가
					//	Debug.Log("Bone IK Used");
					//}
					//else
					//{
					//	isBoneIKControllerUsed = false;//<<추가
					//}
				}
				else
				{
					//ModMesh를 참조하는 Param이다.
					isBoneTarget = false;
					//isBoneIKControllerUsed = false;//<<
				}
				//1. 계산 [중요]
				isUpdatable = calParam.Calculate();

				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//초기화
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				keyParamSetGroup = null;

				calParam._result_Matrix.SetIdentity();

				calParam._isColorCalculated = false;

				if (!isBoneTarget)
				{
					if (_isColorProperty)
					{
						calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						calParam._result_IsVisible = false;
					}
					else
					{
						calParam._result_IsVisible = true;
					}
				}
				else
				{
					calParam._result_IsVisible = true;
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}


				//추가 12.5 : Extra Option 초기화
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;



				//초기화
				tmpMatrix.SetIdentity();
				layerWeight = 0.0f;

				//tmpBoneIKWeight = 0.0f;
				tmpVisible = false;

				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;

				//추가 : Bone 타겟이면 BoneIKWeight를 계산해야한다.
				//calParam._result_BoneIKWeight = 0.0f;
				//calParam._isBoneIKWeightCalculated = false;

				//추가 12.5 : Extra Option 계산 값 초기화
				tmpExtra_DepthChanged = false;
				tmpExtra_TextureChanged = false;
				tmpExtra_DeltaDepth = 0;
				tmpExtra_TextureDataID = 0;
				tmpExtra_TextureData = null;
				tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					//레이어 내부의 임시 데이터를 먼저 초기화
					tmpMatrix.SetZero();//<<TF에서 추가됨
					tmpColor = Color.clear;

					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget;

					if (!isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우

						//추가 20.9.10 : 정밀한 보간을 위해 Default Matrix가 필요하다.
						apMatrix defaultMatrixOfRenderUnit = null;
						if(calParam._targetOptTransform != null)
						{
							defaultMatrixOfRenderUnit = calParam._targetOptTransform._defaultMatrix;
						}



						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							paramKeyValue = subParamKeyValueList[iPV];

							if (!paramKeyValue._isCalculated)
							{ continue; }

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

							//Weight에 맞게 Matrix를 만들자
							if(paramKeyValue._isAnimRotationBias)
							{
								//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								//이전 : apMatrix
								//tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);

								//변경 3.27 : apMatrixCal
								//다시 변경 20.9.10
								tmpMatrix.AddMatrixParallel_ModMesh(paramKeyValue.AnimRotationBiasedMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
							}
							else
							{
								//기본 식
								//이전 : apMatrix
								//tmpMatrix.AddMatrix(paramKeyValue._modifiedMesh._transformMatrix, paramKeyValue._weight, false);

								//변경 3.27 : apMatrixCal
								//다시 변경 20.9.10
								tmpMatrix.AddMatrixParallel_ModMesh(paramKeyValue._modifiedMesh._transformMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
							}
							

							if (tmpIsColoredKeyParamSetGroup)
							{

								if (paramKeyValue._modifiedMesh._isVisible)
								{
									tmpColor += paramKeyValue._modifiedMesh._meshColor * paramKeyValue._weight;
									tmpVisible = true;
								}
								else
								{
									//Visible False
									tmpParamColor = paramKeyValue._modifiedMesh._meshColor;
									tmpParamColor.a = 0.0f;
									tmpColor += tmpParamColor * paramKeyValue._weight;
								}
							}

							nColorCalculated++;


							//---------------------------------------------
							//추가 12.5 : Extra Option
							if(_isExtraPropertyEnabled)
							{
								//1. Modifier의 Extra Property가 켜져 있어야 한다.
								//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
								//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
								//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
								if (paramKeyValue._modifiedMesh._isExtraValueEnabled
									&& (paramKeyValue._modifiedMesh._extraValue._isDepthChanged || paramKeyValue._modifiedMesh._extraValue._isTextureChanged)
									)
								{
									//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
									float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
									float bias = 0.0001f;
									float cutOut = 0.0f;
									bool isExactWeight = false;
									if (IsAnimated)
									{
										switch (paramKeyValue._animKeyPos)
										{
											case apOptCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
											case apOptCalculatedResultParam.AnimKeyPos.NextKey: cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
											case apOptCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
										}
									}
									else
									{
										cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout;
									}

									cutOut = Mathf.Clamp01(cutOut + 0.01f);//살짝 겹치게

									if (isExactWeight)
									{
										extraWeight = 10000.0f;
									}
									else if (cutOut < bias)
									{
										//정확하면 최대값
										//아니면 적용안함
										if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
										else { extraWeight = -1.0f; }
									}
									else
									{
										if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
										else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
									}

									if (extraWeight > 0.0f)
									{
										if (paramKeyValue._modifiedMesh._extraValue._isDepthChanged)
										{
											//2-1. Depth 이벤트
											if(extraWeight > tmpExtra_DepthMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												tmpExtra_DepthMaxWeight = extraWeight;
												tmpExtra_DepthChanged = true;
												tmpExtra_DeltaDepth = paramKeyValue._modifiedMesh._extraValue._deltaDepth;
											}

										}
										if (paramKeyValue._modifiedMesh._extraValue._isTextureChanged)
										{
											//2-2. Texture 이벤트
											if(extraWeight > tmpExtra_TextureMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												tmpExtra_TextureMaxWeight = extraWeight;
												tmpExtra_TextureChanged = true;
												tmpExtra_TextureData = paramKeyValue._modifiedMesh._extraValue._linkedTextureData;
												tmpExtra_TextureDataID = paramKeyValue._modifiedMesh._extraValue._textureDataID;
											}
										}
									}
								}
							}
							//---------------------------------------------
						}

						//위치 변경 20.9.11
						tmpMatrix.CalculateScale_FromAdd();
						tmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit);//추가 (20.9.10) : 위치 보간이슈 수정
					}
					else
					{
						//ModBone을 활용하는 타입인 경우
						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							//paramKeyValue = calParam._paramKeyValues[iPV];
							paramKeyValue = subParamKeyValueList[iPV];
							//layerWeight = Mathf.Clamp01(paramKeyValue._keyParamSetGroup._layerWeight);
							//Debug.Log("Param Key : " + paramKeyValue._weight + " / Cal : " + paramKeyValue._isCalculated);
							if (!paramKeyValue._isCalculated)
							{
								//nSkipWeight++;
								continue;
							}

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;
							//nAddedWeight++;

							//Weight에 맞게 Matrix를 만들자
							
							if(paramKeyValue._isAnimRotationBias)
							{
								//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								//이전 : apMatrix
								//tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);

								//변경 3.27 : apMatrixCal
								tmpMatrix.AddMatrixParallel_ModBone(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight);
							}
							else
							{
								//이전 : apMatrix
								//tmpMatrix.AddMatrix(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight, false);

								//변경 3.27 : apMatrixCal
								tmpMatrix.AddMatrixParallel_ModBone(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight);
							}

							
							nColorCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}

						//위치 변경 20.9.11
						tmpMatrix.CalculateScale_FromAdd();
						
					}

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));
					}


					

					if (layerWeight < 0.001f)
					{
						continue;
					}

					if ((nColorCalculated == 0 && _isColorProperty) || isBoneTarget)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
						if (!isBoneTarget)
						{
							tmpMatrix.SetIdentity();
							tmpColor = _defaultColor;
						}
					}

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					//추가 3.26 : apMatrixCal 계산 > 이건 ModMesh, ModBone에 따라 달라서 위에서 호출하자. (20.9.10)
					//tmpMatrix.CalculateScale_FromAdd();

					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)
					{
						//위 코드를 하나로 합쳤다.
						//이전 : apMatrix로 계산된 tmpMatrix
						//calParam._result_Matrix.SetTRS(	tmpMatrix._pos * layerWeight,
						//								tmpMatrix._angleDeg * layerWeight,
						//								tmpMatrix._scale * layerWeight + Vector2.one * (1.0f - layerWeight));

						//변경 3.27 : apMatrixCal로 계산된 tmpMatrix
						calParam._result_Matrix.SetTRSForLerp(tmpMatrix);

					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//이전 : apMatrix로 계산
									//calParam._result_Matrix.AddMatrix(tmpMatrix, layerWeight, true);

									//변경 3.27 : apMatrixCal로 계산
									calParam._result_Matrix.AddMatrixLayered(tmpMatrix, layerWeight);
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//이전 : apMatrix로 계산
									//calParam._result_Matrix.LerpMartix(tmpMatrix, layerWeight);

									//변경 3.27 : apMatrixCal로 계산
									calParam._result_Matrix.LerpMatrixLayered(tmpMatrix, layerWeight);
								}
								break;
						}

					}

					
					//변경 : 색상은 별도로 카운팅해서 처리하자
					if (tmpIsColoredKeyParamSetGroup)
					{
						if (iColoredKeyParamSetGroup == 0 || keyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						iColoredKeyParamSetGroup++;
						calParam._isColorCalculated = true;
					}

					//추가 12.5 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(tmpExtra_DepthChanged)
						{
							calParam._isExtra_DepthChanged = true;
							calParam._extra_DeltaDepth = tmpExtra_DeltaDepth;
						}

						if(tmpExtra_TextureChanged)
						{
							calParam._isExtra_TextureChanged = true;
							calParam._extra_TextureData = tmpExtra_TextureData;
							calParam._extra_TextureDataID = tmpExtra_TextureDataID;
						}
					}

					iCalculatedSubParam++;
				}

				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;

					//이전 : apMatrix로 계산된 경우
					//calParam._result_Matrix.MakeMatrix();

					//변경 : apMatrixCal로 계산한 경우
					calParam._result_Matrix.CalculateScale_FromLerp();
				}
			}
		}



		//----------------------------------------------------------------------
		// Rigging
		//----------------------------------------------------------------------
		private void Calculate_Rigging(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				//Debug.LogError("Result Param Count : 0");
				return;
			}


			apOptCalculatedResultParam calParam = null;

			//최적화 코드 추가
			//처리 전에 Transform + Bone 리스트를 돌면서 아예 WorldMatrix를 만들어준다.
			//for (int i = 0; i < _transformBoneMatrixPair_List.Count; i++)
			//{
			//	_transformBoneMatrixPair_List[i].UpdateBoneWorldMatrix();
			//}

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크


//#if UNITY_EDITOR
//				Profiler.BeginSample("Rigging - 1. Param Calculate");
//#endif
				// 중요! -> Static은 Weight 계산이 필요없어염
				//-------------------------------------------------------
				//1. Param Weight Calculate
				calParam.Calculate();
				//-------------------------------------------------------
//#if UNITY_EDITOR
//				Profiler.EndSample();
//#endif
				//수정 : Rigging의 VertexPos 대신 Matrix를 설정해주자
				//posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;

				//추가됨.
				//구버전
				//vertMatrixList = calParam._result_VertMatrices;
				//tmpVertMatrixList = calParam._tmp_VertMatrices;

				//신버전
				calParamVertRequestList = calParam._result_VertLocalPairs;


				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				//keyParamSetGroup = null;

				//weigetedVertData = calParam._weightedVertexData;

				//일단 초기화 -> Vertex Pos는 뺀다.
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//신버전
				for (int iVR = 0; iVR < calParamVertRequestList.Count; iVR++)
				{
					calParamVertRequestList[iVR].InitCalculate();
				}



				calParam._result_IsVisible = true;

				//apMatrix3x3 tmpBoneMatrix;
				//Color tmpColor = Color.clear;
				//bool tmpVisible = false;


				int iCalculatedSubParam = 0;



				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;


					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;//<<
																	//keyParamSetGroup._isEnabled = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					//변경 : Rigging Vertex는 사용하지 않습니다.
					//for (int iPos = 0; iPos < posList.Count; iPos++)
					//{
					//	tmpPosList[iPos] = Vector2.zero;
					//}

					//이것도 생략
					//for (int iPos = 0; iPos < vertMatrixList.Length; iPos++)
					//{
					//	tmpVertMatrixList[iPos].SetZero3x2();
					//}

					//신버전
					//Vertex Pos 대신 Vertex Requst를 보간하자
					vertRequest = curSubList._vertexRequest;
					vertRequest.SetCalculated();//<<일단 계산하기 위해 참조 했음을 알린다.


					tmpColor = Color.clear;
					tmpVisible = false;

					float totalWeight = 0.0f;
					int nCalculated = 0;
					//tmpVertRig = null;

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					//Dictionary<apOptBone, TransformBoneMatrixPair> boneMatrixPair = null;

					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						//if (!paramKeyValue._isCalculated) { continue; }

						paramKeyValue._weight = 1.0f;

						totalWeight += paramKeyValue._weight;

						//Modified가 안된 Vert World Pos + Bone의 Modified 안된 World Matrix + Bone의 World Matrix (변형됨) 순으로 계산한다.
						//Editor
						//apMatrix3x3 matx_Vert2Local = paramKeyValue._modifiedMesh._renderUnit._meshTransform._mesh.Matrix_VertToLocal;
						//apMatrix matx_MeshW_NoMod = paramKeyValue._modifiedMesh._renderUnit._meshTransform._matrix_TFResult_WorldWithoutMod;

						//Opt
						//사용안함 -> 단축 식을 직접 이용함. 
						//tmpMatx_Vert2Local = paramKeyValue._modifiedMesh._targetMesh._matrix_Vert2Mesh;
						//tmpMatx_Vert2LocalInv = paramKeyValue._modifiedMesh._targetMesh._matrix_Vert2Mesh_Inverse;

						//tmpMatx_MeshW_NoMod = paramKeyValue._modifiedMesh._targetTransform._matrix_TFResult_WorldWithoutMod;

						//이게 중간 버전 최적화
						//boneMatrixPair = _transformBoneMatrixPair_Dict[paramKeyValue._modifiedMesh._targetTransform];

						

//#if UNITY_EDITOR
//						Profiler.BeginSample("Rigging - 2. Pos Calculate");
//#endif
						

						//>> 최적화 코드)
						//vertRequest._[iPV].SetWeight(paramKeyValue._weight);


						nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif

					}//--- Params


					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					layerWeight = 1.0f;

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					if (nCalculated == 0)
					{
						tmpVisible = true;
					}


					//이전 코드
					//for (int iPos = 0; iPos < posList.Count; iPos++)
					//{
					//	posList[iPos] = tmpPosList[iPos] * layerWeight;
					//}

					//중간 버전 최적화
					//for(int iPos = 0; iPos < vertMatrixList.Length; iPos++)
					//{
					//	vertMatrixList[iPos].SetMatrixWithWeight(tmpVertMatrixList[iPos], layerWeight);
					//}

					//신버전 코드
					//VertexRequest에 넣자
					//vertRequest.MultiplyWeight(layerWeight);//??어차피 LayerWeight가 1인데..

					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				calParam._isAvailable = true;


			}

			//Rigging을 그 이후의 보간이 없다.
		}


		//초당 얼마나 업데이트 요청을 받는지 체크
		//삭제 21.7.7
		//private int _nUpdateCall = 0;
		//private float _tUpdateCall = 0.0f;
		//private int _nUpdateValid = 0;

		private void Calculate_Physics(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			//삭제
			//if (_stopWatch == null)
			//{
			//	_stopWatch = new System.Diagnostics.Stopwatch();
			//	_stopWatch.Start();
			//}



			//삭제 21.7.7 : 스톱워치는 실행시 오히려 성능이 떨어진다.
			//tDelta = (float)(_stopWatch.ElapsedMilliseconds) / 1000.0f;

			//변경 21.7.7
			tDelta = Time.unscaledDeltaTime;

			//v1.4.2 : Delta 시간이 고정 업데이트 시간을 넘겼다면
			//앱이 백그라운드에서 대기를 했거나 FPS가 크게 떨어진 경우
			//최대 시간으로 고정한다.
			if(tDelta > PHYSIC_DELTA_TIME)
			{
				//물리 시간이 너무 크게 발생 > 보정한다.
				tDelta = PHYSIC_DELTA_TIME;
				
			}


			//tDelta *= 0.5f;
			bool isValidFrame = false;
			_tDeltaFixed += tDelta;
			//_tUpdateCall += tDelta;
			//_nUpdateCall++;

			if (_tDeltaFixed > PHYSIC_DELTA_TIME)
			{
				//Debug.Log("Delta Time : " + tDelta + " >> " + PHYSIC_DELTA_TIME);
				tDelta = PHYSIC_DELTA_TIME;
				_tDeltaFixed -= PHYSIC_DELTA_TIME;

				//만약 제외해도 고정 시간을 넘겼다면, (프레임 시간이 순간 너무 컸을 수 있다.)
				if(_tDeltaFixed > PHYSIC_DELTA_TIME)
				{
					while(_tDeltaFixed > PHYSIC_DELTA_TIME)
					{
						_tDeltaFixed -= PHYSIC_DELTA_TIME;
					}
				}


				isValidFrame = true;
			}
			else
			{
				//현재 프레임은 고정 프레임 업데이트가 아닌 그 사이에 호출된 시간이다.
				tDelta = 0.0f;
				isValidFrame = false;
			}

			//삭제 21.7.7
			//if (isValidFrame)
			//{
			//	_nUpdateValid++;
			//}

			//if (_tUpdateCall > 1.0f)
			//{
			//	//Debug.Log("초당 Update Call 횟수 : " + _nUpdateCall + " / Valid : " + _nUpdateValid + " (" + _tUpdateCall + ")");
			//	_tUpdateCall = 0.0f;
			//	_nUpdateCall = 0;
			//	_nUpdateValid = 0;
			//}

			//삭제 21.7.7
			//_stopWatch.Stop();
			//_stopWatch.Reset();
			//_stopWatch.Start();


			//tDelta *= 0.5f;

			apOptCalculatedResultParam calParam = null;

			//지역 변수를 여기서 일괄 선언하자

			//bool isFirstDebug = true;
			//외부 힘을 업데이트해야하는지를 여기서 체크하자
			bool isExtTouchProcessing = false;
			bool isExtTouchWeightRefresh = false;
			if (_portrait.IsAnyTouchEvent)
			{
				isExtTouchProcessing = true;//터치 이벤트 중이다.
				if (_tmpTouchProcessCode != _portrait.TouchProcessCode)
				{
					//처리중인 터치 이벤트가 바뀌었다.
					//새로운 터치라면 Weight를 새로 만들어야하고, 아니면 Weight를 초기화해야함
					_tmpTouchProcessCode = _portrait.TouchProcessCode;
					isExtTouchWeightRefresh = true;

				}
			}
			else
			{
				_tmpTouchProcessCode = 0;
			}



			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				calParam.Calculate();
				//-------------------------------------------------------

				posList = calParam._result_Positions;
				tmpPosList = calParam._tmp_Positions;
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;

				//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
				//weightedVertData = calParam._weightedVertexData;

				//일단 초기화
				for (int iPos = 0; iPos < posList.Length; iPos++)
				{
					posList[iPos] = Vector2.zero;
				}

				calParam._result_IsVisible = true;

				int iCalculatedSubParam = 0;

				//bool isFirstDebug = true;

				//bool isDebugStretchFirst = true;

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					if (curSubList._keyParamSetGroup == null)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;



					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;


					//Vector2 calculatedValue = Vector2.zero;

					bool isFirstParam = true;

					//레이어 내부의 임시 데이터를 먼저 초기화
					for (int iPos = 0; iPos < posList.Length; iPos++)
					{
						tmpPosList[iPos] = Vector2.zero;
					}

					float totalWeight = 0.0f;
					int nCalculated = 0;



					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						//if (!paramKeyValue._isCalculated) { continue; }

						totalWeight += paramKeyValue._weight;



						//물리 계산 순서
						//Vertex 각각의 이전프레임으로 부터의 속력 계산
						//
						if (posList.Length > 0 
							&& _portrait._isPhysicsPlay_Opt//<<Portrait에서 지원하는 경우만
							&& _portrait._isImportant//<<Important 설정이 붙은 객체만
							
#if IS_APDEMO
							&& false//데모 버전에서는 씬에서 물리 기능이 작동하지 않습니다.
#endif
							)
						{



							tmpModVertWeight = null;
							tmpPhysicVertParam = null;
							tmpPhysicMeshParam = paramKeyValue._modifiedMesh.PhysicParam;
							//tmpNumVert = posList.Length;
							tmpMass = tmpPhysicMeshParam._mass;
							if (tmpMass < 0.001f)
							{
								tmpMass = 0.001f;
							}
							//Debug.Log("Mass : " + tmpMass);

							//Vertex에 상관없이 적용되는 힘
							// 중력, 바람
							//1) 중력 : mg
							tmpF_gravity = tmpMass * tmpPhysicMeshParam.GetGravityAcc();

							//2) 바람 : ma
							tmpF_wind = tmpMass * tmpPhysicMeshParam.GetWindAcc(tDelta);


							tmpF_stretch = Vector2.zero;
							//tmpF_airDrag = Vector2.zero;

							//tmpF_inertia = Vector2.zero;
							tmpF_recover = Vector2.zero;
							tmpF_ext = Vector2.zero;
							tmpF_sum = Vector2.zero;

							tmpLinkedVert = null;
							tmpIsViscosity = tmpPhysicMeshParam._viscosity > 0.0f;


							//수정
							// "잡아 당기는 코드"를 미리 만들고, Weight를 지정한다.
							//Weight에 따라서 힘의 결과가 속도로 계산되는 비율이 결정된다.
							//Touch Weight가 클수록 Velocity는 0이 된다.


							//Debug.Log("Wind : " + tmpF_wind + " / Gravity : " + tmpF_gravity);
							//---------------------------- Pos List

							//bool isFirstDebug = true;
							//int iDebugLog = -1;
							bool isTouchCalculated = false;
							float touchCalculatedWeight = 0.0f;
							Vector2 touchCalculatedDeltaPos = Vector2.zero;

							for (int iPos = 0; iPos < tmpNumVert; iPos++)
							{
								//여기서 물리 계산을 하자
								tmpModVertWeight = paramKeyValue._modifiedMesh._vertWeights[iPos];
								tmpModVertWeight.UpdatePhysicVertex(tDelta, isValidFrame);//<<RenderVert의 위치와 속도를 계산한다.

								tmpF_stretch = Vector2.zero;
								//tmpF_airDrag = Vector2.zero;

								tmpF_recover = Vector2.zero;
								tmpF_ext = Vector2.zero;
								tmpF_sum = Vector2.zero;

								if (!tmpModVertWeight._isEnabled)
								{
									//처리 안함다
									tmpModVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (tmpModVertWeight._vertex == null)
								{
									//Debug.LogError("Render Vertex is Not linked");
									break;
								}

								//최적화는 나중에 하고 일단 업데이트만이라도 하자

								tmpPhysicVertParam = tmpModVertWeight._physicParam;


								tmpModVertWeight._isLimitPos = false;
								tmpModVertWeight._limitScale = -1.0f;

								//터치 이벤트 초기화
								isTouchCalculated = false;
								touchCalculatedWeight = 0.0f;
								touchCalculatedDeltaPos = Vector2.zero;

								//"잡아 당김"을 구현하자
								if (isExtTouchProcessing)
								{
									Vector2 pullTouchPos = Vector2.zero;
									float pullTouchTotalWeight = 0.0f;
									//Weight를 새로 갱신하자
									for (int i = 0; i < apForceManager.MAX_TOUCH_UNIT; i++)
									{
										apPullTouch touch = _portrait.GetTouch(i);
										Vector2 touchPos = touch.Position;
										//touchPos *= -1;

										if (isExtTouchWeightRefresh)
										{
											if (touch.IsLive)
											{
												//pos 1F 위치에 의한 Weight를 새로 갱신해야한다.
												tmpModVertWeight._touchedWeight[i] = touch.GetTouchedWeight(tmpModVertWeight._pos_1F);
												tmpModVertWeight._touchedPosDelta[i] = tmpModVertWeight._pos_1F - touch.Position;

												//Debug.Log("Touch Pos Check : " + touch.Position + " / Vert : " + tmpModVertWeight._pos_1F 
												//	+ " (Local : " + tmpModVertWeight._vertex._vertPos_World + ")"
												//	+ " / Weight : " + tmpModVertWeight._touchedWeight[i]);
											}
											else
											{
												tmpModVertWeight._touchedWeight[i] = -1.0f;//Weight를 초기화
											}
										}

										if (touch.IsLive)
										{
											//Weight를 이용하여 보간을 하자
											//이후 누적 후 평균값을 넣자
											//pullTouchPos += touch.GetPulledPos(tmpModVertWeight._pos_1F, tmpModVertWeight._touchedWeight[i]);
											pullTouchPos += (tmpModVertWeight._touchedPosDelta[i] + touch.Position - tmpModVertWeight._pos_1F) * tmpModVertWeight._touchedWeight[i];
											//pullTouchPos += (tmpModVertWeight._touchedPosDelta[i]) * tmpModVertWeight._touchedWeight[i];
											pullTouchTotalWeight += tmpModVertWeight._touchedWeight[i];
										}
									}

									if (pullTouchTotalWeight > 0.0f)
									{
										pullTouchPos /= pullTouchTotalWeight;
										pullTouchPos = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(pullTouchPos);
										pullTouchPos.x = -pullTouchPos.x;
										pullTouchPos.y = -pullTouchPos.y;

										float itpPull = Mathf.Clamp01(pullTouchTotalWeight);

										//Debug.Log("Touch DeltaPos (" + pullTouchTotalWeight + ") " + limitedNextCalPos + " >> " + pullTouchPos + " / ITP : " + itpPull);

										touchCalculatedDeltaPos = pullTouchPos;
										isTouchCalculated = true;
										touchCalculatedWeight = itpPull;


									}
								}


								//추가
								//> 유효한 프레임 : 물리 계산을 한다.
								//> 생략하는 프레임 : 이전 속도를 그대로 이용한다.
								if (isValidFrame)
								{




									tmpF_stretch = Vector2.zero;
									//F_bend = Vector2.zero;
									//float totalStretchWeight = 0.0f;



									//1) 장력 Strech : -k * (<delta Dist> * 기존 UnitVector)
									//int iVert_Src = tmpModVertWeight._vertIndex;
									for (int iLinkVert = 0; iLinkVert < tmpPhysicVertParam._linkedVertices.Count; iLinkVert++)
									{
										tmpLinkedVert = tmpPhysicVertParam._linkedVertices[iLinkVert];
										float linkWeight = tmpLinkedVert._distWeight;

										tmpSrcVertPos_NoMod = tmpModVertWeight._pos_World_NoMod;
										tmpLinkVertPos_NoMod = tmpLinkedVert._modVertWeight._pos_World_NoMod;
										tmpLinkedVert._deltaPosToTarget_NoMod = tmpSrcVertPos_NoMod - tmpLinkVertPos_NoMod;

										//tmpSrcVertPos_Cur = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformPoint(tmpModVertWeight._pos_Real);
										//tmpLinkVertPos_Cur = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformPoint(tmpLinkedVert._modVertWeight._pos_Real);
										tmpSrcVertPos_Cur = tmpModVertWeight._pos_World_LocalTransform;
										tmpLinkVertPos_Cur = tmpLinkedVert._modVertWeight._pos_World_LocalTransform;

										tmpDeltaVec_0 = tmpSrcVertPos_NoMod - tmpLinkVertPos_NoMod;
										tmpDeltaVec_Cur = tmpSrcVertPos_Cur - tmpLinkVertPos_Cur;


										//tmpF_stretch += -1.0f * tmpPhysicMeshParam._stretchK * (tmpDeltaVec_Cur - tmpDeltaVec_0) * linkWeight;
										//totalStretchWeight += linkWeight;
										//길이 차이로 힘을 만들고
										//방향은 현재 Delta

										//<추가> 만약 장력 벡터가 완전히 뒤집힌 경우
										//면이 뒤집혔다.
										if(Vector2.Dot(tmpDeltaVec_0, tmpDeltaVec_Cur) < 0)
										{
											//면이 뒤집혔다.
											tmpF_stretch += tmpPhysicMeshParam._stretchK * (tmpDeltaVec_0 - tmpDeltaVec_Cur) * linkWeight;
										}
										else
										{
											//정상 면
											tmpF_stretch += -1.0f * tmpPhysicMeshParam._stretchK * (tmpDeltaVec_Cur.magnitude - tmpDeltaVec_0.magnitude) * tmpDeltaVec_Cur.normalized * linkWeight;
										}
										
										

									}
									tmpF_stretch *= -1;//<<위치기반인 경우 좌표계가 반대여서 -1을 넣는다. <<< 이게 왜이리 힘들던지;;




									//3) 공기 저항 : "현재 이동 방향의 반대 방향"
									//수정 : 이게 너무 약하다.
									//tmpF_airDrag = -1.0f * tmpPhysicMeshParam._airDrag * tmpModVertWeight._velocity_Real;
									//tmpF_airDrag = -1.0f * tmpPhysicMeshParam._airDrag * tmpModVertWeight._velocity_Real / tDelta;



									//5) 복원력
									tmpF_recover = -1.0f * tmpPhysicMeshParam._restoring * tmpModVertWeight._calculatedDeltaPos;



									//변동
									//중력과 바람은 크기는 그대로 두고, 방향은 World였다고 가정
									//Local로 오기 위해서는 Inverse를 해야한다.
									float gravitySize = tmpF_gravity.magnitude;
									float windSize = tmpF_wind.magnitude;
									Vector2 tmpF_gravityL = Vector2.zero;
									Vector2 tmpF_windL = Vector2.zero;
									if (gravitySize > 0.0f)
									{
										tmpF_gravityL = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(tmpF_gravity.normalized).normalized * gravitySize;
										tmpF_gravityL.y = -tmpF_gravityL.y;
										tmpF_gravityL.x = -tmpF_gravityL.x;
										//tmpF_gravityL *= 10000.0f;
									}
									if (windSize > 0.0f)
									{
										tmpF_windL = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(tmpF_wind.normalized).normalized * windSize;
										tmpF_windL.y = -tmpF_windL.y;
										tmpF_windL.x = -tmpF_windL.x;
										//tmpF_windL *= 10000.0f;
									}

									//if(tmpModVertWeight._weight > 0.5f && isFirstDebug)
									//{
									//	Debug.Log("Wind Local : " + tmpF_windL + " / Gravity Local : " + tmpF_gravityL);
									//	isFirstDebug = false;
									//}

									//6) 추가 : 외부 힘
									if (_portrait.IsAnyForceEvent)
									{
										//이전 프레임에서의 힘을 이용한다.
										//해당 위치가 Local이고, 요청된 힘은 World이다.
										//World로 계산한 뒤의 위치를 잡자...는 이미 World였네요.
										//그대로 하고, 힘만 로컬로 바구면 될 듯
										Vector2 F_extW = _portrait.GetForce(tmpModVertWeight._pos_1F);
										float powerSize = F_extW.magnitude;
										tmpF_ext = paramKeyValue._modifiedMesh._targetTransform._rootUnit._transform.InverseTransformVector(F_extW).normalized * powerSize;
										tmpF_ext.x = -tmpF_ext.x;
										tmpF_ext.y = -tmpF_ext.y;
									}

									float inertiaK = Mathf.Clamp01(tmpPhysicMeshParam._inertiaK);

									//5) 힘의 합력을 구한다.
									//-------------------------------------------
									if (tmpModVertWeight._physicParam._isMain)
									{
										//tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + tmpF_airDrag + tmpF_recover + tmpF_ext;//관성 제외 (중력, 바람 W2L)
										tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + tmpF_recover + tmpF_ext;//관성 제외 (중력, 바람 W2L) - 공기 저항 제외
									}
									else
									{
										//tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + ((tmpF_airDrag + tmpF_recover + tmpF_ext) * 0.5f);//관성 제외 (중력, 바람 W2L)
										tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + ((tmpF_recover + tmpF_ext) * 0.5f);//관성 제외 (중력, 바람 W2L) - 공기저항 제외

										inertiaK *= 0.5f;//<<관성 감소
									}
									//tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_recover + tmpF_airDrag;
									//-------------------------------------------

									//tmpF_sum *= tmpPhysicMeshParam._optPhysicScale;//<<Opt에선 적당히 Scale을 줘야한다.

									if (isTouchCalculated)
									{
										tmpF_sum *= (1.0f - touchCalculatedWeight);
									}

									//F = ma
									//a = F / m
									//Vector2 acc = F_sum / mass;

									//S = vt + S0
									//-------------------------------
									

									//<<수정>>
									tmpModVertWeight._velocity_Next = 
											//(tmpModVertWeight._velocity_Real * inertiaK + tmpModVertWeight._velocity_1F * (1.0f - inertiaK))//관성
											//+ 
											//tmpModVertWeight._velocity_1F + (tmpModVertWeight._velocity_1F - tmpModVertWeight._velocity_Real) * inertiaK
											//+ (tmpF_sum / tmpMass) * tDelta
											//tmpModVertWeight._velocity_Real + (tmpModVertWeight._velocity_1F - tmpModVertWeight._velocity_Real) * inertiaK

											tmpModVertWeight._velocity_1F 
											+ (tmpModVertWeight._velocity_1F - tmpModVertWeight._velocity_Real) * inertiaK
											+ (tmpF_sum / tmpMass) * tDelta											
											;

									
									//Air Drag식 수정
									if(tmpPhysicMeshParam._airDrag > 0.0f)
									{
										tmpModVertWeight._velocity_Next *= Mathf.Clamp01((1.0f - (tmpPhysicMeshParam._airDrag * tDelta) / (tmpMass + 0.5f)));
									}
									//-------------------------------
								}
								else
								{
									//-------------------------------------
									//tmpModVertWeight._velocity_Next = tmpModVertWeight._velocity_Real;
									tmpModVertWeight._velocity_Next = tmpModVertWeight._velocity_1F;
									//-------------------------------------
								}



								//변경.
								//여기서 일단 속력을 미리 적용하자
								if (isValidFrame)
								{
									tmpNextVelocity = tmpModVertWeight._velocity_Next;

									//if(tmpModVertWeight._vertIndex == 0 && tmpNextVelocity.sqrMagnitude > 0)
									//{
									//	Debug.LogError("Next Vel : " + tmpNextVelocity + " / Vel 1F : " + tmpModVertWeight._velocity_1F);
									//}
									Vector2 limitedNextCalPos = tmpModVertWeight._calculatedDeltaPos + (tmpNextVelocity * tDelta);

									//터치 이벤트에 의해서 속도가 보간된다.
									if (isTouchCalculated)
									{
										limitedNextCalPos = (limitedNextCalPos * (1.0f - touchCalculatedWeight)) + (touchCalculatedDeltaPos * touchCalculatedWeight);
										tmpNextVelocity = (limitedNextCalPos - tmpModVertWeight._calculatedDeltaPos) / tDelta;
									}

									//V += at
									//마음대로 증가하지 않도록 한다.
									if (tmpPhysicMeshParam._isRestrictMoveRange)
									{
										float radiusFree = tmpPhysicMeshParam._moveRange * 0.5f;
										float radiusMax = tmpPhysicMeshParam._moveRange;

										if (radiusMax <= radiusFree)
										{
											tmpNextVelocity *= 0.0f;
											//둘다 0이라면 아예 이동이 불가
											if (!tmpModVertWeight._isLimitPos)
											{
												tmpModVertWeight._isLimitPos = true;
												tmpModVertWeight._limitScale = 0.0f;
											}
										}
										else
										{
											float curDeltaPosSize = (limitedNextCalPos).magnitude;

											if (curDeltaPosSize < radiusFree)
											{
												//moveRatio = 1.0f;
												//별일 없슴다
											}
											else
											{
												//기본은 선형의 사이즈이지만,
												//돌아가는 힘은 유지해야한다.
												//[deltaPos unitVector dot newVelocity] = 1일때 : 바깥으로 나가려는 힘
												// = -1일때 : 안으로 들어오려는 힘
												// -1 ~ 1 => 0 ~ 1 : 0이면 moveRatio가 1, 1이면 moveRatio가 거리에 따라 1>0
												float dotVector = Vector2.Dot(tmpModVertWeight._calculatedDeltaPos.normalized, tmpNextVelocity.normalized);
												dotVector = (dotVector * 0.5f) + 0.5f; //0: 속도 느려짐 없음 (안쪽으로 들어가려고 함), 1:증가하는 방향

												float outerItp = Mathf.Clamp01((curDeltaPosSize - radiusFree) / (radiusMax - radiusFree));//0 : 속도 느려짐 없음, 1:속도 0

												tmpNextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자

												if (curDeltaPosSize > radiusMax)
												{
													if (!tmpModVertWeight._isLimitPos || radiusMax < tmpModVertWeight._limitScale)
													{
														tmpModVertWeight._isLimitPos = true;
														tmpModVertWeight._limitScale = radiusMax;
													}
												}
											}
											//else
											//{
											//	//tmpNextCalPos = calPosUnitVec * radiusMax;
											//	limitedNextCalPos = limitedNextCalPos.normalized * radiusMax;//<<최대치만 이동한다.
											//}
										}
									}

									//장력에 의한 길이 제한도 처리한다.
									if (tmpPhysicMeshParam._isRestrictStretchRange)
									{

										bool isLimitVelocity2Max = false;
										Vector2 stretchLimitPos = Vector2.zero;
										float limitCalPosDist = 0.0f;
										for (int iLinkVert = 0; iLinkVert < tmpPhysicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpLinkedVert = tmpPhysicVertParam._linkedVertices[iLinkVert];
											//길이의 Min/Max가 있다.
											float distStretchBase = tmpLinkedVert._deltaPosToTarget_NoMod.magnitude;

											float stretchRangeMax = (tmpPhysicMeshParam._stretchRangeRatio_Max) * distStretchBase;
											float stretchRangeMax_Half = (tmpPhysicMeshParam._stretchRangeRatio_Max * 0.5f) * distStretchBase;

											Vector2 curDeltaFromLinkVert = limitedNextCalPos - tmpLinkedVert._modVertWeight._calculatedDeltaPos_Prev;
											float curDistFromLinkVert = curDeltaFromLinkVert.magnitude;

											//너무 멀면 제한한다.
											//단, 제한 권장은 Weight에 맞게

											//float weight = Mathf.Clamp01(tmpLinkedVert._distWeight);
											isLimitVelocity2Max = false;

											if (curDistFromLinkVert > stretchRangeMax_Half)
											{
												isLimitVelocity2Max = true;//늘어나는 한계점으로 이동하는 중
												stretchLimitPos = tmpLinkedVert._modVertWeight._calculatedDeltaPos_Prev + curDeltaFromLinkVert.normalized * stretchRangeMax;
												stretchLimitPos -= tmpModVertWeight._calculatedDeltaPos_Prev;


												//limitCalPosDist = stretchRangeMax;
												limitCalPosDist = (stretchLimitPos).magnitude;
												//if (curDistFromLinkVert >= stretchRangeMax)
												//{
												//	limitCalPosDist = (stretchLimitPos).magnitude;
												//}
											}

											if (isLimitVelocity2Max)
											{
												//LinkVert간의 벡터를 기준으로 nextVelocity가 확대/축소하는 방향이라면 그 반대의 값을 넣는다.
												float dotVector = Vector2.Dot(curDeltaFromLinkVert.normalized, tmpNextVelocity.normalized);
												//-1 : 축소하려는 방향으로 이동하는 중
												//1 : 확대하려는 방향으로 이동하는 중


												float outerItp = 0.0f;
												if (isLimitVelocity2Max)
												{
													//너무 바깥으로 이동하려고 할때, 속도를 줄인다.
													dotVector = Mathf.Clamp01(dotVector);
													if (stretchRangeMax > stretchRangeMax_Half)
													{
														outerItp = Mathf.Clamp01((curDistFromLinkVert - stretchRangeMax_Half) / (stretchRangeMax - stretchRangeMax_Half));
													}
													else
													{
														outerItp = 1.0f;//무조건 속도 0

														if (!tmpModVertWeight._isLimitPos || limitCalPosDist < tmpModVertWeight._limitScale)
														{
															tmpModVertWeight._isLimitPos = true;
															tmpModVertWeight._limitScale = limitCalPosDist;
														}
													}

												}

												tmpNextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자
											}


										}
										//nextVelocity *= velRatio;

										//Profiler.EndSample();

										//limitedNextCalPos = modVertWeight._calculatedDeltaPos + (nextVelocity * tDelta);
									}
									limitedNextCalPos = tmpModVertWeight._calculatedDeltaPos + (tmpNextVelocity * tDelta);

									//이걸 한번더 해서 위치 보정
									if (isTouchCalculated)
									{
										Vector2 nextTouchPos = (limitedNextCalPos * (1.0f - touchCalculatedWeight)) + (touchCalculatedDeltaPos * touchCalculatedWeight);

										//limitedNextCalPos = nextTouchPos.normalized * limitedNextCalPos.magnitude;
										limitedNextCalPos = nextTouchPos;
										//tmpNextVelocity *= (1.0f - touchCalculatedWeight);
										tmpNextVelocity = (limitedNextCalPos - tmpModVertWeight._calculatedDeltaPos) / tDelta;
									}

									tmpModVertWeight._velocity_Next = tmpNextVelocity;
									tmpModVertWeight._calculatedDeltaPos_Prev = tmpModVertWeight._calculatedDeltaPos;
									//tmpModVertWeight._calculatedDeltaPos += tmpModVertWeight._velocity_Next * tDelta;
									tmpModVertWeight._calculatedDeltaPos = limitedNextCalPos;
								}
								else
								{
									tmpModVertWeight._calculatedDeltaPos_Prev = tmpModVertWeight._calculatedDeltaPos;

									tmpNextVelocity = tmpModVertWeight._velocity_Next;
									tmpModVertWeight._calculatedDeltaPos = tmpModVertWeight._calculatedDeltaPos + (tmpNextVelocity * tDelta);
								}
							}

							//1차로 계산된 값을 이용하여 점성력을 체크한다.
							//수정 : 이미 위치는 계산되었다. 위치를 중심으로 처리를 하자 점성/이동한계를 계산하자
							for (int iPos = 0; iPos < tmpNumVert; iPos++)
							{
								tmpModVertWeight = paramKeyValue._modifiedMesh._vertWeights[iPos];
								tmpPhysicVertParam = tmpModVertWeight._physicParam;

								if (!tmpModVertWeight._isEnabled)
								{
									//처리 안함다
									tmpModVertWeight._calculatedDeltaPos = Vector2.zero;
									continue;
								}
								if (tmpModVertWeight._vertex == null)
								{
									Debug.LogError("Render Vertex is Not linked");
									break;
								}

								if (isValidFrame)
								{

									tmpNextVelocity = tmpModVertWeight._velocity_Next;
									tmpNextCalPos = tmpModVertWeight._calculatedDeltaPos;


									if (tmpIsViscosity && !tmpModVertWeight._physicParam._isMain)
									{

										//점성 로직 추가
										//ID가 같으면 DeltaPos가 비슷해야한다.
										tmpLinkedViscosityWeight = 0.0f;
										//tmpLinkedViscosityNextVelocity = Vector2.zero;
										tmpLinkedTotalCalPos = Vector2.zero;

										int curViscosityID = tmpModVertWeight._physicParam._viscosityGroupID;

										for (int iLinkVert = 0; iLinkVert < tmpPhysicVertParam._linkedVertices.Count; iLinkVert++)
										{
											tmpLinkedVert = tmpPhysicVertParam._linkedVertices[iLinkVert];
											float linkWeight = tmpLinkedVert._distWeight;

											if ((tmpLinkedVert._modVertWeight._physicParam._viscosityGroupID & curViscosityID) != 0)
											{
												//float subWeight = 1.0f;
												//tmpLinkedViscosityNextVelocity += tmpLinkedVert._modVertWeight._velocity_Next * linkWeight * subWeight;//사실 Vertex의 호출 순서에 따라 값이 좀 다르다.
												tmpLinkedTotalCalPos += tmpLinkedVert._modVertWeight._calculatedDeltaPos * linkWeight;//<<Vel 대신 Pos로 바꾸자
												tmpLinkedViscosityWeight += linkWeight;
											}
										}

										//점성도를 추가한다.
										if (tmpLinkedViscosityWeight > 0.0f)
										{
											//tmpLinkedViscosityNextVelocity /= tmpLinkedViscosityWeight;
											//tmpLinkedTotalCalPos /= tmpLinkedViscosityWeight;
											float clampViscosity = Mathf.Clamp01(tmpPhysicMeshParam._viscosity) * 0.7f;


											//tmpNextVelocity = tmpNextVelocity * (1.0f - clampViscosity) + tmpLinkedViscosityNextVelocity * clampViscosity;
											tmpNextCalPos = tmpNextCalPos * (1.0f - clampViscosity) + tmpLinkedTotalCalPos * clampViscosity;
										}

									}

									//이동 한계 한번 더 계산
									if (tmpModVertWeight._isLimitPos && tmpNextCalPos.magnitude > tmpModVertWeight._limitScale)
									{
										tmpNextCalPos = tmpNextCalPos.normalized * tmpModVertWeight._limitScale;
										//Debug.Log("Limit Scale : " + tmpModVertWeight._limitScale);
									}


									//계산 끝!
									//새로운 변위를 넣어주자
									tmpModVertWeight._calculatedDeltaPos = tmpNextCalPos;


									//속도를 다시 계산해주자
									tmpNextVelocity = (tmpModVertWeight._calculatedDeltaPos - tmpModVertWeight._calculatedDeltaPos_Prev) / tDelta;

									
									//-----------------------------------------------------------------------------------------
									//속도 갱신
									tmpModVertWeight._velocity_Next = tmpNextVelocity;
									
									
									//<<수정>
									//tmpModVertWeight._velocity_1F = tmpNextVelocity;//이게 관성을 수정한 버전
									//tmpModVertWeight._velocity_1F = tmpModVertWeight._velocity_Real;//<<이게 이전 버전
									
									//속도 차이가 크다면 Real의 비중이 커야 한다.
									//같은 방향이면 -> 버티기 관성이 더 잘보이는게 좋다
									//다른 방향이면 Real을 관성으로 사용해야한다. (그래야 다음 프레임에 관성이 크게 보임)
									//속도 변화에 따라서 체크
									float velocityRefreshITP_X = Mathf.Clamp01(Mathf.Abs( ((tmpModVertWeight._velocity_Real.x - tmpModVertWeight._velocity_Real1F.x) / (Mathf.Abs(tmpModVertWeight._velocity_Real1F.x) + 0.1f)) * 0.5f ) );
									float velocityRefreshITP_Y = Mathf.Clamp01(Mathf.Abs( ((tmpModVertWeight._velocity_Real.y - tmpModVertWeight._velocity_Real1F.y) / (Mathf.Abs(tmpModVertWeight._velocity_Real1F.y) + 0.1f)) * 0.5f ) );

									//tmpModVertWeight._velocity_1F = tmpNextVelocity * (1.0f - inertiaK) + (inertiaK * (tmpNextVelocity * 0.7f + tmpModVertWeight._velocity_Real * 0.3f));//<<대충 섞어서..
									tmpModVertWeight._velocity_1F.x = tmpNextVelocity.x * (1.0f - velocityRefreshITP_X) + (tmpNextVelocity.x * 0.5f + tmpModVertWeight._velocity_Real.x * 0.5f) * velocityRefreshITP_X;
									tmpModVertWeight._velocity_1F.y = tmpNextVelocity.y * (1.0f - velocityRefreshITP_Y) + (tmpNextVelocity.y * 0.5f + tmpModVertWeight._velocity_Real.y * 0.5f) * velocityRefreshITP_Y;

									tmpModVertWeight._pos_1F = tmpModVertWeight._pos_Real;

									//-----------------------------------------------------------------------------------------


									//Damping
									if (tmpModVertWeight._calculatedDeltaPos.sqrMagnitude < tmpPhysicMeshParam._damping * tmpPhysicMeshParam._damping
										&& tmpNextVelocity.sqrMagnitude < tmpPhysicMeshParam._damping * tmpPhysicMeshParam._damping)
									{
										tmpModVertWeight._calculatedDeltaPos = Vector2.zero;
										tmpModVertWeight.DampPhysicVertex();
									}
								}

								//if (iPos == 0)
								//{
								//if (!isValidFrame)
								//{
								//	Debug.Log("Physics : " + tmpModVertWeight._calculatedDeltaPos + " (" + isValidFrame + " / " + tDelta + ")");
								//}
								//}
								tmpPosList[iPos] +=
										(tmpModVertWeight._calculatedDeltaPos * tmpModVertWeight._weight)
										* paramKeyValue._weight;//<<이 값을 이용한다.


							}
							//---------------------------- Pos List
						}
						if (isFirstParam)
						{
							isFirstParam = false;
						}


						nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params



					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);


					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.


					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)//<<변경
					{
						for (int iPos = 0; iPos < posList.Length; iPos++)
						{
							posList[iPos] = tmpPosList[iPos] * layerWeight;
						}
					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//변경 19.5.20 : weightedVertData를 더이상 사용하지 않음
									//if (weightedVertData != null)
									//{
									//	//Vertex 가중치가 추가되었다.
									//	float vertWeight = 0.0f;
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

									//		posList[iPos] += tmpPosList[iPos] * vertWeight;
									//	}
									//}
									//else
									//{
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		posList[iPos] += tmpPosList[iPos] * layerWeight;
									//	}
									//}

									//변경됨 19.5.20
									for (int iPos = 0; iPos < posList.Length; iPos++)
									{
										posList[iPos] += tmpPosList[iPos] * layerWeight;
									}
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//변경 19.5.20 : weightedVertData를 더이상 사용하지 않음
									//if (weightedVertData != null)
									//{
									//	//Vertex 가중치가 추가되었다.
									//	float vertWeight = 0.0f;
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		vertWeight = layerWeight * weightedVertData._vertWeightList[iPos];

									//		posList[iPos] = (posList[iPos] * (1.0f - vertWeight)) +
									//						(tmpPosList[iPos] * vertWeight);
									//	}
									//}
									//else
									//{
									//	for (int iPos = 0; iPos < posList.Length; iPos++)
									//	{
									//		posList[iPos] = (posList[iPos] * (1.0f - layerWeight)) +
									//						(tmpPosList[iPos] * layerWeight);
									//	}
									//}

									//변경됨 19.5.20
									for (int iPos = 0; iPos < posList.Length; iPos++)
									{
										posList[iPos] = (posList[iPos] * (1.0f - layerWeight)) +
														(tmpPosList[iPos] * layerWeight);
									}
								}
								break;

							default:
								Debug.LogError("Mod-Physics : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}
					}

					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				calParam._isAvailable = true;


			}
		}





		//------------------------------------------------------------------------------------------------------------------------------------
		// ModMeshSet을 사용하는 버전 (v1.1.7)에서의 Calculate 코드들
		//------------------------------------------------------------------------------------------------------------------------------------

		


		//--------------------------------------------------------------------------
		// Morph
		//--------------------------------------------------------------------------
		private void Calculate_Morph_UseModMeshSet(float tDelta)
		{

			
//#if UNITY_EDITOR
//			Profiler.BeginSample("Modifier - Calculate Morph");
//#endif

			//bool isFirstDebug = true;
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;
			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				
				//1. 계산 [중요]
				isUpdatable = calParam.Calculate();
				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//추가 : 색상 처리 초기화
				calParam._isColorCalculated = false;


				//계산 결과를 Vertex에 넣어줘야 한다.
				//구버전
				//posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;

				//신버전
				calParamVertRequestList = calParam._result_VertLocalPairs;


				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;

				//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
				//weightedVertData = calParam._weightedVertexData;

				//일단 초기화
				//구버전
				//for (int iPos = 0; iPos < posList.Length; iPos++)
				//{
				//	posList[iPos] = Vector2.zero;
				//}

				//신버전
				for (int iVR = 0; iVR < calParamVertRequestList.Count; iVR++)
				{
					calParamVertRequestList[iVR].InitCalculate();
				}


				if (_isColorProperty)
				{
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					calParam._result_IsVisible = false;//Alpha와 달리 Visible 값은 false -> OR 연산으로 작동한다.
				}
				else
				{
					calParam._result_IsVisible = true;
				}


				//추가 12.5 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;


				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;



				////추가 12.5 : Extra Option 계산 값				
				//tmpExtra_DepthChanged = false;
				//tmpExtra_TextureChanged = false;
				//tmpExtra_DeltaDepth = 0;
				//tmpExtra_TextureDataID = 0;
				//tmpExtra_TextureData = null;
				//tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				//tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					paramKeyValue = null;

					
					keyParamSetGroup = curSubList._keyParamSetGroup;
					
					//Vertex Pos 대신 Vertex Requst를 보간하자
					vertRequest = curSubList._vertexRequest;
					

					tmpColor = Color.clear;
					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled;

					//추가 20.2.24 : 색상 토글 옵션
					tmpIsToggleShowHideOption = !IsAnimated && tmpIsColoredKeyParamSetGroup && keyParamSetGroup._isToggleShowHideWithoutBlend;
					
					if(tmpIsToggleShowHideOption)
					{
						tmpToggleOpt_IsAnyKey_Shown = false;
						tmpToggleOpt_TotalWeight_Shown = 0.0f;
						tmpToggleOpt_MaxWeight_Shown = 0.0f;
						tmpToggleOpt_KeyIndex_Shown = 0.0f;
						tmpToggleOpt_IsAny_Hidden = false;
						tmpToggleOpt_TotalWeight_Hidden = 0.0f;
						tmpToggleOpt_MaxWeight_Hidden = 0.0f;
						tmpToggleOpt_KeyIndex_Hidden = 0.0f;
					}

					//변경 20.4.20 : 초기화 위치가 여기여야 한다.
					if(_isExtraPropertyEnabled)
					{
						//추가 12.5 : Extra Option 계산 값				
						tmpExtra_DepthChanged = false;
						tmpExtra_TextureChanged = false;
						tmpExtra_DeltaDepth = 0;
						tmpExtra_TextureDataID = 0;
						tmpExtra_TextureData = null;
						tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
						tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값
					}

					//-------------------------------------------
					// 여기가 과부하가 가장 심한 곳이다! 우오오오
					//-------------------------------------------

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						if (!paramKeyValue._isCalculated)
						{ continue; }

						tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

						
						//최적화해야할 부분 1)
						//구버전)
						//Pos를 일일이 돌게 아니라 VertexRequst의 Weight만 지정하자
						////---------------------------- Pos List
						//for (int iPos = 0; iPos < posList.Length; iPos++)
						//{
						//	//calculatedValue = paramKeyValue._modifiedValue._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//	tmpPosList[iPos] += paramKeyValue._modifiedMesh._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//}
						////---------------------------- Pos List

						//>> 최적화 코드)
						vertRequest._modWeightPairs[iPV].SetWeight(paramKeyValue._weight);



						//---------------------------- Color
						if (tmpIsColoredKeyParamSetGroup)
						{
							tmpSubModMesh_Color = paramKeyValue._modifiedMeshSet.SubModMesh_Color;
							if (tmpSubModMesh_Color != null)
							{
								if (!tmpIsToggleShowHideOption)
								{
									//기본방식
									if (tmpSubModMesh_Color._isVisible)
									{
										tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
										tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
									}
									else
									{
										tmpParamColor = tmpSubModMesh_Color._meshColor;
										tmpParamColor.a = 0.0f;
										tmpColor += tmpParamColor * paramKeyValue._weight;
									}
								}
								else
								{
									//추가 20.2.24 : 토글 방식의 ShowHide 방식
									if (tmpSubModMesh_Color._isVisible && paramKeyValue._weight > 0.0f)
									{
										//paramKeyValue._paramSet.ControlParamValue
										tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
										tmpVisible = true;//< 일단 이것도 true

										//토글용 처리
										tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

										//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
										if (!tmpToggleOpt_IsAnyKey_Shown)
										{
											tmpToggleOpt_KeyIndex_Shown = tmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Show Key Index 중 가장 작은 값을 기준으로 한다.
											tmpToggleOpt_KeyIndex_Shown = (tmpToggleOpt_KeyIndex_Cal < tmpToggleOpt_KeyIndex_Shown ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Shown);
										}


										tmpToggleOpt_IsAnyKey_Shown = true;

										tmpToggleOpt_TotalWeight_Shown += paramKeyValue._weight;
										tmpToggleOpt_MaxWeight_Shown = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Shown ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Shown);

									}
									else
									{
										//토글용 처리
										tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

										if (!tmpToggleOpt_IsAny_Hidden)
										{
											tmpToggleOpt_KeyIndex_Hidden = tmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
											tmpToggleOpt_KeyIndex_Hidden = (tmpToggleOpt_KeyIndex_Cal > tmpToggleOpt_KeyIndex_Hidden ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Hidden);
										}

										tmpToggleOpt_IsAny_Hidden = true;
										tmpToggleOpt_TotalWeight_Hidden += paramKeyValue._weight;
										tmpToggleOpt_MaxWeight_Hidden = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Hidden ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Hidden);
									}
								}
							}
						}

						nColorCalculated++;
						//---------------------------- Color

						//---------------------------- Extra Option
						//추가 12.5 : Extra Option
						if (_isExtraPropertyEnabled)
						{
							//이전 코드
							//1. Modifier의 Extra Property가 켜져 있어야 한다.
							//2. 현재 ParamKeyValue의 ModMesh의 Depth나 TextureData Changed 옵션이 켜져 있어야 한다.
							//2-1. Depth인 경우 Ex-Transform이 켜져 있어야 한다.
							//2-2. Texture인 경우 Ex-Color가 켜져 있어야 한다.
							//if (paramKeyValue._modifiedMesh._isExtraValueEnabled
							//	&& (paramKeyValue._modifiedMesh._extraValue._isDepthChanged || paramKeyValue._modifiedMesh._extraValue._isTextureChanged)
							//	)
							//{
							//	//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
							//	float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
							//	float bias = 0.0001f;
							//	float overlapBias = 0.01f;
							//	float cutOut = 0.0f;
							//	bool isExactWeight = false;
							//	if (IsAnimated)
							//	{
							//		switch (paramKeyValue._animKeyPos)
							//		{
							//			case apOptCalculatedResultParam.AnimKeyPos.ExactKey: isExactWeight = true; break;
							//			case apOptCalculatedResultParam.AnimKeyPos.NextKey: cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout_AnimPrev; break; //Next Key라면 Prev와의 CutOut을 가져온다.
							//			case apOptCalculatedResultParam.AnimKeyPos.PrevKey: cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout_AnimNext; break;//Prev Key라면 Next와의 CutOut을 가져온다.
							//		}
							//	}
							//	else
							//	{
							//		cutOut = paramKeyValue._modifiedMesh._extraValue._weightCutout;
							//	}

							//	cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

							//	if (isExactWeight)
							//	{
							//		extraWeight = 10000.0f;
							//	}
							//	else if (cutOut < bias)
							//	{
							//		//정확하면 최대값
							//		//아니면 적용안함
							//		if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
							//		else { extraWeight = -1.0f; }
							//	}
							//	else
							//	{
							//		if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
							//		else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
							//	}

							//	if (extraWeight > 0.0f)
							//	{
							//		if (paramKeyValue._modifiedMesh._extraValue._isDepthChanged)
							//		{
							//			//2-1. Depth 이벤트
							//			if(extraWeight > tmpExtra_DepthMaxWeight)
							//			{
							//				//가중치가 최대값보다 큰 경우
							//				tmpExtra_DepthMaxWeight = extraWeight;
							//				tmpExtra_DepthChanged = true;
							//				tmpExtra_DeltaDepth = paramKeyValue._modifiedMesh._extraValue._deltaDepth;
							//			}

							//		}
							//		if (paramKeyValue._modifiedMesh._extraValue._isTextureChanged)
							//		{
							//			//2-2. Texture 이벤트
							//			if(extraWeight > tmpExtra_TextureMaxWeight)
							//			{
							//				//가중치가 최대값보다 큰 경우
							//				tmpExtra_TextureMaxWeight = extraWeight;
							//				tmpExtra_TextureChanged = true;
							//				tmpExtra_TextureData = paramKeyValue._modifiedMesh._extraValue._linkedTextureData;
							//				tmpExtra_TextureDataID = paramKeyValue._modifiedMesh._extraValue._textureDataID;
							//			}
							//		}
							//	}
							//}

							//변경
							tmpSubModMesh_Extra = paramKeyValue._modifiedMeshSet.SubModMesh_Extra;

							if (tmpSubModMesh_Extra != null
									&& (tmpSubModMesh_Extra._extraValue._isDepthChanged || tmpSubModMesh_Extra._extraValue._isTextureChanged)
									)
							{
								//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
								float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
								float bias = 0.0001f;
								float overlapBias = 0.01f;
								float cutOut = 0.0f;
								bool isExactWeight = false;
								if (IsAnimated)
								{
									switch (paramKeyValue._animKeyPos)
									{
										case apOptCalculatedResultParam.AnimKeyPos.ExactKey:
											isExactWeight = true;
											break;
										case apOptCalculatedResultParam.AnimKeyPos.NextKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimPrev;
											break; //Next Key라면 Prev와의 CutOut을 가져온다.
										case apOptCalculatedResultParam.AnimKeyPos.PrevKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimNext;
											break;//Prev Key라면 Next와의 CutOut을 가져온다.
									}
								}
								else
								{
									cutOut = tmpSubModMesh_Extra._extraValue._weightCutout;
								}

								cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

								if (isExactWeight)
								{
									extraWeight = 10000.0f;
								}
								else if (cutOut < bias)
								{
									//정확하면 최대값
									//아니면 적용안함
									if (extraWeight > 1.0f - bias) { extraWeight = 10000.0f; }
									else { extraWeight = -1.0f; }
								}
								else
								{
									if (extraWeight < 1.0f - cutOut) { extraWeight = -1.0f; }
									else { extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
								}

								if (extraWeight > 0.0f)
								{
									if (tmpSubModMesh_Extra._extraValue._isDepthChanged)
									{
										//2-1. Depth 이벤트
										if (extraWeight > tmpExtra_DepthMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_DepthMaxWeight = extraWeight;
											tmpExtra_DepthChanged = true;
											tmpExtra_DeltaDepth = tmpSubModMesh_Extra._extraValue._deltaDepth;
										}

									}
									if (tmpSubModMesh_Extra._extraValue._isTextureChanged)
									{
										//2-2. Texture 이벤트
										if (extraWeight > tmpExtra_TextureMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_TextureMaxWeight = extraWeight;
											tmpExtra_TextureChanged = true;
											tmpExtra_TextureData = tmpSubModMesh_Extra._extraValue._linkedTextureData;
											tmpExtra_TextureDataID = tmpSubModMesh_Extra._extraValue._textureDataID;
										}
									}
								}
							}
						}
						//---------------------------- Extra Option


//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif
					}//--- Params

//#if UNITY_EDITOR
//					Profiler.EndSample();
//#endif

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.
					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));
					}


					if (layerWeight < 0.001f)
					{
						continue;
					}

					vertRequest.SetCalculated();//<<일단 계산하기 위해 참조 했음을 알린다.

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					if (nColorCalculated == 0)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
					}

					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)
					{
						//구버전 : Vertex Pos를 직접 수정
						//for (int iPos = 0; iPos < posList.Length; iPos++)
						//{
						//	posList[iPos] = tmpPosList[iPos] * layerWeight;
						//}

						//신버전 : VertexRequest에 넣자
						vertRequest.MultiplyWeight(layerWeight);
					}
					else
					{

//#if UNITY_EDITOR
//						Profiler.BeginSample("Modifier - Calculate Morph > Overlap Pos List");
//#endif

						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//VertexRequest에 넣자
									//Additive : Prev + Next * weight이므로
									//Next에만 weight를 곱한다.
									vertRequest.MultiplyWeight(layerWeight);

								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//VertexRequest에 넣자
									//Interpolation : Prev * (1-weight) + Next * weight이므로
									//Prev에 1-weight
									//Next에 weight
									//단, 계산 안한건 제외한다.
									float invWeight = 1.0f - layerWeight;
									for (int iVR = 0; iVR < calParamVertRequestList.Count; iVR++)
									{
										tmpVertRequest = calParamVertRequestList[iVR];
										if(!tmpVertRequest._isCalculated)
										{
											//아직 계산 안한건 패스
											continue;
										}
										if(tmpVertRequest == vertRequest)
										{
											//Next엔 * weight
											tmpVertRequest.MultiplyWeight(layerWeight);
										}
										else
										{
											//Prev엔 * (1-weight)
											tmpVertRequest.MultiplyWeight(invWeight);
										}
									}
								}
								break;

							default:
								Debug.LogError("Mod-Morph : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}

//#if UNITY_EDITOR
//						Profiler.EndSample();
//#endif

					}


					if (tmpIsColoredKeyParamSetGroup)
					{
						//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.
						if (tmpIsToggleShowHideOption)
						{	
							if (tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (tmpToggleOpt_MaxWeight_Shown > tmpToggleOpt_MaxWeight_Hidden)
								{
									//Show가 더 크다
									tmpVisible = true;
								}
								else if (tmpToggleOpt_MaxWeight_Shown < tmpToggleOpt_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									tmpVisible = false;
									tmpColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (tmpToggleOpt_KeyIndex_Shown > tmpToggleOpt_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										tmpVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										tmpVisible = false;
										tmpColor = Color.clear;
									}
								}
							}
							else if (tmpToggleOpt_IsAnyKey_Shown && !tmpToggleOpt_IsAny_Hidden)
							{
								//Show만 있다면
								tmpVisible = true;
							}
							else if (!tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
							{
								//Hide만 있다면
								tmpVisible = false;
								tmpColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								tmpVisible = false;
								tmpColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (tmpVisible && tmpToggleOpt_TotalWeight_Shown > 0.0f)
							{
								tmpColor.r = Mathf.Clamp01(tmpColor.r / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.g = Mathf.Clamp01(tmpColor.g / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.b = Mathf.Clamp01(tmpColor.b / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.a = Mathf.Clamp01(tmpColor.a / tmpToggleOpt_TotalWeight_Shown);
							}
						}

						if (iColoredKeyParamSetGroup == 0 || keyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						iColoredKeyParamSetGroup++;
						calParam._isColorCalculated = true;
					}


					//추가 12.5 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(tmpExtra_DepthChanged)
						{
							calParam._isExtra_DepthChanged = true;
							calParam._extra_DeltaDepth = tmpExtra_DeltaDepth;
						}

						if(tmpExtra_TextureChanged)
						{
							calParam._isExtra_TextureChanged = true;
							calParam._extra_TextureData = tmpExtra_TextureData;
							calParam._extra_TextureDataID = tmpExtra_TextureDataID;
						}
					}


					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)




				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;
				}

			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif	
		}



		//추가 20.4.18 : Morph의 Animation용 처리 로직을 분리했다. 레이어 처리 때문
		private void Calculate_Morph_UseModMeshSet_Animation(float tDelta)
		{

			
//#if UNITY_EDITOR
//			Profiler.BeginSample("Modifier - Calculate Morph");
//#endif

			//bool isFirstDebug = true;
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;
			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				
				//1. 계산 [중요]
				//isUpdatable = calParam.Calculate();//TODO : 최적화를 위해서 이 함수 자체가 사용되지 않아야 한다.

				//변경 20.11.23 : 애니메이션 모디파이어용 최적화된 Calculate 함수
				isUpdatable = calParam.Calculate_AnimMod();

				

				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//추가 : 색상 처리 초기화
				calParam._isColorCalculated = false;


				//계산 결과를 Vertex에 넣어줘야 한다.
				//구버전
				//posList = calParam._result_Positions;
				//tmpPosList = calParam._tmp_Positions;

				//신버전
				calParamVertRequestList = calParam._result_VertLocalPairs;

				//초기화
				//subParamGroupList = calParam._subParamKeyValueList;//삭제 20.11.25 : 사용되지 않는다.


				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;
				_keyAnimClip = null;
				_keyAnimPlayUnit = null;

				//신버전
				for (int iVR = 0; iVR < calParamVertRequestList.Count; iVR++)
				{
					calParamVertRequestList[iVR].InitCalculate();
				}


				if (_isColorProperty)
				{
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					calParam._result_IsVisible = false;//Alpha와 달리 Visible 값은 false -> OR 연산으로 작동한다.
				}
				else
				{
					calParam._result_IsVisible = true;
				}


				//추가 12.5 : Extra Option 초기화
				//이건 ModMesh에서 값을 가진 경우에 한해서만 계산이 된다.
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;





				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;



				////추가 12.5 : Extra Option 계산 값				
				//tmpExtra_DepthChanged = false;
				//tmpExtra_TextureChanged = false;
				//tmpExtra_DeltaDepth = 0;
				//tmpExtra_TextureDataID = 0;
				//tmpExtra_TextureData = null;
				//tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				//tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


				//애니메이션 레이어 변수도 초기화 << 추가 20.4.18
				_curAnimLayeredParam = null;
				_nAnimLayeredParams = 0;
				_curAnimLayer = -1;
				_isAnimAnyColorCalculated = false;
				_isAnimAnyExtraCalculated = false;

				//for (int iLayer = 0; iLayer < _animLayeredParams.Count; iLayer++)
				//{
				//	_animLayeredParams[iLayer].ReadyToUpdate();
				//}

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것



				//이전 : 모든 SubList를 순회
				//for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				//{
				//	curSubList = subParamGroupList[iSubList];

				//변경 20.11.23 : AnimPlayMapping을 이용하여 미리 정렬된 순서로 SubList를 호출
				int iTargetSubList = 0;
				apAnimPlayMapping.LiveUnit curUnit = null;
				for (int iUnit = 0; iUnit < _animPlayMapping._nAnimClips; iUnit++)
				{
					curUnit = _animPlayMapping._liveUnits_Sorted[iUnit];
					if(!curUnit._isLive)
					{
						//재생 종료
						//이 뒤는 모두 재생이 안되는 애니메이션이다.
						break;
					}

					iTargetSubList = curUnit._animIndex;
					curSubList = calParam._subParamKeyValueList_AnimSync[iTargetSubList];
					if(curSubList == null)
					{
						//이게 Null이라는 것은, 이 AnimClip에 대한 TimelineLayer와 Mod는 없다는 것
						continue;
					}
					
					//----------------------여기까지


					//이전 방식
					//int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					//subParamKeyValueList = curSubList._subParamKeyValues;


					//다른 방식 20.11.24 : 모든 ParamKeyValue가 아니라, 유효한 ParamKeyValue만 체크한다.
					int nValidParamKeyValues = curSubList._nResultAnimKey;
					if(nValidParamKeyValues == 0)
					{
						continue;
					}





					paramKeyValue = null;


					keyParamSetGroup = curSubList._keyParamSetGroup;

					//삭제 20.11.24 : 위에서 AnimPlayMapping의 LiveUnit에서 미리 결정된다.
					////애니메이션 모디파이어에서 실행되지 않은 애니메이션에 대한 PSG는 생략한다.
					//if (!keyParamSetGroup.IsAnimEnabled)
					//{
					//	continue;
					//}


					//이 로직이 추가된다. (20.4.18)
					_keyAnimClip = keyParamSetGroup._keyAnimClip;
					_keyAnimPlayUnit = _keyAnimClip._parentPlayUnit;

					//레이어별로 데이터를 그룹으로 만들어서 계산하자
					if (_curAnimLayer != _keyAnimPlayUnit._layer
						|| _curAnimLayeredParam == null)
					{
						//다음 레이어를 꺼내자
						//- 풀에 이미 생성된 레이어가 있다면 가져오고, 없으면 생성한다.
						if (_nAnimLayeredParams < _animLayeredParams.Count)
						{
							//가져올 레이어 파라미터가 있다.
							_curAnimLayeredParam = _animLayeredParams[_nAnimLayeredParams];
							_curAnimLayeredParam.ReadyToUpdate();
						}
						else
						{
							//풀에 레이어 파라미터가 없으므로 새로 만들어서 풀에 넣어주자
							_curAnimLayeredParam = AnimLayeredParam.Make_Morph(_nAnimLayeredParams);//생성자에 ReadyToUpdate가 포함되어 있다.
							_animLayeredParams.Add(_curAnimLayeredParam);
						}

						_curAnimLayer = _keyAnimPlayUnit._layer;
						_nAnimLayeredParams++;//사용된 레이어 파라미터의 개수
					}




					//Vertex Pos 대신 Vertex Requst를 보간하자
					vertRequest = curSubList._vertexRequest;


					tmpColor = Color.clear;
					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled;

					//추가 20.2.24 : 색상 토글 옵션
					tmpIsToggleShowHideOption = !IsAnimated && tmpIsColoredKeyParamSetGroup && keyParamSetGroup._isToggleShowHideWithoutBlend;

					if (tmpIsToggleShowHideOption)
					{
						tmpToggleOpt_IsAnyKey_Shown = false;
						tmpToggleOpt_TotalWeight_Shown = 0.0f;
						tmpToggleOpt_MaxWeight_Shown = 0.0f;
						tmpToggleOpt_KeyIndex_Shown = 0.0f;
						tmpToggleOpt_IsAny_Hidden = false;
						tmpToggleOpt_TotalWeight_Hidden = 0.0f;
						tmpToggleOpt_MaxWeight_Hidden = 0.0f;
						tmpToggleOpt_KeyIndex_Hidden = 0.0f;
					}


					//변경 20.4.20 : 초기화 위치가 여기여야 한다.
					if(_isExtraPropertyEnabled)
					{
						//추가 12.5 : Extra Option 계산 값				
						tmpExtra_DepthChanged = false;
						tmpExtra_TextureChanged = false;
						tmpExtra_DeltaDepth = 0;
						tmpExtra_TextureDataID = 0;
						tmpExtra_TextureData = null;
						tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
						tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값
					}

					//-------------------------------------------
					// 여기가 과부하가 가장 심한 곳이다! 우오오오
					//-------------------------------------------

					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					//변경 20.11.24 모든 키프레임 순회 방식 제거
					//이전 방식
					//for (int iPV = 0; iPV < nParamKeys; iPV++)
					//{
					//	paramKeyValue = subParamKeyValueList[iPV];

					//변경 20.11.24 : 전체 체크 > 이미 보간된 것만 체크
					int iRealPKV = -1;
					for (int iPV = 0; iPV < nValidParamKeyValues; iPV++)
					{
						paramKeyValue = curSubList._resultAnimKeyPKVs[iPV];//보간이 완료된 PKV만 가져온다.
						iRealPKV = curSubList._resultAnimKeyPKVIndices[iPV];//실제 PKV의 인덱스는 따로 가져와야 한다.

						//여기까지...

						if (!paramKeyValue._isCalculated 
							|| iRealPKV < 0//추가 20.11.25
							)
						{ continue; }

						tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;


						//최적화해야할 부분 1)
						//구버전)
						//Pos를 일일이 돌게 아니라 VertexRequst의 Weight만 지정하자
						////---------------------------- Pos List
						//for (int iPos = 0; iPos < posList.Length; iPos++)
						//{
						//	//calculatedValue = paramKeyValue._modifiedValue._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//	tmpPosList[iPos] += paramKeyValue._modifiedMesh._vertices[iPos]._deltaPos * paramKeyValue._weight;
						//}
						////---------------------------- Pos List

						//>> 최적화 코드)
						//vertRequest._modWeightPairs[iPV].SetWeight(paramKeyValue._weight);//이전 : 이 인덱스를 사용하면 안된다.

						vertRequest._modWeightPairs[iRealPKV].SetWeight(paramKeyValue._weight);//변경 20.11.25 : 별도의 인덱스를 사용



						//---------------------------- Color
						if (tmpIsColoredKeyParamSetGroup)
						{
							tmpSubModMesh_Color = paramKeyValue._modifiedMeshSet.SubModMesh_Color;
							if (tmpSubModMesh_Color != null)
							{
								if (!tmpIsToggleShowHideOption)
								{
									//기본방식
									if (tmpSubModMesh_Color._isVisible)
									{
										tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
										tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
									}
									else
									{
										tmpParamColor = tmpSubModMesh_Color._meshColor;
										tmpParamColor.a = 0.0f;
										tmpColor += tmpParamColor * paramKeyValue._weight;
									}
								}
								else
								{
									//추가 20.2.24 : 토글 방식의 ShowHide 방식
									if (tmpSubModMesh_Color._isVisible && paramKeyValue._weight > 0.0f)
									{
										//paramKeyValue._paramSet.ControlParamValue
										tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
										tmpVisible = true;//< 일단 이것도 true

										//토글용 처리
										tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

										//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
										if (!tmpToggleOpt_IsAnyKey_Shown)
										{
											tmpToggleOpt_KeyIndex_Shown = tmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Show Key Index 중 가장 작은 값을 기준으로 한다.
											tmpToggleOpt_KeyIndex_Shown = (tmpToggleOpt_KeyIndex_Cal < tmpToggleOpt_KeyIndex_Shown ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Shown);
										}


										tmpToggleOpt_IsAnyKey_Shown = true;

										tmpToggleOpt_TotalWeight_Shown += paramKeyValue._weight;
										tmpToggleOpt_MaxWeight_Shown = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Shown ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Shown);

									}
									else
									{
										//토글용 처리
										tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

										if (!tmpToggleOpt_IsAny_Hidden)
										{
											tmpToggleOpt_KeyIndex_Hidden = tmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
											tmpToggleOpt_KeyIndex_Hidden = (tmpToggleOpt_KeyIndex_Cal > tmpToggleOpt_KeyIndex_Hidden ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Hidden);
										}

										tmpToggleOpt_IsAny_Hidden = true;
										tmpToggleOpt_TotalWeight_Hidden += paramKeyValue._weight;
										tmpToggleOpt_MaxWeight_Hidden = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Hidden ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Hidden);
									}
								}
							}
						}

						nColorCalculated++;
						//---------------------------- Color

						//---------------------------- Extra Option
						//추가 12.5 : Extra Option
						if (_isExtraPropertyEnabled)
						{
							tmpSubModMesh_Extra = paramKeyValue._modifiedMeshSet.SubModMesh_Extra;

							if (tmpSubModMesh_Extra != null
									&& (tmpSubModMesh_Extra._extraValue._isDepthChanged || tmpSubModMesh_Extra._extraValue._isTextureChanged)
									)
							{
								//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
								float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
								float bias = 0.0001f;
								float overlapBias = 0.01f;
								float cutOut = 0.0f;
								bool isExactWeight = false;
								if (IsAnimated)
								{
									switch (paramKeyValue._animKeyPos)
									{
										case apOptCalculatedResultParam.AnimKeyPos.ExactKey:
											isExactWeight = true;
											break;
										case apOptCalculatedResultParam.AnimKeyPos.NextKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimPrev;
											break; //Next Key라면 Prev와의 CutOut을 가져온다.
										case apOptCalculatedResultParam.AnimKeyPos.PrevKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimNext;
											break;//Prev Key라면 Next와의 CutOut을 가져온다.
									}
								}
								else
								{
									cutOut = tmpSubModMesh_Extra._extraValue._weightCutout;
								}

								cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

								if (isExactWeight)
								{
									extraWeight = 10000.0f;
								}
								else if (cutOut < bias)
								{
									//정확하면 최대값
									//아니면 적용안함
									if (extraWeight > 1.0f - bias)	{ extraWeight = 10000.0f; }
									else							{ extraWeight = -1.0f; }
								}
								else
								{
									if (extraWeight < 1.0f - cutOut)	{ extraWeight = -1.0f; }
									else								{ extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
								}

								if (extraWeight > 0.0f)
								{
									if (tmpSubModMesh_Extra._extraValue._isDepthChanged)
									{
										//2-1. Depth 이벤트
										if (extraWeight > tmpExtra_DepthMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_DepthMaxWeight = extraWeight;
											tmpExtra_DepthChanged = true;
											tmpExtra_DeltaDepth = tmpSubModMesh_Extra._extraValue._deltaDepth;
										}

									}
									if (tmpSubModMesh_Extra._extraValue._isTextureChanged)
									{
										//2-2. Texture 이벤트
										if (extraWeight > tmpExtra_TextureMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_TextureMaxWeight = extraWeight;
											tmpExtra_TextureChanged = true;
											tmpExtra_TextureData = tmpSubModMesh_Extra._extraValue._linkedTextureData;
											tmpExtra_TextureDataID = tmpSubModMesh_Extra._extraValue._textureDataID;
										}
									}
								}
							}
						}
						//---------------------------- Extra Option

					}//--- Params

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.
					if (!_isUseParamSetWeight)
					{
						//layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);//이전
						layerWeight = Mathf.Clamp01(curUnit._playWeight);//변경 20.11.23 : 일일이 계산된 KeyParamSetGroup의 Weight대신, 일괄 계산된 LiveUnit의 값을 이용
					}
					else
					{
						//layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));//이전
						layerWeight = Mathf.Clamp01(curUnit._playWeight * Mathf.Clamp01(tmpTotalParamSetWeight));//변경 20.11.23 : 위와 동일
					}


					if (layerWeight < 0.001f)
					{
						continue;
					}

					vertRequest.SetCalculated();//<<일단 계산하기 위해 참조 했음을 알린다.

					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					if (nColorCalculated == 0)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
					}





					//중요! 20.4.18
					//계산한 값을 애니메이션 레이어에 넣자
					//TF방식과 다른 것은, 여기서는 Weight연산하지 않고 기록만 하는 것이다.
					//나중에 다 처리하기 전에 한꺼번에 계산해야한다.
					_curAnimLayeredParam._totalWeight += layerWeight;

					if (_curAnimLayeredParam._iCurAnimClip == 0)
					{
						//첫번째 애니메이션 클립일 때
						//레이어의 블렌드 방식을 첫번째 클립의 값을 따른다.
						if (_keyAnimPlayUnit.BlendMethod == apAnimPlayUnit.BLEND_METHOD.Interpolation)
						{
							_curAnimLayeredParam._blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;
						}
						else
						{
							_curAnimLayeredParam._blendMethod = apModifierParamSetGroup.BLEND_METHOD.Additive;
						}

						_curAnimLayeredParam._isCalculated = true;
					}

					//VertexRequest와 Weight를 기록하자
					_curAnimLayeredParam._cal_VertRequests.Add(vertRequest);
					_curAnimLayeredParam._cal_VertWeight.Add(layerWeight);
					_curAnimLayeredParam._cal_NumVertRequest++;

					//색상 옵션도 레이어 파라미터에 넣자.
					if (tmpIsColoredKeyParamSetGroup)
					{
						//레이어 내에서는 무조건 Interpolation이다.
						if (_curAnimLayeredParam._nCal_Color == 0)
						{
							//첫번째는 그대로 대입
							_curAnimLayeredParam._cal_Color = tmpColor;
							_curAnimLayeredParam._cal_Visible = tmpVisible;
						}
						else
						{
							//두번째부터는 Interpolation
							_curAnimLayeredParam._cal_Color = apUtil.BlendColor_ITP(
																_curAnimLayeredParam._cal_Color,
																tmpColor,
																layerWeight);

							//Visible은 OR연산
							_curAnimLayeredParam._cal_Visible |= tmpVisible;
						}

						_curAnimLayeredParam._isCal_Color = true;
						_curAnimLayeredParam._nCal_Color++;

						_isAnimAnyColorCalculated = true;
					}


					//Extra Option > 레이어 파라미터
					if (_isExtraPropertyEnabled)
					{
						if (tmpExtra_DepthChanged)
						{
							_curAnimLayeredParam._isCal_ExtraDepth = true;
							_curAnimLayeredParam._cal_ExtraDepth = tmpExtra_DeltaDepth;

							_isAnimAnyExtraCalculated = true;
						}
						else
						{
							_curAnimLayeredParam._isCal_ExtraDepth = false;
							_curAnimLayeredParam._cal_ExtraDepth = 0;
						}

						if (tmpExtra_TextureChanged)
						{
							_curAnimLayeredParam._isCal_ExtraTexture = true;
							_curAnimLayeredParam._cal_ExtraTexture = tmpExtra_TextureData;
							_curAnimLayeredParam._cal_ExtraTextureID = tmpExtra_TextureDataID;

							_isAnimAnyExtraCalculated = true;
						}
						else
						{
							_curAnimLayeredParam._isCal_ExtraTexture = false;
							_curAnimLayeredParam._cal_ExtraTexture = null;
							_curAnimLayeredParam._cal_ExtraTextureID = -1;
						}
					}


					_curAnimLayeredParam._iCurAnimClip++;

					iCalculatedSubParam++;
				}

				//KeyParamSetGroup > 레이어 데이터로 누적 끝


				//CalParam을 계산하자
				//Vertex Request와 Color+Extra를 따로 계산한다.
				//ModVert의 레이어 값은 저장된 값은 <거꾸로> 계산하여 Weight를 할당할 수 있다.
				//Color+Extra는 순서대로 한다.

				if (_nAnimLayeredParams > 0)
				{
					float remainLayerWeight = 1.0f;
					for (int iAnimLayeredParam = _nAnimLayeredParams-1; iAnimLayeredParam >= 0; iAnimLayeredParam--)
					{
						_curAnimLayeredParam = _animLayeredParams[iAnimLayeredParam];
						if(!_curAnimLayeredParam._isCalculated)
						{
							continue;
						}

						_layeredAnimWeightClamped = Mathf.Clamp01(_curAnimLayeredParam._totalWeight);
						calParam._totalParamSetGroupWeight += _layeredAnimWeightClamped;


						//레이어의 총 Weight값이 1보다 크다면, Normalize를 한다.
						float normalRatio = 1.0f;
						if(_curAnimLayeredParam._totalWeight > 1.0f)
						{
							normalRatio = 1.0f / _curAnimLayeredParam._totalWeight;
						}

						float mulWeight = 1.0f;
						for (int iVertReq = 0; iVertReq < _curAnimLayeredParam._cal_NumVertRequest; iVertReq++)
						{
							//현재 VertReq의 Weight의 값은
							//[남은 레이어 Weight] * [Under-Normalized Request Weight]
							mulWeight = remainLayerWeight * _curAnimLayeredParam._cal_VertWeight[iVertReq] * normalRatio;

							//Vertex Reqeust에 Weight를 할당하자
							_curAnimLayeredParam._cal_VertRequests[iVertReq].MultiplyWeight(mulWeight);
						}

						//현재 레이어의 Weight를 다 넣었으니, 그 Weight를 줄이자.
						//- Additive인 경우 줄이지 않는다. (계속 값이 누적하는 방식이므로)
						//- Interpolation인 경우 줄이자
						if(_curAnimLayeredParam._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							remainLayerWeight -= _layeredAnimWeightClamped;
							if(remainLayerWeight < 0.0f)
							{
								remainLayerWeight = 0.0f;
							}
						}
					}
				}
				

				_iColoredAnimLayeredParam = 0;
				_layeredAnimWeightClamped = 0.0f;

				//Color, Extra 처리도 하자
				if (_isAnimAnyColorCalculated || _isAnimAnyExtraCalculated)
				{
					for (int iAnimLayeredParam = 0; iAnimLayeredParam < _nAnimLayeredParams; iAnimLayeredParam++)
					{
						_curAnimLayeredParam = _animLayeredParams[iAnimLayeredParam];
						if (!_curAnimLayeredParam._isCalculated || _curAnimLayeredParam._totalWeight < 0.0001f)
						{
							//처리 끝
							//break;
							continue;
						}

						_layeredAnimWeightClamped = Mathf.Clamp01(_curAnimLayeredParam._totalWeight);

						//위에서 Vertex Reqeust의 Weight를 계산했으므로, 색상과 Extra만 계산하자


						//색상 처리
						if (_isAnimAnyColorCalculated)
						{
							if (_curAnimLayeredParam._isCal_Color)
							{
								if (_iColoredAnimLayeredParam == 0
									|| _curAnimLayeredParam._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
								{
									//색상 Interpolation
									calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color,
																					_curAnimLayeredParam._cal_Color,
																					_layeredAnimWeightClamped);
								}
								else
								{
									//색상 Additive
									calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color,
																					_curAnimLayeredParam._cal_Color,
																					_layeredAnimWeightClamped);
								}

								calParam._result_IsVisible |= _curAnimLayeredParam._cal_Visible;

								_iColoredAnimLayeredParam++;
								calParam._isColorCalculated = true;
							}
						}


						//Extra Option를 CalParam에 전달
						if(_isExtraPropertyEnabled && _isAnimAnyExtraCalculated)
						{
							//활성, 비활성에 상관없이 마지막 레이어의 값이 항상 반영된다.

							if(_curAnimLayeredParam._isCal_ExtraDepth)
							{
								calParam._isExtra_DepthChanged = true;
								calParam._extra_DeltaDepth = _curAnimLayeredParam._cal_ExtraDepth;
							}
							else
							{
								calParam._isExtra_DepthChanged = false;
								calParam._extra_DeltaDepth = 0;
							}


							if(_curAnimLayeredParam._isCal_ExtraTexture)
							{
								calParam._isExtra_TextureChanged = true;
								calParam._extra_TextureData = _curAnimLayeredParam._cal_ExtraTexture;
								calParam._extra_TextureDataID = _curAnimLayeredParam._cal_ExtraTextureID;
							}
							else
							{
								calParam._isExtra_TextureChanged = false;
								calParam._extra_TextureData = null;
								calParam._extra_TextureDataID = -1;
							}
						}

					}
				}




				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;
				}

			}

//#if UNITY_EDITOR
//			Profiler.EndSample();
//#endif	
		}





		//--------------------------------------------------------------------------
		// TF (Transform)
		//--------------------------------------------------------------------------
		private void Calculate_TF_UseModMeshSet(float tDelta)
		{
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;

			//추가 : Bone을 대상으로 하는가
			//Bone대상이면 ModBone을 사용해야한다.
			bool isBoneTarget = false;
			//bool isBoneIKControllerUsed = false;//<<Bone IK 추가

			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				if (calParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					isBoneTarget = true;
					//if(calParam._targetBone._IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
					//{
					//	isBoneIKControllerUsed = true;//<<추가
					//	Debug.Log("Bone IK Used");
					//}
					//else
					//{
					//	isBoneIKControllerUsed = false;//<<추가
					//}
				}
				else
				{
					//ModMesh를 참조하는 Param이다.
					isBoneTarget = false;
					//isBoneIKControllerUsed = false;//<<
				}
				//1. 계산 [중요]
				isUpdatable = calParam.Calculate();

				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//초기화
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				keyParamSetGroup = null;

				calParam._result_Matrix.SetIdentity();

				calParam._isColorCalculated = false;

				if (!isBoneTarget)
				{
					if (_isColorProperty)
					{
						calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						calParam._result_IsVisible = false;
					}
					else
					{
						calParam._result_IsVisible = true;
					}
				}
				else
				{
					calParam._result_IsVisible = true;
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}


				//추가 12.5 : Extra Option 초기화
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;



				//초기화
				tmpMatrix.SetIdentity();
				layerWeight = 0.0f;

				//tmpBoneIKWeight = 0.0f;
				tmpVisible = false;

				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;

				//추가 : Bone 타겟이면 BoneIKWeight를 계산해야한다.
				//calParam._result_BoneIKWeight = 0.0f;
				//calParam._isBoneIKWeightCalculated = false;

				////추가 12.5 : Extra Option 계산 값 초기화
				//tmpExtra_DepthChanged = false;
				//tmpExtra_TextureChanged = false;
				//tmpExtra_DeltaDepth = 0;
				//tmpExtra_TextureDataID = 0;
				//tmpExtra_TextureData = null;
				//tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				//tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					//>>이 검사는 필요없다. 애니메이션 로직이 분리되었기 때문
					////추가 20.4.2 : 애니메이션 모디파이어일때.
					//if(_isAnimated && !keyParamSetGroup.IsAnimEnabled)
					//{	
					//	//실행되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
					//	//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
					//	continue;
					//}
					
					//추가 21.9.2 : <회전 보정>
					_cal_isRotation180Correction = keyParamSetGroup._tfRotationLerpMethod == apOptParamSetGroup.TF_ROTATION_LERP_METHOD.RotationByVector;
					_cal_Rotation180Correction_DeltaAngle = 0.0f;


					//레이어 내부의 임시 데이터를 먼저 초기화
					tmpMatrix.SetZero();//<<TF에서 추가됨
					tmpColor = Color.clear;

					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget;


					//추가 20.2.24 : 색상 토글 옵션
					tmpIsToggleShowHideOption = !IsAnimated && tmpIsColoredKeyParamSetGroup && keyParamSetGroup._isToggleShowHideWithoutBlend;
					
					if(tmpIsToggleShowHideOption)
					{
						tmpToggleOpt_IsAnyKey_Shown = false;
						tmpToggleOpt_TotalWeight_Shown = 0.0f;
						tmpToggleOpt_MaxWeight_Shown = 0.0f;
						tmpToggleOpt_KeyIndex_Shown = 0.0f;
						tmpToggleOpt_IsAny_Hidden = false;
						tmpToggleOpt_TotalWeight_Hidden = 0.0f;
						tmpToggleOpt_MaxWeight_Hidden = 0.0f;
						tmpToggleOpt_KeyIndex_Hidden = 0.0f;
					}


					//변경 20.4.20 : 초기화 위치가 여기여야 한다.
					if(_isExtraPropertyEnabled)
					{
						//추가 12.5 : Extra Option 계산 값				
						tmpExtra_DepthChanged = false;
						tmpExtra_TextureChanged = false;
						tmpExtra_DeltaDepth = 0;
						tmpExtra_TextureDataID = 0;
						tmpExtra_TextureData = null;
						tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
						tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값
					}


					
					if (!isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우 >> ModMeshSet으로 변경

						//추가 20.9.11 : 정밀한 보간을 위해 Default Matrix가 필요하다.
						apMatrix defaultMatrixOfRenderUnit = null;
						//bool isDebug = false;
						if(calParam._targetOptTransform != null)
						{
							defaultMatrixOfRenderUnit = calParam._targetOptTransform._defaultMatrix;

							//if(calParam._targetOptTransform.gameObject.name.Contains("Debug"))
							//{
							//	isDebug = true;
							//}
						}



						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							paramKeyValue = subParamKeyValueList[iPV];

							if (!paramKeyValue._isCalculated)
							{ continue; }

							tmpSubModMesh_Transform = paramKeyValue._modifiedMeshSet.SubModMesh_Transform;

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

							//Weight에 맞게 Matrix를 만들자
							if(paramKeyValue._isAnimRotationBias)
							{
								//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								//이전 : apMatrix
								//tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);

								//변경 3.27 : apMatrixCal
								tmpMatrix.AddMatrixParallel_ModMesh(paramKeyValue.AnimRotationBiasedMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
							}
							else
							{
								//이전
								//tmpMatrix.AddMatrixParallel(paramKeyValue._modifiedMesh._transformMatrix, paramKeyValue._weight);

								//ModMeshSet으로 변경
								tmpMatrix.AddMatrixParallel_ModMesh(tmpSubModMesh_Transform._transformMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight/*, isDebug*/);
							}


							//색상 처리
							if (tmpIsColoredKeyParamSetGroup)
							{
								tmpSubModMesh_Color = paramKeyValue._modifiedMeshSet.SubModMesh_Color;
								if (tmpSubModMesh_Color != null)
								{
									if (!tmpIsToggleShowHideOption)
									{
										//기본방식
										if (tmpSubModMesh_Color._isVisible)
										{
											tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
											tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
										}
										else
										{
											tmpParamColor = tmpSubModMesh_Color._meshColor;
											tmpParamColor.a = 0.0f;
											tmpColor += tmpParamColor * paramKeyValue._weight;
										}
									}
									else
									{
										//추가 20.2.24 : 토글 방식의 ShowHide 방식
										if (tmpSubModMesh_Color._isVisible && paramKeyValue._weight > 0.0f)
										{
											//paramKeyValue._paramSet.ControlParamValue
											tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
											tmpVisible = true;//< 일단 이것도 true

											//토글용 처리
											tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

											//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
											if (!tmpToggleOpt_IsAnyKey_Shown)
											{
												tmpToggleOpt_KeyIndex_Shown = tmpToggleOpt_KeyIndex_Cal;
											}
											else
											{
												//Show Key Index 중 가장 작은 값을 기준으로 한다.
												tmpToggleOpt_KeyIndex_Shown = (tmpToggleOpt_KeyIndex_Cal < tmpToggleOpt_KeyIndex_Shown ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Shown);
											}


											tmpToggleOpt_IsAnyKey_Shown = true;

											tmpToggleOpt_TotalWeight_Shown += paramKeyValue._weight;
											tmpToggleOpt_MaxWeight_Shown = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Shown ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Shown);

										}
										else
										{
											//토글용 처리
											tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

											if (!tmpToggleOpt_IsAny_Hidden)
											{
												tmpToggleOpt_KeyIndex_Hidden = tmpToggleOpt_KeyIndex_Cal;
											}
											else
											{
												//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
												tmpToggleOpt_KeyIndex_Hidden = (tmpToggleOpt_KeyIndex_Cal > tmpToggleOpt_KeyIndex_Hidden ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Hidden);
											}

											tmpToggleOpt_IsAny_Hidden = true;
											tmpToggleOpt_TotalWeight_Hidden += paramKeyValue._weight;
											tmpToggleOpt_MaxWeight_Hidden = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Hidden ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Hidden);
										}
									}
								}
							}

							nColorCalculated++;


							//---------------------------------------------
							//추가 12.5 : Extra Option
							if (_isExtraPropertyEnabled)
							{
								//변경 : ModMeshSet을 이용하는 것으로 변경
								tmpSubModMesh_Extra = paramKeyValue._modifiedMeshSet.SubModMesh_Extra;

								if (tmpSubModMesh_Extra != null
										&& (tmpSubModMesh_Extra._extraValue._isDepthChanged || tmpSubModMesh_Extra._extraValue._isTextureChanged)
										)
								{
									//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
									float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
									float bias = 0.0001f;
									float overlapBias = 0.01f;
									float cutOut = 0.0f;
									bool isExactWeight = false;
									if (IsAnimated)
									{
										switch (paramKeyValue._animKeyPos)
										{
											case apOptCalculatedResultParam.AnimKeyPos.ExactKey:
												isExactWeight = true;
												break;
											case apOptCalculatedResultParam.AnimKeyPos.NextKey:
												cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimPrev;
												break; //Next Key라면 Prev와의 CutOut을 가져온다.
											case apOptCalculatedResultParam.AnimKeyPos.PrevKey:
												cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimNext;
												break;//Prev Key라면 Next와의 CutOut을 가져온다.
										}
									}
									else
									{
										cutOut = tmpSubModMesh_Extra._extraValue._weightCutout;
									}

									cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

									if (isExactWeight)
									{
										extraWeight = 10000.0f;
									}
									else if (cutOut < bias)
									{
										//정확하면 최대값
										//아니면 적용안함
										if (extraWeight > 1.0f - bias)
										{ extraWeight = 10000.0f; }
										else
										{ extraWeight = -1.0f; }
									}
									else
									{
										if (extraWeight < 1.0f - cutOut)
										{ extraWeight = -1.0f; }
										else
										{ extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
									}

									if (extraWeight > 0.0f)
									{
										if (tmpSubModMesh_Extra._extraValue._isDepthChanged)
										{
											//2-1. Depth 이벤트
											if (extraWeight > tmpExtra_DepthMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												tmpExtra_DepthMaxWeight = extraWeight;
												tmpExtra_DepthChanged = true;
												tmpExtra_DeltaDepth = tmpSubModMesh_Extra._extraValue._deltaDepth;
											}

										}
										if (tmpSubModMesh_Extra._extraValue._isTextureChanged)
										{
											//2-2. Texture 이벤트
											if (extraWeight > tmpExtra_TextureMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												tmpExtra_TextureMaxWeight = extraWeight;
												tmpExtra_TextureChanged = true;
												tmpExtra_TextureData = tmpSubModMesh_Extra._extraValue._linkedTextureData;
												tmpExtra_TextureDataID = tmpSubModMesh_Extra._extraValue._textureDataID;
											}
										}
									}
								}
							}
							//---------------------------------------------
						}



						//추가 21.9.2 <회전 보정>
						//180도 각도 보정을 위해서는 전체 파라미터를 따로 돌아서 벡터 합에 의한 회전각을 따로 계산해야한다.
						if(_cal_isRotation180Correction)
						{
							_cal_Rotation180Correction_DeltaAngle = 0.0f;
							_cal_Rotation180Correction_SumVector = Vector2.zero;

							for (int iPV = 0; iPV < nParamKeys; iPV++)
							{
								paramKeyValue = subParamKeyValueList[iPV];

								if(!paramKeyValue._isCalculated)
								{
									continue;
								}

								tmpSubModMesh_Transform = paramKeyValue._modifiedMeshSet.SubModMesh_Transform;

								float curAngle = tmpSubModMesh_Transform._transformMatrix._angleDeg * Mathf.Deg2Rad;
								_cal_Rotation180Correction_CurVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));
								_cal_Rotation180Correction_SumVector += (_cal_Rotation180Correction_CurVector * paramKeyValue._weight) * 10.0f;//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
							}

							//벡터합을 역산해서 현재 상태의 평균 합을 구하자
							//Weight 합이 0이거나, 서로 반대방향을 바라보는 경우가 아니라면..
							if(_cal_Rotation180Correction_SumVector.sqrMagnitude > 0.001f)
							{
								_cal_Rotation180Correction_SumVector *= 10.0f;
								_cal_Rotation180Correction_DeltaAngle = Mathf.Atan2(_cal_Rotation180Correction_SumVector.y, _cal_Rotation180Correction_SumVector.x) * Mathf.Rad2Deg;
								//Debug.Log("보정 각도 : " + _cal_Rotation180Correction_DeltaAngle + " / 벡터 : " + sumVec);
							}

							tmpMatrix._angleDeg = _cal_Rotation180Correction_DeltaAngle;
						}



						tmpMatrix.CalculateScale_FromAdd();
						tmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit/*, isDebug*/);//추가 (20.9.11) : 위치 보간이슈 수정
					}
					else
					{
						//ModBone을 활용하는 타입인 경우
						for (int iPV = 0; iPV < nParamKeys; iPV++)
						{
							paramKeyValue = subParamKeyValueList[iPV];

							if (!paramKeyValue._isCalculated)
							{
								continue;
							}

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;
							//nAddedWeight++;

							//Weight에 맞게 Matrix를 만들자
							
							if(paramKeyValue._isAnimRotationBias)
							{
								//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								//이전 : apMatrix
								//tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);

								//변경 3.27 : apMatrixCal
								tmpMatrix.AddMatrixParallel_ModBone(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight);
							}
							else
							{
								//이전 : apMatrix
								//tmpMatrix.AddMatrix(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight, false);

								//변경 3.27 : apMatrixCal
								tmpMatrix.AddMatrixParallel_ModBone(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight);
							}

							
							nColorCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}


						//추가 21.9.2 <회전 보정>
						//180도 각도 보정을 위해서는 전체 파라미터를 따로 돌아서 벡터 합에 의한 회전각을 따로 계산해야한다.
						if(_cal_isRotation180Correction)
						{
							_cal_Rotation180Correction_DeltaAngle = 0.0f;
							_cal_Rotation180Correction_SumVector = Vector2.zero;
							for (int iPV = 0; iPV < nParamKeys; iPV++)
							{
								paramKeyValue = subParamKeyValueList[iPV];
								if(!paramKeyValue._isCalculated)
								{
									continue;
								}
								float curAngle = paramKeyValue._modifiedBone._transformMatrix._angleDeg * Mathf.Deg2Rad;
								_cal_Rotation180Correction_CurVector = new Vector2(Mathf.Cos(curAngle), Mathf.Sin(curAngle));
								_cal_Rotation180Correction_SumVector += (_cal_Rotation180Correction_CurVector * paramKeyValue._weight) * 10.0f;//벡터합이 너무 작은 값이면 float 오차로 인하여 각도가 제대로 계산되기 힘들다.
							}

							//벡터합을 역산해서 현재 상태의 평균 합을 구하자
							//Weight 합이 0이거나, 서로 반대방향을 바라보는 경우가 아니라면..
							if(_cal_Rotation180Correction_SumVector.sqrMagnitude > 0.001f)
							{
								_cal_Rotation180Correction_SumVector *= 10.0f;
								_cal_Rotation180Correction_DeltaAngle = Mathf.Atan2(_cal_Rotation180Correction_SumVector.y, _cal_Rotation180Correction_SumVector.x) * Mathf.Rad2Deg;
								//Debug.Log("보정 각도 : " + _cal_Rotation180Correction_DeltaAngle + " / 벡터 : " + sumVec);
							}

							tmpMatrix._angleDeg = _cal_Rotation180Correction_DeltaAngle;
						}



						//위치 변경 20.9.11
						tmpMatrix.CalculateScale_FromAdd();
						
					}

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.

					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));
					}


					

					if (layerWeight < 0.001f)
					{
						continue;
					}

					if ((nColorCalculated == 0 && _isColorProperty) || isBoneTarget)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
						if (!isBoneTarget)
						{
							tmpMatrix.SetIdentity();
							tmpColor = _defaultColor;
						}
					}

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					//추가 3.26 : apMatrixCal 계산 > 이건 ModMesh, ModBone에 따라 달라서 위에서 호출하자. (20.9.10)
					//tmpMatrix.CalculateScale_FromAdd();



					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)
					{
						//위 코드를 하나로 합쳤다.
						//이전 : apMatrix로 계산된 tmpMatrix
						//calParam._result_Matrix.SetTRS(	tmpMatrix._pos * layerWeight,
						//								tmpMatrix._angleDeg * layerWeight,
						//								tmpMatrix._scale * layerWeight + Vector2.one * (1.0f - layerWeight));

						//변경 3.27 : apMatrixCal로 계산된 tmpMatrix
						calParam._result_Matrix.SetTRSForLerp(tmpMatrix);

					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//이전 : apMatrix로 계산
									//calParam._result_Matrix.AddMatrix(tmpMatrix, layerWeight, true);

									//변경 3.27 : apMatrixCal로 계산
									calParam._result_Matrix.AddMatrixLayered(tmpMatrix, layerWeight);
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//이전 : apMatrix로 계산
									//calParam._result_Matrix.LerpMartix(tmpMatrix, layerWeight);

									//변경 3.27 : apMatrixCal로 계산
									calParam._result_Matrix.LerpMatrixLayered(tmpMatrix, layerWeight);
								}
								break;
						}

					}

					
					//변경 : 색상은 별도로 카운팅해서 처리하자
					if (tmpIsColoredKeyParamSetGroup)
					{
						//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.
						if (tmpIsToggleShowHideOption)
						{	
							if (tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (tmpToggleOpt_MaxWeight_Shown > tmpToggleOpt_MaxWeight_Hidden)
								{
									//Show가 더 크다
									tmpVisible = true;
								}
								else if (tmpToggleOpt_MaxWeight_Shown < tmpToggleOpt_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									tmpVisible = false;
									tmpColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (tmpToggleOpt_KeyIndex_Shown > tmpToggleOpt_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										tmpVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										tmpVisible = false;
										tmpColor = Color.clear;
									}
								}
							}
							else if (tmpToggleOpt_IsAnyKey_Shown && !tmpToggleOpt_IsAny_Hidden)
							{
								//Show만 있다면
								tmpVisible = true;
							}
							else if (!tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
							{
								//Hide만 있다면
								tmpVisible = false;
								tmpColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								tmpVisible = false;
								tmpColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (tmpVisible && tmpToggleOpt_TotalWeight_Shown > 0.0f)
							{
								tmpColor.r = Mathf.Clamp01(tmpColor.r / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.g = Mathf.Clamp01(tmpColor.g / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.b = Mathf.Clamp01(tmpColor.b / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.a = Mathf.Clamp01(tmpColor.a / tmpToggleOpt_TotalWeight_Shown);
							}
						}

						if (iColoredKeyParamSetGroup == 0 || keyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
							calParam._result_IsVisible |= tmpVisible;
						}
						iColoredKeyParamSetGroup++;
						calParam._isColorCalculated = true;
					}

					//추가 12.5 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(tmpExtra_DepthChanged)
						{
							calParam._isExtra_DepthChanged = true;
							calParam._extra_DeltaDepth = tmpExtra_DeltaDepth;
						}

						if(tmpExtra_TextureChanged)
						{
							calParam._isExtra_TextureChanged = true;
							calParam._extra_TextureData = tmpExtra_TextureData;
							calParam._extra_TextureDataID = tmpExtra_TextureDataID;
						}
					}

					iCalculatedSubParam++;
				}

				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;

					//이전 : apMatrix로 계산된 경우
					//calParam._result_Matrix.MakeMatrix();

					//변경 : apMatrixCal로 계산한 경우
					calParam._result_Matrix.CalculateScale_FromLerp();
				}
			}
		}





		//추가 20.4.18 : TF의 Animation용 처리 로직을 분리한다.
		//레이어 방식 때문에
		private void Calculate_TF_UseModMeshSet_Animation(float tDelta)
		{
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;

			//Bone을 대상으로 하는가 : Bone대상이면 ModBone을 사용해야한다.
			bool isBoneTarget = false;

			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				if (calParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					isBoneTarget = true;
				}
				else
				{
					//ModMesh를 참조하는 Param이다.
					isBoneTarget = false;
				}
				//1. 계산 [중요]
				//isUpdatable = calParam.Calculate();//이전 : 느림

				//변경 20.11.23 : 애니메이션 모디파이어용 최적화된 Calculate 함수
				isUpdatable = calParam.Calculate_AnimMod();

				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//초기화
				//subParamGroupList = calParam._subParamKeyValueList;//삭제 20.11.25 : 사용되지 않는다.

				

				subParamKeyValueList = null;
				keyParamSetGroup = null;
				_keyAnimClip = null;
				_keyAnimPlayUnit = null;

				calParam._result_Matrix.SetIdentity();

				calParam._isColorCalculated = false;

				if (!isBoneTarget)
				{
					if (_isColorProperty)
					{
						calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						calParam._result_IsVisible = false;
					}
					else
					{
						calParam._result_IsVisible = true;
					}
				}
				else
				{
					calParam._result_IsVisible = true;
					calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}


				//추가 12.5 : Extra Option 초기화
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;



				//초기화
				tmpMatrix.SetIdentity();
				layerWeight = 0.0f;

				//tmpBoneIKWeight = 0.0f;
				tmpVisible = false;

				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;

				////Extra Option 계산 값 초기화
				//tmpExtra_DepthChanged = false;
				//tmpExtra_TextureChanged = false;
				//tmpExtra_DeltaDepth = 0;
				//tmpExtra_TextureDataID = 0;
				//tmpExtra_TextureData = null;
				//tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
				//tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값


				//애니메이션 레이어 변수도 초기화 << 추가 20.4.18
				_curAnimLayeredParam = null;
				_nAnimLayeredParams = 0;
				_curAnimLayer = -1;
				_isAnimAnyExtraCalculated = false;



				//-------------------------------------
				
				//이전 : 모든 SubList를 순회
				//for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				//{
				//	curSubList = subParamGroupList[iSubList];

				//변경 20.11.23 : AnimPlayMapping을 이용하여 미리 정렬된 순서로 SubList를 호출
				int iTargetSubList = 0;
				apAnimPlayMapping.LiveUnit curUnit = null;
				for (int iUnit = 0; iUnit < _animPlayMapping._nAnimClips; iUnit++)
				{
					curUnit = _animPlayMapping._liveUnits_Sorted[iUnit];
					if(!curUnit._isLive)
					{
						//재생 종료
						//이 뒤는 모두 재생이 안되는 애니메이션이다.
						break;
					}

					iTargetSubList = curUnit._animIndex;
					curSubList = calParam._subParamKeyValueList_AnimSync[iTargetSubList];
					if(curSubList == null)
					{
						//이게 Null이라는 것은, 이 AnimClip에 대한 TimelineLayer와 Mod는 없다는 것
						continue;
					}
					
					//----------------------여기까지


					//이전 방식
					//int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					//subParamKeyValueList = curSubList._subParamKeyValues;

					//다른 방식 20.11.24 : 모든 ParamKeyValue가 아니라, 유효한 ParamKeyValue만 체크한다.
					int nValidParamKeyValues = curSubList._nResultAnimKey;
					if(nValidParamKeyValues == 0)
					{
						continue;
					}
					


					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					//삭제 20.11.24 : 위에서 AnimPlayMapping의 LiveUnit에서 미리 결정된다.
					////애니메이션 모디파이어에서 실행되지 않은 애니메이션에 대한 PSG는 생략한다.
					//if (!keyParamSetGroup.IsAnimEnabled)
					//{
					//	//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
					//	continue;
					//}


					//이 로직이 추가된다. (20.4.18)
					_keyAnimClip = keyParamSetGroup._keyAnimClip;
					_keyAnimPlayUnit = _keyAnimClip._parentPlayUnit;

					//레이어별로 데이터를 그룹으로 만들어서 계산하자
					if (_curAnimLayer != _keyAnimPlayUnit._layer
						|| _curAnimLayeredParam == null)
					{
						//다음 레이어를 꺼내자
						//- 풀에 이미 생성된 레이어가 있다면 가져오고, 없으면 생성한다.
						if (_nAnimLayeredParams < _animLayeredParams.Count)
						{
							//가져올 레이어 파라미터가 있다.
							_curAnimLayeredParam = _animLayeredParams[_nAnimLayeredParams];
							_curAnimLayeredParam.ReadyToUpdate();
						}
						else
						{
							//풀에 레이어 파라미터가 없으므로 새로 만들어서 풀에 넣어주자
							_curAnimLayeredParam = AnimLayeredParam.Make_TF(_nAnimLayeredParams);//생성자에 ReadyToUpdate가 포함되어 있다.
							_animLayeredParams.Add(_curAnimLayeredParam);
						}

						_curAnimLayer = _keyAnimPlayUnit._layer;
						_nAnimLayeredParams++;//사용된 레이어 파라미터의 개수
					}




					//레이어 내부의 임시 데이터를 먼저 초기화
					tmpMatrix.SetZero();//<<TF에서 추가됨
					tmpColor = Color.clear;

					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget;


					//추가 20.2.24 : 색상 토글 옵션
					tmpIsToggleShowHideOption = !IsAnimated && tmpIsColoredKeyParamSetGroup && keyParamSetGroup._isToggleShowHideWithoutBlend;

					if (tmpIsToggleShowHideOption)
					{
						tmpToggleOpt_IsAnyKey_Shown = false;
						tmpToggleOpt_TotalWeight_Shown = 0.0f;
						tmpToggleOpt_MaxWeight_Shown = 0.0f;
						tmpToggleOpt_KeyIndex_Shown = 0.0f;
						tmpToggleOpt_IsAny_Hidden = false;
						tmpToggleOpt_TotalWeight_Hidden = 0.0f;
						tmpToggleOpt_MaxWeight_Hidden = 0.0f;
						tmpToggleOpt_KeyIndex_Hidden = 0.0f;
					}


					//변경 20.4.20 : 초기화 위치가 여기여야 한다.
					if(_isExtraPropertyEnabled)
					{
						//추가 12.5 : Extra Option 계산 값				
						tmpExtra_DepthChanged = false;
						tmpExtra_TextureChanged = false;
						tmpExtra_DeltaDepth = 0;
						tmpExtra_TextureDataID = 0;
						tmpExtra_TextureData = null;
						tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
						tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값
					}


					if (!isBoneTarget)
					{
						//ModMesh를 활용하는 타입인 경우 >> ModMeshSet으로 변경

						//추가 20.9.11 : 정밀한 보간을 위해 Default Matrix가 필요하다.
						apMatrix defaultMatrixOfRenderUnit = null;
						if(calParam._targetOptTransform != null)
						{
							defaultMatrixOfRenderUnit = calParam._targetOptTransform._defaultMatrix;
						}


						//변경 20.11.24 모든 키프레임 순회 방식 제거
						//이전 방식
						//for (int iPV = 0; iPV < nParamKeys; iPV++)//이건 또 모든 키프레임을 돌려보겠네..
						//{
						//	paramKeyValue = subParamKeyValueList[iPV];

						//변경 20.11.24 : 전체 체크 > 이미 보간된 것만 체크
						for (int iPV = 0; iPV < nValidParamKeyValues; iPV++)
						{
							paramKeyValue = curSubList._resultAnimKeyPKVs[iPV];//보간이 완료된 PKV만 가져온다.

							//여기까지...


							if (!paramKeyValue._isCalculated) { continue; }

							tmpSubModMesh_Transform = paramKeyValue._modifiedMeshSet.SubModMesh_Transform;

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

							//Weight에 맞게 Matrix를 만들자
							if (paramKeyValue._isAnimRotationBias)
							{
								//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								tmpMatrix.AddMatrixParallel_ModMesh(paramKeyValue.AnimRotationBiasedMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
							}
							else
							{
								//ModMeshSet으로 변경
								tmpMatrix.AddMatrixParallel_ModMesh(tmpSubModMesh_Transform._transformMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
							}


							//색상 처리
							if (tmpIsColoredKeyParamSetGroup)
							{
								tmpSubModMesh_Color = paramKeyValue._modifiedMeshSet.SubModMesh_Color;
								if (tmpSubModMesh_Color != null)
								{
									if (!tmpIsToggleShowHideOption)
									{
										//기본방식
										if (tmpSubModMesh_Color._isVisible)
										{
											tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
											tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
										}
										else
										{
											tmpParamColor = tmpSubModMesh_Color._meshColor;
											tmpParamColor.a = 0.0f;
											tmpColor += tmpParamColor * paramKeyValue._weight;
										}
									}
									else
									{
										//추가 20.2.24 : 토글 방식의 ShowHide 방식
										if (tmpSubModMesh_Color._isVisible && paramKeyValue._weight > 0.0f)
										{
											//paramKeyValue._paramSet.ControlParamValue
											tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
											tmpVisible = true;//< 일단 이것도 true

											//토글용 처리
											tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

											//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
											if (!tmpToggleOpt_IsAnyKey_Shown)
											{
												tmpToggleOpt_KeyIndex_Shown = tmpToggleOpt_KeyIndex_Cal;
											}
											else
											{
												//Show Key Index 중 가장 작은 값을 기준으로 한다.
												tmpToggleOpt_KeyIndex_Shown = (tmpToggleOpt_KeyIndex_Cal < tmpToggleOpt_KeyIndex_Shown ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Shown);
											}


											tmpToggleOpt_IsAnyKey_Shown = true;

											tmpToggleOpt_TotalWeight_Shown += paramKeyValue._weight;
											tmpToggleOpt_MaxWeight_Shown = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Shown ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Shown);

										}
										else
										{
											//토글용 처리
											tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

											if (!tmpToggleOpt_IsAny_Hidden)
											{
												tmpToggleOpt_KeyIndex_Hidden = tmpToggleOpt_KeyIndex_Cal;
											}
											else
											{
												//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
												tmpToggleOpt_KeyIndex_Hidden = (tmpToggleOpt_KeyIndex_Cal > tmpToggleOpt_KeyIndex_Hidden ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Hidden);
											}

											tmpToggleOpt_IsAny_Hidden = true;
											tmpToggleOpt_TotalWeight_Hidden += paramKeyValue._weight;
											tmpToggleOpt_MaxWeight_Hidden = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Hidden ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Hidden);
										}
									}
								}
							}

							nColorCalculated++;


							//---------------------------------------------
							//추가 12.5 : Extra Option
							if (_isExtraPropertyEnabled)
							{
								//변경 : ModMeshSet을 이용하는 것으로 변경
								tmpSubModMesh_Extra = paramKeyValue._modifiedMeshSet.SubModMesh_Extra;

								if (tmpSubModMesh_Extra != null
										&& (tmpSubModMesh_Extra._extraValue._isDepthChanged || tmpSubModMesh_Extra._extraValue._isTextureChanged)
										)
								{
									//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
									float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
									float bias = 0.0001f;
									float overlapBias = 0.01f;
									float cutOut = 0.0f;
									bool isExactWeight = false;
									if (IsAnimated)
									{
										switch (paramKeyValue._animKeyPos)
										{
											case apOptCalculatedResultParam.AnimKeyPos.ExactKey:
												isExactWeight = true;
												break;
											case apOptCalculatedResultParam.AnimKeyPos.NextKey:
												cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimPrev;
												break; //Next Key라면 Prev와의 CutOut을 가져온다.
											case apOptCalculatedResultParam.AnimKeyPos.PrevKey:
												cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimNext;
												break;//Prev Key라면 Next와의 CutOut을 가져온다.
										}
									}
									else
									{
										cutOut = tmpSubModMesh_Extra._extraValue._weightCutout;
									}

									cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

									if (isExactWeight)
									{
										extraWeight = 10000.0f;
									}
									else if (cutOut < bias)
									{
										//정확하면 최대값
										//아니면 적용안함
										if (extraWeight > 1.0f - bias)	{ extraWeight = 10000.0f; }
										else							{ extraWeight = -1.0f; }
									}
									else
									{
										if (extraWeight < 1.0f - cutOut)	{ extraWeight = -1.0f; }
										else								{ extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
									}

									if (extraWeight > 0.0f)
									{
										if (tmpSubModMesh_Extra._extraValue._isDepthChanged)
										{
											//2-1. Depth 이벤트
											if (extraWeight > tmpExtra_DepthMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												tmpExtra_DepthMaxWeight = extraWeight;
												tmpExtra_DepthChanged = true;
												tmpExtra_DeltaDepth = tmpSubModMesh_Extra._extraValue._deltaDepth;
											}

										}
										if (tmpSubModMesh_Extra._extraValue._isTextureChanged)
										{
											//2-2. Texture 이벤트
											if (extraWeight > tmpExtra_TextureMaxWeight)
											{
												//가중치가 최대값보다 큰 경우
												tmpExtra_TextureMaxWeight = extraWeight;
												tmpExtra_TextureChanged = true;
												tmpExtra_TextureData = tmpSubModMesh_Extra._extraValue._linkedTextureData;
												tmpExtra_TextureDataID = tmpSubModMesh_Extra._extraValue._textureDataID;
											}
										}
									}
								}
							}
							//---------------------------------------------
						}

						//위치 변경 20.9.11
						tmpMatrix.CalculateScale_FromAdd();
						tmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit);//추가 (20.9.11) : 위치 보간이슈 수정
					}
					else
					{
						//ModBone을 활용하는 타입인 경우
						//for (int iPV = 0; iPV < nParamKeys; iPV++)
						//{
						//	paramKeyValue = subParamKeyValueList[iPV];

						//변경 20.11.24 : 전체 체크 > 이미 보간된 것만 체크
						for (int iPV = 0; iPV < nValidParamKeyValues; iPV++)
						{
							paramKeyValue = curSubList._resultAnimKeyPKVs[iPV];//보간이 완료된 PKV만 가져온다.

							//여기까지...

							if (!paramKeyValue._isCalculated)
							{
								continue;
							}

							//ParamSetWeight를 추가
							tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

							//Weight에 맞게 Matrix를 만들자

							if (paramKeyValue._isAnimRotationBias)
							{
								//RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
								//변경 3.27 : apMatrixCal
								tmpMatrix.AddMatrixParallel_ModBone(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight);
							}
							else
							{
								//변경 3.27 : apMatrixCal
								tmpMatrix.AddMatrixParallel_ModBone(paramKeyValue._modifiedBone._transformMatrix, paramKeyValue._weight);
							}

							nColorCalculated++;//Visible 계산을 위해 "ParamKey 계산 횟수"를 카운트하자
						}

						//위치 변경 20.9.10
						tmpMatrix.CalculateScale_FromAdd();
					}

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.


					
					if (!_isUseParamSetWeight)
					{
						//layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);//이전
						layerWeight = Mathf.Clamp01(curUnit._playWeight);//변경 20.11.23 : 일일이 계산된 KeyParamSetGroup의 Weight대신, 일괄 계산된 LiveUnit의 값을 이용
					}
					else
					{
						//layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));//이전
						layerWeight = Mathf.Clamp01(curUnit._playWeight * Mathf.Clamp01(tmpTotalParamSetWeight));//변경 20.11.23 : 위와 동일
					}

					if (layerWeight < 0.001f)
					{
						continue;
					}

					if ((nColorCalculated == 0 && _isColorProperty) || isBoneTarget)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
						if (!isBoneTarget)
						{
							tmpMatrix.SetIdentity();
							tmpColor = _defaultColor;
						}
					}

					
					//추가 3.26 : apMatrixCal 계산 > 이건 ModMesh, ModBone에 따라 달라서 위에서 호출하자. (20.9.10)
					//tmpMatrix.CalculateScale_FromAdd();

					//20.4.18 : 애니메이션 로직은 레이어별로 처리를 하므로, 색상 로직을 위로 올려서 여기서 미리 처리한다.
					if (tmpIsColoredKeyParamSetGroup)
					{
						//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.
						if (tmpIsToggleShowHideOption)
						{
							if (tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
							{
								//Show / Hide가 모두 있다면 토글 대상
								if (tmpToggleOpt_MaxWeight_Shown > tmpToggleOpt_MaxWeight_Hidden)
								{
									//Show가 더 크다
									tmpVisible = true;
								}
								else if (tmpToggleOpt_MaxWeight_Shown < tmpToggleOpt_MaxWeight_Hidden)
								{
									//Hidden이 더 크다
									tmpVisible = false;
									tmpColor = Color.clear;
								}
								else
								{
									//같다면? (Weight가 0.5 : 0.5로 같은 경우)
									if (tmpToggleOpt_KeyIndex_Shown > tmpToggleOpt_KeyIndex_Hidden)
									{
										//Show의 ParamSet의 키 인덱스가 더 크다.
										tmpVisible = true;
									}
									else
									{
										//Hidden이 더 크다
										tmpVisible = false;
										tmpColor = Color.clear;
									}
								}
							}
							else if (tmpToggleOpt_IsAnyKey_Shown && !tmpToggleOpt_IsAny_Hidden)
							{
								//Show만 있다면
								tmpVisible = true;
							}
							else if (!tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
							{
								//Hide만 있다면
								tmpVisible = false;
								tmpColor = Color.clear;
							}
							else
							{
								//둘다 없다면? 숨기자.
								tmpVisible = false;
								tmpColor = Color.clear;
							}

							//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
							if (tmpVisible && tmpToggleOpt_TotalWeight_Shown > 0.0f)
							{
								tmpColor.r = Mathf.Clamp01(tmpColor.r / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.g = Mathf.Clamp01(tmpColor.g / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.b = Mathf.Clamp01(tmpColor.b / tmpToggleOpt_TotalWeight_Shown);
								tmpColor.a = Mathf.Clamp01(tmpColor.a / tmpToggleOpt_TotalWeight_Shown);
							}
						}
					}



					//중요! 20.4.18
					//애니메이션은 그룹별로 값을 모아야 한다.
					_curAnimLayeredParam._totalWeight += layerWeight;

					if (_curAnimLayeredParam._iCurAnimClip == 0)
					{
						//첫번째 애니메이션 클립일 때
						//레이어의 블렌드 방식은 첫번째 클립의 값을 따른다.
						if (_keyAnimPlayUnit.BlendMethod == apAnimPlayUnit.BLEND_METHOD.Interpolation)
						{
							_curAnimLayeredParam._blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;
						}
						else
						{
							_curAnimLayeredParam._blendMethod = apModifierParamSetGroup.BLEND_METHOD.Additive;
						}

						_curAnimLayeredParam._isCalculated = true;

						//Matrix를 그대로 할당.
						_curAnimLayeredParam._cal_Matrix.SetTRSForLerp(tmpMatrix);
					}
					else
					{
						//두번째 클립부터는 무조건 Interpolation 방식으로 뒤에 값을 추가해야한다. <중요>
						_curAnimLayeredParam._cal_Matrix.LerpMatrixLayered(tmpMatrix, layerWeight);
					}

					//색상 옵션도 레이어 파라미터에 넣자.
					if (tmpIsColoredKeyParamSetGroup)
					{
						//레이어 내에서는 무조건 Interpolation이다.
						if (_curAnimLayeredParam._nCal_Color == 0)
						{
							//첫번째는 그대로 대입
							_curAnimLayeredParam._cal_Color = tmpColor;
							_curAnimLayeredParam._cal_Visible = tmpVisible;
						}
						else
						{
							//두번째부터는 Interpolation
							_curAnimLayeredParam._cal_Color = apUtil.BlendColor_ITP(
																_curAnimLayeredParam._cal_Color,
																tmpColor,
																layerWeight);

							//Visible은 OR연산
							_curAnimLayeredParam._cal_Visible |= tmpVisible;
						}

						

						_curAnimLayeredParam._isCal_Color = true;
						_curAnimLayeredParam._nCal_Color++;
					}


					//Extra Option > 레이어 파라미터
					if (_isExtraPropertyEnabled)
					{
						if (tmpExtra_DepthChanged)
						{
							_curAnimLayeredParam._isCal_ExtraDepth = true;
							_curAnimLayeredParam._cal_ExtraDepth = tmpExtra_DeltaDepth;

							_isAnimAnyExtraCalculated = true;
						}
						else
						{
							_curAnimLayeredParam._isCal_ExtraDepth = false;
							_curAnimLayeredParam._cal_ExtraDepth = 0;
						}

						if (tmpExtra_TextureChanged)
						{
							_curAnimLayeredParam._isCal_ExtraTexture = true;
							_curAnimLayeredParam._cal_ExtraTexture = tmpExtra_TextureData;
							_curAnimLayeredParam._cal_ExtraTextureID = tmpExtra_TextureDataID;

							_isAnimAnyExtraCalculated = true;
						}
						else
						{
							_curAnimLayeredParam._isCal_ExtraTexture = false;
							_curAnimLayeredParam._cal_ExtraTexture = null;
							_curAnimLayeredParam._cal_ExtraTextureID = -1;
						}
					}

					_curAnimLayeredParam._iCurAnimClip++;
					
					iCalculatedSubParam++;
				}
				
				
				
				//KeyParamSetGroup > 레이어 데이터로 누적 끝



				//저장된 AnimLayeredParam들을 CalPram으로 적용하자

				_iColoredAnimLayeredParam = 0;
				_layeredAnimWeightClamped = 0.0f;

				for (int iAnimLayeredParam = 0; iAnimLayeredParam < _nAnimLayeredParams; iAnimLayeredParam++)
				{
					_curAnimLayeredParam = _animLayeredParams[iAnimLayeredParam];
					if(!_curAnimLayeredParam._isCalculated || _curAnimLayeredParam._totalWeight < 0.0001f)
					{
						//처리 끝
						break;
					}

					_layeredAnimWeightClamped = Mathf.Clamp01(_curAnimLayeredParam._totalWeight);
					_curAnimLayeredParam._cal_Matrix.CalculateScale_FromLerp();//계산 완료된 Matrix 처리

					calParam._totalParamSetGroupWeight += _layeredAnimWeightClamped;


					if (iAnimLayeredParam == 0)
					{
						//Matrix 할당
						calParam._result_Matrix.SetTRSForLerp(_curAnimLayeredParam._cal_Matrix);
					}
					else
					{
						//레이어의 블렌드 방식에 따라 값 적용
						switch (_curAnimLayeredParam._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//변경 3.27 : apMatrixCal로 계산
									calParam._result_Matrix.AddMatrixLayered(
														_curAnimLayeredParam._cal_Matrix, 
														_layeredAnimWeightClamped);
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//변경 3.27 : apMatrixCal로 계산
									calParam._result_Matrix.LerpMatrixLayered(
														_curAnimLayeredParam._cal_Matrix, 
														_layeredAnimWeightClamped);
								}
								break;
						}
					}

					//색상 처리
					if(_curAnimLayeredParam._isCal_Color)
					{
						if(_iColoredAnimLayeredParam == 0
							|| _curAnimLayeredParam._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(	calParam._result_Color, 
																			_curAnimLayeredParam._cal_Color, 
																			_layeredAnimWeightClamped);
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(	calParam._result_Color, 
																			_curAnimLayeredParam._cal_Color, 
																			_layeredAnimWeightClamped);
						}

						calParam._result_IsVisible |= _curAnimLayeredParam._cal_Visible;

						_iColoredAnimLayeredParam++;
						calParam._isColorCalculated = true;
					}
					

					//Extra Option를 CalParam에 전달
					if(_isExtraPropertyEnabled && _isAnimAnyExtraCalculated)
					{
						//활성, 비활성에 상관없이 마지막 레이어의 값이 항상 반영된다.

						if(_curAnimLayeredParam._isCal_ExtraDepth)
						{
							calParam._isExtra_DepthChanged = true;
							calParam._extra_DeltaDepth = _curAnimLayeredParam._cal_ExtraDepth;
						}
						else
						{
							calParam._isExtra_DepthChanged = false;
							calParam._extra_DeltaDepth = 0;
						}


						if(_curAnimLayeredParam._isCal_ExtraTexture)
						{
							calParam._isExtra_TextureChanged = true;
							calParam._extra_TextureData = _curAnimLayeredParam._cal_ExtraTexture;
							calParam._extra_TextureDataID = _curAnimLayeredParam._cal_ExtraTextureID;
						}
						else
						{
							calParam._isExtra_TextureChanged = false;
							calParam._extra_TextureData = null;
							calParam._extra_TextureDataID = -1;
						}
					}
				}


				//레이어 > CalParam 처리 끝

				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;

					//변경 : apMatrixCal로 계산한 경우
					calParam._result_Matrix.CalculateScale_FromLerp();
				}
			}
		}



		//----------------------------------------------------------------------
		// Rigging
		//----------------------------------------------------------------------
		private void Calculate_Rigging_UseModMeshSet(float tDelta)
		{

			if (_calculatedResultParams.Count == 0)
			{
				//Debug.LogError("Result Param Count : 0");
				return;
			}


			apOptCalculatedResultParam calParam = null;

			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요! -> Static은 Weight 계산이 필요없어염
				//-------------------------------------------------------
				//1. Param Weight Calculate
				//calParam.Calculate();//삭제 21.5.22
				//-------------------------------------------------------

				//신버전
				calParamVertRequestList = calParam._result_VertLocalPairs;


				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				
				//초기화
				//신버전
				for (int iVR = 0; iVR < calParamVertRequestList.Count; iVR++)
				{
					calParamVertRequestList[iVR].InitCalculate();
				}
				
				calParam._result_IsVisible = true;

				//삭제 19.5.25 : 필요없는 코드
				//int iCalculatedSubParam = 0;



				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;
					
					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;//<<
					
					//레이어 내부의 임시 데이터를 먼저 초기화
					
					//신버전
					//Vertex Pos 대신 Vertex Requst를 보간하자
					vertRequest = curSubList._vertexRequest;
					vertRequest.SetCalculated();//<<일단 계산하기 위해 참조 했음을 알린다.

					//삭제 19.5.25 : 사용되지 않음
					//tmpColor = Color.clear;
					//tmpVisible = false;

					//float totalWeight = 0.0f;
					//int nCalculated = 0;
					
					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.

					
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						paramKeyValue._weight = 1.0f;

						//totalWeight += paramKeyValue._weight;
						//nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자
					}//--- Params


					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					//이전
					//layerWeight = 1.0f;
					//calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.

					//변경
					calParam._totalParamSetGroupWeight += 1.0f;

					//삭제 19.5.25 : 리깅에선 필요없는 코드
					//if (nCalculated == 0)
					//{
					//	tmpVisible = true;
					//}


					
					//삭제 19.5.25 : 필요없는 코드
					//iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				calParam._isAvailable = true;


			}

			//Rigging을 그 이후의 보간이 없다.
		}



		//----------------------------------------------------------------------
		// Physics
		//----------------------------------------------------------------------
		//초당 얼마나 업데이트 요청을 받는지 체크

		private void Calculate_Physics_UseModMeshSet(float tDelta)
		{
			if (_calculatedResultParams.Count == 0)
			{
				return;
			}

			//삭제 21.7.7
			//if (_stopWatch == null)
			//{
			//	_stopWatch = new System.Diagnostics.Stopwatch();
			//	_stopWatch.Start();
			//}


			//삭제 21.7.7 : 스톱워치는 실행시 오히려 성능이 떨어진다.
			//tDelta = (float)(_stopWatch.ElapsedMilliseconds) / 1000.0f;

			//변경 21.7.7
			tDelta = Time.unscaledDeltaTime;

			//v1.4.2 : Delta 시간이 고정 업데이트 시간을 넘겼다면
			//앱이 백그라운드에서 대기를 했거나 FPS가 크게 떨어진 경우
			//최대 시간으로 고정한다.
			if (tDelta > PHYSIC_DELTA_TIME)
			{
				//물리 시간이 너무 크게 발생
				tDelta = PHYSIC_DELTA_TIME;
			}

			//tDelta *= 0.5f;
			bool isValidFrame = false;
			bool isTeleportFrame = false;//추가 22.7.8 : 위치가 갑자기 이동했다면, 로컬 위치 변화를 새로 계산하지 않고 유지해야한다.
			_tDeltaFixed += tDelta;
			
			//삭제 21.7.7
			//_tUpdateCall += tDelta;
			//_nUpdateCall++;

			
			if (_tDeltaFixed > PHYSIC_DELTA_TIME)
			{
				//Debug.Log("Delta Time : " + tDelta + " >> " + PHYSIC_DELTA_TIME);
				tDelta = PHYSIC_DELTA_TIME;
				_tDeltaFixed -= PHYSIC_DELTA_TIME;

				//만약 제외해도 고정 시간을 넘겼다면, (프레임 시간이 순간 너무 컸을 수 있다.)
				if (_tDeltaFixed > PHYSIC_DELTA_TIME)
				{
					while (_tDeltaFixed > PHYSIC_DELTA_TIME)
					{
						_tDeltaFixed -= PHYSIC_DELTA_TIME;
					}
				}


				isValidFrame = true;
			}
			else
			{
				//현재 프레임은 고정 프레임 업데이트가 아닌 그 사이에 호출된 시간이다.
				tDelta = 0.0f;
				isValidFrame = false;
			}

			//추가 22.7.8 : 텔레포트 문제가 발생하는 경우, 이전의 World 물리 위치를 사용하지 않고,
			//기존의 상대 위치를 이용하자
			if(_portrait._isCurrentTeleporting)
			{
				//isValidFrame = false;//Valid Frame과는 상관없다.
				isTeleportFrame = true;
			}


			apOptCalculatedResultParam calParam = null;

			//지역 변수를 여기서 일괄 선언하자

			//bool isFirstDebug = true;
			//외부 힘을 업데이트해야하는지를 여기서 체크하자
			bool isExtTouchProcessing = false;
			bool isExtTouchWeightRefresh = false;
			if (_portrait.IsAnyTouchEvent)
			{
				isExtTouchProcessing = true;//터치 이벤트 중이다.
				if (_tmpTouchProcessCode != _portrait.TouchProcessCode)
				{
					//처리중인 터치 이벤트가 바뀌었다.
					//새로운 터치라면 Weight를 새로 만들어야하고, 아니면 Weight를 초기화해야함
					_tmpTouchProcessCode = _portrait.TouchProcessCode;
					isExtTouchWeightRefresh = true;

				}
			}
			else
			{
				_tmpTouchProcessCode = 0;
			}



			for (int iCalParam = 0; iCalParam < _calculatedResultParams.Count; iCalParam++)
			{
				calParam = _calculatedResultParams[iCalParam];

				//Sub List를 돌면서 Weight 체크

				// 중요!
				//-------------------------------------------------------
				//1. Param Weight Calculate
				//calParam.Calculate();//삭제 21.5.22 : Static은 필요없다.
				//-------------------------------------------------------

				posList = calParam._result_Positions;
				tmpPosList = calParam._tmp_Positions;
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				layerWeight = 0.0f;
				keyParamSetGroup = null;

				tmpNumVert = posList.Length;//버텍스 위치를 여기서 설정

				//삭제 19.5.20 : 이 변수를 더이상 사용하지 않음
				//weightedVertData = calParam._weightedVertexData;


				calParam._result_IsVisible = true;
				calParam._isAvailable = true;

				//버텍스가 0개라면 처리하지 않는다. [v1.4.2]
				if(tmpNumVert == 0)
				{
					continue;
				}

				//초기화
				Array.Clear(posList, 0, tmpNumVert);

				int iCalculatedSubParam = 0;

				bool isPhysicEnabled = _portrait._isPhysicsPlay_Opt && _portrait._isImportant;
				
#if IS_APDEMO
				isPhysicEnabled = false;//데모 버전에서는 씬에서 물리 기능이 작동하지 않습니다.
#endif

				if(!isPhysicEnabled)
				{
					//물리가 켜지지 않았다면 처리하지 않는다.
					continue;
				}

				int nSubParamGroups = subParamGroupList.Count;
				if(nSubParamGroups == 0)
				{
					continue;
				}

				//SubList (ParamSetGroup을 키값으로 레이어화된 데이터)를 순회하면서 먼저 계산한다.
				//레이어간 병합 과정에 신경 쓸것
				for (int iSubList = 0; iSubList < nSubParamGroups; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					if (curSubList._keyParamSetGroup == null)
					{
						//Debug.LogError("Modifier Cal Param Failed : " + DisplayName + " / " + calParam._linkedModifier.DisplayName);
						continue;
					}

					//int nParamKeys = calParam._paramKeyValues.Count;//전체 Params
					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					//bool isFirstParam = true;//사용하지 않는값

					//레이어 내부의 임시 데이터를 먼저 초기화
					
					//변경 21.5.22 : 배열 초기화 방식 변경
					Array.Clear(tmpPosList, 0, tmpNumVert);

					//float totalWeight = 0.0f;
					//int nCalculated = 0;//사용하지 않는값


					//Param (MorphKey에 따라서)을 기준으로 데이터를 넣어준다.
					//Dist에 따른 ParamWeight를 가중치로 적용한다.
					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						//if (!paramKeyValue._isCalculated) { continue; }

						//삭제 v1.4.2
						//totalWeight += paramKeyValue._weight;

						//물리 계산 순서
						//Vertex 각각의 이전프레임으로 부터의 속력 계산
						
						//ModMeshSet 버전
						tmpSubModMesh_Physics = paramKeyValue._modifiedMeshSet.SubModMesh_Physics;
						tmpPhysicMeshParam = tmpSubModMesh_Physics._physicMeshParam;
						tmpModPhysicsVert = null;

						tmpMass = tmpPhysicMeshParam._mass;
						if (tmpMass < 0.001f)
						{
							tmpMass = 0.001f;
						}
							
						//Vertex에 상관없이 적용되는 힘
						// 중력, 바람
						//1) 중력 : mg
						tmpF_gravity = tmpMass * tmpPhysicMeshParam.GetGravityAcc();

						//2) 바람 : ma
						tmpF_wind = tmpMass * tmpPhysicMeshParam.GetWindAcc(tDelta);

						tmpF_stretch = Vector2.zero;
						//tmpF_airDrag = Vector2.zero;

						//tmpF_inertia = Vector2.zero;
						tmpF_recover = Vector2.zero;
						tmpF_ext = Vector2.zero;
						tmpF_sum = Vector2.zero;

						//tmpLinkedVert = null;//<<이전
						tmpLinkedPhysicVert = null;//<<변경

						tmpIsViscosity = tmpPhysicMeshParam._viscosity > 0.0f;


						//수정
						// "잡아 당기는 코드"를 미리 만들고, Weight를 지정한다.
						//Weight에 따라서 힘의 결과가 속도로 계산되는 비율이 결정된다.
						//Touch Weight가 클수록 Velocity는 0이 된다.


						//Debug.Log("Wind : " + tmpF_wind + " / Gravity : " + tmpF_gravity);
						//---------------------------- Pos List

						//bool isFirstDebug = true;
						//int iDebugLog = -1;
						bool isTouchCalculated = false;
						float touchCalculatedWeight = 0.0f;
						Vector2 touchCalculatedDeltaPos = Vector2.zero;

						for (int iPos = 0; iPos < tmpNumVert; iPos++)
						{
							//여기서 물리 계산을 하자
								
							//ModMeshSet용
							tmpModPhysicsVert = tmpSubModMesh_Physics._vertWeights[iPos];
							tmpModPhysicsVert.UpdatePhysicVertex(tDelta, isValidFrame, isTeleportFrame);//버텍스 위치 연산

							tmpF_stretch = Vector2.zero;

							tmpF_recover = Vector2.zero;
							tmpF_ext = Vector2.zero;
							tmpF_sum = Vector2.zero;

							if (!tmpModPhysicsVert._isEnabled)
							{
								//계산하지 않음
								tmpModPhysicsVert._calculatedDeltaPos = Vector2.zero;
								continue;
							}
							if (tmpModPhysicsVert._vertex == null)
							{
								break;
							}

							//이 버텍스와 연결된 다른 버텍스 개수[v1.4.2]
							int nLinkedVertCount = tmpModPhysicsVert._linkedVertices.Count;

							//최적화는 나중에 하고 일단 업데이트만이라도 하자
							tmpModPhysicsVert._isLimitPos = false;
							tmpModPhysicsVert._limitScale = -1.0f;

							//터치 이벤트 초기화
							isTouchCalculated = false;
							touchCalculatedWeight = 0.0f;
							touchCalculatedDeltaPos = Vector2.zero;

							//"잡아 당김"을 구현하자
							if (isExtTouchProcessing)
							{
								Vector2 pullTouchPos = Vector2.zero;
								float pullTouchTotalWeight = 0.0f;
								//Weight를 새로 갱신하자
								for (int i = 0; i < apForceManager.MAX_TOUCH_UNIT; i++)
								{
									apPullTouch touch = _portrait.GetTouch(i);
									Vector2 touchPos = touch.Position;
									//touchPos *= -1;

									if (isExtTouchWeightRefresh)
									{
										if (touch.IsLive)
										{
											//pos 1F 위치에 의한 Weight를 새로 갱신해야한다.
											tmpModPhysicsVert._touchedWeight[i] = touch.GetTouchedWeight(tmpModPhysicsVert._pos_1F);
											tmpModPhysicsVert._touchedPosDelta[i] = tmpModPhysicsVert._pos_1F - touch.Position;
										}
										else
										{
											tmpModPhysicsVert._touchedWeight[i] = -1.0f;//Weight를 초기화
										}
									}

									if (touch.IsLive)
									{
										//Weight를 이용하여 보간을 하자
										//이후 누적 후 평균값을 넣자
										pullTouchPos += (tmpModPhysicsVert._touchedPosDelta[i] + touch.Position - tmpModPhysicsVert._pos_1F) * tmpModPhysicsVert._touchedWeight[i];
										pullTouchTotalWeight += tmpModPhysicsVert._touchedWeight[i];
									}
								}

								if (pullTouchTotalWeight > 0.0f)
								{
									pullTouchPos /= pullTouchTotalWeight;

									pullTouchPos = paramKeyValue._modifiedMeshSet._targetTransform._rootUnit._transform.InverseTransformVector(pullTouchPos);

									pullTouchPos.x = -pullTouchPos.x;
									pullTouchPos.y = -pullTouchPos.y;

									float itpPull = Mathf.Clamp01(pullTouchTotalWeight);

									touchCalculatedDeltaPos = pullTouchPos;
									isTouchCalculated = true;
									touchCalculatedWeight = itpPull;


								}
							}


							//추가
							//> 유효한 프레임 : 물리 계산을 한다.
							//> 생략하는 프레임 : 이전 속도를 그대로 이용한다.
							if (isValidFrame)
							{
								tmpF_stretch = Vector2.zero;


								//1) 장력 Strech : -k * (<delta Dist> * 기존 UnitVector)
								
								
								if (nLinkedVertCount > 0)
								{
									for (int iLinkVert = 0; iLinkVert < nLinkedVertCount; iLinkVert++)//변경
									{
										tmpLinkedPhysicVert = tmpModPhysicsVert._linkedVertices[iLinkVert];

										float linkWeight = tmpLinkedPhysicVert._distWeight;

										tmpSrcVertPos_NoMod = tmpModPhysicsVert._pos_World_NoMod;
										tmpLinkVertPos_NoMod = tmpLinkedPhysicVert._modVertWeight._pos_World_NoMod;
										tmpLinkedPhysicVert._deltaPosToTarget_NoMod = tmpSrcVertPos_NoMod - tmpLinkVertPos_NoMod;

										tmpSrcVertPos_Cur = tmpModPhysicsVert._pos_World_LocalTransform;
										tmpLinkVertPos_Cur = tmpLinkedPhysicVert._modVertWeight._pos_World_LocalTransform;

										tmpDeltaVec_0 = tmpSrcVertPos_NoMod - tmpLinkVertPos_NoMod;
										tmpDeltaVec_Cur = tmpSrcVertPos_Cur - tmpLinkVertPos_Cur;



										//길이 차이로 힘을 만들고
										//방향은 현재 Delta

										//<추가> 만약 장력 벡터가 완전히 뒤집힌 경우
										//면이 뒤집혔다.
										if (Vector2.Dot(tmpDeltaVec_0, tmpDeltaVec_Cur) < 0)
										{
											//면이 뒤집혔다.
											tmpF_stretch += tmpPhysicMeshParam._stretchK * (tmpDeltaVec_0 - tmpDeltaVec_Cur) * linkWeight;
										}
										else
										{
											//정상 면
											tmpF_stretch += -1.0f * tmpPhysicMeshParam._stretchK * (tmpDeltaVec_Cur.magnitude - tmpDeltaVec_0.magnitude) * tmpDeltaVec_Cur.normalized * linkWeight;
										}
									}
								}
								tmpF_stretch *= -1;//<<위치기반인 경우 좌표계가 반대여서 -1을 넣는다. <<< 이게 왜이리 힘들던지;;


								//3) 공기 저항 : "현재 이동 방향의 반대 방향"
								//..삭제됨


								//5) 복원력
								tmpF_recover = -1.0f * tmpPhysicMeshParam._restoring * tmpModPhysicsVert._calculatedDeltaPos;//변경

								//변동
								//중력과 바람은 크기는 그대로 두고, 방향은 World였다고 가정
								//Local로 오기 위해서는 Inverse를 해야한다.
								float gravitySize = tmpF_gravity.magnitude;
								float windSize = tmpF_wind.magnitude;
								Vector2 tmpF_gravityL = Vector2.zero;
								Vector2 tmpF_windL = Vector2.zero;
								if (gravitySize > 0.0f)
								{
									tmpF_gravityL = paramKeyValue._modifiedMeshSet._targetTransform._rootUnit._transform.InverseTransformVector(tmpF_gravity.normalized).normalized * gravitySize;

									tmpF_gravityL.y = -tmpF_gravityL.y;
									tmpF_gravityL.x = -tmpF_gravityL.x;
								}
								if (windSize > 0.0f)
								{
									tmpF_windL = paramKeyValue._modifiedMeshSet._targetTransform._rootUnit._transform.InverseTransformVector(tmpF_wind.normalized).normalized * windSize;

									tmpF_windL.y = -tmpF_windL.y;
									tmpF_windL.x = -tmpF_windL.x;
								}

								//6) 추가 : 외부 힘
								if (_portrait.IsAnyForceEvent)
								{
									//이전 프레임에서의 힘을 이용한다.
									//해당 위치가 Local이고, 요청된 힘은 World이다.
									//World로 계산한 뒤의 위치를 잡자...는 이미 World였네요.
									//그대로 하고, 힘만 로컬로 바구면 될 듯

									Vector2 F_extW = _portrait.GetForce(tmpModPhysicsVert._pos_1F);//변경

									float powerSize = F_extW.magnitude;

									tmpF_ext = paramKeyValue._modifiedMeshSet._targetTransform._rootUnit._transform.InverseTransformVector(F_extW).normalized * powerSize;

									tmpF_ext.x = -tmpF_ext.x;
									tmpF_ext.y = -tmpF_ext.y;
								}

								float inertiaK = Mathf.Clamp01(tmpPhysicMeshParam._inertiaK);

								//5) 힘의 합력을 구한다.
								//-------------------------------------------
								if (tmpModPhysicsVert._isMain)
								{
									tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + tmpF_recover + tmpF_ext;//관성 제외 (중력, 바람 W2L) - 공기 저항 제외
								}
								else
								{
									tmpF_sum = tmpF_gravityL + tmpF_windL + tmpF_stretch + ((tmpF_recover + tmpF_ext) * 0.5f);//관성 제외 (중력, 바람 W2L) - 공기저항 제외

									inertiaK *= 0.5f;//<<관성 감소
								}

								//-------------------------------------------


								if (isTouchCalculated)
								{
									tmpF_sum *= (1.0f - touchCalculatedWeight);
								}

								//F = ma
								//a = F / m
								//Vector2 acc = F_sum / mass;

								//S = vt + S0
								//-------------------------------


								//<<수정>>
								tmpModPhysicsVert._velocity_Next =
										tmpModPhysicsVert._velocity_1F
										+ (tmpModPhysicsVert._velocity_1F - tmpModPhysicsVert._velocity_Real) * inertiaK
										+ (tmpF_sum / tmpMass) * tDelta
										;


								//Air Drag식 수정
								if (tmpPhysicMeshParam._airDrag > 0.0f)
								{
									tmpModPhysicsVert._velocity_Next *= Mathf.Clamp01((1.0f - (tmpPhysicMeshParam._airDrag * tDelta) / (tmpMass + 0.5f)));
								}
								//-------------------------------
							}
							else
							{
								tmpModPhysicsVert._velocity_Next = tmpModPhysicsVert._velocity_1F;//변경
							}



							//여기서 일단 속력을 미리 적용하자
							if (isValidFrame)
							{
								tmpNextVelocity = tmpModPhysicsVert._velocity_Next;

								Vector2 limitedNextCalPos = tmpModPhysicsVert._calculatedDeltaPos + (tmpNextVelocity * tDelta);//변경

								//터치 이벤트에 의해서 속도가 보간된다.
								if (isTouchCalculated)
								{
									limitedNextCalPos = (limitedNextCalPos * (1.0f - touchCalculatedWeight)) + (touchCalculatedDeltaPos * touchCalculatedWeight);

									tmpNextVelocity = (limitedNextCalPos - tmpModPhysicsVert._calculatedDeltaPos) / tDelta;//변경
								}

								//V += at
								//마음대로 증가하지 않도록 한다.
								if (tmpPhysicMeshParam._isRestrictMoveRange)
								{
									float radiusFree = tmpPhysicMeshParam._moveRange * 0.5f;
									float radiusMax = tmpPhysicMeshParam._moveRange;

									if (radiusMax <= radiusFree)
									{
										tmpNextVelocity *= 0.0f;
										//둘다 0이라면 아예 이동이 불가
										if (!tmpModPhysicsVert._isLimitPos)
										{
											tmpModPhysicsVert._isLimitPos = true;
											tmpModPhysicsVert._limitScale = 0.0f;
										}
									}
									else
									{
										float curDeltaPosSize = (limitedNextCalPos).magnitude;

										if (curDeltaPosSize < radiusFree)
										{
											//moveRatio = 1.0f;
											//별일 없슴다
										}
										else
										{
											//기본은 선형의 사이즈이지만,
											//돌아가는 힘은 유지해야한다.
											//[deltaPos unitVector dot newVelocity] = 1일때 : 바깥으로 나가려는 힘
											// = -1일때 : 안으로 들어오려는 힘
											// -1 ~ 1 => 0 ~ 1 : 0이면 moveRatio가 1, 1이면 moveRatio가 거리에 따라 1>0

											float dotVector = Vector2.Dot(tmpModPhysicsVert._calculatedDeltaPos.normalized, tmpNextVelocity.normalized);

											dotVector = (dotVector * 0.5f) + 0.5f; //0: 속도 느려짐 없음 (안쪽으로 들어가려고 함), 1:증가하는 방향

											float outerItp = Mathf.Clamp01((curDeltaPosSize - radiusFree) / (radiusMax - radiusFree));//0 : 속도 느려짐 없음, 1:속도 0

											tmpNextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자

											if (curDeltaPosSize > radiusMax)
											{
												if (!tmpModPhysicsVert._isLimitPos || radiusMax < tmpModPhysicsVert._limitScale)
												{
													tmpModPhysicsVert._isLimitPos = true;
													tmpModPhysicsVert._limitScale = radiusMax;
												}
											}
										}
									}
								}

								//장력에 의한 길이 제한도 처리한다.
								if (tmpPhysicMeshParam._isRestrictStretchRange && nLinkedVertCount > 0)
								{
									bool isLimitVelocity2Max = false;
									Vector2 stretchLimitPos = Vector2.zero;
									float limitCalPosDist = 0.0f;

									for (int iLinkVert = 0; iLinkVert < nLinkedVertCount; iLinkVert++)
									{
										tmpLinkedPhysicVert = tmpModPhysicsVert._linkedVertices[iLinkVert];

										//길이의 Min/Max가 있다.
										float distStretchBase = tmpLinkedPhysicVert._deltaPosToTarget_NoMod.magnitude;

										float stretchRangeMax = (tmpPhysicMeshParam._stretchRangeRatio_Max) * distStretchBase;
										float stretchRangeMax_Half = (tmpPhysicMeshParam._stretchRangeRatio_Max * 0.5f) * distStretchBase;

										Vector2 curDeltaFromLinkVert = limitedNextCalPos - tmpLinkedPhysicVert._modVertWeight._calculatedDeltaPos_Prev;

										float curDistFromLinkVert = curDeltaFromLinkVert.magnitude;

										//너무 멀면 제한한다.
										//단, 제한 권장은 Weight에 맞게

										isLimitVelocity2Max = false;

										if (curDistFromLinkVert > stretchRangeMax_Half)
										{
											isLimitVelocity2Max = true;//늘어나는 한계점으로 이동하는 중

											stretchLimitPos = tmpLinkedPhysicVert._modVertWeight._calculatedDeltaPos_Prev + curDeltaFromLinkVert.normalized * stretchRangeMax;
											stretchLimitPos -= tmpModPhysicsVert._calculatedDeltaPos_Prev;


											limitCalPosDist = (stretchLimitPos).magnitude;
										}

										if (isLimitVelocity2Max)
										{
											//LinkVert간의 벡터를 기준으로 nextVelocity가 확대/축소하는 방향이라면 그 반대의 값을 넣는다.
											float dotVector = Vector2.Dot(curDeltaFromLinkVert.normalized, tmpNextVelocity.normalized);
											//-1 : 축소하려는 방향으로 이동하는 중
											//1 : 확대하려는 방향으로 이동하는 중


											float outerItp = 0.0f;
											if (isLimitVelocity2Max)
											{
												//너무 바깥으로 이동하려고 할때, 속도를 줄인다.
												dotVector = Mathf.Clamp01(dotVector);
												if (stretchRangeMax > stretchRangeMax_Half)
												{
													outerItp = Mathf.Clamp01((curDistFromLinkVert - stretchRangeMax_Half) / (stretchRangeMax - stretchRangeMax_Half));
												}
												else
												{
													outerItp = 1.0f;//무조건 속도 0

													if (!tmpModPhysicsVert._isLimitPos || limitCalPosDist < tmpModPhysicsVert._limitScale)
													{
														tmpModPhysicsVert._isLimitPos = true;
														tmpModPhysicsVert._limitScale = limitCalPosDist;
													}
												}

											}

											tmpNextVelocity *= Mathf.Clamp01(1.0f - (dotVector * outerItp));//적절히 느려지게 만들자
										}
									}

								}

								limitedNextCalPos = tmpModPhysicsVert._calculatedDeltaPos + (tmpNextVelocity * tDelta);


								//이걸 한번더 해서 위치 보정
								if (isTouchCalculated)
								{
									Vector2 nextTouchPos = (limitedNextCalPos * (1.0f - touchCalculatedWeight)) + (touchCalculatedDeltaPos * touchCalculatedWeight);

									limitedNextCalPos = nextTouchPos;

									tmpNextVelocity = (limitedNextCalPos - tmpModPhysicsVert._calculatedDeltaPos) / tDelta;//변경
								}
								
								tmpModPhysicsVert._velocity_Next = tmpNextVelocity;
								tmpModPhysicsVert._calculatedDeltaPos_Prev = tmpModPhysicsVert._calculatedDeltaPos;
								tmpModPhysicsVert._calculatedDeltaPos = limitedNextCalPos;
							}
							else
							{
								tmpModPhysicsVert._calculatedDeltaPos_Prev = tmpModPhysicsVert._calculatedDeltaPos;
								tmpNextVelocity = tmpModPhysicsVert._velocity_Next;
								tmpModPhysicsVert._calculatedDeltaPos = tmpModPhysicsVert._calculatedDeltaPos + (tmpNextVelocity * tDelta);
							}
						}

						//1차로 계산된 값을 이용하여 점성력을 체크한다.
						//수정 : 이미 위치는 계산되었다. 위치를 중심으로 처리를 하자 점성/이동한계를 계산하자
						for (int iPos = 0; iPos < tmpNumVert; iPos++)
						{
							tmpModPhysicsVert = tmpSubModMesh_Physics._vertWeights[iPos];

							if (!tmpModPhysicsVert._isEnabled)
							{
								//처리 안함다
								tmpModPhysicsVert._calculatedDeltaPos = Vector2.zero;
								continue;
							}
							if (tmpModPhysicsVert._vertex == null)
							{
								Debug.LogError("Render Vertex is Not linked");
								break;
							}

							int nLinkedVertCount = tmpModPhysicsVert._linkedVertices.Count;

							if (isValidFrame)
							{
								tmpNextVelocity = tmpModPhysicsVert._velocity_Next;
								tmpNextCalPos = tmpModPhysicsVert._calculatedDeltaPos;

								if (tmpIsViscosity && !tmpModPhysicsVert._isMain)//변경
								{
									//점성 로직 추가
									//ID가 같으면 DeltaPos가 비슷해야한다.
									tmpLinkedViscosityWeight = 0.0f;
									tmpLinkedTotalCalPos = Vector2.zero;

									int curViscosityID = tmpModPhysicsVert._viscosityGroupID;//변경

									if (nLinkedVertCount > 0)
									{
										for (int iLinkVert = 0; iLinkVert < nLinkedVertCount; iLinkVert++)//변경
										{
											tmpLinkedPhysicVert = tmpModPhysicsVert._linkedVertices[iLinkVert];
											float linkWeight = tmpLinkedPhysicVert._distWeight;

											if ((tmpLinkedPhysicVert._modVertWeight._viscosityGroupID & curViscosityID) != 0)
											{
												tmpLinkedTotalCalPos += tmpLinkedPhysicVert._modVertWeight._calculatedDeltaPos * linkWeight;//<<Vel 대신 Pos로 바꾸자
												tmpLinkedViscosityWeight += linkWeight;
											}
										}
									}

									//점성도를 추가한다.
									if (tmpLinkedViscosityWeight > 0.0f)
									{
										float clampViscosity = Mathf.Clamp01(tmpPhysicMeshParam._viscosity) * 0.7f;


										tmpNextCalPos = tmpNextCalPos * (1.0f - clampViscosity) + tmpLinkedTotalCalPos * clampViscosity;
									}

								}

								//이동 한계 한번 더 계산
								if (tmpModPhysicsVert._isLimitPos && tmpNextCalPos.magnitude > tmpModPhysicsVert._limitScale)
								{
									tmpNextCalPos = tmpNextCalPos.normalized * tmpModPhysicsVert._limitScale;
								}

								//계산 끝!
								//새로운 변위를 넣어주자
								tmpModPhysicsVert._calculatedDeltaPos = tmpNextCalPos;

								//속도를 다시 계산해주자
								tmpNextVelocity = (tmpModPhysicsVert._calculatedDeltaPos - tmpModPhysicsVert._calculatedDeltaPos_Prev) / tDelta;


								//-----------------------------------------------------------------------------------------
								//속도 갱신
								tmpModPhysicsVert._velocity_Next = tmpNextVelocity;


								//속도 차이가 크다면 Real의 비중이 커야 한다.
								//같은 방향이면 -> 버티기 관성이 더 잘보이는게 좋다
								//다른 방향이면 Real을 관성으로 사용해야한다. (그래야 다음 프레임에 관성이 크게 보임)
								//속도 변화에 따라서 체크
								float velocityRefreshITP_X = Mathf.Clamp01(Mathf.Abs(((tmpModPhysicsVert._velocity_Real.x - tmpModPhysicsVert._velocity_Real1F.x) / (Mathf.Abs(tmpModPhysicsVert._velocity_Real1F.x) + 0.1f)) * 0.5f));
								float velocityRefreshITP_Y = Mathf.Clamp01(Mathf.Abs(((tmpModPhysicsVert._velocity_Real.y - tmpModPhysicsVert._velocity_Real1F.y) / (Mathf.Abs(tmpModPhysicsVert._velocity_Real1F.y) + 0.1f)) * 0.5f));

								tmpModPhysicsVert._velocity_1F.x = tmpNextVelocity.x * (1.0f - velocityRefreshITP_X) + (tmpNextVelocity.x * 0.5f + tmpModPhysicsVert._velocity_Real.x * 0.5f) * velocityRefreshITP_X;
								tmpModPhysicsVert._velocity_1F.y = tmpNextVelocity.y * (1.0f - velocityRefreshITP_Y) + (tmpNextVelocity.y * 0.5f + tmpModPhysicsVert._velocity_Real.y * 0.5f) * velocityRefreshITP_Y;

								tmpModPhysicsVert._pos_1F = tmpModPhysicsVert._pos_Real;

								//-----------------------------------------------------------------------------------------


								//Damping
								if (tmpModPhysicsVert._calculatedDeltaPos.sqrMagnitude < tmpPhysicMeshParam._damping * tmpPhysicMeshParam._damping
									&& tmpNextVelocity.sqrMagnitude < tmpPhysicMeshParam._damping * tmpPhysicMeshParam._damping)
								{
									tmpModPhysicsVert._calculatedDeltaPos = Vector2.zero;
									tmpModPhysicsVert.DampPhysicVertex();
								}
							}

							//이전
							tmpPosList[iPos] +=
										(tmpModPhysicsVert._calculatedDeltaPos * tmpModPhysicsVert._weight)
										* paramKeyValue._weight;
						}
						//---------------------------- Pos List
						//if (isFirstParam)
						//{
						//	isFirstParam = false;
						//}

						//삭제 v1.4.2 : 필요없당
						//nCalculated++;//Visible 계산을 위해 "paramKey 계산 횟수"를 카운트하자

					}//--- Params



					//이제 tmp값을 Result에 넘겨주자
					//처음 Layer라면 -> 100% 적용
					//그렇지 않다면 Blend를 해주자

					layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);


					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.


					//if (keyParamSetGroup._layerIndex == 0)
					if (iCalculatedSubParam == 0)//<<변경
					{
						for (int iPos = 0; iPos < tmpNumVert; iPos++)
						{
							posList[iPos] = tmpPosList[iPos] * layerWeight;
						}
					}
					else
					{
						switch (keyParamSetGroup._blendMethod)
						{
							case apModifierParamSetGroup.BLEND_METHOD.Additive:
								{
									//변경됨 19.5.20
									for (int iPos = 0; iPos < tmpNumVert; iPos++)
									{
										posList[iPos] += tmpPosList[iPos] * layerWeight;
									}
								}
								break;

							case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
								{
									//변경됨 19.5.20
									for (int iPos = 0; iPos < tmpNumVert; iPos++)
									{
										posList[iPos] = (posList[iPos] * (1.0f - layerWeight)) +
														(tmpPosList[iPos] * layerWeight);
									}
								}
								break;

							default:
								Debug.LogError("Mod-Physics : Unknown BLEND_METHOD : " + keyParamSetGroup._blendMethod);
								break;
						}
					}

					iCalculatedSubParam++;

				}//-SubList (ParamSetGroup을 키값으로 따로 적용한다.)
				


			}
		}




		//--------------------------------------------------------------------------
		// Color Only (추가 21.7.20)
		//--------------------------------------------------------------------------
		private void Calculate_ColorOnly_UseModMeshSet(float tDelta)
		{
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;

			
			//추가 : Bone을 대상으로 하는가
			//Bone대상이면 ModBone을 사용해야한다.
			//bool isBoneTarget = false;//>TF
			
			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				if (calParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					//> 대상이 아니다.
					calParam._isAvailable = false;
					continue;
				}
				

				//1. 계산 [중요]
				isUpdatable = calParam.Calculate();

				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				//초기화
				subParamGroupList = calParam._subParamKeyValueList;
				subParamKeyValueList = null;
				keyParamSetGroup = null;

				//calParam._result_Matrix.SetIdentity();//TF

				calParam._isColorCalculated = false;

				//>Color
				calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				calParam._result_IsVisible = false;


				//추가 12.5 : Extra Option 초기화
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;



				//초기화
				//tmpMatrix.SetIdentity();//> TF
				layerWeight = 0.0f;

				//tmpBoneIKWeight = 0.0f;
				tmpVisible = false;

				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;


				for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				{
					curSubList = subParamGroupList[iSubList];

					int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					subParamKeyValueList = curSubList._subParamKeyValues;

					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					//>>이 검사는 필요없다. 애니메이션 로직이 분리되었기 때문
					////추가 20.4.2 : 애니메이션 모디파이어일때.
					//if(_isAnimated && !keyParamSetGroup.IsAnimEnabled)
					//{	
					//	//실행되지 않은 애니메이션은 연산을 하지 않는다. > 중요 최적화!
					//	//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
					//	continue;
					//}
					

					//레이어 내부의 임시 데이터를 먼저 초기화
					//tmpMatrix.SetZero();//> TF
					tmpColor = Color.clear;

					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					//tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget;//>TF
					tmpIsColoredKeyParamSetGroup = true;//>Color (이 조건문이 의미없다.)


					//추가 20.2.24 : 색상 토글 옵션
					//tmpIsToggleShowHideOption = !IsAnimated && tmpIsColoredKeyParamSetGroup && keyParamSetGroup._isToggleShowHideWithoutBlend;
					tmpIsToggleShowHideOption = keyParamSetGroup._isToggleShowHideWithoutBlend;//앞의 옵션은 필요없다.
					
					if(tmpIsToggleShowHideOption)
					{
						tmpToggleOpt_IsAnyKey_Shown = false;
						tmpToggleOpt_TotalWeight_Shown = 0.0f;
						tmpToggleOpt_MaxWeight_Shown = 0.0f;
						tmpToggleOpt_KeyIndex_Shown = 0.0f;
						tmpToggleOpt_IsAny_Hidden = false;
						tmpToggleOpt_TotalWeight_Hidden = 0.0f;
						tmpToggleOpt_MaxWeight_Hidden = 0.0f;
						tmpToggleOpt_KeyIndex_Hidden = 0.0f;
					}


					//변경 20.4.20 : 초기화 위치가 여기여야 한다.
					if(_isExtraPropertyEnabled)
					{
						//추가 12.5 : Extra Option 계산 값				
						tmpExtra_DepthChanged = false;
						tmpExtra_TextureChanged = false;
						tmpExtra_DeltaDepth = 0;
						tmpExtra_TextureDataID = 0;
						tmpExtra_TextureData = null;
						tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
						tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값
					}


					
					//ModMesh를 활용하는 타입인 경우 >> ModMeshSet으로 변경
					
					//>TF
					//apMatrix defaultMatrixOfRenderUnit = null;					
					//if(calParam._targetOptTransform != null)
					//{
					//	defaultMatrixOfRenderUnit = calParam._targetOptTransform._defaultMatrix;
					//}



					for (int iPV = 0; iPV < nParamKeys; iPV++)
					{
						paramKeyValue = subParamKeyValueList[iPV];

						if (!paramKeyValue._isCalculated)
						{ continue; }

						tmpSubModMesh_Transform = paramKeyValue._modifiedMeshSet.SubModMesh_Transform;

						//ParamSetWeight를 추가
						tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

						//> TF
						//Weight에 맞게 Matrix를 만들자
						//if(paramKeyValue._isAnimRotationBias)
						//{
						//	//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
						//	//이전 : apMatrix
						//	//tmpMatrix.AddMatrix(paramKeyValue.AnimRotationBiasedMatrix, paramKeyValue._weight, false);

						//	//변경 3.27 : apMatrixCal
						//	tmpMatrix.AddMatrixParallel_ModMesh(paramKeyValue.AnimRotationBiasedMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
						//}
						//else
						//{
						//	//이전
						//	//tmpMatrix.AddMatrixParallel(paramKeyValue._modifiedMesh._transformMatrix, paramKeyValue._weight);

						//	//ModMeshSet으로 변경
						//	tmpMatrix.AddMatrixParallel_ModMesh(tmpSubModMesh_Transform._transformMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight/*, isDebug*/);
						//}


						//색상 처리
						if (tmpIsColoredKeyParamSetGroup)
						{
							tmpSubModMesh_Color = paramKeyValue._modifiedMeshSet.SubModMesh_Color;
							if (tmpSubModMesh_Color != null)
							{
								if (!tmpIsToggleShowHideOption)
								{
									//기본방식
									if (tmpSubModMesh_Color._isVisible)
									{
										tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
										tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
									}
									else
									{
										tmpParamColor = tmpSubModMesh_Color._meshColor;
										tmpParamColor.a = 0.0f;
										tmpColor += tmpParamColor * paramKeyValue._weight;
									}
								}
								else
								{
									//추가 20.2.24 : 토글 방식의 ShowHide 방식
									if (tmpSubModMesh_Color._isVisible && paramKeyValue._weight > 0.0f)
									{
										//paramKeyValue._paramSet.ControlParamValue
										tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
										tmpVisible = true;//< 일단 이것도 true

										//토글용 처리
										tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

										//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
										if (!tmpToggleOpt_IsAnyKey_Shown)
										{
											tmpToggleOpt_KeyIndex_Shown = tmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Show Key Index 중 가장 작은 값을 기준으로 한다.
											tmpToggleOpt_KeyIndex_Shown = (tmpToggleOpt_KeyIndex_Cal < tmpToggleOpt_KeyIndex_Shown ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Shown);
										}


										tmpToggleOpt_IsAnyKey_Shown = true;

										tmpToggleOpt_TotalWeight_Shown += paramKeyValue._weight;
										tmpToggleOpt_MaxWeight_Shown = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Shown ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Shown);

									}
									else
									{
										//토글용 처리
										tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

										if (!tmpToggleOpt_IsAny_Hidden)
										{
											tmpToggleOpt_KeyIndex_Hidden = tmpToggleOpt_KeyIndex_Cal;
										}
										else
										{
											//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
											tmpToggleOpt_KeyIndex_Hidden = (tmpToggleOpt_KeyIndex_Cal > tmpToggleOpt_KeyIndex_Hidden ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Hidden);
										}

										tmpToggleOpt_IsAny_Hidden = true;
										tmpToggleOpt_TotalWeight_Hidden += paramKeyValue._weight;
										tmpToggleOpt_MaxWeight_Hidden = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Hidden ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Hidden);
									}
								}
							}
						}

						nColorCalculated++;


						//---------------------------------------------
						//추가 12.5 : Extra Option
						if (_isExtraPropertyEnabled)
						{
							//변경 : ModMeshSet을 이용하는 것으로 변경
							tmpSubModMesh_Extra = paramKeyValue._modifiedMeshSet.SubModMesh_Extra;

							if (tmpSubModMesh_Extra != null
									&& (tmpSubModMesh_Extra._extraValue._isDepthChanged || tmpSubModMesh_Extra._extraValue._isTextureChanged)
									)
							{
								//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
								float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
								float bias = 0.0001f;
								float overlapBias = 0.01f;
								float cutOut = 0.0f;
								bool isExactWeight = false;
								if (IsAnimated)
								{
									switch (paramKeyValue._animKeyPos)
									{
										case apOptCalculatedResultParam.AnimKeyPos.ExactKey:
											isExactWeight = true;
											break;
										case apOptCalculatedResultParam.AnimKeyPos.NextKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimPrev;
											break; //Next Key라면 Prev와의 CutOut을 가져온다.
										case apOptCalculatedResultParam.AnimKeyPos.PrevKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimNext;
											break;//Prev Key라면 Next와의 CutOut을 가져온다.
									}
								}
								else
								{
									cutOut = tmpSubModMesh_Extra._extraValue._weightCutout;
								}

								cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

								if (isExactWeight)
								{
									extraWeight = 10000.0f;
								}
								else if (cutOut < bias)
								{
									//정확하면 최대값
									//아니면 적용안함
									if (extraWeight > 1.0f - bias)	{ extraWeight = 10000.0f; }
									else							{ extraWeight = -1.0f; }
								}
								else
								{
									if (extraWeight < 1.0f - cutOut)	{ extraWeight = -1.0f; }
									else								{ extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
								}

								if (extraWeight > 0.0f)
								{
									if (tmpSubModMesh_Extra._extraValue._isDepthChanged)
									{
										//2-1. Depth 이벤트
										if (extraWeight > tmpExtra_DepthMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_DepthMaxWeight = extraWeight;
											tmpExtra_DepthChanged = true;
											tmpExtra_DeltaDepth = tmpSubModMesh_Extra._extraValue._deltaDepth;
										}

									}
									if (tmpSubModMesh_Extra._extraValue._isTextureChanged)
									{
										//2-2. Texture 이벤트
										if (extraWeight > tmpExtra_TextureMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_TextureMaxWeight = extraWeight;
											tmpExtra_TextureChanged = true;
											tmpExtra_TextureData = tmpSubModMesh_Extra._extraValue._linkedTextureData;
											tmpExtra_TextureDataID = tmpSubModMesh_Extra._extraValue._textureDataID;
										}
									}
								}
							}
						}
						//---------------------------------------------
					}

					//>TF
					//tmpMatrix.CalculateScale_FromAdd();
					//tmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit/*, isDebug*/);//추가 (20.9.11) : 위치 보간이슈 수정

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.
					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight);
					}
					else
					{
						layerWeight = Mathf.Clamp01(keyParamSetGroup._layerWeight * Mathf.Clamp01(tmpTotalParamSetWeight));
					}

					if (layerWeight < 0.001f)
					{
						continue;
					}

					//if ((nColorCalculated == 0 && _isColorProperty) || isBoneTarget)
					if (nColorCalculated == 0)
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
						
						//TF
						//if (!isBoneTarget)
						//{
						//	tmpMatrix.SetIdentity();
						//	tmpColor = _defaultColor;
						//}
					}

					calParam._totalParamSetGroupWeight += layerWeight;//<<수정 : 나중에 Modifier 자체의 Weight를 적용할 수 있게 만든다.


					#region [미사용 코드] TF
					//if (iCalculatedSubParam == 0)
					//{
					//	//위 코드를 하나로 합쳤다.
					//	//이전 : apMatrix로 계산된 tmpMatrix
					//	//calParam._result_Matrix.SetTRS(	tmpMatrix._pos * layerWeight,
					//	//								tmpMatrix._angleDeg * layerWeight,
					//	//								tmpMatrix._scale * layerWeight + Vector2.one * (1.0f - layerWeight));

					//	//변경 3.27 : apMatrixCal로 계산된 tmpMatrix
					//	calParam._result_Matrix.SetTRSForLerp(tmpMatrix);

					//}
					//else
					//{
					//	switch (keyParamSetGroup._blendMethod)
					//	{
					//		case apModifierParamSetGroup.BLEND_METHOD.Additive:
					//			{
					//				//이전 : apMatrix로 계산
					//				//calParam._result_Matrix.AddMatrix(tmpMatrix, layerWeight, true);

					//				//변경 3.27 : apMatrixCal로 계산
					//				calParam._result_Matrix.AddMatrixLayered(tmpMatrix, layerWeight);
					//			}
					//			break;

					//		case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
					//			{
					//				//이전 : apMatrix로 계산
					//				//calParam._result_Matrix.LerpMartix(tmpMatrix, layerWeight);

					//				//변경 3.27 : apMatrixCal로 계산
					//				calParam._result_Matrix.LerpMatrixLayered(tmpMatrix, layerWeight);
					//			}
					//			break;
					//	}

					//} 
					#endregion


					//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.
					if (tmpIsToggleShowHideOption)
					{	
						if (tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
						{
							//Show / Hide가 모두 있다면 토글 대상
							if (tmpToggleOpt_MaxWeight_Shown > tmpToggleOpt_MaxWeight_Hidden)
							{
								//Show가 더 크다
								tmpVisible = true;
							}
							else if (tmpToggleOpt_MaxWeight_Shown < tmpToggleOpt_MaxWeight_Hidden)
							{
								//Hidden이 더 크다
								tmpVisible = false;
								tmpColor = Color.clear;
							}
							else
							{
								//같다면? (Weight가 0.5 : 0.5로 같은 경우)
								if (tmpToggleOpt_KeyIndex_Shown > tmpToggleOpt_KeyIndex_Hidden)
								{
									//Show의 ParamSet의 키 인덱스가 더 크다.
									tmpVisible = true;
								}
								else
								{
									//Hidden이 더 크다
									tmpVisible = false;
									tmpColor = Color.clear;
								}
							}
						}
						else if (tmpToggleOpt_IsAnyKey_Shown && !tmpToggleOpt_IsAny_Hidden)
						{
							//Show만 있다면
							tmpVisible = true;
						}
						else if (!tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
						{
							//Hide만 있다면
							tmpVisible = false;
							tmpColor = Color.clear;
						}
						else
						{
							//둘다 없다면? 숨기자.
							tmpVisible = false;
							tmpColor = Color.clear;
						}

						//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
						if (tmpVisible && tmpToggleOpt_TotalWeight_Shown > 0.0f)
						{
							tmpColor.r = Mathf.Clamp01(tmpColor.r / tmpToggleOpt_TotalWeight_Shown);
							tmpColor.g = Mathf.Clamp01(tmpColor.g / tmpToggleOpt_TotalWeight_Shown);
							tmpColor.b = Mathf.Clamp01(tmpColor.b / tmpToggleOpt_TotalWeight_Shown);
							tmpColor.a = Mathf.Clamp01(tmpColor.a / tmpToggleOpt_TotalWeight_Shown);
						}
					}

					if (iColoredKeyParamSetGroup == 0 || keyParamSetGroup._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
					{
						//색상 Interpolation
						calParam._result_Color = apUtil.BlendColor_ITP(calParam._result_Color, tmpColor, layerWeight);
						calParam._result_IsVisible |= tmpVisible;
					}
					else
					{
						//색상 Additive
						calParam._result_Color = apUtil.BlendColor_Add(calParam._result_Color, tmpColor, layerWeight);
						calParam._result_IsVisible |= tmpVisible;
					}
					iColoredKeyParamSetGroup++;
					calParam._isColorCalculated = true;


					//추가 12.5 : Extra Option
					if(_isExtraPropertyEnabled)
					{
						if(tmpExtra_DepthChanged)
						{
							calParam._isExtra_DepthChanged = true;
							calParam._extra_DeltaDepth = tmpExtra_DeltaDepth;
						}

						if(tmpExtra_TextureChanged)
						{
							calParam._isExtra_TextureChanged = true;
							calParam._extra_TextureData = tmpExtra_TextureData;
							calParam._extra_TextureDataID = tmpExtra_TextureDataID;
						}
					}

					iCalculatedSubParam++;
				}

				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;

					//>TF
					////변경 : apMatrixCal로 계산한 경우
					//calParam._result_Matrix.CalculateScale_FromLerp();
				}
			}
		}





		//추가 20.4.18 : TF의 Animation용 처리 로직을 분리한다.
		//레이어 방식 때문에
		private void Calculate_ColorOnly_UseModMeshSet_Animation(float tDelta)
		{
			apOptCalculatedResultParam calParam = null;
			bool isUpdatable = false;

			//Bone을 대상으로 하는가 : Bone대상이면 ModBone을 사용해야한다.
			//bool isBoneTarget = false;//>TF

			for (int i = 0; i < _calculatedResultParams.Count; i++)
			{
				calParam = _calculatedResultParams[i];

				if (calParam._targetBone != null)
				{
					//ModBone을 참조하는 Param이다.
					//> 대상이 아니다.
					calParam._isAvailable = false;
					continue;
				}
				

				//1. 계산 [중요]
				//isUpdatable = calParam.Calculate();//이전 : 느림

				//변경 20.11.23 : 애니메이션 모디파이어용 최적화된 Calculate 함수
				isUpdatable = calParam.Calculate_AnimMod();

				if (!isUpdatable)
				{
					calParam._isAvailable = false;
					continue;
				}
				else
				{
					calParam._isAvailable = true;
				}

				

				subParamKeyValueList = null;
				keyParamSetGroup = null;
				_keyAnimClip = null;
				_keyAnimPlayUnit = null;

				//calParam._result_Matrix.SetIdentity();//>TF

				calParam._isColorCalculated = false;

				//>Color
				calParam._result_Color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				calParam._result_IsVisible = false;


				//추가 12.5 : Extra Option 초기화
				calParam._isExtra_DepthChanged = false;
				calParam._isExtra_TextureChanged = false;
				calParam._extra_DeltaDepth = 0;
				calParam._extra_TextureDataID = -1;
				calParam._extra_TextureData = null;



				//초기화
				//tmpMatrix.SetIdentity();//>TF
				layerWeight = 0.0f;

				//tmpBoneIKWeight = 0.0f;
				tmpVisible = false;

				int iCalculatedSubParam = 0;

				iColoredKeyParamSetGroup = 0;//<<실제 Color 처리가 된 ParamSetGroup의 개수
				tmpIsColoredKeyParamSetGroup = false;

				
				//애니메이션 레이어 변수도 초기화 << 추가 20.4.18
				_curAnimLayeredParam = null;
				_nAnimLayeredParams = 0;
				_curAnimLayer = -1;
				_isAnimAnyExtraCalculated = false;



				//-------------------------------------
				
				//이전 : 모든 SubList를 순회
				//for (int iSubList = 0; iSubList < subParamGroupList.Count; iSubList++)
				//{
				//	curSubList = subParamGroupList[iSubList];

				//변경 20.11.23 : AnimPlayMapping을 이용하여 미리 정렬된 순서로 SubList를 호출
				int iTargetSubList = 0;
				apAnimPlayMapping.LiveUnit curUnit = null;

				for (int iUnit = 0; iUnit < _animPlayMapping._nAnimClips; iUnit++)
				{
					curUnit = _animPlayMapping._liveUnits_Sorted[iUnit];
					if(!curUnit._isLive)
					{
						//재생 종료
						//이 뒤는 모두 재생이 안되는 애니메이션이다.
						break;
					}

					iTargetSubList = curUnit._animIndex;
					curSubList = calParam._subParamKeyValueList_AnimSync[iTargetSubList];
					if(curSubList == null)
					{
						//이게 Null이라는 것은, 이 AnimClip에 대한 TimelineLayer와 Mod는 없다는 것
						continue;
					}
					
					//----------------------여기까지


					//이전 방식
					//int nParamKeys = curSubList._subParamKeyValues.Count;//Sub Params
					//subParamKeyValueList = curSubList._subParamKeyValues;

					//다른 방식 20.11.24 : 모든 ParamKeyValue가 아니라, 유효한 ParamKeyValue만 체크한다.
					int nValidParamKeyValues = curSubList._nResultAnimKey;
					if(nValidParamKeyValues == 0)
					{
						continue;
					}
					


					paramKeyValue = null;

					keyParamSetGroup = curSubList._keyParamSetGroup;

					//삭제 20.11.24 : 위에서 AnimPlayMapping의 LiveUnit에서 미리 결정된다.
					////애니메이션 모디파이어에서 실행되지 않은 애니메이션에 대한 PSG는 생략한다.
					//if (!keyParamSetGroup.IsAnimEnabled)
					//{
					//	//(KeyParamSetGroup이 AnimClip > Timeline (Modifier) > TimelineLayer에 해당한다.)
					//	continue;
					//}


					//이 로직이 추가된다. (20.4.18)
					_keyAnimClip = keyParamSetGroup._keyAnimClip;
					_keyAnimPlayUnit = _keyAnimClip._parentPlayUnit;

					//레이어별로 데이터를 그룹으로 만들어서 계산하자
					if (_curAnimLayer != _keyAnimPlayUnit._layer || _curAnimLayeredParam == null)
					{
						//다음 레이어를 꺼내자
						//- 풀에 이미 생성된 레이어가 있다면 가져오고, 없으면 생성한다.
						if (_nAnimLayeredParams < _animLayeredParams.Count)
						{
							//가져올 레이어 파라미터가 있다.
							_curAnimLayeredParam = _animLayeredParams[_nAnimLayeredParams];
							_curAnimLayeredParam.ReadyToUpdate();
						}
						else
						{
							//풀에 레이어 파라미터가 없으므로 새로 만들어서 풀에 넣어주자
							//생성자에 ReadyToUpdate가 포함되어 있다.

							//>TF
							//_curAnimLayeredParam = AnimLayeredParam.Make_TF(_nAnimLayeredParams);

							//>Color
							_curAnimLayeredParam = AnimLayeredParam.Make_Color(_nAnimLayeredParams);

							_animLayeredParams.Add(_curAnimLayeredParam);
						}

						_curAnimLayer = _keyAnimPlayUnit._layer;
						_nAnimLayeredParams++;//사용된 레이어 파라미터의 개수
					}




					//레이어 내부의 임시 데이터를 먼저 초기화
					//tmpMatrix.SetZero();//>TF

					tmpColor = Color.clear;

					tmpVisible = false;

					tmpTotalParamSetWeight = 0.0f;
					nColorCalculated = 0;

					//KeyParamSetGroup이 Color를 지원하는지 체크
					//tmpIsColoredKeyParamSetGroup = _isColorProperty && keyParamSetGroup._isColorPropertyEnabled && !isBoneTarget;
					tmpIsColoredKeyParamSetGroup = true;//>Color (이 조건문이 의미없다)


					//추가 20.2.24 : 색상 토글 옵션 > 애니메이션에서는 무조건 false
					//tmpIsToggleShowHideOption = !IsAnimated && tmpIsColoredKeyParamSetGroup && keyParamSetGroup._isToggleShowHideWithoutBlend;
					tmpIsToggleShowHideOption = false;

					#region [미사용 코드] tmpIsToggleShowHideOption는 항상 false다
					//if (tmpIsToggleShowHideOption)
					//{
					//	tmpToggleOpt_IsAnyKey_Shown = false;
					//	tmpToggleOpt_TotalWeight_Shown = 0.0f;
					//	tmpToggleOpt_MaxWeight_Shown = 0.0f;
					//	tmpToggleOpt_KeyIndex_Shown = 0.0f;
					//	tmpToggleOpt_IsAny_Hidden = false;
					//	tmpToggleOpt_TotalWeight_Hidden = 0.0f;
					//	tmpToggleOpt_MaxWeight_Hidden = 0.0f;
					//	tmpToggleOpt_KeyIndex_Hidden = 0.0f;
					//} 
					#endregion


					//변경 20.4.20 : 초기화 위치가 여기여야 한다.
					if(_isExtraPropertyEnabled)
					{
						//추가 12.5 : Extra Option 계산 값				
						tmpExtra_DepthChanged = false;
						tmpExtra_TextureChanged = false;
						tmpExtra_DeltaDepth = 0;
						tmpExtra_TextureDataID = 0;
						tmpExtra_TextureData = null;
						tmpExtra_DepthMaxWeight = -1.0f;//최대 Weight 값
						tmpExtra_TextureMaxWeight = -1.0f;//최대 Weight 값
					}



					//ModMesh를 활용하는 타입인 경우 >> ModMeshSet으로 변경
					//추가 20.9.11 : 정밀한 보간을 위해 Default Matrix가 필요하다.
					//>TF
					//apMatrix defaultMatrixOfRenderUnit = null;
					//if(calParam._targetOptTransform != null)
					//{
					//	defaultMatrixOfRenderUnit = calParam._targetOptTransform._defaultMatrix;
					//}


					//변경 20.11.24 모든 키프레임 순회 방식 제거
					//이전 방식
					
					//변경 20.11.24 : 전체 체크 > 이미 보간된 것만 체크
					for (int iPV = 0; iPV < nValidParamKeyValues; iPV++)
					{
						paramKeyValue = curSubList._resultAnimKeyPKVs[iPV];//보간이 완료된 PKV만 가져온다.

						//여기까지...


						if (!paramKeyValue._isCalculated) { continue; }

						tmpSubModMesh_Transform = paramKeyValue._modifiedMeshSet.SubModMesh_Transform;

						//ParamSetWeight를 추가
						tmpTotalParamSetWeight += paramKeyValue._weight * paramKeyValue._paramSet._overlapWeight;

						//>TF
						////Weight에 맞게 Matrix를 만들자
						//if (paramKeyValue._isAnimRotationBias)
						//{
						//	//추가 : RotationBias가 있다면 미리 계산된 Bias Matrix를 사용한다.
						//	tmpMatrix.AddMatrixParallel_ModMesh(paramKeyValue.AnimRotationBiasedMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
						//}
						//else
						//{
						//	//ModMeshSet으로 변경
						//	tmpMatrix.AddMatrixParallel_ModMesh(tmpSubModMesh_Transform._transformMatrix, defaultMatrixOfRenderUnit, paramKeyValue._weight);
						//}


						//색상 처리
						tmpSubModMesh_Color = paramKeyValue._modifiedMeshSet.SubModMesh_Color;
						if (tmpSubModMesh_Color != null)
						{
							#region [미사용 코드] tmpIsToggleShowHideOption는 항상 false다.
							//if (!tmpIsToggleShowHideOption)
							//{
							//	//기본방식
							//	if (tmpSubModMesh_Color._isVisible)
							//	{
							//		tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
							//		tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
							//	}
							//	else
							//	{
							//		tmpParamColor = tmpSubModMesh_Color._meshColor;
							//		tmpParamColor.a = 0.0f;
							//		tmpColor += tmpParamColor * paramKeyValue._weight;
							//	}
							//}
							//else
							//{
							//	//추가 20.2.24 : 토글 방식의 ShowHide 방식
							//	if (tmpSubModMesh_Color._isVisible && paramKeyValue._weight > 0.0f)
							//	{
							//		//paramKeyValue._paramSet.ControlParamValue
							//		tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
							//		tmpVisible = true;//< 일단 이것도 true

							//		//토글용 처리
							//		tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

							//		//0.5 Weight시 인덱스 비교를 위해 키 인덱스 위치를 저장하자.
							//		if (!tmpToggleOpt_IsAnyKey_Shown)
							//		{
							//			tmpToggleOpt_KeyIndex_Shown = tmpToggleOpt_KeyIndex_Cal;
							//		}
							//		else
							//		{
							//			//Show Key Index 중 가장 작은 값을 기준으로 한다.
							//			tmpToggleOpt_KeyIndex_Shown = (tmpToggleOpt_KeyIndex_Cal < tmpToggleOpt_KeyIndex_Shown ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Shown);
							//		}


							//		tmpToggleOpt_IsAnyKey_Shown = true;

							//		tmpToggleOpt_TotalWeight_Shown += paramKeyValue._weight;
							//		tmpToggleOpt_MaxWeight_Shown = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Shown ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Shown);

							//	}
							//	else
							//	{
							//		//토글용 처리
							//		tmpToggleOpt_KeyIndex_Cal = paramKeyValue._paramSet.ComparableIndex;

							//		if (!tmpToggleOpt_IsAny_Hidden)
							//		{
							//			tmpToggleOpt_KeyIndex_Hidden = tmpToggleOpt_KeyIndex_Cal;
							//		}
							//		else
							//		{
							//			//Hidden Key Index 중 가장 큰 값을 기준으로 한다.
							//			tmpToggleOpt_KeyIndex_Hidden = (tmpToggleOpt_KeyIndex_Cal > tmpToggleOpt_KeyIndex_Hidden ? tmpToggleOpt_KeyIndex_Cal : tmpToggleOpt_KeyIndex_Hidden);
							//		}

							//		tmpToggleOpt_IsAny_Hidden = true;
							//		tmpToggleOpt_TotalWeight_Hidden += paramKeyValue._weight;
							//		tmpToggleOpt_MaxWeight_Hidden = (paramKeyValue._weight > tmpToggleOpt_MaxWeight_Hidden ? paramKeyValue._weight : tmpToggleOpt_MaxWeight_Hidden);
							//	}
							//} 
							#endregion

							//기본방식
							if (tmpSubModMesh_Color._isVisible)
							{
								tmpColor += tmpSubModMesh_Color._meshColor * paramKeyValue._weight;
								tmpVisible = true;//하나라도 Visible이면 Visible이 된다.
							}
							else
							{
								tmpParamColor = tmpSubModMesh_Color._meshColor;
								tmpParamColor.a = 0.0f;
								tmpColor += tmpParamColor * paramKeyValue._weight;
							}
						}

						nColorCalculated++;


						//---------------------------------------------
						//추가 12.5 : Extra Option
						if (_isExtraPropertyEnabled)
						{
							//변경 : ModMeshSet을 이용하는 것으로 변경
							tmpSubModMesh_Extra = paramKeyValue._modifiedMeshSet.SubModMesh_Extra;

							if (tmpSubModMesh_Extra != null
									&& (tmpSubModMesh_Extra._extraValue._isDepthChanged || tmpSubModMesh_Extra._extraValue._isTextureChanged)
									)
							{
								//현재 ParamKeyValue의 CutOut된 가중치를 구해야한다.
								float extraWeight = paramKeyValue._weight;//<<일단 가중치를 더한다.
								float bias = 0.0001f;
								float overlapBias = 0.01f;
								float cutOut = 0.0f;
								bool isExactWeight = false;
								if (IsAnimated)
								{
									switch (paramKeyValue._animKeyPos)
									{
										case apOptCalculatedResultParam.AnimKeyPos.ExactKey:
											isExactWeight = true;
											break;
										case apOptCalculatedResultParam.AnimKeyPos.NextKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimPrev;
											break; //Next Key라면 Prev와의 CutOut을 가져온다.
										case apOptCalculatedResultParam.AnimKeyPos.PrevKey:
											cutOut = tmpSubModMesh_Extra._extraValue._weightCutout_AnimNext;
											break;//Prev Key라면 Next와의 CutOut을 가져온다.
									}
								}
								else
								{
									cutOut = tmpSubModMesh_Extra._extraValue._weightCutout;
								}

								cutOut = Mathf.Clamp01(cutOut + overlapBias);//살짝 겹치게

								if (isExactWeight)
								{
									extraWeight = 10000.0f;
								}
								else if (cutOut < bias)
								{
									//정확하면 최대값
									//아니면 적용안함
									if (extraWeight > 1.0f - bias)	{ extraWeight = 10000.0f; }
									else							{ extraWeight = -1.0f; }
								}
								else
								{
									if (extraWeight < 1.0f - cutOut)	{ extraWeight = -1.0f; }
									else								{ extraWeight = (extraWeight - (1.0f - cutOut)) / cutOut; }
								}

								if (extraWeight > 0.0f)
								{
									if (tmpSubModMesh_Extra._extraValue._isDepthChanged)
									{
										//2-1. Depth 이벤트
										if (extraWeight > tmpExtra_DepthMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_DepthMaxWeight = extraWeight;
											tmpExtra_DepthChanged = true;
											tmpExtra_DeltaDepth = tmpSubModMesh_Extra._extraValue._deltaDepth;
										}

									}
									if (tmpSubModMesh_Extra._extraValue._isTextureChanged)
									{
										//2-2. Texture 이벤트
										if (extraWeight > tmpExtra_TextureMaxWeight)
										{
											//가중치가 최대값보다 큰 경우
											tmpExtra_TextureMaxWeight = extraWeight;
											tmpExtra_TextureChanged = true;
											tmpExtra_TextureData = tmpSubModMesh_Extra._extraValue._linkedTextureData;
											tmpExtra_TextureDataID = tmpSubModMesh_Extra._extraValue._textureDataID;
										}
									}
								}
							}
						}
						//---------------------------------------------
					}

					//>TF
					//tmpMatrix.CalculateScale_FromAdd();
					//tmpMatrix.CalculateLocalPos_ModMesh(defaultMatrixOfRenderUnit);//추가 (20.9.11) : 위치 보간이슈 수정

					//추가 : ParamSetWeight를 사용한다면 -> LayerWeight x ParamSetWeight(0~1)을 사용한다.


					
					if (!_isUseParamSetWeight)
					{
						layerWeight = Mathf.Clamp01(curUnit._playWeight);//변경 20.11.23 : 일일이 계산된 KeyParamSetGroup의 Weight대신, 일괄 계산된 LiveUnit의 값을 이용
					}
					else
					{
						layerWeight = Mathf.Clamp01(curUnit._playWeight * Mathf.Clamp01(tmpTotalParamSetWeight));//변경 20.11.23 : 위와 동일
					}

					if (layerWeight < 0.001f)
					{
						continue;
					}

					//if ((nColorCalculated == 0 && _isColorProperty) || isBoneTarget)
					if (nColorCalculated == 0)//>Color
					{
						tmpVisible = true;
						tmpColor = _defaultColor;
						
						//if (!isBoneTarget)
						//{
						//	tmpMatrix.SetIdentity();
						//	tmpColor = _defaultColor;
						//}
					}



					//20.4.18 : 애니메이션 로직은 레이어별로 처리를 하므로, 색상 로직을 위로 올려서 여기서 미리 처리한다.
					#region [미사용 코드] tmpIsToggleShowHideOption는 항상 false다.
					//if (tmpIsColoredKeyParamSetGroup)
					//{
					//	//토글 방식이면 tmpColor, tmpVisible을 다시 설정한다.
					//	if (tmpIsToggleShowHideOption)
					//	{
					//		if (tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
					//		{
					//			//Show / Hide가 모두 있다면 토글 대상
					//			if (tmpToggleOpt_MaxWeight_Shown > tmpToggleOpt_MaxWeight_Hidden)
					//			{
					//				//Show가 더 크다
					//				tmpVisible = true;
					//			}
					//			else if (tmpToggleOpt_MaxWeight_Shown < tmpToggleOpt_MaxWeight_Hidden)
					//			{
					//				//Hidden이 더 크다
					//				tmpVisible = false;
					//				tmpColor = Color.clear;
					//			}
					//			else
					//			{
					//				//같다면? (Weight가 0.5 : 0.5로 같은 경우)
					//				if (tmpToggleOpt_KeyIndex_Shown > tmpToggleOpt_KeyIndex_Hidden)
					//				{
					//					//Show의 ParamSet의 키 인덱스가 더 크다.
					//					tmpVisible = true;
					//				}
					//				else
					//				{
					//					//Hidden이 더 크다
					//					tmpVisible = false;
					//					tmpColor = Color.clear;
					//				}
					//			}
					//		}
					//		else if (tmpToggleOpt_IsAnyKey_Shown && !tmpToggleOpt_IsAny_Hidden)
					//		{
					//			//Show만 있다면
					//			tmpVisible = true;
					//		}
					//		else if (!tmpToggleOpt_IsAnyKey_Shown && tmpToggleOpt_IsAny_Hidden)
					//		{
					//			//Hide만 있다면
					//			tmpVisible = false;
					//			tmpColor = Color.clear;
					//		}
					//		else
					//		{
					//			//둘다 없다면? 숨기자.
					//			tmpVisible = false;
					//			tmpColor = Color.clear;
					//		}

					//		//Show 상태면 Weight를 다시 역산해서 색상을 만들어야 한다.
					//		if (tmpVisible && tmpToggleOpt_TotalWeight_Shown > 0.0f)
					//		{
					//			tmpColor.r = Mathf.Clamp01(tmpColor.r / tmpToggleOpt_TotalWeight_Shown);
					//			tmpColor.g = Mathf.Clamp01(tmpColor.g / tmpToggleOpt_TotalWeight_Shown);
					//			tmpColor.b = Mathf.Clamp01(tmpColor.b / tmpToggleOpt_TotalWeight_Shown);
					//			tmpColor.a = Mathf.Clamp01(tmpColor.a / tmpToggleOpt_TotalWeight_Shown);
					//		}
					//	}
					//} 
					#endregion



					//중요! 20.4.18
					//애니메이션은 그룹별로 값을 모아야 한다.
					_curAnimLayeredParam._totalWeight += layerWeight;

					if (_curAnimLayeredParam._iCurAnimClip == 0)
					{
						//첫번째 애니메이션 클립일 때
						//레이어의 블렌드 방식은 첫번째 클립의 값을 따른다.
						if (_keyAnimPlayUnit.BlendMethod == apAnimPlayUnit.BLEND_METHOD.Interpolation)
						{
							_curAnimLayeredParam._blendMethod = apModifierParamSetGroup.BLEND_METHOD.Interpolation;
						}
						else
						{
							_curAnimLayeredParam._blendMethod = apModifierParamSetGroup.BLEND_METHOD.Additive;
						}

						_curAnimLayeredParam._isCalculated = true;

						//>TF
						////Matrix를 그대로 할당.
						//_curAnimLayeredParam._cal_Matrix.SetTRSForLerp(tmpMatrix);
					}
					//else
					//{
					//	//>TF
					//	////두번째 클립부터는 무조건 Interpolation 방식으로 뒤에 값을 추가해야한다. <중요>
					//	//_curAnimLayeredParam._cal_Matrix.LerpMatrixLayered(tmpMatrix, layerWeight);
					//}


					//색상 옵션도 레이어 파라미터에 넣자.
					//레이어 내에서는 무조건 Interpolation이다.
					if (_curAnimLayeredParam._nCal_Color == 0)
					{
						//첫번째는 그대로 대입
						_curAnimLayeredParam._cal_Color = tmpColor;
						_curAnimLayeredParam._cal_Visible = tmpVisible;
					}
					else
					{
						//두번째부터는 Interpolation
						_curAnimLayeredParam._cal_Color = apUtil.BlendColor_ITP(
															_curAnimLayeredParam._cal_Color,
															tmpColor,
															layerWeight);

						//Visible은 OR연산
						_curAnimLayeredParam._cal_Visible |= tmpVisible;
					}

						

					_curAnimLayeredParam._isCal_Color = true;
					_curAnimLayeredParam._nCal_Color++;


					//Extra Option > 레이어 파라미터
					if (_isExtraPropertyEnabled)
					{
						if (tmpExtra_DepthChanged)
						{
							_curAnimLayeredParam._isCal_ExtraDepth = true;
							_curAnimLayeredParam._cal_ExtraDepth = tmpExtra_DeltaDepth;

							_isAnimAnyExtraCalculated = true;
						}
						else
						{
							_curAnimLayeredParam._isCal_ExtraDepth = false;
							_curAnimLayeredParam._cal_ExtraDepth = 0;
						}

						if (tmpExtra_TextureChanged)
						{
							_curAnimLayeredParam._isCal_ExtraTexture = true;
							_curAnimLayeredParam._cal_ExtraTexture = tmpExtra_TextureData;
							_curAnimLayeredParam._cal_ExtraTextureID = tmpExtra_TextureDataID;

							_isAnimAnyExtraCalculated = true;
						}
						else
						{
							_curAnimLayeredParam._isCal_ExtraTexture = false;
							_curAnimLayeredParam._cal_ExtraTexture = null;
							_curAnimLayeredParam._cal_ExtraTextureID = -1;
						}
					}

					_curAnimLayeredParam._iCurAnimClip++;
					
					iCalculatedSubParam++;
				}
				
				
				
				//KeyParamSetGroup > 레이어 데이터로 누적 끝



				//저장된 AnimLayeredParam들을 CalPram으로 적용하자

				_iColoredAnimLayeredParam = 0;
				_layeredAnimWeightClamped = 0.0f;

				for (int iAnimLayeredParam = 0; iAnimLayeredParam < _nAnimLayeredParams; iAnimLayeredParam++)
				{
					_curAnimLayeredParam = _animLayeredParams[iAnimLayeredParam];
					if(!_curAnimLayeredParam._isCalculated || _curAnimLayeredParam._totalWeight < 0.0001f)
					{
						//처리 끝
						break;
					}

					_layeredAnimWeightClamped = Mathf.Clamp01(_curAnimLayeredParam._totalWeight);
					
					//>TF
					//_curAnimLayeredParam._cal_Matrix.CalculateScale_FromLerp();//계산 완료된 Matrix 처리

					calParam._totalParamSetGroupWeight += _layeredAnimWeightClamped;


					//>TF
					//if (iAnimLayeredParam == 0)
					//{
					//	//Matrix 할당
					//	calParam._result_Matrix.SetTRSForLerp(_curAnimLayeredParam._cal_Matrix);
					//}
					//else
					//{
					//	//레이어의 블렌드 방식에 따라 값 적용
					//	switch (_curAnimLayeredParam._blendMethod)
					//	{
					//		case apModifierParamSetGroup.BLEND_METHOD.Additive:
					//			{
					//				//변경 3.27 : apMatrixCal로 계산
					//				calParam._result_Matrix.AddMatrixLayered(
					//									_curAnimLayeredParam._cal_Matrix, 
					//									_layeredAnimWeightClamped);
					//			}
					//			break;

					//		case apModifierParamSetGroup.BLEND_METHOD.Interpolation:
					//			{
					//				//변경 3.27 : apMatrixCal로 계산
					//				calParam._result_Matrix.LerpMatrixLayered(
					//									_curAnimLayeredParam._cal_Matrix, 
					//									_layeredAnimWeightClamped);
					//			}
					//			break;
					//	}
					//}

					//색상 처리
					if(_curAnimLayeredParam._isCal_Color)
					{
						if(_iColoredAnimLayeredParam == 0
							|| _curAnimLayeredParam._blendMethod == apModifierParamSetGroup.BLEND_METHOD.Interpolation)
						{
							//색상 Interpolation
							calParam._result_Color = apUtil.BlendColor_ITP(	calParam._result_Color, 
																			_curAnimLayeredParam._cal_Color, 
																			_layeredAnimWeightClamped);
						}
						else
						{
							//색상 Additive
							calParam._result_Color = apUtil.BlendColor_Add(	calParam._result_Color, 
																			_curAnimLayeredParam._cal_Color, 
																			_layeredAnimWeightClamped);
						}

						calParam._result_IsVisible |= _curAnimLayeredParam._cal_Visible;

						_iColoredAnimLayeredParam++;
						calParam._isColorCalculated = true;
					}
					

					//Extra Option를 CalParam에 전달
					if(_isExtraPropertyEnabled && _isAnimAnyExtraCalculated)
					{
						//활성, 비활성에 상관없이 마지막 레이어의 값이 항상 반영된다.

						if(_curAnimLayeredParam._isCal_ExtraDepth)
						{
							calParam._isExtra_DepthChanged = true;
							calParam._extra_DeltaDepth = _curAnimLayeredParam._cal_ExtraDepth;
						}
						else
						{
							calParam._isExtra_DepthChanged = false;
							calParam._extra_DeltaDepth = 0;
						}


						if(_curAnimLayeredParam._isCal_ExtraTexture)
						{
							calParam._isExtra_TextureChanged = true;
							calParam._extra_TextureData = _curAnimLayeredParam._cal_ExtraTexture;
							calParam._extra_TextureDataID = _curAnimLayeredParam._cal_ExtraTextureID;
						}
						else
						{
							calParam._isExtra_TextureChanged = false;
							calParam._extra_TextureData = null;
							calParam._extra_TextureDataID = -1;
						}
					}
				}


				//레이어 > CalParam 처리 끝

				if (iCalculatedSubParam == 0)
				{
					calParam._isAvailable = false;
				}
				else
				{
					calParam._isAvailable = true;

					//>TF
					//변경 : apMatrixCal로 계산한 경우
					//calParam._result_Matrix.CalculateScale_FromLerp();
				}
			}
		}





		//------------------------------------------------------------------------------------------------------------------------------------


		// Get / Set
		//---------------------------------------------------------------------------------------
		/// <summary>
		/// CalculatedResultParam을 찾는다.
		/// Bone은 Null인 대상만을 고려한다.
		/// </summary>
		/// <param name="targetOptTransform"></param>
		/// <returns></returns>
		public apOptCalculatedResultParam GetCalculatedResultParam(apOptTransform targetOptTransform)
		{
			return _calculatedResultParams.Find(delegate (apOptCalculatedResultParam a)
			{
				return a._targetOptTransform == targetOptTransform && a._targetBone == null;
			});
		}

		/// <summary>
		/// GetCalculatedResultParam의 ModBone 버전.
		/// Bone까지 비교하여 동일한 CalculatedResultParam을 찾는다.
		/// </summary>
		/// <param name="targetOptTransform"></param>
		/// <param name="bone"></param>
		/// <returns></returns>
		public apOptCalculatedResultParam GetCalculatedResultParam_Bone(apOptTransform targetOptTransform, apOptBone bone, apOptTransform ownerOptTransform)
		{
			return _calculatedResultParams.Find(delegate (apOptCalculatedResultParam a)
			{
				return a._targetOptTransform == targetOptTransform && a._targetBone == bone
				&& a._ownerOptTransform == ownerOptTransform;
			});
		}


	}

}