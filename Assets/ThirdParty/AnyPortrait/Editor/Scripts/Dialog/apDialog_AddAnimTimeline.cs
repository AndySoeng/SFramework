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

	public class apDialog_AddAnimTimeline : EditorWindow
	{
		// Members
		//------------------------------------------------------------------------
		public delegate void FUNC_ADD_TIMELINE(bool isSuccess, object loadKey, apAnimClip.LINK_TYPE linkType, int modifierUniqueID, apAnimClip targetAnimClip);

		private static apDialog_AddAnimTimeline s_window = null;

		private apEditor _editor = null;
		private object _loadKey = null;

		private FUNC_ADD_TIMELINE _funcResult;
		private apAnimClip _targetAnimClip = null;

		private class LinkableData
		{
			public apAnimClip.LINK_TYPE _linkType = apAnimClip.LINK_TYPE.ControlParam;
			public string _name = "";
			public int _modifierID = -1;//<<Mod에 연결된 경우
			public Texture2D _icon = null;

			public LinkableData(apAnimClip.LINK_TYPE linkType, string name, int modifierID, Texture2D icon)
			{
				_linkType = linkType;
				_name = name;
				_modifierID = modifierID;
				_icon = icon;
			}
		}

		private List<LinkableData> _linkDataList = new List<LinkableData>();
		private LinkableData _selectedLinkData = null;

		private Vector2 _scrollList = new Vector2();
		private apGUIContentWrapper _guiContent_TimelineTypes = null;
		private apGUIContentWrapper _guiContent_LinkDataNameAndIcon = null;

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apAnimClip targetAnimClip, FUNC_ADD_TIMELINE funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_AddAnimTimeline), true, "Select Timeline Type", true);
			apDialog_AddAnimTimeline curTool = curWindow as apDialog_AddAnimTimeline;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 250;
				int height = 400;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetAnimClip, funcResult);

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
		public void Init(apEditor editor, object loadKey, apAnimClip targetAnimGroup, FUNC_ADD_TIMELINE funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetAnimClip = targetAnimGroup;

			_selectedLinkData = null;
			_linkDataList.Clear();

			//타임라인을 검색해보자

			//리스트에 들어가는건 한글로 바꾸기 힘들 수 있다.

			//_linkDataList.Add(new LinkableData(apAnimClip.LINK_TYPE.ControlParam, "Control Parameters", -1, _editor.ImageSet.Get(apImageSet.PRESET.Anim_WithControlParam)));
			AddLinkableData(apAnimClip.LINK_TYPE.ControlParam, "Control Parameters", -1);//"Control Parameters"
			//AddLinkableData(apAnimClip.LINK_TYPE.Bone, "Bones", -1);

			List<apModifierBase> modifiers = _targetAnimClip._targetMeshGroup._modifierStack._modifiers;
			for (int i = 0; i < modifiers.Count; i++)
			{
				apModifierBase curMod = modifiers[i];

				if (!curMod.IsAnimated)
				{
					continue;
				}

				//"Modifier : "
				AddLinkableData(apAnimClip.LINK_TYPE.AnimatedModifier, "Modifier : " + curMod.DisplayName, curMod._uniqueID);

			}

		}

		private void AddLinkableData(apAnimClip.LINK_TYPE linkType, string name, int modifierID)
		{
			bool isExist = _targetAnimClip.IsTimelineContain(linkType, modifierID);
			if (isExist)
			{
				//이미 있는 타입이면 제외하자
				return;
			}

			Texture2D icon = null;
			switch (linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					icon = _editor.ImageSet.Get(apImageSet.PRESET.Anim_WithMod);
					break;

				//case apAnimClip.LINK_TYPE.Bone:
				//	icon = _editor.ImageSet.Get(apImageSet.PRESET.Anim_WithBone);
				//	break;

				case apAnimClip.LINK_TYPE.ControlParam:
					icon = _editor.ImageSet.Get(apImageSet.PRESET.Anim_WithControlParam);
					break;

				default:
					Debug.LogError("알 수 없는 AnimClip LinkType [" + linkType + "]");
					break;
			}

			_linkDataList.Add(new LinkableData(linkType, name, modifierID, icon));
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

			Texture2D iconImageCategory = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

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
			

			GUIStyle guiStyle_Center = new GUIStyle(GUIStyle.none);
			guiStyle_Center.normal.textColor = GUI.skin.label.normal.textColor;
			guiStyle_Center.alignment = TextAnchor.MiddleCenter;

			GUILayout.Space(10);
			//"Select Timeline Type to Add"
			GUILayout.Button(_editor.GetText(TEXT.DLG_SelectTimelineTypeToAdd), guiStyle_Center, GUILayout.Width(width), GUILayout.Height(15));//<투명 버튼
			GUILayout.Space(10);

			_scrollList = EditorGUILayout.BeginScrollView(_scrollList, GUILayout.Width(width), GUILayout.Height(height - (90)));

			//"Timeline Types"
			if(_guiContent_TimelineTypes == null)
			{
				_guiContent_TimelineTypes = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_TimelineTypes), false, iconImageCategory);
			}
			

			GUILayout.Button(_guiContent_TimelineTypes.Content, guiStyle_None, GUILayout.Height(20));//<투명 버튼


			//항목의 이름
			if (_guiContent_LinkDataNameAndIcon == null)
			{
				_guiContent_LinkDataNameAndIcon = new apGUIContentWrapper(false);
			}


			//GUILayout.Space(10);
			for (int i = 0; i < _linkDataList.Count; i++)
			{
				GUIStyle curGUIStyle = guiStyle_None;
				if (_linkDataList[i] == _selectedLinkData)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();

					//이전
					#region [미사용 코드]
					//prevColor = GUI.backgroundColor;

					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}

					//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width, 20), "");
					//GUI.backgroundColor = prevColor; 
					#endregion

					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 20, width - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);

					curGUIStyle = guiStyle_Selected;
				}


				EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
				GUILayout.Space(15);

				//이전
				//if (GUILayout.Button(new GUIContent(" " + _linkDataList[i]._name, _linkDataList[i]._icon), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))

				//변경
				_guiContent_LinkDataNameAndIcon.ClearText(false);
				_guiContent_LinkDataNameAndIcon.AppendSpaceText(1, false);
				_guiContent_LinkDataNameAndIcon.AppendText(_linkDataList[i]._name, true);

				_guiContent_LinkDataNameAndIcon.SetImage(_linkDataList[i]._icon);

				if (GUILayout.Button(_guiContent_LinkDataNameAndIcon.Content, curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))
				{
					_selectedLinkData = _linkDataList[i];
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();


			bool isClose = false;
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Select), GUILayout.Height(30)))//"Select"
			{
				if (_selectedLinkData != null)
				{
					_funcResult(true, _loadKey, _selectedLinkData._linkType, _selectedLinkData._modifierID, _targetAnimClip);
				}
				else
				{
					_funcResult(false, _loadKey, apAnimClip.LINK_TYPE.AnimatedModifier, -1, null);
				}
				isClose = true;
			}
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))//"Close"
			{
				_funcResult(false, _loadKey, apAnimClip.LINK_TYPE.AnimatedModifier, -1, null);
				isClose = true;
			}
			EditorGUILayout.EndHorizontal();

			if (isClose)
			{
				CloseDialog();
			}
		}
	}

}