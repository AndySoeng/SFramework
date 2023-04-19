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
	public class apDialog_SelectMeshTransformsToCopy : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		public delegate void FUNC_SELECT_MESH_TF_COPY(	bool isSuccess, 
														object loadKey, 
														apTransform_Mesh srcMeshTransform, 
														List<apTransform_Mesh> selectedObjects, 
														List<COPIED_PROPERTIES> copiedProperties);

		private static apDialog_SelectMeshTransformsToCopy s_window = null;

		//복사할 설정들
		public enum COPIED_PROPERTIES
		{
			DefaultColor,//_meshColor2X_Default
			ShaderType,//_shaderType
			CustomShader,//_isCustomShader, _customShader
			RenderTextureSize,//_renderTexSize
			TwoSides,//_isAlways2Side
			ShadowSettings,//_isUsePortraitShadowOption, _shadowCastingMode, _receiveShadow
			MaterialSet,//_materialSetID
			MaterialProperties,//_customMaterialProperties
		}
		
		private apEditor _editor = null;
		private apMeshGroup _meshGroup = null;
		private object _loadKey = null;
		private apTransform_Mesh _srcMeshTransform = null;
		private FUNC_SELECT_MESH_TF_COPY _funcResult = null;

		private Vector2 _scrollList = Vector2.zero;

		private List<apTransform_Mesh> _meshTransforms = new List<apTransform_Mesh>();

		private List<apTransform_Mesh> _selectedMeshTransforms = new List<apTransform_Mesh>();

		private Texture2D _img_Mesh = null;
		private Texture2D _img_FoldDown = null;

		private Dictionary<COPIED_PROPERTIES, bool> _copiedProperties = new Dictionary<COPIED_PROPERTIES, bool>();

		// Show Window
		//--------------------------------------------------------------
		public static object ShowDialog(apEditor editor, 
										apMeshGroup srcMeshGroup, 
										apTransform_Mesh srcMeshTransform,
										FUNC_SELECT_MESH_TF_COPY funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return null;
			}

			

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SelectMeshTransformsToCopy), true, "Select Child Meshes", true);
			apDialog_SelectMeshTransformsToCopy curTool = curWindow as apDialog_SelectMeshTransformsToCopy;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 600;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, srcMeshGroup, srcMeshTransform, funcResult);

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
						apTransform_Mesh srcMeshTransform,
						FUNC_SELECT_MESH_TF_COPY funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_meshGroup = srcMeshGroup;
			_funcResult = funcResult;

			_srcMeshTransform = srcMeshTransform;

			//타겟을 검색하자
			_meshTransforms.Clear();

			

			_img_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			//대상이 되는 데이터를 가져온다.
			for (int i = 0; i < _meshGroup._renderUnits_All.Count; i++)
			{
				apRenderUnit renderUnit = _meshGroup._renderUnits_All[i];
				if(renderUnit._meshTransform == null || renderUnit._meshTransform == _srcMeshTransform)
				{
					continue;
				}
				
				if(!_meshTransforms.Contains(renderUnit._meshTransform))
				{
					_meshTransforms.Add(renderUnit._meshTransform);
				}
			}

			_meshTransforms.Reverse();

			
			if(_copiedProperties == null)
			{
				_copiedProperties = new Dictionary<COPIED_PROPERTIES, bool>();
			}

			_copiedProperties.Clear();

			_copiedProperties.Add(COPIED_PROPERTIES.DefaultColor, false);
			_copiedProperties.Add(COPIED_PROPERTIES.ShaderType, false);
			_copiedProperties.Add(COPIED_PROPERTIES.CustomShader, false);
			_copiedProperties.Add(COPIED_PROPERTIES.RenderTextureSize, false);
			_copiedProperties.Add(COPIED_PROPERTIES.TwoSides, false);
			_copiedProperties.Add(COPIED_PROPERTIES.ShadowSettings, false);
			_copiedProperties.Add(COPIED_PROPERTIES.MaterialSet, false);
			_copiedProperties.Add(COPIED_PROPERTIES.MaterialProperties, false);
			
			
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

			int yOffset = 10;
			int height_Properties = 80;
			int height_Bottom = 70;
			int height_Main = height - (height_Properties + height_Bottom + 60);

			
			GUI.Box(new Rect(0, yOffset, width, height_Main), "");


			GUI.backgroundColor = prevColor;

			EditorGUILayout.BeginVertical();


			//Request Type이 "Mesh" 또는 "ChildTransform"이라면 탭이 없다.
			//Request Type이 "MeshAndMeshGroups"라면 탭이 있다.


			//1. Tab
			GUILayout.Space(5);

			
			int width_BtnHalf = (width - 10) / 2 - 2;
			string strCategory = _meshGroup._name;
			GUILayout.Space(5);
			
			GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;

			GUIStyle guiStyle_Selected = new GUIStyle(GUIStyle.none);
			if (EditorGUIUtility.isProSkin)
			{
				guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				guiStyle_Selected.normal.textColor = Color.white;
			}

			int height_ListItem = 20;
			string strSelected = _editor.GetText(TEXT.DLG_Selected);
			string strNotSelected = _editor.GetText(TEXT.DLG_NotSelected);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height_Main));
			EditorGUILayout.BeginVertical(GUILayout.Width(width - 24));

			GUILayout.Button(new GUIContent(strCategory, _img_FoldDown), guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

			//리스트 방식 : 아이콘 + 이름 / Selected 버튼 (토글)

			int width_SelectBtn = 100;
			int width_Label = width - (width_SelectBtn + 42);

			bool isSelected = false;

			//MeshTransform을 출력하자
			apTransform_Mesh curMeshTF = null;
			for (int i = 0; i < _meshTransforms.Count; i++)
			{
				curMeshTF = _meshTransforms[i];

				isSelected = _selectedMeshTransforms.Contains(curMeshTF);

				EditorGUILayout.BeginHorizontal(GUILayout.Height(height_ListItem));
				GUILayout.Space(10);
				EditorGUILayout.LabelField(new GUIContent(" " + curMeshTF._nickName, _img_Mesh), GUILayout.Width(width_Label), GUILayout.Height(height_ListItem));

				if(apEditorUtil.ToggledButton_2Side(strSelected, strNotSelected, isSelected, true, width_SelectBtn, height_ListItem))
				{
					if(isSelected)
					{
						_selectedMeshTransforms.Remove(curMeshTF);
					}
					else
					{
						_selectedMeshTransforms.Add(curMeshTF);
					}
				}

				EditorGUILayout.EndHorizontal();
				GUILayout.Space(2);
			}
			
			GUILayout.Space(height + 20);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(5);

			//복사할 속성들을 토글한다.
			//두줄로 적자
			
			//"Select Properties to copy"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.SelectPropertiesToCopy));
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Properties));

			GUILayout.Space(5);

			//왼쪽 줄
			EditorGUILayout.BeginVertical(GUILayout.Width(width_BtnHalf), GUILayout.Height(height_Properties));
			DrawPropertyToggle(COPIED_PROPERTIES.DefaultColor, _editor.GetText(TEXT.DefaultColor), width_BtnHalf);//"Default Color"
			DrawPropertyToggle(COPIED_PROPERTIES.ShaderType, _editor.GetText(TEXT.BlendingType), width_BtnHalf);//"Blending Type"
			DrawPropertyToggle(COPIED_PROPERTIES.CustomShader, _editor.GetText(TEXT.CustomShader), width_BtnHalf);//"Custom Shader"
			DrawPropertyToggle(COPIED_PROPERTIES.RenderTextureSize, _editor.GetText(TEXT.RenderTextureSize), width_BtnHalf);//"Render Texture Size"

			EditorGUILayout.EndVertical();

			GUILayout.Space(5);

			//오른쪽 줄
			EditorGUILayout.BeginVertical(GUILayout.Width(width_BtnHalf), GUILayout.Height(height_Properties));
			DrawPropertyToggle(COPIED_PROPERTIES.TwoSides, _editor.GetText(TEXT.TwoSidedMesh), width_BtnHalf);//"2-Sides Rendering"
			DrawPropertyToggle(COPIED_PROPERTIES.ShadowSettings, _editor.GetText(TEXT.ShadowSettings), width_BtnHalf);//"Shadow Settings"
			DrawPropertyToggle(COPIED_PROPERTIES.MaterialSet, _editor.GetText(TEXT.MaterialSet), width_BtnHalf);//"Material Set"
			DrawPropertyToggle(COPIED_PROPERTIES.MaterialProperties, _editor.GetText(TEXT.CustomMaterialProperties), width_BtnHalf);//"Custom Material Properties"
			EditorGUILayout.EndVertical();



			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			//첫줄에는 Select All / Deselect All
			//둘째줄에는 Add 또는 Apply (인자로 받음) / Close
			EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_SelectAll), GUILayout.Width(width_BtnHalf), GUILayout.Height(22)))
			{
				//Select All
				for (int i = 0; i < _meshTransforms.Count; i++)
				{
					curMeshTF = _meshTransforms[i];
					if(!_selectedMeshTransforms.Contains(curMeshTF))
					{
						_selectedMeshTransforms.Add(curMeshTF);
					}
				}
			}
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_DeselectAll), GUILayout.Width(width_BtnHalf), GUILayout.Height(22)))
			{
				//Deselect All
				_selectedMeshTransforms.Clear();
			}


			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
			GUILayout.Space(5);

			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Apply), GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))
			{
				_funcResult(true, _loadKey, _srcMeshTransform, _selectedMeshTransforms, GetCopiedPropertyList());
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width_BtnHalf), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, null, null, null);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}

		private void DrawPropertyToggle(COPIED_PROPERTIES propType, string text, int width)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			bool isNextToggle = EditorGUILayout.Toggle(_copiedProperties[propType], GUILayout.Width(15));
			if(isNextToggle != _copiedProperties[propType])
			{
				_copiedProperties[propType] = isNextToggle;
			}

			EditorGUILayout.LabelField(text, GUILayout.Width(width - 19));

			EditorGUILayout.EndHorizontal();
		}

		//복사할 속성들을 List 형태로 만들자.
		private List<COPIED_PROPERTIES> GetCopiedPropertyList()
		{
			List<COPIED_PROPERTIES> resultList = new List<COPIED_PROPERTIES>();

			
			foreach (KeyValuePair<COPIED_PROPERTIES, bool> propPair in _copiedProperties)
			{
				//속성값이 True인 것
				if(propPair.Value)
				{
					resultList.Add(propPair.Key);
				}
			}

			return resultList;
		}
	}
}