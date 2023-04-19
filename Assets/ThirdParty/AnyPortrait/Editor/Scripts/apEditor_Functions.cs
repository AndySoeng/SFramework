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
//using UnityEngine.Profiling;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;


namespace AnyPortrait
{
	// apEditor에서 GUI와 다소 거리가 있는 특수한 함수들을 모아놓은 스크립트
	public partial class apEditor : EditorWindow
	{
		// Undo시 사용되는 기록 갱신하기
		//-------------------------------------------------
		

		// 추가 4.1 : 객체의 추가/삭제가 있는 경우 전체 Link를 다시 해야한다.

		/// <summary>객체가 추가되거나 삭제된 경우 이 함수를 호출해야한다. 이후 Refresh될 때 다시 Link해야할 필요가 있기 때문</summary>
		/// <param name="isStructChanged">만약 생성/추가가 없어도 그에 준하는 변경 사항이 있었다면 true로 넣자. 거의 대부분은 false</param>
		public void OnAnyObjectAddedOrRemoved(bool isStructChanged = false, bool isCalledInUndoRedoCallback = false)
		{

			if (_portrait == null)
			{
				_recordList_TextureData.Clear();
				_recordList_Mesh.Clear();
				//_recordList_MeshGroup.Clear();
				_recordList_AnimClip.Clear();
				_recordList_ControlParam.Clear();
				_recordList_Modifier.Clear();
				_recordList_AnimTimeline.Clear();
				_recordList_AnimTimelineLayer.Clear();
				//_recordList_Transform.Clear();
				_recordList_Bone.Clear();
				_recordList_MeshGroupAndTransform.Clear();
				_recordList_AnimClip2TargetMeshGroup.Clear();
			}
			else
			{
				_recordList_TextureData.Clear();
				_recordList_Mesh.Clear();
				//_recordList_MeshGroup.Clear();
				_recordList_AnimClip.Clear();
				_recordList_ControlParam.Clear();
				_recordList_Modifier.Clear();
				_recordList_AnimTimeline.Clear();
				_recordList_AnimTimelineLayer.Clear();
				//_recordList_Transform.Clear();
				_recordList_Bone.Clear();
				_recordList_MeshGroupAndTransform.Clear();
				_recordList_AnimClip2TargetMeshGroup.Clear();

				if (_portrait._textureData != null && _portrait._textureData.Count > 0)
				{
					for (int i = 0; i < _portrait._textureData.Count; i++)
					{
						_recordList_TextureData.Add(_portrait._textureData[i]._uniqueID);
					}
				}

				if (_portrait._meshes != null && _portrait._meshes.Count > 0)
				{
					for (int i = 0; i < _portrait._meshes.Count; i++)
					{
						_recordList_Mesh.Add(_portrait._meshes[i]._uniqueID);
					}
				}

				apMeshGroup meshGroup = null;
				List<int> curTransformIDs = null;
				if (_portrait._meshGroups != null && _portrait._meshGroups.Count > 0)
				{
					for (int iMeshGroup = 0; iMeshGroup < _portrait._meshGroups.Count; iMeshGroup++)
					{
						meshGroup = _portrait._meshGroups[iMeshGroup];

						//_recordList_MeshGroup.Add(meshGroup._uniqueID);//<<이전

						curTransformIDs = null;
						if (!_recordList_MeshGroupAndTransform.ContainsKey(meshGroup._uniqueID))
						{
							curTransformIDs = new List<int>();
							_recordList_MeshGroupAndTransform.Add(meshGroup._uniqueID, curTransformIDs);
						}
						else
						{
							curTransformIDs = _recordList_MeshGroupAndTransform[meshGroup._uniqueID];
						}


						//MeshGroup -> Modifier
						for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
						{
							_recordList_Modifier.Add(meshGroup._modifierStack._modifiers[iMod]._uniqueID);
						}

						//MeshGroup -> Transform
						for (int iMeshTF = 0; iMeshTF < meshGroup._childMeshTransforms.Count; iMeshTF++)
						{
							//_recordList_Transform.Add(meshGroup._childMeshTransforms[iMeshTF]._transformUniqueID);//이전
							curTransformIDs.Add(meshGroup._childMeshTransforms[iMeshTF]._transformUniqueID);//변경
						}

						for (int iMeshGroupTF = 0; iMeshGroupTF < meshGroup._childMeshGroupTransforms.Count; iMeshGroupTF++)
						{
							//_recordList_Transform.Add(meshGroup._childMeshGroupTransforms[iMeshGroupTF]._transformUniqueID);//이전
							curTransformIDs.Add(meshGroup._childMeshGroupTransforms[iMeshGroupTF]._transformUniqueID);//변경
						}

						for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
						{
							_recordList_Bone.Add(meshGroup._boneList_All[iBone]._uniqueID);
						}

					}
				}

				apAnimClip animClip = null;
				apAnimTimeline timeline = null;
				apAnimTimelineLayer timelineLayer = null;

				if (_portrait._animClips != null && _portrait._animClips.Count > 0)
				{
					for (int iAnimClip = 0; iAnimClip < _portrait._animClips.Count; iAnimClip++)
					{
						animClip = _portrait._animClips[iAnimClip];
						_recordList_AnimClip.Add(animClip._uniqueID);

						for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
						{
							timeline = animClip._timelines[iTimeline];
							_recordList_AnimTimeline.Add(timeline._uniqueID);

							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								timelineLayer = timeline._layers[iLayer];
								_recordList_AnimTimelineLayer.Add(timelineLayer._uniqueID);
							}
						}

						//추가 20.3.19 : AnimClip과 연결된 MeshGroup이 바뀌는 것도 구조적으로 체크해야한다.
						if (!_recordList_AnimClip2TargetMeshGroup.ContainsKey(animClip._uniqueID))
						{
							_recordList_AnimClip2TargetMeshGroup.Add(
								animClip._uniqueID,
								(animClip._targetMeshGroup != null ? animClip._targetMeshGroup._uniqueID : -1));
						}

					}
				}

				if (_portrait._controller._controlParams != null && _portrait._controller._controlParams.Count > 0)
				{
					for (int i = 0; i < _portrait._controller._controlParams.Count; i++)
					{
						_recordList_ControlParam.Add(_portrait._controller._controlParams[i]._uniqueID);
					}
				}
			}

			//추가 20.1.21
			if (isStructChanged)
			{
				_isRecordedStructChanged = true;//<<Undo를 하면 무조건 전체 Refresh를 해야한다.
			}

			if (!isCalledInUndoRedoCallback)
			{
				//추가 21.6.26 : apUndoHistory (GameObject)에 값을 저장한다.
				apEditorUtil.OnAnyObjectAddedOrRemoved();
			}
		}



		//----------------------------------------------------------------------------
		// 리소스 체크 및 로드
		//----------------------------------------------------------------------------
		

		// 리소스 체크 (업데이트 중 초기화)
		//-------------------------------------------------------------------
		private bool CheckEditorResources()
		{
			if (_gizmos == null)
			{
				_gizmos = new apGizmos(this);
			}
			if (_selection == null)
			{
				_selection = new apSelection(this);
			}

			if (_guiStringWrapper_32 == null) { _guiStringWrapper_32 = new apStringWrapper(32); }
			if (_guiStringWrapper_64 == null) { _guiStringWrapper_64 = new apStringWrapper(64); }
			if (_guiStringWrapper_128 == null) { _guiStringWrapper_128 = new apStringWrapper(128); }
			if (_guiStringWrapper_256 == null) { _guiStringWrapper_256 = new apStringWrapper(256); }

			//변경 21.5.30 : 싱글톤으로 변경
			if(!apPathSetting.I.IsFirstLoaded)
			{
				//변경 21.10.4 : 함수 변경
				apPathSetting.I.RefreshAndGetBasePath(false);
			}
			

			if (_imageSet == null)
			{
				_imageSet = new apImageSet();
			}



			//bool isImageReload = _imageSet.ReloadImages();
			apImageSet.ReloadResult imageReloadResult = _imageSet.ReloadImages();

			//if (isImageReload)
			if (imageReloadResult == apImageSet.ReloadResult.NewLoaded)
			{	
				//변경 22.6.9
				apControllerGL.SetTexture(_imageSet.Get(apImageSet.PRESET.Controller_NewAtlas));

				apTimelineGL.SetTexture(
					_imageSet.Get(apImageSet.PRESET.Anim_Keyframe),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyframeDummy),
					_imageSet.Get(apImageSet.PRESET.Anim_KeySummary),
					_imageSet.Get(apImageSet.PRESET.Anim_KeySummaryMove),
					_imageSet.Get(apImageSet.PRESET.Anim_PlayBarHead),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyLoopLeft),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyLoopRight),
					_imageSet.Get(apImageSet.PRESET.Anim_TimelineBGStart),
					_imageSet.Get(apImageSet.PRESET.Anim_TimelineBGEnd),
					_imageSet.Get(apImageSet.PRESET.Anim_CurrentKeyframe),
					_imageSet.Get(apImageSet.PRESET.Anim_KeyframeCursor),
					_imageSet.Get(apImageSet.PRESET.Anim_EventMark),
					_imageSet.Get(apImageSet.PRESET.Anim_OnionMark),
					_imageSet.Get(apImageSet.PRESET.Anim_OnionRangeStart),
					_imageSet.Get(apImageSet.PRESET.Anim_OnionRangeEnd)
					);

				apAnimCurveGL.SetTexture(
					_imageSet.Get(apImageSet.PRESET.Curve_ControlPoint)
					);

				apGL.SetTexture(_imageSet.Get(apImageSet.PRESET.Physic_VertMain),
								_imageSet.Get(apImageSet.PRESET.Physic_VertConst)
								);
			}
			else if (imageReloadResult == apImageSet.ReloadResult.Error)
			{
				//에러가 발생했다.
				//일단 로드를 한번 해보고.
				
				//변경 21.10.4 : 함수 변경
				apPathSetting.I.RefreshAndGetBasePath(true);//강제로 로드후 갱신

				EditorUtility.DisplayDialog("Failed to load", "Failed to load editor resources.\nPlease reinstall the AnyPortrait package or run the [Change Installation Path] function to set the path.", "Okay");


				CloseEditor();
				return false;
			}



			if (_controller == null || _controller.Editor == null)
			{
				_controller = new apEditorController();
				_controller.SetEditor(this);
			}

			if (_gizmoController == null || _gizmoController.Editor == null)
			{
				_gizmoController = new apGizmoController();
				_gizmoController.SetEditor(this);
			}

			//수정 : 폴더 변경 ("Assets/Editor/AnyPortraitTool/" => apEditorUtil.ResourcePath_Material);

			bool isResetMat = false;

			//감마 / 선형 색상 공간에 따라 Shader를 다른 것을 사용해야한다.
			bool isGammaColorSpace = apEditorUtil.IsGammaColorSpace();


			if (_mat_Color == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Color = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Color.mat"));
				}
				else
				{
					_mat_Color = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Color.mat"));
				}

				isResetMat = true;
			}

			if (_mat_GUITexture == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_GUITexture = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_GUITexture.mat"));
				}
				else
				{
					_mat_GUITexture = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_GUITexture.mat"));
				}
				isResetMat = true;
			}


			//AlphaBlend = 0,
			//Additive = 1,
			//SoftAdditive = 2,
			//Multiplicative = 3


			if (_mat_Texture_Normal == null || _mat_Texture_Normal.Length != 4)
			{
				_mat_Texture_Normal = new Material[4];

				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Texture_Normal[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture.mat"));
					_mat_Texture_Normal[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture Additive.mat"));
					_mat_Texture_Normal[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture SoftAdditive.mat"));
					_mat_Texture_Normal[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture Multiplicative.mat"));
				}
				else
				{
					_mat_Texture_Normal[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture.mat"));
					_mat_Texture_Normal[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture Additive.mat"));
					_mat_Texture_Normal[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture SoftAdditive.mat"));
					_mat_Texture_Normal[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture Multiplicative.mat"));
				}

				isResetMat = true;
			}

			if (_mat_Texture_VertAdd == null || _mat_Texture_VertAdd.Length != 4)
			{
				_mat_Texture_VertAdd = new Material[4];

				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Texture_VertAdd[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture_VColorAdd.mat"));
					_mat_Texture_VertAdd[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture_VColorAdd Additive.mat"));
					_mat_Texture_VertAdd[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture_VColorAdd SoftAdditive.mat"));
					_mat_Texture_VertAdd[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture_VColorAdd Multiplicative.mat"));
				}
				else
				{
					_mat_Texture_VertAdd[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture_VColorAdd.mat"));
					_mat_Texture_VertAdd[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture_VColorAdd Additive.mat"));
					_mat_Texture_VertAdd[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture_VColorAdd SoftAdditive.mat"));
					_mat_Texture_VertAdd[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture_VColorAdd Multiplicative.mat"));
				}
				isResetMat = true;
			}

			if (_mat_Clipped == null || _mat_Clipped.Length != 4)
			{
				_mat_Clipped = new Material[4];

				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_Clipped[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_ClippedTexture.mat"));
					_mat_Clipped[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_ClippedTexture Additive.mat"));
					_mat_Clipped[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_ClippedTexture SoftAdditive.mat"));
					_mat_Clipped[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_ClippedTexture Multiplicative.mat"));
				}
				else
				{
					_mat_Clipped[0] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_ClippedTexture.mat"));
					_mat_Clipped[1] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_ClippedTexture Additive.mat"));
					_mat_Clipped[2] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_ClippedTexture SoftAdditive.mat"));
					_mat_Clipped[3] = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_ClippedTexture Multiplicative.mat"));
				}

				isResetMat = true;
			}

			if (_mat_MaskOnly == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_MaskOnly = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_MaskOnly.mat"));
				}
				else
				{
					_mat_MaskOnly = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_MaskOnly.mat"));
				}
				isResetMat = true;
			}


			if (_mat_ToneColor_Normal == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_ToneColor_Normal = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_ToneColor_Texture.mat"));
				}
				else
				{
					_mat_ToneColor_Normal = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_ToneColor_Texture.mat"));
				}

				isResetMat = true;
			}

			if (_mat_ToneColor_Clipped == null)
			{
				//감마/선형에 따라 다르다
				if (isGammaColorSpace)
				{
					_mat_ToneColor_Clipped = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_ToneColor_Clipped.mat"));
				}
				else
				{
					_mat_ToneColor_Clipped = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_ToneColor_Clipped.mat"));
				}

				isResetMat = true;
			}

			if (_mat_Alpha2White == null)
			{
				if (isGammaColorSpace)
				{
					_mat_Alpha2White = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Alpha2White.mat"));
				}
				else
				{
					_mat_Alpha2White = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Alpha2White.mat"));
				}

				isResetMat = true;
			}

			if (_mat_BoneV2 == null)
			{
				_mat_BoneV2 = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Bone.mat"));
				isResetMat = true;
			}

			if (_mat_Texture_VColorMul == null)
			{
				_mat_Texture_VColorMul = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_TextureAndVColor.mat"));
				isResetMat = true;
			}

			if (_mat_RigCircleV2 == null)
			{
				_mat_RigCircleV2 = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_RigCircle.mat"));
				isResetMat = true;
			}


			if(_mat_Gray_Normal == null)
			{
				if (isGammaColorSpace)
				{
					_mat_Gray_Normal = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_Texture Gray.mat"));
				}
				else
				{
					_mat_Gray_Normal = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_Texture Gray.mat"));
				}
			}
			if(_mat_Gray_Clipped == null)
			{
				if (isGammaColorSpace)
				{
					_mat_Gray_Clipped = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_ClippedTexture Gray.mat"));
				}
				else
				{
					_mat_Gray_Clipped = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("Linear/apMat_L_ClippedTexture Gray.mat"));
				}
			}

			if(_mat_VertPin == null)
			{
				_mat_VertPin = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.MakePath_Material("apMat_VertPin.mat"));
				isResetMat = true;
			}

			if (isResetMat)
			{
				Shader[] shaderSet_Normal = new Shader[4];
				Shader[] shaderSet_VertAdd = new Shader[4];
				Shader[] shaderSet_Clipped = new Shader[4];
				for (int i = 0; i < 4; i++)
				{
					shaderSet_Normal[i] = _mat_Texture_Normal[i].shader;
					shaderSet_VertAdd[i] = _mat_Texture_VertAdd[i].shader;
					shaderSet_Clipped[i] = _mat_Clipped[i].shader;
				}

				apGL.SetShader(	_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, 
								_mat_MaskOnly.shader, shaderSet_Clipped, 
								_mat_GUITexture.shader, 
								_mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, 
								_mat_Alpha2White.shader, 
								_mat_BoneV2.shader, ImageSet.Get(apImageSet.PRESET.BoneSpriteSheet), 
								_mat_Texture_VColorMul.shader, 
								_mat_RigCircleV2.shader, ImageSet.Get(apImageSet.PRESET.Gizmo_RigCircle), 
								_mat_Gray_Normal.shader, 
								_mat_Gray_Clipped.shader,
								_mat_VertPin.shader, ImageSet.Get(apImageSet.PRESET.PinVertGUIAtlas));

				apControllerGL.SetShader(_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, /*shaderSet_Mask, */_mat_MaskOnly.shader, shaderSet_Clipped, _mat_GUITexture.shader, _mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, _mat_Alpha2White.shader, _mat_BoneV2.shader, _mat_Texture_VColorMul.shader, _mat_RigCircleV2.shader, _mat_Gray_Normal.shader, _mat_Gray_Clipped.shader, _mat_VertPin.shader);

				apTimelineGL.SetShader(_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, /*shaderSet_Mask, */_mat_MaskOnly.shader, shaderSet_Clipped, _mat_GUITexture.shader, _mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, _mat_Alpha2White.shader, _mat_BoneV2.shader, _mat_Texture_VColorMul.shader, _mat_RigCircleV2.shader, _mat_Gray_Normal.shader, _mat_Gray_Clipped.shader, _mat_VertPin.shader);
				apAnimCurveGL.SetShader(_mat_Color.shader, shaderSet_Normal, shaderSet_VertAdd, /*shaderSet_Mask, */_mat_MaskOnly.shader, shaderSet_Clipped, _mat_GUITexture.shader, _mat_ToneColor_Normal.shader, _mat_ToneColor_Clipped.shader, _mat_Alpha2White.shader, _mat_BoneV2.shader, _mat_Texture_VColorMul.shader, _mat_RigCircleV2.shader, _mat_Gray_Normal.shader, _mat_Gray_Clipped.shader, _mat_VertPin.shader);
			}


			if (_localization == null)
			{
				_localization = new apLocalization();
			}

			if (_localization.CheckToReloadLanguage(_language))//언어 비교
			{
				//언어 다시 로드
				//경로 변경 : "Assets/Editor/AnyPortraitTool/Util/" => apEditorUtil.ResourcePath_Text
				TextAsset textAsset_Dialog = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.MakePath_Text("apLangPack.txt"));
				TextAsset textAsset_UI = AssetDatabase.LoadAssetAtPath<TextAsset>(apEditorUtil.MakePath_Text("apLangPack_UI.txt"));


				_localization.SetTextAsset(_language, textAsset_Dialog, textAsset_UI);

				try
				{
					if (_hierarchy != null) { _hierarchy.ResetAllUnits(); }
					if (_hierarchy_MeshGroup != null) { _hierarchy_MeshGroup.RefreshUnits(); }
					if (_hierarchy_AnimClip != null) { _hierarchy_AnimClip.RefreshUnits(); }
				}
				catch(Exception) { }

				//추가 19.11.22 : 한번 생성되면 언어가 고정되는 GUIContent들을 전부 리셋해야한다.
				ResetGUIContents();
				if (_selection != null)
				{
					_selection.ResetGUIContents();
				}
			}


			_gizmos.LoadResources();


			if (_guiTopTabStaus == null || _guiTopTabStaus.Count != 6)
			{
				_guiTopTabStaus = new Dictionary<GUITOP_TAB, bool>();
				_guiTopTabStaus.Add(GUITOP_TAB.Tab1_BakeAndSetting, true);
				_guiTopTabStaus.Add(GUITOP_TAB.Tab2_TRSTools, true);
				_guiTopTabStaus.Add(GUITOP_TAB.Tab3_Visibility, true);
				_guiTopTabStaus.Add(GUITOP_TAB.Tab4_FFD_Soft_Blur, true);
				_guiTopTabStaus.Add(GUITOP_TAB.Tab5_GizmoValue, true);
				_guiTopTabStaus.Add(GUITOP_TAB.Tab6_Capture, true);
			}

			//추가 19.6.1
			if (_materialLibrary == null)
			{
				_materialLibrary = new apMaterialLibrary(apPathSetting.I.CurrentPath);
			}
			if (!_materialLibrary.IsLoaded)
			{
				_materialLibrary.Load();//Load!
				_materialLibrary.Save();
			}

			//[v1.4.2] 프로젝트 설정 파일
			if(_projectSettingData == null)
			{
				_projectSettingData = new apProjectSettingData();
			}
			if(!_projectSettingData.IsLoaded)
			{
				_projectSettingData.Load();
				_projectSettingData.Save();
			}



			//Hierarchy는 다른 리소스가 모두 로드된 이후에 로드
			if (_hierarchy == null)
			{
				_hierarchy = new apEditorHierarchy(this);

			}

			if (_hierarchy_MeshGroup == null)
			{
				_hierarchy_MeshGroup = new apEditorMeshGroupHierarchy(this);
			}

			if (_hierarchy_AnimClip == null)
			{
				_hierarchy_AnimClip = new apEditorAnimClipTargetHierarchy(this);
			}


			//추가 19.12.2
			if (_stringFactory == null)
			{
				_stringFactory = new apStringFactory();
			}
			if (!_stringFactory.IsInitialize())
			{
				_stringFactory.Init();
			}

			if (_guiLOFactory == null)
			{
				_guiLOFactory = new apGUILOFactory();
			}
			if (!_guiLOFactory.IsInitialize())
			{
				_guiLOFactory.Init();
			}


			//GUIStyleWrapper가 이미 로드되었는지 확인
			if (_guiStyleWrapper != null && _guiStyleWrapper.IsInitialized())
			{
				return true;
			}

			//로드가 안되었다면 > Event의 타입을 봐야한다.
			if (Event.current == null)
			{
				//Debug.LogError("AnyPortrait : CheckEditorResources : No Event");
				return false;
			}

			if (Event.current.type != EventType.Layout)
			{
				//Debug.LogError("AnyPortrait : CheckEditorResources : No Layout Event [" + Event.current.type + "]");
				return false;
			}

			//추가 19.11.21 : GUIStyle 최적화를 위한 코드
			if (_guiStyleWrapper == null)
			{
				_guiStyleWrapper = new apGUIStyleWrapper();
			}
			if (!_guiStyleWrapper.IsInitialized())
			{
				_guiStyleWrapper.Init();
			}

			//추가 21.1.19 : GUI Workspace에 추가되는 버튼들
			if(_guiButton_Menu == null)
			{
				_guiButton_Menu = new apGUIButton(	ImageSet.Get(apImageSet.PRESET.GUI_Button_Menu), 
													ImageSet.Get(apImageSet.PRESET.GUI_Button_Menu_Roll), 
													GUI_STAT_MENUBTN_SIZE, GUI_STAT_MENUBTN_SIZE);
			}
			if(_guiButton_RecordOnion == null)
			{
				_guiButton_RecordOnion = new apGUIButton(	ImageSet.Get(apImageSet.PRESET.GUI_Button_RecordOnion), 
															ImageSet.Get(apImageSet.PRESET.GUI_Button_RecordOnion_Roll), 
															GUI_STAT_MENUBTN_SIZE, GUI_STAT_MENUBTN_SIZE);
			}

			//추가 22.3.20 [v1.4.0]
			if(_guiButton_MorphEditVert == null)
			{
				_guiButton_MorphEditVert = new apGUIButton(	ImageSet.Get(apImageSet.PRESET.GUI_Button_EditVert_Enabled),
															ImageSet.Get(apImageSet.PRESET.GUI_Button_EditVert_EnabledRollOver),
															ImageSet.Get(apImageSet.PRESET.GUI_Button_EditVert_Disabled),
															ImageSet.Get(apImageSet.PRESET.GUI_Button_EditVert_RollOver),
															GUI_STAT_MENUBTN_SIZE, GUI_STAT_MENUBTN_SIZE);
			}
			if(_guiButton_MorphEditPin == null)
			{
				_guiButton_MorphEditPin = new apGUIButton(	ImageSet.Get(apImageSet.PRESET.GUI_Button_EditPin_Enabled),
															ImageSet.Get(apImageSet.PRESET.GUI_Button_EditPin_EnabledRollOver),
															ImageSet.Get(apImageSet.PRESET.GUI_Button_EditPin_Disabled),
															ImageSet.Get(apImageSet.PRESET.GUI_Button_EditPin_RollOver),
															GUI_STAT_MENUBTN_SIZE, GUI_STAT_MENUBTN_SIZE);
			}

			//추가 21.2.18 : GUI에 아이콘을 표시한다.
			if(_guiStatBox == null)
			{
				_guiStatBox = new apGUIStatBox(this);
			}

			if(_guiHowToUse == null)
			{
				_guiHowToUse = new apGUIHowToUseTips(this);
			}

			//GUIStyle 로드 직후에 Reset 가능
			//변경 21.8.3 : Portrait가 있고 ObjectOrder가 Sync된 이후에만 가능
			if(_portrait != null && 
				_portrait._objectOrders != null &&
				_portrait._objectOrders.IsSync)
			{
				_hierarchy.ResetAllUnits();
			}
			
			_hierarchy.ReloadUnitResources();
			_hierarchy_MeshGroup.ReloadUnitResources();
			_hierarchy_AnimClip.ReloadUnitResources();

			if(_cppPluginOption_UsePlugin 
				&& _cppPluginValidateResult == apPluginUtil.VALIDATE_RESULT.Unknown)
			{
				//추가 21.5.13 : CPP 로드 테스트
				_cppPluginValidateResult = apPluginUtil.I.ValidateDLL();
			}

			//렌더 요청 초기화
			if(_renderRequest_Normal == null)
			{
				_renderRequest_Normal = new apGL.RenderTypeRequest();
			}
			if(_renderRequest_Selected == null)
			{
				_renderRequest_Selected = new apGL.RenderTypeRequest();
			}

			if(_guiRenderSettings == null)
			{
				_guiRenderSettings = new apGUIRenderSettings();
			}
			
			return true;
		}



		//----------------------------------------------------------------------------
		// UI 이벤트 / 함수들
		//----------------------------------------------------------------------------
		
		// Hierarchy 클릭 이벤트

		/// <summary>
		/// [1.4.2] : Hierarchy에서 Unit을 클릭하면 이 함수를 호출하자
		/// </summary>
		/// <param name="targetHierarchy"></param>
		public void OnHierachyClicked(LAST_CLICKED_HIERARCHY targetHierarchy)
		{
			if (_isReadyToCheckClickedHierarchy)
			{
				_isClickedHierarchyProcessed = true;
				_lastClickedHierarchy = targetHierarchy;

				//Debug.Log("Hierarchy를 클릭했다 [" + targetHierarchy + "]");
			}
		}


		/// <summary>
		/// GUI Top과 Right에서 사용되는 단축키들을 여기서 등록한다.
		/// </summary>
		private void ProcessHotKey_GUITopRight()
		{
			bool isGizmoUpdatable = Gizmos.IsUpdatable;
			//기즈모를 단축키로 넣자
			if (isGizmoUpdatable)
			{
				//다시 변경 20.12.3
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoSelect,	apHotKeyMapping.KEY_TYPE.Gizmo_Select, null);
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoMove,		apHotKeyMapping.KEY_TYPE.Gizmo_Move, null);
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoRotate,	apHotKeyMapping.KEY_TYPE.Gizmo_Rotate, null);
				AddHotKeyEvent(Controller.OnHotKeyEvent_GizmoScale,		apHotKeyMapping.KEY_TYPE.Gizmo_Scale, null);
			}

			//Onion
			bool isOnionButtonAvailable = false;
			if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation ||
				Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				isOnionButtonAvailable = true;
			}

			//Onion 단축키
			if (isOnionButtonAvailable)
			{
				//AddHotKeyEvent(Controller.OnHotKeyEvent_OnionVisibleToggle, apHotKey.LabelText.OnionSkinToggle, KeyCode.O, false, false, false, null);//"Onion Skin Toggle"
				AddHotKeyEvent(Controller.OnHotKeyEvent_OnionVisibleToggle, apHotKeyMapping.KEY_TYPE.ToggleOnionSkin, null);//변경 20.12.3
			}

			//Bone Visible
			bool isBoneMeshVisibleButtonAvailable = _selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Animation ||
				_selection.SelectionType == apSelection.SELECTION_TYPE.Overall;

			if (isBoneMeshVisibleButtonAvailable)
			{
				//단축키 B로 Bone 렌더링 정보를 토글할 수 있다.
				//AddHotKeyEvent(OnHotKeyEvent_BoneVisibleToggle, apHotKey.LabelText.ChangeBoneVisiblity, KeyCode.B, false, false, false, null);//"Change Bone Visiblity"
				AddHotKeyEvent(OnHotKeyEvent_BoneVisibleToggle, apHotKeyMapping.KEY_TYPE.ToggleBoneVisibility, null);//변경 20.12.3
				AddHotKeyEvent(OnHotKeyEvent_MeshVisibleToggle, apHotKeyMapping.KEY_TYPE.ToggleMeshVisibility, null);//추가 21.1.21 : 메시 가시성 전환
				AddHotKeyEvent(OnHotKeyEvent_PhysicsToggle, apHotKeyMapping.KEY_TYPE.TogglePhysicsPreview, null);//추가 21.1.21 : 물리 미리보기 전환

				//추가 21.1.21 : 가시성 관련으로 옵션을 추가한다.
				AddHotKeyEvent(OnHotKeyEvent_ToggleVisiblityPreset, apHotKeyMapping.KEY_TYPE.TogglePresetVisibility, null);
				AddHotKeyEvent(OnHotKeyEvent_SelectVisibilitPresetRule, apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule1, apVisibilityPresets.HOTKEY.Hotkey1);
				AddHotKeyEvent(OnHotKeyEvent_SelectVisibilitPresetRule, apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule2, apVisibilityPresets.HOTKEY.Hotkey2);
				AddHotKeyEvent(OnHotKeyEvent_SelectVisibilitPresetRule, apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule3, apVisibilityPresets.HOTKEY.Hotkey3);
				AddHotKeyEvent(OnHotKeyEvent_SelectVisibilitPresetRule, apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule4, apVisibilityPresets.HOTKEY.Hotkey4);
				AddHotKeyEvent(OnHotKeyEvent_SelectVisibilitPresetRule, apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule5, apVisibilityPresets.HOTKEY.Hotkey5);

				//추가 21.2.28 : 로토스코핑 옵션 추가
				AddHotKeyEvent(OnHotKey_ToggleRotoscoping, apHotKeyMapping.KEY_TYPE.ToggleRotoscoping, null);
				AddHotKeyEvent(OnHotKey_RotoscopingSwitchingImage, apHotKeyMapping.KEY_TYPE.RotoscopingPrev, false);
				AddHotKeyEvent(OnHotKey_RotoscopingSwitchingImage, apHotKeyMapping.KEY_TYPE.RotoscopingNext, true);

				//추가 21.6.4 : 가이드라인 단축키
				AddHotKeyEvent(OnHotKeyEvent_ToggleGuidelines, apHotKeyMapping.KEY_TYPE.ToggleGuidelines, null);
			}



			if (_selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup || _selection.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				//변경 21.2.13 : 기존의 모디파이어 잠금 기능의 단축키가 나뉘어졌다.
				AddHotKeyEvent(OnHotKeyEvent_ExModOptions, apHotKeyMapping.KEY_TYPE.ExObj_UpdateByOtherMod, 0);
				AddHotKeyEvent(OnHotKeyEvent_ExModOptions, apHotKeyMapping.KEY_TYPE.ExObj_ShowAsGray, 1);
				AddHotKeyEvent(OnHotKeyEvent_ExModOptions, apHotKeyMapping.KEY_TYPE.ExObj_ToggleSelectionSemiLock, 2);
				
				AddHotKeyEvent(OnHotKeyEvent_ShowCalculatedBones, apHotKeyMapping.KEY_TYPE.PreviewModBoneResult, null);
				AddHotKeyEvent(OnHotKeyEvent_ShowCalculatedColor, apHotKeyMapping.KEY_TYPE.PreviewModColorResult, null);
				AddHotKeyEvent(OnHotKeyEvent_ShowModifierListUI, apHotKeyMapping.KEY_TYPE.ShowModifierListUI, null);
				AddHotKeyEvent(OnHotKeyEvent_ToggleMorphTarget, apHotKeyMapping.KEY_TYPE.ToggleMorphTarget, null);
				
			}
			

			


			//Vertex 제어시 단축키
			apGizmos.TRANSFORM_UI_VALID gizmoUI_VetexTF = Gizmos.TransformUI_VertexTransform;

			if (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide && Gizmos.IsSoftSelectionMode)
			{
				//Vertex Transform - Soft 툴
				//크기 조절 단축키
				//변경 20.12.3
				AddHotKeyEvent(Gizmos.IncreaseSoftSelectionRadius, apHotKeyMapping.KEY_TYPE.IncreaseModToolBrushSize, null);//"Increase Brush Size"
				AddHotKeyEvent(Gizmos.DecreaseSoftSelectionRadius, apHotKeyMapping.KEY_TYPE.DecreaseModToolBrushSize, null);//"Decrease Brush Size"
			}
			else if (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide && Gizmos.IsBrushMode)
			{
				//Vertex Transform - Blur 툴
				//변경 20.12.3
				AddHotKeyEvent(OnHotKeyEvent_IncBlurBrushRadius, apHotKeyMapping.KEY_TYPE.IncreaseModToolBrushSize, null);//"Increase Brush Size"
				AddHotKeyEvent(OnHotKeyEvent_DecBlurBrushRadius, apHotKeyMapping.KEY_TYPE.DecreaseModToolBrushSize, null);//"Decreash Brush Size"
			}


			if (Select.SelectionType == apSelection.SELECTION_TYPE.Mesh
				&& Select.Mesh != null
				&& _meshEditMode == MESH_EDIT_MODE.MakeMesh
				&& _meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.AddTools
				&& _meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon)
			{
				//Delete 키로 Polygon을 삭제하는 단축키
				AddHotKeyEvent(Controller.RemoveSelectedMeshPolygon, apHotKeyMapping.KEY_TYPE.MakeMesh_RemovePolygon, null);//변경 20.12.3
			}
		}

		
		//----------------------------------------------------------------------
		// 보기 메뉴
		//----------------------------------------------------------------------
		//추가 21.1.19 : GUI Menu의 View 버튼을 눌렀을 때
		private void OnViewMenu(object obj)
		{	
			if(obj == null)
			{
				return;
			}
			apGUIMenu.MenuCallBackParam guiParam = obj as apGUIMenu.MenuCallBackParam;
			if(guiParam == null)
			{
				//파라미터 타입이 맞지 않는다.
				return;
			}

			apGUIMenu.MENU_ITEM__GUIVIEW menuType = guiParam._menuType;
			object subParam = guiParam._objParam;

			switch (menuType)
			{
				case apGUIMenu.MENU_ITEM__GUIVIEW.FPS:
					_guiOption_isFPSVisible = !_guiOption_isFPSVisible;
					SaveEditorPref();
					break;
				case apGUIMenu.MENU_ITEM__GUIVIEW.Statistics:
					_guiOption_isStatisticsVisible = !_guiOption_isStatisticsVisible;
					SaveEditorPref();
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.MaximizeWorkspace:
					_isFullScreenGUI = !_isFullScreenGUI;
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.InvertBackground://배경색 전환 (21.10.6)
					_isInvertBackgroundColor = !_isInvertBackgroundColor;
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.Mesh:
					if(_meshGUIRenderMode == MESH_RENDER_MODE.Render)	{ _meshGUIRenderMode = MESH_RENDER_MODE.None; }
					else												{ _meshGUIRenderMode = MESH_RENDER_MODE.Render; }
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.Bone_Show:
					if(_boneGUIRenderMode == apEditor.BONE_RENDER_MODE.Render)	{ _boneGUIRenderMode = BONE_RENDER_MODE.None; }
					else														{ _boneGUIRenderMode = BONE_RENDER_MODE.Render; }
					SaveEditorPref();
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.Bone_Outline:
					if(_boneGUIRenderMode == apEditor.BONE_RENDER_MODE.RenderOutline)	{ _boneGUIRenderMode = BONE_RENDER_MODE.None; }
					else																{ _boneGUIRenderMode = BONE_RENDER_MODE.RenderOutline; }
					SaveEditorPref();
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.Physics:
					if(_portrait != null)
					{
						_portrait._isPhysicsPlay_Editor = !_portrait._isPhysicsPlay_Editor;
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ToggleOnionSkin:
					Onion.SetVisible(!Onion.IsVisible);
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.OnionSkinSetting:
					apDialog_OnionSetting.ShowDialog(this, _portrait);//<<Onion 설정 다이얼로그를 호출
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ToggleVisibilityPreset:
					_isAdaptVisibilityPreset = !_isAdaptVisibilityPreset;
					//만약 규칙이 없다면, 맨 위에있는 규칙을 선택하자
					if(_selectedVisibilityPresetRule == null)
					{
						if(_portrait != null && _portrait.VisiblePreset != null)
						{
							int nRules = _portrait.VisiblePreset._rules != null ? _portrait.VisiblePreset._rules.Count : 0;
							if(nRules > 0)
							{
								//첫번째것을 선택한다.
								_selectedVisibilityPresetRule = _portrait.VisiblePreset._rules[0];
							}
						}
					}

					if(_selectedVisibilityPresetRule == null)
					{
						//선택한 규칙이 없다면
						_isAdaptVisibilityPreset = false;
					}
					


					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.VisibilityPresetSettings:
					if(_portrait != null)
					{
						apMeshGroup curSelectedMeshGroup = null;
						if(Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
						{
							curSelectedMeshGroup = Select.MeshGroup;
						}
						else if(Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
						{
							if(Select.AnimClip != null)
							{
								curSelectedMeshGroup = Select.AnimClip._targetMeshGroup;
							}
						}
						apDialog_VisibilityPresets.ShowDialog(this, _portrait.VisiblePreset, curSelectedMeshGroup);//추가 21.1.22 : VisibilityPreset 설정창
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.VisibilityRule:
					//Sub Param으로 선택
					if(subParam != null 
						&& _portrait != null 
						&& _portrait.VisiblePreset != null
						&& subParam is apVisibilityPresets.RuleData)
					{
						apVisibilityPresets.RuleData nextRuleData = subParam as apVisibilityPresets.RuleData;
						if(_portrait.VisiblePreset.IsContains(nextRuleData))
						{
							//유효한 규칙이라면
							_selectedVisibilityPresetRule = nextRuleData;

							if(!_isAdaptVisibilityPreset && _selectedVisibilityPresetRule != null)
							{
								//Rule 바꿀때 자동으로 프리셋 활성
								_isAdaptVisibilityPreset = true;
							}
						}
					}
					else
					{
						_selectedVisibilityPresetRule = null;
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ToggleRotoscoping:
					{
						int nData = Rotoscoping._imageSetDataList != null ? Rotoscoping._imageSetDataList.Count : 0;
						bool prevRoto = _isEnableRotoscoping;
						_isEnableRotoscoping = !_isEnableRotoscoping;

						if(_isEnableRotoscoping && nData == 0)
						{
							_isEnableRotoscoping = false;
						}

						//만약 데이터가 없다면, 맨 위에있는 데이터를 선택하자
						if(_isEnableRotoscoping && _selectedRotoscopingData == null)
						{	
							if(nData > 0)
							{
								_selectedRotoscopingData = Rotoscoping._imageSetDataList[0];
								_selectedRotoscopingData.LoadImages();//이미지들을 열자
								_iRotoscopingImageFile = 0;
							}

							if(_selectedRotoscopingData == null)
							{
								_isEnableRotoscoping = false;
								_iRotoscopingImageFile = 0;
							}
						}

						if(prevRoto != _isEnableRotoscoping && !_isEnableRotoscoping)
						{
							//ON > OFF로 바뀐거라면 이미지파일 모두 해제
							Rotoscoping.DestroyAllImages();
						}

						_iSyncRotoscopingAnimClipFrame = -1;
						_isSyncRotoscopingToAnimClipFrame = false;
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.PrevRotoscopingImage:
					{
						//이미지 파일 인덱스를 이전으로 옮긴다. (유효한 경우에만)
						if(_isEnableRotoscoping && _selectedRotoscopingData != null)
						{
							int nImageFiles = _selectedRotoscopingData._filePathList != null ? _selectedRotoscopingData._filePathList.Count : 0;
							if (nImageFiles == 0)
							{
								_iRotoscopingImageFile = 0;
							}
							else
							{
								_iRotoscopingImageFile--;
								if (_iRotoscopingImageFile < 0)
								{
									_iRotoscopingImageFile = nImageFiles - 1;
								}
							}
						}
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.NextRotoscopingImage:
					{
						//이미지 파일 인덱스를 다음으로 옮긴다. (유효한 경우에만)
						if(_isEnableRotoscoping && _selectedRotoscopingData != null)
						{
							int nImageFiles = _selectedRotoscopingData._filePathList != null ? _selectedRotoscopingData._filePathList.Count : 0;
							if (nImageFiles == 0)
							{
								_iRotoscopingImageFile = 0;
							}
							else
							{
								_iRotoscopingImageFile++;
								if (_iRotoscopingImageFile >= nImageFiles)
								{
									_iRotoscopingImageFile = 0;
								}
							}
						}
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.RotoscopingSettings:
					{
						// 로토스코핑 설정 열기
						apDialog_Rotoscoping.ShowDialog(this);
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.RotoscopingData:
					{
						//로토스코핑 데이터를 바꾸자
						if (subParam != null && subParam is apRotoscoping.ImageSetData)
						{
							apRotoscoping.ImageSetData nextData = subParam as apRotoscoping.ImageSetData;
							if(_selectedRotoscopingData != nextData)
							{
								//기존 이미지는 해제
								if(_selectedRotoscopingData != null)
								{
									_selectedRotoscopingData.DestroyImages();
								}
								
								//바꾸자
								_selectedRotoscopingData = nextData;
								_selectedRotoscopingData.LoadImages();//이미지 열기
								
								//바꿀때는 인덱스 초기화
								_iRotoscopingImageFile = 0;
								
							}
							_isEnableRotoscoping = true;

							_iSyncRotoscopingAnimClipFrame = -1;
							_isSyncRotoscopingToAnimClipFrame = false;
						}
						else
						{
							_selectedRotoscopingData = null;
							_isEnableRotoscoping = false;

							_iSyncRotoscopingAnimClipFrame = -1;
							_isSyncRotoscopingToAnimClipFrame = false;

							Rotoscoping.DestroyAllImages();//이미지 모두 해제
						}
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ModEditingSettings:
					{
						// 편집 모드 설정
						apDialog_ModifierLockSetting.ShowDialog(this, _portrait);
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ExModObj_UpdateByOtherModifiers:
					{
						//비편집 > 다른 모디파이어로 업데이트
						_exModObjOption_UpdateByOtherMod = !_exModObjOption_UpdateByOtherMod;

						//FFD 모드는 취소한다.
						if(Gizmos.IsFFDMode)
						{
							Gizmos.RevertFFDTransformForce();
						}

						
						//이전
						//Select.RefreshModifierExclusiveEditing();
						//Select.RefreshAnimEditingLayerLock();

						Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.14

						
						SaveEditorPref();
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ExModObj_ShowAsGray:
					{
						//비편집 > 회색으로 표시
						_exModObjOption_ShowGray = !_exModObjOption_ShowGray;
						
						//이전
						//Select.RefreshModifierExclusiveEditing();
						//Select.RefreshAnimEditingLayerLock();

						Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.14
						
						SaveEditorPref();
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ExModObj_NotSelectable:
					{
						//비편집 > 선택 불가
						_exModObjOption_NotSelectable = !_exModObjOption_NotSelectable;
						
						// 이전
						//Select.RefreshModifierExclusiveEditing();
						//Select.RefreshAnimEditingLayerLock();

						Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.14

						
						SaveEditorPref();
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ExModObj_PreviewColorResult:
					{
						//색상 처리 결과 미리보기
						_modLockOption_ColorPreview = !_modLockOption_ColorPreview;
						
						//이전
						//Select.RefreshModifierExclusiveEditing();
						//Select.RefreshAnimEditingLayerLock();

						Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.14

						SaveEditorPref();
					}
					break;
				case apGUIMenu.MENU_ITEM__GUIVIEW.ExModObj_PreviewBoneResult:
					{
						//본 처리 결과 미리보기
						_modLockOption_BoneResultPreview = !_modLockOption_BoneResultPreview;
						
						//이전
						//Select.RefreshModifierExclusiveEditing();
						//Select.RefreshAnimEditingLayerLock();

						Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.14
						
						SaveEditorPref();
					}
					break;
				case apGUIMenu.MENU_ITEM__GUIVIEW.ExModObj_ShowModifierList:
					{
						_modLockOption_ModListUI = !_modLockOption_ModListUI;
						SaveEditorPref();
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.HowToEdit:
					{
						//추가 21.3.13 : 편집 방법 알려주는 UI 보이기
						_guiOption_isShowHowToEdit = !_guiOption_isShowHowToEdit;
						SaveEditorPref();
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.ToggleGuidelines:
					{
						//추가 21.6.4 : 가이드라인 보여주기
						_isEnableGuideLine = !_isEnableGuideLine;
					}
					break;

				case apGUIMenu.MENU_ITEM__GUIVIEW.GuideLinesSettings:
					{
						//추가 21.6.4 : 가이드라인 설정 보여주기
						apDialog_GuideLines.ShowDialog(this);
					}
					break;

			}
		}





		//-----------------------------------------------------------------------
		// Hierarchy / Controller / Timeline 갱신 함수들
		//-----------------------------------------------------------------------

		/// <summary>Hierarchy 완전히 리셋하기</summary>
		public void ResetHierarchyAll()
		{
			if (_portrait == null)
			{
				return;
			}

			//추가 3.29 : Hierarchy 재정렬
			if (_portrait._objectOrders == null)
			{
				_portrait._objectOrders = new apObjectOrders();
			}
			_portrait._objectOrders.Sync(_portrait);
			switch (_hierarchySortMode)
			{
				case HIERARCHY_SORT_MODE.RegOrder: _portrait._objectOrders.SortByRegOrder(); break;
				case HIERARCHY_SORT_MODE.AlphaNum: _portrait._objectOrders.SortByAlphaNumeric(); break;
				case HIERARCHY_SORT_MODE.Custom: _portrait._objectOrders.SortByCustom(); break;
			}

			Hierarchy.ResetAllUnits();
			_hierarchy_MeshGroup.ResetSubUnits();
			_hierarchy_AnimClip.ResetSubUnits();
		}


		/// <summary>
		/// 컨트롤러와 Hierarchy UI를 갱신한다. 옵션에 따라서 Timeline도 다시 갱신한다.
		/// </summary>
		/// <param name="isRefreshTimeline"></param>
		public void RefreshControllerAndHierarchy(bool isRefreshTimeline)//<<인자 추가 19.5.21
		{
			if (_portrait == null)
			{
				//Repaint();
				SetRepaint();
				return;
			}

			//메시 그룹들을 체크한다.
			Controller.RefreshMeshGroups();
			Controller.CheckMeshEdgeWorkRemained();

			//추가 3.29 : Hierarchy 재정렬
			if (_portrait._objectOrders == null)
			{
				_portrait._objectOrders = new apObjectOrders();
			}
			_portrait._objectOrders.Sync(_portrait);
			switch (_hierarchySortMode)
			{
				case HIERARCHY_SORT_MODE.RegOrder: _portrait._objectOrders.SortByRegOrder(); break;
				case HIERARCHY_SORT_MODE.AlphaNum: _portrait._objectOrders.SortByAlphaNumeric(); break;
				case HIERARCHY_SORT_MODE.Custom: _portrait._objectOrders.SortByCustom(); break;
			}

			Hierarchy.RefreshUnits();
			_hierarchy_MeshGroup.RefreshUnits();
			_hierarchy_AnimClip.RefreshUnits();

			//이전
			//RefreshTimelineLayers(false);

			//변경 19.5.21
			if (isRefreshTimeline && Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				RefreshTimelineLayers(REFRESH_TIMELINE_REQUEST.All, null, null);
			}


			//통계 재계산 요청
			Select.SetStatisticsRefresh();


			//Repaint();
			SetRepaint();
		}

		//추가 19.5.21 : RefreshTimelineLayers 함수 속도 최적화를 위해서 Request 인자를 받아서 처리한다.
		[Flags]
		public enum REFRESH_TIMELINE_REQUEST
		{
			None = 0,
			Info = 1,
			Timelines = 2,
			LinkKeyframeAndModifier = 4,
			All = 1 | 2 | 4
		}

		//이전
		//public void RefreshTimelineLayers(bool isReset)

		//변경 19.5.21 : 요청 세분화
		/// <summary>
		/// 타임라인을 갱신한다.
		/// 특정 타임라인 레이어나 타임라인 레이어들을 입력하면 해당 레이어만 갱신하므로 속도가 향상된다.
		/// </summary>
		/// <param name="requestType">갱신 방식</param>
		/// <param name="targetTimelineLayer">갱신하고자 하는 타임라인 레이어. Null이면 전체 갱신이다.</param>
		/// <param name="targetTimelineLayers">갱신하고자 하는 타임라인 레이어 리스트. 여러개를 갱신할 땐 이걸 이용하자. 이것도 Null이면 전체 갱신이다.</param>
		public void RefreshTimelineLayers(REFRESH_TIMELINE_REQUEST requestType, 
											apAnimTimelineLayer targetTimelineLayer,
											List<apAnimTimelineLayer> targetTimelineLayers
			)
		{
			//Debug.LogError("TODO : RefreshTimelineLayers 이것도 다중 갱신을 처리해야한다.");
			apAnimClip curAnimClip = Select.AnimClip;
			if (curAnimClip == null)
			{
				_prevAnimClipForTimeline = null;
				_timelineInfoList.Clear();

				//Common Keyframe을 갱신하자
				Select.RefreshCommonAnimKeyframes();
				return;
			}
			if (curAnimClip != _prevAnimClipForTimeline)
			{
				//강제로 리셋한다.
				//이전
				//isReset = true;

				//변경 19.5.21
				requestType |= REFRESH_TIMELINE_REQUEST.All;
				_prevAnimClipForTimeline = curAnimClip;
			}

			bool isRequest_ResetTimelineInfo = (int)(requestType & REFRESH_TIMELINE_REQUEST.Info) != 0;
			bool isRequest_RefreshTimelines = (int)(requestType & REFRESH_TIMELINE_REQUEST.Timelines) != 0;
			bool isRequest_LinkKeyframeAndModifier = (int)(requestType & REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier) != 0;


			bool isTargetTimelineLayer_Multiple = targetTimelineLayers != null && targetTimelineLayers.Count > 0;
			bool isTargetTimelineLayer_Single = !isTargetTimelineLayer_Multiple && targetTimelineLayer != null;//다중 선택이 입력된 경우 단일 선택은 무시한다.
			

			if (isRequest_RefreshTimelines) //<조건문 추가 19.5.21
			{
				//타임라인값도 리프레시 (Sorting 등)
				curAnimClip.RefreshTimelines(targetTimelineLayer, targetTimelineLayers);
			}

			
			//조건문 변경 19.5.21
			if (isRequest_ResetTimelineInfo || _timelineInfoList.Count == 0)
			{
				_timelineInfoList.Clear();
				//AnimClip에 맞게 리스트를 다시 만든다.

				List<apAnimTimeline> timelines = curAnimClip._timelines;
				for (int iTimeline = 0; iTimeline < timelines.Count; iTimeline++)
				{
					apAnimTimeline timeline = timelines[iTimeline];
					apTimelineLayerInfo timelineInfo = new apTimelineLayerInfo(timeline);
					_timelineInfoList.Add(timelineInfo);


					List<apTimelineLayerInfo> subLayers = new List<apTimelineLayerInfo>();
					for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
					{
						apAnimTimelineLayer animLayer = timeline._layers[iLayer];
						subLayers.Add(new apTimelineLayerInfo(animLayer, timeline, timelineInfo));
					}

					//정렬을 여기서 한다.
					switch (_timelineInfoSortType)
					{
						case TIMELINE_INFO_SORT.Registered:
							//정렬을 안한다.
							break;

						case TIMELINE_INFO_SORT.Depth:
							if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
							{
								//Modifier가 Transform을 지원하는 경우
								//Bone이 위쪽에 속한다.

								if (curAnimClip._targetMeshGroup == null)
								{
									//기존 방식
									subLayers.Sort(delegate (apTimelineLayerInfo a, apTimelineLayerInfo b)
									{
										if (a._layerType == b._layerType)
										{
											if (a._layerType == apTimelineLayerInfo.LAYER_TYPE.Transform)
											{
												return b.Depth - a.Depth;
											}
											else
											{
												return a.Depth - b.Depth;
											}

										}
										else
										{
											return (int)a._layerType - (int)b._layerType;
										}
									});
								}
								else
								{
									List<object> sortedObjects = Controller.GetSortedSubObjectsAsHierarchy(curAnimClip._targetMeshGroup, true, true);
									subLayers.Sort(delegate (apTimelineLayerInfo a, apTimelineLayerInfo b)
									{
										if (a._layerType == b._layerType)
										{
											//변경 sortedObject의 값을 이용하자
											object objA = null;
											object objB = null;
											if(a._layer._linkedMeshTransform != null)
											{
												objA = a._layer._linkedMeshTransform;
											}
											else if(a._layer._linkedMeshGroupTransform != null)
											{
												objA = a._layer._linkedMeshGroupTransform;
											}
											else if(a._layer._linkedBone != null)
											{
												objA = a._layer._linkedBone;
											}

											if(b._layer._linkedMeshTransform != null)
											{
												objB = b._layer._linkedMeshTransform;
											}
											else if(b._layer._linkedMeshGroupTransform != null)
											{
												objB = b._layer._linkedMeshGroupTransform;
											}
											else if(b._layer._linkedBone != null)
											{
												objB = b._layer._linkedBone;
											}

											return sortedObjects.IndexOf(objA) - sortedObjects.IndexOf(objB);

											//if (a._layerType == apTimelineLayerInfo.LAYER_TYPE.Transform)
											//{
											//	return b.Depth - a.Depth;
											//}
											//else
											//{
											//	return a.Depth - b.Depth;
											//}

										}
										else
										{
											return (int)a._layerType - (int)b._layerType;
										}
									});
								}
								

								
							}
							break;

						case TIMELINE_INFO_SORT.ABC:
							subLayers.Sort(delegate (apTimelineLayerInfo a, apTimelineLayerInfo b)
							{
								return string.Compare(a.DisplayName, b.DisplayName);
							});
							break;
					}

					//정렬 된 걸 넣어주자
					for (int iSub = 0; iSub < subLayers.Count; iSub++)
					{
						_timelineInfoList.Add(subLayers[iSub]);
					}

				}
			}

			//조건문 추가 19.5.21
			//키프레임-모디파이어 연동도 해주자
			if (isRequest_LinkKeyframeAndModifier)
			{
				//if (targetTimelineLayer == null)
				if(!isTargetTimelineLayer_Single && !isTargetTimelineLayer_Multiple)//변경 20.6.19 : 타겟 레이어가 없는 경우
				{
					//전체 링크
					for (int iTimeline = 0; iTimeline < curAnimClip._timelines.Count; iTimeline++)
					{
						apAnimTimeline timeline = curAnimClip._timelines[iTimeline];
						if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
							timeline._linkedModifier != null)
						{
							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								apAnimTimelineLayer layer = timeline._layers[iLayer];

								apModifierParamSetGroup paramSetGroup = timeline._linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
								{
									return a._keyAnimTimelineLayer == layer;
								});

								if (paramSetGroup != null)
								{
									for (int iKey = 0; iKey < layer._keyframes.Count; iKey++)
									{
										apAnimKeyframe keyframe = layer._keyframes[iKey];
										apModifierParamSet paramSet = paramSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return a.SyncKeyframe == keyframe;
										});

										if (paramSet != null && paramSet._meshData.Count > 0)
										{
											keyframe.LinkModMesh_Editor(paramSet, paramSet._meshData[0]);
										}
										else if (paramSet != null && paramSet._boneData.Count > 0)//<<추가 : boneData => ModBone
										{
											keyframe.LinkModBone_Editor(paramSet, paramSet._boneData[0]);
										}
										else
										{
											keyframe.LinkModMesh_Editor(null, null);//<<null, null을 넣으면 ModBone도 Null이 된다.
										}
									}
								}
							}

						}
					}
				}
				else if(isTargetTimelineLayer_Single)
				{
					//[단일]
					//특정 TimelineLayer만 링크
					apAnimTimeline parentTimeline = targetTimelineLayer._parentTimeline;

					if (parentTimeline != null &&
						parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
							parentTimeline._linkedModifier != null)
					{
						apModifierParamSetGroup paramSetGroup = parentTimeline._linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
						{
							return a._keyAnimTimelineLayer == targetTimelineLayer;
						});

						if (paramSetGroup != null)
						{
							for (int iKey = 0; iKey < targetTimelineLayer._keyframes.Count; iKey++)
							{
								apAnimKeyframe keyframe = targetTimelineLayer._keyframes[iKey];
								apModifierParamSet paramSet = paramSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
								{
									return a.SyncKeyframe == keyframe;
								});

								if (paramSet != null && paramSet._meshData.Count > 0)
								{
									keyframe.LinkModMesh_Editor(paramSet, paramSet._meshData[0]);
								}
								else if (paramSet != null && paramSet._boneData.Count > 0)//<<추가 : boneData => ModBone
								{
									keyframe.LinkModBone_Editor(paramSet, paramSet._boneData[0]);
								}
								else
								{
									keyframe.LinkModMesh_Editor(null, null);//<<null, null을 넣으면 ModBone도 Null이 된다.
								}
							}
						}
					}
				}
				else if(isTargetTimelineLayer_Multiple)
				{
					//[다중] 20.6.19
					//리스트의 타임라인 레이어만 링크
					apAnimTimelineLayer curLayer = null;
					apAnimTimeline curParentTimeline = null;
					for (int iLayer = 0; iLayer < targetTimelineLayers.Count; iLayer++)
					{
						curLayer = targetTimelineLayers[iLayer];
						if(curLayer == null)
						{
							continue;
						}
						curParentTimeline = curLayer._parentTimeline;

						if (curParentTimeline != null &&
							curParentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
								curParentTimeline._linkedModifier != null)
						{
							apModifierParamSetGroup paramSetGroup = curParentTimeline._linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
							{
								return a._keyAnimTimelineLayer == curLayer;
							});

							if (paramSetGroup != null)
							{
								for (int iKey = 0; iKey < curLayer._keyframes.Count; iKey++)
								{
									apAnimKeyframe keyframe = curLayer._keyframes[iKey];
									apModifierParamSet paramSet = paramSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
									{
										return a.SyncKeyframe == keyframe;
									});

									if (paramSet != null && paramSet._meshData.Count > 0)
									{
										keyframe.LinkModMesh_Editor(paramSet, paramSet._meshData[0]);
									}
									else if (paramSet != null && paramSet._boneData.Count > 0)//<<추가 : boneData => ModBone
									{
										keyframe.LinkModBone_Editor(paramSet, paramSet._boneData[0]);
									}
									else
									{
										keyframe.LinkModMesh_Editor(null, null);//<<null, null을 넣으면 ModBone도 Null이 된다.
									}
								}
							}
						}
					}
				}

			}


			//Select / Available 체크를 하자 : 이건 상시
			for (int i = 0; i < _timelineInfoList.Count; i++)
			{
				apTimelineLayerInfo info = _timelineInfoList[i];

				info._isSelected = false;
				info._isAvailable = false;

				if (info._isTimeline)
				{
					if (Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
					{
						if (info._timeline == Select.AnimTimeline)
						{ info._isAvailable = true; }
					}
					else
					{
						info._isAvailable = true;
					}

					if (info._isAvailable)
					{
						//변경 20.6.17 : 다중 선택을 인식해야한다.
						//if (Select.AnimTimelineLayer == null)
						if (Select.NumAnimTimelineLayers == 0)
						{
							if (info._timeline == Select.AnimTimeline)
							{
								info._isSelected = true;
							}
						}
					}
				}
				else
				{
					if (Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
					{
						if (info._parentTimeline == Select.AnimTimeline)
						{
							info._isAvailable = true;

							//이전
							//info._isSelected = (info._layer == Select.AnimTimelineLayer);

							//변경 20.6.17 : 다중 선택 지원
							if (Select.NumAnimTimelineLayers == 1 && Select.AnimTimelineLayer_Main == info._layer)
							{
								//메인 타임라인 레이어
								info._isSelected = true;
							}
							else if(Select.NumAnimTimelineLayers > 1 && Select.AnimTimelineLayers_All.Contains(info._layer))
							{
								//서브 타임라인 레이어
								info._isSelected = true;
							}
						}
					}
					else
					{
						info._isAvailable = true;
						//이전
						//info._isSelected = (info._layer == Select.AnimTimelineLayer);

						//변경 20.6.17 : 다중 선택 지원
						if (Select.NumAnimTimelineLayers == 1 && Select.AnimTimelineLayer_Main == info._layer)
						{
							//메인 타임라인 레이어
							info._isSelected = true;
						}
						else if (Select.NumAnimTimelineLayers > 1 && Select.AnimTimelineLayers_All.Contains(info._layer))
						{
							//서브 타임라인 레이어
							info._isSelected = true;
						}
					}
				}
			}

			//Common Keyframe을 갱신하자
			Select.RefreshCommonAnimKeyframes();


			//통계 재계산 요청
			Select.SetStatisticsRefresh();

			SetRepaint();
		}

		public void ShowAllTimelineLayers()
		{
			for (int i = 0; i < _timelineInfoList.Count; i++)
			{
				_timelineInfoList[i].ShowLayer();
			}
		}

		public void SyncHierarchyOrders()
		{
			if (_portrait == null)
			{
				return;
			}
			//추가 3.29 : Hierarchy Sort
			if (_portrait._objectOrders == null)
			{
				_portrait._objectOrders = new apObjectOrders();
			}
			_portrait._objectOrders.Sync(_portrait);
			switch (HierarchySortMode)
			{
				case HIERARCHY_SORT_MODE.RegOrder:
					_portrait._objectOrders.SortByRegOrder();
					break;
				case HIERARCHY_SORT_MODE.AlphaNum:
					_portrait._objectOrders.SortByAlphaNumeric();
					break;
				case HIERARCHY_SORT_MODE.Custom:
					_portrait._objectOrders.SortByCustom();
					break;
			}
		}

		//----------------------------------------------------------------------
		// 렌더링 서브 함수들
		//----------------------------------------------------------------------
		
		/// <summary>
		/// MeshGroup을 렌더링한다.
		/// </summary>
		/// <param name="isRenderOnlyVisible">단순 재생이면 True, 작업 중이면 False (Alpha가 0인 것도 일단 렌더링을 한다)</param>
		private void RenderMeshGroup(	apMeshGroup meshGroup,
										
										//이전
										//apGL.RENDER_TYPE meshRenderType,
										//apGL.RENDER_TYPE selectedRenderType,
										//변경 22.3.3 (v1.4.0)
										apGL.RenderTypeRequest renderRequest_Normal,
										apGL.RenderTypeRequest renderRequest_Selected,

										//변경 20.5.28
										apTransform_Mesh selectedMeshTF_Main,
										List<apTransform_Mesh> selectedMeshTF_Sub,
										apBone selectedBone,
										List<apBone> selectedBone_Sub,

										bool isRenderOnlyVisible,
										apEditor.BONE_RENDER_MODE boneRenderMode,
										apEditor.MESH_RENDER_MODE meshRenderMode,
										bool isBoneIKUsing,
										BONE_RENDER_TARGET boneRenderTarget,
										bool isSelectedMeshOnly = false,
										bool isUseBoneToneColor = false)
		{
			//Profiler.BeginSample("MeshGroup Render");

			if (meshRenderMode == MESH_RENDER_MODE.Render)
			{
				//이전
				//_tmpSelectedRenderUnits.Clear();

				//변경 20.5.28
				_tmpSelected_MainRenderUnit = null;
				_tmpSelected_SubRenderUnits.Clear();
				
				bool isMeshTF_Main_Checkable = (selectedMeshTF_Main != null);
				bool isMeshTF_Sub_Checkable = (selectedMeshTF_Sub != null && selectedMeshTF_Sub.Count > 0);//Sub 리스트엔 Main도 포함되어 있으므로 2 이상이어야 한다.
				

				List<apRenderUnit> renderUnits = meshGroup.SortedBuffer.SortedRenderUnits;
				int nRenderUnits = renderUnits.Count;



				//선택된 렌더유닛을 먼저 선정. 그 후에 다시 렌더링하자
				//for (int iUnit = 0; iUnit < meshGroup._renderUnits_All.Count; iUnit++)//>>이전 코드
				if (isMeshTF_Main_Checkable || isMeshTF_Sub_Checkable)
				{
					//선택된게 있다면
					apRenderUnit renderUnit = null;
					for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)//<<변경
					{
						renderUnit = renderUnits[iUnit];//<<변경

						//변경 20.5.28
						//메인과 서브를 구분하자
						if (renderUnit._unitType != apRenderUnit.UNIT_TYPE.Mesh
							|| renderUnit._meshTransform == null)
						{
							continue;
						}
						
						if(renderUnit._meshTransform == selectedMeshTF_Main)
						{
							//선택된 "메인" 렌더 유닛
							_tmpSelected_MainRenderUnit = renderUnit;
						}
						else if(isMeshTF_Sub_Checkable)
						{
							//선택된 "서브" 렌더 유닛
							if(selectedMeshTF_Sub.Contains(renderUnit._meshTransform))
							{
								_tmpSelected_SubRenderUnits.Add(renderUnit);
							}
						}
					}
				}

				// Weight 갱신과 Vert Color 연동
				//----------------------------------
				//if ((int)(meshRenderType & apGL.RENDER_TYPE.BoneRigWeightColor) != 0)//이전
				if(renderRequest_Normal.BoneRigWeightColor)//변경 22.3.3
				{
					//Rig Weight를 집어넣자.
					//bool isBoneColor = Select._rigEdit_isBoneColorView;
					//apSelection.RIGGING_EDIT_VIEW_MODE rigViewMode = Select._rigEdit_viewMode;
					apRenderVertex renderVert = null;
					apModifiedMesh modMesh = Select.ModMesh_Main;
					apModifiedVertexRig vertRig = null;
					apModifiedVertexRig.WeightPair weightPair = null;
					apBone selelcedBone = Select.Bone;

					Color colorBlack = Color.black;

					apModifierBase modifier = Select.Modifier;
					if (modifier != null)
					{
						if (modifier._paramSetGroup_controller.Count > 0 &&
							modifier._paramSetGroup_controller[0]._paramSetList.Count > 0)
						{
							List<apModifiedMesh> modMeshes = Select.Modifier._paramSetGroup_controller[0]._paramSetList[0]._meshData;

							for (int iMM = 0; iMM < modMeshes.Count; iMM++)
							{
								modMesh = modMeshes[iMM];
								if (modMesh != null)
								{
									//modMesh.RefreshVertexRigs(_portrait);//삭제 : 20.3.30 > 별달리 Refresh할 것은 없다.

									//이 렌더 유닛이 선택된 경우에만 RigWeightParam을 계산하자.
									//bool isSelectedRenderUnit = _tmpSelectedRenderUnits.Contains(modMesh._renderUnit);//이전
									bool isSelectedRenderUnit = (_tmpSelected_MainRenderUnit == modMesh._renderUnit);//변경 20.5.28

									
									//for (int iRU = 0; iRU < modMesh._renderUnit._renderVerts.Count; iRU++)//이전

									//변경 22.3.23 [v1.4.0] : RenderVertex가 배열로 변경됨
									int nRenderVerts = modMesh._renderUnit._renderVerts != null ? modMesh._renderUnit._renderVerts.Length : 0;
									if (nRenderVerts > 0)
									{
										for (int iRU = 0; iRU < nRenderVerts; iRU++)
										{
											renderVert = modMesh._renderUnit._renderVerts[iRU];
											renderVert._renderColorByTool = colorBlack;
											renderVert._renderWeightByTool = 0.0f;
											renderVert._renderParam = 0;
											renderVert._renderRigWeightParam.Clear();//<<추가 19.7.30
										}
									}

									for (int iVR = 0; iVR < modMesh._vertRigs.Count; iVR++)
									{
										vertRig = modMesh._vertRigs[iVR];
										if (vertRig._renderVertex != null)
										{
											for (int iWP = 0; iWP < vertRig._weightPairs.Count; iWP++)
											{
												weightPair = vertRig._weightPairs[iWP];
												vertRig._renderVertex._renderColorByTool += weightPair._bone._color * weightPair._weight;

												if (weightPair._bone == selelcedBone)
												{
													vertRig._renderVertex._renderWeightByTool += weightPair._weight;
												}

												//선택된 렌더 유닛인 경우 WeightParam에 Rig값을 입력하자.
												if(isSelectedRenderUnit)
												{
													vertRig._renderVertex._renderRigWeightParam.AddRigWeight(weightPair._bone._color, weightPair._bone == selelcedBone, weightPair._weight);
												}
											}
										}
									}

									//선택된 렌더 유닛에 한해서 RigWeight를 계산하자. (19.7.30)

									if (isSelectedRenderUnit
										&& nRenderVerts > 0//추가 22.3.23
										)
									{
										//for (int iRU = 0; iRU < modMesh._renderUnit._renderVerts.Count; iRU++)//이전
										for (int iRU = 0; iRU < nRenderVerts; iRU++)//변경 22.3.23
										{
											renderVert = modMesh._renderUnit._renderVerts[iRU];
											renderVert._renderRigWeightParam.Normalize();
										}
									}
								}
							}
						}
					}
				}

				//Physic/Volume Color를 집어넣어보자
				
				//변경 22.3.3
				if(renderRequest_Normal.PhysicsWeightColor || renderRequest_Normal.VolumeWeightColor)
				{

					//Rig Weight를 집어넣자.
					apRenderVertex renderVert = null;
					apModifiedMesh modMesh = Select.ModMesh_Main;
					apModifiedVertexWeight vertWeight = null;

					Color colorBlack = Color.black;

					apModifierBase modifier = Select.Modifier;

					bool isPhysic = (int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0;
					
					if (modifier != null)
					{
						if (modifier._paramSetGroup_controller.Count > 0 &&
							modifier._paramSetGroup_controller[0]._paramSetList.Count > 0)
						{
							List<apModifiedMesh> modMeshes = Select.Modifier._paramSetGroup_controller[0]._paramSetList[0]._meshData;
							for (int iMM = 0; iMM < modMeshes.Count; iMM++)
							{
								modMesh = modMeshes[iMM];
								if (modMesh != null)
								{
									//Refresh를 여기서 하진 말자
									//modMesh.RefreshVertexWeights(_portrait, isPhysic, isVolume);

									//이전
									//for (int iRU = 0; iRU < modMesh._renderUnit._renderVerts.Count; iRU++)

									//변경 22.3.23 [v1.4.0] : 배열로 변경
									int nRenderVerts = modMesh._renderUnit._renderVerts != null ? modMesh._renderUnit._renderVerts.Length : 0;
									for (int iRU = 0; iRU < nRenderVerts; iRU++)
									{
										renderVert = modMesh._renderUnit._renderVerts[iRU];
										renderVert._renderColorByTool = colorBlack;
										renderVert._renderWeightByTool = 0.0f;
										renderVert._renderParam = 0;
									}

									int nVertWeights = modMesh._vertWeights != null ? modMesh._vertWeights.Count : 0;

									for (int iVR = 0; iVR < nVertWeights; iVR++)
									{
										vertWeight = modMesh._vertWeights[iVR];

										if (vertWeight._renderVertex == null)
										{
											continue;
										}

										//그라데이션을 위한 Weight 값을 넣어주자
										vertWeight._renderVertex._renderWeightByTool = vertWeight._weight;

										if (isPhysic)
										{
											if (vertWeight._isEnabled && vertWeight._physicParam._isMain)
											{
												vertWeight._renderVertex._renderParam = 1;//1 : Main
											}
											else if (!vertWeight._isEnabled && vertWeight._physicParam._isConstraint)
											{
												vertWeight._renderVertex._renderParam = 2;//2 : Constraint
											}
										}
									}
								}
							}
						}
					}
				}


				//----------------------------------

				if (!isSelectedMeshOnly)
				{
					apRenderUnit renderUnit = null;
					for (int iUnit = 0; iUnit < nRenderUnits; iUnit++)
					{
						renderUnit = renderUnits[iUnit];


						if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
						{
							if (renderUnit._meshTransform != null)
							{
								if (renderUnit._meshTransform._isClipping_Parent)
								{
									//Profiler.BeginSample("Render - Mask Unit");

									if (!isRenderOnlyVisible || renderUnit._isVisible)
									{
										apGL.DrawRenderUnit_ClippingParent_Renew(	renderUnit,
																					
																					//meshRenderType,		//이전
																					renderRequest_Normal,	//변경 22.3.3
																					
																					renderUnit._meshTransform._clipChildMeshes,
																					VertController,
																					this,
																					Select);
									}

									//Profiler.EndSample();
								}
								else if (renderUnit._meshTransform._isClipping_Child)
								{
									//렌더링은 생략한다.
								}
								else
								{
									//Profiler.BeginSample("Render - Normal Unit");

									if (!isRenderOnlyVisible || renderUnit._isVisible)
									{
										apGL.DrawRenderUnit(	renderUnit,

																//meshRenderType,		//이전
																renderRequest_Normal,	//변경 22.3.3

																VertController,
																Select,
																this,
																_mouseSet.Pos);
									}

									//Profiler.EndSample();
								}

							}
						}
					}

					//렌더유닛 렌더링 후 Pass 1차 종료
					apGL.EndPass();
				}
			}


			//Bone을 렌더링하자
			if (boneRenderMode != BONE_RENDER_MODE.None)
			{
				bool isDrawBoneOutline = (boneRenderMode == BONE_RENDER_MODE.RenderOutline);

				//추가 20.3.28 : 리깅 중일때 > 현재 선택된 ModMesh에 등록된 본 외에는 반투명으로 만드는 기능이 있다.
				NOLINKED_BONE_VISIBILITY linkedBonesVisibility = NOLINKED_BONE_VISIBILITY.Opaque;//작업 중이지 않은 본은 반투명으로 표시되는 옵션

				bool isRiggingWorks = (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
										&& Select.Modifier != null
										&& Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging
										&& Select.IsRigEditBinding);

				if (isRiggingWorks)
				{
					if (_rigGUIOption_NoLinkedBoneVisibility != NOLINKED_BONE_VISIBILITY.Opaque)
					{
						//옵션이 켜진 경우
						if (Select.IsCheckableToLinkedToModifierBones())
						{
							//Select에서 특정 본들을 안보이거나 반투명하게 만들 필요가 있다면
							linkedBonesVisibility = _rigGUIOption_NoLinkedBoneVisibility;
						}
					}
				}

				if(_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version2)
				{
					apGL.BeginBatch_DrawBones_V2();//본 렌더링 V2 방식은 Batch가 가능하다. (추가 21.5.19)
				}

				//Child MeshGroup의 Bone을 먼저 렌더링합니다. (그래야 렌더링시 뒤로 들어감)
				if ((int)(boneRenderTarget & BONE_RENDER_TARGET.AllBones) != 0)
				{
					//>> Bone Render Target이 AllBones인 경우
					//변경 : Bone Set를 이용한다.
					if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
					{
						bool isSubBoneSelectable = true;
						if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup && _meshGroupEditMode == MESHGROUP_EDIT_MODE.Bone)
						{
							isSubBoneSelectable = false;
						}

						apMeshGroup.BoneListSet boneSet = null;
						int nBoneListSets = meshGroup._boneListSets != null ? meshGroup._boneListSets.Count : 0;
						for (int iSet = 0; iSet < nBoneListSets; iSet++)
						{
							boneSet = meshGroup._boneListSets[iSet];
							if (boneSet._isRootMeshGroup)
							{
								//Root MeshGroup의 Bone이면 패스
								continue;
							}

							int nRootBones = boneSet._bones_Root != null ? boneSet._bones_Root.Count : 0;
							if(nRootBones == 0)
							{
								continue;
							}

							if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
							{
								//Version 1 방식으로 렌더링
								for (int iRoot = 0; iRoot < nRootBones; iRoot++)
								{
									DrawBoneRecursive_V1(boneSet._bones_Root[iRoot], isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, isSubBoneSelectable);
								}
							}
							else
							{
								//Version 2 방식으로 렌더링
								apGL.BeginBatch_DrawBones_V2();//본 렌더링 V2 방식은 Batch가 가능하다.

								for (int iRoot = 0; iRoot < nRootBones; iRoot++)
								{
									DrawBoneRecursive_V2(boneSet._bones_Root[iRoot], isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, isSubBoneSelectable, isRiggingWorks, linkedBonesVisibility);
								}

								//삭제 21.5.19
								//apGL.EndBatch();
							}
							
						}

						
					}


					//Bone도 렌더링 합니당
					if (meshGroup._boneList_Root.Count > 0)
					{
						int nBoneRoots = meshGroup._boneList_Root.Count;
						if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
						{
							//Version 1 방식으로 렌더링
							for (int iRoot = 0; iRoot < nBoneRoots; iRoot++)
							{
								//Root 렌더링을 한다.
								DrawBoneRecursive_V1(meshGroup._boneList_Root[iRoot], isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, true);
							}

							//렌더링 종료
							apGL.EndPass();
						}
						else
						{
							//Version 2 방식으로 렌더링
							apGL.BeginBatch_DrawBones_V2();//본 렌더링 V2 방식은 Batch가 가능하다.

							for (int iRoot = 0; iRoot < nBoneRoots; iRoot++)
							{
								//Root 렌더링을 한다.
								DrawBoneRecursive_V2(meshGroup._boneList_Root[iRoot], isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, true, isRiggingWorks, linkedBonesVisibility);
							}

						}
						
					}
				}
				else if ((int)(boneRenderTarget & BONE_RENDER_TARGET.SelectedOnly) != 0)
				{
					//변경 20.5.28 : 파라미터로 받은 Main/Sub 본을 렌더링한다.
					//Sub부터 렌더링
					if(selectedBone_Sub != null && selectedBone_Sub.Count > 0)
					{
						apBone curSubBone = null;

						int nSelectedSubBons = selectedBone_Sub.Count;
						for (int iSubBone = 0; iSubBone < nSelectedSubBons; iSubBone++)
						{
							curSubBone = selectedBone_Sub[iSubBone];
							if (curSubBone == selectedBone) { continue; }

							curSubBone.GUIUpdate(false, isBoneIKUsing);
							
							if (curSubBone.IsVisibleInGUI)//변경 21.1.28
							{
								if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
								{
									//Version1로 그리기
									apGL.DrawBone_V1(curSubBone, isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, true);
								}
								else
								{
									//Version2로 그리기
									//apGL.DrawBone_V2(curSubBone, isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, true, true, false);
									apGL.DrawBone_V2(curSubBone, isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, true, false, false);//Batch를 위해 true > false
								}
							}
						}

					}

					//Main 본도 그리자
					if (selectedBone != null)
					{
						selectedBone.GUIUpdate(false, isBoneIKUsing);
						
						
						//if (selectedBone.IsGUIVisible)//이전
						if (selectedBone.IsVisibleInGUI)//변경 21.1.28
						{
							if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
							{
								//Version1로 그리기
								apGL.DrawBone_V1(selectedBone, isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, true);
							}
							else
							{
								//Version2로 그리기
								apGL.DrawBone_V2(selectedBone, isDrawBoneOutline, isBoneIKUsing, isUseBoneToneColor, true, true, false);
							}
						}
					}
				}

				//렌더링 종료
				apGL.EndPass();

				if ((int)(boneRenderTarget & BONE_RENDER_TARGET.SelectedOutline) != 0)
				{
					//변경 20.5.28 : 파라미터로 받은 Main/Sub 본을 렌더링한다.
					//Sub부터 렌더링
					if(selectedBone_Sub != null && selectedBone_Sub.Count > 0)
					{
						apBone curSubBone = null;
						int nSelectedSubBones = selectedBone_Sub.Count;

						for (int iSubBone = 0; iSubBone < nSelectedSubBones; iSubBone++)
						{
							curSubBone = selectedBone_Sub[iSubBone];
							if (curSubBone == selectedBone)
							{ continue; }

							if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
							{
								//Version1로 그리기
								apGL.DrawSelectedBone_V1(curSubBone, apGL.BONE_SELECTED_OUTLINE_COLOR.SubSelected, isBoneIKUsing);
							}
							else
							{
								//Version2로 그리기
								apGL.DrawSelectedBone_V2(curSubBone, apGL.BONE_SELECTED_OUTLINE_COLOR.SubSelected, isBoneIKUsing);
							}
						}
					}
					if (Select.Bone != null)
					{
						//선택 아웃라인을 위에 그릴때
						if (Select.BoneEditMode == apSelection.BONE_EDIT_MODE.Link)
						{
							if (Controller.BoneEditRollOverBone != null)
							{
								if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
								{
									//Version1로 그리기
									apGL.DrawSelectedBone_V1(Controller.BoneEditRollOverBone, apGL.BONE_SELECTED_OUTLINE_COLOR.LinkTarget, isBoneIKUsing);
								}
								else
								{
									//Version2로 그리기
									apGL.DrawSelectedBone_V2(Controller.BoneEditRollOverBone, apGL.BONE_SELECTED_OUTLINE_COLOR.LinkTarget, isBoneIKUsing);
								}
								
							}
						}

						if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
						{
							//Version1로 그리기
							apGL.DrawSelectedBone_V1(Select.Bone, apGL.BONE_SELECTED_OUTLINE_COLOR.MainSelected, isBoneIKUsing);
						}
						else
						{
							//Version2로 그리기
							apGL.DrawSelectedBone_V2(Select.Bone, apGL.BONE_SELECTED_OUTLINE_COLOR.MainSelected, isBoneIKUsing);
						}
						
						//렌더링 종료
						apGL.EndPass();

						if (!isDrawBoneOutline)
						{
							//IK 설정 등과 관련된 값을 추가로 렌더링
							apGL.DrawSelectedBonePost(Select.Bone, isBoneIKUsing);
						}
					}
				}

				//렌더링 종료
				apGL.EndPass();

			}

			if (meshRenderMode == MESH_RENDER_MODE.Render)
			{
				//선택된 Render Unit을 그려준다. (Vertex 등)
				
				//변경 20.5.28 : 메인, 서브 렌더유닛을 각각 렌더링
				//서브부터 (아래에 렌더링되게)
				if(_tmpSelected_SubRenderUnits.Count > 0)
				{
					int nSelectedRenderUnits = _tmpSelected_SubRenderUnits.Count;
					for (int i = 0; i < nSelectedRenderUnits; i++)
					{
						apGL.DrawRenderUnit(	_tmpSelected_SubRenderUnits[i], 
												
												//selectedRenderType,		//이전
												_renderRequest_Selected,	//변경 22.3.3

												VertController, Select, this, _mouseSet.Pos, false);
					}
				}

				if(_tmpSelected_MainRenderUnit != null)
				{
					apGL.DrawRenderUnit(	_tmpSelected_MainRenderUnit,
											//selectedRenderType,		//이전
											_renderRequest_Selected,	//변경 22.3.3

											VertController, Select, this, _mouseSet.Pos);
				}
			}
			//Profiler.EndSample();

			//렌더링 종료
			apGL.EndPass();
		}




		/// <summary>
		/// MeshGroup을 렌더링한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="selectedRenderType"></param>
		/// <param name="selectedMeshes"></param>
		/// <param name="selectedMeshGroups"></param>
		/// <param name="isRenderOnlyVisible">단순 재생이면 True, 작업 중이면 False (Alpha가 0인 것도 일단 렌더링을 한다)</param>
		private void RenderBoneOutlineOnly(apMeshGroup meshGroup,
										Color boneLineColor,
										bool isBoneIKUsing)
		{

			//Bone을 렌더링하자
			//선택 아웃라인을 밑에 그릴때
			//if(Select.Bone != null)
			//{
			//	apGL.DrawSelectedBone(Select.Bone);
			//}
			
			//변경 : Bone Set를 이용한다.
			if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
			{
				apMeshGroup.BoneListSet boneSet = null;
				int nBoneListSets = meshGroup._boneListSets.Count;

				for (int iSet = 0; iSet < nBoneListSets; iSet++)
				{
					boneSet = meshGroup._boneListSets[iSet];
					if (boneSet._isRootMeshGroup)
					{
						//Root MeshGroup의 Bone이면 패스
						continue;
					}

					int nRootBones = boneSet._bones_Root != null ? boneSet._bones_Root.Count : 0;
					if(nRootBones == 0)
					{
						continue;
					}

					if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
					{
						//Version 1 방식으로 렌더링
						for (int iRoot = 0; iRoot < nRootBones; iRoot++)
						{
							DrawBoneOutlineRecursive_V1(boneSet._bones_Root[iRoot], boneLineColor, isBoneIKUsing);
						}
					}
					else
					{
						//Version 2 방식으로 렌더링
						apGL.BeginBatch_DrawBones_V2();//본 렌더링 V2 방식은 Batch가 가능하다.
						for (int iRoot = 0; iRoot < nRootBones; iRoot++)
						{
							DrawBoneOutlineRecursive_V2(boneSet._bones_Root[iRoot], boneLineColor, isBoneIKUsing);
						}
						
						//삭제 21.5.19
						//apGL.EndBatch();
					}
					
				}
			}


			//Bone도 렌더링 합니당
			if (meshGroup._boneList_Root.Count > 0)
			{
				int nRootBones = meshGroup._boneList_Root.Count;
				if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
				{
					//Version 1 방식으로 렌더링
					for (int iRoot = 0; iRoot < nRootBones; iRoot++)
					{
						//Root 렌더링을 한다.
						DrawBoneOutlineRecursive_V1(meshGroup._boneList_Root[iRoot], boneLineColor, isBoneIKUsing);
					}
				}
				else
				{
					//Version 2 방식으로 렌더링
					apGL.BeginBatch_DrawBones_V2();//본 렌더링 V2 방식은 Batch가 가능하다.
					for (int iRoot = 0; iRoot < nRootBones; iRoot++)
					{
						//Root 렌더링을 한다.
						DrawBoneOutlineRecursive_V2(meshGroup._boneList_Root[iRoot], boneLineColor, isBoneIKUsing);
					}
					
					//삭제 21.5.19
					//apGL.EndBatch();
				}
			}
		}

		private void DrawBoneRecursive_V1(apBone targetBone, bool isDrawOutline, bool isBoneIKUsing, bool isUseBoneToneColor, bool isBoneAvailable)
		{
			targetBone.GUIUpdate(false, isBoneIKUsing);
			//apGL.DrawBone(targetBone, _selection);

			//if (targetBone.IsGUIVisible)//이전
			if (targetBone.IsVisibleInGUI)//변경 21.1.28
			{
				apGL.DrawBone_V1(targetBone, isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable);
			}

			int nChildBones = targetBone._childBones != null ? targetBone._childBones.Count : 0;
			if(nChildBones == 0)
			{
				return;
			}
			for (int i = 0; i < nChildBones; i++)
			{
				DrawBoneRecursive_V1(targetBone._childBones[i], isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable);
			}
		}



		private void DrawBoneRecursive_V2(apBone targetBone, bool isDrawOutline, bool isBoneIKUsing, bool isUseBoneToneColor, bool isBoneAvailable, 
										bool isRiggingWorks, NOLINKED_BONE_VISIBILITY noLinkedBoneVisibility)
		{
			targetBone.GUIUpdate(false, isBoneIKUsing);

			if (targetBone.IsVisibleInGUI)//변경 21.1.28
			{
				if (isRiggingWorks)
				{
					//리깅 작업 중일땐
					switch (noLinkedBoneVisibility)
					{
						case NOLINKED_BONE_VISIBILITY.Opaque:
							//연결 여부에 상관없이 온전히 출력
							apGL.DrawBone_V2(targetBone, isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable, false, false);
							break;

						case NOLINKED_BONE_VISIBILITY.Translucent:
							//모디파이어에 연결되지 않은 본은 반투명 출력
							apGL.DrawBone_V2(targetBone, isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable, false, !_selection.LinkedToModifierBones.ContainsKey(targetBone));
							break;

						case NOLINKED_BONE_VISIBILITY.Hidden:
							//모디파이어에 연결되지 않은 본은 출력하지 않음 (선택된 본은 반투명으로 출력한다.)
							if (_selection.LinkedToModifierBones.ContainsKey(targetBone))
							{
								apGL.DrawBone_V2(targetBone, isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable, false, false);
							}
							else if (_selection.Bone == targetBone)
							{
								apGL.DrawBone_V2(targetBone, isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable, false, true);
							}
							break;
					}
				}
				else
				{
					//그 외의 작업 중일땐
					if (_exModObjOption_ShowGray &&
						(targetBone._exCalculateMode == apBone.EX_CALCULATE.Disabled_NotEdit ||
						targetBone._exCalculateMode == apBone.EX_CALCULATE.Disabled_ExRun)
						)
					{
						//반투명으로 출력한다.
						apGL.DrawBone_V2(targetBone, isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable, false, true);
					}
					else
					{
						//그냥 출력한다.
						apGL.DrawBone_V2(targetBone, isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable, false, false);
					}
				}
			}
			
			int nChildBones = targetBone._childBones != null ? targetBone._childBones.Count : 0;
			if(nChildBones == 0)
			{
				return;
			}

			for (int i = 0; i < nChildBones; i++)
			{
				DrawBoneRecursive_V2(targetBone._childBones[i], isDrawOutline, isBoneIKUsing, isUseBoneToneColor, isBoneAvailable, isRiggingWorks, noLinkedBoneVisibility);
			}
		}


		private void DrawBoneOutlineRecursive_V1(apBone targetBone, Color boneOutlineColor, bool isBoneIKUsing)
		{
			targetBone.GUIUpdate(false, isBoneIKUsing);

			//if (targetBone.IsGUIVisible)
			if (targetBone.IsVisibleInGUI)//변경 21.1.28
			{
				apGL.DrawBoneOutline_V1(targetBone, boneOutlineColor, isBoneIKUsing);
			}

			int nChildBones = targetBone._childBones != null ? targetBone._childBones.Count : 0;
			if(nChildBones == 0)
			{
				return;
			}

			for (int i = 0; i < nChildBones; i++)
			{
				DrawBoneOutlineRecursive_V1(targetBone._childBones[i], boneOutlineColor, isBoneIKUsing);
			}
		}


		private void DrawBoneOutlineRecursive_V2(apBone targetBone, Color boneOutlineColor, bool isBoneIKUsing)
		{
			targetBone.GUIUpdate(false, isBoneIKUsing);

			//if (targetBone.IsGUIVisible)
			if (targetBone.IsVisibleInGUI)//변경 21.1.28
			{
				apGL.DrawBoneOutline_V2(targetBone, boneOutlineColor, isBoneIKUsing, false);
			}

			int nChildBones = targetBone._childBones != null ? targetBone._childBones.Count : 0;
			if(nChildBones == 0)
			{
				return;
			}

			for (int i = 0; i < nChildBones; i++)
			{
				DrawBoneOutlineRecursive_V2(targetBone._childBones[i], boneOutlineColor, isBoneIKUsing);
			}
		}



		// 편집모드 미리보기용 본 그리기

		
		/// <summary>
		/// Bone Preview를 위해서 ExMode를 변형해서 Rendering하는 부분
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="boneOutlineColor"></param>
		private void RenderExEditBonePreview_Modifier(apMeshGroup meshGroup, Color boneOutlineColor)
		{
			apSelection.EX_EDIT prevExEditMode = Select.ExEditingMode;

			bool isExLockSuccess = Select.SetModifierExclusiveEditing_Tmp(apSelection.EX_EDIT.None);
			if (!isExLockSuccess)
			{
				return;
			}
			
			//이전
			//Select.RefreshMeshGroupExEditingFlags(meshGroup, null, null, null, true);//<<일단 초기화
			
			//변경 21.2.15 : 초기화시에는 다른 함수를 사용하자
			Select.SetEnableMeshGroupExEditingFlagsForce();

			meshGroup.SetBoneIKEnabled(true, false);

			meshGroup.RefreshForce();
			RenderBoneOutlineOnly(meshGroup, boneOutlineColor, true);

			//meshGroup.RefreshForce(false, 0.0f, false);//<ExCalculate를 Ignore한다.

			meshGroup.SetBoneIKEnabled(false, false);

			//Debug.Log("Render ExEdit " + prevExEditMode + " > None > " + prevExEditMode);
			Select.SetModifierExclusiveEditing_Tmp(prevExEditMode);
			meshGroup.RefreshForce();

			meshGroup.BoneGUIUpdate(false);
		}


		private void RenderExEditBonePreview_Animation(apAnimClip animClip, apMeshGroup meshGroup, Color boneOutlineColor)
		{
			if (animClip == null || animClip._targetMeshGroup == null)
			{
				return;
			}

			apSelection.EX_EDIT prevExEditMode = Select.ExAnimEditingMode;

			bool isExLockSuccess = Select.SetAnimExclusiveEditing_Tmp(apSelection.EX_EDIT.None, false);
			if (!isExLockSuccess)
			{
				//Debug.Log("Failed");
				return;
			}

			//apModifierBase curModifier = null;
			//if (Select.AnimTimeline != null)
			//{
			//	curModifier = Select.AnimTimeline._linkedModifier;
			//}
			//Debug.Log("Is Modifier Valid : " + (curModifier != null));

			//이전
			//Select.RefreshMeshGroupExEditingFlags(meshGroup, null, null, animClip, true);//<<일단 초기화

			//변경 21.2.15 : 초기화시에는 다른 함수를 사용하자
			Select.SetEnableMeshGroupExEditingFlagsForce();

			////meshGroup.RefreshForce();//<ExCalculate를 Ignore한다.
			animClip.Update_Editor(0.0f, true, true, false, IsUseCPPDLL);
			RenderBoneOutlineOnly(meshGroup, boneOutlineColor, true);

			Select.SetAnimExclusiveEditing_Tmp(prevExEditMode, false);

			//Debug.Log("TODO : 이 부분 코드 삭제했는데 문제 확인 필요");
			//Select.RefreshMeshGroupExEditingFlags(meshGroup, curModifier, null, animClip, true);//이거 호출할 필요가 있나?
			

			animClip.Update_Editor(0.0f, true, false, false, IsUseCPPDLL);

			//<BONE_EDIT>
			//for (int i = 0; i < animClip._targetMeshGroup._boneList_Root.Count; i++)
			//{
			//	animClip._targetMeshGroup._boneList_Root[i].GUIUpdate(true);
			//}

			//>>Bone Set으로 변경
			apMeshGroup.BoneListSet boneSet = null;
			int nBoneListSets = animClip._targetMeshGroup._boneListSets != null ? animClip._targetMeshGroup._boneListSets.Count : 0;
			if(nBoneListSets == 0)
			{
				return;
			}
			for (int iSet = 0; iSet < nBoneListSets; iSet++)
			{
				boneSet = animClip._targetMeshGroup._boneListSets[iSet];

				int nRootBones = boneSet._bones_Root != null ? boneSet._bones_Root.Count : 0;
				if(nRootBones == 0)
				{
					continue;
				}
				for (int iRoot = 0; iRoot < nRootBones; iRoot++)
				{
					boneSet._bones_Root[iRoot].GUIUpdate(true);
				}
			}


		}
				





		// Onion 그리기 (기본 / 애니메이션)
		

		/// <summary>
		/// Onion을 렌더링한다.
		/// 조건문이 모두 포함되있어서 함수만 호출하면 처리 조건에 따라 자동으로 렌더링을 한다.
		/// 편집 중일 때에만 렌더링을 한다.
		/// Top/Behind 옵션이 있으므로 RenderMeshGroup의 전, 후에 모두 호출해야한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="animClip"></param>
		/// <param name="isBoneIKMatrix"></param>
		/// <param name="isBoneIKRigging"></param>
		/// <param name="isBoneIKUsing"></param>
		private void RenderOnion(apMeshGroup meshGroup,
									apAnimClip animClip,
									bool isBehindRendering, bool isRepaintType,
									bool isBoneIKMatrix, bool isBoneIKRigging, bool isBoneIKUsing,
									bool isRecoverUpdate,
									apTransform_Mesh selectedMeshTF_Main,
									List<apTransform_Mesh> selectedMeshTF_Sub,
									apBone selectedBone,
									List<apBone> selectedBone_Sub,
									bool isUseOnionRecord = true)
		{
			if (meshGroup == null 
				|| !isRepaintType 
				|| !Onion.IsVisible 
				|| (isUseOnionRecord && !Onion.IsRecorded) 
				|| _onionOption_IsRenderBehind != isBehindRendering)
			{
				return;
			}

			if (_onionOption_IKCalculateForce)
			{
				isBoneIKUsing = true;
			}

			if (animClip != null)
			{
				if (animClip.IsPlaying_Editor)
				{
					//재생중엔 실행되지 않는다.
					return;
				}
			}

			if (isUseOnionRecord)
			{
				//저장된 값을 적용
				Onion.AdaptRecord(this);
			}

			//렌더링
			bool isPrevPhysics = _portrait._isPhysicsPlay_Editor;
			_portrait._isPhysicsPlay_Editor = false;


			if (animClip != null)
			{
				//animClip.Update_Editor(0.0f, true, isBoneIKMatrix, isBoneIKRigging);
				animClip.Update_Editor(0.0f, true, true, true, IsUseCPPDLL);
			}
			else
			{
				//meshGroup.SetBoneIKEnabled(isBoneIKMatrix, isBoneIKRigging);
				meshGroup.SetBoneIKEnabled(true, true);
				meshGroup.UpdateRenderUnits(0.0f, true);
				meshGroup.SetBoneIKEnabled(false, false);
			}
			//Debug.Log("Render Onion : isBoneIKMatrix : " + isBoneIKMatrix + " / isBoneIKRigging : " + isBoneIKRigging + " / isBoneIKUsing : " + isBoneIKUsing);

			if (_onionOption_IsRenderOnlySelected)
			{
				//선택된 것만 렌더링

				RenderMeshGroup(	meshGroup,
									
									//apGL.RENDER_TYPE.ToneColor, apGL.RENDER_TYPE.ToneColor,							//이전
									apGL.RenderTypeRequest.Preset_ToneColor, apGL.RenderTypeRequest.Preset_ToneColor,	//변경 22.3.3

									selectedMeshTF_Main, selectedMeshTF_Sub,
									selectedBone, selectedBone_Sub,
									true,
									(_boneGUIRenderMode != BONE_RENDER_MODE.None ? BONE_RENDER_MODE.RenderOutline : BONE_RENDER_MODE.None),
									_meshGUIRenderMode,
									isBoneIKUsing, BONE_RENDER_TARGET.SelectedOnly, true, true);

			}
			else
			{
				//모두 렌더링
				RenderMeshGroup(	meshGroup,
									
									//apGL.RENDER_TYPE.ToneColor, apGL.RENDER_TYPE.Default,							//이전
									apGL.RenderTypeRequest.Preset_ToneColor, apGL.RenderTypeRequest.Preset_Default,	//변경 22.3.3

									null, null, null, null, true,
									(_boneGUIRenderMode != BONE_RENDER_MODE.None ? BONE_RENDER_MODE.RenderOutline : BONE_RENDER_MODE.None),
									_meshGUIRenderMode,
									isBoneIKUsing,
									BONE_RENDER_TARGET.AllBones, false, true);
			}





			if (isUseOnionRecord)
			{
				//원래의 값으로 복구 
				Onion.Recorver(this);
			}

			if (isRecoverUpdate)
			{
				if (animClip != null)
				{
					animClip.Update_Editor(0.0f, true, isBoneIKMatrix, isBoneIKRigging, IsUseCPPDLL);
				}
				else
				{
					meshGroup.SetBoneIKEnabled(isBoneIKMatrix, isBoneIKRigging);
					meshGroup.UpdateRenderUnits(0.0f, true);
					meshGroup.SetBoneIKEnabled(false, false);
				}

				//<BONE_EDIT> : 이전 코드
				//for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
				//{
				//	meshGroup._boneList_Root[i].GUIUpdate(true, isBoneIKUsing);
				//}

				//if (meshGroup._childMeshGroupTransformsWithBones != null)
				//{
				//	for (int iChild = 0; iChild < meshGroup._childMeshGroupTransformsWithBones.Count; iChild++)
				//	{
				//		apMeshGroup childMeshGroup = meshGroup._childMeshGroupTransformsWithBones[iChild]._meshGroup;
				//		if (childMeshGroup != null)
				//		{
				//			for (int i = 0; i < childMeshGroup._boneList_Root.Count; i++)
				//			{
				//				childMeshGroup._boneList_Root[i].GUIUpdate(true, isBoneIKUsing);
				//			}
				//		}

				//	}
				//}

				//Bone Set으로 통합
				if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
				{
					apMeshGroup.BoneListSet boneSet = null;
					for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
					{
						boneSet = meshGroup._boneListSets[iSet];

						for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
						{
							boneSet._bones_Root[iRoot].GUIUpdate(true, isBoneIKUsing);
						}
					}
				}
			}

			_portrait._isPhysicsPlay_Editor = isPrevPhysics;
		}


		private void RenderAnimatedOnion(apAnimClip animClip, bool isBehindRendering, bool isRepaintType,
										bool isBoneIKMatrix, bool isBoneIKRigging, bool isBoneIKUsing,
										apTransform_Mesh selectedMeshTF_Main,
										List<apTransform_Mesh> selectedMeshTF_Sub,
										apBone selectedBone,
										List<apBone> selectedBone_Sub
										)
		{
			if (animClip == null || !isRepaintType || !Onion.IsVisible || _onionOption_IsRenderBehind != isBehindRendering)
			{
				return;
			}
			if (animClip._targetMeshGroup == null || animClip.IsPlaying_Editor)
			{
				return;
			}

			if (_onionOption_IKCalculateForce)
			{
				isBoneIKUsing = true;
			}
			int curFrame = animClip.CurFrame;



			//그려야할 범위를 계산한다.
			//min~max Frame을 CurFrame과 RenderPerFrame 값을 이용해서 계산한다.
			//Loop이면 프레임이 반복된다. 루프가 아니면 종료

			int animLength = (animClip.EndFrame - animClip.StartFrame) + 1;
			if (animLength < 1)
			{
				return;
			}


			bool isLoop = animClip.IsLoop;
			int prevRange = Mathf.Clamp(_onionOption_PrevRange, 0, animLength / 2);
			int nextRange = Mathf.Clamp(_onionOption_NextRange, 0, animLength / 2);
			int renderPerFrame = Mathf.Max(_onionOption_RenderPerFrame, 0);

			if (renderPerFrame == 0 || (prevRange == 0 && nextRange == 0))
			{
				return;
			}

			prevRange = (prevRange / renderPerFrame) * renderPerFrame;
			nextRange = (nextRange / renderPerFrame) * renderPerFrame;

			int minFrame = curFrame - prevRange;
			int maxFrame = curFrame + nextRange;


			bool isPrevPhysics = _portrait._isPhysicsPlay_Editor;
			_portrait._isPhysicsPlay_Editor = false;


			int renderFrame = 0;
			if (prevRange > 0)
			{
				//Min -> Cur 렌더링
				for (int iFrame = minFrame; iFrame < curFrame; iFrame += renderPerFrame)
				{
					renderFrame = iFrame;
					if (renderFrame < animClip.StartFrame)
					{
						if (isLoop)
						{ renderFrame = (renderFrame + animLength) - 1; }
						else
						{ continue; }
					}
					else if (renderFrame > animClip.EndFrame)
					{
						if (isLoop)
						{ renderFrame = (renderFrame - animLength) + 1; }
						else
						{ continue; }
					}

					animClip.SetFrame_EditorNotStop(renderFrame);

					//렌더 설정 변경
					apGL.SetToneOption(_colorOption_OnionAnimPrevColor,
										_onionOption_OutlineThickness,
										_onionOption_IsOutlineRender,
										_onionOption_PosOffsetX * Mathf.Abs(iFrame - curFrame),
										_onionOption_PosOffsetY * Mathf.Abs(iFrame - curFrame),
										_colorOption_OnionBonePrevColor);

					RenderOnion(animClip._targetMeshGroup, animClip,
						isBehindRendering, isRepaintType, isBoneIKMatrix, isBoneIKRigging, isBoneIKUsing,
						false,//<<기본 RenderOnion과 다른 파라미터이다.
						selectedMeshTF_Main, selectedMeshTF_Sub,
						selectedBone, selectedBone_Sub,
						false);
				}
			}

			if (nextRange > 0)
			{
				//Cur <- Max 렌더링. 프레임이 거꾸로 진행한다.
				for (int iFrame = maxFrame; iFrame > curFrame; iFrame -= renderPerFrame)
				{
					renderFrame = iFrame;
					if (renderFrame < animClip.StartFrame)
					{
						if (isLoop)
						{ renderFrame = (renderFrame + animLength) - 1; }
						else
						{ continue; }
					}
					else if (renderFrame > animClip.EndFrame)
					{
						if (isLoop)
						{ renderFrame = (renderFrame - animLength) + 1; }
						else
						{ continue; }
					}

					animClip.SetFrame_EditorNotStop(renderFrame);

					//렌더 설정 변경
					apGL.SetToneOption(_colorOption_OnionAnimNextColor,
										_onionOption_OutlineThickness,
										_onionOption_IsOutlineRender,
										-_onionOption_PosOffsetX * Mathf.Abs(iFrame - curFrame),
										-_onionOption_PosOffsetY * Mathf.Abs(iFrame - curFrame),
										_colorOption_OnionBoneNextColor);

					RenderOnion(animClip._targetMeshGroup, animClip,
						isBehindRendering, isRepaintType, isBoneIKMatrix, isBoneIKRigging, isBoneIKUsing,
						false,//<<기본 RenderOnion과 다른 파라미터이다.
						selectedMeshTF_Main, selectedMeshTF_Sub,
						selectedBone, selectedBone_Sub,
						false);
				}
			}

			//원래의 값으로 복구 
			animClip.SetFrame_EditorNotStop(curFrame);

			apGL.SetToneOption(_colorOption_OnionToneColor,
									_onionOption_OutlineThickness,
									_onionOption_IsOutlineRender,
									_onionOption_PosOffsetX,
									_onionOption_PosOffsetY,
									_colorOption_OnionBoneColor);

			if (animClip != null)
			{
				animClip.Update_Editor(0.0f, true, isBoneIKMatrix, isBoneIKRigging, IsUseCPPDLL);
			}

			//<BONE_EDIT>
			//for (int i = 0; i < animClip._targetMeshGroup._boneList_Root.Count; i++)
			//{
			//	animClip._targetMeshGroup._boneList_Root[i].GUIUpdate(true, isBoneIKUsing);
			//}
			//if (animClip._targetMeshGroup._childMeshGroupTransformsWithBones != null)
			//{
			//	for (int iChild = 0; iChild < animClip._targetMeshGroup._childMeshGroupTransformsWithBones.Count; iChild++)
			//	{
			//		apMeshGroup childMeshGroup = animClip._targetMeshGroup._childMeshGroupTransformsWithBones[iChild]._meshGroup;
			//		if (childMeshGroup != null)
			//		{
			//			for (int i = 0; i < childMeshGroup._boneList_Root.Count; i++)
			//			{
			//				childMeshGroup._boneList_Root[i].GUIUpdate(true, isBoneIKUsing);
			//			}
			//		}

			//	}
			//}


			//Bone Set으로 통합
			if (animClip._targetMeshGroup._boneListSets != null && animClip._targetMeshGroup._boneListSets.Count > 0)
			{
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < animClip._targetMeshGroup._boneListSets.Count; iSet++)
				{
					boneSet = animClip._targetMeshGroup._boneListSets[iSet];

					for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
					{
						boneSet._bones_Root[iRoot].GUIUpdate(true, isBoneIKUsing);
					}
				}
			}

			_portrait._isPhysicsPlay_Editor = isPrevPhysics;

		}


		//-------------------------------------------------------------------------
		// 작업 공간에 다른 요소 그리기 (로토스코핑/가이드라인/모디파이어 리스트)
		//-------------------------------------------------------------------------

		// 로토스코핑 그리기	

		/// <summary>
		/// 추가 21.2.28 : 로토스코핑을 그리자
		/// </summary>
		/// <param name="screenWidth"></param>
		/// <param name="screenHeight"></param>
		private void DrawRotoscoping()
		{
			if(!_isEnableRotoscoping || _selectedRotoscopingData == null)
			{
				return;
			}

			//애니메이션과 동기화되었는지 체크한다. (옵션 적용시 필요)
			if(Select.SelectionType == apSelection.SELECTION_TYPE.Animation
				&& Select.AnimClip != null
				&& _selectedRotoscopingData._isSyncToAnimation)
			{
				if(!_isSyncRotoscopingToAnimClipFrame || Select.AnimClip.CurFrame != _iSyncRotoscopingAnimClipFrame)
				{
					//동기화가 필요하다면
					_isSyncRotoscopingToAnimClipFrame = true;
					_iSyncRotoscopingAnimClipFrame = Select.AnimClip.CurFrame;

					//Index를 동기화하자
					//iRoto = (iFrame - Offset) / FramePerImage
					int biasedFrame = _iSyncRotoscopingAnimClipFrame - _selectedRotoscopingData._frameOffsetToSwitch;
					if(biasedFrame >= 0)
					{
						_iRotoscopingImageFile = biasedFrame / _selectedRotoscopingData._framePerSwitch;
					}
					else
					{
						_iRotoscopingImageFile = ((Mathf.Abs(biasedFrame) / _selectedRotoscopingData._framePerSwitch) + 1) * -1;
					}

					
					int nFiles = _selectedRotoscopingData._filePathList != null ? _selectedRotoscopingData._filePathList.Count : 0;
					if(nFiles > 0)
					{
						while(_iRotoscopingImageFile < 0)
						{
							_iRotoscopingImageFile += nFiles;
						}

						_iRotoscopingImageFile %= nFiles;
					}
					
				}
			}
			else
			{
				_isSyncRotoscopingToAnimClipFrame = false;
				_iSyncRotoscopingAnimClipFrame = -1;
			}

			Texture2D rotoImage = _selectedRotoscopingData.GetImage(_iRotoscopingImageFile);
			if(rotoImage == null)
			{
				//이미지가 없다.
				return;
			}

			//이미지 위치를 정하자
			Vector2 posGL = apGL.WindowSizeHalf + new Vector2(Rotoscoping._posOffset_X, -Rotoscoping._posOffset_Y);
			//Height가 Screen의 Ratio에 맞게 만들자
			float scaleRatio = (((float)Rotoscoping._scaleWithinScreen * 0.01f) * apGL.WindowSize.y) / rotoImage.height;
			scaleRatio /= apGL.Zoom;

			int scaledImageWidth = (int)(rotoImage.width * scaleRatio);
			int scaledImageHeight = (int)(rotoImage.height * scaleRatio);
			Color rotoColor = new Color(0.5f, 0.5f, 0.5f, (float)Rotoscoping._opacity / 255.0f);

			apGL.DrawTextureGL(rotoImage, posGL, scaledImageWidth, scaledImageHeight, rotoColor, 0.0f);
			apGL.EndPass();
		}


		
		//모디파이어 리스트 그리기
		
		private void DrawModifierListUI(int posX, int posY,
										apMeshGroup curMeshGroup, apModifierBase curModifier,
										apAnimClip curAnimClip, apAnimTimeline curAnimTimeline,
										apSelection.EX_EDIT exMode)
		{
			if (curMeshGroup == null)
			{
				return;
			}

			if (curMeshGroup._modifierStack._modifiers.Count == 0)
			{
				return;
			}

			apModifierBase mod = null;
			int imgSize = 16;
			int imgSize_Half = imgSize / 2;
			int textWidth = 130;
			int textHeight = 16;
			int startPosY = posY + (textHeight * curMeshGroup._modifierStack._modifiers.Count) / 2;

			bool isCheckAnim = (curAnimClip != null);

			Texture2D imgCursor = null;
			if (exMode != apSelection.EX_EDIT.ExOnly_Edit)
			{
				imgCursor = ImageSet.Get(apImageSet.PRESET.SmallMod_CursorUnlocked);
			}
			else
			{
				imgCursor = ImageSet.Get(apImageSet.PRESET.SmallMod_CursorLocked);
			}


			int curPosY = 0;
			int posX_Cursor = posX + imgSize_Half;
			int posX_Icon = posX_Cursor + imgSize;
			int posX_Title = posX_Icon + imgSize;
			int posX_ExEnabled = posX_Title + 5 + textWidth;
			int posX_ColorEnabled = posX_ExEnabled + 5 + imgSize;
			Color colorGray = Color.gray;

			Color color_Selected = Color.yellow;
			Color color_NotSelected = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			bool isSelected = false;

			Texture2D imgMod = null;
			Texture2D imgEx = null;
			Texture2D imgColor = null;

			float imgSize_Zoom = (float)imgSize / apGL.Zoom;
			for (int i = 0; i < curMeshGroup._modifierStack._modifiers.Count; i++)
			{
				mod = curMeshGroup._modifierStack._modifiers[i];
				curPosY = startPosY - (i * textHeight);

				isSelected = false;
				if (isCheckAnim)
				{
					if (curAnimTimeline != null && mod == curAnimTimeline._linkedModifier)
					{
						isSelected = true;
					}
				}
				else
				{
					if (curModifier == mod)
					{
						isSelected = true;

					}
				}
				if (isSelected)
				{
					apGL.DrawTextureGL(imgCursor, new Vector2(posX_Cursor, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				}
				imgMod = ImageSet.Get(apEditorUtil.GetSmallModIconType(mod.ModifierType));
				apGL.DrawTextureGL(imgMod, new Vector2(posX_Icon, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				if (isSelected)
				{
					apGL.DrawTextGL(mod.DisplayNameShort, new Vector2(posX_Title, curPosY - imgSize_Half), textWidth, color_Selected);
				}
				else
				{
					apGL.DrawTextGL(mod.DisplayNameShort, new Vector2(posX_Title, curPosY - imgSize_Half), textWidth, color_NotSelected);
				}

				switch (mod._editorExclusiveActiveMod)
				{
					//이전
					//case apModifierBase.MOD_EDITOR_ACTIVE.Disabled:
					//case apModifierBase.MOD_EDITOR_ACTIVE.OnlyColorEnabled:
					//변경 21.2.15
					case apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit:
					case apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force:
					case apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor:
						imgEx = ImageSet.Get(apImageSet.PRESET.SmallMod_ExDisabled);
						break;

					//case apModifierBase.MOD_EDITOR_ACTIVE.ExclusiveEnabled:
					//case apModifierBase.MOD_EDITOR_ACTIVE.Enabled:

					//변경 21.2.15
					case apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Run:
					case apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Edit:
						imgEx = ImageSet.Get(apImageSet.PRESET.SmallMod_ExEnabled);
						break;

					//이전
					//case apModifierBase.MOD_EDITOR_ACTIVE.SubExEnabled:
					//변경 21.2.15
					case apModifierBase.MOD_EDITOR_ACTIVE.Enabled_Background:
						//TODO : SubEdit는 여기서 구분할 수 없다. 
						imgEx = ImageSet.Get(apImageSet.PRESET.SmallMod_ExSubEnabled);
						break;

					default:
						imgEx = null;
						break;
				}

				if (imgEx != null)
				{
					apGL.DrawTextureGL(imgEx, new Vector2(posX_ExEnabled, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				}

				if (mod._isColorPropertyEnabled &&
					(int)(mod.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{
					//if (mod._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled)//이전
					//변경 21.2.15
					if (mod._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force
						|| mod._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit)
					{
						imgColor = ImageSet.Get(apImageSet.PRESET.SmallMod_ColorDisabled);
					}
					else
					{
						imgColor = ImageSet.Get(apImageSet.PRESET.SmallMod_ColorEnabled);
					}
					apGL.DrawTextureGL(imgColor, new Vector2(posX_ColorEnabled, curPosY), imgSize_Zoom, imgSize_Zoom, colorGray, 0);
				}



				curPosY -= textHeight;
			}
		}




		// 가이드라인 그리기
		

		/// <summary>
		/// 추가 21.6.4 : 가이드라인을 렌더링하자
		/// </summary>
		private void DrawGuidelines()
		{	
			if(_portrait == null
				|| _portrait.GuideLines == null)
			{
				return;
			}
			if(_portrait.GuideLines.NumLines == 0)
			{
				return;
			}

			int nLines = _portrait.GuideLines.NumLines;
			List<apGuideLines.LineInfo> lines = _portrait.GuideLines.Lines;
			apGuideLines.LineInfo curLine = null;

			//GL 기준으로 가장자리 위치를 체크하자
			float GLPos_Left = -50;
			float GLPos_Right = apGL._windowWidth + 50;
			float GLPos_Top = -50;
			float GLPos_Bottom = apGL._windowHeight + 50;

			Vector2 pos1 = Vector2.zero;
			Vector2 pos2 = Vector2.zero;

			//두꺼운 선부터 보여주자
			apGL.BeginBatch_ColoredPolygon();
			for (int i = 0; i < nLines; i++)
			{
				curLine = lines[i];

				if(!curLine._isEnabled
					|| curLine._thickness == apGuideLines.LINE_THICKNESS.Thin)
				{
					continue;
				}
				if(curLine._direction == apGuideLines.LINE_DIRECTION.Horizontal)
				{
					//수평선
					pos1.x = GLPos_Left;
					pos2.x = GLPos_Right;
					pos1.y = apGL.World2GL(new Vector2(0.0f, curLine._position)).y;
					pos2.y = pos1.y;
				}
				else
				{
					//수직선
					pos1.y = GLPos_Top;
					pos2.y = GLPos_Bottom;
					pos1.x = apGL.World2GL(new Vector2(curLine._position, 0.0f)).x;
					pos2.x = pos1.x;
				}
				apGL.DrawBoldLineGL(pos1, pos2, 3.0f, curLine._color, false);
			}
			apGL.EndPass();

			//가는 선을 보여주자
			apGL.BeginBatch_ColoredLine();
			for (int i = 0; i < nLines; i++)
			{
				curLine = lines[i];

				if(!curLine._isEnabled
					|| curLine._thickness == apGuideLines.LINE_THICKNESS.Thick)
				{
					continue;
				}
				if(curLine._direction == apGuideLines.LINE_DIRECTION.Horizontal)
				{
					//수평선
					pos1.x = GLPos_Left;
					pos2.x = GLPos_Right;
					pos1.y = apGL.World2GL(new Vector2(0.0f, curLine._position)).y;
					pos2.y = pos1.y;
				}
				else
				{
					//수직선
					pos1.y = GLPos_Top;
					pos2.y = GLPos_Bottom;
					pos1.x = apGL.World2GL(new Vector2(curLine._position, 0.0f)).x;
					pos2.x = pos1.x;
				}
				apGL.DrawLineGL(pos1, pos2, curLine._color, false);
			}
			apGL.EndPass();

		}




		//------------------------------------------------------------------------
		// 보기 프리셋과 객체의 동기화
		//------------------------------------------------------------------------
		
		/// <summary>
		/// 추가 21.1.29 : Visibility Preset 기능을 확인하고 동기화하는 함수.
		/// 플레이 중에도 바뀔 수 있으므로, 렌더링 함수에서 체크하되, 이전 상황과 동일하면 불필요한 처리는 하지 않도록 하자
		/// 메뉴 바꿀때나, 오브젝트가 추가된 경우(인자 true)도 이 함수를 호출하면 렌더링시 불필요한 프레임 드랍을 막을 수 있다.
		/// </summary>
		private void CheckAndSyncVisiblityPreset(bool isCheckLink, bool isForceSync)
		{
			if(_portrait == null)
			{
				return;
			}

			if(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup 
				&& Select.SelectionType != apSelection.SELECTION_TYPE.Animation)
			{
				return;
			}

			//[ 링크 ]
			// isCheckLink가 true일때 링크를 시도한다. (무조건)
			// - VisiblityPreset과 RenderUnit/Bone을 연결한다.

			//[ 동기화 조건 체크 ]
			//체크할 것 (하나라도 바뀌면 동기화 시도)
			// (공통)
			// - Preset 사용 여부
			// - 현재 Rule
			// - 메인 메시 그룹

			// (모디파이어에 의한 규칙)
			// - 현재 모디파이어 / ParamKeySet

			// (자식 메시 그룹의 가시성과 본 가시성 연계)
			// - 본을 가지고 있는 서브 메시 그룹들의 가시성 정보 모두
			
			
			//[ 동기화 ]
			// 동기화 
			apMeshGroup meshGroup = null;
			apModifierBase modifier = null;
			apModifierParamSetGroup modParamSetGroup = null;
			apAnimTimeline animTimeline = null;

			if(Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				//메시 그룹 메뉴에서
				meshGroup = Select.MeshGroup;
				modifier = Select.Modifier;
				if(modifier != null)
				{
					modParamSetGroup = Select.SubEditedParamSetGroup;
				}
			}
			else
			{
				//애니메이션 메뉴에서
				if(Select.AnimClip != null)
				{
					meshGroup = Select.AnimClip._targetMeshGroup;
				}
				if(meshGroup != null)
				{
					animTimeline = Select.AnimTimeline;
					if(animTimeline != null && animTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						modifier = animTimeline._linkedModifier;
					}
				}
			}

			if(meshGroup == null)
			{
				return;
			}

			bool isNeedToSync = _portrait.VisiblePreset.CheckSync(	_isAdaptVisibilityPreset,
																	meshGroup,
																	modifier,
																	modParamSetGroup,
																	animTimeline,
																	_selectedVisibilityPresetRule);
			if(!isNeedToSync && !isForceSync)
			{
				//더 동기화를 할 필요가 없으니 종료
				return;
			}

			//동기화를 하자
			//프리셋 규칙을 사용할지 여부부터
			//사용하지 않으면 모두 초기화를 해야한다.
			
			if(_isAdaptVisibilityPreset 
				&& _selectedVisibilityPresetRule != null)
			{
				//선택된 규칙이 있고 보기 프리셋이 켜진 상태이다.
				//Rule을 이용해서 동기화를 하자
				_portrait.VisiblePreset.Sync(Select.SelectionType == apSelection.SELECTION_TYPE.Animation);
			}
			else
			{
				//Debug.LogWarning("동기화 해제");
				//리셋을 하자
				int nRenderUnits = meshGroup._renderUnits_All.Count;
				for (int i = 0; i < nRenderUnits; i++)
				{
					meshGroup._renderUnits_All[i]._workVisible_Rule = apRenderUnit.WORK_VISIBLE_TYPE.None;
				}

				//본 초기화
				if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
				{
					List<apBone> boneRootList = null;
					for (int iBontSet = 0; iBontSet < meshGroup._boneListSets.Count; iBontSet++)
					{	
						boneRootList = meshGroup._boneListSets[iBontSet]._bones_All;
						if (boneRootList != null && boneRootList.Count > 0)
						{
							for (int iBone = 0; iBone < boneRootList.Count; iBone++)
							{
								boneRootList[iBone].SetGUIVisible_Rule(apBone.GUI_VISIBLE_TYPE.None);
							}
						}
					}
				}
			}

			//동기화시 Hierarchy의 보기 여부를 갱신해야한다.
			_hierarchy_MeshGroup.RefreshUnits();
			_hierarchy_AnimClip.RefreshUnits();
		}


		//-------------------------------------------------------------------------
		// Fold 해제
		//-------------------------------------------------------------------------
		// 추가 19.8.18 : FullScreen, Fold에 대한 개선
		public void UnfoldAllTab()
		{
			_isFullScreenGUI = false;

			_uiFoldType_Left = UI_FOLD_TYPE.Unfolded;
			_uiFoldType_Right1 = UI_FOLD_TYPE.Unfolded;
			_uiFoldType_Right2 = UI_FOLD_TYPE.Unfolded;

			_uiFoldType_Right1_Upper = UI_FOLD_TYPE.Unfolded;
			_uiFoldType_Right1_Lower = UI_FOLD_TYPE.Unfolded;
		}




		//-------------------------------------------------------------------------
		// 프레임 카운트 (FPS)
		//-------------------------------------------------------------------------
		
		// 프레임 카운트
		/// <summary>
		/// 프레임 시간을 계산한다.
		/// Update의 경우 60FPS 기준의 동기화된 시간을 사용한다.
		/// "60FPS으로 동기가 된 상태"인 경우에 true로 리턴을 한다.
		/// </summary>
		/// <param name="frameTimerType"></param>
		/// <returns></returns>
		private bool UpdateFrameCount(FRAME_TIMER_TYPE frameTimerType)
		{
			switch (frameTimerType)
			{
				case FRAME_TIMER_TYPE.None:
					return false;

				case FRAME_TIMER_TYPE.Repaint:
					apTimer.I.CheckTime_Repaint(_lowCPUStatus);

					//개선된 방식 19.11.23
					if(_fpsCounter == null)
					{
						_fpsCounter = new apFPSCounter();
					}
					
					//변경 21.7.17 : 일반 FPS 대신 Repaint 시간을 받자 (기존도 FPS는 Repaint 타입으로 계산되었다.
					_fpsCounter.SetData(apTimer.I.DeltaTime_Repaint);

					return false;

				case FRAME_TIMER_TYPE.Update:
					return apTimer.I.CheckTime_Update(_lowCPUStatus);
			}
			return false;
			
		}

		//-------------------------------------------------------------------------
		// Low CPU를 체크하고 갱신하기
		//-------------------------------------------------------------------------
		//추가 3.1 : CPU가 느리게 재생될 수도 있다.
		private void CheckLowCPUOption()
		{
			if(!_isLowCPUOption)
			{
				_lowCPUStatus = LOW_CPU_STATUS.None;
				return;
			}

			_lowCPUStatus = LOW_CPU_STATUS.Full;
			if(_selection == null)
			{
				return;
			}

			if(_portrait == null)
			{
				_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
				return;
			}

			//메뉴마다 다르다.
			switch (_selection.SelectionType)
			{
				case apSelection.SELECTION_TYPE.Overall:
					{
						//다음의 경우가 아니라면 LowCPU 모드가 작동한다.
						//- 애니메이션이 재생 중
						//- 화면이 캡쳐되는 중
						if(_selection.RootUnitAnimClip != null && _selection.RootUnitAnimClip.IsPlaying_Editor)
						{
							_lowCPUStatus = LOW_CPU_STATUS.Full;
						}
						else if(_isScreenCaptureRequest)
						{
							_lowCPUStatus = LOW_CPU_STATUS.Full;
						}
						else
						{
							
							_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
						}
					}
					
					break;
				case apSelection.SELECTION_TYPE.ImageRes:
					//이미지 메뉴에서는 항상 LowCPU
					_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
					break;

				case apSelection.SELECTION_TYPE.Mesh:
					{
						//탭마다 LowCPU의 정도가 다르다.
						switch (_meshEditMode)
						{
							case MESH_EDIT_MODE.Setting:
								if (_isMeshEdit_AreaEditing)
								{
									//Area 편집 시에는 CPU가 올라가야 한다.
									_lowCPUStatus = LOW_CPU_STATUS.Full;
								}
								else
								{
									_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
								}
								break;

							case MESH_EDIT_MODE.MakeMesh:
								if (_isMeshEdit_AreaEditing)
								{
									_lowCPUStatus = LOW_CPU_STATUS.Full;
								}
								else
								{
									_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
								}
								break;

							case MESH_EDIT_MODE.Modify:
								_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
								break;

							case MESH_EDIT_MODE.PivotEdit:
								_lowCPUStatus = LOW_CPU_STATUS.Full;
								break;
						}
					}
					
					break;

				case apSelection.SELECTION_TYPE.MeshGroup:
					{
						switch (_meshGroupEditMode)
						{
							case MESHGROUP_EDIT_MODE.Setting:
								_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
								break;

							case MESHGROUP_EDIT_MODE.Modifier:
								if(_selection.Modifier != null)
								{
									//모디파이어에 따라서 CPU가 다르다.
									//Physics > Full (편집 모드 상관 없음)
									//Rigging > Low
									//Morph계열 > 편집 모드 : Mid, 단 Blur 툴 켰을땐 Full / 일반 Low
									//Transform계열 > 편집 모드 : Mid / 일반 Low
									
									bool isEditMode = _selection.ExEditingMode != apSelection.EX_EDIT.None;

									switch (_selection.Modifier.ModifierType)
									{
										case apModifierBase.MODIFIER_TYPE.Physic:
											_lowCPUStatus = LOW_CPU_STATUS.Full;
											break;

										case apModifierBase.MODIFIER_TYPE.Rigging:
											if(Select.IsRigEditBinding && Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
											{
												//브러시로 Rigging을 하는 중일때
												_lowCPUStatus = LOW_CPU_STATUS.Full;
											}
											else
											{
												_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
											}
											
											break;

										case apModifierBase.MODIFIER_TYPE.Morph:
										case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
											if(isEditMode)
											{
												if(_gizmos != null && _gizmos.IsBrushMode)
												{
													_lowCPUStatus = LOW_CPU_STATUS.Full;
												}
												else
												{
													_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
												}
											}
											else
											{
												_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
											}
											break;

										case apModifierBase.MODIFIER_TYPE.TF:
										case apModifierBase.MODIFIER_TYPE.AnimatedTF:
											if(isEditMode)
											{
												_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
											}
											else
											{
												_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
											}
											break;

											//추가 21.7.20 : 색상 모디파이어
										case apModifierBase.MODIFIER_TYPE.ColorOnly:
										case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
											if(isEditMode)
											{
												_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
											}
											else
											{
												_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
											}
											break;
									}
								}
								else
								{
									_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
								}
								break;
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Animation:
					if(_selection.AnimClip != null)
					{
						//애니메이션 모드에서는
						//- 재생 중일 때는 항상 Full
						//- 편집 모드에서는 Mid / 단 Blur가 켜질땐 Full
						//- 그 외에는 Low
						bool isEditMode = _selection.ExAnimEditingMode != apSelection.EX_EDIT.None;
						if(_selection.AnimClip.IsPlaying_Editor)
						{
							_lowCPUStatus = LOW_CPU_STATUS.Full;
						}
						else if(isEditMode)
						{
							if(_gizmos != null && _gizmos.IsBrushMode)
							{
								_lowCPUStatus = LOW_CPU_STATUS.Full;
							}
							else
							{
								_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Mid;
							}
						}
						else
						{
							_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
						}
					}
					else
					{
						_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
					}
					break;

				case apSelection.SELECTION_TYPE.Param:
					//컨트롤 파라미터 메뉴에서는 항상 Low
					_lowCPUStatus = LOW_CPU_STATUS.LowCPU_Low;
					break;
			}
		}



		//--------------------------------------------------------------------------
		// 단축키 입력 처리
		//--------------------------------------------------------------------------
		public void OnHotKeyDown(KeyCode keyCode, bool isCtrl, bool isAlt, bool isShift)
		{
			if (_isHotKeyProcessable)
			{
				_isHotKeyEvent = true;
				_hotKeyCode = keyCode;

				_isHotKey_Ctrl = isCtrl;
				_isHotKey_Alt = isAlt;
				_isHotKey_Shift = isShift;
				_isHotKeyProcessable = false;
			}
			else
			{
				if (!_isHotKeyEvent)
				{
					_isHotKeyProcessable = true;
				}
			}
		}

		public void OnHotKeyUp()
		{
			_isHotKeyProcessable = true;
			_isHotKeyEvent = false;
			_hotKeyCode = KeyCode.A;
			_isHotKey_Ctrl = false;
			_isHotKey_Alt = false;
			_isHotKey_Shift = false;
		}

		public void UseHotKey()
		{
			_isHotKeyEvent = false;
			_hotKeyCode = KeyCode.A;
			_isHotKey_Ctrl = false;
			_isHotKey_Alt = false;
			_isHotKey_Shift = false;
		}

		public bool IsHotKeyEvent { get { return _isHotKeyEvent; } }
		public KeyCode HotKeyCode { get { return _hotKeyCode; } }
		public bool IsHotKey_Ctrl { get { return _isHotKey_Ctrl; } }
		public bool IsHotKey_Alt { get { return _isHotKey_Alt; } }
		public bool IsHotKey_Shift { get { return _isHotKey_Shift; } }


		public void AddReservedHotKeyEvent(apHotKey.FUNC_RESV_HOTKEY_EVENT funcEvent, apHotKey.RESERVED_KEY keyType, object paramObject)
		{
			HotKey.AddReservedHotKey(funcEvent, keyType, paramObject);
		}

		/// <summary>
		/// 추가 20.12.3 : 단축키 설정에 의한 단축키 등록
		/// </summary>
		public void AddHotKeyEvent(	apHotKey.FUNC_HOTKEY_EVENT funcEvent, 
									apHotKeyMapping.KEY_TYPE hotkeyType, object paramObject)
		{
			apHotKeyMapping.HotkeyMapUnit unit = HotKeyMap.GetHotkey(hotkeyType);
			if(unit == null
				|| !unit._isAvailable_Cur
				|| unit._keyCode_Cur == apHotKeyMapping.EST_KEYCODE.Unknown)
			{
				//유효하지 않은 단축키이다.
				return;
			}

			if(unit._isIgnoreSpecialKey)
			{
				HotKey.AddHotKeyEventIgnoreCtrlShift(	funcEvent,
														unit,
														paramObject);
			}
			else
			{
				HotKey.AddHotKeyEvent(	funcEvent, 
										unit,
										paramObject);
			}
		}

		




		//----------------------------------------------------------------------------
		// GUI 이벤트 타입에 따른 지연 처리
		//----------------------------------------------------------------------------
		
		// GUILayout의 Visible 여부가 외부에 의해 결정되는 경우
		// 바로 바뀌면 GUI 에러가 나므로 약간의 지연 처리를 해야한다.
		
		/// <summary>
		/// GUILayout가 Show/Hide 토글시, 그 정보를 저장한다.
		/// 범용으로 쓸 수 있게 하기 위해서 String을 키값으로 Dictionary에 저장한다.
		/// </summary>
		/// <param name="keyName">Dictionary 키값</param>
		/// <param name="isVisible">Visible 값</param>
		public void SetGUIVisible(DELAYED_UI_TYPE keyType, bool isVisible)
		{
			if (_delayedGUIShowList.ContainsKey(keyType))
			{
				if (_delayedGUIShowList[keyType] != isVisible)
				{
					_delayedGUIShowList[keyType] = isVisible;//Visible 조건 값

					//_delayedGUIToggledList는 "Visible 값이 바뀌었을때 그걸 바로 GUI에 적용했는지"를 저장한다.
					//바뀐 순간엔 GUI에 적용 전이므로 "Visible Toggle이 완료되었는지"를 저장하는 리스트엔 False를 넣어둔다.
					_delayedGUIToggledList[keyType] = false;
				}
			}
			else
			{
				_delayedGUIShowList.Add(keyType, isVisible);
				_delayedGUIToggledList.Add(keyType, false);
			}
		}


		/// <summary>
		/// GUILayout 출력 가능한지 여부 알려주는 함수
		/// Hide -> Show 상태에서 GUI Event가 Layout/Repaint가 아니라면 잠시 Hide 상태를 유지해야한다.
		/// </summary>
		/// <param name="keyType">Dictionary 키값</param>
		/// <returns>True이면 GUILayout을 출력할 수 있다. False는 안됨</returns>
		public bool IsDelayedGUIVisible(DELAYED_UI_TYPE keyType)
		{
			//GUI Layout이 출력되려면
			//1. Visible 값이 True여야 한다.
			//2-1. GUI Event가 Layout/Repaint 여야 한다.
			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다.


			//1-1. GUI Layout의 Visible 여부를 결정하는 값이 없다면 -> False
			if (!_delayedGUIShowList.ContainsKey(keyType))
			{
				return false;
			}

			//1-2. GUI Layout의 Visible 값이 False라면 -> False
			if (!_delayedGUIShowList[keyType])
			{
				return false;
			}

			//2. (Toggle 처리가 완료되지 않은 상태에서..)
			if (!_delayedGUIToggledList[keyType])
			{
				//2-1. GUI Event가 Layout/Repaint라면 -> 다음 OnGUI까지 일단 보류합니다. False
				if (!_isGUIEvent)
				{
					return false;
				}

				// GUI Event가 유효하다면 Visible이 가능하다고 바꿔줍니다.
				//_delayedGUIToggledList [False -> True]
				_delayedGUIToggledList[keyType] = true;
			}

			//2-2. GUI Event 종류에 상관없이 계속 Visible 상태였다면 출력 가능하다. -> True
			return true;
		}



		//---------------------------------------------------------------------------
		// 모달 체크
		//---------------------------------------------------------------------------
		/// <summary>
		/// FFD 등의 모달을 체크하고 선택한 기능을 수행할 수 있는지 리턴한다.
		/// true가 리턴되면 해당 기능을 계속해서 수행하되, false면 중단한다.
		/// </summary>
		/// <returns></returns>
		public bool CheckModalAndExecutable()
		{
			//1. FFD 모드에 의한 모달시 종료 여부를 물어보고 처리한다.
			if(Gizmos.IsFFDMode)
			{
				bool isFFDEnded = Gizmos.CheckAdaptOrRevertFFD();
				if(!isFFDEnded)
				{
					//FFD를 종료하지 않는다면 기능을 수행할 수 없다.
					return false;
				}
			}

			//TODO : 모달에 해당하는 기능이 있다면 여기서 추가하자

			//Modal 기능을 종료했거나 Modal 기능이 실행중이 아니라면 요청했던 기능들을 충분히 실행할 수 있다.
			return true;
		}



		//---------------------------------------------------------------------------
		// Portrait 생성
		//---------------------------------------------------------------------------
		private void MakeNewPortrait()
		{
			if (!_isMakePortraitRequest)
			{
				return;
			}

			_isMakePortraitRequest = false;

			//v1.4.2 : 프리팹 편집 화면에서는 생성할 수 없다.
			if(apEditorUtil.IsPrefabEditingScene())
			{
				EditorUtility.DisplayDialog(	GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Title),
												GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Body),
												GetText(TEXT.Okay));
				return;
			}


			GameObject newPortraitObj = new GameObject(_requestedNewPortraitName);
			newPortraitObj.transform.position = Vector3.zero;
			newPortraitObj.transform.rotation = Quaternion.identity;
			newPortraitObj.transform.localScale = Vector3.one;

			_portrait = newPortraitObj.AddComponent<apPortrait>();

			//Selection.activeGameObject = newPortraitObj;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			
			_requestedNewPortraitName = "";


			//추가
			//초기화시 Important와 Opt 정보는 여기서 별도로 초기화를 하자
			_portrait._isOptimizedPortrait = false;
			_portrait._bakeTargetOptPortrait = null;
			_portrait._bakeSrcEditablePortrait = null;
			_portrait._isImportant = true;


			_selection.SelectPortrait(_portrait);
			
			//Portrait의 레퍼런스들을 연결해주자
			Controller.PortraitReadyToEdit();//새로운 Portrait를 생성


			//추가 22.12.24
			//Project Setting에 의해 저장된 설정을 그대로 반영한다.
			if(ProjectSettingData.IsCommonSettingSaved)
			{
				ProjectSettingData.AdaptCommonSettingsToPortrait(_portrait);
			}






			//Selection.activeGameObject = _portrait.gameObject;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			//시작은 RootUnit
			_selection.SelectRootUnitDefault();

			OnAnyObjectAddedOrRemoved();

			SyncHierarchyOrders();

			_hierarchy.ResetAllUnits();
			_hierarchy_MeshGroup.ResetSubUnits();
			_hierarchy_AnimClip.ResetSubUnits();
		}

		private void MakePortraitFromBackupFile()
		{
			if(!_isMakePortraitRequestFromBackupFile)
			{
				return;
			}

			_isMakePortraitRequestFromBackupFile = false;


			//v1.4.2 : 프리팹 편집 화면에서는 생성할 수 없다.
			if(apEditorUtil.IsPrefabEditingScene())
			{
				EditorUtility.DisplayDialog(	GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Title),
												GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Body),
												GetText(TEXT.Okay));
				return;
			}


			apPortrait loadedPortrait = Backup.LoadBackup(_requestedLoadedBackupPortraitFilePath);
			if (loadedPortrait != null)
			{
				_portrait = loadedPortrait;

				//초기 설정 추가 (Important는 백업 설정을 따른다)
				_portrait._isOptimizedPortrait = false;
				_portrait._bakeTargetOptPortrait = null;
				_portrait._bakeSrcEditablePortrait = null;

				Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

				_selection.SelectPortrait(_portrait);

				//Portrait의 레퍼런스들을 연결해주자
				Controller.PortraitReadyToEdit();//백업파일로부터 Portrait를 생성


				//Selection.activeGameObject = _portrait.gameObject;
				Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

				//시작은 RootUnit
				_selection.SelectRootUnitDefault();

				OnAnyObjectAddedOrRemoved();

				SyncHierarchyOrders();

				_hierarchy.ResetAllUnits();
				_hierarchy_MeshGroup.ResetSubUnits();
				_hierarchy_AnimClip.ResetSubUnits();

				Notification("Backup File [" + _requestedLoadedBackupPortraitFilePath + "] is loaded", false, false);
			}

			
			_requestedLoadedBackupPortraitFilePath = "";
		}




		//-------------------------------------------------------------------------
		// 에디터 환경 설정
		//-------------------------------------------------------------------------
		public void SaveEditorPref()
		{
			//변경 21.2.10
			//EditorPrefs.SetInt 류의 함수를 SavePref_Int로 변경한다. 기본값을 필요로 한다.

			SavePref_Int("AnyPortrait_HierarchyFilter", (int)_hierarchyFilter, (int)HIERARCHY_FILTER.All);//EditorPrefs.SetInt("AnyPortrait_HierarchyFilter", (int)_hierarchyFilter);

			if (_selection != null)
			{	
				SavePref_Bool("AnyPortrait_IsAutoNormalize", Select._rigEdit_isAutoNormalize, true);//EditorPrefs.SetBool("AnyPortrait_IsAutoNormalize", Select._rigEdit_isAutoNormalize);
			}

			SavePref_Int("AnyPortrait_Language", (int)_language, (int)DefaultLanguage);//EditorPrefs.SetInt("AnyPortrait_Language", (int)_language);
			
			SaveColorPref("AnyPortrait_Color_Backgroud",	_colorOption_Background,	DefaultColor_Background);
			SaveColorPref("AnyPortrait_Color_GridCenter",	_colorOption_GridCenter,	DefaultColor_GridCenter);
			SaveColorPref("AnyPortrait_Color_Grid",			_colorOption_Grid,			DefaultColor_Grid);
			SaveColorPref("AnyPortrait_Color_InvertedBackground", _colorOption_InvertedBackground, DefaultColor_InvertedBackground);//추가 21.10.6

			SaveColorPref("AnyPortrait_Color_MeshEdge",			_colorOption_MeshEdge,			DefaultColor_MeshEdge);
			SaveColorPref("AnyPortrait_Color_MeshHiddenEdge",	_colorOption_MeshHiddenEdge,	DefaultColor_MeshHiddenEdge);
			SaveColorPref("AnyPortrait_Color_Outline",			_colorOption_Outline,			DefaultColor_Outline);
			SaveColorPref("AnyPortrait_Color_TFBorder",			_colorOption_TransformBorder,	DefaultColor_TransformBorder);

			SaveColorPref("AnyPortrait_Color_VertNotSelected",	_colorOption_VertColor_NotSelected,		DefaultColor_VertNotSelected);
			SaveColorPref("AnyPortrait_Color_VertSelected",		_colorOption_VertColor_Selected,		DefaultColor_VertSelected);

			SaveColorPref("AnyPortrait_Color_GizmoFFDLine",			_colorOption_GizmoFFDLine,		DefaultColor_GizmoFFDLine);
			SaveColorPref("AnyPortrait_Color_GizmoFFDInnerLine",	_colorOption_GizmoFFDInnerLine, DefaultColor_GizmoFFDInnerLine);

			SaveColorPref("AnyPortrait_Color_OnionToneColor",		_colorOption_OnionToneColor,		DefaultColor_OnionToneColor);
			SaveColorPref("AnyPortrait_Color_OnionAnimPrevColor",	_colorOption_OnionAnimPrevColor,	DefaultColor_OnionAnimPrevColor);
			SaveColorPref("AnyPortrait_Color_OnionAnimNextColor",	_colorOption_OnionAnimNextColor,	DefaultColor_OnionAnimNextColor);
			SaveColorPref("AnyPortrait_Color_OnionBoneColor",		_colorOption_OnionBoneColor,		DefaultColor_OnionBoneColor);
			SaveColorPref("AnyPortrait_Color_OnionBonePrevColor",	_colorOption_OnionBonePrevColor,	DefaultColor_OnionBonePrevColor);
			SaveColorPref("AnyPortrait_Color_OnionBoneNextColor",	_colorOption_OnionBoneNextColor,	DefaultColor_OnionBoneNextColor);

			SavePref_Bool("AnyPortrait_Onion_OutlineRender",		_onionOption_IsOutlineRender,	true);
			SavePref_Float("AnyPortrait_Onion_OutlineThickness",	_onionOption_OutlineThickness,	0.5f);
			SavePref_Bool("AnyPortrait_Onion_RenderOnlySelected",	_onionOption_IsRenderOnlySelected, false);
			SavePref_Bool("AnyPortrait_Onion_RenderBehind",			_onionOption_IsRenderBehind,	false);
			SavePref_Bool("AnyPortrait_Onion_RenderAnimFrames",		_onionOption_IsRenderAnimFrames, false);
			SavePref_Int("AnyPortrait_Onion_PrevRange",				_onionOption_PrevRange,			1);
			SavePref_Int("AnyPortrait_Onion_NextRange",				_onionOption_NextRange,			1);
			SavePref_Int("AnyPortrait_Onion_RenderPerFrame",		_onionOption_RenderPerFrame,	1);
			SavePref_Float("AnyPortrait_Onion_PosOffsetX",			_onionOption_PosOffsetX,		0.0f);
			SavePref_Float("AnyPortrait_Onion_PosOffsetY",			_onionOption_PosOffsetY,		0.0f);
			SavePref_Bool("AnyPortrait_Onion_IKCalculate",			_onionOption_IKCalculateForce,	false);


			SavePref_Int("AnyPortrait_AnimTimelineLayerSort", (int)_timelineInfoSortType, (int)TIMELINE_INFO_SORT.Registered);

			SavePref_Int("AnyPortrait_Capture_PosX",		_captureFrame_PosX,	0);
			SavePref_Int("AnyPortrait_Capture_PosY",		_captureFrame_PosY,	0);
			SavePref_Int("AnyPortrait_Capture_SrcWidth",	_captureFrame_SrcWidth,		500);
			SavePref_Int("AnyPortrait_Capture_SrcHeight",	_captureFrame_SrcHeight,	500);
			SavePref_Int("AnyPortrait_Capture_DstWidth",	_captureFrame_DstWidth,		500);
			SavePref_Int("AnyPortrait_Capture_DstHeight",	_captureFrame_DstHeight,	500);

			SavePref_Int("AnyPortrait_Capture_SpriteUnitWidth",		_captureFrame_SpriteUnitWidth, 100);
			SavePref_Int("AnyPortrait_Capture_SpriteUnitHeight",	_captureFrame_SpriteUnitHeight, 100);
			SavePref_Int("AnyPortrait_Capture_SpriteMargin",		_captureFrame_SpriteMargin, 0);
			

			SavePref_Bool("AnyPortrait_Capture_IsShowFrame",	_isShowCaptureFrame, true);
			SaveColorPref("AnyPortrait_Capture_BGColor",		_captureFrame_Color, Color.black);
			SavePref_Bool("AnyPortrait_Capture_IsPhysics",		_captureFrame_IsPhysics, false);

			SavePref_Bool("AnyPortrait_Capture_IsAspectRatioFixed",		_isCaptureAspectRatioFixed, true);
			SavePref_Int("AnyPortrait_Capture_GIFQuality",				(int)_captureFrame_GIFQuality, (int)CAPTURE_GIF_QUALITY.High);			
			SavePref_Int("AnyPortrait_Capture_GIFLoopCount",			_captureFrame_GIFSampleLoopCount, 1);

			SavePref_Int("AnyPortrait_Capture_SpritePackImageWidth",	(int)_captureSpritePackImageWidth,	(int)(CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024));
			SavePref_Int("AnyPortrait_Capture_SpritePackImageHeight",	(int)_captureSpritePackImageHeight, (int)(CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024));
			SavePref_Int("AnyPortrait_Capture_SpriteTrimSize",			(int)_captureSpriteTrimSize,		(int)(CAPTURE_SPRITE_TRIM_METHOD.Fixed));
			SavePref_Bool("AnyPortrait_Capture_SpriteMeta_XML",			_captureSpriteMeta_XML,		false);
			SavePref_Bool("AnyPortrait_Capture_SpriteMeta_JSON",		_captureSpriteMeta_JSON,	false);
			SavePref_Bool("AnyPortrait_Capture_SpriteMeta_TXT",			_captureSpriteMeta_TXT,		false);
			
			SavePref_Float("AnyPortrait_Capture_SpriteScreenPosX",		_captureSprite_ScreenPos.x, 0.0f);
			SavePref_Float("AnyPortrait_Capture_SpriteScreenPosY",		_captureSprite_ScreenPos.y, 0.0f);
			SavePref_Int("AnyPortrait_Capture_SpriteScreenZoomIndex",	_captureSprite_ScreenZoom, ZOOM_INDEX_DEFAULT);

		
			SavePref_Int("AnyPortrait_BoneRenderMode",			(int)_boneGUIRenderMode,		(int)BONE_RENDER_MODE.Render);
			
			
			SavePref_Bool("AnyPortrait_GUI_FPSVisible",			_guiOption_isFPSVisible,		DefaultGUIOption_ShowFPS);
			SavePref_Bool("AnyPortrait_GUI_StatisticsVisible",	_guiOption_isStatisticsVisible, DefaultGUIOption_ShowStatistics);


			SavePref_Bool("AnyPortrait_AutoBackup_Enabled",		_backupOption_IsAutoSave,		DefaultBackupOption_IsAutoSave);
			SavePref_String("AnyPortrait_AutoBackup_Path",		_backupOption_BaseFolderName,	DefaultBackupOption_BaseFolderName);
			SavePref_Int("AnyPortrait_AutoBackup_Time",			_backupOption_Minute,			DefaultBackupOption_Minute);

			SavePref_String("AnyPortrait_BonePose_Path", _bonePose_BaseFolderName, DefaultBonePoseOption_BaseFolderName);

			SavePref_Bool("AnyPortrait_StartScreen_IsShow",			_startScreenOption_IsShowStartup,	DefaultStartScreenOption_IsShowStartup);
			SavePref_Int("AnyPortrait_StartScreen_LastMonth",		_startScreenOption_LastMonth,		0);
			SavePref_Int("AnyPortrait_StartScreen_LastDay",			_startScreenOption_LastDay,			0);
			SavePref_Int("AnyPortrait_UpdateLogScreen_LastVersion", _updateLogScreen_LastVersion,		0);
			

			//[v1.4.2 삭제] : ProjectSettings로 이전
			//SavePref_Bool("AnyPortrait_IsBakeColorSpace_ToGamma",	_isBakeColorSpaceToGamma,	true);
			//SavePref_Bool("AnyPortrait_IsUseLWRPShader",			_isUseSRP,					false);

			//이전
			//SavePref_Bool("AnyPortrait_ModLockOp_CalculateIfNotAddedOther",	_modLockOption_CalculateIfNotAddedOther,	false);
			//SavePref_Bool("AnyPortrait_ModLockOp_ColorPreview_Lock",		_modLockOption_ColorPreview_Lock,			false);
			//SavePref_Bool("AnyPortrait_ModLockOp_ColorPreview_Unlock",		_modLockOption_ColorPreview_Unlock,			true);
			//SavePref_Bool("AnyPortrait_ModLockOp_BonePreview_Lock",			_modLockOption_BonePreview_Lock,			false);
			//SavePref_Bool("AnyPortrait_ModLockOp_BonePreview_Unlock",		_modLockOption_BonePreview_Unlock,			true);
			//SavePref_Bool("AnyPortrait_ModLockOp_ModListUI_Lock",			_modLockOption_ModListUI_Lock,				false);
			//SavePref_Bool("AnyPortrait_ModLockOp_ModListUI_Unlock",			_modLockOption_ModListUI_Unlock,			false);

			//변경 21.2.13 : 기존 키 삭제 후, 새로운 옵션 추가
			EditorPrefs.DeleteKey("AnyPortrait_ModLockOp_CalculateIfNotAddedOther");
			EditorPrefs.DeleteKey("AnyPortrait_ModLockOp_ColorPreview_Lock");
			EditorPrefs.DeleteKey("AnyPortrait_ModLockOp_ColorPreview_Unlock");
			EditorPrefs.DeleteKey("AnyPortrait_ModLockOp_BonePreview_Lock");
			EditorPrefs.DeleteKey("AnyPortrait_ModLockOp_BonePreview_Unlock");
			EditorPrefs.DeleteKey("AnyPortrait_ModLockOp_ModListUI_Lock");
			EditorPrefs.DeleteKey("AnyPortrait_ModLockOp_ModListUI_Unlock");

			SavePref_Bool("AnyPortrait_ModLockOption_ColorPreview",			_modLockOption_ColorPreview,		false);
			SavePref_Bool("AnyPortrait_ModLockOption_BoneResultPreview",	_modLockOption_BoneResultPreview,	false);
			SavePref_Bool("AnyPortrait_ModLockOption_ModListUI",			_modLockOption_ModListUI,			false);


			SaveColorPref("AnyPortrait_ModLockOp_BonePreviewColor", _modLockOption_BonePreviewColor, DefauleColor_ModLockOpt_BonePreview);

			SavePref_Int("AnyPortrait_LastCheckLiveVersion_Day",	_lastCheckLiveVersion_Day,		0);
			SavePref_Int("AnyPortrait_LastCheckLiveVersion_Month",	_lastCheckLiveVersion_Month,	0);
			SavePref_String("AnyPortrait_LastCheckLiveVersion",		_currentLiveVersion,			"");
			SavePref_Bool("AnyPortrait_CheckLiveVersionEnabled",	_isCheckLiveVersion_Option,		DefaultCheckLiverVersionOption);

			SavePref_Bool("AnyPortrait_IsVersionCheckIgnored",		_isVersionNoticeIgnored,		false);
			//SavePref_Int("AnyPortrait_VersionCheckIgnored_Year",	_versionNoticeIvnored_Year,		0);
			//SavePref_Int("AnyPortrait_VersionCheckIgnored_Month",	_versionNoticeIvnored_Month,	0);
			//SavePref_Int("AnyPortrait_VersionCheckIgnored_Day",		_versionNoticeIvnored_Day,		0);
			SavePref_Int("AnyPortrait_VersionCheckIgnored_Ver",		_versionNoticeIgnored_Ver,		-1);
			


			SavePref_Float("AnyPortrait_MeshTRSOption_MirrorOffset",			_meshTRSOption_MirrorOffset,			0.5f);
			SavePref_Bool("AnyPortrait_MeshTRSOption_MirrorAddVertOnRuler",		_meshTRSOption_MirrorSnapVertOnRuler,	false);
			SavePref_Bool("AnyPortrait_MeshTRSOption_MirrorRemoved",			_meshTRSOption_MirrorRemoved,			false);
			
			//AutoMesh V1에 대한 키는 지우자
			EditorPrefs.DeleteKey("AnyPortrait_MeshAutoGenOption_AlphaCutOff");
			EditorPrefs.DeleteKey("AnyPortrait_MeshAutoGenOption_GridSize");
			EditorPrefs.DeleteKey("AnyPortrait_MeshAutoGenOption_Margin");
			EditorPrefs.DeleteKey("AnyPortrait_MeshAutoGenOption_NumPoint_QuadX");
			EditorPrefs.DeleteKey("AnyPortrait_MeshAutoGenOption_NumPoint_QuadY");
			EditorPrefs.DeleteKey("AnyPortrait_MeshAutoGenOption_NumPoint_Circle");
			EditorPrefs.DeleteKey("AnyPortrait_MeshAutoGenOption_IsLockAxis");

			//V2
			SavePref_Int("AnyPortrait_MeshAutoV2_Inner_Density", _meshAutoGenV2Option_Inner_Density, 2);
			SavePref_Int("AnyPortrait_MeshAutoV2_OuterMargin", _meshAutoGenV2Option_OuterMargin, 10);
			SavePref_Int("AnyPortrait_MeshAutoV2_InnerMargin", _meshAutoGenV2Option_InnerMargin, 5);
			SavePref_Bool("AnyPortrait_MeshAutoV2_IsInnerMargin", _meshAutoGenV2Option_IsInnerMargin, false);
			SavePref_Int("AnyPortrait_MeshAutoV2_QuickPresetType", _meshAutoGenV2Option_QuickPresetType, 0);
			
			
			//추가 21.2.10
			SavePref_Bool("AnyPortrait_ExModObjOption_UpdateByOtherMod",	_exModObjOption_UpdateByOtherMod,	DefaultExModObjOption_UpdateByOtherMod);
			SavePref_Bool("AnyPortrait_ExModObjOption_ShowGray",			_exModObjOption_ShowGray,			DefaultExModObjOption_ShowGray);
			SavePref_Bool("AnyPortrait_ExModObjOption_NotSelectable",		_exModObjOption_NotSelectable,		DefaultExModObjOption_NotSelectable);
			
			SavePref_Bool("AnyPortrait_LowCPUOption", _isLowCPUOption, DefaultLowCPUOption);

			SavePref_Bool("AnyPortrait_SelectionLockOption_RiggingPhysics",		_isSelectionLockOption_RiggingPhysics,	DefaultSelectionLockOption_RiggingPhysics);
			SavePref_Bool("AnyPortrait_SelectionLockOption_Morph",				_isSelectionLockOption_Morph,			DefaultSelectionLockOption_Morph);
			SavePref_Bool("AnyPortrait_SelectionLockOption_Transform",			_isSelectionLockOption_Transform,		DefaultSelectionLockOption_Transform);
			SavePref_Bool("AnyPortrait_SelectionLockOption_ControlParamTimeline", _isSelectionLockOption_ControlParamTimeline, DefaultSelectionLockOption_ControlParamTimeline);
			
			SavePref_Int("AnyPortrait_HierachySortMode", (int)_hierarchySortMode, (int)HIERARCHY_SORT_MODE.RegOrder);

			SavePref_Bool("AnyPortrait_AmbientCorrectionOption",_isAmbientCorrectionOption, DefaultAmbientCorrectionOption);

			SavePref_Bool("AnyPortrait_AutoSwitchControllerTab_Mod",	_isAutoSwitchControllerTab_Mod,		DefaultAutoSwitchControllerTab_Mod);
			SavePref_Bool("AnyPortrait_AutoSwitchControllerTab_Anim",	_isAutoSwitchControllerTab_Anim,	DefaultAutoSwitchControllerTab_Anim);

			SavePref_Bool("AnyPortrait_RestoreTempMeshVisbility", _isRestoreTempMeshVisibilityWhenTaskEnded, DefaultRestoreTempMeshVisibiilityWhenTaskEnded);


			SavePref_Bool("AnyPortrait_RigView_WeightOnly",	_rigViewOption_WeightOnly,	false);
			SavePref_Bool("AnyPortrait_RigView_BoneColor",	_rigViewOption_BoneColor,	true);
			SavePref_Bool("AnyPortrait_RigView_CircleVert",	_rigViewOption_CircleVert,	false);

			EditorPrefs.DeleteKey("AnyPortrait_RigOption_ColorLikeParent");//키 삭제
			//EditorPrefs.SetBool("AnyPortrait_RigOption_ColorLikeParent", _rigOption_NewChildBoneColorIsLikeParent);//>>_boneGUIOption_NewBoneColor 변수로 변경
			
			SavePref_Bool("AnyPortrait_MacOSXOption_ShowStartup", _macOSXInfoScreenOption_IsShowStartup, true);
			SavePref_Int("AnyPortrait_MacOSXOption_LastMonth", _macOSXInfoScreenOption_LastMonth, 0);
			SavePref_Int("AnyPortrait_MacOSXOption_LastDay", _macOSXInfoScreenOption_LastDay, 0);
			
			SavePref_Int("AnyPortrait_VertGUI_SizeRatio_Index", _vertGUIOption_SizeRatio_Index, DefaultVertGUIOption_SizeRatio_Index);

			SavePref_Int("AnyPortrait_BoneGUI_RenderType", (int)_boneGUIOption_RenderType, (int)DefaultBoneGUIOption_RenderType);
			SavePref_Int("AnyPortrait_BoneGUI_SizeRatio_Index", _boneGUIOption_SizeRatio_Index, DefaultBoneGUIOption_SizeRatio_Index);
			SavePref_Bool("AnyPortrait_BoneGUI_ScaledByZoom", _boneGUIOption_ScaledByZoom, DefaultBoneGUIOption_ScaedByZoom);
			SavePref_Bool("AnyPortrait_RigOption_ColorLikeParent", _boneGUIOption_NewBoneColor == NEW_BONE_COLOR.SimilarColor, DefaultBoneGUIOption_NewBoneColor_Bool);
			SavePref_Int("AnyPortrait_BoneGUI_NewBonePreview", (int)_boneGUIOption_NewBonePreview, (int)DefaultBoneGUIOption_NewBonePreview);

			SavePref_Int("AnyPortrait_RigGUI_SizeRatio_Index",			_rigGUIOption_VertRatio_Index,				DefaultRigGUIOption_VertRatio_Index);
			SavePref_Bool("AnyPortrait_RigGUI_ScaledByZoom",			_rigGUIOption_ScaledByZoom,					DefaultRigGUIOption_ScaledByZoom);
			SavePref_Int("AnyPortrait_RigGUI_SizeRatioSelected_Index",	_rigGUIOption_VertRatio_Selected_Index,		DefaultRigGUIOption_VertRatio_Selected_Index);
			SavePref_Int("AnyPortrait_RigGUI_SelectedWeightGUIType",	(int)_rigGUIOption_SelectedWeightGUIType,	(int)DefaultRigGUIOption_SelectedWeightGUIType);
			SavePref_Int("AnyPortrait_RigGUI_NolinkedBoneVisibility",	(int)_rigGUIOption_NoLinkedBoneVisibility,	(int)DefaultRigGUIOption_NoLinkedBoneVisibility);
			SavePref_Int("AnyPortrait_RigGUI_WeightGradientColor",		(int)_rigGUIOption_WeightGradientColor,		(int)DefaultRigGUIOption_WeightGradientColor);


			SavePref_Bool("AnyPortrait_NeedToAskRemoveVertByPSDImport",	_isNeedToAskRemoveVertByPSDImport, false);
			SavePref_Bool("AnyPortrait_ShowPrevViewMenuBtns",			_option_ShowPrevViewMenuBtns, false);

			SavePref_Bool("AnyPortrait_SetAutoImageToMeshIfOnlyOneImageExist",			_option_SetAutoImageToMeshIfOnlyOneImageExist, true);
			
			SavePref_Bool("AnyPortrait_IsTurnOffAnimAutoKey",			_option_IsTurnOffAnimAutoKey, true);

			SavePref_Bool("AnyPortrait_ShowHowToEdit", _guiOption_isShowHowToEdit, true);

			SavePref_Bool("AnyPortrait_UseCPPPlugin", _cppPluginOption_UsePlugin, false);


			SavePref_Bool("AnyPortrait_IsShowURPWarningMsg", _isShowURPWarningMsg, true);
			SavePref_Bool("AnyPortrait_IsCheckSRPWhenBake", _option_CheckSRPWhenBake, true);


			SavePref_Bool("AnyPortrait_PinOption_AutoWeightRefresh", _pinOption_AutoWeightRefresh, true);

			SavePref_Int("AnyPortrait_ControlParamUI_Size_Option", (int)_controlParamUISizeOption, (int)CONTROL_PARAM_UI_SIZE_OPTION.Default);

			SavePref_Bool("AnyPortrait_VisibilityPresetOption_TurnOff", _option_TurnOffVisibilityPresetWhenSelectObject, false);

			SavePref_Bool("AnyPortrait_AutoScrollListObjSelected", _option_AutoScrollWhenObjectSelected, true);

			SavePref_Bool("AnyPortrait_ObjMovableWithoutClickGizmo", _option_ObjMovableWithoutClickGizmo, true);


			apGL.SetToneOption(_colorOption_OnionToneColor, _onionOption_OutlineThickness, _onionOption_IsOutlineRender, _onionOption_PosOffsetX, _onionOption_PosOffsetY, _colorOption_OnionBoneColor);
			apGL.SetRiggingOption(	RigGUIOption_SizeRatioX100, 
									RigGUIOption_SizeRatioX100_Selected, 
									_rigGUIOption_ScaledByZoom,
									_rigGUIOption_SelectedWeightGUIType,
									_rigGUIOption_WeightGradientColor);
		}

		public void LoadEditorPref()
		{
			//Debug.Log("Load Editor Pref");

			_hierarchyFilter = (HIERARCHY_FILTER)EditorPrefs.GetInt("AnyPortrait_HierarchyFilter", (int)HIERARCHY_FILTER.All);

			if (_selection != null)
			{
				Select._rigEdit_isAutoNormalize = EditorPrefs.GetBool("AnyPortrait_IsAutoNormalize", true);
			}

			_language = (LANGUAGE)EditorPrefs.GetInt("AnyPortrait_Language", (int)DefaultLanguage);

			_colorOption_Background = LoadColorPref("AnyPortrait_Color_Backgroud", DefaultColor_Background);
			_colorOption_GridCenter = LoadColorPref("AnyPortrait_Color_GridCenter", DefaultColor_GridCenter);
			_colorOption_Grid = LoadColorPref("AnyPortrait_Color_Grid", DefaultColor_Grid);
			_colorOption_InvertedBackground = LoadColorPref("AnyPortrait_Color_InvertedBackground", DefaultColor_InvertedBackground);//추가 21.10.6

			_colorOption_MeshEdge = LoadColorPref("AnyPortrait_Color_MeshEdge", DefaultColor_MeshEdge);
			_colorOption_MeshHiddenEdge = LoadColorPref("AnyPortrait_Color_MeshHiddenEdge", DefaultColor_MeshHiddenEdge);
			_colorOption_Outline = LoadColorPref("AnyPortrait_Color_Outline", DefaultColor_Outline);
			_colorOption_TransformBorder = LoadColorPref("AnyPortrait_Color_TFBorder", DefaultColor_TransformBorder);

			_colorOption_VertColor_NotSelected = LoadColorPref("AnyPortrait_Color_VertNotSelected", DefaultColor_VertNotSelected);
			_colorOption_VertColor_Selected = LoadColorPref("AnyPortrait_Color_VertSelected", DefaultColor_VertSelected);

			_colorOption_GizmoFFDLine = LoadColorPref("AnyPortrait_Color_GizmoFFDLine", DefaultColor_GizmoFFDLine);
			_colorOption_GizmoFFDInnerLine = LoadColorPref("AnyPortrait_Color_GizmoFFDInnerLine", DefaultColor_GizmoFFDInnerLine);

			_colorOption_OnionToneColor = LoadColorPref("AnyPortrait_Color_OnionToneColor", DefaultColor_OnionToneColor);
			_colorOption_OnionAnimPrevColor = LoadColorPref("AnyPortrait_Color_OnionAnimPrevColor", DefaultColor_OnionAnimPrevColor);
			_colorOption_OnionAnimNextColor = LoadColorPref("AnyPortrait_Color_OnionAnimNextColor", DefaultColor_OnionAnimNextColor);
			_colorOption_OnionBoneColor = LoadColorPref("AnyPortrait_Color_OnionBoneColor", DefaultColor_OnionBoneColor);
			_colorOption_OnionBonePrevColor = LoadColorPref("AnyPortrait_Color_OnionBonePrevColor", DefaultColor_OnionBonePrevColor);
			_colorOption_OnionBoneNextColor = LoadColorPref("AnyPortrait_Color_OnionBoneNextColor", DefaultColor_OnionBoneNextColor);

			_onionOption_IsOutlineRender = EditorPrefs.GetBool("AnyPortrait_Onion_OutlineRender", true);
			_onionOption_OutlineThickness = EditorPrefs.GetFloat("AnyPortrait_Onion_OutlineThickness", 0.5f);
			_onionOption_IsRenderOnlySelected = EditorPrefs.GetBool("AnyPortrait_Onion_RenderOnlySelected", false);
			_onionOption_IsRenderBehind = EditorPrefs.GetBool("AnyPortrait_Onion_RenderBehind", false);
			_onionOption_IsRenderAnimFrames = EditorPrefs.GetBool("AnyPortrait_Onion_RenderAnimFrames", false);
			_onionOption_PrevRange = EditorPrefs.GetInt("AnyPortrait_Onion_PrevRange", 1);
			_onionOption_NextRange = EditorPrefs.GetInt("AnyPortrait_Onion_NextRange", 1);
			_onionOption_RenderPerFrame = EditorPrefs.GetInt("AnyPortrait_Onion_RenderPerFrame", 1);
			_onionOption_PosOffsetX = EditorPrefs.GetFloat("AnyPortrait_Onion_PosOffsetX", 0.0f);
			_onionOption_PosOffsetY = EditorPrefs.GetFloat("AnyPortrait_Onion_PosOffsetY", 0.0f);
			_onionOption_IKCalculateForce = EditorPrefs.GetBool("AnyPortrait_Onion_IKCalculate", false);
			

			_timelineInfoSortType = (TIMELINE_INFO_SORT)EditorPrefs.GetInt("AnyPortrait_AnimTimelineLayerSort", (int)TIMELINE_INFO_SORT.Registered);

			_captureFrame_PosX = EditorPrefs.GetInt("AnyPortrait_Capture_PosX", 0);
			_captureFrame_PosY = EditorPrefs.GetInt("AnyPortrait_Capture_PosY", 0);
			_captureFrame_SrcWidth = EditorPrefs.GetInt("AnyPortrait_Capture_SrcWidth", 500);
			_captureFrame_SrcHeight = EditorPrefs.GetInt("AnyPortrait_Capture_SrcHeight", 500);
			_captureFrame_DstWidth = EditorPrefs.GetInt("AnyPortrait_Capture_DstWidth", 500);
			_captureFrame_DstHeight = EditorPrefs.GetInt("AnyPortrait_Capture_DstHeight", 500);

			_captureFrame_SpriteUnitWidth = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteUnitWidth", 100);
			_captureFrame_SpriteUnitHeight = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteUnitHeight", 100);
			_captureFrame_SpriteMargin = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteMargin", 0);

			_isShowCaptureFrame = EditorPrefs.GetBool("AnyPortrait_Capture_IsShowFrame", true);
			_captureFrame_GIFQuality = (CAPTURE_GIF_QUALITY)EditorPrefs.GetInt("AnyPortrait_Capture_GIFQuality", (int)CAPTURE_GIF_QUALITY.High);
			_captureFrame_GIFSampleLoopCount = EditorPrefs.GetInt("AnyPortrait_Capture_GIFLoopCount", 1);

			_isCaptureAspectRatioFixed = EditorPrefs.GetBool("AnyPortrait_Capture_IsAspectRatioFixed", true);

			_captureFrame_Color = LoadColorPref("AnyPortrait_Capture_BGColor", Color.black);
			_captureFrame_IsPhysics = EditorPrefs.GetBool("AnyPortrait_Capture_IsPhysics", false);

			_captureSpritePackImageWidth = (CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorPrefs.GetInt("AnyPortrait_Capture_SpritePackImageWidth", (int)(CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024));
			_captureSpritePackImageHeight = (CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorPrefs.GetInt("AnyPortrait_Capture_SpritePackImageHeight", (int)(CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024));
			_captureSpriteTrimSize = (CAPTURE_SPRITE_TRIM_METHOD)EditorPrefs.GetInt("AnyPortrait_Capture_SpriteTrimSize", (int)(CAPTURE_SPRITE_TRIM_METHOD.Fixed));
			_captureSpriteMeta_XML = EditorPrefs.GetBool("AnyPortrait_Capture_SpriteMeta_XML", false);
			_captureSpriteMeta_JSON = EditorPrefs.GetBool("AnyPortrait_Capture_SpriteMeta_JSON", false);
			_captureSpriteMeta_TXT = EditorPrefs.GetBool("AnyPortrait_Capture_SpriteMeta_TXT", false);
			
			_captureSprite_ScreenPos.x = EditorPrefs.GetFloat("AnyPortrait_Capture_SpriteScreenPosX", 0.0f);
			_captureSprite_ScreenPos.y = EditorPrefs.GetFloat("AnyPortrait_Capture_SpriteScreenPosY", 0.0f);
			_captureSprite_ScreenZoom = EditorPrefs.GetInt("AnyPortrait_Capture_SpriteScreenZoomIndex", ZOOM_INDEX_DEFAULT);
			
			

			_boneGUIRenderMode = (BONE_RENDER_MODE)EditorPrefs.GetInt("AnyPortrait_BoneRenderMode", (int)BONE_RENDER_MODE.Render);
			

			_guiOption_isFPSVisible = EditorPrefs.GetBool("AnyPortrait_GUI_FPSVisible", DefaultGUIOption_ShowFPS);
			_guiOption_isStatisticsVisible = EditorPrefs.GetBool("AnyPortrait_GUI_StatisticsVisible", DefaultGUIOption_ShowStatistics);

			_backupOption_IsAutoSave =		EditorPrefs.GetBool("AnyPortrait_AutoBackup_Enabled", DefaultBackupOption_IsAutoSave);
			_backupOption_BaseFolderName =	EditorPrefs.GetString("AnyPortrait_AutoBackup_Path", DefaultBackupOption_BaseFolderName);
			_backupOption_Minute =			EditorPrefs.GetInt("AnyPortrait_AutoBackup_Time", DefaultBackupOption_Minute);
			
			_bonePose_BaseFolderName = EditorPrefs.GetString("AnyPortrait_BonePose_Path", DefaultBonePoseOption_BaseFolderName);

			_startScreenOption_IsShowStartup = EditorPrefs.GetBool("AnyPortrait_StartScreen_IsShow", DefaultStartScreenOption_IsShowStartup);
			_startScreenOption_LastMonth = EditorPrefs.GetInt("AnyPortrait_StartScreen_LastMonth", 0);
			_startScreenOption_LastDay = EditorPrefs.GetInt("AnyPortrait_StartScreen_LastDay", 0);

			_updateLogScreen_LastVersion = EditorPrefs.GetInt("AnyPortrait_UpdateLogScreen_LastVersion", 0);

			//[v1.4.2] 삭제 : ProjectSettingData로 이전
			//_isBakeColorSpaceToGamma = EditorPrefs.GetBool("AnyPortrait_IsBakeColorSpace_ToGamma", true);
			//_isUseSRP = EditorPrefs.GetBool("AnyPortrait_IsUseLWRPShader", false);

			//이전 : 이 옵션들을 삭제
			//_modLockOption_CalculateIfNotAddedOther = EditorPrefs.GetBool("AnyPortrait_ModLockOp_CalculateIfNotAddedOther",	false);			
			//_modLockOption_ColorPreview_Lock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_ColorPreview_Lock",		false);
			//_modLockOption_ColorPreview_Unlock =	EditorPrefs.GetBool("AnyPortrait_ModLockOp_ColorPreview_Unlock",	true);//<< True 기본값
			//_modLockOption_BonePreview_Lock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_BonePreview_Lock",		false);
			//_modLockOption_BonePreview_Unlock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_BonePreview_Unlock",		true);//<< True 기본값
			//_modLockOption_ModListUI_Lock =			EditorPrefs.GetBool("AnyPortrait_ModLockOp_ModListUI_Lock",			false);
			//_modLockOption_ModListUI_Unlock =		EditorPrefs.GetBool("AnyPortrait_ModLockOp_ModListUI_Unlock",		false);

			//변경 21.2.13 : ModLock을 삭제하고 옵션이 모두 통합되었다.
			_modLockOption_ColorPreview = EditorPrefs.GetBool("AnyPortrait_ModLockOption_ColorPreview", false);
			_modLockOption_BoneResultPreview = EditorPrefs.GetBool("AnyPortrait_ModLockOption_BoneResultPreview", false);
			_modLockOption_ModListUI = EditorPrefs.GetBool("AnyPortrait_ModLockOption_ModListUI", false);


			//_modLockOption_MeshPreviewColor = LoadColorPref("AnyPortrait_ModLockOp_MeshPreviewColor", DefauleColor_ModLockOpt_MeshPreview);
			_modLockOption_BonePreviewColor = LoadColorPref("AnyPortrait_ModLockOp_BonePreviewColor", DefauleColor_ModLockOpt_BonePreview);

			_lastCheckLiveVersion_Day = EditorPrefs.GetInt("AnyPortrait_LastCheckLiveVersion_Day", 0);
			_lastCheckLiveVersion_Month = EditorPrefs.GetInt("AnyPortrait_LastCheckLiveVersion_Month", 0);
			_currentLiveVersion = EditorPrefs.GetString("AnyPortrait_LastCheckLiveVersion", "");
			_isCheckLiveVersion_Option = EditorPrefs.GetBool("AnyPortrait_CheckLiveVersionEnabled", DefaultCheckLiverVersionOption);

			_isVersionNoticeIgnored = EditorPrefs.GetBool("AnyPortrait_IsVersionCheckIgnored", false);
			//_versionNoticeIvnored_Year = EditorPrefs.GetInt("AnyPortrait_VersionCheckIgnored_Year", 0);
			//_versionNoticeIvnored_Month = EditorPrefs.GetInt("AnyPortrait_VersionCheckIgnored_Month", 0);
			//_versionNoticeIvnored_Day = EditorPrefs.GetInt("AnyPortrait_VersionCheckIgnored_Day", 0);
			_versionNoticeIgnored_Ver = EditorPrefs.GetInt("AnyPortrait_VersionCheckIgnored_Ver", -1);

			
			_meshTRSOption_MirrorOffset = EditorPrefs.GetFloat("AnyPortrait_MeshTRSOption_MirrorOffset", 0.5f);
			_meshTRSOption_MirrorSnapVertOnRuler = EditorPrefs.GetBool("AnyPortrait_MeshTRSOption_MirrorAddVertOnRuler", false);
			_meshTRSOption_MirrorRemoved = EditorPrefs.GetBool("AnyPortrait_MeshTRSOption_MirrorRemoved", false);
			
			//이전 코드 : AutoMesh V1
			//_meshAutoGenOption_AlphaCutOff = EditorPrefs.GetFloat("AnyPortrait_MeshAutoGenOption_AlphaCutOff", 0.02f);
			//_meshAutoGenOption_GridDivide = EditorPrefs.GetInt("AnyPortrait_MeshAutoGenOption_GridSize", 2);
			//_meshAutoGenOption_Margin = EditorPrefs.GetInt("AnyPortrait_MeshAutoGenOption_Margin", 2);
			//_meshAutoGenOption_numControlPoint_ComplexQuad_X = EditorPrefs.GetInt("AnyPortrait_MeshAutoGenOption_NumPoint_QuadX", 3);
			//_meshAutoGenOption_numControlPoint_ComplexQuad_Y = EditorPrefs.GetInt("AnyPortrait_MeshAutoGenOption_NumPoint_QuadY", 3);
			//_meshAutoGenOption_numControlPoint_CircleRing = EditorPrefs.GetInt("AnyPortrait_MeshAutoGenOption_NumPoint_Circle", 4);
			//_meshAutoGenOption_IsLockAxis = EditorPrefs.GetBool("AnyPortrait_MeshAutoGenOption_IsLockAxis", false);

			//추가 21.1.4 : AutoMesh V2
			//_meshAutoGenV2Option_Outline_Density = EditorPrefs.GetInt("AnyPortrait_MeshAutoV2_OutlineDivide_Density", 2);
			//_meshAutoGenV2Option_OutlineVertMerge_Radius = EditorPrefs.GetFloat("AnyPortrait_MeshAutoV2_OutlineVertMerge_Radius", 60.0f);
			//_meshAutoGenV2Option_OutlineVertMerge_Angle = EditorPrefs.GetFloat("AnyPortrait_MeshAutoV2_OutlineVertMerge_Angle", 10.0f);
			//_meshAutoGenV2Option_Out2Inline_Radius = EditorPrefs.GetFloat("AnyPortrait_MeshAutoV2_Out2Inline_Radius", 20.0f);
			_meshAutoGenV2Option_Inner_Density = EditorPrefs.GetInt("AnyPortrait_MeshAutoV2_Inner_Density", 2);
			//_meshAutoGenV2Option_Inner_IsRelax = EditorPrefs.GetBool("AnyPortrait_MeshAutoV2_Inner_IsRelax", true);
			//_meshAutoGenV2Option_Inner_RelaxTry = EditorPrefs.GetInt("AnyPortrait_MeshAutoV2_Inner_RelaxTry", 5);
			//_meshAutoGenV2Option_Inner_RelaxIntensity = EditorPrefs.GetFloat("AnyPortrait_MeshAutoV2_Inner_RelaxIntensity", 0.2f);
			_meshAutoGenV2Option_OuterMargin = EditorPrefs.GetInt("AnyPortrait_MeshAutoV2_OuterMargin", 10);
			_meshAutoGenV2Option_InnerMargin = EditorPrefs.GetInt("AnyPortrait_MeshAutoV2_InnerMargin", 5);
			_meshAutoGenV2Option_IsInnerMargin = EditorPrefs.GetBool("AnyPortrait_MeshAutoV2_IsInnerMargin", false);
			_meshAutoGenV2Option_QuickPresetType = EditorPrefs.GetInt("AnyPortrait_MeshAutoV2_QuickPresetType", 0);

			_exModObjOption_UpdateByOtherMod = EditorPrefs.GetBool("AnyPortrait_ExModObjOption_UpdateByOtherMod", DefaultExModObjOption_UpdateByOtherMod);
			_exModObjOption_ShowGray = EditorPrefs.GetBool("AnyPortrait_ExModObjOption_ShowGray", DefaultExModObjOption_ShowGray);
			_exModObjOption_NotSelectable = EditorPrefs.GetBool("AnyPortrait_ExModObjOption_NotSelectable", DefaultExModObjOption_NotSelectable);
			


			_isLowCPUOption = EditorPrefs.GetBool("AnyPortrait_LowCPUOption", DefaultLowCPUOption);

			_isSelectionLockOption_RiggingPhysics = EditorPrefs.GetBool("AnyPortrait_SelectionLockOption_RiggingPhysics", DefaultSelectionLockOption_RiggingPhysics);
			_isSelectionLockOption_Morph = EditorPrefs.GetBool("AnyPortrait_SelectionLockOption_Morph", DefaultSelectionLockOption_Morph);
			_isSelectionLockOption_Transform = EditorPrefs.GetBool("AnyPortrait_SelectionLockOption_Transform", DefaultSelectionLockOption_Transform);
			_isSelectionLockOption_ControlParamTimeline = EditorPrefs.GetBool("AnyPortrait_SelectionLockOption_ControlParamTimeline", DefaultSelectionLockOption_ControlParamTimeline);
			
			_hierarchySortMode = (HIERARCHY_SORT_MODE)EditorPrefs.GetInt("AnyPortrait_HierachySortMode", (int)HIERARCHY_SORT_MODE.RegOrder);

			_isAmbientCorrectionOption = EditorPrefs.GetBool("AnyPortrait_AmbientCorrectionOption", DefaultAmbientCorrectionOption);

			_isAutoSwitchControllerTab_Mod = EditorPrefs.GetBool("AnyPortrait_AutoSwitchControllerTab_Mod", DefaultAutoSwitchControllerTab_Mod);
			_isAutoSwitchControllerTab_Anim = EditorPrefs.GetBool("AnyPortrait_AutoSwitchControllerTab_Anim", DefaultAutoSwitchControllerTab_Anim);

			_isRestoreTempMeshVisibilityWhenTaskEnded = EditorPrefs.GetBool("AnyPortrait_RestoreTempMeshVisbility", DefaultRestoreTempMeshVisibiilityWhenTaskEnded);

			_rigViewOption_WeightOnly = EditorPrefs.GetBool("AnyPortrait_RigView_WeightOnly", false);
			_rigViewOption_BoneColor = EditorPrefs.GetBool("AnyPortrait_RigView_BoneColor", true);
			_rigViewOption_CircleVert = EditorPrefs.GetBool("AnyPortrait_RigView_CircleVert", false);

			//_rigOption_NewChildBoneColorIsLikeParent = EditorPrefs.GetBool("AnyPortrait_RigOption_ColorLikeParent", true);//>>_boneGUIOption_NewBoneColor 변수로 변경

			_macOSXInfoScreenOption_IsShowStartup = EditorPrefs.GetBool("AnyPortrait_MacOSXOption_ShowStartup", true);
			_macOSXInfoScreenOption_LastMonth = EditorPrefs.GetInt("AnyPortrait_MacOSXOption_LastMonth", 0);
			_macOSXInfoScreenOption_LastDay = EditorPrefs.GetInt("AnyPortrait_MacOSXOption_LastDay", 0);

			_vertGUIOption_SizeRatio_Index = EditorPrefs.GetInt("AnyPortrait_VertGUI_SizeRatio_Index", DefaultVertGUIOption_SizeRatio_Index);
			
			_boneGUIOption_RenderType = (BONE_DISPLAY_METHOD)EditorPrefs.GetInt("AnyPortrait_BoneGUI_RenderType", (int)DefaultBoneGUIOption_RenderType);
			_boneGUIOption_SizeRatio_Index = EditorPrefs.GetInt("AnyPortrait_BoneGUI_SizeRatio_Index", DefaultBoneGUIOption_SizeRatio_Index);
			_boneGUIOption_ScaledByZoom = EditorPrefs.GetBool("AnyPortrait_BoneGUI_ScaledByZoom", DefaultBoneGUIOption_ScaedByZoom);
			_boneGUIOption_NewBoneColor = (EditorPrefs.GetBool("AnyPortrait_RigOption_ColorLikeParent", DefaultBoneGUIOption_NewBoneColor_Bool) ? NEW_BONE_COLOR.SimilarColor : NEW_BONE_COLOR.DifferentColor);
			_boneGUIOption_NewBonePreview = (NEW_BONE_PREVIEW)EditorPrefs.GetInt("AnyPortrait_BoneGUI_NewBonePreview", (int)DefaultBoneGUIOption_NewBonePreview);

			_rigGUIOption_VertRatio_Index = EditorPrefs.GetInt("AnyPortrait_RigGUI_SizeRatio_Index", DefaultRigGUIOption_VertRatio_Index);
			_rigGUIOption_ScaledByZoom = EditorPrefs.GetBool("AnyPortrait_RigGUI_ScaledByZoom", DefaultRigGUIOption_ScaledByZoom);
			_rigGUIOption_VertRatio_Selected_Index = EditorPrefs.GetInt("AnyPortrait_RigGUI_SizeRatioSelected_Index", DefaultRigGUIOption_VertRatio_Selected_Index);
			_rigGUIOption_SelectedWeightGUIType = (RIG_SELECTED_WEIGHT_GUI_TYPE)EditorPrefs.GetInt("AnyPortrait_RigGUI_SelectedWeightGUIType", (int)DefaultRigGUIOption_SelectedWeightGUIType);
			_rigGUIOption_NoLinkedBoneVisibility = (NOLINKED_BONE_VISIBILITY)EditorPrefs.GetInt("AnyPortrait_RigGUI_NolinkedBoneVisibility", (int)DefaultRigGUIOption_NoLinkedBoneVisibility);
			_rigGUIOption_WeightGradientColor = (RIG_WEIGHT_GRADIENT_COLOR)EditorPrefs.GetInt("AnyPortrait_RigGUI_WeightGradientColor", (int)DefaultRigGUIOption_WeightGradientColor);

			

			apGL.SetToneOption(_colorOption_OnionToneColor, _onionOption_OutlineThickness, _onionOption_IsOutlineRender, _onionOption_PosOffsetX, _onionOption_PosOffsetY, _colorOption_OnionBoneColor);
			
			apGL.SetRiggingOption(	RigGUIOption_SizeRatioX100, 
									RigGUIOption_SizeRatioX100_Selected, 
									_rigGUIOption_ScaledByZoom,
									_rigGUIOption_SelectedWeightGUIType,
									_rigGUIOption_WeightGradientColor);

			_isNeedToAskRemoveVertByPSDImport = EditorPrefs.GetBool("AnyPortrait_NeedToAskRemoveVertByPSDImport", false);
			_option_ShowPrevViewMenuBtns = EditorPrefs.GetBool("AnyPortrait_ShowPrevViewMenuBtns", false);

			_option_SetAutoImageToMeshIfOnlyOneImageExist = EditorPrefs.GetBool("AnyPortrait_SetAutoImageToMeshIfOnlyOneImageExist", true);

			_option_IsTurnOffAnimAutoKey = EditorPrefs.GetBool("AnyPortrait_IsTurnOffAnimAutoKey", true);

			_guiOption_isShowHowToEdit = EditorPrefs.GetBool("AnyPortrait_ShowHowToEdit", true);

			_cppPluginOption_UsePlugin = EditorPrefs.GetBool("AnyPortrait_UseCPPPlugin", false);

			_isShowURPWarningMsg = EditorPrefs.GetBool("AnyPortrait_IsShowURPWarningMsg", true);
			_option_CheckSRPWhenBake = EditorPrefs.GetBool("AnyPortrait_IsCheckSRPWhenBake", true);

			_pinOption_AutoWeightRefresh = EditorPrefs.GetBool("AnyPortrait_PinOption_AutoWeightRefresh", true);

			_controlParamUISizeOption = (CONTROL_PARAM_UI_SIZE_OPTION)EditorPrefs.GetInt("AnyPortrait_ControlParamUI_Size_Option", (int)CONTROL_PARAM_UI_SIZE_OPTION.Default);

			_option_TurnOffVisibilityPresetWhenSelectObject = EditorPrefs.GetBool("AnyPortrait_VisibilityPresetOption_TurnOff", false);

			_option_AutoScrollWhenObjectSelected = EditorPrefs.GetBool("AnyPortrait_AutoScrollListObjSelected", true);

			_option_ObjMovableWithoutClickGizmo = EditorPrefs.GetBool("AnyPortrait_ObjMovableWithoutClickGizmo", true);
			
		}

		private void SaveColorPref(string label, Color color, Color defaultValue)
		{
			//이전
			//EditorPrefs.SetFloat(label + "_R", color.r);
			//EditorPrefs.SetFloat(label + "_G", color.g);
			//EditorPrefs.SetFloat(label + "_B", color.b);
			//EditorPrefs.SetFloat(label + "_A", color.a);

			//변경 21.2.10
			SavePref_Float(label + "_R", color.r, defaultValue.r);
			SavePref_Float(label + "_G", color.g, defaultValue.g);
			SavePref_Float(label + "_B", color.b, defaultValue.b);
			SavePref_Float(label + "_A", color.a, defaultValue.a);
		}

		private Color LoadColorPref(string label, Color defaultValue)
		{
			Color result = Color.black;
			result.r = EditorPrefs.GetFloat(label + "_R", defaultValue.r);
			result.g = EditorPrefs.GetFloat(label + "_G", defaultValue.g);
			result.b = EditorPrefs.GetFloat(label + "_B", defaultValue.b);
			result.a = EditorPrefs.GetFloat(label + "_A", defaultValue.a);
			return result;
		}

		//추가 21.2.10
		//Pref를 저장할 때, 기본값과 동일하다면 저장값을 삭제하자. 그동안 너무 많은 옵션을 사용했던 것 같다. Get은 상관없다.
		private void SavePref_Bool(string key, bool curValue, bool defaultValue)
		{
			if(curValue == defaultValue)	{ EditorPrefs.DeleteKey(key); }
			else							{ EditorPrefs.SetBool(key, curValue); }
		}

		private void SavePref_Int(string key, int curValue, int defaultValue)
		{
			if(curValue == defaultValue)	{ EditorPrefs.DeleteKey(key); }
			else							{ EditorPrefs.SetInt(key, curValue); }
		}

		//Float는 아주 작은 bias를 기준으로 한다.
		private void SavePref_Float(string key, float curValue, float defaultValue)
		{
			if(Mathf.Abs(curValue - defaultValue) < 0.0001f)	{ EditorPrefs.DeleteKey(key); }
			else												{ EditorPrefs.SetFloat(key, curValue); }
		}

		private void SavePref_String(string key, string curValue, string defaultValue)
		{
			if(string.Equals(curValue, defaultValue))	{ EditorPrefs.DeleteKey(key); }
			else										{ EditorPrefs.SetString(key, curValue); }
		}




		public void RestoreEditorPref()
		{
			_language = DefaultLanguage;

			//색상 옵션
			_colorOption_Background = DefaultColor_Background;
			_colorOption_GridCenter = DefaultColor_GridCenter;
			_colorOption_Grid = DefaultColor_Grid;
			_colorOption_InvertedBackground = DefaultColor_InvertedBackground;//추가 21.10.6

			_colorOption_MeshEdge = DefaultColor_MeshEdge;
			_colorOption_MeshHiddenEdge = DefaultColor_MeshHiddenEdge;
			_colorOption_Outline = DefaultColor_Outline;
			_colorOption_TransformBorder = DefaultColor_TransformBorder;

			_colorOption_VertColor_NotSelected = DefaultColor_VertNotSelected;
			_colorOption_VertColor_Selected = DefaultColor_VertSelected;

			_colorOption_GizmoFFDLine = DefaultColor_GizmoFFDLine;
			_colorOption_GizmoFFDInnerLine = DefaultColor_GizmoFFDInnerLine;
			//_colorOption_OnionToneColor = DefaultColor_OnionToneColor;//<<이 값은 Onion Setting에서 설정하는 것으로 변경

			_colorOption_AtlasBorder = DefaultColor_AtlasBorder;

			_guiOption_isFPSVisible = DefaultGUIOption_ShowFPS;
			_guiOption_isStatisticsVisible = DefaultGUIOption_ShowStatistics;

			

			_backupOption_IsAutoSave = DefaultBackupOption_IsAutoSave;//자동 백업을 지원하는가
			_backupOption_BaseFolderName = DefaultBackupOption_BaseFolderName;//"AnyPortraitBackup";//폴더를 지정해야한다. (프로젝트 폴더 기준 + 씬이름+에셋)
			_backupOption_Minute = DefaultBackupOption_Minute;//기본은 30분마다 한번씩 저장한다.

			_bonePose_BaseFolderName = DefaultBonePoseOption_BaseFolderName;//"AnyPortraitBonePose";

			_startScreenOption_IsShowStartup = DefaultStartScreenOption_IsShowStartup;
			_startScreenOption_LastMonth = 0;//Restore하면 다음에 실행할때 다시 나오도록 하자
			_startScreenOption_LastDay = 0;

			_isCheckLiveVersion_Option = DefaultCheckLiverVersionOption;
			
			_isLowCPUOption = DefaultLowCPUOption;

			//여기서 복구하지 않음
			//_isSelectionLockOption_RiggingPhysics = DefaultSelectionLockOption_RiggingPhysics;
			//_isSelectionLockOption_Morph = DefaultSelectionLockOption_Morph;
			//_isSelectionLockOption_Transform = DefaultSelectionLockOption_Transform;
			//_isSelectionLockOption_ControlParamTimeline = DefaultSelectionLockOption_ControlParamTimeline;
			
			_isAmbientCorrectionOption = DefaultAmbientCorrectionOption;

			_isAutoSwitchControllerTab_Mod = DefaultAutoSwitchControllerTab_Mod;
			_isAutoSwitchControllerTab_Anim = DefaultAutoSwitchControllerTab_Anim;

			_isRestoreTempMeshVisibilityWhenTaskEnded = DefaultRestoreTempMeshVisibiilityWhenTaskEnded;

			//이건 복구하지 않는다.
			//_rigViewOption_WeightOnly = false;
			//_rigViewOption_BoneColor = true;
			//_rigViewOption_CircleVert = false;

			//_rigOption_NewChildBoneColorIsLikeParent = true;//>>_boneGUIOption_NewBoneColor 변수로 변경

			_macOSXInfoScreenOption_IsShowStartup = true;
			_macOSXInfoScreenOption_LastMonth = 0;
			_macOSXInfoScreenOption_LastDay = 0;

			_vertGUIOption_SizeRatio_Index = DefaultVertGUIOption_SizeRatio_Index;

			_boneGUIOption_RenderType = DefaultBoneGUIOption_RenderType;
			_boneGUIOption_SizeRatio_Index = DefaultBoneGUIOption_SizeRatio_Index;
			_boneGUIOption_ScaledByZoom = DefaultBoneGUIOption_ScaedByZoom;
			_boneGUIOption_NewBoneColor = DefaultBoneGUIOption_NewBoneColor;
			_boneGUIOption_NewBonePreview = DefaultBoneGUIOption_NewBonePreview;

			_rigGUIOption_VertRatio_Index = DefaultRigGUIOption_VertRatio_Index;
			_rigGUIOption_ScaledByZoom = DefaultRigGUIOption_ScaledByZoom;
			_rigGUIOption_VertRatio_Selected_Index = DefaultRigGUIOption_VertRatio_Selected_Index;
			_rigGUIOption_SelectedWeightGUIType = DefaultRigGUIOption_SelectedWeightGUIType;
			//_rigGUIOption_NoLinkedBoneVisibility = DefaultRigGUIOption_NoLinkedBoneVisibility;//이것도 복구하지 않는다. (작업내 기능임)
			_rigGUIOption_WeightGradientColor = DefaultRigGUIOption_WeightGradientColor;

			_isNeedToAskRemoveVertByPSDImport = DefaultNeedToAskRemoveVertByPSDImport;
			_option_ShowPrevViewMenuBtns = DefaultShowPrevViewMenuBtns;

			_option_SetAutoImageToMeshIfOnlyOneImageExist = DefaultSetAutoImageToMeshIfOnlyOneImageExist;

			_option_IsTurnOffAnimAutoKey = DefaultIsTurnOffAnimAutoKey;


			_isShowURPWarningMsg = DefaultShowURPWarningMsg;
			_option_CheckSRPWhenBake = DefaultCheckSRPWhenBake;

			_pinOption_AutoWeightRefresh = DefaultPinOptionAutoWeightRefresh;

			_controlParamUISizeOption = DefaultControlParamUISizeOption;

			_option_TurnOffVisibilityPresetWhenSelectObject = DefaultVisibilityTurnOffOption;

			_option_AutoScrollWhenObjectSelected = DefaultAutoScrollListWhenObjSelected;

			_option_ObjMovableWithoutClickGizmo = DefaultObjMovableWithoutClickGizmo;


			//버전 체크 무시도 해제
			_isVersionNoticeIgnored = false;
			_versionNoticeIgnored_Ver = -1;

			SaveEditorPref();
		}

		public static LANGUAGE DefaultLanguage { get { return LANGUAGE.English; } }

		public static bool DefaultGUIOption_ShowFPS { get { return true; } }
		public static bool DefaultGUIOption_ShowStatistics { get { return false; } }
		public static bool DefaultEditorOption_UsePlugin { get { return false; } }

		public static bool DefaultBackupOption_IsAutoSave { get { return true; } }//자동 백업을 지원하는가
		public const string DefaultBackupOption_BaseFolderName = "AnyPortraitBackup"; //백업 폴더
		public static int DefaultBackupOption_Minute { get { return 30; } }//기본은 30분마다 한번씩 저장한다.

		public const string DefaultBonePoseOption_BaseFolderName = "AnyPortraitBonePose";


		public static Color DefaultColor_Background { get { return new Color(0.2f, 0.2f, 0.2f, 1.0f); } }
		public static Color DefaultColor_GridCenter { get { return new Color(0.7f, 0.7f, 0.3f, 1.0f); } }
		public static Color DefaultColor_Grid { get { return new Color(0.3f, 0.3f, 0.3f, 1.0f); } }

		public static Color DefaultColor_InvertedBackground { get { return new Color(0.9f, 0.9f, 0.9f, 1.0f); } }

		public static Color DefaultColor_MeshEdge { get { return new Color(1.0f, 0.5f, 0.0f, 0.9f); } }
		public static Color DefaultColor_MeshHiddenEdge { get { return new Color(1.0f, 1.0f, 0.0f, 0.7f); } }
		public static Color DefaultColor_Outline { get { return new Color(0.0f, 0.5f, 1.0f, 0.7f); } }
		public static Color DefaultColor_TransformBorder { get { return new Color(0.0f, 1.0f, 1.0f, 1.0f); } }

		public static Color DefaultColor_VertNotSelected { get { return new Color(0.0f, 0.85f, 1.0f, 0.95f); } }//이전 색상 : (0.0f, 0.3f, 1.0f, 0.6f)
		public static Color DefaultColor_VertSelected { get { return new Color(1.0f, 0.0f, 0.0f, 1.0f); } }

		public static Color DefaultColor_GizmoFFDLine { get { return new Color(1.0f, 0.5f, 0.2f, 0.9f); } }
		public static Color DefaultColor_GizmoFFDInnerLine { get { return new Color(1.0f, 0.7f, 0.2f, 0.7f); } }

		public static Color DefaultColor_OnionToneColor { get { return new Color(0.1f, 0.43f, 0.5f, 0.7f); } }
		public static Color DefaultColor_OnionAnimPrevColor { get { return new Color(0.5f, 0.2f, 0.1f, 0.7f); } }
		public static Color DefaultColor_OnionAnimNextColor { get { return new Color(0.1f, 0.5f, 0.2f, 0.7f); } }

		public static Color DefaultColor_OnionBoneColor { get { return new Color(0.4f, 1.0f, 1.0f, 0.9f); } }
		public static Color DefaultColor_OnionBonePrevColor { get { return new Color(1.0f, 0.6f, 0.3f, 0.9f); } }
		public static Color DefaultColor_OnionBoneNextColor { get { return new Color(0.3f, 1.0f, 0.6f, 0.9f); } }

		public static Color DefaultColor_AtlasBorder { get { return new Color(0.0f, 1.0f, 1.0f, 0.5f); } }

		public static Color DefauleColor_ModLockOpt_MeshPreview { get { return new Color(1.0f, 0.45f, 0.1f, 0.8f); } }
		public static Color DefauleColor_ModLockOpt_BonePreview { get { return new Color(1.0f, 0.8f, 0.1f, 0.8f); } }


		public static int DefaultVertGUIOption_SizeRatio_Index { get { return VERT_GUI_SIZE_INDEX__DEFAULT; } }

		public static BONE_DISPLAY_METHOD DefaultBoneGUIOption_RenderType { get { return BONE_DISPLAY_METHOD.Version2; } }
		public static int DefaultBoneGUIOption_SizeRatio_Index { get { return BONE_RIG_SIZE_INDEX__DEFAULT; } }
		public static bool DefaultBoneGUIOption_ScaedByZoom { get { return false; } }
		public static bool DefaultBoneGUIOption_NewBoneColor_Bool { get { return true; } }
		public static NEW_BONE_COLOR DefaultBoneGUIOption_NewBoneColor { get { return NEW_BONE_COLOR.SimilarColor; } }
		public static NEW_BONE_PREVIEW DefaultBoneGUIOption_NewBonePreview { get { return NEW_BONE_PREVIEW.Line; } }

		public static int DefaultRigGUIOption_VertRatio_Index { get { return BONE_RIG_SIZE_INDEX__DEFAULT; } }
		public static bool DefaultRigGUIOption_ScaledByZoom { get { return false; } }
		public static int DefaultRigGUIOption_VertRatio_Selected_Index { get { return BONE_RIG_SIZE_INDEX__DEFAULT_SELECTED; } }
		public static RIG_SELECTED_WEIGHT_GUI_TYPE DefaultRigGUIOption_SelectedWeightGUIType { get { return RIG_SELECTED_WEIGHT_GUI_TYPE.EnlargedAndFlashing; } }
		public static NOLINKED_BONE_VISIBILITY DefaultRigGUIOption_NoLinkedBoneVisibility {  get {  return NOLINKED_BONE_VISIBILITY.Translucent; } }
		public static RIG_WEIGHT_GRADIENT_COLOR DefaultRigGUIOption_WeightGradientColor { get { return RIG_WEIGHT_GRADIENT_COLOR.Default; } }

		public static bool DefaultStartScreenOption_IsShowStartup { get { return true; } }
		public static bool DefaultCheckLiverVersionOption { get { return true; } }
		public static bool DefaultLowCPUOption { get {  return false;} }

		public static bool DefaultAmbientCorrectionOption { get { return true; } }

		public static bool DefaultAutoSwitchControllerTab_Mod { get { return true; } }
		public static bool DefaultAutoSwitchControllerTab_Anim { get { return false; } }
		public static bool DefaultRestoreTempMeshVisibiilityWhenTaskEnded { get { return true; } }

		public static bool DefaultNeedToAskRemoveVertByPSDImport { get { return false; } }
		public static bool DefaultShowPrevViewMenuBtns { get { return false; } }

		public static bool DefaultSetAutoImageToMeshIfOnlyOneImageExist { get { return true; } }
		public static bool DefaultIsTurnOffAnimAutoKey { get { return true; } }


		public static bool DefaultSelectionLockOption_RiggingPhysics { get { return true; } }
		public static bool DefaultSelectionLockOption_Morph { get { return true; } }
		public static bool DefaultSelectionLockOption_Transform { get { return true; } }
		public static bool DefaultSelectionLockOption_ControlParamTimeline { get { return true; } }

		public static bool DefaultExModObjOption_UpdateByOtherMod { get { return false; } }
		public static bool DefaultExModObjOption_ShowGray { get { return true; } }
		public static bool DefaultExModObjOption_NotSelectable { get { return false; } }

		public static bool DefaultShowURPWarningMsg { get { return true; } }
		public static bool DefaultCheckSRPWhenBake { get { return true; } }
		public static bool DefaultPinOptionAutoWeightRefresh { get { return true; } }

		public static CONTROL_PARAM_UI_SIZE_OPTION DefaultControlParamUISizeOption { get { return CONTROL_PARAM_UI_SIZE_OPTION.Default; } }
		public static bool DefaultVisibilityTurnOffOption { get { return false; } }

		public static bool DefaultAutoScrollListWhenObjSelected { get { return true; } }
		public static bool DefaultObjMovableWithoutClickGizmo { get { return true; } }





		//------------------------------------------------------------------------------
		// 로컬라이제이션
		//------------------------------------------------------------------------------
		public string GetText(TEXT textType)
		{
			return Localization.GetText(textType);
		}


		public string GetTextFormat(TEXT textType, params object[] paramList)
		{
			return string.Format(Localization.GetText(textType), paramList);
		}


		public string GetUIWord(UIWORD uiWordType)
		{
			return Localization.GetUIWord(uiWordType);
		}


		public string GetUIWordFormat(UIWORD uiWordType, params object[] paramList)
		{
			return string.Format(Localization.GetUIWord(uiWordType), paramList);
		}




		//-------------------------------------------------------------------------------
		// 화면 캡쳐 처리
		//-------------------------------------------------------------------------------
		// 화면 캡쳐 이벤트 요청과 처리
		//-----------------------------------------------------------------------
		public void ScreenCaptureRequest(apScreenCaptureRequest screenCaptureRequest)
		{
			
				
			
#if UNITY_EDITOR_OSX
			//OSX에선 딜레이를 줘야한다.
			_isScreenCaptureRequest_OSXReady = true;
			_isScreenCaptureRequest = false;
			_screenCaptureRequest_Count = SCREEN_CAPTURE_REQUEST_OSX_COUNT;
#else
			//Window에선 바로 캡쳐가능
			_isScreenCaptureRequest = true;
#endif
			
			_screenCaptureRequest = screenCaptureRequest;
		}




		//GUI에서 처리할 것
		private void ProcessScreenCapture()
		{
			_isScreenCaptureRequest = false;

			if(_screenCaptureRequest == null)
			{
				return;
			}

			apScreenCaptureRequest curRequest = _screenCaptureRequest;
			_screenCaptureRequest = null;//<<이건 일단 null

			//유효성 체크
			bool isValid = true;
			if (curRequest._editor != this ||
				Select.SelectionType != apSelection.SELECTION_TYPE.Overall ||
				Select.RootUnit == null ||
				curRequest._meshGroup == null ||
				Select.RootUnit._childMeshGroup != curRequest._meshGroup ||
				curRequest._funcCaptureResult == null ||
				_portrait == null)
			{
				isValid = false;
				
			}

			Texture2D result = null;
			if(isValid)
			{
				apMeshGroup targetMeshGroup = curRequest._meshGroup;
				apAnimClip targetAnimClip = curRequest._animClip;
				

				bool prevPhysics = _portrait._isPhysicsPlay_Editor;
				

				//업데이트를 한다.
				if(curRequest._isAnimClipRequest &&
					targetAnimClip != null)
				{
					//추가 20.7.9 : 업데이트 전에 물리 타이머 갱신할 것
					_portrait.SetPhysicsTimerWhenCapture(targetAnimClip.TimePerFrame);

					//Debug.LogWarning("Capture Frame : " + curRequest._animFrame);
					//_portrait._isPhysicsPlay_Editor = false;//<<물리 금지
					//변경 : 옵션에 따라 바뀐다.
					_portrait._isPhysicsPlay_Editor = curRequest._isPhysics;
					if(targetAnimClip._targetMeshGroup != null)
					{
						targetAnimClip._targetMeshGroup.SetBoneIKEnabled(true, true);
					}
					targetAnimClip.SetFrame_Editor(curRequest._animFrame);

					
				}
				else
				{
					//추가 20.7.9 : 업데이트 전에 물리 타이머 갱신할 것
					_portrait.CalculatePhysicsTimer();

					targetMeshGroup.SetBoneIKEnabled(true, true);
					targetMeshGroup.RefreshForce();
					
				}

				result = Exporter.RenderToTexture(
													targetMeshGroup,
													curRequest._winPosX,
													curRequest._winPosY,
													curRequest._srcSizeWidth,
													curRequest._srcSizeHeight,
													curRequest._dstSizeWidth,
													curRequest._dstSizeHeight,
													curRequest._clearColor
													);

				//물리 복구
				_portrait._isPhysicsPlay_Editor = prevPhysics;
				if (curRequest._isAnimClipRequest &&
					targetAnimClip != null)
				{
					if (targetAnimClip._targetMeshGroup != null)
					{
						targetAnimClip._targetMeshGroup.SetBoneIKEnabled(false, false);
					}
				}
				else
				{
					targetMeshGroup.SetBoneIKEnabled(false, false);
				}

				//리! 턴!
				try
				{
					if (result != null)
					{
						curRequest._funcCaptureResult(
													true,
													result,
													curRequest._iProcessStep,
													curRequest._filePath,
													curRequest._loadKey);
					}
					else
					{
						curRequest._funcCaptureResult(
													false,
													null,
													curRequest._iProcessStep,
													curRequest._filePath,
													curRequest._loadKey);
					}

					result = null;
				}
				catch (Exception ex)
				{
					Debug.LogError("Capture Exception : " + ex);

					if (result != null)
					{
						UnityEngine.Object.DestroyImmediate(result);
					}
				}
			}
			else
			{
				if(result != null)
				{
					UnityEngine.Object.DestroyImmediate(result);
				}
				
				//처리가 실패했다.
				if(curRequest != null &&
					curRequest._funcCaptureResult != null)
				{
					//리! 턴!
					try
					{
						curRequest._funcCaptureResult(
							false,
							null,
							curRequest._iProcessStep,
							curRequest._filePath,
							curRequest._loadKey);
					}
					catch(Exception ex)
					{
						Debug.LogError("Capture Exception : " + ex);
					}
				}
			}
				
			//처리 끝!
			

		}

		//----------------------------------------------------------------------------
		// 최신 버전 체크하기
		//----------------------------------------------------------------------------
		private void CheckCurrentLiveVersion()
		{
			if(!_isCheckLiveVersion)
			{
				_isCheckLiveVersion = true;

				if(!_isCheckLiveVersion_Option)
				{
					//만약 버전 체크 옵션이 꺼져있으면 처리하지 않는다.
					//Debug.Log("버전 체크를 하지 않는다.");
					return;
				}
				//else
				//{
				//	Debug.Log("버전 체크 시작");
				//}
				//날짜 확인후 요청
				if(string.IsNullOrEmpty(_currentLiveVersion)
					|| _lastCheckLiveVersion_Day != DateTime.Now.Day
					|| _lastCheckLiveVersion_Month != DateTime.Now.Month
					|| true//<<테스트
					)
				{
					//날짜가 다르거나, 버전이 없으면 요청
					apEditorUtil.RequestCurrentVersion(OnGetCurrentLiveVersion);
				}
				else
				{
					CheckCurrentVersionAndNotification(false);
				}
			}
		}

		private void OnGetCurrentLiveVersion(bool isSuccess, string strResult)
		{
			//Debug.Log("OnGetCurrentLiveVersion : " + isSuccess + " / " + strResult);
			if(isSuccess)
			{
				_currentLiveVersion = strResult;
				//업데이트 버전이 갱신되었다.
				//저장을 하자
				_lastCheckLiveVersion_Day = System.DateTime.Now.Day;
				_lastCheckLiveVersion_Month = System.DateTime.Now.Month;

				SaveEditorPref();

				CheckCurrentVersionAndNotification(true);

			}
		}

		private void CheckCurrentVersionAndNotification(bool isCalledByWebResponse)
		{
			if (string.IsNullOrEmpty(_currentLiveVersion))
			{
				return;
			}
			string strNumeric = "";
			string strText = "";
			for (int i = 0; i < _currentLiveVersion.Length; i++)
			{
				strText = _currentLiveVersion.Substring(i, 1);
				if (strText == "0" || strText == "1" || strText == "2" || strText == "3"
					|| strText == "4" || strText == "5" || strText == "6" || strText == "7"
					|| strText == "8" || strText == "9")
				{
					strNumeric += strText;
				}
			}

			//Debug.Log("[" + strNumeric + "]");
			int liveVersion = 0;
			bool isParse = int.TryParse(strNumeric, out liveVersion);
			if(!isParse)
			{
				//파싱 실패
				return;
			}
			if (liveVersion <= apVersion.I.APP_VERSION_INT)
			{
				//같거나 이전 버전이다.
				//Debug.Log("Last Version : " + _currentLiveVersion);
			}
			else
			{
				Notification("A new version has been updated! [v" + apVersion.I.APP_VERSION_SHORT + " >> v" + _currentLiveVersion + "]", false, false);

				//추가 : 웹에서 로드했으며, 알람을 줄 수 있는 상황이라면
				if(isCalledByWebResponse && Localization.IsLoaded)
				{
					//알람을 해야하는지 테스트
					bool isNoticeEnabled = false;

					if(!_isVersionNoticeIgnored)
					{
						//무시하기 상태가 아니다.
						isNoticeEnabled = true;
					}
					else
					{
						//이전 : 날짜를 기준으로 메시지가 강제로 나왔다.
						//if(_versionNoticeIvnored_Year == 0 || _versionNoticeIvnored_Month == 0 || _versionNoticeIvnored_Day == 0)
						//{
						//	//날짜가 잘못 되어 있다.
						//	isNoticeEnabled = true;
						//}
						//else
						//{
						//	DateTime ignoredDay = new DateTime(_versionNoticeIvnored_Year, _versionNoticeIvnored_Month, _versionNoticeIvnored_Day);
						//	int diffDays = (int)ignoredDay.Subtract(DateTime.Now).TotalDays;
						//	if(diffDays < 0)
						//	{
						//		isNoticeEnabled = true;
						//	}
						//	//else
						//	//{
						//	//	Debug.Log("날짜가 아직 되지 않았다. [오늘 : " + DateTime.Now.ToShortDateString() + "] - [목표 날짜:" + ignoredDay.ToShortDateString() + "] (" + diffDays + ")");
						//	//}
						//}

						//변경 22.7.1 : 무시하려는 버전 외에는 메시지가 항상 나온다.
						if(liveVersion != _versionNoticeIgnored_Ver)
						{
							isNoticeEnabled = true;
						}
					}
					if (isNoticeEnabled)
					{
						//이전
						//한번 메시지를 보면 최소 일주일은 무조건 보이지 않는다.
						//일주일간 무시하기 옵션이 사라진다.
						//_isVersionNoticeIgnored = false;

						//_versionNoticeIvnored_Year = DateTime.Now.Year;
						//_versionNoticeIvnored_Month = DateTime.Now.Month;
						//_versionNoticeIvnored_Day = DateTime.Now.Day;

						//무시하기 조건이 해제된다.
						_isVersionNoticeIgnored = false;

						SaveEditorPref();


						//바로 에셋 스토어를 열 수 있게 하자
						int iBtn = EditorUtility.DisplayDialogComplex(GetText(TEXT.DLG_NewVersion_Title),
																		GetText(TEXT.DLG_NewVersion_Body),
																		GetText(TEXT.DLG_NewVersion_OpenAssetStore),
																		GetText(TEXT.DLG_NewVersion_Ignore),
																		GetText(TEXT.Cancel)
																		);

						if (iBtn == 0)
						{
							apEditorUtil.OpenAssetStorePage();
						}
						else if (iBtn == 1)
						{
							//이전
							//일주일 동안 열지 않음
							//_isVersionNoticeIgnored = true;

							//DateTime dayAfter7 = DateTime.Now;
							//dayAfter7 = dayAfter7.AddDays(7);

							//_versionNoticeIvnored_Year = dayAfter7.Year;
							//_versionNoticeIvnored_Month = dayAfter7.Month;
							//_versionNoticeIvnored_Day = dayAfter7.Day;

							////Debug.Log("7일 동안 알람 안보기 : " + _versionNoticeIvnored_Year + "-" + _versionNoticeIvnored_Month + "-" + _versionNoticeIvnored_Day);
	
							//변경 22.7.1 : 현재 버전 스킵하기
							_isVersionNoticeIgnored = true;
							_versionNoticeIgnored_Ver = liveVersion;

							SaveEditorPref();
						}
					}
					
					

				}
				//GetText()
			}
		}



		//-------------------------------------------------------------------------------------
		// 로딩 팝업 (Progress UI) 요청
		//-------------------------------------------------------------------------------------
		/// <summary>
		///로딩 게이지 팝업을 보여준다.
		/// </summary>
		public void StartProgressPopup(string title, string info)
		{
			StartProgressPopup(title, info, false, null);
		}
		
		public void StartProgressPopup(string title, string info, bool isCancelable, FUNC_CANCEL_PROGRESS_POPUP funcCancelProgressPopup)
		{
			if(_isProgressPopup)
			{
				//이미 진행중이다.
				return;
			}
			_isProgressPopup_StartRequest = true;
			_proogressPopupRatio = 0.0f;
			_isProgressPopup_CompleteRequest = false;
			if(_strProgressPopup_Title == null)
			{
				_strProgressPopup_Title = new apStringWrapper(128);
			}
			if(_strProgressPopup_Info == null)
			{
				_strProgressPopup_Info = new apStringWrapper(128);
			}

			_isProogressPopup_Cancelable = isCancelable;
			_funcProgressPopupCancel = funcCancelProgressPopup;

			_strProgressPopup_Title.SetText(title);
			_strProgressPopup_Info.SetText(info);
		}

		/// <summary>
		/// 로딩 게이지 팝업의 값을 바꾸거나 종료한다.
		/// </summary>
		/// <param name="isComplete"></param>
		/// <param name="ratio">0.0~1.0 사이의 값</param>
		public void SetProgressPopupRatio(bool isComplete, float ratio)
		{
			if(!_isProgressPopup)
			{
				//팝업이 없다.
				return;
			}
			if(isComplete)
			{
				_isProgressPopup_CompleteRequest = true;
				_proogressPopupRatio = 1.0f;
			}
			else
			{
				_proogressPopupRatio = Mathf.Clamp01(ratio);
			}
		}

		/// <summary>
		/// OnGUI에 들어가는 함수. 로딩 팝업이 있는지 조건을 체크하고, 로딩 팝업을 보여준다.
		/// 이 동안은 다른 입력은 제한된다.(이건 자동이며, apGL/Gizmo의 입력 제한도 추가해야한다.)
		/// 만약 로딩 팝업이 보여지는 상태라면 true를 리턴한다.
		/// </summary>
		private bool CheckAndShowProgressPopup()
		{
			if (_isProgressPopup_CompleteRequest)
			{
				//종료 요청
				_isProgressPopup = false;
				EditorUtility.ClearProgressBar();

				_isProgressPopup_CompleteRequest = false;
				_isProgressPopup_StartRequest = false;
				_isProogressPopup_Cancelable = false;
				_funcProgressPopupCancel = null;
			}
			else if (_isProgressPopup_StartRequest)
			{
				//시작 요청
				_isProgressPopup = true;
				_proogressPopupRatio = 0.0f;

				_isProgressPopup_CompleteRequest = false;
				_isProgressPopup_StartRequest = false;
			}

			if (!_isProgressPopup)
			{
				return false;
			}

			if (_strProgressPopup_Title == null)
			{
				_strProgressPopup_Title = new apStringWrapper(128);
			}
			if (_strProgressPopup_Info == null)
			{
				_strProgressPopup_Info = new apStringWrapper(128);
			}

			//Progress Popup을 보여주자.
			//변경 21.1.4 : 취소 가능하면 다른걸 보여주자
			if(_isProogressPopup_Cancelable && _funcProgressPopupCancel != null)
			{
				if(EditorUtility.DisplayCancelableProgressBar(	_strProgressPopup_Title.ToString(),
																_strProgressPopup_Info.ToString(),
																_proogressPopupRatio))
				{
					//취소
					if(_funcProgressPopupCancel != null)
					{
						try
						{
							_funcProgressPopupCancel();
						}
						catch(Exception)
						{

						}
					}
					_isProogressPopup_Cancelable = false;
					_funcProgressPopupCancel = null;

					_isProgressPopup_CompleteRequest = true;
				}
			}
			else
			{
				EditorUtility.DisplayProgressBar(	_strProgressPopup_Title.ToString(),
													_strProgressPopup_Info.ToString(),
													_proogressPopupRatio);
			}
			
			
			
			
			//입력은 무시하도록 하자.
			//Layout / Repaint 이벤트 외에는 모두 Use 처리
			if (Event.current.type != EventType.Layout
				&& Event.current.type != EventType.Repaint)
			{
				Event.current.Use();
			}

			return true;
		}


		//-----------------------------------------------------------------------------------
		// Portrait 비동기 로딩
		//-----------------------------------------------------------------------------------
		/// <summary>
		/// 비동기 로딩중인 Portrait나 함수가 있다면 종료한다.
		/// </summary>
		private void ClearLoadingPortraitAsync()
		{
			_asyncLoading_Portrait = null;
			_asyncLoading_Coroutine = null;
			_asyncLoading_CoroutineTimer = null;
			EditorApplication.update -= ExecuteCoroutine_LoadingPortraitAsync;
		}



		private bool LoadPortraitAsync(apPortrait targetPortrait)
		{
			if(targetPortrait == null)
			{
				//Portrait가 없다.
				return false;
			}
			if(_asyncLoading_Portrait != null)
			{
				//처리 중이면 실패
				return false;
			}
			_asyncLoading_Portrait = targetPortrait;
			_asyncLoading_Coroutine = Crt_LoadingPortraitAsync();
			_asyncLoading_CoroutineTimer = null;

			EditorApplication.update -= ExecuteCoroutine_LoadingPortraitAsync;
			EditorApplication.update += ExecuteCoroutine_LoadingPortraitAsync;

			StartProgressPopup("Loading [" + targetPortrait.name + "]", "Loading...");

			return true;
		}


		private void ExecuteCoroutine_LoadingPortraitAsync()
		{
			if(_asyncLoading_Coroutine == null)
			{
				//예상치 못한 종료
				ClearLoadingPortraitAsync();
				SetProgressPopupRatio(true, 1.0f);//ProgressBar도 종료한다.
				return;
			}

			bool isResult = _asyncLoading_Coroutine.MoveNext();

			if(!isResult)
			{
				//종료!
				//Debug.LogError("<종료!>");
				ClearLoadingPortraitAsync();
				SetProgressPopupRatio(true, 1.0f);//ProgressBar도 종료한다.
				return;
			}
		}

		private IEnumerator Crt_LoadingPortraitAsync()
		{
			yield return false;

			if(_asyncLoading_Portrait == null)
			{
				yield break;
			}

			//하나씩 로딩을 해보자
			//각 단계별로 Ratio가 정해진다.
			

			//1> 간단히 Material Set 설정
			Controller.LinkMaterialSets(_asyncLoading_Portrait);
			SetProgressPopupRatio(false, 0.05f);
			//Debug.Log("Async > 1");
			yield return false;
			
			//2> Ready To Edit들 하나씩 배치
			//(1)
			_asyncLoading_Portrait.ReadyToEdit_Step1();
			SetProgressPopupRatio(false, 0.1f);
			//Debug.Log("Async > 2");
			yield return false;

			//(2)
			_asyncLoading_Portrait.ReadyToEdit_Step2();
			SetProgressPopupRatio(false, 0.2f);
			//Debug.Log("Async > 3");
			yield return false;

			//(3)
			_asyncLoading_Portrait.ReadyToEdit_Step3();
			SetProgressPopupRatio(false, 0.3f);
			//Debug.Log("Async > 4");
			yield return false;

			//(4)
			_asyncLoading_Portrait.ReadyToEdit_Step4();
			SetProgressPopupRatio(false, 0.4f);
			//Debug.Log("Async > 5");
			yield return false;

			//(5)
			_asyncLoading_Portrait.ReadyToEdit_Step5();
			SetProgressPopupRatio(false, 0.5f);
			//Debug.Log("Async > 6");
			yield return false;

			//(6)
			_asyncLoading_Portrait.ReadyToEdit_Step6();
			SetProgressPopupRatio(false, 0.6f);
			//Debug.Log("Async > 7");
			yield return false;

			//(7)
			_asyncLoading_Portrait.ReadyToEdit_Step7();
			SetProgressPopupRatio(false, 0.7f);
			//Debug.Log("Async > 8");
			yield return false;

			//(8)
			_asyncLoading_Portrait.ReadyToEdit_Step8();
			SetProgressPopupRatio(false, 0.8f);
			//Debug.Log("Async > 9");
			yield return false;

			//(9)
			_asyncLoading_Portrait.ReadyToEdit_Step9();
			SetProgressPopupRatio(false, 0.9f);
			//Debug.Log("Async > 10");
			yield return false;


			//3> 마무리 작업들
			_portrait = _asyncLoading_Portrait;
			_selection.SelectPortrait(_portrait);

			Controller.PortraitReadyToEdit_AsyncStep();
			Selection.activeGameObject = null;
			_selection.SelectRootUnitDefault();
			OnAnyObjectAddedOrRemoved();

			SyncHierarchyOrders();

			_hierarchy.ResetAllUnits();
			_hierarchy_MeshGroup.ResetSubUnits();
			_hierarchy_AnimClip.ResetSubUnits();

			SetProgressPopupRatio(true, 1.0f);

			
			//Selection, Controller에 Editor.Portrait를 참조해선 안된다. (독립적으로 수행되어야 함)
			//_portrait = nextPortrait;
			//					Controller.InitTmpValues();
			//					_selection.SetPortrait(_portrait);

			//					//Portrait의 레퍼런스들을 연결해주자
			//					Controller.PortraitReadyToEdit();//화면 좌상단 오브젝트 필드에서 교체할 때 <편집 중인 Portrait가 있을때 교체>




			//					//Selection.activeGameObject = _portrait.gameObject;
			//					Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			//					//시작은 RootUnit
			//					_selection.SetOverallDefault();

			//					OnAnyObjectAddedOrRemoved();

		}

		private bool WaitAsyncTime_LoadingPortraitAsync(float sec)
		{
			if(_asyncLoading_CoroutineTimer == null)
			{
				_asyncLoading_CoroutineTimer = new System.Diagnostics.Stopwatch();
				_asyncLoading_CoroutineTimer.Stop();
				_asyncLoading_CoroutineTimer.Reset();
				_asyncLoading_CoroutineTimer.Start();
				return false;
			}

			if(_asyncLoading_CoroutineTimer.Elapsed.TotalSeconds > sec)
			{
				_asyncLoading_CoroutineTimer.Stop();
				_asyncLoading_CoroutineTimer = null;
				return true;
			}
			return false;

		}


		//---------------------------------------------------------------------------------
		// Hierachy에 맞게 자동으로 스크롤
		//---------------------------------------------------------------------------------
		//공통 알고리즘
		//1. 이 오브젝트에 해당하는 Hierarchy Unit을 찾는다.
		//2. Nested Fold가 되었다면 모두 해제한다.
		//3. 해당 오브젝트의 PosY를 구한다.
		// > 2에 해당한다면 Last Pos Y를 사용할 수 없으므로, 다시 계산한다.
		// > 2에 해당하지 않는다면, "마지막 렌더링 위치"를 이용한다.
		//4. 렌더링 위치에 맞게 스크롤을 수정한다.


		/// <summary>
		/// Hierarchy의 스크롤을 현재 커서에 맞게 자동으로 이동시킨다.
		/// 선택된 Unit 또는 부모가 Fold되어 있으면 Fold를 해제한다.
		/// </summary>
		/// <param name="cursorObj">대상이 되는 Hierarchy의 객체</param>
		/// <param name="isForceRecalculateUnitPosY">이 값이 True라면 Unit들의 Y 위치를 다시 갱신한다. 한번이라도 화면에 그려진 이후엔 상관이 없으나, 객체 생성 직후엔 true를 입력하자</param>
		public void AutoScroll_HierarchyMain(object cursorObj, bool isForceRecalculateUnitPosY = false)
		{
			if(cursorObj == null)
			{
				return;
			}

			//스크롤하기에 윈도우 Height가 너무 작다면 패스
			//UI가 접혀있어도 패스
			if(_lastWindowHeight < 10
				|| _uiFoldType_Left == UI_FOLD_TYPE.Folded)
			{
				return;
			}
			
			//1. Hierarchy Unit 찾기
			apEditorHierarchyUnit targetUnit = Hierarchy.FindUnitByObject(cursorObj);
			if(targetUnit == null)
			{
				//유닛을 찾지 못했다.
				return;
			}

			bool isAnimMenu = false;//애니메이션이 선택되어 있다면 하단에 타임라인이 나오면서 스크롤이 짧아진다.

			//Hierarchy 필터가 꺼져있다면 켜야한다.
			apEditorHierarchy.CATEGORY category =(apEditorHierarchy.CATEGORY)targetUnit._savedKey;
			switch (category)
			{
				case apEditorHierarchy.CATEGORY.Overall_Item:	SetHierarchyFilter(HIERARCHY_FILTER.RootUnit, true); break;
				case apEditorHierarchy.CATEGORY.Images_Item:	SetHierarchyFilter(HIERARCHY_FILTER.Image, true); break;
				case apEditorHierarchy.CATEGORY.Mesh_Item:		SetHierarchyFilter(HIERARCHY_FILTER.Mesh, true); break;
				case apEditorHierarchy.CATEGORY.MeshGroup_Item:	SetHierarchyFilter(HIERARCHY_FILTER.MeshGroup, true); break;
				case apEditorHierarchy.CATEGORY.Animation_Item:	SetHierarchyFilter(HIERARCHY_FILTER.Animation, true); isAnimMenu = true; break;
				case apEditorHierarchy.CATEGORY.Param_Item:		SetHierarchyFilter(HIERARCHY_FILTER.MeshGroup, true); break;
			}

			//2. Nest Fold 여부 및 해제를 시도한다.
			bool isUnfolded = targetUnit.UnfoldAllParent();

			//3. 현재 유닛의 PosY를 구하자
			int unitPosY = 0;
			if(!isUnfolded && !isForceRecalculateUnitPosY)
			{
				//접혀져있지 않았다면 > 마지막 PosY를 이용하자
				unitPosY = targetUnit.GetLastPosY();
			}
			else
			{
				//접혀져있었다면 > PosY를 다시 계산해야한다.
				bool isFindUnitInHierarchy = false;
				unitPosY = Hierarchy.CalculateUnitPosY(targetUnit, _hierarchyFilter, out isFindUnitInHierarchy);
				if(!isFindUnitInHierarchy)
				{
					//만약 해당 유닛을 Hierarchy에서 찾지 못했다면 스크롤 포기
					return;
				}
			}

			//해당 유닛이 화면상에 보이도록 스크롤을 조절하자
			// MainHeight를 계산하자
			// (오브젝트 변경시 Modifier 탭이 해제되면서 편집 모드 버튼이 사라지므로, 애니메이션 Height만 고려한다)
			//다음의 Layout 코드는 apEditor.1670 즈음의 코드를 가져왔다.

			int layoutHeight = _lastWindowHeight;

			int margin = 2;
			
			if(!_isFullScreenGUI)
			{
				//풀스크린이 아닌 경우
				layoutHeight -= 35;//Top
			}
			
			layoutHeight -= margin;//Margin

			if(isAnimMenu)
			{
				int timelineHeight = 0;
				switch (_timelineLayoutSize)
				{
					case TIMELINE_LAYOUTSIZE.Size1: timelineHeight = 200; break;
					case TIMELINE_LAYOUTSIZE.Size2: timelineHeight = 340; break;
					case TIMELINE_LAYOUTSIZE.Size3: timelineHeight = 500; break;
				}

				layoutHeight -= margin + timelineHeight;
			}

			layoutHeight -= 110;//화면 왼쪽 상단 UI등에 의한 수치
			layoutHeight -= 20;//Hierarchy가 시작되는 위쪽 여백

			//4. 렌더링 위치에 맞게 스크롤 수정
			//보여지는 위치 (아래로 갈수록 값이 크다)
			int renderedPosY = unitPosY - (int)_scroll_Left_Hierarchy.y;
			if(renderedPosY < 0)
			{
				//위쪽 영역을 넘어갔다면
				_scroll_Left_Hierarchy.y = unitPosY;
			}
			else if(renderedPosY + apEditorHierarchyUnit.HEIGHT > layoutHeight)
			{
				if (layoutHeight > apEditorHierarchyUnit.HEIGHT * 3.0f)
				{
					//대신 Hieght가 충분히 커야한다.
					//renderedPosY + apEditorHierarchyUnit.HEIGHT = layoutHeight
					//renderedPosY = layoutHeight - apEditorHierarchyUnit.HEIGHT
					//unitPosY - (int)_scroll_Left_Hierarchy.y = layoutHeight - apEditorHierarchyUnit.HEIGHT
					//- (int)_scroll_Left_Hierarchy.y = layoutHeight - (apEditorHierarchyUnit.HEIGHT + unitPosY)
					_scroll_Left_Hierarchy.y = (apEditorHierarchyUnit.HEIGHT + unitPosY) - layoutHeight;
				}
			}

		}


		/// <summary>
		/// 오브젝트를 클릭해서 자동 스크롤이 가능한 상태인가? [애니메이션]
		/// 타임라인 UI로 클릭할 수 있으므로, 타입을 모두 비교한다.
		/// 옵션에 따라 탭을 자동으로 바꾼다.
		/// </summary>
		/// <param name="cursorObj"></param>
		/// <returns></returns>
		public bool IsAutoScrollableWhenClickObject_MeshGroup(object cursorObj, bool isSwitchTabAutomatically)
		{
			if(cursorObj == null)
			{
				return false;
			}

			bool isTF = (cursorObj is apTransform_Mesh) || (cursorObj is apTransform_MeshGroup);
			bool isBone = cursorObj is apBone;
			
			if(!isTF && !isBone)
			{
				//TF나 Bone이 아니라면 무조건 실패
				return false;
			}

			//선택된 객체에 이 오브젝트가 포함되어 있어야 한다.
			
			if(Select.IsSubSelected(cursorObj) == apSelection.SUB_SELECTED_RESULT.None)
			{
				//선택되지 않은 객체에 대해선 스크롤이 작동하지 않는다.
				return false;
			}

			if(isTF)
			{
				//현재 탭을 체크하자
				if(Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
				{
					return true;
				}
				else if(isSwitchTabAutomatically)
				{
					//탭이 다르더라도 옵션에 따라 변경 후 true
					//(변경은 요청 함수 호출 : 이벤트 타이밍 때문에)
					RequestSwitchRightLowerTab_MeshGroup(apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes);
					return true;
				}
			}
			
			if(isBone)
			{
				//현재 탭을 체크하자
				if(Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.Bones)
				{
					return true;
				}
				else if(isSwitchTabAutomatically)
				{
					//탭이 다르더라도 옵션에 따라 변경 후 true
					//(변경은 요청 함수 호출 : 이벤트 타이밍 때문에)
					RequestSwitchRightLowerTab_MeshGroup(apSelection.MESHGROUP_CHILD_HIERARCHY.Bones);
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// 메시그룹의 Hierarchy의 스크롤을 현재 커서에 맞게 자동으로 이동시킨다.
		/// 선택된 Unit 또는 부모가 Fold되어 있으면 Fold를 해제한다.
		/// </summary>
		/// <param name="cursorObj"></param>
		public void AutoScroll_HierarchyMeshGroup(object cursorObj)
		{
			if(cursorObj == null)
			{
				return;
			}

			//스크롤하기에 윈도우 Height가 너무 작다면 패스
			//UI가 숨겨져 있어도 패스
			if(_lastWindowHeight < 10
				|| _uiFoldType_Right1_Lower == UI_FOLD_TYPE.Folded
				|| _lastUIHeight_Right1Lower < 10)
			{
				return;
			}

			bool isMeshTab = (Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes);

			//Right 1 Lower가 존재하는 상태여야 한다. (MeshGroup Hierarchy 전용)
			if(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup)
			{
				return;
			}

			//1. Hierarchy Unit 찾기
			apEditorHierarchyUnit targetUnit = Hierarchy_MeshGroup.FindUnitByObject(cursorObj, isMeshTab);
			if(targetUnit == null)
			{
				//유닛을 찾지 못했다.
				return;
			}

			//2. Nest Fold 여부 및 해제를 시도한다.
			bool isUnfolded = targetUnit.UnfoldAllParent();

			//3. 현재 유닛의 PosY를 구하자
			int unitPosY = 0;
			if(!isUnfolded)
			{
				//접혀져있지 않았다면 > 마지막 PosY를 이용하자
				unitPosY = targetUnit.GetLastPosY();
			}
			else
			{
				//접혀져있었다면 > PosY를 다시 계산해야한다.
				bool isFindUnitInHierarchy = false;
				unitPosY = Hierarchy_MeshGroup.CalculateUnitPosY(targetUnit, isMeshTab, out isFindUnitInHierarchy);
				if(!isFindUnitInHierarchy)
				{
					//만약 해당 유닛을 Hierarchy에서 찾지 못했다면 스크롤 포기
					return;
				}
			}

			//해당 유닛이 화면상에 보이도록 스크롤을 조절하자
			int layoutHeight = _lastUIHeight_Right1Lower - 100;
			if(layoutHeight < 10)
			{
				return;
			}

			//4. 렌더링 위치에 맞게 스크롤 수정
			//보여지는 위치 (아래로 갈수록 값이 크다)
			if(isMeshTab)
			{
				int renderedPosY = unitPosY - (int)_scroll_Right1_Lower_MG_Mesh.y;
				if(renderedPosY < 0)
				{
					//위쪽 영역을 넘어갔다면
					_scroll_Right1_Lower_MG_Mesh.y = unitPosY;
				}
				else if(renderedPosY + apEditorHierarchyUnit.HEIGHT > layoutHeight)
				{
					if (layoutHeight > apEditorHierarchyUnit.HEIGHT * 3.0f)
					{
						//대신 Hieght가 충분히 커야한다.
						_scroll_Right1_Lower_MG_Mesh.y = (apEditorHierarchyUnit.HEIGHT + unitPosY) - layoutHeight;
					}
				}
			}
			else
			{
				
				int renderedPosY = unitPosY - (int)_scroll_Right1_Lower_MG_Bone.y;
				//Debug.Log("Bone 탭 : 스크롤 기존 : " + _scroll_Right1_Lower_MG_Bone.y + " | Height : " + layoutHeight);
				//Debug.Log("유닛의 위치 : " + renderedPosY);

				if(renderedPosY < 0)
				{
					//위쪽 영역을 넘어갔다면
					_scroll_Right1_Lower_MG_Bone.y = unitPosY;

					//Debug.Log("위로 이동");
				}
				else if(renderedPosY + apEditorHierarchyUnit.HEIGHT > layoutHeight)
				{
					if (layoutHeight > apEditorHierarchyUnit.HEIGHT * 3.0f)
					{
						//대신 Hieght가 충분히 커야한다.
						_scroll_Right1_Lower_MG_Bone.y = (apEditorHierarchyUnit.HEIGHT + unitPosY) - layoutHeight;

						//Debug.Log("아래로 이동");
					}
				}
				//Debug.Log(">>> " + _scroll_Right1_Lower_MG_Bone.y);
			}
		}


		/// <summary>
		/// 애니메이션 타임라인 레이어를 클릭, 선택하여 자동 스크롤이 가능한 상태인가?
		/// 타임라인 레이어의 
		/// </summary>
		/// <param name="animLayer"></param>
		/// <param name="isSwitchTabAutomatically"></param>
		/// <returns></returns>
		public bool IsAutoScrollableWhenClickObject_AnimationTimelinelayer(apAnimTimelineLayer animLayer, bool isSwitchTabAutomatically, out object timelineLayerObj)
		{
			if(animLayer == null)
			{
				timelineLayerObj = null;
				return false;
			}

			object linkedObj = null;

			if(animLayer._linkedMeshTransform != null)				{ linkedObj = animLayer._linkedMeshTransform; }
			else if(animLayer._linkedMeshGroupTransform != null)	{ linkedObj = animLayer._linkedMeshGroupTransform; }
			else if(animLayer._linkedBone != null)					{ linkedObj = animLayer._linkedBone; }
			else if(animLayer._linkedControlParam != null)			{ linkedObj = animLayer._linkedControlParam; }

			if(linkedObj == null)
			{
				timelineLayerObj = null;
				return false;
			}

			timelineLayerObj = linkedObj;

			return IsAutoScrollableWhenClickObject_Animation(linkedObj, isSwitchTabAutomatically);

		}

		/// <summary>
		/// 오브젝트를 클릭해서 자동 스크롤이 가능한 상태인가? [애니메이션]
		/// 타임라인 UI로 클릭할 수 있으므로, 타입을 모두 비교한다.
		/// 옵션에 따라 Mesh - Bone 탭이 자동으로 전환될 수 있다. (Control Parameter 리스트로 전환되지는 않음)
		/// </summary>
		/// <param name="cursorObj"></param>
		/// <returns></returns>
		public bool IsAutoScrollableWhenClickObject_Animation(object cursorObj, bool isSwitchTabAutomatically)
		{
			if(cursorObj == null)
			{
				return false;
			}
			if (Select.AnimTimeline != null
				&& Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				//컨트롤 파라미터 Hierachy가 나올때
				return cursorObj is apControlParam;
			}
			else
			{
				//선택된 타임라인이 없을 경우 / 컨트롤 파라미터에 대한 타임라인이 아닐때
				bool isTF = (cursorObj is apTransform_Mesh) || (cursorObj is apTransform_MeshGroup);
				bool isBone = cursorObj is apBone;

				//이 객체들은 선택이 되어 있어야 한다.
				if(Select.IsSubSelected(cursorObj) == apSelection.SUB_SELECTED_RESULT.None)
				{
					//선택이 안되었다면 자동 스크롤은 되지 않는다.
					return false;
				}

				//탭을 확인하고 옵션에 따라 전환하여 리턴.
				if(isTF)
				{
					if(Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
					{
						return true;
					}
					else if(isSwitchTabAutomatically)
					{
						//자동으로 적절히 탭을 전환
						//(변경은 요청 함수 호출 : 이벤트 타이밍 때문에)
						RequestSwitchRightLowerTab_Anim(apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes);
						return true;
					}
				}

				if(isBone)
				{
					if(Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.Bones)
					{
						return true;
					}
					else if(isSwitchTabAutomatically)
					{
						//자동으로 적절히 탭을 전환
						//(변경은 요청 함수 호출 : 이벤트 타이밍 때문에)
						RequestSwitchRightLowerTab_Anim(apSelection.MESHGROUP_CHILD_HIERARCHY.Bones);
						return true;
					}
				}
			}

			return false;
		}


		public void AutoScroll_HierarchyAnimation(object cursorObj)
		{
			if(cursorObj == null)
			{
				return;
			}

			//스크롤하기에 윈도우 Height가 너무 작다면 패스
			//UI가 숨겨져있어도 패스
			if(_lastWindowHeight < 10
				|| _uiFoldType_Right1_Lower == UI_FOLD_TYPE.Folded
				|| _lastUIHeight_Right1Lower < 10)
			{
				return;
			}

			//Right 1 Lower가 존재하는 상태여야 한다. (Animation Hierarchy 전용)
			if(Select.SelectionType != apSelection.SELECTION_TYPE.Animation)
			{
				return;
			}

			//스트롤 타입을 정하자
			//단, 현재 탭과 같아야 한다.
			RIGHT_LOWER_SCROLL_TYPE scrollType = RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh;
			if(Select.AnimTimeline != null
				&& Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				//컨트롤 파라미터 리스트
				scrollType = RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam;
			}
			else
			{
				if (Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
				{
					//메시 리스트
					scrollType = RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh;
				}
				else
				{
					//본 리스트
					scrollType = RIGHT_LOWER_SCROLL_TYPE.Anim_Bone;
				}
			}

			//1. Hierarchy Unit 찾기
			apEditorHierarchyUnit targetUnit = Hierarchy_AnimClip.FindUnitByObject(cursorObj, scrollType);
			if(targetUnit == null)
			{
				//유닛을 찾지 못했다.
				return;
			}

			//2. Nest Fold 여부 및 해제를 시도한다.
			bool isUnfolded = targetUnit.UnfoldAllParent();


			//3. 현재 유닛의 PosY를 구하자
			int unitPosY = 0;
			if(!isUnfolded)
			{
				//접혀져있지 않았다면 > 마지막 PosY를 이용하자
				unitPosY = targetUnit.GetLastPosY();
			}
			else
			{
				//접혀져있었다면 > PosY를 다시 계산해야한다.
				bool isFindUnitInHierarchy = false;
				unitPosY = Hierarchy_AnimClip.CalculateUnitPosY(targetUnit, scrollType, out isFindUnitInHierarchy);
				if(!isFindUnitInHierarchy)
				{
					//만약 해당 유닛을 Hierarchy에서 찾지 못했다면 스크롤 포기
					return;
				}
			}

			//해당 유닛이 화면상에 보이도록 스크롤을 조절하자
			int layoutHeight = _lastUIHeight_Right1Lower - 65;
			if(layoutHeight < 10)
			{
				return;
			}

			//4. 렌더링 위치에 맞게 스크롤 수정
			//보여지는 위치 (아래로 갈수록 값이 크다)
			Vector2 curScrollValue = Vector2.zero;
			switch (scrollType)
			{
				case RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh:
					curScrollValue = _scroll_Right1_Lower_Anim_Mesh;
					break;

				case RIGHT_LOWER_SCROLL_TYPE.Anim_Bone:
					curScrollValue = _scroll_Right1_Lower_Anim_Bone;
					break;

				case RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam:
					curScrollValue = _scroll_Right1_Lower_Anim_ControlParam;
					break;
			}

			int renderedPosY = unitPosY - (int)curScrollValue.y;
			bool isScrollChanged = false;
			if(renderedPosY < 0)
			{
				//위쪽 영역을 넘어갔다면
				curScrollValue.y = unitPosY;
				isScrollChanged = true;
			}
			else if(renderedPosY + apEditorHierarchyUnit.HEIGHT > layoutHeight)
			{
				if (layoutHeight > apEditorHierarchyUnit.HEIGHT * 3.0f)
				{
					//대신 Hieght가 충분히 커야한다.
					curScrollValue.y = (apEditorHierarchyUnit.HEIGHT + unitPosY) - layoutHeight;
					isScrollChanged = true;
				}
			}

			if(isScrollChanged)
			{
				switch (scrollType)
				{
					case RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh:
						_scroll_Right1_Lower_Anim_Mesh = curScrollValue;
						break;

					case RIGHT_LOWER_SCROLL_TYPE.Anim_Bone:
						_scroll_Right1_Lower_Anim_Bone = curScrollValue;
						break;

					case RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam:
						_scroll_Right1_Lower_Anim_ControlParam = curScrollValue;
						break;
				}
			}
		}


		//---------------------------------------------------------------------------------
		// Right Lower 리스트의 탭 전환 요청
		//---------------------------------------------------------------------------------
		private void RequestSwitchRightLowerTab_MeshGroup(apSelection.MESHGROUP_CHILD_HIERARCHY nextTab)
		{
			if(Select._meshGroupChildHierarchy == nextTab)
			{
				return;
			}

			Select._meshGroupChildHierarchy = nextTab;

			SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Meshes, false);
			SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Bones, false);

			apEditorUtil.ReleaseGUIFocus();
		}


		private void RequestSwitchRightLowerTab_Anim(apSelection.MESHGROUP_CHILD_HIERARCHY nextTab)
		{
			if(Select._meshGroupChildHierarchy_Anim == nextTab)
			{
				return;
			}
			Select._meshGroupChildHierarchy_Anim = nextTab;

			SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, false);
			SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, false);
			SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam, false);

			

			apEditorUtil.ReleaseGUIFocus();
		}


		//---------------------------------------------------------------------------------
		// GUI Content 초기화
		//---------------------------------------------------------------------------------
		
		
		private void ResetGUIContents()
		{
			// UI가 추가될때마다 이 함수에 추가해야한다.

			_guiContent_Notification = null;
			_guiContent_TopBtn_Setting = null;
			_guiContent_TopBtn_Bake = null;
			_guiContent_MainLeftUpper_MakeNewPortrait = null;
			_guiContent_MainLeftUpper_RefreshToLoad = null;
			_guiContent_MainLeftUpper_LoadBackupFile = null;
			_guiContent_GUITopTab_Open = null;
			_guiContent_GUITopTab_Folded = null;
			_guiContent_Top_GizmoIcon_Move = null;
			_guiContent_Top_GizmoIcon_Depth = null;
			_guiContent_Top_GizmoIcon_Rotation = null;
			_guiContent_Top_GizmoIcon_Scale = null;
			_guiContent_Top_GizmoIcon_Color = null;
			_guiContent_Top_GizmoIcon_Extra = null;

			//EditorController에서 사용될 GUIContent
			_guiContent_EC_SetDefault = null;
			//_guiContent_EC_EditParameter = null;
			_guiContent_EC_MakeKey_Editing = null;
			_guiContent_EC_MakeKey_NotEdit = null;
			_guiContent_EC_Select = null;
			_guiContent_EC_RemoveKey = null;

			//UI가 추가시 여기에 코드를 작성하자
		}

	}
}