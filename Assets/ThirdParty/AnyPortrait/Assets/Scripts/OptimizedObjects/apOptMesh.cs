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
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Rendered Class that has a Mesh.
	/// </summary>
	public class apOptMesh : MonoBehaviour
	{
		// Members
		//------------------------------------------------
		/// <summary>[Please do not use it] Parent Portrait</summary>
		public apPortrait _portrait = null;

		/// <summary>[Please do not use it] Unique ID</summary>
		public int _uniqueID = -1;//meshID가 아니라 meshTransform의 ID를 사용한다.

		/// <summary>
		/// Paranet Opt Transform
		/// </summary>
		public apOptTransform _parentTransform;

		// Components
		//------------------------------------------------
		[HideInInspector]
		public MeshFilter _meshFilter = null;

		//재질 관련 변수들
		//변경 12.11 : DrawCall 관리를 위한 코드 변경
		public enum MATERIAL_TYPE
		{
			/// <summary>공유된 재질. apOptSharedMaterial에서 받아온다. (apOptBatchedMaterial을 경유)</summary>
			Shared,
			/// <summary>동일한 재질을 사용하는 메시들의 텍스쳐나 색상 등을 한꺼번에 바꾸고자 할 때. apOptBatchedMaterial에서 받아온다.</summary>
			Batched,
			/// <summary>Shader의 파라미터를 변경한 후 인스턴스 타입을 사용한다.</summary>
			Instanced,
			/// <summary>버텍스 색상 채널을 이용해 인위적으로 병합된 재질. apOptMergedMaterial에서 받아온다.</summary>
			Merged,
		}

		[NonSerialized, HideInInspector]
		private MATERIAL_TYPE _materialType = MATERIAL_TYPE.Shared;

		[NonSerialized, HideInInspector]
		private Material _material_Cur = null;
		
		[NonSerialized, HideInInspector]
		private Material _material_Shared = null;

		[NonSerialized, HideInInspector]
		private apOptBatchedMaterial.MaterialUnit _materialUnit_Batched = null;

		[NonSerialized, HideInInspector]
		private Material _material_Batched = null;

		[NonSerialized, HideInInspector]
		private Material _material_Instanced = null;

		[NonSerialized, HideInInspector]
		private bool _isForceBatch2Shared = false;//만약 Batch가 작동하더라도, 강제로 Shared로 전환될 필요가 있다.

		//추가 21.12.26 : 병합된 재질을 사용하는가
		//병합된 재질을 사용하는 경우
		//- Batch, Shared보다 우선시한다
		//- Instanced 전환시 VertexColor를 위해서 버텍스 색상 배열(기본, Merged 두개)를 만든다.
		[NonSerialized, HideInInspector]
		private bool _isMerged = false;

		[NonSerialized, HideInInspector]
		private Material _material_Merged = null;

		[NonSerialized, HideInInspector]
		private Color[] _vertColors_NotMerged = null;//병합이 안될때의 버텍스 색상들. 흰색이 들어가있다.

		[NonSerialized, HideInInspector]
		private Color[] _vertColors_Merged = null;//병합이 될때의 버텍스 색상들. 채널에 맞는 값이 들어가있다.





		[HideInInspector]
		public MeshRenderer _meshRenderer = null;

		
		//기본 설정이 Batch Material을 사용하는 것이라면 별도의 값을 저장한다.
		//이때는 SharedMaterial이 null로 Bake된다
		/// <summary>[Please do not use it] Is Batch Target</summary>
		public bool _isBatchedMaterial = false;

		/// <summary>[Please do not use it] Batch Material ID</summary>
		public int _batchedMatID = -1;

		public bool _isDefaultColorGray = false;

		/// <summary>[Please do not use it] Current Rendered Texture (Read Only)</summary>
		[HideInInspector]
		public Texture2D _texture = null;

		/// <summary>[Please do not use it] Unique ID of Linked Texture Data</summary>
		[HideInInspector]
		public int _textureID = -1;

		//20.4.21 : Extra로부터 복구할 수 있게 Texture를 "Extra용 / Base용"으로 구분하자
		//현재 적용된 MainTexture가 Extra인지 아닌지 구분할 수 있다.
		//Extra는 Extra Option에 의해서만 변경되며, 이 모드동안은 텍스쳐 변경 함수가 적용되지 않는다.
		private enum TEXTURE_MODE
		{
			Base, Extra
		}
		[NonSerialized, HideInInspector]
		private TEXTURE_MODE _textureMode = TEXTURE_MODE.Base;

		[NonSerialized, HideInInspector]
		private Texture2D _texture_Base = null;


		/// <summary>[Please do not use it]</summary>
		[NonSerialized, HideInInspector]
		public Mesh _mesh = null;//<변경 : 저장 안됩니더

		// Vertex 값들
		//apRenderVertex에 해당하는 apOptRenderVertex의 배열 (리스트 아닙니더)로 저장한다.

		//<기본값>
		[SerializeField]
		private apOptRenderVertex[] _renderVerts = null;


		[SerializeField]
		private Vector3[] _vertPositions = null;

		[SerializeField]
		private Vector2[] _vertUVs = null;

		//[SerializeField]
		//private int[] _vertUniqueIDs = null;

		[SerializeField]
		private int[] _vertTris = null;

		[SerializeField]
		private int[] _vertTris_Flipped = null;//<<추가 : Flipped된 경우 Reverse된 값을 사용한다.

		//삭제 19.7.3 : RenderVert와 vertPositon의 개수가 다를 수 있다. (양면의 경우)
		//[SerializeField]
		//private int _nVert = 0;

		//변경 19.7.3 : 버텍스 개수를 두개로 분리. NonSerialized로 바꾸었다. (초기화에서 값 설정)
		[NonSerialized]
		private int _nRenderVerts = 0;


		/// <summary>Rendered Vertices</summary>
		public apOptRenderVertex[] RenderVertices { get { return _renderVerts; } }

		
		

		//<업데이트>
		[NonSerialized]//다시 이걸로 복구 21.5.22
		private Vector3[] _vertPositions_Updated = null;

		

		[SerializeField, HideInInspector]
		public Transform _transform = null;

		[NonSerialized]
		private bool _isInitMesh = false;

		[NonSerialized]
		private bool _isInitMaterial = false;

		[SerializeField]
		private Vector2 _pivotPos = Vector2.zero;

		[SerializeField]
		private bool _isVisibleDefault = true;

		[NonSerialized]
		private bool _isVisible = false;

		[NonSerialized]
		private bool _isUseRiggingCache = false;

		/// <summary>기본값이 아닌 외부에서 숨기려고 할 때 설정된다. RootUnit이 Show 될때 해제된다.</summary>
		[NonSerialized]
		private bool _isHide_External = false;


		//Mask인 경우
		//Child는 업데이트는 하지만 렌더링은 하지 않는다.
		//렌더링을 하지 않으므로 Mesh 갱신을 하지 않음
		//Parent는 업데이트 후 렌더링은 잠시 보류한다.
		//"통합" Vertex으로 정의된 SubMeshData에서 통합 작업을 거친 후에 Vertex 업데이트를 한다.
		//MaskMesh 업데이트는 Portrait에서 Calculate 후 일괄적으로 한다. (List로 관리한다.)
		/// <summary>[Please do not use it] Is Parent Mesh of Clipping Masking</summary>
		public bool _isMaskParent = false;

		/// <summary>[Please do not use it] Is Child Mesh of Clipping Masking</summary>
		public bool _isMaskChild = false;

		//Child인 경우
		/// <summary>[Please do not use it] Masking Parent Mesh ID if clipped</summary>
		public int _clipParentID = -1;

		/// <summary>[Please do not use it] Is Masking Parent Mesh if clipped</summary>
		public apOptMesh _parentOptMesh = null;

		//Parent인 경우
		/// <summary>[Please do not use it] Children if clipping mask </summary>
		public int[] _clipChildIDs = null;
		//public apOptMesh[] _childOptMesh = null;

		[NonSerialized]
		private Color _multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

		[NonSerialized]
		private bool _isAnyMeshColorRequest = false;

		[NonSerialized]
		private bool _isAnyTextureRequest = false;

		[NonSerialized]
		private bool _isAnyCustomPropertyRequest = false;

		
		

		/// <summary>[Please do not use it] Updated Matrix</summary>
		public apMatrix3x3 _matrix_Vert2Mesh = apMatrix3x3.identity;

		/// <summary>[Please do not use it] Updated Matrix</summary>
		public apMatrix3x3 _matrix_Vert2Mesh_Inverse = apMatrix3x3.identity;

		/// <summary>[Please do not use it] Rendering Shader Type</summary>
		[SerializeField]
		public apPortrait.SHADER_TYPE _shaderType = apPortrait.SHADER_TYPE.AlphaBlend;

		/// <summary>[Please do not use it] Shader (not Clipped)</summary>
		[SerializeField]
		public Shader _shaderNormal = null;

		/// <summary>[Please do not use it] Shader (Clipped)</summary>
		[SerializeField]
		public Shader _shaderClipping = null;

		//추가
		/// <summary>[Please do not use it] Shader (Mask Parent)</summary>
		[SerializeField]
		public Shader _shader_AlphaMask = null;//<< Mask Shader를 저장하고 나중에 생성한다.

		[NonSerialized]
		private Material _materialAlphaMask = null;

		/// <summary>[Please do not use it] Mask Texture Size</summary>
		[SerializeField]
		public int _clippingRenderTextureSize = 256;
		
		

		#region [미사용 코드] 이전 : 단일 카메라만 지원
		//[NonSerialized]
		//private RenderTexture _maskRenderTexture = null;

		//[NonSerialized]
		//private RenderTargetIdentifier _maskRenderTargetID = -1;

		//[NonSerialized]
		//private Camera _targetCamera = null;

		//[NonSerialized]
		//private Transform cameraTransform = null;

		//[NonSerialized]
		//private CommandBuffer _commandBuffer = null;

		//public RenderTexture MaskRenderTexture
		//{
		//	get
		//	{
		//		if(!_isRenderTextureCreated || !_isVisible)
		//		{
		//			return null;
		//		}
		//		return _maskRenderTexture;
		//	}
		//}

		///// <summary>[Please do not use it]</summary>
		//[NonSerialized]
		//public Vector4 _maskScreenSpaceOffset = Vector4.zero; 
		#endregion

		//변경 19.9.24 : 1개 또는 여러개의 카메라에 대한 처리를 위해 래핑을 하였다.
		//Mask Child, Mask Parent인 경우에만 생성한다.
		[NonSerialized]
		private apOptMeshRenderCamera _renderCamera = null;

		public apOptMeshRenderCamera.CameraRenderData MainCameraData
		{
			get { if(_renderCamera == null) { return null; } return _renderCamera.MainData; }
		}
		public apOptMeshRenderCamera.CameraRenderData GetCameraData(Camera camera)
		{
			if(_renderCamera == null) { return null; }
			return _renderCamera.GetCameraData(camera);
		}


		private RenderTexture _prevParentRenderTexture = null;
		private RenderTexture _curParentRenderTexture = null;

		//효율적인 Mask를 렌더링하기 위한 변수.
		//딱 렌더링 부분만 렌더링하자
		private Vector3 _vertPosCenter = Vector3.zero;
		//private float _vertRangeMax = 0.0f;

		private float _vertRange_XMin = 0.0f;
		private float _vertRange_YMax = 0.0f;
		private float _vertRange_XMax = 0.0f;
		private float _vertRange_YMin = 0.0f;

		
		private int _shaderID_MainTex = -1;
		private int _shaderID_Color = -1;
		private int _shaderID_MaskTexture = -1;
		private int _shaderID_MaskScreenSpaceOffset = -1;

		private int _shaderID_MaskTexture_L = -1;
		private int _shaderID_MaskTexture_R = -1;

		//계산용 변수들
		private Vector3 _cal_localPos_LT = Vector3.zero;
		private Vector3 _cal_localPos_RB = Vector3.zero;
		private Vector3 _cal_vertWorldPos_Center = Vector3.zero;
		private Vector3 _cal_vertWorldPos_LT = Vector3.zero;
		private Vector3 _cal_vertWorldPos_RB = Vector3.zero;
		private Vector3 _cal_screenPos_Center = Vector3.zero;
		private Vector3 _cal_screenPos_LT = Vector3.zero;
		private Vector3 _cal_screenPos_RB = Vector3.zero;
		private float _cal_prevSizeWidth = 0.0f;
		private float _cal_prevSizeHeight = 0.0f;
		private float _cal_zoomScale = 0.0f;
		private float _cal_aspectRatio = 0.0f;
		private float _cal_newOrthoSize = 0.0f;
		//private Vector2 _cal_centerMoveOffset = Vector2.zero;//<<사용되지 않음
		private float _cal_distCenterToCamera = 0.0f;
		private Vector3 _cal_nextCameraPos = Vector3.zero;
		//private Vector3 _cal_camOffset = Vector3.zero;
		private Matrix4x4 _cal_customWorldToCamera = Matrix4x4.identity;
		private Matrix4x4 _cal_customCullingMatrix = Matrix4x4.identity;
		private Matrix4x4 _cal_newLocalToProjMatrix = Matrix4x4.identity;
		private Matrix4x4 _cal_newWorldMatrix = Matrix4x4.identity;
		private Vector3 _cal_screenPosOffset = Vector3.zero;

		private Color _cal_MeshColor = Color.gray;

		private bool _cal_isVisibleRequest = false;
		private bool _cal_isVisibleRequest_Masked = false;

		//Transform이 Flipped된 경우 -> Vertex 배열을 역으로 계산해야한다.
		//추가 : 2.25

		//Flipped에 관해서 처리를 정밀하게 하자


		private bool _cal_isRootFlipped_X = false;
		private bool _cal_isRootFlipped_Y = false;
		private bool _cal_isRootFlipped = false;
		//private bool _cal_isRootFlipped_Prev = false;

		private bool _cal_isUpdateFlipped_X = false;
		private bool _cal_isUpdateFlipped_Y = false;
		private bool _cal_isUpdateFlipped = false;

		private bool _cal_isFlippedBuffer = false;
		private bool _cal_isFlippedBuffer_Prev = false;

		//추가 : 2.25 계산용 변수 하나 더
		private apMatrix3x3 _cal_Matrix_TFResult_World = apMatrix3x3.identity;

		[SerializeField]
		public bool _isAlways2Side = false;

		//추가 19.6.15 : Material Info가 추가되었다.
		//Bake가 안된 경우 (v1.1.6 또는 이전 버전)에 처리하기 위해서 배열형태로 만든다.
		[SerializeField, NonBackupField]
		private apOptMaterialInfo[] _materialInfo = null;

		public apOptMaterialInfo MaterialInfo
		{
			get
			{
				return (_materialInfo == null || _materialInfo.Length == 0) ? null : _materialInfo[0];
			}
		}

		public bool IsUseMaterialInfo
		{
			get { return MaterialInfo != null; }
		}

		//추가 19.8.5 : SRP를 이용하는 경우 커맨드 버퍼를 이용할 수 없다. 별도의 이벤트를 이용하자. 단 2018.1 이후부터
		[SerializeField, NonBackupField]
		public bool _isUseSRP = false;
		
		//Clipping을 위한 처리 함수를 매번 검사하지 말고 "호출 시점", "해당 함수"를 미리 지정하자
		public delegate void FUNC_UPDATE_COMMAND_BUFFER();
		public delegate void FUNC_UPDATE_MASK_CHILD_VR(Camera camera);

		private FUNC_UPDATE_COMMAND_BUFFER _funcUpdateCommandBuffer = null;
		private FUNC_UPDATE_MASK_CHILD_VR _funcUpdateMaskChildVR = null;

		private enum CLIPPING_FUNC_CALL_TYPE
		{
			None,
			Parent_Calculate,
			Parent_SRP,
			Parent_OnPreRendered,
			Child_Calculate,
			Child_SRP,
			Child_OnPreRendered
		}
		private CLIPPING_FUNC_CALL_TYPE _clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.None;
		

		//추가 21.5.23
		[NonSerialized]
		private float _cal_flipWeight_X = 0.0f;
		
		[NonSerialized]
		private float _cal_flipWeight_Y = 0.0f;

		[NonSerialized]
		private apOptRenderVertex _cal_rVert = null;

		[NonSerialized]
		private apOptCalculatedResultStack _cal_parentCalculateStack = null;


		// Init
		//------------------------------------------------
		void Awake()
		{	
			_transform = transform;

			_cal_isRootFlipped_X = false;
			_cal_isRootFlipped_Y = false;
			_cal_isRootFlipped = false;
			//_cal_isRootFlipped_Prev = false;

			_cal_isUpdateFlipped_X = false;
			_cal_isUpdateFlipped_Y = false;
			_cal_isUpdateFlipped = false;

			_cal_isFlippedBuffer = false;
			_cal_isFlippedBuffer_Prev = false;

			_shaderID_MainTex = Shader.PropertyToID("_MainTex");
			_shaderID_Color = Shader.PropertyToID("_Color");
			_shaderID_MaskTexture = Shader.PropertyToID("_MaskTex");
			_shaderID_MaskScreenSpaceOffset = Shader.PropertyToID("_MaskScreenSpaceOffset");

			_shaderID_MaskTexture_L = Shader.PropertyToID("_MaskTex_L");
			_shaderID_MaskTexture_R = Shader.PropertyToID("_MaskTex_R");

		}

		void Start()
		{
			//InitMesh(false);
			//InstantiateMesh();

			//this.enabled = true;
			this.enabled = false;

			//추가 9.26 : 생성이 안된 경우
			if (_isInitMesh && _isInitMaterial)
			{
				if (_isMaskParent)
				{
					Initialize_MaskParent();
				}
				else if (_isMaskChild)
				{
					Initialize_MaskChild();
				}
			}
		}


		void OnEnable()
		{
			if (_isInitMesh && _isInitMaterial)
			{
				//CleanUpMaskParent();//이전
				ClearCameraData();//변경
			}
		}
#if UNITY_EDITOR
		public bool IsMeshOrMaterialMissingInEditor()
		{
			if (_meshFilter != null && _meshRenderer != null)
			{
				if (_meshFilter.sharedMesh == null
					|| _meshRenderer.sharedMaterial == null)
				{
					return true;
				}
			}
			return false;
		}

		public void ResetMeshAndMaterialIfMissing()
		{
			if (!IsMeshOrMaterialMissingInEditor())
			{
				return;
			}
			if (_meshFilter != null && _meshRenderer != null)
			{
				if (_meshFilter.sharedMesh == null)
				{
					InitMesh(true);
				}

				if (_meshRenderer.sharedMaterial == null)
				{
					MakeInstancedMaterial();
					_meshRenderer.sharedMaterial = _material_Instanced;
				}
			}
		}
#endif

		//변경 : OnDisable이 아닌 Destroy 이벤트에서 Clipping Mask를 초기화하자
		//void OnDisable()
		void OnDestroy()
		{
			if (_isInitMesh && _isInitMaterial)
			{
				//CleanUpMaskParent();
				ClearCameraData();//변경


				//추가 12.12 : 재질 삭제
				try
				{
					if(_material_Instanced != null)
					{
						UnityEngine.Object.Destroy(_material_Instanced);
						_material_Instanced = null;
					}
				}
				catch (Exception)
				{

				}
			}
		}

		void OnWillRenderObject()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif
			if (_isInitMesh && _isInitMaterial)
			{
				if (_isMaskParent)
				{
					Initialize_MaskParent();
				}
				else if (_isMaskChild)
				{
					Initialize_MaskChild();
				}
			}
		}

		// Bake
		//------------------------------------------------
#if UNITY_EDITOR
		/// <summary>[Please do not use it] Bake Functions</summary>
		public void BakeMesh(Vector3[] vertPositions,
								Vector2[] vertUVs,
								int[] vertUniqueIDs,
								int[] vertTris,
								float[] depths,
								Vector2 pivotPos,
								apOptTransform parentTransform,
								Texture2D texture, int textureID,
								apPortrait.SHADER_TYPE shaderType,
								//Shader shaderNormal, Shader shaderClipping,//v1.1.6 또는 이전
								apOptMaterialInfo materialInfo,//v1.1.7 또는 이후
								Shader alphaMask,//<<AlphaMask는 따로
								int maskRenderTextureSize,
								bool isVisibleDefault,
								bool isMaskParent, bool isMaskChild,
								int batchedMatID, //Material batchedMaterial,//<<이건 필요없쩡..
								bool isAlways2Side,
								apPortrait.SHADOW_CASTING_MODE shadowCastMode,
								bool isReceiveShadow,
								bool isUseSRP
								)
		{
			ClearMaterialForBake();

			_parentTransform = parentTransform;

			//변경 21.5.22 : 그냥 할당하지 말고, Copy 이용
			_vertPositions = new Vector3[vertPositions.Length];
			Array.Copy(vertPositions, _vertPositions, vertPositions.Length);
			
			


			_vertUVs = vertUVs;
			//_vertUniqueIDs = vertUniqueIDs;//<<삭제. 의미가 없다.
			_vertTris = vertTris;

			_isAlways2Side = isAlways2Side;

			//추가 : Flipped Tris를 만들자
			_vertTris_Flipped = new int[_vertTris.Length];
			for (int i = 0; i < _vertTris.Length - 2; i+=3)
			{
				_vertTris_Flipped[i + 0] = _vertTris[i + 2];
				_vertTris_Flipped[i + 1] = _vertTris[i + 1];
				_vertTris_Flipped[i + 2] = _vertTris[i + 0];
			}

			if(isAlways2Side)
			{
				//양면을 모두 만들자
				int nTri = _vertTris.Length;
				int nTri2Side = nTri * 2;

				int nVert = vertPositions.Length;
				int nVert2Side = nVert * 2;

				int[] vertTris2Side = new int[nTri2Side];

				//기존 : 같은 버텍스를 두번 공유한다.
				//Array.Copy(_vertTris, 0, vertTris2Side, 0, nTri);
				//Array.Copy(_vertTris_Flipped, 0, vertTris2Side, nTri, nTri);
				
				//_vertTris = vertTris2Side;//<<전환


				//변경 19.7.3 : 그냥 버텍스 리스트를 2개를 만들자
				//정방향
				Array.Copy(_vertTris, 0, vertTris2Side, 0, nTri);

				//역방향 (인덱스 값도 + nTri)
				for (int i = 0; i < _vertTris_Flipped.Length; i++)
				{
					vertTris2Side[i + nTri] = _vertTris_Flipped[i] + nVert;
				}

				_vertTris = vertTris2Side;//적용

				//다른 리스트도 두배로 증가
				
				
				_vertPositions = new Vector3[nVert2Side];
				

				_vertUVs = new Vector2[nVert2Side];
				
				for (int i = 0; i < nVert; i++)
				{
					_vertPositions[i] = vertPositions[i];
					_vertPositions[i + nVert] = vertPositions[i];//<<nVert만큼 뒤에 더 추가
					
					_vertUVs[i] = vertUVs[i];
					_vertUVs[i + nVert] = vertUVs[i];
				}
				
				//변경 19.7.3 : 양면 렌더링일때 (버텍스 수가 다름)
				_nRenderVerts = nVert;
				//_nVertPos = nVert2Side;//사용하지 않음
			}
			else
			{
				//변경 19.7.3 : 단면 렌더링일때 (버텍스 수가 같음)
				_nRenderVerts = vertPositions.Length;
				//_nVertPos = vertPositions.Length;//사용하지 않음
			}
			_texture = texture;
			_textureID = textureID;

			_pivotPos = pivotPos;



			//추가 : 20.4.21
			_textureMode = TEXTURE_MODE.Base;
			_texture_Base = _texture;





			//_nVert = _vertPositions.Length;///이전
			_isVisibleDefault = isVisibleDefault;

			transform.localPosition += new Vector3(-_pivotPos.x, -_pivotPos.y, 0.0f);

			_matrix_Vert2Mesh = apMatrix3x3.TRS(new Vector2(-_pivotPos.x, -_pivotPos.y), 0, Vector2.one);
			_matrix_Vert2Mesh_Inverse = _matrix_Vert2Mesh.inverse;

			_shaderType = shaderType;

			//이전 코드
			//_shaderNormal = shaderNormal;
			//_shaderClipping = shaderClipping;

			//변경된 코드 19.6.15 : MaterialInfo 이용
			_materialInfo = new apOptMaterialInfo[1];
			_materialInfo[0] = materialInfo;



			_shader_AlphaMask = alphaMask;//MaskShader를 넣는다.

			//_materialAlphaMask = new Material(alphaMask);이건 나중에 처리
			_clippingRenderTextureSize = maskRenderTextureSize;

			_isMaskParent = isMaskParent;
			_isMaskChild = isMaskChild;

			//Batch가 가능한 경우
			//1. Mask Child가 아닐 경우
			//2. Parent Tranform의 Default Color가 Gray인 경우
			_isDefaultColorGray =	Mathf.Abs(_parentTransform._meshColor2X_Default.r - 0.5f) < 0.004f &&
									Mathf.Abs(_parentTransform._meshColor2X_Default.g - 0.5f) < 0.004f &&
									Mathf.Abs(_parentTransform._meshColor2X_Default.b - 0.5f) < 0.004f &&
									Mathf.Abs(_parentTransform._meshColor2X_Default.a - 1.0f) < 0.004f;
			
			_isBatchedMaterial = !isMaskChild && _isDefaultColorGray;

			_batchedMatID = batchedMatID;

			//삭제 19.6.16 : 
			//if (_shaderNormal == null)
			//{
			//	Debug.LogError("Shader Normal is Null");
			//}
			//if (_shaderClipping == null)
			//{
			//	Debug.LogError("Shader Clipping is Null");
			//}

			//RenderVert를 만들어주자
			_renderVerts = new apOptRenderVertex[_nRenderVerts];

			//여기서 생성하는 걸로 변경 21.5.23
			_vertPositions_Updated = new Vector3[_nRenderVerts];

			for (int i = 0; i < _nRenderVerts; i++)
			{
				_renderVerts[i] = new apOptRenderVertex(
											_parentTransform, this,
											vertUniqueIDs[i], i,
											new Vector2(vertPositions[i].x, vertPositions[i].y),
											_vertUVs[i],
											depths[i]);

				_renderVerts[i].SetMatrix_1_Static_Vert2Mesh(_matrix_Vert2Mesh);

				//이전 방식
				//_renderVerts[i].SetMatrix_3_Transform_Mesh(parentTransform._matrix_TFResult_WorldWithoutMod.MtrxToSpace);
				//_renderVerts[i].Calculate();

				//변경 21.5.23
				_renderVerts[i].Calculate_None(
					ref parentTransform._matrix_TFResult_WorldWithoutMod._mtrxToSpace,
					1.0f, 1.0f, ref _vertPositions_Updated[i]);
			}

			if (_meshFilter == null || _mesh == null)
			{
				//일단 만들어두기는 한다.
				_meshFilter = GetComponent<MeshFilter>();
				_mesh = new Mesh();
				_mesh.name = this.name + "_Mesh";
				_mesh.Clear();

				_mesh.vertices = _vertPositions;

				_mesh.uv = _vertUVs;
				_mesh.triangles = _vertTris;

				_mesh.RecalculateNormals();
				_mesh.RecalculateBounds();
				_mesh.MarkDynamic();//<<추가 21.5.22

				_meshFilter.sharedMesh = _mesh;
			}

			_mesh.Clear();
			_cal_isFlippedBuffer = false;
			_cal_isFlippedBuffer_Prev = false;

			//재질 설정을 해주자
			//변경 12.12 : Bake시에는 Instanced만 사용하기로 한다.
			//일단 기존의 Material은 삭제
			if (_material_Instanced != null)
			{
				try
				{
					UnityEngine.Object.DestroyImmediate(_material_Instanced);
				}
				catch (Exception) { }
				_material_Instanced = null;
			}
			_materialType = MATERIAL_TYPE.Instanced;

			
			MakeInstancedMaterial();

			_material_Cur = _material_Instanced;
			_material_Batched = null;
			_material_Shared = null;
			_materialUnit_Batched = null;
			_isForceBatch2Shared = false;

			//일단 연결은 삭제한다.
			

			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}

			//이전
			//_meshRenderer.sharedMaterial = _material;//<<현재 Bake된 Material 값을 넣어준다.
			
			//변경 12.12 : Bake에서는 Instanced Material을 넣는다.
			_meshRenderer.sharedMaterial = _material_Cur;
			


			//그림자 설정은 제외 > 변경 : 옵션에 따라서 설정한다.
			//_meshRenderer.receiveShadows = false;
			//_meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

			//변경된 그림자 설정
			_meshRenderer.receiveShadows = isReceiveShadow;
			ShadowCastingMode castMode = ShadowCastingMode.Off;
			switch (shadowCastMode)
			{
				case apPortrait.SHADOW_CASTING_MODE.Off:
					castMode = ShadowCastingMode.Off;
					break;

				case apPortrait.SHADOW_CASTING_MODE.On:
					castMode = ShadowCastingMode.On;
					break;

				case apPortrait.SHADOW_CASTING_MODE.ShadowsOnly:
					castMode = ShadowCastingMode.ShadowsOnly;
					break;

				case apPortrait.SHADOW_CASTING_MODE.TwoSided:
					castMode = ShadowCastingMode.TwoSided;
					break;

			}
			_meshRenderer.shadowCastingMode = castMode;


			_meshRenderer.enabled = _isVisibleDefault;
			_meshRenderer.lightProbeUsage = LightProbeUsage.Off;


			//추가 19.8.5
#if UNITY_2019_1_OR_NEWER
			_isUseSRP = isUseSRP;
#else
			_isUseSRP = false;
#endif
			

			
			//Mask 연결 정보는 일단 리셋
			_clipParentID = -1;
			_clipChildIDs = null;

			//여기서 생성하는건 삭제. 위에서 생성하는 걸로 변경 (21.5.23)
			////여기서 변수를 임시로 생성하자. (Refresh를 위해서)
			//_vertPositions_Updated = new Vector3[_nRenderVerts];
			////_vertPositions_Local = new Vector3[_nRenderVerts];//삭제 21.5.23
			////_vertPositions_World = new Vector2[_nRenderVerts];//삭제 21.5.23
			///


			
			if (!_isAlways2Side)
			{
				//일반 업데이트

				//삭제 21.5.23 : _vertPositions_Updated가 위에서 이미 갱신되었다.
				//for (int i = 0; i < _nRenderVerts; i++)
				//{
				//	//Calculate 전에는 직접 Pivot Pos를 적용해주자 (Calculate에서는 자동 적용)
				//	_vertPositions_Updated[i] = _renderVerts[i]._vertPos3_LocalUpdated;
				//}
			}
			else
			{
				//양면 업데이트
				//이전 코드
				//for (int i = 0; i < _nRenderVerts; i++)
				//{
				//	//Calculate 전에는 직접 Pivot Pos를 적용해주자 (Calculate에서는 자동 적용)
				//	_vertPositions_Updated[i] = _renderVerts[i]._vertPos3_LocalUpdated;
				//	_vertPositions_Updated[i + _nRenderVerts] = _renderVerts[i]._vertPos3_LocalUpdated;
				//}

				//변경 21.5.23
				Vector3[] prevVertPositions_Update = _vertPositions_Updated;
				_vertPositions_Updated = new Vector3[_vertPositions.Length];
				
				//복사 (2배)
				Array.Copy(prevVertPositions_Update, 0, _vertPositions_Updated, 0, _nRenderVerts);//앞쪽
				Array.Copy(prevVertPositions_Update, 0, _vertPositions_Updated, _nRenderVerts, _nRenderVerts);//뒤쪽
			}
			
			

			_transform = transform;

			_shaderID_MainTex = Shader.PropertyToID("_MainTex");
			_shaderID_Color = Shader.PropertyToID("_Color");
			_shaderID_MaskTexture = Shader.PropertyToID("_MaskTex");
			_shaderID_MaskScreenSpaceOffset = Shader.PropertyToID("_MaskScreenSpaceOffset");


			InitMesh(true);

			RefreshMesh();

			if(_isVisibleDefault)
			{
				_meshRenderer.enabled = true;
				_isVisible = true;
				
			}
			else
			{
				_meshRenderer.enabled = false;
				_isVisible = false;
			}
		}
#endif


			//-----------------------------------------------------------------------
#if UNITY_EDITOR
		//Bake를 위해서 이전에 완성된 Material을 삭제하자.
		public void ClearMaterialForBake()
		{
			if(Application.isPlaying)
			{
				return;
			}

			_material_Instanced = null;
			_material_Cur = null;
			_material_Batched = null;
			_material_Shared = null;

			_isInitMaterial = false;
		}
#endif

		//Bake되지 않는 Mesh의 초기화를 호출한다.
		/// <summary>
		/// [Please do not use it]
		/// It is called by "Portrait"
		/// </summary>
		public void InitMesh(bool isForce)
		{
			if (!isForce && _isInitMesh)
			{
				return;
			}

			_transform = transform;


			//체크 19.7.3 : 만약 양면 렌더링인데, RenderVert와 _vertPositions의 개수가 같다면? (마이그레이션 코드)
			if (_isAlways2Side)
			{
				//Debug.Log("Check 2-Sided Mesh [" + this.name + "]");
				//Debug.Log("2-Sided Mesh Check / Vert Pos :" + _vertPositions.Length + ", Render Verts : " + _renderVerts.Length);
				if (_vertPositions.Length == _renderVerts.Length)
				{
					
					//이거의 개수를 두배로 늘려야 한다.
					//다른 리스트도 두배로 증가
					int nVert = _renderVerts.Length;
					int nVert2Side = _renderVerts.Length * 2;

					//Debug.LogError("Expand Verts : " + nVert + " > " + nVert2Side);

					Vector3[] prevVertPos = _vertPositions;
					Vector2[] prevVertUVs = _vertUVs;


					_vertPositions = new Vector3[nVert2Side];
					_vertUVs = new Vector2[nVert2Side];

					for (int i = 0; i < nVert; i++)
					{
						_vertPositions[i] = prevVertPos[i];
						_vertPositions[i + nVert] = prevVertPos[i];//<<nVert만큼 뒤에 더 추가

						_vertUVs[i] = prevVertUVs[i];
						_vertUVs[i + nVert] = prevVertUVs[i];
					}
				}
			}
			


			if(_mesh == null)
			{
				_mesh = new Mesh();
				_mesh.name = this.name + "_Mesh (Instance)";
				_mesh.Clear();

				_mesh.vertices = _vertPositions;
				_mesh.triangles = _vertTris;
				_mesh.uv = _vertUVs;

				_mesh.RecalculateNormals();
				_mesh.RecalculateBounds();
				_mesh.MarkDynamic();//<<추가 21.5.22
			}
			else
			{
				_mesh.Clear();

				_mesh.vertices = _vertPositions;
				_mesh.triangles = _vertTris;
				_mesh.uv = _vertUVs;

				_mesh.RecalculateNormals();
				_mesh.RecalculateBounds();
				_mesh.MarkDynamic();//<<추가 21.5.22
			}

			
			
			if (_meshFilter == null)
			{
				_meshFilter = GetComponent<MeshFilter>();
			}

			_meshFilter.sharedMesh = _mesh;
			
			//_material_Instanced = null;
			//_material_Cur = null;
			//_isInitMaterial = false;

			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}

			//_meshRenderer.material = _material;
			_meshRenderer.sharedMaterial = _material_Cur;

			


			_meshRenderer.enabled = _isVisibleDefault;
			_isVisible = _isVisibleDefault;
			



			_vertPositions_Updated = new Vector3[_vertPositions.Length];

			//삭제 21.5.23 : 필요없는 배열들
			//_vertPositions_Local = new Vector3[_vertPositions.Length];
			//_vertPositions_World = new Vector2[_vertPositions.Length];

			//다시 삭제 21.5.27
			////추가 21.5.23 : 계산 변수가 RenderVertex에서 삭제되고 여기로 이동되었다.
			////개수는 RenderVertex 만큼
			//_renderVertCal_VertexLocalPos = new Vector2[_renderVerts.Length];
			
			////추가 21.5.24 : Rigging Matrix도 여기에 저장한다. RenderVertex의 Rigging Matrix를 대체한다.
			//_renderVertCal_RiggingMatrix = new apMatrix3x3[_renderVerts.Length];


			//이전
			//for (int i = 0; i < _vertPositions.Length; i++)
			//{
			//	_vertPositions_Updated[i] = _vertPositions[i];
			//}

			//변경 21.5.22
			Array.Copy(_vertPositions, _vertPositions_Updated, _vertPositions.Length);

			//다시 삭제 21.5.27
			//Array.Clear(_renderVertCal_VertexLocalPos, 0, _renderVerts.Length);			
			//Array.Clear(_renderVertCal_RiggingMatrix, 0, _renderVerts.Length);


			//_texture_Updated = _texture;

			_isInitMesh = true;

			_cal_isFlippedBuffer = false;
			_cal_isFlippedBuffer_Prev = false;

			_isUseRiggingCache = false;

			//추가 19.7.3 : 버텍스 개수는 InitMesh에서 설정
			_nRenderVerts = (_renderVerts != null) ? _renderVerts.Length : 0;
			//_nVertPos = (_vertPositions != null) ? _vertPositions.Length : 0;//사용하지 않음



			//추가 20.4.21 : ExtraOption 초기화
			_textureMode = TEXTURE_MODE.Base;
			_texture_Base = _texture;

			//여기서 생성되었을 것
			_cal_parentCalculateStack = _parentTransform.CalculatedStack;
			
		}

		/// <summary>
		/// [Please do not use it]
		/// Initialize Mesh
		/// </summary>
		public void InstantiateMesh()
		{	
			if(_mesh == null || _meshFilter == null || _meshFilter.mesh == null)
			{
				return;
			}

			_mesh = Instantiate<Mesh>(_meshFilter.sharedMesh);
			_meshFilter.mesh = _mesh;
			_mesh.MarkDynamic();//<<추가 21.5.22
		}


		//추가 : 먼저 바로 사용할 InstancedMaterial을 만든다.
		//이전과 달리 Batched / Shared는 런타임에서 만들어져서 연결한다.
		//기존 : Batched를 만들고 Instanced로 연결
		//변경 : Instanced를 먼저 만든 뒤, 공유 가능한 재질이 있는지 런타임에서 확인
		//AlphaMask와 Clipping도 만들자
		private void MakeInstancedMaterial()
		{
			//1. Material Instanced를 만들자.
			if (_material_Instanced == null)
			{
				//변경 19.6.16 : MaterialInfo를 이용하여 재질 만들기
				if (IsUseMaterialInfo)
				{
					apOptMaterialInfo matInfo = MaterialInfo;
					_material_Instanced = new Material(matInfo._shader);

					_material_Instanced.SetColor("_Color", _parentTransform._meshColor2X_Default);
					_material_Instanced.SetTexture("_MainTex", matInfo._mainTex);

					//추가 속성도 적용하자.
					matInfo.SetMaterialProperties(_material_Instanced);
				}
				else
				{
					//이전 방식
					if (_isMaskChild)
					{
						_material_Instanced = new Material(_shaderClipping);
					}
					else
					{
						_material_Instanced = new Material(_shaderNormal);
					}
					_material_Instanced.SetColor("_Color", _parentTransform._meshColor2X_Default);
					_material_Instanced.SetTexture("_MainTex", _texture);
				}
			}
			

			//2. Alpha Mask Material을 만들자.
			if(_isMaskParent && _materialAlphaMask == null)
			{
				_materialAlphaMask = new Material(_shader_AlphaMask);
			}

			_materialType = MATERIAL_TYPE.Instanced;
			_material_Cur = _material_Instanced;

			if(_meshRenderer != null && _meshRenderer.sharedMaterial == null)
			{
				_meshRenderer.sharedMaterial = _material_Cur;
			}
		}


		/// <summary>
		/// [Please do not use it]
		/// Initialize Materials
		/// </summary>
		public void InstantiateMaterial(apOptBatchedMaterial batchedMaterial)
		{
			if(_isInitMaterial)
			{
				return;
			}

			//1. Instanced Material(일반/Clipping)과 Alpha Mask Material을 만들자.
			MakeInstancedMaterial();

			//이제 Batched Material과 Shared Material을 각각 받아오자
			if(_isMaskChild)
			{
				//Mask Child라면 Batched/Shared를 사용하지 못한다.
				_material_Batched = null;
				_material_Shared = null;
				_materialUnit_Batched = null;

				//추가 19.10.28 : 일괄 처리를 위해서 클리핑 메시도 다른 형태로 batchedMaterial에 등록해야한다.
				batchedMaterial.LinkClippedMesh(this, _material_Instanced);
			}
			else
			{
				_materialUnit_Batched = batchedMaterial.GetMaterialUnit(_batchedMatID, this);
				if(_materialUnit_Batched != null)
				{
					_material_Batched = _materialUnit_Batched._material;
				}

				//변경 19.6.16
				if(IsUseMaterialInfo)
				{
					//Material Info를 사용한다면
					_material_Shared = batchedMaterial.GetSharedMaterial_MatInfo(MaterialInfo);
				}
				else
				{
					//이전 버전이라면
					_material_Shared = batchedMaterial.GetSharedMaterial_Prev(_texture, _shaderNormal);
				}
				
			}

			_materialType = MATERIAL_TYPE.Instanced;
			_meshRenderer.sharedMaterial = _material_Instanced;//<<일단 Instanced Material 넣기
			
			_isForceBatch2Shared = false;

			//자동으로 선택해보자
			AutoSelectMaterial();
			


			//추가 20.4.21 : ExtraOption 초기화
			_textureMode = TEXTURE_MODE.Base;
			_texture_Base = _texture;


			_isInitMaterial = true;
		}

		//---------------------------------------------------------------------------
		// Mask 관련 초기화
		//---------------------------------------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// Initialize if it is Mask Parent
		/// </summary>
		public void SetMaskBasicSetting_Parent(List<int> clipChildIDs)
		{
			if (clipChildIDs == null || clipChildIDs.Count == 0)
			{
				return;
			}
			_isMaskParent = true;
			_clipParentID = -1;
			_isMaskChild = false;


			if (_clipChildIDs == null || _clipChildIDs.Length != clipChildIDs.Count)
			{
				_clipChildIDs = new int[clipChildIDs.Count];
			}

			for (int i = 0; i < clipChildIDs.Count; i++)
			{
				_clipChildIDs[i] = clipChildIDs[i];
			}
			
		}

		/// <summary>
		/// [Please do not use it]
		/// Initialize if it is Mask Child
		/// </summary>
		public void SetMaskBasicSetting_Child(int parentID)
		{
			_isMaskParent = false;
			_clipParentID = parentID;
			_isMaskChild = true;

			_clipChildIDs = null;
		}
		
		/// <summary>
		/// [Please do not use it]
		/// Initialize reference
		/// </summary>
		public void LinkAsMaskChild(apOptMesh parentMesh)
		{
			_parentOptMesh = parentMesh;

			
			if(_meshRenderer.sharedMaterial == null ||
				_material_Instanced == null)
			{
				MakeInstancedMaterial();
				_meshRenderer.sharedMaterial = _material_Instanced;
			}
		}

		//Mask Parent의 세팅을 리셋한다.
		//카메라 설정이나 씬이 변경되었을 때 호출해야한다.
		/// <summary>
		/// If it is Mask Parent, reset Command Buffers to Camera
		/// </summary>
		public void ResetMaskParentSetting()
		{
			//CleanUpMaskParent();
			//ClearCameraData();//변경

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif

			
			if (_isMaskParent)
			{
				Initialize_MaskParent();
			}
			else if(_isMaskChild)
			{
				Initialize_MaskChild();
			}
		}

		
		//---------------------------------------------------------------------------


		// Update
		//------------------------------------------------
		void Update()
		{
			
		}

		void LateUpdate()
		{

		}
		





		// 외부 업데이트
		//------------------------------------------------
		//삭제 21.5.23 : 미사용 함수
		//public void ReadyToUpdate()
		//{
		//	//?
		//}

		/// <summary>
		/// [Please do not use it]
		/// Update Visibility of Mesh
		/// </summary>
		/// <param name="isVisible"></param>
		public void UpdateVisibility(bool isVisible)
		{
			_cal_isVisibleRequest = _isVisible;
			
			if (!_isHide_External)
			{
				_cal_isVisibleRequest = isVisible;
			}
			else
			{
				//강제로 Hide하는 요청이 있었다면?
				_cal_isVisibleRequest = false;
			}

			_cal_isVisibleRequest_Masked = _cal_isVisibleRequest;

			//추가
			//Mask 메시가 있다면 Visibile 속성이 Parent를 따른다.
			if(_isMaskChild)
			{
				if(_parentOptMesh != null)
				{
					_cal_isVisibleRequest_Masked = _parentOptMesh._isVisible;
				}
			}
		}

		/// <summary>
		/// [Please do not use it]
		/// Update Shape of Mesh
		/// </summary>
		/// <param name="isRigging"></param>
		/// <param name="isVertexLocal"></param>
		/// <param name="isVertexWorld"></param>
		/// <param name="isVisible"></param>
		public void UpdateCalculate(bool isRigging, bool isVertexLocal, bool isVertexWorld, bool isOrthoCorrection)
		{
			
			//이 코드들은 UpdateVisibility() 함수로 이동한다.
			//Visiblity를 별도로 계산한다.
			//_cal_isVisibleRequest = _isVisible;
			
			//if (!_isHide_External)
			//{
			//	_cal_isVisibleRequest = isVisible;
			//}
			//else
			//{
			//	//강제로 Hide하는 요청이 있었다면?
			//	_cal_isVisibleRequest = false;
			//}

			//_cal_isVisibleRequest_Masked = _cal_isVisibleRequest;

			////추가
			////Mask 메시가 있다면 Visibile 속성이 Parent를 따른다.
			//if(_isMaskChild)
			//{
			//	if(_parentOptMesh != null)
			//	{
			//		_cal_isVisibleRequest_Masked = _parentOptMesh._isVisible;
			//	}
			//}

			//둘다 True일때 Show, 하나만 False여도 Hide
			//상태가 바뀔때에만 Show/Hide 토글
			if(_cal_isVisibleRequest && _cal_isVisibleRequest_Masked)
			{
				if(!_isVisible)
				{
					Show();
				}
			}
			else
			{
				if(_isVisible)
				{
					Hide();
				}
			}


			//안보이는건 업데이트하지 말자
			//변경 20.4.2 : 렌더링을 하지 않는다면 아예 리턴하는 것으로 변경
			if (!_isVisible)
			{
				return;
			}
			
			//추가 2.25 : Flipped 관련 코드가 업데이트 초기에 등장한다.
			if (_cal_isRootFlipped)
			{
				//이전 계산에서 Flipped가 되었다면 일단 초기화
				_transform.localScale = Vector3.one;
				_transform.localPosition = new Vector3(-_pivotPos.x, -_pivotPos.y, 0);
			}

			//추가 : World 좌표계의 Flip을 별도로 계산한다.
			_cal_isRootFlipped_X = _parentTransform._rootUnit.IsFlippedX;
			_cal_isRootFlipped_Y = _parentTransform._rootUnit.IsFlippedY;
			//_cal_isRootFlipped_X = _transform.lossyScale.x < 0.0f;
			//_cal_isRootFlipped_Y = _transform.lossyScale.y < 0.0f;
			_cal_isRootFlipped = (_cal_isRootFlipped_X && !_cal_isRootFlipped_Y)
								|| (!_cal_isRootFlipped_X && _cal_isRootFlipped_Y);

			
			//이전
			//float flipWeight_X = 1;
			//float flipWeight_Y = 1;

			//변경 21.5.23
			_cal_flipWeight_X = 1.0f;
			_cal_flipWeight_Y = 1.0f;

			if(_cal_isRootFlipped)
			{
				//이전
				//flipWeight_X = _cal_isRootFlipped_X ? -1 : 1;
				//flipWeight_Y = _cal_isRootFlipped_Y ? -1 : 1;

				//변경
				_cal_flipWeight_X = _cal_isRootFlipped_X ? -1 : 1;
				_cal_flipWeight_Y = _cal_isRootFlipped_Y ? -1 : 1;
			}
				

			_cal_Matrix_TFResult_World = _parentTransform._matrix_TFResult_World.MtrxToSpace;

			//apOptRenderVertex rVert = null;//삭제
			_cal_rVert = null;//변경 21.5.23

//#if UNITY_EDITOR
//			UnityEngine.Profiling.Profiler.BeginSample("Opt Mesh - 1. Calculate Render Vertices");
//#endif


			//삭제
			//apOptCalculatedResultStack calculateStack = _parentTransform.CalculatedStack;

			//변경 21.5.23
			if(_cal_parentCalculateStack == null)
			{
				_cal_parentCalculateStack = _parentTransform.CalculatedStack;
			}
			


			#region [미사용 코드] 원칙에 충실한 이전 방식
			//for (int i = 0; i < _nRenderVerts; i++)
			//{

			//	rVert = _renderVerts[i];

			//	//리깅 추가
			//	if (isRigging)
			//	{	

			//		//변경 20.11.26 : 더 개선된 버전!
			//		if (!_isUseRiggingCache)
			//		{
			//			rVert._matrix_Rigging.SetMatrixWithWeight(
			//				calculateStack.GetDeferredRiggingMatrix_WithLUT(i),
			//				calculateStack._result_RiggingWeight * calculateStack.GetDeferredRiggingWeight(i)//<이 Weight는 런타임에서는 바뀌지 않는다.
			//				);
			//		}
			//		else
			//		{
			//			//캐시를 이용해서 Weight를 가져온다.
			//			rVert._matrix_Rigging.SetMatrixWithWeight(
			//				calculateStack.GetDeferredRiggingMatrix_WithLUT(i),
			//				//calculateStack._result_RiggingWeight * calculateStack.GetDeferredRiggingWeightCache(i)
			//				calculateStack._result_RiggingWeight * calculateStack._result_RiggingVertWeight_Cache[i]//변경 21.5.22 : 직접 호출
			//				);
			//		}


			//	}

			//	if (isVertexLocal)
			//	{
			//		rVert._matrix_Cal_VertLocal.SetTRS(calculateStack.GetDeferredLocalPos(i));//<OPT : VertLocal도 Vector면 된다. + 미리 계산하고 값 복사?할 것>
			//	}

			//	rVert.SetMatrix_3_Transform_Mesh(_cal_Matrix_TFResult_World);//<OPT : _cal_Matrix_TFResult_World도 삭제하고, Calculate에서 바로 전달>

			//	if (isVertexWorld)
			//	{
			//		rVert._matrix_Cal_VertWorld.SetTRS(calculateStack._result_VertWorld[i]);//<OPT : VertWorld는 Matrix가 아닌 Vector면 된다.>
			//	}

			//	//추가
			//	if(isOrthoCorrection)
			//	{
			//		rVert.SetMatrix_5_OrthoCorrection(_parentTransform._convert2TargetMatrix3x3);//<OPT : 이거 할당 삭제할 것>
			//	}

			//	//추가
			//	rVert.SetMatrix_6_FlipWeight(flipWeight_X, flipWeight_Y);//<OPT : 이거 할당 삭제할 것>

			//	rVert.Calculate();

			//	//업데이트 데이터를 넣어준다.
			//	_vertPositions_Updated[i] =  rVert._vertPos3_LocalUpdated;
			//	_vertPositions_World[i] = rVert._vertPos_World;

			//} 
			#endregion


			//변경 21.5.23 : RenderVertex에서 계산하던 것을 외부로 뺐다.
			//조건에 따라 Calculated가 모두 다르다.
			if (isVertexLocal)
			{
//#if UNITY_EDITOR
//				UnityEngine.Profiling.Profiler.BeginSample("<Local Pos>");
//#endif
				//Array.Clear(_renderVertCal_VertexLocalPos, 0, _renderVerts.Length);//다시 삭제
				_cal_parentCalculateStack.SetDeferredLocalPos(/*_renderVertCal_VertexLocalPos, _nRenderVerts*/);

//#if UNITY_EDITOR
//				UnityEngine.Profiling.Profiler.EndSample();
//#endif
			}

			if(isRigging)
			{
				//_cal_parentCalculateStack.GetDeferredRiggingWeight(i)
				//if (!_isUseRiggingCache)
				//{
				//	_cal_rVert._matrix_Rigging.SetMatrixWithWeight(
				//		_cal_parentCalculateStack.GetDeferredRiggingMatrix_WithLUT(i),
				//		_cal_parentCalculateStack._result_RiggingWeight * _cal_parentCalculateStack.GetDeferredRiggingWeight(i)//<이 Weight는 런타임에서는 바뀌지 않는다.
				//		);
				//}
				//else
				//{
				//	//캐시를 이용해서 Weight를 가져온다.
				//	_cal_rVert._matrix_Rigging.SetMatrixWithWeight(
				//		_cal_parentCalculateStack.GetDeferredRiggingMatrix_WithLUT(i),
				//		_cal_parentCalculateStack._result_RiggingWeight * _cal_parentCalculateStack._result_RiggingVertWeight_Cache[i]//변경 21.5.22 : 직접 호출
				//		);
				//}

				//_cal_parentCalculateStack.SetDeferredRiggingMatrix_WithLUT(_renderVertCal_RiggingMatrix);
				_cal_parentCalculateStack.SetDeferredRiggingMatrix_WithLUT();
			}


			if (isVertexLocal)
			{
				if (isVertexWorld)
				{
					if (isRigging)
					{
						if (isOrthoCorrection)
						{
							// Local + World + Rigging + OrthoCorrection
							CalculateRenderVertices_Local_World_Rigging_OrthoCorrection();
						}
						else
						{
							// Local + World + Rigging
							CalculateRenderVertices_Local_World_Rigging();
						}
					}
					else
					{
						if (isOrthoCorrection)
						{
							// Local + World + OrthoCorrection
							CalculateRenderVertices_Local_World_OrthoCorrection();
						}
						else
						{
							// Local + World
							CalculateRenderVertices_Local_World();
						}
					}
					
				}
				else
				{
					if (isRigging)
					{
						if (isOrthoCorrection)
						{
							// Local + Rigging + OrthoCorrection
							CalculateRenderVertices_Local_Rigging_OrthoCorrection();
						}
						else
						{
							// Local + Rigging
							CalculateRenderVertices_Local_Rigging();
						}
					}
					else
					{
						if (isOrthoCorrection)
						{
							// Local + OrthoCorrection
							CalculateRenderVertices_Local_OrthoCorrection();
						}
						else
						{
							// Local
							CalculateRenderVertices_Local();
						}
					}
				}
			}
			else
			{
				if (isVertexWorld)
				{
					if (isRigging)
					{
						if (isOrthoCorrection)
						{
							// World + Rigging + OrthoCorrection
							CalculateRenderVertices_World_Rigging_OrthoCorrection();
						}
						else
						{
							// World + Rigging
							CalculateRenderVertices_World_Rigging();
						}
					}
					else
					{
						if (isOrthoCorrection)
						{
							// World + OrthoCorrection
							CalculateRenderVertices_World_OrthoCorrection();
						}
						else
						{
							// World
							CalculateRenderVertices_World();
						}
					}
					
				}
				else
				{
					if (isRigging)
					{
						if (isOrthoCorrection)
						{
							// Rigging + OrthoCorrection
							CalculateRenderVertices_Rigging_OrthoCorrection();
						}
						else
						{
							// Rigging
//#if UNITY_EDITOR
//							UnityEngine.Profiling.Profiler.BeginSample("Calculate Rigging");
//#endif
							CalculateRenderVertices_Rigging();

//#if UNITY_EDITOR
//							UnityEngine.Profiling.Profiler.EndSample();
//#endif

						}
					}
					else
					{
						if (isOrthoCorrection)
						{
							// OrthoCorrection
							CalculateRenderVertices_OrthoCorrection();
						}
						else
						{
							// <None>
							CalculateRenderVertices_None();
						}
					}
				}
			}






			//추가 19.7.3 : 양면인 경우에는, 뒤쪽면도 업데이트를 해야한다.
			if(_isAlways2Side)
			{
				//이전 : 직접 할당
				//for (int i = 0; i < _nRenderVerts; i++)
				//{
				//	rVert = _renderVerts[i];
				//	_vertPositions_Updated[i + _nRenderVerts] = rVert._vertPos3_LocalUpdated;
				//	_vertPositions_World[i + _nRenderVerts] = rVert._vertPos_World;
				//}

				//변경 21.5.23 : 앞 절반을 뒤 절반에 복사
				Array.Copy(_vertPositions_Updated, 0, _vertPositions_Updated, _nRenderVerts, _nRenderVerts);
			}

			//리깅 캐시를 사용하는 것으로 변경
			if(!_isUseRiggingCache)
			{
				_isUseRiggingCache = true;
			}

//#if UNITY_EDITOR
//			UnityEngine.Profiling.Profiler.EndSample();
//#endif


			//_material.SetColor("_Color", _multiplyColor * _parentTransform._meshColor2X);


			//색상을 제어할 때
			//만약 Color 기본값인 경우 Batch를 위해 Shared로 교체해야한다.
			//색상 지정은 Instance일때만 가능하다

			//if ((_isAnyMeshColorRequest || _parentTransform._isAnyColorCalculated || !_isDefaultColorGray) && _instanceMaterial != null)//이전
			if (_isAnyMeshColorRequest || _parentTransform._isAnyColorCalculated || !_isDefaultColorGray)
			{
				//이전 코드
				//if(_isUseSharedMaterial && !_isMaskChild)
				//{
				//	//Shared를 쓰는 중이라면 교체해야함
				//	AutoSelectMaterial();
				//}

				//일단 색상을 먼저 계산한다. (값이 바뀌었다고 하니까..)
				_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
				_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
				_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
				_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.
				
				//_instanceMaterial.SetColor(_shaderID_Color, _cal_MeshColor);
				_material_Instanced.SetColor(_shaderID_Color, _cal_MeshColor);

				//바로 호출하기 전에
				//만약 결과 값이 Gray(0.5, 0.5, 0.5, 1.0)이라면
				//Shared를 써야한다.

				AutoSelectMaterial(true);//일단 호출해보는 것으로..
			}
			else
			{
				//이전
				//if(!_isUseSharedMaterial && !_isMaskChild)
				//{
				//	//반대로 색상 선택이 없는데 Instance Material을 사용중이라면 Batch를 해야하는 건 아닌지 확인해보자
				//	AutoSelectMaterial();
				//}

				//변경
				if(_materialType == MATERIAL_TYPE.Instanced && !_isMaskChild)
				{
					//반대로 색상 선택이 없는데 Instance Material을 사용중이라면 Shared나 Batch를 써야할 것이다.
					AutoSelectMaterial();
				}
			}


			if (_isMaskChild
				&& _clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Child_Calculate)
			{
				UpdateMaskChild_Basic();

				#region [미사용 코드] 이전 : 단일 카메라만 지원
				//Parent의 Mask를 받아서 넣자
				//if (_parentOptMesh != null)
				//{
				//	_curParentRenderTexture = _parentOptMesh.MaskRenderTexture;
				//}
				//else
				//{
				//	_curParentRenderTexture = null;
				//	Debug.LogError("Null Parent");
				//}

				//if(_curParentRenderTexture != _prevParentRenderTexture)
				//{
				//	//_material.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);//이전
				//	_material_Instanced.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);

				//	_prevParentRenderTexture = _curParentRenderTexture;
				//}
				////_material.SetVector(_shaderID_MaskScreenSpaceOffset, _parentOptMesh._maskScreenSpaceOffset);//이전
				//_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, _parentOptMesh._maskScreenSpaceOffset); 
				#endregion

				//변경 19.9.24 : 멀티 카메라 지원. 이 코드는 그 중에서 "싱글"일 때 동작
				//if (_renderCamera != null
				//		&& _renderCamera.IsValid
				//		&& !_renderCamera.IsVRSupported()
				//		&& _parentOptMesh != null)
				//{
				//	//싱글 카메라일 때는 여기서 처리한다.
				//	//그 외에는 카메라 이벤트에서 처리
				//	apOptMeshRenderCamera.CameraRenderData parentMainCamData = _parentOptMesh.MainCameraData;
				//	if (parentMainCamData != null)
				//	{
				//		_curParentRenderTexture = parentMainCamData._renderTexture;

				//		if (_curParentRenderTexture != _prevParentRenderTexture)
				//		{
				//			_material_Instanced.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);
				//			_prevParentRenderTexture = _curParentRenderTexture;
				//		}
				//		_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, parentMainCamData._maskScreenSpaceOffset);
				//	}
				//}
			}

//#if UNITY_EDITOR
//			UnityEngine.Profiling.Profiler.BeginSample("Opt Mesh - 2. Refresh Mesh");
//#endif

			RefreshMesh();

			if(_isMaskParent 
				&& _clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Parent_Calculate
				&& _funcUpdateCommandBuffer != null)
			{
				//MaskParent면 CommandBuffer를 갱신한다. > Mask 렌더링
				
				//UpdateCommandBuffer();//이전
				_funcUpdateCommandBuffer();//변경
			}

			//TODO 21.5.22 : 이건 옵션으로
			//if (_mesh != null)
			//{
			//	_mesh.RecalculateNormals();
			//}

//#if UNITY_EDITOR
//			UnityEngine.Profiling.Profiler.EndSample();
//#endif
		}





		//추가 21.5.23 : Render Vertex Calculate를 조건 4개에 따라 총 16개의 함수로 나뉘어 호출한다.
		//다 비슷하지만 다르다..
		
		// Local + World + [Rigging] + OrthoCorrection
		private void CalculateRenderVertices_Local_World_Rigging_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				//if (!_isUseRiggingCache)
				//{
				//	_cal_rVert._matrix_Rigging.SetMatrixWithWeight(
				//		_cal_parentCalculateStack.GetDeferredRiggingMatrix_WithLUT(i),
				//		_cal_parentCalculateStack._result_RiggingWeight * _cal_parentCalculateStack.GetDeferredRiggingWeight(i)//<이 Weight는 런타임에서는 바뀌지 않는다.
				//		);
				//}
				//else
				//{
				//	//캐시를 이용해서 Weight를 가져온다.
				//	_cal_rVert._matrix_Rigging.SetMatrixWithWeight(
				//		_cal_parentCalculateStack.GetDeferredRiggingMatrix_WithLUT(i),
				//		_cal_parentCalculateStack._result_RiggingWeight * _cal_parentCalculateStack._result_RiggingVertWeight_Cache[i]//변경 21.5.22 : 직접 호출
				//		);
				//}

				_cal_rVert.Calculate_Local_World_Rigging_OrthoCorrection(	ref _cal_parentCalculateStack._result_VertLocal[i],
																			ref _cal_Matrix_TFResult_World,
																			ref _cal_parentCalculateStack._result_VertWorld[i],
																			ref _cal_parentCalculateStack._result_RiggingMatrices[i],
																			ref _parentTransform._convert2TargetMatrix3x3,
																			_cal_flipWeight_X, _cal_flipWeight_Y,
																			ref _vertPositions_Updated[i]);
			}
		}


		// Local + World + [Rigging]
		private void CalculateRenderVertices_Local_World_Rigging()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Local_World_Rigging(	ref _cal_parentCalculateStack._result_VertLocal[i],
															ref _cal_Matrix_TFResult_World,
															ref _cal_parentCalculateStack._result_VertWorld[i],
															ref _cal_parentCalculateStack._result_RiggingMatrices[i],
															_cal_flipWeight_X, _cal_flipWeight_Y,
															ref _vertPositions_Updated[i]);
			}
		}

		

		// Local + World + OrthoCorrection
		private void CalculateRenderVertices_Local_World_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Local_World_OrthoCorrection(	ref _cal_parentCalculateStack._result_VertLocal[i],
																	ref _cal_Matrix_TFResult_World,
																	ref _cal_parentCalculateStack._result_VertWorld[i],
																	ref _parentTransform._convert2TargetMatrix3x3,
																	_cal_flipWeight_X, _cal_flipWeight_Y,
																	ref _vertPositions_Updated[i]);
			}
		}

		// Local + World
		private void CalculateRenderVertices_Local_World()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Local_World(	ref _cal_parentCalculateStack._result_VertLocal[i],
													ref _cal_Matrix_TFResult_World,
													ref _cal_parentCalculateStack._result_VertWorld[i],
													_cal_flipWeight_X, _cal_flipWeight_Y,
													ref _vertPositions_Updated[i]);
			}
		}

		// Local + [Rigging] + OrthoCorrection
		private void CalculateRenderVertices_Local_Rigging_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Local_Rigging_OrthoCorrection(	ref _cal_parentCalculateStack._result_VertLocal[i],
																	ref _cal_Matrix_TFResult_World,
																	ref _cal_parentCalculateStack._result_RiggingMatrices[i],
																	ref _parentTransform._convert2TargetMatrix3x3,
																	_cal_flipWeight_X, _cal_flipWeight_Y,
																	ref _vertPositions_Updated[i]);
			}
		}


		// Local + [Rigging]
		private void CalculateRenderVertices_Local_Rigging()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Local_Rigging(	ref _cal_parentCalculateStack._result_VertLocal[i],
													ref _cal_Matrix_TFResult_World,
													ref _cal_parentCalculateStack._result_RiggingMatrices[i],
													_cal_flipWeight_X, _cal_flipWeight_Y,
													ref _vertPositions_Updated[i]);
			}
		}

		

		// Local + OrthoCorrection
		private void CalculateRenderVertices_Local_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Local_OrthoCorrection(	ref _cal_parentCalculateStack._result_VertLocal[i],
															ref _cal_Matrix_TFResult_World,
															ref _parentTransform._convert2TargetMatrix3x3,
															_cal_flipWeight_X, _cal_flipWeight_Y,
															ref _vertPositions_Updated[i]);
			}
		}

		// Local
		private void CalculateRenderVertices_Local()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Local(	ref _cal_parentCalculateStack._result_VertLocal[i],
											ref _cal_Matrix_TFResult_World,
											_cal_flipWeight_X, _cal_flipWeight_Y,
											ref _vertPositions_Updated[i]);
			}
		}


		//=====

		// World + [Rigging] + OrthoCorrection
		private void CalculateRenderVertices_World_Rigging_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_World_Rigging_OrthoCorrection(	ref _cal_Matrix_TFResult_World,
																	ref _cal_parentCalculateStack._result_VertWorld[i],
																	ref _cal_parentCalculateStack._result_RiggingMatrices[i],
																	ref _parentTransform._convert2TargetMatrix3x3,
																	_cal_flipWeight_X, _cal_flipWeight_Y,
																	ref _vertPositions_Updated[i]);
			}
		}


		// World + [Rigging]
		private void CalculateRenderVertices_World_Rigging()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_World_Rigging(	ref _cal_Matrix_TFResult_World,
													ref _cal_parentCalculateStack._result_VertWorld[i],
													ref _cal_parentCalculateStack._result_RiggingMatrices[i],
													_cal_flipWeight_X, _cal_flipWeight_Y,
													ref _vertPositions_Updated[i]);
			}
		}

		

		// World + OrthoCorrection
		private void CalculateRenderVertices_World_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_World_OrthoCorrection(	ref _cal_Matrix_TFResult_World,
															ref _cal_parentCalculateStack._result_VertWorld[i],
															ref _parentTransform._convert2TargetMatrix3x3,
															_cal_flipWeight_X, _cal_flipWeight_Y,
															ref _vertPositions_Updated[i]);
			}
		}

		// World
		private void CalculateRenderVertices_World()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_World(	ref _cal_Matrix_TFResult_World,
											ref _cal_parentCalculateStack._result_VertWorld[i],
											_cal_flipWeight_X, _cal_flipWeight_Y,
											ref _vertPositions_Updated[i]);
			}
		}

		// [Rigging] + OrthoCorrection
		private void CalculateRenderVertices_Rigging_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_Rigging_OrthoCorrection(	ref _cal_Matrix_TFResult_World,
																ref _cal_parentCalculateStack._result_RiggingMatrices[i],
																ref _parentTransform._convert2TargetMatrix3x3,
																_cal_flipWeight_X, _cal_flipWeight_Y,
																ref _vertPositions_Updated[i]);
			}
		}


		// [Rigging]
		private void CalculateRenderVertices_Rigging()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];


				//if(_cal_parentCalculateStack == null)
				//{
				//	Debug.LogError("_cal_parentCalculateStack is null");
				//}
				//if(_cal_parentCalculateStack._result_RiggingMatrices == null)
				//{
				//	Debug.LogError("_cal_parentCalculateStack._result_RiggingMatrices is null");
				//}
				//if(_vertPositions_Updated == null)
				//{
				//	Debug.LogError("_vertPositions_Updated is null");
				//}
				_cal_rVert.Calculate_Rigging(	ref _cal_Matrix_TFResult_World,
												ref _cal_parentCalculateStack._result_RiggingMatrices[i],
												_cal_flipWeight_X, _cal_flipWeight_Y,
												ref _vertPositions_Updated[i]);

			}
		}

		

		// OrthoCorrection
		private void CalculateRenderVertices_OrthoCorrection()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_OrthoCorrection(	ref _cal_Matrix_TFResult_World,
														ref _parentTransform._convert2TargetMatrix3x3,
														_cal_flipWeight_X, _cal_flipWeight_Y,
														ref _vertPositions_Updated[i]);
			}
		}

		// None
		private void CalculateRenderVertices_None()
		{	
			for (int i = 0; i < _nRenderVerts; i++)
			{
				_cal_rVert = _renderVerts[i];

				_cal_rVert.Calculate_None(	ref _cal_Matrix_TFResult_World,
											_cal_flipWeight_X, _cal_flipWeight_Y,
											ref _vertPositions_Updated[i]);
			}
		}












		public void RefreshMaskMesh_WithoutUpdateCalculate()
		{
			//Calculate는 하지 않고
			if(_isMaskParent
				&& _clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Parent_Calculate
				&& _funcUpdateCommandBuffer != null
				)
			{
				//MaskParent면 CommandBuffer를 갱신한다.
				
				//UpdateCommandBuffer();//이전
				_funcUpdateCommandBuffer();//변경
			}

			if(_isMaskChild
				&& _clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Child_Calculate)
			{
				UpdateMaskChild_Basic();
			}
		}


		// Vertex Refresh
		//------------------------------------------------
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		public void RefreshMesh()
		{
			//if(_isMaskChild || _isMaskParent)
			//{
			//	return;
			//}

			//Flipped 계산

			//Root의 Scale의 방향이 바뀌었으면 Flipped을 해야한다.
			if (_cal_isRootFlipped)
			{
				//_cal_isRootFlipped_Prev = _cal_isRootFlipped;

				_transform.localScale = new Vector3((_cal_isRootFlipped_X ? -1.0f : 1.0f),
														(_cal_isRootFlipped_Y ? -1.0f : 1.0f),
														1.0f);

				_transform.localPosition = new Vector3((_cal_isRootFlipped_X ? _pivotPos.x : -_pivotPos.x),
														(_cal_isRootFlipped_Y ? _pivotPos.y : -_pivotPos.y),
														0);
			}



			//플립 여부
			//1. optTransform의 스케일이 "한쪽만 음수"인가
			//- 리깅이 적용안되었다면 > optTransform의 worldMatrix의 스케일 검사
			//- 리깅이 적용되었다면 > 리깅이 적용된 모든 본의 "공통 스케일"을 검사 (리깅 본이 없거나 스케일의 부호가 다르다면 처리 실패)
			//2. RootGameObject가 반전되었는가.

			//1또는 2 조건 하나만 적용되어야 한다.

			//이전
			//_cal_isUpdateFlipped_X = _parentTransform._matrix_TFResult_World._scale.x < 0.0f;
			//_cal_isUpdateFlipped_Y = _parentTransform._matrix_TFResult_World._scale.y < 0.0f;
			//_cal_isUpdateFlipped = (_cal_isUpdateFlipped_X != _cal_isUpdateFlipped_Y);

			//변경 20.8.11 : 리깅에도 적용되는 플립 조건
			

			if(_parentTransform._isIgnoreParentModWorldMatrixByRigging)
			{
				//리깅이 적용된 경우
				_cal_isUpdateFlipped = _parentTransform.IsFlippedByRiggingBones();
			}
			else
			{
				//일반적인 경우
				_cal_isUpdateFlipped_X = _parentTransform._matrix_TFResult_World._scale.x < 0.0f;
				_cal_isUpdateFlipped_Y = _parentTransform._matrix_TFResult_World._scale.y < 0.0f;

				_cal_isUpdateFlipped = (_cal_isUpdateFlipped_X != _cal_isUpdateFlipped_Y);
			}			

			_cal_isFlippedBuffer = (_cal_isUpdateFlipped && !_cal_isRootFlipped)
								|| (!_cal_isUpdateFlipped && _cal_isRootFlipped);

			//Transform 제어 -> Vert 제어
			if (_isMaskParent)
			{
				//마스크 처리를 위해서 Vertex의 위치나 분포를 저장해야한다.
				_vertPosCenter = Vector3.zero;
				//_vertRangeMax = -1.0f;

				//Left < Right
				//Bottom < Top

				_vertRange_XMin = float.MaxValue;//Max -> Min
				_vertRange_XMax = float.MinValue;//Min -> Max
				_vertRange_YMin = float.MaxValue;//Max -> Min
				_vertRange_YMax = float.MinValue;//Min -> Max

				
				//_vertPositions_Local를 삭제했다.
				//Array.Copy(_vertPositions_Updated, _vertPositions_Local, _nVertPos);//추가 21.5.22

				//for (int i = 0; i < _nVertPos; i++)
				for (int i = 0; i < _nRenderVerts; i++)//모든 버텍스 체크 필요없다. RenderVertex 만큼만 체크하면 된다.
				{
					//_vertPositions_Local[i] = _vertPositions_Updated[i];//삭제 21.5.22 > Array.Copy로 변경

					//이전
					//_vertRange_XMin = Mathf.Min(_vertRange_XMin, _vertPositions_Local[i].x);
					//_vertRange_XMax = Mathf.Max(_vertRange_XMax, _vertPositions_Local[i].x);
					//_vertRange_YMin = Mathf.Min(_vertRange_YMin, _vertPositions_Local[i].y);
					//_vertRange_YMax = Mathf.Max(_vertRange_YMax, _vertPositions_Local[i].y);

					//변경 21.5.23 : Local 삭제 > Update 바로 이용
					_vertRange_XMin = Mathf.Min(_vertRange_XMin, _vertPositions_Updated[i].x);
					_vertRange_XMax = Mathf.Max(_vertRange_XMax, _vertPositions_Updated[i].x);
					_vertRange_YMin = Mathf.Min(_vertRange_YMin, _vertPositions_Updated[i].y);
					_vertRange_YMax = Mathf.Max(_vertRange_YMax, _vertPositions_Updated[i].y);
				}

				//마스크를 만들 영역을 잡아준다.
				//추가 6.6 : 약간의 공백을 더 넣어준다.
				_vertRange_XMin -= 2.0f;
				_vertRange_XMax += 2.0f;
				_vertRange_YMin -= 2.0f;
				_vertRange_YMax += 2.0f;

				_vertPosCenter.x = (_vertRange_XMin + _vertRange_XMax) * 0.5f;
				_vertPosCenter.y = (_vertRange_YMin + _vertRange_YMax) * 0.5f;
				//_vertRangeMax = Mathf.Max(_vertRange_XMax - _vertRange_XMin, _vertRange_YMax - _vertRange_YMin);
			}
			else
			{
				//이전
				//for (int i = 0; i < _nVertPos; i++)
				//{
				//	//_vertPositions_Local[i] = _transform.InverseTransformPoint(_vertPositions_Updated[i]);
				//	_vertPositions_Local[i] = _vertPositions_Updated[i];
				//}

				//변경 21.5.22 > 삭제 21.5.23 : Local 사용하지 않음
				//Array.Copy(_vertPositions_Updated, _vertPositions_Local, _nVertPos);
			}
			

			//이전
			//_mesh.vertices = _vertPositions_Local;
			//변경 21.5.23 : Update 배열 바로 사용하면 된다.
			_mesh.vertices = _vertPositions_Updated;


			//_mesh.uv = _vertUVs;//삭제 21.5.22
			//추가3.22 : Flip 여부에 따라서 다른 Vertex 배열을 사용한다.
			if (_isAlways2Side)
			{
				_mesh.triangles = _vertTris;
				//_mesh.RecalculateNormals();
			}
			else
			{
				if (_cal_isFlippedBuffer)
				{
					_mesh.triangles = _vertTris_Flipped;
				}
				else
				{
					_mesh.triangles = _vertTris;
				}

				if (_cal_isFlippedBuffer_Prev != _cal_isFlippedBuffer)
				{
					//Flip 여부가 바뀔 대
					//Normal을 다시 계산한다.
					_mesh.RecalculateNormals();
					_cal_isFlippedBuffer_Prev = _cal_isFlippedBuffer;
				}
			}
			
			_mesh.RecalculateBounds();

		}


		//--------------------------------------------------------------------------------
		// 클리핑 Mask Parent
		//--------------------------------------------------------------------------------
		#region [미사용 코드] ClearCameraData로 변경되었다.
		//		//MaskParent일때, 커맨드 버퍼를 초기화한다.
		//		/// <summary>
		//		/// Clean up Command Buffers if it is Mask Parent
		//		/// </summary>
		//		public void CleanUpMaskParent()
		//		{
		//			if (!_isMaskParent)
		//			{
		//				return;
		//			}

		//			_isRenderTextureCreated = false;
		//			if (_targetCamera != null && _commandBuffer != null)
		//			{
		//				_targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);

		//#if UNITY_2019_1_OR_NEWER
		//				RenderPipelineManager.beginCameraRendering -= ProcessSRP;
		//#endif
		//			}
		//			_targetCamera = null;
		//			cameraTransform = null;
		//			_commandBuffer = null;

		//			_maskRenderTargetID = -1;
		//			if (_maskRenderTexture != null)
		//			{
		//				RenderTexture.ReleaseTemporary(_maskRenderTexture);
		//				_maskRenderTexture = null;
		//			}
		//		} 
		#endregion



		public void ClearCameraData()
		{
			if (_isMaskParent)
			{
				if (_renderCamera != null)
				{
					_renderCamera.Clear();
				}

#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering -= ProcessSRP_MaskParent;
#endif
				//_isRenderTextureCreated = false;
			}
			else if (_isMaskChild)
			{
				//여기서도 RenderCamera가 초기화된다.
				if (_renderCamera != null)
				{
					_renderCamera.Clear();
				}

#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering -= ProcessSRP_MaskChild;
#endif
				//_isRenderMaskEventRegistered = false;
				
			}

			
			//_renderCamera = null;
		}

		public void ReleaseRenderEvents()
		{
			//RenderTexture는 그대로 둔 상태로
			//렌더링 관련 이벤트와 커맨드 버퍼만 삭제한다.

			if (_isMaskParent)
			{
				if (_renderCamera != null)
				{
					_renderCamera.ReleaseEvents();
				}

#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering -= ProcessSRP_MaskParent;
#endif
				//_isRenderTextureCreated = false;
			}
			else if (_isMaskChild)
			{
				//여기서도 RenderCamera가 초기화된다.
				if (_renderCamera != null)
				{
					_renderCamera.ReleaseEvents();
				}

#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering -= ProcessSRP_MaskChild;
#endif
				//_isRenderMaskEventRegistered = false;
				
			}
		}

		

		private void Initialize_MaskParent()
		{
			if(!_isMaskParent)
			{
				//CleanUpMaskParent();
				ClearCameraData();//변경
				return;
			}

			if (_renderCamera == null)
			{
				_renderCamera = new apOptMeshRenderCamera(this);
				//_isRenderTextureCreated = false;
			}

			if(_renderCamera.IsAnyCameraChanged(_portrait.GetMainCamera()))
			{
				//카메라에 변동이 있다.
				//_isRenderTextureCreated = false;

				//변경 19.9.24 : 멀티 카메라 지원
				ClearCameraData();
			}

			if(_renderCamera.GetStatus() == apOptMeshRenderCamera.STATUS.RT_Events)
			//if(_isRenderTextureCreated)
			{
				//이미 생성되었으면 포기
				//만약 다시 세팅하고 싶다면 RenderTexture 초기화 함수를 호출하자
				return;
			}

			if(//_material == null 
				_material_Cur == null
				|| _materialAlphaMask == null 
				|| _mesh == null)
			{
				//재질 생성이 안되었다면 포기
				//Debug.LogError("No Init [" + this.name + "]");
				return;
			}

			#region [미사용 코드] 이전 : 단일 카메라만 지원
			//			Camera[] cameras = Camera.allCameras;
			//			if(cameras == null || cameras.Length == 0)
			//			{
			//				//Debug.LogError("NoCamera");
			//				return;
			//			}

			//			Camera targetCam = null;
			//			Camera cam = null;
			//			int layer = gameObject.layer;
			//			//이걸 바라보는 카메라가 하나 있으면 이걸로 설정.
			//			for (int i = 0; i < cameras.Length; i++)
			//			{
			//				cam = cameras[i];

			//				if(cam.cullingMask == (cam.cullingMask | (1 << gameObject.layer)) && cam.enabled)
			//				{
			//					//이 카메라는 이 객체를 바라본다.
			//					targetCam = cam;
			//					break;
			//				}
			//			}

			//			if(targetCam == null)
			//			{
			//				//잉?
			//				//유효한 카메라가 없네요.
			//				//Clean하고 초기화
			//				//Debug.LogError("NoCamera To Render");
			//				CleanUpMaskParent();
			//				return;
			//			}

			//			_targetCamera = targetCam;
			//			cameraTransform = _targetCamera.transform;


			//			if(_maskRenderTexture == null)
			//			{
			//				_maskRenderTexture = RenderTexture.GetTemporary(_clippingRenderTextureSize, _clippingRenderTextureSize, 24, RenderTextureFormat.Default);
			//				_maskRenderTargetID = new RenderTargetIdentifier(_maskRenderTexture);
			//			}

			//			//_materialAlphaMask.SetTexture(_shaderID_MainTex, _material.mainTexture);
			//			//_materialAlphaMask.SetColor(_shaderID_Color, _material.color);

			//			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material_Cur.mainTexture);
			//			_materialAlphaMask.SetColor(_shaderID_Color, _material_Cur.color);

			//			_commandBuffer = new CommandBuffer();
			//			_commandBuffer.name = "AP Clipping Mask [" + name + "]";
			//			_commandBuffer.SetRenderTarget(_maskRenderTargetID, 0);
			//			_commandBuffer.ClearRenderTarget(true, true, Color.clear);

			//			//일단은 기본값
			//			_vertPosCenter = Vector2.zero;
			//			//_vertRangeMax = -1.0f;

			//			_maskScreenSpaceOffset.x = 0;
			//			_maskScreenSpaceOffset.y = 0;
			//			_maskScreenSpaceOffset.z = 1;
			//			_maskScreenSpaceOffset.w = 1;



			//			_commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);


			//#if UNITY_2019_1_OR_NEWER
			//			if(_isUseSRP)
			//			{
			//				RenderPipelineManager.beginCameraRendering += ProcessSRP;
			//			}
			//			else
			//			{
			//				_targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
			//			}
			//#else
			//			_targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
			//#endif 
			#endregion
			
			
			_renderCamera.Refresh(true, _isUseSRP, _portrait.GetMainCamera());
			
			//변경된 코드
			if (!_renderCamera.IsValid)
			{
				return;
			}

			//일단은 기본값
			_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.None;
			_funcUpdateCommandBuffer = null;
			_vertPosCenter = Vector2.zero;

			_renderCamera.MakeRenderTextureAndCommandBuffer(_clippingRenderTextureSize, "AP Clipping Mask [" + name + "]");

			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material_Cur.mainTexture);
			_materialAlphaMask.SetColor(_shaderID_Color, _material_Cur.color);
			

			

			if(_renderCamera.IsVRSupported())
			{
				//VR인 경우 : Command Buffer를 렌더 이벤트에서 갱신 한다.
				//SRP가 아니라면 렌더 이벤트에 추가한다.
#if UNITY_2019_1_OR_NEWER
				if (_isUseSRP)
				{
					//SRP 이벤트에서 커맨드 버퍼 갱신
					_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Parent_SRP;
					
				}
				else
				{
					//OnPreRendered 이벤트에서 
					_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Parent_OnPreRendered;
				}
#else
				_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Parent_OnPreRendered;
#endif

				//SRP이벤트나 렌더 이벤트에서 갱신할 커맨드 버퍼 갱신 함수는 Single / Multiple VR에 따라 다르다.
				if(_renderCamera.GetRenderCameraType() == apOptMeshRenderCamera.RenderCameraType.Single_VR)
				{
					//Single Camera VR인 경우
					//Debug.Log("[Mask : " + name + "] Single VR");
					_funcUpdateCommandBuffer = UpdateCommandBuffer_SingleCameraVR;
				}
				else
				{
					//Multiple Camera VR인 경우
					//Debug.Log("[Mask : " + name + "] Multiple VR");
					_funcUpdateCommandBuffer = UpdateCommandBuffer_MultipleCameraVR;
				}
				
			}
			else
			{
				//일반적인 경우 : Command Buffer를 Calculate에서 갱신한다.
				//Debug.Log("[Mask : " + name + "] Basic");
				_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Parent_Calculate;
				_funcUpdateCommandBuffer = UpdateCommandBuffer_Basic;
			}

			
#if UNITY_2019_1_OR_NEWER
			if (_isUseSRP)
			{
				RenderPipelineManager.beginCameraRendering += ProcessSRP_MaskParent;
			}
			else
			{
				_renderCamera.AddCommandBufferToCamera();
				if(_clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Parent_OnPreRendered)
				{
					//렌더 이벤트에서 커맨드 버퍼를 갱신해야하는 경우
					_renderCamera.SetPreRenderedEvent(OnMeshPreRendered_MaskParent);
				}
			}
#else
			_renderCamera.AddCommandBufferToCamera();
			if(_clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Parent_OnPreRendered)
			{
				//렌더 이벤트에서 커맨드 버퍼를 갱신해야하는 경우
				_renderCamera.SetPreRenderedEvent(OnMeshPreRendered_MaskParent);
			}
#endif
			
			//_isRenderTextureCreated = true;
			_renderCamera.SetStatus_RTEvent();
		}


		private void Initialize_MaskChild()
		{
			
			if (!_isMaskChild)
			{
				//CleanUpMaskParent();
				ClearCameraData();//변경
				return;
			}

			if (_renderCamera == null)
			{
				_renderCamera = new apOptMeshRenderCamera(this);
				//_isRenderMaskEventRegistered = false;
			}

			if(_renderCamera.IsAnyCameraChanged(_portrait.GetMainCamera()))
			{
				//카메라에 변동이 있다.
				//_isRenderMaskEventRegistered = false;

				//변경 19.9.24 : 멀티 카메라 지원
				ClearCameraData();
			}

			//if(_isRenderMaskEventRegistered)
			if(_renderCamera.GetStatus() == apOptMeshRenderCamera.STATUS.RT_Events)
			{
				//이미 이벤트가 등록되었다.
				return;
			}
			
			_renderCamera.Refresh(true, _isUseSRP, _portrait.GetMainCamera());

			if (!_renderCamera.IsValid)
			{
				
				return;
			}

			_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.None;
			_funcUpdateMaskChildVR = null;

			if(_renderCamera.IsVRSupported())
			{
				//VR이 켜진 경우
#if UNITY_2019_1_OR_NEWER
				if(_isUseSRP)
				{
					_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Child_SRP;
				}
				else
				{
					_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Child_OnPreRendered;
				}
#else
				_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Child_OnPreRendered;
#endif

				if(_renderCamera.GetRenderCameraType() == apOptMeshRenderCamera.RenderCameraType.Single_VR)
				{
					//Single Camera VR인 경우
					//Debug.LogWarning(">>> [Child : " + name + "] Single VR");
					_funcUpdateMaskChildVR = UpdateMaskChild_SingleCameraVR;
				}
				else
				{
					//Multiple Camera VR인 경우
					//Debug.LogWarning(">>> [Child : " + name + "] Multiple VR");
					_funcUpdateMaskChildVR = UpdateMaskChild_MultipleCameraVR;
				}
			}
			else
			{
				//Debug.LogWarning(">>> [Child : " + name + "] Basic");
				_funcUpdateMaskChildVR = null;//<< VR 함수는 없어요
				_clippingFuncCallType = CLIPPING_FUNC_CALL_TYPE.Child_Calculate;
			}

			//if (_renderCamera.IsMultiple)
			if(_renderCamera.IsVRSupported())
			{
				//Debug.Log("<Multiple>");
				//Multiple인 경우에 한해서 카메라 이벤트를 잡는다.
				//그 외에는 그냥 Update 코드에서 갱신해도 된다. (<기존 방식)
#if UNITY_2019_1_OR_NEWER
				if (_isUseSRP)
				{
					RenderPipelineManager.beginCameraRendering += ProcessSRP_MaskChild;
				}
				else
				{
					_renderCamera.SetPreRenderedEvent(OnMeshPreRendered_MaskChild);
				}
#else
				_renderCamera.SetPreRenderedEvent(OnMeshPreRendered_MaskChild);
#endif
			}
			
			//_isRenderMaskEventRegistered = true;
			_renderCamera.SetStatus_RTEvent();
		}

		//---------------------------------------------------------------------------------
		// Update Command Buffer 함수들 (Basic, MultipleCamera, SingleVR)
		//---------------------------------------------------------------------------------

		private void UpdateCommandBuffer_Basic()
		{
			//변경 19.9.24 : 
			if (!_isVisible
				|| !_isMaskParent
				|| _renderCamera == null
				)
			{
				return;
			}

			switch (_renderCamera.GetStatus())
			{
				case apOptMeshRenderCamera.STATUS.NoCamera:
				case apOptMeshRenderCamera.STATUS.Camera:
					return;
			}

			//Render Camera를 Refresh 한다. (강제 아님)
			_renderCamera.Refresh(false, _isUseSRP, _portrait.GetMainCamera());

			//현재 Mesh의 화면상의 위치를 체크하여 적절히 "예쁘게 찍히도록" 만든다.
			//크기 비율
			//여백을 조금 추가한다.
			if(!_renderCamera.IsValid)
			{
				//렌더 카메라가 제대로 설정되지 않았다.
				return;
			}

			

			//변경 : 최적화된 RenderMask 방식과 그렇지 않은 방식을 모두 고려해야한다.
			//단일 카메라일 때
			//- 빌보드가 켜진 상태일 때 > 최적화 영역 사용 
			//- 카메라가 Orthographic 방식일 때 > 최적화 영역 사용
			//- 그 외 > 일반 영역 사용
			
			//멀티 카메라일 때
			//- 모든 카메라가 Orthographic 방식일 때 > 최적화 영역 사용
			//- 카메라가 Perspective 방식이며 모든 카메라의 forward가 동일하며 빌보드가 켜진 경우 > 최적화 영역 사용
			//- 그 외 > 일반 영역 사용
			

			bool isOptimizedMaskArea = false;

			//VR 없는 단일 카메라(기본)인 경우
			if (_renderCamera.NumCamera == 1)
			{
				if (_portrait._billboardType != apPortrait.BILLBOARD_TYPE.None)
				{
					//빌보드가 켜진 경우
					isOptimizedMaskArea = true;
				}
				else
				{
					switch (_renderCamera.GetRenderCameraType())
					{
						case apOptMeshRenderCamera.RenderCameraType.None:
							break;

						case apOptMeshRenderCamera.RenderCameraType.Single_NoVR:
							if (_renderCamera.MainData != null && _renderCamera.MainData._camera != null)
							{
								if (_renderCamera.MainData._camera.orthographic)
								{
									//카메라가 Orthographic인 경우
									isOptimizedMaskArea = true;
								}
							}
							break;
					}
				}
			}
			
			
			

			//카메라마다 커맨드 버퍼를 갱신
			int nCamera = _renderCamera.NumCamera;
			apOptMeshRenderCamera.CameraRenderData curCamData = null;
			Camera curCamera = null;
			//Debug.Log("----- Check ScreenSpace [" + name + "] -----");

			//재질 설정 먼저
			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material_Cur.mainTexture);
			_materialAlphaMask.SetColor(_shaderID_Color, _material_Cur.color);

			if (isOptimizedMaskArea)
			{
				//최적화 영역을 사용하는 경우

				//공통의 계산 코드는 여기서 미리 계산
				_cal_localPos_LT = new Vector3(_vertRange_XMin, _vertRange_YMax, 0);
				_cal_localPos_RB = new Vector3(_vertRange_XMax, _vertRange_YMin, 0);

				_cal_vertWorldPos_Center = transform.TransformPoint(_vertPosCenter);

				_cal_vertWorldPos_LT = transform.TransformPoint(_cal_localPos_LT);
				_cal_vertWorldPos_RB = transform.TransformPoint(_cal_localPos_RB);

				

				for (int iCam = 0; iCam < nCamera; iCam++)
				{
					curCamData = _renderCamera.GetCameraData(iCam);
					if (curCamData == null
						|| curCamData._camera == null
						|| !curCamData.IsRenderTextureCreated()
						|| curCamData._commandBuffer == null)
					{
						continue;
					}

					curCamera = curCamData._camera;

					if (curCamera.orthographic)
					{
						_vertPosCenter.z = 0;
					}

					_cal_screenPos_Center = curCamera.WorldToScreenPoint(_cal_vertWorldPos_Center);
					_cal_screenPos_LT = curCamera.WorldToScreenPoint(_cal_vertWorldPos_LT);
					_cal_screenPos_RB = curCamera.WorldToScreenPoint(_cal_vertWorldPos_RB);

					//변경

					float screenWidth = Mathf.Max((float)Screen.width * curCamera.rect.width, 0.001f);
					float screenHeight = Mathf.Max((float)Screen.height * curCamera.rect.height, 0.001f);
					
					Vector3 screenCenterOffect = new Vector3((float)Screen.width * (curCamera.rect.x), (float)Screen.height * (curCamera.rect.y), 0);

					//추가 19.10.26 : 렌더 타겟(TargetTexture)이 있는 경우, Screen Width / Screen Height 대신 TargetTexture 크기를 사용해야한다.
					if(curCamData._targetTexture != null)
					{
						screenWidth = curCamData._targetTexture.width;
						screenHeight = curCamData._targetTexture.height;
						screenCenterOffect.x = 0.0f;
						screenCenterOffect.y = 0.0f;

						//주의 : 카메라의 rect (Viewport Rect)가 기본값인 (0, 0, 1, 1)이 아닌 경우
						//크기와 화면 비율에 따라서 값이 바뀌지만, 이것까지 제어하진 말자
						//-ㅅ- =3
					}


					if (!curCamera.orthographic)
					{
						Vector3 centerSceenPos = _cal_screenPos_LT * 0.5f + _cal_screenPos_RB * 0.5f;

						float distLT2RB_Half = 0.5f * Mathf.Sqrt(
							(_cal_screenPos_LT.x - _cal_screenPos_RB.x) * (_cal_screenPos_LT.x - _cal_screenPos_RB.x)
							+ (_cal_screenPos_LT.y - _cal_screenPos_RB.y) * (_cal_screenPos_LT.y - _cal_screenPos_RB.y));
						distLT2RB_Half *= 1.6f;

						_cal_screenPos_LT.x = centerSceenPos.x - distLT2RB_Half;
						_cal_screenPos_LT.y = centerSceenPos.y - distLT2RB_Half;

						_cal_screenPos_RB.x = centerSceenPos.x + distLT2RB_Half;
						_cal_screenPos_RB.y = centerSceenPos.y + distLT2RB_Half;
					}



					//모든 버텍스가 화면안에 들어온다면 Sceen 좌표계 Scale이 0~1의 값을 가진다.
					_cal_prevSizeWidth = Mathf.Abs(_cal_screenPos_LT.x - _cal_screenPos_RB.x) / screenWidth;
					_cal_prevSizeHeight = Mathf.Abs(_cal_screenPos_LT.y - _cal_screenPos_RB.y) / screenHeight;

					if (_cal_prevSizeWidth < 0.001f)	{ _cal_prevSizeWidth = 0.001f; }
					if (_cal_prevSizeHeight < 0.001f)	{ _cal_prevSizeHeight = 0.001f; }


					//화면에 가득 찰 수 있도록 확대하는 비율은 W, H 중에서 "덜 확대하는 비율"로 진행한다.
					_cal_zoomScale = Mathf.Min(1.0f / _cal_prevSizeWidth, 1.0f / _cal_prevSizeHeight);

					//메시 자체를 평행이동하여 화면 중앙에 위치시켜야 한다.
					_cal_aspectRatio = screenWidth / screenHeight;

					if (curCamera.orthographic)
					{
						_cal_newOrthoSize = curCamera.orthographicSize / _cal_zoomScale;
					}
					else
					{
						float zDepth = Mathf.Abs(curCamera.worldToCameraMatrix.MultiplyPoint3x4(_cal_vertWorldPos_Center).z);

						_cal_newOrthoSize = zDepth * Mathf.Tan(curCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / _cal_zoomScale;
					}

					//다음 카메라 위치는
					//카메라가 바라보는 Ray를 역으로 쐈을때 Center -> Ray*Dist 만큼의 위치
					_cal_distCenterToCamera = Vector3.Distance(_cal_vertWorldPos_Center, curCamData._transform.position);
					_cal_nextCameraPos = _cal_vertWorldPos_Center + curCamData._transform.forward * (-_cal_distCenterToCamera);

					_cal_customWorldToCamera = Matrix4x4.TRS(_cal_nextCameraPos, curCamData._transform.rotation, Vector3.one).inverse;
					_cal_customWorldToCamera.m20 *= -1f;
					_cal_customWorldToCamera.m21 *= -1f;
					_cal_customWorldToCamera.m22 *= -1f;
					_cal_customWorldToCamera.m23 *= -1f;

					// CullingMatrix = Projection * WorldToCamera
					//원래 이 값
					_cal_customCullingMatrix = Matrix4x4.Ortho(-_cal_aspectRatio * _cal_newOrthoSize,    //Left
															_cal_aspectRatio * _cal_newOrthoSize,     //Right
															-_cal_newOrthoSize,                  //Bottom
															_cal_newOrthoSize,                   //Top
															_cal_distCenterToCamera - 10,        //Near
															_cal_distCenterToCamera + 50         //Far
															)
												* _cal_customWorldToCamera;

					
					_cal_newLocalToProjMatrix = _cal_customCullingMatrix * transform.localToWorldMatrix;
					_cal_newWorldMatrix = curCamera.cullingMatrix.inverse * _cal_newLocalToProjMatrix;

					
					curCamData._commandBuffer.Clear();
					curCamData._commandBuffer.SetRenderTarget(curCamData._RTIdentifier, 0);
					curCamData._commandBuffer.ClearRenderTarget(true, true, Color.clear);


#if UNITY_2019_1_OR_NEWER
					curCamData._commandBuffer.SetViewMatrix(curCamera.worldToCameraMatrix);
					curCamData._commandBuffer.SetProjectionMatrix(curCamera.projectionMatrix);
#endif

					curCamData._commandBuffer.DrawMesh(_mesh, _cal_newWorldMatrix, _materialAlphaMask);//원래값
																									   //curCamData._commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);//테스트값

					//ScreenSpace가 얼마나 바뀌었는가
					_cal_screenPosOffset = new Vector3((screenWidth / 2), (screenHeight / 2), 0) - _cal_screenPos_Center;//원래 값
																														 //_cal_screenPosOffset = Vector3.zero;//테스트값

					curCamData._maskScreenSpaceOffset.x = (_cal_screenPosOffset.x + screenCenterOffect.x) / screenWidth;
					curCamData._maskScreenSpaceOffset.y = (_cal_screenPosOffset.y + screenCenterOffect.y) / screenHeight;
					curCamData._maskScreenSpaceOffset.z = _cal_zoomScale;
					curCamData._maskScreenSpaceOffset.w = _cal_zoomScale;
				}
			}
			else
			{
				//최적화 영역을 사용하지 않고 바로 Render Texture를 만드는 경우

				for (int iCam = 0; iCam < nCamera; iCam++)
				{
					curCamData = _renderCamera.GetCameraData(iCam);
					if (curCamData == null
						|| curCamData._camera == null
						|| !curCamData.IsRenderTextureCreated()
						|| curCamData._commandBuffer == null)
					{
						continue;
					}

					curCamera = curCamData._camera;
					
					curCamData._commandBuffer.Clear();

					//싱글 렌더링
					curCamData._commandBuffer.SetRenderTarget(curCamData._RTIdentifier, 0);
					curCamData._commandBuffer.ClearRenderTarget(true, true, Color.clear);
						
#if UNITY_2019_1_OR_NEWER
					curCamData._commandBuffer.SetViewMatrix(curCamera.worldToCameraMatrix);
					curCamData._commandBuffer.SetProjectionMatrix(curCamera.projectionMatrix);
#endif

					curCamData._commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);//비 최적화 영역
					
					//비 최적화 영역
					curCamData._maskScreenSpaceOffset.x = 0;
					curCamData._maskScreenSpaceOffset.y = 0;
					curCamData._maskScreenSpaceOffset.z = 1.0f;
					curCamData._maskScreenSpaceOffset.w = 1.0f;
				}
			}
		}

		private void UpdateCommandBuffer_MultipleCameraVR()
		{
#region [미사용 코드] 이전 : 단일 카메라만 지원
			//			if (!_isVisible
			//				|| !_isMaskParent
			//				|| !_isRenderTextureCreated
			//				|| _commandBuffer == null
			//				|| _targetCamera == null)
			//			{
			//				return;
			//			}

			//			//현재 Mesh의 화면상의 위치를 체크하여 적절히 "예쁘게 찍히도록" 만든다.
			//			//크기 비율
			//			//여백을 조금 추가한다.
			//			if (_targetCamera.orthographic)
			//			{
			//				_vertPosCenter.z = 0;
			//			}

			//			_cal_localPos_LT = new Vector3(_vertRange_XMin, _vertRange_YMax, 0);
			//			_cal_localPos_RB = new Vector3(_vertRange_XMax, _vertRange_YMin, 0);


			//			_cal_vertWorldPos_Center = transform.TransformPoint(_vertPosCenter);

			//			_cal_vertWorldPos_LT = transform.TransformPoint(_cal_localPos_LT);
			//			_cal_vertWorldPos_RB = transform.TransformPoint(_cal_localPos_RB);

			//			_cal_screenPos_Center = _targetCamera.WorldToScreenPoint(_cal_vertWorldPos_Center);
			//			_cal_screenPos_LT = _targetCamera.WorldToScreenPoint(_cal_vertWorldPos_LT);
			//			_cal_screenPos_RB = _targetCamera.WorldToScreenPoint(_cal_vertWorldPos_RB);

			//			if (!_targetCamera.orthographic)
			//			{
			//				Vector3 centerSceenPos = _cal_screenPos_LT * 0.5f + _cal_screenPos_RB * 0.5f;
			//				float distLT2RB_Half = 0.5f * Mathf.Sqrt(
			//					(_cal_screenPos_LT.x - _cal_screenPos_RB.x) * (_cal_screenPos_LT.x - _cal_screenPos_RB.x) 
			//					+ (_cal_screenPos_LT.y - _cal_screenPos_RB.y) * (_cal_screenPos_LT.y - _cal_screenPos_RB.y));
			//				distLT2RB_Half *= 1.6f;

			//				_cal_screenPos_LT.x = centerSceenPos.x - distLT2RB_Half;
			//				_cal_screenPos_LT.y = centerSceenPos.y - distLT2RB_Half;

			//				_cal_screenPos_RB.x = centerSceenPos.x + distLT2RB_Half;
			//				_cal_screenPos_RB.y = centerSceenPos.y + distLT2RB_Half;

			//				//_cal_screenPos_LT = (_cal_screenPos_LT - centerSceenPos) * 1.6f + centerSceenPos;
			//				//_cal_screenPos_RB = (_cal_screenPos_RB - centerSceenPos) * 1.6f + centerSceenPos;
			//			}


			//			//모든 버텍스가 화면안에 들어온다면 Sceen 좌표계 Scale이 0~1의 값을 가진다.
			//			_cal_prevSizeWidth = Mathf.Abs(_cal_screenPos_LT.x - _cal_screenPos_RB.x) / (float)Screen.width;
			//			_cal_prevSizeHeight = Mathf.Abs(_cal_screenPos_LT.y - _cal_screenPos_RB.y) / (float)Screen.height;

			//			if (_cal_prevSizeWidth < 0.001f) { _cal_prevSizeWidth = 0.001f; }
			//			if (_cal_prevSizeHeight < 0.001f) { _cal_prevSizeHeight = 0.001f; }


			//			//화면에 가득 찰 수 있도록 확대하는 비율은 W, H 중에서 "덜 확대하는 비율"로 진행한다.
			//			_cal_zoomScale = Mathf.Min(1.0f / _cal_prevSizeWidth, 1.0f / _cal_prevSizeHeight);

			//			//메시 자체를 평행이동하여 화면 중앙에 위치시켜야 한다.

			//			//<<이거 속도 빠르게 하자
			//			//_materialAlphaMask.SetTexture(_shaderID_MainTex, _material.mainTexture);
			//			//_materialAlphaMask.SetColor(_shaderID_Color, _material.color);

			//			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material_Cur.mainTexture);
			//			_materialAlphaMask.SetColor(_shaderID_Color, _material_Cur.color);


			//			_cal_aspectRatio = (float)Screen.width / (float)Screen.height;
			//			if (_targetCamera.orthographic)
			//			{
			//				_cal_newOrthoSize = _targetCamera.orthographicSize / _cal_zoomScale;
			//				//Debug.Log("Ortho Scaled Size : " + _cal_newOrthoSize + "( Center : "+ _cal_screenPos_Center + " )");
			//			}
			//			else
			//			{
			//				//_cal_newOrthoSize = _targetCamera.nearClipPlane * Mathf.Tan(_targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / _cal_zoomScale;
			//				float zDepth = Mathf.Abs(_targetCamera.worldToCameraMatrix.MultiplyPoint3x4(_cal_vertWorldPos_Center).z);

			//				_cal_newOrthoSize = zDepth * Mathf.Tan(_targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / _cal_zoomScale;
			//				//Debug.Log("Ortho Scaled Size (Pers) : " + _cal_newOrthoSize + "( Center : " + _cal_screenPos_Center + " ) " + gameObject.name);

			//			}


			//			_cal_centerMoveOffset = new Vector2(_cal_screenPos_Center.x - (Screen.width / 2), _cal_screenPos_Center.y - (Screen.height / 2));
			//			_cal_centerMoveOffset.x /= (float)Screen.width;
			//			_cal_centerMoveOffset.y /= (float)Screen.height;

			//			_cal_centerMoveOffset.x *= _cal_aspectRatio * _cal_newOrthoSize;
			//			_cal_centerMoveOffset.y *= _cal_newOrthoSize;

			//			//다음 카메라 위치는
			//			//카메라가 바라보는 Ray를 역으로 쐈을때 Center -> Ray*Dist 만큼의 위치
			//			_cal_distCenterToCamera = Vector3.Distance(_cal_vertWorldPos_Center, _targetCamera.transform.position);
			//			_cal_nextCameraPos = _cal_vertWorldPos_Center + _targetCamera.transform.forward * (-_cal_distCenterToCamera);
			//			//_cal_camOffset = _cal_vertWorldPos_Center - _targetCamera.transform.position;

			//			_cal_customWorldToCamera = Matrix4x4.TRS(_cal_nextCameraPos, cameraTransform.rotation, Vector3.one).inverse;
			//			_cal_customWorldToCamera.m20 *= -1f;
			//			_cal_customWorldToCamera.m21 *= -1f;
			//			_cal_customWorldToCamera.m22 *= -1f;
			//			_cal_customWorldToCamera.m23 *= -1f;

			//			// CullingMatrix = Projection * WorldToCamera
			//			_cal_customCullingMatrix = Matrix4x4.Ortho(	-_cal_aspectRatio * _cal_newOrthoSize,    //Left
			//													_cal_aspectRatio * _cal_newOrthoSize,     //Right
			//													-_cal_newOrthoSize,                  //Bottom
			//													_cal_newOrthoSize,                   //Top
			//													//_targetCamera.nearClipPlane, _targetCamera.farClipPlane
			//													_cal_distCenterToCamera - 10,        //Near
			//													_cal_distCenterToCamera + 50         //Far
			//													)
			//								* _cal_customWorldToCamera;


			//			_cal_newLocalToProjMatrix = _cal_customCullingMatrix * transform.localToWorldMatrix;
			//			_cal_newWorldMatrix = _targetCamera.cullingMatrix.inverse * _cal_newLocalToProjMatrix;

			//			_commandBuffer.Clear();
			//			_commandBuffer.SetRenderTarget(_maskRenderTargetID, 0);
			//			_commandBuffer.ClearRenderTarget(true, true, Color.clear);

			//#if UNITY_2019_1_OR_NEWER
			//			_commandBuffer.SetViewMatrix(_targetCamera.worldToCameraMatrix);
			//			_commandBuffer.SetProjectionMatrix(_targetCamera.projectionMatrix);
			//#endif

			//			_commandBuffer.DrawMesh(_mesh, _cal_newWorldMatrix, _materialAlphaMask);

			//			//ScreenSpace가 얼마나 바뀌었는가
			//			_cal_screenPosOffset = new Vector3(Screen.width / 2, Screen.height / 2, 0) - _cal_screenPos_Center;

			//			_maskScreenSpaceOffset.x = (_cal_screenPosOffset.x / (float)Screen.width);
			//			_maskScreenSpaceOffset.y = (_cal_screenPosOffset.y / (float)Screen.height);
			//			_maskScreenSpaceOffset.z = _cal_zoomScale;
			//			_maskScreenSpaceOffset.w = _cal_zoomScale; 
#endregion

			//변경 19.9.24 : 
			if (!_isVisible
				|| !_isMaskParent
				|| _renderCamera == null
				)
			{
				return;
			}

			switch (_renderCamera.GetStatus())
			{
				case apOptMeshRenderCamera.STATUS.NoCamera:
				case apOptMeshRenderCamera.STATUS.Camera:
					return;
			}

			//Render Camera를 Refresh 한다. (강제 아님)
			_renderCamera.Refresh(false, _isUseSRP, _portrait.GetMainCamera());

			//현재 Mesh의 화면상의 위치를 체크하여 적절히 "예쁘게 찍히도록" 만든다.
			//크기 비율
			//여백을 조금 추가한다.
			if(!_renderCamera.IsValid)
			{
				//렌더 카메라가 제대로 설정되지 않았다.
				return;
			}

			

			//변경 : 최적화된 RenderMask 방식과 그렇지 않은 방식을 모두 고려해야한다.
			//단일 카메라일 때
			//- 빌보드가 켜진 상태일 때 > 최적화 영역 사용 
			//- 카메라가 Orthographic 방식일 때 > 최적화 영역 사용
			//- 그 외 > 일반 영역 사용
			
			//멀티 카메라일 때
			//- 모든 카메라가 Orthographic 방식일 때 > 최적화 영역 사용
			//- 카메라가 Perspective 방식이며 모든 카메라의 forward가 동일하며 빌보드가 켜진 경우 > 최적화 영역 사용
			//- 그 외 > 일반 영역 사용
			

			bool isOptimizedMaskArea = false;

			if (_renderCamera.IsAllCameraOrthographic
				|| (_renderCamera.IsAllSameForward && _portrait._billboardType != apPortrait.BILLBOARD_TYPE.None))
			{
				//다중 카메라인 경우
				isOptimizedMaskArea = true;
			}
			

			//카메라마다 커맨드 버퍼를 갱신
			int nCamera = _renderCamera.NumCamera;
			apOptMeshRenderCamera.CameraRenderData curCamData = null;
			Camera curCamera = null;
			//Debug.Log("----- Check ScreenSpace [" + name + "] -----");

			//재질 설정 먼저
			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material_Cur.mainTexture);
			_materialAlphaMask.SetColor(_shaderID_Color, _material_Cur.color);

			//Debug.Log("<< 커맨드 버퍼 업데이트 : " + name + " >>");

			if (isOptimizedMaskArea)
			{
				//최적화 영역을 사용하는 경우

				//공통의 계산 코드는 여기서 미리 계산
				_cal_localPos_LT = new Vector3(_vertRange_XMin, _vertRange_YMax, 0);
				_cal_localPos_RB = new Vector3(_vertRange_XMax, _vertRange_YMin, 0);

				_cal_vertWorldPos_Center = transform.TransformPoint(_vertPosCenter);

				_cal_vertWorldPos_LT = transform.TransformPoint(_cal_localPos_LT);
				_cal_vertWorldPos_RB = transform.TransformPoint(_cal_localPos_RB);

				

				for (int iCam = 0; iCam < nCamera; iCam++)
				{
					curCamData = _renderCamera.GetCameraData(iCam);
					if (curCamData == null
						|| curCamData._camera == null
						|| !curCamData.IsRenderTextureCreated()
						|| curCamData._commandBuffer == null)
					{
						continue;
					}

					curCamera = curCamData._camera;

					if (curCamera.orthographic)
					{
						_vertPosCenter.z = 0;
					}

					_cal_screenPos_Center = curCamera.WorldToScreenPoint(_cal_vertWorldPos_Center);
					_cal_screenPos_LT = curCamera.WorldToScreenPoint(_cal_vertWorldPos_LT);
					_cal_screenPos_RB = curCamera.WorldToScreenPoint(_cal_vertWorldPos_RB);

					//변경

					float screenWidth = Mathf.Max((float)Screen.width * curCamera.rect.width, 0.001f);
					float screenHeight = Mathf.Max((float)Screen.height * curCamera.rect.height, 0.001f);
					Vector3 screenCenterOffect = new Vector3((float)Screen.width * (curCamera.rect.x), (float)Screen.height * (curCamera.rect.y), 0);


					//추가 19.10.26 : 렌더 타겟(TargetTexture)이 있는 경우, Screen Width / Screen Height 대신 TargetTexture 크기를 사용해야한다.
					if(curCamData._targetTexture != null)
					{
						screenWidth = curCamData._targetTexture.width;
						screenHeight = curCamData._targetTexture.height;
						screenCenterOffect.x = 0.0f;
						screenCenterOffect.y = 0.0f;

						//주의 : 카메라의 rect (Viewport Rect)가 기본값인 (0, 0, 1, 1)이 아닌 경우
						//크기와 화면 비율에 따라서 값이 바뀌지만, 이것까지 제어하진 말자
						//-ㅅ- =3
					}


					if (!curCamera.orthographic)
					{
						Vector3 centerSceenPos = _cal_screenPos_LT * 0.5f + _cal_screenPos_RB * 0.5f;

						float distLT2RB_Half = 0.5f * Mathf.Sqrt(
							(_cal_screenPos_LT.x - _cal_screenPos_RB.x) * (_cal_screenPos_LT.x - _cal_screenPos_RB.x)
							+ (_cal_screenPos_LT.y - _cal_screenPos_RB.y) * (_cal_screenPos_LT.y - _cal_screenPos_RB.y));
						distLT2RB_Half *= 1.6f;

						_cal_screenPos_LT.x = centerSceenPos.x - distLT2RB_Half;
						_cal_screenPos_LT.y = centerSceenPos.y - distLT2RB_Half;

						_cal_screenPos_RB.x = centerSceenPos.x + distLT2RB_Half;
						_cal_screenPos_RB.y = centerSceenPos.y + distLT2RB_Half;
					}



					//모든 버텍스가 화면안에 들어온다면 Sceen 좌표계 Scale이 0~1의 값을 가진다.
					_cal_prevSizeWidth = Mathf.Abs(_cal_screenPos_LT.x - _cal_screenPos_RB.x) / screenWidth;
					_cal_prevSizeHeight = Mathf.Abs(_cal_screenPos_LT.y - _cal_screenPos_RB.y) / screenHeight;

					if (_cal_prevSizeWidth < 0.001f)	{ _cal_prevSizeWidth = 0.001f; }
					if (_cal_prevSizeHeight < 0.001f)	{ _cal_prevSizeHeight = 0.001f; }


					//화면에 가득 찰 수 있도록 확대하는 비율은 W, H 중에서 "덜 확대하는 비율"로 진행한다.
					_cal_zoomScale = Mathf.Min(1.0f / _cal_prevSizeWidth, 1.0f / _cal_prevSizeHeight);

					//메시 자체를 평행이동하여 화면 중앙에 위치시켜야 한다.
					_cal_aspectRatio = screenWidth / screenHeight;

					if (curCamera.orthographic)
					{
						_cal_newOrthoSize = curCamera.orthographicSize / _cal_zoomScale;
					}
					else
					{
						float zDepth = Mathf.Abs(curCamera.worldToCameraMatrix.MultiplyPoint3x4(_cal_vertWorldPos_Center).z);

						_cal_newOrthoSize = zDepth * Mathf.Tan(curCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / _cal_zoomScale;
					}

					//다음 카메라 위치는
					//카메라가 바라보는 Ray를 역으로 쐈을때 Center -> Ray*Dist 만큼의 위치
					_cal_distCenterToCamera = Vector3.Distance(_cal_vertWorldPos_Center, curCamData._transform.position);
					_cal_nextCameraPos = _cal_vertWorldPos_Center + curCamData._transform.forward * (-_cal_distCenterToCamera);

					_cal_customWorldToCamera = Matrix4x4.TRS(_cal_nextCameraPos, curCamData._transform.rotation, Vector3.one).inverse;
					_cal_customWorldToCamera.m20 *= -1f;
					_cal_customWorldToCamera.m21 *= -1f;
					_cal_customWorldToCamera.m22 *= -1f;
					_cal_customWorldToCamera.m23 *= -1f;

					// CullingMatrix = Projection * WorldToCamera
					//원래 이 값
					_cal_customCullingMatrix = Matrix4x4.Ortho(-_cal_aspectRatio * _cal_newOrthoSize,    //Left
															_cal_aspectRatio * _cal_newOrthoSize,     //Right
															-_cal_newOrthoSize,                  //Bottom
															_cal_newOrthoSize,                   //Top
															_cal_distCenterToCamera - 10,        //Near
															_cal_distCenterToCamera + 50         //Far
															)
												* _cal_customWorldToCamera;

					_cal_newLocalToProjMatrix = _cal_customCullingMatrix * transform.localToWorldMatrix;
					_cal_newWorldMatrix = curCamera.cullingMatrix.inverse * _cal_newLocalToProjMatrix;

					curCamData._commandBuffer.Clear();
					curCamData._commandBuffer.SetRenderTarget(curCamData._RTIdentifier, 0);
					curCamData._commandBuffer.ClearRenderTarget(true, true, Color.clear);
					
#if UNITY_2019_1_OR_NEWER
					curCamData._commandBuffer.SetViewMatrix(curCamera.worldToCameraMatrix);
					curCamData._commandBuffer.SetProjectionMatrix(curCamera.projectionMatrix);
#endif

					curCamData._commandBuffer.DrawMesh(_mesh, _cal_newWorldMatrix, _materialAlphaMask);//원래값
																									   //curCamData._commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);//테스트값

					//ScreenSpace가 얼마나 바뀌었는가
					_cal_screenPosOffset = new Vector3((screenWidth / 2), (screenHeight / 2), 0) - _cal_screenPos_Center;//원래 값
																														 //_cal_screenPosOffset = Vector3.zero;//테스트값

					
					curCamData._maskScreenSpaceOffset.x = (_cal_screenPosOffset.x + screenCenterOffect.x) / screenWidth;
					curCamData._maskScreenSpaceOffset.y = (_cal_screenPosOffset.y + screenCenterOffect.y) / screenHeight;
					curCamData._maskScreenSpaceOffset.z = _cal_zoomScale;
					curCamData._maskScreenSpaceOffset.w = _cal_zoomScale;
				}
			}
			else
			{
				//최적화 영역을 사용하지 않고 바로 Render Texture를 만드는 경우

				for (int iCam = 0; iCam < nCamera; iCam++)
				{
					curCamData = _renderCamera.GetCameraData(iCam);
					if (curCamData == null
						|| curCamData._camera == null
						|| !curCamData.IsRenderTextureCreated()
						|| curCamData._commandBuffer == null)
					{
						continue;
					}

					curCamera = curCamData._camera;
					
					curCamData._commandBuffer.Clear();

					//싱글 렌더링
					
					curCamData._commandBuffer.SetRenderTarget(curCamData._RTIdentifier, 0);
					curCamData._commandBuffer.ClearRenderTarget(true, true, Color.clear);
						
#if UNITY_2019_1_OR_NEWER
					curCamData._commandBuffer.SetViewMatrix(curCamera.worldToCameraMatrix);
					curCamData._commandBuffer.SetProjectionMatrix(curCamera.projectionMatrix);
#endif

					curCamData._commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);//비 최적화 영역
					
					curCamData._maskScreenSpaceOffset.x = 0;
					curCamData._maskScreenSpaceOffset.y = 0;
					curCamData._maskScreenSpaceOffset.z = 1.0f;
					curCamData._maskScreenSpaceOffset.w = 1.0f;
				}
			}
		}

		
		

		//유니티 VR용의 커맨드 버퍼 계산 (다수의 계산식이 생략됨)
		private void UpdateCommandBuffer_SingleCameraVR()
		{
			//변경 19.9.24 : 
			if (!_isVisible
				|| !_isMaskParent
				|| _renderCamera == null
				)
			{
				return;
			}

			switch (_renderCamera.GetStatus())
			{
				case apOptMeshRenderCamera.STATUS.NoCamera:
				case apOptMeshRenderCamera.STATUS.Camera:
					return;
			}

			//Render Camera를 Refresh 한다. (강제 아님)
			_renderCamera.Refresh(false, _isUseSRP, _portrait.GetMainCamera());

			//현재 Mesh의 화면상의 위치를 체크하여 적절히 "예쁘게 찍히도록" 만든다.
			//크기 비율
			//여백을 조금 추가한다.
			if(!_renderCamera.IsValid)
			{
				//렌더 카메라가 제대로 설정되지 않았다.
				return;
			}
			
			_materialAlphaMask.SetTexture(_shaderID_MainTex, _material_Cur.mainTexture);
			_materialAlphaMask.SetColor(_shaderID_Color, _material_Cur.color);
			
			//카메라마다 커맨드 버퍼를 갱신
			int nCamera = _renderCamera.NumCamera;
			apOptMeshRenderCamera.CameraRenderData curCamData = null;
#if UNITY_2019_1_OR_NEWER
			Camera curCamera = null;
#endif
			//Debug.Log("----- Check ScreenSpace [" + name + "] -----");

			for (int iCam = 0; iCam < nCamera; iCam++)
			{
				curCamData = _renderCamera.GetCameraData(iCam);
				if (curCamData == null
					|| curCamData._camera == null
					|| !curCamData.IsRenderTextureCreated()
					|| curCamData._commandBuffer == null)
				{
					continue;
				}
#if UNITY_2019_1_OR_NEWER
				curCamera = curCamData._camera;
#endif
					
				curCamData._commandBuffer.Clear();

				//듀얼 렌더링
				//Left Eye
				curCamData._commandBuffer.SetRenderTarget(curCamData._RTIdentifier_L, 0);
				curCamData._commandBuffer.ClearRenderTarget(true, true, Color.clear);
						
#if UNITY_2019_1_OR_NEWER
				curCamData._commandBuffer.SetViewMatrix(curCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left));
				curCamData._commandBuffer.SetProjectionMatrix(curCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
#endif
				curCamData._commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);//비 최적화 영역

				//Right Eye
				curCamData._commandBuffer.SetRenderTarget(curCamData._RTIdentifier_R, 0);
				curCamData._commandBuffer.ClearRenderTarget(true, true, Color.clear);
						
						
#if UNITY_2019_1_OR_NEWER
				curCamData._commandBuffer.SetViewMatrix(curCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right));
				curCamData._commandBuffer.SetProjectionMatrix(curCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
#endif
				curCamData._commandBuffer.DrawMesh(_mesh, transform.localToWorldMatrix, _materialAlphaMask);//비 최적화 영역
	
				//비 최적화 영역
				curCamData._maskScreenSpaceOffset.x = 0;
				curCamData._maskScreenSpaceOffset.y = 0;

				//비 최적화 영역
				curCamData._maskScreenSpaceOffset.z = 1.0f;
				curCamData._maskScreenSpaceOffset.w = 1.0f;
			}
		}



		//---------------------------------------------------------------------------------
		// Update Mask Child 함수들 (Basic / SingleVR / MultipleVR)
		//---------------------------------------------------------------------------------
		//기본 방식이거나 MultipleCamera인 경우 (호출 시점은 다르다)
		private void UpdateMaskChild_Basic()
		{
			if(_renderCamera == null || _parentOptMesh == null)
			{
				return;
			}
			if(!_renderCamera.IsValid)
			{
				return;
			}

			apOptMeshRenderCamera.CameraRenderData parentMainCamData = _parentOptMesh.MainCameraData;
			if (parentMainCamData != null)
			{
				_curParentRenderTexture = parentMainCamData._renderTexture;

				if (_curParentRenderTexture != _prevParentRenderTexture)
				{
					_material_Instanced.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);
					_prevParentRenderTexture = _curParentRenderTexture;
				}
				_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, parentMainCamData._maskScreenSpaceOffset);
			}
		}


		//Single Camera VR 방식인 경우
		private void UpdateMaskChild_SingleCameraVR(Camera camera)
		{
			if (_renderCamera == null || _parentOptMesh == null)
			{
				return;
			}
			if (!_renderCamera.IsValid || !_renderCamera.IsVRSupported())
			{
				return;
			}

			apOptMeshRenderCamera.CameraRenderData parentCamData = _parentOptMesh.GetCameraData(camera);
			if (parentCamData == null)
			{
				//Debug.LogError("OnMeshPreRendered_MaskChild >> Not Work");
				return;
			}

			//카메라 1개에 듀얼 모드일 것이다.
			if (parentCamData._renderTexture_L == null ||
				parentCamData._renderTexture_R == null)
			{
				//L, R의 두개의 렌더 텍스쳐가 모두 생성되어 있어야 한다.
				return;
			}

			_curParentRenderTexture = parentCamData._renderTexture_L;//L을 기준으로 한다.

			if (_curParentRenderTexture != _prevParentRenderTexture)
			{
				_material_Instanced.SetTexture(_shaderID_MaskTexture_L, parentCamData._renderTexture_L);
				_material_Instanced.SetTexture(_shaderID_MaskTexture_R, parentCamData._renderTexture_R);
				_prevParentRenderTexture = _curParentRenderTexture;
			}
			_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, parentCamData._maskScreenSpaceOffset);
		}

		private void UpdateMaskChild_MultipleCameraVR(Camera camera)
		{
			if (_renderCamera == null || _parentOptMesh == null)
			{
				//Debug.LogError(">> UpdateMaskChild_MultipleCameraVR Failed 1 : " + name);
				return;
			}
			if (!_renderCamera.IsValid || !_renderCamera.IsVRSupported())
			{
				//Debug.LogError(">> UpdateMaskChild_MultipleCameraVR Failed 2 : " + name);
				return;
			}

			apOptMeshRenderCamera.CameraRenderData parentCamData = _parentOptMesh.GetCameraData(camera);
			if (parentCamData == null)
			{
				//Debug.LogError("OnMeshPreRendered_MaskChild >> Not Work");
				//Debug.LogError(">> UpdateMaskChild_MultipleCameraVR Failed 3 : " + name);
				return;
			}

			//카메라 두개 이상에 1개씩의 RT만 있을 것
			if (parentCamData._renderTexture == null)
			{
				//Debug.LogError(">> UpdateMaskChild_MultipleCameraVR Failed 4 : " + name);
				return;
			}

			_curParentRenderTexture = parentCamData._renderTexture;

			if (_curParentRenderTexture != _prevParentRenderTexture)
			{
				_material_Instanced.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);
				_prevParentRenderTexture = _curParentRenderTexture;

				
			}
			_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, parentCamData._maskScreenSpaceOffset);
		}


#if UNITY_2019_1_OR_NEWER
		private void ProcessSRP_MaskParent(ScriptableRenderContext context, Camera cam)
		{
			if (!_isVisible
				|| !_isMaskParent
				|| _renderCamera == null
				)
			{
				return;
			}

			switch (_renderCamera.GetStatus())
			{
				case apOptMeshRenderCamera.STATUS.NoCamera:
				case apOptMeshRenderCamera.STATUS.Camera:
					return;
			}

			apOptMeshRenderCamera.CameraRenderData curCamData = _renderCamera.GetCameraData(cam);

			if (context == null || cam == null)
			{
				return;
			}

			if(curCamData == null || curCamData._commandBuffer == null)
			{
				return;
			}

			//만약 커맨드 버퍼를 여기서 업데이트 해야 한다면 실행하자
			if(_clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Parent_SRP
				&& _funcUpdateCommandBuffer != null)
			{
				_funcUpdateCommandBuffer();
			}

			//Debug.Log("Process SRP " + this.name);
			context.ExecuteCommandBuffer(curCamData._commandBuffer);
			context.Submit();
		}

		private void ProcessSRP_MaskChild(ScriptableRenderContext context, Camera cam)
		{
			if (!_isVisible
				|| !_isMaskChild
				|| _renderCamera == null
				|| _parentOptMesh == null
				)
			{
				return;
			}

			switch (_renderCamera.GetStatus())
			{
				case apOptMeshRenderCamera.STATUS.NoCamera:
				case apOptMeshRenderCamera.STATUS.Camera:
					return;
			}
			
			//if (!_renderCamera.IsVRSupported())
			//{
			//	//단일 카메라인 경우 굳이 할 필요는 없다.
			//	return;
			//}

			if (context == null || cam == null)
			{
				return;
			}

			if(_clippingFuncCallType == CLIPPING_FUNC_CALL_TYPE.Child_SRP
				&& _funcUpdateMaskChildVR != null)
			{
				_funcUpdateMaskChildVR(cam);
			}
			
			////현재 렌더링되는 카메라에 맞게 렌더링을 하자.
			//apOptMeshRenderCamera.CameraRenderData parentCamData = _parentOptMesh.GetCameraData(cam);
			//if(parentCamData == null
			//	|| parentCamData._renderTexture == null
			//	|| parentCamData._camera == null
			//	|| !parentCamData._camera.enabled)
			//{
			//	return;
			//}

			//_curParentRenderTexture = parentCamData._renderTexture;

			//if (_curParentRenderTexture != _prevParentRenderTexture)
			//{
			//	_material_Instanced.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);
			//	_prevParentRenderTexture = _curParentRenderTexture;
			//}
			//_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, parentCamData._maskScreenSpaceOffset);
			
		}
#endif

		private void OnMeshPreRendered_MaskParent(Camera camera)
		{
			if(!_isMaskParent 
				|| camera == null 
				|| _clippingFuncCallType != CLIPPING_FUNC_CALL_TYPE.Parent_OnPreRendered
				|| _funcUpdateCommandBuffer == null)
			{
				return;
			}

			//렌더링 전에 커맨드 버퍼를 갱신하자
			_funcUpdateCommandBuffer();
		}


		private void OnMeshPreRendered_MaskChild(Camera camera)
		{
			if(!_isMaskChild
				|| camera == null
				|| _clippingFuncCallType != CLIPPING_FUNC_CALL_TYPE.Child_OnPreRendered
				|| _funcUpdateMaskChildVR == null)
			{
				//Debug.LogError(">> OnMeshPreRendered_MaskChild Failed : " + this.name);
				return;
			}

			//Mask Child의 렌더링 텍스쳐를 갱신하자.
			_funcUpdateMaskChildVR(camera);

#region [미사용 코드]
			//if (!_renderCamera.IsVRSupported())
			//{
			//	//단일 카메라인 경우 굳이 할 필요는 없다.
			//	return;
			//}
			
			////현재 렌더링되는 카메라에 맞게 렌더링을 하자.
			//apOptMeshRenderCamera.CameraRenderData parentCamData = _parentOptMesh.GetCameraData(camera);
			//if(parentCamData == null)
			//{
			//	//Debug.LogError("OnMeshPreRendered_MaskChild >> Not Work");
			//	return;
			//}

			//if (_renderCamera.VRSupportMode == apPortrait.VR_SUPPORT_MODE.SingleCamera)
			//{
			//	//카메라 1개에 듀얼 모드일 것이다.
			//	if (parentCamData._renderTexture_L == null ||
			//		parentCamData._renderTexture_R == null)
			//	{
			//		//L, R의 두개의 렌더 텍스쳐가 모두 생성되어 있어야 한다.
			//		return;
			//	}

			//	_curParentRenderTexture = parentCamData._renderTexture_L;//L을 기준으로 한다.

			//	if (_curParentRenderTexture != _prevParentRenderTexture)
			//	{
			//		_material_Instanced.SetTexture("_MaskTex_L", parentCamData._renderTexture_L);
			//		_material_Instanced.SetTexture("_MaskTex_R", parentCamData._renderTexture_R);
			//		_prevParentRenderTexture = _curParentRenderTexture;
			//	}
			//	_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, parentCamData._maskScreenSpaceOffset);
			//}
			//else
			//{
			//	//카메라 두개 이상에 1개씩의 RT만 있을 것
			//	if (parentCamData._renderTexture == null)
			//	{
			//		return;
			//	}

			//	_curParentRenderTexture = parentCamData._renderTexture;

			//	if (_curParentRenderTexture != _prevParentRenderTexture)
			//	{
			//		_material_Instanced.SetTexture(_shaderID_MaskTexture, _curParentRenderTexture);
			//		_prevParentRenderTexture = _curParentRenderTexture;
			//	}
			//	_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, parentCamData._maskScreenSpaceOffset);
			//} 
			#endregion



		}

		// Functions
		//------------------------------------------------
		/// <summary>
		/// Show Mesh
		/// </summary>
		/// <param name="isResetHideFlag"></param>
		public void Show(bool isResetHideFlag = false)
		{	
			if(isResetHideFlag)
			{
				_isHide_External = false;
			}
			_meshRenderer.enabled = true;
			_isVisible = true;

			if (_isMaskParent)
			{
				//CleanUpMaskParent();
				//ClearCameraData();//변경

#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return;
				}
#endif
				Initialize_MaskParent();
			}
			else if(_isMaskChild)
			{
				//추가됨 19.9.24
				//ClearCameraData();//변경
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return;
				}
#endif
				Initialize_MaskChild();
			}

			_isUseRiggingCache = false;
		}

		/// <summary>
		/// Hide Mesh
		/// </summary>
		public void Hide()
		{
			_meshRenderer.enabled = false;
			_isVisible = false;

			if (_isMaskParent 
				|| _isMaskChild//추가
				)
			{
				//CleanUpMaskParent();
				//ClearCameraData();//변경
				ReleaseRenderEvents();//<<다시 변경. RT는 그대로 두고 이벤트만 날린다.
			}

			_isUseRiggingCache = false;
		}

		/// <summary>
		/// Show or Hide by default
		/// </summary>
		public void SetVisibleByDefault()
		{
			if(_isVisibleDefault)
			{
				Show(true);
			}
			else
			{
				Hide();
			}
		}

		/// <summary>
		/// Hide Mesh ignoring the result
		/// </summary>
		/// <param name="isHide"></param>
		public void SetHideForce(bool isHide)
		{
			_isHide_External = isHide;

			//실제 Visible 갱신은 다음 프레임의 업데이트때 수행된다.
		}





		//---------------------------------------------------------
		// Shader 제어 함수들
		//---------------------------------------------------------
		//추가 12.14
		// 각 함수에 isOverlapBatchedProperty 파라미터가 추가
		// 값이 true라면 -> Instanced 재질 값을 계산할 때 Batch의 색상 속성을 무시한다.
		//
		/// <summary>
		/// Set Main Color (2X)
		/// </summary>
		/// <param name="color2X"></param>
		/// <param name="isOverlapBatchedProperty"></param>
		public void SetMeshColor(Color color2X)
		{
			
			_multiplyColor = color2X;
			
			if(Mathf.Abs(_multiplyColor.r - 0.5f) < 0.004f &&
				Mathf.Abs(_multiplyColor.g - 0.5f) < 0.004f &&
				Mathf.Abs(_multiplyColor.b - 0.5f) < 0.004f &&
				Mathf.Abs(_multiplyColor.a - 1.0f) < 0.004f)
			{
				//기본 값이라면
				_isAnyMeshColorRequest = false;
				_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}
			else
			{
				_isAnyMeshColorRequest = true;
			}

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			//_material_Instanced.SetColor(_shaderID_MainTex, _cal_MeshColor);//버그
			_material_Instanced.SetColor(_shaderID_Color, _cal_MeshColor);//수정

			AutoSelectMaterial(true);//색상 변경 요청시에는 Gray 체크를 한번 더 해야한다.
		}

		public void SetMeshAlpha(float alpha)
		{
			_multiplyColor.a = alpha;
			
			if(Mathf.Abs(_multiplyColor.r - 0.5f) < 0.004f &&
				Mathf.Abs(_multiplyColor.g - 0.5f) < 0.004f &&
				Mathf.Abs(_multiplyColor.b - 0.5f) < 0.004f &&
				Mathf.Abs(_multiplyColor.a - 1.0f) < 0.004f)
			{
				//기본 값이라면
				_isAnyMeshColorRequest = false;
				_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}
			else
			{
				_isAnyMeshColorRequest = true;
			}

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			_material_Instanced.SetColor(_shaderID_MainTex, _cal_MeshColor);

			AutoSelectMaterial(true);//색상 변경 요청시에는 Gray 체크를 한번 더 해야한다.
		}

		/// <summary>
		/// Set Main Texture
		/// </summary>
		/// <param name="texture"></param>
		public void SetMeshTexture(Texture2D texture)
		{
			//추가 : 20.4.21 : 텍스쳐의 모드에 따라 작동 방식이 다르다.
			//- Base : 기존과 같다. _texture_Base에 저장을 하여 언제든 다시 복구할 수 있게 만든다.
			//- Extra : _texture_Base에 저장은 하지만 적용은 하지 않는다.

			//공통적으로 _texture_Base에 저장을 한다.
			_texture_Base = texture;

			if (_textureMode == TEXTURE_MODE.Base)
			{
				//Base 모드일 때
				//동일하게 적용을 한다.

				if (_isMaskChild)
				{
					//Mask Child라면 그냥 Instanced Material에 넣는다.
					_material_Instanced.SetTexture(_shaderID_MainTex, texture);
				}
				else
				{
					//그 외에는 Shared Material과 비교한다.
					if (_material_Shared.mainTexture == texture)
					{
						_isAnyTextureRequest = false;
					}
					else
					{
						_isAnyTextureRequest = true;
					}
					_material_Instanced.SetTexture(_shaderID_MainTex, texture);
				}

				AutoSelectMaterial();
			}
		}

		/// <summary>
		/// Set Color as shader property (not Main Color)
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyName"></param>
		public void SetCustomColor(Color color, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//_instanceMaterial.SetColor(propertyName, color);//이전
			_material_Instanced.SetColor(propertyName, color);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Color as shader property (not Main Color)
		/// </summary>
		/// <param name="color"></param>
		public void SetCustomColor(Color color, int propertyNameID)//ID 버전 [v1.4.3]
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_material_Instanced.SetColor(propertyNameID, color);

			AutoSelectMaterial();
		}



		/// <summary>
		/// Set Alpha as shader property (not Main Color)
		/// </summary>
		/// <param name="color"></param>
		/// <param name="propertyName"></param>
		public void SetCustomAlpha(float alpha, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//Color color = _instanceMaterial.GetColor(propertyName);
			//color.a = alpha;
			//_instanceMaterial.SetColor(propertyName, color);

			//변경 : 현재 Color -> Alpha 변경 -> Instanced에 전달
			Color color = _material_Cur.GetColor(propertyName);
			color.a = alpha;
			_material_Instanced.SetColor(propertyName, color);

			AutoSelectMaterial();
		}



		/// <summary>
		/// Set Alpha as shader property (not Main Color)
		/// </summary>
		public void SetCustomAlpha(float alpha, int propertyNameID)//ID 버전 [v1.4.3]
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			Color color = _material_Cur.GetColor(propertyNameID);
			color.a = alpha;
			_material_Instanced.SetColor(propertyNameID, color);

			AutoSelectMaterial();
		}



		/// <summary>
		/// Set Texture as shader property (not Main Texture)
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="propertyName"></param>
		public void SetCustomTexture(Texture2D texture, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//_instanceMaterial.SetTexture(propertyName, texture);//이전
			_material_Instanced.SetTexture(propertyName, texture);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Texture as shader property (not Main Texture)
		/// </summary>
		public void SetCustomTexture(Texture2D texture, int propertyNameID)//ID 버전 [v1.4.3]
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_material_Instanced.SetTexture(propertyNameID, texture);

			AutoSelectMaterial();
		}



		/// <summary>
		/// Set Float Value as shader property
		/// </summary>
		/// <param name="floatValue"></param>
		/// <param name="propertyName"></param>
		public void SetCustomFloat(float floatValue, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//_instanceMaterial.SetFloat(propertyName, floatValue);//이전
			_material_Instanced.SetFloat(propertyName, floatValue);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Float Value as shader property
		/// </summary>
		/// <param name="floatValue"></param>
		public void SetCustomFloat(float floatValue, int propertyNameID)//ID 버전 [v1.4.3]
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_material_Instanced.SetFloat(propertyNameID, floatValue);

			AutoSelectMaterial();
		}


		/// <summary>
		/// Set Int Value as shader property
		/// </summary>
		/// <param name="intValue"></param>
		/// <param name="propertyName"></param>
		public void SetCustomInt(int intValue, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//_instanceMaterial.SetInt(propertyName, intValue);//이전
			_material_Instanced.SetInt(propertyName, intValue);

			AutoSelectMaterial();
		}


		/// <summary>
		/// Set Int Value as shader property
		/// </summary>
		/// <param name="intValue"></param>
		public void SetCustomInt(int intValue, int propertyNameID)//ID 버전 [v1.4.3]
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_material_Instanced.SetInt(propertyNameID, intValue);

			AutoSelectMaterial();
		}



		/// <summary>
		/// Set Vector4 Value as shader property
		/// </summary>
		/// <param name="vector4Value"></param>
		/// <param name="propertyName"></param>
		public void SetCustomVector4(Vector4 vector4Value, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//_instanceMaterial.SetVector(propertyName, vector4Value);//이전
			_material_Instanced.SetVector(propertyName, vector4Value);

			AutoSelectMaterial();
		}

		/// <summary>
		/// Set Vector4 Value as shader property
		/// </summary>
		/// <param name="vector4Value"></param>
		public void SetCustomVector4(Vector4 vector4Value, int propertyNameID)//ID 버전 [v1.4.3]
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			_material_Instanced.SetVector(propertyNameID, vector4Value);

			AutoSelectMaterial();
		}




		// 추가 12.02 : UV Offset과 Size 조절
		/// <summary>
		/// Set UV Offset Value as shader property
		/// </summary>
		/// <param name="propertyName"></param>
		public void SetCustomTextureOffset(Vector2 uvOffset, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//_instanceMaterial.SetTextureOffset(propertyName, uvOffset);//이전
			_material_Instanced.SetTextureOffset(propertyName, uvOffset);

			AutoSelectMaterial();
		}


		/// <summary>
		/// Set UV Scale Value as shader property
		/// </summary>
		/// <param name="propertyName"></param>
		public void SetCustomTextureScale(Vector2 uvScale, string propertyName)
		{
			//값에 상관없이 이 함수가 호출되면 True
			_isAnyCustomPropertyRequest = true;
			//_instanceMaterial.SetTextureScale(propertyName, uvScale);//이전
			_material_Instanced.SetTextureScale(propertyName, uvScale);

			AutoSelectMaterial();
		}





		private void AutoSelectMaterial(bool isCheckGrayColor = false)
		{
#if UNITY_EDITOR
			if(!Application.isPlaying)
			{
				return;
			}
#endif
			if(_isMaskChild)
			{
				//Mask Child는 무조건 Instanced를 이용한다.
				if(_materialType != MATERIAL_TYPE.Instanced)
				{
					_materialType = MATERIAL_TYPE.Instanced;
					_material_Cur = _material_Instanced;
					_meshRenderer.sharedMaterial = _material_Cur;
				}
				
				return;
			}

			//Batch된 재질이 작동하고 있는가 (Merged가 아닐 때에만 동작)
			bool isBatched = false;
			if (!_isMerged)
			{
				if (_materialUnit_Batched != null && _materialUnit_Batched.IsAnyChanged)
				{
					isBatched = true;
				}

				if (_isForceBatch2Shared)
				{
					//강제로 Shared로 전환해야하는 옵션이 켜질 수 있다.
					isBatched = false;
				}
			}
			

			bool isColorChanged = _isAnyMeshColorRequest || _parentTransform._isAnyColorCalculated || !_isDefaultColorGray;
			if(isCheckGrayColor 
				&& isColorChanged
				&& !_isAnyTextureRequest
				&& !_isAnyCustomPropertyRequest
				)
			{
				//만약, 색상 변경 이벤트가 있었는데, (게다가 다른 이벤트는 없었다면)
				//Gray 체크 요청이 같이 왔다면 Instanced가 아니라 Shared로 바꿀 수 있을 것이다.
				if(_isMerged)
				{	
					//추가 21.12.29 : 병합된 경우엔 Merged Material과 비교해야한다.
					Color mergedColor = _material_Merged.color;

					bool isMergedColor = Mathf.Abs(_cal_MeshColor.r - mergedColor.r) < 0.004f &&
											Mathf.Abs(_cal_MeshColor.g - mergedColor.g) < 0.004f &&
											Mathf.Abs(_cal_MeshColor.b - mergedColor.b) < 0.004f &&
											Mathf.Abs(_cal_MeshColor.a - mergedColor.a) < 0.004f;

					if (isMergedColor)
					{
						//Merged 재질과 같은 색상이다.
						isColorChanged = false;
					}
				}	
				else if (isBatched)
				{
					//만약, Batch 재질이 작동하고 있고, 계산된 MeshColor가 Batch의 색상과 유사하다면, 이건 Batch 쪽으로 전환되어야 한다.
					//(색상에 한해서)

					Color batchedColor = _material_Batched.color;

					bool isBatchedColor = Mathf.Abs(_cal_MeshColor.r - batchedColor.r) < 0.004f &&
											Mathf.Abs(_cal_MeshColor.g - batchedColor.g) < 0.004f &&
											Mathf.Abs(_cal_MeshColor.b - batchedColor.b) < 0.004f &&
											Mathf.Abs(_cal_MeshColor.a - batchedColor.a) < 0.004f;

					if (isBatchedColor)
					{
						//Batch 재질과 같은 색상이다.
						isColorChanged = false;
					}
				}
				else
				{
					//일반적인 경우엔 Gray Color와 같다면 Instanced가 아닌 Shared로 전환한다.
					bool isGrayColor = Mathf.Abs(_cal_MeshColor.r - 0.5f) < 0.004f &&
								Mathf.Abs(_cal_MeshColor.g - 0.5f) < 0.004f &&
								Mathf.Abs(_cal_MeshColor.b - 0.5f) < 0.004f &&
								Mathf.Abs(_cal_MeshColor.a - 1.0f) < 0.004f;

					if (isGrayColor)
					{
						//Gray 색상이라면 색상 이벤트를 무시해도 된다.
						isColorChanged = false;
					}
				}
			}
			

			if (_isAnyTextureRequest
				|| _isAnyCustomPropertyRequest
				|| isColorChanged)
			{
				//Instance Material을 선택해야한다.
				if (_materialType != MATERIAL_TYPE.Instanced)
				{
					_materialType = MATERIAL_TYPE.Instanced;
					_material_Cur = _material_Instanced;
					_meshRenderer.sharedMaterial = _material_Cur;

					_isForceBatch2Shared = false;

					//추가 21.12.26
					//만약 재질이 병합된 상태(Merged)라면, Instanced 되기 전에 버텍스 채널 칼라가 바뀌었을 것
					//흰색으로 복원해야한다
					if(_isMerged && _vertColors_NotMerged != null)
					{
						_mesh.colors = _vertColors_NotMerged;
					}
				}
			}
			else
			{
				//Batched / Shared / Merged 중에 선택해야한다.
				//가장 우선 순위는 Merged

				//Shared Material을 선택해야한다.
				//기본적으론 Shared를 선택해야한다.
				//Batched Material의 "일괄 적용 요청"이 있었다면, Shared와 Batch 중에서 결정해야한다.

				if (_isMerged)
				{
					//병합된 재질로 변환
					if (_materialType != MATERIAL_TYPE.Merged)
					{
						_materialType = MATERIAL_TYPE.Merged;
						_material_Cur = _material_Merged;
						_meshRenderer.sharedMaterial = _material_Cur;

						//병합된 재질은 버텍스 색상도 제어해야한다.
						_mesh.colors = _vertColors_Merged;
					}
				}
				else
				{
					if (isBatched)
					{
						if (_materialType != MATERIAL_TYPE.Batched)
						{
							//-> Batched
							_materialType = MATERIAL_TYPE.Batched;
							_material_Cur = _material_Batched;
							_meshRenderer.sharedMaterial = _material_Cur;
						}
					}
					else
					{
						//가장 높은 최적화 단계인 Shared
						//아무런 변화가 없을때 동작한다.
						if (_materialType != MATERIAL_TYPE.Shared)
						{
							//-> Shared
							_materialType = MATERIAL_TYPE.Shared;
							_material_Cur = _material_Shared;
							_meshRenderer.sharedMaterial = _material_Cur;
						}
					}
				}
			}
		}


		//Material Property 값들을 초기화한다.
		//이 함수를 호출하면 MaskChild를 제외하면 Batch를 위해 SharedMaterial로 변경된다.
		/// <summary>
		/// Return the material value to its initial state. Batch rendering is enabled.
		/// </summary>
		public void ResetMaterialToBatch()
		{
			//Debug.LogError("ResetMaterialToBatch");
			if(_isMaskChild)
			{
				return;
			}

			//Debug.Log("ResetMaterialToBatch : " + this.name);
			if(_material_Shared != null)
			{
				//Shared로 변경
				_material_Instanced.CopyPropertiesFromMaterial(_material_Shared);
				_materialType = MATERIAL_TYPE.Shared;
				_material_Cur = _material_Shared;
				_meshRenderer.sharedMaterial = _material_Cur;

				//Debug.Log(">> Shared");
			}

			//텍스쳐 모드 초기화
			_textureMode = TEXTURE_MODE.Base;
			_texture_Base = _texture;

			//이전 코드
			//if(_isUseSharedMaterial)
			//{
			//	return;
			//}
			//_isUseSharedMaterial = true;
			//_material = _sharedMaterial;

			////일단 InstanceMat도 복사를 해서 리셋을 해준다.
			//_instanceMaterial.CopyPropertiesFromMaterial(_sharedMaterial);

			//_meshRenderer.sharedMaterial = _material;

			_isAnyMeshColorRequest = false;
			_isAnyTextureRequest = false;
			_isAnyCustomPropertyRequest = false;

			//중요 : Batched에서 강제로 Shared로 전환하게 만들어야 한다.
			_isForceBatch2Shared = true;

			//색상 값도 초기화
			_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			AutoSelectMaterial(true);

			
		}

		// Batch 관련 이벤트
		//-------------------------------------------------------------------------
		public void SyncMaterialPropertyByBatch_Texture(Texture2D texture)
		{
			//추가 20.4.21 : Extra 옵션인 경우엔 적용하면 안된다.
			_texture_Base = texture;

			if (_textureMode == TEXTURE_MODE.Base)
			{
				//Debug.Log("SyncMaterialPropertyByBatch_Texture : " + texture.name + " >> " + this.name + " (Clipped : " + _isMaskChild + ")");
				_material_Instanced.SetTexture(_shaderID_MainTex, texture);

				//동기화 되었으므로, Instanced 자체의 옵션은 해제 - 텍스쳐
				_isAnyTextureRequest = false;

				_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

				AutoSelectMaterial();
			}
		}

		public void SyncMaterialPropertyByBatch_Color(Color color2X)
		{
			//일단 Batch의 색상 값을 가져다 쓴다.
			//이 상태에서 다시 Parent Opt Transform의 색상과 계산을 해서 _cal_MeshColor를 계산해야한다.
			//Batch와 동기화한 것이므로 isAnyMeshColorRequest 여부는 확인하지 않는다.
			_multiplyColor = color2X;

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			//_material_Instanced.SetColor(_shaderID_Color, _multiplyColor);//<<버그
			_material_Instanced.SetColor(_shaderID_Color, _cal_MeshColor);//수정 19.10.28

			//동기화 되었으므로, Instanced 자체의 옵션은 해제 - 색상
			_isAnyMeshColorRequest = false;

			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial(true);//<<색상 계산을 해야한다.
		}

		public void SyncMaterialPropertyByBatch_CustomTexture(Texture2D texture, string propertyName)
		{
			_material_Instanced.SetTexture(propertyName, texture);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}

		public void SyncMaterialPropertyByBatch_CustomTexture(Texture2D texture, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			_material_Instanced.SetTexture(propertyNameID, texture);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}



		public void SyncMaterialPropertyByBatch_CustomTextureOffset(Vector2 offset, string propertyName)
		{
			_material_Instanced.SetTextureOffset(propertyName, offset);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}

		public void SyncMaterialPropertyByBatch_CustomTextureScale(Vector2 scale, string propertyName)
		{
			_material_Instanced.SetTextureScale(propertyName, scale);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}



		public void SyncMaterialPropertyByBatch_CustomColor(Color color, string propertyName)
		{
			_material_Instanced.SetColor(propertyName, color);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}

		public void SyncMaterialPropertyByBatch_CustomColor(Color color, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			_material_Instanced.SetColor(propertyNameID, color);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}



		public void SyncMaterialPropertyByBatch_CustomFloat(float floatValue, string propertyName)
		{
			_material_Instanced.SetFloat(propertyName, floatValue);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}
		
		public void SyncMaterialPropertyByBatch_CustomFloat(float floatValue, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			_material_Instanced.SetFloat(propertyNameID, floatValue);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}


		public void SyncMaterialPropertyByBatch_CustomInt(int intValue, string propertyName)
		{
			_material_Instanced.SetInt(propertyName, intValue);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}

		public void SyncMaterialPropertyByBatch_CustomInt(int intValue, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			_material_Instanced.SetInt(propertyNameID, intValue);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}



		public void SyncMaterialPropertyByBatch_CustomVector4(Vector4 vecValue, string propertyName)
		{
			_material_Instanced.SetVector(propertyName, vecValue);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}

		public void SyncMaterialPropertyByBatch_CustomVector4(Vector4 vecValue, int propertyNameID)//ID를 사용한 버전 [v1.4.3]
		{
			_material_Instanced.SetVector(propertyNameID, vecValue);
			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial();
		}


		
		public void SyncMaterialPropertyByBatch_Reset(Material syncMaterial)
		{
			_material_Instanced.CopyPropertiesFromMaterial(syncMaterial);


			//색상은 별도로 초기화
			_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			//Extra Texture 초기화
			_textureMode = TEXTURE_MODE.Base;
			_texture_Base = _texture;


			//동기화 되었으므로, Instanced 자체의 옵션은 해제 - 전체
			_isAnyMeshColorRequest = false;
			_isAnyTextureRequest = false;
			_isAnyCustomPropertyRequest = false;

			_isForceBatch2Shared = false;//Batch를 막는 플래그를 끄자

			AutoSelectMaterial(true);
		}


		


		//클리핑 메시에 관하여 일괄 요청을 하는 경우
		//일반적인 Sync 함수와 다르다.
		//Instance 방식을 유지해야한다.
		//-------------------------------------------------------------------------
		public void SetClippedMaterialPropertyByBatch_Texture(Texture2D texture)
		{
			if(!_isMaskChild)
			{
				return;
			}
			//Debug.Log("SyncMaterialPropertyByBatch_Clipped_Texture : " + texture.name + " >> " + this.name + " (Clipped : " + _isMaskChild + ")");
			_material_Instanced.SetTexture(_shaderID_MainTex, texture);
		}

		public void SetClippedMaterialPropertyByBatch_Color(Color color2X)
		{
			if(!_isMaskChild)
			{
				return;
			}
			//일단 Batch의 색상 값을 가져다 쓴다.
			//이 상태에서 다시 Parent Opt Transform의 색상과 계산을 해서 _cal_MeshColor를 계산해야한다.
			//Batch와 동기화한 것이므로 isAnyMeshColorRequest 여부는 확인하지 않는다.
			_multiplyColor = color2X;

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			_material_Instanced.SetColor(_shaderID_Color, _cal_MeshColor);
		}

		public void SetClippedMaterialPropertyByBatch_Alpha(float alpha)
		{
			if(!_isMaskChild)
			{
				return;
			}
			//일단 Batch의 색상 값을 가져다 쓴다.
			//이 상태에서 다시 Parent Opt Transform의 색상과 계산을 해서 _cal_MeshColor를 계산해야한다.
			//Batch와 동기화한 것이므로 isAnyMeshColorRequest 여부는 확인하지 않는다.
			_multiplyColor.a = alpha;

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			_material_Instanced.SetColor(_shaderID_Color, _cal_MeshColor);
		}


		public void SetClippedMaterialPropertyByBatch_CustomTexture(Texture2D texture, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetTexture(propertyName, texture);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetClippedMaterialPropertyByBatch_CustomTexture(Texture2D texture, int propertyNameID)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetTexture(propertyNameID, texture);
		}

		public void SetClippedMaterialPropertyByBatch_CustomTextureOffset(Vector2 offset, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetTextureOffset(propertyName, offset);
		}

		public void SetClippedMaterialPropertyByBatch_CustomTextureScale(Vector2 scale, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetTextureScale(propertyName, scale);
		}

		public void SetClippedMaterialPropertyByBatch_CustomColor(Color color, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetColor(propertyName, color);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetClippedMaterialPropertyByBatch_CustomColor(Color color, int propertyNameID)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetColor(propertyNameID, color);
		}

		public void SetClippedMaterialPropertyByBatch_CustomAlpha(float alpha, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			if(!_material_Instanced.HasProperty(propertyName))
			{
				return;
			}
			Color curColor = _material_Instanced.GetColor(propertyName);
			curColor.a = alpha;
			_material_Instanced.SetColor(propertyName, curColor);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetClippedMaterialPropertyByBatch_CustomAlpha(float alpha, int propertyNameID)
		{
			if(!_isMaskChild)
			{
				return;
			}
			if(!_material_Instanced.HasProperty(propertyNameID))
			{
				return;
			}
			Color curColor = _material_Instanced.GetColor(propertyNameID);
			curColor.a = alpha;
			_material_Instanced.SetColor(propertyNameID, curColor);
		}

		public void SetClippedMaterialPropertyByBatch_CustomFloat(float floatValue, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetFloat(propertyName, floatValue);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetClippedMaterialPropertyByBatch_CustomFloat(float floatValue, int propertyNameID)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetFloat(propertyNameID, floatValue);
		}

		public void SetClippedMaterialPropertyByBatch_CustomInt(int intValue, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetInt(propertyName, intValue);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetClippedMaterialPropertyByBatch_CustomInt(int intValue, int propertyNameID)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetInt(propertyNameID, intValue);
		}

		public void SetClippedMaterialPropertyByBatch_CustomVector4(Vector4 vecValue, string propertyName)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetVector(propertyName, vecValue);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SetClippedMaterialPropertyByBatch_CustomVector4(Vector4 vecValue, int propertyNameID)
		{
			if(!_isMaskChild)
			{
				return;
			}
			_material_Instanced.SetVector(propertyNameID, vecValue);
		}
		
		public void SetClippedMaterialPropertyByBatch_Reset(Material syncMaterial)
		{
			if(!_isMaskChild)
			{
				return;
			}

			//Clipped인 경우엔 특정 프로퍼티는 복구해서는 안된다.
			Texture maskTex = null;
			Texture maskTex_L = null;
			Texture maskTex_R = null;
			Vector4 maskScreenSpaceOffset = new Vector4(0, 0, 1, 1);
			if(_material_Instanced.HasProperty(_shaderID_MaskTexture))
			{
				maskTex = _material_Instanced.GetTexture(_shaderID_MaskTexture);
			}
			if(_material_Instanced.HasProperty(_shaderID_MaskTexture_L))
			{
				maskTex_L = _material_Instanced.GetTexture(_shaderID_MaskTexture_L);
			}
			if(_material_Instanced.HasProperty(_shaderID_MaskTexture_R))
			{
				maskTex_R = _material_Instanced.GetTexture(_shaderID_MaskTexture_R);
			}
			if(_material_Instanced.HasProperty(_shaderID_MaskScreenSpaceOffset))
			{
				maskScreenSpaceOffset = _material_Instanced.GetVector(_shaderID_MaskScreenSpaceOffset);
			}

			//속성 복사
			_material_Instanced.CopyPropertiesFromMaterial(syncMaterial);

			//색상은 별도로 초기화
			_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			_material_Instanced.SetColor(_shaderID_Color, _cal_MeshColor);



			//텍스쳐 모드 초기화
			_textureMode = TEXTURE_MODE.Base;
			_texture_Base = _texture;


			//Mask 속성 복구
			if(_material_Instanced.HasProperty(_shaderID_MaskTexture) && maskTex != null)
			{
				_material_Instanced.SetTexture(_shaderID_MaskTexture, maskTex);
			}
			if(_material_Instanced.HasProperty(_shaderID_MaskTexture_L) && maskTex_L != null)
			{
				 _material_Instanced.SetTexture(_shaderID_MaskTexture_L, maskTex_L);
			}
			if(_material_Instanced.HasProperty(_shaderID_MaskTexture_R) && maskTex_R != null)
			{
				 _material_Instanced.SetTexture(_shaderID_MaskTexture_R, maskTex_R);
			}
			if(_material_Instanced.HasProperty(_shaderID_MaskScreenSpaceOffset))
			{
				_material_Instanced.SetVector(_shaderID_MaskScreenSpaceOffset, maskScreenSpaceOffset);
			}
		}



		//-------------------------------------------------------------------------
		//// 추가 12.8 : Extra Option에 의해서 Texture가 바뀌었을 경우
		//public Texture2D GetCurrentMainTextureExceptExtra()
		//{
		//	//이전
		//	//if(_isUseSharedMaterial)
		//	//{
		//	//	return _sharedMaterial.mainTexture as Texture2D;
		//	//}
		//	//else
		//	//{
		//	//	return _instanceMaterial.mainTexture as Texture2D;
		//	//}

		//	//변경
		//	return _material_Cur.mainTexture as Texture2D;
		//}

		public void SetExtraChangedTexture(Texture2D texture)
		{
			//코드를 개선 20.4.21 : 텍스쳐 모드와 extra용 텍스쳐를 별도로 둔다.
			
			if(texture == null)
			{
				if(_isMaskChild)
				{
					//Mask Child에서는 Extra Changed가 발생할 때 Null Texture를 적용할 수 없다.
					return;
				}
				//texture = _sharedMaterial.mainTexture as Texture2D;
				texture = _material_Shared.mainTexture as Texture2D;
			}

			_textureMode = TEXTURE_MODE.Extra;//<< 중요

			if (_isMaskChild)
			{
				//Mask Child라면 무조건 Instanced 타입이다.
				_materialType = MATERIAL_TYPE.Instanced;
				_material_Cur = _material_Instanced;
				_material_Cur.SetTexture(_shaderID_MainTex, texture);
				_isAnyTextureRequest = true;
			}
			else
			{
				//Mask Child가 아니라면, Shared Material과 비교하여 다시 Shared로 바꿀지 결정한다.
				if(_material_Shared.mainTexture == texture
					|| _material_Batched.mainTexture == texture)
				{
					//Shared나 Batched로 돌아갈 수 있다. (경우에 따라서..)
					_isAnyTextureRequest = false;
				}
				else
				{
					//Shared -> Instanced (무조건)
					_isAnyTextureRequest = true;
				}

				_material_Instanced.SetTexture(_shaderID_MainTex, texture);
			}

			AutoSelectMaterial();
		}

		//추가 20.4.21 : Extra이벤트로부터 텍스쳐를 복구하고자 하는 경우
		public void RestoreFromExtraTexture()
		{
			_textureMode = TEXTURE_MODE.Base;
			if (_isMaskChild)
			{
				//Mask Child라면 무조건 Instanced 타입이다.
				_materialType = MATERIAL_TYPE.Instanced;
				_material_Cur = _material_Instanced;
				_material_Cur.SetTexture(_shaderID_MainTex, _texture_Base);
				_isAnyTextureRequest = true;
			}
			else
			{
				//Mask Child가 아니라면, Shared Material과 비교하여 다시 Shared로 바꿀지 결정한다.
				if(_material_Shared.mainTexture == _texture_Base
					|| _material_Batched.mainTexture == _texture_Base)
				{
					//Shared나 Batched로 돌아갈 수 있다. (경우에 따라서..)
					_isAnyTextureRequest = false;
				}
				else
				{
					//Shared -> Instanced (무조건)
					_isAnyTextureRequest = true;
				}

				_material_Instanced.SetTexture(_shaderID_MainTex, _texture_Base);
			}

			AutoSelectMaterial();
		}


		//추가 21.12.25 : Merged 재질
		//------------------------------------------------
		public Material GetMaterialBeforeMerge()
		{
			return _material_Instanced;
		}

		public void SetMergedMaterial(Color[] channelColors, Color[] whiteColors, Material mergedMaterial)
		{
			int vertCount = _mesh.vertices.Length;
			if(vertCount == 0)
			{
				return;
			}


			_isMerged = true;
			_material_Merged = mergedMaterial;
			_vertColors_NotMerged = new Color[vertCount];
			_vertColors_Merged = new Color[vertCount];

			if(vertCount > channelColors.Length)
			{
				//배열 복사가 불가능하다
				Debug.LogError("에러 : 버텍스 색상 배열 복사 불가 [" + this.gameObject.name + "]");
				Color targetColor = channelColors[0];
				for (int i = 0; i < vertCount; i++)
				{
					_vertColors_Merged[i] = targetColor;
				}
			}
			else
			{
				//미리 만들어진 배열에서 값을 일부 복사해서 빠르게 완성
				Array.Copy(channelColors, _vertColors_Merged, vertCount);
			}

			if(vertCount > whiteColors.Length)
			{
				//배열 복사가 불가능하다
				Debug.LogError("에러 : 버텍스 색상 배열(White) 복사 불가 [" + this.gameObject.name + "]");
				Color targetColor = whiteColors[0];
				for (int i = 0; i < vertCount; i++)
				{
					_vertColors_NotMerged[i] = targetColor;
				}
			}
			else
			{
				//미리 만들어진 배열에서 값을 일부 복사해서 빠르게 완성
				Array.Copy(whiteColors, _vertColors_NotMerged, vertCount);
			}

			
			//재질 변경
			AutoSelectMaterial(true);
		}


		//병합된 재질(Merged Material)을 해제한다.
		public void ReleaseMergedMaterial()
		{
			if(!_isMerged)
			{
				//Merged된 상태가 아니다.
				AutoSelectMaterial();
				return;
			}

			//Merge되었다면, Vertex Color를 초기화한 후 해제 
			if(_vertColors_NotMerged != null)
			{
				_mesh.colors = _vertColors_NotMerged;
			}
			

			_isMerged = false;
			_material_Merged = null;
			_vertColors_NotMerged = null;
			_vertColors_Merged = null;

			AutoSelectMaterial(true);
			
		}

		//다시 Merged 될 수 있게 재질을 초기화한다.
		public void SyncMergedMaterial_Reset(Material materialOriginal)
		{
			//다시 Merged 될 수 있게 만든다. 이 함수는 ResetMaterialToBatch 함수와 거의 유사하다.
			if(materialOriginal != null)
			{
				_material_Instanced.CopyPropertiesFromMaterial(materialOriginal);
			}
			

			//색상 초기화
			_multiplyColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			//텍스쳐 모드 초기화
			_textureMode = TEXTURE_MODE.Base;
			_texture_Base = _texture;

			_isAnyMeshColorRequest = false;
			_isAnyTextureRequest = false;
			_isAnyCustomPropertyRequest = false;

			//실제 색상 갱신
			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			//지금은 안쓰지만 Batched 설정 초기화
			_isForceBatch2Shared = true;

			//Material 선택
			AutoSelectMaterial(true);
		}


		//Merged Material과 값을 동기화한다. Instanced 상태가 아니어도, 애니메이션에 의해서 Instanced로 전환될 때 기존의 색상 값이 적용되기 위함
		public void SyncMergedMaterial_Color(ref Color color2X)
		{
			_multiplyColor = color2X;

			_cal_MeshColor.r = _multiplyColor.r * _parentTransform._meshColor2X.r * 2;
			_cal_MeshColor.g = _multiplyColor.g * _parentTransform._meshColor2X.g * 2;
			_cal_MeshColor.b = _multiplyColor.b * _parentTransform._meshColor2X.b * 2;
			_cal_MeshColor.a = _multiplyColor.a * _parentTransform._meshColor2X.a;//Alpha는 2X가 아니다.

			//동기화를 유지해야하므로, 색상 요청은 없는 셈 친다.
			_isAnyMeshColorRequest = false;

			//Instanced의 색상 값을 동기화 한다.
			_material_Instanced.SetColor(_shaderID_Color, _cal_MeshColor);

			//Material 선택
			AutoSelectMaterial(true);
		}

		public void SyncMergedMaterial_CustomImage(Texture2D texture, ref string propertyName)
		{
			//Instanced의 값을 동기화한다.
			//Merged는 유지하도록 한다.

			//Custom Property에 대해서는 원래는 이게 True여야 하지만, 그러면 동기화가 풀려버린다.
			//Merged Material에서도 동일하게 커스텀 프로퍼티를 수정하므로 Request는 해제하되 값만 할당한다.
			//_isAnyCustomPropertyRequest = false;//True도 안하고 False도 안한다.

			_material_Instanced.SetTexture(propertyName, texture);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SyncMergedMaterial_CustomImage(Texture2D texture, int propertyNameID)
		{
			_material_Instanced.SetTexture(propertyNameID, texture);
		}

		public void SyncMergedMaterial_CustomImageOffset(ref Vector2 offset, ref string propertyName)
		{
			//Instanced의 값을 동기화하며 Merged는 유지하도록 한다. (설명은 위족에)
			//_isAnyCustomPropertyRequest = false;

			_material_Instanced.SetTextureOffset(propertyName, offset);
		}

		public void SyncMergedMaterial_CustomImageScale(ref Vector2 scale, ref string propertyName)
		{
			//Instanced의 값을 동기화하며 Merged는 유지하도록 한다. (설명은 위족에)
			//_isAnyCustomPropertyRequest = false;

			_material_Instanced.SetTextureScale(propertyName, scale);
		}

		public void SyncMergedMaterial_CustomFloat(float floatValue, ref string propertyName)
		{
			//Instanced의 값을 동기화하며 Merged는 유지하도록 한다. (설명은 위족에)
			//_isAnyCustomPropertyRequest = false;

			_material_Instanced.SetFloat(propertyName, floatValue);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SyncMergedMaterial_CustomFloat(float floatValue, int propertyNameID)
		{
			_material_Instanced.SetFloat(propertyNameID, floatValue);
		}

		public void SyncMergedMaterial_CustomInt(int intValue, ref string propertyName)
		{
			//Instanced의 값을 동기화하며 Merged는 유지하도록 한다. (설명은 위족에)
			//_isAnyCustomPropertyRequest = false;

			_material_Instanced.SetInt(propertyName, intValue);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SyncMergedMaterial_CustomInt(int intValue, int propertyNameID)
		{
			_material_Instanced.SetInt(propertyNameID, intValue);
		}

		public void SyncMergedMaterial_CustomVector4(ref Vector4 vec4Value, ref string propertyName)
		{
			//Instanced의 값을 동기화하며 Merged는 유지하도록 한다. (설명은 위족에)
			//_isAnyCustomPropertyRequest = false;

			_material_Instanced.SetVector(propertyName, vec4Value);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SyncMergedMaterial_CustomVector4(ref Vector4 vec4Value, int propertyNameID)
		{
			_material_Instanced.SetVector(propertyNameID, vec4Value);
		}

		public void SyncMergedMaterial_CustomColor(ref Color color, ref string propertyName)
		{
			//Instanced의 값을 동기화하며 Merged는 유지하도록 한다. (설명은 위족에)
			//_isAnyCustomPropertyRequest = false;

			_material_Instanced.SetColor(propertyName, color);
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SyncMergedMaterial_CustomColor(ref Color color, int propertyNameID)
		{
			_material_Instanced.SetColor(propertyNameID, color);
		}

		public void SyncMergedMaterial_CustomAlpha(float alpha, ref string propertyName)
		{
			//Instanced의 값을 동기화하며 Merged는 유지하도록 한다. (설명은 위족에)
			//_isAnyCustomPropertyRequest = false;
			if(_material_Instanced.HasProperty(propertyName))
			{
				Color curColor = _material_Instanced.GetColor(propertyName);
				curColor.a = alpha;
				_material_Instanced.SetColor(propertyName, curColor);
			}
			
		}

		//ID를 사용한 버전 [v1.4.3]
		public void SyncMergedMaterial_CustomAlpha(float alpha, int propertyNameID)
		{
			if(_material_Instanced.HasProperty(propertyNameID))
			{
				Color curColor = _material_Instanced.GetColor(propertyNameID);
				curColor.a = alpha;
				_material_Instanced.SetColor(propertyNameID, curColor);
			}
			
		}


		// Get / Set
		//------------------------------------------------
		/// <summary>
		/// Calculated Mesh Color (2X)
		/// </summary>
		public Color MeshColor
		{
			get
			{
				//return _multiplyColor * 2.0f * _parentTransform._meshColor2X;
				return _cal_MeshColor;
			}
		}


		public MATERIAL_TYPE GetMaterialTypeForDebug()
		{
			return _materialType;
		}

		//---------------------------------------------------------
		// Mesh Renderer 의 Sorting Order 제어
		//---------------------------------------------------------
		public void SetSortingLayer(string sortingLayerName, int sortingLayerID)
		{
			_meshRenderer.sortingLayerName = sortingLayerName;
			_meshRenderer.sortingLayerID = sortingLayerID;
		}

		public string GetSortingLayerName()
		{
			return _meshRenderer.sortingLayerName;
		}

		public int GetSortingLayerID()
		{
			return _meshRenderer.sortingLayerID;
		}

		public void SetSortingOrder(int sortingOrder)
		{
			_meshRenderer.sortingOrder = sortingOrder;
		}

		public int GetSortingOrder()
		{
			return _meshRenderer.sortingOrder;
		}
	}
}