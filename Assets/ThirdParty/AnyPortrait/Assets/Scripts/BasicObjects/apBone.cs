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
using System.Runtime.InteropServices;

namespace AnyPortrait
{
	// MeshGroup에 포함되어 본 애니메이션을 가능하게 한다.
	// 계층적으로 참조가 가능하며, 여러개의 Root를 가질 수 있다.
	// Opt 버전은 MonoBehaviour로 정의가 가능하다.
	// 기본적인 TRS와 달리, S 왜곡이 없는 계산법을 사용한다. (Scale은 계층적으로 계산되지 않는다)
	/// <summary>
	/// It is a class for Bone animation.
	/// </summary>
	[Serializable]
	public class apBone
	{
		// Members
		//--------------------------------------------
		public string _name = "Bone";

		public int _uniqueID = -1;

		public int _meshGroupID = -1;

		[NonSerialized]
		public apMeshGroup _meshGroup = null;

		/// <summary>
		/// 이 Bone이 속한 MeshGroup이 실제 렌더링/업데이트 되는 RenderUnit을 저장한다.
		/// (자주 갱신된다)
		/// Root Bone은 이 RenderUnit의 Transform을 ParentWorldMatrix로 삼는다.
		/// </summary>
		[NonSerialized]
		public apRenderUnit _renderUnit = null;

		//ParentBone이 없으면 Root이다.
		//그 외에는 Parent Bone을 연결한다.
		public int _parentBoneID = -1;
		public int _level = 0;
		public int _recursiveIndex = 0;

		[NonSerialized]
		public apBone _parentBone = null;



		[SerializeField]
		public List<int> _childBoneIDs = new List<int>();

		[NonSerialized]
		public List<apBone> _childBones = new List<apBone>();

		//Transform은 +Y를 Up으로 삼아서 판단한다.

		// Transform Matrix
		//1) Local Default Transform Matrix [직렬화]
		// : Parent를 기준으로 Matrix가 계산된다. Root인 경우 MeshGroup내에서의 위치와 기본 변환값이 저장된다.

		//2) Local Modified Transform Matrix
		// : Animation, Modifier 등으로 변경된 Matrix이다. 변형값을 따로 가지고 있으며, Default TF Matrix의 값을 포함한다! (<- 중요)

		//2) World Matrix
		// : Parent의 World Matrix * Local Transform Matrix를 구하면 이 Bone의 World Matrix를 구할 수 있다. 매번 업데이트된다.

		[SerializeField]
		public apMatrix _defaultMatrix = new apMatrix();

		//실제로 중요한 값은 아니고,
		//작업 화면에서 겹쳐 보이는 것을 조절할 때 사용된다.
		//렌더링 순서는 Recursive 우선 방식. 같은 Level에서 비교할때만 Depth를 이용한다.
		//Depth가 큰게 나중에(위에) 렌더링
		[SerializeField]
		public int _depth = 0;

		

		//RigTestMatrix : 에디터에서만 정의되는 변수. Rigging 작업시 Test Posing이 켜져있으면 동작한다.
		[NonSerialized]
		public apMatrix _rigTestMatrix = new apMatrix();


		//LocalMatrix : default(Local) Matrix에서 TRS를 각각 적용한 Matrix (곱하기가 아니다)
		[NonSerialized]
		public apMatrix _localMatrix = new apMatrix();

		[NonSerialized]
		private Vector2 _deltaPos = Vector2.zero;

		[NonSerialized]
		private float _deltaAngle = 0.0f;

		[NonSerialized]
		private Vector2 _deltaScale = Vector2.one;
		
		//기존 방식 : 보통의 apMatrix 사용 + RMultiply 처리 방식
		//[NonSerialized]
		//public apMatrix _worldMatrix = new apMatrix();

		//[NonSerialized]
		//public apMatrix _worldMatrix_NonModified = new apMatrix();

		//[NonSerialized]
		//public apMatrix _worldMatrix_IK = new apMatrix();

		//변경 20.8.12 : Skew도 지원하는 본 전용의 WorldMatrix로 변경 [Skew 이슈]
		[NonSerialized]
		public apBoneWorldMatrix _worldMatrix = null;

		[NonSerialized]
		public apBoneWorldMatrix _worldMatrix_NonModified = null;

		[NonSerialized]
		public apBoneWorldMatrix _worldMatrix_IK = null;

		//계산된 Bone IK Controller Weight와 IK용 worldMatrix > 이건 편집과 구분하기 위해 별도로 계산.
		[NonSerialized]
		public float _calculatedBoneIKWeight = 0.0f;

		
		//추가 20.8.23 : GUI / 기즈모용 WorldMatrix
		//EndPos를 찾아서 TRS를 역으로 계산한다. GUIUpdate에서 갱신됨
		//IK도 포함이다.
		[NonSerialized]
		public apMatrix _guiMatrix = null;
		[NonSerialized]
		public apMatrix _guiMatrix_IK = null;

		[NonSerialized]
		public apPortrait.ROOT_BONE_SCALE_METHOD _rootBoneScaleMethod = apPortrait.ROOT_BONE_SCALE_METHOD.Default;

		

		///// <summary>
		///// Parent Bone이나 속한 MeshGroup (Transform)의 World Matrix.
		///// 단순 참조 용이므로 외부로부터 환성된 WorldMatrix를 받아야한다.
		///// </summary>
		//[NonSerialized]
		//private apMatrix _parentMatrix = null;

		// 제어 정보
		// IK 여부
		//1) Child가 하나인 Bone은 편집시에 IK가 1단계 적용이 된다.
		//2) /// TODO : IK는 나중에 합시다

		// UI에 보이는 정보
		//1) 색상 (이걸로 나중에 Weight를 표현한다)
		//2) 모양 (폭, 기본 길이(Scale 1일때의 길이), 뾰족한 정도 % (Taper : 100일때 뾰족, 0일때 직사각형))
		//- 모양에 따라서 GUI에서 클릭 처리 영역이 달라진다.
		//- 말단 노드의 경우 Length를 0으로 만들 수 있다.

		[SerializeField]
		public Color _color = Color.white;
		public int _shapeWidth = 30;
		public int _shapeLength = 50;//<<이 값은 생성할 때 Child와의 거리로 판단한다.
		public int _shapeTaper = 100;//기본값은 뾰족

		public bool _shapeHelper = false;//<<추가 : Helper 속성이 True이면 꼬리가 안보인다. Length는 유지한다.

		//GUI에 표시하기 위한 포인트
		// [ Version 1 ]
		//            [Mid1] (-x)                    [End1] (-x, +y)
		//              |
		// [Start]     <Length - 20%>     ------->   [End] (+y -> )
		//              |
		//            [Mid2] (+x)                    [End2] (+x, +y)
		public class ShapePoints_V1
		{
			public Vector2 End = Vector2.zero;
			public Vector2 Mid1 = Vector2.zero;
			public Vector2 Mid2 = Vector2.zero;
			public Vector2 End1 = Vector2.zero;
			public Vector2 End2 = Vector2.zero;
			public float Width_Half = 1.0f;
			//public float Radius = 1.0f;

			public ShapePoints_V1()
			{
				Init();
			}
			public void Init()
			{
				End = Vector2.zero;
				Mid1 = Vector2.zero;
				Mid2 = Vector2.zero;
				End1 = Vector2.zero;
				End2 = Vector2.zero;
				Width_Half = 1.0f;
				//Radius = 1.0f;
			}
		}

		[NonSerialized]
		public ShapePoints_V1 _shapePoints_V1_Normal = new ShapePoints_V1();

		//추가:IK가 적용된 GUI 레이아웃을 그리기 위해서는 별도의 shapePoint가 필요하다 (동시에 렌더링을 하기 위함)
		[NonSerialized]
		public ShapePoints_V1 _shapePoints_V1_IK = new ShapePoints_V1();

		//GUI에 표시하기 위한 포인트
		// [ Version 2 ]
		// [Back1] (-x, -y (radius))			[Mid1] (-x)				[End1] (-x, +y)
		//
		//							<------		[Start]		----->
		//
		// [Back2] (+x, -y (radius))			[Mid2] (+x)				[End2] (+x, +y)
		public class ShapePoints_V2
		{
			public Vector2 Back1 = Vector2.zero;
			public Vector2 Back2 = Vector2.zero;
			public Vector2 Mid1 = Vector2.zero;
			public Vector2 Mid2 = Vector2.zero;
			public Vector2 End = Vector2.zero;
			public Vector2 End1 = Vector2.zero;
			public Vector2 End2 = Vector2.zero;
			public float RadiusCorrectionRatio = 1.0f;
			public float LineThickness = 1.0f;//클릭을 위한 선 굵기(V2 방식은 스케일, 줌에 따라 선의 굵기가 바뀐다.)
			
			//public float Radius_Org = 1.0f;
			//public float Radius_Helper = 1.0f;

			public ShapePoints_V2()
			{
				Init();
			}

			public void Init()
			{
				Back1 = Vector2.zero;
				Back2 = Vector2.zero;
				Mid1 = Vector2.zero;
				Mid2 = Vector2.zero;
				End = Vector2.zero;
				End1 = Vector2.zero;
				End2 = Vector2.zero;
				//Radius_Org = 1.0f;
				//Radius_Helper = 1.0f;
				RadiusCorrectionRatio = 1.0f;
				LineThickness = 1.0f;
			}
		}

		[NonSerialized]
		public ShapePoints_V2 _shapePoints_V2_Normal = new ShapePoints_V2();
		[NonSerialized]
		public ShapePoints_V2 _shapePoints_V2_IK = new ShapePoints_V2();

		//private const float BONE_V2_REAL_LENGTH_RATIO = 1.0083f;//GUI상의 길이는 아주 약간 더 길어야 한다.
		public const float BONE_V2_REAL_LENGTH_RATIO = 1.0198f;//GUI상의 길이는 아주 약간 더 길어야 한다.
		public const float BONE_V2_DEFAULT_LINE_THICKNESS = 4.0f;
		


		//계산 후 공통 변수
		[NonSerialized]
		public Vector2 _shapePoint_Calculated_End = Vector2.zero;


		//추가 30.2.21 : 본 렌더링 설정을 static 변수 넣으면 한번에 참조할 수 있다.
		public class RenderSettings
		{
			public bool _isVersion2 = false;
			public float _scaleRatio = 1.0f;
			public bool _isScaledByZoom = false;
			public float _workspaceZoom = 1.0f;

			public float _radius_V1_Org = 1.0f;
			public float _radius_V2_Org = 1.0f;
			public float _radius_V2_Helper = 1.0f;
			public float _widthHalf_V2 = 1.0f;

			public RenderSettings() { } 
		}
		private static RenderSettings s_boneRenderSettings = new RenderSettings();
		//public static RenderSettings BoneRenderSettings { get { return s_boneRenderSettings; } }


		private const float BONE_V1_DEFAULT_RADIUS_ORG = 10.0f;
		private const float BONE_V2_DEFAULT_RADIUS_ORG = 12.0f;
		private const float BONE_V2_DEFAULT_RADIUS_HELPER = 24.0f;
		private const float BONE_V2_DEFAULT_WIDTH_HALF = 15.0f;
		
		/// <summary> Bone GUI Update전에는 호출해야 하는 함수. 화면 상태에 따라 매번 호출하자. </summary>
		public static void SetRenderSettings(bool isVersion2, int scaleRatio_x100, bool isScaledByZoom, float workspaceZoom)
		{
			if(s_boneRenderSettings == null)
			{
				s_boneRenderSettings = new RenderSettings();
			}
			s_boneRenderSettings._isVersion2 = isVersion2;
			s_boneRenderSettings._scaleRatio = (float)(Mathf.Clamp(scaleRatio_x100, 10, 200)) * 0.01f;
			s_boneRenderSettings._isScaledByZoom = isScaledByZoom;
			s_boneRenderSettings._workspaceZoom = workspaceZoom;

			//렌더링을 위한 공통 설정을 계산하자.
			s_boneRenderSettings._radius_V1_Org = BONE_V1_DEFAULT_RADIUS_ORG * s_boneRenderSettings._scaleRatio;
			s_boneRenderSettings._radius_V2_Org = BONE_V2_DEFAULT_RADIUS_ORG * s_boneRenderSettings._scaleRatio;
			s_boneRenderSettings._radius_V2_Helper = BONE_V2_DEFAULT_RADIUS_HELPER * s_boneRenderSettings._scaleRatio;
			s_boneRenderSettings._widthHalf_V2 = BONE_V2_DEFAULT_WIDTH_HALF * s_boneRenderSettings._scaleRatio;

			if(!isScaledByZoom)
			{
				s_boneRenderSettings._radius_V1_Org /= workspaceZoom;
				s_boneRenderSettings._radius_V2_Org /= workspaceZoom;
				s_boneRenderSettings._radius_V2_Helper /= workspaceZoom;
				s_boneRenderSettings._widthHalf_V2 /= workspaceZoom;
			}
		}

		public static float RenderSetting_ScaleRatio { get { return s_boneRenderSettings._scaleRatio; } }
		public static bool RenderSetting_IsScaledByZoom { get { return s_boneRenderSettings._isScaledByZoom; } }
		public static float RenderSetting_WorkspaceZoom { get { return s_boneRenderSettings._workspaceZoom; } }

		public static float RenderSetting_V1_Radius_Org { get { return s_boneRenderSettings._radius_V1_Org; } }
		public static float RenderSetting_V2_Radius_Org { get { return s_boneRenderSettings._radius_V2_Org; } }
		public static float RenderSetting_V2_Radius_Helper { get { return s_boneRenderSettings._radius_V2_Helper; } }
		public static float RenderSetting_V2_WidthHalf { get { return s_boneRenderSettings._widthHalf_V2; } }
		


		#region [미사용 코드] 별도의 클래스로 묶었다. 특히 V1, V2의 구조가 달라서 멤버가 너무 많아 복잡해짐
		//[NonSerialized]
		//public Vector2 _shapePointV1_End = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePointV1_Mid1 = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePointV1_Mid2 = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePointV1_End1 = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePointV1_End2 = Vector2.zero;


		//[NonSerialized]
		//public Vector2 _shapePointV1_End_IK = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePoint_Mid1_IK = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePoint_Mid2_IK = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePoint_End1_IK = Vector2.zero;
		//[NonSerialized]
		//public Vector2 _shapePoint_End2_IK = Vector2.zero; 
		#endregion


		//이전
		//[NonSerialized]
		//private bool _isBoneGUIVisible = true;//<<화면상에 숨길 수 있다. 어디까지나 임시. 메뉴를 열면 다시 True가 된다.

		//변경 21.1.28 : 본을 보이거나 숨기게 만들때, Rule과 Tmp 두가지로 나눈다. (Tmp가 우선순위 높음)
		//None 타입이 있다. 둘다 None이면 Show와 같다.
		public enum GUI_VISIBLE_TYPE { None, Show, Hide }
		
		[NonSerialized]
		private GUI_VISIBLE_TYPE _visibleType_Rule = GUI_VISIBLE_TYPE.None;//<<화면상에 숨길 수 있다. 어디까지나 임시. 메뉴를 열면 다시 True가 된다.

		[NonSerialized]
		private GUI_VISIBLE_TYPE _visibleType_Tmp = GUI_VISIBLE_TYPE.None;//<<화면상에 숨길 수 있다. 어디까지나 임시. 메뉴를 열면 다시 True가 된다.

		//Hierarchy에 어떤 아이콘으로 보이는지 여부
		//  (Rule)      :  (Tmp)
		//- Show/None   >  Show/None	: Current_Visible (Show)
		//- Show/None   >  Hide			: TmpWork_NonVisible (Hide)
		//- Hide        >  None         : Rule_NonVisible (Hide)
		//- Hide        >  Show         : TmpWork_Visible (Show)
		//- Hide        >  Hide         : Rule_NonVisible (Hide)
		public enum VISIBLE_COMBINATION_ICON
		{
			Visible_Default, Visible_Tmp, NonVisible_Tmp, NonVisible_Rule
		}




		// 본 옵션
		/// <summary>
		/// Parent Bone으로부터 결정된 기본 Position을 변경할 수 있는가?
		/// Parent의 IK 옵션에 따라 불가능할 수 있다.
		/// </summary>
		public enum OPTION_LOCAL_MOVE
		{
			/// <summary>
			/// 기본값. Local Move가 불가능하다.
			/// Default 제어를 제외하고는 IK로 처리되거나 아예 처리되지 않는다.
			/// </summary>
			Disabled = 0,
			/// <summary>
			/// Local Move가 가능하다. 
			/// 단, Parent의 OPTION_IK가 Disabled이거나 자신을 가리키면 허용되지 않는다.
			/// </summary>
			Enabled = 1
		}

		/// <summary>
		/// IK가 설정되어 있는가. 
		/// 자신을 기준으로 Tail 방향으로 값을 설정한다. 기본값은 IKSingle
		/// </summary>
		public enum OPTION_IK
		{
			/// <summary>IK를 처리하지 않고 FK로만 처리한다.</summary>
			Disabled = 0,

			/// <summary>
			/// 기본값. Child 본 1개에 대해서 IK로 처리한다.
			/// IK Head와 동일하지만 Chain 처리가 없다는 점에서 구분된다.
			/// </summary>
			IKSingle = 1,

			/// <summary>
			/// Chain된 IK 본의 시작부분이다.
			/// Head 이후의 Tail 까지의 본들은 자동으로 Chained 상태가 된다.
			/// </summary>
			IKHead = 2,

			/// <summary>
			/// Head-Tail IK 설정에 의해 강제로 IK가 설정되는 옵션.
			/// 하위에는 Tail이 존재하고, 상위에 Head가 존재한다.
			/// </summary>
			IKChained = 3
		}

		public OPTION_LOCAL_MOVE _optionLocalMove = OPTION_LOCAL_MOVE.Disabled;
		public OPTION_IK _optionIK = OPTION_IK.IKSingle;

		/// <summary>
		/// Parent로부터 IK의 대상이 되는가? IK Single일 때에도 Tail이 된다.
		/// (자신이 IK를 설정하는 것과는 무관함)
		/// </summary>
		public bool _isIKTail = false;

		//IK의 타겟과 Parent
		public int _IKTargetBoneID = -1;
		[NonSerialized]
		public apBone _IKTargetBone = null;

		/// <summary>
		/// IK target이 설정된 경우, IK Target을 포함하고 있는 Child Bone의 ID.
		/// 빠른 검색과 Chain 처리 목적
		/// </summary>
		public int _IKNextChainedBoneID = -1;

		[NonSerialized]
		public apBone _IKNextChainedBone = null;

		/// <summary>
		/// IK Tail이거나 IK Chained 상태라면 Header를 저장하고, Chaining 처리를 해야한다.
		/// </summary>
		public int _IKHeaderBoneID = -1;

		[NonSerialized]
		public apBone _IKHeaderBone = null;

		//IK가 설정된 후, Tail, Tail의 Head 처리는 Chain Refresh때 해주자


		//IK시 추가 옵션

		/// <summary>IK 적용시, 각도를 제한을 줄 것인가 (기본값 False)</summary>
		public bool _isIKAngleRange = false;
		public float _IKAngleRange_Lower = -90.0f;//음수여야 한다. => Upper보다 작지만 -360 ~ 360 각도를 가질 수 있다.
		public float _IKAngleRange_Upper = 90.0f;//양수여야 한다.
		public float _IKAnglePreferred = 0.0f;//선호하는 각도 Offset


		/// <summary>IK 연산이 되었는가</summary>
		[NonSerialized]
		public bool _isIKCalculated = false;

		/// <summary>IK 연산이 발생했을 경우, World 좌표계에서 Angle을 어떻게 만들어야 하는지 계산 결과값</summary>
		[NonSerialized]
		public float _IKRequestAngleResult = 0.0f;

		/// <summary>
		/// IK 계산을 해주는 Chain Set.
		/// </summary>
		[NonBackupField]
		private apBoneIKChainSet _IKChainSet = null;



		//에디터 변수 : Rigging Test가 작동중인가 (기본적으론 False)
		public bool _isRigTestPosing = false;

		//옵션
		public bool _isSocketEnabled = false;//런타임에서 소켓을 활성화할 것인가 (기본값 false)



		//에디터 변수
		//추가 3.22
		//Mod Lock 실행 여부를 저장한다.
		public enum EX_CALCULATE
		{
			///// <summary>기본 상태</summary>
			//Normal,
			///// <summary>Ex Edit 상태 중 "선택된 Modifier"에 포함된 상태</summary>
			//ExAdded,
			///// <summary>Ex Edit 상태 중, "선택된 Modifier"에 포함되지 않은 상태</summary>
			//ExNotAdded

			//변경 21.2.14
			/// <summary>활성화되어 있다. (편집 모드 아닐때)</summary>
			Enabled_Run,
			/// <summary>편집 모드에서 활성 상태이다.</summary>
			Enabled_Edit,
			/// <summary>편집 모드에서 비활성 상태</summary>
			Disabled_NotEdit,
			/// <summary>
			/// 추가된 상태 : 편집 모드에서 선택되지 않았지만, 옵션에 의해 다른 모디파이어가 적용된다.
			/// </summary>
			Disabled_ExRun,
		}

		[NonSerialized]
		public EX_CALCULATE _exCalculateMode = EX_CALCULATE.Enabled_Run;


		//추가 5.8
		//Position Controller와 LookAt Controller를 추가했다.
		[SerializeField]
		public apBoneIKController _IKController = null;

		[NonSerialized]
		public bool _isIKCalculated_Controlled = false;

		[NonSerialized]
		public float _IKRequestAngleResult_Controlled = 0.0f;

		[NonSerialized]
		public float _IKRequestWeight_Controlled = 0.0f;

		[NonSerialized]
		private bool _isIKRendered_Controller = false;

		//추가 5.26
		//미러 본을 연결할 수 있다.
		//미러본 끼리는 서로 연결된다. (한쪽이 끊을 수 있다.)
		[SerializeField]
		public int _mirrorBoneID = -1;//

		[NonSerialized]
		public apBone _mirrorBone = null;

		public enum MIRROR_OPTION
		{
			X = 0, Y = 1
		}
		[SerializeField]
		public MIRROR_OPTION _mirrorOption = MIRROR_OPTION.X;

		public float _mirrorCenterOffset = 0.0f;

		//추가 19.7.26 : 리깅 툴 v2에서 사용될 Rigging Lock
		[NonSerialized]
		public bool _isRigLock = false;


		//추가 20.5.23 : 지글 본 옵션
		public bool _isJiggle = false;
		public float _jiggle_Mass = 1.0f;//질량
		public float _jiggle_K = 50.0f;//복원력에 해당하는 k/m의 값
		public float _jiggle_Drag = 0.8f;//공기 저항. 값이 클 수록 이전 위치에 있으려고 한다.(0~1)
		public float _jiggle_Damping = 5.0f;//감속력에 해당하는 값. 값이 클 수록 금방 속도가 줄어든다. (0~1)
		public bool _isJiggleAngleConstraint = false;
		public float _jiggle_AngleLimit_Min = -30.0f;
		public float _jiggle_AngleLimit_Max = 30.0f;

		public const float JIGGLE_DEFAULT_MASS = 1.0f;
		public const float JIGGLE_DEFAULT_K = 50.0f;
		public const float JIGGLE_DEFAULT_DRAG = 0.8f;
		public const float JIGGLE_DEFAULT_DAMPING = 5.0f;
		public const float JIGGLE_DEFAULT_ANGLE_MIN = -30.0f;
		public const float JIGGLE_DEFAULT_ANGLE_MAX = 30.0f;

		[NonSerialized]
		private bool _isJiggleChecked_Prev = false;//이전 프레임에서 지글본을 위한 값이 저장되었는가

		[NonSerialized]
		private float _calJig_Angle_Result_Prev = 0.0f;//이전 프레임에서의 결과
		
		[NonSerialized]
		private Vector2 _calJig_EndPos_Prev = Vector2.zero;

		[NonSerialized]
		private float _calJig_Velocity = 0.0f;

		//임시 변수들은 static으로 만들자
		[NonSerialized]
		private static apBoneWorldMatrix s_calJig_Tmp_WorldMatrix = new apBoneWorldMatrix(null, apPortrait.ROOT_BONE_SCALE_METHOD.Default);


		//추가 22.7.4 : 지글본에도 가중치 옵션이 들어간다. 컨트롤 파라미터를 연결한다.
		[SerializeField]
		public bool _jiggle_IsControlParamWeight = false;

		[SerializeField]
		public int _jiggle_WeightControlParamID = -1;

		[NonSerialized]
		public apControlParam _linkedJiggleControlParam = null;


		// Init
		//--------------------------------------------
		/// <summary>
		/// 백업용 생성자.
		/// </summary>
		public apBone()
		{

		}


		public apBone(int uniqueID, int meshGroupID, string name)
		{
			//_name = "Bone " + uniqueID;
			_name = name;
			_uniqueID = uniqueID;
			_meshGroupID = meshGroupID;

			_childBoneIDs.Clear();
			_childBones.Clear();

			_mirrorBoneID = -1;
			_mirrorBone = null;
			_mirrorOption = MIRROR_OPTION.X;
			
			MakeRandomColor();

			_isRigLock = false;
		}



		/// <summary>
		/// 각종 옵션을 초기화한다.
		/// 링크한 후에 처리할 것
		/// 단일 Bone의 옵션 초기화시에는 호출가능하지만 일괄 초기화 시에는 호출하지 말자
		/// </summary>
		public void InitOption()
		{
			_optionLocalMove = OPTION_LOCAL_MOVE.Disabled;

			_optionIK = OPTION_IK.IKSingle;

			_isIKTail = false;

			if (_parentBoneID == -1)
			{
				//Parent가 없다면..
				//IK Tail은 해제하고 Local Move는 활성화한다.
				_optionLocalMove = OPTION_LOCAL_MOVE.Enabled;
			}

			//Child가 하나 있다면
			if (_childBoneIDs.Count == 1)
			{
				//IK Single로 설정한다.
				_optionIK = OPTION_IK.IKSingle;
				_IKTargetBoneID = _childBoneIDs[0];
				_IKTargetBone = _childBones[0];

				_IKNextChainedBoneID = _IKTargetBoneID;
				_IKNextChainedBone = _IKTargetBone;
			}
			else
			{
				//Child가 없거나 그 이상이라면
				//기본값으로는 IK를 해제한다.
				_optionIK = OPTION_IK.Disabled;
				_IKTargetBoneID = -1;
				_IKTargetBone = null;

				_IKNextChainedBoneID = -1;
				_IKNextChainedBone = null;
			}

			_IKHeaderBoneID = -1;
			_IKHeaderBone = null;

			if (_parentBone != null)
			{
				if (_parentBone._optionIK != OPTION_IK.Disabled &&
					_parentBone._IKNextChainedBoneID == _uniqueID)
				{
					//이 Bone을 대상으로 IK가 설정되어있다.
					if (_parentBone._optionIK == OPTION_IK.IKSingle ||
						_parentBone._optionIK == OPTION_IK.IKHead)
					{
						//Parent가 Head 또는 Single이라면
						_IKHeaderBone = _parentBone;
						_IKHeaderBoneID = _parentBoneID;
					}
					else if (_parentBone._optionIK == OPTION_IK.IKChained)
					{
						//Parent가 Chained 상태라면 Header를 찾아야한다.
						_IKHeaderBone = _parentBone._IKHeaderBone;
						_IKHeaderBoneID = _parentBone._IKHeaderBoneID;
					}

					_isIKTail = true;
				}
			}
		}

		public void MakeRandomColor()
		{
			float red = UnityEngine.Random.Range(0.2f, 1.0f);
			float green = UnityEngine.Random.Range(0.2f, 1.0f);
			float blue = UnityEngine.Random.Range(0.2f, 1.0f);
			float totalWeight = red + green + blue;
			if (totalWeight < 0.5f)
			{
				red *= 0.5f / totalWeight;
				green *= 0.5f / totalWeight;
				blue *= 0.5f / totalWeight;
			}

			_color = new Color(red, green, blue, 1.0f);
		}

		/// <summary>
		/// Link를 해준다. 소속되는 MeshGroup은 물론이고, ParentBone도 링크해준다.
		/// ParentBone이 있다면 해당 Bone의 Child 리스트에 이 Bone을 체크 후 추가한다.
		/// (Link는 Child -> Parent 로 참조를 한다. Add와 반대)
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="parentBone"></param>
		public void Link(apMeshGroup meshGroup, apBone parentBone, apPortrait portrait)
		{
			_meshGroup = meshGroup;
			_parentBone = parentBone;


			if (_childBones == null)
			{
				_childBones = new List<apBone>();
			}


			//일단 ID가 있는 Bone들은 meshGroup으로부터 값을 받아서 연결을 해준다.
			if (_IKTargetBoneID >= 0)
			{
				_IKTargetBone = _meshGroup.GetBone(_IKTargetBoneID);
				if (_IKTargetBone == null)
				{
					_IKTargetBoneID = -1;
				}
			}

			if (_IKNextChainedBoneID >= 0)
			{
				_IKNextChainedBone = _meshGroup.GetBone(_IKNextChainedBoneID);
				if (_IKNextChainedBone == null)
				{
					_IKNextChainedBoneID = -1;
				}
			}

			if (_IKHeaderBoneID >= 0)
			{
				_IKHeaderBone = _meshGroup.GetBone(_IKHeaderBoneID);
				if (_IKHeaderBone == null)
				{
					_IKHeaderBoneID = -1;
				}
			}

			if (_parentBone != null)
			{
				//호출 순서가 잘못되어서 parent의 List가 초기화가 안된 경우가 있다.
				if (_parentBone._childBones == null)
				{
					_parentBone._childBones = new List<apBone>();
				}

				if (!_parentBone._childBones.Contains(this))
				{
					_parentBone._childBones.Add(this);
				}
				if (!_parentBone._childBoneIDs.Contains(_uniqueID))
				{
					_parentBone._childBoneIDs.Add(_uniqueID);
				}
				_parentBoneID = _parentBone._uniqueID;
			}
			else
			{
				_parentBone = null;
				_parentBoneID = -1;
			}

			if (_rigTestMatrix == null)
			{
				_rigTestMatrix = new apMatrix();
			}
			_isRigTestPosing = false;

			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}

			//변경 20.8.15 : Portrait의 옵션에 따라 초기화 (Null이 아니더라도)
			apPortrait.ROOT_BONE_SCALE_METHOD rootBoneScaleMethod = portrait._rootBoneScaleMethod;
			
			if (_worldMatrix == null)	{ _worldMatrix = new apBoneWorldMatrix(this, rootBoneScaleMethod); }
			else						{ _worldMatrix.SetScaleMethod(rootBoneScaleMethod); }

			if(_worldMatrix_IK == null)	{ _worldMatrix_IK = new apBoneWorldMatrix(this, rootBoneScaleMethod); }
			else						{ _worldMatrix_IK.SetScaleMethod(rootBoneScaleMethod); }

			if (_worldMatrix_NonModified == null)	{ _worldMatrix_NonModified = new apBoneWorldMatrix(this, rootBoneScaleMethod); }
			else									{ _worldMatrix_NonModified.SetScaleMethod(rootBoneScaleMethod); }

			if(s_calJig_Tmp_WorldMatrix == null)	{ s_calJig_Tmp_WorldMatrix = new apBoneWorldMatrix(null, rootBoneScaleMethod); }
			else									{ s_calJig_Tmp_WorldMatrix.SetScaleMethod(rootBoneScaleMethod); }



			//값을 저장하자. IK 사용시 이 값을 참조한다.
			_rootBoneScaleMethod = rootBoneScaleMethod;


			//추가 20.8.23 : GUI용 행렬 추가
			if(_guiMatrix == null)		{ _guiMatrix = new apMatrix(); }
			if(_guiMatrix_IK == null)	{ _guiMatrix_IK = new apMatrix(); }


			
			//Default Angle이 제대로 적용되지 않는 경우가 있다.
			//Default는 -180 ~ 180 안에 들어간다
			_defaultMatrix._angleDeg = apUtil.AngleTo180(_defaultMatrix._angleDeg);


			//추가 5.8
			//Position/LookAt Controller를 Link하자
			if(_IKController == null)
			{
				_IKController = new apBoneIKController();
			}

			_IKController.Link(this, meshGroup, portrait);

			//추가 : 미러본
			if(_mirrorBoneID >= 0)
			{
				_mirrorBone = meshGroup.GetBone(_mirrorBoneID);
				if(_mirrorBone == null)
				{
					_mirrorBoneID = -1;
				}
			}
			else
			{
				_mirrorBone = null;
			}

			_isRigLock = false;//추가

			//추가 22.7.4 : 지글본 옵션에서 컨트롤 파라미터 연결하기
			//설정에서 바뀌면 다시 연결할 것
			_linkedJiggleControlParam = null;
			if(_jiggle_IsControlParamWeight
				&& _jiggle_WeightControlParamID >= 0)
			{
				_linkedJiggleControlParam = portrait._controller.FindParam(_jiggle_WeightControlParamID);
				if(_linkedJiggleControlParam == null)
				{
					_jiggle_WeightControlParamID = -1;
				}
			}
		}

		/// <summary>
		/// Position / LookAt의 IK Controller Validation을 체크한다.
		/// 모든 Link가 끝난 뒤여야 한다.
		/// </summary>
		public void CheckIKControllerValidation()
		{
			_IKController.CheckValidation();
		}

		/// <summary>
		/// 실제로 렌더링되는 RenderUnit을 넣어준다.
		/// RenderUnit의 Transform이 가장 기본이 되는 Root World Matrix다.
		/// </summary>
		/// <param name="renderUnit"></param>
		public void SetParentRenderUnit(apRenderUnit renderUnit)
		{
			_renderUnit = renderUnit;
		}

		/// <summary>
		/// Link시 Parent -> Child 순서로 호출하는 초기화 함수
		/// WorldMatrix의 레퍼런스를 전달해준다.
		/// </summary>
		public void LinkRecursive(int curLevel)
		{
			ReadyToUpdate(false);
			//if(_parentBone != null)
			//{
			//	SetParentMatrix(_parentBone._worldMatrix);
			//}
			if (_parentBone != null)
			{
				_renderUnit = _parentBone._renderUnit;
			}

			_level = curLevel;


			for (int i = 0; i < _childBones.Count; i++)
			{
				_childBones[i].LinkRecursive(curLevel + 1);
			}
		}
		public int SetBoneIndex(int index)
		{

			_recursiveIndex = index;

			int result = index;
			if (_childBones.Count > 0)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					result = _childBones[i].SetBoneIndex(result + 1);
				}
			}
			return result;
		}



		/// <summary>
		/// Bone Chaining 직후에 재귀적으로 호출한다.
		/// Tail이 가지는 -> Head로의 IK 리스트를 만든다.
		/// </summary>
		public void LinkBoneChaining()
		{


			if (_isIKTail)
			{
				apBone curParentBone = _parentBone;
				apBone headBone = _IKHeaderBone;

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
						_IKChainSet = new apBoneIKChainSet(this);
					}
					//Chain을 Refresh한다.
					_IKChainSet.RefreshChain();
				}
				else
				{
					_IKChainSet = null;

					Debug.LogError("[" + _name + "] IK Chaining Error : Parent -> Chain List 연결시 데이터가 누락되었다. "
						+ "[ Parent : " + isParentExist
						+ " / Header : " + isHeaderExist
						+ " / IsHeader Is In Parent : " + isHeaderIsInParents + " ]");
				}
			}
			else
			{
				_IKChainSet = null;
			}

			for (int i = 0; i < _childBones.Count; i++)
			{
				_childBones[i].LinkBoneChaining();
			}

		}

		/// <summary>
		/// Transform을 모두 초기화한다. Default 포함 (업데이트때는 호출하지 말것)
		/// </summary>
		public void InitTransform(apPortrait portrait)
		{
			_defaultMatrix.SetIdentity();

			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}
			_localMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			
			//변경 20.8.15 : Portrait의 옵션에 따라 설정하자
			apPortrait.ROOT_BONE_SCALE_METHOD rootBoneScaleMethod = portrait._rootBoneScaleMethod;
			if (_worldMatrix == null)	{ _worldMatrix = new apBoneWorldMatrix(this, rootBoneScaleMethod); }
			else						{ _worldMatrix.SetScaleMethod(rootBoneScaleMethod); }

			if(_worldMatrix_IK == null)	{ _worldMatrix_IK = new apBoneWorldMatrix(this, rootBoneScaleMethod); }
			else						{ _worldMatrix_IK.SetScaleMethod(rootBoneScaleMethod); }

			if (_worldMatrix_NonModified == null)	{ _worldMatrix_NonModified = new apBoneWorldMatrix(this, rootBoneScaleMethod); }
			else									{ _worldMatrix_NonModified.SetScaleMethod(rootBoneScaleMethod); }

			_calculatedBoneIKWeight = 0.0f;

			_rootBoneScaleMethod = rootBoneScaleMethod;

			_worldMatrix.SetIdentity();
			_worldMatrix_NonModified.SetIdentity();
			
			_worldMatrix_IK.SetIdentity();

			//추가 20.8.23 : GUI용 행렬 추가
			if(_guiMatrix == null)		{ _guiMatrix = new apMatrix(); }
			if(_guiMatrix_IK == null)	{ _guiMatrix_IK = new apMatrix(); }
		}




		// Update
		//-----------------------------------------------------
		/// <summary>
		/// 1) Update Transform Matrix를 초기화한다.
		/// </summary>
		public void ReadyToUpdate(bool isRecursive)
		{
			//_localModifiedTransformMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_isIKCalculated = false;
			_IKRequestAngleResult = 0.0f;

			_isIKCalculated_Controlled = false;
			_IKRequestAngleResult_Controlled = 0.0f;
			_IKRequestWeight_Controlled = 0.0f;
			_isIKRendered_Controller = false;

			_calculatedBoneIKWeight = 0.0f;//<<추가

			if(_IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None)
			{
				_calculatedBoneIKWeight = _IKController._defaultMixWeight;
			}

			//_worldMatrix.SetIdentity();
			if (isRecursive)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].ReadyToUpdate(true);
				}
			}



		}

		/// <summary>
		/// 2) Update된 TRS 값을 넣는다.
		/// </summary>
		/// <param name="deltaPos"></param>
		/// <param name="deltaAngle"></param>
		/// <param name="deltaScale"></param>
		public void UpdateModifiedValue(Vector2 deltaPos, float deltaAngle, Vector2 deltaScale)
		{
			_deltaPos = deltaPos;
			_deltaAngle = deltaAngle;
			_deltaScale = deltaScale;
		}


		public void AddIKAngle(float IKAngle)
		{
			_isIKCalculated = true;

			//기존
			_IKRequestAngleResult += IKAngle;

			////변경 : Y 반전시는 IK 각도가 반대로 들어가야 한다.
			//if(_worldMatrix.Scale.y < 0.0f)
			//{
			//	_IKRequestAngleResult -= IKAngle;
			//}
			//else
			//{
			//	_IKRequestAngleResult += IKAngle;
			//}
		}

		public void AddIKAngle_Controlled(float IKAngle, float weight)
		{
			_isIKCalculated_Controlled = true;
			_isIKRendered_Controller = true;
			
			//기존
			_IKRequestAngleResult_Controlled += (IKAngle) * weight;

			////변경 : Y 반전시는 IK 각도가 반대로 들어가야 한다.
			//if(_worldMatrix.Scale.y < 0.0f)
			//{
			//	_IKRequestAngleResult_Controlled -= (IKAngle) * weight;
			//}
			//else
			//{
			//	_IKRequestAngleResult_Controlled += (IKAngle) * weight;
			//}


			_IKRequestWeight_Controlled += weight;
		}

		/// <summary>
		/// 4) World Matrix를 만든다.
		/// 이 함수는 Parent의 MeshGroupTransform이 연산된 후 -> Vertex가 연산되기 전에 호출되어야 한다.
		/// </summary>
		public void MakeWorldMatrix(bool isRecursive/*, bool isDebug = false*/)
		{
			_localMatrix.SetIdentity();
			_localMatrix._pos = _deltaPos;
			_localMatrix._angleDeg = _deltaAngle;
			_localMatrix._scale.x = _deltaScale.x;
			_localMatrix._scale.y = _deltaScale.y;

			_localMatrix.MakeMatrix();

			//World Matrix = ParentMatrix x LocalMatrix
			//Root인 경우에는 MeshGroup의 Matrix를 이용하자


			//변경 20.8.15 : 래핑된 WorldMatrix 업데이트 <중요>
			if(_isRigTestPosing)
			{
				//리깅의 포즈 테스트 중이라면
				_worldMatrix.MakeWorldMatrix_ModRig(	
											_localMatrix, _rigTestMatrix, 
											(_parentBone != null ? _parentBone._worldMatrix : null),
											(_renderUnit != null ? _renderUnit.WorldMatrixWrap : null)
											);
			}
			else
			{
				//일반 모디파이어 적용식
				_worldMatrix.MakeWorldMatrix_Mod(
											_localMatrix, 
											(_parentBone != null ? _parentBone._worldMatrix : null),
											(_renderUnit != null ? _renderUnit.WorldMatrixWrap : null)
											//, isDebug
											);
			}

			//모디파이어가 적용안된 WorldMatrix 업데이트
			_worldMatrix_NonModified.MakeWorldMatrix_NoMod(
											(_parentBone != null ? _parentBone._worldMatrix_NonModified : null),
											(_renderUnit != null ? _renderUnit.WorldMatrixWrapWithoutModified : null)
											);
			






			//Child도 호출해준다.
			if (isRecursive)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].MakeWorldMatrix(true);
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

			if (_IKChainSet != null)
			{
				//Control Param의 영향을 받는다.
				if (_IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None
					&& _IKController._isWeightByControlParam
					&& _IKController._weightControlParam != null)
				{
					_calculatedBoneIKWeight = Mathf.Clamp01(_IKController._weightControlParam._float_Cur);
				}

				if (_calculatedBoneIKWeight > 0.001f
					&& _IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None
					)
				{
					if (_IKController._controllerType == apBoneIKController.CONTROLLER_TYPE.Position)
					{
						//1. Position 타입일 때
						if (_IKController._effectorBone != null)
						{
							//논리상 EffectorBone은 IK의 영향을 받으면 안된다. (IK가 적용 안된 WorldMatrix를 사용)
							//bool result = _IKChainSet.SimulateIK(_IKController._effectorBone._worldMatrix._pos, true, true);//이전
							//bool result = _IKChainSet.SimulateIK(_IKController._effectorBone._worldMatrix.Pos, true, true);//변경 20.8.17 : 래핑
							
							bool result = _IKChainSet.SimulateIK(_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos), true, true);//변경 20.8.25 : IK Space로 이동


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
							//&& _IKController._startBone != null
							)
						{
							//기존 : 일반적으로는 문제 없는 코드
							//bool result = _IKChainSet.SimulateLookAtIK(_IKController._effectorBone._worldMatrix_NonModified._pos, _IKController._effectorBone._worldMatrix._pos, true, true);

							//변경 20.8.6 [Scale 이슈 해결]
							//만약 Effector 본이 "모디파이어나 상위 객체에 의해서" 반전된 경우에는 위치가 반전되어야 한다.
							//이걸 포함해서 LookAt의 기본이 되는 Default Pos를 계산하는 수식을 보강했다.
							//- 만약 어딘가에서 "스케일이 반전된 경우", Modified가 적용 안된 Effector 본의 위치가 적절하지 않을 수 있다.
							//- 따라서 약간 변경해서, "현재 본"을 기준으로 "Modified가 적용 안된 상태에서의 상대 위치값"을 "Modified가 적용된 Matrix를 기준으로 적용하여 World좌표계의 위치"를 구한다.
							
							//기존 방식
							//Vector2 defaultEffectorPos = _worldMatrix.MulPoint2(_worldMatrix_NonModified.InvMulPoint2(_IKController._effectorBone._worldMatrix_NonModified._pos));
							//bool result = _IKChainSet.SimulateLookAtIK(defaultEffectorPos, _IKController._effectorBone._worldMatrix._pos, true, true);

							//변경 20.8.17 : 래핑된 코드
							Vector2 defaultEffectorPos = _worldMatrix.MulPoint2(_worldMatrix_NonModified.InvMulPoint2(_IKController._effectorBone._worldMatrix_NonModified.Pos));
							//bool result = _IKChainSet.SimulateLookAtIK(defaultEffectorPos, _IKController._effectorBone._worldMatrix.Pos, true, true);
							bool result = _IKChainSet.SimulateLookAtIK(_worldMatrix.ConvertForIK(defaultEffectorPos), 
																		_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos), 
																		true, true);//IK Space로 이동


							

							if (result)
							{
								IKCalculated = true;
								_IKChainSet.AdaptIKResultToBones_ByController(_calculatedBoneIKWeight);

								//이 Bone은 그냥 바라보도록 한다.
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
								//이전
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

								//변경 20.8.17 : 래핑된 코드
								//float tailLookAngleFromVector = apBoneIKChainUnit.Vector2Angle(_IKController._effectorBone._worldMatrix.Pos - _IKChainSet._tailBoneNextPosW);
								
								//다시 변경 20.8.25 : IKSpace로 이동
								float tailLookAngleFromVector = apBoneIKChainUnit.Vector2Angle(
																	_worldMatrix.ConvertForIK(_IKController._effectorBone._worldMatrix.Pos) - _IKChainSet._tailBoneNextPosW);

								

								if (_worldMatrix.Scale.y < 0.0f)	{ tailLookAngleFromVector += 90.0f; }//각도 반전
								else								{ tailLookAngleFromVector -= 90.0f; }
								
								//AddIKAngle_Controlled(	apUtil.AngleTo180(tailLookAngleFromVector - _worldMatrix.Angle), _calculatedBoneIKWeight );
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
				if(_IKController != null && _IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None)
				{
					//Debug.LogError("IK Controller Not Work : [" + _IKController._controllerType + "]");
					if(_IKController._controllerType == apBoneIKController.CONTROLLER_TYPE.LookAt)
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
				for (int i = 0; i < _childBones.Count; i++)
				{
					if(_childBones[i].CalculateIK(true))
					{
						//자식 본 중에 처리 결과가 True라면
						//나중에 전체 True 처리
						IKCalculated = true;
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
		public void MakeWorldMatrixForIK(bool isRecursive, bool isCalculateMatrixForce, bool isPhysics, float tDelta)//추가 20.7.9 : 물리 속성 추가
		{
			if(_isIKCalculated_Controlled)
			{
				//IK가 계산된 결과를 넣자

				//-----------------------------------
				// 기존 코드 : 래핑 전 코드
				//-----------------------------------
				#region [미사용 코드] 이전 방식 (래핑 전)
				////----------------------------
				//// 기존 World Matrix 연산 코드 (RMultiply)
				////----------------------------
				////_worldMatrix_IK.SetMatrix(_defaultMatrix);
				////_worldMatrix_IK.Add(_localMatrix);


				//////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
				////_worldMatrix_IK.OnBeforeRMultiply();

				////if (_parentBone == null)
				////{
				////	if (_renderUnit != null)
				////	{
				////		_worldMatrix_IK.RMultiply(_renderUnit.WorldMatrixWrap);
				////	}
				////}
				////else
				////{
				////	_worldMatrix_IK.RMultiply(_parentBone._worldMatrix_IK);
				////}
				////----------------------------

				////----------------------------
				//// 변경 20.8.12
				//// 변경된 World Matrix 연산 코드 (RMultiply + SMultiply => ComplexMatrix)
				////----------------------------
				//_worldMatrix_IK.SetMatrix_Step1(_defaultMatrix, false);
				//_worldMatrix_IK.Add(_localMatrix);


				////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
				//_worldMatrix_IK.OnBeforeMultiply();

				//if (_parentBone == null)
				//{
				//	if (_renderUnit != null)
				//	{
				//		//<중요> RenderUnit의 Matrix는 RMultiply대신 SMultiply를 적용해서 Skew를 재현해야한다.
				//		_worldMatrix_IK.SMultiply(_renderUnit.WorldMatrixWrap, true);
				//	}
				//	else
				//	{
				//		_worldMatrix_IK.MakeMatrix();
				//	}
				//}
				//else
				//{
				//	//<중요> 부모 본에 Skew가 적용된 Matrix를 연산해야하므로,
				//	//RMultiply + SMultiply가 적용된 ComplexMultiply를 이용한다.
				//	_worldMatrix_IK.ComplexMultiply(_parentBone._worldMatrix_IK, true);
				//}




				////IK 적용

				////추가 20.8.6 : [RMultiply Scale 이슈]
				////플립된 상태라면, 실제 Angle은 반전되어야 한다.
				//float prevAngleW = _worldMatrix._angleDeg;
				//float addIKAngle = (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled);

				//float nextIKAngle = 0.0f;

				//if (_IKRequestWeight_Controlled > 1.0f)
				//{
				//	nextIKAngle = prevAngleW + addIKAngle;
				//}
				//else if (_IKRequestWeight_Controlled > 0.0f)
				//{
				//	//Slerp가 적용된 코드
				//	nextIKAngle = apUtil.AngleSlerp(	prevAngleW,
				//										prevAngleW + addIKAngle,
				//										_IKRequestWeight_Controlled);
				//}

				////_worldMatrix_IK.SetRotate(nextIKAngle);//기본
				//_worldMatrix_IK.RotateAsPostResult(nextIKAngle - _worldMatrix_IK._angleDeg);//변경 20.8.13 : ComplexMatrix 방식 
				#endregion


				//-----------------------------------
				//변경 20.8.17 : 래핑된 코드
				//-----------------------------------
				//IK 적용

				//추가 20.8.6 : [RMultiply Scale 이슈]
				//플립된 상태라면, 실제 Angle은 반전되어야 한다.
				//float prevAngleW = _worldMatrix.Angle;
				float prevAngleW = _worldMatrix.Angle_IKSpace;
				float addIKAngle = (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled);
				
				float nextIKAngle = 0.0f;

				if (_IKRequestWeight_Controlled > 1.0f)
				{
					nextIKAngle = prevAngleW + addIKAngle;
				}
				else if (_IKRequestWeight_Controlled > 0.0f)
				{
					//Slerp가 적용된 코드
					nextIKAngle = apUtil.AngleSlerp(	prevAngleW,
														prevAngleW + addIKAngle,
														_IKRequestWeight_Controlled);
				}

				_worldMatrix_IK.MakeWorldMatrix_IK(	_localMatrix,
													(_parentBone != null ? _parentBone._worldMatrix_IK : null),
													(_renderUnit != null ? _renderUnit.WorldMatrixWrap : null),
													nextIKAngle);



				//-----------------------------------
				isCalculateMatrixForce = true;//<<다음 Child 부터는 무조건 갱신을 해야한다.
			}
			else if(isCalculateMatrixForce)
			{
				//Debug.Log("IK Force [" + _name + "] : " + _IKRequestAngleResult_Controlled);

				//IK 자체는 적용되지 않았으나, Parent에서 적용된게 있어서 WorldMatrix를 그대로 쓸 순 없다.

				#region [미사용 코드] 래핑 전
				////----------------------------
				//// 기존 World Matrix 연산 코드 (RMultiply)
				////----------------------------
				////_worldMatrix_IK.SetMatrix(_defaultMatrix);
				////_worldMatrix_IK.Add(_localMatrix);


				//////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
				////_worldMatrix_IK.OnBeforeRMultiply();

				////if (_parentBone == null)
				////{
				////	if (_renderUnit != null)
				////	{
				////		_worldMatrix_IK.RMultiply(_renderUnit.WorldMatrixWrap);
				////	}
				////}
				////else
				////{
				////	_worldMatrix_IK.RMultiply(_parentBone._worldMatrix_IK);
				////}
				////----------------------------


				////----------------------------
				//// 변경 20.8.12
				//// 변경된 World Matrix 연산 코드 (RMultiply + SMultiply => ComplexMatrix)
				////----------------------------
				//_worldMatrix_IK.SetMatrix_Step1(_defaultMatrix, false);
				//_worldMatrix_IK.Add(_localMatrix);


				////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
				//_worldMatrix_IK.OnBeforeMultiply();

				//if (_parentBone == null)
				//{
				//	if (_renderUnit != null)
				//	{
				//		//<중요> RMultiply가 아닌 SMultiply를 해야한다.
				//		_worldMatrix_IK.SMultiply(_renderUnit.WorldMatrixWrap, true);
				//	}
				//	else
				//	{
				//		_worldMatrix_IK.MakeMatrix();
				//	}
				//}
				//else
				//{
				//	//<중요> 부모 본에 SMultiply가 포함되어 있을 것이므로
				//	//ComplexMultiply를 해야한다.
				//	_worldMatrix_IK.ComplexMultiply(_parentBone._worldMatrix_IK, true);
				//}
				////---------------------------- 
				#endregion

				//--------------------------------------
				// 변경 20.8.17 : 래핑 후 코드
				// Mod의 LocalMatrix만 적용한다.
				//--------------------------------------
				_worldMatrix_IK.MakeWorldMatrix_Mod(_localMatrix, 
													(_parentBone != null ? _parentBone._worldMatrix_IK : null),
													(_renderUnit != null ? _renderUnit.WorldMatrixWrap : null));


				_isIKRendered_Controller = true;
			}
			else
			{
				//World Matrix와 동일하다.

				//------------------------------
				// 이전 방식 : 래핑 전 코드
				//------------------------------
				#region [미사용 코드] 이전 방식
				////--------------------------
				////이전 : 기본 방식
				////--------------------------
				////_worldMatrix_IK.SetMatrix(_worldMatrix);//<<동일하다.

				//////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
				////_worldMatrix_IK.OnBeforeRMultiply();
				////--------------------------

				////--------------------------
				////변경 20.8.12 : ComplexMatrix로 변경
				//_worldMatrix_IK.CopyFromComplexMatrix(_worldMatrix);
				////-------------------------- 
				#endregion

				//------------------------------
				// 변경 20.8.17 : 래핑 후 코드
				//------------------------------
				_worldMatrix_IK.CopyFromMatrix(_worldMatrix);
			}



			//추가 20.5.23 : 지글본이 계산된 경우 WorldMatrix_IK를 변경하자
			//추가 20.5.23 : 지글본
			//지글 본은 계층 처리가 없다.
			//헬퍼는 지글본일 수가 없다.
			//길이가 1 이상이어야 한다.
			
			if(_isJiggle && isPhysics && !_shapeHelper && _shapeLength > 0)
			{
				//Debug.Log("[" + _name + "] Jiggle Update (" + tDelta + ")");
				if (!_isJiggleChecked_Prev)
				{
					//Prev 기록이 없다면, 초기화
					_calJig_Velocity = 0.0f;
					_calJig_Angle_Result_Prev = 0.0f;
					_calJig_EndPos_Prev = Vector2.zero;

					//Debug.LogError(">> No Prev Data");
				}
				else
				{
					//지글본에 따라서 WorldMatrix_IK를 수정하자
					float calJig_Angle_Result = _calJig_Angle_Result_Prev;


					if (tDelta > 0.0f)
					{
						//계산은 tDelta가 0 유효할 때에만

						//추가 22.7.5 [v1.4.0] : 지글 Weight를 지정할 수 있다.
						//다만 조건이 맞지 않는 경우엔 지정 불가
						//가중치가 적용되는 경우엔, 회전 가능한 범위가 줄어들고, 복원력이 줄어든다. (안그러면 빠르게 흔들리는 것을 볼 수 있다.)
						//가중치에 따른 지글 줄어드는 과정
						//- Damping : 가중치가 증가할 수록 Damping에 최대 10배의 값이 곱해진다.
						//- 회전 가능한 범위 : 0.3배까지 줄어든다.
						//- 실제 적용 정도 : 0.3까지는 반영되지 않다가, 0.3부터는 역지수 그래프를 이용하여 줄어든다.
						//- K 값도 줄어든다.
						bool isWeighted = false;
						float adaptWeight = 1.0f;
						float biasWeightCutout = 0.998f;
						
						if(_jiggle_IsControlParamWeight)
						{
							//컨트롤 파라미터인 경우 (Float타입 한정)
							if (_linkedJiggleControlParam != null)
							{
								if(_linkedJiggleControlParam._valueType == apControlParam.TYPE.Float)
								{
									if(_linkedJiggleControlParam._float_Cur < biasWeightCutout)
									{
										isWeighted = true;
										adaptWeight = Mathf.Clamp01(_linkedJiggleControlParam._float_Cur);
									}
								}
							}
						}



						//계산 순서
						//1. Drag를 기준으로 dAngle_FromPrev 만들기 (방향은 > Prev)
						//2. dAngle_woJiggle 바탕으로 복원력(-kx)을 계산하고 속력에 더하기 (방향은 > Cur)
						//3. 속력의 Drag를 더해서 감속하기
						//4. dAngle_woJiggle + (Vt) 결과가 최종 dAngle_Cur


						//1. Drag를 기준으로 dAngle_FromPrev 만들기 (방향은 > Prev)
						//- 현재의 Matrix + 이전 프레임의 dAngle를 더한 값으로 [Expected Pos]를 계산한다.
						//- Prev Pos > Expected Pos가 예상 움직임 내역
						//- 각도를 계산하고, Drag를 곱해서 변화량을 줄이자 (dAngle_Drag)

						//------------------------
						// 이전
						//------------------------
						//s_calJig_Tmp_WorldMatrix.SetMatrix(_worldMatrix_IK);

						//------------------------
						// Complex 코드 적용
						//------------------------

						//이전 방식
						//s_calJig_Tmp_WorldMatrix.SetTRS(_worldMatrix_IK._pos, _worldMatrix_IK._angleDeg, _worldMatrix_IK._scale);

						//변경 20.8.17 : 래핑된 코드
						//s_calJig_Tmp_WorldMatrix.SetTRS(_worldMatrix_IK.Pos, _worldMatrix_IK.Angle, _worldMatrix_IK.Scale, false);


						////추가 20.8.6 : RMultiply 전에 함수를 호출해야한다. [RMultiply Scale 이슈]
						//s_calJig_Tmp_WorldMatrix.OnBeforeRMultiply();


						//s_calJig_Tmp_WorldMatrix._angleDeg += _calJig_Angle_Result_Prev;
						//s_calJig_Tmp_WorldMatrix.MakeMatrix();

						//------------------------
						//다시 변경 20.8.29 : BoneWorldMatrix 방식과 IKSpace 이용하기
						//------------------------
						s_calJig_Tmp_WorldMatrix.CopyFromMatrix(_worldMatrix_IK);
						s_calJig_Tmp_WorldMatrix.RotateAsStep1(_calJig_Angle_Result_Prev, true);




						//예상 위치
						Vector2 endPos_Excepted = s_calJig_Tmp_WorldMatrix.MulPoint2(new Vector2(0, _shapeLength));

						//위치가 유사하지 않다면) 끝점 위치 변화를 감지하자
						float angle_Exp2Prev = 0.0f;

						//외력은 Bone의 Normal 방향일때(각도 90도)일때 최대이며, 0도 일수록 작아져야 한다. (각도차이가 크더라도)
						float normalDeltaRatio = 0.0f;

						if (Mathf.Abs(endPos_Excepted.x - _calJig_EndPos_Prev.x) > 0.001f ||
							Mathf.Abs(endPos_Excepted.y - _calJig_EndPos_Prev.y) > 0.001f)
						{
							Vector2 start2End_Prev = _calJig_EndPos_Prev - s_calJig_Tmp_WorldMatrix.Pos;
							Vector2 start2End_Expected = endPos_Excepted - s_calJig_Tmp_WorldMatrix.Pos;

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

							//Debug.Log("[" + _name + "] Delta Angle : " + angle_Exp2Prev);
						}

						//Drag를 곱하여 계산 + Prev를 더한다.
						//공기 저항이 클 수록 이전 위치에 있으려고 한다.
						float dAngle_Drag = (angle_Exp2Prev * _jiggle_Drag);

						//Jiggle 전과 후의 각도 차이
						//float dAngle_woJiggle = dAngle_Drag + _calJig_Angle_Result_Prev;//이전
						float dAngle_woJiggle = (dAngle_Drag * normalDeltaRatio) + _calJig_Angle_Result_Prev;//변경. 노멀 방향의 움직임에 강향 영향을 받는다.

						if (dAngle_woJiggle < -180.0f)
						{
							//Debug.Log("[" + _name + "] 결과 각도 제한 넘어감 : " + angle_Exp2Prev);
							dAngle_woJiggle += 360.0f;
						}
						else if (dAngle_woJiggle > 180.0f)
						{
							//Debug.Log("[" + _name + "] 결과 각도 제한 넘어감 : " + angle_Exp2Prev);
							dAngle_woJiggle -= 360.0f;
						}

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
						if (isWeighted)
						{
							//회전 가능한 범위 : 0.3배까지 줄어든다.
							float weightedAngle_Min = Mathf.Clamp(minAngle * 0.1f, -5.0f, 0.0f);
							float weightedAngle_Max = Mathf.Clamp(maxAngle * 0.1f, 0.0f, 5.0f);

							if (adaptWeight < 0.3f)
							{
								minAngle = weightedAngle_Min;
								maxAngle = weightedAngle_Max;
							}
							else
							{
								//0.3~1 Weight에서는 "제한된 범위 ~ 원래 범위"로 축소된다.
								float lerp = Mathf.Clamp01((adaptWeight - 0.3f) / 0.7f);
								minAngle = weightedAngle_Min * (1.0f - lerp) + (minAngle * lerp);
								maxAngle = weightedAngle_Max * (1.0f - lerp) + (maxAngle * lerp);
							}
						}

						//Result 계산
						calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);



						#region [미사용 코드] 외부 힘에 의한 범위 제한이 제대로 계산되지 않는다.
						//if(calJig_Angle_Result < 0.0f && _calJig_Velocity < 0.0f)
						//{
						//	//Min과의 거리를 보자
						//	if(calJig_Angle_Result < minAngle)
						//	{
						//		//거리 제한
						//		calJig_Angle_Result = minAngle;
						//		_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
						//	}
						//	else if(calJig_Angle_Result < minAngle * 0.7f)
						//	{
						//		//70% 구간부터는 감속을 한다.
						//		//70% : x1 > 100% : x0
						//		_calJig_Velocity *= 1.0f - ((calJig_Angle_Result - (minAngle * 0.7f)) / (minAngle * 0.3f));
						//		calJig_Angle_Result = dAngle_woJiggle + (_calJig_Velocity * tDelta);
						//	}
						//}
						//else if(calJig_Angle_Result > 0.0f && _calJig_Velocity > 0.0f)
						//{
						//	//Max와의 거리를 보자
						//	if(calJig_Angle_Result > maxAngle)
						//	{
						//		//거리 제한
						//		calJig_Angle_Result = maxAngle;
						//		_calJig_Velocity = (calJig_Angle_Result - dAngle_woJiggle) / tDelta;
						//	}
						//	else if(calJig_Angle_Result > maxAngle * 0.7f)
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


						//- 실제 적용 정도 : 0.5까지는 반영되지 않다가, 0.5부터는 역지수 그래프를 이용하여 줄어든다.
						if(isWeighted)
						{
							float lerpWeight = Mathf.Clamp01(adaptWeight / tDelta);
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


					//이전 코드
					//_worldMatrix_IK._angleDeg += calJig_Angle_Result;
					//_worldMatrix_IK.MakeMatrix();



					

					//변경 20.8.17 : 래핑된 코드
					//_worldMatrix_IK.RotateAsResult(calJig_Angle_Result);//결과에 값을 추가하는 코드 > 행렬 계산에 문제가 있다.
					//결과에 추가하는게 아니라, Step1에 추가해야한다.
					_worldMatrix_IK.RotateAsStep1(calJig_Angle_Result, true);
					



					//현재 End 위치를 갱신하자
					_calJig_EndPos_Prev = _worldMatrix_IK.MulPoint2(new Vector2(0, _shapeLength));
					_calJig_Angle_Result_Prev = calJig_Angle_Result;
				}

				_isJiggleChecked_Prev = true;
				isCalculateMatrixForce = true;
				_isIKRendered_Controller = true;
			}

			

			//자식 본도 업데이트
			if(isRecursive)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].MakeWorldMatrixForIK(true, isCalculateMatrixForce, isPhysics, tDelta);
				}
			}
		}



		//추가 19.8.16 : 단일 본에 대한 LookAt 처리
		private bool CalculateSingleLookAtIK()
		{
			if (_IKController == null)
			{
				return false;
			}

			if (_IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.LookAt)
			{
				return false;
			}

			if (_IKController._effectorBone == null)
			{
				return false;
			}

			//Control Param의 영향을 받는다.
			if (_IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None
				&& _IKController._isWeightByControlParam
				&& _IKController._weightControlParam != null)
			{
				_calculatedBoneIKWeight = Mathf.Clamp01(_IKController._weightControlParam._float_Cur);
			}

			if (_calculatedBoneIKWeight <= 0.001f)
			{
				return false;
			}

			//이전
			//Vector2 srcPos = _worldMatrix_IK._pos;
			//Vector2 dstPos = _IKController._effectorBone._worldMatrix._pos;

			//변경 20.8.17 : 래핑 후 코드
			Vector2 srcPos = _worldMatrix_IK.Pos;
			Vector2 dstPos = _IKController._effectorBone._worldMatrix.Pos;

			float dist = Vector2.Distance(srcPos, dstPos);
			if (dist < 0.00001f)
			{
				return false;
			}
			float angleToDst = Mathf.Atan2(dstPos.y - srcPos.y, dstPos.x - srcPos.x);
			angleToDst *= Mathf.Rad2Deg;

			//기존
			//angleToDst -= 90.0f;

			//변경 20.8.8 [Scale 문제]
			//if (_worldMatrix._scale.y < 0.0f)
			if (_worldMatrix.Scale.y < 0.0f)//래핑 후 코드 20.8.17
			{
				//Y스케일이 반전된 경우
				angleToDst += 90.0f;
			}
			else
			{
				angleToDst -= 90.0f;
			}

			//angleToDst -= _worldMatrix._angleDeg;//이전
			angleToDst -= _worldMatrix.Angle;//래핑 후 코드
			angleToDst = apUtil.AngleTo180(angleToDst);

			AddIKAngle_Controlled(angleToDst, _calculatedBoneIKWeight);

			//Debug.LogWarning("[" + _name + "] Single LookAt : " + angleToDst);

			return true;
		}


		

		/// <summary>
		/// 5) GUI 편집시에는 이 함수까지 호출한다. GUI용 Bone 이미지 데이터가 갱신된다.
		/// 이 부분은 Render 부분에서 호출해도 된다.
		/// </summary>
		public void GUIUpdate(bool isRecursive = false, bool isBoneIKUsing = false)
		{
			//추가 20.8.23 : GUI용 Matrix를 별도로 만든다.
			if(_guiMatrix == null)		{ _guiMatrix = new apMatrix(); }
			if(_guiMatrix_IK == null)	{ _guiMatrix_IK = new apMatrix(); }
			

			//----------------------------------
			// GUIMatrix 계산하기 (20.8.23)
			//----------------------------------
			//찌그러짐이 없이 TRS를 구해야한다.
			//StartPos > EndPos를 구하고, 이걸 역으로 계산하여 ScaleY. Angle을 구한다.
			//ScaleX는 성분 그대로를 이용하고, Pos는 Matrix의 값을 이용하면 된다.
			//Helper의 경우는 Pos를 제외하고 SR 성분을 그대로 이용한다.
			//IK가 적용안된 경우는 행렬 복사
			_guiMatrix.SetIdentity();
			_guiMatrix_IK.SetIdentity();

			bool isIKMatrix = isBoneIKUsing && (_isIKCalculated_Controlled || _isIKRendered_Controller);

			if (_shapeHelper)
			{
				//헬퍼라면 성분을 그대로 이용한다.
				_guiMatrix.SetTRS(_worldMatrix.Pos, _worldMatrix.Angle, _worldMatrix.Scale, true);

				
				if (isIKMatrix)
				{
					//헬퍼지만 IK가 적용되어 있다면
					_guiMatrix_IK.SetTRS(_worldMatrix_IK.Pos, _worldMatrix_IK.Angle, _worldMatrix_IK.Scale, true);
				}
				else
				{
					//그렇지 않다면 그냥 복사
					_guiMatrix_IK.SetMatrix(_guiMatrix, true);
				}
				
			}
			else
			{
				//헬퍼가 아니라면 EndPos를 구해서 계산하자
				//위치는 기본 World Matrix에서 계산
				Vector2 startPos = _worldMatrix.Pos;				
				Vector2 endPos = _worldMatrix.MulPoint2(new Vector2(0, _shapeLength));

				//길이 비율로 ScaleY를 구하자
				float length2End = Vector2.Distance(startPos, endPos);

				float angleToEnd = _worldMatrix.Angle;
				Vector2 scaleToEnd = _worldMatrix.Scale;
				if(length2End > 0.0f)
				{
					angleToEnd = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;//각도 추가할 필요 없을까?
					angleToEnd -= 90.0f;

					//Y 스케일 비율을 계산해야한다.
					scaleToEnd.y = length2End / _shapeLength;
				}

				//World Matrix를 만들자
				_guiMatrix.SetTRS(startPos, angleToEnd, scaleToEnd, true);
				
				if(isIKMatrix)
				{
					//IK가 적용되어 있다면 같은 방식으로 Matrix를 만들자.
					Vector2 startPos_IK = _worldMatrix_IK.Pos;				
					Vector2 endPos_IK = _worldMatrix_IK.MulPoint2(new Vector2(0, _shapeLength));

					//길이 비율로 ScaleY를 구하자
					float length2End_IK = Vector2.Distance(startPos_IK, endPos_IK);

					float angleToEnd_IK = _worldMatrix_IK.Angle;
					Vector2 scaleToEnd_IK = _worldMatrix_IK.Scale;

					if(length2End_IK > 0.0f)
					{
						angleToEnd_IK = Mathf.Atan2(endPos_IK.y - startPos_IK.y, endPos_IK.x - startPos_IK.x) * Mathf.Rad2Deg;//각도 추가할 필요 없을까?
						angleToEnd_IK -= 90.0f;

						//Y 스케일 비율을 계산해야한다.
						scaleToEnd_IK.y = length2End_IK / _shapeLength;
					}

					//World Matrix IK를 만들자
					_guiMatrix_IK.SetTRS(startPos_IK, angleToEnd_IK, scaleToEnd_IK, true);
				}
				else
				{
					//그렇지 않다면 그냥 복사
					_guiMatrix_IK.SetMatrix(_guiMatrix, true);
				}
			}
			//----------------------------------
			


			



			//이제 GUI 이미지를 그리기 위한 좌표를 계산하자. (GUIMatrix 이용)

			//Shape의 Local좌표를 설정하고, WorldMatrix를 적용한다.
			//변경 20.3.21 : 렌더 세팅에 따라서 
			if (!s_boneRenderSettings._isVersion2)
			{
				//[ 버전 1 ]

				float boneWidth = _shapeWidth * s_boneRenderSettings._scaleRatio;

				//원점의 기본 반경은 10픽셀이다. x ScaleRatio
				//float orgRadius = 10.0f * s_boneRenderSettings._scaleRatio;
				if(!s_boneRenderSettings._isScaledByZoom)
				{
					//Zoom에 반비례해야 항상 고정된 크기가 된다.
					boneWidth /= s_boneRenderSettings._workspaceZoom;
					//orgRadius /= s_boneRenderSettings._workspaceZoom;
				}
				float boneRadius = boneWidth * 0.5f;

				_shapePoints_V1_Normal.Width_Half = boneRadius;
				_shapePoints_V1_IK.Width_Half = boneRadius;

				//_shapePoints_V1_Normal.Radius = orgRadius;
				//_shapePoints_V1_IK.Radius = orgRadius;

				_shapePoints_V1_Normal.End.x = 0;
				_shapePoints_V1_Normal.End.y = _shapeLength;


				_shapePoints_V1_Normal.Mid1.x = -boneRadius;
				_shapePoints_V1_Normal.Mid1.y = _shapeLength * 0.2f;

				_shapePoints_V1_Normal.Mid2.x = boneRadius;
				_shapePoints_V1_Normal.Mid2.y = _shapeLength * 0.2f;

				float taperRatio = Mathf.Clamp01((float)(100 - _shapeTaper) / 100.0f);

				_shapePoints_V1_Normal.End1 = new Vector3(-boneRadius * taperRatio, _shapeLength, 0.0f);
				_shapePoints_V1_Normal.End2 = new Vector3(boneRadius * taperRatio, _shapeLength, 0.0f);

				//이전 : WorldMatrix 그대로 사용
				//_shapePoints_V1_Normal.End = _worldMatrix.MulPoint2(_shapePoints_V1_Normal.End);
				//_shapePoints_V1_Normal.Mid1 = _worldMatrix.MulPoint2(_shapePoints_V1_Normal.Mid1);
				//_shapePoints_V1_Normal.Mid2 = _worldMatrix.MulPoint2(_shapePoints_V1_Normal.Mid2);
				//_shapePoints_V1_Normal.End1 = _worldMatrix.MulPoint2(_shapePoints_V1_Normal.End1);
				//_shapePoints_V1_Normal.End2 = _worldMatrix.MulPoint2(_shapePoints_V1_Normal.End2);

				//변경 20.8.23 : GUIMatrix로 변경
				_shapePoints_V1_Normal.End = _guiMatrix.MulPoint2(_shapePoints_V1_Normal.End);
				_shapePoints_V1_Normal.Mid1 = _guiMatrix.MulPoint2(_shapePoints_V1_Normal.Mid1);
				_shapePoints_V1_Normal.Mid2 = _guiMatrix.MulPoint2(_shapePoints_V1_Normal.Mid2);
				_shapePoints_V1_Normal.End1 = _guiMatrix.MulPoint2(_shapePoints_V1_Normal.End1);
				_shapePoints_V1_Normal.End2 = _guiMatrix.MulPoint2(_shapePoints_V1_Normal.End2);

				if (isBoneIKUsing)
				{
					//IK용 GUI 업데이트도 한다.
					if (_isIKCalculated_Controlled || _isIKRendered_Controller)
					{
						_shapePoints_V1_IK.End.x = 0;
						_shapePoints_V1_IK.End.y = _shapeLength;

						_shapePoints_V1_IK.Mid1.x = -boneRadius;
						_shapePoints_V1_IK.Mid1.y = _shapeLength * 0.2f;

						_shapePoints_V1_IK.Mid2.x = boneRadius;
						_shapePoints_V1_IK.Mid2.y = _shapeLength * 0.2f;

						_shapePoints_V1_IK.End1 = new Vector3(-boneRadius * taperRatio, _shapeLength, 0.0f);
						_shapePoints_V1_IK.End2 = new Vector3(boneRadius * taperRatio, _shapeLength, 0.0f);

						//이전 : WorldMatrix IK 그대로 사용
						//_shapePoints_V1_IK.End = _worldMatrix_IK.MulPoint2(_shapePoints_V1_IK.End);
						//_shapePoints_V1_IK.Mid1 = _worldMatrix_IK.MulPoint2(_shapePoints_V1_IK.Mid1);
						//_shapePoints_V1_IK.Mid2 = _worldMatrix_IK.MulPoint2(_shapePoints_V1_IK.Mid2);
						//_shapePoints_V1_IK.End1 = _worldMatrix_IK.MulPoint2(_shapePoints_V1_IK.End1);
						//_shapePoints_V1_IK.End2 = _worldMatrix_IK.MulPoint2(_shapePoints_V1_IK.End2);

						//변경 20.8.23 : GUIMatrix_IK로 변경
						_shapePoints_V1_IK.End = _guiMatrix_IK.MulPoint2(_shapePoints_V1_IK.End);
						_shapePoints_V1_IK.Mid1 = _guiMatrix_IK.MulPoint2(_shapePoints_V1_IK.Mid1);
						_shapePoints_V1_IK.Mid2 = _guiMatrix_IK.MulPoint2(_shapePoints_V1_IK.Mid2);
						_shapePoints_V1_IK.End1 = _guiMatrix_IK.MulPoint2(_shapePoints_V1_IK.End1);
						_shapePoints_V1_IK.End2 = _guiMatrix_IK.MulPoint2(_shapePoints_V1_IK.End2);


					}
					else
					{
						//동일한 형태
						_shapePoints_V1_IK.End = _shapePoints_V1_Normal.End;
						_shapePoints_V1_IK.Mid1 = _shapePoints_V1_Normal.Mid1;
						_shapePoints_V1_IK.Mid2 = _shapePoints_V1_Normal.Mid2;
						_shapePoints_V1_IK.End1 = _shapePoints_V1_Normal.End1;
						_shapePoints_V1_IK.End2 = _shapePoints_V1_Normal.End2;
					}
				}

				_shapePoint_Calculated_End = _shapePoints_V1_Normal.End;
			}
			else
			{
				//[ 버전 2 ]
				//본의 폭을 정하자. Width에 상관없이 고정값으로..?
				float boneLength = _shapeLength * BONE_V2_REAL_LENGTH_RATIO;
				
				float boneWidthHalf = s_boneRenderSettings._widthHalf_V2;
				float backScaleYCorrection = 1.0f;
				
				//이전
				//if(_worldMatrix._scale.y != 0.0f)
				//{
				//	backScaleYCorrection = Mathf.Abs(_worldMatrix._scale.x) / Mathf.Abs(_worldMatrix._scale.y);//이렇게 해야 원형이 유지된다.
				//}
				//변경 20.8.17 : 래핑 후 코드
				//if(_worldMatrix.Scale.y != 0.0f)
				//{
				//	backScaleYCorrection = Mathf.Abs(_worldMatrix.Scale.x) / Mathf.Abs(_worldMatrix.Scale.y);//이렇게 해야 원형이 유지된다.
				//}

				//다시 변경 20.8.23 : GUIMatrix로 변경
				if(_guiMatrix._scale.y != 0.0f)
				{
					backScaleYCorrection = Mathf.Abs(_guiMatrix._scale.x) / Mathf.Abs(_guiMatrix._scale.y);//이렇게 해야 원형이 유지된다.
				}

				_shapePoints_V2_Normal.RadiusCorrectionRatio = backScaleYCorrection;
				_shapePoints_V2_IK.RadiusCorrectionRatio = backScaleYCorrection;

				//선 굵기도 정하자
				//float lineThick = BONE_V2_DEFAULT_LINE_THICKNESS * Mathf.Abs(_worldMatrix.Scale.x) * Mathf.Abs(_worldMatrix.Scale.y);
				//변경 20.8.23 : GUIMatrix로 변경
				float lineThick = BONE_V2_DEFAULT_LINE_THICKNESS * Mathf.Abs(_guiMatrix._scale.x) * Mathf.Abs(_guiMatrix._scale.y);


				if(!s_boneRenderSettings._isScaledByZoom)
				{
					lineThick /= s_boneRenderSettings._workspaceZoom;
				}
				_shapePoints_V2_Normal.LineThickness = lineThick;
				_shapePoints_V2_IK.LineThickness = lineThick;
				
				//_shapePoints_V2_Normal.Radius = boneRadius;
				//_shapePoints_V2_IK.Radius = boneRadius;

				_shapePoints_V2_Normal.End.x = 0;
				_shapePoints_V2_Normal.End.y = boneLength;

				_shapePoints_V2_Normal.Back1.x = -boneWidthHalf;
				_shapePoints_V2_Normal.Back1.y = -boneWidthHalf * backScaleYCorrection;

				_shapePoints_V2_Normal.Back2.x = boneWidthHalf;
				_shapePoints_V2_Normal.Back2.y = -boneWidthHalf * backScaleYCorrection;

				_shapePoints_V2_Normal.Mid1.x = -boneWidthHalf;
				_shapePoints_V2_Normal.Mid1.y = 0;

				_shapePoints_V2_Normal.Mid2.x = boneWidthHalf;
				_shapePoints_V2_Normal.Mid2.y = 0;

				_shapePoints_V2_Normal.End1.x = -boneWidthHalf;
				_shapePoints_V2_Normal.End1.y = boneLength;

				_shapePoints_V2_Normal.End2.x = boneWidthHalf;
				_shapePoints_V2_Normal.End2.y = boneLength;

				//이전
				//_shapePoints_V2_Normal.End = _worldMatrix.MulPoint2(_shapePoints_V2_Normal.End);
				//_shapePoints_V2_Normal.Back1 = _worldMatrix.MulPoint2(_shapePoints_V2_Normal.Back1);
				//_shapePoints_V2_Normal.Back2 = _worldMatrix.MulPoint2(_shapePoints_V2_Normal.Back2);
				//_shapePoints_V2_Normal.Mid1 = _worldMatrix.MulPoint2(_shapePoints_V2_Normal.Mid1);
				//_shapePoints_V2_Normal.Mid2 = _worldMatrix.MulPoint2(_shapePoints_V2_Normal.Mid2);
				//_shapePoints_V2_Normal.End1 = _worldMatrix.MulPoint2(_shapePoints_V2_Normal.End1);
				//_shapePoints_V2_Normal.End2 = _worldMatrix.MulPoint2(_shapePoints_V2_Normal.End2);

				//변경 20.8.23 : GUIMatrix로 변경
				_shapePoints_V2_Normal.End =	_guiMatrix.MulPoint2(_shapePoints_V2_Normal.End);
				_shapePoints_V2_Normal.Back1 =	_guiMatrix.MulPoint2(_shapePoints_V2_Normal.Back1);
				_shapePoints_V2_Normal.Back2 =	_guiMatrix.MulPoint2(_shapePoints_V2_Normal.Back2);
				_shapePoints_V2_Normal.Mid1 =	_guiMatrix.MulPoint2(_shapePoints_V2_Normal.Mid1);
				_shapePoints_V2_Normal.Mid2 =	_guiMatrix.MulPoint2(_shapePoints_V2_Normal.Mid2);
				_shapePoints_V2_Normal.End1 =	_guiMatrix.MulPoint2(_shapePoints_V2_Normal.End1);
				_shapePoints_V2_Normal.End2 =	_guiMatrix.MulPoint2(_shapePoints_V2_Normal.End2);

				//UnityEngine.Debug.Log("Bone [" + this._name + "] Vers Update : " + _shapePoints_V2_Normal.Back1 + " > " + _shapePoints_V2_Normal.Back2 + " > " + _shapePoints_V2_Normal.End1);

				if (isBoneIKUsing)
				{
					//IK용 GUI 업데이트도 한다.
					if (_isIKCalculated_Controlled || _isIKRendered_Controller)
					{
						float backScaleYCorrection_IK = 1.0f;

						//if (_worldMatrix_IK.Scale.y != 0.0f)
						//{
						//	backScaleYCorrection_IK = Mathf.Abs(_worldMatrix_IK.Scale.x) / Mathf.Abs(_worldMatrix_IK.Scale.y);//이렇게 해야 원형이 유지된다.
						//}

						//20.8.23 : GUIMatrix IK로 변경
						if (_guiMatrix_IK._scale.y != 0.0f)
						{
							backScaleYCorrection_IK = Mathf.Abs(_guiMatrix_IK._scale.x) / Mathf.Abs(_guiMatrix_IK._scale.y);//이렇게 해야 원형이 유지된다.
						}

						_shapePoints_V2_IK.End.x = 0;
						_shapePoints_V2_IK.End.y = boneLength;

						_shapePoints_V2_IK.Back1.x = -boneWidthHalf;
						_shapePoints_V2_IK.Back1.y = -boneWidthHalf * backScaleYCorrection_IK;

						_shapePoints_V2_IK.Back2.x = boneWidthHalf;
						_shapePoints_V2_IK.Back2.y = -boneWidthHalf * backScaleYCorrection_IK;

						_shapePoints_V2_IK.Mid1.x = -boneWidthHalf;
						_shapePoints_V2_IK.Mid1.y = 0;

						_shapePoints_V2_IK.Mid2.x = boneWidthHalf;
						_shapePoints_V2_IK.Mid2.y = 0;

						_shapePoints_V2_IK.End1.x = -boneWidthHalf;
						_shapePoints_V2_IK.End1.y = boneLength;

						_shapePoints_V2_IK.End2.x = boneWidthHalf;
						_shapePoints_V2_IK.End2.y = boneLength;

						//이전
						//_shapePoints_V2_IK.End = _worldMatrix_IK.MulPoint2(_shapePoints_V2_IK.End);
						//_shapePoints_V2_IK.Back1 = _worldMatrix_IK.MulPoint2(_shapePoints_V2_IK.Back1);
						//_shapePoints_V2_IK.Back2 = _worldMatrix_IK.MulPoint2(_shapePoints_V2_IK.Back2);
						//_shapePoints_V2_IK.Mid1 = _worldMatrix_IK.MulPoint2(_shapePoints_V2_IK.Mid1);
						//_shapePoints_V2_IK.Mid2 = _worldMatrix_IK.MulPoint2(_shapePoints_V2_IK.Mid2);
						//_shapePoints_V2_IK.End1 = _worldMatrix_IK.MulPoint2(_shapePoints_V2_IK.End1);
						//_shapePoints_V2_IK.End2 = _worldMatrix_IK.MulPoint2(_shapePoints_V2_IK.End2);

						//변경 20.8.23 : GUIMatrix IK로 변경
						_shapePoints_V2_IK.End =	_guiMatrix_IK.MulPoint2(_shapePoints_V2_IK.End);
						_shapePoints_V2_IK.Back1 =	_guiMatrix_IK.MulPoint2(_shapePoints_V2_IK.Back1);
						_shapePoints_V2_IK.Back2 =	_guiMatrix_IK.MulPoint2(_shapePoints_V2_IK.Back2);
						_shapePoints_V2_IK.Mid1 =	_guiMatrix_IK.MulPoint2(_shapePoints_V2_IK.Mid1);
						_shapePoints_V2_IK.Mid2 =	_guiMatrix_IK.MulPoint2(_shapePoints_V2_IK.Mid2);
						_shapePoints_V2_IK.End1 =	_guiMatrix_IK.MulPoint2(_shapePoints_V2_IK.End1);
						_shapePoints_V2_IK.End2 =	_guiMatrix_IK.MulPoint2(_shapePoints_V2_IK.End2);
					}
					else
					{
						//동일한 형태
						_shapePoints_V2_IK.End = _shapePoints_V2_Normal.End;
						_shapePoints_V2_IK.Back1 = _shapePoints_V2_Normal.Back1;
						_shapePoints_V2_IK.Back2 = _shapePoints_V2_Normal.Back2;
						_shapePoints_V2_IK.Mid1 = _shapePoints_V2_Normal.Mid1;
						_shapePoints_V2_IK.Mid2 = _shapePoints_V2_Normal.Mid2;
						_shapePoints_V2_IK.End1 = _shapePoints_V2_Normal.End1;
						_shapePoints_V2_IK.End2 = _shapePoints_V2_Normal.End2;
					}
				}

				_shapePoint_Calculated_End = _shapePoints_V2_Normal.End;
			}
			


			if (isRecursive)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].GUIUpdate(true, isBoneIKUsing);
				}
			}
		}

		
		// Functions
		//--------------------------------------------
		/// <summary>
		/// 다른 Bone을 Child로 둔다.
		/// Child->Parent 연결도 자동으로 수행한다.
		/// </summary>
		/// <param name="bone"></param>
		public bool AddChildBone(apBone bone)
		{
			if (bone == null)
			{
				return false;
			}
			if (bone._meshGroup != _meshGroup)
			{
				//다른 MeshGroup에 속해있다면 실패
				return false;
			}

			int boneID = bone._uniqueID;
			if (!_childBoneIDs.Contains(boneID))
			{
				_childBoneIDs.Add(boneID);
			}

			if (!_childBones.Contains(bone))
			{
				_childBones.Add(bone);
			}

			bone._parentBone = this;
			bone._parentBoneID = _uniqueID;

			return true;
		}

		/// <summary>
		/// Child Bone 하나를 제외한다.
		/// </summary>
		/// <param name="bone"></param>
		/// <returns></returns>
		public bool ReleaseChildBone(apBone bone)
		{
			if (bone == null)
			{
				return false;
			}

			int boneID = bone._uniqueID;
			_childBoneIDs.Remove(boneID);
			_childBones.Remove(bone);

			//Parent 연결도 끊어준다.
			bone._parentBone = null;
			bone._parentBoneID = -1;

			return true;
		}


		public void ReleaseAllChildBones(apBone bone)
		{
			List<apBone> tmpChildBones = new List<apBone>();
			for (int i = 0; i < _childBones.Count; i++)
			{
				tmpChildBones.Add(_childBones[i]);
			}

			for (int i = 0; i < tmpChildBones.Count; i++)
			{
				ReleaseChildBone(tmpChildBones[i]);
			}

			_childBones.Clear();
			_childBoneIDs.Clear();

		}

		// Get / Set
		//--------------------------------------------
		/// <summary>
		/// boneID를 가지는 Bone을 자식 노드로 두고 있는가.
		/// 재귀적으로 찾는다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apBone GetChildBoneRecursive(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Count; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해보자

			for (int i = 0; i < _childBones.Count; i++)
			{
				apBone result = _childBones[i].GetChildBoneRecursive(boneID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// 바로 아래의 자식 Bone을 검색한다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apBone GetChildBone(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Count; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			return null;
		}

		/// <summary>
		/// 자식 Bone 중에서 특정 Target Bone을 재귀적인 자식으로 가지는 시작 Bone을 찾는다.
		/// </summary>
		/// <param name="targetBoneID"></param>
		/// <returns></returns>
		public apBone FindNextChainedBone(int targetBoneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Count; i++)
			{
				if (_childBones[i]._uniqueID == targetBoneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해서, 그 중에 실제로 Target Bone을 포함하는 Child Bone을 리턴하자

			for (int i = 0; i < _childBones.Count; i++)
			{
				apBone result = _childBones[i].GetChildBoneRecursive(targetBoneID);
				if (result != null)
				{
					//return result;
					return _childBones[i];//<<Result가 아니라, ChildBone을 리턴
				}
			}
			return null;
		}

		/// <summary>
		/// 요청한 boneID를 가지는 Bone을 부모 노드로 두고 있는가.
		/// 재귀적으로 찾는다.
		/// </summary>
		/// <param name="boneID"></param>
		/// <returns></returns>
		public apBone GetParentRecursive(int boneID)
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


		public void SetRiggingTest(bool isRiggingTest)
		{
			_isRigTestPosing = isRiggingTest;
		}

		public void ResetRiggingTestPose()
		{
			_rigTestMatrix.SetIdentity();
		}


		// IK Chained에 관한 검색
		/// <summary>
		/// IK Chained인 경우, 자신을 제외한 Chain 내의 모든 자식 본들을 리턴한다.
		/// 마지막 본은 Chain의 타겟이며, IK Chain의 End Bone본이다. (실제로 IK 작동을 보장하지는 않는다)
		/// </summary>
		/// <returns></returns>
		public List<apBone> GetAllChainedChildBones()
		{
			if(_optionIK == OPTION_IK.Disabled)
			{
				return null;
			}
			List<apBone> resultBones = new List<apBone>();
			apBone curBone = this;
			
			
			
			apBone IKHeaderBone = null;
			if(this._optionIK == OPTION_IK.IKHead)
			{
				IKHeaderBone = this;
			}
			else
			{
				IKHeaderBone = this._IKHeaderBone;
			}

			apBone nextBone = null;
			while(true)
			{
				if(curBone._IKNextChainedBone == null)
				{
					break;
				}

				nextBone = curBone._IKNextChainedBone;
				//Debug.Log("Check : " + nextBone._name);
				
				//Chain이 연결되어야 한다.
				if (IKHeaderBone != null)
				{
					if (nextBone._IKHeaderBone != IKHeaderBone)
					{
						//만약 다음 IK 본의 Header가 이 Bone을 가리키지 않는다면 무효
						//Debug.Log("Header가 서로 같지 않다. > Break [" + (nextBone._IKHeaderBone != null ? nextBone._IKHeaderBone._name : "<None>") + "] =? " + curBone._name);
						break;
					}
				}
				

				resultBones.Add(nextBone);

				//자식 본으로 이동
				curBone = nextBone;
			}

			return resultBones;
			
		}

		/// <summary>
		/// Chained로 연결된 상위 본들의 리스트
		/// </summary>
		/// <returns></returns>
		public List<apBone> GetAllChainedParentBones()
		{
			if(this._parentBone == null)
			{
				return null;
			}
			List<apBone> resultBones = new List<apBone>();
			apBone curBone = this._parentBone;
			apBone prevChainedBone = this;
			while(true)
			{
				if(curBone == null)
				{
					break;
				}
				if(curBone._optionIK == OPTION_IK.Disabled)
				{
					//자식이 없다.
					break;
				}
				if(curBone._IKNextChainedBone != prevChainedBone)
				{
					//Chain으로 연결되지 않았다.
					break;
				}

				resultBones.Add(curBone);

				prevChainedBone = curBone;
				curBone = curBone._parentBone;
				//위로 이동
			}

			return resultBones;
		}

		


		/// <summary>
		/// 자기 자신을 제외한 모든 자식 본을 리턴한다.
		/// 순서는 보장되지 않는다.
		/// </summary>
		/// <returns></returns>
		public List<apBone> GetAllChildBones()
		{
			List<apBone> resultBones = new List<apBone>();

			if(_childBones != null)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					GetAllChildBonesRecursive(_childBones[i], resultBones);
				}
			}

			return resultBones;
		}

		private void GetAllChildBonesRecursive(apBone targetBone, List<apBone> resultBones)
		{
			if(targetBone == null)
			{
				return;
			}
			resultBones.Add(targetBone);

			if (targetBone._childBones != null)
			{
				for (int i = 0; i < targetBone._childBones.Count; i++)
				{
					GetAllChildBonesRecursive(targetBone._childBones[i], resultBones);
				}
			}
		}


		

		//-------------------------------------------------------------------
		/// <summary>
		/// Bone을 기준으로 해당 위치를 바라보는 각도를 구한다.
		/// Bone의 현재 각도는 포함하지 않고 전체 각도만 계산하므로 따로 빼주어야 한다.
		/// Bone은 +Y로 향하므로 거기에 맞게 각도를 조절한다.
		/// 좌표계는 동일해야한다.
		/// </summary>
		/// <param name="targetPos"></param>
		/// <param name="bonePos"></param>
		/// <param name="prevAngle">연산 전의 결과값. LookAt 실패시 이 값을 리턴한다.</param>
		/// <returns></returns>
		public static float GetLookAtAngle(Vector2 targetPos, Vector2 bonePos, float prevAngle)
		{
			//두 점이 너무 가까우면 LookAt을 할 수 없다.
			if (Mathf.Abs(targetPos.y - bonePos.y) < 0.0001f && Mathf.Abs(targetPos.x - bonePos.x) < 0.0001f)
			{
				return prevAngle;
			}

			float angle = Mathf.Atan2(targetPos.y - bonePos.y, targetPos.x - bonePos.x) * Mathf.Rad2Deg;
			//angle += 90.0f;
			//angle += 180.0f;

			angle -= 90.0f;
			if (angle > 180.0f)
			{
				angle -= 360.0f;
			}
			else if (angle < -180.0f)
			{
				angle += 360.0f;
			}

			return angle;
		}


		
		/// <summary>
		/// IK 요청을 한다. World 좌표계에서 얼마나 각도를 더 변경해야하는지 값이 변수로 저장된다.
		/// IK Chain의 Tail에서 호출해야한다. 연산 순서는 Tail -> Parent
		/// </summary>
		/// <param name="targetPosW">현재 Bone부터 </param>
		/// <param name="weight">IK가 적용되는 정도. 0~1</param>
		public bool RequestIK(Vector2 targetPosW, float weight, bool isContinuous)
		{
			if (!_isIKTail || _IKChainSet == null)
			{
				//Debug.LogError("[" + _name + "] Request IK Failed : Is Tail : " + _isIKTail + " / Chain Set Exist : " + (_IKChainSet != null));
				//_isIKtargetDebug = false;
				return false;
			}


			//bool isSuccess = _IKChainSet.SimulateIK(targetPosW, isContinuous);//이전 (World 좌표계)
			bool isSuccess = _IKChainSet.SimulateIK(_worldMatrix.ConvertForIK(targetPosW), isContinuous);//변경 20.9.4 (IK 좌표계)

			//IK가 실패하면 패스
			if (!isSuccess)
			{
				//Debug.LogError("[" + _name + "] Request IK Failed Calculate : " + targetPosW);
				//_isIKtargetDebug = false;
				return false;
			}

			//IK 결과값을 Bone에 넣어주자
			_IKChainSet.AdaptIKResultToBones(weight);

			//Debug.Log("[" + _name + "] Request IK Success : " + targetPosW);
			return true;
		}



		/// <summary>
		/// RequestIK의 제한된 버전
		/// limitedBones에 포함된 Bone으로만 IK를 만들어야한다.
		/// Chain을 검색해서 포함된 것의 Head를 검색해서 IK를 처리한다.
		/// RequestIK와 달리 "마지막으로 Head처럼 처리된 Bone"을 리턴한다.
		/// 실패시 null리턴
		/// </summary>
		/// <param name="targetPosW"></param>
		/// <param name="weight"></param>
		/// <param name="isContinuous"></param>
		/// <param name="limitedBones"></param>
		/// <returns></returns>
		public apBone RequestIK_Limited(Vector2 targetPosW, float weight, bool isContinuous, List<apBone> limitedBones)
		{
			if (!_isIKTail || _IKChainSet == null)
			{
				return null;
			}

			apBoneIKChainUnit lastCheckChain = null;
			apBoneIKChainUnit curCheckChain = null;
			//[Tail : 0] .... [Head : Count - 1]이므로
			//앞부터 갱신하면서 Head쪽으로 가는 가장 마지막 레퍼런스를 찾으면 된다.
			for (int i = 0; i < _IKChainSet._chainUnits.Count; i++)
			{
				curCheckChain = _IKChainSet._chainUnits[i];
				if (limitedBones.Contains(curCheckChain._baseBone))
				{
					//이건 포함된 BoneUnit이다.
					lastCheckChain = curCheckChain;
				}
				else
				{
					break;
				}
			}
			//잉... 하나도 해당 안되는데용..
			if (lastCheckChain == null)
			{
				return null;
			}


			//Debug.Log("Request IK [" + _name + "] / Target PosW : " + targetPosW);
			//bool isSuccess = _IKChainSet.SimulateIK(targetPosW, isContinuous);
			
			//bool isSuccess = _IKChainSet.SimulateIK_Limited(targetPosW, isContinuous, lastCheckChain);//이전 (World 좌표계)
			bool isSuccess = _IKChainSet.SimulateIK_Limited(_worldMatrix.ConvertForIK(targetPosW), isContinuous, lastCheckChain);//변경 20.9.4 (IK 좌표계)


			//IK가 실패하면 패스
			if (!isSuccess)
			{
				//Debug.LogError("[" + _name + "] Request IK Failed Calculate : " + targetPosW);
				//_isIKtargetDebug = false;
				return null;
			}

			//IK 결과값을 Bone에 넣어주자
			_IKChainSet.AdaptIKResultToBones(weight);

			//Debug.Log("[" + _name + "] Request IK Success : " + targetPosW);
			//return true;
			return lastCheckChain._baseBone;
		}


		/// <summary>
		/// 추가 20.10.9 : Gizmo에서 IK를 이용해서 편집할 때, Scale에 따라서 IKResultAngle(Delta)가 반대로 적용되어야 한다.
		/// (자세한 것은 apBoneIKChainUnit의 ReadyToSimulate코드를 확인할 것)
		/// </summary>
		/// <returns></returns>		
		public bool IsNeedInvertIKDeltaAngle_Gizmo()
		{
			if (_rootBoneScaleMethod == apPortrait.ROOT_BONE_SCALE_METHOD.Default)
			{
				if (_parentBone != null)
				{
					return (_worldMatrix.Scale.x * _worldMatrix.Scale.y < 0.0f);
				}
				else if (_renderUnit != null)
				{
					return (_renderUnit.WorldMatrixWrap._scale.x * _renderUnit.WorldMatrixWrap._scale.y < 0.0f);
				}
			}
			else
			{
				if (_parentBone != null)
				{
					return (_worldMatrix._mtx_Skew._scale_Step1.x * _worldMatrix._mtx_Skew._scale_Step1.y < 0.0f);
				}
			}
			return false;
		}

		//--------------------------------------------------------------------------------------------------
		//public bool IsGUIVisible //이전
		//변경 21.1.28 : Tmp와 Rule을 별개로 나눈다. + 타입 바꿈
		public GUI_VISIBLE_TYPE VisibleType_Tmp
		{
			get
			{
				//return _isBoneGUIVisible;//이전
				return _visibleType_Tmp;
			}
		}

		public GUI_VISIBLE_TYPE VisibleType_Rule
		{
			get { return _visibleType_Rule; }
		}

		//Hierarchy에 어떤 아이콘으로 보이는지 여부
		//  (Rule)      :  (Tmp)
		//- Show/None   >  Show/None	: Current_Visible (Show)
		//- Show/None   >  Hide			: TmpWork_NonVisible (Hide)
		//- Hide        >  None         : Rule_NonVisible (Hide)
		//- Hide        >  Show         : TmpWork_Visible (Show)
		//- Hide        >  Hide         : Rule_NonVisible (Hide)
		public VISIBLE_COMBINATION_ICON VisibleIconType
		{
			get
			{
				if(_visibleType_Rule != GUI_VISIBLE_TYPE.Hide)
				{
					if(_visibleType_Tmp != GUI_VISIBLE_TYPE.Hide)
					{
						//- Show/None > Show/None : Current_Visible (Show)
						return VISIBLE_COMBINATION_ICON.Visible_Default;
					}
					else
					{
						//- Show/None > Hide : TmpWork_NonVisible (Hide)
						return VISIBLE_COMBINATION_ICON.NonVisible_Tmp;
					}
				}
				else
				{
					switch (_visibleType_Tmp)
					{
						case GUI_VISIBLE_TYPE.None:
							//- Hide > None : Rule_NonVisible (Hide)
							return VISIBLE_COMBINATION_ICON.NonVisible_Rule;

						case GUI_VISIBLE_TYPE.Show:
							//- Hide > Show : TmpWork_Visible (Show)
							return VISIBLE_COMBINATION_ICON.Visible_Tmp;

						case GUI_VISIBLE_TYPE.Hide:
							//- Hide > Hide : Rule_NonVisible (Hide)
							return VISIBLE_COMBINATION_ICON.NonVisible_Rule;

					}
				}
				return VISIBLE_COMBINATION_ICON.Visible_Default;
			}
			
			//Visible_Default, Visible_Tmp, NonVisible_Tmp, NonVisible_Rule
				
		}

		//IsGUIVisible에 해당하는 변수. Tmp와 Rule의 조합으로 완성된다.
		//코드 확인을 위해 이름을 바꾼다.
		public bool IsVisibleInGUI
		{
			get
			{
				return _visibleType_Tmp == GUI_VISIBLE_TYPE.Show
					|| (_visibleType_Tmp == GUI_VISIBLE_TYPE.None && _visibleType_Rule != GUI_VISIBLE_TYPE.Hide);
			}
		}

		

		/// <summary>
		/// Bone의 GUI상의 Visible을 리셋한다. 숨겨진 모든 본을 보이게 한다.
		/// </summary>
		public void ResetGUIVisibleRecursive(bool isWithRule)
		{
			//이전
			//_isBoneGUIVisible = true;

			//변경 21.1.28
			if(isWithRule)
			{
				//Debug.LogError("Bone : ResetGUIVisibleRecursive > Rule");
				_visibleType_Rule = GUI_VISIBLE_TYPE.None;
			}
			_visibleType_Tmp = GUI_VISIBLE_TYPE.None;

			if(_childBones.Count > 0)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].ResetGUIVisibleRecursive(isWithRule);
				}
			}
		}
		
		/// <summary>Bone의 GUI Visible을 지정한다.</summary>
		//public void SetGUIVisible(bool isVisible)//이전
		public void SetGUIVisible_Tmp(GUI_VISIBLE_TYPE visibleType)
		{
			//_isBoneGUIVisible = isVisible;
			_visibleType_Tmp = visibleType;
		}

		/// <summary>
		/// Rule의 값을 체크하면서 Visible의 타입을 결정한다.
		/// </summary>
		/// <param name="isVisible"></param>
		public void SetGUIVisible_Tmp_ByCheckRule(bool isVisible, bool isChildRecursive)
		{
			//_isBoneGUIVisible = isVisible;
			if(isVisible)
			{
				//Hide > Show 또는 None
				if(_visibleType_Rule == GUI_VISIBLE_TYPE.Hide)
				{
					//Hide > Show로 강제
					_visibleType_Tmp = GUI_VISIBLE_TYPE.Show;
				}
				else
				{
					//그 외에는 None으로 리셋
					_visibleType_Tmp = GUI_VISIBLE_TYPE.None;
				}
			}
			else
			{
				//Show > Hide 또는 None
				if(_visibleType_Rule == GUI_VISIBLE_TYPE.Hide)
				{
					//Hide > None
					_visibleType_Tmp = GUI_VISIBLE_TYPE.None;
				}
				else
				{
					//그 외에는 Hide로 강제
					_visibleType_Tmp = GUI_VISIBLE_TYPE.Hide;
				}
			}

			//v1.4.2 : 
			if(isChildRecursive)
			{
				int nChildBones = _childBones != null ? _childBones.Count : 0;
				if(nChildBones == 0)
				{
					return;
				}

				apBone childBone = null;
				for (int i = 0; i < nChildBones; i++)
				{
					childBone = _childBones[i];
					if(childBone == null 
						|| childBone == this)
					{
						continue;
					}
					childBone.SetGUIVisible_Tmp_ByCheckRule_Recursive(isVisible, this);
				}
			}
		}

		/// <summary>SetGUIVisible_Tmp_ByCheckRule 함수의 자식 본으로의 재귀 함수. 내용은 SetGUIVisible_Tmp_ByCheckRule를 참고하자.</summary>
		private void SetGUIVisible_Tmp_ByCheckRule_Recursive(bool isVisible, apBone startBone)
		{
			if(isVisible)
			{
				if(_visibleType_Rule == GUI_VISIBLE_TYPE.Hide) { _visibleType_Tmp = GUI_VISIBLE_TYPE.Show; }
				else { _visibleType_Tmp = GUI_VISIBLE_TYPE.None; }
			}
			else
			{
				if(_visibleType_Rule == GUI_VISIBLE_TYPE.Hide) { _visibleType_Tmp = GUI_VISIBLE_TYPE.None; }
				else { _visibleType_Tmp = GUI_VISIBLE_TYPE.Hide; }
			}

			int nChildBones = _childBones != null ? _childBones.Count : 0;
			if(nChildBones == 0)
			{
				return;
			}

			apBone childBone = null;
			for (int i = 0; i < nChildBones; i++)
			{
				childBone = _childBones[i];
				if(childBone == null 
					|| childBone == startBone
					|| childBone == this)
				{
					continue;
				}
				childBone.SetGUIVisible_Tmp_ByCheckRule_Recursive(isVisible, startBone);
			}
		}

		public void SetGUIVisible_Rule(GUI_VISIBLE_TYPE visibleType)
		{
			_visibleType_Rule = visibleType;
		}

		public void SetGUIVisibleWithExceptBone(bool isVisible, bool isRecursive, apBone exceptBone)
		{
			bool isRequestVisible = isVisible;//요청값을 일단 그대로 적용
			if(exceptBone == this)
			{
				//반대로 적용
				//이전
				//_isBoneGUIVisible = !isVisible;

				//변경 21.1.28 : Tmp를 변경하기 위해 별도의 Request 변수를 만들어서 값 전환
				isRequestVisible = !isVisible;
			}
			//else
			//{
			//	_isBoneGUIVisible = isVisible;
			//}

			//변경 21.1.28 : Tmp를 변경
			//여기서는 전체 처리라서 유지하는 값도 있어야 한다.
			if(isRequestVisible)
			{
				//> Show
				if(_visibleType_Rule == GUI_VISIBLE_TYPE.Hide)
				{
					//전단계인 Rule이 Hide면 강제로 Show
					_visibleType_Tmp = GUI_VISIBLE_TYPE.Show;
				}
				else
				{
					//그 외의 경우는 None (Show인 경우는 유지)
					if(_visibleType_Tmp != GUI_VISIBLE_TYPE.Show)
					{
						_visibleType_Tmp = GUI_VISIBLE_TYPE.None;
					}
					
				}
			}
			else
			{
				//> Hide
				if(_visibleType_Rule == GUI_VISIBLE_TYPE.Hide)
				{
					//전단계인 Rule이 Hide면 강제로 Show/None > None 또는 Hide 유지
					//Hide면 유지
					if(_visibleType_Tmp != GUI_VISIBLE_TYPE.Hide)
					{
						_visibleType_Tmp = GUI_VISIBLE_TYPE.None;
					}
				}
				else
				{
					//그 외의 경우는 Hide
					_visibleType_Tmp = GUI_VISIBLE_TYPE.Hide;
				}
			}




			//if(_parentBone != null)
			//{
			//	_isBoneGUIVisible_Parent = _parentBone.IsGUIVisible;
			//}
			//else
			//{
			//	_isBoneGUIVisible_Parent = true;
			//}

			if (isRecursive)
			{
				if (_childBones.Count > 0)
				{
					for (int i = 0; i < _childBones.Count; i++)
					{
						_childBones[i].SetGUIVisibleWithExceptBone(isVisible, isRecursive, exceptBone);
					}
				}
			}
		}

		//추가 20.7.15
		//지글본 테스트 함수
		public void SetJiggleTest(float testVelocity)
		{
			if(_isJiggle && _shapeLength > 0)
			{

				_isJiggleChecked_Prev = true;
				_calJig_Velocity = testVelocity;
				_calJig_Angle_Result_Prev = 0.0f;

				//기존 방식
				//s_calJig_Tmp_WorldMatrix.SetMatrix(_worldMatrix_IK);

				//변경 : ComplexMatrix 방식 (20.8.12)
				//s_calJig_Tmp_WorldMatrix.SetTRS(_worldMatrix_IK.Pos, _worldMatrix_IK.Angle, _worldMatrix_IK.Scale, false);//래핑

				//s_calJig_Tmp_WorldMatrix._angleDeg += _calJig_Angle_Result_Prev;
				//s_calJig_Tmp_WorldMatrix.MakeMatrix();

				//다시 변경 20.8.29 : BoneMatrix 방식으로 변경
				s_calJig_Tmp_WorldMatrix.CopyFromMatrix(_worldMatrix_IK);
				s_calJig_Tmp_WorldMatrix.RotateAsStep1(_calJig_Angle_Result_Prev, true);


				_calJig_EndPos_Prev = s_calJig_Tmp_WorldMatrix.MulPoint2(new Vector2(0, _shapeLength));
			}


			if (_childBones.Count > 0)
			{
				for (int i = 0; i < _childBones.Count; i++)
				{
					_childBones[i].SetJiggleTest(testVelocity);
				}
			}
		}


		//추가 20.8.20 : World Matrix의 방식 변경
		public void SetWorldMatrixScaleMode(apPortrait.ROOT_BONE_SCALE_METHOD scaleMode)
		{
			if (_worldMatrix == null)	{ _worldMatrix = new apBoneWorldMatrix(this, scaleMode); }
			else						{ _worldMatrix.SetScaleMethod(scaleMode); }

			if(_worldMatrix_IK == null)	{ _worldMatrix_IK = new apBoneWorldMatrix(this, scaleMode); }
			else						{ _worldMatrix_IK.SetScaleMethod(scaleMode); }

			if (_worldMatrix_NonModified == null)	{ _worldMatrix_NonModified = new apBoneWorldMatrix(this, scaleMode); }
			else									{ _worldMatrix_NonModified.SetScaleMethod(scaleMode); }

			if(s_calJig_Tmp_WorldMatrix == null)	{ s_calJig_Tmp_WorldMatrix = new apBoneWorldMatrix(null, scaleMode); }
			else									{ s_calJig_Tmp_WorldMatrix.SetScaleMethod(scaleMode); }

			_rootBoneScaleMethod = scaleMode;
		}
	}

}