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
using System.Diagnostics;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.3.13 : 에디터 사용중에 중요한 키는 화면 하단에 알려준다.
	/// 기존에 일반 UI에 있었던 툴 사용법 UI가 작업 영역으로 옮겨진 셈
	/// </summary>
	public class apGUIHowToUseTips
	{
		// Members
		//-------------------------------------------
		public apEditor _editor = null;

		public enum ICON_TYPE
		{
			MouseLeft,
			MouseMiddle,
			MouseRight,
			Ctrl,
			Shift,
			Alt,
			Delete
		}

		private Dictionary<ICON_TYPE, Texture2D> _iconType2Images = null;
		private Vector2 _curPos = Vector2.zero;

		//현재 메시지를 보자.
		//같은 종류의 메시지를 연속으로 출력할때는 시간이 지나면 사라진다.
		private enum TIP_MSG
		{
			None,
			Mesh_MakeMesh_AddTool_VertexAndEdge,
			Mesh_MakeMesh_AddTool_VertexOnly,
			Mesh_MakeMesh_AddTool_EdgeOnly,
			Mesh_MakeMesh_AddTool_Polygon,
			Mesh_PivotEdit,
			Mesh_Modify,
			Mesh_Pin_Select,
			Mesh_Pin_Add,
			Mesh_Pin_Link,
			Mesh_Pin_Test,
			MeshGroup_Bone_SelectOnly,
			MeshGroup_Bone_SelectAndTRS,
			MeshGroup_Bone_Add,
			MeshGroup_Bone_Link,
		}
		private TIP_MSG _curMsg = TIP_MSG.None;
		private bool _isTimeout = false;
		private float _tMsgLive = 0.0f;
		private const float MSG_LIVE_TIME = 10.0f;
		private float _msgAlpha = 1.0f;
		
		
		private float _scaledIconSize = 1.0f;
		private const float ICON_SIZE = 28.0f;
		private const int WIDTH_TEXT = 300;
		private const float HEIGHT_TEXT = 25.0f;

		//미리 등록을 하고 하나씩 출력한다.
		private const int MAX_REQUEST = 10;
		private ICON_TYPE[] _requestedIcons = null;
		private UIWORD[] _requestedTexts = null;
		private int _nRequest = 0;
		private int _maxTextLength = 0;

		private Stopwatch _timer = null;
		private const float MAX_TIMEUNIT = 0.1f;//0.1초 (10FPS)보다 오래 걸린 프레임은 0.1로 계산한다.

		// Init
		//-------------------------------------------
		public apGUIHowToUseTips(apEditor editor)
		{
			_editor = editor;

			_nRequest = 0;
			_requestedIcons = new ICON_TYPE[MAX_REQUEST];
			_requestedTexts = new UIWORD[MAX_REQUEST];
			_maxTextLength = 0;

			_timer = new Stopwatch();
			_timer.Stop();
			_timer.Reset();
			_timer.Start();

			InitImages();
		}

		public void InitImages()
		{
			_iconType2Images = new Dictionary<ICON_TYPE, Texture2D>();
			_iconType2Images.Add(ICON_TYPE.MouseLeft,	_editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseLeft));
			_iconType2Images.Add(ICON_TYPE.MouseMiddle,	_editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseMiddle));
			_iconType2Images.Add(ICON_TYPE.MouseRight,	_editor.ImageSet.Get(apImageSet.PRESET.Edit_MouseRight));
			_iconType2Images.Add(ICON_TYPE.Ctrl,		_editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyCtrl));
			_iconType2Images.Add(ICON_TYPE.Shift,		_editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyShift));
			_iconType2Images.Add(ICON_TYPE.Alt,			_editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyAlt));
			_iconType2Images.Add(ICON_TYPE.Delete,		_editor.ImageSet.Get(apImageSet.PRESET.Edit_KeyDelete));
			
		}



		// Functions
		//-------------------------------------------
		/// <summary>
		/// 상황에 맞게 자동으로 팁을 보여준다.
		/// Gizmo가 없는 경우에만 사용된다.
		/// </summary>
		/// <param name="rightCenter"></param>
		public void DrawTips(Vector2 rightCenter)
		{
			if (_editor == null
				|| _editor._portrait == null
				|| _editor.Select == null
				|| !_editor._guiOption_isShowHowToEdit)
			{
				return;
			}

			float tDelta = (float)_timer.ElapsedMilliseconds / 1000.0f;
			if (tDelta > 0.0f)
			{
				_timer.Stop();
				_timer.Reset();
				_timer.Start();
			}

			if(tDelta > MAX_TIMEUNIT && !_editor._isLowCPUOption)
			{
				tDelta = MAX_TIMEUNIT;
			}


			TIP_MSG nextMsg = TIP_MSG.None;
			switch (_editor.Select.SelectionType)
			{
				case apSelection.SELECTION_TYPE.Mesh:
					//메시 메뉴에서
					switch (_editor._meshEditMode)
					{
						case apEditor.MESH_EDIT_MODE.MakeMesh:
							{
								switch (_editor._meshEditeMode_MakeMesh_Tab)
								{
									case apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AddTools:
										{
											switch (_editor._meshEditeMode_MakeMesh_AddTool)
											{
												case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge:
													nextMsg = TIP_MSG.Mesh_MakeMesh_AddTool_VertexAndEdge;
													break;

												case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly:
													nextMsg = TIP_MSG.Mesh_MakeMesh_AddTool_VertexOnly;
													break;

												case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly:
													nextMsg = TIP_MSG.Mesh_MakeMesh_AddTool_EdgeOnly;
													break;

												case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon:
													nextMsg = TIP_MSG.Mesh_MakeMesh_AddTool_Polygon;
													break;
											}
										}
										break;
								}
							}
							break;

						case apEditor.MESH_EDIT_MODE.PivotEdit:
							nextMsg = TIP_MSG.Mesh_PivotEdit;
							break;

						case apEditor.MESH_EDIT_MODE.Modify:
							nextMsg = TIP_MSG.Mesh_Modify;
							break;

						case apEditor.MESH_EDIT_MODE.Pin:
							{
								switch (_editor._meshEditMode_Pin_ToolMode)
								{
									case apEditor.MESH_EDIT_PIN_TOOL_MODE.Select:
										nextMsg = TIP_MSG.Mesh_Pin_Select;
										break;

									case apEditor.MESH_EDIT_PIN_TOOL_MODE.Add:
										nextMsg = TIP_MSG.Mesh_Pin_Add;
										break;

									case apEditor.MESH_EDIT_PIN_TOOL_MODE.Link:
										nextMsg = TIP_MSG.Mesh_Pin_Link;
										break;

									case apEditor.MESH_EDIT_PIN_TOOL_MODE.Test:
										nextMsg = TIP_MSG.Mesh_Pin_Test;
										break;
								}
							}
							break;
					}
					break;

				case apSelection.SELECTION_TYPE.MeshGroup:
					{
						switch (_editor._meshGroupEditMode)
						{
							case apEditor.MESHGROUP_EDIT_MODE.Bone:
								{
									switch (_editor.Select.BoneEditMode)
									{
										case apSelection.BONE_EDIT_MODE.SelectOnly:
											nextMsg = TIP_MSG.MeshGroup_Bone_SelectOnly;
											break;

										case apSelection.BONE_EDIT_MODE.SelectAndTRS:
											nextMsg = TIP_MSG.MeshGroup_Bone_SelectAndTRS;
											break;

										case apSelection.BONE_EDIT_MODE.Add:
											nextMsg = TIP_MSG.MeshGroup_Bone_Add;
											break;

										case apSelection.BONE_EDIT_MODE.Link:
											nextMsg = TIP_MSG.MeshGroup_Bone_Link;
											break;
									}
								}
								break;
						}
					}
					break;
			}
			if (nextMsg != _curMsg)
			{
				//메시지가 바뀌었다.
				_curMsg = nextMsg;
				if (_curMsg == TIP_MSG.None)
				{
					_isTimeout = true;
					_tMsgLive = 0.0f;
					_msgAlpha = 0.0f;
				}
				else
				{
					_isTimeout = false;
					_tMsgLive = 0.0f;
					_msgAlpha = 1.0f;
				}
			}
			else if (!_isTimeout)
			{
				//유지가 되는 중. 타이머를 돌리자.
				_tMsgLive += tDelta;
				if (_tMsgLive > MSG_LIVE_TIME)
				{
					//시간이 종료되어 메시지가 사라진다.
					_isTimeout = true;
					_msgAlpha = 0.0f;
				}
				else
				{
					if(_tMsgLive < MSG_LIVE_TIME * 0.9f)
					{
						_msgAlpha = 1.0f;
					}
					else
					{
						float lerp = (_tMsgLive - (MSG_LIVE_TIME * 0.9f)) / (MSG_LIVE_TIME * 0.1f);
						_msgAlpha = 1.0f * (1.0f - lerp);
					}
					
				}
			}

			//중요
			//Window는 Ctrl + Alt + Shift 순서로 한다. 단 특성이 겹치면 붙인다. (Ctrl이 먼저)
			//Mac은 Option + Shift + Command 순서로 한다. (Shift가 먼저)
			if (_curMsg != TIP_MSG.None && !_isTimeout)
			{
				switch (_curMsg)
				{
					case TIP_MSG.Mesh_MakeMesh_AddTool_VertexAndEdge:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.AddOrMoveVertexWithEdges);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.RemoveVertexorEdge);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.LCutEdge_RDeleteVertex);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SnapToVertex);
#else
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SnapToVertex);
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.LCutEdge_RDeleteVertex);
#endif
							
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_MakeMesh_AddTool_VertexOnly:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.AddOrMoveVertex);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.RemoveVertex);
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_MakeMesh_AddTool_EdgeOnly:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft, UIWORD.LinkVertices_TurnEdge);
							AddIconAndText(ICON_TYPE.MouseMiddle, UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight, UIWORD.RemoveEdge);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Shift, UIWORD.CutEdge);
							AddIconAndText(ICON_TYPE.Ctrl, UIWORD.SnapToVertex);
							
#else
							AddIconAndText(ICON_TYPE.Ctrl, UIWORD.SnapToVertex);
							AddIconAndText(ICON_TYPE.Shift, UIWORD.CutEdge);
#endif
							
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_MakeMesh_AddTool_Polygon:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft, UIWORD.SelectPolygon);
							AddIconAndText(ICON_TYPE.MouseMiddle, UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.Delete, UIWORD.RemovePolygon);
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_PivotEdit:
						{
							//피벗 이동
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft, UIWORD.MovePivot);
							AddIconAndText(ICON_TYPE.MouseMiddle, UIWORD.MoveView);
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_Modify:
						{
							//버텍스 선택만
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.SelectVertex);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.DeselectAll);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Alt,			UIWORD.Deselect);
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
#else
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Alt,			UIWORD.Deselect);
#endif
							
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_Pin_Select:
					case TIP_MSG.Mesh_Pin_Test:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.SelectPin);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.DeselectAll);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
#else
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
#endif
							
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_Pin_Add:
						{
							//핀 선택/추가/연결
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.AddLinkMovePin);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.DeselectRemovePinCurve);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Alt,			UIWORD.SwitchCurveShape);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SnapToPin);
#else
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SnapToPin);
							AddIconAndText(ICON_TYPE.Alt,			UIWORD.SwitchCurveShape);
#endif
							
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.Mesh_Pin_Link:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.LinkPins_HowToUse);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.DeselectRemoveCurve);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Alt,			UIWORD.SwitchCurveShape);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SnapToPin);
#else
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SnapToPin);
							AddIconAndText(ICON_TYPE.Alt,			UIWORD.SwitchCurveShape);
#endif
							
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.MeshGroup_Bone_SelectOnly:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.SelectBones);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.DeselectAll);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
#else
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
#endif
							
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.MeshGroup_Bone_SelectAndTRS:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.SelectBones);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.DeselectAll);
#if UNITY_EDITOR_OSX
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
#else
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SelectMore);
							AddIconAndText(ICON_TYPE.Shift,			UIWORD.SelectMore);
#endif					
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.MeshGroup_Bone_Add:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft,		UIWORD.AddBones);
							AddIconAndText(ICON_TYPE.MouseMiddle,	UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight,	UIWORD.Deselect);
							AddIconAndText(ICON_TYPE.Ctrl,			UIWORD.SetIn8Directions);
							DrawIconsAndTexts(rightCenter);
						}
						break;

					case TIP_MSG.MeshGroup_Bone_Link:
						{
							ReadyToDraw();
							AddIconAndText(ICON_TYPE.MouseLeft, UIWORD.SelectAndLinkBones);
							AddIconAndText(ICON_TYPE.MouseMiddle, UIWORD.MoveView);
							AddIconAndText(ICON_TYPE.MouseRight, UIWORD.Deselect);
							DrawIconsAndTexts(rightCenter);
						}
						break;
				}
			}
		}

		// 하나씩 렌더링
		//커서 위치를 리셋한다.
		private void ReadyToDraw()
		{
			_nRequest = 0;
			_maxTextLength = 0;
		}

		/// <summary>
		/// 아이콘과 텍스트를 출력한다.
		/// </summary>
		/// <param name="iconType"></param>
		/// <param name="tipText"></param>
		private void AddIconAndText(ICON_TYPE iconType, UIWORD tipText)
		{
			_requestedIcons[_nRequest] = iconType;
			_requestedTexts[_nRequest] = tipText;
			_nRequest++;

			//int nText = apUtil.GetStringRealLength(_editor.GetUIWord(tipText));
			int nTextLength = _editor.GetUIWord(tipText).Length;
			if(nTextLength > _maxTextLength)
			{
				_maxTextLength = nTextLength;
			}
		}

		private void DrawIconsAndTexts(Vector2 rightCenter)
		{
			_curPos = rightCenter;
			_curPos.y -= (HEIGHT_TEXT * _nRequest) / 2;
			_scaledIconSize = ICON_SIZE / apGL.Zoom;

			int textWidth = 0;
			switch (_editor._language)
			{
				case apEditor.LANGUAGE.Korean:
				case apEditor.LANGUAGE.Japanese:
				case apEditor.LANGUAGE.Chinese_Simplified:
				case apEditor.LANGUAGE.Chinese_Traditional:
					textWidth = _maxTextLength * 11;
					break;

				default:
					textWidth = _maxTextLength * 8;
					break;

			}
			
			if(textWidth < 100)
			{
				textWidth = 100;
			}
			else if(textWidth > WIDTH_TEXT)
			{
				textWidth = WIDTH_TEXT;
			}

			for (int i = 0; i < _nRequest; i++)
			{
				apGL.DrawTextGL(_editor.GetUIWord(_requestedTexts[i]), _curPos - new Vector2(textWidth, 0.0f), 
					textWidth, 
					new Color(1.0f, 1.0f, 0.0f, _msgAlpha));

				apGL.DrawTextureGL(	_iconType2Images[_requestedIcons[i]], 
									_curPos - new Vector2(textWidth + (ICON_SIZE * 0.5f + 5), -(ICON_SIZE * 0.5f - 7)), 
									_scaledIconSize, _scaledIconSize, new Color(0.5f, 0.5f, 0.5f, _msgAlpha), 1.0f);

				_curPos.y += HEIGHT_TEXT;
			}
		}
	}
}