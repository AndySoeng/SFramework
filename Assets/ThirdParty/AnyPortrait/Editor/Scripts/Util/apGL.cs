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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public static class apGL
	{
		//private static Material _mat_Color = null;
		//private static Material _mat_Texture = null;

		//private static int _windowPosX = 0;
		//private static int _windowPosY = 0;
		public static int _windowWidth = 0;
		public static int _windowHeight = 0;
		public static int _totalEditorWidth = 0;
		public static int _totalEditorHeight = 0;
		public static Vector2 _scrol_NotCalculated = Vector2.zero;
		public static int _posX_NotCalculated = 0;
		public static int _posY_NotCalculated = 0;

		public static Vector2 _windowScroll = Vector2.zero;

		public static float _zoom = 1.0f;
		public static float Zoom { get { return _zoom; } }

		public static Vector2 WindowSize { get { return new Vector2(_windowWidth, _windowHeight); } }
		public static Vector2 WindowSizeHalf { get { return new Vector2(_windowWidth / 2, _windowHeight / 2); } }

		private static GUIStyle _textStyle = GUIStyle.none;

		private static Vector4 _glScreenClippingSize = Vector4.zero;

		private static bool _isAnyCursorEvent = false;
		private static bool _isDelayedCursorEvent = false;
		private static Vector2 _delayedCursorPos = Vector2.zero;
		private static MouseCursor _delayedCursorType = MouseCursor.Zoom;

		//본 그리기를 위한 임시 Matrix
		private static apMatrix _cal_TmpMatrix;

		// 이전 : Flag를 이용한 렌더 방식
		//[Flags]
		//public enum RENDER_TYPE : int
		//{
		//	Default = 0,

		//	ShadeAllMesh = 1,
		//	AllMesh = 2,

		//	Vertex = 4,

		//	Outlines = 8,
		//	AllEdges = 16,
		//	TransparentEdges = 32,//추가 10.6 : DrawMesh에만 적용되는 "반투명 메시 엣지". AllEdges의 반투명 버전이다.

		//	VolumeWeightColor = 64,
		//	PhysicsWeightColor = 128,
		//	BoneRigWeightColor = 256,

		//	TransformBorderLine = 512,
		//	PolygonOutline = 1024,

		//	ToneColor = 2048,
		//	BoneOutlineOnly = 4096
		//}


		/// <summary>
		/// 변경 22.3.3 (v1.4.0) 클래스 타입의 렌더 방식.
		/// 기존의 Flag 타입의 RenderType 대신 그 이상의 요청을 모두 담을 수 있는 클래스 작성
		/// </summary>
		public class RenderTypeRequest
		{
			// 프리셋 (Static)
			private static RenderTypeRequest s_preset_Default = Make_Default();
			private static RenderTypeRequest s_preset_ToneColor = Make_ToneColor();
			public static RenderTypeRequest Preset_Default		{ get { return s_preset_Default; } }
			public static RenderTypeRequest Preset_ToneColor	{ get { return s_preset_ToneColor; } }

			public enum VISIBILITY
			{
				Shown,
				Hidden,
				Transparent
			}

			public enum SHOW_OPTION
			{
				Normal, Transparent
			}

			// Members
			//--------------------------------------------------------------
			private bool _isShadeAllMesh = false;
			private bool _isAllMesh = false;
			private VISIBILITY _visibility_Vertex = VISIBILITY.Hidden;
			private bool _isOutlines = false;
			private bool _isAllEdges = false;
			private bool _isTransparentEdges = false;
			private bool _isVolumeWeightColor = false;
			private bool _isPhysicsWeightColor = false;
			private bool _isBoneRigWeightColor = false;
			private bool _isTransformBorderLine = false;
			private bool _isPolygonOutline = false;
			private bool _isToneColor = false;
			private bool _isBoneOutlineOnly = false;
			private VISIBILITY _visibility_Pin = VISIBILITY.Hidden;			//핀
			private bool _isTestPinWeight = false;//핀 가중치를 테스트하는 상태 (다른 좌표계를 사용한다.)
			private bool _isPinVertWeight = false;//버텍스에 핀 가중치를 표시하는 상태
			private bool _isPinRange = false;//핀의 Range를 표시할지 여부

			

			public RenderTypeRequest()
			{
				Reset();
			}
			/// <summary>
			/// 모든 렌더링 옵션을 초기화한다.
			/// </summary>
			public void Reset()
			{
				_isShadeAllMesh = false;
				_isAllMesh = false;
				_visibility_Vertex = VISIBILITY.Hidden;
				_isOutlines = false;
				_isAllEdges = false;
				_isTransparentEdges = false;
				_isVolumeWeightColor = false;
				_isPhysicsWeightColor = false;
				_isBoneRigWeightColor = false;
				_isTransformBorderLine = false;
				_isPolygonOutline = false;
				_isToneColor = false;
				_isBoneOutlineOnly = false;
				_visibility_Pin = VISIBILITY.Hidden;
				_isTestPinWeight = false;
				_isPinVertWeight = false;
				_isPinRange = false;
			}

			public void SetShadeAllMesh()			{ _isShadeAllMesh = true; }
			public void SetAllMesh()				{ _isAllMesh = true; }
			public void SetVertex(SHOW_OPTION showOption)
			{
				_visibility_Vertex = (showOption == SHOW_OPTION.Normal) ? VISIBILITY.Shown : VISIBILITY.Transparent;
			}
			public void SetOutlines()				{ _isOutlines = true; }
			public void SetAllEdges()				{ _isAllEdges = true; }
			public void SetTransparentEdges()		{ _isTransparentEdges = true; }
			public void SetVolumeWeightColor()		{ _isVolumeWeightColor = true; }
			public void SetPhysicsWeightColor()		{ _isPhysicsWeightColor = true; }
			public void SetBoneRigWeightColor()		{ _isBoneRigWeightColor = true; }
			public void SetTransformBorderLine()	{ _isTransformBorderLine = true; }
			public void SetPolygonOutline()			{ _isPolygonOutline = true; }
			public void SetToneColor()				{ _isToneColor = true; }
			public void SetBoneOutlineOnly()		{ _isBoneOutlineOnly = true; }
			public void SetPin(SHOW_OPTION showOption)
			{
				_visibility_Pin = showOption == SHOW_OPTION.Normal ? VISIBILITY.Shown : VISIBILITY.Transparent;
			}
			
			public void SetTestPinWeight()			{ _isTestPinWeight = true; }
			public void SetPinVertWeight()			{ _isPinVertWeight = true; }
			public void SetPinRange()				{ _isPinRange = true; }

			public bool ShadeAllMesh		{ get { return _isShadeAllMesh; } }
			public bool AllMesh				{ get { return _isAllMesh; } }
			public VISIBILITY Vertex		{ get { return _visibility_Vertex; } }
			public bool Outlines			{ get { return _isOutlines; } }
			public bool AllEdges			{ get { return _isAllEdges; } }
			public bool TransparentEdges	{ get { return _isTransparentEdges; } }//AllEdges의 반투명 버전
			public bool VolumeWeightColor	{ get { return _isVolumeWeightColor; } }
			public bool PhysicsWeightColor	{ get { return _isPhysicsWeightColor; } }
			public bool BoneRigWeightColor	{ get { return _isBoneRigWeightColor; } }
			public bool TransformBorderLine	{ get { return _isTransformBorderLine; } }
			public bool PolygonOutline		{ get { return _isPolygonOutline; } }
			public bool ToneColor			{ get { return _isToneColor; } }
			public bool BoneOutlineOnly		{ get { return _isBoneOutlineOnly; } }
			public VISIBILITY Pin			{ get { return _visibility_Pin; } }
			public bool TestPinWeight		{ get { return _isTestPinWeight; } }
			public bool PinVertWeight		{ get { return _isPinVertWeight; } }
			public bool PinRange			{ get { return _isPinRange; } }

			//Static 프리셋 생성
			private static RenderTypeRequest Make_Default()
			{
				RenderTypeRequest newRequest = new RenderTypeRequest();
				newRequest.Reset();
				return newRequest;
			}

			private static RenderTypeRequest Make_ToneColor()
			{
				RenderTypeRequest newRequest = new RenderTypeRequest();
				newRequest.Reset();
				newRequest.SetToneColor();
				return newRequest;
			}
		}
		



		

		private static Color _textureColor_Gray = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		private static Color _textureColor_Shade = new Color(0.3f, 0.3f, 0.3f, 1.0f);

		//private static Color _vertColor_NotSelected = new Color(0.0f, 0.3f, 1.0f, 0.6f);
		//private static Color _vertColor_Selected = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		private static Color _vertColor_NextSelected = new Color(1.0f, 0.0f, 1.0f, 0.6f);

		//삭제 22.4.17 : 버텍스 이미지 변경으로 외곽선을 따로 그리지 않는다.
		//private static Color _vertColor_Outline = new Color(0.0f, 0.0f, 0.0f, 0.8f);
		//private static Color _vertColor_Outline_White = new Color(1.0f, 1.0f, 1.0f, 0.8f);

		//Weight인 경우 보(0)-파(25)-초(50)-노(75)-빨(100)로 이어진다.
		private static Color _vertColor_Weighted_0 = new Color(1.0f, 0.0f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted_25 = new Color(0.0f, 0.5f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted_50 = new Color(0.0f, 1.0f, 0.5f, 1.0f);
		private static Color _vertColor_Weighted_75 = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		
		//리깅 가중치 > 기본 (Vert는 약간 더 밝다)
		private static Color _vertColor_Weighted3_0 = new Color(0.0f, 0.0f, 0.0f, 1.0f);//검은색
		private static Color _vertColor_Weighted3_25 = new Color(0.0f, 0.0f, 1.0f, 1.0f);//파랑
		private static Color _vertColor_Weighted3_50 = new Color(1.0f, 1.0f, 0.0f, 1.0f);//노랑
		private static Color _vertColor_Weighted3_75 = new Color(1.0f, 0.5f, 0.0f, 1.0f);//주황
		private static Color _vertColor_Weighted3_100 = new Color(1.0f, 0.0f, 0.0f, 1.0f);//빨강

		private static Color _vertColor_Weighted3Vert_0 = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted3Vert_25 = new Color(0.2f, 0.2f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted3Vert_50 = new Color(1.0f, 1.0f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted3Vert_75 = new Color(1.0f, 0.5f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted3Vert_100 = new Color(1.0f, 0.2f, 0.2f, 1.0f);

		//리깅 가중치 > Vivid : HSV 값에 의해서 보간하기 때문에 보간 도중에 색이 탁해지는 것이 덜하다. (20.3.288)
		private static Color _vertHSV_Weighted3_NULL = new Color(0.0f, 0.0f, 0.0f);//검은색 (RGB)
		//private static Vector3 _vertHSV_Weighted3_0 = new Vector3(0.84f, 1.0f, 0.4f);//보라색
		//private static Vector3 _vertHSV_Weighted3_15 = new Vector3(0.667f, 1.0f, 1.0f);//파랑색
		//private static Vector3 _vertHSV_Weighted3_30 = new Vector3(0.5f, 1.0f, 0.7f);//하늘색 (살짝 어두워야 한다.)
		//private static Vector3 _vertHSV_Weighted3_50 = new Vector3(0.33f, 1.0f, 1.0f);//초록색
		//private static Vector3 _vertHSV_Weighted3_75 = new Vector3(0.167f, 1.0f, 1.0f);//노랑색
		//private static Vector3 _vertHSV_Weighted3_100 = new Vector3(0.0f, 1.0f, 1.0f);//빨강색
		//H : 0 - 0.167 - 0.33 (빨강 > 노랑 > 초록)
		//H : 0.5 - 0.667 - 0.83 (하늘색 > 파랑 > 보라)

		//색상을 리턴하는 함수를 Delegate로 만들어서 빠르게 전환하도록 한다. (20.3.28)
		public delegate Color FUNC_GET_GRADIENT_COLOR(float weight);
		private static FUNC_GET_GRADIENT_COLOR _func_GetWeightColor3 = null;
		private static FUNC_GET_GRADIENT_COLOR _func_GetWeightColor3_Vert = null;



		private static Color _vertColor_Weighted4_0_Null = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted4_0 = new Color(1.0f, 0.5f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted4_33 = new Color(0.0f, 1.0f, 0.0f, 1.0f);
		private static Color _vertColor_Weighted4_66 = new Color(0.0f, 1.0f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted4_100 = new Color(1.0f, 0.0f, 1.0f, 1.0f);

		private static Color _vertColor_Weighted4Vert_Null = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted4Vert_0 = new Color(1.0f, 0.5f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted4Vert_33 = new Color(0.2f, 1.0f, 0.2f, 1.0f);
		private static Color _vertColor_Weighted4Vert_66 = new Color(0.2f, 1.0f, 1.0f, 1.0f);
		private static Color _vertColor_Weighted4Vert_100 = new Color(1.0f, 0.2f, 1.0f, 1.0f);


		//본 선택 색상 
		//V1
		//- 메인 : 붉은색 / R: 노란색
		//- 서브 : 밝은 주황색 / R: 연두색 > 서브 색상은 사용하지 않는다.
		//- 링크 : 붉은색 / R: 노란색 (더 투명함) 
		private static Color _lineColor_BoneOutline_V1_Default = new Color(1.0f, 0.0f, 0.2f, 0.8f);
		private static Color _lineColor_BoneOutline_V1_Reverse = new Color(1.0f, 0.8f, 0.0f, 0.8f);//Default색과 유사한 경우 두번째 색상을 이용한다.
		//private static Color _lineColor_BoneOutlineSub_V1_Default = new Color(1.0f, 0.6f, 0.2f, 0.8f);
		//private static Color _lineColor_BoneOutlineSub_V1_Reverse = new Color(0.5f, 1.0f, 0.0f, 0.8f);
		private static Color _lineColor_BoneOutlineRollOver_V1_Default = new Color(1.0f, 0.2f, 0.0f, 0.5f);
		private static Color _lineColor_BoneOutlineRollOver_V1_Reverse = new Color(1.0f, 1.0f, 0.0f, 0.5f);

		//V2
		//- 메인 : 붉은색 / R: 밝은 노란색
		//- 서브 : 밝은 주황색 / R: 밝은 연두색
		//- 링크 : 붉은 주황색 / R: 밝은 노란색 (더 투명함)
		private static Color _lineColor_BoneOutline_V2_Default = new Color(1.0f, 0.0f, 0.1f, 0.9f);
		private static Color _lineColor_BoneOutline_V2_Reverse = new Color(1.0f, 0.9f, 0.5f, 0.9f);
		//private static Color _lineColor_BoneOutlineSub_V2_Default = new Color(1.0f, 0.7f, 0.3f, 0.9f);
		//private static Color _lineColor_BoneOutlineSub_V2_Reverse = new Color(0.6f, 1.0f, 0.3f, 0.9f);
		private static Color _lineColor_BoneOutlineRollOver_V2_Default = new Color(1.0f, 0.4f, 0.0f, 0.7f);
		private static Color _lineColor_BoneOutlineRollOver_V2_Reverse = new Color(1.0f, 1.0f, 0.5f, 0.7f);
		
		//선택된 본 외곽선의 반짝임
		private static float _animRatio_BoneOutlineAlpha = 0.0f;
		private static float _animCount_BoneOutlineAlpha = 0.0f;
		private const float ANIM_LENGTH_BONE_OUTLINE_ALPHA = 2.4f;

		//선택된 본의 리깅 영역의 반짝임 (옵션)
		private static float _animRatio_SelectedRigFlashing = 0.0f;
		private static float _animCount_SelectedRigFlashing = 0.0f;
		private const float ANIM_LENGTH_SELECTED_RIG_FLASHING = 0.6f;

		private const float COLOR_SIMILAR_BIAS = 0.3f;
		private const float BRIGHTNESS_OUTLINE = 0.3f;//너무 어두워도 Reverse 색상을 사용하자

		private static Texture2D _img_VertPhysicMain = null;
		private static Texture2D _img_VertPhysicConstraint = null;
		//private static Texture2D _img_RigCircle = null;

		private static Color _toneColor = new Color(0.1f, 0.3f, 0.5f, 0.7f);
		private static float _toneLineThickness = 0.0f;
		private static float _toneShapeRatio = 0.0f;
		private static Vector2 _tonePosOffset = Vector2.zero;
		private static Color _toneBoneColor = new Color(1, 1, 1, 0.9f);


		//애니메이션 GUI를 위한 타이머
		private static System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();
		private static float _animationTimeRatio = 0.0f;
		private static float _animationTimeCount = 0.0f;
		private const float ANIMATION_TIME_LENGTH = 0.9f;
		private const float ANIMATED_LINE_UNIT_LENGTH = 10.0f;
		private const float ANIMATED_LINE_SPACE_LENGTH = 6.0f;


		//리깅 버텍스의 크기를 별도의 변수로 두어서 옵션으로 컨트롤할 수 있게 만들자. (20.3.25)
		private const float RIG_CIRCLE_SIZE_NORIG = 14.0f;
		private const float RIG_CIRCLE_SIZE_NORIG_SELECTED = 18.0f;
		public const float RIG_CIRCLE_SIZE_NORIG_CLICK_SIZE = 12.0f;
		private const float RIG_CIRCLE_SIZE_DEF = 16.0f;
		private const float RIG_CIRCLE_ENLARGED_SCALE_RATIO = 1.5f;

		private static float _rigCircleSize_NoSelectedVert = 14.0f;
		private static float _rigCircleSize_NoSelectedVert_Enlarged = 14.0f;
		private static float _rigCircleSize_SelectedVert = 14.0f;
		private static float _rigCircleSize_SelectedVert_Enlarged = 24.0f;

		//클릭 영역을 정하자. 가능한 작게 설정
		private static float _rigCircleSize_ClickSize_Rigged = 10.0f;
		public static float RigCircleSize_Clickable
		{
			get
			{	
				return Mathf.Max((_isRigCircleScaledByZoom ? (_rigCircleSize_ClickSize_Rigged * _zoom) : _rigCircleSize_ClickSize_Rigged), 12.0f);
			}
		}

		private static bool _isRigCircleScaledByZoom = false;
		//private static apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE _rigSelectedWeightGUIType = apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE.Enlarged;
		private static bool _isRigSelectedWeightArea_Enlarged = false;
		private static bool _isRigSelectedWeightArea_Flashing = false;
		private static apEditor.RIG_WEIGHT_GRADIENT_COLOR _rigGradientColorType = apEditor.RIG_WEIGHT_GRADIENT_COLOR.Default;


		//버텍스, 핀 렌더링 크기
		//변경 v1.4.2 : 고정 값이 아닌 옵션에 의해 결정된다.
		//public const float VERTEX_RENDER_SIZE = 20.0f;
		//public const float PIN_RENDER_SIZE = 26.0f;
		//public const float PIN_LINE_THICKNESS = 2.5f;
		private static float _vertexRenderSizeHalf = 0.0f;
		private static float _pinRenderSizeHalf = 0.0f;
		private static float _pinLineThickness = 0.0f;


		//------------------------------------------------------------------------
		public class MaterialBatch
		{
			public enum MatType
			{
				None, Color,
				Texture_Normal, Texture_VColorAdd,
				//MaskedTexture,//<<구형 방식
				MaskOnly, Clipped,
				GUITexture,
				ToneColor_Normal, ToneColor_Clipped,
				
				//Capture용 Shader
				Alpha2White,

				//추가 20.3.20 : 본 렌더링의 v2 방식
				BoneV2,
				//추가 20.3.25 : 텍스쳐*VertColor (_Color없음)
				Texture_VColorMul,
				//리깅용 Circle Vert v2 방식
				RigCircleV2,

				//추가 21.2.16 : 비활성화된 객체 선택
				Gray_Normal, Gray_Clipped,

				//추가 22.4.12 [v1.4.0] 버텍스, 핀 그리기
				VertexAndPin,

			}
			private Material _mat_Color = null;
			private Material _mat_MaskOnly = null;

			//추가 : 일반 Texture Transparent같지만 GUI 전용이며 _Color가 없고 Vertex Color를 사용하여 Batch하기에 좋다.
			private Material _mat_GUITexture = null;

			private Material[] _mat_Texture_Normal = null;
			private Material[] _mat_Texture_VColorAdd = null;
			//private Material[] _mat_MaskedTexture = null;
			private Material[] _mat_Clipped = null;

			private Material _mat_ToneColor_Normal = null;
			private Material _mat_ToneColor_Clipped = null;

			private Material _mat_Alpha2White = null;

			private Material _mat_BoneV2 = null;//추가 20.3.20
			private Material _mat_Texture_VColorMul = null;//추가 20.3.25
			private Material _mat_RigCircleV2 = null;

			private Material _mat_Gray_Normal = null;//추가 21.2.16 : 비활성화된 객체를 표현하기 위한 재질
			private Material _mat_Gray_Clipped = null;//추가 21.2.16 : 비활성화된 객체를 표현하기 위한 재질

			private Material _mat_VertAndPin = null;//추가 22.4.12 : 버텍스와 핀 재질

			private MatType _matType = MatType.None;

			//마지막 입력 값
			private Vector4 _glScreenClippingSize = Vector4.zero;

			public Color _color = Color.black;
			private Texture2D _texture = null;


			//추가 21.5.18 : SetPass, Begin, End 호출 횟수를 줄이기 위해서, 이전 요청과 동일하면 Begin을 하지 않는다.
			//설명
			//: 이전에는 무조건 Begin+SetPass > End
			//: Begin-End를 직접 명시하지 않는다.
			//> DynamicBegin > DynamicBegin > ... > ForceEnd 로 호출한다. 즉, 마지막만 End 호출
			/// <summary>이전에 렌더링을 하고 있는 중이었는가.</summary>
			private bool _isRenderingBegun = false;
			private int _lastGLMode = -1;

			private float _lastToneLineThickness = 0.0f;
			private float _lastToneShapeRatio = 0.0f;
			private Vector2 _lastTonePosOffset = Vector2.zero;
			private float _lastVertColorRatio = 0.0f;
			//private Color _lastParentColor = Color.black;
			//클리핑된건 병합을 막자
			//private RenderTexture _lastRenderTexture = null;
			//private Texture2D _lastMaskedTexutre = null;

			private const float PASS_EQUAL_BIAS = 0.001f;


			//마스크 버전은 좀 많다..
			private RenderTexture _renderTexture = null;
			private int _renderTextureSize_Width = -1;
			private int _renderTextureSize_Height = -1;
			public RenderTexture RenderTex { get { return _renderTexture; } }

			public const int ALPHABLEND = 0;
			public const int ADDITIVE = 1;
			public const int SOFT_ADDITIVE = 2;
			public const int MULTIPLICATIVE = 3;

			private apPortrait.SHADER_TYPE _shaderType_Main = apPortrait.SHADER_TYPE.AlphaBlend;
			private int _iShaderType_Main = -1;
			
			//쉐이더 프로퍼티 인덱스
			private int _propertyID__ScreenSize = -1;
			private int _propertyID__Color = -1;
			private int _propertyID__MainTex = -1;
			private int _propertyID__Thickness = -1;
			private int _propertyID__ShapeRatio = -1;
			private int _propertyID__PosOffsetX = -1;
			private int _propertyID__PosOffsetY = -1;
			private int _propertyID__vColorITP = -1;
			private int _propertyID__MaskRenderTexture = -1;
			private int _propertyID__MaskColor = -1;

			

			public MaterialBatch()
			{
				
			}

			public void SetShader(Shader shader_Color,
									Shader[] shader_Texture_Normal_Set,
									Shader[] shader_Texture_VColorAdd_Set,
									//Shader[] shader_MaskedTexture_Set,
									Shader shader_MaskOnly,
									Shader[] shader_Clipped_Set,
									Shader shader_GUITexture,
									Shader shader_ToneColor_Normal,
									Shader shader_ToneColor_Clipped,
									Shader shader_Alpha2White,
									Shader shader_BoneV2, Texture2D uniformTexture_BoneSpriteSheet,
									Shader shader_Texture_VColorMul,
									Shader shader_RigCircleV2, Texture2D uniformTexture_RigCircle,
									Shader shader_Gray_Normal, Shader shader_Gray_Clipped,
									Shader shader_VertPin, Texture2D uniformTexture_VertPinAtlas
									)
			{
				//_mat_Color = mat_Color;
				//_mat_Texture = mat_Texture;
				//_mat_MaskedTexture = mat_MaskedTexture;

				_mat_Color = new Material(shader_Color);
				_mat_Color.color = new Color(1, 1, 1, 1);

				_mat_MaskOnly = new Material(shader_MaskOnly);
				_mat_MaskOnly.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				//추가 : GUI용 텍스쳐
				_mat_GUITexture = new Material(shader_GUITexture);


				//AlphaBlend, Add, SoftAdditive
				_mat_Texture_Normal = new Material[4];
				_mat_Texture_VColorAdd = new Material[4];
				//_mat_MaskedTexture = new Material[4];
				_mat_Clipped = new Material[4];

				for (int i = 0; i < 4; i++)
				{
					_mat_Texture_Normal[i] = new Material(shader_Texture_Normal_Set[i]);
					_mat_Texture_Normal[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

					_mat_Texture_VColorAdd[i] = new Material(shader_Texture_VColorAdd_Set[i]);
					_mat_Texture_VColorAdd[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

					//_mat_MaskedTexture[i] = new Material(shader_MaskedTexture_Set[i]);
					//_mat_MaskedTexture[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

					_mat_Clipped[i] = new Material(shader_Clipped_Set[i]);
					_mat_Clipped[i].color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}

				_mat_ToneColor_Normal = new Material(shader_ToneColor_Normal);
				_mat_ToneColor_Normal.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				_mat_ToneColor_Clipped = new Material(shader_ToneColor_Clipped);
				_mat_ToneColor_Clipped.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				_mat_Alpha2White = new Material(shader_Alpha2White);
				_mat_Alpha2White.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);

				_mat_BoneV2 = new Material(shader_BoneV2);
				if (_mat_BoneV2 != null && uniformTexture_BoneSpriteSheet != null)
				{
					_mat_BoneV2.mainTexture = uniformTexture_BoneSpriteSheet;
				}

				_mat_Texture_VColorMul = new Material(shader_Texture_VColorMul);

				_mat_RigCircleV2 = new Material(shader_RigCircleV2);
				if(_mat_RigCircleV2 != null && uniformTexture_RigCircle != null)
				{
					_mat_RigCircleV2.mainTexture = uniformTexture_RigCircle;
				}

				//추가 21.2.16 : 비활성화된 객체를 표현하기 위한 재질
				_mat_Gray_Normal = new Material(shader_Gray_Normal);
				_mat_Gray_Clipped = new Material(shader_Gray_Clipped);

				//추가 22.4.12 : 핀-버텍스 쉐이더.
				_mat_VertAndPin = new Material(shader_VertPin);
				if(_mat_VertAndPin != null && uniformTexture_VertPinAtlas != null)
				{
					_mat_VertAndPin.mainTexture = uniformTexture_VertPinAtlas;
				}


				//쉐이더 프로퍼티
				_propertyID__ScreenSize =	Shader.PropertyToID("_ScreenSize");
				_propertyID__Color =		Shader.PropertyToID("_Color");
				_propertyID__MainTex =		Shader.PropertyToID("_MainTex");
				_propertyID__Thickness =	Shader.PropertyToID("_Thickness");
				_propertyID__ShapeRatio =	Shader.PropertyToID("_ShapeRatio");
				_propertyID__PosOffsetX =	Shader.PropertyToID("_PosOffsetX");
				_propertyID__PosOffsetY =	Shader.PropertyToID("_PosOffsetY");
				_propertyID__vColorITP =	Shader.PropertyToID("_vColorITP");
				_propertyID__MaskRenderTexture = Shader.PropertyToID("_MaskRenderTexture");
				_propertyID__MaskColor =	Shader.PropertyToID("_MaskColor");

				_isRenderingBegun = false;
				_lastGLMode = -1;
			}

			#region [미사용 코드]
			//public void SetMaterialType_Color()
			//{
			//	_matType = MatType.Color;
			//}
			//public void SetMaterialType_Texture_Normal(apPortrait.SHADER_TYPE shaderType)
			//{
			//	_matType = MatType.Texture_Normal;
			//	_shaderType_Main = (int)shaderType;
			//}
			//public void SetMaterialType_Texture_VColorAdd(apPortrait.SHADER_TYPE shaderType)
			//{
			//	_matType = MatType.Texture_VColorAdd;
			//	_shaderType_Main = (int)shaderType;
			//}
			//public void SetMaterialType_MaskedTexture(apPortrait.SHADER_TYPE shaderTypeMain,
			//											apPortrait.SHADER_TYPE shaderTypeClip1,
			//											apPortrait.SHADER_TYPE shaderTypeClip2,
			//											apPortrait.SHADER_TYPE shaderTypeClip3)
			//{
			//	_matType = MatType.MaskedTexture;
			//	_shaderType_Main = (int)shaderTypeMain;
			//	_shaderType_Clip1 = (int)shaderTypeClip1;
			//	_shaderType_Clip2 = (int)shaderTypeClip2;
			//	_shaderType_Clip3 = (int)shaderTypeClip3;

			//	_shaderTypeColor_Clip1 = ShaderTypeColor[_shaderType_Clip1];
			//	_shaderTypeColor_Clip2 = ShaderTypeColor[_shaderType_Clip2];
			//	_shaderTypeColor_Clip3 = ShaderTypeColor[_shaderType_Clip3];
			//}

			//public void SetMaterialType_MaskAndClipped(apPortrait.SHADER_TYPE shaderTypeMain,
			//											apPortrait.SHADER_TYPE shaderTypeClip1,
			//											apPortrait.SHADER_TYPE shaderTypeClip2,
			//											apPortrait.SHADER_TYPE shaderTypeClip3)
			//{
			//	_shaderType_Main = (int)shaderTypeMain;
			//	_shaderType_Clip1 = (int)shaderTypeClip1;
			//	_shaderType_Clip2 = (int)shaderTypeClip2;
			//	_shaderType_Clip3 = (int)shaderTypeClip3;

			//	_shaderTypeColor_Clip1 = ShaderTypeColor[_shaderType_Clip1];
			//	_shaderTypeColor_Clip2 = ShaderTypeColor[_shaderType_Clip2];
			//	_shaderTypeColor_Clip3 = ShaderTypeColor[_shaderType_Clip3];
			//} 
			#endregion

			public void SetClippingSize(Vector4 screenSize)
			{
				_glScreenClippingSize = screenSize;

				switch (_matType)
				{
					case MatType.Color:
						_mat_Color.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.Texture_Normal:
						_mat_Texture_Normal[_iShaderType_Main].SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.Texture_VColorAdd:
						_mat_Texture_VColorAdd[_iShaderType_Main].SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.Clipped:
						_mat_Clipped[_iShaderType_Main].SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.MaskOnly:
						_mat_MaskOnly.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.GUITexture:
						_mat_GUITexture.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.ToneColor_Normal:
						_mat_ToneColor_Normal.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.ToneColor_Clipped:
						_mat_ToneColor_Clipped.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.Alpha2White:
						_mat_Alpha2White.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.BoneV2:
						if(_mat_BoneV2 != null)
						{
							_mat_BoneV2.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						}
						break;

					case MatType.Texture_VColorMul:
						_mat_Texture_VColorMul.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.RigCircleV2:
						_mat_RigCircleV2.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.Gray_Normal:
						_mat_Gray_Normal.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.Gray_Clipped:
						_mat_Gray_Clipped.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;

					case MatType.VertexAndPin:
						_mat_VertAndPin.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
						break;
				}



				//GL.Flush();
			}


			public void SetClippingSizeToAllMaterial(Vector4 screenSize)
			{
				_glScreenClippingSize = screenSize;
				//_ScreenSize

				_mat_Color.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				for (int i = 0; i < 4; i++)
				{
					_mat_Texture_Normal[i].SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
					_mat_Texture_VColorAdd[i].SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
					_mat_Clipped[i].SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				}
				
				_mat_MaskOnly.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				_mat_GUITexture.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				_mat_ToneColor_Normal.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				_mat_ToneColor_Clipped.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				_mat_Alpha2White.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				
				if (_mat_BoneV2 != null)
				{
					//Bone Material은 Null일 수 있다.
					_mat_BoneV2.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				}
				_mat_Texture_VColorMul.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);
				_mat_RigCircleV2.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize

				_mat_Gray_Normal.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
				_mat_Gray_Clipped.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
				if(_mat_VertAndPin != null)
				{
					_mat_VertAndPin.SetVector(_propertyID__ScreenSize, _glScreenClippingSize);//_ScreenSize
				}
				
			}

			/// <summary>
			/// RenderTexture를 사용하는 GL계열에서는 이 함수를 윈도우 크기 호출시에 같이 호출한다.
			/// </summary>
			/// <param name="windowWidth"></param>
			/// <param name="windowHeight"></param>
			public void CheckMaskTexture(int windowWidth, int windowHeight)
			{
				//if(_renderTexture == null || _renderTextureSize_Width != windowWidth || _renderTextureSize_Height != windowHeight)
				//{
				//	if(_renderTexture != null)
				//	{
				//		//UnityEngine.Object.DestroyImmediate(_renderTexture);
				//		RenderTexture.ReleaseTemporary(_renderTexture);
				//		_renderTexture = null;
				//	}
				//	//_renderTexture = new RenderTexture(windowWidth, windowHeight, 24);
				//	_renderTexture = RenderTexture.GetTemporary(windowWidth, windowHeight, 24);
				//	_renderTexture.wrapMode = TextureWrapMode.Clamp;
				//	_renderTextureSize_Width = windowWidth;
				//	_renderTextureSize_Height = windowHeight;
				//}

				_renderTextureSize_Width = windowWidth;
				_renderTextureSize_Height = windowHeight;
			}

			//변경 21.5.18 : 다이나믹 Begin-End 방식으로 모두 변경하자
			//렌더링 중이었으면
			// > 연속적으로 Pass를 유지할 수 없다면 > End 후 Pass+Begin 시작
			// > 연속적으로 Pass를 유지할 수 있다면 > 리턴
			//렌더링 중이 아니었다면
			// > Pass 시작

			/// <summary>
			/// 강제로 현재 Pass를 종료한다. (렌더링중인 Pass가 있다면 동작. 그렇지 않으면 무시한다.
			/// 렌더링 단계가 종료되었거나 Screen Space가 바뀌면 꼭 호출한다.
			/// </summary>
			public void EndPass()
			{
				if(!_isRenderingBegun)
				{
					return;
				}

				GL.End();
				GL.Flush();

				_isRenderingBegun = false;
				_lastGLMode = -1;

				_lastToneLineThickness = 0.0f;
				_lastToneShapeRatio = 0.0f;
				_lastTonePosOffset = Vector2.zero;
				_lastVertColorRatio = 0.0f;
				//_lastParentColor = Color.black;
				//_lastRenderTexture = null;
				//_lastMaskedTexutre = null;
			}

			private bool IsColorDifferent(Color colorA, Color colorB)
			{
				return Mathf.Abs(colorA.r - colorB.r) > 0.002f
					|| Mathf.Abs(colorA.g - colorB.g) > 0.002f
					|| Mathf.Abs(colorA.b - colorB.b) > 0.002f
					|| Mathf.Abs(colorA.a - colorB.a) > 0.002f;
					
			}

			public void BeginPass_Color(int GLMode)
			{	
				if(_isRenderingBegun)
				{
					if(_matType != MatType.Color 
						|| _lastGLMode != GLMode)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						//Debug.Log("Pass 유지 - Color");
						return;
					}
				}

				//Pass 시작
				_matType = MatType.Color;

				_mat_Color.color = new Color(1, 1, 1, 1);

				_mat_Color.SetPass(0);

				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}

			public void BeginPass_GUITexture(int GLMode, Texture2D texture)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.GUITexture 
						|| _lastGLMode != GLMode
						|| _texture != texture)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						//Debug.Log("Pass 유지 - GUI Texture");
						return;
					}
				}

				_matType = MatType.GUITexture;

				_texture = texture;
				_mat_GUITexture.SetTexture(_propertyID__MainTex, _texture);//_MainTex

				_mat_GUITexture.SetPass(0);				

				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				//GL.sRGBWrite = true;
			}

			public void BeginPass_Texture_Normal(int GLMode, Color color, Texture2D texture, apPortrait.SHADER_TYPE shaderType)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.Texture_Normal 
						|| _shaderType_Main != shaderType
						|| _lastGLMode != GLMode
						|| _texture != texture
						|| IsColorDifferent(_color, color)
						)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						//Debug.Log("Pass 유지 - Texture Normal");
						return;
					}
				}

				_matType = MatType.Texture_Normal;

				_shaderType_Main = shaderType;
				_iShaderType_Main = (int)_shaderType_Main;
				_color = color;
				_texture = texture;

				_mat_Texture_Normal[_iShaderType_Main].SetColor(_propertyID__Color, _color);//_Color				
				_mat_Texture_Normal[_iShaderType_Main].SetTexture(_propertyID__MainTex, _texture);//_MainTex

				_mat_Texture_Normal[_iShaderType_Main].SetPass(0);

				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}

			public void BeginPass_ToneColor_Normal(int GLMode, Color color, Texture2D texture)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.ToneColor_Normal 
						|| _lastGLMode != GLMode
						|| _texture != texture
						|| IsColorDifferent(_color, color)
						|| Mathf.Abs(_lastToneLineThickness - _toneLineThickness) > PASS_EQUAL_BIAS
						|| Mathf.Abs(_lastToneShapeRatio - _toneShapeRatio) > PASS_EQUAL_BIAS
						|| Mathf.Abs(_lastTonePosOffset.x - (_tonePosOffset.x * _zoom)) > PASS_EQUAL_BIAS
						|| Mathf.Abs(_lastTonePosOffset.y - (_tonePosOffset.y * _zoom)) > PASS_EQUAL_BIAS
						)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.ToneColor_Normal;
				
				_color = color;
				_texture = texture;

				_mat_ToneColor_Normal.SetColor(_propertyID__Color, _color);//_Color
				_mat_ToneColor_Normal.SetFloat(_propertyID__Thickness, _toneLineThickness);//_Thickness
				_mat_ToneColor_Normal.SetFloat(_propertyID__ShapeRatio, _toneShapeRatio);//_ShapeRatio
				_mat_ToneColor_Normal.SetFloat(_propertyID__PosOffsetX, _tonePosOffset.x * _zoom);//_PosOffsetX
				_mat_ToneColor_Normal.SetFloat(_propertyID__PosOffsetY, _tonePosOffset.y * _zoom);//_PosOffsetY
								
				_mat_ToneColor_Normal.SetTexture(_propertyID__MainTex, _texture);//_MainTex

				_mat_ToneColor_Normal.SetPass(0);
				
				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				_lastToneLineThickness = _toneLineThickness;
				_lastToneShapeRatio = _toneShapeRatio;
				_lastTonePosOffset.x = _tonePosOffset.x * _zoom;
				_lastTonePosOffset.y = _tonePosOffset.y * _zoom;
			}

			public void BeginPass_ToneColor_Custom(int GLMode, Color color, Texture2D texture, float thickness, float shapeRatio)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.ToneColor_Normal 
						|| _lastGLMode != GLMode
						|| _texture != texture
						|| IsColorDifferent(_color, color)
						|| Mathf.Abs(_lastToneLineThickness - thickness) > PASS_EQUAL_BIAS
						|| Mathf.Abs(_lastToneShapeRatio - shapeRatio) > PASS_EQUAL_BIAS
						|| Mathf.Abs(_lastTonePosOffset.x - 0.0f) > PASS_EQUAL_BIAS
						|| Mathf.Abs(_lastTonePosOffset.y - 0.0f) > PASS_EQUAL_BIAS
						)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.ToneColor_Normal;

				_color = color;
				_texture = texture;

				_mat_ToneColor_Normal.SetColor(_propertyID__Color, _color);//_Color
				_mat_ToneColor_Normal.SetFloat(_propertyID__Thickness, thickness);//_Thickness
				_mat_ToneColor_Normal.SetFloat(_propertyID__ShapeRatio, shapeRatio);//_ShapeRatio
				_mat_ToneColor_Normal.SetFloat(_propertyID__PosOffsetX, 0.0f);//_PosOffsetX
				_mat_ToneColor_Normal.SetFloat(_propertyID__PosOffsetY, 0.0f);//_PosOffsetY
				_mat_ToneColor_Normal.SetTexture(_propertyID__MainTex, _texture);//_MainTex

				_mat_ToneColor_Normal.SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				_lastToneLineThickness = thickness;
				_lastToneShapeRatio = shapeRatio;
				_lastTonePosOffset.x = 0.0f;
				_lastTonePosOffset.y = 0.0f;
			}

			public void BeginPass_Texture_VColor(	int GLMode, Color color, Texture2D texture, 
													float vertColorRatio, 
													apPortrait.SHADER_TYPE shaderType, 
													bool isSetScreenSize, Vector4 screenSize)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.Texture_VColorAdd 
						|| _shaderType_Main != shaderType
						|| _lastGLMode != GLMode
						|| _texture != texture
						|| IsColorDifferent(_color, color)
						|| Mathf.Abs(_lastVertColorRatio - vertColorRatio) > PASS_EQUAL_BIAS
						)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						//Debug.Log("Pass 유지 - Texture V Color");
						return;
					}
				}

				_matType = MatType.Texture_VColorAdd;

				_shaderType_Main = shaderType;
				_iShaderType_Main = (int)_shaderType_Main;

				_color = color;
				_texture = texture;

				_mat_Texture_VColorAdd[_iShaderType_Main].SetColor(_propertyID__Color, _color);//_Color
				_mat_Texture_VColorAdd[_iShaderType_Main].SetTexture(_propertyID__MainTex, _texture);//_MainTex
				_mat_Texture_VColorAdd[_iShaderType_Main].SetFloat(_propertyID__vColorITP, vertColorRatio);//_vColorITP
				
				if(isSetScreenSize)
				{
					SetClippingSize(screenSize);
				}

				_mat_Texture_VColorAdd[_iShaderType_Main].SetPass(0);
				

				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				_lastVertColorRatio = vertColorRatio;
			}

			

			public void BeginPass_Mask(int GLMode, Color color, Texture2D texture,
									float vertColorRatio, apPortrait.SHADER_TYPE shaderType,
									bool isRenderMask,
									bool isSetScreenSize,
									Vector4 screenSize
									)
			{
				//Mask는 무조건 Pass를 시작해야한다.
				//조건 체크후 return하는 구문이 없다.
				if(_isRenderingBegun)
				{
					//End 후 Pass 시작
					GL.End();
					GL.Flush();
				}


				_shaderType_Main = shaderType;
				_iShaderType_Main = (int)_shaderType_Main;

				_color = color;
				_texture = texture;

				if (isRenderMask)
				{
					//RenderTexture로 만든다.
					_matType = MatType.MaskOnly;

					//RenderTexture를 활성화한다.
					_renderTexture = RenderTexture.GetTemporary(_renderTextureSize_Width, _renderTextureSize_Height, 8);
					_renderTexture.wrapMode = TextureWrapMode.Clamp;

					//RenderTexture를 사용
					RenderTexture.active = _renderTexture;

					//[중요] Temp RenderTexture는 색상 초기화가 안되어있다. 꼭 해준다.
					GL.Clear(true, true, Color.clear, 0.0f);


					_mat_MaskOnly.SetColor(_propertyID__Color, _color);//_Color
					_mat_MaskOnly.SetTexture(_propertyID__MainTex, _texture);//_MainTex
					_mat_MaskOnly.SetFloat(_propertyID__vColorITP, vertColorRatio);//_vColorITP
					_mat_MaskOnly.SetFloat(_propertyID__PosOffsetX, 0);//_PosOffsetX
					_mat_MaskOnly.SetFloat(_propertyID__PosOffsetY, 0);//_PosOffsetY

					if(isSetScreenSize)
					{
						SetClippingSize(screenSize);
					}

					_mat_MaskOnly.SetPass(0);

					//GL.Begin 및 정보 저장
					GL.Begin(GLMode);

					_isRenderingBegun = true;
					_lastGLMode = GLMode;

					_lastVertColorRatio = vertColorRatio;
					_lastTonePosOffset.x = 0.0f;
					_lastTonePosOffset.y = 0.0f;


				}
				else
				{
					_matType = MatType.Texture_VColorAdd;

					_mat_Texture_VColorAdd[_iShaderType_Main].SetColor(_propertyID__Color, _color);//_Color
					_mat_Texture_VColorAdd[_iShaderType_Main].SetTexture(_propertyID__MainTex, _texture);//_MainTex
					_mat_Texture_VColorAdd[_iShaderType_Main].SetFloat(_propertyID__vColorITP, vertColorRatio);//_vColorITP

					if(isSetScreenSize)
					{
						SetClippingSize(screenSize);
					}

					_mat_Texture_VColorAdd[_iShaderType_Main].SetPass(0);


					//GL.Begin 및 정보 저장
					GL.Begin(GLMode);

					_isRenderingBegun = true;
					_lastGLMode = GLMode;

					_lastVertColorRatio = vertColorRatio;
				}
			}


			public void BeginPass_Mask_Gray(int GLMode, Color color, Texture2D texture, bool isRenderMask)
			{
				//Mask는 무조건 Pass를 시작해야한다.
				//조건 체크후 return하는 구문이 없다.
				if(_isRenderingBegun)
				{
					//End 후 Pass 시작
					GL.End();
					GL.Flush();
				}


				_color = color;
				_texture = texture;

				if (isRenderMask)
				{
					//RenderTexture로 만든다.
					_matType = MatType.MaskOnly;

					//RenderTexture를 활성화한다.
					_renderTexture = RenderTexture.GetTemporary(_renderTextureSize_Width, _renderTextureSize_Height, 8);
					_renderTexture.wrapMode = TextureWrapMode.Clamp;

					//RenderTexture를 사용
					RenderTexture.active = _renderTexture;

					//[중요] Temp RenderTexture는 색상 초기화가 안되어있다. 꼭 해준다.
					GL.Clear(true, true, Color.clear, 0.0f);


					_mat_MaskOnly.SetColor(_propertyID__Color, _color);//_Color
					_mat_MaskOnly.SetTexture(_propertyID__MainTex, _texture);//_MainTex
					//_mat_MaskOnly.SetFloat(_propertyID__vColorITP, vertColorRatio);//_vColorITP
					_mat_MaskOnly.SetFloat(_propertyID__PosOffsetX, 0);//_PosOffsetX
					_mat_MaskOnly.SetFloat(_propertyID__PosOffsetY, 0);//_PosOffsetY
					_mat_MaskOnly.SetPass(0);

					//GL.Begin 및 정보 저장
					GL.Begin(GLMode);

					_isRenderingBegun = true;
					_lastGLMode = GLMode;

					_lastTonePosOffset.x = 0.0f;
					_lastTonePosOffset.y = 0.0f;
				}
				else
				{
					_matType = MatType.Gray_Normal;
					
					_mat_Gray_Normal.SetColor(_propertyID__Color, _color);//_Color
					_mat_Gray_Normal.SetTexture(_propertyID__MainTex, _texture);//_MainTex
					_mat_Gray_Normal.SetPass(0);

					//GL.Begin 및 정보 저장
					GL.Begin(GLMode);

					_isRenderingBegun = true;
					_lastGLMode = GLMode;
				}
			}

			public void BeginPass_Clipped(int GLMode, Color color, Texture2D texture, float vertColorRatio, apPortrait.SHADER_TYPE shaderType, Color parentColor)
			{	
				if(_isRenderingBegun)
				{
					//RenderTexture를 이용하는 경우엔 Pass를 유지하지 않는다.
					//End 후 Pass 시작
					GL.End();
					GL.Flush();
				}


				_matType = MatType.Clipped;

				_shaderType_Main = shaderType;
				_iShaderType_Main = (int)_shaderType_Main;

				_color = color;
				_texture = texture;
				_mat_Clipped[_iShaderType_Main].SetColor(_propertyID__Color, _color);//_Color
				_mat_Clipped[_iShaderType_Main].SetTexture(_propertyID__MainTex, _texture);//_MainTex
				_mat_Clipped[_iShaderType_Main].SetFloat(_propertyID__vColorITP, vertColorRatio);//_vColorITP

				//Mask를 넣자
				_mat_Clipped[_iShaderType_Main].SetTexture(_propertyID__MaskRenderTexture, _renderTexture);//_MaskRenderTexture
				_mat_Clipped[_iShaderType_Main].SetColor(_propertyID__MaskColor, parentColor);//_MaskColor

				_mat_Clipped[_iShaderType_Main].SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				//_lastParentColor = parentColor;
				_lastVertColorRatio = vertColorRatio;
				//_lastRenderTexture = _renderTexture;
			}


			public void BeginPass_Mask_ToneColor(int GLMode, Color color, Texture2D texture, bool isRenderMask)
			{
				//Mask는 무조건 Pass를 시작해야한다.
				//조건 체크후 return하는 구문이 없다.
				if(_isRenderingBegun)
				{
					//End 후 Pass 시작
					GL.End();
					GL.Flush();
				}

				_color = color;
				_texture = texture;

				if (isRenderMask)
				{
					//RenderTexture로 만든다.
					_matType = MatType.MaskOnly;

					//RenderTexture를 활성화한다.
					_renderTexture = RenderTexture.GetTemporary(_renderTextureSize_Width, _renderTextureSize_Height, 8);
					_renderTexture.wrapMode = TextureWrapMode.Clamp;

					//RenderTexture를 사용
					RenderTexture.active = _renderTexture;

					//[중요] Temp RenderTexture는 색상 초기화가 안되어있다. 꼭 해준다.
					GL.Clear(true, true, Color.clear, 0.0f);


					_mat_MaskOnly.SetColor(_propertyID__Color, _color);//_Color
					_mat_MaskOnly.SetTexture(_propertyID__MainTex, _texture);//_MainTex
					_mat_MaskOnly.SetFloat(_propertyID__vColorITP, 0.0f);//_vColorITP
					_mat_MaskOnly.SetFloat(_propertyID__PosOffsetX, _tonePosOffset.x * _zoom);//_PosOffsetX
					_mat_MaskOnly.SetFloat(_propertyID__PosOffsetY, _tonePosOffset.y * _zoom);//_PosOffsetY
					_mat_MaskOnly.SetPass(0);


					//GL.Begin 및 정보 저장
					GL.Begin(GLMode);

					_isRenderingBegun = true;
					_lastGLMode = GLMode;

					_lastVertColorRatio = 0.0f;
					_lastTonePosOffset.x = _tonePosOffset.x * _zoom;
					_lastTonePosOffset.y = _tonePosOffset.y * _zoom;
				}
				else
				{
					_matType = MatType.ToneColor_Normal;

					_mat_ToneColor_Normal.SetColor(_propertyID__Color, _color);//_Color
					_mat_ToneColor_Normal.SetTexture(_propertyID__MainTex, _texture);//_MainTex
					_mat_ToneColor_Normal.SetFloat(_propertyID__Thickness, _toneLineThickness);//_Thickness
					_mat_ToneColor_Normal.SetFloat(_propertyID__ShapeRatio, _toneShapeRatio);//_ShapeRatio
					_mat_ToneColor_Normal.SetFloat(_propertyID__PosOffsetX, _tonePosOffset.x * _zoom);//_PosOffsetX
					_mat_ToneColor_Normal.SetFloat(_propertyID__PosOffsetY, _tonePosOffset.y * _zoom);//_PosOffsetY
					_mat_ToneColor_Normal.SetPass(0);

					//GL.Begin 및 정보 저장
					GL.Begin(GLMode);

					_isRenderingBegun = true;
					_lastGLMode = GLMode;

					_lastToneLineThickness = _toneLineThickness;
					_lastToneShapeRatio = _toneShapeRatio;
					_lastTonePosOffset.x = _tonePosOffset.x * _zoom;
					_lastTonePosOffset.y = _tonePosOffset.y * _zoom;
				}
			}



			public void BeginPass_Clipped_ToneColor(int GLMode, Color color, Texture2D texture, Color parentColor)
			{
				if(_isRenderingBegun)
				{
					//RenderTexture를 이용하는 경우엔 Pass를 유지하지 않는다.
					//End 후 Pass 시작
					GL.End();
					GL.Flush();
				}

				_matType = MatType.ToneColor_Clipped;

				_color = color;
				_texture = texture;

				_mat_ToneColor_Clipped.SetColor(_propertyID__Color, _color);//_Color
				_mat_ToneColor_Clipped.SetTexture(_propertyID__MainTex, _texture);//_MainTex
				_mat_ToneColor_Clipped.SetTexture(_propertyID__MaskRenderTexture, _renderTexture);//_MaskRenderTexture
				_mat_ToneColor_Clipped.SetColor(_propertyID__MaskColor, parentColor);//_MaskColor
				_mat_ToneColor_Clipped.SetFloat(_propertyID__Thickness, _toneLineThickness);//_Thickness
				_mat_ToneColor_Clipped.SetFloat(_propertyID__ShapeRatio, _toneShapeRatio);//_ShapeRatio
				_mat_ToneColor_Clipped.SetFloat(_propertyID__PosOffsetX, _tonePosOffset.x * _zoom);//_PosOffsetX
				_mat_ToneColor_Clipped.SetFloat(_propertyID__PosOffsetY, _tonePosOffset.y * _zoom);//_PosOffsetY

				_mat_ToneColor_Clipped.SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				//_lastRenderTexture = _renderTexture;
				//_lastParentColor = parentColor;

				_lastToneLineThickness = _toneLineThickness;
				_lastToneShapeRatio = _toneShapeRatio;
				_lastTonePosOffset.x = _tonePosOffset.x * _zoom;
				_lastTonePosOffset.y = _tonePosOffset.y * _zoom;
			}

			public void BeginPass_ClippedWithMaskedTexture(	int GLMode, 
															Color color, Texture2D texture, float vertColorRatio,
															apPortrait.SHADER_TYPE shaderType, Color parentColor,
															Texture2D maskedTexture, Vector4 screenSize)
			{
				if(_isRenderingBegun)
				{
					//RenderTexture를 이용하는 경우엔 Pass를 유지하지 않는다.
					//End 후 Pass 시작
					GL.End();
					GL.Flush();
				}

				_matType = MatType.Clipped;

				_shaderType_Main = shaderType;
				_iShaderType_Main = (int)_shaderType_Main;

				_color = color;
				_texture = texture;
				_mat_Clipped[_iShaderType_Main].SetColor(_propertyID__Color, _color);//_Color
				_mat_Clipped[_iShaderType_Main].SetTexture(_propertyID__MainTex, _texture);//_MainTex
				_mat_Clipped[_iShaderType_Main].SetFloat(_propertyID__vColorITP, vertColorRatio);//_vColorITP

				////<<Mask를 넣자
				_mat_Clipped[_iShaderType_Main].SetTexture(_propertyID__MaskRenderTexture, maskedTexture);//_MaskRenderTexture
				_mat_Clipped[_iShaderType_Main].SetColor(_propertyID__MaskColor, parentColor);//_MaskColor

				//추가 21.5.19 : ScreenSize 적용
				SetClippingSize(screenSize);

				_mat_Clipped[_iShaderType_Main].SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				//_lastMaskedTexutre = maskedTexture;
				//_lastParentColor = parentColor;

				_lastVertColorRatio = vertColorRatio;
			}

			public void BeginPass_Alpha2White(int GLMode, Color color, Texture2D texture, Vector4 screenSize)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.Alpha2White 
						|| _lastGLMode != GLMode						
						|| _texture != texture
						|| IsColorDifferent(_color, color)
						)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.Alpha2White;
				_shaderType_Main = apPortrait.SHADER_TYPE.AlphaBlend;
				_iShaderType_Main = 0;

				_color = color;
				_texture = texture;

				_mat_Alpha2White.SetColor(_propertyID__Color, _color);//_Color
				_mat_Alpha2White.SetTexture(_propertyID__MainTex, _texture);//_MainTex

				SetClippingSize(screenSize);

				_mat_Alpha2White.SetPass(0);

				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}

			public void BeginPass_BoneV2(int GLMode)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.BoneV2 
						|| _lastGLMode != GLMode)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.BoneV2;
				_shaderType_Main = apPortrait.SHADER_TYPE.AlphaBlend;
				_iShaderType_Main = 0;
				
				_mat_BoneV2.SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}

			public void BeginPass_TextureVColorMul(int GLMode, Texture2D texture)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.Texture_VColorMul 
						|| _lastGLMode != GLMode						
						|| _texture != texture
						)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.Texture_VColorMul;

				_shaderType_Main = apPortrait.SHADER_TYPE.AlphaBlend;
				_iShaderType_Main = 0;

				_texture = texture;

				_mat_Texture_VColorMul.SetTexture(_propertyID__MainTex, _texture);
				_mat_Texture_VColorMul.SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}

			public void BeginPass_RigCircleV2(int GLMode)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.RigCircleV2 
						|| _lastGLMode != GLMode)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.RigCircleV2;
				_shaderType_Main = 0;

				_mat_RigCircleV2.SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}


			public void BeginPass_Gray_Normal(int GLMode, Color color, Texture2D texture)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.Gray_Normal 
						|| _lastGLMode != GLMode
						|| _texture != texture
						|| IsColorDifferent(_color, color)
						)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.Gray_Normal;

				_color = color;
				_texture = texture;

				_mat_Gray_Normal.SetColor(_propertyID__Color, _color);//_Color
				_mat_Gray_Normal.SetTexture(_propertyID__MainTex, _texture);//_MainTex

				_mat_Gray_Normal.SetPass(0);

				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}

			public void BeginPass_Gray_Clipped(int GLMode, Color color, Texture2D texture, Color parentColor)
			{
				if(_isRenderingBegun)
				{
					//RenderTexture를 이용하는 경우엔 Pass를 유지하지 않는다.
					//End 후 Pass 시작
					GL.End();
					GL.Flush();
				}

				_matType = MatType.Gray_Clipped;
				
				_color = color;
				_texture = texture;
				_mat_Gray_Clipped.SetColor(_propertyID__Color, _color);//_Color
				_mat_Gray_Clipped.SetTexture(_propertyID__MainTex, _texture);//_MainTex
				
				//Mask를 넣자
				_mat_Gray_Clipped.SetTexture(_propertyID__MaskRenderTexture, _renderTexture);//_MaskRenderTexture
				_mat_Gray_Clipped.SetColor(_propertyID__MaskColor, parentColor);//_MaskColor

				_mat_Gray_Clipped.SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;

				//_lastRenderTexture = _renderTexture;
				//_lastParentColor = parentColor;
			}




			public void BeginPass_VertexAndPin(int GLMode)
			{
				if(_isRenderingBegun)
				{
					if(_matType != MatType.VertexAndPin 
						|| _lastGLMode != GLMode)
					{
						//End 후 Pass 시작
						GL.End();
						GL.Flush();
					}
					else
					{
						//Pass 유지
						return;
					}
				}

				_matType = MatType.VertexAndPin;
				_shaderType_Main = apPortrait.SHADER_TYPE.AlphaBlend;
				_iShaderType_Main = 0;
				
				_mat_VertAndPin.SetPass(0);


				//GL.Begin 및 정보 저장
				GL.Begin(GLMode);

				_isRenderingBegun = true;
				_lastGLMode = GLMode;
			}





			/// <summary>
			/// MultiPass에서 사용한 RenderTexture를 해제한다.
			/// 다만, 삭제하지는 않는다.
			/// </summary>
			public void DeactiveRenderTexture()
			{
				RenderTexture.active = null;
			}
			/// <summary>
			/// MultiPass의 모든 과정이 끝나면 사용했던 RenderTexture를 해제한다.
			/// </summary>
			public void ReleaseRenderTexture()
			{
				if (_renderTexture != null)
				{
					RenderTexture.active = null;
					RenderTexture.ReleaseTemporary(_renderTexture);
					_renderTexture = null;					
				}
				//_lastRenderTexture = null;
				//_lastMaskedTexutre = null;
			}
			public bool IsNotReady()
			{
				return (_mat_Color == null
					|| _mat_Texture_Normal == null
					|| _mat_Texture_VColorAdd == null
					//|| _mat_MaskedTexture == null
					|| _mat_Clipped == null
					|| _mat_MaskOnly == null
					|| _mat_GUITexture == null
					|| _mat_ToneColor_Normal == null
					|| _mat_ToneColor_Clipped == null
					|| _mat_Alpha2White == null
					|| _mat_Gray_Normal == null
					|| _mat_Gray_Clipped == null
					//|| _mat_VertAndPin == null
					);
			}

			

		}

		private static MaterialBatch _matBatch = new MaterialBatch();
		public static MaterialBatch MatBatch { get { return _matBatch; } }
		//------------------------------------------------------------------------

		public static void SetShader(Shader shader_Color,
									Shader[] shader_Texture_Normal_Set,
									Shader[] shader_Texture_VColorAdd_Set,
									//Shader[] shader_MaskedTexture_Set,
									Shader shader_MaskOnly,
									Shader[] shader_Clipped_Set,
									Shader shader_GUITexture,
									Shader shader_ToneColor_Normal,
									Shader shader_ToneColor_Clipped,
									Shader shader_Alpha2White,
									Shader shader_BoneV2, Texture2D uniformTexture_BoneSpriteSheet,
									Shader shader_TextureVColorMul,
									Shader shader_RigCircleV2, Texture2D uniformTexture_RigCircleV2,
									Shader shader_Gray_Normal, Shader shader_Gray_Clipped,
									Shader shader_VertPin, Texture2D uniformTexture_VertAndPinAtlas
			)
		{
			//_mat_Color = mat_Color;
			//_mat_Texture = mat_Texture;

			//_matBatch.SetMaterial(mat_Color, mat_Texture, mat_MaskedTexture);
			_matBatch.SetShader(shader_Color,
								shader_Texture_Normal_Set,
								shader_Texture_VColorAdd_Set,
								//shader_MaskedTexture_Set,
								shader_MaskOnly,
								shader_Clipped_Set,
								shader_GUITexture,
								shader_ToneColor_Normal,
								shader_ToneColor_Clipped,
								shader_Alpha2White,
								shader_BoneV2, uniformTexture_BoneSpriteSheet,
								shader_TextureVColorMul,
								shader_RigCircleV2, uniformTexture_RigCircleV2,
								shader_Gray_Normal, shader_Gray_Clipped,
								shader_VertPin, uniformTexture_VertAndPinAtlas);

			if(_func_GetWeightColor3 == null)
			{
				_func_GetWeightColor3 = GetWeightColor3;
			}
			if(_func_GetWeightColor3_Vert == null)
			{
				_func_GetWeightColor3_Vert = GetWeightColor3_Vert;
			}
		}

		public static void SetTexture(	Texture2D img_VertPhysicMain, 
										Texture2D img_VertPhysicConstraint
										//Texture2D img_RigCircle
			)
		{
			_img_VertPhysicMain = img_VertPhysicMain;
			_img_VertPhysicConstraint = img_VertPhysicConstraint;
			//_img_RigCircle = img_RigCircle;
		}


		public static void SetWindowSize(int windowWidth, int windowHeight, Vector2 scroll, float zoom,
			int posX, int posY, int totalEditorWidth, int totalEditorHeight)
		{
			_windowWidth = windowWidth;
			_windowHeight = windowHeight;
			_scrol_NotCalculated = scroll;
			_windowScroll.x = scroll.x * _windowWidth * 0.1f;
			_windowScroll.y = scroll.y * windowHeight * 0.1f;
			_totalEditorWidth = totalEditorWidth;
			_totalEditorHeight = totalEditorHeight;

			_posX_NotCalculated = posX;
			_posY_NotCalculated = posY;

			_zoom = zoom;

			totalEditorHeight += 30;
			posY += 30;
			posX += 5;
			windowWidth -= 25;
			windowHeight -= 20;

			//_windowPosX = posX;
			//_windowPosY = posY;

			//float leftMargin = posX;
			//float rightMargin = totalEditorWidth - (posX + windowWidth);
			//float topMargin = posY;
			//float bottomMargin = totalEditorHeight - (posY + windowHeight);

			_glScreenClippingSize.x = (float)posX / (float)totalEditorWidth;
			_glScreenClippingSize.y = (float)(posY) / (float)totalEditorHeight;
			_glScreenClippingSize.z = (float)(posX + windowWidth) / (float)totalEditorWidth;
			_glScreenClippingSize.w = (float)(posY + windowHeight) / (float)totalEditorHeight;

			_matBatch.CheckMaskTexture(_windowWidth, _windowHeight);

			

			//추가: 타이머
			float timer = (float)_stopWatch.Elapsed.TotalSeconds;
			_animationTimeCount += timer;
			_animCount_BoneOutlineAlpha += timer;
			_animCount_SelectedRigFlashing += timer;

			_stopWatch.Stop();
			_stopWatch.Reset();
			_stopWatch.Start();

			while(_animationTimeCount > ANIMATION_TIME_LENGTH)
			{
				_animationTimeCount -= ANIMATION_TIME_LENGTH;
			}
			_animationTimeRatio = Mathf.Clamp01(_animationTimeCount / ANIMATION_TIME_LENGTH);



			while(_animCount_BoneOutlineAlpha > ANIM_LENGTH_BONE_OUTLINE_ALPHA)
			{
				_animCount_BoneOutlineAlpha -= ANIM_LENGTH_BONE_OUTLINE_ALPHA;
			}
			float outlineLerp = (Mathf.Cos(Mathf.Clamp01(_animCount_BoneOutlineAlpha / ANIM_LENGTH_BONE_OUTLINE_ALPHA) * Mathf.PI * 2.0f) * 0.5f) + 0.5f;
			_animRatio_BoneOutlineAlpha = (0.5f * (1.0f - outlineLerp)) + (1.0f * outlineLerp);//최소 Alpha는 0이 아닌 0.5



			while(_animCount_SelectedRigFlashing > ANIM_LENGTH_SELECTED_RIG_FLASHING)
			{
				_animCount_SelectedRigFlashing -= ANIM_LENGTH_SELECTED_RIG_FLASHING;
			}
			_animRatio_SelectedRigFlashing = (Mathf.Cos(Mathf.Clamp01(_animCount_SelectedRigFlashing / ANIM_LENGTH_SELECTED_RIG_FLASHING) * Mathf.PI * 2.0f) * 0.5f) + 0.5f;
			
			//추가 21.5.18 : 스크린 크기는 여기서 일괄 수정한다.
			_matBatch.SetClippingSizeToAllMaterial(_glScreenClippingSize);
		}

		




		public static void SetToneOption(Color toneColor, float toneLineThickness, bool isToneOutlineRender, float tonePosOffsetX, float tonePosOffsetY, Color toneBoneColor)
		{
			_toneColor = toneColor;
			_toneLineThickness = Mathf.Clamp01(toneLineThickness);
			_toneShapeRatio = isToneOutlineRender ? 0.0f : 1.0f;
			_tonePosOffset.x = tonePosOffsetX;
			_tonePosOffset.y = tonePosOffsetY;
			_toneBoneColor = toneBoneColor;
		}


		//추가 20.3.25 : RigCircle에 대한 옵션을 설정할 수 있다.
		public static void SetRiggingOption(	int rigCircleScale_x100, 
												int rigCircleScale_x100_Selected, 
												bool isScaledByZoom, 
												apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE rigSelectedWeightGUIType,
												apEditor.RIG_WEIGHT_GRADIENT_COLOR rigGradientColorType)
		{
			float rigCircleScaleRatio = ((float)rigCircleScale_x100) * 0.01f;
			float rigCircleScaleRatio_Selected = ((float)rigCircleScale_x100_Selected) * 0.01f;

			_rigCircleSize_NoSelectedVert = RIG_CIRCLE_SIZE_DEF * rigCircleScaleRatio;
			_rigCircleSize_SelectedVert = RIG_CIRCLE_SIZE_DEF * rigCircleScaleRatio_Selected;
			
			_isRigCircleScaledByZoom = isScaledByZoom;


			_isRigSelectedWeightArea_Enlarged = rigSelectedWeightGUIType == apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE.Enlarged || rigSelectedWeightGUIType == apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE.EnlargedAndFlashing;
			_isRigSelectedWeightArea_Flashing = rigSelectedWeightGUIType == apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE.Flashing || rigSelectedWeightGUIType == apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE.EnlargedAndFlashing;

			//_rigSelectedWeightGUIType = rigSelectedWeightGUIType;
			_rigGradientColorType = rigGradientColorType;

			if (_isRigSelectedWeightArea_Enlarged)
			{
				//선택된 영역의 크기가 커지는 경우
				_rigCircleSize_NoSelectedVert_Enlarged = _rigCircleSize_NoSelectedVert * RIG_CIRCLE_ENLARGED_SCALE_RATIO;
				_rigCircleSize_SelectedVert_Enlarged = _rigCircleSize_SelectedVert * RIG_CIRCLE_ENLARGED_SCALE_RATIO;
			}
			else
			{
				//선택된 영역도 크기가 동일할 경우
				_rigCircleSize_NoSelectedVert_Enlarged = _rigCircleSize_NoSelectedVert;
				_rigCircleSize_SelectedVert_Enlarged = _rigCircleSize_SelectedVert;
			}

			//클릭 범위는 작은 범위를 기준으로 한다.
			_rigCircleSize_ClickSize_Rigged = (_rigCircleSize_NoSelectedVert < _rigCircleSize_SelectedVert) ? _rigCircleSize_NoSelectedVert : _rigCircleSize_SelectedVert;
			//_rigCircleSize_ClickSize_Rigged = Mathf.Max(_rigCircleSize_ClickSize_Rigged * 0.9f, _rigCircleSize_ClickSize_Rigged - 2.0f);//원형이므로 크기를 조금 축소한다.


			if (_rigGradientColorType == apEditor.RIG_WEIGHT_GRADIENT_COLOR.Vivid)
			{
				//Vivid 방식이다.
				_func_GetWeightColor3 = GetWeightColor3_Vivid;;
				_func_GetWeightColor3_Vert = GetWeightColor3_Vivid;
			}
			else
			{
				_func_GetWeightColor3 = GetWeightColor3;;
				_func_GetWeightColor3_Vert = GetWeightColor3_Vert;
			}
			
		}

		/// <summary>
		/// v1.4.2 : 버텍스, 핀의 크기값을 입력한다.
		/// </summary>
		public static void SetVertexPinRenderOption(	float vertRenderSizeHalf,
														float pinRenderSizeHalf,
														float pinLineThickness)
		{
			_vertexRenderSizeHalf = vertRenderSizeHalf;
			_pinRenderSizeHalf = pinRenderSizeHalf;
			_pinLineThickness = pinLineThickness;
		}



		public static Vector2 World2GL(Vector2 pos)
		{
			return new Vector2(
				(pos.x * _zoom) + (_windowWidth * 0.5f)
				- _windowScroll.x,

				(_windowHeight - (pos.y * _zoom)) - (_windowHeight * 0.5f)
				- _windowScroll.y
				);
		}

		public static Vector2 GL2World(Vector2 glPos)
		{
			return new Vector2(
				(glPos.x + (_windowScroll.x) - (_windowWidth * 0.5f)) / _zoom,
				(-1.0f * (glPos.y + _windowScroll.y + (_windowHeight * 0.5f) - (_windowHeight))) / _zoom
				);
		}

		private static bool IsVertexClipped(Vector2 posGL)
		{
			return (posGL.x < 1.0f || posGL.x > _windowWidth - 1 ||
									posGL.y < 1.0f || posGL.y > _windowHeight - 1);
		}

		private static bool Is2VertexClippedAll(Vector2 pos1GL, Vector2 pos2GL)
		{
			bool isPos1Clipped = IsVertexClipped(pos1GL);

			bool isPos2Clipped = IsVertexClipped(pos2GL);


			if (!isPos1Clipped || !isPos2Clipped)
			{
				//둘중 하나라도 안에 들어있다.
				return false;
			}


			//두 점이 밖에 나갔어도, 중간 점이 걸쳐서 들어올 수 있다.
			Vector2 posDir = pos2GL - pos1GL;
			for (int i = 1; i < 5; i++)
			{
				Vector2 posSub = pos1GL + posDir * ((float)i / 5.0f);

				bool isPosSubClipped = IsVertexClipped(posSub);

				//중간점 하나가 들어와있다.
				if (!isPosSubClipped)
				{
					return false;
				}
			}
			return true;
		}

		private static Vector2 GetClippedVertex(Vector2 posTargetGL, Vector2 posBaseGL)
		{
			Vector2 pos1_Real = posTargetGL;
			Vector2 pos2_Real = posBaseGL;

			Vector2 dir1To2 = (pos2_Real - pos1_Real).normalized;
			Vector2 dir2To1 = -dir1To2;

			if (pos1_Real.x < 0.0f || pos1_Real.x > _windowWidth ||
				pos1_Real.y < 0.0f || pos1_Real.y > _windowHeight)
			{
				//2 + dir(2 -> 1) * t = 1'
				//dir * t = 1' - 2
				//t = (1' - 2) / dir

				float tX = 0.0f;
				float tY = 0.0f;
				float tResult = 0.0f;

				bool isClipX = false;
				bool isClipY = false;


				if (posTargetGL.x < 0.0f)
				{
					pos1_Real.x = 0.0f;
					isClipX = true;
				}
				else if (posTargetGL.x > _windowWidth)
				{
					pos1_Real.x = _windowWidth;
					isClipX = true;
				}

				if (posTargetGL.y < 0.0f)
				{
					pos1_Real.y = 0.0f;
					isClipY = true;
				}
				else if (posTargetGL.y > _windowHeight)
				{
					pos1_Real.y = _windowHeight;
					isClipY = true;
				}

				if (isClipX)
				{
					if (Mathf.Abs(dir2To1.x) > 0.0f)
					{ tX = (pos1_Real.x - pos2_Real.x) / dir2To1.x; }
					else
					{ return new Vector2(-100.0f, -100.0f); }//둘다 나갔다...
				}

				if (isClipY)
				{
					if (Mathf.Abs(dir2To1.y) > 0.0f)
					{ tY = (pos1_Real.y - pos2_Real.y) / dir2To1.y; }
					else
					{ return new Vector2(-100.0f, -100.0f); }//둘다 나갔다...
				}
				if (isClipX && isClipY)
				{
					if (Mathf.Abs(tX) < Mathf.Abs(tY))
					{
						tResult = tX;
					}
					else
					{
						tResult = tY;
					}
				}
				else if (isClipX)
				{
					tResult = tX;
				}
				else if (isClipY)
				{
					tResult = tY;
				}

				//2 + dir(2 -> 1) * t = 1'
				pos1_Real = pos2_Real + dir2To1 * tResult;
				return pos1_Real;
			}
			else
			{
				return pos1_Real;
			}
		}


		private static Vector2 GetClippedVertexNoBase(Vector2 posTargetGL)
		{
			Vector2 pos1_Real = posTargetGL;

			if (pos1_Real.x < 0.0f || pos1_Real.x > _windowWidth ||
				pos1_Real.y < 0.0f || pos1_Real.y > _windowHeight)
			{
				if (posTargetGL.x < 0.0f)
				{
					pos1_Real.x = 0.0f;
				}
				else if (posTargetGL.x > _windowWidth)
				{
					pos1_Real.x = _windowWidth;
				}

				if (posTargetGL.y < 0.0f)
				{
					pos1_Real.y = 0.0f;
				}
				else if (posTargetGL.y > _windowHeight)
				{
					pos1_Real.y = _windowHeight;
				}
				return pos1_Real;
			}
			else
			{
				return pos1_Real;
			}
		}

		// 최적화형
		//-------------------------------------------------------------------------------
		//삭제 21.5.18 : 이 함수는 사용하지 않는다. 직접 호출할 것
		public static void BeginBatch_ColoredPolygon()
		{
			_matBatch.BeginPass_Color(GL.TRIANGLES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);

			//GL.Begin(GL.TRIANGLES);
		}

		public static void BeginBatch_ColoredLine()
		{
			//변경 21.5.18
			_matBatch.BeginPass_Color(GL.LINES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);

			//GL.Begin(GL.LINES);
		}

		//public static void EndBatch()
		//{
		//	GL.End();
		//	GL.Flush();
		//}

		//남은 모든 패스를 종료한다.
		public static void EndPass()
		{
			_matBatch.EndPass();
		}

		public static void RefreshScreenSizeToBatch()
		{
			_matBatch.SetClippingSizeToAllMaterial(_glScreenClippingSize);
		}
		//-------------------------------------------------------------------------------
		// Draw Line
		//-------------------------------------------------------------------------------
		//public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color)
		//{
		//	DrawLine(pos1, pos2, color, true);
		//}

		public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1, pos2))
			{ return; }

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			if (isNeedResetMat)
			{
				_matBatch.BeginPass_Color(GL.LINES);

				//삭제 21.5.18
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);
			}

			GL.Color(color);
			GL.Vertex(new Vector3(pos1.x, pos1.y, 0.0f));
			GL.Vertex(new Vector3(pos2.x, pos2.y, 0.0f));

			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}

		public static void DrawLineGL(Vector2 pos1_GL, Vector2 pos2_GL, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1_GL, pos2_GL))
			{ return; }


			if (isNeedResetMat)
			{
				_matBatch.BeginPass_Color(GL.LINES);

				//삭제 21.5.18
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);
			}

			GL.Color(color);
			GL.Vertex(new Vector3(pos1_GL.x, pos1_GL.y, 0.0f));
			GL.Vertex(new Vector3(pos2_GL.x, pos2_GL.y, 0.0f));


			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}




		//추가 : 애니메이션되는 라인
		public static void DrawAnimatedLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1, pos2))
			{ return; }

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);

				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);
			}

			GL.Color(color);

			Vector2 vLine = (pos2 - pos1);
			float remainedLength = vLine.magnitude;
			vLine.Normalize();
			float startOffset = _animationTimeRatio * (ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH);
			Vector2 curPos = pos1 + vLine * startOffset;
			remainedLength -= startOffset;

			if(startOffset - ANIMATED_LINE_SPACE_LENGTH > 0)
			{
				GL.Vertex(new Vector3(pos1.x, pos1.y, 0.0f));
				GL.Vertex(new Vector3(	pos1.x + vLine.x * (startOffset - ANIMATED_LINE_SPACE_LENGTH), 
										pos1.y + vLine.y * (startOffset - ANIMATED_LINE_SPACE_LENGTH), 0.0f));
			}
			
			//움직이는 점선라인을 그리자
			while(true)
			{
				if(remainedLength < 0.0f)
				{
					break;
				}

				GL.Vertex(new Vector3(curPos.x, curPos.y, 0.0f));
				if(remainedLength > ANIMATED_LINE_UNIT_LENGTH)
				{
					GL.Vertex(new Vector3(	curPos.x + vLine.x * ANIMATED_LINE_UNIT_LENGTH, 
											curPos.y + vLine.y * ANIMATED_LINE_UNIT_LENGTH, 
											0.0f));
				}
				else
				{
					GL.Vertex(new Vector3(	curPos.x + vLine.x * remainedLength, 
											curPos.y + vLine.y * remainedLength, 
											0.0f));
					break;
				}
				//이동
				curPos += vLine * (ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH);
				remainedLength -= ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH;
			}

			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}

		public static void DrawAnimatedLineGL(Vector2 pos1_GL, Vector2 pos2_GL, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1_GL, pos2_GL))
			{ return; }


			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);

				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);
			}

			GL.Color(color);

			Vector2 vLine = (pos2_GL - pos1_GL);
			float remainedLength = vLine.magnitude;
			vLine.Normalize();
			float startOffset = _animationTimeRatio * (ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH);
			Vector2 curPos = pos1_GL + vLine * startOffset;
			remainedLength -= startOffset;

			if(startOffset - ANIMATED_LINE_SPACE_LENGTH > 0)
			{
				GL.Vertex(new Vector3(pos1_GL.x, pos1_GL.y, 0.0f));
				GL.Vertex(new Vector3(	pos1_GL.x + vLine.x * (startOffset - ANIMATED_LINE_SPACE_LENGTH), 
										pos1_GL.y + vLine.y * (startOffset - ANIMATED_LINE_SPACE_LENGTH), 0.0f));
			}

			//움직이는 점선라인을 그리자
			while(true)
			{
				if(remainedLength < 0.0f)
				{
					break;
				}

				GL.Vertex(new Vector3(curPos.x, curPos.y, 0.0f));
				if(remainedLength > ANIMATED_LINE_UNIT_LENGTH)
				{
					GL.Vertex(new Vector3(	curPos.x + vLine.x * ANIMATED_LINE_UNIT_LENGTH, 
											curPos.y + vLine.y * ANIMATED_LINE_UNIT_LENGTH, 
											0.0f));
				}
				else
				{
					GL.Vertex(new Vector3(	curPos.x + vLine.x * remainedLength, 
											curPos.y + vLine.y * remainedLength, 
											0.0f));
					break;
				}
				//이동
				curPos += vLine * (ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH);
				remainedLength -= ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH;
			}

			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}




		public static void DrawDotLine(Vector2 pos1, Vector2 pos2, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			if (Vector2.Equals(pos1, pos2))
			{ return; }

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);

				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);
			}

			GL.Color(color);

			Vector2 vLine = (pos2 - pos1);
			float remainedLength = vLine.magnitude;
			vLine.Normalize();
			//float startOffset = _animationTimeRatio * (ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH);
			//Vector2 curPos = pos1 + vLine * startOffset;
			Vector2 curPos = pos1 + vLine;
			//remainedLength -= startOffset;

			//if(startOffset - ANIMATED_LINE_SPACE_LENGTH > 0)
			//{
			//	GL.Vertex(new Vector3(pos1.x, pos1.y, 0.0f));
			//	GL.Vertex(new Vector3(	pos1.x + vLine.x * (startOffset - ANIMATED_LINE_SPACE_LENGTH), 
			//							pos1.y + vLine.y * (startOffset - ANIMATED_LINE_SPACE_LENGTH), 0.0f));
			//}
			
			//움직이는 점선라인을 그리자
			while(true)
			{
				if(remainedLength < 0.0f)
				{
					break;
				}

				GL.Vertex(new Vector3(curPos.x, curPos.y, 0.0f));
				if(remainedLength > ANIMATED_LINE_UNIT_LENGTH)
				{
					GL.Vertex(new Vector3(	curPos.x + vLine.x * ANIMATED_LINE_UNIT_LENGTH, 
											curPos.y + vLine.y * ANIMATED_LINE_UNIT_LENGTH, 
											0.0f));
				}
				else
				{
					GL.Vertex(new Vector3(	curPos.x + vLine.x * remainedLength, 
											curPos.y + vLine.y * remainedLength, 
											0.0f));
					break;
				}
				//이동
				curPos += vLine * (ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH);
				remainedLength -= ANIMATED_LINE_UNIT_LENGTH + ANIMATED_LINE_SPACE_LENGTH;
			}


			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}

		//-------------------------------------------------------------------------------
		// Draw Box
		//-------------------------------------------------------------------------------
		public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isWireframe)
		{
			DrawBox(pos, width, height, color, isWireframe, true);
		}
		public static void DrawBox(Vector2 pos, float width, float height, Color color, bool isWireframe, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			pos = World2GL(pos);

			float halfWidth = width * 0.5f * _zoom;
			float halfHeight = height * 0.5f * _zoom;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - halfWidth, pos.y - halfHeight);
			Vector2 pos_1 = new Vector2(pos.x + halfWidth, pos.y - halfHeight);
			Vector2 pos_2 = new Vector2(pos.x + halfWidth, pos.y + halfHeight);
			Vector2 pos_3 = new Vector2(pos.x - halfWidth, pos.y + halfHeight);

			if (isWireframe)
			{
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.LINES);

					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.LINES);
				}

				GL.Color(color);
				GL.Vertex(pos_0);
				GL.Vertex(pos_1);

				GL.Vertex(pos_1);
				GL.Vertex(pos_2);

				GL.Vertex(pos_2);
				GL.Vertex(pos_3);

				GL.Vertex(pos_3);
				GL.Vertex(pos_0);


				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					//GL.Flush();
					_matBatch.EndPass();
				}
			}
			else
			{
				//CW
				// -------->
				// | 0   1
				// | 		
				// | 3   2
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.TRIANGLES);

					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);
				}
				GL.Color(color);
				GL.Vertex(pos_0); // 0
				GL.Vertex(pos_1); // 1
				GL.Vertex(pos_2); // 2

				GL.Vertex(pos_2); // 2
				GL.Vertex(pos_3); // 3
				GL.Vertex(pos_0); // 0

				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					//GL.Flush();
					_matBatch.EndPass();
				}
			}

			//GL.Flush();
		}


		public static void DrawBoxGL(Vector2 pos, float width, float height, Color color, bool isWireframe, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{ return; }

			float halfWidth = width * 0.5f * _zoom;
			float halfHeight = height * 0.5f * _zoom;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - halfWidth, pos.y - halfHeight);
			Vector2 pos_1 = new Vector2(pos.x + halfWidth, pos.y - halfHeight);
			Vector2 pos_2 = new Vector2(pos.x + halfWidth, pos.y + halfHeight);
			Vector2 pos_3 = new Vector2(pos.x - halfWidth, pos.y + halfHeight);

			if (isWireframe)
			{
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.LINES);

					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.LINES);
				}

				GL.Color(color);
				GL.Vertex(pos_0);
				GL.Vertex(pos_1);
				GL.Vertex(pos_1);
				GL.Vertex(pos_2);
				GL.Vertex(pos_2);
				GL.Vertex(pos_3);
				GL.Vertex(pos_3);
				GL.Vertex(pos_0);

				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					//GL.Flush();
					_matBatch.EndPass();
				}
			}
			else
			{
				//CW
				// -------->
				// | 0   1
				// | 		
				// | 3   2
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.TRIANGLES);

					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);
				}
				GL.Color(color);
				// 0 - 1 - 2
				GL.Vertex(pos_0);
				GL.Vertex(pos_1);
				GL.Vertex(pos_2);

				// 2 - 3 - 0
				GL.Vertex(pos_2);
				GL.Vertex(pos_3);
				GL.Vertex(pos_0);

				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					//GL.Flush();
					_matBatch.EndPass();
				}
			}

			//GL.Flush();
		}



		public static void DrawCircle(Vector2 pos, float radius, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos = World2GL(pos);

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);

				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);
			}

			float radiusGL = radius * _zoom;
			GL.Color(color);
			for (int i = 0; i < 36; i++)
			{
				float angleRad_0 = (i / 36.0f) * Mathf.PI * 2.0f;
				float angleRad_1 = ((i + 1) / 36.0f) * Mathf.PI * 2.0f;

				Vector2 pos0 = pos + new Vector2(Mathf.Cos(angleRad_0) * radiusGL, Mathf.Sin(angleRad_0) * radiusGL);
				Vector2 pos1 = pos + new Vector2(Mathf.Cos(angleRad_1) * radiusGL, Mathf.Sin(angleRad_1) * radiusGL);

				GL.Vertex(pos0);
				GL.Vertex(pos1);
			}


			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}


			//GL.Flush();
		}



		public static void DrawBoldCircleGL(Vector2 posGL, float radius, float lineWidth, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.TRIANGLES);

				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);
			}

			float radiusGL = radius * _zoom;
			//float radiusGL = radius;
			GL.Color(color);
			for (int i = 0; i < 36; i++)
			{
				float angleRad_0 = (i / 36.0f) * Mathf.PI * 2.0f;
				float angleRad_1 = ((i + 1) / 36.0f) * Mathf.PI * 2.0f;

				Vector2 pos0 = posGL + new Vector2(Mathf.Cos(angleRad_0) * radiusGL, Mathf.Sin(angleRad_0) * radiusGL);
				Vector2 pos1 = posGL + new Vector2(Mathf.Cos(angleRad_1) * radiusGL, Mathf.Sin(angleRad_1) * radiusGL);

				//GL.Vertex(pos0);
				//GL.Vertex(pos1);

				DrawBoldLineGL(pos0, pos1, lineWidth, color, false);
			}

			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}


			//GL.Flush();
		}



		public static void DrawBoldLine(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos1 = World2GL(pos1);
			pos2 = World2GL(pos2);

			if (pos1 == pos2)
			{
				return;
			}

			//float halfWidth = width * 0.5f / _zoom;
			float halfWidth = width * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			// -------->
			// |    1
			// | 0     2
			// | 
			// | 
			// | 
			// | 5     3
			// |    4

			Vector2 dir = (pos1 - pos2).normalized;
			Vector2 dirRev = new Vector2(-dir.y, dir.x);

			Vector2 pos_0 = pos1 - dirRev * halfWidth;
			Vector2 pos_1 = pos1 + dir * halfWidth;
			//Vector2 pos_1 = pos1;
			Vector2 pos_2 = pos1 + dirRev * halfWidth;

			Vector2 pos_3 = pos2 + dirRev * halfWidth;
			Vector2 pos_4 = pos2 - dir * halfWidth;
			//Vector2 pos_4 = pos2;
			Vector2 pos_5 = pos2 - dirRev * halfWidth;

			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.TRIANGLES);

				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color);
			// 0 - 1 - 2
			GL.Vertex(pos_0);	GL.Vertex(pos_1);	GL.Vertex(pos_2);
			GL.Vertex(pos_2);	GL.Vertex(pos_1);	GL.Vertex(pos_0);

			// 0 - 2 - 3
			GL.Vertex(pos_0);	GL.Vertex(pos_2);	GL.Vertex(pos_3);
			GL.Vertex(pos_3);	GL.Vertex(pos_2);	GL.Vertex(pos_0);

			// 3 - 5 - 0
			GL.Vertex(pos_3);	GL.Vertex(pos_5);	GL.Vertex(pos_0);
			GL.Vertex(pos_0);	GL.Vertex(pos_5);	GL.Vertex(pos_3);

			// 3 - 4 - 5
			GL.Vertex(pos_3);	GL.Vertex(pos_4);	GL.Vertex(pos_5);
			GL.Vertex(pos_5);	GL.Vertex(pos_4);	GL.Vertex(pos_3);

			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				_matBatch.EndPass();
			}
		}


		public static void DrawBoldLineGL(Vector2 pos1, Vector2 pos2, float width, Color color, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			if (pos1 == pos2)
			{ return; }

			//float halfWidth = width * 0.5f / _zoom;
			float halfWidth = width * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)

			// -------->
			// |    1
			// | 0     2
			// | 
			// | 
			// | 
			// | 5     3
			// |    4

			Vector2 dir = (pos1 - pos2).normalized;
			Vector2 dirRev = new Vector2(-dir.y, dir.x);

			Vector2 pos_0 = pos1 - dirRev * halfWidth;
			Vector2 pos_1 = pos1 + dir * halfWidth;
			//Vector2 pos_1 = pos1;
			Vector2 pos_2 = pos1 + dirRev * halfWidth;

			Vector2 pos_3 = pos2 + dirRev * halfWidth;
			Vector2 pos_4 = pos2 - dir * halfWidth;
			//Vector2 pos_4 = pos2;
			Vector2 pos_5 = pos2 - dirRev * halfWidth;

			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.TRIANGLES);

				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);
			}
			GL.Color(color);
			// 0 - 1 - 2
			GL.Vertex(pos_0);	GL.Vertex(pos_1);	GL.Vertex(pos_2);
			GL.Vertex(pos_2);	GL.Vertex(pos_1);	GL.Vertex(pos_0);

			// 0 - 2 - 3
			GL.Vertex(pos_0);	GL.Vertex(pos_2);	GL.Vertex(pos_3);
			GL.Vertex(pos_3);	GL.Vertex(pos_2);	GL.Vertex(pos_0);

			// 3 - 5 - 0
			GL.Vertex(pos_3);	GL.Vertex(pos_5);	GL.Vertex(pos_0);
			GL.Vertex(pos_0);	GL.Vertex(pos_5);	GL.Vertex(pos_3);

			// 3 - 4 - 5
			GL.Vertex(pos_3);	GL.Vertex(pos_4);	GL.Vertex(pos_5);
			GL.Vertex(pos_5);	GL.Vertex(pos_4);	GL.Vertex(pos_3);

			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				EndPass();
			}
		}
		//-------------------------------------------------------------------------------
		// Draw Text
		//-------------------------------------------------------------------------------
		public static void DrawText(string text, Vector2 pos, float width, Color color)
		{
			//if(_mat_Color == null || _mat_Texture == null)
			//{
			//	return;
			//}
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos = World2GL(pos);

			if (IsVertexClipped(pos))
			{
				return;
			}

			if (IsVertexClipped(pos + new Vector2(width * _zoom, 15)))
			{
				return;
			}
			_textStyle.normal.textColor = color;


			GUI.Label(new Rect(pos.x, pos.y, 100.0f, 30.0f), text, _textStyle);
		}


		public static void DrawTextGL(string text, Vector2 pos, float width, Color color)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			if (IsVertexClipped(pos))
			{
				return;
			}

			if (IsVertexClipped(pos + new Vector2(width, 15)))
			{
				return;
			}
			_textStyle.normal.textColor = color;


			GUI.Label(new Rect(pos.x, pos.y, width + 50, 30.0f), text, _textStyle);
		}

		public static void DrawTextGL_IgnoreRightClipping(string text, Vector2 pos, float width, Color color)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			if (IsVertexClipped(pos))
			{
				return;
			}

			//if (IsVertexClipped(pos + new Vector2(width, 15)))
			//{
			//	return;
			//}
			_textStyle.normal.textColor = color;

			GUI.Label(new Rect(pos.x, pos.y, width + 50, 30.0f), text, _textStyle);
		}


		//-------------------------------------------------------------------------------
		// Draw Texture
		//-------------------------------------------------------------------------------
		public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X)
		{
			DrawTexture(image, pos, width, height, color2X, 0.0f);
		}

		public static void DrawTexture(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			pos = World2GL(pos);


			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);


			float widthResize = (pos_1.x - pos_0.x);
			float heightResize = (pos_3.y - pos_0.y);

			if (widthResize < 1.0f || heightResize < 1.0f)
			{
				return;
			}


			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			//변경 21.5.18
			_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);

			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);

			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0

			//삭제 21.5.18
			//GL.End();//<전환완료>
			//GL.Flush();
			EndPass();
		}






		public static void DrawTexture(Texture2D image, apMatrix3x3 matrix, float width, float height, Color color2X, float depth)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float width_Half = width * 0.5f;
			float height_Half = height * 0.5f;

			//Zero 대신 mesh Pivot 위치로 삼자
			Vector2 pos_0 = World2GL(matrix.MultiplyPoint(new Vector2(-width_Half, +height_Half)));
			Vector2 pos_1 = World2GL(matrix.MultiplyPoint(new Vector2(+width_Half, +height_Half)));
			Vector2 pos_2 = World2GL(matrix.MultiplyPoint(new Vector2(+width_Half, -height_Half)));
			Vector2 pos_3 = World2GL(matrix.MultiplyPoint(new Vector2(-width_Half, -height_Half)));

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			//변경 21.5.18
			_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);

			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);

			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0


			//삭제 21.5.18
			//GL.End();//<전환 완료>
			//GL.Flush();
			EndPass();
		}

		public static void DrawTextureGL(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);


			float widthResize = (pos_1.x - pos_0.x);
			float heightResize = (pos_3.y - pos_0.y);

			if (widthResize < 1.0f || heightResize < 1.0f)
			{
				return;
			}


			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			//변경 21.5.18
			_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
			
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);


			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0


			//삭제 21.5.18
			//GL.End();//<전환 완료>
			//GL.Flush();
			EndPass();
		}



		public static void DrawTextureGL(Texture2D image, Vector2 pos, float width, float height, Color color2X, float depth, bool isNeedResetMat)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);


			float widthResize = (pos_1.x - pos_0.x);
			float heightResize = (pos_3.y - pos_0.y);

			if (widthResize < 1.0f || heightResize < 1.0f)
			{
				return;
			}


			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, image, apPortrait.SHADER_TYPE.AlphaBlend);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);
			}

			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0

			//삭제 21.5.18
			if (isNeedResetMat)
			{
				//GL.End();//<전환 완료>
				//GL.Flush();
				EndPass();
			}


			//GL.Flush();
		}


		/// <summary>
		/// 이미 VColor Texture Pass가 시작될 때 사용하는 텍스쳐
		/// </summary>
		/// <param name="image"></param>
		/// <param name="pos"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="VColor1x"></param>
		/// <param name="depth"></param>
		public static void DrawTextureGLWithVColor(Texture2D image, Vector2 pos, float width, float height, Color VColor1x, float depth)
		{
			if (_matBatch.IsNotReady())
			{
				return;
			}

			float realWidth = width * _zoom;
			float realHeight = height * _zoom;

			float realWidth_Half = realWidth * 0.5f;
			float realHeight_Half = realHeight * 0.5f;

			//CW
			// -------->
			// | 0(--) 1
			// | 		
			// | 3   2 (++)
			Vector2 pos_0 = new Vector2(pos.x - realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_1 = new Vector2(pos.x + realWidth_Half, pos.y - realHeight_Half);
			Vector2 pos_2 = new Vector2(pos.x + realWidth_Half, pos.y + realHeight_Half);
			Vector2 pos_3 = new Vector2(pos.x - realWidth_Half, pos.y + realHeight_Half);


			float widthResize = (pos_1.x - pos_0.x);
			float heightResize = (pos_3.y - pos_0.y);

			if (widthResize < 1.0f || heightResize < 1.0f)
			{
				return;
			}

			float u_left = 0.0f;
			float u_right = 1.0f;

			float v_top = 0.0f;
			float v_bottom = 1.0f;

			Vector3 uv_0 = new Vector3(u_left, v_bottom, 0.0f);
			Vector3 uv_1 = new Vector3(u_right, v_bottom, 0.0f);
			Vector3 uv_2 = new Vector3(u_right, v_top, 0.0f);
			Vector3 uv_3 = new Vector3(u_left, v_top, 0.0f);

			//CW
			// -------->
			// | 0   1
			// | 		
			// | 3   2
			GL.Color(VColor1x);
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos_1.x, pos_1.y, depth)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos_2.x, pos_2.y, depth)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos_3.x, pos_3.y, depth)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos_0.x, pos_0.y, depth)); // 0
		}

		/// <summary>
		/// 이미 VColor Texture Pass가 시작될 때 사용하는 Box 그리기 함수
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="vColor"></param>
		private static void DrawBoxWithVColorAndUV(Vector2 pos, float width, float height, Color vColor)
		{
			float width_Half = width * 0.5f;
			float height_Half = height * 0.5f;

			Vector3 uv_0 = new Vector3(0, 1, 0.0f);
			Vector3 uv_1 = new Vector3(1, 1, 0.0f);
			Vector3 uv_2 = new Vector3(1, 0, 0.0f);
			Vector3 uv_3 = new Vector3(0, 0, 0.0f);

			GL.Color(vColor);

			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos.x - width_Half, pos.y - height_Half, 0)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos.x + width_Half, pos.y - height_Half, 0)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos.x + width_Half, pos.y + height_Half, 0)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos.x + width_Half, pos.y + height_Half, 0)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos.x - width_Half, pos.y + height_Half, 0)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos.x - width_Half, pos.y - height_Half, 0)); // 0
		}

		private static void DrawBoxWithVColorAndUV(Vector2 pos, float width, float height, Color vColor, Vector2 uvOffset)
		{
			float width_Half = width * 0.5f;
			float height_Half = height * 0.5f;

			Vector3 uv_0 = new Vector3(0 + uvOffset.x, 1 + uvOffset.y, 0.0f);
			Vector3 uv_1 = new Vector3(1 + uvOffset.x, 1 + uvOffset.y, 0.0f);
			Vector3 uv_2 = new Vector3(1 + uvOffset.x, 0 + uvOffset.y, 0.0f);
			Vector3 uv_3 = new Vector3(0 + uvOffset.x, 0 + uvOffset.y, 0.0f);

			GL.Color(vColor);

			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos.x - width_Half, pos.y - height_Half, 0)); // 0
			GL.TexCoord(uv_1);	GL.Vertex(new Vector3(pos.x + width_Half, pos.y - height_Half, 0)); // 1
			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos.x + width_Half, pos.y + height_Half, 0)); // 2

			GL.TexCoord(uv_2);	GL.Vertex(new Vector3(pos.x + width_Half, pos.y + height_Half, 0)); // 2
			GL.TexCoord(uv_3);	GL.Vertex(new Vector3(pos.x - width_Half, pos.y + height_Half, 0)); // 3
			GL.TexCoord(uv_0);	GL.Vertex(new Vector3(pos.x - width_Half, pos.y - height_Half, 0)); // 0
		}


		//-------------------------------------------------------------------------------
		// Draw Vertices / Pins
		//-------------------------------------------------------------------------------
		private static Vector2 s_VertUV_LB = new Vector2(0.0f, 0.0f);
		private static Vector2 s_VertUV_RB = new Vector2(0.5f, 0.0f);
		private static Vector2 s_VertUV_LT = new Vector2(0.0f, 0.5f);
		private static Vector2 s_VertUV_RT = new Vector2(0.5f, 0.5f);

		private static Vector2 s_VertWhiteOutlineUV_LB = new Vector2(0.0f, 0.5f);
		private static Vector2 s_VertWhiteOutlineUV_RB = new Vector2(0.5f, 0.5f);
		private static Vector2 s_VertWhiteOutlineUV_LT = new Vector2(0.0f, 1.0f);
		private static Vector2 s_VertWhiteOutlineUV_RT = new Vector2(0.5f, 1.0f);

		private static Vector2 s_PinUV_LB = new Vector2(0.5f, 0.0f);
		private static Vector2 s_PinUV_RB = new Vector2(1.0f, 0.0f);
		private static Vector2 s_PinUV_LT = new Vector2(0.5f, 0.5f);
		private static Vector2 s_PinUV_RT = new Vector2(1.0f, 0.5f);

		public static void DrawVertex(ref Vector2 posGL, float halfSize, ref Color vColor)
		{
			//UV는 (0~0.5, 0.0~0.5)
			GL.Color(vColor);

			//2  -  3
			//0  -  1

			GL.TexCoord(s_VertUV_LB);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_VertUV_RB);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_VertUV_LT);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y + halfSize, 0));

			GL.TexCoord(s_VertUV_RB);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_VertUV_RT);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y + halfSize, 0));
			GL.TexCoord(s_VertUV_LT);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y + halfSize, 0));
		}

		public static void DrawVertex_WhiteOutline(ref Vector2 posGL, float halfSize, ref Color vColor)
		{
			//UV는 (0~0.5, 0.0~0.5)
			GL.Color(vColor);

			//2  -  3
			//0  -  1

			GL.TexCoord(s_VertWhiteOutlineUV_LB);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_VertWhiteOutlineUV_RB);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_VertWhiteOutlineUV_LT);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y + halfSize, 0));

			GL.TexCoord(s_VertWhiteOutlineUV_RB);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_VertWhiteOutlineUV_RT);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y + halfSize, 0));
			GL.TexCoord(s_VertWhiteOutlineUV_LT);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y + halfSize, 0));
		}

		public static void DrawPin(ref Vector2 posGL, float halfSize, ref Color vColor)
		{
			//UV는 (0.5~1.0, 0.0~0.5)
			GL.Color(vColor);

			//2  -  3
			//0  -  1

			GL.TexCoord(s_PinUV_LB);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_PinUV_RB);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_PinUV_LT);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y + halfSize, 0));

			GL.TexCoord(s_PinUV_RB);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y - halfSize, 0));
			GL.TexCoord(s_PinUV_RT);	GL.Vertex(new Vector3(posGL.x + halfSize, posGL.y + halfSize, 0));
			GL.TexCoord(s_PinUV_LT);	GL.Vertex(new Vector3(posGL.x - halfSize, posGL.y + halfSize, 0));
		}



		//-------------------------------------------------------------------------------
		// Draw Mesh
		//-------------------------------------------------------------------------------
		public static void DrawMesh(	apMesh mesh, 
										apMatrix3x3 matrix, 
										Color color2X, 
										
										//RENDER_TYPE renderType, 이전
										RenderTypeRequest renderRequest,//변경 22.3.3

										apVertexController vertexController, 
										apEditor editor, 
										Vector2 mousePosition,
										bool isPSDAreaEditing)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)//이전 코드
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)//변경 코드
				{
					DrawBox(Vector2.zero, 512, 512, Color.red, true);
					DrawText("No Image", Vector2.zero, 80, Color.cyan);
					return;
				}



				//1. 모든 메시를 보여줄때 (또는 클리핑된 메시가 없을 때) => 
				bool isShowAllTexture = false;
				Color textureColor = _textureColor_Gray;
				if (
					//(renderType & RENDER_TYPE.ShadeAllMesh) != 0	//이전
					renderRequest.ShadeAllMesh						//변경 22.3.3 (v1.4.0)
					|| mesh._indexBuffer.Count < 3)
				{
					isShowAllTexture = true;
					textureColor = _textureColor_Shade;
				}
				else if (
					//(renderType & RENDER_TYPE.AllMesh) != 0	//이전
					renderRequest.AllMesh						//변경 22.3.3
					)
				{
					isShowAllTexture = true;
				}

				matrix *= mesh.Matrix_VertToLocal;

				if (isShowAllTexture)
				{
					//DrawTexture(mesh._textureData._image, matrix, mesh._textureData._width, mesh._textureData._height, textureColor, -10);
					DrawTexture(mesh.LinkedTextureData._image, matrix, mesh.LinkedTextureData._width, mesh.LinkedTextureData._height, textureColor, -10);
				}

				apVertex selectedVertex = null;
				List<apVertex> selectedVertices = null;
				apVertex nextSelectedVertex = null;
				apBone selectedBone = null;
				apMeshPolygon selectedPolygon = null;

				//핀
				apMeshPin selectedPin = editor.Select.MeshPin;
				List<apMeshPin> selectedPins = editor.Select.MeshPins;


				//메시의 버텍스/인덱스 리스트
				List<apVertex> meshVerts = mesh._vertexData;
				int nVerts = meshVerts != null ? meshVerts.Count : 0;

				List<int> meshIndexBuffers = mesh._indexBuffer;
				int nIndexBuffers = meshIndexBuffers != null ? meshIndexBuffers.Count : 0;

				List<apMeshEdge> meshEdges = mesh._edges;
				int nEdges = meshEdges != null ? meshEdges.Count : 0;

				List<apMeshPolygon> meshPolygons = mesh._polygons;
				int nPolygons = meshPolygons != null ? meshPolygons.Count : 0;
				


				if (vertexController != null)
				{
					selectedVertex = vertexController.Vertex;
					selectedVertices = vertexController.Vertices;
					nextSelectedVertex = vertexController.LinkedNextVertex;
					selectedBone = vertexController.Bone;
					selectedPolygon = vertexController.Polygon;
				}

				Vector2 pos2_0 = Vector2.zero;
				Vector2 pos2_1 = Vector2.zero;
				Vector2 pos2_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;

				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;

				//2. 메시를 렌더링하자
				if (nIndexBuffers >= 3)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					// <참고> Weight를 출력하고 싶다면 Normal 대신 VColor를 넣고, VertexColor를 넣어주자
					
					//if ((renderType & RENDER_TYPE.VolumeWeightColor) != 0)	//이전
					if (renderRequest.VolumeWeightColor)						//변경 22.3.3 (v1.4.0)
					{
						_matBatch.BeginPass_Texture_VColor(GL.TRIANGLES, _textureColor_Gray, mesh.LinkedTextureData._image, 1.0f, apPortrait.SHADER_TYPE.AlphaBlend, false, Vector4.zero);
					}
					else
					{
						_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, color2X, mesh.LinkedTextureData._image, apPortrait.SHADER_TYPE.AlphaBlend);

					}
					
					//삭제
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);


					//------------------------------------------
					apVertex vert0, vert1, vert2;
					
					int iVert_0 = 0;
					int iVert_1 = 0;
					int iVert_2 = 0;

					
					if (renderRequest.TestPinWeight)
					{
						//[핀 테스트모드]에서의 메시 렌더링
						
						//버텍스 색상이 white로 공통
						GL.Color(Color.white);

						for (int i = 0; i < nIndexBuffers; i += 3)
						{
							if (i + 2 >= nIndexBuffers) { break; }

							iVert_0 = meshIndexBuffers[i + 0];
							iVert_1 = meshIndexBuffers[i + 1];
							iVert_2 = meshIndexBuffers[i + 2];

							if (iVert_0 >= nVerts ||
								iVert_1 >= nVerts ||
								iVert_2 >= nVerts)
							{
								break;
							}

							vert0 = meshVerts[iVert_0];
							vert1 = meshVerts[iVert_1];
							vert2 = meshVerts[iVert_2];

						
							pos2_0 = World2GL(matrix.MultiplyPoint(vert0._pos_PinTest));
							pos2_1 = World2GL(matrix.MultiplyPoint(vert1._pos_PinTest));
							pos2_2 = World2GL(matrix.MultiplyPoint(vert2._pos_PinTest));

							pos_0 = new Vector3(pos2_0.x, pos2_0.y, vert0._zDepth * 0.1f);
							pos_1 = new Vector3(pos2_1.x, pos2_1.y, vert1._zDepth * 0.5f);
							pos_2 = new Vector3(pos2_2.x, pos2_2.y, vert2._zDepth * 0.5f);//<<Z값이 반영되었다.

							uv_0 = vert0._uv;
							uv_1 = vert1._uv;
							uv_2 = vert2._uv;

							GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
							GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
							GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

							//Back Side
							GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
							GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
							GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						}
					}
					else if (renderRequest.VolumeWeightColor)
					{
						//[Depth Weight 렌더링]에서의 메시 렌더링

						Color color0 = Color.white;
						Color color1 = Color.white;
						Color color2 = Color.white;

						for (int i = 0; i < nIndexBuffers; i += 3)
						{
							if (i + 2 >= nIndexBuffers) { break; }

							iVert_0 = meshIndexBuffers[i + 0];
							iVert_1 = meshIndexBuffers[i + 1];
							iVert_2 = meshIndexBuffers[i + 2];

							if (iVert_0 >= nVerts ||
								iVert_1 >= nVerts ||
								iVert_2 >= nVerts)
							{
								break;
							}

							vert0 = meshVerts[iVert_0];
							vert1 = meshVerts[iVert_1];
							vert2 = meshVerts[iVert_2];

						
							pos2_0 = World2GL(matrix.MultiplyPoint(vert0._pos));
							pos2_1 = World2GL(matrix.MultiplyPoint(vert1._pos));
							pos2_2 = World2GL(matrix.MultiplyPoint(vert2._pos));

							pos_0 = new Vector3(pos2_0.x, pos2_0.y, vert0._zDepth * 0.1f);
							pos_1 = new Vector3(pos2_1.x, pos2_1.y, vert1._zDepth * 0.5f);
							pos_2 = new Vector3(pos2_2.x, pos2_2.y, vert2._zDepth * 0.5f);//<<Z값이 반영되었다.

							uv_0 = vert0._uv;
							uv_1 = vert1._uv;
							uv_2 = vert2._uv;

							//VolumeWeightColor
							color0 = GetWeightGrayscale(vert0._zDepth);
							color1 = GetWeightGrayscale(vert1._zDepth);
							color2 = GetWeightGrayscale(vert2._zDepth);

							////------------------------------------------

							GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
							GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
							GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

							//Back Side
							GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
							GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
							GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

							////------------------------------------------
						}
					}
					else
					{
						//[일반 모드]에서의 메시 렌더링

						//버텍스 색상이 white로 공통
						GL.Color(Color.white);

						for (int i = 0; i < nIndexBuffers; i += 3)
						{
							if (i + 2 >= nIndexBuffers) { break; }

							iVert_0 = meshIndexBuffers[i + 0];
							iVert_1 = meshIndexBuffers[i + 1];
							iVert_2 = meshIndexBuffers[i + 2];

							if (iVert_0 >= nVerts ||
								iVert_1 >= nVerts ||
								iVert_2 >= nVerts)
							{
								break;
							}

							vert0 = meshVerts[iVert_0];
							vert1 = meshVerts[iVert_1];
							vert2 = meshVerts[iVert_2];

						
							pos2_0 = World2GL(matrix.MultiplyPoint(vert0._pos));
							pos2_1 = World2GL(matrix.MultiplyPoint(vert1._pos));
							pos2_2 = World2GL(matrix.MultiplyPoint(vert2._pos));

							pos_0 = new Vector3(pos2_0.x, pos2_0.y, vert0._zDepth * 0.1f);
							pos_1 = new Vector3(pos2_1.x, pos2_1.y, vert1._zDepth * 0.5f);
							pos_2 = new Vector3(pos2_2.x, pos2_2.y, vert2._zDepth * 0.5f);//<<Z값이 반영되었다.

							uv_0 = vert0._uv;
							uv_1 = vert1._uv;
							uv_2 = vert2._uv;

							GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
							GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
							GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

							//Back Side
							GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
							GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
							GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

							////------------------------------------------
						}
					}
					

					//삭제 21.5.18
					//GL.End();//<전환 완료>
					//GL.Flush();
					EndPass();

				}

				//미러 모드
				//--------------------------------------------------------------
				if(editor._meshEditMode == apEditor.MESH_EDIT_MODE.MakeMesh &&
					editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
				{
					DrawMeshMirror(mesh);
				}


				// Atlas 외곽선
				//--------------------------------------------------------------
				if (mesh._isPSDParsed)
				{
					Vector2 pos_LT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_LT.y));
					Vector2 pos_RT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_LT.y));
					Vector2 pos_LB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_RB.y));
					Vector2 pos_RB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_RB.y));

					_matBatch.BeginPass_Color(GL.LINES);
					
					if(!isPSDAreaEditing)
					{
						DrawLine(pos_LT, pos_RT, editor._colorOption_AtlasBorder, false);
						DrawLine(pos_RT, pos_RB, editor._colorOption_AtlasBorder, false);
						DrawLine(pos_RB, pos_LB, editor._colorOption_AtlasBorder, false);
						DrawLine(pos_LB, pos_LT, editor._colorOption_AtlasBorder, false);
					}
					else
					{
						DrawAnimatedLine(pos_LT, pos_RT, editor._colorOption_AtlasBorder, false);
						DrawAnimatedLine(pos_RT, pos_RB, editor._colorOption_AtlasBorder, false);
						DrawAnimatedLine(pos_RB, pos_LB, editor._colorOption_AtlasBorder, false);
						DrawAnimatedLine(pos_LB, pos_LT, editor._colorOption_AtlasBorder, false);
					}
					
					//삭제 21.5.18
					//GL.End();//<전환 완료>
					//GL.Flush();
					
				}

				//외곽선을 그려주자
				//float imageWidthHalf = mesh._textureData._width * 0.5f;
				//float imageHeightHalf = mesh._textureData._height * 0.5f;

				float imageWidthHalf = mesh.LinkedTextureData._width * 0.5f;
				float imageHeightHalf = mesh.LinkedTextureData._height * 0.5f;

				Vector2 pos_TexOutline_LT = matrix.MultiplyPoint(new Vector2(-imageWidthHalf, -imageHeightHalf));
				Vector2 pos_TexOutline_RT = matrix.MultiplyPoint(new Vector2(imageWidthHalf, -imageHeightHalf));
				Vector2 pos_TexOutline_LB = matrix.MultiplyPoint(new Vector2(-imageWidthHalf, imageHeightHalf));
				Vector2 pos_TexOutline_RB = matrix.MultiplyPoint(new Vector2(imageWidthHalf, imageHeightHalf));

				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);



				DrawLine(pos_TexOutline_LT, pos_TexOutline_RT, editor._colorOption_AtlasBorder, false);
				DrawLine(pos_TexOutline_RT, pos_TexOutline_RB, editor._colorOption_AtlasBorder, false);
				DrawLine(pos_TexOutline_RB, pos_TexOutline_LB, editor._colorOption_AtlasBorder, false);
				DrawLine(pos_TexOutline_LB, pos_TexOutline_LT, editor._colorOption_AtlasBorder, false);

				//삭제 21.5.18
				//GL.End();//<전환 완료>
				//GL.Flush();
				EndPass();

				
				//3. Edge를 렌더링하자 (전체 / Ouline)
				if (nEdges > 0
					&& (renderRequest.AllEdges || renderRequest.TransparentEdges)										//변경 22.3.3 (v1.4.0)
					)
				{
					Color edgeColor = editor._colorOption_MeshEdge;
					
					//if((renderType & RENDER_TYPE.TransparentEdges) != 0)	//이전
					if(renderRequest.TransparentEdges)						//변경 22.3.3
					{
						edgeColor.a *= 0.5f;//반투명인 경우
					}
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;

					apMeshEdge curEdge = null;
					apMeshPolygon curPolygon = null;

					List<apMeshEdge> curHiddenEdges = null;
					int nCurHiddenEdges = 0;

					apMeshEdge curHiddenEdge = null;

					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.LINES);
						
					if (renderRequest.TestPinWeight)
					{
						//[핀 테스트 모드]에서의 렌더링
						for (int i = 0; i < nEdges; i++)
						{
							curEdge = mesh._edges[i];

							pos0 = matrix.MultiplyPoint(curEdge._vert1._pos_PinTest);
							pos1 = matrix.MultiplyPoint(curEdge._vert2._pos_PinTest);

							DrawLine(pos0, pos1, edgeColor, false);
						}

						if (renderRequest.AllEdges && nPolygons > 0) //변경 22.3.3
						{
							for (int iPoly = 0; iPoly < nPolygons; iPoly++)
							{
								curPolygon = mesh._polygons[iPoly];
								curHiddenEdges = curPolygon._hidddenEdges;
								nCurHiddenEdges = curHiddenEdges != null ? curHiddenEdges.Count : 0;

								if (nCurHiddenEdges > 0)
								{
									for (int iHE = 0; iHE < nCurHiddenEdges; iHE++)
									{
										curHiddenEdge = curHiddenEdges[iHE];

										pos0 = matrix.MultiplyPoint(curHiddenEdge._vert1._pos_PinTest);
										pos1 = matrix.MultiplyPoint(curHiddenEdge._vert2._pos_PinTest);

										DrawLine(pos0, pos1, editor._colorOption_MeshHiddenEdge, false);
									}
								}
							}
						}
					}
					else
					{
						//[일반 모드]에서의 렌더링
						for (int i = 0; i < nEdges; i++)
						{
							curEdge = mesh._edges[i];

							pos0 = matrix.MultiplyPoint(curEdge._vert1._pos);
							pos1 = matrix.MultiplyPoint(curEdge._vert2._pos);

							DrawLine(pos0, pos1, edgeColor, false);
						}

						if (renderRequest.AllEdges && nPolygons > 0)                     //변경 22.3.3
						{
							for (int iPoly = 0; iPoly < nPolygons; iPoly++)
							{
								curPolygon = mesh._polygons[iPoly];
								curHiddenEdges = curPolygon._hidddenEdges;
								nCurHiddenEdges = curHiddenEdges != null ? curHiddenEdges.Count : 0;

								if (nCurHiddenEdges > 0)
								{
									for (int iHE = 0; iHE < nCurHiddenEdges; iHE++)
									{
										curHiddenEdge = curHiddenEdges[iHE];

										pos0 = matrix.MultiplyPoint(curHiddenEdge._vert1._pos);
										pos1 = matrix.MultiplyPoint(curHiddenEdge._vert2._pos);

										DrawLine(pos0, pos1, editor._colorOption_MeshHiddenEdge, false);
									}
								}
							}
						}
					}
				}
				
				//if ((renderType & RENDER_TYPE.Outlines) != 0)		//이전
				if (renderRequest.Outlines && nEdges > 0)							//변경 22.3.3
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					
					_matBatch.BeginPass_Color(GL.TRIANGLES);
						
					apMeshEdge curEdge = null;

					for (int i = 0; i < nEdges; i++)
					{
						curEdge = mesh._edges[i];
							
						if (!curEdge._isOutline)
						{
							continue;
						}

						pos0 = matrix.MultiplyPoint(curEdge._vert1._pos);
						pos1 = matrix.MultiplyPoint(curEdge._vert2._pos);

						DrawBoldLine(pos0, pos1, 6.0f, editor._colorOption_Outline, false);
					}
				}

				//if ((renderType & RENDER_TYPE.PolygonOutline) != 0)	//이전
				if (renderRequest.PolygonOutline
					&& selectedPolygon != null
					&& nPolygons > 0)						//변경 22.3.3
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;

					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.TRIANGLES);
					
					List<apMeshEdge> selectedPolyEdges = selectedPolygon._edges;
					int nSelectedPolyEdges = selectedPolyEdges != null ? selectedPolyEdges.Count : 0;

					if (nSelectedPolyEdges > 0)
					{
						apMeshEdge curSelectedPolyEdge = null;

						for (int i = 0; i < nSelectedPolyEdges; i++)
						{
							curSelectedPolyEdge = selectedPolygon._edges[i];
							pos0 = matrix.MultiplyPoint(curSelectedPolyEdge._vert1._pos);
							pos1 = matrix.MultiplyPoint(curSelectedPolyEdge._vert2._pos);

							DrawBoldLine(pos0, pos1, 6.0f, editor._colorOption_Outline, false);
						}
					}
					
				}

				//3. 버텍스를 렌더링하자
				//if ((renderType & RENDER_TYPE.Vertex) != 0)	//이전
				if (renderRequest.Vertex != RenderTypeRequest.VISIBILITY.Hidden && nVerts > 0) //변경 22.3.3
				{
					//변경 22.4.12 [v1.4.0]
					_matBatch.BeginPass_VertexAndPin(GL.TRIANGLES);


					//이전 : 텍스쳐 없는 World 기준 사이즈
					//float pointSize = 10.0f / _zoom;

					//변경 22.4.12 : 텍스쳐로 그려지는 GL 기준 사이즈
					//float halfPointSize = VERTEX_RENDER_SIZE * 0.5f; // 삭제 v1.4.2 : 옵션에 따른 변수값을 바로 사용
					

					Vector2 posGL = Vector2.zero;

					//버텍스 투명도 설정
					float vertAlphaRatio = renderRequest.Vertex == RenderTypeRequest.VISIBILITY.Transparent ? 0.5f : 1.0f;//메시는 메시 그룹때보단 버텍스 반투명도가 조금 높다. Weight를 보기 위함

					Color vColor = Color.black;
					apVertex curVert = null;

					if (renderRequest.TestPinWeight)
					{
						//Pin 편집 모드일때 - 버텍스 Test 위치 + Ratio에 따른 가중치 색상 출력
						for (int i = 0; i < nVerts; i++)
						{
							curVert = meshVerts[i];

							vColor = GetWeightColor4_Vert(curVert._pinWeightRatio);
							vColor.a *= vertAlphaRatio;

							posGL = World2GL(matrix.MultiplyPoint(curVert._pos_PinTest));
							
							DrawVertex(ref posGL, _vertexRenderSizeHalf, ref vColor);
						}
					}
					else if(renderRequest.PinVertWeight)
					{
						//Pin 편집 모드 중 테스트 모드가 아닌 그 외의 모드일 때 : 버텍스 기본 위치 + 가중치 표시
						
						for (int i = 0; i < nVerts; i++)
						{
							curVert = meshVerts[i];
							
							vColor = GetWeightColor4_Vert(curVert._pinWeightRatio);
							vColor.a *= vertAlphaRatio;

							posGL = World2GL(matrix.MultiplyPoint(curVert._pos));
							
							DrawVertex(ref posGL, _vertexRenderSizeHalf, ref vColor);
						}
					}
					else
					{
						//나머지 일반 모드일때 - 버텍스 기본 위치 출력
						for (int i = 0; i < nVerts; i++)
						{
							curVert = meshVerts[i];
							
							vColor = editor._colorOption_VertColor_NotSelected;

							if (curVert == selectedVertex)
							{
								vColor = editor._colorOption_VertColor_Selected;
							}
							else if (curVert == nextSelectedVertex)
							{
								vColor = _vertColor_NextSelected;
							}
							else if (selectedVertices != null)
							{
								if (selectedVertices.Contains(curVert))
								{
									vColor = editor._colorOption_VertColor_Selected;
								}
							}
							vColor.a *= vertAlphaRatio;

							posGL = World2GL(matrix.MultiplyPoint(curVert._pos));
							
							//이전
							//DrawBox(posGL, pointSize, pointSize, vColor, isWireFramePoint, false);

							//변경 22.4.12
							DrawVertex(ref posGL, _vertexRenderSizeHalf, ref vColor);

							AddCursorRect(mousePosition, posGL, 10, 10, MouseCursor.MoveArrow);
						}
					}

					//삭제 21.5.18
					//GL.End();//<전환 완료>  (맨밑에)
					//GL.Flush();
				}

				EndPass();


				//추가 22.3.4 (v1.4.0)
				//4. 핀을 렌더링하자
				if(renderRequest.Pin != RenderTypeRequest.VISIBILITY.Hidden)
				{
					int nPins = 0;
					List<apMeshPin> meshPins = null;
					if(mesh._pinGroup != null)
					{
						meshPins = mesh._pinGroup._pins_All;
						nPins = meshPins != null ? meshPins.Count : 0;
					}
					if (nPins > 0)
					{
						apMeshPin curPin = null;
						apMeshPinCurve cur2NextCurve = null;

						

						//4-1. 핀 라인을 렌더링하자 : Transparent일 때는 패스
						if (renderRequest.Pin == RenderTypeRequest.VISIBILITY.Shown)
						{
							_matBatch.BeginPass_Color(GL.TRIANGLES);

							Vector2 posLineA = Vector2.zero;
							Vector2 posLineB = Vector2.zero;
							int nCurvePoints = 20;
							Color curveLineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
							Color curveLineSelected = new Color(1.0f, 0.7f, 0.0f, 1.0f);

							Color curCurveColor = Color.black;

							if (renderRequest.TestPinWeight)
							{
								//[테스트 모드]
								for (int iPin = 0; iPin < nPins; iPin++)
								{
									curPin = meshPins[iPin];

									//Next로만 연결
									cur2NextCurve = curPin._nextCurve;

									if (cur2NextCurve == null)
									{
										continue;
									}

									bool isSelected = false;
									if (selectedPins != null)
									{
										if (selectedPins.Contains(cur2NextCurve._prevPin) || selectedPins.Contains(cur2NextCurve._nextPin))
										{
											isSelected = true;
										}
									}
									curCurveColor = isSelected ? curveLineSelected : curveLineColor;

									if (cur2NextCurve.IsLinear())
									{
										//두개의 핀 사이가 직선이라면
										posLineA = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Test(apMeshPin.TMP_VAR_TYPE.MeshTest, 0.0f));
										posLineB = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Test(apMeshPin.TMP_VAR_TYPE.MeshTest, 1.0f));

										DrawBoldLine(posLineA, posLineB, _pinLineThickness, curCurveColor, false);
									}
									else
									{
										//두개의 핀 사이가 커브라면
										for (int iLerp = 0; iLerp < nCurvePoints; iLerp++)
										{
											float lerpA = (float)iLerp / (float)nCurvePoints;
											float lerpB = (float)(iLerp + 1) / (float)nCurvePoints;

											posLineA = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Test(apMeshPin.TMP_VAR_TYPE.MeshTest, lerpA));
											posLineB = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Test(apMeshPin.TMP_VAR_TYPE.MeshTest, lerpB));
											DrawBoldLine(posLineA, posLineB, _pinLineThickness, curCurveColor, false);
										}
									}
								}
							}
							else
							{
								//[일반 모드]
								for (int iPin = 0; iPin < nPins; iPin++)
								{
									curPin = meshPins[iPin];
									
									//Next로만 연결
									cur2NextCurve = curPin._nextCurve;
									
									if (cur2NextCurve == null)
									{
										continue;
									}

									bool isSelected = false;
									if (selectedPins != null)
									{
										if (selectedPins.Contains(cur2NextCurve._prevPin) || selectedPins.Contains(cur2NextCurve._nextPin))
										{
											isSelected = true;
										}
									}

									curCurveColor = isSelected ? curveLineSelected : curveLineColor;

									if (cur2NextCurve.IsLinear())
									{
										//두개의 핀 사이가 직선이라면
										posLineA = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Default(0.0f));
										posLineB = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Default(1.0f));

										DrawBoldLine(posLineA, posLineB, _pinLineThickness, curCurveColor, false);
									}
									else
									{
										//두개의 핀 사이가 커브라면
										for (int iLerp = 0; iLerp < nCurvePoints; iLerp++)
										{
											float lerpA = (float)iLerp / (float)nCurvePoints;
											float lerpB = (float)(iLerp + 1) / (float)nCurvePoints;

											posLineA = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Default(lerpA));
											posLineB = matrix.MultiplyPoint(cur2NextCurve.GetCurvePos_Default(lerpB));
											DrawBoldLine(posLineA, posLineB, _pinLineThickness, curCurveColor, false);
										}
									}
								}
							}
						}

						//4-2. 선택한 Pin들의 Range를 보여주자
						if (renderRequest.PinRange)
						{
							if (selectedPin != null && selectedPins != null)
							{
								Color color_RangeInner = new Color(0.0f, 1.0f, 0.0f, 0.7f);
								Color color_RangeOuter = new Color(1.0f, 1.0f, 0.0f, 0.5f);

								_matBatch.BeginPass_Color(GL.TRIANGLES);

								int nSelectedMeshPins = selectedPins != null ? selectedPins.Count : 0;

								for (int iPin = 0; iPin < nSelectedMeshPins; iPin++)
								{
									curPin = selectedPins[iPin];
									Vector2 pinPos = renderRequest.TestPinWeight ? curPin.TmpPos_MeshTest : curPin._defaultPos;
									pinPos = matrix.MultiplyPoint(pinPos);

									float weightRange = Mathf.Max((float)curPin._range, 0.0f);
									float weightFadeRange = weightRange + Mathf.Max((float)curPin._fade, 0.0f);

									DrawBoldCircleGL(World2GL(pinPos), weightRange, 1.5f, color_RangeInner, false);
									DrawBoldCircleGL(World2GL(pinPos), weightFadeRange, 1.0f, color_RangeOuter, false);
								}

							}
						}

						//4-3. 핀을 렌더링하자
						Color pinColor_None = new Color(1.0f, 1.0f, 0.0f, 1.0f);
						Color pinColor_Selected = new Color(1.0f, 0.15f, 0.5f, 1.0f);
						if (renderRequest.Pin == RenderTypeRequest.VISIBILITY.Transparent)
						{
							//투명도를 더 줄이자 (기존 0.4, 0.6 > 변경 0.3, 0.5 v1.4.2)
							pinColor_None.a = 0.3f;
							pinColor_Selected.a = 0.5f;
						}

						_matBatch.BeginPass_VertexAndPin(GL.TRIANGLES);

						//float halfPointSizeGL = PIN_RENDER_SIZE * 0.5f;//삭제 v1.4.2 : 옵션에 따른 변수를 직접 사용

						Vector2 posGL = Vector2.zero;
						Color vColor = Color.black;

						Vector2 cpPoint_Prev = Vector2.zero;
						Vector2 cpPoint_Next = Vector2.zero;

						if (renderRequest.TestPinWeight)
						{
							//Test의 위치를 출력하자
							for (int iPin = 0; iPin < nPins; iPin++)
							{
								curPin = meshPins[iPin];

								if (selectedPins != null && selectedPins.Contains(curPin))
								{
									vColor = pinColor_Selected;
								}
								else
								{
									vColor = pinColor_None;
								}

								posGL = World2GL(matrix.MultiplyPoint(curPin.TmpPos_MeshTest));
								cpPoint_Prev = matrix.MultiplyPoint(curPin.TmpControlPos_Prev_MeshTest);
								cpPoint_Next = matrix.MultiplyPoint(curPin.TmpControlPos_Next_MeshTest);

								DrawPin(ref posGL, _pinRenderSizeHalf, ref vColor);

								AddCursorRect(mousePosition, posGL, 14, 14, MouseCursor.MoveArrow);//이건 옵션 켤때만
							}
						}
						else
						{
							//Default의 위치를 출력하자
							for (int iPin = 0; iPin < nPins; iPin++)
							{
								curPin = meshPins[iPin];

								if (selectedPins != null && selectedPins.Contains(curPin))
								{
									vColor = pinColor_Selected;
								}
								else
								{
									vColor = pinColor_None;
								}

								posGL = World2GL(matrix.MultiplyPoint(curPin._defaultPos));
								cpPoint_Prev = World2GL(matrix.MultiplyPoint(curPin._controlPointPos_Def_Prev));
								cpPoint_Next = World2GL(matrix.MultiplyPoint(curPin._controlPointPos_Def_Next));

								DrawPin(ref posGL, _pinRenderSizeHalf, ref vColor);

								AddCursorRect(mousePosition, posGL, 14, 14, MouseCursor.MoveArrow);//이건 옵션 켤때만
							}
						}

						//EndPass();

						
					}
				}

				EndPass();

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		public static void DrawMeshAreaEditing(	apMesh mesh, 
												apMatrix3x3 matrix, 
												apEditor editor,
												Vector2 mousePosition)
		{
			
			try
			{
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)//변경 코드
				{
					return;
				}
				if(!mesh._isPSDParsed)
				{
					return;
				}
				matrix *= mesh.Matrix_VertToLocal;

				Texture2D imgControlPoint = editor.ImageSet.Get(apImageSet.PRESET.TransformControlPoint);
				
				//크기는 26
				float imgSize = 26.0f / apGL.Zoom;

				Vector2 pos_LT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_LT.y));
				Vector2 pos_RT = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_LT.y));
				Vector2 pos_LB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_LT.x, mesh._atlasFromPSD_RB.y));
				Vector2 pos_RB = matrix.MultiplyPoint(new Vector2(mesh._atlasFromPSD_RB.x, mesh._atlasFromPSD_RB.y));
				
				AddCursorRect(mousePosition, World2GL(pos_LT), 20, 20, MouseCursor.MoveArrow);
				AddCursorRect(mousePosition, World2GL(pos_RT), 20, 20, MouseCursor.MoveArrow);
				AddCursorRect(mousePosition, World2GL(pos_LB), 20, 20, MouseCursor.MoveArrow);
				AddCursorRect(mousePosition, World2GL(pos_RB), 20, 20, MouseCursor.MoveArrow);

				//변경 21.5.18
				_matBatch.BeginPass_Texture_VColor(GL.TRIANGLES, _textureColor_Gray, imgControlPoint, 1.0f, apPortrait.SHADER_TYPE.AlphaBlend, false, Vector4.zero);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);

				//4개의 점을 만든다.
				


				DrawTextureGLWithVColor(imgControlPoint, World2GL(pos_LT), imgSize, imgSize, (editor.Select._meshAreaPointEditType == apSelection.MESH_AREA_POINT_EDIT.LT ? Color.red : Color.white), 1.0f);
				DrawTextureGLWithVColor(imgControlPoint, World2GL(pos_RT), imgSize, imgSize, (editor.Select._meshAreaPointEditType == apSelection.MESH_AREA_POINT_EDIT.RT ? Color.red : Color.white), 1.0f);
				DrawTextureGLWithVColor(imgControlPoint, World2GL(pos_LB), imgSize, imgSize, (editor.Select._meshAreaPointEditType == apSelection.MESH_AREA_POINT_EDIT.LB ? Color.red : Color.white), 1.0f);
				DrawTextureGLWithVColor(imgControlPoint, World2GL(pos_RB), imgSize, imgSize, (editor.Select._meshAreaPointEditType == apSelection.MESH_AREA_POINT_EDIT.RB ? Color.red : Color.white), 1.0f);

				//삭제 21.5.18
				//GL.End();//<전환 완료>
				//GL.Flush();

				EndPass();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}



		//------------------------------------------------------------------------------------------------
		// Mesh 모드에서 Mirror 라인 긋기
		//------------------------------------------------------------------------------------------------
		public static void DrawMeshMirror(apMesh mesh)
		{
			if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
			{
				return;
			}

			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			
			Vector2 posW = Vector2.zero;
			Vector2 posA_GL = Vector2.zero;
			Vector2 posB_GL = Vector2.zero;
			if(mesh._isMirrorX)
			{
				//세로 줄을 긋는다.
				//posW.x = mesh._mirrorAxis.x - (mesh._offsetPos.x + imageHalfOffset.x);
				posW.x = mesh._mirrorAxis.x - (mesh._offsetPos.x);
				posA_GL = World2GL(posW);
				posB_GL = posA_GL;

				posA_GL.y = -500;
				posB_GL.y = _windowHeight + 500;
			}
			else
			{
				//가로 줄을 긋는다.
				//posW.y = mesh._mirrorAxis.y - (mesh._offsetPos.y + imageHalfOffset.y);
				posW.y = mesh._mirrorAxis.y - (mesh._offsetPos.y);
				posA_GL = World2GL(posW);
				posB_GL = posA_GL;

				posA_GL.x = -500;
				posB_GL.x = _windowWidth + 500;
			}
			
			DrawBoldLineGL(posA_GL, posB_GL, 3, new Color(0.0f, 1.0f, 0.5f, 0.4f), true);

			
		}

		//------------------------------------------------------------------------------------------------
		// Draw Mesh의 Edge Wire
		//------------------------------------------------------------------------------------------------
		public static void DrawMeshWorkSnapNextVertex(apMesh mesh, apVertexController vertexController)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
				{
					return;
				}

				if (vertexController.LinkedNextVertex != null && vertexController.LinkedNextVertex != vertexController.Vertex)
				{
					Vector2 linkedVertPosW = vertexController.LinkedNextVertex._pos - mesh._offsetPos;
					
					//float size = 24.0f / _zoom;					
					float size = (_vertexRenderSizeHalf * 2.4f) / _zoom;//변경 v1.4.2 옵션에 의한 크기 보다 조금 더 큰 정도

					DrawBox(linkedVertPosW, size, size, Color.green, true);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("AnyPortrait : DrawMeshWorkSnapNextVertex() Exception : " + ex);
			}
		}
		public static void DrawMeshWorkEdgeSnap(apMesh mesh, apVertexController vertexController)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
				{
					return;
				}

				//if (vertexController.IsTmpSnapToEdge && vertexController.Vertex == null)
				if (vertexController.IsTmpSnapToEdge)
				{
					//float size = 20.0f / _zoom;//이전
					float size = (_vertexRenderSizeHalf * 2.0f) / _zoom;//변경 v1.4.2


					DrawBox(vertexController.TmpSnapToEdgePos - mesh._offsetPos, size, size, new Color(0.0f, 1.0f, 1.0f, 1.0f), true);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("AnyPortrait : DrawMeshWorkEdgeSnap() Exception : " + ex);
			}
		}
		public static void DrawMeshWorkEdgeWire(apMesh mesh, apMatrix3x3 matrix, apVertexController vertexController, bool isCross, bool isCrossMultiple)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				//if (mesh == null || mesh._textureData == null || mesh._textureData._image == null)
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
				{
					return;
				}

				if (vertexController.Vertex == null)
				{
					return;
				}

				matrix *= mesh.Matrix_VertToLocal;

				Vector2 mouseW = GL2World(vertexController.TmpEdgeWirePos);
				Vector2 vertPosW = matrix.MultiplyPoint(vertexController.Vertex._pos);

				Color lineColor = Color.green;
				if (isCross)
				{
					lineColor = Color.red;
				}
				else if (isCrossMultiple)
				{
					lineColor = new Color(0.2f, 0.8f, 1.0f, 1.0f);
				}

				//DrawLine(vertPosW, mouseW, lineColor, true);
				DrawAnimatedLine(vertPosW, mouseW, lineColor, true);

				//if (vertexController.LinkedNextVertex != null && vertexController.LinkedNextVertex != vertexController.Vertex)
				//{
				//	Vector2 linkedVertPosW = matrix.MultiplyPoint(vertexController.LinkedNextVertex._pos);
				//	float size = 20.0f / _zoom;
				//	DrawBox(linkedVertPosW, size, size, lineColor, true);
				//}

				//float size = 20.0f / _zoom;//이전
				float size = (_vertexRenderSizeHalf * 2.0f) / _zoom;//변경 v1.4.2

				if (isCross)
				{
					Vector2 crossPointW = matrix.MultiplyPoint(vertexController.EdgeWireCrossPoint());
					
					DrawBox(crossPointW, size, size, Color.cyan, true);
				}
				else if (isCrossMultiple)
				{
					List<Vector2> crossVerts = vertexController.EdgeWireMultipleCrossPoints();

					for (int i = 0; i < crossVerts.Count; i++)
					{
						Vector2 crossPointW = matrix.MultiplyPoint(crossVerts[i]);
						DrawBox(crossPointW, size, size, Color.yellow, true);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		public static void DrawMeshWorkMirrorEdgeWire(apMesh mesh, apMirrorVertexSet mirrorSet)
		{
			try
			{
				if (mesh == null)
				{
					return;
				}

				//DrawBox(-mesh._offsetPos, 50, 50, Color.yellow, false);

				Color lineColor = new Color(1, 1, 0, 0.5f);
				Color nearVertColor = new Color(1, 1, 0, 0.5f);

				//둘다 유효하다면 => 선을 긋는다.
				//하나만 유효하다면 => 점 또는 Vertex 외곽선을 만든다.
				bool isPrevEnabled = mirrorSet._meshWork_TypePrev != apMirrorVertexSet.MIRROR_MESH_WORK_TYPE.None;
				bool isNextEnabled = mirrorSet._meshWork_TypeNext != apMirrorVertexSet.MIRROR_MESH_WORK_TYPE.None;
				bool isPrevNearVert = false;
				bool isNextNearVert = false;
				Vector2 posPrev = Vector2.zero;
				Vector2 posNext = Vector2.zero;

				//float pointSize = 10.0f / _zoom;//<<원래 크기
				//float pointSize_Wire = 18.0f / _zoom;

				//변경 v1.4.2
				float pointSize = (_vertexRenderSizeHalf * 1.7f) / _zoom;
				float pointSize_Wire = (_vertexRenderSizeHalf * 2.0f) / _zoom;

				if(isPrevEnabled)
				{
					posPrev = mirrorSet._meshWork_PosPrev - mesh._offsetPos;
					if(mirrorSet._meshWork_VertPrev != null)
					{
						//버텍스 위치로 이동
						isPrevNearVert = true;
						posPrev = mirrorSet._meshWork_VertPrev._pos - mesh._offsetPos;
					}
				}
				if(isNextEnabled)
				{
					posNext = mirrorSet._meshWork_PosNext - mesh._offsetPos;
					if(mirrorSet._meshWork_VertNext != null)
					{
						//버텍스 위치로 이동
						isNextNearVert = true;
						posNext = mirrorSet._meshWork_VertNext._pos - mesh._offsetPos;
					}
				}
				if(isPrevEnabled && isNextEnabled)
				{
					//선을 긋자
					DrawAnimatedLine(posPrev, posNext, lineColor, true);
				}

				//점을 찍자
				if(isPrevEnabled)
				{
					if(isPrevNearVert)
					{
						DrawBox(posPrev, pointSize_Wire, pointSize_Wire, nearVertColor, true);
					}
					else
					{
						DrawBox(posPrev, pointSize, pointSize, nearVertColor, true);
					}
				}
				if(isNextEnabled)
				{
					if(isNextNearVert)
					{
						DrawBox(posNext, pointSize_Wire, pointSize_Wire, nearVertColor, true);
					}
					else
					{
						DrawBox(posNext, pointSize, pointSize, nearVertColor, true);
					}
				}

				//만약 Snap이 되는 상황이면, Snap되는 위치를 찍자
				if(mirrorSet._meshWork_SnapToAxis)
				{
					DrawBox(mirrorSet._meshWork_PosNextSnapped - mesh._offsetPos, pointSize_Wire, pointSize_Wire, new Color(0, 1, 1, 0.8f), true);
				}
			}

			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : DrawMeshWorkMirrorEdgeWire Exception : " + ex);
			}
		}

		//------------------------------------------------------------------------------------------------
		// Draw Mirror Mesh PreviewLines
		//------------------------------------------------------------------------------------------------
		public static void DrawMirrorMeshPreview(apMesh mesh, apMirrorVertexSet mirrorSet, apEditor editor, apVertexController vertexController)
		{
			try
			{
				if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)//변경 코드
				{
					return;
				}

				if(mirrorSet._cloneVerts.Count == 0)
				{
					return;
				}

				Vector2 offsetPos = mesh._offsetPos;
				apMirrorVertexSet.CloneEdge cloneEdge = null;
				apMirrorVertexSet.CloneVertex cloneVert = null;
				Vector2 pos1 = Vector2.zero;
				Vector2 pos2 = Vector2.zero;

				//Edge 렌더
				if (mirrorSet._cloneEdges.Count > 0)
				{
					Color edgeColor = editor._colorOption_MeshHiddenEdge;
					edgeColor.a *= 0.5f;

					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.LINES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.LINES);

					for (int iEdge = 0; iEdge < mirrorSet._cloneEdges.Count; iEdge++)
					{
						cloneEdge = mirrorSet._cloneEdges[iEdge];
						pos1 = cloneEdge._cloneVert1._pos - offsetPos;
						pos2 = cloneEdge._cloneVert2._pos - offsetPos;
						DrawDotLine(pos1, pos2, edgeColor, false);
					}

					//삭제 21.5.18
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}

				//Vertex 렌더 (Clone / Cross)
				Color vertColor_Mirror = new Color(0.0f, 1.0f, 0.5f, 0.5f);
				Color vertColor_Cross = new Color(1.0f, 1.0f, 0.0f, 0.5f);
				float pointSize = 10.0f / _zoom;
				float pointSize_Wire = 18.0f / _zoom;

				//1) Mirror
				
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);

				for (int iVert = 0; iVert < mirrorSet._cloneVerts.Count; iVert++)
				{
					cloneVert = mirrorSet._cloneVerts[iVert];
					pos1 = cloneVert._pos - offsetPos;
					if (!cloneVert._isOnAxis)
					{
						DrawBox(pos1, pointSize, pointSize, vertColor_Mirror, false, false);
					}
				}
				
				//삭제 21.5.18
				//GL.End();//<전환 완료>
				_matBatch.EndPass();


				//2) On Axis
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.LINES);

				for (int iVert = 0; iVert < mirrorSet._cloneVerts.Count; iVert++)
				{
					cloneVert = mirrorSet._cloneVerts[iVert];
					pos1 = cloneVert._pos - offsetPos;
					if (cloneVert._isOnAxis)
					{
						DrawBox(pos1, pointSize_Wire, pointSize_Wire, vertColor_Cross, true, false);
					}
				}
				
				//삭제 21.5.18
				//GL.End();//<전환 완료>
				_matBatch.EndPass();

				//3) Cross
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);
				//GL.Begin(GL.TRIANGLES);

				for (int iVert = 0; iVert < mirrorSet._crossVerts.Count; iVert++)
				{
					cloneVert = mirrorSet._crossVerts[iVert];
					pos1 = cloneVert._pos - offsetPos;
					DrawBox(pos1, pointSize, pointSize, vertColor_Cross, false, false);
				}

				//삭제 21.5.18
				//GL.End();//<전환 완료>
				_matBatch.EndPass();
			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : DrawMirrorMeshPreview Exception : " + ex);
			}
		}



		public static void DrawMeshWorkSnapNextPin(apMesh mesh, apMeshPin nextPin)
		{
			//0. 메시, 텍스쳐가 없을 때
			if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
			{
				return;
			}

			Vector2 linkedPinPosW = nextPin._defaultPos - mesh._offsetPos;
			float size = 20.0f / _zoom;
			DrawCircle(linkedPinPosW, size, Color.green, true);
		}


		public static void DrawMeshWorkPinWire(apMesh mesh, apMeshPin srcPin, Vector2 mousePosW, apMeshPin snapedPin)
		{
			//0. 메시, 텍스쳐가 없을 때
			if (mesh == null || mesh.LinkedTextureData == null || mesh.LinkedTextureData._image == null)
			{
				return;
			}

			if(srcPin == null)
			{
				return;
			}

			if(snapedPin != null)
			{
				//스냅시에는 스냅 핀으로 붙이자
				Vector2 linkedPinPosW = snapedPin._defaultPos - mesh._offsetPos;
				DrawAnimatedLine(srcPin._defaultPos - mesh._offsetPos, linkedPinPosW, Color.green, true);
			}
			else
			{
				DrawAnimatedLine(srcPin._defaultPos - mesh._offsetPos, mousePosW - mesh._offsetPos, Color.green, true);
			}

			
		}


		//------------------------------------------------------------------------------------------------
		// Draw Render Unit (Mesh / Outline)
		//------------------------------------------------------------------------------------------------
		private static List<apRenderVertex> _tmpSelectedVertices = new List<apRenderVertex>();
		private static List<apRenderVertex> _tmpSelectedVertices_Weighted = new List<apRenderVertex>();
		private static List<apRenderPin> _tmpSelectedPins = new List<apRenderPin>();
		private static List<apRenderPin> _tmpSelectedPins_Weighted = new List<apRenderPin>();
		//private static List<float> _tmpSelectedVertices_WeightedValue = new List<float>();

		public static void DrawRenderUnit(apRenderUnit renderUnit,
											
											//RENDER_TYPE renderType,			//이전
											RenderTypeRequest renderRequest,	//변경 22.3.3

											apVertexController vertexController,
											apSelection select,
											apEditor editor,
											Vector2 mousePos,
											
											bool isMainSelected = true)//선택된 경우, Main인지 여부 (20.5.28)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0) { return; }

				//변경 22.3.23 [v1.4.0]
				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return;
				}

				Color textureColor = renderUnit._meshColor2X;

				
				apMesh mesh = renderUnit._meshTransform._mesh;
				bool isVisible = renderUnit._isVisible;

				//메시의 버텍스/인덱스/선분 리스트
				List<apVertex> meshVerts = mesh._vertexData;
				int nVerts = meshVerts != null ? meshVerts.Count : 0;

				List<int> meshIndexBuffers = mesh._indexBuffer;
				int nIndexBuffers = meshIndexBuffers != null ? meshIndexBuffers.Count : 0;

				List<apMeshEdge> meshEdges = mesh._edges;
				int nEdges = meshEdges != null ? meshEdges.Count : 0;


				apTextureData linkedTextureData = mesh.LinkedTextureData;

				//추가 12.4 : Extra Option에 의해 Texture가 바귀었을 경우
				if(renderUnit.IsExtraTextureChanged)
				{
					linkedTextureData = renderUnit.ChangedExtraTextureData;
				}

				if(linkedTextureData == null)
				{
					return;
				}
				

				//미리 GL 좌표를 연산하고, 나중에 중복 연산(World -> GL)을 하지 않도록 하자
				apRenderVertex rVert = null;
				for (int i = 0; i < nRenderVerts; i++)
				{
					rVert = renderUnit._renderVerts[i];
					rVert._pos_GL = World2GL(rVert._pos_World);
				}


				bool isAnyVertexSelected = false;
				bool isWeightedSelected = false;

				//이전
				//bool isBoneWeightColor = (renderType & RENDER_TYPE.BoneRigWeightColor) != 0;
				//bool isPhyVolumeWeightColor = (renderType & RENDER_TYPE.PhysicsWeightColor) != 0 || (renderType & RENDER_TYPE.VolumeWeightColor) != 0;

				//변경 22.3.3 (v1.4.0)
				bool isBoneWeightColor = renderRequest.BoneRigWeightColor;
				bool isPhyVolumeWeightColor = renderRequest.PhysicsWeightColor || renderRequest.VolumeWeightColor;

				bool isBoneColor = false;
				bool isCircleRiggingVert = editor._rigViewOption_CircleVert;
				float vertexColorRatio = 0.0f;

				if (select != null)
				{
					_tmpSelectedVertices.Clear();
					_tmpSelectedVertices_Weighted.Clear();
					//_tmpSelectedVertices_WeightedValue.Clear();

					//Soft Selection + TODO 나중에 Volume 등에서 Weighted 설정을 하자
					if (select.Editor.Gizmos.IsSoftSelectionMode)
					{
						isWeightedSelected = true;
					}

					//isBoneColor = select._rigEdit_isBoneColorView;//이전
					isBoneColor = editor._rigViewOption_BoneColor;//변경 19.7.31

					if (isBoneWeightColor)
					{
						//if (select._rigEdit_viewMode == apSelection.RIGGING_EDIT_VIEW_MODE.WeightColorOnly)
						if(editor._rigViewOption_WeightOnly)
						{
							vertexColorRatio = 1.0f;
						}
						else
						{
							vertexColorRatio = 0.5f;
						}
					}
					else if (isPhyVolumeWeightColor)
					{
						vertexColorRatio = 0.7f;
					}

					isAnyVertexSelected = true;
					if (select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
						|| select.SelectionType == apSelection.SELECTION_TYPE.Animation//<<추가 20.6.29 : 통합됨
						)
					{
						if (select.ModRenderVerts_All != null && select.ModRenderVerts_All.Count > 0)
						{
							List<apSelection.ModRenderVert> selectedMRVs = select.ModRenderVerts_All;
							int nSelectedMRV = selectedMRVs.Count;

							for (int i = 0; i < nSelectedMRV; i++)
							{
								_tmpSelectedVertices.Add(selectedMRVs[i]._renderVert);
							}

							if (isWeightedSelected)
							{
								List<apSelection.ModRenderVert> weightedMRVs = select.ModRenderVerts_Weighted;
								int nWeightedMRV = weightedMRVs.Count;

								if (nWeightedMRV > 0)
								{
									apSelection.ModRenderVert curMRV = null;
									for (int i = 0; i < nWeightedMRV; i++)
									{
										curMRV = weightedMRVs[i];
										curMRV._renderVert._renderWeightByTool = curMRV._vertWeightByTool;

										_tmpSelectedVertices_Weighted.Add(curMRV._renderVert);
									}
								}
								
							}
						}
					}
				}


				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.
				bool isMeshRender = false;
				
				//이전
				//bool isVertexRender = ((renderType & RENDER_TYPE.Vertex) != 0);
				//bool isOutlineRender = ((renderType & RENDER_TYPE.Outlines) != 0);
				//bool isAllEdgeRender = ((renderType & RENDER_TYPE.AllEdges) != 0);
				//bool isToneColor = ((renderType & RENDER_TYPE.ToneColor) != 0);

				//변경 22.3.3 (v1.4.0)
				bool isVertexRender =	renderRequest.Vertex != RenderTypeRequest.VISIBILITY.Hidden;
				bool isOutlineRender =	renderRequest.Outlines;
				bool isAllEdgeRender =	renderRequest.AllEdges;
				bool isToneColor =		renderRequest.ToneColor;



				if (!isVertexRender && !isOutlineRender)
				{
					isMeshRender = true;
				}
				bool isNotEditedGrayColor = false;

				if(editor._exModObjOption_ShowGray && 
					(	renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_NotEdit ||
						renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_ExRun))
				{
					//선택되지 않은 건 Gray 색상으로 표시하고자 할 때
					isNotEditedGrayColor = true;
				}

				
				//bool isDrawTFBorderLine = ((int)(renderType & RENDER_TYPE.TransformBorderLine) != 0);	//이전
				bool isDrawTFBorderLine = renderRequest.TransformBorderLine;							//변경 22.3.3 (v1.4.0)

				//2. 메시를 렌더링하자
				if (nIndexBuffers >= 3 && isMeshRender && isVisible)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					Color color0 = Color.black, color1 = Color.black, color2 = Color.black;

					int iVertColor = 0;

					if (renderRequest.VolumeWeightColor)						//변경 22.3.3
					{
						iVertColor = 1;
					}
					//else if ((renderType & RENDER_TYPE.PhysicsWeightColor) != 0)	//이전
					else if (renderRequest.PhysicsWeightColor)						//변경 22.3.3
					{
						iVertColor = 2;
					}
					//else if ((renderType & RENDER_TYPE.BoneRigWeightColor) != 0)	//이전
					else if (renderRequest.BoneRigWeightColor)						//변경 22.3.3
					{
						iVertColor = 3;
					}
					else
					{
						iVertColor = 0;
						color0 = Color.black;
						color1 = Color.black;
						color2 = Color.black;
					}


					if (isToneColor)
					{
						_matBatch.BeginPass_ToneColor_Normal(GL.TRIANGLES, _toneColor, linkedTextureData._image);

					}
					else if (isNotEditedGrayColor)
					{
						//추가 21.2.16 : 편집되지 않은 경우
						_matBatch.BeginPass_Gray_Normal(GL.TRIANGLES, textureColor, linkedTextureData._image);
					}
					else if (isBoneWeightColor || isPhyVolumeWeightColor)
					{
						//가중치 색상
						_matBatch.BeginPass_Texture_VColor(GL.TRIANGLES, textureColor, linkedTextureData._image, vertexColorRatio, renderUnit.ShaderType, false, Vector4.zero);
					}
					else
					{
						//기본 색상
						_matBatch.BeginPass_Texture_VColor(GL.TRIANGLES, textureColor, linkedTextureData._image, 0.0f, renderUnit.ShaderType, false, Vector4.zero);
					}

					//삭제 21.5.18 : SetPass시 자동으로 설정한다.
					//_matBatch.SetClippingSize(_glScreenClippingSize);					
					//GL.Begin(GL.TRIANGLES);

					//------------------------------------------
					//apVertex vert0, vert1, vert2;
					apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;


					Vector3 pos_0 = Vector3.zero;
					Vector3 pos_1 = Vector3.zero;
					Vector3 pos_2 = Vector3.zero;


					Vector2 uv_0 = Vector2.zero;
					Vector2 uv_1 = Vector2.zero;
					Vector2 uv_2 = Vector2.zero;

					int iVert_0 = 0;
					int iVert_1 = 0;
					int iVert_2 = 0;

					for (int i = 0; i < nIndexBuffers; i += 3)
					{
						if (i + 2 >= nIndexBuffers) { break; }

						iVert_0 = meshIndexBuffers[i + 0];
						iVert_1 = meshIndexBuffers[i + 1];
						iVert_2 = meshIndexBuffers[i + 2];

						if (iVert_0 >= nVerts ||
							iVert_1 >= nVerts ||
							iVert_2 >= nVerts)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[iVert_0];
						rVert1 = renderUnit._renderVerts[iVert_1];
						rVert2 = renderUnit._renderVerts[iVert_2];

						pos_0.x = rVert0._pos_GL.x;
						pos_0.y = rVert0._pos_GL.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = rVert1._pos_GL.x;
						pos_1.y = rVert1._pos_GL.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = rVert2._pos_GL.x;
						pos_2.y = rVert2._pos_GL.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						//uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						//uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						//uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						uv_0 = rVert0._vertex._uv;
						uv_1 = rVert1._vertex._uv;
						uv_2 = rVert2._vertex._uv;

						switch (iVertColor)
						{
							case 1: //VolumeWeightColor
								color0 = GetWeightGrayscale(rVert0._renderWeightByTool);
								color1 = GetWeightGrayscale(rVert1._renderWeightByTool);
								color2 = GetWeightGrayscale(rVert2._renderWeightByTool);
								break;

							case 2: //PhysicsWeightColor
								color0 = GetWeightColor4(rVert0._renderWeightByTool);
								color1 = GetWeightColor4(rVert1._renderWeightByTool);
								color2 = GetWeightColor4(rVert2._renderWeightByTool);
								break;

							case 3: //BoneRigWeightColor
									//TODO : 본 리스트를 받아서 해야하는디..
								if (isBoneColor)
								{
									color0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									color1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									color2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								}
								else
								{
									color0 = _func_GetWeightColor3(rVert0._renderWeightByTool);
									color1 = _func_GetWeightColor3(rVert1._renderWeightByTool);
									color2 = _func_GetWeightColor3(rVert2._renderWeightByTool);
								}
								color0.a = 1.0f;
								color1.a = 1.0f;
								color2.a = 1.0f;
								
								break;
						}
						////------------------------------------------

						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						GL.Color(color2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(color1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(color0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						////------------------------------------------
					}

					//삭제 21.5.18 : 자동으로 End 호출되도록 변경
					//GL.End();//전환 완료 > 외부에서 한번에 EndPass
					//GL.Flush();

				}

				//3. Edge를 렌더링하자
				if (isAllEdgeRender && nEdges > 0)
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;

					apMeshEdge curEdge = null;

					apRenderVertex rVert0 = null, rVert1 = null;
					
					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.LINES);
					
					for (int i = 0; i < nEdges; i++)
					{
						curEdge = meshEdges[i];

						rVert0 = renderUnit._renderVerts[curEdge._vert1._index];
						rVert1 = renderUnit._renderVerts[curEdge._vert2._index];

						pos0 = rVert0._pos_GL;
						pos1 = rVert1._pos_GL;

						DrawLineGL(pos0, pos1, editor._colorOption_MeshEdge, false);
					}

					//삭제 21.5.18
					//GL.End();//전환 완료 > 외부에서 한번에 EndPass
				}
				else if (isOutlineRender && nEdges > 0)
				{
					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					apRenderVertex rVert0 = null, rVert1 = null;
					
					
					//변경 21.5.18
					_matBatch.BeginPass_Color(GL.TRIANGLES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);

					apMeshEdge curEdge = null;

					for (int i = 0; i < nEdges; i++)
					{
						curEdge = meshEdges[i];

						if (!curEdge._isOutline) { continue; }

						rVert0 = renderUnit._renderVerts[curEdge._vert1._index];
						rVert1 = renderUnit._renderVerts[curEdge._vert2._index];
							
						pos0 = rVert0._pos_GL;
						pos1 = rVert1._pos_GL;

						DrawBoldLineGL(pos0, pos1, 6.0f, editor._colorOption_Outline, false);
					}
				}

				if (isDrawTFBorderLine && nEdges > 0)
				{
					float minPosLocal_X = float.MaxValue;
					float maxPosLocal_X = float.MinValue;
					float minPosLocal_Y = float.MaxValue;
					float maxPosLocal_Y = float.MinValue;

					Vector2 pos0 = Vector2.zero, pos1 = Vector2.zero;
					apRenderVertex rVert0 = null, rVert1 = null;

					apMeshEdge curEdge = null;

					for (int i = 0; i < nEdges; i++)
					{
						curEdge = mesh._edges[i];
						
						if (!curEdge._isOutline) { continue; }

						rVert0 = renderUnit._renderVerts[curEdge._vert1._index];
						rVert1 = renderUnit._renderVerts[curEdge._vert2._index];

						pos0 = rVert0._pos_World;
						pos1 = rVert1._pos_World;

						if (rVert0._pos_LocalOnMesh.x < minPosLocal_X) { minPosLocal_X = rVert0._pos_LocalOnMesh.x; }
						if (rVert0._pos_LocalOnMesh.x > maxPosLocal_X) { maxPosLocal_X = rVert0._pos_LocalOnMesh.x; }
						if (rVert0._pos_LocalOnMesh.y < minPosLocal_Y) { minPosLocal_Y = rVert0._pos_LocalOnMesh.y; }
						if (rVert0._pos_LocalOnMesh.y > maxPosLocal_Y) { maxPosLocal_Y = rVert0._pos_LocalOnMesh.y; }

						if (rVert1._pos_LocalOnMesh.x < minPosLocal_X) { minPosLocal_X = rVert1._pos_LocalOnMesh.x; }
						if (rVert1._pos_LocalOnMesh.x > maxPosLocal_X) { maxPosLocal_X = rVert1._pos_LocalOnMesh.x; }
						if (rVert1._pos_LocalOnMesh.y < minPosLocal_Y) { minPosLocal_Y = rVert1._pos_LocalOnMesh.y; }
						if (rVert1._pos_LocalOnMesh.y > maxPosLocal_Y) { maxPosLocal_Y = rVert1._pos_LocalOnMesh.y; }
					}


					DrawTransformBorderFormOfRenderUnit(editor._colorOption_TransformBorder, minPosLocal_X, maxPosLocal_X, maxPosLocal_Y, minPosLocal_Y, renderUnit.WorldMatrix);
				}


				//3. 버텍스를 렌더링하자
				if (isVertexRender && nRenderVerts > 0)
				{
					//float halfPointSize = VERTEX_RENDER_SIZE * 0.5f;//삭제 v1.4.2 : 옵션에 따른 변수값을 바로 사용

					Vector2 posGL = Vector2.zero;
					bool isVertSelected = false;

					float vertAlphaRatio = renderRequest.Vertex == RenderTypeRequest.VISIBILITY.Transparent ? 0.3f : 1.0f;

					if (isAnyVertexSelected)
					{
						bool isDrawRigCircle = (isBoneWeightColor && isCircleRiggingVert);
						if(isDrawRigCircle)
						{
							//원형의 Rigging 버텍스
							_matBatch.BeginPass_RigCircleV2(GL.TRIANGLES);//변경 20.3.25 > V2
							
						}
						else
						{
							//기본 사각형 버텍스
							//_matBatch.BeginPass_Color(GL.TRIANGLES);//이전

							//변경 22.4.12
							_matBatch.BeginPass_VertexAndPin(GL.TRIANGLES);
						}
						
						//삭제 21.5.18
						//_matBatch.SetClippingSize(_glScreenClippingSize);
						//GL.Begin(GL.TRIANGLES);


						Color vColor = Color.black;
						//Color vColorOutline = _vertColor_Outline;
						for (int i = 0; i < nRenderVerts; i++)
						{
							vColor = editor._colorOption_VertColor_NotSelected;
							//vColorOutline = _vertColor_Outline;

							rVert = renderUnit._renderVerts[i];
							isVertSelected = false;

							if (isBoneWeightColor)
							{
								if (isBoneColor)
								{
									vColor = rVert._renderColorByTool;
								}
								else
								{
									vColor = _func_GetWeightColor3_Vert(rVert._renderWeightByTool);
								}
							}
							else if (isPhyVolumeWeightColor)
							{
								vColor = GetWeightColor4_Vert(rVert._renderWeightByTool);
							}

							if (_tmpSelectedVertices != null)
							{
								if (_tmpSelectedVertices.Contains(rVert))
								{
									//선택된 경우
									isVertSelected = true;

									if (isBoneWeightColor || isPhyVolumeWeightColor)
									{
										//vColorOutline = _vertColor_Outline_White;
									}
									else
									{
										vColor = editor._colorOption_VertColor_Selected;
									}
									
								}
								else if (isWeightedSelected && _tmpSelectedVertices_Weighted != null)
								{
									if (_tmpSelectedVertices_Weighted.Contains(rVert))
									{
										vColor = GetWeightColor2(rVert._renderWeightByTool, editor);
									}
								}
							}
							
							vColor.a *= vertAlphaRatio;

							posGL = rVert._pos_GL;
							if(isDrawRigCircle)
							{
								//V1
								//DrawRiggingRenderVert(rVert, vColorOutline, isBoneColor, isVertSelected);
								
								//V2
								float clickSize = DrawRiggingRenderVert_V2(rVert, isBoneColor, isVertSelected);
								
								AddCursorRect(mousePos, posGL, clickSize, clickSize, MouseCursor.MoveArrow);
							}
							else
							{
								//이전의 Box
								//if (isVertSelected || isBoneWeightColor || isPhyVolumeWeightColor)
								//{
								//	DrawBoxGL(posGL, pointSizeOutline, pointSizeOutline, vColorOutline, false, false);
								//}

								//DrawBoxGL(posGL, pointSize, pointSize, vColor, false, false);

								if (isVertSelected && (isBoneWeightColor || isPhyVolumeWeightColor))
								{
									//하얀색 외곽선으로 보인다.
									DrawVertex_WhiteOutline(ref posGL, _vertexRenderSizeHalf, ref vColor);
								}
								else
								{
									DrawVertex(ref posGL, _vertexRenderSizeHalf, ref vColor);
								}

								//선택 영역 : 박스 형태는 고정 크기이다.
								AddCursorRect(mousePos, posGL, 10, 10, MouseCursor.MoveArrow);
							}
							

							
						}

						//삭제 21.5.18
						//GL.End();//전환 완료 > 외부에서 한번에 EndPass
					}
					

					if (isPhyVolumeWeightColor && nRenderVerts > 0)
					{
						float pointSize_PhysicImg = 40.0f / _zoom;
						
						//추가적인 Vertex 이미지를 추가한다.
						//RenderVertex의 Param으로 이미지를 추가한다.

						//1. Physic Main
						//변경 21.5.18
						_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, _textureColor_Gray, _img_VertPhysicMain, apPortrait.SHADER_TYPE.AlphaBlend);
						
						for (int i = 0; i < nRenderVerts; i++)
						{
							rVert = renderUnit._renderVerts[i];
							if (rVert._renderParam == 1)
							{
								DrawTextureGL(_img_VertPhysicMain, rVert._pos_GL, pointSize_PhysicImg, pointSize_PhysicImg, _textureColor_Gray, 0.0f, false);
							}
						}

						//삭제 21.5.18
						//GL.End();//전환 완료 > 외부에서 한번에 EndPass
						


						//2. Physic Constraint
						//변경 21.5.18
						_matBatch.BeginPass_Texture_Normal(GL.TRIANGLES, _textureColor_Gray, _img_VertPhysicConstraint, apPortrait.SHADER_TYPE.AlphaBlend);
						//_matBatch.SetClippingSize(_glScreenClippingSize);
						//GL.Begin(GL.TRIANGLES);

						for (int i = 0; i < nRenderVerts; i++)
						{
							rVert = renderUnit._renderVerts[i];
							if (rVert._renderParam == 2)
							{
								DrawTextureGL(_img_VertPhysicConstraint, rVert._pos_GL, pointSize_PhysicImg, pointSize_PhysicImg, _textureColor_Gray, 0.0f, false);
							}
						}

						//삭제 21.5.18
						//GL.End();//전환 완료 > 외부에서 한번에 EndPass
						
					}
				}



				//추가 22.4.4 [v1.4.0]
				//4. 핀을 렌더링하자
				if(renderRequest.Pin != RenderTypeRequest.VISIBILITY.Hidden)
				{
					//핀
					int nPins = 0;
					apRenderPinGroup rPinGroup = renderUnit._renderPinGroup;
					if(rPinGroup != null)
					{
						nPins = rPinGroup.NumPins;
					}

					if(nPins > 0)
					{
						//선택된 RenderPin을 가져와야 한다.
						//apRenderPin selectedPin = null;
						_tmpSelectedPins.Clear();
						_tmpSelectedPins_Weighted.Clear();

						if (select != null)
						{
							int nModRenderPins = select.ModRenderPins_All != null ? select.ModRenderPins_All.Count : 0;
							if (nModRenderPins > 0)
							{
								List<apSelection.ModRenderPin> selectedMRPs = select.ModRenderPins_All;

								for (int i = 0; i < nModRenderPins; i++)
								{
									_tmpSelectedPins.Add(selectedMRPs[i]._renderPin);
								}

								if (isWeightedSelected)
								{
									List<apSelection.ModRenderPin> weightedMRPs = select.ModRenderPins_Weighted;
									int nWeightedMRP = weightedMRPs != null ? weightedMRPs.Count : 0;

									apSelection.ModRenderPin curMRP = null;
									for (int i = 0; i < nWeightedMRP; i++)
									{
										curMRP = weightedMRPs[i];
										curMRP._renderPin._renderWeightByTool = curMRP._pinWeightByTool;
										_tmpSelectedPins_Weighted.Add(curMRP._renderPin);
									}
								}
							}
						}
					
						apRenderPin curPin = null;

						//4-1. 핀 라인을 렌더링하자
						if (renderRequest.Pin == RenderTypeRequest.VISIBILITY.Shown)
						{
							apRenderPinCurve cur2NextCurve = null;

							_matBatch.BeginPass_Color(GL.TRIANGLES);


							Vector2 posLineA = Vector2.zero;
							Vector2 posLineB = Vector2.zero;
							int nCurvePoints = 20;

							Color curveLineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
							Color curveLineSelected = new Color(1.0f, 0.7f, 0.0f, 1.0f);

							Color curCurveColor = Color.black;

							//4-1. 핀 라인을 렌더링하자 (커브)
							for (int iPin = 0; iPin < nPins; iPin++)
							{
								curPin = rPinGroup._pins[iPin];
								cur2NextCurve = curPin._nextRenderCurve;

								//Next로만 연결
								if (cur2NextCurve == null)
								{
									continue;
								}

								bool isSelected = false;
								if (_tmpSelectedPins != null)
								{
									if (_tmpSelectedPins.Contains(cur2NextCurve._prevPin) || _tmpSelectedPins.Contains(cur2NextCurve._nextPin))
									{
										isSelected = true;
									}
								}

								curCurveColor = isSelected ? curveLineSelected : curveLineColor;


								if (cur2NextCurve.IsLinear())
								{
									//두개의 핀 사이가 직선이라면
									posLineA = cur2NextCurve.GetCurvePosW(0.0f);
									posLineB = cur2NextCurve.GetCurvePosW(1.0f);
									DrawBoldLine(posLineA, posLineB, _pinLineThickness, curCurveColor, false);
								}
								else
								{
									//두개의 핀 사이가 커브라면
									for (int iLerp = 0; iLerp < nCurvePoints; iLerp++)
									{
										float lerpA = (float)iLerp / (float)nCurvePoints;
										float lerpB = (float)(iLerp + 1) / (float)nCurvePoints;

										posLineA = cur2NextCurve.GetCurvePosW(lerpA);
										posLineB = cur2NextCurve.GetCurvePosW(lerpB);
										DrawBoldLine(posLineA, posLineB, _pinLineThickness, curCurveColor, false);
									}
								}
							}
						}
						
						

						//4-2. 핀을 렌더링하자
						Color pinColor_None = new Color(1.0f, 1.0f, 0.0f, 1.0f);
						Color pinColor_Selected = new Color(1.0f, 0.15f, 0.5f, 1.0f);
						Color pinColor_Black = new Color(0.2f, 0.2f, 0.2f, 1.0f);
						
						if (renderRequest.Pin == RenderTypeRequest.VISIBILITY.Transparent)
						{
							pinColor_None.a = 0.4f;
							pinColor_Selected.a = 0.6f;
							pinColor_Black.a = 0.3f;
						}

						_matBatch.BeginPass_VertexAndPin(GL.TRIANGLES);

						//float halfPointSize = PIN_RENDER_SIZE * 0.5f;//삭제 v1.4.2 : 옵션에 따른 
						
						Vector2 posGL = Vector2.zero;
						Color vColor = Color.black;

						Vector2 cpPoint_Prev = Vector2.zero;
						Vector2 cpPoint_Next = Vector2.zero;

						for (int iPin = 0; iPin < nPins; iPin++)
						{
							curPin = rPinGroup._pins[iPin];

							vColor = pinColor_None;

							if (_tmpSelectedPins != null && _tmpSelectedPins.Contains(curPin))
							{
								vColor = pinColor_Selected;
							}
							else if (isWeightedSelected)
							{
								if (_tmpSelectedPins_Weighted.Contains(curPin))
								{
									vColor = GetWeightColor2(curPin._renderWeightByTool, editor);
								}
								else
								{
									//SoftSelection시의 선택되지 않은 핀들
									vColor = pinColor_Black;
								}
							}

							posGL = World2GL(curPin._pos_World);
							
							DrawPin(ref posGL, _pinRenderSizeHalf, ref vColor);

							AddCursorRect(mousePos, World2GL(posGL), 10, 10, MouseCursor.MoveArrow);//이건 옵션 켤때만
						}
						

						//EndPass();
					}
				}
				//DrawText("<-[" + renderUnit.Name + "_" + renderUnit._debugID + "]", renderUnit.WorldMatrixWrap._pos + new Vector2(10.0f, 0.0f), 100, Color.yellow);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}



		public static void DrawRenderUnit_Basic_ForExport(apRenderUnit renderUnit)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0) { return; }

				//변경 22.3.23 [v1.3.0]
				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return;
				}

				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;
				bool isVisible = renderUnit._isVisible;


				apTextureData linkedTextureData = mesh.LinkedTextureData;

				//추가 12.4 : Extra Option에 의해 Texture가 바귀었을 경우
				if(renderUnit.IsExtraTextureChanged)
				{
					linkedTextureData = renderUnit.ChangedExtraTextureData;
				}


				//if (mesh.LinkedTextureData == null)//이전
				if(linkedTextureData == null)
				{
					return;
				}

				//미리 GL 좌표를 연산하고, 나중에 중복 연산(World -> GL)을 하지 않도록 하자
				apRenderVertex rVert = null;
				for (int i = 0; i < nRenderVerts; i++)
				{
					rVert = renderUnit._renderVerts[i];
					rVert._pos_GL = World2GL(rVert._pos_World);
				}



				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3 && isVisible)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					// Debug.Log("Texture Color : " + textureColor);
					//Color color0 = Color.black, color1 = Color.black, color2 = Color.black;
					Color color0 = Color.black;

					//int iVertColor = 0;
					color0 = Color.black;
					//color1 = Color.black;
					//color2 = Color.black;

					//변경 21.5.18 : Clipping Size가 바뀐다면 이전 Pass는 종료시키자
					_matBatch.EndPass();
					_matBatch.BeginPass_Texture_VColor(GL.TRIANGLES, textureColor, linkedTextureData._image, 0.0f, renderUnit.ShaderType, true, new Vector4(0, 0, 1, 1));//변경
					//_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));
					//GL.Begin(GL.TRIANGLES);


					//------------------------------------------
					//apVertex vert0, vert1, vert2;
					apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

					Vector3 pos_0 = Vector3.zero;
					Vector3 pos_1 = Vector3.zero;
					Vector3 pos_2 = Vector3.zero;


					Vector2 uv_0 = Vector2.zero;
					Vector2 uv_1 = Vector2.zero;
					Vector2 uv_2 = Vector2.zero;

					GL.Color(color0);//추가. Color는 한번만 적용

					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						//Vector3 pos_0 = World2GL(rVert0._pos_World3);
						//Vector3 pos_1 = World2GL(rVert1._pos_World3);
						//Vector3 pos_2 = World2GL(rVert2._pos_World3);

						pos_0.x = rVert0._pos_GL.x;
						pos_0.y = rVert0._pos_GL.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = rVert1._pos_GL.x;
						pos_1.y = rVert1._pos_GL.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = rVert2._pos_GL.x;
						pos_2.y = rVert2._pos_GL.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


						////------------------------------------------
						
						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						////------------------------------------------
					}
					
					//삭제 21.5.18
					//GL.End();//전환 완료

					_matBatch.EndPass();
					_matBatch.SetClippingSize(_glScreenClippingSize);
					
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}


		public static void DrawRenderUnit_Basic_Alpha2White_ForExport(apRenderUnit renderUnit)
		{
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0) { return; }

				//변경 22.3.23 [v1.4.0]
				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return;
				}


				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;
				bool isVisible = renderUnit._isVisible;

				apTextureData linkedTextureData = mesh.LinkedTextureData;

				//추가 12.4 : Extra Option에 의해 Texture가 바귀었을 경우
				if(renderUnit.IsExtraTextureChanged)
				{
					linkedTextureData = renderUnit.ChangedExtraTextureData;
				}

				//if (mesh.LinkedTextureData == null)//이전
				if(linkedTextureData == null)
				{
					return;
				}

				//미리 GL 좌표를 연산하고, 나중에 중복 연산(World -> GL)을 하지 않도록 하자
				apRenderVertex rVert = null;
				for (int i = 0; i < nRenderVerts; i++)
				{
					rVert = renderUnit._renderVerts[i];
					rVert._pos_GL = World2GL(rVert._pos_World);
				}



				//2. 메시를 렌더링하자
				if (mesh._indexBuffer.Count >= 3 && isVisible)
				{
					//------------------------------------------
					// Drawcall Batch를 했을때
					// Debug.Log("Texture Color : " + textureColor);
					//Color color0 = Color.black, color1 = Color.black, color2 = Color.black;
					Color color0 = Color.black;

					//int iVertColor = 0;
					color0 = Color.black;
					//color1 = Color.black;
					//color2 = Color.black;


					//변경 21.5.18
					//Clipping Size를 바꾼다면, 이전의 Pass를 종료시켜야 한다.
					_matBatch.EndPass();
					_matBatch.BeginPass_Alpha2White(GL.TRIANGLES, textureColor, linkedTextureData._image, new Vector4(0, 0, 1, 1));//<<Shader를 Alpha2White로 한다. + ExtraOption
					//GL.Begin(GL.TRIANGLES);

					//------------------------------------------
					//apVertex vert0, vert1, vert2;
					apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

					Vector3 pos_0 = Vector3.zero;
					Vector3 pos_1 = Vector3.zero;
					Vector3 pos_2 = Vector3.zero;


					Vector2 uv_0 = Vector2.zero;
					Vector2 uv_1 = Vector2.zero;
					Vector2 uv_2 = Vector2.zero;

					//색상은 한번만 적용하자
					GL.Color(color0);

					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						//Vector3 pos_0 = World2GL(rVert0._pos_World3);
						//Vector3 pos_1 = World2GL(rVert1._pos_World3);
						//Vector3 pos_2 = World2GL(rVert2._pos_World3);

						pos_0.x = rVert0._pos_GL.x;
						pos_0.y = rVert0._pos_GL.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = rVert1._pos_GL.x;
						pos_1.y = rVert1._pos_GL.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = rVert2._pos_GL.x;
						pos_2.y = rVert2._pos_GL.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


						////------------------------------------------

						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						/*GL.Color(color2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						/*GL.Color(color1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(color0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0

						////------------------------------------------
					}

					//삭제 21.5.18
					//GL.End();//<전환 완료>

					//Clipped Size를 복구하고 Pass를 강제로 종료한다.					
					_matBatch.EndPass();
					_matBatch.SetClippingSize(_glScreenClippingSize);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}




		//---------------------------------------------------------------------------------------
		// Draw Render Unit : Clipping
		// RenderType은 MeshColor에 영향을 주는 것들만 허용한다.
		//---------------------------------------------------------------------------------------
		

		public static void DrawRenderUnit_ClippingParent_Renew(	apRenderUnit renderUnit,
																
																//RENDER_TYPE renderType,			//이전
																RenderTypeRequest renderRequest,	//변경 22.3.3 (v1.4.0)


																List<apTransform_Mesh.ClipMeshSet> childClippedSet,
																//List<apTransform_Mesh> childMeshTransforms, 
																//List<apRenderUnit> childRenderUnits, 
																apVertexController vertexController,
																apEditor editor,
																apSelection select,
																RenderTexture externalRenderTexture = null)
		{
			//렌더링 순서
			//Parent - 기본
			//Parent - Mask
			//(For) Child - Clipped
			//Release RenderMask
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0)
				//{
				//	return;
				//}

				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return;
				}


				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;


				apTextureData linkedTextureData = mesh.LinkedTextureData;

				//추가 12.4 : Extra Option에 의해 Texture가 바귀었을 경우
				if(renderUnit.IsExtraTextureChanged)
				{
					linkedTextureData = renderUnit.ChangedExtraTextureData;
				}

				//if (mesh.LinkedTextureData == null)//이전
				if(linkedTextureData == null)
				{
					return;
				}



				int nClipMeshes = childClippedSet.Count;

				//이전
				//bool isBoneWeightColor = (int)(renderType & RENDER_TYPE.BoneRigWeightColor) != 0;
				//bool isPhyVolumeWeightColor = (renderType & RENDER_TYPE.PhysicsWeightColor) != 0 || (renderType & RENDER_TYPE.VolumeWeightColor) != 0;

				//변경 22.3.3 (v1.4.0)
				bool isBoneWeightColor = renderRequest.BoneRigWeightColor;
				bool isPhyVolumeWeightColor = renderRequest.PhysicsWeightColor || renderRequest.VolumeWeightColor;

				int iVertColor = 0;

				//if ((renderType & RENDER_TYPE.VolumeWeightColor) != 0)	//이전
				if (renderRequest.VolumeWeightColor)						//변경 22.3.3
				{
					iVertColor = 1;
				}
				//else if ((renderType & RENDER_TYPE.PhysicsWeightColor) != 0)	//이전
				else if (renderRequest.PhysicsWeightColor)						//변경 22.3.3
				{
					iVertColor = 2;
				}
				//else if ((renderType & RENDER_TYPE.BoneRigWeightColor) != 0)	//이전
				else if (renderRequest.BoneRigWeightColor)						//변경 22.3.3
				{
					iVertColor = 3;
				}
				else
				{
					iVertColor = 0;
				}

				bool isBoneColor = false;
				//bool isCircleRiggingVert = editor._rigViewOption_CircleVert;
				float vertexColorRatio = 0.0f;

				//bool isToneColor = (int)(renderType & RENDER_TYPE.ToneColor) != 0;	//이전
				bool isToneColor = renderRequest.ToneColor;								//변경 22.3.3

				if (select != null)
				{
					//isBoneColor = select._rigEdit_isBoneColorView;//이전
					isBoneColor = editor._rigViewOption_BoneColor;//변경 19.7.31

					if (isBoneWeightColor)
					{
						//if (select._rigEdit_viewMode == apSelection.RIGGING_EDIT_VIEW_MODE.WeightColorOnly)
						if(editor._rigViewOption_WeightOnly)
						{
							vertexColorRatio = 1.0f;
						}
						else
						{
							vertexColorRatio = 0.5f;
						}
					}
					else if (isPhyVolumeWeightColor)
					{
						vertexColorRatio = 0.7f;
					}
				}

				//추가 21.2.16 : 선택되지 않은 RenderUnit은 회색으로 표시
				bool isNotEditedGrayColor_Parent = false;

				if (editor._exModObjOption_ShowGray &&
					(renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_NotEdit ||
						renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_ExRun))
				{
					//선택되지 않은 건 Gray 색상으로 표시하고자 할 때
					isNotEditedGrayColor_Parent = true;
				}



				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

				//1. Parent의 기본 렌더링을 하자
				//+2. Parent의 마스크를 렌더링하자
				if (mesh._indexBuffer.Count < 3)
				{
					return;
				}

				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

				Color vertexChannelColor = Color.black;
				Color vColor0 = Color.black, vColor1 = Color.black, vColor2 = Color.black;

				Vector2 posGL_0 = Vector2.zero;
				Vector2 posGL_1 = Vector2.zero;
				Vector2 posGL_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;
		
				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;


				
				RenderTexture.active = null;

				for (int iPass = 0; iPass < 2; iPass++)
				{
					bool isRenderTexture = false;
					if (iPass == 1)
					{
						isRenderTexture = true;
					}
					if(isToneColor)
					{
						// ToneColor Mask
						_matBatch.BeginPass_Mask_ToneColor(GL.TRIANGLES, _toneColor, linkedTextureData._image, isRenderTexture);
						
					}
					else if(isNotEditedGrayColor_Parent)
					{
						_matBatch.BeginPass_Mask_Gray(GL.TRIANGLES, textureColor, linkedTextureData._image, isRenderTexture);
					}
					else
					{
						//일반적인 Mask
						_matBatch.BeginPass_Mask(GL.TRIANGLES, textureColor, linkedTextureData._image, vertexColorRatio, renderUnit.ShaderType, isRenderTexture, false, Vector4.zero);
					}
					
					//삭제 21.5.18
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);


					//------------------------------------------
					for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
					{

						if (i + 2 >= mesh._indexBuffer.Count)
						{ break; }

						if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
							mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
						{
							break;
						}

						rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
						rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
						rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

						vColor0 = Color.black;
						vColor1 = Color.black;
						vColor2 = Color.black;

						
						switch (iVertColor)
						{
							case 1: //VolumeWeightColor
								vColor0 = GetWeightGrayscale(rVert0._renderWeightByTool);
								vColor1 = GetWeightGrayscale(rVert1._renderWeightByTool);
								vColor2 = GetWeightGrayscale(rVert2._renderWeightByTool);
								break;

							case 2: //PhysicsWeightColor
								vColor0 = GetWeightColor4(rVert0._renderWeightByTool);
								vColor1 = GetWeightColor4(rVert1._renderWeightByTool);
								vColor2 = GetWeightColor4(rVert2._renderWeightByTool);
								break;

							case 3: //BoneRigWeightColor
								if (isBoneColor)
								{
									vColor0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									vColor1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
									vColor2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								}
								else
								{
									vColor0 = _func_GetWeightColor3(rVert0._renderWeightByTool);
									vColor1 = _func_GetWeightColor3(rVert1._renderWeightByTool);
									vColor2 = _func_GetWeightColor3(rVert2._renderWeightByTool);
								}
								vColor0.a = 1.0f;
								vColor1.a = 1.0f;
								vColor2.a = 1.0f;
								break;
						}


						posGL_0 = World2GL(rVert0._pos_World);
						posGL_1 = World2GL(rVert1._pos_World);
						posGL_2 = World2GL(rVert2._pos_World);

						pos_0.x = posGL_0.x;
						pos_0.y = posGL_0.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = posGL_1.x;
						pos_1.y = posGL_1.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = posGL_2.x;
						pos_2.y = posGL_2.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;
						
						uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
						uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
						uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;

						
						GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						// Back Side
						GL.Color(vColor2); GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						GL.Color(vColor1); GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						GL.Color(vColor0); GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
					}



					//------------------------------------------

					//삭제 21.5.18
					//GL.End();//<전환 완료>
					//Clipping Pass마다 Pass 한번씩 종료
					_matBatch.EndPass();
				}

				if (externalRenderTexture == null)
				{
					_matBatch.DeactiveRenderTexture();
				}
				else
				{
					RenderTexture.active = externalRenderTexture;
				}

				

				//3. Child를 렌더링하자
				
				apTransform_Mesh.ClipMeshSet clipMeshSet = null;

				for (int iClip = 0; iClip < nClipMeshes; iClip++)
				{
					clipMeshSet = childClippedSet[iClip];
					if (clipMeshSet == null || clipMeshSet._meshTransform == null)
					{
						continue;
					}
					apMesh clipMesh = clipMeshSet._meshTransform._mesh;
					apRenderUnit clipRenderUnit = clipMeshSet._renderUnit;

					if (clipMesh == null || clipRenderUnit == null) { continue; }
					if (clipRenderUnit._meshTransform == null) { continue; }
					if (!clipRenderUnit._isVisible) { continue; }

					if (clipMesh._indexBuffer.Count < 3)
					{
						continue;
					}

					//추가 12.04 : Extra 옵션 적용
					apTextureData childTextureData = clipMesh.LinkedTextureData;
					if (clipRenderUnit.IsExtraTextureChanged)
					{
						childTextureData = clipRenderUnit.ChangedExtraTextureData;
					}

					//추가 21.2.16 : 선택되지 않은 RenderUnit은 회색으로 표시
					bool isNotEditedGrayColor = false;

					if (editor._exModObjOption_ShowGray &&
						(clipRenderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_NotEdit ||
							clipRenderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_ExRun))
					{
						//선택되지 않은 건 Gray 색상으로 표시하고자 할 때
						isNotEditedGrayColor = true;
					}


					if (isToneColor)
					{
						//Onion ToneColor Clipping
						_matBatch.BeginPass_Clipped_ToneColor(GL.TRIANGLES, _toneColor, childTextureData._image, renderUnit._meshColor2X);
					}
					else if (isNotEditedGrayColor)
					{
						_matBatch.BeginPass_Gray_Clipped(GL.TRIANGLES, clipRenderUnit._meshColor2X, childTextureData._image, renderUnit._meshColor2X);
					}
					else
					{
						//일반 Clipping
						_matBatch.BeginPass_Clipped(GL.TRIANGLES, clipRenderUnit._meshColor2X, childTextureData._image, vertexColorRatio, clipRenderUnit.ShaderType, renderUnit._meshColor2X);
					}
					
					//삭제 21.5.18
					//_matBatch.SetClippingSize(_glScreenClippingSize);
					//GL.Begin(GL.TRIANGLES);


					//------------------------------------------
					for (int i = 0; i < clipMesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= clipMesh._indexBuffer.Count)
						{ break; }

						if (clipMesh._indexBuffer[i + 0] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 1] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 2] >= clipMesh._vertexData.Count)
						{
							break;
						}

						rVert0 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 0]];
						rVert1 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 1]];
						rVert2 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 2]];


						vColor0 = Color.black;
						vColor1 = Color.black;
						vColor2 = Color.black;

						if (isBoneWeightColor)
						{
							if (isBoneColor)
							{
								vColor0 = rVert0._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								vColor1 = rVert1._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
								vColor2 = rVert2._renderColorByTool * (vertexColorRatio) + Color.black * (1.0f - vertexColorRatio);
							}
							else
							{
								vColor0 = _func_GetWeightColor3(rVert0._renderWeightByTool);
								vColor1 = _func_GetWeightColor3(rVert1._renderWeightByTool);
								vColor2 = _func_GetWeightColor3(rVert2._renderWeightByTool);
							}
						}
						else if (isPhyVolumeWeightColor)
						{
							vColor0 = GetWeightGrayscale(rVert0._renderWeightByTool);
							vColor1 = GetWeightGrayscale(rVert1._renderWeightByTool);
							vColor2 = GetWeightGrayscale(rVert2._renderWeightByTool);
						}



						posGL_0 = World2GL(rVert0._pos_World);
						posGL_1 = World2GL(rVert1._pos_World);
						posGL_2 = World2GL(rVert2._pos_World);

						pos_0.x = posGL_0.x;
						pos_0.y = posGL_0.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = posGL_1.x;
						pos_1.y = posGL_1.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = posGL_2.x;
						pos_2.y = posGL_2.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;

						uv_0 = clipMesh._vertexData[clipMesh._indexBuffer[i + 0]]._uv;
						uv_1 = clipMesh._vertexData[clipMesh._indexBuffer[i + 1]]._uv;
						uv_2 = clipMesh._vertexData[clipMesh._indexBuffer[i + 2]]._uv;


						GL.Color(vColor0);	GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0
						GL.Color(vColor1);	GL.TexCoord(uv_1);	GL.Vertex(pos_1); // 1
						GL.Color(vColor2);	GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2

						//Back Side
						GL.Color(vColor2);	GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2
						GL.Color(vColor1);	GL.TexCoord(uv_1);	GL.Vertex(pos_1); // 1
						GL.Color(vColor0);	GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0


					}
					//------------------------------------------------
					
					//삭제 21.5.18
					//GL.End();//<전환 완료> (밑에)

				}

				//Clipping 렌더링 후 Pass 한번 종료
				_matBatch.EndPass();

				//사용했던 RenderTexture를 해제한다.
				_matBatch.ReleaseRenderTexture();
				//_matBatch.DeactiveRenderTexture();

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}




		/// <summary>
		/// Clipping Render의 Mask Texture만 취하는 함수
		/// RTT 후 실제 Texture2D로 굽기 때문에 실시간으로는 사용하기 힘들다.
		/// 클리핑을 하지 않는다.
		/// </summary>
		/// <param name="renderUnit"></param>
		/// <returns></returns>
		public static Texture2D GetMaskTexture_ClippingParent(apRenderUnit renderUnit)
		{
			//렌더링 순서
			//Parent - 기본
			//Parent - Mask
			//(For) Child - Clipped
			//Release RenderMask
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return null;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0) { return null; }

				//변경 22.3.23 [v1.4.0]
				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return null;
				}	


				apMesh mesh = renderUnit._meshTransform._mesh;

				apTextureData linkedTextureData = mesh.LinkedTextureData;

				//추가 12.4 : Extra Option에 의해 Texture가 바귀었을 경우
				if(renderUnit.IsExtraTextureChanged)
				{
					linkedTextureData = renderUnit.ChangedExtraTextureData;
				}

				//if (mesh.LinkedTextureData == null)//이전
				if(linkedTextureData == null)
				{
					return null;
				}
				

				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

				//1. Parent의 기본 렌더링을 하자
				//+2. Parent의 마스크를 렌더링하자
				if (mesh._indexBuffer.Count < 3)
				{
					return null;
				}

				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

				//Pass는 RTT용 Pass 한개만 둔다.
				bool isRenderTexture = true; //<<RTT만 한다.

				//변경 21.5.18
				//클리핑을 안한다면 기존 Pass를 종료한다.
				_matBatch.EndPass();
				_matBatch.BeginPass_Mask(	GL.TRIANGLES, Color.gray, linkedTextureData._image, 0.0f, renderUnit.ShaderType, isRenderTexture, 
											true, new Vector4(0, 0, 1, 1)//<<클리핑을 하지 않는다.
											);
				//_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));//<<클리핑을 하지 않는다.
				//GL.Begin(GL.TRIANGLES);


				Vector2 posGL_0 = Vector2.zero;
				Vector2 posGL_1 = Vector2.zero;
				Vector2 posGL_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;

				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;
				
				//색상은 처음에만
				GL.Color(Color.black); 
				
				//------------------------------------------
				for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
				{

					if (i + 2 >= mesh._indexBuffer.Count)
					{ break; }

					if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
					{
						break;
					}

					rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
					rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
					rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

					posGL_0 = World2GL(rVert0._pos_World);
					posGL_1 = World2GL(rVert1._pos_World);
					posGL_2 = World2GL(rVert2._pos_World);


					pos_0.x = posGL_0.x;
					pos_0.y = posGL_0.y;
					pos_0.z = rVert0._vertex._zDepth * 0.5f;

					pos_1.x = posGL_1.x;
					pos_1.y = posGL_1.y;
					pos_1.z = rVert1._vertex._zDepth * 0.5f;			   

					pos_2.x = posGL_2.x;
					pos_2.y = posGL_2.y;
					pos_2.z = rVert2._vertex._zDepth * 0.5f;			   

					uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
					uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
					uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


					/*GL.Color(Color.black);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
					/*GL.Color(Color.black);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					/*GL.Color(Color.black);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

					// Back Side
					/*GL.Color(Color.black);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
					/*GL.Color(Color.black);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					/*GL.Color(Color.black);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
				}



				//------------------------------------------
				
				//삭제 21.5.18
				//GL.End();//<변환 완료>

				//Clipping Size 복구
				_matBatch.EndPass();
				_matBatch.SetClippingSize(_glScreenClippingSize);




				//Texture2D로 굽자
				Texture2D resultTex = new Texture2D(_matBatch.RenderTex.width, _matBatch.RenderTex.height, TextureFormat.RGBA32, false);
				resultTex.ReadPixels(new Rect(0, 0, _matBatch.RenderTex.width, _matBatch.RenderTex.height), 0, 0);
				resultTex.Apply();

				//사용했던 RenderTexture를 해제한다.
				_matBatch.ReleaseRenderTexture();
				//_matBatch.DeactiveRenderTexture();

				return resultTex;

			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return null;
		}


		/// <summary>
		/// RTT 없이 "이미 구워진 MaskTexture"를 이용해서 Clipping 렌더링을 한다.
		/// 클리핑을 하지 않는다.
		/// </summary>
		/// <param name="renderUnit"></param>
		/// <param name="renderType"></param>
		/// <param name="childClippedSet"></param>
		/// <param name="vertexController"></param>
		/// <param name="select"></param>
		/// <param name="externalRenderTexture"></param>
		public static void DrawRenderUnit_ClippingParent_Renew_WithoutRTT(apRenderUnit renderUnit,
																			List<apTransform_Mesh.ClipMeshSet> childClippedSet,
																			Texture2D maskedTexture
																		)
		{
			//렌더링 순서
			//Parent - 기본
			//Parent - Mask
			//(For) Child - Clipped
			//Release RenderMask
			try
			{
				//0. 메시, 텍스쳐가 없을 때
				if (renderUnit == null || renderUnit._meshTransform == null || renderUnit._meshTransform._mesh == null)
				{
					return;
				}

				//이전
				//if (renderUnit._renderVerts.Count == 0) { return; }

				//변경 22.3.23 [v1.4.0]
				int nRenderVerts = renderUnit._renderVerts != null ? renderUnit._renderVerts.Length : 0;
				if(nRenderVerts == 0)
				{
					return;
				}

				Color textureColor = renderUnit._meshColor2X;
				apMesh mesh = renderUnit._meshTransform._mesh;


				apTextureData linkedTextureData = mesh.LinkedTextureData;

				//추가 12.4 : Extra Option에 의해 Texture가 바귀었을 경우
				if(renderUnit.IsExtraTextureChanged)
				{
					linkedTextureData = renderUnit.ChangedExtraTextureData;
				}

				//if (mesh.LinkedTextureData == null)//이전
				if(linkedTextureData == null)
				{
					return;
				}

				

				int nClipMeshes = childClippedSet.Count;


				//렌더링 방식은 Mesh (with Color) 또는 Vertex / Outline이 있다.

				//1. Parent의 기본 렌더링을 하자
				//+2. Parent의 마스크를 렌더링하자
				if (mesh._indexBuffer.Count < 3)
				{
					return;
				}

				apRenderVertex rVert0 = null, rVert1 = null, rVert2 = null;

				Color vertexChannelColor = Color.black;
				Color vColor0 = Color.black, vColor1 = Color.black, vColor2 = Color.black;


				Vector2 posGL_0 = Vector2.zero;
				Vector2 posGL_1 = Vector2.zero;
				Vector2 posGL_2 = Vector2.zero;

				Vector3 pos_0 = Vector3.zero;
				Vector3 pos_1 = Vector3.zero;
				Vector3 pos_2 = Vector3.zero;

				Vector2 uv_0 = Vector2.zero;
				Vector2 uv_1 = Vector2.zero;
				Vector2 uv_2 = Vector2.zero;

				//RTT 관련 코드는 모두 뺀다. Pass도 한번이고 기본 렌더링

				//변경 21.5.18
				//클리핑을 안한다면 기존의 Pass를 종료시킨다.
				_matBatch.EndPass();
				_matBatch.BeginPass_Texture_VColor(	GL.TRIANGLES, textureColor, linkedTextureData._image, 0.0f, renderUnit.ShaderType, 
													true, new Vector4(0, 0, 1, 1)//<<클리핑을 하지 않는다.
													);
				//_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));//<<클리핑을 하지 않는다.
				//GL.Begin(GL.TRIANGLES);

				//색은 한번만
				GL.Color(Color.black);

				//------------------------------------------
				for (int i = 0; i < mesh._indexBuffer.Count; i += 3)
				{

					if (i + 2 >= mesh._indexBuffer.Count)
					{ break; }

					if (mesh._indexBuffer[i + 0] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 1] >= mesh._vertexData.Count ||
						mesh._indexBuffer[i + 2] >= mesh._vertexData.Count)
					{
						break;
					}

					rVert0 = renderUnit._renderVerts[mesh._indexBuffer[i + 0]];
					rVert1 = renderUnit._renderVerts[mesh._indexBuffer[i + 1]];
					rVert2 = renderUnit._renderVerts[mesh._indexBuffer[i + 2]];

					//vColor0 = Color.black;
					//vColor1 = Color.black;
					//vColor2 = Color.black;

					posGL_0 = World2GL(rVert0._pos_World);
					posGL_1 = World2GL(rVert1._pos_World);
					posGL_2 = World2GL(rVert2._pos_World);

					pos_0.x = posGL_0.x;
					pos_0.y = posGL_0.y;
					pos_0.z = rVert0._vertex._zDepth * 0.5f;

					pos_1.x = posGL_1.x;
					pos_1.y = posGL_1.y;
					pos_1.z = rVert1._vertex._zDepth * 0.5f;

					pos_2.x = posGL_2.x;
					pos_2.y = posGL_2.y;
					pos_2.z = rVert2._vertex._zDepth * 0.5f;
					


					uv_0 = mesh._vertexData[mesh._indexBuffer[i + 0]]._uv;
					uv_1 = mesh._vertexData[mesh._indexBuffer[i + 1]]._uv;
					uv_2 = mesh._vertexData[mesh._indexBuffer[i + 2]]._uv;


					/*GL.Color(vColor0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
					/*GL.Color(vColor1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					/*GL.Color(vColor2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

					// Back Side
					/*GL.Color(vColor2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
					/*GL.Color(vColor1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
					/*GL.Color(vColor0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
				}



				//------------------------------------------
				//삭제 21.5.18
				//GL.End();//<변환 완료>

				//클리핑 사이즈 복구
				_matBatch.EndPass();
				_matBatch.SetClippingSize(_glScreenClippingSize);//<<클리핑을 하지 않는다.


				//3. Child를 렌더링하자. MaskedTexture를 직접 이용
				apTransform_Mesh.ClipMeshSet clipMeshSet = null;
				for (int iClip = 0; iClip < nClipMeshes; iClip++)
				{
					clipMeshSet = childClippedSet[iClip];
					if (clipMeshSet == null || clipMeshSet._meshTransform == null)
					{
						continue;
					}
					apMesh clipMesh = clipMeshSet._meshTransform._mesh;
					apRenderUnit clipRenderUnit = clipMeshSet._renderUnit;

					if (clipMesh == null || clipRenderUnit == null)		{ continue; }
					if (clipRenderUnit._meshTransform == null)			{ continue; }
					if (!clipRenderUnit._isVisible)						{ continue; }

					if (clipMesh._indexBuffer.Count < 3)
					{
						continue;
					}

					//추가 12.4 : Extra Option
					apTextureData clipTextureData = clipMesh.LinkedTextureData;
					if(clipRenderUnit.IsExtraTextureChanged)
					{
						clipTextureData = clipRenderUnit.ChangedExtraTextureData;
					}

					//변경 21.5.18
					//클리핑을 하지 않는다면, 기존 Pass 종료
					_matBatch.EndPass();
					_matBatch.BeginPass_ClippedWithMaskedTexture(GL.TRIANGLES, clipRenderUnit._meshColor2X,
																//clipMesh.LinkedTextureData._image,//이전
																clipTextureData._image,
																0.0f,
																clipRenderUnit.ShaderType,
																renderUnit._meshColor2X,
																maskedTexture,
																new Vector4(0, 0, 1, 1)//<<클리핑을 하지 않는다.
																);

					//_matBatch.SetClippingSize(new Vector4(0, 0, 1, 1));//<<클리핑을 하지 않는다.
					//GL.Begin(GL.TRIANGLES);//삭제

					//색은 한번만
					GL.Color(Color.black);

					//------------------------------------------
					for (int i = 0; i < clipMesh._indexBuffer.Count; i += 3)
					{
						if (i + 2 >= clipMesh._indexBuffer.Count)
						{ break; }

						if (clipMesh._indexBuffer[i + 0] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 1] >= clipMesh._vertexData.Count ||
							clipMesh._indexBuffer[i + 2] >= clipMesh._vertexData.Count)
						{
							break;
						}

						rVert0 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 0]];
						rVert1 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 1]];
						rVert2 = clipRenderUnit._renderVerts[clipMesh._indexBuffer[i + 2]];


						//vColor0 = Color.black;
						//vColor1 = Color.black;
						//vColor2 = Color.black;


						posGL_0 = World2GL(rVert0._pos_World);
						posGL_1 = World2GL(rVert1._pos_World);
						posGL_2 = World2GL(rVert2._pos_World);

						pos_0.x = posGL_0.x;
						pos_0.y = posGL_0.y;
						pos_0.z = rVert0._vertex._zDepth * 0.5f;

						pos_1.x = posGL_1.x;
						pos_1.y = posGL_1.y;
						pos_1.z = rVert1._vertex._zDepth * 0.5f;

						pos_2.x = posGL_2.x;
						pos_2.y = posGL_2.y;
						pos_2.z = rVert2._vertex._zDepth * 0.5f;


						uv_0 = clipMesh._vertexData[clipMesh._indexBuffer[i + 0]]._uv;
						uv_1 = clipMesh._vertexData[clipMesh._indexBuffer[i + 1]]._uv;
						uv_2 = clipMesh._vertexData[clipMesh._indexBuffer[i + 2]]._uv;


						/*GL.Color(vColor0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0
						/*GL.Color(vColor1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(vColor2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2

						//Back Side
						/*GL.Color(vColor2);*/ GL.TexCoord(uv_2); GL.Vertex(pos_2); // 2
						/*GL.Color(vColor1);*/ GL.TexCoord(uv_1); GL.Vertex(pos_1); // 1
						/*GL.Color(vColor0);*/ GL.TexCoord(uv_0); GL.Vertex(pos_0); // 0


					}
					//------------------------------------------------
					//삭제 21.5.18
					//GL.End();//<변환 완료>


					//클리핑 사이즈 복구
					_matBatch.EndPass();
					_matBatch.SetClippingSize(_glScreenClippingSize);//<<클리핑을 하지 않는다.

				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		//------------------------------------------------------------------------------------------------
		// Rigging Vertex (Circle)
		//------------------------------------------------------------------------------------------------
		

		// Rig Circle V2
		/// <summary>
		/// v2 방식의 Rig Circle 버텍스 렌더링 함수. 
		/// 리턴값으로 선택 범위를 리턴한다.
		/// </summary>
		/// <param name="renderVertex"></param>
		/// <param name="isUseBoneColor"></param>
		/// <param name="isVertSelected"></param>
		private static float DrawRiggingRenderVert_V2(apRenderVertex renderVertex, bool isUseBoneColor, bool isVertSelected)
		{
			/*
			_matBatch.SetPass_Color();
			_matBatch.SetClippingSize(_glScreenClippingSize);

			GL.Begin(GL.TRIANGLES);
			*/

			Vector2 posCenterGL = renderVertex._pos_GL;

			
			//colorOutline.a = 1.0f;//<<여기선 반투명 Outline을 지원하지 않는다.

			if (renderVertex._renderRigWeightParam._nParam == 0)
			{
				//데이터가 없는 경우 > 고정 크기의 작은 점
				//이전
				//float size_None = 10.0f;
				//float size_None_Outline = 14.0f;

				//변경 20.3.25 : 별도의 정의 값을 이용
				float size_None_Half = (isVertSelected ? (RIG_CIRCLE_SIZE_NORIG_SELECTED * 0.5f) : (RIG_CIRCLE_SIZE_NORIG * 0.5f));

				Vector2 uv_0 = (isVertSelected ? new Vector2(0.5f, 1.0f) : new Vector2(0.0f, 1.0f));
				Vector2 uv_1 = (isVertSelected ? new Vector2(1.0f, 1.0f) : new Vector2(0.5f, 1.0f));
				Vector2 uv_2 = (isVertSelected ? new Vector2(1.0f, 0.0f) : new Vector2(0.5f, 0.0f));
				Vector2 uv_3 = (isVertSelected ? new Vector2(0.5f, 0.0f) : new Vector2(0.0f, 0.0f));

				Vector2 pos_0 = new Vector2(posCenterGL.x - size_None_Half, posCenterGL.y - size_None_Half);
				Vector2 pos_1 = new Vector2(posCenterGL.x + size_None_Half, posCenterGL.y - size_None_Half);
				Vector2 pos_2 = new Vector2(posCenterGL.x + size_None_Half, posCenterGL.y + size_None_Half);
				Vector2 pos_3 = new Vector2(posCenterGL.x - size_None_Half, posCenterGL.y + size_None_Half);

				//그냥 원형 아이콘
				GL.Color(isVertSelected ? Color.red : Color.white);

				GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0
				GL.TexCoord(uv_1);	GL.Vertex(pos_1); // 1
				GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2

				GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2
				GL.TexCoord(uv_3);	GL.Vertex(pos_3); // 3
				GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0


				return RIG_CIRCLE_SIZE_NORIG_CLICK_SIZE;//선택 범위는 고정값.

			}
			else
			{
				//N개의 데이터

				//Degree 방식으로 단순 증가
				//> -1 * Deg2Rad - (0.5 * PI)
				//UV 중요
				//45도마다 분할
				float prevAngle_Deg = 0.0f;
				float nextAngle_Deg = 0.0f;
				
				//변경 20.3.25 : 옵션으로 정할 수 있다.
				float radius = (isVertSelected ? _rigCircleSize_SelectedVert : _rigCircleSize_NoSelectedVert);
				float radius_SelectedArea = (isVertSelected ? _rigCircleSize_SelectedVert_Enlarged : _rigCircleSize_NoSelectedVert_Enlarged);
				
				//bool isSelectedAreaFlashing = (_rigSelectedWeightGUIType == apEditor.RIG_SELECTED_WEIGHT_GUI_TYPE.Flashing);


				if(_isRigCircleScaledByZoom)
				{
					radius *= _zoom;
					radius_SelectedArea *= _zoom;
				}
				
				//radius = Mathf.Clamp(radius, RIG_CIRCLE_SIZE_DEF * 0.1f, RIG_CIRCLE_SIZE_DEF__SMALL_OUTLINE * 10.0f);
				//radius_SelectedArea = Mathf.Clamp(radius_SelectedArea, RIG_CIRCLE_SIZE_DEF__LARGE_OUTLINE * 0.1f, RIG_CIRCLE_SIZE_DEF__LARGE_OUTLINE * 10.0f);
				
				float curRatio = 0.0f;
				float prevRatio = 0.0f;
				float nextRatio = 0.0f;
				Color curColor = Color.black;
				bool curSelected = false;

				float realAngleDeg_Prev = 0.0f;
				float realAngleDeg_Next = 0.0f;
				//float difAngle = 0.0f;

				bool isAnySelectedBone = false;
				Color selectedAreaColor = Color.black;

				for (int iRig = 0; iRig < renderVertex._renderRigWeightParam._nParam; iRig++)
				{
					curRatio = renderVertex._renderRigWeightParam._ratios[iRig];
					curSelected = renderVertex._renderRigWeightParam._isSelecteds[iRig];

					if (isUseBoneColor)
					{
						curColor = renderVertex._renderRigWeightParam._colors[iRig];
						curColor.a = 1.0f;
					}
					else
					{
						//본 색상을 사용하지 않는 방식
						//이전 방식 : 선택된 영역은 그라디언트, 그외의 영역은 어두운 본 색상
						if (curSelected)
						{
							curColor = _func_GetWeightColor3(curRatio);
						}
						else
						{
							curColor = renderVertex._renderRigWeightParam._colors[iRig];
							curColor *= 0.8f;
							curColor.a = 1.0f;
						}

						//변경 방식 : 20.3.28 : 선택 여부 상관없이 그라디언트. 단, 그 외의 영역은 살짝 반투명(선택된 본에 한해서)
						//curColor = _func_GetWeightColor3(curRatio);
						//if(isVertSelected && !curSelected)
						//{
						//	curColor.a = 0.8f;
						//}
						//else
						//{
						//	curColor.a = 1.0f;
						//}
					}
					nextRatio = prevRatio + curRatio;
					nextAngle_Deg = prevAngle_Deg + (curRatio * 360.0f);


					realAngleDeg_Prev = prevAngle_Deg;
					realAngleDeg_Next = nextAngle_Deg;

					//선택된 본이 있다면
					if(curSelected && isVertSelected)
					{
						isAnySelectedBone = true;
						//if (isUseBoneColor)
						//{
						//	selectedAreaColor = _func_GetWeightColor3(curRatio);
						//}
						//else
						//{
						//	selectedAreaColor = curColor;
						//}
						selectedAreaColor = curColor;
						selectedAreaColor.a = 1.0f;
					}

					if(curSelected && _isRigSelectedWeightArea_Flashing)
					{
						//선택된 본 영역이 반짝거리는 옵션이 켜진 경우
						float flashingLerp = _animRatio_SelectedRigFlashing;
						Color darkColor = curColor * 0.4f;
						Color brightColor = (curColor + new Color(0.1f, 0.1f, 0.1f, 1.0f)) * 2.0f;

						brightColor.r = Mathf.Clamp01(brightColor.r);
						brightColor.g = Mathf.Clamp01(brightColor.g);
						brightColor.b = Mathf.Clamp01(brightColor.b);

						curColor = (darkColor * flashingLerp) + (brightColor * (1.0f - flashingLerp));
						curColor.a = 1.0f;
					}
					
					DrawRigCirclePart_V2(posCenterGL, (curSelected ? radius_SelectedArea : radius), curColor, realAngleDeg_Prev, realAngleDeg_Next, isVertSelected);

					//다음으로 이동
					prevRatio = nextRatio;
					prevAngle_Deg = nextAngle_Deg;

					
				}

				if(isAnySelectedBone && isVertSelected)
				{
					//선택된 본이 있다면 중앙의 원으로 한번 더 보여주자
					float radius_Summary = radius * 0.4f;
					Vector2 uv_0 = new Vector2(0.0f, 1.0f);
					Vector2 uv_1 = new Vector2(0.5f, 1.0f);
					Vector2 uv_2 = new Vector2(0.5f, 0.0f);
					Vector2 uv_3 = new Vector2(0.0f, 0.0f);

					Vector2 pos_0 = new Vector2(posCenterGL.x - radius_Summary, posCenterGL.y - radius_Summary);
					Vector2 pos_1 = new Vector2(posCenterGL.x + radius_Summary, posCenterGL.y - radius_Summary);
					Vector2 pos_2 = new Vector2(posCenterGL.x + radius_Summary, posCenterGL.y + radius_Summary);
					Vector2 pos_3 = new Vector2(posCenterGL.x - radius_Summary, posCenterGL.y + radius_Summary);

					//그냥 박스
					GL.Color(selectedAreaColor);

					GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0
					GL.TexCoord(uv_1);	GL.Vertex(pos_1); // 1
					GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2

					GL.TexCoord(uv_2);	GL.Vertex(pos_2); // 2
					GL.TexCoord(uv_3);	GL.Vertex(pos_3); // 3
					GL.TexCoord(uv_0);	GL.Vertex(pos_0); // 0
				}

				return _isRigCircleScaledByZoom ? (_rigCircleSize_ClickSize_Rigged * _zoom) : (_rigCircleSize_ClickSize_Rigged);
			}


		}


		private static void DrawRigCirclePart_V2(Vector2 posCenterGL, 
												float radius, Color color,
												float angleDeg_Prev, float angleDeg_Next,
												bool isVertSelected)
		{
			//45, 135, 225, 315를 사이에 두면 꼭지점을 추가해야한다.
			int iPart_Prev = 0;
			int iPart_Next = 0;

			//color.a = 1.0f;

			//Vector2 uv_Center = new Vector2(0.5f, 0.5f);
			Vector2 uv_Center = new Vector2((isVertSelected ? 0.75f : 0.25f), 0.5f);

			if(angleDeg_Prev > angleDeg_Next)
			{
				float tmpAngle = angleDeg_Next;
				angleDeg_Next = angleDeg_Prev;
				angleDeg_Prev = tmpAngle;
			}

			if (angleDeg_Prev < 45.0f)			{ iPart_Prev = 0; }
			else if(angleDeg_Prev < 135.0f)		{ iPart_Prev = 1; }
			else if(angleDeg_Prev < 225.0f)		{ iPart_Prev = 2; }
			else if(angleDeg_Prev < 315.0f)		{ iPart_Prev = 3; }
			else								{ iPart_Prev = 4; }

			if (angleDeg_Next < 45.0f)			{ iPart_Next = 0; }
			else if(angleDeg_Next < 135.0f)		{ iPart_Next = 1; }
			else if(angleDeg_Next < 225.0f)		{ iPart_Next = 2; }
			else if(angleDeg_Next < 315.0f)		{ iPart_Next = 3; }
			else								{ iPart_Next = 4; }

			Vector2 pos_Prev = Vector2.zero;
			Vector2 pos_Next = Vector2.zero;
			
			Vector2 uv_Prev = Vector2.zero;
			Vector2 uv_Next = Vector2.zero;
			float uOffset = (isVertSelected ? 0.5f : 0.0f);

			float c2S_Ratio_Prev = 1.0f;
			float c2S_Ratio_Next = 1.0f;

			float angleRad_Prev = 0.0f;
			float angleRad_Next = 0.0f;

			//위치 좌표계 : LT > RB (CCW?)
			
			//UV 좌표계 : LB > RT

			while(true)
			{
				//만약 같은 대각-사분면에 위치한다면 > 바로 삼각형을 만들어서 그리기 > break;
				//그렇지 않다면 > next에 해당 꼭지점과 삼각형을 만들어서 그리기 > 그 꼭지점을 prev로 하여 한번 더 반복. 단, 사분면 증가
				if(iPart_Prev == iPart_Next)
				{
					angleRad_Prev = angleDeg_Prev * Mathf.Deg2Rad;
					angleRad_Next = angleDeg_Next * Mathf.Deg2Rad;

					switch (iPart_Prev)
					{
						
						//위/아래 : Y 고정으로 비율 계산
						case 0:
						case 4:
						case 2:
							c2S_Ratio_Prev = 1.0f / Mathf.Abs(Mathf.Cos(angleRad_Prev));
							c2S_Ratio_Next = 1.0f / Mathf.Abs(Mathf.Cos(angleRad_Next));
							break;

						//좌/우 : X 고정으로 비율 계산
						case 1:
						case 3:
							c2S_Ratio_Prev = 1.0f / Mathf.Abs(Mathf.Sin(angleRad_Prev));
							c2S_Ratio_Next = 1.0f / Mathf.Abs(Mathf.Sin(angleRad_Next));
							break;
					}
					
					//Sin, Cos을 반대로
					pos_Prev.x = posCenterGL.x + (Mathf.Sin(angleRad_Prev) * radius * c2S_Ratio_Prev);
					pos_Prev.y = posCenterGL.y + (-Mathf.Cos(angleRad_Prev) * radius * c2S_Ratio_Prev);

					pos_Next.x = posCenterGL.x + (Mathf.Sin(angleRad_Next) * radius * c2S_Ratio_Next);
					pos_Next.y = posCenterGL.y + (-Mathf.Cos(angleRad_Next) * radius * c2S_Ratio_Next);

					uv_Prev.x = Mathf.Cos(angleRad_Prev) * c2S_Ratio_Prev;
					uv_Prev.y = Mathf.Sin(angleRad_Prev) * c2S_Ratio_Prev;

					uv_Next.x = Mathf.Cos(angleRad_Next) * c2S_Ratio_Next;
					uv_Next.y = Mathf.Sin(angleRad_Next) * c2S_Ratio_Next;

					//uv_Prev.x = (uv_Prev.x * 0.5f) + 0.5f;//0 ~ 1
					uv_Prev.x = ((uv_Prev.x * 0.5f) + 0.5f) * 0.5f + uOffset;//(0 ~ 0.5) or (0.5 ~ 1)
					uv_Prev.y = (uv_Prev.y * -0.5f) + 0.5f;

					//uv_Next.x = (uv_Next.x * 0.5f) + 0.5f;//0 ~ 1
					uv_Next.x = ((uv_Next.x * 0.5f) + 0.5f) * 0.5f + uOffset;//(0 ~ 0.5) or (0.5 ~ 1)
					uv_Next.y = (uv_Next.y * -0.5f) + 0.5f;

					//안쪽
					GL.Color(color);
					
					GL.TexCoord(uv_Center);	GL.Vertex(posCenterGL);
					GL.TexCoord(uv_Prev);	GL.Vertex(pos_Prev);
					GL.TexCoord(uv_Next);	GL.Vertex(pos_Next);
					

					//종료!
					break;
				}
				else
				{
					//일단 Prev의 각도부터
					angleRad_Prev = angleDeg_Prev * Mathf.Deg2Rad;

					switch (iPart_Prev)
					{
						case 0:case 4:case 2:	c2S_Ratio_Prev = 1.0f / Mathf.Abs(Mathf.Cos(angleRad_Prev));	break;
						case 1:case 3:			c2S_Ratio_Prev = 1.0f / Mathf.Abs(Mathf.Sin(angleRad_Prev));	break;
					}

					//Sin, Cos를 반대로
					pos_Prev.x = posCenterGL.x + (Mathf.Sin(angleRad_Prev) * radius * c2S_Ratio_Prev);
					pos_Prev.y = posCenterGL.y + (-Mathf.Cos(angleRad_Prev) * radius * c2S_Ratio_Prev);

					uv_Prev.x = Mathf.Cos(angleRad_Prev) * c2S_Ratio_Prev;
					uv_Prev.y = Mathf.Sin(angleRad_Prev) * c2S_Ratio_Prev;

					//Next의 각도는 꼭지점이다.
					switch (iPart_Prev)
					{
						case 0:
						case 4:
							//45도보다 작은 경우 (Pos : 1, -1 / UV : 1, 1)
							angleRad_Next = 45.0f * Mathf.Deg2Rad;
							break;

						case 1:
							//135도보다 작은 경우 (Pos : 1, 1 / UV : 1, 0)
							angleRad_Next = 135.0f * Mathf.Deg2Rad;
							break;

						case 2:
							//225도보다 작은 경우 (Pos : -1, 1 / UV : 0, 0)
							angleRad_Next = 225.0f * Mathf.Deg2Rad;
							break;

						case 3:
							//315도보다 작은 경우 (Pos : -1, -1 / UV : 0, 1)
							angleRad_Next = 315.0f * Mathf.Deg2Rad;
							break;

					}

					switch (iPart_Prev)
					{
						case 0:case 4:case 2:	c2S_Ratio_Next = 1.0f / Mathf.Abs(Mathf.Cos(angleRad_Next));	break;
						case 1:case 3:			c2S_Ratio_Next = 1.0f / Mathf.Abs(Mathf.Sin(angleRad_Next));	break;
					}
					
					//Sin, Cos를 반대로
					pos_Next.x = posCenterGL.x + (Mathf.Sin(angleRad_Next) * radius * c2S_Ratio_Next);
					pos_Next.y = posCenterGL.y + (-Mathf.Cos(angleRad_Next) * radius * c2S_Ratio_Next);

					uv_Next.x = Mathf.Cos(angleRad_Next) * c2S_Ratio_Next;
					uv_Next.y = Mathf.Sin(angleRad_Next) * c2S_Ratio_Next;

					//uv_Prev.x = (uv_Prev.x * 0.5f) + 0.5f;//0 ~ 1
					uv_Prev.x = ((uv_Prev.x * 0.5f) + 0.5f) * 0.5f + uOffset;//(0 ~ 0.5) or (0.5 ~ 1)
					uv_Prev.y = (uv_Prev.y * -0.5f) + 0.5f;

					//uv_Next.x = (uv_Next.x * 0.5f) + 0.5f;//0 ~ 1
					uv_Next.x = ((uv_Next.x * 0.5f) + 0.5f) * 0.5f + uOffset;//(0 ~ 0.5) or (0.5 ~ 1)
					uv_Next.y = (uv_Next.y * -0.5f) + 0.5f;

					//안쪽
					GL.Color(color);
					
					GL.TexCoord(uv_Center);	GL.Vertex(posCenterGL);
					GL.TexCoord(uv_Prev);	GL.Vertex(pos_Prev);
					GL.TexCoord(uv_Next);	GL.Vertex(pos_Next);
					

					//일부의 렌더링이 끝나고 Prev 꼭지점을 옮긴다.
					switch (iPart_Prev)
					{
						case 0:
							angleDeg_Prev = 45.0f;
							iPart_Prev = 1;
							break;

						case 1:
							angleDeg_Prev = 135.0f;
							iPart_Prev = 2;
							break;

						case 2:
							angleDeg_Prev = 225.0f;
							iPart_Prev = 3;
							break;

						case 3:
							angleDeg_Prev = 315.0f;
							iPart_Prev = 4;
							break;

						case 4:
							return;
					}
				}
			}
		}



		//------------------------------------------------------------------------------------------------
		// Draw Transform Border Form of Render Unit
		//------------------------------------------------------------------------------------------------
		public static void DrawTransformBorderFormOfRenderUnit(Color lineColor, float posL, float posR, float posT, float posB, apMatrix3x3 worldMatrix)
		{
			float marginOffset = 10;
			posL -= marginOffset;
			posR += marginOffset;
			posT += marginOffset;
			posB -= marginOffset;

			//Vector3 pos3W_LT = worldMatrix.MultiplyPoint3x4(new Vector3(posL, posT, 0));
			//Vector3 pos3W_RT = worldMatrix.MultiplyPoint3x4(new Vector3(posR, posT, 0));
			//Vector3 pos3W_LB = worldMatrix.MultiplyPoint3x4(new Vector3(posL, posB, 0));
			//Vector3 pos3W_RB = worldMatrix.MultiplyPoint3x4(new Vector3(posR, posB, 0));

			//Vector2 posW_LT = new Vector2(pos3W_LT.x, pos3W_LT.y);
			//Vector2 posW_RT = new Vector2(pos3W_RT.x, pos3W_RT.y);
			//Vector2 posW_LB = new Vector2(pos3W_LB.x, pos3W_LB.y);
			//Vector2 posW_RB = new Vector2(pos3W_RB.x, pos3W_RB.y);


			Vector2 posW_LT = worldMatrix.MultiplyPoint(new Vector2(posL, posT));
			Vector2 posW_RT = worldMatrix.MultiplyPoint(new Vector2(posR, posT));
			Vector2 posW_LB = worldMatrix.MultiplyPoint(new Vector2(posL, posB));
			Vector2 posW_RB = worldMatrix.MultiplyPoint(new Vector2(posR, posB));


			float tfFormLineLength = 32.0f;

			//변경 21.5.18
			_matBatch.BeginPass_Color(GL.LINES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.LINES);

			DrawLine(posW_LT, GetUnitLineEndPoint(posW_LT, posW_RT, tfFormLineLength), lineColor, false);
			DrawLine(posW_RT, GetUnitLineEndPoint(posW_RT, posW_RB, tfFormLineLength), lineColor, false);
			DrawLine(posW_RB, GetUnitLineEndPoint(posW_RB, posW_LB, tfFormLineLength), lineColor, false);
			DrawLine(posW_LB, GetUnitLineEndPoint(posW_LB, posW_LT, tfFormLineLength), lineColor, false);

			DrawLine(posW_LT, GetUnitLineEndPoint(posW_LT, posW_LB, tfFormLineLength), lineColor, false);
			DrawLine(posW_LB, GetUnitLineEndPoint(posW_LB, posW_RB, tfFormLineLength), lineColor, false);
			DrawLine(posW_RB, GetUnitLineEndPoint(posW_RB, posW_RT, tfFormLineLength), lineColor, false);
			DrawLine(posW_RT, GetUnitLineEndPoint(posW_RT, posW_LT, tfFormLineLength), lineColor, false);

			//삭제 21.5.18
			//GL.End();//<변환 완료>
			_matBatch.EndPass();

		}

		private static Vector2 GetUnitLineEndPoint(Vector2 startPos, Vector2 endPos, float maxLength)
		{
			Vector2 dir = endPos - startPos;
			if (dir.sqrMagnitude <= maxLength * maxLength)
			{
				return endPos;
			}
			return startPos + dir.normalized * maxLength;
		}


		//------------------------------------------------------------------------------------------------
		// Draw Bone
		//------------------------------------------------------------------------------------------------

		//	본 그리기 > 버전 1 (화살촉 형태)
		public static void DrawBone_V1(apBone bone, bool isDrawOutline, bool isBoneIKUsing, bool isUseBoneToneColor, bool isAvailable)
		{
			if (bone == null)
			{
				return;
			}

			
			Color boneColor = bone._color;
			if(isUseBoneToneColor)
			{
				boneColor = _toneBoneColor;
			}
			else if(!isAvailable)
			{
				//추가 : 사용 불가능하다면 회색으로 보인다.
				boneColor = Color.gray;
			}

			Color boneOutlineColor = boneColor * 0.5f;
			boneOutlineColor.a = 1.0f;

			
			apMatrix worldMatrix = null;//이전 > 다시 이거 20.8.23
			//apBoneWorldMatrix worldMatrix = null;//변경 20.8.12 : CompleMatrix 방식 > BoneWorldMatrix

			Vector2 posW_Start = Vector2.zero;

			bool isHelperBone = bone._shapeHelper;
			Vector2 posGL_Start = Vector2.zero;
			Vector2 posGL_Mid1 = Vector2.zero;
			Vector2 posGL_Mid2 = Vector2.zero;
			Vector2 posGL_End1 = Vector2.zero;
			Vector2 posGL_End2 = Vector2.zero;

			if(isBoneIKUsing)
			{
				//IK 값이 포함될 때
				//worldMatrix = bone._worldMatrix_IK;
				//posW_Start = worldMatrix.Pos;

				//GUIMatrix로 변경 (20.8.23)
				worldMatrix = bone._guiMatrix_IK;
				posW_Start = worldMatrix._pos;

				if(!isUseBoneToneColor)
				{
					posGL_Start = apGL.World2GL(posW_Start);
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_IK.Mid1);
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_IK.Mid2);
					posGL_End1 = apGL.World2GL(bone._shapePoints_V1_IK.End1);
					posGL_End2 = apGL.World2GL(bone._shapePoints_V1_IK.End2);
				}
				else
				{
					//Onion Skin 전용 좌표 계산
					Vector2 deltaOnionPos = apGL._tonePosOffset * apGL._zoom;
					//Vector2 deltaOnionPos = apGL._tonePosOffset * 0.001f;
					//Vector2 deltaOnionPos = apGL._tonePosOffset;
					
					posGL_Start = apGL.World2GL(posW_Start) + deltaOnionPos;
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_IK.Mid1) + deltaOnionPos;
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_IK.Mid2) + deltaOnionPos;
					posGL_End1 = apGL.World2GL(bone._shapePoints_V1_IK.End1) + deltaOnionPos;
					posGL_End2 = apGL.World2GL(bone._shapePoints_V1_IK.End2) + deltaOnionPos;
				}
				
			}
			else
			{
				//worldMatrix = bone._worldMatrix;
				//posW_Start = worldMatrix.Pos;

				//GUIMatrix로 변경 (20.8.23)
				worldMatrix = bone._guiMatrix;
				posW_Start = worldMatrix._pos;

				if (!isUseBoneToneColor)
				{
					posGL_Start = apGL.World2GL(posW_Start);
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid1);
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid2);
					posGL_End1 = apGL.World2GL(bone._shapePoints_V1_Normal.End1);
					posGL_End2 = apGL.World2GL(bone._shapePoints_V1_Normal.End2);
				}
				else
				{
					//Onion Skin 전용 좌표 계산
					Vector2 deltaOnionPos = apGL._tonePosOffset * apGL._zoom;
					//Vector2 deltaOnionPos = apGL._tonePosOffset * 0.001f;
					//Vector2 deltaOnionPos = apGL._tonePosOffset;

					posGL_Start = apGL.World2GL(posW_Start) + deltaOnionPos;
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid1) + deltaOnionPos;
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid2) + deltaOnionPos;
					posGL_End1 = apGL.World2GL(bone._shapePoints_V1_Normal.End1) + deltaOnionPos;
					posGL_End2 = apGL.World2GL(bone._shapePoints_V1_Normal.End2) + deltaOnionPos;
				}
				
			}

			

			//float orgSize = 10.0f * Zoom;
			//float orgSize = bone._shapePoints_V1_Normal.Radius * Zoom;
			float orgSize = apBone.RenderSetting_V1_Radius_Org * Zoom;
			Vector3 orgPos_Up = new Vector3(posGL_Start.x, posGL_Start.y + orgSize, 0);
			Vector3 orgPos_Left = new Vector3(posGL_Start.x - orgSize, posGL_Start.y, 0);
			Vector3 orgPos_Down = new Vector3(posGL_Start.x, posGL_Start.y - orgSize, 0);
			Vector3 orgPos_Right = new Vector3(posGL_Start.x + orgSize, posGL_Start.y, 0);

			if (!isDrawOutline)
			{
				//1. 전부다 그릴때
				//_matBatch.SetPass_Color();
				//_matBatch.SetClippingSize(_glScreenClippingSize);

				if (!isHelperBone)//<헬퍼가 아닐때
				{
					//GL.Begin(GL.TRIANGLES);

					//변경 5.18
					_matBatch.BeginPass_Color(GL.TRIANGLES);

					GL.Color(boneColor);

					//1. 사다리꼴 모양을 먼저 그리자
					//    [End1]    [End2]
					//
					//
					//
					//[Mid1]            [Mid2]
					//        [Start]

					//1) Start - Mid1 - End1
					//2) Start - Mid2 - End2
					//3) Start - End1 - End2

					//1) Start - Mid1 - End1
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_Mid1);
					GL.Vertex(posGL_End1);
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_End1);
					GL.Vertex(posGL_Mid1);

					//2) Start - Mid2 - End2
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_Mid2);
					GL.Vertex(posGL_End2);
					GL.Vertex(posGL_Start);
					GL.Vertex(posGL_End2);
					GL.Vertex(posGL_Mid2);

					//3) Start - End1 - End2 (taper가 100 미만일 때)
					if (bone._shapeTaper < 100)
					{
						GL.Vertex(posGL_Start);
						GL.Vertex(posGL_End1);
						GL.Vertex(posGL_End2);
						GL.Vertex(posGL_Start);
						GL.Vertex(posGL_End2);
						GL.Vertex(posGL_End1);
					}
					
					//삭제 21.5.18
					//GL.End();//<나중에 일괄 EndPass>
					//GL.Begin(GL.LINES);


					//변경 5.18
					_matBatch.BeginPass_Color(GL.LINES);

					DrawLineGL(posGL_Start, posGL_Mid1, boneOutlineColor, false);
					DrawLineGL(posGL_Mid1, posGL_End1, boneOutlineColor, false);
					DrawLineGL(posGL_End1, posGL_End2, boneOutlineColor, false);
					DrawLineGL(posGL_End2, posGL_Mid2, boneOutlineColor, false);
					DrawLineGL(posGL_Mid2, posGL_Start, boneOutlineColor, false);

					//삭제
					//GL.End();//<나중에 일괄 EndPass>
				}

				//삭제 21.5.18
				//GL.Begin(GL.TRIANGLES);

				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.TRIANGLES);

				GL.Color(boneColor);

				//2. 원점 부분은 다각형 형태로 다시 그려주자
				//다이아몬드 형태로..



				//       Up
				// Left  |   Right
				//      Down

				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Left);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Left);

				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Right);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Up);
				GL.Vertex(orgPos_Down);
				GL.Vertex(orgPos_Right);

				//삭제
				//GL.End();//<나중에 일괄 EndPass>
				//GL.Begin(GL.LINES);

				//qusrud 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);

				DrawLineGL(orgPos_Up, orgPos_Left, boneOutlineColor, false);
				DrawLineGL(orgPos_Left, orgPos_Down, boneOutlineColor, false);
				DrawLineGL(orgPos_Down, orgPos_Right, boneOutlineColor, false);
				DrawLineGL(orgPos_Right, orgPos_Up, boneOutlineColor, false);

				//삭제
				//GL.End();//<나중에 일괄 EndPass>
			}
			else
			{
				//변경 21.5.18
				_matBatch.BeginPass_Color(GL.LINES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);

				//2. Outline만 그릴때
				//1> 헬퍼가 아니라면 사다리꼴만
				//2> 헬퍼라면 다이아몬드만
				//GL.Begin(GL.LINES);
				if (!isHelperBone)
				{
					DrawLineGL(posGL_Start, posGL_Mid1, boneColor, false);
					DrawLineGL(posGL_Mid1, posGL_End1, boneColor, false);
					DrawLineGL(posGL_End1, posGL_End2, boneColor, false);
					DrawLineGL(posGL_End2, posGL_Mid2, boneColor, false);
					DrawLineGL(posGL_Mid2, posGL_Start, boneColor, false);
				}
				else
				{
					DrawLineGL(orgPos_Up, orgPos_Left, boneColor, false);
					DrawLineGL(orgPos_Left, orgPos_Down, boneColor, false);
					DrawLineGL(orgPos_Down, orgPos_Right, boneColor, false);
					DrawLineGL(orgPos_Right, orgPos_Up, boneColor, false);
				}

				//삭제
				//GL.End();//<나중에 일괄 EndPass>
			}
		}

		//추가 20.5.29 : 선택된 본의 색상 방식
		public enum BONE_SELECTED_OUTLINE_COLOR
		{
			MainSelected, SubSelected, LinkTarget
		}

		//색상간의 차이를 계산해서 리턴한다. 이 값에 따라서 Default/Reverse
		private static float GetColorDif(Color colorA, Color colorB)
		{
			float diff_R = Mathf.Abs(colorA.r - colorB.r);
			float diff_G = Mathf.Abs(colorA.g - colorB.g);
			float diff_B = Mathf.Abs(colorA.b - colorB.b);
			if(diff_R > 0.5f) { diff_R -= 0.5f; }
			if(diff_G > 0.5f) { diff_G -= 0.5f; }
			if(diff_B > 0.5f) { diff_B -= 0.5f; }
			return ((diff_R * 0.3f) + (diff_G * 0.6f) + (diff_B * 0.1f)) * 2.0f;
		}

		public static void DrawSelectedBone_V1(apBone bone, BONE_SELECTED_OUTLINE_COLOR outlineColor, bool isBoneIKUsing = false)
		{
			//TODO : isMainSelect > 3개의 Enum으로 바꾸자 (메인 선택 색상, 서브 선택 생상, Link시 마우스 롤 오버 색상)
			if (bone == null)
			{
				return;
			}

			apMatrix worldMatrix = null;//이전 > 다시 이걸로 변경 (20.8.23)
			//apBoneWorldMatrix worldMatrix = null;//변경 20.8.12 : CompleMatrix 방식 > BoneWorldMatrix

			Vector2 posW_Start = Vector2.zero;

			bool isHelperBone = bone._shapeHelper;
			Vector2 posGL_Start = Vector2.zero;
			Vector2 posGL_Mid1 = Vector2.zero;
			Vector2 posGL_Mid2 = Vector2.zero;
			Vector2 posGL_End1 = Vector2.zero;
			Vector2 posGL_End2 = Vector2.zero;
			
			if(isBoneIKUsing)
			{
				//worldMatrix = bone._worldMatrix_IK;
				//posW_Start = worldMatrix.Pos;

				//GUIMatrix로 변경 (20.8.23)
				worldMatrix = bone._guiMatrix_IK;
				posW_Start = worldMatrix._pos;

				posGL_Start = apGL.World2GL(posW_Start);
				posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_IK.Mid1);
				posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_IK.Mid2);
				posGL_End1 = apGL.World2GL(bone._shapePoints_V1_IK.End1);
				posGL_End2 = apGL.World2GL(bone._shapePoints_V1_IK.End2);
			}
			else
			{
				//worldMatrix = bone._worldMatrix;
				//posW_Start = worldMatrix.Pos;

				//GUIMatrix로 변경 (20.8.23)
				worldMatrix = bone._guiMatrix;
				posW_Start = worldMatrix._pos;
			
				posGL_Start = apGL.World2GL(posW_Start);
				posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid1);
				posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid2);
				posGL_End1 = apGL.World2GL(bone._shapePoints_V1_Normal.End1);
				posGL_End2 = apGL.World2GL(bone._shapePoints_V1_Normal.End2);
			}



			//2. 원점 부분은 다각형 형태로 다시 그려주자
			//다이아몬드 형태로..
			//float orgSize = 10.0f * Zoom;
			//float orgSize = bone._shapePoints_V1_Normal.Radius * Zoom;
			float orgSize = apBone.RenderSetting_V1_Radius_Org * Zoom;
			Vector3 orgPos_Up = new Vector3(posGL_Start.x, posGL_Start.y + orgSize, 0);
			Vector3 orgPos_Left = new Vector3(posGL_Start.x - orgSize, posGL_Start.y, 0);
			Vector3 orgPos_Down = new Vector3(posGL_Start.x, posGL_Start.y - orgSize, 0);
			Vector3 orgPos_Right = new Vector3(posGL_Start.x + orgSize, posGL_Start.y, 0);

			//변경 21.5.18
			_matBatch.BeginPass_Color(GL.TRIANGLES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);

			Color lineColor;

			//이전
			//float boneColorBrightness = (bone._color.r * 0.3f + bone._color.g * 0.6f + bone._color.b * 0.1f);
			
			//if(outlineColor == BONE_SELECTED_OUTLINE_COLOR.MainSelected)
			//{
			//	//Bone 색상과 너무 닮았다면, Reverse 색상을 적용한다.
			//	if ((Mathf.Abs(bone._color.r - _lineColor_BoneOutline_V1_Default.r) > COLOR_SIMILAR_BIAS ||
			//		Mathf.Abs(bone._color.g - _lineColor_BoneOutline_V1_Default.g) > COLOR_SIMILAR_BIAS ||
			//		Mathf.Abs(bone._color.b - _lineColor_BoneOutline_V1_Default.b) > COLOR_SIMILAR_BIAS)
			//		&& boneColorBrightness > BRIGHTNESS_OUTLINE)
			//	{
			//		//RGB에서 하나라도 차이가 좀 크다 > Default
			//		lineColor = _lineColor_BoneOutline_V1_Default;
			//	}
			//	else
			//	{
			//		//RGB 모든 채널에서 색상 차이가 비슷하다 > Reverse
			//		lineColor = _lineColor_BoneOutline_V1_Reverse;
			//	}
			//}
			//else
			//{
			//	//추가 20.3.23 : Bone 색상과 너무 닮았다면, Reverse 색상을 적용한다.
			//	if((Mathf.Abs(bone._color.r - _lineColor_BoneOutlineRollOver_V1_Default.r) > COLOR_SIMILAR_BIAS ||
			//		Mathf.Abs(bone._color.g - _lineColor_BoneOutlineRollOver_V1_Default.g) > COLOR_SIMILAR_BIAS ||
			//		Mathf.Abs(bone._color.b - _lineColor_BoneOutlineRollOver_V1_Default.b) > COLOR_SIMILAR_BIAS)
			//		&& boneColorBrightness > BRIGHTNESS_OUTLINE)
			//	{
			//		lineColor = _lineColor_BoneOutlineRollOver_V1_Default;
			//	}
			//	else
			//	{
			//		lineColor = _lineColor_BoneOutlineRollOver_V1_Reverse;
			//	}
			//}


			//변경 20.5.29
			Color lineColor_Default = Color.black;
			Color lineColor_Reserve = Color.black;

			switch (outlineColor)
			{
				case BONE_SELECTED_OUTLINE_COLOR.MainSelected:
				case BONE_SELECTED_OUTLINE_COLOR.SubSelected:
					lineColor_Default = _lineColor_BoneOutline_V1_Default;
					lineColor_Reserve = _lineColor_BoneOutline_V1_Reverse;
					break;

				case BONE_SELECTED_OUTLINE_COLOR.LinkTarget:
					lineColor_Default = _lineColor_BoneOutlineRollOver_V1_Default;
					lineColor_Reserve = _lineColor_BoneOutlineRollOver_V1_Reverse;
					break;
			}

			float diff_Default = GetColorDif(bone._color, lineColor_Default);
			//float diff_Reverse = GetColorDif(bone._color, lineColor_Reserve);

			//if(diff_Default * COLOR_DEFAULT_BIAS > diff_Reverse)
			if(diff_Default > COLOR_SIMILAR_BIAS)
			{
				//Default 색상이 더 차이가 크다. (다른 색상을 이용해야함)
				lineColor = lineColor_Default;
			}
			else
			{
				//Reserve 색상이 더 가깝다.
				lineColor = lineColor_Reserve;
			}


			lineColor.a = _animRatio_BoneOutlineAlpha;


			float lineThickness = 8.0f;

			if (!isHelperBone)
			{
				//헬퍼가 아닐때
				//1. 사다리꼴 모양을 먼저 그리자
				//    [End1]    [End2]
				//
				//
				//
				//[Mid1]            [Mid2]
				//        [Start]

				//1) Start - Mid1 - End1
				//2) Start - Mid2 - End2
				//3) Start - End1 - End2

				//1) Start - Mid1 - End1
				

				
				DrawBoldLineGL(posGL_Start, posGL_Mid1, lineThickness, lineColor, false);
				DrawBoldLineGL(posGL_Mid1, posGL_End1, lineThickness, lineColor, false);

				if (bone._shapeTaper < 100)
				{
					DrawBoldLineGL(posGL_End1, posGL_End2, lineThickness, lineColor, false);
				}
				DrawBoldLineGL(posGL_End2, posGL_Mid2, lineThickness, lineColor, false);
				DrawBoldLineGL(posGL_Mid2, posGL_Start, lineThickness, lineColor, false);
			}
			DrawBoldLineGL(orgPos_Up, orgPos_Left, lineThickness, lineColor, false);
			DrawBoldLineGL(orgPos_Left, orgPos_Down, lineThickness, lineColor, false);
			DrawBoldLineGL(orgPos_Down, orgPos_Right, lineThickness, lineColor, false);
			DrawBoldLineGL(orgPos_Right, orgPos_Up, lineThickness, lineColor, false);

			//삭제 21.5.18
			//GL.End();//<나중에 일괄 EndPass>

			//추가 : IK 속성이 있는 경우, GUI에 표시하자
			if (bone._IKTargetBone != null)
			{
				Vector2 IKTargetPos = Vector2.zero;
				if (isBoneIKUsing)
				{
					IKTargetPos = World2GL(bone._IKTargetBone._worldMatrix_IK.Pos);
				}
				else
				{
					IKTargetPos = World2GL(bone._IKTargetBone._worldMatrix.Pos);
				}
				DrawAnimatedLineGL(posGL_Start, IKTargetPos, Color.magenta, true);
			}

			if (bone._IKHeaderBone != null)
			{
				Vector2 IKHeadPos = Vector2.zero;
				if (isBoneIKUsing)
				{
					IKHeadPos = World2GL(bone._IKHeaderBone._worldMatrix_IK.Pos);
				}
				else
				{
					IKHeadPos = World2GL(bone._IKHeaderBone._worldMatrix.Pos);
				}
				DrawAnimatedLineGL(IKHeadPos, posGL_Start, Color.magenta, true);
			}

			if(bone._IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None)
			{
				if(bone._IKController._effectorBone != null)
				{
					Color lineColorIK = Color.yellow;
					if(bone._IKController._controllerType == apBoneIKController.CONTROLLER_TYPE.LookAt)
					{
						lineColorIK = Color.cyan;
					}
					Vector2 effectorPos = Vector2.zero;
					if(isBoneIKUsing)
					{
						effectorPos = World2GL(bone._IKController._effectorBone._worldMatrix_IK.Pos);
					}
					else
					{
						effectorPos = World2GL(bone._IKController._effectorBone._worldMatrix.Pos);
					}
					DrawAnimatedLineGL(posGL_Start, effectorPos, lineColorIK, true);
				}
			}
		}


		



		public static void DrawBoneOutline_V1(apBone bone, Color outlineColor, bool isBoneIKUsing)
		{
			if (bone == null)
			{
				return;
			}

			
			apMatrix worldMatrix = null;//이전 > 다시 이걸로 변경 20.8.23
			//apBoneWorldMatrix worldMatrix = null;//변경 20.8.12 : CompleMatrix 방식


			Vector2 posW_Start = Vector2.zero;
			bool isHelperBone = bone._shapeHelper;

			Vector2 posGL_Start = Vector2.zero;
			Vector2 posGL_Mid1 = Vector2.zero;
			Vector2 posGL_Mid2 = Vector2.zero;
			Vector2 posGL_End1 = Vector2.zero;
			Vector2 posGL_End2 = Vector2.zero;

			if(isBoneIKUsing)
			{
				//worldMatrix = bone._worldMatrix_IK;
				//posW_Start = worldMatrix.Pos;
				
				//GUI Matrix로 변경 (20.8.23)
				worldMatrix = bone._guiMatrix_IK;
				posW_Start = worldMatrix._pos;

				posGL_Start = apGL.World2GL(posW_Start);
				posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_IK.Mid1);
				posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_IK.Mid2);
				posGL_End1 = apGL.World2GL(bone._shapePoints_V1_IK.End1);
				posGL_End2 = apGL.World2GL(bone._shapePoints_V1_IK.End2);
			}
			else
			{
				//worldMatrix = bone._worldMatrix;
				//posW_Start = worldMatrix.Pos;

				//GUI Matrix로 변경 (20.8.23)
				worldMatrix = bone._guiMatrix;
				posW_Start = worldMatrix._pos;

				posGL_Start = apGL.World2GL(posW_Start);
				posGL_Mid1 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid1);
				posGL_Mid2 = apGL.World2GL(bone._shapePoints_V1_Normal.Mid2);
				posGL_End1 = apGL.World2GL(bone._shapePoints_V1_Normal.End1);
				posGL_End2 = apGL.World2GL(bone._shapePoints_V1_Normal.End2);
			}

			//float orgSize = 10.0f * Zoom;
			//float orgSize = bone._shapePoints_V1_Normal.Radius * Zoom;
			float orgSize = apBone.RenderSetting_V1_Radius_Org * Zoom;
			Vector3 orgPos_Up = new Vector3(posGL_Start.x, posGL_Start.y + orgSize, 0);
			Vector3 orgPos_Left = new Vector3(posGL_Start.x - orgSize, posGL_Start.y, 0);
			Vector3 orgPos_Down = new Vector3(posGL_Start.x, posGL_Start.y - orgSize, 0);
			Vector3 orgPos_Right = new Vector3(posGL_Start.x + orgSize, posGL_Start.y, 0);

			//변경 21.5.18
			_matBatch.BeginPass_Color(GL.TRIANGLES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);
			//GL.Begin(GL.TRIANGLES);


			//2. Outline만 그릴때
			//1> 헬퍼가 아니라면 사다리꼴만
			//2> 헬퍼라면 다이아몬드만
			float width = 3.0f;
			
			if (!isHelperBone)
			{
				DrawBoldLineGL(posGL_Start, posGL_Mid1, width, outlineColor, false);
				DrawBoldLineGL(posGL_Mid1, posGL_End1, width, outlineColor, false);
				if (Mathf.Abs(posGL_End1.x - posGL_End2.x) > 2f
					&& Mathf.Abs(posGL_End1.y - posGL_End2.y) > 2f)
				{
					DrawBoldLineGL(posGL_End1, posGL_End2, width, outlineColor, false);
				}
				DrawBoldLineGL(posGL_End2, posGL_Mid2, width, outlineColor, false);
				DrawBoldLineGL(posGL_Mid2, posGL_Start, width, outlineColor, false);
			}
			else
			{
				DrawBoldLineGL(orgPos_Up, orgPos_Left, width, outlineColor, false);
				DrawBoldLineGL(orgPos_Left, orgPos_Down, width, outlineColor, false);
				DrawBoldLineGL(orgPos_Down, orgPos_Right, width, outlineColor, false);
				DrawBoldLineGL(orgPos_Right, orgPos_Up, width, outlineColor, false);
			}

			//삭제 21.5.18
			//GL.End();//<나중에 일괄 EndPass>
		}


		public static void DrawBone_Virtual_V1(Vector2 startPosW,
												Vector2 endPosW,
												Color boneColor,
												Color outlineColor,
												int shapeWidth,
												int shapeTaper)
		{
			float length = (endPosW - startPosW).magnitude;
			float angle = 0.0f;

			if (length > 0.0f)
			{
				angle = Mathf.Atan2(endPosW.y - startPosW.y, endPosW.x - startPosW.x) * Mathf.Rad2Deg;
				angle += 90.0f;
			}

			angle += 180.0f;
			angle = apUtil.AngleTo180(angle);

			if (_cal_TmpMatrix == null)
			{
				_cal_TmpMatrix = new apMatrix();
			}
			_cal_TmpMatrix.SetIdentity();
			_cal_TmpMatrix.SetTRS(startPosW, angle, Vector2.one, true);

			//본 계산은 apBone의 GUIUpdate 중 V1 관련 코드를 기반으로 한다.

			float boneWidth = shapeWidth * apBone.RenderSetting_ScaleRatio;
			if (!apBone.RenderSetting_IsScaledByZoom)
			{
				boneWidth /= apBone.RenderSetting_WorkspaceZoom;
			}
			float boneRadius = boneWidth * 0.5f;
			float taperRatio = Mathf.Clamp01((float)(100 - shapeTaper) / 100.0f);

			Vector2 bonePos_End1 = apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(-boneRadius * taperRatio, length)));
			Vector2 bonePos_End2 = apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(boneRadius * taperRatio, length)));
			Vector2 bonePos_Mid1 = apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(-boneRadius, length * 0.2f)));
			Vector2 bonePos_Mid2 = apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(boneRadius, length * 0.2f)));
			Vector2 bonePos_Start = apGL.World2GL(startPosW);

			//float orgSize = 10.0f * Zoom;
			//float orgSize = bone._shapePoints_V1_Normal.Radius * Zoom;
			float orgSize = apBone.RenderSetting_V1_Radius_Org * Zoom;
			Vector3 orgPos_Up = new Vector3(bonePos_Start.x, bonePos_Start.y + orgSize, 0);
			Vector3 orgPos_Left = new Vector3(bonePos_Start.x - orgSize, bonePos_Start.y, 0);
			Vector3 orgPos_Down = new Vector3(bonePos_Start.x, bonePos_Start.y - orgSize, 0);
			Vector3 orgPos_Right = new Vector3(bonePos_Start.x + orgSize, bonePos_Start.y, 0);



			_matBatch.BeginPass_Color(GL.TRIANGLES);

			GL.Color(boneColor);

			//1. 사다리꼴 모양을 먼저 그리자
			//    [End1]    [End2]
			//
			//
			//
			//[Mid1]            [Mid2]
			//        [Start]

			//1) Start - Mid1 - End1
			//2) Start - Mid2 - End2
			//3) Start - End1 - End2

			//1) Start - Mid1 - End1
			GL.Vertex(bonePos_Start);
			GL.Vertex(bonePos_Mid1);
			GL.Vertex(bonePos_End1);
			GL.Vertex(bonePos_Start);
			GL.Vertex(bonePos_End1);
			GL.Vertex(bonePos_Mid1);

			//2) Start - Mid2 - End2
			GL.Vertex(bonePos_Start);
			GL.Vertex(bonePos_Mid2);
			GL.Vertex(bonePos_End2);
			GL.Vertex(bonePos_Start);
			GL.Vertex(bonePos_End2);
			GL.Vertex(bonePos_Mid2);

			//3) Start - End1 - End2 (taper가 100 미만일 때)
			if (shapeTaper < 100)
			{
				GL.Vertex(bonePos_Start);
				GL.Vertex(bonePos_End1);
				GL.Vertex(bonePos_End2);
				GL.Vertex(bonePos_Start);
				GL.Vertex(bonePos_End2);
				GL.Vertex(bonePos_End1);
			}


			//변경 5.18
			_matBatch.BeginPass_Color(GL.LINES);

			DrawLineGL(bonePos_Start,	bonePos_Mid1,	outlineColor, false);
			DrawLineGL(bonePos_Mid1,	bonePos_End1,	outlineColor, false);
			DrawLineGL(bonePos_End1,	bonePos_End2,	outlineColor, false);
			DrawLineGL(bonePos_End2,	bonePos_Mid2,	outlineColor, false);
			DrawLineGL(bonePos_Mid2,	bonePos_Start,	outlineColor, false);

			
			//2. 원점 부분은 다각형 형태로 다시 그려주자
			_matBatch.BeginPass_Color(GL.TRIANGLES);

			GL.Color(boneColor);
			
			//다이아몬드 형태로..
			//       Up
			// Left  |   Right
			//      Down

			GL.Vertex(orgPos_Up);
			GL.Vertex(orgPos_Left);
			GL.Vertex(orgPos_Down);
			GL.Vertex(orgPos_Up);
			GL.Vertex(orgPos_Down);
			GL.Vertex(orgPos_Left);

			GL.Vertex(orgPos_Up);
			GL.Vertex(orgPos_Right);
			GL.Vertex(orgPos_Down);
			GL.Vertex(orgPos_Up);
			GL.Vertex(orgPos_Down);
			GL.Vertex(orgPos_Right);

			
			
			_matBatch.BeginPass_Color(GL.LINES);

			DrawLineGL(orgPos_Up, orgPos_Left,		outlineColor, false);
			DrawLineGL(orgPos_Left, orgPos_Down,	outlineColor, false);
			DrawLineGL(orgPos_Down, orgPos_Right,	outlineColor, false);
			DrawLineGL(orgPos_Right, orgPos_Up,		outlineColor, false);

			_matBatch.EndPass();
		}


		public static void DrawSelectedBonePost(apBone bone, bool isBoneIKUsing)
		{
			if (bone == null)
			{
				return;
			}

			//여기서 IK 범위 / 지글본 범위를 그린다.
			//조건에 맞지 않으면 return
			bool isDraw_IK = false;
			bool isDraw_Jiggle = false;
			
			if (bone._isIKAngleRange 
				&& bone._optionIK != apBone.OPTION_IK.Disabled)
			{
				//IK 범위를 그리는 경우
				isDraw_IK = true;
			}
			if(bone._isJiggle && bone._isJiggleAngleConstraint)
			{
				isDraw_Jiggle = true;
			}


			if(!isDraw_IK && !isDraw_Jiggle)
			{
				//그릴게 읍당
				return;
			}


			Vector2 posW_Start = Vector2.zero;
			
			//apMatrix worldMatrix = null;//이전
			apBoneWorldMatrix worldMatrix = null;//변경 20.8.12 : CompleMatrix 방식
			
			
			if(isBoneIKUsing)
			{
				worldMatrix = bone._worldMatrix_IK;
			}
			else
			{
				worldMatrix = bone._worldMatrix;
			}
			posW_Start = worldMatrix.Pos;

			Vector2 unitVector = new Vector2(0, 1);
			if (bone._parentBone != null)
			{
				//이전 방식
				//if (isBoneIKUsing)
				//{
				//	unitVector = bone._parentBone._worldMatrix_IK.MtrxOnlyRotation.MultiplyPoint(new Vector2(0, 1));
				//}
				//else
				//{
				//	unitVector = bone._parentBone._worldMatrix.MtrxOnlyRotation.MultiplyPoint(new Vector2(0, 1));
				//}

				//변경 20.8.12 : ComplexMatrix에 MtrxOnlyRotation이 없으므로 다르게 계산한다.
				if (isBoneIKUsing)
				{
					unitVector = bone._parentBone._worldMatrix_IK.MulPoint2(new Vector2(0, 1)) - bone._parentBone._worldMatrix_IK.Pos;
				}
				else
				{
					unitVector = bone._parentBone._worldMatrix.MulPoint2(new Vector2(0, 1)) - bone._parentBone._worldMatrix.Pos;
				}
			}

			float defaultAngle = bone._defaultMatrix._angleDeg;
			if(bone._renderUnit != null 
				&& bone._parentBone == null//버그 수정 v1.4.2 : 부모 본이 있다면 이미 부모 본이 회전하였으므로 그냥 따라가면 된다.
				)
			{
				defaultAngle += bone._renderUnit.WorldMatrixWrap._angleDeg;
			}

			bool isFliped_X = bone._worldMatrix.Scale.x < 0.0f;
			bool isFliped_Y = bone._worldMatrix.Scale.y < 0.0f;
			bool is1AxisFlipped = isFliped_X != isFliped_Y;//1개의 축만 뒤집힌 경우

			//추가 20.10.6 : defaultAngle은 부모 본의 Scale에 따라 반전된다. (자식 본은 제외..?)
			//bool isParentFlipped_X = false;
			//bool isParentFlipped_Y = false;
			
			if(bone._parentBone != null)
			{
				//isParentFlipped_X = bone._parentBone._worldMatrix.Scale.x < 0.0f;
				//isParentFlipped_Y = bone._parentBone._worldMatrix.Scale.y < 0.0f;
				//Y
				if(bone._parentBone._worldMatrix.Scale.y < 0.0f)
				{
					defaultAngle = 180.0f - defaultAngle;
				}
				//X
				if(bone._parentBone._worldMatrix.Scale.x < 0.0f)
				{
					defaultAngle = -defaultAngle;
				}
			}
			else if (bone._renderUnit != null)
			{
				//isParentFlipped_X = bone._renderUnit.WorldMatrixWrap._scale.x < 0.0f;
				//isParentFlipped_Y = bone._renderUnit.WorldMatrixWrap._scale.y < 0.0f;

				//원래는 Scale되는 (반전 뿐만 아니라) 벡터를 이용해서 계산해야한다.
				//defaultAngle = Mathf.Atan(Mathf.Tan((defaultAngle + 90) * Mathf.Deg2Rad) * (bone._renderUnit.WorldMatrixWrap._scale.y / bone._renderUnit.WorldMatrixWrap._scale.x)) - 90;
				
				//Y
				if (bone._renderUnit.WorldMatrixWrap._scale.y < 0.0f)
				{
					defaultAngle = -defaultAngle;
				}
				//X
				if (bone._renderUnit.WorldMatrixWrap._scale.x < 0.0f)
				{
					defaultAngle = -defaultAngle;
				}
			}




			

			////if(isFliped_Y)
			//if(isParentFlipped_Y)
			//{
			//	defaultAngle = 180.0f - defaultAngle;
			//}

			////if(isFliped_X)
			//if(isParentFlipped_X)
			//{
			//	defaultAngle = -defaultAngle;
			//}

			
			defaultAngle = apUtil.AngleTo180(defaultAngle);

			if (isDraw_IK)
			{
				//IK Angle 범위를 그려준다.
				Vector2 unitVector_Lower = Vector2.zero;
				Vector2 unitVector_Upper = Vector2.zero;
				Vector2 unitVector_Pref = Vector2.zero;

				//추가 20.8.8 : 스케일에 따라서 벡터의 방향이 바뀌어야 한다.
				if(is1AxisFlipped)
				{
					//한개의 축만 뒤집혀진 경우
					//부호와 Lower<->Upper가 반대이다.
					unitVector_Lower = apMatrix3x3.TRS(Vector2.zero, defaultAngle - bone._IKAngleRange_Upper, Vector2.one).MultiplyPoint(unitVector);
					unitVector_Upper = apMatrix3x3.TRS(Vector2.zero, defaultAngle - bone._IKAngleRange_Lower, Vector2.one).MultiplyPoint(unitVector);
					unitVector_Pref = apMatrix3x3.TRS(Vector2.zero, defaultAngle - bone._IKAnglePreferred, Vector2.one).MultiplyPoint(unitVector);
				}
				else
				{
					//일반적인 경우
					unitVector_Lower = apMatrix3x3.TRS(Vector2.zero, defaultAngle + bone._IKAngleRange_Lower, Vector2.one).MultiplyPoint(unitVector);
					unitVector_Upper = apMatrix3x3.TRS(Vector2.zero, defaultAngle + bone._IKAngleRange_Upper, Vector2.one).MultiplyPoint(unitVector);
					unitVector_Pref = apMatrix3x3.TRS(Vector2.zero, defaultAngle + bone._IKAnglePreferred, Vector2.one).MultiplyPoint(unitVector);
				}
				

				unitVector_Lower.Normalize();
				unitVector_Upper.Normalize();
				unitVector_Pref.Normalize();

				unitVector_Lower *= bone._shapeLength * worldMatrix.Scale.y * 1.2f;
				unitVector_Upper *= bone._shapeLength * worldMatrix.Scale.y * 1.2f;
				unitVector_Pref *= bone._shapeLength * worldMatrix.Scale.y * 1.5f;

				//BeginBatch_ColoredPolygon();//이전				
				_matBatch.BeginPass_Color(GL.TRIANGLES);//변경 21.5.18


				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Lower.x, unitVector_Lower.y), 3, Color.magenta, false);
				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Upper.x, unitVector_Upper.y), 3, Color.magenta, false);
				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Pref.x, unitVector_Pref.y), 3, Color.green, false);
				
				//삭제 21.5.18
				//EndBatch();


			}

			//추가 20.5.24 : 지글 본의 각도 제한을 보여주자
			if(isDraw_Jiggle)
			{	
				Vector2 unitVector_Lower = Vector2.zero;
				Vector2 unitVector_Upper = Vector2.zero;

				//추가 20.8.8 : 스케일에 따라서 벡터의 방향이 바뀌어야 한다.
				if (is1AxisFlipped)
				{
					//한개의 축만 뒤집혀진 경우
					//부호와 Lower<->Upper가 반대이다.
					unitVector_Lower = apMatrix3x3.TRS(Vector2.zero, defaultAngle - bone._jiggle_AngleLimit_Max, Vector2.one).MultiplyPoint(unitVector);
					unitVector_Upper = apMatrix3x3.TRS(Vector2.zero, defaultAngle - bone._jiggle_AngleLimit_Min, Vector2.one).MultiplyPoint(unitVector);
				}
				else
				{
					//일반적인 경우
					unitVector_Lower = apMatrix3x3.TRS(Vector2.zero, defaultAngle + bone._jiggle_AngleLimit_Min, Vector2.one).MultiplyPoint(unitVector);
					unitVector_Upper = apMatrix3x3.TRS(Vector2.zero, defaultAngle + bone._jiggle_AngleLimit_Max, Vector2.one).MultiplyPoint(unitVector);
				}
				
				
				unitVector_Lower.Normalize();
				unitVector_Upper.Normalize();

				unitVector_Lower *= bone._shapeLength * worldMatrix.Scale.y * 1.4f;
				unitVector_Upper *= bone._shapeLength * worldMatrix.Scale.y * 1.4f;

				//BeginBatch_ColoredPolygon();
				_matBatch.BeginPass_Color(GL.TRIANGLES);//변경 21.5.18

				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Lower.x, unitVector_Lower.y), 3, Color.yellow, false);
				DrawBoldLine(posW_Start, posW_Start + new Vector2(unitVector_Upper.x, unitVector_Upper.y), 3, Color.yellow, false);
				
				
				//EndBatch();//삭제 21.5.18
			}

			//if(bone._isIKtargetDebug)
			//{
			//	DrawBox(bone._calculatedIKTargetPosDebug, 30, 30, new Color(1.0f, 0.0f, 1.0f, 1.0f), false);
			//	int nBosDebug = bone._calculatedIKBonePosDebug.Count;
			//	for (int i = 0; i < nBosDebug; i++)
			//	{
			//		Color debugColor = (new Color(0.0f, 1.0f, 0.0f, 1.0f) * ((nBosDebug - 1)- i) + new Color(0.0f, 0.0f, 1.0f, 1.0f) * i) / (float)(nBosDebug - 1);

			//		DrawBox(bone._calculatedIKBonePosDebug[i], 20 + i * 5, 20 + i * 5, debugColor, false);
			//	}

			//}
		}


		//	본 그리기 > 버전 2 (바늘 형태)
		/// <summary>
		/// DrawBone_V2, DrawBoneOutline_V2 함수를 연속으로 사용할때는 Batch가 가능하다.
		/// Begin / End 함수를 이용하자.
		/// </summary>
		public static void BeginBatch_DrawBones_V2()
		{
			_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
			//_matBatch.SetClippingSize(_glScreenClippingSize);

			//GL.Begin(GL.TRIANGLES);
		}

		

		public static void DrawBone_V2(	apBone bone, 
										bool isDrawOutline, 
										bool isBoneIKUsing, 
										bool isUseBoneToneColor, 
										bool isAvailable, 
										bool isNeedResetMat, 
										bool isTransculentRender)
		{
			if (bone == null)
			{
				return;
			}

			

			Color boneColor = bone._color;
			if(isUseBoneToneColor)
			{
				boneColor = _toneBoneColor;
			}
			else if(!isAvailable)
			{
				//추가 : 사용 불가능하다면 회색으로 보인다.
				boneColor = Color.gray;
			}
			else
			{
				//그 외의 모든 경우는 Bone 색상을 이용하되, Alpha는 상황에 따라 결정하자.
				if(isTransculentRender)
				{
					//반투명 옵션으로 렌더링 + 회색
					//Debug.Log("Transculent Render [" + bone._name + "]");
					boneColor = Color.gray;
					boneColor.a = 0.25f;
				}
				else
				{
					//그 외는 모두 불투명 렌더링
					boneColor.a = 1.0f;
				}
			}

			
			apMatrix worldMatrix = null;//이전
			//apBoneWorldMatrix worldMatrix = null;//변경 20.8.12 : CompleMatrix 방식
			

			Vector2 posW_Start = Vector2.zero;
			Vector2 posGL_Start = Vector2.zero;

			bool isHelperBone = bone._shapeHelper;

			if(!isHelperBone)
			{
				//헬퍼 본이 아닐때
				
				Vector2 posGL_Mid1 = Vector2.zero;
				Vector2 posGL_Mid2 = Vector2.zero;
				Vector2 posGL_End1 = Vector2.zero;
				Vector2 posGL_End2 = Vector2.zero;
				Vector2 posGL_Back1 = Vector2.zero;
				Vector2 posGL_Back2 = Vector2.zero;

				float uOffset = (isDrawOutline ? 0.25f : 0.0f);

				Vector2 uv_Back1 = new Vector2(0.25f + uOffset, 1.0f);
				Vector2 uv_Back2 = new Vector2(0.0f + uOffset, 1.0f);
				Vector2 uv_Mid1 = new Vector2(0.25f + uOffset, 0.9375f);
				Vector2 uv_Mid2 = new Vector2(0.0f + uOffset, 0.9375f);
				Vector2 uv_End1 = new Vector2(0.25f + uOffset, 0.0f);
				Vector2 uv_End2 = new Vector2(0.0f + uOffset, 0.0f);

				if (isBoneIKUsing)
				{
					//worldMatrix = bone._worldMatrix_IK;//이전
					worldMatrix = bone._guiMatrix_IK;//변경 20.8.23 : GUIMatrix IK 이용

					posW_Start = worldMatrix._pos;
					if(!isUseBoneToneColor)
					{
						posGL_Start = apGL.World2GL(posW_Start);
						posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_IK.Mid1);
						posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_IK.Mid2);
						posGL_End1 = apGL.World2GL(bone._shapePoints_V2_IK.End1);
						posGL_End2 = apGL.World2GL(bone._shapePoints_V2_IK.End2);
						posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_IK.Back1);
						posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_IK.Back2);
					}
					else
					{
						//Onion Skin 전용 좌표 계산
						Vector2 deltaOnionPos = apGL._tonePosOffset * apGL._zoom;
						//Vector2 deltaOnionPos = apGL._tonePosOffset * 0.001f;
						//Vector2 deltaOnionPos = apGL._tonePosOffset;
					
						posGL_Start = apGL.World2GL(posW_Start) + deltaOnionPos;
						posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_IK.Mid1) + deltaOnionPos;
						posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_IK.Mid2) + deltaOnionPos;
						posGL_End1 = apGL.World2GL(bone._shapePoints_V2_IK.End1) + deltaOnionPos;
						posGL_End2 = apGL.World2GL(bone._shapePoints_V2_IK.End2) + deltaOnionPos;
						posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_IK.Back1) + deltaOnionPos;
						posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_IK.Back2) + deltaOnionPos;
					}
				}
				else
				{
					//worldMatrix = bone._worldMatrix;
					worldMatrix = bone._guiMatrix;
					posW_Start = worldMatrix._pos;

					if (!isUseBoneToneColor)
					{
						posGL_Start = apGL.World2GL(posW_Start);
						posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid1);
						posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid2);
						posGL_End1 = apGL.World2GL(bone._shapePoints_V2_Normal.End1);
						posGL_End2 = apGL.World2GL(bone._shapePoints_V2_Normal.End2);
						posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_Normal.Back1);
						posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_Normal.Back2);
					}
					else
					{
						//Onion Skin 전용 좌표 계산
						Vector2 deltaOnionPos = apGL._tonePosOffset * apGL._zoom;
						//Vector2 deltaOnionPos = apGL._tonePosOffset * 0.001f;
						//Vector2 deltaOnionPos = apGL._tonePosOffset;

						posGL_Start = apGL.World2GL(posW_Start) + deltaOnionPos;
						posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid1) + deltaOnionPos;
						posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid2) + deltaOnionPos;
						posGL_End1 = apGL.World2GL(bone._shapePoints_V2_Normal.End1) + deltaOnionPos;
						posGL_End2 = apGL.World2GL(bone._shapePoints_V2_Normal.End2) + deltaOnionPos;
						posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_Normal.Back1) + deltaOnionPos;
						posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_Normal.Back2) + deltaOnionPos;
					}

				
				}

				//그려보자
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);

					//GL.Begin(GL.TRIANGLES);
				}
				
				GL.Color(boneColor);

				//CCW
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Back2);	GL.Vertex(posGL_Back2);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);

				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);

				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);

				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_End1);	GL.Vertex(posGL_End1);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);

				//CW
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Back2);	GL.Vertex(posGL_Back2);

				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				

				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				

				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_End1);	GL.Vertex(posGL_End1);

				if(!isDrawOutline)
				{
					//외곽선 렌더링이 아닌 경우에만 원점 그리기
					//float orgRadius = bone._shapePoints_V2_Normal.Radius * Zoom;
					float radius_Org = apBone.RenderSetting_V2_Radius_Org * Zoom;
					Vector2 posGL_Org_LT = new Vector2(posGL_Start.x - radius_Org, posGL_Start.y + radius_Org);
					Vector2 posGL_Org_RT = new Vector2(posGL_Start.x + radius_Org, posGL_Start.y + radius_Org);
					Vector2 posGL_Org_LB = new Vector2(posGL_Start.x - radius_Org, posGL_Start.y - radius_Org);
					Vector2 posGL_Org_RB = new Vector2(posGL_Start.x + radius_Org, posGL_Start.y - radius_Org);

					Vector2 uv_Org_LT = new Vector2(0.75f, 1.0f);
					Vector2 uv_Org_RT = new Vector2(1.0f, 1.0f);
					Vector2 uv_Org_LB = new Vector2(0.75f, 0.875f);
					Vector2 uv_Org_RB = new Vector2(1.0f, 0.875f);

					//ORG
					GL.TexCoord(uv_Org_LT);	GL.Vertex(posGL_Org_LT);
					GL.TexCoord(uv_Org_LB);	GL.Vertex(posGL_Org_LB);
					GL.TexCoord(uv_Org_RB);	GL.Vertex(posGL_Org_RB);

					GL.TexCoord(uv_Org_RB);	GL.Vertex(posGL_Org_RB);
					GL.TexCoord(uv_Org_RT);	GL.Vertex(posGL_Org_RT);
					GL.TexCoord(uv_Org_LT);	GL.Vertex(posGL_Org_LT);
				}

				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}
			}
			else
			{
				//헬퍼본일때
				if (isBoneIKUsing)
				{
					//worldMatrix = bone._worldMatrix_IK;
					//posW_Start = worldMatrix.Pos;
					//GUIMatrix로 변경
					worldMatrix = bone._guiMatrix_IK;
					posW_Start = worldMatrix._pos;
				}
				else
				{
					//worldMatrix = bone._worldMatrix;
					//posW_Start = worldMatrix.Pos;
					//GUIMatrix로 변경
					worldMatrix = bone._guiMatrix;
					posW_Start = worldMatrix._pos;
				}

				if (!isUseBoneToneColor)
				{
					posGL_Start = apGL.World2GL(posW_Start);
				}
				else
				{
					//Onion Skin 전용 좌표 계산
					Vector2 deltaOnionPos = apGL._tonePosOffset * apGL._zoom;
					posGL_Start = apGL.World2GL(posW_Start) + deltaOnionPos;
				}
				//float orgRadius = bone._shapePoints_V2_Normal.Radius * Zoom;
				
				float radius_Helper = apBone.RenderSetting_V2_Radius_Helper * Zoom;

				

				Vector2 posGL_Helper_LT = new Vector2(posGL_Start.x - radius_Helper, posGL_Start.y + radius_Helper);
				Vector2 posGL_Helper_RT = new Vector2(posGL_Start.x + radius_Helper, posGL_Start.y + radius_Helper);
				Vector2 posGL_Helper_LB = new Vector2(posGL_Start.x - radius_Helper, posGL_Start.y - radius_Helper);
				Vector2 posGL_Helper_RB = new Vector2(posGL_Start.x + radius_Helper, posGL_Start.y - radius_Helper);

				
				
				float vOffset = (isDrawOutline ? -0.125f : 0.0f);

				Vector2 uv_Helper_LT = new Vector2(0.75f, 0.875f + vOffset);
				Vector2 uv_Helper_RT = new Vector2(1.0f, 0.875f + vOffset);
				Vector2 uv_Helper_LB = new Vector2(0.75f, 0.75f + vOffset);
				Vector2 uv_Helper_RB = new Vector2(1.0f, 0.75f + vOffset);

				//그려보자
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);

					//GL.Begin(GL.TRIANGLES);
				}
				
				GL.Color(boneColor);

				//Helper
				GL.TexCoord(uv_Helper_LT);	GL.Vertex(posGL_Helper_LT);
				GL.TexCoord(uv_Helper_LB);	GL.Vertex(posGL_Helper_LB);
				GL.TexCoord(uv_Helper_RB);	GL.Vertex(posGL_Helper_RB);

				GL.TexCoord(uv_Helper_RB);	GL.Vertex(posGL_Helper_RB);
				GL.TexCoord(uv_Helper_RT);	GL.Vertex(posGL_Helper_RT);
				GL.TexCoord(uv_Helper_LT);	GL.Vertex(posGL_Helper_LT);

				if(!isDrawOutline)
				{
					//외곽선 렌더링이 아닌 경우에만 원점 그리기
					float radius_Org = apBone.RenderSetting_V2_Radius_Org * Zoom;

					Vector2 posGL_Org_LT = new Vector2(posGL_Start.x - radius_Org, posGL_Start.y + radius_Org);
					Vector2 posGL_Org_RT = new Vector2(posGL_Start.x + radius_Org, posGL_Start.y + radius_Org);
					Vector2 posGL_Org_LB = new Vector2(posGL_Start.x - radius_Org, posGL_Start.y - radius_Org);
					Vector2 posGL_Org_RB = new Vector2(posGL_Start.x + radius_Org, posGL_Start.y - radius_Org);

					Vector2 uv_Org_LT = new Vector2(0.75f, 1.0f);
					Vector2 uv_Org_RT = new Vector2(1.0f, 1.0f);
					Vector2 uv_Org_LB = new Vector2(0.75f, 0.875f);
					Vector2 uv_Org_RB = new Vector2(1.0f, 0.875f);

					//ORG
					GL.TexCoord(uv_Org_LT);	GL.Vertex(posGL_Org_LT);
					GL.TexCoord(uv_Org_LB);	GL.Vertex(posGL_Org_LB);
					GL.TexCoord(uv_Org_RB);	GL.Vertex(posGL_Org_RB);

					GL.TexCoord(uv_Org_RB);	GL.Vertex(posGL_Org_RB);
					GL.TexCoord(uv_Org_RT);	GL.Vertex(posGL_Org_RT);
					GL.TexCoord(uv_Org_LT);	GL.Vertex(posGL_Org_LT);
				}

				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}
			}
		}

		public static void DrawSelectedBone_V2(apBone bone, BONE_SELECTED_OUTLINE_COLOR outlineColor, bool isBoneIKUsing = false)
		{
			//본 그리기 + 두꺼운 외곽선
			//+ IK 속성이 있는 경우, GUI에 표시하자
			if (bone == null)
			{
				return;
			}

			Color lineColor;


			Color lineColor_Default = Color.black;
			Color lineColor_Reserve = Color.black;

			switch (outlineColor)
			{
				case BONE_SELECTED_OUTLINE_COLOR.MainSelected:
				case BONE_SELECTED_OUTLINE_COLOR.SubSelected:
					lineColor_Default = _lineColor_BoneOutline_V2_Default;
					lineColor_Reserve = _lineColor_BoneOutline_V2_Reverse;
					break;

				case BONE_SELECTED_OUTLINE_COLOR.LinkTarget:
					lineColor_Default = _lineColor_BoneOutlineRollOver_V2_Default;
					lineColor_Reserve = _lineColor_BoneOutlineRollOver_V2_Reverse;
					break;
			}

			float diff_Default = GetColorDif(bone._color, lineColor_Default);
			//float diff_Reverse = GetColorDif(bone._color, lineColor_Reserve);

			//if(diff_Default * COLOR_DEFAULT_BIAS > diff_Reverse)
			if(diff_Default > COLOR_SIMILAR_BIAS)
			{
				//Default 색상이 더 차이가 크다.
				lineColor = lineColor_Default;
			}
			else
			{
				//Reserve 색상이 더 차이가 크다.
				lineColor = lineColor_Reserve;
			}

			lineColor.a = _animRatio_BoneOutlineAlpha;

			
			apMatrix worldMatrix = null;//이전
			//apBoneWorldMatrix worldMatrix = null;//변경 20.8.12 : ComplexMatrix로 변경



			Vector2 posW_Start = Vector2.zero;
			Vector2 posGL_Start = Vector2.zero;

			bool isHelperBone = bone._shapeHelper;

			if(!isHelperBone)
			{
				//헬퍼 본이 아닐때
				Vector2 posGL_Mid1 = Vector2.zero;
				Vector2 posGL_Mid2 = Vector2.zero;
				Vector2 posGL_End1 = Vector2.zero;
				Vector2 posGL_End2 = Vector2.zero;
				Vector2 posGL_Back1 = Vector2.zero;
				Vector2 posGL_Back2 = Vector2.zero;

				Vector2 uv_Outline_Back1 = new Vector2(0.75f, 1.0f);
				Vector2 uv_Outline_Back2 = new Vector2(0.5f, 1.0f);
				Vector2 uv_Outline_Mid1 = new Vector2(0.75f, 0.9375f);
				Vector2 uv_Outline_Mid2 = new Vector2(0.5f, 0.9375f);
				Vector2 uv_Outline_End1 = new Vector2(0.75f, 0.0f);
				Vector2 uv_Outline_End2 = new Vector2(0.5f, 0.0f);

				if (isBoneIKUsing)
				{
					//worldMatrix = bone._worldMatrix_IK;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix_IK;
					posW_Start = worldMatrix._pos;
					
					posGL_Start = apGL.World2GL(posW_Start);
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_IK.Mid1);
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_IK.Mid2);
					posGL_End1 = apGL.World2GL(bone._shapePoints_V2_IK.End1);
					posGL_End2 = apGL.World2GL(bone._shapePoints_V2_IK.End2);
					posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_IK.Back1);
					posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_IK.Back2);
				}
				else
				{
					//worldMatrix = bone._worldMatrix;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix;
					posW_Start = worldMatrix._pos;

					posGL_Start = apGL.World2GL(posW_Start);
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid1);
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid2);
					posGL_End1 = apGL.World2GL(bone._shapePoints_V2_Normal.End1);
					posGL_End2 = apGL.World2GL(bone._shapePoints_V2_Normal.End2);
					posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_Normal.Back1);
					posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_Normal.Back2);
				}

				
				//그려보자
				//변경 21.5.18
				_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);

				//GL.Begin(GL.TRIANGLES);


				GL.Color(lineColor);

				GL.TexCoord(uv_Outline_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Outline_Back2);	GL.Vertex(posGL_Back2);
				GL.TexCoord(uv_Outline_Mid2);	GL.Vertex(posGL_Mid2);

				GL.TexCoord(uv_Outline_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Outline_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Outline_Back1);	GL.Vertex(posGL_Back1);

				GL.TexCoord(uv_Outline_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Outline_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Outline_End2);	GL.Vertex(posGL_End2);

				GL.TexCoord(uv_Outline_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_Outline_End1);	GL.Vertex(posGL_End1);
				GL.TexCoord(uv_Outline_Mid1);	GL.Vertex(posGL_Mid1);

				//CW
				GL.TexCoord(uv_Outline_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Outline_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Outline_Back2);	GL.Vertex(posGL_Back2);

				GL.TexCoord(uv_Outline_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Outline_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Outline_Mid1);	GL.Vertex(posGL_Mid1);
				

				GL.TexCoord(uv_Outline_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Outline_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_Outline_Mid2);	GL.Vertex(posGL_Mid2);
				

				GL.TexCoord(uv_Outline_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_Outline_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Outline_End1);	GL.Vertex(posGL_End1);
				
				
				//삭제
				//GL.End();//<나중에 일괄 EndPass>
			}
			else
			{
				//헬퍼본일때
				if (isBoneIKUsing)
				{
					//worldMatrix = bone._worldMatrix_IK;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix_IK;
					posW_Start = worldMatrix._pos;
				}
				else
				{
					//worldMatrix = bone._worldMatrix;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix;
					posW_Start = worldMatrix._pos;
				}

				posGL_Start = apGL.World2GL(posW_Start);

				//float orgRadius = bone._shapePoints_V2_Normal.Radius * Zoom;
				float radius_Helper = apBone.RenderSetting_V2_Radius_Helper * Zoom;

				Vector2 posGL_Helper_LT = new Vector2(posGL_Start.x - radius_Helper, posGL_Start.y + radius_Helper);
				Vector2 posGL_Helper_RT = new Vector2(posGL_Start.x + radius_Helper, posGL_Start.y + radius_Helper);
				Vector2 posGL_Helper_LB = new Vector2(posGL_Start.x - radius_Helper, posGL_Start.y - radius_Helper);
				Vector2 posGL_Helper_RB = new Vector2(posGL_Start.x + radius_Helper, posGL_Start.y - radius_Helper);

				Vector2 uv_Helper_LT = new Vector2(0.75f, 0.625f);
				Vector2 uv_Helper_RT = new Vector2(1.0f, 0.625f);
				Vector2 uv_Helper_LB = new Vector2(0.75f, 0.5f);
				Vector2 uv_Helper_RB = new Vector2(1.0f, 0.5f);

				//그려보자
				//변경 21.5.18
				_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
				//_matBatch.SetClippingSize(_glScreenClippingSize);

				//GL.Begin(GL.TRIANGLES);


				GL.Color(lineColor);

				//Helper
				GL.TexCoord(uv_Helper_LT);	GL.Vertex(posGL_Helper_LT);
				GL.TexCoord(uv_Helper_LB);	GL.Vertex(posGL_Helper_LB);
				GL.TexCoord(uv_Helper_RB);	GL.Vertex(posGL_Helper_RB);

				GL.TexCoord(uv_Helper_RB);	GL.Vertex(posGL_Helper_RB);
				GL.TexCoord(uv_Helper_RT);	GL.Vertex(posGL_Helper_RT);
				GL.TexCoord(uv_Helper_LT);	GL.Vertex(posGL_Helper_LT);
				
				//삭제 21.5.18
				//GL.End();//<나중에 일괄 EndPass>
			}

			//추가 : IK 속성이 있는 경우, GUI에 표시하자
			if (bone._IKTargetBone != null)
			{
				Vector2 IKTargetPos = Vector2.zero;
				if (isBoneIKUsing)
				{
					//IKTargetPos = World2GL(bone._IKTargetBone._worldMatrix_IK._pos);
					IKTargetPos = World2GL(bone._IKTargetBone._worldMatrix_IK.Pos);
				}
				else
				{
					//IKTargetPos = World2GL(bone._IKTargetBone._worldMatrix._pos);
					IKTargetPos = World2GL(bone._IKTargetBone._worldMatrix.Pos);
				}
				DrawAnimatedLineGL(posGL_Start, IKTargetPos, Color.magenta, true);
			}

			if (bone._IKHeaderBone != null)
			{
				Vector2 IKHeadPos = Vector2.zero;
				if (isBoneIKUsing)
				{
					IKHeadPos = World2GL(bone._IKHeaderBone._worldMatrix_IK.Pos);
				}
				else
				{
					IKHeadPos = World2GL(bone._IKHeaderBone._worldMatrix.Pos);
				}
				DrawAnimatedLineGL(IKHeadPos, posGL_Start, Color.magenta, true);
			}

			if(bone._IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None)
			{
				if(bone._IKController._effectorBone != null)
				{
					Color lineColorIK = Color.yellow;
					if(bone._IKController._controllerType == apBoneIKController.CONTROLLER_TYPE.LookAt)
					{
						lineColorIK = Color.cyan;
					}
					Vector2 effectorPos = Vector2.zero;
					if(isBoneIKUsing)
					{
						effectorPos = World2GL(bone._IKController._effectorBone._worldMatrix_IK.Pos);
					}
					else
					{
						effectorPos = World2GL(bone._IKController._effectorBone._worldMatrix.Pos);
					}
					DrawAnimatedLineGL(posGL_Start, effectorPos, lineColorIK, true);
				}
			}
		}

		public static void DrawBoneOutline_V2(apBone bone, Color outlineColor, bool isBoneIKUsing, bool isNeedResetMat)
		{
			if (bone == null)
			{
				return;
			}
			
			apMatrix worldMatrix = null;//이전
			//apBoneWorldMatrix worldMatrix = null;//변경 20.8.12 : ComplexMatrix로 변경


			Vector2 posW_Start = Vector2.zero;
			Vector2 posGL_Start = Vector2.zero;

			bool isHelperBone = bone._shapeHelper;

			if(!isHelperBone)
			{
				//헬퍼 본이 아닐때
				
				Vector2 posGL_Mid1 = Vector2.zero;
				Vector2 posGL_Mid2 = Vector2.zero;
				Vector2 posGL_End1 = Vector2.zero;
				Vector2 posGL_End2 = Vector2.zero;
				Vector2 posGL_Back1 = Vector2.zero;
				Vector2 posGL_Back2 = Vector2.zero;

				//Outline (비선택)
				Vector2 uv_Back1 = new Vector2(0.5f, 1.0f);
				Vector2 uv_Back2 = new Vector2(0.25f, 1.0f);
				Vector2 uv_Mid1 = new Vector2(0.5f, 0.9375f);
				Vector2 uv_Mid2 = new Vector2(0.25f, 0.9375f);
				Vector2 uv_End1 = new Vector2(0.5f, 0.0f);
				Vector2 uv_End2 = new Vector2(0.25f, 0.0f);

				if (isBoneIKUsing)
				{
					//worldMatrix = bone._worldMatrix_IK;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix_IK;
					posW_Start = worldMatrix._pos;

					posGL_Start = apGL.World2GL(posW_Start);
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_IK.Mid1);
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_IK.Mid2);
					posGL_End1 = apGL.World2GL(bone._shapePoints_V2_IK.End1);
					posGL_End2 = apGL.World2GL(bone._shapePoints_V2_IK.End2);
					posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_IK.Back1);
					posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_IK.Back2);
				}
				else
				{
					//worldMatrix = bone._worldMatrix;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix;
					posW_Start = worldMatrix._pos;

					posGL_Start = apGL.World2GL(posW_Start);
					posGL_Mid1 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid1);
					posGL_Mid2 = apGL.World2GL(bone._shapePoints_V2_Normal.Mid2);
					posGL_End1 = apGL.World2GL(bone._shapePoints_V2_Normal.End1);
					posGL_End2 = apGL.World2GL(bone._shapePoints_V2_Normal.End2);
					posGL_Back1 = apGL.World2GL(bone._shapePoints_V2_Normal.Back1);
					posGL_Back2 = apGL.World2GL(bone._shapePoints_V2_Normal.Back2);
				}

				//그려보자
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);

					//GL.Begin(GL.TRIANGLES);
				}
				
				GL.Color(outlineColor);

				//CCW
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Back2);	GL.Vertex(posGL_Back2);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);

				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);

				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);

				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_End1);	GL.Vertex(posGL_End1);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);

				//CW
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Back2);	GL.Vertex(posGL_Back2);

				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				GL.TexCoord(uv_Back1);	GL.Vertex(posGL_Back1);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				

				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_Mid2);	GL.Vertex(posGL_Mid2);
				

				GL.TexCoord(uv_End2);	GL.Vertex(posGL_End2);
				GL.TexCoord(uv_Mid1);	GL.Vertex(posGL_Mid1);
				GL.TexCoord(uv_End1);	GL.Vertex(posGL_End1);

				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}
			}
			else
			{
				//헬퍼본일때
				if (isBoneIKUsing)
				{
					//worldMatrix = bone._worldMatrix_IK;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix_IK;
					posW_Start = worldMatrix._pos;
				}
				else
				{
					//worldMatrix = bone._worldMatrix;
					//posW_Start = worldMatrix.Pos;

					//GUI Matrix로 변경 (20.8.23)
					worldMatrix = bone._guiMatrix;
					posW_Start = worldMatrix._pos;
				}

				posGL_Start = apGL.World2GL(posW_Start);

				//float orgRadius = bone._shapePoints_V2_Normal.Radius * Zoom;
				float radius_Helper = apBone.RenderSetting_V2_Radius_Helper * Zoom;

				Vector2 posGL_Helper_LT = new Vector2(posGL_Start.x - radius_Helper, posGL_Start.y + radius_Helper);
				Vector2 posGL_Helper_RT = new Vector2(posGL_Start.x + radius_Helper, posGL_Start.y + radius_Helper);
				Vector2 posGL_Helper_LB = new Vector2(posGL_Start.x - radius_Helper, posGL_Start.y - radius_Helper);
				Vector2 posGL_Helper_RB = new Vector2(posGL_Start.x + radius_Helper, posGL_Start.y - radius_Helper);

				//Outline (비선택)
				Vector2 uv_Helper_LT = new Vector2(0.75f, 0.75f);
				Vector2 uv_Helper_RT = new Vector2(1.0f, 0.75f);
				Vector2 uv_Helper_LB = new Vector2(0.75f, 0.625f);
				Vector2 uv_Helper_RB = new Vector2(1.0f, 0.625f);

				//그려보자
				if (isNeedResetMat)
				{
					//변경 21.5.18
					_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
					//_matBatch.SetClippingSize(_glScreenClippingSize);

					//GL.Begin(GL.TRIANGLES);
				}
				
				GL.Color(outlineColor);

				//Helper
				GL.TexCoord(uv_Helper_LT);	GL.Vertex(posGL_Helper_LT);
				GL.TexCoord(uv_Helper_LB);	GL.Vertex(posGL_Helper_LB);
				GL.TexCoord(uv_Helper_RB);	GL.Vertex(posGL_Helper_RB);

				GL.TexCoord(uv_Helper_RB);	GL.Vertex(posGL_Helper_RB);
				GL.TexCoord(uv_Helper_RT);	GL.Vertex(posGL_Helper_RT);
				GL.TexCoord(uv_Helper_LT);	GL.Vertex(posGL_Helper_LT);

				//삭제 21.5.18
				if (isNeedResetMat)
				{
					//GL.End();//<전환 완료>
					_matBatch.EndPass();
				}
			}
		}



		//본은 아니지만 시작>끝>Width를 계산하여 가상으로 본 그리기
		public static void DrawBone_Virtual_V2(	Vector2 startPosW,
												Vector2 endPosW,
												Color boneColor,
												bool isNeedResetMat)
		{	
			float length = (endPosW - startPosW).magnitude;
			float angle = 0.0f;

			if(length > 0.0f)
			{
				angle = Mathf.Atan2(endPosW.y - startPosW.y, endPosW.x - startPosW.x) * Mathf.Rad2Deg;
				angle += 90.0f;
			}

			angle += 180.0f;
			angle = apUtil.AngleTo180(angle);

			if(_cal_TmpMatrix == null)
			{
				_cal_TmpMatrix = new apMatrix();
			}
			_cal_TmpMatrix.SetIdentity();
			_cal_TmpMatrix.SetTRS(startPosW, angle, Vector2.one, true);
			
			//실제 Bone Length, Width를 계산하자
			//V2 에선 메시 여백때문에 약간 더 길어야 한다.
			float boneLength = length * apBone.BONE_V2_REAL_LENGTH_RATIO;
			float boneWidthHalf = apBone.RenderSetting_V2_WidthHalf;
			
			//이 계산+코드는 apBone의 GUIUpdate 함수 중 V2 코드를 참고한다.
			Vector2 bonePos_End1 =	apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(-boneWidthHalf,	boneLength)));
			Vector2 bonePos_End2 =	apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(boneWidthHalf,	boneLength)));
			Vector2 bonePos_Back1 = apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(-boneWidthHalf,	-boneWidthHalf)));
			Vector2 bonePos_Back2 =	apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(boneWidthHalf,	-boneWidthHalf)));
			Vector2 bonePos_Mid1 =	apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(-boneWidthHalf,	0.0f)));
			Vector2 bonePos_Mid2 =	apGL.World2GL(_cal_TmpMatrix.MulPoint2(new Vector2(boneWidthHalf,	0.0f)));


			
			Vector2 uv_Back1 = new Vector2(0.25f, 1.0f);
			Vector2 uv_Back2 = new Vector2(0.0f, 1.0f);
			Vector2 uv_Mid1 = new Vector2(0.25f, 0.9375f);
			Vector2 uv_Mid2 = new Vector2(0.0f, 0.9375f);
			Vector2 uv_End1 = new Vector2(0.25f, 0.0f);
			Vector2 uv_End2 = new Vector2(0.0f, 0.0f);

			//그려보자
			if (isNeedResetMat)
			{
				//변경 21.5.18
				_matBatch.BeginPass_BoneV2(GL.TRIANGLES);
			}
				
			GL.Color(boneColor);

			//CCW
			GL.TexCoord(uv_Back1);	GL.Vertex(bonePos_Back1);
			GL.TexCoord(uv_Back2);	GL.Vertex(bonePos_Back2);
			GL.TexCoord(uv_Mid2);	GL.Vertex(bonePos_Mid2);

			GL.TexCoord(uv_Mid2);	GL.Vertex(bonePos_Mid2);
			GL.TexCoord(uv_Mid1);	GL.Vertex(bonePos_Mid1);
			GL.TexCoord(uv_Back1);	GL.Vertex(bonePos_Back1);

			GL.TexCoord(uv_Mid1);	GL.Vertex(bonePos_Mid1);
			GL.TexCoord(uv_Mid2);	GL.Vertex(bonePos_Mid2);
			GL.TexCoord(uv_End2);	GL.Vertex(bonePos_End2);

			GL.TexCoord(uv_End2);	GL.Vertex(bonePos_End2);
			GL.TexCoord(uv_End1);	GL.Vertex(bonePos_End1);
			GL.TexCoord(uv_Mid1);	GL.Vertex(bonePos_Mid1);

			//CW
			GL.TexCoord(uv_Back1);	GL.Vertex(bonePos_Back1);
			GL.TexCoord(uv_Mid2);	GL.Vertex(bonePos_Mid2);
			GL.TexCoord(uv_Back2);	GL.Vertex(bonePos_Back2);

			GL.TexCoord(uv_Mid2);	GL.Vertex(bonePos_Mid2);
			GL.TexCoord(uv_Back1);	GL.Vertex(bonePos_Back1);
			GL.TexCoord(uv_Mid1);	GL.Vertex(bonePos_Mid1);

			GL.TexCoord(uv_Mid1);	GL.Vertex(bonePos_Mid1);
			GL.TexCoord(uv_End2);	GL.Vertex(bonePos_End2);
			GL.TexCoord(uv_Mid2);	GL.Vertex(bonePos_Mid2);

			GL.TexCoord(uv_End2);	GL.Vertex(bonePos_End2);
			GL.TexCoord(uv_Mid1);	GL.Vertex(bonePos_Mid1);
			GL.TexCoord(uv_End1);	GL.Vertex(bonePos_End1);

			//외곽선 렌더링이 아닌 경우에만 원점 그리기
			//float orgRadius = bone._shapePoints_V2_Normal.Radius * Zoom;
			float radius_Org = apBone.RenderSetting_V2_Radius_Org * Zoom;


			Vector2 startPosGL = World2GL(startPosW);
			Vector2 posGL_Org_LT = new Vector2(startPosGL.x - radius_Org, startPosGL.y + radius_Org);
			Vector2 posGL_Org_RT = new Vector2(startPosGL.x + radius_Org, startPosGL.y + radius_Org);
			Vector2 posGL_Org_LB = new Vector2(startPosGL.x - radius_Org, startPosGL.y - radius_Org);
			Vector2 posGL_Org_RB = new Vector2(startPosGL.x + radius_Org, startPosGL.y - radius_Org);

			Vector2 uv_Org_LT = new Vector2(0.75f, 1.0f);
			Vector2 uv_Org_RT = new Vector2(1.0f, 1.0f);
			Vector2 uv_Org_LB = new Vector2(0.75f, 0.875f);
			Vector2 uv_Org_RB = new Vector2(1.0f, 0.875f);

			//ORG
			GL.TexCoord(uv_Org_LT);	GL.Vertex(posGL_Org_LT);
			GL.TexCoord(uv_Org_LB);	GL.Vertex(posGL_Org_LB);
			GL.TexCoord(uv_Org_RB);	GL.Vertex(posGL_Org_RB);

			GL.TexCoord(uv_Org_RB);	GL.Vertex(posGL_Org_RB);
			GL.TexCoord(uv_Org_RT);	GL.Vertex(posGL_Org_RT);
			GL.TexCoord(uv_Org_LT);	GL.Vertex(posGL_Org_LT);

			if (isNeedResetMat)
			{
				_matBatch.EndPass();
			}

				
			
		}

		//------------------------------------------------------------------------------------------------
		// Draw Grid
		//------------------------------------------------------------------------------------------------
		public static void DrawGrid(Color lineColor_Center, Color lineColor)
		{
			int pixelSize = 50;

			//Color lineColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
			//Color lineColor_Center = new Color(0.7f, 0.7f, 0.3f, 1.0f);

			if (_zoom < 0.2f + 0.05f)
			{
				pixelSize = 200;
				lineColor.a = 0.4f;
			}
			else if (_zoom < 0.5f + 0.05f)
			{
				pixelSize = 100;
				lineColor.a = 0.7f;
			}

			//Vector2 centerPos = World2GL(Vector2.zero);

			//Screen의 Width, Height에 해당하는 극점을 찾자
			//Vector2 pos_LT = GL2World(new Vector2(0, 0));
			//Vector2 pos_RB = GL2World(new Vector2(_windowWidth, _windowHeight));
			Vector2 pos_LT = GL2World(new Vector2(-500, -500));
			Vector2 pos_RB = GL2World(new Vector2(_windowWidth + 500, _windowHeight + 500));

			float yWorld_Max = Mathf.Max(pos_LT.y, pos_RB.y) + 100;
			float yWorld_Min = Mathf.Min(pos_LT.y, pos_RB.y) - 200;
			float xWorld_Max = Mathf.Max(pos_LT.x, pos_RB.x);
			float xWorld_Min = Mathf.Min(pos_LT.x, pos_RB.x);

			// 가로줄 먼저 (+- Y로 움직임)
			Vector2 curPos = Vector2.zero;
			//Vector2 curPosGL = Vector2.zero;
			Vector2 posA, posB;

			curPos.y = (int)(yWorld_Min / pixelSize) * pixelSize;


			//추가 21.5.18
			_matBatch.EndPass();
			_matBatch.BeginPass_Color(GL.LINES);

			// + Y 방향 (아래)
			while (true)
			{
				//curPosGL = World2GL(curPos);

				//if(curPosGL.y < 0 || curPosGL.y > _windowHeight)
				//{
				//	break;
				//}
				if (curPos.y > yWorld_Max)
				{
					break;
				}


				posA.x = pos_LT.x;
				posA.y = curPos.y;

				posB.x = pos_RB.x;
				posB.y = curPos.y;

				DrawLine(posA, posB, lineColor, false);

				curPos.y += pixelSize;
			}


			curPos = Vector2.zero;
			curPos.x = (int)(xWorld_Min / pixelSize) * pixelSize;

			// + X 방향 (오른쪽)
			while (true)
			{
				//curPosGL = World2GL(curPos);

				//if(curPosGL.x < 0 || curPosGL.x > _windowWidth)
				//{
				//	break;
				//}
				if (curPos.x > xWorld_Max)
				{
					break;
				}

				posA.y = pos_LT.y;
				posA.x = curPos.x;

				posB.y = pos_RB.y;
				posB.x = curPos.x;

				DrawLine(posA, posB, lineColor, false);

				curPos.x += pixelSize;
			}

			//중앙선

			curPos = Vector2.zero;

			posA.x = pos_LT.x;
			posA.y = curPos.y;

			posB.x = pos_RB.x;
			posB.y = curPos.y;

			DrawLine(posA, posB, lineColor_Center, false);


			posA.y = pos_LT.y;
			posA.x = curPos.x;

			posB.y = pos_RB.y;
			posB.x = curPos.x;

			DrawLine(posA, posB, lineColor_Center, false);

			_matBatch.EndPass();
		}


		// Editing Border
		public static void DrawEditingBorderline()
		{
			//Vector2 pos = new Vector2(_windowPosX + (_windowWidth / 2), _windowPosY + (_windowHeight / 2));
			Vector2 pos = new Vector2((_windowWidth / 2), (_windowHeight));

			Color borderColor = new Color(0.7f, 0.0f, 0.0f, 0.8f);
			DrawBox(GL2World(pos), (float)(_windowWidth + 100) / _zoom, 50.0f / _zoom, borderColor, false);

			pos.y = -12;

			DrawBox(GL2World(pos), (float)(_windowWidth + 100) / _zoom, 50.0f / _zoom, borderColor, false);
		}

		//-----------------------------------------------------------------------------------------
		public static void ResetCursorEvent()
		{
			_isAnyCursorEvent = false;
			_isDelayedCursorEvent = false;
			_delayedCursorPos = Vector2.zero;
			_delayedCursorType = MouseCursor.Zoom;
		}

		/// <summary>
		/// 마우스 커서를 나오게 하자
		/// </summary>
		/// <param name="mousePos"></param>
		/// <param name="pos"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="cursorType"></param>
		private static void AddCursorRect(Vector2 mousePos, Vector2 pos, float width, float height, MouseCursor cursorType)
		{
			if (pos.x < 0 || pos.x > _windowWidth || pos.y < 0 || pos.y > _windowHeight)
			{
				return;
			}

			if (mousePos.x < pos.x - width * 2 ||
				mousePos.x > pos.x + width * 2 ||
				mousePos.y < pos.y - height * 2 ||
				mousePos.y > pos.y + height * 2)
			{
				//영역을 벗어났다.
				return;
			}

			pos.x -= width / 2;
			pos.y -= height / 2;

			EditorGUIUtility.AddCursorRect(new Rect(pos.x, pos.y, width, height), cursorType);

			_isAnyCursorEvent = true;

		}

		/// <summary>
		/// 마우스 커서를 바꾼다.
		/// 범위는 GL영역 전부이다.
		/// 한번만 호출되며, GUI가 모두 끝날 때 ProcessDelayedCursor()함수를 호출해야한다.
		/// </summary>
		/// <param name="mousePos"></param>
		/// <param name="cursorType"></param>
		public static void AddCursorRectDelayed(Vector2 mousePos, MouseCursor cursorType)
		{
			if(_isAnyCursorEvent || _isDelayedCursorEvent)
			{
				return;
			}
			if (mousePos.x < 0 || mousePos.x > _windowWidth || mousePos.y < 0 || mousePos.y > _windowHeight)
			{
				return;
			}

			_isDelayedCursorEvent = true;
			_delayedCursorPos = mousePos;
			_delayedCursorType = cursorType;

			//Debug.Log("AddCursorRectDelayed : " + mousePos);

			
		}

		public static void ProcessDelayedCursor()
		{
			//Debug.Log("ProcessDelayedCursor : " + _isDelayedCursorEvent);

			if(!_isDelayedCursorEvent
				|| _isAnyCursorEvent
				)
			{
				_isDelayedCursorEvent = false;
				return;
			}
			_isDelayedCursorEvent = false;
			float bias = 20;
			
			EditorGUIUtility.AddCursorRect(
								new Rect(
									_posX_NotCalculated + _delayedCursorPos.x - bias, 
									_posY_NotCalculated + _delayedCursorPos.y - bias, 
									bias * 2, bias * 2), 
								//new Rect(mousePos.x - bias, mousePos.y - bias, bias * 2, bias * 2), 
								//new Rect(_posX_NotCalculated, _posY_NotCalculated, _windowWidth, _windowHeight),
								_delayedCursorType);
			//Debug.Log("AddCurRect : " + _delayedCursorType);
		}


		//-----------------------------------------------------------------------------------------
		private static Color _weightColor_Gray = new Color(0.2f, 0.2f, 0.2f, 1.0f);
		private static Color _weightColor_Blue = new Color(0.0f, 0.2f, 1.0f, 1.0f);
		private static Color _weightColor_Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		private static Color _weightColor_Red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		public static Color GetWeightColor(float weight)
		{
			if (weight < 0.0f)
			{
				return _weightColor_Gray;
			}
			else if (weight < 0.5f)
			{
				return _weightColor_Blue * (1.0f - weight * 2.0f) + _weightColor_Yellow * (weight * 2.0f);
			}
			else if (weight < 1.0f)
			{
				return _weightColor_Yellow * (1.0f - (weight - 0.5f) * 2.0f) + _weightColor_Red * ((weight - 0.5f) * 2.0f);
			}
			else
			{
				return _weightColor_Red;
			}
		}

		public static Color GetWeightColor2(float weight, apEditor editor)
		{
			if (weight < 0.0f)
			{
				return editor._colorOption_VertColor_NotSelected;
			}
			else if (weight < 0.25f)
			{
				return (_vertColor_Weighted_0 * (0.25f - weight) + _vertColor_Weighted_25 * (weight)) / 0.25f;
			}
			else if (weight < 0.5f)
			{
				return (_vertColor_Weighted_25 * (0.25f - (weight - 0.25f)) + _vertColor_Weighted_50 * (weight - 0.25f)) / 0.25f;
			}
			else if (weight < 0.75f)
			{
				return (_vertColor_Weighted_50 * (0.25f - (weight - 0.5f)) + _vertColor_Weighted_75 * (weight - 0.5f)) / 0.25f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted_75 * (0.25f - (weight - 0.75f)) + editor._colorOption_VertColor_Selected * (weight - 0.75f)) / 0.25f;
			}
			else
			{
				//return _weightColor_Red;
				return editor._colorOption_VertColor_Selected;
			}
		}

		public static Color GetWeightColor3(float weight)
		{

			if (weight < 0.0f)
			{
				return _vertColor_Weighted3_0;
			}
			else if (weight < 0.25f)
			{
				return (_vertColor_Weighted3_0 * (0.25f - weight) + _vertColor_Weighted3_25 * (weight)) / 0.25f;
			}
			else if (weight < 0.5f)
			{
				return (_vertColor_Weighted3_25 * (0.25f - (weight - 0.25f)) + _vertColor_Weighted3_50 * (weight - 0.25f)) / 0.25f;
			}
			else if (weight < 0.75f)
			{
				return (_vertColor_Weighted3_50 * (0.25f - (weight - 0.5f)) + _vertColor_Weighted3_75 * (weight - 0.5f)) / 0.25f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted3_75 * (0.25f - (weight - 0.75f)) + _vertColor_Weighted3_100 * (weight - 0.75f)) / 0.25f;
			}
			else
			{
				//return _weightColor_Red;
				return _vertColor_Weighted3_100;
			}
		}


		public static Color GetWeightColor3_Vert(float weight)
		{
			if (weight < 0.0f)
			{
				return _vertColor_Weighted3Vert_0;
			}
			else if (weight < 0.25f)
			{
				return (_vertColor_Weighted3Vert_0 * (0.25f - weight) + _vertColor_Weighted3Vert_25 * (weight)) / 0.25f;
			}
			else if (weight < 0.5f)
			{
				return (_vertColor_Weighted3Vert_25 * (0.25f - (weight - 0.25f)) + _vertColor_Weighted3Vert_50 * (weight - 0.25f)) / 0.25f;
			}
			else if (weight < 0.75f)
			{
				return (_vertColor_Weighted3Vert_50 * (0.25f - (weight - 0.5f)) + _vertColor_Weighted3Vert_75 * (weight - 0.5f)) / 0.25f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted3Vert_75 * (0.25f - (weight - 0.75f)) + _vertColor_Weighted3Vert_100 * (weight - 0.75f)) / 0.25f;
			}
			else
			{
				//return _weightColor_Red;
				return _vertColor_Weighted3Vert_100;
			}
		}



		//추가 20.3.28 : Vivid 방식의 리깅 가중치 그라디언트. (HSV를 이용한다.)
		public static Color GetWeightColor3_Vivid(float weight)
		{
			if(weight < 0.00001f)
			{
				return _vertHSV_Weighted3_NULL;//검은색. 이건 RGB타입이다.
			}
			Vector3 curHSV = Vector3.zero;
			//Hue는 0 (빨강) ~ 0.167 (노랑) ~ 0.667 (파랑)
			//Sat는 1.0 (고정)
			//Value는 Weight = 0.5까지는 1, 그 이하는 서서히 0.5로 수렴
			
			curHSV.y = 1.0f;
			float lerp = 0.0f;
			if(weight < 0.5f)
			{
				lerp = weight * 2.0f;
				curHSV.x = (0.667f * (1.0f - lerp)) + (0.167f * lerp);
				curHSV.z = (0.5f * (1.0f - lerp)) + (1.0f * lerp);
			}
			else
			{
				lerp = (weight - 0.5f) * 2.0f;
				curHSV.x = (0.167f * (1.0f - lerp)) + (0.0f * lerp);
				curHSV.z = 1.0f;
			}
			//if (weight < 0.15f)
			//{
			//	curHSV = (_vertHSV_Weighted3_0 * (0.15f - weight) + _vertHSV_Weighted3_15 * (weight)) / 0.15f;
			//}
			//else if (weight < 0.30f)
			//{
			//	curHSV = (_vertHSV_Weighted3_15 * (0.15f - (weight - 0.15f)) + _vertHSV_Weighted3_30 * (weight - 0.15f)) / 0.15f;
			//}
			//else if (weight < 0.5f)
			//{
			//	curHSV = (_vertHSV_Weighted3_30 * (0.20f - (weight - 0.30f)) + _vertHSV_Weighted3_50 * (weight - 0.30f)) / 0.20f;
			//}
			//else if (weight < 0.75f)
			//{
			//	curHSV = (_vertHSV_Weighted3_50 * (0.25f - (weight - 0.5f)) + _vertHSV_Weighted3_75 * (weight - 0.5f)) / 0.25f;
			//}
			//else if (weight < 1.0f)
			//{
			//	curHSV = (_vertHSV_Weighted3_75 * (0.25f - (weight - 0.75f)) + _vertHSV_Weighted3_100 * (weight - 0.75f)) / 0.25f;
			//}
			//else
			//{
			//	curHSV = _vertHSV_Weighted3_100;
			//}
			return Color.HSVToRGB(curHSV.x, curHSV.y, curHSV.z, false);
		}




		public static Color GetWeightColor4(float weight)
		{
			if (weight <= 0.0001f)
			{
				return _vertColor_Weighted4_0_Null;
			}
			else if (weight < 0.33f)
			{
				return (_vertColor_Weighted4_0 * (0.33f - weight) + _vertColor_Weighted4_33 * (weight)) / 0.33f;
			}
			else if (weight < 0.66f)
			{
				return (_vertColor_Weighted4_33 * (0.33f - (weight - 0.33f)) + _vertColor_Weighted4_66 * (weight - 0.33f)) / 0.33f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted4_66 * (0.34f - (weight - 0.66f)) + _vertColor_Weighted4_100 * (weight - 0.66f)) / 0.34f;
			}
			else
			{
				return _vertColor_Weighted4_100;
			}
		}


		public static Color GetWeightColor4_Vert(float weight)
		{
			if (weight <= 0.0001f)
			{
				return _vertColor_Weighted4Vert_Null;
			}
			else if (weight < 0.33f)
			{
				return (_vertColor_Weighted4Vert_0 * (0.33f - weight) + _vertColor_Weighted4Vert_33 * (weight)) / 0.33f;
			}
			else if (weight < 0.66f)
			{
				return (_vertColor_Weighted4Vert_33 * (0.33f - (weight - 0.33f)) + _vertColor_Weighted4Vert_66 * (weight - 0.33f)) / 0.33f;
			}
			else if (weight < 1.0f)
			{
				return (_vertColor_Weighted4Vert_66 * (0.34f - (weight - 0.66f)) + _vertColor_Weighted4Vert_100 * (weight - 0.66f)) / 0.34f;
			}
			else
			{
				return _vertColor_Weighted4Vert_100;
			}
		}

		public static Color GetWeightGrayscale(float weight)
		{
			//return _weightColor_Gray * (1.0f - weight) + Color.black * weight;
			return Color.black * (1.0f - weight) + Color.white * weight;
		}

		// Window 파라미터 복사 및 복구
		//------------------------------------------------------------------------------------
		public class WindowParameters
		{
			public int _windowWidth;
			public int _windowHeight;
			public Vector2 _scrol_NotCalculated;
			public Vector2 _windowScroll;
			public int _totalEditorWidth;
			public int _totalEditorHeight;
			public int _posX_NotCalculated;
			public int _posY_NotCalculated;
			public float _zoom;
			public Vector4 _glScreenClippingSize;
			public float _animationTimeCount;
			public float _animationTimeRatio;

			public WindowParameters() { }
		}

		public static void GetWindowParameters(WindowParameters inParam)
		{
			inParam._windowWidth = _windowWidth;
			inParam._windowHeight = _windowHeight;
			inParam._scrol_NotCalculated = _scrol_NotCalculated;
			inParam._windowScroll = _windowScroll;
			inParam._totalEditorWidth = _totalEditorWidth;
			inParam._totalEditorHeight = _totalEditorHeight;
			inParam._posX_NotCalculated = _posX_NotCalculated;
			inParam._posY_NotCalculated = _posY_NotCalculated;
			inParam._zoom = _zoom;
			inParam._glScreenClippingSize = _glScreenClippingSize;
			inParam._animationTimeCount = _animationTimeCount;
			inParam._animationTimeRatio = _animationTimeRatio;
		}

		public static void RecoverWindowSize(WindowParameters winParam)
		{
			_windowWidth = winParam._windowWidth;
			_windowHeight = winParam._windowHeight;
			_scrol_NotCalculated = winParam._scrol_NotCalculated;
			_windowScroll = winParam._windowScroll;
			_totalEditorWidth = winParam._totalEditorWidth;
			_totalEditorHeight = winParam._totalEditorHeight;
			_posX_NotCalculated = winParam._posX_NotCalculated;
			_posY_NotCalculated = winParam._posY_NotCalculated;
			_zoom = winParam._zoom;
			_glScreenClippingSize = winParam._glScreenClippingSize;
			_animationTimeCount = winParam._animationTimeCount;
			_animationTimeRatio = winParam._animationTimeRatio;
		}

		public static void SetScreenClippingSizeTmp(Vector4 clippingSize)
		{
			_glScreenClippingSize = clippingSize;
		}
	}

}