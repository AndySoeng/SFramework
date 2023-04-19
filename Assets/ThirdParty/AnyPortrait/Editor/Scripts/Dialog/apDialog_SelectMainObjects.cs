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
	public class apDialog_SelectMainObjects : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_MAIN_OBJECTS(bool isSuccess, object loadKey, TARGET_OBJECT_TYPE targetObjectType, List<object> selectedObjects);

		private static apDialog_SelectMainObjects s_window = null;

		public enum REQUEST_TYPE
		{
			RemoveMainObjects
		}

		public enum TARGET_OBJECT_TYPE
		{
			Image,
			Mesh,
			MeshGroup,
			AnimClip,
			ControlParam,
		}


		private TARGET_OBJECT_TYPE _targetObjectType = TARGET_OBJECT_TYPE.Image;


		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private object _loadKey = null;
		private FUNC_SELECT_MAIN_OBJECTS _funcResult = null;

		private Vector2 _scrollList = Vector2.zero;

		private Texture2D _img_Image = null;
		private Texture2D _img_Mesh = null;
		private Texture2D _img_MeshGroup = null;
		private Texture2D _img_AnimClip = null;
		private Texture2D _img_ControlParam = null;

		private Texture2D _img_FoldDown = null;

		private string _positiveBtnText = "";
		private string _strCategory = "";

		private enum OBJECT_TYPE
		{
			Image,
			Mesh,
			MeshGroup,
			AnimClip,
			ControlParam,
		}
		private class Unit
		{
			public OBJECT_TYPE _objectType = OBJECT_TYPE.Image;
			public object _savedObject = null;
			public string _name = null;

			public Unit(OBJECT_TYPE objectType, object savedObject, string name)
			{
				_objectType = objectType;
				_savedObject = savedObject;
				_name = name;
			}
		}

		private List<Unit> _units = null;
		private List<Unit> _selectedUnits = null;
		private Unit _lastClickedUnit = null;


		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(apEditor editor,
											REQUEST_TYPE requestType,
											TARGET_OBJECT_TYPE targetObjectType,
											FUNC_SELECT_MAIN_OBJECTS funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._objectOrders == null)
			{
				return null;
			}

			string windowName = "";
			string positiveBtnText = "";
			switch (requestType)
			{
				case REQUEST_TYPE.RemoveMainObjects:
					windowName = "Remove Objects";
					positiveBtnText = editor.GetText(TEXT.Remove);
					break;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectMainObjects), true, windowName, true);
			apDialog_SelectMainObjects curTool = curWindow as apDialog_SelectMainObjects;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, requestType, targetObjectType, funcResult, positiveBtnText);

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
			REQUEST_TYPE requestType,
			TARGET_OBJECT_TYPE targetObjectType,
			FUNC_SELECT_MAIN_OBJECTS funcResult,
			string positiveBtnText)
		{
			_editor = editor;
			_portrait = _editor._portrait;
			_loadKey = loadKey;			
			_targetObjectType = targetObjectType;
			_funcResult = funcResult;

			_positiveBtnText = positiveBtnText;

			_img_Image = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image);
			_img_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			_img_MeshGroup = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			_img_AnimClip = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);
			_img_ControlParam = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);


			_units = new List<Unit>();
			_selectedUnits = new List<Unit>();
			_lastClickedUnit = null;

			//대상 오브젝트들을 추가한다.
			switch (_targetObjectType)
			{
				case TARGET_OBJECT_TYPE.Image:
					{
						int nImages = _portrait._objectOrders.Images != null ? _portrait._objectOrders.Images.Count : 0;
						if (nImages > 0)
						{
							apTextureData curImage = null;
							for (int iImage = 0; iImage < nImages; iImage++)
							{
								curImage = _portrait._objectOrders.Images[iImage]._linked_Image;
								if (curImage == null)
								{
									continue;
								}
								_units.Add(new Unit(OBJECT_TYPE.Image, curImage, curImage._name));
							}
						}

						_strCategory = _editor.GetUIWord(UIWORD.Image);
					}
					break;

				case TARGET_OBJECT_TYPE.Mesh:
					{
						int nMeshes = _portrait._objectOrders.Meshes != null ? _portrait._objectOrders.Meshes.Count : 0;
						if (nMeshes > 0)
						{
							apMesh curMesh = null;
							for (int iMesh = 0; iMesh < nMeshes; iMesh++)
							{
								curMesh = _portrait._objectOrders.Meshes[iMesh]._linked_Mesh;
								if (curMesh == null)
								{
									continue;
								}
								_units.Add(new Unit(OBJECT_TYPE.Mesh, curMesh, curMesh._name));
							}
						}

						_strCategory = _editor.GetUIWord(UIWORD.Mesh);
					}
					break;

				case TARGET_OBJECT_TYPE.MeshGroup:
					{
						int nMeshGroups = _portrait._objectOrders.MeshGroups != null ? _portrait._objectOrders.MeshGroups.Count : 0;
						if (nMeshGroups > 0)
						{
							apMeshGroup curMeshGroup = null;
							for (int iMG = 0; iMG < nMeshGroups; iMG++)
							{
								curMeshGroup = _portrait._objectOrders.MeshGroups[iMG]._linked_MeshGroup;
								if (curMeshGroup == null)
								{
									continue;
								}
								_units.Add(new Unit(OBJECT_TYPE.MeshGroup, curMeshGroup, curMeshGroup._name));
							}
						}

						_strCategory = _editor.GetUIWord(UIWORD.MeshGroup);
					}
					break;

				case TARGET_OBJECT_TYPE.AnimClip:
					{
						int nAnimClips = _portrait._objectOrders.AnimClips != null ? _portrait._objectOrders.AnimClips.Count : 0;
						if (nAnimClips > 0)
						{
							apAnimClip curAnimClip = null;
							for (int iAnim = 0; iAnim < nAnimClips; iAnim++)
							{
								curAnimClip = _portrait._objectOrders.AnimClips[iAnim]._linked_AnimClip;
								if (curAnimClip == null)
								{
									continue;
								}
								_units.Add(new Unit(OBJECT_TYPE.AnimClip, curAnimClip, curAnimClip._name));
							}
						}

						_strCategory = _editor.GetUIWord(UIWORD.AnimationClip);
					}
					break;

				case TARGET_OBJECT_TYPE.ControlParam:
					{
						int nControlParams = _portrait._objectOrders.ControlParams != null ? _portrait._objectOrders.ControlParams.Count : 0;
						if (nControlParams > 0)
						{
							apControlParam curParam = null;
							for (int iCP = 0; iCP < nControlParams; iCP++)
							{
								curParam = _portrait._objectOrders.ControlParams[iCP]._linked_ControlParam;
								if (curParam == null)
								{
									continue;
								}
								_units.Add(new Unit(OBJECT_TYPE.ControlParam, curParam, curParam._keyName));
							}
						}

						_strCategory = _editor.GetUIWord(UIWORD.ControlParameter);
					}
					break;
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
			int height_Bottom = 40;
			int height_Main = height - (height_Bottom + 20);


			GUI.Box(new Rect(0, 5, width, height_Main), "");


			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();


			//Request Type이 "Mesh" 또는 "ChildTransform"이라면 탭이 없다.
			//Request Type이 "MeshAndMeshGroups"라면 탭이 있다.


			//1. Tab
			GUILayout.Space(5);


			int width_BtnHalf = (width - 10) / 2 - 2;

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


			int height_ListItem = 24;

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height_Main));
			EditorGUILayout.BeginVertical();

			GUILayout.Button(new GUIContent(_strCategory, _img_FoldDown), guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

			//리스트 방식 : 아이콘 + 이름 / Selected 버튼 (토글)

			//int width_SelectBtn = 100;
			//int width_Label = width - (width_SelectBtn + 42);

			bool isSelected = false;

			GUIStyle guiStyle_ItemLabelBtn = new GUIStyle(GUI.skin.label);
			guiStyle_ItemLabelBtn.alignment = TextAnchor.MiddleLeft;

			//Ctrl키나 Shift키를 누르면 여러개를 선택할 수 있다.
			//bool isCtrlOrShift = false;
			bool isCtrl = false;
			bool isShift = false;

			if (
#if UNITY_EDITOR_OSX
				Event.current.command
#else
				Event.current.control
#endif
				)
			{
				isCtrl = true;
			}

			if (Event.current.shift)
			{
				isShift = true;
			}


			int nUnits = _units != null ? _units.Count : 0;

			Unit curUnit = null;
			Texture2D imgIcon = null;
			for (int i = 0; i < nUnits; i++)
			{
				curUnit = _units[i];

				isSelected = _selectedUnits.Contains(curUnit);


				switch (curUnit._objectType)
				{
					case OBJECT_TYPE.Image:
						imgIcon = _img_Image;
						break;

					case OBJECT_TYPE.Mesh:
						imgIcon = _img_Mesh;
						break;

					case OBJECT_TYPE.MeshGroup:
						imgIcon = _img_MeshGroup;
						break;

					case OBJECT_TYPE.AnimClip:
						imgIcon = _img_AnimClip;
						break;

					case OBJECT_TYPE.ControlParam:
						imgIcon = _img_ControlParam;
						break;
				}

				if (DrawItem(curUnit._name, imgIcon, isSelected, guiStyle_None, guiStyle_Selected, i, width, height_ListItem, _scrollList.x))
				{
					//Ctrl / Shift 키를 누르지 않았다면
					if (!isCtrl)
					{
						//일단 무조건 클리어
						_selectedUnits.Clear();
						isSelected = false;
					}

					int iLastClick = -1;
					bool isLastClick = false;

					if(isCtrl || isShift)
					{
						if(_lastClickedUnit != null && _units.Contains(_lastClickedUnit))
						{
							//이전 클릭이 없으니 처음 클릭한 것과 같다.
							iLastClick = _units.IndexOf(_lastClickedUnit);
							isLastClick = true;
						}
					}

					int iCurClick = _units.IndexOf(curUnit);
					if(iCurClick == iLastClick)
					{
						isLastClick = false;
					}

					//기본 : 선택하기 (선택 해제는 없다) + 다른거 선택 해제 + 커서 이동
					//Ctrl : 추가 선택 / 선택 해제 / 커서 이동
					//Shift : 범위 선택 / 선택 해제 / 커서 이동 안함
					//Ctrl + Shift : 범위 선택 / 선택 해제 / 커서 이동

					if (!isShift)
					{
						if (!isCtrl)
						{
							//기본 클릭
							//선택하기만 존재
							if (!_selectedUnits.Contains(curUnit))
							{
								_selectedUnits.Add(curUnit);
							}
							_lastClickedUnit = curUnit;
						}
						else
						{
							//Ctrl 클릭
							//선택 또는 선택 해제 / 커서 이동
							if (isSelected)
							{
								_selectedUnits.Remove(curUnit);
							}
							else
							{
								_selectedUnits.Add(curUnit);
							}
							_lastClickedUnit = curUnit;
						}
					}
					else
					{
						//1. 이전에 선택한게 있다면 범위 선택
						//2. 그렇지 않다면 기본 클릭
						if(isLastClick)
						{
							//현재 커서의 위치
							bool isAddToList = false;
							if(!isSelected)
							{
								//선택되지 않은걸 선택했다면 그 사이는 모두 활성화
								isAddToList = true;
							}
							int iStart = iLastClick < iCurClick ? iLastClick : iCurClick;
							int iEnd = iLastClick < iCurClick ? iCurClick : iLastClick;

							Unit subUnit = null;
							for (int iSub = iStart; iSub <= iEnd; iSub++)
							{
								if(iSub < 0 || iSub >= _units.Count)
								{
									continue;
								}

								subUnit = _units[iSub];
								if(isAddToList)
								{
									//추가
									if(!_selectedUnits.Contains(subUnit))
									{
										_selectedUnits.Add(subUnit);
									}
								}
								else
								{
									//제외
									_selectedUnits.Remove(subUnit);
								}
							}
						}
						else
						{
							if (!_selectedUnits.Contains(curUnit))
							{
								_selectedUnits.Add(curUnit);
							}
						}

						//Ctrl을 누른 상태에서는 커서가 이동한다.
						if(isCtrl)
						{
							_lastClickedUnit = curUnit;
						}
					}
				}
				GUILayout.Space(2);
			}


			GUILayout.Space(height + 20);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);


			EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
			GUILayout.Space(5);

			bool isClose = false;
			if (GUILayout.Button(_positiveBtnText, GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))
			{
				List<object> selectObjects = new List<object>();
				for (int i = 0; i < _selectedUnits.Count; i++)
				{
					selectObjects.Add(_selectedUnits[i]._savedObject);
				}

				_funcResult(selectObjects.Count > 0, _loadKey, _targetObjectType, selectObjects);
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, _targetObjectType, null);
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
				if (index == 0)
				{
					yOffset = height - 2;
				}


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


				//GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + yOffset, width + 10, height + 3), "");
				//GUI.backgroundColor = prevColor; 
				#endregion

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + scrollX + 1, lastRect.y + yOffset, width + 10 - 2, height + 3, apEditorUtil.UNIT_BG_STYLE.Main);
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			bool isClick = false;

			if (GUILayout.Button(new GUIContent(" " + name, imgIcon), (isSelected ? guiStyle_Selected : guiStyle_None), GUILayout.Height(height)))
			{
				isClick = true;
			}

			EditorGUILayout.EndHorizontal();

			return isClick;
		}
	}
}