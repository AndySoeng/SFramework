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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnyPortrait
{

	//apSelection의 객체가 선택되거나 편집되면서 호출되는 함수들을 모은 부분 클래스
	public partial class apSelection
	{
		//----------------------------------------------------
		// 메인 객체 선택하기 / 해제하기
		//----------------------------------------------------

		/// <summary>
		/// Portrait를 선택한다. 나머지 모든 객체 및 변수가 초기화
		/// </summary>
		public void SelectPortrait(apPortrait portrait)
		{
			//v1.4.2 : 편집 가능 여부는 Portrait 선택 전에 실시해야한다.



			if (portrait != _portrait)
			{
				Clear();
				_portrait = portrait;

				//추가 21.8.3 : 포트레이트 변경시
				Editor.SetEditorResourceReloadable();
			}

			apSnapShotManager.I.Clear();

			if (_portrait != null)
			{
				try
				{
					//프리팹 정보를 조회하기 전에, 프리팹 정보를 갱신하자 (20.9.14)
					apEditorUtil.CheckAndRefreshPrefabInfo(_portrait);

					if (apEditorUtil.IsPrefabConnected(_portrait.gameObject))
					{
						//Prefab 해제 안내
						if (EditorUtility.DisplayDialog(
										Editor.GetText(TEXT.DLG_PrefabDisconn_Title),
										Editor.GetText(TEXT.DLG_PrefabDisconn_Body),
										Editor.GetText(TEXT.Okay)))
						{	
							apEditorUtil.DisconnectPrefab(_portrait);
						}
					}


				}
				catch (Exception ex)
				{
					Debug.LogError("Prefab Check Error : " + ex);
				}

			}
			//통계 재계산 요청
			SetStatisticsRefresh();
		}


		/// <summary>
		/// [선택 해제]
		/// 메인 객체에 대한 선택을 해제한다.
		/// 다른 메인 객체를 선택할 때도 호출된다.
		/// </summary>
		public void SelectNone()
		{
			_selectionType = SELECTION_TYPE.None;

			//_portrait = null;
			_rootUnit = null;
			_rootUnitAnimClips.Clear();
			_curRootUnitAnimClip = null;

			_image = null;
			_mesh = null;
			_meshGroup = null;
			_param = null;
			_animClip = null;

			//변경 20.5.27 : 선택된 SubObject를 래핑
			_subObjects.Clear();
			_modData.ClearAll();//추가 20.6.10

			_modifier = null;

			_isMeshGroupSetting_EditDefaultTransform = false;

			_paramSetOfMod = null;

			
			_modRegistableBones.Clear();
			_subEditedParamSetGroup = null;
			_subEditedParamSetGroupAnimPack = null;

			if(_subControlParam2ParamSetGroup == null) { _subControlParam2ParamSetGroup = new Dictionary<apControlParam, apModifierParamSetGroup>(); }
			_subControlParam2ParamSetGroup.Clear();

			//_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
			_exclusiveEditing = EX_EDIT.None;
			_isSelectionLock = false;

			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();

			//추가 22.4.6 [v1.4.0]
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();


			_subAnimTimeline = null;

			_subAnimKeyframe = null;
			
			_subAnimKeyframeList.Clear();
			_exAnimEditingMode = EX_EDIT.None;
			_isAnimSelectionLock = false;

			_animTimelineCommonCurve.Clear();//추가 3.30

			_subAnimCommonKeyframeList.Clear();
			_subAnimCommonKeyframeList_Selected.Clear();

			
			//추가 20.7.19
			_lastClickTimelineLayer = null;


			
			_isBoneDefaultEditing = false;

			//_rigEdit_isBindingEdit = false;//Rig 작업중인가 > 삭제 22.5.15
			_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가

			_imageImported = null;
			_imageImporter = null;

			SetBoneRiggingTest();
			Editor.Hierarchy_MeshGroup.ResetSubUnits();

			if (Editor._portrait != null)
			{
				for (int i = 0; i < Editor._portrait._animClips.Count; i++)
				{
					Editor._portrait._animClips[i]._isSelectedInEditor = false;
				}
			}

			Editor.Gizmos.RevertFFDTransformForce();//<추가

			//기즈모 일단 초기화
			Editor.Gizmos.Unlink();
			Editor._blurEnabled = false;

			apEditorUtil.ReleaseGUIFocus();

			apEditorUtil.ResetUndo();//메뉴가 바뀌면 Undo 기록을 초기화한다.

			//통계 재계산 요청
			SetStatisticsRefresh();

			//스크롤 초기화
			Editor.ResetScrollPosition(false, true, true, true, true);

			//Onion 초기화
			Editor.Onion.Clear();


			//21.1.31 : Visibility Preset 초기화 / 검사
			//> 옵션에 따라 다르다.

			//일단 선택된 Rule이 유효한지 판단
			if (Editor._selectedVisibilityPresetRule != null)
			{
				if (_portrait == null
				|| _portrait.VisiblePreset == null
				|| !_portrait.VisiblePreset.IsContains(Editor._selectedVisibilityPresetRule))
				{
					//유효하지 않은 경우 Rule 생략
					Editor._selectedVisibilityPresetRule = null;
				}
			}

			if (Editor._option_TurnOffVisibilityPresetWhenSelectObject)
			{
				//무조건 보기 프리셋을 해제한다.
				Editor._isAdaptVisibilityPreset = false;
			}
			else
			{
				//보기 프리셋은 유지한다.
				//단, 유효한 룰이 없다면 강제로 꺼진다.
				if(Editor._selectedVisibilityPresetRule == null)
				{
					Editor._isAdaptVisibilityPreset = false;
				}
			}

			//보기 프리셋 옵션에 관계없이
			if (_portrait != null && _portrait.VisiblePreset != null)
			{
				//동기화는 해제된다.
				_portrait.VisiblePreset.ClearSync();
			}
			


			//추가 21.2.28 : 로토스코핑 초기화
			Editor._isEnableRotoscoping = false;
			Editor._selectedRotoscopingData = null;
			Editor._iRotoscopingImageFile = 0;



			//Capture 변수 초기화
			_captureSelectedAnimClip = null;
			_captureMode = CAPTURE_MODE.None;
			_captureLoadKey = null;
			_captureSelectedAnimClip = null;

			_captureGIF_IsProgressDialog = false;

			_captureSprite_IsAnimClipInit = false;
			_captureSprite_AnimClips.Clear();
			_captureSprite_AnimClipFlags.Clear();


			//당장 다음 1프레임은 쉰다.
			Editor.RefreshClippingGL();


			//애니메이션 Auto Key도 False
			Editor._isAnimAutoKey = false;

			_isScrollingTimelineY = false;

			//추가 : 8.22 > 삭제 21.1.4
			//Editor.MeshGenerator.Clear();

			//추가 21.1.6 : 메시의 영역 편집 모드
			Editor._isMeshEdit_AreaEditing = false;
			_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;


			

			//미러도 초기화
			Editor._meshEditMirrorMode = apEditor.MESH_EDIT_MIRROR_MODE.None;
			Editor.MirrorSet.Clear();
			Editor.MirrorSet.ClearMovedVertex();

			//추가 : Hierarchy SortMode 비활성화
			Editor.TurnOffHierarchyOrderEdit();

			//추가 21.3.18 : Paste 슬롯 누른거 초기화
			//_iMultiPasteSlotMethod = 0;//Method는 초기화하지 않음
			_iPasteSlot_Main = 0;
			for (int i = 0; i < NUM_PASTE_SLOTS; i++)
			{
				_isPasteSlotSelected[i] = false;
			}


			_linkedToModBones.Clear();
			_prevRenderUnit_CheckLinkedToModBones = null;

			// 핀 선택 초기화 (22.3.2 : v1.4.0)
			_selectedPin = null;
			if(_selectedPinList == null)
			{
				_selectedPinList = new List<apMeshPin>();
			}
			_selectedPinList.Clear();

			_snapPin = null;
			_isPinMouseWire = false;
			_pinMouseWirePosW = Vector2.zero;
		}




		//이 함수가 호출되면 첫번째 RootUnit을 자동으로 호출한다.
		/// <summary>
		/// 메인 객체 : 기본 Root Unit을 선택한다.
		/// </summary>
		public void SelectRootUnitDefault(bool isRefreshHierarchy = true)
		{
			if (_portrait == null)
			{
				return;
			}

			if (_portrait._rootUnits.Count == 0)
			{
				SelectNone();

				Editor.Gizmos.Unlink();
				return;
			}

			//첫번째 유닛을 호출
			SelectRootUnit(_portrait._rootUnits[0], isRefreshHierarchy);

			
		}



		/// <summary>
		/// 메인 객체 : Root Unit을 선택한다.
		/// </summary>
		/// <param name="rootUnit"></param>
		public void SelectRootUnit(apRootUnit rootUnit, bool isRefreshHierarchy = true)
		{
			SelectNone();

			if (_rootUnit != rootUnit)
			{
				_curRootUnitAnimClip = null;
			}

			_rootUnitAnimClips.Clear();

			_selectionType = SELECTION_TYPE.Overall;

			_exAnimEditingMode = EX_EDIT.None;
			_isAnimSelectionLock = false;
			//이전
			//SetModifierExclusiveEditing(EX_EDIT.None);

			//변경 22.5.15
			_exclusiveEditing = EX_EDIT.None;
			//AutoRefreshModifierExclusiveEditing();



			SetModifierSelectionLock(false);


			//SetModifierEditMode(EX_EDIT_KEY_VALUE.None);//삭제 22.5.14

			//_rigEdit_isBindingEdit = false;//삭제 22.5.15
			_rigEdit_isTestPosing = false;



			if (rootUnit != null)
			{
				_rootUnit = rootUnit;

				//이 RootUnit에 적용할 AnimClip이 뭐가 있는지 확인하자
				for (int i = 0; i < _portrait._animClips.Count; i++)
				{
					apAnimClip animClip = _portrait._animClips[i];
					if (_rootUnit._childMeshGroup == animClip._targetMeshGroup)
					{
						_rootUnitAnimClips.Add(animClip);//<<연동되는 AnimClip이다.
					}
				}

				if (_rootUnit._childMeshGroup != null)
				{
					//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.
					//이전 > 이렇게 작성하는건 안전하지만 무의미한 코드도 실행된다.
					//_rootUnit._childMeshGroup.SetDirtyToReset();
					//_rootUnit._childMeshGroup.SetDirtyToSort();
					//_rootUnit._childMeshGroup.RefreshForce(true);

					//_rootUnit._childMeshGroup.LinkModMeshRenderUnits();
					//_rootUnit._childMeshGroup.RefreshModifierLink();

					//변경 20.4.4 : 아래와 같이 호출하자
					apMeshGroup rootMeshGroup = _rootUnit._childMeshGroup;
					
					apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(rootMeshGroup);

					//추가 21.3.20 : 이 함수를 호출하면 RenderUnit이 재활용되지 않고 아예 새로 만들어진다.
					rootMeshGroup.ClearRenderUnits();

					rootMeshGroup.SetDirtyToReset();//<<이게 있어야 마스크 메시가 제대로 설정된다.
					rootMeshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);
					
					//rootMeshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh.Set_AllObjects(rootMeshGroup));
					rootMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);



					_rootUnit._childMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					_rootUnit._childMeshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

					//이전
					//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_rootUnit._childMeshGroup, true, true, true);

					//변경 20.4.13
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	_rootUnit._childMeshGroup, 
																		apEditorController.RESET_VISIBLE_ACTION.ResetForceAndNoSync, 
																		apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

					//추가 21.1.32 : Rule 가시성도 초기화
					Editor.Controller.ResetMeshGroupRuleVisibility(_rootUnit._childMeshGroup);


					
					//_rootUnit._childMeshGroup._modifierStack.RefreshAndSort(true);//이전
					//변경 22.12.13
					_rootUnit._childMeshGroup._modifierStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
																				apModifierStack.REFRESH_OPTION_REMOVE.Ignore);

					_rootUnit._childMeshGroup.ResetBoneGUIVisible();


					//RefreshMeshGroupExEditingFlags(_rootUnit._childMeshGroup, null, null, null, false);//<<추가
					//변경 21.2.15
					SetEnableMeshGroupExEditingFlagsForce();

					_isSelectionLock = false;

				}
			}

			if (_curRootUnitAnimClip != null)
			{
				if (!_rootUnitAnimClips.Contains(_curRootUnitAnimClip))
				{
					_curRootUnitAnimClip = null;//<<이건 포함되지 않습니더
				}
			}

			if (_curRootUnitAnimClip != null)
			{
				_curRootUnitAnimClip._isSelectedInEditor = true;
			}



			Editor.Gizmos.Unlink();

			//통계 재계산 요청
			SetStatisticsRefresh();

			//변경 21.3.4 : 자동으로 가운데로 초점 이동
			Editor.ResetScrollPosition(false, true, false, false, false);

			if(isRefreshHierarchy)//Inspector에서 여는 경우엔 이 값이 false다.
			{
				Editor.RefreshControllerAndHierarchy(false);
			}
			

		}





		/// <summary>
		/// 메인 객체 : 이미지를 선택한다.
		/// </summary>
		/// <param name="image"></param>
		public void SelectImage(apTextureData image)
		{
			SelectNone();

			_selectionType = SELECTION_TYPE.ImageRes;

			_image = image;

			//이미지의 Asset 정보는 매번 갱신한다. (언제든 바뀔 수 있으므로)
			if (image._image != null)
			{
				string fullPath = AssetDatabase.GetAssetPath(image._image);
				//Debug.Log("Image Path : " + fullPath);

				if (string.IsNullOrEmpty(fullPath))
				{
					image._assetFullPath = "";
				}
				else
				{
					image._assetFullPath = fullPath;
				}
			}
			else
			{
				//주의
				//만약 assetFullPath가 유효하다면 그걸 이용하자
				bool isRestoreImageFromPath = false;
				if (!string.IsNullOrEmpty(image._assetFullPath))
				{
					Texture2D restoreImage = AssetDatabase.LoadAssetAtPath<Texture2D>(image._assetFullPath);
					if (restoreImage != null)
					{
						isRestoreImageFromPath = true;
						image._image = restoreImage;
						//사라진 이미지를 경로로 복구했다. [" + image._assetFullPath + "]
					}
				}
				if (!isRestoreImageFromPath)
				{
					image._assetFullPath = "";
				}
			}

			//추가 21.3.4 : 이미지 선택시 자동으로 가운데로 초점 이동
			Editor.ResetScrollPosition(false, true, false, false, false);

			//통계 재계산 요청
			SetStatisticsRefresh();
		}



		/// <summary>
		/// 메인 객체 : 메시를 선택한다.
		/// UI가 바뀌면서 여기서 초기 상태의 Gizmo 연결도 수행한다.
		/// </summary>
		/// <param name="mesh"></param>
		public void SelectMesh(apMesh mesh)
		{
			SelectNone();

			_selectionType = SELECTION_TYPE.Mesh;

			_mesh = mesh;
			//_prevMesh_Name = _mesh._name;

			Editor.VertController.UnselectVertex();//메시가 바뀌었는데 기즈모가 남아있는 버그 제거(20.9.16)
			UnselectMeshPins();

			//통계 재계산 요청
			SetStatisticsRefresh();

			//새로 생성된 Mesh라면 탭을 Setting으로 변경
			if (mesh != null && _createdNewMeshes.Contains(mesh))
			{
				_createdNewMeshes.Remove(mesh);
				Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Setting;

				//Pin Tool 추가 (v1.4.0 : 22.3.2)
				Editor._meshEditMode_Pin_ToolMode = apEditor.MESH_EDIT_PIN_TOOL_MODE.Select;
			}

			//핀 그룹을 재계산
			if(_mesh._pinGroup != null)
			{
				_mesh._pinGroup.Test_ResetMatrixAll(apMeshPin.TMP_VAR_TYPE.MeshTest);
				_mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
			}

			//현재 MeshEditMode에 따라서 Gizmo 처리를 해야한다.
			switch (Editor._meshEditMode)
			{
				case apEditor.MESH_EDIT_MODE.Setting:
					Editor.Gizmos.Unlink();
					break;

				case apEditor.MESH_EDIT_MODE.MakeMesh:
					{
						Editor.Gizmos.Unlink();
						//변경 : MakeMesh 중 Gizmo가 사용되는 서브 툴이 있다.
						switch (Editor._meshEditeMode_MakeMesh_Tab)
						{
							case apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS:
								//TRS는 기즈모를 등록해야한다.
								Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshTRS());
								break;

							case apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen:
								//Auto Gen도 Control Point를 제어하는 기즈모가 있다.
								//Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshAreaEdit());
								Editor.Gizmos.Unlink();//기즈모 삭제됨 21.1.6
								break;
						}
					}

					break;

				case apEditor.MESH_EDIT_MODE.Modify:
					Editor.Gizmos.Unlink();
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshEdit_Modify());
					break;

				case apEditor.MESH_EDIT_MODE.PivotEdit:
					Editor.Gizmos.Unlink();
					break;

				case apEditor.MESH_EDIT_MODE.Pin:
					{
						Editor.Gizmos.Unlink();
						RefreshPinModeEvent();
					}
					break;
			}

			//삭제 21/1/4
			//Editor.MeshGenerator.CheckAndSetMesh(mesh);

			//추가 21.3.4
			//Area가 없는 경우 : 초점을 가운데로 맞춤 (Pivot 체크)
			//Area가 있는 경우 : Area로 초점을 맞춤
			if (_mesh != null && _mesh._isPSDParsed)
			{
				Vector2 areaCenter = _mesh._atlasFromPSD_LT * 0.5f + _mesh._atlasFromPSD_RB * 0.5f;
				areaCenter -= _mesh._offsetPos;

				areaCenter.y *= -1;
				areaCenter *= apGL.Zoom;
				Editor._scroll_CenterWorkSpace.x = (areaCenter.x) / (apGL.WindowSize.x * 0.1f);
				Editor._scroll_CenterWorkSpace.y = (areaCenter.y) / (apGL.WindowSize.y * 0.1f);
			}
			else
			{
				//자동으로 가운데로 초점 이동
				Editor.ResetScrollPosition(false, true, false, false, false);
			}
		}



		/// <summary>
		/// 메인 객체 : 메시 그룹을 선택한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void SelectMeshGroup(apMeshGroup meshGroup)
		{
			SelectNone();

			_selectionType = SELECTION_TYPE.MeshGroup;

			bool isChanged = false;
			if (_meshGroup != meshGroup)
			{
				isChanged = true;
			}
			_meshGroup = meshGroup;
			
			//이전 > 이렇게 작성하는건 안전하지만 무의미한 코드도 같이 실행된다.
			//_meshGroup.SetDirtyToReset();
			//_meshGroup.SetDirtyToSort();
			//_meshGroup.RefreshForce(true);//Depth 바뀌었다고 강제한다.

			//변경 20.4.4 : 아래와 같이 호출하자
			apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(_meshGroup);

			//추가 21.3.20 : 이 함수를 호출하면 RenderUnit이 재활용되지 않고 아예 새로 만들어진다.
			_meshGroup.ClearRenderUnits();

			_meshGroup.SetDirtyToReset();
			_meshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);




			Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Setting;

			if (isChanged)
			{
				
				//변경 20.4.4
				_meshGroup.RefreshModifierLink(apUtil.LinkRefresh);


				_meshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

				//_meshGroup._modifierStack.RefreshAndSort(true);//이전
				//변경 22.12.13
				_meshGroup._modifierStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
															apModifierStack.REFRESH_OPTION_REMOVE.Ignore);

				////ModParamSetGroup의 RefreshSync 함수가 호출되는 이후애 ModLinkInfo의 연결을 갱신하자
				//_modLinkInfo.LinkRefresh();//추가 21.2.14

				Editor.Gizmos.RevertFFDTransformForce();
			}


			//렌더 유닛/본의 작업용 임시 Visibility를 설정하자.
			//[20.4.13] Visibility Controller를 이용하자
			//동기화 후에 옵션에 따라 초기화를 하자
			Editor.VisiblityController.SyncMeshGroup(_meshGroup);
			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	_meshGroup, 
																apEditorController.RESET_VISIBLE_ACTION.RestoreByOption, 
																apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

			//추가 21.1.32 : Rule 가시성도 초기화
			Editor.Controller.ResetMeshGroupRuleVisibility(_meshGroup);



			//추가 19.7.27 : 본의 RigLock도 해제한다.
			Editor.Controller.ResetBoneRigLock(meshGroup);

			Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshGroupSetting());

			SetModifierExclusiveEditing(EX_EDIT.None);
			SetModifierSelectionLock(false);
			//SetModifierEditMode(EX_EDIT_KEY_VALUE.None);

			//[v1.4.2] 편집 모드가 리셋되면서 ExEdit 플래그 등을 갱신해야한다.
			AutoRefreshModifierExclusiveEditing();

			//통계 재계산 요청
			SetStatisticsRefresh();

			Editor.Hierarchy_MeshGroup.ResetSubUnits();
			Editor.Hierarchy_MeshGroup.RefreshUnits();

			//변경 21.3.4 : 자동으로 가운데로 초점 이동
			Editor.ResetScrollPosition(false, true, false, false, false);
		}



		/// <summary>
		/// 메인 객체 : 컨트롤 파라미터를 선택한다.
		/// </summary>
		/// <param name="controlParam"></param>
		public void SelectControlParam(apControlParam controlParam)
		{
			SelectNone();

			_selectionType = SELECTION_TYPE.Param;

			_param = controlParam;

			//통계 재계산 요청
			SetStatisticsRefresh();

			//변경 21.3.4 : 자동으로 가운데로 초점 이동
			Editor.ResetScrollPosition(false, true, false, false, false);
		}


		/// <summary>
		/// 메인 객체 : 애니메이션 클립을 선택한다.
		/// </summary>
		/// <param name="animClip"></param>
		public void SelectAnimClip(apAnimClip animClip)
		{
			SelectNone();

			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip != animClip
				|| _animClip == null)
			{
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);
			}

			bool isResetInfo = false;

			for (int i = 0; i < Editor._portrait._animClips.Count; i++)
			{
				Editor._portrait._animClips[i]._isSelectedInEditor = false;
			}

			//[v1.4.2] 애니메이션 기즈모를 리셋하기 위해 상태를 변경한다.
			_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.None;
			


			bool isChanged = false;
			if (_animClip != animClip)
			{
				_animClip = animClip;

				_subAnimTimeline = null;
				_subAnimKeyframe = null;


				//이전
				//_subAnimTimelineLayer = null;>>이것도 SubObjects로 옮겨졌다.
				//_subAnimWorkKeyframe = null;

				//_subMeshTransformOnAnimClip = null;
				//_subMeshGroupTransformOnAnimClip = null;
				//_subControlParamOnAnimClip = null;

				//통합 20.5.27
				_subObjects.Clear();
				

				_subAnimKeyframeList.Clear();
				//_isAnimEditing = false;
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimSelectionLock = false;

				_subAnimCommonKeyframeList.Clear();
				_subAnimCommonKeyframeList_Selected.Clear();

				_animTimelineCommonCurve.Clear();//추가 3.30

				//이전
				//_modMeshOfAnim = null;
				//_modBoneOfAnim = null;
				//_renderUnitOfAnim = null;

				//변경 20.6.11 : 래핑
				_modData.ClearAll();

				//이전
				//_modRenderVertOfAnim = null;
				//_modRenderVertListOfAnim.Clear();
				//_modRenderVertListOfAnim_Weighted.Clear();


				//변경 20.6.29 : Mod랑 통합되었다.
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();



				isResetInfo = true;
				isChanged = true;


				//v1.4.2 추가 : 유효성 테스트를 수행해서 AnimClip의 Link를 한번 더 확인하자
				bool isValidAnimClip = _animClip.ValidateForLinkEditor();
				if(!isValidAnimClip)
				{
					//Debug.LogError("AnimClip 유효성 테스트를 통과하지 못했다. 다시 링크 [" + _animClip._name + "]");
					_animClip.LinkEditor(_portrait);
				}


				if (_animClip._targetMeshGroup != null)
				{
					//Mesh Group을 선택하면 이 초기화를 전부 실행해야한다.
					
					//이전 > 이렇게 작성하는건 안전하지만 무의미한 코드도 실행된다.
					//_animClip._targetMeshGroup.SetDirtyToReset();
					//_animClip._targetMeshGroup.SetDirtyToSort();
					//_animClip._targetMeshGroup.RefreshForce(true);
					
					//변경 20.4.3 : 이렇게 직접 호출하자
					apUtil.LinkRefresh.Set_AnimClip(_animClip);

					//추가 21.3.20 : 이 함수를 호출하면 RenderUnit이 재활용되지 않고 아예 새로 만들어진다.
					_animClip._targetMeshGroup.ClearRenderUnits();

					_animClip._targetMeshGroup.SetDirtyToReset();
					_animClip._targetMeshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);//<<이게 맞다

					//이전
					//_animClip._targetMeshGroup.LinkModMeshRenderUnits();
					//_animClip._targetMeshGroup.RefreshModifierLink();
					
					//변경 20.4.3
					//_animClip._targetMeshGroup.LinkModMeshRenderUnits(_animClip);//>이 함수는 RefreshForce(true..) > ResetRenderUnits에 포함되어 있다.
					_animClip._targetMeshGroup.RefreshModifierLink(apUtil.LinkRefresh);

					_animClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();//<<값 초기화

					//_animClip._targetMeshGroup._modifierStack.RefreshAndSort(true);
					//변경 22.12.13
					_animClip._targetMeshGroup._modifierStack.RefreshAndSort(	apModifierStack.REFRESH_OPTION_ACTIVE.ActiveAllModifierIfPossible,
																				apModifierStack.REFRESH_OPTION_REMOVE.Ignore);



					//이전
					//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup, true, true, true);
					//_animClip._targetMeshGroup.ResetBoneGUIVisible();

					//변경 20.4.13 : VisibilityController를 이용하여 작업용 출력 여부를 초기화 및 복구하자
					//동기화 후 옵션에 따라 결정
					Editor.VisiblityController.SyncMeshGroup(_animClip._targetMeshGroup);
					Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	_animClip._targetMeshGroup, 
																		apEditorController.RESET_VISIBLE_ACTION.RestoreByOption, 
																		apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

					//추가 21.1.32 : Rule 가시성도 초기화
					Editor.Controller.ResetMeshGroupRuleVisibility(_animClip._targetMeshGroup);


					//변경 20.4.3 : 위에서 RefreshForce를 하는 코드를 지우고 여기서 갱신을 한다.
					//_animClip._targetMeshGroup.RefreshForce(true);


					//Debug.LogError("--------------------------------------------------");
				}

				_animClip.Pause_Editor();

				Editor.Gizmos.RevertFFDTransformForce();

			}
			
			
			_animClip = animClip;
			_animClip._isSelectedInEditor = true;

			_selectionType = SELECTION_TYPE.Animation;
			
			if (isChanged && _animClip != null)
			{
				//타임라인을 자동으로 선택해주자
				if (_animClip._timelines.Count > 0)
				{
					apAnimTimeline firstTimeline = _animClip._timelines[0];
					SelectAnimTimeline(firstTimeline, true, true, false);
				}
			}

			bool isWorkKeyframeChanged = false;
			AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//Work Keyframe 변화를 별도로 체크하지는 않는다.

			if (isResetInfo)
			{
				//Sync를 한번 돌려주자
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;
				_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Next;
				_animPropertyCurveUI_Multi = ANIM_MULTI_PROPERTY_CURVE_UI.Next;
				Editor.Controller.AddAndSyncAnimClipToModifier(_animClip);//<<여기서 Modifier의 Ex 설정을 한다.
			}


			//추가 21.3.7 : 옵션에 따라, AnimAutoKey를 복구할 필요가 있다.
			if(!Editor._option_IsTurnOffAnimAutoKey)
			{
				//초기화하면 안되는 경우
				Editor._isAnimAutoKey = EditorPrefs.GetBool("AnyPortrait_LastAnimAutoKeyValue", false);//값이 없다면 False이다.
			}

			Editor.RefreshTimelineLayers((isResetInfo ? apEditor.REFRESH_TIMELINE_REQUEST.All : apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier),
											null, null
											);

			Editor.Hierarchy_AnimClip.ResetSubUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();


			////ModParamSetGroup의 RefreshSync 함수가 호출되는 이후애 ModLinkInfo의 연결을 갱신하자
			//_modLinkInfo.LinkRefresh();//추가 21.2.14

			//Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Morph());

			SetAnimClipGizmoEvent();//Gizmo 이벤트 연결
			
			//이전
			//RefreshAnimEditing(true);

			Editor.Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.15



			//통계 재계산 요청
			SetStatisticsRefresh();

			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();

			//변경 21.3.4 : 자동으로 가운데로 초점 이동
			Editor.ResetScrollPosition(false, true, false, false, false);
		}





		//---------------------------------------------------------
		// 서브 객체 선택
		//---------------------------------------------------------
		public void SelectMeshTF(apTransform_Mesh subMeshTransformInGroup, MULTI_SELECT multiSelect)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				//변경 20.5.27 : 선택 정보 래핑
				_subObjects.Clear();
				_modData.ClearAll();

				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				return;
			}

			bool isChanged = _subObjects.SelectMeshTF(subMeshTransformInGroup, multiSelect);

			//여기서 만약 Modifier 선택중이며, 특정 ParamKey를 선택하고 있다면
			//자동으로 ModifierMesh를 선택해보자
			AutoSelectModMeshOrModBone();//<<이 안에서 래핑 이후 선택 처리가 된다.

			if(isChanged || _subObjects.MeshTF == null)//래핑후
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}
		}

		
		public void SelectMeshGroupTF(apTransform_MeshGroup subMeshGroupTransformInGroup, MULTI_SELECT multiSelect)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_subObjects.Clear();
				_modData.ClearAll();//<<이것도

				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				return;
			}

			//변경 20.5.27 : 래핑하였다.
			bool isChanged = _subObjects.SelectMeshGroupTF(subMeshGroupTransformInGroup, multiSelect);

			//여기서 만약 Modifier 선택중이며, 특정 ParamKey를 선택하고 있다면
			//자동으로 ModifierMesh를 선택해보자
			AutoSelectModMeshOrModBone();//<<여기서 처리가 개선되었다.

			if(isChanged || _subObjects.MeshTF == null)//래핑 후
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}
		}




		//추가 20.4.11 : Transform 계열 모디파이어 편집시 Mesh/MeshGroup Transform / Bone을 한번에 선택한다.
		//(Gizmo 이벤트와 Hierarchy에서 호출하자)
		//Riggin 모디파이어에서는 호출하지 말자
		/// <summary>
		/// Transform 계열 모디파이어에서 오브젝트를 호출하는 함수. Mesh/MeshGroup Transform이나 Bone을 배타적으로 선택한다.
		/// </summary>
		/// <param name="meshTransform"></param>
		/// <param name="meshGroupTransform"></param>
		/// <param name="bone"></param>
		public void SelectSubObject(	apTransform_Mesh meshTransform,
										apTransform_MeshGroup meshGroupTransform,
										apBone bone,
										MULTI_SELECT multiSelect,
										TF_BONE_SELECT selectTFBoneType//이게 True이면 TF가 선택될 땐 Bone이 null이 되거나 그 반대이다. (동시 선택 불가)
										)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup 
				&& _selectionType != SELECTION_TYPE.Animation)
			{	
				//변경 20.5.27 : 래핑 후 코드
				_subObjects.Clear();
				_modData.ClearAll();

				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				return;
			}

			//변경 20.5.27 : 래핑 후
			bool isChanged = false;

			if(meshTransform != null)
			{
				isChanged = _subObjects.Select(meshTransform, null, null, multiSelect, selectTFBoneType);
				_subObjects.ClearControlParam();
			}
			else if(meshGroupTransform != null)
			{
				isChanged = _subObjects.Select(null, meshGroupTransform, null, multiSelect, selectTFBoneType);
				_subObjects.ClearControlParam();
			}
			else if(bone != null)
			{
				isChanged = _subObjects.Select(null, null, bone, multiSelect, selectTFBoneType);
				_subObjects.ClearControlParam();
			}
			else
			{
				isChanged = _subObjects.Select(null, null, null, multiSelect, selectTFBoneType);
			}


			if (isChanged)
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}

			if (SelectionType == SELECTION_TYPE.MeshGroup &&
				Modifier != null)
			{
				AutoSelectModMeshOrModBone();
			}
			if (SelectionType == SELECTION_TYPE.Animation && AnimClip != null)
			{
				AutoSelectAnimTimelineLayer(true);
			}

			apEditorUtil.ReleaseGUIFocus();
		}

		/// <summary>
		/// 현재 메시 그룹의 모든 오브젝트 (메시 or 본)들을 선택한다.
		/// </summary>
		/// <param name="isMeshTFs"></param>
		public void SelectAllSubObjects(bool isMeshTFs)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup 
				&& _selectionType != SELECTION_TYPE.Animation)
			{
				//래핑 전 코드
				_subObjects.Clear();
				_modData.ClearAll();

				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				return;
			}


			//전체를 돌면서 선택하자
			apMeshGroup curMeshGroup = null;
			if(_selectionType == SELECTION_TYPE.MeshGroup)
			{
				curMeshGroup = MeshGroup;
			}
			else if(_selectionType == SELECTION_TYPE.Animation)
			{
				if(AnimClip != null)
				{
					curMeshGroup = AnimClip._targetMeshGroup;
				}
			}
			if(curMeshGroup == null)
			{
				return;
			}

			//일단 초기화
			_subObjects.Select(null, null, null, MULTI_SELECT.Main, TF_BONE_SELECT.Exclusive);

			if (isMeshTFs)
			{
				//메시, 메시그룹 TF
				List<apTransform_Mesh> meshTFs = new List<apTransform_Mesh>();
				List<apTransform_MeshGroup> meshGroupTFs = new List<apTransform_MeshGroup>();

				AddAllTFsToList_Recv(curMeshGroup, curMeshGroup, meshTFs, meshGroupTFs);

				//이제 전부 돌면서 선택하자
				for (int i = 0; i < meshTFs.Count; i++)
				{
					_subObjects.SelectMeshTF(meshTFs[i], MULTI_SELECT.AddOrSubtract);
				}

				for (int i = 0; i < meshGroupTFs.Count; i++)
				{
					_subObjects.SelectMeshGroupTF(meshGroupTFs[i], MULTI_SELECT.AddOrSubtract);
				}
			}
			else
			{
				//본
				List<apBone> bones = new List<apBone>();
				int nBoneListSets = curMeshGroup._boneListSets != null ? curMeshGroup._boneListSets.Count : 0;
				apMeshGroup.BoneListSet curBLS = null;

				if (nBoneListSets > 0)
				{
					for (int iBoneSet = 0; iBoneSet < nBoneListSets; iBoneSet++)
					{
						curBLS = curMeshGroup._boneListSets[iBoneSet];
						if (curBLS._bones_All != null && curBLS._bones_All.Count > 0)
						{
							for (int iBone = 0; iBone < curBLS._bones_All.Count; iBone++)
							{
								bones.Add(curBLS._bones_All[iBone]);
							}
						}
					}
				}

				for (int i = 0; i < bones.Count; i++)
				{
					_subObjects.SelectBone(bones[i], MULTI_SELECT.AddOrSubtract);
				}
			}




			Editor.Gizmos.RevertFFDTransformForce();

			if (SelectionType == SELECTION_TYPE.MeshGroup &&
				Modifier != null)
			{
				AutoSelectModMeshOrModBone();
			}
			if (SelectionType == SELECTION_TYPE.Animation && AnimClip != null)
			{
				AutoSelectAnimTimelineLayer(true);
			}

			apEditorUtil.ReleaseGUIFocus();
		}


		//현재 메시 그룹의 모든 오브젝트들을 리스트에 넣는다. (TF와	Bone 타입을 구분하자)
		private void AddAllTFsToList_Recv(	apMeshGroup curMeshGroup, 
											apMeshGroup rootMeshGroup, 
											List<apTransform_Mesh> meshTFs, 
											List<apTransform_MeshGroup> meshGroupTFs)
		{
			if(curMeshGroup == null)
			{
				return;
			}
			int nMeshTFs = curMeshGroup._childMeshTransforms != null ? curMeshGroup._childMeshTransforms.Count : 0;
			int nMeshGroupTFs = curMeshGroup._childMeshGroupTransforms != null ? curMeshGroup._childMeshGroupTransforms.Count : 0;

			if(nMeshTFs > 0)
			{
				for (int iMeshTF = 0; iMeshTF < nMeshTFs; iMeshTF++)
				{
					meshTFs.Add(curMeshGroup._childMeshTransforms[iMeshTF]);
				}
			}

			if(nMeshGroupTFs > 0)
			{
				apTransform_MeshGroup curMGTF = null;
				for (int iMGTF = 0; iMGTF < nMeshGroupTFs; iMGTF++)
				{
					curMGTF = curMeshGroup._childMeshGroupTransforms[iMGTF];
					meshGroupTFs.Add(curMGTF);

					if(curMGTF._meshGroup != null
						&& curMGTF._meshGroup != curMeshGroup
						&& curMGTF._meshGroup != rootMeshGroup)
					{
						//재귀적으로 호출
						AddAllTFsToList_Recv(curMGTF._meshGroup, rootMeshGroup, meshTFs, meshGroupTFs);
					}
				}
			}
		}





		//---------------------------------------------------------
		// 메시 편집 화면에서의 객체 선택
		//---------------------------------------------------------
		/// <summary>메시 편집 화면에서 핀을 선택한다.</summary>
		/// <param name="pin"></param>
		/// <param name="selectType"></param>
		public void SelectMeshPin(apMeshPin pin, apGizmos.SELECT_TYPE selectType)
		{
			if(selectType == apGizmos.SELECT_TYPE.New)
			{
				UnselectMeshPins();
			}

			
			if(_selectedPinList == null)
			{
				_selectedPinList = new List<apMeshPin>();
			}

			if(selectType == apGizmos.SELECT_TYPE.Subtract)
			{
				//빼기
				_selectedPinList.Remove(pin);
				if(_selectedPin == pin || _selectedPin == null)
				{
					if(_selectedPinList.Count > 0)
					{
						//메인 선택 해제하고 다음 첫번째 항목을 메인으로 선택
						_selectedPin = _selectedPinList[0];
					}
					else
					{
						//모두 해제
						_selectedPin = null;
					}
					
				}
			}
			else
			{
				//더하기
				if(_selectedPin == null)
				{
					_selectedPin = pin;
				}
				if(!_selectedPinList.Contains(pin))
				{
					_selectedPinList.Add(pin);
				}
			}
			
		}


		public void SelectMeshPins(List<apMeshPin> pins, apGizmos.SELECT_TYPE selectType)
		{
			if(selectType == apGizmos.SELECT_TYPE.New)
			{
				UnselectMeshPins();
			}

			int nPins = pins != null ? pins.Count : 0;
			if(nPins == 0)
			{
				return;
			}
			
			if(_selectedPinList == null)
			{
				_selectedPinList = new List<apMeshPin>();
			}

			apMeshPin curPin = null;

			if(selectType == apGizmos.SELECT_TYPE.Subtract)
			{
				//빼기
				for (int iPin = 0; iPin < nPins; iPin++)
				{
					curPin = pins[iPin];
					_selectedPinList.Remove(curPin);
					if (_selectedPin == curPin || _selectedPin == null)
					{
						if (_selectedPinList.Count > 0)
						{
							//메인 선택 해제하고 다음 첫번째 항목을 메인으로 선택
							_selectedPin = _selectedPinList[0];
						}
						else
						{
							//모두 해제
							_selectedPin = null;
						}
					}
				}
			}
			else
			{
				//더하기
				for (int iPin = 0; iPin < nPins; iPin++)
				{
					curPin = pins[iPin];
					if (_selectedPin == null)
					{
						_selectedPin = curPin;
					}
					if (!_selectedPinList.Contains(curPin))
					{
						_selectedPinList.Add(curPin);
					}
				}
			}
			
		}

		/// <summary>메시 편집화면에서 핀 선택을 해제한다.</summary>
		public void UnselectMeshPins()
		{
			_selectedPin = null;
			if(_selectedPinList == null)
			{
				_selectedPinList = new List<apMeshPin>();
			}
			_selectedPinList.Clear();
		}

		/// <summary>메시 편집 화면의 핀 편집 화면에서 Ctrl 키를 눌렀을 때, 가장 가까운 핀을 다음 스냅 대상으로 삼는다.</summary>
		public void SelectSnapPin(apMeshPin pin)
		{
			_snapPin = pin;
		}

		public void UnselectSnapPin()
		{
			_snapPin = null;
		}

		public void UpdatePinEditWire(Vector2 mouseWirePosW)
		{
			_isPinMouseWire = true;
			_pinMouseWirePosW = mouseWirePosW;
		}



		/// <summary>
		/// 옵션에 따라서 현재 메시의 Pin-Vertex 가중치를 다시 계산한다.
		/// </summary>
		public void RecalculatePinWeightByOption()
		{
			if(!Editor._pinOption_AutoWeightRefresh)
			{
				return;
			}

			if(_selectionType != SELECTION_TYPE.Mesh
				|| _mesh == null
				|| _mesh._pinGroup == null)
			{
				return;
			}
			_mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
		}

		
		//---------------------------------------------------------------
		// 모드 변경 (Morph 타겟)
		//---------------------------------------------------------------
		public void SetMorphEditTarget(MORPH_EDIT_TARGET editTarget)
		{
			if(_morphEditTarget == editTarget)
			{
				return;
			}

			_morphEditTarget = editTarget;

			//FFD 중이라면 해제
			if(Editor.Gizmos.IsFFDMode)
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}

			//모두 선택 해제
			//UnselectMeshPins();///<< 이건 메시 선택화면이고
			if(SelectionType == SELECTION_TYPE.MeshGroup)
			{
				//메시 그룹에서는
				SelectModRenderVertOfModifier(null);
				SelectModRenderPinOfModifier(null);
			}
			else
			{
				SelectModRenderVert_ForAnimEdit(null);
				SelectModRenderPin_ForAnimEdit(null);
			}

			//모디파이어 상태 갱신
			AutoRefreshModifierExclusiveEditing();
		}

		//---------------------------------------------------------
		// 메시 그룹에서의 객체 선택 (애니메이션 아님)
		//---------------------------------------------------------
		/// <summary>
		/// [MeshGroup 편집 화면에서] 모디파이어를 선택한다.
		/// </summary>
		/// <param name="modifier"></param>
		public void SelectModifier(apModifierBase modifier)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup)
			{
				_modifier = null;
				return;
			}

			bool isChanged = false;
			if (_modifier != modifier || modifier == null)
			{
				_paramSetOfMod = null;
				
				//변경 20.6.11
				_modData.ClearAll();

				_modRegistableBones.Clear();
				_subEditedParamSetGroup = null;
				_subEditedParamSetGroupAnimPack = null;

				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				//_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
				_exclusiveEditing = EX_EDIT.None;

				_modifier = modifier;
				isChanged = true;

				//_rigEdit_isBindingEdit = false;//Rig 작업중인가 > 삭제 22.5.15
				_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가
				_rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None;
				Editor.Gizmos.EndBrush();

				SetBoneRiggingTest();

				//스크롤 초기화 (오른쪽과 아래쪽)
				Editor.ResetScrollPosition(false, false, false, true, true);

			}

			_modifier = modifier;

			if (modifier != null)
			{
				//삭제 22.5.14
				//if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
				//{
				//	SetModifierEditMode(EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert);
				//}
				//else
				//{
				//	SetModifierEditMode(EX_EDIT_KEY_VALUE.ParamKey_ModMesh);
				//}
				
				//ParamSetGroup이 선택되어 있다면 Modifier와의 유효성 체크
				bool isSubEditedParamSetGroupInit = false;
				if (_subEditedParamSetGroup != null)
				{
					if (!_modifier._paramSetGroup_controller.Contains(_subEditedParamSetGroup))
					{
						isSubEditedParamSetGroupInit = true;

					}
				}
				else if (_subEditedParamSetGroupAnimPack != null)
				{
					if (!_modifier._paramSetGroupAnimPacks.Contains(_subEditedParamSetGroupAnimPack))
					{
						isSubEditedParamSetGroupInit = true;
					}
				}
				if (isSubEditedParamSetGroupInit)
				{
					_paramSetOfMod = null;
					
					//변경 20.6.10
					_modData.ClearAll();

					_modRegistableBones.Clear();
					_subEditedParamSetGroup = null;
					_subEditedParamSetGroupAnimPack = null;

					
					
					_modRenderVert_Main = null;
					_modRenderVerts_All.Clear();
					_modRenderVerts_Weighted.Clear();

					//추가 22.4.6 [v1.4.0]
					_modRenderPin_Main = null;
					_modRenderPins_All.Clear();
					_modRenderPins_Weighted.Clear();
				}


				//이전
				//if (MeshGroup != null)
				//{
				//	//Exclusive 모두 해제
				//	//MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					
				//	////변경 20.4.13 : 
				//	//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	MeshGroup, 
				//	//													apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff,
				//	//													apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

				//	//RefreshMeshGroupExEditingFlags(true);//변경 21.2.15

					
				//}

				//변경 22.5.14 : 완성된 함수를 이용하자
				SetModifierExclusiveEditing(EX_EDIT.None);


				//각 타입에 따라 Gizmo를 넣어주자
				if (_modifier is apModifier_Morph)
				{
					//Morph
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Morph());
				}
				else if (_modifier is apModifier_TF)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_TF());
				}
				else if (_modifier is apModifier_Rigging)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Rigging());
					_rigEdit_isTestPosing = false;//Modifier를 선택하면 TestPosing은 취소된다.

					SetBoneRiggingTest();
				}
				else if (_modifier is apModifier_Physic)
				{
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_Physics());
				}
				else if(_modifier is apModifier_ColorOnly)//추가 21.7.20
				{
					
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Modifier_ColorOnly());
				}
				else
				{
					if (!_modifier.IsAnimated)
					{
						Debug.LogError("Modifier를 선택하였으나 Animation 타입이 아닌데도 Gizmo에 지정되지 않은 타입 : " + _modifier.GetType());
					}
					//아니면 말고 >> Gizmo 초기화
					Editor.Gizmos.Unlink();
				}

				//AutoSelect하기 전에
				//현재 타입이 Static이라면
				//ParamSetGroup/ParamSet은 자동으로 선택한다.
				//ParamSetGroup, ParamSet은 각각 한개씩 존재한다.
				if (_modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Static)
				{
					apModifierParamSetGroup paramSetGroup = null;
					apModifierParamSet paramSet = null;
					if (_modifier._paramSetGroup_controller.Count == 0)
					{
						Editor.Controller.AddStaticParamSetGroupToModifier();
					}

					paramSetGroup = _modifier._paramSetGroup_controller[0];

					if (paramSetGroup._paramSetList.Count == 0)
					{
						paramSet = new apModifierParamSet();
						paramSet.LinkParamSetGroup(paramSetGroup);
						paramSetGroup._paramSetList.Add(paramSet);
					}

					paramSet = paramSetGroup._paramSetList[0];

					SelectParamSetGroupOfModifier(paramSetGroup);
					SelectParamSetOfModifier(paramSet);
				}
				else if (!_modifier.IsAnimated)
				{
					if (_subEditedParamSetGroup == null)
					{
						if (_modifier._paramSetGroup_controller.Count > 0)
						{
							//마지막으로 입력된 PSG를 선택
							SelectParamSetGroupOfModifier(_modifier._paramSetGroup_controller[_modifier._paramSetGroup_controller.Count - 1]);
						}
					}
					//맨 위의 ParamSetGroup을 선택하자
				}

				if (_modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller)
				{
					//옵션이 허용하는 경우 (19.6.28 변경)
					if (Editor._isAutoSwitchControllerTab_Mod)
					{
						Editor.SetLeftTab(apEditor.TAB_LEFT.Controller);
					}
				}
			}
			else
			{
				// 선택된 모디파이어가 없을 때

				//이전
				//if (MeshGroup != null)
				//{
				//	//Exclusive 모두 해제
				//	MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					
				//	//변경 20.4.13
				//	Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	MeshGroup, 
				//														apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff, 
				//														apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

				//	//변경 21.2.15
				//	RefreshMeshGroupExEditingFlags(true);
				//}

				//이전
				//SetModifierEditMode(EX_EDIT_KEY_VALUE.None);


				//변경 22.5.14 : 하나로 최적화
				SetModifierExclusiveEditing(EX_EDIT.None);
				

				//아니면 말고 >> Gizmo 초기화
				Editor.Gizmos.Unlink();
			}


			//추가 22.6.10 : 컨트롤 파라미터 > PSG 매핑을 갱신한다.
			RefreshControlParam2PSGMapping();

			//삭제 22.5.14 : 위에서 이미 편집모드, 모디파이어 활성화를 모두 처리했다. (SetModifierExclusiveEditing 함수)
			//RefreshModifierExclusiveEditing();//<<Mod Lock 갱신 

			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}






		/// <summary>
		/// [MeshGroup 편집 화면에서] 선택된 모디파이어의 ParamSetGroup을 선택한다.
		/// </summary>
		/// <param name="paramSetGroup"></param>
		public void SelectParamSetGroupOfModifier(apModifierParamSetGroup paramSetGroup)
		{
			//AnimPack 선택은 여기서 무조건 해제된다.
			_subEditedParamSetGroupAnimPack = null;

			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_subEditedParamSetGroup = null;
				return;
			}
			bool isCheck = false;

			bool isChangedTarget = (_subEditedParamSetGroup != paramSetGroup);
			bool isTurnOffEditMode = false;

			if (_subEditedParamSetGroup != paramSetGroup)
			{
				_paramSetOfMod = null;
				//이전
				//_modMeshOfMod = null;
				//_modBoneOfMod = null;
				//_renderUnitOfMod = null;

				//변경
				_modData.ClearAll();

				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				//_exclusiveEditMode = EXCLUSIVE_EDIT_MODE.None;
				//_isExclusiveEditing = false;

				//삭제 22.5.14 : 아래에서 포괄적으로 하자
				//if (ExEditingMode == EX_EDIT.ExOnly_Edit)
				//{
				//	//SetModifierExclusiveEditing(false);
				//	SetModifierExclusiveEditing(EX_EDIT.None);
				//}

				//변경 22.5.14
				isTurnOffEditMode = true;
				
				isCheck = true;
			}
			_subEditedParamSetGroup = paramSetGroup;

			if (isCheck && SubEditedParamSetGroup != null)
			{
				bool isChanged = SubEditedParamSetGroup.RefreshSync();
				if (isChanged)
				{
					apUtil.LinkRefresh.Set_MeshGroup_Modifier(MeshGroup, _modifier);

					MeshGroup.LinkModMeshRenderUnits(apUtil.LinkRefresh);//<<이걸 먼저 선언한다.
					MeshGroup.RefreshModifierLink(apUtil.LinkRefresh);
				}
			}

			//변경 22.5.14
			if(isTurnOffEditMode)
			{
				//편집 모드 끄기
				SetModifierExclusiveEditing(EX_EDIT.None);
			}
			else
			{
				//편집 모드 유지한 상태에서 갱신
				AutoRefreshModifierExclusiveEditing();
			}

			//MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();

			//이전
			//RefreshModifierExclusiveEditing();//<<Mod Lock 갱신



			AutoSelectModMeshOrModBone();

			if (isChangedTarget)
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}
		}


		/// <summary>
		/// [MeshGroup 편집 화면에서] 모디파이어의 ParamSet을 선택한다.
		/// </summary>
		/// <param name="paramSetOfMod"></param>
		/// <param name="isIgnoreExEditable"></param>
		public void SelectParamSetOfModifier(apModifierParamSet paramSetOfMod, bool isIgnoreExEditable = false)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup || _modifier == null)
			{
				_paramSetOfMod = null;
				return;
			}

			bool isChanged = false;
			if (_paramSetOfMod != paramSetOfMod)
			{
				isChanged = true;
			}
			_paramSetOfMod = paramSetOfMod;

			//이전
			//RefreshModifierExclusiveEditing(isIgnoreExEditable);//<<Mod Lock 갱신

			//변경 22.5.14
			AutoRefreshModifierExclusiveEditing();

			AutoSelectModMeshOrModBone();

			if (isChanged)
			{
				Editor.Gizmos.RevertFFD(false);//<<변경 : Refresh -> Revert (강제)
			}
		}

		/// <summary>
		/// [MeshGroup 편집 화면에서]
		/// MeshGroup->Modifier->ParamSetGroup을 선택한 상태에서 ParamSet을 선택하지 않았다면,
		/// Modifier의 종류에 따라 ParamSet을 선택한다. (라고 하지만 Controller 입력 타입만 해당한다..)
		/// </summary>
		public void AutoSelectParamSetOfModifier()
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _portrait == null
				|| _meshGroup == null
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				|| _paramSetOfMod != null)//<<ParamSet이 이미 선택되어도 걍 리턴한다.
			{
				return;
			}

			apEditorUtil.ReleaseGUIFocus();

			apModifierParamSet targetParamSet = null;
			switch (_modifier.SyncTarget)
			{
				case apModifierParamSetGroup.SYNC_TARGET.Controller:
					{
						if (_subEditedParamSetGroup._keyControlParam != null)
						{
							apControlParam controlParam = _subEditedParamSetGroup._keyControlParam;
							//해당 ControlParam이 위치한 곳과 같은 값을 가지는 ParamSet이 있으면 이동한다.
							switch (_subEditedParamSetGroup._keyControlParam._valueType)
							{
								case apControlParam.TYPE.Int:
									{
										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return controlParam._int_Cur == a._conSyncValue_Int;
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._int_Cur = targetParamSet._conSyncValue_Int;
										}
									}
									break;

								case apControlParam.TYPE.Float:
									{
										float fSnapSize = Mathf.Abs(controlParam._float_Max - controlParam._float_Min) / controlParam._snapSize;
										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return Mathf.Abs(controlParam._float_Cur - a._conSyncValue_Float) < (fSnapSize * 0.25f);
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._float_Cur = targetParamSet._conSyncValue_Float;
										}
									}
									break;

								case apControlParam.TYPE.Vector2:
									{
										float vSnapSizeX = Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) / controlParam._snapSize;
										float vSnapSizeY = Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) / controlParam._snapSize;

										targetParamSet = _subEditedParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
										{
											return Mathf.Abs(controlParam._vec2_Cur.x - a._conSyncValue_Vector2.x) < (vSnapSizeX * 0.25f)
												&& Mathf.Abs(controlParam._vec2_Cur.y - a._conSyncValue_Vector2.y) < (vSnapSizeY * 0.25f);
										});

										//선택할만한게 있으면 아예 Control Param값을 동기화
										if (targetParamSet != null)
										{
											controlParam._vec2_Cur = targetParamSet._conSyncValue_Vector2;
										}
									}
									break;
							}
						}
					}
					break;
				default:
					//그 외에는.. 적용되는게 없어요
					break;
			}

			if (targetParamSet != null)
			{
				_paramSetOfMod = targetParamSet;

				AutoSelectModMeshOrModBone();

				//Editor.RefreshControllerAndHierarchy();
				Editor.Gizmos.RevertFFDTransformForce();//<추가
			}

		}



		/// <summary>
		/// 추가 22.6.10 : 컨트롤 파라미터 UI를 위해서 "현재 선택된 모디파이어"에 등록된 컨트롤 파라미터 정보를 매핑한다.
		/// PSG를 추가하거나 모디파이어를 선택할 때 이 함수를 항상 호출한다.
		/// 단, MeshGroup 편집 상태가 아니라면 나타나지 않는다.
		/// </summary>
		public void RefreshControlParam2PSGMapping()
		{
			if (_subControlParam2ParamSetGroup == null)
			{
				_subControlParam2ParamSetGroup = new Dictionary<apControlParam, apModifierParamSetGroup>();
			}
			_subControlParam2ParamSetGroup.Clear();

			if (_selectionType != SELECTION_TYPE.MeshGroup) { return; }
			if (Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Modifier) { return; }
			if (_modifier == null) { return; }
			if (_modifier.SyncTarget != apModifierParamSetGroup.SYNC_TARGET.Controller) { return; }

			int nPSGs = _modifier._paramSetGroup_controller != null ? _modifier._paramSetGroup_controller.Count : 0;
			
			apModifierParamSetGroup curPSG = null;
			apControlParam curControlParam = null;

			for (int i = 0; i < nPSGs; i++)
			{
				curPSG = _modifier._paramSetGroup_controller[i];
				curControlParam = curPSG._keyControlParam;

				if(curControlParam == null)
				{
					continue;
				}

				if(_subControlParam2ParamSetGroup.ContainsKey(curControlParam))
				{
					continue;
				}

				//컨트롤 파라미터 > 현재 모디파이어의 PSG를 매핑한다.
				_subControlParam2ParamSetGroup.Add(curControlParam, curPSG);
			}
		}




		// Mod Render Vertex 선택하기

		/// <summary>
		/// [MeshGroup 편집 화면에서] 모디파이어의 ModRenderVert를 선택한다.
		/// </summary>
		/// <param name="modRenderVert"></param>
		public void SelectModRenderVertOfModifier(ModRenderVert modRenderVert)//변경 20.6.25 : MRV를 미리 만들어두고 선택하는 방식
		{
			//Debug.LogError("TODO : SetModVertexOfModifier 고칠것");
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				//|| _modMeshOfMod == null
				|| ModMesh_Main == null
				)
			{
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();
				return;
			}

			AutoSelectModMeshOrModBone();

			bool isInitReturn = false;
			
			//MRV를 선택하는 경우라면 MRP는 무조건 선택 해제 (22.4.6)
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();


			//변경 20.6.25 : MRV 직접 확인
			if(modRenderVert == null)
			{
				isInitReturn = true;
			}

			if (isInitReturn)
			{
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();
				return;
			}

			bool isChangeModVert = false;
			//기존의 ModRenderVert를 유지할 것인가 또는 새로 선택(생성)할 것인가
			
			if (_modRenderVert_Main != null)
			{
				//변경 20.6.25
				if(modRenderVert != _modRenderVert_Main)
				{
					isChangeModVert = true;
				}
			}
			else
			{
				isChangeModVert = true;
			}

			if (isChangeModVert)
			{
				//변경 20.6.25
				_modRenderVert_Main = modRenderVert;

				//리스트를 갱신한다.
				_modRenderVerts_All.Clear();
				_modRenderVerts_All.Add(_modRenderVert_Main);

				_modRenderVerts_Weighted.Clear();
			}
		}


		/// <summary>
		/// [MeshGroup 편집 화면에서] 
		/// ModVert를 더 선택하여 Mod-Render Vertex를 추가한다.
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		public void AddModRenderVertOfModifier(ModRenderVert modRenderVert)//변경 20.6.25 : MRV를 직접 설정
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			//변경 20.6.25
			if(modRenderVert == null)
			{
				return;
			}

			//MRV를 선택하는 경우라면 MRP는 무조건 선택 해제 [v1.4.0] (22.4.6)
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();


			//변경 20.6.25 : MRV 직접 비교
			bool isExistSame = _modRenderVerts_All.Contains(modRenderVert);

			if (!isExistSame)
			{
				//변경 20.6.25 : MRV를 그대로 사용
				_modRenderVerts_All.Add(modRenderVert);

				if (_modRenderVerts_All.Count == 1)
				{
					_modRenderVert_Main = modRenderVert;
				}

				//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
				if(_modRenderVerts_Weighted != null 
					&& _modRenderVerts_Weighted.Count > 0
					&& _modRenderVerts_Weighted.Contains(modRenderVert))
				{
					_modRenderVerts_Weighted.Remove(modRenderVert);
				}
			}
		}

		/// <summary>
		/// [MeshGroup 편집 화면에서] 여러개의 ModRenderVert를 추가로 선택한다.
		/// </summary>
		/// <param name="modRenderVerts"></param>
		public void AddModRenderVertsOfModifier(List<ModRenderVert> modRenderVerts)//변경 20.6.25 : MRV를 직접 설정
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modRenderVerts == null || modRenderVerts.Count == 0)
			{
				return;
			}


			//MRV를 선택하는 경우라면 MRP는 무조건 선택 해제 [v1.4.0] (22.4.6)
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();


			//변경 20.6.25 : MRV 직접 비교
			//입력이 ModVertRig이므로, 이걸 MRV로 변환해야 한다.
			ModRenderVert targetMRV = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				targetMRV = modRenderVerts[i];

				if(targetMRV == null)
				{
					continue;
				}

				bool isExistSame = _modRenderVerts_All.Contains(targetMRV);

				if (!isExistSame)
				{
					//변경 20.6.25 : MRV를 그대로 사용
					_modRenderVerts_All.Add(targetMRV);

					if (_modRenderVerts_All.Count == 1)
					{
						_modRenderVert_Main = targetMRV;
					}

					//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
					if(_modRenderVerts_Weighted != null 
						&& _modRenderVerts_Weighted.Count > 0
						&& _modRenderVerts_Weighted.Contains(targetMRV))
					{
						_modRenderVerts_Weighted.Remove(targetMRV);
					}
				}
			}
		}

		//추가 20.6.26 : Rigging 툴로부터 ModVertRig 리스트를 받아서 추가/삭제할 수 있다.
		/// <summary>
		/// [MeshGroup 편집 화면에서] ModVertRig를 추가로 선택한다.
		/// </summary>
		/// <param name="modVertRigs"></param>
		public void AddModVertRigsOfModifier(List<apModifiedVertexRig> modVertRigs)//변경 20.6.25 : MRV를 직접 설정
		{
			//Debug.LogError("TODO : AddModVertexOfModifier 고칠것");

			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				//|| _modMeshOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modVertRigs == null || modVertRigs.Count == 0)
			{
				return;
			}


			//MRV를 선택하는 경우라면 MRP는 무조건 선택 해제 [v1.4.0] (22.4.6)
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();


			//변경 20.6.25 : MRV 직접 비교
			//입력이 ModVertRig이므로, 이걸 MRV로 변환해야 한다.
			apModifiedVertexRig curModVertRig = null;
			ModRenderVert targetMRV = null;
			for (int i = 0; i < modVertRigs.Count; i++)
			{
				curModVertRig = modVertRigs[i];
				targetMRV = _modData.GetMRV(curModVertRig);

				if(targetMRV == null)
				{
					continue;
				}

				bool isExistSame = _modRenderVerts_All.Contains(targetMRV);

				if (!isExistSame)
				{
					//변경 20.6.25 : MRV를 그대로 사용
					_modRenderVerts_All.Add(targetMRV);

					if (_modRenderVerts_All.Count == 1)
					{
						_modRenderVert_Main = targetMRV;
					}
				}
			}
		}


		//ModVertWeight 리스트를 입력하여 한번에 MRV들을 선택한다.
		/// <summary>
		/// [MeshGroup 편집 화면에서] Mod Vert Weight들을 추가로 선택한다. MRV_Weighted로 생성되어 선택된다.
		/// </summary>
		/// <param name="modVertWeights"></param>
		public void AddModVertWeights(List<apModifiedVertexWeight> modVertWeights)
		{
			
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				//|| _modMeshOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modVertWeights == null || modVertWeights.Count == 0)
			{
				return;
			}

			//MRV를 선택하는 경우라면 MRP는 무조건 선택 해제 [v1.4.0] (22.4.6)
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();


			//변경 20.6.25 : MRV 직접 비교
			//입력이 ModVertRig이므로, 이걸 MRV로 변환해야 한다.
			apModifiedVertexWeight curModVertWeight = null;
			ModRenderVert targetMRV = null;
			for (int i = 0; i < modVertWeights.Count; i++)
			{
				curModVertWeight = modVertWeights[i];
				targetMRV = _modData.GetMRV(curModVertWeight);

				if(targetMRV == null)
				{
					continue;
				}

				bool isExistSame = _modRenderVerts_All.Contains(targetMRV);

				if (!isExistSame)
				{
					//변경 20.6.25 : MRV를 그대로 사용
					_modRenderVerts_All.Add(targetMRV);

					if (_modRenderVerts_All.Count == 1)
					{
						_modRenderVert_Main = targetMRV;
					}

					//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
					if(_modRenderVerts_Weighted != null 
						&& _modRenderVerts_Weighted.Count > 0
						&& _modRenderVerts_Weighted.Contains(targetMRV))
					{
						_modRenderVerts_Weighted.Remove(targetMRV);
					}
				}
			}
		}


		

		//TODO : 이 함수가 호출되는 코드 자체를 효율적으로 수정해야한다.
		//MRV를 매번 생성하지 말고, 생성된 MRV를 선택하는 것으로 변경할 것 

		/// <summary>
		/// Mod-Render Vertex를 삭제한다. [Modifier 수정작업시]
		/// ModVert, ModVertRig, ModVertWeight 중 값 하나를 넣어줘야 한다.
		/// </summary>
		public void RemoveModVertexOfModifier(ModRenderVert modRenderVert)//변경 20.6.25 : MRV를 직접 설정
		{
			//Debug.LogError("TODO : RemoveModVertexOfModifier 고칠것");

			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				//|| _modMeshOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			//AutoSelectModMesh();//<<여기선 생략

			if(modRenderVert == null)
			{
				return;
			}
			
			
			//변경 20.6.25
			if(_modRenderVert_Main == modRenderVert)
			{
				_modRenderVert_Main = null;
			}
			_modRenderVerts_All.Remove(modRenderVert);

			//메인이 해제되었다면
			//남은 것 중에서 Main을 설정하자
			if (_modRenderVert_Main == null
				&& _modRenderVerts_All.Count > 0)
			{
				_modRenderVert_Main = _modRenderVerts_All[0];
			}
		}




		/// <summary>
		/// 선택된 ModVertRig들을 MRV로부터 여러개를 동시에 선택해제한다.
		/// </summary>
		/// <param name="modVertRigs"></param>
		public void RemoveModVertRigs(List<apModifiedVertexRig> modVertRigs)//변경 20.6.25 : MRV를 직접 설정
		{
			//Debug.LogError("TODO : RemoveModVertexOfModifier 고칠것");

			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modVertRigs == null || modVertRigs.Count == 0)
			{
				return;
			}
			
			
			//변경 20.6.25 : MRV 직접 비교
			//입력이 ModVertRig이므로, 이걸 MRV로 변환해야 한다.
			apModifiedVertexRig curModVertRig = null;
			ModRenderVert targetMRV = null;
			for (int i = 0; i < modVertRigs.Count; i++)
			{
				curModVertRig = modVertRigs[i];
				targetMRV = _modData.GetMRV(curModVertRig);

				if (targetMRV == null)
				{
					continue;
				}

				if(_modRenderVert_Main == targetMRV)
				{
					_modRenderVert_Main = null;
				}
				_modRenderVerts_All.Remove(targetMRV);
			}

			
			//메인이 해제되었다면
			//남은 것 중에서 Main을 설정하자
			if (_modRenderVert_Main == null
				&& _modRenderVerts_All.Count > 0)
			{
				_modRenderVert_Main = _modRenderVerts_All[0];
			}
		}


		/// <summary>
		/// 선택된 ModVertWeight들을 MRV로부터 여러개를 동시에 선택해제한다.
		/// </summary>
		public void RemoveModVertWeights(List<apModifiedVertexWeight> modVertWeights)//변경 20.6.25 : MRV를 직접 설정
		{
			//Debug.LogError("TODO : RemoveModVertexOfModifier 고칠것");

			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modVertWeights == null || modVertWeights.Count == 0)
			{
				return;
			}
			
			
			//변경 20.6.25 : MRV 직접 비교
			//입력이 ModVertRig이므로, 이걸 MRV로 변환해야 한다.
			apModifiedVertexWeight curModVertWeight = null;
			ModRenderVert targetMRV = null;
			for (int i = 0; i < modVertWeights.Count; i++)
			{
				curModVertWeight = modVertWeights[i];
				targetMRV = _modData.GetMRV(curModVertWeight);

				if (targetMRV == null)
				{
					continue;
				}

				if(_modRenderVert_Main == targetMRV)
				{
					_modRenderVert_Main = null;
				}
				_modRenderVerts_All.Remove(targetMRV);
			}


			//메인이 해제되었다면
			//남은 것 중에서 Main을 설정하자
			if (_modRenderVert_Main == null
				&& _modRenderVerts_All.Count > 0)
			{
				_modRenderVert_Main = _modRenderVerts_All[0];
			}
		}



		// 추가 22.4.6 [v1.4.0] : MRP 선택 함수들

		/// <summary>Mod Render Pin을 선택한다. null 입력시 선택 해제</summary>
		/// <param name="modRenderPin"></param>
		public void SelectModRenderPinOfModifier(ModRenderPin modRenderPin)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();
				return;
			}

			AutoSelectModMeshOrModBone();

			bool isInitReturn = false;
			
			//MRP를 선택하는 경우라면 MRV는 무조건 선택 해제 (22.4.6)
			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();


			//변경 20.6.25 : MRV 직접 확인
			if(modRenderPin == null)
			{
				isInitReturn = true;
			}

			if (isInitReturn)
			{
				//Pin 선택 초기화
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();
				return;
			}

			bool isChangeModPin = false;
			
			//기존의 ModRenderPin를 유지할 것인가 또는 새로 선택(생성)할 것인가
			
			if (_modRenderPin_Main != null)
			{
				if(modRenderPin != _modRenderPin_Main)
				{
					isChangeModPin = true;//다른 선택
				}
			}
			else
			{
				isChangeModPin = true;//새로 선택
			}

			if (isChangeModPin)
			{
				_modRenderPin_Main = modRenderPin;

				//리스트를 갱신한다.
				_modRenderPins_All.Clear();
				_modRenderPins_All.Add(_modRenderPin_Main);

				_modRenderPins_Weighted.Clear();
			}
		}

		/// <summary>[MeshGroup 편집 화면에서] Mod Render Pin을 추가로 선택한다.</summary>
		public void AddModRenderPinOfModifier(ModRenderPin modRenderPin)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modRenderPin == null) { return; }

			//MRP를 선택하는 경우라면 MRV는 무조건 선택 해제 (22.4.6)
			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();

			//이미 추가되어 있다면
			bool isExistSame = _modRenderPins_All.Contains(modRenderPin);

			if (!isExistSame)
			{
				//선택되어 있지 않다면
				_modRenderPins_All.Add(modRenderPin);

				if (_modRenderPins_All.Count == 1)
				{
					_modRenderPin_Main = modRenderPin;
				}

				//선택된 핀은 Weighted 리스트에서는 제외해야한다.
				if(_modRenderPins_Weighted != null 
					&& _modRenderPins_Weighted.Count > 0
					&& _modRenderPins_Weighted.Contains(modRenderPin))
				{
					_modRenderPins_Weighted.Remove(modRenderPin);
				}
			}
		}

		/// <summary> [MeshGroup 편집 화면에서] 여러개의 Mod Render Pin 들을 추가로 선택한다. </summary>
		public void AddModRenderPinsOfModifier(List<ModRenderPin> modRenderPins)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modRenderPins == null || modRenderPins.Count == 0)
			{
				return;
			}


			//MRP를 선택하는 경우라면 MRV는 무조건 선택 해제 (22.4.6)
			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();


			ModRenderPin targetMRP = null;
			int nInputMRPs = modRenderPins.Count;
			for (int i = 0; i < nInputMRPs; i++)
			{
				targetMRP = modRenderPins[i];

				if(targetMRP == null) { continue; }

				bool isExistSame = _modRenderPins_All.Contains(targetMRP);

				if (!isExistSame)
				{
					//이미 추가된게 아니라면
					_modRenderPins_All.Add(targetMRP);

					if (_modRenderPins_All.Count == 1)
					{
						_modRenderPin_Main = targetMRP;
					}

					//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
					if(_modRenderPins_Weighted != null 
						&& _modRenderPins_Weighted.Count > 0
						&& _modRenderPins_Weighted.Contains(targetMRP))
					{
						_modRenderPins_Weighted.Remove(targetMRP);
					}
				}
			}
		}


		/// <summary>
		/// Mod Render Pin의 선택을 해제한다. (일부만 해제하는 경우)
		/// </summary>
		/// <param name="modRenderVert"></param>
		public void RemoveModPinOfModifier(ModRenderPin modRenderPin)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _paramSetOfMod == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			//AutoSelectModMesh();//<<여기선 생략

			if(modRenderPin == null) { return; }
			
			if(_modRenderPin_Main == modRenderPin)
			{
				_modRenderPin_Main = null;
			}
			_modRenderPins_All.Remove(modRenderPin);

			//메인이 해제되었다면
			//남은 것 중에서 Main을 설정하자
			if (_modRenderPin_Main == null
				&& _modRenderPins_All.Count > 0)
			{
				_modRenderPin_Main = _modRenderPins_All[0];
			}
		}



		//-------------------------------------------------------------



		//MeshTransform(MeshGroupT)이 선택되어있다면 자동으로 ParamSet 내부의 ModMesh를 선택한다.
		public void AutoSelectModMeshOrModBone()
		{
			//0. ParamSet까지 선택이 안되었다면 아무것도 선택 불가
			//1. ModMesh를 선택할 수 있는가
			//2. ModMesh의 유효한 선택이 없다면 ModBone 선택이 가능한가
			//거기에 맞게 처리
			apEditorUtil.ReleaseGUIFocus();

			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _meshGroup == null
				|| _modifier == null
				|| _paramSetOfMod == null
				|| _subEditedParamSetGroup == null
				)
			{
				//아무것도 선택하지 못할 경우

				//이전
				//_modMeshOfMod = null;
				//_modBoneOfMod = null;
				//_renderUnitOfMod = null;

				//변경 20.6.10 : 래핑된 Mod 데이터들
				_modData.ClearAll();

				_modRegistableBones.Clear();

				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();
				

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				_linkedToModBones.Clear();//ModMesh와 연결된 본들도 리셋
				_prevRenderUnit_CheckLinkedToModBones = null;

				return;
			}



			
			//변경된 방식 20.6.10 : 래핑+다중 선택을 지원한다.
			//다중 선택된 오브젝트(_selectedSubObjects)로 부터 동기화를 하자
			bool isChanged = _modData.SyncModData_ModEdit();

			
			if(!_modData.IsAnyModMesh || !_modData.IsAnyRenderUnit || isChanged)
			{
				//ModMesh를 선택하지 않았다면..
				// or RenderUnit이 바뀌었다면 > MorRenderVertOfMod 취소
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0]
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();
			}



			//ModMesh나 ModBone을 선택한 다음에,
			//ModBone으로 선택 가능한 Bone 리스트를 만들어준다.
			_modRegistableBones.Clear();

			for (int i = 0; i < _paramSetOfMod._boneData.Count; i++)
			{
				_modRegistableBones.Add(_paramSetOfMod._boneData[i]._bone);
			}

			//추가 : 이 ModMesh와 연결된 본들을 리스트로 모으자
			CheckLinkedToModMeshBones(false);

			//MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}



		/// <summary>
		/// 추가 20.3.28 : 현재 작업의 대상이 되는 본들을 리스트로 저장한다.
		/// 리깅 모디파이어를 선택한 상태에서 ModMesh에 연결된 본들을 선택한다.
		/// 리깅은 다중 선택을 지원하지 않으므로, 메인 선택된 MeshTF와 ModMesh만 체크하자
		/// </summary>
		public void CheckLinkedToModMeshBones(bool isForce)
		{
			if (_modifier == null ||

				//_subMeshTransformInGroup == null ||//이전
				_subObjects.MeshTF == null ||//변경 20.5.27 : 래핑
				
				//_modMeshOfMod == null || _renderUnitOfMod == null//이전
				_modData.ModMesh == null || _modData.RenderUnit == null//변경 20.6.11 : 래핑
				)
			{
				if (_linkedToModBones.Count > 0)
				{
					_linkedToModBones.Clear();
				}
				_prevRenderUnit_CheckLinkedToModBones = null;
				return;
			}

			apTransform_Mesh targetMeshTF = _subObjects.MeshTF;
			apModifiedMesh targetModMesh = _modData.ModMesh;
			apRenderUnit targetRenderUnit = _modData.RenderUnit;


			if (_modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging
				//이전 : 래핑 전
				//|| !_modMeshOfMod._isMeshTransform
				//|| _modMeshOfMod._transform_Mesh == null
				//|| _renderUnitOfMod._meshTransform == null
				//|| _renderUnitOfMod._meshTransform != _modMeshOfMod._transform_Mesh

				//변경 20.6.11 : 래핑 후
				|| !targetModMesh._isMeshTransform
				|| targetModMesh._transform_Mesh == null
				|| targetRenderUnit._meshTransform == null
				|| targetRenderUnit._meshTransform != targetModMesh._transform_Mesh
				|| targetRenderUnit._meshTransform != targetMeshTF
				)
			{
				if (_linkedToModBones.Count > 0)
				{
					_linkedToModBones.Clear();
				}
				_prevRenderUnit_CheckLinkedToModBones = null;

				//Debug.LogError("TODO : ModMesh 동기화가 잘못되었다. - _linkedToModBones 설정 실패");

				return;
			}

			if (!isForce
				&& _prevRenderUnit_CheckLinkedToModBones == targetRenderUnit)
			{
				//렌더 유닛이 이전 처리와 동일하다면 패스하자.
				//Debug.LogWarning("Pass >>>");
				return;
			}

			_prevRenderUnit_CheckLinkedToModBones = targetRenderUnit;
			_linkedToModBones.Clear();

			if (//이전
				//_modMeshOfMod._vertRigs == null ||
				//_modMeshOfMod._vertRigs.Count == 0

				//변경 20.6.11
				targetModMesh._vertRigs == null ||
				targetModMesh._vertRigs.Count == 0
				)
			{
				return;
			}



			int nVert = targetModMesh._vertRigs.Count;
			apModifiedVertexRig curModVertRig = null;

			int nWeightPairs = 0;
			apBone curBone = null;

			for (int iVert = 0; iVert < nVert; iVert++)
			{
				curModVertRig = targetModMesh._vertRigs[iVert];
				if (curModVertRig == null)
				{
					continue;
				}
				nWeightPairs = curModVertRig._weightPairs.Count;

				for (int iPair = 0; iPair < nWeightPairs; iPair++)
				{
					curBone = curModVertRig._weightPairs[iPair]._bone;
					if (!_linkedToModBones.ContainsKey(curBone))
					{
						//리깅으로 등록된 본을 리스트에 넣는다.
						_linkedToModBones.Add(curBone, true);
					}
				}
			}

		}

		/// <summary>
		/// 추가 20.3.29
		/// 에디터의 옵션 (_rigGUIOption_NoLinkedBoneVisibility)에 따라 "모디파이어에 연결되지 않은" 본들을 렌더링이나 선택에서 제외할 필요가 있는데, 그때 LinkedToModifierBones를 이용해야한다.
		/// 본을 렌더링하거나 선택할 때 이 함수의 값이 true라면 LinkedToModifierBones를 이용하자
		/// </summary>
		/// <returns></returns>
		public bool IsCheckableToLinkedToModifierBones()
		{
			//1. 리깅 화면에서 리깅 편집 중일때
			if (SelectionType == apSelection.SELECTION_TYPE.MeshGroup
						&& Modifier != null
						&& IsRigEditBinding
						
						//이전
						//&& ModMeshOfMod != null
						//&& RenderUnitOfMod != null
						
						//변경 20.6.11 : 래핑
						&& _modData.ModMesh != null
						&& _modData.RenderUnit != null


						&& LinkedToModifierBones.Count > 0
						)
			{
				//모디파이어와 모드 메시가 선택된 상태일 때.
				if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					return true;
				}
			}
			return false;
		}



		//추가 20.7.3 : Undo시 MRV가 초기화되는 문제가 있다.
		//Undo직후 선택된 상태의 MRV를 저장했다가 복구를 하는 기능을 제공한다.
		/// <summary>
		/// [Undo시 호출] 선택된 MRV를 임시로 저장한다. (RenderVert를 키값으로 한다.)
		/// (추가 22.4.6) MRP도 추가
		/// </summary>
		public void StoreSelectedModRenderVertsPins_ForUndo()
		{
			//Debug.Log("StoreSelectedModRenderVerts_ForUndo");
			//int nStoreVert = 0;

			//일단 초기화
			_isAnyMRVStoredToRecover = false;

			_recoverVert2MRV_MainMeshTF = null;
			_recoverVert2MRV_MainVert = null;
			if (_recoverVert2MRV_All == null) { _recoverVert2MRV_All = new Dictionary<apTransform_Mesh, List<apVertex>>(); }
			_recoverVert2MRV_All.Clear();

			//추가 22.4.6 [v1.4.0] MRP도 저장&복구하자
			_recoverPin2MRP_MainPin = null;
			if(_recoverPin2MRP_All == null) { _recoverPin2MRP_All = new Dictionary<apTransform_Mesh, List<apMeshPin>>(); }
			_recoverPin2MRP_All.Clear();

			bool isAnyMRVSelected = _modRenderVert_Main != null && _modRenderVerts_All.Count > 0;
			bool isAnyMRPSelected = _modRenderPin_Main != null && _modRenderPins_All.Count > 0;

			//저장하는 값이 없다.
			if(!isAnyMRVSelected && !isAnyMRPSelected)
			{
				return;
			}

			//추가 20.9.13 : SoftSelection에 해당하는 MRV는 제외한다. > ?? 이 문제를 재현하기가 어렵다.
			//bool isWeightedMRVExist = _modRenderVerts_Weighted != null && _modRenderVerts_Weighted.Count > 0;

			//키값은 MeshTF와 Vertex 두개를 저장하자
			//RenderVertex는 휘발성이므로 사용하면 안된다.

			
			if (isAnyMRVSelected)
			{
				//<1> Mod Render Vertex를 이용하는 경우
				if (_modRenderVert_Main != null
				&& _modRenderVert_Main._renderVert != null
				&& _modRenderVert_Main._modVert != null
				&& _modRenderVert_Main._modVert._modifiedMesh != null
				&& _modRenderVert_Main._modVert._modifiedMesh._transform_Mesh != null)
				{
					_recoverVert2MRV_MainMeshTF = _modRenderVert_Main._modVert._modifiedMesh._transform_Mesh;
					_recoverVert2MRV_MainVert = _modRenderVert_Main._renderVert._vertex;
				}


				if (_modRenderVerts_All != null && _modRenderVerts_All.Count > 0)
				{
					ModRenderVert curMRV = null;
					apTransform_Mesh curMeshTF = null;
					for (int iMRV = 0; iMRV < _modRenderVerts_All.Count; iMRV++)
					{
						curMRV = _modRenderVerts_All[iMRV];
						if (curMRV == null
							|| curMRV._renderVert == null
							|| curMRV._renderVert._vertex == null
							|| curMRV._modVert == null
							|| curMRV._modVert._modifiedMesh == null
							|| curMRV._modVert._modifiedMesh._transform_Mesh == null)
						{
							continue;
						}

						curMeshTF = curMRV._modVert._modifiedMesh._transform_Mesh;

						if (!_recoverVert2MRV_All.ContainsKey(curMeshTF))
						{
							_recoverVert2MRV_All.Add(curMeshTF, new List<apVertex>());
						}
						_recoverVert2MRV_All[curMeshTF].Add(curMRV._renderVert._vertex);
					}
				}
			}
			else
			{
				//<2> Mod Render Pin을 이용하는 경우
				if (_modRenderPin_Main != null
				&& _modRenderPin_Main._renderPin != null
				&& _modRenderPin_Main._modPin != null
				&& _modRenderPin_Main._modPin._modifiedMesh != null
				&& _modRenderPin_Main._modPin._modifiedMesh._transform_Mesh != null)
				{
					_recoverVert2MRV_MainMeshTF = _modRenderPin_Main._modPin._modifiedMesh._transform_Mesh;
					_recoverPin2MRP_MainPin = _modRenderPin_Main._renderPin._srcPin;
				}


				if (_modRenderPins_All != null && _modRenderPins_All.Count > 0)
				{
					ModRenderPin curMRP = null;
					apTransform_Mesh curMeshTF = null;
					for (int iMRP = 0; iMRP < _modRenderPins_All.Count; iMRP++)
					{
						curMRP = _modRenderPins_All[iMRP];
						if (curMRP == null
							|| curMRP._renderPin == null
							|| curMRP._renderPin._srcPin == null
							|| curMRP._modPin == null
							|| curMRP._modPin._modifiedMesh == null
							|| curMRP._modPin._modifiedMesh._transform_Mesh == null)
						{
							continue;
						}

						curMeshTF = curMRP._modPin._modifiedMesh._transform_Mesh;

						if (!_recoverPin2MRP_All.ContainsKey(curMeshTF))
						{
							_recoverPin2MRP_All.Add(curMeshTF, new List<apMeshPin>());
						}
						_recoverPin2MRP_All[curMeshTF].Add(curMRP._renderPin._srcPin);
					}
				}
			}
			
			_isAnyMRVStoredToRecover = true;

			//Debug.LogWarning("> Stored : " + nStoreVert);
		}



		/// <summary>
		/// [Undo시 호출] 저장되었던 "선택된 MRV"를 복구한다.
		/// 이미 선택된 MRV가 하나라도 있다면 복구를 포기한다.
		/// </summary>
		public void RecoverSelectedModRenderVerts_ForUndo()
		{
			//복구 조건 확인
			bool isRecoverable_MRV = true;
			bool isRecoverable_MRP = true;

			if (!_isAnyMRVStoredToRecover
				|| _recoverVert2MRV_MainMeshTF == null)
			{
				//공통적으로 저장된게 없다.
				isRecoverable_MRV = false;
				isRecoverable_MRP = false;
			}

			if (_recoverVert2MRV_MainVert == null
				|| _recoverVert2MRV_All == null
				|| _recoverVert2MRV_All.Count == 0)
			{
				//MRV에 대해서 저장된게 없다.
				isRecoverable_MRV = false;
			}

			if (_recoverPin2MRP_MainPin == null
				|| _recoverPin2MRP_All == null
				|| _recoverPin2MRP_All.Count == 0)
			{
				//MRP에 대해서 저장된게 없다.
				isRecoverable_MRP = false;
			}



			//갱신 후 복구할 대상이 없거나
			if (_modData.NumModRenderVert_All == 0)
			{
				//저장과 무관하게 선택을 복구할 MRV가 없다.
				isRecoverable_MRV = false;
			}

			if (_modData.NumModRenderPin_All == 0)
			{
				//저장과 무관하게 선택을 복구할 MRP가 없다.
				isRecoverable_MRP = false;
			}


			//이미 다른게 선택되었다면 (이건 어느하나라도 선택되면 모두 해제)
			if (_modRenderVert_Main != null
				|| _modRenderVerts_All.Count > 0
				|| _modRenderPin_Main != null
				|| _modRenderPins_All.Count > 0)
			{
				isRecoverable_MRV = false;
				isRecoverable_MRP = false;
			}


			if (isRecoverable_MRV)
			{
				//<1> MRV의 선택 정보를 복구한다면
				ModRenderVert curMRV = null;

				//Debug.Log("Recover : Target MRVs " + _modData.NumModRenderVert_All);

				//메인이 유효한 경우에만 복구를 한다.
				//메인도 없다면 다른건 검사할 필요가 없다.

				curMRV = _modData.GetMRV(_recoverVert2MRV_MainMeshTF, _recoverVert2MRV_MainVert);
				if (curMRV != null)
				{
					_modRenderVert_Main = curMRV;
					_modRenderVerts_All.Add(_modRenderVert_Main);

					//메인이 유효한 경우에만 복구를 한다.
					//메인도 없다면 다른건 검사할 필요가 없다.
					apTransform_Mesh meshTF = null;
					List<apVertex> vertList = null;
					foreach (KeyValuePair<apTransform_Mesh, List<apVertex>> mesh2VertList in _recoverVert2MRV_All)
					{
						meshTF = mesh2VertList.Key;
						vertList = mesh2VertList.Value;

						for (int iVert = 0; iVert < vertList.Count; iVert++)
						{
							curMRV = _modData.GetMRV(meshTF, vertList[iVert]);

							if (curMRV != null && !_modRenderVerts_All.Contains(curMRV))
							{
								_modRenderVerts_All.Add(curMRV);
							}
						}
					}

				}

				_modRenderVerts_Weighted.Clear();//<<Weighted 리스트는 초기화하자

				//이 경우엔 MRP는 모두 초기화
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();
			}
			else if (isRecoverable_MRP)
			{
				//<2> MRP의 선택 정보를 복구한다면
				ModRenderPin curMRP = null;

				//메인이 유효한 경우에만 복구를 한다.
				//메인도 없다면 다른건 검사할 필요가 없다.

				curMRP = _modData.GetMRP(_recoverVert2MRV_MainMeshTF, _recoverPin2MRP_MainPin);
				if (curMRP != null)
				{
					_modRenderPin_Main = curMRP;
					_modRenderPins_All.Add(_modRenderPin_Main);

					//메인이 유효한 경우에만 복구를 한다.
					//메인도 없다면 다른건 검사할 필요가 없다.
					apTransform_Mesh meshTF = null;
					List<apMeshPin> pinList = null;
					foreach (KeyValuePair<apTransform_Mesh, List<apMeshPin>> mesh2PinList in _recoverPin2MRP_All)
					{
						meshTF = mesh2PinList.Key;
						pinList = mesh2PinList.Value;

						for (int iPin = 0; iPin < pinList.Count; iPin++)
						{
							curMRP = _modData.GetMRP(meshTF, pinList[iPin]);

							if (curMRP != null && !_modRenderPins_All.Contains(curMRP))
							{
								_modRenderPins_All.Add(curMRP);
							}
						}
					}

				}

				_modRenderPins_Weighted.Clear();//<<Weighted 리스트는 초기화하자

				//이 경우엔 MRV는 모두 초기화
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();
			}


			//저장되었던 값들 모두 해제

			_isAnyMRVStoredToRecover = false;
			_recoverVert2MRV_MainMeshTF = null;
			_recoverVert2MRV_MainVert = null;
			if(_recoverVert2MRV_All == null) { _recoverVert2MRV_All = new Dictionary<apTransform_Mesh, List<apVertex>>(); }
			_recoverVert2MRV_All.Clear();

			_recoverPin2MRP_MainPin = null;
			if(_recoverPin2MRP_All == null) { _recoverPin2MRP_All = new Dictionary<apTransform_Mesh, List<apMeshPin>>(); }
			_recoverPin2MRP_All.Clear();
		}




		//-----------------------------------------------------------------------
		// 애니메이션 편집시의 함수들
		//-----------------------------------------------------------------------
		
		/// <summary>
		/// AnimClip 상태에서 현재 상태에 맞는 GizmoEvent를 등록한다.
		/// 만약 이전의 연결 상태와 같다면 억지로 리셋하지 않도록 한다.
		/// </summary>
		private void SetAnimClipGizmoEvent(
											//bool isForceReset//일단 이건 사용하지 않는다.
											)
		{
			if (_animClip == null)
			{
				//AnimClip이 없다면 항상 Unlink
				Editor.Gizmos.Unlink();
				_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.None;
				return;
			}

			
			//이전
			//if (isForceReset)
			//{
			//	Editor.Gizmos.Unlink();
			//}


			//[v1.4.2] 이전에는 옵션에 따라 무조건 Unlink를 했지만,
			//많은 경우에 Unlink를 하면 안되는 상황 (Timeline 등이 바뀌지 않은 상태)임에도 계속 Unlink를 하여
			//마우스 동작이 계속 초기화되는 문제가 있었다.
			//그래서 Timeline과 Gizmo 연결 상태를 별도의 변수로 저장하여, 그것이 바뀔 때에만 Unlink를 수행하도록 한다.
			//즉, "인자에 의한 수동 리셋" 에서 "상태 확인 후 자동 리셋"으로 개선됨

			if (AnimTimeline == null)
			{
				//타임라인이 없으면 선택만 가능하다
				if(_animGizmoLinkedStatus != ANIM_GIZMO_LINKED_STATUS.NoTimeline)
				{
					_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.NoTimeline;

					Editor.Gizmos.Unlink();
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_OnlySelectTransform());
				}
			}
			else
			{
				switch (AnimTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						if (AnimTimeline._linkedModifier != null)
						{
							if ((int)(AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
							{
								//Vertex와 관련된 Modifier다.
								if(_animGizmoLinkedStatus != ANIM_GIZMO_LINKED_STATUS.AnimMod_EditVertex)
								{
									_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.AnimMod_EditVertex;

									Editor.Gizmos.Unlink();
									Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditVertex());
								}
							}
							else if(AnimTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedColorOnly)
							{
								//추가 21.7.20 : 애니메이션 색상 모디파이어
								if(_animGizmoLinkedStatus != ANIM_GIZMO_LINKED_STATUS.AnimMod_EditColor)
								{
									_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.AnimMod_EditColor;

									Editor.Gizmos.Unlink();
									Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditColorOnly());
								}
							}
							else
							{
								//Transform과 관련된 Modifier다.		
								if(_animGizmoLinkedStatus != ANIM_GIZMO_LINKED_STATUS.AnimMod_EditTF)
								{
									_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.AnimMod_EditTF;

									Editor.Gizmos.Unlink();
									Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditTransform());
								}
							}
						}
						else
						{
							if (_animGizmoLinkedStatus != ANIM_GIZMO_LINKED_STATUS.UnknownTimeline)
							{
								_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.UnknownTimeline;

								Debug.LogError("Error : 선택된 Timeline의 Modifier가 연결되지 않음");
								Editor.Gizmos.Unlink();
							}
						}
						break;

					//이거 삭제하고, 
					//GetEventSet__Animation_EditTransform에서 Bone을 제어하는 코드를 추가하자
					//case apAnimClip.LINK_TYPE.Bone:
					//	Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_EditBone());
					//	break;

					case apAnimClip.LINK_TYPE.ControlParam:						
						{
							//Control Param일땐 선택만 가능
							if (_animGizmoLinkedStatus != ANIM_GIZMO_LINKED_STATUS.ControlParam)
							{
								_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.ControlParam;

								Editor.Gizmos.Unlink();
								Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet__Animation_OnlySelectTransform());
							}
						}
						break;

					default:
						{
							if (_animGizmoLinkedStatus != ANIM_GIZMO_LINKED_STATUS.None)
							{
								_animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.None;

								Debug.LogError("TODO : 알 수 없는 Timeline LinkType [" + AnimTimeline._linkType + "]");
								Editor.Gizmos.Unlink();
							}
						}
						break;
				}
			}

		}



		/// <summary>
		/// [Animation 편집시] AnimClip -> Timeline 을 선택한다. (단일 선택)
		/// </summary>
		/// <param name="timeLine"></param>
		public void SelectAnimTimeline(apAnimTimeline timeLine,
										bool isKeyframeSelectReset,
										bool isIgnoreLock = false,
										bool isAutoChangeLeftTab = true)
		{
			//통계 재계산 요청
			SetStatisticsRefresh();

			if (!isIgnoreLock)
			{
				//현재 작업중 + Lock이 걸리면 바꾸지 못한다.
				if (ExAnimEditingMode != EX_EDIT.None && IsAnimSelectionLock)
				{
					return;
				}
			}



			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				timeLine == null ||
				!_animClip.IsTimelineContain(timeLine))
			{
				_subAnimTimeline = null;

				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				
				//변경 20.6.11
				_subObjects.Clear();

				
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimSelectionLock = false;

				_animTimelineCommonCurve.Clear();//추가 3.30

				
				//변경 20.6.11
				_modData.ClearAll();

				
				//변경 20.6.29 : Mod랑 통합되었다.
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0] MRP
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				bool isWorkKeyframeChanged = false;
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
				
				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//이 부분까지도 FFD가 해제되지 않았다면 강제로 Revert (Adapt 여부는 앞에서 물어봤어야 한다.)
					Editor.Gizmos.RevertFFDTransformForce();
				}
				
				//RefreshAnimEditing(true);//이전
				AutoRefreshModifierExclusiveEditing();//변경 22.5.15

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, null, null);
				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결 : 선택된게 없으므로 그냥 초기화

				//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, false);//"GUI Anim Hierarchy Delayed - Meshes"
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, false);//"GUI Anim Hierarchy Delayed - Bone"
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam, false);//"GUI Anim Hierarchy Delayed - ControlParam"

				return;
			}

			if (_subAnimTimeline != timeLine)
			{
				//변경 20.6.11 : 래핑
				_subObjects.ClearTimelineLayers();

				if (isKeyframeSelectReset)
				{
					_subAnimKeyframe = null;

					_subAnimKeyframeList.Clear();

					_animTimelineCommonCurve.Clear();//추가 3.30
				}

				//변경 20.6.11 : 래핑
				_modData.ClearAll();

				//변경 20.6.29 : Mod랑 통합되었다.
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0] MRP
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				bool isWorkKeyframeChanged = false;
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//이 부분까지도 FFD가 해제되지 않았다면 강제로 Revert (Adapt 여부는 앞에서 물어봤어야 한다.)
					Editor.Gizmos.RevertFFDTransformForce();
				}

				//Editing에서 바꿀 수 있으므로 AnimEditing를 갱신한다.
				//RefreshAnimEditing(true);
				AutoRefreshModifierExclusiveEditing();//변경 22.5.15

				//스크롤 초기화 (오른쪽2)
				Editor.ResetScrollPosition(false, false, false, true, false);
			}

			_subAnimTimeline = timeLine;


			AutoSelectAnimTimelineLayer(false, isAutoChangeLeftTab);

			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);//<<Timeline을 선택하는 것 만으로는 크게 바뀌는게 없다.

			SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();

			//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, false);//"GUI Anim Hierarchy Delayed - Meshes"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, false);//"GUI Anim Hierarchy Delayed - Bone"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam, false);//"GUI Anim Hierarchy Delayed - ControlParam"
																										   //Editor.SetGUIVisible("AnimationRight2GUI_Timeline_Layers", false);

			apEditorUtil.ReleaseGUIFocus();
		}


		/// <summary>
		/// [애니메이션 편집시]
		/// Timeline Layer를 선택한다.
		/// </summary>
		/// <param name="timelineLayer"></param>
		/// <param name="multiSelect"></param>
		/// <param name="isKeyframeSelectReset"></param>
		/// <param name="isAutoSelectTargetObject"></param>
		/// <param name="isIgnoreLock"></param>
		public void SelectAnimTimelineLayer(	apAnimTimelineLayer timelineLayer, 
												MULTI_SELECT multiSelect,
												bool isKeyframeSelectReset, 
												bool isAutoSelectTargetObject = false, 
												bool isIgnoreLock = false)
		{
			//Debug.Log("SetAnimTimelineLayer : " + (timelineLayer != null ? timelineLayer.DisplayName : "<Null>"));

			apAnimTimeline prevTimeline = _subAnimTimeline;

			//처리 후 이전 레이어
			//통계 재계산 요청
			SetStatisticsRefresh();

			//현재 작업중+Lock이 걸리면 바꾸지 못한다.
			if (!isIgnoreLock)
			{
				if (ExAnimEditingMode != EX_EDIT.None && IsAnimSelectionLock)
				{
					return;
				}
			}

			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				_subAnimTimeline == null ||
				timelineLayer == null ||
				!_subAnimTimeline.IsTimelineLayerContain(timelineLayer)
				)
			{
				//Debug.LogError("<Not Select>");

				//_subAnimTimelineLayer = null;//이전
				_subObjects.ClearTimelineLayers();//변경 20.6.11

				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();//추가 3.30

				//이전
				//_modMeshOfAnim = null;
				//_modBoneOfAnim = null;
				//_renderUnitOfAnim = null;

				//변경 20.6.11 : 래핑
				_modData.ClearAll();

				//이전
				//_modRenderVertOfAnim = null;
				//_modRenderVertListOfAnim.Clear();
				//_modRenderVertListOfAnim_Weighted.Clear();

				//변경 20.6.29 : Mod랑 통합되었다.
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0] MRP
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();

				bool isWorkKeyframeChanged = false;
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
				
				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//이 부분까지도 FFD가 해제되지 않았다면 강제로 Revert (Adapt 여부는 앞에서 물어봤어야 한다.)
					Editor.Gizmos.RevertFFDTransformForce();
				}



				//Editing에서 바꿀 수 있으므로 AnimEditing를 갱신한다.
				//RefreshAnimEditing(true);
				AutoRefreshModifierExclusiveEditing();//변경 22.5.15

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);
				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

				return;
			}

			//변경 20.6.11 : 래핑
			bool isChanged = _subObjects.SelectAnimTimelineLayer(timelineLayer, multiSelect);
			
			//변경 20.6.20
			//1. 이 코드는 대체로 _subObjects.SelectAnimTimelineLayer에서도 항상 작동한다.
			//2. 위치를 바꾸었다.
			if (isAutoSelectTargetObject)
			{
				AutoSelectAnimTargetObject();
			}

			
			if(isChanged && isKeyframeSelectReset)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();

				bool isWorkKeyframeChanged = false;
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
				
				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//Timeline Layer 선택시 Work Keyframe의 변경이 생겼다면
					//FFD 여부를 물어봐야한다. 단, 여기서는 Cancel이 불가능하다.
					Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();

				}

				//RefreshAnimEditing(true);
				AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}


			


			//이전. 함수 호출 시점이 위로 옮겨졌다.
			//if (isAutoSelectTargetObject)
			//{
			//	AutoSelectAnimTargetObject();
			//}


			//선택하는 것 만으로는 변경되는게 없다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);

			SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

			//만약 처리 이전-이후의 타임라인이 그대로라면 GUI가 깜빡이는걸 막자
			if (prevTimeline == _subAnimTimeline)
			{
				_isIgnoreAnimTimelineGUI = true;
			}

			apEditorUtil.ReleaseGUIFocus();
		}


		

		/// <summary>
		/// [애니메이션 편집시]
		/// 여러개의 타임라인 레이어를 선택하는데, "비활성 > 활성" 또는 "활성 > 비활성"으로만 선택한다.
		/// 각각이 무조건 토글되는건 아니다.
		/// 기존의 선택이 유지되는 AddOrSubtract 타입이다. (처리 실패시 그냥 리턴)
		/// </summary>
		/// <param name="timelineLayers"></param>
		/// <param name="multiSelect"></param>
		/// <param name="isKeyframeSelectReset"></param>
		/// <param name="isAutoSelectTargetObject"></param>
		/// <param name="isIgnoreLock"></param>
		public void SelectAnimTimelineLayersAddable(	List<apAnimTimelineLayer> timelineLayers, 
														bool isDeselected2Selected,
														bool isKeyframeSelectReset
										)
		{
			//Debug.Log("SetAnimTimelineLayer : " + (timelineLayer != null ? timelineLayer.DisplayName : "<Null>"));

			apAnimTimeline prevTimeline = _subAnimTimeline;

			//처리 후 이전 레이어
			//통계 재계산 요청
			SetStatisticsRefresh();

			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| _subAnimTimeline == null
				|| timelineLayers == null
				|| timelineLayers.Count == 0
				//|| !_subAnimTimeline.IsTimelineLayerContain(timelineLayer)
				)
			{
				//처리 실패
				return;
			}


			//FFD 모드가 켜져있다면 강제로 종료한다. [v1.4.2]
			if(Editor.Gizmos.IsFFDMode)
			{
				Editor.Gizmos.RevertFFDTransformForce();
			}

			//여러개의 타임라인 레이어를 "추가" 또는 "해제"한다.
			_subObjects.SelectAnimTimelineLayersAddable(timelineLayers, isDeselected2Selected);
			
			
			if(isKeyframeSelectReset)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();

				bool isWorkKeyframeChanged = false;
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				//RefreshAnimEditing(true);
				AutoRefreshModifierExclusiveEditing();//변경 22.5.15
			}

			//선택하는 것 만으로는 변경되는게 없다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);

			SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

			//만약 처리 이전-이후의 타임라인이 그대로라면 GUI가 깜빡이는걸 막자
			if (prevTimeline == _subAnimTimeline)
			{
				_isIgnoreAnimTimelineGUI = true;
			}

			apEditorUtil.ReleaseGUIFocus();
		}


		
		/// <summary>
		/// Timeline GUI에서 Keyframe을 선택한다.
		/// AutoSelect를 켜면 선택한 Keyframe에 맞게 다른 TimelineLayer / Timeline을 선택한다.
		/// 이 함수는단일 선택이므로 "다중 선택"은 항상 현재 선택한 것만 가지도록 한다.
		/// </summary>
		/// <param name="keyframe"></param>
		/// <param name="isTimelineAutoSelect"></param>
		public void SelectAnimKeyframe(	apAnimKeyframe keyframe,
										bool isTimelineAutoSelect,
										apGizmos.SELECT_TYPE selectType,
										bool isSelectLoopDummy = false)
		{

			bool isWorkKeyframeChanged = false;

			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				//_subAnimTimelineLayer = null;//이전
				_subObjects.ClearTimelineLayers();//변경 20.6.11

				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();//추가 3.30

				
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결 + Unlink

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);
				return;
			}

			apAnimTimeline prevTimeline = _subAnimTimeline;

			if (selectType != apGizmos.SELECT_TYPE.New)
			{
				List<apAnimKeyframe> singleKeyframes = new List<apAnimKeyframe>();
				if (keyframe != null)
				{
					singleKeyframes.Add(keyframe);
				}

				SelectAnimMultipleKeyframes(singleKeyframes, selectType, isTimelineAutoSelect);
				return;
			}



			if (keyframe == null)
			{
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();//추가 3.30

				//선택된 키프레임이 없다면 Gizmo는 무조건 리셋이다.
				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결
				return;
			}

			bool isKeyframeChanged = (keyframe != _subAnimKeyframe);



			//단일 선택 + 타임라인 선택
			if (isTimelineAutoSelect)
			{
				//[타임 라인을 자동으로 선택할 때]

				//Layer가 선택되지 않았거나, 선택된 Layer에 포함되지 않을 때
				apAnimTimelineLayer parentLayer = keyframe._parentTimelineLayer;
				if (parentLayer == null)
				{
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();

					_animTimelineCommonCurve.Clear();//추가 3.30

					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//FFD 적용 여부를 물어보자 다만, 취소는 없다.
						//(취소가 있는 질문을 하고자 했다면 기능의 초반에 직접 체크하여 호출하자)
						Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
					}

					return;
				}
				apAnimTimeline parentTimeline = parentLayer._parentTimeline;
				if (parentTimeline == null || !_animClip.IsTimelineContain(parentTimeline))
				{
					//유효하지 않은 타임라인일때
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();

					_animTimelineCommonCurve.Clear();//추가 3.30

					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//FFD 적용 여부를 물어보자 다만, 취소는 없다.
						//(취소가 있는 질문을 하고자 했다면 기능의 초반에 직접 체크하여 호출하자)
						Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
					}

					SetAnimClipGizmoEvent();//Gizmo 이벤트 연결
					return;
				}

				//자동으로 체크해주자
				_subAnimTimeline = parentTimeline;
				
				//이전
				//_subAnimTimelineLayer = parentLayer;
				
				//변경 20.6.11 : 한개의 키프레임만 선택하므로 타임라인 레이어도 하나만 선택한다.
				_subObjects.SelectAnimTimelineLayer(parentLayer, MULTI_SELECT.Main);

				_subAnimKeyframe = keyframe;
				_subAnimKeyframeList.Clear();
				_subAnimKeyframeList.Add(keyframe);

				_animTimelineCommonCurve.Clear();//추가 3.30

				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//FFD 적용 여부를 물어보자 다만, 취소는 없다.
					//(취소가 있는 질문을 하고자 했다면 기능의 초반에 직접 체크하여 호출하자)
					Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
				}
				
				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, parentLayer, null);//<<변경 19.5.21 : 선택하는 것 만으로는 변경되지 않도록
			}
			else
			{
				//[타임 라인을 자동으로 선택하지 않을 때]
				//= TimelineLayer에 있는 키프레임만 선택할 때
				if (_subAnimTimeline == null ||
					//_subAnimTimelineLayer == null//이전
					_subObjects.TimelineLayer == null//변경 20.6.11
					)
				{
					//적절한 타임라인 레이어가 없을 때
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();

					_animTimelineCommonCurve.Clear();//추가 3.30

					SetAnimClipGizmoEvent();//Gizmo 이벤트 연결
					return;//처리 못함
				}

				//타임라인 레이어에 포함된 키프레임인가
				//기존 : 단일 선택 키프레임이므로 1개의 타임라인에 키프레임이 있는지만 테스트하자
				//변경 20.6.11 : 선택된 모든 타임라인에 포함되어 있는지 확인한다.
				//if (_subAnimTimelineLayer.IsKeyframeContain(keyframe))
				if(_subObjects.IsContainKeyframInAllTimelineLayers(keyframe))
				{
					//Layer에 포함된 Keyframe이다.
					_subAnimKeyframe = keyframe;
					_subAnimKeyframeList.Clear();
					_subAnimKeyframeList.Add(_subAnimKeyframe);
				}
				else
				{
					//Layer에 포함되지 않은 Keyframe이다. => 처리 못함
					_subAnimKeyframe = null;
					_subAnimKeyframeList.Clear();
				}
				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결


				_animTimelineCommonCurve.Clear();//추가 3.30

			}

			_subAnimKeyframe._parentTimelineLayer.SortAndRefreshKeyframes();


			//키프레임 선택시 자동으로 Frame을 이동한다.
			if (_subAnimKeyframe != null)
			{
				int selectedFrameIndex = _subAnimKeyframe._frameIndex;
				if (_animClip.IsLoop &&
					(selectedFrameIndex < _animClip.StartFrame || selectedFrameIndex > _animClip.EndFrame))
				{
					selectedFrameIndex = _subAnimKeyframe._loopFrameIndex;
				}

				if (selectedFrameIndex >= _animClip.StartFrame
					&& selectedFrameIndex <= _animClip.EndFrame)
				{
					_animClip.SetFrame_Editor(selectedFrameIndex);
					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}


				SetAutoAnimScroll();
			}

			if (isKeyframeChanged)
			{
				AutoSelectAnimTargetObject();
			}

			AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//<Work+Mod 자동 연결

			if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
			{
				//FFD 적용 여부를 물어보자 다만, 취소는 없다.
				//(취소가 있는 질문을 하고자 했다면 기능의 초반에 직접 체크하여 호출하자)
				Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
			}


			SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();

			//만약 처리 이전-이후의 타임라인이 그대로라면 GUI가 깜빡이는걸 막자
			if (prevTimeline == _subAnimTimeline)
			{
				_isIgnoreAnimTimelineGUI = true;
			}

			apEditorUtil.ReleaseGUIFocus();
		}


		/// <summary>
		/// Keyframe 다중 선택을 한다.
		/// 이때는 Timeline, Timelinelayer는 변동이 되지 않는다. (다만 다중 선택시에는 Timeline, Timelinelayer를 별도로 수정하지 못한다)
		/// </summary>
		/// <param name="keyframes"></param>
		/// <param name="selectType"></param>
		public void SelectAnimMultipleKeyframes(List<apAnimKeyframe> keyframes, apGizmos.SELECT_TYPE selectType, bool isTimelineAutoSelect)
		{
			bool isWorkKeyframeChanged = false;


			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				//_subAnimTimelineLayer = null;//이전
				_subObjects.ClearTimelineLayers();//변경 20.6.11
				
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

				_animTimelineCommonCurve.Clear();//추가 3.30

				return;
			}

			int nInputKeyframes = keyframes != null ? keyframes.Count : 0;

			//[v1.4.2]
			//선택된 키프레임이 없다면 타임라인 레이어 선택이 해제되는데
			//이러면 대충 "헛 영역"을 잡은 후에 선택된 객체(+Layer)가 풀리게 된다.
			//FFD 등 도구들이 해제되는 문제가 발생하므로, InputKeyframe이 없을땐 처리하지 말자
			if(nInputKeyframes == 0)
			{
				return;
			}
			

			apAnimKeyframe curKeyframe = null;
			if (selectType == apGizmos.SELECT_TYPE.New)
			{
				//_subAnimWorkKeyframe = null;//삭제 20.6.11 > 어차피 아래서 갱신함.

				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();
			}

			_animTimelineCommonCurve.Clear();//추가 3.30


			//키프레임들을 선택했을때, 기즈모 이벤트 초기화를 검사하기 위해
			//FrameIndex, Timeline, Timeline Layer, WorkKeyframe이 변경되었는지를 체크한다.
			//사실 WorkKeyframe이 변경되었다면 Timeline + Layer가 변경된 것이므로,
			//FrameIndex만 같이 체크하자

			int prevFrameIndex = _animClip.CurFrame;



			if (nInputKeyframes > 0)
			{
				for (int i = 0; i < nInputKeyframes; i++)
				{
					curKeyframe = keyframes[i];
					if (curKeyframe == null ||
						curKeyframe._parentTimelineLayer == null ||
						curKeyframe._parentTimelineLayer._parentAnimClip != _animClip)
					{
						continue;
					}

					if (selectType == apGizmos.SELECT_TYPE.Add ||
						selectType == apGizmos.SELECT_TYPE.New)
					{
						//Debug.Log("Add");
						if (!_subAnimKeyframeList.Contains(curKeyframe))
						{
							_subAnimKeyframeList.Add(curKeyframe);
						}
					}
					else
					{
						_subAnimKeyframeList.Remove(curKeyframe);
					}
				}
			}
			

			if (_subAnimKeyframeList.Count > 0)
			{
				if (!_subAnimKeyframeList.Contains(_subAnimKeyframe))
				{
					_subAnimKeyframe = _subAnimKeyframeList[0];
				}
			}
			else
			{
				_subAnimKeyframe = null;
			}




			if (isTimelineAutoSelect)
			{
				//공통된 타임라인 레이어가 있는 경우
				//해당 타임라인 레이어를 선택한다.
				
				//이슈 20.6.11
				//여러개의 레이어를 걸친다면 > (1) 모든 레이어를 선택할 것인가? / (2) 공통된 레이어만 선택할 것인가?
				//> "모든 레이어를 선택한다."로 결정
				//대신, 가능한 메인은 유지해야한다.
				
				//- 1. 공통된 타임라인이 없다. : 타임라인 레이어 선택 해제 (키프레임만 선택한다.)
				//- 2. 공통된 타임라인이 있다.
				//- 2-1. 타임라인 레이어가 한개 : 해당 타임라인 레이어가 메인이다.
				//- 2-2. 타임라인 레이어가 여러개 : "기존 메인 레이어"를 가능한 유지한 상태에서 레이어들을 선택한다.

				

				//변경 20.6.11 : 공통 타임라인 체크를 여기서 하자. 원래는 위에서 함
				apAnimTimeline commonTimeline = null;
				List<apAnimTimelineLayer> commonTimelineLayers = new List<apAnimTimelineLayer>();//추가 20.6.11

				if(_subAnimKeyframeList.Count > 0)
				{
					apAnimTimelineLayer curTimelineLayer = null;
					for (int i = 0; i < _subAnimKeyframeList.Count; i++)
					{
						curKeyframe = _subAnimKeyframeList[i];
						curTimelineLayer = curKeyframe._parentTimelineLayer;
						if(commonTimelineLayers.Count == 0)
						{
							//첫 입력이다.
							commonTimeline = curTimelineLayer._parentTimeline;
							commonTimelineLayers.Add(curTimelineLayer);
						}
						else
						{
							//기존에 입력된 것과 비교하자
							if(commonTimeline != null && 
								commonTimeline != curTimelineLayer._parentTimeline)
							{
								//타임라인이 다르다면..
								commonTimeline = null;//아예 null (공통이 아니니까..)
							}

							if(!commonTimelineLayers.Contains(curTimelineLayer))
							{
								//없는거라면 추가
								commonTimelineLayers.Add(curTimelineLayer);
							}
						}
					}
				}

				//이제 조건별로 타임라임/타임라인레이어를 선택하자
				if (commonTimelineLayers.Count == 0 || commonTimeline == null)
				{
					//- 1. 공통된 타임라인이 없다. : 타임라인 레이어 선택 해제 (키프레임만 선택한다.)
					_subObjects.ClearTimelineLayers();
					if (ExAnimEditingMode == EX_EDIT.None)
					{
						_subAnimTimeline = null;
					}
					Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);

				}
				else if (commonTimelineLayers.Count == 1)
				{
					//- 2. 공통된 타임라인이 있다. 
					//> 2-1. 타임라인 레이어가 한개 : 해당 타임라인 레이어가 메인이다.
					apAnimTimelineLayer commonTimelineLayer = commonTimelineLayers[0];
					
					//타임라인 레이어 1개만 메인으로 설정되어야 하는 경우
					if (commonTimelineLayer != _subObjects.TimelineLayer
						|| _subObjects.AllTimelineLayers.Count != 1)
					{
						_subObjects.SelectAnimTimelineLayer(commonTimelineLayer, MULTI_SELECT.Main);//메인으로 설정

						if (ExAnimEditingMode == EX_EDIT.None)
						{
							_subAnimTimeline = commonTimeline;
						}

						Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, commonTimelineLayer, null);
					}
				}
				else
				{
					//- 2. 공통된 타임라인이 있다.
					//> 2-2. 타임라인 레이어가 여러개 : "기존 메인 레이어"를 가능한 유지한 상태에서 레이어들을 선택한다.
					_subObjects.SelectAnimTimelineLayers(commonTimelineLayers, commonTimeline);//메인으로 설정

					if (ExAnimEditingMode == EX_EDIT.None)
					{
						_subAnimTimeline = commonTimeline;
					}

					Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, commonTimelineLayers);//여러개의 레이어들을 대상으로 갱신
				}
			}
			else
			{
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);
			}

			List<apAnimTimelineLayer> refreshLayer = new List<apAnimTimelineLayer>();
			for (int i = 0; i < _subAnimKeyframeList.Count; i++)
			{
				if (!refreshLayer.Contains(_subAnimKeyframeList[i]._parentTimelineLayer))
				{
					refreshLayer.Add(_subAnimKeyframeList[i]._parentTimelineLayer);
				}
			}
			for (int i = 0; i < refreshLayer.Count; i++)
			{
				refreshLayer[i].SortAndRefreshKeyframes();
			}



			//키프레임 선택시 자동으로 Frame을 이동한다.
			//단, 공통 프레임이 있는 경우에만 이동한다.
			if (_subAnimKeyframeList.Count > 0 && selectType == apGizmos.SELECT_TYPE.New)
			{
				bool isCommonKeyframe = true;

				int selectedFrameIndex = -1;
				for (int iKey = 0; iKey < _subAnimKeyframeList.Count; iKey++)
				{
					apAnimKeyframe subKeyframe = _subAnimKeyframeList[iKey];
					if (iKey == 0)
					{
						selectedFrameIndex = subKeyframe._frameIndex;
						isCommonKeyframe = true;
					}
					else
					{
						if (subKeyframe._frameIndex != selectedFrameIndex)
						{
							//선택한 키프레임이 다 다르군요. 자동 이동 포기
							isCommonKeyframe = false;
							break;
						}
					}

				}
				if (isCommonKeyframe)
				{
					//모든 키프레임이 공통의 프레임을 갖는다.
					//이동하자
					if (_animClip.IsLoop &&
						(selectedFrameIndex < _animClip.StartFrame || selectedFrameIndex > _animClip.EndFrame))
					{
						selectedFrameIndex = _subAnimKeyframe._loopFrameIndex;
					}

					if (selectedFrameIndex >= _animClip.StartFrame
						&& selectedFrameIndex <= _animClip.EndFrame)
					{
						_animClip.SetFrame_Editor(selectedFrameIndex);
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}


					SetAutoAnimScroll();
				}
			}

			
			AutoSelectAnimTargetObject();
			
			AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//<Work+Mod 자동 연결

			//FFD 완료 (취소 없음)
			if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
			{
				Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
			}
			
			SetAnimClipGizmoEvent();//Gizmo 이벤트 연결 (기존 true > v1.4.2 변경 isWorkKeyframeChanged)

			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();

			//추가 3.30 : 여러개의 키프레임들을 선택했을 때, CommonCurve도 갱신하자
			_animTimelineCommonCurve.Clear();//추가 3.30
			if (_subAnimKeyframeList.Count > 1)
			{
				_animTimelineCommonCurve.SetKeyframes(_subAnimKeyframeList);
			}

			apEditorUtil.ReleaseGUIFocus();
		}



		public void AutoRefreshCommonCurve()
		{
			//추가 3.30 : 여러개의 키프레임들을 선택했을 때, CommonCurve도 갱신하자
			_animTimelineCommonCurve.Clear();//추가 3.30
			if (_subAnimKeyframeList != null && _subAnimKeyframeList.Count > 1)
			{
				_animTimelineCommonCurve.SetKeyframes(_subAnimKeyframeList);
			}
		}




		private void AutoSelectAnimTargetObject()
		{
			//자동으로 타겟을 정하자

			//변경 20.6.11 : 통합
			_subObjects.AutoSelectObjectsFromTimelineLayers();
		}

		//---------------------------------------------------------------

		/// <summary>
		/// Keyframe의 변동사항이 있을때 Common Keyframe을 갱신한다.
		/// </summary>
		public void RefreshCommonAnimKeyframes()
		{


			if (_animClip == null)
			{
				_subAnimCommonKeyframeList.Clear();
				_subAnimCommonKeyframeList_Selected.Clear();
				return;
			}

			//0. 전체 Keyframe과 FrameIndex를 리스트로 모은다.
			List<int> commFrameIndexList = new List<int>();
			List<apAnimKeyframe> totalKeyframes = new List<apAnimKeyframe>();
			apAnimTimeline timeline = null;
			apAnimTimelineLayer timelineLayer = null;
			apAnimKeyframe keyframe = null;
			for (int iTimeline = 0; iTimeline < _animClip._timelines.Count; iTimeline++)
			{
				timeline = _animClip._timelines[iTimeline];
				for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
				{
					timelineLayer = timeline._layers[iLayer];
					for (int iKeyframe = 0; iKeyframe < timelineLayer._keyframes.Count; iKeyframe++)
					{
						keyframe = timelineLayer._keyframes[iKeyframe];

						//키프레임과 프레임 인덱스를 저장
						totalKeyframes.Add(keyframe);

						if (!commFrameIndexList.Contains(keyframe._frameIndex))
						{
							commFrameIndexList.Add(keyframe._frameIndex);
						}
					}
				}
			}

			//기존의 AnimCommonKeyframe에서 불필요한 것들을 먼저 없애고, 일단 Keyframe을 클리어한다.
			_subAnimCommonKeyframeList.RemoveAll(delegate (apAnimCommonKeyframe a)
			{
				//공통적으로 존재하지 않는 FrameIndex를 가진다면 삭제
				return !commFrameIndexList.Contains(a._frameIndex);
			});

			for (int i = 0; i < _subAnimCommonKeyframeList.Count; i++)
			{
				_subAnimCommonKeyframeList[i].Clear();
				_subAnimCommonKeyframeList[i].ReadyToAdd();
			}




			//1. Keyframe들의 공통 Index를 먼저 가져온다.
			for (int iKF = 0; iKF < totalKeyframes.Count; iKF++)
			{
				keyframe = totalKeyframes[iKF];

				apAnimCommonKeyframe commonKeyframe = GetCommonKeyframe(keyframe._frameIndex);

				if (commonKeyframe == null)
				{
					commonKeyframe = new apAnimCommonKeyframe(keyframe._frameIndex);
					commonKeyframe.ReadyToAdd();

					_subAnimCommonKeyframeList.Add(commonKeyframe);
				}

				//Common Keyframe에 추가한다.
				commonKeyframe.AddAnimKeyframe(keyframe, _subAnimKeyframeList.Contains(keyframe));
			}


			_subAnimCommonKeyframeList_Selected.Clear();

			//선택된 Common Keyframe만 처리한다.
			for (int i = 0; i < _subAnimCommonKeyframeList.Count; i++)
			{
				if (_subAnimCommonKeyframeList[i]._isSelected)
				{
					_subAnimCommonKeyframeList_Selected.Add(_subAnimCommonKeyframeList[i]);
				}
			}



		}

		public apAnimCommonKeyframe GetCommonKeyframe(int frameIndex)
		{
			return _subAnimCommonKeyframeList.Find(delegate (apAnimCommonKeyframe a)
			{
				return a._frameIndex == frameIndex;
			});
		}


		/// <summary>
		/// 공통 키프레임을 선택한다.
		/// </summary>
		/// <param name="commonKeyframe"></param>
		/// <param name="selectType"></param>
		public void SelectAnimCommonKeyframe(apAnimCommonKeyframe commonKeyframe, apGizmos.SELECT_TYPE selectType)
		{
			List<apAnimCommonKeyframe> commonKeyframes = new List<apAnimCommonKeyframe>();
			commonKeyframes.Add(commonKeyframe);
			SelectAnimCommonKeyframes(commonKeyframes, selectType);
		}

		



		/// <summary>
		/// SetAnimKeyframe과 비슷하지만 CommonKeyframe을 선택하여 다중 선택을 한다.
		/// SelectionType에 따라서 다르게 처리를 한다.
		/// TimelineAutoSelect는 하지 않는다.
		/// </summary>
		public void SelectAnimCommonKeyframes(List<apAnimCommonKeyframe> commonKeyframes, apGizmos.SELECT_TYPE selectType)
		{
			bool isWorkKeyframeChanged = false;

			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				_subAnimTimeline = null;
				

				//_subAnimTimelineLayer = null;//이전
				_subObjects.ClearTimelineLayers();//변경 20.6.11

				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();

				_subAnimCommonKeyframeList.Clear();
				_subAnimCommonKeyframeList_Selected.Clear();

				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//<Work+Mod 자동 연결

				SetAnimClipGizmoEvent();//Gizmo 이벤트 연결

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null, null);
				return;
			}

			if (selectType == apGizmos.SELECT_TYPE.New)
			{

				//New라면 다른 AnimKeyframe은 일단 취소해야하므로..
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();

				//Refresh는 처리 후 일괄적으로 한다.

				//New에선
				//일단 모든 CommonKeyframe의 Selected를 false로 돌린다.
				for (int i = 0; i < _subAnimCommonKeyframeList.Count; i++)
				{
					_subAnimCommonKeyframeList[i]._isSelected = false;
				}
				_subAnimCommonKeyframeList_Selected.Clear();
			}



			apAnimCommonKeyframe commonKeyframe = null;
			for (int iCK = 0; iCK < commonKeyframes.Count; iCK++)
			{
				commonKeyframe = commonKeyframes[iCK];
				if (selectType == apGizmos.SELECT_TYPE.New ||
					selectType == apGizmos.SELECT_TYPE.Add)
				{

					commonKeyframe._isSelected = true;
					for (int iSubKey = 0; iSubKey < commonKeyframe._keyframes.Count; iSubKey++)
					{
						apAnimKeyframe keyframe = commonKeyframe._keyframes[iSubKey];
						//Add / New에서는 리스트에 더해주자
						if (!_subAnimKeyframeList.Contains(keyframe))
						{
							_subAnimKeyframeList.Add(keyframe);
						}
					}
				}
				else
				{
					//Subtract에서는 선택된 걸 제외한다.
					commonKeyframe._isSelected = false;

					for (int iSubKey = 0; iSubKey < commonKeyframe._keyframes.Count; iSubKey++)
					{
						apAnimKeyframe keyframe = commonKeyframe._keyframes[iSubKey];

						_subAnimKeyframeList.Remove(keyframe);
					}
				}
			}

			if (_subAnimKeyframeList.Count > 0)
			{
				if (!_subAnimKeyframeList.Contains(_subAnimKeyframe))
				{
					_subAnimKeyframe = _subAnimKeyframeList[0];
				}

				if (_subAnimKeyframeList.Count == 1)
				{
					//타임라인 / 타임라인 레이어 교체
					//이전
					//_subAnimTimelineLayer = _subAnimKeyframe._parentTimelineLayer;
					//_subAnimTimeline = _subAnimTimelineLayer._parentTimeline;

					//변경 20.6.11
					_subObjects.SelectAnimTimelineLayer(_subAnimKeyframe._parentTimelineLayer, MULTI_SELECT.Main);
					_subAnimTimeline = _subAnimKeyframe._parentTimelineLayer._parentTimeline;
				}
				else
				{
					//_subAnimTimeline//<<이건 건들지 않는다.

					//이전
					//_subAnimTimelineLayer = null;
					
					//변경 20.6.11
					_subObjects.ClearTimelineLayers();
				}
			}
			else
			{
				_subAnimKeyframe = null;
			}

			//Common Keyframe을 갱신하자
			RefreshCommonAnimKeyframes();


			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);

			List<apAnimTimelineLayer> refreshLayer = new List<apAnimTimelineLayer>();
			for (int i = 0; i < _subAnimKeyframeList.Count; i++)
			{
				if (!refreshLayer.Contains(_subAnimKeyframeList[i]._parentTimelineLayer))
				{
					refreshLayer.Add(_subAnimKeyframeList[i]._parentTimelineLayer);
				}
			}
			for (int i = 0; i < refreshLayer.Count; i++)
			{
				refreshLayer[i].SortAndRefreshKeyframes();
			}

			AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//<Work+Mod 자동 연결

			if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
			{
				//FFD를 완료한다.
				Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
			}

			SetAnimClipGizmoEvent();//Gizmo 이벤트 연결 (기존 true > isWorkKeyframeChanged로 변경 v1.4.2)

			//추가 3.30 : 키프레임들이 여러개 선택된 경우, CommonCurve를 갱신한다.
			_animTimelineCommonCurve.Clear();
			if (_subAnimKeyframeList.Count > 1)
			{
				_animTimelineCommonCurve.SetKeyframes(_subAnimKeyframeList);
			}
		}

		/// <summary>
		/// Common Keyframes (Summary)를 유지한 상태에서 키프레임에 변경이 생겨서 커브 연결을 다시 해야 하는 경우
		/// </summary>
		public void SyncAnimCommonCurves()
		{
			//추가 3.30 : 키프레임들이 여러개 선택된 경우, CommonCurve를 갱신한다.
			_animTimelineCommonCurve.Clear();
			if (_subAnimKeyframeList.Count > 1)
			{
				_animTimelineCommonCurve.SetKeyframes(_subAnimKeyframeList);
			}
		}

		//---------------------------------------------------------------
		// 애니메이션 편집시의 서브 객체 선택
		//---------------------------------------------------------------
		/// <summary>
		/// AnimClip 작업을 위해 MeshTransform을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다.
		/// </summary>
		/// <param name="meshTransform"></param>
		public void SelectMeshTF_ForAnimEdit(apTransform_Mesh meshTransform, 
														bool isAutoSelectAnimTimelineLayer, 
														bool isAutoTimelineUIScroll,
														MULTI_SELECT multiSelect)
		{
			//변경 20.5.27 : 래핑
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subObjects.Clear();
				return;
			}

			_subObjects.Select(meshTransform, null, null, multiSelect, TF_BONE_SELECT.Exclusive);
			_subObjects.ClearControlParam();

			if (isAutoSelectAnimTimelineLayer)
			{
				AutoSelectAnimTimelineLayer(isAutoTimelineUIScroll);
			}
		}



		/// <summary>
		/// AnimClip 작업을 위해 MeshGroupTransform을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다.
		/// </summary>
		/// <param name="meshGroupTransform"></param>
		public void SelectMeshGroupTF_ForAnimEdit(apTransform_MeshGroup meshGroupTransform, 
															bool isAutoSelectAnimTimelineLayer, 
															bool isAutoTimelineUIScroll,
															MULTI_SELECT multiSelect)
		{
			//변경 20.5.27 : 래핑
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subObjects.Clear();
				return;
			}

			_subObjects.Select(null, meshGroupTransform, null, multiSelect, TF_BONE_SELECT.Exclusive);
			_subObjects.ClearControlParam();

			if (isAutoSelectAnimTimelineLayer)
			{
				AutoSelectAnimTimelineLayer(isAutoTimelineUIScroll);
			}
		}



		/// <summary>
		/// AnimClip 작업을 위해 Control Param을 선택한다.
		/// 해당 데이터가 Timeline에 없어도 선택 가능하다
		/// </summary>
		/// <param name="controlParam"></param>
		public void SelectControlParam_ForAnimEdit(apControlParam controlParam, bool isAutoSelectAnimTimelineLayer, bool isAutoTimelineUIScroll)
		{
			//변경 20.5.27 : 래핑 
			_subObjects.Clear();

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				return;
			}

			if(controlParam != null)
			{
				_subObjects.SelectControlParamForAnim(controlParam);
			}
			
			if (isAutoSelectAnimTimelineLayer)
			{
				AutoSelectAnimTimelineLayer(isAutoTimelineUIScroll);
			}
		}

		/// <summary>
		/// [애니메이션 편집시] 선택된 객체(Transform/Bone/ControlParam) 중에서 "현재 타임라인"이 선택할 수 있는 객체를 리턴한다.
		/// </summary>
		/// <returns></returns>
		public object GetSelectedAnimTimelineObject()
		{
			//???
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null ||
				_subAnimTimeline == null)
			{
				return null;
			}

			switch (_subAnimTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					return _subObjects.SelectedObject_WithoutControlParam;

				case apAnimClip.LINK_TYPE.ControlParam:
					return SelectedControlParamOnAnimClip;
			}
			return null;
		}


		/// <summary>
		/// 현재 선택한 Sub 객체 (Transform, Bone, ControlParam)에 따라서
		/// 자동으로 Timeline의 Layer를 선택해준다.
		/// </summary>
		/// <param name="isAutoChangeLeftTabToControlParam">이 값이 True이면 Timeline이 ControlParam 타입일때 자동으로 왼쪽 탭이 Controller로 바뀐다.</param>
		public void AutoSelectAnimTimelineLayer(bool isAutoTimelineScroll, bool isAutoChangeLeftTabToControlParam = true)
		{	
			bool isWorkKeyframeChanged = false;

			//수정 :
			//Timeline을 선택하지 않았다 하더라도 자동으로 선택을 할 수 있다.
			//수정작업중이 아니며 + 해당 오브젝트를 포함하는 Layer를 가진 Timeline의 개수가 1개일 땐 그것을 선택한다.
			if (_selectionType != SELECTION_TYPE.Animation ||
				_animClip == null)
			{
				// 아예 작업 불가
				_subAnimTimeline = null;
				
				//변경 20.6.11
				_subObjects.ClearTimelineLayers();

				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();//<<추가

				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				//FFD는 해제한다.
				if(Editor.Gizmos.IsFFDMode)
				{
					Editor.Gizmos.RevertFFDTransformForce();
				}

				//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, false);//"GUI Anim Hierarchy Delayed - Meshes"
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, false);//"GUI Anim Hierarchy Delayed - Bone"
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam, false);//"GUI Anim Hierarchy Delayed - ControlParam"
				
				//[v1.4.2] UI 상에서 "마지막으로 클릭한 타임라인 레이어"를 초기화한다.
				_lastClickTimelineLayer = null;

				return;
			}

			//변경 20.6.11 : 래핑을 하였다. << 여러가지로 중요!
			apAnimTimeline resultTimeline = _subObjects.AutoSelectTimelineLayers(_subAnimTimeline, _animClip, ExAnimEditingMode);

			if(resultTimeline == null)
			{
				//선택된 타임라인이 없는 경우
				_subAnimTimeline = null;
				_subObjects.ClearTimelineLayers();

				_subAnimTimeline = null;
				_subAnimKeyframe = null;
				_subAnimKeyframeList.Clear();

				_animTimelineCommonCurve.Clear();//<<추가

				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//취소 없이 FFD를 완료한다.
					Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
				}

				//[v1.4.2] UI 상에서의 "마지막 클릭 타임라인 레이어" 초기화
				_lastClickTimelineLayer = null;
			}
			else
			{
				//선택된 타임라인이 있는 경우
				bool isChanged = _subAnimTimeline != resultTimeline;
				_subAnimTimeline = resultTimeline;
				
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					//취소 없이 FFD를 완료한다.
					Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
				}

				//여기서는 아예 Work Keyframe 뿐만아니라 Keyframe으로도 선택을 한다.
				SelectAnimKeyframe(_subObjects.WorkKeyframe, false, apGizmos.SELECT_TYPE.New);//<<이것도 다중 처리?

				_modRegistableBones.Clear();//<<이것도 갱신해주자 [타임라인에 등록된 Bone]
				if (_subAnimTimeline != null)
				{
					for (int i = 0; i < _subAnimTimeline._layers.Count; i++)
					{
						apAnimTimelineLayer timelineLayer = _subAnimTimeline._layers[i];
						if (timelineLayer._linkedBone != null)
						{
							_modRegistableBones.Add(timelineLayer._linkedBone);
						}
					}
				}

				if (isChanged)
				{
					//timeline이 ControlParam계열이라면 에디터의 탭을 변경
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam
						&& isAutoChangeLeftTabToControlParam)
					{
						//옵션이 허용하는 경우 (19.6.28 변경)
						if (Editor._isAutoSwitchControllerTab_Anim)
						{
							Editor.SetLeftTab(apEditor.TAB_LEFT.Controller);
						}
					}
				}


				//변경 19.11.22 : 제한적 상황에서만 자동 스크롤
				if (isAutoTimelineScroll)
				{
					//자동으로 타임라인 UI을 스크롤한다면
					if (_subObjects.TimelineLayer != null)
					{
						_isAnimTimelineLayerGUIScrollRequest = true;//자동 스크롤을 켜자
					}
				}
			}

			//우측 Hierarchy GUI 변동이 있을 수 있으니 리셋
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, false);//"GUI Anim Hierarchy Delayed - Meshes"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, false);//"GUI Anim Hierarchy Delayed - Bone"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam, false);//"GUI Anim Hierarchy Delayed - ControlParam"

		}


		/// <summary>
		/// v1.4.2 : 객체를 클릭하여 선택하면,
		/// 해당 객체에 해당하는 Timeline Layer에 대한 UI 하단의 Layer Info를 직접 클릭한 것과 같은 판정이 나도록 만든다.
		/// 그게 "클릭한 판정"이 나야 이후에 Shift를 이용한 다중 레이어 선택이 가능해진다.
		/// 일단 타임라인 레이어가 선택된 이후에 이 함수를 호출하자
		/// </summary>
		/// <param name="clickedObject"></param>
		public void SyncLastClickedTimelineLayerInfo(object clickedObject)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| _subAnimTimeline == null
				|| clickedObject == null)
			{
				_lastClickTimelineLayer = null;
				return;
			}

			
			List<apAnimTimelineLayer> selectedLayers = _subObjects.AllTimelineLayers;
			int nTimelineLayers = selectedLayers != null ? selectedLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				_lastClickTimelineLayer = null;
				return;
			}

			apTransform_Mesh targetMeshTF = null;
			apTransform_MeshGroup targetMeshGroupTF = null;
			apBone targetBone = null;
			apControlParam targetCP = null;

			_lastClickTimelineLayer = null;

			int selectedObjType = -1;
			if(clickedObject is apTransform_Mesh)
			{
				targetMeshTF = clickedObject as apTransform_Mesh;
				selectedObjType = 0;

				if(_subAnimTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					return;//타입이 맞지 않으면 패스
				}
			}
			else if(clickedObject is apTransform_MeshGroup)
			{
				targetMeshGroupTF = clickedObject as apTransform_MeshGroup;
				selectedObjType = 1;

				if(_subAnimTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					return;//타입이 맞지 않으면 패스
				}
			}
			else if(clickedObject is apBone)
			{
				targetBone = clickedObject as apBone;
				selectedObjType = 2;

				if(_subAnimTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					return;//타입이 맞지 않으면 패스
				}
			}
			else if(clickedObject is apControlParam)
			{
				targetCP = clickedObject as apControlParam;
				selectedObjType = 3;

				if(_subAnimTimeline._linkType != apAnimClip.LINK_TYPE.ControlParam)
				{	
					return;//타입이 맞지 않으면 패스
				}
			}
			else
			{
				//뭘 클릭한거징
				return;
			}


			//클릭한 대상의 레이어를 찾아서 "마지막 클릭한 레이어"로 동기화하고 리턴
			apAnimTimelineLayer curLayer = null;
			for (int i = 0; i < nTimelineLayers; i++)
			{
				curLayer = selectedLayers[i];
				switch (selectedObjType)
				{
					case 0://Mesh TF
						{
							if(curLayer._linkedMeshTransform != null
								&& curLayer._linkedMeshTransform == targetMeshTF)
							{								_lastClickTimelineLayer = curLayer;
								_lastClickTimelineLayer = curLayer;
								return;
							}
						}
						break;

					case 1://MeshGroup TF
						{
							if(curLayer._linkedMeshGroupTransform != null
								&& curLayer._linkedMeshGroupTransform == targetMeshGroupTF)
							{
								_lastClickTimelineLayer = curLayer;
								return;
							}
						}
						break;

					case 2://Bone
						{
							if(curLayer._linkedBone != null
								&& curLayer._linkedBone == targetBone)
							{
								_lastClickTimelineLayer = curLayer;
								return;
							}
						}
						break;

					case 3://Control Param
						{
							if(curLayer._linkedControlParam != null
								&& curLayer._linkedControlParam == targetCP)
							{
								_lastClickTimelineLayer = curLayer;
								return;
							}
						}
						break;
				}
				
			}
			_lastClickTimelineLayer = null;

		}



		/// <summary>
		/// 현재 재생중인 프레임에 맞게 WorkKeyframe을 자동으로 선택한다.
		/// 키프레임을 바꾸거나 레이어를 바꿀때 자동으로 호출한다.
		/// 수동으로 선택하는 키프레임과 다르다.
		/// Work Keyframe이 변경되었거나 없다면 true를 리턴한다.
		/// </summary>
		public void AutoSelectAnimWorkKeyframe(out bool isWorkKeyframeChangedOrNone)
		{
			Editor.Gizmos.SetUpdate();

			//타임라인레이어가 없어도 초기화
			//플레이 중에는 모든 선택이 초기화된다.
			//if (_subAnimTimelineLayer == null || IsAnimPlaying)//이전
			if (_subObjects.NumTimelineLayers == 0 || IsAnimPlaying)
			{
				//if (_subAnimWorkKeyframe != null)
				if(_subObjects.NumWorkKeyframes > 0)//변경 20.6.12
				{
					//변경 20.6.12
					_subObjects.ClearWorkKeyframes();
					_modData.ClearAll();

					//변경 20.6.29 : Mod와 통합되었다.
					_modRenderVert_Main = null;
					_modRenderVerts_All.Clear();
					_modRenderVerts_Weighted.Clear();

					//추가 22.4.6 [v1.4.0] MRP
					_modRenderPin_Main = null;
					_modRenderPins_All.Clear();
					_modRenderPins_Weighted.Clear();

					//추가 : 기즈모 갱신이 필요한 경우 (주로 FFD)
					//Editor.Gizmos.RevertFFDTransformForce(); //>> 삭제. 외부에서 처리하자
				}

				Editor.Hierarchy_AnimClip.RefreshUnits();

				isWorkKeyframeChangedOrNone = true;
				return;
			}

			
			//변경된 코드 20.6.12 : 래핑
			bool isSelectedWorkKeyframeChanged = false;
			_subObjects.AutoSelectWorkKeyframes(_animClip, IsAnimPlaying, out isSelectedWorkKeyframeChanged);

			

			if(_subObjects.NumWorkKeyframes == 0)
			{
				//선택된 WorkKeyframe이 없다.
				_modData.ClearAll();

				//변경 20.6.29 : Mod와 통합되었다.
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0] MRP
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();


				//외부에서 처리하자 [v1.4.2]
				//Editor.Gizmos.RevertFFDTransformForce();//<기즈모 갱신

				Editor.Hierarchy_AnimClip.RefreshUnits();

				isWorkKeyframeChangedOrNone = true;
				return;
			}

			//ModData를 동기화하자
			_modData.SyncModData_AnimEdit();

			

			if (!isSelectedWorkKeyframeChanged)
			{
				//이전의 WorkKeyframe들이 그대로 유지되었을때
				//Debug.Log("Work Keyframe들이 계속 유지됨");
				isWorkKeyframeChangedOrNone = false;
			}
			else
			{
				//이전의 WorkKeyframe들은 선택되지 않았다. > 초기화

				//Debug.LogWarning("Work Keyframe들이 변경됨 > 다 초기화됨");

				//MRV는 모두 초기화
				//변경 20.6.29 : Mod와 통합되었다.
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				//추가 22.4.6 [v1.4.0] MRP
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();


				//완료
				//Editor.Gizmos.RevertFFDTransformForce();//이전

				//변경 v1.4.2
				//if (Editor.Gizmos.IsFFDMode)
				//{
				//	//FFD 모드였다면
				//	Editor.Gizmos.CheckAdaptOrRevertFFD(Editor);
				//}
				//else
				//{
				//	Editor.Gizmos.RevertFFDTransformForce();
				//}
				//FFD 체크는 이 함수를 호출하는 외부에서 일괄적으로 하자
				//대신 변경 내역을 out 변수에 입력하자
				isWorkKeyframeChangedOrNone = true;
				
			}
			

			//Hierarchy 갱신
			Editor.Hierarchy_AnimClip.RefreshUnits();

			return;
		}


		/// <summary>
		/// Anim 편집시 모든 선택된 오브젝트를 해제한다.
		/// </summary>
		public void UnselectAllObjects_ForAnimEdit()
		{
			//변경 20.6.29 : Mod와 통합되었다.
			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();

			//추가 22.4.6 [v1.4.0] MRP
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();

			//변경 20.5.27 : 래핑
			_subObjects.Clear();

			//변경 20.6.12 : 래핑
			_modData.ClearAll();

			SelectAnimTimelineLayer(null, MULTI_SELECT.Main, true, false, true);//TImelineLayer의 선택을 취소해야 AutoSelect가 정상작동한다.
			AutoSelectAnimTimelineLayer(false);
		}




		// [ 애니메이션 Mod Render Vertex 선택 함수들 ]

		/// <summary>
		/// Mod-Render Vertex를 선택한다. [Animation 수정작업시]
		/// </summary>
		/// <param name="modVertOfAnim">Modified Vertex of Anim Keyframe</param>
		/// <param name="renderVertOfAnim">Render Vertex of Anim Keyframe</param>
		public void SelectModRenderVert_ForAnimEdit(ModRenderVert modRenderVert)//변경 20.6.29 : MRV를 미리 만들어두고 선택하는 방식
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				//이전
				//|| AnimWorkKeyframe == null
				//|| ModMeshOfAnim == null
				//변경 20.6.12
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			//추가 22.4.6 [v1.4.0] MRP는 무조건 초기화
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();

			//변경 20.6.29
			//- MRV 미리 생성 방식으로 변경
			//- 변수는 Mod와 통합
			if(modRenderVert == null)
			{
				//다 초기화
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();
				return;
			}

			if(_modRenderVert_Main != modRenderVert)
			{
				_modRenderVert_Main = modRenderVert;
				//리스트를 갱신한다.
				_modRenderVerts_All.Clear();
				_modRenderVerts_All.Add(_modRenderVert_Main);

				_modRenderVerts_Weighted.Clear();
			}
		}



		/// <summary>
		/// [Animation 편집시] Mod-Render Vertex를 추가한다. 
		/// </summary>
		public void AddModRenderVert_ForAnimEdit(ModRenderVert modRenderVert)//변경 20.6.29 : MRV를 직접 설정
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				//이전
				//|| AnimWorkKeyframe == null
				//|| ModMeshOfAnim == null
				//변경 20.6.12
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modRenderVert == null)
			{
				return;
			}


			//추가 22.4.6 [v1.4.0] MRP는 무조건 초기화
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();

			bool isExist = _modRenderVerts_All.Contains(modRenderVert);

			if(!isExist)
			{
				//변경 20.6.25 : MRV를 그대로 사용
				_modRenderVerts_All.Add(modRenderVert);

				if (_modRenderVerts_All.Count == 1)
				{
					_modRenderVert_Main = modRenderVert;
				}

				//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
				if(_modRenderVerts_Weighted != null 
					&& _modRenderVerts_Weighted.Count > 0
					&& _modRenderVerts_Weighted.Contains(modRenderVert))
				{
					_modRenderVerts_Weighted.Remove(modRenderVert);
				}
			}
		}



		/// <summary>
		/// [Animation 편집시] Mod-Render Vertex를 추가한다.
		/// </summary>
		public void AddModRenderVertices_ForAnimEdit(List<ModRenderVert> modRenderVerts)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				//이전
				//|| AnimWorkKeyframe == null
				//|| ModMeshOfAnim == null
				//변경 20.6.12
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modRenderVerts == null || modRenderVerts.Count == 0)
			{
				return;
			}

			//추가 22.4.6 [v1.4.0] MRP는 무조건 초기화
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();

			//변경 20.6.25 : MRV 직접 비교
			//입력이 ModVertRig이므로, 이걸 MRV로 변환해야 한다.
			ModRenderVert targetMRV = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				targetMRV = modRenderVerts[i];

				if(targetMRV == null)
				{
					continue;
				}

				bool isExistSame = _modRenderVerts_All.Contains(targetMRV);

				if (!isExistSame)
				{
					//변경 20.6.25 : MRV를 그대로 사용
					_modRenderVerts_All.Add(targetMRV);

					if (_modRenderVerts_All.Count == 1)
					{
						_modRenderVert_Main = targetMRV;
					}

					//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
					if(_modRenderVerts_Weighted != null 
						&& _modRenderVerts_Weighted.Count > 0
						&& _modRenderVerts_Weighted.Contains(targetMRV))
					{
						_modRenderVerts_Weighted.Remove(targetMRV);
					}
				}
			}
		}







		/// <summary>
		/// [Animation 편집시] Mod-Render Vertex를 삭제한다.
		/// </summary>
		public void RemoveModRenderVert_ForAnimEdit(ModRenderVert modRenderVert)//변경 20.6.29 : MRV를 직접 설정
		{
			//Debug.LogError("TODO : RemoveModVertexOfAnim 다시 작성해야한다.");

			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				//이전
				//|| AnimWorkKeyframe == null
				//|| ModMeshOfAnim == null
				//변경 20.6.12
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			//변경 20.6.29 : 새로운 방식
			if(modRenderVert == null)
			{
				return;
			}

			//변경 20.6.25
			if(_modRenderVert_Main == modRenderVert)
			{
				_modRenderVert_Main = null;
			}
			_modRenderVerts_All.Remove(modRenderVert);

			//메인이 해제되었다면
			//남은 것 중에서 Main을 설정하자
			//메인이 해제되었다면
			//남은 것 중에서 Main을 설정하자
			if (_modRenderVert_Main == null
				&& _modRenderVerts_All.Count > 0)
			{
				_modRenderVert_Main = _modRenderVerts_All[0];
			}
		}



		// [ 애니메이션 Mod Render Pin 선택 함수들 ]
		//추가 22.4.6 [v1.4.0]

		/// <summary>
		/// Mod-Render Pin을 선택한다. [Animation 수정작업시]
		/// </summary>
		public void SelectModRenderPin_ForAnimEdit(ModRenderPin modRenderPin)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			//MRV는 무조건 초기화
			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();

			if(modRenderPin == null)
			{
				//다 초기화
				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();
				return;
			}

			if(_modRenderPin_Main != modRenderPin)
			{
				_modRenderPin_Main = modRenderPin;
				//리스트를 갱신한다.
				_modRenderPins_All.Clear();
				_modRenderPins_All.Add(_modRenderPin_Main);

				_modRenderPins_Weighted.Clear();
			}
		}



		/// <summary>
		/// [Animation 편집시] Mod-Render Pin을 추가로 선택한다.
		/// </summary>
		public void AddModRenderPin_ForAnimEdit(ModRenderPin modRenderPin)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modRenderPin == null)
			{
				return;
			}


			//MRV는 무조건 초기화
			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();

			bool isExist = _modRenderPins_All.Contains(modRenderPin);

			if(!isExist)
			{
				_modRenderPins_All.Add(modRenderPin);

				if (_modRenderPins_All.Count == 1)
				{
					_modRenderPin_Main = modRenderPin;
				}

				//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
				if(_modRenderPins_Weighted != null 
					&& _modRenderPins_Weighted.Count > 0
					&& _modRenderPins_Weighted.Contains(modRenderPin))
				{
					_modRenderPins_Weighted.Remove(modRenderPin);
				}
			}
		}



		/// <summary>
		/// [Animation 편집시] Mod-Render Pin들을 추가로 선택한다.
		/// </summary>
		public void AddModRenderPins_ForAnimEdit(List<ModRenderPin> modRenderPins)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			if(modRenderPins == null || modRenderPins.Count == 0)
			{
				return;
			}

			//MRV는 무조건 초기화
			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();

			ModRenderPin targetMRP = null;
			int nInputMRPs = modRenderPins.Count;
			for (int i = 0; i < nInputMRPs; i++)
			{
				targetMRP = modRenderPins[i];

				if(targetMRP == null)
				{
					continue;
				}

				bool isExistSame = _modRenderPins_All.Contains(targetMRP);

				if (!isExistSame)
				{
					_modRenderPins_All.Add(targetMRP);

					if (_modRenderPins_All.Count == 1)
					{
						_modRenderPin_Main = targetMRP;
					}

					//추가 20.9.13 : 선택된 버텍스는 Weighted 리스트에서는 제외해야한다.
					if(_modRenderPins_Weighted != null 
						&& _modRenderPins_Weighted.Count > 0
						&& _modRenderPins_Weighted.Contains(targetMRP))
					{
						_modRenderPins_Weighted.Remove(targetMRP);
					}
				}
			}
		}







		/// <summary>
		/// [Animation 편집시] Mod-Render Pin의 선택을 해제한다.
		/// </summary>
		public void RemoveModRenderPin_ForAnimEdit(ModRenderPin modRenderPin)
		{
			if (_selectionType != SELECTION_TYPE.Animation
				|| _animClip == null
				|| AnimWorkKeyframe_Main == null
				|| ModMesh_Main == null
				)
			{
				return;
			}

			//변경 20.6.29 : 새로운 방식
			if(modRenderPin == null)
			{
				return;
			}

			//변경 20.6.25
			if(_modRenderPin_Main == modRenderPin)
			{
				_modRenderPin_Main = null;
			}
			_modRenderPins_All.Remove(modRenderPin);

			//메인이 해제되었다면
			//남은 것 중에서 Main을 설정하자
			if (_modRenderPin_Main == null
				&& _modRenderPins_All.Count > 0)
			{
				_modRenderPin_Main = _modRenderPins_All[0];
			}
		}





		//-------------------------------------------------------------------
		// 본 선택 함수들
		//-------------------------------------------------------------------
		/// <summary>
		/// [MeshGroup 편집시] 본을 선택한다.
		/// </summary>
		/// <param name="bone"></param>
		/// <param name="multiSelect"></param>
		public void SelectBone(apBone bone, MULTI_SELECT multiSelect)
		{
			if (bone != null)
			{
				if (SelectionType == SELECTION_TYPE.MeshGroup
					&& Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Bone)
				{
					//만약 MeshGroup 메뉴 -> Bone 탭일 때
					//현재 선택한 meshGroup의 Bone이 아닌 Sub Bone이라면 > 선택 취소
					if (MeshGroup != bone._meshGroup)
					{
						bone = null;
					}
				}
			}

			//변경 20.5.27 : 래핑 후 코드
			bool isChanged = _subObjects.SelectBone(bone, multiSelect);
			if(isChanged)
			{
				apEditorUtil.ReleaseGUIFocus();
			}
			if (SelectionType == SELECTION_TYPE.MeshGroup &&
				Modifier != null)
			{
				AutoSelectModMeshOrModBone();
			}
			if (SelectionType == SELECTION_TYPE.Animation && AnimClip != null)
			{
				AutoSelectAnimTimelineLayer(false);
			}
		}

		/// <summary>
		/// [애니메이션 편집시] 본을 선택한다.
		/// AnimClip 작업시 Bone을 선택하면 SetBone대신 이 함수를 호출한다.
		/// </summary>
		/// <param name="bone"></param>
		public void SelectBone_ForAnimEdit(	apBone bone, 
											bool isAutoSelectTimelineLayer, 
											bool isAutoTimelineUIScroll, 
											MULTI_SELECT multiSelect)
		{
			//변경 20.5.27 : 래핑
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				_subObjects.Select(null, null, null, MULTI_SELECT.Main, TF_BONE_SELECT.Exclusive);
				_subObjects.ClearControlParam();
				return;
			}

			if(bone != null)
			{
				_subObjects.Select(null, null, bone, multiSelect, TF_BONE_SELECT.Exclusive);
				_subObjects.ClearControlParam();
			}
			else
			{
				_subObjects.SelectBone(null, multiSelect);
				_subObjects.ClearControlParam();
			}

			//Debug.Log(">>> MidResult1 Selected Bones [" + _selectedSubObject.NumBone + "]");
			apBone prevMainBone = _subObjects.Bone;
			

			SelectAnimTimelineLayer(null, MULTI_SELECT.Main, true);//TImelineLayer의 선택을 취소해야 AutoSelect가 정상작동한다.

			if (isAutoSelectTimelineLayer)
			{
				AutoSelectAnimTimelineLayer(isAutoTimelineUIScroll);
			}

			apBone curMainBone = _subObjects.Bone;
			
			//AutoSelectAnimTimelineLayer 호출 후 결과가 바뀌었다?
			//if (_bone != bone && bone != null)//이전
			if(prevMainBone != curMainBone && curMainBone != null)//변경 20.5.27
			{
				//Debug.Log("Bone > Restore Main");
				//bone은 유지하자
				//_bone = bone;//이전
				_subObjects.SelectBone(bone, MULTI_SELECT.Main);//변경 20.5.27 : 메인 본으로 설정
				
				//이전
				//_modBoneOfAnim = null;//<<응?
				_modData.SyncModData_AnimEdit();//<<이걸로 바꿔보자 (20.6.12)
			}
		}
	}
}