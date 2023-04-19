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
	/// <summary>
	/// 검색어를 입력해서 오브젝트들을 선택하는 툴. 다이얼로그가 켜진 상태에서도 선택이 가능하다.
	/// </summary>
	public class apDialog_SearchObjects : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_SearchObjects s_window = null;

		private enum SEARCH_MODE
		{
			Portrait,
			MeshGroup_Mesh,
			MeshGroup_Bone,
		}


		private apEditor _editor = null;
		private object _loadKey = null;
		//private SEARCH_MODE _searchMode = SEARCH_MODE.Portrait;
		private bool _isSelectAllBtnAvailable = false;
		private apPortrait _targetPortrait = null;
		private apMeshGroup _targetMeshGroup = null;

		private class NameObjectPair
		{
			public string _name = null;
			public object _object = null;
			public List<NameObjectPair> _childUnits = null;
			public bool _isSelectable = true;

			public NameObjectPair(string name, object obj, bool isSelectable = true)
			{
				_name = name;
				_object = obj;
				_childUnits = null;
				_isSelectable = isSelectable;
			}

			public List<NameObjectPair> MakeChildList()
			{
				if(_childUnits == null)
				{
					_childUnits = new List<NameObjectPair>();
				}
				return _childUnits;
			}
		}
		
		//전체 리스트
		private List<NameObjectPair> _objects = null;
		//검색된 리스트와 커서
		private bool _isSearched = false;
		private List<NameObjectPair> _searchedObjects = null;
		private int _nSearched = 0;
		private int _iFind = 0;

		private string _searchWords = "";

		public delegate void FUNC_SELECT_OBJECT(object loadKey, object targetObject);
		private FUNC_SELECT_OBJECT _funcResult_Single = null;

		public delegate void FUNC_SELECT_ALL_OBJECTS(object loadKey, List<object> targetObjects);
		private FUNC_SELECT_ALL_OBJECTS _funcResult_Multiple = null;

		
		private bool _isInitFocused = false;

		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog_Portrait(apEditor editor, FUNC_SELECT_OBJECT funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SearchObjects), true, "Search Objects", true);
			apDialog_SearchObjects curTool = curWindow as apDialog_SearchObjects;

			object loadKey = new object();
			if (curTool != null)
			{
				int width = 300;
				int height = 120;//Portrait모드에서는 Select All은 없다.
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, loadKey, SEARCH_MODE.Portrait, null, funcResult, null, false);

				return loadKey;
			}
			else
			{
				return null;
			}
		}

		public static object ShowDialog_SubObjects(apEditor editor, apMeshGroup meshGroup, bool isMeshTransforms, bool isSelectAllBtnAvailable, FUNC_SELECT_OBJECT funcResult, FUNC_SELECT_ALL_OBJECTS funcResult_Multiple)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || meshGroup == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SearchObjects), true, "Search Objects", true);
			apDialog_SearchObjects curTool = curWindow as apDialog_SearchObjects;

			object loadKey = new object();
			if (curTool != null)
			{
				int width = 300;
				int height = isSelectAllBtnAvailable ? 160 : 120;//MeshGroup의 객체를 선택하는 모드에서는 Select All 버튼이 있어서 조금 더 길어야 한다.
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, loadKey, isMeshTransforms ? SEARCH_MODE.MeshGroup_Mesh : SEARCH_MODE.MeshGroup_Bone, meshGroup, funcResult, funcResult_Multiple, isSelectAllBtnAvailable);

				return loadKey;
			}
			else
			{
				return null;
			}
		}



		public static void CloseDialog()
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
		//------------------------------------------------------------------
		private void Init(apEditor editor, object loadKey, SEARCH_MODE searchMode, apMeshGroup meshGroup, 
							FUNC_SELECT_OBJECT funcResult_Single, 
							FUNC_SELECT_ALL_OBJECTS funcResult_Multiple,
							bool isSelectAllBtnAvailable)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetPortrait = _editor._portrait;
			_targetMeshGroup = meshGroup;
			_funcResult_Single = funcResult_Single;
			_funcResult_Multiple = funcResult_Multiple;
			_isInitFocused = false;

			//전체 리스트
			_objects = new List<NameObjectPair>();
		
			//검색된 리스트와 커서
			_isSearched = false;
			_searchedObjects = new List<NameObjectPair>();
			_nSearched = 0;
			_iFind = 0;

			_searchWords = "";
			//_searchMode = searchMode;
			_isSelectAllBtnAvailable = isSelectAllBtnAvailable;

			switch (searchMode)
			{
				case SEARCH_MODE.Portrait:
					MakeObjectList_Portrait();
					break;

				case SEARCH_MODE.MeshGroup_Mesh:
					MakeObjectList_SubMeshes();
					break;

				case SEARCH_MODE.MeshGroup_Bone:
					MakeObjectList_SubBones();
					break;
			}
		}


		/// <summary>
		/// Portrait를 기준으로 
		/// </summary>
		private void MakeObjectList_Portrait()
		{
			_objects.Clear();

			if(_targetPortrait == null || _targetPortrait._objectOrders == null)
			{
				return;
			}

			//0. 루트 유닛
			int nRootUnit = _targetPortrait._objectOrders.RootUnits != null ? _targetPortrait._objectOrders.RootUnits.Count : 0;
			if (nRootUnit > 0)
			{
				apRootUnit curRootUnit = null;
				for (int i = 0; i < nRootUnit; i++)
				{
					curRootUnit = _targetPortrait._objectOrders.RootUnits[i]._linked_RootUnit;
					if(curRootUnit != null)
					{
						_objects.Add(new NameObjectPair("Root Unit " + i + " (" + curRootUnit.Name + ")", curRootUnit));
					}
				}
			}
			

			//1. 이미지
			int nImages = _targetPortrait._objectOrders.Images != null ? _targetPortrait._objectOrders.Images.Count : 0;
			if(nImages > 0)
			{
				apTextureData curTextureData = null;
				for (int i = 0; i < nImages; i++)
				{
					curTextureData = _targetPortrait._objectOrders.Images[i]._linked_Image;
					if(curTextureData != null)
					{
						_objects.Add(new NameObjectPair(curTextureData._name, curTextureData));
					}
				}
			}

			//2. 메시
			int nMeshes = _targetPortrait._objectOrders.Meshes != null ? _targetPortrait._objectOrders.Meshes.Count : 0;
			if(nMeshes > 0)
			{
				apMesh curMesh = null;
				for (int i = 0; i < nMeshes; i++)
				{
					curMesh = _targetPortrait._objectOrders.Meshes[i]._linked_Mesh;
					if(curMesh != null)
					{
						_objects.Add(new NameObjectPair(curMesh._name, curMesh));
					}
				}
			}

			//3. 메시 그룹
			int nMeshGroups = _targetPortrait._objectOrders.MeshGroups != null ? _targetPortrait._objectOrders.MeshGroups.Count : 0;
			if(nMeshGroups > 0)
			{
				apMeshGroup curMeshGroup = null;
				for (int i = 0; i < nMeshGroups; i++)
				{
					curMeshGroup = _targetPortrait._objectOrders.MeshGroups[i]._linked_MeshGroup;
					if(curMeshGroup != null)
					{
						_objects.Add(new NameObjectPair(curMeshGroup._name, curMeshGroup));
					}
				}
			}

			//4. 애니메이션
			int nAnimClips = _targetPortrait._objectOrders.AnimClips != null ? _targetPortrait._objectOrders.AnimClips.Count : 0;
			if(nAnimClips > 0)
			{
				apAnimClip curAnimClip = null;
				for (int i = 0; i < nAnimClips; i++)
				{
					curAnimClip = _targetPortrait._objectOrders.AnimClips[i]._linked_AnimClip;
					if(curAnimClip != null)
					{
						_objects.Add(new NameObjectPair(curAnimClip._name, curAnimClip));
					}
				}
			}

			//5. 컨트롤 파라미터
			int nControlParams = _targetPortrait._objectOrders.ControlParams != null ? _targetPortrait._objectOrders.ControlParams.Count : 0;
			if(nControlParams > 0)
			{
				apControlParam curControlParam = null;
				for (int i = 0; i < nControlParams; i++)
				{
					curControlParam = _targetPortrait._objectOrders.ControlParams[i]._linked_ControlParam;
					if(curControlParam != null)
					{
						_objects.Add(new NameObjectPair(curControlParam._keyName, curControlParam));
					}
				}
			}
		}




		private void MakeObjectList_SubMeshes()
		{
			_objects.Clear();

			

			if (_targetPortrait == null || _targetMeshGroup == null)
			{
				return;
			}

			List<NameObjectPair> rootList = new List<NameObjectPair>();

			//Transform 리그트를 재귀적으로 만들자.
			AddSubMeshesToList(null, rootList, _targetMeshGroup, _targetMeshGroup);

			//정렬하기
			SortRecv_SubMeshes(rootList);
		}

		private void AddSubMeshesToList(NameObjectPair parentPair, List<NameObjectPair> rootList, apMeshGroup targetMeshGroup, apMeshGroup rootMeshGroup)
		{
			int nMeshTFs = targetMeshGroup._childMeshTransforms != null ? targetMeshGroup._childMeshTransforms.Count : 0;
			int nMeshGroupTFs = targetMeshGroup._childMeshGroupTransforms != null ? targetMeshGroup._childMeshGroupTransforms.Count : 0;

			if(parentPair != null)
			{
				parentPair.MakeChildList();
			}

			apTransform_Mesh curMeshTF = null;			
			for (int i = 0; i < nMeshTFs; i++)
			{
				curMeshTF = targetMeshGroup._childMeshTransforms[i];
				if(parentPair == null)
				{
					rootList.Add(new NameObjectPair(curMeshTF._nickName, curMeshTF));
				}
				else
				{
					parentPair._childUnits.Add(new NameObjectPair(curMeshTF._nickName, curMeshTF));
				}
			}

			apTransform_MeshGroup curMeshGroupTF = null;
			for (int i = 0; i < nMeshGroupTFs; i++)
			{
				curMeshGroupTF = targetMeshGroup._childMeshGroupTransforms[i];

				NameObjectPair meshGroupPair = new NameObjectPair(curMeshGroupTF._nickName, curMeshGroupTF);
				if(parentPair == null)
				{
					rootList.Add(meshGroupPair);	
				}
				else
				{
					parentPair._childUnits.Add(meshGroupPair);
				}
				

				if(curMeshGroupTF._meshGroup != null
					&& curMeshGroupTF._meshGroup != targetMeshGroup
					&& curMeshGroupTF._meshGroup != rootMeshGroup)
				{
					//재귀적으로 호출
					AddSubMeshesToList(meshGroupPair, rootList, curMeshGroupTF._meshGroup, rootMeshGroup);
				}
			}
		}

		private void SortRecv_SubMeshes(List<NameObjectPair> curList)
		{
			if(curList == null || curList.Count == 0)
			{
				return;
			}

			//현재 리스트를 정렬한다.
			curList.Sort(delegate(NameObjectPair a, NameObjectPair b)
			{
				int depthA = -1;
				int depthB = -1;

				if(a._object is apTransform_Mesh)
				{
					depthA = (a._object as apTransform_Mesh)._depth;
				}
				else if(a._object is apTransform_MeshGroup)
				{
					depthA = (a._object as apTransform_MeshGroup)._depth;
				}

				if(b._object is apTransform_Mesh)
				{
					depthB = (b._object as apTransform_Mesh)._depth;
				}
				else if(b._object is apTransform_MeshGroup)
				{
					depthB = (b._object as apTransform_MeshGroup)._depth;
				}

				//역순
				return depthB - depthA;
			});

			//하나씩 넣으면서 자식이 있다면 자식 리스트를 정렬하고 메인 리스트에 넣자
			NameObjectPair curPair = null;
			for (int i = 0; i < curList.Count; i++)
			{
				curPair = curList[i];
				//일단 메인 리스트에 넣자
				_objects.Add(curPair);
				if(curPair._childUnits != null && curPair._childUnits.Count > 0)
				{
					//자식 리스트도 정렬+리스트에 병합하자
					SortRecv_SubMeshes(curPair._childUnits);
				}
			}
		}




		private void MakeObjectList_SubBones()
		{
			_objects.Clear();

			if (_targetPortrait == null || _targetMeshGroup == null)
			{
				return;
			}

			//본들의 부모인 메시 그룹이 포함된다. 단, 검색으로 선택되지는 않는다.
			NameObjectPair root_Main = new NameObjectPair(_targetMeshGroup._name, _targetMeshGroup, false);
			List<NameObjectPair> rootList_Sub = new List<NameObjectPair>();

			int nBoneListSets = _targetMeshGroup._boneListSets != null ? _targetMeshGroup._boneListSets.Count : 0;
			
			//메인 / 서브 본들을 트리 형태로 만들자.
			if(nBoneListSets > 0)
			{
				apMeshGroup.BoneListSet curBoneListSet = null;
				for (int i = 0; i < _targetMeshGroup._boneListSets.Count; i++)
				{
					curBoneListSet = _targetMeshGroup._boneListSets[i];
					if(curBoneListSet._bones_Root == null
						|| curBoneListSet._bones_Root.Count == 0)
					{
						continue;
					}

					NameObjectPair targetRootPair = null;

					if (curBoneListSet._isRootMeshGroup)
					{
						targetRootPair = root_Main;
					}
					else if(curBoneListSet._meshGroup != null
						&& curBoneListSet._meshGroupTransform != null)
					{
						NameObjectPair newRootPair = new NameObjectPair(curBoneListSet._meshGroupTransform._nickName, curBoneListSet._meshGroupTransform, false);
						rootList_Sub.Add(newRootPair);

						targetRootPair = newRootPair;
					}
					else
					{
						continue;
					}

					int nRootBones = curBoneListSet._bones_Root.Count;

					for (int iBone = 0; iBone < nRootBones; iBone++)
					{
						AddSubBonesToList(targetRootPair, curBoneListSet._bones_Root[iBone]);
					}
					
				}
			}

			//정렬을 하자
			//Sub부터 정렬
			if(rootList_Sub.Count > 1)
			{
				rootList_Sub.Sort(delegate(NameObjectPair a, NameObjectPair b)
				{
					if(a._object is apTransform_MeshGroup && b._object is apTransform_MeshGroup)
					{
						int depthA = (a._object as apTransform_MeshGroup)._depth;
						int depthB = (b._object as apTransform_MeshGroup)._depth;
						return depthB - depthA;
					}
					return 0;
				});

			}

			//Main부터 정렬+입력
			SortRecv_SubBones(root_Main._childUnits);

			//Sub 입력
			if(rootList_Sub.Count > 1)
			{
				for (int iSub = 0; iSub < rootList_Sub.Count; iSub++)
				{
					SortRecv_SubBones(rootList_Sub[iSub]._childUnits);
				}
			}

		}

		private void AddSubBonesToList(NameObjectPair parentPair, apBone targetBone)
		{
			if(targetBone == null)
			{
				return;
			}
			parentPair.MakeChildList();

			NameObjectPair newPair = new NameObjectPair(targetBone._name, targetBone);
			parentPair._childUnits.Add(newPair);

			//자식 본들에 대해서도 Pair를 등록한다.
			if(targetBone._childBones != null && targetBone._childBones.Count > 0)
			{
				int nChildBones = targetBone._childBones.Count;
				for (int i = 0; i < nChildBones; i++)
				{
					AddSubBonesToList(newPair, targetBone._childBones[i]);
				}
			}
		}


		private void SortRecv_SubBones(List<NameObjectPair> curList)
		{
			if(curList == null || curList.Count == 0)
			{
				return;
			}

			//현재 리스트를 정렬한다.
			curList.Sort(delegate(NameObjectPair a, NameObjectPair b)
			{
				int depthA = -1;
				int depthB = -1;

				if(a._object is apBone)
				{
					depthA = (a._object as apBone)._depth;
				}
				else if(a._object is apBone)
				{
					depthA = (a._object as apBone)._depth;
				}

				if(b._object is apBone)
				{
					depthB = (b._object as apBone)._depth;
				}
				else if(b._object is apBone)
				{
					depthB = (b._object as apBone)._depth;
				}

				if (depthB != depthA)
				{
					//역순
					return depthB - depthA;
				}
				
				//이름순
				return string.Compare(a._name, b._name);
			});

			//하나씩 넣으면서 자식이 있다면 자식 리스트를 정렬하고 메인 리스트에 넣자
			NameObjectPair curPair = null;
			for (int i = 0; i < curList.Count; i++)
			{
				curPair = curList[i];

				//일단 메인 리스트에 넣자
				_objects.Add(curPair);

				if(curPair._childUnits != null && curPair._childUnits.Count > 0)
				{
					//자식 리스트도 정렬+리스트에 병합하자
					SortRecv_SubBones(curPair._childUnits);
				}
			}
		}




		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null 
				|| _editor._portrait == null
				|| _editor._portrait != _targetPortrait
				|| _funcResult_Single == null)
			{
				CloseDialog();
				return;
			}



			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor)
			{
				CloseDialog();
				return;
			}

			width -= 10;
			
			//<Search Label>
			//텍스트 박스 (Delayed) > 텍스트 바뀌면 인식
			//FInd Next (i/n)
			//Close

			//EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Search), GUILayout.Width(width));
			
			//GUILayout.Space(10);

			//bool isSearchWordChanged = false;

			apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__SearchWord);
			string nextSearchWords = EditorGUILayout.DelayedTextField(_searchWords, GUILayout.Width(width));
			if(!string.Equals(nextSearchWords, _searchWords))
			{
				_searchWords = nextSearchWords;
				//isSearchWordChanged = true;
				if(string.IsNullOrEmpty(_searchWords))
				{
					//빈칸이다.
					_isSearched = false;
					_iFind = 0;
					_nSearched = 0;
					if(_searchedObjects == null)
					{
						_searchedObjects = new List<NameObjectPair>();
					}
					_searchedObjects.Clear();
				}
				else
				{
					//다시 검색하자
					Search();
				}
				
			}

			GUILayout.Space(5);
			if(_isSearched 
				&& _searchedObjects != null
				&& _searchedObjects.Count > 0
				&& _iFind >= 0 && _iFind < _searchedObjects.Count
				&& _searchedObjects[_iFind] != null)
			{
				EditorGUILayout.LabelField(_searchedObjects[_iFind]._name, GUILayout.Width(width));
			}
			else
			{
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_NotSelected), GUILayout.Width(width));
			}
			
			
			

			//검색 버튼
			GUILayout.Space(5);
			string strFindNext = _editor.GetUIWord(UIWORD.Next);
			if(_isSearched)
			{
				strFindNext += " [ " + (_iFind + 1) + " / " + _nSearched + " ]";
			}
			else
			{
				strFindNext += " [ 0 ]";
			}
			if(apEditorUtil.ToggledButton_2Side(strFindNext, false, _isSearched, width, 30))
			{	
				FindNext();
			}

			if(_isSelectAllBtnAvailable)
			{
				//Portrait 모드 외에는 전체 선택이 가능하다.
				if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_SelectAll), false, _isSearched, width, 30))
				{
					SelectAll();
				}
			}

			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.Close), false, true, width, 30))
			{
				CloseDialog();
			}


			//추가 21.6.13 : 다이얼로그를 열면 자동으로 이름 텍스트 필드에 포커스를 설정하자
			if(!_isInitFocused)
			{
				_isInitFocused = true;
				apEditorUtil.SetGUIFocus_TextField(apStringFactory.I.GUI_ID__SearchWord);
			}

			//if(Event.current.type != EventType.used
			//	&& Event.current.type == EventType.KeyUp
			//	&& !isSearchWordChanged)
			//{
			//	if(Event.current.keyCode == KeyCode.Return)
			//	{
			//		//키입력 처리
			//		if(string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
			//		{
			//			FindNext();
			//			Event.current.Use();
			//		}
			//	}
			//}
		}


		private void Search()
		{
			if(_searchedObjects == null)
			{
				_searchedObjects = new List<NameObjectPair>();
			}
			_searchedObjects.Clear();

			//전체 오브젝트에서 이름 글자를 포함하는 경우
			List<NameObjectPair> result = _objects.FindAll(delegate(NameObjectPair a)
			{
				return a._name.Contains(_searchWords) && a._isSelectable;
			});

			if(result != null && result.Count > 0)
			{
				for (int i = 0; i < result.Count; i++)
				{
					_searchedObjects.Add(result[i]);
				}
			}

			if(_searchedObjects.Count > 0)
			{
				_isSearched = true;
				_iFind = 0;
				_nSearched = _searchedObjects.Count;

				//처음 검색할 때 결과를 호출하자
				if(_funcResult_Single != null)
				{
					_funcResult_Single(_loadKey, _searchedObjects[_iFind]._object);
				}
			}
			else
			{
				_isSearched = false;
				_iFind = 0;
				_nSearched = 0;
			}
			
			
		}

		private void FindNext()
		{
			//다음으로 커서를 옮기자
			if(!_isSearched
				|| _nSearched == 0
				|| _searchedObjects == null
				|| _searchedObjects.Count == 0)
			{
				return;
			}

			_iFind += 1;
			if(_iFind >= _nSearched)
			{
				_iFind = 0;
			}

			//커서를 옮긴 후 호출한다.
			if(_funcResult_Single != null)
			{
				_funcResult_Single(_loadKey, _searchedObjects[_iFind]._object);
			}
		}

		private void SelectAll()
		{
			if(!_isSearched
				|| _nSearched == 0
				|| _searchedObjects == null
				|| _searchedObjects.Count == 0
				|| _funcResult_Multiple == null)
			{
				return;
			}

			//검색된 걸 모두 선택하자
			List<object> resultObjects = new List<object>();
			for (int i = 0; i < _searchedObjects.Count; i++)
			{
				resultObjects.Add(_searchedObjects[i]._object);
			}
			_funcResult_Multiple(_loadKey, resultObjects);
		}
	}
}