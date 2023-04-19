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
	public class apDialog_AnimationEvents : EditorWindow
	{
		// Members
		//--------------------------------------------------------------
		private static apDialog_AnimationEvents s_window = null;
		
		private apEditor _editor = null;
		private apPortrait _portrait = null;
		private apAnimClip _animClip = null;
		
		private Vector2 _scrollList_EventList = new Vector2();
		private Vector2 _scrollList_Param = new Vector2();
		private Vector2 _scrollList_Preset = new Vector2();
		private Vector2 _scrollList_PresetParams = new Vector2();
		private apAnimEvent _curSelectedEvent = null;

		//[v1.4.1] 현재 이벤트 이름의 유효성 상태
		//이벤트를 선택하거나 이름을 변경할 때, 파라미터를 추가/삭제할 때 확인한다. (Undo 포함)
		private enum EVENT_NAME_VALIDATION
		{
			/// <summary>검사하지 않았다.</summary>
			NotChecked,
			/// <summary>유효하다. 경고 메시지가 나타나지 않는다.</summary>
			Valid,
			/// <summary>공백이다.</summary>
			Empty,
			/// <summary>유효하지 않은 글자가 포함되어 있다.</summary>
			InvalidWord,
			/// <summary>동일한 이름의 다른 파라미터를 가진 다른 이벤트가 있다.</summary>
			Overloading
		}
		private Dictionary<apAnimEvent, EVENT_NAME_VALIDATION> _event2Validation = null;

		private int _prevNumSubParams = -1;
		private apGUIContentWrapper _guiContent_AnimClipName = null;

		//삭제 : 22.6.11 : 애니메이션의 길이/루프 정보는 더이상 필요없다. 다 보고 있는데 뭐 =3=
		//private apGUIContentWrapper _guiContent_Range = null;
		//private apGUIContentWrapper _guiContent_IsLoop = null;
		private apGUIContentWrapper _guiContent_AnimEventCategory = null;
		private apGUIContentWrapper _guiContent_AddParameter = null;

		private apGUIContentWrapper _guiContent_Presets = null;
		private apGUIContentWrapper _guiContent_PresetCategory = null;


		
		private Texture2D _img_Category = null;
		private Texture2D _img_AddParam = null;
		private Texture2D _img_LayerUp = null;
		private Texture2D _img_LayerDown = null;
		private Texture2D _img_Remove = null;

		
		private Texture2D _img_Event = null;
		private Texture2D _img_Presets = null;


		private GUIStyle _guiStyle_Box = null;
		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_Selected = null;
		private GUIStyle _guiStyle_Center = null;
		private GUIStyle _guiStyle_CenterLabel = null;
		private GUIStyle _guiStyle_Button_NoPadding = null;


		private apAnimEventPresetUnit _selectedPreset = null;

		// Show Window
		//--------------------------------------------------------------
		public static void ShowDialog(apEditor editor, apPortrait portrait, apAnimClip animClip)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return;
			}

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_AnimationEvents), true, "Animation Events", true);
			apDialog_AnimationEvents curTool = curWindow as apDialog_AnimationEvents;

			if (curTool != null && curTool != s_window)
			{
				int width = 800;//라인이 두개가 되면서 조금 더 커졌다.
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, portrait, animClip);
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
		public void Init(apEditor editor, apPortrait portrait, apAnimClip animClip)
		{
			_editor = editor;
			_portrait = portrait;
			_animClip = animClip;

			_curSelectedEvent = null;
			
			_img_Category = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			_img_AddParam = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Add);
			_img_LayerUp = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp);
			_img_LayerDown = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown);
			_img_Remove = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform);

			_img_Event = _editor.ImageSet.Get(apImageSet.PRESET.AnimEvent_MainIcon);
			_img_Presets = _editor.ImageSet.Get(apImageSet.PRESET.AnimEvent_Presets);

			_selectedPreset = null;

			//전체 이벤트의 유효성을 검사한다.
			ResetValidation();

			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		private void OnUndoRedoPerformed()
		{
			//전체 이벤트의 유효성을 검사한다.
			ResetValidation();

			Repaint();
		}


		void OnDestroy()
		{
			//Debug.Log("On Destroy");
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
		}


		// GUI
		//--------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _editor._portrait == null || _editor._portrait != _portrait)
			{
				CloseDialog();
				return;
			}

			bool isGUIEvent = (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout);
			
			if(_guiStyle_Box == null)
			{
				_guiStyle_Box = new GUIStyle(GUI.skin.box);
				_guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				_guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;
			}

			//변경
			if(_guiContent_AnimClipName == null)
			{
				_guiContent_AnimClipName = apGUIContentWrapper.Make(string.Format("  [ {0} ] {1}", _animClip._name, _editor.GetText(TEXT.DLG_AnimationEvents)), false, _img_Event);
			}

			if(_guiStyle_None == null)
			{
				_guiStyle_None = new GUIStyle(GUIStyle.none);
				_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
				_guiStyle_None.alignment = TextAnchor.MiddleLeft;
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

			if(_guiStyle_CenterLabel == null)
			{
				_guiStyle_CenterLabel = new GUIStyle(GUI.skin.label);
				_guiStyle_CenterLabel.alignment = TextAnchor.MiddleCenter;
			}

			
			if(_guiStyle_Button_NoPadding == null)
			{
				_guiStyle_Button_NoPadding = new GUIStyle(GUI.skin.button);
				_guiStyle_Button_NoPadding.margin = new RectOffset(1, 1, 0, 0);
			}

				


			//삭제 : 애니메이션의 정보는 더이상 출력하지 않는다.
			//if(_guiContent_Range == null)
			//{
			//	_guiContent_Range = apGUIContentWrapper.Make(string.Format("{0} : {1} ~ {2}", _editor.GetText(TEXT.DLG_Range), _animClip.StartFrame, _animClip.EndFrame), false);
			//}
			//if(_guiContent_IsLoop == null)
			//{
			//	_guiContent_IsLoop = apGUIContentWrapper.Make(string.Format("{0} : {1}", _editor.GetText(TEXT.DLG_IsLoopAnimation), _animClip.IsLoop), false);
			//}



			//2개의 열이 있다.
			//왼쪽 : 현재 애니메이션 이벤트의 이벤트들
			//오른쪽 : 저장된 애니메이션 프리셋들 (조금 작다.)
			int width_Right = 250;
			int width_Left = width - (width_Right + 50);
			
			int height_Top = 40;
			int height_Bottom = 50;
			int height_Main = height - (height_Top + height_Bottom + 10);
			

			// Top
			//-------------------------------------------------
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height_Top));

			GUILayout.Space(5);
			// 애니메이션 이벤트 타이틀
			GUILayout.Box(_guiContent_AnimClipName.Content, _guiStyle_Box, GUILayout.Width(width - 10), GUILayout.Height(35));

			//삭제 : 애니메이션의 정보는 출력하지 않는다.
			//GUILayout.Space(5);
			////"Range : " + _animClip.StartFrame + " ~ " + _animClip.EndFrame
			//EditorGUILayout.LabelField(_guiContent_Range.Content);

			////"Is Loop Animation : " + _animClip.IsLoop
			//EditorGUILayout.LabelField(_guiContent_IsLoop.Content);
			EditorGUILayout.EndVertical();
			//-------------------------------------------------
			
			GUILayout.Space(5);


			int mainPosY = 49;
			// Main (Left + Right)
			//-------------------------------------------------
			//왼쪽과 오른쪽을 구분하여 보여준다.
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Main));
			
			GUILayout.Space(5);


			//왼쪽 GUI : 현재 애니메이션에 등록된 애니메이션 이벤트들을 보여준다.
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height_Main));

			GUI_Left(width_Left, height_Main, 5, mainPosY, isGUIEvent);

			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxV(height_Main);
			GUILayout.Space(10);

			//오른쪽 GUI : 애니메이션 이벤트 프리셋들을 보여준다.
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height_Main));

			GUI_Right(width_Right, height_Main, width_Left + 35, mainPosY, isGUIEvent);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			
			
			GUILayout.Space(10);

			// Bottom
			//-------------------------------------------------
			//화면 하단 Close 버튼

			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(height_Bottom));

			bool isClose = false;
			
			//"Close"
			if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Height(30)))
			{
				isClose = true;
				apEditorUtil.ReleaseGUIFocus();
			}
			
			EditorGUILayout.EndVertical();
			

			if (isClose)
			{
				CloseDialog();
			}
		}



		//왼쪽 GUI : 현재의 애니메이션에 등록된 애니메이션 이벤트들을 보여준다.
		private void GUI_Left(int width, int height, int posX, int posY, bool isGUIEvent)
		{
			
			
			int height_2_ItemButtons = 30;
			int height_3_Properties = 180;
			int height_4_ParameterList = 105;//< 여기에 프리셋 버튼들을 추가해야한다.
			int height_5_RemoveEvent = 35;//이벤트 삭제 버튼 (여백 포함)
			int height_1_List = height - (height_2_ItemButtons + height_3_Properties + height_4_ParameterList + 102 + height_5_RemoveEvent);



			//"Animation Events"
			if(_guiContent_AnimEventCategory == null)
			{
				_guiContent_AnimEventCategory = apGUIContentWrapper.Make(_editor.GetText(TEXT.DLG_AnimationEvents), false, _img_Category);
			}

			

			int nAnimEvents = _animClip._animEvents != null ? _animClip._animEvents.Count : 0;


			Color prevColor = GUI.backgroundColor;

			// [ 위쪽 : 이벤트 리스트 ]

			//리스트 배경
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(posX, posY, width, height_1_List), "");
			GUI.backgroundColor = prevColor;

			int width_ScrollListItem = width - 30;
			
			//------------------------------------
			//1. 이벤트 리스트
			//------------------------------------
			_scrollList_EventList = EditorGUILayout.BeginScrollView(_scrollList_EventList, GUILayout.Width(width), GUILayout.Height(height_1_List));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(width_ScrollListItem));

			GUILayout.Button(_guiContent_AnimEventCategory.Content, _guiStyle_None, GUILayout.Height(20));//<투명 버튼//

			//이벤트 리스트 항목을 하나씩 출력하자

			//출력할 정보
			//아이콘 색상, 프레임 (Start 기준) , 이벤트 이름 | 파라미터 (타입:값, 타입:값.. 반복)
			int width_Info_Icon = 30;
			int width_Info_Frame = 60;
			int width_Info_EventName = (width_ScrollListItem - (width_Info_Icon + width_Info_Frame + 20));
			

			apAnimEvent animEvent = null;
			apStringWrapper strParams = new apStringWrapper(500);
			for (int i = 0; i < nAnimEvents; i++)
			{
				GUIStyle curGUIStyle = _guiStyle_None;
				animEvent = _animClip._animEvents[i];

				bool isSelect = false;

				if (animEvent == _curSelectedEvent)
				{
					Rect lastRect = GUILayoutUtility.GetLastRect();

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

					//GUI.Box(new Rect(lastRect.x, lastRect.y + 21, width_ScrollListItem + 16, 20), "");
					//GUI.backgroundColor = prevColor; 
					#endregion

					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 21, width_ScrollListItem + 16 - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);

					curGUIStyle = _guiStyle_Selected;
				}


				EditorGUILayout.BeginHorizontal(GUILayout.Width(width_ScrollListItem));
				GUILayout.Space(10);

				//이벤트의 내용을 간단히 보여준다.
				//TODO : 파라미터 등을 더 보여주어야 한다.

				//1. 아이콘
				GUI.backgroundColor = animEvent.GetIconColor();
				GUILayout.Box(apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box, apGUILOFactory.I.Width(14), apGUILOFactory.I.Height(14));//일반 박스 이미지
				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);

				//2. 프레임
				if (animEvent._callType == apAnimEvent.CALL_TYPE.Once)
				{
					if(GUILayout.Button(animEvent._frameIndex.ToString(), curGUIStyle, GUILayout.Width(width_Info_Frame), GUILayout.Height(20)))
					{
						isSelect = true;
					}
				}
				else
				{
					if(GUILayout.Button(animEvent._frameIndex + " ~ " + animEvent._frameIndex_End, curGUIStyle, GUILayout.Width(width_Info_Frame), GUILayout.Height(20)))
					{
						isSelect = true;
					}
				}


				//4. 이벤트 이름 + 파라미터
				strParams.Clear();

				strParams.Append(animEvent._eventName, false);

				int nCurSubParams = animEvent._subParams != null ? animEvent._subParams.Count : 0;
				if(nCurSubParams > 0)
				{
					strParams.Append("  ( ", false);

					apAnimEvent.SubParameter curSubParam = null;
					for (int iSubParam = 0; iSubParam < nCurSubParams; iSubParam++)
					{
						curSubParam = animEvent._subParams[iSubParam];
						switch (curSubParam._paramType)
						{
							case apAnimEvent.PARAM_TYPE.Bool:		strParams.Append("bool", false);	break;
							case apAnimEvent.PARAM_TYPE.Integer:	strParams.Append("int", false);		break;
							case apAnimEvent.PARAM_TYPE.Float:		strParams.Append("float", false);	break;
							case apAnimEvent.PARAM_TYPE.Vector2:	strParams.Append("Vector2", false);	break;
							case apAnimEvent.PARAM_TYPE.String:		strParams.Append("string", false);	break;
						}
						if(iSubParam < nCurSubParams - 1)
						{
							//여백
							strParams.Append(", ", false);
						}
					}

					strParams.Append(" )", false);
				}
				strParams.MakeString();

				if (GUILayout.Button(strParams.ToString(), curGUIStyle, GUILayout.Width(width_Info_EventName), GUILayout.Height(20)))
				{
					isSelect = true;
				}

				EditorGUILayout.EndHorizontal();

				if(isSelect)
				{
					_curSelectedEvent = animEvent;

					//유효성 체크를 하자
					ValidateEvent(_curSelectedEvent, false);


					apEditorUtil.ReleaseGUIFocus();
				}
			}

			GUILayout.Space(height_1_List + 150);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();


			GUILayout.Space(10);

			//------------------------------------
			// 2. 리스트 하단의 버튼들
			//------------------------------------
			EditorGUILayout.BeginHorizontal(GUILayout.Height(height_2_ItemButtons));
			GUILayout.Space(5);

			// (1) 이벤트 추가하기 버튼
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_AddEvent), GUILayout.Width(width - (10 + 6 + 160)), GUILayout.Height(30)))
			{
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_AddEvent, 
													_editor, 
													_animClip._portrait, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				if(_animClip._animEvents == null)
				{
					_animClip._animEvents = new List<apAnimEvent>();
				}

				apAnimEvent newEvent = new apAnimEvent();
				//새로운 이름을 찾자
				int iNewName = 0;
				string newName = "NewAnimEvent_" + iNewName;

				int cnt = 0;
				while(true)
				{
					if(cnt > 500)
					{
						newName = "NewAnimEvent_Infinity";
						break;
					}
					//중복되는 이름이 있는가
					newName = "NewAnimEvent_" + iNewName;
					bool isExist = _animClip._animEvents.Exists(delegate (apAnimEvent a)
					{
						return string.Equals(a._eventName, newName);
					});
					if(!isExist)
					{
						//중복되지 않는 이름이다.
						break;
					}

					//이름이 중복되는 군염
					cnt++;
					iNewName++;
				}

				newEvent._eventName = newName;

				int curFrame = _animClip.CurFrame;

				newEvent._frameIndex = curFrame;
				newEvent._frameIndex_End = curFrame;

				_animClip._animEvents.Add(newEvent);

				_curSelectedEvent = newEvent;

				//유효성 체크를 하자
				ValidateEvent(_curSelectedEvent, true);
			}

			//추가 3.29 : 이벤트 복사하기 : 프레임은 무조건 다음 프레임에
			string strCopy = _editor.GetUIWord(UIWORD.Copy);
			//if(GUILayout.Button(_editor.GetUIWord(UIWORD.Copy), GUILayout.Width(80), GUILayout.Height(30)))
			if(apEditorUtil.ToggledButton_2Side(strCopy, strCopy, false, _curSelectedEvent != null, 80, 30))
			{
				if (_curSelectedEvent != null)
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_AddEvent, 
														_editor, 
														_animClip._portrait, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					if (_animClip._animEvents == null)
					{
						_animClip._animEvents = new List<apAnimEvent>();
					}

					apAnimEvent newEvent = new apAnimEvent();
					
					//선택한 이벤트와 동일한 속성으로 설정.
					//- _defaultFrame과 다르다면, 거기에 복사
					//- _defaultFrame과 같다면 +1프레임
					newEvent._eventName = _curSelectedEvent._eventName;
					newEvent._callType = _curSelectedEvent._callType;
					
					//[v1.4.1] 마커 색상도 복사해야한다.
					newEvent._iconColor = _curSelectedEvent._iconColor;

					int curFrame = _animClip.CurFrame;

					if(newEvent._callType == apAnimEvent.CALL_TYPE.Once)
					{
						if(_curSelectedEvent._frameIndex == curFrame)
						{
							newEvent._frameIndex = _curSelectedEvent._frameIndex + 1;
							newEvent._frameIndex_End = _curSelectedEvent._frameIndex_End + 1;
						}
						else
						{
							int frameLength = Mathf.Max(_curSelectedEvent._frameIndex_End - _curSelectedEvent._frameIndex, 0);

							newEvent._frameIndex = curFrame;
							newEvent._frameIndex_End = newEvent._frameIndex + frameLength;
						}
						
					}
					else
					{
						int frameLength = Mathf.Max(_curSelectedEvent._frameIndex_End - _curSelectedEvent._frameIndex, 0);

						if (_curSelectedEvent._frameIndex <= curFrame && curFrame <= _curSelectedEvent._frameIndex + frameLength)
						{
							//DefaultFrame이 기존 이벤트 영역에 포함되어 있다.
							//기존 영역의 밖에서 생성
							newEvent._frameIndex = _curSelectedEvent._frameIndex + frameLength + 1;
							newEvent._frameIndex_End = newEvent._frameIndex + frameLength;
						}
						else
						{
							//DefaultFrame이 기존 이벤트 영역 밖에 있다.
							//DefaultFrame부터 생성
							newEvent._frameIndex = curFrame;
							newEvent._frameIndex_End = newEvent._frameIndex + frameLength;
						}

						
					}

					if(_curSelectedEvent._subParams == null)
					{
						_curSelectedEvent._subParams = new List<apAnimEvent.SubParameter>();
					}
					if(_curSelectedEvent._subParams != null)
					{
						for (int iParam = 0; iParam < _curSelectedEvent._subParams.Count; iParam++)
						{
							apAnimEvent.SubParameter existParam = _curSelectedEvent._subParams[iParam];
							apAnimEvent.SubParameter newParam = new apAnimEvent.SubParameter();

							//속성들 복사
							newParam._paramType =		existParam._paramType;
							newParam._boolValue =		existParam._boolValue;
							newParam._intValue =		existParam._intValue;
							newParam._floatValue =		existParam._floatValue;
							newParam._vec2Value =		existParam._vec2Value;
							newParam._strValue =		existParam._strValue;
							newParam._intValue_End =	existParam._intValue_End;
							newParam._floatValue_End =	existParam._floatValue_End;
							newParam._vec2Value_End =	existParam._vec2Value_End;

							newEvent._subParams.Add(newParam);
						}
					}
					

					_animClip._animEvents.Add(newEvent);

					_curSelectedEvent = newEvent;

					//유효성 체크를 하자
					ValidateEvent(_curSelectedEvent, true);
				}
			}

			//"Sort"
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Sort), GUILayout.Width(80), GUILayout.Height(30)))
			{
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_SortEvents, 
													_editor, 
													_animClip._portrait, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				//프레임 순으로 정렬을 한다.
				if (_animClip._animEvents != null)
				{
					_animClip._animEvents.Sort(delegate (apAnimEvent a, apAnimEvent b)
					{
						if(a._frameIndex == b._frameIndex)
						{
							return string.Compare(a._eventName, b._eventName);
						}
						return a._frameIndex - b._frameIndex;
					});

					apEditorUtil.SetEditorDirty();
				}
			}
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width - 10);
			GUILayout.Space(5);


			//------------------------------------
			// 3. 기본 속성들
			//------------------------------------
			// [ 아래쪽 : 선택된 이벤트에 대한 설정 ]

			//선택된 AnimEvent에 대한 설정을 하자
			//+ 프리셋 관련 설정도 해야한다.
			
			
			//선택이 안되었다면 더미 데이터로 채워야함
			int prevFrameIndex = 0;
			int prevFrameIndex_End = 0;
			string prevEventName = "<None>";
			apAnimEvent.CALL_TYPE prevCallType = apAnimEvent.CALL_TYPE.Once;
			apAnimEvent.ICON_COLOR prevIconColor = apAnimEvent.ICON_COLOR.Yellow;

			List<apAnimEvent.SubParameter> curSubParams = null;
			int curNumSubParams = 0;


			bool isSelected = _curSelectedEvent != null && _animClip._animEvents.Contains(_curSelectedEvent);

			if (isSelected)
			{
				prevFrameIndex = _curSelectedEvent._frameIndex;
				prevFrameIndex_End = _curSelectedEvent._frameIndex_End;
				prevEventName = _curSelectedEvent._eventName;
				prevCallType = _curSelectedEvent._callType;
				prevIconColor = _curSelectedEvent._iconColor;
				curSubParams = _curSelectedEvent._subParams;
				curNumSubParams = curSubParams.Count;
			}

			
			
			if(isSelected)
			{
				GUI.backgroundColor = new Color(0.6f, 0.8f, 0.9f, 1.0f);
			}
			
			//"(Not Selected)"
			GUILayout.Box((isSelected) ? prevEventName : "(" + _editor.GetText(TEXT.DLG_NotSelected) + ")", _guiStyle_Box, GUILayout.Width(width - 10), GUILayout.Height(25));

			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);

			//선택된 이벤트 정보를 보여주자
			int height_3_Properties_Lower = 40;
			int height_3_Properties_Upper = height_3_Properties - (height_3_Properties_Lower + 4);

			int width_PropLabel = 200;
			int width_PropValue_1 = width - (10 + width_PropLabel + 2);
			int width_PropValue_2 = (width - (10 + width_PropLabel + 2)) / 2 - 14;
			
			//+, - 버튼이 있는 경우
			int width_PropMoveBtn = 30;
			int width_PropValue_1_WithBtn = width_PropValue_1 - (width_PropMoveBtn * 2 + 5);
			int width_PropValue_2_WithBtn = width_PropValue_2 - (width_PropMoveBtn * 2 + 6);
			
			int height_Prop = 20;
			int height_Btn = 20;
			
			//1. 기본 속성 (이름, 프레임, 호출 타입)
			EditorGUILayout.BeginVertical(GUILayout.Height(height_3_Properties_Upper));

			//선택되지 않았다면
			if(!isSelected) { GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f); }

			EditorGUI.BeginChangeCheck();//GUI 변경 이벤트 체크 시작

			//1. 이벤트 이름
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Prop));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_EventName), GUILayout.Width(width_PropLabel));
			string nextEventName = EditorGUILayout.DelayedTextField(prevEventName, GUILayout.Width(width_PropValue_1));//"Event(Function) Name"
			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				if (isSelected)
				{
					if(!string.Equals(_curSelectedEvent._eventName, nextEventName))
					{
						//이름 변경
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_EventChanged, 
															_editor, 
															_animClip._portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_curSelectedEvent._eventName = nextEventName;
					}

					//이름을 바꿨다면 유효성 검사를 다시 하자
					ValidateEvent(_curSelectedEvent, true);
				}
			}

			//유효성 검사 실패시 박스로 보여주자
			if (_curSelectedEvent != null)
			{
				EVENT_NAME_VALIDATION validStatus = EVENT_NAME_VALIDATION.Valid;
				if (_event2Validation != null && _event2Validation.ContainsKey(_curSelectedEvent))
				{
					validStatus = _event2Validation[_curSelectedEvent];
				}

				if (validStatus != EVENT_NAME_VALIDATION.NotChecked &&
					validStatus != EVENT_NAME_VALIDATION.Valid)
				{
					//유효하지 않다면 경고 메시지를 보여주자
					int width_FixBtn = 100;
					int width_ValidBox = width - (10 + width_FixBtn + 2);

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(27));
					GUILayout.Space(5);

					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					string strValidWarning = "";
					switch (validStatus)
					{
						case EVENT_NAME_VALIDATION.Empty:	//"이름이 설정되지 않았습니다."							
							strValidWarning = _editor.GetText(TEXT.AnimEventWarning_EmptyName);
							break;

						case EVENT_NAME_VALIDATION.InvalidWord://"유효하지 않은 글자가 포함되어 있습니다."
							strValidWarning = _editor.GetText(TEXT.AnimEventWarning_InvalidCharacter);
							break;

						case EVENT_NAME_VALIDATION.Overloading://"파라미터가 다른 동일한 이름의 다른 이벤트가 있습니다."
							strValidWarning = _editor.GetText(TEXT.AnimEventWarning_Overloaded);
							break;

						default://"알 수 없는 에러입니다."
							strValidWarning = _editor.GetText(TEXT.AnimEventWarning_Unknown);
							break;
					}
					GUILayout.Box(strValidWarning, _guiStyle_Box, GUILayout.Width(width_ValidBox), GUILayout.Height(25));
					GUI.backgroundColor = prevColor;

					//"Fix Now"
					if (GUILayout.Button(_editor.GetText(TEXT.FixNow), GUILayout.Width(width_FixBtn), GUILayout.Height(25)))
					{
						//고치기
						string fixedName = GetFixedEventName(_curSelectedEvent);
						if(!string.IsNullOrEmpty(fixedName))
						{
							//이름 변경
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_EventChanged, 
																_editor, 
																_animClip._portrait, 
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_curSelectedEvent._eventName = fixedName;
							apEditorUtil.ReleaseGUIFocus();

							//이름을 바꿨다면 유효성 검사를 다시 하자
							ValidateEvent(_curSelectedEvent, true);
						}
					}



					EditorGUILayout.EndHorizontal();
					GUILayout.Space(5);
				}
			}
			

			
			GUILayout.Space(2);


			EditorGUI.BeginChangeCheck();//GUI 변경 이벤트 체크 시작 2
			

			//2. 호출 방식 + 프레임
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Prop));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_CallMethod), GUILayout.Width(width_PropLabel));
			apAnimEvent.CALL_TYPE nextCallType = (apAnimEvent.CALL_TYPE)EditorGUILayout.EnumPopup(prevCallType, GUILayout.Width(width_PropValue_1));//"Call Method"
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Prop));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_TargetFrame), GUILayout.Width(width_PropLabel));
			
			int nextFrameIndex = prevFrameIndex;
			int nextFrameIndex_End = prevFrameIndex_End;
			if (prevCallType == apAnimEvent.CALL_TYPE.Once)
			{
				//단일 프레임
				nextFrameIndex = EditorGUILayout.DelayedIntField(prevFrameIndex, GUILayout.Width(width_PropValue_1_WithBtn));//"Target Frame"
				if(GUILayout.Button("◀", _guiStyle_Button_NoPadding, GUILayout.Width(width_PropMoveBtn), GUILayout.Height(height_Btn)))
				{
					nextFrameIndex = Mathf.Max(prevFrameIndex - 1, _animClip.StartFrame);
				}
				if(GUILayout.Button("▶", _guiStyle_Button_NoPadding, GUILayout.Width(width_PropMoveBtn), GUILayout.Height(height_Btn)))
				{
					nextFrameIndex = Mathf.Min(prevFrameIndex + 1, _animClip.EndFrame);
				}
			}
			else
			{
				//시작 - 끝 프레임
				nextFrameIndex = EditorGUILayout.DelayedIntField(prevFrameIndex, GUILayout.Width(width_PropValue_2_WithBtn));//"Start Frame"
				if(GUILayout.Button("◀", _guiStyle_Button_NoPadding, GUILayout.Width(width_PropMoveBtn), GUILayout.Height(height_Btn)))
				{
					nextFrameIndex = Mathf.Max(prevFrameIndex - 1, _animClip.StartFrame);
				}
				if(GUILayout.Button("▶", _guiStyle_Button_NoPadding, GUILayout.Width(width_PropMoveBtn), GUILayout.Height(height_Btn)))
				{
					nextFrameIndex = Mathf.Min(prevFrameIndex + 1, _animClip.EndFrame);
				}


				EditorGUILayout.LabelField("~", _guiStyle_CenterLabel, GUILayout.Width(22));

				nextFrameIndex_End = EditorGUILayout.DelayedIntField(prevFrameIndex_End, GUILayout.Width(width_PropValue_2_WithBtn));//"End Frame"
				if(GUILayout.Button("◀", _guiStyle_Button_NoPadding, GUILayout.Width(width_PropMoveBtn), GUILayout.Height(height_Btn)))
				{
					nextFrameIndex_End = Mathf.Max(prevFrameIndex_End - 1, _animClip.StartFrame);
				}
				if(GUILayout.Button("▶", _guiStyle_Button_NoPadding, GUILayout.Width(width_PropMoveBtn), GUILayout.Height(height_Btn)))
				{
					nextFrameIndex_End = Mathf.Min(prevFrameIndex_End + 1, _animClip.EndFrame);
				}
			}
			EditorGUILayout.EndHorizontal();

			//3. 아이콘 색상
			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height_Prop));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.MarkerColor), GUILayout.Width(width_PropLabel), GUILayout.Height(height_Prop));//"아이콘 색상"
			apAnimEvent.ICON_COLOR nextIconColor = (apAnimEvent.ICON_COLOR)EditorGUILayout.EnumPopup(prevIconColor, GUILayout.Width(width_PropValue_1));
			
			EditorGUILayout.EndHorizontal();


			//기본 설정이 바뀌었는지 체크한다.
			if(EditorGUI.EndChangeCheck())
			{
				if(isSelected)
				{
					if(	_curSelectedEvent._frameIndex != nextFrameIndex
						|| _curSelectedEvent._frameIndex_End != nextFrameIndex_End
						|| _curSelectedEvent._callType != nextCallType
						|| _curSelectedEvent._iconColor != nextIconColor)
					{

						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_EventChanged, 
															_editor, 
															_animClip._portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_curSelectedEvent._frameIndex = nextFrameIndex;
						_curSelectedEvent._frameIndex_End = nextFrameIndex_End;
						_curSelectedEvent._callType = nextCallType;
						_curSelectedEvent._iconColor = nextIconColor;
					}

				}

				apEditorUtil.ReleaseGUIFocus();
			}

			GUI.backgroundColor = prevColor;


			EditorGUILayout.EndVertical();

			//프리셋 관련 버튼을 보여주자 (오른쪽에 정렬)
			//- Save As Preset
			//- Load from Selected Preset
			int width_PresetBtn = 250;
			int width_PresetBtnMargin = width - (10 + width_PresetBtn);
			int height_PresetBtn = 25;

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height_3_Properties_Lower));
			GUILayout.Space(width_PresetBtnMargin);

			// (1) 프리셋으로 저장하기 버튼
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.SaveAsPreset), false, _curSelectedEvent != null, width_PresetBtn, height_PresetBtn))
			{
				apAnimEventPresetUnit newPresetUnit = _editor.AnimEventPreset.AddEventAsPreset(_curSelectedEvent);
				if(newPresetUnit != null)
				{
					_selectedPreset = newPresetUnit;
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(5);



			//2. 파라미터들
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Parameters));//"Parameters"
			GUILayout.Space(5);


			if (isSelected)
			{
				//이벤트가 선택되었다면
				GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			}
			else
			{
				//이벤트가 선택되지 않았다면
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
			}
			GUI.Box(new Rect(posX, posY + height_1_List + height_2_ItemButtons + height_3_Properties + 99, width, height_4_ParameterList), "");
			GUI.backgroundColor = prevColor;

			if (!isSelected)
			{
				if(!isGUIEvent)
				{
					curNumSubParams = _prevNumSubParams;
				}
			}

			//파라미터들을 보여주자
			_scrollList_Param = EditorGUILayout.BeginScrollView(_scrollList_Param, GUILayout.Width(width), GUILayout.Height(height_4_ParameterList));

			EditorGUILayout.BeginVertical(GUILayout.Width(width_ScrollListItem));


			GUILayout.Space(5);
			int valueWidth_Single = width - (10 + 35 + 130 + 36 + 36 + 36 + 20);
			int valueWidth_Range = (valueWidth_Single / 2) - 10;

			//Vector의 경우는 다르다. 두칸 혹은 네칸 필요
			int valueWidth_Vector_Single = (valueWidth_Single / 2) + 2;
			int valueWidth_Vector_Range = (valueWidth_Vector_Single / 2) - 8;



			int midWaveWidth = 20;
			valueWidth_Single += 7;

			GUIStyle guiStyleListBtn = new GUIStyle(GUI.skin.button);
			guiStyleListBtn.margin = GUI.skin.textField.margin;

			apAnimEvent.SubParameter targetSubParam = null;
			bool isLayerUp = false;
			bool isLayerDown = false;
			bool isRemoveParam = false;

			//SubParam 리스트를 출력하자
			if(curNumSubParams > 0)
			{
				if (isSelected && curSubParams != null && curSubParams.Count == curNumSubParams)
				{
					apAnimEvent.SubParameter curSubParam = null;

					for (int i = 0; i < curNumSubParams; i++)
					{
						curSubParam = curSubParams[i];

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50), GUILayout.Height(24));
						GUILayout.Space(15);
						GUILayout.Label("[" + i + "]", GUILayout.Width(30), GUILayout.Height(20));

						apAnimEvent.PARAM_TYPE nextParamType = (apAnimEvent.PARAM_TYPE)EditorGUILayout.EnumPopup(curSubParam._paramType, GUILayout.Width(120), GUILayout.Height(20));


						// [v1.4.1] 타입 바꾸면 Undo에 등록해야한다.
						if (nextParamType != curSubParam._paramType)
						{
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																				_editor,
																				_animClip._portrait,
																				//null, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);

							curSubParam._paramType = nextParamType;

							//타입 바꾸면 유효성 검사를 다시 해야한다.
							ValidateEvent(_curSelectedEvent, true);
						}



						switch (curSubParam._paramType)
						{
							case apAnimEvent.PARAM_TYPE.Bool:
								{	
									// Bool 파라미터
									EditorGUI.BeginChangeCheck();
									bool nextValue = EditorGUILayout.Toggle(curSubParam._boolValue, GUILayout.Width(valueWidth_Single));
									if (EditorGUI.EndChangeCheck())
									{
										if (curSubParam._boolValue != nextValue)
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																				_editor,
																				_animClip._portrait,
																				//null, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);

											curSubParam._boolValue = nextValue;
										}

										apEditorUtil.ReleaseGUIFocus();
									}
								}
								break;

							case apAnimEvent.PARAM_TYPE.Integer:
								{
									// Int 파라미터 [Once / Continuous]
									if (prevCallType == apAnimEvent.CALL_TYPE.Once)
									{
										EditorGUI.BeginChangeCheck();
										int nextValue = EditorGUILayout.DelayedIntField(curSubParam._intValue, GUILayout.Width(valueWidth_Single));
										if (EditorGUI.EndChangeCheck())
										{
											if (curSubParam._intValue != nextValue)
											{
												apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																					_editor,
																					_animClip._portrait,
																					//null, 
																					false,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												curSubParam._intValue = nextValue;
											}
											apEditorUtil.ReleaseGUIFocus();
										}
									}
									else
									{
										EditorGUI.BeginChangeCheck();
										int nextValue_Prev = EditorGUILayout.DelayedIntField(curSubParam._intValue, GUILayout.Width(valueWidth_Range));
										EditorGUILayout.LabelField("~", _guiStyle_CenterLabel, GUILayout.Width(midWaveWidth));
										int nextValue_Next = EditorGUILayout.DelayedIntField(curSubParam._intValue_End, GUILayout.Width(valueWidth_Range));

										if (EditorGUI.EndChangeCheck())
										{
											if (curSubParam._intValue != nextValue_Prev
												|| curSubParam._intValue_End != nextValue_Next)
											{
												apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																					_editor,
																					_animClip._portrait,
																					//null, 
																					false,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												curSubParam._intValue = nextValue_Prev;
												curSubParam._intValue_End = nextValue_Next;
											}

											apEditorUtil.ReleaseGUIFocus();
										}

										
									}
								}
								break;

							case apAnimEvent.PARAM_TYPE.Float:
								{
									// Float 파라미터 [Once / Continuous]
									if (prevCallType == apAnimEvent.CALL_TYPE.Once)
									{
										EditorGUI.BeginChangeCheck();
										float nextValue = EditorGUILayout.DelayedFloatField(curSubParam._floatValue, GUILayout.Width(valueWidth_Single));
										if (EditorGUI.EndChangeCheck())
										{
											if (curSubParam._floatValue != nextValue)
											{
												apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																					_editor,
																					_animClip._portrait,
																					//null, 
																					false,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												curSubParam._floatValue = nextValue;
											}
											apEditorUtil.ReleaseGUIFocus();
										}
										
									}
									else
									{
										EditorGUI.BeginChangeCheck();
										float nextValue_Prev = EditorGUILayout.DelayedFloatField(curSubParam._floatValue, GUILayout.Width(valueWidth_Range));
										EditorGUILayout.LabelField("~", _guiStyle_CenterLabel, GUILayout.Width(midWaveWidth));
										float nextVelue_Next = EditorGUILayout.DelayedFloatField(curSubParam._floatValue_End, GUILayout.Width(valueWidth_Range));

										if (EditorGUI.EndChangeCheck())
										{
											if (curSubParam._floatValue != nextValue_Prev
												|| curSubParam._floatValue_End != nextVelue_Next)
											{
												apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																					_editor,
																					_animClip._portrait,
																					//null, 
																					false,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												curSubParam._floatValue = nextValue_Prev;
												curSubParam._floatValue_End = nextVelue_Next;
											}

											apEditorUtil.ReleaseGUIFocus();
										}
									}
								}
								break;

							case apAnimEvent.PARAM_TYPE.Vector2:
								{
									// Vector 파라미터 [Once / Continuous]
									if (prevCallType == apAnimEvent.CALL_TYPE.Once)
									{
										EditorGUI.BeginChangeCheck();
										float nextValue_X = EditorGUILayout.DelayedFloatField(curSubParam._vec2Value.x, GUILayout.Width(valueWidth_Vector_Single));
										float nextValue_Y = EditorGUILayout.DelayedFloatField(curSubParam._vec2Value.y, GUILayout.Width(valueWidth_Vector_Single));
										if (EditorGUI.EndChangeCheck())
										{
											if (curSubParam._vec2Value.x != nextValue_X
												|| curSubParam._vec2Value.y != nextValue_Y)
											{
												apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																					_editor,
																					_animClip._portrait,
																					//null, 
																					false,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												curSubParam._vec2Value.x = nextValue_X;
												curSubParam._vec2Value.y = nextValue_Y;
											}

											apEditorUtil.ReleaseGUIFocus();
										}
									}
									else
									{
										EditorGUI.BeginChangeCheck();

										float nextValue_Prev_X = EditorGUILayout.DelayedFloatField(curSubParam._vec2Value.x, GUILayout.Width(valueWidth_Vector_Range));
										float nextValue_Prev_Y = EditorGUILayout.DelayedFloatField(curSubParam._vec2Value.y, GUILayout.Width(valueWidth_Vector_Range));
										EditorGUILayout.LabelField("~", _guiStyle_CenterLabel, GUILayout.Width(midWaveWidth));
										float nextValue_Next_X = EditorGUILayout.DelayedFloatField(curSubParam._vec2Value_End.x, GUILayout.Width(valueWidth_Vector_Range));
										float nextValue_Next_Y = EditorGUILayout.DelayedFloatField(curSubParam._vec2Value_End.y, GUILayout.Width(valueWidth_Vector_Range));

										if (EditorGUI.EndChangeCheck())
										{
											if (curSubParam._vec2Value.x != nextValue_Prev_X
												|| curSubParam._vec2Value.y != nextValue_Prev_Y
												|| curSubParam._vec2Value_End.x != nextValue_Next_X
												|| curSubParam._vec2Value_End.y != nextValue_Next_Y)
											{
												apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																					_editor,
																					_animClip._portrait,
																					//null, 
																					false,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												curSubParam._vec2Value.x = nextValue_Prev_X;
												curSubParam._vec2Value.y = nextValue_Prev_Y;
												curSubParam._vec2Value_End.x = nextValue_Next_X;
												curSubParam._vec2Value_End.y = nextValue_Next_Y;
											}

											apEditorUtil.ReleaseGUIFocus();
										}
									}
								}
								break;

							case apAnimEvent.PARAM_TYPE.String:
								{
									EditorGUI.BeginChangeCheck();

									string nextValue = EditorGUILayout.DelayedTextField(curSubParam._strValue, GUILayout.Width(valueWidth_Single));

									if (EditorGUI.EndChangeCheck())
									{
										if (!string.Equals(curSubParam._strValue, nextValue))
										{
											apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																				_editor,
																				_animClip._portrait,
																				//null, 
																				false,
																				apEditorUtil.UNDO_STRUCT.ValueOnly);

											curSubParam._strValue = nextValue;
										}
										apEditorUtil.ReleaseGUIFocus();
									}
								}
								break;
						}

						if(GUILayout.Button(_img_LayerUp, guiStyleListBtn, GUILayout.Width(30), GUILayout.Height(20)))
						{
							targetSubParam = curSubParam;
							isLayerUp = true;
						}
						if(GUILayout.Button(_img_LayerDown, guiStyleListBtn, GUILayout.Width(30), GUILayout.Height(20)))
						{
							targetSubParam = curSubParam;
							isLayerDown = true;
						}
						if(GUILayout.Button(_img_Remove, guiStyleListBtn, GUILayout.Width(30), GUILayout.Height(20)))
						{
							targetSubParam = curSubParam;
							isRemoveParam = true;
						}


						EditorGUILayout.EndHorizontal();
					}
				}
				else
				{
					for (int i = 0; i < curNumSubParams; i++)
					{
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
						GUILayout.Space(15);
						EditorGUILayout.EndHorizontal();
					}
				}
				
			}
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width - 50));
			GUILayout.Space(15);

			//변경
			if(_guiContent_AddParameter == null)
			{
				_guiContent_AddParameter = new apGUIContentWrapper();
				_guiContent_AddParameter.ClearText(false);
				_guiContent_AddParameter.AppendSpaceText(1, false);
				_guiContent_AddParameter.AppendText(_editor.GetText(TEXT.DLG_AddParameter), true);

				_guiContent_AddParameter.SetImage(_img_AddParam);
			}

			if (isSelected)
			{
				//" Add Parameter"
				if (GUILayout.Button(_guiContent_AddParameter.Content, _guiStyle_None, GUILayout.Height(20)))
				{
					if (isSelected && curSubParams != null && curSubParams.Count == curNumSubParams)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_EventChanged, 
															_editor, 
															_animClip._portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curSubParams.Add(new apAnimEvent.SubParameter());

						//파라미터를 추가하면 유효성 검사를 다시 하자
						ValidateEvent(_curSelectedEvent, true);
					}
				}
			}
			else
			{
				if (GUILayout.Button(apEditorUtil.Text_EMPTY, _guiStyle_None, GUILayout.Height(20)))
				{
					//Nooo..
				}
			}
			EditorGUILayout.EndHorizontal();



			GUILayout.Space(height_3_Properties + 100);

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();






			if (isLayerUp || isLayerDown || isRemoveParam)
			{
				if (isSelected && _curSelectedEvent != null)
				{
					//순서를 바꾸거나 SubParam을 삭제하는 요청이 있으면 처리해주자
					if (targetSubParam != null && _curSelectedEvent._subParams.Contains(targetSubParam))
					{
						if (isLayerUp)
						{
							//Index -1

							int index = _curSelectedEvent._subParams.IndexOf(targetSubParam);
							if (index > 0)
							{
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																	_editor,
																	_animClip._portrait,
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								_curSelectedEvent._subParams.Remove(targetSubParam);
								_curSelectedEvent._subParams.Insert(index - 1, targetSubParam);
							}
						}
						else if (isLayerDown)
						{
							//Index +1
							int index = _curSelectedEvent._subParams.IndexOf(targetSubParam);
							if (index < _curSelectedEvent._subParams.Count - 1)
							{
								apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																	_editor,
																	_animClip._portrait,
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								_curSelectedEvent._subParams.Remove(targetSubParam);
								_curSelectedEvent._subParams.Insert(index + 1, targetSubParam);
							}
						}
						else if (isRemoveParam)
						{
							//삭제한다.
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																_editor,
																_animClip._portrait,
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_curSelectedEvent._subParams.Remove(targetSubParam);
						}
					}


					//파라미터의 구조가 바뀌었다면 유효성 검사를 다시 한다.
					ValidateEvent(_curSelectedEvent, true);
				}
			}
			



			GUILayout.Space(10);
			// [v1.4.1] 이벤트 삭제 버튼
			string strRemoveBtnName = (isSelected && _curSelectedEvent != null) ? 
				string.Format("{0} [{1}]", _editor.GetText(TEXT.DLG_RemoveEvent), _curSelectedEvent._eventName)
				: _editor.GetText(TEXT.DLG_RemoveEvent);
			
			if(apEditorUtil.ToggledButton_2Side(strRemoveBtnName, false, isSelected && _curSelectedEvent != null, width - 10, height_5_RemoveEvent - 15))
			{
				//삭제하기
				if (isSelected && _curSelectedEvent != null)
				{
					//"애니메이션 이벤트 삭제"
					//"정말 애니메이션 이벤트 [" + _curSelectedEvent._eventName +"]를 삭제하시겠습니까?"
					bool isResult = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.DLG_RemoveAnimEventMain_Title),
																	_editor.GetTextFormat(TEXT.DLG_RemoveAnimEventMain_Body, _curSelectedEvent._eventName),
																	_editor.GetText(TEXT.Remove),
																	_editor.GetText(TEXT.Cancel));

					if (isResult)
					{
						apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_RemoveEvent,
															_editor,
															_animClip._portrait,
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_animClip._animEvents.Remove(_curSelectedEvent);

						_curSelectedEvent = null;
						isSelected = false;
						isGUIEvent = true;
					}
				}
			}

			if(isGUIEvent)
			{
				_prevNumSubParams = curNumSubParams;
			}
		}


		//오른쪽 GUI : 이벤트 프리셋을 보여준다.
		private void GUI_Right(int width, int height, int posX, int posY, bool isGUIEvent)
		{
			//0. 프리셋 타이틀
			//1. 프리셋 리스트 (스크롤뷰)
			//2. 선택된 프리셋 속성들
			//3. 프리셋 삭제하기/복제하기 버튼들
			

			int height_0_Title = 30;
			int height_2_AdaptButton = 35;
			int height_3_PresetProperties = 160;
			int height_4_PresetParams = 100;
			int height_5_RemoveButton = 20;
			int height_1_PresetList = height 
				- (height_0_Title + height_3_PresetProperties + height_2_AdaptButton + height_4_PresetParams + height_5_RemoveButton + 65);


			//프리셋들
			List<apAnimEventPresetUnit> presets = _editor.AnimEventPreset.Presets;
			int nPresets = presets != null ? presets.Count : 0;

			//------------------------------
			// 0. 타이틀
			//------------------------------
			if(_guiContent_Presets == null)
			{
				_guiContent_Presets = apGUIContentWrapper.Make(1, _editor.GetText(TEXT.EventPresets), _img_Presets);
			}

			if(_guiContent_PresetCategory == null)
			{
				_guiContent_PresetCategory = apGUIContentWrapper.Make(_editor.GetUIWord(UIWORD.Presets), false, _img_Category);
			}


			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.LabelField(_guiContent_Presets.Content, GUILayout.Height(height_0_Title));

			GUILayout.Space(5);


			//------------------------------
			// 1. 프리셋 리스트
			//------------------------------


			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(posX + 3, posY + height_0_Title + 7, width, height_1_PresetList), "");
			GUI.backgroundColor = prevColor;

			int width_ScrollListItem = width - 30;
			_scrollList_Preset = EditorGUILayout.BeginScrollView(_scrollList_Preset, GUILayout.Width(width), GUILayout.Height(height_1_PresetList));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(width_ScrollListItem));



			GUILayout.Button(_guiContent_PresetCategory.Content, _guiStyle_None, GUILayout.Height(20));//<투명 버튼//

			if(nPresets > 0)
			{
				apAnimEventPresetUnit curPreset = null;
				

				for (int i = 0; i < nPresets; i++)
				{
					GUIStyle curGUIStyle = _guiStyle_None;
					curPreset = presets[i];

					bool isSelect = false;
					if (curPreset == _selectedPreset)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();


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

						//GUI.Box(new Rect(lastRect.x, lastRect.y + 21, width_ScrollListItem + 16, 20), "");
						//GUI.backgroundColor = prevColor; 
						#endregion

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 21, width_ScrollListItem + 16 - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);

						curGUIStyle = _guiStyle_Selected;
					}


					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_ScrollListItem));
					GUILayout.Space(15);
					//프리셋 정보를 보여주자

					//1. 색상
					//2. 이름 + 파라미터


					//1. 아이콘
					GUI.backgroundColor = curPreset.GetIconColor();
					GUILayout.Box(apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box, apGUILOFactory.I.Width(14), apGUILOFactory.I.Height(14));//일반 박스 이미지
					GUI.backgroundColor = prevColor;

					GUILayout.Space(10);

					if (GUILayout.Button(curPreset._eventName, curGUIStyle, GUILayout.Width(width_ScrollListItem - 80), GUILayout.Height(20)))
					{
						isSelect = true;
					}
					EditorGUILayout.EndHorizontal();

					if(isSelect)
					{
						_selectedPreset = curPreset;
						apEditorUtil.ReleaseGUIFocus();
					}
				}
			}

			GUILayout.Space(height_1_PresetList + 100);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			GUILayout.Space(5);

			int width_Prop = width - 10;
			
			//-------------------------------
			// 3. 프리셋 적용 버튼
			//-------------------------------
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.ApplySelectedPreset), false, _selectedPreset != null && _curSelectedEvent != null, width_Prop, height_2_AdaptButton))
			{
				//bool result = EditorUtility.DisplayDialog(
				//	"프리셋 적용하기",
				//	"프리셋(" + _selectedPreset._eventName + ")의 속성을 선택된 애니메이션 이벤트(" + _curSelectedEvent._eventName + ")에 복사하여 적용하시겠습니까?",
				//	"적용", "취소");


				bool result = EditorUtility.DisplayDialog(
					_editor.GetText(TEXT.DLG_ApplyAnimEventPreset_Ttitle),
					_editor.GetTextFormat(TEXT.DLG_ApplyAnimEventPreset_Body, _selectedPreset._eventName, _curSelectedEvent._eventName),
					_editor.GetText(TEXT.DLG_Apply),
					_editor.GetText(TEXT.Cancel));

				if (result)
				{
					//현재 선택된 프리셋을 선택된 이벤트에 적용한다.
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_EventChanged,
																	_editor,
																	_animClip._portrait,
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

					_editor.AnimEventPreset.AdaptToEventFromPreset(_selectedPreset, _curSelectedEvent);
				}
				
				apEditorUtil.ReleaseGUIFocus();
			}

			GUILayout.Space(10);

			//----------------------------------------------------
			// 4. 선택된 프리셋의 속성 보여주기
			//----------------------------------------------------
			string curProp_EventName = "";
			apAnimEvent.CALL_TYPE curProp_CallType = apAnimEvent.CALL_TYPE.Once;
			apAnimEvent.ICON_COLOR curProp_IconColor = apAnimEvent.ICON_COLOR.Yellow;
			int nPropParams = 0;
			List<apAnimEventPresetUnit.SubParamInfo> curProp_Params = null;

			if(_selectedPreset != null)
			{
				curProp_EventName = _selectedPreset._eventName;
				curProp_CallType = _selectedPreset._callType;
				curProp_IconColor = _selectedPreset._iconColor;
				curProp_Params = _selectedPreset._subParams;
				nPropParams = curProp_Params != null ? curProp_Params.Count : 0;
			}

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Prop), GUILayout.Height(height_3_PresetProperties));

			if(_selectedPreset == null)
			{
				//선택되지 않았다면
				GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_EventName));			
			string nextProp_EventName = EditorGUILayout.DelayedTextField(curProp_EventName);
			
			GUILayout.Space(4);

			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_CallMethod));
			apAnimEvent.CALL_TYPE nextProp_CallType = (apAnimEvent.CALL_TYPE)EditorGUILayout.EnumPopup(curProp_CallType);

			GUILayout.Space(4);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.MarkerColor));
			apAnimEvent.ICON_COLOR nextProp_IconColor = (apAnimEvent.ICON_COLOR)EditorGUILayout.EnumPopup(curProp_IconColor);

			if(EditorGUI.EndChangeCheck())
			{
				//설정 변경
				if(_selectedPreset != null)
				{
					_selectedPreset._eventName = nextProp_EventName;
					_selectedPreset._callType = nextProp_CallType;
					_selectedPreset._iconColor = nextProp_IconColor;
					
					//저장
					_editor.AnimEventPreset.Save();
				}

				apEditorUtil.ReleaseGUIFocus();
			}


			GUI.backgroundColor = prevColor;

			EditorGUILayout.EndVertical();

			//----------------------------------------------------
			// 5. 선택된 프리셋의 파라미터 리스트
			//----------------------------------------------------
			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
			GUI.Box(new Rect(posX + 3, posY + height_0_Title + height_1_PresetList + height_2_AdaptButton + height_3_PresetProperties + 28, width, height_4_PresetParams), "");
			GUI.backgroundColor = prevColor;

			_scrollList_PresetParams = EditorGUILayout.BeginScrollView(_scrollList_PresetParams, GUILayout.Width(width), GUILayout.Height(height_4_PresetParams));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(width_ScrollListItem));

			if(nPropParams > 0)
			{
				apAnimEventPresetUnit.SubParamInfo curParamInfo = null;
				for (int i = 0; i < nPropParams; i++)
				{
					curParamInfo = curProp_Params[i];

					EditorGUILayout.BeginHorizontal(GUILayout.Width(width_ScrollListItem));
					GUILayout.Space(15);
					EditorGUILayout.LabelField("[" + i + "] " + curParamInfo._paramType);
					EditorGUILayout.EndHorizontal();

				}
			}

			GUILayout.Space(height_4_PresetParams + 100);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width_Prop);
			GUILayout.Space(10);

			//----------------------------------------------------
			// 6. 선택된 프리셋 삭제하기
			//----------------------------------------------------
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.RemoveEventPreset), false, _selectedPreset != null, width_Prop, height_5_RemoveButton))
			{
				if(_selectedPreset != null)
				{
					//bool result = EditorUtility.DisplayDialog(	"프리셋 삭제하기",
					//											"프리셋(" + _selectedPreset._eventName + ")을 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
					//											"삭제", "취소");


					bool result = EditorUtility.DisplayDialog(	_editor.GetText(TEXT.RemoveEventPreset),
																_editor.GetTextFormat(TEXT.DLG_RemoveAnimEvent_Body, _selectedPreset._eventName),
																_editor.GetText(TEXT.Remove),
																_editor.GetText(TEXT.Cancel));

					

					if(result)
					{
						_editor.AnimEventPreset.RemovePreset(_selectedPreset);
						_editor.AnimEventPreset.Save();//바로 저장을 한다.
						_selectedPreset = null;
					}
				}
			}
		}







		// Functions
		//--------------------------------------------------------------
		/// <summary>
		/// 모든 애니메이션을 체크한다.
		/// </summary>
		private void ResetValidation()
		{
			if(_event2Validation == null)
			{
				_event2Validation = new Dictionary<apAnimEvent, EVENT_NAME_VALIDATION>();
			}
			_event2Validation.Clear();//리셋!

			if(_animClip == null)
			{
				return;
			}
			//이벤트 전체를 하나씩 체크하자
			int nEvents = _animClip._animEvents != null ? _animClip._animEvents.Count : 0;

			if(nEvents == 0)
			{
				return;
			}

			apAnimEvent curAnimEvent = null;
			
			for (int i = 0; i < nEvents; i++)
			{
				curAnimEvent = _animClip._animEvents[i];
				if(curAnimEvent == null)
				{
					continue;
				}

				ValidateEvent(curAnimEvent, true);//강제로 체크한다.

			}

		}


		private void ValidateEvent(apAnimEvent targetEvent, bool isValidationForce)
		{
			if(targetEvent == null)
			{
				return;
			}

			EVENT_NAME_VALIDATION validStatus = EVENT_NAME_VALIDATION.NotChecked;

			//가장 먼저 Dictionary에 입력 (또는 추가)
			if(_event2Validation.ContainsKey(targetEvent))
			{
				//현재 저장된 값이 있다면 그걸 받자
				validStatus = _event2Validation[targetEvent];

			}
			else
			{
				//없다면 일단 검사 여부 상관없이 추가 (NotChecked 상태로)
				_event2Validation.Add(targetEvent, validStatus);
			}

			//만약 이미 검사를 마친 상태에서 강제 옵션이 없다면 검사를 생략한다.
			if(!isValidationForce && validStatus != EVENT_NAME_VALIDATION.NotChecked)
			{
				return;
			}


			//체크를 하자
			validStatus = EVENT_NAME_VALIDATION.Valid;//일단 유효하다고 설정


			//1. 이름 체크
			string correctedName = "";//사용하진 않는다.
			EVENT_NAME_VALIDATION_RESULT nameCheckResult = CheckEventNameAlphaNumeric(targetEvent._eventName, ref correctedName);

			if(nameCheckResult == EVENT_NAME_VALIDATION_RESULT.Empty)
			{
				//공백이다.
				validStatus = EVENT_NAME_VALIDATION.Empty;
			}
			else if(nameCheckResult == EVENT_NAME_VALIDATION_RESULT.NotAllowedCharacters)
			{
				//유효하지 않은 글자가 포함되어 있다.
				validStatus = EVENT_NAME_VALIDATION.InvalidWord;
			}
			else
			{
				//이름은 유효하다.
				//다른 이벤트와 비교하자 (SendMessage인 경우)
				if(_portrait._animEventCallMode == apPortrait.ANIM_EVENT_CALL_MODE.SendMessage)
				{
					//이름이 같은 다른 애니메이션을 찾자
					int nAnimClips = _portrait._animClips != null ? _portrait._animClips.Count : 0;
					bool isAnyOverloaded = false;

					if (nAnimClips > 0)
					{
						apAnimClip curAnimClip = null;
						for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
						{
							curAnimClip = _portrait._animClips[iAnimClip];
							if(curAnimClip == null)
							{
								continue;
							}

							int nEvents = curAnimClip._animEvents != null ? curAnimClip._animEvents.Count : 0;

							if(nEvents == 0)
							{
								continue;
							}

							apAnimEvent otherEvent = null;
							for (int iEvent = 0; iEvent < nEvents; iEvent++)
							{
								otherEvent = curAnimClip._animEvents[iEvent];

								//이름이 동일하고 파라미터가 다른 오버로드 이벤트를 찾자
								if(targetEvent == otherEvent)
								{
									continue;
								}

								bool isOverload = IsOverloadEvents(targetEvent, otherEvent, targetEvent._eventName);
								if(isOverload)
								{
									//오버로드된 걸 찾았다.
									isAnyOverloaded = true;
									break;
								}
							}


							if(isAnyOverloaded)
							{
								break;
							}
						}
					}

					if(isAnyOverloaded)
					{
						//다른 이벤트와 오버로딩 관계이다.
						validStatus = EVENT_NAME_VALIDATION.Overloading;
					}
					
				}
			}

			//유효성 결과를 넣어주자
			_event2Validation[targetEvent] = validStatus;
		}


		/// <summary>
		/// 유효한 이름을 계산해서 리턴한다.
		/// </summary>
		/// <param name="targetEvent"></param>
		private string GetFixedEventName(apAnimEvent targetEvent)
		{
			if(targetEvent == null)
			{
				return null;
			}

			//중요한 순서대로 이름을 체크한다.
			//(유효성 문제는 순차적으로 발생할 수 있다.)
			string resultName = targetEvent._eventName;

			//1. 비어있다면
			if(string.IsNullOrEmpty(resultName))
			{	
				resultName = "Nonamed_Event";
			}

			//1. 비어있거나 유효하지 않은 글자가 있다면
			string correctedName = "";//사용하진 않는다.
			EVENT_NAME_VALIDATION_RESULT nameCheckResult = CheckEventNameAlphaNumeric(resultName, ref correctedName);
			if(nameCheckResult == EVENT_NAME_VALIDATION_RESULT.NotAllowedCharacters)
			{
				//유효하지 않은 글자가 있었다.
				resultName = correctedName;
			}
			else if(nameCheckResult == EVENT_NAME_VALIDATION_RESULT.Empty)
			{
				//비어있다.
				resultName = "NonamedEvent";
			}
			else
			{
				//일단 유효하긴 하다
			}

			//이제 다른 이벤트의 이름과 중복되는지 + 오버로드 상태인지 확인한다.
			//중복된다면 _1, _2과 같이 숫자를 붙인다.
			
			if(_portrait._animEventCallMode == apPortrait.ANIM_EVENT_CALL_MODE.SendMessage)
			{
				//이름이 같은 다른 애니메이션을 찾자
				int nAnimClips = _portrait._animClips != null ? _portrait._animClips.Count : 0;
				string checkName = resultName;
				bool isPostNameAddable = false;
				int iCount = 1;

				while (true)
				{
					bool isAnyOverloaded = false;

					if(!isPostNameAddable)
					{
						checkName = resultName;
					}
					else
					{
						checkName = resultName + "_" + iCount;
					}

					if (nAnimClips > 0)
					{
						apAnimClip curAnimClip = null;
						for (int iAnimClip = 0; iAnimClip < nAnimClips; iAnimClip++)
						{
							curAnimClip = _portrait._animClips[iAnimClip];
							if (curAnimClip == null)
							{
								continue;
							}

							int nEvents = curAnimClip._animEvents != null ? curAnimClip._animEvents.Count : 0;

							if (nEvents == 0)
							{
								continue;
							}

							apAnimEvent otherEvent = null;
							for (int iEvent = 0; iEvent < nEvents; iEvent++)
							{
								otherEvent = curAnimClip._animEvents[iEvent];

								//이름이 동일하고 파라미터가 다른 오버로드 이벤트를 찾자
								if (targetEvent == otherEvent)
								{
									continue;
								}

								bool isOverload = IsOverloadEvents(targetEvent, otherEvent, checkName);
								if (isOverload)
								{
									//오버로드된 걸 찾았다.
									isAnyOverloaded = true;
									break;
								}
							}


							if (isAnyOverloaded)
							{
								break;
							}
						}
					}

					if (isAnyOverloaded)
					{
						//오버로드된게 있다면
						//이름을 바꿔서 다시 체크하도록 한다
						if (!isPostNameAddable)
						{
							isPostNameAddable = true;
						}
						else
						{
							iCount += 1;
						}
					}
					else
					{
						//오버로드된게 없다면 종료
						resultName = checkName;
						break;
					}
				}	
			}

			return resultName;
		}



		private bool IsOverloadEvents(apAnimEvent eventA, apAnimEvent eventB, string eventNameA)
		{
			if(eventA == eventB
				|| eventA == null
				|| eventB == null)
			{
				return false;
			}

			//이름이 같고, 파라미터 타입이 달라야 한다.
			if(!string.Equals(eventNameA, eventB._eventName))
			{
				//이름이 다르다면 오버로드가 아니다.
				return false;
			}

			//파라미터가 모두 동일핟면 오버로드가 아니다.
			int nParams_A = eventA._subParams != null ? eventA._subParams.Count : 0;
			int nParams_B = eventB._subParams != null ? eventB._subParams.Count : 0;

			//개수가 다르면 비교할 필요도 없이 오버로드다
			if(nParams_A != nParams_B)
			{
				return true;
			}

			//하나라도 타입이 다르면 오버로드다 (값은 안본다.)
			for (int iParam = 0; iParam < nParams_A; iParam++)
			{
				if(eventA._subParams[iParam]._paramType != eventB._subParams[iParam]._paramType)
				{
					//A와 B의 파라미터가 다르다.
					return true;
				}
			}

			// 모두 동일하니 오버로드가 아니다.
			return false;

		}


		private enum EVENT_NAME_VALIDATION_RESULT
		{
			Valid,
			Empty,
			NotAllowedCharacters
		}
		/// <summary>
		/// 이벤트의 이름이 함수로 불릴 정도로 유효한지 체크한다. (영어+숫자+_)
		/// </summary>
		private EVENT_NAME_VALIDATION_RESULT CheckEventNameAlphaNumeric(string strEventName, ref string strCorrectedName)
		{
			//비어있으면 false
			if(string.IsNullOrEmpty(strEventName))
			{
				return EVENT_NAME_VALIDATION_RESULT.Empty;
			}

			int nLength = strEventName.Length;
			bool isValid = true;
			
			strCorrectedName = "";//유효한 글자만 모으자

			for (int i = 0; i < nLength; i++)
			{
				char c = strEventName[i];
				switch (c)
				{
					//유효한 글자들
					case 'a': case 'b': case 'c': case 'd': case 'e': case 'f': case 'g': case 'h': case 'i': case 'j': 
					case 'k': case 'l': case 'm': case 'n': case 'o': case 'p': case 'q': case 'r': case 's': case 't': 
					case 'u': case 'v': case 'w': case 'x': case 'y': case 'z':
						//영어 소문자

					case 'A': case 'B': case 'C': case 'D': case 'E': case 'F': case 'G': case 'H': case 'I': case 'J': 
					case 'K': case 'L': case 'M': case 'N': case 'O': case 'P': case 'Q': case 'R': case 'S': case 'T': 
					case 'U': case 'V': case 'W': case 'X': case 'Y': case 'Z':
						//영어 대문자

					case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9':
						//숫자

					case '_':
						//언더바

						strCorrectedName += c;
						break;

					default:
						
						//유효하지 않은 글자 발견
						isValid = false;
						break;
				}
			}

			if(isValid)
			{
				return EVENT_NAME_VALIDATION_RESULT.Valid;
			}

			//유효하지 않은 글자가 있었다.
			return EVENT_NAME_VALIDATION_RESULT.NotAllowedCharacters;


		}


		// Get / Set
		//--------------------------------------------------------------
	}
}