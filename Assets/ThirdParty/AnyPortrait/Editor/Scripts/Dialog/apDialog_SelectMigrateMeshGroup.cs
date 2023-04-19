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

	public class apDialog_SelectMigrateMeshGroup : EditorWindow
	{
		// Members
		//-------------------------------------------------------------
		public delegate void FUNC_SELECT_MIGRATE_MESHGROUP(	bool isSuccess, 
															object loadKey, 
															apMeshGroup dstMeshGroup, 
															bool isSingleTF, 
															apTransform_Mesh targetMeshTransform, 
															List<apTransform_Mesh> targetMeshTransforms, 
															apMeshGroup srcMeshGroup, bool isSelectParent);

		private static apDialog_SelectMigrateMeshGroup s_window = null;

		private apEditor _editor = null;
		private object _loadKey = null;
		//private apMeshGroup _dstMeshGroup = null;

		private enum TARGET_MODE
		{
			Single,
			Multiple
		}
		private TARGET_MODE _targetMode = TARGET_MODE.Single;
		private apTransform_Mesh _targetMeshTransform = null;
		private List<apTransform_Mesh> _targetMeshTransforms = null;
		private apMeshGroup _srcMeshGroup = null;

		private FUNC_SELECT_MIGRATE_MESHGROUP _funcResult;

		private class MeshGroupUnit
		{
			public apMeshGroup _meshGroup;
			public bool _isSelectable = false;
			public bool _isParent = false;
			
			public string _name = "";//<<Transform의 닉네임을 이름으로 사용한다.
			public int _level = 0;

			public MeshGroupUnit _parentUnit = null;
			public List<MeshGroupUnit> _childUnits = new List<MeshGroupUnit>();

			public MeshGroupUnit(apMeshGroup meshGroup, bool isSelectable, bool isParent, string name, int level, MeshGroupUnit parentUnit)
			{
				_meshGroup = meshGroup;
				_isSelectable = isSelectable;
				_isParent = isParent;
				_name = name;
				_level = level;

				_parentUnit = parentUnit;

				if(_parentUnit != null)
				{
					//자식으로도 등록한다.
					_parentUnit._childUnits.Add(this);
				}
			}
		}

		private List<MeshGroupUnit> _units = new List<MeshGroupUnit>();
		private MeshGroupUnit _rootUnit = null;

		private MeshGroupUnit _selectedUnit = null;
		private Vector2 _scrollList = new Vector2();

		//GUI Contents
		private apGUIContentWrapper _guiContent_MeshGroupIcon = null;
		private apGUIContentWrapper _guiContent_Category = null;
		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_NotSelectable = null;
		private GUIStyle _guiStyle_Center = null;
		private GUIStyle _guiStyle_Selected = null;


		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apTransform_Mesh targetMeshTransform, 
										List<apTransform_Mesh> targetMeshTransforms,
										FUNC_SELECT_MIGRATE_MESHGROUP funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			//메시 그룹에서 이 MeshTransform을 가지는 실제 MeshGroup을 찾자
			if(editor.Select == null || editor.Select.MeshGroup == null)
			{
				return null;
			}
			
			
			apMeshGroup srcMeshGroup = editor.Select.MeshGroup.GetSubParentMeshGroupOfTransformRecursive(targetMeshTransform, null);
			if(srcMeshGroup == null)
			{
				//대상이 되는 메시 그룹이 없다.
				return null;

			}


			
			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectMigrateMeshGroup), true, "Select Mesh Group to Migrate", true);
			apDialog_SelectMigrateMeshGroup curTool = curWindow as apDialog_SelectMigrateMeshGroup;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				//기본 Dialog보다 조금 더 크다. Hierarchy 방식으로 가로 스크롤이 포함되기 때문
				int width = 350;
				int height = 500;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetMeshTransform, targetMeshTransforms, srcMeshGroup, funcResult);

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
		//---------------------------------------------------------------------------------------------------------------
		private void Init(apEditor editor, object loadKey, apTransform_Mesh targetMeshTransform, 
							List<apTransform_Mesh> targetMeshTransforms,
							apMeshGroup srcMeshGroup, FUNC_SELECT_MIGRATE_MESHGROUP funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			//_dstMeshGroup = null;
			_targetMeshTransform = targetMeshTransform;
			_targetMeshTransforms = targetMeshTransforms;

			if (_targetMeshTransforms == null || _targetMeshTransforms.Count == 1)
			{
				_targetMode = TARGET_MODE.Single;
			}
			else
			{
				_targetMode = TARGET_MODE.Multiple;
			}

			_srcMeshGroup = srcMeshGroup;

			_funcResult = funcResult;

			_units = new List<MeshGroupUnit>();
			_rootUnit = null;

			_selectedUnit = null;
			_scrollList = Vector2.zero;

			//GUI Contents
			if(_guiContent_MeshGroupIcon == null)
			{
				_guiContent_MeshGroupIcon = apGUIContentWrapper.Make(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup));
			}
			
			if(_guiContent_Category == null)
			{
				_guiContent_Category = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_MeshGroups), false, _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown));
			}
			

			//먼저 Root MeshGroup을 찾자
			apMeshGroup rootMeshGroup = _srcMeshGroup;
			while (true)
			{
				if (rootMeshGroup == null)
				{
					break;
				}
				if (rootMeshGroup._parentMeshGroup == null)
				{
					break;
				}
				rootMeshGroup = rootMeshGroup._parentMeshGroup;//<<위로 이동
			}

			List<apMeshGroup> parentMeshGroups = new List<apMeshGroup>();
			List<apMeshGroup> allMeshGroups = new List<apMeshGroup>();
			
			//Parent 리스트
			_editor.Controller.FindAllParentMeshGroups(srcMeshGroup, parentMeshGroups);
			//전체 리스트
			_editor.Controller.FindAllChildMeshGroups(rootMeshGroup, allMeshGroups);
			allMeshGroups.Add(rootMeshGroup);

			
			//이제 유닛들을 만들어주자
			MakeUnit_Recursive(rootMeshGroup, true, 0, null, parentMeshGroups, srcMeshGroup, allMeshGroups);
		}

		private void MakeUnit_Recursive(apMeshGroup curMeshGroup, bool isRoot, int level, MeshGroupUnit parentUnit, List<apMeshGroup> parentMeshGroups, apMeshGroup srcMeshGroup, List<apMeshGroup> allMeshGroups)
		{
			if(!allMeshGroups.Contains(curMeshGroup))
			{
				return;
			}
			MeshGroupUnit unit = new MeshGroupUnit(curMeshGroup, (curMeshGroup != srcMeshGroup), parentMeshGroups.Contains(curMeshGroup), curMeshGroup._name, level, parentUnit);
			_units.Add(unit);
			if(isRoot)
			{
				_rootUnit = unit;
			}
			if(curMeshGroup._childMeshGroupTransforms != null
				&& curMeshGroup._childMeshGroupTransforms.Count > 0)
			{
				apTransform_MeshGroup childMeshGroupTransform = null;
				apMeshGroup childMeshGroup = null;
				
				for (int iChild = curMeshGroup._childMeshGroupTransforms.Count - 1; iChild >= 0; iChild--)
				{
					childMeshGroupTransform = curMeshGroup._childMeshGroupTransforms[iChild];
					if(childMeshGroupTransform == null
						|| childMeshGroupTransform._meshGroup == null)
					{
						continue;
					}
					childMeshGroup = childMeshGroupTransform._meshGroup;
					if(childMeshGroup == curMeshGroup)
					{
						continue;
					}

					//재귀적으로 호출
					MakeUnit_Recursive(childMeshGroup, false, level+1, unit, parentMeshGroups, srcMeshGroup, allMeshGroups);
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

			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(0, 35, width, height - (90)), "");
			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();

			if (_guiStyle_None == null)
			{
				_guiStyle_None = new GUIStyle(GUIStyle.none);
				_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_None.alignment = TextAnchor.MiddleLeft;
			}

			if(_guiStyle_NotSelectable == null)
			{
				_guiStyle_NotSelectable = new GUIStyle(GUIStyle.none);
				_guiStyle_NotSelectable.normal.textColor = Color.red;
				_guiStyle_NotSelectable.alignment = TextAnchor.MiddleLeft;
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

			GUILayout.Space(10);
			//TODO : 언어
			//"Select MeshGroup To Migrate"
			GUILayout.Button(_editor.GetText(TEXT.SelectMeshGroupToMigrate), _guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼//"Select Mesh Group to Link"
			GUILayout.Space(10);
			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (90)));

			GUILayout.Button(_guiContent_Category.Content, _guiStyle_None, GUILayout.Height(24));//<투명 버튼//"Mesh Groups"

			DrawMeshGroupUI(_rootUnit, width, (int)_scrollList.x);

			GUILayout.Space(height);

			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();


			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Height(30)))//"Select"
			{
				if (_funcResult != null)
				{
					if (_selectedUnit != null)
					{
						if (_selectedUnit._isSelectable)
						{
							_funcResult(true, _loadKey, _selectedUnit._meshGroup, _targetMode == TARGET_MODE.Single, _targetMeshTransform, _targetMeshTransforms, _srcMeshGroup, _selectedUnit._isParent);
						}
						else
						{
							_funcResult(false, _loadKey, null, true, null, null, null, false);
						}
					}
					else
					{
						_funcResult(false, _loadKey, null, true, null, null, null, false);
					}
				}
				
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, null, true, null, null, null, false);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}

		private void DrawMeshGroupUI(MeshGroupUnit unit, int width, int scrollX)
		{
			GUIStyle curGUIStyle = _guiStyle_None;
			if(!unit._isSelectable)
			{
				curGUIStyle = _guiStyle_NotSelectable;
			}
			else if (unit == _selectedUnit)
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


				//GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + 26, width, 24), "");
				//GUI.backgroundColor = prevColor; 
				#endregion

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + scrollX + 1, lastRect.y + 26, width - 2, 24, apEditorUtil.UNIT_BG_STYLE.Main);

				curGUIStyle = _guiStyle_Selected;
			}


			//EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
			EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
			GUILayout.Space(15 + (unit._level * 10));
			
			EditorGUILayout.LabelField(_guiContent_MeshGroupIcon.Content, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(24));

			if (GUILayout.Button(" " + unit._name, curGUIStyle, GUILayout.Height(24)))
			{
				if (unit._isSelectable)
				{
					_selectedUnit = unit;
				}
			}

			EditorGUILayout.EndHorizontal();

			if(unit._childUnits.Count > 0)
			{
				for (int iChild = 0; iChild < unit._childUnits.Count; iChild++)
				{
					DrawMeshGroupUI(unit._childUnits[iChild], width, scrollX);

				}
			}
		}

	}
}