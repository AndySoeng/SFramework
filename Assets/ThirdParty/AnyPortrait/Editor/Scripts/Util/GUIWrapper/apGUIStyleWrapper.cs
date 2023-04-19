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
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 19.11.16 : GUIStyle을 매번 만들지 말고 미리 만들어진 값을 사용하자.
	/// apEditor에 멤버로 속하며, static으로 바로 호출할 수 있다.
	/// apEditor에서 초기화를 한다.
	/// 빠른 접근을 위해서 일일이 멤버 변수로 만들자. (매...우 많을 듯)
	/// </summary>
	public class apGUIStyleWrapper
	{
		//타입 작성 방식
		// Members
		//-------------------------------------------
		private bool _isInitialized = false;
		private static apGUIStyleWrapper s_instance = null;

		private GUIStyle _gs_None = null;
		private GUIStyle _gs_None_LabelColor = null;
		private GUIStyle _gs_None_White2Cyan = null;
		private GUIStyle _gs_None_Margin0_Padding0 = null;
		private GUIStyle _gs_None_MiddleLeft_LabelColor = null;
		private GUIStyle _gs_None_MiddleLeft_White2Cyan = null;
		private GUIStyle _gs_None_MiddleLeft_Margin0_Black2LabelColor = null;
		private GUIStyle _gs_None_MiddleLeft_Margin0_White2Cyan = null;
		private GUIStyle _gs_None_MiddleLeft_Margin0_GrayColor = null;
		private GUIStyle _gs_None_MiddleCenter_Margin0 = null;

		private GUIStyle _gs_Label = null;
		private GUIStyle _gs_Label_MiddleCenter = null;
		private GUIStyle _gs_Label_MiddleCenter_Margin0 = null;
		private GUIStyle _gs_Label_LowerCenter_Margin0 = null;
		private GUIStyle _gs_Label_GrayColor = null;
		private GUIStyle _gs_Label_RedColor = null;
		private GUIStyle _gs_Label_MiddleLeft = null;
		private GUIStyle _gs_Label_MiddleRight = null;
		private GUIStyle _gs_Label_MiddleLeft_BlackColor = null;
		private GUIStyle _gs_Label_MiddleLeft_RedColor = null;
		private GUIStyle _gs_Label_MiddleRight_BlackColor = null;
		private GUIStyle _gs_Label_MiddleRight_RedColor = null;
		private GUIStyle _gs_Label_BoxMargin = null;
		private GUIStyle _gs_Label_LowerLeft_BoxTextColor = null;
		private GUIStyle _gs_Label_LightBlueColor = null;
		private GUIStyle _gs_Label_MiddleLeft_BtnPadding_BlackColor = null;
		private GUIStyle _gs_Label_MiddleLeft_BtnPadding_WhiteColor = null;
		private GUIStyle _gs_Label_MiddleLeft_BtnPadding_GrayColor = null;
		private GUIStyle _gs_Label_MiddleLeft_BtnPadding_Left20_BlackColor = null;
		private GUIStyle _gs_Label_MiddleLeft_BtnPadding_Left20_WhiteColor = null;
		private GUIStyle _gs_Label_MiddleLeft_BtnPadding_Left20_GrayColor = null;
		private GUIStyle _gs_Label_UpperRight = null;


		private GUIStyle _gs_Button_LabelPadding = null;
		private GUIStyle _gs_Button_MiddleCenter_BoxPadding = null;
		private GUIStyle _gs_Button_MiddleCenter_BoxPadding_White2Cyan = null;//기본 White / Pro Cyan
		private GUIStyle _gs_Button_MiddleCenter_BoxPadding_White2Black = null;//기본 White / Pro Black
		private GUIStyle _gs_Button_MiddleCenter_BoxPadding_Orange2Yellow = null;//기본 Orange / Pro Yellow
		private GUIStyle _gs_Button_MiddleLeft_BoxPadding = null;
		private GUIStyle _gs_Button_MiddleLeft_BoxPadding_White2Cyan = null;//기본 White / Pro Cyan
		private GUIStyle _gs_Button_MiddleLeft_BoxPadding_White2Black = null;//기본 White / Pro Black
		private GUIStyle _gs_Button_MiddleCenter_BoxMargin = null;
		private GUIStyle _gs_Button_TextFieldMargin = null;
		private GUIStyle _gs_Button_Margin0 = null;
		private GUIStyle _gs_Button_Margin0_Padding0 = null;
		private GUIStyle _gs_Button_VerticalMargin0 = null;
		private GUIStyle _gs_Button_MiddleCenter_VerticalMargin0 = null;
		private GUIStyle _gs_Button_MiddleCenter_VerticalMargin0_White2Cyan = null;
		private GUIStyle _gs_Button_MiddleCenter_WrapText = null;
		private GUIStyle _gs_Button_MarginHalf = null;

		private GUIStyle _gs_Box_Basic = null;
		private GUIStyle _gs_Box_MiddleCenter = null;
		private GUIStyle _gs_Box_LabelMargin_Padding0 = null;
		private GUIStyle _gs_Box_MiddleCenter_WhiteColor = null;
		private GUIStyle _gs_Box_MiddleCenter_BtnMargin_White = null;
		private GUIStyle _gs_Box_MiddleCenter_BtnMargin_White2Cyan = null;//기본 White / Pro Cyan
		private GUIStyle _gs_Box_MiddleCenter_BtnMargin_White2Black = null;//기본 White / Pro Black
		private GUIStyle _gs_Box_MiddleLeft_BtnMargin_White2Black = null;//기본 White / Pro Black
		private GUIStyle _gs_Box_MiddleCenter_BoxTextColor = null;//apEditorUtil.BoxTextColor 이용
		private GUIStyle _gs_Box_MiddleCenter_LabelMargin_WhiteColor = null;
		private GUIStyle _gs_Box_UpperCenter_WhiteColor = null;
		private GUIStyle _gs_Box_MiddleCenter_VerticalMargin0 = null;
		private GUIStyle _gs_Box_MiddleCenter_VerticalMargin0_White2Cyan = null;//기본 White / Pro Cyan


		private GUIStyle _gs_TextField_BtnMargin = null;
		private GUIStyle _gs_TextField_MiddleLeft = null;

		// Init
		//-------------------------------------------
		public apGUIStyleWrapper()
		{
			_isInitialized = false;
			s_instance = this;
		}
		public void Init()
		{
			if (_isInitialized)
			{
				return;
			}

			//하나씩 생성하자
			_gs_None = new GUIStyle(GUIStyle.none);

			_gs_None_LabelColor = new GUIStyle(GUIStyle.none);
			_gs_None_LabelColor.normal.textColor = GUI.skin.label.normal.textColor;

			_gs_None_White2Cyan = new GUIStyle(GUIStyle.none);
			_gs_None_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;

			_gs_None_Margin0_Padding0 = new GUIStyle(GUIStyle.none);
			_gs_None_Margin0_Padding0.margin = new RectOffset(0, 0, 0, 0);
			_gs_None_Margin0_Padding0.padding = new RectOffset(0, 0, 0, 0);

			_gs_None_MiddleLeft_LabelColor = new GUIStyle(GUIStyle.none);
			_gs_None_MiddleLeft_LabelColor.alignment = TextAnchor.MiddleLeft;
			_gs_None_MiddleLeft_LabelColor.normal.textColor = GUI.skin.label.normal.textColor;

			_gs_None_MiddleLeft_White2Cyan = new GUIStyle(GUIStyle.none);
			_gs_None_MiddleLeft_White2Cyan.alignment = TextAnchor.MiddleLeft;
			_gs_None_MiddleLeft_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;

			
			_gs_None_MiddleLeft_Margin0_Black2LabelColor = new GUIStyle(GUIStyle.none);
			_gs_None_MiddleLeft_Margin0_Black2LabelColor.alignment = TextAnchor.MiddleLeft;
			_gs_None_MiddleLeft_Margin0_Black2LabelColor.margin = new RectOffset(0, 0, 0, 0);
			_gs_None_MiddleLeft_Margin0_Black2LabelColor.normal.textColor = !EditorGUIUtility.isProSkin ? Color.black : GUI.skin.label.normal.textColor;
			
			_gs_None_MiddleLeft_Margin0_White2Cyan = new GUIStyle(GUIStyle.none);
			_gs_None_MiddleLeft_Margin0_White2Cyan.alignment = TextAnchor.MiddleLeft;
			_gs_None_MiddleLeft_Margin0_White2Cyan.margin = new RectOffset(0, 0, 0, 0);
			_gs_None_MiddleLeft_Margin0_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;
			
			_gs_None_MiddleLeft_Margin0_GrayColor = new GUIStyle(GUIStyle.none);
			_gs_None_MiddleLeft_Margin0_GrayColor.alignment = TextAnchor.MiddleLeft;
			_gs_None_MiddleLeft_Margin0_GrayColor.margin = new RectOffset(0, 0, 0, 0);
			_gs_None_MiddleLeft_Margin0_GrayColor.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			_gs_None_MiddleCenter_Margin0 = new GUIStyle(GUIStyle.none);
			_gs_None_MiddleCenter_Margin0.alignment = TextAnchor.MiddleCenter;
			_gs_None_MiddleCenter_Margin0.margin = new RectOffset(0, 0, 0, 0);


			_gs_Label = new GUIStyle(GUI.skin.label);

			_gs_Label_MiddleCenter = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleCenter.alignment = TextAnchor.MiddleCenter;

			_gs_Label_MiddleCenter_Margin0 = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleCenter_Margin0.alignment = TextAnchor.MiddleCenter;
			_gs_Label_MiddleCenter_Margin0.margin = new RectOffset(0, 0, 0, 0);

			_gs_Label_LowerCenter_Margin0 = new GUIStyle(GUI.skin.label);
			_gs_Label_LowerCenter_Margin0.alignment = TextAnchor.LowerCenter;
			_gs_Label_LowerCenter_Margin0.margin = new RectOffset(0, 0, 0, 0);
			_gs_Label_LowerCenter_Margin0.padding = new RectOffset(0, 0, 0, 0);

			_gs_Label_GrayColor = new GUIStyle(GUI.skin.label);
			_gs_Label_GrayColor.normal.textColor = Color.gray;

			_gs_Label_RedColor = new GUIStyle(GUI.skin.label);
			_gs_Label_RedColor.normal.textColor = Color.red;

			_gs_Label_MiddleLeft = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft.alignment = TextAnchor.MiddleLeft;

			_gs_Label_MiddleRight = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleRight.alignment = TextAnchor.MiddleRight;

			_gs_Label_MiddleLeft_BlackColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_BlackColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_BlackColor.normal.textColor = Color.black;

			_gs_Label_MiddleLeft_RedColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_RedColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_RedColor.normal.textColor = Color.red;

			_gs_Label_MiddleRight_BlackColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleRight_BlackColor.alignment = TextAnchor.MiddleRight;
			_gs_Label_MiddleRight_BlackColor.normal.textColor = Color.black;

			_gs_Label_MiddleRight_RedColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleRight_RedColor.alignment = TextAnchor.MiddleRight;
			_gs_Label_MiddleRight_RedColor.normal.textColor = Color.red;

			_gs_Label_BoxMargin = new GUIStyle(GUI.skin.label);
			_gs_Label_BoxMargin.margin = GUI.skin.box.margin;

			_gs_Label_LowerLeft_BoxTextColor = new GUIStyle(GUI.skin.label);
			_gs_Label_LowerLeft_BoxTextColor.alignment = TextAnchor.LowerLeft;
			_gs_Label_LowerLeft_BoxTextColor.normal.textColor = apEditorUtil.BoxTextColor;

			_gs_Label_LightBlueColor = new GUIStyle(GUI.skin.label);
			_gs_Label_LightBlueColor.normal.textColor = new Color(0.0f, 0.5f, 1.0f, 1.0f);

			_gs_Label_MiddleLeft_BtnPadding_BlackColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_BtnPadding_BlackColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_BtnPadding_BlackColor.padding = GUI.skin.button.padding;
			_gs_Label_MiddleLeft_BtnPadding_BlackColor.normal.textColor = Color.black;

			_gs_Label_MiddleLeft_BtnPadding_WhiteColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_BtnPadding_WhiteColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_BtnPadding_WhiteColor.padding = GUI.skin.button.padding;
			_gs_Label_MiddleLeft_BtnPadding_WhiteColor.normal.textColor = Color.white;

			_gs_Label_MiddleLeft_BtnPadding_GrayColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_BtnPadding_GrayColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_BtnPadding_GrayColor.padding = GUI.skin.button.padding;
			_gs_Label_MiddleLeft_BtnPadding_GrayColor.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			_gs_Label_MiddleLeft_BtnPadding_Left20_BlackColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_BtnPadding_Left20_BlackColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_BtnPadding_Left20_BlackColor.padding = GUI.skin.button.padding;
			_gs_Label_MiddleLeft_BtnPadding_Left20_BlackColor.padding.left += 20;
			_gs_Label_MiddleLeft_BtnPadding_Left20_BlackColor.normal.textColor = Color.black;

			_gs_Label_MiddleLeft_BtnPadding_Left20_WhiteColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_BtnPadding_Left20_WhiteColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_BtnPadding_Left20_WhiteColor.padding = GUI.skin.button.padding;
			_gs_Label_MiddleLeft_BtnPadding_Left20_WhiteColor.padding.left += 20;
			_gs_Label_MiddleLeft_BtnPadding_Left20_WhiteColor.normal.textColor = Color.white;

			_gs_Label_MiddleLeft_BtnPadding_Left20_GrayColor = new GUIStyle(GUI.skin.label);
			_gs_Label_MiddleLeft_BtnPadding_Left20_GrayColor.alignment = TextAnchor.MiddleLeft;
			_gs_Label_MiddleLeft_BtnPadding_Left20_GrayColor.padding = GUI.skin.button.padding;
			_gs_Label_MiddleLeft_BtnPadding_Left20_GrayColor.padding.left += 20;
			_gs_Label_MiddleLeft_BtnPadding_Left20_GrayColor.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

			_gs_Label_UpperRight = new GUIStyle(GUI.skin.label);
			_gs_Label_UpperRight.alignment = TextAnchor.UpperRight;


			_gs_Button_LabelPadding = new GUIStyle(GUI.skin.button);
			_gs_Button_LabelPadding.padding = GUI.skin.label.padding;

			_gs_Button_MiddleCenter_BoxPadding = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_BoxPadding.alignment = TextAnchor.MiddleCenter;
			_gs_Button_MiddleCenter_BoxPadding.padding = GUI.skin.box.padding;

			_gs_Button_MiddleCenter_BoxPadding_White2Cyan = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_BoxPadding_White2Cyan.alignment = TextAnchor.MiddleCenter;
			_gs_Button_MiddleCenter_BoxPadding_White2Cyan.padding = GUI.skin.box.padding;
			_gs_Button_MiddleCenter_BoxPadding_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;

			_gs_Button_MiddleCenter_BoxPadding_White2Black = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_BoxPadding_White2Black.alignment = TextAnchor.MiddleCenter;
			_gs_Button_MiddleCenter_BoxPadding_White2Black.padding = GUI.skin.box.padding;
			_gs_Button_MiddleCenter_BoxPadding_White2Black.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.black;
						
			_gs_Button_MiddleCenter_BoxPadding_Orange2Yellow = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_BoxPadding_Orange2Yellow.alignment = TextAnchor.MiddleCenter;
			_gs_Button_MiddleCenter_BoxPadding_Orange2Yellow.padding = GUI.skin.box.padding;
			_gs_Button_MiddleCenter_BoxPadding_Orange2Yellow.normal.textColor = !EditorGUIUtility.isProSkin ? new Color(1.0f, 0.5f, 0.0f, 1.0f) : Color.yellow;

			_gs_Button_MiddleLeft_BoxPadding = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleLeft_BoxPadding.alignment = TextAnchor.MiddleLeft;
			_gs_Button_MiddleLeft_BoxPadding.padding = GUI.skin.box.padding;

			_gs_Button_MiddleLeft_BoxPadding_White2Cyan = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleLeft_BoxPadding_White2Cyan.alignment = TextAnchor.MiddleLeft;
			_gs_Button_MiddleLeft_BoxPadding_White2Cyan.padding = GUI.skin.box.padding;
			_gs_Button_MiddleLeft_BoxPadding_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;

			_gs_Button_MiddleLeft_BoxPadding_White2Black = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleLeft_BoxPadding_White2Black.alignment = TextAnchor.MiddleLeft;
			_gs_Button_MiddleLeft_BoxPadding_White2Black.padding = GUI.skin.box.padding;
			_gs_Button_MiddleLeft_BoxPadding_White2Black.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.black;

			_gs_Button_MiddleCenter_BoxMargin = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_BoxMargin.alignment = TextAnchor.MiddleCenter;
			_gs_Button_MiddleCenter_BoxMargin.margin = GUI.skin.box.margin;

			_gs_Button_TextFieldMargin = new GUIStyle(GUI.skin.button);
			_gs_Button_TextFieldMargin.margin = GUI.skin.textField.margin;

			_gs_Button_Margin0 = new GUIStyle(GUI.skin.button);
			_gs_Button_Margin0.padding = new RectOffset(0, 0, 0, 0);

			_gs_Button_Margin0_Padding0 = new GUIStyle(GUI.skin.button);
			_gs_Button_Margin0_Padding0.margin = new RectOffset(0, 0, 0, 0);
			_gs_Button_Margin0_Padding0.padding = new RectOffset(0, 0, 0, 0);

			_gs_Button_VerticalMargin0 = new GUIStyle(GUI.skin.button);
			_gs_Button_VerticalMargin0.margin.top = 0;
			_gs_Button_VerticalMargin0.margin.bottom = 0;

			_gs_Button_MiddleCenter_VerticalMargin0 = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_VerticalMargin0.alignment = TextAnchor.MiddleCenter;
			_gs_Button_MiddleCenter_VerticalMargin0.margin.top = 0;
			_gs_Button_MiddleCenter_VerticalMargin0.margin.bottom = 0;

			_gs_Button_MiddleCenter_VerticalMargin0_White2Cyan = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_VerticalMargin0_White2Cyan.alignment = TextAnchor.MiddleCenter;
			_gs_Button_MiddleCenter_VerticalMargin0_White2Cyan.margin.top = 0;
			_gs_Button_MiddleCenter_VerticalMargin0_White2Cyan.margin.bottom = 0;
			_gs_Button_MiddleCenter_VerticalMargin0_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;

			_gs_Button_MiddleCenter_WrapText = new GUIStyle(GUI.skin.button);
			_gs_Button_MiddleCenter_WrapText.wordWrap = true;

			_gs_Button_MarginHalf = new GUIStyle(GUI.skin.button);
			_gs_Button_MarginHalf.padding.top = 5;
			_gs_Button_MarginHalf.padding.bottom = 5;
			_gs_Button_MarginHalf.padding.left = 5;
			_gs_Button_MarginHalf.padding.right = 5;


			_gs_Box_Basic = new GUIStyle(GUI.skin.box);

			_gs_Box_MiddleCenter = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter.alignment = TextAnchor.MiddleCenter;

			_gs_Box_LabelMargin_Padding0 = new GUIStyle(GUI.skin.box);
			_gs_Box_LabelMargin_Padding0.margin = GUI.skin.label.margin;
			_gs_Box_LabelMargin_Padding0.padding = new RectOffset(0, 0, 0, 0);

			_gs_Box_MiddleCenter_WhiteColor = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_WhiteColor.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_WhiteColor.normal.textColor = Color.white;


			_gs_Box_MiddleCenter_BtnMargin_White = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_BtnMargin_White.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_BtnMargin_White.margin = GUI.skin.button.margin;
			_gs_Box_MiddleCenter_BtnMargin_White.normal.textColor = Color.white;

			_gs_Box_MiddleCenter_BtnMargin_White2Cyan = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_BtnMargin_White2Cyan.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_BtnMargin_White2Cyan.margin = GUI.skin.button.margin;
			_gs_Box_MiddleCenter_BtnMargin_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;

			_gs_Box_MiddleCenter_BtnMargin_White2Black = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_BtnMargin_White2Black.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_BtnMargin_White2Black.margin = GUI.skin.button.margin;
			_gs_Box_MiddleCenter_BtnMargin_White2Black.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.black;

			_gs_Box_MiddleLeft_BtnMargin_White2Black = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleLeft_BtnMargin_White2Black.alignment = TextAnchor.MiddleLeft;
			_gs_Box_MiddleLeft_BtnMargin_White2Black.margin = GUI.skin.button.margin;
			_gs_Box_MiddleLeft_BtnMargin_White2Black.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.black;

			_gs_Box_MiddleCenter_BoxTextColor = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_BoxTextColor.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_BoxTextColor.normal.textColor = apEditorUtil.BoxTextColor;

			_gs_Box_MiddleCenter_LabelMargin_WhiteColor = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_LabelMargin_WhiteColor.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_LabelMargin_WhiteColor.margin = GUI.skin.label.margin;
			_gs_Box_MiddleCenter_LabelMargin_WhiteColor.normal.textColor = Color.white;

			_gs_Box_UpperCenter_WhiteColor = new GUIStyle(GUI.skin.box);
			_gs_Box_UpperCenter_WhiteColor.alignment = TextAnchor.UpperCenter;
			_gs_Box_UpperCenter_WhiteColor.normal.textColor = Color.white;

			_gs_Box_MiddleCenter_VerticalMargin0 = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_VerticalMargin0.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_VerticalMargin0.padding = GUI.skin.button.padding;
			_gs_Box_MiddleCenter_VerticalMargin0.margin = GUI.skin.button.margin;
			_gs_Box_MiddleCenter_VerticalMargin0.margin.top = 0;
			_gs_Box_MiddleCenter_VerticalMargin0.margin.bottom = 0;

			_gs_Box_MiddleCenter_VerticalMargin0_White2Cyan = new GUIStyle(GUI.skin.box);
			_gs_Box_MiddleCenter_VerticalMargin0_White2Cyan.alignment = TextAnchor.MiddleCenter;
			_gs_Box_MiddleCenter_VerticalMargin0_White2Cyan.padding = GUI.skin.button.padding;
			_gs_Box_MiddleCenter_VerticalMargin0_White2Cyan.margin = GUI.skin.button.margin;
			_gs_Box_MiddleCenter_VerticalMargin0_White2Cyan.margin.top = 0;
			_gs_Box_MiddleCenter_VerticalMargin0_White2Cyan.margin.bottom = 0;
			_gs_Box_MiddleCenter_VerticalMargin0_White2Cyan.normal.textColor = !EditorGUIUtility.isProSkin ? Color.white : Color.cyan;
			



			_gs_TextField_BtnMargin = new GUIStyle(GUI.skin.textField);
			_gs_TextField_BtnMargin.margin = GUI.skin.button.margin;

			_gs_TextField_MiddleLeft = new GUIStyle(GUI.skin.textField);
			_gs_TextField_MiddleLeft.alignment = TextAnchor.MiddleLeft;


			_isInitialized = true;
			s_instance = this;
		}

		// Get
		//-------------------------------------------
		public static apGUIStyleWrapper I { get { return s_instance; } }
		public bool IsInitialized() { return _isInitialized; }

		//자주 사용하는 GUIStyle Get함수
		public GUIStyle GetNone_Label_White2Cyan(bool isSelected)
		{
			return isSelected ? _gs_None_White2Cyan : _gs_None_LabelColor;
		}

		//GUIStyle들
		public GUIStyle None										{ get { return _gs_None; } }
		public GUIStyle None_LabelColor								{ get { return _gs_None_LabelColor; } }
		public GUIStyle None_White2Cyan								{ get { return _gs_None_White2Cyan; } }
		public GUIStyle None_Margin0_Padding0						{ get { return _gs_None_Margin0_Padding0; } }
		public GUIStyle None_MiddleLeft_LabelColor					{ get { return _gs_None_MiddleLeft_LabelColor; } }
		public GUIStyle None_MiddleLeft_White2Cyan					{ get { return _gs_None_MiddleLeft_White2Cyan; } }
		public GUIStyle None_MiddleLeft_Margin0_Black2LabelColor	{ get { return _gs_None_MiddleLeft_Margin0_Black2LabelColor; } }
		public GUIStyle None_MiddleLeft_Margin0_White2Cyan			{ get { return _gs_None_MiddleLeft_Margin0_White2Cyan; } }
		public GUIStyle None_MiddleLeft_Margin0_GrayColor			{ get { return _gs_None_MiddleLeft_Margin0_GrayColor; } }
		public GUIStyle None_MiddleCenter_Margin0					{ get { return _gs_None_MiddleCenter_Margin0; } }

		public GUIStyle Label					{ get { return _gs_Label; } }
		public GUIStyle Label_MiddleCenter		{ get { return _gs_Label_MiddleCenter; } }
		public GUIStyle Label_MiddleCenter_Margin0 { get { return _gs_Label_MiddleCenter_Margin0; } }
		public GUIStyle Label_LowerCenter_Margin0 { get { return _gs_Label_LowerCenter_Margin0; } }
		public GUIStyle Label_GrayColor			{ get { return _gs_Label_GrayColor; } }
		public GUIStyle Label_RedColor			{ get { return _gs_Label_RedColor; } }
		public GUIStyle Label_MiddleLeft		{ get { return _gs_Label_MiddleLeft; } }
		public GUIStyle Label_MiddleRight		{ get { return _gs_Label_MiddleRight; } }
		public GUIStyle Label_MiddleLeft_BlackColor		{ get { return _gs_Label_MiddleLeft_BlackColor; } }
		public GUIStyle Label_MiddleLeft_RedColor		{ get { return _gs_Label_MiddleLeft_RedColor; } }
		public GUIStyle Label_MiddleRight_BlackColor	{ get { return _gs_Label_MiddleRight_BlackColor; } }
		public GUIStyle Label_MiddleRight_RedColor		{ get { return _gs_Label_MiddleRight_RedColor; } }
		
		public GUIStyle Label_BoxMargin					{ get { return _gs_Label_BoxMargin; } }
		public GUIStyle Label_LowerLeft_BoxTextColor	{ get { return _gs_Label_LowerLeft_BoxTextColor; } }
		public GUIStyle Label_LightBlueColor			{ get { return _gs_Label_LightBlueColor; } }
		public GUIStyle Label_MiddleLeft_BtnPadding_BlackColor			{ get { return _gs_Label_MiddleLeft_BtnPadding_BlackColor; } }
		public GUIStyle Label_MiddleLeft_BtnPadding_WhiteColor			{ get { return _gs_Label_MiddleLeft_BtnPadding_WhiteColor; } }
		public GUIStyle Label_MiddleLeft_BtnPadding_GrayColor			{ get { return _gs_Label_MiddleLeft_BtnPadding_GrayColor; } }
		public GUIStyle Label_MiddleLeft_BtnPadding_Left20_BlackColor	{ get { return _gs_Label_MiddleLeft_BtnPadding_Left20_BlackColor; } }
		public GUIStyle Label_MiddleLeft_BtnPadding_Left20_WhiteColor	{ get { return _gs_Label_MiddleLeft_BtnPadding_Left20_WhiteColor; } }
		public GUIStyle Label_MiddleLeft_BtnPadding_Left20_GrayColor	{ get { return _gs_Label_MiddleLeft_BtnPadding_Left20_GrayColor; } }
		public GUIStyle Label_UpperRight				{ get { return _gs_Label_UpperRight; } }

		public GUIStyle Button_LabelPadding							{ get { return _gs_Button_LabelPadding; } }
		public GUIStyle Button_MiddleCenter_BoxPadding				{ get { return _gs_Button_MiddleCenter_BoxPadding; } }
		public GUIStyle Button_MiddleCenter_BoxPadding_White2Cyan	{ get { return _gs_Button_MiddleCenter_BoxPadding_White2Cyan; } }
		public GUIStyle Button_MiddleCenter_BoxPadding_White2Black	{ get { return _gs_Button_MiddleCenter_BoxPadding_White2Black; } }
		public GUIStyle Button_MiddleCenter_BoxPadding_Orange2Yellow	{ get { return _gs_Button_MiddleCenter_BoxPadding_Orange2Yellow; } }
		public GUIStyle Button_MiddleLeft_BoxPadding				{ get { return _gs_Button_MiddleLeft_BoxPadding; } }
		public GUIStyle Button_MiddleLeft_BoxPadding_White2Cyan		{ get { return _gs_Button_MiddleLeft_BoxPadding_White2Cyan; } }
		public GUIStyle Button_MiddleLeft_BoxPadding_White2Black	{ get { return _gs_Button_MiddleLeft_BoxPadding_White2Black; } }
		public GUIStyle Button_MiddleCenter_BoxMargin				{ get { return _gs_Button_MiddleCenter_BoxMargin; } }
		public GUIStyle Button_TextFieldMargin						{ get { return _gs_Button_TextFieldMargin; } }
		public GUIStyle Button_Margin0								{ get { return _gs_Button_Margin0; } }
		public GUIStyle Button_Margin0_Padding0						{ get { return _gs_Button_Margin0_Padding0; } }
		public GUIStyle Button_VerticalMargin0						{ get { return _gs_Button_VerticalMargin0; } }
		public GUIStyle Button_MiddleCenter_VerticalMargin0			{ get { return _gs_Button_MiddleCenter_VerticalMargin0; } }
		public GUIStyle Button_MiddleCenter_VerticalMargin0_White2Cyan	{ get { return _gs_Button_MiddleCenter_VerticalMargin0_White2Cyan; } }
		public GUIStyle Button_WrapText								{ get { return _gs_Button_MiddleCenter_WrapText; } }
		public GUIStyle Button_MarginHalf							{ get { return _gs_Button_MarginHalf; } }
		
		

		public GUIStyle Box_Basic								{ get { return _gs_Box_Basic; } }
		public GUIStyle Box_MiddleCenter						{ get { return _gs_Box_MiddleCenter; } }
		public GUIStyle Box_LabelMargin_Padding0				{ get { return _gs_Box_LabelMargin_Padding0; } }
		public GUIStyle Box_MiddleCenter_WhiteColor				{ get { return _gs_Box_MiddleCenter_WhiteColor; } }
		public GUIStyle Box_MiddleCenter_BtnMargin_White		{ get { return _gs_Box_MiddleCenter_BtnMargin_White; } }
		public GUIStyle Box_MiddleCenter_BtnMargin_White2Cyan	{ get { return _gs_Box_MiddleCenter_BtnMargin_White2Cyan; } }
		public GUIStyle Box_MiddleCenter_BtnMargin_White2Black	{ get { return _gs_Box_MiddleCenter_BtnMargin_White2Black; } }
		public GUIStyle Box_MiddleLeft_BtnMargin_White2Black	{ get { return _gs_Box_MiddleLeft_BtnMargin_White2Black; } }
		public GUIStyle Box_MiddleCenter_BoxTextColor			{ get { return _gs_Box_MiddleCenter_BoxTextColor; } }
		public GUIStyle Box_MiddleCenter_LabelMargin_WhiteColor { get { return _gs_Box_MiddleCenter_LabelMargin_WhiteColor; } }
		public GUIStyle Box_UpperCenter_WhiteColor				{ get { return _gs_Box_UpperCenter_WhiteColor; } }
		public GUIStyle Box_MiddleCenter_VerticalMargin0		{ get { return _gs_Box_MiddleCenter_VerticalMargin0; } }
		public GUIStyle Box_MiddleCenter_VerticalMargin0_White2Cyan { get { return _gs_Box_MiddleCenter_VerticalMargin0_White2Cyan; } }

		public GUIStyle TextField_BtnMargin			{ get { return _gs_TextField_BtnMargin; } }
		public GUIStyle TextField_MiddleLeft		{ get { return _gs_TextField_MiddleLeft; } }

	}
}