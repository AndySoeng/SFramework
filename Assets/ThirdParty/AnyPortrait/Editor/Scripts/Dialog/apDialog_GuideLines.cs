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
	public class apDialog_GuideLines : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		private static apDialog_GuideLines s_window = null;

		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private apGuideLines _guideLines = null;

		private Vector2 _scroll = Vector2.zero;
		private GUIStyle _guiStyle_CenterLabel = null;

		private Texture2D _img_Visible = null;
		private Texture2D _img_Invisible = null;
		private Texture2D _img_Remove = null;

		// Show Window
		//--------------------------------------------------------------
		public static void ShowDialog(apEditor editor)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null)
			{
				return;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_GuideLines), true, "Guidelines", true);
			apDialog_GuideLines curTool = curWindow as apDialog_GuideLines;

			
			if (curTool != null && curTool != s_window)
			{
				int width = 450;
				int height = 450;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor);
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
		public void Init(apEditor editor)
		{
			_editor = editor;
			_portrait = _editor._portrait;
			_guideLines = _editor._portrait.GuideLines;

			_img_Visible = editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current);
			_img_Invisible = editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current);
			_img_Remove = editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform);
		}

		// GUI
		//--------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null
				|| _editor._portrait != _portrait)
			{
				CloseDialog();
				return;
			}

			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.GUIMenu_Guidelines));//"Guidelines"
			GUILayout.Space(5);

			//스크롤 박스
			Color prevColor = GUI.backgroundColor;


			int width_List = width - 25;
			int height_List = height - 160;
			GUI.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
			GUI.Box(new Rect(0, 5 + 20, width_List + 25, height_List + 22), apStringFactory.I.None);//왼쪽 위 리스트
			GUI.backgroundColor = prevColor;


			if (_guiStyle_CenterLabel == null)
			{
				_guiStyle_CenterLabel = new GUIStyle(GUI.skin.label);
				_guiStyle_CenterLabel.alignment = TextAnchor.MiddleCenter;
			}

			int height_Item = 25;
			int width_Enabled = 30;
			int width_Remove = 30;
			int width_Direction = 80;
			int width_Thickness = 80;
			int width_Color = 60;
			int width_Position = (width_List - 20) - (width_Enabled + width_Direction + width_Thickness + width_Color + width_Remove + 24 + 25);
			
			//항목 설명
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10 + width_Enabled + 5 + 2);
			
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Direction), _guiStyle_CenterLabel, GUILayout.Width(width_Direction));//"Direction"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Position), _guiStyle_CenterLabel, GUILayout.Width(width_Position));//"Position"
			GUILayout.Space(10);
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Color), _guiStyle_CenterLabel, GUILayout.Width(width_Color));//"Color"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Thickness), _guiStyle_CenterLabel, GUILayout.Width(width_Thickness));//"Thickness"
			EditorGUILayout.EndHorizontal();

			_scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(height_List));
			EditorGUILayout.BeginVertical(GUILayout.Width(width_List));
			GUILayout.Space(5);

			//가이드 라인 정보들
			int nGuidelines = _guideLines.NumLines;
			List<apGuideLines.LineInfo> lineInfos = _guideLines.Lines;
			apGuideLines.LineInfo curInfo = null;
			apGuideLines.LineInfo removeInfo = null;

			
			int margin_Vertical = ((height_Item - 20) / 2) + 4;

			if (nGuidelines > 0 && lineInfos != null)
			{
				for (int iLineInfo = 0; iLineInfo < nGuidelines; iLineInfo++)
				{
					curInfo = lineInfos[iLineInfo];

					EditorGUILayout.BeginHorizontal(GUILayout.Height(height_Item));
					GUILayout.Space(10);

					//1. Enabled
					if(apEditorUtil.ToggledButton_2Side(_img_Visible, _img_Invisible, curInfo._isEnabled, true, width_Enabled, height_Item))
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curInfo._isEnabled = !curInfo._isEnabled;
					}
					GUILayout.Space(5);

					//2. 방향
					EditorGUILayout.BeginVertical(GUILayout.Width(width_Direction), GUILayout.Height(height_Item));
					GUILayout.Space(margin_Vertical);
					apGuideLines.LINE_DIRECTION nextDirection = (apGuideLines.LINE_DIRECTION)EditorGUILayout.EnumPopup(curInfo._direction);
					if(nextDirection != curInfo._direction)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curInfo._direction = nextDirection;
					}
					EditorGUILayout.EndVertical();

					//3. 위치
					EditorGUILayout.BeginVertical(GUILayout.Width(width_Position), GUILayout.Height(height_Item));
					GUILayout.Space(margin_Vertical);
					int nextPosition = EditorGUILayout.DelayedIntField(curInfo._position, GUILayout.Width(width_Position));
					if(nextPosition != curInfo._position)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);
						curInfo._position = nextPosition;
					}
					EditorGUILayout.EndVertical();


					GUILayout.Space(10);

					//4. 색상
					EditorGUILayout.BeginVertical(GUILayout.Width(width_Color), GUILayout.Height(height_Item));
					GUILayout.Space(margin_Vertical);
					Color nextColor = curInfo._color;
					try
					{
						nextColor = EditorGUILayout.ColorField(curInfo._color, GUILayout.Width(width_Color));
						if(Mathf.Abs(nextColor.r - curInfo._color.r) > 0.01f
							|| Mathf.Abs(nextColor.g - curInfo._color.g) > 0.01f
							|| Mathf.Abs(nextColor.b - curInfo._color.b) > 0.01f
							|| Mathf.Abs(nextColor.a - curInfo._color.a) > 0.01f)
						{
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

							curInfo._color = nextColor;
						}
					}
					catch(Exception) {}
					EditorGUILayout.EndVertical();

					//5. 두께
					EditorGUILayout.BeginVertical(GUILayout.Width(width_Thickness), GUILayout.Height(height_Item));
					GUILayout.Space(margin_Vertical);
					apGuideLines.LINE_THICKNESS nextThickness = (apGuideLines.LINE_THICKNESS)EditorGUILayout.EnumPopup(curInfo._thickness, GUILayout.Width(width_Thickness));
					if(nextThickness != curInfo._thickness)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curInfo._thickness = nextThickness;
					}
					EditorGUILayout.EndVertical();

					GUILayout.Space(10);

					//삭제 버튼
					if(GUILayout.Button(_img_Remove, GUILayout.Width(width_Remove), GUILayout.Height(height_Item)))
					{
						removeInfo = curInfo;
					}

					EditorGUILayout.EndHorizontal();
					GUILayout.Space(10);
					
				}
			}
			

			GUILayout.Space(height_List + 50);
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndScrollView();
			GUILayout.Space(10);
			//추가하기
			if(GUILayout.Button(_editor.GetText(TEXT.AddNewGuidelines), GUILayout.Height(30)))//"Add New Guideline"
			{
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

				_guideLines.AddNewLine();

				//추가 21.10.6
				//가이드라인이 바로 보이게 하자
				_editor._isEnableGuideLine = true;
			}
			if(GUILayout.Button(_editor.GetText(TEXT.RemoveAllGuidelines), GUILayout.Height(20)))//"Remove All Guidelines"
			{
				//TODO : 언어
				//"Remove Guidelines", "Do you want to remove all Guidelines?", "Remove", "Cancel"
				bool isResult = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_RemoveGuideline_Title),
																_editor.GetText(TEXT.DLG_RemoveGuideline_All_Body),
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));
				if(isResult)
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

					_guideLines.RemoveAllLines();
				}
			}
			GUILayout.Space(5);
			if(GUILayout.Button(_editor.GetText(TEXT.Close), GUILayout.Height(25)))//"Close"
			{
				CloseDialog();
			}

			if(removeInfo != null)
			{
				//"Remove Guideline", "Do you want to remove the selected Guideline?", "Remove", "Cancel"
				bool isResult = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_RemoveGuideline_Title),
																_editor.GetText(TEXT.DLG_RemoveGuideline_Single_Body),
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));
				if(isResult)
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.GuidelineChanged, 
															_editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

					_guideLines.RemoveLine(removeInfo);
				}
			}
		}
	}
}