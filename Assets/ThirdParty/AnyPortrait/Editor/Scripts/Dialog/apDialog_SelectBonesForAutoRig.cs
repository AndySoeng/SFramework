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
	/// 추가 12.29 : AutoRig를 위해 사용되는 Dialog
	/// Hierarchy 방식으로 추천하면서 추가적으로 [권장]이 더 표시되는 특징이 있다.
	/// </summary>
	public class apDialog_SelectBonesForAutoRig : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_BONES_FOR_AUTO_RIG(bool isSuccess, object loadKey, List<apBone> selectedBones);

		private static apDialog_SelectBonesForAutoRig s_window = null;

		
		
		private class BoneInfo
		{
			public BoneInfo _parentUnit = null;
			public apBone _bone = null;
			public List<BoneInfo> _childUnits = null;
			public bool _isRecommended = false;
			public string _name = "";
			public int _level = 0;
			public float _distance = -1.0f;//요청된 위치로부터의 거리
			public bool _isSelected = false;

			public bool _isFoldable = false;
			public bool _isFolded = false;

			public BoneInfo(BoneInfo parentUnit, apBone bone, float dist, int level)
			{
				_parentUnit = parentUnit;
				_bone = bone;

				if (_bone != null)
				{
					_name = " " + _bone._name;
				}
				else
				{
					_name = " None";
				}
				
				_childUnits = new List<BoneInfo>();
				_level = level;

				_isRecommended = false;
				_distance = dist;

				_isSelected = false;

				_isFoldable = false;
				_isFolded = false;

				//Debug.Log("Bone Info : " + _name + " / " + _distance);

				if(_parentUnit != null)
				{
					_parentUnit._childUnits.Add(this);
					_parentUnit._isFoldable = true;
				}
			}

			
		}

		private apEditor _editor = null;
		private object _loadKey = null;
		private apMeshGroup _meshGroup = null;
		//private apTransform_Mesh _meshTransform = null;

		private List<BoneInfo> _boneInfo_All = new List<BoneInfo>();
		private Dictionary<apMeshGroup, List<BoneInfo>> _boneInfo_Root = new Dictionary<apMeshGroup, List<BoneInfo>>();

		private Texture2D _img_Bone = null;
		private Texture2D _img_FoldDown = null;
		//private Texture2D _img_FoldRight = null;
		private Texture2D _img_Recommended = null;

		private Vector2 _scrollList = Vector2.zero;

		private bool _isSearched = false;
		private string _strSearchKeyword = "";

		private FUNC_SELECT_BONES_FOR_AUTO_RIG _funcResult = null;

		private Vector2 _requestPos_Min = Vector2.zero;//위치 기반으로 "추천"기능이 동작한다. 버텍스가 선택되어 있다면 그 버텍스들을 기준으로 하고, 
		private Vector2 _requestPos_Max = Vector2.zero;//위치 기반으로 "추천"기능이 동작한다. 버텍스가 선택되어 있다면 그 버텍스들을 기준으로 하고, 
		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_None_Margin0 = null;
		private GUIStyle _guiStyle_Selected = null;
		private GUIStyle _guiStyle_Center = null;
		private GUIContent _guiContent_BoneIcon = null;
		private GUIContent _guiContent_Recommended = null;
		private GUIContent _guiContent_SelectAllNearBone = null;

		// Show Window
		//------------------------------------------------------------------------------------------
		public static object ShowDialog(	apEditor editor, 
											apMeshGroup targetMeshGroup, 
											apTransform_Mesh meshTransform, 
											List<apSelection.ModRenderVert> modRenderVerts,
											FUNC_SELECT_BONES_FOR_AUTO_RIG funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectBonesForAutoRig), true, "Select Bones for Auto-Rig", true);
			apDialog_SelectBonesForAutoRig curTool = curWindow as apDialog_SelectBonesForAutoRig;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				//기본 Dialog보다 조금 더 크다. Hierarchy 방식으로 가로 스크롤이 포함되기 때문
				int width = 400;
				int height = 650;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetMeshGroup, meshTransform, modRenderVerts, funcResult);

				return loadKey;
			}
			else
			{
				return null;
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
		//------------------------------------------------------------------------
		public void Init(apEditor editor, object loadKey, 
						apMeshGroup targetMeshGroup, 
						apTransform_Mesh meshTransform,
						
						List<apSelection.ModRenderVert> modRenderVerts,
						FUNC_SELECT_BONES_FOR_AUTO_RIG funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_meshGroup = targetMeshGroup;
			//_meshTransform = meshTransform;
			if(meshTransform._linkedRenderUnit != null)
			{
				meshTransform._linkedRenderUnit.CalculateWorldPositionWithoutModifier();//Mod가 적용되지 않은 World Pos를 구해야한다.
			}
			
			if(_boneInfo_All == null)
			{
				_boneInfo_All = new List<BoneInfo>();
			}
			_boneInfo_All.Clear();

			if(_boneInfo_Root == null)
			{
				_boneInfo_Root = new Dictionary<apMeshGroup, List<BoneInfo>>();
			}
			_boneInfo_Root.Clear();
			

			_img_Bone = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging);
			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			//_img_FoldRight = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight);
			_img_Recommended = _editor.ImageSet.Get(apImageSet.PRESET.RecommendedIcon);

			_scrollList = Vector2.zero;

			_isSearched = false;
			_strSearchKeyword = "";

			_funcResult = funcResult;

			//거리를 체크하기 위한 위치를 계산하자
			if(modRenderVerts == null || modRenderVerts.Count == 0)
			{
				//Debug.LogError("< No Verts>");
				//선택된 버텍스가 없다면 > Mesh Transform의 위치를 사용
				_requestPos_Min = meshTransform._matrix_TFResult_WorldWithoutMod._pos;
				_requestPos_Max = meshTransform._matrix_TFResult_WorldWithoutMod._pos;
			}
			else
			{
				//Debug.LogError("< Verts >");
				//선택된 버텍스의 LBRT를 계산하여 거리 계산에 참고하자.
				apSelection.ModRenderVert curModRenderVert = null;
				Vector2 curPos = Vector2.zero;

				for (int i = 0; i < modRenderVerts.Count; i++)
				{
					curModRenderVert = modRenderVerts[i];
					curPos = curModRenderVert._renderVert._pos_World_NoMod;
					
					//Debug.Log("Vert Pos : " + curPos);
					if(i == 0)
					{
						_requestPos_Min = curPos;
						_requestPos_Max = curPos;
					}
					else
					{
						//XY축으로 MinMax 영역을 계산하자
						_requestPos_Min.x = Mathf.Min(curPos.x, _requestPos_Min.x);
						_requestPos_Min.y = Mathf.Min(curPos.y, _requestPos_Min.y);
						_requestPos_Max.x = Mathf.Max(curPos.x, _requestPos_Max.x);
						_requestPos_Max.y = Mathf.Max(curPos.y, _requestPos_Max.y);
					}
				}
			}
			//Debug.LogError("< Request Pos>  Min : " + _requestPos_Min.ToString() + " / Max : " + _requestPos_Max.ToString());

			//본 리스트를 만들자
			MakeBoneListPerMeshGroupRecursive(_meshGroup, _meshGroup);

			//이제 거리가 가까운 순서대로 몇개를 선택해서 Recommended를 만들자
			//몇개, 어느 거리까지의 본을 선택할까
			//- Min 거리, Max 거리를 계산하여 Min의 10%이내의 모든 본 선택
			//- Parent, Child를 1레벨씩 선택하여 추가

			float minDist = 0.0f;
			float maxDist = 0.0f;

			BoneInfo curBoneInfo = null;
			for (int i = 0; i < _boneInfo_All.Count; i++)
			{
				curBoneInfo = _boneInfo_All[i];
				if(i == 0)
				{
					minDist = curBoneInfo._distance;
					maxDist = curBoneInfo._distance;
				}
				else
				{
					minDist = Mathf.Min(curBoneInfo._distance, minDist);
					maxDist = Mathf.Max(curBoneInfo._distance, maxDist);
				}
			}
			float nearestBiasDist = (maxDist - minDist) * 0.1f + minDist;
			if (nearestBiasDist < 1.0f)
			{
				//너무 짧다면
				nearestBiasDist = 1.0f;
			}

			//다시 체크
			for (int i = 0; i < _boneInfo_All.Count; i++)
			{
				curBoneInfo = _boneInfo_All[i];
				if(curBoneInfo._distance < nearestBiasDist)
				{
					//추천!
					curBoneInfo._isRecommended = true;

					//이 Bone의 부모와 자식 Info도 Recommended에 추가하자
					if(curBoneInfo._parentUnit != null)
					{
						curBoneInfo._parentUnit._isRecommended = true;
					}

					if(curBoneInfo._childUnits != null && curBoneInfo._childUnits.Count > 0)
					{
						for (int iChild = 0; iChild < curBoneInfo._childUnits.Count; iChild++)
						{
							curBoneInfo._childUnits[iChild]._isRecommended = true;
						}
					}
				}
			}
		}

		private void MakeBoneListPerMeshGroupRecursive(apMeshGroup rootMeshGroup, apMeshGroup meshGroup)
		{	
			if (meshGroup._boneList_Root != null && meshGroup._boneList_Root.Count > 0)
			{
				apBone rootBone = null;
				//거꾸로 값을 넣어야 한다.
				for (int i = meshGroup._boneList_Root.Count - 1; i >= 0; i--)
				{
					rootBone = _meshGroup._boneList_Root[i];
					if (rootBone == null)
					{
						continue;
					}
					MakeBoneListRecursive(rootMeshGroup, meshGroup, rootBone, null, 0);
				}
			}

			//자식 메시 그룹도 체크하자
			if (meshGroup._childMeshGroupTransforms != null && meshGroup._childMeshGroupTransforms.Count > 0)
			{
				apTransform_MeshGroup childMeshGroupTransform = null;
				apMeshGroup childMeshGroup = null;
				apBone childRootBone = null;

				for (int iChildMG = 0; iChildMG < meshGroup._childMeshGroupTransforms.Count; iChildMG++)
				{
					childMeshGroupTransform = meshGroup._childMeshGroupTransforms[iChildMG];
					if (childMeshGroupTransform == null)
					{
						continue;
					}

					childMeshGroup = childMeshGroupTransform._meshGroup;
					if (childMeshGroup == null || childMeshGroup == rootMeshGroup || childMeshGroup == meshGroup)
					{
						continue;
					}

					if (childMeshGroup._boneList_Root == null || childMeshGroup._boneList_Root.Count == 0)
					{
						continue;
					}

					for (int iRootBone = 0; iRootBone < childMeshGroup._boneList_Root.Count; iRootBone++)
					{
						childRootBone = childMeshGroup._boneList_Root[iRootBone];
						if (childRootBone == null)
						{
							continue;
						}

						MakeBoneListRecursive(rootMeshGroup, childMeshGroup, childRootBone, null, 0);//Root Bone으로 등록
					}
				}
			}
		}



		private void MakeBoneListRecursive(apMeshGroup rootMeshGroup, apMeshGroup meshGroup, apBone targetBone, BoneInfo parentBoneInfo, int level)
		{
			if(targetBone == null)
			{
				return;
			}

			//거리 계산을 하자
			float distX = 0.0f;
			float distY = 0.0f;

			//Vector2 bonePos = targetBone._worldMatrix_NonModified._pos;
			Vector2 bonePos = targetBone._worldMatrix_NonModified.Pos;//변경 20.8.17 : 래핑
			
			if(bonePos.x < _requestPos_Min.x)
			{
				distX = Mathf.Abs(bonePos.x - _requestPos_Min.x);
			}
			else if(bonePos.x < _requestPos_Max.x)
			{
				distX = 0.0f;
			}
			else
			{
				distX = Mathf.Abs(bonePos.x - _requestPos_Max.x);
			}

			if(bonePos.y < _requestPos_Min.y)
			{
				distY = Mathf.Abs(bonePos.y - _requestPos_Min.y);
			}
			else if(bonePos.y < _requestPos_Max.y)
			{
				distY = 0.0f;
			}
			else
			{
				distY = Mathf.Abs(bonePos.y - _requestPos_Max.y);
			}
			float dist = distX + distY;

			BoneInfo boneInfo = new BoneInfo(parentBoneInfo, targetBone, dist, level);
			
			_boneInfo_All.Add(boneInfo);
			
			if(parentBoneInfo == null)
			{
				//루트라면 리스트에 넣자
				if(!_boneInfo_Root.ContainsKey(meshGroup))
				{
					_boneInfo_Root.Add(meshGroup, new List<BoneInfo>());
				}
				_boneInfo_Root[meshGroup].Add(boneInfo);
			}

			//자식 본을 추가하자
			if(targetBone._childBones != null && targetBone._childBones.Count > 0)
			{
				apBone curChildBone = null;
				for (int i = targetBone._childBones.Count - 1; i >= 0; i--)
				{
					curChildBone = targetBone._childBones[i];
					MakeBoneListRecursive(rootMeshGroup, meshGroup, curChildBone, boneInfo, level+1);
				}
			}

			
		}


		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _funcResult == null)
			{
				return;
			}

			//레이아웃
			//- 검색 박스
			//- 리스트
			// 버튼들
			//- 모두 선택 / 모두 해제
			//- 가까이 있는 본들을 선택
			//- 적용 / 취소

			int searchHeight = 35;
			int bottomButtonsHeight = 100;
			int listHeight = height - (searchHeight + bottomButtonsHeight);

			if (_guiStyle_None == null)
			{
				_guiStyle_None = new GUIStyle(GUIStyle.none);
				_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_None.alignment = TextAnchor.MiddleLeft;
			}

			if (_guiStyle_None_Margin0 == null)
			{
				_guiStyle_None_Margin0 = new GUIStyle(GUIStyle.none);
				_guiStyle_None_Margin0.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_None_Margin0.alignment = TextAnchor.MiddleLeft;
				_guiStyle_None_Margin0.margin = new RectOffset(0, 0, 0, 0);
			}

			if (_guiStyle_Selected == null)
			{
				_guiStyle_Selected = new GUIStyle(GUIStyle.none);
				if (EditorGUIUtility.isProSkin)
				{
					_guiStyle_Selected.normal.textColor = Color.cyan;
				}
				else
				{
					_guiStyle_Selected.normal.textColor = Color.white;
				}
				_guiStyle_Selected.alignment = TextAnchor.MiddleLeft;
			}

			if (_guiStyle_Center == null)
			{
				_guiStyle_Center = new GUIStyle(GUIStyle.none);
				_guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_Center.alignment = TextAnchor.MiddleCenter;
			}

			if (_guiContent_BoneIcon == null)
			{
				_guiContent_BoneIcon = new GUIContent(_img_Bone);
			}
			if (_guiContent_Recommended == null)
			{
				_guiContent_Recommended = new GUIContent(_img_Recommended);
			}




			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			//GUI.Box(new Rect(0, 35, width, height - (90 + 12)), "");
			GUI.Box(new Rect(0, searchHeight + 2, width, listHeight), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			GUILayout.Space(10);

			_strSearchKeyword = EditorGUILayout.DelayedTextField(_editor.GetText(TEXT.DLG_Search) + "  ", _strSearchKeyword, GUILayout.Width(width - 20), GUILayout.Height(15));

			if (string.IsNullOrEmpty(_strSearchKeyword))
			{
				_isSearched = false;
			}
			else
			{
				_isSearched = true;
			}
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(listHeight));


#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif
			bool isShift = Event.current.shift;

			//중요! 리스트 그리기
			apMeshGroup curMeshGroup = null;
			List<BoneInfo> curBoneRootList = null;
			foreach (KeyValuePair<apMeshGroup, List<BoneInfo>> meshGroup2BoneInfoList in _boneInfo_Root)
			{
				curMeshGroup = meshGroup2BoneInfoList.Key;
				curBoneRootList = meshGroup2BoneInfoList.Value;

				GUILayout.Button(new GUIContent(string.Format("{0} {1}", curMeshGroup._name, _editor.GetText(TEXT.DLG_Bones)), _img_FoldDown), _guiStyle_None, GUILayout.Height(20));//<투명 버튼

				for (int i = 0; i < curBoneRootList.Count; i++)
				{
					DrawBoneUnit(curBoneRootList[i], 0, width, _scrollList.x, isCtrl || isShift);
				}

				GUILayout.Space(5);
			}


			GUILayout.Space(height);

			EditorGUILayout.EndScrollView();

			GUILayout.Space(5);
			int width_Half = ((width - 10) / 2) - 2;
			int height_BottomButton = 25;

			//- 모두 선택 / 모두 해제
			EditorGUILayout.BeginHorizontal(GUILayout.Height(height_BottomButton));
			GUILayout.Space(5);
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_SelectAll), GUILayout.Width(width_Half), GUILayout.Height(height_BottomButton)))
			{
				SelectAll();
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_DeselectAll), GUILayout.Width(width_Half), GUILayout.Height(height_BottomButton)))
			{
				UnselectAll();
			}
			EditorGUILayout.EndHorizontal();

			//- 가까이 있는 본들을 선택
			EditorGUILayout.BeginHorizontal(GUILayout.Height(height_BottomButton));
			GUILayout.Space(5);

			//TODO : 언어
			//"Select All Near Bones"
			if (_guiContent_SelectAllNearBone == null)
			{
				_guiContent_SelectAllNearBone = new GUIContent(
					_editor.GetText(TEXT.SelectAllNearBones),
					"Select bones near selected vertices. Pressing the Ctrl key includes the existing selected bones."
					);
			}
			if(GUILayout.Button(_guiContent_SelectAllNearBone, GUILayout.Width(width - 10), GUILayout.Height(height_BottomButton)))
			{
				SelectAllRecommended(isCtrl || isShift);
			}
			EditorGUILayout.EndHorizontal();

			bool isClose = false;
			bool isSuccess = false;

			//- 적용 / 취소
			EditorGUILayout.BeginHorizontal(GUILayout.Height(height_BottomButton));
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Apply), GUILayout.Width(width_Half), GUILayout.Height(height_BottomButton)))
			{
				//SelectAll();
				isClose = true;
				isSuccess = true;
			}
			if(GUILayout.Button(_editor.GetText(TEXT.Cancel), GUILayout.Width(width_Half), GUILayout.Height(height_BottomButton)))
			{
				//UnselectAll();
				isClose = true;
				isSuccess = false;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			if(isClose)
			{
				if(_funcResult != null)
				{
					if(isSuccess)
					{
						List<apBone> selectedBones = new List<apBone>();
						BoneInfo curBoneInfo = null;
						for (int i = 0; i < _boneInfo_All.Count; i++)
						{
							curBoneInfo = _boneInfo_All[i];
							if(curBoneInfo._isSelected && curBoneInfo._bone != null)
							{
								selectedBones.Add(curBoneInfo._bone);
							}
						}
						//Apply
						_funcResult(true, _loadKey, selectedBones);
					}
					else
					{
						//Cancel
						_funcResult(false, _loadKey, null);
					}
				}
				CloseDialog();
			}
		}

		private void DrawBoneUnit(BoneInfo boneInfo, int level, int width, float scrollX, bool isCtrl)
		{
			//Search 옵션에 따라 다르다.
			bool isRenderable = true;
			if (_isSearched)
			{
				if (boneInfo._bone != null)
				{
					if (boneInfo._name.Contains(_strSearchKeyword))
					{
						isRenderable = true;
					}
					else
					{
						isRenderable = false;
					}
				}
			}

			if (isRenderable)
			{	
				if (boneInfo._isSelected)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();


					#region [미사용 코드]
					//Color prevColor = GUI.backgroundColor;

					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}

					//GUI.Box(new Rect(scrollX, lastRect.y + 21, width, 21), "");
					//GUI.backgroundColor = prevColor; 
					#endregion

					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(scrollX + 1, lastRect.y + 21, width - 2, 21, apEditorUtil.UNIT_BG_STYLE.Main);
				}

				if (_isSearched)
				{
					if (boneInfo._parentUnit != null)
					{
						if (boneInfo._parentUnit._bone != null && !boneInfo._parentUnit._name.Contains(_strSearchKeyword))
						{
							//Parent Unit이 검색에 포함되지 않는 경우
							level = 0;
						}
					}
				}

				

				EditorGUILayout.BeginHorizontal(GUILayout.Width((width - 50) + level * 10));
				if(boneInfo._isRecommended)
				{
					EditorGUILayout.LabelField(_guiContent_Recommended, _guiStyle_None_Margin0, GUILayout.Width(15), GUILayout.Height(20));
					GUILayout.Space(level * 10);
				}
				else
				{
					GUILayout.Space(15 + (level * 10));
				}
				GUILayout.Space(15 + (level * 10));


				//Fold 관련
				//if (boneInfo._isFoldable)
				//{
				//	Texture2D foldIcon = boneInfo._isFolded ? _img_FoldDown : _img_FoldRight;

				//	if (GUILayout.Button(foldIcon, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(20)))
				//	{
				//		boneInfo._isFolded = !boneInfo._isFolded;
				//	}
				//}
				//else
				//{
				//	if (boneInfo._bone != null)
				//	{
				//		EditorGUILayout.LabelField(_guiContent_BoneIcon, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));
				//	}
				//	else
				//	{
				//		EditorGUILayout.LabelField("", _guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));
				//	}
				//}
				if (boneInfo._bone != null)
				{
					EditorGUILayout.LabelField(_guiContent_BoneIcon, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));
				}
				else
				{
					EditorGUILayout.LabelField("", _guiStyle_None, GUILayout.Width(20), GUILayout.Height(20));
				}

				GUIStyle guiStyleLabel = boneInfo._isSelected ? _guiStyle_Selected : _guiStyle_None;
				
				//if (GUILayout.Button(boneInfo._name, guiStyleLabel, GUILayout.Width((width - 35) - 22), GUILayout.Height(20)))
				if (GUILayout.Button(boneInfo._name, guiStyleLabel, GUILayout.Width(400), GUILayout.Height(20)))
				{
					//if(boneUnit._isSelectable && !boneUnit._isTarget)
					//선택을 하자
					if(isCtrl)
					{
						//Ctrl 키를 누른 상태에서는
						//선택 추가 / 제거 토글
						boneInfo._isSelected = !boneInfo._isSelected;
					}
					else
					{
						//Ctrl 키를 누르지 않았다면 이것만 선택
						UnselectAll();
						boneInfo._isSelected = true;
					}
				}

				EditorGUILayout.EndHorizontal();

			}
			if (!boneInfo._isFolded)
			{
				for (int i = 0; i < boneInfo._childUnits.Count; i++)
				{
					DrawBoneUnit(boneInfo._childUnits[i], level + 1, width, scrollX, isCtrl);
				}
			}
		}


		private void UnselectAll()
		{
			if(_boneInfo_All == null)
			{
				return;
			}
			for (int i = 0; i < _boneInfo_All.Count; i++)
			{
				_boneInfo_All[i]._isSelected = false;
			}
		}

		private void SelectAll()
		{
			if(_boneInfo_All == null)
			{
				return;
			}
			for (int i = 0; i < _boneInfo_All.Count; i++)
			{
				_boneInfo_All[i]._isSelected = true;
			}
		}
		
		private void SelectAllRecommended(bool isCtrl)
		{
			if(_boneInfo_All == null)
			{
				return;
			}
			BoneInfo boneInfo = null;
			for (int i = 0; i < _boneInfo_All.Count; i++)
			{
				boneInfo = _boneInfo_All[i];
				if (isCtrl)
				{
					if(boneInfo._isRecommended)
					{
						boneInfo._isSelected = true;
					}
				}
				else
				{
					boneInfo._isSelected = boneInfo._isRecommended;
				}
				
			}
		}
	}
}