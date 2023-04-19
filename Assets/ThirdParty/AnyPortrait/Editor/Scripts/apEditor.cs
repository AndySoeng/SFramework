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

	public partial class apEditor : EditorWindow
	{
		private static apEditor s_window = null;

		public static bool IsOpen()
		{
			return (s_window != null);
		}

		public static apEditor CurrentEditor
		{
			get
			{
				return s_window;
			}
		}



		/// <summary>
		/// 작업을 위해서 상세하게 디버그 로그를 출력할 것인가. (True인 경우 정상적인 처리 중에도 Debug가 나온다.)
		/// </summary>
		public static bool IS_DEBUG_DETAILED
		{
			get
			{
				return false;
			}
		}


		//--------------------------------------------------------------


		[MenuItem("Window/AnyPortrait/2D Editor", false, 10), ExecuteInEditMode]
		public static apEditor ShowWindow()
		{
			if (s_window != null)
			{
				try
				{	
					s_window._isLockOnEnable = true;
					s_window.Close();
					s_window = null;
				}
				catch (Exception)
				{
					//Debug.LogError("S_Window Exception : " + ex);
				}
			}

			//C++ 플러그인 설치 처리부터
			apPluginUtil.I.CheckAndInstallCPPDLLPackage();

			//리소스 다시 로딩
			apEditor.s_isEditorResourcesLoaded = false;//여기서 호출해야 OnEnabled 호출 전에 비활성화할 수 있다.

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apEditor), false, "AnyPortrait");
			apEditor curTool = curWindow as apEditor;

			//에러가 발생하는 Dialog는 여기서 미리 꺼준다.
			apDialog_PortraitSetting.CloseDialog();
			apDialog_Bake.CloseDialog();

			if (curTool != null)
			{
				curTool.LoadEditorPref();
			}


			//Debug.LogError("Show Window : " + (curTool != null) + " / " + (curTool != s_window) + " / s_window is null : " + (s_window == null));
			if (curTool != null && curTool != s_window)
			//if(curTool != null)
			{
				//첫 실행 이후엔 다시 에디터를 껐다 켜도 이 구문은 실행되지 않는다.
				//에디터를 꺼도 curTool와 s_window가 동일하기 때문 > OnDisable에서 Init이 호출되고, OnEnable에서 s_window에 할당이 되버렸다..



				Debug.ClearDeveloperConsole();
				s_window = curTool;
				//s_window.position = new Rect(0, 0, 200, 200);
				s_window.Init(true);
				s_window._isFirstOnGUI = true;
			}


			return curTool;

		}

		public static void CloseEditor()
		{
			if (s_window != null)
			{
				try
				{
					s_window.Close();
				}
				catch (Exception ex)
				{
					Debug.LogError("Close Exception : " + ex);

				}

				s_window = null;
			}
		}

		

		//----------------------------------------------------------------------------------
		// EditorWindow의 기본 이벤트
		//----------------------------------------------------------------------------------
		void OnDisable()
		{
			//재빌드 등을 통해서 다시 시작하는 경우 -> 다시 Init
			Init(false);
			SaveEditorPref();

			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			//SceneView.onSceneGUIDelegate -= OnSceneViewEvent;
			//EditorApplication.modifierKeysChanged -= OnKeychanged;

			//추가 3.25 : 씬이 바뀌면 작업을 초기화해야한다.
			//EditorApplication.hierarchyWindowChanged -= OnEditorHierarchyChanged;
#if UNITY_2018_1_OR_NEWER
			EditorApplication.hierarchyChanged -= OnEditorHierarchyChanged;
#else
			EditorApplication.hierarchyWindowChanged -= OnEditorHierarchyChanged;
#endif

			if (_backup != null)
			{
				_backup.StopForce();
			}

			//에디터를 끌때 EditorDirty를 수행한다.
			apEditorUtil.SetEditorDirty();
			EditorUtility.ClearProgressBar();

			//비동기 로딩 초기화
			ClearLoadingPortraitAsync();

			//로토스코핑 초기화 (21.2.28)
			_isEnableRotoscoping = false;
			_selectedRotoscopingData = null;
			if(Rotoscoping != null)
			{
				Rotoscoping.DestroyAllImages();
			}

			//가이드라인 (21.6.4)
			_isEnableGuideLine = false;
		}



		void OnEnable()
		{
			if (_isLockOnEnable)
			{
				Debug.Log("apEditor : OnEnable >> Locked");
				return;
			}
			//Debug.Log("apEditor : OnEnable");
			if (this.maximized)
			{

			}
			if (s_window != this && s_window != null)
			{
				try
				{
					apEditor closedEditor = s_window;
					s_window = null;
					if (closedEditor != null)
					{
						closedEditor.Close();
					}
				}
				catch (Exception)
				{
					//Debug.LogError("OnEnable -> Close Exception : " + ex);
					return;
				}

			}
			s_window = this;
			

			autoRepaintOnSceneChange = true;


			_isFirstOnGUI = true;
			if (apEditorUtil.IsGammaColorSpace())
			{
				Notification(apVersion.I.APP_VERSION, false, false);
			}
			else
			{
				Notification(apVersion.I.APP_VERSION + " (Linear Color Space Mode)", false, false);
			}

			//Debug.Log("Add Scene Delegate");
			Undo.undoRedoPerformed += OnUndoRedoPerformed;

			//추가 3.25 : 씬이 바뀌면 작업을 초기화해야한다.

			_currentScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

#if UNITY_2018_1_OR_NEWER
			EditorApplication.hierarchyChanged += OnEditorHierarchyChanged;
#else
			EditorApplication.hierarchyWindowChanged += OnEditorHierarchyChanged;
#endif
			//EditorApplication.hierarchyWindowChanged += OnEditorHierarchyChanged;

			//SceneView.onSceneGUIDelegate += OnSceneViewEvent;
			//EditorApplication.modifierKeysChanged += OnKeychanged;
			PhysicsPreset.Load();
			ControlParamPreset.Load();
			AnimEventPreset.Load();//추가 22.6.13 : 애니메이션 이벤트 프리셋


			//로토스코핑 초기화 (21.2.28)
			_isEnableRotoscoping = false;
			_selectedRotoscopingData = null;
			_iSyncRotoscopingAnimClipFrame = -1;
			_isSyncRotoscopingToAnimClipFrame = false;

			Rotoscoping.DestroyAllImages();
			Rotoscoping.Load();//추가 21.2.27

			//가이드라인 (21.6.4)
			_isEnableGuideLine = false;
		}




		// 윈도우 포커스에 따른 처리

		//-----------------------------------------------------------------

		//22.6.11 [v1.4.0]
		//< 포커스를 잃었다가 복구했을 때 발생하는 문제 >
		//- Repaint 횟수가 크게 줄어들어서 Delta Time이 크게 증가한다.
		//- 애니메이션에 프레임 이상의 Delta Time이 입력되므로, "남은 시간"이 발생한다.
		//- 바로 Repaint가 되어야 "남은 시간"을 소비하여 애니메이션을 재생하는데, Repaint를 적게 하니 남은 시간이 소비되지 않는다.
		//- 포커스를 되찾아서 Repaint가 원래대로 발생하면 중첩된 "남은 시간"이 계속 돌아가면서 엄청나게 가속을 한다.
			
		//< 해결법 >
		//- 포커스를 복구했을때)
		//(1) portrait 애니메이션 재생에 "남은 시간"을 모두 0으로 초기화해야한다.
		//(2) 혹시 모르니 타이머도 Delta Time을 초기화한다.


		//윈도우가 포커스를 잃었을 때
		void OnLostFocus()
		{
			//Debug.LogError("On Lost Focus");
			//포커스를 잃는다면
			
			if(apTimer.I != null)
			{
				apTimer.I.OnLostFocus();
			}
		}

		//윈도우가 포커스를 복구했을 때
		void OnFocus()
		{
			//Debug.LogWarning("On Focus");

			//이전에 포커스를 잃었다가 복구했다면
			//타이머의 첫번째 Delta 시간을 0으로 리셋한다. Delta 시간이 과도하게 증가하는 것을 막기 위해
			if(apTimer.I != null)
			{
				apTimer.I.OnRecoverFocus();
			}

			//남은 시간을 0으로 초기화한다.
			
			if(_portrait != null && Select != null)
			{
				apMeshGroup targetMeshGroup = null;

				switch (Select.SelectionType)
				{
					case apSelection.SELECTION_TYPE.Overall:
						{
							//애니메이션 시간 리셋
							if(Select.RootUnit != null
								&& Select.RootUnitAnimClip != null)
							{
								Select.RootUnitAnimClip.OnEditorRestoreFocus();
							}

							//물리 시간 리셋
							_portrait.ResetPhysicsTimer();
							if(Select.RootUnit != null)
							{
								targetMeshGroup = Select.RootUnit._childMeshGroup;
							}

						}
						break;

					case apSelection.SELECTION_TYPE.MeshGroup:
						{
							_portrait.ResetPhysicsTimer();
							targetMeshGroup = Select.MeshGroup;
						}
						break;

					case apSelection.SELECTION_TYPE.Animation:
						{
							_portrait.ResetPhysicsTimer();

							if(Select.AnimClip != null)
							{
								Select.AnimClip.OnEditorRestoreFocus();

								targetMeshGroup = Select.AnimClip._targetMeshGroup;
							}
						}
						break;
				}

				//포커스를 회복했을 때, 일부 모디파이어의 자체 업데이트 타이머도 리셋한다.
				//재귀적으로 호출
				if(targetMeshGroup != null)
				{
					ResetModifierPhysicsTimerRecursive(targetMeshGroup, targetMeshGroup);
				}
			}

			
		}

		//에디터 포커스 회복시 물리 업데이트의 남은 시간을 리셋하기 위한 함수
		private void ResetModifierPhysicsTimerRecursive(apMeshGroup targetMeshGroup, apMeshGroup rootMeshGroup)
		{
			if(targetMeshGroup == null)
			{
				return;
			}

			if(targetMeshGroup._modifierStack != null)
			{
				int nModifiers = targetMeshGroup._modifierStack._modifiers != null ? targetMeshGroup._modifierStack._modifiers.Count : 0;
				if(nModifiers > 0)
				{
					apModifierBase curMod = null;
					for (int i = 0; i < nModifiers; i++)
					{
						curMod = targetMeshGroup._modifierStack._modifiers[i];
						curMod.ResetPhysicsTime();//물리 업데이트 시간을 리셋한다.
					}
				}
			}

			//자식 메시 그룹에도 재귀적으로 호출
			int nChildMGs = targetMeshGroup._childMeshGroupTransforms != null ? targetMeshGroup._childMeshGroupTransforms.Count : 0;
			if(nChildMGs > 0)
			{
				apTransform_MeshGroup childMGTF = null;
				for (int i = 0; i < nChildMGs; i++)
				{
					childMGTF = targetMeshGroup._childMeshGroupTransforms[i];
					if(childMGTF == null
						|| childMGTF._meshGroup
						|| childMGTF._meshGroup == targetMeshGroup
						|| childMGTF._meshGroup == rootMeshGroup)
					{
						continue;
					}

					//재귀적으로 호출
					ResetModifierPhysicsTimerRecursive(childMGTF._meshGroup, rootMeshGroup);
				}
			}
		}

		//-----------------------------------------------------------------


		private void OnUndoRedoPerformed()
		{
			if (_portrait == null)
			{
				return;
			}
			apUndoHistory.UNDO_RESULT undoResult = apEditorUtil.OnUndoRedoPerformed();


			//실제로 오브젝트가 복원되었거나 추가된게 사라지는 내역이 있는가
			apSelection.RestoredResult restoreResult = Select.SetAutoSelectWhenUndoPerformed(_portrait,
										_recordList_TextureData,
										_recordList_Mesh,
										//_recordList_MeshGroup,
										_recordList_AnimClip,
										_recordList_ControlParam,
										_recordList_Modifier,
										_recordList_AnimTimeline,
										_recordList_AnimTimelineLayer,
										//_recordList_Transform,
										_recordList_Bone,
										_recordList_MeshGroupAndTransform,
										_recordList_AnimClip2TargetMeshGroup,
										_isRecordedStructChanged);

			//Debug.Log("Undo Result : " + undoResult);
			if (!restoreResult._isAnyRestored && undoResult == apUndoHistory.UNDO_RESULT.StructChanged)
			{
				//Debug.LogError("실행 취소 >> RestoredResult에서 감지하지 못한 구조 변화");
				restoreResult._isAnyRestored = true;
				restoreResult._isRestoreToAdded = true;
				restoreResult._isRestoreToRemoved = true;
			}

			//복구시 ID 체크해야하는 종류
			//Texture = 0,
			//Vertex = 1,
			//Mesh = 2,
			//MeshGroup = 3,
			//Transform = 4,
			//Modifier = 5,
			//ControlParam = 6,
			//AnimClip = 7,
			//AnimTimeline = 8,//<이게 모디파이어에 연결
			//AnimTimelineLayer = 9,
			//AnimKeyFrame = 10,
			//Bone = 11,
			//MeshPin = 12,//추가 22.2.26
			
			bool isResetHierarchyAll = false;
			if (restoreResult._isAnyRestored)
			{
				//Debug.LogWarning("Undo에서 오브젝트가 추가되거나 삭제된 것을 되돌린다.");
				isResetHierarchyAll = true;
			}

			//4.1 추가
			//Undo 이후에 텍스쳐가 리셋되는 문제가 있다.
			for (int iTexture = 0; iTexture < _portrait._textureData.Count; iTexture++)
			{
				apTextureData textureData = _portrait._textureData[iTexture];
				if (textureData == null)
				{
					continue;
				}
				if (textureData._image == null)
				{
					//Debug.Log("Image가 없는 경우 발견 : " + textureData._name + " / [" + textureData._assetFullPath + "]");
					if (!string.IsNullOrEmpty(textureData._assetFullPath))
					{
						//Debug.Log("저장된 경로 : " + textureData._assetFullPath);
						Texture2D restoreImage = AssetDatabase.LoadAssetAtPath<Texture2D>(textureData._assetFullPath);
						if (restoreImage != null)
						{
							//Debug.Log("이미지 복원 완료");
							textureData._image = restoreImage;
						}
					}
				}
			}


			//Mesh의 TextureData를 다시 확인해봐야한다.
			for (int iMesh = 0; iMesh < _portrait._meshes.Count; iMesh++)
			{
				apMesh mesh = _portrait._meshes[iMesh];
				if (!mesh.IsTextureDataLinked)
				{
					//Link가 풀렸다면..
					mesh.SetTextureData(_portrait.GetTexture(mesh.LinkedTextureDataID));
				}
			}

			


			//이전
			//_portrait.LinkAndRefreshInEditor(restoreResult._isAnyRestored, null, null);

			//변경 20.4.3
			if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation &&
				Select.AnimClip != null)
			{
				//추가 20.7.3 : MRV 복구 함수 (Anim/Mod)
				Select.StoreSelectedModRenderVertsPins_ForUndo();

				//추가 20.7.15 : 작업 가시성 저장
				if (Select.AnimClip._targetMeshGroup != null)
				{
					VisiblityController.Save_AllRenderUnits(Select.AnimClip._targetMeshGroup);
					VisiblityController.Save_AllBones(Select.AnimClip._targetMeshGroup);
				}


				//이전
				//_portrait.LinkAndRefreshInEditor(restoreResult._isAnyRestored, apUtil.LinkRefresh.Set_AnimClip(Select.AnimClip));

				//<여기가 문제>

				//변경 20.7.2 : 조금 더 확실하게 복구
				_portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_AnimClip(Select.AnimClip));
			}
			else if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup &&
					Select.MeshGroup != null)
			{
				//추가 20.7.3 : MRV 복구 함수 (Anim/Mod)
				Select.StoreSelectedModRenderVertsPins_ForUndo();

				//추가 20.7.15 : 작업 가시성 저장
				VisiblityController.Save_AllRenderUnits(Select.MeshGroup);
				VisiblityController.Save_AllBones(Select.MeshGroup);

				
				//추가 21.7.1 : Depth를 렌더 유닛에 적용
				Select.MeshGroup.TFDepthToRenderUnitsOnUndo();

				
				//_portrait.LinkAndRefreshInEditor(restoreResult._isAnyRestored, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(Select.MeshGroup));
				//변경 20.7.3 : 조금 더 확실하게 복구
				_portrait.LinkAndRefreshInEditor(true, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(Select.MeshGroup));

				
				if (Select.Modifier != null)
				{
					//모디파이어 편집 중이라면
					//Undo시 추가적인 처리가 필요할 수 있다.
					apModifierBase.MODIFIER_TYPE curModifierType = Select.Modifier.ModifierType;

					switch (curModifierType)
					{
						case apModifierBase.MODIFIER_TYPE.Rigging:
							Select.SetBoneRiggingTest();//추가 20.7.3 : 이걸로 리깅시 포즈 테스트가 작동 안되는 버그가 해결된다.
							break;

							
					}
				}

				//컨트롤 파라미터 > PSG 매핑도 다시 갱신한다.
				Select.RefreshControlParam2PSGMapping();

				//[v1.4.1] Undo 전후로 PSG를 해제하거나 자동으로 선택하는 기능
				if (Select.Modifier != null)
				{
					//PSG 매핑 후에
					//Control Param 타입의 PSG 리스트가 있는 상태에서 Selected PSG가 없다면
					//> 자동으로 선택한다.
					apModifierBase.MODIFIER_TYPE curModifierType = Select.Modifier.ModifierType;
					if(curModifierType == apModifierBase.MODIFIER_TYPE.Morph
						|| curModifierType == apModifierBase.MODIFIER_TYPE.TF
						|| curModifierType == apModifierBase.MODIFIER_TYPE.ColorOnly)
					{
						//컨트롤 파라미터를 이용하는 타입
						int nPSGs = Select.Modifier._paramSetGroup_controller != null ? 
									Select.Modifier._paramSetGroup_controller.Count : 0;

						apModifierParamSetGroup selectedPSG = Select.SubEditedParamSetGroup;
						bool isPSGChanged = false;
						if(selectedPSG != null)
						{
							//선택된 PSG가 있다.
							if(nPSGs > 0)
							{
								if(!Select.Modifier._paramSetGroup_controller.Contains(selectedPSG))
								{
									//선택된 PSG가 리스트에 없다면 해제
									Select.SelectParamSetGroupOfModifier(null);
									isPSGChanged = true;
								}
							}
							else
							{
								//리스트가 없다.
								Select.SelectParamSetGroupOfModifier(null);
								isPSGChanged = true;
							}
						}
						
						selectedPSG = Select.SubEditedParamSetGroup;

						if(nPSGs > 0 && selectedPSG == null)
						{
							//리스트중 하나를 그냥 선택한다.
							Select.SelectParamSetGroupOfModifier(Select.Modifier._paramSetGroup_controller[0]);
							isPSGChanged = true;
						}

						if(isPSGChanged)
						{
							Select.AutoSelectParamSetOfModifier();
						}
					}
				}
			}
			else
			{
				_portrait.LinkAndRefreshInEditor(restoreResult._isAnyRestored, apUtil.LinkRefresh.Set_AllObjects(null));
			}

			if (Select.SelectionType == apSelection.SELECTION_TYPE.Mesh)
			{
				if (Select.Mesh != null)
				{
					Select.Mesh.OnUndoPerformed();//20.7.6 : 메시 제작 버그 해결용
					Select.Mesh.MakeOffsetPosMatrix();
					Select.Mesh.RefreshPolygonsToIndexBuffer();
				}
			}
			else if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				if (Select.MeshGroup != null)
				{
					//Debug.LogError(">>> LinkRefresh : " + Select.MeshGroup._name);

					apMeshGroup meshGroup = Select.MeshGroup;
					apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(meshGroup);
					//apUtil.LinkRefresh.Set_AllObjects(null);//테스트


					if (restoreResult._isAnyRestored || _meshGroupEditMode == MESHGROUP_EDIT_MODE.Setting)
					{
						//추가 : Setting 탭에서는 무조건 Sort를 하자
						//meshGroup.SortRenderUnits(true);//삭제 20.4.4
						//Debug.Log("Undo > Sort MeshGroup");
						meshGroup.SetDirtyToReset();

						Hierarchy.SetNeedReset();
					}


					//TODO : 이 코드는 임시 방편이며
					//(자식 메시 그룹의 Morph 수정 > Undo시 RenderVert가 선택되지 않는 문제)
					//실제로는 RenderUnit이 재활용될 수 있게 만들어야 한다.
					//제한적으로 자식 메시 그룹에 한해서 이 코드를 실행시키자
					//meshGroup.SetDirtyToReset();//문제 생기면 이거 주석 해제 20.4.7

					//단 이 방식은 일시적인 해결 방안이다.
					if (meshGroup._parentMeshGroup != null)
					{
						meshGroup.SetDirtyToReset();//문제 생기면 이거 주석 해제 20.4.7
					}

					meshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);

					//추가 : 계층적으로 MeshGroup/Modifier가 연결된 경우 이게 코드들이 추가되어야 함
					if (meshGroup._rootRenderUnit != null)
					{
						meshGroup._rootRenderUnit.ReadyToUpdate();//RenderVert 정리를 위해서 ReadyToUpdate를 한번 호출해야한다.
					}
					//meshGroup.LinkModMeshRenderUnits();//삭제 20.4.4 : RefreshForce > ResetRenderUnits에 해당 함수가 호출된다.
					meshGroup.RefreshModifierLink(apUtil.LinkRefresh);

					//>> BoneSet으로 변경
					apMeshGroup.BoneListSet boneSet = null;
					if (meshGroup._boneListSets.Count > 0)
					{
						meshGroup.UpdateBonesWorldMatrix();//<<전체 갱신 5.17

						for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
						{
							boneSet = meshGroup._boneListSets[iSet];
							for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
							{
								boneSet._bones_Root[iRoot].GUIUpdate(true);
							}
						}

						meshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
						meshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);
					}

					//추가 21.1.32 : Rule 가시성 동기화 초기화
					Controller.ResetVisibilityPresetSync();
				}
				//추가 20.7.2 : 혹시 동기화가 풀릴 수 있으니 확인
				Select.AutoSelectModMeshOrModBone();
			}
			else if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				if (Select.AnimClip != null)
				{
					//Debug.LogError("실행취소 중간 B1 >>> ");
					//DebugAnimModMeshValues();

					apAnimClip animClip = Select.AnimClip;
					animClip.RefreshTimelines(null, null);//변경 19.5.21 : 전체 Refresh일 경우 null입력
					animClip.UpdateMeshGroup_Editor(true, 0.0f, true, true);

					apUtil.LinkRefresh.Set_AnimClip(animClip);//갱신 최적화 20.4.4

					if (animClip._targetMeshGroup != null)
					{
						apMeshGroup meshGroup = animClip._targetMeshGroup;
						//if(restoreResult._isAnyRestored)
						//{
						//	//Debug.LogError("Undo : AnyRestored");
						//	apUtil.LinkRefresh.Set_AllObjects(null);
						//}
						//else
						//{
						//	Debug.LogWarning("Undo : 유지");
						//}

						if (restoreResult._isAnyRestored)
						{
							//meshGroup.SortRenderUnits(true);//삭제 20.4.4
							//Debug.Log("Undo > Sort MeshGroup");
							meshGroup.SetDirtyToReset();
						}

						//Debug.LogError("실행취소 중간 B2 >>> ");
						//DebugAnimModMeshValues();

						//meshGroup.SetDirtyToReset();//문제 생기면 이거 주석 해제 20.4.7
						meshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);


						//Debug.LogError("실행취소 중간 B3 >>> ");
						//DebugAnimModMeshValues();

						//추가 : 계층적으로 MeshGroup/Modifier가 연결된 경우 이게 코드들이 추가되어야 함
						//meshGroup.LinkModMeshRenderUnits();//삭제 20.4.4 : RefreshForce > ResetRenderUnits에 해당 함수가 호출된다.
						meshGroup.RefreshModifierLink(apUtil.LinkRefresh);



						//>> BoneSet으로 변경
						apMeshGroup.BoneListSet boneSet = null;
						if (meshGroup._boneListSets.Count > 0)
						{
							meshGroup.UpdateBonesWorldMatrix();//<<전체 갱신 5.17

							for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
							{
								boneSet = meshGroup._boneListSets[iSet];
								for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
								{
									boneSet._bones_Root[iRoot].GUIUpdate(true);
								}
							}

							meshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
							meshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh);
						}

					}

					//추가 21.1.32 : Rule 가시성 동기화 초기화
					Controller.ResetVisibilityPresetSync();

					//추가 3.31 : 공통 커브 갱신
					Select.AutoRefreshCommonCurve();
				}

				//추가 20.7.2
				bool isWorkKeyframeChanged = false;
				Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);//Work Keyframe만 체크하고 추가적인 처리를 하지 않음


			}
			


			//만약 FFD 모드 중이었다면 FFD 중단
			if (Gizmos.IsFFDMode)
			{
				Gizmos.RevertFFD(false);//Undo는 기록하지 않는다.
			}

			if (restoreResult._isAnyRestored)
			{
				Hierarchy.SetNeedReset();
				
				//+ ID 리셋해야함
				_portrait.RefreshAllUniqueIDs();
			}



			//Debug.Log("Undo > Hierarchy Refresh");
			if (isResetHierarchyAll)
			{
				//Debug.Log("Hierarchy 아예 리셋");
				ResetHierarchyAll();
			}
			else
			{
				RefreshControllerAndHierarchy(false);
			}

			if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				//RefreshTimelineLayers(restoreResult._isAnyRestored);//<<이걸 True로 할 때가 있는데;

				RefreshTimelineLayers(
					(restoreResult._isAnyRestored ?
					REFRESH_TIMELINE_REQUEST.All :
					REFRESH_TIMELINE_REQUEST.Timelines | REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier), null, null);

				//RefreshTimelineLayers(REFRESH_TIMELINE_REQUEST.All, null, null);
				//추가 20.7.2 : Undo 연속 2회시 Gizmo가 먹통이 되는 버그를 해결하기 위해 추가
				//단, SoftSelection은 유지하도록 만들자.
				bool isPrevSoftSelection = Gizmos.IsSoftSelectionMode;

				//이전
				//Select.RefreshAnimEditing(true);

				//변경 22.5.15
				Select.AutoRefreshModifierExclusiveEditing();

				if (isPrevSoftSelection)
				{
					Gizmos.StartSoftSelection();
				}
			}

			//자동으로 페이지 전환
			if (restoreResult._isAnyRestored)
			{
				Select.SetAutoSelectOrUnselectFromRestore(restoreResult, _portrait);
				RefreshControllerAndHierarchy(true);
			}

			//여기서 Undo이후 현재 오브젝트 상태 기록
			OnAnyObjectAddedOrRemoved(false, true);



			//MRV 복구
			if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
				Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				//추가 20.7.3 : MRV 복구 함수 (Anim/Mod)
				Select.RecoverSelectedModRenderVerts_ForUndo();

				//추가 20.7.15 : 작업 가시성 복구
				if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation
					&& Select.AnimClip != null
					&& Select.AnimClip._targetMeshGroup != null)
				{
					VisiblityController.LoadAll(Select.AnimClip._targetMeshGroup);
				}
				else if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
					&& Select.MeshGroup != null)
				{
					VisiblityController.LoadAll(Select.MeshGroup);
				}

				RefreshControllerAndHierarchy(false);

				Select.RefreshMeshGroupExEditingFlags(true);//추가 21.2.17 : Undo이후에 ExFlag가 깨지는것을 막는다.
			}

			Repaint();

			_isRecordedStructChanged = false;//Undo를 하면 구조 변경 플래그는 무조건 사라진다.

			Notification(UNDO_REDO_TEXT, true, false);

			//추가 20.6.25 : Undo 후 키보드 단축키로 다시 제어할 때 Undo 등록이 안되는 문제 수정
			Gizmos.ResetEventAfterUndo();
		}








		//----------------------------------------------------------------------------------------------------

		/// <summary>
		/// 화면내에 Notification Ballon을 출력합니다.
		/// </summary>
		/// <param name="strText"></param>
		/// <param name="_isShortTime"></param>
		public void Notification(string strText, bool _isShortTime, bool isDrawBalloon)
		{
			if (string.IsNullOrEmpty(strText))
			{
				return;

			}
			if (isDrawBalloon)
			{
				RemoveNotification();

				if (_guiContent_Notification == null)
				{
					_guiContent_Notification = apGUIContentWrapper.Make(strText, true);
				}
				else
				{
					_guiContent_Notification.SetText(strText);
				}
				ShowNotification(_guiContent_Notification.Content);
				_isNotification = true;
				if (_isShortTime)
				{
					_tNotification = NOTIFICATION_TIME_SHORT;
				}
				else
				{
					_tNotification = NOTIFICATION_TIME_LONG;
				}
			}
			else
			{
				_isNotification_GUI = true;
				if (_isShortTime)
				{
					_tNotification_GUI = NOTIFICATION_TIME_SHORT;
				}
				else
				{
					_tNotification_GUI = NOTIFICATION_TIME_LONG;
				}
				_strNotification_GUI = strText;
			}
		}



		//-------------------------------------------------------------------------------------------------
		private void OnEditorHierarchyChanged()
		{
			//Debug.Log("OnEditorHierarchyChanged");
			UnityEngine.SceneManagement.Scene curScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

			string prevScenePath = (_currentScene.path == null ? "" : _currentScene.path);
			string nextScenePath = (curScene.path == null ? "" : curScene.path);


			//추가 3.25 : 현재 작업 중인 Scene과 다른지 확인
			if (!prevScenePath.Equals(nextScenePath))
			{
				_currentScene = curScene;
				OnEditorSceneUnloadedOrChanged();
			}

			EditorUtility.ClearProgressBar();
		}






		private void OnEditorSceneUnloadedOrChanged()
		{
			try
			{
				Debug.Log("AnyPortrait : The scene you are working on has changed, so the editor is initialized.");

				Init(false);

				_selection.SelectNone();
				_portrait = null;

				_hierarchy.ResetAllUnits();
				_hierarchy_MeshGroup.ResetSubUnits();
				_hierarchy_AnimClip.ResetSubUnits();

				_portraitsInScene.Clear();
			}
			catch (Exception ex)
			{
				Debug.LogError("AnyPortrait : Error of Changed Scene\n" + ex);
			}
		}



		// Init
		//-------------------------------------------------------------------------------------------------
		private void Init(bool isShowEditor)
		{	

			//_tab = TAB.ProjectSetting;
			_portrait = null;
			_portraitsInScene.Clear();
			if(_selection == null)
			{
				_selection = new apSelection(this);
			}
			
			if(_controller == null)
			{
				_controller = new apEditorController();
				_controller.SetEditor(this);
			}
			

			if(_hierarchy == null)
			{
				_hierarchy = new apEditorHierarchy(this);
			}
			if(_hierarchy_MeshGroup == null)
			{
				_hierarchy_MeshGroup = new apEditorMeshGroupHierarchy(this);
			}
			if(_hierarchy_AnimClip == null)
			{
				_hierarchy_AnimClip = new apEditorAnimClipTargetHierarchy(this);
			}
			if(_imageSet == null)
			{
				_imageSet = new apImageSet();
			}



			//추가 20.3.17 : 기본 경로를 파일로부터 연다
			//이전
			//if (_pathSetting == null)
			//{
			//	_pathSetting = new apPathSetting();
			//	_pathSetting.Load();
			//	apEditorUtil.SetPackagePath(_pathSetting.CurrentPath);
			//}

			//변경 21.5.30 : 싱글톤으로 변경
			if(!apPathSetting.I.IsFirstLoaded)
			{
				//이전
				//apPathSetting.I.Load();
				//apEditorUtil.SetPackagePath(apPathSetting.I.CurrentPath);

				//변경 21.10.4 : 함수 변경
				apPathSetting.I.RefreshAndGetBasePath(false);
			}
			


			

			if(_materialLibrary == null)
			{
				_materialLibrary = new apMaterialLibrary(apPathSetting.I.CurrentPath);//추가 20.4.21 : 재질 라이브러리도 경로 변경
			}
			
			//추가 v1.4.2
			if(_projectSettingData == null)
			{
				_projectSettingData = new apProjectSettingData();
				_projectSettingData.Load();
			}



			_mat_Color = null;
			_mat_GUITexture = null;
			//_mat_MaskedTexture = null;
			_mat_Texture_Normal = null;
			_mat_Texture_VertAdd = null;

			if(_gizmos == null)
			{
				_gizmos = new apGizmos(this);
			}
			if(_gizmoController == null)
			{
				_gizmoController = new apGizmoController();
				_gizmoController.SetEditor(this);
			}
			

			wantsMouseMove = true;

			_dialogShowCall = DIALOG_SHOW_CALL.None;

			//설정값을 로드하자
			LoadEditorPref();

			//에디터 리소스 로드 초기화 (21.8.3)
			s_isEditorResourcesLoaded = false;

			PhysicsPreset.Load();//<<로드!
			PhysicsPreset.Save();//그리고 바로 저장 한번 더

			ControlParamPreset.Load();
			ControlParamPreset.Save();

			//추가 22.6.13 [v1.4.0]
			AnimEventPreset.Load();
			AnimEventPreset.Save();

			//추가 21.2.27
			Rotoscoping.DestroyAllImages();
			Rotoscoping.Load();
			Rotoscoping.Save();


			_isMakePortraitRequest = false;
			_isMakePortraitRequestFromBackupFile = false;

			_isFullScreenGUI = false;
			_isInvertBackgroundColor = false;

			//변경 19.8.18
			_uiFoldType_Left = UI_FOLD_TYPE.Unfolded;
			_uiFoldType_Right1 = UI_FOLD_TYPE.Unfolded;
			_uiFoldType_Right1_Upper = UI_FOLD_TYPE.Unfolded;
			_uiFoldType_Right1_Lower = UI_FOLD_TYPE.Unfolded;
			_uiFoldType_Right2 = UI_FOLD_TYPE.Unfolded;


			apDebugLog.I.Clear();

			if(_boneGUIRenderMode == BONE_RENDER_MODE.None)
			{
				//본을 숨겨둔 상태라면 보이게 만든다.
				_boneGUIRenderMode = BONE_RENDER_MODE.Render;
			}
			_meshGUIRenderMode = MESH_RENDER_MODE.Render;

			//if (_meshGenerator == null)
			//{
			//	_meshGenerator = new apMeshGenerator(this);
			//}

			if (_mirrorVertexSet == null)
			{
				_mirrorVertexSet = new apMirrorVertexSet(this);
			}

			_isRequestRemoveVerticesIfImportedFromPSD_Step1 = false;
			_isRequestRemoveVerticesIfImportedFromPSD_Step2 = false;
			_requestMeshRemoveVerticesIfImportedFromPSD = null;

			if (isShowEditor)
			{
				_isLockOnEnable = false;
			}

			_isHierarchyOrderEditEnabled = false;//<<추가 : SortMode 해제

			//추가 19.11.21
			if(_guiStyleWrapper == null)
			{
				_guiStyleWrapper = new apGUIStyleWrapper();
			}
			

			//추가 19.12.2
			if(_stringFactory == null)
			{
				_stringFactory = new apStringFactory();
			}
			

			if (_guiStringWrapper_32 == null) { _guiStringWrapper_32 = new apStringWrapper(32); }
			if (_guiStringWrapper_64 == null) { _guiStringWrapper_64 = new apStringWrapper(64); }
			if (_guiStringWrapper_128 == null) { _guiStringWrapper_128 = new apStringWrapper(128); }
			if (_guiStringWrapper_256 == null) { _guiStringWrapper_256 = new apStringWrapper(256); }

			if(_guiLOFactory == null)
			{
				_guiLOFactory = new apGUILOFactory();
			}
			


			




			//추가 20.4.6 : 로딩 팝업 초기화
			_isProgressPopup = false;
			_isProgressPopup_StartRequest = false;
			_isProgressPopup_CompleteRequest = false;
			_proogressPopupRatio = 0.0f;
			_isProogressPopup_Cancelable = false;
			_funcProgressPopupCancel = null;
			if(_strProgressPopup_Title == null)
			{
				_strProgressPopup_Title = new apStringWrapper(128);
			}
			if(_strProgressPopup_Info == null)
			{
				_strProgressPopup_Info = new apStringWrapper(128);
			}

			EditorUtility.ClearProgressBar();


			//추가 21.5.13 : CPP 로드 테스트 : 사용하는 경우에 한해서
			if(_cppPluginOption_UsePlugin)
			{
				_cppPluginValidateResult = apPluginUtil.I.ValidateDLL();
			}
			
			//추가 21.6.26 : Undo History 초기화
			apUndoHistory.MakeNewGameObject();





			//비동기 로딩 초기화
			ClearLoadingPortraitAsync();

			//렌더 요청 초기화
			if(_renderRequest_Normal == null)
			{
				_renderRequest_Normal = new apGL.RenderTypeRequest();
			}
			if(_renderRequest_Selected == null)
			{
				_renderRequest_Selected = new apGL.RenderTypeRequest();
			}


			//Hierarchy 클릭 요청 초기화
			_lastClickedHierarchy = LAST_CLICKED_HIERARCHY.None;
			_isReadyToCheckClickedHierarchy = false;
			_isClickedHierarchyProcessed = false;
			_curEventTypeBeforeAnyUsed = EventType.KeyUp;

			//추가 v1.4.2 : GUI의 버텍스 렌더 크기/클릭 범위등의 설정 초기화
			if(_guiRenderSettings == null)
			{
				_guiRenderSettings = new apGUIRenderSettings();
			}
		}

		



		// Update
		//-----------------------------------------------------------------------------------
		void Update()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			//업데이트 시간과 상관없이 Update를 호출하는 시간 간격을 모두 기록한다.
			//재생 시간과 별도로 "Repaint 하지 않아도 되는 불필요한 시간"을 체크하기 위함

			//추가 3.1 : CPU 프레임이 낮아도 되는지 체크
			CheckLowCPUOption();


			//Update 타입의 타이머를 작동한다.
			if (UpdateFrameCount(FRAME_TIMER_TYPE.Update))//중요! : 여기서 리턴값에 의해서 업데이트 빈도를 조절한다. (실제 FPS는 아니다.)
			{
				//동기화가 되었다.
				//Update에 의한 Repaint가 유효하다.
				_isRepaintTimerUsable = true;
			}

			//Debug.Log("Update [" + _isRepaintTimerUsable + "]");

			_tMemGC += DeltaTime_UpdateAllFrame;
			if (_tMemGC > 30.0f)
			{
				//System.GC.AddMemoryPressure(1024 * 200);//200MB 정도 압박을 줘보자
				System.GC.Collect();

				_tMemGC = 0.0f;
			}

			if (_isRepaintable)
			{
				//바로 Repaint : 강제로 Repaint를 하는 경우
				_isRepaintable = false;
				//_prevDateTime = DateTime.Now;

				//강제로 호출한 건 Usable의 영향을 받지 않는다.
				_isValidGUIRepaint = true;
				Repaint();
				_isRepaintTimerUsable = false;
			}
			else
			{
				if (!_isUpdateSkip)
				{
					if (_isRepaintTimerUsable)
					{
						_isValidGUIRepaint = true;
						Repaint();
					}
					_isRepaintTimerUsable = false;
				}

				_isUpdateSkip = false;


			}

			//Notification이나 없애주자
			if (_isNotification)
			{
				_tNotification -= DeltaTime_UpdateAllFrame;
				if (_tNotification < 0.0f)
				{
					_isNotification = false;
					_tNotification = 0.0f;
					RemoveNotification();
				}
			}
			if (_isNotification_GUI)
			{
				_tNotification_GUI -= DeltaTime_UpdateAllFrame;
				if (_tNotification_GUI < 0.0f)
				{
					_isNotification_GUI = false;
					_tNotification_GUI = 0.0f;
				}
			}
			//백업 레이블 애니메이션 처리를 하자
			if (_isBackupProcessing != Backup.IsAutoSaveWorking())
			{
				_isBackupProcessing = Backup.IsAutoSaveWorking();
				_tBackupProcessing_Icon = 0.0f;
				_tBackupProcessing_Label = 0.0f;
			}
			if (_isBackupProcessing)
			{
				_tBackupProcessing_Icon += DeltaTime_UpdateAllFrame;
				_tBackupProcessing_Label += DeltaTime_UpdateAllFrame;

				if (_tBackupProcessing_Icon > BACKUP_ICON_TIME_LENGTH)
				{
					_tBackupProcessing_Icon -= BACKUP_ICON_TIME_LENGTH;
				}

				if (_tBackupProcessing_Label > BACKUP_LABEL_TIME_LENGTH)
				{
					_tBackupProcessing_Label -= BACKUP_LABEL_TIME_LENGTH;
				}
			}
			//_isCountDeltaTime = false;

			//_prevUpdateFrameDateTime = DateTime.Now;

			//타이머 종료
			//UpdateFrameCount(FRAME_TIMER_TYPE.None);


		}



		// GUI 그리기 < 중요 >
		//--------------------------------------------------------------------------------------------
		void OnGUI()
		{

			if (Application.isPlaying)
			{
				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;

				EditorGUILayout.BeginVertical(GUILayout.Width((int)position.width), GUILayout.Height((int)position.height));

				GUILayout.Space((windowHeight / 2) - 10);
				EditorGUILayout.BeginHorizontal();
				GUIStyle guiStyle_CenterLabel = new GUIStyle(GUI.skin.label);//이건 최적화 대상 아님
				guiStyle_CenterLabel.alignment = TextAnchor.MiddleCenter;

				EditorGUILayout.LabelField("Unity Editor is Playing.", guiStyle_CenterLabel, GUILayout.Width((int)position.width));

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();

				_isUpdateAfterEditorRunning = true;
				return;
			}


			if (_isUpdateAfterEditorRunning)
			{
				Init(false);
				_isUpdateAfterEditorRunning = false;
			}

			//최신 버전을 체크한다. (1회만 수행)
			CheckCurrentLiveVersion();

			_curEventType = Event.current.type;
			_curEventTypeBeforeAnyUsed = Event.current.type;

			if (Event.current.type == EventType.Repaint)
			{
				//_isCountDeltaTime = true;
				//GUI Repaint 타입의 타이머를 작동한다.
				UpdateFrameCount(FRAME_TIMER_TYPE.Repaint);
			}
			else
			{
				//_isCountDeltaTime = false;
				//이 호출에서는 타이머 종료
				//UpdateFrameCount(FRAME_TIMER_TYPE.None);
			}

			//v1.4.2 : Hierarchy 클릭 여부 체크
			//마우스 Down-Pressed-Up (휠 제외) 모두 Hierarchy 입력을 받을 준비를 한다.
			//실제로는 Hierarchy 입력은 MouseUp에서 받지만, 그 외의 상황에서는 항상 선택 해제가 되도록
			if(Event.current.isMouse)
			{
				if(_curEventType == EventType.MouseUp
					|| _curEventType == EventType.MouseDown
					|| _curEventType == EventType.MouseDrag)
				{
					_isReadyToCheckClickedHierarchy = true;
					_isClickedHierarchyProcessed = false;
					_lastClickedHierarchy = LAST_CLICKED_HIERARCHY.None;//일단 해제
				}
			}


			//언어팩 옵션 적용
			//_localization.SetLanguage(_language);


			if (_portrait != null)
			{
				//포커스가 EditorWindow에 없다면 물리 사용을 해제한다.
				_portrait._isPhysicsSupport_Editor = (EditorWindow.focusedWindow == this) && !Onion.IsVisible;
			}

			////return;
			//UnityEngine.Profiling.Profiler.BeginSample("Editor Main GUI [" + Event.current.type + "]");
			
			//v1.4.2 : Undo 처리를 준비하는 함수를 호출하자
			apEditorUtil.ReadyToRecordUndo();



			try
			{
				//CheckEditorResources()//이전

				//변경 21.8.3 : 한번만 체크하자
				//다시 체크해야하는 경우
				//- 언어를 바꾸었을때
				//- Portrait를 바꾸었을때
				if (!s_isEditorResourcesLoaded)
				{
					if(CheckEditorResources())
					{
						s_isEditorResourcesLoaded = true;
					}
				}
				

				
				//추가 20.4.6 : 로딩 팝업을 보여주고 입력을 제한한다.
				if(CheckAndShowProgressPopup())
				{
					//로딩 팝업이 출력된 상태에서는,
					//- 마우스 Up 이벤트로 강제. 그 이후에는 새로 Down 이벤트를 받지 못한다. 휠값도 없앤다.
					//- 키보드입력은 이미 Use가 되어서 괜찮다.
					//- EditorController, Gizmo에 락을 건다.
					Controller.LockInputWhenPopupShown();
				}
				else
				{
					//정상적으로 입력을 한다.
					Controller.CheckInputEvent();
				}


				

				HotKey.Clear();


				if (_isFirstOnGUI)
				{
					LoadEditorPref();

					//추가 : HotKey설정을 가져오자
					if(_hotKeyMap == null)
					{
						_hotKeyMap = new apHotKeyMapping();
					}
					_hotKeyMap.Load();

#if UNITY_EDITOR_OSX
					bool isStartScreenShown = false;
#endif
					if (apVersion.I.IsDemo)
					{
						//데모인 경우 : 항상 나옴 / 추가 다이얼로그도 그냥 다 나온다.
						apDialog_StartPage.ShowDialog(	this, 
														ImageSet.Get(apImageSet.PRESET.StartPageLogo_Demo), 
														ImageSet.Get(apImageSet.PRESET.StartPage_GettingStarted),
														ImageSet.Get(apImageSet.PRESET.StartPage_VideoTutorial),
														ImageSet.Get(apImageSet.PRESET.StartPage_Manual),
														ImageSet.Get(apImageSet.PRESET.StartPage_Forum),
														false);
					}
					else
					{
						if (_startScreenOption_IsShowStartup)
						{
							//데모가 아닌 경우 : 옵션에 따라 완전 처음 또는 매일 한번 나온다.
							//옵션 True + 날짜가 달라야 나온다.
							if (DateTime.Now.Month != _startScreenOption_LastMonth ||
								DateTime.Now.Day != _startScreenOption_LastDay
								//|| true//테스트
								)
							{
								apDialog_StartPage.ShowDialog(	this, 
																ImageSet.Get(apImageSet.PRESET.StartPageLogo_Full), 
																ImageSet.Get(apImageSet.PRESET.StartPage_GettingStarted),
																ImageSet.Get(apImageSet.PRESET.StartPage_VideoTutorial),
																ImageSet.Get(apImageSet.PRESET.StartPage_Manual),
																ImageSet.Get(apImageSet.PRESET.StartPage_Forum),
																true);

								_startScreenOption_LastMonth = DateTime.Now.Month;
								_startScreenOption_LastDay = DateTime.Now.Day;
#if UNITY_EDITOR_OSX
								isStartScreenShown = true;
#endif

								SaveEditorPref();
							}
						}
					}
					
		
					//삭제 22.7.1 : 애플 실리콘 + 유니티 최신버전에선 무조건 Metal을 써야한다.
//					//만약 Mac인데 처음 실행했다면 > 안내 다이얼로그 보여줘야함
//					//+ StartScreen가 나타나지 않는 경우에만 보여주도록 하자
//#if UNITY_EDITOR_OSX
//					if(_macOSXInfoScreenOption_IsShowStartup && !isStartScreenShown)
//					{
//						if (DateTime.Now.Month != _macOSXInfoScreenOption_LastMonth ||
//								DateTime.Now.Day != _macOSXInfoScreenOption_LastDay)
//						{
//							//항상 출력된다는 옵션이 있어도 날짜가 달라야 나온다.
//							apDialog_MacMetalInfo.ShowDialog(this, _language);

//							_macOSXInfoScreenOption_LastMonth = DateTime.Now.Month;
//							_macOSXInfoScreenOption_LastDay = DateTime.Now.Day;

//							SaveEditorPref();
//						}
//					}
//#endif



					//업데이트 로그도 띄우자
					if (_updateLogScreen_LastVersion != apVersion.I.APP_VERSION_INT)
					{
						apDialog_UpdateLog.ShowDialog(this);

						_updateLogScreen_LastVersion = apVersion.I.APP_VERSION_INT;

						SaveEditorPref();
					}



					_isFirstOnGUI = false;
				}



				//현재 GUI 이벤트가 Layout/Repaint 이벤트인지
				//또는 마우스/키보드 등의 다른 이벤트인지 판별
				if (Event.current.type != EventType.Layout
					&& Event.current.type != EventType.Repaint)
				{
					_isGUIEvent = false;
				}
				else
				{
					_isGUIEvent = true;
				}

				//추가 21.6.30
				if(Event.current.rawType == EventType.MouseUp)
				{
					//마우스 Up 이벤트라면 Undo 기록을 초기화한다. (Continuous 못하게)
					apEditorUtil.ResetUndoContinuous();
				}


				//자동 저장 기능
				Backup.CheckAutoBackup(this, Event.current.type);


				int windowWidth = (int)position.width;
				int windowHeight = (int)position.height;

				//윈도우 크기를 변수에 저장하자
				//_lastWindowWidth = windowWidth;
				_lastWindowHeight = windowHeight;

				//GUILayoutOption Wraper에 크기를 체크해주자
				apGUILOFactory.I.CheckSize(windowWidth, windowHeight);


				//버텍스, 핀 렌더 설정도 여기서 업데이트한다. [v1.4.2]
				GUIRenderSettings.UpdateRenderSettings(VertGUIOption_SizeRatioX100, windowHeight);
				apGL.SetVertexPinRenderOption(	GUIRenderSettings.VertexRenderSize_Half,
												GUIRenderSettings.PinRenderSize_Half,
												GUIRenderSettings.PinLineThickness);
				

				bool isTopVisible = true;
				bool isLeftVisible = true;
				bool isRightVisible = true;

				//int topHeight = 45;//이전 v1.2.6 까지
				int topHeight = 35;//변경 v1.3.0 : 상단 영역을 줄인다.

				//Bottom 레이아웃은 일부 기능에서만 나타난다.
				int bottomTimelineHeight = 0;
				bool isBottomVisible_Timeline = false;

				//추가 Bottom위에 Edit 툴이 나오는 Bottom 2 Layout이 있다.
				int bottomEditButtonsHeight = 45;

				//이전
				//bool isBottom2Render = (Select.ExEditMode != apSelection.EX_EDIT_KEY_VALUE.None) && (_meshGroupEditMode == MESHGROUP_EDIT_MODE.Modifier);

				//변경 22.5.14
				bool isBottomVisible_EditButtons = Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
										&& _meshGroupEditMode == MESHGROUP_EDIT_MODE.Modifier
										&& Select.Modifier != null;

				if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
				{
					//TODO : 애니메이션의 하단 레이아웃은 Summary / 4 Line / 7 Line으로 조절 가능하다
					switch (_timelineLayoutSize)
					{
						case TIMELINE_LAYOUTSIZE.Size1:
							//bottomHeight = 250;
							bottomTimelineHeight = 200;//<더축소
							break;

						case TIMELINE_LAYOUTSIZE.Size2:
							//bottomHeight = 375;
							bottomTimelineHeight = 340;
							break;

						case TIMELINE_LAYOUTSIZE.Size3:
							bottomTimelineHeight = 500;
							break;
					}
					isBottomVisible_Timeline = true;
				}



				//int marginHeight = 10;//이전 v1.2.6까지의 여백
				int marginHeight = 2;//변경 v1.3.0부터의 여백

				//int mainMarginWidth = 5;//이전 v1.2.6까지의 여백
				int mainMarginWidth = 2;//변경 v1.3.0부터의 여백

				int mainHeight = windowHeight - (topHeight + marginHeight);

				if(isBottomVisible_Timeline)
				{
					mainHeight -= marginHeight + bottomTimelineHeight;
				}

				if (isBottomVisible_EditButtons)
				{
					mainHeight -= (marginHeight + bottomEditButtonsHeight);
				}

				int topWidth = windowWidth;
				int bottomWidth = windowWidth;

				int mainLeftWidth = 250;
				int mainRightWidth = 250;
				int mainRight2Width = 250;
				
				int mainCenterWidth = windowWidth - (mainLeftWidth + mainMarginWidth * 2 + mainRightWidth);

				bool isRight2Visible = false;

				if (_isFullScreenGUI)
				{
					//< 전체 화면 모드 >
					//추가 : FullScreen이라면 -> Bottom 빼고 다 사라진다.
					//위, 양쪽의 길이는 물론이고, margin 계산도 하단 제외하고 다 빼준다.
					topHeight = 0;
					isTopVisible = false;
					isLeftVisible = false;
					isRightVisible = false;

					mainHeight = windowHeight;
					
					if (isBottomVisible_Timeline)
					{
						mainHeight -= (marginHeight + bottomTimelineHeight);
					}

					if (isBottomVisible_EditButtons)
					{
						mainHeight -= (marginHeight + bottomEditButtonsHeight);
					}

					mainLeftWidth = 0;
					mainRightWidth = 0;
					mainRight2Width = 0;

					mainCenterWidth = windowWidth;
				}
				else
				{
					//< 일반 모드 >
					if (isRightVisible)
					{
						//Right 패널이 두개가 필요한 경우 (Right GUI가 보여진다는 가정하에)
						switch (Select.SelectionType)
						{
							case apSelection.SELECTION_TYPE.MeshGroup:
								isRight2Visible = true;
								break;

							case apSelection.SELECTION_TYPE.Animation:
								isRight2Visible = true;
								break;
						}
					}

					//Fold 상태에 따라서 Width가 바뀐다. 19.8/18
					if (_uiFoldType_Left == UI_FOLD_TYPE.Folded)
					{
						mainLeftWidth = 24;
					}

					if (_uiFoldType_Right1 == UI_FOLD_TYPE.Folded)
					{
						mainRightWidth = 24;
					}

					if (_uiFoldType_Right2 == UI_FOLD_TYPE.Folded)
					{
						mainRight2Width = 24;
					}

					if (isRight2Visible)
					{
						mainCenterWidth = windowWidth - (mainLeftWidth + mainMarginWidth * 3 + mainRightWidth + mainRight2Width);
					}
					else
					{
						mainCenterWidth = windowWidth - (mainLeftWidth + mainMarginWidth * 2 + mainRightWidth);
					}
				}




				// Layout
				//-----------------------------------------
				// Top (Tab / Toolbar)
				//-----------------------------------------
				//		|						|
				//		|			Main		|
				// Func	|		GUI Editor		| Inspector
				//		|						|
				//		|						|
				//		|						|
				//		|						|
				//-----------------------------------------
				// Bottom (Timeline / Status) : 선택
				//-----------------------------------------

				//Portrait 생성 요청이 있다면 여기서 처리
				if (_isMakePortraitRequest && _curEventType == EventType.Layout)
				{
					MakeNewPortrait();
				}
				if (_isMakePortraitRequestFromBackupFile && _curEventType == EventType.Layout)
				{
					MakePortraitFromBackupFile();
				}

				Color guiBasicColor = GUI.backgroundColor;


				// Top UI : Tab Buttons / Basic Toolbar
				//-------------------------------------------------
				GUILayout.Space(5);

				if (isTopVisible)
				{
					GUI.Box(new Rect(0, 0, topWidth, topHeight), apStringFactory.I.None);

					//Profiler.BeginSample("1. GUI Top");

					GUI_Top(windowWidth, topHeight);

					//Profiler.EndSample();
					//-------------------------------------------------

					GUILayout.Space(marginHeight);
				}

				//HotKey 처리를 두자
				ProcessHotKey_GUITopRight();//<<렌더여부 상관 없이..




				//Rect mainRect_LT = GUILayoutUtility.GetLastRect();

				//-------------------------------------------------
				// Left Func + Main GUI Editor + Right Inspector
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Height(mainHeight));//GUILayout.Height(mainHeight)

				if (isLeftVisible)
				{
					// Main Left Layout
					//------------------------------------
					EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainLeftWidth));

					Rect rectMainLeft = new Rect(0, topHeight + marginHeight, mainLeftWidth, mainHeight);
					GUI.Box(rectMainLeft, apStringFactory.I.None);
					GUILayout.BeginArea(rectMainLeft, apStringFactory.I.None);

					//추가 19.8.18 : UI가 Fold될 수 있다. (전체 화면 모드와 다름)
					if (_uiFoldType_Left == UI_FOLD_TYPE.Folded)
					{
						//접혀진 탭을 펼칠 수 있다.
						UI_FOLD_BTN_RESULT foldResult_Left = apEditorUtil.DrawTabFoldTitle_H(this, 0, 0, mainLeftWidth, mainHeight, _uiFoldType_Left, true);
						if (foldResult_Left == UI_FOLD_BTN_RESULT.ToggleFold_Horizontal)
						{
							//Left의 Fold를 변경한다.
							_uiFoldType_Left = UI_FOLD_TYPE.Unfolded;
						}
					}
					else
					{
						//이전 : 기존엔 10짜리 여백
						//GUILayout.Space(10);

						//변경 19.8.17 : 탭 축소 버튼이 위쪽에
						UI_FOLD_BTN_RESULT foldResult_Left = apEditorUtil.DrawTabFoldTitle_H(this, 0, 0, mainLeftWidth, 9, _uiFoldType_Left, true);
						if (foldResult_Left == UI_FOLD_BTN_RESULT.ToggleFold_Horizontal)
						{
							//Left의 Fold를 변경한다.
							_uiFoldType_Left = UI_FOLD_TYPE.Folded;
						}


						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Height(20));
						GUILayout.Space(5);
						if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Hierarchy), _tabLeft == TAB_LEFT.Hierarchy, (mainLeftWidth - 16) / 2, 20))//"Hierarchy"
						{
							_tabLeft = TAB_LEFT.Hierarchy;
							_isHierarchyOrderEditEnabled = false;//<<추가 : SortMode 해제
						}
						if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Controller), _tabLeft == TAB_LEFT.Controller, (mainLeftWidth - 16) / 2, 20))//"Controller"
						{
							_tabLeft = TAB_LEFT.Controller;
							_isHierarchyOrderEditEnabled = false;//<<추가 : SortMode 해제
						}

						EditorGUILayout.EndHorizontal();


						//추가 20.12.4 : 단축키로 탭 전환
						AddHotKeyEvent(OnHotKeyEvent_SwitchLeftTab, apHotKeyMapping.KEY_TYPE.SwitchLeftTab, null);


						GUILayout.Space(10);

						int leftUpperHeight = GUI_LeftUpper(mainLeftWidth - 20);

						//int leftHeight = mainHeight - ((20 - 40) + leftUpperHeight);

						//변경 22.12.16 : 현재 리스트 내용에 따라 스크롤이 다르다.
						Vector2 scroll_Left = Vector2.zero;
						if (_portrait == null)
						{
							//1. Portrait가 없는 경우
							_scroll_Left_FirstPage = EditorGUILayout.BeginScrollView(_scroll_Left_FirstPage, false, true);
							scroll_Left = _scroll_Left_FirstPage;
						}
						else
						{
							if(_tabLeft == TAB_LEFT.Hierarchy)
							{
								//2. Hierarchy 탭
								_scroll_Left_Hierarchy = EditorGUILayout.BeginScrollView(_scroll_Left_Hierarchy, false, true);
								scroll_Left = _scroll_Left_Hierarchy;
							}
							else
							{
								//3. Controller 탭
								_scroll_Left_Controller = EditorGUILayout.BeginScrollView(_scroll_Left_Controller, false, true);
								scroll_Left = _scroll_Left_Controller;
							}
						}
						

						//크기 체크
						apGUILOFactory.I.CheckSize(mainLeftWidth, mainHeight - ((20 - 40) + leftUpperHeight));

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainLeftWidth - 20), apGUILOFactory.I.Height(mainHeight - ((20 - 40) + leftUpperHeight)));

						apControllerGL.SetLayoutSize(
											//mainLeftWidth - 20,
											mainLeftWidth - 14,
											//mainHeight - ((20 - 40) + leftUpperHeight + 60),
											mainHeight - ((20 - 40) + leftUpperHeight + 66),
											(int)rectMainLeft.x,
											//(int)rectMainLeft.y + leftUpperHeight + 38,
											(int)rectMainLeft.y + leftUpperHeight + 39,
											(int)position.width, (int)position.height, scroll_Left);

						//ControllerGL의 Snap여부를 결정하자.
						//일반적으로는 True이지만, ControlParam을 제어하는 AnimTimeline 작업시에는 False가 된다.
						//bool isAnimControlParamEditing = false;

						if (_selection.SelectionType == apSelection.SELECTION_TYPE.Animation &&
								_selection.AnimClip != null &&
								_selection.ExAnimEditingMode != apSelection.EX_EDIT.None &&
								_selection.AnimTimeline != null &&
								_selection.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
						{
							apControllerGL.SetSnapWhenReleased(false);
						}
						else
						{
							apControllerGL.SetSnapWhenReleased(true);
						}

						//Profiler.BeginSample("2. GUI Left");

						GUI_Left(mainLeftWidth - 20, mainHeight - 20, scroll_Left, _isGUIEvent);

						//Profiler.EndSample();

						//apControllerGL.EndUpdate();//21.2.9 : 일단 이걸 여기선 제외

						EditorGUILayout.EndVertical();

						GUILayout.Space(50);

						EditorGUILayout.EndScrollView();
					}

					GUILayout.EndArea();

					EditorGUILayout.EndVertical();


					//------------------------------------


					GUILayout.Space(mainMarginWidth);
				}


				//Profiler.BeginSample("3. GUI Center");

				// Main Center Layout
				//------------------------------------
				//Rect mainRect_LB = GUILayoutUtility.GetLastRect();
				EditorGUILayout.BeginVertical();

				int guiViewBtnSize = 15;

				//이전 : 그냥 색상 고정
				//GUI.backgroundColor = _colorOption_Background;

				//변경 21.10.6 : 배경 색상을 빠르게 바꿀 수 있다.
				if(!_isInvertBackgroundColor)
				{
					//기존 색상
					GUI.backgroundColor = _colorOption_Background;
				}
				else
				{
					//반전된 색상
					GUI.backgroundColor = _colorOption_InvertedBackground;
				}


				Rect rectMainCenter = new Rect(mainLeftWidth + mainMarginWidth, topHeight + marginHeight, mainCenterWidth, mainHeight);
				if (_isFullScreenGUI)
				{
					//전체 화면일때 화면 구성이 바뀐다.
					rectMainCenter = new Rect(0, 0, mainCenterWidth, mainHeight);
				}



				GUI.Box(new Rect(rectMainCenter.x, rectMainCenter.y, rectMainCenter.width - guiViewBtnSize, rectMainCenter.height - guiViewBtnSize), apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box);
				GUILayout.BeginArea(rectMainCenter, apStringFactory.I.None);

				GUI.backgroundColor = guiBasicColor;
				//_scroll_MainCenter = EditorGUILayout.BeginScrollView(_scroll_MainCenter, true, true);

				//_scroll_MainCenter.y = GUI.VerticalScrollbar(new Rect(rectMainCenter.x + mainCenterWidth - 20, rectMainCenter.width, 20, rectMainCenter.height), _scroll_MainCenter.y, 1.0f, -20.0f, 20.0f);

				_scroll_CenterWorkSpace.y = GUI.VerticalScrollbar(new Rect(rectMainCenter.width - 15, 0, 20, rectMainCenter.height - guiViewBtnSize), _scroll_CenterWorkSpace.y, 5.0f, -500.0f, 500.0f + 5.0f);
				_scroll_CenterWorkSpace.x = GUI.HorizontalScrollbar(new Rect(guiViewBtnSize + 110, rectMainCenter.height - 15, rectMainCenter.width - (guiViewBtnSize + guiViewBtnSize + 110), 20), _scroll_CenterWorkSpace.x, 5.0f, -500.0f, 500.0f + 5.0f);

				//이전 > 사용 안함
				//GUIStyle guiStyle_GUIViewBtn = new GUIStyle(GUI.skin.button);
				//guiStyle_GUIViewBtn.padding = GUI.skin.label.padding;


				//화면 중심으로 이동하는 버튼
				if (GUI.Button(new Rect(rectMainCenter.width - guiViewBtnSize, rectMainCenter.height - guiViewBtnSize, guiViewBtnSize, guiViewBtnSize),
								new GUIContent(_imageSet.Get(apImageSet.PRESET.GUI_Center), apStringFactory.I.ResetZoomAndPositon),//"Reset Zoom and Position"
																																   //guiStyle_GUIViewBtn,//이전
								GUIStyleWrapper.Button_LabelPadding//변경
								))
				{
					_scroll_CenterWorkSpace = Vector2.zero;
					_iZoomX100 = ZOOM_INDEX_DEFAULT;
				}

				//전체 화면 기능 추가
				Color prevColor = GUI.backgroundColor;

				if (_isFullScreenGUI)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}





				//Alt+W를 눌러서 크기를 바꿀 수 있다.				
				AddHotKeyEvent(OnHotKeyEvent_FullScreenToggle, apHotKeyMapping.KEY_TYPE.ToggleWorkspaceSize, null);//변경 20.12.3
				
				//추가 21.10.6 : Alt+G를 눌러서 배경 색을 바꿀 수 있다.
				AddHotKeyEvent(OnHotKeyEvent_InvertBackgroundColor, apHotKeyMapping.KEY_TYPE.ToggleInvertBGColor, null);


				if(_guiStringWrapper_128 == null)
				{
					_guiStringWrapper_128 = new apStringWrapper(128);
				}


				if (GUI.Button(	new Rect(0, rectMainCenter.height - guiViewBtnSize, guiViewBtnSize, guiViewBtnSize),
								new GUIContent(_imageSet.Get(apImageSet.PRESET.GUI_FullScreen), apStringFactory.I.GetHotkeyTooltip_ToggleWorkspaceSize(HotKeyMap)),//"Toogle Workspace Size (Alt+W)"
								GUIStyleWrapper.Button_LabelPadding//변경
								))
				{
					_isFullScreenGUI = !_isFullScreenGUI;
				}
				GUI.backgroundColor = prevColor;

				//추가 : GUI_Top에 있었던 Zoom이 여기로 왔다.
				int minZoom = 0;
				int maxZoom = _zoomListX100.Length - 1;

				//float fZoom = GUILayout.HorizontalSlider(_iZoomX100, minZoom, maxZoom + 0.5f, GUILayout.Width(60));
				float fZoom = GUI.HorizontalSlider(new Rect(guiViewBtnSize + 5, rectMainCenter.height - (guiViewBtnSize + 1), 40, guiViewBtnSize), _iZoomX100, minZoom, maxZoom + 0.5f);
				_iZoomX100 = Mathf.Clamp((int)fZoom, 0, _zoomListX100.Length - 1);

				//GUI.Label(new Rect(guiViewBtnSize + 50, rectMainCenter.height - guiViewBtnSize, 60, guiViewBtnSize), _zoomListX100[_iZoomX100] + "%");//이전
				GUI.Label(new Rect(guiViewBtnSize + 50, rectMainCenter.height - guiViewBtnSize, 60, guiViewBtnSize), _zoomListX100_Label[_iZoomX100]);//변경 : String 최적화

				//줌 관련 처리를 해보자
				//bool isZoomControlling = Controller.GUI_Input_ZoomAndScroll();

				apGL.ResetCursorEvent();

				float fZoomRatio = (float)(_zoomListX100[_iZoomX100]) * 0.01f;
				apGL.SetWindowSize(mainCenterWidth, mainHeight,
									_scroll_CenterWorkSpace,
									fZoomRatio,
									(int)rectMainCenter.x, (int)rectMainCenter.y,
									(int)position.width, (int)position.height);

				//추가 20.3.21 : 본 렌더링을 위해서 현재 설정을 갱신하자
				apBone.SetRenderSettings(	_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version2,
											BoneGUIOption_SizeRatioX100,
											_boneGUIOption_ScaledByZoom,
											fZoomRatio);




				Controller.GUI_Input_ZoomAndScroll(
#if UNITY_EDITOR_OSX
					Event.current.command,
#else
					Event.current.control,
#endif
					Event.current.shift,
					Event.current.alt
					);





				//크기 체크
				apGUILOFactory.I.CheckSize(mainCenterWidth, mainHeight);

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainCenterWidth - 20), apGUILOFactory.I.Height(mainHeight - 20));

				_mainGUIRect = new Rect(rectMainCenter.x, rectMainCenter.y, rectMainCenter.width - 20, rectMainCenter.height - 20);

				//추가 4.10. 만약, Capture도중이면 화면 이동/줌이 강제다
				if (_isScreenCaptureRequest
#if UNITY_EDITOR_OSX
					|| _isScreenCaptureRequest_OSXReady
#endif
					)
				{
					if (_screenCaptureRequest != null)
					{
						_scroll_CenterWorkSpace = _screenCaptureRequest._screenPosition;
						_iZoomX100 = _screenCaptureRequest._screenZoomIndex;
					}
				}


				//CheckGizmoAvailable();
				//if (Event.current.type == EventType.Repaint || Event.current.isMouse || Event.current.isKey)
				//{
				//	_gizmos.ReadyToUpdate();
				//}
				_gizmos.ReadyToUpdate();

				GUI_Center(mainCenterWidth - 20, mainHeight - 20);

				if (Event.current.type == EventType.Repaint ||
					Event.current.isMouse ||
					Event.current.isKey ||
					_gizmos.IsDrawFlag)
				{
					_gizmos.EndUpdate();

					_gizmos.GUI_Render_Controller(this);
				}
				if (Event.current.type == EventType.Repaint)
				{
					_gizmos.CheckUpdate(DeltaTime_Repaint);
				}
				else
				{
					_gizmos.CheckUpdate(0.0f);
				}

				
				EditorGUILayout.EndVertical();
				//EditorGUILayout.EndScrollView();


				GUILayout.EndArea();

				EditorGUILayout.EndVertical();

				//------------------------------------

				//Profiler.EndSample();


				if (isRightVisible)
				{
					GUILayout.Space(mainMarginWidth);


					// Main Right Layout
					//------------------------------------

					//Profiler.BeginSample("4. GUI Right");

					EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainRightWidth));

					GUI.backgroundColor = guiBasicColor;

					bool isRightUpper_Scroll = true;

					bool isRightLower = false;
					bool isRightLower_Scroll = false;
					RIGHT_LOWER_SCROLL_TYPE rightLowerScrollType = RIGHT_LOWER_SCROLL_TYPE.MeshGroup_Mesh;//0 : MeshGroup - Mesh, 1 : MeshGroup - Bone, 2 : Animation
					bool isRightLower_2LineHeader = false;

					int rightUpperHeight = mainHeight;
					int rightLowerHeight = 0;

					//Mesh Group / Animation인 경우 우측의 레이아웃이 2개가 된다.
					if (_uiFoldType_Right1 == UI_FOLD_TYPE.Unfolded
						&&
						(Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup || Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
						)
					{
						isRightLower = true;
						//if (_rightLowerLayout == RIGHT_LOWER_LAYOUT.Hide)//이전
						if (_uiFoldType_Right1_Lower == UI_FOLD_TYPE.Folded)//변경 19.8.18
						{
							//하단이 Hide일때
							//if (_right_UpperLayout == RIGHT_UPPER_LAYOUT.Show)//이전
							if (_uiFoldType_Right1_Upper == UI_FOLD_TYPE.Unfolded)//변경 19.8.18
							{
								//상단이 Show일때 : 위에만 보일때
								//rightUpperHeight = mainHeight - (36 + marginHeight);
								rightUpperHeight = mainHeight - (15 + marginHeight);
							}
							else
							{
								//상단이 Hide일때 : 둘다 안보일때
								//rightUpperHeight = 36;
								rightUpperHeight = 15;
								isRightUpper_Scroll = false;
							}


							//rightLowerHeight = 36;
							rightLowerHeight = 15;

							//변경 19.8.18 : 상하로 UI가 축소되었을때의 크기가 36에서 15로 변경

							isRightLower_Scroll = false;
							isRightLower_2LineHeader = false;
						}
						else
						{
							//하단이 Show일때
							//if (_right_UpperLayout == RIGHT_UPPER_LAYOUT.Show)//이전
							if (_uiFoldType_Right1_Upper == UI_FOLD_TYPE.Unfolded)//변경 19.8.18
							{
								//상단이 Show일때 : 둘다 보일 때 > 반반
								//rightUpperHeight = mainHeight / 2;//이전 : 50%씩
								rightUpperHeight = (int)(mainHeight * 0.4f);//변경 21.3.13 : 40%만 먹어랑
							}
							else
							{
								//상단이 Hide일때 : 아래만 보일때
								//rightUpperHeight = 36;
								rightUpperHeight = 15;
								isRightUpper_Scroll = false;
							}

							//변경 19.8.18 : 상하로 UI가 축소되었을때의 크기가 36에서 9로 변경

							rightLowerHeight = mainHeight - (rightUpperHeight + marginHeight);
							isRightLower_Scroll = true;
							rightLowerScrollType = 0;

							if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
							{
								if (_meshGroupEditMode == MESHGROUP_EDIT_MODE.Setting || _meshGroupEditMode == MESHGROUP_EDIT_MODE.Bone)
								{
									isRightLower_2LineHeader = true;
								}
								else
								{
									isRightLower_2LineHeader = false;
								}

								if (Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
								{
									rightLowerScrollType = RIGHT_LOWER_SCROLL_TYPE.MeshGroup_Mesh;

									//이벤트 타이밍 동기화도 한다 - 메시탭
									if (Event.current.type == EventType.Layout)
									{
										SetGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Meshes, true);
									}
								}
								else
								{
									rightLowerScrollType = RIGHT_LOWER_SCROLL_TYPE.MeshGroup_Bone;

									//이벤트 타이밍 동기화도 한다 - 본탭
									if (Event.current.type == EventType.Layout)
									{
										SetGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Bones, true);
									}
								}
							}
							else//if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
							{
								isRightLower_2LineHeader = false;

								//선택된 타임라인에 따라 우측 스크롤의 종류가 바뀐다.
								if (Select.AnimTimeline != null
									&& Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
								{
									//선택된 타임라인이 있고 Control Param일때
									rightLowerScrollType = RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam;

									//이벤트 타이밍 동기화도 한다 - Anim-컨트롤파라미터
									if (Event.current.type == EventType.Layout)
									{
										SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam, true);//"GUI Anim Hierarchy Delayed - ControlParam"
									}
								}
								else
								{
									if (Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
									{
										rightLowerScrollType = RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh;

										//이벤트 타이밍 동기화도 한다 - Anim-메시탭
										if (Event.current.type == EventType.Layout)
										{
											SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, true);//"GUI Anim Hierarchy Delayed - Meshes"
										}
									}
									else
									{
										rightLowerScrollType = RIGHT_LOWER_SCROLL_TYPE.Anim_Bone;

										//이벤트 타이밍 동기화도 한다 - Anim-본탭
										if (Event.current.type == EventType.Layout)
										{
											SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, true);//"GUI Anim Hierarchy Delayed - Bone"
										}
									}
								}
							}
						}
					}

					//크기 체크
					apGUILOFactory.I.CheckSize_Width(mainRightWidth);

					Rect recMainRight = new Rect(mainLeftWidth + mainMarginWidth * 2 + mainCenterWidth, topHeight + marginHeight, mainRightWidth, rightUpperHeight);
					GUI.Box(recMainRight, apStringFactory.I.None);
					GUILayout.BeginArea(recMainRight, apStringFactory.I.None);
					if (_uiFoldType_Right1 == UI_FOLD_TYPE.Folded)
					{
						//Right1 UI가 모두 접힌 경우
						//변경 19.8.17 : 탭 축소 버튼이 위쪽에
						UI_FOLD_BTN_RESULT foldResult_Right = apEditorUtil.DrawTabFoldTitle_HV(this, 0, 0, mainRightWidth, rightUpperHeight, _uiFoldType_Right1, _uiFoldType_Right1_Upper);
						if (foldResult_Right == UI_FOLD_BTN_RESULT.ToggleFold_Horizontal)
						{
							//Right의 Fold를 변경한다.
							_uiFoldType_Right1 = UI_FOLD_TYPE.Unfolded;
						}
					}
					else
					{
						//변경 19.8.17 : 탭 축소 버튼이 위쪽에
						UI_FOLD_BTN_RESULT foldResult_RightUpper = UI_FOLD_BTN_RESULT.None;
						if (isRightLower)
						{
							//세로로 접기 버튼도 존재
							foldResult_RightUpper = apEditorUtil.DrawTabFoldTitle_HV(this, 0, 0, mainRightWidth, (!isRightUpper_Scroll ? rightUpperHeight : 9), _uiFoldType_Right1, _uiFoldType_Right1_Upper);
						}
						else
						{
							//가로 버튼만 존재
							foldResult_RightUpper = apEditorUtil.DrawTabFoldTitle_H(this, 0, 0, mainRightWidth, (!isRightUpper_Scroll ? rightUpperHeight : 9), _uiFoldType_Right1, false);
						}

						if (foldResult_RightUpper == UI_FOLD_BTN_RESULT.ToggleFold_Horizontal)
						{
							//Right의 Fold를 변경한다.
							_uiFoldType_Right1 = UI_FOLD_TYPE.Folded;
						}
						else if (foldResult_RightUpper == UI_FOLD_BTN_RESULT.ToggleFold_Vertical)
						{
							//Right Upper의 Fold를 변경한다.
							_uiFoldType_Right1_Upper = (_uiFoldType_Right1_Upper == UI_FOLD_TYPE.Folded ? UI_FOLD_TYPE.Unfolded : UI_FOLD_TYPE.Folded);
						}




						if (isRightUpper_Scroll)
						{
							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainRightWidth - 20), apGUILOFactory.I.Height(25));

							GUI_Right1_Header(mainRightWidth - 8, 20);
							EditorGUILayout.EndVertical();

							_scroll_Right1_Upper = EditorGUILayout.BeginScrollView(_scroll_Right1_Upper, false, true);

							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainRightWidth - 20), apGUILOFactory.I.Height(rightUpperHeight - (20 + 30)));

							GUI_Right1(mainRightWidth - 24, rightUpperHeight - (20 + 30));
							#region [미사용 코드]
							//switch (_tab)
							//{
							//	case TAB.ProjectSetting: GUI_MainRight_ProjectSetting(mainRightWidth - 24, mainHeight - 20); break;
							//	case TAB.MeshEdit: GUI_MainRight_MeshEdit(mainRightWidth - 24, mainHeight - 20); break;
							//	case TAB.FaceEdit: GUI_MainRight_FaceEdit(mainRightWidth - 24, mainHeight - 20); break;
							//	case TAB.RigEdit: GUI_MainRight_RigEdit(mainRightWidth - 24, mainHeight - 20); break;
							//	case TAB.Animation: GUI_MainRight_Animation(mainRightWidth - 24, mainHeight - 20); break;
							//} 
							#endregion
							EditorGUILayout.EndVertical();

							GUILayout.Space(500);

							EditorGUILayout.EndScrollView();
						}
					}
					GUILayout.EndArea();


					if (isRightLower)
					{
						GUILayout.Space(marginHeight);

						Rect recMainRight_Lower = new Rect(mainLeftWidth + mainMarginWidth * 2 + mainCenterWidth,
															topHeight + marginHeight + rightUpperHeight + marginHeight,
															mainRightWidth,
															rightLowerHeight);

						GUI.Box(recMainRight_Lower, apStringFactory.I.None);
						GUILayout.BeginArea(recMainRight_Lower, apStringFactory.I.None);

						//추가 19.8.17 : 탭 축소 버튼이 위쪽에
						UI_FOLD_BTN_RESULT foldResult_RightLower = apEditorUtil.DrawTabFoldTitle_V(this, 0, 0, mainRightWidth, (!isRightLower_Scroll ? rightLowerHeight : 9), _uiFoldType_Right1_Lower);
						if (foldResult_RightLower == UI_FOLD_BTN_RESULT.ToggleFold_Vertical)
						{
							//Right Upper의 Fold를 변경한다.
							_uiFoldType_Right1_Lower = (_uiFoldType_Right1_Lower == UI_FOLD_TYPE.Folded ? UI_FOLD_TYPE.Unfolded : UI_FOLD_TYPE.Folded);
						}

						if (isRightLower_Scroll)
						{
							int rightLowerHeaderHeight = 30;
							if (isRightLower_2LineHeader)
							{
								rightLowerHeaderHeight = 65;
							}

							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainRightWidth - 20), apGUILOFactory.I.Height(rightLowerHeaderHeight));
							GUILayout.Space(6);
							if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
							{
								GUI_Right1_Lower_MeshGroupHeader(mainRightWidth - 8, rightLowerHeaderHeight - 6, isRightLower_2LineHeader);
							}
							else if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
							{
								GUI_Right1_Lower_AnimationHeader(mainRightWidth - 8, rightLowerHeaderHeight - 6);
							}
							EditorGUILayout.EndVertical();

							Vector2 lowerScrollValue = _scroll_Right1_Lower_MG_Mesh;

							//스크롤은 이벤트 타이밍이 맞지 않다면 더미 스크롤값을 이용한다.
							bool isValidScroll = false;
							
							switch (rightLowerScrollType)
							{
								case RIGHT_LOWER_SCROLL_TYPE.MeshGroup_Mesh:
									{
										if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Meshes))
										{
											_scroll_Right1_Lower_MG_Mesh = EditorGUILayout.BeginScrollView(_scroll_Right1_Lower_MG_Mesh, false, true);
											lowerScrollValue = _scroll_Right1_Lower_MG_Mesh;
											isValidScroll = true;
										}
									}
									break;

								case RIGHT_LOWER_SCROLL_TYPE.MeshGroup_Bone:
									{
										if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Bones))
										{
											_scroll_Right1_Lower_MG_Bone = EditorGUILayout.BeginScrollView(_scroll_Right1_Lower_MG_Bone, false, true);
											lowerScrollValue = _scroll_Right1_Lower_MG_Bone;
											isValidScroll = true;
										}
									}
									break;

								case RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh:
									{
										if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes))
										{
											_scroll_Right1_Lower_Anim_Mesh = EditorGUILayout.BeginScrollView(_scroll_Right1_Lower_Anim_Mesh, false, true);
											lowerScrollValue = _scroll_Right1_Lower_Anim_Mesh;
											isValidScroll = true;
										}
									}
									break;

								case RIGHT_LOWER_SCROLL_TYPE.Anim_Bone:
									{
										if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone))
										{
											_scroll_Right1_Lower_Anim_Bone = EditorGUILayout.BeginScrollView(_scroll_Right1_Lower_Anim_Bone, false, true);
											lowerScrollValue = _scroll_Right1_Lower_Anim_Bone;
											isValidScroll = true;
										}
									}
									break;

								case RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam:
									{
										if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam))
										{
											_scroll_Right1_Lower_Anim_ControlParam = EditorGUILayout.BeginScrollView(_scroll_Right1_Lower_Anim_ControlParam, false, true);
											lowerScrollValue = _scroll_Right1_Lower_Anim_ControlParam;
											isValidScroll = true;
										}
									}
									break;
							}

							//더미를 사용하자
							if(!isValidScroll)
							{
								EditorGUILayout.BeginScrollView(_scroll_Right1_Lower_Dummy, false, true);
								lowerScrollValue = _scroll_Right1_Lower_Dummy;
							}

							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainRightWidth - 20), apGUILOFactory.I.Height(rightLowerHeight - 20));

							GUI_Right1_Lower(mainRightWidth - 24, rightLowerHeight - 20, lowerScrollValue, _isGUIEvent);


							//[v1.4.2] Right Lower Height를 변수에 저장하자. 자동 스크롤용
							_lastUIHeight_Right1Lower = rightLowerHeight;


							EditorGUILayout.EndVertical();

							GUILayout.Space(500);

							EditorGUILayout.EndScrollView();
						}
						GUILayout.EndArea();
					}


					EditorGUILayout.EndVertical();



					//------------------------------------
					if (isRight2Visible)
					{
						//SetGUIVisible("Right2GUI", true);
						SetGUIVisible(DELAYED_UI_TYPE.Right2GUI, true);
					}
					else
					{
						//SetGUIVisible("Right2GUI", false);
						SetGUIVisible(DELAYED_UI_TYPE.Right2GUI, false);
					}

					//if (IsDelayedGUIVisible("Right2GUI"))
					if (IsDelayedGUIVisible(DELAYED_UI_TYPE.Right2GUI))
					{
						GUILayout.Space(mainMarginWidth);

						//크기 체크
						apGUILOFactory.I.CheckSize_Width(mainRight2Width);


						// Main Right Layout
						//------------------------------------
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainRight2Width));

						GUI.backgroundColor = guiBasicColor;

						Rect recMainRight2 = new Rect(mainLeftWidth + mainMarginWidth * 3 + mainCenterWidth + mainRightWidth, topHeight + marginHeight, mainRight2Width, mainHeight);
						GUI.Box(recMainRight2, apStringFactory.I.None);
						GUILayout.BeginArea(recMainRight2, apStringFactory.I.None);

						if (_uiFoldType_Right2 == UI_FOLD_TYPE.Folded)
						{
							//탭 축소에서 펼치기
							UI_FOLD_BTN_RESULT foldResult_Right2 = apEditorUtil.DrawTabFoldTitle_H(this, 0, 0, mainRight2Width, mainHeight, _uiFoldType_Right2, false);
							if (foldResult_Right2 == UI_FOLD_BTN_RESULT.ToggleFold_Horizontal)
							{
								//Right의 Fold를 변경한다.
								_uiFoldType_Right2 = UI_FOLD_TYPE.Unfolded;
							}
						}
						else
						{
							//변경 19.8.17 : 탭 축소 버튼이 위쪽에
							UI_FOLD_BTN_RESULT foldResult_Right2 = apEditorUtil.DrawTabFoldTitle_H(this, 0, 0, mainRight2Width, 9, _uiFoldType_Right2, false);
							if (foldResult_Right2 == UI_FOLD_BTN_RESULT.ToggleFold_Horizontal)
							{
								//Right의 Fold를 변경한다.
								_uiFoldType_Right2 = UI_FOLD_TYPE.Folded;
							}

							_scroll_Right2 = EditorGUILayout.BeginScrollView(_scroll_Right2, false, true);

							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainRight2Width - 20), apGUILOFactory.I.Height(mainHeight - 20));

							GUI_Right2(mainRight2Width - 24, mainHeight - 20);

							EditorGUILayout.EndVertical();

							GUILayout.Space(500);

							EditorGUILayout.EndScrollView();
						}
						GUILayout.EndArea();


						EditorGUILayout.EndVertical();
					}

					//Profiler.EndSample();//Profiler GUI Right
				}

				EditorGUILayout.EndHorizontal();

				//Profiler.BeginSample("5. GUI Bottom");
				//-------------------------------------------------
				if (isBottomVisible_EditButtons)
				{
					GUILayout.Space(marginHeight);

					EditorGUILayout.BeginVertical(apGUILOFactory.I.Height(bottomEditButtonsHeight));

					Rect rectBottom2 = new Rect(0, topHeight + marginHeight * 2 + mainHeight, bottomWidth, bottomEditButtonsHeight);
					if (_isFullScreenGUI)
					{
						//전체 화면인 경우 Top을 제외
						rectBottom2 = new Rect(0, marginHeight * 1 + mainHeight, bottomWidth, bottomEditButtonsHeight);
					}

					//Color prevGUIColor = GUI.backgroundColor;
					//if(Select.IsExEditing)
					//{
					//	GUI.backgroundColor = new Color(prevGUIColor.r * 1.5f, prevGUIColor.g * 0.5f, prevGUIColor.b * 0.5f, 1.0f);
					//}

					GUI.Box(rectBottom2, apStringFactory.I.None);

					//GUI.backgroundColor = prevGUIColor;
					GUILayout.BeginArea(rectBottom2, apStringFactory.I.None);

					GUILayout.Space(4);
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(bottomWidth), apGUILOFactory.I.Height(bottomEditButtonsHeight - 10));

					GUI_Bottom_EditButtons(bottomWidth, bottomEditButtonsHeight);

					EditorGUILayout.EndHorizontal();

					GUILayout.EndArea();

					EditorGUILayout.EndVertical();
				}

				if (isBottomVisible_Timeline)
				{

					//bool isBottomScroll = true;
					//switch (Select.SelectionType)
					//{
					//	case apSelection.SELECTION_TYPE.MeshGroup:
					//		isBottomScroll = false;
					//		break;

					//	case apSelection.SELECTION_TYPE.Animation:
					//		isBottomScroll = false;//<<스크롤은 따로 합니다.
					//		break;

					//	default:
					//		break;
					//}

					GUILayout.Space(marginHeight);


					// Bottom Layout (Timeline / Status)
					//-------------------------------------------------
					EditorGUILayout.BeginVertical(apGUILOFactory.I.Height(bottomTimelineHeight));

					int bottomPosY = topHeight + marginHeight * 2 + mainHeight;
					if (_isFullScreenGUI)
					{
						//전체화면인 경우 Top을 제외
						bottomPosY = marginHeight * 1 + mainHeight;
					}
					if (isBottomVisible_EditButtons)
					{
						bottomPosY += marginHeight + bottomEditButtonsHeight;
					}

					Rect rectBottom = new Rect(0, bottomPosY, bottomWidth, bottomTimelineHeight);



					GUI.Box(rectBottom, apStringFactory.I.None);
					GUILayout.BeginArea(rectBottom, apStringFactory.I.None);



					//if (isBottomScroll)
					//{
					//	_scroll_Bottom = EditorGUILayout.BeginScrollView(_scroll_Bottom, false, true);

					//	EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(bottomWidth - 20), apGUILOFactory.I.Height(bottomTimelineHeight - 20));

					//	GUI_Bottom_Timeline(bottomWidth - 20, bottomTimelineHeight - 20, (int)rectBottom.x, (int)rectBottom.y, (int)position.width, (int)position.height);

					//	EditorGUILayout.EndVertical();
					//	EditorGUILayout.EndScrollView();
					//}
					//else
					//{


					//	EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(bottomWidth), apGUILOFactory.I.Height(bottomTimelineHeight));

					//	GUI_Bottom_Timeline(bottomWidth, bottomTimelineHeight, (int)rectBottom.x, (int)rectBottom.y, (int)position.width, (int)position.height);

					//	EditorGUILayout.EndVertical();
					//}

					//변경 22.12.16 [v1.4.2] 불필요한 비교문 삭제
					EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(bottomWidth), apGUILOFactory.I.Height(bottomTimelineHeight));

					GUI_Bottom_Timeline(bottomWidth, bottomTimelineHeight, (int)rectBottom.x, (int)rectBottom.y, (int)position.width, (int)position.height);

					EditorGUILayout.EndVertical();



					GUILayout.EndArea();

					EditorGUILayout.EndVertical();
				}

				//-------------------------------------------------
				//Profiler.EndSample();

				//Event e = Event.current;

				if (EditorWindow.focusedWindow == this)
				{	
					if (Event.current.type == EventType.KeyDown ||
						Event.current.type == EventType.KeyUp)
					{
						if (GUIUtility.keyboardControl == 0)
						{
							//Down 이벤트 중심으로 변경
							//Up은 키 해제
							//Debug.Log("KeyDown : " + Event.current.keyCode + " + Ctrl : " + Event.current.control);
							//추가 3.24 : Ctrl (Command), Shift, Alt키가 먼저 눌렸다면
#if UNITY_EDITOR_OSX
							bool isCtrl = Event.current.command;
#else
							bool isCtrl = Event.current.control;
#endif

							bool isShift = Event.current.shift;
							bool isAlt = Event.current.alt;

							//bool isHotKeyAction = false;

							//변경 20.1.27 : 단축키 로직이 변경되었다.
							apHotKey.EVENT_RESULT eventResult = HotKey.OnKeyEvent(Event.current.keyCode, isCtrl, isShift, isAlt, Event.current.type == EventType.KeyDown);
							if (eventResult == apHotKey.EVENT_RESULT.NormalEvent)
							{
								//일반 단축키 이벤트일때
								apHotKey.HotKeyEvent hotkeyEvent = HotKey.GetResultEvent();
								apHotKey.HotKeyResult callbackResult = HotKey.GetResultAfterCallback();


								if (hotkeyEvent != null
									//&& hotkeyEvent._labelType != apHotKey.LabelText.None//이전
									&& hotkeyEvent._labelText != null//변경 20.12.3
									&& callbackResult != null//추가 21.2.8 : 콜백이 성공해야한다.
									)
								{
									//추가 19.12.2 : StringWrapper를 이용해서 최적화
									if (_hotKeyStringWrapper == null)
									{
										_hotKeyStringWrapper = new apStringWrapper(128);
									}

									//string strHotKeyEvent = "[ " + hotkeyEvent._label + " ] - ";
									_hotKeyStringWrapper.Clear();
									_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_1, false);

									//이전
									//_hotKeyStringWrapper.Append(HotKey.GetText(hotkeyEvent).ToString(), false);

									//변경
									_hotKeyStringWrapper.Append(hotkeyEvent._labelText.ToString(), false);

									//만약, 콜백 결과에서 커스텀 레이블이 있다면, 그걸 반영하자
									if(!string.IsNullOrEmpty(callbackResult._customLabel))
									{
										_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_CUSTOMLABEL_1, false);
										_hotKeyStringWrapper.Append(callbackResult._customLabel, false);
										_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_CUSTOMLABEL_2, false);
									}

									_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_2, false);


									if (hotkeyEvent._isCtrl)
									{
#if UNITY_EDITOR_OSX
										//strHotKeyEvent += "Command+";//<<Mac용
										_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_Command, false);
#else
										//strHotKeyEvent += "Ctrl+";
										_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_Ctrl, false);
#endif
									}
									if (hotkeyEvent._isAlt)
									{
										//strHotKeyEvent += "Alt+";
										_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_Alt, false);
									}
									if (hotkeyEvent._isShift)
									{
										//strHotKeyEvent += "Shift+";
										_hotKeyStringWrapper.Append(HOTKEY_NOTI_TEXT_Shift, false);
									}

									//strHotKeyEvent += hotkeyEvent._keyCode;
									_hotKeyStringWrapper.Append(hotkeyEvent._keyCode.ToString(), true);

									//Notification(strHotKeyEvent, true, false);
									Notification(_hotKeyStringWrapper.ToString(), true, false);

									//isHotKeyAction = true;
								}
							}

							//if (isHotKeyAction)//기존
							if (eventResult != apHotKey.EVENT_RESULT.None)//변경
							{
								//키 이벤트를 사용했다고 알려주자
								Event.current.Use();
								//Repaint();
							}
						}
					}
					else
					{
						//추가 21.2.9 : 만약 키 입력 이벤트가 아닌데, Shift 키가 변경되었다면, 그것만 따로 체크하자
						if (GUIUtility.keyboardControl == 0)
						{
							HotKey.OnShiftKeyEvent(Event.current.shift);
						}
					}

					//추가 : apGL의 DelayedCursor이벤트를 처리한다.
					apGL.ProcessDelayedCursor();
				}

			}
			catch (Exception ex)
			{
				if (ex is UnityEngine.ExitGUIException)
				{
					//걍 리턴
					//return;
					//무시하자
				}
				else
				{
					Debug.LogError("Exception : " + ex);
				}

			}



			//UnityEngine.Profiling.Profiler.EndSample();

			//_isCountDeltaTime = false;
			//타이머 종료
			UpdateFrameCount(FRAME_TIMER_TYPE.None);

			//GUIPool에서 사용했던 리소스들 모두 정리
			//apGUIPool.Reset();

			//일부 Dialog는 OnGUI가 지나고 호출해야한다.
			if (_dialogShowCall != DIALOG_SHOW_CALL.None && _curEventType == EventType.Layout)
			{
				try
				{
					//Debug.Log("Show Dialog [" + _curEventType + "]");
					switch (_dialogShowCall)
					{
						case DIALOG_SHOW_CALL.Bake:
							apDialog_Bake.ShowDialog(this, _portrait);
							break;

						case DIALOG_SHOW_CALL.Setting:
							apDialog_PortraitSetting.ShowDialog(this, _portrait);
							break;

						case DIALOG_SHOW_CALL.Capture:
							{
								if (apVersion.I.IsDemo)
								{
									EditorUtility.DisplayDialog(
										GetText(TEXT.DemoLimitation_Title),
										GetText(TEXT.DemoLimitation_Body),
										GetText(TEXT.Okay));
								}
								else
								{
									apDialog_CaptureScreen.ShowDialog(this, _portrait);
								}
							}

							break;

					}

				}
				catch (Exception ex)
				{
					Debug.LogError("Dialog Call Exception : " + ex);
				}
				_dialogShowCall = DIALOG_SHOW_CALL.None;
			}

			//Portrait 생성 요청이 들어왔을 때 => Repaint 이벤트 외의 루틴에서 함수를 연산해준다.


			//v1.4.2 : Hierachy Unit 클릭이 발생했을 경우
			//마지막 선택이 Hierarchy임을 알린다.
			//그 외에는 모두 선택을 해제한다.
			if(_isReadyToCheckClickedHierarchy)
			{
				if(!_isClickedHierarchyProcessed)
				{
					//입력이 되지 않았다면 해제
					_lastClickedHierarchy = LAST_CLICKED_HIERARCHY.None;
					//Debug.Log("클릭이 되지 않아서 Hierarchy 선택 해제");
				}

				_isReadyToCheckClickedHierarchy = false;
			}

			//v1.4.2 : Undo 처리를 마무리한다.
			apEditorUtil.EndRecordUndo();
		}


		// Sub 그리기 함수들
		//-------------------------------------------------------------------------------
		

		//-------------------------------------------------------------------------------
		// GUI - Top
		//-------------------------------------------------------------------------------
		
		// Top UI : Tab Buttons / Basic Toolbar
		private void GUI_Top(int width, int height)
		{
			Texture2D imbTabOpen = ImageSet.Get(apImageSet.PRESET.ToolBtn_TabOpen);
			Texture2D imbTabFolded = ImageSet.Get(apImageSet.PRESET.ToolBtn_TabFolded);


			//EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Height(height - 10));//이전
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Height(height));//변경 21.1.20 (v1.3.0)

			GUILayout.Space(5);

			
			int paddingY_Height24 = ((height - 2) - 24) / 2;
			int paddingY_Height20 = ((height - 2) - 20) / 2;
			int height_Btn = height - 8;//기존엔 height - 14

			if (_portrait == null)
			{
				GUILayout.Space(15);
				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(250));
				
				//삭제 21.1.20
				//EditorGUILayout.LabelField(GetUIWord(UIWORD.SelectPortraitFromScene), apGUILOFactory.I.Width(200));//"Select Portrait From Scene"

				GUILayout.Space(paddingY_Height20);//추가 21.1.20
				apPortrait nextPortrait = EditorGUILayout.ObjectField(_portrait, typeof(apPortrait), true, apGUILOFactory.I.Width(200)) as apPortrait;

				//바뀌었다.
				if (_portrait != nextPortrait && nextPortrait != null)
				{
					if (nextPortrait._isOptimizedPortrait)
					{
						//Optimized Portrait는 편집이 불가능하다
						EditorUtility.DisplayDialog(GetText(TEXT.OptPortrait_LoadError_Title),
														GetText(TEXT.OptPortrait_LoadError_Body),
														GetText(TEXT.Okay));

					}
					else
					{
						//v1.4.2 : 프리팹 에셋 등의 추가 체크 필요
						bool isValid = true;

						apEditorUtil.CHECK_EDITABLE_RESULT checkResult = apEditorUtil.CheckEditablePortrait(nextPortrait);
						switch (checkResult)
						{
							case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_NoGameObject:
								//객체가 없는 경우
								isValid = false;
								break;

							case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_PrefabAsset:
								{
									//프리팹 에셋인 경우 > 실행 불가
									EditorUtility.DisplayDialog(	GetText(TEXT.DLG_NotOpenEditorPrefabAsset_Title),
																	GetText(TEXT.DLG_NotOpenEditorPrefabAsset_Body),
																	GetText(TEXT.Okay));

									isValid = false;
								}
								break;

							case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_PrefabEditScene:
								{
									//프리팹 편집 화면이라면
									EditorUtility.DisplayDialog(	GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Title),
																	GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Body),
																	GetText(TEXT.Okay));

									isValid = false;
								}
								break;
						}


						if (isValid)
						{
							//비동기 로딩
							Selection.activeGameObject = null;
							LoadPortraitAsync(nextPortrait);//선택이 안된 상태에서 좌상단 오브젝트 필드로 선택 (비동기)
						}
					}

					apEditorUtil.ReleaseGUIFocus();
				}
				EditorGUILayout.EndVertical();

				GUILayout.Space(20);

				EditorGUILayout.EndHorizontal();

				return;
			}
			else//if (_portrait != null)
			{
				//>>>>>>>>>>>> Tab1_Bake And Setting >>>>>>>>>>>>>>>>>>>
				bool isGUITab_BakeAndSetting = DrawAndCheckGUITopTab(GUITOP_TAB.Tab1_BakeAndSetting, imbTabOpen, imbTabFolded, height - 10);

				if (isGUITab_BakeAndSetting)
				{
					EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(100));//이전은 140 > 100 (21.1.20)
					//이전
					//EditorGUILayout.LabelField(GetUIWord(UIWORD.Portrait), apGUILOFactory.I.Width(140));//"Portrait"

					//변경 21.1.20 / 여백 추가, 길이 감소 140 > 100
					GUILayout.Space(paddingY_Height20);//추가 21.1.20
					apPortrait nextPortrait = EditorGUILayout.ObjectField(_portrait, typeof(apPortrait), true, apGUILOFactory.I.Width(100)) as apPortrait;

					if (_portrait != nextPortrait)
					{
						//바뀌었다.
						if (nextPortrait != null)
						{
							if (nextPortrait._isOptimizedPortrait)
							{
								//Optimized Portrait는 편집이 불가능하다
								EditorUtility.DisplayDialog(GetText(TEXT.OptPortrait_LoadError_Title),
																GetText(TEXT.OptPortrait_LoadError_Body),
																GetText(TEXT.Okay));
							}
							else
							{
								//NextPortrait를 선택
								

								//v1.4.2 : 프리팹 에셋 등의 추가 체크 필요
								bool isValid = true;

								apEditorUtil.CHECK_EDITABLE_RESULT checkResult = apEditorUtil.CheckEditablePortrait(nextPortrait);
								switch (checkResult)
								{
									case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_NoGameObject:
										//객체가 없는 경우
										isValid = false;
										break;

									case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_PrefabAsset:
										{
											//프리팹 에셋인 경우 > 실행 불가
											EditorUtility.DisplayDialog(	GetText(TEXT.DLG_NotOpenEditorPrefabAsset_Title),
																			GetText(TEXT.DLG_NotOpenEditorPrefabAsset_Body),
																			GetText(TEXT.Okay));

											isValid = false;
										}
										break;

									case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_PrefabEditScene:
										{
											//프리팹 편집 화면이라면
											EditorUtility.DisplayDialog(	GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Title),
																			GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Body),
																			GetText(TEXT.Okay));

											isValid = false;
										}
										break;
								}

								if (isValid)
								{
									//비동기 로딩
									Selection.activeGameObject = null;
									LoadPortraitAsync(nextPortrait);//이미 편집중인 Portrait가 있는 상태에서 새롭게	비동기 로딩
								}
							}
						}
						else
						{
							_selection.SelectNone();
							_portrait = null;

							SyncHierarchyOrders();

							_hierarchy.ResetAllUnits();
							_hierarchy_MeshGroup.ResetSubUnits();
							_hierarchy_AnimClip.ResetSubUnits();
						}

						apEditorUtil.ReleaseGUIFocus();
					}
					EditorGUILayout.EndVertical();

					if (_guiContent_TopBtn_Setting == null)
					{
						_guiContent_TopBtn_Setting = apGUIContentWrapper.Make(ImageSet.Get(apImageSet.PRESET.ToolBtn_Setting), apStringFactory.I.SettingsOfTherPortraitAndEditor);//"Settings of the Portrait and Editor"
					}
					if (_guiContent_TopBtn_Bake == null)
					{
						_guiStringWrapper_32.Clear();
						_guiStringWrapper_32.AppendSpace(2, false);
						_guiStringWrapper_32.Append(GetUIWord(UIWORD.Bake), true);
						_guiContent_TopBtn_Bake = apGUIContentWrapper.Make(_guiStringWrapper_32.ToString(), false, ImageSet.Get(apImageSet.PRESET.ToolBtn_Bake), apStringFactory.I.BakeToScene);//"Bake to Scene"
					}

					//변경 21.1.20 : 크기가 변경되었다. height-14 > height_Btn(height - 4)
					if (GUILayout.Button(_guiContent_TopBtn_Setting.Content, apGUIStyleWrapper.I.Button_VerticalMargin0, apGUILOFactory.I.Width(height_Btn), apGUILOFactory.I.Height(height_Btn)))
					{
						_dialogShowCall = DIALOG_SHOW_CALL.Setting;//<<Delay된 호출을 한다.
					}

					GUILayout.Space(2);

					//"  Bake"
					if (GUILayout.Button(_guiContent_TopBtn_Bake.Content, apGUIStyleWrapper.I.Button_VerticalMargin0, apGUILOFactory.I.Width(90), apGUILOFactory.I.Height(height_Btn)))
					{
						_dialogShowCall = DIALOG_SHOW_CALL.Bake;
						//apDialog_Bake.ShowDialog(this, _portrait);//<<이건 Delay 해야한다.
					}
				}
			}

			//int tabBtnHeight = height - (15);//이전
			int tabBtnHeight = height_Btn;//변경 21.1.20
			int tabBtnWidth = tabBtnHeight + 10;

			bool isGizmoUpdatable = Gizmos.IsUpdatable;

			//>>>>>>>>>>>> Tab2_TRS Tools >>>>>>>>>>>>>>>>>>>
			bool isGUITab_TRSTools = DrawAndCheckGUITopTab(GUITOP_TAB.Tab2_TRSTools, imbTabOpen, imbTabFolded, height - 10);

			if (isGUITab_TRSTools)
			{
				if (apEditorUtil.ToggledButton_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_Select), 
																(_gizmos.ControlType == apGizmos.CONTROL_TYPE.Select), 
																isGizmoUpdatable, tabBtnWidth, tabBtnHeight, 
																apStringFactory.I.GetHotkeyTooltip_SelectTool(HotKeyMap)//"Select Tool (Q)"//TODO
																))
				{
					_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Select);
				}
				if (apEditorUtil.ToggledButton_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_Move), 
																(_gizmos.ControlType == apGizmos.CONTROL_TYPE.Move), 
																isGizmoUpdatable, tabBtnWidth, tabBtnHeight, 
																apStringFactory.I.GetHotkeyTooltip_MoveTool(HotKeyMap)//"Move Tool (W)"//TODO
																))
				{
					_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Move);
				}
				if (apEditorUtil.ToggledButton_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_Rotate), 
																(_gizmos.ControlType == apGizmos.CONTROL_TYPE.Rotate), 
																isGizmoUpdatable, tabBtnWidth, tabBtnHeight, 
																apStringFactory.I.GetHotkeyTooltip_RotateTool(HotKeyMap)//"Rotate Tool (E)"//TODO
																))
				{
					_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Rotate);
				}
				if (apEditorUtil.ToggledButton_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_Scale), 
																(_gizmos.ControlType == apGizmos.CONTROL_TYPE.Scale), 
																isGizmoUpdatable, tabBtnWidth, tabBtnHeight, 
																apStringFactory.I.GetHotkeyTooltip_ScaleTool(HotKeyMap)//"Scale Tool (R)"//TODO
																))
				{
					_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Scale);
				}



				//GUILayout.Space(5);

				//변경 21.1.20 : 텍스트 삭제후, Width를 95 > 70으로
				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(70));
				//이전
				//EditorGUILayout.LabelField(GetUIWord(UIWORD.Coordinate), apGUILOFactory.I.Width(95));//"Coordinate"
				//변경 21.1.20
				GUILayout.Space(paddingY_Height20);
				apGizmos.COORDINATE_TYPE nextCoordinate = (apGizmos.COORDINATE_TYPE)EditorGUILayout.EnumPopup(Gizmos.Coordinate, apGUILOFactory.I.Width(70));
				if (nextCoordinate != Gizmos.Coordinate)
				{
					Gizmos.SetCoordinateType(nextCoordinate);
				}
				EditorGUILayout.EndVertical();

			}
			//GUILayout.Space(5);

			//>>>>>>>>>>>> Tab3_Visibility >>>>>>>>>>>>>>>>>>>
			//변경 21.1.20 : 이 버튼들은 신버전에서는 옵션으로 열기 전에는 보이지 않는다.

			if (_option_ShowPrevViewMenuBtns)
			{
				bool isGUITab_Visibility = DrawAndCheckGUITopTab(GUITOP_TAB.Tab3_Visibility, imbTabOpen, imbTabFolded, height - 10);

				if (isGUITab_Visibility)
				{
					// Onion 버튼.
					bool isOnionButtonAvailable = false;
					bool isOnionButtonRecordable = false;
					if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation ||
						Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
					{
						isOnionButtonAvailable = true;
						if (!_onionOption_IsRenderAnimFrames || Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
						{
							isOnionButtonRecordable = true;
						}
					}
					if (apEditorUtil.ToggledButton_2Side_Ctrl_VerticalMargin0(	
																ImageSet.Get(apImageSet.PRESET.ToolBtn_OnionView),
																Onion.IsVisible, isOnionButtonAvailable, tabBtnWidth, tabBtnHeight,
																apStringFactory.I.GetHotkeyTooltip_ToggleOnionSkin(HotKeyMap),//"Show/Hide Onion Skin (O)",
																Event.current.control, Event.current.command))
					{
#if UNITY_EDITOR_OSX
						if(Event.current.command)
#else
						if (Event.current.control)
#endif
						{
							apDialog_OnionSetting.ShowDialog(this, _portrait);//<<Onion 설정 다이얼로그를 호출
						}
						else
						{
							Onion.SetVisible(!Onion.IsVisible);
						}

					}




					//만약 Onion이 켜졌다면 => Record 버튼이 활성화된다.
					bool isOnionRecordable = isOnionButtonAvailable && Onion.IsVisible && isOnionButtonRecordable;

					//SetGUIVisible("GUI Top Onion Visible", isOnionRecordable);
					SetGUIVisible(DELAYED_UI_TYPE.GUI_Top_Onion_Visible, isOnionRecordable);

					//if (IsDelayedGUIVisible("GUI Top Onion Visible"))
					if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Top_Onion_Visible))
					{
						if (apEditorUtil.ToggledButton_2Side_VerticalMargin0(
														ImageSet.Get(apImageSet.PRESET.ToolBtn_OnionRecord),
														false, true, 
														tabBtnWidth, tabBtnHeight, 
														apStringFactory.I.RecordOnionSkin//"Record Onion Skin"//TODO
														))
						{
							//현재 상태를 기록한다.
							Onion.Record(this);
						}

						GUILayout.Space(5);
					}



					// Bone Visible 여부 버튼
					Texture2D iconImg_boneGUI = null;
					if (_boneGUIRenderMode == BONE_RENDER_MODE.RenderOutline)
					{
						iconImg_boneGUI = ImageSet.Get(apImageSet.PRESET.ToolBtn_BoneVisibleOutlineOnly);
					}
					else
					{
						iconImg_boneGUI = ImageSet.Get(apImageSet.PRESET.ToolBtn_BoneVisible);
					}


					bool isBoneVisibleButtonAvailable = _selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
						_selection.SelectionType == apSelection.SELECTION_TYPE.Animation ||
						_selection.SelectionType == apSelection.SELECTION_TYPE.Overall;


					//이전
					////텍스트를 StringWrapper를 이용해서 만들자
					//_guiStringWrapper_256.Clear();
					//_guiStringWrapper_256.Append(apStringFactory.I.BoneGUIToolTip_1, false);
					//_guiStringWrapper_256.Append(apStringFactory.I.GetCtrlOrCommand(), false);
					//_guiStringWrapper_256.Append(apStringFactory.I.BoneGUIToolTip_2, true);

					//변경 21.1.21 : 툴팁 가져오는 방식이 간편하게 되었다.

					if (apEditorUtil.ToggledButton_2Side_Ctrl_VerticalMargin0(
													iconImg_boneGUI,
													_boneGUIRenderMode != BONE_RENDER_MODE.None,
													isBoneVisibleButtonAvailable, tabBtnWidth, tabBtnHeight,
													//"Change Bone Visiblity (B) / If you press the button while holding down [" + strCtrlKey + "], the function works in reverse.",//이전
													//_guiStringWrapper_256.ToString(),
													apStringFactory.I.GetHotkeyTooltip_BoneVisibility(HotKeyMap),
													Event.current.control,
													Event.current.command))
					{
#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
						bool isCtrl = Event.current.control;
#endif

						//Control 키를 누르면 그냥 누른 것과 반대로 변경된다.
						//Ctrl : Outline -> Render -> None -> Outline
						//그냥 : None -> Render -> Outline -> None


						switch (_boneGUIRenderMode)
						{
							case BONE_RENDER_MODE.None:
								{
									if (isCtrl)
									{ _boneGUIRenderMode = BONE_RENDER_MODE.RenderOutline; }
									else
									{ _boneGUIRenderMode = BONE_RENDER_MODE.Render; }
								}
								break;

							case BONE_RENDER_MODE.Render:
								{
									if (isCtrl)
									{ _boneGUIRenderMode = BONE_RENDER_MODE.None; }
									else
									{ _boneGUIRenderMode = BONE_RENDER_MODE.RenderOutline; }
								}
								break;

							case BONE_RENDER_MODE.RenderOutline:
								{
									if (isCtrl)
									{ _boneGUIRenderMode = BONE_RENDER_MODE.Render; }
									else
									{ _boneGUIRenderMode = BONE_RENDER_MODE.None; }
								}
								break;

						}
						SaveEditorPref();
					}


					//메시 렌더링
					if (apEditorUtil.ToggledButton_2Side_VerticalMargin0(ImageSet.Get(apImageSet.PRESET.ToolBtn_MeshVisible),
						_meshGUIRenderMode == MESH_RENDER_MODE.Render,
						_selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
						_selection.SelectionType == apSelection.SELECTION_TYPE.Animation ||
						_selection.SelectionType == apSelection.SELECTION_TYPE.Overall, tabBtnWidth, tabBtnHeight,
						//"Enable/Disable Mesh Visiblity"
						apStringFactory.I.GetHotkeyTooltip_MeshVisibility(HotKeyMap)
						))
					{
						if (_meshGUIRenderMode == MESH_RENDER_MODE.None)
						{
							_meshGUIRenderMode = MESH_RENDER_MODE.Render;
						}
						else
						{
							_meshGUIRenderMode = MESH_RENDER_MODE.None;
						}
					}


					//물리 적용 여부
					bool isPhysic = false;

					if (_portrait != null)
					{
						isPhysic = _portrait._isPhysicsPlay_Editor;
					}

					if (apEditorUtil.ToggledButton_2Side_VerticalMargin0(ImageSet.Get(apImageSet.PRESET.ToolBtn_Physic),
						isPhysic,
						_selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup ||
						_selection.SelectionType == apSelection.SELECTION_TYPE.Animation ||
						_selection.SelectionType == apSelection.SELECTION_TYPE.Overall, tabBtnWidth, tabBtnHeight,
						//"Enable/Disable Physical Effect"//이전
						apStringFactory.I.GetHotkeyTooltip_PhysicsFxEnable(HotKeyMap)
						))
					{
						if (_portrait != null)
						{
							//물리 기능 토글
							_portrait._isPhysicsPlay_Editor = !isPhysic;
							if (_portrait._isPhysicsPlay_Editor)
							{
								//물리 값을 리셋합시다.
								Controller.ResetPhysicsValues();
							}
						}
					}

				}

			}
			

			//GUILayout.Space(15);


			//Gizmo에 의해 어떤 UI가 나와야 하는지 판단하자.
			apGizmos.TRANSFORM_UI_VALID gizmoUI_VetexTF = Gizmos.TransformUI_VertexTransform;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Position2D = Gizmos.TransformUI_Position;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Rotation = Gizmos.TransformUI_Rotation;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Scale = Gizmos.TransformUI_Scale;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Depth = Gizmos.TransformUI_Depth;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Color = Gizmos.TransformUI_Color;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_Extra = Gizmos.TransformUI_Extra;
			apGizmos.TRANSFORM_UI_VALID gizmoUI_BoneIKController = Gizmos.TransformUI_BoneIKController;


			bool isGizmoGUIVisible_VertexTransform = false;

			bool isGizmoGUIVisible_VTF_FFD = false;
			bool isGizmoGUIVisible_VTF_Soft = false;
			bool isGizmoGUIVisible_VTF_Blur = false;

			bool isGizmoGUIVisible_Position2D = false;
			bool isGizmoGUIVisible_Rotation = false;
			bool isGizmoGUIVisible_Scale = false;
			bool isGizmoGUIVisible_Depth = false;
			bool isGizmoGUIVisible_Color = false;
			bool isGizmoGUIVisible_Extra = false;
			bool isGizmoGUIVisible_BoneIKController = false;

			//Vertex Transform
			//1) FFD
			//2) Soft Selection
			//3) Blur가 있다.

			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Vertex_Transform, (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - Vertex Transform"

			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Position, (gizmoUI_Position2D != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - Position"
			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Rotation, (gizmoUI_Rotation != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - Rotation"
			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Scale, (gizmoUI_Scale != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - Scale"
			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Depth, (gizmoUI_Depth != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - Depth"
			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Color, (gizmoUI_Color != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - Color"
			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Extra, (gizmoUI_Extra != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - Extra"

			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__BoneIKController, (gizmoUI_BoneIKController != apGizmos.TRANSFORM_UI_VALID.Hide));//"Top UI - BoneIKController"

			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__VTF_FFD, (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide) && Gizmos.IsFFDMode);//"Top UI - VTF FFD"
			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__VTF_Soft, (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide) && Gizmos.IsSoftSelectionMode);//"Top UI - VTF Soft"
			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__VTF_Blur, (gizmoUI_VetexTF != apGizmos.TRANSFORM_UI_VALID.Hide) && Gizmos.IsBrushMode);//"Top UI - VTF Blur"

			SetGUIVisible(DELAYED_UI_TYPE.Top_UI__Overall, _selection.SelectionType == apSelection.SELECTION_TYPE.Overall);//"Top UI - Overall"


			isGizmoGUIVisible_VertexTransform = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Vertex_Transform);//"Top UI - Vertex Transform"

			isGizmoGUIVisible_VTF_FFD = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__VTF_FFD);//"Top UI - VTF FFD"
			if (Event.current.type == EventType.Layout)
			{
				_isGizmoGUIVisible_VTF_FFD_Prev = isGizmoGUIVisible_VTF_FFD;
			}
			isGizmoGUIVisible_VTF_Soft = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__VTF_Soft);//"Top UI - VTF Soft"
			isGizmoGUIVisible_VTF_Blur = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__VTF_Blur);//"Top UI - VTF Blur"

			isGizmoGUIVisible_Position2D = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Position);//"Top UI - Position"
			isGizmoGUIVisible_Rotation = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Rotation);//"Top UI - Rotation"
			isGizmoGUIVisible_Scale = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Scale);//"Top UI - Scale"
			isGizmoGUIVisible_Depth = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Depth);//"Top UI - Depth"
			isGizmoGUIVisible_Color = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Color);//"Top UI - Color"
			isGizmoGUIVisible_Extra = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Extra);//"Top UI - Extra"
			isGizmoGUIVisible_BoneIKController = IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__BoneIKController);//"Top UI - BoneIKController"




			if (isGizmoGUIVisible_VertexTransform)
			{

				//>>>>>>>>>>>> Tab4_FFD Soft Blur >>>>>>>>>>>>>>>>>>>
				bool isGUITab_FFD_Soft_Blur = DrawAndCheckGUITopTab(GUITOP_TAB.Tab4_FFD_Soft_Blur, imbTabOpen, imbTabFolded, height - 10);

				//1. FFD, Soft, Blur 선택 버튼
				if (isGUITab_FFD_Soft_Blur)
				{
					//apEditorUtil.GUI_DelimeterBoxV(height - 15);
					//GUILayout.Space(15);

					_guiStringWrapper_256.Clear();
					_guiStringWrapper_256.Append(apStringFactory.I.FFDModeToolTip_1, false);
					_guiStringWrapper_256.Append(apStringFactory.I.GetCtrlOrCommand(), false);
					_guiStringWrapper_256.Append(apStringFactory.I.FFDModeToolTip_2, true);

					if (apEditorUtil.ToggledButton_Ctrl_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_Transform),
																			Gizmos.IsFFDMode, Gizmos.IsFFDModeAvailable, tabBtnWidth, tabBtnHeight,
																			_guiStringWrapper_256.ToString(),//변경
																			Event.current.control,
																			Event.current.command))
					{

#if UNITY_EDITOR_OSX
						bool isCtrl = Event.current.command;
#else
						bool isCtrl = Event.current.control;
#endif
						if (isCtrl)
						{
							//커스텀 사이즈로 연다.
							_loadKey_FFDStart = apDialog_FFDSize.ShowDialog(this, _portrait, OnDialogEvent_FFDStart, _curFFDSizeX, _curFFDSizeY);
						}
						else
						{


							Gizmos.StartTransformMode(this);//원래는 <이거 기본 3X3
						}

					}

					//2-1) FFD
					if (isGizmoGUIVisible_VTF_FFD)
					{
						if (apEditorUtil.ToggledButton_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformAdapt),
																		false, Gizmos.IsFFDMode, 
																		tabBtnWidth, tabBtnHeight, 
																		apStringFactory.I.ApplyFFD))//"Apply FFD"
						{
							if (Gizmos.IsFFDMode)
							{
								Gizmos.AdaptFFD(this);
							}
						}

						if (apEditorUtil.ToggledButton_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformRevert),
																		false, Gizmos.IsFFDMode, 
																		tabBtnWidth, tabBtnHeight, 
																		apStringFactory.I.RevertFFD))//"Revert FFD"
						{
							//_gizmos.SetControlType(apGizmos.CONTROL_TYPE.Transform);
							//Gizmos.StartTransformMode();
							if (Gizmos.IsFFDMode)
							{
								Gizmos.RevertFFD(this);
							}
						}

						GUILayout.Space(10);

						_isGizmoGUIVisible_VTF_FFD_Prev = true;
					}
					else if (_isGizmoGUIVisible_VTF_FFD_Prev)
					{
						//만약 Layout이 아닌 이전 이벤트에서 FFD 편집 중이었는데
						//갑자기 사라지는 경우 -> 더미를 출력하자
						apEditorUtil.ToggledButton_VerticalMargin0(ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformAdapt), false, Gizmos.IsFFDMode, tabBtnWidth, tabBtnHeight, apStringFactory.I.ApplyFFD);//"Apply FFD"
						apEditorUtil.ToggledButton_VerticalMargin0(ImageSet.Get(apImageSet.PRESET.ToolBtn_TransformRevert), false, Gizmos.IsFFDMode, tabBtnWidth, tabBtnHeight, apStringFactory.I.RevertFFD);//"Revert FFD"

						GUILayout.Space(10);
					}

					if (Event.current.type == EventType.Layout)
					{
						_isGizmoGUIVisible_VTF_FFD_Prev = isGizmoGUIVisible_VTF_FFD;
					}

					GUILayout.Space(4);

					if (apEditorUtil.ToggledButton_2Side_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_SoftSelection),
																			Gizmos.IsSoftSelectionMode, Gizmos.IsSoftSelectionModeAvailable, tabBtnWidth, tabBtnHeight,
																			apStringFactory.I.GetHotkeyTooltip_SoftSelectionToolTip(HotKeyMap)//"Soft Selection (Adjust brush size with [, ])"
																		))
					{
						if (Gizmos.IsSoftSelectionMode)
						{
							Gizmos.EndSoftSelection();
						}
						else
						{
							Gizmos.StartSoftSelection();
						}
					}


					
					//이전
					//int labelSize = 70;
					//int sliderSize = 200;
					//int sliderSetSize = labelSize + sliderSize + 4;

					//변경 : 슬라이더가 좌우로 배치되면서 길이 짧아짐 (Radius와 Curve 길이 다르게 처리)
					
					int labelSize_Radius = 50;
					int labelSize_Curve_Soft = 40;
					int labelSize_Curve_Blur = 60;
					switch (_language)
					{
						case LANGUAGE.Korean:
						case LANGUAGE.Japanese:
						case LANGUAGE.Chinese_Simplified:
						case LANGUAGE.Chinese_Traditional:
							//Radius / Curve 글자 길이를 줄일 수 있다.
							labelSize_Radius = 30;
							labelSize_Curve_Soft = 30;
							labelSize_Curve_Blur = 30;
							break;

						case LANGUAGE.Spanish:
						case LANGUAGE.Polish:
							//이건 글자를 더 길게
							labelSize_Curve_Blur = 70;
							break;
					}
					
					int sliderValueWidth = 35;

					int sliderSize_Radius = 120 - sliderValueWidth;
					int sliderSetSize_Radius = labelSize_Radius + sliderSize_Radius + sliderValueWidth + 6;

					
					int sliderSize_Curve_Soft = 80 - sliderValueWidth;
					int sliderSetSize_Curve_Soft = labelSize_Curve_Soft + sliderSize_Curve_Soft + sliderValueWidth + 6;

					int sliderSize_Curve_Blur = 80 - sliderValueWidth;
					int sliderSetSize_Curve_Blur = labelSize_Curve_Blur + sliderSize_Curve_Blur + sliderValueWidth + 6;

					//2-2) Soft Selection
					if (isGizmoGUIVisible_VTF_Soft)
					{
						//Radius와 Curve
						//레이아웃 변경 21.1.20 : Radius와 Curve가 세로로 배치되던걸 좌우로 배치
						//EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(sliderSetSize));
						
						//int softRadius = Gizmos.SoftSelectionRadius;//이전
						int softRadiusIndex = Gizmos.SoftSelectionRadiusIndex;//변경 22.1.8 : 브러시 크기를 인덱스로 설정

						int softCurveRatio = Gizmos.SoftSelectionCurveRatio;

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(sliderSetSize_Radius));
						GUILayout.Space(paddingY_Height24);
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(sliderSetSize_Radius), apGUILOFactory.I.Height(20));
						EditorGUILayout.LabelField(GetUIWord(UIWORD.Radius), apGUILOFactory.I.Width(labelSize_Radius));//"Radius"
						
						//이전 : 선형 Radius 조절
						//softRadius = (int)GUILayout.HorizontalSlider(softRadius, 1, apGizmos.MAX_SOFT_SELECTION_RADIUS, apGUILOFactory.I.Width(sliderSize_Radius));
						//softRadius = EditorGUILayout.IntField(softRadius, apGUILOFactory.I.Width(sliderValueWidth));
						//softRadius = Mathf.Clamp(softRadius, 1, apGizmos.MAX_SOFT_SELECTION_RADIUS);

						//변경 22.1.8 : 인덱스 방식의 크기 조절 + 텍스트 박스 사용 불가
						softRadiusIndex = (int)(GUILayout.HorizontalSlider(softRadiusIndex, 0, apGizmos.MAX_BRUSH_INDEX, apGUILOFactory.I.Width(sliderSize_Radius)) + 0.5f);
						EditorGUILayout.IntField(apGizmos.GetBrushSizeByIndex(softRadiusIndex), apGUILOFactory.I.Width(sliderValueWidth));
						
						
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.EndVertical();


						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(sliderSetSize_Curve_Soft));
						GUILayout.Space(paddingY_Height24);

						//string strCurveLabel = "Curve";
						string strCurveLabel = GetUIWord(UIWORD.Curve);

						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(sliderSetSize_Curve_Soft));
						EditorGUILayout.LabelField(strCurveLabel, apGUILOFactory.I.Width(labelSize_Curve_Soft));
						softCurveRatio = (int)GUILayout.HorizontalSlider(softCurveRatio, -100, 100, apGUILOFactory.I.Width(sliderSize_Curve_Soft));
						softCurveRatio = EditorGUILayout.IntField(softCurveRatio, apGUILOFactory.I.Width(sliderValueWidth));
						softCurveRatio = Mathf.Clamp(softCurveRatio, -100, 100);
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.EndVertical();

						//EditorGUILayout.EndVertical();

						GUILayout.Space(10);

						//이전
						//if (softRadius != Gizmos.SoftSelectionRadius || softCurveRatio != Gizmos.SoftSelectionCurveRatio)
						//{
						//	//TODO : 브러시 미리보기 기동
						//	Gizmos.RefreshSoftSelectionValue(softRadius, softCurveRatio);
						//}

						//변경 22.1.9
						if (softRadiusIndex != Gizmos.SoftSelectionRadiusIndex || softCurveRatio != Gizmos.SoftSelectionCurveRatio)
						{
							//이전
							//Gizmos.RefreshSoftSelectionValue(softRadius, softCurveRatio);

							//변경
							Gizmos.RefreshSoftSelectionRadiusIndexAndCurveRatio(softRadiusIndex, softCurveRatio);
						}
					}


					GUILayout.Space(4);

					if (apEditorUtil.ToggledButton_2Side_VerticalMargin0(	ImageSet.Get(apImageSet.PRESET.ToolBtn_Blur),
																			//Gizmos.IsBrushMode, 
																			_blurEnabled,
																			Gizmos.IsBrushModeAvailable, tabBtnWidth, tabBtnHeight,
																			apStringFactory.I.GetHotkeyTooltip_BlurToolTip(HotKeyMap)//"Blur (Adjust brush size with [, ]"
																			))
					{
						if (_blurEnabled)
						//if (Gizmos.IsBrushMode)
						{
							_blurEnabled = false;
							Gizmos.EndBrush();
						}
						else
						{
							_blurEnabled = true;
							Gizmos.StartBrush();
						}
					}

					//2-3) Blur
					if (isGizmoGUIVisible_VTF_Blur)
					{
						//Range와 Intensity
						//Radius와 Curve
						//레이아웃 변경 21.1.20 : Radius와 Curve가 세로로 배치되던걸 좌우로 배치
						//EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(sliderSetSize));

						//int blurRadius = Gizmos.BrushRadius;
						//int blurIntensity = Gizmos.BrushIntensity;

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(sliderSetSize_Radius));
						GUILayout.Space(paddingY_Height24);

						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(sliderSetSize_Radius));
						EditorGUILayout.LabelField(GetUIWord(UIWORD.Radius), apGUILOFactory.I.Width(labelSize_Radius));//"Radius"
						
						//이전 : 선형으로 크기 조절
						//_blurRadius = (int)GUILayout.HorizontalSlider(_blurRadius, 1, apGizmos.MAX_BRUSH_RADIUS, apGUILOFactory.I.Width(sliderSize_Radius));
						//_blurRadius = EditorGUILayout.IntField(_blurRadius, apGUILOFactory.I.Width(sliderValueWidth));
						//_blurRadius = Mathf.Clamp(_blurRadius, 1, apGizmos.MAX_BRUSH_RADIUS);

						//변경 22.1.9 : 프리셋 인덱스를 이용하여 크기 조절
						_blurRadiusIndex = (int)(GUILayout.HorizontalSlider(_blurRadiusIndex, 0, apGizmos.MAX_BRUSH_INDEX, apGUILOFactory.I.Width(sliderSize_Radius)) + 0.5f);
						EditorGUILayout.IntField(apGizmos.GetBrushSizeByIndex(_blurRadiusIndex), apGUILOFactory.I.Width(sliderValueWidth));
						_blurRadiusIndex = Mathf.Clamp(_blurRadiusIndex, 0, apGizmos.MAX_BRUSH_INDEX);


						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndVertical();


						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(sliderSetSize_Radius));
						GUILayout.Space(paddingY_Height24);

						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(sliderSetSize_Curve_Blur));
						EditorGUILayout.LabelField(GetUIWord(UIWORD.Intensity), apGUILOFactory.I.Width(labelSize_Curve_Blur));//"Intensity"
						_blurIntensity = (int)GUILayout.HorizontalSlider(_blurIntensity, 0, 100, apGUILOFactory.I.Width(sliderSize_Curve_Blur));
						_blurIntensity = EditorGUILayout.IntField(_blurIntensity, apGUILOFactory.I.Width(sliderValueWidth));
						_blurIntensity = Mathf.Clamp(_blurIntensity, 0, 100);
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.EndVertical();

						//EditorGUILayout.EndVertical();

					}

					//GUILayout.Space(15);
				}

			}



			//bool isGizmoGUIVisible_Position = IsDelayedGUIVisible("Top UI - Position");
			//bool isGizmoGUIVisible_Rotation = IsDelayedGUIVisible("Top UI - Rotation");
			//bool isGizmoGUIVisible_Scale = IsDelayedGUIVisible("Top UI - Scale");
			//bool isGizmoGUIVisible_Color = IsDelayedGUIVisible("Top UI - Color");

			//if (Gizmos._isGizmoRenderable)
			if (isGizmoGUIVisible_Position2D
				|| isGizmoGUIVisible_Rotation
				|| isGizmoGUIVisible_Scale
				|| isGizmoGUIVisible_Depth
				|| isGizmoGUIVisible_Color
				|| isGizmoGUIVisible_Extra
				|| isGizmoGUIVisible_BoneIKController)
			{
				//하나라도 Gizmo Transform 이 나타난다면 이 영역을 그려준다.
				//apEditorUtil.GUI_DelimeterBoxV(height - 15);
				//GUILayout.Space(15);

				//>>>>>>>>>>>> Tab5_GizmoValue >>>>>>>>>>>>>>>>>>>
				bool isGUITab_GizmoValue = DrawAndCheckGUITopTab(GUITOP_TAB.Tab5_GizmoValue, imbTabOpen, imbTabFolded, height - 10);

				if (isGUITab_GizmoValue)
				{
					//Transform

					//Position
					apGizmos.TransformParam curTransformParam = Gizmos.GetCurTransformParam();
					Vector2 prevPos = Vector2.zero;
					int prevDepth = 0;
					float prevRotation = 0.0f;
					Vector2 prevScale2 = Vector2.one;
					Color prevColor = Color.black;
					bool prevVisible = true;
					//float prevBoneIKMixWeight = 0.0f;

					if (curTransformParam != null)
					{
						//prevPos = curTransformParam._posW;//<<GUI용으로 변경
						prevPos = curTransformParam._pos_GUI;
						prevDepth = curTransformParam._depth;
						//prevRotation = curTransformParam._angle;
						prevRotation = curTransformParam._angle_GUI;//<<GUI용으로 변경

						//prevScale2 = curTransformParam._scale;
						prevScale2 = curTransformParam._scale_GUI;//GUI 용으로 변경
						prevColor = curTransformParam._color;
						prevVisible = curTransformParam._isVisible;

						//prevBoneIKMixWeight = curTransformParam._boneIKMixWeight;
					}

					Vector2 curPos = prevPos;
					int curDepth = prevDepth;
					float curRotation = prevRotation;
					Vector2 curScale = prevScale2;
					Color curColor = prevColor;
					bool curVisible = prevVisible;
					//float curBoneIKMixWeight = prevBoneIKMixWeight;

					if (_guiContent_Top_GizmoIcon_Move == null) { _guiContent_Top_GizmoIcon_Move = apGUIContentWrapper.Make(ImageSet.Get(apImageSet.PRESET.Transform_Move)); }
					if (_guiContent_Top_GizmoIcon_Depth == null) { _guiContent_Top_GizmoIcon_Depth = apGUIContentWrapper.Make(ImageSet.Get(apImageSet.PRESET.Transform_Depth)); }
					if (_guiContent_Top_GizmoIcon_Rotation == null) { _guiContent_Top_GizmoIcon_Rotation = apGUIContentWrapper.Make(ImageSet.Get(apImageSet.PRESET.Transform_Rotate)); }
					if (_guiContent_Top_GizmoIcon_Scale == null) { _guiContent_Top_GizmoIcon_Scale = apGUIContentWrapper.Make(ImageSet.Get(apImageSet.PRESET.Transform_Scale)); }
					if (_guiContent_Top_GizmoIcon_Color == null) { _guiContent_Top_GizmoIcon_Color = apGUIContentWrapper.Make(ImageSet.Get(apImageSet.PRESET.Transform_Color)); }
					if (_guiContent_Top_GizmoIcon_Extra == null) { _guiContent_Top_GizmoIcon_Extra = apGUIContentWrapper.Make(ImageSet.Get(apImageSet.PRESET.Transform_ExtraOption)); }


					if (isGizmoGUIVisible_Position2D)
					{
						//아이콘 크기 변경 (21.1.20) : 30 > height_Btn
						EditorGUILayout.LabelField(_guiContent_Top_GizmoIcon_Move.Content, apGUILOFactory.I.Width(height_Btn), apGUILOFactory.I.Height(height_Btn));

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(130));

						//텍스트 삭제 21.1.20
						//"Position"
						//EditorGUILayout.LabelField(	GetUIWord(UIWORD.Position),
						//							(gizmoUI_Position2D != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled ? GUIStyleWrapper.Label_GrayColor : GUIStyleWrapper.Label),
						//							apGUILOFactory.I.Width(130));

						//추가 21.1.20
						GUILayout.Space(paddingY_Height24);

						if (gizmoUI_Position2D == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
						{
							curPos = apEditorUtil.DelayedVector2Field(prevPos, 130);
						}
						else
						{
							curPos = apEditorUtil.DelayedVector2Field(Vector2.zero, 130);
						}
						EditorGUILayout.EndVertical();

						GUILayout.Space(10);


					}

					if (isGizmoGUIVisible_Depth)
					{
						//Depth와 Position은 같이 묶인다. > Depth는 따로 분리한다. (11.26)
						EditorGUILayout.LabelField(	_guiContent_Top_GizmoIcon_Depth.Content, 
													apGUILOFactory.I.Width(height_Btn), apGUILOFactory.I.Height(height_Btn));

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(70));

						//"Depth"
						//텍스트 삭제 21.1.20
						//EditorGUILayout.LabelField(GetUIWord(UIWORD.Depth),
						//	(gizmoUI_Depth != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled ? GUIStyleWrapper.Label_GrayColor : GUIStyleWrapper.Label),
						//	apGUILOFactory.I.Width(70));//"Depth"

						//변경 21.1.20
						GUILayout.Space(paddingY_Height20);

						if (gizmoUI_Depth == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
						{
							curDepth = EditorGUILayout.DelayedIntField(apStringFactory.I.None, prevDepth, apGUILOFactory.I.Width(70));
						}
						else
						{
							EditorGUILayout.DelayedIntField(apStringFactory.I.None, 0, apGUILOFactory.I.Width(70));
						}
						EditorGUILayout.EndVertical();

						GUILayout.Space(10);
					}

					if (isGizmoGUIVisible_Rotation)
					{
						EditorGUILayout.LabelField(	_guiContent_Top_GizmoIcon_Rotation.Content, 
													apGUILOFactory.I.Width(height_Btn), apGUILOFactory.I.Height(height_Btn));

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(70));

						//텍스트 삭제 21.1.20
						//"Rotation"
						//EditorGUILayout.LabelField(GetUIWord(UIWORD.Rotation),
						//	(gizmoUI_Rotation != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled ? GUIStyleWrapper.Label_GrayColor : GUIStyleWrapper.Label),
						//	apGUILOFactory.I.Width(70));

						//변경 21.1.20
						GUILayout.Space(paddingY_Height20);

						if (gizmoUI_Rotation == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
						{
							curRotation = EditorGUILayout.DelayedFloatField(prevRotation, apGUILOFactory.I.Width(70));
						}
						else
						{
							EditorGUILayout.DelayedFloatField(0.0f, apGUILOFactory.I.Width(70));
						}
						EditorGUILayout.EndVertical();

						GUILayout.Space(10);
					}

					if (isGizmoGUIVisible_Scale)
					{
						EditorGUILayout.LabelField(	_guiContent_Top_GizmoIcon_Scale.Content, 
													apGUILOFactory.I.Width(height_Btn), apGUILOFactory.I.Height(height_Btn));

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(120));

						//텍스트 삭제 21.1.20
						//"Scaling"
						//EditorGUILayout.LabelField(GetUIWord(UIWORD.Scaling),
						//	(gizmoUI_Scale != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled ? GUIStyleWrapper.Label_GrayColor : GUIStyleWrapper.Label),
						//	apGUILOFactory.I.Width(120));


						//변경 21.1.20
						GUILayout.Space(paddingY_Height24);

						if (gizmoUI_Scale == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
						{
							curScale = apEditorUtil.DelayedVector2Field(prevScale2, 120);
							//EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(120), apGUILOFactory.I.Height(20));
							//curScale.x = EditorGUILayout.DelayedFloatField(prevScale2.x, apGUILOFactory.I.Width(60 - 2));
							//curScale.y = EditorGUILayout.DelayedFloatField(prevScale2.y, apGUILOFactory.I.Width(60 - 2));
							//EditorGUILayout.EndHorizontal();
						}
						else
						{
							apEditorUtil.DelayedVector2Field(Vector2.zero, 120);
							//EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(120));
							//EditorGUILayout.DelayedFloatField(0, apGUILOFactory.I.Width(60 - 2));
							//EditorGUILayout.DelayedFloatField(0, apGUILOFactory.I.Width(60 - 2));
							//EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndVertical();

						GUILayout.Space(10);
					}


					if (isGizmoGUIVisible_Color)
					{
						EditorGUILayout.LabelField(	_guiContent_Top_GizmoIcon_Color.Content, 
													apGUILOFactory.I.Width(height_Btn), apGUILOFactory.I.Height(height_Btn));

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(70));
						
						//변경 21.1.20
						//"Color"
						//EditorGUILayout.LabelField(GetUIWord(UIWORD.Color),
						//	(gizmoUI_Color != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled ? GUIStyleWrapper.Label_GrayColor : GUIStyleWrapper.Label),
						//	apGUILOFactory.I.Width(70));

						//변경 21.1.20
						GUILayout.Space(paddingY_Height20);


						if (gizmoUI_Color == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
						{
							curColor = EditorGUILayout.ColorField(apStringFactory.I.None, prevColor, apGUILOFactory.I.Width(70));
						}
						else
						{
							EditorGUILayout.ColorField(apStringFactory.I.None, Color.black, apGUILOFactory.I.Width(70));
						}
						EditorGUILayout.EndVertical();

						//변경 21.1.20 : Visible을 ToggleBox에서 버튼으로 변경 (길이도 45에서 60으로 길어짐)
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(60));

						//텍스트 삭제 21.1.20
						//"Visible"
						//EditorGUILayout.LabelField(GetUIWord(UIWORD.Visible),
						//	(gizmoUI_Color != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled ? GUIStyleWrapper.Label_GrayColor : GUIStyleWrapper.Label),
						//	apGUILOFactory.I.Width(45));

						//변경 21.1.20
						GUILayout.Space(paddingY_Height20);

						if (gizmoUI_Color == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled)
						{
							//이전 : Toggle 박스
							//curVisible = EditorGUILayout.Toggle(prevVisible, apGUILOFactory.I.Width(45));
						}
						else
						{
							//이전 : Toggle 박스
							//EditorGUILayout.Toggle(false, apGUILOFactory.I.Width(45));
						}
						//변경 : Visible이라는 텍스트의 버튼
						if (apEditorUtil.ToggledButton_2Side(	GetUIWord(UIWORD.Visible), GetUIWord(UIWORD.Visible), 
																prevVisible, 
																gizmoUI_Color == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled, 
																60, 18))
						{
							curVisible = !prevVisible;
						}

						EditorGUILayout.EndVertical();

						GUILayout.Space(10);
					}


					if (isGizmoGUIVisible_Extra)
					{
						EditorGUILayout.LabelField(	_guiContent_Top_GizmoIcon_Extra.Content, 
													apGUILOFactory.I.Width(height_Btn), apGUILOFactory.I.Height(height_Btn));

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(70));
						
						//텍스트 삭제 21.1.20
						//"Extra"
						//EditorGUILayout.LabelField(GetUIWord(UIWORD.Extra),
						//	(gizmoUI_Extra != apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled ? GUIStyleWrapper.Label_GrayColor : GUIStyleWrapper.Label),
						//	apGUILOFactory.I.Width(70));

						//추가 21.1.20
						GUILayout.Space(paddingY_Height20);

						if (apEditorUtil.ToggledButton_2Side(GetUIWord(UIWORD.Set), GetUIWord(UIWORD.Set), false, (gizmoUI_Extra == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled), 65, 18))
						{
							if (gizmoUI_Extra == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && curTransformParam != null)
							{
								Gizmos.OnTransformChanged_Extra();
							}
						}
						EditorGUILayout.EndVertical();
					}




					if (curTransformParam != null)
					{
						if (gizmoUI_Position2D == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && prevPos != curPos)
						{
							Gizmos.OnTransformChanged_Position(curPos);
						}
						if (gizmoUI_Rotation == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevRotation != curRotation))
						{
							Gizmos.OnTransformChanged_Rotate(curRotation);
						}
						if (gizmoUI_Scale == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevScale2 != curScale))
						{
							Gizmos.OnTransformChanged_Scale(curScale);
						}
						if (gizmoUI_Depth == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && curDepth != prevDepth)
						{
							Gizmos.OnTransformChanged_Depth(curDepth);
						}
						if (gizmoUI_Color == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevColor != curColor || prevVisible != curVisible))
						{
							Gizmos.OnTransformChanged_Color(curColor, curVisible);
							//추가 20.7.5 : Visible이 바뀌면 Hierarchy를 갱신해야한다. (눈 아이콘때문에)
							if(prevVisible != curVisible)
							{	
								RefreshControllerAndHierarchy(false);
							}
						}

						//if(gizmoUI_BoneIKController == apGizmos.TRANSFORM_UI_VALID.ShowAndEnabled && (prevBoneIKMixWeight != curBoneIKMixWeight))
						//{
						//	//추가 : BoneIKWeight 변경됨
						//	Gizmos.OnTransformChanged_BoneIKController(curBoneIKMixWeight);
						//}
					}
				}
				//GUILayout.Space(15);
			}


			//변경 4.9
			//Show Frame
			//>> Capture 버튼 및 Dialog 호출은 삭제한다. Capture기능은 우측 탭으로 이동한다.
			if (IsDelayedGUIVisible(DELAYED_UI_TYPE.Top_UI__Overall))//"Top UI - Overall"
			{
				//>>>>>>>>>>>> Tab6_Capture >>>>>>>>>>>>>>>>>>>
				bool isGUITab_Capture = DrawAndCheckGUITopTab(GUITOP_TAB.Tab6_Capture, imbTabOpen, imbTabFolded, height - 10);

				//apEditorUtil.GUI_DelimeterBoxV(height - 15);
				//GUILayout.Space(15);
				if (isGUITab_Capture)
				{
					//버튼 이름을 Wrapper를 이용하여 다시 설정
					_guiStringWrapper_32.Clear();
					_guiStringWrapper_32.AppendSpace(2, false);
					_guiStringWrapper_32.Append(GetUIWord(UIWORD.ShowFrame), true);

					//"Show Frame", "Show Frame"
					if (apEditorUtil.ToggledButton_2Side_VerticalMargin0(ImageSet.Get(apImageSet.PRESET.Capture_Frame),
															_guiStringWrapper_32.ToString(),//"  " + GetUIWord(UIWORD.ShowFrame),
															_guiStringWrapper_32.ToString(),//"  " + GetUIWord(UIWORD.ShowFrame),
															_isShowCaptureFrame, true, 130, tabBtnHeight))
					{
						_isShowCaptureFrame = !_isShowCaptureFrame;
					}

					//"Capture" >> 이게 삭제된다.
					//if (GUILayout.Button(GetUIWord(UIWORD.Capture), GUILayout.Width(80), GUILayout.Height(tabBtnHeight)))
					//{

					//	_dialogShowCall = DIALOG_SHOW_CALL.Capture;
					//}



					//추가 19.5.31 : Material Library 설정

					//버튼 이름을 Wrapper를 이용하여 다시 설정
					_guiStringWrapper_32.Clear();
					_guiStringWrapper_32.AppendSpace(2, false);
					_guiStringWrapper_32.Append(GetUIWord(UIWORD.MaterialLibrary), true);

					//"  Material Library"
					if (apEditorUtil.ToggledButton_2Side_VerticalMargin0(ImageSet.Get(apImageSet.PRESET.ToolBtn_MaterialLibrary),
															_guiStringWrapper_32.ToString(),//"  " + GetUIWord(UIWORD.MaterialLibrary),
															_guiStringWrapper_32.ToString(),//"  " + GetUIWord(UIWORD.MaterialLibrary),
															false, true, 150, tabBtnHeight))
					{
						try
						{
							apDialog_MaterialLibrary.ShowDialog(this, _portrait);
						}
						catch (Exception ex)
						{
							Debug.LogError("Exception : " + ex);
						}

					}
				}
				//GUILayout.Space(20);
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

		}

		

		//GUI Top에서 UI들을 묶어서 보였다가 안보이게 할 수 있다.
		private bool DrawAndCheckGUITopTab(GUITOP_TAB tabType, Texture2D imgOpen, Texture2D imgFolded, int height)
		{
			GUILayout.Space(8);
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(10), apGUILOFactory.I.Height(height));
			GUILayout.Space((height - (32)) / 2);

			bool isTabOpen = _guiTopTabStaus[tabType];

			//이전
			//GUIStyle guiStyle = new GUIStyle(GUIStyle.none);
			//guiStyle.margin = new RectOffset(0, 0, 0, 0);
			//guiStyle.padding = new RectOffset(0, 0, 0, 0);

			if (_guiContent_GUITopTab_Open == null)
			{
				_guiContent_GUITopTab_Open = apGUIContentWrapper.Make(imgOpen);
			}
			if (_guiContent_GUITopTab_Folded == null)
			{
				_guiContent_GUITopTab_Folded = apGUIContentWrapper.Make(imgFolded);
			}

			//이전
			//if (GUILayout.Button(new GUIContent("", (isTabOpen ? imgOpen : imgFolded)), guiStyle, GUILayout.Width(10), GUILayout.Height(32)))

			//변경
			if (GUILayout.Button((isTabOpen ? _guiContent_GUITopTab_Open.Content : _guiContent_GUITopTab_Folded.Content), GUIStyleWrapper.None_Margin0_Padding0, apGUILOFactory.I.Width(10), apGUILOFactory.I.Height(32)))
			{
				_guiTopTabStaus[tabType] = !_guiTopTabStaus[tabType];
			}
			EditorGUILayout.EndVertical();
			GUILayout.Space(2);

			return _guiTopTabStaus[tabType];
		}



		


		//------------------------------------------------------------------------
		// GUI - Left Upper
		//------------------------------------------------------------------------
		private object _loadKey_MakeNewPortrait = null;


		private int GUI_LeftUpper(int width)
		{
			if (_guiContent_MainLeftUpper_MakeNewPortrait == null)
			{
				_guiStringWrapper_32.Clear();
				_guiStringWrapper_32.AppendSpace(3, false);
				_guiStringWrapper_32.Append(GetUIWord(UIWORD.MakeNewPortrait), true);

				_guiContent_MainLeftUpper_MakeNewPortrait = apGUIContentWrapper.Make(_guiStringWrapper_32.ToString(), ImageSet.Get(apImageSet.PRESET.Hierarchy_MakeNewPortrait), apStringFactory.I.CreateANewPortrait);//"Create a new Portrait"
			}
			if (_guiContent_MainLeftUpper_RefreshToLoad == null)
			{
				_guiStringWrapper_32.Clear();
				_guiStringWrapper_32.AppendSpace(3, false);
				_guiStringWrapper_32.Append(GetUIWord(UIWORD.RefreshToLoad), true);

				_guiContent_MainLeftUpper_RefreshToLoad = apGUIContentWrapper.Make(_guiStringWrapper_32.ToString(), ImageSet.Get(apImageSet.PRESET.Controller_Default), apStringFactory.I.SearchPortraitsAgainInTheScene);//"Search Portraits again in the scene"
			}
			if (_guiContent_MainLeftUpper_LoadBackupFile == null)
			{
				_guiContent_MainLeftUpper_LoadBackupFile = apGUIContentWrapper.Make(GetUIWord(UIWORD.LoadBackupFile), false, apStringFactory.I.LoadBackupFileToolTip);//"Open a Portrait saved as a backup file. It will be created as a new Portrait"
			}


			if (_portrait == null)
			{
				bool isRefresh = false;
				if (GUILayout.Button(_guiContent_MainLeftUpper_MakeNewPortrait.Content, apGUILOFactory.I.Height(45)))
				{
					//Portrait를 생성하는 Dialog를 띄우자
					_loadKey_MakeNewPortrait = apDialog_NewPortrait.ShowDialog(this, OnMakeNewPortraitResult);
				}

				GUILayout.Space(5);
				if (GUILayout.Button(_guiContent_MainLeftUpper_RefreshToLoad.Content, apGUILOFactory.I.Height(25)))
				{
					isRefresh = true;
				}
				if (GUILayout.Button(_guiContent_MainLeftUpper_LoadBackupFile.Content, apGUILOFactory.I.Height(20)))
				{
					if (apVersion.I.IsDemo)
					{
						//추가 : 데모 버전일 때에는 백업 파일을 로드할 수 없다.
						EditorUtility.DisplayDialog(
										GetText(TEXT.DemoLimitation_Title),
										GetText(TEXT.DemoLimitation_Body),
										GetText(TEXT.Okay));
					}
					else
					{
						//추가 21.3.1 : apEditorUtil.GetLastOpenFileDirectoryPath() 함수 이용해서 경로 초기화 막음
						string strPath = EditorUtility.OpenFilePanel("Load Backup File", apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BackupFile), "bck");//추가 21.3.6 : 실행 환경에 따른 문제 개선 포맷인 bck+corrected 가 추가되었다. 
						if (!string.IsNullOrEmpty(strPath))
						{
							//추가 21.7.3 : 이스케이프 문자 삭제
							strPath = apUtil.ConvertEscapeToPlainText(strPath);

							//Debug.Log("Load Backup File [" + strPath + "]");

							_isMakePortraitRequestFromBackupFile = true;
							_requestedLoadedBackupPortraitFilePath = strPath;

							apEditorUtil.SetLastExternalOpenSaveFilePath(strPath, apEditorUtil.SAVED_LAST_FILE_PATH.BackupFile);//추가 21.3.1
						}
					}
				}

				if (_portraitsInScene.Count == 0 && !_isPortraitListLoaded)
				{
					isRefresh = true;
				}

				if (isRefresh)
				{
					//씬에 있는 Portrait를 찾는다.
					//추가) 썸네일도 표시
					_portraitsInScene.Clear();
					apPortrait[] portraits = UnityEngine.Object.FindObjectsOfType<apPortrait>();
					if (portraits != null)
					{
						int nPortraits = portraits.Length;
						apPortrait curPortrait = null;
						for (int i = 0; i < nPortraits; i++)
						{
							curPortrait = portraits[i];

							//Opt Portrait는 생략한다.
							if (curPortrait._isOptimizedPortrait)
							{
								continue;
							}

							//프리팹 에셋 등의 상태라면 리스트에서 제거 (v1.4.2)
							apEditorUtil.CHECK_EDITABLE_RESULT checkResult = apEditorUtil.CheckEditablePortrait(curPortrait);
							if(checkResult != apEditorUtil.CHECK_EDITABLE_RESULT.Valid)
							{
								continue;
							}


							//썸네일을 연결하자
							string thumnailPath = curPortrait._imageFilePath_Thumbnail;
							if (string.IsNullOrEmpty(thumnailPath))
							{
								curPortrait._thumbnailImage = null;
							}
							else
							{
								Texture2D thumnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumnailPath);

								if (thumnailImage != null)
								{
									//추가 : 크기가 이상하다. 보정한다.
									int thumbWidth = thumnailImage.width;
									int thumbHeight = thumnailImage.height;
									float thumbAspectRatio = (float)thumbWidth / (float)thumbHeight;
									if (thumbAspectRatio < 1.8f)//원래는 2.0 근처여야 한다.
									{

										TextureImporter ti = TextureImporter.GetAtPath(thumnailPath) as TextureImporter;
										ti.textureCompression = TextureImporterCompression.Uncompressed;
										ti.SaveAndReimport();

										thumnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumnailPath);
										//Debug.Log("iOS에서 크기가 이상하여 보정함 : " + thumbWidth + "x" + thumbHeight + " >> " + thumnailImage.width + "x" + thumnailImage.height);
									}


								}
								curPortrait._thumbnailImage = thumnailImage;
							}

							_portraitsInScene.Add(curPortrait);
						}
					}

					_isPortraitListLoaded = true;
				}
				return 85;
			}

			if (_tabLeft == TAB_LEFT.Hierarchy)
			{
				int filterIconSize = (width / 8) - 2;
				int filterIconWidth = filterIconSize + 3;

				//Hierarchy의 필터 선택 버튼들
				//변경 3.28 : All, None 버튼이 사라지고, Sort Mode가 추가되었다.
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(filterIconSize + 5));
				GUILayout.Space(7);

				if (!_isHierarchyOrderEditEnabled)
				{
					//>> All Filter 삭제
					//if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_All), IsHierarchyFilterContain(HIERARCHY_FILTER.All), true, filterIconSize, filterIconSize, "Show All"))
					//{ SetHierarchyFilter(HIERARCHY_FILTER.All, true); }//All

					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), IsHierarchyFilterContain(HIERARCHY_FILTER.RootUnit), true, filterIconWidth, filterIconSize, apStringFactory.I.RootUnits))//"Root Units"
					{ SetHierarchyFilter(HIERARCHY_FILTER.RootUnit, !IsHierarchyFilterContain(HIERARCHY_FILTER.RootUnit)); } //Root Toggle
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), IsHierarchyFilterContain(HIERARCHY_FILTER.Image), true, filterIconWidth, filterIconSize, apStringFactory.I.Images))//"Images"
					{ SetHierarchyFilter(HIERARCHY_FILTER.Image, !IsHierarchyFilterContain(HIERARCHY_FILTER.Image)); }//Image Toggle
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh), IsHierarchyFilterContain(HIERARCHY_FILTER.Mesh), true, filterIconWidth, filterIconSize, apStringFactory.I.Meshes))//"Meshes"
					{ SetHierarchyFilter(HIERARCHY_FILTER.Mesh, !IsHierarchyFilterContain(HIERARCHY_FILTER.Mesh)); }//Mesh Toggle
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), IsHierarchyFilterContain(HIERARCHY_FILTER.MeshGroup), true, filterIconWidth, filterIconSize, apStringFactory.I.MeshGroups))//"Mesh Groups"
					{ SetHierarchyFilter(HIERARCHY_FILTER.MeshGroup, !IsHierarchyFilterContain(HIERARCHY_FILTER.MeshGroup)); }//MeshGroup Toggle
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation), IsHierarchyFilterContain(HIERARCHY_FILTER.Animation), true, filterIconWidth, filterIconSize, apStringFactory.I.AnimationClips))//"Animation Clips"
					{ SetHierarchyFilter(HIERARCHY_FILTER.Animation, !IsHierarchyFilterContain(HIERARCHY_FILTER.Animation)); }//Animation Toggle
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_Param), IsHierarchyFilterContain(HIERARCHY_FILTER.Param), true, filterIconWidth, filterIconSize, apStringFactory.I.ControlParameters))//"Control Parameters"
					{ SetHierarchyFilter(HIERARCHY_FILTER.Param, !IsHierarchyFilterContain(HIERARCHY_FILTER.Param)); }//Param Toggle

					//>> None Filter 삭제
					//if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_None), IsHierarchyFilterContain(HIERARCHY_FILTER.None), true, filterIconSize, filterIconSize, "Hide All"))
					//{ SetHierarchyFilter(HIERARCHY_FILTER.None, true); }//None
				}
				else
				{
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_SortMode_RegOrder), _hierarchySortMode == HIERARCHY_SORT_MODE.RegOrder, true, filterIconWidth + 8, filterIconSize, apStringFactory.I.HierarchySortModeToolTip_RegOrder))//"Show in order of registration"
					{
						_hierarchySortMode = HIERARCHY_SORT_MODE.RegOrder;
						SaveEditorPref();
						RefreshControllerAndHierarchy(false);
					}
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_SortMode_AlphaNum), _hierarchySortMode == HIERARCHY_SORT_MODE.AlphaNum, true, filterIconWidth + 8, filterIconSize, apStringFactory.I.HierarchySortModeToolTip_AlphaNum))//"Show in order of name's alphanumeric"
					{
						_hierarchySortMode = HIERARCHY_SORT_MODE.AlphaNum;
						SaveEditorPref();
						RefreshControllerAndHierarchy(false);
					}
					if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_SortMode_Custom), _hierarchySortMode == HIERARCHY_SORT_MODE.Custom, true, filterIconWidth + 8, filterIconSize, apStringFactory.I.HierarchySortModeToolTip_Custom))//"Show in order of custom"
					{
						_hierarchySortMode = HIERARCHY_SORT_MODE.Custom;
						SaveEditorPref();
						RefreshControllerAndHierarchy(false);
					}
					GUILayout.Space(75);
				}
				//추가 3.28 : Hierarchy Sort Mode
				GUILayout.Space(7);
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_SortMode), _isHierarchyOrderEditEnabled, true, filterIconWidth + 2, filterIconSize, apStringFactory.I.HierarchySortModeToolTip_Toggle))//"Toggle Sort Mode"
				{
					_isHierarchyOrderEditEnabled = !_isHierarchyOrderEditEnabled;
				}

				EditorGUILayout.EndHorizontal();

				return filterIconSize + 5;
			}
			else
			{
				return Controller.GUI_Controller_Upper(width);
			}
		}


		/// <summary>팝업 이벤트 > 새로운 Portrait 만들기</summary>
		private void OnMakeNewPortraitResult(bool isSuccess, object loadKey, string name)
		{
			if (!isSuccess || loadKey != _loadKey_MakeNewPortrait)
			{
				_loadKey_MakeNewPortrait = null;
				return;
			}
			_loadKey_MakeNewPortrait = null;

			//Portrait를 만들어준다.
			_isMakePortraitRequest = true;
			_requestedNewPortraitName = name;
			if (string.IsNullOrEmpty(_requestedNewPortraitName))
			{
				_requestedNewPortraitName = "<No Named Portrait>";
			}


		}



		//-----------------------------------------------------------------------
		// GUI - Left : Hierarchy가 나온다.
		//-----------------------------------------------------------------------
		private void GUI_Left(int width, int height, Vector2 scroll, bool isGUIEvent)
		{
			GUILayout.Space(20);

			if (_portrait == null)
			{
				int portraitSelectWidth = width - 10;
				int thumbnailHeight = 60;
				int thumbnailWidth = thumbnailHeight * 2;

				//삭제 > 래퍼 이용
				//GUIStyle guiStyle_Thumb = new GUIStyle(GUI.skin.box);
				//guiStyle_Thumb.margin = GUI.skin.label.margin;
				////guiStyle_Thumb.padding = GUI.skin.label.padding;
				//guiStyle_Thumb.padding = new RectOffset(0, 0, 0, 0);


				int selectBtnWidth = portraitSelectWidth - (thumbnailWidth);
				int portraitSelectHeight = thumbnailHeight + 24;

				int nPortraitsInScene = _portraitsInScene != null ? _portraitsInScene.Count : 0;

				apPortrait curPortraitInScene = null;
				for (int i = 0; i < nPortraitsInScene; i++)
				{
					curPortraitInScene = _portraitsInScene[i];
					if (curPortraitInScene == null)
					{
						_portraitsInScene.Clear();
						break;
					}
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width - 10), apGUILOFactory.I.Height(portraitSelectHeight));
					GUILayout.Space(5);
					EditorGUILayout.BeginVertical();



					EditorGUILayout.LabelField(curPortraitInScene.transform.name, apGUILOFactory.I.Width(portraitSelectWidth));

					EditorGUILayout.BeginHorizontal();

					GUILayout.Box(curPortraitInScene._thumbnailImage,
									GUIStyleWrapper.Box_LabelMargin_Padding0,
									apGUILOFactory.I.Width(thumbnailWidth), apGUILOFactory.I.Height(thumbnailHeight));

					if (GUILayout.Button(GetUIWord(UIWORD.Select), apGUILOFactory.I.Width(selectBtnWidth), apGUILOFactory.I.Height(thumbnailHeight)))
					{
						//Portrait를 선택했다.
						bool isValid = true;

						if(curPortraitInScene._isOptimizedPortrait)
						{
							//Optimized는 제외한다.
							isValid = false;
						}

						//v1.4.2 : 유효한지 체크한다.
						apEditorUtil.CHECK_EDITABLE_RESULT checkResult = apEditorUtil.CheckEditablePortrait(curPortraitInScene);
						if(checkResult != apEditorUtil.CHECK_EDITABLE_RESULT.Valid)
						{
							//프리팹 에셋 등의 이유로 유효하지 않다면
							isValid = false;
						}

						if(isValid)//유효한 경우에만 로딩
						{
							//변경 > 비동기 로딩
							apPortrait selectedPortrait = curPortraitInScene;
							if (selectedPortrait != null)
							{
								//선택된게 있다면 비동기 로딩
								LoadPortraitAsync(selectedPortrait);
							}
							else
							{
								//선택된게 없다면 동기 로딩 (비어있는걸 할당)
								_portrait = selectedPortrait;
								_selection.SelectNone();

								SyncHierarchyOrders();

								_hierarchy.ResetAllUnits();
								_hierarchy_MeshGroup.ResetSubUnits();
								_hierarchy_AnimClip.ResetSubUnits();
							}

						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(10);
				}


			}
			else
			{
				if (_tabLeft == TAB_LEFT.Hierarchy)
				{
					//1. Hierarchy 탭
					Hierarchy.GUI_RenderHierarchy(width, _hierarchyFilter, scroll, height - 20, _isHierarchyOrderEditEnabled && _hierarchySortMode == HIERARCHY_SORT_MODE.Custom);
				}
				else
				{
					//2. Control Parameter 탭
					apControllerGL.ReadyToUpdate(
						_mouseSet.GetStatus(apMouseSet.Button.LeftNoBound, apMouseSet.ACTION.ControllerGL),
						_mouseSet.PosNotBound,
						Event.current.isMouse
						);

					//Profiler.BeginSample("Left : Controller UI");
					//컨트롤 파라미터를 처리하자
					Controller.GUI_Controller(width, height, (int)scroll.y);
					//Profiler.EndSample();


					//위치 변경 21.2.9 : 이게 왜 밖에 있었지?
					apControllerGL.EndUpdate();

				}
			}
		}






		//------------------------------------------------------------------------------
		// GUI - Center <중요> : 작업 공간
		//------------------------------------------------------------------------------
		private void GUI_Center(int width, int height)
		{
			//변경 : 일부 이벤트는 Ignored가 발생했지만 마우스 이벤트로서 작동을 해야한다.
			//마우스 업 이벤트가 발생했지만, 다른 Area로 이동하면서 Ignored로 바뀐 경우는 특별히 처리해야한다.
			bool isIgnoredMouseUpEvent = false;


			if (Event.current.type != EventType.Repaint
				&& !Event.current.isMouse
				&& !Event.current.isKey)
			{
				//추가 : 20.3.31 : 만약 이 경우에 마우스 이벤트는 맞는데, 위에서 Ignored가 발생했다면 리턴하면 안된다.
				bool isMouseEvent = Event.current.isMouse
									|| Event.current.rawType == EventType.MouseDown
									|| Event.current.rawType == EventType.MouseDrag
									|| Event.current.rawType == EventType.MouseMove
									|| Event.current.rawType == EventType.MouseUp;

				if (Mouse.IsAnyButtonUpEvent
					&& Event.current.type == EventType.Ignore
					&& isMouseEvent
					)
				{
					//이 상황이면 리턴하면 안된다.
					isIgnoredMouseUpEvent = true;
					//Debug.LogWarning("Up Event And Ignored [" + Event.current.type + " / " + Event.current.rawType + "]");
				}
				else
				{
					//Raw 마우스 타입도 아니다.
					return;
				}

			}

			float deltaTime = 0.0f;
			if (Event.current.type == EventType.Repaint)
			{
				deltaTime = DeltaTime_Repaint;
			}




			//--------------------------------------------------------
			//      업데이트 / 입력
			//--------------------------------------------------------

			//Input은 여기서 미리 처리한다.
			//--------------------------------------------------
			bool isMeshGroupUpdatable = _isUpdateWhenInputEvent || Event.current.type == EventType.Repaint;
			if (_isUpdateWhenInputEvent)
			{
				_isUpdateWhenInputEvent = false;
			}

			//추가 : GUI Repaint할 시간이 아니더라도, MeshGroup이 변경되었다면
			//GUI를 Repaint해야한다.
			if (!_isValidGUIRepaint)
			{
				if (_isMeshGroupChangedByEditor)
				{
					_isValidGUIRepaint = true;
				}
			}
			_isMeshGroupChangedByEditor = false;


			//추가 : MeshGroup를 업데이트 하기 위한 옵션을 설정한다.
			bool isUpdate_BoneIKMatrix = _selection.IsBoneIKMatrixUpdatable;
			bool isUpdate_BoneIKRigging = _selection.IsBoneIKRiggingUpdatable;
			bool isRender_BoneIK = _selection.IsBoneIKRenderable && isUpdate_BoneIKMatrix;



			//클릭 후 GUIFocust를 릴리즈하자
			Controller.GUI_Input_CheckClickInCenter();


			//--------------------------------------------------
			//추가 21.1.19 : GUI 버튼 업데이트
			//Menu 버튼은 항상 나온다.
			//Margin 5에 크기 32이니까 5 + 16 = 21

			int guiMenuPosX = GUI_STAT_MARGIN + (GUI_STAT_MENUBTN_SIZE / 2);
			int guiMenuPosY = GUI_STAT_MARGIN + (GUI_STAT_MENUBTN_SIZE / 2);
			if (_guiButton_Menu.Update( new Vector2(guiMenuPosX, guiMenuPosY), 
										Mouse.Pos, Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu)))
			{
				//메뉴가 보여야 한다.
				GUIMenu.ShowMenu_GUIView(OnViewMenu,
					//new Rect(_mainGUIRect.x + _guiButton_Menu._pos.x - (_guiButton_Menu._width / 2),
					//		_mainGUIRect.y + _guiButton_Menu._pos.y + (_guiButton_Menu._height / 2),
					new Rect(_guiButton_Menu._pos.x - (_guiButton_Menu._width / 2),
							_guiButton_Menu._pos.y + (_guiButton_Menu._height / 2),
							500, 0));
				//Event.current.Use();
				Mouse.UseMouseButton(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu);
				Mouse.UseMouseButton(apMouseSet.Button.LeftNoBound, apMouseSet.ACTION.GUIMenu);
				Mouse.Update_ReleaseForce();
			}
			//커서 이동
			guiMenuPosX += GUI_STAT_MENUBTN_SIZE + GUI_STAT_MENUBTN_MARGIN_DIFGROUP;//Width + 약간의 Margin
		
			//OnionSkin Record 버튼도 추가할 수 있다.
			if (Onion.IsVisible
				&& ((Select.SelectionType == apSelection.SELECTION_TYPE.Animation && !_onionOption_IsRenderAnimFrames)
					|| Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup))
			{
				if (_guiButton_RecordOnion.Update(	new Vector2(guiMenuPosX, guiMenuPosY), 
													Mouse.Pos, Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu)))
				{
					//현재 상태를 기록한다.
					Onion.Record(this);

					//마우스 이벤트 종료
					Mouse.UseMouseButton(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu);
					Mouse.UseMouseButton(apMouseSet.Button.LeftNoBound, apMouseSet.ACTION.GUIMenu);
					Mouse.Update_ReleaseForce();
				}

				guiMenuPosX += GUI_STAT_MENUBTN_SIZE + GUI_STAT_MENUBTN_MARGIN_DIFGROUP;//Width + 약간의 Margin
			}
			else
			{
				_guiButton_RecordOnion.Hide();
			}

			//추가 22.3.20 : Morph 모디파이어 편집시(메시 그룹 or 애니메이션)에는 Vertex를 결정할 수 있다.
			bool isMorphEditing = false;
			
			if (_selection.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
				&& _meshGroupEditMode == MESHGROUP_EDIT_MODE.Modifier
				&& _selection.Modifier != null
				&& _selection.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Morph)
			{
				isMorphEditing = true;
			}
			else if(_selection.SelectionType == apSelection.SELECTION_TYPE.Animation
				&& _selection.AnimTimeline != null
				&& _selection.AnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
				&& _selection.AnimTimeline._linkedModifier != null
				&& _selection.AnimTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
			{
				isMorphEditing = true;
			}

			if(isMorphEditing)
			{
				//Morph 모디파이어 한정해서 GUI 버튼 등장
				//Morph시 [Vertex]를 편집하는 모드로 변경
				if (_guiButton_MorphEditVert.Update(	new Vector2(guiMenuPosX, guiMenuPosY), 
													Mouse.Pos, Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu),
													_selection.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex))
				{
					if (_selection.MorphEditTarget != apSelection.MORPH_EDIT_TARGET.Vertex)
					{
						//v1.4.2 : 모달 상태를 체크해야한다.
						bool isExecutable = CheckModalAndExecutable();

						if (isExecutable)
						{
							//타겟 변경 + 선택 해제
							Select.SetMorphEditTarget(apSelection.MORPH_EDIT_TARGET.Vertex);


							//마우스 이벤트 종료
							Mouse.UseMouseButton(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu);
							Mouse.UseMouseButton(apMouseSet.Button.LeftNoBound, apMouseSet.ACTION.GUIMenu);
							Mouse.Update_ReleaseForce();
						}
					}
				}
				guiMenuPosX += GUI_STAT_MENUBTN_SIZE + GUI_STAT_MENUBTN_MARGIN_SAMEGROUP;//같은 그룹간에는 Margin이 조금 작다.

				//Morph시 [Pin]을 편집하는 모드로 변경
				if (_guiButton_MorphEditPin.Update(	new Vector2(guiMenuPosX, guiMenuPosY), 
													Mouse.Pos, Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu),
													_selection.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Pin))
				{
					if (_selection.MorphEditTarget != apSelection.MORPH_EDIT_TARGET.Pin)
					{
						//v1.4.2 : 모달 상태를 체크해야한다.
						bool isExecutable = CheckModalAndExecutable();

						if (isExecutable)
						{
							//타겟 변경 + 선택 해제
							Select.SetMorphEditTarget(apSelection.MORPH_EDIT_TARGET.Pin);

							//마우스 이벤트 종료
							Mouse.UseMouseButton(apMouseSet.Button.Left, apMouseSet.ACTION.GUIMenu);
							Mouse.UseMouseButton(apMouseSet.Button.LeftNoBound, apMouseSet.ACTION.GUIMenu);
							Mouse.Update_ReleaseForce();
						}
					}
				}

				guiMenuPosX += GUI_STAT_MENUBTN_SIZE + GUI_STAT_MENUBTN_MARGIN_DIFGROUP;//Width + 약간의 Margin
			}
			else
			{
				_guiButton_MorphEditVert.Hide();
				_guiButton_MorphEditPin.Hide();
			}
			//--------------------------------------------------



			switch (_selection.SelectionType)
			{
				case apSelection.SELECTION_TYPE.None:
					break;

				case apSelection.SELECTION_TYPE.Overall:
					{

						if (Event.current.type == EventType.Repaint)//업데이트는 Repaint에서만
						{
							if (Select.RootUnit != null)
							{
								//Debug.Log("Draw >>>>>>>>>>>>>>>");
								if (Select.RootUnitAnimClip != null)
								{
									//실행중인 AnimClip이 있다.
									int curFrame = Select.RootUnitAnimClip.CurFrame;

									if (_isValidGUIRepaint)
									{
										//추가 20.7.9 : 물리 타이머를 갱신하자
										_portrait.CalculatePhysicsTimer();

										_portrait.ForceManager.Update(deltaTime);//<<힘 업데이트 추가

										//애니메이션 재생!!!
										//Profiler.BeginSample("Anim - Root Animation");

										//추가
										if (Select.RootUnitAnimClip._targetMeshGroup != null)
										{
											Select.RootUnitAnimClip._targetMeshGroup.SetBoneIKEnabled(isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging);
										}

										//TODO
										//Update_Editor에 DLL 옵션을 넣어서 빠른 실행이 가능하도록 만들자

										Select.RootUnitAnimClip.Update_Editor(
															deltaTime,
															true,
															isUpdate_BoneIKMatrix,
															isUpdate_BoneIKRigging,
															IsUseCPPDLL);
										//Profiler.EndSample();


									}

									//프레임이 바뀌면 AutoScroll 옵션을 적용한다.
									if (curFrame != Select.RootUnitAnimClip.CurFrame)
									{
										if (_isAnimAutoScroll)
										{
											Select.SetAutoAnimScroll();
										}
									}
								}
								else
								{
									//AnimClip이 없다면 자체 실행
									if(IsUseCPPDLL)
									{
										//C++ DLL 을 이용하는 버전 (21.5.13)
										Select.RootUnit.Update_DLL(deltaTime, isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging);
									}
									else
									{
										//일반 버전
										Select.RootUnit.Update(deltaTime, isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging);
									}
									
								}

							}
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Param:
					break;

				case apSelection.SELECTION_TYPE.ImageRes:
					break;

				case apSelection.SELECTION_TYPE.Mesh:
					if (_selection.Mesh != null)
					{
						switch (_meshEditMode)
						{
							case MESH_EDIT_MODE.Setting:
								//GUI이벤트 추가 21.1.6
								//Area를 편집하는 GUI 도구 추가
								if (_selection.Mesh._isPSDParsed
									&& _isMeshEdit_AreaEditing)
								{
									Controller.GUI_Input_MakeMesh_AtlasAreaEdit();
								}

								if (_meshGeneratorV2 != null && _meshGeneratorV2.IsProcessing)
								{
									_meshGeneratorV2.Update();
								}
								break;

							case MESH_EDIT_MODE.Modify:
								Controller.GUI_Input_Modify(deltaTime, isIgnoredMouseUpEvent);
								break;

							case MESH_EDIT_MODE.MakeMesh:
								if (_meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
								{
									Controller.GUI_Input_MakeMesh_TRS(deltaTime, isIgnoredMouseUpEvent);

								}
								else if (_meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen)
								{
									//Area를 편집하는 GUI 도구 추가 21.1.6
									if (_selection.Mesh._isPSDParsed && _isMeshEdit_AreaEditing)
									{
										Controller.GUI_Input_MakeMesh_AtlasAreaEdit();
									}
								}
								else
								{
									//Make Mesh 기본 도구
									Controller.GUI_Input_MakeMesh(_meshEditeMode_MakeMesh_AddTool);
								}

								//추가 20.12.9 : 자동 생성 기능 업데이트
								if (_meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen)
								{
									if (_meshGeneratorV2 != null && _meshGeneratorV2.IsProcessing)
									{
										_meshGeneratorV2.Update();
									}
								}
								break;

							case MESH_EDIT_MODE.PivotEdit:
								Controller.GUI_Input_PivotEdit(deltaTime, isIgnoredMouseUpEvent);
								break;

								//추가 22.3.2 (v1.4.0) : 핀 편집
							case MESH_EDIT_MODE.Pin:
								{
									if(_meshEditMode_Pin_ToolMode == MESH_EDIT_PIN_TOOL_MODE.Test)
									{
										//테스트 모드의 경우
										_selection.Mesh.UpdatePinTestMode(_selection.MeshPin);
									}
									else
									{
										//그 외의 기본 편집 모드의 경우
										_selection.Mesh.UpdatePinDefaultMode(_selection.MeshPin);
									}

									Controller.GUI_Input_MeshPinEdit(deltaTime, isIgnoredMouseUpEvent);
								}
								
								break;
						}
					}
					break;

				case apSelection.SELECTION_TYPE.MeshGroup:
					{
						switch (_meshGroupEditMode)
						{
							case MESHGROUP_EDIT_MODE.Setting:
								Controller.GUI_Input_MeshGroup_Setting(deltaTime, isIgnoredMouseUpEvent);
								break;

							case MESHGROUP_EDIT_MODE.Bone:
								//Debug.Log("Bone Edit : " + Event.current.type);
								Controller.GUI_Input_MeshGroup_Bone(deltaTime, isIgnoredMouseUpEvent);
								break;

							case MESHGROUP_EDIT_MODE.Modifier:
								{
									apModifierBase.MODIFIER_TYPE modifierType = apModifierBase.MODIFIER_TYPE.Base;
									if (Select.Modifier != null)
									{
										modifierType = Select.Modifier.ModifierType;
									}
									Controller.GUI_Input_MeshGroup_Modifier(modifierType, deltaTime, isIgnoredMouseUpEvent);

								}
								break;
						}

						if (Event.current.type == EventType.Repaint)//업데이트는 Repaint에서만
						{
							if (Select.MeshGroup != null)
							{
								if (_isValidGUIRepaint)
								{
									//추가 20.7.9 : 물리 타이머를 갱신하자
									_portrait.CalculatePhysicsTimer();

									_portrait.ForceManager.Update(deltaTime);//<<힘 업데이트 추가

									//변경 : 업데이트 가능한 상태에서만 업데이트를 한다.
									if (isMeshGroupUpdatable)
									{
										//Profiler.BeginSample("MeshGroup Update");
										//1. Render Unit을 돌면서 렌더링을 한다.
										Select.MeshGroup.SetBoneIKEnabled(isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging);

										if (IsUseCPPDLL)
										{
											//추가 21.5.14 : C++ DLL로 업데이트를 한다.
											Select.MeshGroup.UpdateRenderUnits_DLL(deltaTime, true);
										}
										else
										{
											Select.MeshGroup.UpdateRenderUnits(deltaTime, true);
										}




										Select.MeshGroup.SetBoneIKEnabled(false, false);

										//추가 : Bone GUI를 여기서 업데이트한번 해야한다.
										//Select.MeshGroup.BoneGUIUpdate(false);
										//Profiler.EndSample();
									}
								}
							}
						}


					}
					break;

				case apSelection.SELECTION_TYPE.Animation:
					{

						Controller.GUI_Input_Animation(deltaTime, isIgnoredMouseUpEvent);

						if (Event.current.type == EventType.Repaint)//업데이트는 Repaint에서만
						{
							if (Select.AnimClip != null)
							{
								if (_isValidGUIRepaint)
								{
									//추가 20.7.9 : 물리 타이머를 갱신하자
									_portrait.CalculatePhysicsTimer();

									_portrait.ForceManager.Update(deltaTime);//<<힘 업데이트 추가

									//변경 : 업데이트 가능한 상태에서만 업데이트를 한다.
									if (isMeshGroupUpdatable)
									{
										int curFrame = Select.AnimClip.CurFrame;
										//애니메이션 업데이트를 해야..
										Select.AnimClip.Update_Editor(deltaTime,
											Select.ExAnimEditingMode != apSelection.EX_EDIT.None,
											isUpdate_BoneIKMatrix,
											isUpdate_BoneIKRigging,
											IsUseCPPDLL);

										//프레임이 바뀌면 AutoScroll 옵션을 적용한다.
										if (curFrame != Select.AnimClip.CurFrame)
										{
											if (_isAnimAutoScroll)
											{
												Select.SetAutoAnimScroll();
											}
										}
									}
								}
							}
						}


					}
					break;
			}



			//추가 22.12.14
			//기즈모에서 방향키를 점유하지 않았다면, Hierarchy용 단축키로서 사용될 수 있다.
			if(LastClickedHierarchy != LAST_CLICKED_HIERARCHY.None)
			{
				if(!Gizmos.IsArrowHotKeyOccupied)
				{
					//단축키가 사용되지 않았다면
					AddReservedHotKeyEvent(OnHotKeyEvent_MoveHierarchyCursor, apHotKey.RESERVED_KEY.Arrow, null);
				}
			}



			if (_isValidGUIRepaint)
			{
				_isValidGUIRepaint = false;
			}




			//--------------------------------------------------------
			//      렌더링
			//--------------------------------------------------------

			//렌더링은 Repaint에서만
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}

			

			//만약 렌더링 클리핑을 갱신할 필요가 있다면 처리할 것
			if (_isRefreshClippingGL)
			{
				_isRefreshClippingGL = false;
				apGL.RefreshScreenSizeToBatch();
			}

			//추가 21.1.29 : Visibility Preset을 호출한다.
			CheckAndSyncVisiblityPreset(false, false);


			//그리드 그리기
			apGL.DrawGrid(_colorOption_GridCenter, _colorOption_Grid);


			//로토스코핑 그리기
			DrawRotoscoping();


			//렌더 요청 초기화
			_renderRequest_Normal.Reset();
			_renderRequest_Selected.Reset();

			switch (_selection.SelectionType)
			{
				case apSelection.SELECTION_TYPE.None:
					break;

				case apSelection.SELECTION_TYPE.Overall:
					{
						//_portrait._rootUnit.Update(DeltaFrameTime);

						if (Select.RootUnit != null)
						{
							if (Select.RootUnit._childMeshGroup != null)
							{
								apMeshGroup rootMeshGroup = Select.RootUnit._childMeshGroup;
								

								RenderMeshGroup(rootMeshGroup,
													
													//apGL.RENDER_TYPE.Default, apGL.RENDER_TYPE.Default,//이전													
													_renderRequest_Normal, _renderRequest_Selected,//변경 22.3.3

													null, null, null, null,
													true, _boneGUIRenderMode, _meshGUIRenderMode, isRender_BoneIK,
													BONE_RENDER_TARGET.AllBones);

								//Debug.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<");
							}
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Param:
					if (_selection.Param != null)
					{
						apGL.DrawBox(Vector2.zero, 600, 300, Color.cyan, true);
						apGL.DrawText(apStringFactory.I.ParamEdit, new Vector2(-30, 30), 70, Color.yellow);
						apGL.DrawText(_selection.Param._keyName, new Vector2(-30, -15), 200, Color.yellow);
					}
					break;

				case apSelection.SELECTION_TYPE.ImageRes:
					if (_selection.TextureData != null)
					{
						apGL.DrawTexture(_selection.TextureData._image,
											Vector2.zero,
											_selection.TextureData._width,
											_selection.TextureData._height,
											new Color(0.5f, 0.5f, 0.5f, 1.0f));
					}
					break;

				case apSelection.SELECTION_TYPE.Mesh:
					if (_selection.Mesh != null)
					{
						//apGL.RENDER_TYPE renderType = apGL.RENDER_TYPE.Default;//이전
						_renderRequest_Normal.Reset();//변경 22.3.3 (v1.4.0)

						bool isEdgeExpectRender = false;
						bool isEdgeExpectRenderSnapToEdge = false;
						bool isEdgeExpectRenderSnapToVertex = false;
						bool isShowSnappedPin = false;

						switch (_meshEditMode)
						{
							case MESH_EDIT_MODE.Setting:
								//renderType |= apGL.RENDER_TYPE.Outlines;//이전
								_renderRequest_Normal.SetOutlines();//변경 22.3.3
								
								if (_selection.Mesh._isPSDParsed && _isMeshEdit_AreaEditing)
								{
									//Area 편집중에는 전부 보여야 함
									_renderRequest_Normal.SetAllMesh();
									_renderRequest_Normal.SetAllEdges();
								}
								else
								{
									_renderRequest_Normal.SetTransparentEdges();
									_renderRequest_Normal.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
									_renderRequest_Normal.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
								}
								break;

							case MESH_EDIT_MODE.Modify:
								_renderRequest_Normal.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
								_renderRequest_Normal.SetAllEdges();
								_renderRequest_Normal.SetShadeAllMesh();

								if (_meshEditZDepthView == MESH_EDIT_RENDER_MODE.ZDepth)
								{
									_renderRequest_Normal.SetVolumeWeightColor();
								}
								else
								{
									//ZDepth를 보일때 외에는 핀이 보인다 [v1.4.2]
									_renderRequest_Normal.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
								}
								break;

							case MESH_EDIT_MODE.MakeMesh:
								{
									//공통
									_renderRequest_Normal.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
									_renderRequest_Normal.SetAllEdges();

									if (_meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.AddTools)
									{
										if (_meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon)
										{
											_renderRequest_Normal.SetAllEdges();
											_renderRequest_Normal.SetShadeAllMesh();
											_renderRequest_Normal.SetPolygonOutline();

										}
										else
										{
											_renderRequest_Normal.SetAllEdges();
											_renderRequest_Normal.SetShadeAllMesh();

											if (_meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge ||
												_meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly)
											{
												isEdgeExpectRender = true;
											}
											if (_meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge ||
												_meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly)
											{
												isEdgeExpectRenderSnapToEdge = true;
											}
											isEdgeExpectRenderSnapToVertex = true;
										}
									}
									else
									{
										//TRS와 AutoGen
										
										if (_selection.Mesh._isPSDParsed && _isMeshEdit_AreaEditing)
										{
											_renderRequest_Normal.SetAllMesh();
										}
										else
										{
											_renderRequest_Normal.SetShadeAllMesh();
										}
									}

									//[v1.4.2] Make Mesh에서도 Pin이 반투명으로 보인다.
									_renderRequest_Normal.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
								}
								break;

							case MESH_EDIT_MODE.PivotEdit:
								{
									_renderRequest_Normal.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
									_renderRequest_Normal.SetOutlines();

									//[v1.4.2] Pin이 반투명으로 보인다.
									_renderRequest_Normal.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
								}
								break;

							case MESH_EDIT_MODE.Pin:
								{
									_renderRequest_Normal.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
									_renderRequest_Normal.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
									_renderRequest_Normal.SetAllEdges();
									_renderRequest_Normal.SetPinVertWeight();

									if(_meshEditMode_Pin_ToolMode != MESH_EDIT_PIN_TOOL_MODE.Add)
									{
										//Add 툴 외에는 Range를 보여준다.
										_renderRequest_Normal.SetPinRange();
									}

									if(_meshEditMode_Pin_ToolMode == MESH_EDIT_PIN_TOOL_MODE.Test)
									{
										//Pin 가중치를 테스트한다.
										_renderRequest_Normal.SetTestPinWeight();
									}

									if(_meshEditMode_Pin_ToolMode == MESH_EDIT_PIN_TOOL_MODE.Add
										|| _meshEditMode_Pin_ToolMode == MESH_EDIT_PIN_TOOL_MODE.Link)
									{
										isShowSnappedPin = true;
									}
								}
								break;

						}




						apGL.DrawMesh(_selection.Mesh,
								apMatrix3x3.identity,
								new Color(0.5f, 0.5f, 0.5f, 1.0f),
								
								//renderType,//이전
								_renderRequest_Normal,//변경 22.3.3


								VertController, this,
								_mouseSet.Pos,
								_isMeshEdit_AreaEditing
								);

						if (_meshEditMode == MESH_EDIT_MODE.MakeMesh && _meshEditMirrorMode == MESH_EDIT_MIRROR_MODE.Mirror)
						{
							MirrorSet.Refresh(Select.Mesh, false);
							if (_meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
							{
								apGL.DrawMirrorMeshPreview(Select.Mesh, MirrorSet, this, VertController);
							}
						}

						//추가 8.24 : 메시 자동 생성 기능 미리보기
						//삭제 21.1.4 : V2로 바뀌었다.
						//if (_meshEditMode == MESH_EDIT_MODE.MakeMesh
						//	&& _meshEditeMode_MakeMesh == MESH_EDIT_MODE_MAKEMESH.AutoGenerate)
						//{
						//	//자동 생성 기능
						//	if (MeshGenerator.IsScanned)
						//	{
						//		//스캔된 상태
						//		apGL.DrawMeshAutoGenerationPreview(_selection.Mesh,
						//									apMatrix3x3.identity,
						//									this, MeshGenerator);
						//	}
						//}


						if (_meshEditMode == MESH_EDIT_MODE.PivotEdit)
						{
							//가운데 십자선
							apGL.DrawBoldLine(new Vector2(0, -10), new Vector2(0, 10), 5, new Color(0.3f, 0.7f, 1.0f, 0.7f), true);
							apGL.DrawBoldLine(new Vector2(-10, 0), new Vector2(10, 0), 5, new Color(0.3f, 0.7f, 1.0f, 0.7f), true);
						}

						////추가 9.7 : 미러 모드의 축을 출력하자
						//if(_meshEditMode == MESH_EDIT_MODE.MakeMesh && _meshEditMirrorMode == MESH_EDIT_MIRROR_MODE.Mirror)
						//{
						//	apGL.DrawMeshMirror(_selection.Mesh);
						//}


						if (_meshEditMode == MESH_EDIT_MODE.MakeMesh)
						{
							if (isEdgeExpectRenderSnapToEdge && VertController.IsTmpSnapToEdge)
							{
								apGL.DrawMeshWorkEdgeSnap(_selection.Mesh, VertController);
							}
							if (isEdgeExpectRenderSnapToVertex && VertController.LinkedNextVertex != null)
							{
								apGL.DrawMeshWorkSnapNextVertex(_selection.Mesh, VertController);
							}


							if (isEdgeExpectRender && VertController.IsEdgeWireRenderable())
							{
								//마우스 위치에 맞게 Connect를 예측할 수 있게 만들자

								apGL.DrawMeshWorkEdgeWire(_selection.Mesh, apMatrix3x3.identity, VertController, VertController.IsEdgeWireCross(), VertController.IsEdgeWireMultipleCross());

								if (_meshEditMirrorMode == MESH_EDIT_MIRROR_MODE.Mirror
									&& _meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.AddTools
								&& (_meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge
									|| _meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly
									|| _meshEditeMode_MakeMesh_AddTool == MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly)
									)
								{
									//여기서 한번 더 -> 반전되서 처리 가능한 Mesh Edge Work를 계산한다.
									MirrorSet.RefreshMeshWork(Select.Mesh, VertController);
									//apGL.DrawMeshWorkMirrorEdgeWire(_selection.Mesh, MirrorSet, Mouse.IsPressed(apMouseSet.Button.LeftNoBound, apMouseSet.ACTION.MeshEdit_Make));
									apGL.DrawMeshWorkMirrorEdgeWire(_selection.Mesh, MirrorSet);
								}
							}
						}

						if(isShowSnappedPin)
						{
							if(_selection.SnapPin != null && _selection.SnapPin != _selection.MeshPin)
							{
								apGL.DrawMeshWorkSnapNextPin(_selection.Mesh, _selection.SnapPin);
							}

							if(_selection.IsPinEdit_MouseWire)
							{	
								apGL.DrawMeshWorkPinWire(_selection.Mesh, _selection.MeshPin, _selection.PinEdit_MouseWirePosW, _selection.SnapPin);
							}
							
						}

						//추가 21.1.6 : Area 편집시 (Setting 또는 AutoGen)
						if (_isMeshEdit_AreaEditing
							&& Select.Mesh._isPSDParsed
							&& (_meshEditMode == MESH_EDIT_MODE.Setting || (_meshEditMode == MESH_EDIT_MODE.MakeMesh && _meshEditeMode_MakeMesh_Tab == MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen))
							)
						{
							apGL.DrawMeshAreaEditing(Select.Mesh, apMatrix3x3.identity, this, _mouseSet.Pos);
						}

						//추가 20.7.6 : 메시가 출력된 이후, PSD 임포트 메시의 버텍스를 삭제할지 묻는 메시지가 출력될 수 있다.
						if (_isRequestRemoveVerticesIfImportedFromPSD_Step1)
						{
							_isRequestRemoveVerticesIfImportedFromPSD_Step2 = true;
						}
					}
					break;

				case apSelection.SELECTION_TYPE.MeshGroup:
					{
						//이전
						//apGL.RENDER_TYPE selectRenderType = apGL.RENDER_TYPE.Default;
						//apGL.RENDER_TYPE meshRenderType = apGL.RENDER_TYPE.Default;

						//변경 22.3.3 (v1.4.0)
						_renderRequest_Normal.Reset();
						_renderRequest_Selected.Reset();


						switch (_meshGroupEditMode)
						{
							case MESHGROUP_EDIT_MODE.Setting:

								//selectRenderType |= apGL.RENDER_TYPE.Outlines;	//이전
								_renderRequest_Selected.SetOutlines();				//변경 22.3.3

								if (Select.IsMeshGroupSettingEditDefaultTransform)
								{
									//selectRenderType |= apGL.RENDER_TYPE.TransformBorderLine;	//이전
									_renderRequest_Selected.SetTransformBorderLine();			//변경 22.3.3
								}

								break;
							case MESHGROUP_EDIT_MODE.Bone:
								//이전
								//selectRenderType |= apGL.RENDER_TYPE.Vertex;
								//selectRenderType |= apGL.RENDER_TYPE.AllEdges;

								//변경 22.3.3
								_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
								_renderRequest_Selected.SetAllEdges();
								break;


							case MESHGROUP_EDIT_MODE.Modifier:
								{
									apModifierBase.MODIFIER_TYPE modifierType = apModifierBase.MODIFIER_TYPE.Base;
									if (Select.Modifier != null)
									{
										modifierType = Select.Modifier.ModifierType;
										switch (Select.Modifier.ModifierType)
										{
											case apModifierBase.MODIFIER_TYPE.Volume:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.Outlines;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
													_renderRequest_Selected.SetOutlines();
												}
												break;

											case apModifierBase.MODIFIER_TYPE.Morph:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;

													if (Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
													{
														// [ 버텍스 편집 모드 ]
														//변경 22.3.3
														_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
														_renderRequest_Selected.SetAllEdges();

														//추가 22.4.4 : 핀 보이기 (v1.4.0)
														//단 단일 편집 모드에서만 = "다른 모디파이어가 동작하지 않을때"
														if(Select.ExEditingMode == apSelection.EX_EDIT.ExOnly_Edit && !_exModObjOption_UpdateByOtherMod)
														{
															_renderRequest_Selected.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
														}
													}
													else
													{
														// [ 핀 편집 모드 ]
														_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
														_renderRequest_Selected.SetAllEdges();

														_renderRequest_Selected.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
													}
												}
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;

													if (Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
													{
														// [ 버텍스 편집 모드 ]
														//변경 22.3.3
														_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
														_renderRequest_Selected.SetAllEdges();

														//추가 22.4.4 : 핀 보이기 (v1.4.0)
														_renderRequest_Selected.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
													}
													else
													{
														// [ 핀 편집 모드 ]
														_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
														_renderRequest_Selected.SetAllEdges();

														_renderRequest_Selected.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
													}
												}
												break;

											case apModifierBase.MODIFIER_TYPE.Rigging:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;
													//selectRenderType |= apGL.RENDER_TYPE.BoneRigWeightColor;
													//meshRenderType |= apGL.RENDER_TYPE.BoneRigWeightColor;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
													_renderRequest_Selected.SetAllEdges();
													_renderRequest_Selected.SetBoneRigWeightColor();
													_renderRequest_Normal.SetBoneRigWeightColor();
												}
												break;

											case apModifierBase.MODIFIER_TYPE.Physic:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.PhysicsWeightColor;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;
													//meshRenderType |= apGL.RENDER_TYPE.PhysicsWeightColor;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
													_renderRequest_Selected.SetPhysicsWeightColor();
													_renderRequest_Selected.SetAllEdges();
													_renderRequest_Normal.SetPhysicsWeightColor();
												}
												break;

											case apModifierBase.MODIFIER_TYPE.TF:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;
													//selectRenderType |= apGL.RENDER_TYPE.TransformBorderLine;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
													_renderRequest_Selected.SetAllEdges();
													_renderRequest_Selected.SetTransformBorderLine();
												}
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedTF:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;
													//selectRenderType |= apGL.RENDER_TYPE.TransformBorderLine;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
													_renderRequest_Selected.SetAllEdges();
													_renderRequest_Selected.SetTransformBorderLine();
												}
												break;

											case apModifierBase.MODIFIER_TYPE.FFD:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
													_renderRequest_Selected.SetAllEdges();
												}
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
													_renderRequest_Selected.SetAllEdges();
												}
												break;

											//추가 21.7.20
											case apModifierBase.MODIFIER_TYPE.ColorOnly:
											case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly:
												{
													//이전
													//selectRenderType |= apGL.RENDER_TYPE.Vertex;
													//selectRenderType |= apGL.RENDER_TYPE.AllEdges;
													//selectRenderType |= apGL.RENDER_TYPE.TransformBorderLine;

													//변경 22.3.3
													_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
													_renderRequest_Selected.SetAllEdges();
													_renderRequest_Selected.SetTransformBorderLine();
												}
												break;


											case apModifierBase.MODIFIER_TYPE.Base:
												//이전
												//selectRenderType |= apGL.RENDER_TYPE.Outlines;

												//변경 22.3.3.
												_renderRequest_Selected.SetOutlines();
												break;
										}
									}
									else
									{
										//selectRenderType |= apGL.RENDER_TYPE.Outlines;	//이전
										_renderRequest_Selected.SetOutlines();				//변경 22.3.3 (v1.4.0)
									}
								}
								break;
						}

						if (Select.MeshGroup != null)
						{
							//변경 20.5.29
							_tmpSelectedMainMeshTF = Select.MeshTF_Main;
							_tmpSelectedSubMeshTFs = Select.GetSubSeletedMeshTFs(true);
							_tmpSelectedMainBone = Select.Bone;
							_tmpSelectedSubBones = Select.GetSubSeletedBones(true);


							//Onion - Behind인 경우
							RenderOnion(Select.MeshGroup,
								null,
								true,
								Event.current.type == EventType.Repaint,
								isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging, isRender_BoneIK, true,

								_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
								_tmpSelectedMainBone, _tmpSelectedSubBones);


							//기본 MeshGroup 렌더링
							RenderMeshGroup(Select.MeshGroup,
								
								//meshRenderType,		//이전
								//selectRenderType,
								_renderRequest_Normal,	//변경 22.3.3 (v1.4.0)
								_renderRequest_Selected,

								_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
								_tmpSelectedMainBone, _tmpSelectedSubBones,
								false,
								_boneGUIRenderMode,
								_meshGUIRenderMode,
								isRender_BoneIK,
								BONE_RENDER_TARGET.Default);


							//Onion - Top인 경우
							RenderOnion(Select.MeshGroup,
								null,
								false,
								Event.current.type == EventType.Repaint,
								isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging, isRender_BoneIK, true,
								_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
								_tmpSelectedMainBone, _tmpSelectedSubBones);


							//추가:3.22
							//ExMod 편집중인 경우
							//Bone Preview를 할 수도 있다.
							if (Select.ExEditingMode != apSelection.EX_EDIT.None)
							{
								if (Select.Modifier != null)
								{
									if (Select.Modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
									{
										//리깅은 안된다.
										if (_modLockOption_BoneResultPreview)
										{
											//Bone Preview를 하자
											RenderExEditBonePreview_Modifier(Select.MeshGroup, _modLockOption_BonePreviewColor);
										}
									}
								}
							}

							//Bone Edit 중이라면
							//마우스를 이용한 본이 생성될 위치 그리기
							if (Controller.IsBoneEditGhostBoneDraw)
							{
								//옵션에 따라 그려지는 방식이 다르다 [v1.4.2]
								//Add 모드에서는 옵션에 따라 Line/Ghost 중에서 선택할 수 있다.
								//Link 모드에서는 무조건 Line이다.

								if (Select.BoneEditMode == apSelection.BONE_EDIT_MODE.Add)
								{
									//Add 모드에선
									if (_boneGUIOption_NewBonePreview == NEW_BONE_PREVIEW.Line)
									{
										//1. 이전 버전 : 그냥 굵은 선으로 그리기
										apGL.DrawBoldLine(Controller.BoneEditGhostBonePosW_Start,
															Controller.BoneEditGhostBonePosW_End,
															7,
															//new Color(1.0f, 0.0f, 0.5f, 0.8f),//이전 : 고정 색상
															apEditorUtil.GetAnimatedGhostBoneColor(),//변경 : 반짝거리는 색상 [v1.4.2]
															true
															);
									}
									else
									{
										//2. 본 형태로 그리기
										if (_boneGUIOption_RenderType == BONE_DISPLAY_METHOD.Version1)
										{
											//버전 1 : ArrowHead
											int lastBoneShapeWidth = 30;
											int lastBoneShapeTaper = 100;
											if (Select.Bone != null)
											{
												lastBoneShapeWidth = Select.Bone._shapeWidth;
												lastBoneShapeTaper = Select.Bone._shapeTaper;
											}
											else if (Select._isLastBoneShapeWidthChanged)
											{
												lastBoneShapeWidth = Select._lastBoneShapeWidth;
												lastBoneShapeTaper = Select._lastBoneShapeTaper;
											}
											apGL.DrawBone_Virtual_V1(Controller.BoneEditGhostBonePosW_Start,
																		Controller.BoneEditGhostBonePosW_End,
																		apEditorUtil.GetAnimatedGhostBoneColor(),
																		apEditorUtil.GetAnimatedGhostBoneOutlineColor(),
																		lastBoneShapeWidth,
																		lastBoneShapeTaper);
										}
										else
										{
											//버전 2 : Needle
											apGL.DrawBone_Virtual_V2(Controller.BoneEditGhostBonePosW_Start,
																Controller.BoneEditGhostBonePosW_End,
																apEditorUtil.GetAnimatedGhostBoneColor(),
																true);
										}
									}
								}
								else
								{
									//그 외의 모드에선 (Link)
									//굵은선 (조금 가는 선)
									apGL.DrawBoldLine(Controller.BoneEditGhostBonePosW_Start,
															Controller.BoneEditGhostBonePosW_End,
															7,
															apEditorUtil.GetAnimatedLinkBonesColor(),//변경 : 반짝거리는 색상 [v1.4.2]
															true
															);

									apGL.DrawAnimatedLine(	Controller.BoneEditGhostBonePosW_Start,
															Controller.BoneEditGhostBonePosW_End,
															Color.white,
															true);
								}
								
							}
						}

						if (_meshGroupEditMode == MESHGROUP_EDIT_MODE.Modifier)
						{
							//if (GetModLockOption_ListUI(Select.ExEditingMode))
							if (_modLockOption_ModListUI)//변경 21.2.13
							{
								//추가 3.22
								//현재 상태에 대해서 ListUI를 출력하자
								DrawModifierListUI(0, height / 2, Select.MeshGroup, Select.Modifier, null, null, Select.ExEditingMode);
							}
						}

						if (Select.ExEditingMode != apSelection.EX_EDIT.None)
						{
							//Editing이라면
							//화면 위/아래에 붉은 라인들 그려주자
							apGL.DrawEditingBorderline();
						}

						if (Gizmos.IsBrushMode)
						{
							Controller.GUI_PrintBrushCursor(Gizmos);
						}
					}
					break;

				case apSelection.SELECTION_TYPE.Animation:
					{
						if (Select.AnimClip != null && Select.AnimClip._targetMeshGroup != null)
						{
							//apGL.RENDER_TYPE renderType = apGL.RENDER_TYPE.Default;//이전

							//변경 22.3.3
							_renderRequest_Normal.Reset();
							_renderRequest_Selected.Reset();


							bool isVertexRender = false;
							if (Select.AnimTimeline != null)
							{
								if (Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
									Select.AnimTimeline._linkedModifier != null)
								{
									if ((int)(Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
									{
										//VertexPos 제어하는 모디파이어와 연결시
										//Vertex를 보여주자
										isVertexRender = true;

									}
								}
							}
							if (isVertexRender)
							{
								if(Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
								{
									// [ 버텍스 편집 모드 ]
									//이전
									//renderType |= apGL.RENDER_TYPE.Vertex;
									//renderType |= apGL.RENDER_TYPE.AllEdges;

									//변경 22.3.3
									_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
									_renderRequest_Selected.SetAllEdges();

									//추가 22.4.4 : 핀 보이기 (v1.4.0)
									//단, 단일 편집 모드에서만 = "다른 모디파이어가 동작하지 않을때"
									if(Select.ExAnimEditingMode == apSelection.EX_EDIT.ExOnly_Edit && !_exModObjOption_UpdateByOtherMod)
									{
										_renderRequest_Selected.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);	
									}
								}
								else
								{
									// [ 핀 편집 모드 ]
									_renderRequest_Selected.SetVertex(apGL.RenderTypeRequest.SHOW_OPTION.Transparent);
									_renderRequest_Selected.SetAllEdges();

									_renderRequest_Selected.SetPin(apGL.RenderTypeRequest.SHOW_OPTION.Normal);
								}
							}
							else
							{
								//이전
								//renderType |= apGL.RENDER_TYPE.Outlines;

								//변경 22.3.3
								_renderRequest_Selected.SetOutlines();
							}

							//변경 20.5.29
							_tmpSelectedMainMeshTF = Select.MeshTF_Main;
							_tmpSelectedSubMeshTFs = Select.GetSubSeletedMeshTFs(true);
							_tmpSelectedMainBone = Select.Bone;
							_tmpSelectedSubBones = Select.GetSubSeletedBones(true);


							// Onion - Behind 경우
							if (_onionOption_IsRenderAnimFrames)
							{
								RenderAnimatedOnion(Select.AnimClip, true,
														Event.current.type == EventType.Repaint,
														isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging, isRender_BoneIK,
														_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
														_tmpSelectedMainBone, _tmpSelectedSubBones
														);
							}
							else
							{
								RenderOnion(Select.AnimClip._targetMeshGroup,
														Select.AnimClip,
														true,
														Event.current.type == EventType.Repaint,
														isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging, isRender_BoneIK, true,
														_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
														_tmpSelectedMainBone, _tmpSelectedSubBones);
							}


							// 애니메이션-메시그룹 렌더링
							RenderMeshGroup(Select.AnimClip._targetMeshGroup,
												
												//apGL.RENDER_TYPE.Default,	//이전
												//renderType,
												_renderRequest_Normal,		//변경 22.3.3
												_renderRequest_Selected,
												
												_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
												_tmpSelectedMainBone, _tmpSelectedSubBones,
												false,
												_boneGUIRenderMode,
												_meshGUIRenderMode,
												isRender_BoneIK,
												BONE_RENDER_TARGET.Default);

							// Onion - Top인 경우
							// Onion Render 처리를 한다. + 재생중이 아닐때

							if (_onionOption_IsRenderAnimFrames)
							{
								RenderAnimatedOnion(Select.AnimClip, false,
														Event.current.type == EventType.Repaint,
														isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging, isRender_BoneIK,
														_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
														_tmpSelectedMainBone, _tmpSelectedSubBones);
							}
							else
							{
								RenderOnion(Select.AnimClip._targetMeshGroup,
												Select.AnimClip,
												false,
												Event.current.type == EventType.Repaint,
												isUpdate_BoneIKMatrix, isUpdate_BoneIKRigging, isRender_BoneIK, true,
												_tmpSelectedMainMeshTF, _tmpSelectedSubMeshTFs,
												_tmpSelectedMainBone, _tmpSelectedSubBones);
							}



							//추가:3.22
							//ExMod 편집중인 경우
							//Bone Preview를 할 수도 있다.
							if (Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
							{
								if (Select.AnimClip != null && Select.AnimClip._targetMeshGroup != null)
								{
									//리깅은 안된다.
									if(_modLockOption_BoneResultPreview)
									{
										//Bone Preview를 하자
										RenderExEditBonePreview_Animation(Select.AnimClip, Select.AnimClip._targetMeshGroup, _modLockOption_BonePreviewColor);

										Select.AnimClip.Update_Editor(	0.0f, 
																		true, 
																		isUpdate_BoneIKMatrix, 
																		isUpdate_BoneIKRigging, 
																		IsUseCPPDLL);
									}
								}
							}
						}

						if (_modLockOption_ModListUI)//변경 21.2.13
						{
							//추가 3.22
							//현재 상태에 대해서 ListUI를 출력하자
							DrawModifierListUI(0, height / 2, Select.AnimClip._targetMeshGroup, null, Select.AnimClip, Select.AnimTimeline, Select.ExAnimEditingMode);
						}

						//화면 위/아래 붉은 라인을 그려주자
						if (Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
						{
							apGL.DrawEditingBorderline();
						}

						if (Gizmos.IsBrushMode)
						{
							Controller.GUI_PrintBrushCursor(Gizmos);
						}


					}
					break;
			}


			apGL.EndPass();


			//추가 21.6.5 : 가이드라인 그리기
			if(_isEnableGuideLine)
			{
				switch (_selection.SelectionType)
				{
					case apSelection.SELECTION_TYPE.Overall:
					case apSelection.SELECTION_TYPE.MeshGroup:
					case apSelection.SELECTION_TYPE.Animation:						
						DrawGuidelines();
						break;
				}
			}


			if (_isShowCaptureFrame && _portrait != null && Select.SelectionType == apSelection.SELECTION_TYPE.Overall)
			{

				Vector2 framePos_Center = new Vector2(_captureFrame_PosX + apGL.WindowSizeHalf.x, _captureFrame_PosY + apGL.WindowSizeHalf.y);
				Vector2 frameHalfSize = new Vector2(_captureFrame_SrcWidth / 2, _captureFrame_SrcHeight / 2);
				Vector2 framePos_LT = framePos_Center + new Vector2(-frameHalfSize.x, -frameHalfSize.y);
				Vector2 framePos_RT = framePos_Center + new Vector2(frameHalfSize.x, -frameHalfSize.y);
				Vector2 framePos_LB = framePos_Center + new Vector2(-frameHalfSize.x, frameHalfSize.y);
				Vector2 framePos_RB = framePos_Center + new Vector2(frameHalfSize.x, frameHalfSize.y);
				Color frameColor = new Color(0.3f, 1.0f, 1.0f, 0.7f);

				apGL.BeginBatch_ColoredLine();
				apGL.DrawLineGL(framePos_LT, framePos_RT, frameColor, false);
				apGL.DrawLineGL(framePos_RT, framePos_RB, frameColor, false);
				apGL.DrawLineGL(framePos_RB, framePos_LB, frameColor, false);
				apGL.DrawLineGL(framePos_LB, framePos_LT, frameColor, false);

				//<<< 추가 >>>
				//추가 : 썸네일 프레임을 만들자
				float preferAspectRatio = 2.0f; //256 x 128
				float srcAspectRatio = (float)_captureFrame_SrcWidth / (float)_captureFrame_SrcHeight;

				//긴쪽으로 캡쳐 크기를 맞춘다.
				int srcThumbWidth = _captureFrame_SrcWidth;
				int srcThumbHeight = _captureFrame_SrcHeight;

				//AspectRatio = W / H
				if (srcAspectRatio < preferAspectRatio)
				{
					srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
				}
				else
				{
					srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
				}
				srcThumbWidth /= 2;
				srcThumbHeight /= 2;



				Vector2 thumbFramePos_LT = framePos_Center + new Vector2(-srcThumbWidth, -srcThumbHeight);
				Vector2 thumbFramePos_RT = framePos_Center + new Vector2(srcThumbWidth, -srcThumbHeight);
				Vector2 thumbFramePos_LB = framePos_Center + new Vector2(-srcThumbWidth, srcThumbHeight);
				Vector2 thumbFramePos_RB = framePos_Center + new Vector2(srcThumbWidth, srcThumbHeight);

				Color thumbFrameColor = new Color(1.0f, 1.0f, 0.0f, 0.4f);
				apGL.DrawLineGL(thumbFramePos_LT, thumbFramePos_RT, thumbFrameColor, false);
				apGL.DrawLineGL(thumbFramePos_RT, thumbFramePos_RB, thumbFrameColor, false);
				apGL.DrawLineGL(thumbFramePos_RB, thumbFramePos_LB, thumbFrameColor, false);
				apGL.DrawLineGL(thumbFramePos_LB, thumbFramePos_LT, thumbFrameColor, false);
				
				//삭제 21.5.18
				//apGL.EndBatch();
				apGL.EndPass();
			}

			//이후에 나올 아이콘의 크기들
			float scaledIconSize = (float)GUI_STAT_ICON_SIZE / apGL.Zoom;

			//--------------------------------------------------
			//추가 21.1.19 : GUI 버튼 + 상태 그리기
			_guiButton_Menu.Draw();
			_guiButton_RecordOnion.Draw();
			_guiButton_MorphEditPin.Draw();
			_guiButton_MorphEditVert.Draw();

			//순서
			//LowCPU / Mesh / Bone / Physics / Onion Skin / Preset Visible / Rotoscoping
			//int iconPosY = GUI_STAT_MARGIN + GUI_STAT_MENUBTN_SIZE + 2 + (GUI_STAT_ICON_SIZE / 2); //32 + 14 + 4(여백)
			//int iconPosX = GUI_STAT_MARGIN + (GUI_STAT_ICON_SIZE / 2);//5 + 14
			
			bool isAnyStatIcons = false;


			//--------------------------------------------------


			//int guiLabelY = 20;//기존
			//int guiLabelY = 48;//변경 21.1.19 : 16 + 32 : GUI Menu 추가
			//int guiLabelY = 66;//변경 21.1.19 : 16 + 50: GUI Menu / Icon
			//아이콘에 따라서 기본 위치 다름
			int guiLabelY = isAnyStatIcons ? (GUI_STAT_MARGIN + GUI_STAT_MENUBTN_SIZE + 2 + GUI_STAT_ICON_SIZE + 2) : (GUI_STAT_MARGIN + GUI_STAT_MENUBTN_SIZE + 2);
			if (_guiOption_isFPSVisible)
			{
				//이전 : 최적화 코드 아님
				//apGL.DrawTextGL("FPS " + FPS, new Vector2(10, guiLabelY), 150, Color.yellow);
				//변경 19.11.23 : 텍스트 최적화
				if(_fpsString == null)
				{
					_fpsString = new apStringWrapper(16);
					_fpsString.SavePresetText("FPS ");
					//_fpsString.SavePresetText(" / Low ");
					//_fpsString.SavePresetText(" / High ");
				}
				_fpsString.Clear();
				_fpsString.AppendPreset(0, false);
				//_fpsString.Append(FPS, true);
				_fpsString.Append(_fpsCounter.AvgFPS, true);


				apGL.DrawTextGL(_fpsString.ToString(), new Vector2(GUI_STAT_MARGIN, guiLabelY), 150, Color.yellow);

				guiLabelY += 15;
			}

			if (_isNotification_GUI)
			{
				Color notiColor = Color.yellow;
				if (_tNotification_GUI < 1.0f)
				{
					notiColor.a = _tNotification_GUI;
				}
				apGL.DrawTextGL(_strNotification_GUI, new Vector2(GUI_STAT_MARGIN, guiLabelY), 400, notiColor);
				guiLabelY += 15;
			}



			if (Backup.IsAutoSaveWorking() && _isBackupProcessing)
			{
				//자동 저장 중일때
				
				//int yPos = 35 + 8 + iconSize / 2;
				//if(_isNotification_GUI)
				//{
				//	yPos += 15;
				//}

				int yPos = guiLabelY + 8 + GUI_STAT_ICON_SIZE / 2;
				guiLabelY += GUI_STAT_ICON_SIZE + 4;


				Color labelColor = Color.yellow;
				float alpha = Mathf.Sin((_tBackupProcessing_Label / BACKUP_LABEL_TIME_LENGTH) * Mathf.PI * 2.0f);
				//-1 ~ 1
				//-0.2 ~ 0.2 (x0.2)
				//0.6 ~ 1(+0.8)
				alpha = (alpha * 0.2f) + 0.8f;
				labelColor.a = alpha;


				if (_imgBackupIcon_Frame1 == null || _imgBackupIcon_Frame2 == null)
				{
					_imgBackupIcon_Frame1 = ImageSet.Get(apImageSet.PRESET.AutoSave_Frame1);
					_imgBackupIcon_Frame2 = ImageSet.Get(apImageSet.PRESET.AutoSave_Frame2);
				}


				if (_tBackupProcessing_Icon < BACKUP_ICON_TIME_LENGTH * 0.5f)
				{
					apGL.DrawTextureGL(_imgBackupIcon_Frame1, new Vector2(GUI_STAT_MARGIN + GUI_STAT_ICON_SIZE / 2, yPos), scaledIconSize, scaledIconSize, Color.gray, 0.0f);
				}
				else
				{
					apGL.DrawTextureGL(_imgBackupIcon_Frame2, new Vector2(GUI_STAT_MARGIN + GUI_STAT_ICON_SIZE / 2, yPos), scaledIconSize, scaledIconSize, Color.gray, 0.0f);
				}
				apGL.DrawTextGL(Backup.Label, new Vector2(GUI_STAT_MARGIN + GUI_STAT_ICON_SIZE + 10, yPos - 6), 400, labelColor);
			}

			//중요! 21.5.18
			apGL.EndPass();




			//이전에 선택 잠금만 표시하던 영역
			//if (Select.IsSelectionLockGUI)
			//{
			//	apGL.DrawTextureGL(ImageSet.Get(apImageSet.PRESET.GUI_SelectionLock), new Vector2(width - 20, 30), scaledIconSize, scaledIconSize, Color.gray, 0.0f);
			//}

			//변경 21.2.18 : 아이콘들을 통합적으로 관리
			if(_guiStatBox != null)
			{
				_guiStatBox.UpdateAndRender(new Vector2(width - 20, 30), Mouse.Pos);
				apGL.EndPass();
			}

			

			if(_guiHowToUse != null)
			{
				_guiHowToUse.DrawTips(new Vector2(width, height / 2));
				apGL.EndPass();
			}

			


			//통계 정보를 출력한다.
			if (_guiOption_isStatisticsVisible)
			{
				
				//통계 정보는 아래서 출력한다.
				//어떤 메뉴인가에 따라 다르다
				Select.CalculateStatistics();
				if (Select.IsStatisticsCalculated)
				{
					int posY = height - 30;
					if (Select.Statistics_NumKeyframe >= 0)
					{
						apGL.DrawTextGL(apStringFactory.I.Keyframes, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Keyframes"
						apGL.DrawTextGL(Select.Statistics_NumKeyframe.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}
					if (Select.Statistics_NumTimelineLayer >= 0)
					{
						apGL.DrawTextGL(apStringFactory.I.TimelineLayers, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Timeline Layers"
						apGL.DrawTextGL(Select.Statistics_NumTimelineLayer.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}

					//TODO : Bone 개수도 보여주자
					if(Select.Statistics_NumBone > 0)
					{
						apGL.DrawTextGL(apStringFactory.I.Bones, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Bones"
						apGL.DrawTextGL(Select.Statistics_NumBone.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}

					if (Select.Statistics_NumClippedMesh >= 0)
					{
						apGL.DrawTextGL(apStringFactory.I.ClippedVertices, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Clipped Vertices"
						apGL.DrawTextGL(Select.Statistics_NumClippedVertex.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;

						apGL.DrawTextGL(apStringFactory.I.ClippedMeshes, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Clipped Meshes"
						apGL.DrawTextGL(Select.Statistics_NumClippedMesh.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}

					apGL.DrawTextGL(apStringFactory.I.Triangles, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Triangles"
					apGL.DrawTextGL(Select.Statistics_NumTri.ToString(), new Vector2(120, posY), 150, Color.yellow);
					posY -= 15;

					apGL.DrawTextGL(apStringFactory.I.Edges, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Edges"
					apGL.DrawTextGL(Select.Statistics_NumEdge.ToString(), new Vector2(120, posY), 150, Color.yellow);
					posY -= 15;

					apGL.DrawTextGL(apStringFactory.I.Vertices, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Vertices"
					apGL.DrawTextGL(Select.Statistics_NumVertex.ToString(), new Vector2(120, posY), 150, Color.yellow);
					posY -= 15;

					if (Select.Statistics_NumMesh >= 0)
					{
						apGL.DrawTextGL(apStringFactory.I.Meshes, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Meshes"
						apGL.DrawTextGL(Select.Statistics_NumMesh.ToString(), new Vector2(120, posY), 150, Color.yellow);
						posY -= 15;
					}

					


					posY -= 5;
					apGL.DrawTextGL(apStringFactory.I.Statistics, new Vector2(GUI_STAT_MARGIN, posY), 120, Color.yellow);//"Statistics"
				}
			}

			//화면 캡쳐 이벤트
#if UNITY_EDITOR_OSX
			if(_isScreenCaptureRequest_OSXReady)
			{
				_screenCaptureRequest_Count--;
				if(_screenCaptureRequest_Count < 0)
				{
					_isScreenCaptureRequest = true;
					_isScreenCaptureRequest_OSXReady = false;
					_screenCaptureRequest_Count = 0;
				}
			}
#endif
			if (_isScreenCaptureRequest)
			{
				_isScreenCaptureRequest = false;
#if UNITY_EDITOR_OSX
				_isScreenCaptureRequest_OSXReady = false;
				_screenCaptureRequest_Count = 0;
#endif

				ProcessScreenCapture();
				SetRepaint();
			}


			//중요! 21.5.18
			apGL.EndPass();



			
			////물리 설정이 변경되었다면 복구
			//if (_portrait != null)
			//{
			//	_portrait._isPhysicsPlay_Editor = isPrevPhysicsPlay_Editor;
			//}
		}

		




		//---------------------------------------------------------------------------
		// GUI - Right (Header/Body)
		//---------------------------------------------------------------------------
		/// <summary>
		/// 선택된 항목의 분류(타이틀)을 표시한다.
		/// </summary>
		private void GUI_Right1_Header(int width, int height)
		{
			if (_portrait == null)
			{
				return;
			}

			
			_selection.DrawEditor_Header(width - 2, height);

		}


		/// <summary>
		/// 오른쪽 UI 1열 (2분할시 위쪽)
		/// </summary>
		private void GUI_Right1(int width, int height)
		{
			if (_portrait == null)
			{
				return;
			}
			if (!_selection.DrawEditor(width - 2, height))
			{
				if (_portrait != null)
				{
					_selection.SelectPortrait(_portrait);

					//시작은 RootUnit
					_selection.SelectRootUnitDefault();

					OnAnyObjectAddedOrRemoved();
				}
				else
				{
					_selection.Clear();
				}

				SyncHierarchyOrders();

				_hierarchy.ResetAllUnits();
				_hierarchy_MeshGroup.ResetSubUnits();
				_hierarchy_AnimClip.ResetSubUnits();
			}
		}

		

		private enum RIGHT_LOWER_SELECTED_TYPE
		{
			None, MeshTF, MeshGroupTF, Bone
		}


		private void GUI_Right1_Lower_MeshGroupHeader(int width, int height, bool is2LineLayout)
		{
			width -= 2;
			if (_portrait == null)
			{
				return;
			}
			//1. 타이틀 + Show/Hide
			//수정) 타이틀이 한개가 아니라 Meshes / Bones로 나뉘며 버튼이 된다.
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
			GUILayout.Space(5);


			bool isChildMesh = Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
			bool isBones = Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.Bones;

			//int toggleBtnWidth = ((width - (25 + 2)) / 2) - 2;
			int toggleBtnWidth = (width / 2) - 2;


			if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Meshes), isChildMesh, toggleBtnWidth, 20))//"Meshes"
			{
				Select._meshGroupChildHierarchy = apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
			}
			if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Bones), isBones, toggleBtnWidth, 20))//"Bones"
			{
				Select._meshGroupChildHierarchy = apSelection.MESHGROUP_CHILD_HIERARCHY.Bones;
			}


			EditorGUILayout.EndHorizontal();

			//2. 추가, 레이어 변경 버튼들
			if (is2LineLayout)
			{
				int btnSize = Mathf.Min(height - (25 + 4), (width / 5) - 5);
				//현재 레이어에 대한 버튼들을 출력한다.
				//1) 추가
				//2) 클리핑 (On/Off)
				//3, 4) 레이어 Up/Down
				//5) 삭제
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height - (25 + 4)));
				GUILayout.Space(5);

				apTransform_Mesh selectedMeshTF = Select.MeshTF_Main;
				apTransform_MeshGroup selectedMeshGroupTF = Select.MeshGroupTF_Main;
				apBone selectedBone = Select.Bone;

				

				RIGHT_LOWER_SELECTED_TYPE selectedType = RIGHT_LOWER_SELECTED_TYPE.None;
				apMeshGroup curMeshGroup = Select.MeshGroup;
				if (curMeshGroup != null)
				{
					//메인 오브젝트의 종류를 이용해서 UI를 결정한다.
					if (selectedMeshTF != null)				{ selectedType = RIGHT_LOWER_SELECTED_TYPE.MeshTF; }
					else if (selectedMeshGroupTF != null)	{ selectedType = RIGHT_LOWER_SELECTED_TYPE.MeshGroupTF; }
					else if (selectedBone != null)			{ selectedType = RIGHT_LOWER_SELECTED_TYPE.Bone; }
				}

				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Hierarchy_AddTransform), false, true, btnSize, btnSize, apStringFactory.I.AddSubMeshMeshGroup))//"Add Sub Mesh / Mesh Group"
				{
					//1) 추가하기
					//_loadKey_AddChildTransform = apDialog_AddChildTransform.ShowDialog(this, curMeshGroup, OnAddChildTransformDialogResult);
					_loadKey_AddChildTransform = apDialog_SelectMultipleObjects.ShowDialog(	this, 
																				curMeshGroup, 
																				apDialog_SelectMultipleObjects.REQUEST_TARGET.MeshAndMeshGroups, 
																				OnAddMultipleChildTransformDialogResult,
																				GetText(TEXT.Add),
																				curMeshGroup,
																				curMeshGroup);
				}

				bool isClipped = false;
				if (selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshTF)
				{
					isClipped = selectedMeshTF._isClipping_Child;
				}
				if (apEditorUtil.ToggledButton_2Side(	ImageSet.Get(apImageSet.PRESET.Hierarchy_SetClipping),
														isClipped,
														selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshTF,
														btnSize, btnSize,
														apStringFactory.I.SetClippingMask))//"Set Clipping Mask"
				{
					//2) 클리핑
					if (selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshTF)
					{
						if (selectedMeshTF._isClipping_Child)
						{
							//Clip -> Release
							Controller.ReleaseClippingMeshTransform(curMeshGroup, selectedMeshTF);
						}
						else
						{
							//Release -> Clip
							Controller.AddClippingMeshTransform(curMeshGroup, selectedMeshTF, true, true, true);
						}
					}
				}


				bool isRequestDepthChanged = false;
				bool isDepthMoveUp = false;


				//3) Layer Up
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp), false, selectedType != 0, btnSize, btnSize, apStringFactory.I.LayerUp))//"Layer Up"
				{
					isRequestDepthChanged = true;
					isDepthMoveUp = true;
				}

				//4) Layer Down
				if (apEditorUtil.ToggledButton_2Side(ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown), false, selectedType != 0, btnSize, btnSize, apStringFactory.I.LayerDown))//"Layer Down"
				{
					isRequestDepthChanged = true;
					isDepthMoveUp = false;
				}

				//Layer Up/Down 버튼을 눌렀을 때
				if(isRequestDepthChanged)
				{
					int deltaDepth = isDepthMoveUp ? 1 : -1;


					//여러개 편집을 지원한다.
					List<apTransform_Mesh> selectedMeshTFs = Select.GetSubSeletedMeshTFs(false);
					List<apTransform_MeshGroup> selectedMeshGroupTFs = Select.GetSubSeletedMeshGroupTFs(false);

					if (selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshTF
						|| selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshGroupTF)
					{
						apRenderUnit targetRenderUnit = null;
						
						List<apRenderUnit> targetRenderUnits = null;//v1.4.2 : 여러개를 동시에 움직이는 경우도 추가
						int nMultRenderUnits = -1;

						//MeshTF / MeshGroupTF를 여러개 선택한 경우 > 부호 상관없이 동시에 움직일 수 있다.
						//여러개를 동시에 옮기고자 하는 경우
						int nMeshTFs = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
						if(nMeshTFs > 0)
						{
							//여러개의 MeshTF 옮기기
							if (targetRenderUnits == null)
							{
								targetRenderUnits = new List<apRenderUnit>();
							}
							apTransform_Mesh curMeshTF = null;

							for (int iTF = 0; iTF < nMeshTFs; iTF++)
							{
								curMeshTF = selectedMeshTFs[iTF];
								if(curMeshTF == null) { continue; }

								if(curMeshTF._linkedRenderUnit != null)
								{
									targetRenderUnits.Add(curMeshTF._linkedRenderUnit);
								}
							}
						}

						int nMeshGroupTFs = selectedMeshGroupTFs != null  ? selectedMeshGroupTFs.Count : 0;
						if(nMeshGroupTFs > 0)
						{
							//여러개의 MeshGroupTF도 같이 옮기기
							if (targetRenderUnits == null)
							{
								targetRenderUnits = new List<apRenderUnit>();
							}
							apTransform_MeshGroup curMeshGroupTF = null;

							for (int iTF = 0; iTF < nMeshGroupTFs; iTF++)
							{
								curMeshGroupTF = selectedMeshGroupTFs[iTF];
								if(curMeshGroupTF == null) { continue; }

								if(curMeshGroupTF._linkedRenderUnit != null)
								{
									targetRenderUnits.Add(curMeshGroupTF._linkedRenderUnit);
								}
							}
						}

						nMultRenderUnits = targetRenderUnits.Count;



						if (selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshTF)
						{
							targetRenderUnit = curMeshGroup.GetRenderUnit(selectedMeshTF);
							
							
						}
						else if (selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshGroupTF)
						{
							targetRenderUnit = curMeshGroup.GetRenderUnit(selectedMeshGroupTF);

							
						}

						if (targetRenderUnit != null)
						{
							//변경 22.8.19 : 관련된 모든 메시 그룹들이 영향을 받는다. [v1.4.2]
							apEditorUtil.SetRecord_MeshGroup_AllParentAndChildren(
																apUndoGroupData.ACTION.MeshGroup_DepthChanged, 
																this, 
																curMeshGroup, 
																apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
																);

							
							if(nMultRenderUnits > 1)
							{
								//여러개를 동시에 옮기고자 할 때 [v1.4.2]
								curMeshGroup.ChangeMultipleRenderUnitsDepth(targetRenderUnits, deltaDepth);
							}
							else
							{
								//하나만 옮기고자 할 때
								curMeshGroup.ChangeRenderUnitDepth(targetRenderUnit, targetRenderUnit.GetDepth() + deltaDepth);
							}
							

							OnAnyObjectAddedOrRemoved(true);
							RefreshControllerAndHierarchy(false);
						}
					}
					else
					{
						//본도 여러개 이동이 가능하다.
						List<apBone> selectedBones = Select.GetSubSeletedBones(false);
						int nSelectedBones = selectedBones != null ? selectedBones.Count : 0;

						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DepthChanged, 
															this, 
															curMeshGroup, 
															//selectedBone, 
															false, 
															true,
															//apEditorUtil.UNDO_STRUCT.ValueOnly
															apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
															);

						
						if(nSelectedBones > 1)
						{
							//여러개가 선택된 경우
							curMeshGroup.ChangeMultipleBonesDepth(selectedBones, deltaDepth);
						}
						else
						{
							//하나만 선택된 경우
							curMeshGroup.ChangeBoneDepth(selectedBone, selectedBone._depth + deltaDepth);
						}

						
						RefreshControllerAndHierarchy(false);
					}
				}


				if (apEditorUtil.ToggledButton_2Side(	ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform),
														false,
														selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshTF || selectedType == RIGHT_LOWER_SELECTED_TYPE.MeshGroupTF,
														btnSize, btnSize,
														apStringFactory.I.RemoveSubMeshMeshGroup))//"Remove Sub Mesh / Mesh Group"
				{
					//변경 22.8.21 [v1.4.2] : 다중 선택된 경우 처리가 달라져야 한다.
					List<apTransform_Mesh> selectedMeshTFs = Select.GetSubSeletedMeshTFs(false);
					List<apTransform_MeshGroup> selectedMeshGroupTFs = Select.GetSubSeletedMeshGroupTFs(false);
					int nSubSelectedMeshTFs_All = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
					int nSubSelectedMeshGroupTFs_All = selectedMeshGroupTFs != null ? selectedMeshGroupTFs.Count : 0;


					#region [미사용 코드] 이전 방식
					//if(Select.MeshTF_Main != null)
					//{
					//	//Mesh TF를 한개 또는 여러개 삭제할 때

					//	//확인 메시지를 보여주자 (개수에 따라 메시지가 다르다)
					//	string strDialogInfo = "";
					//	if(nSubSelectedMeshTFs_All > 1)
					//	{
					//		// 여러개의 Sub Mesh TF를 삭제
					//		strDialogInfo = Localization.GetText(TEXT.Detach_Body);
					//	}
					//	else
					//	{
					//		// 한개의 Sub Mesh TF를 삭제
					//		strDialogInfo = Controller.GetRemoveItemMessage(
					//										_portrait,
					//										Select.MeshTF_Main,
					//										5,
					//										Localization.GetText(TEXT.Detach_Body),
					//										Localization.GetText(TEXT.DLG_RemoveItemChangedWarning));
					//	}

					//	bool isResult = EditorUtility.DisplayDialog(Localization.GetText(TEXT.Detach_Title),
					//												strDialogInfo,
					//												Localization.GetText(TEXT.Detach_Ok),
					//												Localization.GetText(TEXT.Cancel)
					//												);

					//	if(isResult)
					//	{
					//		if (nSubSelectedMeshTFs_All > 1)
					//		{
					//			// 여러개의 Sub Mesh TF를 삭제
					//			Controller.DetachMeshTransforms(selectedMeshTFs, curMeshGroup);
					//		}
					//		else
					//		{
					//			// 한개의 Sub Mesh TF를 삭제
					//			Controller.DetachMeshTransform(Select.MeshTF_Main, curMeshGroup);
					//		}

					//		// 선택을 해제한다.
					//		Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Inclusive);
					//	}

					//}
					//else if(Select.MeshGroupTF_Main != null)
					//{
					//	//MeshGroup TF를 한개 또는 여러개 삭제할 때

					//	//확인 메시지를 보여주자 (개수에 따라 메시지가 다르다)
					//	string strDialogInfo = "";
					//	if(nSubSelectedMeshGroupTFs_All > 1)
					//	{
					//		// 여러개의 Sub MeshGroup TF를 삭제
					//		strDialogInfo = Localization.GetText(TEXT.Detach_Body);
					//	}
					//	else
					//	{
					//		// 한개의 Sub MeshGroup TF를 삭제
					//		strDialogInfo = Controller.GetRemoveItemMessage(
					//										_portrait,
					//										selectedMeshGroupTF,
					//										5,
					//										Localization.GetText(TEXT.Detach_Body),
					//										Localization.GetText(TEXT.DLG_RemoveItemChangedWarning));
					//	}

					//	bool isResult = EditorUtility.DisplayDialog(Localization.GetText(TEXT.Detach_Title),
					//												strDialogInfo,
					//												Localization.GetText(TEXT.Detach_Ok),
					//												Localization.GetText(TEXT.Cancel)
					//												);

					//	if(isResult)
					//	{
					//		if(nSubSelectedMeshGroupTFs_All > 1)
					//		{
					//			// 여러개의 Sub MeshGroup TF를 삭제
					//			Controller.DetachMeshGroupTransforms(selectedMeshGroupTFs, curMeshGroup);
					//		}
					//		else
					//		{
					//			// 한개의 Sub MeshGroup TF를 삭제
					//			Controller.DetachMeshGroupTransform(Select.MeshGroupTF_Main, curMeshGroup);
					//		}

					//		// 선택을 해제한다.
					//		Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Inclusive);
					//	}
					//} 
					#endregion

					//변경 v1.4.2 : 더 유연하게 삭제하도록 EditorController의 함수를 이용하자
					if (nSubSelectedMeshTFs_All > 0 || nSubSelectedMeshGroupTFs_All > 0)
					{
						//경고 메시지를 보여주자
						string strDialogInfo = GetText(TEXT.Detach_Body);

						strDialogInfo = Controller.GetRemoveItemsMessage(_portrait,
																			selectedMeshTFs, selectedMeshGroupTFs,
																			5,
																			GetText(TEXT.Detach_Body),
																			GetText(TEXT.DLG_RemoveItemChangedWarning));

						bool isResult = EditorUtility.DisplayDialog(GetText(TEXT.Detach_Title),
																		strDialogInfo,
																		GetText(TEXT.Detach_Ok),
																		GetText(TEXT.Cancel)
																		);
						if (isResult)
						{
							//오케이한 경우에만 삭제 시도

							if (nSubSelectedMeshTFs_All == 1
							&& Select.MeshTF_Main != null
							&& nSubSelectedMeshGroupTFs_All == 0)
							{
								//단일 메시 삭제
								Controller.DetachMeshTransform(Select.MeshTF_Main, Select.MeshGroup);
								Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
							}
							else if (nSubSelectedMeshGroupTFs_All == 1
								&& Select.MeshGroupTF_Main != null
								&& nSubSelectedMeshTFs_All == 0)
							{
								//단일 메시 그룹 삭제
								Controller.DetachMeshGroupTransform(Select.MeshGroupTF_Main, Select.MeshGroup);
								Select.SelectMeshGroupTF(null, apSelection.MULTI_SELECT.Main);
							}
							else
							{
								//다중 삭제
								Controller.DetachMultipleTransforms(selectedMeshTFs, selectedMeshGroupTFs, Select.MeshGroup);
								Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}


							curMeshGroup.SetDirtyToSort();//Sort에서 자식 객체 변한것 체크 : Clip 그룹 체크
							curMeshGroup.RefreshForce();
							SetRepaint();
						}
					}
				}
				
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			
			
		}


		private void GUI_Right1_Lower_AnimationHeader(int width, int height)
		{
			width -= 2;
			if (_portrait == null)
			{
				return;
			}

			if (Select.AnimClip == null)
			{
				return;
			}

			//1. 타이틀 + Show/Hide
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
			GUILayout.Space(5);

			//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
			//guiStyle.normal.textColor = Color.white;
			//guiStyle.alignment = TextAnchor.MiddleCenter;

			Color prevColor = GUI.backgroundColor;


			bool isTimelineSelected = false;
			apMeshGroup targetMeshGroup = Select.AnimClip._targetMeshGroup;

			if (Select.AnimTimeline != null)
			{
				isTimelineSelected = true;
			}

			//선택한 타임 라인의 타입에 따라 하단 하이라키가 바뀐다.
			if(_mainRightAnimHeaderTextWrapper == null)
			{
				_mainRightAnimHeaderTextWrapper = new apStringWrapper(64);
			}
			
			//string headerTitle = "";
			_mainRightAnimHeaderTextWrapper.Clear();

			bool isBtnHeader = false;//버튼을 두어서 Header 타입을 보여줄 것인가, Box lable로 보여줄 것인가.

			apSelection.MESHGROUP_CHILD_HIERARCHY childHierarchy = Select._meshGroupChildHierarchy_Anim;
			bool isChildMesh = childHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
			if (targetMeshGroup == null)
			{
				//headerTitle = "(" + GetUIWord(UIWORD.SelectMeshGroup) + ")";
				_mainRightAnimHeaderTextWrapper.Append(apStringFactory.I.Bracket_1_L, false);
				_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.SelectMeshGroup), false);
				_mainRightAnimHeaderTextWrapper.Append(apStringFactory.I.Bracket_1_R, true);
			}
			else
			{
				if (!isTimelineSelected)
				{
					if (childHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
					{
						//headerTitle = "Mesh Group Layers";
						//headerTitle = GetUIWord(UIWORD.MeshGroup) + GetUIWord(UIWORD.Layer);
						_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.MeshGroup), false);
						_mainRightAnimHeaderTextWrapper.AppendSpace(1, false);
						_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.Layer), true);
					}
					else
					{
						//AnimatedModifier인 경우에는 버튼을 두어서 Layer/Bone 처리할 수 있게 하자
						//headerTitle = "Bones";
						//headerTitle = GetUIWord(UIWORD.Bones);
						_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.Bones), true);
					}
					isBtnHeader = true;
				}
				else
				{
					switch (Select.AnimTimeline._linkType)
					{
						case apAnimClip.LINK_TYPE.AnimatedModifier:
							if (childHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
							{
								//headerTitle = "Mesh Group Layers";
								//headerTitle = GetUIWord(UIWORD.MeshGroup) + GetUIWord(UIWORD.Layer);
								_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.MeshGroup), false);
								_mainRightAnimHeaderTextWrapper.AppendSpace(1, false);
								_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.Layer), true);
							}
							else
							{
								//AnimatedModifier인 경우에는 버튼을 두어서 Layer/Bone 처리할 수 있게 하자
								//headerTitle = "Bones";
								//headerTitle = GetUIWord(UIWORD.Bones);
								_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.Bones), true);
							}
							isBtnHeader = true;
							break;


						//case apAnimClip.LINK_TYPE.Bone:
						//	headerTitle = "Bones";
						//	break;

						case apAnimClip.LINK_TYPE.ControlParam:
							//headerTitle = "Control Parameters";
							//headerTitle = GetUIWord(UIWORD.ControlParameters);
							_mainRightAnimHeaderTextWrapper.Append(GetUIWord(UIWORD.ControlParameters), true);
							break;
					}
				}
			}

			if (isBtnHeader)
			{
				//int toggleBtnWidth = ((width - (25 + 2)) / 2) - 2;//이전
				int toggleBtnWidth = (width / 2) - 2;
				if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Meshes), isChildMesh, toggleBtnWidth, 20))//"Meshes"
				{
					Select._meshGroupChildHierarchy_Anim = apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
				}
				if (apEditorUtil.ToggledButton(GetUIWord(UIWORD.Bones), !isChildMesh, toggleBtnWidth, 20))//"Bones"
				{
					Select._meshGroupChildHierarchy_Anim = apSelection.MESHGROUP_CHILD_HIERARCHY.Bones;
				}
			}
			else
			{
				//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
				GUI.backgroundColor = apEditorUtil.ToggleBoxColor_Selected;

				//GUILayout.Box(headerTitle, GUIStyleWrapper.Box_MiddleCenter_WhiteColor, GUILayout.Width(width - (25 + 2)), GUILayout.Height(20));
				GUILayout.Box(_mainRightAnimHeaderTextWrapper.ToString(), GUIStyleWrapper.Box_MiddleCenter_WhiteColor, apGUILOFactory.I.Width(width - (25 + 2)), apGUILOFactory.I.Height(20));

				GUI.backgroundColor = prevColor;
			}


			
			EditorGUILayout.EndHorizontal();

			
		}

		private object _loadKey_AddChildTransform = null;
		private void OnAddChildTransformDialogResult(bool isSuccess, object loadKey, apMesh mesh, apMeshGroup meshGroup)
		{
			if (!isSuccess)
			{
				return;
			}

			if (_loadKey_AddChildTransform != loadKey)
			{
				return;
			}


			_loadKey_AddChildTransform = null;

			if (Select.MeshGroup == null)
			{
				return;
			}
			if (mesh != null)
			{
				apTransform_Mesh addedMeshTransform = Controller.AddMeshToMeshGroup(mesh);
				if (addedMeshTransform != null)
				{
					Select.SelectMeshTF(addedMeshTransform, apSelection.MULTI_SELECT.Main);
					RefreshControllerAndHierarchy(false);
				}

			}
			else if (meshGroup != null)
			{
				apTransform_MeshGroup addedMeshGroupTransform = Controller.AddMeshGroupToMeshGroup(meshGroup, null);
				if (addedMeshGroupTransform != null)
				{
					Select.SelectMeshGroupTF(addedMeshGroupTransform, apSelection.MULTI_SELECT.Main);
					RefreshControllerAndHierarchy(false);
				}
			}
		}

		public void OnAddMultipleChildTransformDialogResult(bool isSuccess, object loadKey, List<object> selectedObjects, object savedObject)
		{
			if(!isSuccess || _loadKey_AddChildTransform == null || _loadKey_AddChildTransform != loadKey || savedObject == null || selectedObjects == null)
			{
				_loadKey_AddChildTransform = null;
				return;
			}

			_loadKey_AddChildTransform = null;

			apMeshGroup savedMeshGroup = null;
			if(savedObject is apMeshGroup)
			{
				savedMeshGroup = savedObject as apMeshGroup;
			}

			if (Select.MeshGroup == null 
				|| savedMeshGroup == null 
				|| Select.MeshGroup != savedMeshGroup 
				|| selectedObjects.Count == 0)
			{
				return;
			}


			apTransform_Mesh finalAdded_MeshTF = null;
			apTransform_MeshGroup finalAdded_MeshGroupTF = null;

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_AttachMesh, 
												this, 
												savedMeshGroup, 
												//savedMeshGroup, 
												false, false,
												apEditorUtil.UNDO_STRUCT.StructChanged);

			//추가 19.8.20 : 경고 메시지를 위한 객체
			apEditorController.AttachMeshGroupError attachMeshGroupError = new apEditorController.AttachMeshGroupError();

			//하나씩 열어봅시다.
			for (int i = 0; i < selectedObjects.Count; i++)
			{
				object curObject = selectedObjects[i];
				if(curObject == null)
				{
					continue;
				}

				if(curObject is apMesh)
				{
					apMesh targetMesh = curObject as apMesh;
					if(targetMesh == null)
					{
						continue;
					}

					apTransform_Mesh addedMeshTransform = Controller.AddMeshToMeshGroup(targetMesh, false);
					if (addedMeshTransform != null)
					{
						finalAdded_MeshTF = addedMeshTransform;
						finalAdded_MeshGroupTF = null;
					}
				}
				else if(curObject is apMeshGroup)
				{
					apMeshGroup targetMeshGroup = curObject as apMeshGroup;
					if(targetMeshGroup == null)
					{
						continue;
					}

					apTransform_MeshGroup addedMeshGroupTransform = Controller.AddMeshGroupToMeshGroup(targetMeshGroup, attachMeshGroupError, false);
					if (addedMeshGroupTransform != null)
					{
						finalAdded_MeshTF = null;
						finalAdded_MeshGroupTF = addedMeshGroupTransform;
					}
				}
			}

			savedMeshGroup.SetDirtyToReset();
			//savedMeshGroup.RefreshForce();//이 코드를 하면 메시의 클리핑 설정들이 갱신되지 않는다.
			savedMeshGroup.RefreshForce(true, 0.0f, apUtil.LinkRefresh.Set_MeshGroup_ExceptAnimModifiers(savedMeshGroup));

			//마지막으로 추가된 것을 선택하자.
			if(finalAdded_MeshTF != null)
			{
				Select.SelectMeshTF(finalAdded_MeshTF, apSelection.MULTI_SELECT.Main);
			}
			else if(finalAdded_MeshGroupTF != null)
			{
				Select.SelectMeshGroupTF(finalAdded_MeshGroupTF, apSelection.MULTI_SELECT.Main);
			}

			//추가 19.8.20 : 추가된 메시 그룹에 애니메이션 모디파이어가 있다면 경고
			if(attachMeshGroupError._isError)
			{
				string strMsg = null;
				if(attachMeshGroupError._nError == 1)
				{
					//strMsg = "The added Mesh Group [" + attachMeshGroupError._meshGroups[0]._name + "] has animation modifiers.\nAnimations associated with this Mesh Group may not work properly.\nSelect those animations and change the target Mesh Group.";
					strMsg = string.Format(GetText(TEXT.DLG_AttachMeshGroupInfo_Single_Body), attachMeshGroupError._meshGroups[0]._name);
				}
				else
				{
					//strMsg = "Added " + attachMeshGroupError._nError + " Mesh Groups have animation modifiers.\nAnimations associated with these Mesh Groups may not work properly.\nSelect those animations and change the target Mesh Group.";
					strMsg = string.Format(GetText(TEXT.DLG_AttachMeshGroupInfo_Multi_Body), attachMeshGroupError._nError);
				}

				EditorUtility.DisplayDialog(GetText(TEXT.DLG_AttachMeshGroupInfo_Title), strMsg, GetText(TEXT.Okay));
			}
			

			//추가 21.1.32 : Rule 가시성 동기화 초기화
			Controller.ResetVisibilityPresetSync();

			//추가 / 삭제시 요청한다.
			OnAnyObjectAddedOrRemoved();
			ResetHierarchyAll();
			RefreshControllerAndHierarchy(false);
			SetRepaint();
		}



		/// <summary>
		/// 오른쪽 1열 UI 중 아래 (Sub Hierarchy가 나온다)
		/// </summary>
		private void GUI_Right1_Lower(int width, int height, Vector2 scroll, bool isGUIEvent)
		{
			if (_portrait == null)
			{
				return;
			}
			if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				GUILayout.Space(5);
				
				//추가 19.8.18
				if(_uiFoldType_Right1_Lower == UI_FOLD_TYPE.Unfolded)
				{
					if(Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
					{
						//메시 탭
						//if (Event.current.type == EventType.Layout)
						//{
						//	SetGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Meshes, true);
						//}

						if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Meshes))
						{
							Hierarchy_MeshGroup.GUI_RenderHierarchy(width,
																	true,//true : 메시 탭
																	scroll,
																	height);
						}
					}
					else
					{
						//본 탭
						//if (Event.current.type == EventType.Layout)
						//{
						//	SetGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Bones, true);
						//}

						if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed__Bones))
						{
							Hierarchy_MeshGroup.GUI_RenderHierarchy(width,
																	false,//false : 본 탭
																	scroll,
																	height);
						}
					}
				}

			}
			else if (Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				GUILayout.Space(5);

				//추가 19.8.18
				if (_uiFoldType_Right1_Lower == UI_FOLD_TYPE.Unfolded)
				{
					if (Select.AnimTimeline == null)
					{
						if (Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
						{
							//Mesh 리스트
							//if (Event.current.type == EventType.Layout)
							//{
							//	SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, true);//"GUI Anim Hierarchy Delayed - Meshes"
							//}

							if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes))//"GUI Anim Hierarchy Delayed - Meshes"
							{
								Hierarchy_AnimClip.GUI_RenderHierarchy_Transform(width, scroll, height);
							}
						}
						else
						{
							//Bone 리스트
							//if (Event.current.type == EventType.Layout)
							//{
							//	SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, true);//"GUI Anim Hierarchy Delayed - Bone"
							//}
							if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone))//"GUI Anim Hierarchy Delayed - Bone"
							{
								Hierarchy_AnimClip.GUI_RenderHierarchy_Bone(width, scroll, height);
							}
						}


					}
					else
					{
						switch (Select.AnimTimeline._linkType)
						{
							case apAnimClip.LINK_TYPE.AnimatedModifier:
								//Hierarchy_AnimClip.GUI_RenderHierarchy_Transform(width, scrollX);
								if (Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
								{
									//Mesh 리스트
									//if (Event.current.type == EventType.Layout)
									//{
									//	SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes, true);//"GUI Anim Hierarchy Delayed - Meshes"
									//}

									if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Meshes))//"GUI Anim Hierarchy Delayed - Meshes"
									{
										Hierarchy_AnimClip.GUI_RenderHierarchy_Transform(width, scroll, height);
									}
								}
								else
								{
									//Bone 리스트
									//if (Event.current.type == EventType.Layout)
									//{
									//	SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone, true);//"GUI Anim Hierarchy Delayed - Bone"
									//}

									if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__Bone))//"GUI Anim Hierarchy Delayed - Bone"
									{
										Hierarchy_AnimClip.GUI_RenderHierarchy_Bone(width, scroll, height);
									}
								}
								break;

							case apAnimClip.LINK_TYPE.ControlParam:
								{
									//if (Event.current.type == EventType.Layout)
									//{
									//	SetGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam, true);//"GUI Anim Hierarchy Delayed - ControlParam"
									//}
									if (IsDelayedGUIVisible(DELAYED_UI_TYPE.GUI_Anim_Hierarchy_Delayed__ControlParam))//"GUI Anim Hierarchy Delayed - ControlParam"
									{
										Hierarchy_AnimClip.GUI_RenderHierarchy_ControlParam(width, scroll, height);
									}
								}

								break;



							default:
								//TODO.. 새로운 타입이 추가될 경우
								break;
						}
					}
				}
			}
		}



		//----------------------------------------------------------------------
		// GUI - Right 2
		//----------------------------------------------------------------------
		private void GUI_Right2(int width, int height)
		{
			if (_portrait == null)
			{
				return;
			}
			Select.DrawEditor_Right2(width - 2, height);
		}






		//----------------------------------------------------------------------
		// GUI - Bottom (편집 버튼들 / 타임라인)
		//----------------------------------------------------------------------
		private void GUI_Bottom_EditButtons(int width, int height)
		{
			GUILayout.Space(10);

			Select.DrawEditor_Bottom_EditButtons(width, height - 12);
		}

		private void GUI_Bottom_Timeline(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			GUILayout.Space(5);

			Select.DrawEditor_Bottom_Timeline(width, height - 8, layoutX, layoutY, windowWidth, windowHeight);
		}



		//--------------------------------------------------------------

		/// <summary>
		/// Scroll Position을 리셋한다.
		/// </summary>
		public void ResetScrollPosition(bool isResetLeft, bool isResetCenter, bool isResetRight, bool isResetRight2, bool isResetBottom)
		{
			if (isResetLeft)
			{
				_scroll_Left_FirstPage = Vector2.zero;
				_scroll_Left_Hierarchy = Vector2.zero;
				_scroll_Left_Controller = Vector2.zero;
			}
			if (isResetCenter)
			{
				_scroll_CenterWorkSpace = Vector2.zero;
			}
			if (isResetRight)
			{
				_scroll_Right1_Upper = Vector2.zero;
				_scroll_Right1_Lower_MG_Mesh = Vector2.zero;
				_scroll_Right1_Lower_MG_Bone = Vector2.zero;
				_scroll_Right1_Lower_Anim_Mesh = Vector2.zero;
				_scroll_Right1_Lower_Anim_Bone = Vector2.zero;
				_scroll_Right1_Lower_Anim_ControlParam = Vector2.zero;
			}

			if(isResetRight2)
			{
				_scroll_Right2 = Vector2.zero;
			}

			//if (isResetBottom)
			//{
			//	_scroll_Bottom = Vector2.zero;
			//}
		}


		public void SetLeftTab(TAB_LEFT leftTabType)
		{	
			if(_tabLeft != leftTabType)
			{
				ResetScrollPosition(true, false, false, false, false);
			}
			_tabLeft = leftTabType;
		}



		//------------------------------------------------------------------------
		// Hierarchy Filter 제어.
		//------------------------------------------------------------------------
		/// <summary>
		/// Hierarchy Filter 제어.
		/// 수동으로 제어하거나, "어떤 객체가 추가"되었다면 Filter가 열린다.
		/// All, None은 isEnabled의 영향을 받지 않는다. (All은 모두 True, None은 모두 False)
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="isEnabled"></param>
		public void SetHierarchyFilter(HIERARCHY_FILTER filter, bool isEnabled)
		{
			HIERARCHY_FILTER prevFilter = _hierarchyFilter;
			bool isRootUnit = ((int)(_hierarchyFilter & HIERARCHY_FILTER.RootUnit) != 0);
			bool isImage = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Image) != 0);
			bool isMesh = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Mesh) != 0);
			bool isMeshGroup = ((int)(_hierarchyFilter & HIERARCHY_FILTER.MeshGroup) != 0);
			bool isAnimation = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Animation) != 0);
			bool isParam = ((int)(_hierarchyFilter & HIERARCHY_FILTER.Param) != 0);

			switch (filter)
			{
				case HIERARCHY_FILTER.All:
					isRootUnit = true;
					isImage = true;
					isMesh = true;
					isMeshGroup = true;
					isAnimation = true;
					isParam = true;
					break;

				case HIERARCHY_FILTER.RootUnit:
					isRootUnit = isEnabled;
					break;


				case HIERARCHY_FILTER.Image:
					isImage = isEnabled;
					break;

				case HIERARCHY_FILTER.Mesh:
					isMesh = isEnabled;
					break;

				case HIERARCHY_FILTER.MeshGroup:
					isMeshGroup = isEnabled;
					break;

				case HIERARCHY_FILTER.Animation:
					isAnimation = isEnabled;
					break;

				case HIERARCHY_FILTER.Param:
					isParam = isEnabled;
					break;

				case HIERARCHY_FILTER.None:
					isRootUnit = false;
					isImage = false;
					isMesh = false;
					isMeshGroup = false;
					isAnimation = false;
					isParam = false;
					break;
			}

			_hierarchyFilter = HIERARCHY_FILTER.None;

			if (isRootUnit)		{ _hierarchyFilter |= HIERARCHY_FILTER.RootUnit; }
			if (isImage)		{ _hierarchyFilter |= HIERARCHY_FILTER.Image; }
			if (isMesh)			{ _hierarchyFilter |= HIERARCHY_FILTER.Mesh; }
			if (isMeshGroup)	{ _hierarchyFilter |= HIERARCHY_FILTER.MeshGroup; }
			if (isAnimation)	{ _hierarchyFilter |= HIERARCHY_FILTER.Animation; }
			if (isParam)		{ _hierarchyFilter |= HIERARCHY_FILTER.Param; }

			if (prevFilter != _hierarchyFilter && _tabLeft == TAB_LEFT.Hierarchy)
			{
				//_scroll_MainLeft = Vector2.zero;
			}

			//이건 설정으로 저장해야한다.
			SaveEditorPref();
		}

		public bool IsHierarchyFilterContain(HIERARCHY_FILTER filter)
		{
			if (filter == HIERARCHY_FILTER.All)
			{
				return _hierarchyFilter == HIERARCHY_FILTER.All;
			}
			return (int)(_hierarchyFilter & filter) != 0;
		}




		//Dialog Event
		//----------------------------------------------------------------------------------
		private void OnDialogEvent_FFDStart(bool isSuccess, object loadKey, int numX, int numY)
		{
			if (_loadKey_FFDStart != loadKey || !isSuccess)
			{
				_loadKey_FFDStart = null;
				return;
			}
			_loadKey_FFDStart = null;

			if(numX < 2)
			{
				numX = 2;
			}
			if(numY < 2)
			{
				numY = 2;
			}
			_curFFDSizeX = numX;
			_curFFDSizeY = numY;

			Gizmos.StartTransformMode(this, _curFFDSizeX, _curFFDSizeY);//원래는 <이거
		}



		//----------------------------------------------------------------------------
		// Inspector를 통해서 여는 경우
		//----------------------------------------------------------------------------
		public void SetPortraitByInspector(apPortrait targetPortrait, bool isBakeAndClose)
		{
			try
			{
				if (_portrait != targetPortrait && targetPortrait != null)
				{
					if (targetPortrait._isOptimizedPortrait)
					{
						//Optimized Portrait는 편집이 불가능하다
						return;
					}

					//v1.4.2 : 상태에 따라서 열지 못하는 Portrait도 존재한다.
					apEditorUtil.CHECK_EDITABLE_RESULT checkResult = apEditorUtil.CheckEditablePortrait(targetPortrait);
					switch(checkResult)
					{
						case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_PrefabAsset:
							{
								//프리팹 에셋인 경우 > 실행 불가
								EditorUtility.DisplayDialog(GetText(TEXT.DLG_NotOpenEditorPrefabAsset_Title),
																GetText(TEXT.DLG_NotOpenEditorPrefabAsset_Body),
																GetText(TEXT.Okay));
							}
							return;

						case apEditorUtil.CHECK_EDITABLE_RESULT.Invalid_PrefabEditScene:
							{
								//프리팹 편집 화면이라면 > 실행 불가
								EditorUtility.DisplayDialog(	GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Title),
																GetText(TEXT.DLG_NotOpenEditorOnPrefabEditingScreen_Body),
																GetText(TEXT.Okay));
							}
							return;
					}

					

					bool isLoadEditResources = CheckEditorResources();

					_portrait = targetPortrait;//선택!

					_selection.SelectPortrait(_portrait);

					//Portrait의 레퍼런스들을 연결해주자
					Controller.PortraitReadyToEdit();//Inspector를 통해서 여는 경우


					SyncHierarchyOrders();

					if (isLoadEditResources)
					{
						_hierarchy.ResetAllUnits();
						_hierarchy_MeshGroup.ResetSubUnits();
						_hierarchy_AnimClip.ResetSubUnits();
					}
					else
					{
						//추가 21.8.3 : 리소스가 성공적으로 로딩되지 않았어도 Order 설정 이후에 try-catch를 한 상태로 강제로 시도
						try
						{
							_hierarchy.ResetAllUnits();
							_hierarchy_MeshGroup.ResetSubUnits();
							_hierarchy_AnimClip.ResetSubUnits();

							s_isEditorResourcesLoaded = false;
						}
						catch (Exception) { }
					}
					
					if(!isLoadEditResources)
					{
						//Debug.Log("리소스 로드 실패");
						//_selection.SelectNone();
						_selection.SelectRootUnitDefault(false);//리소스 로드가 실패했다면, Hierachy Refresh를 하면 안된다. (에러 발생함)
					}
					else
					{
						//시작은 RootUnit
						_selection.SelectRootUnitDefault();
					}

					

					

					OnAnyObjectAddedOrRemoved();

					if (isBakeAndClose)
					{
						apBakeResult bakeResult = Controller.Bake();
						
						if (bakeResult != null)
						{
							if (bakeResult.NumUnlinkedExternalObject > 0)
							{
								EditorUtility.DisplayDialog(GetText(TEXT.BakeWarning_Title),
									GetTextFormat(TEXT.BakeWarning_Body, bakeResult.NumUnlinkedExternalObject),
									GetText(TEXT.Okay));
							}
						}
						

						CloseEditor();
					}
				}
			}
			catch(Exception ex)
			{
				Debug.LogError("AnyPortrait : Open 2D Editor Failed");
				Debug.LogException(ex);
			}
		}


		



	}

}