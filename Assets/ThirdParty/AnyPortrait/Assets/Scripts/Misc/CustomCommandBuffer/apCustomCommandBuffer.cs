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
using UnityEngine.Rendering;

namespace AnyPortrait
{
	// 추가 22.5.19 : 사용자가 직접 커스텀 버퍼를 만들어서 렌더링을 하고자 하는 경우에 사용되는 클래스
	// 렌더링을 커스텀하게 제어할 수 있는 커맨드 버퍼를 래핑했다.
	// 대상의 카메라, apPortrait, 커맨드 버퍼를 가지고, 업데이트 함수를 사용할 수 있게 만든다.
	// apPortrait 내부에서 생성되는 것은 아니고, 사용자가 스크립트로 사용할 수 있도록 제공한다.
	
	/// <summary>
	/// A wrapper class for Command Buffer that renders the character of AnyPortrait using user scripts.
	/// </summary>
	public class apCustomCommandBuffer
	{
		// Members
		//-------------------------------------------
		private Camera _camera = null;
		private apPortrait _portrait = null;

		//생성된 커맨드 버퍼
		private CommandBuffer _commandBuffer = null;

		//SRP에 등록했는가?
		private bool _isSRP = false;

		//등록한 이벤트 타이밍
		// 1. Native
		private CameraEvent _nativeCameraEvent = CameraEvent.AfterFinalPass;

		public enum SRPRenderEvent
		{
			BeginCameraRendering, EndCameraRendering
		}
		
		//요청에 의해서 OptTransform의 텍스쳐별로 Material을 구분할 수 있다.
		private Dictionary<Texture, Material> _img2Material = null;


		private List<apOptTransform> _cal_TargetOptTransforms = null;
		private List<apOptTransform> _cal_SortedOptTransforms = null;
		private apOptTransform _cal_CurOptTransform = null;

		// Init
		//-------------------------------------------
		/// <summary>
		/// Create a Command Buffer while specifying the target apPortrait and Camera.
		/// </summary>
		/// <param name="camera">Target Camera</param>
		/// <param name="portrait">Target apPortrait</param>
		/// <param name="commandBufferName">Name of the Command Buffer</param>
		public apCustomCommandBuffer(Camera camera, apPortrait portrait, string commandBufferName)
		{
			_camera = camera;
			_portrait = portrait;

			_commandBuffer = new CommandBuffer();
			_commandBuffer.name = commandBufferName;
		}

		// Release
		//----------------------------------------------------
		/// <summary>
		/// Unregisters the rendering event and deletes the internal data.
		/// If the camera disappears or there is no character, be sure to call this function!
		/// </summary>
		public void Destory()
		{

			if(_isSRP)
			{
#if UNITY_2019_1_OR_NEWER
				//SRP라면
				RenderPipelineManager.beginCameraRendering -= ProcessSRP;
				RenderPipelineManager.endCameraRendering -= ProcessSRP;
#endif
			}
			else
			{
				//Native RP라면
				if(_camera != null)
				{
					_camera.RemoveCommandBuffer(_nativeCameraEvent, _commandBuffer);
				}
				
			}

			_commandBuffer.Dispose();
			_commandBuffer = null;
		}

		// Add Render Event
		//----------------------------------------------------
		/// <summary>
		/// If SRP is not used, call this function to register the Command Buffer with the camera.
		/// </summary>
		/// <param name="cameraEvent">The type of event being rendered</param>
		public void AddToCamera(CameraEvent cameraEvent)
		{
			_isSRP = false;
			_nativeCameraEvent = cameraEvent;

			_camera.AddCommandBuffer(cameraEvent, _commandBuffer);
		}

#if UNITY_2019_1_OR_NEWER
		/// <summary>
		/// If SRP is used, call this function to register the Command Buffer with the Render Pipeline.
		/// </summary>
		/// <param name="srpRenderEvent">The type of event being rendered</param>
		public void AddToCameraSRP(SRPRenderEvent srpRenderEvent)
		{
			_isSRP = true;

			//혹시라도 이벤트가 등록되었다면 이벤트를 일단 해제한다.
			RenderPipelineManager.beginCameraRendering -= ProcessSRP;
			RenderPipelineManager.endCameraRendering -= ProcessSRP;

			//옵션에 맞게 렌더 타이밍을 설정한다.
			if(srpRenderEvent == SRPRenderEvent.BeginCameraRendering)
			{
				RenderPipelineManager.beginCameraRendering += ProcessSRP;
			}
			else
			{
				RenderPipelineManager.endCameraRendering += ProcessSRP;
			}
		}

		// SRP의 경우의 함수
		private void ProcessSRP(ScriptableRenderContext context, Camera cam)
		{
			if(context == null || cam != _camera || _commandBuffer == null)
			{
				return;
			}
			context.ExecuteCommandBuffer(_commandBuffer);//커맨드 버퍼를 여기서 실행하자
			context.Submit();
		}
#endif


		// Function
		//--------------------------------------------------------------
		/// <summary>
		/// Copy the material properties of the argument and use it when rendering.
		/// Regardless of the properties of the source material, the MainTexture retains its original image.
		/// By duplicating the material on an image-by-image basis, drawcall optimization is done as is.
		/// </summary>
		/// <param name="alternativeMaterial">A material with alternative properties. Virtual materials whose properties are duplicated are used for rendering.</param>
		public void CreateAlternativeMaterials(Material alternativeMaterial)
		{
			if(_img2Material == null)
			{
				_img2Material = new Dictionary<Texture, Material>();
			}
			_img2Material.Clear();

			int nOptTranforms = _portrait._optTransforms != null ? _portrait._optTransforms.Count : 0;
			if(nOptTranforms == 0)
			{
				return;
			}

			Texture curTexture = null;
			for (int i = 0; i < nOptTranforms; i++)
			{
				_cal_CurOptTransform = _portrait._optTransforms[i];
				if(_cal_CurOptTransform._childMesh == null
					|| _cal_CurOptTransform._childMesh._meshRenderer == null)
				{
					continue;
				}

				curTexture = _cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture;

				if(_img2Material.ContainsKey(curTexture))
				{
					continue;
				}

				Material duplicatedMaterial = UnityEngine.Object.Instantiate<Material>(alternativeMaterial);
				duplicatedMaterial.CopyPropertiesFromMaterial(alternativeMaterial);
				duplicatedMaterial.mainTexture = curTexture;

				_img2Material.Add(curTexture, duplicatedMaterial);
			}
		}


		/// <summary>
		/// A variant of the "CreateAlternativeMaterials" function. You can specify a different material for each image.
		/// </summary>
		/// <param name="imageNameToAlterMaterialMap">A Dictionary that associates image names and materials. If possible, you should create a Dictionary of all images in Portrait.</param>
		/// <param name="unmatchedMaterial">Material used when unmatched to any image</param>
		public void CreateAlternativeMaterials(Dictionary<string, Material> imageNameToAlterMaterialMap, Material unmatchedMaterial)
		{
			if(_img2Material == null)
			{
				_img2Material = new Dictionary<Texture, Material>();
			}
			_img2Material.Clear();

			int nOptTranforms = _portrait._optTransforms != null ? _portrait._optTransforms.Count : 0;
			if(nOptTranforms == 0)
			{
				return;
			}

			//OptTextureData를 모두 찾아서 해당 Image를 연결하자
			int nOptTextureData = _portrait._optTextureData != null ? _portrait._optTextureData.Count : 0;
			int nArgMats = imageNameToAlterMaterialMap != null ? imageNameToAlterMaterialMap.Count : 0;
			Dictionary<Texture2D, Material> optTextureImage2ArgMats = new Dictionary<Texture2D, Material>();
			
			if(nOptTextureData > 0 && nArgMats > 0)
			{
				apOptTextureData curOptTexData = null;
				Material targetAtlterMat = null;
				for (int i = 0; i < nOptTextureData; i++)
				{
					curOptTexData = _portrait._optTextureData[i];

					//이미지가 없거나 동일한 이미지가 등록되었다면 패스
					if(curOptTexData._texture == null)
					{
						continue;
					}

					if(optTextureImage2ArgMats.ContainsKey(curOptTexData._texture))
					{
						continue;
					}

					//이 텍스쳐의 이름이 지정되었는지 찾자
					targetAtlterMat = null;
					if(imageNameToAlterMaterialMap.ContainsKey(curOptTexData._name))
					{
						targetAtlterMat = imageNameToAlterMaterialMap[curOptTexData._name];
					}

					if(targetAtlterMat == null)
					{
						//매칭되지 않았다. > 예외 재질 사용
						targetAtlterMat = unmatchedMaterial;
					}

					//연결에 사용될 Dictionary를 만들자
					optTextureImage2ArgMats.Add(curOptTexData._texture, targetAtlterMat);
				}
			}

			Texture curTexture = null;
			for (int i = 0; i < nOptTranforms; i++)
			{
				_cal_CurOptTransform = _portrait._optTransforms[i];
				if(_cal_CurOptTransform._childMesh == null
					|| _cal_CurOptTransform._childMesh._meshRenderer == null)
				{
					continue;
				}

				curTexture = _cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture;

				if(_img2Material.ContainsKey(curTexture))
				{
					continue;
				}

				//이 이미지를 가진 TextureData를 찾자
				Material srcAlterMaterial = null;
				bool isFindSrc = optTextureImage2ArgMats.TryGetValue(curTexture as Texture2D, out srcAlterMaterial);
				if(!isFindSrc || srcAlterMaterial == null)
				{
					//대체 Material 사용
					srcAlterMaterial = unmatchedMaterial;
				}

				//그러고도 null이라면
				if(srcAlterMaterial == null)
				{
					continue;
				}


				Material duplicatedMaterial = UnityEngine.Object.Instantiate<Material>(srcAlterMaterial);
				duplicatedMaterial.CopyPropertiesFromMaterial(srcAlterMaterial);
				duplicatedMaterial.mainTexture = curTexture;

				_img2Material.Add(curTexture, duplicatedMaterial);
			}
		}


		// Functions : Wrapper
		//-------------------------------------------
		//커맨드 버퍼를 만들자 (래핑)
		/// <summary>
		/// Wrapper: Clear all Commands.
		/// </summary>
		public void ClearCommands()
		{
			_commandBuffer.Clear();
		}

		/// <summary>
		/// Wrapper: Set to render by targeting the render texture.
		/// </summary>
		/// <param name="renderTexture">Render Texture where the result of rendering is drawn</param>
		public void SetRenderTarget(RenderTexture renderTexture)
		{	
			_commandBuffer.SetRenderTarget(new RenderTargetIdentifier(renderTexture), 0);
		}

		/// <summary>
		/// Wrapper: Clear the data of the render target.
		/// </summary>
		/// <param name="clearDepth">If True, the depth buffer is cleared</param>
		/// <param name="clearColor">If True, the color buffer is cleared</param>
		/// <param name="backgroundColor">The default background color</param>
		/// <param name="depth">Depth value to be initialized</param>
		public void ClearRenderTarget(bool clearDepth, bool clearColor, Color backgroundColor, float depth = 1.0f)
		{
			_commandBuffer.ClearRenderTarget(clearDepth, clearColor, backgroundColor, depth);
		}

#if UNITY_2021_1_OR_NEWER
		/// <summary>
		/// Wrapper: Clear the data of the render target.
		/// </summary>
		/// <param name="clearFlags">Which render targets to clear, defined using a bitwise OR combination of RTClearFlags values.</param>
		/// <param name="backgroundColor">The default background color</param>
		/// <param name="depth">Depth value to be initialized</param>
		/// <param name="stencil">Stencil value to be initialized</param>
		public void ClearRenderTarget(UnityEngine.Rendering.RTClearFlags clearFlags, Color backgroundColor, float depth = 1.0f, uint stencil = 0)
		{
			_commandBuffer.ClearRenderTarget(clearFlags, backgroundColor, depth, stencil);
		}
#endif

#if UNITY_2019_1_OR_NEWER
		/// <summary>
		/// Wrapper: Apply the View Matrix of the current target camera to the Command Buffer.
		/// </summary>
		public void SetViewMatrix()
		{
			_commandBuffer.SetViewMatrix(_camera.worldToCameraMatrix);
		}

		/// <summary>
		/// Wrapper: Apply the View Matrix to the Command Buffer.
		/// </summary>
		/// <param name="customViewMatrix"></param>
		public void SetViewMatrix(Matrix4x4 customViewMatrix)
		{
			_commandBuffer.SetViewMatrix(customViewMatrix);
		}

		/// <summary>
		/// Wrapper: Apply the Projection Matrix of the current target camera to the Command Buffer.
		/// </summary>
		public void SetProjectionMatrix()
		{
			_commandBuffer.SetProjectionMatrix(_camera.projectionMatrix);
		}

		/// <summary>
		/// Wrapper: Apply the Projection Matrix to the Command Buffer.
		/// </summary>
		public void SetProjectionMatrix(Matrix4x4 customProjectionMatrix)
		{
			_commandBuffer.SetProjectionMatrix(customProjectionMatrix);
		}
#endif

		/// <summary>
		/// Wrapper: Input a request to draw the target OptTransform's mesh into the Command Buffer.
		/// </summary>
		/// <param name="optTransform">Target OptTransform with mesh</param>
		public void DrawMesh(apOptTransform optTransform)
		{
			if(optTransform._childMesh == null
				|| optTransform._childMesh._mesh == null)
			{
				return;
			}

			_commandBuffer.DrawMesh(	optTransform._childMesh._mesh, 
										optTransform._childMesh._transform.localToWorldMatrix, 
										optTransform._childMesh._meshRenderer.sharedMaterial,
										0, 0);
		}


		/// <summary>
		/// Wrapper: Input a request to draw the target OptTransform's mesh into the Command Buffer.
		/// </summary>
		/// <param name="optTransform">Target OptTransform with mesh</param>
		/// <param name="material">Material to be used for rendering</param>
		public void DrawMesh(apOptTransform optTransform, Material material)
		{
			if(optTransform._childMesh == null
				|| optTransform._childMesh._mesh == null)
			{
				return;
			}

			material.mainTexture = optTransform._childMesh._meshRenderer.sharedMaterial.mainTexture;

			_commandBuffer.DrawMesh(	optTransform._childMesh._mesh, 
										optTransform._childMesh._transform.localToWorldMatrix, 
										material,
										0, 0);
		}

		/// <summary>
		/// Wrapper: Input a request to draw the target OptTransform's mesh into the Command Buffer.
		/// </summary>
		/// <param name="optTransform">Target OptTransform with mesh</param>
		/// <param name="localToWorldMatrix">User-written Local To World Matrix (defaults to transform's localToWorldMatrix)</param>
		public void DrawMesh(apOptTransform optTransform, Matrix4x4 localToWorldMatrix)
		{
			if(optTransform._childMesh == null
				|| optTransform._childMesh._mesh == null)
			{
				return;
			}

			_commandBuffer.DrawMesh(	optTransform._childMesh._mesh, 
										localToWorldMatrix, 
										optTransform._childMesh._meshRenderer.sharedMaterial,
										0, 0);
		}


		/// <summary>
		/// Wrapper: Input a request to draw the target OptTransform's mesh into the Command Buffer.
		/// </summary>
		/// <param name="optTransform">Target OptTransform with mesh</param>
		/// <param name="material">Material to be used for rendering</param>
		/// <param name="localToWorldMatrix">User-written Local To World Matrix (defaults to transform's localToWorldMatrix)</param>
		public void DrawMesh(apOptTransform optTransform, Matrix4x4 localToWorldMatrix, Material material)
		{
			if(optTransform._childMesh == null
				|| optTransform._childMesh._mesh == null)
			{
				return;
			}

			material.mainTexture = optTransform._childMesh._meshRenderer.sharedMaterial.mainTexture;

			_commandBuffer.DrawMesh(	optTransform._childMesh._mesh, 
										localToWorldMatrix, 
										material,
										0, 0);
		}


		/// <summary>
		/// Wrapper: Render all meshes in apPortrait that are currently being rendered.
		/// </summary>
		/// <param name="sortOrder">Whether to render meshes sorted</param>
		/// <param name="excludeClippedChildMeshes">Whether to exclude clipped meshes</param>
		public void DrawAllMeshes(bool sortOrder, bool excludeClippedChildMeshes)
		{
			if(_portrait._curPlayingOptRootUnit == null)
			{
				return;
			}

			_cal_TargetOptTransforms = _portrait._curPlayingOptRootUnit.OptTransforms;
			
			int nTransforms = _cal_TargetOptTransforms != null ? _cal_TargetOptTransforms.Count : 0;

			if(nTransforms == 0)
			{
				return;
			}

			if (sortOrder)
			{
				//Sorting된 경우
				if(_cal_SortedOptTransforms == null)
				{
					_cal_SortedOptTransforms = new List<apOptTransform>();
				}
				_cal_SortedOptTransforms.Clear();

				for (int i = 0; i < nTransforms; i++)
				{
					_cal_CurOptTransform = _cal_TargetOptTransforms[i];

					if(!_cal_CurOptTransform._isVisible
						||_cal_CurOptTransform._childMesh == null
						|| _cal_CurOptTransform._childMesh._mesh == null)
					{
						continue;
					}

					if(_cal_CurOptTransform._childMesh._isMaskChild
						&& excludeClippedChildMeshes)
					{
						//옵션에 의해 클리핑되는 메시는 제외한다.
						continue;
					}

					_cal_SortedOptTransforms.Add(_cal_CurOptTransform);
				}

				//Sorting을 하자
				if(_portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder
					|| _portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
				{
					//Sorting Order를 이용하여 Sorting (Order 순서로 오름차순)
					_cal_SortedOptTransforms.Sort(delegate(apOptTransform a, apOptTransform b)
					{
						return a._childMesh.GetSortingOrder() - b._childMesh.GetSortingOrder();
					});
				}
				else
				{
					//Z 위치를 기준으로 Sort (Z 내림차순. Z가 클 수록 뒤에 있다.)
					_cal_SortedOptTransforms.Sort(delegate(apOptTransform a, apOptTransform b)
					{
						return (int)((b._transform.position.z - a._transform.position.z) * 1000.0f);
					});
				}

				int nSortedMeshes = _cal_SortedOptTransforms.Count;

				if(nSortedMeshes == 0)
				{
					return;
				}

				for (int i = 0; i < nSortedMeshes; i++)
				{
					_cal_CurOptTransform = _cal_SortedOptTransforms[i];

					_commandBuffer.DrawMesh(	_cal_CurOptTransform._childMesh._mesh,
												_cal_CurOptTransform._childMesh._transform.localToWorldMatrix,
												_cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial,
												0, 0);
				}
			}
			else
			{
				//Sorting없이 렌더링
				for (int i = 0; i < nTransforms; i++)
				{
					_cal_CurOptTransform = _cal_TargetOptTransforms[i];

					if (!_cal_CurOptTransform._isVisible
						|| _cal_CurOptTransform._childMesh == null
						|| _cal_CurOptTransform._childMesh._mesh == null)
					{
						continue;
					}

					if(_cal_CurOptTransform._childMesh._isMaskChild
						&& excludeClippedChildMeshes)
					{
						//옵션에 의해 클리핑되는 메시는 제외한다.
						continue;
					}

					_commandBuffer.DrawMesh(	_cal_CurOptTransform._childMesh._mesh,
												_cal_CurOptTransform._childMesh._transform.localToWorldMatrix,
												_cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial,
												0, 0);
				}
			}
		}


		

		/// <summary>
		/// Wrapper: Render all meshes in apPortrait that are currently being rendered.
		/// </summary>
		/// <param name="material">Material to be used for rendering</param>
		/// <param name="sortOrder">Whether to render meshes sorted</param>
		/// <param name="excludeClippedChildMeshes">Whether to exclude clipped meshes</param>
		public void DrawAllMeshes(Material material, bool sortOrder, bool excludeClippedChildMeshes)
		{
			if(_portrait._curPlayingOptRootUnit == null)
			{
				return;
			}

			_cal_TargetOptTransforms = _portrait._curPlayingOptRootUnit.OptTransforms;
			
			int nTransforms = _cal_TargetOptTransforms != null ? _cal_TargetOptTransforms.Count : 0;

			if(nTransforms == 0)
			{
				return;
			}

			if (sortOrder)
			{
				//Sorting된 경우
				if(_cal_SortedOptTransforms == null)
				{
					_cal_SortedOptTransforms = new List<apOptTransform>();
				}
				_cal_SortedOptTransforms.Clear();

				for (int i = 0; i < nTransforms; i++)
				{
					_cal_CurOptTransform = _cal_TargetOptTransforms[i];

					if(!_cal_CurOptTransform._isVisible
						||_cal_CurOptTransform._childMesh == null
						|| _cal_CurOptTransform._childMesh._mesh == null)
					{
						continue;
					}

					if(_cal_CurOptTransform._childMesh._isMaskChild
						&& excludeClippedChildMeshes)
					{
						//옵션에 의해 클리핑되는 메시는 제외한다.
						continue;
					}

					_cal_SortedOptTransforms.Add(_cal_CurOptTransform);
				}

				//Sorting을 하자
				if(_portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder
					|| _portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
				{
					//Sorting Order를 이용하여 Sorting (Order 순서로 오름차순)
					_cal_SortedOptTransforms.Sort(delegate(apOptTransform a, apOptTransform b)
					{
						return a._childMesh.GetSortingOrder() - b._childMesh.GetSortingOrder();
					});
				}
				else
				{
					//Z 위치를 기준으로 Sort (Z 내림차순. Z가 클 수록 뒤에 있다.)
					_cal_SortedOptTransforms.Sort(delegate(apOptTransform a, apOptTransform b)
					{
						return (int)((b._transform.position.z - a._transform.position.z) * 1000.0f);
					});
				}

				int nSortedMeshes = _cal_SortedOptTransforms.Count;

				if(nSortedMeshes == 0)
				{
					return;
				}

				for (int i = 0; i < nSortedMeshes; i++)
				{
					_cal_CurOptTransform = _cal_SortedOptTransforms[i];

					material.mainTexture = _cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture;

					_commandBuffer.DrawMesh(	_cal_CurOptTransform._childMesh._mesh,
												_cal_CurOptTransform._childMesh._transform.localToWorldMatrix,
												material,
												0, 0);
				}
			}
			else
			{
				//Sorting없이 렌더링
				for (int i = 0; i < nTransforms; i++)
				{
					_cal_CurOptTransform = _cal_TargetOptTransforms[i];

					if (!_cal_CurOptTransform._isVisible
						|| _cal_CurOptTransform._childMesh == null
						|| _cal_CurOptTransform._childMesh._mesh == null)
					{
						continue;
					}

					if(_cal_CurOptTransform._childMesh._isMaskChild
						&& excludeClippedChildMeshes)
					{
						//옵션에 의해 클리핑되는 메시는 제외한다.
						continue;
					}

					material.mainTexture = _cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture;

					_commandBuffer.DrawMesh(	_cal_CurOptTransform._childMesh._mesh,
												_cal_CurOptTransform._childMesh._transform.localToWorldMatrix,
												material,
												0, 0);
				}
			}
		}



		/// <summary>
		/// Wrapper: Render all meshes in apPortrait that are currently being rendered.
		/// Use the materials created by the MakeDuplicateMaterialsPerImage function.
		/// </summary>
		/// <param name="sortOrder">Whether to render meshes sorted</param>
		/// <param name="excludeClippedChildMeshes">Whether to exclude clipped meshes</param>
		public void DrawAllMeshesWithAlternativeMaterials(bool sortOrder, bool excludeClippedChildMeshes)
		{
			if(_portrait._curPlayingOptRootUnit == null)
			{
				return;
			}

			_cal_TargetOptTransforms = _portrait._curPlayingOptRootUnit.OptTransforms;
			
			int nTransforms = _cal_TargetOptTransforms != null ? _cal_TargetOptTransforms.Count : 0;

			if(nTransforms == 0)
			{
				return;
			}

			if(_img2Material == null)
			{
				return;
			}

			

			if (sortOrder)
			{
				//Sorting된 경우
				if(_cal_SortedOptTransforms == null)
				{
					_cal_SortedOptTransforms = new List<apOptTransform>();
				}
				_cal_SortedOptTransforms.Clear();

				for (int i = 0; i < nTransforms; i++)
				{
					_cal_CurOptTransform = _cal_TargetOptTransforms[i];

					
					if(!_cal_CurOptTransform._isVisible
						|| _cal_CurOptTransform._childMesh == null
						|| _cal_CurOptTransform._childMesh._mesh == null)
					{
						continue;
					}

					if(_cal_CurOptTransform._childMesh._isMaskChild
						&& excludeClippedChildMeshes)
					{
						//옵션에 의해 클리핑되는 메시는 제외한다.
						continue;
					}

					_cal_SortedOptTransforms.Add(_cal_CurOptTransform);
				}

				//Sorting을 하자
				if(_portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.DepthToOrder
					|| _portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.ReverseDepthToOrder)
				{
					//Sorting Order를 이용하여 Sorting (Order 순서로 오름차순)
					_cal_SortedOptTransforms.Sort(delegate(apOptTransform a, apOptTransform b)
					{
						return a._childMesh.GetSortingOrder() - b._childMesh.GetSortingOrder();
					});
				}
				else
				{
					//Z 위치를 기준으로 Sort (Z 내림차순. Z가 클 수록 뒤에 있다.)
					_cal_SortedOptTransforms.Sort(delegate(apOptTransform a, apOptTransform b)
					{
						return (int)((b._transform.position.z - a._transform.position.z) * 1000.0f);
					});
				}

				int nSortedMeshes = _cal_SortedOptTransforms.Count;

				if(nSortedMeshes == 0)
				{
					return;
				}

				for (int i = 0; i < nSortedMeshes; i++)
				{
					_cal_CurOptTransform = _cal_SortedOptTransforms[i];

					Material targetMaterial = null;
					if(!_img2Material.ContainsKey(_cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture))
					{
						continue;
					}

					targetMaterial = _img2Material[_cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture];

					if(targetMaterial == null)
					{
						//재질이 없다면 패스한다.
						continue;
					}

					_commandBuffer.DrawMesh(	_cal_CurOptTransform._childMesh._mesh,
												_cal_CurOptTransform._childMesh._transform.localToWorldMatrix,
												targetMaterial,
												0, 0);
				}
			}
			else
			{
				//Sorting없이 렌더링
				for (int i = 0; i < nTransforms; i++)
				{
					_cal_CurOptTransform = _cal_TargetOptTransforms[i];

					if (!_cal_CurOptTransform._isVisible
						|| _cal_CurOptTransform._childMesh == null
						|| _cal_CurOptTransform._childMesh._mesh == null)
					{
						continue;
					}

					if(_cal_CurOptTransform._childMesh._isMaskChild
						&& excludeClippedChildMeshes)
					{
						//옵션에 의해 클리핑되는 메시는 제외한다.
						continue;
					}

					Material targetMaterial = null;
					if(!_img2Material.ContainsKey(_cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture))
					{
						continue;
					}

					targetMaterial = _img2Material[_cal_CurOptTransform._childMesh._meshRenderer.sharedMaterial.mainTexture];

					if(targetMaterial == null)
					{
						//재질이 없다면 패스한다.
						continue;
					}

					_commandBuffer.DrawMesh(	_cal_CurOptTransform._childMesh._mesh,
												_cal_CurOptTransform._childMesh._transform.localToWorldMatrix,
												targetMaterial,
												0, 0);
				}
			}
			
		}


		// Get / Set
		//-------------------------------------------
		/// <summary>
		/// Return the created Command Buffer.
		/// </summary>
		/// <returns></returns>
		public CommandBuffer GetCommandBuffer()
		{
			return _commandBuffer;
		}
	}
}