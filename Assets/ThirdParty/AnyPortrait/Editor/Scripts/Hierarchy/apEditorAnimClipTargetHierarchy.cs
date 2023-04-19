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

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;


namespace AnyPortrait
{

	public class apEditorAnimClipTargetHierarchy
	{
		// Members
		//----------------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }


		public enum HIERARCHY_TYPE
		{
			Transform,
			Bone,
			ControlParam
		}

		public enum CATEGORY
		{
			MainName,
			Mesh_Item,
			MeshGroup_Item,
			Bone_Item,
			SubName_Bone,
			ControlParam
		}


		//private HIERARCHY_TYPE _hierarchyType = HIERARCHY_TYPE.Transform;

		//루트들만 따로 적용
		//private apEditorHierarchyUnit _rootUnit_Mesh = null;
		//private apEditorHierarchyUnit _rootUnit_MeshGroup = null;

		private apEditorHierarchyUnit _rootUnit_Transform = null;
		private apEditorHierarchyUnit _rootUnit_Bone_Main = null;
		private List<apEditorHierarchyUnit> _rootUnit_Bones_Sub = new List<apEditorHierarchyUnit>();//<<추가
		private apEditorHierarchyUnit _rootUnit_ControlParam = null;

		public List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Transform = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Bone = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_ControlParam = new List<apEditorHierarchyUnit>();

		private Dictionary<object, apEditorHierarchyUnit> _object2Unit_Transform = new Dictionary<object, apEditorHierarchyUnit>();
		private Dictionary<object, apEditorHierarchyUnit> _object2Unit_Bone = new Dictionary<object, apEditorHierarchyUnit>();
		private Dictionary<object, apEditorHierarchyUnit> _object2Unit_ControlParam = new Dictionary<object, apEditorHierarchyUnit>();

		//개선 20.3.28 : Pool을 이용하자
		private apEditorHierarchyUnitPool _unitPool = new apEditorHierarchyUnitPool();


		//추가 20.7.4 : 다중 선택 위해서 마지막으로 선택된 Unit을 기록하자
		private apEditorHierarchyUnit _lastClickedUnit = null;//마지막으로 클릭한 유닛 (클릭한 경우만 유효)
		private List<apEditorHierarchyUnit> _shiftClickedUnits = new List<apEditorHierarchyUnit>();//Shift로 클릭했을때 결과로 나올 추가 선택 유닛들



		//추가 21.6.12 : 우클릭에 대한 메뉴 멤버 추가
		private apEditorHierarchyMenu _rightMenu = null;
		private apMeshGroup _requestMeshGroup = null;
		private object _loadKey_Search = null;



		//Visible Icon에 대한 GUIContent
		private apGUIContentWrapper _guiContent_Visible_Current = null;
		private apGUIContentWrapper _guiContent_NonVisible_Current = null;

		private apGUIContentWrapper _guiContent_Visible_TmpWork = null;
		private apGUIContentWrapper _guiContent_NonVisible_TmpWork = null;

		private apGUIContentWrapper _guiContent_Visible_Default = null;
		private apGUIContentWrapper _guiContent_NonVisible_Default = null;

		private apGUIContentWrapper _guiContent_Visible_ModKey = null;
		private apGUIContentWrapper _guiContent_NonVisible_ModKey = null;

		private apGUIContentWrapper _guiContent_Visible_Rule = null;
		private apGUIContentWrapper _guiContent_NonVisible_Rule = null;

		private apGUIContentWrapper _guiContent_NoKey = null;
		private apGUIContentWrapper _guiContent_NoKeyDisabled = null;

		//추가 19.11.16
		private apGUIContentWrapper _guiContent_FoldDown = null;
		private apGUIContentWrapper _guiContent_FoldRight = null;
		private apGUIContentWrapper _guiContent_ModRegisted = null;
		
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_ON = null;
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_OFF = null;
		private int _curUnitPosY = 0;

		// Init
		//------------------------------------------------------------------------
		public apEditorAnimClipTargetHierarchy(apEditor editor)
		{
			_editor = editor;


			if(_rootUnit_Bones_Sub == null) { _rootUnit_Bones_Sub = new List<apEditorHierarchyUnit>(); }
			_rootUnit_Bones_Sub.Clear();

			if (_units_All == null)					{ _units_All = new List<apEditorHierarchyUnit>(); }
			if (_units_Root_Transform == null)		{ _units_Root_Transform = new List<apEditorHierarchyUnit>(); }
			if (_units_Root_Bone == null)			{ _units_Root_Bone = new List<apEditorHierarchyUnit>(); }
			if (_units_Root_ControlParam == null)	{ _units_Root_ControlParam = new List<apEditorHierarchyUnit>(); }
			_units_All.Clear();
			_units_Root_Transform.Clear();
			_units_Root_Bone.Clear();
			_units_Root_ControlParam.Clear();

			if (_object2Unit_Transform == null)		{ _object2Unit_Transform = new Dictionary<object, apEditorHierarchyUnit>(); }
			if (_object2Unit_Bone == null)			{ _object2Unit_Bone = new Dictionary<object, apEditorHierarchyUnit>(); }
			if (_object2Unit_ControlParam == null)	{ _object2Unit_ControlParam = new Dictionary<object, apEditorHierarchyUnit>(); }
			_object2Unit_Transform.Clear();
			_object2Unit_Bone.Clear();
			_object2Unit_ControlParam.Clear();


			if(_unitPool == null)
			{
				_unitPool = new apEditorHierarchyUnitPool();
			}


			_rightMenu = new apEditorHierarchyMenu(_editor, OnSelectRightMenu);

		}


		private void ReloadGUIContent()
		{
			if (_editor == null)
			{
				return;
			}
			if (_guiContent_Visible_Current == null)	{ _guiContent_Visible_Current =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current)); }
			if (_guiContent_NonVisible_Current == null) { _guiContent_NonVisible_Current =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current)); }
			if (_guiContent_Visible_TmpWork == null)	{ _guiContent_Visible_TmpWork =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_TmpWork)); }
			if (_guiContent_NonVisible_TmpWork == null) { _guiContent_NonVisible_TmpWork =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_TmpWork)); }
			if (_guiContent_Visible_Default == null)	{ _guiContent_Visible_Default =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Default)); }
			if (_guiContent_NonVisible_Default == null) { _guiContent_NonVisible_Default =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Default)); }
			if (_guiContent_Visible_ModKey == null)		{ _guiContent_Visible_ModKey =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_ModKey)); }
			if (_guiContent_NonVisible_ModKey == null)	{ _guiContent_NonVisible_ModKey =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_ModKey)); }
			if (_guiContent_Visible_Rule == null)		{ _guiContent_Visible_Rule =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Rule)); }
			if (_guiContent_NonVisible_Rule == null)	{ _guiContent_NonVisible_Rule =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Rule)); }
			if (_guiContent_NoKey == null)				{ _guiContent_NoKey =				apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NoKey)); }
			if (_guiContent_NoKeyDisabled == null)		{ _guiContent_NoKeyDisabled =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NoKeyDisabled)); }

			
			//GUIContent 추가
			if (_guiContent_FoldDown == null)			{ _guiContent_FoldDown =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)); }
			if (_guiContent_FoldRight == null)			{ _guiContent_FoldRight =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight)); }
			if (_guiContent_ModRegisted == null)		{ _guiContent_ModRegisted =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered)); }

			if (_guiContent_RestoreTmpWorkVisible_ON == null)		{ _guiContent_RestoreTmpWorkVisible_ON =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON)); }
			if (_guiContent_RestoreTmpWorkVisible_OFF == null)		{ _guiContent_RestoreTmpWorkVisible_OFF =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF)); }
		}

		public void ResetSubUnits()
		{
			_units_All.Clear();
			_units_Root_Transform.Clear();
			_units_Root_Bone.Clear();
			_units_Root_ControlParam.Clear();

			_rootUnit_Transform = null;
			_rootUnit_Bone_Main = null;
			_rootUnit_Bones_Sub.Clear();

			_rootUnit_ControlParam = null;

			_object2Unit_Transform.Clear();
			_object2Unit_Bone.Clear();
			_object2Unit_ControlParam.Clear();


			//추가 20.7.4 : 다중 선택용 변수
			_lastClickedUnit = null;



			if (Editor == null || Editor._portrait == null || Editor.Select.AnimClip == null)
			{
				return;
			}

			ReloadGUIContent();//<<추가

			//Pool 초기화
			if(_unitPool.IsInitialized)
			{
				_unitPool.PushAll();
			}
			else
			{
				_unitPool.Init();
			}


			//1. Linked MeshGroup의 Child List부터 만들어주자
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup == null)
			{
				//MeshGroup이 연결이 안되었네용
				_rootUnit_Transform = AddUnit_Label(null, "[No MeshGroup]", CATEGORY.MainName, null, null);
			}
			else
			{
				//MeshGroup에 연결해서 세팅
				string meshGroupName = targetMeshGroup._name;
				if (meshGroupName.Length > 16)
				{
					meshGroupName = meshGroupName.Substring(0, 14) + "..";
				}
				_rootUnit_Transform = AddUnit_Label(null, meshGroupName, CATEGORY.MainName, null, null);

				//추가 19.6.29 : RestoreTmpWorkVisible 버튼을 추가하자.
				//_rootUnit_Transform.SetRestoreTmpWorkVisible(	Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON), 
				//												Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF),
				//												OnUnitClickRestoreTmpWork_Mesh);

				//변경 19.11.16 : GUIContent 이용
				_rootUnit_Transform.SetRestoreTmpWorkVisible(	_guiContent_RestoreTmpWorkVisible_ON, 
																_guiContent_RestoreTmpWorkVisible_OFF,
																OnUnitClickRestoreTmpWork_Mesh);


				_rootUnit_Transform.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Meshes);



				AddTransformOfMeshGroup(targetMeshGroup, _rootUnit_Transform);
			}

			//2. Bone List를 만들자
			if (targetMeshGroup == null)
			{
				_rootUnit_Bone_Main = AddUnit_Label(null, "[No MeshGroup]", CATEGORY.MainName, null, null);
			}
			else
			{
				//"Bones"
				_rootUnit_Bone_Main = AddUnit_Label(null, _editor.GetUIWord(UIWORD.Bones), CATEGORY.MainName, null, null);


				//추가 19.6.29 : RestoreTmpWorkVisible 버튼을 추가하자.
				//_rootUnit_Bone_Main.SetRestoreTmpWorkVisible(	Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON), 
				//												Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF),
				//												OnUnitClickRestoreTmpWork_Bone);

				//추가 19.11.16 : GUIContent 이용
				_rootUnit_Bone_Main.SetRestoreTmpWorkVisible(	_guiContent_RestoreTmpWorkVisible_ON, 
																_guiContent_RestoreTmpWorkVisible_OFF,
																OnUnitClickRestoreTmpWork_Bone);

				_rootUnit_Bone_Main.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Bones);

				//Root Bone에 대해서
				AddBoneUnitsOfMeshGroup(targetMeshGroup, _rootUnit_Bone_Main);


				//추가:하위 MeshGroup의 Bone들은 별도로 묶는다. Bone Set 이용
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
				{
					boneSet = targetMeshGroup._boneListSets[iSet];
					if(boneSet._isRootMeshGroup)
					{
						//Root MeshGroup의 Bone이면 패스
						continue;
					}

					apEditorHierarchyUnit subRootUnit = AddUnit_Label(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px),//변경 22.6.10 : 16px로 변경
										boneSet._meshGroupTransform._nickName,
										CATEGORY.SubName_Bone,
										boneSet._meshGroupTransform, //<Saved Obj
										null);

					_rootUnit_Bones_Sub.Add(subRootUnit);

					//여기서 추가
					AddBoneUnitsOfMeshGroup(boneSet._meshGroup, subRootUnit);
				}



				//_units_Root_Bone.Add(addedUnit);
				//AddBoneUnitsOfMeshGroup(targetMeshGroup, _rootUnit_Bone);
			}


			//3. Control Param 리스트를 만들자
			//"Control Parameters"
			_rootUnit_ControlParam = AddUnit_Label(null, _editor.GetUIWord(UIWORD.ControlParameters), CATEGORY.MainName, null, null);

			//Texture2D iconImage_Control = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);

			List<apControlParam> controlParams = Editor._portrait._controller._controlParams;
			for (int i = 0; i < controlParams.Count; i++)
			{
				apControlParam curParam = controlParams[i];

				AddUnit_ToggleButton(
												//iconImage_Control,
												Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(curParam._iconPreset)),
												curParam._keyName,
												CATEGORY.ControlParam,
												curParam,
												IsModRegistered(curParam),
												_rootUnit_ControlParam,
												RIGHTCLICK_MENU.Enabled,
												HIERARCHY_TYPE.ControlParam);
			}

			_units_Root_Transform.Add(_rootUnit_Transform);
			_units_Root_Bone.Add(_rootUnit_Bone_Main);
			//추가 9.28
			for (int i = 0; i < _rootUnit_Bones_Sub.Count; i++)
			{
				//Sub 추가
				_units_Root_Bone.Add(_rootUnit_Bones_Sub[i]);
			}
			_units_Root_ControlParam.Add(_rootUnit_ControlParam);

			//추가 20.7.4 : 다중 선택을 위한 LinearIndex를 갱신하자
			RefreshLinearIndices();
		}



		//추가 21.8.7 : 리소스를 다시 로딩할 경우 유닛 풀의 모든 유닛들의 리소스도 다시 로드해야한다.
		public void ReloadUnitResources()
		{
			_unitPool.CheckAndReInitUnit();
		}



		private void AddTransformOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			for (int i = 0; i < childMeshTransforms.Count; i++)
			{
				apTransform_Mesh meshTransform = childMeshTransforms[i];
				Texture2D iconImage = null;
				if (meshTransform._isClipping_Child)
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
				}
				else
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh_16px);//변경 22.6.10 : 16px로 변경
				}

				bool isModRegistered = IsModRegistered(meshTransform);

				AddUnit_ToggleButton_Visible(	iconImage,
												meshTransform._nickName,
												CATEGORY.Mesh_Item,
												meshTransform,
												false,
												parentUnit,
												GetVisibleIconType(meshTransform, isModRegistered, true),
												GetVisibleIconType(meshTransform, isModRegistered, false),
												isModRegistered,
												true,
												//_rootUnit_Mesh,
												RIGHTCLICK_MENU.Enabled,
												HIERARCHY_TYPE.Transform
												);


			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				bool isModRegistered = IsModRegistered(meshGroupTransform);

				apEditorHierarchyUnit newUnit = AddUnit_ToggleButton_Visible(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px),//변경 22.6.10 : 16px로 변경
													meshGroupTransform._nickName,
													CATEGORY.MeshGroup_Item,
													meshGroupTransform,
													false,
													parentUnit,
													GetVisibleIconType(meshGroupTransform, isModRegistered, true),
													GetVisibleIconType(meshGroupTransform, isModRegistered, false),
													isModRegistered,
													true,
													//_rootUnit_MeshGroup,
													RIGHTCLICK_MENU.Enabled,
													HIERARCHY_TYPE.Transform													
													);


				if (meshGroupTransform._meshGroup != null)
				{
					AddTransformOfMeshGroup(meshGroupTransform._meshGroup, newUnit);
				}
			}
		}

		private void AddBoneUnitsOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			//<BONE_EDIT>
			if (targetMeshGroup._boneList_Root == null || targetMeshGroup._boneList_Root.Count == 0)
			{
				return;
			}

			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			//Root 부터 재귀적으로 호출한다.
			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			{
				AddBoneUnit(targetMeshGroup._boneList_Root[i], parentUnit,
					iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			}

			//자식 객체도 호출해주자
			//>> 삭제. 이미 자식 객체는 별도로 처리되었다.
			//for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
			//{
			//	apMeshGroup meshGroup = targetMeshGroup._childMeshGroupTransforms[i]._meshGroup;
			//	if (meshGroup != null)
			//	{
			//		AddBoneUnitsOfMeshGroup(meshGroup, parentUnit);
			//	}
			//}


		}


		private void AddBoneUnit(apBone bone, apEditorHierarchyUnit parentUnit,
								Texture2D iconNormal, Texture2D iconIKHead, Texture2D iconIKChained, Texture2D iconIKSingle)
		{
			Texture2D icon = iconNormal;

			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.IKHead:
					icon = iconIKHead;
					break;

				case apBone.OPTION_IK.IKChained:
					icon = iconIKChained;
					break;

				case apBone.OPTION_IK.IKSingle:
					icon = iconIKSingle;
					break;
			}

			bool isModRegisted = IsModRegistered(bone);

			//이전
			//apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			
			//if(bone.IsGUIVisible)
			//{
			//	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			//}

			//변경 21.1.28
			//- 본의 보이기 타입이 많아졌다.
			apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;

			switch (bone.VisibleIconType)
			{
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Default:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Tmp:		visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Tmp:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Rule:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible; break;
			}




			apEditorHierarchyUnit addedUnit = AddUnit_ToggleButton_Visible(icon,
														bone._name,
														CATEGORY.Bone_Item,
														bone,
														false,
														//_rootUnit_Mesh,
														parentUnit,
														visibleType,
														apEditorHierarchyUnit.VISIBLE_TYPE.None,
														isModRegisted,
														false,
														RIGHTCLICK_MENU.Enabled,
														HIERARCHY_TYPE.Bone
														);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				AddBoneUnit(bone._childBones[i], addedUnit, iconNormal, iconIKHead, iconIKChained, iconIKSingle);
			}
		}



		// Functions
		//------------------------------------------------------------------------

		private apEditorHierarchyUnit AddUnit_Label(Texture2D icon, string text, CATEGORY savedKey, object savedObj, apEditorHierarchyUnit parent)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			

			//변경 19.11.16 : GUIContent 이용
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);

			newUnit.SetEvent(OnUnitClick, null);
			newUnit.SetLabel(icon, text, (int)savedKey, savedObj);
			newUnit.SetModRegistered(false);

			_units_All.Add(newUnit);

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		private enum RIGHTCLICK_MENU
		{
			None, Enabled
		}

		private apEditorHierarchyUnit AddUnit_ToggleButton(Texture2D icon,
															string text,
															CATEGORY savedKey,
															object savedObj,
															bool isModRegistered,
															apEditorHierarchyUnit parent,
															RIGHTCLICK_MENU rightClickSupported,
															HIERARCHY_TYPE hierarchyType)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent 이용
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);

			switch (hierarchyType)
			{
				case HIERARCHY_TYPE.Transform:
					newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_TF);
					break;

				case HIERARCHY_TYPE.Bone:
					newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_Bone);
					break;

				case HIERARCHY_TYPE.ControlParam:
					newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_ControlParam);
					break;
			}
			
			newUnit.SetToggleButton(icon, text, (int)savedKey, savedObj, true);
			newUnit.SetModRegistered(isModRegistered);


			//추가 21.6.13 : 우클릭 지원
			if(rightClickSupported == RIGHTCLICK_MENU.Enabled)
			{
				newUnit.SetRightClickEvent(OnUnitRightClick);
			}


			_units_All.Add(newUnit);


			//[1.4.2] 오브젝트 매핑
			if(savedObj != null)
			{
				switch (hierarchyType)
				{
					case HIERARCHY_TYPE.Transform:
						if(!_object2Unit_Transform.ContainsKey(savedObj))
						{
							_object2Unit_Transform.Add(savedObj, newUnit);
						}
						break;

					case HIERARCHY_TYPE.Bone:
						if(!_object2Unit_Bone.ContainsKey(savedObj))
						{
							_object2Unit_Bone.Add(savedObj, newUnit);
						}
						break;

					case HIERARCHY_TYPE.ControlParam:
						if(!_object2Unit_ControlParam.ContainsKey(savedObj))
						{
							_object2Unit_ControlParam.Add(savedObj, newUnit);
						}
						break;
				}
			}


			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}

		private apEditorHierarchyUnit AddUnit_ToggleButton_Visible(Texture2D icon,
																		string text,
																		CATEGORY savedKey,
																		object savedObj,
																		bool isRoot,
																		apEditorHierarchyUnit parent,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Prefix,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Postfix,
																		bool isModRegisted,
																		bool isRefreshVisiblePrefixWhenRender,
																		RIGHTCLICK_MENU rightClickSupported,
																		HIERARCHY_TYPE hierarchyType)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent 이용
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);

			apEditorHierarchyUnit.FUNC_REFRESH_VISIBLE_PREFIX funcRefreshVislbePrefix = null;
			if(isRefreshVisiblePrefixWhenRender)
			{
				funcRefreshVislbePrefix = OnRefreshVisiblePrefix;
			}

			newUnit.SetVisibleIconImage(
				_guiContent_Visible_Current, _guiContent_NonVisible_Current,
				_guiContent_Visible_TmpWork, _guiContent_NonVisible_TmpWork,
				_guiContent_Visible_Default, _guiContent_NonVisible_Default,
				_guiContent_Visible_ModKey, _guiContent_NonVisible_ModKey,
				_guiContent_Visible_Rule, _guiContent_NonVisible_Rule,
				_guiContent_NoKey, _guiContent_NoKeyDisabled,
				funcRefreshVislbePrefix
				);

			switch (hierarchyType)
			{
				case HIERARCHY_TYPE.Transform:
					newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_TF);
					break;

				case HIERARCHY_TYPE.Bone:
					newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_Bone);
					break;

				case HIERARCHY_TYPE.ControlParam:
					newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_ControlParam);
					break;
			}
			
			newUnit.SetToggleButton_Visible(icon, text, (int)savedKey, savedObj, true, visibleType_Prefix, visibleType_Postfix);

			newUnit.SetModRegistered(isModRegisted);


			//추가 21.6.13 : 우클릭 지원
			if(rightClickSupported == RIGHTCLICK_MENU.Enabled)
			{
				newUnit.SetRightClickEvent(OnUnitRightClick);
			}


			_units_All.Add(newUnit);


			//[1.4.2] 오브젝트 매핑
			if(savedObj != null)
			{
				switch (hierarchyType)
				{
					case HIERARCHY_TYPE.Transform:
						if(!_object2Unit_Transform.ContainsKey(savedObj))
						{
							_object2Unit_Transform.Add(savedObj, newUnit);
						}
						break;

					case HIERARCHY_TYPE.Bone:
						if(!_object2Unit_Bone.ContainsKey(savedObj))
						{
							_object2Unit_Bone.Add(savedObj, newUnit);
						}
						break;

					case HIERARCHY_TYPE.ControlParam:
						if(!_object2Unit_ControlParam.ContainsKey(savedObj))
						{
							_object2Unit_ControlParam.Add(savedObj, newUnit);
						}
						break;
				}
			}


			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		// Refresh (without Reset)
		//-----------------------------------------------------------------------------------------
		public void RefreshUnits()
		{
			if (Editor == null || Editor._portrait == null || Editor.Select.AnimClip == null)
			{
				return;
			}

			ReloadGUIContent();//<<추가

			//Pool 초기화
			if(!_unitPool.IsInitialized)
			{
				_unitPool.Init();
			}

			List<apEditorHierarchyUnit> deletedUnits = new List<apEditorHierarchyUnit>();
			//AddUnit_ToggleButton(null, "Select Overall", CATEGORY.Overall_item, null, false, _rootUnit_Overall);

			//1. Transform에 대해서 Refresh를 하자
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if (targetMeshGroup == null)
			{
				//MeshGroup이 없네염
				//기존 Transform 관련 데이터 다 날려야함 + Bone도
			}
			else
			{
				//재귀적으로 만든 경우는 단일 리스트로 만들기 힘들다.
				//단일 리스트 조회 후 한번에 처리해야한다. (근데 Refresh는 또 재귀 호출일세 ㅜㅜ)

				List<apTransform_Mesh> childMeshTransforms = new List<apTransform_Mesh>();
				List<apTransform_MeshGroup> childMeshGroupTransforms = new List<apTransform_MeshGroup>();

				SearchMeshGroupTransforms(targetMeshGroup, _rootUnit_Transform, childMeshTransforms, childMeshGroupTransforms);

				CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);
				CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms);

				//추가 19.6.29 : TmpWorkVisible 확인
				if (_rootUnit_Transform != null)
				{
					_rootUnit_Transform.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Meshes);
				}
				if(_rootUnit_Bone_Main != null)
				{
					_rootUnit_Bone_Main.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Bones);
				}
				
			}

			List<apBone> resultBones = new List<apBone>();

			//2. Bone에 대해서 Refresh를 하자
			if (targetMeshGroup != null)
			{
				SearchBones(targetMeshGroup, _rootUnit_Bone_Main, _rootUnit_Bones_Sub, resultBones);

				
			}

			CheckRemovableUnits<apBone>(deletedUnits, CATEGORY.Bone_Item, resultBones);


			//3. Control Param에 대해서 Refresh를 하자
			List<apControlParam> controlParams = Editor._portrait._controller._controlParams;
			//Texture2D iconImage_Control = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);

			for (int i = 0; i < controlParams.Count; i++)
			{
				apControlParam curParam = controlParams[i];

				bool isModRegistered = IsModRegistered(curParam);
				RefreshUnit(CATEGORY.ControlParam,
								//iconImage_Control, 
								Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(curParam._iconPreset)),
								curParam,
								curParam._keyName,
								Editor.Select.SelectedControlParamOnAnimClip,
								null,//ControlParam은 다중 선택 불가
								apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible,
								apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible,
								isModRegistered,
								//_rootUnit_Mesh
								_rootUnit_ControlParam,
								HIERARCHY_TYPE.ControlParam
								);
			}

			CheckRemovableUnits<apControlParam>(deletedUnits, CATEGORY.ControlParam, controlParams);




			for (int i = 0; i < deletedUnits.Count; i++)
			{
				//1. 먼저 All에서 없앤다.
				//2. Parent가 있는경우,  Parent에서 없애달라고 한다.
				apEditorHierarchyUnit dUnit = deletedUnits[i];
				if (dUnit._parentUnit != null)
				{
					dUnit._parentUnit._childUnits.Remove(dUnit);
				}

				_units_All.Remove(dUnit);

				//[1.4.2] 오브젝트 매핑에서도 삭제
				if(dUnit._savedObj != null)
				{
					_object2Unit_Transform.Remove(dUnit._savedObj);
					_object2Unit_Bone.Remove(dUnit._savedObj);
					_object2Unit_ControlParam.Remove(dUnit._savedObj);
				}

				//추가 20.3.18 : 그냥 삭제하면 안되고, Pool에 반납해야 한다.
				_unitPool.PushUnit(dUnit);
			}

			//전체 Sort를 한다.
			//재귀적으로 실행
			for (int i = 0; i < _units_Root_Transform.Count; i++)
			{
				SortUnit_Recv(_units_Root_Transform[i]);
			}



			//추가 21.2.9 : Sub List는 MeshGroupTF의 순서대로 표시해야한다.
			_units_Root_Bone.Sort(delegate(apEditorHierarchyUnit a, apEditorHierarchyUnit b)
			{
				if ((CATEGORY)(a._savedKey) == CATEGORY.MainName)
				{
					return -1;
				}
				if ((CATEGORY)(b._savedKey) == CATEGORY.MainName)
				{
					return 1;
				}
				if ((CATEGORY)(a._savedKey) == CATEGORY.SubName_Bone && (CATEGORY)(b._savedKey) == CATEGORY.SubName_Bone)
				{
					//둘다 서브일 경우에 (추가 21.2.9)
					apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
					apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;
					if(meshGroup_a != null && meshGroup_b != null)
					{
						return meshGroup_b._depth - meshGroup_a._depth;
					}
				}
				return 0;
			});

			for (int i = 0; i < _units_Root_Bone.Count; i++)
			{
				SortUnit_Recv(_units_Root_Bone[i]);
			}

			for (int i = 0; i < _units_Root_ControlParam.Count; i++)
			{
				SortUnit_Recv(_units_Root_ControlParam[i]);
			}

			

			//추가 20.7.4 : 다중 선택을 위한 LinearIndex를 갱신하자
			RefreshLinearIndices();
			
		}

		private void SearchMeshGroupTransforms(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apTransform_Mesh> resultMeshTransforms, List<apTransform_MeshGroup> resultMeshGroupTransforms)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			for (int i = 0; i < childMeshTransforms.Count; i++)
			{
				apTransform_Mesh meshTransform = childMeshTransforms[i];
				Texture2D iconImage = null;
				if (meshTransform._isClipping_Child)
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
				}
				else
				{
					iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh_16px);//변경 22.6.10 : 16px로 변경
				}

				resultMeshTransforms.Add(meshTransform);

				bool isModRegistered = IsModRegistered(meshTransform);
				RefreshUnit(CATEGORY.Mesh_Item,
								iconImage,
								meshTransform,
								meshTransform._nickName,
								
								//Editor.Select.SubMeshTransformOnAnimClip,//이전
								Editor.Select.MeshTF_Main,//변경 20.6.16
								
								Editor.Select.IsSubSelected,
								GetVisibleIconType(meshTransform, isModRegistered, true),
								GetVisibleIconType(meshTransform, isModRegistered, false),
								isModRegistered,
								//_rootUnit_Mesh
								parentUnit,
								HIERARCHY_TYPE.Transform
								);
			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				resultMeshGroupTransforms.Add(meshGroupTransform);

				bool isModRegistered = IsModRegistered(meshGroupTransform);
				apEditorHierarchyUnit existUnit = RefreshUnit(	CATEGORY.MeshGroup_Item,
																Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px),//변경 22.6.10 : 16px로 변경
																meshGroupTransform,
																meshGroupTransform._nickName,
													
																//Editor.Select.SubMeshGroupTransformOnAnimClip,//이전
																Editor.Select.MeshGroupTF_Main,//변경 20.6.17

																Editor.Select.IsSubSelected,
																GetVisibleIconType(meshGroupTransform, isModRegistered, true),
																GetVisibleIconType(meshGroupTransform, isModRegistered, false),
																isModRegistered,
																//_rootUnit_MeshGroup
																parentUnit,
																HIERARCHY_TYPE.Transform
																);

				if (meshGroupTransform._meshGroup != null)
				{
					SearchMeshGroupTransforms(meshGroupTransform._meshGroup, existUnit, resultMeshTransforms, resultMeshGroupTransforms);
				}
			}
		}


		private void SearchBones(apMeshGroup targetMeshGroup, apEditorHierarchyUnit rootUnit, List<apEditorHierarchyUnit> subRootUnits, List<apBone> resultBones)
		{
			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			//<BONE_EDIT>
			//List<apBone> rootBones = targetMeshGroup._boneList_Root;

			//for (int i = 0; i < rootBones.Count; i++)
			//{
			//	SearchAndRefreshBone(rootBones[i], parentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			//}

			////Child Mesh도 체크한다.
			//for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
			//{
			//	apMeshGroup childMeshGroup = targetMeshGroup._childMeshGroupTransforms[i]._meshGroup;
			//	if (childMeshGroup != null)
			//	{
			//		if (childMeshGroup._boneList_Root.Count > 0)
			//		{
			//			for (int iRootBone = 0; iRootBone < childMeshGroup._boneList_Root.Count; iRootBone++)
			//			{
			//				SearchAndRefreshBone(childMeshGroup._boneList_Root[iRootBone], parentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
			//			}
			//		}

			//	}
			//}

			//>>BoneSet으로 변경
			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = targetMeshGroup._boneListSets[iSet];

				apEditorHierarchyUnit targetParentUnit = null;
				if(boneSet._isRootMeshGroup)
				{
					//Root Mesh인 경우
					targetParentUnit = rootUnit;
				}
				else
				{
					//Sub Mesh인 경우
					targetParentUnit = subRootUnits.Find(delegate(apEditorHierarchyUnit a)
					{
						return a._savedObj == boneSet._meshGroupTransform;
					});
				}

				if(targetParentUnit == null)
				{
					//Debug.LogError("AnyPortrait : No Sub MeshGroup of Bones [" + boneSet._meshGroupTransform._nickName + "]");
					continue;
				}

				
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					SearchAndRefreshBone(boneSet._bones_Root[iRoot], targetParentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
				}
			}

		}

		private void SearchAndRefreshBone(apBone bone, apEditorHierarchyUnit parentUnit, List<apBone> resultBones,
			Texture2D iconNormal, Texture2D iconIKHead, Texture2D iconIKChained, Texture2D iconIKSingle)
		{
			resultBones.Add(bone);

			Texture2D icon = iconNormal;
			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.IKHead:
					icon = iconIKHead;
					break;

				case apBone.OPTION_IK.IKChained:
					icon = iconIKChained;
					break;

				case apBone.OPTION_IK.IKSingle:
					icon = iconIKSingle;
					break;
			}

			bool isModRegisted = IsModRegistered(bone);

			//이전
			//apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			
			//if(bone.IsGUIVisible)
			//{
			//	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			//}

			//변경 21.1.28
			//- 본의 보이기 타입이 많아졌다.
			apEditorHierarchyUnit.VISIBLE_TYPE visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;

			switch (bone.VisibleIconType)
			{
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Default:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Tmp:		visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Tmp:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Rule:	visibleType = apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible; break;
			}

			apEditorHierarchyUnit curUnit = RefreshUnit(CATEGORY.Bone_Item,
															icon,
															bone,
															bone._name,
															//Editor.Select.SubMeshInGroup, 
															Editor.Select.Bone,
															Editor.Select.IsSubSelected,
															visibleType,
															apEditorHierarchyUnit.VISIBLE_TYPE.None,
															isModRegisted,
															//_rootUnit_Mesh
															parentUnit,
															HIERARCHY_TYPE.Bone
															);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				SearchAndRefreshBone(bone._childBones[i], curUnit, resultBones, iconNormal,
					iconIKHead, iconIKChained, iconIKSingle);
			}
		}




		private apEditorHierarchyUnit RefreshUnit(CATEGORY category,
													Texture2D iconImage,
													object obj, string objName, 
													object selectedObj,
													apSelection.FUNC_IS_SUB_SELECTED funcIsSubSelected,//추가 20.5.28 : 다중 선택도 체크한다.
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Pre,
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Post,
													bool isModRegistered,
													apEditorHierarchyUnit parentUnit,
													HIERARCHY_TYPE hierarchyType)
		{
			apEditorHierarchyUnit unit = _units_All.Find(delegate (apEditorHierarchyUnit a)
				{
					if (obj != null)
					{
						if ((CATEGORY)a._savedKey == category)
						{
							if (a._savedObj == obj)
							{
								return true;
							}

							if (a._savedObj is apTransform_Mesh && obj is apTransform_Mesh)
							{
								if ((a._savedObj as apTransform_Mesh)._transformUniqueID == (obj as apTransform_Mesh)._transformUniqueID)
								{
									//Debug.Log("찾음 : 레퍼런스는 다르지만 Mesh TF ID가 동일하다.");
									return true;
								}
							}
							else if (a._savedObj is apTransform_MeshGroup && obj is apTransform_MeshGroup)
							{
								if ((a._savedObj as apTransform_MeshGroup)._transformUniqueID == (obj as apTransform_MeshGroup)._transformUniqueID)
								{
									//Debug.Log("찾음 : 레퍼런스는 다르지만 MeshGroup TF ID가 동일하다.");
									return true;
								}
							}
							else if (a._savedObj is apBone && obj is apBone)
							{
								if ((a._savedObj as apBone)._uniqueID == (obj as apBone)._uniqueID)
								{
									//Debug.Log("찾음 : 레퍼런스는 다르지만 Bone ID가 동일하다.");
									return true;
								}
							}
							else if (a._savedObj is apControlParam && obj is apControlParam)
							{
								if ((a._savedObj as apControlParam)._uniqueID == (obj as apControlParam)._uniqueID)
								{
									return true;
								}
							}
						}
						return (CATEGORY)a._savedKey == category && a._savedObj == obj;
					}
					else
					{
						return (CATEGORY)a._savedKey == category;
					}
				});

			if (objName == null)
			{
				objName = "";
			}

			if (unit != null)
			{
				if(unit._savedObj != null && obj != null)
				{
					unit._savedObj = obj;//ID만 같은 경우를 대비해서 다시 갱신
				}

				if (selectedObj != null && unit._savedObj == selectedObj)
				{
					//unit._isSelected = true;
					unit.SetSelected(true, true);
				}
				else
				{
					//추가 20.6.5 : 다중 선택도 허용한다.
					if(funcIsSubSelected != null)
					{
						apSelection.SUB_SELECTED_RESULT selectedResult = funcIsSubSelected(unit._savedObj);
						if(selectedResult == apSelection.SUB_SELECTED_RESULT.Main)
						{
							unit.SetSelected(true, true);
						}
						else if(selectedResult == apSelection.SUB_SELECTED_RESULT.Added)
						{
							unit.SetSelected(true, false);
						}
						else
						{
							unit.SetSelected(false, true);
						}
					}
					else
					{
						unit.SetSelected(false, true);
					}
				}

				//unit._isVisible = isVisible;
				unit._visibleType_Prefix = visibleType_Pre;
				unit._visibleType_Postfix = visibleType_Post;

				//수정 1.1 : 버그
				//if(unit._text == null)
				//{
				//	unit._text = "";
				//}

				int nextLevel = (parentUnit == null) ? 0 : (parentUnit._level + 1);

				if (!unit._text.Equals(objName) || unit._level != nextLevel)
				{
					unit._level = nextLevel;
					unit.ChangeText(objName);
				}

				if (unit._icon != iconImage)
				{
					unit.ChangeIcon(iconImage);
				}

				unit.SetModRegistered(isModRegistered);
			}
			else
			{
				//이전
				//if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
				//{
				//	unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Pre, visibleType_Post, isModRegistered, true, RIGHTCLICK_MENU.Enabled);
				//}
				//else
				//{
				//	//unit = AddUnit_ToggleButton(iconImage, objName, category, obj, isModRegistered, parentUnit);
				//	unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Pre, visibleType_Post, isModRegistered, false, RIGHTCLICK_MENU.Enabled);
				//}

				//변경
				bool isRefreshVisiblePrefixWhenRender = false;
				if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
				{
					isRefreshVisiblePrefixWhenRender = true;
				}
				unit = AddUnit_ToggleButton_Visible(	iconImage,
														objName,
														category,
														obj,
														false,
														parentUnit,
														visibleType_Pre,
														visibleType_Post,
														isModRegistered,
														isRefreshVisiblePrefixWhenRender,
														RIGHTCLICK_MENU.Enabled,
														hierarchyType);

				

			}
			return unit;
		}

		private void CheckRemovableUnits<T>(List<apEditorHierarchyUnit> deletedUnits, CATEGORY category, List<T> objList)
		{
			List<apEditorHierarchyUnit> deletedUnits_Sub = _units_All.FindAll(delegate (apEditorHierarchyUnit a)
			{
				if ((CATEGORY)a._savedKey == category)
				{
					if (a._savedObj == null || !(a._savedObj is T))
					{
						return true;
					}

					T savedData = (T)a._savedObj;
					if (!objList.Contains(savedData))
					{
					//리스트에 없는 경우 (무효한 경우)
					return true;
					}
				}
				return false;
			});
			for (int i = 0; i < deletedUnits_Sub.Count; i++)
			{
				deletedUnits.Add(deletedUnits_Sub[i]);
			}
		}


		private void SortUnit_Recv(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					int depthA = -1;
					int depthB = -1;
					int indexPerParentA = a._indexPerParent;
					int indexPerParentB = b._indexPerParent;

					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName)
					{
						return -1;
					}
					if ((CATEGORY)(a._savedKey) == CATEGORY.ControlParam && (CATEGORY)(b._savedKey) == CATEGORY.ControlParam)
					{
						apControlParam cpA = a._savedObj as apControlParam;
						apControlParam cpB = b._savedObj as apControlParam;
						return string.Compare(cpA._keyName, cpB._keyName);
					}
					if ((CATEGORY)(a._savedKey) == CATEGORY.Bone_Item && (CATEGORY)(b._savedKey) == CATEGORY.Bone_Item)
					{
						apBone bone_a = a._savedObj as apBone;
						apBone bone_b = b._savedObj as apBone;

						depthA = bone_a._depth;
						depthB = bone_b._depth;

						if (depthA == depthB)
						{
							int compare = string.Compare(a._text.ToString(), b._text.ToString());
							if (compare == 0)
							{
								return a._indexPerParent - b._indexPerParent;
							}
							return compare;
						}
					}


					

					if (a._savedObj is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
						depthA = meshGroup_a._depth;
					}
					else if (a._savedObj is apTransform_Mesh)
					{
						apTransform_Mesh mesh_a = a._savedObj as apTransform_Mesh;
						depthA = mesh_a._depth;
					}


					if (b._savedObj is apTransform_MeshGroup)
					{
						apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;
						depthB = meshGroup_b._depth;
					}
					else if (b._savedObj is apTransform_Mesh)
					{
						apTransform_Mesh mesh_b = b._savedObj as apTransform_Mesh;
						depthB = mesh_b._depth;
					}

					if (depthA == depthB)
					{
						return indexPerParentA - indexPerParentB;
					}

					return depthB - depthA;

				#region [미사용 코드]

				//if(a._savedKey == b._savedKey)
				//{	
				//	if(a._savedObj is apTransform_MeshGroup && b._savedObj is apTransform_MeshGroup)
				//	{
				//		apTransform_MeshGroup meshGroup_a = a._savedObj as apTransform_MeshGroup;
				//		apTransform_MeshGroup meshGroup_b = b._savedObj as apTransform_MeshGroup;

				//		if(Mathf.Abs(meshGroup_b._depth - meshGroup_a._depth) < 0.0001f)
				//		{
				//			return a._indexPerParent - b._indexPerParent;
				//		}
				//		return (int)((meshGroup_b._depth - meshGroup_a._depth) * 1000.0f);
				//	}
				//	else if(a._savedObj is apTransform_Mesh && b._savedObj is apTransform_Mesh)
				//	{
				//		apTransform_Mesh mesh_a = a._savedObj as apTransform_Mesh;
				//		apTransform_Mesh mesh_b = b._savedObj as apTransform_Mesh;

				//		//Clip인 경우
				//		//서로 같은 Parent를 가지는 Child인 경우 -> Index의 역순
				//		//하나가 Parent인 경우 -> Parent가 아래쪽으로
				//		//그 외에는 Depth 비교
				//		if (mesh_a._isClipping_Child && mesh_b._isClipping_Child &&
				//			mesh_a._clipParentMeshTransform == mesh_b._clipParentMeshTransform)
				//		{
				//			return (mesh_b._clipIndexFromParent - mesh_a._clipIndexFromParent);
				//		}
				//		else if (	mesh_a._isClipping_Child && mesh_b._isClipping_Parent &&
				//					mesh_a._clipParentMeshTransform == mesh_b)
				//		{
				//			//b가 Parent -> b가 뒤로 가야함
				//			return -1;
				//		}
				//		else if (	mesh_b._isClipping_Child && mesh_a._isClipping_Parent &&
				//					mesh_b._clipParentMeshTransform == mesh_a)
				//		{
				//			//a가 Parent -> a가 뒤로 가야함
				//			return 1;
				//		}
				//		else
				//		{
				//			if (Mathf.Abs(mesh_b._depth - mesh_a._depth) < 0.0001f)
				//			{
				//				return a._indexPerParent - b._indexPerParent;
				//			}
				//			return (int)((mesh_b._depth - mesh_a._depth) * 1000.0f);
				//		}
				//	}
				//	return a._indexPerParent - b._indexPerParent;
				//}
				//return a._savedKey - b._savedKey; 
				#endregion
			});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv(unit._childUnits[i]);
				}
			}
		}





		// 갱신된 유닛들을 "선형"으로 정리한다. (Shift로 여러개 선택할 수 있게)
		//-----------------------------------------------------------------------------------------
		//추가 20.7.4 : GUI 출력 순서에 맞게 "재귀 Index"와 다른 "선형 Index"를 설정한다.
		//GUI 출력 함수를 참고하자
		public void RefreshLinearIndices()
		{
			//Mesh 따로, Bone 따로
			int curIndex = 0;
			for (int i = 0; i < _units_Root_Transform.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(_units_Root_Transform[i], curIndex);
			}

			curIndex = 0;
			for (int i = 0; i < _units_Root_Bone.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(_units_Root_Bone[i], curIndex);
			}

			curIndex = 0;
			//컨트롤 파라미터는 다중 선택이 안되지만 일단 선형 인덱스는 넣어주자
			for (int i = 0; i < _units_Root_ControlParam.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(_units_Root_ControlParam[i], curIndex);
			}
		}

		private int SetLinearIndexRecursive(apEditorHierarchyUnit unit, int curIndex)
		{	
			unit.SetLinearIndex(curIndex);
			curIndex++;//적용 후 1 증가

			if(unit._childUnits == null || unit._childUnits.Count == 0)
			{
				return curIndex;
			}

			for (int i = 0; i < unit._childUnits.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(unit._childUnits[i], curIndex);
			}

			return curIndex;
		}


		//추가 20.7.4 : Shift키를 눌렀을 때, 이전 유닛 ~ 현재 클릭한 유닛 사이의 유닛들을 클릭할 수 있게 만들자
		private bool GetShiftClickedUnits(apEditorHierarchyUnit clickedUnit, CATEGORY category)
		{
			if(_shiftClickedUnits == null)
			{
				_shiftClickedUnits = new List<apEditorHierarchyUnit>();
			}
			_shiftClickedUnits.Clear();
			
			//이전에 클릭한게 없거나 카테고리가 다르면
			//현재 Main으로 선택된게 있나 찾는다.
			bool isNeedToFindSelectedMainUnit = false;
			if(_lastClickedUnit == null || !_units_All.Contains(_lastClickedUnit))
			{
				//클릭한게 없다.
				isNeedToFindSelectedMainUnit = true;
			}
			else if(category != (CATEGORY)_lastClickedUnit._savedKey)
			{
				//카테고리가 다르다.
				isNeedToFindSelectedMainUnit = true;
			}
			//if(!_lastClickedUnit.IsSelected_Main && !_lastClickedUnit.IsSelected_Sub)
			//{
			//	//클릭한게 선택된게 아니다.
			//	isNeedToFindSelectedMainUnit = true;
			//}

			apEditorHierarchyUnit startUnit = null;

			if(isNeedToFindSelectedMainUnit)
			{
				//클릭한게 없으므로 선택된 유닛을 찾자. 못찾을수도 있다.
				List<apEditorHierarchyUnit> prevSelectedUnits = null;
				
				if(category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
				{
					prevSelectedUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
					{
						return (a.IsSelected_Main || a.IsSelected_Sub)
							&& ((CATEGORY)a._savedKey == CATEGORY.Mesh_Item || (CATEGORY)a._savedKey == CATEGORY.MeshGroup_Item);
					});
				}
				else if(category == CATEGORY.Bone_Item)
				{
					prevSelectedUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
					{
						return (a.IsSelected_Main || a.IsSelected_Sub)
							&& (CATEGORY)a._savedKey == CATEGORY.Bone_Item;
					});
				}

				if(prevSelectedUnits != null && prevSelectedUnits.Count == 1)
				{
					//선택된 유닛이 한개만 있을때
					startUnit = prevSelectedUnits[0];
				}
			}
			else
			{
				//이전에 클릭했던게 영역 시작이다.
				startUnit = _lastClickedUnit;
			}

			if(startUnit == null)
			{
				//실패..
				return false;
			}

			if(clickedUnit == startUnit)
			{
				//같은걸 선택했다면
				return false;
			}

			int prevIndex = startUnit.LinearIndex;
			int nextIndex = clickedUnit.LinearIndex;

			//그 사이에 있는 걸 찾자
			//이미 선택된 것과 클릭한건 따로 처리될 것이므로 제외
			//선택 여부는 클릭한 유닛의 반대 값이어야 한다.
			bool isGetUnselected = !clickedUnit.IsSelected_Main && !clickedUnit.IsSelected_Sub; //선택되지 않은걸 목표로 클릭했으면 다같이 선택하는 것으로만)


			int minIndex = Mathf.Min(prevIndex, nextIndex);
			int maxIndex = Mathf.Max(prevIndex, nextIndex);

			List<apEditorHierarchyUnit> selectableUnits = null;
			if(category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
			{
				selectableUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
				{
					return (a.LinearIndex > minIndex)
						&& (a.LinearIndex < maxIndex)
						&& (
							(isGetUnselected && (!a.IsSelected_Main && !a.IsSelected_Sub))//선택되지 않은 것만 선택하거나
							|| (!isGetUnselected && (a.IsSelected_Main || a.IsSelected_Sub))//선택된 것만 선택해서 해제하자
							)
						&& ((CATEGORY)a._savedKey == CATEGORY.Mesh_Item || (CATEGORY)a._savedKey == CATEGORY.MeshGroup_Item);
				});
			}
			else if(category == CATEGORY.Bone_Item)
			{
				selectableUnits = _units_All.FindAll(delegate(apEditorHierarchyUnit a)
				{
					return (a.LinearIndex > minIndex)
						&& (a.LinearIndex < maxIndex)
						&& (
							(isGetUnselected && (!a.IsSelected_Main && !a.IsSelected_Sub))//선택되지 않은 것만 선택하거나
							|| (!isGetUnselected && (a.IsSelected_Main || a.IsSelected_Sub))//선택된 것만 선택해서 해제하자
							)
						&& (CATEGORY)a._savedKey == CATEGORY.Bone_Item;
				});
			}

			if(selectableUnits == null || selectableUnits.Count == 0)
			{
				//선택될게 없다면
				return false;
			}

			//결과 복사
			//일단, startUnit의 선택 여부가 타겟의 선택 여부와 다르다면 포함시키자
			if((isGetUnselected && (!startUnit.IsSelected_Main && !startUnit.IsSelected_Sub))
				|| (!isGetUnselected && (startUnit.IsSelected_Main || startUnit.IsSelected_Sub))
				)
			{
				_shiftClickedUnits.Add(startUnit);
			}
			for (int i = 0; i < selectableUnits.Count; i++)
			{
				_shiftClickedUnits.Add(selectableUnits[i]);
			}

			return _shiftClickedUnits.Count > 0;
		}


		// 일반 이벤트
		//-----------------------------------------------------------------------------------------
		//추가 22.12.12 : 막 클릭한 Hierarchy는 별도의 단축키가 적용된다.
		public bool OnCheckSelectedHierarchy_TF(apEditorHierarchyUnit unit)
		{
			if(unit == null || unit._savedObj == null)
			{
				return false;
			}
			return Editor.LastClickedHierarchy == apEditor.LAST_CLICKED_HIERARCHY.AnimClip_TF;
		}

		public bool OnCheckSelectedHierarchy_Bone(apEditorHierarchyUnit unit)
		{
			if(unit == null || unit._savedObj == null)
			{
				return false;
			}
			return Editor.LastClickedHierarchy == apEditor.LAST_CLICKED_HIERARCHY.AnimClip_Bone;
		}

		public bool OnCheckSelectedHierarchy_ControlParam(apEditorHierarchyUnit unit)
		{
			if(unit == null || unit._savedObj == null)
			{
				return false;
			}
			return Editor.LastClickedHierarchy == apEditor.LAST_CLICKED_HIERARCHY.AnimClip_ControlParam;
		}


		// Click Event
		//-----------------------------------------------------------------------------------------
		public void OnUnitClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isCtrl, bool isShift)
		{
			if (Editor == null || Editor.Select.AnimClip == null)
			{
				return;
			}

			apSelection.MULTI_SELECT multSelect = ((isCtrl || isShift) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main);

			//이전 : 선택한것만 찾아서 나머지 비활성하면 되는 것이었다.
			//apEditorHierarchyUnit selectedUnit = null;
			//변경 20.6.5 : 변경된 내역이 있다면 모두 다시 검토해봐야 한다. (다중 선택 때문에)
			bool isAnyChanged = false;
			

			//v1.4.2 : FFD 등의 모달 체크를 하자
			bool isExecutable = Editor.CheckModalAndExecutable();
			if(!isExecutable)
			{
				return;
			}



			CATEGORY category = (CATEGORY)savedKey;

			switch (category)
			{
				case CATEGORY.Mesh_Item:
					{
						//추가 20.7.5 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
						if(isShift)
						{
							if(GetShiftClickedUnits(eventUnit, category))
							{
								apEditorHierarchyUnit shiftUnit = null;
								apTransform_Mesh shiftMeshTF = null;
								apTransform_MeshGroup shiftMeshGroupTF = null;

								for (int iShiftUnit = 0; iShiftUnit < _shiftClickedUnits.Count; iShiftUnit++)
								{
									//사이의 것을 추가한다.
									shiftUnit = _shiftClickedUnits[iShiftUnit];
											
									if((CATEGORY)shiftUnit._savedKey == CATEGORY.Mesh_Item)
									{
										shiftMeshTF = shiftUnit._savedObj as apTransform_Mesh;
										if(shiftMeshTF != null)
										{
											Editor.Select.SelectMeshTF_ForAnimEdit(shiftMeshTF, false, false,
																				apSelection.MULTI_SELECT.AddOrSubtract);
										}
									}
									else if((CATEGORY)shiftUnit._savedKey == CATEGORY.MeshGroup_Item)
									{
										shiftMeshGroupTF = shiftUnit._savedObj as apTransform_MeshGroup;
										if(shiftMeshGroupTF != null)
										{
											Editor.Select.SelectMeshGroupTF_ForAnimEdit(shiftMeshGroupTF, false, false,
																				apSelection.MULTI_SELECT.AddOrSubtract);
										}
									}
								}

								_shiftClickedUnits.Clear();

							}	
						}	

						//클릭한거 추가
						apTransform_Mesh meshTransform = savedObj as apTransform_Mesh;
						if (meshTransform != null)
						{
							Editor.Select.SelectMeshTF_ForAnimEdit(meshTransform, true, true, multSelect);
							
							isAnyChanged = true;

							//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.AnimClip_TF);

							//[1.4.2] 하단 UI의 클릭 이벤트와 동기화를 하고, 자동으로 스크롤하자
							Editor.Select.SyncLastClickedTimelineLayerInfo(meshTransform);

							if(Editor._option_AutoScrollWhenObjectSelected)
							{
								//스크롤 가능한 상황인지 체크하고
								if(Editor.IsAutoScrollableWhenClickObject_Animation(meshTransform, true))
								{
									//자동 스크롤을 요청한다.
									Editor.AutoScroll_HierarchyAnimation(meshTransform);
								}
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						//추가 20.7.5 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
						if(isShift)
						{
							if(GetShiftClickedUnits(eventUnit, category))
							{
								apEditorHierarchyUnit shiftUnit = null;
								apTransform_Mesh shiftMeshTF = null;
								apTransform_MeshGroup shiftMeshGroupTF = null;

								for (int iShiftUnit = 0; iShiftUnit < _shiftClickedUnits.Count; iShiftUnit++)
								{
									//사이의 것을 추가한다.
									shiftUnit = _shiftClickedUnits[iShiftUnit];
											
									if((CATEGORY)shiftUnit._savedKey == CATEGORY.Mesh_Item)
									{
										shiftMeshTF = shiftUnit._savedObj as apTransform_Mesh;
										if(shiftMeshTF != null)
										{
											Editor.Select.SelectMeshTF_ForAnimEdit(shiftMeshTF, false, false,
																				apSelection.MULTI_SELECT.AddOrSubtract);
										}
									}
									else if((CATEGORY)shiftUnit._savedKey == CATEGORY.MeshGroup_Item)
									{
										shiftMeshGroupTF = shiftUnit._savedObj as apTransform_MeshGroup;
										if(shiftMeshGroupTF != null)
										{
											Editor.Select.SelectMeshGroupTF_ForAnimEdit(shiftMeshGroupTF, false, false,
																				apSelection.MULTI_SELECT.AddOrSubtract);
										}
									}
								}

								_shiftClickedUnits.Clear();
							}	
						}	


						apTransform_MeshGroup meshGroupTransform = savedObj as apTransform_MeshGroup;
						if (meshGroupTransform != null)
						{
							Editor.Select.SelectMeshGroupTF_ForAnimEdit(meshGroupTransform, true, true, multSelect);
							//if (Editor.Select.SubMeshGroupTransformOnAnimClip == meshGroupTransform)
							//{
							//	selectedUnit = eventUnit;
							//}

							isAnyChanged = true;

							//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.AnimClip_TF);


							//[1.4.2] 하단 UI의 클릭 이벤트와 동기화를 하고, 자동으로 스크롤하자
							Editor.Select.SyncLastClickedTimelineLayerInfo(meshGroupTransform);

							if(Editor._option_AutoScrollWhenObjectSelected)
							{
								//스크롤 가능한 상황인지 체크하고
								if(Editor.IsAutoScrollableWhenClickObject_Animation(meshGroupTransform, true))
								{
									//자동 스크롤을 요청한다.
									Editor.AutoScroll_HierarchyAnimation(meshGroupTransform);
								}
							}
						}
					}
					break;

				case CATEGORY.ControlParam:
					{
						apControlParam controlParam = savedObj as apControlParam;
						if (controlParam != null)
						{
							Editor.Select.SelectControlParam_ForAnimEdit(controlParam, true, true);
							//if (Editor.Select.SubControlParamOnAnimClip == controlParam)
							//{
							//	selectedUnit = eventUnit;
							//}

							isAnyChanged = true;

							//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.AnimClip_ControlParam);


							//[1.4.2] 하단 UI의 클릭 이벤트와 동기화를 하고, 자동으로 스크롤하자
							Editor.Select.SyncLastClickedTimelineLayerInfo(controlParam);

							if(Editor._option_AutoScrollWhenObjectSelected)
							{
								//스크롤 가능한 상황인지 체크하고
								if(Editor.IsAutoScrollableWhenClickObject_Animation(controlParam, true))
								{
									//자동 스크롤을 요청한다.
									Editor.AutoScroll_HierarchyAnimation(controlParam);
								}
							}
						}
					}
					break;

				case CATEGORY.Bone_Item:
					{
						//추가 20.7.5 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
						if (isShift)
						{
							if (GetShiftClickedUnits(eventUnit, category))
							{
								apEditorHierarchyUnit shiftUnit = null;
								apBone shiftBone = null;

								for (int iShiftUnit = 0; iShiftUnit < _shiftClickedUnits.Count; iShiftUnit++)
								{
									//사이의 것을 추가한다.
									shiftUnit = _shiftClickedUnits[iShiftUnit];

									if ((CATEGORY)shiftUnit._savedKey == CATEGORY.Bone_Item)
									{
										shiftBone = shiftUnit._savedObj as apBone;
										if (shiftBone != null)
										{
											Editor.Select.SelectBone_ForAnimEdit(	shiftBone, false, false, 
																				apSelection.MULTI_SELECT.AddOrSubtract);
										}
									}
								}
								_shiftClickedUnits.Clear();
							}
						}	


						apBone bone = savedObj as apBone;
						if (bone != null)
						{
							Editor.Select.SelectBone_ForAnimEdit(bone, true, true, multSelect);
							//if (Editor.Select.Bone == bone)
							//{
							//	selectedUnit = eventUnit;
							//}

							isAnyChanged = true;

							//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.AnimClip_Bone);


							//[1.4.2] 하단 UI의 클릭 이벤트와 동기화를 하고, 자동으로 스크롤하자
							Editor.Select.SyncLastClickedTimelineLayerInfo(bone);

							if(Editor._option_AutoScrollWhenObjectSelected)
							{
								//스크롤 가능한 상황인지 체크하고
								if(Editor.IsAutoScrollableWhenClickObject_Animation(bone, true))
								{
									//자동 스크롤을 요청한다.
									Editor.AutoScroll_HierarchyAnimation(bone);
								}
							}
						}
					}
					break;
			}

			//이전
			//if (selectedUnit != null)
			//{
			//	for (int i = 0; i < _units_All.Count; i++)
			//	{
			//		if (_units_All[i] == selectedUnit)
			//		{
			//			//_units_All[i]._isSelected = true;
			//			_units_All[i].SetSelected(true, true);
			//		}
			//		else
			//		{
			//			//_units_All[i]._isSelected = false;
			//			_units_All[i].SetSelected(false, true);
			//		}
			//	}
			//}
			//else
			//{
			//	for (int i = 0; i < _units_All.Count; i++)
			//	{
			//		//_units_All[i]._isSelected = false;
			//		_units_All[i].SetSelected(false, true);
			//	}
			//}

			//변경 20.6.5 : 전체 갱신
			if (isAnyChanged)
			{
				//변경 20.5.28 : 전체 갱신
				apEditorHierarchyUnit curUnit = null;
				apSelection.SUB_SELECTED_RESULT selectResult = apSelection.SUB_SELECTED_RESULT.None;

				for (int i = 0; i < _units_All.Count; i++)
				{
					curUnit = _units_All[i];

					selectResult = apSelection.SUB_SELECTED_RESULT.None;

					switch ((CATEGORY)curUnit._savedKey)
					{
						case CATEGORY.Mesh_Item:
						case CATEGORY.MeshGroup_Item:
						case CATEGORY.Bone_Item:
							{
								selectResult = Editor.Select.IsSubSelected(curUnit._savedObj);
							}
							break;

						case CATEGORY.ControlParam:
							{
								if(Editor.Select.SelectedControlParamOnAnimClip == curUnit._savedObj)
								{
									selectResult = apSelection.SUB_SELECTED_RESULT.Main;
								}
							}
							break;
					}

					if (selectResult == apSelection.SUB_SELECTED_RESULT.Main)
					{
						curUnit.SetSelected(true, true);
					}
					else if (selectResult == apSelection.SUB_SELECTED_RESULT.Added)
					{
						curUnit.SetSelected(true, false);
					}
					else
					{
						curUnit.SetSelected(false, true);
					}
				}

				//추가 20.7.4 : 클릭한 유닛은 여기서 체크한다.
				_lastClickedUnit = eventUnit;

				//20.7.5 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
				Editor.Gizmos.OnSelectedObjectsChanged();
			}
		}


		public void OnUnitVisibleClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isVisible, bool isPrefixButton)
		{
			if (Editor == null || Editor.Select.AnimClip == null)
			{
				return;
			}

			apAnimClip curAnimClip = Editor.Select.AnimClip;
			apMeshGroup targetMeshGroup = curAnimClip._targetMeshGroup;

			if(targetMeshGroup == null)
			{
				return;
			}

			CATEGORY category = (CATEGORY)savedKey;

			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			apBone bone = null;

			bool isVisibleDefault = false;

			bool isCtrl = false;
			bool isAlt = false;
			if(Event.current != null)
			{
#if UNITY_EDITOR_OSX
				isCtrl = Event.current.command;
#else
				isCtrl = Event.current.control;
#endif		
				isAlt = Event.current.alt;
			}

			switch (category)
			{
				case CATEGORY.Mesh_Item:
					{
						meshTransform = savedObj as apTransform_Mesh;
						isVisibleDefault = meshTransform._isVisible_Default;
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						meshGroupTransform = savedObj as apTransform_MeshGroup;
						isVisibleDefault = meshGroupTransform._isVisible_Default;
					}
					break;

				case CATEGORY.Bone_Item:
					{
						bone = savedObj as apBone;
						//isVisibleDefault = bone.IsGUIVisible;
						isVisibleDefault = bone.IsVisibleInGUI;//변경 21.1.28

					}
					break;

				default:
					return;
			}

			//수정
			//Prefix : TmpWorkVisible을 토글한다.
			if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
			{
				if (isPrefixButton)
				{
					//TmpWorkVisible을 토글하자
					apRenderUnit linkedRenderUnit = null;
					if (meshTransform != null)
					{
						linkedRenderUnit = meshTransform._linkedRenderUnit;
					}
					else if (meshGroupTransform != null)
					{
						linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
					}

					if (linkedRenderUnit != null)
					{
						bool isTmpWorkToShow = false;
						if (linkedRenderUnit._isVisible_WithoutParent == linkedRenderUnit._isVisibleCalculated)
						{
							//TmpWork가 꺼져있다.
							if (linkedRenderUnit._isVisible_WithoutParent)//이 값은 Rule/Tmp 포함이다.
							{
								//Show -> Hide
								//이전
								//linkedRenderUnit._isVisibleWorkToggle_Show2Hide = true;
								//linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

								//변경 21.1.28 : Show(None) -> Hide
								linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;

								isTmpWorkToShow = false;
							}
							else
							{
								//Hide -> Show
								//이전
								//linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
								//linkedRenderUnit._isVisibleWorkToggle_Hide2Show = true;

								//변경 21.1.28 : Hide(None) -> Show
								linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;

								isTmpWorkToShow = true;
							}
						}
						else
						{
							//TmpWork가 켜져있다. 꺼야한다.
							//이전
							//linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
							//linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

							//변경 21.1.28 : Show/Hide -> None
							if(linkedRenderUnit._workVisible_Rule == apRenderUnit.WORK_VISIBLE_TYPE.None)
							{
								linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.None;
							}
							else
							{
								//만약 겉으로 보기엔 다르다 > 같게 만들어야 하는데, Rule이 None이 아니다.
								//계산값과 같게 만들자
								if(linkedRenderUnit._isVisibleCalculated)
								{
									linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToShow;
								}
								else
								{
									linkedRenderUnit._workVisible_Tmp = apRenderUnit.WORK_VISIBLE_TYPE.ToHide;
								}
							}

							isTmpWorkToShow = !linkedRenderUnit._isVisible_WithoutParent;
						}

						if (isCtrl)
						{
							//Ctrl을 눌렀으면 반대로 행동
							//Debug.Log("TmpWork를 다른 RenderUnit에 반대로 적용 : Show : " + !isTmpWorkToShow);
							Editor.Controller.SetMeshGroupTmpWorkVisibleAll(Editor.Select.AnimClip._targetMeshGroup, !isTmpWorkToShow, linkedRenderUnit);
						}
						else
						{
							//이 RenderUnit의 Visible 정보를 저장한다 (20.4.13)
							Editor.VisiblityController.Save_RenderUnit(Editor.Select.AnimClip._targetMeshGroup, linkedRenderUnit);
						}
					}

					Editor.Controller.CheckTmpWorkVisible(Editor.Select.AnimClip._targetMeshGroup);//TmpWorkVisible이 변경되었다면 이 함수 호출
				}
				else
				{
					//Postfix :

					//v1.4.2 : Visibility를 전환할 때, FFD가 켜져있으면 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();
					if(!isExecutable)
					{
						return;
					}


					//여기서는 AnimClip에서는 isVisibleDefault를 수정하지는 않는다.
					//Visible을 눌렀을때
					//Timeline이 있다면 + AnimatedModifier 타입일때 + 그 Modifier가 Color를 지원할 때
					//Layer가 등록되었는가?
					//>> 1. Layer가 등록이 되었다.
					//       >>> 1-1. 재생상태의 현재 키프레임이 있다. -> 현재 프레임을 찾고 Visible을 세팅한다.
					//       >>> 1-2. 현재 키 프레임이 없는 곳이다. -> 키프레임을 추가하고 Visible을 세팅한다.
					//>> 2. Layer가 등록이 되지 않았다.
					//       -> 레이어를 등록하고, 맨 앞프레임에 키프레임을 추가하여 Visible 값을 넣는다.

					bool isColorSupported = false;
					apAnimTimeline curTimeline = Editor.Select.AnimTimeline;
					apModifierBase linkedModifier = null;
					if(curTimeline != null)
					{
						if(curTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
						{
							linkedModifier = curTimeline._linkedModifier;
							if(linkedModifier != null
								&& (int)(Editor.Select.AnimTimeline._linkedModifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.Color) != 0)
							{
								//Color가 지원되는 모디파이어와 연동된 타임라인일 때
								isColorSupported = true;
							}
						}
					}

					if (!isColorSupported)
					{
						return;
					}

					if(meshTransform != null)
					{
						//만약 TF 모디파이어인데, Rigging된 자식 메시 그룹의 메시라면
						//등록하면 안된다.
						if (linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedTF
							&& meshTransform.IsRiggedChildMeshTF(targetMeshGroup))
						{
							EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_RiggedChildMeshUnableToMod_Title),
															Editor.GetText(TEXT.DLG_RiggedChildMeshUnableToMod_Body),
															Editor.GetText(TEXT.Okay));

							return;
						}
					}
					


					//1. TimelineLayer를 찾자
					apAnimTimelineLayer targetTimelineLayer = Editor.Select.AnimTimeline.GetTimelineLayer(savedObj);

					//v1.4.2 : Color Option을 켜야한다.
					//- 모디파이어의 Color Option이 꺼져있거나
					//- 타임라인 레이어가 있는 상태에서 ParamSetGroup의 Color Option이 꺼져있다면
					if(!linkedModifier._isColorPropertyEnabled
						|| (targetTimelineLayer != null 
							&& targetTimelineLayer._targetParamSetGroup != null 
							&& !targetTimelineLayer._targetParamSetGroup._isColorPropertyEnabled))
					{
						//색상 옵션이 꺼진 상태라서 이 상태로 키프레임을 생성하거나 값을 변경하는 것은 의미가 없다.
						//색상 옵션을 먼저 켤지 물어보자
						bool isTurnOnColorOption = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_ColorOptionIsDisabled_Title),
																					Editor.GetText(TEXT.DLG_ColorOptionIsDisabled_Body),
																					Editor.GetText(TEXT.Okay),
																					Editor.GetText(TEXT.Cancel));

						if(isTurnOnColorOption)
						{	
							apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged,
																Editor,
																Editor._portrait,
																Editor.Select.AnimClip._targetMeshGroup,
																Editor.Select.AnimTimeline._linkedModifier, 
																//savedObj, 
																//null,
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly
																);

							linkedModifier._isColorPropertyEnabled = true;

							//타임라인 레이어가 있었다면
							if(targetTimelineLayer != null
								&& targetTimelineLayer._targetParamSetGroup != null)
							{
								targetTimelineLayer._targetParamSetGroup._isColorPropertyEnabled = true;
							}
						}
					}


					if (targetTimelineLayer != null)
					{

						//>> 1. Layer가 등록이 되었다.
						apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged,
																		Editor,
																		Editor._portrait,
																		Editor.Select.AnimClip._targetMeshGroup,
																		Editor.Select.AnimTimeline._linkedModifier,
																		//savedObj, 
																		//null,
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly
																		);

						

						apAnimKeyframe curKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(Editor.Select.AnimClip.CurFrame);
						if (curKeyframe != null)
						{
							//>>> 1-1. 재생상태의 현재 키프레임이 있다. -> 현재 프레임의 Visible을 세팅한다.
							if (curKeyframe._linkedModMesh_Editor != null)
							{
								curKeyframe._linkedModMesh_Editor._isVisible = !curKeyframe._linkedModMesh_Editor._isVisible;
							}
						}
						else
						{
							//>>> 1-2. 현재 키 프레임이 없는 곳이다. -> 키프레임을 추가하고 Visible을 세팅한다.
							curKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, targetTimelineLayer, false, false, false, true);
							if (curKeyframe != null && curKeyframe._linkedModMesh_Editor != null)
							{
								curKeyframe._linkedModMesh_Editor._isVisible = !isVisibleDefault;
							}
						}
					}
					else
					{
						

						//>> 2. Layer가 등록이 되지 않았다.
						//       -> 레이어를 등록하고, 맨 앞프레임에 키프레임을 추가하여 Visible 값을 넣는다.	
						targetTimelineLayer = Editor.Controller.AddAnimTimelineLayer(savedObj, Editor.Select.AnimTimeline);
						if (targetTimelineLayer != null)
						{
							//Color Option도 같이 켜야한다.
							if(targetTimelineLayer._targetParamSetGroup != null)
							{
								targetTimelineLayer._targetParamSetGroup._isColorPropertyEnabled = true;
							}


							apAnimKeyframe curKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.StartFrame, targetTimelineLayer, false, false, false, true);
							if (curKeyframe != null)
							{
								if (curKeyframe._linkedModMesh_Editor != null)
								{
									curKeyframe._linkedModMesh_Editor._isVisible = !isVisibleDefault;
								}
							}
						}
					}

					Editor.Select.SelectAnimTimelineLayer(targetTimelineLayer, apSelection.MULTI_SELECT.Main, false);
				}
			}
			else
			{
				if(bone != null)
				{
					if(isCtrl)
					{
						Editor.Select.AnimClip._targetMeshGroup.SetBoneGUIVisibleAll(isVisibleDefault, bone);
						
						//이 함수가 추가되면 일괄적으로 저장을 해야한다.
						Editor.VisiblityController.Save_AllBones(Editor.Select.AnimClip._targetMeshGroup);
					}
					else
					{
						//이전
						//bone.SetGUIVisible(!isVisibleDefault);

						//변경 21.1.28
						bone.SetGUIVisible_Tmp_ByCheckRule(!isVisibleDefault, isAlt);//Alt를 누른채로 버튼을 누르면 자식 본들도 같이 바뀐다.

						//Visible Controller에도 반영해야 한다. (20.4.13)
						Editor.VisiblityController.Save_Bone(Editor.Select.AnimClip._targetMeshGroup, bone);
					}

					Editor.Controller.CheckTmpWorkVisible(Editor.Select.AnimClip._targetMeshGroup);//TmpWorkVisible이 변경되었다면 이 함수 호출
					
				}
			}

			if (Editor.Select.AnimClip._targetMeshGroup != null)
			{
				Editor.Select.AnimClip._targetMeshGroup.RefreshForce();
			}

			Editor.RefreshControllerAndHierarchy(false);
			//Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Info, null, null);//이전
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);//변경 21.3.17
		}


		private void OnUnitClickRestoreTmpWork_Mesh()
		{
			if(Editor == null || Editor.Select.AnimClip == null)
			{
				return;
			}
			apMeshGroup meshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if(meshGroup == null)
			{
				return;
			}

			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(meshGroup, true, true, false);//이전
			//변경 20.4.13
			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	meshGroup, 
																apEditorController.RESET_VISIBLE_ACTION.ResetForce, 
																apEditorController.RESET_VISIBLE_TARGET.RenderUnits);

			Editor.RefreshControllerAndHierarchy(true);
		}

		private void OnUnitClickRestoreTmpWork_Bone()
		{
			if(Editor == null || Editor.Select.AnimClip == null)
			{
				return;
			}
			apMeshGroup meshGroup = Editor.Select.AnimClip._targetMeshGroup;
			if(meshGroup == null)
			{
				return;
			}

			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(meshGroup, true, false, true);//이전
			//변경 20.4.13
			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	meshGroup, 
																apEditorController.RESET_VISIBLE_ACTION.ResetForce, 
																apEditorController.RESET_VISIBLE_TARGET.Bones);
			Editor.RefreshControllerAndHierarchy(true);
		}

		// Mod Registered 체크
		//------------------------------------------------------------------------------------------
		private bool IsModRegistered(object obj)
		{
			if (Editor.Select.AnimTimeline == null)
			{
				return false;
			}
			return Editor.Select.AnimTimeline.IsObjectAddedInLayers(obj);
		}


		private apEditorHierarchyUnit.VISIBLE_TYPE GetVisibleIconType(object targetObject, bool isModRegistered, bool isPrefix)
		{
			apRenderUnit linkedRenderUnit = null;
			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			if (targetObject is apTransform_Mesh)
			{
				meshTransform = targetObject as apTransform_Mesh;
				linkedRenderUnit = meshTransform._linkedRenderUnit;
			}
			else if (targetObject is apTransform_MeshGroup)
			{
				meshGroupTransform = targetObject as apTransform_MeshGroup;
				linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
			}
			else
			{
				return apEditorHierarchyUnit.VISIBLE_TYPE.None;
			}


			if (linkedRenderUnit == null)
			{
				return apEditorHierarchyUnit.VISIBLE_TYPE.None;
			}

			if (isPrefix)
			{
				//Prefix는
				//1) RenderUnit의 현재 렌더링 상태 -> Current
				//2) MeshTransform/MeshGroupTransform의 
				bool isVisible = linkedRenderUnit._isVisible_WithoutParent;//Visible이 아닌 VisibleParent를 출력한다.
																		   //TmpWork에 의해서 Visible 값이 바뀌는가)
																		   //Calculate != WOParent 인 경우 (TmpWork의 영향을 받았다)
				if (linkedRenderUnit._isVisibleCalculated != linkedRenderUnit._isVisible_WithoutParent)
				{
					//이전
					//if (isVisible)
					//{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; }
					//else
					//{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; }

					//변경 21.1.29 : Tmp와 Rule이 따로 있다.
					if(linkedRenderUnit._workVisible_Tmp != linkedRenderUnit._workVisible_Rule)
					{
						//Tmp와 다르다면, Tmp의 값을 따른다. 단, None인 경우 빼고
						switch (linkedRenderUnit._workVisible_Tmp)
						{
							case apRenderUnit.WORK_VISIBLE_TYPE.None: return isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
							case apRenderUnit.WORK_VISIBLE_TYPE.ToShow: return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible;
							case apRenderUnit.WORK_VISIBLE_TYPE.ToHide: return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
						}
					}
					else
					{
						//Tmp와 같다면, Rule을 따른다.
						return isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
					}
				}
				else
				{
					//TmpWork의 영향을 받지 않았다.
					if (isVisible)
					{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; }
					else
					{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible; }
				}
			}
			else
			{
				//PostFix는
				//1) MeshGroup Setting에서는 Default를 표시하고,
				//2) Modifier/AnimClip 상태에서 ModMesh가 발견 되었을때 ModKey 상태를 출력한다.
				//아무것도 아닐때는 None 리턴

				if (_editor.Select.AnimClip != null
					&& _editor.Select.AnimTimeline != null
					&& _editor.Select.AnimTimeline._linkedModifier != null
					&& linkedRenderUnit != null
					)
				{
					apAnimClip curAnimClip = _editor.Select.AnimClip;
					apAnimTimeline curTimeline = _editor.Select.AnimTimeline;
					apModifierBase linkedModifier = _editor.Select.AnimTimeline._linkedModifier;

					//v1.4.2
					//Color Option이 비활성화되어있다면 NoKeyDisabled가 대신 나와야 한다.
					//<조건은 두가지>
					//(1) Modifier의 Color Option이 꺼져있다.
					//(2) Modifier의 Color Option이 켜져있고 Timeline Layer에 해당하는 ParamSetGroup의 Color Option이 꺼져있다.
					

					//해당 Modifier가 Color/Visible을 지원하는가
					if ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
					{
						if(!linkedModifier._isColorPropertyEnabled)
						{
							//Color Option이 꺼져있다.
							return apEditorHierarchyUnit.VISIBLE_TYPE.NoKeyDisabled;
						}

						if (isModRegistered)
						{
							//1. 자신의 타임라인 레이어를 찾는다.
							//2. 현재의 키프레임을 찾는다.
							//3. (키프레임이 있다면) 그 키프레임에 해당하는 ModMesh를 찾는다.
							apModifiedMesh modMesh = null;
							apAnimTimelineLayer timelineLayer = curTimeline.GetTimelineLayer(targetObject);

							if (timelineLayer != null && timelineLayer._targetParamSetGroup != null)
							{	
								if(!timelineLayer._targetParamSetGroup._isColorPropertyEnabled)
								{
									//만약 이 타임라인 레이어의 Color Option이 인위적으로 꺼진 상태라면
									return apEditorHierarchyUnit.VISIBLE_TYPE.NoKeyDisabled;
								}

								apAnimKeyframe curKeyframe = timelineLayer.GetKeyframeByFrameIndex(_editor.Select.AnimClip.CurFrame);
								if (curKeyframe != null)
								{
									modMesh = curKeyframe._linkedModMesh_Editor;
								}
							}

							if (modMesh != null)
							{
								//다르다면
								//Mod 아이콘
								if (modMesh._isVisible)
								{
									return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_Visible;
								}
								else
								{
									return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_NonVisible;
								}
							}
							else
							{
								//ModMesh가 없다면 => NoKey를 리턴한다.
								return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;
							}
						}
						else
						{
							//Mod 등록이 안되어도 NoKey 출력
							return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;
						}
					}
				}
			}
			return apEditorHierarchyUnit.VISIBLE_TYPE.None;
		}



		private void OnRefreshVisiblePrefix(apEditorHierarchyUnit unit)
		{
			object targetObject = unit._savedObj;
			apRenderUnit linkedRenderUnit = null;
			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			if (targetObject is apTransform_Mesh)
			{
				meshTransform = targetObject as apTransform_Mesh;
				linkedRenderUnit = meshTransform._linkedRenderUnit;
			}
			else if (targetObject is apTransform_MeshGroup)
			{
				meshGroupTransform = targetObject as apTransform_MeshGroup;
				linkedRenderUnit = meshGroupTransform._linkedRenderUnit;
			}
			else
			{
				return;
			}


			if (linkedRenderUnit == null)
			{
				return;
			}

			//Prefix는
			//1) RenderUnit의 현재 렌더링 상태 -> Current
			//2) MeshTransform/MeshGroupTransform의 

			//Visible이 아닌 VisibleParent를 출력한다.
			//TmpWork에 의해서 Visible 값이 바뀌는가)
																		//Calculate != WOParent 인 경우 (TmpWork의 영향을 받았다)
			bool isVisible = linkedRenderUnit._isVisible_WithoutParent;
																		
			//if (linkedRenderUnit._isVisibleCalculated != linkedRenderUnit._isVisible_WithoutParent)
			//{
			//	if (isVisible)
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible;
			//	}
			//	else
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			//	}
			//}
			//else
			//{
			//	//TmpWork의 영향을 받지 않았다.
			//	if (isVisible)
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			//	}
			//	else
			//	{
			//		unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible;
			//	}
			//}


			//변경 21.1.31
			if (linkedRenderUnit._isVisibleCalculated != linkedRenderUnit._isVisible_WithoutParent)
			{
				//이전
				//if (isVisible)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; }
				//else				{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; }

				//변경 21.1.29 : Tmp와 Rule이 따로 있다.
				if(linkedRenderUnit._workVisible_Tmp != linkedRenderUnit._workVisible_Rule)
				{
					//Tmp와 다르다면, Tmp의 값을 따른다. 단, None인 경우 빼고
					switch (linkedRenderUnit._workVisible_Tmp)
					{
						case apRenderUnit.WORK_VISIBLE_TYPE.None:
							unit._visibleType_Prefix = isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
							break;
						case apRenderUnit.WORK_VISIBLE_TYPE.ToShow:
							unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible;
							break;
						case apRenderUnit.WORK_VISIBLE_TYPE.ToHide:
							unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
							break;
					}
				}
				else
				{
					//Tmp와 같다면, Rule을 따른다.
					unit._visibleType_Prefix = isVisible ? apEditorHierarchyUnit.VISIBLE_TYPE.Rule_Visible : apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible;
				}
			}
			else
			{
				//TmpWork의 영향을 받지 않았다.
				if (isVisible)	{ unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; }
				else			{ unit._visibleType_Prefix = apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible; }
			}
		}



		// 우클릭 이벤트
		//--------------------------------------------------------------------------------
		//추가 21.6.12
		//우클릭시 메뉴가 마우스에서 나온다.
		public void OnUnitRightClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj)
		{
			//Debug.Log("우클릭");
			if(_rightMenu != null && savedObj != null)
			{
				//v1.4.2 : 우클릭 하기 전에 모달(FFD 모드 등)을 먼저 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();
				if(!isExecutable)
				{
					return;
				}



				//우클릭 메뉴를 열자 (해당되는 항목만)
				bool isShowMenu = false;
				string objName = null;
				int nSelectedObjs = 0;


				_rightMenu.ReadyToMakeMenu();
				//애니메이션 탭에서는 기능이 제한된다. (Search/SearchAll만 있다)


				CATEGORY category = (CATEGORY)savedKey;
				switch (category)
				{					
					case CATEGORY.Mesh_Item:
					case CATEGORY.MeshGroup_Item:
						{
							isShowMenu = true;

							apTransform_Mesh clickedMeshTF = null;
							apTransform_MeshGroup clickedMeshGroupTF = null;

							if(savedObj is apTransform_Mesh)
							{
								clickedMeshTF = savedObj as apTransform_Mesh;
								objName = clickedMeshTF._nickName;
							}
							else if(savedObj is apTransform_MeshGroup)
							{
								clickedMeshGroupTF = savedObj as apTransform_MeshGroup;
								objName = clickedMeshGroupTF._nickName;
							}
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_SelectAll();

							//선택된 렌더 유닛들의 개수를 찾자
							//선택된 것 중 하나에서 클릭을 했다면 > 여러개중 하나를 선택한 것으로 인식
							//선택한 것과 무관하게 클릭을 했다면 > 하나만 선택한 것으로 인식
							List<apTransform_Mesh> selectedMeshTFs = Editor.Select.GetSubSeletedMeshTFs(false);
							List<apTransform_MeshGroup> selectedMeshGroupTFs = Editor.Select.GetSubSeletedMeshGroupTFs(false);

							int nMeshTFs = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
							int nMeshGroupTFs = selectedMeshGroupTFs != null ? selectedMeshGroupTFs.Count : 0;

							bool isClickInSelected = false;
							if(nMeshTFs > 0 && clickedMeshTF != null)
							{
								if(selectedMeshTFs.Contains(clickedMeshTF))
								{
									isClickInSelected = true;
								}
							}
							if(nMeshGroupTFs > 0 && clickedMeshGroupTF != null)
							{
								if(selectedMeshGroupTFs.Contains(clickedMeshGroupTF))
								{
									isClickInSelected = true;
								}
							}

							if(isClickInSelected)
							{
								//선택한 객체들 중에 하나를 클릭했다면
								nSelectedObjs = nMeshTFs + nMeshGroupTFs;
							}
							else
							{
								//선택한 것들과 상관없는 것을 클릭했다면
								nSelectedObjs = 1;
							}
							
						}
						break;

					case CATEGORY.Bone_Item:
						{
							isShowMenu = true;
							apBone clickedBone = null;
							if(savedObj is apBone)
							{
								clickedBone = savedObj as apBone;
								objName = clickedBone._name;
							}
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_SelectAll();

							//선택된 본의 개수를 찾자
							List<apBone> selectedBones = Editor.Select.GetSubSeletedBones(false);
							int nBones = selectedBones != null ? selectedBones.Count : 0;

							bool isClickInSelected = false;
							if(nBones > 0 && clickedBone != null)
							{
								if(selectedBones.Contains(clickedBone))
								{
									isClickInSelected = true;
								}
							}

							if(isClickInSelected)
							{
								//선택한 객체들 중에 하나를 클릭했다면
								nSelectedObjs = nBones;
							}
							else
							{
								//선택한 것들과 상관없는 것을 클릭했다면
								nSelectedObjs = 1;
							}
							
						}
						break;
				}

				if(isShowMenu)
				{
					_rightMenu.ShowMenu(objName, nSelectedObjs, savedKey, savedObj, eventUnit);
				}
				
			}
		}


		//우클릭 메뉴에서 항목을 선택했을 때
		private void OnSelectRightMenu(	apEditorHierarchyMenu.MENU_ITEM_HIERARCHY menuType,
										int hierachyUnitType,
										object requestedObj,
										apEditorHierarchyUnit clickedUnit)
		{
			if(_editor == null || _editor._portrait == null)
			{
				return;
			}
			
			apMeshGroup curMeshGroup = null;
			if(_editor.Select.AnimClip != null)
			{
				curMeshGroup = _editor.Select.AnimClip._targetMeshGroup;
			}
			if(curMeshGroup == null)
			{
				return;
			}

			CATEGORY category = (CATEGORY)hierachyUnitType;
			switch (category)
			{
				case CATEGORY.Mesh_Item:
				case CATEGORY.MeshGroup_Item:
					{
						switch (menuType)
						{
							case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
								_requestMeshGroup = curMeshGroup;
								_loadKey_Search = apDialog_SearchObjects.ShowDialog_SubObjects(_editor, curMeshGroup, true, true, OnObjectSearched, OnMultipleObjectSearched);
								break;

							case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.SelectAll:
								OnSelectAll(true);
								break;
						}
					}
					break;

				case CATEGORY.Bone_Item:
					{
						switch (menuType)
						{
							case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
								_requestMeshGroup = curMeshGroup;
								_loadKey_Search = apDialog_SearchObjects.ShowDialog_SubObjects(_editor, curMeshGroup, false, true, OnObjectSearched, OnMultipleObjectSearched);
								break;

							case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.SelectAll:
								OnSelectAll(false);
								break;
						}
					}
					break;
			}

			//메뉴를 선택하면 변경이 될 것이므로 Dirty
			apEditorUtil.SetEditorDirty();
			Editor.RefreshControllerAndHierarchy(false);
		}

		


		//오브젝트 찾기 다이얼로그 결과
		private void OnObjectSearched(object loadKey, object targetObject)
		{
			apMeshGroup curMeshGroup = null;
			if(Editor.Select.AnimClip != null)
			{
				curMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			}

			//창이 열린 이후에 연속으로 이 이벤트가 호출될 수 있다.
			if(_loadKey_Search != loadKey
				|| loadKey == null
				|| _editor == null
				|| _editor._portrait == null
				|| _requestMeshGroup != curMeshGroup
				|| _requestMeshGroup == null
				|| curMeshGroup == null)
			{
				_requestMeshGroup = null;
				_loadKey_Search = null;
				return;
			}

			//_loadKey_Search = null;

			if(targetObject == null)
			{
				return;
			}

			
			if(targetObject is apTransform_Mesh)
			{
				apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
				if(meshTransform != null)
				{	
					Editor.Select.SelectSubObject(	meshTransform, null, null, 
														apSelection.MULTI_SELECT.Main,
														apSelection.TF_BONE_SELECT.Exclusive);
				}
			}
			else if(targetObject is apTransform_MeshGroup)
			{
				apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
				if(meshGroupTransform != null)
				{
					Editor.Select.SelectSubObject(	null, meshGroupTransform, null, 
														apSelection.MULTI_SELECT.Main,
														apSelection.TF_BONE_SELECT.Exclusive);
				}
			}
			else if(targetObject is apBone)
			{
				apBone bone = targetObject as apBone;
				if (bone != null)
				{
					Editor.Select.SelectSubObject(	null, null, bone, 
														apSelection.MULTI_SELECT.Main,
														apSelection.TF_BONE_SELECT.Exclusive);
				}
			}


			//[1.4.2] 선택된 객체에 맞게 자동 스크롤			
			if(Editor.IsAutoScrollableWhenClickObject_Animation(targetObject, true))
			{
				//스크롤 가능한 상황인지 체크하고
				//자동 스크롤을 요청한다.
				Editor.AutoScroll_HierarchyAnimation(targetObject);
			}
			

			Editor.RefreshControllerAndHierarchy(true);
		}




		private void OnMultipleObjectSearched(object loadKey, List<object> targetObjects)
		{
			apMeshGroup curMeshGroup = null;
			if(Editor.Select.AnimClip != null)
			{
				curMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			}

			//창이 열린 이후에 연속으로 이 이벤트가 호출될 수 있다.
			if(_loadKey_Search != loadKey
				|| loadKey == null
				|| _editor == null
				|| _editor._portrait == null
				|| _requestMeshGroup != curMeshGroup
				|| _requestMeshGroup == null
				|| curMeshGroup == null)
			{
				_requestMeshGroup = null;
				_loadKey_Search = null;
				return;
			}

			//_loadKey_Search = null;

			if(targetObjects == null || targetObjects.Count == 0)
			{
				return;
			}

			//다중 선택이 있으므로, MeshTF / MeshGroupTF와 Bone 리스트를 구분한다.
			List<apTransform_Mesh> meshTFs = new List<apTransform_Mesh>();
			List<apTransform_MeshGroup> meshGroupTFs = new List<apTransform_MeshGroup>();
			List<apBone> bones = new List<apBone>();

			object curObj = null;
			for (int i = 0; i < targetObjects.Count; i++)
			{
				curObj = targetObjects[i];
				if(curObj == null)
				{
					continue;
				}
				if(curObj is apTransform_Mesh)
				{
					meshTFs.Add(curObj as apTransform_Mesh);
				}
				else if(curObj is apTransform_MeshGroup)
				{
					meshGroupTFs.Add(curObj as apTransform_MeshGroup);
				}
				else if(curObj is apBone)
				{
					bones.Add(curObj as apBone);
				}
			}

			//일단 선택을 초기화
			Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);

			//리깅/물리 모디파이어에서는 단일 선택만 지원
			if (bones.Count > 0)
			{
				//모두 선택하자
				for (int iBone = 0; iBone < bones.Count; iBone++)
				{
					Editor.Select.SelectSubObject(null, null, bones[iBone],
														apSelection.MULTI_SELECT.AddOrSubtract,
														apSelection.TF_BONE_SELECT.Exclusive);
				}
			}
			else if (meshTFs.Count > 0 || meshGroupTFs.Count > 0)
			{
				//모두 선택하자
				if (meshTFs.Count > 0)
				{
					for (int iMeshTF = 0; iMeshTF < meshTFs.Count; iMeshTF++)
					{
						Editor.Select.SelectSubObject(meshTFs[iMeshTF], null, null,
															apSelection.MULTI_SELECT.AddOrSubtract,
															apSelection.TF_BONE_SELECT.Exclusive);
					}
				}
				if (meshGroupTFs.Count > 0)
				{
					for (int iMeshGroupTF = 0; iMeshGroupTF < meshGroupTFs.Count; iMeshGroupTF++)
					{
						Editor.Select.SelectSubObject(null, meshGroupTFs[iMeshGroupTF], null,
															apSelection.MULTI_SELECT.AddOrSubtract,
															apSelection.TF_BONE_SELECT.Exclusive);
					}
				}
			}

			//[1.4.2] 자동 스크롤 처리 (Main을 기준)
			object selectedMainObj = null;
			if(Editor.Select.MeshTF_Main != null)				{ selectedMainObj = Editor.Select.MeshTF_Main; }
			else if(Editor.Select.MeshGroupTF_Main != null)		{ selectedMainObj = Editor.Select.MeshGroupTF_Main; }
			else if(Editor.Select.Bone != null)					{ selectedMainObj = Editor.Select.Bone; }

			if (selectedMainObj != null)
			{
				if (Editor.IsAutoScrollableWhenClickObject_Animation(selectedMainObj, true))
				{
					//스크롤 가능한 상황인지 체크하고
					//자동 스크롤을 요청한다.
					Editor.AutoScroll_HierarchyAnimation(selectedMainObj);
				}
			}
			

			Editor.RefreshControllerAndHierarchy(true);
		}



		//모든 객체를 선택한다.
		private void OnSelectAll(bool isMeshTransforms)
		{
			apMeshGroup curMeshGroup = null;
			if(Editor.Select.AnimClip != null)
			{
				curMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			}

			if(_editor == null
				|| _editor._portrait == null
				|| curMeshGroup == null)
			{
				return;
			}

			//모든 객체를 돌면서 선택하자

			//일단 선택을 초기화
			Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);

			//단, 리깅, 물리 모디파이어에서는 전체 선택이 지원되지 않는다.
			if(Editor.Select.Modifier != null)
			{
				if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					return;
				}
				else if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					return;
				}
			}

			Editor.Select.SelectAllSubObjects(isMeshTransforms);

			Editor.RefreshControllerAndHierarchy(true);
		}





		// GUI
		//---------------------------------------------
		//Hierarchy 레이아웃 출력
		public int GUI_RenderHierarchy_Transform(int width, Vector2 scroll, int scrollLayoutHeight)
		{
			//레이아웃 이벤트일때 GUI요소 갱신
			bool isGUIEvent = (Event.current.type == EventType.Layout);

			_curUnitPosY = 0;
			int maxLevel = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			for (int i = 0; i < _units_Root_Transform.Count; i++)
			{
				int curLevel = GUI_RenderUnit(_units_Root_Transform[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent);
				if (curLevel > maxLevel)
				{
					maxLevel = curLevel;
				}
				GUILayout.Space(10);
			}

			return maxLevel;
		}

		public int GUI_RenderHierarchy_Bone(int width, Vector2 scroll, int scrollLayoutHeight)
		{
			//레이아웃 이벤트일때 GUI요소 갱신
			bool isGUIEvent = (Event.current.type == EventType.Layout);

			_curUnitPosY = 0;
			int maxLevel = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			for (int i = 0; i < _units_Root_Bone.Count; i++)
			{
				int curLevel = GUI_RenderUnit(_units_Root_Bone[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent);
				if (curLevel > maxLevel)
				{
					maxLevel = curLevel;
				}
				GUILayout.Space(10);
			}

			return maxLevel;
		}

		public int GUI_RenderHierarchy_ControlParam(int width, Vector2 scroll, int scrollLayoutHeight)
		{
			//레이아웃 이벤트일때 GUI요소 갱신
			bool isGUIEvent = (Event.current.type == EventType.Layout);

			_curUnitPosY = 0;
			int maxLevel = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			for (int i = 0; i < _units_Root_ControlParam.Count; i++)
			{
				int curLevel = GUI_RenderUnit(_units_Root_ControlParam[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent);
				if (curLevel > maxLevel)
				{
					maxLevel = curLevel;
				}
				GUILayout.Space(10);
			}

			return maxLevel;
		}

		//재귀적으로 Hierarchy 레이아웃을 출력
		//Child에 진입할때마다 Level을 높인다. (여백과 Fold의 기준이 됨)
		private int GUI_RenderUnit(apEditorHierarchyUnit unit, int level, int width, Vector2 scroll, int scrollLayoutHeight, bool isGUIEvent)
		{	
			int maxLevel = level;
			//unit.GUI_Render(_curUnitPosY, level * 10, width, scroll, scrollLayoutHeight, isGUIEvent, level);//이전
			unit.GUI_Render(_curUnitPosY, width, scroll, scrollLayoutHeight, isGUIEvent, level);//변경
			
			//_curUnitPosY += 20;
			_curUnitPosY += apEditorHierarchyUnit.HEIGHT;

			if (unit.IsFoldOut)
			{
				if (unit._childUnits.Count > 0)
				{
					for (int i = 0; i < unit._childUnits.Count; i++)
					{
						//재귀적으로 호출
						int curLevel = GUI_RenderUnit(unit._childUnits[i], level + 1, width, scroll, scrollLayoutHeight, isGUIEvent);
						if (curLevel > maxLevel)
						{
							maxLevel = curLevel;
						}
					}

					//추가 > 자식 Unit을 호출한 이후에는 약간 갭을 두자
					//GUILayout.Space(2);
				}
			}
			return maxLevel;
		}

		//--------------------------------------------------------------------------------
		public void FindCursor_Transform(out object curObject, out object prevObject, out object nextObject)
		{
			FindCursorByType(_units_Root_Transform, out curObject, out prevObject, out nextObject);
		}

		public void FindCursor_Bone(out object curObject, out object prevObject, out object nextObject)
		{
			FindCursorByType(_units_Root_Bone, out curObject, out prevObject, out nextObject);
		}

		public void FindCursor_ControlParam(out object curObject, out object prevObject, out object nextObject)
		{
			FindCursorByType(_units_Root_ControlParam, out curObject, out prevObject, out nextObject);
		}

		private void FindCursorByType(List<apEditorHierarchyUnit> rootUnits, out object curObject, out object prevObject, out object nextObject)
		{
			curObject = null;
			prevObject = null;
			nextObject = null;

			if(rootUnits == null || rootUnits.Count == 0)
			{
				return;
			}

			apEditorHierarchyUnit selectedUnit_Cur = null;
			apEditorHierarchyUnit selectedUnit_Prev = null;
			apEditorHierarchyUnit selectedUnit_Next = null;

			for (int i = 0; i < rootUnits.Count; i++)
			{
				bool isFind = FindCursor_Recursive(	rootUnits[i], 
													ref selectedUnit_Cur,
													ref selectedUnit_Prev,
													ref selectedUnit_Next);

				if(isFind)
				{
					break;
				}
			}

			//결과 넣어서 종료
			if(selectedUnit_Cur != null)		{ curObject = selectedUnit_Cur._savedObj; }
			if(selectedUnit_Prev != null)		{ prevObject = selectedUnit_Prev._savedObj; }
			if(selectedUnit_Next != null)		{ nextObject = selectedUnit_Next._savedObj; }
		}







		/// <summary>
		/// 재귀적으로 현재, 이전, 다음 선택된 유닛을 찾는다. 모두 찾았다면 true 리턴
		/// </summary>
		private bool FindCursor_Recursive(apEditorHierarchyUnit unit,
											ref apEditorHierarchyUnit selectedUnit_Cur,
											ref apEditorHierarchyUnit selectedUnit_Prev,
											ref apEditorHierarchyUnit selectedUnit_Next)
		{
			//이미 3개의 유닛을 찾았다면 리턴
			if(selectedUnit_Cur != null
				&& selectedUnit_Prev != null
				&& selectedUnit_Next != null)
			{
				return true;
			}

			if(unit.IsSelected)
			{
				//이 유닛이 선택되었을 때
				if(selectedUnit_Cur == null)
				{
					//현재 커서로 갱신
					selectedUnit_Cur = unit;
				}
			}
			else
			{
				//이 유닛이 선택되지 않았다면
				if(selectedUnit_Cur == null)
				{
					//아직 Cur가 선택되기 전이다. > Prev를 갱신하자 + 다음으로 이동
					selectedUnit_Prev = unit;
				}
				else
				{
					//Cur가 선택된 이후다. > Next를 갱신하고 종료한다.
					selectedUnit_Next = unit;
					return true;
				}
			}
			
			if (unit._childUnits.Count > 0)
			{
				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					//재귀적으로 호출
					bool isFindAll = FindCursor_Recursive(	unit._childUnits[i],
															ref selectedUnit_Cur,
															ref selectedUnit_Prev,
															ref selectedUnit_Next);

					if(isFindAll)
					{
						return true;
					}
				}
			}

			return false;//다 못찾고 이 재귀 루틴은 종료
		}



		// 자동 스크롤을 위한 함수들
		//--------------------------------------------------------------------
		public apEditorHierarchyUnit FindUnitByObject(object targetObj, apEditor.RIGHT_LOWER_SCROLL_TYPE scrollType)
		{
			if(targetObj == null) { return null; }

			
			apEditorHierarchyUnit resultUnit = null;

			//1. 오브젝트 매핑에서 찾자
			bool isFindInMap = false;
			switch (scrollType)
			{
				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh:
					isFindInMap = _object2Unit_Transform.TryGetValue(targetObj, out resultUnit);
					break;

				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_Bone:
					isFindInMap = _object2Unit_Bone.TryGetValue(targetObj, out resultUnit);
					break;

				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam:
					isFindInMap = _object2Unit_ControlParam.TryGetValue(targetObj, out resultUnit);
					break;
			}
			if(isFindInMap && resultUnit != null)
			{
				//Debug.Log("오브젝트 매핑에서 찾았다.");
				return resultUnit;
			}

			

			//2. 루트 유닛에서 검색하자
			List<apEditorHierarchyUnit> rootUnits = null;

			switch (scrollType)
			{
				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh:
					rootUnits = _units_Root_Transform;
					break;

				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_Bone:
					rootUnits = _units_Root_Bone;
					break;

				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam:
					rootUnits = _units_Root_ControlParam;
					break;
			}

			if(rootUnits == null)
			{
				return null;
			}

			int nRootUnits = rootUnits != null ? rootUnits.Count : 0;
			if(nRootUnits > 0)
			{
				for (int i = 0; i < nRootUnits; i++)
				{
					resultUnit = FindUnitByObject_Recursive(rootUnits[i], targetObj);
					if(resultUnit != null)
					{
						//찾았당
						return resultUnit;
					}
				}
			}
			return null;
		}




		private apEditorHierarchyUnit FindUnitByObject_Recursive(apEditorHierarchyUnit unit, object targetObj)
		{
			if(unit == null)
			{
				return null;
			}
			if(unit._savedObj != null && unit._savedObj == targetObj)
			{
				//대상을 찾았다.
				return unit;
			}

			//자식에서 찾자
			if(unit._childUnits != null && unit._childUnits.Count > 0)
			{
				apEditorHierarchyUnit result = null;
				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					result = FindUnitByObject_Recursive(unit._childUnits[i], targetObj);
					if(result != null)
					{
						//자식 유닛에서 찾았다.
						return result;
					}
				}
			}

			return null;
		}


		/// <summary>
		/// 입력된 Unit의 PosY 위치를 계산한다. Hierarchy 렌더링과 같은 방식으로 동작한다.
		/// </summary>
		public int CalculateUnitPosY(apEditorHierarchyUnit targetUnit, apEditor.RIGHT_LOWER_SCROLL_TYPE scrollType, out bool result)
		{
			int curUnitPosY = 0;
			result = false;

			List<apEditorHierarchyUnit> rootUnits = null;
			
			switch (scrollType)
			{
				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_Mesh:
					rootUnits = _units_Root_Transform;
					break;

				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_Bone:
					rootUnits = _units_Root_Bone;
					break;

				case apEditor.RIGHT_LOWER_SCROLL_TYPE.Anim_ControlParam:
					rootUnits = _units_Root_ControlParam;
					break;
			}

			if(rootUnits == null)
			{
				result = false;
				return 0;
			}

			int nRootUnits = rootUnits != null ? rootUnits.Count : 0;
			if(nRootUnits == 0)
			{
				result = false;
				return 0;
			}

			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			for (int i = 0; i < nRootUnits; i++)
			{
				bool isFind = CalculateUnitPosY_Recursive(rootUnits[i], targetUnit, ref curUnitPosY);

				if(isFind)
				{
					//해당 유닛을 찾았다.
					result = true;
					return curUnitPosY;
				}

				curUnitPosY += 10;//여백
			}

			//못찾았당..
			result = false;
			return curUnitPosY;
		}

		private bool CalculateUnitPosY_Recursive(	apEditorHierarchyUnit curUnit,
													apEditorHierarchyUnit targetUnit,
													ref int curUnitPosY)
		{
			if(curUnit == null)
			{
				return false;
			}

			//타겟을 찾았다면 true 리턴
			if(curUnit == targetUnit)
			{
				return true;
			}

			//Height 만큼 커서 위치 이동
			curUnitPosY += apEditorHierarchyUnit.HEIGHT;

			//자식 Unit 계산
			if (curUnit.IsFoldOut)
			{
				if (curUnit._childUnits != null && curUnit._childUnits.Count > 0)
				{
					for (int i = 0; i < curUnit._childUnits.Count; i++)
					{
						//재귀적으로 호출
						bool isFind = CalculateUnitPosY_Recursive(	curUnit._childUnits[i],
																	targetUnit,
																	ref curUnitPosY);

						if(isFind)
						{
							//자식 유닛에서 대상을 찾았다면 더 찾지 않는다.
							return true;
						}
					}
				}
			}

			return false;
		}
	}

}