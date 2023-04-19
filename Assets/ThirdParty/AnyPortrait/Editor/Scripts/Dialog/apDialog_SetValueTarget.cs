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
	public class apDialog_SetValueTarget : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_SetValueTarget s_window = null;

		private apEditor _editor = null;
		private apMeshGroup _meshGroup = null;
		private apModifierBase _modifier = null;

		private object _loadKey = null;

		[Flags]
		public enum SELECTABLE_TARGETS : int
		{
			None = 0,
			Vertices = 1,
			Pins = 2,
			Transform = 4,
			Visibility = 8,
			Color = 16,
			Extra = 32
		}

		

		public delegate void FUNC_ONSETTARGET(	bool isSuccess, object loadKey, 
												apMeshGroup meshGroup,
												apModifierBase modifier,
												SELECTABLE_TARGETS selectedTargets, 
												bool isSelectedOnly,
												object savedObject,
												List<apModifiedMesh> modMeshes,
												List<apModifiedBone> modBones,
												List<apModifiedVertex> modVerts,
												List<apModifiedPin> modPins,
												int pasteSlotIndex_Main,
												bool[] pasteSlotSelected,
												int pasteMethod,
												int nSelectedSlots
												);

		private FUNC_ONSETTARGET _func_OnSetTarget = null;

		private SELECTABLE_TARGETS _selectableTargets = SELECTABLE_TARGETS.None;
		private bool _isTarget_Vertices = false;
		private bool _isTarget_Pins = false;
		private bool _isTarget_Transform = false;
		private bool _isTarget_Visibility = false;
		private bool _isTarget_Color = false;
		private bool _isTarget_Extra = false;
		private bool _isSelectedOnly = false;//<<버텍스, 핀이 있는 경우에 설정 가능

		private object _savedObject = null;

		//선택된 객체들
		private List<apModifiedMesh> _modMeshes = null;
		private List<apModifiedBone> _modBones = null;
		private List<apModifiedVertex> _modVerts = null;
		private List<apModifiedPin> _modPins = null;

		private const string PREF_TARGET_VERTICES =			"AnyPortrait_SetTargetProps_Vertices";
		private const string PREF_TARGET_PINS =				"AnyPortrait_SetTargetProps_Pins";
		private const string PREF_TARGET_TRANSFORM =		"AnyPortrait_SetTargetProps_Transform";
		private const string PREF_TARGET_VISIBILITY =		"AnyPortrait_SetTargetProps_Visibility";
		private const string PREF_TARGET_COLOR =			"AnyPortrait_SetTargetProps_Color";
		private const string PREF_TARGET_EXTRA =			"AnyPortrait_SetTargetProps_Extra";
		private const string PREF_TARGET_SELECTED_ONLY =	"AnyPortrait_SetTargetProps_SelectedOnly";

		private int _pasteSlotIndex_Main = -1;
		private bool[] _pasteSlotSelecteds = null;
		private int _pasteMethod = -1;
		private int _nSelectedSlots = -1;


		
		// Show Window
		//------------------------------------------------------------------
		public static object ShowDialog(	apEditor editor,
											apMeshGroup meshGroup,
											apModifierBase modifier,
											SELECTABLE_TARGETS selectableTargets,
											object savedObject,
											FUNC_ONSETTARGET funcOnSetTargets,
											string strTitle)
		{
			return ShowDialog(	editor,
								meshGroup,
								modifier,
								selectableTargets,
								savedObject,
								funcOnSetTargets,
								strTitle,
								-1,
								null,
								-1,
								0);
		}



		public static object ShowDialog(	apEditor editor,
											apMeshGroup meshGroup,
											apModifierBase modifier,
											SELECTABLE_TARGETS selectableTargets,
											object savedObject,
											FUNC_ONSETTARGET funcOnSetTargets,
											string strTitle,
											//복사 정보도 붙여주자
											int pasteSlotIndex_Main,
											bool[] pasteSlotSelected,
											int pasteMethod,
											int nSelectedSlots)
		{
			CloseDialog();

			if (editor == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_SetValueTarget), true, strTitle, true);
			apDialog_SetValueTarget curTool = curWindow as apDialog_SetValueTarget;

			//object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 300;
				int height = 250;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				object loadKey = new object();

				s_window.Init(	editor, 
								meshGroup,
								modifier,
								selectableTargets,
								savedObject,
								funcOnSetTargets,
								loadKey,
								pasteSlotIndex_Main,
								pasteSlotSelected,
								pasteMethod,
								nSelectedSlots);

				return loadKey;
			}

			return null;
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
		public void Init(	apEditor editor,
							apMeshGroup meshGroup,
							apModifierBase modifier,
							SELECTABLE_TARGETS selectableTargets,
							object savedObject,
							FUNC_ONSETTARGET funcOnSetTargets,
							object loadKey,
							int pasteSlotIndex_Main,
							bool[] pasteSlotSelected,
							int pasteMethod,
							int nSelectedSlots)
		{
			_editor = editor;
			_meshGroup = meshGroup;
			_modifier = modifier;

			_modMeshes = new List<apModifiedMesh>();
			_modBones = new List<apModifiedBone>();
			_modVerts = new List<apModifiedVertex>();
			_modPins = new List<apModifiedPin>();

			int nModMeshes = editor.Select.ModMeshes_All != null ? editor.Select.ModMeshes_All.Count : 0;
			int nModBones = editor.Select.ModBones_All != null ? editor.Select.ModBones_All.Count : 0;
			int nModVerts = editor.Select.ModRenderVerts_All != null ? editor.Select.ModRenderVerts_All.Count : 0;
			int nModPins = editor.Select.ModRenderPins_All != null ? editor.Select.ModRenderPins_All.Count : 0;

			if(nModMeshes > 0)
			{
				for (int i = 0; i < nModMeshes; i++)
				{
					_modMeshes.Add(editor.Select.ModMeshes_All[i]);
				}

				//ModMesh가 선택되었을 때
				if(nModVerts > 0)
				{
					for (int i = 0; i < nModVerts; i++)
					{
						_modVerts.Add(editor.Select.ModRenderVerts_All[i]._modVert);
					}
				}

				if(nModPins > 0)
				{
					for (int i = 0; i < nModPins; i++)
					{
						_modPins.Add(editor.Select.ModRenderPins_All[i]._modPin);
					}
				}
			}

			if(nModBones > 0)
			{
				for (int i = 0; i < nModBones; i++)
				{
					_modBones.Add(editor.Select.ModBones_All[i]);
				}
			}




			_loadKey = loadKey;
			_savedObject = savedObject;

			_func_OnSetTarget = funcOnSetTargets;

			_selectableTargets = selectableTargets;
			
			_isTarget_Vertices =	GetPref(PREF_TARGET_VERTICES, true);
			_isTarget_Pins =		GetPref(PREF_TARGET_PINS, true);
			_isTarget_Transform =	GetPref(PREF_TARGET_TRANSFORM, true);
			_isTarget_Visibility =	GetPref(PREF_TARGET_VISIBILITY, true);
			_isTarget_Color =		GetPref(PREF_TARGET_COLOR, true);
			_isTarget_Extra =		GetPref(PREF_TARGET_EXTRA, true);

			_isSelectedOnly = GetPref(PREF_TARGET_SELECTED_ONLY, false);//<<이건 false가 기본값

			//어떤 값들이 보이는지 정하자
			_pasteSlotIndex_Main = pasteSlotIndex_Main;

			_pasteSlotSelecteds = null;

			if(pasteSlotSelected != null
				&& pasteSlotSelected.Length > 0)
			{
				_pasteSlotSelecteds = new bool[pasteSlotSelected.Length];
				for (int i = 0; i < pasteSlotSelected.Length; i++)
				{
					_pasteSlotSelecteds[i] = pasteSlotSelected[i];
				}
			}

			_pasteMethod = pasteMethod;
			_nSelectedSlots = nSelectedSlots;
			
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;

			int width_2Btn = ((width - 10) / 2) - 2;
			int width_1Btn = (width - 10);
			int height_PropBtn = 24;

			if(_editor == null)
			{
				CloseDialog();
				return;
			}

			//"Select Target Properties"
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.SelectTargetProperties));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Height(height - 90));
			//순서대로 설정을 보여주자

			
			


			
			if((int)(_selectableTargets & SELECTABLE_TARGETS.Vertices) != 0
				|| (int)(_selectableTargets & SELECTABLE_TARGETS.Pins) != 0)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_PropBtn));
				GUILayout.Space(5);

				//Vertices
				if(apEditorUtil.ToggledButton_2Side(	_editor.ImageSet.Get(apImageSet.PRESET.SetValueProp_Vert),
														2,
														_editor.GetUIWord(UIWORD.Vertices),
														_editor.GetUIWord(UIWORD.Vertices),
														_isTarget_Vertices,
														true,
														width_2Btn,
														height_PropBtn))
				{
					_isTarget_Vertices = !_isTarget_Vertices;
					SetPref(PREF_TARGET_VERTICES, _isTarget_Vertices, true);
				}

				//Pin
				if(apEditorUtil.ToggledButton_2Side(	_editor.ImageSet.Get(apImageSet.PRESET.SetValueProp_Pin),
														2,
														_editor.GetUIWord(UIWORD.Pins),
														_editor.GetUIWord(UIWORD.Pins),
														_isTarget_Pins,
														true,
														width_2Btn,
														height_PropBtn))
				{
					_isTarget_Pins = !_isTarget_Pins;
					SetPref(PREF_TARGET_PINS, _isTarget_Pins, true);
				}

				EditorGUILayout.EndHorizontal();

				//Vertices나 Pin이 있다면 Morph 계열이라 "선택 Only"를 보여줄 수 있다.
				//"Selected Vertices/Pins Only"
				if(DrawToggle(_editor.GetUIWord(UIWORD.OnlySelectedVerticesPins), _isSelectedOnly, width))
				{
					_isSelectedOnly = !_isSelectedOnly;
					SetPref(PREF_TARGET_SELECTED_ONLY, _isSelectedOnly, false);
				}
			}


			

			//Transform (이건 한칸 모두 차지)
			if((int)(_selectableTargets & SELECTABLE_TARGETS.Transform) != 0)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_PropBtn));
				GUILayout.Space(5);

				if(apEditorUtil.ToggledButton_2Side(	_editor.ImageSet.Get(apImageSet.PRESET.SetValueProp_Transform),
														2,
														_editor.GetUIWord(UIWORD.Transform),
														_editor.GetUIWord(UIWORD.Transform),
														_isTarget_Transform,
														true,
														width_1Btn,
														height_PropBtn))
				{
					_isTarget_Transform = !_isTarget_Transform;
					SetPref(PREF_TARGET_TRANSFORM, _isTarget_Transform, true);
				}

				EditorGUILayout.EndHorizontal();
			}
			


			if ((int)(_selectableTargets & SELECTABLE_TARGETS.Vertices) != 0
				|| (int)(_selectableTargets & SELECTABLE_TARGETS.Pins) != 0
				|| (int)(_selectableTargets & SELECTABLE_TARGETS.Transform) != 0)
			{
				//GUILayout.Space(5);
				//apEditorUtil.GUI_DelimeterBoxH(width - 10);
				//GUILayout.Space(5);
				GUILayout.Space(10);
			}
			
			if((int)(_selectableTargets & SELECTABLE_TARGETS.Visibility) != 0
				|| (int)(_selectableTargets & SELECTABLE_TARGETS.Color) != 0)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_PropBtn));
				GUILayout.Space(5);

				//Visibility
				if(apEditorUtil.ToggledButton_2Side(	_editor.ImageSet.Get(apImageSet.PRESET.SetValueProp_Visibility),
														2,
														_editor.GetUIWord(UIWORD.Visibility),
														_editor.GetUIWord(UIWORD.Visibility),
														_isTarget_Visibility,
														true,
														width_2Btn,
														height_PropBtn))
				{
					_isTarget_Visibility = !_isTarget_Visibility;
					SetPref(PREF_TARGET_VISIBILITY, _isTarget_Visibility, true);
				}

				//Color
				if(apEditorUtil.ToggledButton_2Side(	_editor.ImageSet.Get(apImageSet.PRESET.SetValueProp_Color),
														2,
														_editor.GetUIWord(UIWORD.Color),
														_editor.GetUIWord(UIWORD.Color),
														_isTarget_Color,
														true,
														width_2Btn,
														height_PropBtn))
				{
					_isTarget_Color = !_isTarget_Color;
					SetPref(PREF_TARGET_COLOR, _isTarget_Color, true);
				}
			
				EditorGUILayout.EndHorizontal();
			}

			

			

			//Extra
			if((int)(_selectableTargets & SELECTABLE_TARGETS.Extra) != 0)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_PropBtn));
				GUILayout.Space(5);

				if(apEditorUtil.ToggledButton_2Side(	_editor.ImageSet.Get(apImageSet.PRESET.SetValueProp_Extra),
														2,
														_editor.GetUIWord(UIWORD.Extra),
														_editor.GetUIWord(UIWORD.Extra),
														_isTarget_Extra,
														true,
														width_2Btn,
														height_PropBtn))
				{
					_isTarget_Extra = !_isTarget_Extra;
					SetPref(PREF_TARGET_EXTRA, _isTarget_Extra, true);
				}

				EditorGUILayout.EndHorizontal();
			}
			

			EditorGUILayout.EndVertical();

			bool isClose = false;

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal(GUILayout.Height(34));

			GUILayout.Space(5);

			
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Apply), GUILayout.Width(width_2Btn), GUILayout.Height(32)))
			{
				if(_func_OnSetTarget != null)
				{
					SELECTABLE_TARGETS selectedTargets = SELECTABLE_TARGETS.None;
					if(_isTarget_Vertices)		{ selectedTargets |= SELECTABLE_TARGETS.Vertices; }
					if(_isTarget_Pins)			{ selectedTargets |= SELECTABLE_TARGETS.Pins; }
					if(_isTarget_Transform)		{ selectedTargets |= SELECTABLE_TARGETS.Transform; }
					if(_isTarget_Visibility)	{ selectedTargets |= SELECTABLE_TARGETS.Visibility; }
					if(_isTarget_Color)			{ selectedTargets |= SELECTABLE_TARGETS.Color; }
					if(_isTarget_Extra)			{ selectedTargets |= SELECTABLE_TARGETS.Extra; }

					_func_OnSetTarget(	true,
										_loadKey,
										_meshGroup,
										_modifier,
										selectedTargets,
										_isSelectedOnly,
										_savedObject,
										_modMeshes,
										_modBones,
										_modVerts,
										_modPins,
										_pasteSlotIndex_Main,
										_pasteSlotSelecteds,
										_pasteMethod,
										_nSelectedSlots
										);
				}
				isClose = true;
			}

			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Cancel), GUILayout.Width(width_2Btn), GUILayout.Height(32)))
			{
				if(_func_OnSetTarget != null)
				{
					_func_OnSetTarget(	false,
										_loadKey,
										null,
										null,
										SELECTABLE_TARGETS.None,
										false,
										null,
										null, null, null, null,
										-1, null, -1, -1);
				}
				isClose = true;
			}

			EditorGUILayout.EndHorizontal();

			if(isClose)
			{
				CloseDialog();
			}
		}


		

		private bool DrawToggle(string strLabel, bool curValue, int width)
		{
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(strLabel, GUILayout.Width(width - (10 + 20)));

			bool nextValue = EditorGUILayout.Toggle(curValue, GUILayout.Width(20));
			EditorGUILayout.EndHorizontal();

			if(EditorGUI.EndChangeCheck())
			{
				if(nextValue != curValue)
				{
					return true;
				}
			}

			return false;
		}


		private bool GetPref(string strPref, bool isDefaultValue)
		{
			return EditorPrefs.GetBool(strPref, isDefaultValue);
		}

		private void SetPref(string strPref, bool curValue, bool isDefaultValue)
		{
			if(curValue == isDefaultValue)
			{
				//기본값이라면 삭제
				EditorPrefs.DeleteKey(strPref);
			}
			else
			{
				EditorPrefs.SetBool(strPref, curValue);
			}
		}
	}
}