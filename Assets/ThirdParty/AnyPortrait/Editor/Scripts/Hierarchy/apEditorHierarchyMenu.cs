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
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// Hierarchy에서 우클릭을 할 때 나오는 메뉴.
	/// 콜백과 파라미터 등이 많아서 기능의 일부를 대행한다.
	/// Hierachy마다 멤버로서 존재한다.
	/// </summary>
	public class apEditorHierarchyMenu
	{
		// Members
		//---------------------------------------------
		private apEditor _editor;

		//- 검색
		//- 이름 변경
		//- (위치 변경)
		//- 복제
		//- 삭제 등
		//이 중에서 필요한 걸 선택해서 메뉴 호출을 요청하자
		[Flags]
		public enum MENU_ITEM_HIERARCHY : int
		{
			None = 0,			
			Rename = 1,
			MoveUp = 2,
			MoveDown = 4,
			Search = 8,
			SelectAll = 16,
			Duplicate = 32,
			Remove = 64,
			RemoveMultiple = 128,//이건 좌측 UI에서만 나온다. (다른건 다중 선택을 지원하므로)
			Edit = 256,//<<추가 22.7.13 : 바로 편집화면으로 이동하기 (메시 그룹 하이라키에서 메시만 대상으로 한다.)
		}

		

		

		private int _hierarchyUnitType = 0;
		private object _requestedObj = null;
		private apEditorHierarchyUnit _clickedUnit = null;

		//public class MenuCallbackParam
		//{
		//	public MENU_ITEM_HIERARCHY _selectedMenu = MENU_ITEM_HIERARCHY.None;
		//	public object _requestedObj = 
		//}

		public delegate void FUNC_MENU_SELECTED(MENU_ITEM_HIERARCHY menuType, int hierachyUnitType, object requestedObj, apEditorHierarchyUnit clickedUnit);
		private FUNC_MENU_SELECTED _funcMenuSelected = null;
		private const string STR_EMPTY = "";

		private const int MAX_TITLE_LENGTH = 25;
		private apStringWrapper _strWrapper = null;


		private bool _isMenu_Rename = false;
		private bool _isMenu_MoveUpDown = false;
		private bool _isMenu_Search = false;
		private bool _isMenu_SelectAll = false;
		private bool _isMenu_Duplicate = false;
		private bool _isMenu_Remove = false;
		private bool _isMenu_RemoveMultiple = false;
		private bool _isMenu_Edit = false;


		




		// Init
		//---------------------------------------------
		public apEditorHierarchyMenu(apEditor editor, FUNC_MENU_SELECTED funcMenuSelected)
		{
			_editor = editor;
			_funcMenuSelected = funcMenuSelected;
			_strWrapper = new apStringWrapper(128);
		}


		// Functions
		//---------------------------------------------
		//메뉴 만들기
		public void ReadyToMakeMenu()
		{
			_isMenu_Rename = false;
			_isMenu_MoveUpDown = false;
			_isMenu_Search = false;
			_isMenu_SelectAll = false;
			_isMenu_Duplicate = false;
			_isMenu_Remove = false;
			_isMenu_RemoveMultiple = false;
			_isMenu_Edit = false;
		}

		public void SetMenu_Rename() { _isMenu_Rename = true; }
		public void SetMenu_MoveUpDown() { _isMenu_MoveUpDown = true; }
		public void SetMenu_Search() { _isMenu_Search = true; }
		public void SetMenu_SelectAll() { _isMenu_SelectAll = true; }
		public void SetMenu_Duplicate() { _isMenu_Duplicate = true; }
		public void SetMenu_Remove() { _isMenu_Remove = true; }
		public void SetMenu_RemoveMultiple() { _isMenu_RemoveMultiple = true; }
		public void SetMenu_Edit() { _isMenu_Edit = true; }



		//메뉴 보여주기
		public void ShowMenu(string title, int numSelectedObjects, int hierachyUnitType, object requestObj, apEditorHierarchyUnit clickedUnit)
		{
			_hierarchyUnitType = hierachyUnitType;
			_requestedObj = requestObj;
			_clickedUnit = clickedUnit;

			GenericMenu newMenu = new GenericMenu();

			//타이틀을 추가하자
			if(!string.IsNullOrEmpty(title))
			{
				//타이틀 양식은
				//1개 이하 선택시 > "타이틀"
				//2개 이상 선택시 > "타이틀" + (개수 - 1)

				//길이가 길다면
				//자르고 ...추가

				if(_strWrapper == null)
				{
					_strWrapper = new apStringWrapper(128);
				}
				_strWrapper.Clear();

				//글자수 체크
				if(title.Length > MAX_TITLE_LENGTH)
				{
					_strWrapper.Append(title.Substring(0, MAX_TITLE_LENGTH), false);
					_strWrapper.Append("..", false);
				}
				else
				{
					_strWrapper.Append(title, false);
				}

				//복수개를 선택했다면
				if(numSelectedObjects > 1)
				{
					_strWrapper.Append(" (+", false);
					_strWrapper.Append(numSelectedObjects - 1, false);
					_strWrapper.Append(")", false);
				}
				_strWrapper.MakeString();

				newMenu.AddDisabledItem(new GUIContent(_strWrapper.ToString()));
				newMenu.AddSeparator(STR_EMPTY);
			}


			MENU_ITEM_HIERARCHY lastMenu = MENU_ITEM_HIERARCHY.None;



			//이름
			//if((int)(menus & MENU_ITEM_HIERARCHY.Rename) != 0)
			if(_isMenu_Rename)
			{
				//검색
				//"Rename"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Rename)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Rename);
				lastMenu = MENU_ITEM_HIERARCHY.Rename;
			}

			//이동 (Up, Down)
			//if ((int)(menus & MENU_ITEM_HIERARCHY.MoveUp) != 0)
			if(_isMenu_MoveUpDown)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Move Up"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.MoveUp)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.MoveUp);
				lastMenu = MENU_ITEM_HIERARCHY.MoveUp;
			

				if(lastMenu != MENU_ITEM_HIERARCHY.None
					&& lastMenu != MENU_ITEM_HIERARCHY.MoveUp)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Move Down"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.MoveDown)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.MoveDown);
				lastMenu = MENU_ITEM_HIERARCHY.MoveDown;
			}

			//검색
			//if((int)(menus & MENU_ITEM_HIERARCHY.Search) != 0)
			if(_isMenu_Search)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Search"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Search)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Search);
				lastMenu = MENU_ITEM_HIERARCHY.Search;
			}
			
			//모두 선택
			//if((int)(menus & MENU_ITEM_HIERARCHY.SelectAll) != 0)
			if(_isMenu_SelectAll)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None
					&& lastMenu != MENU_ITEM_HIERARCHY.Search)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Select All"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.SelectAll)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.SelectAll);
				lastMenu = MENU_ITEM_HIERARCHY.SelectAll;
			}
			

			//복제
			//if((int)(menus & MENU_ITEM_HIERARCHY.Duplicate) != 0)
			if(_isMenu_Duplicate)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Duplicate"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Duplicate)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Duplicate);
				lastMenu = MENU_ITEM_HIERARCHY.Duplicate;
			}

			//편집하기 (추가 22.7.13)
			//if((int)(menus & MENU_ITEM_HIERARCHY.Edit) != 0)
			if(_isMenu_Edit)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Duplicate"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Modify)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Edit);
				lastMenu = MENU_ITEM_HIERARCHY.Edit;
			}

			//삭제
			//if((int)(menus & MENU_ITEM_HIERARCHY.Remove) != 0)
			if(_isMenu_Remove)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				//"Remove"
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.Remove)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.Remove);
				lastMenu = MENU_ITEM_HIERARCHY.Remove;
			}

			//추가 21.10.8 : 여러개 삭제
			//if((int)(menus & MENU_ITEM_HIERARCHY.RemoveMultiple) != 0)
			if(_isMenu_RemoveMultiple)
			{
				if(lastMenu != MENU_ITEM_HIERARCHY.None
					&& lastMenu != MENU_ITEM_HIERARCHY.Remove)
				{
					newMenu.AddSeparator(STR_EMPTY);
				}
				
				newMenu.AddItem(new GUIContent(_editor.GetUIWord(UIWORD.GUIMenu_RemoveMultiple)), false, OnMenuSelected, MENU_ITEM_HIERARCHY.RemoveMultiple);
				lastMenu = MENU_ITEM_HIERARCHY.RemoveMultiple;
			}


			newMenu.ShowAsContext();
		}

		private void OnMenuSelected(object obj)
		{
			if(_requestedObj == null
				|| _funcMenuSelected == null)
			{
				return;
			}
			if(!(obj is MENU_ITEM_HIERARCHY))
			{
				return;
			}

			MENU_ITEM_HIERARCHY menuType = (MENU_ITEM_HIERARCHY)obj;
			
			_funcMenuSelected(menuType, _hierarchyUnitType, _requestedObj, _clickedUnit);

		}
	}
}