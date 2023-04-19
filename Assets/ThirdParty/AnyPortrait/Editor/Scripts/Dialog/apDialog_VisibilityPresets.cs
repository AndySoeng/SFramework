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
	public class apDialog_VisibilityPresets : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		private static apDialog_VisibilityPresets s_window = null;

		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private apVisibilityPresets _visiblePreset = null;


		private Vector2 _scroll_RuleList = Vector2.zero;
		private Vector2 _scroll_Objects_Mesh = Vector2.zero;
		private Vector2 _scroll_Objects_Bone = Vector2.zero;

		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_Selected = null;
		
		private GUIStyle _guiStyle_HotkeyBox = null;
		private apStringWrapper _strWrapper_HotkeyBox = null;


		private string[] _hotkeyNames = null;
		

		private Texture2D _img_Rule = null;
		private Texture2D _img_Mesh = null;
		private Texture2D _img_MeshGroup = null;
		private Texture2D _img_Bone = null;
		private Texture2D _img_FoldDown = null;
		private Texture2D _img_LayerUp = null;
		private Texture2D _img_LayerDown = null;
		private Texture2D _img_OptionShow = null;
		private Texture2D _img_OptionHide = null;
		private Texture2D _img_DefaultShow = null;
		private Texture2D _img_DefaultHide = null;
		private Texture2D _img_Bypass = null;

		private enum OBJECT_TAB
		{
			Mesh,
			Bone,
		}
		private OBJECT_TAB _objectTab = OBJECT_TAB.Mesh;
		private apVisibilityPresets.RuleData _curRule = null;//현재 규칙
		
		private apMeshGroup _curMeshGroup = null;

		//메시 그룹들 (Portrait 기준)
		private List<apMeshGroup> _srcMeshGroups = null;
		private string[] _meshGroupNames = null;
		private int _iMeshGroup = 0;

		/// <summary>
		/// 지정된 메시 그룹과 현재 규칙의 오브젝트의 값. 메시 그룹에 따라서 만들어주자
		/// </summary>
		private class ObjectData
		{
			public apMeshGroup _rootMeshGroup = null;
			public ObjectData _parentData = null;

			public List<ObjectData> _childData = null;

			public apTransform_Mesh _linked_MeshTF = null;
			public apTransform_MeshGroup _linked_MeshGroupTF = null;
			public apBone _linked_Bone = null;

			public string _name = null;

			//연동된 값
			public apVisibilityPresets.ObjectVisibilityData _sync_Data = null;

			//결과
			public apVisibilityPresets.VISIBLE_OPTION _option = apVisibilityPresets.VISIBLE_OPTION.None;

			public ObjectData(apMeshGroup rootMeshGroup, ObjectData parentData, apTransform_Mesh meshTF)
			{
				_rootMeshGroup = rootMeshGroup;
				_parentData = parentData;

				if(_parentData != null)
				{
					//부모와 연결
					if(_parentData._childData == null)
					{
						_parentData._childData = new List<ObjectData>();
					}
					_parentData._childData.Add(this);
				}
				
				_linked_MeshTF = meshTF;
				_linked_MeshGroupTF = null;
				_linked_Bone = null;

				_name = meshTF._nickName;

				_sync_Data = null;
				_option = apVisibilityPresets.VISIBLE_OPTION.None;
			}

			public ObjectData(apMeshGroup rootMeshGroup, ObjectData parentData, apTransform_MeshGroup meshGroupTF)
			{
				_rootMeshGroup = rootMeshGroup;
				_parentData = parentData;

				if(_parentData != null)
				{
					//부모와 연결
					if(_parentData._childData == null)
					{
						_parentData._childData = new List<ObjectData>();
					}
					_parentData._childData.Add(this);
				}
				
				_linked_MeshTF = null;
				_linked_MeshGroupTF = meshGroupTF;
				_linked_Bone = null;

				_name = meshGroupTF._nickName;

				_sync_Data = null;
				_option = apVisibilityPresets.VISIBLE_OPTION.None;
			}

			public ObjectData(apMeshGroup rootMeshGroup, ObjectData parentData, apBone bone)
			{
				_rootMeshGroup = rootMeshGroup;
				_parentData = parentData;

				if(_parentData != null)
				{
					//부모와 연결
					if(_parentData._childData == null)
					{
						_parentData._childData = new List<ObjectData>();
					}
					_parentData._childData.Add(this);
				}
				
				_linked_MeshTF = null;
				_linked_MeshGroupTF = null;
				_linked_Bone = bone;

				_name = _linked_Bone._name;

				_sync_Data = null;
				_option = apVisibilityPresets.VISIBLE_OPTION.None;
			}

			public void ClearSync()
			{
				_sync_Data = null;
				_option = apVisibilityPresets.VISIBLE_OPTION.None;
			}
			
			public void Sync(apVisibilityPresets.ObjectVisibilityData syncData)
			{
				_sync_Data = syncData;
				_option = _sync_Data._visibleOption;
			}
		}

		private Dictionary<apMeshGroup, List<ObjectData>> _meshGroup2TFList_Root = null;
		private Dictionary<apMeshGroup, List<ObjectData>> _meshGroup2BoneList_Root = null;

		private Dictionary<apMeshGroup, List<ObjectData>> _meshGroup2TFList_All = null;
		private Dictionary<apMeshGroup, List<ObjectData>> _meshGroup2BoneList_All = null;

		private List<ObjectData> _objectData_All = null;

		private const string STR_NONE = "(None)";



		// Show Window
		//--------------------------------------------------------------
		public static void ShowDialog(apEditor editor, apVisibilityPresets visiblePreset, apMeshGroup curSelectedMeshGroup)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_VisibilityPresets), true, "Visibility Preset", true);
			apDialog_VisibilityPresets curTool = curWindow as apDialog_VisibilityPresets;

			
			if (curTool != null && curTool != s_window)
			{
				int width = 750;
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, visiblePreset, curSelectedMeshGroup);
			}
		}

		private static void CloseDialog()
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


		// Init
		//--------------------------------------------------------------
		public void Init(apEditor editor, apVisibilityPresets visiblePreset, apMeshGroup curSelectedMeshGroup)
		{
			_editor = editor;
			_portrait = _editor._portrait;
			_visiblePreset = visiblePreset;


			_img_Rule = _editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_PresetVisible);
			_img_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			_img_MeshGroup = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			_img_Bone = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Bone);
			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			_img_LayerUp = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp);
			_img_LayerDown = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown);
			_img_OptionShow = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Rule);
			_img_OptionHide = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Rule);

			_img_DefaultShow = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current);
			_img_DefaultHide = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current);
			_img_Bypass = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_BypassVisible);

			_objectTab = OBJECT_TAB.Mesh;
			_curRule = null;//현재 규칙
			_curMeshGroup = null;


			//단축키의 레이블 만들기
			_hotkeyNames = new string[6];//전체 단축키는 6개이다.
			_hotkeyNames[0] = "(None)";
			apStringWrapper strHotKeyLabel = new apStringWrapper(128);
			for (int iKey = 1; iKey <= 5; iKey++)
			{
				apHotKeyMapping.KEY_TYPE hotKeyType = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule1;
				switch (iKey)
				{
					case 1: hotKeyType = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule1; break;
					case 2: hotKeyType = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule2; break;
					case 3: hotKeyType = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule3; break;
					case 4: hotKeyType = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule4; break;
					case 5: hotKeyType = apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule5; break;
				}
				
				apHotKeyMapping.HotkeyMapUnit hotkeyUnit = _editor.HotKeyMap.GetHotkey(hotKeyType);

				strHotKeyLabel.Clear();
				strHotKeyLabel.Append(iKey, false);

				if(!hotkeyUnit._isAvailable_Cur || hotkeyUnit._keyCode_Cur == apHotKeyMapping.EST_KEYCODE.Unknown)
				{
					//단축키를 연결할 수 없다면.
					strHotKeyLabel.Append(" (Invalid)", true);
				}
				else
				{
					_editor.HotKeyMap.AddHotkeyTextToWrapper(hotKeyType, strHotKeyLabel, true);
				}

				_hotkeyNames[iKey] = "Key " + strHotKeyLabel.ToString();
				
			}
			


			//규칙이 이미 선택되어 있다면 그걸 선택한다.
			//그렇지 않다면 첫번째를 선택한다.

			//메시 그룹들 (Portrait 기준)
			_srcMeshGroups = new List<apMeshGroup>();
			_iMeshGroup = 0;

			
			//메시 그룹들의 오브젝트 리스트를 만들자
			_meshGroup2TFList_Root = new Dictionary<apMeshGroup, List<ObjectData>>();
			_meshGroup2BoneList_Root = new Dictionary<apMeshGroup, List<ObjectData>>();

			_meshGroup2TFList_All = new Dictionary<apMeshGroup, List<ObjectData>>();
			_meshGroup2BoneList_All = new Dictionary<apMeshGroup, List<ObjectData>>();

			_objectData_All = new List<ObjectData>();

			int nMeshGroups = _portrait._meshGroups != null ? _portrait._meshGroups.Count : 0;
			if (nMeshGroups > 0)
			{
				_meshGroupNames = new string[nMeshGroups];
				_iMeshGroup = -1;
				apMeshGroup curMeshGroup = null;
				for (int i = 0; i < nMeshGroups; i++)
				{
					curMeshGroup = _portrait._meshGroups[i];
					_srcMeshGroups.Add(curMeshGroup);
					_meshGroupNames[i] = curMeshGroup._name;

					if(curSelectedMeshGroup != null && curSelectedMeshGroup == curMeshGroup)
					{
						//이걸 선택하자
						_iMeshGroup = i;
						_curMeshGroup = curMeshGroup;
					}
				}

				//메시그룹을 선택하자.
				//현재 선택된게 없으면 첫번째를 선택
				if(_iMeshGroup < 0)
				{
					_curMeshGroup = _srcMeshGroups[0];
					_iMeshGroup = 0;
				}


				for (int i = 0; i < nMeshGroups; i++)
				{
					curMeshGroup = _portrait._meshGroups[i];
					List<ObjectData> meshList_Root = new List<ObjectData>();
					List<ObjectData> boneList_Root = new List<ObjectData>();

					List<ObjectData> meshList_All = new List<ObjectData>();
					List<ObjectData> boneList_All = new List<ObjectData>();

					_meshGroup2TFList_Root.Add(curMeshGroup, meshList_Root);
					_meshGroup2BoneList_Root.Add(curMeshGroup, boneList_Root);

					_meshGroup2TFList_All.Add(curMeshGroup, meshList_All);
					_meshGroup2BoneList_All.Add(curMeshGroup, boneList_All);

					//추가는 Recursive
					AddTransformData(meshList_Root, curMeshGroup, curMeshGroup, null);
					AddBoneRootData(boneList_Root, curMeshGroup, curMeshGroup, null);

					//Sort를 하자
					SortTransform(meshList_Root, meshList_All);
					SortBone(boneList_Root, boneList_All);

					for (int iMeshData = 0; iMeshData < meshList_All.Count; iMeshData++)
					{
						_objectData_All.Add(meshList_All[iMeshData]);
					}

					for (int iBoneData = 0; iBoneData < boneList_All.Count; iBoneData++)
					{
						_objectData_All.Add(boneList_All[iBoneData]);
					}
				}
			}
			else
			{
				//메시 그룹이 없다.
				_meshGroupNames = new string[1];
				_meshGroupNames[0] = STR_NONE;
				_iMeshGroup = 0;
				_curMeshGroup = null;
			}
		}


		private void AddTransformData(List<ObjectData> rootList, apMeshGroup rootMeshGroup, apMeshGroup meshGroup, ObjectData parentData)
		{
			List<apTransform_Mesh> childMeshes = meshGroup._childMeshTransforms;
			List<apTransform_MeshGroup> childMeshGroups = meshGroup._childMeshGroupTransforms;

			int nChildMesh = childMeshes != null ? childMeshes.Count : 0;
			int nChildMeshGroup = childMeshGroups != null ? childMeshGroups.Count : 0;

			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			for (int iMesh = 0; iMesh < nChildMesh; iMesh++)
			{
				curMeshTF = childMeshes[iMesh];
				ObjectData newObjData = new ObjectData(rootMeshGroup, parentData, curMeshTF);
				if(parentData == null)
				{
					//루트에 넣자
					rootList.Add(newObjData);
				}
			}


			for (int iMeshGroup = 0; iMeshGroup < nChildMeshGroup; iMeshGroup++)
			{
				curMeshGroupTF = childMeshGroups[iMeshGroup];
				ObjectData newObjData = new ObjectData(rootMeshGroup, parentData, curMeshGroupTF);
				if(parentData == null)
				{
					//루트에 넣자
					rootList.Add(newObjData);
				}

				if(curMeshGroupTF._meshGroup != meshGroup &&
					curMeshGroupTF._meshGroup != rootMeshGroup)
				{
					//재귀적으로 호출하여 자식도 등록한다.
					AddTransformData(rootList, rootMeshGroup, curMeshGroupTF._meshGroup, newObjData);
				}
			}

		}



		private void AddBoneRootData(List<ObjectData> rootList, apMeshGroup rootMeshGroup, apMeshGroup meshGroup, ObjectData parentData)
		{
			List<apBone> bones = meshGroup._boneList_Root;
			List<apTransform_MeshGroup> childMeshGroups = meshGroup._childMeshGroupTransforms;

			int nBones = bones != null ? bones.Count : 0;
			int nChildMeshGroup = childMeshGroups != null ? childMeshGroups.Count : 0;

			apBone curBone = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			for (int iBone = 0; iBone < nBones; iBone++)
			{
				curBone = bones[iBone];
				ObjectData newObjData = new ObjectData(rootMeshGroup, parentData, curBone);
				if(parentData == null)
				{
					//루트에 넣자
					rootList.Add(newObjData);
				}

				if(curBone._childBones != null && curBone._childBones.Count > 0)
				{
					//자식 본도 추가
					AddBoneChildData(rootMeshGroup, curBone, newObjData);
				}
			}


			for (int iMeshGroup = 0; iMeshGroup < nChildMeshGroup; iMeshGroup++)
			{
				curMeshGroupTF = childMeshGroups[iMeshGroup];
				ObjectData newObjData = new ObjectData(rootMeshGroup, parentData, curMeshGroupTF);
				if(parentData == null)
				{
					//루트에 넣자
					rootList.Add(newObjData);
				}

				if(curMeshGroupTF._meshGroup != meshGroup &&
					curMeshGroupTF._meshGroup != rootMeshGroup)
				{
					//재귀적으로 호출하여 자식도 등록한다.
					AddBoneRootData(rootList, rootMeshGroup, curMeshGroupTF._meshGroup, newObjData);
				}
			}
		}

		private void AddBoneChildData(apMeshGroup rootMeshGroup, apBone parentBone, ObjectData parentData)
		{
			List<apBone> bones = parentBone._childBones;

			int nBones = bones != null ? bones.Count : 0;

			apBone curBone = null;

			for (int iBone = 0; iBone < nBones; iBone++)
			{
				curBone = bones[iBone];
				ObjectData newObjData = new ObjectData(rootMeshGroup, parentData, curBone);//생성과 동시에 연결됨

				if (curBone._childBones != null && curBone._childBones.Count > 0)
				{
					//자식 본도 추가
					AddBoneChildData(rootMeshGroup, curBone, newObjData);
				}
			}
		}


		private void SortTransform(List<ObjectData> objectList, List<ObjectData> targetAllList)
		{
			objectList.Sort(delegate(ObjectData a, ObjectData b)
			{
				int depthA = -1;
				int depthB = -1;
				
				if(a._linked_MeshTF != null)
				{
					depthA = a._linked_MeshTF._depth;
				}
				else if(a._linked_MeshGroupTF != null)
				{
					depthA = a._linked_MeshGroupTF._depth;
				}

				if(b._linked_MeshTF != null)
				{
					depthB = b._linked_MeshTF._depth;
				}
				else if(b._linked_MeshGroupTF != null)
				{
					depthB = b._linked_MeshGroupTF._depth;
				}

				return depthB - depthA;
			});

			//정렬 이후에, 자식 리스트를 찾아서 정렬을 한다.
			ObjectData curData = null;
			for (int i = 0; i < objectList.Count; i++)
			{
				curData = objectList[i];
				targetAllList.Add(curData);

				if(curData._childData != null && curData._childData.Count > 0)
				{
					//자식 리스트도 적용
					SortTransform(curData._childData, targetAllList);
				}
			}
		}


		private void SortBone(List<ObjectData> objectList, List<ObjectData> targetAllList)
		{
			objectList.Sort(delegate(ObjectData a, ObjectData b)
			{
				int depthA = -1;
				int depthB = -1;

				//둘다 Bone이라면

				if (a._linked_Bone != null && b._linked_Bone != null)
				{
					depthA = a._linked_Bone._depth;
					depthB = b._linked_Bone._depth;

					if(depthA == depthB)
					{
						//그 외에는 그냥 문자열 순서로 매기자
						int compare = string.Compare(a._linked_Bone._name, b._linked_Bone._name);
						return compare;
					}

					return depthB - depthA;
				}
				else if(a._linked_MeshGroupTF != null && b._linked_MeshGroupTF != null)
				{
					depthA = a._linked_MeshGroupTF._depth;
					depthB = b._linked_MeshGroupTF._depth;

					return depthB - depthA;
				}
				else if(a._linked_Bone != null && b._linked_MeshGroupTF != null)
				{
					return -1;
				}
				else if(a._linked_MeshGroupTF != null && b._linked_Bone != null)
				{
					return 1;
				}

				return 0;
			});

			//정렬 이후에, 자식 리스트를 찾아서 정렬을 한다.
			ObjectData curData = null;
			for (int i = 0; i < objectList.Count; i++)
			{
				curData = objectList[i];
				targetAllList.Add(curData);

				if(curData._childData != null && curData._childData.Count > 0)
				{
					//자식 리스트도 적용
					SortBone(curData._childData, targetAllList);
				}
			}
		}





		// GUI
		//--------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null)
			{
				CloseDialog();
				return;
			}

			//GUI Style 만들기
			if (_guiStyle_None == null || _guiStyle_Selected == null)
			{
				_guiStyle_None = new GUIStyle(GUIStyle.none);
				_guiStyle_Selected = new GUIStyle(GUIStyle.none);

				if (EditorGUIUtility.isProSkin)
				{
					_guiStyle_Selected.normal.textColor = Color.cyan;
					_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
				}
				else
				{
					_guiStyle_Selected.normal.textColor = Color.white;
					_guiStyle_None.normal.textColor = Color.black;
				}
			}

			if(_guiStyle_HotkeyBox == null)
			{
				_guiStyle_HotkeyBox = new GUIStyle(GUI.skin.box);
				_guiStyle_HotkeyBox.alignment = TextAnchor.MiddleCenter;
			}
			if(_strWrapper_HotkeyBox == null)
			{
				_strWrapper_HotkeyBox = new apStringWrapper(256);
			}


			Color prevColor = GUI.backgroundColor;

			//레이아웃은 4개로 나뉘어진다.
			//Left : 규칙 리스트 / 현재 규칙 정보
			//Right : Custom시) 대상 메시 그룹(드롭박스) / 메시 그룹의 오브젝트들. 단 Custom 타입이 아니면 배경이 어두워짐
			int width_Left = 300;
			int width_Right = (width - 10) - (width_Left + 6);

			int height_Left_RuleSetting = 250;
			int height_Left_RuleList = height - (height_Left_RuleSetting + 15);
			int height_RuleListScroll = height_Left_RuleList - (20 + 25 + 6);

			int height_Right_MeshGroup = 70;
			int height_Right_ObjectList = height - (height_Right_MeshGroup + 10);

			int height_ListItem = 30;
			//int height_ObjectListScroll = height_Right_ObjectList - 24;
			
			Color guiColor_Default = new Color(0.95f, 0.95f, 0.95f);
			Color guiColor_ListNoEditible = new Color(0.2f, 0.2f, 0.2f);

			bool isObjectEditible = (_curRule != null && _curRule._ruleType == apVisibilityPresets.RULE_TYPE.Custom);

			GUI.backgroundColor = guiColor_Default;
			GUI.Box(new Rect(5, 5 + 21, width_Left + 1, height_RuleListScroll + 2), apStringFactory.I.None);//왼쪽 위 리스트
			GUI.backgroundColor = prevColor;

			GUI.backgroundColor = isObjectEditible ? guiColor_Default : guiColor_ListNoEditible;
			GUI.Box(new Rect(5 + width_Left + 7, height_Right_MeshGroup + 3, width_Right + 2, height_Right_ObjectList + 2), apStringFactory.I.None);//오른쪽 리스트
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

			GUILayout.Space(5);
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width_Left), apGUILOFactory.I.Height(height));
			//왼쪽 영역
			//------------------------------------------
			// 1. 규칙(프리셋) 리스트
			// 2. 현재 규칙

			GUILayout.Space(5);

			// 1. 규칙(프리셋) 리스트
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width_Left), apGUILOFactory.I.Height(height_Left_RuleList));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.VisibilityRules), apGUILOFactory.I.Height(20));//"Visibility Rules"

			
			_scroll_RuleList = EditorGUILayout.BeginScrollView(_scroll_RuleList, false, true, apGUILOFactory.I.Width(width_Left), apGUILOFactory.I.Height(height_RuleListScroll));

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width_Left - 20));
			
			//Rule들을 리스트로 보여주자

			//"Rules"
			GUILayout.Button(new GUIContent(_editor.GetText(TEXT.Rules), _img_FoldDown), _guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

			int nRules = _visiblePreset._rules != null ? _visiblePreset._rules.Count  : 0;
			apVisibilityPresets.RuleData curRule = null;
			for (int iRule = 0; iRule < nRules; iRule++)
			{
				curRule = _visiblePreset._rules[iRule];
				bool isSelected = curRule == _curRule;

				if(DrawRule(curRule, isSelected, iRule, width_Left - 20, height_ListItem, _scroll_RuleList.x))
				{
					_curRule = curRule;
					SyncToRule();//동기화
				}
			}

			GUILayout.Space(height);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			//버튼
			//"Add New Rule"
			if(GUILayout.Button(_editor.GetText(TEXT.AddNewRule), apGUILOFactory.I.Height(25)))
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo

				_curRule = _visiblePreset.AddNewRule();
				SyncToRule();//동기화
			}

			EditorGUILayout.EndVertical();
			


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width_Left - 5);
			GUILayout.Space(10);

			// 2. 현재 규칙
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width_Left), apGUILOFactory.I.Height(height_Left_RuleSetting));
			EditorGUILayout.LabelField(_editor.GetText(TEXT.RuleProperties), apGUILOFactory.I.Height(20));//"Rule Properties"
			
			GUILayout.Space(5);

			//- 이름
			//- 타입
			//- 단축키
			//- 복제 / 삭제

			string prevName = _curRule != null ? _curRule._name : STR_NONE;
			apVisibilityPresets.RULE_TYPE prevRuleType = _curRule != null ? _curRule._ruleType : apVisibilityPresets.RULE_TYPE.Custom;
			apVisibilityPresets.HOTKEY prevHotKey = _curRule != null ? _curRule._hotKey : apVisibilityPresets.HOTKEY.None;

			string nextName = EditorGUILayout.DelayedTextField(_editor.GetText(TEXT.DLG_Name), prevName);//"Name"
			GUILayout.Space(5);
			apVisibilityPresets.RULE_TYPE nextRuleType = (apVisibilityPresets.RULE_TYPE)EditorGUILayout.EnumPopup(_editor.GetText(TEXT.Method), prevRuleType);
			//apVisibilityPresets.HOTKEY nextHotKey = (apVisibilityPresets.HOTKEY)EditorGUILayout.EnumPopup(_editor.GetText(TEXT.ShortcutKey), prevHotKey);
			apVisibilityPresets.HOTKEY nextHotKey = (apVisibilityPresets.HOTKEY)EditorGUILayout.Popup(_editor.GetText(TEXT.ShortcutKey), (int)prevHotKey, _hotkeyNames);
			

			//TODO : RuleType 바꾸면, 해당 Rule의 Clear 해야함 + Undo
			if (_curRule != null)
			{
				if(!string.Equals(nextName, prevName))
				{
					//이름 바꾸기
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo
					_curRule._name = nextName;
				}
				if(nextRuleType != prevRuleType)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo
					_curRule._ruleType = nextRuleType;

					if(prevRuleType == apVisibilityPresets.RULE_TYPE.Custom && nextRuleType != apVisibilityPresets.RULE_TYPE.Custom)
					{
						//Rule Type에 따라서 데이터 날려야 함
						_curRule.ClearMeshGroupData();
					}
				}
				if(nextHotKey != prevHotKey)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo
					_curRule._hotKey = nextHotKey;

					//None타입이 아닐 때 동일한 단축키를 가진 다른 규칙이 있다면, 그 규칙의 단축키를 해제
					if(nextHotKey != apVisibilityPresets.HOTKEY.None)
					{
						apVisibilityPresets.RuleData otherRule = null;
						for (int iRule = 0; iRule < nRules; iRule++)
						{
							otherRule = _visiblePreset._rules[iRule];
							if(otherRule == _curRule)
							{
								continue;
							}

							if(otherRule._hotKey == nextHotKey)
							{
								otherRule._hotKey = apVisibilityPresets.HOTKEY.None;//겹친건 안된다.
							}
						}
					}
				}

				//만약 단축키가 None이 아니라면, 현재 설정된 단축키를 찾자
				if(nextHotKey != apVisibilityPresets.HOTKEY.None)
				{
					apHotKeyMapping.HotkeyMapUnit hotkeyUnit = null;
					switch (nextHotKey)
					{
						case apVisibilityPresets.HOTKEY.Hotkey1: hotkeyUnit = _editor.HotKeyMap.GetHotkey(apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule1); break;
						case apVisibilityPresets.HOTKEY.Hotkey2: hotkeyUnit = _editor.HotKeyMap.GetHotkey(apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule2); break;
						case apVisibilityPresets.HOTKEY.Hotkey3: hotkeyUnit = _editor.HotKeyMap.GetHotkey(apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule3); break;
						case apVisibilityPresets.HOTKEY.Hotkey4: hotkeyUnit = _editor.HotKeyMap.GetHotkey(apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule4); break;
						case apVisibilityPresets.HOTKEY.Hotkey5: hotkeyUnit = _editor.HotKeyMap.GetHotkey(apHotKeyMapping.KEY_TYPE.PresetVisibilityCustomRule5); break;
					}

					Color hotKeyColor = Color.black;
										
					if (hotkeyUnit != null)
					{
						_strWrapper_HotkeyBox.Clear();
						if (!hotkeyUnit._isAvailable_Cur || hotkeyUnit._keyCode_Cur == apHotKeyMapping.EST_KEYCODE.Unknown)
						{
							//유효하지 않은 단축키이다.
							//"Invalid shortcut key.\nSet a shortcut key in the Setting Dialog."
							_strWrapper_HotkeyBox.Append(_editor.GetText(TEXT.RuleHotkeyMsg_Invalid), true);
							hotKeyColor = new Color(GUI.backgroundColor.r * 1.0f, GUI.backgroundColor.g * 0.5f, GUI.backgroundColor.b * 0.5f, 1.0f);
						}
						else
						{
							//유효한 단축키이다.
							_strWrapper_HotkeyBox.Append(_editor.GetText(TEXT.RuleHotkeyMsg_Success1), false);//"Short key is ["
							_editor.HotKeyMap.AddHotkeyTextToWrapper(hotkeyUnit._hotKeyType, _strWrapper_HotkeyBox, false);
							_strWrapper_HotkeyBox.Append(_editor.GetText(TEXT.RuleHotkeyMsg_Success2), true);//"]\nIt can be changed in the Setting Dialog."
							hotKeyColor = new Color(GUI.backgroundColor.r * 0.8f, GUI.backgroundColor.g * 1.0f, GUI.backgroundColor.b * 0.8f, 1.0f);
						}

						GUI.backgroundColor = hotKeyColor;
						GUILayout.Box(_strWrapper_HotkeyBox.ToString(), _guiStyle_HotkeyBox, apGUILOFactory.I.Width(width_Left - 6), apGUILOFactory.I.Height(45));
						GUI.backgroundColor = prevColor;
					}
					
				}
			}

			



			GUILayout.Space(5);

			int width_UpDown = (width_Left - 10) / 2;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Height(25));
			GUILayout.Space(4);

			if(GUILayout.Button(_img_LayerUp, apGUILOFactory.I.Width(width_UpDown), apGUILOFactory.I.Height(20)))
			{
				//순서 위로 (-1)
				if(_curRule != null)
				{
					int iRule = _visiblePreset._rules.IndexOf(_curRule);
					if(iRule >= 1 && iRule < _visiblePreset._rules.Count)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo

						//인덱스 교환해서 정렬
						int iPrevRule = iRule;
						int iNextRule = iRule - 1;

						apVisibilityPresets.RuleData targetRule = _visiblePreset._rules[iNextRule];
						targetRule._index = iPrevRule;
						_curRule._index = iNextRule;
						_visiblePreset.Sort();
					}
				}
			}
			if(GUILayout.Button(_img_LayerDown, apGUILOFactory.I.Width(width_UpDown), apGUILOFactory.I.Height(20)))
			{
				//순서 아래로 (+1)
				if(_curRule != null)
				{
					int iRule = _visiblePreset._rules.IndexOf(_curRule);
					if(iRule >= 0 && iRule < _visiblePreset._rules.Count - 1)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo

						//인덱스 교환해서 정렬
						int iPrevRule = iRule;
						int iNextRule = iRule + 1;

						apVisibilityPresets.RuleData targetRule = _visiblePreset._rules[iNextRule];
						targetRule._index = iPrevRule;
						_curRule._index = iNextRule;
						_visiblePreset.Sort();
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			if(GUILayout.Button(_editor.GetUIWord(UIWORD.Duplicate), apGUILOFactory.I.Height(20)))//"Duplicate"
			{
				if (_curRule != null)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo
					apVisibilityPresets.RuleData prevRule = _curRule;
					_curRule = _visiblePreset.AddNewRule();

					_curRule._name = prevRule._name + " (Copied)";
					_curRule.CopyFromSrc(prevRule);
					
					SyncToRule();//동기화
				}
			}
			if(GUILayout.Button(_editor.GetText(TEXT.Remove), apGUILOFactory.I.Height(20)))
			{
				if(_curRule != null)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo
					_visiblePreset.RemoveRule(_curRule);

					//만약, 에디터의 현재 Rule이 이 Rule이면 생략
					if(_editor._selectedVisibilityPresetRule == _curRule)
					{
						_editor._selectedVisibilityPresetRule = null;
					}

					_curRule = null;
				}
			}

			
			

			EditorGUILayout.EndVertical();
			//------------------------------------------
			EditorGUILayout.EndVertical();

			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width_Right), apGUILOFactory.I.Height(height));
			//오른쪽 영역
			//------------------------------------------
			// 1. 메시 그룹 선택하기
			// 2. 메시 그룹의 오브젝트 선택하기


			GUILayout.Space(5);

			// 1. 메시 그룹 선택하기
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.MeshGroup));
			int iNextMeshGroup = EditorGUILayout.Popup(_iMeshGroup, _meshGroupNames);
			if(iNextMeshGroup != _iMeshGroup)
			{
				_iMeshGroup = iNextMeshGroup;
				_curMeshGroup = _srcMeshGroups[_iMeshGroup];
			}


			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width_Right), apGUILOFactory.I.Height(25));
			//탭 버튼
			int width_ObjectTab = (width_Right - 10) / 2;
			if(apEditorUtil.ToggledButton(_editor.GetUIWord(UIWORD.Meshes), _objectTab == OBJECT_TAB.Mesh, width_ObjectTab))
			{
				_objectTab = OBJECT_TAB.Mesh;
			}
			if(apEditorUtil.ToggledButton(_editor.GetUIWord(UIWORD.Bones), _objectTab == OBJECT_TAB.Bone, width_ObjectTab))
			{
				_objectTab = OBJECT_TAB.Bone;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(2);

			// 2. 메시 그룹의 오브젝트 선택하기
			if(_objectTab == OBJECT_TAB.Mesh)
			{
				_scroll_Objects_Mesh = EditorGUILayout.BeginScrollView(_scroll_Objects_Mesh, false, true, apGUILOFactory.I.Width(width_Right), apGUILOFactory.I.Height(height_Right_ObjectList));
			}
			else
			{
				_scroll_Objects_Bone = EditorGUILayout.BeginScrollView(_scroll_Objects_Bone, false, true, apGUILOFactory.I.Width(width_Right), apGUILOFactory.I.Height(height_Right_ObjectList));
			}
			

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width_Right - 20));
			
			GUILayout.Space(5);
			
			//Rule들을 리스트로 보여주자
			if(_objectTab == OBJECT_TAB.Mesh)
			{
				//Mesh인 경우
				GUILayout.Button(new GUIContent(_editor.GetUIWord(UIWORD.Meshes), _img_FoldDown), _guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼
				List<ObjectData> meshTFList = null;
				int nMeshTFList = 0;
				if(_meshGroup2TFList_All != null && _meshGroup2TFList_All.ContainsKey(_curMeshGroup))
				{
					meshTFList = _meshGroup2TFList_All[_curMeshGroup];
					nMeshTFList = meshTFList.Count;
				}
				for (int iData = 0; iData < nMeshTFList; iData++)
				{
					DrawData(meshTFList[iData], width_Right - 20, height_ListItem, _scroll_Objects_Mesh.x, isObjectEditible, false);
				}
			}
			else
			{
				//Bone인 경우
				GUILayout.Button(new GUIContent(_editor.GetUIWord(UIWORD.Bones), _img_FoldDown), _guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

				List<ObjectData> boneList = null;
				int nBoneList = 0;
				if(_meshGroup2BoneList_All != null && _meshGroup2BoneList_All.ContainsKey(_curMeshGroup))
				{
					boneList = _meshGroup2BoneList_All[_curMeshGroup];
					nBoneList = boneList.Count;
				}

				ObjectData curBoneData = null;
				for (int iData = 0; iData < nBoneList; iData++)
				{
					curBoneData = boneList[iData];
					DrawData(curBoneData, width_Right - 20, height_ListItem, _scroll_Objects_Bone.x, isObjectEditible, curBoneData._linked_Bone == null);
				}
			}
			
			
			GUILayout.Space(height);


			GUILayout.Space(height);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();


			//------------------------------------------
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
		}

		private bool DrawRule(apVisibilityPresets.RuleData rule, bool isSelected, int index, int width, int height, float scrollX)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();

			if (isSelected)
			{
				int yOffset = height;

				#region [미사용 코드]
				//Color prevColor = GUI.backgroundColor;

				//if(EditorGUIUtility.isProSkin)
				//{
				//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				//}
				//else
				//{
				//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
				//}

				//GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + yOffset , width + 10, height + 3), apStringFactory.I.None);
				//GUI.backgroundColor = prevColor; 
				#endregion

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + scrollX + 1, lastRect.y + yOffset , width + 10 - 2, height + 3, apEditorUtil.UNIT_BG_STYLE.Main);
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			bool isClick = false;


			if(GUILayout.Button(new GUIContent(" " + rule._name, _img_Rule), (isSelected ? _guiStyle_Selected : _guiStyle_None), GUILayout.Height(height)))
			{
				isClick = true;
			}

			EditorGUILayout.EndHorizontal();

			return isClick;
		}

		private void DrawData(ObjectData curData, int width, int height, float scrollX, bool isEditable, bool isDummyData)
		{
			//Rect lastRect = GUILayoutUtility.GetLastRect();

			//if (isSelected)
			//{
			//	int yOffset = height;
			//	Color prevColor = GUI.backgroundColor;

			//	if(EditorGUIUtility.isProSkin)
			//	{
			//		GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
			//	}
			//	else
			//	{
			//		GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
			//	}
				

			//	GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + yOffset , width + 10, height + 3), apStringFactory.I.None);
			//	GUI.backgroundColor = prevColor;
			//}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			int btnSize_Visible = height - 4;
			bool isVisibleChanged = false;
			apVisibilityPresets.VISIBLE_OPTION nextOption = apVisibilityPresets.VISIBLE_OPTION.None;

			if (isDummyData)
			{
				//더미인 경우 (Bone의 MeshGroupTF인 경우)
				//GUILayout.Space(height * 2);//기존
				//보기 버튼을 만들되, 이 SubMeshGroupTF의 본들의 가시성을 모두 변경한다. (토글은 아니다)
				if (apEditorUtil.ToggledButton_2Side(_img_Bypass, false, isEditable, btnSize_Visible, btnSize_Visible))
				{
					nextOption = apVisibilityPresets.VISIBLE_OPTION.None;
					isVisibleChanged = true;
				}

				GUILayout.Space(3);

				if (apEditorUtil.ToggledButton_2Side(_img_DefaultShow, false, isEditable, btnSize_Visible, btnSize_Visible))
				{
					nextOption = apVisibilityPresets.VISIBLE_OPTION.Show;
					isVisibleChanged = true;
				}

				if (apEditorUtil.ToggledButton_2Side(_img_DefaultHide, false, isEditable, btnSize_Visible, btnSize_Visible))
				{
					nextOption = apVisibilityPresets.VISIBLE_OPTION.Hide;
					isVisibleChanged = true;
				}

				if(isVisibleChanged && _curRule != null && curData._linked_MeshGroupTF != null)
				{
					//자식 본들을 모두 설정한다.

					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo


					if(_meshGroup2BoneList_All != null && _meshGroup2BoneList_All.ContainsKey(_curMeshGroup)
						&& curData._linked_MeshGroupTF._meshGroup != null)
					{
						List<ObjectData> boneDataList = _meshGroup2BoneList_All[_curMeshGroup];
					

						apMeshGroup targetMeshGroup = curData._linked_MeshGroupTF._meshGroup;
						
						if (targetMeshGroup._boneList_All != null && targetMeshGroup._boneList_All.Count > 0)
						{
							apBone curBone = null;
							ObjectData targetData = null;
							for (int iBone = 0; iBone < targetMeshGroup._boneList_All.Count; iBone++)
							{
								curBone = targetMeshGroup._boneList_All[iBone];
								targetData = boneDataList.Find(delegate(ObjectData a)
								{
									return a._linked_Bone == curBone;
								});

								if(targetData == null)
								{
									continue;
								}
								if(nextOption == apVisibilityPresets.VISIBLE_OPTION.None)
								{
									_curRule.ClearCustomData_Bone(curData._rootMeshGroup, curBone._uniqueID);
									//이 데이터 말고, 이 본에 대한 데이터의 동기화를 해제한다.
									targetData.ClearSync();
								}
								else
								{
									apVisibilityPresets.ObjectVisibilityData syncData = _curRule.SetCustomData_Bone(curData._rootMeshGroup, curBone._uniqueID, nextOption);
									//동기화
									targetData.Sync(syncData);
								}
							}
						}
					}
				}
			}
			else
			{
				//보기 버튼을 만든다.
				
				if (apEditorUtil.ToggledButton_2Side(_img_Bypass,
														curData._option == apVisibilityPresets.VISIBLE_OPTION.None,
														isEditable, btnSize_Visible, btnSize_Visible))
				{
					//아무것도 안나오게
					if(_curRule != null)
					{
						nextOption = apVisibilityPresets.VISIBLE_OPTION.None;
						isVisibleChanged = true;
					}
				}

				GUILayout.Space(3);

				if (apEditorUtil.ToggledButton_2Side(_img_OptionShow,
														curData._option == apVisibilityPresets.VISIBLE_OPTION.Show,
														isEditable, btnSize_Visible, btnSize_Visible))
				{
					//설정하기 (None <-> Show)
					if (_curRule != null)
					{	
						if(curData._option == apVisibilityPresets.VISIBLE_OPTION.Show)
						{
							nextOption = apVisibilityPresets.VISIBLE_OPTION.None;
						}
						else
						{
							nextOption = apVisibilityPresets.VISIBLE_OPTION.Show;
						}
						isVisibleChanged = true;
					}
				}

				if (apEditorUtil.ToggledButton_2Side(_img_OptionHide,
														curData._option == apVisibilityPresets.VISIBLE_OPTION.Hide,
														isEditable, btnSize_Visible, btnSize_Visible))
				{
					//설정하기 (None <-> Hide)
					if (_curRule != null)
					{	
						if(curData._option == apVisibilityPresets.VISIBLE_OPTION.Hide)
						{
							nextOption = apVisibilityPresets.VISIBLE_OPTION.None;
						}
						else
						{
							nextOption = apVisibilityPresets.VISIBLE_OPTION.Hide;
						}
						isVisibleChanged = true;
					}
				}


				if(isVisibleChanged)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.VisibilityChanged, _editor, _portrait, false, apEditorUtil.UNDO_STRUCT.ValueOnly);//Undo

					//-> None이면 데이터 삭제
					//-> Show/Hide면 데이터 갱신

					int meshTFID = -1;
					int boneID = -1;
					if(curData._linked_MeshTF != null)
					{
						meshTFID = curData._linked_MeshTF._transformUniqueID;
					}
					else if(curData._linked_MeshGroupTF != null)
					{
						meshTFID = curData._linked_MeshGroupTF._transformUniqueID;
					}
					else if(curData._linked_Bone != null)
					{
						boneID = curData._linked_Bone._uniqueID;
					}

					if(nextOption == apVisibilityPresets.VISIBLE_OPTION.None)
					{
						if(meshTFID >= 0)
						{
							_curRule.ClearCustomData_TF(curData._rootMeshGroup, meshTFID);
						}
						else if(boneID >= 0)
						{
							_curRule.ClearCustomData_Bone(curData._rootMeshGroup, boneID);
						}
						curData.ClearSync();
					}
					else
					{
						
						if(meshTFID >= 0)
						{
							apVisibilityPresets.ObjectVisibilityData syncData = _curRule.SetCustomData_TF(curData._rootMeshGroup, meshTFID, nextOption);

							//새로운 데이터가 있다면 동기화도 하자
							curData.Sync(syncData);
						}
						else if(boneID >= 0)
						{
							apVisibilityPresets.ObjectVisibilityData syncData = _curRule.SetCustomData_Bone(curData._rootMeshGroup, boneID, nextOption);

							//새로운 데이터가 있다면 동기화도 하자
							curData.Sync(syncData);
						}
							
					}
				}
			}
			


			Texture2D imgIcon = null;
			if(curData._linked_MeshTF != null)
			{
				imgIcon = _img_Mesh;
			}
			else if(curData._linked_MeshGroupTF != null)
			{
				imgIcon = _img_MeshGroup;
			}
			else
			{
				imgIcon = _img_Bone;
			}
			GUILayout.Space(10);

			if(GUILayout.Button(new GUIContent(" " + curData._name, imgIcon), _guiStyle_None, GUILayout.Height(height)))
			{
				
			}

			EditorGUILayout.EndHorizontal();
		}

		// Select Rule
		//------------------------------------------------------------------
		private void SyncToRule()
		{
			bool isAnySyncData = false;
			if (_curRule != null && _curRule._ruleType == apVisibilityPresets.RULE_TYPE.Custom)
			{
				isAnySyncData = true;
			}
			
			if (isAnySyncData)
			{
				//데이터를 연동한다.
				ObjectData curObjData = null;
				apVisibilityPresets.MeshGroupVisibilityData syncMeshGroupData = null;
				apVisibilityPresets.ObjectVisibilityData syncObjData = null;


				if (_meshGroup2TFList_All != null)
				{
					foreach (KeyValuePair<apMeshGroup, List<ObjectData>> meshGroup2DataList in _meshGroup2TFList_All)
					{
						apMeshGroup meshGroup = meshGroup2DataList.Key;
						List<ObjectData> meshTFList = meshGroup2DataList.Value;

						syncMeshGroupData = _curRule.GetMeshGroupData(meshGroup);
						if(syncMeshGroupData == null)
						{
							//연결된 MG 데이터가 없으면 모두 초기화
							for (int i = 0; i < meshTFList.Count; i++)
							{
								meshTFList[i].ClearSync();
							}
						}
						else
						{
							//있으면 연동
							for (int i = 0; i < meshTFList.Count; i++)
							{
								curObjData = meshTFList[i];
								syncObjData = null;
								if(curObjData._linked_MeshTF != null)
								{
									syncObjData = syncMeshGroupData.GetObjData_TF(curObjData._linked_MeshTF._transformUniqueID);
								}
								else if(curObjData._linked_MeshGroupTF != null)
								{
									syncObjData = syncMeshGroupData.GetObjData_TF(curObjData._linked_MeshGroupTF._transformUniqueID);
								}

								if(syncObjData == null)
								{
									curObjData.ClearSync();
								}
								else
								{
									//TF와 연동
									curObjData.Sync(syncObjData);
								}
							}
						}
						
					}
				}

				if (_meshGroup2BoneList_All != null)
				{
					foreach (KeyValuePair<apMeshGroup, List<ObjectData>> meshGroup2DataList in _meshGroup2BoneList_All)
					{
						apMeshGroup meshGroup = meshGroup2DataList.Key;
						List<ObjectData> boneList = meshGroup2DataList.Value;

						syncMeshGroupData = _curRule.GetMeshGroupData(meshGroup);
						if(syncMeshGroupData == null)
						{
							//연결된 MG 데이터가 없으면 모두 초기화
							for (int i = 0; i < boneList.Count; i++)
							{
								boneList[i].ClearSync();
							}
						}
						else
						{
							//있으면 연동
							for (int i = 0; i < boneList.Count; i++)
							{
								curObjData = boneList[i];
								syncObjData = null;
								if(curObjData._linked_Bone != null)
								{
									syncObjData = syncMeshGroupData.GetObjData_Bone(curObjData._linked_Bone._uniqueID);
								}

								if(syncObjData == null)
								{
									curObjData.ClearSync();
								}
								else
								{
									//TF와 연동
									curObjData.Sync(syncObjData);
								}
							}
						}
					}
				}
			}
			else
			{
				//데이터를 초기화한다.
				for (int i = 0; i < _objectData_All.Count; i++)
				{
					_objectData_All[i].ClearSync();
				}
			}
		}
	}
}