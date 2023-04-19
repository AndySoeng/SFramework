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
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	// apEditor의 "단축키 이벤트"만 모은 스크립트 소스
	public partial class apEditor : EditorWindow
	{
		//-----------------------------------------------------------------------
		// 왼쪽 UI 탭 전환
		//-----------------------------------------------------------------------
		
		//추가 20.12.4 : 단축키로 탭 전환
		private apHotKey.HotKeyResult OnHotKeyEvent_SwitchLeftTab(object paramObject)
		{
			if(_tabLeft == TAB_LEFT.Controller)
			{
				_tabLeft = TAB_LEFT.Hierarchy;
			}
			else
			{
				_tabLeft = TAB_LEFT.Controller;
			}

			//성공 리턴
			return apHotKey.HotKeyResult.MakeResult();
				
		}


		//-----------------------------------------------------------------------
		// Hierarchy 커서 이동
		//-----------------------------------------------------------------------
		
		/// <summary>
		/// 추가 1.4.2 : Hierarchy의 커서를 방향키를 이용해서 움직일 수 있다.
		/// </summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_MoveHierarchyCursor(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			//활성화된 Hierarchy가 있어야 한다.
			//방향키는 Up, Down만 사용된다.
			//Shift 등이 눌리면 작동하지 않는다.

			if(LastClickedHierarchy == LAST_CLICKED_HIERARCHY.None
				|| (keyCode != KeyCode.UpArrow && keyCode != KeyCode.DownArrow)
				|| isShift || isAlt || isCtrl)
			{
				//단축키 사용 안함
				return null;
			}

			//FFD가 켜진 상태에서도 방향키로 객체를 선택할 수 없다.
			if(Gizmos.IsFFDMode)
			{
				return null;
			}

			bool isMoveUp = keyCode == KeyCode.UpArrow;

			object curObj = null;
			object prevObj = null;
			object nextObj = null;

			switch (LastClickedHierarchy)
			{
				case LAST_CLICKED_HIERARCHY.Main:
					{
						_hierarchy.FindCursor(out curObj, out prevObj, out nextObj, _hierarchyFilter);

						object targetObj = null;
						if(curObj != null)
						{
							//현재 선택된 커서가 있을 때
							if(isMoveUp)	{ targetObj = prevObj; }//위(Prev)로 이동시
							else			{ targetObj = nextObj; }//아래(Next)로 이동시

							//타입에 맞게 이동을 하자
							if(targetObj != null)
							{
								if(targetObj is apRootUnit)				{ Select.SelectRootUnit(targetObj as apRootUnit); }
								else if(targetObj is apTextureData)		{ Select.SelectImage(targetObj as apTextureData); }
								else if(targetObj is apMesh)			{ Select.SelectMesh(targetObj as apMesh); }
								else if(targetObj is apMeshGroup)		{ Select.SelectMeshGroup(targetObj as apMeshGroup); }
								else if(targetObj is apAnimClip)		{ Select.SelectAnimClip(targetObj as apAnimClip); }
								else if(targetObj is apControlParam)	{ Select.SelectControlParam(targetObj as apControlParam); }


								RefreshControllerAndHierarchy(false);

								OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);//Hierarchy 타입 유지

								//스크롤을 자동으로 이동한다.
								AutoScroll_HierarchyMain(targetObj);

								//성공 리턴
								return apHotKey.HotKeyResult.MakeResult();
							}
						}
					}
					break;

				case LAST_CLICKED_HIERARCHY.MeshGroup_TF:
				case LAST_CLICKED_HIERARCHY.MeshGroup_Bone:
					{
						//포커스 Hierarchy와 현재 탭이 맞아야 한다.
						//현재 메뉴가 MeshGroup이어야 한다.
						if(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup)
						{
							break;
						}

						bool isSameTab = false;
						bool isMeshHierarchy = false;
						if(Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes
							&& LastClickedHierarchy == LAST_CLICKED_HIERARCHY.MeshGroup_TF)
						{
							isSameTab = true;
							isMeshHierarchy = true;
						}
						else if(Select._meshGroupChildHierarchy == apSelection.MESHGROUP_CHILD_HIERARCHY.Bones
							&& LastClickedHierarchy == LAST_CLICKED_HIERARCHY.MeshGroup_Bone)
						{
							isSameTab = true;
							isMeshHierarchy = false;
						}

						if(!isSameTab)
						{
							//방향키를 눌렀을 때와 커서의 리스트 타입이 다르다.
							break;
						}

						Hierarchy_MeshGroup.FindCursor(out curObj, out prevObj, out nextObj, isMeshHierarchy);

						if(curObj == null)
						{
							break;
						}

						//모디파이어의 종류에 따라 코드가 조금 다르다.
						bool isRiggingModifier = false;

						if(Select.Modifier != null)
						{
							if(Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
							{
								isRiggingModifier = true;
							}
						}

						object targetObj = null;
						//현재 선택된 커서가 있을 때
						if(isMoveUp)	{ targetObj = prevObj; }//위(Prev)로 이동시
						else			{ targetObj = nextObj; }//아래(Next)로 이동시

						if(targetObj == null)
						{
							break;
						}

						//타입에 맞게 이동을 하자
						bool isProcessed = false;
						if(targetObj is apTransform_Mesh)
						{
							apTransform_Mesh targetMeshTF = targetObj as apTransform_Mesh;
							if(isRiggingModifier)
							{
								Select.SelectMeshTF(targetMeshTF, apSelection.MULTI_SELECT.Main);
							}
							else
							{
								Select.SelectSubObject(targetMeshTF, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}

							isProcessed = true;
						}
						else if(targetObj is apTransform_MeshGroup)
						{
							apTransform_MeshGroup targetMeshGroupTF = targetObj as apTransform_MeshGroup;
							if(isRiggingModifier)
							{
								Select.SelectMeshGroupTF(targetMeshGroupTF, apSelection.MULTI_SELECT.Main);
							}
							else
							{
								Select.SelectSubObject(null, targetMeshGroupTF, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							isProcessed = true;
						}
						else if(targetObj is apBone)
						{
							apBone targetBone = targetObj as apBone;
							if(isRiggingModifier)
							{
								Select.SelectBone(targetBone, apSelection.MULTI_SELECT.Main);
							}
							else
							{
								Select.SelectSubObject(null, null, targetBone, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							isProcessed = true;
						}

						if (isProcessed)
						{
							RefreshControllerAndHierarchy(false);

							OnHierachyClicked(LastClickedHierarchy);//Hierarchy 타입 유지

							//자동으로 스크롤
							AutoScroll_HierarchyMeshGroup(targetObj);

							//성공 리턴
							return apHotKey.HotKeyResult.MakeResult();
						}
						
					}
					break;

				case LAST_CLICKED_HIERARCHY.AnimClip_TF:
				case LAST_CLICKED_HIERARCHY.AnimClip_Bone:
				case LAST_CLICKED_HIERARCHY.AnimClip_ControlParam:
					{
						//현재 보여져야 하는 리스트 종류
						//선택된 타임 라인에 따라 다르다.
						bool isValidList = false;
						
						if (Select.AnimTimeline != null
							&& Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
						{
							//선택된 타임라인이 있고 Control Param일때
							if(LastClickedHierarchy == LAST_CLICKED_HIERARCHY.AnimClip_ControlParam)
							{
								isValidList = true;
							}
						}
						else
						{
							//선택된 타임라인이 없을 경우
							if (Select._meshGroupChildHierarchy_Anim == apSelection.MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
							{
								if(LastClickedHierarchy == LAST_CLICKED_HIERARCHY.AnimClip_TF)
								{
									isValidList = true;
								}
							}
							else
							{
								if(LastClickedHierarchy == LAST_CLICKED_HIERARCHY.AnimClip_Bone)
								{
									isValidList = true;
								}
							}
						}

						if(!isValidList)
						{
							//단축키 입력시와 지금의 리스트가 다르다.
							break;
						}

						switch (LastClickedHierarchy)
						{
							case LAST_CLICKED_HIERARCHY.AnimClip_TF:
								Hierarchy_AnimClip.FindCursor_Transform(out curObj, out prevObj, out nextObj);
								break;

							case LAST_CLICKED_HIERARCHY.AnimClip_Bone:
								Hierarchy_AnimClip.FindCursor_Bone(out curObj, out prevObj, out nextObj);
								break;

							case LAST_CLICKED_HIERARCHY.AnimClip_ControlParam:
								Hierarchy_AnimClip.FindCursor_ControlParam(out curObj, out prevObj, out nextObj);
								break;
						}
						
						if(curObj == null)
						{
							break;
						}

						object targetObj = null;
						//현재 선택된 커서가 있을 때
						if(isMoveUp)	{ targetObj = prevObj; }//위(Prev)로 이동시
						else			{ targetObj = nextObj; }//아래(Next)로 이동시

						if(targetObj == null)
						{
							break;
						}
						//타입에 맞게 이동을 하자
						bool isProcessed = false;
						if(targetObj is apTransform_Mesh)
						{
							apTransform_Mesh targetMeshTF = targetObj as apTransform_Mesh;
							Select.SelectMeshTF_ForAnimEdit(targetMeshTF, true, true, apSelection.MULTI_SELECT.Main);
							isProcessed = true;
						}
						else if(targetObj is apTransform_MeshGroup)
						{
							apTransform_MeshGroup targetMeshGroupTF = targetObj as apTransform_MeshGroup;
							Select.SelectMeshGroupTF_ForAnimEdit(targetMeshGroupTF, true, true, apSelection.MULTI_SELECT.Main);
							isProcessed = true;
						}
						else if(targetObj is apBone)
						{
							apBone targetBone = targetObj as apBone;
							Select.SelectBone_ForAnimEdit(targetBone, true, true, apSelection.MULTI_SELECT.Main);
							isProcessed = true;
						}
						else if(targetObj is apControlParam)
						{
							apControlParam targetCP = targetObj as apControlParam;
							Select.SelectControlParam_ForAnimEdit(targetCP, true, true);
							isProcessed = true;
						}

						if (isProcessed)
						{
							RefreshControllerAndHierarchy(true);

							OnHierachyClicked(LastClickedHierarchy);//Hierarchy 타입 유지

							//자동으로 스크롤
							AutoScroll_HierarchyAnimation(targetObj);

							//성공 리턴
							return apHotKey.HotKeyResult.MakeResult();
						}
					}
					break;
			}

			//이 단축키는 처리되지 않았다.
			return null;

		}


		//-----------------------------------------------------------------------
		// 본/메시/물리 보기 여부 전환
		//-----------------------------------------------------------------------
		
		/// <summary>
		/// 단축키 B를 눌러서 Bone Visible을 바꾼다.
		/// </summary>
		/// <param name="paramObject"></param>
		private apHotKey.HotKeyResult OnHotKeyEvent_BoneVisibleToggle(object paramObject)
		{
			switch (_boneGUIRenderMode)
			{
				case BONE_RENDER_MODE.None: _boneGUIRenderMode = BONE_RENDER_MODE.Render; break;
				case BONE_RENDER_MODE.Render: _boneGUIRenderMode = BONE_RENDER_MODE.RenderOutline; break;
				case BONE_RENDER_MODE.RenderOutline: _boneGUIRenderMode = BONE_RENDER_MODE.None; break;
			}

			SaveEditorPref();

			//본 보여주기 상태를 전달하자
			switch (_boneGUIRenderMode)
			{
				case BONE_RENDER_MODE.None: return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.Hide);
				case BONE_RENDER_MODE.Render: return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.Show);
				case BONE_RENDER_MODE.RenderOutline: return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.Outline);
			}
			return null;
		}

		
		/// <summary>추가 21.1.21 : 메시의 보이기 여부 단축키</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_MeshVisibleToggle(object paramObject)
		{
			if(_meshGUIRenderMode == MESH_RENDER_MODE.Render)	{ _meshGUIRenderMode = MESH_RENDER_MODE.None; }
			else												{ _meshGUIRenderMode = MESH_RENDER_MODE.Render; }

			return apHotKey.HotKeyResult.MakeResult(_meshGUIRenderMode == MESH_RENDER_MODE.None ? apStringFactory.I.Hide : apStringFactory.I.Show);
		}


		/// <summary>물리의 미리보기 여부 단축키</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_PhysicsToggle(object paramObject)
		{
			if(_portrait != null)
			{
				_portrait._isPhysicsPlay_Editor = !_portrait._isPhysicsPlay_Editor;

				return apHotKey.HotKeyResult.MakeResult(_portrait._isPhysicsPlay_Editor ? apStringFactory.I.ON : apStringFactory.I.OFF);
			}

			return null;
		}


		//-----------------------------------------------------------------------
		// 작업 공간의 크기/색상
		//-----------------------------------------------------------------------
		
		/// <summary>단축키 Alt+W를 눌러서 FullScreen 모드를 바꾼다.</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_FullScreenToggle(object paramObject)
		{
			_isFullScreenGUI = !_isFullScreenGUI;

			return apHotKey.HotKeyResult.MakeResult(_isFullScreenGUI ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}


		/// <summary>배경색을 반전하는 단축키</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_InvertBackgroundColor(object paramObject)
		{
			_isInvertBackgroundColor = !_isInvertBackgroundColor;

			return apHotKey.HotKeyResult.MakeResult(_isInvertBackgroundColor ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}


		//-----------------------------------------------------------------------
		// 블러 브러시 크기 조절
		//-----------------------------------------------------------------------

		/// <summary>블러 브러시의 크기를 증가시키는 단축키</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_IncBlurBrushRadius(object paramObject)
		{
			//이전 : 직접 크기 조절
			//_blurRadius = Mathf.Clamp(_blurRadius + 10, 1, apGizmos.MAX_BRUSH_RADIUS);

			//변경 22.1.9 : 프리셋 인덱스로 크기 조절
			_blurRadiusIndex += 1;
			if(_blurRadiusIndex > apGizmos.MAX_BRUSH_INDEX)
			{
				_blurRadiusIndex = apGizmos.MAX_BRUSH_INDEX;
			}	
			return apHotKey.HotKeyResult.MakeResult();
		}



		/// <summary>블러 브러시의 크기를 감소시키는 단축키</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_DecBlurBrushRadius(object paramObject)
		{
			//이전 : 직접 크기 조절
			//_blurRadius = Mathf.Clamp(_blurRadius - 10, 1, apGizmos.MAX_BRUSH_RADIUS);

			//변경 22.1.9 : 프리셋 인덱스로 크기 조절
			_blurRadiusIndex -= 1;
			if(_blurRadiusIndex < 0)
			{
				_blurRadiusIndex = 0;
			}
			return apHotKey.HotKeyResult.MakeResult();
		}

		
		
		//-----------------------------------------------------------------------
		// 보기 프리셋 (Visibility Preset)
		//-----------------------------------------------------------------------
		
		/// <summary>보기 프리셋 켜기/끄기 단축키</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_ToggleVisiblityPreset(object paramObject)
		{
			bool prevIsVP = _isAdaptVisibilityPreset;
			_isAdaptVisibilityPreset = !_isAdaptVisibilityPreset;

			int nRules = 0;
			if (_portrait != null && _portrait.VisiblePreset != null)
			{
				nRules = _portrait.VisiblePreset._rules != null ? _portrait.VisiblePreset._rules.Count : 0;
			}

			if (nRules == 0)
			{
				//규칙이 없다면 항상 비활성
				_isAdaptVisibilityPreset = false;
			}
			//규칙을 선택한게 없다면
			if (_selectedVisibilityPresetRule == null)
			{
				if (nRules > 0)
				{
					//첫번째것을 선택한다.
					_selectedVisibilityPresetRule = _portrait.VisiblePreset._rules[0];
				}
			}
			if(_selectedVisibilityPresetRule == null)
			{
				//선택한 규칙이 없다면
				_isAdaptVisibilityPreset = false;
			}


			if(prevIsVP != _isAdaptVisibilityPreset)
			{
				//변경이 된 경우에
				if(_isAdaptVisibilityPreset)
				{
					//규칙 켜짐
					return apHotKey.HotKeyResult.MakeResult(_selectedVisibilityPresetRule._name);
				}
				else
				{
					//설정 꺼짐
					return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.OFF);
				}
				
			}
			return null;

			
		}

		/// <summary>보기 프리셋 규칙을 변경하기</summary>
		/// <param name="paramObject">규칙 단축키</param>
		private apHotKey.HotKeyResult OnHotKeyEvent_SelectVisibilitPresetRule(object paramObject)
		{
			//단축키로 보기 규칙을 변경할 수 있다.
			
			if(paramObject != null)
			{
				apVisibilityPresets.HOTKEY hotKey = (apVisibilityPresets.HOTKEY)paramObject;
				if(_portrait != null && _portrait.VisiblePreset != null && hotKey != apVisibilityPresets.HOTKEY.None)
				{
					_selectedVisibilityPresetRule = _portrait.VisiblePreset.GetRuleByHotkey(hotKey);
					if(!_isAdaptVisibilityPreset && _selectedVisibilityPresetRule != null)
					{
						//Rule 바꿀때 자동으로 프리셋 활성
						_isAdaptVisibilityPreset = true;
					}

					if(_selectedVisibilityPresetRule != null)
					{
						//켜졌을 때
						return apHotKey.HotKeyResult.MakeResult(_selectedVisibilityPresetRule._name);
					}
				}
			}
			return null;
		}


		//-----------------------------------------------------------------------
		// 로토스코핑
		//-----------------------------------------------------------------------

		/// <summary>로토스코핑 켜기/끄기 단축키</summary>
		private apHotKey.HotKeyResult OnHotKey_ToggleRotoscoping(object paramObject)
		{
			int nData = Rotoscoping._imageSetDataList != null ? Rotoscoping._imageSetDataList.Count : 0;

			bool prevRoto = _isEnableRotoscoping;
			_isEnableRotoscoping = !_isEnableRotoscoping;

			
			if(_isEnableRotoscoping && nData == 0)
			{
				_isEnableRotoscoping = false;
			}


			if(_isEnableRotoscoping && _selectedRotoscopingData == null)
			{	
				if(nData > 0)
				{
					_selectedRotoscopingData = Rotoscoping._imageSetDataList[0];
					_selectedRotoscopingData.LoadImages();//이미지들을 열자
					_iRotoscopingImageFile = 0;
				}
				else
				{
					_isEnableRotoscoping = false;
				}
			}

			_iSyncRotoscopingAnimClipFrame = -1;
			_isSyncRotoscopingToAnimClipFrame = false;
			

			//변경이 된 경우에
			if(prevRoto != _isEnableRotoscoping)
			{
				if(_isEnableRotoscoping)
				{
					return apHotKey.HotKeyResult.MakeResult(_selectedRotoscopingData._name);
				}
				else
				{	
					Rotoscoping.DestroyAllImages();//끝때는 이미지를 모두 삭제하자
					return apHotKey.HotKeyResult.MakeResult(apStringFactory.I.OFF);
				}
			}
			return null;
			
		}

		/// <summary>로토스코핑 이미지 전환하기</summary>
		private apHotKey.HotKeyResult OnHotKey_RotoscopingSwitchingImage(object paramObject)
		{
			if(paramObject != null && _isEnableRotoscoping && _selectedRotoscopingData != null)
			{
				bool isNextImage = (bool)paramObject;
				if (!isNextImage)
				{
					//이전 이미지
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

					return apHotKey.HotKeyResult.MakeResult();
				}
				else
				{
					//다음 이미지
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

					return apHotKey.HotKeyResult.MakeResult();
				}
			}
			return null;
			
		}


		//-----------------------------------------------------------------------
		// 작업 공간 가이드라인 
		//-----------------------------------------------------------------------
		
		/// <summary>작업 공간에서 가이드라인 켜기/끄기</summary>
		private apHotKey.HotKeyResult OnHotKeyEvent_ToggleGuidelines(object paramObject)
		{
			_isEnableGuideLine = !_isEnableGuideLine;
			return apHotKey.HotKeyResult.MakeResult(_isEnableGuideLine ? apStringFactory.I.ON : apStringFactory.I.OFF);
			
		}



		//-----------------------------------------------------------------------
		// 편집 모드
		//-----------------------------------------------------------------------
		private apHotKey.HotKeyResult OnHotKeyEvent_ExModOptions(object paramObject)
		{
			if(Select == null || 
				(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup && Select.SelectionType != apSelection.SELECTION_TYPE.Animation)
				)
			{
				return null;
			}

			if(paramObject == null
				|| !(paramObject is int))
			{
				return null;
			}
			int iOption = (int)paramObject;
			bool isOptionResult = false;

			if(iOption == 0)
			{
				//ExObj_UpdateByOtherMod
				_exModObjOption_UpdateByOtherMod = !_exModObjOption_UpdateByOtherMod;
				isOptionResult = _exModObjOption_UpdateByOtherMod;

				//FFD 모드는 취소한다.
				if(Gizmos.IsFFDMode)
				{
					Gizmos.RevertFFDTransformForce();
				}
			}
			else if(iOption == 1)
			{
				//ExObj_ShowAsGray
				_exModObjOption_ShowGray = !_exModObjOption_ShowGray;
				isOptionResult = _exModObjOption_ShowGray;
			}
			else if(iOption == 2)
			{
				//ExObj_ToggleSelectionSemiLock
				_exModObjOption_NotSelectable = !_exModObjOption_NotSelectable;
				isOptionResult = _exModObjOption_NotSelectable;
			}
			else
			{
				//에잉 모르겠다.
				return null;
			}
			
			SaveEditorPref();
			
			//이전
			//Select.RefreshModifierExclusiveEditing();
			//if(Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			//{
			//	Select.RefreshAnimEditingLayerLock();
			//}

			//변경 22.5.14
			Select.AutoRefreshModifierExclusiveEditing();
			

			return apHotKey.HotKeyResult.MakeResult(isOptionResult ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}


		//추가 21.2.13 : 계산된 본 미리보기 단축키
		private apHotKey.HotKeyResult OnHotKeyEvent_ShowCalculatedBones(object paramObject)
		{
			if(Select == null || 
				(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup && Select.SelectionType != apSelection.SELECTION_TYPE.Animation))
			{
				return null;
			}

			_modLockOption_BoneResultPreview = !_modLockOption_BoneResultPreview;
			SaveEditorPref();
			
			//이전
			//Select.RefreshModifierExclusiveEditing();
			//Select.RefreshAnimEditingLayerLock();

			//변경 22.5.14
			Select.AutoRefreshModifierExclusiveEditing();

			

			return apHotKey.HotKeyResult.MakeResult(_modLockOption_BoneResultPreview ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}

		//추가 21.2.13 : 계산된 색상 미리보기 단축키
		private apHotKey.HotKeyResult OnHotKeyEvent_ShowCalculatedColor(object paramObject)
		{
			if(Select == null || 
				(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup && Select.SelectionType != apSelection.SELECTION_TYPE.Animation))
			{
				return null;
			}

			_modLockOption_ColorPreview = !_modLockOption_ColorPreview;
			SaveEditorPref();
			
			//이전
			//Select.RefreshModifierExclusiveEditing();
			//Select.RefreshAnimEditingLayerLock();

			Select.AutoRefreshModifierExclusiveEditing();//변경 22.5.14
			
			
			

			return apHotKey.HotKeyResult.MakeResult(_modLockOption_ColorPreview ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}

		//추가 21.2.13 : 모디파이어 UI 출력하기
		private apHotKey.HotKeyResult OnHotKeyEvent_ShowModifierListUI(object paramObject)
		{
			if(Select == null || 
				(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup && Select.SelectionType != apSelection.SELECTION_TYPE.Animation))
			{
				return null;
			}

			_modLockOption_ModListUI = !_modLockOption_ModListUI;
			SaveEditorPref();
			return apHotKey.HotKeyResult.MakeResult(_modLockOption_ModListUI ? apStringFactory.I.ON : apStringFactory.I.OFF);
		}



		//-----------------------------------------------------------------------
		// 모핑 대상 전환 (Pin <-> Vertex)
		//-----------------------------------------------------------------------
		private apHotKey.HotKeyResult OnHotKeyEvent_ToggleMorphTarget(object paramObject)
		{
			if (Select == null ||
				(Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup && Select.SelectionType != apSelection.SELECTION_TYPE.Animation))
			{
				return null;
			}

			//해당하는 모디파이어를 찾자
			apModifierBase curModifier = null;
			if (Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				curModifier = Select.Modifier;
				if(curModifier == null)
				{
					return null;
				}
				if(curModifier.ModifierType != apModifierBase.MODIFIER_TYPE.Morph)
				{
					return null;
				}
			}
			else
			{
				if (Select.AnimTimeline != null)
				{
					curModifier = Select.AnimTimeline._linkedModifier;
				}

				if(curModifier == null)
				{
					return null;
				}
				if(curModifier.ModifierType != apModifierBase.MODIFIER_TYPE.AnimatedMorph)
				{
					return null;
				}
			}

			//모달 상태를 체크해야한다.
			bool isExecutable = CheckModalAndExecutable();

			if(!isExecutable)
			{
				//모달 상태가 해제되지 않아서 편집 대상을 변경할 수 없다.
				return null;
			}
			
			if (Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				Select.SetMorphEditTarget(apSelection.MORPH_EDIT_TARGET.Pin);
			}
			else
			{
				Select.SetMorphEditTarget(apSelection.MORPH_EDIT_TARGET.Vertex);
			}

			return apHotKey.HotKeyResult.MakeResult(Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex ? apStringFactory.I.MorphTarget_Vertex : apStringFactory.I.MorphTarget_Pin);

		}
	}
}