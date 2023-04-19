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
	public class apDialog_SelectMultipleObjects : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_MULTIPLE_OBJECTS(bool isSuccess, object loadKey, List<object> selectedObjects, object savedObject);

		private static apDialog_SelectMultipleObjects s_window = null;

		public enum REQUEST_TARGET
		{
			Mesh,
			MeshAndMeshGroups,
			ChildMeshTransforms,
		}


		

		public enum TARGET_TAB
		{
			Mesh,
			MeshGroup,
		}

		public enum LIST_TYPE
		{
			Mesh,
			MeshGroup,
			MeshTransforms,
		}

		private REQUEST_TARGET _requestTarget = REQUEST_TARGET.Mesh;
		private TARGET_TAB _target = TARGET_TAB.Mesh;
		private apEditor _editor = null;
		private apMeshGroup _meshGroup = null;
		private object _loadKey = null;
		private object _savedObject = null;
		private FUNC_SELECT_MULTIPLE_OBJECTS _funcResult = null;

		//private apMeshGroup _srcMeshGroup = null;
		private Vector2 _scrollList = Vector2.zero;

		private List<apMesh> _meshes = new List<apMesh>();
		private List<apMeshGroup> _meshGroups = new List<apMeshGroup>();
		private List<apTransform_Mesh> _meshTransforms = new List<apTransform_Mesh>();

		private List<object> _selectedObjects = new List<object>();

		private Texture2D _img_Mesh = null;
		private Texture2D _img_MeshGroup = null;
		private Texture2D _img_FoldDown = null;

		private string _positiveBtnText = "";
		
		private object _exceptObject = null;

		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(	apEditor editor, 
											apMeshGroup srcMeshGroup, 
											REQUEST_TARGET requestTarget, 
											FUNC_SELECT_MULTIPLE_OBJECTS funcResult, 
											string positiveBtnText, 
											object savedObject,
											object exceptObject = null)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}

			string windowName = "";
			switch (requestTarget)
			{
				case REQUEST_TARGET.Mesh:
					windowName = "Select Meshes";
					break;

				case REQUEST_TARGET.MeshAndMeshGroups:
					windowName = "Meshes and MeshGroups";
					break;

				case REQUEST_TARGET.ChildMeshTransforms:
					windowName = "Select Child Meshes";
					break;

			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectMultipleObjects), true, windowName, true);
			apDialog_SelectMultipleObjects curTool = curWindow as apDialog_SelectMultipleObjects;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, srcMeshGroup, requestTarget, funcResult, positiveBtnText, savedObject, exceptObject);

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
		//--------------------------------------------------------------------
		public void Init(apEditor editor, 
			object loadKey, 
			apMeshGroup srcMeshGroup, 
			REQUEST_TARGET requestTarget, 
			FUNC_SELECT_MULTIPLE_OBJECTS funcResult, 
			string positiveBtnText, 
			object savedObject,
			object exceptObject)
		{
			_editor = editor;
			_loadKey = loadKey;
			_meshGroup = srcMeshGroup;
			_funcResult = funcResult;

			//타겟을 검색하자
			_meshes.Clear();
			_meshGroups.Clear();
			_meshTransforms.Clear();

			_requestTarget = requestTarget;

			_target = TARGET_TAB.Mesh;

			_positiveBtnText = positiveBtnText;

			_savedObject = savedObject;
			_exceptObject = exceptObject;


			_img_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			_img_MeshGroup = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			//대상이 되는 데이터를 가져온다.
			if (_requestTarget == REQUEST_TARGET.Mesh ||
				_requestTarget == REQUEST_TARGET.MeshAndMeshGroups)
			{
				//1. Mesh
				for (int i = 0; i < _editor._portrait._meshes.Count; i++)
				{
					_meshes.Add(_editor._portrait._meshes[i]);
				}
			}

			if (_requestTarget == REQUEST_TARGET.MeshAndMeshGroups)
			{
				//2. Mesh Group
				for (int i = 0; i < _editor._portrait._meshGroups.Count; i++)
				{
					apMeshGroup meshGroup = _editor._portrait._meshGroups[i];
					if (meshGroup == srcMeshGroup || meshGroup._parentMeshGroup != null)//다른 ChildMeshGroup도 가져오지 못하게..
					{
						continue;
					}
					//재귀적으로 이미 포함된 MeshGroup인지 판단한다.
					//추가 12.03 : 그 반대도 포함해야 한다.
					apTransform_MeshGroup childMeshGroupTransform = srcMeshGroup.FindChildMeshGroupTransform(meshGroup);
					apTransform_MeshGroup childMeshGroupTransform_Rev = meshGroup.FindChildMeshGroupTransform(srcMeshGroup);
					if (childMeshGroupTransform == null && childMeshGroupTransform_Rev == null)
					{
						//child가 아닐때
						_meshGroups.Add(meshGroup);
					}
				}
			}

			if(_requestTarget == REQUEST_TARGET.ChildMeshTransforms)
			{
				//3. Child Mesh Transform
				for (int i = 0; i < _meshGroup._renderUnits_All.Count; i++)
				{
					apRenderUnit renderUnit = _meshGroup._renderUnits_All[i];
					if(renderUnit._meshTransform != null)
					{
						if(!_meshTransforms.Contains(renderUnit._meshTransform))
						{
							_meshTransforms.Add(renderUnit._meshTransform);
						}
					}
				}

				_meshTransforms.Reverse();
				
			}

			if(_exceptObject != null)
			{
				if (_exceptObject is apMesh)
				{
					apMesh exceptMesh = _exceptObject as apMesh;

					if (_meshes.Contains(exceptMesh))
					{
						_meshes.Remove(exceptMesh);
					}
				}

				if(_exceptObject is apMeshGroup)
				{
					apMeshGroup exceptMeshGroup = _exceptObject as apMeshGroup;

					if(_meshGroups.Contains(exceptMeshGroup))
					{
						_meshGroups.Remove(exceptMeshGroup);
					}
				}

				if(_exceptObject is apTransform_Mesh)
				{
					apTransform_Mesh exceptMestTF = _exceptObject as apTransform_Mesh;

					if(_meshTransforms.Contains(exceptMestTF))
					{
						_meshTransforms.Remove(exceptMestTF);
					}
				}
				
			}
			
			
		}
		


		// GUI
		//--------------------------------------------------------------------
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

			//Top : (탭) 또는 없음

			int height_Top = (_requestTarget == REQUEST_TARGET.MeshAndMeshGroups) ? 25 : 0;
			int yOffset = (_requestTarget == REQUEST_TARGET.MeshAndMeshGroups) ? (height_Top + 11) : (height_Top + 10);
			int height_Bottom = 70;
			int height_Main = height - (height_Top + height_Bottom + 20);

			
			GUI.Box(new Rect(0, yOffset, width, height_Main), "");


			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();


			//Request Type이 "Mesh" 또는 "ChildTransform"이라면 탭이 없다.
			//Request Type이 "MeshAndMeshGroups"라면 탭이 있다.


			//1. Tab
			GUILayout.Space(5);

			
			int width_BtnHalf = (width - 10) / 2 - 2;
			string strCategory = "";

			if(_requestTarget == REQUEST_TARGET.MeshAndMeshGroups)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Top));

				GUILayout.Space(5);

				//1. Mesh + MeshGroup을 선택하는 경우 탭으로 구분한다.
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Mesh), (_target == TARGET_TAB.Mesh), width_BtnHalf, height_Top))//"Mesh"
				{
					_target = TARGET_TAB.Mesh;
					_scrollList = Vector2.zero;
				}

				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_MeshGroup), (_target == TARGET_TAB.MeshGroup), width_BtnHalf, height_Top))//"MeshGroup"
				{
					_target = TARGET_TAB.MeshGroup;
					_scrollList = Vector2.zero;
				}

				
				if (_target == TARGET_TAB.Mesh)
				{
					//"Meshes";
					strCategory = _editor.GetText(TEXT.DLG_Meshes);
				}
				else
				{
					//"Mesh Groups";
					strCategory = _editor.GetText(TEXT.DLG_MeshGroups);
				}

				EditorGUILayout.EndHorizontal();
			}
			else
			{
				if(_requestTarget == REQUEST_TARGET.Mesh)
				{
					//"Meshes";
					strCategory = _editor.GetText(TEXT.DLG_Meshes);
				}
				else
				{
					//Child MeshTransform > MeshGroup 이름;
					strCategory = _meshGroup._name;
				}

				GUILayout.Space(5);
			}


			LIST_TYPE curListType = LIST_TYPE.Mesh;
			switch (_requestTarget)
			{
				case REQUEST_TARGET.Mesh:
					curListType = LIST_TYPE.Mesh;
					break;

				case REQUEST_TARGET.ChildMeshTransforms:
					curListType = LIST_TYPE.MeshTransforms;
					break;

				case REQUEST_TARGET.MeshAndMeshGroups:
					{
						if(_target == TARGET_TAB.Mesh)
						{
							curListType = LIST_TYPE.Mesh;
						}
						else
						{
							curListType = LIST_TYPE.MeshGroup;
						}
					}
					break;
			}

			
			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);

			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
				guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
				guiStyle_None.normal.textColor = Color.black;
			}

			//Texture2D whildImg = apEditorUtil.WhiteTexture;


			int height_ListItem = 25;
			//string strSelected = _editor.GetText(TEXT.DLG_Selected);
			//string strNotSelected = _editor.GetText(TEXT.DLG_NotSelected);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height_Main));
			EditorGUILayout.BeginVertical();

			GUILayout.Button(new GUIContent(strCategory, _img_FoldDown), guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

			//리스트 방식 : 아이콘 + 이름 / Selected 버튼 (토글)

			//int width_SelectBtn = 100;
			//int width_Label = width - (width_SelectBtn + 42);

			bool isSelected = false;

			GUIStyle guiStyle_ItemLabelBtn = new GUIStyle(GUI.skin.label);
			guiStyle_ItemLabelBtn.alignment = TextAnchor.MiddleLeft;
			
			//Ctrl키나 Shift키를 누르면 여러개를 선택할 수 있다.
			bool isCtrlOrShift = false;

			if(Event.current.shift
#if UNITY_EDITOR_OSX
				|| Event.current.command
#else
				|| Event.current.control
#endif		
				)
			{
				isCtrlOrShift = true;
			}


			//어떤 리스트를 보여야 하는지 여부
			if(curListType == LIST_TYPE.Mesh)
			{
				//1. Mesh를 출력하자
				apMesh curMesh = null;
				for (int i = 0; i < _meshes.Count; i++)
				{
					curMesh = _meshes[i];

					isSelected = _selectedObjects.Contains(curMesh);
					

					if(DrawItem(curMesh._name, _img_Mesh, isSelected, guiStyle_None, guiStyle_Selected, i, width, height_ListItem, _scrollList.x))
					{
						if(!isCtrlOrShift)
						{
							//일단 무조건 클리어
							_selectedObjects.Clear();
						}

						if(isSelected)
						{
							_selectedObjects.Remove(curMesh);
						}
						else
						{
							_selectedObjects.Add(curMesh);
						}
					}
					GUILayout.Space(2);
				}
			}
			else if(curListType == LIST_TYPE.MeshGroup)
			{
				//2. MeshGroup을 출력하자
				apMeshGroup curMeshGroup = null;
				for (int i = 0; i < _meshGroups.Count; i++)
				{
					curMeshGroup = _meshGroups[i];

					isSelected = _selectedObjects.Contains(curMeshGroup);
					

					if(DrawItem(curMeshGroup._name, _img_MeshGroup, isSelected, guiStyle_None, guiStyle_Selected, i, width, height_ListItem, _scrollList.x))
					{
						if(!isCtrlOrShift)
						{
							//일단 무조건 클리어
							_selectedObjects.Clear();
						}

						if(isSelected)
						{
							_selectedObjects.Remove(curMeshGroup);
						}
						else
						{
							_selectedObjects.Add(curMeshGroup);
						}
					}
					GUILayout.Space(2);
				}
			}
			else
			{
				//3. MeshTransform을 출력하자
				apTransform_Mesh curMeshTF = null;
				for (int i = 0; i < _meshTransforms.Count; i++)
				{
					curMeshTF = _meshTransforms[i];

					isSelected = _selectedObjects.Contains(curMeshTF);
					

					if(DrawItem(curMeshTF._nickName, _img_Mesh, isSelected, guiStyle_None, guiStyle_Selected, i, width, height_ListItem, _scrollList.x))
					{
						if(!isCtrlOrShift)
						{
							//일단 무조건 클리어
							_selectedObjects.Clear();
						}

						if(isSelected)
						{
							_selectedObjects.Remove(curMeshTF);
						}
						else
						{
							_selectedObjects.Add(curMeshTF);
						}
					}
					GUILayout.Space(2);
				}
			}
			
			GUILayout.Space(height + 20);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			//첫줄에는 Select All / Deselect All
			//둘째줄에는 Add 또는 Apply (인자로 받음) / Close
			EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_SelectAll), GUILayout.Width(width_BtnHalf), GUILayout.Height(22)))
			{
				//Select All
				for (int i = 0; i < _meshes.Count; i++)
				{
					apMesh curMesh = _meshes[i];
					if(!_selectedObjects.Contains(curMesh))
					{
						_selectedObjects.Add(curMesh);
					}
				}

				for (int i = 0; i < _meshGroups.Count; i++)
				{
					apMeshGroup curMeshGroup = _meshGroups[i];
					if(!_selectedObjects.Contains(curMeshGroup))
					{
						_selectedObjects.Add(curMeshGroup);
					}
				}

				for (int i = 0; i < _meshTransforms.Count; i++)
				{
					apTransform_Mesh curMeshTF = _meshTransforms[i];
					if(!_selectedObjects.Contains(curMeshTF))
					{
						_selectedObjects.Add(curMeshTF);
					}
				}


			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_DeselectAll), GUILayout.Width(width_BtnHalf), GUILayout.Height(22)))
			{
				//Deselect All
				_selectedObjects.Clear();
			}


			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
			GUILayout.Space(5);

			bool isClose = false;
			if (GUILayout.Button(_positiveBtnText, GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))
			{
				_funcResult(true, _loadKey, _selectedObjects, _savedObject);
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, null, _savedObject);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}

		private bool DrawItem(string name, Texture2D imgIcon, bool isSelected, GUIStyle guiStyle_None, GUIStyle guiStyle_Selected, int index, int width, int height, float scrollX)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();

			if (isSelected)
			{
				int yOffset = 0;
				if(index == 0)
				{
					yOffset = height - 2;
				}

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

				//GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + yOffset , width + 10, height + 3), "");
				//GUI.backgroundColor = prevColor; 
				#endregion

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + scrollX + 1, lastRect.y + yOffset , width + 10 - 2, height + 3, apEditorUtil.UNIT_BG_STYLE.Main);
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			bool isClick = false;

			if(GUILayout.Button(new GUIContent(" " + name, imgIcon), (isSelected ? guiStyle_Selected : guiStyle_None), GUILayout.Height(height)))
			{
				isClick = true;
			}

			EditorGUILayout.EndHorizontal();

			return isClick;
		}
	}
}