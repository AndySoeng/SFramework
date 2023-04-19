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
	public class apDialog_OnionSetting : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		private static apDialog_OnionSetting s_window = null;

		private apEditor _editor = null;
		private apPortrait _targetPortrait = null;
		
		private string[] _enumTitle_RenderMode = new string[] {"Glow Outline", "Solid"};
		private string[] _enumTitle_SelectedMode = new string[] {"Render All", "Selected Only"};
		private string[] _enumTitle_RenderOrder = new string[] {"On Top", "On Behind"};
		private string[] _enimTitle_IKCalculate = new string[] { "Always calculate", "Same as editing" };

		//private Vector2 _scroll = Vector2.zero;

		// Show Window
		//------------------------------------------------------------------
		public static void ShowDialog(apEditor editor, apPortrait portrait)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_OnionSetting), true, "Onion Setting", true);
			apDialog_OnionSetting curTool = curWindow as apDialog_OnionSetting;

			if (curTool != null && curTool != s_window)
			{
				int width = 350;
				int height = 590;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(editor, portrait);
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
		public void Init(apEditor editor, apPortrait portrait)
		{
			_editor = editor;
			_targetPortrait = portrait;
		}

		// GUI
		//------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetPortrait == null)
			{
				CloseDialog();
				return;
			}

			//만약 Portriat가 바뀌었거나 Editor가 리셋되면 닫자
			if (_editor != apEditor.CurrentEditor || _targetPortrait != apEditor.CurrentEditor._portrait)
			{
				CloseDialog();
				return;
			}

			//_scroll = EditorGUILayout.BeginScrollView(_scroll, false, true, GUILayout.Width(width), GUILayout.Height(height));
			//EditorGUILayout.BeginVertical(GUILayout.Width(width - 28));
			width -= 10;


			//Color toneColor = _editor._colorOption_OnionToneColor;
			//Color animPrevColor = _editor._colorOption_OnionAnimPrevColor;
			//Color animNextColor = _editor._colorOption_OnionAnimNextColor;

			bool isOutlineRender = _editor._onionOption_IsOutlineRender;
			float outlineThickness = _editor._onionOption_OutlineThickness;
			bool isRenderOnlySelected = _editor._onionOption_IsRenderOnlySelected;
			bool isRenderBehind = _editor._onionOption_IsRenderBehind;
			bool isRenderAnimFrames = _editor._onionOption_IsRenderAnimFrames;
			int prevRange = _editor._onionOption_PrevRange;
			int nextRange = _editor._onionOption_NextRange;
			int renderPerFrame = _editor._onionOption_RenderPerFrame;
			float posOffsetX = _editor._onionOption_PosOffsetX;
			float posOffsetY = _editor._onionOption_PosOffsetY;
			bool IKCalculateForce = _editor._onionOption_IKCalculateForce;

			bool isChanged = false;

			GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			guiStyle_Box.alignment = TextAnchor.MiddleCenter;
			if(EditorGUIUtility.isProSkin)
			{
				guiStyle_Box.normal.textColor = Color.white;
			}

			Color prevColor = GUI.backgroundColor;

			GUILayout.Space(10);
			GUI.backgroundColor = new Color(0.7f, 1.0f, 0.6f, 1.0f);
			
			GUILayout.Box(_editor.GetText(TEXT.DLG_BasicSettings), guiStyle_Box, GUILayout.Width(width), GUILayout.Height(25));//"Basic Settings"
			GUI.backgroundColor = prevColor;
			GUILayout.Space(5);
			//- 색상
			//- 렌더링 방식 < Outline / Solid (Enum)
			//- 두께
			//- 앞/뒤 < Enum
			//- 선택Only / 전체 < Enum
			int widthValue = 130;
			int widthLabel = width - (10 + widthValue + 5);
			int widthValueHalf = (widthValue / 2) - 2;

			//1. 색상들
			
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Colors), GUILayout.Width(widthLabel));//"Colors"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Mesh), GUILayout.Width(widthValueHalf));//"Mesh"
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Bone), GUILayout.Width(widthValueHalf));//"Bone"
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_SingleMarker), GUILayout.Width(widthLabel));//"Single Marker"
			try
			{
				Color nextToneColor = EditorGUILayout.ColorField(_editor._colorOption_OnionToneColor, GUILayout.Width(widthValueHalf));
				if(!IsSameColor(_editor._colorOption_OnionToneColor, nextToneColor))
				{
					_editor._colorOption_OnionToneColor = nextToneColor;
					apGL.SetToneOption(_editor._colorOption_OnionToneColor, _editor._onionOption_OutlineThickness, _editor._onionOption_IsOutlineRender, _editor._onionOption_PosOffsetX, _editor._onionOption_PosOffsetY, _editor._colorOption_OnionBoneColor);
				}
			}
			catch (Exception) { }
			try
			{
				Color nextBoneColor = EditorGUILayout.ColorField(_editor._colorOption_OnionBoneColor, GUILayout.Width(widthValueHalf));
				if(!IsSameColor(nextBoneColor, _editor._colorOption_OnionBoneColor))
				{
					_editor._colorOption_OnionBoneColor = nextBoneColor;
					apGL.SetToneOption(_editor._colorOption_OnionToneColor, _editor._onionOption_OutlineThickness, _editor._onionOption_IsOutlineRender, _editor._onionOption_PosOffsetX, _editor._onionOption_PosOffsetY, _editor._colorOption_OnionBoneColor);
				}
			}
			catch (Exception) { }
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PreviousFrames), GUILayout.Width(widthLabel));//"Previous Frames"
			try { _editor._colorOption_OnionAnimPrevColor = EditorGUILayout.ColorField(_editor._colorOption_OnionAnimPrevColor, GUILayout.Width(widthValueHalf)); }
			catch (Exception) { }
			try { _editor._colorOption_OnionBonePrevColor = EditorGUILayout.ColorField(_editor._colorOption_OnionBonePrevColor, GUILayout.Width(widthValueHalf)); }
			catch (Exception) { }
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_NextFrames), GUILayout.Width(widthLabel));//"Next Frames"
			try { _editor._colorOption_OnionAnimNextColor = EditorGUILayout.ColorField(_editor._colorOption_OnionAnimNextColor, GUILayout.Width(widthValueHalf)); }
			catch (Exception) { }
			try { _editor._colorOption_OnionBoneNextColor = EditorGUILayout.ColorField(_editor._colorOption_OnionBoneNextColor, GUILayout.Width(widthValueHalf)); }
			catch (Exception) { }
			EditorGUILayout.EndHorizontal();

			
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_RestoretoDefaultColors), GUILayout.Width(width), GUILayout.Height(20)))//"Restore Default Colors"
			{
				_editor._colorOption_OnionToneColor = apEditor.DefaultColor_OnionToneColor;
				_editor._colorOption_OnionAnimPrevColor = apEditor.DefaultColor_OnionAnimPrevColor;
				_editor._colorOption_OnionAnimNextColor = apEditor.DefaultColor_OnionAnimNextColor;
				_editor._colorOption_OnionBoneColor = apEditor.DefaultColor_OnionBoneColor;
				_editor._colorOption_OnionBonePrevColor = apEditor.DefaultColor_OnionBonePrevColor;
				_editor._colorOption_OnionBoneNextColor = apEditor.DefaultColor_OnionBoneNextColor;
				_editor.SaveEditorPref();
			}
			GUILayout.Space(10);

			//렌더 모드
			int iEnumRenderMode = (isOutlineRender ? 0 : 1);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Shape), GUILayout.Width(widthLabel));//"Shape"
			int nextEnumRenderMode = EditorGUILayout.Popup(iEnumRenderMode, _enumTitle_RenderMode, GUILayout.Width(widthValue));
			if (nextEnumRenderMode != iEnumRenderMode)
			{
				isOutlineRender = (nextEnumRenderMode == 0 ? true : false);
				isChanged = true;
			}
			EditorGUILayout.EndHorizontal();

			//- 두께
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Thickness01), GUILayout.Width(widthLabel));//"Thickness (0~1)"
			float nextOutlineThickness = EditorGUILayout.DelayedFloatField(outlineThickness, GUILayout.Width(widthValue));
			if (nextOutlineThickness != outlineThickness)
			{
				outlineThickness = nextOutlineThickness;
				isChanged = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			//- 앞/뒤 < Enum
			int iEnumRenderOrder = (isRenderBehind ? 1 : 0);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Order), GUILayout.Width(widthLabel));//"Order"
			int nextEnumRenderOrder = EditorGUILayout.Popup(iEnumRenderOrder, _enumTitle_RenderOrder, GUILayout.Width(widthValue));
			if (nextEnumRenderOrder != iEnumRenderOrder)
			{
				isRenderBehind = (nextEnumRenderOrder == 0 ? false : true);
				isChanged = true;
			}
			EditorGUILayout.EndHorizontal();

			//- 선택Only / 전체 < Enum
			int iEnumSelected = (isRenderOnlySelected ? 1 : 0);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Target), GUILayout.Width(widthLabel));//"Target"
			int nextEnumSelected = EditorGUILayout.Popup(iEnumSelected, _enumTitle_SelectedMode, GUILayout.Width(widthValue));
			if (nextEnumSelected != iEnumSelected)
			{
				isRenderOnlySelected = (nextEnumSelected == 0 ? false : true);
				isChanged = true;
			}
			EditorGUILayout.EndHorizontal();

			//float posOffsetX = _editor._onionOption_PosOffsetX;
			//float posOffsetY = _editor._onionOption_PosOffsetY;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PositionOffset), GUILayout.Width(widthLabel));//"Position Offset"
			float nextPosOffsetX = EditorGUILayout.DelayedFloatField(posOffsetX, GUILayout.Width((widthValue / 2) - 2));
			float nextPosOffsetY = EditorGUILayout.DelayedFloatField(posOffsetY, GUILayout.Width((widthValue / 2) - 2));
			if(Mathf.Abs(nextPosOffsetX - posOffsetX) > 0.0001f)
			{
				posOffsetX = nextPosOffsetX;
				isChanged = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			if(Mathf.Abs(nextPosOffsetY - posOffsetY) > 0.0001f)
			{
				posOffsetY = nextPosOffsetY;
				isChanged = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			
			EditorGUILayout.EndHorizontal();
			
			//IK 계산
			int iEnumIK = (IKCalculateForce ? 0 : 1);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_IKCalculation), GUILayout.Width(widthLabel));//"IK calculation"
			int nextEnumIK = EditorGUILayout.Popup(iEnumIK, _enimTitle_IKCalculate, GUILayout.Width(widthValue));
			if (nextEnumIK != iEnumIK)
			{
				IKCalculateForce = (nextEnumIK == 0 ? true : false);
				isChanged = true;
			}
			EditorGUILayout.EndHorizontal();

			
			
			GUILayout.Space(20);
			GUI.backgroundColor = new Color(0.3f, 0.9f, 1.0f, 1.0f);
			GUILayout.Box(_editor.GetText(TEXT.DLG_AnimationSettings), guiStyle_Box, GUILayout.Width(width), GUILayout.Height(25));//"Animation Settings"
			GUI.backgroundColor = prevColor;
			GUILayout.Space(5);
			//- 단일 프레임 <-> 범위 프레임 비교 < 아이콘
			//- 이전/이후 프레임
			//- Render Per Frame

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_AnimationFrameRendering), GUILayout.Width(width));//"Animation Frame Rendering"
			Texture2D img_SingleFrame = _editor.ImageSet.Get(apImageSet.PRESET.OnionSkin_SingleFrame);
			Texture2D img_MultipleFrame = _editor.ImageSet.Get(apImageSet.PRESET.OnionSkin_MultipleFrame);
			int width_Half = ((width - 10) / 2) - 2;


			string strBtn_SingleFrame = " " + _editor.GetText(TEXT.DLG_SingleFrame);//" Single Frame"
			string strBtn_MultipleFrames = " " + _editor.GetText(TEXT.DLG_MultipleFrames);//" Multiple Frames"
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton_2Side(img_SingleFrame, strBtn_SingleFrame, strBtn_SingleFrame, !isRenderAnimFrames, true, width_Half, 30))
			{
				isRenderAnimFrames = false;
				isChanged = true;
			}
			if(apEditorUtil.ToggledButton_2Side(img_MultipleFrame, strBtn_MultipleFrames, strBtn_MultipleFrames, isRenderAnimFrames, true, width_Half, 30))
			{
				isRenderAnimFrames = true;
				isChanged = true;
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Range), GUILayout.Width(width));//"Range"
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PreviousRange), GUILayout.Width(widthLabel));//"Previous"
			int changedPrevRange = EditorGUILayout.DelayedIntField(prevRange, GUILayout.Width(widthValue));
			if(changedPrevRange != prevRange)
			{
				prevRange = changedPrevRange;
				isChanged = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_NextRange), GUILayout.Width(widthLabel));//"Next"
			int changedNextRange = EditorGUILayout.DelayedIntField(nextRange, GUILayout.Width(widthValue));
			if(changedNextRange != nextRange)
			{
				nextRange = changedNextRange;
				isChanged = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_FramePerRender), GUILayout.Width(widthLabel));//"Render per Frame"
			int changedRenderPerFrame = EditorGUILayout.DelayedIntField(renderPerFrame, GUILayout.Width(widthValue));
			if(changedRenderPerFrame != renderPerFrame)
			{
				renderPerFrame = changedRenderPerFrame;
				isChanged = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			if(isChanged)
			{
				_editor._onionOption_IsOutlineRender = isOutlineRender;
				_editor._onionOption_OutlineThickness = Mathf.Clamp01(outlineThickness);
				_editor._onionOption_IsRenderOnlySelected = isRenderOnlySelected;
				_editor._onionOption_IsRenderBehind = isRenderBehind;
				_editor._onionOption_IsRenderAnimFrames = isRenderAnimFrames;
				_editor._onionOption_PrevRange = Mathf.Max(prevRange, 0);
				_editor._onionOption_NextRange = Mathf.Max(nextRange, 0);
				_editor._onionOption_RenderPerFrame = Mathf.Max(renderPerFrame, 1);
				_editor._onionOption_PosOffsetX = posOffsetX;
				_editor._onionOption_PosOffsetY = posOffsetY;
				_editor._onionOption_IKCalculateForce = IKCalculateForce;

				_editor.SaveEditorPref();
			}


			

			GUILayout.Space(20);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_RestoretoDefaultSettings), GUILayout.Width(width), GUILayout.Height(20)))//"Restore to Default Settings"
			{
				//TODO
				_editor._onionOption_IsOutlineRender = true;
				_editor._onionOption_OutlineThickness = 0.5f;
				_editor._onionOption_IsRenderOnlySelected = false;
				_editor._onionOption_IsRenderBehind = false;
				_editor._onionOption_IsRenderAnimFrames = false;
				_editor._onionOption_PrevRange = 1;
				_editor._onionOption_NextRange = 1;
				_editor._onionOption_RenderPerFrame = 1;
				_editor._onionOption_PosOffsetX = 0.0f;
				_editor._onionOption_PosOffsetY = 0.0f;
				_editor._onionOption_IKCalculateForce = false;

				_editor.SaveEditorPref();
			}
			GUILayout.Space(5);

			if (GUILayout.Button(_editor.GetText(TEXT.Close), GUILayout.Width(width), GUILayout.Height(35)))
			{
				_editor.SaveEditorPref();
				CloseDialog();
			}

			//GUILayout.Height(height + 300);

			//EditorGUILayout.EndVertical();
			//EditorGUILayout.EndScrollView();
		}

		private bool IsSameColor(Color colorA, Color colorB)
		{
			float bias = 0.001f;
			if(Mathf.Abs(colorA.r - colorB.r) > bias
				|| Mathf.Abs(colorA.g - colorB.g) > bias
				|| Mathf.Abs(colorA.b - colorB.b) > bias
				|| Mathf.Abs(colorA.a - colorB.a) > bias
				)
			{
				//다른 색상이다.
				return false;
			}
			//같은 색상이다.
			return true;
		}
	}
}