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
	/// The bound of the region where the position of the bone is restricted
	/// </summary>
	public enum ConstraintBound
	{
		/// <summary>Recommended X-value of position limited</summary>
		Xprefer,
		/// <summary>Minimum X-value of position limited</summary>
		Xmin,
		/// <summary>Maxmum X-value of position limited</summary>
		Xmax,
		/// <summary>Recommended Y-value of position limited</summary>
		Yprefer,
		/// <summary>Minimum Y-value of position limited</summary>
		Ymin,
		/// <summary>Maxmum Y-value of position limited</summary>
		Ymax
	}
	public enum ConstraintSurface
	{
		/// <summary>The surface of the X axis. It is like a wall</summary>
		Xsurface,
		/// <summary>The surface of the Y axis. It is like a ground</summary>
		Ysurface
	}

	// MeshGroup에 포함되는 apBone의 Opt 버전
	// MeshGroup에 해당되는 Root OptTransform의 "Bones" GameObject에 포함된다.
	// Matrix 계산은 Bone과 동일하며, Transform에 반영되지 않는다. (Transform은 Local Pos와 Rotation만 계산된다)
	// Transform은 Rigging에 반영되지는 않지만, 만약 어떤 오브젝트를 Attachment 한다면 사용되어야 한다.
	// Opt Bone의 Transform은 외부 입력은 무시하며, Attachment를 하는 용도로만 사용된다.
	// Attachment를 하는 경우 하위에 Socket Transform을 생성한뒤, 거기서 WorldMatrix에 해당하는 TRS를 넣는다. (값 자체는 Local Matrix)
	
		/// <summary>
	/// A class in which "apBone" is baked
	/// It belongs to "apOptTransform".
	/// (It is recommended to use the functions of "apPortrait" to control the position of the bone, and it is recommended to use "Socket" when referring to the position.)
	/// </summary>
	public class apOptBone : MonoBehaviour
	{
		// Members
		//---------------------------------------------------------------
		// apBone 정보를 옮기자
		/// <summary>Bone Name</summary>
		public string _name = "";

		/// <summary>[Please do not use it] Bone's Unique ID</summary>
		public int _uniqueID = -1;

		/// <summary>[Please do not use it] Parent MeshGroup ID</summary>
		public int _meshGroupID = -1;

		//이건 Serialize가 된다.
		/// <summary>Parent Opt-Transform</summary>
		public apOptTransform _parentOptTransform = null;

		/// <summary>
		/// Parent Opt-Bone
		/// </summary>
		public apOptBone _parentBone = null;

		/// <summary>
		/// Children of a Bone
		/// </summary>
		public apOptBone[] _childBones = null;//<ChildBones의 배열버전

		/// <summary>
		/// [Please do not use it] Default Matrix
		/// </summary>
		[SerializeField]
		public apMatrix _defaultMatrix = new apMatrix();

		[NonSerialized]
		private Vector2 _deltaPos = Vector2.zero;

		[NonSerialized]
		private float _deltaAngle = 0.0f;

		[NonSerialized]
		private Vector2 _deltaScale = Vector2.one;

		/// <summary>[Please do not use it] Local Matrix</summary>
		[NonSerialized]
		public apMatrix _localMatrix = new apMatrix();
		
		/// <summary>[Please do not use it] World Matrix</summary>
		[NonSerialized]
		public apOptBoneWorldMatrix _worldMatrix = null;//변경 20.8.30 : BoneWorldMatrix를 이용한다.
		//public apMatrix _worldMatrix = new apMatrix();//<<이전

		/// <summary>
		/// [Please do not use it] World Matrix (Default)
		/// </summary>
		[NonSerialized]
		public apOptBoneWorldMatrix _worldMatrix_NonModified = null;//변경 20.8.30 : BoneWorldMatrix를 이용한다.
		//public apMatrix _worldMatrix_NonModified = new apMatrix();
		

		//추가 : 계산된 Bone IK Controller Weight와 IK용 worldMatrix > 이건 편집과 구분하기 위해 별도로 계산.
		[NonSerialized]
		public float _calculatedBoneIKWeight = 0.0f;

		/// <summary>
		/// [Please do not use it] Rigging Matrix
		/// </summary>
		//리깅을 위한 통합 Matrix
		[NonSerialized]
		public apMatrix3x3 _vertWorld2BoneModWorldMatrix = new apMatrix3x3();//<<이게 문제


		[NonSerialized]
		public apPortrait.ROOT_BONE_SCALE_METHOD _rootBoneScaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;
		


		//Shape 계열
		/// <summary>
		/// [Please do not use it] Bone Color in Editor / Gizmo
		/// </summary>
		[SerializeField]
		public Color _color = Color.white;

		public int _shapeWidth = 30;
		public int _shapeLength = 50;//<<이 값은 생성할 때 Child와의 거리로 판단한다.
		public int _shapeTaper = 100;//기본값은 뾰족

#if UNITY_EDITOR
		private Vector2 _shapePoint_End = Vector3.zero;

		private Vector2 _shapePoint_Mid1 = Vector3.zero;
		private Vector2 _shapePoint_Mid2 = Vector3.zero;
		private Vector2 _shapePoint_End1 = Vector3.zero;
		private Vector2 _shapePoint_End2 = Vector3.zero;
#endif

		//IK 정보
		/// <summary>[Please do not use it]</summary>
		public apBone.OPTION_LOCAL_MOVE _optionLocalMove = apBone.OPTION_LOCAL_MOVE.Disabled;

		/// <summary>[Please do not use it] Bone's IK Type</summary>
		public apBone.OPTION_IK _optionIK = apBone.OPTION_IK.IKSingle;

		// Parent로부터 IK의 대상이 되는가? IK Single일 때에도 Tail이 된다.
		// (자신이 IK를 설정하는 것과는 무관함)
		/// <summary> [Please do not use it] </summary>
		public bool _isIKTail = false;

		//IK의 타겟과 Parent
		/// <summary>[Please do not use it]</summary>
		public int _IKTargetBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKTargetBone = null;

		/// <summary>[Please do not use it]</summary>
		public int _IKNextChainedBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKNextChainedBone = null;


		// IK Tail이거나 IK Chained 상태라면 Header를 저장하고, Chaining 처리를 해야한다.
		/// <summary>[Please do not use it]</summary>
		public int _IKHeaderBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKHeaderBone = null;



		//IK시 추가 옵션

		// IK 적용시, 각도를 제한을 줄 것인가 (기본값 False)
		/// <summary>[Please do not use it] IK Angle Contraint Option</summary>
		public bool _isIKAngleRange = false;

		/// <summary>[Please do not use it]</summary>
		public float _IKAngleRange_Lower = -90.0f;//음수여야 한다.

		/// <summary>[Please do not use it]</summary>
		public float _IKAngleRange_Upper = 90.0f;//양수여야 한다.

		/// <summary>[Please do not use it]</summary>
		public float _IKAnglePreferred = 0.0f;//선호하는 각도 Offset



		// IK 연산이 되었는가
		/// <summary>
		/// Is IK Calculated
		/// </summary>
		[NonSerialized]
		public bool _isIKCalculated = false;

		// IK 연산이 발생했을 경우, World 좌표계에서 Angle을 어떻게 만들어야 하는지 계산 결과값
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public float _IKRequestAngleResult_World = 0.0f;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public float _IKRequestAngleResult_Delta = 0.0f;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public float _IKRequestWeight = 0.0f;
		

		/// <summary>
		/// IK 계산을 해주는 Chain Set.
		/// </summary>
		[SerializeField]
		private apOptBoneIKChainSet _IKChainSet = null;//<<이거 Opt 버전으로 만들자

		[SerializeField]
		private bool _isIKChainSetAvailable = false;

		private bool _isIKChainInit = false;



		//추가 : 이건 나중에 세팅하자
		//Transform에 적용되는 Local Matrix 값 (Scale이 없다)
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public apMatrix _transformLocalMatrix = new apMatrix();

		//Attach시 만들어지는 Socket
		//Socket 옵션은 Bone에서 미리 세팅해야한다.
		/// <summary>
		/// Socket Transform.
		/// In Unity World, this is a Socket that actually has the position, rotation, and size of the bone. 
		/// If you want to refer to the position or rotation of the bone from the outside, it is recommended to use Socket.
		/// </summary>
		public Transform _socketTransform = null;

		//추가 5.8
		//Position Controller와 LookAt Controller를 추가했다.
		[SerializeField]
		public apOptBoneIKController _IKController = null;

		[NonSerialized]
		public bool _isIKCalculated_Controlled = false;

		[NonSerialized]
		public float _IKRequestAngleResult_Controlled = 0.0f;

		[NonSerialized]
		public float _IKRequestWeight_Controlled = 0.0f;

		//[NonSerialized]
		//private bool _isIKRendered_Controller = false;


		//추가 20.5.24 : 지글본 옵션
		public bool _isJiggle = false;
		public float _jiggle_Mass = 1.0f;//질량
		public float _jiggle_K = 0.5f;//복원력에 해당하는 k/m의 값
		public float _jiggle_Drag = 0.5f;//공기 저항. 값이 클 수록 이전 위치에 있으려고 한다.(0~1)
		public float _jiggle_Damping = 0.5f;//감속력에 해당하는 값. 값이 클 수록 금방 속도가 줄어든다. (0~1)
		public bool _isJiggleAngleConstraint = false;
		public float _jiggle_AngleLimit_Min = 0.0f;
		public float _jiggle_AngleLimit_Max = 0.0f;

		//추가 22.7.6 : 지글본 가중치
		[SerializeField] public bool _jiggle_IsControlParamWeight = false;
		[SerializeField] public int _jiggle_WeightControlParamID = -1;
		[NonSerialized] private apControlParam _linkedJiggleControlParam = null;
		[NonSerialized] private bool _isJiggleWeightCalculatable = false;


		[NonSerialized]
		private bool _isJiggleChecked_Prev = false;//이전 프레임에서 지글본을 위한 값이 저장되었는가

		[NonSerialized]
		private float _calJig_Angle_Result_Prev = 0.0f;//이전 프레임에서의 결과
		
		[NonSerialized]
		private Vector2 _calJig_EndPos_Prev = Vector2.zero;
		
		[NonSerialized]
		private Vector3 _calJig_EndPosW_Prev = Vector3.zero;

		[NonSerialized]
		private float _calJig_Velocity = 0.0f;

		[NonSerialized] private bool _calJig_IsWeight = false;
		[NonSerialized] private float _calJig_Weight = 0.0f;
		[NonSerialized] private bool _calJig_IsZeroWeight = false;
		[NonSerialized] private bool _calJig_IsZeroWeight_Prev = false;
		private const float JIG_WEIGHT_CUTOUT_1 = 0.998f;
		private const float JIG_WEIGHT_CUTOUT_0 = 0.002f;



		//임시 변수들은 static으로 만들자 > 멤버로 만들어야 한다.
		[NonSerialized]
		private apOptBoneWorldMatrix _calJig_Tmp_WorldMatrix = null;//변경 20.8.30 : BoneWorldMatrix로 변경
		//private static apMatrix s_calJig_Tmp_WorldMatrix = new apMatrix();
		






		//스크립트로 TRS를 직접 제어할 수 있다.
		//단 Update마다 매번 설정해야한다.
		//좌표계는 WorldMatrix를 기준으로 한다.
		//값 자체는 절대값을 기준으로 한다.
		private bool _isExternalUpdate_Position = false;
		private bool _isExternalUpdate_Rotation = false;
		private bool _isExternalUpdate_Scaling = false;
		//private bool _isExternalUpdate_IK = false;//<<추가 20.8.31 : 외부의 요청에 따라 IK를 수정하는 경우
		private float _externalUpdateWeight = 0.0f;
		private Vector2 _exUpdate_Pos = Vector2.zero;
		private float _exUpdate_Angle = 0.0f;
		private Vector2 _exUpdate_Scale = Vector2.zero;


		//추가 6.7 : 영역을 제한하자
		private bool _isExternalConstraint = false;
		private bool _isExternalConstraint_Xmin = false;
		private bool _isExternalConstraint_Xmax = false;
		private bool _isExternalConstraint_Ymin = false;
		private bool _isExternalConstraint_Ymax = false;
		private bool _isExternalConstraint_Xpref = false;
		private bool _isExternalConstraint_Ypref = false;
		private bool _isExternalConstraint_Xsurface = false;
		private bool _isExternalConstraint_Ysurface = false;
		private Vector3 _externalConstraint_PosX = Vector3.zero;//x:min, y:pref, z:max 순서
		private Vector3 _externalConstraint_PosY = Vector3.zero;
		private Vector4 _externalConstraint_PosSurfaceX = Vector4.zero;//x:기준 좌표, y:surface의 현재 좌표, z:min, w:max
		private Vector4 _externalConstraint_PosSurfaceY = Vector4.zero;

		//처리된 TRS
		
		private Vector3 _updatedWorldPos = Vector3.zero;
		private float _updatedWorldAngle = 0.0f;
		private Vector3 _updatedWorldScale = Vector3.one;

		private Vector3 _updatedWorldPos_NoRequest = Vector3.zero;
		private float _updatedWorldAngle_NoRequest = 0.0f;
		private Vector3 _updatedWorldScale_NoRequest = Vector3.one;


		//추가 21.9.19 : 리타겟팅 동기화
		[NonSerialized]
		public apOptBone _syncBone = null;



		// Init
		//---------------------------------------------------------------
		void Start()
		{
			//업데이트 안합니더
			this.enabled = false;

			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;
			//_isExternalUpdate_IK = false;

			_isExternalConstraint = false;
			_isExternalConstraint_Xmin = false;
			_isExternalConstraint_Xmax = false;
			_isExternalConstraint_Ymin = false;
			_isExternalConstraint_Ymax = false;
			_isExternalConstraint_Xpref = false;
			_isExternalConstraint_Ypref = false;
			_isExternalConstraint_Xsurface = false;
			_isExternalConstraint_Ysurface = false;
		
		}


		//Link 함수의 내용은 Bake 시에 진행해야한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="bone"></param>
		public void Bake(apBone bone)
		{
			_name = bone._name;
			_uniqueID = bone._uniqueID;
			_meshGroupID = bone._meshGroupID;
			_defaultMatrix.SetMatrix(bone._defaultMatrix, false);

			
			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_localMatrix.SetIdentity();


			//추가 20.5.24 : 지글 본
			//지글본 설정에 맞아야만 동작한다.
			//- 헬퍼 본이 아니어야 한다 + 길이가 1이상
			_isJiggle = bone._isJiggle && !bone._shapeHelper && bone._shapeLength > 0;
			
			_jiggle_Mass = bone._jiggle_Mass;
			_jiggle_K = bone._jiggle_K;
			_jiggle_Drag = bone._jiggle_Drag;
			_jiggle_Damping = bone._jiggle_Damping;
			_isJiggleAngleConstraint = bone._isJiggleAngleConstraint;
			_jiggle_AngleLimit_Min = bone._jiggle_AngleLimit_Min;
			_jiggle_AngleLimit_Max = bone._jiggle_AngleLimit_Max;

			//추가 22.7.6 : 지글본 가중치
			_jiggle_IsControlParamWeight = bone._jiggle_IsControlParamWeight;
			_jiggle_WeightControlParamID = bone._jiggle_WeightControlParamID;
			_linkedJiggleControlParam = null;
			_isJiggleWeightCalculatable = false;



			InitWorldMatrix();



			_worldMatrix.SetIdentity();
			_worldMatrix_NonModified.SetIdentity();


			_vertWorld2BoneModWorldMatrix.SetIdentity();

			_color = bone._color;
			_shapeWidth = bone._shapeWidth;
			_shapeLength = bone._shapeLength;
			_shapeTaper = bone._shapeTaper;

			_optionLocalMove = bone._optionLocalMove;
			_optionIK = bone._optionIK;

			_isIKTail = bone._isIKTail;

			_IKTargetBoneID = bone._IKTargetBoneID;
			_IKTargetBone = null;//<<나중에 링크

			_IKNextChainedBoneID = bone._IKNextChainedBoneID;
			_IKNextChainedBone = null;//<<나중에 링크


			_IKHeaderBoneID = bone._IKHeaderBoneID;
			_IKHeaderBone = null;//<<나중에 링크


			_isIKAngleRange = bone._isIKAngleRange;
			//이게 기존 코드
			_IKAngleRange_Lower = bone._IKAngleRange_Lower;
			_IKAngleRange_Upper = bone._IKAngleRange_Upper;
			_IKAnglePreferred = bone._IKAnglePreferred;

			//이게 변경된 IK 코드
			//_IKAngleRange_Lower = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower;
			//_IKAngleRange_Upper = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper;
			//_IKAnglePreferred = bone._defaultMatrix._angleDeg + bone._IKAnglePreferred;


			_isIKCalculated = false;
			_IKRequestAngleResult_World = 0.0f;
			_IKRequestAngleResult_Delta = 0.0f;
			_IKRequestWeight = 0.0f;

			_socketTransform = null;

			_transformLocalMatrix.SetIdentity();

			_childBones = null;

			_isIKChainInit = false;

			//IKController
			if(_IKController == null)
			{
				_IKController = new apOptBoneIKController();
			}
			_IKController.Bake(this, 
				bone._IKController._effectorBoneID, 
				bone._IKController._controllerType, 
				bone._IKController._defaultMixWeight,
				bone._IKController._isWeightByControlParam,
				bone._IKController._weightControlParamID);
		}


		/// <summary>
		///	추가 20.8.30 : 런타임 초기화 함수. WorldMatrix를 만드는 등의 초기화 코드가 동작한다.
		/// </summary>
		public void Initialize(apPortrait parentPortrait)
		{
			InitWorldMatrix();

			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;
			//_isExternalUpdate_IK = false;

			_isExternalConstraint = false;
			_isExternalConstraint_Xmin = false;
			_isExternalConstraint_Xmax = false;
			_isExternalConstraint_Ymin = false;
			_isExternalConstraint_Ymax = false;
			_isExternalConstraint_Xpref = false;
			_isExternalConstraint_Ypref = false;
			_isExternalConstraint_Xsurface = false;
			_isExternalConstraint_Ysurface = false;

			if(_isJiggle 
				&& _jiggle_IsControlParamWeight
				&& _jiggle_WeightControlParamID >= 0)
			{
				_linkedJiggleControlParam = parentPortrait._controller.FindParam(_jiggle_WeightControlParamID);
			}
			else
			{
				_linkedJiggleControlParam = null;
			}

			//지글본의 가중치를 계산해야한는가
			_isJiggleWeightCalculatable = false;
			if(_isJiggle)
			{
				if(_jiggle_IsControlParamWeight)
				{
					if(_linkedJiggleControlParam != null 
						&& _linkedJiggleControlParam._valueType == apControlParam.TYPE.Float)
					{
						//유효한 컨트롤 파라미터를 가진다면 가중치 연산 필요
						_isJiggleWeightCalculatable = true;
					}
				}
			}

			_calJig_IsZeroWeight = false;
			_calJig_IsZeroWeight_Prev = false;
		}



		/// <summary>
		/// 추가 20.8.30 : 초기화 단계에서 WorldMatrix를 생성한다.
		/// </summary>
		private void InitWorldMatrix()
		{
			apPortrait.ROOT_BONE_SCALE_METHOD scaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;
			if(_parentOptTransform != null)
			{
				scaleMethod = _parentOptTransform._portrait._rootBoneScaleMethod;
			}

			//World Matrix를 만들자.
			bool isNeedToMakeWorldMatrix = false;
			if(_worldMatrix == null  || _worldMatrix_NonModified == null)												{ isNeedToMakeWorldMatrix = true; }
			else if(_worldMatrix.ScaleMethod != scaleMethod  || _worldMatrix_NonModified.ScaleMethod != scaleMethod)	{ isNeedToMakeWorldMatrix = true; }

			if(isNeedToMakeWorldMatrix)
			{
				if(scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
				{
					//<기존 방식>
					_worldMatrix = new apOptBoneWorldMatrix_Default(this);
					_worldMatrix_NonModified = new apOptBoneWorldMatrix_Default(this);

					//Debug.LogWarning("[Init World Matrix] Init WM <Default> (" + this.gameObject.name + ")");
				}
				else
				{
					//<Skew Scale 방식>
					_worldMatrix = new apOptBoneWorldMatrix_Skew(this);
					_worldMatrix_NonModified = new apOptBoneWorldMatrix_Skew(this);

					//Debug.LogWarning("[Init World Matrix] Init WM <Skew> (" + this.gameObject.name + ")");
				}	
			}
			

			_rootBoneScaleMethod = scaleMethod;//IK를 위한 변수


			//지글본을 위한 행렬을 만들자
			if(_isJiggle)
			{
				bool isNeedToMakeJiggleMatrix = false;
				if(_calJig_Tmp_WorldMatrix == null)							{ isNeedToMakeJiggleMatrix = true; }
				else if(_calJig_Tmp_WorldMatrix.ScaleMethod != scaleMethod)	{ isNeedToMakeJiggleMatrix = true; }

				if(isNeedToMakeJiggleMatrix || isNeedToMakeWorldMatrix)
				{	
					if(scaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
					{
						//<기존 방식>
						_calJig_Tmp_WorldMatrix = new apOptBoneWorldMatrix_Default(null);
					}
					else
					{
						//<Skew Scale 방식>
						_calJig_Tmp_WorldMatrix = new apOptBoneWorldMatrix_Skew(null);
					}
				}
			}
			else
			{
				_calJig_Tmp_WorldMatrix = null;
			}
			
		}


		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="targetOptTransform"></param>
		public void LinkOnBake(apOptTransform targetOptTransform)
		{
			_parentOptTransform = targetOptTransform;
			if (_parentOptTransform == null)
			{
				//??
				Debug.LogError("[" + transform.name + "] ParentOptTransform of OptBone is Null [" + _meshGroupID + "]");
				_IKTargetBone = null;
				_IKNextChainedBone = null;
				_IKHeaderBone = null;

				//LinkBoneChaining();


				return;
			}


			_IKTargetBone = _parentOptTransform.GetBone(_IKTargetBoneID);
			_IKNextChainedBone = _parentOptTransform.GetBone(_IKNextChainedBoneID);
			_IKHeaderBone = _parentOptTransform.GetBone(_IKHeaderBoneID);

			//LinkBoneChaining();

			//추가 : EffectorBone을 연결한다.
			if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
				&& _IKController._effectorBoneID >= 0)
			{
				_IKController.LinkEffector(targetOptTransform.GetBone(_IKController._effectorBoneID));
			}
			

			InitWorldMatrix();//<<추가 : WorldMatrix를 직접 초기화해야한다.
		}



		//여기서는 LinkBoneChaining만 진행
		// Bone Chaining 직후에 재귀적으로 호출한다.
		// Tail이 가지는 -> Head로의 IK 리스트를 만든다.
		/// <summary>
		/// [Please do not use it] IK Link
		/// </summary>
		public void LinkBoneChaining()
		{
			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}

			//이전
			//if (_worldMatrix == null)
			//{
			//	_worldMatrix = new apMatrix();
			//}
			//if (_worldMatrix_NonModified == null)
			//{
			//	_worldMatrix_NonModified = new apMatrix();
			//}

			//변경 20.8.30 : BoneWorldMatrix 생성하기. 매번 만든다.
			InitWorldMatrix();
			


			if (_isIKTail)
			{
				apOptBone curParentBone = _parentBone;
				apOptBone headBone = _IKHeaderBone;

				bool isParentExist = (curParentBone != null);
				bool isHeaderExist = (headBone != null);
				bool isHeaderIsInParents = false;
				if (isParentExist && isHeaderExist)
				{
					isHeaderIsInParents = (GetParentRecursive(headBone._uniqueID) != null);
				}


				if (isParentExist && isHeaderExist && isHeaderIsInParents)
				{
					if (_IKChainSet == null)
					{
						_IKChainSet = new apOptBoneIKChainSet(this);
					}
					_isIKChainSetAvailable = true;
					//Chain을 Refresh한다.
					//_IKChainSet.RefreshChain();//<<수정. 이건 Runtime에 해야한다.
				}
				else
				{
					_IKChainSet = null;

					Debug.LogError("[" + transform.name + "] IK Chaining Error : Parent -> Chain List Connection Error "
						+ "[ Parent : " + isParentExist
						+ " / Header : " + isHeaderExist
						+ " / IsHeader Is In Parent : " + isHeaderIsInParents + " ]");
					_isIKChainSetAvailable = false;
				}
			}
			else
			{
				_IKChainSet = null;
				_isIKChainSetAvailable = false;
			}

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].LinkBoneChaining();
				}
			}

		}


		// Update
		//---------------------------------------------------------------
		// Update Transform Matrix를 초기화한다.
		public void ReadyToUpdate(bool isRecursive)
		{
			//_localModifiedTransformMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			//_isIKCalculated = false;
			//_IKRequestAngleResult = 0.0f;

			//추가 : Bone IK
			_isIKCalculated_Controlled = false;
			_IKRequestAngleResult_Controlled = 0.0f;
			_IKRequestWeight_Controlled = 0.0f;
			//_isIKRendered_Controller = false;

			_calculatedBoneIKWeight = 0.0f;//<<추가

			if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
			{
				_calculatedBoneIKWeight = _IKController._defaultMixWeight;

				if (!_isIKChainInit)
				{
					InitIKChain();
					_isIKChainInit = true;
				}
			}

			//_worldMatrix.SetIdentity();
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].ReadyToUpdate(true);
				}
			}
		}

		/// <summary>
		/// Bake를 위해서 BoneMatrix를 초기화한다.
		/// </summary>
		/// <param name="isRecursive"></param>
		public void ResetBoneMatrixForBake(bool isRecursive)
		{
			
			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_localMatrix.SetIdentity();
			_worldMatrix.SetIdentity();

			_worldMatrix_NonModified.SetIdentity();
			_vertWorld2BoneModWorldMatrix.SetIdentity();

			
			//=============================================
			// 이전 방식
			//=============================================
			//if (_parentBone == null)
			//{
			//	_worldMatrix.SetMatrix(_defaultMatrix, false);
			//	_worldMatrix.Add(_localMatrix);

			//	_worldMatrix_NonModified.SetMatrix(_defaultMatrix, false);//Local Matrix 없이 Default만 지정

			//	//추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//	_worldMatrix.OnBeforeRMultiply();
			//	_worldMatrix_NonModified.OnBeforeRMultiply();

			//	if (_parentOptTransform != null)
			//	{
			//		//Debug.Log("SetParentOptTransform Matrix : [" + _parentOptTransform.transform.name + "] : " + _parentOptTransform._matrix_TFResult_World.Scale2);
			//		//Non Modified도 동일하게 적용
			//		//렌더유닛의 WorldMatrix를 넣어주자
			//		_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod, false);//RenderUnit의 WorldMatrixWrap의 Opt 버전
			//		_worldMatrix_NonModified.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod, false);

			//	}
			//}
			//else
			//{
			//	_worldMatrix.SetMatrix(_defaultMatrix, false);
			//	_worldMatrix.Add(_localMatrix);

			//	_worldMatrix_NonModified.SetMatrix(_defaultMatrix, false);//Local Matrix 없이 Default만 지정

			//	//추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//	_worldMatrix.OnBeforeRMultiply();
			//	_worldMatrix_NonModified.OnBeforeRMultiply();

			//	_worldMatrix.RMultiply(_parentBone._worldMatrix_NonModified, false);
			//	_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified, false);
			//}

			//_worldMatrix.SetMatrix(_worldMatrix_NonModified, false);//Bake에선 리셋..인듯하다.(까먹음)

			////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//_worldMatrix.OnBeforeRMultiply();

			//_worldMatrix.MakeMatrix();
			//=============================================

			//=============================================
			//변경 20.8.30 : BoneWorldMatrix를 이용한 코드
			//=============================================
			_worldMatrix.MakeWorldMatrix_Mod(	_localMatrix, 
												(_parentBone != null ? _parentBone._worldMatrix : null),
												(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_WorldWithoutMod : null)
												);

			_worldMatrix_NonModified.MakeWorldMatrix_NoMod(
												(_parentBone != null ? _parentBone._worldMatrix_NonModified : null),
												(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_WorldWithoutMod : null)
												);
			//=============================================

			_vertWorld2BoneModWorldMatrix = _worldMatrix_NonModified.MtrxToSpace;
			_vertWorld2BoneModWorldMatrix *= _worldMatrix_NonModified.MtrxToLowerSpace;


			
			//Debug.Log("Reset Bone Matrix [" + this.name + "]");
			//Debug.Log("World Matrix [ " + _worldMatrix.ToString() + "]");

			if (isRecursive)
			{
				if (_childBones != null && _childBones.Length > 0)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						_childBones[i].ResetBoneMatrixForBake(true);
					}
				}
			}
			
		}


		// 2) Update된 TRS 값을 넣는다.
		public void UpdateModifiedValue(Vector2 deltaPos, float deltaAngle, Vector2 deltaScale)
		{
			_deltaPos = deltaPos;
			_deltaAngle = deltaAngle;
			_deltaScale = deltaScale;
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="IKAngle"></param>
		/// <param name="weight"></param>
		public void AddIKAngle(float IKAngle, float IKAngleDelta, float weight)
		{
			_isIKCalculated = true;
			_IKRequestWeight = weight;
			_IKRequestAngleResult_World += IKAngle;
			_IKRequestAngleResult_Delta += IKAngleDelta;
		}


		public void AddIKAngle_Controlled(float IKAngle, float weight)
		{
			_isIKCalculated_Controlled = true;
			_IKRequestAngleResult_Controlled += (IKAngle) * weight;
			_IKRequestWeight_Controlled += weight;
		}


		// 4) World Matrix를 만든다.
		// 이 함수는 Parent의 MeshGroupTransform이 연산된 후 -> Vertex가 연산되기 전에 호출되어야 한다.
		public void MakeWorldMatrix(bool isRecursive)
		{
			_localMatrix.SetIdentity();
			_localMatrix._pos = _deltaPos;
			_localMatrix._angleDeg = _deltaAngle;
			_localMatrix._scale.x = _deltaScale.x;
			_localMatrix._scale.y = _deltaScale.y;

			_localMatrix.MakeMatrix();

			//World Matrix = ParentMatrix x LocalMatrix
			//Root인 경우에는 MeshGroup의 Matrix를 이용하자

			//_invWorldMatrix_NonModified.SetIdentity();

			//================================================
			// 이전 방식
			//================================================
			//if (_parentBone == null)
			//{
			//	_worldMatrix.SetMatrix(_defaultMatrix, false);
			//	_worldMatrix.Add(_localMatrix);

			//	_worldMatrix_NonModified.SetMatrix(_defaultMatrix, false);//Local Matrix 없이 Default만 지정

			//	//추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//	_worldMatrix.OnBeforeRMultiply();
			//	_worldMatrix_NonModified.OnBeforeRMultiply();

			//	if (_parentOptTransform != null)
			//	{
			//		//Debug.Log("SetParentOptTransform Matrix : [" + _parentOptTransform.transform.name + "] : " + _parentOptTransform._matrix_TFResult_World.Scale2);
			//		//Non Modified도 동일하게 적용
			//		//렌더유닛의 WorldMatrix를 넣어주자
			//		_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World, false);//RenderUnit의 WorldMatrixWrap의 Opt 버전
			//		_worldMatrix_NonModified.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod, false);

			//	}
			//}
			//else
			//{
			//	_worldMatrix.SetMatrix(_defaultMatrix, false);
			//	_worldMatrix.Add(_localMatrix);
			//	_worldMatrix_NonModified.SetMatrix(_defaultMatrix, false);//Local Matrix 없이 Default만 지정

			//	//추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
			//	_worldMatrix.OnBeforeRMultiply();
			//	_worldMatrix_NonModified.OnBeforeRMultiply();

			//	_worldMatrix.RMultiply(_parentBone._worldMatrix, false);
			//	_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified, false);
			//}

			//_worldMatrix.MakeMatrix();
			//_worldMatrix_NonModified.MakeMatrix();


			//================================================
			// 변경 20.8.30 : BoneWorldMatrix를 이용한 코드
			//================================================
			_worldMatrix.MakeWorldMatrix_Mod(
											_localMatrix, 
											(_parentBone != null ? _parentBone._worldMatrix : null),
											(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_World : null)
											);

			_worldMatrix_NonModified.MakeWorldMatrix_NoMod(
											(_parentBone != null ? _parentBone._worldMatrix_NonModified : null),
											(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_WorldWithoutMod : null)
											);
			//================================================



			//추가 : 외부 변수들은 이 함수에서 처리한다.
			UpdateExternalRequest();
			

			//World Matrix는 MeshGroup과 동일한 Space의 값을 가진다.
			//그러나 실제로 Bone World Matrix는
			//Root - MeshGroup...(Rec) - Bone Group - Bone.. (Rec <- 여기)
			//의 레벨을 가진다.
			//Root 밑으로는 모두 World에 대해서 동일한 Space를 가지므로
			//Root를 찾아서 Scale을 제어하자...?
			//일단 Parent에서 빼두자
			//_transformLocalMatrix.SetMatrix(_worldMatrix);

			//>>UpdatePostRecursive() 함수에서 나중에 일괄적으로 갱신한다.
			
			//Child도 호출해준다.
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].MakeWorldMatrix(true);
				}
			}
		}


		/// <summary>
		/// 본이 동기화된 경우에 호출되는 MakeWorldMatrix 함수. 코드 일부가 다르다.
		/// </summary>
		/// <param name="isRecursive"></param>
		public void MakeWorldMatrixAsSyncBones(bool isRecursive)
		{
			if(_syncBone == null)
			{
				//[Sync] 동기화된 본이 없는 경우에만 연산하기

				_localMatrix.SetIdentity();
				_localMatrix._pos = _deltaPos;
				_localMatrix._angleDeg = _deltaAngle;
				_localMatrix._scale.x = _deltaScale.x;
				_localMatrix._scale.y = _deltaScale.y;

				_localMatrix.MakeMatrix();

			

				//================================================
				// 변경 20.8.30 : BoneWorldMatrix를 이용한 코드
				//================================================

				//[Sync] Parent가 Sync된 것인지도 파악해야한다.

				_worldMatrix.MakeWorldMatrix_Mod(
												_localMatrix, 
												//(_parentBone != null ? _parentBone._worldMatrix : null),
												(_parentBone != null ? (_parentBone._syncBone != null ? _parentBone._syncBone._worldMatrix : _parentBone._worldMatrix) : null),//[Sync]
												(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_World : null)
												);

				_worldMatrix_NonModified.MakeWorldMatrix_NoMod(
												//(_parentBone != null ? _parentBone._worldMatrix_NonModified : null),
												(_parentBone != null ? (_parentBone._syncBone != null ? _parentBone._syncBone._worldMatrix_NonModified : _parentBone._worldMatrix_NonModified) : null),//[Sync]
												(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_WorldWithoutMod : null)
												);
				//================================================

				//추가 : 외부 변수들은 이 함수에서 처리한다.
				UpdateExternalRequest();
			}
			
			
			//Child도 호출해준다.
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].MakeWorldMatrixAsSyncBones(true);
				}
			}
		}






		/// <summary>
		/// IK Controlled가 있는 경우 IK를 계산한다.
		/// Child 중 하나라도 계산이 되었다면 True를 리턴한다.
		/// 계산 자체는 IK Controller가 활성화된 경우에 한한다. (Chain되어서 처리가 된다.)
		/// </summary>
		/// <param name="isRecursive"></param>
		public bool CalculateIK(bool isRecursive)
		{
			bool IKCalculated = false;

			if (_IKChainSet != null && _isIKChainSetAvailable)
			{

				//Control Param의 영향을 받는다.
				if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
					&& _IKController._isWeightByControlParam
					&& _IKController._weightControlParam != null)
				{
					_calculatedBoneIKWeight = Mathf.Clamp01(_IKController._weightControlParam._float_Cur);
				}


				if (_calculatedBoneIKWeight > 0.001f
					&& _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
					)
				{

					if (_IKController._controllerType == apOptBoneIKController.CONTROLLER_TYPE.Position)
					{
						//1. Position 타입일 때
						if (_IKController._effectorBone != null)
						{
							//bool result = _IKChainSet.SimulateIK(_IKController._effectorBone._worldMatrix._pos, true, true);//<<논리상 EffectorBone은 IK의 영향을 받으면 안된다.

							//bool result = _IKChainSet.SimulateIK(_IKController._effectorBone._worldMatrix._pos, true);
							bool result = _IKChainSet.SimulateIK(_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos), true);//변경 20.8.31 : IK Space의 위치
							
							if (result)
							{
								IKCalculated = true;
								_IKChainSet.AdaptIKResultToBones_ByController(_calculatedBoneIKWeight);
							}
						}
					}
					else
					{
						//2. LookAt 타입일 때
						if (_IKController._effectorBone != null
							)
						{
							//기존 : 일반적으로는 문제 없는 코드
							//bool result = _IKChainSet.SimulateLookAtIK(_IKController._effectorBone._worldMatrix_NonModified._pos, _IKController._effectorBone._worldMatrix._pos, true);

							//변경 20.8.6 [Scale 이슈 해결]
							//만약 Effector 본이 "모디파이어나 상위 객체에 의해서" 반전된 경우에는 위치가 반전되어야 한다.
							//이걸 포함해서 LookAt의 기본이 되는 Default Pos를 계산하는 수식을 보강했다.
							//- 만약 어딘가에서 "스케일이 반전된 경우", Modified가 적용 안된 Effector 본의 위치가 적절하지 않을 수 있다.
							//- 따라서 약간 변경해서, "현재 본"을 기준으로 "Modified가 적용 안된 상태에서의 상대 위치값"을 "Modified가 적용된 Matrix를 기준으로 적용하여 World좌표계의 위치"를 구한다.
							
							//기존 방식
							//Vector2 defaultEffectorPos = _worldMatrix.MulPoint2(_worldMatrix_NonModified.InvMulPoint2(_IKController._effectorBone._worldMatrix_NonModified._pos));
							//bool result = _IKChainSet.SimulateLookAtIK(defaultEffectorPos, _IKController._effectorBone._worldMatrix._pos, true);

							//변경 20.8.31 : 래핑 + IK Space
							Vector2 defaultEffectorPos = _worldMatrix.MulPoint2(_worldMatrix_NonModified.InvMulPoint2(_IKController._effectorBone._worldMatrix_NonModified.Pos));
							bool result = _IKChainSet.SimulateLookAtIK(_worldMatrix.ConvertForIK(defaultEffectorPos), 
																		_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos), 
																		true);//IK Space로 이동

							if (result)
							{
								IKCalculated = true;
								_IKChainSet.AdaptIKResultToBones_ByController(_calculatedBoneIKWeight);

								//이 Tail Bone은 그냥 바라보도록 한다.
								//기존
								//AddIKAngle_Controlled(
								//	(apBoneIKChainUnit.Vector2Angle(_IKController._effectorBone._worldMatrix._pos - _IKChainSet._tailBoneNextPosW) - 90)
								//	- _worldMatrix._angleDeg
								//	//- apBoneIKChainUnit.Vector2Angle(_IKController._effectorBone._worldMatrix_NonModified._pos - _worldMatrix._pos)
								//	,
								//	_calculatedBoneIKWeight
								//	);

								//변경 20.8.8 : 마지막으로 Tail본에 LookAt을 적용할 때,
								//만약 Y축으로 반전되어 있다면 AngleDeg를 반전해야한다.
								//float tailLookAngleFromVector = apBoneIKChainUnit.Vector2Angle(_IKController._effectorBone._worldMatrix._pos - _IKChainSet._tailBoneNextPosW);

								//if(_worldMatrix._scale.y < 0.0f)
								//{
								//	//각도 반전
								//	tailLookAngleFromVector += 90.0f;
								//}
								//else
								//{
								//	tailLookAngleFromVector -= 90.0f;
								//}


								//AddIKAngle_Controlled(	apUtil.AngleTo180(tailLookAngleFromVector - _worldMatrix._angleDeg),
								//						_calculatedBoneIKWeight
								//						);




								//다시 변경 20.8.31 : IKSpace로 이동 + 래핑
								float tailLookAngleFromVector = apBoneIKChainUnit.Vector2Angle(
																	_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos) - _IKChainSet._tailBoneNextPosW);

								if (_worldMatrix.Scale.y < 0.0f)	{ tailLookAngleFromVector += 90.0f; }//각도 반전
								else								{ tailLookAngleFromVector -= 90.0f; }


								AddIKAngle_Controlled(	apUtil.AngleTo180(tailLookAngleFromVector - _worldMatrix.Angle_IKSpace), _calculatedBoneIKWeight );
							}
						}
					}
				}
			}
			else
			{
				//추가 19.8.16 : 만약 IK ChainSet이 없거나 비활성화 될 때, IK Controller가 LookAt이라면
				//단일 본에 대해서도 LookAt을 처리할 수 있어야 한다.
				if(_IKController != null && _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
				{
					//Debug.LogError("IK Controller Not Work : [" + _IKController._controllerType + "]");
					if(_IKController._controllerType == apOptBoneIKController.CONTROLLER_TYPE.LookAt)
					{
						//LookAt IK에 한해서 단일 본에서도 IK가 처리될 수 있다.
						if(CalculateSingleLookAtIK())
						{
							IKCalculated = true;
						}
					}
				}
			}

			//자식 본도 업데이트
			if(isRecursive)
			{
				if (_childBones != null)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						if (_childBones[i].CalculateIK(true))
						{
							//자식 본 중에 처리 결과가 True라면
							//나중에 전체 True 처리
							IKCalculated = true;
						}
					}
				}
			}

			return IKCalculated;
		}




		/// <summary>
		/// CalculateIK의 동기화 버전. 동기화되지 않은 본에 대해서만 IK를 계산한다.
		/// </summary>
		/// <param name="isRecursive"></param>
		/// <returns></returns>
		public bool CalculateIKAsSyncBones(bool isRecursive)
		{
			bool IKCalculated = false;

			if (_syncBone == null)
			{
				//동기화 되지 않은 경우만 처리
				if (_IKChainSet != null && _isIKChainSetAvailable)
				{
					//Control Param의 영향을 받는다.
					if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
						&& _IKController._isWeightByControlParam
						&& _IKController._weightControlParam != null)
					{
						_calculatedBoneIKWeight = Mathf.Clamp01(_IKController._weightControlParam._float_Cur);
					}

					if (_calculatedBoneIKWeight > 0.001f
						&& _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
						)
					{

						if (_IKController._controllerType == apOptBoneIKController.CONTROLLER_TYPE.Position)
						{
							//1. Position 타입일 때
							if (_IKController._effectorBone != null)
							{
								bool result = _IKChainSet.SimulateIK(_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos), true);//변경 20.8.31 : IK Space의 위치

								if (result)
								{
									IKCalculated = true;
									_IKChainSet.AdaptIKResultToBones_ByController(_calculatedBoneIKWeight);
								}
							}
						}
						else
						{
							//2. LookAt 타입일 때
							if (_IKController._effectorBone != null
								)
							{
								Vector2 defaultEffectorPos = _worldMatrix.MulPoint2(_worldMatrix_NonModified.InvMulPoint2(_IKController._effectorBone._worldMatrix_NonModified.Pos));
								bool result = _IKChainSet.SimulateLookAtIK(_worldMatrix.ConvertForIK(defaultEffectorPos),
																			_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos),
																			true);//IK Space로 이동

								if (result)
								{
									IKCalculated = true;
									_IKChainSet.AdaptIKResultToBones_ByController(_calculatedBoneIKWeight);

									//다시 변경 20.8.31 : IKSpace로 이동 + 래핑
									float tailLookAngleFromVector = apBoneIKChainUnit.Vector2Angle(
																		_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos) - _IKChainSet._tailBoneNextPosW);

									if (_worldMatrix.Scale.y < 0.0f)
									{ tailLookAngleFromVector += 90.0f; }//각도 반전
									else
									{ tailLookAngleFromVector -= 90.0f; }


									AddIKAngle_Controlled(apUtil.AngleTo180(tailLookAngleFromVector - _worldMatrix.Angle_IKSpace), _calculatedBoneIKWeight);
								}
							}
						}
					}
				}
				else
				{
					//추가 19.8.16 : 만약 IK ChainSet이 없거나 비활성화 될 때, IK Controller가 LookAt이라면
					//단일 본에 대해서도 LookAt을 처리할 수 있어야 한다.
					if (_IKController != null && _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
					{
						//Debug.LogError("IK Controller Not Work : [" + _IKController._controllerType + "]");
						if (_IKController._controllerType == apOptBoneIKController.CONTROLLER_TYPE.LookAt)
						{
							//LookAt IK에 한해서 단일 본에서도 IK가 처리될 수 있다.
							if (CalculateSingleLookAtIK())
							{
								IKCalculated = true;
							}
						}
					}
				}
			}
			

			//자식 본도 업데이트
			if(isRecursive)
			{
				if (_childBones != null)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						if (_childBones[i].CalculateIKAsSyncBones(true))
						{
							//자식 본 중에 처리 결과가 True라면
							//나중에 전체 True 처리
							IKCalculated = true;
						}
					}
				}
			}

			return IKCalculated;
		}





		/// <summary>
		/// IK가 포함된 WorldMatrix를 계산하는 함수
		/// 렌더링 직전에만 따로 수행한다.
		/// IK Controller에 의한 IK 연산이 있다면 이 함수에서 계산 및 WorldMatrix
		/// IK용 GUI 업데이트도 동시에 실행된다.
		/// </summary>
		public void MakeWorldMatrixForIK(bool isRecursive, bool isCalculateMatrixForce, bool isPhysics, bool isTeleportFrame, float tDelta)
		{
			if(_isIKCalculated_Controlled)
			{
				//IK가 계산된 결과를 넣자
				//World Matrix 재계산


				//--------------------------
				// 기존 코드
				//--------------------------
				#region [미사용 코드] 이전 방식
				//float prevWorldMatrixAngle = _worldMatrix._angleDeg;

				//_worldMatrix.SetMatrix(_defaultMatrix, false);
				//_worldMatrix.Add(_localMatrix);

				////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
				//_worldMatrix.OnBeforeRMultiply();

				//if (_parentBone == null)
				//{
				//	if (_parentOptTransform != null)
				//	{
				//		_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World, false);
				//	}
				//}
				//else
				//{
				//	_worldMatrix.RMultiply(_parentBone._worldMatrix, false);
				//}

				//_worldMatrix.MakeMatrix();

				////여기서도 External Request를 적용해준다.
				//UpdateExternalRequest();


				////IK 적용

				////추가 20.8.6 : [RMultiply Scale 이슈]
				////플립된 상태라면, 실제 Angle은 반전되어야 한다.
				//float addIKAngle = (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled);
				//float nextIKAngle = 0.0f;

				//if (_IKRequestWeight_Controlled > 1.0f)
				//{
				//	//_worldMatrix.SetRotate(prevWorldMatrixAngle + (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled));
				//	nextIKAngle = prevWorldMatrixAngle + addIKAngle;
				//}
				//else if (_IKRequestWeight_Controlled > 0.0f)
				//{
				//	//Slerp가 적용된 코드
				//	nextIKAngle = apUtil.AngleSlerp(	prevWorldMatrixAngle,
				//										prevWorldMatrixAngle + addIKAngle,
				//										_IKRequestWeight_Controlled);
				//}

				//_worldMatrix.SetRotate(nextIKAngle, true); 
				#endregion

				//--------------------------
				// 변경된 코드 20.8.31 : 래핑 + Skew + IKSpace
				//--------------------------

				float prevWorldMatrixAngle = _worldMatrix.Angle_IKSpace;
				float addIKAngle = (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled);
				
				float nextIKAngle = 0.0f;

				if (_IKRequestWeight_Controlled > 1.0f)
				{
					nextIKAngle = prevWorldMatrixAngle + addIKAngle;
				}
				else if (_IKRequestWeight_Controlled > 0.0f)
				{
					//Slerp가 적용된 코드
					nextIKAngle = apUtil.AngleSlerp(	prevWorldMatrixAngle,
														prevWorldMatrixAngle + addIKAngle,
														_IKRequestWeight_Controlled);
				}

				//IK 적용
				//중요 : 래핑된 코드
				_worldMatrix.MakeWorldMatrix_IK(	_localMatrix,
													(_parentBone != null ? _parentBone._worldMatrix : null),
													(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_World : null),
													nextIKAngle);


				//여기서도 External Request를 적용해준다.
				UpdateExternalRequest();//<Opt 코드

				//--------------------------

				isCalculateMatrixForce = true;//<<다음 Child 부터는 무조건 갱신을 해야한다.
			}
			else if(isCalculateMatrixForce)
			{
				//Debug.Log("IK Force [" + _name + "] : " + _IKRequestAngleResult_Controlled);

				//----------------------------------
				// 기존 방식
				//----------------------------------
				#region [미사용 코드] 기존 방식
				////IK 자체는 적용되지 않았으나, Parent에서 적용된게 있어서 WorldMatrix를 그대로 쓸 순 없다.
				//_worldMatrix.SetMatrix(_defaultMatrix, false);
				//_worldMatrix.Add(_localMatrix);


				////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
				//_worldMatrix.OnBeforeRMultiply();


				//if (_parentBone == null)
				//{
				//	if (_parentOptTransform != null)
				//	{
				//		_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World, false);
				//	}
				//}
				//else
				//{
				//	_worldMatrix.RMultiply(_parentBone._worldMatrix, false);
				//}

				//_worldMatrix.MakeMatrix(); 
				#endregion


				//----------------------------------
				// 변경된 방식 20.8.31 : 래핑
				//----------------------------------
				//IK 자체는 적용되지 않았으나, Parent에서 적용된게 있어서 WorldMatrix를 그대로 쓸 순 없다.
				_worldMatrix.MakeWorldMatrix_Mod(_localMatrix, 
													(_parentBone != null ? _parentBone._worldMatrix : null),
													(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_World : null));

				//----------------------------------

				//_isIKRendered_Controller = true;//<에디터 코드
			}
			//else
			//{
			//	//World Matrix와 동일하다.
			//	//생략
			//	//_worldMatrix_IK.SetMatrix(_worldMatrix);//<<동일하다.
			//}


			//추가 20.5.23 : 지글본이 계산된 경우 WorldMatrix_IK를 변경하자
			//추가 20.5.23 : 지글본
			//지글 본은 계층 처리가 없다.
			//헬퍼는 지글본일 수가 없다.
			//길이가 1 이상이어야 한다.
			
			if(_isJiggle && isPhysics)
			{
				//추가 22.7.6 : 계산 전, 가중치 적용 여부와 적용 정도를 계산한다.
				//1. Damping
				//2. 회전 범위 제한
				//3. 실제 적용 정도
				_calJig_IsWeight = false;
				_calJig_IsZeroWeight = false;
				if(_isJiggleWeightCalculatable)
				{
					if(_jiggle_IsControlParamWeight
						&& _linkedJiggleControlParam != null)
					{
						//컨트롤 파라미터인 경우 (Float타입 한정)
						if(_linkedJiggleControlParam._float_Cur < JIG_WEIGHT_CUTOUT_1)
						{
							_calJig_IsWeight = true;
							_calJig_Weight = Mathf.Clamp01(_linkedJiggleControlParam._float_Cur);
						}
					}

					if (_calJig_IsWeight &&
						_calJig_Weight < JIG_WEIGHT_CUTOUT_0)
					{
						//가중치가 너무 적어서 지글본 처리가 중단된다.
						//단, 이전 프레임에서도 ZeroWeight였어야 무시되며, 그렇지 않다면 일단 적용은 한다.
						_calJig_IsZeroWeight = true;
					}
				}


				//Debug.Log("[" + _name + "] Jiggle Update (" + tDelta + ")");
				if (!_isJiggleChecked_Prev)
				{
					//Prev 기록이 없다면, 초기화
					_calJig_Velocity = 0.0f;
					_calJig_Angle_Result_Prev = 0.0f;
					_calJig_EndPos_Prev = Vector2.zero;
					_calJig_EndPosW_Prev = Vector3.zero;

					//변경 22.7.6
					isCalculateMatrixForce = true;//지글본 적용되었으니 하위 본의 매트릭스 갱신해야함
				}
				else if(_calJig_IsZeroWeight && _calJig_IsZeroWeight_Prev)
				{
					//가중치가 0이라면 이번 프레임에서 지글본은 동작하지 않는다. (이전프레임에서도 가중치가 0이어야 함)
					_calJig_Velocity = 0.0f;
					_calJig_Angle_Result_Prev = 0.0f;
					_calJig_EndPos_Prev = Vector2.zero;
					_calJig_EndPosW_Prev = Vector3.zero;

					//>> 여기서는 지글본이 적용되지 않아서 자식 본들의 매트릭스 갱신 안함
					//Debug.Log("지글본 업데이트 안함");
				}
				else
				{
					//지글본에 따라서 WorldMatrix_IK를 수정하자
					float calJig_Angle_Result = _calJig_Angle_Result_Prev;

					
					//TODO : 빌보드 방식에서의 지글본을 제대로 연산해야한다.

					if (tDelta > 0.0f 
						&& !isTeleportFrame//추가 22.7.7 : 텔레포트 프레임이 아닐때라는 조건이 덧붙여진다.
						)
					{
						//계산은 tDelta가 0 유효할 때에만


						//계산 순서
						//1. Drag를 기준으로 dAngle_FromPrev 만들기 (방향은 > Prev)
						//2. dAngle_woJiggle 바탕으로 복원력(-kx)을 계산하고 속력에 더하기 (방향은 > Cur)
						//3. 속력의 Drag를 더해서 감속하기
						//4. dAngle_woJiggle + (Vt) 결과가 최종 dAngle_Cur


						//1. Drag를 기준으로 dAngle_FromPrev 만들기 (방향은 > Prev)
						//- 현재의 Matrix + 이전 프레임의 dAngle를 더한 값으로 [Expected Pos]를 계산한다.
						//- Prev Pos > Expected Pos가 예상 움직임 내역
						//- 각도를 계산하고, Drag를 곱해서 변화량을 줄이자 (dAngle_Drag)

						//-----------------------------------------
						// 이전 방식
						//-----------------------------------------
						#region [미사용 코드]
						//s_calJig_Tmp_WorldMatrix.SetMatrix(_worldMatrix, false);

						////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
						//s_calJig_Tmp_WorldMatrix.OnBeforeRMultiply();


						//s_calJig_Tmp_WorldMatrix._angleDeg += _calJig_Angle_Result_Prev;
						//s_calJig_Tmp_WorldMatrix.MakeMatrix(); 
						#endregion


						//-----------------------------------------
						// 변경된 방식 20.8.31 : 래핑된 코드
						//-----------------------------------------
						_calJig_Tmp_WorldMatrix.CopyFromMatrix(_worldMatrix);
						_calJig_Tmp_WorldMatrix.RotateAsStep1(_calJig_Angle_Result_Prev, true);

						//-----------------------------------------

						//예상 위치
						Vector2 endPos_Excepted = _calJig_Tmp_WorldMatrix.MulPoint2(new Vector2(0, _shapeLength));


						//[Opt 코드] 좌표계 전환
						//기존에 저장된 값 : Unity World 좌표계.
						//이걸 Portrait 좌표계로 바꿔서 테스트하자
						//외부 움직임을 인식하기 위함
						if(_parentOptTransform._portrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
						{
							//추가 20.9.15
							//빌보드 타입이라면
							//World 좌표계가 그대로 저장된게 아니라,
							//카메라 좌표계 (프로젝션 아님)에서의 RootUnit 중심과의 위치 오프셋 (Vec2)로서 저장되어 있다.
							//이 오프셋부터 해결하여 World Position으로 옮겨야 한다.

							//상대 좌표 > Offset으로 변환 (이 값은 정사영과 같으므로 z값이 0이여야 한다. 이상하면 디버그할 것)
							//<중요> 이게 메인 식
							//Matrix4x4 camMatrix = Matrix4x4.TRS(Vector3.zero, _parentOptTransform._portrait._transform.rotation, Vector3.one);
							//_calJig_EndPosW_Prev = camMatrix.MultiplyPoint3x4(_calJig_EndPosW_Prev) + _calJig_RootUnitPosW_Prev;

							//apPortrait에서 함수로 묶였다.
							_calJig_EndPosW_Prev = _parentOptTransform._portrait.OffsetPos2World_Prev(_calJig_EndPosW_Prev);
							
						}
						_calJig_EndPos_Prev = _parentOptTransform._rootUnit._transform.InverseTransformPoint(_calJig_EndPosW_Prev);

						//위치가 유사하지 않다면) 끝점 위치 변화를 감지하자
						float angle_Exp2Prev = 0.0f;

						//외력은 Bone의 Normal 방향일때(각도 90도)일때 최대이며, 0도 일수록 작아져야 한다. (각도차이가 크더라도)
						float normalDeltaRatio = 0.0f;

						if (Mathf.Abs(endPos_Excepted.x - _calJig_EndPos_Prev.x) > 0.001f ||
							Mathf.Abs(endPos_Excepted.y - _calJig_EndPos_Prev.y) > 0.001f)
						{
							Vector2 start2End_Prev = _calJig_EndPos_Prev - _calJig_Tmp_WorldMatrix.Pos;
							Vector2 start2End_Expected = endPos_Excepted - _calJig_Tmp_WorldMatrix.Pos;
							angle_Exp2Prev = (Mathf.Atan2(start2End_Prev.y, start2End_Prev.x) - Mathf.Atan2(start2End_Expected.y, start2End_Expected.x)) * Mathf.Rad2Deg;
							if (angle_Exp2Prev < -180.0f)
							{
								//Debug.Log("[" + _name + "] 각도 제한 넘어감 : " + angle_Exp2Prev);
								angle_Exp2Prev += 360.0f;
							}
							else if (angle_Exp2Prev > 180.0f)
							{
								//Debug.Log("[" + _name + "] 각도 제한 넘어감 : " + angle_Exp2Prev);
								angle_Exp2Prev -= 360.0f;
							}

							//normalDeltaRatio을 구하자
							//[본 위치 -> 본 Exp 끝점]와 [본 Prev 끝점 -> 본 Exp 끝점]의 각도를 구하고, Abs(Sin) 값으로 Ratio를 계산한다.
							normalDeltaRatio = Mathf.Abs(Mathf.Sin(Vector2.Angle(start2End_Expected, endPos_Excepted - _calJig_EndPos_Prev) * Mathf.Deg2Rad));

							
						}


						//추가 21.3.9 : 외부 힘의 적용을 받는 지글본
						if (_parentOptTransform._portrait.IsAnyForceEvent)
						{
							//이전 프레임에서의 힘을 이용한다.
							//끝점(World)에서의 힘을 구하자
							Vector2 F_extW = _parentOptTransform._portrait.GetForce(_calJig_EndPosW_Prev);
							float powerSize = F_extW.magnitude;
							Vector2 Acc_extL = _parentOptTransform._rootUnit._transform.InverseTransformVector(F_extW).normalized;
							Acc_extL /= _jiggle_Mass;

							Vector2 prevDir = _calJig_EndPos_Prev - _worldMatrix.Pos_IKSpace;
							//F = ma
							//a = F/m
							//V += at
							float angularAcc = Mathf.Sin((float)Mathf.Atan2(Acc_extL.y, Acc_extL.x) - (float)Math.Atan2(prevDir.y, prevDir.x)) * powerSize;
							_calJig_Velocity += angularAcc * tDelta;
							//Debug.Log("Jiggle 본 힘 적용 : " + angularAcc);
						}


						//Drag를 곱하여 계산 + Prev를 더한다.
						//공기 저항이 클 수록 이전 위치에 있으려고 한다.
						float dAngle_Drag = (angle_Exp2Prev * _jiggle_Drag);

						//Jiggle 전과 후의 각도 차이
						//float dAngle_woJiggle = dAngle_Drag + _calJig_Angle_Result_Prev;//이전
						float dAngle_woJiggle = (dAngle_Drag * normalDeltaRatio) + _calJig_Angle_Result_Prev;//변경. 노멀 방향의 움직임에 강향 영향을 받는다.

						if(dAngle_woJiggle < -180.0f)
						{
							//Debug.Log("[" + _name + "] 결과 각도 제한 넘어감 : " + angle_Exp2Prev);
							dAngle_woJiggle += 360.0f;
						}
						else if(dAngle_woJiggle > 180.0f)
						{
							//Debug.Log("[" + _name + "] 결과 각도 제한 넘어감 : " + angle_Exp2Prev);
							dAngle_woJiggle -= 360.0f;
						}

						//2. dAngle_woJiggle 바탕으로 복원력(-kx)을 계산하고 속력에 더하기 (방향은 > Cur)
						_calJig_Velocity += (-1.0f * (_jiggle_K / _jiggle_Mass) * dAngle_woJiggle) * tDelta;
						


						//추가 21.3.8 외부 힘
						
						//3. 속력의 Damping를 더해서 감속하기
						//이동 각도의 반대 방향으로 계속 가해진다.
						_calJig_Velocity -= Mathf.Clamp01(_jiggle_Damping * Mathf.Clamp01(tDelta)) * _calJig_Velocity;
						

						//4. dAngle_woJiggle + (Vt) 결과가 최종 dAngle_Cur
						float minAngle = -180.0f;
						float maxAngle = 180.0f;
						//제한 범위 옵션 확인
						if(_isJiggleAngleConstraint)
						{
							minAngle = _jiggle_AngleLimit_Min;
							maxAngle = _jiggle_AngleLimit_Max;
						}


						//가중치가 적용된 경우 [v1.4.0 : 22.7.5]
						if (_calJig_IsWeight)
						{
							//회전 가능한 범위 : 0.3배까지 줄어든다.
							float weightedAngle_Min = Mathf.Clamp(minAngle * 0.1f, -5.0f, 0.0f);
							float weightedAngle_Max = Mathf.Clamp(maxAngle * 0.1f, 0.0f, 5.0f);

							if (_calJig_Weight < 0.3f)
							{
								minAngle = weightedAngle_Min;
								maxAngle = weightedAngle_Max;
							}
							else
							{
								//0.3~1 Weight에서는 "제한된 범위 ~ 원래 범위"로 축소된다.
								float lerp = Mathf.Clamp01((_calJig_Weight - 0.3f) / 0.7f);
								minAngle = weightedAngle_Min * (1.0f - lerp) + (minAngle * lerp);
								maxAngle = weightedAngle_Max * (1.0f - lerp) + (maxAngle * lerp);
							}
						}




						//Result 계산
						calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);



						//이전
						#region [미사용 코드]
						//if (calJig_Angle_Result < 0.0f && _calJig_Velocity < 0.0f)
						//{
						//	//Min과의 거리를 보자
						//	if (calJig_Angle_Result < minAngle)
						//	{
						//		//거리 제한
						//		calJig_Angle_Result = minAngle;
						//		_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
						//	}
						//	else if (calJig_Angle_Result < minAngle * 0.7f)
						//	{
						//		//70% 구간부터는 감속을 한다.
						//		//70% : x1 > 100% : x0
						//		_calJig_Velocity *= 1.0f - ((calJig_Angle_Result - (minAngle * 0.7f)) / (minAngle * 0.3f));
						//		calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);
						//	}
						//}
						//else if (calJig_Angle_Result > 0.0f && _calJig_Velocity > 0.0f)
						//{
						//	//Max와의 거리를 보자
						//	if (calJig_Angle_Result > maxAngle)
						//	{
						//		//거리 제한
						//		calJig_Angle_Result = maxAngle;
						//		_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
						//	}
						//	else if (calJig_Angle_Result > maxAngle * 0.7f)
						//	{
						//		//70% 구간부터는 감속을 한다.
						//		//70% : x1 > 100% : x0
						//		_calJig_Velocity *= 1.0f - ((calJig_Angle_Result - (maxAngle * 0.7f)) / (maxAngle * 0.3f));
						//		calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);
						//	}
						//} 
						#endregion

						//개선 20.7.15 : 속도에 상관없이 외부 힘이 작동하여 범위를 벗어나는 경우 포함
						if (calJig_Angle_Result < 0.0f)
						{
							//Min과의 거리를 보자
							if (calJig_Angle_Result < minAngle)
							{
								//거리 제한 (이건 속도 상관 없음)
								//> 그냥 Clamp를 하면 덜컹 거리므로, 이전 각도와 약간의 보정을 하자
								//calJig_Angle_Result = minAngle;
								calJig_Angle_Result = minAngle * 0.2f + Mathf.Clamp(_calJig_Angle_Result_Prev, minAngle, maxAngle) * 0.8f;
								_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
							}
							else if (calJig_Angle_Result < minAngle * 0.7f)
							{
								//70% 구간부터는 감속을 한다.
								if (_calJig_Velocity < 0.0f)
								{
									//지글 속도에 의해서 바깥으로 움직이는 경우
									
									//70% : x1 > 100% : x0
									_calJig_Velocity *= 1.0f - ((calJig_Angle_Result - (minAngle * 0.7f)) / (minAngle * 0.3f));
									calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);
								}
								else if (calJig_Angle_Result < _calJig_Angle_Result_Prev)
								{
									//지글 속도는 안쪽으로 향하는데 이전 프레임에 비해서 바깥으로 움직이는 경우
									//> 외부의 힘이 작동했다.
									//속도를 조금 줄여야 한다.
									//Prev >> Result 비율 : //Min 기준 70%일때 : 50% / Min 기준 100%일때 : 0%

									float correctRatio = (calJig_Angle_Result - (minAngle * 0.7f)) / (minAngle * 0.3f);
									correctRatio = Mathf.Clamp01(correctRatio * 0.5f + 0.5f);
									calJig_Angle_Result = (calJig_Angle_Result * (1.0f - correctRatio)) + (Mathf.Clamp(_calJig_Angle_Result_Prev, minAngle, 0.0f) * correctRatio);
									_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
								}
							}
						}
						else if (calJig_Angle_Result > 0.0f)
						{
							//Max와의 거리를 보자
							if (calJig_Angle_Result > maxAngle)
							{
								//거리 제한 (이건 속도 상관 없음)
								//> 그냥 Clamp를 하면 덜컹 거리므로, 이전 각도와 약간의 보정을 하자
								//calJig_Angle_Result = maxAngle;
								calJig_Angle_Result = maxAngle * 0.2f + Mathf.Clamp(_calJig_Angle_Result_Prev, minAngle, maxAngle) * 0.8f;
								_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
							}
							else if (calJig_Angle_Result > maxAngle * 0.7f)//<<여기에 속도 연산 추가
							{
								//70% 구간부터는 감속을 한다.
								if (_calJig_Velocity > 0.0f)
								{	
									//70% : x1 > 100% : x0
									_calJig_Velocity *= 1.0f - ((calJig_Angle_Result - (maxAngle * 0.7f)) / (maxAngle * 0.3f));
									calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);
								}
								else if (calJig_Angle_Result > _calJig_Angle_Result_Prev)
								{
									//지글 속도는 안쪽으로 향하는데 이전 프레임에 비해서 바깥으로 움직이는 경우
									//> 외부의 힘이 작동했다.
									//속도를 조금 줄여야 한다.
									//Prev >> Result 비율 : //Min 기준 70%일때 : 100% / Min 기준 100%일때 : 0%

									float correctRatio = (calJig_Angle_Result - (maxAngle * 0.7f)) / (maxAngle * 0.3f);
									correctRatio = Mathf.Clamp01(correctRatio * 0.5f + 0.5f);
									calJig_Angle_Result = (calJig_Angle_Result * (1.0f - correctRatio)) + (Mathf.Clamp(_calJig_Angle_Result_Prev, 0.0f, maxAngle) * correctRatio);
									_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
								}
							}
						}

						//calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);


						//- 실제 적용 정도 : 0.5까지는 반영되지 않다가, 0.5부터는 역지수 그래프를 이용하여 줄어든다.
						if(_calJig_IsWeight)
						{
							float lerpWeight = Mathf.Clamp01(_calJig_Weight / tDelta);
							calJig_Angle_Result *= lerpWeight;
							_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
						}




						//제한 범위를 넘었다.
						if (calJig_Angle_Result < -180.0f)
						{
							calJig_Angle_Result = -180.0f;
							_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
						}
						else if(calJig_Angle_Result > 180.0f)
						{
							calJig_Angle_Result = 180.0f;
							_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
						}

						//Debug.Log("[" + _name + "] Jiggle Angle : " + calJig_Angle_Result);
					}

					//---------------------------------
					// 이전 코드
					//---------------------------------
					//_worldMatrix._angleDeg += calJig_Angle_Result;
					//_worldMatrix.MakeMatrix();

					//---------------------------------
					// 래핑된 코드 20.8.31
					//---------------------------------
					_worldMatrix.RotateAsStep1(calJig_Angle_Result, true);

					//---------------------------------


					//현재 End 위치를 갱신하자
					_calJig_EndPos_Prev = _worldMatrix.MulPoint2(new Vector2(0, _shapeLength));

					//[Opt 코드] 저장시 Unity 좌표계로 한번 더 변환하자
					//_calJig_EndPosW_Prev = _parentOptTransform._rootUnit._transform.TransformPoint(_calJig_EndPos_Prev);
					_calJig_EndPosW_Prev = _parentOptTransform._rootUnit._transform.TransformPoint(_calJig_EndPos_Prev);//Portrait로 변경

					if(_parentOptTransform._portrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
					{
						//추가 20.9.15
						//빌보드 타입이라면 World 좌표계의 위치값을
						//카메라 좌표계 (프로젝션 아님)에서의 RootUnit 중심과의 위치 오프셋 (Vec2)으로 변환해서 저장하자.
						//회전만 하므로, Root Unit의 Rotation을 체크하면 된다.

						//상대 좌표 > Offset으로 변환 (이 값은 정사영과 같으므로 z값이 0이여야 한다. 이상하면 디버그할 것)
						//<중요> 이게 메인 식
						//Matrix4x4 camMatrix = Matrix4x4.TRS(Vector3.zero, _parentOptTransform._rootUnit._transform.rotation, Vector3.one).inverse;
						//_calJig_EndPosW_Prev = camMatrix.MultiplyPoint3x4(_calJig_EndPosW_Prev - _parentOptTransform._rootUnit._transform.position);
						//_calJig_RootUnitPosW_Prev = _parentOptTransform._rootUnit._transform.position;

						//apPortrait에서 함수를 단축했다.
						_calJig_EndPosW_Prev = _parentOptTransform._portrait.WorldPos2OffsetPos(_calJig_EndPosW_Prev);
					}

					_calJig_Angle_Result_Prev = calJig_Angle_Result;

					//변경 22.7.6
					isCalculateMatrixForce = true;//지글본 적용되었으니 하위 본의 매트릭스 갱신해야함
				}

				_isJiggleChecked_Prev = true;
				//isCalculateMatrixForce = true;//이전 : 지글본이 동작하면 항상 true > 그렇지 않는 경우가 있다.

				_calJig_IsZeroWeight_Prev = _calJig_IsZeroWeight;
			}


			
			//자식 본도 업데이트
			if(isRecursive)
			{
				if (_childBones != null)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						_childBones[i].MakeWorldMatrixForIK(true, isCalculateMatrixForce, isPhysics, isTeleportFrame, tDelta);
					}
				}
			}
		}


		/// <summary>
		/// MakeWorldMatrixForIK의 동기화 버전. 동기화된 본이 없는 경우만 처리한다.
		/// </summary>
		/// <param name="isRecursive"></param>
		/// <param name="isCalculateMatrixForce"></param>
		/// <param name="isPhysics"></param>
		/// <param name="tDelta"></param>
		public void MakeWorldMatrixForIKAsSyncBones(bool isRecursive, bool isCalculateMatrixForce, bool isPhysics, bool isTeleportFrame, float tDelta)
		{
			if (_syncBone == null)
			{
				if (_isIKCalculated_Controlled)
				{
					//IK가 계산된 결과를 넣자
					//World Matrix 재계산
					//--------------------------
					// 변경된 코드 20.8.31 : 래핑 + Skew + IKSpace
					//--------------------------

					float prevWorldMatrixAngle = _worldMatrix.Angle_IKSpace;
					float addIKAngle = (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled);

					float nextIKAngle = 0.0f;

					if (_IKRequestWeight_Controlled > 1.0f)
					{
						nextIKAngle = prevWorldMatrixAngle + addIKAngle;
					}
					else if (_IKRequestWeight_Controlled > 0.0f)
					{
						//Slerp가 적용된 코드
						nextIKAngle = apUtil.AngleSlerp(prevWorldMatrixAngle,
															prevWorldMatrixAngle + addIKAngle,
															_IKRequestWeight_Controlled);
					}

					//IK 적용
					//중요 : 래핑된 코드
					_worldMatrix.MakeWorldMatrix_IK(_localMatrix,
														//(_parentBone != null ? _parentBone._worldMatrix : null),
														(_parentBone != null ? (_parentBone._syncBone != null ? _parentBone._syncBone._worldMatrix : _parentBone._worldMatrix) : null),//[Sync]
														(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_World : null),
														nextIKAngle);


					//여기서도 External Request를 적용해준다.
					UpdateExternalRequest();//<Opt 코드

					//--------------------------

					isCalculateMatrixForce = true;//<<다음 Child 부터는 무조건 갱신을 해야한다.
				}
				else if (isCalculateMatrixForce)
				{
					//----------------------------------
					// 변경된 방식 20.8.31 : 래핑
					//----------------------------------
					//IK 자체는 적용되지 않았으나, Parent에서 적용된게 있어서 WorldMatrix를 그대로 쓸 순 없다.
					_worldMatrix.MakeWorldMatrix_Mod(_localMatrix,
														//(_parentBone != null ? _parentBone._worldMatrix : null),
														(_parentBone != null ? (_parentBone._syncBone != null ? _parentBone._syncBone._worldMatrix : _parentBone._worldMatrix) : null),//[Sync]
														(_parentOptTransform != null ? _parentOptTransform._matrix_TFResult_World : null));
				}
				
				
				if (_isJiggle && isPhysics)
				{
					//추가 22.7.6 : 계산 전, 가중치 적용 여부와 적용 정도를 계산한다.
					//1. Damping
					//2. 회전 범위 제한
					//3. 실제 적용 정도
					//Opt에서는 가중치를 미리 계산하고, 만약 가중치가 0에 수렴하면 아예 지글본을 끈다.
					_calJig_IsWeight = false;
					_calJig_IsZeroWeight = false;
					if(_isJiggleWeightCalculatable)
					{	
						if(_jiggle_IsControlParamWeight
							&& _linkedJiggleControlParam != null)
						{
							//컨트롤 파라미터인 경우 (Float타입 한정)
							if(_linkedJiggleControlParam._float_Cur < JIG_WEIGHT_CUTOUT_1)
							{
								_calJig_IsWeight = true;
								_calJig_Weight = Mathf.Clamp01(_linkedJiggleControlParam._float_Cur);
							}
						}

						if (_calJig_Weight < JIG_WEIGHT_CUTOUT_0)
						{
							//가중치가 너무 적어서 지글본 처리가 중단된다.
							//단, 이전 프레임에서도 ZeroWeight였어야 무시되며, 그렇지 않다면 일단 적용은 한다.
							_calJig_IsZeroWeight = true;
						}
					}


					if (!_isJiggleChecked_Prev)
					{
						//Prev 기록이 없다면, 초기화
						_calJig_Velocity = 0.0f;
						_calJig_Angle_Result_Prev = 0.0f;
						_calJig_EndPos_Prev = Vector2.zero;
						_calJig_EndPosW_Prev = Vector3.zero;

						//변경 22.7.6
						isCalculateMatrixForce = true;//지글본 적용되었으니 하위 본의 매트릭스 갱신해야함
					}
					else if(_calJig_IsZeroWeight && _calJig_IsZeroWeight_Prev)
					{
						//가중치가 0이라면 이번 프레임에서 지글본은 동작하지 않는다. (이전프레임에서도 가중치가 0이어야 함)
						_calJig_Velocity = 0.0f;
						_calJig_Angle_Result_Prev = 0.0f;
						_calJig_EndPos_Prev = Vector2.zero;
						_calJig_EndPosW_Prev = Vector3.zero;

						//>> 여기서는 지글본이 적용되지 않아서 자식 본들의 매트릭스 갱신 안함
					}
					else
					{
						//지글본에 따라서 WorldMatrix_IK를 수정하자
						float calJig_Angle_Result = _calJig_Angle_Result_Prev;

						if (tDelta > 0.0f
							&& !isTeleportFrame//추가 22.7.7 : 텔레포트 프레임이 아닐때라는 조건이 덧붙여진다.
							)
						{
							//계산은 tDelta가 0 유효할 때에만

							
							_calJig_Tmp_WorldMatrix.CopyFromMatrix(_worldMatrix);
							_calJig_Tmp_WorldMatrix.RotateAsStep1(_calJig_Angle_Result_Prev, true);

							//예상 위치
							Vector2 endPos_Excepted = _calJig_Tmp_WorldMatrix.MulPoint2(new Vector2(0, _shapeLength));


							//[Opt 코드] 좌표계 전환
							//기존에 저장된 값 : Unity World 좌표계.
							//이걸 Portrait 좌표계로 바꿔서 테스트하자
							//외부 움직임을 인식하기 위함
							if (_parentOptTransform._portrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
							{
								_calJig_EndPosW_Prev = _parentOptTransform._portrait.OffsetPos2World_Prev(_calJig_EndPosW_Prev);
							}
							_calJig_EndPos_Prev = _parentOptTransform._rootUnit._transform.InverseTransformPoint(_calJig_EndPosW_Prev);

							//위치가 유사하지 않다면) 끝점 위치 변화를 감지하자
							float angle_Exp2Prev = 0.0f;

							//외력은 Bone의 Normal 방향일때(각도 90도)일때 최대이며, 0도 일수록 작아져야 한다. (각도차이가 크더라도)
							float normalDeltaRatio = 0.0f;

							if (Mathf.Abs(endPos_Excepted.x - _calJig_EndPos_Prev.x) > 0.001f ||
								Mathf.Abs(endPos_Excepted.y - _calJig_EndPos_Prev.y) > 0.001f)
							{
								Vector2 start2End_Prev = _calJig_EndPos_Prev - _calJig_Tmp_WorldMatrix.Pos;
								Vector2 start2End_Expected = endPos_Excepted - _calJig_Tmp_WorldMatrix.Pos;
								angle_Exp2Prev = (Mathf.Atan2(start2End_Prev.y, start2End_Prev.x) - Mathf.Atan2(start2End_Expected.y, start2End_Expected.x)) * Mathf.Rad2Deg;
								if (angle_Exp2Prev < -180.0f)
								{
									//Debug.Log("[" + _name + "] 각도 제한 넘어감 : " + angle_Exp2Prev);
									angle_Exp2Prev += 360.0f;
								}
								else if (angle_Exp2Prev > 180.0f)
								{
									//Debug.Log("[" + _name + "] 각도 제한 넘어감 : " + angle_Exp2Prev);
									angle_Exp2Prev -= 360.0f;
								}

								//normalDeltaRatio을 구하자
								//[본 위치 -> 본 Exp 끝점]와 [본 Prev 끝점 -> 본 Exp 끝점]의 각도를 구하고, Abs(Sin) 값으로 Ratio를 계산한다.
								normalDeltaRatio = Mathf.Abs(Mathf.Sin(Vector2.Angle(start2End_Expected, endPos_Excepted - _calJig_EndPos_Prev) * Mathf.Deg2Rad));
							}

							//추가 21.3.9 : 외부 힘의 적용을 받는 지글본
							if (_parentOptTransform._portrait.IsAnyForceEvent)
							{
								//이전 프레임에서의 힘을 이용한다.
								//끝점(World)에서의 힘을 구하자
								Vector2 F_extW = _parentOptTransform._portrait.GetForce(_calJig_EndPosW_Prev);
								float powerSize = F_extW.magnitude;
								Vector2 Acc_extL = _parentOptTransform._rootUnit._transform.InverseTransformVector(F_extW).normalized;
								Acc_extL /= _jiggle_Mass;

								Vector2 prevDir = _calJig_EndPos_Prev - _worldMatrix.Pos_IKSpace;
								//F = ma
								//a = F/m
								//V += at
								float angularAcc = Mathf.Sin((float)Mathf.Atan2(Acc_extL.y, Acc_extL.x) - (float)Math.Atan2(prevDir.y, prevDir.x)) * powerSize;
								_calJig_Velocity += angularAcc * tDelta;
								//Debug.Log("Jiggle 본 힘 적용 : " + angularAcc);
							}


							//Drag를 곱하여 계산 + Prev를 더한다.
							//공기 저항이 클 수록 이전 위치에 있으려고 한다.
							float dAngle_Drag = (angle_Exp2Prev * _jiggle_Drag);

							//Jiggle 전과 후의 각도 차이
							//float dAngle_woJiggle = dAngle_Drag + _calJig_Angle_Result_Prev;//이전
							float dAngle_woJiggle = (dAngle_Drag * normalDeltaRatio) + _calJig_Angle_Result_Prev;//변경. 노멀 방향의 움직임에 강향 영향을 받는다.

							if (dAngle_woJiggle < -180.0f)
							{
								dAngle_woJiggle += 360.0f;
							}
							else if (dAngle_woJiggle > 180.0f)
							{
								dAngle_woJiggle -= 360.0f;
							}

							//2. dAngle_woJiggle 바탕으로 복원력(-kx)을 계산하고 속력에 더하기 (방향은 > Cur)
							_calJig_Velocity += (-1.0f * (_jiggle_K / _jiggle_Mass) * dAngle_woJiggle) * tDelta;

							//3. 속력의 Damping를 더해서 감속하기
							//이동 각도의 반대 방향으로 계속 가해진다.
							_calJig_Velocity -= Mathf.Clamp01(_jiggle_Damping * Mathf.Clamp01(tDelta)) * _calJig_Velocity;


							//4. dAngle_woJiggle + (Vt) 결과가 최종 dAngle_Cur
							float minAngle = -180.0f;
							float maxAngle = 180.0f;
							//제한 범위 옵션 확인
							if (_isJiggleAngleConstraint)
							{
								minAngle = _jiggle_AngleLimit_Min;
								maxAngle = _jiggle_AngleLimit_Max;
							}


							//가중치가 적용된 경우 [v1.4.0 : 22.7.5]
							if (_calJig_IsWeight)
							{
								//회전 가능한 범위 : 0.3배까지 줄어든다.
								float weightedAngle_Min = Mathf.Clamp(minAngle * 0.1f, -5.0f, 0.0f);
								float weightedAngle_Max = Mathf.Clamp(maxAngle * 0.1f, 0.0f, 5.0f);

								if (_calJig_Weight < 0.3f)
								{
									minAngle = weightedAngle_Min;
									maxAngle = weightedAngle_Max;
								}
								else
								{
									//0.3~1 Weight에서는 "제한된 범위 ~ 원래 범위"로 축소된다.
									float lerp = Mathf.Clamp01((_calJig_Weight - 0.3f) / 0.7f);
									minAngle = weightedAngle_Min * (1.0f - lerp) + (minAngle * lerp);
									maxAngle = weightedAngle_Max * (1.0f - lerp) + (maxAngle * lerp);
								}
							}



							//Result 계산
							calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);



							//개선 20.7.15 : 속도에 상관없이 외부 힘이 작동하여 범위를 벗어나는 경우 포함
							if (calJig_Angle_Result < 0.0f)
							{
								//Min과의 거리를 보자
								if (calJig_Angle_Result < minAngle)
								{
									//거리 제한 (이건 속도 상관 없음)
									//> 그냥 Clamp를 하면 덜컹 거리므로, 이전 각도와 약간의 보정을 하자
									//calJig_Angle_Result = minAngle;
									calJig_Angle_Result = minAngle * 0.2f + Mathf.Clamp(_calJig_Angle_Result_Prev, minAngle, maxAngle) * 0.8f;
									_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
								}
								else if (calJig_Angle_Result < minAngle * 0.7f)
								{
									//70% 구간부터는 감속을 한다.
									if (_calJig_Velocity < 0.0f)
									{
										//지글 속도에 의해서 바깥으로 움직이는 경우
										//70% : x1 > 100% : x0
										_calJig_Velocity *= 1.0f - ((calJig_Angle_Result - (minAngle * 0.7f)) / (minAngle * 0.3f));
										calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);
									}
									else if (calJig_Angle_Result < _calJig_Angle_Result_Prev)
									{
										float correctRatio = (calJig_Angle_Result - (minAngle * 0.7f)) / (minAngle * 0.3f);
										correctRatio = Mathf.Clamp01(correctRatio * 0.5f + 0.5f);
										calJig_Angle_Result = (calJig_Angle_Result * (1.0f - correctRatio)) + (Mathf.Clamp(_calJig_Angle_Result_Prev, minAngle, 0.0f) * correctRatio);
										_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
									}
								}
							}
							else if (calJig_Angle_Result > 0.0f)
							{
								//Max와의 거리를 보자
								if (calJig_Angle_Result > maxAngle)
								{
									//거리 제한 (이건 속도 상관 없음)
									//> 그냥 Clamp를 하면 덜컹 거리므로, 이전 각도와 약간의 보정을 하자
									//calJig_Angle_Result = maxAngle;
									calJig_Angle_Result = maxAngle * 0.2f + Mathf.Clamp(_calJig_Angle_Result_Prev, minAngle, maxAngle) * 0.8f;
									_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
								}
								else if (calJig_Angle_Result > maxAngle * 0.7f)//<<여기에 속도 연산 추가
								{
									//70% 구간부터는 감속을 한다.
									if (_calJig_Velocity > 0.0f)
									{
										//70% : x1 > 100% : x0
										_calJig_Velocity *= 1.0f - ((calJig_Angle_Result - (maxAngle * 0.7f)) / (maxAngle * 0.3f));
										calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);
									}
									else if (calJig_Angle_Result > _calJig_Angle_Result_Prev)
									{
										//지글 속도는 안쪽으로 향하는데 이전 프레임에 비해서 바깥으로 움직이는 경우
										//> 외부의 힘이 작동했다.
										//속도를 조금 줄여야 한다.
										//Prev >> Result 비율 : //Min 기준 70%일때 : 100% / Min 기준 100%일때 : 0%

										float correctRatio = (calJig_Angle_Result - (maxAngle * 0.7f)) / (maxAngle * 0.3f);
										correctRatio = Mathf.Clamp01(correctRatio * 0.5f + 0.5f);
										calJig_Angle_Result = (calJig_Angle_Result * (1.0f - correctRatio)) + (Mathf.Clamp(_calJig_Angle_Result_Prev, 0.0f, maxAngle) * correctRatio);
										_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
									}
								}
							}


							//- 실제 적용 정도 : 0.5까지는 반영되지 않다가, 0.5부터는 역지수 그래프를 이용하여 줄어든다.
							if(_calJig_IsWeight)
							{
								float lerpWeight = Mathf.Clamp01(_calJig_Weight / tDelta);
								calJig_Angle_Result *= lerpWeight;
								_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
							}



							//제한 범위를 넘었다.
							if (calJig_Angle_Result < -180.0f)
							{
								calJig_Angle_Result = -180.0f;
								_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
							}
							else if (calJig_Angle_Result > 180.0f)
							{
								calJig_Angle_Result = 180.0f;
								_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
							}
						}
						
						_worldMatrix.RotateAsStep1(calJig_Angle_Result, true);


						//현재 End 위치를 갱신하자
						_calJig_EndPos_Prev = _worldMatrix.MulPoint2(new Vector2(0, _shapeLength));

						//[Opt 코드] 저장시 Unity 좌표계로 한번 더 변환하자
						//_calJig_EndPosW_Prev = _parentOptTransform._rootUnit._transform.TransformPoint(_calJig_EndPos_Prev);
						_calJig_EndPosW_Prev = _parentOptTransform._rootUnit._transform.TransformPoint(_calJig_EndPos_Prev);//Portrait로 변경

						if (_parentOptTransform._portrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
						{
							_calJig_EndPosW_Prev = _parentOptTransform._portrait.WorldPos2OffsetPos(_calJig_EndPosW_Prev);
						}

						_calJig_Angle_Result_Prev = calJig_Angle_Result;

						//변경 22.7.6
						isCalculateMatrixForce = true;//지글본 적용되었으니 하위 본의 매트릭스 갱신해야함
					}

					_isJiggleChecked_Prev = true;
					//isCalculateMatrixForce = true;//이전 : 지글본이 동작하면 항상 true > 그렇지 않는 경우가 있다.

					_calJig_IsZeroWeight_Prev = _calJig_IsZeroWeight;
				}
			}
			


			
			//자식 본도 업데이트
			if(isRecursive)
			{
				if (_childBones != null)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						_childBones[i].MakeWorldMatrixForIKAsSyncBones(true, isCalculateMatrixForce, isPhysics, isTeleportFrame, tDelta);
					}
				}
			}
		}



		//추가 19.8.16 : 단일 본에 대한 LookAt 처리
		private bool CalculateSingleLookAtIK()
		{
			if(_IKController == null)
			{
				return false;
			}

			if(_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.LookAt)
			{
				return false;
			}

			if(_IKController._effectorBone == null)
			{
				return false;
			}

			//Control Param의 영향을 받는다.
			if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
				&& _IKController._isWeightByControlParam
				&& _IKController._weightControlParam != null)
			{
				_calculatedBoneIKWeight = Mathf.Clamp01(_IKController._weightControlParam._float_Cur);
			}

			if(_calculatedBoneIKWeight <= 0.001f)
			{
				return false;
			}

			Vector2 srcPos = _worldMatrix.Pos;
			Vector2 dstPos = _IKController._effectorBone._worldMatrix.Pos;



			float dist = Vector2.Distance(srcPos, dstPos);
			if(dist < 0.00001f)
			{
				return false;
			}
			float angleToDst = Mathf.Atan2(dstPos.y - srcPos.y, dstPos.x - srcPos.x);
			angleToDst *= Mathf.Rad2Deg;

			//기존
			//angleToDst -= 90.0f;

			//변경 20.8.8 [Scale 문제]
			if (_worldMatrix.Scale.y < 0.0f)
			{
				//Y스케일이 반전된 경우
				angleToDst += 90.0f;
			}
			else
			{
				angleToDst -= 90.0f;
			}


			angleToDst -= _worldMatrix.Angle;
			angleToDst = apUtil.AngleTo180(angleToDst);

			AddIKAngle_Controlled(angleToDst, _calculatedBoneIKWeight);

			//Debug.LogWarning("[" + _name + "] Single LookAt : " + angleToDst);

			return true;
		}

		/// <summary>
		/// 외부에서 요청한 TRS / IK를 적용한다.
		/// 2번 호출될 수 있다. (MakeWorldMatrix / MakeWorldMatrix_IK)
		/// </summary>
		private void UpdateExternalRequest()
		{
			//처리된 TRS
			_updatedWorldPos_NoRequest.x = _worldMatrix.Pos.x;
			_updatedWorldPos_NoRequest.y = _worldMatrix.Pos.y;

			_updatedWorldAngle_NoRequest = _worldMatrix.Angle;

			_updatedWorldScale_NoRequest.x = _worldMatrix.Scale.x;
			_updatedWorldScale_NoRequest.y = _worldMatrix.Scale.y;

			_updatedWorldPos = _updatedWorldPos_NoRequest;
			_updatedWorldAngle = _updatedWorldAngle_NoRequest;
			_updatedWorldScale = _updatedWorldScale_NoRequest;


			if(_isIKCalculated)
			{	
				_IKRequestAngleResult_World = apUtil.AngleTo180(_IKRequestAngleResult_World - 90.0f);

				//값을 할당하는 방식이 조금더 세분화 되었다.
				////float prevAngle = _updatedWorldAngle;
				//_updatedWorldAngle = _updatedWorldAngle * (1.0f - _IKRequestWeight) + (_IKRequestAngleResult_World * _IKRequestWeight);
				////Debug.Log("Add IK [" + _name + "] : " + prevAngle + " > " + _IKRequestAngleResult);

				//초기화는 아래에서 실행한다.(20.9.2)
				//_IKRequestAngleResult = 0.0f;
				//_IKRequestWeight = 0.0f;
			}


			

			//스크립트로 외부에서 제어한 경우
			if (_isExternalUpdate_Position)
			{
				_updatedWorldPos.x = (_exUpdate_Pos.x * _externalUpdateWeight) + (_updatedWorldPos.x * (1.0f - _externalUpdateWeight));
				_updatedWorldPos.y = (_exUpdate_Pos.y * _externalUpdateWeight) + (_updatedWorldPos.y * (1.0f - _externalUpdateWeight));
			}

			if (_isExternalUpdate_Rotation)
			{
				_updatedWorldAngle = (_exUpdate_Angle * _externalUpdateWeight) + (_updatedWorldAngle * (1.0f - _externalUpdateWeight));
			}

			if(_isExternalUpdate_Scaling)
			{ 
				_updatedWorldScale.x = (_exUpdate_Scale.x * _externalUpdateWeight) + (_updatedWorldScale.x * (1.0f - _externalUpdateWeight));
				_updatedWorldScale.y = (_exUpdate_Scale.y * _externalUpdateWeight) + (_updatedWorldScale.y * (1.0f - _externalUpdateWeight));
			}

			if(_isExternalConstraint)
			{
				//if(_isExternalConstraint_Xsurface)
				//{

				//}
				if(_isExternalConstraint_Xpref)
				{
					_updatedWorldPos.x = _externalConstraint_PosX.y;
				}
				if(_isExternalConstraint_Ypref)
				{
					_updatedWorldPos.y = _externalConstraint_PosY.y;
				}
				if (_isExternalConstraint_Xmin)
				{
					_updatedWorldPos.x = Mathf.Max(_updatedWorldPos.x, _externalConstraint_PosX.x);
				}
				if (_isExternalConstraint_Xmax)
				{
					_updatedWorldPos.x = Mathf.Min(_updatedWorldPos.x, _externalConstraint_PosX.z);
				}
				if (_isExternalConstraint_Ymin)
				{
					_updatedWorldPos.y = Mathf.Max(_updatedWorldPos.y, _externalConstraint_PosY.x);
				}
				if (_isExternalConstraint_Ymax)
				{
					_updatedWorldPos.y = Mathf.Min(_updatedWorldPos.y, _externalConstraint_PosY.z);
				}

				if(_isExternalConstraint_Xsurface)
				{
					_updatedWorldPos.x = Mathf.Clamp((_updatedWorldPos.x - _externalConstraint_PosSurfaceX.x) + _externalConstraint_PosSurfaceX.y, _externalConstraint_PosSurfaceX.z, _externalConstraint_PosSurfaceX.w);
				}
				if(_isExternalConstraint_Ysurface)
				{
					_updatedWorldPos.y = Mathf.Clamp((_updatedWorldPos.y - _externalConstraint_PosSurfaceY.x) + _externalConstraint_PosSurfaceY.y, _externalConstraint_PosSurfaceY.z, _externalConstraint_PosSurfaceY.w);
				}
			}
			
			// 이전 코드
			//if (_isIKCalculated 
			//	|| _isExternalUpdate_Position 
			//	|| _isExternalUpdate_Rotation 
			//	|| _isExternalUpdate_Scaling 
			//	|| _isExternalConstraint)
			//{
			//	//WorldMatrix를 갱신해주자
			//	_worldMatrix.SetTRS(	_updatedWorldPos.x, _updatedWorldPos.y,
			//							_updatedWorldAngle,
			//							_updatedWorldScale.x, _updatedWorldScale.y, true);

			//	//>> 이건 Post에서 처리
			//	//_isIKCalculated = false;
			//	//_isExternalUpdate_Position = false;
			//	//_isExternalUpdate_Rotation = false;
			//	//_isExternalUpdate_Scaling = false;
			//	//
			//}

			//변경 20.8.31 : 래핑된 함수를 위한 새로운 TRS 코드 + 다른 플래그
			if (_isIKCalculated //<<이거 없어져야함
				|| _isExternalUpdate_Position 
				|| _isExternalUpdate_Rotation 
				|| _isExternalUpdate_Scaling 
				//|| _isExternalUpdate_IK
				|| _isExternalConstraint)
			{
				//WorldMatrix를 갱신해주자
				//Debug.Log("[" + this.gameObject.name + "] Set TRS As Result : (IK : " + _isIKCalculated + " / " + _updatedWorldAngle + ")");

				_worldMatrix.SetTRSAsResult(
									(_isExternalUpdate_Position || _isExternalConstraint), _updatedWorldPos,
									//(_isExternalUpdate_Rotation || _isIKCalculated), _updatedWorldAngle,//이전
									_isExternalUpdate_Rotation, _updatedWorldAngle,//변경
									_isExternalUpdate_Scaling, _updatedWorldScale,
									_isIKCalculated, _IKRequestAngleResult_World, _IKRequestAngleResult_Delta, _IKRequestWeight//추가 20.9.2
									);
			}


			//변경 20.9.2 : IK 요청 초기화는 여기서 하자
			if(_isIKCalculated)
			{
				_IKRequestAngleResult_World = 0.0f;
				_IKRequestAngleResult_Delta = 0.0f;
				_IKRequestWeight = 0.0f;
			}
			
		}

		/// <summary>
		/// 모든 WorldMatrix가 끝나고, WorldMatrix에 영향을 받는 변수들을 갱신한다.
		/// </summary>
		public void UpdatePostRecursive()
		{
			//TODO : 이건 나중에 일괄적으로 업데이트 해야한다.
#if UNITY_EDITOR
			_shapePoint_End = new Vector2(0.0f, _shapeLength);


			_shapePoint_Mid1 = new Vector2(-_shapeWidth * 0.5f, _shapeLength * 0.2f);
			_shapePoint_Mid2 = new Vector2(_shapeWidth * 0.5f, _shapeLength * 0.2f);

			float taperRatio = Mathf.Clamp01((float)(100 - _shapeTaper) / 100.0f);

			_shapePoint_End1 = new Vector2(-_shapeWidth * 0.5f * taperRatio, _shapeLength);
			_shapePoint_End2 = new Vector2(_shapeWidth * 0.5f * taperRatio, _shapeLength);

			_shapePoint_End = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End);
			_shapePoint_Mid1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid1);
			_shapePoint_Mid2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid2);
			_shapePoint_End1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End1);
			_shapePoint_End2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End2);
#endif

			//TODO : 이것도 나중에 일괄적으로 업데이트 해야한다.

			//Rigging을 위해서 Matrix 통합 식을 만들자
			//실제 식
			// world * default_inv * VertPos W
			_vertWorld2BoneModWorldMatrix = _worldMatrix.MtrxToSpace;
			_vertWorld2BoneModWorldMatrix *= _worldMatrix_NonModified.MtrxToLowerSpace;


			


			if (_socketTransform != null)
			{
				//소켓을 업데이트 하자
				_socketTransform.localPosition = new Vector3(_worldMatrix.Pos.x, _worldMatrix.Pos.y, 0);
				_socketTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, _worldMatrix.Angle);
				_socketTransform.localScale = new Vector3(_worldMatrix.Scale.x, _worldMatrix.Scale.y, 1.0f);

				//이건 Skew Scale에서는 정상적으로 동작하지 않을 가능성이 높다.
			}

			_isIKCalculated = false;
			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;
			//_isExternalUpdate_IK = false;

			_isExternalConstraint = false;
			_isExternalConstraint_Xmin = false;
			_isExternalConstraint_Xmax = false;
			_isExternalConstraint_Ymin = false;
			_isExternalConstraint_Ymax = false;
			_isExternalConstraint_Xpref = false;
			_isExternalConstraint_Ypref = false;
			_isExternalConstraint_Xsurface = false;
			_isExternalConstraint_Ysurface = false;

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].UpdatePostRecursive();

				}
			}
		}



		/// <summary>
		/// UpdatePostRecursive의 동기화 버전. 동기화된 본이 없는 경우만 처리한다.
		/// </summary>
		public void UpdatePostRecursiveAsSyncBones()
		{
			if (_syncBone == null)
			{
#if UNITY_EDITOR
				_shapePoint_End = new Vector2(0.0f, _shapeLength);

				_shapePoint_Mid1 = new Vector2(-_shapeWidth * 0.5f, _shapeLength * 0.2f);
				_shapePoint_Mid2 = new Vector2(_shapeWidth * 0.5f, _shapeLength * 0.2f);

				float taperRatio = Mathf.Clamp01((float)(100 - _shapeTaper) / 100.0f);

				_shapePoint_End1 = new Vector2(-_shapeWidth * 0.5f * taperRatio, _shapeLength);
				_shapePoint_End2 = new Vector2(_shapeWidth * 0.5f * taperRatio, _shapeLength);

				_shapePoint_End = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End);
				_shapePoint_Mid1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid1);
				_shapePoint_Mid2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid2);
				_shapePoint_End1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End1);
				_shapePoint_End2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End2);
#endif

				//Rigging을 위해서 Matrix 통합 식을 만들자
				//실제 식
				// world * default_inv * VertPos W
				_vertWorld2BoneModWorldMatrix = _worldMatrix.MtrxToSpace;
				_vertWorld2BoneModWorldMatrix *= _worldMatrix_NonModified.MtrxToLowerSpace;

				if (_socketTransform != null)
				{
					//소켓을 업데이트 하자
					_socketTransform.localPosition = new Vector3(_worldMatrix.Pos.x, _worldMatrix.Pos.y, 0);
					_socketTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, _worldMatrix.Angle);
					_socketTransform.localScale = new Vector3(_worldMatrix.Scale.x, _worldMatrix.Scale.y, 1.0f);

					//이건 Skew Scale에서는 정상적으로 동작하지 않을 가능성이 높다.
				}
			}

			_isIKCalculated = false;
			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;
			//_isExternalUpdate_IK = false;

			_isExternalConstraint = false;
			_isExternalConstraint_Xmin = false;
			_isExternalConstraint_Xmax = false;
			_isExternalConstraint_Ymin = false;
			_isExternalConstraint_Ymax = false;
			_isExternalConstraint_Xpref = false;
			_isExternalConstraint_Ypref = false;
			_isExternalConstraint_Xsurface = false;
			_isExternalConstraint_Ysurface = false;

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].UpdatePostRecursiveAsSyncBones();

				}
			}
		}



		// Functions
		//---------------------------------------------------------------
		// 외부 제어 코드를 넣자
		// <Portrait 기준으로 Local Space = Bone 기준으로 World Space 로 설정한다 >
		/// <summary>
		/// Set Position
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="weight"></param>
		public void SetPosition(Vector2 worldPosition, float weight = 1.0f)
		{
			_isExternalUpdate_Position = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Pos = worldPosition;
		}

		/// <summary>
		/// Set Rotation
		/// </summary>
		/// <param name="worldAngle"></param>
		/// <param name="weight"></param>
		public void SetRotation(float worldAngle, float weight = 1.0f)
		{
			_isExternalUpdate_Rotation = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Angle = worldAngle;
		}


		/// <summary>
		/// Set Scale
		/// </summary>
		/// <param name="worldScale"></param>
		/// <param name="weight"></param>
		public void SetScale(Vector2 worldScale, float weight = 1.0f)
		{
			_isExternalUpdate_Scaling = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Scale = worldScale;
		}
		

		/// <summary>
		/// Set TRS (Position, Rotation, Scale)
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="worldAngle"></param>
		/// <param name="worldScale"></param>
		/// <param name="weight"></param>
		public void SetTRS(Vector2 worldPosition, float worldAngle, Vector2 worldScale, float weight = 1.0f)
		{
			_isExternalUpdate_Position = true;
			_isExternalUpdate_Rotation = true;
			_isExternalUpdate_Scaling = true;

			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Pos = worldPosition;
			_exUpdate_Angle = worldAngle;
			_exUpdate_Scale = worldScale;
		}

		public void SetPositionConstraint(float worldPositionValue, ConstraintBound constraintBound)
		{
			//Debug.Log("PosConst [" + _name + "] : " + constraintBound + " >>  " + worldPositionValue);
			_isExternalConstraint = true;
			switch (constraintBound)
			{
				case ConstraintBound.Xprefer:
					_isExternalConstraint_Xpref = true;
					_externalConstraint_PosX.y = worldPositionValue;
					break;

				case ConstraintBound.Xmin:
					_isExternalConstraint_Xmin = true;
					_externalConstraint_PosX.x = worldPositionValue;
					break;

				case ConstraintBound.Xmax:
					_isExternalConstraint_Xmax = true;
					_externalConstraint_PosX.z = worldPositionValue;
					break;

				case ConstraintBound.Yprefer:
					_isExternalConstraint_Ypref = true;
					_externalConstraint_PosY.y = worldPositionValue;
					break;
				case ConstraintBound.Ymin:
					_isExternalConstraint_Ymin = true;
					_externalConstraint_PosY.x = worldPositionValue;
					break;

				case ConstraintBound.Ymax:
					_isExternalConstraint_Ymax = true;
					_externalConstraint_PosY.z = worldPositionValue;
					break;
			}
		}

		public void SetPositionConstraintSurface(float defaultSurfacePos, float curSufracePos, float minSurfacePos, float maxSurfacePos, ConstraintSurface constraintSurface)
		{
			_isExternalConstraint = true;
			switch (constraintSurface)
			{
				case ConstraintSurface.Xsurface:
					_isExternalConstraint_Xsurface = true;
					_externalConstraint_PosSurfaceX.x = defaultSurfacePos;
					_externalConstraint_PosSurfaceX.y = curSufracePos;
					_externalConstraint_PosSurfaceX.z = minSurfacePos;
					_externalConstraint_PosSurfaceX.w = maxSurfacePos;
					break;

				case ConstraintSurface.Ysurface:
					_isExternalConstraint_Ysurface = true;
					_externalConstraint_PosSurfaceY.x = defaultSurfacePos;
					_externalConstraint_PosSurfaceY.y = curSufracePos;
					_externalConstraint_PosSurfaceY.z = minSurfacePos;
					_externalConstraint_PosSurfaceY.w = maxSurfacePos;
					break;
			}
		}



		// IK 요청을 하자
		//-------------------------------------------------------------------------------
		/// <summary>
		/// IK is calculated. Depending on the location requested, all Bones connected by IK move automatically.
		/// </summary>
		/// <param name="targetPosW"></param>
		/// <param name="weight"></param>
		/// <param name="isContinuous"></param>
		/// <returns></returns>
		public bool RequestIK(Vector2 targetPosW, float weight, bool isContinuous)
		{
			if (!_isIKTail || _IKChainSet == null || !_isIKChainSetAvailable)
			{
				//Debug.LogError("End -> _isIKTail : " + _isIKTail + " / _IKChainSet : " + _IKChainSet);
				return false;
			}

			if(!_isIKChainInit)
			{
				InitIKChain();
				_isIKChainInit = true;
			}

			//Debug.Log("Request IK > " + targetPosW);

			//bool isSuccess = _IKChainSet.SimulateIK(targetPosW, isContinuous);
			bool isSuccess = _IKChainSet.SimulateIK(_worldMatrix.ConvertForIK(targetPosW), isContinuous);//IKSpace로 변경

			//IK가 실패하면 패스
			if (!isSuccess)
			{
				//Debug.LogError("Failed");
				return false;
			}

			//IK 결과값을 Bone에 넣어주자
			_IKChainSet.AdaptIKResultToBones(weight);

			//Debug.Log("Success");
			//TODO

			return true;
		}

		/// <summary>
		/// [Please do not use it] Initialize IK Chain
		/// </summary>
		public void InitIKChain()
		{
			if(_IKChainSet != null && _isIKChainSetAvailable)
			{
				if (_IKChainSet._bone == null)
				{
					_IKChainSet._bone = this;
					Debug.LogError("AnyPortrait : BoneIK Settings are wrong : "+ _name);
				}
				_IKChainSet.RefreshChain();
			}

			//if(_IKController != null && _isIKChainSetAvailable && _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
			if(_IKController != null && _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
			{
				//변경 : _isIKChainSetAvailable가 false여도 일단 Link를 하자.
				_IKController.Link(_parentOptTransform._portrait);
			}

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].InitIKChain();
				}
			}
		}


		// World Matrix 복사 (리타겟용)
		//---------------------------------------------------------------
		public void CopyWorldMatrixFromBone(apOptBone srcBone)
		{
			_worldMatrix.CopyFromMatrix(srcBone._worldMatrix);
		}


		// Get / Set
		//---------------------------------------------------------------
		// boneID를 가지는 Bone을 자식 노드로 두고 있는가.
		// 재귀적으로 찾는다.
		public apOptBone GetChildBoneRecursive(int boneID)
		{
			if (_childBones == null)
			{
				return null;
			}
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해보자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(boneID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		// 바로 아래의 자식 Bone을 검색한다.
		public apOptBone GetChildBone(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			return null;
		}

		// 자식 Bone 중에서 특정 Target Bone을 재귀적인 자식으로 가지는 시작 Bone을 찾는다.
		public apOptBone FindNextChainedBone(int targetBoneID)
		{
			//바로 아래의 자식 노드를 검색
			if (_childBones == null)
			{
				return null;
			}
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == targetBoneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해서, 그 중에 실제로 Target Bone을 포함하는 Child Bone을 리턴하자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(targetBoneID);
				if (result != null)
				{
					//return result;
					return _childBones[i];//<<Result가 아니라, ChildBone을 리턴
				}
			}
			return null;
		}

		// 요청한 boneID를 가지는 Bone을 부모 노드로 두고 있는가.
		// 재귀적으로 찾는다.
		public apOptBone GetParentRecursive(int boneID)
		{
			if (_parentBone == null)
			{
				return null;
			}

			if (_parentBone._uniqueID == boneID)
			{
				return _parentBone;
			}

			//재귀적으로 검색해보자
			return _parentBone.GetParentRecursive(boneID);

		}


		//-----------------------------------------------------------------------------------------------
		/// <summary>Bone's Position</summary>
		public Vector3 Position { get { return _updatedWorldPos; } }

		/// <summary>Bone's Angle (Degree)</summary>
		public float Angle {  get { return _updatedWorldAngle; } }

		/// <summary>Bone's Scale</summary>
		public Vector3 Scale { get { return _updatedWorldScale; } }

		
		/// <summary>Bone's Position without User's external request</summary>
		public Vector3 PositionWithouEditing {  get { return _updatedWorldPos_NoRequest; } }
		
		/// <summary>Bone's Angle without User's external request</summary>
		public float AngleWithouEditing {  get { return _updatedWorldAngle_NoRequest; } }
		
		/// <summary>Bone's Scale without User's external request</summary>
		public Vector3 ScaleWithouEditing {  get { return _updatedWorldScale_NoRequest; } }


		//-----------------------------------------------------------------------------------------------


//		// Gizmo Event
//#if UNITY_EDITOR
//		void OnDrawGizmosSelected()
//		{
//			Gizmos.color = _color;

//			Matrix4x4 tfMatrix = transform.localToWorldMatrix;
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_End));

//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1), tfMatrix.MultiplyPoint3x4(_shapePoint_End1));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_End1), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
//		}
//#endif
	}

}