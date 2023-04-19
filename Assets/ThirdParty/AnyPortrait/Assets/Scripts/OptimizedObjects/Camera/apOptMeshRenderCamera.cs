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

using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// OptMesh를 바라보는 카메라에 대한 정보
	/// MaskParent, MaskChild인 경우에만 사용되며 커맨드 버퍼나 이벤트를 설정할 수 있다.
	/// 여러개의 카메라가 있다 하더라도 OptMesh에서 동일하게 함수를 사용할 수 있도록 래핑 (1개의 카메라처럼)
	/// </summary>
	public class apOptMeshRenderCamera
	{
		// Members
		//-----------------------------------------------
		
		public class CameraRenderData
		{
			//기본 데이터
			public Camera _camera = null;
			public apOptMultiCameraController _multiCamController = null;
			public Transform _transform = null;

			//Mask Parent 기준
			//렌더 텍스쳐 / 커맨드 버퍼
			private bool _isDualRT = false;//이 값이 true이면 1개의 카메라에 두개의 렌더 텍스쳐가 적용된다.

			public RenderTexture _renderTexture = null;
			public RenderTargetIdentifier _RTIdentifier;

			public RenderTexture _renderTexture_L = null;
			public RenderTexture _renderTexture_R = null;
			public RenderTargetIdentifier _RTIdentifier_L;
			public RenderTargetIdentifier _RTIdentifier_R;

#if UNITY_2019_1_OR_NEWER
			private bool _isEyeTextureSize = false;
#endif

			public CommandBuffer _commandBuffer = null;
			public Vector4 _maskScreenSpaceOffset = Vector4.zero;

			private apOptMesh _parentMesh = null;

			public bool _isUpdated = false;

			//카메라의 렌더 타겟이다.
			public RenderTexture _targetTexture = null;

			public CameraRenderData(apOptMesh parentMesh, Camera camera, bool isDualRT
#if UNITY_2019_1_OR_NEWER
				, bool isEyeTextureSize
#endif
				)
			{
				_parentMesh = parentMesh;

				_camera = camera;
				_multiCamController = null;
				_transform = _camera.transform;

				_renderTexture = null;
				_renderTexture_L = null;
				_renderTexture_R = null;
				_commandBuffer = null;

				//_debugName = _parentMesh.name + " / " + _camera.name;

				_isDualRT = isDualRT;
#if UNITY_2019_1_OR_NEWER
				_isEyeTextureSize = isEyeTextureSize;
#endif

				_targetTexture = _camera.targetTexture;
				//Debug.LogWarning(">> Create : " + _debugName + " ( IsDual : " + _isDualRT + ")");
			}



			//카메라가 복수개라면 멀티 카메라 컨트롤러를 이용해야한다.
			//단일이라면 필요 없음
			public void MakeAndLinkMultiCameraController(apOptMultiCameraController.FUNC_MESH_PRE_RENDERED funcMeshPreRendered)
			{
				_multiCamController = _camera.gameObject.GetComponent<apOptMultiCameraController>();
				if(_multiCamController == null)
				{
					_multiCamController = _camera.gameObject.AddComponent<apOptMultiCameraController>();
					_multiCamController.Init();
				}

				//함수 연결
				_multiCamController.AddPreRenderEvent(_parentMesh, funcMeshPreRendered);

				//Debug.Log("++ MultiCam 컨트롤러에 PreRendered 이벤트 등록 : " + _debugName);
			}

			public void UnlinkMultiCameraController()
			{
				if(_multiCamController != null)
				{
					_multiCamController.RemovePreRenderEvent(_parentMesh);
					_multiCamController = null;

					//Debug.LogError("-- MultiCam 컨트롤러에서 PreRendered 이벤트 제거 : " + _debugName);
				}
			}

			public void MakeRenderTextureAndCommandBuffer(int renderTextureSize, string commandBufferName)
			{
				if (_isDualRT)
				{
					//듀얼 렌더 텍스쳐
					if (_renderTexture_L == null)
					{
						_renderTexture_L = MakeRenderTexture(renderTextureSize);
						_RTIdentifier_L = new RenderTargetIdentifier(_renderTexture_L);

						//Debug.LogWarning("++ 렌더 텍스쳐 생성(Dual - Left) : " + _debugName);
					}

					if (_renderTexture_R == null)
					{
						_renderTexture_R = MakeRenderTexture(renderTextureSize);
						_RTIdentifier_R = new RenderTargetIdentifier(_renderTexture_R);

						//Debug.LogWarning("++ 렌더 텍스쳐 생성(Dual - Right) : " + _debugName);
					}
				}
				else
				{
					//싱글 렌더 텍스쳐
					if (_renderTexture == null)
					{
						_renderTexture = MakeRenderTexture(renderTextureSize);
						_RTIdentifier = new RenderTargetIdentifier(_renderTexture);

						//Debug.LogWarning("++ 렌더 텍스쳐 생성(Single) : " + _debugName);
					}
				}

				if(_commandBuffer == null)
				{
					_commandBuffer = new CommandBuffer();
					_commandBuffer.name = commandBufferName;
					_commandBuffer.SetRenderTarget(_RTIdentifier, 0);
					_commandBuffer.ClearRenderTarget(true, true, Color.clear);

					//Debug.Log("++ 커맨드 버퍼 생성 : " + _debugName);
				}
			}

			private RenderTexture MakeRenderTexture(int renderTextureSize)
			{
#if UNITY_2019_1_OR_NEWER
				if(UnityEngine.XR.XRSettings.enabled)
				{
					if(_isEyeTextureSize)
					{
						//완전히 EyeTexture와 동일하게
						//return RenderTexture.GetTemporary(UnityEngine.XR.XRSettings.eyeTextureDesc);//<<이건 정상적으로 동작하지 않는다.
						return RenderTexture.GetTemporary(	UnityEngine.XR.XRSettings.eyeTextureDesc.width, 
															UnityEngine.XR.XRSettings.eyeTextureDesc.height, 
															UnityEngine.XR.XRSettings.eyeTextureDesc.depthBufferBits, 
															UnityEngine.XR.XRSettings.eyeTextureDesc.colorFormat);
					}
					else
					{
						//크기는 설정대로, 포맷만 EyeTexture
						return RenderTexture.GetTemporary(
							renderTextureSize, renderTextureSize, 
							UnityEngine.XR.XRSettings.eyeTextureDesc.depthBufferBits, 
							UnityEngine.XR.XRSettings.eyeTextureDesc.colorFormat);
					}
				}
				else
				{
					return RenderTexture.GetTemporary(renderTextureSize, renderTextureSize, 24, RenderTextureFormat.Default);
				}
#else
				return RenderTexture.GetTemporary(renderTextureSize, renderTextureSize, 24, RenderTextureFormat.Default);
#endif
			}

			public void AddCommandBufferToCamera()
			{
				if(_camera == null || _commandBuffer == null)
				{
					return;
				}
				_camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
				//Debug.Log("++ 커맨드 버퍼를 카메라에 등록 : " + _debugName);
			}

			public void ReleaseEvent()
			{
				//Debug.LogError(">> ReleaseEvent : " + _debugName);
				//기존의 데이터를 날려야 한다.
				if(_camera != null)
				{
					if(_commandBuffer != null)
					{
						_camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
						_commandBuffer = null;

						//Debug.LogError("-- 커맨드 버퍼를 카메라에서 제거 : " + _debugName);
					}
					
				}

				if(_multiCamController != null)
				{
					_multiCamController.RemovePreRenderEvent(_parentMesh);
					_multiCamController = null;

					//Debug.LogError("-- PreRendered 이벤트를 제거 : " + _debugName);
				}
			}

			public void ReleaseAll()
			{
				//Debug.LogError(">> ReleaseAll : " + _debugName);
				//기존의 데이터를 날려야 한다.
				if(_camera != null)
				{
					if(_commandBuffer != null)
					{
						_camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, _commandBuffer);
						_commandBuffer = null;

						//Debug.LogError("-- 커맨드 버퍼를 카메라에서 제거 : " + _debugName);
					}
					
				}

				if(_multiCamController != null)
				{
					_multiCamController.RemovePreRenderEvent(_parentMesh);
					_multiCamController = null;

					//Debug.LogError("-- PreRendered 이벤트를 제거 : " + _debugName);
				}

				if(_renderTexture != null)
				{
					RenderTexture.ReleaseTemporary(_renderTexture);
					_renderTexture = null;

					//Debug.LogError("-- 렌더 텍스쳐를 삭제(Single) : " + _debugName);
				}

				if(_renderTexture_L != null)
				{
					RenderTexture.ReleaseTemporary(_renderTexture_L);
					_renderTexture_L = null;

					//Debug.LogError("-- 렌더 텍스쳐를 삭제(Dual - Left) : " + _debugName);
				}

				if(_renderTexture_R != null)
				{
					RenderTexture.ReleaseTemporary(_renderTexture_R);
					_renderTexture_R = null;

					//Debug.LogError("-- 렌더 텍스쳐를 삭제(Dual - Right) : " + _debugName);
				}

				//_camera = null;
				//_transform = null;
			}

			public void ReadyToUpdate()
			{
				_isUpdated = false;

			}

			public void Update()
			{
				_isUpdated = true;

				//추가로 갱신해야할 것들을 확인하자.
				_targetTexture = _camera.targetTexture;
			}

			public bool IsRenderTextureCreated()
			{
				if(_isDualRT)
				{
					return _renderTexture_L != null && _renderTexture_R != null;
				}
				else
				{
					return _renderTexture != null;
				}
			}
		}

		public enum RenderCameraType
		{
			None,
			Single_NoVR,
			Single_VR,
			Multiple
		}
		private RenderCameraType _renderCameraType = RenderCameraType.None;
		private RenderCameraType _renderCameraType_Prev = RenderCameraType.None;
		private int _refreshKey = -1;

		private List<CameraRenderData> _cameraRenderDataList = new List<CameraRenderData>();
		private Dictionary<Camera, CameraRenderData> _cam2CamRenderData = new Dictionary<Camera, CameraRenderData>();
		private int _nCameras = 0;

		private apOptMesh _parentOptMesh = null;
		

		//Single인 경우 : 빠른 접근을 위해서
		private CameraRenderData _mainData = null;
		

		private bool _isAllSameForward = true;
		private bool _isAllCameraOrthographic = true;
		private apPortrait.VR_SUPPORT_MODE _vrSupportMode = apPortrait.VR_SUPPORT_MODE.None;

		private CameraRenderData _cal_curCamRenderData = null;

		//생성된 상태
		public enum STATUS
		{
			NoCamera,//카메라 데이터가 없다.
			Camera,//카메라만 갱신된 상태
			RenderTexture,//렌더 텍스쳐만 생성된 상태
			RT_Events,//렌더 텍스쳐와 이벤트가 모두 생성된 상태
		}

		public STATUS _status = STATUS.NoCamera;
		private bool _isMaskParent = false;
		
		// Init
		//-----------------------------------------------
		public apOptMeshRenderCamera(apOptMesh parentOptMesh)
		{
			_parentOptMesh = parentOptMesh;

			if (_cameraRenderDataList == null)
			{
				_cameraRenderDataList = new List<CameraRenderData>();
			}

			if(_cam2CamRenderData == null)
			{
				_cam2CamRenderData = new Dictionary<Camera, CameraRenderData>();
			}

			_cameraRenderDataList.Clear();
			_cam2CamRenderData.Clear();

			Clear();

			_refreshKey = -1;
			_isMaskParent = parentOptMesh._isMaskParent;
		}

		public void Clear()
		{
			//만약 등록된 카메라가 있다면..
			ReleaseAll();

			_cameraRenderDataList.Clear();
			_cam2CamRenderData.Clear();
			_nCameras = 0;

			_renderCameraType = RenderCameraType.None;
			_renderCameraType_Prev = _renderCameraType;
			_mainData = null;

			_status = STATUS.NoCamera;
		}

		public void ReleaseAll()
		{
			if(_cameraRenderDataList == null)
			{
				_cameraRenderDataList = new List<CameraRenderData>();
				return;
			}

			CameraRenderData camData = null;
			for (int i = 0; i < _cameraRenderDataList.Count; i++)
			{
				camData = _cameraRenderDataList[i];
				camData.ReleaseAll();
			}

			//Status : NoCamera가 아니라면 Camera만 생성된 상태로 변경
			if(_status != STATUS.NoCamera)
			{
				_status = STATUS.Camera;
			}
			
		}

		public void ReleaseEvents()
		{
			if(_cameraRenderDataList == null)
			{
				_cameraRenderDataList = new List<CameraRenderData>();
				return;
			}

			CameraRenderData camData = null;
			for (int i = 0; i < _cameraRenderDataList.Count; i++)
			{
				camData = _cameraRenderDataList[i];
				camData.ReleaseEvent();
			}

			//Status : 이벤트가 생성되었다면 그 전 상태로 변경
			if (_status == STATUS.RT_Events)
			{
				if (_isMaskParent)
				{
					_status = STATUS.RenderTexture;//RT는 유지
				}
				else
				{
					_status = STATUS.Camera;//Camera만 갱신된 상태
				}
			}
		}

		// Function
		//-----------------------------------------------
		public void Refresh(bool isRefreshForce, bool isUseSRP, apOptMainCamera mainCamera)
		{
			//변경 : mainCamera에서 Refresh를 통합으로 하고, 변경 사항이 있는 경우에만 갱신
			//그대로 유지할지 확인
			if(mainCamera == null)
			{
				Clear();
				return;
			}

			//카메라 상태를 갱신하자.
			_vrSupportMode = mainCamera.VRSupportMode;
			switch (mainCamera.GetNumberOfCamera())
			{
				case apOptMainCamera.NumberOfCamera.None:
					_renderCameraType = RenderCameraType.None;
					
					break;

				case apOptMainCamera.NumberOfCamera.Single:
					if(_vrSupportMode == apPortrait.VR_SUPPORT_MODE.SingleCamera)
					{
						_renderCameraType = RenderCameraType.Single_VR;
					}
					else
					{
						_renderCameraType = RenderCameraType.Single_NoVR;
					}
					break;

				case apOptMainCamera.NumberOfCamera.Multiple:
					_renderCameraType = RenderCameraType.Multiple;
					break;
			}

			if(mainCamera._refreshKey != _refreshKey 
				|| isRefreshForce 
				|| _status == STATUS.NoCamera
				|| _renderCameraType_Prev != _renderCameraType)
			{
				//다시 갱신
				//삭제할 카메라를 찾자
				_cal_curCamRenderData = null;
				for (int i = 0; i < _nCameras; i++)
				{
					_cal_curCamRenderData = _cameraRenderDataList[i];
					_cal_curCamRenderData.ReadyToUpdate();
				}

				List<apOptMainCamera.CameraData> mainRenderCameraDataList = mainCamera.RenderCameraDataList;

				if(mainCamera._refreshKey > 0 && mainRenderCameraDataList != null)
				{
					List<CameraRenderData> addedCameraDataList = new List<CameraRenderData>();//<<추가해야하는 리스트

					int nMainRenderCam = mainRenderCameraDataList.Count;
					apOptMainCamera.CameraData curRenderCamData = null;
					for (int iMainRCD = 0; iMainRCD < nMainRenderCam; iMainRCD++)
					{
						curRenderCamData = mainRenderCameraDataList[iMainRCD];
						if(_cam2CamRenderData.ContainsKey(curRenderCamData._camera))
						{
							_cal_curCamRenderData = _cam2CamRenderData[curRenderCamData._camera];
							
							//이건 계속 업데이트 되는 카메라
							_cal_curCamRenderData.Update();
						}
						else
						{
							//새로 발견된 카메라
							CameraRenderData newCamRenderData = new CameraRenderData(	_parentOptMesh, 
																						curRenderCamData._camera, 
																						(_vrSupportMode == apPortrait.VR_SUPPORT_MODE.SingleCamera)
#if UNITY_2019_1_OR_NEWER
																						, mainCamera.VRRenderTextureSize == apPortrait.VR_RT_SIZE.ByEyeTextureSize
#endif
																						);
							newCamRenderData._isUpdated = true;

							addedCameraDataList.Add(newCamRenderData);
						}
					}

					bool isAnyReleased = false;
					//업데이트 되지 않은 것은 삭제하자.
					for (int iCamRD = 0; iCamRD < _nCameras; iCamRD++)
					{
						_cal_curCamRenderData = _cameraRenderDataList[iCamRD];
						if(!_cal_curCamRenderData._isUpdated)
						{
							//저장된 렌더 텍스쳐, 커맨드 버퍼 등을 Release
							_cal_curCamRenderData.ReleaseAll();
							isAnyReleased = true;
						}
					}

					if(isAnyReleased)
					{
						_cameraRenderDataList.RemoveAll(delegate(CameraRenderData a)
						{
							return !a._isUpdated;
						});
					}

					//새로 추가를 하자.
					if(addedCameraDataList.Count > 0)
					{
						_cal_curCamRenderData = null;
						for (int i = 0; i < addedCameraDataList.Count; i++)
						{
							_cameraRenderDataList.Add(addedCameraDataList[i]);
						}
					}

					//Cam 2 Data 갱신
					_nCameras = _cameraRenderDataList.Count;
					_cam2CamRenderData.Clear();
					for (int i = 0; i < _nCameras; i++)
					{
						_cal_curCamRenderData = _cameraRenderDataList[i];
						_cam2CamRenderData.Add(_cal_curCamRenderData._camera, _cal_curCamRenderData);
					}
				}

				_refreshKey = mainCamera._refreshKey;

				if(_nCameras == 0)
				{
					_mainData = null;

					_status = STATUS.NoCamera;
				}
				else
				{
					_mainData = _cameraRenderDataList[0];

					_status = STATUS.Camera;
				}

				_renderCameraType_Prev = _renderCameraType;
			}

			
			_isAllSameForward = mainCamera.IsAllSameForward;
			_isAllCameraOrthographic = mainCamera.IsAllOrthographic;
			

#region [미사용 코드]
			////- 카메라의 개수
			////- 현재 카메라가 유효한지
			//bool isNeedToRefresh = false;
			//Camera[] sceneCameras = Camera.allCameras;
			//int nSceneCameras = (sceneCameras != null ? sceneCameras.Length : 0);

			//if (nSceneCameras == 0)
			//{
			//	//카메라가 없네요.
			//	if (_numberOfCameraType != NumberOfCameraType.None)
			//	{
			//		Clear();
			//	}
			//	return;
			//}

			//if(!isRefreshForce)
			//{
			//	//조건을 체크하자
			//	if (_numberOfCameraType == NumberOfCameraType.None
			//		|| _mainData == null
			//		|| _cameraRenderDataList.Count == 0
			//		|| _nCameras == 0
			//		|| _prevNumSceneCamera != nSceneCameras)
			//	{
			//		//초기화 된 상태이거나 씬의 카메라 개수가 다르다면
			//		isNeedToRefresh = true;
			//	}
			//	else
			//	{
			//		//존재하는 카메라가 유효하지 않다면
			//		if(_numberOfCameraType == NumberOfCameraType.Single)
			//		{
			//			//1개 일때
			//			if(!IsLookMesh(_mainData._camera))
			//			{
			//				//Single 카메라가 제대로 바라보고 있지 않다면
			//				isNeedToRefresh = true;
			//			}
			//		}
			//		else
			//		{
			//			//여러개일때
			//			CameraRenderData curCamData = null;
			//			for (int i = 0; i < _nCameras; i++)
			//			{
			//				curCamData = _cameraRenderDataList[i];
			//				if(curCamData == null 
			//					|| curCamData._camera == null
			//					|| !IsLookMesh(curCamData._camera))
			//				{
			//					isNeedToRefresh = true;
			//					break;
			//				}

			//			}
			//		}
			//	}
			//}
			//else
			//{
			//	//무조건 Refresh
			//	isNeedToRefresh = true;
			//}

			//if(!isNeedToRefresh)
			//{
			//	//카메라를 새로 갱신할 필요가 없다.
			//	//카메라의 Forward와 타입만 비교하자.
			//	_isAllSameForward = true;
			//	_isAllCameraOrthographic = true;
			//	if(_numberOfCameraType == NumberOfCameraType.Multiple)
			//	{
			//		CameraRenderData curCamData = null;
			//		Vector3 mainForward = _mainData._transform.forward;
			//		for (int i = 0; i < _nCameras; i++)
			//		{
			//			curCamData = _cameraRenderDataList[i];
			//			if(!curCamData._camera.orthographic)
			//			{
			//				//하나라도 Orthographic이 아니라면..
			//				_isAllCameraOrthographic = false;
			//			}
			//			if(Mathf.Abs(curCamData._transform.forward.x - mainForward.x) > 0.01f ||
			//				Mathf.Abs(curCamData._transform.forward.y - mainForward.y) > 0.01f ||
			//				Mathf.Abs(curCamData._transform.forward.z - mainForward.z) > 0.01f)
			//			{
			//				//Forward가 하나라도 다르다면
			//				_isAllSameForward = false;
			//			}
			//		}
			//	}
			//	return;
			//}

			////다시 Refresh를 해야한다.
			//if(_numberOfCameraType != NumberOfCameraType.None)
			//{
			//	//일단 초기화
			//	Clear();
			//}

			////씬 카메라를 찾아서 추가하자
			//if (nSceneCameras > 0)
			//{
			//	Camera curSceneCamera = null;
			//	for (int i = 0; i < sceneCameras.Length; i++)
			//	{
			//		curSceneCamera = sceneCameras[i];
			//		if (curSceneCamera != null && IsLookMesh(curSceneCamera))
			//		{
			//			//바라보고 있는 카메라다. > 추가
			//			CameraRenderData newCamData = new CameraRenderData(_parentOptMesh, curSceneCamera);
			//			_cameraRenderDataList.Add(newCamData);
			//			_camera2RenderData.Add(curSceneCamera, newCamData);
			//		}
			//	}
			//}
			//_nCameras = _cameraRenderDataList.Count;
			//_prevNumSceneCamera = nSceneCameras;

			//if(_nCameras == 0)
			//{
			//	_numberOfCameraType = NumberOfCameraType.None;
			//	_mainData = null;
			//}
			//else if(_nCameras == 1)
			//{
			//	_numberOfCameraType = NumberOfCameraType.Single;
			//	_mainData = _cameraRenderDataList[0];
			//}
			//else
			//{
			//	_numberOfCameraType = NumberOfCameraType.Multiple;
			//	_mainData = _cameraRenderDataList[0];
			//}


			////새로 갱신 후 카메라의 Forward와 Ortho타입만 비교하자.
			//_isAllSameForward = true;
			//_isAllCameraOrthographic = true;
			//if(_numberOfCameraType == NumberOfCameraType.Multiple)
			//{
			//	CameraRenderData curCamData = null;
			//	Vector3 mainForward = _mainData._transform.forward;
			//	for (int i = 0; i < _nCameras; i++)
			//	{
			//		curCamData = _cameraRenderDataList[i];
			//		if(!curCamData._camera.orthographic)
			//		{
			//			//하나라도 Orthographic이 아니라면..
			//			_isAllCameraOrthographic = false;
			//		}
			//		if(Mathf.Abs(curCamData._transform.forward.x - mainForward.x) > 0.01f ||
			//			Mathf.Abs(curCamData._transform.forward.y - mainForward.y) > 0.01f ||
			//			Mathf.Abs(curCamData._transform.forward.z - mainForward.z) > 0.01f)
			//		{
			//			//Forward가 하나라도 다르다면
			//			_isAllSameForward = false;
			//		}
			//	}
			//} 
#endregion
		}


		private bool IsLookMesh(Camera camera)
		{
			if(camera == null)
			{
				return false;
			}
			return (camera.cullingMask == (camera.cullingMask | (1 << _parentOptMesh.gameObject.layer)) && camera.enabled);
		}


		public bool IsAnyCameraChanged(apOptMainCamera mainCamera)
		{
			if(mainCamera == null)
			{
				return false;
			}
			return _refreshKey != mainCamera._refreshKey;
		}


		// Event
		//-----------------------------------------------
		public void SetPreRenderedEvent(apOptMultiCameraController.FUNC_MESH_PRE_RENDERED funcMeshPreRendered)
		{
			if(_renderCameraType == RenderCameraType.None)
			{
				return;
			}

			for (int i = 0; i < _nCameras; i++)
			{
				_cameraRenderDataList[i].MakeAndLinkMultiCameraController(funcMeshPreRendered);
			}

			_status = STATUS.RT_Events;
		}
		
		public void MakeRenderTextureAndCommandBuffer(int renderTextureSize, string commandBufferName)
		{
			if(_renderCameraType == RenderCameraType.None)
			{
				return;
			}

			for (int i = 0; i < _nCameras; i++)
			{
				_cameraRenderDataList[i].MakeRenderTextureAndCommandBuffer(renderTextureSize, commandBufferName);
			}

			_status = STATUS.RT_Events;
		}

		public void AddCommandBufferToCamera()
		{
			if(_renderCameraType == RenderCameraType.None)
			{
				return;
			}
			for (int i = 0; i < _nCameras; i++)
			{
				_cameraRenderDataList[i].AddCommandBufferToCamera();
			}
			
			_status = STATUS.RT_Events;
		}


		// Get / Set
		//-----------------------------------------------
		public RenderCameraType GetRenderCameraType()
		{
			return _renderCameraType;
		}

		public STATUS GetStatus()
		{
			return _status;
		}

		public void SetStatus_RTEvent()
		{
			_status = STATUS.RT_Events;
		}

		public bool IsValid
		{
			get { return _renderCameraType != RenderCameraType.None; }
		}
		
		
		public bool IsVRSupported()
		{
			return _renderCameraType == RenderCameraType.Multiple || _renderCameraType == RenderCameraType.Single_VR;
		}
 
		public int NumCamera
		{
			get {  return _nCameras; }
		}

		public CameraRenderData GetCameraData(int index)
		{
			return _cameraRenderDataList[index];
		}

		public CameraRenderData GetCameraData(Camera camera)
		{
			if(_renderCameraType == RenderCameraType.None)
			{
				return null;
			}

			if(_cam2CamRenderData.ContainsKey(camera))
			{
				return _cam2CamRenderData[camera];
			}

			return null;
		}

		public CameraRenderData MainData
		{
			get {  return _mainData; }
		}


		public bool IsAllSameForward
		{
			get { return _isAllSameForward; }
		}

		public bool IsAllCameraOrthographic
		{
			get {  return _isAllCameraOrthographic; }
		}
		
		public apPortrait.VR_SUPPORT_MODE VRSupportMode
		{
			get {  return _vrSupportMode; }
		}

		
	}
}