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
using UnityEngine.Windows.Speech;

namespace AnyPortrait
{

	public class apEditorMeshGroupHierarchy
	{
		// Members
		//---------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }



		public enum CATEGORY
		{
			MainName,
			//Transform,
			//Mesh_Name,
			Mesh_Item,
			//Mesh_Load,
			//MeshGroup_Name,
			MeshGroup_Item,
			//MeshGroup_Load,

			MainName_Bone,
			SubName_Bone,
			Bone_Item
		}

		//루트들만 따로 적용
		//private apEditorHierarchyUnit _rootUnit_Mesh = null;
		//private apEditorHierarchyUnit _rootUnit_MeshGroup = null;

		//Mesh / Bone으로 나눔
		private apEditorHierarchyUnit _rootUnit_Meshes = null;
		private apEditorHierarchyUnit _rootUnit_Bones_Main = null;
		private List<apEditorHierarchyUnit> _rootUnit_Bones_Sub = new List<apEditorHierarchyUnit>();

		public List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Meshes = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root_Bones = new List<apEditorHierarchyUnit>();

		private Dictionary<object, apEditorHierarchyUnit> _object2Unit_Meshes = new Dictionary<object, apEditorHierarchyUnit>();
		private Dictionary<object, apEditorHierarchyUnit> _object2Unit_Bones = new Dictionary<object, apEditorHierarchyUnit>();

		//추가 20.7.4 : 다중 선택 위해서 마지막으로 선택된 Unit을 기록하자
		private apEditorHierarchyUnit _lastClickedUnit = null;//마지막으로 클릭한 유닛 (클릭한 경우만 유효)
		private List<apEditorHierarchyUnit> _shiftClickedUnits = new List<apEditorHierarchyUnit>();//Shift로 클릭했을때 결과로 나올 추가 선택 유닛들



		//개선 20.3.28 : Pool을 이용하자
		private apEditorHierarchyUnitPool _unitPool = new apEditorHierarchyUnitPool();

		public enum HIERARCHY_TYPE
		{
			Meshes, Bones
		}



		//추가 21.6.12 : 우클릭에 대한 메뉴 멤버 추가
		private apEditorHierarchyMenu _rightMenu = null;
		private object _loadKey_Rename = null;//이름 변경 다이얼로그의 유효성 테스트를 위한 로드키 
		private object _loadKey_Search = null;
		private object _loadKey_DuplicateBone = null;
		private apMeshGroup _requestMeshGroup = null;//팝업이 떴을 때의 MeshGroup



		//Visible Icon에 대한 GUIContent
		private apGUIContentWrapper _guiContent_Visible_Current = null;
		private apGUIContentWrapper _guiContent_NonVisible_Current = null;

		private apGUIContentWrapper _guiContent_Visible_TmpWork = null;
		private apGUIContentWrapper _guiContent_NonVisible_TmpWork = null;

		private apGUIContentWrapper _guiContent_Visible_Rule = null;
		private apGUIContentWrapper _guiContent_NonVisible_Rule = null;

		private apGUIContentWrapper _guiContent_Visible_Default = null;
		private apGUIContentWrapper _guiContent_NonVisible_Default = null;

		private apGUIContentWrapper _guiContent_Visible_ModKey = null;
		private apGUIContentWrapper _guiContent_NonVisible_ModKey = null;

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
		public apEditorMeshGroupHierarchy(apEditor editor)
		{
			_editor = editor;

			if(_rootUnit_Bones_Sub == null) { _rootUnit_Bones_Sub = new List<apEditorHierarchyUnit>(); }
			_rootUnit_Bones_Sub.Clear();
			

			if(_units_All == null) { _units_All = new List<apEditorHierarchyUnit>(); }
			_units_All.Clear();

			if (_units_Root_Meshes == null) { _units_Root_Meshes = new List<apEditorHierarchyUnit>(); }
			_units_Root_Meshes.Clear();
			
			if(_units_Root_Bones == null) { _units_Root_Bones = new List<apEditorHierarchyUnit>(); }
			_units_Root_Bones.Clear();

			if(_object2Unit_Meshes == null) { _object2Unit_Meshes = new Dictionary<object, apEditorHierarchyUnit>(); }
			_object2Unit_Meshes.Clear();

			if (_object2Unit_Bones == null) { _object2Unit_Bones = new Dictionary<object, apEditorHierarchyUnit>(); }
			_object2Unit_Bones.Clear();



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
			if (_guiContent_Visible_Current == null)		{ _guiContent_Visible_Current =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current)); }
			if (_guiContent_NonVisible_Current == null)		{ _guiContent_NonVisible_Current =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current)); }
			if (_guiContent_Visible_TmpWork == null)		{ _guiContent_Visible_TmpWork =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_TmpWork)); }
			if (_guiContent_NonVisible_TmpWork == null)		{ _guiContent_NonVisible_TmpWork =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_TmpWork)); }
			if (_guiContent_Visible_Rule == null)			{ _guiContent_Visible_Rule =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Rule)); }
			if (_guiContent_NonVisible_Rule == null)		{ _guiContent_NonVisible_Rule =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Rule)); }
			if (_guiContent_Visible_Default == null)		{ _guiContent_Visible_Default =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Default)); }
			if (_guiContent_NonVisible_Default == null)		{ _guiContent_NonVisible_Default =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Default)); }
			if (_guiContent_Visible_ModKey == null)			{ _guiContent_Visible_ModKey =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_ModKey)); }
			if (_guiContent_NonVisible_ModKey == null)		{ _guiContent_NonVisible_ModKey =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_ModKey)); }
			if (_guiContent_NoKey == null)					{ _guiContent_NoKey =				apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NoKey)); }
			if (_guiContent_NoKeyDisabled == null)			{ _guiContent_NoKeyDisabled =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NoKeyDisabled)); }

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
			_units_Root_Meshes.Clear();
			_units_Root_Bones.Clear();

			_rootUnit_Meshes = null;
			_rootUnit_Bones_Main = null;
			_rootUnit_Bones_Sub.Clear();

			_object2Unit_Meshes.Clear();
			_object2Unit_Bones.Clear();

			//추가 20.7.4 : 다중 선택용 변수
			_lastClickedUnit = null;

			

			//_rootUnit_Mesh = AddUnit_Label(null, "Child Meshes", CATEGORY.Mesh_Name, null, true, null);
			//_rootUnit_MeshGroup = AddUnit_Label(null, "Child Mesh Groups", CATEGORY.MeshGroup_Name, null, true, null);

			if (Editor == null || Editor._portrait == null || Editor.Select.MeshGroup == null)
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

			string meshGroupName = Editor.Select.MeshGroup._name;
			if(meshGroupName.Length > 16)
			{
				meshGroupName = meshGroupName.Substring(0, 14) + "..";
			}
			_rootUnit_Meshes = AddUnit_Label(null, meshGroupName, CATEGORY.MainName, null, true, null, HIERARCHY_TYPE.Meshes);
			_rootUnit_Bones_Main = AddUnit_Label(null, meshGroupName, CATEGORY.MainName_Bone, null, true, null, HIERARCHY_TYPE.Bones);

			//추가 19.6.29 : RestoreTmpWorkVisible 버튼을 추가하자.
			//_rootUnit_Meshes.SetRestoreTmpWorkVisible(	Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON), 
			//											Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF),
			//											OnUnitClickRestoreTmpWork_Mesh);

			_rootUnit_Meshes.SetRestoreTmpWorkVisible(	_guiContent_RestoreTmpWorkVisible_ON, 
														_guiContent_RestoreTmpWorkVisible_OFF,
														OnUnitClickRestoreTmpWork_Mesh);

			_rootUnit_Meshes.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Meshes);

			//_rootUnit_Bones_Main.SetRestoreTmpWorkVisible(	Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON), 
			//												Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF),
			//												OnUnitClickRestoreTmpWork_Bone);

			_rootUnit_Bones_Main.SetRestoreTmpWorkVisible(	_guiContent_RestoreTmpWorkVisible_ON, 
															_guiContent_RestoreTmpWorkVisible_OFF,
															OnUnitClickRestoreTmpWork_Bone);


			_rootUnit_Bones_Main.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Bones);



			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			

			//수정
			if(meshGroup._boneListSets.Count > 0)
			{
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
				{
					boneSet = meshGroup._boneListSets[iSet];
					if(boneSet._isRootMeshGroup)
					{
						//Root MeshGroup의 Bone이면 패스
						continue;
					}

					//Bone을 가지고 있는 Child MeshGroup Transform을 Sub 루트로 삼는다.
					//나중에 구분하기 위해 meshGroupTransform을 SavedObj에 넣는다.
					_rootUnit_Bones_Sub.Add(
						AddUnit_Label(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px),//변경 22.6.10 : 16px로 변경
										boneSet._meshGroupTransform._nickName,
										CATEGORY.SubName_Bone,
										boneSet._meshGroupTransform, //<Saved Obj
										true, null,
										HIERARCHY_TYPE.Bones));
				}
				
			}

			//List<apTransform_Mesh> childMeshTransforms = Editor.Select.MeshGroup._childMeshTransforms;
			//List<apTransform_MeshGroup> childMeshGroupTransforms = Editor.Select.MeshGroup._childMeshGroupTransforms;

			//구버전 코드
			#region [미사용 코드]
			////> 재귀적인 Hierarchy를 허용하지 않는다.
			//for (int i = 0; i < childMeshTransforms.Count; i++)
			//{
			//	apTransform_Mesh meshTransform = childMeshTransforms[i];
			//	Texture2D iconImage = null;
			//	if(meshTransform._isClipping_Child)
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			//	}
			//	else
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//	}

			//	AddUnit_ToggleButton_Visible(	iconImage, 
			//									meshTransform._nickName, 
			//									CATEGORY.Mesh_Item, 
			//									meshTransform, 
			//									false, 
			//									//_rootUnit_Mesh,
			//									_rootUnit,
			//									meshTransform._isVisible_Default);
			//}

			//for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			//{
			//	apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];
			//	AddUnit_ToggleButton_Visible(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
			//									meshGroupTransform._nickName, 
			//									CATEGORY.MeshGroup_Item, 
			//									meshGroupTransform, 
			//									false, 
			//									//_rootUnit_MeshGroup,
			//									_rootUnit,
			//									meshGroupTransform._isVisible_Default);
			//} 
			#endregion

			//변경된 코드
			//재귀적인 구조를 만들고 싶다면 여길 수정하자
			AddTransformOfMeshGroup(Editor.Select.MeshGroup, Editor.Select.MeshGroup, _rootUnit_Meshes);

			//Bone도 만들어주자
			//Main과 Sub 모두
			AddBoneUnitsOfMeshGroup(meshGroup, _rootUnit_Bones_Main);

			if (_rootUnit_Bones_Sub.Count > 0)
			{
				for (int i = 0; i < _rootUnit_Bones_Sub.Count; i++)
				{
					apTransform_MeshGroup mgTranform = _rootUnit_Bones_Sub[i]._savedObj as apTransform_MeshGroup;
					if (mgTranform != null && mgTranform._meshGroup != null)
					{
						AddBoneUnitsOfMeshGroup(mgTranform._meshGroup, _rootUnit_Bones_Sub[i]);
					}
				}
			}


			//추가 20.7.4 : 다중 선택을 위한 LinearIndex를 갱신하자
			RefreshLinearIndices();
		}


		//추가 21.8.7 : 리소스를 다시 로딩할 경우 유닛 풀의 모든 유닛들의 리소스도 다시 로드해야한다.
		public void ReloadUnitResources()
		{
			_unitPool.CheckAndReInitUnit();
		}


		// Functions
		//------------------------------------------------------------------------

		private apEditorHierarchyUnit AddUnit_Label(Texture2D icon,
														string text,
														CATEGORY savedKey,
														object savedObj,
														bool isRoot,
														apEditorHierarchyUnit parent,
														HIERARCHY_TYPE hierarchyType)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent로 변경
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);
			
			if(hierarchyType == HIERARCHY_TYPE.Meshes)
			{
				newUnit.SetEvent(OnUnitClick, OnCheckSelectedHierarchy_TFList);
			}
			else
			{
				newUnit.SetEvent(OnUnitClick, OnCheckSelectedHierarchy_BoneList);
			}
			
			newUnit.SetLabel(icon, text, (int)savedKey, savedObj);
			newUnit.SetModRegistered(false);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}
			}

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


		private apEditorHierarchyUnit AddUnit_ToggleButton_Visible(Texture2D icon,
																		string text,
																		CATEGORY savedKey,
																		object savedObj,
																		bool isRoot,
																		apEditorHierarchyUnit parent,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Prefix,
																		apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Postfix,
																		bool isModRegisted,
																		HIERARCHY_TYPE hierarchyType,
																		bool isRefreshVisiblePrefixWhenRender,
																		RIGHTCLICK_MENU rightClickSupported)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));


			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent로 변경
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

			if(hierarchyType == HIERARCHY_TYPE.Meshes)
			{
				newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_TFList);
			}
			else
			{
				newUnit.SetEvent(OnUnitClick, OnUnitVisibleClick, OnCheckSelectedHierarchy_BoneList);
			}
			
			newUnit.SetToggleButton_Visible(icon, text, (int)savedKey, savedObj, true, visibleType_Prefix, visibleType_Postfix);

			newUnit.SetModRegistered(isModRegisted);

			//추가 21.6.13 : 우클릭 지원
			if(rightClickSupported == RIGHTCLICK_MENU.Enabled)
			{
				newUnit.SetRightClickEvent(OnUnitRightClick);
			}

			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}
			}


			//추가 [1.4.2] 오브젝트 매핑
			if(savedObj != null)
			{
				if(hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					if(!_object2Unit_Meshes.ContainsKey(savedObj))
					{
						_object2Unit_Meshes.Add(savedObj, newUnit);
					}
				}
				else
				{
					if(!_object2Unit_Bones.ContainsKey(savedObj))
					{
						_object2Unit_Bones.Add(savedObj, newUnit);
					}
				}
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		private apEditorHierarchyUnit AddUnit_ToggleButton(Texture2D icon,
																		string text,
																		CATEGORY savedKey,
																		object savedObj,
																		bool isRoot, bool isModRegisted,
																		apEditorHierarchyUnit parent,
																		HIERARCHY_TYPE hierarchyType,
																		RIGHTCLICK_MENU rightClickSupported)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//변경 19.11.16 : GUIContent로 변경
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);

			if(hierarchyType == HIERARCHY_TYPE.Meshes)
			{
				newUnit.SetEvent(OnUnitClick, OnCheckSelectedHierarchy_TFList);
			}
			else
			{
				newUnit.SetEvent(OnUnitClick, OnCheckSelectedHierarchy_BoneList);
			}
			
			newUnit.SetToggleButton(icon, text, (int)savedKey, savedObj, true);

			newUnit.SetModRegistered(isModRegisted);


			//추가 21.6.13 : 우클릭 지원
			if(rightClickSupported == RIGHTCLICK_MENU.Enabled)
			{
				newUnit.SetRightClickEvent(OnUnitRightClick);
			}


			_units_All.Add(newUnit);
			if (isRoot)
			{
				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					_units_Root_Meshes.Add(newUnit);
				}
				else
				{
					_units_Root_Bones.Add(newUnit);
				}

			}

			//추가 [1.4.2] 오브젝트 매핑
			if(savedObj != null)
			{
				if(hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					if(!_object2Unit_Meshes.ContainsKey(savedObj))
					{
						_object2Unit_Meshes.Add(savedObj, newUnit);
					}
				}
				else
				{
					if(!_object2Unit_Bones.ContainsKey(savedObj))
					{
						_object2Unit_Bones.Add(savedObj, newUnit);
					}
				}
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}
			return newUnit;
		}


		// 추가 : Transform 리스트를 재귀적으로 만든다.
		//-----------------------------------------------------------------------------------------
		private void AddTransformOfMeshGroup(apMeshGroup rootMeshGroup, apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			bool isModRegisted = false;
			apModifierBase modifier = Editor.Select.Modifier;

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

				isModRegisted = false;
				if (modifier != null)
				{
					isModRegisted = IsModRegistered(meshTransform);
				}

				//apEditorHierarchyUnit.VISIBLE_ICON_TYPE visibleType = (meshTransform._isVisible_Default) ? apEditorHierarchyUnit.VISIBLE_ICON_TYPE.Visible : apEditorHierarchyUnit.VISIBLE_ICON_TYPE.NonVisible;



				AddUnit_ToggleButton_Visible(	iconImage,
												meshTransform._nickName,
												CATEGORY.Mesh_Item,
												meshTransform,
												false,
												//_rootUnit_Mesh,
												parentUnit,
												GetVisibleIconType(/*rootMeshGroup, */meshTransform, isModRegisted, true),
												GetVisibleIconType(/*rootMeshGroup, */meshTransform, isModRegisted, false),
												isModRegisted,
												HIERARCHY_TYPE.Meshes,
												true,
												RIGHTCLICK_MENU.Enabled
												);


			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				isModRegisted = false;
				if (modifier != null)
				{
					isModRegisted = IsModRegistered(meshGroupTransform);
				}

				apEditorHierarchyUnit newUnit = AddUnit_ToggleButton_Visible(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px),//변경 22.6.10 : 16px로 변경
													meshGroupTransform._nickName,
													CATEGORY.MeshGroup_Item,
													meshGroupTransform,
													false,
													//_rootUnit_MeshGroup,
													parentUnit,
													//meshGroupTransform._isVisible_Default,
													GetVisibleIconType(/*rootMeshGroup, */meshGroupTransform, isModRegisted, true),
													GetVisibleIconType(/*rootMeshGroup, */meshGroupTransform, isModRegisted, false),
													isModRegisted,
													HIERARCHY_TYPE.Meshes,
													true,
													RIGHTCLICK_MENU.Enabled);


				if (meshGroupTransform._meshGroup != null)
				{
					AddTransformOfMeshGroup(rootMeshGroup, meshGroupTransform._meshGroup, newUnit);
				}
			}
		}


		// 본 리스트를 만든다.
		// 본은 Child MeshGroup의 본에 상위의 메시가 리깅되는 경우가 없다.
		// 다만, Transform계열 Modifier (Anim 포함)에서 Child MeshGroup의 Bone을 제어할 수 있다.
		// Root Node를 여러개두자.
		//------------------------------------------------------------------------------------

		private void AddBoneUnitsOfMeshGroup(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit)
		{
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
														HIERARCHY_TYPE.Bones,
														false,
														RIGHTCLICK_MENU.Enabled
														);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				AddBoneUnit(bone._childBones[i], addedUnit, iconNormal, iconIKHead, iconIKChained, iconIKSingle);
			}
		}

		// Refresh (without Reset)
		//-----------------------------------------------------------------------------------------
		public void RefreshUnits()
		{
			if (Editor == null || Editor._portrait == null || Editor.Select.MeshGroup == null)
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


			#region [미사용 코드] 재귀적인 구조가 없는 기존 코드
			//1. 메시 들을 검색하자
			//구버전 : 단일 레벨의 Child Transform에 대해서 Refresh
			//List<apTransform_Mesh> childMeshTransforms = Editor.Select.MeshGroup._childMeshTransforms;
			//List<apTransform_MeshGroup> childMeshGroupTransforms = Editor.Select.MeshGroup._childMeshGroupTransforms;

			//for (int i = 0; i < childMeshTransforms.Count; i++)
			//{
			//	apTransform_Mesh meshTransform = childMeshTransforms[i];
			//	Texture2D iconImage = null;
			//	if(meshTransform._isClipping_Child)
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Clipping);
			//	}
			//	else
			//	{
			//		iconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//	}
			//	RefreshUnit(	CATEGORY.Mesh_Item, 
			//					iconImage, 
			//					meshTransform, 
			//					meshTransform._nickName,
			//					Editor.Select.SubMeshInGroup, 
			//					meshTransform._isVisible_Default,
			//					//_rootUnit_Mesh
			//					_rootUnit
			//					);
			//}

			//CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);

			////2. Mesh Group들을 검색하자
			//for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			//{
			//	apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];
			//	RefreshUnit(	CATEGORY.MeshGroup_Item, 
			//					Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup), 
			//					meshGroupTransform, 
			//					meshGroupTransform._nickName,
			//					Editor.Select.SubMeshGroupInGroup, 
			//					meshGroupTransform._isVisible_Default,
			//					//_rootUnit_MeshGroup
			//					_rootUnit
			//					);
			//}

			//CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms); 
			#endregion



			
			//신버전 : 재귀적으로 탐색 및 갱신을 한다.
			List<apTransform_Mesh> childMeshTransforms = new List<apTransform_Mesh>();
			List<apTransform_MeshGroup> childMeshGroupTransforms = new List<apTransform_MeshGroup>();

			SearchMeshGroupTransforms(Editor.Select.MeshGroup, Editor.Select.MeshGroup, _rootUnit_Meshes, childMeshTransforms, childMeshGroupTransforms);

			//Debug.Log(">>> Hierarchy 갱신 완료 > 검색된 MeshTF : " + childMeshTransforms.Count + " / MeshGroupTF : " + childMeshGroupTransforms.Count);



			CheckRemovableUnits<apTransform_Mesh>(deletedUnits, CATEGORY.Mesh_Item, childMeshTransforms);
			CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, childMeshGroupTransforms);

			//본도 Refresh한다.
			//본의 경우 Child MeshGroup Transform에 속한 Bone도 있으므로, 이 ChildMeshGroup이 현재도 유효한지 체크하는 것이 중요하다

			List<apBone> resultBones = new List<apBone>();
			List<apTransform_MeshGroup> resultMeshGroupTransformWithBones = new List<apTransform_MeshGroup>();

			
			//일단 메인부터
			SearchBones(Editor.Select.MeshGroup, _rootUnit_Bones_Main, resultBones);

			//<BONE_EDIT>
			////서브는 Child MeshGroup Transform 부터 체크한다.
			//List<apTransform_MeshGroup> childMeshGroupTransformWithBones = Editor.Select.MeshGroup._childMeshGroupTransformsWithBones;

			//>> Bone Set으로 변경
			List<apMeshGroup.BoneListSet> subBoneSetList = Editor.Select.MeshGroup._boneListSets;
			List<apTransform_MeshGroup> subMeshGroups = new List<apTransform_MeshGroup>();
			if(subBoneSetList != null && subBoneSetList.Count > 0)
			{
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < subBoneSetList.Count; iSet++)
				{
					boneSet = subBoneSetList[iSet];
					if(boneSet._isRootMeshGroup)
					{
						continue;
					}
					subMeshGroups.Add(boneSet._meshGroupTransform);
				}
			}

			for (int i = 0; i < _rootUnit_Bones_Sub.Count; i++)
			{
				apEditorHierarchyUnit subBoneRootUnit = _rootUnit_Bones_Sub[i];
				apTransform_MeshGroup mgTranform = subBoneRootUnit._savedObj as apTransform_MeshGroup;
				if (mgTranform != null && mgTranform._meshGroup != null 
					//&& childMeshGroupTransformWithBones.Contains(mgTranform)//<BONE_EDIT>
					&& subMeshGroups.Contains(mgTranform)//Bone Set에서 추출
					)
				{
					//유효하게 포함되어있는 mgTransform이네염
					resultMeshGroupTransformWithBones.Add(mgTranform);
					SearchBones(mgTranform._meshGroup, subBoneRootUnit, resultBones);
				}
			}

			CheckRemovableUnits<apTransform_MeshGroup>(deletedUnits, CATEGORY.SubName_Bone, resultMeshGroupTransformWithBones);
			CheckRemovableUnits<apBone>(deletedUnits, CATEGORY.Bone_Item, resultBones);


			//Debug.Log("Refresh Unit > 삭제 : " + deletedUnits.Count);
			for (int i = 0; i < deletedUnits.Count; i++)
			{
				//1. 먼저 All에서 없앤다.
				//2. Parent가 있는경우,  Parent에서 없애달라고 한다.
				apEditorHierarchyUnit dUnit = deletedUnits[i];
				if (dUnit._parentUnit != null)
				{
					dUnit._parentUnit._childUnits.Remove(dUnit);
				}

				//Debug.Log("-" + dUnit._text);
				_units_All.Remove(dUnit);

				//[1.4.2] 오브젝트매핑에서도 삭제
				if(dUnit._savedObj != null)
				{
					_object2Unit_Meshes.Remove(dUnit._savedObj);
					_object2Unit_Bones.Remove(dUnit._savedObj);
				}

				//추가 20.3.18 : 그냥 삭제하면 안되고, Pool에 반납해야 한다.
				_unitPool.PushUnit(dUnit);
			}

			//전체 Sort를 한다.
			//재귀적으로 실행
			for (int i = 0; i < _units_Root_Meshes.Count; i++)
			{
				SortUnit_Recv(_units_Root_Meshes[i]);
			}

			//추가 21.2.9 : Sub List는 MeshGroupTF의 순서대로 표시해야한다.
			_units_Root_Bones.Sort(delegate(apEditorHierarchyUnit a, apEditorHierarchyUnit b)
			{
				if ((CATEGORY)(a._savedKey) == CATEGORY.MainName_Bone)
				{
					return -1;
				}
				if ((CATEGORY)(b._savedKey) == CATEGORY.MainName_Bone)
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

			for (int i = 0; i < _units_Root_Bones.Count; i++)
			{
				SortUnit_Recv_Bones(_units_Root_Bones[i]);
			}

			//추가 19.6.29 : TmpWorkVisible 확인
			_rootUnit_Meshes.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Meshes);
			_rootUnit_Bones_Main.SetRestoreTmpWorkVisibleAnyChanged(Editor.Select.IsTmpWorkVisibleChanged_Bones);

			//추가 20.7.4 : 다중 선택을 위한 LinearIndex를 갱신하자
			RefreshLinearIndices();
		}

		private apEditorHierarchyUnit RefreshUnit(CATEGORY category,
													Texture2D iconImage,
													object obj, string objName, 
													object selectedObj, 
													apSelection.FUNC_IS_SUB_SELECTED funcIsSubSelected,//추가 20.5.28 : 다중 선택도 체크한다.
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Prefix,
													apEditorHierarchyUnit.VISIBLE_TYPE visibleType_Postfix,
													bool isModRegistered,
													bool isAvailable,
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

						if(a._savedObj is apTransform_Mesh && obj is apTransform_Mesh)
						{
							if((a._savedObj as apTransform_Mesh)._transformUniqueID == (obj as apTransform_Mesh)._transformUniqueID)
							{
								//Debug.Log("찾음 : 레퍼런스는 다르지만 Mesh TF ID가 동일하다.");
								return true;
							}
						}
						else if(a._savedObj is apTransform_MeshGroup && obj is apTransform_MeshGroup)
						{
							if((a._savedObj as apTransform_MeshGroup)._transformUniqueID == (obj as apTransform_MeshGroup)._transformUniqueID)
							{
								//Debug.Log("찾음 : 레퍼런스는 다르지만 MeshGroup TF ID가 동일하다.");
								return true;
							}
						}
						else if(a._savedObj is apBone && obj is apBone)
						{
							if((a._savedObj as apBone)._uniqueID == (obj as apBone)._uniqueID)
							{
								//Debug.Log("찾음 : 레퍼런스는 다르지만 Bone ID가 동일하다.");
								return true;
							}
						}
					}
					return false;
					
					//return (CATEGORY)a._savedKey == category && a._savedObj == obj;
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
					//unit._isSelected = false;

					//추가 20.5.28 : 다중 선택도 허용한다.
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

				unit.SetModRegistered(isModRegistered);
				unit.SetAvailable(isAvailable);

				unit._visibleType_Prefix = visibleType_Prefix;
				unit._visibleType_Postfix = visibleType_Postfix;

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
			}
			else
			{	

				if (hierarchyType == HIERARCHY_TYPE.Meshes)
				{
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Prefix, visibleType_Postfix, isModRegistered, HIERARCHY_TYPE.Meshes, true, RIGHTCLICK_MENU.Enabled);
				}
				else
				{
					//unit = AddUnit_ToggleButton(iconImage, objName, category, obj, false, isModRegistered, parentUnit, HIERARCHY_TYPE.Bones);
					unit = AddUnit_ToggleButton_Visible(iconImage, objName, category, obj, false, parentUnit, visibleType_Prefix, visibleType_Postfix, isModRegistered, HIERARCHY_TYPE.Bones, false, RIGHTCLICK_MENU.Enabled);
					
				}
				if (selectedObj == obj)
				{
					//unit._isSelected = true;
					unit.SetSelected(true, true);
				}
				unit.SetAvailable(isAvailable);
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

		/// <summary>
		/// 재귀적으로 검색을 하여 존재하는 Transform인지 찾고, Refresh 또는 제거 리스트에 넣는다.
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="parentUnit"></param>
		/// <param name="resultMeshTransforms"></param>
		/// <param name="resultMeshGroupTransforms"></param>
		private void SearchMeshGroupTransforms(apMeshGroup rootMeshGroup, apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apTransform_Mesh> resultMeshTransforms, List<apTransform_MeshGroup> resultMeshGroupTransforms)
		{
			List<apTransform_Mesh> childMeshTransforms = targetMeshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroupTransforms = targetMeshGroup._childMeshGroupTransforms;

			bool isModRegistered = false;
			apModifierBase modifier = Editor.Select.Modifier;

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

				isModRegistered = false;
				if (modifier != null)
				{
					isModRegistered = IsModRegistered(meshTransform);
				}


				RefreshUnit(CATEGORY.Mesh_Item,
								iconImage,
								meshTransform,
								meshTransform._nickName,
								//Editor.Select.SubMeshInGroup, 
								Editor.Select.MeshTF_Main,
								Editor.Select.IsSubSelected,
								//meshTransform._isVisible_Default,
								GetVisibleIconType(/*rootMeshGroup, */meshTransform, isModRegistered, true),
								GetVisibleIconType(/*rootMeshGroup, */meshTransform, isModRegistered, false),
								isModRegistered,
								true,
								//_rootUnit_Mesh
								parentUnit,
								HIERARCHY_TYPE.Meshes
								);
			}

			for (int i = 0; i < childMeshGroupTransforms.Count; i++)
			{
				apTransform_MeshGroup meshGroupTransform = childMeshGroupTransforms[i];

				resultMeshGroupTransforms.Add(meshGroupTransform);


				isModRegistered = false;
				if (modifier != null)
				{
					isModRegistered = IsModRegistered(meshGroupTransform);
				}

				apEditorHierarchyUnit existUnit = RefreshUnit(CATEGORY.MeshGroup_Item,
													Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px),//변경 22.6.10 : 16px로 변경
													meshGroupTransform,
													meshGroupTransform._nickName,
													//Editor.Select.SubMeshGroupInGroup, 
													Editor.Select.MeshGroupTF_Main,
													Editor.Select.IsSubSelected,
													//meshGroupTransform._isVisible_Default,
													GetVisibleIconType(/*rootMeshGroup, */meshGroupTransform, isModRegistered, true),
													GetVisibleIconType(/*rootMeshGroup, */meshGroupTransform, isModRegistered, false),
													isModRegistered,
													true,
													//_rootUnit_MeshGroup
													parentUnit,
													HIERARCHY_TYPE.Meshes

													);

				if (meshGroupTransform._meshGroup != null)
				{
					SearchMeshGroupTransforms(rootMeshGroup, meshGroupTransform._meshGroup, existUnit, resultMeshTransforms, resultMeshGroupTransforms);
				}
				//else
				//{
				//	Debug.LogError("meshGroupTransform._meshGroup이 Null : " + meshGroupTransform._nickName);
				//}
			}
		}







		/// <summary>
		/// 재귀적으로 검색을 하여 존재하는 Bone인지 찾고, Refresh 또는 제거 리스트에 넣는다.
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		private void SearchBones(apMeshGroup targetMeshGroup, apEditorHierarchyUnit parentUnit, List<apBone> resultBones)
		{


			List<apBone> rootBones = targetMeshGroup._boneList_Root;

			if (rootBones.Count == 0)
			{
				return;
			}

			//if(Editor.Select.Bone == null)
			//{
			//	Debug.Log("Mesh Group Hierarchy - Bone Refresh [ Not Selected ]");
			//}
			//else
			//{
			//	Debug.Log("Mesh Group Hierarchy - Bone Refresh [" + Editor.Select.Bone._name + "]");
			//}

			Texture2D iconImage_Normal = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			Texture2D iconImage_IKHead = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKHead);
			Texture2D iconImage_IKChained = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKChained);
			Texture2D iconImage_IKSingle = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HierarchyIcon_IKSingle);

			for (int i = 0; i < rootBones.Count; i++)
			{
				apBone rootBone = rootBones[i];

				SearchAndRefreshBone(rootBone, parentUnit, resultBones, iconImage_Normal, iconImage_IKHead, iconImage_IKChained, iconImage_IKSingle);
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
			bool isAvailable = IsBoneAvailable(bone);

			//이전
			//apEditorHierarchyUnit.VISIBLE_TYPE boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;
			//if(bone.IsGUIVisible)
			//{
			//	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible;
			//}

			//변경 21.1.28
			//- 본의 보이기 타입이 많아졌다.
			apEditorHierarchyUnit.VISIBLE_TYPE boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible;

			switch (bone.VisibleIconType)
			{
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Default:	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.Visible_Tmp:		boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Tmp:	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; break;
				case apBone.VISIBLE_COMBINATION_ICON.NonVisible_Rule:	boneVisible = apEditorHierarchyUnit.VISIBLE_TYPE.Rule_NonVisible; break;
			}

			apEditorHierarchyUnit curUnit = RefreshUnit(CATEGORY.Bone_Item,
															icon,
															bone,
															bone._name,
															//Editor.Select.SubMeshInGroup, 
															Editor.Select.Bone,
															Editor.Select.IsSubSelected,
															//true,
															boneVisible,
															apEditorHierarchyUnit.VISIBLE_TYPE.None,
															isModRegisted,
															isAvailable,
															//_rootUnit_Mesh
															parentUnit,
															HIERARCHY_TYPE.Bones
															);

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				SearchAndRefreshBone(bone._childBones[i], curUnit, resultBones, iconNormal,
					iconIKHead, iconIKChained, iconIKSingle);
			}
		}









		private void SortUnit_Recv(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName)
					{
						return -1;
					}

					int depthA = -1;
					int depthB = -1;
					int indexPerParentA = a._indexPerParent;
					int indexPerParentB = b._indexPerParent;

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


		private void SortUnit_Recv_Bones(apEditorHierarchyUnit unit)
		{
			if (unit._childUnits.Count > 0)
			{
				unit._childUnits.Sort(delegate (apEditorHierarchyUnit a, apEditorHierarchyUnit b)
				{
					if ((CATEGORY)(a._savedKey) == CATEGORY.MainName_Bone)
					{
						return 1;
					}
					if ((CATEGORY)(b._savedKey) == CATEGORY.MainName_Bone)
					{
						return -1;
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


					int depthA = -1;
					int depthB = -1;

					if(a._savedObj is apBone)
					{
						apBone bone_a = a._savedObj as apBone;
						depthA = bone_a._depth;
					}

					if(b._savedObj is apBone)
					{
						apBone bone_b = b._savedObj as apBone;
						depthB = bone_b._depth;
					}

					if (depthA == depthB)
					{
						//그 외에는 그냥 문자열 순서로 매기자
						int compare = string.Compare(a._text.ToString(), b._text.ToString());
						if (compare == 0)
						{
							return a._indexPerParent - b._indexPerParent;
						}
						return compare;
					}

					return depthB - depthA;
				});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv_Bones(unit._childUnits[i]);
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
			for (int i = 0; i < _units_Root_Meshes.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(_units_Root_Meshes[i], curIndex);
			}

			curIndex = 0;
			for (int i = 0; i < _units_Root_Bones.Count; i++)
			{
				curIndex = SetLinearIndexRecursive(_units_Root_Bones[i], curIndex);
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
		public bool OnCheckSelectedHierarchy_TFList(apEditorHierarchyUnit unit)
		{
			if(unit == null || unit._savedObj == null)
			{
				return false;
			}

			//현재 선택된 객체와 리스트 타입 비교
			return Editor.LastClickedHierarchy == apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_TF;
		}


		public bool OnCheckSelectedHierarchy_BoneList(apEditorHierarchyUnit unit)
		{
			if(unit == null || unit._savedObj == null)
			{
				return false;
			}

			//현재 선택된 객체와 리스트 타입 비교
			return Editor.LastClickedHierarchy == apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_Bone;
		}


		// Click Event
		//-----------------------------------------------------------------------------------------
		public void OnUnitClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isCtrl, bool isShift)
		{
			if (Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//이전 : 선택한것만 찾아서 나머지 비활성하면 되는 것이었다.
			//apEditorHierarchyUnit selectedUnit = null;

			//변경 20.5.28 : 변경된 내역이 있다면 모두 다시 검토해봐야 한다. (다중 선택 때문에)
			bool isAnyChanged = false;

			//현재 모디파이어가 선택되어 있는가
			//> 리깅 모디파이어가 선택된 상태가 아니라면, Mesh/MeshGroup Transform 선택시 Bone을 null로, 또는 그 반대로 설정해야한다.
			//(리깅 모디파이어는 둘다 선택된 상태에서 작업하기 때문)
			//물리 모디파이어는 다중 선택이 안된다.
			bool isRiggingModifier = false;
			bool isPhysicModifier = false;

			if(Editor.Select.Modifier != null)
			{
				if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					isRiggingModifier = true;
				}
				else if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					isPhysicModifier = true;
				}
			}


			//v1.4.2 : FFD 중에는 적용 여부 확인후 선택해야한다.
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
						apTransform_Mesh meshTransform = savedObj as apTransform_Mesh;
						if (meshTransform != null)
						{
							if(isRiggingModifier)
							{
								//리깅 모디파이어인 경우
								//- 다중 선택 불가.
								//- 메시 1개, 본 1개 따로 선택 가능
								Editor.Select.SelectMeshTF(meshTransform, apSelection.MULTI_SELECT.Main);
							}
							else if(isPhysicModifier)
							{
								//물리 모디파이어인 경우
								//- 다중 선택 불가
								//- TF/Bone 중에서 하나만 선택 가능
								Editor.Select.SelectSubObject(meshTransform, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							else
							{
								//그외
								//- 다중 선택 가능
								//- TF/Bone 중에서 하나만 선택 가능
								
								//추가 20.7.4 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
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
													Editor.Select.SelectSubObject(	shiftMeshTF, null, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
											else if((CATEGORY)shiftUnit._savedKey == CATEGORY.MeshGroup_Item)
											{
												shiftMeshGroupTF = shiftUnit._savedObj as apTransform_MeshGroup;
												if(shiftMeshGroupTF != null)
												{
													Editor.Select.SelectSubObject(	null, shiftMeshGroupTF, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
										}

										_shiftClickedUnits.Clear();

									}	
								}	

								//클릭한걸 추가하자
								Editor.Select.SelectSubObject(meshTransform, null, null, 
																	((isCtrl || isShift) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main),
																	apSelection.TF_BONE_SELECT.Exclusive);

								
							}
							
							//이전 : 선택된게 1개이다.
							//if (Editor.Select.SubMeshInGroup == meshTransform)
							//{
							//	selectedUnit = eventUnit;
							//	isAnyChanged = true;
							//}

							//변경 : 무조건 전체 리셋
							isAnyChanged = true;


							//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_TF);
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apTransform_MeshGroup meshGroupTransform = savedObj as apTransform_MeshGroup;
						if (meshGroupTransform != null)
						{
							if(isRiggingModifier)
							{
								//리깅 모디파이어인 경우
								//- 다중 선택 불가.
								//- 메시 1개, 본 1개 따로 선택 가능
								Editor.Select.SelectMeshGroupTF(meshGroupTransform, apSelection.MULTI_SELECT.Main);
							}
							else if(isPhysicModifier)
							{
								//물리 모디파이어인 경우
								//- 다중 선택 불가
								//- TF/Bone 중에서 하나만 선택 가능
								Editor.Select.SelectSubObject(null, meshGroupTransform, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							else
							{
								//그외
								//- 다중 선택 가능
								//- TF/Bone 중에서 하나만 선택 가능

								//추가 20.7.4 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
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
													Editor.Select.SelectSubObject(	shiftMeshTF, null, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
											else if((CATEGORY)shiftUnit._savedKey == CATEGORY.MeshGroup_Item)
											{
												shiftMeshGroupTF = shiftUnit._savedObj as apTransform_MeshGroup;
												if(shiftMeshGroupTF != null)
												{
													Editor.Select.SelectSubObject(	null, shiftMeshGroupTF, null, 
																						apSelection.MULTI_SELECT.AddOrSubtract,
																						apSelection.TF_BONE_SELECT.Exclusive);
												}
											}
										}

										_shiftClickedUnits.Clear();
									}	
								}	


								Editor.Select.SelectSubObject(null, meshGroupTransform, null, 
																	((isCtrl || isShift) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main),
																	apSelection.TF_BONE_SELECT.Exclusive);
							}

							
							//이전
							//if (Editor.Select.SubMeshGroupInGroup == meshGroupTransform)
							//{
							//	selectedUnit = eventUnit;
							//	isAnyChanged = true;
							//}

							//변경
							isAnyChanged = true;

							//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_TF);
						}
					}
					break;

				case CATEGORY.Bone_Item:
					{
						if (eventUnit.IsAvailable)//추가 : Bone의 경우 선택이 불가능한 경우가 있다.
						{
							apBone bone = savedObj as apBone;
							if (bone != null)
							{
								if(isRiggingModifier)
								{
									//리깅 모디파이어인 경우
									//- 다중 선택 불가.
									//- 메시 1개, 본 1개 따로 선택 가능
									Editor.Select.SelectBone(bone, apSelection.MULTI_SELECT.Main);
								}
								else if(isPhysicModifier)
								{
									//물리 모디파이어인 경우
									//- 다중 선택 불가
									//- TF/Bone 중에서 하나만 선택 가능
									Editor.Select.SelectSubObject(null, null, bone, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
								}
								else
								{
									//그외
									//- 다중 선택 가능
									//- TF/Bone 중에서 하나만 선택 가능

									//추가 20.7.4 : Shift를 눌렀다면, 그 사이의 것을 먼저 선택하자
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
														Editor.Select.SelectSubObject(null, null, shiftBone,
																							apSelection.MULTI_SELECT.AddOrSubtract,
																							apSelection.TF_BONE_SELECT.Exclusive);
													}
												}
											}
											_shiftClickedUnits.Clear();
										}
									}	

									Editor.Select.SelectSubObject(null, null, bone, 
																		((isCtrl || isShift) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main),
																		apSelection.TF_BONE_SELECT.Exclusive);
								}
								
								//이전
								//if (Editor.Select.Bone == bone)
								//{
								//	selectedUnit = eventUnit;
								//	isAnyChanged = true;
								//}

								//변경
								isAnyChanged = true;

								//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
								_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_Bone);
							}
						}
					}
					break;
			}

			if (isAnyChanged)
			{
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
					}

					if(selectResult == apSelection.SUB_SELECTED_RESULT.Main)
					{
						curUnit.SetSelected(true, true);
					}
					else if(selectResult == apSelection.SUB_SELECTED_RESULT.Added)
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
			if (Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}


			CATEGORY category = (CATEGORY)savedKey;

			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			apBone bone = null;

			bool isMeshTransform = false;

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
						isMeshTransform = true;
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						meshGroupTransform = savedObj as apTransform_MeshGroup;
						isVisibleDefault = meshGroupTransform._isVisible_Default;
						isMeshTransform = false;
					}
					break;

				case CATEGORY.Bone_Item:
					{
						bone = savedObj as apBone;
						isVisibleDefault = bone.IsVisibleInGUI;
					}
					break;
				default:
					return;
			}
			if (meshTransform == null && meshGroupTransform == null && bone == null)
			{
				//?? 뭘 선택했나염..
				return;
			}


			if (category == CATEGORY.Mesh_Item || category == CATEGORY.MeshGroup_Item)
			{
				if (isPrefixButton)
				{
					//Prefix : TmpWorkVisible을 토글한다.
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
						//Editor.Select.MeshGroup.
						if (linkedRenderUnit._isVisible_WithoutParent == linkedRenderUnit._isVisibleCalculated)
						{

							//TmpWork가 꺼져있다. (실제 Visible과 값이 같다)
							if (linkedRenderUnit._isVisible_WithoutParent)
							{
								//Debug.Log("Visible Type 1");

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
								//Debug.Log("Visible Type 2");

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
							//TmpWork가 켜져있다. 꺼야한다. (같은 값으로 바꾼다)
							//이전
							//linkedRenderUnit._isVisibleWorkToggle_Show2Hide = false;
							//linkedRenderUnit._isVisibleWorkToggle_Hide2Show = false;

							//Debug.Log("Visible Type 3 : " + linkedRenderUnit._workVisible_Rule);

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
							Editor.Controller.SetMeshGroupTmpWorkVisibleAll(Editor.Select.MeshGroup, !isTmpWorkToShow, linkedRenderUnit);
						}
						else
						{
							//Render Unit의 Visibility를 저장하자
							Editor.VisiblityController.Save_RenderUnit(Editor.Select.MeshGroup, linkedRenderUnit);
						}
					}

					Editor.Controller.CheckTmpWorkVisible(Editor.Select.MeshGroup);//TmpWorkVisible이 변경되었다면 이 함수 호출

					//그냥 Refresh
					Editor.Select.MeshGroup.RefreshForce();

					Editor.RefreshControllerAndHierarchy(false);
				}
				else
				{
					//Postfix :
					//Setting : isVisibleDefault를 토글한다.
					//Modifier 선택중 + ParamSetGroup을 선택하고 있다. : ModMesh의 isVisible을 변경한다.


					//v1.4.2 : Visibility를 전환할 때, 모달(FFD 등)이 켜져있으면 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();
					if(!isExecutable)
					{
						return;
					}


					bool isModVisibleSetting = Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
													&& Editor.Select.Modifier != null
													&& Editor.Select.SubEditedParamSetGroup != null
													&& Editor.Select.ParamSetOfMod != null;


					if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
					{
						//Setting 탭에서는 Default Visible을 토글한다.
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
															Editor, 
															Editor.Select.MeshGroup, 
															//savedObj, 
															false, 
															true,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						if (meshTransform != null)
						{
							meshTransform._isVisible_Default = !meshTransform._isVisible_Default;
						}
						else if (meshGroupTransform != null)
						{
							meshGroupTransform._isVisible_Default = !meshGroupTransform._isVisible_Default;
						}

						Editor.Select.MeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy(false);
					}
					else if (isModVisibleSetting)
					{

						//Mod Visible을 조절한다.
						//가능한지 여부 체크
						apModifierBase curModifier = Editor.Select.Modifier;
						apModifierParamSetGroup curParamSetGroup = Editor.Select.SubEditedParamSetGroup;

						//색상을 지원하는가
						bool isColorSupported = (int)(curModifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.Color) != 0;

						//색상 옵션이 켜져 있는가
						bool isColorEnabled = curModifier._isColorPropertyEnabled && curParamSetGroup._isColorPropertyEnabled;

						//[v1.4.2]
						//만약 Color를 지원하는데 옵션이 꺼진 상태라면 자동으로 Color Option을 켜게 만들 수 있다.
						if(isColorSupported && !isColorEnabled)
						{
							//Color Option을 켤지 물어보자
							bool isTurnOnColorOption = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_ColorOptionIsDisabled_Title),
																					Editor.GetText(TEXT.DLG_ColorOptionIsDisabled_Body),
																					Editor.GetText(TEXT.Okay),
																					Editor.GetText(TEXT.Cancel));

							if(isTurnOnColorOption)
							{	
								apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
																	Editor, 
																	curModifier, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								curModifier._isColorPropertyEnabled = true;
								curParamSetGroup._isColorPropertyEnabled = true;
								isColorEnabled = true;
							}
						}

						//색상을 지원하고 Color Option도 켜진 상태라면
						if (isColorSupported && isColorEnabled)
						{
							//apEditorUtil.SetRecord(apUndoGroupData.ACTION.Modifier_SettingChanged, Editor.Select.MeshGroup, savedObj, false, Editor);

							//MeshGroup이 아닌 Modifier를 저장할것
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
															Editor,
															curModifier,
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);
							

							//색상을 지원한다면
							//현재 객체를 선택하고,
							//ModMesh가 있는지 확인
							//없으면 -> 추가 후 받아온다.
							//ModMesh의 isVisible 값을 지정한다.

							apModifiedMesh targetModMesh = null;


							if (meshTransform != null)
							{
								Editor.Select.SelectMeshTF(meshTransform, apSelection.MULTI_SELECT.Main);
							}
							else
							{
								Editor.Select.SelectMeshGroupTF(meshGroupTransform, apSelection.MULTI_SELECT.Main);
							}

							

							Editor.Select.AutoSelectModMeshOrModBone();

							targetModMesh = Editor.Select.ModMesh_Main;

							if (targetModMesh == null)
							{
								//ModMesh가 등록이 안되어있다면
								//추가를 시도한다.

								//추가 v1.4.2 : 리깅이 된 자식 메시 그룹의 메시는 ModMesh의 대상이 되면 안된다.
								if(Editor.Select.Modifier != null)
								{
									if(meshTransform != null)
									{
										if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.TF
											&& meshTransform.IsRiggedChildMeshTF(Editor.Select.MeshGroup))
										{
											//"자식 메시 그룹의 메시 + 리깅됨" 조건에 의해 이건 TF 모디파이어에는 추가할 수 없다.
											//안내 메시지
											EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_RiggedChildMeshUnableToMod_Title),
																			Editor.GetText(TEXT.DLG_RiggedChildMeshUnableToMod_Body),
																			Editor.GetText(TEXT.Okay));

											return;
										}
									}
								}

								Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

								//여기서 주의 : "현재 ParamSet"의 ModMesh가 아니라 "모든 ParamSet"의 ModMesh에 대해서 처리를 해야한다.
								List<apModifierParamSet> paramSets = Editor.Select.SubEditedParamSetGroup._paramSetList;
								for (int i = 0; i < paramSets.Count; i++)
								{
									apModifierParamSet paramSet = paramSets[i];
									apModifiedMesh addedMesh = paramSet._meshData.Find(delegate (apModifiedMesh a)
									{
										if (isMeshTransform)
										{
											return a._transform_Mesh == meshTransform;
										}
										else
										{
											return a._transform_MeshGroup == meshGroupTransform;
										}
									});
									if (addedMesh != null)
									{
										addedMesh._isVisible = !isVisibleDefault;
									}
								}

								targetModMesh = Editor.Select.ModMesh_Main;
								if (targetModMesh == null)
								{
									return;
								}
								targetModMesh._isVisible = !isVisibleDefault;

							}
							else
							{
								//Visible을 변경한다.
								targetModMesh._isVisible = !targetModMesh._isVisible;
							}

							Editor.Select.MeshGroup.RefreshForce();
							Editor.RefreshControllerAndHierarchy(false);


						}
					}
				}
			}
			else if(category == CATEGORY.Bone_Item)
			{
				if(isPrefixButton)
				{
					//Bone의 GUI Visible을 토글한다.
					if(isCtrl)
					{
						Editor.Select.MeshGroup.SetBoneGUIVisibleAll(isVisibleDefault, bone);

						//이 함수가 추가되면 일괄적으로 저장을 해야한다.
						Editor.VisiblityController.Save_AllBones(Editor.Select.MeshGroup);
					}
					else
					{
						//bone.SetGUIVisible(!isVisibleDefault);//이전
						bone.SetGUIVisible_Tmp_ByCheckRule(!isVisibleDefault, isAlt);//Alt를 누른 상태로 Visible을 바꾸면 자식 본에도 적용된다.

						//Visible Controller에도 반영해야 한다. (20.4.13)
						Editor.VisiblityController.Save_Bone(Editor.Select.MeshGroup, bone);
					}
					
					Editor.Controller.CheckTmpWorkVisible(Editor.Select.MeshGroup);//TmpWorkVisible이 변경되었다면 이 함수 호출
					Editor.RefreshControllerAndHierarchy(false);
				}
			}
		}


		//추가 19.6.29 : RestoreTmpWork 이벤트
		public void OnUnitClickRestoreTmpWork_Mesh()
		{
			if(Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(Editor.Select.MeshGroup, true, true, false);//이전
			//변경 20.4.13
			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	Editor.Select.MeshGroup, 
																apEditorController.RESET_VISIBLE_ACTION.ResetForce, 
																apEditorController.RESET_VISIBLE_TARGET.RenderUnits);
			Editor.RefreshControllerAndHierarchy(true);
		}

		public void OnUnitClickRestoreTmpWork_Bone()
		{
			if(Editor == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(Editor.Select.MeshGroup, true, false, true);//이전
			//변경 20.4.13
			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	Editor.Select.MeshGroup, 
																apEditorController.RESET_VISIBLE_ACTION.ResetForce, 
																apEditorController.RESET_VISIBLE_TARGET.Bones);
			Editor.RefreshControllerAndHierarchy(true);
		}



		//------------------------------------------------------------------------------------------------------------
		// Modifier에 등록되었는지 체크
		//------------------------------------------------------------------------------------------------------------
		private bool IsModRegistered(apTransform_Mesh meshTransform)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(meshTransform);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsMeshTransformContain(meshTransform);
				}
			}

			return false;
		}



		private bool IsModRegistered(apTransform_MeshGroup meshGroupTransform)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(meshGroupTransform);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsMeshGroupTransformContain(meshGroupTransform);
				}
			}

			return false;
		}


		private bool IsModRegistered(apBone bone)
		{
			apModifierBase modifier = Editor.Select.Modifier;
			if (modifier == null)
			{
				return false;
			}

			if (modifier.IsAnimated)
			{
				//타임라인 기준으로 처리하자
				if (Editor.Select.AnimTimeline != null)
				{
					return Editor.Select.AnimTimeline.IsObjectAddedInLayers(bone);
				}
			}
			else
			{
				//현재 선택한 Modifier의 ParamSetGroup에 포함되어있는가
				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					return Editor.Select.SubEditedParamSetGroup.IsBoneContain(bone);
				}
			}

			return false;
		}

		/// <summary>
		/// Hierarchy에 포함될 Bone이 선택 가능한가.
		/// MeshGroup 메뉴 -> Bone 탭에서는 Sub MeshGroup의 Bone은 선택될 수 없다.
		/// </summary>
		/// <param name="bone"></param>
		/// <returns></returns>
		private bool IsBoneAvailable(apBone bone)
		{
			if(bone == null)
			{
				return false;
			}

			if(Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				if(Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Bone)
				{
					if(bone._meshGroup != Editor.Select.MeshGroup)
					{
						//조건 충족. 선택 불가이다.
						return false;
					}
				}
			}
			return true;
		}


		private apEditorHierarchyUnit.VISIBLE_TYPE GetVisibleIconType(/*apMeshGroup rootMeshGroup, */object targetObject, bool isModRegistered, bool isPrefix)
		{

			apRenderUnit linkedRenderUnit = null;
			apTransform_Mesh meshTransform = null;
			apTransform_MeshGroup meshGroupTransform = null;
			if (targetObject is apTransform_Mesh)
			{
				meshTransform = targetObject as apTransform_Mesh;
				linkedRenderUnit = meshTransform._linkedRenderUnit;

				//디버그
				//apRenderUnit renderUnitFound = rootMeshGroup.GetRenderUnit(meshTransform);
				//if(linkedRenderUnit != renderUnitFound)
				//{
				//	Debug.LogError("실제 RenderUnit과 다름 [" + meshTransform._nickName + "]");
				//}
			}
			else if (targetObject is apTransform_MeshGroup)
			{
				meshGroupTransform = targetObject as apTransform_MeshGroup;
				linkedRenderUnit = meshGroupTransform._linkedRenderUnit;

				//디버그
				//apRenderUnit renderUnitFound = rootMeshGroup.GetRenderUnit(meshGroupTransform);
				//if(linkedRenderUnit != renderUnitFound)
				//{
				//	Debug.LogError("실제 RenderUnit과 다름 [" + meshGroupTransform._nickName + "]");
				//}
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
					//if (isVisible)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_Visible; }
					//else				{ return apEditorHierarchyUnit.VISIBLE_TYPE.TmpWork_NonVisible; }

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
					if (isVisible)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_Visible; }
					else			{ return apEditorHierarchyUnit.VISIBLE_TYPE.Current_NonVisible; }
				}
			}
			else
			{
				//PostFix는
				//1) MeshGroup Setting에서는 Default를 표시하고,
				//2) Modifier/AnimClip 상태에서 ModMesh가 발견 되었을때 ModKey 상태를 출력한다.
				//아무것도 아닐때는 None 리턴
				if (_editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
				{
					if (meshTransform != null)
					{
						if (meshTransform._isVisible_Default)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_Visible; }
						else									{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_NonVisible; }
					}
					else if (meshGroupTransform != null)
					{
						if (meshGroupTransform._isVisible_Default)	{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_Visible; }
						else										{ return apEditorHierarchyUnit.VISIBLE_TYPE.Default_NonVisible; }
					}
				}
				else if (_editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier)
				{
					apModifierBase linkedModifier = _editor.Select.Modifier;
					apModifierParamSetGroup linkedSubParamSetGroup = _editor.Select.SubEditedParamSetGroup;

					if (linkedModifier != null && linkedSubParamSetGroup != null)
					{
						//Modifier가 Color를 지원하는 경우
						if ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
						{
							bool isColorOptionEnabled = linkedModifier._isColorPropertyEnabled && linkedSubParamSetGroup._isColorPropertyEnabled;

							apModifierParamSet linkedParamSet = _editor.Select.ParamSetOfMod;

							if (linkedParamSet != null)//선택된 키가 있을 때
							{	
								//v1.4.2 : Color Option이 켜지지 않은 경우를 체크하자
								if(!isColorOptionEnabled)
								{
									//Color Option이 꺼진 상태라면
									//ModMesh의 상태에 관계없이 PostFix 아이콘엔 사용 불가 아이콘이 떠야한다.
									return apEditorHierarchyUnit.VISIBLE_TYPE.NoKeyDisabled;
								}

								if (isModRegistered)//모디파이어에 등록된 객체다.
								{
									apModifiedMesh modMesh = null;
									if (meshTransform != null)
									{
										modMesh = linkedParamSet._meshData.Find(delegate (apModifiedMesh a)
										{
											return a._transform_Mesh == meshTransform;
										});
									}
									else if(meshGroupTransform != null)
									{
										modMesh = linkedParamSet._meshData.Find(delegate (apModifiedMesh a)
										{
											return a._transform_MeshGroup == meshGroupTransform;
										});
									}
									
									//if (linkedRenderUnit._isVisible_WithoutParent != meshTransform._isVisible_Default)
									if (modMesh != null)
									{
										//Mod 아이콘
										if (modMesh._isVisible)		{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_Visible; }
										else						{ return apEditorHierarchyUnit.VISIBLE_TYPE.ModKey_NonVisible; }
									}
									else
									{
										//ModMesh가 없다면 => NoKey를 리턴한다.
										return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;//<<키가 등록 안되어 있네염
									}
								}
								else
								{
									//Mod에 등록이 안되었다면 NoKey 출력
									return apEditorHierarchyUnit.VISIBLE_TYPE.NoKey;
								}
							}
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
			//bool isVisible = linkedRenderUnit._isVisible;


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
				//v1.4.2 : 우클릭 하기 전에 FFD 모드를 먼저 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();
				if(!isExecutable)
				{
					return;
				}



				//리깅, 물리 모디파이어에서는 다중 선택이 불가능하다.
				bool isRiggingPhysicsModifier = false;

				if(Editor.Select.Modifier != null)
				{
					if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
					{
						isRiggingPhysicsModifier = true;
					}
					else if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
					{
						isRiggingPhysicsModifier = true;
					}
				}

				bool isShowMenu = false;
				string objName = null;

				//미리 선택된 객체들의 리스트와 개수를 여기서 받아오자
				List<apTransform_Mesh> selectedMeshTFs = Editor.Select.GetSubSeletedMeshTFs(false);
				List<apTransform_MeshGroup> selectedMeshGroupTFs = Editor.Select.GetSubSeletedMeshGroupTFs(false);
				List<apBone> selectedBones = Editor.Select.GetSubSeletedBones(false);

				int nSelectedMeshTFs = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
				int nSelectedMeshGroupTFs = selectedMeshGroupTFs != null ? selectedMeshGroupTFs.Count : 0;
				int nSelectedBones = selectedBones != null ? selectedBones.Count : 0;

				_rightMenu.ReadyToMakeMenu();
				int nObjects = 0;

				//우클릭 메뉴를 열자 (해당되는 항목만)
				CATEGORY category = (CATEGORY)savedKey;
				switch (category)
				{
					case CATEGORY.Mesh_Item:
						{
							isShowMenu = true;

							apTransform_Mesh clickedMeshTF = null;
							if(savedObj is apTransform_Mesh)
							{
								clickedMeshTF = savedObj as apTransform_Mesh;
								objName = clickedMeshTF._nickName;
							}

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Duplicate();
							_rightMenu.SetMenu_Edit();
							_rightMenu.SetMenu_Remove();

							if(!isRiggingPhysicsModifier)
							{
								//리깅, 물리 모디파이어가 아니라면 SelectAll 메뉴가 활성화된다.
								_rightMenu.SetMenu_SelectAll();
							}

							//선택된 객체의 개수
							bool isClickedInSelected = false;
							if(nSelectedMeshTFs > 0 && clickedMeshTF != null)
							{
								if(selectedMeshTFs.Contains(clickedMeshTF))
								{
									isClickedInSelected = true;
								}
							}

							if(isClickedInSelected)
							{
								//선택된 객체들중에서 하나를 클릭했다면 선택된 TF 객체들의 개수를 표시한다.
								nObjects = nSelectedMeshTFs + nSelectedMeshGroupTFs;
							}
							else
							{
								//선택된것과 별도로 클릭했다면 1개만 표시
								nObjects = 1;
							}
						}

						break;

					case CATEGORY.MeshGroup_Item:
						{
							isShowMenu = true;

							apTransform_MeshGroup clickedMeshGroupTF = null;
							if(savedObj is apTransform_MeshGroup)
							{
								clickedMeshGroupTF = savedObj as apTransform_MeshGroup;
								objName = clickedMeshGroupTF._nickName;
							}

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Duplicate();
							_rightMenu.SetMenu_Edit();
							_rightMenu.SetMenu_Remove();

							if(!isRiggingPhysicsModifier)
							{
								//리깅, 물리 모디파이어가 아니라면 SelectAll 메뉴가 활성화된다.
								_rightMenu.SetMenu_SelectAll();
							}

							//선택된 객체의 개수
							bool isClickedInSelected = false;
							if(nSelectedMeshGroupTFs > 0 && clickedMeshGroupTF != null)
							{
								if(selectedMeshGroupTFs.Contains(clickedMeshGroupTF))
								{
									isClickedInSelected = true;
								}
							}

							if(isClickedInSelected)
							{
								//선택된 객체들중에서 하나를 클릭했다면 선택된 TF 객체들의 개수를 표시한다.
								nObjects = nSelectedMeshTFs + nSelectedMeshGroupTFs;
							}
							else
							{
								//선택된것과 별도로 클릭했다면 1개만 표시
								nObjects = 1;
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

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Duplicate();
							_rightMenu.SetMenu_Remove();

							if(!isRiggingPhysicsModifier)
							{
								//리깅, 물리 모디파이어가 아니라면 SelectAll 메뉴가 활성화된다.
								_rightMenu.SetMenu_SelectAll();
							}

							//선택된 객체의 개수
							bool isClickedInSelected = false;
							if(nSelectedBones > 0 && clickedBone != null)
							{
								if(selectedBones.Contains(clickedBone))
								{
									isClickedInSelected = true;
								}
							}

							if(isClickedInSelected)
							{
								//선택된 객체들중에서 하나를 클릭했다면 선택된 Bone 객체들의 개수를 표시한다.
								nObjects = nSelectedBones;
							}
							else
							{
								//선택된것과 별도로 클릭했다면 1개만 표시
								nObjects = 1;
							}
						}
						break;
					
				}

				if(isShowMenu)
				{
					_rightMenu.ShowMenu(objName, nObjects, savedKey, savedObj, eventUnit);
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

			apMeshGroup curMeshGroup = Editor.Select.MeshGroup;
			if(curMeshGroup == null)
			{
				return;
			}


			//리깅, 물리 모디파이어에서는 다중 선택이 불가능하다.
			bool isRiggingPhysicsModifier = false;

			if(Editor.Select.Modifier != null)
			{
				if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					isRiggingPhysicsModifier = true;
				}
				else if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					isRiggingPhysicsModifier = true;
				}
			}

			//미리 현재 선택된 TF들을 가져온다.
			List<apTransform_Mesh> selectedMeshTFs = Editor.Select.GetSubSeletedMeshTFs(false);
			List<apTransform_MeshGroup> selectedMeshGroupTFs = Editor.Select.GetSubSeletedMeshGroupTFs(false);
			int nSelectedMeshTFs = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
			int nSelectedMeshGroupTFs = selectedMeshGroupTFs != null ? selectedMeshGroupTFs.Count : 0;
			int nSelectedTotal = nSelectedMeshTFs + nSelectedMeshGroupTFs;

			//선택된 본들도 가져온다.
			List<apBone> selectedBones = Editor.Select.GetSubSeletedBones(false);
			int nSelectedBones = selectedBones != null ? selectedBones.Count : 0;


			CATEGORY category = (CATEGORY)hierachyUnitType;
			switch (category)
			{
				case CATEGORY.Mesh_Item:
					{
						apTransform_Mesh meshTransform = requestedObj as apTransform_Mesh;
						if (meshTransform != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									{
										_requestMeshGroup = curMeshGroup;
										_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, meshTransform, clickedUnit, meshTransform._nickName, OnObjectRenamed);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									{
										if (meshTransform._linkedRenderUnit != null)
										{

											#region [미사용 코드] 아래의 함수로 대체
											////변경 22.8.19 : 관련된 모든 메시 그룹들이 영향을 받는다. [v1.4.2]
											//apEditorUtil.SetRecord_MeshGroup_AllParentAndChildren(
											//									apUndoGroupData.ACTION.MeshGroup_DepthChanged, 
											//									Editor, 
											//									curMeshGroup, 
											//									apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
											//									);

											//curMeshGroup.ChangeRenderUnitDepth(meshTransform._linkedRenderUnit, meshTransform._linkedRenderUnit.GetDepth() + 1);
											//Editor.OnAnyObjectAddedOrRemoved(true); 
											#endregion

											MoveRenderUnitUpDownOnRightClickMenu(meshTransform._linkedRenderUnit, curMeshGroup, true, clickedUnit);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										if (meshTransform._linkedRenderUnit != null)
										{
											#region [미사용 코드] 아래의 함수로 대체
											////변경 22.8.19 : 관련된 모든 메시 그룹들이 영향을 받는다. [v1.4.2]
											//apEditorUtil.SetRecord_MeshGroup_AllParentAndChildren(
											//									apUndoGroupData.ACTION.MeshGroup_DepthChanged, 
											//									Editor, 
											//									curMeshGroup, 
											//									apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
											//									);

											//curMeshGroup.ChangeRenderUnitDepth(meshTransform._linkedRenderUnit, meshTransform._linkedRenderUnit.GetDepth() - 1);
											//Editor.OnAnyObjectAddedOrRemoved(true); 
											#endregion

											MoveRenderUnitUpDownOnRightClickMenu(meshTransform._linkedRenderUnit, curMeshGroup, false, clickedUnit);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									{
										_requestMeshGroup = curMeshGroup;
										_loadKey_Search = apDialog_SearchObjects.ShowDialog_SubObjects(_editor, curMeshGroup, true, !isRiggingPhysicsModifier, OnObjectSearched, OnMultipleObjectSearched);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.SelectAll:
									{
										OnSelectAll(true);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										//v1.4.2 만약 우클릭한 객체가 "다중 선택된 객체들 중 하나"라면
										//질문을 하고 선택에 따라 다중 복사를 하자
										bool isDuplicatedOnlyClicked = false;
										bool isDuplicatedSelectedAll = false;
										bool isClickedInSelected = false;

										//메시가 1개 이상 선택되어 있고, 전체 2개 이상 선택되어있을 때
										if(nSelectedMeshTFs > 0 && nSelectedTotal > 1)
										{
											if(selectedMeshTFs.Contains(meshTransform))
											{
												isClickedInSelected = true;

												//선택된 것들 중에서 클릭을 했다.
												int iResult = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.DLG_RightClickMultipleObj_Title),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_Duplicate),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_OnlyClicked),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_AllSelected),
																									Editor.GetText(TEXT.Cancel));

												if(iResult == 0)
												{
													//클릭한 것만 복제하자.
													isDuplicatedOnlyClicked = true;
												}
												else if(iResult == 1)
												{
													//선택된 모든 객체를 복제하자
													isDuplicatedSelectedAll = true;
												}
											}
										}

										if(isDuplicatedOnlyClicked || !isClickedInSelected)
										{
											//단일 객체만 복제 선택 또는 선택되지 않은 대상을 클릭함
											Editor.Controller.DuplicateMeshTransformInSameMeshGroup(meshTransform);
										}
										else if(isDuplicatedSelectedAll)
										{
											//선택된 모든 대상을 복제하기로 결정
											Editor.Controller.DuplicateMultipleTFsInSameMeshGroup(selectedMeshTFs, selectedMeshGroupTFs);
										}
										
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Edit:
									{
										if(meshTransform != null
											&& meshTransform._mesh != null)
										{
											//편집 화면으로 이동한다. [v1.4.0]
											Editor.Select.SelectMesh(meshTransform._mesh);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										//v1.4.2 : 만약 우클릭한 객체가 "다중 선택된 객체들 중 하나"라면
										bool isRemove_OnlyClicked = false;
										bool isRemove_SelectedAll = false;
										bool isClickedSelected = false;

										//메시가 1개 이상 선택되어 있고, 전체 2개 이상 선택되어있을 때
										if (nSelectedMeshTFs > 0 && nSelectedTotal > 1)
										{
											if (selectedMeshTFs.Contains(meshTransform))
											{
												//선택된 것 중에서 클릭을 했다. > 다중 삭제 가능
												isClickedSelected = true;

												//다중 삭제를 할 지, 하나만 삭제할지 물어보자
												int iResult = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.DLG_RightClickMultipleObj_Title),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_Remove),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_OnlyClicked),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_AllSelected),
																									Editor.GetText(TEXT.Cancel));

												if (iResult == 0)
												{
													//클릭한 것만 삭제하자
													isRemove_OnlyClicked = true;
												}
												else if(iResult == 1)
												{
													//선택된 모든 객체를 삭제하자
													isRemove_SelectedAll = true;
												}
											}
										}

										if(isRemove_OnlyClicked || !isClickedSelected)
										{
											//단일 객체만 삭제하기를 결정했거나, 선택되지 않은 다른 대상을 클릭했다.
											//< 단일 객체 삭제하기 >

											//경고 메시지를 보여준다.
											string strDialogInfo = Editor.Controller.GetRemoveItemMessage(
																			Editor._portrait,
																			meshTransform,
																			5,
																			Editor.GetText(TEXT.Detach_Body),
																			Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

											bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																					strDialogInfo,
																					Editor.GetText(TEXT.Detach_Ok),
																					Editor.GetText(TEXT.Cancel)
																					);

											if (isResult)
											{
												bool isResetSelection = Editor.Select.MeshTFs_All != null && Editor.Select.MeshTFs_All.Contains(meshTransform);
											
												Editor.Controller.DetachMeshTransform(meshTransform, curMeshGroup);
												if(isResetSelection)
												{
													Editor.Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
												}
											}
										}
										else if(isRemove_SelectedAll)
										{
											//선택된 모든 대상을 삭제하기로 결정

											//< 선택된 MeshTF/MeshGroupTF 삭제하기 >

											//경고 메시지를 보여주자
											string strDialogInfo = Editor.Controller.GetRemoveItemsMessage(
																			Editor._portrait,
																			selectedMeshTFs, selectedMeshGroupTFs,
																			5,
																			Editor.GetText(TEXT.Detach_Body),
																			Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

											bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																					strDialogInfo,
																					Editor.GetText(TEXT.Detach_Ok),
																					Editor.GetText(TEXT.Cancel)
																					);

											if (isResult)
											{
												//다 삭제하고 선택 해제
												Editor.Controller.DetachMultipleTransforms(selectedMeshTFs, selectedMeshGroupTFs, curMeshGroup);
												Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
											}
										}
										
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apTransform_MeshGroup meshGroupTransform = requestedObj as apTransform_MeshGroup;
						if (meshGroupTransform != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									{
										_requestMeshGroup = curMeshGroup;
										_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, meshGroupTransform, clickedUnit, meshGroupTransform._nickName, OnObjectRenamed);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									{
										if (meshGroupTransform._linkedRenderUnit != null)
										{
											#region [미사용 코드] 아래의 함수로 대체
											////변경 22.8.19 : 관련된 모든 메시 그룹들이 영향을 받는다. [v1.4.2]
											//apEditorUtil.SetRecord_MeshGroup_AllParentAndChildren(
											//									apUndoGroupData.ACTION.MeshGroup_DepthChanged, 
											//									Editor, 
											//									curMeshGroup, 
											//									apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
											//									);

											//curMeshGroup.ChangeRenderUnitDepth(meshGroupTransform._linkedRenderUnit, meshGroupTransform._linkedRenderUnit.GetDepth() + 1);
											//Editor.OnAnyObjectAddedOrRemoved(true); 
											#endregion

											MoveRenderUnitUpDownOnRightClickMenu(meshGroupTransform._linkedRenderUnit, curMeshGroup, true, clickedUnit);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										if (meshGroupTransform._linkedRenderUnit != null)
										{
											#region [미사용 코드] 아래의 함수로 대체
											////변경 22.8.19 : 관련된 모든 메시 그룹들이 영향을 받는다. [v1.4.2]
											//apEditorUtil.SetRecord_MeshGroup_AllParentAndChildren(
											//									apUndoGroupData.ACTION.MeshGroup_DepthChanged, 
											//									Editor, 
											//									curMeshGroup, 
											//									apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
											//									);

											//curMeshGroup.ChangeRenderUnitDepth(meshGroupTransform._linkedRenderUnit, meshGroupTransform._linkedRenderUnit.GetDepth() - 1);
											//Editor.OnAnyObjectAddedOrRemoved(true); 
											#endregion

											MoveRenderUnitUpDownOnRightClickMenu(meshGroupTransform._linkedRenderUnit, curMeshGroup, false, clickedUnit);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									{
										_requestMeshGroup = curMeshGroup;
										_loadKey_Search = apDialog_SearchObjects.ShowDialog_SubObjects(_editor, curMeshGroup, true, !isRiggingPhysicsModifier, OnObjectSearched, OnMultipleObjectSearched);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.SelectAll:
									{
										OnSelectAll(true);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										//v1.4.2 만약 우클릭한 객체가 "다중 선택된 객체들 중 하나"라면
										//질문을 하고 선택에 따라 다중 복사를 하자
										bool isDuplicatedOnlyClicked = false;
										bool isDuplicatedSelectedAll = false;
										bool isClickedInSelected = false;

										//메시그룹TF가 1개 이상 선택되어 있고, 전체 2개 이상 선택되어있을 때
										if(nSelectedMeshGroupTFs > 0 && nSelectedTotal > 1)
										{
											if(selectedMeshGroupTFs.Contains(meshGroupTransform))
											{
												isClickedInSelected = true;

												//선택된 것들 중에서 클릭을 했다.
												int iResult = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.DLG_RightClickMultipleObj_Title),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_Duplicate),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_OnlyClicked),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_AllSelected),
																									Editor.GetText(TEXT.Cancel));

												if(iResult == 0)
												{
													//클릭한 것만 복제하자.
													isDuplicatedOnlyClicked = true;
												}
												else if(iResult == 1)
												{
													//선택된 모든 객체를 복제하자
													isDuplicatedSelectedAll = true;
												}
											}
										}

										if(isDuplicatedOnlyClicked || !isClickedInSelected)
										{
											//단일 객체만 복제 선택 또는 선택되지 않은 대상을 클릭함
											Editor.Controller.DuplicateMeshGroupTransformInSameMeshGroup(meshGroupTransform);
										}
										else if(isDuplicatedSelectedAll)
										{
											//선택된 모든 대상을 복제하기로 결정
											Editor.Controller.DuplicateMultipleTFsInSameMeshGroup(selectedMeshTFs, selectedMeshGroupTFs);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Edit:
									{
										if(meshGroupTransform != null
											&& meshGroupTransform._meshGroup != null)
										{
											//편집 화면으로 이동한다. [v1.4.0]
											Editor.Select.SelectMeshGroup(meshGroupTransform._meshGroup);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										//v1.4.2 : 만약 우클릭한 객체가 "다중 선택된 객체들 중 하나"라면
										bool isRemove_OnlyClicked = false;
										bool isRemove_SelectedAll = false;
										bool isClickedSelected = false;

										//메시가 1개 이상 선택되어 있고, 전체 2개 이상 선택되어있을 때
										if (nSelectedMeshGroupTFs > 0 && nSelectedTotal > 1)
										{
											if (selectedMeshGroupTFs.Contains(meshGroupTransform))
											{
												//선택된 것 중에서 클릭을 했다. > 다중 삭제 가능
												isClickedSelected = true;

												//다중 삭제를 할 지, 하나만 삭제할지 물어보자
												int iResult = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.DLG_RightClickMultipleObj_Title),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_Remove),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_OnlyClicked),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_AllSelected),
																									Editor.GetText(TEXT.Cancel));

												if (iResult == 0)
												{
													//클릭한 것만 삭제하자
													isRemove_OnlyClicked = true;
												}
												else if(iResult == 1)
												{
													//선택된 모든 객체를 삭제하자
													isRemove_SelectedAll = true;
												}
											}
										}
										if (isRemove_OnlyClicked || !isClickedSelected)
										{
											//단일 객체만 삭제하기를 결정했거나, 선택되지 않은 다른 대상을 클릭했다.
											//< 단일 객체 삭제하기 >

											//경고 메시지를 보여준다.
											string strDialogInfo = Editor.Controller.GetRemoveItemMessage(
																			Editor._portrait,
																			meshGroupTransform,
																			5,
																			Editor.GetText(TEXT.Detach_Body),
																			Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

											bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																					//Editor.GetText(TEXT.Detach_Body),
																					strDialogInfo,
																					Editor.GetText(TEXT.Detach_Ok),
																					Editor.GetText(TEXT.Cancel)
																					);

											if (isResult)
											{
												bool isResetSelection = Editor.Select.MeshGroupTFs_All != null && Editor.Select.MeshGroupTFs_All.Contains(meshGroupTransform);

												Editor.Controller.DetachMeshGroupTransform(meshGroupTransform, curMeshGroup);
												if (isResetSelection)
												{
													Editor.Select.SelectMeshGroupTF(null, apSelection.MULTI_SELECT.Main);
												}
											}
										}
										else if (isRemove_SelectedAll)
										{
											//선택된 모든 대상을 삭제하기로 결정

											//< 선택된 MeshTF/MeshGroupTF 삭제하기 >
											//경고 메시지를 보여주자
											string strDialogInfo = Editor.Controller.GetRemoveItemsMessage(
																			Editor._portrait,
																			selectedMeshTFs, selectedMeshGroupTFs,
																			5,
																			Editor.GetText(TEXT.Detach_Body),
																			Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

											bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																					strDialogInfo,
																					Editor.GetText(TEXT.Detach_Ok),
																					Editor.GetText(TEXT.Cancel)
																					);

											if (isResult)
											{
												//다 삭제하고 선택 해제
												Editor.Controller.DetachMultipleTransforms(selectedMeshTFs, selectedMeshGroupTFs, curMeshGroup);
												Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
											}
										}

										
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Bone_Item:
					{
						apBone bone = requestedObj as apBone;
						if (bone != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									{
										_requestMeshGroup = curMeshGroup;
										_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, bone, clickedUnit, bone._name, OnObjectRenamed);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									{
										#region [미사용 코드] 아래 함수로 대체
										//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DepthChanged,
										//									Editor,
										//									curMeshGroup,
										//									//bone, 
										//									false, true,
										//									//apEditorUtil.UNDO_STRUCT.ValueOnly
										//									apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
										//									);

										//curMeshGroup.ChangeBoneDepth(bone, bone._depth + 1); 
										#endregion

										MoveBoneUpDownOnRightClickMenu(bone, curMeshGroup, true, clickedUnit);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										#region [미사용 코드] 아래 함수로 대체
										//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DepthChanged,
										//									Editor,
										//									curMeshGroup,
										//									//bone, 
										//									false, true,
										//									//apEditorUtil.UNDO_STRUCT.ValueOnly
										//									apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
										//									);

										//curMeshGroup.ChangeBoneDepth(bone, bone._depth - 1); 
										#endregion

										MoveBoneUpDownOnRightClickMenu(bone, curMeshGroup, false, clickedUnit);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									{
										_requestMeshGroup = curMeshGroup;
										_loadKey_Search = apDialog_SearchObjects.ShowDialog_SubObjects(_editor, curMeshGroup, false, !isRiggingPhysicsModifier, OnObjectSearched, OnMultipleObjectSearched);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.SelectAll:
									{
										OnSelectAll(false);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										_requestMeshGroup = curMeshGroup;
										_loadKey_DuplicateBone = apDialog_DuplicateBone.ShowDialog(Editor, bone, OnDuplicateBoneResult);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										//v1.4.2 : 만약 우클릭한 객체가 "다중 선택된 객체들 중 하나"라면
										bool isRemove_OnlyClicked = false;
										bool isRemove_SelectedAll = false;
										bool isClickedSelected = false;

										//본이 2개 이상 선택되어 있을 때
										if (nSelectedBones > 1)
										{
											if (selectedBones.Contains(bone))
											{
												//선택된 것 중에서 클릭을 했다. > 다중 삭제 가능
												isClickedSelected = true;

												//다중 삭제를 할 지, 하나만 삭제할지 물어보자
												int iResult = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.DLG_RightClickMultipleObj_Title),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_Remove),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_OnlyClicked),
																									Editor.GetText(TEXT.DLG_RightClickMultipleObj_Body_AllSelected),
																									Editor.GetText(TEXT.Cancel));

												if (iResult == 0)
												{
													//클릭한 것만 삭제하자
													isRemove_OnlyClicked = true;
												}
												else if(iResult == 1)
												{
													//선택된 모든 객체를 삭제하자
													isRemove_SelectedAll = true;
												}
											}
										}

										if (isRemove_OnlyClicked || !isClickedSelected)
										{
											//단일 객체만 삭제하기를 결정했거나, 선택되지 않은 다른 대상을 클릭했다.
											//< 단일 객체 삭제하기 >
											//경고 메시지, 자식 본 같이 삭제 여부
											string strRemoveBoneText = Editor.Controller.GetRemoveItemMessage(
																							Editor._portrait,
																							bone,
																							5,
																							Editor.GetTextFormat(TEXT.RemoveBone_Body, bone._name),
																							Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																							);

											int btnIndex = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.RemoveBone_Title),
																							strRemoveBoneText,
																							Editor.GetText(TEXT.Remove),
																							Editor.GetText(TEXT.RemoveBone_RemoveAllChildren),
																							Editor.GetText(TEXT.Cancel));

											if (btnIndex == 0)
											{
												//Bone을 삭제한다.
												Editor.Controller.RemoveBone(bone, false);
											}
											else if (btnIndex == 1)
											{
												//Bone과 자식을 모두 삭제한다.
												Editor.Controller.RemoveBone(bone, true);
											}

										}
										else if (isRemove_SelectedAll)
										{
											//선택된 모든 대상을 삭제하기로 결정

											//< 선택된 모든 Bone들 삭제하기 >
											
											string strRemoveBoneText = Editor.Controller.GetRemoveItemsMessage(
																							Editor._portrait,
																							selectedBones,
																							5,
																							Editor.GetTextFormat(TEXT.RemoveBone_Body, bone._name),
																							Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																							);

											int btnIndex = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.RemoveBone_Title),
																							strRemoveBoneText,
																							Editor.GetText(TEXT.Remove),
																							Editor.GetText(TEXT.RemoveBone_RemoveAllChildren),
																							Editor.GetText(TEXT.Cancel));

											if (btnIndex == 0)
											{
												//Bone을 삭제한다.
												Editor.Controller.RemoveBones(selectedBones, curMeshGroup, false);
											}
											else if (btnIndex == 1)
											{
												//Bone과 자식을 모두 삭제한다.
												Editor.Controller.RemoveBones(selectedBones, curMeshGroup, true);
											}
										}
										

										
										
									}
									break;
							}
						}
					}
					break;
			}

			//메뉴를 선택하면 변경이 될 것이므로 Dirty
			apEditorUtil.SetEditorDirty();
			Editor.RefreshControllerAndHierarchy(false);
		}


		//우클릭 메뉴에서 객체의 Depth를 변경하는 Move Up/Move Down을 눌렀을 때 (Render Unit 버전)
		private void MoveRenderUnitUpDownOnRightClickMenu(apRenderUnit clickedRenderUnit, apMeshGroup meshGroup, bool isMoveUp, apEditorHierarchyUnit clickedUnit)
		{
			if(clickedRenderUnit == null || meshGroup == null)
			{
				return;
			}

			//규칙
			//해당 객체가 "현재 선택된 객체"인 경우에 > 다중 선택된 상황이면 모두 Depth를 이동시킨다.
			//해당 객체가 선택된 객체가 아닌 경우에 > 선택된 객체 상관없이 그 객체의 Depth만 이동시킨다.

			bool isSelectedRenderUnit = false;
			if(clickedRenderUnit._meshTransform != null)
			{
				if(Editor.Select.IsSubSelected(clickedRenderUnit._meshTransform) != apSelection.SUB_SELECTED_RESULT.None)
				{
					//Mesh TF로서 선택된 객체다.
					isSelectedRenderUnit = true;
				}
			}
			else if(clickedRenderUnit._meshGroupTransform != null)
			{
				if(Editor.Select.IsSubSelected(clickedRenderUnit._meshGroupTransform) != apSelection.SUB_SELECTED_RESULT.None)
				{
					//MeshGroup TF로서 선택된 객체다.
					isSelectedRenderUnit = true;
				}
			}


			//변경 22.8.19 : 관련된 모든 메시 그룹들이 영향을 받는다. [v1.4.2]
			apEditorUtil.SetRecord_MeshGroup_AllParentAndChildren(
												apUndoGroupData.ACTION.MeshGroup_DepthChanged, 
												Editor, 
												meshGroup, 
												apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
												);


			int deltaDepth = isMoveUp ? 1 : -1;

			if(isSelectedRenderUnit)
			{
				//선택된 객체를 우클릭해서 위아래로 이동시키고자 했다면
				//선택된 모든 RenderUnit을 대상으로 이동시키도록 하자
				List<apRenderUnit> selectedRenderUnits = new List<apRenderUnit>();

				List<apTransform_Mesh> selectedMeshTFs = Editor.Select.GetSubSeletedMeshTFs(false);
				List<apTransform_MeshGroup> selectedMeshGroupTFs = Editor.Select.GetSubSeletedMeshGroupTFs(false);

				int nMeshTFs = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
				int nMeshGroupTFs = selectedMeshGroupTFs != null ? selectedMeshGroupTFs.Count : 0;

				if(nMeshTFs > 0)
				{
					apTransform_Mesh curMeshTF = null;
					for (int i = 0; i < nMeshTFs; i++)
					{
						curMeshTF = selectedMeshTFs[i];
						if(curMeshTF == null || curMeshTF._linkedRenderUnit == null)
						{
							continue;
						}

						selectedRenderUnits.Add(curMeshTF._linkedRenderUnit);
					}
				}

				if(nMeshGroupTFs > 0)
				{
					apTransform_MeshGroup curMeshGroupTF = null;
					for (int i = 0; i < nMeshGroupTFs; i++)
					{
						curMeshGroupTF = selectedMeshGroupTFs[i];
						if(curMeshGroupTF == null || curMeshGroupTF._linkedRenderUnit == null)
						{
							continue;
						}

						selectedRenderUnits.Add(curMeshGroupTF._linkedRenderUnit);
					}
				}

				//혹시라도 현재 객체가 추가되지 않았다면 추가
				if (!selectedRenderUnits.Contains(clickedRenderUnit))
				{
					selectedRenderUnits.Add(clickedRenderUnit);
				}

				//다같이 이동시키자
				meshGroup.ChangeMultipleRenderUnitsDepth(selectedRenderUnits, deltaDepth);
			}
			else
			{
				//선택되지 않은 객체를 우클릭했다면, 해당 객체만 이동시키자.
				meshGroup.ChangeRenderUnitDepth(clickedRenderUnit, clickedRenderUnit.GetDepth() + deltaDepth);

				//해당 객체를 선택하자
				if(clickedRenderUnit._meshTransform != null)
				{
					Editor.Select.SelectSubObject(clickedRenderUnit._meshTransform, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
				}
				else if(clickedRenderUnit._meshGroupTransform != null)
				{
					Editor.Select.SelectSubObject(null, clickedRenderUnit._meshGroupTransform, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
				}

				//리스트 유닛의 활성 여부를 모두 갱신한다.
				apEditorHierarchyUnit curHierarchyUnit = null;
				apSelection.SUB_SELECTED_RESULT selectResult = apSelection.SUB_SELECTED_RESULT.None;

				for (int i = 0; i < _units_All.Count; i++)
				{
					curHierarchyUnit = _units_All[i];
					
					selectResult = apSelection.SUB_SELECTED_RESULT.None;
					
					switch ((CATEGORY)curHierarchyUnit._savedKey)
					{
						case CATEGORY.Mesh_Item:
						case CATEGORY.MeshGroup_Item:
						case CATEGORY.Bone_Item:
							{
								selectResult = Editor.Select.IsSubSelected(curHierarchyUnit._savedObj);
							}
							break;
					}

					if(selectResult == apSelection.SUB_SELECTED_RESULT.Main)
					{
						curHierarchyUnit.SetSelected(true, true);
					}
					else if(selectResult == apSelection.SUB_SELECTED_RESULT.Added)
					{
						curHierarchyUnit.SetSelected(true, false);
					}
					else
					{
						curHierarchyUnit.SetSelected(false, true);
					}
				}

				//클릭한 유닛은 여기서 체크한다.
				_lastClickedUnit = clickedUnit;

				//기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
				Editor.Gizmos.OnSelectedObjectsChanged();

			}
			
			Editor.OnAnyObjectAddedOrRemoved(true);
		}


		//우클릭 메뉴에서 본의 Depth를 변경하는 Move Up/Move Down을 눌렀을 때 (Bone 버전)
		private void MoveBoneUpDownOnRightClickMenu(apBone clickedBone, apMeshGroup meshGroup, bool isMoveUp, apEditorHierarchyUnit clickedUnit)
		{
			if(clickedBone == null || meshGroup == null)
			{
				return;
			}

			//규칙
			//해당 객체가 "현재 선택된 객체"인 경우에 > 다중 선택된 상황이면 모두 Depth를 이동시킨다.
			//해당 객체가 선택된 객체가 아닌 경우에 > 선택된 객체 상관없이 그 객체의 Depth만 이동시킨다.

			bool isSelectedBone = false;
			if(Editor.Select.IsSubSelected(clickedBone) != apSelection.SUB_SELECTED_RESULT.None)
			{
				//이건 선택된 본이다.
				isSelectedBone = true;
			}


			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DepthChanged,
																			Editor,
																			meshGroup,
																			//bone, 
																			false, true,
																			//apEditorUtil.UNDO_STRUCT.ValueOnly
																			apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
																			);

			int deltaDepth = isMoveUp ? 1 : -1;

			if(isSelectedBone)
			{
				//선택된 객체를 우클릭해서 위아래로 이동시키고자 했다면
				//선택된 모든 본들을 대상으로 이동시키자
				List<apBone> selectedBones = Editor.Select.GetSubSeletedBones(false);
				int nSelectedBones = selectedBones != null ? selectedBones.Count : 0;

				List<apBone> movingBones = new List<apBone>();//유효한것만 넣어서 이걸 이동시키자

				if(nSelectedBones > 0)
				{
					apBone curBone = null;
					for (int i = 0; i < nSelectedBones; i++)
					{
						curBone = selectedBones[i];
						if(curBone != null)
						{
							movingBones.Add(curBone);
						}
					}
				}
				if(!movingBones.Contains(clickedBone))
				{
					//클릭한 본이 정작 리스트 내에 없다면 추가
					movingBones.Add(clickedBone);
				}

				//다같이 이동시키자
				meshGroup.ChangeMultipleBonesDepth(movingBones, deltaDepth);
			}
			else
			{
				//선택된 객체 하나만 이동시키자
				meshGroup.ChangeBoneDepth(clickedBone, clickedBone._depth + deltaDepth);

				//이 본을 선택하자
				Editor.Select.SelectSubObject(null, null, clickedBone, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);

				//리스트 유닛의 활성 여부를 모두 갱신한다.
				apEditorHierarchyUnit curHierarchyUnit = null;
				apSelection.SUB_SELECTED_RESULT selectResult = apSelection.SUB_SELECTED_RESULT.None;

				for (int i = 0; i < _units_All.Count; i++)
				{
					curHierarchyUnit = _units_All[i];
					
					selectResult = apSelection.SUB_SELECTED_RESULT.None;
					
					switch ((CATEGORY)curHierarchyUnit._savedKey)
					{
						case CATEGORY.Mesh_Item:
						case CATEGORY.MeshGroup_Item:
						case CATEGORY.Bone_Item:
							{
								selectResult = Editor.Select.IsSubSelected(curHierarchyUnit._savedObj);
							}
							break;
					}

					if(selectResult == apSelection.SUB_SELECTED_RESULT.Main)
					{
						curHierarchyUnit.SetSelected(true, true);
					}
					else if(selectResult == apSelection.SUB_SELECTED_RESULT.Added)
					{
						curHierarchyUnit.SetSelected(true, false);
					}
					else
					{
						curHierarchyUnit.SetSelected(false, true);
					}
				}

				//클릭한 유닛은 여기서 체크한다.
				_lastClickedUnit = clickedUnit;

				//기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
				Editor.Gizmos.OnSelectedObjectsChanged();
			}
										
		}



		//오브젝트 이름 바꾸기 다이얼로그 결과
		public void OnObjectRenamed(bool isSuccess, object loadKey, object targetObject, apEditorHierarchyUnit targetHierarchyUnit, string name)
		{
			if(!isSuccess
				|| loadKey == null
				|| _loadKey_Rename != loadKey
				|| targetObject == null
				|| string.IsNullOrEmpty(name)
				|| _editor == null
				|| _editor._portrait == null
				|| _requestMeshGroup != Editor.Select.MeshGroup)
			{
				_requestMeshGroup = null;
				_loadKey_Rename = null;
				return;
			}

			_loadKey_Rename = null;

			//대상에 따라 다르다.
			if(targetObject is apTransform_Mesh)
			{
				apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
				if(meshTransform != null)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
														Editor, 
														_requestMeshGroup, 
														//meshTransform,
														false, true,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					meshTransform._nickName = name;

					//해당 객체를 선택하자 [v1.4.2]
					if (targetHierarchyUnit != null)
					{	
						Editor.Select.SelectSubObject(meshTransform, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);

						//TF 탭에서 눌렀음을 알림
						_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_TF);

						//클릭한 유닛은 여기서 체크한다.
						_lastClickedUnit = targetHierarchyUnit;

						//기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
						Editor.Gizmos.OnSelectedObjectsChanged();
					}
					
				}
			}
			else if(targetObject is apTransform_MeshGroup)
			{
				apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
				if(meshGroupTransform != null)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
														Editor, 
														_requestMeshGroup, 
														//meshGroupTransform, 
														false, true,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					meshGroupTransform._nickName = name;


					//해당 객체를 선택하자 [v1.4.2]
					if (targetHierarchyUnit != null)
					{	
						Editor.Select.SelectSubObject(null, meshGroupTransform, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);

						//TF 탭에서 눌렀음을 알림
						_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_TF);

						//클릭한 유닛은 여기서 체크한다.
						_lastClickedUnit = targetHierarchyUnit;

						//기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
						Editor.Gizmos.OnSelectedObjectsChanged();
					}
				}
			}
			else if(targetObject is apBone)
			{
				apBone bone = targetObject as apBone;
				if(bone != null)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														bone._meshGroup, 
														//bone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					bone._name = name;

					//해당 객체를 선택하자 [v1.4.2]
					if (targetHierarchyUnit != null)
					{	
						Editor.Select.SelectSubObject(null, null, bone, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);

						//TF 탭에서 눌렀음을 알림
						_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.MeshGroup_Bone);

						//클릭한 유닛은 여기서 체크한다.
						_lastClickedUnit = targetHierarchyUnit;

						//기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
						Editor.Gizmos.OnSelectedObjectsChanged();
					}
				}
			}
			

			Editor.RefreshControllerAndHierarchy(false);
		}


		//오브젝트 찾기 다이얼로그 결과
		private void OnObjectSearched(object loadKey, object targetObject)
		{	
			
			if(_loadKey_Search != loadKey
				|| loadKey == null
				|| _editor == null
				|| _editor._portrait == null
				|| _requestMeshGroup != Editor.Select.MeshGroup
				|| Editor.Select.MeshGroup == null)
			{
				_loadKey_Search = null;
				return;
			}

			//창이 열린 이후에 연속으로 이 이벤트가 호출될 수 있다.
			//_loadKey_Search = null;//null로 만들면 안된다.

			if(targetObject == null)
			{
				return;
			}

			//현재 모디파이어가 선택되어 있는가
			//> 리깅 모디파이어가 선택된 상태가 아니라면, Mesh/MeshGroup Transform 선택시 Bone을 null로, 또는 그 반대로 설정해야한다.
			bool isRiggingModifier = false;
			
			if(Editor.Select.Modifier != null)
			{
				if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					isRiggingModifier = true;
				}				
			}


			if(targetObject is apTransform_Mesh)
			{
				apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
				if(meshTransform != null)
				{
					if (isRiggingModifier)
					{
						//리깅 모디파이어인 경우
						//- 메시 1개, 본 1개 따로 선택 가능
						Editor.Select.SelectMeshTF(meshTransform, apSelection.MULTI_SELECT.Main);
					}
					else
					{
						Editor.Select.SelectSubObject(meshTransform, null, null, 
																	apSelection.MULTI_SELECT.Main,
																	apSelection.TF_BONE_SELECT.Exclusive);
					}
				}
			}
			else if(targetObject is apTransform_MeshGroup)
			{
				apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
				if(meshGroupTransform != null)
				{
					if(isRiggingModifier)
					{
						//리깅 모디파이어인 경우
						//- 메시 1개, 본 1개 따로 선택 가능
						Editor.Select.SelectMeshGroupTF(meshGroupTransform, apSelection.MULTI_SELECT.Main);
					}
					else
					{
						Editor.Select.SelectSubObject(null, meshGroupTransform, null, 
															apSelection.MULTI_SELECT.Main,
															apSelection.TF_BONE_SELECT.Exclusive);
					}
				}
			}
			else if(targetObject is apBone)
			{
				apBone bone = targetObject as apBone;
				if (bone != null)
				{
					if (isRiggingModifier)
					{
						//리깅 모디파이어인 경우
						//- 메시 1개, 본 1개 따로 선택 가능
						Editor.Select.SelectBone(bone, apSelection.MULTI_SELECT.Main);
					}
					else
					{
						Editor.Select.SelectSubObject(null, null, bone, 
														apSelection.MULTI_SELECT.Main,
														apSelection.TF_BONE_SELECT.Exclusive);
					}
				}
			}			
			

			//[1.4.2] 선택된 객체에 맞게 자동 스크롤			
			if(Editor.IsAutoScrollableWhenClickObject_MeshGroup(targetObject, true))
			{
				//스크롤 가능한 상황인지 체크하고
				//자동 스크롤을 요청한다.
				Editor.AutoScroll_HierarchyMeshGroup(targetObject);
			}

			Editor.RefreshControllerAndHierarchy(false);
		}




		private void OnMultipleObjectSearched(object loadKey, List<object> targetObjects)
		{
			
			if(_loadKey_Search != loadKey
				|| loadKey == null
				|| _editor == null
				|| _editor._portrait == null
				|| _requestMeshGroup != Editor.Select.MeshGroup
				|| Editor.Select.MeshGroup == null)
			{
				_loadKey_Search = null;
				return;
			}

			//창이 열린 이후에 연속으로 이 이벤트가 호출될 수 있다.
			//_loadKey_Search = null; //null로 만들면 안된다.

			if(targetObjects == null || targetObjects.Count == 0)
			{
				return;
			}

			//현재 모디파이어가 선택되어 있는가
			//> 리깅 모디파이어가 선택된 상태가 아니라면, Mesh/MeshGroup Transform 선택시 Bone을 null로, 또는 그 반대로 설정해야한다.
			//> 리깅과 물리 모디파이어는 단일 선택만 지원한다.
			bool isRiggingModifier = false;
			bool isPhysicModifier = false;

			if(Editor.Select.Modifier != null)
			{
				if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					isRiggingModifier = true;
				}
				else if(Editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					isPhysicModifier = true;
				}
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
			if(bones.Count > 0)
			{
				//Bone을 검색했나보다.
				if(isRiggingModifier || isPhysicModifier)
				{
					//하나만 선택하자
					Editor.Select.SelectBone(bones[0], apSelection.MULTI_SELECT.Main);
				}
				else
				{
					//모두 선택하자
					for (int iBone = 0; iBone < bones.Count; iBone++)
					{
						Editor.Select.SelectSubObject(null, null, bones[iBone],
															apSelection.MULTI_SELECT.AddOrSubtract,
															apSelection.TF_BONE_SELECT.Exclusive);
					}
				}
			}
			else if(meshTFs.Count > 0 || meshGroupTFs.Count > 0)
			{
				//Mesh를 검색했나보다.
				if(isRiggingModifier || isPhysicModifier)
				{
					//하나만 선택하자. MeshTF 먼저
					if(meshTFs.Count > 0)
					{
						Editor.Select.SelectMeshTF(meshTFs[0], apSelection.MULTI_SELECT.Main);
					}
					else if(meshGroupTFs.Count > 0)
					{
						Editor.Select.SelectMeshGroupTF(meshGroupTFs[0], apSelection.MULTI_SELECT.Main);
					}
				}
				else
				{
					//모두 선택하자
					if(meshTFs.Count > 0)
					{
						for (int iMeshTF = 0; iMeshTF < meshTFs.Count; iMeshTF++)
						{
							Editor.Select.SelectSubObject(	meshTFs[iMeshTF], null, null, 
																apSelection.MULTI_SELECT.AddOrSubtract,
																apSelection.TF_BONE_SELECT.Exclusive);
						}
					}
					if(meshGroupTFs.Count > 0)
					{
						for (int iMeshGroupTF = 0; iMeshGroupTF < meshGroupTFs.Count; iMeshGroupTF++)
						{
							Editor.Select.SelectSubObject(	null, meshGroupTFs[iMeshGroupTF], null, 
																apSelection.MULTI_SELECT.AddOrSubtract,
																apSelection.TF_BONE_SELECT.Exclusive);
						}
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
				if (Editor.IsAutoScrollableWhenClickObject_MeshGroup(selectedMainObj, true))
				{
					//스크롤 가능한 상황인지 체크하고
					//자동 스크롤을 요청한다.
					Editor.AutoScroll_HierarchyMeshGroup(selectedMainObj);
				}
			}


			Editor.RefreshControllerAndHierarchy(false);
		}



		private void OnDuplicateBoneResult(bool isSuccess, apBone targetBone, object loadKey, float offsetX, float offsetY, bool isDuplicateChildren)
		{
			if (!isSuccess
				|| Editor._portrait == null
				|| _loadKey_DuplicateBone != loadKey
				|| targetBone == null
				|| Editor.Select.SelectionType != apSelection.SELECTION_TYPE.MeshGroup
				|| targetBone._meshGroup == null
				|| _requestMeshGroup != Editor.Select.MeshGroup)
			{
				_loadKey_DuplicateBone = null;
				return;
			}
			_loadKey_DuplicateBone = null;

			
			//복제 함수를 호출하자.
			Editor.Controller.DuplicateBone(_requestMeshGroup, targetBone, offsetX, offsetY, isDuplicateChildren);
		}


		//모든 객체를 선택한다.
		private void OnSelectAll(bool isMeshTransforms)
		{
			if(_editor == null
				|| _editor._portrait == null
				|| Editor.Select.MeshGroup == null)
			{
				return;
			}

			//모든 객체를 돌면서 선택하자


			apMeshGroup curMeshGroup = Editor.Select.MeshGroup;

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

			Editor.RefreshControllerAndHierarchy(false);
		}




		// GUI
		//---------------------------------------------
		//Hierarchy 레이아웃 출력
		public void GUI_RenderHierarchy(int width, bool isMeshHierarchy, Vector2 scroll, int scrollLayoutHeight)
		{
			//레이아웃 이벤트일때 GUI요소 갱신
			bool isGUIEvent = (Event.current.type == EventType.Layout);

			_curUnitPosY = 0;
			if (isMeshHierarchy)
			{
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Meshes.Count; i++)
				{
					GUI_RenderUnit(_units_Root_Meshes[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent);
					GUILayout.Space(10);
				}
			}
			else
			{
				//Bone 루트를 출력합시다.
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Bones.Count; i++)
				{
					GUI_RenderUnit(_units_Root_Bones[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent);
					GUILayout.Space(10);
				}
			}

		}

		//재귀적으로 Hierarchy 레이아웃을 출력
		//Child에 진입할때마다 Level을 높인다. (여백과 Fold의 기준이 됨)
		private void GUI_RenderUnit(apEditorHierarchyUnit unit, int level, int width, Vector2 scroll, int scrollLayoutHeight, bool isGUIEvent)
		{
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
						GUI_RenderUnit(unit._childUnits[i], level + 1, width, scroll, scrollLayoutHeight, isGUIEvent);
					}

					//추가 > 자식 Unit을 호출한 이후에는 약간 갭을 두자
					//GUILayout.Space(2);
				}

			}
		}
		//-------------------------------------------------------------------------------
		/// <summary>
		/// [1.4.2] 현재 선택된 커서 및 전후의 오브젝트들을 리턴한다.
		/// </summary>
		public void FindCursor(out object curObject, out object prevObject, out object nextObject, bool isMeshHierarchy)
		{
			//렌더링 순서대로 검색을 하여 Prev, Cur, Next를 찾는다.
			//타입은 고려하지 않는다.
			curObject = null;
			prevObject = null;
			nextObject = null;

			apEditorHierarchyUnit selectedUnit_Cur = null;
			apEditorHierarchyUnit selectedUnit_Prev = null;
			apEditorHierarchyUnit selectedUnit_Next = null;

			//렌더링 코드에 맞추어 렌더링하기
			//레이아웃 이벤트일때 GUI요소 갱신
			if (isMeshHierarchy)
			{
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Meshes.Count; i++)
				{
					bool isFind = FindCursor_Recursive(	_units_Root_Meshes[i], 
														ref selectedUnit_Cur,
														ref selectedUnit_Prev,
														ref selectedUnit_Next);
					if(isFind)
					{
						break;
					}
				}
			}
			else
			{
				//Bone 루트를 출력합시다.
				//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
				for (int i = 0; i < _units_Root_Bones.Count; i++)
				{
					bool isFind = FindCursor_Recursive(	_units_Root_Bones[i], 
														ref selectedUnit_Cur,
														ref selectedUnit_Prev,
														ref selectedUnit_Next);
					if(isFind)
					{
						break;
					}
				}
			}

			//결과 넣어서 종료
			if(selectedUnit_Cur != null)
			{
				curObject = selectedUnit_Cur._savedObj;
			}
			if(selectedUnit_Prev != null)
			{
				prevObject = selectedUnit_Prev._savedObj;
			}
			if(selectedUnit_Next != null)
			{
				nextObject = selectedUnit_Next._savedObj;
			}
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
		public apEditorHierarchyUnit FindUnitByObject(object targetObj, bool isMeshList)
		{
			if(targetObj == null)
			{
				return null;
			}

			apEditorHierarchyUnit resultUnit = null;

			//1. 오브젝트 매핑에서 먼저 찾아보자
			if(isMeshList)
			{
				if(_object2Unit_Meshes.TryGetValue(targetObj, out resultUnit))
				{
					if(resultUnit != null)
					{
						//Debug.Log("오브젝트 매핑에서 찾음 - Mesh");
						return resultUnit;
					}
				}
			}
			else
			{
				if(_object2Unit_Bones.TryGetValue(targetObj, out resultUnit))
				{
					if(resultUnit != null)
					{
						//Debug.Log("오브젝트 매핑에서 찾음 - Bone");
						return resultUnit;
					}
				}
			}


			//2. 루트 유닛부터 하나씩 검색하자
			List<apEditorHierarchyUnit> rootUnits = null;
			if(isMeshList)
			{
				rootUnits = _units_Root_Meshes;
			}
			else
			{
				rootUnits = _units_Root_Bones;
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
		public int CalculateUnitPosY(apEditorHierarchyUnit targetUnit, bool isMeshList, out bool result)
		{
			int curUnitPosY = 0;
			result = false;

			List<apEditorHierarchyUnit> rootUnits = null;
			
			if(isMeshList)	{ rootUnits = _units_Root_Meshes; }
			else			{ rootUnits = _units_Root_Bones; }

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