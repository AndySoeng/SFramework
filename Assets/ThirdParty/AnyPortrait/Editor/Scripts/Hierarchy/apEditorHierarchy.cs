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

	public class apEditorHierarchy
	{
		// Members
		//---------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }


		public List<apEditorHierarchyUnit> _units_All = new List<apEditorHierarchyUnit>();
		public List<apEditorHierarchyUnit> _units_Root = new List<apEditorHierarchyUnit>();
		private Dictionary<object, apEditorHierarchyUnit> _object2Unit = new Dictionary<object, apEditorHierarchyUnit>();

		//개선 20.3.28 : Pool을 이용하자
		private apEditorHierarchyUnitPool _unitPool = new apEditorHierarchyUnitPool();

		//추가 21.6.12 : 우클릭에 대한 메뉴 멤버 추가
		private apEditorHierarchyMenu _rightMenu = null;
		private object _loadKey_Rename = null;//이름 변경 다이얼로그의 유효성 테스트를 위한 로드키 
		private object _loadKey_Search = null;
		
		public enum CATEGORY
		{
			Overall_Name,
			Overall_Item,
			Images_Name,
			Images_Item,
			Images_Add,
			Images_AddPSD,
			Mesh_Name,
			Mesh_Item,
			Mesh_Add,
			MeshGroup_Name,
			MeshGroup_Item,
			MeshGroup_Add,
			//Face_Name,
			//Face_Item,
			//Face_Add,
			Animation_Name,
			Animation_Item,
			Animation_Add,
			Param_Name,
			Param_Item,
			Param_Add,
		}

		//루트들만 따로 적용
		private apEditorHierarchyUnit _rootUnit_Overall = null;
		private apEditorHierarchyUnit _rootUnit_Image = null;
		private apEditorHierarchyUnit _rootUnit_Mesh = null;
		private apEditorHierarchyUnit _rootUnit_MeshGroup = null;
		//private apEditorHierarchyUnit _rootUnit_Face = null;
		private apEditorHierarchyUnit _rootUnit_Animation = null;
		private apEditorHierarchyUnit _rootUnit_Param = null;

		//public Texture2D _icon_Image = null;
		//public Texture2D _icon_Mesh = null;
		//public Texture2D _icon_MeshGroup = null;
		//public Texture2D _icon_Face = null;
		//public Texture2D _icon_Animation = null;
		//public Texture2D _icon_Add = null;

		//public Texture2D _icon_FoldDown = null;
		//public Texture2D _icon_FoldRight = null;

		//추가 19.11.16
		private apGUIContentWrapper _guiContent_FoldDown = null;
		private apGUIContentWrapper _guiContent_FoldRight = null;
		private apGUIContentWrapper _guiContent_ModRegisted = null;
		
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_ON = null;
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_OFF = null;

		private apGUIContentWrapper _guiContent_OrderUp = null;
		private apGUIContentWrapper _guiContent_OrderDown = null;

		private bool _isNeedReset = false;
		private int _curUnitPosY = 0;//<<추가

		private object _loadKey_MultipleObjectRemoved = null;


		


		public void SetNeedReset()
		{
			_isNeedReset = true;
		}

		// Init
		//---------------------------------------------
		public apEditorHierarchy(apEditor editor)
		{
			_editor = editor;

			_rightMenu = new apEditorHierarchyMenu(_editor, OnSelectRightMenu);

			if(_units_All == null) { _units_All = new List<apEditorHierarchyUnit>(); }
			_units_All.Clear();

			if(_units_Root == null) { _units_Root = new List<apEditorHierarchyUnit>(); }
			_units_Root.Clear();

			if(_object2Unit == null) { _object2Unit = new Dictionary<object, apEditorHierarchyUnit>(); }
			_object2Unit.Clear();

			if(_unitPool == null)
			{
				_unitPool = new apEditorHierarchyUnitPool();
			}
		}


		private void ReloadGUIContent()
		{
			if (_editor == null)
			{
				return;
			}
			//GUIContent 추가
			if (_guiContent_FoldDown == null)			{ _guiContent_FoldDown =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown)); }
			if (_guiContent_FoldRight == null)			{ _guiContent_FoldRight =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight)); }
			if (_guiContent_ModRegisted == null)		{ _guiContent_ModRegisted =			apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered)); }

			if (_guiContent_RestoreTmpWorkVisible_ON == null)		{ _guiContent_RestoreTmpWorkVisible_ON =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_ON)); }
			if (_guiContent_RestoreTmpWorkVisible_OFF == null)		{ _guiContent_RestoreTmpWorkVisible_OFF =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.RestoreTmpVisibility_OFF)); }

			if (_guiContent_OrderUp == null)		{ _guiContent_OrderUp =		apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp)); }
			if (_guiContent_OrderDown == null)		{ _guiContent_OrderDown =	apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown)); }
		}
		
		//추가 21.8.7 : 리소스를 다시 로딩할 경우 유닛 풀의 모든 유닛들의 리소스도 다시 로드해야한다.
		public void ReloadUnitResources()
		{
			_unitPool.CheckAndReInitUnit();
		}

		// Functions
		//---------------------------------------------
		public void ResetAllUnits()
		{
			_isNeedReset = false;
			_units_All.Clear();
			_units_Root.Clear();
			_object2Unit.Clear();

			

			ReloadGUIContent();

			//Pool 초기화
			if(_unitPool.IsInitialized)
			{
				_unitPool.PushAll();
			}
			else
			{
				_unitPool.Init();
			}

			//메인 루트들을 만들어주자
			//_rootUnit_Overall =		AddUnit_OnlyButton(null, "Portrait", CATEGORY.Overall_Name, null, true, null);
			_rootUnit_Overall = AddUnit_Label(null, Editor.GetUIWord(UIWORD.RootUnits), CATEGORY.Overall_Name, null, true, null);
			_rootUnit_Image = AddUnit_Label(null, Editor.GetUIWord(UIWORD.Images), CATEGORY.Images_Name, null, true, null);
			_rootUnit_Mesh = AddUnit_Label(null, Editor.GetUIWord(UIWORD.Meshes), CATEGORY.Mesh_Name, null, true, null);
			_rootUnit_MeshGroup = AddUnit_Label(null, Editor.GetUIWord(UIWORD.MeshGroups), CATEGORY.MeshGroup_Name, null, true, null);
			//_rootUnit_Face =		AddUnit_Label(null, "Faces", CATEGORY.Face_Name, null, true, null);
			_rootUnit_Animation = AddUnit_Label(null, Editor.GetUIWord(UIWORD.AnimationClips), CATEGORY.Animation_Name, null, true, null);
			_rootUnit_Param = AddUnit_Label(null, Editor.GetUIWord(UIWORD.ControlParameters), CATEGORY.Param_Name, null, true, null);

			if (Editor == null || Editor._portrait == null)
			{
				return;
			}



			//추가 20.7.6 : 선택된 객체를 생성할때 바로 확인해야한다.
			object selectedObject = null;
			switch (Editor.Select.SelectionType)
			{
				case apSelection.SELECTION_TYPE.Overall:	if(Editor.Select.RootUnit != null)		{ selectedObject = Editor.Select.RootUnit; } break;
				case apSelection.SELECTION_TYPE.ImageRes:	if(Editor.Select.TextureData != null)	{ selectedObject = Editor.Select.TextureData; } break;
				case apSelection.SELECTION_TYPE.Mesh:		if(Editor.Select.Mesh != null)			{ selectedObject = Editor.Select.Mesh; } break;
				case apSelection.SELECTION_TYPE.MeshGroup:	if(Editor.Select.MeshGroup != null)		{ selectedObject = Editor.Select.MeshGroup; } break;
				case apSelection.SELECTION_TYPE.Animation:	if(Editor.Select.AnimClip != null)		{ selectedObject = Editor.Select.AnimClip; } break;
				case apSelection.SELECTION_TYPE.Param:		if(Editor.Select.Param != null)			{ selectedObject = Editor.Select.Param; } break;
			}
			

			//0. 루트 유닛
			//기존 : 리스트 그대로
			//List<apRootUnit> rootUnits = Editor._portrait._rootUnits;
			//for (int i = 0; i < rootUnits.Count; i++)
			//{
			//	apRootUnit rootUnit = rootUnits[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), "Root Unit " + i, CATEGORY.Overall_Item, rootUnit, false, _rootUnit_Overall);
			//}

			//변경 3.29 : 정렬된 리스트
			List<apObjectOrders.OrderSet> rootUnitSets = Editor._portrait._objectOrders.RootUnits;
			for (int i = 0; i < rootUnitSets.Count; i++)
			{
				apObjectOrders.OrderSet rootUnitSet = rootUnitSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root_16px), //변경 22.6.10 : 16px로 변경
										"Root Unit " + i + " (" + rootUnitSet._linked_RootUnit.Name + ")",
										CATEGORY.Overall_Item, rootUnitSet._linked_RootUnit, selectedObject,
										false, _rootUnit_Overall, RIGHTCLICK_MENU.Enabled);
			}


			//1. 이미지 파일들을 검색하자
			//기존 : 리스트 그대로
			//List<apTextureData> textures = Editor._portrait._textureData;
			//for (int i = 0; i < textures.Count; i++)
			//{
			//	apTextureData textureData = textures[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image), textureData._name, CATEGORY.Images_Item, textureData, false, _rootUnit_Image);
			//}

			//변경 3.29 : 정렬된 리스트
			List<apObjectOrders.OrderSet> textureSets = Editor._portrait._objectOrders.Images;
			for (int i = 0; i < textureSets.Count; i++)
			{
				apObjectOrders.OrderSet textureDataSet = textureSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image_16px), //변경 22.6.10 : 16px로 변경
										textureDataSet._linked_Image._name, 
										CATEGORY.Images_Item, 
										textureDataSet._linked_Image, 
										selectedObject,
										false, _rootUnit_Image,
										RIGHTCLICK_MENU.Enabled);
			}

			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddImage), CATEGORY.Images_Add, null, false, _rootUnit_Image);
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_AddPSD), Editor.GetUIWord(UIWORD.ImportPSDFile), CATEGORY.Images_AddPSD, null, false, _rootUnit_Image);

			//2. 메시 들을 검색하자
			//기존 : 리스트 그대로
			//List<apMesh> meshes = Editor._portrait._meshes;
			//for (int i = 0; i < meshes.Count; i++)
			//{
			//	apMesh mesh = meshes[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh), mesh._name, CATEGORY.Mesh_Item, mesh, false, _rootUnit_Mesh);
			//}

			//변경 3.29 : 정렬된 리스트
			List<apObjectOrders.OrderSet> mesheSets = Editor._portrait._objectOrders.Meshes;
			for (int i = 0; i < mesheSets.Count; i++)
			{
				apObjectOrders.OrderSet meshSet = mesheSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh_16px), //변경 22.6.10 : 16px로 변경
										meshSet._linked_Mesh._name, 
										CATEGORY.Mesh_Item, 
										meshSet._linked_Mesh, 
										selectedObject,
										false, _rootUnit_Mesh,
										RIGHTCLICK_MENU.Enabled);
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddMesh), CATEGORY.Mesh_Add, null, false, _rootUnit_Mesh);

			//3. 메시 그룹들을 검색하자
			//메시 그룹들은 하위에 또다른 Mesh Group을 가지고 있다.
			//기존 : 리스트 그대로
			//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;

			//변경 : 정렬된 리스트
			List<apObjectOrders.OrderSet> meshGroupSets = Editor._portrait._objectOrders.MeshGroups;

			for (int i = 0; i < meshGroupSets.Count; i++)
			{
				//기존
				//apMeshGroup meshGroup = meshGroupSets[i];

				//변경
				apObjectOrders.OrderSet meshGrouSet = meshGroupSets[i];
				apMeshGroup meshGroup = meshGrouSet._linked_MeshGroup;

				if (meshGroup._parentMeshGroup == null || meshGroup._parentMeshGroupID < 0)
				{
					//Debug.Log("Reset H : MeshGroup(" + meshGroup._name + ") - Root");
					apEditorHierarchyUnit addedHierarchyUnit = AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px), //변경 22.6.10 : 16px로 변경
																						meshGroup._name, 
																						CATEGORY.MeshGroup_Item, 
																						meshGroup, 
																						selectedObject,
																						false, _rootUnit_MeshGroup,
																						RIGHTCLICK_MENU.Enabled);
					if (meshGroup._childMeshGroupTransforms.Count > 0)
					{
						AddUnit_SubMeshGroup(meshGroup, addedHierarchyUnit, selectedObject);
					}
				}
				//else
				//{
				//	//Debug.Log("Reset H : MeshGroup(" + meshGroup._name + ") - Child");
				//}
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddMeshGroup), CATEGORY.MeshGroup_Add, null, false, _rootUnit_MeshGroup);


			//7. 파라미터들을 검색하자
			//기존 : 리스트 그대로
			//List<apControlParam> cParams = Editor.ParamControl._controlParams;
			//for (int i = 0; i < cParams.Count; i++)
			//{
			//	apControlParam cParam = cParams[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param), cParam._keyName, CATEGORY.Param_Item, cParam, false, _rootUnit_Param);
			//}

			//변경 : 정렬된 리스트
			List<apObjectOrders.OrderSet> cParamSets = Editor._portrait._objectOrders.ControlParams;
			for (int i = 0; i < cParamSets.Count; i++)
			{
				apObjectOrders.OrderSet cParamSet = cParamSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param_16px), //변경 22.6.10 : 16px로 변경
										cParamSet._linked_ControlParam._keyName, 
										CATEGORY.Param_Item, 
										cParamSet._linked_ControlParam, 
										selectedObject,
										false, _rootUnit_Param,
										RIGHTCLICK_MENU.Enabled);
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddControlParameter), CATEGORY.Param_Add, null, false, _rootUnit_Param);


			//8. 애니메이션을 넣자
			//기존 : 리스트 그대로
			//List<apAnimClip> animClips = Editor._portrait._animClips;
			//for (int i = 0; i < animClips.Count; i++)
			//{
			//	apAnimClip animClip = animClips[i];
			//	AddUnit_ToggleButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation), animClip._name, CATEGORY.Animation_Item, animClip, false, _rootUnit_Animation);
			//}

			//변경 : 정렬된 리스트
			List<apObjectOrders.OrderSet> animClipSets = Editor._portrait._objectOrders.AnimClips;
			for (int i = 0; i < animClipSets.Count; i++)
			{
				apObjectOrders.OrderSet animClipSet = animClipSets[i];
				AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation_16px), //변경 22.6.10 : 16px로 변경
										animClipSet._linked_AnimClip._name, 
										CATEGORY.Animation_Item, 
										animClipSet._linked_AnimClip, 
										selectedObject,
										false, _rootUnit_Animation,
										RIGHTCLICK_MENU.Enabled);
			}
			AddUnit_OnlyButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add), Editor.GetUIWord(UIWORD.AddAnimationClip), CATEGORY.Animation_Add, null, false, _rootUnit_Animation);
		}


		private void AddUnit_SubMeshGroup(apMeshGroup parentMeshGroup, apEditorHierarchyUnit parentUnit, object selectedObject)
		{
			for (int iChild = 0; iChild < parentMeshGroup._childMeshGroupTransforms.Count; iChild++)
			{
				if (parentMeshGroup._childMeshGroupTransforms[iChild]._meshGroup != null)
				{
					apMeshGroup childMeshGroup = parentMeshGroup._childMeshGroupTransforms[iChild]._meshGroup;
					apEditorHierarchyUnit hierarchyUnit = AddUnit_ToggleButton(	Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px), //변경 22.6.10 : 16px로 변경
																				childMeshGroup._name, 
																				CATEGORY.MeshGroup_Item, 
																				childMeshGroup, 
																				selectedObject,
																				false, parentUnit,
																				RIGHTCLICK_MENU.Enabled,
																				ORDER_CHANGABLE.None);

					if (childMeshGroup._childMeshGroupTransforms.Count > 0)
					{
						AddUnit_SubMeshGroup(childMeshGroup, hierarchyUnit, selectedObject);
					}
				}
			}
		}

		private apEditorHierarchyUnit AddUnit_Label(Texture2D icon, string text, CATEGORY savedKey, object savedObj, bool isRoot, apEditorHierarchyUnit parent)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//19.11.16
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);
			

			newUnit.SetEvent(OnUnitClick, OnCheckSelectedHierarchy);
			newUnit.SetLabel(icon, text, (int)savedKey, savedObj);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				_units_Root.Add(newUnit);
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

		private enum ORDER_CHANGABLE
		{
			None, Changable
		}

		private apEditorHierarchyUnit AddUnit_ToggleButton(	Texture2D icon, string text, 
															CATEGORY savedKey, object savedObj, object curSelectedObj,
															bool isRoot, apEditorHierarchyUnit parent, 
															RIGHTCLICK_MENU rightClickSupported,
															ORDER_CHANGABLE isOrderChangable = ORDER_CHANGABLE.Changable)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown)
			//							);

			//19.11.16
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted,
										_guiContent_OrderUp,
										_guiContent_OrderDown
										);
			

			if(isOrderChangable == ORDER_CHANGABLE.Changable)
			{
				newUnit.SetEvent(OnUnitClick, null, OnCheckSelectedHierarchy, OnUnitClickOrderChanged);
			}
			else
			{
				newUnit.SetEvent(OnUnitClick, OnCheckSelectedHierarchy);
			}

			//추가 21.6.13 : 우클릭 지원
			if(rightClickSupported == RIGHTCLICK_MENU.Enabled)
			{
				newUnit.SetRightClickEvent(OnUnitRightClick);
			}
			

			newUnit.SetToggleButton(icon, text, (int)savedKey, savedObj, false);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				_units_Root.Add(newUnit);
			}

			//추가 [1.4.2] 오브젝트 매핑
			if(savedObj != null)
			{
				if(!_object2Unit.ContainsKey(savedObj))
				{
					_object2Unit.Add(savedObj, newUnit);
				}
			}

			if (parent != null)
			{
				newUnit.SetParent(parent);
				parent.AddChild(newUnit);
			}

			//추가 20.7.6 : 선택 여부를 추가할때 알려줘야 한다.
			newUnit.SetSelected(curSelectedObj == savedObj && curSelectedObj != null, true);

			return newUnit;
		}

		private apEditorHierarchyUnit AddUnit_OnlyButton(Texture2D icon, string text, CATEGORY savedKey, object savedObj, bool isRoot, apEditorHierarchyUnit parent)
		{
			//이전
			//apEditorHierarchyUnit newUnit = new apEditorHierarchyUnit();

			//변경 20.3.18
			apEditorHierarchyUnit newUnit = _unitPool.PullUnit((parent == null) ? 0 : (parent._level + 1));

			//newUnit.SetBasicIconImg(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight),
			//							Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Registered));

			//19.11.16
			newUnit.SetBasicIconImg(	_guiContent_FoldDown,
										_guiContent_FoldRight,
										_guiContent_ModRegisted);


			newUnit.SetEvent(OnUnitClick, OnCheckSelectedHierarchy);
			newUnit.SetOnlyButton(icon, text, (int)savedKey, savedObj);

			_units_All.Add(newUnit);
			if (isRoot)
			{
				_units_Root.Add(newUnit);
			}

			//추가 [1.4.2] 오브젝트 매핑
			if(savedObj != null)
			{
				if(!_object2Unit.ContainsKey(savedObj))
				{
					_object2Unit.Add(savedObj, newUnit);
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
			if (Editor == null || Editor._portrait == null || _isNeedReset)
			{
				ResetAllUnits();

				return;
			}

			ReloadGUIContent();

			//Pool 초기화
			if(!_unitPool.IsInitialized)
			{
				_unitPool.Init();
			}


			List<apEditorHierarchyUnit> deletedUnits = new List<apEditorHierarchyUnit>();
			
			//0. 루트 유닛들을 검색하자
			//이전
			//List<apRootUnit> rootUnits = Editor._portrait._rootUnits;

			//변경
			List<apObjectOrders.OrderSet> rootUnitSets = Editor._portrait._objectOrders.RootUnits;
			for (int i = 0; i < rootUnitSets.Count; i++)
			{
				//이전
				//apRootUnit rootUnit = Editor._portrait._rootUnits[i];
				
				//변경
				apRootUnit rootUnit = rootUnitSets[i]._linked_RootUnit;
				RefreshUnit(CATEGORY.Overall_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root_16px),//변경 22.6.10 : 16px로 변경
								rootUnit,
								"Root Unit " + i + " (" + rootUnit.Name + ")",//<<변경
								Editor.Select.RootUnit,
								_rootUnit_Overall, 
								i,
								RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apRootUnit>(deletedUnits, CATEGORY.Overall_Item, rootUnits);

			//변경
			CheckRemovableUnits<apRootUnit>(deletedUnits, CATEGORY.Overall_Item, Editor._portrait._rootUnits);


			//1. 이미지 파일들을 검색하자 -> 있는건 없애고, 없는건 만들자
			//이전
			//List<apTextureData> textures = Editor._portrait._textureData;

			//변경
			List<apObjectOrders.OrderSet> textureSets = Editor._portrait._objectOrders.Images;
			for (int i = 0; i < textureSets.Count; i++)
			{
				//이전
				//apTextureData textureData = textures[i];

				//변경
				apTextureData textureData = textureSets[i]._linked_Image;
				RefreshUnit(CATEGORY.Images_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image_16px),//변경 22.6.10 : 16px로 변경
								textureData,
								textureData._name,
								Editor.Select.TextureData,
								_rootUnit_Image,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apTextureData>(deletedUnits, CATEGORY.Images_Item, textures);

			//변경
			CheckRemovableUnits<apTextureData>(deletedUnits, CATEGORY.Images_Item, Editor._portrait._textureData);



			//2. 메시 들을 검색하자
			//이전
			//List<apMesh> meshes = Editor._portrait._meshes;
			
			//변경
			List<apObjectOrders.OrderSet> mesheSets = Editor._portrait._objectOrders.Meshes;
			for (int i = 0; i < mesheSets.Count; i++)
			{
				//이전
				//apMesh mesh = meshes[i];

				//변경
				apMesh mesh = mesheSets[i]._linked_Mesh;
				RefreshUnit(CATEGORY.Mesh_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh_16px),//변경 22.6.10 : 16px로 변경
								mesh,
								mesh._name,
								Editor.Select.Mesh,
								_rootUnit_Mesh,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apMesh>(deletedUnits, CATEGORY.Mesh_Item, meshes);
			
			//변경
			CheckRemovableUnits<apMesh>(deletedUnits, CATEGORY.Mesh_Item, Editor._portrait._meshes);


			//3. Mesh Group들을 검색하자
			//이전
			//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
			//변경
			List<apObjectOrders.OrderSet> meshGroupSets = Editor._portrait._objectOrders.MeshGroups;

			for (int i = 0; i < meshGroupSets.Count; i++)
			{
				//이건 재귀 함수 -_-;
				apMeshGroup meshGroup = meshGroupSets[i]._linked_MeshGroup;
				if (meshGroup._parentMeshGroup == null)
				{
					RefreshUnit_MeshGroup(meshGroup, _rootUnit_MeshGroup, i);
				}
			}
			//이전
			//CheckRemovableUnits<apMeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, meshGroups);

			//변경
			CheckRemovableUnits<apMeshGroup>(deletedUnits, CATEGORY.MeshGroup_Item, Editor._portrait._meshGroups);


			//7. 파라미터들을 검색하자
			//이전
			//List<apControlParam> cParams = Editor.ParamControl._controlParams;
			
			//변경
			List<apObjectOrders.OrderSet> cParamSets = Editor._portrait._objectOrders.ControlParams;
			for (int i = 0; i < cParamSets.Count; i++)
			{
				//이전
				//apControlParam cParam = cParams[i];

				//변경
				apControlParam cParam = cParamSets[i]._linked_ControlParam;
				RefreshUnit(CATEGORY.Param_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param_16px),//변경 22.6.10 : 16px로 변경
								cParam,
								cParam._keyName,
								Editor.Select.Param,
								_rootUnit_Param,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apControlParam>(deletedUnits, CATEGORY.Param_Item, cParams);
			
			//변경
			CheckRemovableUnits<apControlParam>(deletedUnits, CATEGORY.Param_Item, Editor.ParamControl._controlParams);


			//8. 애니메이션을 넣자
			//이전
			//List<apAnimClip> animClips = Editor._portrait._animClips;

			List<apObjectOrders.OrderSet> animClipSets = Editor._portrait._objectOrders.AnimClips;
			for (int i = 0; i < animClipSets.Count; i++)
			{
				//이전
				//apAnimClip animClip = animClips[i];
				//변경
				apAnimClip animClip = animClipSets[i]._linked_AnimClip;
				RefreshUnit(CATEGORY.Animation_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation_16px),//변경 22.6.10 : 16px로 변경
								animClip,
								animClip._name,
								Editor.Select.AnimClip,
								_rootUnit_Animation,
								i, RIGHTCLICK_MENU.Enabled);
			}
			//이전
			//CheckRemovableUnits<apAnimClip>(deletedUnits, CATEGORY.Animation_Item, animClips);
			
			//변경
			CheckRemovableUnits<apAnimClip>(deletedUnits, CATEGORY.Animation_Item, Editor._portrait._animClips);

			//삭제할 유닛을 체크하고 계산하자
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
					_object2Unit.Remove(dUnit._savedObj);
				}


				//20.3.18 그냥 삭제하지 말고 Pool에 넣자
				_unitPool.PushUnit(dUnit);
			}

			//전체 Sort를 한다.
			//재귀적으로 실행
			for (int i = 0; i < _units_Root.Count; i++)
			{
				SortUnit_Recv(_units_Root[i]);
			}
		}



		private void RefreshUnit_MeshGroup(apMeshGroup parentMeshGroup, apEditorHierarchyUnit refreshedHierarchyUnit, int indexPerParent)
		{
			apEditorHierarchyUnit unit = RefreshUnit(CATEGORY.MeshGroup_Item,
								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup_16px),//변경 22.6.10 : 16px로 변경
								parentMeshGroup,
								parentMeshGroup._name,
								Editor.Select.MeshGroup,
								refreshedHierarchyUnit,
								indexPerParent,
								RIGHTCLICK_MENU.Enabled,
								(parentMeshGroup._parentMeshGroup == null) ? ORDER_CHANGABLE.Changable : ORDER_CHANGABLE.None//Root MeshGroup만 순서 바꿀 수 있다.
								);

			if (parentMeshGroup._childMeshGroupTransforms.Count > 0)
			{

				for (int i = 0; i < parentMeshGroup._childMeshGroupTransforms.Count; i++)
				{
					apMeshGroup childMeshGroup = parentMeshGroup._childMeshGroupTransforms[i]._meshGroup;
					if (childMeshGroup != null)
					{
						RefreshUnit_MeshGroup(childMeshGroup, unit, i);
					}
				}
			}
		}


		private apEditorHierarchyUnit RefreshUnit(	CATEGORY category, 
													Texture2D iconImage, 
													object obj, 
													string objName, 
													object selectedObj, 
													apEditorHierarchyUnit parentUnit,
													int indexPerParent,
													RIGHTCLICK_MENU rightClickMenu,
													ORDER_CHANGABLE orderChangable = ORDER_CHANGABLE.Changable)
		{
			apEditorHierarchyUnit unit = _units_All.Find(delegate (apEditorHierarchyUnit a)
			{
				if (obj != null)
				{
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
				if (selectedObj != null && unit._savedObj == selectedObj)
				{
					//unit._isSelected = true;
					unit.SetSelected(true, true);
				}
				else
				{
					//unit._isSelected = false;
					unit.SetSelected(false, true);
				}

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
			}
			else
			{
				unit = AddUnit_ToggleButton(iconImage, objName, category, obj, selectedObj, false, parentUnit, rightClickMenu, orderChangable);
			}

			//추가 3.29 : Refresh의 경우 Index를 외부에서 지정한다.
			unit._indexPerParent = indexPerParent;

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
					if (a._savedKey == b._savedKey)
					{
						return a._indexPerParent - b._indexPerParent;
					}
					return a._savedKey - b._savedKey;
				});

				for (int i = 0; i < unit._childUnits.Count; i++)
				{
					SortUnit_Recv(unit._childUnits[i]);
				}
			}
		}


		// 일반 이벤트
		//-----------------------------------------------------------------------------------------
		//추가 22.12.12 : 막 클릭한 Hierarchy는 별도의 단축키가 적용된다.
		public bool OnCheckSelectedHierarchy(apEditorHierarchyUnit unit)
		{
			if(unit == null || unit._savedObj == null)
			{
				return false;
			}

			return Editor.LastClickedHierarchy == apEditor.LAST_CLICKED_HIERARCHY.Main;
		}


		// Click Event
		//-----------------------------------------------------------------------------------------
		public void OnUnitClick(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isCtrl, bool isShift)
		{
			if (Editor == null)
			{
				return;
			}

			apEditorHierarchyUnit selectedUnit = null;

			
			//여기서 이벤트를 설정해주자
			bool isAnyAdded = false;//추가 20.7.14 : 뭔가 추가가 되었다면, Refresh를 해야한다.
			
			//v1.4.2 : 애니메이션 추가시 스크롤이 자동으로 바뀌는데, 새로 생성한 직후에는 RefreshUnit 이후에 스크롤을 해야하므로 처리가 딜레이된다.
			bool isDelayedAutoScroll_AnimClip = false;
			apAnimClip delayedScrollObj_AnimClip = null;

			


			CATEGORY category = (CATEGORY)savedKey;

			//처리가 필요없는 Label 메뉴는 바로 리턴하자
			switch (category)
			{
				case CATEGORY.Overall_Name:
				case CATEGORY.Images_Name:
				case CATEGORY.Mesh_Name:
				case CATEGORY.MeshGroup_Name:
				case CATEGORY.Animation_Name:
				case CATEGORY.Param_Name:
					return;//<<리턴
			}

			//v1.4.2 : FFD가 켜진 상태에서는 메뉴 선택이 제한될 수 있다.
			bool isExecutable = Editor.CheckModalAndExecutable();
			if(!isExecutable)
			{
				return;
			}

			switch (category)
			{
				case CATEGORY.Overall_Item:
					//전체 선택
					apRootUnit rootUnit = savedObj as apRootUnit;
					if (rootUnit != null)
					{
						Editor.Select.SelectRootUnit(rootUnit);
						if (Editor.Select.RootUnit == rootUnit)
						{
							selectedUnit = eventUnit;

							//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
						}
					}
					break;

				case CATEGORY.Images_Item:
					{
						apTextureData textureData = savedObj as apTextureData;
						if (textureData != null)
						{
							Editor.Select.SelectImage(textureData);//<< 선택하자
							if (Editor.Select.TextureData == textureData)
							{
								selectedUnit = eventUnit;

								//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
								_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
							}
						}
					}
					break;

				case CATEGORY.Images_Add:
					{
						Editor.Controller.AddImage();
						isAnyAdded = true;
					}
					break;

				case CATEGORY.Images_AddPSD://추가 : PSD 로드
					{
						Editor.Controller.ShowPSDLoadDialog();
					}
					break;

				case CATEGORY.Mesh_Item:
					{
						apMesh mesh = savedObj as apMesh;
						if (mesh != null)
						{
							Editor.Select.SelectMesh(mesh);//<< 선택하자

							if (Editor.Select.Mesh == mesh)
							{
								selectedUnit = eventUnit;

								//추가 20.7.6 : 여기서 메시를 열때에만
								//PSD 파일로부터 열린 메시의 버텍스를 초기화할 것인지 물어보기
								if(mesh._isNeedToAskRemoveVertByPSDImport)
								{
									//물어보기 요청
									Editor._isRequestRemoveVerticesIfImportedFromPSD_Step1 = true;
									Editor._isRequestRemoveVerticesIfImportedFromPSD_Step2 = false;
									Editor._requestMeshRemoveVerticesIfImportedFromPSD = mesh;
								}

								//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
								_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
							}
						}
					}
					break;

				case CATEGORY.Mesh_Add:
					{
						apMesh newMesh = Editor.Controller.AddMesh();
						if(newMesh != null)
						{
							//추가 : 새로 Mesh를 추가한 경우, 다음에 이 Mesh를 선택할 때에는 (새거이므로)
							//MeshEdit 탭을 Setting으로 설정해야한다.
							if(!Editor.Select._createdNewMeshes.Contains(newMesh))
							{
								Editor.Select._createdNewMeshes.Add(newMesh);
							}
							
							//추가 21.3.6 : 새로 생성된 메시는 이미지를 할당할 수도 있다.
							Editor.Controller.CheckAndSetImageToMeshAutomatically(newMesh);
							

							Editor.Select.SelectMesh(newMesh);//<< 선택하자
						}
						isAnyAdded = true;
					}
					
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apMeshGroup meshGroup = savedObj as apMeshGroup;
						if (meshGroup != null)
						{
							Editor.Select.SelectMeshGroup(meshGroup);

							if (Editor.Select.MeshGroup == meshGroup)
							{
								selectedUnit = eventUnit;

								//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
								_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Add:
					{
						apMeshGroup newMeshGroup = Editor.Controller.AddMeshGroup();
						if (newMeshGroup != null)
						{
							Editor.Select.SelectMeshGroup(newMeshGroup);
						}
						isAnyAdded = true;
					}
					
					break;

				//case CATEGORY.Face_Item:
				//	break;

				//case CATEGORY.Face_Add:
				//	break;

				case CATEGORY.Animation_Item:
					{
						apAnimClip animClip = savedObj as apAnimClip;
						if (animClip != null)
						{
							Editor.Select.SelectAnimClip(animClip);
							if (Editor.Select.AnimClip == animClip)
							{
								selectedUnit = eventUnit;

								//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
								_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);

								//[v1.4.2]
								//애니메이션의 경우 UI가 작아지므로,
								//스크롤을 자동으로 해야한다.
								Editor.AutoScroll_HierarchyMain(animClip);
							}
						}
					}
					break;

				case CATEGORY.Animation_Add:
					{
						//데모 기능 제한
						//Param 개수는 2개로 제한되며, 이걸 넘어가면 추가할 수 없다.
						if (apVersion.I.IsDemo)
						{
							if (Editor._portrait._animClips.Count >= 2)
							{
								//이미 2개를 넘었다.
								EditorUtility.DisplayDialog(
									Editor.GetText(TEXT.DemoLimitation_Title),
									Editor.GetText(TEXT.DemoLimitation_Body_AddAnimation),
									Editor.GetText(TEXT.Okay)
									);

								break;
							}
						}

						apAnimClip newAnimClip = Editor.Controller.AddAnimClip();
						if(newAnimClip != null)
						{
							Editor.Select.SelectAnimClip(newAnimClip);

							//애니메이션 생성 직후 스크롤을 조절한다.
							isDelayedAutoScroll_AnimClip = true;
							delayedScrollObj_AnimClip = newAnimClip;
						}

						isAnyAdded = true;
					}
					
					break;

				case CATEGORY.Param_Item:
					{
						apControlParam cParam = savedObj as apControlParam;
						if (cParam != null)
						{
							Editor.Select.SelectControlParam(cParam);

							if (Editor.Select.Param == cParam)
							{
								selectedUnit = eventUnit;

								//추가 22.10.27 [v1.4.2] : Hierarchy 클릭을 Editor에 알린다.
								_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
							}
						}
					}
					break;

				case CATEGORY.Param_Add:
					{
						//데모 기능 제한
						//Param 개수는 2개로 제한되며, 이걸 넘어가면 추가할 수 없다.
						if (apVersion.I.IsDemo)
						{
							if (Editor.ParamControl._controlParams.Count >= 2)
							{
								//이미 2개를 넘었다.
								EditorUtility.DisplayDialog(
									Editor.GetText(TEXT.DemoLimitation_Title),
									Editor.GetText(TEXT.DemoLimitation_Body_AddParam),
									Editor.GetText(TEXT.Okay)
									);

								break;
							}
						}

						//Param 추가
						Editor.Controller.AddParam();

						isAnyAdded = true;
					}
					
					break;
			}

			if (isAnyAdded)
			{
				RefreshUnits();

				if (isDelayedAutoScroll_AnimClip
					&& delayedScrollObj_AnimClip != null)
				{
					//Refresh 이후 애니메이션 클립에 의한 스크롤을 해야한다.
					Editor.AutoScroll_HierarchyMain(delayedScrollObj_AnimClip, true);
				}			
			}
			else
			{
				if (selectedUnit != null)
				{
					for (int i = 0; i < _units_All.Count; i++)
					{
						if (_units_All[i] == selectedUnit)
						{
							//_units_All[i]._isSelected = true;
							_units_All[i].SetSelected(true, true);
						}
						else
						{
							//_units_All[i]._isSelected = false;
							_units_All[i].SetSelected(false, true);
						}
					}
				}
				else
				{
					for (int i = 0; i < _units_All.Count; i++)
					{
						//_units_All[i]._isSelected = false;
						_units_All[i].SetSelected(false, true);
					}
				}
			}
		}




		public void OnUnitClickOrderChanged(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isOrderUp)
		{
			//Hierarchy의 항목 순서를 바꾸자
			if (Editor == null || Editor._portrait == null)
			{
				return;
			}
			apObjectOrders orders = Editor._portrait._objectOrders;

			bool isChanged = false;
			bool isResult = false;
			CATEGORY category = (CATEGORY)savedKey;
			switch (category)
			{
				case CATEGORY.Overall_Item:
					{
						apRootUnit rootUnit = savedObj as apRootUnit;
						if(rootUnit != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Images_Item:
					{
						apTextureData textureData = savedObj as apTextureData;
						if(textureData != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Image, textureData._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Mesh_Item:
					{
						apMesh mesh = savedObj as apMesh;
						if(mesh != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Mesh, mesh._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apMeshGroup meshGroup = savedObj as apMeshGroup;
						if(meshGroup != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.MeshGroup, meshGroup._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Animation_Item:
					{
						apAnimClip animClip = savedObj as apAnimClip;
						if(animClip != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.AnimClip, animClip._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;

				case CATEGORY.Param_Item:
					{
						apControlParam cParam = savedObj as apControlParam;
						if(cParam != null)
						{
							isResult = orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.ControlParam, cParam._uniqueID, isOrderUp);
							if(isResult)
							{
								isChanged = true;
							}
						}
					}
					break;
			}

			if(isChanged)
			{
				apEditorUtil.SetEditorDirty();
				Editor.RefreshControllerAndHierarchy(false);
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
				//v1.4.2 : FFD가 켜지면 적용 여부를 묻고 경우에 따라 우클릭 메뉴를 보이지 않는다.
				bool isExecutable = Editor.CheckModalAndExecutable();
				if(!isExecutable)
				{
					return;
				}


				//우클릭 메뉴를 열자 (해당되는 항목만)

				//클릭한 객체의 이름을 받자
				string objName = null;
				bool isShowMenu = false;

				_rightMenu.ReadyToMakeMenu();//메뉴 구성 시작

				
				CATEGORY category = (CATEGORY)savedKey;

				switch (category)
				{
					case CATEGORY.Overall_Item:
						{
							//루트유닛
							isShowMenu = true;
							
							if(savedObj is apRootUnit)
							{
								objName = (savedObj as apRootUnit).Name;
							}

							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Remove();
						}
						break;
					case CATEGORY.Images_Item:
						{
							//이미지
							isShowMenu = true;
							
							if(savedObj is apTextureData)
							{
								objName = (savedObj as apTextureData)._name;
							}

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Remove();
							_rightMenu.SetMenu_RemoveMultiple();
						}
						break;

					case CATEGORY.Mesh_Item:
						{
							//메시
							isShowMenu = true;
							
							if(savedObj is apMesh)
							{
								objName = (savedObj as apMesh)._name;
							}

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Duplicate();
							_rightMenu.SetMenu_Remove();
							_rightMenu.SetMenu_RemoveMultiple();
						}
						break;
					case CATEGORY.MeshGroup_Item:
						{
							//메시 그룹
							isShowMenu = true;
							
							if(savedObj is apMeshGroup)
							{
								objName = (savedObj as apMeshGroup)._name;
							}

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Duplicate();
							_rightMenu.SetMenu_Remove();
							_rightMenu.SetMenu_RemoveMultiple();

							apMeshGroup meshGroup = savedObj as apMeshGroup;
							if(meshGroup != null && meshGroup._parentMeshGroup == null)
							{
								//Root MeshGroup이라면 Move Up/Move Down을 추가한다.
								_rightMenu.SetMenu_MoveUpDown();
							}
						}
						break;
					case CATEGORY.Animation_Item:
						{
							//애니메이션
							isShowMenu = true;
							
							if(savedObj is apAnimClip)
							{
								objName = (savedObj as apAnimClip)._name;
							}

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Duplicate();
							_rightMenu.SetMenu_Remove();
							_rightMenu.SetMenu_RemoveMultiple();
						}
						break;
					case CATEGORY.Param_Item:
						{
							//컨트롤 파라미터
							isShowMenu = true;
							
							if(savedObj is apControlParam)
							{
								objName = (savedObj as apControlParam)._keyName;
							}

							_rightMenu.SetMenu_Rename();
							_rightMenu.SetMenu_MoveUpDown();
							_rightMenu.SetMenu_Search();
							_rightMenu.SetMenu_Duplicate();
							_rightMenu.SetMenu_Remove();
							_rightMenu.SetMenu_RemoveMultiple();
						}
					break;
				}


				if (isShowMenu)
				{
					//우클릭 메뉴를 보여주자
					_rightMenu.ShowMenu(objName, 1, savedKey, savedObj, eventUnit);
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
			apObjectOrders orders = _editor._portrait._objectOrders;//위치 변경용

			//변경사항 v1.4.2
			//- 만약 위치 변경시 Hierarchy 정렬 모드가 Custom이 아니라면 변경하지 말고 메시지를 보여야 한다.
			//- 복제시 복제된 객체를 바로 선택한다. 이때, AnimClip의 경우 스크롤도 자동으로 이동한다. (Refresh 후에 하도록 변수를 만들자)

			bool isDelayedAutoScroll_AnimClip = false;
			apAnimClip delayedScrollTargetAnimClip = null;


			CATEGORY category = (CATEGORY)hierachyUnitType;
			switch (category)
			{
				case CATEGORY.Overall_Item:
					{
						apRootUnit rootUnit = requestedObj as apRootUnit;
						if(rootUnit != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
									{
										orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, true);

										//변경 후엔 해당 객체를 선택하자
										if(Editor.Select.RootUnit != rootUnit)
										{
											Editor.Select.SelectRootUnit(rootUnit);
											if (Editor.Select.RootUnit == rootUnit)
											{
												_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
											}
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.RootUnit, rootUnit._childMeshGroup._uniqueID, false);

										//변경 후엔 해당 객체를 선택하자
										if(Editor.Select.RootUnit != rootUnit)
										{
											Editor.Select.SelectRootUnit(rootUnit);
											if (Editor.Select.RootUnit == rootUnit)
											{
												_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
											}
										}
									}
									
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										//Unregister
										apMeshGroup targetRootMeshGroup = rootUnit._childMeshGroup;
										if (targetRootMeshGroup != null)
										{
											bool isNeedToNone = _editor.Select.RootUnit == rootUnit;
											apEditorUtil.SetRecord_PortraitMeshGroup(apUndoGroupData.ACTION.Portrait_SetMeshGroup, _editor, _editor._portrait, targetRootMeshGroup, false, true, apEditorUtil.UNDO_STRUCT.StructChanged);

											_editor._portrait._mainMeshGroupIDList.Remove(targetRootMeshGroup._uniqueID);
											_editor._portrait._mainMeshGroupList.Remove(targetRootMeshGroup);

											_editor._portrait._rootUnits.Remove(rootUnit);

											if (isNeedToNone)
											{
												_editor.Select.SelectNone();
											}
										}
									}

									break;
							}
						}
					}
					break;

				case CATEGORY.Images_Item:
					{
						apTextureData textureData = requestedObj as apTextureData;
						if(textureData != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, textureData, clickedUnit, textureData._name, OnObjectRenamed);
									break;


								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										if(Editor.HierarchySortMode == apEditor.HIERARCHY_SORT_MODE.Custom)
										{
											//정렬 모드가 "사용자 정의 모드"라면 > 위치 이동
											bool isMoveUp = menuType == apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp;

											orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Image, textureData._uniqueID, isMoveUp);

											//위치 변경 후 해당 객체를 선택한다.
											if (Editor.Select.TextureData != textureData)
											{
												Editor.Select.SelectImage(textureData);
												if (Editor.Select.TextureData == textureData)
												{
													_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
												}
											}
										}
										else
										{
											//정렬 모드를 사용자 정의 모드로 바꿀지 물어보고 처리
											bool isResult = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_WarningChangeSortMode_Title),
																						Editor.GetText(TEXT.DLG_WarningChangeSortMode_Body),
																						Editor.GetText(TEXT.DLG_Change),
																						Editor.GetText(TEXT.Cancel));
											if(isResult)
											{
												Editor.SetHierarchySortMode(apEditor.HIERARCHY_SORT_MODE.Custom);
											}
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									{
										_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									}
									
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										//경고 메시지를 먼저 보여준다.
										string strDialogInfo = _editor.Controller.GetRemoveItemMessage(
																				_editor._portrait,
																				textureData,
																				5,
																				_editor.GetTextFormat(TEXT.RemoveImage_Body, textureData._name),
																				_editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

										bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveImage_Title),
																strDialogInfo,
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));

										if (isResult)
										{
											_editor.Controller.RemoveTexture(textureData);
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.RemoveMultiple:
									{
										_loadKey_MultipleObjectRemoved = 
											apDialog_SelectMainObjects.ShowDialog(	_editor, 
																					apDialog_SelectMainObjects.REQUEST_TYPE.RemoveMainObjects, 
																					apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.Image,
																					OnMultipleObjectRemoved);
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Mesh_Item:
					{
						apMesh mesh = requestedObj as apMesh;
						if(mesh != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									{
										_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, mesh, clickedUnit, mesh._name, OnObjectRenamed);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										if (Editor.HierarchySortMode == apEditor.HIERARCHY_SORT_MODE.Custom)
										{
											//정렬 모드가 "사용자 정의 모드"라면 > 위치 이동
											bool isMoveUp = menuType == apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp;

											orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.Mesh, mesh._uniqueID, isMoveUp);

											//위치 변경 후 해당 객체를 선택한다.
											if (Editor.Select.Mesh != mesh)
											{
												Editor.Select.SelectMesh(mesh);
												if (Editor.Select.Mesh == mesh)
												{
													_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
												}
											}
											
										}
										else
										{
											//정렬 모드를 사용자 정의 모드로 바꿀지 물어보고 처리
											bool isResult = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_WarningChangeSortMode_Title),
																						Editor.GetText(TEXT.DLG_WarningChangeSortMode_Body),
																						Editor.GetText(TEXT.DLG_Change),
																						Editor.GetText(TEXT.Cancel));
											if(isResult)
											{
												Editor.SetHierarchySortMode(apEditor.HIERARCHY_SORT_MODE.Custom);
											}
										}
									}
									break;


								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									{
										_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										apMesh duplicatedMesh = _editor.Controller.DuplicateMesh(mesh);
										if(duplicatedMesh != null)
										{
											//복제가 되었다면 바로 선택 [v1.4.2]
											Editor.Select.SelectMesh(duplicatedMesh);
											if (Editor.Select.Mesh == duplicatedMesh)
											{
												_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
											}
										}
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										string strRemoveDialogInfo = _editor.Controller.GetRemoveItemMessage(
																				_editor._portrait,
																				mesh,
																				5,
																				_editor.GetTextFormat(TEXT.RemoveMesh_Body, mesh._name),
																				_editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																				);

										bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveMesh_Title),
																						strRemoveDialogInfo,
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel));

										if (isResult)
										{
											_editor.Controller.RemoveMesh(mesh);
										}
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.RemoveMultiple:
									{
										_loadKey_MultipleObjectRemoved = 
											apDialog_SelectMainObjects.ShowDialog(	_editor, 
																					apDialog_SelectMainObjects.REQUEST_TYPE.RemoveMainObjects, 
																					apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.Mesh,
																					OnMultipleObjectRemoved);
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.MeshGroup_Item:
					{
						apMeshGroup meshGroup = requestedObj as apMeshGroup;
						if(meshGroup != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									{
										_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, meshGroup, clickedUnit, meshGroup._name, OnObjectRenamed);
									}
									break;

									//Root MeshGroup인 경우에만 호출될 것이다.
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										if (Editor.HierarchySortMode == apEditor.HIERARCHY_SORT_MODE.Custom)
										{
											//정렬 모드가 "사용자 정의 모드"라면 > 위치 이동
											bool isMoveUp = menuType == apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp;
											orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.MeshGroup, meshGroup._uniqueID, isMoveUp);

											//위치 변경 후 해당 객체를 선택한다.
											if (Editor.Select.MeshGroup != meshGroup)
											{
												Editor.Select.SelectMeshGroup(meshGroup);
												if (Editor.Select.MeshGroup == meshGroup)
												{
													_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
												}
											}
										}
										else
										{
											//정렬 모드를 사용자 정의 모드로 바꿀지 물어보고 처리
											bool isResult = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_WarningChangeSortMode_Title),
																						Editor.GetText(TEXT.DLG_WarningChangeSortMode_Body),
																						Editor.GetText(TEXT.DLG_Change),
																						Editor.GetText(TEXT.Cancel));
											if(isResult)
											{
												Editor.SetHierarchySortMode(apEditor.HIERARCHY_SORT_MODE.Custom);
											}
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									{
										_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										apMeshGroup duplicatedMeshGroup = _editor.Controller.DuplicateMeshGroup(meshGroup, null, meshGroup._parentMeshGroup == null, true);
										if(duplicatedMeshGroup != null)
										{
											//복제가 되었다면 바로 선택 [v1.4.2]
											Editor.Select.SelectMeshGroup(duplicatedMeshGroup);
											if (Editor.Select.MeshGroup == duplicatedMeshGroup)
											{
												_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
											}
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										string strRemoveDialogInfo = _editor.Controller.GetRemoveItemMessage(
																				_editor._portrait,
																				meshGroup,
																				5,
																				Editor.GetTextFormat(TEXT.RemoveMeshGroup_Body, meshGroup._name),
																				Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																				);

										bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveMeshGroup_Title),
																						strRemoveDialogInfo,
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel)
																						);
										if (isResult)
										{
											_editor.Controller.RemoveMeshGroup(meshGroup);
										}
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.RemoveMultiple:
									{
										_loadKey_MultipleObjectRemoved = 
											apDialog_SelectMainObjects.ShowDialog(	_editor, 
																					apDialog_SelectMainObjects.REQUEST_TYPE.RemoveMainObjects, 
																					apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.MeshGroup,
																					OnMultipleObjectRemoved);
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Animation_Item:
					{
						apAnimClip animClip = requestedObj as apAnimClip;
						if (animClip != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									{
										_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, animClip, clickedUnit, animClip._name, OnObjectRenamed);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										if (Editor.HierarchySortMode == apEditor.HIERARCHY_SORT_MODE.Custom)
										{
											//정렬 모드가 "사용자 정의 모드"라면 > 위치 이동
											bool isMoveUp = menuType == apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp;

											orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.AnimClip, animClip._uniqueID, isMoveUp);

											//위치 변경 후 해당 객체를 선택한다.
											if (Editor.Select.AnimClip != animClip)
											{
												Editor.Select.SelectAnimClip(animClip);
												if (Editor.Select.AnimClip == animClip)
												{
													_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);

													//애니메이션의 경우 UI가 작아지므로, 스크롤을 자동으로 해야한다.
													Editor.AutoScroll_HierarchyMain(animClip);
												}
											}
											
										}
										else
										{
											//정렬 모드를 사용자 정의 모드로 바꿀지 물어보고 처리
											bool isResult = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_WarningChangeSortMode_Title),
																						Editor.GetText(TEXT.DLG_WarningChangeSortMode_Body),
																						Editor.GetText(TEXT.DLG_Change),
																						Editor.GetText(TEXT.Cancel));
											if(isResult)
											{
												Editor.SetHierarchySortMode(apEditor.HIERARCHY_SORT_MODE.Custom);
											}
										}
									}
									break;


								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									{
										_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										apAnimClip duplicatedAnimClip = _editor.Controller.DuplicateAnimClip(animClip);
										if (duplicatedAnimClip != null)
										{
											//복제가 되었다면 바로 선택 [v1.4.2]
											Editor.Select.SelectAnimClip(duplicatedAnimClip);
											if (Editor.Select.AnimClip == duplicatedAnimClip)
											{
												_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);

												//애니메이션의 경우 UI가 작아지므로, 스크롤을 자동으로 해야한다. (딜레이로 처리)
												isDelayedAutoScroll_AnimClip = true;
												delayedScrollTargetAnimClip = duplicatedAnimClip;
											}
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										bool isResult = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.RemoveAnimClip_Title),
																						_editor.GetTextFormat(TEXT.RemoveAnimClip_Body, animClip._name),
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel));
										if (isResult)
										{
											_editor.Controller.RemoveAnimClip(animClip);
										}
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.RemoveMultiple:
									{
										_loadKey_MultipleObjectRemoved = 
											apDialog_SelectMainObjects.ShowDialog(	_editor, 
																					apDialog_SelectMainObjects.REQUEST_TYPE.RemoveMainObjects, 
																					apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.AnimClip,
																					OnMultipleObjectRemoved);
									}
									break;
							}
						}
					}
					break;

				case CATEGORY.Param_Item:
					{
						apControlParam cParam = requestedObj as apControlParam;
						if(cParam != null)
						{
							switch (menuType)
							{
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Rename:
									_loadKey_Rename = apDialog_Rename.ShowDialog(_editor, cParam, clickedUnit, cParam._keyName, OnObjectRenamed);
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp:
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveDown:
									{
										if (Editor.HierarchySortMode == apEditor.HIERARCHY_SORT_MODE.Custom)
										{
											//정렬 모드가 "사용자 정의 모드"라면 > 위치 이동
											bool isMoveUp = menuType == apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.MoveUp;
											orders.ChangeOrder(Editor._portrait, apObjectOrders.OBJECT_TYPE.ControlParam, cParam._uniqueID, isMoveUp);

											//위치 변경 후 해당 객체를 선택한다.
											if (Editor.Select.Param != cParam)
											{
												Editor.Select.SelectControlParam(cParam);
												if (Editor.Select.Param == cParam)
												{
													_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
												}
											}
										}
										else
										{
											//정렬 모드를 사용자 정의 모드로 바꿀지 물어보고 처리
											bool isResult = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_WarningChangeSortMode_Title),
																						Editor.GetText(TEXT.DLG_WarningChangeSortMode_Body),
																						Editor.GetText(TEXT.DLG_Change),
																						Editor.GetText(TEXT.Cancel));
											if(isResult)
											{
												Editor.SetHierarchySortMode(apEditor.HIERARCHY_SORT_MODE.Custom);
											}
										}
									}
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Search:
									_loadKey_Search = apDialog_SearchObjects.ShowDialog_Portrait(_editor, OnObjectSearched);
									break;

								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Duplicate:
									{
										apControlParam dupParam = _editor.Controller.DuplicateParam(cParam);
										if(dupParam != null)
										{
											Editor.Select.SelectControlParam(dupParam);
											if (Editor.Select.Param == dupParam)
											{
												_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
											}
										}
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.Remove:
									{
										string strRemoveParamText = Editor.Controller.GetRemoveItemMessage(
														_editor._portrait,
														cParam,
														5,
														_editor.GetTextFormat(TEXT.RemoveControlParam_Body, cParam._keyName),
														_editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
														);

										bool isResult = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.RemoveControlParam_Title),
																						strRemoveParamText,
																						_editor.GetText(TEXT.Remove),
																						_editor.GetText(TEXT.Cancel));
										if (isResult)
										{
											_editor.Controller.RemoveParam(cParam);
										}
									}
									break;
								case apEditorHierarchyMenu.MENU_ITEM_HIERARCHY.RemoveMultiple:
									{
										_loadKey_MultipleObjectRemoved = 
											apDialog_SelectMainObjects.ShowDialog(	_editor, 
																					apDialog_SelectMainObjects.REQUEST_TYPE.RemoveMainObjects, 
																					apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.ControlParam,
																					OnMultipleObjectRemoved);
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

			//추가된 AnimClip이 있다면 Refresh 후에 자동으로 스크롤을 이동시키자
			if(isDelayedAutoScroll_AnimClip && delayedScrollTargetAnimClip != null)
			{
				Editor.AutoScroll_HierarchyMain(delayedScrollTargetAnimClip, true);//true : 생성 직후 렌더링 전엔 PosY가 계산되지 않으므로, 강제로 Calculate를 실행해야한다.
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
				|| _editor._portrait == null)
			{
				_loadKey_Rename = null;
				return;
			}

			_loadKey_Rename = null;

			//대상에 따라 다르다.
			if(targetObject is apTextureData)
			{
				apTextureData textureData = targetObject as apTextureData;

				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Image_SettingChanged, 
													_editor, 
													_editor._portrait, 
													//textureData, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				textureData._name = name;

				//[v1.4.2]
				//Rename후 객체를 선택하자 (단, 선택되지 않은 경우만)
				if (Editor.Select.TextureData != textureData)
				{
					Editor.Select.SelectImage(textureData);//<< 선택하자
					if (Editor.Select.TextureData == textureData)
					{
						_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
					}
				}
				

			}
			else if(targetObject is apMesh)
			{
				apMesh mesh = targetObject as apMesh;

				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SettingChanged, 
												_editor, 
												mesh, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				mesh._name = name;

				//Rename후 객체를 선택하자[v1.4.2]
				if (Editor.Select.Mesh != mesh)
				{
					Editor.Select.SelectMesh(mesh);
					if (Editor.Select.Mesh == mesh)
					{
						_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
					}
				}
				
			}
			else if(targetObject is apMeshGroup)
			{
				apMeshGroup meshGroup = targetObject as apMeshGroup;

				//메시 그룹의 이름은 아래 함수를 이용한다.
				_editor.Controller.RenameMeshGroup(meshGroup, name);

				//Rename후 객체를 선택하자[v1.4.2]
				if (Editor.Select.MeshGroup != meshGroup)
				{
					Editor.Select.SelectMeshGroup(meshGroup);
					if (Editor.Select.MeshGroup == meshGroup)
					{
						_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
					}
				}
				
			}
			else if(targetObject is apAnimClip)
			{
				apAnimClip animClip = targetObject as apAnimClip;

				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_SettingChanged, 
													_editor, 
													_editor._portrait, 
													//animClip, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);					
				animClip._name = name;

				//Rename후 객체를 선택하자[v1.4.2]
				if (Editor.Select.AnimClip != animClip)
				{
					Editor.Select.SelectAnimClip(animClip);
					if (Editor.Select.AnimClip == animClip)
					{
						_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);

						//애니메이션의 경우 UI가 작아지므로, 스크롤을 자동으로 해야한다.
						Editor.AutoScroll_HierarchyMain(animClip);
					}
				}
			}
			else if(targetObject is apControlParam)
			{
				apControlParam controlParam = targetObject as apControlParam;

				if (Editor.ParamControl.FindParam(name) != null)
				{
					//이미 사용중인 이름이다.
					EditorUtility.DisplayDialog(_editor.GetText(TEXT.ControlParamNameError_Title),
												_editor.GetText(TEXT.ControlParamNameError_Body_Used),
												_editor.GetText(TEXT.Close));
				}
				else
				{
					_editor.Controller.ChangeParamName(controlParam, name);

					//Rename후 객체를 선택하자[v1.4.2]
					if (Editor.Select.Param != controlParam)
					{
						Editor.Select.SelectControlParam(controlParam);

						if (Editor.Select.Param == controlParam)
						{
							_editor.OnHierachyClicked(apEditor.LAST_CLICKED_HIERARCHY.Main);
						}
					}
					
				}
			}

			Editor.RefreshControllerAndHierarchy(false);
		}


		//오브젝트 찾기 다이얼로그 결과
		private void OnObjectSearched(object loadKey, object targetObject)
		{
			
			//창이 열린 이후에 연속으로 이 이벤트가 호출될 수 있다.
			if(_loadKey_Search != loadKey
				|| loadKey == null
				|| _editor == null
				|| _editor._portrait == null)
			{
				_loadKey_Search = null;
				return;
			}
			if(targetObject == null)
			{
				return;
			}

			
			if(targetObject is apRootUnit)
			{
				//루트유닛
				apRootUnit rootUnit = targetObject as apRootUnit;
				if(rootUnit != null
					&& _editor._portrait != null
					&& _editor._portrait._rootUnits.Contains(rootUnit))
				{
					Editor.Select.SelectRootUnit(rootUnit);
				}
			}
			else if(targetObject is apTextureData)
			{
				//텍스쳐
				apTextureData textureData = targetObject as apTextureData;
				if(textureData != null 
					&& _editor._portrait._textureData != null
					&& _editor._portrait._textureData.Contains(textureData))
				{
					Editor.Select.SelectImage(textureData);//<< 선택하자
				}
			}
			else if(targetObject is apMesh)
			{
				//메시
				apMesh mesh = targetObject as apMesh;
				if(mesh != null 
					&& _editor._portrait._meshes != null
					&& _editor._portrait._meshes.Contains(mesh))
				{
					Editor.Select.SelectMesh(mesh);//<< 선택하자
				}
			}
			else if(targetObject is apMeshGroup)
			{
				//메시 그룹
				apMeshGroup meshGroup = targetObject as apMeshGroup;
				if(meshGroup != null 
					&& _editor._portrait._meshGroups != null
					&& _editor._portrait._meshGroups.Contains(meshGroup))
				{
					Editor.Select.SelectMeshGroup(meshGroup);
				}
			}
			else if(targetObject is apAnimClip)
			{
				//애니메이션 클립
				apAnimClip animClip = targetObject as apAnimClip;
				if(animClip != null 
					&& _editor._portrait._animClips != null
					&& _editor._portrait._animClips.Contains(animClip))
				{
					Editor.Select.SelectAnimClip(animClip);
				}
			}
			else if(targetObject is apControlParam)
			{
				//컨트롤 파라미터
				apControlParam cParam = targetObject as apControlParam;
				if(cParam != null 
					&& _editor._portrait._controller != null
					&& _editor._portrait._controller._controlParams != null
					&& _editor._portrait._controller._controlParams.Contains(cParam))
				{
					Editor.Select.SelectControlParam(cParam);
				}
			}

			Editor.RefreshControllerAndHierarchy(false);
		}


		private void OnMultipleObjectRemoved(bool isSuccess, object loadKey, apDialog_SelectMainObjects.TARGET_OBJECT_TYPE targetObjectType, List<object> selectedObjects)
		{
			if(!isSuccess
				|| _loadKey_MultipleObjectRemoved != loadKey
				|| selectedObjects == null
				)
			{
				_loadKey_MultipleObjectRemoved = null;
				return;
			}
			_loadKey_MultipleObjectRemoved = null;
			if(selectedObjects.Count == 0)
			{
				return;
			}

			int nSelectObjects = selectedObjects.Count;

			//선택된 오브젝트들을 삭제하시겠습니까? (삭제 이후 모든 선택이 해제됩니다.)
			bool result = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.Remove),
														_editor.GetText(TEXT.DLG_AskMultipleRemove_Body),
														_editor.GetText(TEXT.Remove),
														_editor.GetText(TEXT.Cancel));

			if(!result)
			{
				return;
			}
			
			//이제 객체 리스트들을 다시 정리해서 단체 삭제를 하자
			switch (targetObjectType)
			{

				case apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.Image:
					{
						List<apTextureData> removedTextureData = new List<apTextureData>();
						for (int i = 0; i < nSelectObjects; i++)
						{
							apTextureData curImage = selectedObjects[i] as apTextureData;
							if(curImage == null || removedTextureData.Contains(curImage))
							{
								continue;
							}
							removedTextureData.Add(curImage);
						}

						//이미지들을 삭제한다.
						if(removedTextureData.Count > 0)
						{
							_editor.Controller.RemoveTextures(removedTextureData);							
						}
					}
					break;

				case apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.Mesh:
					{
						List<apMesh> removedMeshes = new List<apMesh>();
						for (int i = 0; i < nSelectObjects; i++)
						{
							apMesh curMesh = selectedObjects[i] as apMesh;
							if(curMesh == null || removedMeshes.Contains(curMesh))
							{
								continue;
							}
							removedMeshes.Add(curMesh);
						}

						if (removedMeshes.Count > 0)
						{
							_editor.Controller.RemoveMeshes(removedMeshes);
						}
					}
					break;

				case apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.MeshGroup:
					{
						List<apMeshGroup> removedMeshGroups = new List<apMeshGroup>();
						for (int i = 0; i < nSelectObjects; i++)
						{
							apMeshGroup curMeshGroup = selectedObjects[i] as apMeshGroup;
							if(curMeshGroup == null || removedMeshGroups.Contains(curMeshGroup))
							{
								continue;
							}
							removedMeshGroups.Add(curMeshGroup);
						}

						if(removedMeshGroups.Count > 0)
						{
							_editor.Controller.RemoveMeshGroups(removedMeshGroups);
						}
					}
					break;

				case apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.AnimClip:
					{
						List<apAnimClip> removedAnimClips = new List<apAnimClip>();
						for (int i = 0; i < nSelectObjects; i++)
						{
							apAnimClip curAnimClip = selectedObjects[i] as apAnimClip;
							if(curAnimClip == null || removedAnimClips.Contains(curAnimClip))
							{
								continue;
							}
							removedAnimClips.Add(curAnimClip);
						}
						if(removedAnimClips.Count > 0)
						{
							_editor.Controller.RemoveAnimClips(removedAnimClips);
						}
					}
					break;

				case apDialog_SelectMainObjects.TARGET_OBJECT_TYPE.ControlParam:
					{
						List<apControlParam> removedControlParams = new List<apControlParam>();
						for (int i = 0; i < nSelectObjects; i++)
						{
							apControlParam curParam = selectedObjects[i] as apControlParam;
							if(curParam == null || removedControlParams.Contains(curParam))
							{
								continue;
							}
							removedControlParams.Add(curParam);
						}

						if(removedControlParams.Count > 0)
						{
							_editor.Controller.RemoveParams(removedControlParams);
						}
					}
					break;
			}

			//무조건 현재 메뉴 초기화
			_editor.Select.SelectNone();
			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(true);
		}

		// GUI
		//---------------------------------------------
		//Hierarchy 레이아웃 출력
		public void GUI_RenderHierarchy(int width, apEditor.HIERARCHY_FILTER hierarchyFilter, Vector2 scroll, int scrollLayoutHeight, bool isOrderChanged)
		{
			//레이아웃 이벤트일때 GUI요소 갱신
			bool isGUIEvent = (Event.current.type == EventType.Layout);

			_curUnitPosY = 0;
			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			bool isUnitRenderable = false;
			for (int i = 0; i < _units_Root.Count; i++)
			{
				CATEGORY category = (CATEGORY)_units_Root[i]._savedKey;
				isUnitRenderable = false;

				switch (category)
				{
					case CATEGORY.Overall_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.RootUnit) != 0;
						break;
					case CATEGORY.Images_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Image) != 0;
						break;
					case CATEGORY.Mesh_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Mesh) != 0;
						break;
					case CATEGORY.MeshGroup_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.MeshGroup) != 0;
						break;
					case CATEGORY.Animation_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Animation) != 0;
						break;
					case CATEGORY.Param_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Param) != 0;
						break;
				}
				if (isUnitRenderable)
				{
					GUI_RenderUnit(_units_Root[i], 0, width, scroll, scrollLayoutHeight, isGUIEvent, isOrderChanged);

					GUILayout.Space(10);
					_curUnitPosY += 10;
				}
			}
			GUILayout.Space(20);

		}

		//재귀적으로 Hierarchy 레이아웃을 출력
		//Child에 진입할때마다 Level을 높인다. (여백과 Fold의 기준이 됨)
		private void GUI_RenderUnit(apEditorHierarchyUnit unit, int level, int width, Vector2 scroll, int scrollLayoutHeight, bool isGUIEvent, bool isOrderChanged)
		{
			//unit.GUI_Render(_curUnitPosY, level * 10, width, scroll, scrollLayoutHeight, isGUIEvent, level, isOrderChanged);//이전
			unit.GUI_Render(_curUnitPosY, width, scroll, scrollLayoutHeight, isGUIEvent, level, isOrderChanged);//변경

			//_curUnitPosY += 20;//Height만큼 증가
			_curUnitPosY += apEditorHierarchyUnit.HEIGHT;

			if (unit.IsFoldOut)
			{
				if (unit._childUnits.Count > 0)
				{
					for (int i = 0; i < unit._childUnits.Count; i++)
					{
						//재귀적으로 호출
						GUI_RenderUnit(unit._childUnits[i], level + 1, width, scroll, scrollLayoutHeight, isGUIEvent, isOrderChanged);
						
					}
				}
			}
		}


		// 커서 이동 함수
		//--------------------------------------------------------------------
		/// <summary>
		/// 1.4.2 : 현재 선택된 커서를 찾고, 그 전후의 오브젝트들을 리턴한다.
		/// 출력 순서를 고려해야한다. (재귀적으로 동작)
		/// </summary>
		/// <param name="curObject"></param>
		/// <param name="prevObject"></param>
		/// <param name="nextObject"></param>
		public void FindCursor(out object curObject, out object prevObject, out object nextObject, apEditor.HIERARCHY_FILTER hierarchyFilter)
		{
			//렌더링 순서를 그대로 수행한다.
			//Prev, Cur, Next를 찾는다.
			//다만, Prev, Next가 같은 타입이어야 한다.
			curObject = null;
			prevObject = null;
			nextObject = null;

			apEditorHierarchyUnit selectedUnit_Cur = null;
			apEditorHierarchyUnit selectedUnit_Prev = null;
			apEditorHierarchyUnit selectedUnit_Next = null;

			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			bool isUnitRenderable = false;
			for (int i = 0; i < _units_Root.Count; i++)
			{
				CATEGORY category = (CATEGORY)_units_Root[i]._savedKey;
				isUnitRenderable = false;

				switch (category)
				{
					case CATEGORY.Overall_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.RootUnit) != 0;
						break;
					case CATEGORY.Images_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Image) != 0;
						break;
					case CATEGORY.Mesh_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Mesh) != 0;
						break;
					case CATEGORY.MeshGroup_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.MeshGroup) != 0;
						break;
					case CATEGORY.Animation_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Animation) != 0;
						break;
					case CATEGORY.Param_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Param) != 0;
						break;
				}
				if (isUnitRenderable)
				{
					bool isFindAll = FindCursor_Recursive(_units_Root[i],
															ref selectedUnit_Cur,
															ref selectedUnit_Prev,
															ref selectedUnit_Next);

					if(isFindAll)
					{
						break;
					}
				}
			}

			//Prev, Next는 Cur와 같은 타입이어야 한다.
			if(selectedUnit_Cur != null)
			{
				//Cur 유닛과 같은 Key (Category = 타입)를 가져야 한다.
				int curKey = selectedUnit_Cur._savedKey;
				if(selectedUnit_Prev != null)
				{
					if(selectedUnit_Prev._savedKey != curKey)
					{
						selectedUnit_Prev = null;
					}
				}
				if(selectedUnit_Next != null)
				{
					if(selectedUnit_Next._savedKey != curKey)
					{
						selectedUnit_Next = null;
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

		/// <summary>재귀적으로 현재,이전,다음 선택된 유닛을 찾는다. 모두 찾았다면 true 리턴</summary>
		private bool FindCursor_Recursive(	apEditorHierarchyUnit unit,
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
		public apEditorHierarchyUnit FindUnitByObject(object targetObj)
		{
			if(targetObj == null)
			{
				return null;
			}

			//1. 오브젝트 매핑에서 찾자
			apEditorHierarchyUnit result = null;
			if(_object2Unit.TryGetValue(targetObj, out result))
			{
				if(result != null)
				{
					//오브젝트 매핑에서 찾았다.
					return result;
				}
			}

			//2. 전체 유닛을 검색하자
			int nUnits = _units_All != null ? _units_All.Count : 0;
			if(nUnits == 0)
			{
				return null;
			}

			//해당 오브젝트를 가진 Unit을 리턴한다.
			result = _units_All.Find(delegate(apEditorHierarchyUnit a)
			{
				if(a._savedObj != null
				&& a._savedObj == targetObj)
				{
					return true;
				}
				return false;
			});

			return result;
		}

		/// <summary>
		/// 입력된 Unit의 PosY 위치를 계산한다. Hierarchy 렌더링과 같은 방식으로 동작한다.
		/// </summary>
		public int CalculateUnitPosY(apEditorHierarchyUnit targetUnit, apEditor.HIERARCHY_FILTER hierarchyFilter, out bool result)
		{
			int curUnitPosY = 0;
			result = false;

			//루트 노드는 For문으로 돌리고, 그 이후부터는 재귀 호출
			bool isUnitRenderable = false;
			for (int i = 0; i < _units_Root.Count; i++)
			{
				CATEGORY category = (CATEGORY)_units_Root[i]._savedKey;
				isUnitRenderable = false;

				switch (category)
				{
					case CATEGORY.Overall_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.RootUnit) != 0;
						break;
					case CATEGORY.Images_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Image) != 0;
						break;
					case CATEGORY.Mesh_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Mesh) != 0;
						break;
					case CATEGORY.MeshGroup_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.MeshGroup) != 0;
						break;
					case CATEGORY.Animation_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Animation) != 0;
						break;
					case CATEGORY.Param_Name:
						isUnitRenderable = (int)(hierarchyFilter & apEditor.HIERARCHY_FILTER.Param) != 0;
						break;
				}
				if (isUnitRenderable)
				{
					bool isFind = CalculateUnitPosY_Recursive(_units_Root[i], targetUnit, ref curUnitPosY);

					if(isFind)
					{
						//해당 유닛을 찾았다.
						result = true;
						return curUnitPosY;
					}

					curUnitPosY += 10;//여백
				}
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