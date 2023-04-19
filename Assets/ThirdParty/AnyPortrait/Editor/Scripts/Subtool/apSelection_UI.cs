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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngineInternal;

namespace AnyPortrait
{

	//apSelection의 UI를 그리는 함수들을 모은 부분 클래스
	public partial class apSelection
	{
		// Draw - 오른쪽
		//--------------------------------------------------------------
		public bool DrawEditor(int width, int height)
		{
			if (_portrait == null)
			{
				return false;
			}

			//EditorGUILayout.Space();//삭제 21.3.12

			switch (_selectionType)
			{
				case SELECTION_TYPE.None:
					Draw_None(width, height);
					break;

				case SELECTION_TYPE.ImageRes:
					Draw_ImageRes(width, height);
					break;
				case SELECTION_TYPE.Mesh:
					Draw_Mesh(width, height);
					break;
				case SELECTION_TYPE.Face:
					Draw_Face(width, height);
					break;
				case SELECTION_TYPE.MeshGroup:
					Draw_MeshGroup(width, height);
					break;
				case SELECTION_TYPE.Animation:
					Draw_Animation(width, height);
					break;
				case SELECTION_TYPE.Overall:
					Draw_Overall(width, height);
					break;
				case SELECTION_TYPE.Param:
					Draw_Param(width, height);
					break;
			}

			EditorGUILayout.Space();

			return true;
		}



		public void DrawEditor_Header(int width, int height)
		{
			switch (_selectionType)
			{
				case SELECTION_TYPE.None:
					DrawTitle(Editor.GetUIWord(UIWORD.NotSelected), width, height);//"Not Selected"
					break;

				case SELECTION_TYPE.ImageRes:
					DrawTitle(Editor.GetUIWord(UIWORD.Image), width, height);//"Image"
					break;
				case SELECTION_TYPE.Mesh:
					DrawTitle(Editor.GetUIWord(UIWORD.Mesh), width, height);//"Mesh"
					break;
				case SELECTION_TYPE.Face:
					DrawTitle("Face", width, height);
					break;
				case SELECTION_TYPE.MeshGroup:
					DrawTitle(Editor.GetUIWord(UIWORD.MeshGroup), width, height);//"Mesh Group"
					break;
				case SELECTION_TYPE.Animation:
					DrawTitle(Editor.GetUIWord(UIWORD.AnimationClip), width, height);//"Animation Clip"
					break;
				case SELECTION_TYPE.Overall:
					DrawTitle(Editor.GetUIWord(UIWORD.RootUnit), width, height);//"Root Unit"
					break;
				case SELECTION_TYPE.Param:
					DrawTitle(Editor.GetUIWord(UIWORD.ControlParameter), width, height);//"Control Parameter"
					break;
			}
		}








		private void Draw_None(int width, int height)
		{
			//EditorGUILayout.Space();
		}

		private void Draw_Face(int width, int height)
		{
			//GUILayout.Box("Face", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Face", width);
			EditorGUILayout.Space();

		}


		

		private void DrawTitle(string strTitle, int width, int height)
		{
			int titleWidth = width;

			//삭제 19.8.18 : Layout 출력 여부 버튼의 위치가 바뀌었다.
			//bool isShowHideBtn = false;
			//if (_selectionType == SELECTION_TYPE.MeshGroup || _selectionType == SELECTION_TYPE.Animation)
			//{
			//	titleWidth = width - (height + 2);
			//	isShowHideBtn = true;
			//}

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));

			GUILayout.Space(5);

			//GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
			//guiStyle_Box.normal.textColor = Color.white;
			//guiStyle_Box.alignment = TextAnchor.MiddleCenter;
			//guiStyle_Box.margin = GUI.skin.label.margin;


			Color prevColor = GUI.backgroundColor;
			//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
			GUI.backgroundColor = apEditorUtil.ToggleBoxColor_Selected;

			GUILayout.Box(strTitle, apGUIStyleWrapper.I.Box_MiddleCenter_LabelMargin_WhiteColor, apGUILOFactory.I.Width(titleWidth), apGUILOFactory.I.Height(20));

			GUI.backgroundColor = prevColor;


			EditorGUILayout.EndHorizontal();
		}


		//-----------------------------------------------
		// 루트 유닛 UI
		//-----------------------------------------------
		

		private void Draw_Overall(int width, int height)
		{
			//GUILayout.Box("Overall", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Overall", width);
			//EditorGUILayout.Space();

			apRootUnit rootUnit = RootUnit;
			if (rootUnit == null)
			{
				SelectNone();
				return;
			}

			Color prevColor = GUI.backgroundColor;

			//Setting / Capture Tab
			bool isRootUnitTab_Setting = (Editor._rootUnitEditMode == apEditor.ROOTUNIT_EDIT_MODE.Setting);
			bool isRootUnitTab_Capture = (Editor._rootUnitEditMode == apEditor.ROOTUNIT_EDIT_MODE.Capture);

			int subTabWidth = (width / 2) - 5;
			int subTabHeight = 24;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));
			GUILayout.Space(5);

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Setting), 1, Editor.GetUIWord(UIWORD.Setting), isRootUnitTab_Setting, true, subTabWidth, subTabHeight, apStringFactory.I.SettingsOfRootUnit))//"Settings of Root Unit"
			{
				if (!isRootUnitTab_Setting)
				{
					Editor._rootUnitEditMode = apEditor.ROOTUNIT_EDIT_MODE.Setting;
				}
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Tab), 1, Editor.GetUIWord(UIWORD.Capture), isRootUnitTab_Capture, true, subTabWidth, subTabHeight, apStringFactory.I.CapturingTheScreenShot))//"Capturing the screenshot"
			{
				if (!isRootUnitTab_Capture)
				{
					if (apVersion.I.IsDemo)
					{
						//추가 : 데모 버전일 때에는 Capture 기능을 사용할 수 없다.
						EditorUtility.DisplayDialog(
										Editor.GetText(TEXT.DemoLimitation_Title),
										Editor.GetText(TEXT.DemoLimitation_Body),
										Editor.GetText(TEXT.Okay));
					}
					else
					{
						Editor._rootUnitEditMode = apEditor.ROOTUNIT_EDIT_MODE.Capture;
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			if (Editor._rootUnitEditMode == apEditor.ROOTUNIT_EDIT_MODE.Setting)
			{
				//1. Setting 메뉴
				//------------------------------------------------
				//1. 연결된 MeshGroup 설정 (+ 해제)
				apMeshGroup targetMeshGroup = rootUnit._childMeshGroup;

				if (_strWrapper_64 == null)
				{
					_strWrapper_64 = new apStringWrapper(64);
				}
				_strWrapper_64.Clear();

				//string strMeshGroupName = "";
				Color bgColor = Color.black;
				if (targetMeshGroup != null)
				{
					//strMeshGroupName = "[" + targetMeshGroup._name + "]";
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
					_strWrapper_64.Append(targetMeshGroup._name, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

					bgColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				}
				else
				{
					//strMeshGroupName = "Error! No MeshGroup Linked";

					_strWrapper_64.Append(apStringFactory.I.ErrorNoMeshGroupLinked, true);

					bgColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				}
				GUI.backgroundColor = bgColor;

				//GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
				//guiStyleBox.alignment = TextAnchor.MiddleCenter;
				//guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);

				//2. 애니메이션 제어

				apAnimClip curAnimClip = RootUnitAnimClip;
				bool isAnimClipAvailable = (curAnimClip != null);


				Texture2D icon_FirstFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_FirstFrame);
				Texture2D icon_PrevFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_PrevFrame);

				Texture2D icon_NextFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_NextFrame);
				Texture2D icon_LastFrame = Editor.ImageSet.Get(apImageSet.PRESET.Anim_LastFrame);

				Texture2D icon_PlayPause = null;
				if (curAnimClip != null)
				{
					if (curAnimClip.IsPlaying_Editor) { icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Pause); }
					else { icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play); }
				}
				else
				{
					icon_PlayPause = Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play);
				}

				int btnSize = 30;
				int btnWidth_Play = 45;
				int btnWidth_PrevNext = 35;
				int btnWidth_FirstLast = (width - (btnWidth_Play + btnWidth_PrevNext * 2 + 4 * 3 + 5)) / 2;
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(btnSize));
				GUILayout.Space(2);
				if (apEditorUtil.ToggledButton_2Side(icon_FirstFrame, false, isAnimClipAvailable, btnWidth_FirstLast, btnSize))
				{
					if (curAnimClip != null)
					{
						curAnimClip.SetFrame_Editor(curAnimClip.StartFrame);
						curAnimClip.Pause_Editor();
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_PrevFrame, false, isAnimClipAvailable, btnWidth_PrevNext, btnSize))
				{
					if (curAnimClip != null)
					{
						int prevFrame = curAnimClip.CurFrame - 1;
						if (prevFrame < curAnimClip.StartFrame && curAnimClip.IsLoop)
						{
							prevFrame = curAnimClip.EndFrame;
						}
						curAnimClip.SetFrame_Editor(prevFrame);
						curAnimClip.Pause_Editor();
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_PlayPause, false, isAnimClipAvailable, btnWidth_Play, btnSize))
				{
					if (curAnimClip != null)
					{
						if (curAnimClip.IsPlaying_Editor)
						{
							curAnimClip.Pause_Editor();
						}
						else
						{
							if (curAnimClip.CurFrame == curAnimClip.EndFrame &&
								!curAnimClip.IsLoop)
							{
								curAnimClip.SetFrame_Editor(curAnimClip.StartFrame);
							}

							curAnimClip.Play_Editor();
						}
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.


					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_NextFrame, false, isAnimClipAvailable, btnWidth_PrevNext, btnSize))
				{
					if (curAnimClip != null)
					{
						int nextFrame = curAnimClip.CurFrame + 1;
						if (nextFrame > curAnimClip.EndFrame && curAnimClip.IsLoop)
						{
							nextFrame = curAnimClip.StartFrame;
						}
						curAnimClip.SetFrame_Editor(nextFrame);
						curAnimClip.Pause_Editor();
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
				}
				if (apEditorUtil.ToggledButton_2Side(icon_LastFrame, false, isAnimClipAvailable, btnWidth_FirstLast, btnSize))
				{
					if (curAnimClip != null)
					{
						curAnimClip.SetFrame_Editor(curAnimClip.EndFrame);
						curAnimClip.Pause_Editor();
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
				}

				EditorGUILayout.EndHorizontal();

				int curFrame = 0;
				int startFrame = 0;
				int endFrame = 10;
				if (curAnimClip != null)
				{
					curFrame = curAnimClip.CurFrame;
					startFrame = curAnimClip.StartFrame;
					endFrame = curAnimClip.EndFrame;
				}
				int sliderFrame = EditorGUILayout.IntSlider(curFrame, startFrame, endFrame, apGUILOFactory.I.Width(width));
				if (sliderFrame != curFrame)
				{
					if (curAnimClip != null)
					{
						curAnimClip.SetFrame_Editor(sliderFrame);
						curAnimClip.Pause_Editor();
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
				}

				GUILayout.Space(5);

				//추가 : 자동 플레이하는 AnimClip을 선택한다.
				bool isAutoPlayAnimClip = false;
				if (curAnimClip != null)
				{
					isAutoPlayAnimClip = (_portrait._autoPlayAnimClipID == curAnimClip._uniqueID);
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.AutoPlayEnabled), Editor.GetUIWord(UIWORD.AutoPlayDisabled), isAutoPlayAnimClip, curAnimClip != null, width, 25))//"Auto Play Enabled", "Auto Play Disabled"
				{
					if (curAnimClip != null)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Portrait_SettingChanged, 
															Editor, 
															_portrait, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						if (_portrait._autoPlayAnimClipID == curAnimClip._uniqueID)
						{
							//선택됨 -> 선택 해제
							_portrait._autoPlayAnimClipID = -1;

						}
						else
						{
							//선택 해제 -> 선택
							_portrait._autoPlayAnimClipID = curAnimClip._uniqueID;
						}


					}

				}


				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);

				//3. 애니메이션 리스트
				List<apAnimClip> subAnimClips = RootUnitAnimClipList;
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AnimationClips), apGUILOFactory.I.Width(width));//"Animation Clips"
				GUILayout.Space(5);
				if (subAnimClips != null && subAnimClips.Count > 0)
				{
					apAnimClip nextSelectedAnimClip = null;


					Rect lastRect = GUILayoutUtility.GetLastRect();

					int scrollWidth = width - 20;

					//Texture2D icon_Anim = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

					GUIStyle curGUIStyle = null;//최적화된 코드
					for (int i = 0; i < subAnimClips.Count; i++)
					{
						//GUIStyle curGUIStyle = guiNone;
						curGUIStyle = null;

						apAnimClip subAnimClip = subAnimClips[i];
						if (subAnimClip == curAnimClip)
						{
							lastRect = GUILayoutUtility.GetLastRect();

							//if (EditorGUIUtility.isProSkin)
							//{
							//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
							//}
							//else
							//{
							//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
							//}

							//int offsetHeight = 20 + 3;
							int offsetHeight = 1 + 3;
							if (i == 0)
							{
								offsetHeight = 4 + 3;
							}

							//GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 24), apStringFactory.I.None);
							//GUI.backgroundColor = prevColor;

							//변경 v1.4.2
							apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + offsetHeight, scrollWidth + 35 - 2, 24, apEditorUtil.UNIT_BG_STYLE.Main);

							//curGUIStyle = guiSelected;
							curGUIStyle = apGUIStyleWrapper.I.None_White2Cyan;
						}
						else
						{
							curGUIStyle = apGUIStyleWrapper.I.None_LabelColor;
						}

						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(scrollWidth - 5));
						GUILayout.Space(5);

						if (_guiContent_Overall_SelectedAnimClp == null)
						{
							_guiContent_Overall_SelectedAnimClp = new apGUIContentWrapper();
							_guiContent_Overall_SelectedAnimClp.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation));
						}
						_guiContent_Overall_SelectedAnimClp.ClearText(false);
						_guiContent_Overall_SelectedAnimClp.AppendSpaceText(1, false);
						_guiContent_Overall_SelectedAnimClp.AppendText(subAnimClip._name, true);


						//변경
						if (GUILayout.Button(_guiContent_Overall_SelectedAnimClp.Content,
												curGUIStyle,
												apGUILOFactory.I.Width(scrollWidth - 5), apGUILOFactory.I.Height(24)))
						{
							nextSelectedAnimClip = subAnimClip;
						}
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(4);

					}

					if (nextSelectedAnimClip != null)
					{
						for (int i = 0; i < Editor._portrait._animClips.Count; i++)
						{
							Editor._portrait._animClips[i]._isSelectedInEditor = false;
						}

						_curRootUnitAnimClip = nextSelectedAnimClip;
						_curRootUnitAnimClip.LinkEditor(Editor._portrait);
						_curRootUnitAnimClip.RefreshTimelines(null, null);//<<모든 타임라인 Refresh
						_curRootUnitAnimClip.SetFrame_Editor(_curRootUnitAnimClip.StartFrame);
						_curRootUnitAnimClip.Pause_Editor();

						_curRootUnitAnimClip._isSelectedInEditor = true;


						//통계 재계산 요청
						SetStatisticsRefresh();

						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.

						//Debug.Log("Select Root Unit Anim Clip : " + _curRootUnitAnimClip._name);
					}
				}



				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(20);
				//MainMesh에서 해제

				if(_guiContent_Overall_Unregister == null)
				{
					_guiContent_Overall_Unregister = new apGUIContentWrapper();
				}
				_guiContent_Overall_Unregister.ClearText(false);
				_guiContent_Overall_Unregister.AppendSpaceText(2, false);
				_guiContent_Overall_Unregister.AppendText(Editor.GetUIWord(UIWORD.UnregistRootUnit), true);
				_guiContent_Overall_Unregister.SetImage(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
				

				if (GUILayout.Button(	_guiContent_Overall_Unregister.Content,
										apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))//"Unregist Root Unit"
				{
					//Debug.LogError("TODO : MainMeshGroup 해제");
					apMeshGroup targetRootMeshGroup = rootUnit._childMeshGroup;
					if (targetRootMeshGroup != null)
					{
						apEditorUtil.SetRecord_PortraitMeshGroup(	apUndoGroupData.ACTION.Portrait_SetMeshGroup, 
																	Editor, 
																	_portrait, 
																	targetRootMeshGroup, 
																	//null, 
																	false, 
																	true,
																	apEditorUtil.UNDO_STRUCT.StructChanged);

						_portrait._mainMeshGroupIDList.Remove(targetRootMeshGroup._uniqueID);
						_portrait._mainMeshGroupList.Remove(targetRootMeshGroup);

						_portrait._rootUnits.Remove(rootUnit);

						SelectNone();

						Editor.RefreshControllerAndHierarchy(false);
						Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.RootUnit, true);
					}
				}
			}
			else
			{
				//2. Capture 메뉴
				//-------------------------------------------

				//>>여기서부터
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Thumbnail),
												1, Editor.GetUIWord(UIWORD.CaptureTabThumbnail),
												Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail, true, subTabWidth, subTabHeight,
												apStringFactory.I.MakeAThumbnail))//"Make a Thumbnail"
				{
					//"Thumbnail"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail;
				}

				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Image),
												1, Editor.GetUIWord(UIWORD.CaptureTabScreenshot),
												Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot, true, subTabWidth, subTabHeight,
												apStringFactory.I.MakeAScreenshot))//"Make a Screenshot"
				{
					//"Screen Shot"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot;
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));

				GUILayout.Space(5);

				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_GIF),
												1, Editor.GetUIWord(UIWORD.CaptureTabGIFAnim),
												Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation, true, subTabWidth, subTabHeight,
												apStringFactory.I.MakeAGIFAnimation))//"Make a GIF Animation"
				{
					//"GIF Anim"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation;
				}

				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Capture_Sprite),
												1, Editor.GetUIWord(UIWORD.CaptureTabSpritesheet),
												Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet, true, subTabWidth, subTabHeight,
												apStringFactory.I.MakeSpriteSheets))//"Make Spritesheets"
				{
					//"Spritesheet"
					Editor._rootUnitCaptureMode = apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet;
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);


				int settingWidth_Label = 80;
				int settingWidth_Value = width - (settingWidth_Label + 8);

				//각 캡쳐별로 설정을 한다.
				//공통 설정도 있고 아닌 경우도 있다.

				//Setting
				//------------------------
				//EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Setting));//"Setting"
				//GUILayout.Space(5);

				//Position
				//------------------------
				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Position));//"Position"

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(apStringFactory.I.X, apGUILOFactory.I.Width(settingWidth_Label));
				int posX = EditorGUILayout.DelayedIntField(Editor._captureFrame_PosX, apGUILOFactory.I.Width(settingWidth_Value));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(apStringFactory.I.Y, apGUILOFactory.I.Width(settingWidth_Label));
				int posY = EditorGUILayout.DelayedIntField(Editor._captureFrame_PosY, apGUILOFactory.I.Width(settingWidth_Value));
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				//Capture Size
				//------------------------
				//Thumbnail인 경우 Width만 설정한다. (Height는 자동 계산)
				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_CaptureSize));//"Capture Size"

				//Src Width
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				//"Width"
				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), apGUILOFactory.I.Width(settingWidth_Label));
				int srcSizeWidth = EditorGUILayout.DelayedIntField(Editor._captureFrame_SrcWidth, apGUILOFactory.I.Width(settingWidth_Value));
				EditorGUILayout.EndHorizontal();


				int srcSizeHeight = Editor._captureFrame_SrcHeight;
				//Src Height : Tumbnail이 아닌 경우만
				if (Editor._rootUnitCaptureMode != apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail)
				{
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), apGUILOFactory.I.Width(settingWidth_Label));//"Height"

					srcSizeHeight = EditorGUILayout.DelayedIntField(Editor._captureFrame_SrcHeight, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();
				}

				if (srcSizeWidth < 8) { srcSizeWidth = 8; }
				if (srcSizeHeight < 8) { srcSizeHeight = 8; }

				GUILayout.Space(5);

				//File Size
				//-------------------------------
				int dstSizeWidth = Editor._captureFrame_DstWidth;
				int dstSizeHeight = Editor._captureFrame_DstHeight;
				int spriteUnitSizeWidth = Editor._captureFrame_SpriteUnitWidth;
				int spriteUnitSizeHeight = Editor._captureFrame_SpriteUnitHeight;

				apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE spritePackImageWidth = Editor._captureSpritePackImageWidth;
				apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE spritePackImageHeight = Editor._captureSpritePackImageHeight;
				apEditor.CAPTURE_SPRITE_TRIM_METHOD spriteTrimSize = Editor._captureSpriteTrimSize;
				int spriteMargin = Editor._captureFrame_SpriteMargin;
				bool isPhysicsEnabled = Editor._captureFrame_IsPhysics;

				//Screenshot / GIF Animation은 Dst Image Size를 결정한다.
				if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot ||
					Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation)
				{
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_ImageSize));//"Image Size"

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), apGUILOFactory.I.Width(settingWidth_Label));
					dstSizeWidth = EditorGUILayout.DelayedIntField(Editor._captureFrame_DstWidth, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					//"Height"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), apGUILOFactory.I.Width(settingWidth_Label));
					dstSizeHeight = EditorGUILayout.DelayedIntField(Editor._captureFrame_DstHeight, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(5);
				}
				else if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
				{
					//Sprite Sheet는 Capture Unit과 Pack Image 사이즈, 압축 방식을 결정한다.
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ImageSizePerFrame));//"Image Size"
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), apGUILOFactory.I.Width(settingWidth_Label));
					spriteUnitSizeWidth = EditorGUILayout.DelayedIntField(Editor._captureFrame_SpriteUnitWidth, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					//"Height"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), apGUILOFactory.I.Width(settingWidth_Label));
					spriteUnitSizeHeight = EditorGUILayout.DelayedIntField(Editor._captureFrame_SpriteUnitHeight, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(5);



					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SizeofSpritesheet));//"Size of Sprite Sheet"
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Width), apGUILOFactory.I.Width(settingWidth_Label));
					spritePackImageWidth = (apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorGUILayout.Popup((int)Editor._captureSpritePackImageWidth, _captureSpritePackSizeNames, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					//"Height"
					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_Height), apGUILOFactory.I.Width(settingWidth_Label));
					spritePackImageHeight = (apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE)EditorGUILayout.Popup((int)Editor._captureSpritePackImageHeight, _captureSpritePackSizeNames, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

					if ((int)spritePackImageWidth < 0) { spritePackImageWidth = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256; }
					else if ((int)spritePackImageWidth > 4) { spritePackImageWidth = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096; }

					if ((int)spritePackImageHeight < 0) { spritePackImageHeight = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256; }
					else if ((int)spritePackImageHeight > 4) { spritePackImageHeight = apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096; }

					GUILayout.Space(5);

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteSizeCompression));//"Image size compression method"
					spriteTrimSize = (apEditor.CAPTURE_SPRITE_TRIM_METHOD)EditorGUILayout.EnumPopup(Editor._captureSpriteTrimSize, apGUILOFactory.I.Width(width));

					GUILayout.Space(5);

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					//"Width"
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteMargin), apGUILOFactory.I.Width(settingWidth_Label));//"Margin"
					spriteMargin = EditorGUILayout.DelayedIntField(Editor._captureFrame_SpriteMargin, apGUILOFactory.I.Width(settingWidth_Value));
					EditorGUILayout.EndHorizontal();

				}



				//Color와 물리와 AspectRatio
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
				GUILayout.Space(5);

				EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_BGColor), apGUILOFactory.I.Width(settingWidth_Label));//"BG Color"
				Color prevCaptureColor = Editor._captureFrame_Color;
				try
				{
					Editor._captureFrame_Color = EditorGUILayout.ColorField(Editor._captureFrame_Color, apGUILOFactory.I.Width(settingWidth_Value));
				}
				catch (Exception) { }
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation ||
					Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
				{
					//GIF, Spritesheet인 경우 물리 효과를 정해야 한다.
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
					GUILayout.Space(5);

					EditorGUILayout.LabelField(Editor.GetText(TEXT.DLG_CaptureIsPhysics), apGUILOFactory.I.Width(width - (10 + 30)));
					isPhysicsEnabled = EditorGUILayout.Toggle(Editor._captureFrame_IsPhysics, apGUILOFactory.I.Width(30));
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(5);
				}

				GUILayout.Space(5);

				if (Editor._rootUnitCaptureMode != apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail)
				{
					//Thumbnail이 아니라면 Aspect Ratio가 중요하다
					//Aspect Ratio
					if (apEditorUtil.ToggledButton_2Side(Editor.GetText(TEXT.DLG_FixedAspectRatio), Editor.GetText(TEXT.DLG_NotFixedAspectRatio), Editor._isCaptureAspectRatioFixed, true, width, 20))
					{
						Editor._isCaptureAspectRatioFixed = !Editor._isCaptureAspectRatioFixed;

						if (Editor._isCaptureAspectRatioFixed)
						{
							//AspectRatio를 굳혔다.
							//Dst계열 변수를 Src에 맞춘다.
							//Height를 고정, Width를 맞춘다.
							if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
							{
								//Spritesheet라면 Unit 사이즈를 변경
								Editor._captureFrame_SpriteUnitWidth = apEditorUtil.GetAspectRatio_Width(
																							Editor._captureFrame_SpriteUnitHeight,
																							Editor._captureFrame_SrcWidth,
																							Editor._captureFrame_SrcHeight);
								spriteUnitSizeWidth = Editor._captureFrame_SpriteUnitWidth;
							}
							else
							{
								//Screenshot과 GIF Animation이라면 Dst 사이즈를 변경
								Editor._captureFrame_DstWidth = apEditorUtil.GetAspectRatio_Width(
																							Editor._captureFrame_DstHeight,
																							Editor._captureFrame_SrcWidth,
																							Editor._captureFrame_SrcHeight);
								dstSizeWidth = Editor._captureFrame_DstWidth;
							}

						}

						Editor.SaveEditorPref();
						apEditorUtil.ReleaseGUIFocus();
					}

					GUILayout.Space(5);
				}




				//AspectRatio를 맞추어보자
				if (Editor._isCaptureAspectRatioFixed)
				{
					if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot ||
						Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation)
					{
						//Screenshot / GIFAnimation은 Src, Dst를 서로 맞춘다.
						if (srcSizeWidth != Editor._captureFrame_SrcWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Width
							dstSizeWidth = apEditorUtil.GetAspectRatio_Width(dstSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (srcSizeHeight != Editor._captureFrame_SrcHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Height
							dstSizeHeight = apEditorUtil.GetAspectRatio_Height(dstSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (dstSizeWidth != Editor._captureFrame_DstWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							dstSizeHeight = apEditorUtil.GetAspectRatio_Height(dstSizeWidth, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
							//>> Src도 바꾸다 => Width
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
						}
						else if (dstSizeHeight != Editor._captureFrame_DstHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							dstSizeWidth = apEditorUtil.GetAspectRatio_Width(dstSizeHeight, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
							//>> Dst도 바꾸자 => Height
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_DstWidth, Editor._captureFrame_DstHeight);
						}
					}
					else if (Editor._rootUnitCaptureMode == apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet)
					{
						//Sprite sheet는 Src, Unit을 맞춘다.
						if (srcSizeWidth != Editor._captureFrame_SrcWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Width
							spriteUnitSizeWidth = apEditorUtil.GetAspectRatio_Width(spriteUnitSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (srcSizeHeight != Editor._captureFrame_SrcHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
							//>> Dst도 바꾸자 => Height
							spriteUnitSizeHeight = apEditorUtil.GetAspectRatio_Height(spriteUnitSizeWidth, Editor._captureFrame_SrcWidth, Editor._captureFrame_SrcHeight);
						}
						else if (spriteUnitSizeWidth != Editor._captureFrame_SpriteUnitWidth)
						{
							//Width가 바뀌었다. => Height를 맞추자
							spriteUnitSizeHeight = apEditorUtil.GetAspectRatio_Height(spriteUnitSizeWidth, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
							//>> Src도 바꾸다 => Width
							srcSizeWidth = apEditorUtil.GetAspectRatio_Width(srcSizeHeight, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
						}
						else if (spriteUnitSizeHeight != Editor._captureFrame_SpriteUnitHeight)
						{
							//Height가 바뀌었다. => Width를 맞추자
							spriteUnitSizeWidth = apEditorUtil.GetAspectRatio_Width(spriteUnitSizeHeight, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
							//>> Dst도 바꾸자 => Height
							srcSizeHeight = apEditorUtil.GetAspectRatio_Height(srcSizeWidth, Editor._captureFrame_SpriteUnitWidth, Editor._captureFrame_SpriteUnitHeight);
						}
					}
				}

				if (posX != Editor._captureFrame_PosX
					|| posY != Editor._captureFrame_PosY
					|| srcSizeWidth != Editor._captureFrame_SrcWidth
					|| srcSizeHeight != Editor._captureFrame_SrcHeight
					|| dstSizeWidth != Editor._captureFrame_DstWidth
					|| dstSizeHeight != Editor._captureFrame_DstHeight
					|| spriteUnitSizeWidth != Editor._captureFrame_SpriteUnitWidth
					|| spriteUnitSizeHeight != Editor._captureFrame_SpriteUnitHeight
					|| spritePackImageWidth != Editor._captureSpritePackImageWidth
					|| spritePackImageHeight != Editor._captureSpritePackImageHeight
					|| spriteTrimSize != Editor._captureSpriteTrimSize
					|| spriteMargin != Editor._captureFrame_SpriteMargin
					|| isPhysicsEnabled != Editor._captureFrame_IsPhysics
					)
				{
					Editor._captureFrame_PosX = posX;
					Editor._captureFrame_PosY = posY;

					if (srcSizeWidth < 10) { srcSizeWidth = 10; }
					if (srcSizeHeight < 10) { srcSizeHeight = 10; }
					Editor._captureFrame_SrcWidth = srcSizeWidth;
					Editor._captureFrame_SrcHeight = srcSizeHeight;

					if (dstSizeWidth < 10) { dstSizeWidth = 10; }
					if (dstSizeHeight < 10) { dstSizeHeight = 10; }
					Editor._captureFrame_DstWidth = dstSizeWidth;
					Editor._captureFrame_DstHeight = dstSizeHeight;

					if (spriteUnitSizeWidth < 10) { spriteUnitSizeWidth = 10; }
					if (spriteUnitSizeHeight < 10) { spriteUnitSizeHeight = 10; }
					Editor._captureFrame_SpriteUnitWidth = spriteUnitSizeWidth;
					Editor._captureFrame_SpriteUnitHeight = spriteUnitSizeHeight;

					Editor._captureSpritePackImageWidth = spritePackImageWidth;
					Editor._captureSpritePackImageHeight = spritePackImageHeight;
					Editor._captureSpriteTrimSize = spriteTrimSize;

					if (spriteMargin < 0) { spriteMargin = 0; }
					Editor._captureFrame_SpriteMargin = spriteMargin;

					Editor._captureFrame_IsPhysics = isPhysicsEnabled;

					Editor.SaveEditorPref();
					apEditorUtil.ReleaseGUIFocus();
				}

				if (Mathf.Abs(prevCaptureColor.r - Editor._captureFrame_Color.r) > 0.01f
					|| Mathf.Abs(prevCaptureColor.g - Editor._captureFrame_Color.g) > 0.01f
					|| Mathf.Abs(prevCaptureColor.b - Editor._captureFrame_Color.b) > 0.01f
					|| Mathf.Abs(prevCaptureColor.a - Editor._captureFrame_Color.a) > 0.01f)
				{
					_editor.SaveEditorPref();
					//색상은 GUIFocus를 null로 만들면 안되기에..
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				if (_guiContent_Overall_MakeThumbnail == null)
				{
					_guiContent_Overall_MakeThumbnail = new apGUIContentWrapper();
					_guiContent_Overall_MakeThumbnail.ClearText(false);
					_guiContent_Overall_MakeThumbnail.AppendSpaceText(1, false);
					_guiContent_Overall_MakeThumbnail.AppendText(_editor.GetText(TEXT.DLG_MakeThumbnail), true);
					_guiContent_Overall_MakeThumbnail.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportThumb));
				}

				if (_guiContent_Overall_TakeAScreenshot == null)
				{
					_guiContent_Overall_TakeAScreenshot = new apGUIContentWrapper();
					_guiContent_Overall_TakeAScreenshot.ClearText(false);
					_guiContent_Overall_TakeAScreenshot.AppendSpaceText(1, false);
					_guiContent_Overall_TakeAScreenshot.AppendText(_editor.GetText(TEXT.DLG_TakeAScreenshot), true);
					_guiContent_Overall_TakeAScreenshot.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportScreenshot));
				}

				switch (Editor._rootUnitCaptureMode)
				{
					case apEditor.ROOTUNIT_CAPTURE_MODE.Thumbnail:
						{
							//1. 썸네일 캡쳐
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ThumbnailCapture));//"Thumbnail Capture"
							GUILayout.Space(5);
							string prev_ImageFilePath = _editor._portrait._imageFilePath_Thumbnail;

							//Preview 이미지
							GUILayout.Box(_editor._portrait._thumbnailImage, GUI.skin.label, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(width / 2));

							//File Path
							GUILayout.Space(5);
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_FilePath));//"File Path"
							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
							GUILayout.Space(5);

							//파일 경로 > 

							_editor._portrait._imageFilePath_Thumbnail = EditorGUILayout.TextField(_editor._portrait._imageFilePath_Thumbnail, apGUILOFactory.I.Width(width - (68)));
							if (GUILayout.Button(_editor.GetText(TEXT.DLG_Change), apGUILOFactory.I.Width(60)))//"Change"
							{
								string fileName = EditorUtility.SaveFilePanelInProject("Thumbnail File Path", _editor._portrait.name + "_Thumb.png", "png", "Please Enter a file name to save Thumbnail to");
								if (!string.IsNullOrEmpty(fileName))
								{
									_editor._portrait._imageFilePath_Thumbnail = apUtil.ConvertEscapeToPlainText(fileName);//변경 21.7.3 : 이스케이프 문자 삭제
									apEditorUtil.ReleaseGUIFocus();
								}
							}
							EditorGUILayout.EndHorizontal();

							if (!_editor._portrait._imageFilePath_Thumbnail.Equals(prev_ImageFilePath))
							{
								//경로가 바뀌었다. -> 저장
								apEditorUtil.SetEditorDirty();

							}

							//썸네일 만들기 버튼
							if (GUILayout.Button(_guiContent_Overall_MakeThumbnail.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
							{
								if (string.IsNullOrEmpty(_editor._portrait._imageFilePath_Thumbnail))
								{
									//EditorUtility.DisplayDialog("Thumbnail Creating Failed", "File Name is Empty", "Close");
									EditorUtility.DisplayDialog(_editor.GetText(TEXT.ThumbCreateFailed_Title),
																	_editor.GetText(TEXT.ThumbCreateFailed_Body_NoFile),
																	_editor.GetText(TEXT.Close)
																	);
								}
								else
								{
									//RequestExport(EXPORT_TYPE.Thumbnail);//<<이전 코드
									StartMakeThumbnail();//<<새로운 코드

								}
							}
						}
						break;

					case apEditor.ROOTUNIT_CAPTURE_MODE.ScreenShot:
						{
							//2. 스크린샷 캡쳐
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_ScreenshotCapture));//"Screenshot Capture"
							GUILayout.Space(5);

							if (GUILayout.Button(_guiContent_Overall_TakeAScreenshot.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
							{
								if (CheckComputeShaderSupportedForScreenCapture())//추가 : 캡쳐 처리 가능한지 확인
								{
									StartTakeScreenShot();
								}
							}
						}
						break;

					case apEditor.ROOTUNIT_CAPTURE_MODE.GIFAnimation:
						{
							//3. GIF 애니메이션 캡쳐
							EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_GIFAnimation));//"GIF Animation"
							GUILayout.Space(5);

							List<apAnimClip> subAnimClips = RootUnitAnimClipList;

							string animName = _editor.GetText(TEXT.DLG_NotAnimation);
							Color animBGColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
							if (_captureSelectedAnimClip != null)
							{
								animName = _captureSelectedAnimClip._name;
								animBGColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);
							}

							Color prevGUIColor = GUI.backgroundColor;
							//GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
							//guiStyleBox.alignment = TextAnchor.MiddleCenter;
							//guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

							GUI.backgroundColor = animBGColor;

							GUILayout.Box(animName, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

							GUI.backgroundColor = prevGUIColor;

							GUILayout.Space(5);


							bool isDrawProgressBar = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_GIF_ProgressBar);//"Capture GIF ProgressBar"
							bool isDrawGIFAnimClips = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_GIF_Clips);//"Capture GIF Clips"
							try
							{
								if (_captureMode != CAPTURE_MODE.None)
								{
									if (isDrawProgressBar)
									{
										//캡쳐 중에는 다른 UI 제어 불가

										if (_captureMode == CAPTURE_MODE.Capturing_GIF_Animation
											|| _captureMode == CAPTURE_MODE.Capturing_MP4_Animation)
										{
											EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteGIFWait));//"Please wait until finished. - TODO"

											float barRatio = Editor.SeqExporter.ProcessRatio;
											string barLabel = (int)(Mathf.Clamp01(barRatio) * 100.0f) + " %";

											string strTitleText = (_captureMode == CAPTURE_MODE.Capturing_GIF_Animation) ? "Exporting to GIF" : "Exporting to MP4";

											bool isCancel = EditorUtility.DisplayCancelableProgressBar(strTitleText, "Processing... " + barLabel, barRatio);
											_captureGIF_IsProgressDialog = true;
											if (isCancel)
											{
												//취소 버튼을 눌렀다.
												Editor.SeqExporter.RequestStop();
												apEditorUtil.ReleaseGUIFocus();
											}
										}
									}

								}
								else
								{
									if (_captureGIF_IsProgressDialog)
									{
										EditorUtility.ClearProgressBar();
										_captureGIF_IsProgressDialog = false;
									}

									if (isDrawGIFAnimClips)
									{
										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.CaptureScreenPosZoom));//"Screen Position and Zoom"
										GUILayout.Space(5);

										//화면 위치
										int width_ScreenPos = ((width - (10 + 30)) / 2) - 20;
										GUILayout.Space(5);
										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
										GUILayout.Space(4);
										EditorGUILayout.LabelField(apStringFactory.I.X, apGUILOFactory.I.Width(15));
										Editor._captureSprite_ScreenPos.x = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.x, apGUILOFactory.I.Width(width_ScreenPos));
										EditorGUILayout.LabelField(apStringFactory.I.Y, apGUILOFactory.I.Width(15));
										Editor._captureSprite_ScreenPos.y = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.y, apGUILOFactory.I.Width(width_ScreenPos));
										//GUIStyle guiStyle_SetBtn = new GUIStyle(GUI.skin.button);
										//guiStyle_SetBtn.margin = GUI.skin.textField.margin;

										if (GUILayout.Button(apStringFactory.I.Set, apGUIStyleWrapper.I.Button_TextFieldMargin, apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(18)))//"Set"
										{
											Editor._scroll_CenterWorkSpace = Editor._captureSprite_ScreenPos * 0.01f;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										EditorGUILayout.EndHorizontal();
										//Zoom

										Rect lastRect = GUILayoutUtility.GetLastRect();
										lastRect.x += 5;
										lastRect.y += 25;
										lastRect.width = width - (30 + 10 + 60 + 10);
										lastRect.height = 20;

										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
										GUILayout.Space(6);
										GUILayout.Space(width - (30 + 10 + 60));
										//Editor._captureSprite_ScreenZoom = EditorGUILayout.IntSlider(Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1, GUILayout.Width(width - (30 + 10 + 40)));
										float fScreenZoom = GUI.HorizontalSlider(lastRect, Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1);
										Editor._captureSprite_ScreenZoom = Mathf.Clamp((int)fScreenZoom, 0, Editor._zoomListX100.Length - 1);

										EditorGUILayout.LabelField(Editor._zoomListX100_Label[Editor._captureSprite_ScreenZoom], apGUILOFactory.I.Width(60));
										if (GUILayout.Button(apStringFactory.I.Set, apGUIStyleWrapper.I.Button_TextFieldMargin, apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(18)))//"Set"
										{
											Editor._iZoomX100 = Editor._captureSprite_ScreenZoom;
											if (Editor._iZoomX100 < 0)
											{
												Editor._iZoomX100 = 0;
											}
											else if (Editor._iZoomX100 >= Editor._zoomListX100.Length)
											{
												Editor._iZoomX100 = Editor._zoomListX100.Length - 1;
											}
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										EditorGUILayout.EndHorizontal();

										//"Focus To Center"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureMoveToCenter), Editor.GetUIWord(UIWORD.CaptureMoveToCenter), false, true, width, 20))
										{
											Editor._scroll_CenterWorkSpace = Vector2.zero;
											Editor._captureSprite_ScreenPos = Vector2.zero;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
										GUILayout.Space(4);

										if (_strWrapper_64 == null)
										{
											_strWrapper_64 = new apStringWrapper(64);
										}

										_strWrapper_64.Clear();
										_strWrapper_64.Append(Editor.GetUIWord(UIWORD.CaptureZoom), false);
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(apStringFactory.I.Minus, true);

										//"Zoom -"
										if (apEditorUtil.ToggledButton_2Side(_strWrapper_64.ToString(), _strWrapper_64.ToString(), false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100--;
											if (Editor._iZoomX100 < 0) { Editor._iZoomX100 = 0; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										_strWrapper_64.Clear();
										_strWrapper_64.Append(Editor.GetUIWord(UIWORD.CaptureZoom), false);
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(apStringFactory.I.Plus, true);

										//"Zoom +"
										if (apEditorUtil.ToggledButton_2Side(_strWrapper_64.ToString(), _strWrapper_64.ToString(), false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100++;
											if (Editor._iZoomX100 >= Editor._zoomListX100.Length) { Editor._iZoomX100 = Editor._zoomListX100.Length - 1; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
										}
										EditorGUILayout.EndHorizontal();


										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										//Quality의 Min : 0, Max : 246이다.

										//변경 11.4 : GIF 저장 퀄리티를 4개의 타입으로 나누고, Maximum 추가
										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
										GUILayout.Space(5);

										if (_strWrapper_64 == null)
										{
											_strWrapper_64 = new apStringWrapper(64);
										}

										_strWrapper_64.Clear();
										_strWrapper_64.Append(apStringFactory.I.GIF, false);
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Quality), true);

										EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(100));
										apEditor.CAPTURE_GIF_QUALITY gifQulity = (apEditor.CAPTURE_GIF_QUALITY)EditorGUILayout.EnumPopup(_editor._captureFrame_GIFQuality, apGUILOFactory.I.Width(width - (5 + 100 + 5)));

										if (gifQulity != _editor._captureFrame_GIFQuality)
										{
											_editor._captureFrame_GIFQuality = gifQulity;
											_editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										EditorGUILayout.EndHorizontal();

										GUILayout.Space(5);
										EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_LoopCount), apGUILOFactory.I.Width(width));//"Loop Count"
										int loopCount = EditorGUILayout.DelayedIntField(_editor._captureFrame_GIFSampleLoopCount, apGUILOFactory.I.Width(width));
										if (loopCount != _editor._captureFrame_GIFSampleLoopCount)
										{
											loopCount = Mathf.Clamp(loopCount, 1, 10);
											_editor._captureFrame_GIFSampleLoopCount = loopCount;
											_editor.SaveEditorPref();
										}

										GUILayout.Space(5);

										_strWrapper_64.Clear();
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(_editor.GetText(TEXT.DLG_TakeAGIFAnimation), true);

										//string strTakeAGIFAnimation = " " + _editor.GetText(TEXT.DLG_TakeAGIFAnimation);

										//"Take a GIF Animation", "Take a GIF Animation"
										if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportGIF), _strWrapper_64.ToString(), _strWrapper_64.ToString(), false, (_captureSelectedAnimClip != null), width, 30))
										{
											if (CheckComputeShaderSupportedForScreenCapture())//추가 : 캡쳐 처리 가능한지 확인
											{
												StartGIFAnimation();
											}
										}


#if UNITY_2017_4_OR_NEWER
										_strWrapper_64.Clear();
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(_editor.GetText(TEXT.DLG_ExportMP4), true);

										//string strTakeAMP4Animation = " " + _editor.GetText(TEXT.DLG_ExportMP4);

										if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportMP4), _strWrapper_64.ToString(), _strWrapper_64.ToString(), false, (_captureSelectedAnimClip != null), width, 30))
										{
											if (CheckComputeShaderSupportedForScreenCapture())//추가 : 캡쳐 처리 가능한지 확인
											{
												StartMP4Animation();
											}
										}
#endif

										GUILayout.Space(10);


										_strWrapper_64.Clear();
										_strWrapper_64.AppendSpace(2, false);
										_strWrapper_64.Append(_editor.GetText(TEXT.DLG_AnimationClips), true);

										//"Animation Clips"
										GUILayout.Button(_strWrapper_64.ToString(), apGUIStyleWrapper.I.None_LabelColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));//투명 버튼

										//애니메이션 클립 리스트를 만들어야 한다.
										if (subAnimClips.Count > 0)
										{

											if (_guiContent_Overall_AnimItem == null)
											{
												_guiContent_Overall_AnimItem = new apGUIContentWrapper();
												_guiContent_Overall_AnimItem.ClearText(true);
												_guiContent_Overall_AnimItem.SetImage(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation));
											}

											GUIStyle curGUIStyle = null;//최적화 코드

											apAnimClip nextSelectedAnimClip = null;
											for (int i = 0; i < subAnimClips.Count; i++)
											{
												apAnimClip animClip = subAnimClips[i];

												if (animClip == _captureSelectedAnimClip)
												{
													lastRect = GUILayoutUtility.GetLastRect();
													
													#region [미사용 코드]
													//prevCaptureColor = GUI.backgroundColor;

													//if (EditorGUIUtility.isProSkin)
													//{
													//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
													//}
													//else
													//{
													//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
													//}

													//GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width + 20, 20), apStringFactory.I.None);
													//GUI.backgroundColor = prevGUIColor; 
													#endregion

													//변경 v1.4.2
													apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + 20, width + 20 - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);

													curGUIStyle = apGUIStyleWrapper.I.None_White2Cyan;
												}
												else
												{
													curGUIStyle = apGUIStyleWrapper.I.None_LabelColor;
												}



												EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width - 50));
												GUILayout.Space(15);

												//이전
												//if (GUILayout.Button(new GUIContent(" " + animClip._name, iconImage), curGUIStyle, GUILayout.Width(width - 35), GUILayout.Height(20)))

												//변경
												_guiContent_Overall_AnimItem.ClearText(false);
												_guiContent_Overall_AnimItem.AppendSpaceText(1, false);
												_guiContent_Overall_AnimItem.AppendText(animClip._name, true);

												if (GUILayout.Button(_guiContent_Overall_AnimItem.Content, curGUIStyle, apGUILOFactory.I.Width(width - 35), apGUILOFactory.I.Height(20)))
												{
													nextSelectedAnimClip = animClip;
												}

												EditorGUILayout.EndHorizontal();
											}

											if (nextSelectedAnimClip != null)
											{
												for (int i = 0; i < _editor._portrait._animClips.Count; i++)
												{
													_editor._portrait._animClips[i]._isSelectedInEditor = false;
												}

												nextSelectedAnimClip.LinkEditor(_editor._portrait);
												nextSelectedAnimClip.RefreshTimelines(null, null);
												nextSelectedAnimClip.SetFrame_Editor(nextSelectedAnimClip.StartFrame);
												nextSelectedAnimClip.Pause_Editor();
												nextSelectedAnimClip._isSelectedInEditor = true;

												_captureSelectedAnimClip = nextSelectedAnimClip;

												_editor._portrait._animPlayManager.SetAnimClip_Editor(_captureSelectedAnimClip);

												Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
											}
										}
									}
								}
							}
							catch (Exception)
							{
								//Debug.LogError("GUI Exception : " + ex);
								//Debug.Log("Capture Mode : " + _captureMode);
								//Debug.Log("isDrawProgressBar : " + isDrawProgressBar);
								//Debug.Log("isDrawGIFAnimClips : " + isDrawGIFAnimClips);
								//Debug.Log("Event : " + Event.current.type);
							}

							Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_GIF_ProgressBar, _captureMode != CAPTURE_MODE.None);//"Capture GIF ProgressBar"
							Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_GIF_Clips, _captureMode == CAPTURE_MODE.None);//"Capture GIF Clips"
						}
						break;

					case apEditor.ROOTUNIT_CAPTURE_MODE.SpriteSheet:
						{
							//4. 스프라이트 시트 캡쳐
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteSheet));//"Sprite Sheet"
							GUILayout.Space(5);

							bool isDrawProgressBar = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_Spritesheet_ProgressBar);//"Capture Spritesheet ProgressBar"
							bool isDrawSpritesheetSettings = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_Spritesheet_Settings);//"Capture Spritesheet Settings"

							try
							{
								if (_captureMode != CAPTURE_MODE.None)
								{
									if (isDrawProgressBar)
									{
										//캡쳐 중에는 다른 UI 제어 불가

										if (_captureMode == CAPTURE_MODE.Capturing_Spritesheet)
										{
											EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SpriteGIFWait));//"Please wait until finished. - TODO"
																											   //Rect lastRect = GUILayoutUtility.GetLastRect();

											//Rect barRect = new Rect(lastRect.x + 10, lastRect.y + 30, width - 20, 20);
											//float barRatio = (float)(_captureGIF_CurAnimProcess) / (float)(_captureGIF_TotalAnimProcess);

											float barRatio = Editor.SeqExporter.ProcessRatio;
											string barLabel = (int)(Mathf.Clamp01(barRatio) * 100.0f) + " %";

											//EditorGUI.ProgressBar(barRect, barRatio, barLabel);
											bool isCancel = EditorUtility.DisplayCancelableProgressBar("Exporting to Sprite Sheet", "Processing... " + barLabel, barRatio);
											_captureGIF_IsProgressDialog = true;
											if (isCancel)
											{
												Editor.SeqExporter.RequestStop();
												apEditorUtil.ReleaseGUIFocus();
											}
										}
									}
								}
								else
								{
									if (_captureGIF_IsProgressDialog)
									{
										EditorUtility.ClearProgressBar();
										_captureGIF_IsProgressDialog = false;
									}

									if (isDrawSpritesheetSettings)
									{
										List<apAnimClip> subAnimClips = RootUnitAnimClipList;

										//그 전에 AnimClip 갱신부터
										if (!_captureSprite_IsAnimClipInit)
										{
											_captureSprite_AnimClips.Clear();
											_captureSprite_AnimClipFlags.Clear();
											for (int i = 0; i < subAnimClips.Count; i++)
											{
												_captureSprite_AnimClips.Add(subAnimClips[i]);
												_captureSprite_AnimClipFlags.Add(false);
											}

											_captureSprite_IsAnimClipInit = true;
										}

										Color prevGUIColor = GUI.backgroundColor;
										//GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
										//guiStyleBox.alignment = TextAnchor.MiddleCenter;
										//guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

										GUI.backgroundColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);

										if (_strWrapper_128 == null)
										{
											_strWrapper_128 = new apStringWrapper(128);
										}

										_strWrapper_128.Clear();
										_strWrapper_128.Append(Editor.GetUIWord(UIWORD.ExpectedNumSprites), false);
										_strWrapper_128.Append("\n", false);

										//string strNumOfSprites = Editor.GetUIWord(UIWORD.ExpectedNumSprites) + "\n";//"Expected number of sprites - TODO\n";

										int spriteTotalSize_X = 0;
										int spriteTotalSize_Y = 0;
										switch (Editor._captureSpritePackImageWidth)
										{
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256: spriteTotalSize_X = 256; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s512: spriteTotalSize_X = 512; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024: spriteTotalSize_X = 1024; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s2048: spriteTotalSize_X = 2048; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096: spriteTotalSize_X = 4096; break;
											default: spriteTotalSize_X = 256; break;
										}

										switch (Editor._captureSpritePackImageHeight)
										{
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s256: spriteTotalSize_Y = 256; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s512: spriteTotalSize_Y = 512; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s1024: spriteTotalSize_Y = 1024; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s2048: spriteTotalSize_Y = 2048; break;
											case apEditor.CAPTURE_SPRITE_PACK_IMAGE_SIZE.s4096: spriteTotalSize_Y = 4096; break;
											default: spriteTotalSize_Y = 256; break;
										}
										//X축 개수
										int numXOfSprite = -1;
										if (Editor._captureFrame_SpriteUnitWidth > 0 || Editor._captureFrame_SpriteUnitWidth < spriteTotalSize_X)
										{
											numXOfSprite = spriteTotalSize_X / Editor._captureFrame_SpriteUnitWidth;
										}

										//Y축 개수
										int numYOfSprite = -1;
										if (Editor._captureFrame_SpriteUnitHeight > 0 || Editor._captureFrame_SpriteUnitHeight < spriteTotalSize_Y)
										{
											numYOfSprite = spriteTotalSize_Y / Editor._captureFrame_SpriteUnitHeight;
										}
										if (numXOfSprite <= 0 || numYOfSprite <= 0)
										{
											//strNumOfSprites += Editor.GetUIWord(UIWORD.InvalidSpriteSizeSettings);//"Invalid size settings";

											_strWrapper_128.Append(Editor.GetUIWord(UIWORD.InvalidSpriteSizeSettings), true);

											GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
										}
										else
										{
											//strNumOfSprites += numXOfSprite + " X " + numYOfSprite;

											_strWrapper_128.Append(numXOfSprite, false);
											_strWrapper_128.AppendSpace(1, false);
											_strWrapper_128.Append(apStringFactory.I.X, false);
											_strWrapper_128.AppendSpace(1, false);
											_strWrapper_128.Append(numYOfSprite, true);
										}
										GUILayout.Box(_strWrapper_128.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40));

										GUI.backgroundColor = prevGUIColor;

										GUILayout.Space(5);



										//Export Format
										int width_ToggleLabel = width - (10 + 30);
										EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ExportMetaFile));//"Export Meta File - TODO"
										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
										GUILayout.Space(5);
										EditorGUILayout.LabelField(apStringFactory.I.XML, apGUILOFactory.I.Width(width_ToggleLabel));//"XML"
										bool isMetaXML = EditorGUILayout.Toggle(Editor._captureSpriteMeta_XML, apGUILOFactory.I.Width(30));
										EditorGUILayout.EndHorizontal();

										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
										GUILayout.Space(5);
										EditorGUILayout.LabelField(apStringFactory.I.JSON, apGUILOFactory.I.Width(width_ToggleLabel));//"JSON"
										bool isMetaJSON = EditorGUILayout.Toggle(Editor._captureSpriteMeta_JSON, apGUILOFactory.I.Width(30));
										EditorGUILayout.EndHorizontal();

										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
										GUILayout.Space(5);
										EditorGUILayout.LabelField(apStringFactory.I.TXT, apGUILOFactory.I.Width(width_ToggleLabel));//"TXT"
										bool isMetaTXT = EditorGUILayout.Toggle(Editor._captureSpriteMeta_TXT, apGUILOFactory.I.Width(30));
										EditorGUILayout.EndHorizontal();

										if (isMetaXML != Editor._captureSpriteMeta_XML
											|| isMetaJSON != Editor._captureSpriteMeta_JSON
											|| isMetaTXT != Editor._captureSpriteMeta_TXT
											)
										{
											Editor._captureSpriteMeta_XML = isMetaXML;
											Editor._captureSpriteMeta_JSON = isMetaJSON;
											Editor._captureSpriteMeta_TXT = isMetaTXT;

											_editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										GUILayout.Space(5);

										EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.CaptureScreenPosZoom));//"Screen Position and Zoom"
										GUILayout.Space(5);

										//화면 위치
										int width_ScreenPos = ((width - (10 + 30)) / 2) - 20;
										GUILayout.Space(5);
										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
										GUILayout.Space(4);
										EditorGUILayout.LabelField(apStringFactory.I.X, apGUILOFactory.I.Width(15));
										Editor._captureSprite_ScreenPos.x = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.x, apGUILOFactory.I.Width(width_ScreenPos));
										EditorGUILayout.LabelField(apStringFactory.I.Y, apGUILOFactory.I.Width(15));
										Editor._captureSprite_ScreenPos.y = EditorGUILayout.DelayedFloatField(Editor._captureSprite_ScreenPos.y, apGUILOFactory.I.Width(width_ScreenPos));

										//GUIStyle guiStyle_SetBtn = new GUIStyle(GUI.skin.button);
										//guiStyle_SetBtn.margin = GUI.skin.textField.margin;

										if (GUILayout.Button(apStringFactory.I.Set, apGUIStyleWrapper.I.Button_TextFieldMargin, apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(18)))
										{
											Editor._scroll_CenterWorkSpace = Editor._captureSprite_ScreenPos * 0.01f;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										EditorGUILayout.EndHorizontal();
										//Zoom

										Rect lastRect = GUILayoutUtility.GetLastRect();
										lastRect.x += 5;
										lastRect.y += 25;
										lastRect.width = width - (30 + 10 + 60 + 10);
										lastRect.height = 20;

										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
										GUILayout.Space(6);
										GUILayout.Space(width - (30 + 10 + 60));
										//Editor._captureSprite_ScreenZoom = EditorGUILayout.IntSlider(Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1, GUILayout.Width(width - (30 + 10 + 40)));
										float fScreenZoom = GUI.HorizontalSlider(lastRect, Editor._captureSprite_ScreenZoom, 0, Editor._zoomListX100.Length - 1);
										Editor._captureSprite_ScreenZoom = Mathf.Clamp((int)fScreenZoom, 0, Editor._zoomListX100.Length - 1);

										EditorGUILayout.LabelField(Editor._zoomListX100_Label[Editor._captureSprite_ScreenZoom], apGUILOFactory.I.Width(60));
										if (GUILayout.Button(apStringFactory.I.Set, apGUIStyleWrapper.I.Button_TextFieldMargin, apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(18)))
										{
											Editor._iZoomX100 = Editor._captureSprite_ScreenZoom;
											if (Editor._iZoomX100 < 0)
											{
												Editor._iZoomX100 = 0;
											}
											else if (Editor._iZoomX100 >= Editor._zoomListX100.Length)
											{
												Editor._iZoomX100 = Editor._zoomListX100.Length - 1;
											}
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										EditorGUILayout.EndHorizontal();

										//"Focus To Center"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureMoveToCenter), Editor.GetUIWord(UIWORD.CaptureMoveToCenter), false, true, width, 20))
										{
											Editor._scroll_CenterWorkSpace = Vector2.zero;
											Editor._captureSprite_ScreenPos = Vector2.zero;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}
										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
										GUILayout.Space(4);


										if (_strWrapper_64 == null)
										{
											_strWrapper_64 = new apStringWrapper(64);
										}

										_strWrapper_64.Clear();
										_strWrapper_64.Append(Editor.GetUIWord(UIWORD.CaptureZoom), false);
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(apStringFactory.I.Minus, true);

										//"Zoom -"
										if (apEditorUtil.ToggledButton_2Side(_strWrapper_64.ToString(), _strWrapper_64.ToString(), false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100--;
											if (Editor._iZoomX100 < 0) { Editor._iZoomX100 = 0; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
											apEditorUtil.ReleaseGUIFocus();
										}

										_strWrapper_64.Clear();
										_strWrapper_64.Append(Editor.GetUIWord(UIWORD.CaptureZoom), false);
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(apStringFactory.I.Plus, true);

										//"Zoom +"
										if (apEditorUtil.ToggledButton_2Side(_strWrapper_64.ToString(), _strWrapper_64.ToString(), false, true, width / 2 - 2, 20))
										{
											Editor._iZoomX100++;
											if (Editor._iZoomX100 >= Editor._zoomListX100.Length) { Editor._iZoomX100 = Editor._zoomListX100.Length - 1; }
											Editor._captureSprite_ScreenZoom = Editor._iZoomX100;
											Editor.SaveEditorPref();
										}
										EditorGUILayout.EndHorizontal();

										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										int nAnimClipToExport = 0;
										for (int i = 0; i < _captureSprite_AnimClipFlags.Count; i++)
										{
											if (_captureSprite_AnimClipFlags[i])
											{
												nAnimClipToExport++;
											}
										}

										//string strTakeSpriteSheets = " " + Editor.GetUIWord(UIWORD.CaptureExportSpriteSheets);//"Export Sprite Sheets";
										//string strTakeSequenceFiles = " " + Editor.GetUIWord(UIWORD.CaptureExportSeqFiles);//"Export Sequence Files";

										_strWrapper_64.Clear();
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(Editor.GetUIWord(UIWORD.CaptureExportSpriteSheets), true);

										if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportSprite), _strWrapper_64.ToString(), _strWrapper_64.ToString(), false, (numXOfSprite > 0 && numYOfSprite > 0 && nAnimClipToExport > 0), width, 30))
										{
											if (CheckComputeShaderSupportedForScreenCapture())//추가 : 캡쳐 처리 가능한지 확인
											{
												StartSpriteSheet(false);
											}
										}

										_strWrapper_64.Clear();
										_strWrapper_64.AppendSpace(1, false);
										_strWrapper_64.Append(Editor.GetUIWord(UIWORD.CaptureExportSeqFiles), true);

										if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Capture_ExportSequence), _strWrapper_64.ToString(), _strWrapper_64.ToString(), false, (nAnimClipToExport > 0), width, 25))
										{
											if (CheckComputeShaderSupportedForScreenCapture())//추가 : 캡쳐 처리 가능한지 확인
											{
												StartSpriteSheet(true);
											}
										}
										GUILayout.Space(5);

										GUILayout.Space(10);
										apEditorUtil.GUI_DelimeterBoxH(width);
										GUILayout.Space(10);

										EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
										GUILayout.Space(4);
										//"Select All"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureSelectAll), Editor.GetUIWord(UIWORD.CaptureSelectAll), false, true, width / 2 - 2, 20))
										{
											for (int i = 0; i < _captureSprite_AnimClipFlags.Count; i++)
											{
												_captureSprite_AnimClipFlags[i] = true;
											}
										}
										//"Deselect All"
										if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.CaptureDeselectAll), Editor.GetUIWord(UIWORD.CaptureDeselectAll), false, true, width / 2 - 2, 20))
										{
											for (int i = 0; i < _captureSprite_AnimClipFlags.Count; i++)
											{
												_captureSprite_AnimClipFlags[i] = false;
											}
										}
										EditorGUILayout.EndHorizontal();

										GUILayout.Space(10);

										//애니메이션 클립별로 "Export"할 것인지 지정
										//GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
										//guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;


										//"Animation Clips"
										_strWrapper_64.Clear();
										_strWrapper_64.AppendSpace(2, false);
										_strWrapper_64.Append(_editor.GetText(TEXT.DLG_AnimationClips), true);

										GUILayout.Button(_strWrapper_64.ToString(), apGUIStyleWrapper.I.None_LabelColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));//투명 버튼



										//애니메이션 클립 리스트를 만들어야 한다.
										if (subAnimClips.Count > 0)
										{
											if (_guiContent_Overall_AnimItem == null)
											{
												_guiContent_Overall_AnimItem = new apGUIContentWrapper();
												_guiContent_Overall_AnimItem.ClearText(true);
												_guiContent_Overall_AnimItem.SetImage(_editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation));
											}

											//Texture2D iconImage = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

											for (int i = 0; i < subAnimClips.Count; i++)
											{
												//GUIStyle curGUIStyle = guiStyle_None;

												apAnimClip animClip = subAnimClips[i];

												//if (animClip == _captureSelectedAnimClip)
												//{
												//	Rect lastRect = GUILayoutUtility.GetLastRect();
												//	prevCaptureColor = GUI.backgroundColor;

												//	if (EditorGUIUtility.isProSkin)
												//	{
												//		GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
												//	}
												//	else
												//	{
												//		GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
												//	}

												//	GUI.Box(new Rect(lastRect.x, lastRect.y + 20, width + 20, 20), "");
												//	GUI.backgroundColor = prevGUIColor;

												//	curGUIStyle = guiStyle_Selected;
												//}

												EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width - 50));
												GUILayout.Space(15);

												//이전
												//if (GUILayout.Button(new GUIContent(" " + animClip._name, iconImage), curGUIStyle, GUILayout.Width((width - 35) - 35), GUILayout.Height(20)))

												//변경
												_guiContent_Overall_AnimItem.ClearText(false);
												_guiContent_Overall_AnimItem.AppendSpaceText(1, false);
												_guiContent_Overall_AnimItem.AppendText(animClip._name, true);

												if (GUILayout.Button(_guiContent_Overall_AnimItem.Content, apGUIStyleWrapper.I.None_LabelColor, apGUILOFactory.I.Width((width - 35) - 35), apGUILOFactory.I.Height(20)))
												{
													//nextSelectedAnimClip = animClip;
												}
												_captureSprite_AnimClipFlags[i] = EditorGUILayout.Toggle(_captureSprite_AnimClipFlags[i], apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(20));

												EditorGUILayout.EndHorizontal();
											}


										}
									}
								}
							}
							catch (Exception ex)
							{
								Debug.LogError("GUI Exception : " + ex);

							}

							Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_Spritesheet_ProgressBar, _captureMode != CAPTURE_MODE.None);//"Capture Spritesheet ProgressBar"
							Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Capture_Spritesheet_Settings, _captureMode == CAPTURE_MODE.None);//"Capture Spritesheet Settings"
						}
						break;
				}

				//이것도 Thumbnail이 아닌 경우
				//Screenshot + GIF Animation : _captureFrame_DstWidth x _captureFrame_DstHeight을 사용한다.
				//Sprite Sheet : 단위 유닛 크기 / 전체 이미지 파일 크기 (_captureFrame_DstWidth)의 두가지를 이용한다.

				//1) Setting
				//Position + Capture Size + File Size / BG Color / Aspect Ratio Fixed

				//<Export 방식은 탭으로 구분한다>
				//2) Thumbnail
				// Size (Width) Preview / Path + Change / Make Thumbnail
				//3) Screen Shot
				// Size (Width / Height x Src + Dst) / Take a Screenshot
				//4) GIF Animation
				// Size (Width / Height x Src + Dst) Animation Clip Name / Quality / Loop Count / Animation Clips / Take a GIF Animation + ProgressBar
				//5) Sprite
				//- Size (개별 캡쳐 크기 / 전체 이미지 크기 / 
				//- 출력 방식 : 스프라이트 시트 Only / Sprite + XML / Sprite + JSON
				//- 
			}
		}


		private void StartMakeThumbnail()
		{
			_captureMode = CAPTURE_MODE.Capturing_Thumbnail;

			//썸네일 크기
			int thumbnailWidth = 256;
			int thumbnailHeight = 128;

			float preferAspectRatio = (float)thumbnailWidth / (float)thumbnailHeight;

			float srcAspectRatio = (float)_editor._captureFrame_SrcWidth / (float)_editor._captureFrame_SrcHeight;
			//긴쪽으로 캡쳐 크기를 맞춘다.
			int srcThumbWidth = _editor._captureFrame_SrcWidth;
			int srcThumbHeight = _editor._captureFrame_SrcHeight;

			//AspectRatio = W / H
			if (srcAspectRatio < preferAspectRatio)
			{
				//가로가 더 길군요. 가로를 자릅시다.
				//H = W / AspectRatio;
				srcThumbHeight = (int)((srcThumbWidth / preferAspectRatio) + 0.5f);
			}
			else
			{
				//세로가 더 길군요. 세로를 자릅시다.
				//W = AspectRatio * H
				srcThumbWidth = (int)((srcThumbHeight * preferAspectRatio) + 0.5f);
			}

			//Request를 만든다.
			apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
			_captureLoadKey = newRequest.MakeScreenShot(OnThumbnailCaptured,
														_editor,
														_editor.Select.RootUnit._childMeshGroup,
														(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x),
														(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
														srcThumbWidth, srcThumbHeight,
														thumbnailWidth, thumbnailHeight,
														Editor._scroll_CenterWorkSpace, Editor._iZoomX100,
														_editor._captureFrame_Color, 0, "");

			//에디터에 대신 렌더링해달라고 요청을 합시다.
			Editor.ScreenCaptureRequest(newRequest);
			Editor.SetRepaint();
		}


		// 2. PNG 스크린샷
		private void StartTakeScreenShot()
		{
			try
			{
				string defFileName = "ScreenShot_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				string saveFilePath = EditorUtility.SaveFilePanel("Save Screenshot as PNG", _capturePrevFilePath_Directory, defFileName, "png");
				if (!string.IsNullOrEmpty(saveFilePath))
				{
					_captureMode = CAPTURE_MODE.Capturing_ScreenShot;


					//추가 21.7.3 : 이스케이프 문자 삭제
					saveFilePath = apUtil.ConvertEscapeToPlainText(saveFilePath);

					//Request를 만든다.
					apScreenCaptureRequest newRequest = new apScreenCaptureRequest();
					_captureLoadKey = newRequest.MakeScreenShot(OnScreeenShotCaptured,
																_editor,
																_editor.Select.RootUnit._childMeshGroup,
																(int)(_editor._captureFrame_PosX + apGL.WindowSizeHalf.x),
																(int)(_editor._captureFrame_PosY + apGL.WindowSizeHalf.y),
																_editor._captureFrame_SrcWidth, _editor._captureFrame_SrcHeight,
																_editor._captureFrame_DstWidth, _editor._captureFrame_DstHeight,
																Editor._scroll_CenterWorkSpace, Editor._iZoomX100,
																_editor._captureFrame_Color, 0, saveFilePath);

					//에디터에 대신 렌더링해달라고 요청을 합시다.
					Editor.ScreenCaptureRequest(newRequest);
					Editor.SetRepaint();
				}
			}
			catch (Exception)
			{

			}
		}


		//3. GIF 애니메이션 만들기
		private void StartGIFAnimation()
		{
			if (_captureSelectedAnimClip == null || _editor.Select.RootUnit._childMeshGroup == null)
			{
				return;
			}

			string defFileName = "GIF_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".gif";
			string saveFilePath = EditorUtility.SaveFilePanel("Save GIF Animation", _capturePrevFilePath_Directory, defFileName, "gif");
			if (!string.IsNullOrEmpty(saveFilePath))
			{
				//추가 21.7.3 : 이스케이프 문자 삭제
				saveFilePath = apUtil.ConvertEscapeToPlainText(saveFilePath);

				//변경 11.4 : GIF 퀄리티 관련 팝업 및 Int -> Enum -> Int로 변경
				bool isAbleToSave = true;
				if (_editor._captureFrame_GIFQuality == apEditor.CAPTURE_GIF_QUALITY.Maximum)
				{
					//"GIF Quality Warning", "Saving with Maximum Quality takes a very long time to process. Are you sure you want to save with this type?", "Okay", "Cancel"
					isAbleToSave = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_ExportGIXMaxQualityWarining_Title),
																_editor.GetText(TEXT.DLG_ExportGIXMaxQualityWarining_Body),
																_editor.GetText(TEXT.Okay),
																_editor.GetText(TEXT.Cancel));
				}
				if (isAbleToSave)
				{
					int gifQuality_255 = 128;
					switch (_editor._captureFrame_GIFQuality)
					{
						case apEditor.CAPTURE_GIF_QUALITY.Low: gifQuality_255 = 128; break;
						case apEditor.CAPTURE_GIF_QUALITY.Medium: gifQuality_255 = 50; break;
						case apEditor.CAPTURE_GIF_QUALITY.High: gifQuality_255 = 10; break;
						case apEditor.CAPTURE_GIF_QUALITY.Maximum: gifQuality_255 = 1; break;
					}


					bool isResult = Editor.SeqExporter.StartGIFAnimation(_editor.Select.RootUnit,
						_captureSelectedAnimClip,
						_editor._captureFrame_GIFSampleLoopCount,
						//_editor._captureFrame_GIFSampleQuality,
						//(256 - gifQuality_255),
						gifQuality_255,
						saveFilePath,
						OnGIFMP4AnimationSaved);

					if (isResult)
					{
						System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);//Path 빈 문자열 확인했음 (21.9.10)
						_capturePrevFilePath_Directory = fi.Directory.FullName;
						_captureMode = CAPTURE_MODE.Capturing_GIF_Animation;
					}
				}

			}
		}



		private void StartMP4Animation()
		{
			if (_captureSelectedAnimClip == null || _editor.Select.RootUnit._childMeshGroup == null)
			{
				return;
			}

			string defFileName = "MP4_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".mp4";
			string saveFilePath = EditorUtility.SaveFilePanel("Save MP4 Animation", _capturePrevFilePath_Directory, defFileName, "mp4");
			if (!string.IsNullOrEmpty(saveFilePath))
			{
				//추가 21.7.3 : 이스케이프 문자 삭제
				saveFilePath = apUtil.ConvertEscapeToPlainText(saveFilePath);

				bool isResult = Editor.SeqExporter.StartMP4Animation(_editor.Select.RootUnit,
						_captureSelectedAnimClip,
						_editor._captureFrame_GIFSampleLoopCount,
						saveFilePath,
						OnGIFMP4AnimationSaved);

				if (isResult)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);//Path 빈 문자열 확인했음 (21.9.10)
					_capturePrevFilePath_Directory = fi.Directory.FullName;
					_captureMode = CAPTURE_MODE.Capturing_MP4_Animation;
				}

			}
		}


		//4. Sprite Sheet로 만들기
		private void StartSpriteSheet(bool isSequenceFiles)
		{
			if (RootUnitAnimClipList.Count != _captureSprite_AnimClipFlags.Count || _editor.Select.RootUnit._childMeshGroup == null)
			{
				return;
			}
			string defFileName = "";
			string saveFileDialogTitle = "";
			if (!isSequenceFiles)
			{
				defFileName = "Spritesheet_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				saveFileDialogTitle = "Save Spritesheet";
			}
			else
			{
				defFileName = "Sequence_" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second + ".png";
				saveFileDialogTitle = "Save Sequence Files";
			}

			string saveFilePath = EditorUtility.SaveFilePanel(saveFileDialogTitle, _capturePrevFilePath_Directory, defFileName, "png");
			if (!string.IsNullOrEmpty(saveFilePath))
			{
				//추가 21.7.3 : 이스케이프 문자 삭제
				saveFilePath = apUtil.ConvertEscapeToPlainText(saveFilePath);


				bool isResult = Editor.SeqExporter.StartSpritesheet(_editor.Select.RootUnit,
													RootUnitAnimClipList,
													_captureSprite_AnimClipFlags,
													saveFilePath,
													_editor._captureSpriteTrimSize == apEditor.CAPTURE_SPRITE_TRIM_METHOD.Compressed,
													isSequenceFiles,
													Editor._captureFrame_SpriteMargin,
													Editor._captureSpriteMeta_XML,
													Editor._captureSpriteMeta_JSON,
													Editor._captureSpriteMeta_TXT,
													OnSpritesheetSaved);

				if (isResult)
				{
					System.IO.FileInfo fi = new System.IO.FileInfo(saveFilePath);//Path 빈 문자열 확인했음 (21.9.10)
					_capturePrevFilePath_Directory = fi.Directory.FullName;
					_captureMode = CAPTURE_MODE.Capturing_Spritesheet;
				}
			}
		}


		//추가 11.15 : 
		/// <summary>
		/// 그래픽 가속을 지원하지 않는 경우, 안내 메시지를 보여주고 선택하게 해야한다.
		/// 그래픽 가속을 지원하거나 계속 처리가 가능한 경우(무시 포함) true를 리턴한다.
		/// </summary>
		/// <returns></returns>
		private bool CheckComputeShaderSupportedForScreenCapture()
		{
			//1. Compute Shader를 지원한다.
			if (SystemInfo.supportsComputeShaders)
			{
				return true;
			}

#if UNITY_EDITOR_WIN
			// 이 코드는 Window 에디터에서만 열린다.
			int iBtn = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.DLG_NoComputeShaderOnCapture_Title),
															Editor.GetText(TEXT.DLG_NoComputeShaderOnCapture_Body),
															Editor.GetText(TEXT.DLG_NoComputeShaderOnCapture_IgnoreAndCapture),
															Editor.GetText(TEXT.DLG_NoComputeShaderOnCapture_OpenBuildSettings),
															Editor.GetText(TEXT.Cancel)
															);

			if (iBtn == 0)
			{
				//무시하고 진행하기

				return true;
			}
			else if (iBtn == 1)
			{
				//BuildSetting 창을 열자
				EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
			}

			return false;

#else
			return true;
#endif


		}



		//-----------------------------------------------
		// 이미지 선택시 UI 그리기
		//-----------------------------------------------
		//이미지 크기 변수 처리를 위한 임시 변수
		private apTextureData _tmp_PrevTextureData = null;
		private Texture2D _tmp_PrevImageOfTextureData = null;
		private int _tmp_PrevTextureDataWidth = 0;
		private int _tmp_PrevTextureDataHeight = 0;

		
		private void Draw_ImageRes(int width, int height)
		{
			//EditorGUILayout.Space();

			apTextureData textureData = _image;
			if (textureData == null)
			{
				SelectNone();
				return;
			}

			if (_tmp_PrevTextureData != textureData
				|| _tmp_PrevImageOfTextureData != textureData._image)
			{
				//대상 이미지가 바뀌었다. > 크기 값을 반영하자
				_tmp_PrevTextureData = textureData;
				_tmp_PrevImageOfTextureData = textureData._image;
				_tmp_PrevTextureDataWidth = textureData._width;
				_tmp_PrevTextureDataHeight = textureData._height;
				apEditorUtil.ReleaseGUIFocus();
			}

			Color prevColor = GUI.backgroundColor;

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ImageAsset));//"Image Asset"

			//textureData._image = EditorGUILayout.ObjectField(textureData._image, typeof(Texture2D), true, GUILayout.Width(width), GUILayout.Height(50)) as Texture2D;
			Texture2D nextImage = EditorGUILayout.ObjectField(textureData._image, typeof(Texture2D), true) as Texture2D;


			//텍스쳐 에셋이 할당되지 않았다면, 이미지 선택 버튼이 반짝거린다. [v1.4.2]
			if(textureData._image == null)
			{
				GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();
			}
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.SelectImage), apGUILOFactory.I.Height(30)))//"Select Image"
			{
				_loadKey_SelectTextureAsset = apDialog_SelectTextureAsset.ShowDialog(Editor, textureData, OnTextureAssetSelected);
			}
			GUI.backgroundColor = prevColor;


			if (textureData._image != nextImage)
			{
				//이미지가 추가되었다.
				if (nextImage != null)
				{
					//Undo
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Image_SettingChanged, 
														Editor, 
														Editor._portrait, 
														//textureData._image, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					//추가 21.9.11 : 이미지를 처음 할당할 때와 그렇지 않을 때의 처리가 다르다.
					bool isFirstAssign = false;
					if(textureData._image == null && nextImage != null)
					{
						isFirstAssign = true;
					}

					textureData._image = nextImage;//이미지 추가
					textureData._name = textureData._image.name;

					//이전 : 그냥 변경
					//textureData._width = textureData._image.width;
					//textureData._height = textureData._image.height;

					//변경 21.9.11 : 처음 할당인 경우에만 크기를 변경한다. (또는 크기값이 유효하지 않은 경우)
					if(isFirstAssign
						|| textureData._width == 0
						|| textureData._height == 0)
					{
						textureData._width = textureData._image.width;
						textureData._height = textureData._image.height;

						//임시 값도 동기화
						_tmp_PrevTextureDataWidth = textureData._width;
						_tmp_PrevTextureDataHeight = textureData._height;
					}


					
					//이미지 에셋의 Path를 확인하고, PSD인지 체크한다.
					if (textureData._image != null)
					{
						string fullPath = AssetDatabase.GetAssetPath(textureData._image);
						//Debug.Log("Image Path : " + fullPath);

						if (string.IsNullOrEmpty(fullPath))
						{
							textureData._assetFullPath = "";
							//textureData._isPSDFile = false;
						}
						else
						{
							textureData._assetFullPath = fullPath;
							//if (fullPath.Contains(".psd") || fullPath.Contains(".PSD"))
							//{
							//	textureData._isPSDFile = true;
							//}
							//else
							//{
							//	textureData._isPSDFile = false;
							//}
						}
					}
					else
					{
						textureData._assetFullPath = "";
						//textureData._isPSDFile = false;
					}

					apEditorUtil.ReleaseGUIFocus();
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy(false);
			}

			EditorGUILayout.Space();


			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"
			string nextName = EditorGUILayout.DelayedTextField(textureData._name);

			GUILayout.Space(20);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Size));//"Size"

			//변경 21.9.12 : 크기 정보를 미리 보여주고, 변경 버튼과 함께 만든다.
			//이전 UI
			//EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Width), apGUILOFactory.I.Width(40));//"Width"
			//int nextWidth = EditorGUILayout.DelayedIntField(textureData._width);
			//EditorGUILayout.EndHorizontal();

			//EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Height), apGUILOFactory.I.Width(40));//"Height"
			//int nextHeight = EditorGUILayout.DelayedIntField(textureData._height);
			//EditorGUILayout.EndHorizontal();

			//변경 21.9.12
			//크기 정보를 보여주자
			//- 프로퍼티 
			if(_strWrapper_128 == null)
			{
				_strWrapper_128 = new apStringWrapper(128);
			}			

			GUILayout.Space(5);

			//프로퍼티상의 크기
			_strWrapper_128.Clear();
			_strWrapper_128.Append(textureData._width, false);
			_strWrapper_128.Append(apStringFactory.I.InterXofSize, false);
			_strWrapper_128.Append(textureData._height, true);

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SizeOfImage));//"Size as Property"
			EditorGUILayout.LabelField(_strWrapper_128.ToString());

			GUILayout.Space(5);

			//이미지 에셋으로서의 크기
			_strWrapper_128.Clear();
			if(textureData._image == null)
			{
				_strWrapper_128.Append(apStringFactory.I.NoImage, true);
			}
			else
			{
				_strWrapper_128.Append(textureData._image.width, false);
				_strWrapper_128.Append(apStringFactory.I.InterXofSize, false);
				_strWrapper_128.Append(textureData._image.height, true);
			}
			
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SizeOfTextureAsset));//"Size as Texture Asset"
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			
			if(textureData.IsResized())
			{
				EditorGUILayout.LabelField(_strWrapper_128.ToString(), apGUIStyleWrapper.I.Label_RedColor);
			}
			else
			{
				EditorGUILayout.LabelField(_strWrapper_128.ToString());
			}
			
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ChangeSize));//"Change Size"
			int widthHalf = (width - 8) / 2;
			//이미지 크기를 설정할 수 있다.
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			
			if(_tmp_PrevTextureDataWidth != textureData._width)
			{
				GUI.backgroundColor = new Color(GUI.backgroundColor.r * 0.7f, GUI.backgroundColor.g * 1.5f, GUI.backgroundColor.b * 0.7f, GUI.backgroundColor.a);
			}
			_tmp_PrevTextureDataWidth = EditorGUILayout.IntField(_tmp_PrevTextureDataWidth, apGUILOFactory.I.Width(widthHalf));
			GUI.backgroundColor = prevColor;

			if(_tmp_PrevTextureDataHeight != textureData._height)
			{
				GUI.backgroundColor = new Color(GUI.backgroundColor.r * 0.7f, GUI.backgroundColor.g * 1.5f, GUI.backgroundColor.b * 0.7f, GUI.backgroundColor.a);
			}
			_tmp_PrevTextureDataHeight = EditorGUILayout.IntField(_tmp_PrevTextureDataHeight, apGUILOFactory.I.Width(widthHalf));
			GUI.backgroundColor = prevColor;

			EditorGUILayout.EndHorizontal();



			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
			GUILayout.Space(5);
			bool isAnyTextureSizeChangable = _tmp_PrevTextureDataWidth != textureData._width || _tmp_PrevTextureDataHeight != textureData._height;

			if(apEditorUtil.ToggledButton_2Side(Editor.GetText(TEXT.DLG_Apply), false, isAnyTextureSizeChangable, widthHalf, 20))
			{
				if(_tmp_PrevTextureDataWidth != textureData._width
					|| _tmp_PrevTextureDataHeight != textureData._height)
				{
					//이미지의 크기를 변경하자
					//크기가 바뀐 경우 (21.9.11)
					//메시들의 버텍스 위치를 바꾸어야 하므로 경고를 준다.
					//"Image Size Changed"
					//"Changing the size of the image also affects other meshes.\nAre you sure you want to change it?"
					bool isResult = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_ChangeImageSize_Title), 
																	Editor.GetText(TEXT.DLG_ChangeImageSize_Body1),
																	Editor.GetText(TEXT.Okay), 
																	Editor.GetText(TEXT.Cancel));

					if(isResult)
					{
						//메시 트랜스폼의 크기 조절 여부
						//"Resize related Objects"
						//"Do you want to correct the scale of objects that use this image?"
						//"Calibrate Scale"
						int iBtn = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.DLG_ChangeImageSize_Title),
																		Editor.GetText(TEXT.DLG_ChangeImageSize_Body2),
																		Editor.GetText(TEXT.CalibrateScale),
																		Editor.GetText(TEXT.Ignore),
																		Editor.GetText(TEXT.Cancel));

						if(iBtn == 0)
						{
							//관련된 메시 TF들의 크기 변화와 함께 텍스쳐 크기 변경
							Editor.Controller.ResizeTextureData(_portrait, textureData, _tmp_PrevTextureDataWidth, _tmp_PrevTextureDataHeight, true);
							Editor.RefreshControllerAndHierarchy(false);
						}
						else if(iBtn == 1)
						{
							//텍스쳐 크기만 변경
							Editor.Controller.ResizeTextureData(_portrait, textureData, _tmp_PrevTextureDataWidth, _tmp_PrevTextureDataHeight, false);
							Editor.RefreshControllerAndHierarchy(false);
						}
					}

					_tmp_PrevTextureDataWidth = textureData._width;
					_tmp_PrevTextureDataHeight = textureData._height;
					apEditorUtil.ReleaseGUIFocus();
				}
			}


			if(apEditorUtil.ToggledButton_2Side(Editor.GetText(TEXT.Cancel), false, isAnyTextureSizeChangable, widthHalf, 20))//"Revert"
			{
				_tmp_PrevTextureDataWidth = textureData._width;
				_tmp_PrevTextureDataHeight = textureData._height;
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			

			//if(GUILayout.Button("크기 테스트", GUILayout.Height(30)))
			//{
			//	int realWidth = 0;
			//	int realHeight = 0;
			//	apEditorUtil.GetTextureAssetRealSize(textureData._image, ref realWidth, ref realHeight);
			//}



			//이름이 바뀐 경우
			if (!string.Equals(nextName, textureData._name))				
			{
				//Undo
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Image_SettingChanged, 
													Editor, 
													Editor._portrait, 
													//textureData, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				textureData._name = nextName;				

				Editor.RefreshControllerAndHierarchy(false);

				apEditorUtil.ReleaseGUIFocus();
			}

			GUILayout.Space(20);
			if (textureData._image != null)
			{
				if (textureData._image != _imageImported || _imageImporter == null)
				{
					string path = AssetDatabase.GetAssetPath(textureData._image);
					_imageImported = textureData._image;
					_imageImporter = (TextureImporter)TextureImporter.GetAtPath(path);
				}
			}
			else
			{
				_imageImported = null;
				_imageImporter = null;
			}



			//텍스쳐 설정을 할 수 있다.
			if (_imageImporter != null)
			{
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(20);

				bool prev_sRGB = _imageImporter.sRGBTexture;
				TextureImporterCompression prev_compressed = _imageImporter.textureCompression;

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ColorSpace));

				if (_strWrapper_64 == null)
				{
					_strWrapper_64 = new apStringWrapper(64);
				}
				_strWrapper_64.Clear();
				_strWrapper_64.Append(apStringFactory.I.Bracket_1_L, false);
				_strWrapper_64.AppendSpace(1, false);
				_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Current), false);
				_strWrapper_64.AppendSpace(1, false);
				_strWrapper_64.Append(apStringFactory.I.Colon, false);
				_strWrapper_64.AppendSpace(1, false);
				if (apEditorUtil.IsGammaColorSpace())
				{
					_strWrapper_64.Append(apStringFactory.I.Gamma, false);
				}
				else
				{
					_strWrapper_64.Append(apStringFactory.I.Linear, false);
				}
				_strWrapper_64.AppendSpace(1, false);
				_strWrapper_64.Append(apStringFactory.I.Bracket_1_R, true);

				//EditorGUILayout.LabelField("( " + Editor.GetUIWord(UIWORD.Current) + " : " + (apEditorUtil.IsGammaColorSpace() ? "Gamma" : "Linear") + " )");
				EditorGUILayout.LabelField(_strWrapper_64.ToString());

				//sRGB True => Gamma Color Space이다.
				int iColorSpace = _imageImporter.sRGBTexture ? 0 : 1;
				int nextColorSpace = EditorGUILayout.Popup(iColorSpace, _imageColorSpaceNames);
				if (nextColorSpace != iColorSpace)
				{
					if (nextColorSpace == 0)
					{
						//Gamma : sRGB 사용
						_imageImporter.sRGBTexture = true;
					}
					else
					{
						//Linear : sRGB 사용 안함
						_imageImporter.sRGBTexture = false;
					}
				}



				GUILayout.Space(5);
				int prevQuality = 0;
				if (_imageImporter.textureCompression == TextureImporterCompression.CompressedLQ)
				{
					prevQuality = 0;
				}
				else if (_imageImporter.textureCompression == TextureImporterCompression.Compressed)
				{
					prevQuality = 1;
				}
				else if (_imageImporter.textureCompression == TextureImporterCompression.CompressedHQ)
				{
					prevQuality = 2;
				}
				else if (_imageImporter.textureCompression == TextureImporterCompression.Uncompressed)
				{
					prevQuality = 3;
				}

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Compression));//"Compression"
				int nextQuality = EditorGUILayout.Popup(prevQuality, _imageQualityNames);

				GUILayout.Space(5);
				bool prevMipmap = _imageImporter.mipmapEnabled;
				_imageImporter.mipmapEnabled = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.UseMipmap), _imageImporter.mipmapEnabled);//"Use Mipmap"

				if (nextQuality != prevQuality)
				{
					switch (nextQuality)
					{
						case 0://ComLQ
							_imageImporter.textureCompression = TextureImporterCompression.CompressedLQ;
							break;

						case 1://Com
							_imageImporter.textureCompression = TextureImporterCompression.Compressed;
							break;

						case 2://ComHQ
							_imageImporter.textureCompression = TextureImporterCompression.CompressedHQ;
							break;

						case 3://Uncom
							_imageImporter.textureCompression = TextureImporterCompression.Uncompressed;
							break;
					}
				}

				GUILayout.Space(5);

				//추가 : Read/Write 옵션 확인
				bool prevReadWrite = _imageImporter.isReadable;
				_imageImporter.isReadable = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.TextureRW), _imageImporter.isReadable);//"Use Mipmap"


				

				if (_imageImporter.isReadable)
				{
					//경고 메시지
					//GUIStyle guiStyle_Info = new GUIStyle(GUI.skin.box);
					//guiStyle_Info.alignment = TextAnchor.MiddleCenter;
					//guiStyle_Info.normal.textColor = apEditorUtil.BoxTextColor;

					

					GUI.backgroundColor = new Color(1.0f, 0.6f, 0.5f, 1.0f);
					GUILayout.Box(Editor.GetUIWord(UIWORD.WarningTextureRWNeedToDisabledForOpt), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(48));

					GUI.backgroundColor = prevColor;
				}

				if (nextQuality != prevQuality ||
					_imageImporter.sRGBTexture != prev_sRGB ||
					_imageImporter.mipmapEnabled != prevMipmap ||
					_imageImporter.isReadable != prevReadWrite)
				{

					_imageImporter.SaveAndReimport();
					_imageImporter = null;
					_imageImported = null;
					AssetDatabase.Refresh();
				}

				GUILayout.Space(20);
			}


			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			if (GUILayout.Button(Editor.GetUIWord(UIWORD.RefreshImageProperty), apGUILOFactory.I.Height(30)))//"Refresh Image Property"
			{
				//Undo
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Image_SettingChanged, 
													Editor, 
													Editor._portrait, 
													//textureData,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				if (textureData._image != null)
				{
					textureData._name = textureData._image.name;
					
					//삭제 21.9.11 : 이미지가 변경되어도 TextureData 상의 크기를 함부로 바꾸면 안된다.
					//메시가 깨질 수 있다.
					//textureData._width = textureData._image.width;
					//textureData._height = textureData._image.height;
				}
				else
				{
					textureData._name = "";

					//같은 이유로 초기화시에도 조심할 것
					//textureData._width = 0;
					//textureData._height = 0;
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy(false);
			}

			// Remove
			GUILayout.Space(30);



			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveImage),
			//										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
			//										),

			if (_guiContent_Image_RemoveImage == null)
			{
				_guiContent_Image_RemoveImage = new apGUIContentWrapper();
				_guiContent_Image_RemoveImage.ClearText(false);
				_guiContent_Image_RemoveImage.AppendSpaceText(2, false);
				_guiContent_Image_RemoveImage.AppendText(Editor.GetUIWord(UIWORD.RemoveImage), true);
				_guiContent_Image_RemoveImage.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}

			//변경
			if (GUILayout.Button(_guiContent_Image_RemoveImage.Content, apGUILOFactory.I.Height(24)))//"  Remove Image"
			{

				//bool isResult = EditorUtility.DisplayDialog("Remove Image", "Do you want to remove [" + textureData._name + "]?", "Remove", "Cancel");

				//Texture를 삭제하면 영향을 받는 메시들을 확인하자
				string strDialogInfo = Editor.Controller.GetRemoveItemMessage(
															_portrait,
															textureData,
															5,
															Editor.GetTextFormat(TEXT.RemoveImage_Body, textureData._name),
															Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveImage_Title),
																strDialogInfo,
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));


				if (isResult)
				{
					Editor.Controller.RemoveTexture(textureData);
					//_portrait._textureData.Remove(textureData);

					SelectNone();
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy(false);
			}
		}


		//--------------------------------------------------------------
		// 메시 UI
		//--------------------------------------------------------------
		private void Draw_Mesh(int width, int height)
		{
			//GUILayout.Box("Mesh", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Mesh", width);
			//EditorGUILayout.Space();

			if (_mesh == null)
			{
				SelectNone();
				return;
			}

			//탭
			bool isEditMeshMode_None = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Setting);
			bool isEditMeshMode_MakeMesh = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.MakeMesh);
			bool isEditMeshMode_Modify = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Modify);
			bool isEditMeshMode_Pivot = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.PivotEdit);
			bool isEditMeshMode_Pin = (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Pin);

			int subTabWidth = (width / 2) - 5;
			int subTabHeight = 24;

			//탭 뒤 음영 배경
			Color prevColor = GUI.backgroundColor;
			if(EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color(	0.15f, 0.15f, 0.15f, 1.0f);
			}
			else
			{
				GUI.backgroundColor = new Color(	GUI.backgroundColor.r * 0.7f, GUI.backgroundColor.g * 0.7f, GUI.backgroundColor.b * 0.7f, 1.0f);
			}
#if UNITY_2019_1_OR_NEWER
			GUI.Box(new Rect(1, 0, width + 20, subTabHeight * 3 + 15), apStringFactory.I.None, apEditorUtil.WhiteGUIStyle);
#else
			GUI.Box(new Rect(1, 0, width + 20, subTabHeight * 3 + 18), apStringFactory.I.None, apEditorUtil.WhiteGUIStyle);
#endif	
			GUI.backgroundColor = prevColor;



			//변경 22.2.28 : 탭 레이아웃 변경
			// Setting | Make Mesh
			//  Pivot  | Modify | Pin

			


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));
			GUILayout.Space(5);

			//" Setting"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Setting), 1, Editor.GetUIWord(UIWORD.Setting), isEditMeshMode_None, true, subTabWidth, subTabHeight, apStringFactory.I.SettingsOfMesh))
			{
				if (!isEditMeshMode_None)
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.CheckMeshEdgeWorkRemained();
						Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Setting;
						Editor._isMeshEdit_AreaEditing = false;
						_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;
						Editor.Gizmos.Unlink();
					}
				}
			}

			//" Make Mesh"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MeshEditMenu), 1, Editor.GetUIWord(UIWORD.MakeMesh), isEditMeshMode_MakeMesh, true, subTabWidth, subTabHeight, apStringFactory.I.MakeVerticesAndPolygons))//"Make Vertices and Polygons"
			{
				if (!isEditMeshMode_MakeMesh)
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.CheckMeshEdgeWorkRemained();
						Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
						Editor._isMeshEdit_AreaEditing = false;
						_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;


						Editor.Controller.StartMeshEdgeWork();
						Editor.VertController.SetMesh(_mesh);
						Editor.VertController.UnselectVertex();

						Editor.Gizmos.Unlink();

						//이벤트 등록 코드가 빠져있다. (20.9.16)
						switch (Editor._meshEditeMode_MakeMesh_Tab)
						{
							case apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS:
								//TRS는 기즈모를 등록해야한다.
								Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshTRS());
								break;

							case apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen:
								//Auto Gen도 Control Point를 제어하는 기즈모가 있다.
								//Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshAreaEdit());
								Editor.Gizmos.Unlink();//기즈모 삭제됨 21.1.6
								break;
						}
					}
				}
			}

			


			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));
			GUILayout.Space(5);

			//" Pivot"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_PivotMenu), 1, Editor.GetUIWord(UIWORD.Pivot), isEditMeshMode_Pivot, true, subTabWidth, subTabHeight, apStringFactory.I.EditPivotOfMesh))//"Edit Pivot of Mesh"
			{
				if (!isEditMeshMode_Pivot)
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.CheckMeshEdgeWorkRemained();
						Editor._meshEditMode = apEditor.MESH_EDIT_MODE.PivotEdit;
						Editor._isMeshEdit_AreaEditing = false;
						_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

						Editor.Gizmos.Unlink();
					}
				}
			}

			//" Modify"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ModifyMenu), 1, Editor.GetUIWord(UIWORD.Modify), isEditMeshMode_Modify, true, subTabWidth, subTabHeight, apStringFactory.I.ModifyVertices))//"Modify Vertices"
			{
				if (!isEditMeshMode_Modify)
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.CheckMeshEdgeWorkRemained();
						Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Modify;
						Editor._isMeshEdit_AreaEditing = false;
						_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

						Editor.Gizmos.Unlink();
						Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshEdit_Modify());
					}
				}
			}

			

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));
			GUILayout.Space(5);
			
			
			//" Pin"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_PinMenu), 1, Editor.GetUIWord(UIWORD.Pin), isEditMeshMode_Pin, true, subTabWidth, subTabHeight, apStringFactory.I.AddAndEditPins))
			{
				if (!isEditMeshMode_Pin)
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.CheckMeshEdgeWorkRemained();
						Editor._meshEditMode = apEditor.MESH_EDIT_MODE.Pin;
						Editor._isMeshEdit_AreaEditing = false;
						_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

						Editor.Gizmos.Unlink();
						//기즈모 추가 필요
						RefreshPinModeEvent();
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			//20.12.4 단축키 등록 : 숫자키로 탭 전환
			Editor.AddHotKeyEvent(OnHotKeyEvent_SetMeshTab, apHotKeyMapping.KEY_TYPE.MakeMesh_Tab_1_Setting, 0);
			Editor.AddHotKeyEvent(OnHotKeyEvent_SetMeshTab, apHotKeyMapping.KEY_TYPE.MakeMesh_Tab_2_MakeMesh_Add, 1);
			Editor.AddHotKeyEvent(OnHotKeyEvent_SetMeshTab, apHotKeyMapping.KEY_TYPE.MakeMesh_Tab_2_MakeMesh_Edit, 2);
			Editor.AddHotKeyEvent(OnHotKeyEvent_SetMeshTab, apHotKeyMapping.KEY_TYPE.MakeMesh_Tab_2_MakeMesh_Auto, 3);
			Editor.AddHotKeyEvent(OnHotKeyEvent_SetMeshTab, apHotKeyMapping.KEY_TYPE.MakeMesh_Tab_3_Pivot, 4);
			Editor.AddHotKeyEvent(OnHotKeyEvent_SetMeshTab, apHotKeyMapping.KEY_TYPE.MakeMesh_Tab_4_Modify, 5);
			Editor.AddHotKeyEvent(OnHotKeyEvent_SetMeshTab, apHotKeyMapping.KEY_TYPE.MakeMesh_Tab_5_Pin, 6);


			switch (Editor._meshEditMode)
			{
				case apEditor.MESH_EDIT_MODE.Setting:
					DrawMeshProperty_Setting(width, height);
					break;

				case apEditor.MESH_EDIT_MODE.Modify:
					DrawMeshProperty_Modify(width, height);
					break;

				case apEditor.MESH_EDIT_MODE.MakeMesh:
					DrawMeshProperty_MakeMesh(width, height);
					break;
				//case apEditor.MESH_EDIT_MODE.AddVertex:
				//	MeshProperty_AddVertex(width, height);
				//	break;

				//case apEditor.MESH_EDIT_MODE.LinkEdge:
				//	MeshProperty_LinkEdge(width, height);
				//	break;

				case apEditor.MESH_EDIT_MODE.PivotEdit:
					DrawMeshProperty_Pivot(width, height);
					break;

				case apEditor.MESH_EDIT_MODE.Pin:
					DrawMeshProperty_Pin(width, height);
					break;
			}


			//추가 20.7.6 : PSD 파일로부터 생성된 Mesh는 열었을 때 질문을 해야한다.
			if (Editor._isRequestRemoveVerticesIfImportedFromPSD_Step1 &&
				Editor._isRequestRemoveVerticesIfImportedFromPSD_Step2 &&
				Editor._requestMeshRemoveVerticesIfImportedFromPSD == _mesh &&
				Event.current.type == EventType.Repaint)
			{
				Editor._isRequestRemoveVerticesIfImportedFromPSD_Step1 = false;
				Editor._isRequestRemoveVerticesIfImportedFromPSD_Step2 = false;
				Editor._requestMeshRemoveVerticesIfImportedFromPSD = null;

				Editor.Controller.AskRemoveVerticesIfImportedFromPSD(_mesh);
			}

		}




		// Mesh 세부 탭 UI
		
		/// <summary>
		/// Mesh의 Setting 탭 UI
		/// </summary>
		private void DrawMeshProperty_Setting(int width, int height)
		{
			Color prevColor = GUI.backgroundColor;

			GUILayout.Space(5);

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"

			//추가 20.12.4 : 단축키로 이름 UI에 포커스를 주기 위해
			apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__MeshName);

			string nextMeshName = EditorGUILayout.DelayedTextField(_mesh._name, apGUILOFactory.I.Width(width));
			if (!string.Equals(nextMeshName, _mesh._name))
			{
				if (apEditorUtil.IsDelayedTextFieldEventValid(apStringFactory.I.GUI_ID__MeshName))//텍스트 변경이 유효한지도 체크한다.
				{
					apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SettingChanged, 
													Editor, 
													_mesh, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
					_mesh._name = nextMeshName;
				}
				apEditorUtil.ReleaseGUIFocus();
				Editor.RefreshControllerAndHierarchy(false);
			}


			//단축키 추가 (20.12.4)
			Editor.AddHotKeyEvent(OnHotKeyEvent_RenameMesh, apHotKeyMapping.KEY_TYPE.RenameObject, null);


			EditorGUILayout.Space();

			//1. 어느 텍스쳐를 사용할 것인가
			//[수정]
			//다이얼로그를 보여주자

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Image));//"Image"
																	   //apTextureData textureData = _mesh._textureData;

			string strTextureName = null;
			Texture2D curTextureImage = null;
			int selectedImageHeight = 20;
			if (_mesh.LinkedTextureData != null)
			{
				strTextureName = _mesh.LinkedTextureData._name;
				curTextureImage = _mesh.LinkedTextureData._image;

				if (curTextureImage != null && _mesh.LinkedTextureData._width > 0 && _mesh.LinkedTextureData._height > 0)
				{
					if(_mesh.LinkedTextureData._width > _mesh.LinkedTextureData._height)
					{
						selectedImageHeight = (int)((float)(width * _mesh.LinkedTextureData._height) / (float)(_mesh.LinkedTextureData._width));
					}
					else
					{
						selectedImageHeight = width;
					}
					
				}
			}

			if (_guiContent_MeshProperty_Texture == null)
			{
				_guiContent_MeshProperty_Texture = new apGUIContentWrapper();
			}


			if (curTextureImage != null)
			{
				//EditorGUILayout.TextField(strTextureName);
				EditorGUILayout.LabelField(strTextureName != null ? strTextureName : apStringFactory.I.NoImage);
				GUILayout.Space(10);

				

				//EditorGUILayout.LabelField(new GUIContent(curTextureImage), GUILayout.Height(selectedImageHeight));
				_guiContent_MeshProperty_Texture.SetImage(curTextureImage);
				EditorGUILayout.LabelField(_guiContent_MeshProperty_Texture.Content, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, apGUILOFactory.I.Height(selectedImageHeight));
				GUILayout.Space(10);
			}
			else
			{
				//이전
				//EditorGUILayout.LabelField(apStringFactory.I.NoImage);

				GUI.backgroundColor = new Color(GUI.backgroundColor.r * 1.0f, GUI.backgroundColor.g * 0.7f, GUI.backgroundColor.b * 0.7f, GUI.backgroundColor.a);
				GUILayout.Box(apStringFactory.I.NoImage, apGUIStyleWrapper.I.Box_MiddleCenter, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(60));
				GUI.backgroundColor = prevColor;
			}

			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.ChangeImage), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image)), GUILayout.Height(30)))//"  Change Image"



			//변경
			if (_guiContent_MeshProperty_ChangeImage == null)
			{
				_guiContent_MeshProperty_ChangeImage = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.ChangeImage), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Image));
			}

			//이미지가 할당되지 않았다면 버튼이 반짝거리도록 만들자 [v1.4.2]
			if(_mesh._textureData_Linked == null)
			{
				GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();
			}
			if (GUILayout.Button(_guiContent_MeshProperty_ChangeImage.Content, apGUILOFactory.I.Height(30)))//"  Change Image"
			{
				//_isShowTextureDataList = !_isShowTextureDataList;
				_loadKey_SelectTextureDataToMesh = apDialog_SelectTextureData.ShowDialog(Editor, _mesh, OnSelectTextureDataToMesh);
			}
			GUI.backgroundColor = prevColor;

			//EditorGUILayout.Space();


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			if (curTextureImage != null)
			{
				//Atlas + Texutre의 Read 세팅
				DrawMeshAtlasOption(width, false);

				GUILayout.Space(5);

				bool isAutoGenEnabled = curTextureImage != null && _mesh._isPSDParsed;


				if (_guiContent_MakeMesh_QuickGenerate == null)
				{
					_guiContent_MakeMesh_QuickGenerate = new apGUIContentWrapper();
					_guiContent_MakeMesh_QuickGenerate.AppendSpaceText(2, false);
					_guiContent_MakeMesh_QuickGenerate.AppendText(Editor.GetUIWord(UIWORD.QuickGenerate), true);
				}

				if(_guiContent_MakeMesh_MultipleQuickGenerate == null)
				{
					_guiContent_MakeMesh_MultipleQuickGenerate = new apGUIContentWrapper();
					_guiContent_MakeMesh_MultipleQuickGenerate.AppendSpaceText(2, false);
					_guiContent_MakeMesh_MultipleQuickGenerate.AppendText(Editor.GetUIWord(UIWORD.MultipleQuickGenerate), true);
				}

				//추가 : 여기서도 생성 가능
				//단 여기서는 3단계로 설정할 수 있다.
				//Simple / Moderate / Complex
				//설정을 원하면 Advanced 버튼을 누르게 하자
				int iNextQuickPresetType = EditorGUILayout.Popup(Editor._meshAutoGenV2Option_QuickPresetType, _quickMeshGeneratePresetNames);
				
				if(iNextQuickPresetType != Editor._meshAutoGenV2Option_QuickPresetType)
				{
					//만약 iNextQuickPresetType == 3이라면 MakeMesh + AutoGen 메뉴로 전환해야한다.
					//0, 1, 2의 값이라면 값 전환
					if(iNextQuickPresetType == 3)
					{
						//Advanced Settings을 선택한다면 페이지를 이동
						Editor._meshEditMode = apEditor.MESH_EDIT_MODE.MakeMesh;
						Editor._meshEditeMode_MakeMesh_Tab = apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen;
					}
					else
					{
						Editor._meshAutoGenV2Option_QuickPresetType = Mathf.Clamp(iNextQuickPresetType, 0, 2);
						Editor.SaveEditorPref();
					}
				}

				//메시 자동 생성 (Quick Make)
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_QuickMake),
					_guiContent_MakeMesh_QuickGenerate.Content.text,
					_guiContent_MakeMesh_QuickGenerate.Content.text, false, isAutoGenEnabled, width, 40))
				{
					//Generate 버튼
					bool isStartAutoGen = false;
					if (Editor.Select.Mesh._vertexData.Count > 0)
					{
						//버텍스를 모두 삭제할지 여부를 묻기
						//"Replace or Append vertices", "Do you want to replace existing vertices when creating the mesh?", "Replace", "Append", "Cancel"
						int iRemoveType = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.DLG_ReplaceAppendVertices_Title),
																				Editor.GetText(TEXT.DLG_ReplaceAppendVertices_Body),
																				Editor.GetText(TEXT.DLG_Replace),
																				Editor.GetText(TEXT.DLG_Append),
																				Editor.GetText(TEXT.Cancel));
						if (iRemoveType == 0)
						{
							//삭제
							apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_RemoveAllVertices, 
															Editor, 
															_mesh, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.StructChanged);
							_mesh._vertexData.Clear();
							_mesh._indexBuffer.Clear();
							_mesh._edges.Clear();
							_mesh._polygons.Clear();

							_mesh.MakeEdgesToPolygonAndIndexBuffer();

							Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

							Editor.VertController.UnselectVertex();
							Editor.VertController.UnselectNextVertex();
						}
						if (iRemoveType != 2)
						{
							//실행
							isStartAutoGen = true;
						}
					}
					else
					{
						//버텍스가 없다. 그냥 실행
						isStartAutoGen = true;
					}

					if (isStartAutoGen)
					{
						apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_AutoGen, 
														Editor, 
														_mesh, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						//QuickGenerate에서는 프리셋에 따라 다르다
						bool preset_IsInnerMargin = false;
						int preset_Density = 1;
						int preset_InnerMargin = 5;
						int preset_OuterMargin = 10;
						switch (Editor._meshAutoGenV2Option_QuickPresetType)
						{
							case 0://Simple
								preset_IsInnerMargin = false;
								preset_Density = 1;
								preset_InnerMargin = 1;
								preset_OuterMargin = 5;
								break;

							case 1://Moderate
								preset_IsInnerMargin = true;
								preset_Density = 2;
								preset_InnerMargin = 5;
								preset_OuterMargin = 10;
								break;

							case 2://Complex
								preset_IsInnerMargin = true;
								preset_Density = 5;
								preset_InnerMargin = 5;
								preset_OuterMargin = 10;
								break;
						}

						Editor.MeshGeneratorV2.ReadyToRequest(OnMeshAutoGeneratedV2);
						Editor.MeshGeneratorV2.AddRequest(	Mesh,
															preset_Density,
															preset_OuterMargin,
															preset_InnerMargin,
															preset_IsInnerMargin);
						Editor.MeshGeneratorV2.StartGenerate();//시작!

						//프로그래스바도 출현
						Editor.StartProgressPopup("Mesh Generation", "Generating..", true, OnAutoGenProgressCancel);

					}
				}

				GUILayout.Space(5);

				//추가 21.10.4 : 여러개의 메시들을 대상으로 동시에 QuickMake
				if (apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MultipleQuickMake), 
					_guiContent_MakeMesh_MultipleQuickGenerate.Content.text, 
					_guiContent_MakeMesh_MultipleQuickGenerate.Content.text, false, true, width, 25))
				{
					_loadKey_QuickMeshWizard = apDialog_QuickMeshWizard.ShowDialog(	Editor,
																					_mesh, 
																					Editor._meshAutoGenV2Option_QuickPresetType,
																					OnQuickMeshWizardCompleted);
				}


				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);
			}

			

			if (_guiContent_MeshProperty_ResetVerts == null)
			{
				_guiContent_MeshProperty_ResetVerts = apGUIContentWrapper.Make(Editor.GetUIWord(UIWORD.ResetVertices), false, apStringFactory.I.RemoveAllVerticesAndPolygons);//"Remove all Vertices and Polygons"
			}

			//2. 버텍스 세팅
			if (GUILayout.Button(_guiContent_MeshProperty_ResetVerts.Content))//"Reset Vertices"
			{
				if (_mesh.LinkedTextureData != null && _mesh.LinkedTextureData._image != null)
				{
					bool isConfirmReset = false;
					if (_mesh._vertexData != null && _mesh._vertexData.Count > 0 &&
						_mesh._indexBuffer != null && _mesh._indexBuffer.Count > 0)
					{
						//isConfirmReset = EditorUtility.DisplayDialog("Reset Vertex", "If you reset vertices, All data is reset.", "Reset", "Cancel");
						isConfirmReset = EditorUtility.DisplayDialog(Editor.GetText(TEXT.ResetMeshVertices_Title),
																		Editor.GetText(TEXT.ResetMeshVertices_Body),
																		Editor.GetText(TEXT.ResetMeshVertices_Okay),
																		Editor.GetText(TEXT.Cancel));


					}
					else
					{
						isConfirmReset = true;
					}

					if (isConfirmReset)
					{
						apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_ResetVertices, 
														Editor, 
														_mesh, 
														//_mesh, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						_mesh._vertexData.Clear();
						_mesh._indexBuffer.Clear();
						_mesh._edges.Clear();
						_mesh._polygons.Clear();
						_mesh.MakeEdgesToPolygonAndIndexBuffer();

						//1.4.2 : 메시에 영역이 설정되었는지 여부에 따라 리셋이 다르게 동작한다.
						//Atlas 영역이 설정되지 않은 경우 > 이미지 전체 영역을 대상으로 사각형 그리기
						//Atlas 영역이 설정된 경우 > Atlas 영역 크기만큼만 사각형 그리기 (추가)
						if(_mesh._isPSDParsed)
						{
							_mesh.ResetVerticesByRect(	_mesh._offsetPos,
														_mesh._atlasFromPSD_LT.x,
														_mesh._atlasFromPSD_LT.y,
														_mesh._atlasFromPSD_RB.x,
														_mesh._atlasFromPSD_RB.y);
						}
						else
						{
							_mesh.ResetVerticesByImageOutline();
						}
						
						_mesh.MakeEdgesToPolygonAndIndexBuffer();


						//Pin-Weight 갱신
						//옵션이 없어도 무조건 Weight 갱신
						if(_mesh._pinGroup != null)
						{
							_mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
						}

						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
					}
				}
			}

			//와! 새해 첫 코드!
			//추가 20.1.5 : 메시 복제하기
			GUILayout.Space(5);
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.Duplicate), apGUILOFactory.I.Width(width)))//"Duplicate"
			{
				Editor.Controller.DuplicateMesh(_mesh);
				//주의 : Duplicate Mesh에 "자동 선택 기능"이 있으므로, 이 이후에 Select.Mesh를 사용하면 안된다.
			}


			// Remove Mesh
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			if (_guiContent_MeshProperty_RemoveMesh == null)
			{
				_guiContent_MeshProperty_RemoveMesh = new apGUIContentWrapper();
				_guiContent_MeshProperty_RemoveMesh.ClearText(false);
				_guiContent_MeshProperty_RemoveMesh.AppendSpaceText(2, false);
				_guiContent_MeshProperty_RemoveMesh.AppendText(Editor.GetUIWord(UIWORD.RemoveMesh), true);
				_guiContent_MeshProperty_RemoveMesh.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}

			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveMesh), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform) ),
			//						GUILayout.Height(24)))//"  Remove Mesh"

			//변경
			if (GUILayout.Button(_guiContent_MeshProperty_RemoveMesh.Content, apGUILOFactory.I.Height(24)))//"  Remove Mesh"
			{
				string strRemoveDialogInfo = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																_mesh,
																5,
																Editor.GetTextFormat(TEXT.RemoveMesh_Body, _mesh._name),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																);

				//bool isResult = EditorUtility.DisplayDialog("Remove Mesh", "Do you want to remove [" + _mesh._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveMesh_Title),
																//Editor.GetTextFormat(TEXT.RemoveMesh_Body, _mesh._name),
																strRemoveDialogInfo,
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));

				if (isResult)
				{
					//apEditorUtil.SetRecord("Remove Mesh", _portrait);

					//MonoBehaviour.DestroyImmediate(_mesh.gameObject);
					//_portrait._meshes.Remove(_mesh);
					Editor.Controller.RemoveMesh(_mesh);

					SelectNone();
				}
			}
		}

		

		/// <summary>
		/// 메시 MakeMesh 탭 UI 그리기
		/// </summary>
		private void DrawMeshProperty_MakeMesh(int width, int height)
		{
			//GUILayout.Space(10);
			//apEditorUtil.GUI_DelimeterBoxH(width);
			//GUILayout.Space(10);

			GUILayout.Space(5);

			//추가 : 8.22
			//Auto와 TRS 기능이 추가되어서 별도의 서브탭이 필요하다.
			//변수가 추가된 것은 아니며, Enum을 묶어서 SubTab으로 처리
			bool isSubTab_AutoGen = Editor._meshEditeMode_MakeMesh_Tab == apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen;
			bool isSubTab_TRS = Editor._meshEditeMode_MakeMesh_Tab == apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS;
			bool isSubTab_Add = (!isSubTab_AutoGen && !isSubTab_TRS);//나머지

			Texture2D icon_SubTab_Add = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakeTab_Add);
			Texture2D icon_SubTab_AutoGen = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakeTab_Auto);
			Texture2D icon_SubTab_TRS = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakeTab_TRS);


			int subTab_btnWidth = (width / 3) - 4;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(28));
			GUILayout.Space(5);
			bool isSubTabBtn_Add = apEditorUtil.ToggledButton(icon_SubTab_Add, 1, Editor.GetUIWord(UIWORD.MakeMeshTab_Add), isSubTab_Add, true, subTab_btnWidth, 28, apStringFactory.I.CreateAMeshManually);//Add//"Create a mesh manually"
			bool isSubTabBtn_TRS = apEditorUtil.ToggledButton(icon_SubTab_TRS, 1, Editor.GetUIWord(UIWORD.MakeMeshTab_Edit), isSubTab_TRS, true, subTab_btnWidth, 28, apStringFactory.I.SelectAndModifyVertices);//Edit//"Select and modify vertices"
			bool isSubTabBtn_AutoGen = apEditorUtil.ToggledButton(icon_SubTab_AutoGen, 1, Editor.GetUIWord(UIWORD.MakeMeshTab_Auto), isSubTab_AutoGen, true, subTab_btnWidth, 28, apStringFactory.I.GenerateAMeshAutomatically);//Auto//"Generate a mesh automatically"
			EditorGUILayout.EndHorizontal();



			if (isSubTabBtn_Add && !isSubTab_Add)
			{
				//Add Tool로 변환
				
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					Editor._meshEditeMode_MakeMesh_Tab = apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AddTools;
					Editor._isMeshEdit_AreaEditing = false;
					_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

					isSubTab_Add = true;
					isSubTab_TRS = false;
					isSubTab_AutoGen = false;
					//미러도 초기화
					Editor._meshEditMirrorMode = apEditor.MESH_EDIT_MIRROR_MODE.None;
					Editor.MirrorSet.Clear();
					Editor.MirrorSet.ClearMovedVertex();
				}
			}
			if (isSubTabBtn_TRS && !isSubTab_TRS)
			{
				//TRS로 변환

				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					Editor._meshEditeMode_MakeMesh_Tab = apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS;
					Editor._isMeshEdit_AreaEditing = false;
					_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

					isSubTab_Add = false;
					isSubTab_TRS = true;
					isSubTab_AutoGen = false;

					//기즈모 이벤트 변경
					Editor.Gizmos.Unlink();
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshTRS());
					//미러도 초기화
					Editor._meshEditMirrorMode = apEditor.MESH_EDIT_MIRROR_MODE.None;
					Editor.MirrorSet.Clear();
					Editor.MirrorSet.ClearMovedVertex();
				}
			}
			if (isSubTabBtn_AutoGen && !isSubTab_AutoGen)
			{
				//Auto Gen으로 변환

				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					Editor._meshEditeMode_MakeMesh_Tab = apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen;
					Editor._isMeshEdit_AreaEditing = false;
					_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

					isSubTab_Add = false;
					isSubTab_TRS = false;
					isSubTab_AutoGen = true;

					//기즈모 이벤트 변경
					Editor.Gizmos.Unlink();
					//Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshAreaEdit());

					//미러도 초기화
					Editor._meshEditMirrorMode = apEditor.MESH_EDIT_MIRROR_MODE.None;
					Editor.MirrorSet.Clear();
					Editor.MirrorSet.ClearMovedVertex();
				}
			}
			
			//GUILayout.Space(10);


			if (isSubTab_Add)
			{
				//서브탭 1 : Add 모드

				Texture2D icon_EditVertexWithEdge = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_VertexEdge);
				Texture2D icon_EditVertexOnly = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_VertexOnly);
				Texture2D icon_EditEdgeOnly = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_EdgeOnly);
				Texture2D icon_EditPolygon = Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Polygon);

				bool isSubEditMode_VE = (Editor._meshEditeMode_MakeMesh_AddTool == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge);
				bool isSubEditMode_Vertex = (Editor._meshEditeMode_MakeMesh_AddTool == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly);
				bool isSubEditMode_Edge = (Editor._meshEditeMode_MakeMesh_AddTool == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly);
				bool isSubEditMode_Polygon = (Editor._meshEditeMode_MakeMesh_AddTool == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon);

				//int btnWidth = (width / 3) - 4;
				int btnWidth = (width / 4) - 4;

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(45));
				GUILayout.Space(5);
				bool nextEditMode_VE = apEditorUtil.ToggledButton(icon_EditVertexWithEdge, isSubEditMode_VE, true, btnWidth, 35, apStringFactory.I.MakeMeshTooltip_AddVertexLinkEdge);//"Add Vertex / Link Edge"
				bool nextEditMode_Vertex = apEditorUtil.ToggledButton(icon_EditVertexOnly, isSubEditMode_Vertex, true, btnWidth, 35, apStringFactory.I.MakeMeshTooltip_AddVertex);//"Add Vertex"
				bool nextEditMode_Edge = apEditorUtil.ToggledButton(icon_EditEdgeOnly, isSubEditMode_Edge, true, btnWidth, 35, apStringFactory.I.MakeMeshTooltip_LinkEdge);//"Link Edge"
				bool nextEditMode_Polygon = apEditorUtil.ToggledButton(icon_EditPolygon, isSubEditMode_Polygon, true, btnWidth, 35, apStringFactory.I.MakeMeshTooltip_SelectPolygon);//"Select Polygon"

				EditorGUILayout.EndHorizontal();

				if (nextEditMode_VE && !isSubEditMode_VE)
				{
					Editor._meshEditeMode_MakeMesh_AddTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge;
					Editor.VertController.UnselectVertex();
				}

				if (nextEditMode_Vertex && !isSubEditMode_Vertex)
				{
					Editor._meshEditeMode_MakeMesh_AddTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly;
					Editor.VertController.UnselectVertex();
				}

				if (nextEditMode_Edge && !isSubEditMode_Edge)
				{
					Editor._meshEditeMode_MakeMesh_AddTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly;
					Editor.VertController.UnselectVertex();
				}

				if (nextEditMode_Polygon && !isSubEditMode_Polygon)
				{
					Editor._meshEditeMode_MakeMesh_AddTool = apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon;
					Editor.VertController.UnselectVertex();
				}

				GUILayout.Space(5);

				Color makeMeshModeColor = Color.black;
				string strMakeMeshModeInfo = null;
				switch (Editor._meshEditeMode_MakeMesh_AddTool)
				{
					case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge:
						//strMakeMeshModeInfo = "Add Vertex / Link Edge";
						strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.AddVertexLinkEdge);
						makeMeshModeColor = new Color(0.87f, 0.57f, 0.92f, 1.0f);
						break;

					case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly:
						//strMakeMeshModeInfo = "Add Vertex";
						strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.AddVertex);
						makeMeshModeColor = new Color(0.57f, 0.82f, 0.95f, 1.0f);
						break;

					case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly:
						//strMakeMeshModeInfo = "Link Edge";
						strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.LinkEdge);
						makeMeshModeColor = new Color(0.95f, 0.65f, 0.65f, 1.0f);
						break;

					case apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.Polygon:
						//strMakeMeshModeInfo = "Polygon";
						strMakeMeshModeInfo = Editor.GetUIWord(UIWORD.Polygon);
						makeMeshModeColor = new Color(0.65f, 0.95f, 0.65f, 1.0f);
						break;
				}
				//Polygon HotKey 이벤트 추가 -> 변경
				//이건 Layout이 안나타나면 처리가 안된다. 다른 곳으로 옮기자
				//if (Editor._meshEditeMode_MakeMesh == apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
				//{
				//	Editor.AddHotKeyEvent(Editor.Controller.RemoveSelectedMeshPolygon, "Remove Polygon", KeyCode.Delete, false, false, false, null);
				//}


				//GUIStyle guiStyle_Info = new GUIStyle(GUI.skin.box);
				//guiStyle_Info.alignment = TextAnchor.MiddleCenter;
				//guiStyle_Info.normal.textColor = apEditorUtil.BoxTextColor;

				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = makeMeshModeColor;
				GUILayout.Box((strMakeMeshModeInfo != null ? strMakeMeshModeInfo : apStringFactory.I.None), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 8), apGUILOFactory.I.Height(34));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				DrawMakePolygonsTool(width);


				if (Editor._meshEditeMode_MakeMesh_AddTool == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexAndEdge
					|| Editor._meshEditeMode_MakeMesh_AddTool == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.EdgeOnly
					|| Editor._meshEditeMode_MakeMesh_AddTool == apEditor.MESH_EDIT_MODE_MAKEMESH_ADDTOOLS.VertexOnly)
				{
					//추가 9.12 : Vertex+Edge / Vertex 추가 모드에서는 미러 툴이 나타난다.
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width);
					GUILayout.Space(10);

					DrawMakeMeshMirrorTool(width, false, true);
				}
			}
			else if (isSubTab_TRS)
			{
				//서브탭 2 : TRS 모드
				//도구들
				//- 미러 복사
				//- 합치기
				//- 정렬

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				DrawMakePolygonsTool(width);

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				DrawMakeMeshMirrorTool(width, true, false);



				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AlignTools));//"Align Tool"
				GUILayout.Space(5);
				//- X/Y 합치기 (3개 * 2)
				//- X/Y 고르게 정렬 (2개)
				int width_AlignBtn4 = ((width - 10) / 4) - 1;
				int height_AlignBtn = 28;

				//X Align + X Distribute
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_AlignBtn));
				GUILayout.Space(5);
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Align_XLeft), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.MinX);
					}
				}
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Align_XCenter), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.CenterX);
					}
				}
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Align_XRight), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.MaxX);
					}
				}
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Distribute_X), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.DistributeX);
					}
				}
				EditorGUILayout.EndHorizontal();

				//Y Align
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_AlignBtn));
				GUILayout.Space(5);
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Align_YUp), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.MaxY);
					}
				}
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Align_YCenter), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.CenterY);
					}
				}
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Align_YDown), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.MinY);
					}
				}
				if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Distribute_Y), apGUILOFactory.I.Width(width_AlignBtn4), apGUILOFactory.I.Height(height_AlignBtn)))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Controller.AlignVertices(apEditorController.VERTEX_ALIGN_REQUEST.DistributeY);
					}
				}
				EditorGUILayout.EndHorizontal();



				//추가 21.10.6 : 버텍스 복사하기
				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				int width_CopyBtn = (width - 5) / 2;
				//int width_CopyBtn = width - (10 + width_AxisBtn * 2 + 7);
				int height_CopyBtn = 25;
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_CopyBtn));
				GUILayout.Space(5);

				//" Copy"
				if(_guiContent_CopyTextIcon == null)
				{
					_guiContent_CopyTextIcon = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Copy), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy));
				}
				if(_guiContent_PasteTextIcon == null)
				{
					_guiContent_PasteTextIcon = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Paste), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste));
				}


				// 버텍스 복사하기
				int nSelectedVerts = Editor.VertController.Vertices != null ? Editor.VertController.Vertices.Count : 0;

				if(apEditorUtil.ToggledButton_2Side(_guiContent_CopyTextIcon, false, nSelectedVerts > 0, width_CopyBtn, height_CopyBtn))
				{
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						apSnapShotManager.I.Copy_MeshVertices(Mesh, Editor.VertController.Vertices);
						apEditorUtil.ReleaseGUIFocus();
					}
				}

				//버텍스 - Edge 붙여넣기
				if(apEditorUtil.ToggledButton_2Side(_guiContent_PasteTextIcon, false, apSnapShotManager.I.IsPastable_MeshVertices(), width_CopyBtn, height_CopyBtn))
				{
					//변경 1.4.2 : 바로 붙이지 않고, 좌표 변환을 물어본다.
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						_loadKey_PasteMestVertPin = apDialog_CopyMeshVertPin.ShowDialog(Editor,
																							Mesh,
																							OnCopyMeshVerts,
																							false);//false : 핀이 아닌 버텍스를 복사한다.
					}
				}

				EditorGUILayout.EndHorizontal();




				//Hot Key를 등록하자
				//Delete 버튼을 누르면 버텍스 삭제
				//Editor.AddHotKeyEvent(OnHotKeyEvent_RemoveVertexOnTRS, apHotKey.LabelText.RemoveVertices, KeyCode.Delete, false, false, false, null);//"Remove Vertices"
				//Editor.AddHotKeyEvent(OnHotKeyEvent_RemoveVertexOnTRS, apHotKey.LabelText.RemoveVertices, KeyCode.Delete, true, false, false, null);//"Remove Vertices"

				//변경 20.12.3
				Editor.AddHotKeyEvent(OnHotKeyEvent_RemoveVertexOnTRS, apHotKeyMapping.KEY_TYPE.MakeMesh_RemoveVertex, null);//"Remove Vertices"
				Editor.AddHotKeyEvent(OnHotKeyEvent_RemoveVertexOnTRS, apHotKeyMapping.KEY_TYPE.MakeMesh_RemoveVertex_KeepEdge, null);//"Remove Vertices Keep Edge"

			}
			else
			{
				//서브탭 3 : 자동 생성 모드
				//3개의 그룹으로 나뉜다.
				//- Atlas 설정
				//- Scan
				//- Preview / Make
				//이전
				//GUIContent guiContent_StepCompleted = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_StepCompleted));
				//GUIContent guiContent_StepUncompleted = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_StepUncompleted));
				//GUIContent guiContent_StepUnUsed = new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_StepUnused));

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				//변경
				//화면 구성
				//Area 설정화면
				//변경해야한다.
				DrawMeshAtlasOption(width, true);

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				
				//옵션
				//Density (Outer, Inner)
				//Margin (Outer, Inner)
				//기본값
				
				int width_Label = 100;
				int width_Value = width - (10 + width_Label);
				int width_LevelBtn = 20;
				int width_ValueLevel = width - (10 + width_Label + 4 + width_LevelBtn * 2);


				//좌, 우 버튼
				if (_guiContent_imgValueLeft == null)
				{
					_guiContent_imgValueLeft = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Left));
				}
				if (_guiContent_imgValueRight == null)
				{
					_guiContent_imgValueRight = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Right));
				}

				
				//1. Density
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Density), apGUILOFactory.I.Width(width_Label));
				int nextDensity_In = EditorGUILayout.DelayedIntField(Editor._meshAutoGenV2Option_Inner_Density, apGUILOFactory.I.Width(width_ValueLevel));

				//버튼으로 조절
				if (GUILayout.Button(_guiContent_imgValueLeft.Content, apGUIStyleWrapper.I.Button_TextFieldMargin, apGUILOFactory.I.Width(width_LevelBtn), apGUILOFactory.I.Height(18)))
				{
					nextDensity_In -= 1;
				}
				if (GUILayout.Button(_guiContent_imgValueRight.Content, apGUIStyleWrapper.I.Button_TextFieldMargin, apGUILOFactory.I.Width(width_LevelBtn), apGUILOFactory.I.Height(18)))
				{
					nextDensity_In += 1;
				}
				EditorGUILayout.EndHorizontal();

				//2. Margin (Out)
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Margin), apGUILOFactory.I.Width(width_Label));
				int nextMargin_Out = EditorGUILayout.DelayedIntField(Editor._meshAutoGenV2Option_OuterMargin, apGUILOFactory.I.Width(width_Value));
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);

				//3. Margin (In) > Padding
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
				GUILayout.Space(5);

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Padding), apGUILOFactory.I.Width(width_Label));				

				//Margin Option이 비활성이면 어두워야 한다.
				Color prevColor = GUI.backgroundColor;
				if(!Editor._meshAutoGenV2Option_IsInnerMargin)
				{
					GUI.backgroundColor = new Color(GUI.backgroundColor.r * 0.8f,
													GUI.backgroundColor.g * 0.6f,
													GUI.backgroundColor.b * 0.6f,
													GUI.backgroundColor.a);
				}
				int nextMargin_In = EditorGUILayout.DelayedIntField(Editor._meshAutoGenV2Option_InnerMargin, apGUILOFactory.I.Width(width_Value - 22));

				GUI.backgroundColor = prevColor;

				bool nextIsMargin_In = EditorGUILayout.Toggle(Editor._meshAutoGenV2Option_IsInnerMargin, apGUILOFactory.I.Width(20));
				EditorGUILayout.EndHorizontal();
				
				//설정 갱신
				if(nextDensity_In != Editor._meshAutoGenV2Option_Inner_Density
					|| nextMargin_Out != Editor._meshAutoGenV2Option_OuterMargin
					|| nextMargin_In != Editor._meshAutoGenV2Option_InnerMargin
					|| nextIsMargin_In != Editor._meshAutoGenV2Option_IsInnerMargin)
				{
					Editor._meshAutoGenV2Option_Inner_Density = Mathf.Clamp(nextDensity_In, 1, 10);//In은 1에서 20
					Editor._meshAutoGenV2Option_OuterMargin = Mathf.Max(nextMargin_Out, 1);
					Editor._meshAutoGenV2Option_InnerMargin = Mathf.Max(nextMargin_In, 1);
					Editor._meshAutoGenV2Option_IsInnerMargin = nextIsMargin_In;
					Editor.SaveEditorPref();//에디터 설정 저장
					apEditorUtil.ReleaseGUIFocus();
				}

				if(GUILayout.Button(Editor.GetUIWord(UIWORD.Default), apGUILOFactory.I.Height(18)))
				{
					//기본값으로 변경
					Editor._meshAutoGenV2Option_Inner_Density = 2;
					Editor._meshAutoGenV2Option_OuterMargin = 10;
					Editor._meshAutoGenV2Option_InnerMargin = 5;
					Editor._meshAutoGenV2Option_IsInnerMargin = false;
					Editor.SaveEditorPref();//에디터 설정 저장
					apEditorUtil.ReleaseGUIFocus();
				}

				if (_guiContent_MakeMesh_GenerateMesh == null)
				{
					_guiContent_MakeMesh_GenerateMesh = new apGUIContentWrapper();
					_guiContent_MakeMesh_GenerateMesh.AppendSpaceText(2, false);
					_guiContent_MakeMesh_GenerateMesh.AppendText(Editor.GetUIWord(UIWORD.GenerateMesh), true);
				}


				bool isAutoGenEnabled = Mesh._isPSDParsed;

				GUILayout.Space(5);
				
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_QuickMake), 
					_guiContent_MakeMesh_GenerateMesh.Content.text, 
					_guiContent_MakeMesh_GenerateMesh.Content.text, false, isAutoGenEnabled, width, 40))
				{
					//Generate 버튼
					bool isStartAutoGen = false;
					if (Editor.Select.Mesh._vertexData.Count > 0)
					{
						//버텍스를 모두 삭제할지 여부를 묻기
						//"Replace or Append vertices", "Do you want to replace existing vertices when creating the mesh?", "Replace", "Append", "Cancel"
						int iRemoveType = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.DLG_ReplaceAppendVertices_Title),
																				Editor.GetText(TEXT.DLG_ReplaceAppendVertices_Body),
																				Editor.GetText(TEXT.DLG_Replace),
																				Editor.GetText(TEXT.DLG_Append),
																				Editor.GetText(TEXT.Cancel));
						if(iRemoveType == 0)
						{
							//삭제
							apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_RemoveAllVertices, 
															Editor, 
															_mesh, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);
							_mesh._vertexData.Clear();
							_mesh._indexBuffer.Clear();
							_mesh._edges.Clear();
							_mesh._polygons.Clear();

							_mesh.MakeEdgesToPolygonAndIndexBuffer();

							Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

							Editor.VertController.UnselectVertex();
							Editor.VertController.UnselectNextVertex();
						}
						if (iRemoveType != 2)
						{
							//실행
							isStartAutoGen = true;
						}
					}
					else
					{
						//버텍스가 없다. 그냥 실행
						isStartAutoGen = true;						
					}

					if(isStartAutoGen)
					{
						apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_AutoGen, 
														Editor, 
														_mesh, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
						
						Editor.MeshGeneratorV2.ReadyToRequest(OnMeshAutoGeneratedV2);
						Editor.MeshGeneratorV2.AddRequest(	Mesh,
															Editor._meshAutoGenV2Option_Inner_Density,
															Editor._meshAutoGenV2Option_OuterMargin,
															Editor._meshAutoGenV2Option_InnerMargin,
															Editor._meshAutoGenV2Option_IsInnerMargin);

						Editor.MeshGeneratorV2.StartGenerate();//시작!

						//프로그래스바도 출현
						Editor.StartProgressPopup("Mesh Generation", "Generating..", true, OnAutoGenProgressCancel);
						
					}
				}
			}



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//"Remove All Vertices"
			//이전
			//if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.RemoveAllVertices), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform), "Remove all Vertices and Polygons"), GUILayout.Height(24)))

			//변경
			if (_guiContent_MeshProperty_RemoveAllVertices == null)
			{
				_guiContent_MeshProperty_RemoveAllVertices = apGUIContentWrapper.Make(
					2,
					Editor.GetUIWord(UIWORD.RemoveAllVertices), 
					Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_RemoveVertices), 
					apStringFactory.I.RemoveAllVerticesAndPolygons);//"Remove all Vertices and Polygons"
			}

			// 모든 버텍스 삭제
			if (GUILayout.Button(_guiContent_MeshProperty_RemoveAllVertices.Content, apGUILOFactory.I.Height(30)))
			{
				//v1.4.2 : 모달을 체크하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveMeshVertices_Title),
																	Editor.GetText(TEXT.RemoveMeshVertices_Body),
																	Editor.GetText(TEXT.RemoveMeshVertices_Okay),
																	Editor.GetText(TEXT.Cancel));

					if (isResult)
					{
						//Undo
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemoveAllVertices,
														Editor,
														_mesh,
														//null,
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						_mesh._vertexData.Clear();
						_mesh._indexBuffer.Clear();
						_mesh._edges.Clear();
						_mesh._polygons.Clear();

						_mesh.MakeEdgesToPolygonAndIndexBuffer();

						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영

						Editor.VertController.UnselectVertex();
						Editor.VertController.UnselectNextVertex();
					}
				}
			}

		}




		
		/// <summary>
		/// 메시 Modify UI 그리기
		/// </summary>
		private void DrawMeshProperty_Modify(int width, int height)
		{
			GUILayout.Space(5);

			//삭제 21.3.14 : 사용 방법 UI는 작업 공간으로 넘어갔다.
			//DrawHowToControl(width, Editor.GetUIWord(UIWORD.SelectVertex), null, null);//"Select Vertex"

			//EditorGUILayout.Space();

			bool isSingleVertex = Editor.VertController.Vertex != null && Editor.VertController.Vertices.Count == 1;
			bool isMultipleVertex = Editor.VertController.Vertices.Count > 1;

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Property_Modify_UI_Single, isSingleVertex);//"Mesh Property Modify UI Single"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Property_Modify_UI_Multiple, isMultipleVertex);//"Mesh Property Modify UI Multiple"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Property_Modify_UI_No_Info, (Editor.VertController.Vertex == null));//"Mesh Property Modify UI No Info"

			if (isSingleVertex)
			{
				if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Property_Modify_UI_Single))//"Mesh Property Modify UI Single"
				{
					if (_strWrapper_64 == null)
					{
						_strWrapper_64 = new apStringWrapper(64);
					}
					_strWrapper_64.Clear();
					_strWrapper_64.Append(apStringFactory.I.Index_Colon, false);
					_strWrapper_64.Append(Editor.VertController.Vertex._index, true);


					EditorGUILayout.LabelField(_strWrapper_64.ToString());//"Index : " + Editor.VertController.Vertex._index

					GUILayout.Space(5);

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position));//"Position"
					Vector2 prevPos2 = Editor.VertController.Vertex._pos;
					Vector2 nextPos2 = apEditorUtil.DelayedVector2Field(Editor.VertController.Vertex._pos, width - 10);

					GUILayout.Space(5);

					EditorGUILayout.LabelField(apStringFactory.I.UV);//"UV"
					Vector2 prevUV = Editor.VertController.Vertex._uv;
					Vector2 nextUV = apEditorUtil.DelayedVector2Field(Editor.VertController.Vertex._uv, width - 10);

					GUILayout.Space(5);

					_strWrapper_64.Clear();
					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Z_Depth), false);
					_strWrapper_64.Append(apStringFactory.I.DepthZeroToOne, true);

					EditorGUILayout.LabelField(_strWrapper_64.ToString());//Editor.GetUIWord(UIWORD.Z_Depth) + " (0~1)"
					float prevDepth = Editor.VertController.Vertex._zDepth;
					//float nextDepth = EditorGUILayout.DelayedFloatField(Editor.VertController.Vertex._zDepth, GUILayout.Width(width));
					float nextDepth = EditorGUILayout.Slider(Editor.VertController.Vertex._zDepth, 0.0f, 1.0f, apGUILOFactory.I.Width(width));

					if (nextPos2.x != prevPos2.x ||
						nextPos2.y != prevPos2.y ||
						nextUV.x != prevUV.x ||
						nextUV.y != prevUV.y ||
						nextDepth != prevDepth)
					{
						//Vertex 정보가 바뀌었다.
						apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertexDepth, 
														Editor, 
														Mesh, 
														//Editor.VertController.Vertex, 
														true,//연속으로 변경
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						Editor.VertController.Vertex._pos = nextPos2;
						Editor.VertController.Vertex._uv = nextUV;
						Editor.VertController.Vertex._zDepth = nextDepth;

						//Mesh.RefreshPolygonsToIndexBuffer();
						Editor.SetRepaint();

					}
				}
			}
			else if (isMultipleVertex)
			{
				if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Property_Modify_UI_Multiple))//"Mesh Property Modify UI Multiple"
				{
					if (_strWrapper_64 == null)
					{
						_strWrapper_64 = new apStringWrapper(64);
					}
					_strWrapper_64.Clear();
					_strWrapper_64.Append(Editor.VertController.Vertices.Count, false);
					_strWrapper_64.AppendSpace(1, false);
					_strWrapper_64.Append(apStringFactory.I.VerticesSelected, true);

					EditorGUILayout.LabelField(_strWrapper_64.ToString());//Editor.VertController.Vertices.Count + " Vertices Selected"

					GUILayout.Space(5);

					_strWrapper_64.Clear();
					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Z_Depth), false);
					_strWrapper_64.Append(apStringFactory.I.DepthZeroToOne, true);

					EditorGUILayout.LabelField(_strWrapper_64.ToString());//Editor.GetUIWord(UIWORD.Z_Depth) + " (0~1)"

					float prevDepth_Avg = 0.0f;
					float prevDepth_Min = -1.0f;
					float prevDepth_Max = -1.0f;

					apVertex vert = null;
					for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
					{
						vert = Editor.VertController.Vertices[i];

						prevDepth_Avg += vert._zDepth;
						if (prevDepth_Min < 0.0f || vert._zDepth < prevDepth_Min)
						{
							prevDepth_Min = vert._zDepth;
						}

						if (prevDepth_Max < 0.0f || vert._zDepth > prevDepth_Max)
						{
							prevDepth_Max = vert._zDepth;
						}
					}

					prevDepth_Avg /= Editor.VertController.Vertices.Count;

					_strWrapper_64.Clear();
					_strWrapper_64.Append(apStringFactory.I.Min_Colon, false);
					_strWrapper_64.Append(prevDepth_Min, true);

					EditorGUILayout.LabelField(_strWrapper_64.ToString());//"Min : " + prevDepth_Min

					_strWrapper_64.Clear();
					_strWrapper_64.Append(apStringFactory.I.Max_Colon, false);
					_strWrapper_64.Append(prevDepth_Max, true);

					EditorGUILayout.LabelField(_strWrapper_64.ToString());//"Max : " + prevDepth_Max

					_strWrapper_64.Clear();
					_strWrapper_64.Append(apStringFactory.I.Average_Colon, false);
					_strWrapper_64.Append(prevDepth_Avg, true);

					EditorGUILayout.LabelField(_strWrapper_64.ToString());//"Average : " + prevDepth_Avg

					GUILayout.Space(5);

					int heightSetWeight = 25;
					int widthSetBtn = 90;
					int widthIncDecBtn = 30;
					int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

					bool isDepthChanged = false;
					float nextDepth = 0.0f;
					int calculateType = 0;


					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightSetWeight));
					GUILayout.Space(5);

					EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(widthValue), apGUILOFactory.I.Height(heightSetWeight - 2));
					GUILayout.Space(8);
					_meshEdit_zDepthWeight = EditorGUILayout.DelayedFloatField(_meshEdit_zDepthWeight);

					EditorGUILayout.EndVertical();

					//"Set Weight"
					if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.SetWeight), false, true, widthSetBtn, heightSetWeight, apStringFactory.I.SetMeshZDepthWeightTooltip))//"Specify the Z value of the vertex. The larger the value, the more in front."
					{
						isDepthChanged = true;
						nextDepth = _meshEdit_zDepthWeight;
						calculateType = 1;
						GUI.FocusControl(null);
					}

					if (apEditorUtil.ToggledButton(apStringFactory.I.Plus, false, true, widthIncDecBtn, heightSetWeight))//"+"
					{
						////0.05 단위로 올라가거나 내려온다. (5%)
						isDepthChanged = true;
						nextDepth = 0.05f;
						calculateType = 2;

						GUI.FocusControl(null);
					}
					if (apEditorUtil.ToggledButton(apStringFactory.I.Minus, false, true, widthIncDecBtn, heightSetWeight))//"-"
					{
						//0.05 단위로 올라가거나 내려온다. (5%)
						isDepthChanged = true;
						nextDepth = -0.05f;
						calculateType = 2;

						GUI.FocusControl(null);
					}
					EditorGUILayout.EndHorizontal();


					if (isDepthChanged)
					{
						apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertexDepth, 
														Editor, 
														Mesh, 
														//Editor.VertController.Vertex, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						if (calculateType == 1)
						{
							//SET : 선택된 모든 Vertex의 값을 지정한다.
							for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
							{
								vert = Editor.VertController.Vertices[i];
								vert._zDepth = Mathf.Clamp01(nextDepth);
							}
						}
						else if (calculateType == 2)
						{
							//ADD : 선택된 Vertex 각각의 값을 증감한다.
							for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
							{
								vert = Editor.VertController.Vertices[i];
								vert._zDepth = Mathf.Clamp01(vert._zDepth + nextDepth);
							}
						}

						//Mesh.RefreshPolygonsToIndexBuffer();
						Editor.SetRepaint();
					}

				}


			}
			else
			{
				if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Property_Modify_UI_No_Info))//"Mesh Property Modify UI No Info"
				{
					EditorGUILayout.LabelField(apStringFactory.I.NoVertexSelected);//"No vertex selected"
				}
			}

			GUILayout.Space(20);

			//"Z-Depth Rendering"
			if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.Z_DepthRendering), Editor.GetUIWord(UIWORD.Z_DepthRendering), Editor._meshEditZDepthView == apEditor.MESH_EDIT_RENDER_MODE.ZDepth, true, width, 30))
			{
				if (Editor._meshEditZDepthView == apEditor.MESH_EDIT_RENDER_MODE.Normal)
				{
					Editor._meshEditZDepthView = apEditor.MESH_EDIT_RENDER_MODE.ZDepth;
				}
				else
				{
					Editor._meshEditZDepthView = apEditor.MESH_EDIT_RENDER_MODE.Normal;
				}
			}
			GUILayout.Space(5);
			//"Make Polygons"


			//이전
			//if (GUILayout.Button(new GUIContent(Editor.GetUIWord(UIWORD.MakePolygons), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakePolygon), "Make Polygons and Refresh Mesh"), GUILayout.Width(width), GUILayout.Height(40)))

			//변경
			if (_guiContent_MeshProperty_MakePolygones == null)
			{
				_guiContent_MeshProperty_MakePolygones = apGUIContentWrapper.Make(Editor.GetUIWord(UIWORD.MakePolygons), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakePolygon), apStringFactory.I.MakePolygonsAndRefreshMesh);//"Make Polygons and Refresh Mesh"
			}
			if (GUILayout.Button(_guiContent_MeshProperty_MakePolygones.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40)))
			{
				//Undo
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MakeEdges, 
												Editor, 
												Editor.Select.Mesh, 
												//Editor.Select.Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				//Editor.VertController.StopEdgeWire();

				_mesh.MakeEdgesToPolygonAndIndexBuffer();
				_mesh.RefreshPolygonsToIndexBuffer();

				//Pin-Weight 갱신
				//옵션이 없어도 무조건 Weight 갱신
				if(_mesh._pinGroup != null)
				{
					_mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
				}

				Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
			}
		}


		/// <summary>
		/// 메시 탭의 "Pivot" UI 그리기
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawMeshProperty_Pivot(int width, int height)
		{
			//GUILayout.Space(10);
			GUILayout.Space(5);


			//삭제 21.3.14 : 사용 방법 UI는 작업 공간으로 넘어갔다.
			//DrawHowToControl(width, Editor.GetUIWord(UIWORD.MovePivot), null, null, null);
			//EditorGUILayout.Space();

			//"Reset Pivot"
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetPivot), apGUILOFactory.I.Height(40)))
			{
				//아예 함수로 만들것
				//이전 코드
				//>> OffsetPos만 바꾸는 코드
				//apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SetPivot, Editor, _mesh, _mesh._offsetPos, false);

				//Editor.Select.Mesh._offsetPos = Vector2.zero;//<TODO : 이걸 사용하는 MeshGroup들의 DefaultPos를 역연산해야한다.

				//Editor.Select.Mesh.MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자

				Editor.Controller.SetMeshPivot(Editor.Select.Mesh, Vector2.zero);
			}
		}



		/// <summary>
		/// 메시 탭의 "Pin" UI 그리기
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawMeshProperty_Pin(int width, int height)
		{
			//GUILayout.Space(10);
			
			GUILayout.Space(5);


			if(_guiContent_MeshProperty_PinRangeOption == null)
			{
				_guiContent_MeshProperty_PinRangeOption = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Range), Editor.ImageSet.Get(apImageSet.PRESET.PinOption_Range));
			}
			if(_guiContent_MeshProperty_PinCalculateWeight == null)
			{
				_guiContent_MeshProperty_PinCalculateWeight = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.RefreshWeight), Editor.ImageSet.Get(apImageSet.PRESET.PinCalculateWeight));
			}
			if(_guiContent_MeshProperty_PinResetTestPos == null)
			{
				_guiContent_MeshProperty_PinResetTestPos = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.ResetTestPosition), Editor.ImageSet.Get(apImageSet.PRESET.PinRestTestPos));
			}
			if(_guiContent_MeshProperty_RemoveAllPins == null)
			{
				_guiContent_MeshProperty_RemoveAllPins = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.RemoveAllPins), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}


			int subTabWidth = (width / 4) - 4;

			// Pin 편집 중에 따라서 UI가 바뀐다.
			//4개의 편집 툴이 나온다.
				
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_PinSelect),
											Editor._meshEditMode_Pin_ToolMode == apEditor.MESH_EDIT_PIN_TOOL_MODE.Select,
											true,
											subTabWidth, 35, apStringFactory.I.SelectPins))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if(isExecutable)
				{
					SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Select);
				}
				
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_PinAdd),
											Editor._meshEditMode_Pin_ToolMode == apEditor.MESH_EDIT_PIN_TOOL_MODE.Add,
											true,
											subTabWidth, 35, apStringFactory.I.AddPins))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Add);
				}
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_PinLink),
											Editor._meshEditMode_Pin_ToolMode == apEditor.MESH_EDIT_PIN_TOOL_MODE.Link,
											true,
											subTabWidth, 35, apStringFactory.I.LinkPins))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Link);
				}
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_PinTest),
											Editor._meshEditMode_Pin_ToolMode == apEditor.MESH_EDIT_PIN_TOOL_MODE.Test,
											true,
											subTabWidth, 35, apStringFactory.I.TestPins))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE.Test);
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			string strPinEditInfo = null;
			Color prevColor = GUI.backgroundColor;
			Color colorPinEdit = Color.black;
			switch (Editor._meshEditMode_Pin_ToolMode)
			{
				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Select:
					strPinEditInfo = Editor.GetUIWord(UIWORD.SelectPins);//"핀 선택하기"
					colorPinEdit = new Color(0.5f, 0.9f, 0.6f, 1.0f);
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Add:
					strPinEditInfo = Editor.GetUIWord(UIWORD.AddPins);//"핀 추가하기"
					colorPinEdit = new Color(0.95f, 0.65f, 0.65f, 1.0f);
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Link:
					strPinEditInfo = Editor.GetUIWord(UIWORD.LinkPins);//"핀 연결하기"
					colorPinEdit = new Color(0.57f, 0.82f, 0.95f, 1.0f);
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Test:
					strPinEditInfo = Editor.GetUIWord(UIWORD.TestPins);//"핀 테스트하기"
					colorPinEdit = new Color(0.9f, 0.9f, 0.4f, 1.0f);
					break;
			}


			//선택 모드시 삭제 단축키
			if (Editor._meshEditMode_Pin_ToolMode == apEditor.MESH_EDIT_PIN_TOOL_MODE.Select)
			{
				Editor.AddHotKeyEvent(OnHotKeyEvent_RemovePin, apHotKeyMapping.KEY_TYPE.RemoveObject, null);
			}


			GUI.backgroundColor = colorPinEdit;
			GUILayout.Box((strPinEditInfo != null ? strPinEditInfo : apStringFactory.I.None), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 4), apGUILOFactory.I.Height(34));
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);

			//선택된 핀에 대해서 속성을 보여주자
			
			int nPinSelected = _selectedPinList != null ? _selectedPinList.Count : 0;

			bool isPinSelected_None = nPinSelected == 0;
			bool isPinSelected_Single = (nPinSelected == 1 && _selectedPin != null);
			bool isPinSelected_Multiple = (nPinSelected > 1);

			

			//Range, Fade
			//각각 Int 박스

			int curValue_Range = 0;
			int curValue_Fade = 0;
			apMeshPin.TANGENT_TYPE curValue_Tangent = apMeshPin.TANGENT_TYPE.Smooth;

			//여러개인 경우 동기화 되었는지가 중요하다
			//bool isMultipleSync_Range = true;
			//bool isMultipleSync_Fade = true;

			apEditorUtil.VALUE_SYNC_STATUS syncStatus_Range = apEditorUtil.VALUE_SYNC_STATUS.SingleSelectedOrSync;
			apEditorUtil.VALUE_SYNC_STATUS syncStatus_Fade = apEditorUtil.VALUE_SYNC_STATUS.SingleSelectedOrSync;
			
			apEditorUtil.VALUE_SYNC_STATUS syncStatus_Tangent = apEditorUtil.VALUE_SYNC_STATUS.SingleSelectedOrSync;

			if (isPinSelected_None)
			{
				syncStatus_Range = apEditorUtil.VALUE_SYNC_STATUS.NoSelected;
				syncStatus_Fade = apEditorUtil.VALUE_SYNC_STATUS.NoSelected;
				syncStatus_Tangent = apEditorUtil.VALUE_SYNC_STATUS.NoSelected;
			}
			else if(isPinSelected_Single)
			{
				curValue_Range = _selectedPin._range;
				curValue_Fade = _selectedPin._fade;

				curValue_Tangent = _selectedPin._tangentType;
			}
			else if(isPinSelected_Multiple)
			{
				//값 동기화 체크				
				//isMultipleSync_Range = true;
				//isMultipleSync_Fade = true;

				apMeshPin pin = null;

				for (int i = 0; i < nPinSelected; i++)
				{
					pin = _selectedPinList[i];
					if(i == 0)
					{
						curValue_Range = pin._range;
						curValue_Fade = pin._fade;
						curValue_Tangent = pin._tangentType;
					}
					else
					{
						//첫번째 핀 외에는 값이 같은지만 체크한다.
						if(curValue_Range != pin._range)
						{
							syncStatus_Range = apEditorUtil.VALUE_SYNC_STATUS.NotSync;
						}

						if(curValue_Fade != pin._fade)
						{
							syncStatus_Fade = apEditorUtil.VALUE_SYNC_STATUS.NotSync;
						}

						if(curValue_Tangent != pin._tangentType)
						{
							syncStatus_Tangent = apEditorUtil.VALUE_SYNC_STATUS.NotSync;
						}

						if(syncStatus_Range == apEditorUtil.VALUE_SYNC_STATUS.NotSync 
							&& syncStatus_Fade == apEditorUtil.VALUE_SYNC_STATUS.NotSync
							&& syncStatus_Tangent == apEditorUtil.VALUE_SYNC_STATUS.NotSync)
						{
							//셋다 동기화 실패시 더 체크할 필요가 없다.
							break;
						}
					}
				}
			}

			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//탄젠트 타입
			int width_2Btn = (width / 2) - 2;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Height(30));
			GUILayout.Space(4);

			if(apEditorUtil.ToggledButton_2Side_Sync(Editor.ImageSet.Get(apImageSet.PRESET.PinOption_Tangent_Smooth),
				curValue_Tangent == apMeshPin.TANGENT_TYPE.Smooth,
				syncStatus_Tangent != apEditorUtil.VALUE_SYNC_STATUS.NoSelected,
				syncStatus_Tangent == apEditorUtil.VALUE_SYNC_STATUS.SingleSelectedOrSync,
				width_2Btn, 30))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					if (nPinSelected > 0)
					{
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

						apMeshPin pin = null;
						for (int i = 0; i < nPinSelected; i++)
						{
							pin = _selectedPinList[i];
							pin._tangentType = apMeshPin.TANGENT_TYPE.Smooth;
						}

						//옵션에 따른 가중치 재계산
						RecalculatePinWeightByOption();
					}
				}
				
			}
			if(apEditorUtil.ToggledButton_2Side_Sync(Editor.ImageSet.Get(apImageSet.PRESET.PinOption_Tangent_Sharp),
				curValue_Tangent == apMeshPin.TANGENT_TYPE.Sharp,
				syncStatus_Tangent != apEditorUtil.VALUE_SYNC_STATUS.NoSelected,
				syncStatus_Tangent == apEditorUtil.VALUE_SYNC_STATUS.SingleSelectedOrSync,
				width_2Btn, 30))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					if (nPinSelected > 0)
					{
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_ChangePin,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

						apMeshPin pin = null;
						for (int i = 0; i < nPinSelected; i++)
						{
							pin = _selectedPinList[i];
							pin._tangentType = apMeshPin.TANGENT_TYPE.Sharp;
						}

						//옵션에 따른 가중치 재계산
						RecalculatePinWeightByOption();
					}
				}
			}



			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			EditorGUILayout.LabelField(_guiContent_MeshProperty_PinRangeOption.Content, apGUILOFactory.I.Height(24));

			GUILayout.Space(4);
			//대상이 없거나 동기화가 안되었다면 색상 적용
			int width_Label = 100;
			int width_Value = width - (10 + width_Label + 2);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Radius), apGUILOFactory.I.Width(width_Label));

			apEditorUtil.SetBackgroundColorBySync(syncStatus_Range);
			int nextRange = EditorGUILayout.DelayedIntField(curValue_Range, apGUILOFactory.I.Width(width_Value));
			GUI.backgroundColor = prevColor;
			EditorGUILayout.EndHorizontal();

			

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Falloff), apGUILOFactory.I.Width(width_Label));

			apEditorUtil.SetBackgroundColorBySync(syncStatus_Fade);
			int nextFade = EditorGUILayout.DelayedIntField(curValue_Fade, apGUILOFactory.I.Width(width_Value));
			GUI.backgroundColor = prevColor;
			EditorGUILayout.EndHorizontal();

			if(nextRange != curValue_Range && !isPinSelected_None)
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				if(Editor.Gizmos.IsFFDMode)
				{
					Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();//취소 없는 FFD 종료 체크
				}

				//값 적용 [Range]
				if(nextRange < 1)
				{
					nextRange = 1;
				}
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_ChangePin, 
												Editor, 
												Mesh, 
												//Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				apMeshPin pin = null;
				for (int i = 0; i < nPinSelected; i++)
				{
					pin = _selectedPinList[i];
					pin._range = nextRange;
				}

				//옵션에 따른 가중치 재계산
				RecalculatePinWeightByOption();
			}


			if(nextFade != curValue_Fade && !isPinSelected_None)
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				if(Editor.Gizmos.IsFFDMode)
				{
					Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();//취소 없는 FFD 종료 체크
				}

				//값 적용 [Fade]
				if(nextFade < 0)
				{
					nextFade = 0;
				}
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_ChangePin, 
												Editor, 
												Mesh, 
												//Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				apMeshPin pin = null;
				for (int i = 0; i < nPinSelected; i++)
				{
					pin = _selectedPinList[i];
					pin._fade = nextFade;
				}

				//옵션에 따른 가중치 재계산
				RecalculatePinWeightByOption();
			}



			//추가 1.4.2 : 복사/붙여넣기
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			int width_CopyBtn = (width - 5) / 2;
			int height_CopyBtn = 25;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_CopyBtn));
			GUILayout.Space(5);

			//" Copy"
			if(_guiContent_CopyTextIcon == null)
			{
				_guiContent_CopyTextIcon = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Copy), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy));
			}
			if(_guiContent_PasteTextIcon == null)
			{
				_guiContent_PasteTextIcon = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Paste), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste));
			}


			// 핀 복사하기
				
			if(apEditorUtil.ToggledButton_2Side(_guiContent_CopyTextIcon, false, nPinSelected > 0, width_CopyBtn, height_CopyBtn))
			{
				apSnapShotManager.I.Copy_MeshPins(Mesh, _selectedPinList);
				apEditorUtil.ReleaseGUIFocus();
			}

			//버텍스 - Edge 붙여넣기
			if(apEditorUtil.ToggledButton_2Side(_guiContent_PasteTextIcon, false, apSnapShotManager.I.IsPastable_MeshPins(), width_CopyBtn, height_CopyBtn))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{

					//변경 1.4.2 : 바로 붙이지 않고, 좌표 변환을 물어본다.
					_loadKey_PasteMestVertPin = apDialog_CopyMeshVertPin.ShowDialog(Editor,
																						Mesh,
																						OnCopyMeshPins,
																						true);//true : 핀을 복사한다.
				}
			}

			EditorGUILayout.EndHorizontal();




			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			

			//연결 갱신 버튼
			//자동 갱신 설정
			if(apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.PinCalculateWeightAuto), 1,
													Editor.GetUIWord(UIWORD.AutoRefreshON), Editor.GetUIWord(UIWORD.AutoRefreshOFF), 
													Editor._pinOption_AutoWeightRefresh,
													true, width, 30))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					Editor._pinOption_AutoWeightRefresh = !Editor._pinOption_AutoWeightRefresh;
					Editor.SaveEditorPref();
				}
			}
			GUILayout.Space(2);
			if(GUILayout.Button(_guiContent_MeshProperty_PinCalculateWeight.Content, apGUILOFactory.I.Height(40)))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_CalculatePinWeight,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					Mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
				}
			}




			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);



			if (Editor._meshEditMode_Pin_ToolMode == apEditor.MESH_EDIT_PIN_TOOL_MODE.Test)
			{	
				//위치 리셋 버튼 (테스트 중일 때)
				if (GUILayout.Button(_guiContent_MeshProperty_PinResetTestPos.Content, apGUILOFactory.I.Height(30)))
				{	
					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor.Select.Mesh._pinGroup.Test_ResetMatrixAll(apMeshPin.TMP_VAR_TYPE.MeshTest);
					}
				}

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);
			}



			
			//"모든 핀 삭제"
			if(GUILayout.Button(_guiContent_MeshProperty_RemoveAllPins.Content, apGUILOFactory.I.Height(24)))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					bool isRemove = EditorUtility.DisplayDialog(Editor.GetText(TEXT.DLG_RemovePin_Title),
																Editor.GetText(TEXT.DLG_RemovePin_All_Body),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));
					if (isRemove)
					{
						apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemovePin,
														Editor,
														Mesh,
														//Mesh, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						UnselectMeshPins();
						Mesh._pinGroup.Clear();
						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
					}
				}
			}
		}


		private apHotKey.HotKeyResult OnHotKeyEvent_RemovePin(object paramObj)
		{
			if (_selectionType != SELECTION_TYPE.Mesh
				|| _mesh == null
				|| _mesh._pinGroup == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select)
			{
				return null;
			}

			//v1.4.2 탭 전환 전에 모달 상태를 확인하자
			bool isExecutable = Editor.CheckModalAndExecutable();

			if (!isExecutable)
			{
				return null;
			}

			//선택한 핀들
			int nPinSelected = _selectedPinList != null ? _selectedPinList.Count : 0;
			if(nPinSelected == 0)
			{
				return null;
			}

			
			//선택된 핀에 대해서 속성을 보여주자
			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemovePin,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

			//별도의 리스트에 삭제할 핀을 모은다.
			apMeshPin curPin = null;
			apMeshPin prevPin = null;
			apMeshPin nextPin = null;

			apMeshPinGroup pinGroup = Mesh._pinGroup;

			List<apMeshPin> removePins = new List<apMeshPin>();
			for (int i = 0; i < nPinSelected; i++)
			{
				curPin = _selectedPinList[i];
				removePins.Add(curPin);
			}
			
			UnselectMeshPins();

			//삭제하는걸 설정
			for (int i = 0; i < nPinSelected; i++)
			{
				curPin = removePins[i];

				// 연결부터 끊자
				prevPin = curPin._prevPin;
				nextPin = curPin._nextPin;

				if(prevPin != null && prevPin._nextPin == curPin)
				{
					prevPin._nextPin = null;
					prevPin._nextPinID = -1;
					prevPin._nextCurve = null;
				}

				if(nextPin != null && nextPin._prevPin == curPin)
				{
					nextPin._prevPin = null;
					nextPin._prevPinID = -1;
					nextPin._prevCurve = null;
				}

				//리스트에서 삭제한다.
				pinGroup._pins_All.Remove(curPin);
				pinGroup._curves_All.RemoveAll(delegate(apMeshPinCurve a)
				{
					return a._prevPin == curPin || a._nextPin == curPin;
				});

				//_portrait.PushUnusedID(apIDManager.TARGET.MeshPin, curPin._uniqueID);
				
			}

			Mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
			Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
		
			return apHotKey.HotKeyResult.MakeResult();
		}


		
		
		/// <summary>
		/// 메시 UI에 공통으로 나오는 "폴리곤 도구들"
		/// </summary>
		private void DrawMakePolygonsTool(int width)
		{
			// Make Mesh + Auto Link

			if (_guiContent_MeshProperty_AutoLinkEdge == null)
			{
				_guiContent_MeshProperty_AutoLinkEdge = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AutoLinkEdge), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_AutoLink));
			}
			if (_guiContent_MeshProperty_Draw_MakePolygones == null)
			{
				_guiContent_MeshProperty_Draw_MakePolygones = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.MakePolygons), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MakePolygon), apStringFactory.I.MakePolygonsAndRefreshMesh);//"Make Polygons and Refresh Mesh"
			}


			//"Auto Link Edge"
			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AutoLinkEdge), Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_AutoLink), "Automatically creates edges connecting vertices"), GUILayout.Height(30)))
			//변경
			if (GUILayout.Button(_guiContent_MeshProperty_AutoLinkEdge.Content, apGUILOFactory.I.Height(30)))
			{
				//Undo

				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MakeEdges,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					//Editor.VertController.StopEdgeWire();
					Mesh.AutoLinkEdges();
				}
			}
			GUILayout.Space(10);
			
			
			// [ Make Polygons ]
			if (GUILayout.Button(_guiContent_MeshProperty_Draw_MakePolygones.Content, apGUILOFactory.I.Height(50)))
			{
				//v1.4.2 탭 전환 전에 모달 상태를 확인하자
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					//Undo
					apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MakeEdges,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					//Editor.VertController.StopEdgeWire();

					Mesh.MakeEdgesToPolygonAndIndexBuffer();
					Mesh.RefreshPolygonsToIndexBuffer();
					Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영


					//옵션에 따른 핀-버텍스 가중치 재계산 [v1.4.0]
					//RecalculatePinWeightByOption();//옵션이 있는 경우에만

					//옵션이 없어도 무조건 Weight 갱신
					if (_mesh != null && _mesh._pinGroup != null)
					{
						_mesh._pinGroup.Refresh(apMeshPinGroup.REFRESH_TYPE.RecalculateAll);
					}


					apEditorUtil.ReleaseGUIFocus();
				}
			}

			//추가 20.12.4 : 단축키로 Polygon 생성
			Editor.AddHotKeyEvent(OnHotKeyEvent_MakeMeshPolygon, apHotKeyMapping.KEY_TYPE.MakeMesh_MakePolygon, Mesh);
		}



		
		/// <summary>
		/// 메시 UI 중 "미러 툴" UI를 그린다.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="isUseCopyTool"></param>
		/// <param name="isUseSnapTool"></param>
		private void DrawMakeMeshMirrorTool(int width, bool isUseCopyTool, bool isUseSnapTool)
		{
			//- 미러 복사
			//미러 모드는 직접 복사하기 전의 축 설정에서는 모달의 영향을 받지 않는다.

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MirrorTool));//"Mirror Tool"
			GUILayout.Space(5);
			//X/Y 툴 켜기/끄기 (+ 위치)
			bool isMirrorEnabled = (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror);
			bool isMirrorX = Mesh._isMirrorX;

			int nVertices = Editor.VertController.Vertices != null ? Editor.VertController.Vertices.Count : 0;


			//미러 모드 켜고 끄기
			if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.MirrorEnabled), Editor.GetUIWord(UIWORD.MirrorDisabled), isMirrorEnabled, true, width, 30))//"Mirror Enabled" / "Mirror Disabled"
			{
				isMirrorEnabled = !isMirrorEnabled;
				Editor._meshEditMirrorMode = isMirrorEnabled ? apEditor.MESH_EDIT_MIRROR_MODE.Mirror : apEditor.MESH_EDIT_MIRROR_MODE.None;
				apEditorUtil.ReleaseGUIFocus();
			}


			//int width_AxisBtn = 60;
			int width_AxisBtn = (width - (5)) / 2;
			//int width_CopyBtn = width - (10 + width_AxisBtn * 2 + 7);
			int height_Axis = 30;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_Axis));
			GUILayout.Space(5);

			//GUIStyle guiStyle_AxisValue = new GUIStyle(GUI.skin.textField);
			//guiStyle_AxisValue.margin = GUI.skin.button.margin;

			//X/Y축 설정 + 복사하기
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MirrorAxis_X), isMirrorX, isMirrorEnabled, width_AxisBtn, height_Axis))
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SettingChanged,
												Editor,
												Mesh,
												//Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
				Mesh._isMirrorX = true;
				isMirrorX = Mesh._isMirrorX;
				apEditorUtil.ReleaseGUIFocus();
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_MirrorAxis_Y), !isMirrorX, isMirrorEnabled, width_AxisBtn, height_Axis))
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SettingChanged,
												Editor,
												Mesh,
												//Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
				Mesh._isMirrorX = false;
				isMirrorX = Mesh._isMirrorX;
				apEditorUtil.ReleaseGUIFocus();
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			//축 위치 바꾸기
			int width_Label = 100;
			int width_Value = width - (10 + width_Label);
			int width_Label_Long = 200;
			int width_Value_Long = width - (10 + width_Label_Long);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.RulerSettings));//"Ruler Settings"
																			   //GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.RulerPosition), apGUILOFactory.I.Width(width_Label));//"Position"

			float prevAxisValue = isMirrorX ? Mesh._mirrorAxis.x : Mesh._mirrorAxis.y;

			float nextAxisValue = EditorGUILayout.DelayedFloatField(prevAxisValue, apGUIStyleWrapper.I.TextField_BtnMargin, apGUILOFactory.I.Width(width_Value));
			if (Mathf.Abs(nextAxisValue - prevAxisValue) > 0.0001f)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SettingChanged,
												Editor,
												Mesh,
												//Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				if (isMirrorX)
				{
					Mesh._mirrorAxis.x = nextAxisValue;
				}
				else
				{
					Mesh._mirrorAxis.y = nextAxisValue;
				}
				apEditorUtil.ReleaseGUIFocus();
			}

			EditorGUILayout.EndHorizontal();


			//위치 : 상하좌우, + Area 중심으로 이동
			//버튼을 Left, Right, Up, Down, Move To Center로 설정

			int width_MoveAxisBtn = 26;
			int width_CenterAxisBtn = width - (10 + width_MoveAxisBtn * 4 + 8);
			int height_MoveAxisBtn = 20;
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_MoveAxisBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Left), false, isMirrorX && isMirrorEnabled, width_MoveAxisBtn, height_MoveAxisBtn))
			{
				//Axis Y의 X 이동 (-) (Left)
				if (isMirrorX && isMirrorEnabled)
				{
					apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SettingChanged,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					Mesh._mirrorAxis.x -= 2;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Right), false, isMirrorX && isMirrorEnabled, width_MoveAxisBtn, height_MoveAxisBtn))
			{
				//Axis Y의 X 이동 (+) (Right)
				if (isMirrorX && isMirrorEnabled)
				{
					apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SettingChanged,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					Mesh._mirrorAxis.x += 2;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Up), false, !isMirrorX && isMirrorEnabled, width_MoveAxisBtn, height_MoveAxisBtn))
			{
				//Axis X의 Y 이동 (+) (Up)
				if (!isMirrorX && isMirrorEnabled)
				{
					apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SettingChanged,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					Mesh._mirrorAxis.y += 2;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Down), false, !isMirrorX && isMirrorEnabled, width_MoveAxisBtn, height_MoveAxisBtn))
			{
				//Axis X의 Y 이동 (-) (Down)
				if (!isMirrorX && isMirrorEnabled)
				{
					apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SettingChanged,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
					Mesh._mirrorAxis.y -= 2;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.MoveToCenter), false, isMirrorEnabled, width_CenterAxisBtn, height_MoveAxisBtn))//"Move to Center"
			{
				if (isMirrorEnabled)
				{
					apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_SettingChanged,
													Editor,
													Mesh,
													//Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
					if (Mesh._isPSDParsed)
					{
						//Area의 중심으로 이동
						if (isMirrorX)
						{
							Mesh._mirrorAxis.x = (Mesh._atlasFromPSD_LT.x + Mesh._atlasFromPSD_RB.x) * 0.5f;
						}
						else
						{
							Mesh._mirrorAxis.y = (Mesh._atlasFromPSD_LT.y + Mesh._atlasFromPSD_RB.y) * 0.5f;
						}
					}
					else
					{
						//그냥 Zero
						if (isMirrorX)
						{
							Mesh._mirrorAxis.x = 0;
						}
						else
						{
							Mesh._mirrorAxis.y = 0;
						}
					}
					apEditorUtil.ReleaseGUIFocus();
				}
			}
			//GUILayout.Space(5 + (width - (10 + width_MoveAxisBtn)) / 2);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Offset), apGUILOFactory.I.Width(width_Label));//"Offset"

			float nextMirrorCopyOffset = EditorGUILayout.DelayedFloatField(Editor._meshTRSOption_MirrorOffset, apGUIStyleWrapper.I.TextField_BtnMargin, apGUILOFactory.I.Width(width_Value));
			if (Mathf.Abs(nextMirrorCopyOffset - Editor._meshTRSOption_MirrorOffset) > 0.0001f)
			{
				Editor._meshTRSOption_MirrorOffset = nextMirrorCopyOffset;
				Editor.SaveEditorPref();
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			if (isUseCopyTool)
			{
				GUILayout.Space(10);
				//복사
				//EditorGUILayout.LabelField("Mirror Copy");
				//" Copy"
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(isMirrorX ? apImageSet.PRESET.MeshEdit_MirrorCopy_X : apImageSet.PRESET.MeshEdit_MirrorCopy_Y), 1, Editor.GetUIWord(UIWORD.CopySymmetry), false, nVertices > 0 && isMirrorEnabled, width, 25))
				{
					//미러 복사하기

					//v1.4.2 탭 전환 전에 모달 상태를 확인하자
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						if (nVertices > 0)
						{
							Editor.Controller.DuplicateMirrorVertices();
						}
						apEditorUtil.ReleaseGUIFocus();
					}
				}
			}

			if (isUseSnapTool)
			{
				GUILayout.Space(10);
				//EditorGUILayout.LabelField("Snap to Ruler");
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(18));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SnapToRuler), apGUILOFactory.I.Width(width_Label_Long));//"Snap to Ruler"
				bool isNextSnapOpt = EditorGUILayout.Toggle(Editor._meshTRSOption_MirrorSnapVertOnRuler, apGUILOFactory.I.Width(width_Value_Long));
				if (isNextSnapOpt != Editor._meshTRSOption_MirrorSnapVertOnRuler)
				{
					Editor._meshTRSOption_MirrorSnapVertOnRuler = isNextSnapOpt;
					Editor.SaveEditorPref();
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(18));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.RemoveSymmetry), apGUILOFactory.I.Width(width_Label_Long));//"Remove Symmetry"
				bool isNextRemoveSymm = EditorGUILayout.Toggle(Editor._meshTRSOption_MirrorRemoved, apGUILOFactory.I.Width(width_Value_Long));
				if (isNextRemoveSymm != Editor._meshTRSOption_MirrorRemoved)
				{
					Editor._meshTRSOption_MirrorRemoved = isNextRemoveSymm;
					Editor.SaveEditorPref();
				}
				EditorGUILayout.EndHorizontal();


			}
		}

		/// <summary>
		/// Mesh의 기본 설정에서 Area 설정과 Mesh가 연결된 Texture의 Read 설정을 표시하는 UI.
		/// 자동 메시 생성과도 관련있다.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="isShowDetails"></param>
		private void DrawMeshAtlasOption(int width, bool isShowDetails)
		{

			if(_guiContent_MeshEdit_Area_Enabled == null)
			{
				_guiContent_MeshEdit_Area_Enabled = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AreaOptionEnabled));
			}
			if(_guiContent_MeshEdit_Area_Disabled == null)
			{
				_guiContent_MeshEdit_Area_Disabled = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AreaOptionDisabled));
			}
			if(_guiContent_MeshEdit_AreaEditing_Off == null)
			{
				_guiContent_MeshEdit_AreaEditing_Off = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.EditArea));
			}
			if(_guiContent_MeshEdit_AreaEditing_On == null)
			{
				_guiContent_MeshEdit_AreaEditing_On = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.EditingArea));
			}


			//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AreaSizeSettings));//"Area Size within the Atlas"
			//GUILayout.Space(5);

			//isPSDParsed 옵션 여부
			if (apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_Area), 
													_guiContent_MeshEdit_Area_Enabled.Content.text, 
													_guiContent_MeshEdit_Area_Disabled.Content.text, 
													Mesh._isPSDParsed, true, width, 30))//"Area Option Enabled", "Area Option Disabled"
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_AtlasChanged, 
												Editor, 
												Mesh, 
												//Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

				Mesh._isPSDParsed = !Mesh._isPSDParsed;
				Editor._isMeshEdit_AreaEditing = false;
				_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;

				if(Mesh._isPSDParsed)
				{
					//만약 Atlas 설정을 켤 때, 크기가 5 미만이면 강제로 10으로 설정
					float atlasWidth = Mathf.Abs(Mesh._atlasFromPSD_LT.x - Mesh._atlasFromPSD_RB.x);
					float atlasHeight = Mathf.Abs(Mesh._atlasFromPSD_LT.y - Mesh._atlasFromPSD_RB.y);
					if(atlasWidth < 4.0f)
					{
						int centerX = (int)(Mesh._atlasFromPSD_LT.x * 0.5f + Mesh._atlasFromPSD_RB.x * 0.5f);
						Mesh._atlasFromPSD_LT.x = centerX - 10;
						Mesh._atlasFromPSD_RB.x = centerX + 10;
					}
					if(atlasHeight < 4.0f)
					{
						int centerY = (int)(Mesh._atlasFromPSD_LT.y * 0.5f + Mesh._atlasFromPSD_RB.y * 0.5f);
						Mesh._atlasFromPSD_LT.y = centerY + 10;
						Mesh._atlasFromPSD_RB.y = centerY - 10;
					}
				}
			}

			//변경 19.11.20
			if (_guiContent_imgValueUp == null)
			{
				_guiContent_imgValueUp = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Up));
			}
			if (_guiContent_imgValueDown == null)
			{
				_guiContent_imgValueDown = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Down));
			}
			if (_guiContent_imgValueLeft == null)
			{
				_guiContent_imgValueLeft = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Left));
			}
			if (_guiContent_imgValueRight == null)
			{
				_guiContent_imgValueRight = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_ValueChange_Right));
			}

			
			//추가 21.1.6 : Area 편집 모드 (GUI로)//"Editing Area..", "Edit Area"
			if(apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.MeshEdit_AreaEditing),
													_guiContent_MeshEdit_AreaEditing_On.Content.text,
													_guiContent_MeshEdit_AreaEditing_Off.Content.text, 
													Editor._isMeshEdit_AreaEditing, Mesh._isPSDParsed, width, 35))
			{
				Editor._isMeshEdit_AreaEditing = !Editor._isMeshEdit_AreaEditing;
				_meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;
			}

			//int width_Label = 100;
			//int width_Value = width - (5 + width_Label);

			if (Mesh._isPSDParsed)
			{
				//이게 활성화 될 때에만 가능하다
				

				//영역을 설정
				//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AreaSize));//"Area Size"

				if (isShowDetails)
				{

					GUILayout.Space(5);

					//변경 21.1.7 : Area 크기 변경을 단순화하자. 위에 있으니까
					

					//변경 21.1.7 : 영역 LTRB를 간단하게 설정. LR / TB 순서로 표기한다.
					int width_Area_Label = 14;
					int width_Area_Value = ((width - (10 + width_Area_Label * 2)) / 2) - 2;
					//LR
					//TB 순으로
					GUILayout.Space(5);
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(18));
					GUILayout.Space(5);

					//L
					EditorGUILayout.LabelField(apStringFactory.I.L, apGUILOFactory.I.Width(width_Area_Label));
					float meshAtlas_Left = EditorGUILayout.DelayedFloatField(Mesh._atlasFromPSD_LT.x, apGUILOFactory.I.Width(width_Area_Value));
					//R
					EditorGUILayout.LabelField(apStringFactory.I.R, apGUILOFactory.I.Width(width_Area_Label));
					float meshAtlas_Right = EditorGUILayout.DelayedFloatField(Mesh._atlasFromPSD_RB.x, apGUILOFactory.I.Width(width_Area_Value));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(18));
					GUILayout.Space(5);
					//T
					EditorGUILayout.LabelField(apStringFactory.I.T, apGUILOFactory.I.Width(width_Area_Label));
					float meshAtlas_Top = EditorGUILayout.DelayedFloatField(Mesh._atlasFromPSD_LT.y, apGUILOFactory.I.Width(width_Area_Value));
					//B
					EditorGUILayout.LabelField(apStringFactory.I.B, apGUILOFactory.I.Width(width_Area_Label));
					float meshAtlas_Bottom = EditorGUILayout.DelayedFloatField(Mesh._atlasFromPSD_RB.y, apGUILOFactory.I.Width(width_Area_Value));
					EditorGUILayout.EndHorizontal();



					//Atlas의 범위 값이 바뀌었을 때
					if (meshAtlas_Top != Mesh._atlasFromPSD_LT.y
						|| meshAtlas_Left != Mesh._atlasFromPSD_LT.x
						|| meshAtlas_Right != Mesh._atlasFromPSD_RB.x
						|| meshAtlas_Bottom != Mesh._atlasFromPSD_RB.y)
					{
						apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_AtlasChanged, 
														Editor, 
														Mesh, 
														//Mesh, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						Mesh._atlasFromPSD_LT.y = meshAtlas_Top;
						Mesh._atlasFromPSD_LT.x = meshAtlas_Left;
						Mesh._atlasFromPSD_RB.x = meshAtlas_Right;
						Mesh._atlasFromPSD_RB.y = meshAtlas_Bottom;
						apEditorUtil.ReleaseGUIFocus();
					}

					GUILayout.Space(10);

				}			

			}
		}



		//------------------------------------------------------------------------
		// 메시 그룹 UI
		//------------------------------------------------------------------------
		
		private void Draw_MeshGroup(int width, int height)
		{
			//DrawTitle("Mesh Group", width);
			//EditorGUILayout.Space();

			if (_meshGroup == null)
			{
				SelectNone();
				return;
			}

			bool isEditMeshGroupMode_Setting = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting);
			bool isEditMeshGroupMode_Bone = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Bone);
			bool isEditMeshGroupMode_Modifier = (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier);
			int subTabWidth = (width / 2) - 4;
			int subTabHeight = 24;

			//탭 뒤 음영 배경
			Color prevColor = GUI.backgroundColor;
			if(EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color(	0.15f, 0.15f, 0.15f, 1.0f);
			}
			else
			{
				GUI.backgroundColor = new Color(	GUI.backgroundColor.r * 0.7f, GUI.backgroundColor.g * 0.7f, GUI.backgroundColor.b * 0.7f, 1.0f);
			}
#if UNITY_2019_1_OR_NEWER
			GUI.Box(new Rect(1, 0, width + 20, subTabHeight * 2 + 10), apStringFactory.I.None, apEditorUtil.WhiteGUIStyle);
#else
			GUI.Box(new Rect(1, 0, width + 20, subTabHeight * 2 + 13), apStringFactory.I.None, apEditorUtil.WhiteGUIStyle);
#endif	
			GUI.backgroundColor = prevColor;



			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));
			GUILayout.Space(5);

			//" Setting"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Setting), 1, Editor.GetUIWord(UIWORD.Setting), isEditMeshGroupMode_Setting, true, subTabWidth, subTabHeight, apStringFactory.I.SettingsOfMeshGroup))//"Settings of Mesh Group"
			{
				if (!isEditMeshGroupMode_Setting)
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Setting;
						_isMeshGroupSetting_EditDefaultTransform = false;

						//선택 모두 초기화
						
						//변경 20.6.11 : 통합 + 이 안에 "자동 ModMesh선택"이 있다.
						SelectSubObject(null, null, null, MULTI_SELECT.Main, TF_BONE_SELECT.Exclusive);

						SelectModifier(null);

						SetBoneEditing(false, false);//Bone 처리는 종료 

						//Gizmo 컨트롤 방식을 Setting에 맞게 바꾸자
						Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshGroupSetting());


						//이전
						//SetModifierEditMode(EX_EDIT_KEY_VALUE.None);

						//변경 22.5.14 : 편집 모드 해제
						SetModifierExclusiveEditing(EX_EDIT.None);

						//_rigEdit_isBindingEdit = false;//삭제 22.5.15
						_rigEdit_isTestPosing = false;
						SetBoneRiggingTest();


						//[v1.4.2] 리깅 도중에 탭 변경시, "모디파이어에 선택되지 않은 본/메시" 정보를 리셋한다.
						SetEnableMeshGroupExEditingFlagsForce();


						//스크롤 초기화 (오른쪽2)
						Editor.ResetScrollPosition(false, false, false, true, false);
					}
				}
			}

			//" Bone"
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Bone), 1, Editor.GetUIWord(UIWORD.Bone), isEditMeshGroupMode_Bone, true, subTabWidth, subTabHeight, apStringFactory.I.BonesOfMeshGroup))//"Bones of Mesh Group"
			{
				if (!isEditMeshGroupMode_Bone)
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Bone;
						_isBoneDefaultEditing = false;


						//선택 초기화
						
						//변경 20.6.11 : 통합 선택 + ModMesh선택 포함됨
						SelectSubObject(null, null, null, MULTI_SELECT.Main, TF_BONE_SELECT.Exclusive);


						SelectModifier(null);

						//일단 Gizmo 초기화
						Editor.Gizmos.Unlink();

						_meshGroupChildHierarchy = MESHGROUP_CHILD_HIERARCHY.Bones;//하단 UI도 변경

						//이전
						//SetModifierEditMode(EX_EDIT_KEY_VALUE.ParamKey_Bone);

						//변경 22.5.14 : 편집 모드 해제
						SetModifierExclusiveEditing(EX_EDIT.None);


						//_rigEdit_isBindingEdit = false;//삭제 22.5.15
						_rigEdit_isTestPosing = false;
						SetBoneRiggingTest();

						SetBoneEditing(false, true);


						//[v1.4.2] 리깅 도중에 탭 변경시, "모디파이어에 선택되지 않은 본/메시" 정보를 리셋한다.
						SetEnableMeshGroupExEditingFlagsForce();


						//스크롤 초기화 (오른쪽2)
						Editor.ResetScrollPosition(false, false, false, true, false);

						//추가 21.10.6
						//본 탭을 누를때 "본이 숨겨진 상태"면 본이 보여지도록 만든다.
						if (Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.None)
						{
							Editor._boneGUIRenderMode = apEditor.BONE_RENDER_MODE.Render;
						}
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(subTabHeight));
			GUILayout.Space(5);

			//Modifer
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Modifier), 1, Editor.GetUIWord(UIWORD.Modifier), isEditMeshGroupMode_Modifier, true, width - 5, subTabHeight, apStringFactory.I.ModifiersOfMeshGroup))//"Modifiers of Mesh Group"
			{
				if (!isEditMeshGroupMode_Modifier)
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						SelectBone(null, MULTI_SELECT.Main);
						SetBoneEditing(false, false);//Bone 처리는 종료 

						Editor._meshGroupEditMode = apEditor.MESHGROUP_EDIT_MODE.Modifier;

						bool isSelectMod = false;
						if (Modifier == null)
						{
							//이전에 선택했던 Modifier가 없다면..
							if (_meshGroup._modifierStack != null)
							{
								if (_meshGroup._modifierStack._modifiers.Count > 0)
								{
									//맨 위의 Modifier를 자동으로 선택해주자
									int nMod = _meshGroup._modifierStack._modifiers.Count;
									apModifierBase lastMod = _meshGroup._modifierStack._modifiers[nMod - 1];
									SelectModifier(lastMod);
									isSelectMod = true;
								}
							}
						}
						else
						{
							SelectModifier(Modifier);

							isSelectMod = true;
						}

						if (!isSelectMod)
						{
							SelectModifier(null);
						}



						//스크롤 초기화 (오른쪽2)
						Editor.ResetScrollPosition(false, false, false, true, false);
					}

				}
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			if (Editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Setting)
			{
				_isMeshGroupSetting_EditDefaultTransform = false;
			}

			switch (Editor._meshGroupEditMode)
			{
				case apEditor.MESHGROUP_EDIT_MODE.Setting:
					DrawMeshGroupProperty_Setting(width, height);
					break;

				case apEditor.MESHGROUP_EDIT_MODE.Bone:
					DrawMeshGroupProperty_Bone(width, height);
					break;

				case apEditor.MESHGROUP_EDIT_MODE.Modifier:
					DrawMeshGroupProperty_Modifier(width, height);
					break;
			}
		}




		// 메시 그룹 UI의 서브 탭 UI 그리기


		/// <summary>
		/// 메시 그룹 탭 중 "Setting" UI 그리기
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawMeshGroupProperty_Setting(int width, int height)
		{	
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"

			apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__MeshGroupName); //추가 20.12.4 : 단축키로 포커싱하기 위함

			string nextMeshGroupName = EditorGUILayout.DelayedTextField(_meshGroup._name, apGUILOFactory.I.Width(width));
			if (!string.Equals(nextMeshGroupName, _meshGroup._name))
			{
				if (apEditorUtil.IsDelayedTextFieldEventValid(apStringFactory.I.GUI_ID__MeshGroupName))//텍스트 변경이 유효한지도 체크한다.
				{
					
					
					//이전
					//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, null, false, false);
					//_meshGroup._name = nextMeshGroupName;

					//변경 21.3.9
					//메시 그룹의 이름을 바꾼다.
					//단, 이 메시 그룹이 다른 메시 그룹의 MeshGroupTF라면, 이름이 같이 바뀌어야 한다.
					//이름 동기화 여부에 따라 Undo가 다르다.
					Editor.Controller.RenameMeshGroup(_meshGroup, nextMeshGroupName);
				}
				apEditorUtil.ReleaseGUIFocus();
				Editor.RefreshControllerAndHierarchy(false);
			}

			if(SubObjects.NumMeshTF == 0 && SubObjects.NumMeshGroupTF == 0)
			{
				//단축키 추가 (20.12.4) : 선택된게 없을땐, 메시 그룹의 이름을 바꾼다.
				Editor.AddHotKeyEvent(OnHotKeyEvent_RenameMeshGroup, apHotKeyMapping.KEY_TYPE.RenameObject, _meshGroup);
			}	
			
			GUILayout.Space(10);

			//" Editing.." / " Edit Default Transform"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_MeshGroupDefaultTransform),
				1, Editor.GetUIWord(UIWORD.EditingDefaultTransform),
				Editor.GetUIWord(UIWORD.EditDefaultTransform),
				_isMeshGroupSetting_EditDefaultTransform, true, width, 30, apStringFactory.I.EditDefaultTransformsOfSubMeshesMeshGroups))//"Edit Default Transforms of Sub Meshs/MeshGroups"
			{
				_isMeshGroupSetting_EditDefaultTransform = !_isMeshGroupSetting_EditDefaultTransform;
				

				//변경 22.5.14
				AutoRefreshModifierExclusiveEditing();

				//20.7.5 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
				Editor.Gizmos.OnSelectedObjectsChanged();
			}

			GUILayout.Space(5);


			//MainMesh에 포함되는가
			int nRootUnits = _portrait._mainMeshGroupList != null ? _portrait._mainMeshGroupList.Count : 0;
			bool isMainMeshGroup = nRootUnits > 0 ? _portrait._mainMeshGroupList.Contains(MeshGroup) : false;
			
			Color prevColor = GUI.backgroundColor;

			if (isMainMeshGroup)
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.normal.textColor = apEditorUtil.BoxTextColor;

				GUI.backgroundColor = new Color(0.5f, 0.7f, 0.9f, 1.0f);

				//"Root Unit"
				GUILayout.Box(Editor.GetUIWord(UIWORD.RootUnit), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				//" Set Root Unit"
				//이전
				//if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.SetRootUnit), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), "Make Mesh Group as Root Unit"), GUILayout.Width(width), GUILayout.Height(30)))

				//변경
				if (_guiContent_MeshGroupProperty_SetRootUnit == null)
				{
					_guiContent_MeshGroupProperty_SetRootUnit = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.SetRootUnit), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Root), apStringFactory.I.MakeMeshGroupAsRootUnit);//"Make Mesh Group as Root Unit"
				}

				//추가 22.6.10 : 루트 유닛이 하나도 없다면 버튼이 반짝거린다.
				if(nRootUnits == 0)
				{
					GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();
				}

				if (GUILayout.Button(_guiContent_MeshGroupProperty_SetRootUnit.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxMargin, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
				{
					apEditorUtil.SetRecord_PortraitMeshGroup(	apUndoGroupData.ACTION.Portrait_SetMeshGroup, 
																Editor, 
																_portrait, 
																MeshGroup, 
																//null, 
																false, 
																true,
																apEditorUtil.UNDO_STRUCT.StructChanged);

					_portrait._mainMeshGroupIDList.Add(MeshGroup._uniqueID);
					_portrait._mainMeshGroupList.Add(MeshGroup);

					apRootUnit newRootUnit = new apRootUnit();
					newRootUnit.SetPortrait(_portrait);
					newRootUnit.SetMeshGroup(MeshGroup);

					_portrait._rootUnits.Add(newRootUnit);

					Editor.RefreshControllerAndHierarchy(false);

					//Root Hierarchy Filter를 활성화한다.
					Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.RootUnit, true);
				}

				GUI.backgroundColor = prevColor;
			}

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			//와! 새해 첫 코드!
			//추가 20.1.5 : 메시 복제하기
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.Duplicate), apGUILOFactory.I.Width(width)))//"Duplicate"
			{
				//TODO : 애니메이션도 복사할지 물어봐야함
				Editor.Controller.DuplicateMeshGroup(
					MeshGroup, null, true, true);
			}

			//삭제하기
			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			//"  Remove Mesh Group"
			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveMeshGroup),
			//										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
			//										),
			//						GUILayout.Height(24)))

			//변경
			if (_guiContent_MeshGroupProperty_RemoveMeshGroup == null)
			{
				_guiContent_MeshGroupProperty_RemoveMeshGroup = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveMeshGroup), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}


			if (GUILayout.Button(_guiContent_MeshGroupProperty_RemoveMeshGroup.Content, apGUILOFactory.I.Height(24)))
			{

				string strRemoveDialogInfo = Editor.Controller.GetRemoveItemMessage(
																_portrait,
																_meshGroup,
																5,
																Editor.GetTextFormat(TEXT.RemoveMeshGroup_Body, _meshGroup._name),
																Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																);

				//bool isResult = EditorUtility.DisplayDialog("Remove Mesh Group", "Do you want to remove [" + _meshGroup._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveMeshGroup_Title),
																//Editor.GetTextFormat(TEXT.RemoveMeshGroup_Body, _meshGroup._name),
																strRemoveDialogInfo,
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);
				if (isResult)
				{
					Editor.Controller.RemoveMeshGroup(_meshGroup);

					SelectNone();
				}
			}


		}



		/// <summary>
		/// 메시 그룹 탭 중 "Bone" UI 그리기
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawMeshGroupProperty_Bone(int width, int height)
		{
			//GUILayout.Space(10);

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Editable, _isBoneDefaultEditing);//"BoneEditMode - Editable"

			//" Editing Bones", " Start Editing Bones"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_EditMode),
												1,
												Editor.GetUIWord(UIWORD.EditingBones), Editor.GetUIWord(UIWORD.StartEditingBones),
												IsBoneDefaultEditing, true, width, 30, apStringFactory.I.EditBones))//"Edit Bones"
			{
				//Bone을 수정할 수 있다.
				SetBoneEditing(!_isBoneDefaultEditing, true);

				//추가 22.5.14 : 버그 : 본 편집시에도 모디파이어가 동작하는 버그
				AutoRefreshModifierExclusiveEditing();
			}

			GUILayout.Space(5);

			//Add 툴과 Select 툴 On/Off

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Select, _boneEditMode == BONE_EDIT_MODE.SelectAndTRS);  //"BoneEditMode - Select"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Add, _boneEditMode == BONE_EDIT_MODE.Add);          //"BoneEditMode - Add"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Link, _boneEditMode == BONE_EDIT_MODE.Link);            //"BoneEditMode - Link"

			bool isBoneEditable = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Editable);//"BoneEditMode - Editable"
			bool isBoneEditMode_Select = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Select);//"BoneEditMode - Select"
			bool isBoneEditMode_Add = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Add);//"BoneEditMode - Add"
			bool isBoneEditMode_Link = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.BoneEditMode__Link);//"BoneEditMode - Link"

			int subTabWidth = (width / 3) - 4;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));



			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Select),
											isBoneEditMode_Select, _isBoneDefaultEditing,
											subTabWidth, 40, Editor.GetUIWord(UIWORD.SelectBones)))//"Select Bones"
			{
				SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Add),
											isBoneEditMode_Add, _isBoneDefaultEditing,
											subTabWidth, 40, Editor.GetUIWord(UIWORD.AddBones)))//"Add Bones"
			{
				SetBoneEditMode(BONE_EDIT_MODE.Add, true);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Link),
											isBoneEditMode_Link, _isBoneDefaultEditing,
											subTabWidth, 40, Editor.GetUIWord(UIWORD.LinkBones)))//"Link Bones"
			{
				SetBoneEditMode(BONE_EDIT_MODE.Link, true);
			}

			EditorGUILayout.EndHorizontal();


			GUILayout.Space(5);

			if (isBoneEditable)
			{
				string strBoneEditInfo = null;
				Color prevColor = GUI.backgroundColor;
				Color colorBoneEdit = Color.black;
				switch (_boneEditMode)
				{
					case BONE_EDIT_MODE.None:
						strBoneEditInfo = apStringFactory.I.NotEditable;
						colorBoneEdit = new Color(0.6f, 0.6f, 0.6f, 1.0f);
						break;

					case BONE_EDIT_MODE.SelectOnly:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.SelectBones);//"Select Bones"
						colorBoneEdit = new Color(0.6f, 0.9f, 0.9f, 1.0f);
						break;

					case BONE_EDIT_MODE.SelectAndTRS:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.SelectBones);//"Select Bones"
						colorBoneEdit = new Color(0.5f, 0.9f, 0.6f, 1.0f);
						break;

					case BONE_EDIT_MODE.Add:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.AddBones);//"Add Bones"
						colorBoneEdit = new Color(0.95f, 0.65f, 0.65f, 1.0f);
						break;

					case BONE_EDIT_MODE.Link:
						strBoneEditInfo = Editor.GetUIWord(UIWORD.LinkBones);//"Link Bones"
						colorBoneEdit = new Color(0.57f, 0.82f, 0.95f, 1.0f);
						break;
				}

				//GUIStyle guiStyle_Info = new GUIStyle(GUI.skin.box);
				//guiStyle_Info.alignment = TextAnchor.MiddleCenter;
				//guiStyle_Info.normal.textColor = apEditorUtil.BoxTextColor;

				GUI.backgroundColor = colorBoneEdit;
				GUILayout.Box((strBoneEditInfo != null ? strBoneEditInfo : apStringFactory.I.None), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 8), apGUILOFactory.I.Height(34));

				GUI.backgroundColor = prevColor;

				//GUILayout.Space(5);


			}

			GUILayout.Space(5);

			//" Export/Import Bones"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad),
												1, Editor.GetUIWord(UIWORD.ExportImportBones),
												Editor.GetUIWord(UIWORD.ExportImportBones),
												false, true, width, 26))
			{
				//Bone을 파일로 저장하거나 열수 있는 다이얼로그를 호출한다.
				_loadKey_OnBoneStructureLoaded = apDialog_RetargetBase.ShowDialog(Editor, _meshGroup, OnBoneStruceLoaded);
			}

			GUILayout.Space(20);

			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			//"Remove All Bones"
			//이전
			//if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.RemoveAllBones), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)), GUILayout.Width(width), GUILayout.Height(24)))

			//변경
			if (_guiContent_MeshGroupProperty_RemoveAllBones == null)
			{
				_guiContent_MeshGroupProperty_RemoveAllBones = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.RemoveAllBones), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}

			if (GUILayout.Button(_guiContent_MeshGroupProperty_RemoveAllBones.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Bones", "Remove All Bones?", "Remove", "Cancel");
				//이건 관련 메시지가 없다.
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveBonesAll_Title),
																Editor.GetText(TEXT.RemoveBonesAll_Body),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);
				if (isResult)
				{
					Editor.Controller.RemoveAllBones(MeshGroup);
				}
			}
		}




		
		/// <summary>
		/// 메시 그룹의 탭 중 "Modifier" 탭 UI 그리기
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawMeshGroupProperty_Modifier(int width, int height)
		{
			//EditorGUILayout.LabelField("Presets");
			//GUILayout.Space(10);

			//"  Add Modifier"
			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddModifier), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddNewMod), "Add a New Modifier"), GUILayout.Height(30)))

			//변경
			if (_guiContent_MeshGroupProperty_AddModifier == null)
			{
				_guiContent_MeshGroupProperty_AddModifier = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AddModifier), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddNewMod), apStringFactory.I.AddANewModifier);//"Add a New Modifier"
			}

			if (GUILayout.Button(_guiContent_MeshGroupProperty_AddModifier.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
			{
				//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					_loadKey_AddModifier = apDialog_AddModifier.ShowDialog(Editor, MeshGroup, OnAddModifier);
				}
			}

			GUILayout.Space(5);
			//EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
			GUILayout.Space(2);

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ModifierStack), apGUILOFactory.I.Height(25));//"Modifier Stack"

			//GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			GUILayout.Button(apStringFactory.I.None, apGUIStyleWrapper.I.None, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20));//<레이아웃 정렬을 위한의미없는 숨은 버튼
			EditorGUILayout.EndHorizontal();
			apModifierStack modStack = MeshGroup._modifierStack;

			if (_guiContent_MeshGroupProperty_ModifierLayerUnit == null)
			{
				_guiContent_MeshGroupProperty_ModifierLayerUnit = new apGUIContentWrapper();
			}


			//등록된 Modifier 리스트를 출력하자
			int nModifiers = 0;
			if(modStack != null)
			{
				nModifiers = modStack._modifiers != null ? modStack._modifiers.Count : 0;
			}

			if (nModifiers > 0)
			{
				//역순으로 출력한다.
				for (int i = nModifiers - 1; i >= 0; i--)
				{
					DrawModifierLayerUnit(modStack._modifiers[i], width, 25);
				}
			}


		}





		/// <summary>
		/// 메시 그룹의 탭에서 "모디파이어 리스트"에서의 항목 그리기
		/// </summary>
		/// <param name="modifier"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		private int DrawModifierLayerUnit(apModifierBase modifier, int width, int height)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();

			GUIStyle curGUIStyle = null;//<<최적화된 코드
										//Color texColor = GUI.skin.label.normal.textColor;

			if (Modifier == modifier)
			{
				Color prevColor = GUI.backgroundColor;

				#region [미사용 코드]
				//if (EditorGUIUtility.isProSkin)
				//{
				//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				//	//texColor = Color.cyan;
				//}
				//else
				//{
				//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
				//	//texColor = Color.white;
				//}

				//GUI.Box(new Rect(lastRect.x, lastRect.y + height, width + 15, height), "");
				//GUI.backgroundColor = prevColor; 
				#endregion

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + height, width + 15 - 2, height, apEditorUtil.UNIT_BG_STYLE.Main);

				curGUIStyle = apGUIStyleWrapper.I.None_White2Cyan;
			}
			else
			{
				curGUIStyle = apGUIStyleWrapper.I.None_LabelColor;
			}

			//GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
			//guiStyle_None.normal.textColor = texColor;

			apImageSet.PRESET iconType = apEditorUtil.GetModifierIconType(modifier.ModifierType);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			GUILayout.Space(10);

			//이전
			//if (GUILayout.Button(new GUIContent(" " + modifier.DisplayName, Editor.ImageSet.Get(iconType)), guiStyle_None, GUILayout.Width(width - 40), GUILayout.Height(height)))

			//변경
			_guiContent_MeshGroupProperty_ModifierLayerUnit.SetText(1, modifier.DisplayName);
			_guiContent_MeshGroupProperty_ModifierLayerUnit.SetImage(Editor.ImageSet.Get(iconType));

			if (GUILayout.Button(_guiContent_MeshGroupProperty_ModifierLayerUnit.Content, curGUIStyle, apGUILOFactory.I.Width(width - 40), apGUILOFactory.I.Height(height)))
			{
				if (Modifier != modifier)
				{
					//모디파이어가 다른 경우에만 선택할 수 있다.

					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						SelectModifier(modifier);
					}
				}
			}

			int iResult = 0;

			Texture2D activeBtn = null;
			bool isActiveMod = false;
			//if (modifier._isActive && modifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled)//이전
			if (modifier._isActive 
				&& modifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit
				&& modifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force
				&& modifier._editorExclusiveActiveMod != apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor//변경 21.2.15
				)
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Active);
				isActiveMod = true;
			}
			else
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Deactive);
				isActiveMod = false;
			}
			if (GUILayout.Button(activeBtn, curGUIStyle, apGUILOFactory.I.Width(height), apGUILOFactory.I.Height(height)))
			{
				//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					//토글한다.
					modifier._isActive = !isActiveMod;

					if (ExEditingMode != EX_EDIT.None)
					{
						//if (modifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled)//이전
						if (modifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_ExceptColor
							|| modifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_Force
							|| modifier._editorExclusiveActiveMod == apModifierBase.MOD_EDITOR_ACTIVE.Disabled_NotEdit)//변경 21.2.15
						{
							//작업이 허용된 Modifier가 아닌데 Active를 제어했다면
							//ExEdit를 해제해야한다.
							SetModifierExclusiveEditing(EX_EDIT.None);
						}
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			return iResult;
		}





		//-------------------------------------------------------------------------
		// 애니메이션 UI
		//-------------------------------------------------------------------------
		

		private void Draw_Animation(int width, int height)
		{
			//GUILayout.Box("Animation", GUILayout.Width(width), GUILayout.Height(30));
			//DrawTitle("Animation", width);
			//EditorGUILayout.Space();

			if (_animClip == null)
			{
				SelectNone();
				return;
			}

			//왼쪽엔 기본 세팅/ 우측 (Right2)엔 편집 도구들 + 생성된 Timeline리스트
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name));//"Name"

			//추가 20.12.4 : 단축키로 이름 UI에 포커스를 주기 위해
			apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__AnimClipName);

			string nextAnimClipName = EditorGUILayout.DelayedTextField(_animClip._name, apGUILOFactory.I.Width(width));

			if (!string.Equals(nextAnimClipName, _animClip._name))
			{
				if (apEditorUtil.IsDelayedTextFieldEventValid(apStringFactory.I.GUI_ID__AnimClipName))//텍스트 변경이 유효한지도 체크한다.
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_SettingChanged, 
														Editor, 
														Editor._portrait, 
														//_animClip, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					_animClip._name = nextAnimClipName;
				}
				apEditorUtil.ReleaseGUIFocus();
				Editor.RefreshControllerAndHierarchy(false);
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Info, null, null);
			}


			//단축키 추가 (20.12.4)
			Editor.AddHotKeyEvent(OnHotKey_RenameAnimClip, apHotKeyMapping.KEY_TYPE.RenameObject, AnimClip);


			GUILayout.Space(5);
			//MeshGroup에 연동해야한다.


			Color prevColor = GUI.backgroundColor;

			bool isValidMeshGroup = false;
			if (_animClip._targetMeshGroup != null && _animClip._targetMeshGroup._parentMeshGroup == null)
			{
				//유효한 메시 그룹은
				//- null이 아니고
				//- Root 여야 한다.
				isValidMeshGroup = true;
			}

			//추가 19.11.20
			if (_guiContent_Animation_SelectMeshGroupBtn == null)
			{
				_guiContent_Animation_SelectMeshGroupBtn = new apGUIContentWrapper();
				_guiContent_Animation_SelectMeshGroupBtn.ClearText(false);
				_guiContent_Animation_SelectMeshGroupBtn.AppendSpaceText(1, false);
				_guiContent_Animation_SelectMeshGroupBtn.AppendText(Editor.GetUIWord(UIWORD.SelectMeshGroup), true);
				_guiContent_Animation_SelectMeshGroupBtn.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup));
			}



			if (_animClip._targetMeshGroup == null)
			{
				//GUI.color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//GUILayout.Box("Linked Mesh Group\n[ None ]", guiStyle_Box, GUILayout.Width(width), GUILayout.Height(40));
				//GUI.color = prevColor;

				//GUILayout.Space(2);

				//" Select MeshGroup"

				//추가 22.6.10 : 메시 그룹을 선택하는 것은 중요하므로 버튼이 반짝거린다.
				
				GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();

				if (GUILayout.Button(_guiContent_Animation_SelectMeshGroupBtn.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35)))
				{
					_loadKey_SelectMeshGroupToAnimClip = apDialog_SelectLinkedMeshGroup.ShowDialog(Editor, _animClip, OnSelectMeshGroupToAnimClip);
				}

				GUI.backgroundColor = prevColor;
			}
			else
			{
				//Label 삭제 21.3.14
				//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TargetMeshGroup));//"Target Mesh Group"


				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

				if (isValidMeshGroup)
				{
					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				}
				else
				{
					//유효하지 않다면 붉은 색
					GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);
				}

				if (_strWrapper_64 == null)
				{
					_strWrapper_64 = new apStringWrapper(64);
				}
				if (_animClip._targetMeshGroup._name.Length > 16)
				{
					_strWrapper_64.Clear();
					_strWrapper_64.Append(_animClip._targetMeshGroup._name.Substring(0, 14), false);
					_strWrapper_64.Append(apStringFactory.I.Dot2, true);
				}
				else
				{
					_strWrapper_64.Clear();
					_strWrapper_64.Append(_animClip._targetMeshGroup._name, true);
				}

				//string strMeshGroupName = _animClip._targetMeshGroup._name;
				//if (strMeshGroupName.Length > 16)
				//{
				//	//이름이 너무 기네용.
				//	strMeshGroupName = strMeshGroupName.Substring(0, 14) + "..";
				//}

				GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - (80 + 2)), apGUILOFactory.I.Height(18));
				GUI.backgroundColor = prevColor;


				//애니메이션의 메시 그룹 바꾸기
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), apGUILOFactory.I.Width(80)))//"Change"
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if(isExecutable)
					{
						_loadKey_SelectMeshGroupToAnimClip = apDialog_SelectLinkedMeshGroup.ShowDialog(Editor, _animClip, OnSelectMeshGroupToAnimClip);
					}
					
				}
				EditorGUILayout.EndHorizontal();

				//추가 19.8.23 : 유효하지 않다면 Box로 만들자.
				if (!isValidMeshGroup)
				{
					GUILayout.Space(2);

					GUI.backgroundColor = new Color(1.0f, 0.7f, 0.7f, 1.0f);

					GUILayout.Box(Editor.GetUIWord(UIWORD.AnimLinkedToInvalidMeshGroup), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));

					GUI.backgroundColor = prevColor;
				}

				//삭제 21.3.14 : Duplicate 버튼은 Right2 UI로 옮긴다.
				//GUILayout.Space(5);
				//if (GUILayout.Button(Editor.GetUIWord(UIWORD.Duplicate), apGUILOFactory.I.Width(width)))//"Duplicate"
				//{
				//	Editor.Controller.DuplicateAnimClip(_animClip);
				//	Editor.RefreshControllerAndHierarchy(true);
				//}
				//GUILayout.Space(5);

				//Timeline을 추가하자
				//Timeline은 ControlParam, Modifier, Bone에 연동된다.
				//TimelineLayer은 각 Timeline에서 어느 Transform(Mesh/MeshGroup), Bone, ControlParam 에 적용 될지를 결정한다.

				//" Add Timeline"
				if (_guiContent_Animation_AddTimeline == null)
				{
					_guiContent_Animation_AddTimeline = new apGUIContentWrapper();
					_guiContent_Animation_AddTimeline.ClearText(false);
					_guiContent_Animation_AddTimeline.AppendSpaceText(1, false);
					_guiContent_Animation_AddTimeline.AppendText(Editor.GetUIWord(UIWORD.AddTimeline), true);
					_guiContent_Animation_AddTimeline.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddTimeline));
				}


				//만약 타임라인이 하나도 없다면 이 버튼이 반짝거린다. (22.6.10)
				
				
				if(_animClip != null)
				{
					int nTimelines = _animClip._timelines != null ? _animClip._timelines.Count : 0;
					if(nTimelines == 0)
					{
						GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();
					}
				}

				if (GUILayout.Button(_guiContent_Animation_AddTimeline.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();
					
					if(isExecutable)
					{
						_loadKey_AddTimelineToAnimClip = apDialog_AddAnimTimeline.ShowDialog(Editor, _animClip, OnAddTimelineToAnimClip);
					}
				}

				GUI.backgroundColor = prevColor;



				//등록된 Timeline 리스트를 보여주자
				GUILayout.Space(10);
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
				GUILayout.Space(2);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Timelines), apGUILOFactory.I.Height(25));//"Timelines"

				//GUIStyle guiStyle_None = new GUIStyle(GUIStyle.none);
				GUILayout.Button(apStringFactory.I.None, apGUIStyleWrapper.I.None, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20));//<레이아웃 정렬을 위한의미없는 숨은 버튼
				EditorGUILayout.EndHorizontal();


				if (_guiContent_Animation_TimelineUnit_AnimMod == null)
				{
					_guiContent_Animation_TimelineUnit_AnimMod = new apGUIContentWrapper();
					_guiContent_Animation_TimelineUnit_AnimMod.ClearText(true);
					_guiContent_Animation_TimelineUnit_AnimMod.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithMod));
				}

				if (_guiContent_Animation_TimelineUnit_ControlParam == null)
				{
					_guiContent_Animation_TimelineUnit_ControlParam = new apGUIContentWrapper();
					_guiContent_Animation_TimelineUnit_ControlParam.ClearText(true);
					_guiContent_Animation_TimelineUnit_ControlParam.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithControlParam));
				}


				//등록된 Modifier 리스트를 출력하자
				int nAnimTimelines = _animClip._timelines != null ?_animClip._timelines.Count : 0;
				if (nAnimTimelines > 0)
				{
					for (int i = 0; i < nAnimTimelines; i++)
					{
						DrawTimelineUnit(_animClip._timelines[i], width, 25);
					}
				}
			}

			GUILayout.Space(10);

			apEditorUtil.GUI_DelimeterBoxH(width);

			//등등
			GUILayout.Space(10);

			//"  Remove Animation"
			if (_guiContent_Animation_RemoveAnimation == null)
			{
				_guiContent_Animation_RemoveAnimation = new apGUIContentWrapper();
				_guiContent_Animation_RemoveAnimation.ClearText(false);
				_guiContent_Animation_RemoveAnimation.AppendSpaceText(2, false);
				_guiContent_Animation_RemoveAnimation.AppendText(Editor.GetUIWord(UIWORD.RemoveAnimation), true);
				_guiContent_Animation_RemoveAnimation.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}


			if (GUILayout.Button(_guiContent_Animation_RemoveAnimation.Content, apGUILOFactory.I.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Animation", "Do you want to remove [" + _animClip._name + "]?", "Remove", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveAnimClip_Title),
																Editor.GetTextFormat(TEXT.RemoveAnimClip_Body, _animClip._name),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));
				if (isResult)
				{
					Editor.Controller.RemoveAnimClip(_animClip);

					SelectNone();
					Editor.RefreshControllerAndHierarchy(true);
					//Editor.RefreshTimelineLayers(true);
				}
			}
		}


		


		/// <summary>
		/// 리스트에 타임라인 그리기
		/// </summary>
		/// <param name="timeline"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawTimelineUnit(apAnimTimeline timeline, int width, int height)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			//Color textColor = GUI.skin.label.normal.textColor;
			GUIStyle curGUIStyle = null;//<<최적화된 코드
			if (AnimTimeline == timeline)
			{
				#region [미사용 코드]
				//Color prevColor = GUI.backgroundColor;

				//if (EditorGUIUtility.isProSkin)
				//{
				//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				//	//textColor = Color.cyan;
				//}
				//else
				//{
				//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
				//	//textColor = Color.white;
				//}

				//GUI.Box(new Rect(lastRect.x, lastRect.y + height, width + 15, height), apStringFactory.I.None);
				//GUI.backgroundColor = prevColor; 
				#endregion

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + height, width + 15 - 2, height, apEditorUtil.UNIT_BG_STYLE.Main);

				curGUIStyle = apGUIStyleWrapper.I.None_White2Cyan;
			}
			else
			{
				curGUIStyle = apGUIStyleWrapper.I.None_LabelColor;
			}

			

			//변경
			apGUIContentWrapper curGUIContentWrapper = timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier ? _guiContent_Animation_TimelineUnit_AnimMod : _guiContent_Animation_TimelineUnit_ControlParam;

			curGUIContentWrapper.ClearText(false);
			curGUIContentWrapper.AppendSpaceText(1, false);
			curGUIContentWrapper.AppendText(timeline.DisplayName, true);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			GUILayout.Space(10);

			//이전
			//if (GUILayout.Button(new GUIContent(" " + timeline.DisplayName, Editor.ImageSet.Get(iconType)), guiStyle_None, GUILayout.Width(width - 40), GUILayout.Height(height)))
			if (GUILayout.Button(curGUIContentWrapper.Content, curGUIStyle, apGUILOFactory.I.Width(width - 40), apGUILOFactory.I.Height(height)))
			{
				//선택중인 타임라인은 생략한다.
				if (AnimTimeline != timeline)
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						SelectAnimTimeline(timeline, true);
						SelectAnimTimelineLayer(null, MULTI_SELECT.Main, true);
						SelectAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
					}
				}
			}

			Texture2D activeBtn = null;
			bool isActiveMod = false;
			if (timeline._isActiveInEditing)
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Active);
				isActiveMod = true;
			}
			else
			{
				activeBtn = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Deactive);
				isActiveMod = false;
			}
			if (GUILayout.Button(activeBtn, curGUIStyle, apGUILOFactory.I.Width(height), apGUILOFactory.I.Height(height)))
			{
				//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					//타임라인을 토글한다.
					timeline._isActiveInEditing = !isActiveMod;
				}
				
			}
			EditorGUILayout.EndHorizontal();
		}








		//--------------------------------------------------------------------
		// 컨트롤 파라미터 UI
		//--------------------------------------------------------------------
		private void Draw_Param(int width, int height)
		{
			//EditorGUILayout.Space();

			apControlParam cParam = _param;
			if (cParam == null)
			{
				SelectNone();
				return;
			}
			if (_prevParam != cParam)
			{
				_prevParam = cParam;
				//_prevParamName = cParam._keyName;
			}
			if (cParam._isReserved)
			{
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ReservedParameter), apGUIStyleWrapper.I.Label_RedColor);//"Reserved Parameter"
				GUILayout.Space(10);
			}

			apControlParam.CATEGORY next_category = cParam._category;
			apControlParam.ICON_PRESET next_iconPreset = cParam._iconPreset;
			apControlParam.TYPE next_valueType = cParam._valueType;

			string next_label_Min = cParam._label_Min;
			string next_label_Max = cParam._label_Max;
			int next_snapSize = cParam._snapSize;

			int next_int_Def = cParam._int_Def;
			float next_float_Def = cParam._float_Def;
			Vector2 next_vec2_Def = cParam._vec2_Def;
			int next_int_Min = cParam._int_Min;
			int next_int_Max = cParam._int_Max;
			float next_float_Min = cParam._float_Min;
			float next_float_Max = cParam._float_Max;
			Vector2 next_vec2_Min = cParam._vec2_Min;
			Vector2 next_vec2_Max = cParam._vec2_Max;


			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.NameUnique));//"Name (Unique)"

			if (cParam._isReserved)
			{
				EditorGUILayout.LabelField(cParam._keyName, GUI.skin.textField, apGUILOFactory.I.Width(width));
			}
			else
			{
				//추가 20.12.4 : 단축키로 이름 UI에 포커스를 주기 위해
				apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__ControlParamName);

				string nextKeyName = EditorGUILayout.DelayedTextField(cParam._keyName, apGUILOFactory.I.Width(width));


				if (!string.Equals(nextKeyName, cParam._keyName))
				{
					if (apEditorUtil.IsDelayedTextFieldEventValid(apStringFactory.I.GUI_ID__ControlParamName))//텍스트 변경이 유효한지도 체크한다.
					{
						if (string.IsNullOrEmpty(nextKeyName))
						{
							//이름이 빈칸이다
							//EditorUtility.DisplayDialog("Error", "Empty Name is not allowed", "Okay");

							EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamNameError_Title),
														Editor.GetText(TEXT.ControlParamNameError_Body_Wrong),
														Editor.GetText(TEXT.Close));
						}
						else if (Editor.ParamControl.FindParam(nextKeyName) != null)
						{
							//이미 사용중인 이름이다.
							//EditorUtility.DisplayDialog("Error", "It is used Name", "Okay");
							EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamNameError_Title),
													Editor.GetText(TEXT.ControlParamNameError_Body_Used),
													Editor.GetText(TEXT.Close));
						}
						else
						{


							Editor.Controller.ChangeParamName(cParam, nextKeyName);
							cParam._keyName = nextKeyName;
						}
					}
					apEditorUtil.ReleaseGUIFocus();
				}
			}


			
			
			//단축키 추가 (20.12.4)
			Editor.AddHotKeyEvent(OnHotKeyEvent_RenameControlParam, apHotKeyMapping.KEY_TYPE.RenameObject, Param);
			



			//EditorGUILayout.Space();
			GUILayout.Space(10);

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ValueType));//"Type"
			if (cParam._isReserved)
			{
				EditorGUILayout.EnumPopup(cParam._valueType);
			}
			else
			{
				next_valueType = (apControlParam.TYPE)EditorGUILayout.EnumPopup(cParam._valueType);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Category));//"Category"
			if (cParam._isReserved)
			{
				EditorGUILayout.EnumPopup(cParam._category);
			}
			else
			{
				next_category = (apControlParam.CATEGORY)EditorGUILayout.EnumPopup(cParam._category);
			}
			GUILayout.Space(10);

			int iconSize = 32;
			int iconPresetHeight = 32;
			int presetCategoryWidth = width - (iconSize + 8 + 5);
			Texture2D imgIcon = Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(cParam._iconPreset));

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconPresetHeight));
			GUILayout.Space(2);

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(presetCategoryWidth), apGUILOFactory.I.Height(iconPresetHeight));

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IconPreset), apGUILOFactory.I.Width(presetCategoryWidth));//"Icon Preset"
			next_iconPreset = (apControlParam.ICON_PRESET)EditorGUILayout.EnumPopup(cParam._iconPreset, apGUILOFactory.I.Width(presetCategoryWidth));

			EditorGUILayout.EndVertical();
			GUILayout.Space(2);

			//이전
			//EditorGUILayout.LabelField(new GUIContent(imgIcon), GUILayout.Width(iconSize), GUILayout.Height(iconPresetHeight));

			//변경
			if (_guiContent_Param_IconPreset == null)
			{
				_guiContent_Param_IconPreset = apGUIContentWrapper.Make(imgIcon);
			}
			else
			{
				_guiContent_Param_IconPreset.SetImage(imgIcon);
			}

			EditorGUILayout.LabelField(_guiContent_Param_IconPreset.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconPresetHeight));


			EditorGUILayout.EndHorizontal();


			//EditorGUILayout.Space();
			GUILayout.Space(10);


			string strRangeLabelName_Min = Editor.GetUIWord(UIWORD.Min);//"Min"
			string strRangeLabelName_Max = Editor.GetUIWord(UIWORD.Max);//"Max"
			switch (cParam._valueType)
			{
				case apControlParam.TYPE.Int:
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_IntegerType));//"Integer Type"
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_DefaultValue));//"Default Value"
					next_int_Def = EditorGUILayout.DelayedIntField(cParam._int_Def);
					break;

				case apControlParam.TYPE.Float:
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_FloatType));//"Float Number Type"
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_DefaultValue));//"Default Value"
					next_float_Def = EditorGUILayout.DelayedFloatField(cParam._float_Def);
					break;

				case apControlParam.TYPE.Vector2:
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_Vector2Type));//"Vector2 Type"
					EditorGUILayout.Space();

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Param_DefaultValue));//"Default Value"

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					next_vec2_Def.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Def.x, apGUILOFactory.I.Width((width / 2) - 2));
					next_vec2_Def.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Def.y, apGUILOFactory.I.Width((width / 2) - 2));
					EditorGUILayout.EndHorizontal();

					strRangeLabelName_Min = Editor.GetUIWord(UIWORD.Param_Axis1);//"Axis 1"
					strRangeLabelName_Max = Editor.GetUIWord(UIWORD.Param_Axis2);//"Axis 2"
					break;
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);


			GUILayoutOption opt_Label = apGUILOFactory.I.Width(50);
			GUILayoutOption opt_Data = apGUILOFactory.I.Width(width - (50 + 5));
			GUILayoutOption opt_SubData2 = apGUILOFactory.I.Width((width - (50 + 5)) / 2 - 2);

			
			//GUILayout.Space(20);
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.RangeValueLabel));//"Range Value Label" -> Name of value Range

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(strRangeLabelName_Min, opt_Label);
			next_label_Min = EditorGUILayout.DelayedTextField(cParam._label_Min, opt_Data);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(strRangeLabelName_Max, opt_Label);
			next_label_Max = EditorGUILayout.DelayedTextField(cParam._label_Max, opt_Data);
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(20);

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Range));//"Range Value" -> "Range"


			switch (cParam._valueType)
			{
				case apControlParam.TYPE.Int:
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), opt_Label);//"Min"
					next_int_Min = EditorGUILayout.DelayedIntField(cParam._int_Min, opt_Data);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), opt_Label);//"Max"
					next_int_Max = EditorGUILayout.DelayedIntField(cParam._int_Max, opt_Data);
					EditorGUILayout.EndHorizontal();
					break;

				case apControlParam.TYPE.Float:
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), opt_Label);//"Min"
					next_float_Min = EditorGUILayout.DelayedFloatField(cParam._float_Min, opt_Data);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), opt_Label);//"Max"
					next_float_Max = EditorGUILayout.DelayedFloatField(cParam._float_Max, opt_Data);
					EditorGUILayout.EndHorizontal();
					break;

				case apControlParam.TYPE.Vector2:
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(apStringFactory.I.None, opt_Label);
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), opt_SubData2);//"Min"
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), apGUIStyleWrapper.I.Label_MiddleRight, opt_SubData2);//"Max"
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(apStringFactory.I.X, opt_Label);
					next_vec2_Min.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Min.x, opt_SubData2);
					next_vec2_Max.x = EditorGUILayout.DelayedFloatField(cParam._vec2_Max.x, opt_SubData2);
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(apStringFactory.I.Y, opt_Label);
					next_vec2_Min.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Min.y, opt_SubData2);
					next_vec2_Max.y = EditorGUILayout.DelayedFloatField(cParam._vec2_Max.y, opt_SubData2);
					EditorGUILayout.EndHorizontal();
					break;

			}


			if (cParam._valueType == apControlParam.TYPE.Float ||
				cParam._valueType == apControlParam.TYPE.Vector2)
			{
				GUILayout.Space(15);

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SnapSize));//"Snap Size"
				next_snapSize = EditorGUILayout.DelayedIntField(cParam._snapSize, apGUILOFactory.I.Width(width));
				
			}



			if (next_category != cParam._category ||
				next_iconPreset != cParam._iconPreset ||
				next_valueType != cParam._valueType ||

				next_label_Min != cParam._label_Min ||
				next_label_Max != cParam._label_Max ||
				next_snapSize != cParam._snapSize ||

				next_int_Def != cParam._int_Def ||
				next_float_Def != cParam._float_Def ||
				next_vec2_Def.x != cParam._vec2_Def.x ||
				next_vec2_Def.y != cParam._vec2_Def.y ||

				next_int_Min != cParam._int_Min ||
				next_int_Max != cParam._int_Max ||

				next_float_Min != cParam._float_Min ||
				next_float_Max != cParam._float_Max ||

				next_vec2_Min.x != cParam._vec2_Min.x ||
				next_vec2_Min.y != cParam._vec2_Min.y ||
				next_vec2_Max.x != cParam._vec2_Max.x ||
				next_vec2_Max.y != cParam._vec2_Max.y
				)
			{
				apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.ControlParam_SettingChanged, 
													Editor, 
													Editor._portrait, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				if (next_snapSize < 1)
				{
					next_snapSize = 1;
				}

				if (cParam._iconPreset != next_iconPreset)
				{
					cParam._isIconChanged = true;
				}
				else if (cParam._category != next_category && !cParam._isIconChanged)
				{
					//아이콘을 한번도 바꾸지 않았더라면 자동으로 다음 아이콘을 추천해주자
					next_iconPreset = apEditorUtil.GetControlParamPresetIconTypeByCategory(next_category);
				}

				cParam._category = next_category;
				cParam._iconPreset = next_iconPreset;
				cParam._valueType = next_valueType;

				cParam._label_Min = next_label_Min;
				cParam._label_Max = next_label_Max;
				cParam._snapSize = next_snapSize;

				cParam._int_Def = next_int_Def;
				cParam._float_Def = next_float_Def;
				cParam._vec2_Def = next_vec2_Def;

				cParam._int_Min = next_int_Min;
				cParam._int_Max = next_int_Max;

				cParam._float_Min = next_float_Min;
				cParam._float_Max = next_float_Max;

				cParam._vec2_Min = next_vec2_Min;
				cParam._vec2_Max = next_vec2_Max;

				cParam.MakeInterpolationRange();
				GUI.FocusControl(null);
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//"Presets"
			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.Presets), Editor.ImageSet.Get(apImageSet.PRESET.ControlParam_Palette)), GUILayout.Height(30)))

			//변경
			if (_guiContent_Param_Presets == null)
			{
				_guiContent_Param_Presets = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.Presets), Editor.ImageSet.Get(apImageSet.PRESET.ControlParam_Palette));
			}
			if (GUILayout.Button(_guiContent_Param_Presets.Content, apGUILOFactory.I.Height(30)))
			{
				_loadKey_OnSelectControlParamPreset = apDialog_ControlParamPreset.ShowDialog(Editor, cParam, OnSelectControlParamPreset);
			}

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);





			if (!cParam._isReserved)
			{


				//"Remove Parameter"
				//이전
				//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveParameter),
				//								Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
				//								),
				//				GUILayout.Height(24)))

				//변경
				if (_guiContent_Param_RemoveParam == null)
				{
					_guiContent_Param_RemoveParam = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveParameter), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
				}

				if (GUILayout.Button(_guiContent_Param_RemoveParam.Content, apGUILOFactory.I.Height(24)))
				{
					string strRemoveParamText = Editor.Controller.GetRemoveItemMessage(_portrait,
														cParam,
														5,
														Editor.GetTextFormat(TEXT.RemoveControlParam_Body, cParam._keyName),
														Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
														);

					//bool isResult = EditorUtility.DisplayDialog("Warning", "If this param removed, some motion data may be not worked correctly", "Remove it!", "Cancel");
					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveControlParam_Title),
																	//Editor.GetTextFormat(TEXT.RemoveControlParam_Body, cParam._keyName),
																	strRemoveParamText,
																	Editor.GetText(TEXT.Remove),
																	Editor.GetText(TEXT.Cancel));
					if (isResult)
					{
						Editor.Controller.RemoveParam(cParam);
					}
				}
			}
		}


		//---------------------------------------------------------------------
		// 오른쪽 UI가 두줄일 때, 두번째 (바깥쪽 UI) 그리기
		//---------------------------------------------------------------------
		/// <summary>
		/// 오른쪽 두번째 UI 그리기 (메시 그룹과 애니메이션만 해당)
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void DrawEditor_Right2(int width, int height)
		{
			if (Editor == null || Editor.Select.Portrait == null)
			{
				return;
			}

			EditorGUILayout.Space();

			switch (_selectionType)
			{
				case SELECTION_TYPE.MeshGroup:
					{
						switch (Editor._meshGroupEditMode)
						{
							case apEditor.MESHGROUP_EDIT_MODE.Setting:
								DrawEditor_Right2_MeshGroup_Setting(width, height);
								break;

							case apEditor.MESHGROUP_EDIT_MODE.Bone:
								DrawEditor_Right2_MeshGroup_Bone(width, height);
								break;

							case apEditor.MESHGROUP_EDIT_MODE.Modifier:
								DrawEditor_Right2_MeshGroup_Modifier(width, height);
								break;
						}
					}
					break;

				case SELECTION_TYPE.Animation:
					{
						DrawEditor_Right2_Animation(width, height);
					}
					break;
			}
		}


		// 메시 그룹의 Right 2 UI 그리기
		//----------------------------------------------------------
		
		/// <summary>
		/// 메시 그룹의 Right 2 UI 중 "Setting" 탭에서의 UI
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawEditor_Right2_MeshGroup_Setting(int width, int height)
		{
			bool isMeshTransform = false;//<<단일 선택일 때
			bool isValidSelect = false;

			//추가 20.6.1 : 다중 선택
			bool isMultipleSelected = false;

			List<apTransform_Mesh> selectedMeshTFs = GetSubSeletedMeshTFs(false);
			List<apTransform_MeshGroup> selectedMeshGroupTFs = GetSubSeletedMeshGroupTFs(false);
			int nSubSelectedMeshTFs_All = selectedMeshTFs.Count;
			int nSubSelectedMeshGroupTFs_All = selectedMeshGroupTFs.Count;

			
			if (MeshTF_Main != null)
			{
				if (MeshTF_Main._mesh != null)
				{
					isMeshTransform = true;
					isValidSelect = true;

					//다중 선택 여부
					isMultipleSelected = (nSubSelectedMeshTFs_All + nSubSelectedMeshGroupTFs_All) > 1;
				}
			}
			else if (MeshGroupTF_Main != null)
			{
				if (MeshGroupTF_Main._meshGroup != null)
				{
					isMeshTransform = false;
					isValidSelect = true;

					//다중 선택 여부
					isMultipleSelected = (nSubSelectedMeshTFs_All + nSubSelectedMeshGroupTFs_All) > 1;
				}
			}

			//int uiType = 0;//이전
			//0 : NotSelected
			//1 : SingleMesh
			//2 : SingleMeshGroup
			//3 : MultiMesh (메인이 Mesh)
			//4 : MultiMeshGroup (메인이 MeshGroup)

			MESHGROUP_RIGHT_SETTING_PROPERTY_UI propUIType = MESHGROUP_RIGHT_SETTING_PROPERTY_UI.NoSelected;

			if(isValidSelect)
			{
				if (nSubSelectedMeshTFs_All > 0 && nSubSelectedMeshGroupTFs_All > 0)
				{
					//두 종류의 TF가 모두 선택된 경우
					propUIType = MESHGROUP_RIGHT_SETTING_PROPERTY_UI.Mixed;
				}
				else
				{
					//한종류만 선택된 경우
					if (isMeshTransform)
					{
						propUIType = (!isMultipleSelected ? MESHGROUP_RIGHT_SETTING_PROPERTY_UI.SingleMeshTF : MESHGROUP_RIGHT_SETTING_PROPERTY_UI.MultipleMeshTF);
					}
					else
					{
						propUIType = (!isMultipleSelected ? MESHGROUP_RIGHT_SETTING_PROPERTY_UI.SingleMeshGroupTF : MESHGROUP_RIGHT_SETTING_PROPERTY_UI.MultipleMeshGroupTF);
					}
				}
				
			}

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_SingleMeshTF,		propUIType == MESHGROUP_RIGHT_SETTING_PROPERTY_UI.SingleMeshTF);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_SingleMeshGroupTF,	propUIType == MESHGROUP_RIGHT_SETTING_PROPERTY_UI.SingleMeshGroupTF);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_MultiMeshTF,		propUIType == MESHGROUP_RIGHT_SETTING_PROPERTY_UI.MultipleMeshTF);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_MultiMeshGroupTF,	propUIType == MESHGROUP_RIGHT_SETTING_PROPERTY_UI.MultipleMeshGroupTF);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_Mixed,				propUIType == MESHGROUP_RIGHT_SETTING_PROPERTY_UI.Mixed);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectNotSelected,					!isValidSelect || propUIType == MESHGROUP_RIGHT_SETTING_PROPERTY_UI.NoSelected);

			bool isRender_SingleMeshTF =		Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_SingleMeshTF);
			bool isRender_SingleMeshGroupTF =	Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_SingleMeshGroupTF);
			bool isRender_MultiMeshTF =			Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_MultiMeshTF);
			bool isRender_MultiMeshGroupTF =	Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_MultiMeshGroupTF);
			bool isRender_Mixed =				Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectSelected_Mixed);
			bool isRender_NotSelected =			Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight_Setting_ObjectNotSelected);

			

			if (!isRender_SingleMeshTF &&
				!isRender_SingleMeshGroupTF &&
				!isRender_MultiMeshTF &&
				!isRender_MultiMeshGroupTF &&
				!isRender_Mixed &&
				!isRender_NotSelected)
			{
				return;
			}

			//공통 렌더링
			bool isRender_Selected = isRender_SingleMeshTF || isRender_SingleMeshGroupTF || isRender_MultiMeshTF || isRender_MultiMeshGroupTF || isRender_Mixed;

			//더미 여부 : 실제 데이터와 맞지 않을때 더미를 보여준다.
			bool isDummy = false;
			
			if(	(isRender_SingleMeshTF && propUIType != MESHGROUP_RIGHT_SETTING_PROPERTY_UI.SingleMeshTF) ||
				(isRender_SingleMeshGroupTF && propUIType != MESHGROUP_RIGHT_SETTING_PROPERTY_UI.SingleMeshGroupTF) ||
				(isRender_MultiMeshTF && propUIType != MESHGROUP_RIGHT_SETTING_PROPERTY_UI.MultipleMeshTF) ||
				(isRender_MultiMeshGroupTF && propUIType != MESHGROUP_RIGHT_SETTING_PROPERTY_UI.MultipleMeshGroupTF) ||
				(isRender_Mixed && propUIType != MESHGROUP_RIGHT_SETTING_PROPERTY_UI.Mixed)
				)
			{
				isDummy = true;
			}


			if (_guiContent_Right_MeshGroup_MeshIcon == null) { _guiContent_Right_MeshGroup_MeshIcon = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh)); }
			if (_guiContent_Right_MeshGroup_MeshGroupIcon == null) { _guiContent_Right_MeshGroup_MeshGroupIcon = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)); }
			if (_guiContent_Right_MeshGroup_MultipleSelected == null) { _guiContent_Right_MeshGroup_MultipleSelected = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MultiSelected)); }

			if (_guiContent_Right2MeshGroup_ObjectProp_Name == null) { _guiContent_Right2MeshGroup_ObjectProp_Name = new apGUIContentWrapper(); }
			if (_guiContent_Right2MeshGroup_ObjectProp_Type == null) { _guiContent_Right2MeshGroup_ObjectProp_Type = new apGUIContentWrapper(); }
			if (_guiContent_Right2MeshGroup_ObjectProp_NickName == null) { _guiContent_Right2MeshGroup_ObjectProp_NickName = new apGUIContentWrapper(); }

			//1. 오브젝트가 선택이 되었다.
			if (isRender_Selected)
			{
				_guiContent_Right2MeshGroup_ObjectProp_Name.ClearText(false);
				_guiContent_Right2MeshGroup_ObjectProp_Type.ClearText(false);
				_guiContent_Right2MeshGroup_ObjectProp_NickName.ClearText(false);

				bool isSocket = false;

				//더미가 아닌 경우에 값 할당
				if (!isDummy)
				{
					if (!isMultipleSelected)
					{
						//단일 선택인 경우
						if (isMeshTransform)
						{
							_guiContent_Right2MeshGroup_ObjectProp_Type.AppendText(Editor.GetUIWord(UIWORD.Mesh), true);
							_guiContent_Right2MeshGroup_ObjectProp_Name.AppendText(MeshTF_Main._mesh._name, true);
							_guiContent_Right2MeshGroup_ObjectProp_NickName.AppendText(MeshTF_Main._nickName, true);

							isSocket = MeshTF_Main._isSocket;
						}
						else
						{
							_guiContent_Right2MeshGroup_ObjectProp_Type.AppendText(Editor.GetUIWord(UIWORD.MeshGroup), true);
							_guiContent_Right2MeshGroup_ObjectProp_Name.AppendText(MeshGroupTF_Main._meshGroup._name, true);
							_guiContent_Right2MeshGroup_ObjectProp_NickName.AppendText(MeshGroupTF_Main._nickName, true);

							isSocket = MeshGroupTF_Main._isSocket;
						}
					}
					else
					{
						//다중 선택인 경우
						_guiContent_Right2MeshGroup_ObjectProp_Type.AppendText(Editor.GetUIWord(UIWORD.MultipleSelected), true);//"Multiple Selected"
						_guiContent_Right2MeshGroup_ObjectProp_Name.AppendText((nSubSelectedMeshTFs_All + nSubSelectedMeshGroupTFs_All), false);
						_guiContent_Right2MeshGroup_ObjectProp_Name.AppendSpaceText(1, false);
						_guiContent_Right2MeshGroup_ObjectProp_Name.AppendText(Editor.GetUIWord(UIWORD.Objects), true);//"Objects"
					}
				}

				

				//1. 아이콘 / 타입
				//[공통]
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(50));
				GUILayout.Space(10);
				//추가 20.6.1 : 다중 선택시 다르게 출력된다.
				if (!isMultipleSelected)
				{
					if (isMeshTransform)
					{
						EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_MeshIcon.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(50));
					}
					else
					{
						EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_MeshGroupIcon.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(50));
					}
				}
				else
				{
					//다중 선택인 경우, 다른 아이콘이 나온다.
					EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_MultipleSelected.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(50));
				}

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width - (50 + 10)));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_guiContent_Right2MeshGroup_ObjectProp_Type.Content, apGUILOFactory.I.Width(width - (50 + 10)));
				EditorGUILayout.LabelField(_guiContent_Right2MeshGroup_ObjectProp_Name.Content, apGUILOFactory.I.Width(width - (50 + 10)));


				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);


				//2. 닉네임 > 단일 선택만
				//[단일 선택]
				if (isRender_SingleMeshTF || isRender_SingleMeshGroupTF)
				{
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Name), apGUILOFactory.I.Width(80));//"Name"

					//추가 20.12.4 : 단축키로 포커싱하기 위함
					apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__SubTransformName);

					string nextNickName = EditorGUILayout.DelayedTextField(_guiContent_Right2MeshGroup_ObjectProp_NickName.Content.text, apGUILOFactory.I.Width(width));
					if (!isDummy && !string.Equals(nextNickName, _guiContent_Right2MeshGroup_ObjectProp_NickName.Content.text))
					{
						if (apEditorUtil.IsDelayedTextFieldEventValid(apStringFactory.I.GUI_ID__SubTransformName))//텍스트 변경이 유효한지도 체크한다.
						{
							if (isMeshTransform)
							{
								apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	_meshGroup, 
																	//MeshTF_Main, 
																	false, true,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								MeshTF_Main._nickName = nextNickName;
							}
							else
							{
								apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	_meshGroup, 
																	//MeshGroupTF_Main, 
																	false, true,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								MeshGroupTF_Main._nickName = nextNickName;
							}
						}
						//else
						//{
						//	Debug.LogError("포커스가 되지 않은 상태로 텍스트 변경");
						//}

						apEditorUtil.ReleaseGUIFocus();
						Editor.RefreshControllerAndHierarchy(false);
					}

					//추가 20.12.4 단축키로 트랜스폼의 이름을 바꿀 수 있다.
					if (!isDummy)
					{
						if (MeshTF_Main != null)
						{
							Editor.AddHotKeyEvent(OnHotKeyEvent_RenameTransform, apHotKeyMapping.KEY_TYPE.RenameObject, MeshTF_Main != null ? (object)MeshTF_Main : (object)MeshGroupTF_Main);
						}
					}
					


					GUILayout.Space(10);
				}

				//3. 소켓
				//[공통 - 분기]
				if (isRender_SingleMeshTF || isRender_SingleMeshGroupTF)
				{
					//단일 선택
					if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.SocketEnabled), Editor.GetUIWord(UIWORD.SocketDisabled), isSocket, true, width, 25))
					{
						if (!isDummy)
						{
							if (isMeshTransform)
							{
								apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	_meshGroup, 
																	//MeshTF_Main, 
																	false, true,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								MeshTF_Main._isSocket = !MeshTF_Main._isSocket;
							}
							else
							{
								apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	_meshGroup, 
																	//MeshGroupTF_Main, 
																	false, true,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
								MeshGroupTF_Main._isSocket = !MeshGroupTF_Main._isSocket;
							}
						}
					}
				}
				else
				{
					//추가 20.6.1 : IsSocket의 동기화된 값을 가져오자/설정하기
					_subObjects.Check_TF_IsSocket();
					if (apEditorUtil.ToggledButton_2Side_Sync(	Editor.GetUIWord(UIWORD.SocketEnabled), 
																Editor.GetUIWord(UIWORD.SocketDisabled), 
																_subObjects.Sync0.SyncValue_Bool,	//동기화 결과 (동기화가 되었다면)
																_subObjects.Sync0.IsValid,			//동기화 결과의 유효성
																_subObjects.Sync0.IsSync,			//동기화 여부
																width, 25))
					{
						if (!isDummy)
						{
							apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																Editor, 
																_meshGroup, 
																//MeshGroupTF_Main, 
																false, true,
																apEditorUtil.UNDO_STRUCT.ValueOnly);
							_subObjects.Set_TF_IsSocket();
						}
					}
				}
				

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);
				

				//4. MeshTF 전용 설정들
				//일부에 항목에 따라서 [단일/다중] 처리
				//if (isMeshTransform && isMeshTransformDetailRendererable)//이전
				if(isMeshTransform && (isRender_SingleMeshTF || isRender_MultiMeshTF))//변경
				{
					//4-1. "Shader Setting"
					//[공통-분기]
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ShaderSetting));

					GUILayout.Space(5);

					//Blend 방식 (Enum)
					apPortrait.SHADER_TYPE curShaderType = apPortrait.SHADER_TYPE.AlphaBlend;
					if(!isDummy)
					{
						if(isRender_SingleMeshTF)
						{
							//단일 선택인 경우
							curShaderType = MeshTF_Main._shaderType;
						}
						else
						{
							//다중 선택인 경우
							_subObjects.Check_Mesh_ShaderType();
							if(_subObjects.Sync0.IsSync && _subObjects.Sync0.IsValid)
							{
								curShaderType = (apPortrait.SHADER_TYPE)(_subObjects.Sync0.SyncValue_Int);
							}
							else
							{
								curShaderType = (apPortrait.SHADER_TYPE)(-1);
							}
						}
					}
					apPortrait.SHADER_TYPE nextShaderType = (apPortrait.SHADER_TYPE)EditorGUILayout.EnumPopup(curShaderType);
					if (!isDummy && nextShaderType != curShaderType)
					{
						//값이 바뀌었다면
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, 
															_meshGroup, 
															//MeshTF_Main, 
															false, true,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						if(isRender_SingleMeshTF)	//[단일 선택]
						{	
							MeshTF_Main._shaderType = nextShaderType;
						}
						else						//[다중 선택]
						{
							_subObjects.Set_Mesh_ShaderType(nextShaderType);
						}
						
					}

					GUILayout.Space(5);



					//이 아래로는 Single 선택인 경우에만 적용한다.
					if (isRender_SingleMeshTF)
					{
						//Shader Mode (Material Library <-> Custom Shader)
						int shaderMode = MeshTF_Main._isCustomShader ? MESH_SHADER_MODE__CUSTOM_SHADER : MESH_SHADER_MODE__MATERIAL_SET;
						int nextShaderMode = EditorGUILayout.Popup(shaderMode, _shaderMode_Names);

						if (nextShaderMode != shaderMode)
						{
							apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																Editor, 
																_meshGroup, 
																//MeshTF_Main, 
																false, true,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							MeshTF_Main._isCustomShader = (nextShaderMode == MESH_SHADER_MODE__CUSTOM_SHADER);
							apEditorUtil.ReleaseGUIFocus();
						}

						bool isCustomShader = MeshTF_Main._isCustomShader;

						GUILayout.Space(5);
						//현재 모드를 Box로 설명
						//GUIStyle guiStyle_BoxCenter = new GUIStyle(GUI.skin.box);
						//guiStyle_BoxCenter.alignment = TextAnchor.MiddleCenter;

						//Box의 색상을 지정해주자.
						Color prevColor = GUI.backgroundColor;

						if (_guiContent_Right_MeshGroup_MaterialSet == null)
						{
							_guiContent_Right_MeshGroup_MaterialSet = apGUIContentWrapper.Make(1, _editor.GetUIWord(UIWORD.MaterialSet), _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet));
						}
						if (_guiContent_Right_MeshGroup_CustomShader == null)
						{
							_guiContent_Right_MeshGroup_CustomShader = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.CustomShader), _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_CustomShader));
						}


						if (!isCustomShader)
						{
							//Material Library
							//초록색
							GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 1.0f, prevColor.b * 0.7f, 1.0f);
							//" Material Set"
							//이전
							//GUILayout.Box(new GUIContent(" " + _editor.GetUIWord(UIWORD.MaterialSet), _editor.ImageSet.Get(apImageSet.PRESET.MaterialSet)), guiStyle_BoxCenter, GUILayout.Width(width), GUILayout.Height(30));

							//변경
							GUILayout.Box(_guiContent_Right_MeshGroup_MaterialSet.Content, apGUIStyleWrapper.I.Box_MiddleCenter, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
						}
						else
						{
							//Custom Shader > Material Set을 사용하지 않는다.
							//파란색
							GUI.backgroundColor = new Color(prevColor.r * 0.7f, prevColor.g * 0.9f, prevColor.b * 1.0f, 1.0f);

							//이전
							//GUILayout.Box(new GUIContent(" " + Editor.GetUIWord(UIWORD.CustomShader),
							//								_editor.ImageSet.Get(apImageSet.PRESET.MaterialSet_CustomShader)),
							//					guiStyle_BoxCenter,
							//					GUILayout.Width(width), GUILayout.Height(30));

							GUILayout.Box(_guiContent_Right_MeshGroup_CustomShader.Content,
												apGUIStyleWrapper.I.Box_MiddleCenter,
												apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
						}

						GUI.backgroundColor = prevColor;

						GUILayout.Space(5);

						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__CustomShader, isCustomShader);//"MeshGroup Mesh Setting - CustomShader"
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__MaterialLibrary, !isCustomShader);//"MeshGroup Mesh Setting - MaterialLibrary"
						bool isDraw_CustomShader = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__CustomShader);//"MeshGroup Mesh Setting - CustomShader"
						bool isDraw_MaterialLibrary = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__MaterialLibrary);//"MeshGroup Mesh Setting - MaterialLibrary"

						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__MatLib_NotUseDefault, !isCustomShader && !MeshTF_Main._isUseDefaultMaterialSet);//"MeshGroup Mesh Setting - MatLib/NotUseDefault"
						bool isDraw_MatLib_NotUseDefault = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__MatLib_NotUseDefault);//"MeshGroup Mesh Setting - MatLib/NotUseDefault"

						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__Same_Mesh, _tmp_PrevMeshTransform_MeshGroupSettingUI == MeshTF_Main);//"MeshGroup Mesh Setting - Same Mesh"
						bool isSameMeshTF = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Mesh_Setting__Same_Mesh);//"MeshGroup Mesh Setting - Same Mesh"
						if (Event.current.type != EventType.Layout)
						{
							_tmp_PrevMeshTransform_MeshGroupSettingUI = MeshTF_Main;
						}

						//if (!isCustomShader)
						if (isDraw_MaterialLibrary)
						{
							//1. Material Library 모드
							//"Material Set"
							EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.MaterialSet));

							//"Use Default Material Set"
							//bool nextUseDefaultMaterialSet = EditorGUILayout.Toggle(, SubMeshInGroup._isUseDefaultMaterialSet);

							if (_guiContent_MaterialSet_ON == null)
							{
								_guiContent_MaterialSet_ON = new apGUIContentWrapper();
								_guiContent_MaterialSet_ON.ClearText(false);
								_guiContent_MaterialSet_ON.AppendText(Editor.GetUIWord(UIWORD.UseDefaultMaterialSet), false);
								_guiContent_MaterialSet_ON.AppendSpaceText(1, false);
								_guiContent_MaterialSet_ON.AppendText(apStringFactory.I.ON, true);
							}
							if (_guiContent_MaterialSet_OFF == null)
							{
								_guiContent_MaterialSet_OFF = new apGUIContentWrapper();
								_guiContent_MaterialSet_OFF.ClearText(false);
								_guiContent_MaterialSet_OFF.AppendText(Editor.GetUIWord(UIWORD.UseDefaultMaterialSet), false);
								_guiContent_MaterialSet_OFF.AppendSpaceText(1, false);
								_guiContent_MaterialSet_OFF.AppendText(apStringFactory.I.OFF, true);
							}



							//if (nextUseDefaultMaterialSet != SubMeshInGroup._isUseDefaultMaterialSet)
							if (apEditorUtil.ToggledButton_2Side(_guiContent_MaterialSet_ON.Content.text,
																	_guiContent_MaterialSet_OFF.Content.text,
																	MeshTF_Main._isUseDefaultMaterialSet, true, width, 22))
							{
								apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	_meshGroup, 
																	//MeshTF_Main, 
																	false, true,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								bool nextUseDefaultMaterialSet = !MeshTF_Main._isUseDefaultMaterialSet;

								MeshTF_Main._isUseDefaultMaterialSet = nextUseDefaultMaterialSet;

								if (nextUseDefaultMaterialSet)
								{
									//Default MaterialSet이 False > True로 바뀐 경우
									MeshTF_Main._linkedMaterialSet = Portrait.GetDefaultMaterialSet();
									if (MeshTF_Main._linkedMaterialSet != null)
									{
										MeshTF_Main._materialSetID = MeshTF_Main._linkedMaterialSet._uniqueID;
									}
								}

								apEditorUtil.ReleaseGUIFocus();
							}



							if (!MeshTF_Main._isUseDefaultMaterialSet && isDraw_MatLib_NotUseDefault)
							{
								//Default Material Set을 사용하지 않는 경우
								//EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(26));
								//GUILayout.Space(5);
								if (MeshTF_Main._linkedMaterialSet != null)
								{
									//string matSetName = " " + SubMeshInGroup._linkedMaterialSet._name;
									Texture2D matSetImg = null;
									switch (MeshTF_Main._linkedMaterialSet._icon)
									{
										case apMaterialSet.ICON.Unlit:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Unlit);
											break;
										case apMaterialSet.ICON.Lit:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Lit);
											break;
										case apMaterialSet.ICON.LitSpecular:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitSpecular);
											break;
										case apMaterialSet.ICON.LitSpecularEmission:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitSpecularEmission);
											break;
										case apMaterialSet.ICON.LitRimlight:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitRim);
											break;
										case apMaterialSet.ICON.LitRamp:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitRamp);
											break;
										case apMaterialSet.ICON.Effect:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_FX);
											break;
										case apMaterialSet.ICON.Cartoon:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Cartoon);
											break;
										case apMaterialSet.ICON.Custom1:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom1);
											break;
										case apMaterialSet.ICON.Custom2:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom2);
											break;
										case apMaterialSet.ICON.Custom3:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_Custom3);
											break;
										case apMaterialSet.ICON.UnlitVR:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_UnlitVR);
											break;
										case apMaterialSet.ICON.LitVR:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_LitVR);
											break;

											//추가 22.1.5
										case apMaterialSet.ICON.UnlitMergeable:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_MergeableUnlit);
											break;
										case apMaterialSet.ICON.LitMergeable:
											matSetImg = Editor.ImageSet.Get(apImageSet.PRESET.MaterialSetIcon_MergeableLit);
											break;
									}

									//이전
									//GUILayout.Box(new GUIContent(matSetName, matSetImg), guiStyle_BoxCenter, GUILayout.Width(width), GUILayout.Height(30));

									//변경
									if (_guiContent_Right_MeshGroup_MatSetName == null)
									{
										_guiContent_Right_MeshGroup_MatSetName = new apGUIContentWrapper();
									}

									_guiContent_Right_MeshGroup_MatSetName.SetText(1, MeshTF_Main._linkedMaterialSet._name);
									_guiContent_Right_MeshGroup_MatSetName.SetImage(matSetImg);

									GUILayout.Box(_guiContent_Right_MeshGroup_MatSetName.Content, apGUIStyleWrapper.I.Box_MiddleCenter, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
								}
								else
								{
									//붉은색으로 표시
									GUI.backgroundColor = new Color(prevColor.r * 1.0f, prevColor.g * 0.7f, prevColor.b * 0.7f, 1.0f);
									//GUILayout.Box(Editor.GetUIWord(UIWORD.None), guiStyle_BoxCenter, GUILayout.Width(width - (5 + 80)), GUILayout.Height(26));
									GUILayout.Box(Editor.GetUIWord(UIWORD.None), apGUIStyleWrapper.I.Box_MiddleCenter, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
									GUI.backgroundColor = prevColor;
								}
								//if (GUILayout.Button(_editor.GetUIWord(UIWORD.Change), GUILayout.Width(80), GUILayout.Height(26)))
								if (GUILayout.Button(_editor.GetUIWord(UIWORD.Change), apGUILOFactory.I.Height(22)))
								{
									//Material Set을 선택하자.
									_loadKey_SelectMaterialSetOfMeshTransform = apDialog_SelectMaterialSet.ShowDialog(
																									Editor,
																									false,
																									Editor.GetUIWord(UIWORD.SelectMaterialSet),//"Select Material Set",
																									false, OnMaterialSetOfMeshTFSelected,
																									MeshTF_Main);
								}

								//EditorGUILayout.EndHorizontal();
							}
							//Material Library 열기 버튼
							//"Open Material Library"
							if (GUILayout.Button(Editor.GetUIWord(UIWORD.OpenMaterialLibrary), apGUILOFactory.I.Height(24)))
							{
								apDialog_MaterialLibrary.ShowDialog(Editor, Portrait);
							}
						}

						if (isDraw_CustomShader)
						{
							//2. Custom Shader 모드
							try
							{
								Shader nextCustomShader = (Shader)EditorGUILayout.ObjectField(MeshTF_Main._customShader, typeof(Shader), false);
								if (MeshTF_Main._customShader != nextCustomShader)
								{
									//Object Field 열때 Record하면 안된다.
									//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshInGroup, false, true);

									MeshTF_Main._customShader = nextCustomShader;

									//apEditorUtil.ReleaseGUIFocus();
									apEditorUtil.SetEditorDirty();
								}
							}
							catch (Exception) { }
						}
						GUILayout.Space(20);

						//추가 19.6.10 : Shader Parameter의 기본값을 정하자
						//"Custom Shader Properties"
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.CustomShaderProperties));
						GUILayout.Space(5);

						//GUIStyle guiStyle_PropRemoveBtn = new GUIStyle(GUI.skin.button);
						//guiStyle_PropRemoveBtn.margin = GUI.skin.textField.margin;

						List<apTransform_Mesh.CustomMaterialProperty> cutomMatProps = MeshTF_Main._customMaterialProperties;
						apTransform_Mesh.CustomMaterialProperty removeMatProp = null;


						if (isSameMeshTF)//출력 가능한 경우
						{
							for (int iMatProp = 0; iMatProp < cutomMatProps.Count; iMatProp++)
							{
								apTransform_Mesh.CustomMaterialProperty matProp = cutomMatProps[iMatProp];

								//Line1 : 이름, 타입, 삭제
								EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));

								GUILayout.Space(5);
								string nextPropName = EditorGUILayout.DelayedTextField(matProp._name, apGUILOFactory.I.Width(width - (5 + 70 + 20 + 10)));
								if (!string.Equals(nextPropName, matProp._name))
								{
									//이름 변경
									apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																		Editor, 
																		_meshGroup, 
																		//MeshTF_Main, 
																		false, true,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

									matProp._name = nextPropName;
								}
								apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE nextPropType =
									(apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE)EditorGUILayout.EnumPopup(matProp._propType, apGUILOFactory.I.Width(70));

								if (nextPropType != matProp._propType)
								{
									//타입 변경
									apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																		Editor, 
																		_meshGroup, 
																		//MeshTF_Main, 
																		false, true,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);
									matProp._propType = nextPropType;
								}

								GUILayout.Space(5);
								if (GUILayout.Button(apStringFactory.I.X, apGUIStyleWrapper.I.Button_TextFieldMargin, apGUILOFactory.I.Width(18), apGUILOFactory.I.Height(18)))//"X"
								{
									//삭제
									removeMatProp = matProp;
								}
								EditorGUILayout.EndHorizontal();

								//Line2 : 속성에 따른 값
								EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(24));
								GUILayout.Space(10);
								int width_PropLabel = 80;
								int width_PropValue = width - (width_PropLabel + 10 + 5);
								switch (matProp._propType)
								{
									case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Float:
										{
											EditorGUILayout.LabelField(apStringFactory.I.Float, apGUILOFactory.I.Width(width_PropLabel));//"Float"
											float nextFloatValue = EditorGUILayout.DelayedFloatField(matProp._value_Float, apGUILOFactory.I.Width(width_PropValue));
											if (Mathf.Abs(nextFloatValue - matProp._value_Float) > 0.0001f)
											{
												apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																					Editor, 
																					_meshGroup, 
																					//MeshTF_Main, 
																					false, true,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												matProp._value_Float = nextFloatValue;
												apEditorUtil.ReleaseGUIFocus();
											}
										}
										break;
									case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Int:
										{
											EditorGUILayout.LabelField(apStringFactory.I.Integer, apGUILOFactory.I.Width(width_PropLabel));//"Integer"
											int nextIntValue = EditorGUILayout.DelayedIntField(matProp._value_Int, apGUILOFactory.I.Width(width_PropValue));
											if (nextIntValue != matProp._value_Int)
											{
												apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																					Editor, _meshGroup, 
																					//MeshTF_Main, 
																					false, true,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);
												matProp._value_Int = nextIntValue;
												apEditorUtil.ReleaseGUIFocus();
											}
										}
										break;
									case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Vector:
										{
											int width_PropVecValue = (width_PropValue / 4) - 3;
											EditorGUILayout.LabelField(apStringFactory.I.Vector, apGUILOFactory.I.Width(width_PropLabel));//"Vector"
											float nextV_X = EditorGUILayout.DelayedFloatField(matProp._value_Vector.x, apGUILOFactory.I.Width(width_PropVecValue));
											float nextV_Y = EditorGUILayout.DelayedFloatField(matProp._value_Vector.y, apGUILOFactory.I.Width(width_PropVecValue));
											float nextV_Z = EditorGUILayout.DelayedFloatField(matProp._value_Vector.z, apGUILOFactory.I.Width(width_PropVecValue));
											float nextV_W = EditorGUILayout.DelayedFloatField(matProp._value_Vector.w, apGUILOFactory.I.Width(width_PropVecValue + 1));

											if (Mathf.Abs(nextV_X - matProp._value_Vector.x) > 0.0001f ||
												Mathf.Abs(nextV_Y - matProp._value_Vector.y) > 0.0001f ||
												Mathf.Abs(nextV_Z - matProp._value_Vector.z) > 0.0001f ||
												Mathf.Abs(nextV_W - matProp._value_Vector.w) > 0.0001f)
											{
												apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																					Editor, 
																					_meshGroup, 
																					//MeshTF_Main, 
																					false, true,
																					apEditorUtil.UNDO_STRUCT.ValueOnly);

												matProp._value_Vector.x = nextV_X;
												matProp._value_Vector.y = nextV_Y;
												matProp._value_Vector.z = nextV_Z;
												matProp._value_Vector.w = nextV_W;
												apEditorUtil.ReleaseGUIFocus();
											}
										}
										break;
									case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Texture:
										{
											EditorGUILayout.LabelField(apStringFactory.I.Texture, apGUILOFactory.I.Width(width_PropLabel));//"Texture"
											try
											{
												Texture nextTex = (Texture)EditorGUILayout.ObjectField(matProp._value_Texture, typeof(Texture), false, apGUILOFactory.I.Width(width_PropValue));
												if (nextTex != matProp._value_Texture)
												{
													//Object Field 사용시 Record 불가
													//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshInGroup, false, true);
													matProp._value_Texture = nextTex;
													//apEditorUtil.ReleaseGUIFocus();
												}
											}
											catch (Exception) { }

										}
										break;
									case apTransform_Mesh.CustomMaterialProperty.SHADER_PROP_TYPE.Color:
										{
											EditorGUILayout.LabelField(apStringFactory.I.Color, apGUILOFactory.I.Width(width_PropLabel));//"Color"
											try
											{
												Color nextColor = EditorGUILayout.ColorField(matProp._value_Color, apGUILOFactory.I.Width(width_PropValue));
												if (Mathf.Abs(nextColor.r - matProp._value_Color.r) > 0.0001f ||
													Mathf.Abs(nextColor.g - matProp._value_Color.g) > 0.0001f ||
													Mathf.Abs(nextColor.b - matProp._value_Color.b) > 0.0001f ||
													Mathf.Abs(nextColor.a - matProp._value_Color.a) > 0.0001f)
												{
													//색상은 Record 불가
													//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshInGroup, false, true);
													matProp._value_Color = nextColor;
													//apEditorUtil.ReleaseGUIFocus();
												}
											}
											catch (Exception) { }
										}
										break;
								}

								EditorGUILayout.EndHorizontal();

								GUILayout.Space(5);
							}
						}

						if (removeMatProp != null)
						{
							//"Remove Shader Property", "Do you want to remove the Custom Property [ " + removeMatProp._name + " ]?"
							bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.DLG_RemoveCustomShaderProp_Title),
																			Editor.GetTextFormat(TEXT.DLG_RemoveCustomShaderProp_Body, removeMatProp._name),
																			Editor.GetText(TEXT.Remove),
																			Editor.GetText(TEXT.Cancel));

							if (isResult)
							{
								apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	_meshGroup, 
																	//MeshTF_Main, 
																	false, true,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
								cutomMatProps.Remove(removeMatProp);
							}
							removeMatProp = null;
						}

						//"Add Custom Proprty"
						if (GUILayout.Button(Editor.GetUIWord(UIWORD.AddCustomProperty)))
						{
							apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																Editor, 
																_meshGroup, 
																//MeshTF_Main, 
																false, true,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							apTransform_Mesh.CustomMaterialProperty newProp = new apTransform_Mesh.CustomMaterialProperty();
							newProp.MakeEmpty();
							cutomMatProps.Add(newProp);
						}


						GUILayout.Space(20);

						//양면 렌더링

						//이전 : 토글 UI
						bool isNext2Side = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.TwoSidesRendering), MeshTF_Main._isAlways2Side);//"2-Sides"
						if (MeshTF_Main._isAlways2Side != isNext2Side)
						{
							apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																Editor, 
																_meshGroup, 
																//MeshTF_Main, 
																false, true,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							MeshTF_Main._isAlways2Side = isNext2Side;
						}

						//변경 19.6.10 : 토글 버튼
						//string str_twoSidesRendering = Editor.GetUIWord(UIWORD.TwoSidesRendering);
						//if(apEditorUtil.ToggledButton_2Side(str_twoSidesRendering, str_twoSidesRendering, SubMeshInGroup._isAlways2Side, true, width, 20))
						//{
						//	apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, Editor, _meshGroup, SubMeshInGroup, false, true);
						//	SubMeshInGroup._isAlways2Side = !SubMeshInGroup._isAlways2Side;
						//}

						GUILayout.Space(10);
						apEditorUtil.GUI_DelimeterBoxH(width - 10);
						GUILayout.Space(10);

						//추가 9.25 : 그림자 설정 : TODO 번역
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ShadowSetting));//"Shadow Setting"

						//"Override Shadow Setting", "Use Common Shadow Setting"
						if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.OverrideShadow), Editor.GetUIWord(UIWORD.UseCommonShadowSetting), !MeshTF_Main._isUsePortraitShadowOption, true, width, 20))
						{
							apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																Editor, 
																_meshGroup, 
																//MeshTF_Main, 
																false, true,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							MeshTF_Main._isUsePortraitShadowOption = !MeshTF_Main._isUsePortraitShadowOption;
						}
						GUILayout.Space(5);
						apPortrait.SHADOW_CASTING_MODE prevShadowCastMode = MeshTF_Main._isUsePortraitShadowOption ? Portrait._meshShadowCastingMode : MeshTF_Main._shadowCastingMode;
						bool prevReceiveShadow = MeshTF_Main._isUsePortraitShadowOption ? Portrait._meshReceiveShadow : MeshTF_Main._receiveShadow;


						//"Cast Shadows"
						apPortrait.SHADOW_CASTING_MODE nextChastShadows = (apPortrait.SHADOW_CASTING_MODE)EditorGUILayout.EnumPopup(Editor.GetUIWord(UIWORD.CastShadows), prevShadowCastMode);
						//"Receive Shadows"
						bool nextReceiveShaodw = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.ReceiveShadows), prevReceiveShadow);

						if (!MeshTF_Main._isUsePortraitShadowOption)
						{
							if (nextChastShadows != MeshTF_Main._shadowCastingMode
								|| nextReceiveShaodw != MeshTF_Main._receiveShadow)
							{
								apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
																	Editor, 
																	_meshGroup, 
																	//MeshTF_Main, 
																	false, true,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								MeshTF_Main._shadowCastingMode = nextChastShadows;
								MeshTF_Main._receiveShadow = nextReceiveShaodw;
							}
						}

						GUILayout.Space(10);
						apEditorUtil.GUI_DelimeterBoxH(width - 10);
						GUILayout.Space(10);

						//다른 메시로 설정 복사하기
						//" Copy Settings to Other Meshes"

						//이전
						//if (GUILayout.Button(new GUIContent(" " + Editor.GetUIWord(UIWORD.CopySettingsToOtherMeshes), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy)), GUILayout.Height(24)))
						if (_guiContent_Right_MeshGroup_CopySettingToOtherMeshes == null)
						{
							_guiContent_Right_MeshGroup_CopySettingToOtherMeshes = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.CopySettingsToOtherMeshes), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy));
						}

						if (GUILayout.Button(_guiContent_Right_MeshGroup_CopySettingToOtherMeshes.Content, apGUILOFactory.I.Height(24)))
						{
							_loadKey_SelectOtherMeshTransformForCopyingSettings = apDialog_SelectMeshTransformsToCopy.ShowDialog(
																		Editor,
																		MeshGroup,
																		MeshTF_Main,
																		OnSelectOtherMeshTransformsForCopyingSettings);
						}

						GUILayout.Space(10);
						apEditorUtil.GUI_DelimeterBoxH(width - 10);
						GUILayout.Space(10);

						//GUIStyle guiStyle_ClipStatus = new GUIStyle(GUI.skin.box);
						//guiStyle_ClipStatus.alignment = TextAnchor.MiddleCenter;
						//guiStyle_ClipStatus.normal.textColor = apEditorUtil.BoxTextColor;

						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Transform_Detail_Status__Clipping_Child, MeshTF_Main._isClipping_Child);//"Mesh Transform Detail Status - Clipping Child"
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Transform_Detail_Status__Clipping_Parent, MeshTF_Main._isClipping_Parent);//"Mesh Transform Detail Status - Clipping Parent"
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Transform_Detail_Status__Clipping_None, (!MeshTF_Main._isClipping_Parent && !MeshTF_Main._isClipping_Child));//"Mesh Transform Detail Status - Clipping None"

						if (MeshTF_Main._isClipping_Parent)
						{
							if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Transform_Detail_Status__Clipping_Parent))//"Mesh Transform Detail Status - Clipping Parent"
							{
								//1. 자식 메시를 가지는 Clipping의 Base Parent이다.
								//- Mask 사이즈를 보여준다.
								//- 자식 메시 리스트들을 보여준다.
								//-> 레이어 순서를 바꾼다. / Clip을 해제한다..

								//"Parent Mask Mesh"
								GUILayout.Box(Editor.GetUIWord(UIWORD.ParentMaskMesh), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
								GUILayout.Space(5);

								EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MaskTextureSize), apGUILOFactory.I.Width(width));//"Mask Texture Size"
								int prevRTTIndex = (int)MeshTF_Main._renderTexSize;
								if (prevRTTIndex < 0 || prevRTTIndex >= apEditorUtil.GetRenderTextureSizeNames().Length)
								{
									prevRTTIndex = (int)(apTransform_Mesh.RENDER_TEXTURE_SIZE.s_256);
								}
								int nextRTTIndex = EditorGUILayout.Popup(prevRTTIndex, apEditorUtil.GetRenderTextureSizeNames(), apGUILOFactory.I.Width(width));
								if (nextRTTIndex != prevRTTIndex)
								{
									apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_ClippingChanged, 
																		Editor, 
																		MeshGroup, 
																		//null, 
																		false, true,
																		apEditorUtil.UNDO_STRUCT.StructChanged);

									MeshTF_Main._renderTexSize = (apTransform_Mesh.RENDER_TEXTURE_SIZE)nextRTTIndex;
								}


								GUILayout.Space(5);


								//Texture2D btnImg_Down = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown);
								//Texture2D btnImg_Up = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp);
								Texture2D btnImg_Delete = Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey);

								int iBtn = -1;
								//int btnRequestType = -1;


								for (int iChild = 0; iChild < MeshTF_Main._clipChildMeshes.Count; iChild++)
								{
									apTransform_Mesh childMesh = MeshTF_Main._clipChildMeshes[iChild]._meshTransform;
									EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
									if (childMesh != null)
									{
										EditorGUILayout.LabelField(childMesh._nickName, apGUILOFactory.I.Width(width - (20 + 5)), apGUILOFactory.I.Height(20));
										if (GUILayout.Button(btnImg_Delete, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20)))
										{
											iBtn = iChild;
											//btnRequestType = 2;//2 : Delete

										}
									}
									else
									{
										EditorGUILayout.LabelField(apStringFactory.I.Dot3, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
									}
									EditorGUILayout.EndHorizontal();
								}


								if (iBtn >= 0)
								{
									//Debug.LogError("TODO : Mesh 삭제");
									apTransform_Mesh targetChildTransform = MeshTF_Main._clipChildMeshes[iBtn]._meshTransform;
									if (targetChildTransform != null)
									{
										//해당 ChildMesh를 Release하자
										Editor.Controller.ReleaseClippingMeshTransform(MeshGroup, targetChildTransform);
									}
								}
							}
						}
						else if (MeshTF_Main._isClipping_Child)
						{
							if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Mesh_Transform_Detail_Status__Clipping_Child))//"Mesh Transform Detail Status - Clipping Child"
							{
								//2. Parent를 Mask로 삼는 자식 Mesh이다.
								//- 부모 메시를 보여준다.
								//-> 순서 바꾸기를 요청한다
								//-> Clip을 해제한다.
								//"Child Clipped Mesh" ->"Clipped Child Mesh"
								GUILayout.Box(Editor.GetUIWord(UIWORD.ClippedChildMesh), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
								GUILayout.Space(5);

								if (_guiContent_Right2MeshGroup_MaskParentName == null)
								{
									_guiContent_Right2MeshGroup_MaskParentName = new apGUIContentWrapper();
								}

								//string strParentName = "<No Mask Parent>";
								if (MeshTF_Main._clipParentMeshTransform != null)
								{
									//strParentName = SubMeshInGroup._clipParentMeshTransform._nickName;
									_guiContent_Right2MeshGroup_MaskParentName.ClearText(false);
									_guiContent_Right2MeshGroup_MaskParentName.AppendText(Editor.GetUIWord(UIWORD.MaskMesh), false);
									_guiContent_Right2MeshGroup_MaskParentName.AppendText(apStringFactory.I.Colon_Space, false);
									_guiContent_Right2MeshGroup_MaskParentName.AppendText(MeshTF_Main._clipParentMeshTransform._nickName, true);
								}
								else
								{
									_guiContent_Right2MeshGroup_MaskParentName.ClearText(false);
									_guiContent_Right2MeshGroup_MaskParentName.AppendText(Editor.GetUIWord(UIWORD.MaskMesh), false);
									_guiContent_Right2MeshGroup_MaskParentName.AppendText(apStringFactory.I.Colon_Space, false);
									_guiContent_Right2MeshGroup_MaskParentName.AppendText(apStringFactory.I.NoMaskParent, true);
								}

								//"Mask Parent" -> "Mask Mesh"
								EditorGUILayout.LabelField(_guiContent_Right2MeshGroup_MaskParentName.Content, apGUILOFactory.I.Width(width));
								//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ClippedIndex) + " : " + SubMeshInGroup._clipIndexFromParent, GUILayout.Width(width));//"Clipped Index : "//<<필요없으니 삭제 19.12.6
								EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

								//int btnRequestType = -1;
								//"Release"
								if (GUILayout.Button(Editor.GetUIWord(UIWORD.Release), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))
								{
									//btnRequestType = 2;//2 : Delete
									Editor.Controller.ReleaseClippingMeshTransform(MeshGroup, MeshTF_Main);
								}
								EditorGUILayout.EndHorizontal();


							}
						}
						else
						{
							//3. 기본 상태의 Mesh이다.
							//Clip을 요청한다.
							//"Clipping To Below Mesh" -> "Clip to Below Mesh"
							if (GUILayout.Button(Editor.GetUIWord(UIWORD.ClipToBelowMesh), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))
							{
								Editor.Controller.AddClippingMeshTransform(MeshGroup, MeshTF_Main, true, true, true);
							}
						}
					}


					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width - 10);
					GUILayout.Space(10);
				}



				//Duplicate / Migrate (단일)

				if (isRender_Selected 
					&& (
					isRender_SingleMeshTF 
					|| isRender_SingleMeshGroupTF
					//추가 21.6.21 : 여러개의 객체들을 대상으로도 복제나 이주가 가능하다
					|| isRender_MultiMeshTF
					|| isRender_MultiMeshGroupTF
					|| isRender_Mixed
					))
				{
					//추가 20.1.16 : Duplicate기능 추가하자

					//Duplicate 버튼
					if (_guiContent_Right2MeshGroup_DuplicateTransform == null)
					{
						_guiContent_Right2MeshGroup_DuplicateTransform = apGUIContentWrapper.Make(Editor.GetUIWord(UIWORD.Duplicate), false);
					}
					if (GUILayout.Button(_guiContent_Right2MeshGroup_DuplicateTransform.Content))
					{
						//복사하기

						if (!isDummy)
						{
							if (isRender_SingleMeshTF || isRender_SingleMeshGroupTF)
							{
								//[단일]
								if (isMeshTransform)
								{
									Editor.Controller.DuplicateMeshTransformInSameMeshGroup(MeshTF_Main);
								}
								else
								{
									Editor.Controller.DuplicateMeshGroupTransformInSameMeshGroup(MeshGroupTF_Main);
								}
							}
							else if(isRender_MultiMeshTF || isRender_MultiMeshGroupTF || isRender_Mixed)
							{
								//[다중]
								Editor.Controller.DuplicateMultipleTFsInSameMeshGroup(selectedMeshTFs, selectedMeshGroupTFs);
							}
						}
					}

					//20.1.20 : Migrate 버튼 추가
					//21.6.23 : 메시 여러개 이주 가능
					//if (isMeshTransform && isMeshTransformDetailRendererable)
					if (isMeshTransform && (isRender_SingleMeshTF || isRender_MultiMeshTF) && MeshTF_Main != null)
					{
						if (!isDummy)
						{
							if (_guiContent_Right2MeshGroup_MigrateTransform == null)
							{
								_guiContent_Right2MeshGroup_MigrateTransform = apGUIContentWrapper.Make(Editor.GetUIWord(UIWORD.Migrate), false);//"Migrate"
							}

							if (GUILayout.Button(_guiContent_Right2MeshGroup_MigrateTransform.Content))
							{
								//추가 20.1.18 : 다른 메시 그룹으로 메시를 이전하자
								if(isRender_SingleMeshTF)
								{
									_loadKey_MigrateMeshTransform = apDialog_SelectMigrateMeshGroup.ShowDialog(Editor, MeshTF_Main, null, OnSelectMeshGroupToMigrate);
								}
								else if(isRender_MultiMeshTF)
								{
									//대상이 여러개인 경우, 모든 MeshTF를 넣을게 아니라, 조건에 맞는 TF만 리스트에 넣어야 한다.
									//- 리스트에 MainTF가 들어가야 한다.
									//- MainTF의 ParentMeshGroup에 동일하게 속하고 있어야 한다.
									//- 제외되는 MeshTF가 있다면 안내문을 보여준다.
									List<apTransform_Mesh> targetMeshTFs = new List<apTransform_Mesh>();
									apMeshGroup srcParentMeshGroup = MeshGroup.GetSubParentMeshGroupOfTransformRecursive(MeshTF_Main, null);
									if(srcParentMeshGroup != null)
									{
										//조건에 맞는걸 하나씩 넣자
										int nUnmatchedTFs = 0;
										
										int nSubMeshTFs = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
										apTransform_Mesh curSubMeshTF = null;
										
										for (int iSubMeshTF = 0; iSubMeshTF < nSubMeshTFs; iSubMeshTF++)
										{
											curSubMeshTF = selectedMeshTFs[iSubMeshTF];

											if(srcParentMeshGroup._childMeshTransforms != null
												&& srcParentMeshGroup._childMeshTransforms.Contains(curSubMeshTF))
											{
												//추가한다.
												targetMeshTFs.Add(curSubMeshTF);
											}
											else
											{
												//추가할 수 없다. 다른 메시 그룹에 속해있다.
												nUnmatchedTFs += 1;
											}
										}

										if(!targetMeshTFs.Contains(MeshTF_Main))
										{
											//메인이 없으면 추가한다.
											targetMeshTFs.Add(MeshTF_Main);
										}

										//만약 조건에 맞지 않은 MeshTF가 있다면 > 안내문을 보여준다.
										if(nUnmatchedTFs > 0)
										{
											//"Migration Info"
											//nUnmatchedTFs + " Sub Meshes cannot be migrated because they belong to different mesh groups.\nRun the migration by selecting the sub-meshes again."
											EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_MigratationMultipleMeshTF_Title), 
																			Editor.GetTextFormat(TEXT.DLG_MigratationMultipleMeshTF_Body, nUnmatchedTFs), 
																			Editor.GetText(TEXT.Okay));
										}


										_loadKey_MigrateMeshTransform = apDialog_SelectMigrateMeshGroup.ShowDialog(Editor, MeshTF_Main, targetMeshTFs, OnSelectMeshGroupToMigrate);
									}
								}
							}
						}
					}
					

					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width - 10);
					GUILayout.Space(10);
				}



				//4. Detach [단일 / 다중]
				//20.9.16 : 다중이 추가되었다. 단 같은 종류의 경우만 (Mixed 안됨)
				//v1.4.2 : Mixed를 허용한다.
				//--------------------------------------------------------------
				if(isRender_Selected && 
					(isRender_SingleMeshTF || isRender_SingleMeshGroupTF || isRender_MultiMeshTF || isRender_MultiMeshGroupTF || isRender_Mixed)
					)
				{

					if (_guiContent_Right2MeshGroup_DetachObject == null)
					{
						_guiContent_Right2MeshGroup_DetachObject = new apGUIContentWrapper();
					}

					if(isRender_SingleMeshTF || isRender_SingleMeshGroupTF)
					{
						//[단일 선택]

						_guiContent_Right2MeshGroup_DetachObject.ClearText(false);
						_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(2, false);
						_guiContent_Right2MeshGroup_DetachObject.AppendText(Editor.GetUIWord(UIWORD.Detach), false);
						_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(1, false);
						_guiContent_Right2MeshGroup_DetachObject.AppendText(apStringFactory.I.Bracket_2_L, false);
						//_guiContent_Right2MeshGroup_DetachObject.AppendText(_guiContent_Right2MeshGroup_ObjectProp_Type.Content.text, true);
						if(_guiContent_Right2MeshGroup_ObjectProp_NickName.Content.text.Length > 10)
						{
							_guiContent_Right2MeshGroup_DetachObject.AppendText(_guiContent_Right2MeshGroup_ObjectProp_NickName.Content.text.Substring(0, 10), false);
							_guiContent_Right2MeshGroup_DetachObject.AppendText(apStringFactory.I.Dot2, false);
						}
						else
						{
							_guiContent_Right2MeshGroup_DetachObject.AppendText(_guiContent_Right2MeshGroup_ObjectProp_NickName.Content.text, false);
						}
					
						_guiContent_Right2MeshGroup_DetachObject.AppendText(apStringFactory.I.Bracket_2_R, true);
						_guiContent_Right2MeshGroup_DetachObject.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
					}
					else if(isRender_MultiMeshTF || isRender_MultiMeshGroupTF)
					{
						//[다중 선택]
						_guiContent_Right2MeshGroup_DetachObject.ClearText(false);
						_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(2, false);
						_guiContent_Right2MeshGroup_DetachObject.AppendText(Editor.GetUIWord(UIWORD.Detach), false);
						_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(1, false);
						_guiContent_Right2MeshGroup_DetachObject.AppendText(apStringFactory.I.Bracket_2_L, false);
						//삭제하는 객체 개수 입력
						if(isRender_MultiMeshTF)
						{
							_guiContent_Right2MeshGroup_DetachObject.AppendText(nSubSelectedMeshTFs_All, false);
							_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(1, false);
							_guiContent_Right2MeshGroup_DetachObject.AppendText(Editor.GetUIWord(UIWORD.Meshes), false);
						}
						else
						{
							_guiContent_Right2MeshGroup_DetachObject.AppendText(nSubSelectedMeshGroupTFs_All, false);
							_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(1, false);
							_guiContent_Right2MeshGroup_DetachObject.AppendText(Editor.GetUIWord(UIWORD.MeshGroups), false);
						}

						_guiContent_Right2MeshGroup_DetachObject.AppendText(apStringFactory.I.Bracket_2_R, true);
						_guiContent_Right2MeshGroup_DetachObject.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
					}
					else if(isRender_Mixed)
					{
						//추가 v1.4.2 : 다중 선택이면서 MeshTF와 MeshGroupTF가 섞여있는 상태
						_guiContent_Right2MeshGroup_DetachObject.ClearText(false);
						_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(2, false);
						_guiContent_Right2MeshGroup_DetachObject.AppendText(Editor.GetUIWord(UIWORD.Detach), false);
						_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(1, false);
						_guiContent_Right2MeshGroup_DetachObject.AppendText(apStringFactory.I.Bracket_2_L, false);
						
						//삭제하는 객체 개수(혼합) 입력
						_guiContent_Right2MeshGroup_DetachObject.AppendText((nSubSelectedMeshTFs_All + nSubSelectedMeshGroupTFs_All), false);
						_guiContent_Right2MeshGroup_DetachObject.AppendSpaceText(1, false);
						_guiContent_Right2MeshGroup_DetachObject.AppendText(Editor.GetUIWord(UIWORD.Objects), false);

						_guiContent_Right2MeshGroup_DetachObject.AppendText(apStringFactory.I.Bracket_2_R, true);

						_guiContent_Right2MeshGroup_DetachObject.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
					}
					
					//단축키로 삭제할 수 있다. (20.9.16)
					//Editor.AddHotKeyEvent(OnHotKeyEvent_DetachTransform, apHotKey.LabelText.DetachTrasnforms, KeyCode.Delete, false, false, false, propUIType);
					Editor.AddHotKeyEvent(OnHotKeyEvent_DetachTransform, apHotKeyMapping.KEY_TYPE.RemoveObject, propUIType);//변경 20.12.3

					//Detach 버튼
					if (GUILayout.Button(_guiContent_Right2MeshGroup_DetachObject.Content, apGUILOFactory.I.Height(25)))//"Detach [" + strType + "]"
					{
						if (!isDummy)
						{
							if (isRender_SingleMeshTF || isRender_SingleMeshGroupTF)
							{
								//[단일 선택]
								//삭제 메시지 만들기
								string strDialogInfo = Editor.GetText(TEXT.Detach_Body);
								if (isMeshTransform)
								{
									strDialogInfo = Editor.Controller.GetRemoveItemMessage(
																		_portrait,
																		MeshTF_Main,
																		5,
																		Editor.GetText(TEXT.Detach_Body),
																		Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));
								}
								else
								{
									strDialogInfo = Editor.Controller.GetRemoveItemMessage(
																		_portrait,
																		MeshGroupTF_Main,
																		5,
																		Editor.GetText(TEXT.Detach_Body),
																		Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));
								}

								//bool isResult = EditorUtility.DisplayDialog("Detach", "Detach it?", "Detach", "Cancel");
								bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																				//Editor.GetText(TEXT.Detach_Body),
																				strDialogInfo,
																				Editor.GetText(TEXT.Detach_Ok),
																				Editor.GetText(TEXT.Cancel)
																				);
								if (isResult)
								{
									if (isMeshTransform)
									{
										Editor.Controller.DetachMeshTransform(MeshTF_Main, MeshGroup);
										Editor.Select.SelectMeshTF(null, MULTI_SELECT.Main);
									}
									else
									{
										Editor.Controller.DetachMeshGroupTransform(MeshGroupTF_Main, MeshGroup);
										Editor.Select.SelectMeshGroupTF(null, MULTI_SELECT.Main);
									}
								}
							}
							else
							{
								//[다중 선택]

								//변경 v1.4.2 : 다중 선택의 경우에도 변경되는 내역을 조회하자
								string strDialogInfo = Editor.Controller.GetRemoveItemsMessage(
																		_portrait,
																		selectedMeshTFs, selectedMeshGroupTFs,
																		5,
																		Editor.GetText(TEXT.Detach_Body),
																		Editor.GetText(TEXT.DLG_RemoveItemChangedWarning));


								bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.Detach_Title),
																				//Editor.GetText(TEXT.Detach_Body),
																				strDialogInfo,
																				Editor.GetText(TEXT.Detach_Ok),
																				Editor.GetText(TEXT.Cancel)
																				);

								if(isResult)
								{
									//이전
									//if(isRender_MultiMeshTF && nSubSelectedMeshTFs_All > 0)
									//{
									//	Editor.Controller.DetachMeshTransforms(selectedMeshTFs, MeshGroup);
									//	Editor.Select.SelectMeshTF(null, MULTI_SELECT.Main);
									//}
									//else if(isRender_MultiMeshGroupTF && nSubSelectedMeshGroupTFs_All > 0)
									//{
									//	Editor.Controller.DetachMeshGroupTransforms(selectedMeshGroupTFs, MeshGroup);
									//	Editor.Select.SelectMeshGroupTF(null, MULTI_SELECT.Main);
									//}

									//변경 v1.4.2 : 통합
									Editor.Controller.DetachMultipleTransforms(selectedMeshTFs, selectedMeshGroupTFs, MeshGroup);
									Editor.Select.SelectSubObject(null, null, null, MULTI_SELECT.Main, TF_BONE_SELECT.Exclusive);
								}
							}
							
							MeshGroup.SetDirtyToSort();//TODO : Sort에서 자식 객체 변한것 체크 : Clip 그룹 체크
							MeshGroup.RefreshForce();
							Editor.SetRepaint();
						}
					}
					//EditorGUILayout.EndVertical();
				}
			}
			else if (isRender_NotSelected)
			{
				//2. 오브젝트가 선택이 안되었다.
				//기본 정보를 출력하고, 루트 MeshGroupTransform의 Transform 값을 설정한다.
				apTransform_MeshGroup rootMeshGroupTransform = MeshGroup._rootMeshGroupTransform;

				if (_guiContent_Right_MeshGroup_MeshGroupIcon == null) { _guiContent_Right_MeshGroup_MeshGroupIcon = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)); }

				//1. 아이콘 / 타입
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(50));
				GUILayout.Space(10);

				//EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup)), GUILayout.Width(50), GUILayout.Height(50));
				EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_MeshGroupIcon.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(50));

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width - (50 + 10)));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MeshGroup), apGUILOFactory.I.Width(width - (50 + 12)));//"Mesh Group"
				EditorGUILayout.LabelField(MeshGroup._name, apGUILOFactory.I.Width(width - (50 + 12)));

				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width - 10);
				GUILayout.Space(10);


				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width));


				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.RootTransform));//"Root Transform"

				//Texture2D img_Pos = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move);
				//Texture2D img_Rot = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate);
				//Texture2D img_Scale = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale);

				if (_guiContent_Icon_ModTF_Pos == null) { _guiContent_Icon_ModTF_Pos = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move)); }
				if (_guiContent_Icon_ModTF_Rot == null) { _guiContent_Icon_ModTF_Rot = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate)); }
				if (_guiContent_Icon_ModTF_Scale == null) { _guiContent_Icon_ModTF_Scale = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale)); }

				int iconSize = 30;
				int propertyWidth = width - (iconSize + 12);

				//Position
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(iconSize));

				//EditorGUILayout.LabelField(new GUIContent(img_Pos), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField(_guiContent_Icon_ModTF_Pos.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconSize));

				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(propertyWidth), apGUILOFactory.I.Height(iconSize));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position), apGUILOFactory.I.Width(propertyWidth));//"Position"
																													 //nextPos = EditorGUILayout.Vector2Field("", nextPos, GUILayout.Width(propertyWidth));
				Vector2 rootPos = apEditorUtil.DelayedVector2Field(rootMeshGroupTransform._matrix._pos, propertyWidth);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				//Rotation
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(iconSize));

				//EditorGUILayout.LabelField(new GUIContent(img_Rot), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField(_guiContent_Icon_ModTF_Rot.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconSize));

				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(propertyWidth), apGUILOFactory.I.Height(iconSize));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation), apGUILOFactory.I.Width(propertyWidth));//"Rotation"

				float rootAngle = EditorGUILayout.DelayedFloatField(rootMeshGroupTransform._matrix._angleDeg, apGUILOFactory.I.Width(propertyWidth));
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				//Scaling
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(iconSize));

				//EditorGUILayout.LabelField(new GUIContent(img_Scale), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
				EditorGUILayout.LabelField(_guiContent_Icon_ModTF_Scale.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconSize));

				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(propertyWidth), apGUILOFactory.I.Height(iconSize));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling), apGUILOFactory.I.Width(propertyWidth));//"Scaling"

				//nextScale = EditorGUILayout.Vector2Field("", nextScale, GUILayout.Width(propertyWidth));
				Vector2 rootScale = apEditorUtil.DelayedVector2Field(rootMeshGroupTransform._matrix._scale, propertyWidth);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();

				//테스트용
				//rootMeshGroupTransform._isVisible_Default = EditorGUILayout.Toggle("Is Visible", rootMeshGroupTransform._isVisible_Default, GUILayout.Width(width));
				//EditorGUILayout.ColorField("Color2x", rootMeshGroupTransform._meshColor2X_Default);


				if (rootPos != rootMeshGroupTransform._matrix._pos
					|| rootAngle != rootMeshGroupTransform._matrix._angleDeg
					|| rootScale != rootMeshGroupTransform._matrix._scale)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_DefaultSettingChanged, 
														Editor, 
														MeshGroup, 
														//MeshGroup, 
														false, true,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					rootMeshGroupTransform._matrix.SetTRS(rootPos.x, rootPos.y, rootAngle, rootScale.x, rootScale.y, true);
					MeshGroup.RefreshForce();
					apEditorUtil.ReleaseGUIFocus();
				}
			}
		}



		//본-컨트롤 파라미터 연결 정보를 빨리 참조하기 위한 임시 변수
		private apControlParam _tmpJiggleBoneLinkedControlParam = null;
		private int _tmpJiggleBoneLinkedControlParamID = -1;
		private object _loadKey_SelectControlParamToJiggleBones = null;
		
		/// <summary>
		/// 메시 그룹의 Right 2 UI 중 "Bone" 탭에서의 UI
		/// </summary>
		private void DrawEditor_Right2_MeshGroup_Bone(int width, int height)
		{
			//int subWidth = 250;
			apBone curBone = Bone;

			bool isRefresh = false;
			bool isAnyGUIAction = false;

			//bool isChildBoneChanged = false;

			bool isBoneChanged = (_prevBone_BoneProperty != curBone);
			
			//본 선택 개수가 바뀌었더도 Changed가 True이다.
			int nSelectedBones = _subObjects.NumBone;
			bool isMultipleSelected = nSelectedBones > 1;

			if (_prevBone_BoneProperty != curBone
				|| _prevBone_NumSelected != nSelectedBones //<<추가 20.6.3 : 선택 개수 변경
				)
			{
				_prevBone_BoneProperty = curBone;
				_prevBone_NumSelected = nSelectedBones;

				if (curBone != null)
				{
					//_prevName_BoneProperty = curBone._name;
					_prevChildBoneCount = curBone._childBones.Count;
				}
				else
				{
					//_prevName_BoneProperty = "";
					_prevChildBoneCount = 0;
				}

				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Update_Child_Bones, false);//"Update Child Bones"
			}

			if (curBone != null)
			{
				if (_prevChildBoneCount != curBone._childBones.Count)
				{
					Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Update_Child_Bones, true);//"Update Child Bones"
					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Update_Child_Bones))//"Update Child Bones"
					{
						//Debug.Log("Child Bone Count Changed : " + _prevChildBoneCount + " -> " + curBone._childBones.Count);
						_prevChildBoneCount = curBone._childBones.Count;
					}
				}
			}


			//"MeshGroupRight2 Bone"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight2_Bone_Single, curBone != null && !isMultipleSelected && !isBoneChanged);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight2_Bone_Multiple, isMultipleSelected && !isBoneChanged);

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Bone__Child_Bone_Drawable, true);//"MeshGroup Bone - Child Bone Drawable"

			bool isRender_Single = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight2_Bone_Single);
			bool isRender_Multiple = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupRight2_Bone_Multiple);

			if (!isRender_Single && !isRender_Multiple)
			{
				//둘다 아니면 패스
				return;
			}


			Color prevColor = GUI.backgroundColor;

			if (_strWrapper_64 == null)
			{
				_strWrapper_64 = new apStringWrapper(64);
			}


			//현재 렌더 정보와 값이 다른 경우
			//bool isDummy = (isRender_Single && isMultipleSelected) || (isRender_Multiple && !isMultipleSelected);




			//1. 아이콘 / 타입
			//[단일/다중 공통 - 아이콘이 다르다.]
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(50));
			GUILayout.Space(10);

			
			//변경
			if (_guiContent_Right_MeshGroup_ModIcon == null)
			{
				_guiContent_Right_MeshGroup_ModIcon = new apGUIContentWrapper();
			}
			if (isRender_Single)
			{
				//[단일 선택]
				_guiContent_Right_MeshGroup_ModIcon.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging));
			}
			else
			{
				//[다중 선택[
				_guiContent_Right_MeshGroup_ModIcon.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MultiSelected));
			}

			EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_ModIcon.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(50));

			int nameWidth = width - (50 + 10);
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(nameWidth));
			GUILayout.Space(5);

			if (_guiContent_Right2MeshGroup_ObjectProp_Name == null) { _guiContent_Right2MeshGroup_ObjectProp_Name = new apGUIContentWrapper(); }
			if (_guiContent_Right2MeshGroup_ObjectProp_Type == null) { _guiContent_Right2MeshGroup_ObjectProp_Type = new apGUIContentWrapper(); }

			_guiContent_Right2MeshGroup_ObjectProp_Name.ClearText(false);
			_guiContent_Right2MeshGroup_ObjectProp_Type.ClearText(false);

			if (isRender_Single)
			{
				//[단일 선택]
				//Label - "Bone"
				//Text Box - Name

				_guiContent_Right2MeshGroup_ObjectProp_Type.AppendText(Editor.GetUIWord(UIWORD.Bone), true);

				EditorGUILayout.LabelField(_guiContent_Right2MeshGroup_ObjectProp_Type.Content, apGUILOFactory.I.Width(nameWidth));//"Bone"

				//추가 21.2.9 : 단축키로 포커싱하기 위함
				apEditorUtil.SetNextGUIID(apStringFactory.I.GUI_ID__BoneName);

				string nextBoneName = EditorGUILayout.DelayedTextField(curBone._name, apGUILOFactory.I.Width(nameWidth));
				if (!string.Equals(nextBoneName, curBone._name))
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					curBone._name = nextBoneName;
					isRefresh = true;
					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}

				//이름 바꾸기 단축키 (21.2.9)
				Editor.AddHotKeyEvent(OnHotKeyEvent_RenameBone, apHotKeyMapping.KEY_TYPE.RenameObject, curBone);
			}
			else
			{
				//[다중 선택]
				_guiContent_Right2MeshGroup_ObjectProp_Type.AppendText(Editor.GetUIWord(UIWORD.MultipleSelected), true);//"Multiple Selected"
				_guiContent_Right2MeshGroup_ObjectProp_Name.AppendText(nSelectedBones, false);
				_guiContent_Right2MeshGroup_ObjectProp_Name.AppendSpaceText(1, false);
				_guiContent_Right2MeshGroup_ObjectProp_Name.AppendText(Editor.GetUIWord(UIWORD.Bones), true);

				EditorGUILayout.LabelField(_guiContent_Right2MeshGroup_ObjectProp_Type.Content, apGUILOFactory.I.Width(nameWidth));
				EditorGUILayout.LabelField(_guiContent_Right2MeshGroup_ObjectProp_Name.Content, apGUILOFactory.I.Width(nameWidth));
			}

			


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);

			//2. Default Matrix 설정
			//[공통 - 처리 방식은 다름]
			//"Base Pose Transformation"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.BasePoseTransformation), apGUILOFactory.I.Width(width));
			int widthValue = width - 80;

			if (_strWrapper_64 == null)
			{
				_strWrapper_64 = new apStringWrapper(64);
			}

			Vector2 defPos = Vector2.zero;
			float defAngle = 0.0f;
			Vector2 defScale = Vector2.one;

			if(isRender_Single)
			{
				//[단일]
				defPos = curBone._defaultMatrix._pos;
				defAngle = curBone._defaultMatrix._angleDeg;
				defScale = curBone._defaultMatrix._scale;
			}
			else
			{
				//[다중]
				//Sync_0이 Pos / Sync_1이 Angle / Sync_2가 Scale
				_subObjects.Check_Bone_DefaultMatrix();
				defPos = (_subObjects.Sync0.IsSync ? _subObjects.Sync0.SyncValue_Vec2 : Vector2.zero);
				defAngle = (_subObjects.Sync1.IsSync ? _subObjects.Sync1.SyncValue_Float : 0.0f);
				defScale = (_subObjects.Sync2.IsSync ? _subObjects.Sync2.SyncValue_Vec2 : Vector2.one);
			}

			if (!IsBoneDefaultEditing)
			{
				//여기서는 보여주기만
				_strWrapper_64.Clear();
				_strWrapper_64.Append(defPos.x, false);
				_strWrapper_64.Append(apStringFactory.I.Comma_Space, false);
				_strWrapper_64.Append(defPos.y, true);

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position), apGUILOFactory.I.Width(70));//"Position"
				EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				_strWrapper_64.Clear();
				_strWrapper_64.Append(defAngle, true);

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation), apGUILOFactory.I.Width(70));//"Rotation"
				EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(widthValue));
				EditorGUILayout.EndHorizontal();

				_strWrapper_64.Clear();
				_strWrapper_64.Append(defScale.x, false);
				_strWrapper_64.Append(apStringFactory.I.Comma_Space, false);
				_strWrapper_64.Append(defScale.y, true);

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling), apGUILOFactory.I.Width(70));//"Scaling"
				EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(widthValue));
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				//여기서는 설정이 가능하다

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position), apGUILOFactory.I.Width(70));//"Position"
				Vector2 nextDefPos = apEditorUtil.DelayedVector2Field(defPos, widthValue);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation), apGUILOFactory.I.Width(70));//"Rotation"
				float nextDefAngle = EditorGUILayout.DelayedFloatField(defAngle, apGUILOFactory.I.Width(widthValue + 4));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling), apGUILOFactory.I.Width(70));//"Scaling"
				Vector2 nextDefScale = apEditorUtil.DelayedVector2Field(defScale, widthValue);
				EditorGUILayout.EndHorizontal();

				bool isChanged_PosX = Mathf.Abs(nextDefPos.x - defPos.x) > 0.001f;
				bool isChanged_PosY = Mathf.Abs(nextDefPos.y - defPos.y) > 0.001f;
				bool isChanged_Angle = Mathf.Abs(nextDefAngle - defAngle) > 0.001f;
				bool isChanged_ScaleX = Mathf.Abs(nextDefScale.x - defScale.x) > 0.001f;
				bool isChanged_ScaleY = Mathf.Abs(nextDefScale.y - defScale.y) > 0.001f;

				if (isChanged_PosX || isChanged_PosY || isChanged_Angle || isChanged_ScaleX || isChanged_ScaleY)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					nextDefAngle = apUtil.AngleTo180(nextDefAngle);

					//기본 행렬이 변경되었을 때
					if (!isMultipleSelected)
					{
						//[단일]
						curBone._defaultMatrix.SetPos(nextDefPos, false);
						curBone._defaultMatrix.SetRotate(nextDefAngle, false);
						curBone._defaultMatrix.SetScale(nextDefScale, false);

						curBone.MakeWorldMatrix(true);//<<이때는 IK가 꺼져서 이것만 수정해도 된다.
					}
					else
					{
						//[다중]
						if(isChanged_PosX || isChanged_PosY)
						{	
							_subObjects.Set_Bone_DefaultMatrix_Pos(nextDefPos, isChanged_PosX, isChanged_PosY);//위치 동기화
						}
						if(isChanged_Angle)
						{
							_subObjects.Set_Bone_DefaultMatrix_Angle(nextDefAngle);//각도 동기화
						}
						if(isChanged_ScaleX || isChanged_ScaleY)
						{
							_subObjects.Set_Bone_DefaultMatrix_Scale(nextDefScale, isChanged_ScaleX, isChanged_ScaleY);//크기 동기화
						}

						//WorldMatrix는 아예 맨 위에서 해야한다.
						_meshGroup.UpdateBonesWorldMatrix();
					}

					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}
			}


			GUILayout.Space(10);

			//3. 소켓 설정
			//[공통 - 분기]
			if (isRender_Single)
			{
				//[단일]
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.SocketEnabled), Editor.GetUIWord(UIWORD.SocketDisabled), curBone._isSocketEnabled, true, width, 25))
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					curBone._isSocketEnabled = !curBone._isSocketEnabled;
				}
			}
			else if(isRender_Multiple)
			{
				//[다중]
				_subObjects.Check_Bone_IsSocketEnabled();

				if (apEditorUtil.ToggledButton_2Side_Sync(	Editor.GetUIWord(UIWORD.SocketEnabled), 
															Editor.GetUIWord(UIWORD.SocketDisabled),
															_subObjects.Sync0.SyncValue_Bool,
															_subObjects.Sync0.IsValid,
															_subObjects.Sync0.IsSync, 
															width, 25))
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_subObjects.Set_Bone_IsSocketEnabled();//<토글 방식
				}
			}


			//4. 복제 설정
			//[단일만]
			if (isRender_Single)
			{
				//추가 8.13 : 복제 기능 (오프셋을 입력해야한다.)
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Duplicate), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20)))
				{
					//본 복사하는 다이얼로그를 열자
					_loadKey_DuplicateBone = apDialog_DuplicateBone.ShowDialog(Editor, Bone, OnDuplicateBoneResult);
				}
			}


			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);//------------------------
			GUILayout.Space(10);


			//--------------------------------------
			// [ Shape 설정 ]
			//--------------------------------------
			//[공통 - 분기]
			Color prevBoneColor = Color.black;
			int prevBoneWidth = 0;
			int prevBoneLength = 0;
			int prevBoneTaper = 0;
			bool prevBoneHelper = false;

			if(isRender_Single)
			{
				//[단일]
				prevBoneColor = curBone._color;
				prevBoneWidth = curBone._shapeWidth;
				prevBoneLength = curBone._shapeLength;
				prevBoneTaper = curBone._shapeTaper;
				prevBoneHelper = curBone._shapeHelper;
			}
			else if(isRender_Multiple)
			{
				//[다중]
				_subObjects.Check_Bone_Shape();
				prevBoneColor = _subObjects.Sync0.IsSync ? _subObjects.Sync0.SyncValue_Color : Color.black;
				prevBoneWidth = _subObjects.Sync1.IsSync ? _subObjects.Sync1.SyncValue_Int : 0;
				prevBoneLength = _subObjects.Sync2.IsSync ? _subObjects.Sync2.SyncValue_Int : 0;
				prevBoneTaper = _subObjects.Sync3.IsSync ? _subObjects.Sync3.SyncValue_Int : 0;
				prevBoneHelper = _subObjects.Sync4.IsSync ? _subObjects.Sync4.SyncValue_Bool : false;
			}

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Shape), apGUILOFactory.I.Width(width));//"Shape"

			//본 색상
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Color), apGUILOFactory.I.Width(70));//"Color"
			try
			{
				
				Color nextColor = EditorGUILayout.ColorField(prevBoneColor, apGUILOFactory.I.Width(widthValue));
				if (nextColor != prevBoneColor)
				{
					apEditorUtil.SetEditorDirty();
					if(isRender_Single)
					{
						//[단일]
						curBone._color = nextColor;
					}
					else if(isRender_Multiple)
					{
						//[다중]
						_subObjects.Set_Bone_Shape_Color(nextColor);
					}
				}
			}
			catch (Exception) { }
			EditorGUILayout.EndHorizontal();


			//[단일]
			//추가 20.3.24 : 색상 프리셋을 두어서 빠르게 색상을 지정할 수 있다.
			//6개의 색상 프리셋을 만들자.
			if (isRender_Single)
			{	
				int nPreset = apEditorUtil.BoneColorPresetsCount;
				int width_ColorPresetBtn = ((width - 84) / nPreset) - 2;
				int height_ColorPresetBtn = 12;

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_ColorPresetBtn));
				GUILayout.Space(78);
				Color presetBoneColor = Color.black;
				for (int iPreset = 0; iPreset < nPreset; iPreset++)
				{
					presetBoneColor = apEditorUtil.GetBoneColorPreset(iPreset);
					GUI.backgroundColor = presetBoneColor;
					if (GUILayout.Button(apGUIContentWrapper.Empty.Content, apEditorUtil.WhiteGUIStyle, apGUILOFactory.I.Width(width_ColorPresetBtn), apGUILOFactory.I.Height(height_ColorPresetBtn)))
					{
						//색상을 프리셋에 맞게 바꾸자
						Editor.Controller.ChangeBoneColorWithPreset(MeshGroup, curBone, presetBoneColor);

					}
					GUI.backgroundColor = prevColor;
				}
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(10);

			//Width [공통]
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Width), apGUILOFactory.I.Width(70));//"Width"
			int nextShapeWidth = EditorGUILayout.DelayedIntField(prevBoneWidth, apGUILOFactory.I.Width(widthValue));
			if (nextShapeWidth != prevBoneWidth)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
													Editor, 
													curBone._meshGroup, 
													//curBone, 
													false, false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
				if(isRender_Single)
				{
					//[단일]
					curBone._shapeWidth = nextShapeWidth;
				}
				else if(isRender_Multiple)
				{
					//[다중]
					_subObjects.Set_Bone_Shape_Width(nextShapeWidth);
				}

				//다음 본을 생성시에 지금 값을 이용하도록 값을 저장하자
				_lastBoneShapeWidth = nextShapeWidth;
				_isLastBoneShapeWidthChanged = true;

				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();



			//Length [공통]
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Length), apGUILOFactory.I.Width(70));
			int nextShapeLength = EditorGUILayout.DelayedIntField(prevBoneLength, apGUILOFactory.I.Width(widthValue));
			if (nextShapeLength != prevBoneLength)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
													Editor, 
													curBone._meshGroup, 
													//curBone, 
													false, false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				if(isRender_Single)
				{
					//[단일]
					curBone._shapeLength = nextShapeLength;
				}
				else if(isRender_Multiple)
				{
					//[다중]
					_subObjects.Set_Bone_Shape_Length(nextShapeLength);
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();

			
			
			//Taper [공통]
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Taper), apGUILOFactory.I.Width(70));//"Taper"
			int nextShapeTaper = EditorGUILayout.DelayedIntField(prevBoneTaper, apGUILOFactory.I.Width(widthValue));
			if (nextShapeTaper != prevBoneTaper)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
													Editor, 
													curBone._meshGroup, 
													//curBone, 
													false, false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				if(nextShapeTaper < 0) { nextShapeTaper = 0; }
				else if(nextShapeTaper > 100) { nextShapeTaper = 100; }

				if(isRender_Single)
				{
					//[단일]
					curBone._shapeTaper = nextShapeTaper;
				}
				else if(isRender_Multiple)
				{
					//[다중]
					_subObjects.Set_Bone_Shape_Taper(nextShapeTaper);
				}

				//다음 본을 생성시에 지금 값을 이용하도록 값을 저장하자
				_lastBoneShapeTaper = nextShapeTaper;
				_isLastBoneShapeWidthChanged = true;


				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();



			//Helper [공통]
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Helper), apGUILOFactory.I.Width(70));//"Helper"
			bool nextHelper = EditorGUILayout.Toggle(prevBoneHelper, apGUILOFactory.I.Width(widthValue));
			if (nextHelper != prevBoneHelper)
			{
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
													Editor, 
													curBone._meshGroup, 
													//curBone, 
													false, false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
				if(isRender_Single)
				{
					//[단일]
					curBone._shapeHelper = nextHelper;
				}
				else if(isRender_Multiple)
				{
					//[다중]]
					_subObjects.Set_Bone_Shape_Helper();//<토글
				}
				apEditorUtil.ReleaseGUIFocus();
			}
			EditorGUILayout.EndHorizontal();
			

			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);//------------------------
			GUILayout.Space(10);


			//추가 20.5.23 : 지글본
			//[공통 - 분기]

			//이전 값을 미리 계산하자
			bool jiggle_IsEnabled = false;
			float prevJiggle_Mass = 0.0f;
			float prevJiggle_K = 0.0f;
			float prevJiggle_Drag = 0.0f;
			float prevJiggle_Damping = 0.0f;
			bool jiggle_IsConstraint = false;
			float prevJiggle_AngleMin = 0.0f;
			float prevJiggle_AngleMax = 0.0f;

			//추가 22.7.5 : 가중치 관련 값 (컨트롤 파라미터)
			bool prevJiggleWeightIsLinkControlParam = false;
			int prevJiggleWeightControlParamID = -1;

			if(isRender_Single)
			{
				//[단일]
				prevJiggle_Mass = curBone._jiggle_Mass;
				prevJiggle_K = curBone._jiggle_K;
				prevJiggle_Drag = curBone._jiggle_Drag;
				prevJiggle_Damping = curBone._jiggle_Damping;
				prevJiggle_AngleMin = curBone._jiggle_AngleLimit_Min;
				prevJiggle_AngleMax = curBone._jiggle_AngleLimit_Max;
				prevJiggleWeightIsLinkControlParam = curBone._jiggle_IsControlParamWeight;
				prevJiggleWeightControlParamID = curBone._jiggle_WeightControlParamID;
			}
			else if(isRender_Multiple)
			{
				//[다중]
				_subObjects.Check_Bone_Jiggle();
				prevJiggle_Mass =						_subObjects.Sync1.IsSync ? _subObjects.Sync1.SyncValue_Float : 0.0f;
				prevJiggle_K =							_subObjects.Sync2.IsSync ? _subObjects.Sync2.SyncValue_Float : 0.0f;
				prevJiggle_Drag =						_subObjects.Sync3.IsSync ? _subObjects.Sync3.SyncValue_Float : 0.0f;
				prevJiggle_Damping =					_subObjects.Sync4.IsSync ? _subObjects.Sync4.SyncValue_Float : 0.0f;
				prevJiggle_AngleMin =					_subObjects.Sync6.IsSync ? _subObjects.Sync6.SyncValue_Float : 0.0f;
				prevJiggle_AngleMax =					_subObjects.Sync7.IsSync ? _subObjects.Sync7.SyncValue_Float : 0.0f;
				prevJiggleWeightIsLinkControlParam =	_subObjects.Sync8.IsSync ? _subObjects.Sync8.SyncValue_Bool : false;
				prevJiggleWeightControlParamID =		_subObjects.Sync9.IsSync ? _subObjects.Sync9.SyncValue_Int : -1;
			}

			//이름을 표시하기 위해 컨트롤 파라미터를 참조한다.
			if(prevJiggleWeightControlParamID <= 0)
			{
				_tmpJiggleBoneLinkedControlParam = null;
				_tmpJiggleBoneLinkedControlParamID = -1;
			}
			else
			{
				if(_tmpJiggleBoneLinkedControlParamID != prevJiggleWeightControlParamID)
				{
					_tmpJiggleBoneLinkedControlParam = Editor._portrait._controller.FindParam(prevJiggleWeightControlParamID);
					_tmpJiggleBoneLinkedControlParamID = prevJiggleWeightControlParamID;
				}
			}

			
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.JiggleBone), apGUILOFactory.I.Width(width));//"Jiggle Bone"


			if(_guiContent_Right2MeshGroup_JiggleBone == null)
			{
				_guiContent_Right2MeshGroup_JiggleBone = new apGUIContentWrapper();
			}

			//지글본 설정 : Toggle
			if (isRender_Single)
			{
				_guiContent_Right2MeshGroup_JiggleBone.ClearAll();
				_guiContent_Right2MeshGroup_JiggleBone.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Jiggle));
				if(curBone._isJiggle)
				{	
					_guiContent_Right2MeshGroup_JiggleBone.SetText(2, Editor.GetUIWord(UIWORD.JiggleBoneON));
				}
				else
				{
					_guiContent_Right2MeshGroup_JiggleBone.SetText(2, Editor.GetUIWord(UIWORD.JiggleBoneOFF));
				}

				if (apEditorUtil.ToggledButton_2Side( 	_guiContent_Right2MeshGroup_JiggleBone,
														curBone._isJiggle, true, width, 28))//"Jiggle Bone ON", "Jiggle Bone OFF"
				{
					apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged,
														Editor,
														curBone._meshGroup,
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					curBone._isJiggle = !curBone._isJiggle;
					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}

				jiggle_IsEnabled = curBone._isJiggle;
			}
			else if (isRender_Multiple)
			{
				_guiContent_Right2MeshGroup_JiggleBone.ClearAll();
				_guiContent_Right2MeshGroup_JiggleBone.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Jiggle));
				if(_subObjects.Sync0.SyncValue_Bool || !_subObjects.Sync0.IsSync)
				{	
					_guiContent_Right2MeshGroup_JiggleBone.SetText(2, Editor.GetUIWord(UIWORD.JiggleBoneON));
				}
				else
				{
					_guiContent_Right2MeshGroup_JiggleBone.SetText(2, Editor.GetUIWord(UIWORD.JiggleBoneOFF));
				}
				if (apEditorUtil.ToggledButton_2Side_Sync(	_guiContent_Right2MeshGroup_JiggleBone,
															_subObjects.Sync0.SyncValue_Bool,
															_subObjects.Sync0.IsValid,
															_subObjects.Sync0.IsSync, 
															width, 28))//"Jiggle Bone ON", "Jiggle Bone OFF"
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					_subObjects.Set_Bone_Jiggle_IsEnabled();//Toggle

					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}

				//동기화가 안되었지만 여기서는 true로 해야 다른 설정도 수정할 수 있다.
				jiggle_IsEnabled = _subObjects.Sync0.IsSync ? _subObjects.Sync0.SyncValue_Bool : true;
			}


			if (jiggle_IsEnabled)
			{
				//만약 헬퍼 본이거나 길이가 0이면 동작하지 않으므로 경고를 띄우자 [단일 전용]
				if (isRender_Single)
				{
					if (curBone._shapeHelper || curBone._shapeLength <= 0)
					{
						GUI.backgroundColor = new Color(prevColor.r * 1.0f, prevColor.g * 0.7f, prevColor.b * 0.7f, 1.0f);
						GUILayout.Box(Editor.GetUIWord(UIWORD.JiggleWarning), apGUIStyleWrapper.I.Box_MiddleCenter, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
						GUI.backgroundColor = prevColor;

						GUILayout.Space(5);
					}
				}


				//Jiggle Mass
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Mass), apGUILOFactory.I.Width(70));//"Mass"
				float nexJig_Mass = EditorGUILayout.DelayedFloatField(prevJiggle_Mass, apGUILOFactory.I.Width(widthValue));
				if (Mathf.Abs(nexJig_Mass - prevJiggle_Mass) > 0.0001f)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					if (nexJig_Mass < 0.01f)
					{ nexJig_Mass = 0.01f; }//최소값 적용

					if (isRender_Single)
					{
						//[단일 적용]
						curBone._jiggle_Mass = nexJig_Mass;
					}
					else if (isRender_Multiple)
					{
						//[다중 적용]
						_subObjects.Set_Bone_Jiggle_Mass(nexJig_Mass);
					}

					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}
				EditorGUILayout.EndHorizontal();



				//Jiggle K-Value
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.K_Value), apGUILOFactory.I.Width(70));//"K-Value"
				float nexJig_KValue = EditorGUILayout.DelayedFloatField(prevJiggle_K, apGUILOFactory.I.Width(widthValue));
				if (Mathf.Abs(nexJig_KValue - prevJiggle_K) > 0.001f)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					if (isRender_Single)
					{
						//[단일 적용]
						curBone._jiggle_K = nexJig_KValue;
					}
					else if (isRender_Multiple)
					{
						//[다중 적용]
						_subObjects.Set_Bone_Jiggle_K(nexJig_KValue);
					}

					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}
				EditorGUILayout.EndHorizontal();



				//Jiggle Drag
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AirDrag), apGUILOFactory.I.Width(70));//"Air Drag"
				float nexJig_Drag = EditorGUILayout.DelayedFloatField(prevJiggle_Drag, apGUILOFactory.I.Width(widthValue));
				if (Mathf.Abs(nexJig_Drag - prevJiggle_Drag) > 0.001f)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					nexJig_Drag = Mathf.Clamp01(nexJig_Drag);

					if (isRender_Single)
					{
						//[단일 적용]
						curBone._jiggle_Drag = nexJig_Drag;
					}
					else if (isRender_Multiple)
					{
						//[다중 적용]
						_subObjects.Set_Bone_Jiggle_Drag(nexJig_Drag);
					}

					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}
				EditorGUILayout.EndHorizontal();


				//Jiggle Damping
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Damping), apGUILOFactory.I.Width(70));//"Damping"
				float nexJig_Damping = EditorGUILayout.DelayedFloatField(prevJiggle_Damping, apGUILOFactory.I.Width(widthValue));
				if (Mathf.Abs(nexJig_Damping - prevJiggle_Damping) > 0.001f)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					if (isRender_Single)
					{
						//[단일 적용]
						curBone._jiggle_Damping = nexJig_Damping;
					}
					else if (isRender_Multiple)
					{
						//[다중 적용]
						_subObjects.Set_Bone_Jiggle_Damping(nexJig_Damping);
					}

					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				//추가 22.7.5 : Jiggle Bone Weight (직접 값 적용 또는 컨트롤 파라미터 연결)
				//"Weight by Control Parameter"
				if (apEditorUtil.ToggledButton_2Side_Sync(	Editor.GetUIWord(UIWORD.WeightByControlParameter), 
															Editor.GetUIWord(UIWORD.WeightByControlParameter),
															prevJiggleWeightIsLinkControlParam,
															(isRender_Single ? true : (isRender_Multiple ? _subObjects.Sync8.IsValid : false)),
															(isRender_Single ? true : (isRender_Multiple ? _subObjects.Sync8.IsSync : false)),
															width, 20))
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					if (isRender_Single)
					{
						//[단일 적용]
						curBone._jiggle_IsControlParamWeight = !prevJiggleWeightIsLinkControlParam;
					}
					else if (isRender_Multiple)
					{
						//[다중 적용]
						_subObjects.Set_Bone_Jiggle_WeightLinkControlParam(!prevJiggleWeightIsLinkControlParam);
					}
					isAnyGUIAction = true;
					apEditorUtil.ReleaseGUIFocus();
				}

				if(prevJiggleWeightIsLinkControlParam)
				{
					//컨트롤 파라미터와 연결한다.
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
					GUILayout.Space(5);
					if (_tmpJiggleBoneLinkedControlParam != null)
					{
						if(_strWrapper_64 == null)
						{
							_strWrapper_64 = new apStringWrapper(64);
						}

						_strWrapper_64.Clear();
						_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
						_strWrapper_64.Append(_tmpJiggleBoneLinkedControlParam._keyName, false);
						_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);


						GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
						GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 34), apGUILOFactory.I.Height(25));

						GUI.backgroundColor = prevColor;
					}
					else
					{
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						GUILayout.Box(Editor.GetUIWord(UIWORD.NoControlParam), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 34), apGUILOFactory.I.Height(25));//"No ControlParam"

						GUI.backgroundColor = prevColor;
					}

					if (GUILayout.Button(Editor.GetUIWord(UIWORD.Set), apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(25)))//"Set"
					{
						//Control Param을 선택하는 Dialog를 호출하자
						_loadKey_SelectControlParamToJiggleBones = apDialog_SelectControlParam.ShowDialog(	Editor,
																											apDialog_SelectControlParam.PARAM_TYPE.Float,
																											OnSelectControlParamToJiggleBones,
																											_subObjects.AllBones);
					}
					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(10);

				


				//Jiggle Angle Limit
				//[공통 - 분기]
				if (isRender_Single)
				{
					//[단일]
					if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.ConstraintOn), Editor.GetUIWord(UIWORD.ConstraintOff),
															curBone._isJiggleAngleConstraint, true, width, 20))//"Angle Constraint ON", "Angle Constraint OFF"
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curBone._isJiggleAngleConstraint = !curBone._isJiggleAngleConstraint;
						isAnyGUIAction = true;
						apEditorUtil.ReleaseGUIFocus();
					}

					jiggle_IsConstraint = curBone._isJiggleAngleConstraint;
				}
				else if (isRender_Multiple)
				{
					//[다중]
					if (apEditorUtil.ToggledButton_2Side_Sync(Editor.GetUIWord(UIWORD.ConstraintOn), Editor.GetUIWord(UIWORD.ConstraintOff),
																_subObjects.Sync5.SyncValue_Bool,
																_subObjects.Sync5.IsValid,
																_subObjects.Sync5.IsSync,
																width, 20))//"Angle Constraint ON", "Angle Constraint OFF"
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_subObjects.Set_Bone_Jiggle_IsConstraint();

						isAnyGUIAction = true;
						apEditorUtil.ReleaseGUIFocus();
					}

					jiggle_IsConstraint = _subObjects.Sync5.IsSync ? _subObjects.Sync5.SyncValue_Bool : true;//동기화가 안되어도 True로 설정해야 다른 값을 설정할 수 있다.
				}


				//각도 제한
				if (jiggle_IsConstraint)
				{
					float nextLowerAngle = prevJiggle_AngleMin;
					float nextUpperAngle = prevJiggle_AngleMax;
					EditorGUILayout.MinMaxSlider(ref nextLowerAngle, ref nextUpperAngle, -180, 180, apGUILOFactory.I.Width(width));//<<이거 걍 쓰지 말까

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), apGUILOFactory.I.Width(70));//"Min"
					nextLowerAngle = EditorGUILayout.DelayedFloatField(nextLowerAngle, apGUILOFactory.I.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), apGUILOFactory.I.Width(70));//"Max"
					nextUpperAngle = EditorGUILayout.DelayedFloatField(nextUpperAngle, apGUILOFactory.I.Width(widthValue));
					EditorGUILayout.EndHorizontal();

					if (Mathf.Abs(nextLowerAngle - prevJiggle_AngleMin) > 0.001f)
					{
						//Undo에 저장하지 않는다.
						//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);
						apEditorUtil.SetEditorDirty();

						nextLowerAngle = Mathf.Clamp(nextLowerAngle, -180.0f, 0.0f);
						if (isRender_Single)
						{
							//[단일]
							curBone._jiggle_AngleLimit_Min = nextLowerAngle;
						}
						else if (isRender_Multiple)
						{
							//[다중]
							_subObjects.Set_Bone_Jiggle_AngleMin(nextLowerAngle);
						}

						apEditorUtil.ReleaseGUIFocus();
					}

					if (Mathf.Abs(nextUpperAngle - prevJiggle_AngleMax) > 0.001f)
					{
						//Undo에 저장하지 않는다.
						//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);
						apEditorUtil.SetEditorDirty();
						nextUpperAngle = Mathf.Clamp(nextUpperAngle, 0.0f, 180.0f);

						if (isRender_Single)
						{
							//[단일]
							curBone._jiggle_AngleLimit_Max = nextUpperAngle;
						}
						else if (isRender_Multiple)
						{
							//[다중]
							_subObjects.Set_Bone_Jiggle_AngleMax(nextUpperAngle);
						}

						apEditorUtil.ReleaseGUIFocus();
					}
				}

				GUILayout.Space(5);
				//리셋 버튼 추가
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetValue), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20)))
				{
					//지글본의 값을 ON/OFF를 제외하고 모두 리셋한다. (Constraint도 각도만 리셋)

					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					
					apEditorUtil.ReleaseGUIFocus();//일단 먼저 포커스를 제외

					if (isRender_Single)
					{
						//[단일 적용]
						curBone._jiggle_Mass = apBone.JIGGLE_DEFAULT_MASS;
						curBone._jiggle_K = apBone.JIGGLE_DEFAULT_K;
						curBone._jiggle_Drag = apBone.JIGGLE_DEFAULT_DRAG;
						curBone._jiggle_Damping = apBone.JIGGLE_DEFAULT_DAMPING;
						curBone._jiggle_AngleLimit_Min = apBone.JIGGLE_DEFAULT_ANGLE_MIN;
						curBone._jiggle_AngleLimit_Max = apBone.JIGGLE_DEFAULT_ANGLE_MAX;
					}
					else if (isRender_Multiple)
					{
						//[다중 적용]
						_subObjects.Set_Bone_Jiggle_Mass(apBone.JIGGLE_DEFAULT_MASS);
						_subObjects.Set_Bone_Jiggle_K(apBone.JIGGLE_DEFAULT_K);
						_subObjects.Set_Bone_Jiggle_Drag(apBone.JIGGLE_DEFAULT_DRAG);
						_subObjects.Set_Bone_Jiggle_Damping(apBone.JIGGLE_DEFAULT_DAMPING);
						_subObjects.Set_Bone_Jiggle_AngleMin(apBone.JIGGLE_DEFAULT_ANGLE_MIN);
						_subObjects.Set_Bone_Jiggle_AngleMax(apBone.JIGGLE_DEFAULT_ANGLE_MAX);
					}

					isAnyGUIAction = true;
				}

				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.Test), Editor.GetUIWord(UIWORD.Test),
														false, !IsBoneDefaultEditing, width, 25))
				{
					//테스트 기능
					//랜덤하게 모든 본의 지글 각도를 변경한다.
					MeshGroup.SetBoneJiggleTest();
					apEditorUtil.ReleaseGUIFocus();//일단 먼저 포커스를 제외
				}
			}



			// [ IK 설정 ]
			//------------------------------
			//[단일만]
			if (isRender_Single)
			{

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);//------------------------
				GUILayout.Space(10);


				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKSetting), apGUILOFactory.I.Width(width));//"IK Setting"

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40));
				int IKModeBtnSize = (width / 4) - 4;
				//EditorGUILayout.LabelField("IK Option", GUILayout.Width(70));
				GUILayout.Space(5);
				apBone.OPTION_IK nextOptionIK = curBone._optionIK;

				//apBone.OPTION_IK nextOptionIK = (apBone.OPTION_IK)EditorGUILayout.EnumPopup(curBone._optionIK, GUILayout.Width(widthValue));

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKSingle), curBone._optionIK == apBone.OPTION_IK.IKSingle, true, IKModeBtnSize, 40, apStringFactory.I.IKSingle))//"IK Single"
				{
					nextOptionIK = apBone.OPTION_IK.IKSingle;
					isAnyGUIAction = true;
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKHead), curBone._optionIK == apBone.OPTION_IK.IKHead, true, IKModeBtnSize, 40, apStringFactory.I.IKHead))//"IK Head"
				{
					nextOptionIK = apBone.OPTION_IK.IKHead;
					isAnyGUIAction = true;
				}
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKChained), curBone._optionIK == apBone.OPTION_IK.IKChained, curBone._optionIK == apBone.OPTION_IK.IKChained, IKModeBtnSize, 40, apStringFactory.I.IKChain))//"IK Chain"
				{
					//nextOptionIK = apBone.OPTION_IK.IKSingle;//Chained는 직접 설정할 수 있는게 아니다.
					isAnyGUIAction = true;
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_IKDisabled), curBone._optionIK == apBone.OPTION_IK.Disabled, true, IKModeBtnSize, 40, apStringFactory.I.Disabled))//"Disabled"
				{
					nextOptionIK = apBone.OPTION_IK.Disabled;
					isAnyGUIAction = true;
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				

				Color boxColor = Color.black;
				switch (curBone._optionIK)
				{
					case apBone.OPTION_IK.IKSingle:
						boxColor = new Color(1.0f, 0.6f, 0.5f, 1.0f);
						break;
					case apBone.OPTION_IK.IKHead:
						boxColor = new Color(1.0f, 0.5f, 0.6f, 1.0f);
						break;
					case apBone.OPTION_IK.IKChained:
						boxColor = new Color(0.7f, 0.5f, 1.0f, 1.0f);
						break;
					case apBone.OPTION_IK.Disabled:
						boxColor = new Color(0.6f, 0.8f, 1.0f, 1.0f);
						break;
				}
				GUI.backgroundColor = boxColor;

				switch (curBone._optionIK)
				{
					case apBone.OPTION_IK.IKSingle:
						GUILayout.Box(Editor.GetUIWord(UIWORD.IKInfo_Single), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40));
						break;
					case apBone.OPTION_IK.IKHead:
						GUILayout.Box(Editor.GetUIWord(UIWORD.IKInfo_Head), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40));
						break;
					case apBone.OPTION_IK.IKChained:
						GUILayout.Box(Editor.GetUIWord(UIWORD.IKInfo_Chain), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40));
						break;
					case apBone.OPTION_IK.Disabled:
						GUILayout.Box(Editor.GetUIWord(UIWORD.IKInfo_Disabled), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40));
						break;
				}


				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);


				if (nextOptionIK != curBone._optionIK)
				{
					//Debug.Log("IK Change : " + curBone._optionIK + " > " + nextOptionIK);

					bool isIKOptionChangeValid = false;


					//이제 IK 옵션에 맞는지 체크해주자
					if (curBone._optionIK == apBone.OPTION_IK.IKChained)
					{
						//Chained 상태에서는 아예 바꿀 수 없다.
						//EditorUtility.DisplayDialog("IK Option Information",
						//	"<IK Chained> setting has been forced.\nTo Change, change the IK setting in the <IK Header>.",
						//	"Close");

						EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
														Editor.GetText(TEXT.IKOption_Body_Chained),
														Editor.GetText(TEXT.Close));
					}
					else
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						//그외에는 변경이 가능하다
						switch (nextOptionIK)
						{
							case apBone.OPTION_IK.Disabled:
								//끄는 건 쉽다.
								isIKOptionChangeValid = true;
								break;

							case apBone.OPTION_IK.IKChained:
								//IK Chained는 직접 할 수 있는게 아니다.
								//EditorUtility.DisplayDialog("IK Option Information",
								//"<IK Chained> setting is set automatically.\nTo change, change the setting in the <IK Header>.",
								//"Close");

								EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
													Editor.GetText(TEXT.IKOption_Body_Chained),
													Editor.GetText(TEXT.Close));
								break;

							case apBone.OPTION_IK.IKHead:
								{
									//자식으로 연결된게 없으면 일단 바로 아래 자식을 연결하자.
									//자식이 없으면 실패

									apBone nextChainedBone = curBone._IKNextChainedBone;
									apBone targetBone = curBone._IKTargetBone;

									bool isRefreshNeed = true;
									if (nextChainedBone != null && targetBone != null)
									{
										//이전에 연결된 값이 존재하고, 재귀적인 연결도 유효한 경우는 패스
										if (curBone.GetChildBone(nextChainedBone._uniqueID) != null
											&& curBone.GetChildBoneRecursive(targetBone._uniqueID) != null)
										{
											//유효한 설정이다.
											isRefreshNeed = false;
										}
									}

									if (isRefreshNeed)
									{
										//자식 Bone의 하나를 연결하자
										if (curBone._childBones.Count > 0)
										{
											curBone._IKNextChainedBone = curBone._childBones[0];
											curBone._IKTargetBone = curBone._childBones[0];

											curBone._IKNextChainedBoneID = curBone._IKNextChainedBone._uniqueID;
											curBone._IKTargetBoneID = curBone._IKTargetBone._uniqueID;

											isIKOptionChangeValid = true;//기본값을 넣어서 변경 가능
										}
										else
										{
											//EditorUtility.DisplayDialog("IK Option Information",
											//"<IK Head> setting requires one or more child Bones.",
											//"Close");

											EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
														Editor.GetText(TEXT.IKOption_Body_Head),
														Editor.GetText(TEXT.Close));
										}
									}
									else
									{
										isIKOptionChangeValid = true;
									}
								}
								break;

							case apBone.OPTION_IK.IKSingle:
								{
									//IK Target과 NextChained가 다르면 일단 그것부터 같게 하자.
									//나머지는 Head와 동일
									curBone._IKTargetBone = curBone._IKNextChainedBone;
									curBone._IKTargetBoneID = curBone._IKNextChainedBoneID;

									apBone nextChainedBone = curBone._IKNextChainedBone;

									bool isRefreshNeed = true;
									if (nextChainedBone != null)
									{
										//이전에 연결된 값이 존재하고, 재귀적인 연결도 유효한 경우는 패스
										if (curBone.GetChildBone(nextChainedBone._uniqueID) != null)
										{
											//유효한 설정이다.
											isRefreshNeed = false;
										}
									}

									if (isRefreshNeed)
									{
										//자식 Bone의 하나를 연결하자
										if (curBone._childBones.Count > 0)
										{
											curBone._IKNextChainedBone = curBone._childBones[0];
											curBone._IKTargetBone = curBone._childBones[0];

											curBone._IKNextChainedBoneID = curBone._IKNextChainedBone._uniqueID;
											curBone._IKTargetBoneID = curBone._IKTargetBone._uniqueID;

											isIKOptionChangeValid = true;//기본값을 넣어서 변경 가능
										}
										else
										{
											//EditorUtility.DisplayDialog("IK Option Information",
											//"<IK Single> setting requires a child Bone.",
											//"Close");

											EditorUtility.DisplayDialog(Editor.GetText(TEXT.IKOption_Title),
														Editor.GetText(TEXT.IKOption_Body_Single),
														Editor.GetText(TEXT.Close));
										}
									}
									else
									{
										isIKOptionChangeValid = true;
									}
								}
								break;
						}
					}



					if (isIKOptionChangeValid)
					{
						curBone._optionIK = nextOptionIK;

						isRefresh = true;
					}
					//TODO : 너무 자동으로 Bone Chain을 하는것 같다;
					//옵션이 적용이 안된다;
				}

				//추가
				if (_guiContent_Right_MeshGroup_RiggingIconAndText == null)
				{
					_guiContent_Right_MeshGroup_RiggingIconAndText = new apGUIContentWrapper();
					_guiContent_Right_MeshGroup_RiggingIconAndText.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging));
				}



				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKHeader), apGUILOFactory.I.Width(width));//"IK Header"

				//string headerBoneName = "<None>";//이전
				string headerBoneName = apEditorUtil.Text_NoneName;//변경

				if (curBone._IKHeaderBone != null)
				{
					headerBoneName = curBone._IKHeaderBone._name;
				}

				//이전
				//EditorGUILayout.LabelField(new GUIContent(" " + headerBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));

				//변경
				_guiContent_Right_MeshGroup_RiggingIconAndText.SetText(1, headerBoneName);
				EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_RiggingIconAndText.Content, apGUILOFactory.I.Width(width));

				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKNextChainToTarget), apGUILOFactory.I.Width(width));//"IK Next Chain To Target"

				//string nextChainedBoneName = "<None>";//이전
				string nextChainedBoneName = apEditorUtil.Text_NoneName;//변경

				if (curBone._IKNextChainedBone != null)
				{
					nextChainedBoneName = curBone._IKNextChainedBone._name;
				}

				//이전
				//EditorGUILayout.LabelField(new GUIContent(" " + nextChainedBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));

				//변경
				_guiContent_Right_MeshGroup_RiggingIconAndText.SetText(1, nextChainedBoneName);
				EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_RiggingIconAndText.Content, apGUILOFactory.I.Width(width));
				GUILayout.Space(5);


				if (curBone._optionIK != apBone.OPTION_IK.Disabled)
				{
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKTarget), apGUILOFactory.I.Width(width));//"IK Target"

					apBone targetBone = curBone._IKTargetBone;

					//string targetBoneName = "<None>";//이전
					string targetBoneName = apEditorUtil.Text_NoneName;//변경

					if (targetBone != null)
					{
						targetBoneName = targetBone._name;
					}

					//이전
					//EditorGUILayout.LabelField(new GUIContent(" " + targetBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width));

					//변경
					_guiContent_Right_MeshGroup_RiggingIconAndText.SetText(1, targetBoneName);
					EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_RiggingIconAndText.Content, apGUILOFactory.I.Width(width));

					//Target을 설정하자.
					if (curBone._optionIK == apBone.OPTION_IK.IKHead)
					{
						//"Change IK Target"
						if (GUILayout.Button(Editor.GetUIWord(UIWORD.ChangeIKTarget), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20)))
						{
							//Debug.LogError("TODO : IK Target을 Dialog를 열어서 설정하자.");
							_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKTarget, OnDialogSelectBone);
							isAnyGUIAction = true;
						}
					}



					GUILayout.Space(15);
					//"IK Angle Constraint"
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKAngleConstraint), apGUILOFactory.I.Width(width));

					//"Constraint On", "Constraint Off"
					if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.ConstraintOn), Editor.GetUIWord(UIWORD.ConstraintOff), curBone._isIKAngleRange, true, width, 25))
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curBone._isIKAngleRange = !curBone._isIKAngleRange;
						isAnyGUIAction = true;
					}

					if (curBone._isIKAngleRange)
					{
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Range), apGUILOFactory.I.Width(70));//"Range"

						//변경전 Lower : -180 ~ 0, Uppder : 0 ~ 180
						//변경후 Lower : -360 ~ 360, Upper : -360 ~ 360 (크기만 맞춘다.)
						float nextLowerAngle = curBone._IKAngleRange_Lower;
						float nextUpperAngle = curBone._IKAngleRange_Upper;
						//EditorGUILayout.MinMaxSlider(ref nextLowerAngle, ref nextUpperAngle, -360, 360, GUILayout.Width(widthValue));
						EditorGUILayout.MinMaxSlider(ref nextLowerAngle, ref nextUpperAngle, -360, 360, apGUILOFactory.I.Width(width));

						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Min), apGUILOFactory.I.Width(70));//"Min"
						nextLowerAngle = EditorGUILayout.DelayedFloatField(nextLowerAngle, apGUILOFactory.I.Width(widthValue));
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Max), apGUILOFactory.I.Width(70));//"Max"
						nextUpperAngle = EditorGUILayout.DelayedFloatField(nextUpperAngle, apGUILOFactory.I.Width(widthValue));
						EditorGUILayout.EndHorizontal();

						//EditorGUILayout.EndHorizontal();


						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Preferred), apGUILOFactory.I.Width(70));//"Preferred"
						float nextPreferredAngle = EditorGUILayout.DelayedFloatField(curBone._IKAnglePreferred, apGUILOFactory.I.Width(widthValue));//<<정밀한 작업을 위해서 변경
						EditorGUILayout.EndHorizontal();

						if (nextLowerAngle != curBone._IKAngleRange_Lower ||
							nextUpperAngle != curBone._IKAngleRange_Upper ||
							nextPreferredAngle != curBone._IKAnglePreferred)
						{

							//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, curBone._meshGroup, curBone, false, false);
							apEditorUtil.SetEditorDirty();

							nextLowerAngle = Mathf.Clamp(nextLowerAngle, -360, 360);
							nextUpperAngle = Mathf.Clamp(nextUpperAngle, -360, 360);
							nextPreferredAngle = Mathf.Clamp(nextPreferredAngle, -360, 360);

							if (nextLowerAngle > nextUpperAngle)
							{
								float tmp = nextLowerAngle;
								nextLowerAngle = nextUpperAngle;
								nextUpperAngle = tmp;
							}

							curBone._IKAngleRange_Lower = nextLowerAngle;
							curBone._IKAngleRange_Upper = nextUpperAngle;
							curBone._IKAnglePreferred = nextPreferredAngle;
							//isRefresh = true;
							isAnyGUIAction = true;

							apEditorUtil.ReleaseGUIFocus();
						}
					}
				}


				//추가 5.8 IK Controller (Position / LookAt)
				GUILayout.Space(20);

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKConSettings), apGUILOFactory.I.Width(width));//"IK Controller Settings"
																												  //GUILayout.Space(10);
				apBoneIKController.CONTROLLER_TYPE nextIKControllerType = (apBoneIKController.CONTROLLER_TYPE)EditorGUILayout.EnumPopup(curBone._IKController._controllerType, apGUILOFactory.I.Width(width));
				if (nextIKControllerType != curBone._IKController._controllerType)
				{
					apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, 
														Editor, 
														curBone._meshGroup, 
														//curBone, 
														false, false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					curBone._IKController._controllerType = nextIKControllerType;
					apEditorUtil.ReleaseGUIFocus();
				}

#region [미사용 코드]


				//if (curBone._positionController._isEnabled)
				//{
				//	//Position Controller가 켜져 있다면
				//	//- Effector
				//	//- Default Mix Weight
				//	GUILayout.Space(5);
				//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				//	EditorGUILayout.LabelField("Default FK/IK Weight", GUILayout.Width(width - 60));//"Default FK/IK Weight"
				//	float nextPosMixWeight = EditorGUILayout.DelayedFloatField(curBone._positionController._defaultMixWeight, GUILayout.Width(58));
				//	if(nextPosMixWeight != curBone._positionController._defaultMixWeight)
				//	{
				//		apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, Editor, curBone._meshGroup, curBone, false, false);
				//		curBone._positionController._defaultMixWeight = Mathf.Clamp01(nextPosMixWeight);
				//		apEditorUtil.ReleaseGUIFocus();
				//	}

				//	EditorGUILayout.EndHorizontal();


				//	GUILayout.Space(5);

				//	EditorGUILayout.LabelField("Effector Bone", GUILayout.Width(width));//"Effector Bone"
				//	string posEffectorBoneName = "<None>";
				//	if (curBone._positionController._effectorBone != null)
				//	{
				//		posEffectorBoneName = curBone._positionController._effectorBone._name;
				//	}
				//	EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
				//	EditorGUILayout.LabelField(new GUIContent(" " + posEffectorBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));
				//	if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), GUILayout.Width(58)))//"Change"
				//	{
				//		isAnyGUIAction = true;
				//		_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(	Editor, curBone, curBone._meshGroup, 
				//																	apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKPositionControllerEffector, 
				//																	OnDialogSelectBone);
				//	}
				//	EditorGUILayout.EndHorizontal();

				//	GUILayout.Space(5);


				//	//TODO : Undo
				//} 
#endregion

				if (curBone._IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None)
				{
					//Position / LookAt 공통 설정
					//- Default Mix Weight
					//- Effector
					GUILayout.Space(5);

					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKConEffectorBone), apGUILOFactory.I.Width(width));//"Effector Bone"

					//string lookAtEffectorBoneName = "<None>";//이전
					string lookAtEffectorBoneName = apEditorUtil.Text_NoneName;//변경

					if (curBone._IKController._effectorBone != null)
					{
						lookAtEffectorBoneName = curBone._IKController._effectorBone._name;
					}
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

					//이전
					//EditorGUILayout.LabelField(new GUIContent(" " + lookAtEffectorBoneName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));

					//변경
					_guiContent_Right_MeshGroup_RiggingIconAndText.SetText(1, lookAtEffectorBoneName);
					EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_RiggingIconAndText.Content, apGUILOFactory.I.Width(width - 60));

					if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), apGUILOFactory.I.Width(58)))//"Change"
					{
						isAnyGUIAction = true;

						_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup,
																					(curBone._IKController._controllerType == apBoneIKController.CONTROLLER_TYPE.Position ?
																						apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKPositionControllerEffector :
																						apDialog_SelectLinkedBone.REQUEST_TYPE.SelectIKLookAtControllerEffector),
																					OnDialogSelectBone);
					}
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(5);

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IKConDefaultWeight), apGUILOFactory.I.Width(width - 60));//"Default FK/IK Weight"
					float nextLookAtMixWeight = EditorGUILayout.DelayedFloatField(curBone._IKController._defaultMixWeight, apGUILOFactory.I.Width(58));
					if (nextLookAtMixWeight != curBone._IKController._defaultMixWeight)
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curBone._IKController._defaultMixWeight = Mathf.Clamp01(nextLookAtMixWeight);
						apEditorUtil.ReleaseGUIFocus();
					}
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(5);

					//Control Parameter에 의해서 제어
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ControlParameter), apGUILOFactory.I.Width(width - 60));//"Default FK/IK Weight"
					bool nextUseControlParam = EditorGUILayout.Toggle(curBone._IKController._isWeightByControlParam, apGUILOFactory.I.Width(58));
					if (nextUseControlParam != curBone._IKController._isWeightByControlParam)
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curBone._IKController._isWeightByControlParam = nextUseControlParam;
						apEditorUtil.ReleaseGUIFocus();
					}
					EditorGUILayout.EndHorizontal();

					//추가
					if (_guiContent_Right_MeshGroup_ParamIconAndText == null)
					{
						_guiContent_Right_MeshGroup_ParamIconAndText = new apGUIContentWrapper();
						_guiContent_Right_MeshGroup_ParamIconAndText.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param));
					}



					if (curBone._IKController._isWeightByControlParam)
					{
						//Control Param 선택하기
						//string controlParamName = "<None>";//이전
						string controlParamName = apEditorUtil.Text_NoneName;//변경

						if (curBone._IKController._weightControlParam != null)
						{
							controlParamName = curBone._IKController._weightControlParam._keyName;
						}
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

						//이전
						//EditorGUILayout.LabelField(new GUIContent(" " + controlParamName, Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param)), GUILayout.Width(width - 60));

						//변경
						_guiContent_Right_MeshGroup_ParamIconAndText.SetText(1, controlParamName);
						EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_ParamIconAndText.Content, apGUILOFactory.I.Width(width - 60));

						if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), apGUILOFactory.I.Width(58)))//"Change"
						{
							isAnyGUIAction = true;
							//Control Param 선택 다이얼로그
							_loadKey_SelectControlParamForIKController = apDialog_SelectControlParam.ShowDialog(
																					Editor,
																					apDialog_SelectControlParam.PARAM_TYPE.Float,
																					OnSelectControlParamForIKController,
																					curBone);
						}
						EditorGUILayout.EndHorizontal();
					}


					GUILayout.Space(5);
				}

			}
			
			

			// [ Hierarchy 설정 ]
			//------------------------------
			//[단일만]
			if (isRender_Single)
			{
				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);//------------------------
				GUILayout.Space(10);


				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Hierarchy), apGUILOFactory.I.Width(width));//"Hierarchy"
																											  //Parent와 Child List를 보여주자.
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ParentBone), apGUILOFactory.I.Width(width));//"Parent Bone"

				//string parentName = "<None>";//이전
				string parentName = apEditorUtil.Text_NoneName;//변경

				if (curBone._parentBone != null)
				{
					parentName = curBone._parentBone._name;
				}
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

				//이전
				//EditorGUILayout.LabelField(new GUIContent(" " + parentName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));

				//변경
				_guiContent_Right_MeshGroup_RiggingIconAndText.SetText(1, parentName);
				EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_RiggingIconAndText.Content, apGUILOFactory.I.Width(width - 60));

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), apGUILOFactory.I.Width(58)))//"Change"
				{
					//Debug.LogError("TODO : Change Parent Dialog 구현할 것");
					isAnyGUIAction = true;
					_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.ChangeParent, OnDialogSelectBone);
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				int nChildList = curBone._childBones.Count;
				if (_prevChildBoneCount != nChildList)
				{
					Debug.Log("AnyPortrait : Count is not matched : " + _prevChildBoneCount + " > " + nChildList);
				}

				//"Children Bones"

				_strWrapper_64.Clear();
				_strWrapper_64.Append(Editor.GetUIWord(UIWORD.ChildrenBones), false);
				_strWrapper_64.AppendSpace(1, false);
				_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
				_strWrapper_64.Append(nChildList, false);
				_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

				//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ChildrenBones) + " [" + nChildList + "]", apGUILOFactory.I.Width(width));
				EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(width));

				//Detach가 
				apBone detachedBone = null;

				for (int iChild = 0; iChild < _prevChildBoneCount; iChild++)
				{
					if (iChild >= nChildList)
					{
						//리스트를 벗어났다.
						//더미 Layout을 그리자
						//유니티 레이아웃 처리방식때문..
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
						EditorGUILayout.LabelField(apStringFactory.I.None, apGUILOFactory.I.Width(width - 60));
						if (GUILayout.Button(Editor.GetUIWord(UIWORD.Detach), apGUILOFactory.I.Width(58)))//"Detach"
						{

						}
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						apBone childBone = curBone._childBones[iChild];
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

						//이전
						//EditorGUILayout.LabelField(new GUIContent(" " + childBone._name, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));

						//변경
						_guiContent_Right_MeshGroup_RiggingIconAndText.SetText(1, childBone._name);
						EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_RiggingIconAndText.Content, apGUILOFactory.I.Width(width - 60));

						if (GUILayout.Button(Editor.GetUIWord(UIWORD.Detach), apGUILOFactory.I.Width(58)))//"Detach"
						{
							//bool isResult = EditorUtility.DisplayDialog("Detach Child Bone", "Detach Bone? [" + childBone._name + "]", "Detach", "Cancel")
							bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.DetachChildBone_Title),
																			Editor.GetTextFormat(TEXT.DetachChildBone_Body, childBone._name),
																			Editor.GetText(TEXT.Detach_Ok),
																			Editor.GetText(TEXT.Cancel)
																			);

							if (isResult)
							{
								//Debug.LogError("TODO : Detach Child Bone 구현할 것");
								//Detach Child Bone 선택
								detachedBone = childBone;
								isAnyGUIAction = true;
							}
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.AttachChildBone), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20)))//"Attach Child Bone"
				{
					isAnyGUIAction = true;
					_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.AttachChild, OnDialogSelectBone);
				}

				GUILayout.Space(2);

				//추가 8.13 : 자식 본을 향하도록 만들기
				bool isSnapAvailable = (Bone._childBones != null) && (Bone._childBones.Count > 0);

				//"Snap to Child Bone
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.SnapToChildBone), Editor.GetUIWord(UIWORD.SnapToChildBone), false, isSnapAvailable, width, 20))
				{
					if (Bone._childBones != null)
					{
						if (Bone._childBones.Count == 1)
						{
							//자식이 1개라면
							//바로 함수 호출
							Editor.Controller.SnapBoneEndToChildBone(Bone, Bone._childBones[0], MeshGroup);
						}
						else
						{
							//자식이 여러개라면 
							//선택 다이얼로그 호출
							_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, Bone, MeshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.Select1LevelChildToSnap, OnDialogSelectBone);
						}
					}

				}


				//Detach 요청이 있으면 수행 후 Refresh를 하자
				if (detachedBone != null)
				{
					isAnyGUIAction = true;
					Editor.Controller.DetachBoneFromChild(curBone, detachedBone);
					Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroup_Bone__Child_Bone_Drawable, false);//"MeshGroup Bone - Child Bone Drawable"
					isRefresh = true;
				}
			}


			



			// [ Mirror Bone ]
			//------------------------------
			//[단일만]
			if (isRender_Single)
			{
				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MirrorBone), apGUILOFactory.I.Width(width));//"Mirror Bone"

				//string mirrorName = "<None>";//이전
				string mirrorName = apEditorUtil.Text_NoneName;//변경

				if (curBone._mirrorBone != null)
				{
					mirrorName = curBone._mirrorBone._name;
				}
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

				//이전
				//EditorGUILayout.LabelField(new GUIContent(" " + mirrorName, Editor.ImageSet.Get(apImageSet.PRESET.Modifier_Rigging)), GUILayout.Width(width - 60));

				//변경
				_guiContent_Right_MeshGroup_RiggingIconAndText.SetText(1, mirrorName);
				EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_RiggingIconAndText.Content, apGUILOFactory.I.Width(width - 60));

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Change), apGUILOFactory.I.Width(58)))//"Change"
				{
					isAnyGUIAction = true;
					_loadKey_SelectBone = apDialog_SelectLinkedBone.ShowDialog(Editor, curBone, curBone._meshGroup, apDialog_SelectLinkedBone.REQUEST_TYPE.Mirror, OnDialogSelectBone);
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				//Mirror Option 중 Offset과 Axis는 루트 본만 적용된다.
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Bone_Mirror_Axis_Option_Visible, curBone._parentBone == null);//"Bone Mirror Axis Option Visible"
				if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Bone_Mirror_Axis_Option_Visible))//"Bone Mirror Axis Option Visible"
				{
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Axis), apGUILOFactory.I.Width(70));//"Axis"
					apBone.MIRROR_OPTION nextMirrorOption = (apBone.MIRROR_OPTION)EditorGUILayout.EnumPopup(curBone._mirrorOption, apGUILOFactory.I.Width(widthValue));
					if (nextMirrorOption != curBone._mirrorOption)
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curBone._mirrorOption = nextMirrorOption;
					}
					EditorGUILayout.EndHorizontal();


					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Offset), apGUILOFactory.I.Width(70));//"Offset"
					float nextMirrorCenterOffset = EditorGUILayout.DelayedFloatField(curBone._mirrorCenterOffset, apGUILOFactory.I.Width(widthValue));
					if (nextMirrorCenterOffset != curBone._mirrorCenterOffset)
					{
						apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
															Editor, 
															curBone._meshGroup, 
															//curBone, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						curBone._mirrorCenterOffset = nextMirrorCenterOffset;
						apEditorUtil.ReleaseGUIFocus();
					}
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(5);
				}

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.MakeNewMirrorBone), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))//"Make a New Mirror Bone"
				{
					//-Mirror 본 생성
					//-이름에 " L " <-> " R " 전환
					//-팝업으로 Children 포함할지 물어보기
					Editor.Controller.MakeNewMirrorBone(MeshGroup, curBone);
				}

				
			}





			// [ 본 삭제 ]
			//[단일 / 다중 (분기)]
			//------------------------------

			if (isRender_Single || isRender_Multiple)
			{
				GUILayout.Space(20);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(20);	

				if (_guiContent_Right_MeshGroup_RemoveBone == null)
				{
					_guiContent_Right_MeshGroup_RemoveBone = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveBone), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
				}


				//본 삭제 단축키 (Delete)
				//Editor.AddHotKeyEvent(OnHotKeyEvent_RemoveBone, apHotKey.LabelText.RemoveBone, KeyCode.Delete, false, false, false, Bone);
				Editor.AddHotKeyEvent(OnHotKeyEvent_RemoveBone, apHotKeyMapping.KEY_TYPE.RemoveObject, Bone);//변경 20.12.3


				if (GUILayout.Button(_guiContent_Right_MeshGroup_RemoveBone.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(24)))
				{
					isAnyGUIAction = true;
					SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);

					int btnIndex = 2;
					if(isRender_Single)
					{
						//단일 삭제
						string strRemoveBoneText = Editor.Controller.GetRemoveItemMessage(
																		_portrait,
																		curBone,
																		5,
																		Editor.GetTextFormat(TEXT.RemoveBone_Body, curBone._name),
																		Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																		);

						btnIndex = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.RemoveBone_Title),
																		strRemoveBoneText,
																		Editor.GetText(TEXT.Remove),
																		Editor.GetText(TEXT.RemoveBone_RemoveAllChildren),
																		Editor.GetText(TEXT.Cancel));
					}
					else
					{
						//다중 삭제
						btnIndex = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.RemoveBone_Title),
																		Editor.GetTextFormat(TEXT.DLG_RemoveBone_Multiple_Body, nSelectedBones),
																		Editor.GetText(TEXT.Remove),
																		Editor.GetText(TEXT.RemoveBone_RemoveAllChildren),
																		Editor.GetText(TEXT.Cancel));
						
					}
					
					if (btnIndex == 0)
					{
						//Bone을 삭제한다.
						if(isRender_Single)
						{
							Editor.Controller.RemoveBone(curBone, false);
						}
						else if(isRender_Multiple && nSelectedBones > 0)
						{
							//추가 20.9.15 : 다중 삭제
							Editor.Controller.RemoveBones(_subObjects.AllBones, MeshGroup, false);
						}
					}
					else if (btnIndex == 1)
					{
						//Bone과 자식을 모두 삭제한다.
						if(isRender_Single)
						{
							Editor.Controller.RemoveBone(curBone, true);
						}
						else if(isRender_Multiple && nSelectedBones > 0)
						{
							//추가 20.9.15 : 다중 삭제 : AllGizmoBones를 이용해야한다.
							Editor.Controller.RemoveBones(_subObjects.AllGizmoBones, MeshGroup, true);
						}
						
					}
				}
			}
			if (isAnyGUIAction)
			{
				//여기서 뭔가 처리를 했으면 Select 모드로 강제된다.
				if (_boneEditMode != BONE_EDIT_MODE.SelectAndTRS)
				{
					SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, true);
				}
			}

			if (isRefresh)
			{
				Editor.RefreshControllerAndHierarchy(false);
				Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_AllModifiers(MeshGroup));
			}
		}


		//지글본의 가중치를 결정하는 컨트롤 파라미터 연결
		public void OnSelectControlParamToJiggleBones(bool isSuccess, object loadKey, apControlParam resultControlParam, object savedObject)
		{
			if (_loadKey_SelectControlParamToJiggleBones != loadKey 
				|| !isSuccess
				|| savedObject == null)
			{
				_loadKey_SelectControlParamToJiggleBones = null;
				return;
			}

			_loadKey_SelectControlParamToJiggleBones = null;
			
			bool isBoneSaved = savedObject is List<apBone>;
			if(!isBoneSaved)
			{
				return;
			}

			List<apBone> bones = savedObject as List<apBone>;
			int nBones = bones != null ? bones.Count : 0;
			if(nBones == 0)
			{
				return;
			}

			//저장된 본들의 지글본 ID를 변경한다.
			int resultControlParamID = resultControlParam != null ? resultControlParam._uniqueID : -1;

			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, 
															Editor,
															bones[0]._meshGroup, 
															false, false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

			apBone curBone = null; 
			for (int i = 0; i < nBones; i++)
			{
				curBone = bones[i];
				curBone._jiggle_WeightControlParamID = resultControlParamID;
				curBone._linkedJiggleControlParam = resultControlParam;
			}

		}

		



		/// <summary>
		/// 메시 그룹의 Right 2 UI 중 "Modifier" 탭에서의 UI
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawEditor_Right2_MeshGroup_Modifier(int width, int height)
		{
			if (Modifier != null)
			{
				//1-1. 선택된 객체가 존재하여 [객체 정보]를 출력할 수 있다.
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupBottom_Modifier, true);//"MeshGroupBottom_Modifier"
			}
			else
			{
				//1-2. 선택된 객체가 없어서 하단 UI를 출력하지 않는다.
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupBottom_Modifier, false);//"MeshGroupBottom_Modifier"

				return; //바로 리턴
			}

			//2. 출력할 정보가 있다 하더라도
			//=> 바로 출력 가능한게 아니라 경우에 따라 Hide 상태를 조금 더 유지할 필요가 있다.
			if (!Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.MeshGroupBottom_Modifier))//"MeshGroupBottom_Modifier"
			{
				//아직 출력하면 안된다.
				return;
			}
			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(50));
			GUILayout.Space(10);

			//모디파이어 아이콘
			//이전
			//EditorGUILayout.LabelField(
			//	new GUIContent(Editor.ImageSet.Get(apEditorUtil.GetModifierIconType(Modifier.ModifierType))),
			//	GUILayout.Width(50), GUILayout.Height(50));

			//변경
			if(_guiContent_Right_MeshGroup_ModIcon == null)
			{
				_guiContent_Right_MeshGroup_ModIcon = new apGUIContentWrapper();
			}
			_guiContent_Right_MeshGroup_ModIcon.SetImage(Editor.ImageSet.Get(apEditorUtil.GetModifierIconType(Modifier.ModifierType)));
			EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_ModIcon.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(50));


			//아이콘 옆의 모디파이어 이름과 레이어 (Label)
			//> 변경 20.3.29 : 원래 하단에 나왔어야 할 Blend 방식과 Weight를 "레이어(Label)" 항목 대신 넣는다.
			int headerRightWidth = width - (50 + 10);
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(headerRightWidth));
			GUILayout.Space(5);

			//모디파이어 이름
			EditorGUILayout.LabelField(Modifier.DisplayName, apGUILOFactory.I.Width(headerRightWidth));


			//블렌드 방식과 Weight (별도의 설명 없이)
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(headerRightWidth));

			//블렌드 방식
			apModifierBase.BLEND_METHOD blendMethod = (apModifierBase.BLEND_METHOD)EditorGUILayout.EnumPopup(Modifier._blendMethod, apGUILOFactory.I.Width(headerRightWidth - (50)));
			if (blendMethod != Modifier._blendMethod)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
													Editor, 
													Modifier, 
													//Modifier, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				Modifier._blendMethod = blendMethod;
			}

			//블렌드 가중치
			float layerWeight = EditorGUILayout.DelayedFloatField(Modifier._layerWeight, apGUILOFactory.I.Width(44));

			layerWeight = Mathf.Clamp01(layerWeight);
			if (layerWeight != Modifier._layerWeight)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
													Editor, 
													Modifier, 
													//Modifier, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				Modifier._layerWeight = layerWeight;
			}
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			if(_strWrapper_64 == null)
			{
				_strWrapper_64 = new apStringWrapper(64);
			}

			//레이어 이동 (Up/Down)
			//GUILayout.Space(5);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerUp), apGUILOFactory.I.Width(width / 2 - 5), apGUILOFactory.I.Height(16)))//"Layer Up"
			{
				Editor.Controller.LayerChange(Modifier, true);
			}
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerDown), apGUILOFactory.I.Width(width / 2 - 5), apGUILOFactory.I.Height(16)))//"Layer Down"
			{
				Editor.Controller.LayerChange(Modifier, false);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			
			//추가
			//만약 색상 옵션이 있는 경우 설정을 하자
			if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//" Color Option On", " Color Option Off"
				if (Modifier.ModifierType != apModifierBase.MODIFIER_TYPE.ColorOnly
					&& Modifier.ModifierType != apModifierBase.MODIFIER_TYPE.AnimatedColorOnly)
				{
					//모디파이어가 ColorOnly 옵션이 아니라면 Color Option 설정 가능.
					//그 ColorOnly에서는 항상 이 옵션이 켜져있다.
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
															1, Editor.GetUIWord(UIWORD.ColorOptionOn),
															Editor.GetUIWord(UIWORD.ColorOptionOff),
															Modifier._isColorPropertyEnabled, true,
															width, 24
														))
					{
						//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
						bool isExecutable = Editor.CheckModalAndExecutable();

						if (isExecutable)
						{
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
																Editor,
																Modifier,
																//Modifier, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							Modifier._isColorPropertyEnabled = !Modifier._isColorPropertyEnabled;
							Editor.RefreshControllerAndHierarchy(false);
						}
					}
				}
				//추가 : Color Option이 있는 경우 Extra 설정도 가능하다.
				//Extra Option On / Off
				if(apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ExtraOption),
													1, Editor.GetUIWord(UIWORD.ExtraOptionON), 
													Editor.GetUIWord(UIWORD.ExtraOptionOFF), 
													Modifier._isExtraPropertyEnabled, true, width, 20))
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
															Editor,
															Modifier,
															//Modifier, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						Modifier._isExtraPropertyEnabled = !Modifier._isExtraPropertyEnabled;

						_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(MeshGroup, Modifier));//<<이거 다시 연결해줘야 한다.

						Editor.RefreshControllerAndHierarchy(false);
					}
				}

				//구분선
				GUILayout.Space(5);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(5);
			}

			//3. 각 프로퍼티 렌더링
			// 수정
			//일괄적으로 호출하자
			DrawModifierPropertyGUI(width, height);
			
			GUILayout.Space(20);


			//4. Modifier 삭제
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(20);

			//"  Remove Modifier"

			//이전
			//if (GUILayout.Button(	new GUIContent(	"  " + Editor.GetUIWord(UIWORD.RemoveModifier),
			//										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
			//										),
			//						GUILayout.Height(24)))

			
			//변경
			if (_guiContent_Right_MeshGroup_RemoveModifier == null)
			{
				_guiContent_Right_MeshGroup_RemoveModifier = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveModifier), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}

			if (GUILayout.Button(	_guiContent_Right_MeshGroup_RemoveModifier.Content, apGUILOFactory.I.Height(24)))
			{
				//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					string strRemoveModifierText = Editor.Controller.GetRemoveItemMessage(
																	_portrait,
																	Modifier,
																	5,
																	Editor.GetTextFormat(TEXT.RemoveModifier_Body, Modifier.DisplayName),
																	Editor.GetText(TEXT.DLG_RemoveItemChangedWarning)
																	);

					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveModifier_Title),
																	//Editor.GetTextFormat(TEXT.RemoveModifier_Body, Modifier.DisplayName),
																	strRemoveModifierText,
																	Editor.GetText(TEXT.Remove),
																	Editor.GetText(TEXT.Cancel)
																	);

					if (isResult)
					{
						Editor.Controller.RemoveModifier(Modifier);
					}
				}
			}


			//삭제 직후라면 출력 에러가 발생한다.
			if (Modifier == null)
			{
				return;
			}

		}


		/// <summary>
		/// 메시 그룹 Right 2 UI의 "Modifeir 탭"의 UI 중 선택된 모디파이어의 속성에 따른 UI 그리기
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawModifierPropertyGUI(int width, int height)
		{
			if (Modifier != null)
			{
				string strRecordName = Modifier.DisplayName;


				if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
				{
					//Rigging UI를 작성
					DrawModifierPropertyGUI_Rigging(width, height, strRecordName);
				}
				else if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
				{
					//Physic UI를 작성
					DrawModifierPropertyGUI_Physics(width, height);
				}
				else
				{
					//그 외에는 ParamSetGroup에 따라서 UI를 구성하면 된다.
					switch (Modifier.SyncTarget)
					{
						case apModifierParamSetGroup.SYNC_TARGET.Bones:
							break;

						case apModifierParamSetGroup.SYNC_TARGET.Controller:
							{
								//Control Param 리스트
								apDialog_SelectControlParam.PARAM_TYPE paramFilter = apDialog_SelectControlParam.PARAM_TYPE.All;
								DrawModifierPropertyGUI_ControllerParamSet(width, height, paramFilter, strRecordName);
							}
							break;

						case apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey:
							break;

						case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
							{
								//Keyframe 리스트
								DrawModifierPropertyGUI_KeyframeParamSet(width, height, strRecordName);
							}
							break;

						case apModifierParamSetGroup.SYNC_TARGET.Static:
							break;
					}
				}

			}
		}


		// 메시 그룹 Right 2 UI > Modifier 탭 > 속성 UI들 <모디파이어 특성별>
		//---------------------------------------------------------
		/// <summary>
		/// 메시 그룹 Right 2 UI > Modifier 탭 > 속성 UI [컨트롤 파라미터에 의한 모디파이어]
		/// </summary>
		private void DrawModifierPropertyGUI_ControllerParamSet(int width, int height, apDialog_SelectControlParam.PARAM_TYPE paramFilter, string recordName)
		{
			
			// SyncTarget으로 Control Param을 받아서 Modifier를 제어하는 경우
			
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ControlParameters), apGUILOFactory.I.Width(width));//"Control Parameters"

			GUILayout.Space(5);


			// 생성된 Morph Key (Parameter Group)를 선택하자
			//------------------------------------------------------------------
			// Control Param에 따른 Param Set Group 리스트
			//------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(120));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 120), apStringFactory.I.None);
			GUI.backgroundColor = prevColor;

			//처리 역순으로 보여준다.
			List<apModifierParamSetGroup> paramSetGroups = new List<apModifierParamSetGroup>();
			int nParamSetGroups = Modifier._paramSetGroup_controller != null ? Modifier._paramSetGroup_controller.Count : 0;
			if (nParamSetGroups > 0)
			{
				for (int i = nParamSetGroups - 1; i >= 0; i--)
				{
					paramSetGroups.Add(Modifier._paramSetGroup_controller[i]);
				}
			}

			nParamSetGroups = paramSetGroups.Count;

			//등록된 Control Param Group 리스트를 출력하자
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(120));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(scrollWidth), apGUILOFactory.I.Height(120));
			GUILayout.Space(3);

			//Texture2D paramIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);
			Texture2D visibleIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Visible_Current);
			Texture2D nonvisibleIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_NonVisible_Current);

			//현재 선택중인 파라미터 그룹
			apModifierParamSetGroup selectedParamSetGroup = SubEditedParamSetGroup;


			//추가
			if (_guiContent_Modifier_ParamSetItem == null)
			{
				_guiContent_Modifier_ParamSetItem = new apGUIContentWrapper();
				_guiContent_Modifier_ParamSetItem.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param));
			}

			GUIStyle curGUIStyle = null;//최적화된 코드

			apModifierParamSetGroup curPSG = null;
			apControlParam curControlParam = null;
			Texture2D img_ControlParamIcon = null;

			for (int i = 0; i < paramSetGroups.Count; i++)
			{
				curPSG = paramSetGroups[i];
				curControlParam = curPSG._keyControlParam;

				img_ControlParamIcon = _editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(curControlParam._iconPreset));

				if (selectedParamSetGroup == curPSG)
				{
					lastRect = GUILayoutUtility.GetLastRect();

					//if (EditorGUIUtility.isProSkin)
					//{
					//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
					//}


					int offsetHeight = 18 + 3;
					if (i == 0)
					{
						offsetHeight = 1 + 3;
					}

					//GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), apStringFactory.I.None);
					//GUI.backgroundColor = prevColor;


					//변경 v1.4.2
					apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + offsetHeight, scrollWidth + 35 - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);


					curGUIStyle = apGUIStyleWrapper.I.None_White2Cyan;
				}
				else
				{
					curGUIStyle = apGUIStyleWrapper.I.None_LabelColor;
				}

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(scrollWidth - 5));
				GUILayout.Space(5);
				
				_guiContent_Modifier_ParamSetItem.SetImage(img_ControlParamIcon);//컨트롤 파라미터의 프리셋 아이콘이 나타난다.
				_guiContent_Modifier_ParamSetItem.SetText(1, curControlParam._keyName);
				if (GUILayout.Button(_guiContent_Modifier_ParamSetItem.Content, curGUIStyle, apGUILOFactory.I.Width(scrollWidth - (5 + 25)), apGUILOFactory.I.Height(20)))
				{
					if (selectedParamSetGroup != curPSG)
					{	
						//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
						bool isExecutable = Editor.CheckModalAndExecutable();

						if (isExecutable)
						{
							//ParamSetGroup을 선택했다.
							SelectParamSetGroupOfModifier(curPSG);
							AutoSelectParamSetOfModifier();//<자동 선택까지

							Editor.RefreshControllerAndHierarchy(false);
						}
					}
				}

				Texture2D imageVisible = visibleIconImage;

				if (!curPSG._isEnabled)
				{
					imageVisible = nonvisibleIconImage;
				}
				if (GUILayout.Button(imageVisible, apGUIStyleWrapper.I.None_LabelColor, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20)))
				{
					curPSG._isEnabled = !curPSG._isEnabled;
				}
				EditorGUILayout.EndHorizontal();
			}


			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			//------------------------------------------------------------------ < Param Set Group 리스트
			//추가 3.22
			//이전
			//if(GUILayout.Button(new GUIContent("  Add Control Parameter", Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_AddTransform)), GUILayout.Height(25)))

			//변경
			if (_guiContent_Modifier_AddControlParameter == null)
			{
				_guiContent_Modifier_AddControlParameter = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AddControlParameter), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_AddTransform));
			}
			
			
			if(GUILayout.Button(_guiContent_Modifier_AddControlParameter.Content, apGUILOFactory.I.Height(25)))
			{
				//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					//ParamSetGroup에 추가되지 않은 컨트롤 파라미터를 추가하자
					List<apControlParam> addableControlParams = new List<apControlParam>();
					List<apControlParam> totalControlParams = _portrait._controller._controlParams;
					for (int i = 0; i < totalControlParams.Count; i++)
					{
						//paramSetGroup에 등록 안된 컨트롤 파라미터를 추가
						apControlParam curParam = totalControlParams[i];
						bool isAlreadyRegistered = paramSetGroups.Exists(delegate (apModifierParamSetGroup a)
						{
							return curParam == a._keyControlParam;
						});
						if (!isAlreadyRegistered)
						{
							addableControlParams.Add(curParam);
						}
					}
					_loadKey_AddControlParam = apDialog_SelectControlParam.ShowDialogWithList(
																		Editor,
																		addableControlParams,
																		OnAddControlParameterToModifierAsParamSetGroup,
																		Modifier);//<<SaveObject로는 모디파이어를 입력
				}
			}


			//-----------------------------------------------------------------------------------
			// Param Set Group 선택시 / 선택된 Param Set Group 정보와 포함된 Param Set 리스트
			//-----------------------------------------------------------------------------------



			GUILayout.Space(10);

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.CP_Selected_ParamSetGroup, (selectedParamSetGroup != null));//"CP Selected ParamSetGroup"

			if (!Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.CP_Selected_ParamSetGroup))//"CP Selected ParamSetGroup"
			{
				return;
			}
			//ParamSetGroup에 레이어 옵션이 추가되었다.
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.SetOfKeys));//"Parameters Setting" -> "Set of Keys"
			GUILayout.Space(2);
			//"Blend Method" -> "Blend"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Blend));
			apModifierParamSetGroup.BLEND_METHOD psgBlendMethod = (apModifierParamSetGroup.BLEND_METHOD)EditorGUILayout.EnumPopup(selectedParamSetGroup._blendMethod, apGUILOFactory.I.Width(width));
			if (psgBlendMethod != selectedParamSetGroup._blendMethod)
			{
				//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
														Editor,
														Modifier,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					selectedParamSetGroup._blendMethod = psgBlendMethod;
				}
			}

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Weight), apGUILOFactory.I.Width(80));//"Weight"
			float psgLayerWeight = EditorGUILayout.Slider(selectedParamSetGroup._layerWeight, 0.0f, 1.0f, apGUILOFactory.I.Width(width - 85));
			if (psgLayerWeight != selectedParamSetGroup._layerWeight)
			{
				//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
														Editor,
														Modifier,
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					selectedParamSetGroup._layerWeight = psgLayerWeight;
				}
			}

			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerUp), apGUILOFactory.I.Width(width / 2 - 2)))//"Layer Up"
			{
				Modifier.ChangeParamSetGroupLayerIndex(selectedParamSetGroup, selectedParamSetGroup._layerIndex + 1);
			}
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.LayerDown), apGUILOFactory.I.Width(width / 2 - 2)))//"Layer Down"
			{
				Modifier.ChangeParamSetGroupLayerIndex(selectedParamSetGroup, selectedParamSetGroup._layerIndex - 1);
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);


			if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
			{
				//색상 옵션을 넣어주자
				//단, Color Only 모디파이어에서는 항상 true이므로 옵션이 필요없다.
				if (Modifier.ModifierType != apModifierBase.MODIFIER_TYPE.ColorOnly)
				{
					//" Color Option On", " Color Option Off"
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
														1, Editor.GetUIWord(UIWORD.ColorOptionOn), Editor.GetUIWord(UIWORD.ColorOptionOff),
														selectedParamSetGroup._isColorPropertyEnabled, true,
														width, 24))
					{
						//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
						bool isExecutable = Editor.CheckModalAndExecutable();

						if (isExecutable)
						{
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
																Editor,
																Modifier,
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							selectedParamSetGroup._isColorPropertyEnabled = !selectedParamSetGroup._isColorPropertyEnabled;
							Editor.RefreshControllerAndHierarchy(false);
						}
					}
				}
				

				//추가 20.02.22 : Show Hide 토글 기능이 추가되었다.
				//TODO : 텍스트 번역 필요
				//"Toggle Visibility without blending"
				if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.ToggleVisibilityWOBlending),
													selectedParamSetGroup._isToggleShowHideWithoutBlend, selectedParamSetGroup._isColorPropertyEnabled,
													width, 22))
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
															Editor,
															Modifier,
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						selectedParamSetGroup._isToggleShowHideWithoutBlend = !selectedParamSetGroup._isToggleShowHideWithoutBlend;
						Editor.RefreshControllerAndHierarchy(false);
					}
				}
				GUILayout.Space(5);
			}


			//추가 21.9.1 [1.3.5]
			//TF 계열에서 키 보간시 회전값을 어떻게 보간할 지에 대한 옵션
			if ((int)(Modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0)
			{
				
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(24));

				GUILayout.Space(4);

				Texture2D imgRotationIcon = null;
				if (selectedParamSetGroup._tfRotationLerpMethod == apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.Default)
				{
					imgRotationIcon = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RotationByAngles);
				}
				else
				{
					imgRotationIcon = Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RotationByVectors);
				}

				if (apEditorUtil.ToggledButton_2Side(	imgRotationIcon, 1,
														//"Rotating by Vectors", "Rotating by Angles",
														Editor.GetUIWord(UIWORD.RotationByVector),
														Editor.GetUIWord(UIWORD.RotationByAngle),
														selectedParamSetGroup._tfRotationLerpMethod == apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.RotationByVector,
														true, width - 28, 24, "Option to determine whether to compute interpolation of rotations using vector arithmetic."))
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SettingChanged,
															Editor,
															Modifier,
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						if (selectedParamSetGroup._tfRotationLerpMethod == apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.Default)
						{
							selectedParamSetGroup._tfRotationLerpMethod = apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.RotationByVector;
						}
						else
						{
							selectedParamSetGroup._tfRotationLerpMethod = apModifierParamSetGroup.TF_ROTATION_LERP_METHOD.Default;
						}
					}
				}

				//EditorGUILayout.EndVertical();

				//모디파이어 회전 잠금
				if (apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_180Lock),
														Editor.ImageSet.Get(apImageSet.PRESET.Anim_180Unlock), 
														Editor._isModRotation180Lock,
														true, 24, 24))
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						Editor._isModRotation180Lock = !Editor._isModRotation180Lock;
					}
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);
			}



			//변경
			bool isSingleModMeshSelected = ModMesh_Main != null && ModMeshes_All != null && ModMeshes_All.Count == 1;
			bool isSingleModBoneSelected = ModBone_Main != null && ModBones_All != null && ModBones_All.Count == 1 && Modifier.IsTarget_Bone;

			//추가 : 다중 복사도 가능
			bool isAnyModMeshSelected = ModMeshes_All != null && ModMeshes_All.Count > 0;
			bool isAnyModBoneSelected = ModBones_All != null && ModBones_All.Count > 0 && Modifier.IsTarget_Bone;

			//단일 대상으로 복사하는 것인가 (21.6.25)
			//단일 대상으로 복사하는 경우와 다중 복사인 경우 데이터 슬롯이 다르다.
			bool isCopyPasteSingleTarget = true;
			if((isAnyModMeshSelected && !isSingleModMeshSelected) || (isAnyModBoneSelected && !isSingleModBoneSelected))
			{
				//다중 복사
				isCopyPasteSingleTarget = false;
			}

			
			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);
			
			
			
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			// 복사/붙여넣기/리셋 UI
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

			//복사 가능한가
			//변경 21.3.19
			bool isModPastable_0 = false;
			bool isModPastable_1 = false;
			bool isModPastable_2 = false;
			bool isModPastable_3 = false;


			if((int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0)
			{
				//Morph 계열 모디파이어 인 경우 (Mesh만 처리한다)
				if (isSingleModMeshSelected)//붙여넣기 가능한지 여부는 Main만 체크한다.
				{
					isModPastable_0 = apSnapShotManager.I.IsPastable_Morph_SingleTarget(ModMesh_Main, 0);
					isModPastable_1 = apSnapShotManager.I.IsPastable_Morph_SingleTarget(ModMesh_Main, 1);
					isModPastable_2 = apSnapShotManager.I.IsPastable_Morph_SingleTarget(ModMesh_Main, 2);
					isModPastable_3 = apSnapShotManager.I.IsPastable_Morph_SingleTarget(ModMesh_Main, 3);
				}
				else if(isAnyModMeshSelected)
				{
					//모두 체크해서 하나라도 붙여넣을 수 있는지 판단한다.
					isModPastable_0 = apSnapShotManager.I.IsPastable_Morph_MultipleTargets(ModMeshes_All, 0);
					isModPastable_1 = apSnapShotManager.I.IsPastable_Morph_MultipleTargets(ModMeshes_All, 1);
					isModPastable_2 = apSnapShotManager.I.IsPastable_Morph_MultipleTargets(ModMeshes_All, 2);
					isModPastable_3 = apSnapShotManager.I.IsPastable_Morph_MultipleTargets(ModMeshes_All, 3);
				}
			}
			else
			{
				//그 외의 경우 (Mesh, Bone을 모두 처리한다)
				if(isSingleModMeshSelected)
				{
					//한개의 ModMesh를 대상으로
					isModPastable_0 = apSnapShotManager.I.IsPastable_TF_SingleTarget(ModMesh_Main, 0);
					isModPastable_1 = apSnapShotManager.I.IsPastable_TF_SingleTarget(ModMesh_Main, 1);
					isModPastable_2 = apSnapShotManager.I.IsPastable_TF_SingleTarget(ModMesh_Main, 2);
					isModPastable_3 = apSnapShotManager.I.IsPastable_TF_SingleTarget(ModMesh_Main, 3);
				}
				else if(isAnyModMeshSelected)
				{
					//여러개의 ModMesh
					isModPastable_0 = apSnapShotManager.I.IsPastable_TF_MultipleTargets(ModMeshes_All, 0);
					isModPastable_1 = apSnapShotManager.I.IsPastable_TF_MultipleTargets(ModMeshes_All, 1);
					isModPastable_2 = apSnapShotManager.I.IsPastable_TF_MultipleTargets(ModMeshes_All, 2);
					isModPastable_3 = apSnapShotManager.I.IsPastable_TF_MultipleTargets(ModMeshes_All, 3);
				}

				if (isSingleModBoneSelected)
				{
					//한개의 ModBone
					if(!isModPastable_0) { isModPastable_0 = apSnapShotManager.I.IsPastable_SingleModBone(ModBone_Main, 0); }
					if(!isModPastable_1) { isModPastable_1 = apSnapShotManager.I.IsPastable_SingleModBone(ModBone_Main, 1); }
					if(!isModPastable_2) { isModPastable_2 = apSnapShotManager.I.IsPastable_SingleModBone(ModBone_Main, 2); }
					if(!isModPastable_3) { isModPastable_3 = apSnapShotManager.I.IsPastable_SingleModBone(ModBone_Main, 3); }
				}
				else if(isAnyModBoneSelected)
				{
					//여러개의 ModBone
					if(!isModPastable_0) { isModPastable_0 = apSnapShotManager.I.IsPastable_MultipleModBones(ModBones_All, 0); }
					if(!isModPastable_1) { isModPastable_1 = apSnapShotManager.I.IsPastable_MultipleModBones(ModBones_All, 1); }
					if(!isModPastable_2) { isModPastable_2 = apSnapShotManager.I.IsPastable_MultipleModBones(ModBones_All, 2); }
					if(!isModPastable_3) { isModPastable_3 = apSnapShotManager.I.IsPastable_MultipleModBones(ModBones_All, 3); }
				}
			}
			
			

			//변경 21.3.18 : 슬롯 개념으로 바꾼다. 슬롯 4개에 저장한 후, 각각 합산하여 붙여넣을 수 있다.
			int height_SlotBtn = 15;
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_SlotBtn));

			GUILayout.Space(4);

			int width_MultiPasteMethod = 80;
			int width_PasteTargetMode = 16;
			int width_SlotBtn = (width - (width_MultiPasteMethod + width_PasteTargetMode + 6)) / 4 - 3;


			//현재 복붙 상태를 알려주자
			if(_guiContent_CopyTargetIcon == null)
			{
				_guiContent_CopyTargetIcon = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.CopyPaste_SingleTarget));
			}
			_guiContent_CopyTargetIcon.SetImage(isCopyPasteSingleTarget ? Editor.ImageSet.Get(apImageSet.PRESET.CopyPaste_SingleTarget) : Editor.ImageSet.Get(apImageSet.PRESET.CopyPaste_MultiTarget));
			
			EditorGUILayout.LabelField(_guiContent_CopyTargetIcon.Content, apGUIStyleWrapper.I.Label_LowerCenter_Margin0,
										apGUILOFactory.I.Width(width_PasteTargetMode), apGUILOFactory.I.Height(height_SlotBtn));



			//Clipboard 이름을 툴팁으로 지정 설정
			
#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			bool isPasteSlotSelected = false;
			int selectedPasteSlot = -1;

			//4개의 슬롯 버튼
			if (apEditorUtil.ToggledButton_3Side_Ctrl(	isModPastable_0 ? apStringFactory.I.Symbol_FilledCircle : apStringFactory.I.Symbol_EmptyCircle, 
														(_iPasteSlot_Main == 0) ? 1 : (_isPasteSlotSelected[0] && isModPastable_0 ? 2 : 0), true,
														width_SlotBtn, height_SlotBtn, 
														//isSingleModMeshSelected ? apSnapShotManager.I.GetClipboardName_ModMesh(0) : (isSingleModBoneSelected ? apSnapShotManager.I.GetClipboardName_ModBone(0) : null),
														null,//삭제 21.6.24
														(_iPasteSlot_Main != 0 && isModPastable_0) ? Event.current.control : false,
														(_iPasteSlot_Main != 0 && isModPastable_0) ? Event.current.command : false
														))
			{
				isPasteSlotSelected = true;
				selectedPasteSlot = 0;
			}
			if(apEditorUtil.ToggledButton_3Side_Ctrl(	isModPastable_1 ? apStringFactory.I.Symbol_FilledCircle : apStringFactory.I.Symbol_EmptyCircle, 
														(_iPasteSlot_Main == 1) ? 1 : (_isPasteSlotSelected[1] && isModPastable_1 ? 2 : 0), true,
														width_SlotBtn, height_SlotBtn, 
														//isSingleModMeshSelected ? apSnapShotManager.I.GetClipboardName_ModMesh(1) : (isSingleModBoneSelected ? apSnapShotManager.I.GetClipboardName_ModBone(1) : null),
														null,//삭제 21.6.24
														(_iPasteSlot_Main != 1 && isModPastable_1) ? Event.current.control : false,
														(_iPasteSlot_Main != 1 && isModPastable_1) ? Event.current.command : false
														))
			{
				isPasteSlotSelected = true;
				selectedPasteSlot = 1;
			}
			if(apEditorUtil.ToggledButton_3Side_Ctrl(	isModPastable_2 ? apStringFactory.I.Symbol_FilledCircle : apStringFactory.I.Symbol_EmptyCircle, 
														(_iPasteSlot_Main == 2) ? 1 : (_isPasteSlotSelected[2] && isModPastable_2 ? 2 : 0), true,
														width_SlotBtn, height_SlotBtn, 
														//isSingleModMeshSelected ? apSnapShotManager.I.GetClipboardName_ModMesh(2) : (isSingleModBoneSelected ? apSnapShotManager.I.GetClipboardName_ModBone(2) : null),
														null,//삭제 21.6.24
														(_iPasteSlot_Main != 2 && isModPastable_2) ? Event.current.control : false,
														(_iPasteSlot_Main != 2 && isModPastable_2) ? Event.current.command : false
														))
			{
				isPasteSlotSelected = true;
				selectedPasteSlot = 2;
			}
			if(apEditorUtil.ToggledButton_3Side_Ctrl(	isModPastable_3 ? apStringFactory.I.Symbol_FilledCircle : apStringFactory.I.Symbol_EmptyCircle, 
														(_iPasteSlot_Main == 3) ? 1 : (_isPasteSlotSelected[3] && isModPastable_3 ? 2 : 0), true,
														width_SlotBtn, height_SlotBtn, 
														//isSingleModMeshSelected ? apSnapShotManager.I.GetClipboardName_ModMesh(3) : (isSingleModBoneSelected ? apSnapShotManager.I.GetClipboardName_ModBone(3) : null),n
														null,//삭제 21/6/24
														(_iPasteSlot_Main != 3 && isModPastable_3) ? Event.current.control : false,
														(_iPasteSlot_Main != 3 && isModPastable_3) ? Event.current.command : false
														))
			{
				isPasteSlotSelected = true;
				selectedPasteSlot = 3;
			}

			if (isPasteSlotSelected)
			{
				if (isCtrl)
				{
					//Ctrl로 선택시 : 메인이 아니라면 선택 토글, 메인이라면 무시
					if(_iPasteSlot_Main != selectedPasteSlot)
					{
						_isPasteSlotSelected[selectedPasteSlot] = !_isPasteSlotSelected[selectedPasteSlot];
						if(_isPasteSlotSelected[selectedPasteSlot])
						{
							//만약 True가 될때 > Pastable이 아니면 false로 강제
							switch (selectedPasteSlot)
							{
								case 0:	if(!isModPastable_0) { _isPasteSlotSelected[selectedPasteSlot] = false; } break;
								case 1: if(!isModPastable_1) { _isPasteSlotSelected[selectedPasteSlot] = false; } break;
								case 2: if(!isModPastable_2) { _isPasteSlotSelected[selectedPasteSlot] = false; } break;
								case 3: if(!isModPastable_3) { _isPasteSlotSelected[selectedPasteSlot] = false; } break;
							}
						}
					}
					else
					{
						_isPasteSlotSelected[selectedPasteSlot] = true;
					}
				}
				else
				{
					//그냥 선택시 : 나머지 선택 모두 해제
					_iPasteSlot_Main = selectedPasteSlot;
					for (int i = 0; i < NUM_PASTE_SLOTS; i++)
					{
						_isPasteSlotSelected[i] = false;
					}
					_isPasteSlotSelected[_iPasteSlot_Main] = true;

				}
			}

			//선택된 복붙 슬롯의 개수를 확인한다.
			//메인의 경우 서브로 선택 안되어도 카운트드된다.
			int nSelectedPastableSlots = 0;
			for (int i = 0; i < NUM_PASTE_SLOTS; i++)
			{
				bool isPastable = false;
				switch (i)
				{
					case 0: isPastable = isModPastable_0; break;
					case 1: isPastable = isModPastable_1; break;
					case 2: isPastable = isModPastable_2; break;
					case 3: isPastable = isModPastable_3; break;
				}
				if(!isPastable)
				{
					continue;
				}

				if (_iPasteSlot_Main == i)
				{
					_isPasteSlotSelected[i] = true;
					nSelectedPastableSlots++;
				}
				else if(_isPasteSlotSelected[i])
				{
					nSelectedPastableSlots++;
				}
			}

			if(nSelectedPastableSlots > 1)
			{
				//붙여넣기 가능한 슬롯이 다중 선택된 경우
				_iMultiPasteSlotMethod = EditorGUILayout.Popup(_iMultiPasteSlotMethod, _multiPasteSlotNames_MultiSelected, apGUILOFactory.I.Width(width_MultiPasteMethod), apGUILOFactory.I.Height(height_SlotBtn));
			}
			else
			{
				//붙여넣기 가능한 슬롯이 한개 혹은 선택 안된 경우 (없을 수도 있다)
				GUI.backgroundColor = new Color(GUI.backgroundColor.r * 0.5f, GUI.backgroundColor.g * 0.5f, GUI.backgroundColor.b * 0.5f, 1.0f);
				
				EditorGUILayout.Popup(0, _multiPasteSlotNames_NotMultiSelected, apGUILOFactory.I.Width(width_MultiPasteMethod), apGUILOFactory.I.Height(height_SlotBtn));
				GUI.backgroundColor = prevColor;
			}


			EditorGUILayout.EndHorizontal();

			

			//추가
			//선택된 키가 있다면 => Copy / Paste / Reset 버튼을 만든다.
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));

			//" Copy"
			if(_guiContent_CopyTextIcon == null)
			{
				_guiContent_CopyTextIcon = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Copy), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy));
			}
			if(_guiContent_PasteTextIcon == null)
			{
				_guiContent_PasteTextIcon = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Paste), Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste));
			}
			

			//복사 버튼
			if (GUILayout.Button(_guiContent_CopyTextIcon.Content, apGUILOFactory.I.Width(width / 2 - 2), apGUILOFactory.I.Height(24)))
			{
				//ModMesh를 복사할 것인지, ModBone을 복사할 것인지 결정
				//추가 21.6.25 : 단일 복사인지, 다중 복사인지 설정
				if (selectedParamSetGroup != null && ParamSetOfMod != null)
				{

					//복사하기 > 여기선 string 사용 가능
					if (isSingleModMeshSelected && ParamSetOfMod._meshData.Contains(ModMesh_Main))
					{
						//ModMesh 복사
						string clipboardName = "";
						if (ModMesh_Main._transform_Mesh != null)				{ clipboardName = ModMesh_Main._transform_Mesh._nickName; }
						else if (ModMesh_Main._transform_MeshGroup != null)		{ clipboardName = ModMesh_Main._transform_MeshGroup._nickName; }

						//clipboardName += "\n" + ParamSetOfMod._controlKeyName + "( " + ParamSetOfMod.ControlParamValue + " )";
						string controlParamName = "[Unknown Param]";
						if (selectedParamSetGroup._keyControlParam != null)
						{
							controlParamName = selectedParamSetGroup._keyControlParam._keyName;
						}
						//clipboardName += "\n" + controlParamName + "( " + ParamSetOfMod.ControlParamValue + " )";
						clipboardName += " > " + controlParamName + " ( " + ParamSetOfMod.ControlParamValue + " )";

						apSnapShotManager.I.Copy_ModMesh_SingleTarget(ModMesh_Main, clipboardName, _iPasteSlot_Main);
					}
					else if (isSingleModBoneSelected && ParamSetOfMod._boneData.Contains(ModBone_Main))
					{
						//ModBone 복사
						string clipboardName = "";
						if (ModBone_Main._bone != null) { clipboardName = ModBone_Main._bone._name; }

						//clipboardName += "\n" + ParamSetOfMod._controlKeyName + "( " + ParamSetOfMod.ControlParamValue + " )";
						string controlParamName = "[Unknown Param]";
						if (selectedParamSetGroup._keyControlParam != null)
						{
							controlParamName = selectedParamSetGroup._keyControlParam._keyName;
						}
						//clipboardName += "\n" + controlParamName + "( " + ParamSetOfMod.ControlParamValue + " )";
						clipboardName += " > " + controlParamName + " ( " + ParamSetOfMod.ControlParamValue + " )";

						apSnapShotManager.I.Copy_ModBone_SingleTarget(ModBone_Main, clipboardName, _iPasteSlot_Main);
					}
					else if (isAnyModMeshSelected)
					{
						apSnapShotManager.I.Copy_ModMesh_MultipleTargets(ModMeshes_All, "Multiple Meshes", _iPasteSlot_Main);
					}
					else if (isAnyModBoneSelected)
					{
						//추가 21.6.25 : 여러개의 ModBone 복사 가능 (리스트 상태로 입력한다.)
						apSnapShotManager.I.Copy_ModBones_MultipleTargets(ModBones_All, "Multiple Bones", _iPasteSlot_Main);
					}
				}
			}

			//붙여넣기 버튼
			if (GUILayout.Button(_guiContent_PasteTextIcon.Content, apGUILOFactory.I.Width(width / 2 - 2), apGUILOFactory.I.Height(24)))
			{
				//ModMesh를 복사할 것인지, ModBone을 복사할 것인지 결정
				if (isAnyModMeshSelected || isAnyModBoneSelected)
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						//변경 22.7.10
						//리셋과 마찬가지로, 속성을 별개로 지정하여 붙여넣기를 할 수 있다.

						apDialog_SetValueTarget.SELECTABLE_TARGETS selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.None;
						switch (Modifier.ModifierType)
						{
							case apModifierBase.MODIFIER_TYPE.Morph:
								selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.Vertices
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Pins
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Color
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra;
								break;
							case apModifierBase.MODIFIER_TYPE.TF:
								selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.Transform
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Color
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra;
								break;
							case apModifierBase.MODIFIER_TYPE.ColorOnly:
								selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Color
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra;
								break;
						}


						_loadKey_ParamSetMod_SetValue = apDialog_SetValueTarget.ShowDialog(Editor,
																							MeshGroup,
																							Modifier,
																							selectableTargets,
																							ParamSetOfMod,
																							OnParamSetModTargetProperty_Paste,
																							"Paste Value",
																							_iPasteSlot_Main,
																							_isPasteSlotSelected,
																							_iMultiPasteSlotMethod,
																							nSelectedPastableSlots
																							);
					}
				}
			}
			EditorGUILayout.EndHorizontal();


			//리셋 버튼
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetValue), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20)))//"Reset Value"
			{
				if (ParamSetOfMod != null)
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						//변경 22.7.9 : 리셋하는 대상을 정할 수 있다.
						apDialog_SetValueTarget.SELECTABLE_TARGETS selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.None;
						switch (Modifier.ModifierType)
						{
							case apModifierBase.MODIFIER_TYPE.Morph:
								selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.Vertices
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Pins
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Color
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra;
								break;
							case apModifierBase.MODIFIER_TYPE.TF:
								selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.Transform
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Color
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra;
								break;
							case apModifierBase.MODIFIER_TYPE.ColorOnly:
								selectableTargets = apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Color
													| apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra;
								break;
						}


						//어떤 프로퍼티를 초기화할 지 선택하자 (v1.4.0)
						_loadKey_ParamSetMod_SetValue = apDialog_SetValueTarget.ShowDialog(
															Editor,
															MeshGroup,
															Modifier,
															selectableTargets,
															ParamSetOfMod,
															OnParamSetModTargetProperty_Reset,
															"Reset Value");
					}

				}
			}

			


			//Transform(Controller)에 한해서 Pose를 저장할 수 있다.
			if(Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.TF)
			{
				GUILayout.Space(5);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(5);

				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ExportImportPose));//"Export/Import Pose"
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
				GUILayout.Space(4);

				if(_guiContent_Modifier_RigExport == null)
				{
					_guiContent_Modifier_RigExport = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Export), Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad));
				}
				
				if(_guiContent_Modifier_RigImport == null)
				{
					_guiContent_Modifier_RigImport = apGUIContentWrapper.Make(1, Editor.GetUIWord(UIWORD.Import), Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones));
				}
				


				//" Export"
				if(GUILayout.Button(_guiContent_Modifier_RigExport.Content, apGUILOFactory.I.Width((width / 2) - 2), apGUILOFactory.I.Height(25)))
				{
					//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						//Export Dialog 호출
						apDialog_RetargetSinglePoseExport.ShowDialog(Editor, MeshGroup, Bone);
					}
				}

				//" Import"
				if(GUILayout.Button(_guiContent_Modifier_RigImport.Content, apGUILOFactory.I.Width((width / 2) - 2), apGUILOFactory.I.Height(25)))
				{
					//Import Dialog 호출
					if (selectedParamSetGroup != null && ParamSetOfMod != null)
					{
						//v1.4.2 : FFD 모드 같은 모달 상태를 체크한다.
						bool isExecutable = Editor.CheckModalAndExecutable();

						if (isExecutable)
						{
							_loadKey_SinglePoseImport_Mod = apDialog_RetargetSinglePoseImport.ShowDialog(OnRetargetSinglePoseImportMod, Editor, MeshGroup, Modifier, ParamSetOfMod);
						}
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);



			//--------------------------------------------------------------
			// Param Set 중 하나를 선택했을 때
			// 타겟을 등록 / 해제한다.
			// Transform 등록 / 해제
			//--------------------------------------------------------------
			bool isMultipleSelected = false;
			bool isAnyTargetSelected = false;
			bool isContain = false;
			
			//string strTargetName = "";
			bool isTargetName = false;

			object selectedObj = null;

			if(_guiContent_ModProp_ParamSetTarget_Name == null)			{ _guiContent_ModProp_ParamSetTarget_Name = new apGUIContentWrapper(); }
			if(_guiContent_ModProp_ParamSetTarget_StatusText == null)	{ _guiContent_ModProp_ParamSetTarget_StatusText = new apGUIContentWrapper(); }
			_guiContent_ModProp_ParamSetTarget_Name.ClearText(false);
			_guiContent_ModProp_ParamSetTarget_StatusText.ClearText(false);

			bool isTarget_Bone = Modifier.IsTarget_Bone;
			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_MeshGroupTransform = Modifier.IsTarget_MeshGroupTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isBoneTarget = false;

			// 타겟을 선택하자
			bool isAddable = false;


			//추가 20.9.8 : 특수한 상황에서 Add가 불가능한데, 이때 띄울 경로 문구 타입을 정하자
			MOD_ADD_FAIL_REASON modAddFailReason = MOD_ADD_FAIL_REASON.NotSupportedType;//기본값을 먼저 할당


			//추가 20.6.4 : 다중 선택 여부 체크하자
			//그냥 총 개수만 볼게 아니라, 모디파이어에 맞게 확인할 것 (지원 안하면 0)
			int nMeshTF =		isTarget_MeshTransform ? _subObjects.NumMeshTF : 0;
			int nMeshGroupTF =	isTarget_MeshGroupTransform ? _subObjects.NumMeshGroupTF : 0;
			int nBones =		isTarget_Bone ? _subObjects.NumBone : 0;
			int nSelectedObjects = nMeshTF + nMeshGroupTF + nBones;

			if (nSelectedObjects > 1)
			{
				//2개 이상 선택된 상태이다.
				//> 다중 선택을 처리하자
				isMultipleSelected = true;

				//모두 등록된 상태인지 확인하자.
				//isContain : 모두 True일 때만 True. 하나라도 False면 False
				//isAddable : 하나라도 True라면 True
				isContain = true;
				isAddable = false;

				isAnyTargetSelected = true;
				int nValidObjects = 0;

				if(isTarget_Bone && nBones > 0)
				{
					List<apBone> bones = _subObjects.AllBones;
					apBone curBone = null;
					for (int i = 0; i < nBones; i++)
					{
						curBone = bones[i];
						if(curBone == null) { continue; }

						isAnyTargetSelected = true;
						isAddable = true;//하나라도 추가 가능
						if(!selectedParamSetGroup.IsBoneContain(curBone))
						{
							isContain = false;//하나라도 추가 안됨 > False
						}
						nValidObjects++;
					}
				}
				if(isTarget_MeshTransform && nMeshTF > 0)
				{
					List<apTransform_Mesh> meshTFs = _subObjects.AllMeshTFs;
					apTransform_Mesh curMeshTF = null;
					apRenderUnit targetRenderUnit = null;

					for (int i = 0; i < nMeshTF; i++)
					{
						curMeshTF = meshTFs[i];
						if(curMeshTF == null) { continue; }

						targetRenderUnit = null;

						//Child Mesh 허용여부에 따라 RenderUnit을 가져오는게 다르다.
						if (isTarget_ChildMeshTransform)	{ targetRenderUnit = MeshGroup.GetRenderUnit(curMeshTF); }//Child 허용
						else								{ targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(curMeshTF); }

						if (targetRenderUnit != null)
						{
							//유효한 선택인 경우
							
							if(!selectedParamSetGroup.IsMeshTransformContain(curMeshTF))
							{
								isContain = false;//하나라도 추가 안됨 > False
							}

							//추가 가능한 MeshTF의 개수를 찾자
							//기존
							//isAddable = true;
							//nValidObjects++;


							//변경 20.9.8 : 리깅 여부에 따라서 일부 메시는 모디파이어에 추가할 수 없다.
							if(Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.TF && curMeshTF.IsRiggedChildMeshTF(MeshGroup))
							{
								//추가 불가
								//개수는 모르지만 사유는 적어두자
								modAddFailReason = MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod;
							}
							else
							{
								//별다른 이유가 없으니 추가 가능
								isAddable = true;
								nValidObjects++;
							}
							
						}
					}
				}
				if(isTarget_MeshGroupTransform && nMeshGroupTF > 0)
				{
					List<apTransform_MeshGroup> meshGroupTFs = _subObjects.AllMeshGroupTFs;
					apTransform_MeshGroup curMeshGroupTF = null;
					
					for (int i = 0; i < nMeshGroupTF; i++)
					{
						curMeshGroupTF = meshGroupTFs[i];
						if(curMeshGroupTF == null) { continue; }

						isAddable = true;//하나라도 추가 가능
						if(!selectedParamSetGroup.IsMeshGroupTransformContain(curMeshGroupTF))
						{
							isContain = false;//하나라도 추가 안됨 > False
						}
						nValidObjects++;
					}
				}

				if(nValidObjects > 0)
				{
					//선택된 개수를 이름으로 설정
					_guiContent_ModProp_ParamSetTarget_Name.AppendText(nValidObjects, false);
					_guiContent_ModProp_ParamSetTarget_Name.AppendSpaceText(1, false);
					_guiContent_ModProp_ParamSetTarget_Name.AppendText(Editor.GetUIWord(UIWORD.Objects), true);
				}
				else
				{
					//선택된게 정말 없다.
					_guiContent_ModProp_ParamSetTarget_Name.AppendText(apStringFactory.I.None, true);
				}
			}
			else
			{
				//1개 혹은 0개가 선택되었다.
				//기존 방식대로 단일 선택을 처리하자.
				isMultipleSelected = false;

				if (isTarget_Bone && !isAnyTargetSelected)
				{
					//1. Bone 선택
					//TODO : Bone 체크
					if (Bone != null)
					{
						isAnyTargetSelected = true;
						isAddable = true;
						isContain = selectedParamSetGroup.IsBoneContain(Bone);

						_guiContent_ModProp_ParamSetTarget_Name.AppendText(Bone._name, true);
						isTargetName = true;

						selectedObj = Bone;
						isBoneTarget = true;
					}
				}
				if (isTarget_MeshTransform && !isAnyTargetSelected)
				{
					//2. Mesh Transform 선택
					//Child 체크가 가능할까
					if (MeshTF_Main != null)
					{
						apRenderUnit targetRenderUnit = null;
						//Child Mesh를 허용하는가
						if (isTarget_ChildMeshTransform)
						{
							//Child를 허용한다.
							targetRenderUnit = MeshGroup.GetRenderUnit(MeshTF_Main);
						}
						else
						{
							//Child를 허용하지 않는다.
							targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(MeshTF_Main);
						}

						if (targetRenderUnit != null)
						{
							//유효한 선택인 경우
							isContain = selectedParamSetGroup.IsMeshTransformContain(MeshTF_Main);
							isAnyTargetSelected = true;

							//strTargetName = SubMeshInGroup._nickName;
							_guiContent_ModProp_ParamSetTarget_Name.AppendText(MeshTF_Main._nickName, true);
							isTargetName = true;

							selectedObj = MeshTF_Main;

							//기존
							//isAddable = true;

							//변경 20.9.8 : 리깅된 하위 메시 그룹의 메시는 Mod TF에 추가할 수 없다. (Morph는 가능)
							//조건은 다음과 같다.
							//- TF 모디파이어
							//- 자식 메시 그룹의 메시
							//- 리깅된 상태
							if(Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.TF 
								&& MeshTF_Main.IsRiggedChildMeshTF(MeshGroup))
							{
								//이 조건에서는 추가 불가능하다.
								modAddFailReason = MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod;
								isAddable = false;
							}
							else
							{
								//추가 가능
								isAddable = true;
							}
						}
					}
				}
				if (isTarget_MeshGroupTransform && !isAnyTargetSelected)
				{
					if (MeshGroupTF_Main != null)
					{
						//3. MeshGroup Transform 선택
						isContain = selectedParamSetGroup.IsMeshGroupTransformContain(MeshGroupTF_Main);
						isAnyTargetSelected = true;

						//strTargetName = SubMeshGroupInGroup._nickName;
						_guiContent_ModProp_ParamSetTarget_Name.AppendText(MeshGroupTF_Main._nickName, true);
						isTargetName = true;

						selectedObj = MeshGroupTF_Main;

						isAddable = true;
					}
				}
			}

			

			//타겟이 없었다면 빈이름 대입
			if(!isTargetName)
			{
				_guiContent_ModProp_ParamSetTarget_Name.AppendText(apStringFactory.I.None, true);
			}


			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check_Single, isAnyTargetSelected && !isMultipleSelected);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check_Multiple, isAnyTargetSelected && isMultipleSelected);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check_Unselected, !isAnyTargetSelected);

			bool isRender_Target_Single = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check_Single);
			bool isRender_Target_Multi = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check_Multiple);

			bool isRender_Target_Unselected = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check_Unselected);



			//타겟의 값이 선택(단일/다중), 또는 해제가 변경되고 유효한 프레임인 경우
			if (isRender_Target_Single || isRender_Target_Multi || isRender_Target_Unselected)
			{
				//선택된 경우, 값을 보여주자.
				//[공통 - 처리 분기]
				if (isRender_Target_Single || isRender_Target_Multi)
				{	
					if (isContain)
					{
						//1. [등록된 상태]

						//등록 상태 문구
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.Selected), true);


						GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
						GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

						GUI.backgroundColor = prevColor;

						//삭제 버튼
						if(_guiContent_Modifier_RemoveFromKeys == null)
						{
							_guiContent_Modifier_RemoveFromKeys = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveFromKeys), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromControlParamKey));
						}
						
						//버튼 출력 가능 여부를 체크 (20.9.10)
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_AddOrRemoveKeyButton, true);
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_NoAddKeyBtn, false);
						bool isRemoveKeyBtnVisible = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_AddOrRemoveKeyButton);

						if (isRemoveKeyBtnVisible)
						{
							// [ Remove From Key 버튼 ]
							if (GUILayout.Button(_guiContent_Modifier_RemoveFromKeys.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35)))
							{
								//v1.4.2 : 모달 상태를 체크해야한다.
								bool isExecutable = Editor.CheckModalAndExecutable();

								if (isExecutable)
								{
									bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveFromKeys_Title),
																		Editor.GetTextFormat(TEXT.RemoveFromKeys_Body, _guiContent_ModProp_ParamSetTarget_Name.Content.text),
																		Editor.GetText(TEXT.Remove),
																		Editor.GetText(TEXT.Cancel)
																		);

									if (result)
									{
										if (isRender_Target_Single)
										{
											//[단일]
											object targetObj = null;
											if (MeshTF_Main != null && selectedObj == MeshTF_Main)
											{
												targetObj = MeshTF_Main;
											}
											else if (MeshGroupTF_Main != null && selectedObj == MeshGroupTF_Main)
											{
												targetObj = MeshGroupTF_Main;
											}
											else if (Bone != null && selectedObj == Bone)
											{
												targetObj = Bone;
											}

											//Undo
											apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_RemoveModMeshFromParamSet,
																				Editor,
																				Modifier,
																				//targetObj, 
																				false,
																				apEditorUtil.UNDO_STRUCT.StructChanged);

											if (MeshTF_Main != null && selectedObj == MeshTF_Main)
											{
												selectedParamSetGroup.RemoveModifierMeshes(MeshTF_Main);
											}
											else if (MeshGroupTF_Main != null && selectedObj == MeshGroupTF_Main)
											{
												selectedParamSetGroup.RemoveModifierMeshes(MeshGroupTF_Main);
											}
											else if (Bone != null && selectedObj == Bone)
											{
												selectedParamSetGroup.RemoveModifierBones(Bone);
											}
										}
										else if (isRender_Target_Multi)
										{
											//[다중] 20.6.4
											//리스트로 찾아서 직접 하나씩 다 없애자
											//Undo
											apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_RemoveModMeshFromParamSet,
																				Editor,
																				Modifier,
																				//null, 
																				false,
																				apEditorUtil.UNDO_STRUCT.StructChanged);
											if (isTarget_Bone && nBones > 0)
											{
												List<apBone> bones = _subObjects.AllBones;
												apBone curBone = null;
												for (int i = 0; i < nBones; i++)
												{
													curBone = bones[i];
													if (curBone == null)
													{ continue; }

													selectedParamSetGroup.RemoveModifierBones(curBone);
												}
											}
											if (isTarget_MeshTransform && nMeshTF > 0)
											{
												List<apTransform_Mesh> meshTFs = _subObjects.AllMeshTFs;
												apTransform_Mesh curMeshTF = null;

												for (int i = 0; i < nMeshTF; i++)
												{
													curMeshTF = meshTFs[i];
													if (curMeshTF == null)
													{ continue; }

													selectedParamSetGroup.RemoveModifierMeshes(curMeshTF);
												}
											}
											if (isTarget_MeshGroupTransform && nMeshGroupTF > 0)
											{
												List<apTransform_MeshGroup> meshGroupTFs = _subObjects.AllMeshGroupTFs;
												apTransform_MeshGroup curMeshGroupTF = null;

												for (int i = 0; i < nMeshGroupTF; i++)
												{
													curMeshGroupTF = meshGroupTFs[i];
													if (curMeshGroupTF == null)
													{ continue; }

													selectedParamSetGroup.RemoveModifierMeshes(curMeshGroupTF);
												}
											}
										}

										//Sync를 다시 해야한다.
										selectedParamSetGroup.RefreshSync();

										//다시 갱신
										Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_Modifier(MeshGroup, Modifier));
										AutoSelectModMeshOrModBone();
										Editor.RefreshControllerAndHierarchy(false);

										//추가 21.1.32 : Rule 가시성 동기화 초기화 / 추가 삭제 코드가 왜 없었지
										Editor.Controller.ResetVisibilityPresetSync();
										Editor.OnAnyObjectAddedOrRemoved();

										//변경 21.2.17
										//Debug.LogError("ExFlag");
										RefreshMeshGroupExEditingFlags(true);

										Editor.SetRepaint();
									}
								}
							}
						}
					}
					else if (!isAddable)
					{

						//2. [등록 안된 상태 + 추가 가능하지 않다.]
						//추가 불가 안내
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);

						//불가 이유에 따라서 메시지가 다르다 (20.9.8)
						//업데이트시 내용이 추가될 수 있다.
						if(modAddFailReason == MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod)
						{
							//리깅 자식 메시의 경우
							_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.NotAbleToBeAdded_RiggedChildMesh), true);
						}
						else
						{
							//기본 (지원하지 않는 타입)
							_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.NotAbleToBeAdded), true);
						}
						


						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

						GUI.backgroundColor = prevColor;

						//버튼 출력 가능 여부를 체크 (20.9.10)
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_AddOrRemoveKeyButton, false);
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_NoAddKeyBtn, true);
					}
					else
					{
						//3. [등록 안된 상태 + 추가할 수 있다.]
						//추가 20.6.4 : 다중 선택시 "일부만 등록되고 일부는 안된 경우">"모두 마저 등록시키는 것"을 전제로함
						// 등록 아직 안됨 안내
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
						_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.NotAddedtoEdit), true);

						

						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

						GUI.backgroundColor = prevColor;


						//버튼 출력 가능 여부를 체크 (20.9.10)
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_AddOrRemoveKeyButton, true);
						Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_NoAddKeyBtn, false);
						bool isAddKeyBtnVisible = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_AddOrRemoveKeyButton);

						if (isAddKeyBtnVisible)
						{
							//키 추가 버튼
							if (_guiContent_Modifier_AddToKeys == null)
							{
								_guiContent_Modifier_AddToKeys = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AddToKeys), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToControlParamKey));
							}

							//추가 22.6.10 : Add To Key 버튼은 반짝인다.
							GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();

							if (GUILayout.Button(_guiContent_Modifier_AddToKeys.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(50)))
							{
								//v1.4.2 : 모달 상태를 체크해야한다.
								bool isExecutable = Editor.CheckModalAndExecutable();

								if (isExecutable)
								{
									if (isRender_Target_Single)
									{
										//[단일]
										//ModMesh또는 ModBone으로 생성 후 추가한다.
										if (isBoneTarget)
										{
											//Bone
											Editor.Controller.AddModBone_WithSelectedBone();
										}
										else
										{
											//MeshTransform, MeshGroup
											Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();
										}
									}
									else if (isRender_Target_Multi)
									{
										//[다중]
										//선택된 리스트를 바탕으로 여러개의 ModMesh와 ModeBone을 생성한다.
										Editor.Controller.AddModMeshesBones_WithMultipleSelected();

									}

									//Sync를 다시 해야한다.									
									Editor.SetRepaint();

									//추가 : ExEdit 모드가 아니라면, Modifier에 추가할 때 자동으로 ExEdit 상태로 전환
									if (ExEditingMode == EX_EDIT.None && IsExEditable)
									{
										SetModifierExclusiveEditing(EX_EDIT.ExOnly_Edit);

										//변경 3.23 : 선택 잠금을 무조건 켜는게 아니라, 에디터 설정에 따라 켤지 말지 결정한다.
										//true 또는 변경 없음 (false가 아님)
										//모디파이어의 종류에 따라서 다른 옵션을 적용
										if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic ||
											Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
										{
											if (Editor._isSelectionLockOption_RiggingPhysics)
											{
												_isSelectionLock = true;//처음 Editing 작업시 Lock을 거는 것으로 변경
											}
										}
										else if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Morph ||
											Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
										{
											if (Editor._isSelectionLockOption_Morph)
											{
												_isSelectionLock = true;//처음 Editing 작업시 Lock을 거는 것으로 변경
											}
										}
										else
										{
											if (Editor._isSelectionLockOption_Transform)
											{
												_isSelectionLock = true;//처음 Editing 작업시 Lock을 거는 것으로 변경
											}
										}

									}
								}
							}

							GUI.backgroundColor = prevColor;
						}
						
					}
					GUI.backgroundColor = prevColor;
				}
				else
				{
					//선택한게 없다. > 버튼 없음
					Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_AddOrRemoveKeyButton, false);
					Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_NoAddKeyBtn, true);
				}

				//버튼 출력 가능 여부가 유지되는 경우에만 하위 UI를 표시 (20.9.10)
				bool isSameObjectUI = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_AddOrRemoveKeyButton)
										|| Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_ControlParam_NoAddKeyBtn);

				if (isSameObjectUI)
				{
					EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(10));
					//<< 아래의 리스트가 제대로 유지하도록 만드는 더미 Layout
					EditorGUILayout.EndVertical();
					GUILayout.Space(11);

					//ParamSetWeight를 사용하는 Modifier인가
					bool isUseParamSetWeight = Modifier.IsUseParamSetWeight;


					// Param Set 리스트를 출력한다.
					//-------------------------------------
					int iRemove = -1;
					
					int nParamSetList = selectedParamSetGroup._paramSetList != null ? selectedParamSetGroup._paramSetList.Count : 0;
					
					for (int i = 0; i < nParamSetList; i++)
					{
						bool isRemove = DrawModParamSetProperty(i, selectedParamSetGroup, selectedParamSetGroup._paramSetList[i], width - 10, ParamSetOfMod, isUseParamSetWeight);
						if (isRemove)
						{
							iRemove = i;
						}
					}
					if (iRemove >= 0)
					{
						Editor.Controller.RemoveRecordKey(selectedParamSetGroup._paramSetList[iRemove]);
					}

					//추가 22.6.10 : 컨트롤 파라미터의 등록을 통째로 해제하는 버튼을 추가한다.

					GUILayout.Space(10);

					if(_guiContent_Modifier_RemoveAllKeys == null)
					{
						_guiContent_Modifier_RemoveAllKeys = apGUIContentWrapper.Make(	2, 
																						Editor.GetUIWord(UIWORD.RemoveAllKeys),
																						Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
					}

					
					// [ Remove All Keys 버튼 ]

					if(GUILayout.Button(_guiContent_Modifier_RemoveAllKeys.Content, apGUILOFactory.I.Height(25)))
					{
						//v1.4.2 : 모달 상태를 체크해야한다.
						bool isExecutable = Editor.CheckModalAndExecutable();

						if (isExecutable)
						{
							//물어보고
							//"컨트롤 파라미터 등록 해제"
							//"현재 모디파이어에서 선택된 컨트롤 파라미터와 연결된 키들을 모두 삭제하시겠습니까?"
							bool result = EditorUtility.DisplayDialog(
																Editor.GetText(TEXT.DLG_RemoveAllKeysOfControlParam_Title),
																Editor.GetTextFormat(TEXT.DLG_RemoveAllKeysOfControlParam_Body, selectedParamSetGroup._keyControlParam._keyName),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel));


							//현재 PSG를 삭제한다.
							if (result)
							{
								Editor.Controller.RemoveParamSetGroup(selectedParamSetGroup);
							}
						}	
					}
				}
			}


			//-----------------------------------------------------------------------------------
		}

		
		/// <summary>
		/// 메시 그룹 Right 2 UI > Control Param Modifier에서의 Param Set UI
		/// </summary>
		private bool DrawModParamSetProperty(int index, apModifierParamSetGroup paramSetGroup, apModifierParamSet paramSet, int width, apModifierParamSet selectedParamSet, bool isUseParamSetWeight)
		{
			bool isRemove = false;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Color prevColor = GUI.backgroundColor;

			bool isSelect = false;
			if (paramSet == selectedParamSet)
			{
				GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
				isSelect = true;
			}
			else
			{
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
			}

			int heightOffset = 18;
			if (index == 0)
			{
				//heightOffset = 5;
				heightOffset = 9;
			}

			GUI.Box(new Rect(lastRect.x, lastRect.y + heightOffset, width + 10, 30), apStringFactory.I.None);
			GUI.backgroundColor = prevColor;



			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));

			GUILayout.Space(10);

			//v1.4.2 : ParamSet Key의 위치/가중치를 변경하고자 할 때 FFD가 켜져있다면 적용하도록 하자 (UI 이벤트때문에 취소는 안됨)

			int compWidth = width - (55 + 20 + 5 + 10);
			if (isUseParamSetWeight)
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.textField);
				//guiStyle.alignment = TextAnchor.MiddleLeft;

				//ParamSetWeight를 출력/수정할 수 있게 한다.
				
				EditorGUI.BeginChangeCheck();
				float paramSetWeight = EditorGUILayout.DelayedFloatField(paramSet._overlapWeight, apGUIStyleWrapper.I.TextField_MiddleLeft, apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(20));
				bool isChanged = EditorGUI.EndChangeCheck();

				if (isChanged && Mathf.Abs(paramSetWeight - paramSet._overlapWeight) > 0.001f)
				{
					//v1.4.2 FFD가 켜진 경우 취소 없이 적용/복구
					if(Editor.Gizmos.IsFFDMode)
					{
						Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
					}


					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
														Editor, 
														Modifier, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					paramSet._overlapWeight = Mathf.Clamp01(paramSetWeight);
					apEditorUtil.ReleaseGUIFocus();
					MeshGroup.RefreshForce();
					Editor.RefreshControllerAndHierarchy(false);
				}
				compWidth -= 34;
			}

			switch (paramSetGroup._keyControlParam._valueType)
			{
				case apControlParam.TYPE.Int:
					{
						EditorGUI.BeginChangeCheck();				
						int conInt = EditorGUILayout.DelayedIntField(paramSet._conSyncValue_Int, apGUIStyleWrapper.I.TextField_MiddleLeft, apGUILOFactory.I.Width(compWidth), apGUILOFactory.I.Height(20));
						bool isChanged = EditorGUI.EndChangeCheck();

						if (isChanged && conInt != paramSet._conSyncValue_Int)
						{	
							//v1.4.2 FFD가 켜진 경우 취소 없이 적용/복구
							if(Editor.Gizmos.IsFFDMode)
							{
								Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
							}

							apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
																Editor, 
																Modifier, 
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							paramSet._conSyncValue_Int = conInt;
							apEditorUtil.ReleaseGUIFocus();
						}

					}
					break;

				case apControlParam.TYPE.Float:
					{
						EditorGUI.BeginChangeCheck();
						float conFloat = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Float, apGUIStyleWrapper.I.TextField_MiddleLeft, apGUILOFactory.I.Width(compWidth), apGUILOFactory.I.Height(20));
						bool isChanged = EditorGUI.EndChangeCheck();

						if (isChanged && Mathf.Abs(conFloat - paramSet._conSyncValue_Float) > 0.001f)
						{
							//v1.4.2 FFD가 켜진 경우 취소 없이 적용/복구
							if(Editor.Gizmos.IsFFDMode)
							{
								Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
							}

							apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
																Editor, 
																Modifier, 
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							paramSet._conSyncValue_Float = conFloat;
							apEditorUtil.ReleaseGUIFocus();
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						EditorGUI.BeginChangeCheck();
						float conVec2X = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Vector2.x, apGUIStyleWrapper.I.TextField_MiddleLeft, apGUILOFactory.I.Width(compWidth / 2 - 2), apGUILOFactory.I.Height(20));
						float conVec2Y = EditorGUILayout.DelayedFloatField(paramSet._conSyncValue_Vector2.y, apGUIStyleWrapper.I.TextField_MiddleLeft, apGUILOFactory.I.Width(compWidth / 2 - 2), apGUILOFactory.I.Height(20));
						bool isChanged = EditorGUI.EndChangeCheck();

						if (isChanged
							&& (Mathf.Abs(conVec2X - paramSet._conSyncValue_Vector2.x) > 0.001f || Mathf.Abs(conVec2Y - paramSet._conSyncValue_Vector2.y) > 0.001f)
							)
						{
							//v1.4.2 FFD가 켜진 경우 취소 없이 적용/복구
							if(Editor.Gizmos.IsFFDMode)
							{
								Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
							}

							apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
																Editor, 
																Modifier, 
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							paramSet._conSyncValue_Vector2.x = conVec2X;
							paramSet._conSyncValue_Vector2.y = conVec2Y;
							apEditorUtil.ReleaseGUIFocus();
						}

					}
					break;
			}

			if (isSelect)
			{
				GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
				GUILayout.Box(Editor.GetUIWord(UIWORD.Selected), apGUIStyleWrapper.I.Box_UpperCenter_WhiteColor, apGUILOFactory.I.Width(55), apGUILOFactory.I.Height(20));//"Editing" -> Selected
				GUI.backgroundColor = prevColor;
			}
			else
			{
				//"Select"
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Select), apGUILOFactory.I.Width(55), apGUILOFactory.I.Height(20)))
				{
					if (ParamSetOfMod != paramSet)
					{
						//v1.4.2 : 모달 상태를 체크한다.
						bool isExecutable = Editor.CheckModalAndExecutable();

						if (isExecutable)
						{
							if (Editor.LeftTab != apEditor.TAB_LEFT.Controller)
							{
								//옵션이 허용하는 경우 (19.6.28 변경)
								if (Editor._isAutoSwitchControllerTab_Mod)
								{
									Editor.SetLeftTab(apEditor.TAB_LEFT.Controller);
								}
							}

							SelectParamSetOfModifier(paramSet);
							if (ParamSetOfMod != null)
							{
								apControlParam targetControlParam = paramSetGroup._keyControlParam;
								if (targetControlParam != null)
								{
									//switch (ParamSetOfMod._controlParam._valueType)
									switch (targetControlParam._valueType)
									{
										//case apControlParam.TYPE.Bool:
										//	targetControlParam._bool_Cur = paramSet._conSyncValue_Bool;
										//	break;

										case apControlParam.TYPE.Int:
											targetControlParam._int_Cur = paramSet._conSyncValue_Int;
											//if (targetControlParam._isRange)
											{
												targetControlParam._int_Cur =
													Mathf.Clamp(targetControlParam._int_Cur,
																targetControlParam._int_Min,
																targetControlParam._int_Max);
											}
											break;

										case apControlParam.TYPE.Float:
											targetControlParam._float_Cur = paramSet._conSyncValue_Float;
											//if (targetControlParam._isRange)
											{
												targetControlParam._float_Cur =
													Mathf.Clamp(targetControlParam._float_Cur,
																targetControlParam._float_Min,
																targetControlParam._float_Max);
											}
											break;

										case apControlParam.TYPE.Vector2:
											targetControlParam._vec2_Cur = paramSet._conSyncValue_Vector2;
											//if (targetControlParam._isRange)
											{
												targetControlParam._vec2_Cur.x =
													Mathf.Clamp(targetControlParam._vec2_Cur.x,
																targetControlParam._vec2_Min.x,
																targetControlParam._vec2_Max.x);

												targetControlParam._vec2_Cur.y =
													Mathf.Clamp(targetControlParam._vec2_Cur.y,
																targetControlParam._vec2_Min.y,
																targetControlParam._vec2_Max.y);
											}
											break;


									}
								}
							}
						}
					}
				}
			}

			if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20)))
			{
				//v1.4.2 : 모달 상태를 체크한다.
				bool isExecutable = Editor.CheckModalAndExecutable();
				if (isExecutable)
				{
					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveRecordKey_Title),
																	Editor.GetText(TEXT.RemoveRecordKey_Body),
																	Editor.GetText(TEXT.Remove),
																	Editor.GetText(TEXT.Cancel));
					if (isResult)
					{
						//삭제시 true 리턴
						isRemove = true;
					}
				}
			}



			EditorGUILayout.EndHorizontal();
			GUILayout.Space(20);

			return isRemove;
		}

		
		//추가 22.7.9 : 컨트롤 파라미터-모디파이어의 ParamSet의 값을 리셋할 때, 대상의 일부만 리셋할 수 있다.
		private object _loadKey_ParamSetMod_SetValue = null;
		private void OnParamSetModTargetProperty_Reset(	bool isSuccess,
														object loadKey, 
														apMeshGroup meshGroup,
														apModifierBase modifier,
														apDialog_SetValueTarget.SELECTABLE_TARGETS selectedTargets, 
														bool isSelectedOnly,
														object savedObject,
														List<apModifiedMesh> modMeshes,
														List<apModifiedBone> modBones,
														List<apModifiedVertex> modVerts,
														List<apModifiedPin> modPins,
														int pasteSlotIndex_Main,
														bool[] pasteSlotSelected,
														int pasteMethod,
														int nSelectedSlots)
		{
			
			if(!isSuccess
				|| _loadKey_ParamSetMod_SetValue != loadKey
				|| loadKey == null
				|| meshGroup == null
				|| modifier == null
				|| MeshGroup != meshGroup
				|| Modifier != modifier)
			{
				_loadKey_ParamSetMod_SetValue = null;
				
				return;
			}

			_loadKey_ParamSetMod_SetValue = null;

			//추가적으로 조건을 확인한다.
			if(savedObject == null)
			{
				//Debug.Log("Saved Object 없음");
				return;
			}

			apModifierParamSet savedParamSet = null;
			if(savedObject is apModifierParamSet)
			{
				savedParamSet = savedObject as apModifierParamSet;
			}
			if(ParamSetOfMod == null
				|| ParamSetOfMod != savedParamSet)
			{
				//Debug.Log("Saved Param Set 맞지 않음");
				return;
			}

			

			int nModMeshes = modMeshes != null ? modMeshes.Count : 0;
			int nModBones = modBones != null ? modBones.Count : 0;
			
			if(nModMeshes == 0 && nModBones == 0)
			{
				//대상이 없다.
				//Debug.Log("대상 Mod Mesh/Mod Bone이 없음");
				return;
			}

			//특정 프로퍼티만 초기화 할 수 있다.
			bool isResetVerts =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Vertices) != 0;
			bool isResetPins =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Pins) != 0;
			bool isResetTransform =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Transform) != 0;
			bool isResetVisibility =	(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility) != 0;
			bool isResetColor =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Color) != 0;
			bool isResetExtra =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra) != 0;
			
			//Debug.Log("Reset 대상 : 버텍스 : " + isResetVerts);
			//Debug.Log("Reset 대상 : 핀 : " + isResetPins);
			//Debug.Log("Reset 대상 : 트랜스폼 : " + isResetTransform);
			//Debug.Log("Reset 대상 : 가시성 : " + isResetVisibility);
			//Debug.Log("Reset 대상 : 색상 : " + isResetColor);
			//Debug.Log("Reset 대상 : 엑스트라 : " + isResetExtra);

			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_ModMeshValueReset,
														Editor,
														Modifier,
														//targetObj, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

			if (nModMeshes > 0 && savedParamSet._meshData != null)
			{
				apModifiedMesh curModMesh = null;
				for (int i = 0; i < nModMeshes; i++)
				{
					curModMesh = modMeshes[i];
					if(savedParamSet._meshData.Contains(curModMesh))
					{
						curModMesh.ResetValues(isResetVerts,
													isResetPins,
													isResetTransform,
													isResetVisibility,
													isResetColor,
													isResetExtra,
													isSelectedOnly,
													modVerts,
													modPins);
					}
				}
			}
			if(nModBones > 0 && isResetTransform && savedParamSet._boneData != null)
			{
				//본은 Transform 초기화만 한다.
				apModifiedBone curModBone = null;
				for (int i = 0; i < nModBones; i++)
				{
					curModBone = modBones[i];
					if(savedParamSet._boneData.Contains(curModBone))
					{
						curModBone._transformMatrix.SetIdentity();
					}
				}
			}

			MeshGroup.RefreshForce();
		}

		/// <summary>
		/// 붙여넣기시 속성 선택 다이얼로그 결과
		/// </summary>
		private void OnParamSetModTargetProperty_Paste(	bool isSuccess,
														object loadKey, 
														apMeshGroup meshGroup,
														apModifierBase modifier,
														apDialog_SetValueTarget.SELECTABLE_TARGETS selectedTargets, 
														bool isSelectedOnly,
														object savedObject,
														List<apModifiedMesh> modMeshes,
														List<apModifiedBone> modBones,
														List<apModifiedVertex> modVerts,
														List<apModifiedPin> modPins,
														int pasteSlotIndex_Main,
														bool[] pasteSlotSelected,
														int pasteMethod,
														int nSelectedSlots)
		{
			if(!isSuccess
				|| _loadKey_ParamSetMod_SetValue != loadKey
				|| loadKey == null
				|| meshGroup == null
				|| modifier == null
				|| MeshGroup != meshGroup
				|| Modifier != modifier)
			{
				_loadKey_ParamSetMod_SetValue = null;
				
				return;
			}

			_loadKey_ParamSetMod_SetValue = null;

			//추가적으로 조건을 확인한다.
			if(savedObject == null)
			{
				//Debug.Log("Saved Object 없음");
				return;
			}

			apModifierParamSet savedParamSet = null;
			if(savedObject is apModifierParamSet)
			{
				savedParamSet = savedObject as apModifierParamSet;
			}
			if(ParamSetOfMod == null
				|| ParamSetOfMod != savedParamSet)
			{
				//Debug.Log("Saved Param Set 맞지 않음");
				return;
			}

			

			int nModMeshes = modMeshes != null ? modMeshes.Count : 0;
			int nModBones = modBones != null ? modBones.Count : 0;
			
			if(nModMeshes == 0 && nModBones == 0)
			{
				//대상이 없다.
				//Debug.Log("대상 Mod Mesh/Mod Bone이 없음");
				return;
			}

			if(nSelectedSlots <= 0)
			{
				//값을 가져올 슬롯이 0개다..
				return;
			}

			//실제 슬롯을 가져오자
			bool[] selectedSlots = new bool[NUM_PASTE_SLOTS];
			for (int i = 0; i < NUM_PASTE_SLOTS; i++)
			{	
				if(pasteSlotSelected != null && i < pasteSlotSelected.Length)
				{
					selectedSlots[i] = pasteSlotSelected[i];
				}
				else
				{
					selectedSlots[i] = false;
				}
			}

			if(pasteSlotIndex_Main < 0 || pasteSlotIndex_Main >= NUM_PASTE_SLOTS)
			{
				return;
			}

			//특정 프로퍼티만 초기화 할 수 있다.
			bool isPasteProp_Verts =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Vertices) != 0;
			bool isPasteProp_Pins =			(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Pins) != 0;
			bool isPasteProp_Transform =	(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Transform) != 0;
			bool isPasteProp_Visibility =	(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Visibility) != 0;
			bool isPasteProp_Color =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Color) != 0;
			bool isPasteProp_Extra =		(int)(selectedTargets & apDialog_SetValueTarget.SELECTABLE_TARGETS.Extra) != 0;


			apEditorUtil.SetRecord_MeshGroupAndModifier(	apUndoGroupData.ACTION.Modifier_ModMeshValuePaste, 
																		Editor, 
																		MeshGroup, 
																		Modifier, 
																		//targetObj, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

			bool isResult = false;

			//조건에 맞는 모드 메시만 찾자
			List<apModifiedMesh> targetModMeshes = new List<apModifiedMesh>();
			List<apModifiedBone> targetModBones = new List<apModifiedBone>();

			//Mod Mesh
			if (nModMeshes > 0 && ParamSetOfMod._meshData != null)
			{
				apModifiedMesh curModMesh = null;

				for (int i = 0; i < nModMeshes; i++)
				{
					curModMesh = modMeshes[i];
					if (curModMesh != null && ParamSetOfMod._meshData.Contains(curModMesh))
					{
						targetModMeshes.Add(curModMesh);
					}
				}

				if (targetModMeshes.Count == 1)
				{
					if (nSelectedSlots == 1)
					{
						//[단일 메시 + 단일 슬롯]
						if(apSnapShotManager.I.Paste_ModMesh_SingleSlot_SingleTarget(
																				targetModMeshes[0],
																				pasteSlotIndex_Main,
																				(int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0,//Morph 타입인가
																				isPasteProp_Verts,
																				isPasteProp_Pins,
																				isPasteProp_Transform,
																				isPasteProp_Visibility,
																				isPasteProp_Color,
																				isPasteProp_Extra,
																				isSelectedOnly, modVerts, modPins))
						{
							isResult = true;
						}
					}
					else if(nSelectedSlots > 1)
					{
						//[단일 메시 + 다중 슬롯]
						if(apSnapShotManager.I.Paste_ModMesh_MultipleSlot_SingleTarget(
																				targetModMeshes[0],
																				(int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0,//Morph 타입인가
																				pasteSlotIndex_Main, selectedSlots,
																				pasteMethod,
																				isPasteProp_Verts,
																				isPasteProp_Pins,
																				isPasteProp_Transform,
																				isPasteProp_Visibility,
																				isPasteProp_Color,
																				isPasteProp_Extra,
																				isSelectedOnly, modVerts, modPins))
						{
							isResult = true;
						}
					}
				}
				else if (targetModMeshes.Count > 1)
				{
					if (nSelectedSlots == 1)
					{
						//[다중 메시 + 단일 슬롯]
						if(apSnapShotManager.I.Paste_ModMeshes_SingleSlot_MultipleTargets(
																				targetModMeshes,
																				pasteSlotIndex_Main,
																				(int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0,//Morph 타입인가
																				isPasteProp_Verts,
																				isPasteProp_Pins,
																				isPasteProp_Transform,
																				isPasteProp_Visibility,
																				isPasteProp_Color,
																				isPasteProp_Extra,
																				isSelectedOnly, modVerts, modPins))
						{
							isResult = true;
						}
					}
					else if(nSelectedSlots > 1)
					{
						//[다중 메시 + 다중 슬롯]
						if(apSnapShotManager.I.Paste_ModMeshes_MultipleSlot_MultipleTargets(
																				targetModMeshes,
																				(int)(Modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0,//Morph 타입인가
																				pasteSlotIndex_Main, selectedSlots,
																				pasteMethod,
																				isPasteProp_Verts,
																				isPasteProp_Pins,
																				isPasteProp_Transform,
																				isPasteProp_Visibility,
																				isPasteProp_Color,
																				isPasteProp_Extra,
																				isSelectedOnly, modVerts, modPins))
						{
							isResult = true;
						}
					}
				}
				
			}

			//Mod Bone >> Transform 붙여넣기를 지원하는 경우에만
			if (isPasteProp_Transform)
			{
				if (nModBones > 0 && ParamSetOfMod._boneData != null)
				{
					apModifiedBone curModBone = null;

					for (int i = 0; i < nModBones; i++)
					{
						curModBone = modBones[i];
						if (curModBone != null && ParamSetOfMod._boneData.Contains(curModBone))
						{
							targetModBones.Add(curModBone);
						}
					}

					if (targetModBones.Count == 1)
					{
						if (nSelectedSlots == 1)
						{
							//[단일 본 + 단일 슬롯]
							if (apSnapShotManager.I.Paste_ModBone_SingleSlot_SingleTarget(targetModBones[0],
																							pasteSlotIndex_Main))
							{
								isResult = true;
							}
						}
						else if (nSelectedSlots > 1)
						{
							//[단일 본 + 다중 슬롯]
							if (apSnapShotManager.I.Paste_ModBone_MultipleSlot_SingleTarget(targetModBones[0],
																								pasteSlotIndex_Main,
																								selectedSlots,
																								pasteMethod))
							{
								isResult = true;
							}
						}
					}
					else if (targetModBones.Count > 1)
					{
						if (nSelectedSlots == 1)
						{
							//[다중 본 + 단일 슬롯]
							if (apSnapShotManager.I.Paste_ModBones_SingleSlot_MultipleTargets(targetModBones,
																								pasteSlotIndex_Main))
							{
								isResult = true;
							}
						}
						else if (nSelectedSlots > 1)
						{
							//[다중 본 + 다중 슬롯]
							if (apSnapShotManager.I.Paste_ModBones_MultipleSlot_MultipleTargets(targetModBones,
																								pasteSlotIndex_Main,
																								selectedSlots,
																								pasteMethod))
							{
								isResult = true;
							}
						}
					}
				}
			}


			if (!isResult)
			{
				Editor.Notification("Paste Failed", true, false);
			}
			MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}


		
		/// <summary>
		/// 메시 그룹 Right 2 UI > Modifier 탭 > 속성 UI [키프레임에 의한 모디파이어]
		/// </summary>
		private void DrawModifierPropertyGUI_KeyframeParamSet(int width, int height, string recordName)
		{	
			//"Animation Clips"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AnimationClips), apGUILOFactory.I.Width(width));

			GUILayout.Space(5);

			// 생성된 ParamSet Group을 선택하자
			//------------------------------------------------------------------
			// AnimClip에 따른 Param Set Group Anim Pack 리스트
			//------------------------------------------------------------------
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(120));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			Color prevColor = GUI.backgroundColor;

			GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, 120), apStringFactory.I.None);
			GUI.backgroundColor = prevColor;


			List<apModifierParamSetGroupAnimPack> paramSetGroupAnimPacks = Modifier._paramSetGroupAnimPacks;


			//등록된 Keyframe Param Group 리스트를 출력하자
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(120));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(scrollWidth), apGUILOFactory.I.Height(120));
			GUILayout.Space(3);


			//이전
			//Texture2D animIconImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);

			//추가
			if (_guiContent_Modifier_AnimIconText == null)
			{
				_guiContent_Modifier_AnimIconText = new apGUIContentWrapper();
				_guiContent_Modifier_AnimIconText.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation));
			}
			

			////현재 선택중인 파라미터 그룹
			//apModifierParamSetGroupAnimPack curParamSetGroupAnimPack = SubEditedParamSetGroupAnimPack;

			GUIStyle curGUIStyle = null;//최적화된 코드
			for (int i = 0; i < paramSetGroupAnimPacks.Count; i++)
			{
				curGUIStyle = apGUIStyleWrapper.I.None_LabelColor;

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(scrollWidth - 5));
				GUILayout.Space(5);

				_guiContent_Modifier_AnimIconText.SetText(1, paramSetGroupAnimPacks[i].LinkedAnimClip._name);
				if (GUILayout.Button(_guiContent_Modifier_AnimIconText.Content, curGUIStyle,
									apGUILOFactory.I.Width(scrollWidth - (5)), apGUILOFactory.I.Height(20)))
				{
					//이전 : 클릭하면 선택을 한다.
					//SetParamSetGroupAnimPackOfModifier(paramSetGroupAnimPacks[i]);
					//Editor.RefreshControllerAndHierarchy(false);

					//변경 20.4.4 : 이걸 선택해서 사용할 수 있는 기능도 없고 AnimParamSetGroup이 Link가 안되어 있을 수 있다.
					//따라서 선택 기능을 아예 없앤다.
				}
				EditorGUILayout.EndHorizontal();
			}


			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			//------------------------------------------------------------------ < Param Set Group 리스트

			
		}

		

		//Rigging Modifier UI를 출력한다.
		/// <summary>
		/// 메시 그룹 Right 2 UI > Modifier 탭 > 속성 UI [리깅 모디파이어]
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="recordName"></param>
		private void DrawModifierPropertyGUI_Rigging(int width, int height, string recordName)
		{	
			//"Target Mesh Transform" > 생략 20.3.29
			//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TargetMeshTransform), apGUILOFactory.I.Width(width));

			//1. Mesh Transform 등록 체크
			//2. Weight 툴
			// 선택한 Vertex
			// Auto Normalize
			// Set Weight, +/- Weight, * Weight
			// Blend, Auto Rigging, Normalize, Prune,
			// Copy / Paste
			// Bone (Color, Remove)

			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isContainInParamSetGroup = false;
			
			//string strTargetName = "";
			bool isTargetName = false;

			if(_guiContent_ModProp_ParamSetTarget_Name == null)
			{
				_guiContent_ModProp_ParamSetTarget_Name = new apGUIContentWrapper();
			}
			if(_guiContent_ModProp_ParamSetTarget_StatusText == null)
			{
				_guiContent_ModProp_ParamSetTarget_StatusText = new apGUIContentWrapper();
			}
			_guiContent_ModProp_ParamSetTarget_Name.ClearText(false);
			_guiContent_ModProp_ParamSetTarget_StatusText.ClearText(false);


			object selectedObj = null;
			bool isAnyTargetSelected = false;
			bool isAddable = false;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			apTransform_Mesh targetMeshTransform = MeshTF_Main;
			apModifierParamSetGroup paramSetGroup = SubEditedParamSetGroup;
			if (paramSetGroup == null)
			{
				//? Rigging에서는 ParamSetGroup이 있어야 한다.
				Editor.Controller.AddStaticParamSetGroupToModifier();

				if (Modifier._paramSetGroup_controller.Count > 0)
				{
					SelectParamSetGroupOfModifier(Modifier._paramSetGroup_controller[0]);
				}
				paramSetGroup = SubEditedParamSetGroup;
				if (paramSetGroup == null)
				{
					Debug.LogError("AnyPortrait : ParamSet Group Is Null (" + Modifier._paramSetGroup_controller.Count + ")");
					return;
				}

				AutoSelectModMeshOrModBone();
			}
			apModifierParamSet paramSet = ParamSetOfMod;
			if (paramSet == null)
			{
				//Rigging에서는 1개의 ParamSetGroup과 1개의 ParamSet이 있어야 한다.
				//선택된게 없다면, ParamSet이 1개 있는지 확인
				//그후 선택한다.

				if (paramSetGroup._paramSetList.Count == 0)
				{
					paramSet = new apModifierParamSet();
					paramSet.LinkParamSetGroup(paramSetGroup);
					paramSetGroup._paramSetList.Add(paramSet);
				}
				else
				{
					paramSet = paramSetGroup._paramSetList[0];
				}
				SelectParamSetOfModifier(paramSet);
			}



			//1. Mesh Transform 등록 체크
			if (targetMeshTransform != null)
			{
				apRenderUnit targetRenderUnit = null;
				//Child Mesh를 허용하는가
				if (isTarget_ChildMeshTransform)
				{
					//Child를 허용한다.
					targetRenderUnit = MeshGroup.GetRenderUnit(targetMeshTransform);
				}
				else
				{
					//Child를 허용하지 않는다.
					targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(targetMeshTransform);
				}
				if (targetRenderUnit != null)
				{
					//유효한 선택인 경우
					isContainInParamSetGroup = paramSetGroup.IsMeshTransformContain(targetMeshTransform);
					isAnyTargetSelected = true;
					
					//strTargetName = targetMeshTransform._nickName;
					_guiContent_ModProp_ParamSetTarget_Name.AppendText(targetMeshTransform._nickName, true);
					isTargetName = true;

					selectedObj = targetMeshTransform;

					isAddable = true;
				}
			}

			//대상이 없다면
			if(!isTargetName)
			{
				_guiContent_ModProp_ParamSetTarget_Name.AppendText(apStringFactory.I.None, true);
			}


			if (Event.current.type == EventType.Layout ||
				Event.current.type == EventType.Repaint)
			{
				_riggingModifier_prevSelectedTransform = targetMeshTransform;
				_riggingModifier_prevIsContained = isContainInParamSetGroup;
			}
			bool isSameSetting = (targetMeshTransform == _riggingModifier_prevSelectedTransform)
								&& (isContainInParamSetGroup == _riggingModifier_prevIsContained);


			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check__Rigging, isSameSetting);//"Modifier_Add Transform Check [Rigging]



			if (!Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check__Rigging))//"Modifier_Add Transform Check [Rigging]
			{
				return;
			}

			Color prevColor = GUI.backgroundColor;

			//GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
			//boxGUIStyle.alignment = TextAnchor.MiddleCenter;
			//boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

			if (targetMeshTransform == null)
			{
				// [ 선택된 MeshTF가 없다 ] 메시지
				//선택된 MeshTransform이 없다.

				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//"No Mesh is Selected"
				GUILayout.Box(Editor.GetUIWord(UIWORD.NoMeshIsSelected), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;
			}
			else if (isContainInParamSetGroup)
			{
				// [ 리깅에서 제외하기 ] 버튼
				// 이미 등록되어 있다.

				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nSelected"

				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.Selected), true);

				//GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.Selected), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;

				//"  Remove From Rigging"

				//이전
				//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveFromRigging), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromRigging)), GUILayout.Width(width), GUILayout.Height(30)))

				//변경
				if (_guiContent_Modifier_RemoveFromRigging == null)
				{
					_guiContent_Modifier_RemoveFromRigging = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveFromRigging), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromRigging));
				}
				
				if (GUILayout.Button(_guiContent_Modifier_RemoveFromRigging.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
				{

					//bool result = EditorUtility.DisplayDialog("Remove From Rigging", "Remove From Rigging [" + strTargetName + "]", "Remove", "Cancel");

					bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveFromRigging_Title),
																Editor.GetTextFormat(TEXT.RemoveFromRigging_Body, _guiContent_ModProp_ParamSetTarget_Name.Content.text),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

					if (result)
					{
						object targetObj = MeshTF_Main;
						if (MeshGroupTF_Main != null && selectedObj == MeshGroupTF_Main)
						{
							targetObj = MeshGroupTF_Main;
						}

						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_RemoveBoneRigging, 
															Editor, 
															Modifier, 
															//targetObj, 
															false,
															apEditorUtil.UNDO_STRUCT.StructChanged);

						if (MeshTF_Main != null && selectedObj == MeshTF_Main)
						{
							SubEditedParamSetGroup.RemoveModifierMeshes(MeshTF_Main);
						}
						else if (MeshGroupTF_Main != null && selectedObj == MeshGroupTF_Main)
						{
							SubEditedParamSetGroup.RemoveModifierMeshes(MeshGroupTF_Main);
						}
						else
						{
							//TODO : Bone 제거
						}

						Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_Modifier(MeshGroup, Modifier));
						AutoSelectModMeshOrModBone();

						Editor.Hierarchy_MeshGroup.RefreshUnits();
						Editor.RefreshControllerAndHierarchy(false);

						//추가 21.1.32 : Rule 가시성 동기화 초기화 / 추가 삭제 코드가 왜 없었지
						Editor.Controller.ResetVisibilityPresetSync();
						Editor.OnAnyObjectAddedOrRemoved();

						//추가 21.2.17
						RefreshMeshGroupExEditingFlags(true);

						Editor.SetRepaint();
					}
				}
			}
			else if (!isAddable)
			{
				// [ 추가 불가 ] 메시지
				//추가 가능하지 않다.

				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.NotAbleToBeAdded), true);

				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot able to be Added"
				//GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				// [ Rigging에 추가하기 ] 버튼

				//아직 추가하지 않았다. 추가하자

				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.NotAddedtoEdit), true);

				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot Added to Edit"
				//GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAddedtoEdit), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;

				//"  Add Rigging" -> "  Add to Rigging"
				//이전
				//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToRigging), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToRigging)), GUILayout.Width(width), GUILayout.Height(30)))

				//변경
				if(_guiContent_Modifier_AddToRigging == null)
				{
					_guiContent_Modifier_AddToRigging = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AddToRigging), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToRigging));
				}

				
				//Add To Rigging 버튼은 반짝거린다.
				GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();

				if (GUILayout.Button(_guiContent_Modifier_AddToRigging.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
				{
					apModifiedMesh newModMesh = Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

					Editor.Hierarchy_MeshGroup.RefreshUnits();

					Editor.SetRepaint();

					//추가 11.7 : 만약 Rig Edit 모드가 아니면, Rig Edit모드로 바로 활성화
					if(!IsRigEditBinding)
					{
						ToggleRigEditBinding();
					}

					//추가 22.7.13 [v1.4.0]
					//빠른 접근 위해 바로 모든 버텍스를 선택한다.
					if(newModMesh != null && 
						newModMesh == Editor.Select.ModMesh_Main)
					{
						//버텍스를 모두 선택한다. 해당 코드는 Rigging Gizmo Controller의 Ctrl+A의 코드 참조
						Editor.Select.AddModRenderVertsOfModifier(Editor.Select.ModData.ModRenderVert_All);
						Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
						Editor.Select.AutoSelectModMeshOrModBone();
					}

				}
				GUI.backgroundColor = prevColor;
			}
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			List<ModRenderVert> selectedVerts = Editor.Select.ModRenderVerts_All;
			//bool isAnyVertSelected = (selectedVerts != null && selectedVerts.Count > 0);


			//2. Weight 툴
			// 선택한 Vertex
			// Auto Normalize
			// Set Weight, +/- Weight, * Weight
			// Blend, Auto Rigging, Normalize, Prune,
			// Copy / Paste
			// Bone (Color, Remove)

			//어떤 Vertex가 선택되었는지 표기한다.

			_rigEdit_vertRigDataList.Clear();
			VertRigData curBoneRigData = null;
			
			int nSelectedVerts = 0;
			if (isAnyTargetSelected)
			{
				nSelectedVerts = selectedVerts.Count;

				//리스트에 넣을 Rig 리스트를 완성하자
				for (int i = 0; i < selectedVerts.Count; i++)
				{
					apModifiedVertexRig modVertRig = selectedVerts[i]._modVertRig;
					if (modVertRig == null)
					{
						// -ㅅ-?
						continue;
					}
					for (int iPair = 0; iPair < modVertRig._weightPairs.Count; iPair++)
					{
						apModifiedVertexRig.WeightPair pair = modVertRig._weightPairs[iPair];
						VertRigData targetBoneData = _rigEdit_vertRigDataList.Find(delegate (VertRigData a)
						{
							return a._bone == pair._bone;
						});

						if (targetBoneData != null)
						{
							targetBoneData.AddRig(pair._weight);
						}
						else
						{
							targetBoneData = new VertRigData(pair._bone, pair._weight);
							_rigEdit_vertRigDataList.Add(targetBoneData);
						}

						if(Bone != null && targetBoneData._bone == Bone)
						{
							curBoneRigData = targetBoneData;
						}
					}
				}
			}

			//추가 19.7.27 : RigLock에 따라 최대 가중치가 1이 아닌 그 이하의 값일 수 있다.
			float maxRigWeight = GetMaxRigWeight(curBoneRigData);


			//-----------------------------------------------
			//2. 리깅 정보 리스트 (20.3.29 : 아래에서 위로 올라옴)
			//-----------------------------------------------
			int rigListHeight = 150;//200 > 150
			int nRigDataList = _rigEdit_vertRigDataList.Count;
			if (_riggingModifier_prevNumBoneWeights != nRigDataList)
			{
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Rig_Mod__RigDataCount_Refreshed, true);//"Rig Mod - RigDataCount Refreshed"
				if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Rig_Mod__RigDataCount_Refreshed))//"Rig Mod - RigDataCount Refreshed"
				{
					_riggingModifier_prevNumBoneWeights = nRigDataList;
				}
			}
			else
			{
				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Rig_Mod__RigDataCount_Refreshed, false);//"Rig Mod - RigDataCount Refreshed"
			}

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(rigListHeight));
			GUILayout.Space(5);

			Rect lastRect = GUILayoutUtility.GetLastRect();

			if(isContainInParamSetGroup)
			{
				//Rigging에 등록된 상태라면 (유효함)
				GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
			}
			else
			{
				//이 Mesh TF가 Rigging에 등록되지 않았다면
				GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
			}

			GUI.Box(new Rect(lastRect.x + 5, lastRect.y, width, rigListHeight), apStringFactory.I.None);
			GUI.backgroundColor = prevColor;


			//Weight 리스트를 출력하자
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(rigListHeight));
			_scrollBottom_Status = EditorGUILayout.BeginScrollView(_scrollBottom_Status, false, true);
			GUILayout.Space(2);
			int scrollWidth = width - (30);
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(scrollWidth), apGUILOFactory.I.Height(rigListHeight));
			GUILayout.Space(3);

			Texture2D imgRemove = Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey);

			VertRigData vertRigData = null;

			if (_guiContent_RiggingBoneWeightLabel == null)
			{
				_guiContent_RiggingBoneWeightLabel = new apGUIContentWrapper();
			}
			if(_guiContent_RiggingBoneWeightBoneName == null)
			{
				_guiContent_RiggingBoneWeightBoneName = new apGUIContentWrapper();
			}

			//string strLabel = "";
			//선택, 삭제할 리깅 데이터를 선택하면 화면 하단에서 처리한다. (안그러면 UI가 꼬인다)
			VertRigData selectRigData = null;
			VertRigData removeRigData = null;
			int widthLabel_Name = scrollWidth - (5 + 25 + 14 + 2 + 60);

			//GUIStyle guiStyle_RigIcon_Normal = apEditorUtil.WhiteGUIStyle_Box;
			if (_guiStyle_RigIcon_Lock == null)
			{
				_guiStyle_RigIcon_Lock = new GUIStyle(GUI.skin.box);//<<최적화된 코드
				_guiStyle_RigIcon_Lock.normal.background = Editor.ImageSet.Get(apImageSet.PRESET.Rig_Lock16px);
			}
			

			GUIStyle curGUIStyle = null;//<<최적화된 코드

			//for (int i = 0; i < _rigEdit_vertRigDataList.Count; i++)
			for (int i = 0; i < _riggingModifier_prevNumBoneWeights; i++)
			{
				if (i < _rigEdit_vertRigDataList.Count)
				{
					//GUIStyle curGUIStyle = guiNone;
					vertRigData = _rigEdit_vertRigDataList[i];
					if (vertRigData._bone == Bone)
					{
						lastRect = GUILayoutUtility.GetLastRect();

						//if (EditorGUIUtility.isProSkin)
						//{
						//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
						//}
						//else
						//{
						//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
						//}

						int offsetHeight = 18 + 3;
						if (i == 0)
						{
							offsetHeight = 1 + 3;
						}

						//GUI.Box(new Rect(lastRect.x, lastRect.y + offsetHeight, scrollWidth + 35, 20), apStringFactory.I.None);
						//GUI.backgroundColor = prevColor;

						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(lastRect.x + 1, lastRect.y + offsetHeight, scrollWidth + 35 - 2, 20, apEditorUtil.UNIT_BG_STYLE.Main);


						//curGUIStyle = guiSelected;
						curGUIStyle = apGUIStyleWrapper.I.None_MiddleLeft_White2Cyan;
					}
					else
					{
						curGUIStyle = apGUIStyleWrapper.I.None_MiddleLeft_LabelColor;
					}


					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(scrollWidth - 5));
					GUILayout.Space(5);

					//Bone의 색상, 이름, Weight, X를 출력
					

					if(vertRigData._bone != null && vertRigData._bone._isRigLock)
					{
						GUILayout.Box(apStringFactory.I.None, _guiStyle_RigIcon_Lock, apGUILOFactory.I.Width(14), apGUILOFactory.I.Height(14));//자물쇠 이미지
					}
					else
					{
						GUI.backgroundColor = vertRigData._bone._color;
						GUILayout.Box(apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box, apGUILOFactory.I.Width(14), apGUILOFactory.I.Height(14));//일반 박스 이미지
						GUI.backgroundColor = prevColor;
					}

					_guiContent_RiggingBoneWeightLabel.ClearText(false);
					

					if (nSelectedVerts > 1 && (vertRigData._weight_Max - vertRigData._weight_Min) > 0.01f)
					{
						//여러개가 섞여서 Weight가 의미가 없어졌다.
						//Min + Max로 표현하자
						//strLabel = string.Format("{0:N2}~{1:N2}", vertRigData._weight_Min, vertRigData._weight_Max);
						_guiContent_RiggingBoneWeightLabel.AppendText(string.Format("{0:N2}~{1:N2}", vertRigData._weight_Min, vertRigData._weight_Max), true);
						
					}
					else
					{
						//Weight를 출력한다.
						//strLabel = ((int)vertRigData._weight) + "." + ((int)(vertRigData._weight * 1000.0f + 0.5f) % 1000);
						//strLabel = string.Format("{0:N3}", vertRigData._weight);
						_guiContent_RiggingBoneWeightLabel.AppendText(string.Format("{0:N3}", vertRigData._weight), true);
					}

					//이전
					//string rigName = vertRigData._bone._name;
					//if (rigName.Length > 14)
					//{
					//	rigName = rigName.Substring(0, 12) + "..";
					//}

					//변경
					_guiContent_RiggingBoneWeightBoneName.ClearText(false);
					if(vertRigData._bone._name.Length > 14)
					{
						_guiContent_RiggingBoneWeightBoneName.AppendText(vertRigData._bone._name.Substring(0, 12), false);
						_guiContent_RiggingBoneWeightBoneName.AppendText(apStringFactory.I.Dot2, true);
					}
					else
					{
						_guiContent_RiggingBoneWeightBoneName.AppendText(vertRigData._bone._name, true);
					}

					if (GUILayout.Button(_guiContent_RiggingBoneWeightBoneName.Content,
										curGUIStyle,
										apGUILOFactory.I.Width(widthLabel_Name), apGUILOFactory.I.Height(20)))
					{	
						//Editor.Select.SetBone(vertRigData._bone);//이전
						selectRigData = vertRigData;//변경 : 바로 SetBone을 호출하지 말자
					}
					if (GUILayout.Button(_guiContent_RiggingBoneWeightLabel.Content,
										curGUIStyle,
										apGUILOFactory.I.Width(60), apGUILOFactory.I.Height(20)))
					{
						//Editor.Select.SetBone(vertRigData._bone);
						selectRigData = vertRigData;//변경 : 바로 SetBone을 호출하지 말자
					}

					if (GUILayout.Button(imgRemove, curGUIStyle, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20)))
					{
						//Debug.LogError("TODO : Bone Remove From Rigging");
						removeRigData = vertRigData;
					}

					EditorGUILayout.EndHorizontal();
				}
				else
				{
					//GUI 렌더 문제로 더미 렌더링
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(scrollWidth - 5));
					GUILayout.Space(5);

					GUILayout.Box(apStringFactory.I.None, apGUILOFactory.I.Width(14), apGUILOFactory.I.Height(14));

					if (GUILayout.Button(apStringFactory.I.None,
										apGUIStyleWrapper.I.None_MiddleLeft_LabelColor,
										apGUILOFactory.I.Width(widthLabel_Name), apGUILOFactory.I.Height(20)))
					{
						//Dummy
					}
					if (GUILayout.Button(apStringFactory.I.None,
										apGUIStyleWrapper.I.None_MiddleLeft_LabelColor,
										apGUILOFactory.I.Width(60), apGUILOFactory.I.Height(20)))
					{
						//Dummy
					}

					if (GUILayout.Button(imgRemove, apGUIStyleWrapper.I.None_MiddleLeft_LabelColor, apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20)))
					{
						//Debug.LogError("TODO : Bone Remove From Rigging");
						//removeRigData = vertRigData;
						//Dummy
					}


					EditorGUILayout.EndHorizontal();
				}
			}

			EditorGUILayout.EndVertical();

			GUILayout.Space(120);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();


			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			int width_TabHalf = (width - (5)) / 2;

			//변경 19.7.25 : 버텍스 정보 + 본 정보와 Weight를 직접 설정할 수 있다.
			Color boxColor_VertexInfo = Color.black;
			//string str_VertexInfo = null;

			Color boxColor_BoneInfo = Color.black;
			//string str_BoneInfo = null;
			bool isBoneRigLock = false;

			if(_guiContent_ModProp_Rigging_VertInfo == null)
			{
				_guiContent_ModProp_Rigging_VertInfo = new apGUIContentWrapper();
			}
			if(_guiContent_ModProp_Rigging_BoneInfo == null)
			{
				_guiContent_ModProp_Rigging_BoneInfo = new apGUIContentWrapper();
			}
			
			_guiContent_ModProp_Rigging_VertInfo.ClearText(false);
			_guiContent_ModProp_Rigging_BoneInfo.ClearText(false);

			//버텍스 정보 :	박스 색상은 파란색 계열
			if (!isAnyTargetSelected || (selectedVerts != null && selectedVerts.Count == 0))
			{
				boxColor_VertexInfo = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//str_VertexInfo = "No Vertex";
				_guiContent_ModProp_Rigging_VertInfo.AppendText(apStringFactory.I.NoVertex, true);
			}
			else if (selectedVerts.Count == 1)
			{
				boxColor_VertexInfo = new Color(0.4f, 1.0f, 1.0f, 1.0f);
				//str_VertexInfo = "Vertex [" + selectedVerts[0]._renderVert._vertex._index + "]";
				_guiContent_ModProp_Rigging_VertInfo.AppendText(apStringFactory.I.VertexWithBracket, false);
				_guiContent_ModProp_Rigging_VertInfo.AppendText(selectedVerts[0]._renderVert._vertex._index, false);
				_guiContent_ModProp_Rigging_VertInfo.AppendText(apStringFactory.I.Bracket_2_R, true);
			}
			else
			{
				boxColor_VertexInfo = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//str_VertexInfo = selectedVerts.Count + " Vertices";
				_guiContent_ModProp_Rigging_VertInfo.AppendText(selectedVerts.Count, false);
				_guiContent_ModProp_Rigging_VertInfo.AppendText(apStringFactory.I.VerticesWithSpace, true);
			}

			//본 정보 : 본의 색상 이용 (밝기를 0.8 이상으로 맞춘다.)
			if(Bone == null)
			{
				boxColor_BoneInfo = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//str_BoneInfo = "No Bone";
				_guiContent_ModProp_Rigging_BoneInfo.AppendText(apStringFactory.I.NoBone, true);
			}
			else
			{
				boxColor_BoneInfo = Bone._color;
				boxColor_BoneInfo.r = Mathf.Max(boxColor_BoneInfo.r, 0.2f);
				boxColor_BoneInfo.g = Mathf.Max(boxColor_BoneInfo.g, 0.2f);
				boxColor_BoneInfo.b = Mathf.Max(boxColor_BoneInfo.b, 0.2f);

				float lum = (boxColor_BoneInfo.r * 0.5f + boxColor_BoneInfo.g * 0.3f + boxColor_BoneInfo.b * 0.2f);
				if(lum < 0.7f)
				{
					boxColor_BoneInfo.r *= 0.7f / lum;
					boxColor_BoneInfo.g *= 0.7f / lum;
					boxColor_BoneInfo.b *= 0.7f / lum;
				}

				//str_BoneInfo = Bone._name;
				_guiContent_ModProp_Rigging_BoneInfo.AppendText(Bone._name, true);

				isBoneRigLock = Bone._isRigLock;
			}


			//버텍스 정보와 본 정보를 2개의 박스로 표시

			//버텍스 선택 정보
			GUI.backgroundColor = boxColor_VertexInfo;
			GUILayout.Box(_guiContent_ModProp_Rigging_VertInfo.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
			GUI.backgroundColor = prevColor;

			//본 선택 정보 + 본의 Rig Lock
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(22));
			GUILayout.Space(4);

			GUI.backgroundColor = boxColor_BoneInfo;
			GUILayout.Box(_guiContent_ModProp_Rigging_BoneInfo.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 24), apGUILOFactory.I.Height(20));
			GUI.backgroundColor = prevColor;

			
			if(apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_Lock16px), 
													Editor.ImageSet.Get(apImageSet.PRESET.Rig_Unlock16px),
													isBoneRigLock,
													Bone != null && isContainInParamSetGroup,
													20, 22))
			{
				if(Bone != null)
				{
					Bone._isRigLock = !Bone._isRigLock;

					//Max 값을 바꾸자
					maxRigWeight = GetMaxRigWeight(curBoneRigData);

					apEditorUtil.ReleaseGUIFocus();
				}
			}

			EditorGUILayout.EndHorizontal();


			GUILayout.Space(2);

			//추가 19.7.25 : 현재 본과의 Weight를 직접 설정
			//다음의 3가지 상태가 있다.
			//- 편집 불가 상태 / 단일값의 Weight (버텍스가 여러개라도 Weight가 같은 경우) / 범위값의 Weight

			bool isRigUIInfo_MultipleVert = false;
			bool isRigUIInfo_SingleVert = false;
			bool isRigUIInfo_UnregRigData = false;
			int rigUIInfoMode = -1;

			if(isAnyTargetSelected && selectedVerts != null && selectedVerts.Count > 0)
			{
				if (curBoneRigData != null)
				{
					if (curBoneRigData._nRig > 1 && (curBoneRigData._weight_Max - curBoneRigData._weight_Min) > 0.01f)
					{
						isRigUIInfo_MultipleVert = true;
						rigUIInfoMode = 0;
					}
					else
					{
						isRigUIInfo_SingleVert = true;
						rigUIInfoMode = 1;
					}
				}
				else if(Bone != null)
				{
					isRigUIInfo_UnregRigData = true;
					rigUIInfoMode = 2;
				}
			}

			

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__MultipleVert, isRigUIInfo_MultipleVert);//"Rigging UI Info - MultipleVert"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__SingleVert, isRigUIInfo_SingleVert);//"Rigging UI Info - SingleVert"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__UnregRigData, isRigUIInfo_UnregRigData);//"Rigging UI Info - UnregRigData"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__SameMode, _riggingModifier_prevInfoMode == rigUIInfoMode);//"Rigging UI Info - SameMode"

			bool isSameRigUIInfoMode = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__SameMode);//"Rigging UI Info - SameMode"

			int height_VertRigInfo = 30;
			

			if(_strWrapper_64 == null)
			{
				_strWrapper_64 = new apStringWrapper(64);
			}

			if (isSameRigUIInfoMode)
			{
				if (isRigUIInfo_MultipleVert)
				{
					//범위값의 Weight : 범위값 알려주고 평균값으로 통일

					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__MultipleVert))//"Rigging UI Info - MultipleVert"
					{
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
						GUILayout.Space(5);

						_strWrapper_64.Clear();
						_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Weight), false);
						_strWrapper_64.Append(apStringFactory.I.Colon_Space, false);
						_strWrapper_64.Append(string.Format("{0:N2} ~ {1:N2}", curBoneRigData._weight_Min, curBoneRigData._weight_Max), true);

						//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Weight) + " : " + string.Format("{0:N2} ~ {1:N2}", curBoneRigData._weight_Min, curBoneRigData._weight_Max), apGUILOFactory.I.Width(width - 10));
						EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(width - 10));

						//float maxMultipleRigWeight = Mathf.Max(maxRigWeight, (curBoneRigData._weight_Min + curBoneRigData._weight_Max) / 2.0f);
						//if (GUILayout.Button(string.Format("Set {0:N2}", maxMultipleRigWeight), GUILayout.Height(18)))
						//{
						//	//평균값으로 적용한다.
						//	Editor.Controller.SetBoneWeight(maxMultipleRigWeight, 0, true);//True인자를 넣어서 다른 모든 Rig Weight가 0이 되었다고 해도 값이 할당될 수 있다.
						//}

						EditorGUILayout.EndHorizontal();
					}
					else
					{
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(apStringFactory.I.None);
						EditorGUILayout.EndHorizontal();
					}
				}
				else if (isRigUIInfo_SingleVert)
				{
					//단일값의 Weight : 슬라이더로 값 제어

					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__SingleVert))//"Rigging UI Info - SingleVert"
					{
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));


						EditorGUI.BeginChangeCheck();//수정 22.8.21 : 버그 수정
						float nextWeight = apEditorUtil.FloatSlider(Editor.GetUIWord(UIWORD.Weight), curBoneRigData._weight, 0.0f, maxRigWeight, width - 5, 80);
						if(EditorGUI.EndChangeCheck())
						{
							if(Mathf.Abs(nextWeight - curBoneRigData._weight) > 0.001f)
							{
								if (curBoneRigData._bone == Bone)
								{
									Editor.Controller.SetBoneWeight(nextWeight, 0, true, true);//True인자를 넣어서 다른 모든 Rig Weight가 0이 되었다고 해도 값이 할당될 수 있다.
								}
								apEditorUtil.ReleaseGUIFocus();
							}
						}


						EditorGUILayout.EndVertical();
					}
					else
					{
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(apStringFactory.I.None, apGUILOFactory.I.Width(80));
						GUILayout.HorizontalSlider(0, 0, 1);
						EditorGUILayout.FloatField(0);
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndVertical();
					}
				}
				else if (isRigUIInfo_UnregRigData)
				{
					//Rig Data만 없다.

					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Rigging_UI_Info__UnregRigData))//"Rigging UI Info - UnregRigData"
					{
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
						GUILayout.Space(5);
						if (GUILayout.Button(Editor.GetUIWord(UIWORD.RegisterWithRigging), apGUILOFactory.I.Height(18)))//"Register With Rigging"
						{
							//0의 값을 넣고 등록
							Editor.Controller.SetBoneWeight(0.0f, 0);
							apEditorUtil.ReleaseGUIFocus();
						}
						EditorGUILayout.EndHorizontal();
					}
					else
					{
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
						GUILayout.Space(5);
						GUILayout.Button(apStringFactory.I.None);
						EditorGUILayout.EndHorizontal();
					}
				}
				else
				{
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
					GUILayout.Space(5);
					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				//더미 UI
				switch (_riggingModifier_prevInfoMode)
				{
					case 0://MultipleVert의 더미
						{
							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
							GUILayout.Space(5);

							EditorGUILayout.LabelField(apStringFactory.I.None, apGUILOFactory.I.Width(width - 10));
							//GUILayout.Button("", GUILayout.Height(18));

							EditorGUILayout.EndHorizontal();
						}
						break;

					case 1://SingleVert의 더미
						{
							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
							GUILayout.Space(5);

							EditorGUILayout.LabelField(apStringFactory.I.None, apGUILOFactory.I.Width(80));
							GUILayout.HorizontalSlider(0, 0, 1);
							EditorGUILayout.FloatField(0);
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.EndVertical();
						}
						break;

					case 2://NoReg의 더미
						{
							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
							GUILayout.Space(5);
							GUILayout.Button(apStringFactory.I.None, apGUILOFactory.I.Height(18));
							EditorGUILayout.EndHorizontal();
						}
						break;
					default:
						{
							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height_VertRigInfo));
							GUILayout.Space(5);
							EditorGUILayout.EndHorizontal();
						}
						break;
				}
			}
			

			if(Event.current.type != EventType.Layout)
			{
				_riggingModifier_prevInfoMode = rigUIInfoMode;
			}
			

			//GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);



			//단축키 : Z, X로 가중치 증감 (20.3.29)
			//- Z키 : 감소, X키 : 증가 (값은 Shift를 누른 경우 0.05, 그냥은 0.02)
			if (isContainInParamSetGroup)
			{
				//메시가 리깅에 등록된 경우에만 단축키 작동
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingValueChanged_05, apHotKeyMapping.KEY_TYPE.Rig_IncreaseWeight_05, true);
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingValueChanged_02, apHotKeyMapping.KEY_TYPE.Rig_IncreaseWeight_02, true);
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingValueChanged_05, apHotKeyMapping.KEY_TYPE.Rig_DecreaseWeight_05, false);
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingValueChanged_02, apHotKeyMapping.KEY_TYPE.Rig_DecreaseWeight_02, false);
			}

			// < 기본 토대는 3ds Max와 유사하게 가자 >


			// Edit가 활성화되지 않으면 버튼 선택불가
			bool isBtnAvailable = _exclusiveEditing == EX_EDIT.ExOnly_Edit && isContainInParamSetGroup;//변경 22.5.15 / 22.6.10 : 편집 모드값 공유

			//변경 19.7.24 : 모드에 따라서 숫자 툴로 설정할지, 브러시 툴로 설정할지 결정
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(22));
			GUILayout.Space(5);

			// "Weight" 모드
			if(apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_WeightMode16px), 
											1, Editor.GetUIWord(UIWORD.Numpad),  
											_rigEdit_WeightToolMode == RIGGING_WEIGHT_TOOL_MODE.NumericTool, true, width_TabHalf, 22))
			{
				_rigEdit_WeightToolMode = RIGGING_WEIGHT_TOOL_MODE.NumericTool;
				_rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None;
				Editor.Gizmos.EndBrush();
			}

			// "Brush" 모드
			if(apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_PaintMode16px), 
											1, Editor.GetUIWord(UIWORD.Brush),  
											_rigEdit_WeightToolMode == RIGGING_WEIGHT_TOOL_MODE.BrushTool, true, width_TabHalf, 22))
			{
				if (_rigEdit_WeightToolMode != RIGGING_WEIGHT_TOOL_MODE.BrushTool)
				{
					_rigEdit_WeightToolMode = RIGGING_WEIGHT_TOOL_MODE.BrushTool;
					_rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None;
					Editor.Gizmos.EndBrush();
				}
				
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(4);

			//변경 19.7.24 : 모드에 따라서 숫자 툴로 설정할지, 브러시 툴로 설정할지 결정
			if (_rigEdit_WeightToolMode == RIGGING_WEIGHT_TOOL_MODE.NumericTool)
			{
				//기존의 "숫자 가중치 툴"

				int CALCULATE_SET = 0;
				int CALCULATE_ADD = 1;
				int CALCULATE_MULTIPLY = 2;

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
				GUILayout.Space(5);
				//고정된 Weight 값
				//0, 0.1, 0.3, 0.5, 0.7, 0.9, 1 (7개)
				int widthPresetWeight = ((width - 2 * 7) / 7) - 2;
				bool isPresetAdapt = false;
				float presetWeight = 0.0f;
				if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_00, false, isBtnAvailable, widthPresetWeight, 30))//"0"
				{
					isPresetAdapt = true;
					presetWeight = 0.0f;
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_01, false, isBtnAvailable, widthPresetWeight, 30))//".1"
				{
					isPresetAdapt = true;
					presetWeight = 0.1f;
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_03, false, isBtnAvailable, widthPresetWeight, 30))//".3"
				{
					isPresetAdapt = true;
					presetWeight = 0.3f;
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_05, false, isBtnAvailable, widthPresetWeight, 30))//".5"
				{
					isPresetAdapt = true;
					presetWeight = 0.5f;
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_07, false, isBtnAvailable, widthPresetWeight, 30))//".7"
				{
					isPresetAdapt = true;
					presetWeight = 0.7f;
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_09, false, isBtnAvailable, widthPresetWeight, 30))//".9"
				{
					isPresetAdapt = true;
					presetWeight = 0.9f;
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_10, false, isBtnAvailable, widthPresetWeight, 30))//"1"
				{
					isPresetAdapt = true;
					presetWeight = 1f;
				}
				EditorGUILayout.EndHorizontal();

				if (isPresetAdapt)
				{
					Editor.Controller.SetBoneWeight(presetWeight, CALCULATE_SET);
				}

				int heightSetWeight = 25;
				int widthSetBtn = 90;
				int widthIncDecBtn = 30;
				int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightSetWeight));
				GUILayout.Space(5);

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(widthValue), apGUILOFactory.I.Height(heightSetWeight - 2));
				GUILayout.Space(8);
				_rigEdit_setWeightValue = EditorGUILayout.DelayedFloatField(_rigEdit_setWeightValue);
				EditorGUILayout.EndVertical();

				//"Set Weight"
				if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.SetWeight), false, isBtnAvailable, widthSetBtn, heightSetWeight))
				{
					//Debug.LogError("TODO : Weight 적용 - Set");
					Editor.Controller.SetBoneWeight(_rigEdit_setWeightValue, CALCULATE_SET);
					GUI.FocusControl(null);
				}

				if (apEditorUtil.ToggledButton(apStringFactory.I.Plus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"+"
				{
					////0.05 단위로 올라가거나 내려온다. (5%)
					////현재 값에서 "int형 반올림"을 수행하고 처리
					//_rigEdit_setWeightValue = Mathf.Clamp01((float)((int)(_rigEdit_setWeightValue * 20.0f + 0.5f) + 1) / 20.0f);
					//이게 아니었다..
					//0.05 추가
					Editor.Controller.SetBoneWeight(0.05f, CALCULATE_ADD);

					GUI.FocusControl(null);
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Minus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"-"
				{
					//0.05 단위로 올라가거나 내려온다. (5%)
					//현재 값에서 "int형 반올림"을 수행하고 처리
					//_rigEdit_setWeightValue = Mathf.Clamp01((float)((int)(_rigEdit_setWeightValue * 20.0f + 0.5f) - 1) / 20.0f);
					//0.05 빼기
					Editor.Controller.SetBoneWeight(-0.05f, CALCULATE_ADD);

					GUI.FocusControl(null);
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(3);

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightSetWeight));
				GUILayout.Space(5);


				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(widthValue), apGUILOFactory.I.Height(heightSetWeight - 2));
				GUILayout.Space(8);
				_rigEdit_scaleWeightValue = EditorGUILayout.DelayedFloatField(_rigEdit_scaleWeightValue);
				EditorGUILayout.EndVertical();

				//"Scale Weight"
				if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.ScaleWeight), false, isBtnAvailable, widthSetBtn, heightSetWeight))
				{
					//Debug.LogError("TODO : Weight 적용 - Set");
					Editor.Controller.SetBoneWeight(_rigEdit_scaleWeightValue, CALCULATE_MULTIPLY);//Multiply 방식
					GUI.FocusControl(null);
				}

				if (apEditorUtil.ToggledButton(apStringFactory.I.Plus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"+"
				{
					//0.01 단위로 올라가거나 내려온다. (1%)
					//현재 값에서 반올림을 수행하고 처리
					//Scale은 Clamp가 걸리지 않는다.
					
					//x1.05
					Editor.Controller.SetBoneWeight(1.05f, CALCULATE_MULTIPLY);//Multiply 방식

					GUI.FocusControl(null);
				}
				if (apEditorUtil.ToggledButton(apStringFactory.I.Minus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"-"
				{
					//0.01 단위로 올라가거나 내려온다. (1%)
					//현재 값에서 반올림을 수행하고 처리
					
					//x0.95
					Editor.Controller.SetBoneWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식

					GUI.FocusControl(null);
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				//추가 19.7.24 : 리깅툴 v2의 브러시툴
				//- 브러시모드 : Add, Multiply, Blur
				//- 크기와 커브는 공유, 값은 별도
				//- 우클릭시 모드 취소 ("값"은 더미값이 들어간다.)
				//- 버텍스가 선택 안된 경우 모드 비활성화
				//- 브러시 단축키와 동일
				//- 이 모드 활성시 업데이트가 풀파워로 가동 (절전 모드시)
				//- 브러시 모드가 켜진 상태에서는 버텍스 선택이 불가능. 우클릭으로 모드 해제해야..

				//브러시 모드 선택
				int width_BrushTabBtn = ((width - 5) / 3) - 2;

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
				GUILayout.Space(5);
				if(apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_BrushAdd), 
														_rigEdit_BrushToolMode == RIGGING_BRUSH_TOOL_MODE.Add, 
														isBtnAvailable && Bone != null, 
														width_BrushTabBtn, 25))
				{
					if(isBtnAvailable && Bone != null)
					{
						if(_rigEdit_BrushToolMode != RIGGING_BRUSH_TOOL_MODE.Add)	{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.Add; Editor.Gizmos.StartBrush(); }
						else														{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None; Editor.Gizmos.EndBrush(); }
					}
				}
				if(apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_BrushMultiply), 
														_rigEdit_BrushToolMode == RIGGING_BRUSH_TOOL_MODE.Multiply, 
														isBtnAvailable && Bone != null, 
														width_BrushTabBtn, 25))
				{
					if(isBtnAvailable && Bone != null)
					{
						if(_rigEdit_BrushToolMode != RIGGING_BRUSH_TOOL_MODE.Multiply)	{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.Multiply; Editor.Gizmos.StartBrush(); }
						else															{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None; Editor.Gizmos.EndBrush(); }
					}
				}
				if(apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_BrushBlur), 
														_rigEdit_BrushToolMode == RIGGING_BRUSH_TOOL_MODE.Blur, 
														isBtnAvailable && Bone != null, 
														width_BrushTabBtn, 25))
				{
					if(isBtnAvailable && Bone != null)
					{
						if(_rigEdit_BrushToolMode != RIGGING_BRUSH_TOOL_MODE.Blur)	{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.Blur; Editor.Gizmos.StartBrush(); }
						else														{ _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None; Editor.Gizmos.EndBrush(); }
					}
				}
				
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(2);
				
				//브러시 사이즈
				//이전
				//_rigEdit_BrushRadius = apEditorUtil.IntSlider(Editor.GetUIWord(UIWORD.Radius), _rigEdit_BrushRadius, 1, apGizmos.MAX_BRUSH_RADIUS, width, 80);

				//변경 22.1.9 : 프리셋 인덱스로 변경
				//TODO : 크기 정보가 나타나야 하는데 인덱스가 나올 듯
				_rigEdit_BrushRadius_Index = apEditorUtil.IntSliderWithGhostLabel(	Editor.GetUIWord(UIWORD.Radius), 
																					_rigEdit_BrushRadius_Index, 
																					0, apGizmos.MAX_BRUSH_INDEX, 
																					apGizmos.GetBrushSizeByIndex(_rigEdit_BrushRadius_Index),
																					width, 80);
				_rigEdit_BrushRadius_Index = Mathf.Clamp(_rigEdit_BrushRadius_Index, 0, apGizmos.MAX_BRUSH_INDEX);

				//단축키
				
				//브러시 크기 : [, ]
				//변경 20.12.3
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingBrushSizeChanged, apHotKeyMapping.KEY_TYPE.Rig_IncreaseBrushSize, true);//"Increase Brush Radius"
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingBrushSizeChanged, apHotKeyMapping.KEY_TYPE.Rig_DecreaseBrushSize, false);//"Decrease Brush Radius"
				

				//브러시 모드 선택 : Add-J, Multiply-K, Blur-L
				//변경 20.12.3
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingBrushMode_Add,		apHotKeyMapping.KEY_TYPE.Rig_BrushMode_Add, null);//"Brush Mode - Add"
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingBrushMode_Multiply,	apHotKeyMapping.KEY_TYPE.Rig_BrushMode_Multiply, null);//"Brush Mode - Multiply"
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingBrushMode_Blur,		apHotKeyMapping.KEY_TYPE.Rig_BrushMode_Blur, null);//"Brush Mode - Blur"


				//브러시 세기 : <, >
				//변경 20.12.3
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingBrushIntensity, apHotKeyMapping.KEY_TYPE.Rig_IncreaseBrushIntensity, true);//"Increase Brush Intensity"
				Editor.AddHotKeyEvent(OnHotKeyEvent_RiggingBrushIntensity, apHotKeyMapping.KEY_TYPE.Rig_DecreaseBrushIntensity, false);//"Decrease Brush Intensity"


				//브러시 세기 (모드마다 다름)
				switch (_rigEdit_BrushToolMode)
				{
					case RIGGING_BRUSH_TOOL_MODE.None://툴이 선택 안된 상태
						apEditorUtil.IntSlider(Editor.GetUIWord(UIWORD.Intensity), 0, 0, 100, width, 80);
						break;
					case RIGGING_BRUSH_TOOL_MODE.Add:
						_rigEdit_BrushIntensity_Add = apEditorUtil.FloatSlider(Editor.GetUIWord(UIWORD.Intensity), _rigEdit_BrushIntensity_Add, -1, 1, width, 80);
						break;
					case RIGGING_BRUSH_TOOL_MODE.Multiply:
						_rigEdit_BrushIntensity_Multiply = apEditorUtil.FloatSlider(Editor.GetUIWord(UIWORD.Intensity), _rigEdit_BrushIntensity_Multiply, 0.5f, 1.5f, width, 80);
						break;
					case RIGGING_BRUSH_TOOL_MODE.Blur:
						_rigEdit_BrushIntensity_Blur = apEditorUtil.IntSlider(Editor.GetUIWord(UIWORD.Intensity), _rigEdit_BrushIntensity_Blur, 0, 100, width, 80);
						break;
				}
				
			}

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			int heightToolBtn = 25;
			//int width4Btn = ((width - 5) / 4) - (2);

			//Blend, Prune, Normalize, Auto Rigging
			//Normalize On/Off
			//Copy / Paste

			int width2Btn = (width - 5) / 2;

			//Auto Rigging
			//"  Auto Normalize", "  Auto Normalize"
			if (apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_AutoNormalize), 
													2, Editor.GetUIWord(UIWORD.AutoNormalize), Editor.GetUIWord(UIWORD.AutoNormalize), 
													_rigEdit_isAutoNormalize,
													isBtnAvailable,
													width, 28))
			{
				//만약 옵션을 끄는 거라면, 경고창을 띄우자 (22.5.18 v1.4.0)
				bool isToggleOption = false;
				bool nextAutoNormalize = !_rigEdit_isAutoNormalize;
				if(!nextAutoNormalize)
				{
					bool isDisable = EditorUtility.DisplayDialog(
											Editor.GetText(TEXT.DLG_StopRigAutoNormalize_Title),
											Editor.GetText(TEXT.DLG_StopRigAutoNormalize_Body),
											Editor.GetText(TEXT.DLG_StopRigAutoNormalize_Disable),
											Editor.GetText(TEXT.Cancel));

					if(isDisable)
					{
						//정말 끄려고 한다.
						isToggleOption = true;
					}

				}
				else
				{
					//켜는 거는 경고창 없이 ㄱㄱ
					isToggleOption = true;
				}

				if (isToggleOption)
				{
					_rigEdit_isAutoNormalize = !_rigEdit_isAutoNormalize;

					//Off -> On 시에 Normalize를 적용하자
					if (_rigEdit_isAutoNormalize)
					{
						Editor.Controller.SetBoneWeightNormalize();
					}
					//Auto Normalize는 에디터 옵션으로 저장된다.
					Editor.SaveEditorPref();
				}
			}


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Blend"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_Blend), 
											1, Editor.GetUIWord(UIWORD.Blend), 
											false, isBtnAvailable, width2Btn, heightToolBtn, 
											apStringFactory.I.RiggingTooltip_Blend))//"Blend the weights of vertices"
			{
				//Blend
				Editor.Controller.SetBoneWeightBlend();
			}
			//" Normalize"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_Normalize), 
											1, Editor.GetUIWord(UIWORD.Normalize), 
											false, isBtnAvailable, width2Btn, heightToolBtn, 
											apStringFactory.I.RiggingTooltip_Normalize))//"Normalize rigging weights"
			{
				//Normalize
				Editor.Controller.SetBoneWeightNormalize();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Prune"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_Prune), 
											1, Editor.GetUIWord(UIWORD.Prune), 
											false, isBtnAvailable, width2Btn, heightToolBtn, 
											apStringFactory.I.RiggingTooltip_Prune))//"Remove rigging bones its weight is under 0.01"
			{
				//Prune
				Editor.Controller.SetBoneWeightPrune();
			}
			//" Auto Rig"
			if (apEditorUtil.ToggledButton_Ctrl(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_Auto), 
											1, Editor.GetUIWord(UIWORD.AutoRig), 
											false, isBtnAvailable, width2Btn, heightToolBtn, 
											apStringFactory.I.RiggingTooltip_AutoRig,
											Event.current.control,
											Event.current.command))//"Rig Automatically"
			{
				//Auto
				//변경 19.12.29 : Ctrl 키를 누르면 본을 선택한다.
				//그냥 누르면 바로 AutoRig 실행
				if(isCtrl)
				{
					//apDialog_SelectMultipleObjects
					_loadKey_SelectBonesForAutoRig = apDialog_SelectBonesForAutoRig.ShowDialog(Editor, MeshGroup, targetMeshTransform, ModRenderVerts_All, OnSelectBonesForAutoRig);
				}
				else
				{	
					Editor.Controller.SetBoneAutoRig();
					//Editor.Controller.SetBoneAutoRig_Old();
				}
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Grow"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_Grow), 
											1, Editor.GetUIWord(UIWORD.Grow), 
											false, isBtnAvailable, width2Btn, heightToolBtn, 
											apStringFactory.I.RiggingTooltip_Grow))//"Select more of the surrounding vertices"
			{
				Editor.Controller.SelectVertexRigGrowOrShrink(true);
			}
			//" Shrink"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Rig_Shrink), 
											1, Editor.GetUIWord(UIWORD.Shrink), 
											false, isBtnAvailable, width2Btn, heightToolBtn, 
											apStringFactory.I.RiggingTooltip_Shrink))//"Reduce selected vertices"
			{
				Editor.Controller.SelectVertexRigGrowOrShrink(false);
			}
			EditorGUILayout.EndHorizontal();

			//추가 19.7.25 : 현재 선택된 본에 리깅된 모든 버텍스들을 선택하기
			//"Select Vertices of the Bone"
			if(apEditorUtil.ToggledButton_Ctrl(	Editor.GetUIWord(UIWORD.SelectVerticesOfTheBone), 
												false, isBtnAvailable && Bone != null, width, heightToolBtn, 
												apStringFactory.I.RiggingTooltip_SelectVerticesOfBone, //"Select vertices connected to the current bone. Hold down the Ctrl(or Command) key and press the button to select with existing vertices."
												Event.current.control, Event.current.command))
			{
				Editor.Controller.SelectVerticesOfTheBone();
			}
			//Editor.GetUIWord(UIWORD.AddToRigging)

			bool isCopyAvailable = isBtnAvailable && selectedVerts.Count == 1;
			
			bool isPasteAvailable = false;
			if (isCopyAvailable)
			{
				if (apSnapShotManager.I.IsPastable(selectedVerts[0]._modVertRig))
				{
					isPasteAvailable = true;
				}
			}

			bool isPosCopyAvailable = isBtnAvailable && selectedVerts.Count >= 1;
			bool isPosPasteAvailable = false;
			if(isPosCopyAvailable)
			{
				if (apSnapShotManager.I.IsRiggingPosPastable(MeshGroup, selectedVerts))
				{
					isPosPasteAvailable = true;
				}
			}

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightToolBtn));
			GUILayout.Space(5);
			//" Copy"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy), 
											1, Editor.GetUIWord(UIWORD.Copy), 
											false, isCopyAvailable, width2Btn, heightToolBtn))
			{
				//Copy	
				apSnapShotManager.I.Copy_VertRig(selectedVerts[0]._modVertRig, "Mod Vert Rig");
			}
			//" Paste"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste), 
											1, Editor.GetUIWord(UIWORD.Paste), 
											false, isPasteAvailable, width2Btn, heightToolBtn))
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_RiggingWeightChanged, 
													Editor, 
													Modifier, 
													//Modifier, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
				//Paste
				if (apSnapShotManager.I.Paste_VertRig(selectedVerts[0]._modVertRig))
				{
					MeshGroup.RefreshForce();
				}
			}
			EditorGUILayout.EndHorizontal();

			//추가 19.7.25 : Pos-Copy / Pos-Paste
			EditorGUILayout.BeginHorizontal(	apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightToolBtn));
			GUILayout.Space(5);
			
			//" Pos-Copy"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy), 
											1, Editor.GetUIWord(UIWORD.PosCopy), 
											false, isPosCopyAvailable, width2Btn, heightToolBtn))
			{
				//Pos-Copy	
				if(ModMesh_Main != null && ModMesh_Main._renderUnit != null)
				{
					ModMesh_Main._renderUnit.CalculateWorldPositionWithoutModifier();
					apSnapShotManager.I.Copy_MultipleVertRig(MeshGroup, selectedVerts);
				}
			}
			
			//" Pos-Paste"
			if (apEditorUtil.ToggledButton(	Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste), 
											1, Editor.GetUIWord(UIWORD.PosPaste), 
											false, isPosPasteAvailable, width2Btn, heightToolBtn))
			{
				//Pos-Paste	
				if (ModMesh_Main != null && ModMesh_Main._renderUnit != null)
				{
					ModMesh_Main._renderUnit.CalculateWorldPositionWithoutModifier();
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_RiggingWeightChanged, 
														Editor, 
														Modifier, 
														//Modifier, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					if (apSnapShotManager.I.Paste_MultipleVertRig(MeshGroup, selectedVerts))
					{	
						CheckLinkedToModMeshBones(true);//추가 21.3.15 : 이걸 호출해야 "등록된 본 외에 회색으로 표시"가 제대로 갱신된다.
						MeshGroup.RefreshForce();
					}
				}
			}
			EditorGUILayout.EndHorizontal();

			if(selectRigData != null && selectRigData._bone != null)
			{
				Editor.Select.SelectBone(selectRigData._bone, MULTI_SELECT.Main);
			}
			else if (removeRigData != null)
			{
				Editor.Controller.RemoveVertRigData(selectedVerts, removeRigData._bone);
			}

			
		}

		/// <summary>
		/// RigDataList를 바탕으로 설정 가능한 "최대의 공통 Weight"를 리턴한다.
		/// </summary>
		/// <param name="curRigData"></param>
		/// <returns></returns>
		private float GetMaxRigWeight(VertRigData curRigData)
		{
			if(_rigEdit_vertRigDataList.Count == 0 || curRigData == null)
			{
				return 1.0f;
			}

			VertRigData rigData = null;
			float lockedWeight = 0.0f;
			for (int i = 0; i < _rigEdit_vertRigDataList.Count; i++)
			{
				rigData = _rigEdit_vertRigDataList[i];
				if(rigData == curRigData)
				{
					continue;
				}
				else if(rigData._bone != null && rigData._bone._isRigLock)
				{
					//RigLock이 켜진 Bone을 찾는다.
					if(rigData._nRig == 1)
					{
						lockedWeight += rigData._weight;
					}
					else
					{
						lockedWeight += rigData._weight_Max;
					}
				}
			}
			
			return Mathf.Clamp01(1.0f - lockedWeight);
		}


		
		//Physics 모디파이어의 설정 화면
		
		/// <summary>
		/// 메시 그룹 Right 2 UI > Modifier 탭 > 속성 UI [Physics 모디파이어]
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		private void DrawModifierPropertyGUI_Physics(int width, int height)
		{
			//"Target Mesh Transform"
			//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TargetMeshTransform), apGUILOFactory.I.Width(width));
			//1. Mesh Transform 등록 체크
			//2. Weight 툴
			//3. Mesh Physics 툴

			bool isTarget_MeshTransform = Modifier.IsTarget_MeshTransform;
			bool isTarget_ChildMeshTransform = Modifier.IsTarget_ChildMeshTransform;

			bool isContainInParamSetGroup = false;
			
			
			//string strTargetName = "";
			bool isTargetName = false;

			if(_guiContent_ModProp_ParamSetTarget_Name == null)
			{
				_guiContent_ModProp_ParamSetTarget_Name = new apGUIContentWrapper();
			}
			if(_guiContent_ModProp_ParamSetTarget_StatusText == null)
			{
				_guiContent_ModProp_ParamSetTarget_StatusText = new apGUIContentWrapper();
			}
			_guiContent_ModProp_ParamSetTarget_Name.ClearText(false);
			_guiContent_ModProp_ParamSetTarget_StatusText.ClearText(false);


			object selectedObj = null;
			bool isAnyTargetSelected = false;
			bool isAddable = false;

			apTransform_Mesh targetMeshTransform = MeshTF_Main;
			apModifierParamSetGroup paramSetGroup = SubEditedParamSetGroup;
			if (paramSetGroup == null)
			{
				//? Physics에서는 1개의 ParamSetGroup이 있어야 한다.
				Editor.Controller.AddStaticParamSetGroupToModifier();

				if (Modifier._paramSetGroup_controller.Count > 0)
				{
					SelectParamSetGroupOfModifier(Modifier._paramSetGroup_controller[0]);
				}
				paramSetGroup = SubEditedParamSetGroup;
				if (paramSetGroup == null)
				{
					Debug.LogError("AnyPortrait : ParamSet Group Is Null (" + Modifier._paramSetGroup_controller.Count + ")");
					return;
				}

				AutoSelectModMeshOrModBone();
			}

			apModifierParamSet paramSet = ParamSetOfMod;
			if (paramSet == null)
			{
				//Rigging에서는 1개의 ParamSetGroup과 1개의 ParamSet이 있어야 한다.
				//선택된게 없다면, ParamSet이 1개 있는지 확인
				//그후 선택한다.

				if (paramSetGroup._paramSetList.Count == 0)
				{
					paramSet = new apModifierParamSet();
					paramSet.LinkParamSetGroup(paramSetGroup);
					paramSetGroup._paramSetList.Add(paramSet);
				}
				else
				{
					paramSet = paramSetGroup._paramSetList[0];
				}
				SelectParamSetOfModifier(paramSet);
			}

			//1. Mesh Transform 등록 체크
			if (targetMeshTransform != null)
			{
				apRenderUnit targetRenderUnit = null;
				//Child Mesh를 허용하는가
				if (isTarget_ChildMeshTransform)
				{
					//Child를 허용한다.
					targetRenderUnit = MeshGroup.GetRenderUnit(targetMeshTransform);
				}
				else
				{
					//Child를 허용하지 않는다.
					targetRenderUnit = MeshGroup.GetRenderUnit_NoRecursive(targetMeshTransform);
				}
				if (targetRenderUnit != null)
				{
					//유효한 선택인 경우
					isContainInParamSetGroup = paramSetGroup.IsMeshTransformContain(targetMeshTransform);
					isAnyTargetSelected = true;

					//strTargetName = targetMeshTransform._nickName;
					_guiContent_ModProp_ParamSetTarget_Name.AppendText(targetMeshTransform._nickName, true);
					isTargetName = true;


					selectedObj = targetMeshTransform;

					isAddable = true;
				}
			}

			//대상이 없다면
			if(!isTargetName)
			{
				_guiContent_ModProp_ParamSetTarget_Name.AppendText(apStringFactory.I.None, true);
			}

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check__Physic__Valid, targetMeshTransform != null);//"Modifier_Add Transform Check [Physic] Valid"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check__Physic__Invalid, targetMeshTransform == null);//"Modifier_Add Transform Check [Physic] Invalid"


			bool isMeshTransformValid = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check__Physic__Valid);//"Modifier_Add Transform Check [Physic] Valid"
			bool isMeshTransformInvalid = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_Add_Transform_Check__Physic__Invalid);//"Modifier_Add Transform Check [Physic] Invalid"

			
			bool isDummyTransform = false;

			if (!isMeshTransformValid && !isMeshTransformInvalid)
			{
				//둘중 하나는 true여야 GUI를 그릴 수 있다.
				isDummyTransform = true;//<<더미로 출력해야한다...
			}
			else
			{
				_physicModifier_prevSelectedTransform = targetMeshTransform;
				_physicModifier_prevIsContained = isContainInParamSetGroup;
			}



			Color prevColor = GUI.backgroundColor;

			//GUIStyle boxGUIStyle = new GUIStyle(GUI.skin.box);
			//boxGUIStyle.alignment = TextAnchor.MiddleCenter;
			//boxGUIStyle.normal.textColor = apEditorUtil.BoxTextColor;

			//추가
			if(_guiContent_Modifier_AddToPhysics == null)
			{
				_guiContent_Modifier_AddToPhysics = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AddToPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToPhysics));
			}
			if(_guiContent_Modifier_RemoveFromPhysics == null)
			{
				_guiContent_Modifier_RemoveFromPhysics = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveFromPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromPhysics));
			}
			
			

			if (targetMeshTransform == null)
			{
				//선택된 MeshTransform이 없다.
				GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//"No Mesh is Selected"
				GUILayout.Box(Editor.GetUIWord(UIWORD.NoMeshIsSelected), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;

				if (isDummyTransform)
				{
					//"  Add Physics"
					//이전
					//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToPhysics)), GUILayout.Width(width), GUILayout.Height(25)))
					
					//변경
					if (GUILayout.Button(_guiContent_Modifier_AddToPhysics.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))
					{
						//더미용 버튼
					}
				}
			}
			else if (isContainInParamSetGroup)
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);

				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.Selected), true);

				//"[" + strTargetName + "]\nSelected"
				//GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.Selected), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));


				GUI.backgroundColor = prevColor;

				if (!isDummyTransform)
				{
					//더미 처리 중이 아닐때 버튼이 등장한다
					//"  Remove From Physics".
					//이전
					//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveFromPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_RemoveFromPhysics)), GUILayout.Width(width), GUILayout.Height(30)))
					
					//변경
					if (GUILayout.Button(_guiContent_Modifier_RemoveFromPhysics.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
					{

						//bool result = EditorUtility.DisplayDialog("Remove From Physics", "Remove From Physics [" + strTargetName + "]", "Remove", "Cancel");

						bool result = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveFromPhysics_Title),
																Editor.GetTextFormat(TEXT.RemoveFromPhysics_Body, _guiContent_ModProp_ParamSetTarget_Name.Content.text),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

						if (result)
						{
							//object targetObj = MeshTF_Main;
							//if (MeshGroupTF_Main != null && selectedObj == MeshGroupTF_Main)
							//{
							//	targetObj = MeshGroupTF_Main;
							//}

							apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_RemovePhysics, 
																Editor, 
																Modifier, 
																//targetObj, 
																false,
																apEditorUtil.UNDO_STRUCT.StructChanged);

							if (MeshTF_Main != null && selectedObj == MeshTF_Main)
							{
								SubEditedParamSetGroup.RemoveModifierMeshes(MeshTF_Main);
								
								
								//SetModMeshOfModifier(null);//이전
								AutoSelectModMeshOrModBone();//변경 20.6.11 : 자동 선택으로 변경
							}
							else if (MeshGroupTF_Main != null && selectedObj == MeshGroupTF_Main)
							{
								SubEditedParamSetGroup.RemoveModifierMeshes(MeshGroupTF_Main);
								
								//SetModMeshOfModifier(null);//이전
								AutoSelectModMeshOrModBone();//변경 20.6.11 : 자동 선택으로 변경

							}



							if (MeshGroup != null)
							{
								MeshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(MeshGroup, Modifier));
							}

							SelectMeshGroupTF(null, MULTI_SELECT.Main);
							SelectMeshTF(null, MULTI_SELECT.Main);

							Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_MeshGroup_Modifier(MeshGroup, Modifier));
							AutoSelectModMeshOrModBone();

							SetModifierExclusiveEditing(EX_EDIT.None);

							if (ModMesh_Main != null)
							{
								ModMesh_Main.RefreshVertexWeights(Editor._portrait, true, false);
							}

							Editor.Hierarchy_MeshGroup.RefreshUnits();
							Editor.RefreshControllerAndHierarchy(false);

							//추가 21.1.32 : Rule 가시성 동기화 초기화 / 추가 삭제 코드가 왜 없었지
							Editor.Controller.ResetVisibilityPresetSync();
							Editor.OnAnyObjectAddedOrRemoved();


							//추가 21.2.17
							RefreshMeshGroupExEditingFlags(true);

							Editor.SetRepaint();

							isContainInParamSetGroup = false;
						}
					}
				}
			}
			else if (!isAddable)
			{
				//추가 가능하지 않다.

				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.NotAbleToBeAdded), true);


				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot able to be Added"
				//GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;

				if (isDummyTransform)
				{
					//이전
					//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.AddToPhysics), Editor.ImageSet.Get(apImageSet.PRESET.Modifier_AddToPhysics)), GUILayout.Width(width), GUILayout.Height(25)))
					//변경
					if (GUILayout.Button(_guiContent_Modifier_AddToPhysics.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))
					{
						//더미용 버튼
					}
				}
			}
			else
			{
				//아직 추가하지 않았다. 추가하자

				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_L, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(_guiContent_ModProp_ParamSetTarget_Name.Content.text, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Bracket_2_R, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(apStringFactory.I.Return, false);
				_guiContent_ModProp_ParamSetTarget_StatusText.AppendText(Editor.GetUIWord(UIWORD.NotAddedtoEdit), true);


				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"[" + strTargetName + "]\nNot Added to Edit"
				//GUILayout.Box("[" + strTargetName + "]\n" + Editor.GetUIWord(UIWORD.NotAddedtoEdit), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Box(_guiContent_ModProp_ParamSetTarget_StatusText.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

				GUI.backgroundColor = prevColor;

				if (!isDummyTransform)
				{
					//더미 처리 중이 아닐때 버튼이 등장한다.
					
					//Add To ..  버튼은 반짝인다.

					GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();

					if (GUILayout.Button(_guiContent_Modifier_AddToPhysics.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30)))
					{
						Editor.Controller.AddModMesh_WithSubMeshOrSubMeshGroup();

						Editor.Hierarchy_MeshGroup.RefreshUnits();

						//추가 3.24 : Physics에 등록했다면, Edit모드 시작
						if (ExEditingMode == EX_EDIT.None)
						{
							SetModifierExclusiveEditing(EX_EDIT.ExOnly_Edit);
						}

						Editor.SetRepaint();
					}

					GUI.backgroundColor = prevColor;
				}
			}
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			List<ModRenderVert> selectedVerts = Editor.Select.ModRenderVerts_All;
			//bool isAnyVertSelected = (selectedVerts != null && selectedVerts.Count > 0);

			bool isExEditMode = ExEditingMode != EX_EDIT.None;

			//2. Weight 툴
			// 선택한 Vertex
			// Set Weight, +/- Weight, * Weight
			// Blend
			// Grow, Shrink

			//어떤 Vertex가 선택되었는지 표기한다.
			if (!isAnyTargetSelected || selectedVerts.Count == 0)
			{
				//선택된게 없다.
				GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
				//"No Vetex is Selected"
				GUILayout.Box(Editor.GetUIWord(UIWORD.NoVertexisSelected), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));

				GUI.backgroundColor = prevColor;


			}
			else if (selectedVerts.Count == 1)
			{
				//1개의 Vertex
				GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				//"[Vertex " + selectedVerts[0]._renderVert._vertex._index + "] : " + selectedVerts[0]._modVertWeight._weight
				GUILayout.Box(string.Format("[ {0} {1} ] : {2}", Editor.GetUIWord(UIWORD.Vertex), selectedVerts[0]._renderVert._vertex._index, selectedVerts[0]._modVertWeight._weight), 
								apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));

				GUI.backgroundColor = prevColor;

			}
			else
			{
				GUI.backgroundColor = new Color(0.4f, 1.0f, 1.0f, 1.0f);
				//selectedVerts.Count + " Verts are Selected"
				GUILayout.Box(Editor.GetUIWordFormat(UIWORD.NumVertsareSelected, selectedVerts.Count), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));

				GUI.backgroundColor = prevColor;
			}
			int nSelectedVerts = selectedVerts.Count;

			bool isMainVert = false;
			bool isMainVertSwitchable = false;
			if (nSelectedVerts == 1)
			{
				if (selectedVerts[0]._modVertWeight._isEnabled)
				{
					isMainVert = selectedVerts[0]._modVertWeight._physicParam._isMain;
					isMainVertSwitchable = true;
				}
			}
			else if (nSelectedVerts > 1)
			{
				//전부다 MainVert인가
				bool isAllMainVert = true;
				for (int iVert = 0; iVert < selectedVerts.Count; iVert++)
				{
					if (!selectedVerts[iVert]._modVertWeight._physicParam._isMain)
					{
						isAllMainVert = false;
						break;
					}
				}
				isMainVert = isAllMainVert;
				isMainVertSwitchable = true;
			}

			//>> 여기서부터 하자


			//" Important Vertex", " Set Important",
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Physic_SetMainVertex),
												1, Editor.GetUIWord(UIWORD.ImportantVertex), Editor.GetUIWord(UIWORD.SetImportant),
												isMainVert,
												isMainVertSwitchable && isExEditMode && isContainInParamSetGroup,
												width, 25,
												"Force calculation is performed based on the [Important Vertex]"))
			{
				if (isMainVertSwitchable)
				{
					for (int i = 0; i < selectedVerts.Count; i++)
					{
						selectedVerts[i]._modVertWeight._physicParam._isMain = !isMainVert;
					}

					ModMesh_Main.RefreshVertexWeights(Editor._portrait, true, false);
				}
			}

			//Weight Tool
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
			GUILayout.Space(5);
			//고정된 Weight 값
			//0, 0.1, 0.3, 0.5, 0.7, 0.9, 1 (7개)
			int CALCULATE_SET = 0;
			int CALCULATE_ADD = 1;
			int CALCULATE_MULTIPLY = 2;

			int widthPresetWeight = ((width - 2 * 7) / 7) - 2;
			bool isPresetAdapt = false;
			float presetWeight = 0.0f;

			bool isBtnAvailable = isContainInParamSetGroup && isExEditMode;

			if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_00, false, isBtnAvailable, widthPresetWeight, 30))//"0"
			{
				isPresetAdapt = true;
				presetWeight = 0.0f;
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_01, false, isBtnAvailable, widthPresetWeight, 30))//".1"
			{
				isPresetAdapt = true;
				presetWeight = 0.1f;
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_03, false, isBtnAvailable, widthPresetWeight, 30))//".3"
			{
				isPresetAdapt = true;
				presetWeight = 0.3f;
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_05, false, isBtnAvailable, widthPresetWeight, 30))//".5"
			{
				isPresetAdapt = true;
				presetWeight = 0.5f;
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_07, false, isBtnAvailable, widthPresetWeight, 30))//".7"
			{
				isPresetAdapt = true;
				presetWeight = 0.7f;
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_09, false, isBtnAvailable, widthPresetWeight, 30))//".9"
			{
				isPresetAdapt = true;
				presetWeight = 0.9f;
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Weight_10, false, isBtnAvailable, widthPresetWeight, 30))//"1"
			{
				isPresetAdapt = true;
				presetWeight = 1f;
			}
			EditorGUILayout.EndHorizontal();

			if (isPresetAdapt)
			{
				//고정 Weight를 지정하자
				Editor.Controller.SetPhyVolWeight(presetWeight, CALCULATE_SET);
				isPresetAdapt = false;
			}



			int heightSetWeight = 25;
			int widthSetBtn = 90;
			int widthIncDecBtn = 30;
			int widthValue = width - (widthSetBtn + widthIncDecBtn * 2 + 2 * 5 + 5);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightSetWeight));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(widthValue), apGUILOFactory.I.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_physics_setWeightValue = EditorGUILayout.DelayedFloatField(_physics_setWeightValue);
			EditorGUILayout.EndVertical();
			
			//"Set Weight"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.SetWeight), false, isBtnAvailable, widthSetBtn, heightSetWeight))
			{
				Editor.Controller.SetPhyVolWeight(_physics_setWeightValue, CALCULATE_SET);
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton(apStringFactory.I.Plus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"+"
			{
				////0.05 단위로 올라가거나 내려온다. (5%)
				Editor.Controller.SetPhyVolWeight(0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Minus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"-"
			{
				//0.05 단위로 올라가거나 내려온다. (5%)
				Editor.Controller.SetPhyVolWeight(-0.05f, CALCULATE_ADD);

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(3);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightSetWeight));
			GUILayout.Space(5);


			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(widthValue), apGUILOFactory.I.Height(heightSetWeight - 2));
			GUILayout.Space(8);
			_physics_scaleWeightValue = EditorGUILayout.DelayedFloatField(_physics_scaleWeightValue);
			EditorGUILayout.EndVertical();

			//"Scale Weight"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.ScaleWeight), false, isBtnAvailable, widthSetBtn, heightSetWeight))
			{
				Editor.Controller.SetPhyVolWeight(_physics_scaleWeightValue, CALCULATE_MULTIPLY);//Multiply 방식
				GUI.FocusControl(null);
			}

			if (apEditorUtil.ToggledButton(apStringFactory.I.Plus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"+"
			{
				//x1.05
				//Debug.LogError("TODO : Physic Weight 적용 - x1.05");
				Editor.Controller.SetPhyVolWeight(1.05f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			if (apEditorUtil.ToggledButton(apStringFactory.I.Minus, false, isBtnAvailable, widthIncDecBtn, heightSetWeight))//"-"
			{
				//x0.95
				//Debug.LogError("TODO : Physic Weight 적용 - x0.95");
				//Editor.Controller.SetBoneWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식
				Editor.Controller.SetPhyVolWeight(0.95f, CALCULATE_MULTIPLY);//Multiply 방식

				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(8);

			int heightToolBtn = 25;
			int width2Btn = (width - 5) / 2;
			
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Blend), 1, Editor.GetUIWord(UIWORD.Blend), false, isBtnAvailable, width, heightToolBtn,
											apStringFactory.I.RiggingTooltip_Blend))//"The weights of vertices are blended" // 리깅 툴의 툴팁과 동일
			{
				//Blend
				Editor.Controller.SetPhyVolWeightBlend();
			}

			GUILayout.Space(5);

			
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightToolBtn));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Grow), 1, Editor.GetUIWord(UIWORD.Grow), false, isBtnAvailable, width2Btn, heightToolBtn,
											apStringFactory.I.RiggingTooltip_Grow))//"Select more of the surrounding vertices"
			{
				//Grow
				Editor.Controller.SelectVertexWeightGrowOrShrink(true);
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Rig_Shrink), 1, Editor.GetUIWord(UIWORD.Shrink), false, isBtnAvailable, width2Btn, heightToolBtn,
											apStringFactory.I.RiggingTooltip_Shrink))//"Reduce selected vertices"
			{
				//Shrink
				Editor.Controller.SelectVertexWeightGrowOrShrink(false);
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			//추가
			//Viscosity를 위한 그룹
			int viscosityGroupID = 0;
			bool isViscosityAvailable = false;
			if (isBtnAvailable && nSelectedVerts > 0)
			{
				for (int i = 0; i < selectedVerts.Count; i++)
				{
					viscosityGroupID |= selectedVerts[i]._modVertWeight._physicParam._viscosityGroupID;
				}
				isViscosityAvailable = true;
			}
			int iViscosityChanged = -1;
			bool isViscosityAdd = false;

			int heightVisTool = 20;
			int widthVisTool = ((width - 5) / 5) - 2;

			//5줄씩 총 10개 (0은 모두 0으로 만든다.)

			//"Viscosity Group ID"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.ViscosityGroupID));
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightVisTool));
			GUILayout.Space(5);


			if(_guiContent_PhysicsGroupID_None == null)		{ _guiContent_PhysicsGroupID_None = apGUIContentWrapper.Make(apStringFactory.I.X, false); }
			if(_guiContent_PhysicsGroupID_1 == null)		{ _guiContent_PhysicsGroupID_1 = apGUIContentWrapper.Make(apStringFactory.I.Num1, false); }
			if(_guiContent_PhysicsGroupID_2 == null)		{ _guiContent_PhysicsGroupID_2 = apGUIContentWrapper.Make(apStringFactory.I.Num2, false); }
			if(_guiContent_PhysicsGroupID_3 == null)		{ _guiContent_PhysicsGroupID_3 = apGUIContentWrapper.Make(apStringFactory.I.Num3, false); }
			if(_guiContent_PhysicsGroupID_4 == null)		{ _guiContent_PhysicsGroupID_4 = apGUIContentWrapper.Make(apStringFactory.I.Num4, false); }
			if(_guiContent_PhysicsGroupID_5 == null)		{ _guiContent_PhysicsGroupID_5 = apGUIContentWrapper.Make(apStringFactory.I.Num5, false); }
			if(_guiContent_PhysicsGroupID_6 == null)		{ _guiContent_PhysicsGroupID_6 = apGUIContentWrapper.Make(apStringFactory.I.Num6, false); }
			if(_guiContent_PhysicsGroupID_7 == null)		{ _guiContent_PhysicsGroupID_7 = apGUIContentWrapper.Make(apStringFactory.I.Num7, false); }
			if(_guiContent_PhysicsGroupID_8 == null)		{ _guiContent_PhysicsGroupID_8 = apGUIContentWrapper.Make(apStringFactory.I.Num8, false); }
			if(_guiContent_PhysicsGroupID_9 == null)		{ _guiContent_PhysicsGroupID_9 = apGUIContentWrapper.Make(apStringFactory.I.Num9, false); }
			
			apGUIContentWrapper curGUIContent = null;

			for (int i = 0; i < 10; i++)
			{
				if (i == 5)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(heightVisTool));
					GUILayout.Space(5);
				}


				//string label = "";
				int iResult = 0;
				
				//string 쓰지 말자
				switch (i)
				{
					case 0:	curGUIContent = _guiContent_PhysicsGroupID_None;	iResult = 0;	break;
					case 1:	curGUIContent = _guiContent_PhysicsGroupID_1;		iResult = 1;	break;
					case 2:	curGUIContent = _guiContent_PhysicsGroupID_2;		iResult = 2;	break;
					case 3:	curGUIContent = _guiContent_PhysicsGroupID_3;		iResult = 4;	break;
					case 4:	curGUIContent = _guiContent_PhysicsGroupID_4;		iResult = 8;	break;
					case 5:	curGUIContent = _guiContent_PhysicsGroupID_5;		iResult = 16;	break;
					case 6:	curGUIContent = _guiContent_PhysicsGroupID_6;		iResult = 32;	break;
					case 7:	curGUIContent = _guiContent_PhysicsGroupID_7;		iResult = 64;	break;
					case 8:	curGUIContent = _guiContent_PhysicsGroupID_8;		iResult = 128;	break;
					case 9:	curGUIContent = _guiContent_PhysicsGroupID_9;		iResult = 256;	break;
				}

				bool isSelected = (viscosityGroupID & iResult) != 0;
				if (apEditorUtil.ToggledButton_2Side(curGUIContent.Content.text, curGUIContent.Content.text, isSelected, isViscosityAvailable, widthVisTool, heightVisTool))
				{
					iViscosityChanged = iResult;
					isViscosityAdd = !isSelected;
				}
			}
			EditorGUILayout.EndHorizontal();

			if (iViscosityChanged > -1)
			{
				Editor.Controller.SetPhysicsViscostyGroupID(iViscosityChanged, isViscosityAdd);
			}



			

			//메시 설정
			apPhysicsMeshParam physicMeshParam = null;
			if (ModMesh_Main != null && ModMesh_Main.PhysicParam != null)
			{
				physicMeshParam = ModMesh_Main.PhysicParam;
			}

			//물리 프리셋을 출력할 수 있는가
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_PhysicsPreset_Valid, ModMesh_Main != null);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_PhysicsPreset_Invalid, ModMesh_Main == null);
			bool isDraw_PhysicsPreset = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_PhysicsPreset_Valid)
										|| Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Modifier_PhysicsPreset_Invalid);

			if (!isDraw_PhysicsPreset)
			{
				return;
			}

			//더미를 다시 정의하자
			//위에서는 추가/삭제의 MeshTransform
			//여기서부터는 ModMesh의 존재
			isDummyTransform = false;
			if(ModMesh_Main == null || physicMeshParam == null)
			{
				isDummyTransform = true;
			}

			if ((physicMeshParam == null && !isDummyTransform)
				|| (physicMeshParam != null && isDummyTransform))
			{
				//Mesh도 없고, Dummy도 없으면..
				//또는 Mesh가 있는데도 Dummy 판정이 났다면.. 
				return;
			}

			//여기서부턴 Dummy가 있으면 그 값을 이용한다.
			
			if (isDummyTransform && (_physicModifier_prevSelectedTransform == null || !_physicModifier_prevIsContained))
			{
				return;
			}



			//물리 재질에 대한 UI

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);


			int labelHeight = 30;

			apPhysicsPresetUnit presetUnit = null;
			if (!isDummyTransform)
			{
				if (physicMeshParam._presetID >= 0)
				{
					presetUnit = Editor.PhysicsPreset.GetPresetUnit(physicMeshParam._presetID);
					if (presetUnit == null)
					{
						physicMeshParam._presetID = -1;
					}
				}
			}
			
			if (_guiContent_Modifier_PhysicsSetting_NameIcon == null)		{ _guiContent_Modifier_PhysicsSetting_NameIcon = new apGUIContentWrapper(); }
			if (_guiContent_Modifier_PhysicsSetting_Basic == null)			{ _guiContent_Modifier_PhysicsSetting_Basic = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.BasicSetting), Editor.ImageSet.Get(apImageSet.PRESET.Physic_BasicSetting)); }
			if (_guiContent_Modifier_PhysicsSetting_Stretchiness == null)	{ _guiContent_Modifier_PhysicsSetting_Stretchiness = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.Stretchiness), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Stretch)); }
			if (_guiContent_Modifier_PhysicsSetting_Inertia == null)		{ _guiContent_Modifier_PhysicsSetting_Inertia = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.Inertia), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Inertia)); }
			if (_guiContent_Modifier_PhysicsSetting_Restoring == null)		{ _guiContent_Modifier_PhysicsSetting_Restoring = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.Restoring), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Recover)); }
			if (_guiContent_Modifier_PhysicsSetting_Viscosity == null)		{ _guiContent_Modifier_PhysicsSetting_Viscosity = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.Viscosity), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Viscosity)); }
			if (_guiContent_Modifier_PhysicsSetting_Gravity == null)		{ _guiContent_Modifier_PhysicsSetting_Gravity = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.Gravity), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Gravity)); }
			if (_guiContent_Modifier_PhysicsSetting_Wind == null)			{ _guiContent_Modifier_PhysicsSetting_Wind = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.Wind), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Wind)); }


			if (presetUnit != null)
			{
				bool isPropertySame = presetUnit.IsSameProperties(physicMeshParam);
				if (isPropertySame)
				{
					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(0.4f, 1.0f, 1.1f, 1.0f);
				}

				_guiContent_Modifier_PhysicsSetting_NameIcon.SetText(2, presetUnit._name);
				_guiContent_Modifier_PhysicsSetting_NameIcon.SetImage(Editor.ImageSet.Get(apEditorUtil.GetPhysicsPresetIconType(presetUnit._icon)));
				GUILayout.Box(_guiContent_Modifier_PhysicsSetting_NameIcon.Content, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

				GUI.backgroundColor = prevColor;
			}
			else
			{
				//"Physical Material"
				GUILayout.Box(Editor.GetUIWord(UIWORD.PhysicalMaterial), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
			}

			GUILayout.Space(5);
			//Preset
			//값이 바뀌었으면 Dirty
			
			//"  Basic Setting"
			EditorGUILayout.LabelField(_guiContent_Modifier_PhysicsSetting_Basic.Content, apGUILOFactory.I.Height(labelHeight));
			
			float nextMass = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.Mass), (!isDummyTransform) ? physicMeshParam._mass : 0.0f);//"Mass"
			float nextDamping = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.Damping), (!isDummyTransform) ? physicMeshParam._damping : 0.0f);//"Damping"
			float nextAirDrag = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.AirDrag), (!isDummyTransform) ? physicMeshParam._airDrag : 0.0f);//"Air Drag"
			bool nextIsRestrictMoveRange = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.SetMoveRange), (!isDummyTransform) ? physicMeshParam._isRestrictMoveRange : false);//"Set Move Range"
			float nextMoveRange = (!isDummyTransform) ? physicMeshParam._moveRange : 0.0f;
			if (nextIsRestrictMoveRange)
			{
				nextMoveRange = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.MoveRange), (!isDummyTransform) ? physicMeshParam._moveRange : 0.0f);//"Move Range"
			}
			else
			{
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.MoveRangeUnlimited));//"Move Range : Unlimited"
			}

			GUILayout.Space(5);

			int valueWidth = 74;//캬... 꼼꼼하다
			int labelWidth = width - (valueWidth + 2 + 5);
			int leftMargin = 3;
			int topMargin = 10;

			//이전
			//EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Stretchiness), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Stretch)), GUILayout.Height(labelHeight));//"  Stretchiness"

			//변경
			EditorGUILayout.LabelField(_guiContent_Modifier_PhysicsSetting_Stretchiness.Content, apGUILOFactory.I.Height(labelHeight));//"  Stretchiness"

			
			float nextStretchK = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.K_Value), (!isDummyTransform) ? physicMeshParam._stretchK : 0.0f);//"K-Value"
			bool nextIsRestrictStretchRange = EditorGUILayout.Toggle(Editor.GetUIWord(UIWORD.SetStretchRange), (!isDummyTransform) ? physicMeshParam._isRestrictStretchRange : false);//"Set Stretch Range"
			float nextStretchRange_Max = (!isDummyTransform) ? physicMeshParam._stretchRangeRatio_Max : 0.0f;
			if (nextIsRestrictStretchRange)
			{
				//"Lengthen Ratio"
				nextStretchRange_Max = EditorGUILayout.DelayedFloatField(Editor.GetUIWord(UIWORD.LengthenRatio), (!isDummyTransform) ? physicMeshParam._stretchRangeRatio_Max : 0.0f);
			}
			else
			{
				//"Lengthen Ratio : Unlimited"
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.LengthenRatioUnlimited));
			}
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(labelHeight));
			GUILayout.Space(leftMargin);
			
			//"  Inertia"
			//이전
			//EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Inertia), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Inertia)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			//변경
			EditorGUILayout.LabelField(_guiContent_Modifier_PhysicsSetting_Inertia.Content, apGUILOFactory.I.Width(labelWidth), apGUILOFactory.I.Height(labelHeight));

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(valueWidth), apGUILOFactory.I.Height(labelHeight));//>>2
			GUILayout.Space(topMargin);
			float nextInertiaK = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._inertiaK : 0.0f, apGUILOFactory.I.Width(valueWidth));
			EditorGUILayout.EndVertical();//<<2
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(labelHeight));
			GUILayout.Space(leftMargin);
			
			//"  Restoring"
			//이전
			//EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Restoring), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Recover)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			
			//변경
			EditorGUILayout.LabelField(_guiContent_Modifier_PhysicsSetting_Restoring.Content, apGUILOFactory.I.Width(labelWidth), apGUILOFactory.I.Height(labelHeight));

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(valueWidth), apGUILOFactory.I.Height(labelHeight));//>>2
			GUILayout.Space(topMargin);
			float nextRestoring = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._restoring : 0.0f, apGUILOFactory.I.Width(valueWidth));
			EditorGUILayout.EndVertical();//<<2
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);


			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(labelHeight));
			GUILayout.Space(leftMargin);
			//"  Viscosity"
			//이전
			//EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Viscosity), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Viscosity)), GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
			
			//변경
			EditorGUILayout.LabelField(_guiContent_Modifier_PhysicsSetting_Viscosity.Content, apGUILOFactory.I.Width(labelWidth), apGUILOFactory.I.Height(labelHeight));

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(valueWidth), apGUILOFactory.I.Height(labelHeight));//>>2
			GUILayout.Space(topMargin);
			float nextViscosity = EditorGUILayout.DelayedFloatField((!isDummyTransform) ? physicMeshParam._viscosity : 0.0f, apGUILOFactory.I.Width(valueWidth));
			EditorGUILayout.EndVertical();//<<2
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			

			//값이 바뀌었으면 적용
			if (!isDummyTransform)
			{
				if (nextMass != physicMeshParam._mass
					|| nextDamping != physicMeshParam._damping
					|| nextAirDrag != physicMeshParam._airDrag
					|| nextMoveRange != physicMeshParam._moveRange
					|| nextStretchK != physicMeshParam._stretchK
					//|| nextStretchRange_Min != physicMeshParam._stretchRangeRatio_Min
					|| nextStretchRange_Max != physicMeshParam._stretchRangeRatio_Max
					|| nextInertiaK != physicMeshParam._inertiaK
					|| nextRestoring != physicMeshParam._restoring
					|| nextViscosity != physicMeshParam._viscosity
					|| nextIsRestrictStretchRange != physicMeshParam._isRestrictStretchRange
					|| nextIsRestrictMoveRange != physicMeshParam._isRestrictMoveRange)
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
														Editor, 
														Modifier, 
														//ModMesh_Main, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					physicMeshParam._mass = nextMass;
					physicMeshParam._damping = nextDamping;
					physicMeshParam._airDrag = nextAirDrag;
					physicMeshParam._moveRange = nextMoveRange;
					physicMeshParam._stretchK = nextStretchK;

					//physicMeshParam._stretchRangeRatio_Min = Mathf.Clamp01(nextStretchRange_Min);
					physicMeshParam._stretchRangeRatio_Max = nextStretchRange_Max;
					if (physicMeshParam._stretchRangeRatio_Max < 0.0f)
					{
						physicMeshParam._stretchRangeRatio_Max = 0.0f;
					}

					physicMeshParam._isRestrictStretchRange = nextIsRestrictStretchRange;
					physicMeshParam._isRestrictMoveRange = nextIsRestrictMoveRange;


					physicMeshParam._inertiaK = nextInertiaK;
					physicMeshParam._restoring = nextRestoring;
					physicMeshParam._viscosity = nextViscosity;

					apEditorUtil.ReleaseGUIFocus();
				}
			}



			


			//GUILayout.Space(5);

			//이전
			//EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Gravity), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Gravity)), GUILayout.Height(labelHeight));//"  Gravity"

			//변경
			EditorGUILayout.LabelField(_guiContent_Modifier_PhysicsSetting_Gravity.Content, apGUILOFactory.I.Height(labelHeight));//"  Gravity"

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.InputType));//"Input Type"
			apPhysicsMeshParam.ExternalParamType nextGravityParam = (apPhysicsMeshParam.ExternalParamType)EditorGUILayout.EnumPopup((!isDummyTransform) ? physicMeshParam._gravityParamType : apPhysicsMeshParam.ExternalParamType.Constant);

			Vector2 nextGravityConstValue = (!isDummyTransform) ? physicMeshParam._gravityConstValue : Vector2.zero;

			apPhysicsMeshParam.ExternalParamType curGravityParam = (physicMeshParam != null) ? physicMeshParam._gravityParamType : apPhysicsMeshParam.ExternalParamType.Constant;

			if (curGravityParam == apPhysicsMeshParam.ExternalParamType.Constant)
			{
				nextGravityConstValue = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._gravityConstValue : Vector2.zero, width - 4);
			}
			else
			{
				//?
				//TODO : GravityControlParam 링크할 것
				apControlParam controlParam = physicMeshParam._gravityControlParam;
				if (controlParam == null && physicMeshParam._gravityControlParamID > 0)
				{
					physicMeshParam._gravityControlParam = Editor._portrait._controller.FindParam(physicMeshParam._gravityControlParamID);
					controlParam = physicMeshParam._gravityControlParam;
					if (controlParam == null)
					{
						physicMeshParam._gravityControlParamID = -1;
					}
				}

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
				GUILayout.Space(5);
				if (controlParam != null)
				{
					if(_strWrapper_64 == null)
					{
						_strWrapper_64 = new apStringWrapper(64);
					}

					_strWrapper_64.Clear();
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
					_strWrapper_64.Append(controlParam._keyName, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

					GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
					//GUILayout.Box("[" + controlParam._keyName + "]", apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 34), apGUILOFactory.I.Height(25));
					GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 34), apGUILOFactory.I.Height(25));

					GUI.backgroundColor = prevColor;
				}
				else
				{
					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					GUILayout.Box(Editor.GetUIWord(UIWORD.NoControlParam), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 34), apGUILOFactory.I.Height(25));//"No ControlParam

					GUI.backgroundColor = prevColor;
				}

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Set), apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(25)))//"Set"
				{
					//Control Param을 선택하는 Dialog를 호출하자
					_loadKey_SelectControlParamToPhyGravity = apDialog_SelectControlParam.ShowDialog(Editor, apDialog_SelectControlParam.PARAM_TYPE.Vector2, OnSelectControlParamToPhysicGravity, null);
				}
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(5);

			//이전
			//EditorGUILayout.LabelField(new GUIContent("  " + Editor.GetUIWord(UIWORD.Wind), Editor.ImageSet.Get(apImageSet.PRESET.Physic_Wind)), GUILayout.Height(labelHeight));//"  Wind"
			
			//변경
			EditorGUILayout.LabelField(_guiContent_Modifier_PhysicsSetting_Wind.Content, apGUILOFactory.I.Height(labelHeight));//"  Wind"

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.InputType));//"Input Type"
			apPhysicsMeshParam.ExternalParamType nextWindParamType = (apPhysicsMeshParam.ExternalParamType)EditorGUILayout.EnumPopup((!isDummyTransform) ? physicMeshParam._windParamType : apPhysicsMeshParam.ExternalParamType.Constant);

			Vector2 nextWindConstValue = (!isDummyTransform) ? physicMeshParam._windConstValue : Vector2.zero;
			Vector2 nextWindRandomRange = (!isDummyTransform) ? physicMeshParam._windRandomRange : Vector2.zero;

			apPhysicsMeshParam.ExternalParamType curWindParamType = (physicMeshParam != null) ? physicMeshParam._windParamType : apPhysicsMeshParam.ExternalParamType.Constant;

			if (curWindParamType == apPhysicsMeshParam.ExternalParamType.Constant)
			{
				nextWindConstValue = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._windConstValue : Vector2.zero, width - 4);
			}
			else
			{
				//?
				//TODO : GravityControlParam 링크할 것
				apControlParam controlParam = physicMeshParam._windControlParam;
				if (controlParam == null && physicMeshParam._windControlParamID > 0)
				{
					physicMeshParam._windControlParam = Editor._portrait._controller.FindParam(physicMeshParam._windControlParamID);
					controlParam = physicMeshParam._windControlParam;
					if (controlParam == null)
					{
						physicMeshParam._windControlParamID = -1;
					}
				}

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
				GUILayout.Space(5);
				if (controlParam != null)
				{
					if(_strWrapper_64 == null)
					{
						_strWrapper_64 = new apStringWrapper(64);
					}

					_strWrapper_64.Clear();
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
					_strWrapper_64.Append(controlParam._keyName, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);


					GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
					//GUILayout.Box("[" + controlParam._keyName + "]", apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width - 34), GUILayout.Height(25));
					GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 34), apGUILOFactory.I.Height(25));

					GUI.backgroundColor = prevColor;
				}
				else
				{
					GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
					GUILayout.Box(Editor.GetUIWord(UIWORD.NoControlParam), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width - 34), apGUILOFactory.I.Height(25));//"No ControlParam"

					GUI.backgroundColor = prevColor;
				}

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.Set), apGUILOFactory.I.Width(30), apGUILOFactory.I.Height(25)))//"Set"
				{
					//Control Param을 선택하는 Dialog를 호출하자
					_loadKey_SelectControlParamToPhyWind = apDialog_SelectControlParam.ShowDialog(Editor, apDialog_SelectControlParam.PARAM_TYPE.Vector2, OnSelectControlParamToPhysicWind, null);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.WindRandomRangeSize));//"Wind Random Range Size"
			nextWindRandomRange = apEditorUtil.DelayedVector2Field((!isDummyTransform) ? physicMeshParam._windRandomRange : Vector2.zero, width - 4);

			GUILayout.Space(10);
			

			//Preset 창을 열자
			//" Physics Presets"
			if (apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Physic_Palette), 
													1, Editor.GetUIWord(UIWORD.PhysicsPresets), 
													Editor.GetUIWord(UIWORD.PhysicsPresets), 
													false, physicMeshParam != null, width, 32))
			{
				_loadKey_SelectPhysicsParam = apDialog_PhysicsPreset.ShowDialog(Editor, ModMesh_Main, OnSelectPhysicsPreset);
			}


			if (!isDummyTransform)
			{
				if (nextGravityParam != physicMeshParam._gravityParamType
					|| nextGravityConstValue.x != physicMeshParam._gravityConstValue.x
					|| nextGravityConstValue.y != physicMeshParam._gravityConstValue.y
					|| nextWindParamType != physicMeshParam._windParamType
					|| nextWindConstValue.x != physicMeshParam._windConstValue.x
					|| nextWindConstValue.y != physicMeshParam._windConstValue.y
					|| nextWindRandomRange.x != physicMeshParam._windRandomRange.x
					|| nextWindRandomRange.y != physicMeshParam._windRandomRange.y
					)
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_SettingChanged, 
														Editor, 
														Modifier, 
														//ModMesh_Main, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					physicMeshParam._gravityParamType = nextGravityParam;
					physicMeshParam._gravityConstValue = nextGravityConstValue;
					physicMeshParam._windParamType = nextWindParamType;
					physicMeshParam._windConstValue = nextWindConstValue;
					physicMeshParam._windRandomRange = nextWindRandomRange;
					apEditorUtil.ReleaseGUIFocus();
				}
			}
		}







		// 애니메이션의 Right 2 UI


		
		// Animation Right 2 GUI
		//------------------------------------------------------------------------------------
		/// <summary>
		/// 애니메이션 편집시의 Right 2 UI 그리기
		/// </summary>
		private void DrawEditor_Right2_Animation(int width, int height)
		{
			// 상단부는 AnimClip의 정보를 출력하며,
			// 하단부는 선택된 Timeline의 정보를 출력한다.

			// AnimClip 정보 출력 부분

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_AnimClip, (AnimClip != null));//"AnimationRight2GUI_AnimClip"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline, (AnimTimeline != null));//"AnimationRight2GUI_Timeline"

			if (AnimClip == null)
			{
				return;
			}

			if (!Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_AnimClip))//"AnimationRight2GUI_AnimClip"
			{
				//아직 출력하면 안된다.
				return;
			}

			apAnimClip animClip = AnimClip;

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(50));
			GUILayout.Space(10);

			//이전
			//EditorGUILayout.LabelField(
			//	new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation)),
			//	GUILayout.Width(50), GUILayout.Height(50));

			//변경
			if(_guiContent_Right_MeshGroup_AnimIcon == null)
			{
				_guiContent_Right_MeshGroup_AnimIcon = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation));
			}
			EditorGUILayout.LabelField(_guiContent_Right_MeshGroup_AnimIcon.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(50));

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(animClip._name, apGUILOFactory.I.Width(width - (50 + 10)));


			if(_strWrapper_64 == null)
			{
				_strWrapper_64 = new apStringWrapper(64);
			}

			if (animClip._targetMeshGroup != null)
			{
				_strWrapper_64.Clear();
				_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Target), false);
				_strWrapper_64.Append(apStringFactory.I.Colon_Space, false);
				_strWrapper_64.Append(animClip._targetMeshGroup._name, true);

				//"Target : " + animClip._targetMeshGroup._name
				//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Target) + " : " + animClip._targetMeshGroup._name, apGUILOFactory.I.Width(width - (50 + 10)));
				EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(width - (50 + 10)));
			}
			else
			{
				_strWrapper_64.Clear();
				_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Target), false);
				_strWrapper_64.Append(apStringFactory.I.Colon_Space, false);
				_strWrapper_64.Append(Editor.GetUIWord(UIWORD.NoMeshGroup), true);

				//EditorGUILayout.LabelField(string.Format("{0} : {1}", Editor.GetUIWord(UIWORD.Target), Editor.GetUIWord(UIWORD.NoMeshGroup)), GUILayout.Width(width - (50 + 10)));
				EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(width - (50 + 10)));
			}


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);


			//애니메이션 기본 정보

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.AnimationSettings), apGUILOFactory.I.Width(width));//"Animation Settings"
			GUILayout.Space(2);
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.StartFrame), apGUILOFactory.I.Width(110));//"Start Frame"
			int nextStartFrame = EditorGUILayout.DelayedIntField(animClip.StartFrame, apGUILOFactory.I.Width(width - (110 + 5)));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.EndFrame), apGUILOFactory.I.Width(110));//"End Frame"
			int nextEndFrame = EditorGUILayout.DelayedIntField(animClip.EndFrame, apGUILOFactory.I.Width(width - (110 + 5)));
			EditorGUILayout.EndHorizontal();

			bool isNextLoop = animClip.IsLoop;
			//" Loop On", " Loop Off"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Loop),
													1, Editor.GetUIWord(UIWORD.LoopOn),
													Editor.GetUIWord(UIWORD.LoopOff),
													animClip.IsLoop, true, width, 24))
			{
				isNextLoop = !animClip.IsLoop;
				//값 적용은 아래에서
			}

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
			EditorGUILayout.LabelField(apStringFactory.I.FPS, apGUILOFactory.I.Width(110));//<<이건 고정//"FPS"
			
			int nextFPS = EditorGUILayout.DelayedIntField(animClip.FPS, apGUILOFactory.I.Width(width - (110 + 5)));
			//int nextFPS = EditorGUILayout.IntSlider("FPS", animClip._FPS, 1, 240, GUILayout.Width(width));
			
			bool isBasicSettingChanged = EditorGUI.EndChangeCheck();

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			//추가 : 애니메이션 이벤트
			int nAnimEvents = 0;
			if (animClip._animEvents != null)
			{
				nAnimEvents = animClip._animEvents.Count;
			}

			_strWrapper_64.Clear();
			_strWrapper_64.AppendSpace(1, false);
			_strWrapper_64.Append(Editor.GetUIWord(UIWORD.AnimationEvents), false);
			_strWrapper_64.AppendSpace(1, false);
			_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
			_strWrapper_64.Append(nAnimEvents, false);
			_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

			// 애니메이션 이벤트
			//변경 21.3.7 : MeshGroup이 설정되지 않았다면 이 버튼은 누를 수 없다.
			if(apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.AnimEvent_MainIcon),
													_strWrapper_64.ToString(),
													_strWrapper_64.ToString(),
													false, animClip._targetMeshGroup != null, width, 22))
			{
				if(animClip._targetMeshGroup != null)
				{
					apDialog_AnimationEvents.ShowDialog(Editor, Editor._portrait, animClip);
				}
				
			}

			// [ Export / Import ]
			//변경 21.3.7 : MeshGroup이 설정되지 않았다면 이 버튼을 누를 수 없다.
			if (apEditorUtil.ToggledButton_2Side(	Editor.ImageSet.Get(apImageSet.PRESET.Anim_Save),
													Editor.GetUIWord(UIWORD.ExportImport),
													Editor.GetUIWord(UIWORD.ExportImport), 
													false, animClip._targetMeshGroup != null, width, 22))
			{
				//AnimClip을 Export/Import 하자
				if (animClip._targetMeshGroup != null)
				{
					//v1.4.2 : FFD가 켜져있다면, Import 전에 물어봐야 한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if(isExecutable)
					{
						_loadKey_ImportAnimClipRetarget = apDialog_RetargetPose.ShowDialog(Editor, _animClip._targetMeshGroup, _animClip, OnImportAnimClipRetarget);
					}
					
				}
			}


			
			GUILayout.Space(5);

			// [ Duplicate ] 변경 21.3.14 : Duplicate 버튼이 이쪽으로 왔다.
			if (GUILayout.Button(Editor.GetUIWord(UIWORD.Duplicate), apGUILOFactory.I.Width(width)))//"Duplicate"
			{
				//v1.4.2 : FFD가 켜져있다면, 복사 전에 물어봐야 한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					Editor.Controller.DuplicateAnimClip(_animClip);
					Editor.RefreshControllerAndHierarchy(true);
				}
				
			}

			
			if ((isBasicSettingChanged && (	nextStartFrame != animClip.StartFrame
											|| nextEndFrame != animClip.EndFrame
											|| nextFPS != animClip.FPS))
				|| isNextLoop != animClip.IsLoop)
			{
				//바뀌었다면 타임라인 GUI를 세팅할 필요가 있을 수 있다.
				//Debug.Log("Anim Setting Changed");

				//v1.4.2 : 여기서는 FFD가 켜진 경우 취소 없이 물어보기만 가능하다 UI 문제
				if(Editor.Gizmos.IsFFDMode)
				{
					Editor.Gizmos.CheckAdaptOrRevertFFD_WithoutCancel();
				}

				//Undo에 저장하자
				if (animClip._targetMeshGroup != null)
				{
					apEditorUtil.SetRecord_PortraitMeshGroup(apUndoGroupData.ACTION.Anim_SettingChanged,
																Editor,
																Editor._portrait,
																animClip._targetMeshGroup,
																//animClip,
																false,
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_SettingChanged,
														Editor,
														Editor._portrait,
														//animClip,
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
				}

				apEditorUtil.SetEditorDirty();

				//Start Frame과 Next Frame의 값이 뒤집혀져있는지 확인
				if (nextStartFrame > nextEndFrame)
				{
					int tmp = nextStartFrame;
					nextStartFrame = nextEndFrame;
					nextEndFrame = tmp;
				}

				animClip.SetOption_StartFrame(nextStartFrame);
				animClip.SetOption_EndFrame(nextEndFrame);
				animClip.SetOption_FPS(nextFPS);
				animClip.SetOption_IsLoop(isNextLoop);

				//추가 20.4.14 : 애니메이션의 길이나 루프 설정이 바뀌면, 전체적으로 리프레시를 하자
				animClip.RefreshTimelines(null, null);
				

				apEditorUtil.ReleaseGUIFocus();
			}



			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);


			// Timeline 정보 출력 부분

			if (AnimTimeline == null)
			{
				return;
			}

			if (!Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline))//"AnimationRight2GUI_Timeline"
			{
				//아직 출력하면 안된다.
				return;
			}

			apAnimTimeline curTimeline = AnimTimeline;
			apAnimTimelineLayer curTimelineLayer_Main = AnimTimelineLayer_Main;
			int nTimelineLayers = NumAnimTimelineLayers;

			
			
			//Timeline 정보 출력
			

			if(_guiContent_Right_Animation_TimelineIcon_AnimWithMod == null)
			{
				_guiContent_Right_Animation_TimelineIcon_AnimWithMod = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithMod));
			}
			if(_guiContent_Right_Animation_TimelineIcon_AnimWithControlParam == null)
			{
				_guiContent_Right_Animation_TimelineIcon_AnimWithControlParam = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_WithControlParam));
			}
			
			apGUIContentWrapper curIconGUIContent = (curTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier) ? _guiContent_Right_Animation_TimelineIcon_AnimWithMod : _guiContent_Right_Animation_TimelineIcon_AnimWithControlParam;
			
			//1. 아이콘 / 타입
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
			GUILayout.Space(10);
			
			EditorGUILayout.LabelField(curIconGUIContent.Content, apGUILOFactory.I.Width(50), apGUILOFactory.I.Height(30));

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(width - (50 + 10)));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(curTimeline.DisplayName, apGUILOFactory.I.Width(width - (50 + 10)));


			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			//GUILayout.Space(10);


			//현재 선택한 객체를 레이어로 만들 수 있다.
			//상태 : 선택한게 없다. / 선택은 했으나 레이어에 등록이 안되었다. (등록할 수 있다) / 선택한게 이미 등록한 객체다. (
			//bool isAnyTargetObjectSelected = false;
			bool isAddableType = false;
			bool isAddable = false;

			//추가 20.6.5 : 다중 선택
			int nMeshTF = 0;
			int nMeshGroupTF = 0;
			int nBones = 0;

			bool isMultiSelected = false;


			//추가 20.9.8 : 등록 불가 객체의 불가 사유를 구분하자
			//리깅된 자식 메시는 TF 타임라인에 추가할 수 없는데, 이 이유를 표시하는게 필요 
			MOD_ADD_FAIL_REASON modAddFaileReason = MOD_ADD_FAIL_REASON.NotSupportedType;//기본값

			

			//string targetObjectName = "";
			if(_guiContent_Right2_Animation_TargetObjectName == null)
			{
				_guiContent_Right2_Animation_TargetObjectName = new apGUIContentWrapper();
			}

			object targetObject = null;
			bool isAddingLayerOnce_MeshOrBone = false;
			bool isAddingLayerOnce_ControlParams = false;//추가 21.3.10

			bool isAddChildTransformAddable = false;
			bool isAnySelected = false;
			bool isRegistered = false;//추가. 선택한 객체가 등록되었다. 단일 객체가 등록된 경우에만 True이고, 여러개인 경우엔 하나만 등록되어도 True

			//v1.4.2 : 이미 선택된 레이어를 이용하면 "선택된 객체가 선택되었는지" 더 빠르게 확인할 수 있다.
			apAnimTimelineLayer selectedLayer_Main = _subObjects.TimelineLayer;
			List<apAnimTimelineLayer> selectedLayers_All = _subObjects.AllTimelineLayers;
			int nSelectedLayers = selectedLayers_All != null ? selectedLayers_All.Count : 0;

			switch (curTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						//추가 20.6.5 : 다중 선택도 체크하자
						nMeshTF = curTimeline._linkedModifier.IsTarget_MeshTransform ? _subObjects.NumMeshTF : 0;
						nMeshGroupTF = curTimeline._linkedModifier.IsTarget_MeshGroupTransform ? _subObjects.NumMeshGroupTF : 0;
						nBones = curTimeline._linkedModifier.IsTarget_Bone ? _subObjects.NumBone : 0;

						isRegistered = false;

						if (nMeshTF + nMeshGroupTF + nBones > 1)//[다중 선택]
						{
							//여러개가 선택된 상태다.
							isMultiSelected = true;

							//추가 여부는 하나라도 추가할 수 있으면 true
							isAddableType = false;
							isAddable = false;
							

							int nValidObjects = 0;//선택된 유효한 객체 (등록 여부 상관 없음)

							if (nMeshTF > 0)
							{
								List<apTransform_Mesh> meshTFs = _subObjects.AllMeshTFs;
								apTransform_Mesh curMeshTF = null;

								for (int i = 0; i < nMeshTF; i++)
								{
									curMeshTF = meshTFs[i];
									if (curMeshTF == null) { continue; }

									if (curTimeline.IsLayerAddableType(curMeshTF))
									{
										//기존 : 그냥 타입만 맞으면 된다.
										//isAddableType = true;//등록 가능한 타입이다.
										//nValidObjects++;

										//변경 20.9.8 : 조건을 조금 더 체크해야한다.
										isAddableType = true;//등록 가능한 타입이다.

										if (!curTimeline.IsObjectAddedInLayers(curMeshTF))//변경 20.9.8 : 코드가 안쪽으로 들어옴.
										{
											//등록이 안된 상태일때

											//추가 가능한지 체크
											//변경 20.9.8 : TF 타임라인에서는 메시의 리깅 여부도 확인해야한다.
											if (curTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedTF
												&& curMeshTF.IsRiggedChildMeshTF(animClip._targetMeshGroup))
											{
												//이 조건에서는 추가할 수 없다.
												modAddFaileReason = MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod;
											}
											else
											{
												//추가할 수 있다.
												isAddable = true;
												nValidObjects++;
											}
										}
										else
										{
											//이미 등록했다면 카운트만 올린다.
											isRegistered = true;//<<등록됨
											nValidObjects++;
										}
									}
									else
									{
										//v1.4.2 : 에러상황 : 대상 타입은 아닌데 등록된 경우가 있다.
										//현재 선택된 레이어가 있다면 체크한다.
										if(nSelectedLayers > 0)
										{
											apAnimTimelineLayer curSelectedLayer = null;
											for (int iSL = 0; iSL < nSelectedLayers; iSL++)
											{
												curSelectedLayer = selectedLayers_All[iSL];
												if(curSelectedLayer._linkedMeshTransform == curMeshTF
													&& curSelectedLayer._parentTimeline == curTimeline)
												{
													//에러 상황 : 대상이 아닌데 등록이 되어있다. 최소한 삭제를 하도록 만들자
													isRegistered = true;
													nValidObjects++;
													break;
												}
											}
										}
									}
									
								}
							}
							if (nMeshGroupTF > 0)
							{
								List<apTransform_MeshGroup> meshGroupTFs = _subObjects.AllMeshGroupTFs;
								apTransform_MeshGroup curMeshGroupTF = null;

								for (int i = 0; i < nMeshGroupTF; i++)
								{
									curMeshGroupTF = meshGroupTFs[i];
									if (curMeshGroupTF == null) { continue; }

									if (curTimeline.IsLayerAddableType(curMeshGroupTF))
									{
										isAddableType = true;//등록 가능한 타입이다.
										nValidObjects++;

										if (!curTimeline.IsObjectAddedInLayers(curMeshGroupTF))
										{
											isAddable = true;//하나라도 등록이 안되었다.
										}
										else
										{
											isRegistered = true;//<<등록됨
										}
									}
									else
									{
										//v1.4.2 : 에러상황 : 대상 타입은 아닌데 등록된 경우가 있다.
										//현재 선택된 레이어가 있다면 체크한다.
										if(nSelectedLayers > 0)
										{
											apAnimTimelineLayer curSelectedLayer = null;
											for (int iSL = 0; iSL < nSelectedLayers; iSL++)
											{
												curSelectedLayer = selectedLayers_All[iSL];
												if(curSelectedLayer._linkedMeshGroupTransform == curMeshGroupTF
													&& curSelectedLayer._parentTimeline == curTimeline)
												{
													//에러 상황 : 대상이 아닌데 등록이 되어있다. 최소한 삭제를 하도록 만들자
													isRegistered = true;
													nValidObjects++;
													break;
												}
											}
										}
									}
									
								}
							}
							if (nBones > 0)
							{
								List<apBone> bones = _subObjects.AllBones;
								apBone curBone = null;
								for (int i = 0; i < nBones; i++)
								{
									curBone = bones[i];
									if (curBone == null) { continue; }

									if (curTimeline.IsLayerAddableType(curBone))
									{
										isAddableType = true;//등록 가능한 타입이다.
										nValidObjects++;

										if (!curTimeline.IsObjectAddedInLayers(curBone))
										{
											isAddable = true;//하나라도 등록이 안되었다.
										}
										else
										{
											isRegistered = true;//<<등록됨
										}
									}
									else
									{
										//v1.4.2 : 에러상황 : 대상 타입은 아닌데 등록된 경우가 있다.
										//현재 선택된 레이어가 있다면 체크한다.
										if(nSelectedLayers > 0)
										{
											apAnimTimelineLayer curSelectedLayer = null;
											for (int iSL = 0; iSL < nSelectedLayers; iSL++)
											{
												curSelectedLayer = selectedLayers_All[iSL];
												if(curSelectedLayer._linkedBone == curBone
													&& curSelectedLayer._parentTimeline == curTimeline)
												{
													//에러 상황 : 대상이 아닌데 등록이 되어있다. 최소한 삭제를 하도록 만들자
													isRegistered = true;
													nValidObjects++;
													break;
												}
											}
										}
									}
								}
							}

							if (nValidObjects > 0)
							{
								//선택된 개수를 이름으로 설정
								_guiContent_Right2_Animation_TargetObjectName.ClearText(false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_L, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(nValidObjects, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendSpaceText(1, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(Editor.GetUIWord(UIWORD.Objects), false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_R, true);

								isAnySelected = true;
							}
							else
							{
								//선택된게 정말 없다.
								_guiContent_Right2_Animation_TargetObjectName.ClearText(true);
							}
						}
						else//[단일 선택]
						{
							//한개가 선택되었거나 선택되지 않은 상태다.
							isMultiSelected = false;


							//Transform이 속해있는지 확인하자
							if (MeshTF_Main != null)
							{
								apTransform_Mesh curSelectedMeshTF = MeshTF_Main;

								_guiContent_Right2_Animation_TargetObjectName.ClearText(false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_L, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(MeshTF_Main._nickName, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_R, true);//변경

								targetObject = curSelectedMeshTF;

								//v1.4.2
								//이미 선택되었는지 여부를 먼저 확인할 수 있다.
								//최적화 및 에러로 인하여 등록시 레이어 삭제가 가능하게 만듬
								if(selectedLayer_Main != null)
								{
									if(selectedLayer_Main._linkedMeshTransform == curSelectedMeshTF)
									{
										//여기서 미리 빠르게 체크하자
										isRegistered = true;
									}
								}


								//레이어로 등록가능한가
								isAddableType = curTimeline.IsLayerAddableType(curSelectedMeshTF);

								if (isAddableType)
								{
									#region [미사용 코드]
									//if (!curTimeline.IsObjectAddedInLayers(curSelectedMeshTF))
									//{
									//	//추가되지 않은 상태

									//	//추가 20.9.8 : 리깅된 자식 메시 여부에 따라서 추가 가능 여부가 결정될 수 있다.
									//	if (curTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedTF
									//			&& curSelectedMeshTF.IsRiggedChildMeshTF(animClip._targetMeshGroup))
									//	{
									//		//이 조건에서는 추가할 수 없다.
									//		modAddFaileReason = MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod;
									//		isAddable = false;
									//	}
									//	else
									//	{
									//		//추가할 수 있다.
									//		isAddable = true;
									//	}
									//}
									//else
									//{
									//	//추가된 상태
									//	isAddable = false;
									//	isRegistered = true;//<<등록됨
									//} 
									#endregion

									//추가되지 않았다면 타임라인 기준으로 한번 더 확인하자
									if(!isRegistered)
									{
										if (curTimeline.IsObjectAddedInLayers(curSelectedMeshTF))
										{
											//타임라인에서 확인해보니 이미 레이어로 등록된게 맞다.
											isRegistered = true;
										}
									}
									
									if(isRegistered)
									{
										//이미 추가된 상태라면 레이어로 등록할 수 없다.
										isAddable = false;
									}
									else
									{
										//추가되지 않은 상태라면 레이어로 등록 가능한지 체크하자.
										
										//20.9.8 : 리깅된 자식 메시 여부에 따라서 추가 가능 여부가 결정될 수 있다.
										if (curTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedTF
												&& curSelectedMeshTF.IsRiggedChildMeshTF(animClip._targetMeshGroup))
										{
											//이 조건에서는 추가할 수 없다.
											modAddFaileReason = MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod;
											isAddable = false;
										}
										else
										{
											//추가할 수 있다.
											isAddable = true;
										}
									}
								}
								else
								{
									//지원되는 타입이 아니다.
									isAddable = false;
									
								}
								

								isAnySelected = true;
							}
							else if (MeshGroupTF_Main != null)
							{
								apTransform_MeshGroup curSelectedMeshGroupTF = MeshGroupTF_Main;

								_guiContent_Right2_Animation_TargetObjectName.ClearText(false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_L, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(curSelectedMeshGroupTF._nickName, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_R, true);//변경

								targetObject = curSelectedMeshGroupTF;

								//v1.4.2
								//이미 선택되었는지 여부를 먼저 확인할 수 있다.
								//최적화 및 에러로 인하여 등록시 레이어 삭제가 가능하게 만듬
								if(selectedLayer_Main != null)
								{
									if(selectedLayer_Main._linkedMeshGroupTransform == curSelectedMeshGroupTF)
									{
										//여기서 미리 빠르게 체크하자
										isRegistered = true;
									}
								}

								//레이어로 등록가능한가.
								isAddableType = curTimeline.IsLayerAddableType(curSelectedMeshGroupTF);
								if(isAddableType)
								{
									//등록 가능한 타입

									#region [미사용 코드]
									////등록 가능한 타입
									//if(!curTimeline.IsObjectAddedInLayers(curSelectedMeshGroupTF))
									//{
									//	//추가되지 않은 상태
									//	isAddable = true;
									//}
									//else
									//{
									//	//추가된 상태
									//	isAddable = false;
									//	isRegistered = true;//<<등록됨
									//} 
									#endregion

									//추가되지 않았다면 타임라인 기준으로 한번 더 확인하자
									if(!isRegistered)
									{
										if(curTimeline.IsObjectAddedInLayers(curSelectedMeshGroupTF))
										{
											//타임라인에서 확인해보니 이미 레이어로 등록된게 맞다.
											isRegistered = true;
										}
									}

									if(isRegistered)
									{
										//이미 추가된 상태라면 레이어로 등록할 수 없다.
										isAddable = false;
									}
									else
									{
										//새로 추가할 수 있다.
										isAddable = true;
									}
								}
								else
								{
									//추가할 수 없는 타입
									isAddable = false;
								}


								isAnySelected = true;
							}
							else if (Bone != null)
							{
								apBone curSelectedBone = Bone;

								_guiContent_Right2_Animation_TargetObjectName.ClearText(false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_L, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(curSelectedBone._name, false);
								_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_R, true);//변경

								targetObject = curSelectedBone;


								//v1.4.2
								//이미 선택되었는지 여부를 먼저 확인할 수 있다.
								//최적화 및 에러로 인하여 등록시 레이어 삭제가 가능하게 만듬
								if(selectedLayer_Main != null)
								{
									if(selectedLayer_Main._linkedBone == curSelectedBone)
									{
										//여기서 미리 빠르게 체크하자
										isRegistered = true;
									}
								}


								//레이어로 등록 가능한가.
								isAddableType = curTimeline.IsLayerAddableType(curSelectedBone);
								if(isAddableType)
								{
									//추가할 수 있는 타입
									#region [미사용 코드]
									//if(!curTimeline.IsObjectAddedInLayers(curSelectedBone))
									//{
									//	//추가되지 않은 상태
									//	isAddable = true;
									//}
									//else
									//{
									//	//추가된 상태
									//	isAddable = false;
									//	isRegistered = true;//<<등록됨
									//} 
									#endregion

									//추가되지 않았다면 타임라인 기준으로 한번 더 확인하자
									if(!isRegistered)
									{
										if(curTimeline.IsObjectAddedInLayers(curSelectedBone))
										{
											//타임라인에서 확인해보니 이미 레이어로 등록된게 맞다.
											isRegistered = true;
										}
									}

									if(isRegistered)
									{
										//이미 추가된 상태라면 레이어로 등록할 수 없다.
										isAddable = false;
									}
									else
									{
										//추가되지 않은 상태라면 레이어로 등록 가능하다.
										isAddable = true;
									}
								}
								else
								{
									//추가할 수 없는 타입
									isAddable = false;
								}
								

								isAnySelected = true;
							}
							else
							{
								_guiContent_Right2_Animation_TargetObjectName.ClearText(true);//추가
							}
						}


						//한번에 레이어를 추가할 수 있다.
						//이전
						//isAddingLayerOnce = true;

						//변경 20.7.5 : Morph 타임라인에 Bone을 추가할 수는 없다.
						//한번에 추가하기도 그때그때 다르다.
						if((int)(curTimeline._linkedModifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0
							|| curTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedColorOnly)
						{
							//Vertex Pos 타입의 모디파이어라면
							//메시 탭에서만 추가할 수 있다.
							if (_meshGroupChildHierarchy_Anim == MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
							{
								isAddingLayerOnce_MeshOrBone = true;
							}
						}
						else
						{
							//그 외에는 항상 가능
							isAddingLayerOnce_MeshOrBone = true;
						}

						isAddChildTransformAddable = curTimeline._linkedModifier.IsTarget_ChildMeshTransform;
					}
					
					
					break;


				case apAnimClip.LINK_TYPE.ControlParam:
					{
						if (SelectedControlParamOnAnimClip != null)
						{
							apControlParam curSelectedControlParam = SelectedControlParamOnAnimClip;
							
							_guiContent_Right2_Animation_TargetObjectName.ClearText(false);
							_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_L, false);
							_guiContent_Right2_Animation_TargetObjectName.AppendText(curSelectedControlParam._keyName, false);
							_guiContent_Right2_Animation_TargetObjectName.AppendText(apStringFactory.I.Bracket_2_R, true);//변경

							targetObject = curSelectedControlParam;


							//레이어로 등록 가능한가
							//isAddableType = curTimeline.IsLayerAddableType(curSelectedControlParam); //>이건 체크할 필요가 없다. 모든 ControlParam이 이 타임라인에 등록할 수 있기 때문
							
							//v1.4.2
							//이미 선택되었는지 여부를 먼저 확인할 수 있다.
							//최적화 및 에러로 인하여 등록시 레이어 삭제가 가능하게 만듬
							if(selectedLayer_Main != null)
							{
								if(selectedLayer_Main._linkedControlParam == curSelectedControlParam)
								{
									//여기서 미리 빠르게 체크하자
									isRegistered = true;
								}
							}

							#region [미사용 코드]
							//if(!curTimeline.IsObjectAddedInLayers(curSelectedControlParam))
							//{
							//	//추가되지 않음
							//	isAddable = true;
							//}
							//else
							//{
							//	//추가된 상태
							//	isAddable = false;
							//	isRegistered = true;//<<등록됨
							//} 
							#endregion

							//추가되지 않았다면 타임라인 기준으로 한번 더 확인하자
							if(!isRegistered)
							{
								if(curTimeline.IsObjectAddedInLayers(curSelectedControlParam))
								{
									//타임라인에서 확인해보니 이미 레이어로 등록된게 맞다.
									isRegistered = true;
								}
							}

							if(isRegistered)
							{
								//이미 추가된 상태라면
								isAddable = false;
							}
							else
							{
								//추가되지 않은 상태라면 새로 등록이 가능하다.
								isAddable = true;
							}

							isAnySelected = true;
						}
						else
						{
							_guiContent_Right2_Animation_TargetObjectName.ClearText(true);//추가
						}

						//이전 : 전체 추가 불가
						//isAddingLayerOnce = false;

						//변경 21.3.10 : 모든 컨트롤 파라미터 추가 가능
						isAddingLayerOnce_ControlParams = true;
					}
					break;

				default:
					{
						_guiContent_Right2_Animation_TargetObjectName.ClearText(true);//추가
					}
					break;
			}
			bool isRemoveTimeline = false;

			bool isRemoveTimelineLayer_Single = false;
			bool isRemoveTimelineLayer_Multiple = false;
			
			//삭제 요청 > 단일 / 다중
			apAnimTimelineLayer removeLayer = null;
			List<apAnimTimelineLayer> removeLayers = null;//<<삭제할때 리스트 생성

			//추가 : 추가 가능한 모든 객체에 대해서 TimelineLayer를 추가한다.
			if (isAddingLayerOnce_MeshOrBone)
			{
				//string strTargetObject = "";
				bool isTargetTF = true;
				//Texture2D addIconImage = null;

				//변경
				if(_guiContent_Right_Animation_AllObjectToLayers == null)
				{
					_guiContent_Right_Animation_AllObjectToLayers = new apGUIContentWrapper();
				}

				if (_meshGroupChildHierarchy_Anim == MESHGROUP_CHILD_HIERARCHY.ChildMeshes)
				{
					isTargetTF = true;
					_guiContent_Right_Animation_AllObjectToLayers.SetText(Editor.GetUIWordFormat(UIWORD.AllObjectToLayers, Editor.GetUIWord(UIWORD.Meshes)));
					_guiContent_Right_Animation_AllObjectToLayers.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddAllMeshesToLayer));
				}
				else
				{
					isTargetTF = false;
					_guiContent_Right_Animation_AllObjectToLayers.SetText(Editor.GetUIWordFormat(UIWORD.AllObjectToLayers, Editor.GetUIWord(UIWORD.Bones)));
					_guiContent_Right_Animation_AllObjectToLayers.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddAllBonesToLayer));
				}
				
				//이전
				//if (GUILayout.Button(new GUIContent(Editor.GetUIWordFormat(UIWORD.AllObjectToLayers, strTargetObject), addIconImage), GUILayout.Height(30)))

				//변경
				if (GUILayout.Button(_guiContent_Right_Animation_AllObjectToLayers.Content, apGUIStyleWrapper.I.Button_WrapText, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40)))
				{
					//v1.4.2 : FFD가 켜져있다면, 적용할지 여부를 먼저 물어봐야 한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.AddAllObjects2Timeline_Title),
																	//Editor.GetTextFormat(TEXT.AddAllObjects2Timeline_Body, strTargetObject),//이전
																	Editor.GetTextFormat(TEXT.AddAllObjects2Timeline_Body, isTargetTF ? Editor.GetUIWord(UIWORD.Meshes) : Editor.GetUIWord(UIWORD.Bones)),//변경 19.12.23
																	Editor.GetText(TEXT.Okay),
																	Editor.GetText(TEXT.Cancel)
																	);

						if (isResult)
						{
							//모든 객체를 TimelineLayer로 등록한다.
							Editor.Controller.AddAnimTimelineLayerForAllTransformObject(	animClip._targetMeshGroup,
																							isTargetTF,
																							isAddChildTransformAddable,
																							curTimeline);
						}
					}
				}
			}
			else if(isAddingLayerOnce_ControlParams)
			{
				//추가 21.3.10
				//모든 파라미터 추가도 가능하다.
				if(_guiContent_Right_Animation_AllObjectToLayers == null)
				{
					_guiContent_Right_Animation_AllObjectToLayers = new apGUIContentWrapper();
				}

				_guiContent_Right_Animation_AllObjectToLayers.SetText(Editor.GetUIWordFormat(UIWORD.AllObjectToLayers, Editor.GetUIWord(UIWORD.ControlParameters)));
				_guiContent_Right_Animation_AllObjectToLayers.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddAllControlParamsToLayer));

				//변경
				if (GUILayout.Button(_guiContent_Right_Animation_AllObjectToLayers.Content, apGUIStyleWrapper.I.Button_WrapText, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(40)))
				{
					//v1.4.2 : FFD가 켜져있다면, 적용할지 여부를 먼저 물어봐야 한다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.AddAllObjects2Timeline_Title),
																Editor.GetTextFormat(TEXT.AddAllObjects2Timeline_Body, Editor.GetUIWord(UIWORD.ControlParameters)),
																Editor.GetText(TEXT.Okay),
																Editor.GetText(TEXT.Cancel)
																);

						if (isResult)
						{
							//모든 객체를 TimelineLayer로 등록한다.
							Editor.Controller.AddAnimTimelineLayerForAllTransformObject(animClip._targetMeshGroup,
																							false,
																							false,
																							curTimeline);
						}
					}
				}
			}

			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);


			//타임라인 제거
			if(_guiContent_Right_Animation_RemoveTimeline == null)
			{
				_guiContent_Right_Animation_RemoveTimeline = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveTimeline), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}
			
			if (GUILayout.Button(_guiContent_Right_Animation_RemoveTimeline.Content, apGUILOFactory.I.Height(24)))
			{
				//v1.4.2 : FFD가 켜져있다면, 적용할지 여부를 먼저 물어봐야 한다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveTimeline_Title),
																Editor.GetTextFormat(TEXT.RemoveTimeline_Body, curTimeline.DisplayName),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

					if (isResult)
					{
						isRemoveTimeline = true;
					}
				}

				
			}
			GUILayout.Space(5);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(5);

			Color prevColor = GUI.backgroundColor;


			//이전
			//Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline_SelectedObject, (_prevSelectedAnimObject != null) == (targetObject != null));//"AnimationRight2GUI_Timeline_SelectedObject"

			//변경 20.6.5
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline_Selected_Single, isAnySelected && _prevSelectedAnimObject == targetObject && !isMultiSelected);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline_Selected_Multiple, isAnySelected && isMultiSelected);
			bool isRender_Selected_Single = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline_Selected_Single);
			bool isRender_Selected_Multi = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline_Selected_Multiple);

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline_Layers, 
									(_prevSelectedAnimTimeline == _subAnimTimeline 
										&& _prevSelectedAnimTimelineLayer == curTimelineLayer_Main
										&& _prevSelectedNumAnimTimelineLayer == nTimelineLayers)//<<추가 20.6.18 : 개수도 같아야 한다. 
									|| _isIgnoreAnimTimelineGUI);
			bool isGUI_SameLayer = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimationRight2GUI_Timeline_Layers);//"AnimationRight2GUI_Timeline_Layers"
			

			if (Event.current.type == EventType.Repaint && Event.current.type != EventType.Ignore)
			{
				_prevSelectedAnimTimeline = _subAnimTimeline;
				_prevSelectedAnimTimelineLayer = curTimelineLayer_Main;
				_prevSelectedAnimObject = targetObject;
				_prevSelectedNumAnimTimelineLayer = nTimelineLayers;

				if (_isIgnoreAnimTimelineGUI)
				{
					_isIgnoreAnimTimelineGUI = false;
				}
			}

			if(_strWrapper_64 == null)
			{
				_strWrapper_64 = new apStringWrapper(64);
			}

			// -----------------------------------------
			if (isRender_Selected_Single || isRender_Selected_Multi)
			{
				//UI가 표시되는건 다음의 경우로 나뉜다.
				//1. 이미 등록된 경우
				//2. 등록 가능한 타입이며 등록 가능한 경우
				//3. 등록 가능한 타입이지만 등록 불가능한 경우 (Rigging등의 이슈)
				//4. 등록 가능한 타입이 아닌 경우

				if(isRegistered)
				{
					//1. 이미 등록된 경우 : 삭제 버튼
					//레이어에 (하나라도) 이미 등록되어 있다.
					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);
						
					//이미 등록되었음을 안내
					_strWrapper_64.Clear();
					_strWrapper_64.Append(_guiContent_Right2_Animation_TargetObjectName.Content.text, false);
					_strWrapper_64.Append(apStringFactory.I.Return, false);
					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Selected), true);
					GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

					GUI.backgroundColor = prevColor;

					//[단일] 타임라인 레이어 삭제 버튼
					if (nTimelineLayers == 1 && curTimelineLayer_Main != null)
					{
						if (_guiContent_Right_Animation_RemoveTimelineLayer == null)
						{
							_guiContent_Right_Animation_RemoveTimelineLayer = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveTimelineLayer), Editor.ImageSet.Get(apImageSet.PRESET.Anim_RemoveTimelineLayer));
						}


						if (GUILayout.Button(_guiContent_Right_Animation_RemoveTimelineLayer.Content, apGUILOFactory.I.Height(24)))
						{
							//v1.4.2 : FFD 모드가 켜져있다면 선택에 따라 이 기능은 동작하지 않을 수 있다.
							bool isExecutable = Editor.CheckModalAndExecutable();

							if (isExecutable)
							{
								bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveTimelineLayer_Title),
																Editor.GetTextFormat(TEXT.RemoveTimelineLayer_Body, curTimelineLayer_Main.DisplayName),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

								if (isResult)
								{
									isRemoveTimelineLayer_Single = true;
									if (isRender_Selected_Single)
									{
										removeLayer = curTimelineLayer_Main;
									}
								}
							}
						}
					}
					else if(nTimelineLayers > 1)
					{
						//다중 선택시 여러개 삭제하기 (20.7.17)
						if (_guiContent_Right_Animation_RemoveTimelineLayer == null)
						{
							_guiContent_Right_Animation_RemoveTimelineLayer = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveTimelineLayer), Editor.ImageSet.Get(apImageSet.PRESET.Anim_RemoveTimelineLayer));
						}

						if (GUILayout.Button(_guiContent_Right_Animation_RemoveTimelineLayer.Content, apGUILOFactory.I.Height(24)))
						{
							//v1.4.2 : FFD 모드가 켜져있다면 선택에 따라 이 기능은 동작하지 않을 수 있다.
							bool isExecutable = Editor.CheckModalAndExecutable();

							if (isExecutable)
							{
								bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveTimelineLayer_Title),
																Editor.GetTextFormat(TEXT.RemoveTimelineLayer_Multiple_Body, nTimelineLayers),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);
								if (isResult)
								{
									isRemoveTimelineLayer_Multiple = true;
									if (isRender_Selected_Multi && AnimTimelineLayers_All != null)
									{
										removeLayers = new List<apAnimTimelineLayer>();
										for (int i = 0; i < AnimTimelineLayers_All.Count; i++)
										{
											removeLayers.Add(AnimTimelineLayers_All[i]);
										}
									}
								}
							}
						}
					}
				}
				else if (isAddableType)
				{
					//추가 가능함
					if (isAddable)
					{
						//2. 등록 가능한 타입이며 등록 가능한 경우

						//아직 레이어로 추가가 되지 않았다.
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);

						//Not Added to Edit
						_strWrapper_64.Clear();
						_strWrapper_64.Append(_guiContent_Right2_Animation_TargetObjectName.Content.text, false);
						_strWrapper_64.Append(apStringFactory.I.Return, false);
						_strWrapper_64.Append(Editor.GetUIWord(UIWORD.NotAddedtoEdit), true);
						GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

						

						GUI.backgroundColor = prevColor;

						//"타임라인 레이어로 추가" 버튼
						if (_guiContent_Right_Animation_AddTimelineLayerToEdit == null)
						{
							_guiContent_Right_Animation_AddTimelineLayerToEdit = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.AddTimelineLayerToEdit), Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddTimeline));
						}
						
						//추가 22.6.10 : 타임라인 레이어에 등록하는 버튼은 반짝거린다.
						GUI.backgroundColor = apEditorUtil.GetAnimatedHighlightButtonColor();

						if (GUILayout.Button(_guiContent_Right_Animation_AddTimelineLayerToEdit.Content, apGUILOFactory.I.Height(35)))
						{	
							//v1.4.2 : FFD 모드가 켜져있다면 선택에 따라 이 기능은 동작하지 않을 수 있다.
							bool isExecutable = Editor.CheckModalAndExecutable();

							if (isExecutable)
							{
								//타임라인 레이어를 추가하자.
								if (isRender_Selected_Single)
								{
									Editor.Controller.AddAnimTimelineLayer(targetObject, curTimeline);
								}
								else
								{
									//추가 20.6.19 여러개를 타임라인 레이어로 추가하자.
									Editor.Controller.AddAnimTimelineLayersForMultipleSelection(curTimeline);
								}
							}
							
						}

						GUI.backgroundColor = prevColor;
					}
					else
					{ 
						//3. 등록 가능한 타입이지만 등록 불가능한 경우 (Rigging등의 이슈)

						//추가할 수도 없고, 등록되지도 않은 경우
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						//"[" + targetObjectName + "]\nUnable to be Added"
						//이전
						//GUILayout.Box("[" + targetObjectName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
						
						_strWrapper_64.Clear();
						_strWrapper_64.Append(_guiContent_Right2_Animation_TargetObjectName.Content.text, false);
						_strWrapper_64.Append(apStringFactory.I.Return, false);

						//추가 20.9.8 : 추가 불가 원인에 따라 다른 메시지가 나온다.
						if(modAddFaileReason == MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod)
						{
							//자식 메시의 리깅 문제
							_strWrapper_64.Append(Editor.GetUIWord(UIWORD.NotAbleToBeAdded_RiggedChildMesh), true);
						}
						else
						{
							//기본값. (지원하는 타입이 아님)
							_strWrapper_64.Append(Editor.GetUIWord(UIWORD.NotAbleToBeAdded), true);
						}

						
						GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

						GUI.backgroundColor = prevColor;
					}
				}
				else
				{
					//4. 등록 가능한 타입이 아닌 경우

					//추가 불가능한 타입
					if (targetObject != null && isRender_Selected_Single)
					{
						//추가할 수 있는 타입이 아니다.
						GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
						//"[" + targetObjectName + "]\nUnable to be Added"
						//이전
						//GUILayout.Box("[" + targetObjectName + "]\n" + Editor.GetUIWord(UIWORD.NotAbleToBeAdded), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, GUILayout.Width(width), GUILayout.Height(35));
						
						_strWrapper_64.Clear();
						_strWrapper_64.Append(_guiContent_Right2_Animation_TargetObjectName.Content.text, false);
						_strWrapper_64.Append(apStringFactory.I.Return, false);

						//추가 20.9.8 : 추가 불가 원인에 따라 다른 메시지가 나온다.
						if(modAddFaileReason == MOD_ADD_FAIL_REASON.RiggedChildMeshInTFMod)
						{
							//자식 메시의 리깅 문제
							_strWrapper_64.Append(Editor.GetUIWord(UIWORD.NotAbleToBeAdded_RiggedChildMesh), true);
						}
						else
						{
							//기본값. (지원하는 타입이 아님)
							_strWrapper_64.Append(Editor.GetUIWord(UIWORD.NotAbleToBeAdded), true);
						}

						
						GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

						GUI.backgroundColor = prevColor;
					}
					else
					{
						//객체가 없다.
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
						//"[" + targetObjectName + "]\nUnable to be Added"
						GUILayout.Box(Editor.GetUIWord(UIWORD.NotAbleToBeAdded), apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(35));

						GUI.backgroundColor = prevColor;
					}
				}


				//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(10));
				//EditorGUILayout.EndVertical();
				GUILayout.Space(11);



				//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TimelineLayers));//"Timeline Layers"
				_isFoldUI_AnimationTimelineLayers = EditorGUILayout.Foldout(_isFoldUI_AnimationTimelineLayers, Editor.GetUIWord(UIWORD.TimelineLayers));
				if (_isFoldUI_AnimationTimelineLayers)
				{
					GUILayout.Space(8);


					//현재의 타임라인 레이어 리스트를 만들어야한다.
					List<apAnimTimelineLayer> timelineLayers = curTimeline._layers;
					apAnimTimelineLayer curLayer = null;

					//레이어 정보가 Layout 이벤트와 동일한 경우에만 작동

					if (isGUI_SameLayer)
					{
						for (int i = 0; i < timelineLayers.Count; i++)
						{
							Rect lastRect = GUILayoutUtility.GetLastRect();

							curLayer = timelineLayers[i];
							if (curTimelineLayer_Main == curLayer)
							{
								//선택된 레이어다.
								GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
							}
							else
							{
								//선택되지 않은 레이어다. (다중 선택도 있지만 제외)
								GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
							}

							int heightOffset = 18;
							if (i == 0)
							{
								heightOffset = 8;//9
							}

							GUI.Box(new Rect(lastRect.x, lastRect.y + heightOffset, width + 10, 30), apStringFactory.I.None);
							GUI.backgroundColor = prevColor;

							int compWidth = width - (55 + 20 + 5 + 10);

							//GUIStyle guiStyle_Label = new GUIStyle(GUI.skin.label);
							//guiStyle_Label.alignment = TextAnchor.MiddleLeft;

							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
							GUILayout.Space(10);
							EditorGUILayout.LabelField(curLayer.DisplayName, apGUIStyleWrapper.I.Label_MiddleLeft, apGUILOFactory.I.Width(compWidth), apGUILOFactory.I.Height(20));

							if (curTimelineLayer_Main == curLayer)
							{
								//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
								//guiStyle.normal.textColor = Color.white;
								//guiStyle.alignment = TextAnchor.UpperCenter;

								GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
								GUILayout.Box(Editor.GetUIWord(UIWORD.Selected), apGUIStyleWrapper.I.Box_UpperCenter_WhiteColor, apGUILOFactory.I.Width(55), apGUILOFactory.I.Height(20));//"Selected"
								GUI.backgroundColor = prevColor;
							}
							else
							{
								if (GUILayout.Button(Editor.GetUIWord(UIWORD.Select), apGUILOFactory.I.Width(55), apGUILOFactory.I.Height(20)))//"Select"
								{
									_isIgnoreAnimTimelineGUI = true;//<깜빡이지 않게..
									SelectAnimTimelineLayer(curLayer, MULTI_SELECT.Main, true);
								}
							}

							if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), apGUILOFactory.I.Width(20), apGUILOFactory.I.Height(20)))
							{

								//v1.4.2 : FFD가 켜져있다면, 적용할지 여부를 먼저 물어봐야 한다.
								bool isExecutable = Editor.CheckModalAndExecutable();

								if (isExecutable)
								{
									bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveTimelineLayer_Title),
																			Editor.GetTextFormat(TEXT.RemoveTimelineLayer_Body, curLayer.DisplayName),
																			Editor.GetText(TEXT.Remove),
																			Editor.GetText(TEXT.Cancel)
																			);

									if (isResult)
									{
										isRemoveTimelineLayer_Single = true;
										removeLayer = curLayer;
									}
								}
							}
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);
						}
					}
				}
			}


			//----------------------------------
			// 삭제 플래그가 있다.
			if (isRemoveTimelineLayer_Single)
			{
				//[단일 선택시]
				if(isRender_Selected_Single && removeLayer != null)
				{
					Editor.Controller.RemoveAnimTimelineLayer(removeLayer);
				}
				
				SelectAnimTimelineLayer(null, MULTI_SELECT.Main, true, true);
				SetAnimClipGizmoEvent();
			}
			else if(isRemoveTimelineLayer_Multiple)
			{
				//[다중 선택시] (20.7.17)
				if(isRender_Selected_Multi && removeLayers != null && removeLayers.Count > 0)
				{
					Editor.Controller.RemoveAnimTimelineLayers(removeLayers, AnimClip);
				}
			}
			else if (isRemoveTimeline)
			{
				Editor.Controller.RemoveAnimTimeline(curTimeline);
				SelectAnimTimeline(null, true);
				SetAnimClipGizmoEvent();

			}

		}










		//---------------------------------------------------------------------------
		// 아래 UI 그리기
		//---------------------------------------------------------------------------
		




		//--------------------------------------------------------------------------------------
		public void DrawEditor_Bottom_EditButtons(int width, int height)
		{
			if (Editor == null || Editor.Select.Portrait == null || Modifier == null)
			{
				return;
			}

			bool isRiggingModifier = (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging);
			

			//기본 Modifier가 있고
			//Rigging용 Modifier UI가 따로 있다.
			//추가 : Weight값을 사용하는 Physic/Volume도 따로 설정
			int editToggleWidth = 160;//140 > 180
			if (isRiggingModifier)
			{
				//리깅 타입인 경우
				//리깅 편집 툴 / 보기 버튼들이 나온다.
				//1. Rigging On/Off
				//+ 선택된 Mesh Transform
				//2. View 모드
				//3. Test Posing On/Off
				//"  Binding..", "  Start Binding"
				if (apEditorUtil.ToggledButton_2Side_LeftAlign(Editor.ImageSet.Get(apImageSet.PRESET.Rig_EditBinding),
														2,
														Editor.GetUIWord(UIWORD.ModBinding),
														Editor.GetUIWord(UIWORD.ModStartBinding),
														
														//_rigEdit_isBindingEdit,//이전
														_exclusiveEditing == EX_EDIT.ExOnly_Edit,//변경 22.5.15
														
														true, editToggleWidth, height,
														apStringFactory.I.GetHotkeyTooltip_BindingModeToggle(Editor.HotKeyMap)
														))//"Enable/Disable Bind Mode (A)"
				{
					//변경 22.5.15
					ToggleRigEditBinding();
				}
				GUILayout.Space(10);

			}
			else
			{
				//그외의 Modifier
				//편집 On/Off와 현재 선택된 Key/Value가 나온다.
				//"  Editing..", "  Start Editing", "  Not Editiable"
				if (apEditorUtil.ToggledButton_2Side_LeftAlign(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Recording),
													Editor.ImageSet.Get(apImageSet.PRESET.Edit_Record),
													Editor.ImageSet.Get(apImageSet.PRESET.Edit_NoRecord),
													2, Editor.GetUIWord(UIWORD.ModEditing),
													Editor.GetUIWord(UIWORD.ModStartEditing),
													Editor.GetUIWord(UIWORD.ModNotEditable),
													_exclusiveEditing != EX_EDIT.None,
													IsExEditable,
													editToggleWidth, height,
													apStringFactory.I.GetHotkeyTooltip_EditModeToggle(Editor.HotKeyMap)
													))//"Enable/Disable Edit Mode (A)"
				{
					//v1.4.2 : FFD일땐 토글이 제한될 수 있다.
					bool isExecutable = Editor.CheckModalAndExecutable();

					if(isExecutable)
					{
						ToggleRigEditBinding();
					}
					
				}


				


				GUILayout.Space(10);
				//Lock 걸린 키 / 수정중인 객체 / 그 값을 각각 표시하자

			}


			//기존
			//변경 : Ctrl을 누르고 Edit 버튼을 누르면 Editing Setting 다이얼로그가 열린다.
			if (apEditorUtil.ToggledButton_2Side_Ctrl(Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionLock),
												Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionUnlock),
												IsSelectionLock, true, height, height,
												apStringFactory.I.GetHotkeyTooltip_SelectionLockToggle(Editor.HotKeyMap),
												Event.current.control, Event.current.command
												))//"Selection Lock/Unlock (S)"
			{

				//변경 3.22 : Ctrl 키를 누르고 클릭하면 설정 Dialog가 뜬다.
#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
				bool isCtrl = Event.current.control;
#endif
				if (isCtrl)
				{
					apDialog_ModifierLockSetting.ShowDialog(Editor, _portrait);
				}
				else
				{
					SetModifierSelectionLock(!IsSelectionLock);
				}
			}

			//GUILayout.Space(10);


#region [미사용 코드] 모디파이어 잠금 버튼. 21.2.13 에서 삭제되었다.

			//			//#if UNITY_EDITOR_OSX
			//			//			string strCtrlKey = "Command";
			//			//#else
			//			//			string strCtrlKey = "Ctrl";
			//			//#endif




			//			if (apEditorUtil.ToggledButton_2Side_Ctrl(Editor.ImageSet.Get(apImageSet.PRESET.Edit_ExModOption),
			//												Editor.ImageSet.Get(apImageSet.PRESET.Edit_ExModOption),
			//												_exclusiveEditing == EX_EDIT.ExOnly_Edit,
			//												IsExEditable && _exclusiveEditing != EX_EDIT.None,
			//												height, height,
			//												apStringFactory.I.GetHotkeyTooltip_ModifierLockToggle(Editor.HotKeyMap),
			//												Event.current.control,
			//												Event.current.command))////"Modifier Lock/Unlock (D) / If you press the button while holding down [" + strCtrlKey + "], the Setting dialog opens",
			//			{
			//				//여기서 ExOnly <-> General 사이를 바꾼다.

			//				//변경 3.22 : Ctrl 키를 누르고 클릭하면 설정 Dialog가 뜬다.
			//#if UNITY_EDITOR_OSX
			//				bool isCtrl = Event.current.command;
			//#else
			//				bool isCtrl = Event.current.control;
			//#endif
			//				if (isCtrl)
			//				{
			//					apDialog_ModifierLockSetting.ShowDialog(Editor, _portrait);
			//				}
			//				else
			//				{
			//					if (IsExEditable && _exclusiveEditing != EX_EDIT.None)
			//					{
			//						EX_EDIT nextEditMode = EX_EDIT.ExOnly_Edit;
			//						if (_exclusiveEditing == EX_EDIT.ExOnly_Edit)
			//						{
			//							nextEditMode = EX_EDIT.General_Edit;
			//						}
			//						SetModifierExclusiveEditing(nextEditMode);
			//					}
			//				}
			//			} 
#endregion


			//토글 단축키를 입력하자
			//[A : Editor Toggle]
			//[S (Space에서 S로 변경) : Selection Lock]
			//[D : Modifier Lock)
			
			//변경 20.12.3
			Editor.AddHotKeyEvent(OnHotKeyEvent_ToggleModifierEditing, apHotKeyMapping.KEY_TYPE.ToggleEditingMode, null);//"Toggle Editing Mode"
			Editor.AddHotKeyEvent(OnHotKeyEvent_ToggleExclusiveEditKeyLock, apHotKeyMapping.KEY_TYPE.ToggleSelectionLock, null);//"Toggle Selection Lock"

			//삭제 21.2.13 : 모디파이어 잠금 삭제 > 각 기능이 통폐합되었다.
			//Editor.AddHotKeyEvent(OnHotKeyEvent_ToggleExclusiveModifierLock, apHotKeyMapping.KEY_TYPE.ToggleModifierLock, null);//"Toggle Modifier Lock"

			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxV(height - 6);
			GUILayout.Space(10);

#region [미사용 코드] 더이상 선택된 객체 정보를 출력하지 않는다. (다중 선택도 있고 불필요하다고 판단) 20.6.12
			//apImageSet.PRESET modImagePreset = apEditorUtil.GetModifierIconType(Modifier.ModifierType);

			//if (_guiContent_Bottom_EditMode_CommonIcon == null)
			//{
			//	_guiContent_Bottom_EditMode_CommonIcon = new apGUIContentWrapper();
			//}

			////이전
			////GUIStyle guiStyle_Key = new GUIStyle(GUI.skin.label);
			////if (IsSelectionLock)
			////{
			////	guiStyle_Key.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			////}

			////변경
			//GUIStyle guiStyle_Key = IsSelectionLock ? apGUIStyleWrapper.I.Label_RedColor : apGUIStyleWrapper.I.Label;//최적화된 코드

			////이전
			////GUIStyle guiStyle_NotSelected = new GUIStyle(GUI.skin.label);
			////guiStyle_NotSelected.normal.textColor = new Color(0.0f, 0.5f, 1.0f, 1.0f);

			////변경
			//GUIStyle guiStyle_NotSelected = apGUIStyleWrapper.I.Label_LightBlueColor;//최적화된 코드


			//int paramSetWidth = 140;//100 -> 140
			//int modValueWidth = 200;//170 -> 200

			//switch (_exEditKeyValue)
			//{
			//	case EX_EDIT_KEY_VALUE.None:
			//		break;

			//	case EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert:
			//	case EX_EDIT_KEY_VALUE.ParamKey_ModMesh://ModVert와 ModMesh는 비슷하다
			//		{
			//			//Key
			//			//EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(modImagePreset)), GUILayout.Width(height), GUILayout.Height(height));

			//			_guiContent_Bottom_EditMode_CommonIcon.SetImage(Editor.ImageSet.Get(modImagePreset));
			//			EditorGUILayout.LabelField(_guiContent_Bottom_EditMode_CommonIcon.Content, apGUILOFactory.I.Width(height), apGUILOFactory.I.Height(height));


			//			Texture2D selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);

			//			string strKey_ParamSetGroup = Editor.GetUIWord(UIWORD.ModNoParam);//"<No Parameter>"
			//			string strKey_ParamSet = Editor.GetUIWord(UIWORD.ModNoKey);//"<No Key>"
			//			string strKey_ModMesh = Editor.GetUIWord(UIWORD.ModNoSelected);//"<Not Selected>"
			//			string strKey_ModMeshLabel = Editor.GetUIWord(UIWORD.ModSubObject);//"Sub Object"

			//			GUIStyle guiStyle_ParamSetGroup = guiStyle_NotSelected;//<<최적화된 코드
			//			GUIStyle guiStyle_ParamSet = guiStyle_NotSelected;//<<최적화된 코드
			//			GUIStyle guiStyle_Transform = guiStyle_NotSelected;//<<최적화된 코드

			//			if (ExKey_ModParamSetGroup != null)
			//			{
			//				if (ExKey_ModParamSetGroup._keyControlParam != null)
			//				{
			//					strKey_ParamSetGroup = ExKey_ModParamSetGroup._keyControlParam._keyName;
			//					guiStyle_ParamSetGroup = guiStyle_Key;
			//				}
			//			}

			//			if (ExKey_ModParamSet != null)
			//			{
			//				//TODO : 컨트롤 타입이 아니면 다른 이름을 쓰자
			//				strKey_ParamSet = ExKey_ModParamSet.ControlParamValue;
			//				guiStyle_ParamSet = guiStyle_Key;
			//			}

			//			apModifiedMesh modMesh = null;
			//			if (_exEditKeyValue == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
			//			{
			//				modMesh = ExKey_ModMesh;
			//			}
			//			else
			//			{
			//				modMesh = ExValue_ModMesh;
			//			}

			//			if (modMesh != null)
			//			{
			//				if (modMesh._transform_Mesh != null)
			//				{
			//					strKey_ModMeshLabel = Editor.GetUIWord(UIWORD.Mesh);//>그냥 Mesh로 표현
			//					strKey_ModMesh = modMesh._transform_Mesh._nickName;
			//					selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//					guiStyle_Transform = guiStyle_Key;
			//				}
			//				else if (modMesh._transform_MeshGroup != null)
			//				{
			//					strKey_ModMeshLabel = Editor.GetUIWord(UIWORD.MeshGroup);//>그냥 MeshGroup으로 표현
			//					strKey_ModMesh = modMesh._transform_MeshGroup._nickName;
			//					selectedImage = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			//					guiStyle_Transform = guiStyle_Key;
			//				}
			//			}
			//			else
			//			{
			//				if (ExKey_ModParamSet == null)
			//				{
			//					//Key를 먼저 선택할 것을 알려야한다.
			//					strKey_ModMesh = Editor.GetUIWord(UIWORD.ModSelectKeyFirst);//"<Select Key First>"
			//				}
			//			}

			//			if (Modifier.SyncTarget != apModifierParamSetGroup.SYNC_TARGET.Static)
			//			{
			//				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(paramSetWidth), apGUILOFactory.I.Height(height));
			//				EditorGUILayout.LabelField(strKey_ParamSetGroup, guiStyle_ParamSetGroup, apGUILOFactory.I.Width(paramSetWidth));
			//				EditorGUILayout.LabelField(strKey_ParamSet, guiStyle_ParamSet, apGUILOFactory.I.Width(paramSetWidth));
			//				EditorGUILayout.EndVertical();
			//			}
			//			else
			//			{
			//				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(paramSetWidth), apGUILOFactory.I.Height(height));
			//				EditorGUILayout.LabelField(Modifier.DisplayName, guiStyle_Key, apGUILOFactory.I.Width(paramSetWidth));
			//				//EditorGUILayout.LabelField(strKey_ParamSet, guiStyle_ParamSet, GUILayout.Width(100));
			//				EditorGUILayout.EndVertical();
			//			}

			//			//EditorGUILayout.LabelField(new GUIContent(selectedImage), GUILayout.Width(height), GUILayout.Height(height));

			//			_guiContent_Bottom_EditMode_CommonIcon.SetImage(selectedImage);
			//			EditorGUILayout.LabelField(_guiContent_Bottom_EditMode_CommonIcon.Content, apGUILOFactory.I.Width(height), apGUILOFactory.I.Height(height));

			//			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(modValueWidth), apGUILOFactory.I.Height(height));
			//			EditorGUILayout.LabelField(strKey_ModMeshLabel, apGUILOFactory.I.Width(modValueWidth));
			//			EditorGUILayout.LabelField(strKey_ModMesh, guiStyle_Transform, apGUILOFactory.I.Width(modValueWidth));
			//			EditorGUILayout.EndVertical();


			//			GUILayout.Space(10);
			//			apEditorUtil.GUI_DelimeterBoxV(height - 6);
			//			GUILayout.Space(10);

			//			//Value
			//			//(선택한 Vert의 값을 출력하자. 단, Rigging Modifier가 아닐때)
			//			if (_exEditKeyValue == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert && !isRiggingModifier && !isWeightedVertModifier)
			//			{

			//				bool isModVertSelected = (ExValue_ModVert != null);
			//				Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom2_Transform_Mod_Vert, isModVertSelected);//"Bottom2 Transform Mod Vert"

			//				if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom2_Transform_Mod_Vert))//"Bottom2 Transform Mod Vert"
			//				{
			//					//EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Vertex)), GUILayout.Width(height), GUILayout.Height(height));
			//					_guiContent_Bottom_EditMode_CommonIcon.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Vertex));
			//					EditorGUILayout.LabelField(_guiContent_Bottom_EditMode_CommonIcon.Content, apGUILOFactory.I.Width(height), apGUILOFactory.I.Height(height));

			//					EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(150), apGUILOFactory.I.Height(height));

			//					//"Vertex : " + ExValue_ModVert._modVert._vertexUniqueID

			//					if (_strWrapper_64 == null)
			//					{
			//						_strWrapper_64 = new apStringWrapper(64);
			//					}
			//					_strWrapper_64.Clear();
			//					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Vertex), false);
			//					_strWrapper_64.Append(apStringFactory.I.Colon_Space, false);
			//					_strWrapper_64.Append(ExValue_ModVert._modVert._vertexUniqueID, true);

			//					//EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Vertex) + " : " + ExValue_ModVert._modVert._vertexUniqueID, apGUILOFactory.I.Width(150));
			//					EditorGUILayout.LabelField(_strWrapper_64.ToString(), apGUILOFactory.I.Width(150));

			//					//Vector2 newDeltaPos = EditorGUILayout.Vector2Field("", ExValue_ModVert._modVert._deltaPos, GUILayout.Width(150));
			//					Vector2 newDeltaPos = apEditorUtil.DelayedVector2Field(ExValue_ModVert._modVert._deltaPos, 150);
			//					if (ExEditingMode != EX_EDIT.None)
			//					{
			//						ExValue_ModVert._modVert._deltaPos = newDeltaPos;
			//					}
			//					EditorGUILayout.EndVertical();
			//				}
			//			}


			//		}
			//		break;

			//	case EX_EDIT_KEY_VALUE.ParamKey_Bone:
			//		{
			//			//EditorGUILayout.LabelField(new GUIContent(Editor.ImageSet.Get(modImagePreset)), GUILayout.Width(height), GUILayout.Height(height));

			//			_guiContent_Bottom_EditMode_CommonIcon.SetImage(Editor.ImageSet.Get(modImagePreset));
			//			EditorGUILayout.LabelField(_guiContent_Bottom_EditMode_CommonIcon.Content, apGUILOFactory.I.Width(height), apGUILOFactory.I.Height(height));
			//		}
			//		break;

			//} 
#endregion

			if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 타입이면 몇가지 제어 버튼이 추가된다.
				//2. View 모드
				//3. Test Posing On/Off

				//View 모드는 Weight+Texture 여부 / Bone Color / Circle<->Square Vertex의 세가지로 구성된다.
				if (apEditorUtil.ToggledButton_2Side(
					Editor.ImageSet.Get(apImageSet.PRESET.Rig_WeightColorOnly),
					Editor.ImageSet.Get(apImageSet.PRESET.Rig_WeightColorWithTexture),
					Editor._rigViewOption_WeightOnly, true, height + 5, height, apStringFactory.I.RiggingViewModeTooltip_ColorWithTexture))//"Whether to render the Rigging weight with the texture of the image"
				{
					Editor._rigViewOption_WeightOnly = !Editor._rigViewOption_WeightOnly;
					Editor.SaveEditorPref();
				}

				GUILayout.Space(2);

				//"Bone Color", "Bone Color"
				if (apEditorUtil.ToggledButton_2Side(
					Editor.ImageSet.Get(apImageSet.PRESET.Rig_BoneColor),
					Editor.ImageSet.Get(apImageSet.PRESET.Rig_NoBoneColor),
					Editor._rigViewOption_BoneColor,
					true,
					height + 5, height, apStringFactory.I.RiggingViewModeTooltip_BoneColor))//"Whether to render the Rigging weight by the color of the Bone"
				{
					Editor._rigViewOption_BoneColor = !Editor._rigViewOption_BoneColor;
					Editor.SaveEditorPref();//<<이것도 Save 요건
				}

				GUILayout.Space(2);

				if (apEditorUtil.ToggledButton_2Side(
					Editor.ImageSet.Get(apImageSet.PRESET.Rig_CircleVert),
					Editor.ImageSet.Get(apImageSet.PRESET.Rig_SquareColorVert),
					Editor._rigViewOption_CircleVert,
					true,
					height + 5, height, apStringFactory.I.RiggingViewModeTooltip_CircleVert))//"Whether to render vertices into circular shapes"
				{
					Editor._rigViewOption_CircleVert = !Editor._rigViewOption_CircleVert;
					Editor.SaveEditorPref();//<<이것도 Save 요건
				}

				Texture2D iconNoLinkedBone = null;
				switch (Editor._rigGUIOption_NoLinkedBoneVisibility)
				{
					case apEditor.NOLINKED_BONE_VISIBILITY.Opaque: iconNoLinkedBone = Editor.ImageSet.Get(apImageSet.PRESET.Rig_ShowAllBones); break;
					case apEditor.NOLINKED_BONE_VISIBILITY.Translucent: iconNoLinkedBone = Editor.ImageSet.Get(apImageSet.PRESET.Rig_TransculentBones); break;
					case apEditor.NOLINKED_BONE_VISIBILITY.Hidden: iconNoLinkedBone = Editor.ImageSet.Get(apImageSet.PRESET.Rig_HideBones); break;
				}

				if (apEditorUtil.ToggledButton_2Side_Ctrl(
					iconNoLinkedBone, iconNoLinkedBone,
					Editor._rigGUIOption_NoLinkedBoneVisibility != apEditor.NOLINKED_BONE_VISIBILITY.Opaque,
					true,
					height + 5, height, apStringFactory.I.RiggingViewModeTooltip_NoLinkedBoneVisibility,
					Event.current.control, Event.current.command))//"Whether to render vertices into circular shapes"
				{
					//클릭할 때마다 하나씩 다음 단계로 이동. Ctrl를 누르면 반대로 이동
#if UNITY_EDITOR_OSX
					if(Event.current.command)
#else
					if (Event.current.control)
#endif
					{
						switch (Editor._rigGUIOption_NoLinkedBoneVisibility)
						{
							case apEditor.NOLINKED_BONE_VISIBILITY.Opaque: Editor._rigGUIOption_NoLinkedBoneVisibility = apEditor.NOLINKED_BONE_VISIBILITY.Hidden; break;
							case apEditor.NOLINKED_BONE_VISIBILITY.Translucent: Editor._rigGUIOption_NoLinkedBoneVisibility = apEditor.NOLINKED_BONE_VISIBILITY.Opaque; break;
							case apEditor.NOLINKED_BONE_VISIBILITY.Hidden: Editor._rigGUIOption_NoLinkedBoneVisibility = apEditor.NOLINKED_BONE_VISIBILITY.Translucent; break;
						}
					}
					else
					{
						switch (Editor._rigGUIOption_NoLinkedBoneVisibility)
						{
							case apEditor.NOLINKED_BONE_VISIBILITY.Opaque: Editor._rigGUIOption_NoLinkedBoneVisibility = apEditor.NOLINKED_BONE_VISIBILITY.Translucent; break;
							case apEditor.NOLINKED_BONE_VISIBILITY.Translucent: Editor._rigGUIOption_NoLinkedBoneVisibility = apEditor.NOLINKED_BONE_VISIBILITY.Hidden; break;
							case apEditor.NOLINKED_BONE_VISIBILITY.Hidden: Editor._rigGUIOption_NoLinkedBoneVisibility = apEditor.NOLINKED_BONE_VISIBILITY.Opaque; break;
						}
					}
					Editor.SaveEditorPref();//<<이것도 Save 요건
				}



				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxV(height - 6);
				GUILayout.Space(10);

				//"  Pose Test", "  Pose Test"
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_TestPosing),
													2, Editor.GetUIWord(UIWORD.RigPoseTest), Editor.GetUIWord(UIWORD.RigPoseTest),
													_rigEdit_isTestPosing,
													
													//_rigEdit_isBindingEdit,//이전
													_exclusiveEditing == EX_EDIT.ExOnly_Edit,//변경 22.5.15

													130, height,
													apStringFactory.I.RiggingViewModeTooltip_TestPose))//"Enable/Disable Pose Test Mode"
				{
					_rigEdit_isTestPosing = !_rigEdit_isTestPosing;

					SetBoneRiggingTest();
				}

				if (GUILayout.Button(Editor.GetUIWord(UIWORD.RigResetPose), apGUILOFactory.I.Width(120), apGUILOFactory.I.Height(height)))//"Reset Pose"
				{
					ResetRiggingTestPose();
				}
			}
			else if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
			{
				//테스트로 시뮬레이션을 할 수 있다.
				//바람을 켜고 끌 수 있다.
				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(100), apGUILOFactory.I.Height(height));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PxDirection), apGUILOFactory.I.Width(100));//"Direction"
				_physics_windSimulationDir = apEditorUtil.DelayedVector2Field(_physics_windSimulationDir, 100 - 4);
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(100), apGUILOFactory.I.Height(height));
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PxPower), apGUILOFactory.I.Width(100));//"Power"
				_physics_windSimulationScale = EditorGUILayout.DelayedFloatField(_physics_windSimulationScale, apGUILOFactory.I.Width(100));
				EditorGUILayout.EndVertical();

				//"Wind On"

				if (_guiContent_Bottom2_Physic_WindON == null)
				{
					_guiContent_Bottom2_Physic_WindON = apGUIContentWrapper.Make(Editor.GetUIWord(UIWORD.PxWindOn), false, apStringFactory.I.SimulateWindForce);//"Simulate wind force"
				}

				if (_guiContent_Bottom2_Physic_WindOFF == null)
				{
					_guiContent_Bottom2_Physic_WindOFF = apGUIContentWrapper.Make(Editor.GetUIWord(UIWORD.PxWindOff), false, apStringFactory.I.ClearWindForce);//"Clear wind force"
				}

				if (GUILayout.Button(_guiContent_Bottom2_Physic_WindON.Content, apGUILOFactory.I.Width(110), apGUILOFactory.I.Height(height)))
				{
					GUI.FocusControl(null);

					if (_portrait != null)
					{
						_portrait.ClearForce();
						_portrait.AddForce_Direction(_physics_windSimulationDir,
							0.3f,
							0.3f,
							3, 5)
							.SetPower(_physics_windSimulationScale, _physics_windSimulationScale * 0.3f, 4.0f)
							.EmitLoop();
					}
				}
				//"Wind Off"
				if (GUILayout.Button(_guiContent_Bottom2_Physic_WindOFF.Content, apGUILOFactory.I.Width(110), apGUILOFactory.I.Height(height)))
				{
					GUI.FocusControl(null);
					if (_portrait != null)
					{
						_portrait.ClearForce();
					}
				}


			}

			return;
		}
		//------------------------------------------------------------------------------------


		public void DrawEditor_Bottom_Timeline(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			if (_portrait == null)
			{
				return;
			}

			//애니메이션 외에는 이 그려지지 않는다.
			if(_selectionType != SELECTION_TYPE.Animation)
			{
				return;
			}

			DrawEditor_Bottom_Animation(width, height, layoutX, layoutY, windowWidth, windowHeight);

			return;
		}




		private void DrawEditor_Bottom_Animation(int width, int height, int layoutX, int layoutY, int windowWidth, int windowHeight)
		{
			//좌우 두개의 탭으로 나뉜다. [타임라인 - 선택된 객체 정보]
			int rightTabWidth = 300;
			int margin = 5;
			int mainTabWidth = width - (rightTabWidth + margin);
			Rect lastRect = GUILayoutUtility.GetLastRect();

			List<apTimelineLayerInfo> timelineInfoList = Editor.TimelineInfoList;
			int nTimelineInfoList = timelineInfoList != null ? timelineInfoList.Count : 0;
			apTimelineLayerInfo nextSelectLayerInfo = null;

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(height));
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainTabWidth), apGUILOFactory.I.Height(height));
			//1. [좌측] 타임라인 레이아웃

			//1-1 요약부 : [레코드] + [타임과 통합 키프레임]
			//1-2 메인 타임라인 : [레이어] + [타임라인 메인]
			//1-3 하단 컨트롤과 스크롤 : [컨트롤러] + [스크롤 + 애니메이션 설정]
			int leftTabWidth = 280;
			int timelineWidth = mainTabWidth - (leftTabWidth + 4);

			if (Event.current.type == EventType.Repaint)
			{
				_timlineGUIWidth = timelineWidth;
			}

			//int recordAndSummaryHeight = 45;
			int recordAndSummaryHeight = 70;
			int bottomControlHeight = 54;
			int timelineHeight = height - (recordAndSummaryHeight + bottomControlHeight + 4);
			int guiHeight = height - bottomControlHeight;


			//레이어의 높이
			int heightPerTimeline = 24;
			int heightPerLayer = 28;//조금 작게 만들자

			//>>원래 자동 스크롤 코드가 있던 위치




			//if(Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size1)
			//{
			//	guiHeight = viewAndSummaryHeight;
			//}

			bool isDrawMainTimeline = (Editor._timelineLayoutSize != apEditor.TIMELINE_LAYOUTSIZE.Size1);

			//스크롤 값을 넣어주자
			int startFrame = AnimClip.StartFrame;
			int endFrame = AnimClip.EndFrame;
			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;

			int timelineLayoutSize_Min = 0;
			int timelineLayoutSize_Max = Editor._timelineZoomWPFPreset.Length - 1;



			//세로 스크롤 영역에 대한 처리 (20.4.14)
			//스크롤값에 따라서 일부 타임라인 레이어의 GUI가 렌더링되지 않는데,
			//이때문에 GUI 출력 순서가 바뀌어서 세로 스크롤의 GUI의 포커싱이 풀리는 문제가 있다. (스크롤 도중에 스크롤바 인식이 풀리는 문제)
			//그래서 스크롤하는 도중에는 미리 Down이벤트를 감지하여 높이에 따른 렌더링 생략 로직을 피하고 모두 렌더링해야한다.
			Rect timelineVerticalScrollRect = new Rect(	lastRect.x + leftTabWidth + 4 + timelineWidth - 15,
														lastRect.y,
														15,
														timelineHeight + recordAndSummaryHeight + 4);
			if(Event.current.type == EventType.MouseDown)
			{
				Vector2 mousePos = Event.current.mousePosition;
				Vector2 rectPosRange_Min = new Vector2(timelineVerticalScrollRect.x, timelineVerticalScrollRect.y);
				Vector2 rectPosRange_Max = new Vector2(timelineVerticalScrollRect.x + timelineVerticalScrollRect.width, timelineVerticalScrollRect.y + timelineVerticalScrollRect.height);
				if(mousePos.x >= rectPosRange_Min.x && mousePos.x <= rectPosRange_Max.x
					&& mousePos.y >= rectPosRange_Min.y && mousePos.y <= rectPosRange_Max.y)
				{
					//스크롤바를 움직일 것입니더
					//스크롤바를 움직이는 동안에는 모든 레이어가 출력되어야 한다.
					//Debug.LogError("Start Scrolling");
					_isScrollingTimelineY = true;
				}
			}
			if(_isScrollingTimelineY)
			{
				//스크롤을 중단시키자
				//다른 요소를 이용했거나 Up 이벤트 발생시
				if(Event.current.type == EventType.Used
					|| Event.current.rawType == EventType.Used
					|| Event.current.type == EventType.MouseUp
					|| Event.current.rawType == EventType.MouseUp)
				{
					//Debug.LogWarning("End Scrolling");
					_isScrollingTimelineY = false;
				}
			}

			//출력할 레이어 개수

			//삭제 19.11.22
			//int timelineLayers = Mathf.Max(10, Editor.TimelineInfoList.Count);
			//int heightForScrollLayer = (timelineLayers * heightPerLayer);

			//이벤트가 발생했다면 Repaint하자
			bool isEventOccurred = false;


			//GL에 크기값을 넣어주자
			apTimelineGL.SetLayoutSize(timelineWidth, recordAndSummaryHeight, timelineHeight,
											layoutX + leftTabWidth,
											layoutY, layoutY + recordAndSummaryHeight,
											windowWidth, windowHeight,
											isDrawMainTimeline, _scroll_Timeline);

			//GL에 마우스 값을 넣고 업데이트를 하자

			bool isLeftBtnPressed = false;
			bool isRightBtnPressed = false;

			if (Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag)
			{
				if (Event.current.button == 0) { isLeftBtnPressed = true; }
				else if (Event.current.button == 1) { isRightBtnPressed = true; }
			}

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			apTimelineGL.SetMouseValue(isLeftBtnPressed,
										isRightBtnPressed,
										//apMouse.PosNotBound,//이전
										Editor.Mouse.PosNotBound,//이후
										Event.current.shift, isCtrl, Event.current.alt,
										Event.current.rawType,
										this);


			//GUI의 배경 색상
			Color prevColor = GUI.backgroundColor;
			if (EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color(Editor._guiMainEditorColor.r * 0.8f,
										Editor._guiMainEditorColor.g * 0.8f,
										Editor._guiMainEditorColor.b * 0.8f,
										1.0f);
			}
			else
			{
				GUI.backgroundColor = Editor._guiMainEditorColor;
			}

			Rect timelineRect = new Rect(lastRect.x + leftTabWidth + 4, lastRect.y, timelineWidth, guiHeight + 15);
			GUI.Box(timelineRect, apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box);

			if (EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color(Editor._guiSubEditorColor.r * 0.8f,
										Editor._guiSubEditorColor.g * 0.8f,
										Editor._guiSubEditorColor.b * 0.8f,
										1.0f);
			}
			else
			{
				GUI.backgroundColor = Editor._guiSubEditorColor;
			}

			Rect timelineBottomRect = new Rect(lastRect.x + leftTabWidth + 4, lastRect.y + guiHeight + 15, timelineWidth, height - (guiHeight));
			GUI.Box(timelineBottomRect, apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box);

			GUI.backgroundColor = prevColor;

			//추가 : 하단 GUI도 넣어주자

			bool isWheelDrag = false;
			//마우스 휠 이벤트를 직접 주자
			if (Event.current.rawType == EventType.ScrollWheel)
			{
				//휠 드르륵..
				Vector2 mousePos = Event.current.mousePosition;

				if (mousePos.x > 0 && mousePos.x < lastRect.x + leftTabWidth + timelineWidth &&
					mousePos.y > lastRect.y + recordAndSummaryHeight && mousePos.y < lastRect.y + guiHeight)
				{
					if (isCtrl)
					{
						//추가 20.4.14 : Ctrl을 누른 상태로 휠을 돌리면 확대/축소가 된다.
						if(Event.current.delta.y > 0)
						{
							Editor._timelineZoom_Index = Mathf.Clamp(Editor._timelineZoom_Index - 1, timelineLayoutSize_Min, timelineLayoutSize_Max);
						}
						else if(Event.current.delta.y < 0)
						{
							Editor._timelineZoom_Index = Mathf.Clamp(Editor._timelineZoom_Index + 1, timelineLayoutSize_Min, timelineLayoutSize_Max);
						}
					}
					else
					{
						//Ctrl를 누르지 않고 휠을 돌리면 상하좌우로 스크롤된다.
						_scroll_Timeline += Event.current.delta * 7;

					}

					Event.current.Use();
					apTimelineGL.SetMouseUse();

					isEventOccurred = true;

					//클릭시 GUI 포커스 날림
					apEditorUtil.ReleaseGUIFocus();//추가 : 19.11.23
				}
			}

			//추가 20.12.4 : 단축키로 스크롤을 할 수 있다.
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimTimelineUIScroll, apHotKeyMapping.KEY_TYPE.Anim_TimelineScrollUp, 0);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimTimelineUIScroll, apHotKeyMapping.KEY_TYPE.Anim_TimelineScrollDown, 1);

			if (Event.current.isMouse && Event.current.type != EventType.Used)
			{
				//휠 클릭 후 드래그
				if (Event.current.button == 2)
				{
					
					if (Event.current.type == EventType.MouseDown)
					{
						Vector2 mousePos = Event.current.mousePosition;

						if (mousePos.x > leftTabWidth && mousePos.x < lastRect.x + leftTabWidth + timelineWidth &&
							mousePos.y > lastRect.y + recordAndSummaryHeight && mousePos.y < lastRect.y + guiHeight)
						{
							//휠클릭 드래그 시작
							_isTimelineWheelDrag = true;
							_prevTimelineWheelDragPos = mousePos;

							isWheelDrag = true;
							Event.current.Use();
							apTimelineGL.SetMouseUse();

							isEventOccurred = true;

							//클릭시 GUI 포커스 날림
							apEditorUtil.ReleaseGUIFocus();//추가 : 19.11.23

							//Debug.LogError("Mouse Input > Use");
						}
					}
					else if (Event.current.type == EventType.MouseDrag && _isTimelineWheelDrag)
					{
						//휠-드래그로 움직이는 중
						Vector2 mousePos = Event.current.mousePosition;
						Vector2 deltaPos = mousePos - _prevTimelineWheelDragPos;

						//_scroll_Timeline -= deltaPos * 1.0f;
						_scroll_Timeline.x -= deltaPos.x * 1.0f;//X만 움직이자

						_prevTimelineWheelDragPos = mousePos;
						isWheelDrag = true;
						Event.current.Use();
						apTimelineGL.SetMouseUse();

						isEventOccurred = true;

						//Debug.LogError("Mouse Input > Use");
					}
				}
			}

			if (!isWheelDrag && Event.current.isMouse)
			{
				_isTimelineWheelDrag = false;
			}

			// ┌──┬─────┬──┐
			// │ㅁㅁ│	  v      │ inf│
			// ├──┼─────┤    │
			// │~~~~│  ㅁ  ㅁ  │    │
			// │~~~~│    ㅁ    │    │
			// ├──┼─────┤    │
			// │ >  │Zoom      │    │
			// └──┴─────┴──┘

			//1-1 요약부 : [레코드] + [타임과 통합 키프레임]
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(mainTabWidth), apGUILOFactory.I.Height(recordAndSummaryHeight));

			int animEditBtnGroupHeight = 30;

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(leftTabWidth), apGUILOFactory.I.Height(recordAndSummaryHeight));
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(leftTabWidth), apGUILOFactory.I.Height(animEditBtnGroupHeight));
			GUILayout.Space(5);

			//Texture2D imgAutoKey = null;
			//if (IsAnimAutoKey)	{ imgAutoKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_KeyOn); }
			//else					{ imgAutoKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_KeyOff); }

			Texture2D imgKeyLock = null;
			if (IsAnimSelectionLock) { imgKeyLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionLock); }
			else { imgKeyLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_SelectionUnlock); }

			//삭제 21.2.13 : 모디파이어 잠금 사용하지 않음
			//Texture2D imgLayerLock = null;
			//if (ExAnimEditingMode == EX_EDIT.General_Edit) { imgLayerLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_ExModOption); }
			//else { imgLayerLock = Editor.ImageSet.Get(apImageSet.PRESET.Edit_ExModOption); }

			Texture2D imgAddKeyframe = Editor.ImageSet.Get(apImageSet.PRESET.Anim_AddKeyframe);

			// 요약부 + 왼쪽의 [레코드] 부분
			//1. Start / Stop Editing (Toggle)
			//2. Auto Key (Toggle)
			//3. Set Key
			//4. Lock (Toggle)로 이루어져 있다.


			Texture2D editIcon = null;
			string strButtonName = null;
			bool isEditable = false;


			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//현재 애니메이션 수정 작업중이라면..
				editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Recording);
				//strButtonName = " Editing";
				strButtonName = Editor.GetUIWord(UIWORD.EditingAnim);
				isEditable = true;
			}
			else
			{
				//현재 애니메이션 수정 작업을 하고 있지 않다면..
				if (IsAnimEditable)
				{
					editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_Record);
					//strButtonName = " Start Edit";
					strButtonName = Editor.GetUIWord(UIWORD.StartEdit);
					isEditable = true;
				}
				else
				{
					editIcon = Editor.ImageSet.Get(apImageSet.PRESET.Edit_NoRecord);
					//strButtonName = " No-Editable";
					strButtonName = Editor.GetUIWord(UIWORD.NoEditable);
				}
			}



			// Anim 편집 On/Off
			//Animation Editing On / Off
			if (apEditorUtil.ToggledButton_2Side_LeftAlign(	editIcon, 
															1, strButtonName, strButtonName, 
															ExAnimEditingMode != EX_EDIT.None, 
															isEditable, 
															//105, 
															130,
															animEditBtnGroupHeight, 
															apStringFactory.I.GetHotkeyTooltip_AnimationEditModeToggle(Editor.HotKeyMap)
															))//"Animation Edit Mode (A)"
			{
				//v1.4.2 : FFD 모드에서는 토글이 되지 않을 수 있다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					//AnimEditing을 On<->Off를 전환하고 기즈모 이벤트를 설정한다.
					SetAnimEditingToggle();

					//추가 : 19.11.23
					apEditorUtil.ReleaseGUIFocus();
				}
			}

			//2개의 Lock 버튼
			if (apEditorUtil.ToggledButton_2Side(	imgKeyLock, 
													IsAnimSelectionLock, 
													ExAnimEditingMode != EX_EDIT.None, 
													35, animEditBtnGroupHeight, 
													apStringFactory.I.GetHotkeyTooltip_SelectionLockToggle(Editor.HotKeyMap)
													))//"Selection Lock/Unlock (S)"
			{
				_isAnimSelectionLock = !_isAnimSelectionLock;

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);
			}


			//"Add Key"
			if (apEditorUtil.ToggledButton_2Side(	imgAddKeyframe, 
													Editor.GetUIWord(UIWORD.AddKey), Editor.GetUIWord(UIWORD.AddKey), 
													false, ExAnimEditingMode != EX_EDIT.None, 
													//85, 
													90,
													animEditBtnGroupHeight, 
													apStringFactory.I.GetHotkeyTooltip_AddKeyframe(Editor.HotKeyMap)
													))//"Add Keyframe"
			{	
				//v1.4.2 : FFD 모드에서는 키프레임 추가가 제한될 수 있다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					//변경 20.6.12 : 선택된 모든 타임라인 레이어에 대해서 키프레임을 생성한다.
					if (AnimTimelineLayer_Main != null)
					{
						if (AnimTimelineLayers_All.Count > 1)
						{
							//1. 여러개의 타임라인 레이어에 대해서 키프레임 추가
							List<apAnimKeyframe> addedKeyframes = Editor.Controller.AddAnimKeyframes(AnimClip.CurFrame, AnimClip, AnimTimelineLayers_All, true);
							if (addedKeyframes != null && addedKeyframes.Count == 0)
							{
								//프레임을 이동하자
								_animClip.SetFrame_Editor(AnimClip.CurFrame);
								SelectAnimMultipleKeyframes(addedKeyframes, apGizmos.SELECT_TYPE.New, true);
							}
						}
						else
						{
							//2. 키프레임 한개 추가
							apAnimKeyframe addedKeyframe = Editor.Controller.AddAnimKeyframe(AnimClip.CurFrame, AnimTimelineLayer_Main, true);
							if (addedKeyframe != null)
							{
								//프레임을 이동하자
								_animClip.SetFrame_Editor(addedKeyframe._frameIndex);
								SelectAnimKeyframe(addedKeyframe, true, apGizmos.SELECT_TYPE.New);
							}
						}

						//추가 : 자동 스크롤
						AutoSelectAnimTimelineLayer(true, false);
						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
				}
			}



			//단축키 [A]로 Editing 상태 토글
			//단축키 [S]에 의해서 Seletion Lock을 켜고 끌 수 있다.
			//단축키 [D]로 Layer Lock을 토글
			
			//변경 20.12.3
			Editor.AddHotKeyEvent(OnHotKey_AnimEditingToggle, apHotKeyMapping.KEY_TYPE.ToggleEditingMode, null);//"Toggle Editing Mode"
			Editor.AddHotKeyEvent(OnHotKey_AnimSelectionLockToggle, apHotKeyMapping.KEY_TYPE.ToggleSelectionLock, null);//"Toggle Selection Lock"
			Editor.AddHotKeyEvent(OnHotKey_AnimAddKeyframe, apHotKeyMapping.KEY_TYPE.Anim_AddKeyframes, null);//"Add New Keyframe"

			
			EditorGUILayout.EndHorizontal();

			//"Add Keyframes to All Layers"
			if (apEditorUtil.ToggledButton_2Side(Editor.GetUIWord(UIWORD.AddKeyframesToAllLayers), Editor.GetUIWord(UIWORD.AddKeyframesToAllLayers), false, ExAnimEditingMode != EX_EDIT.None, leftTabWidth - (10), 20))
			{
				//현재 프레임의 모든 레이어에 Keyframe을 추가한다.
				//이건 다이얼로그로 꼭 물어보자

				//v1.4.2 : FFD 모드에서는 키프레임 추가가 제한될 수 있다.
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.AddKeyframeToAllLayer_Title),
															Editor.GetText(TEXT.AddKeyframeToAllLayer_Body),
															Editor.GetText(TEXT.Okay),
															Editor.GetText(TEXT.Cancel));

					if (isResult)
					{
						Editor.Controller.AddAnimKeyframeToAllLayer(AnimClip.CurFrame, AnimClip, true);
					}
				}
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(timelineWidth), apGUILOFactory.I.Height(recordAndSummaryHeight));

			// 요약부 + 오른쪽의 [시간 / 통합 키 프레임]
			// 이건 GUI로 해야한다.
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();



			//1-2 메인 타임라인 : [레이어] + [타임라인 메인]
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(mainTabWidth), apGUILOFactory.I.Height(timelineHeight));

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(leftTabWidth), apGUILOFactory.I.Height(timelineHeight));
			GUILayout.BeginArea(new Rect(lastRect.x, lastRect.y + recordAndSummaryHeight, leftTabWidth, timelineHeight));
			// 메인 + 왼쪽의 [레이어] 부분

			// 레이어에 대한 렌더링 (정보 부분)
			//--------------------------------------------------------------
			int nTimelines = AnimClip._timelines.Count;
			//apAnimTimeline curTimeline = null;


			GUIStyle curGuiStyle_layerInfoBox = null;//최적화 코드
			bool isLeftPaddingAdded = false;
			int textColorType = 0;//Black / White / Gray

			int btnWidth_Layer = leftTabWidth + 4;
			Texture2D img_TimelineFolded = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight);
			Texture2D img_TimelineNotFolded = Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			Texture2D img_CurFold = null;

			if (_guiContent_Bottom_Animation_TimelineLayerInfo == null)
			{
				_guiContent_Bottom_Animation_TimelineLayerInfo = new apGUIContentWrapper();
			}


			int curLayerY = 0;
			int totalTimelineInfoHeight = 0;
			

			//1. 타임라인/타임라인 레이어의 왼쪽의 이름 리스트를 출력하자.
			//추가로, 현재 렌더링되는 LayerInfo의 전체 Height를 계산하자
			for (int iLayer = 0; iLayer < nTimelineInfoList; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];

				//일단 렌더링 여부를 초기화한다.
				//Layer Info는 GUI에서 그리도록 하고, 나중에 TimelineGL에서 렌더링을 할지 결정한다.
				info._isRenderable = false;

				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					//숨겨진 레이어이다.
					info._guiLayerPosY = 0.0f;
					continue;
				}
				int layerHeight = heightPerLayer;
				isLeftPaddingAdded = true;
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
					isLeftPaddingAdded = false;
				}

				//배경 / 텍스트 색상을 정하자
				Color layerBGColor = info.GUIColor;
				textColorType = 0;//<<0 : Black, 1 : White, 2 : Gray

				info._guiLayerPosY = curLayerY;

				if(_isScrollingTimelineY)
				{
					//우측 세로 스크롤중에는 다 보여야 한다. (유니티 이벤트때문에) (20.4.14)
					info._isRenderable = true;
				}
				else
				{
					if (info._guiLayerPosY < _scroll_Timeline.y - layerHeight
						|| info._guiLayerPosY > _scroll_Timeline.y + timelineHeight)
					{
						//렌더링 영역 바깥에 있다.
						//TimelineGL에서는 출력하지 않도록 한다. 에디터가 빨라지겠져
						info._isRenderable = false;
					}
					else
					{
						info._isRenderable = true;
					}
				}


				//변경 19.11.22 : 여기서부터는 "렌더링되는 Info"만 렌더링하자
				if (info._isRenderable)
				{
					if (!info._isAvailable)
					{
						textColorType = 2;
					}
					else
					{
						float grayScale = (layerBGColor.r + layerBGColor.g + layerBGColor.b) / 3.0f;
						if (grayScale < 0.3f)
						{
							textColorType = 1;
						}
					}

					//아이콘을 결정하자
					switch (textColorType)
					{
						case 0://Black
							curGuiStyle_layerInfoBox = (isLeftPaddingAdded ? apGUIStyleWrapper.I.Label_MiddleLeft_BtnPadding_Left20_BlackColor : apGUIStyleWrapper.I.Label_MiddleLeft_BtnPadding_BlackColor);
							break;

						case 1://White
							curGuiStyle_layerInfoBox = (isLeftPaddingAdded ? apGUIStyleWrapper.I.Label_MiddleLeft_BtnPadding_Left20_WhiteColor : apGUIStyleWrapper.I.Label_MiddleLeft_BtnPadding_WhiteColor);
							break;

						case 2://Gray
						default:
							curGuiStyle_layerInfoBox = (isLeftPaddingAdded ? apGUIStyleWrapper.I.Label_MiddleLeft_BtnPadding_Left20_GrayColor : apGUIStyleWrapper.I.Label_MiddleLeft_BtnPadding_GrayColor);
							break;
					}

					_guiContent_Bottom_Animation_TimelineLayerInfo.SetText(2, info.DisplayName);
					_guiContent_Bottom_Animation_TimelineLayerInfo.SetImage(Editor.ImageSet.Get(info.IconImgType));



					// [ 레이어 선택 ]

					if (info._isTimeline)
					{
						GUI.backgroundColor = layerBGColor;
						GUI.Box(new Rect(0, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight), apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box);

						int yOffset = (layerHeight - 18) / 2;

						if (info.IsTimelineFolded)
						{
							img_CurFold = img_TimelineFolded;
						}
						else
						{
							img_CurFold = img_TimelineNotFolded;
						}
						if (GUI.Button(new Rect(2, (curLayerY + yOffset) - _scroll_Timeline.y, 18, 18), img_CurFold, apGUIStyleWrapper.I.Button_Margin0))
						{
							if (info._timeline != null)
							{
								info._timeline._guiTimelineFolded = !info._timeline._guiTimelineFolded;
							}
						}

						GUI.backgroundColor = prevColor;

						if (GUI.Button(new Rect(19, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight),
										//new GUIContent("  " + info.DisplayName, layerIcon), //이전
										_guiContent_Bottom_Animation_TimelineLayerInfo.Content,//변경
										curGuiStyle_layerInfoBox))
						{
							nextSelectLayerInfo = info;//<<선택!
						}
					}
					else
					{
						//[ Hide 버튼]
						//int xOffset = (btnWidth_Layer - (layerHeight + 4)) + 2;
						//int xOffset = 18;
						int yOffset = (layerHeight - 18) / 2;

						GUI.backgroundColor = layerBGColor;
						GUI.Box(new Rect(0, curLayerY - _scroll_Timeline.y, btnWidth_Layer, layerHeight), apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box);

						if (GUI.Button(new Rect(2, (curLayerY + yOffset) - _scroll_Timeline.y, 18, 18), apStringFactory.I.Minus, apGUIStyleWrapper.I.Button_Margin0))
						{
							//Hide
							info._layer._guiLayerVisible = false;//<<숨기자!
						}

						GUI.backgroundColor = prevColor;


						//2 + 18 + 2 = 22
						if (GUI.Button(new Rect(19, curLayerY - _scroll_Timeline.y, btnWidth_Layer - 22, layerHeight),
										//new GUIContent("  " + info.DisplayName, layerIcon), //이전
										_guiContent_Bottom_Animation_TimelineLayerInfo.Content,//변경
										curGuiStyle_layerInfoBox))
						{
							nextSelectLayerInfo = info;//<<선택!
						}
					}
				}

				curLayerY += layerHeight;
				totalTimelineInfoHeight += layerHeight;//전체 높이도 계산하자
			}

			//너무 작다면 크게 바꾸어야 한다
			totalTimelineInfoHeight = Mathf.Max(totalTimelineInfoHeight, heightPerLayer * 10);//레이어 10개분 정도는 기본적으로 나와야 한다.
			totalTimelineInfoHeight += 50;
			
			//--------------------------------------------------------------

			GUILayout.EndArea();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(timelineWidth), apGUILOFactory.I.Height(timelineHeight));
			GUILayout.BeginArea(timelineRect);
			// 메인 + 오른쪽의 [메인 타임라인]
			// 이건 GUI로 해야한다.

			//기본 타임라인 GL 세팅
			apTimelineGL.SetTimelineSetting(0, AnimClip.StartFrame, AnimClip.EndFrame, Editor.WidthPerFrameInTimeline, AnimClip.IsLoop);
			
			// 레이어에 대한 렌더링 (타임라인 부분 - BG)
			//--------------------------------------------------------------


			// < 타임라인 GUI - 배경 >
			curLayerY = 0;
			for (int iLayer = 0; iLayer < nTimelineInfoList; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];
				int layerHeight = heightPerLayer;

				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					continue;
				}
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
				}
				if (info._isSelected)
				{
					apTimelineGL.DrawTimeBars_MainBG(info.TimelineColor, curLayerY + layerHeight - (int)_scroll_Timeline.y, layerHeight);
				}
				curLayerY += layerHeight;
			}

			//Grid를 그린다.
			apTimelineGL.DrawTimelineAreaBG(ExAnimEditingMode != EX_EDIT.None);
			apTimelineGL.DrawTimeGrid(new Color(0.4f, 0.4f, 0.4f, 1.0f), new Color(0.3f, 0.3f, 0.3f, 1.0f), new Color(0.7f, 0.7f, 0.7f, 1.0f));
			apTimelineGL.DrawTimeBars_Header(new Color(0.4f, 0.4f, 0.4f, 1.0f));



			// 레이어에 대한 렌더링 (타임라인 부분 - Line + Frames)
			//--------------------------------------------------------------

			// < 타임라인 GUI - 메인>

			//추가 : 커브 편집에 관한 데이터
			bool isSelectedAnimTimeline = false;
			bool isSingleCurveEdit = false;
			bool isCurveEdit = false;
			int iMultiCurveType = 0;
			if (IsAnimKeyframeMultipleSelected)
			{
				//키프레임 여러개 편집중인 경우
				isSingleCurveEdit = false;
				isCurveEdit = true;
				switch (_animPropertyCurveUI_Multi)
				{
					case ANIM_MULTI_PROPERTY_CURVE_UI.Prev:		iMultiCurveType = 0; break;
					case ANIM_MULTI_PROPERTY_CURVE_UI.Middle:	iMultiCurveType = 1; break;
					case ANIM_MULTI_PROPERTY_CURVE_UI.Next:		iMultiCurveType = 2; break;
				}
			}
			else
			{
				//1개의 커브를 편집 중(이거나 없거나)
				isSingleCurveEdit = true;
				isCurveEdit = (_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve);
			}

			curLayerY = 0;
			bool isAnyHidedLayer = false;


			apTimelineGL.BeginKeyframeControl();

			//추가 21.3.9 : 보이지 않는 외부의 키프레임들을 드래그를 통해서 선택 중인가
			bool isAreaMultiSelectingHiddenKeyframes = apTimelineGL.IsSelectingHiddenMultiKeyframesByArea();

			for (int iLayer = 0; iLayer < nTimelineInfoList; iLayer++)
			{
				apTimelineLayerInfo info = timelineInfoList[iLayer];
				if (!info._isTimeline && !info.IsVisibleLayer)
				{
					//숨겨진 레이어
					isAnyHidedLayer = true;
					continue;
				}
				int layerHeight = heightPerLayer;
				if (info._isTimeline)
				{
					layerHeight = heightPerTimeline;
				}

				if (info._isRenderable)
				{
					apTimelineGL.DrawTimeBars_MainLine(new Color(0.3f, 0.3f, 0.3f, 1.0f), curLayerY + layerHeight - (int)_scroll_Timeline.y);

					if (!info._isTimeline)
					{
						Color curveEditColor = Color.black;

						//커브를 여러개 편집 중인지 아닌지 확인
						if (isSingleCurveEdit)
						{
							//단일 키프레임 편집 중일때
							//isSelectedAnimTimeline = (AnimTimelineLayer == info._layer);//이전

							//변경 20.6.12 : 타임라인 레이어"들"에 속하는지 확인
							if(AnimTimelineLayers_All != null)
							{
								if(AnimTimelineLayers_All.Count == 1 && AnimTimelineLayer_Main != null)
								{
									isSelectedAnimTimeline = (AnimTimelineLayer_Main == info._layer);
								}
								else
								{
									isSelectedAnimTimeline = AnimTimelineLayers_All.Contains(info._layer);
								}
							}
							else
							{
								isSelectedAnimTimeline = false;
							}

						}
						else
						{
							//여러개의 키프레임을 편집 중일때
							isSelectedAnimTimeline = _animTimelineCommonCurve.IsSelectedTimelineLayer(info._layer);
						}

						apTimelineGL.DrawKeyframes(	info._layer,
													curLayerY + layerHeight / 2,
													info.GUIColor,
													info._isAvailable,
													layerHeight,
													AnimClip.CurFrame,
													isSelectedAnimTimeline,
													isCurveEdit,
													isSingleCurveEdit,
													_animPropertyCurveUI,
													iMultiCurveType,
													_animTimelineCommonCurve
													//curveEditColor
													);
					}
				}
				else if(isAreaMultiSelectingHiddenKeyframes)
				{
					if (!info._isTimeline)
					{
						//렌더링은 아닌데, 선택은 체크해봐야한다.
						apTimelineGL.CheckKeyframesAreaSelectedOnly(info._layer,
																		curLayerY + layerHeight / 2,
																		info._isAvailable,
																		layerHeight);
					}
				}

				curLayerY += layerHeight;
			}



			// Play Bar / Event Marker / Summary(Common Keyframe) 를 그린다.
			
			apTimelineGL.DrawKeySummry(_subAnimCommonKeyframeList, 58);

			if (Editor.Onion.IsVisible)
			{
				if (Editor._onionOption_IsRenderAnimFrames)
				{
					//영역 Onion인 경우
					int animLength = (AnimClip.EndFrame - AnimClip.StartFrame) + 1;
					int renderPerFrame = Mathf.Max(Editor._onionOption_RenderPerFrame, 0);
					if (animLength >= 1 && renderPerFrame > 0)
					{
						int prevRange = Mathf.Clamp(Editor._onionOption_PrevRange, 0, animLength / 2);
						int nextRange = Mathf.Clamp(Editor._onionOption_NextRange, 0, animLength / 2);

						prevRange = (prevRange / renderPerFrame) * renderPerFrame;
						nextRange = (nextRange / renderPerFrame) * renderPerFrame;

						int minFrame = AnimClip.CurFrame - prevRange;
						int maxFrame = AnimClip.CurFrame + nextRange;

						if (AnimClip.IsLoop)
						{
							if (minFrame < AnimClip.StartFrame) { minFrame = (minFrame + animLength) - 1; }
							if (maxFrame > AnimClip.EndFrame) { maxFrame = (maxFrame - animLength) + 1; }
						}
						minFrame = Mathf.Clamp(minFrame, AnimClip.StartFrame, AnimClip.EndFrame);
						maxFrame = Mathf.Clamp(maxFrame, AnimClip.StartFrame, AnimClip.EndFrame);

						if (prevRange > 0)
						{
							apTimelineGL.DrawOnionMarkers(minFrame,
													new Color(Mathf.Clamp01(Editor._colorOption_OnionAnimPrevColor.r * 2),
																Mathf.Clamp01(Editor._colorOption_OnionAnimPrevColor.g * 2),
																Mathf.Clamp01(Editor._colorOption_OnionAnimPrevColor.b * 2),
																1.0f),
													44, 1);
						}
						if (nextRange > 0)
						{
							apTimelineGL.DrawOnionMarkers(maxFrame,
													new Color(Mathf.Clamp01(Editor._colorOption_OnionAnimNextColor.r * 2),
																Mathf.Clamp01(Editor._colorOption_OnionAnimNextColor.g * 2),
																Mathf.Clamp01(Editor._colorOption_OnionAnimNextColor.b * 2),
																1.0f),
													44, 2);
						}
					}
				}
				else if (Editor.Onion.IsRecorded)
				{
					apTimelineGL.DrawOnionMarkers(Editor.Onion.RecordAnimFrame,
													new Color(Mathf.Clamp01(Editor._colorOption_OnionToneColor.r * 2),
																Mathf.Clamp01(Editor._colorOption_OnionToneColor.g * 2),
																Mathf.Clamp01(Editor._colorOption_OnionToneColor.b * 2),
																1.0f),
													44, 0);
				}


			}


			bool isChangeFrame = apTimelineGL.DrawPlayBar(AnimClip.CurFrame);

			//변경 22.7.2 : PlayBar 앞에서 그려지도록 변경되었다.
			apTimelineGL.DrawEventMarkers(_animClip._animEvents, 30);

			if (isChangeFrame)
			{
				bool isWorkKeyframeChanged = false;
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				//v1.4.2 : 키프레임 변화에 의해 WorkKeyframe들이 바뀌었다면 DrawPlayBar 안에서 FFD 관련 처리를 했을 것이다.
				//만약 FFD가 아직도 켜져있다면, Revert를 하자.
				if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
				{
					Editor.Gizmos.RevertFFDTransformForce();
				}

				apEditorUtil.ReleaseGUIFocus();//추가 : 19.11.23
			}


			// < 타임라인 GUI - 렌더링 후 입력 처리 >
			bool isKeyframeEvent = apTimelineGL.EndKeyframeControl();//<<제어용 함수

			if (isKeyframeEvent) { isEventOccurred = true; }



			apTimelineGL.DrawAndUpdateSelectArea();

			
			//키프레임+타임 슬라이더를 화면 끝으로 이동한 경우 자동 스크롤
			if (apTimelineGL.IsKeyframeDragging)
			{
				float rightBound = timelineRect.xMin + (timelineWidth - rightTabWidth) + 30;
				float leftBound = 30;
				//if(apMouse.Pos.x > rightBound || apMouse.Pos.x < leftBound)//이전
				if (Editor.Mouse.Pos.x > rightBound || Editor.Mouse.Pos.x < leftBound)//이후
				{
					_animKeyframeAutoScrollTimer += apTimer.I.DeltaTime_Repaint;
					if (_animKeyframeAutoScrollTimer > 0.1f)
					{
						_animKeyframeAutoScrollTimer = 0.0f;
						AutoAnimScrollWithoutFrameMoving(apTimelineGL.FrameOnMouseX, 1);
						apTimelineGL.RefreshScrollDown();
					}
				}
			}
			else if(apTimelineGL.IsAreaSelecting)
			{
				//추가 21.3.9 : 마우스로 드래그하여 선택 중일 때에도 자동으로 화면 스크롤이 되도록
				
				float rightBound = timelineRect.xMin + (timelineWidth - rightTabWidth) + 30;
				float leftBound = 30;
				float topY = layoutY + recordAndSummaryHeight;
				//float topY = layoutY;
				float upBound = topY - 40;
				float downBound = topY + (timelineHeight - bottomControlHeight) + 25;

				//Debug.Log("마우스 : " + Editor.Mouse.Pos.y + "( Top Y : " + topY + " ) ");
				//Debug.Log("Height : " + timelineHeight);

				if (Editor.Mouse.Pos.x > rightBound 
					|| Editor.Mouse.Pos.x < leftBound
					|| Editor.Mouse.Pos.y < upBound
					|| Editor.Mouse.Pos.y > downBound)
				{
					_animKeyframeAutoScrollTimer += apTimer.I.DeltaTime_Repaint;
					if (_animKeyframeAutoScrollTimer > 0.1f)
					{
						_animKeyframeAutoScrollTimer = 0.0f;

						//거리에 따라 스크롤 속도가 다르다.
						Vector2 moveOffset = Vector2.zero;
						if(Editor.Mouse.Pos.x > rightBound)
						{	
							if(Editor.Mouse.Pos.x > rightBound + 50)	{ moveOffset.x += 20.0f; }
							else										{ moveOffset.x += 10.0f; }
						}
						else if(Editor.Mouse.Pos.x < leftBound)
						{
							if(Editor.Mouse.Pos.x < leftBound - 50)	{ moveOffset.x -= 20.0f; }
							else									{ moveOffset.x -= 10.0f; }
							
						}

						if(Editor.Mouse.Pos.y < upBound)
						{
							if(Editor.Mouse.Pos.y < upBound - 50)	{ moveOffset.y -= 20.0f; }
							else									{ moveOffset.y -= 10.0f; }
						}
						else if(Editor.Mouse.Pos.y > downBound)
						{
							if(Editor.Mouse.Pos.y > downBound + 50)	{ moveOffset.y += 20.0f; }
							else									{ moveOffset.y += 10.0f; }
						}

						Vector2 prevScroll = _scroll_Timeline;

						//강제로 스크롤을 하되, 최대 영역을 생각하자
						_scroll_Timeline.x = Mathf.Clamp(_scroll_Timeline.x + moveOffset.x, 0.0f, widthForScrollFrame - ANIM_SCROLL_BAR_BTN_SIZE_X);
						_scroll_Timeline.y = Mathf.Clamp(_scroll_Timeline.y + moveOffset.y, 0.0f, totalTimelineInfoHeight - ANIM_SCROLL_BAR_BTN_SIZE_Y);

						//실제 스크롤 이동 거리를 다시 계산
						moveOffset = _scroll_Timeline - prevScroll;

						apTimelineGL.SetMoveMouseDownPos(-moveOffset);//드래그와 반대방향으로 Down 위치를 옮긴다.
						apTimelineGL.RefreshScrollDown();
						Editor.SetRepaint();
					}
				}
			}


			//Timeline GL 렌더링 끝 (21.5.19
			apTimelineGL.EndPass();

			//--------------------------------------------------------------

			GUILayout.EndArea();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			//스크롤은 현재 키프레임의 범위, 레이어의 개수에 따라 바뀐다.
			

			//float prevScrollTimelineY = _scroll_Timeline.y;
			
			//if(Event.current.type == EventType.Layout)
			//{
			//	//_scroll_Timeline_DummyY = _scroll_Timeline.y;
			//}

			//추가 20.4.14 : 세로 스크롤을 하면 타임라인에서 보여지는 레이어가 바뀌는데(최적화때문에)
			//이때 GUI 호출 순서가 바뀌면서 이 스크롤바가 연속으로 인식되지 않아서 스크롤 도중에 입력이 풀리는 문제가 있다.
			//세로 스크롤바의 영역을 클릭했다면, 마우스가 Up되기 전에는 모든 리스트가 보여져야 한다.
			//단, 이 체크는 위에서 해야한다.

			
			_scroll_Timeline.y = GUI.VerticalScrollbar(
													//new Rect(	lastRect.x + leftTabWidth + 4 + timelineWidth - 15,
													//				lastRect.y,
													//				15,
													//				timelineHeight + recordAndSummaryHeight + 4),
													timelineVerticalScrollRect,
														_scroll_Timeline.y,
														ANIM_SCROLL_BAR_BTN_SIZE_Y,
														0.0f,
														//heightForScrollLayer//이전
														totalTimelineInfoHeight//변경 19.11.22
														);



			//Anim 레이어를 선택하자
			
			//[v1.4.2] FFD 모드에서 레이어를 변경하고자 했다면,
			//FFD가 종료되어야 한다.
			if (nextSelectLayerInfo != null)
			{
				bool isExecutable = Editor.CheckModalAndExecutable();
				if(!isExecutable)
				{
					//FFD를 계속 하고자 한다면 레이어 선택은 취소
					nextSelectLayerInfo = null;
				}
			}

			if (nextSelectLayerInfo != null)
			{
				_isIgnoreAnimTimelineGUI = true;//<깜빡이지 않게..
				if (nextSelectLayerInfo._isTimeline)
				{
					//Timeline을 선택하기 전에
					//Anim객체를 초기화한다. (안그러면 자동으로 선택된 오브젝트에 의해서 TimelineLayer를 선택하게 된다.)
					SelectBone_ForAnimEdit(null, false, false, MULTI_SELECT.Main);
					SelectControlParam_ForAnimEdit(null, false, false);
					SelectMeshTF_ForAnimEdit(null, false, false, MULTI_SELECT.Main);
					SelectMeshGroupTF_ForAnimEdit(null, false, false, MULTI_SELECT.Main);

					SelectAnimTimeline(nextSelectLayerInfo._timeline, true, true);
					SelectAnimTimelineLayer(null, MULTI_SELECT.Main, true, true, true);
					SelectAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);

					AutoSelectAnimTimelineLayer(false);

					//최근 클릭한 레이어 초기화
					_lastClickTimelineLayer = null;
				}
				else
				{	
					SelectAnimTimeline(nextSelectLayerInfo._parentTimeline, true, true);

					//먼저 Shift 클릭을 체크한다.
					//- 이전에 클릭한 타임라인 레이어가 있는 경우
					//- 같은 Parent Timeline을 공유할 때
					//- 지금 클릭한 것과 다른 경우
					//>>>> 위 조건을 만족하면 여러개를 모두 선택한다. ("범위내 모두 활성" or "범위내 모두 비활성"으로만)
					//>>>> 그렇지 않으면 Shift 클릭은 Ctrl 클릭과 동일하다.

					bool isShiftMultipleSelect = false;
					if(Event.current.shift 
						&& _lastClickTimelineLayer != null 
						&& nextSelectLayerInfo._layer != _lastClickTimelineLayer
						&& nextSelectLayerInfo._layer != null)
					{
						apTimelineLayerInfo lastClickInfo = null;
						int iLast = -1;
						int iNext = -1;
						if(_lastClickTimelineLayer._parentTimeline != null
							&& _lastClickTimelineLayer._parentTimeline == AnimTimeline)
						{
							//이전에 선택한 TimelineInfo를 찾자
							lastClickInfo = timelineInfoList.Find(delegate(apTimelineLayerInfo a)
							{
								return a._layer == _lastClickTimelineLayer;
							});
						}

						if(lastClickInfo != null)
						{
							//lastClickInfo 부터 nextSelectLayerInfo 까지 찾자
							iLast = timelineInfoList.IndexOf(lastClickInfo);
							iNext = timelineInfoList.IndexOf(nextSelectLayerInfo);
							
						}

						if(iLast >= 0 && iNext >= 0 && iLast != iNext)
						{
							//이제 전체 선택하자
							bool isDeselected2Selected = false;
							if(AnimTimelineLayers_All.Contains(nextSelectLayerInfo._layer))
							{
								//다음에 클릭한 타임라인 레이어가 선택된 상태라면
								//"범위 내 모두 비활성"
								isDeselected2Selected = false;
							}
							else
							{
								//다음에 클릭한 타임라인 레이어가 선택되지 않은 상태라면
								//"범위 내 모두 활성"
								isDeselected2Selected = true;
							}

							//반복문을 통해서 처리하자
							//단, Hide된 레이어는 생략
							int iStart = Mathf.Min(iLast, iNext);
							int iEnd = Mathf.Max(iLast, iNext);

							apTimelineLayerInfo curInfo = null;
							List<apAnimTimelineLayer> targetLayers = new List<apAnimTimelineLayer>();
							for (int i = iStart; i <= iEnd; i++)
							{
								curInfo = timelineInfoList[i];
								if(!curInfo.IsVisibleLayer || curInfo._layer == null)
								{
									continue;
								}

								targetLayers.Add(curInfo._layer);
							}

							SelectAnimTimelineLayersAddable(targetLayers, isDeselected2Selected, true);

							isShiftMultipleSelect = true;//처리 완료
						}

					}

					//Shift에 의한 다중 선택이 아니면
					//일반적으로 처리하자
					if (!isShiftMultipleSelect)
					{
						if (isCtrl || Event.current.shift)
						{
							//다중 선택 20.6.20
							SelectAnimTimelineLayer(nextSelectLayerInfo._layer, MULTI_SELECT.AddOrSubtract, true, true, true);
						}
						else
						{
							//단일 선택
							SelectAnimTimelineLayer(nextSelectLayerInfo._layer, MULTI_SELECT.Main, true, true, true);
						}
					}

					//[1.4.2] 타임라인 레이어에 따라 선택한 오브젝트로 오른쪽 UI 스크롤을 옮긴다.
					if(Editor._option_AutoScrollWhenObjectSelected)
					{
						object selectedObj = null;

						//스크롤 가능한 상황인지 체크하고 (타임라인 레이어용)
						if(Editor.IsAutoScrollableWhenClickObject_AnimationTimelinelayer(nextSelectLayerInfo._layer, true, out selectedObj))
						{
							//자동 스크롤을 요청한다.
							Editor.AutoScroll_HierarchyAnimation(selectedObj);
						}
					}

					
					
					SelectAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);

					//클릭했던건 기억을 하자
					if(nextSelectLayerInfo._layer != null)
					{
						_lastClickTimelineLayer = nextSelectLayerInfo._layer;
					}
				}


				bool isWorkKeyframeChanged = false;
				AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

				
				Editor.RefreshControllerAndHierarchy(false);
				//List<apTimelineLayerInfo> timelineInfoList = Editor.TimelineInfoList;

				
			}

			//float prevScrollTimelineX = _scroll_Timeline.x;
			
			_scroll_Timeline.x = GUI.HorizontalScrollbar(new Rect(lastRect.x + leftTabWidth + 4, lastRect.y + recordAndSummaryHeight + timelineHeight + 4, timelineWidth - 15, 15),
															_scroll_Timeline.x,
															ANIM_SCROLL_BAR_BTN_SIZE_X, 
															0.0f, widthForScrollFrame);

			//이것도 왜 있는거지??
			//if (Mathf.Abs(prevScrollTimelineX - _scroll_Timeline.x) > 0.5f)
			//{
			//	//Debug.Log("Scroll X");
			//	Event.current.Use();
			//	apTimelineGL.SetMouseUse();
			//}

			if (GUI.Button(new Rect(lastRect.x + leftTabWidth + 4 + timelineWidth - 15, lastRect.y + recordAndSummaryHeight + timelineHeight + 4, 15, 15), apStringFactory.I.None))
			{
				_scroll_Timeline.x = 0;
				_scroll_Timeline.y = 0;
			}

			//1-3 하단 컨트롤과 스크롤 : [컨트롤러] + [스크롤 + 애니메이션 설정]
			int ctrlBtnSize_Small = 30;
			int ctrlBtnSize_Large = 30;
			int ctrlBtnSize_LargeUnder = bottomControlHeight - (ctrlBtnSize_Large + 6);

			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(mainTabWidth), apGUILOFactory.I.Height(bottomControlHeight));
			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(leftTabWidth), apGUILOFactory.I.Height(bottomControlHeight));
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(leftTabWidth), apGUILOFactory.I.Height(ctrlBtnSize_Large + 2));
			GUILayout.Space(5);


			//플레이 제어 단축키
			//단축키 [<, >]로 키프레임을 이동할 수 있다.
			// Space : 재생/정지
			// <, > : 1프레임 이동
			// Shift + <, > : 첫프레임, 끝프레임으로 이동
			
			//변경 20.12.3
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, apHotKeyMapping.KEY_TYPE.Anim_PlayPause, 0);//"Play/Pause"
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, apHotKeyMapping.KEY_TYPE.Anim_MovePrevFrame, 1);//"Previous Frame"
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, apHotKeyMapping.KEY_TYPE.Anim_MoveNextFrame, 2);//"Next Frame"
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, apHotKeyMapping.KEY_TYPE.Anim_MoveFirstFrame, 3);//"First Frame"
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, apHotKeyMapping.KEY_TYPE.Anim_MoveLastFrame, 4);//"Last Frame"

			//추가된 단축키 20.12.4 : 키프레임간 이동
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, apHotKeyMapping.KEY_TYPE.Anim_MovePrevKeyframe, 5);//"Previous Key-Frame"
			Editor.AddHotKeyEvent(OnHotKey_AnimMoveFrame, apHotKeyMapping.KEY_TYPE.Anim_MoveNextKeyframe, 6);//"Next Key-Frame"

			//추가 3.29 : 키프레임 선택해서 복사, 붙여넣기
			if (_subAnimKeyframeList != null && _subAnimKeyframeList.Count > 0)
			{
				//타임라인에서 선택된 키프레임들이 있을 때 복사하기
				//변경 20.12.3
				Editor.AddHotKeyEvent(OnHotKey_AnimCopyKeyframes, apHotKeyMapping.KEY_TYPE.Anim_CopyKeyframes, null);//"Copy Keyframes"
			}

			//현재 AnimClip에 키프레임들을 Ctrl+V로 복사할 수 있다면..
			Editor.AddHotKeyEvent(OnHotKey_AnimPasteKeyframes, apHotKeyMapping.KEY_TYPE.Anim_PasteKeyframes, null);//"Paste Keyframes"

			if (_guiContent_Bottom_Animation_FirstFrame == null) { _guiContent_Bottom_Animation_FirstFrame = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_FirstFrame), "Move to First Frame (Shift + <)"); }
			if (_guiContent_Bottom_Animation_PrevFrame == null) { _guiContent_Bottom_Animation_PrevFrame = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_PrevFrame), "Move to Previous Frame (<)"); }
			if (_guiContent_Bottom_Animation_Play == null) { _guiContent_Bottom_Animation_Play = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Play), "Play (Space Bar)"); }
			if (_guiContent_Bottom_Animation_Pause == null) { _guiContent_Bottom_Animation_Pause = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Pause), "Pause (Space Bar)"); }
			if (_guiContent_Bottom_Animation_NextFrame == null) { _guiContent_Bottom_Animation_NextFrame = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_NextFrame), "Move to Next Frame (>)"); }
			if (_guiContent_Bottom_Animation_LastFrame == null) { _guiContent_Bottom_Animation_LastFrame = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Anim_LastFrame), "Move to Last Frame (Shift + >)"); }


			//플레이 제어
			//if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_FirstFrame), "Move to First Frame (Shift + <)"), GUILayout.Width(ctrlBtnSize_Large), GUILayout.Height(ctrlBtnSize_Large)))
			if (GUILayout.Button(_guiContent_Bottom_Animation_FirstFrame.Content, apGUILOFactory.I.Width(ctrlBtnSize_Large), apGUILOFactory.I.Height(ctrlBtnSize_Large)))
			{
				//제어 : 첫 프레임으로 이동

				//[v1.4.2] FFD 체크
				bool isExecutable = Editor.CheckModalAndExecutable();
				
				if (isExecutable)
				{
					//첫프레임으로 이동한다.
					AnimClip.SetFrame_Editor(AnimClip.StartFrame);

					bool isWorkKeyframeChanged = false;
					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}
				
			}

			//if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_PrevFrame), "Move to Previous Frame (<)"), GUILayout.Width(ctrlBtnSize_Large + 10), GUILayout.Height(ctrlBtnSize_Large)))
			if (GUILayout.Button(_guiContent_Bottom_Animation_PrevFrame.Content, apGUILOFactory.I.Width(ctrlBtnSize_Large + 10), apGUILOFactory.I.Height(ctrlBtnSize_Large)))
			{
				//제어 : 이전 프레임으로 이동

				//[v1.4.2] FFD 체크
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					int prevFrame = AnimClip.CurFrame - 1;
					if (prevFrame < AnimClip.StartFrame)
					{
						if (AnimClip.IsLoop)
						{
							prevFrame = AnimClip.EndFrame;
						}
					}
					AnimClip.SetFrame_Editor(prevFrame);

					bool isWorkKeyframeChanged = false;
					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}
			}

			apGUIContentWrapper curPlayPauseGUIContent = AnimClip.IsPlaying_Editor ? _guiContent_Bottom_Animation_Pause : _guiContent_Bottom_Animation_Play;

			if (GUILayout.Button(curPlayPauseGUIContent.Content, apGUILOFactory.I.Width(ctrlBtnSize_Large + 30), apGUILOFactory.I.Height(ctrlBtnSize_Large)))
			{
				//제어 : 플레이 / 일시정지

				//[v1.4.2] FFD 체크
				bool isExecutable = Editor.CheckModalAndExecutable();
				
				if (isExecutable)
				{
					if (AnimClip.IsPlaying_Editor)
					{
						// 플레이 -> 일시 정지
						AnimClip.Pause_Editor();
					}
					else
					{
						//마지막 프레임이라면 첫 프레임으로 이동하여 재생한다.
						if (AnimClip.CurFrame == AnimClip.EndFrame)
						{
							AnimClip.SetFrame_Editor(AnimClip.StartFrame);
							Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
						}
						// 일시 정지 -> 플레이
						AnimClip.Play_Editor();
					}

					//Play 전환 여부에 따라서도 WorkKeyframe을 전환한다.
					bool isWorkKeyframeChanged = false;
					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
					Editor.SetRepaint();
					Editor.Gizmos.SetUpdate();
				}

			}

			//if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_NextFrame), "Move to Next Frame (>)"), GUILayout.Width(ctrlBtnSize_Large + 10), GUILayout.Height(ctrlBtnSize_Large)))
			if (GUILayout.Button(_guiContent_Bottom_Animation_NextFrame.Content, apGUILOFactory.I.Width(ctrlBtnSize_Large + 10), apGUILOFactory.I.Height(ctrlBtnSize_Large)))
			{
				//제어 : 다음 프레임으로 이동

				//[v1.4.2] FFD 체크
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					int nextFrame = AnimClip.CurFrame + 1;
					if (nextFrame > AnimClip.EndFrame)
					{
						if (AnimClip.IsLoop)
						{
							nextFrame = AnimClip.StartFrame;
						}
					}
					AnimClip.SetFrame_Editor(nextFrame);

					bool isWorkKeyframeChanged = false;
					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}
			}

			//if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Anim_LastFrame), "Move to Last Frame (Shift + >)"), GUILayout.Width(ctrlBtnSize_Large), GUILayout.Height(ctrlBtnSize_Large)))
			if (GUILayout.Button(_guiContent_Bottom_Animation_LastFrame.Content, apGUILOFactory.I.Width(ctrlBtnSize_Large), apGUILOFactory.I.Height(ctrlBtnSize_Large)))
			{
				//제어 : 마지막 프레임으로 이동

				//[v1.4.2] FFD 체크
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					AnimClip.SetFrame_Editor(AnimClip.EndFrame);

					bool isWorkKeyframeChanged = false;
					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}
			}

			GUILayout.Space(10);
			bool isLoopPlay = AnimClip.IsLoop;
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_Loop), isLoopPlay, true, ctrlBtnSize_Large, ctrlBtnSize_Large, apStringFactory.I.ToggleAnimLoop))//"Enable/Disable Loop"
			{
				if(AnimClip._targetMeshGroup != null)
				{
					apEditorUtil.SetRecord_PortraitMeshGroup(	apUndoGroupData.ACTION.Anim_SettingChanged,
																Editor, 
																Editor._portrait,
																AnimClip._targetMeshGroup,
																//AnimClip,
																false,
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_SettingChanged,
														Editor, 
														Editor._portrait,
														//AnimClip,
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				
				//AnimClip._isLoop = !AnimClip._isLoop;
				AnimClip.SetOption_IsLoop(!AnimClip.IsLoop);
				AnimClip.SetFrame_Editor(AnimClip.CurFrame);

				Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.

				//Loop를 바꿨다면 전체 Sort를 해야겠다.
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, null, null);
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(leftTabWidth), apGUILOFactory.I.Height(ctrlBtnSize_LargeUnder + 2));

			GUILayout.Space(5);

			//현재 프레임 + 세밀 조정
			//"Frame"
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Frame), apGUILOFactory.I.Width(80), apGUILOFactory.I.Height(ctrlBtnSize_LargeUnder));
			int curFrame = AnimClip.CurFrame;
			int nextCurFrame = EditorGUILayout.IntSlider(curFrame, AnimClip.StartFrame, AnimClip.EndFrame, apGUILOFactory.I.Width(leftTabWidth - 95), apGUILOFactory.I.Height(ctrlBtnSize_LargeUnder));
			if (nextCurFrame != curFrame)
			{	
				//[v1.4.2] FFD 체크
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					AnimClip.SetFrame_Editor(nextCurFrame);

					bool isWorkKeyframeChanged = false;
					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(mainTabWidth - leftTabWidth), apGUILOFactory.I.Height(bottomControlHeight));
			GUILayout.Space(18);
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(mainTabWidth - leftTabWidth), apGUILOFactory.I.Height(bottomControlHeight - 18));

			//>>여기서부터

			//맨 하단은 키 복붙이나 View, 영역 등에 관련된 정보를 출력한다.
			GUILayout.Space(10);

			//Timeline 정렬
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortRegOrder),
											Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.Registered,
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											apStringFactory.I.AnimTimelineSort_RegOrder))//"Sort by registeration order"
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.Registered;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Info | apEditor.REFRESH_TIMELINE_REQUEST.Timelines, null, null);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortABC),
											Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.ABC,
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											apStringFactory.I.AnimTImelineSort_Name))//"Sort by name"
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.ABC;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Info | apEditor.REFRESH_TIMELINE_REQUEST.Timelines, null, null);
			}

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_SortDepth),
											Editor._timelineInfoSortType == apEditor.TIMELINE_INFO_SORT.Depth,
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											apStringFactory.I.AnimTimelineSort_Depth))//"Sort by Depth"
			{
				Editor._timelineInfoSortType = apEditor.TIMELINE_INFO_SORT.Depth;
				Editor.SaveEditorPref();
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Info | apEditor.REFRESH_TIMELINE_REQUEST.Timelines, null, null);

			}

			GUILayout.Space(20);

			//"Unhide Layers"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.UnhideLayers), !isAnyHidedLayer, 120, ctrlBtnSize_Small))
			{
				Editor.ShowAllTimelineLayers();
			}

			GUILayout.Space(20);



			// 타임라인 사이즈 (1, 2, 3)
			apEditor.TIMELINE_LAYOUTSIZE nextLayoutSize = Editor._timelineLayoutSize;

			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize1),
											Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size1,
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											apStringFactory.I.AnimTimelineSize_Small))//"Timeline UI Size [Small]"
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size1;
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize2),
											Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size2,
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											apStringFactory.I.AnimTimelineSize_Medium))//"Timeline UI Size [Medium]"
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size2;
			}
			if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Anim_TimelineSize3),
											Editor._timelineLayoutSize == apEditor.TIMELINE_LAYOUTSIZE.Size3,
											true, ctrlBtnSize_Small, ctrlBtnSize_Small,
											apStringFactory.I.AnimTimelineSize_Large))//"Timeline UI Size [Large]"
			{
				nextLayoutSize = apEditor.TIMELINE_LAYOUTSIZE.Size3;
			}


			//Zoom
			GUILayout.Space(4);

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(90));
			//EditorGUILayout.LabelField("Zoom", GUILayout.Width(100), GUILayout.Height(15));
			GUILayout.Space(7);
			
			//이전
			//int nextTimelineIndex = (int)(GUILayout.HorizontalSlider(Editor._timelineZoom_Index, timelineLayoutSize_Min, timelineLayoutSize_Max, apGUILOFactory.I.Width(90), apGUILOFactory.I.Height(20)) + 0.5f);

			//변경 22.12.27 [v1.4.2] : 슬라이더 방향을 거꾸로 만든다.
			int iReverseZoom = timelineLayoutSize_Max - Editor._timelineZoom_Index;
			int iNextReverseZoom = (int)(GUILayout.HorizontalSlider(iReverseZoom, timelineLayoutSize_Min, timelineLayoutSize_Max, apGUILOFactory.I.Width(90), apGUILOFactory.I.Height(20)) + 0.5f);
			int nextTimelineIndex = timelineLayoutSize_Max - iNextReverseZoom;

			if (nextTimelineIndex != Editor._timelineZoom_Index)
			{
				if (nextTimelineIndex < timelineLayoutSize_Min) { nextTimelineIndex = timelineLayoutSize_Min; }
				else if (nextTimelineIndex > timelineLayoutSize_Max) { nextTimelineIndex = timelineLayoutSize_Max; }

				Editor._timelineZoom_Index = nextTimelineIndex;
			}
			EditorGUILayout.EndVertical();

			//Fit은 유지

			//if (GUILayout.Button(new GUIContent(" Fit", Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoZoom), "Zoom to fit the animation length"),
			//						GUILayout.Width(80), GUILayout.Height(ctrlBtnSize_Small)))

			if (_guiContent_Bottom_Animation_Fit == null)
			{
				//" Fit" / "Zoom to fit the animation length"
				_guiContent_Bottom_Animation_Fit = apGUIContentWrapper.Make(apStringFactory.I.AnimTimelineFit, Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoZoom), apStringFactory.I.AnimTimelineFitTooltip);
			}

			if (GUILayout.Button(_guiContent_Bottom_Animation_Fit.Content, apGUILOFactory.I.Width(80), apGUILOFactory.I.Height(ctrlBtnSize_Small)))
			{
				//Debug.LogError("TODO : Timeline AutoZoom");
				//Width / 전체 Frame수 = 목표 WidthPerFrame
				int numFrames = Mathf.Max(AnimClip.EndFrame - AnimClip.StartFrame, 1);
				int targetWidthPerFrame = (int)((float)timelineWidth / (float)numFrames + 0.5f);
				_scroll_Timeline.x = 0;
				//적절한 값을 찾자
				int optWPFIndex = -1;
				for (int i = 0; i < Editor._timelineZoomWPFPreset.Length; i++)
				{
					int curWPF = Editor._timelineZoomWPFPreset[i];
					if (curWPF < targetWidthPerFrame)
					{
						optWPFIndex = i;
						break;
					}
				}
				if (optWPFIndex < 0)
				{
					Editor._timelineZoom_Index = Editor._timelineZoomWPFPreset.Length - 1;
				}
				else
				{
					Editor._timelineZoom_Index = optWPFIndex;
				}
			}

			GUILayout.Space(4);

			//Auto Scroll
			//" Auto Scroll"
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoScroll),
													1, Editor.GetUIWord(UIWORD.AutoScroll), Editor.GetUIWord(UIWORD.AutoScroll),
													Editor._isAnimAutoScroll, true,
													140, ctrlBtnSize_Small,
													apStringFactory.I.AnimTimelineAutoScrollTooltip))//"Scrolls automatically according to the frame of the animation"
			{
				Editor._isAnimAutoScroll = !Editor._isAnimAutoScroll;
			}


			//AutoKey
			GUILayout.Space(6);
			if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Anim_AutoKey),
													1, Editor.GetUIWord(UIWORD.AutoKey), Editor.GetUIWord(UIWORD.AutoKey),
													Editor._isAnimAutoKey, true,
													140, ctrlBtnSize_Small,
													apStringFactory.I.GetHotkeyTooltip_AnimTimelineAutoKeyTooltip(Editor.HotKeyMap)
													))//"When you move the object, keyframes are automatically created"
			{
				Editor._isAnimAutoKey = !Editor._isAnimAutoKey;

				//마지막 값을 저장하자
				EditorPrefs.SetBool("AnyPortrait_LastAnimAutoKeyValue", Editor._isAnimAutoKey);
			}


			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();


			EditorGUILayout.EndVertical();


			//추가 20.12.5
			//Auto Key 단축키 이벤트를 추가하자
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimAutoKeyToggle, apHotKeyMapping.KEY_TYPE.Anim_ToggleAutoKey, null);




			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(rightTabWidth), apGUILOFactory.I.Height(height));
			//2. [우측] 선택된 레이어/키 정보

			_scrollPos_BottomAnimationRightProperty = EditorGUILayout.BeginScrollView(_scrollPos_BottomAnimationRightProperty, false, true, apGUILOFactory.I.Width(rightTabWidth), apGUILOFactory.I.Height(height));

			//int rightPropertyWidth = rightTabWidth - 24;
			int rightPropertyWidth = rightTabWidth - 28;

			EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(rightPropertyWidth));


			//프로퍼티 타이틀
			//프로퍼티는 (KeyFrame -> Layer -> Timeline -> None) 순으로 정보를 보여준다.
			//string propertyTitle = "";

			if (_strWrapper_64 == null)
			{
				_strWrapper_64 = new apStringWrapper(64);
			}
			_strWrapper_64.Clear();

			ANIM_BOTTOM_PROPERTY_UI propertyUIType = ANIM_BOTTOM_PROPERTY_UI.NoSelected;
			if (AnimKeyframe != null)
			{
				if (IsAnimKeyframeMultipleSelected)
				{
					//propertyTitle = "Keyframes [ " + AnimKeyframes.Count + " Selected ]";
					//propertyTitle = string.Format("{0} [ {1} {2} ]", Editor.GetUIWord(UIWORD.Keyframes), AnimKeyframes.Count, Editor.GetUIWord(UIWORD.Selected));

					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Keyframes), false);
					_strWrapper_64.AppendSpace(1, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
					_strWrapper_64.Append(AnimKeyframes.Count, false);
					_strWrapper_64.AppendSpace(1, false);
					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Selected), false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

					propertyUIType = ANIM_BOTTOM_PROPERTY_UI.MultipleKeyframes;
				}
				else
				{
					//propertyTitle = "Keyframe [ " + AnimKeyframe._frameIndex + " ]";
					//propertyTitle = string.Format("{0} [ {1} ]", Editor.GetUIWord(UIWORD.Keyframe), AnimKeyframe._frameIndex);

					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Keyframe), false);
					_strWrapper_64.AppendSpace(1, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
					_strWrapper_64.Append(AnimKeyframe._frameIndex, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

					propertyUIType = ANIM_BOTTOM_PROPERTY_UI.SingleKeyframe;
				}

			}
			else if (AnimTimelineLayer_Main != null)
			{
				if (AnimTimelineLayers_All != null && AnimTimelineLayers_All.Count > 1)
				{
					//타임라인 레이어가 여러개인 경우 (20.6.12)
					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.TimelineLayers), false);
					_strWrapper_64.AppendSpace(1, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
					_strWrapper_64.Append(AnimTimelineLayers_All.Count, false);
					_strWrapper_64.AppendSpace(1, false);
					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Selected), false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

					propertyUIType = ANIM_BOTTOM_PROPERTY_UI.MultipleTimelineLayers;
				}
				else
				{
					//타임라인 레이어가 한개인 경우
					//propertyTitle = "Layer [" + AnimTimelineLayer.DisplayName + " ]";
					//propertyTitle = string.Format("{0} [ {1} ]", Editor.GetUIWord(UIWORD.Layer), AnimTimelineLayer.DisplayName);

					_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Layer), false);
					_strWrapper_64.AppendSpace(1, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
					_strWrapper_64.Append(AnimTimelineLayer_Main.DisplayName, false);
					_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

					propertyUIType = ANIM_BOTTOM_PROPERTY_UI.SingleTimelineLayer;
				}
				
			}
			else if (AnimTimeline != null)
			{
				//propertyTitle = "Timeline [ " + AnimTimeline.DisplayName + " ]";
				//propertyTitle = string.Format("{0} [ {1} ]", Editor.GetUIWord(UIWORD.Timeline), AnimTimeline.DisplayName);

				_strWrapper_64.Append(Editor.GetUIWord(UIWORD.Timeline), false);
				_strWrapper_64.AppendSpace(1, false);
				_strWrapper_64.Append(apStringFactory.I.Bracket_2_L, false);
				_strWrapper_64.Append(AnimTimeline.DisplayName, false);
				_strWrapper_64.Append(apStringFactory.I.Bracket_2_R, true);

				propertyUIType = ANIM_BOTTOM_PROPERTY_UI.Timeline;
			}
			else
			{
				//propertyTitle = "Not Selected";
				//propertyTitle = Editor.GetUIWord(UIWORD.NotSelected);
				_strWrapper_64.Append(Editor.GetUIWord(UIWORD.NotSelected), true);

				propertyUIType = ANIM_BOTTOM_PROPERTY_UI.NoSelected;
			}

			//GUIStyle guiStyleProperty = new GUIStyle(GUI.skin.box);
			//guiStyleProperty.normal.textColor = Color.white;
			//guiStyleProperty.alignment = TextAnchor.MiddleCenter;

			//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
			GUI.backgroundColor = apEditorUtil.ToggleBoxColor_Selected;

			GUILayout.Box(_strWrapper_64.ToString(), apGUIStyleWrapper.I.Box_MiddleCenter_WhiteColor, apGUILOFactory.I.Width(rightPropertyWidth), apGUILOFactory.I.Height(20));
			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);


			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__MK, propertyUIType == ANIM_BOTTOM_PROPERTY_UI.MultipleKeyframes);//"Animation Bottom Property - MK"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__SK, propertyUIType == ANIM_BOTTOM_PROPERTY_UI.SingleKeyframe);//"Animation Bottom Property - SK"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__ML, propertyUIType == ANIM_BOTTOM_PROPERTY_UI.MultipleTimelineLayers);//"Animation Bottom Property - L"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__SL, propertyUIType == ANIM_BOTTOM_PROPERTY_UI.SingleTimelineLayer);//"Animation Bottom Property - L"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__T, propertyUIType == ANIM_BOTTOM_PROPERTY_UI.Timeline);//"Animation Bottom Property - T"

			switch (propertyUIType)
			{
				case ANIM_BOTTOM_PROPERTY_UI.MultipleKeyframes:

					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__MK))//"Animation Bottom Property - MK"
					{
						DrawEditor_Bottom_AnimationProperty_MultipleKeyframes(AnimKeyframes, rightPropertyWidth,
							windowWidth,
							windowHeight,
							(layoutX + leftTabWidth + margin + mainTabWidth + margin),
							(int)(layoutY),
							(int)(_scrollPos_BottomAnimationRightProperty.y));
					}
					break;

				case ANIM_BOTTOM_PROPERTY_UI.SingleKeyframe:

					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__SK))//"Animation Bottom Property - SK"
					{
						DrawEditor_Bottom_AnimationProperty_SingleKeyframe(
							AnimKeyframe,
							rightPropertyWidth,
							windowWidth,
							windowHeight,
							(layoutX + leftTabWidth + margin + mainTabWidth + margin),
							//layoutX + margin + mainTabWidth + margin, 
							//leftTabWidth + margin + mainTabWidth + margin, 
							(int)(layoutY),
							(int)(_scrollPos_BottomAnimationRightProperty.y)
							//(int)(layoutY)
							);
					}
					break;

				//추가 20.6.12 : 타임라인 레이어를 여러개 선택한 경우
				case ANIM_BOTTOM_PROPERTY_UI.MultipleTimelineLayers:
					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__ML))
					{
						DrawEditor_Bottom_AnimationProperty_MultipleTimelineLayers(AnimTimelineLayers_All, rightPropertyWidth);
					}
					break;



				case ANIM_BOTTOM_PROPERTY_UI.SingleTimelineLayer:
					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__SL))//"Animation Bottom Property - L"
					{
						DrawEditor_Bottom_AnimationProperty_TimelineLayer(AnimTimelineLayer_Main, rightPropertyWidth);
					}
					break;

				case ANIM_BOTTOM_PROPERTY_UI.Timeline:
					if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Animation_Bottom_Property__T))//"Animation Bottom Property - T"
					{
						DrawEditor_Bottom_AnimationProperty_Timeline(AnimTimeline, rightPropertyWidth);
					}
					break;
			}



			GUILayout.Space(height);

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();


			EditorGUILayout.EndHorizontal();



			//자동 스크롤 이벤트 요청이 들어왔다.
			//처리를 해주자
			if (_isAnimTimelineLayerGUIScrollRequest)
			{
				//일단 어느 TimelineInfo인지 찾고,
				//그 값으로 이동
				apTimelineLayerInfo targetInfo = null;
				if (AnimTimelineLayer_Main != null)
				{
					targetInfo = timelineInfoList.Find(delegate (apTimelineLayerInfo a)
					{
						return a._layer == AnimTimelineLayer_Main && a.IsVisibleLayer;
					});
				}


				//삭제 : 19.11.22 : 타임라인으로는 자동으로 스크롤 되지 않는다.
				//else if (_subAnimTimeline != null)
				//{
				//	targetInfo = timelineInfoList.Find(delegate (apTimelineLayerInfo a)
				//	{
				//		return a._timeline == _subAnimTimeline && a._isTimeline;
				//	});
				//}


				if (targetInfo != null)
				{
					//화면 밖에 있는 경우에 한해서
					//위쪽에 있으면 위쪽으로, 아니면 아래쪽으로 설정하자
					if (Editor._timelineLayoutSize != apEditor.TIMELINE_LAYOUTSIZE.Size1)
					{
						//타임라인의 크기가 1이 아니라면 항목 위치에 따라서 스크롤을 조절
						if (targetInfo._guiLayerPosY < _scroll_Timeline.y - (heightPerLayer + 3))
						{

							//스크롤보다 위쪽 (작은 인덱스)인 경우
							_scroll_Timeline.y = targetInfo._guiLayerPosY - heightPerLayer;
							if (_scroll_Timeline.y < 0.0f)
							{
								_scroll_Timeline.y = 0.0f;
							}
						}
						else if (targetInfo._guiLayerPosY > _scroll_Timeline.y + (timelineHeight + 3))
						{
							//스크롤보다 아래쪽 (큰 인덱스)인 경우
							//중간으로 올린다.
							_scroll_Timeline.y = targetInfo._guiLayerPosY - ((timelineHeight * 0.5f) + heightPerLayer);
							if (_scroll_Timeline.y < 0.0f)
							{
								_scroll_Timeline.y = 0.0f;
							}
						}
					}
					else
					{
						//타임라인의 크기가 1이라면 그냥 매번 고정
						_scroll_Timeline.y = targetInfo._guiLayerPosY - heightPerLayer;
						if (_scroll_Timeline.y < 0.0f)
						{
							_scroll_Timeline.y = 0.0f;
						}
					}

					//Debug.LogError("Timeline Y Changed : " + GUI.GetNameOfFocusedControl());
				}

				_isAnimTimelineLayerGUIScrollRequest = false;
				Editor.SetRepaint();//<<화면 갱신 요청
			}




			if (Editor._timelineLayoutSize != nextLayoutSize)
			{
				Editor._timelineLayoutSize = nextLayoutSize;
			}

			if (isEventOccurred)
			{
				Editor.SetRepaint();
			}
		}


		

		/// <summary>
		/// 애니메이션 우측 하단 UI : 키프레임을 "1개 선택할 때" 출력되는 UI
		/// </summary>
		private void DrawEditor_Bottom_AnimationProperty_SingleKeyframe(apAnimKeyframe keyframe, int width, int windowWidth, int windowHeight, int layoutX, int layoutY, int scrollValue)
		{
			
			//프레임 이동
			//EditorGUILayout.LabelField("Frame [" + keyframe._frameIndex + "]", GUILayout.Width(width));
			//GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));

			GUILayout.Space(5);

			Texture2D imgPrev = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame);
			Texture2D imgNext = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame);
			Texture2D imgCurKey = Editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToCurrentFrame);

			int btnWidthSide = ((width - (10 + 80)) / 2) - 4;
			int btnWidthCenter = 90;
			bool isPrevKeyExist = false;
			bool isNextKeyExist = false;
			bool isCurKeyExist = (AnimClip.CurFrame == keyframe._frameIndex);
			if (keyframe._prevLinkedKeyframe != null)
			{
				isPrevKeyExist = true;
			}
			if (keyframe._nextLinkedKeyframe != null)
			{
				isNextKeyExist = true;
			}

			//[v1.4.2 변경점]
			//FFD가 켜진 상태에서 프레임 변화가 발생하면 FFD를 종료시켜야 한다.

			if (apEditorUtil.ToggledButton_2Side(imgPrev, false, isPrevKeyExist, btnWidthSide, 20))
			{
				//연결된 이전 프레임으로 이동한다.
				if (isPrevKeyExist)
				{
					//FFD 체크
					bool isExecutable = Editor.CheckModalAndExecutable();

					if (isExecutable)
					{
						AnimClip.SetFrame_Editor(keyframe._prevLinkedKeyframe._frameIndex);
						SelectAnimKeyframe(keyframe._prevLinkedKeyframe, true, apGizmos.SELECT_TYPE.New);

						Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
					}
				}
			}
			if (apEditorUtil.ToggledButton_2Side(imgCurKey, isCurKeyExist, true, btnWidthCenter, 20))
			{
				//현재 프레임으로 이동한다.
				
				//FFD 체크
				bool isExecutable = Editor.CheckModalAndExecutable();

				if (isExecutable)
				{
					AnimClip.SetFrame_Editor(keyframe._frameIndex);

					bool isWorkKeyframeChanged = false;
					AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
					SetAutoAnimScroll();

					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}
			}
			if (apEditorUtil.ToggledButton_2Side(imgNext, false, isNextKeyExist, btnWidthSide, 20))
			{
				//연결된 다음 프레임으로 이동한다.
				if (isNextKeyExist)
				{
					AnimClip.SetFrame_Editor(keyframe._nextLinkedKeyframe._frameIndex);
					SelectAnimKeyframe(keyframe._nextLinkedKeyframe, true, apGizmos.SELECT_TYPE.New);

					Editor.SetMeshGroupChanged();//<<추가 : 강제 업데이트를 해야한다.
				}
			}


			EditorGUILayout.EndHorizontal();


			//Value / Curve에 따라서 다른 UI가 나온다.
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(22));
			GUILayout.Space(5);
			//"Transform"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Transform),
											(_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Value),
											(width / 2) - 2
										))
			{
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;
			}
			//"Curve"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Curve),
											(_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Curve),
											(width / 2) - 2
										))
			{
				_animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Curve;
			}

			EditorGUILayout.EndHorizontal();

			apAnimTimelineLayer parentTimelineLayer = keyframe._parentTimelineLayer;
			apAnimTimeline parentTimeline = parentTimelineLayer._parentTimeline;

			//키프레임 타입인 경우
			bool isControlParamUI = (parentTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam &&
									parentTimelineLayer._linkedControlParam != null);
			bool isModifierUI = (parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier &&
									parentTimeline._linkedModifier != null);

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom_Right_Anim_Property__ControlParamUI, isControlParamUI);//"Bottom Right Anim Property - ControlParamUI"
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom_Right_Anim_Property__ModifierUI, isModifierUI);//"Bottom Right Anim Property - ModifierUI"

			bool isDrawControlParamUI = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom_Right_Anim_Property__ControlParamUI);//"Bottom Right Anim Property - ControlParamUI"
			bool isDrawModifierUI = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom_Right_Anim_Property__ModifierUI);//"Bottom Right Anim Property - ModifierUI"


			apControlParam controlParam = parentTimelineLayer._linkedControlParam;

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Anim_Property__SameKeyframe, _tmpPrevSelectedAnimKeyframe == keyframe);//"Anim Property - SameKeyframe"
			bool isSameKP = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Anim_Property__SameKeyframe);//"Anim Property - SameKeyframe"

			//if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
			if (Event.current.type != EventType.Layout)
			{
				_tmpPrevSelectedAnimKeyframe = keyframe;
			}

			Color prevColor = GUI.backgroundColor;


			if (_animPropertyUI == ANIM_SINGLE_PROPERTY_UI.Value)
			{
				//1. Value Mode
				if (isDrawControlParamUI && isSameKP)
				{
#region Control Param UI 그리는 코드
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width);
					GUILayout.Space(10);

					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);

					//GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
					//guiStyleBox.alignment = TextAnchor.MiddleCenter;
					//guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

					//"Control Parameter Value"
					GUILayout.Box(Editor.GetUIWord(UIWORD.ControlParameterValue),
									apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor,
									apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

					GUI.backgroundColor = prevColor;


					//GUIStyle guiStyle_LableMin = new GUIStyle(GUI.skin.label);
					//guiStyle_LableMin.alignment = TextAnchor.MiddleLeft;

					//GUIStyle guiStyle_LableMax = new GUIStyle(GUI.skin.label);
					//guiStyle_LableMax.alignment = TextAnchor.MiddleRight;

					int widthLabelRange = (width / 2) - 2;

					GUILayout.Space(5);

					bool isChanged = false;

					switch (controlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							{
								int iNext = keyframe._conSyncValue_Int;

								EditorGUILayout.LabelField(controlParam._keyName, apGUILOFactory.I.Width(width));
								EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
								EditorGUILayout.LabelField(controlParam._label_Min, apGUIStyleWrapper.I.Label_MiddleLeft, apGUILOFactory.I.Width(widthLabelRange));
								EditorGUILayout.LabelField(controlParam._label_Max, apGUIStyleWrapper.I.Label_MiddleRight, apGUILOFactory.I.Width(widthLabelRange));
								EditorGUILayout.EndHorizontal();
								iNext = EditorGUILayout.IntSlider(keyframe._conSyncValue_Int, controlParam._int_Min, controlParam._int_Max, apGUILOFactory.I.Width(width));


								if (iNext != keyframe._conSyncValue_Int)
								{
									apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																		Editor, 
																		Editor._portrait, 
																		//keyframe, 
																		true,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

									keyframe._conSyncValue_Int = iNext;
									isChanged = true;
								}
							}
							break;

						case apControlParam.TYPE.Float:
							{
								float fNext = keyframe._conSyncValue_Float;

								EditorGUILayout.LabelField(controlParam._keyName, apGUILOFactory.I.Width(width));
								EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width));
								EditorGUILayout.LabelField(controlParam._label_Min, apGUIStyleWrapper.I.Label_MiddleLeft, apGUILOFactory.I.Width(widthLabelRange));
								EditorGUILayout.LabelField(controlParam._label_Max, apGUIStyleWrapper.I.Label_MiddleRight, apGUILOFactory.I.Width(widthLabelRange));
								EditorGUILayout.EndHorizontal();
								fNext = EditorGUILayout.Slider(keyframe._conSyncValue_Float, controlParam._float_Min, controlParam._float_Max, apGUILOFactory.I.Width(width));

								if (fNext != keyframe._conSyncValue_Float)
								{
									apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																		Editor, 
																		Editor._portrait, 
																		//keyframe, 
																		true,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

									keyframe._conSyncValue_Float = fNext;
									isChanged = true;
								}
							}
							break;

						case apControlParam.TYPE.Vector2:
							{
								Vector2 v2Next = keyframe._conSyncValue_Vector2;
								EditorGUILayout.LabelField(controlParam._keyName, apGUILOFactory.I.Width(width));

								EditorGUILayout.LabelField(controlParam._label_Min, apGUILOFactory.I.Width(width));
								v2Next.x = EditorGUILayout.Slider(keyframe._conSyncValue_Vector2.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x, apGUILOFactory.I.Width(width));

								EditorGUILayout.LabelField(controlParam._label_Max, apGUILOFactory.I.Width(width));
								v2Next.y = EditorGUILayout.Slider(keyframe._conSyncValue_Vector2.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y, apGUILOFactory.I.Width(width));

								if (v2Next.x != keyframe._conSyncValue_Vector2.x ||
									v2Next.y != keyframe._conSyncValue_Vector2.y)
								{
									apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																		Editor, 
																		Editor._portrait, 
																		//keyframe, 
																		true,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

									keyframe._conSyncValue_Vector2 = v2Next;
									isChanged = true;
								}
							}
							break;

					}

					if (isChanged)
					{
						AnimClip.UpdateControlParam_Editor();
					}
#endregion
				}

				if (isDrawModifierUI && isSameKP)
				{
					GUILayout.Space(10);
					apEditorUtil.GUI_DelimeterBoxH(width);
					GUILayout.Space(10);

					GUI.backgroundColor = new Color(0.4f, 1.0f, 0.5f, 1.0f);

					//GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
					//guiStyleBox.alignment = TextAnchor.MiddleCenter;
					//guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

					apModifierBase linkedModifier = AnimTimeline._linkedModifier;


					//string boxText = null;
					bool isMod_Morph = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0);
					bool isMod_TF = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.TransformMatrix) != 0);
					bool isMod_Color = ((int)(linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0);

					//if (isMod_Morph)
					//{
					//	//boxText = "Morph Modifier Value";
					//	boxText = Editor.GetUIWord(UIWORD.MorphModifierValue);
					//}
					//else
					//{
					//	//boxText = "Transform Modifier Value";
					//	boxText = Editor.GetUIWord(UIWORD.TransformModifierValue);
					//}

					GUILayout.Box(isMod_Morph ? Editor.GetUIWord(UIWORD.MorphModifierValue) : Editor.GetUIWord(UIWORD.TransformModifierValue),
									apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor,
									apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

					GUI.backgroundColor = prevColor;

					//apModifierParamSet paramSet = keyframe._linkedParamSet_Editor;
					apModifiedMesh modMesh = keyframe._linkedModMesh_Editor;
					apModifiedBone modBone = keyframe._linkedModBone_Editor;
					if (modMesh == null)
					{
						isMod_Morph = false;
						isMod_Color = false;
					}
					if (modBone == null && modMesh == null)
					{
						//TF 타입은 Bone 타입이 적용될 수 있다.
						isMod_TF = false;
					}
					//TODO : 여기서부터 작성하자

					bool isChanged = false;

					if (_guiContent_Icon_ModTF_Pos == null) { _guiContent_Icon_ModTF_Pos = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move)); }
					if (_guiContent_Icon_ModTF_Rot == null) { _guiContent_Icon_ModTF_Rot = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate)); }
					if (_guiContent_Icon_ModTF_Scale == null) { _guiContent_Icon_ModTF_Scale = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale)); }
					if (_guiContent_Icon_Mod_Color == null) { _guiContent_Icon_Mod_Color = apGUIContentWrapper.Make(Editor.ImageSet.Get(apImageSet.PRESET.Transform_Color)); }

					if (isMod_Morph)
					{
						GUILayout.Space(5);
					}

					if (isMod_TF)
					{
						GUILayout.Space(5);

						//Texture2D img_Pos = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Move);
						//Texture2D img_Rot = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Rotate);
						//Texture2D img_Scale = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Scale);


						//Texture2D img_BoneIK = Editor.ImageSet.Get(apImageSet.PRESET.Transform_IKController);

						Vector2 nextPos = Vector2.zero;
						float nextAngle = 0.0f;
						Vector2 nextScale = Vector2.one;
						//float nextBoneIKWeight = 0.0f;

						if (modMesh != null)
						{
							nextPos = modMesh._transformMatrix._pos;
							nextAngle = modMesh._transformMatrix._angleDeg;
							nextScale = modMesh._transformMatrix._scale;
						}
						else if (modBone != null)
						{
							nextPos = modBone._transformMatrix._pos;
							nextAngle = modBone._transformMatrix._angleDeg;
							nextScale = modBone._transformMatrix._scale;
							//nextBoneIKWeight = modBone._boneIKController_MixWeight;
						}

						bool isAngleIsOutRange = nextAngle <= -180.0f || nextAngle >= 180.0f;

						int iconSize = 30;
						int propertyWidth = width - (iconSize + 8);
						int rotationLockBtnSize = 26;
						int propertyRotationWidth = width - (iconSize + rotationLockBtnSize + 16);



						//Position
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(iconSize));

						//EditorGUILayout.LabelField(new GUIContent(img_Pos), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField(_guiContent_Icon_ModTF_Pos.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconSize));

						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(propertyWidth), apGUILOFactory.I.Height(iconSize));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Position));//"Position"
																					  //nextPos = EditorGUILayout.Vector2Field("", nextPos, GUILayout.Width(propertyWidth));
						nextPos = apEditorUtil.DelayedVector2Field(nextPos, propertyWidth);
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();

						//Rotation
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(iconSize));

						//EditorGUILayout.LabelField(new GUIContent(img_Rot), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField(_guiContent_Icon_ModTF_Rot.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconSize));

						EditorGUILayout.EndVertical();

						//변경 20.1.21 : 180도 제한을 풀 수 있다.
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(propertyRotationWidth), apGUILOFactory.I.Height(iconSize));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Rotation), apGUILOFactory.I.Width(propertyRotationWidth));//"Rotation"

						if (isAngleIsOutRange)
						{
							//만약 각도 범위가 180 제한을 벗어났다면 색상을 구분해서 주의를 주자
							GUI.backgroundColor = new Color(1.0f, 0.8f, 0.8f, 1.0f);
						}
						nextAngle = EditorGUILayout.DelayedFloatField(nextAngle, apGUILOFactory.I.Width(propertyRotationWidth));

						if (isAngleIsOutRange)
						{
							GUI.backgroundColor = prevColor;
						}

						EditorGUILayout.EndVertical();


						//추가 20.1.21 : 180도 제한 설정/해제 버튼
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(rotationLockBtnSize), apGUILOFactory.I.Height(iconSize));
						GUILayout.Space(6);
						if (apEditorUtil.ToggledButton_2Side(
							Editor.ImageSet.Get(apImageSet.PRESET.Anim_180Lock),
							Editor.ImageSet.Get(apImageSet.PRESET.Anim_180Unlock), Editor._isAnimRotation180Lock, true, rotationLockBtnSize, rotationLockBtnSize))
						{
							Editor._isAnimRotation180Lock = !Editor._isAnimRotation180Lock;
						}
						EditorGUILayout.EndVertical();


						EditorGUILayout.EndHorizontal();

						//추가 : CW, CCW 옵션을 표시한다.
						int rotationEnumWidth = 80;
						int rotationValueWidth = (((width - 10) / 2) - rotationEnumWidth) - 4;
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
						GUILayout.Space(5);
						apAnimKeyframe.ROTATION_BIAS prevRotationBias = (apAnimKeyframe.ROTATION_BIAS)EditorGUILayout.EnumPopup(keyframe._prevRotationBiasMode, apGUILOFactory.I.Width(rotationEnumWidth));
						int prevRotationBiasCount = EditorGUILayout.IntField(keyframe._prevRotationBiasCount, apGUILOFactory.I.Width(rotationValueWidth));
						apAnimKeyframe.ROTATION_BIAS nextRotationBias = (apAnimKeyframe.ROTATION_BIAS)EditorGUILayout.EnumPopup(keyframe._nextRotationBiasMode, apGUILOFactory.I.Width(rotationEnumWidth));
						int nextRotationBiasCount = EditorGUILayout.IntField(keyframe._nextRotationBiasCount, apGUILOFactory.I.Width(rotationValueWidth));
						EditorGUILayout.EndHorizontal();



						//Scaling
						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(iconSize));

						//EditorGUILayout.LabelField(new GUIContent(img_Scale), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
						EditorGUILayout.LabelField(_guiContent_Icon_ModTF_Scale.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconSize));

						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(propertyWidth), apGUILOFactory.I.Height(iconSize));
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Scaling));//"Scaling"

						//nextScale = EditorGUILayout.Vector2Field("", nextScale, GUILayout.Width(propertyWidth));
						nextScale = apEditorUtil.DelayedVector2Field(nextScale, propertyWidth);
						EditorGUILayout.EndVertical();
						EditorGUILayout.EndHorizontal();




						if (modMesh != null)
						{
							if (nextPos.x != modMesh._transformMatrix._pos.x ||
								nextPos.y != modMesh._transformMatrix._pos.y ||
								nextAngle != modMesh._transformMatrix._angleDeg ||
								nextScale.x != modMesh._transformMatrix._scale.x ||
								nextScale.y != modMesh._transformMatrix._scale.y
								)
							{
								isChanged = true;

								apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	linkedModifier, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);


								//추가 20.1.21 : 180 제한 옵션에 따라 이 각도를 180 이내로 제한한다
								if (Editor._isAnimRotation180Lock)
								{
									nextAngle = apUtil.AngleTo180(nextAngle);
								}

								modMesh._transformMatrix.SetPos(nextPos, false);
								modMesh._transformMatrix.SetRotate(nextAngle, false);
								modMesh._transformMatrix.SetScale(nextScale, false);
								modMesh._transformMatrix.MakeMatrix();

								apEditorUtil.ReleaseGUIFocus();
							}
						}
						else if (modBone != null)
						{
							if (nextPos.x != modBone._transformMatrix._pos.x ||
								nextPos.y != modBone._transformMatrix._pos.y ||
								nextAngle != modBone._transformMatrix._angleDeg ||
								nextScale.x != modBone._transformMatrix._scale.x ||
								nextScale.y != modBone._transformMatrix._scale.y
								//nextBoneIKWeight != modBone._boneIKController_MixWeight
								)
							{
								isChanged = true;

								apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	linkedModifier, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								//추가 20.1.21 : 180 제한 옵션에 따라 이 각도를 180 이내로 제한한다
								if (Editor._isAnimRotation180Lock)
								{
									nextAngle = apUtil.AngleTo180(nextAngle);
								}

								modBone._transformMatrix.SetPos(nextPos, false);
								modBone._transformMatrix.SetRotate(nextAngle, false);
								modBone._transformMatrix.SetScale(nextScale, false);
								modBone._transformMatrix.MakeMatrix();

								//modBone._boneIKController_MixWeight = Mathf.Clamp01(nextBoneIKWeight);

								apEditorUtil.ReleaseGUIFocus();
							}


						}

						if (prevRotationBias != keyframe._prevRotationBiasMode ||
							prevRotationBiasCount != keyframe._prevRotationBiasCount ||
							nextRotationBias != keyframe._nextRotationBiasMode ||
							nextRotationBiasCount != keyframe._nextRotationBiasCount)
						{
							isChanged = true;

							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																Editor, 
																Editor._portrait, 
																//null, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							if (prevRotationBiasCount < 0) { prevRotationBiasCount = 0; }
							if (nextRotationBiasCount < 0) { nextRotationBiasCount = 0; }

							keyframe._prevRotationBiasMode = prevRotationBias;
							keyframe._prevRotationBiasCount = prevRotationBiasCount;
							keyframe._nextRotationBiasMode = nextRotationBias;
							keyframe._nextRotationBiasCount = nextRotationBiasCount;


						}

					}

					if (isMod_Color)
					{
						GUILayout.Space(5);

						if (linkedModifier._isColorPropertyEnabled)
						{
							//Texture2D img_Color = Editor.ImageSet.Get(apImageSet.PRESET.Transform_Color);

							Color nextColor = modMesh._meshColor;
							bool isMeshVisible = modMesh._isVisible;

							int iconSize = 30;
							int propertyWidth = width - (iconSize + 8);

							//Color
							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(iconSize));

							//EditorGUILayout.LabelField(new GUIContent(img_Color), GUILayout.Width(iconSize), GUILayout.Height(iconSize));
							EditorGUILayout.LabelField(_guiContent_Icon_Mod_Color.Content, apGUILOFactory.I.Width(iconSize), apGUILOFactory.I.Height(iconSize));

							EditorGUILayout.EndVertical();

							EditorGUILayout.BeginVertical(apGUILOFactory.I.Width(propertyWidth), apGUILOFactory.I.Height(iconSize));
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Color2X));//"Color (2X)"
							try
							{
								nextColor = EditorGUILayout.ColorField(apStringFactory.I.None, modMesh._meshColor, apGUILOFactory.I.Width(propertyWidth));
							}
							catch (Exception)
							{

							}

							EditorGUILayout.EndVertical();
							EditorGUILayout.EndHorizontal();


							//Visible
							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(iconSize));
							GUILayout.Space(5);
							EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.IsVisible), apGUILOFactory.I.Width(propertyWidth));//"Is Visible..
							isMeshVisible = EditorGUILayout.Toggle(isMeshVisible, apGUILOFactory.I.Width(iconSize));
							EditorGUILayout.EndHorizontal();



							if (nextColor.r != modMesh._meshColor.r ||
								nextColor.g != modMesh._meshColor.g ||
								nextColor.b != modMesh._meshColor.b ||
								nextColor.a != modMesh._meshColor.a ||
								isMeshVisible != modMesh._isVisible)
							{
								isChanged = true;

								apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	linkedModifier, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								modMesh._meshColor = nextColor;
								modMesh._isVisible = isMeshVisible;

								//apEditorUtil.ReleaseGUIFocus();

								//추가 20.7.5 : Visible이 바뀌면 Hierarchy를 갱신해야한다. (눈 아이콘때문에)
								Editor.RefreshControllerAndHierarchy(false);
							}
						}
						else
						{
							GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);

							//"Color Property is disabled"
							GUILayout.Box(Editor.GetUIWord(UIWORD.ColorPropertyIsDisabled),
											apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor,
											apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));

							GUI.backgroundColor = prevColor;
						}
					}

					GUILayout.Space(10);

					if (isChanged)
					{
						AnimClip.UpdateControlParam_Editor();
					}
				}
			}
			else
			{
				//2. Curve Mode
				//1) Prev 커브를 선택할 것인지, Next 커브를 선택할 것인지 결정해야한다.
				//2) 양쪽의 컨트롤 포인트의 설정을 결정한다. (Linear / Smooth / Constant(Stepped))
				//3) 커브 GUI

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

				GUILayout.Space(5);

				int curveTypeBtnSize = 30;
				int curveBtnSize = (width - (curveTypeBtnSize * 3 + 2 * 5)) / 2 - 6;

				apAnimCurve curveA = null;
				apAnimCurve curveB = null;
				apAnimCurveResult curveResult = null;

				//string strPrevKey = "";
				//string strNextKey = "";

				if (_guiContent_AnimKeyframeProp_PrevKeyLabel == null)
				{
					_guiContent_AnimKeyframeProp_PrevKeyLabel = new apGUIContentWrapper();
				}
				if (_guiContent_AnimKeyframeProp_NextKeyLabel == null)
				{
					_guiContent_AnimKeyframeProp_NextKeyLabel = new apGUIContentWrapper();
				}



				//변경
				bool isColorLabelRed_Prev = false;
				bool isColorLabelRed_Next = false;

				if (_animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev)
				{
					curveA = keyframe._curveKey._prevLinkedCurveKey;
					curveB = keyframe._curveKey;
					curveResult = keyframe._curveKey._prevCurveResult;

					if (keyframe._prevLinkedKeyframe != null)
					{
						//strPrevKey = "Prev [" + keyframe._prevLinkedKeyframe._frameIndex + "]";
						//strPrevKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Prev), keyframe._prevLinkedKeyframe._frameIndex);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.ClearText(false);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.AppendText(Editor.GetUIWord(UIWORD.Prev), false);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.AppendSpaceText(1, false);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.AppendText(keyframe._prevLinkedKeyframe._frameIndex, true);
					}
					else
					{
						_guiContent_AnimKeyframeProp_PrevKeyLabel.ClearText(true);
					}
					//strNextKey = "Current [" + keyframe._frameIndex + "]";
					//strNextKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Current), keyframe._frameIndex);

					_guiContent_AnimKeyframeProp_NextKeyLabel.ClearText(false);
					_guiContent_AnimKeyframeProp_NextKeyLabel.AppendText(Editor.GetUIWord(UIWORD.Current), false);
					_guiContent_AnimKeyframeProp_NextKeyLabel.AppendSpaceText(1, false);
					_guiContent_AnimKeyframeProp_NextKeyLabel.AppendText(keyframe._frameIndex, true);


					//colorLabel_Next = Color.red;
					isColorLabelRed_Next = true;
				}
				else
				{
					curveA = keyframe._curveKey;
					curveB = keyframe._curveKey._nextLinkedCurveKey;
					curveResult = keyframe._curveKey._nextCurveResult;


					//strPrevKey = "Current [" + keyframe._frameIndex + "]";
					//strNextKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Current), keyframe._frameIndex);
					_guiContent_AnimKeyframeProp_NextKeyLabel.ClearText(false);
					_guiContent_AnimKeyframeProp_NextKeyLabel.AppendText(Editor.GetUIWord(UIWORD.Current), false);
					_guiContent_AnimKeyframeProp_NextKeyLabel.AppendSpaceText(1, false);
					_guiContent_AnimKeyframeProp_NextKeyLabel.AppendText(keyframe._frameIndex, true);

					if (keyframe._nextLinkedKeyframe != null)
					{
						//strNextKey = "Next [" + keyframe._nextLinkedKeyframe._frameIndex + "]";
						//strPrevKey = string.Format("{0} [{1}]", Editor.GetUIWord(UIWORD.Next), keyframe._nextLinkedKeyframe._frameIndex);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.ClearText(false);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.AppendText(Editor.GetUIWord(UIWORD.Next), false);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.AppendSpaceText(1, false);
						_guiContent_AnimKeyframeProp_PrevKeyLabel.AppendText(keyframe._nextLinkedKeyframe._frameIndex, true);
					}
					else
					{
						_guiContent_AnimKeyframeProp_PrevKeyLabel.ClearText(true);
					}

					//colorLabel_Prev = Color.red;
					isColorLabelRed_Prev = true;
				}



				if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Prev), _animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Prev, curveBtnSize, 30))//"Prev"
				{
					_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Prev;
				}
				if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Next), _animPropertyCurveUI == ANIM_SINGLE_PROPERTY_CURVE_UI.Next, curveBtnSize, 30))//"Next"
				{
					_animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Next;
				}
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Linear), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Linear, true, curveTypeBtnSize, 30,
												apStringFactory.I.AnimCurveTooltip_Linear))//"Linear Curve"
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
														Editor, 
														_portrait, 
														//curveResult,
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Linear);
				}
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Smooth), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth, true, curveTypeBtnSize, 30,
												apStringFactory.I.AnimCurveTooltip_Smooth))//"Smooth Curve"
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
														Editor, 
														_portrait, 
														//curveResult, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Smooth);
				}
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Stepped), curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Constant, true, curveTypeBtnSize, 30,
												apStringFactory.I.AnimCurveTooltip_Constant))//"Constant Curve"
				{
					apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
														Editor, 
														_portrait, 
														//curveResult, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

					curveResult.SetTangent(apAnimCurve.TANGENT_TYPE.Constant);
				}



				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);

				if (isSameKP)
				{

					if (curveA == null || curveB == null)
					{
						EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.KeyframeIsNotLinked));//"Keyframe is not linked"
					}
					else
					{

						int curveUI_Width = width - 1;
						int curveUI_Height = 200;
						prevColor = GUI.backgroundColor;

						Rect lastRect = GUILayoutUtility.GetLastRect();

						if (EditorGUIUtility.isProSkin)
						{
							GUI.backgroundColor = new Color(Editor._guiMainEditorColor.r * 0.8f,
													Editor._guiMainEditorColor.g * 0.8f,
													Editor._guiMainEditorColor.b * 0.8f,
													1.0f);
						}
						else
						{
							GUI.backgroundColor = Editor._guiMainEditorColor;
						}

						Rect curveRect = new Rect(lastRect.x + 5, lastRect.y, curveUI_Width, curveUI_Height);

						curveUI_Width -= 2;
						curveUI_Height -= 4;

						//int layoutY_Clip = layoutY - Mathf.Min(scrollValue, 115);
						//int layoutY_Clip = layoutY - Mathf.Clamp(scrollValue, 0, 115);
						//int layoutY_Clip = (layoutY - (scrollValue + (115 - scrollValue));//scrollValue > 115
						//int clipPosY = 115 - scrollValue;


						//Debug.Log("Lyout Y / layoutY : " + layoutY + " / scrollValue : " + scrollValue + " => " + layoutY_Clip);
						apAnimCurveGL.SetLayoutSize(
							curveUI_Width,
							curveUI_Height,
							(int)(lastRect.x) + layoutX - (curveUI_Width + 10),
							//(int)(lastRect.y) + layoutY_Clip,
							(int)(lastRect.y) + layoutY,
							scrollValue,
							Mathf.Min(scrollValue, 115),
							windowWidth, windowHeight);

						bool isLeftBtnPressed = false;
						if (Event.current.rawType == EventType.MouseDown ||
							Event.current.rawType == EventType.MouseDrag)
						{
							if (Event.current.button == 0)
							{ isLeftBtnPressed = true; }
						}

						//apAnimCurveGL.SetMouseValue(isLeftBtnPressed, apMouse.PosNotBound, Event.current.rawType, this);//이전
						apAnimCurveGL.SetMouseValue(isLeftBtnPressed, Editor.Mouse.PosNotBound, Event.current.rawType, this);

						GUI.Box(curveRect, apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box);
						//EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.Height(curveUI_Height));
						GUILayout.BeginArea(new Rect(lastRect.x + 8, lastRect.y + 124, curveUI_Width - 2, curveUI_Height - 2));

						Color curveGraphColorA = Color.black;
						Color curveGraphColorB = Color.black;

						if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Linear)
						{
							curveGraphColorA = new Color(1.0f, 0.1f, 0.1f, 1.0f);
							curveGraphColorB = new Color(1.0f, 1.0f, 0.1f, 1.0f);
						}
						else if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
						{
							curveGraphColorA = new Color(0.2f, 0.2f, 1.0f, 1.0f);
							curveGraphColorB = new Color(0.2f, 1.0f, 1.0f, 1.0f);
						}
						else
						{
							curveGraphColorA = new Color(0.2f, 1.0f, 0.1f, 1.0f);
							curveGraphColorB = new Color(0.1f, 1.0f, 0.6f, 1.0f);
						}


						apAnimCurveGL.DrawCurve(curveA, curveB, curveResult, curveGraphColorA, curveGraphColorB);


						GUILayout.EndArea();
						//EditorGUILayout.EndVertical();



						//GUILayout.Space(10);

						GUI.backgroundColor = prevColor;


						GUILayout.Space(curveUI_Height - 2);


						EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

						//삭제 19.11.22
						//GUIStyle guiStyle_FrameLabel_Prev = new GUIStyle(GUI.skin.label);
						//GUIStyle guiStyle_FrameLabel_Next = new GUIStyle(GUI.skin.label);
						//guiStyle_FrameLabel_Next.alignment = TextAnchor.MiddleRight;

						//guiStyle_FrameLabel_Prev.normal.textColor = colorLabel_Prev;
						//guiStyle_FrameLabel_Next.normal.textColor = colorLabel_Next;




						GUILayout.Space(5);
						//EditorGUILayout.LabelField(strPrevKey, guiStyle_FrameLabel_Prev, GUILayout.Width(width / 2 - 4));
						EditorGUILayout.LabelField(_guiContent_AnimKeyframeProp_PrevKeyLabel.Content,
							(isColorLabelRed_Prev ? apGUIStyleWrapper.I.Label_MiddleLeft_RedColor : apGUIStyleWrapper.I.Label_MiddleLeft_BlackColor),
							apGUILOFactory.I.Width(width / 2 - 4));

						//EditorGUILayout.LabelField(strNextKey, guiStyle_FrameLabel_Next, GUILayout.Width(width / 2 - 4));
						EditorGUILayout.LabelField(_guiContent_AnimKeyframeProp_NextKeyLabel.Content,
							(isColorLabelRed_Next ? apGUIStyleWrapper.I.Label_MiddleRight_RedColor : apGUIStyleWrapper.I.Label_MiddleRight_BlackColor),
							apGUILOFactory.I.Width(width / 2 - 4));

						EditorGUILayout.EndHorizontal();

						if (curveResult.CurveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
						{
							GUILayout.Space(5);

							EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
							GUILayout.Space(5);

							int smoothPresetBtnWidth = ((width - 10) / 4) - 1;
							if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Default), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
							{
								//커브 프리셋 : 기본
								apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	_portrait, 
																	//_animClip._targetMeshGroup, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								curveResult.SetCurvePreset_Default();
							}
							if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Hard), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
							{
								//커브 프리셋 : 하드
								apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	_portrait, 
																	//_animClip._targetMeshGroup, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								curveResult.SetCurvePreset_Hard();
							}
							if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Acc), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
							{
								//커브 프리셋 : 가속 (느리다가 빠르게)
								apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	_portrait, 
																	//_animClip._targetMeshGroup, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								curveResult.SetCurvePreset_Acc();
							}
							if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Dec), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
							{
								//커브 프리셋 : 감속 (빠르다가 느리게)
								apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	_portrait, 
																	//_animClip._targetMeshGroup, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

								curveResult.SetCurvePreset_Dec();
							}

							EditorGUILayout.EndHorizontal();

							if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetSmoothSetting), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))//"Reset Smooth Setting"
							{
								//Curve는 Anim 고유의 값이다. -> Portrait
								apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	_portrait, 
																	//_animClip._targetMeshGroup, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
								curveResult.ResetSmoothSetting();

								Editor.SetRepaint();
								//Editor.Repaint();
							}
						}
						GUILayout.Space(5);
						if (GUILayout.Button(Editor.GetUIWord(UIWORD.CopyCurveToAllKeyframes), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))//"Copy Curve to All Keyframes"
						{

							Editor.Controller.CopyAnimCurveToAllKeyframes(curveResult, keyframe._parentTimelineLayer, keyframe._parentTimelineLayer._parentAnimClip);
							Editor.SetRepaint();
						}

					}
				}
			}



			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			if (isSameKP)
			{
				//복사 / 붙여넣기 / 삭제 // (복붙은 모든 타입에서 등장한다)
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
				GUILayout.Space(5);
				//int editBtnWidth = ((width) / 2) - 3;
				int editBtnWidth_Copy = 80;
				int editBtnWidth_Paste = width - (80 + 4);
				//if (GUILayout.Button(new GUIContent(" Copy", Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy)), GUILayout.Width(editBtnWidth), GUILayout.Height(25)))

				//string strCopy = " " + Editor.GetUIWord(UIWORD.Copy);

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Copy), 1, Editor.GetUIWord(UIWORD.Copy), Editor.GetUIWord(UIWORD.Copy), false, true, editBtnWidth_Copy, 25))//" Copy"
				{
					//Debug.LogError("TODO : Copy Keyframe");
					if (keyframe != null)
					{
						string copyName = "";
						if (keyframe._parentTimelineLayer != null)
						{
							copyName += keyframe._parentTimelineLayer.DisplayName + " ";
						}
						copyName += "[ " + keyframe._frameIndex + " ]";
						apSnapShotManager.I.Copy_Keyframe(keyframe, copyName);
					}
				}

				string pasteKeyName = apSnapShotManager.I.GetClipboardName_Keyframe();
				bool isPastable = apSnapShotManager.I.IsPastable(keyframe);
				if (string.IsNullOrEmpty(pasteKeyName) || !isPastable)
				{
					//pasteKeyName = "Paste";
					pasteKeyName = Editor.GetUIWord(UIWORD.Paste);
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Edit_Paste), 1, pasteKeyName, pasteKeyName, false, isPastable, editBtnWidth_Paste, 25))
				{
					if (keyframe != null)
					{
						//붙여넣기
						//Anim (portrait) + Keyframe+LinkedMod (Modifier = nullable)
						apEditorUtil.SetRecord_PortraitModifier(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																	Editor, 
																	_portrait, 
																	keyframe._parentTimelineLayer._parentTimeline._linkedModifier, 
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);

						apSnapShotManager.I.Paste_Keyframe(keyframe);
						
						//이전
						//RefreshAnimEditing(true);

						//변경 22.5.15
						AutoRefreshModifierExclusiveEditing();
					}
				}
				EditorGUILayout.EndHorizontal();


				//Pose Export / Import
				if (keyframe._parentTimelineLayer._linkedBone != null
					&& keyframe._parentTimelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
					)
				{
					//Bone 타입인 경우
					//Pose 복사 / 붙여넣기를 할 수 있다.

					GUILayout.Space(5);
					EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PoseExportImportLabel), apGUILOFactory.I.Width(width));//"Pose Export / Import"

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
					GUILayout.Space(5);

					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), 1, Editor.GetUIWord(UIWORD.Export), Editor.GetUIWord(UIWORD.Export), false, true, ((width) / 2) - 2, 25))
					{
						if (keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup != null)
						{
							apDialog_RetargetSinglePoseExport.ShowDialog(Editor, keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup, keyframe._parentTimelineLayer._linkedBone);
						}
					}
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones), 1, Editor.GetUIWord(UIWORD.Import), Editor.GetUIWord(UIWORD.Import), false, true, ((width) / 2) - 2, 25))
					{
						if (keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup != null)
						{
							_loadKey_SinglePoseImport_Anim = apDialog_RetargetSinglePoseImport.ShowDialog(
								OnRetargetSinglePoseImportAnim, Editor,
								keyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup,
								keyframe._parentTimelineLayer._parentAnimClip,
								keyframe._parentTimelineLayer._parentTimeline,
								keyframe._parentTimelineLayer._parentAnimClip.CurFrame
								);
						}
					}
					EditorGUILayout.EndHorizontal();
				}


				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				//삭제 단축키 이벤트를 넣자
				//Editor.AddHotKeyEvent(OnHotKeyRemoveKeyframes, apHotKey.LabelText.RemoveKeyframe, KeyCode.Delete, false, false, false, keyframe);//"Remove Keyframe"
				//변경 20.12.3
				Editor.AddHotKeyEvent(OnHotKeyRemoveKeyframes, apHotKeyMapping.KEY_TYPE.Anim_RemoveKeyframes, keyframe);//"Remove Keyframe"



				//키 삭제
				//"Remove Keyframe"
				//이전
				//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWord(UIWORD.RemoveKeyframe),
				//									Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
				//									),
				//					GUILayout.Width(width), GUILayout.Height(24)))

				//변경
				if (_guiContent_Bottom_Animation_RemoveKeyframes == null)
				{
					_guiContent_Bottom_Animation_RemoveKeyframes = apGUIContentWrapper.Make(2, Editor.GetUIWord(UIWORD.RemoveKeyframe), Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
				}

				if (GUILayout.Button(_guiContent_Bottom_Animation_RemoveKeyframes.Content,
									apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(24)))
				{

					bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveKeyframe1_Title),
																Editor.GetText(TEXT.RemoveKeyframe1_Body),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);
					if (isResult)
					{
						Editor.Controller.RemoveKeyframe(keyframe);
					}
				}


			}


			//단축키 추가 20.12.4 : 단축키로 애니메이션 커브의 값을 변경할 수 있다.
			//자동으로 커브 탭으로 전환한다.
			//다중 선택시에는 다른 함수를 호출하자
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_SingleKeyframe, apHotKeyMapping.KEY_TYPE.Anim_Curve_Linear, 0);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_SingleKeyframe, apHotKeyMapping.KEY_TYPE.Anim_Curve_Constant, 1);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_SingleKeyframe, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_Default, 2);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_SingleKeyframe, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_AccAndDec, 3);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_SingleKeyframe, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_Acc, 4);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_SingleKeyframe, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_Dec, 5);

		}


		/// <summary>
		/// 애니메이션 우측 하단 UI : 키프레임을 "여러개 선택할 때" 출력되는 UI
		/// </summary>
		private void DrawEditor_Bottom_AnimationProperty_MultipleKeyframes(List<apAnimKeyframe> keyframes, int width, int windowWidth, int windowHeight, int layoutX, int layoutY, int scrollValue)
		{
			//추가 3.30 : 공통 커브 설정 기능

			//keyframes.Count + " Keyframes Selected"
			Color prevColor = GUI.backgroundColor;

			//GUILayout.Space(10);
			//apEditorUtil.GUI_DelimeterBoxH(width);
			//GUILayout.Space(10);
			//GUILayout.Space(5);

			//추가 19.12.31 : 어느 커브를 설정할지 선택하여 편집할 수 있음
			int curvePosBtnSize = ((width - 10) / 3);
			EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(20));
			GUILayout.Space(5);
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Prev), _animPropertyCurveUI_Multi == ANIM_MULTI_PROPERTY_CURVE_UI.Prev, curvePosBtnSize, 20))//"Prev"
			{
				_animPropertyCurveUI_Multi = ANIM_MULTI_PROPERTY_CURVE_UI.Prev;
			}
			//"Between"
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Between), _animPropertyCurveUI_Multi == ANIM_MULTI_PROPERTY_CURVE_UI.Middle, curvePosBtnSize, 20))//"Mid"
			{
				_animPropertyCurveUI_Multi = ANIM_MULTI_PROPERTY_CURVE_UI.Middle;
			}
			if (apEditorUtil.ToggledButton(Editor.GetUIWord(UIWORD.Next), _animPropertyCurveUI_Multi == ANIM_MULTI_PROPERTY_CURVE_UI.Next, curvePosBtnSize, 20))//"Prev"
			{
				_animPropertyCurveUI_Multi = ANIM_MULTI_PROPERTY_CURVE_UI.Next;
			}
			EditorGUILayout.EndHorizontal();


			int iCurveType = 0;
			switch (_animPropertyCurveUI_Multi)
			{
				case ANIM_MULTI_PROPERTY_CURVE_UI.Prev: iCurveType = 0; break;
				case ANIM_MULTI_PROPERTY_CURVE_UI.Middle: iCurveType = 1; break;
				case ANIM_MULTI_PROPERTY_CURVE_UI.Next: iCurveType = 2; break;
			}
			apTimelineCommonCurve.SYNC_STATUS curveSync = _animTimelineCommonCurve.GetSyncStatus(iCurveType);

			bool isCurves_NoKey = (curveSync == apTimelineCommonCurve.SYNC_STATUS.NoKeyframes);
			bool isCurves_Sync = (curveSync == apTimelineCommonCurve.SYNC_STATUS.Sync);
			bool isCurves_NotSync = (curveSync == apTimelineCommonCurve.SYNC_STATUS.NotSync);

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimProperty_MultipleCurve__NoKey, isCurves_NoKey);
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimProperty_MultipleCurve__Sync, isCurves_Sync);//AnimProperty_MultipleCurve : Sync
			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.AnimProperty_MultipleCurve__NotSync, isCurves_NotSync);//"AnimProperty_MultipleCurve : NotSync"

			//3가지 경우가 있다.
			//1) 편집할 수 있는 커브가 없다.
			//2) 동기화된 상태여서 편집이 가능하다.
			//3) 동기화되지 않았다.

			if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimProperty_MultipleCurve__NoKey))
			{
				//1) 편집할 수 있는 커브가 없다.
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.NoSelectedCurveEdit));//"There is no selected curve to edit."
				GUILayout.Space(5);
			}
			else if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimProperty_MultipleCurve__Sync))//"AnimProperty_MultipleCurve : Sync"
			{
				//2) 동기화된 상태여서 편집이 가능하다.
				//동시에 여러개의 "동기화된" 커브를 조작할 수 있다.

				apAnimCurve.TANGENT_TYPE curveTangentType = apAnimCurve.TANGENT_TYPE.Smooth;
				if (isCurves_Sync)
				{
					curveTangentType = _animTimelineCommonCurve.GetSyncCurveResult(iCurveType).CurveTangentType;
				}

				//추가 19.12.31 : 1. 커브 위치
				int curveTypeBtnSize = ((width - 10) / 3);

				//1. 커브 종류
				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Linear), curveTangentType == apAnimCurve.TANGENT_TYPE.Linear, true, curveTypeBtnSize, 25,
												apStringFactory.I.AnimCurveTooltip_Linear))//"Linear Curve"
				{
					if (isCurves_Sync)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
															Editor, 
															_portrait, 
															//_animTimelineCommonCurve, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Linear, iCurveType);
					}
				}
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Smooth), curveTangentType == apAnimCurve.TANGENT_TYPE.Smooth, true, curveTypeBtnSize, 25,
												apStringFactory.I.AnimCurveTooltip_Smooth))//"Smooth Curve"
				{
					if (isCurves_Sync)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
															Editor, 
															_portrait, 
															//_animTimelineCommonCurve, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Smooth, iCurveType);
					}
				}
				if (apEditorUtil.ToggledButton(Editor.ImageSet.Get(apImageSet.PRESET.Curve_Stepped), curveTangentType == apAnimCurve.TANGENT_TYPE.Constant, true, curveTypeBtnSize, 26,
												apStringFactory.I.AnimCurveTooltip_Constant))//"Constant Curve"
				{
					if (isCurves_Sync)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
															Editor, 
															_portrait, 
															//_animTimelineCommonCurve, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_animTimelineCommonCurve.SetTangentType(apAnimCurve.TANGENT_TYPE.Constant, iCurveType);
					}
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(5);

				//2. 커브를 그리자

				int curveUI_Width = width - 1;
				int curveUI_Height = 200;
				prevColor = GUI.backgroundColor;

				Rect lastRect = GUILayoutUtility.GetLastRect();

				if (EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = new Color(Editor._guiMainEditorColor.r * 0.8f,
														Editor._guiMainEditorColor.g * 0.8f,
														Editor._guiMainEditorColor.b * 0.8f,
														1.0f);
				}
				else
				{
					GUI.backgroundColor = Editor._guiMainEditorColor;
				}

				Rect curveRect = new Rect(lastRect.x + 5, lastRect.y, curveUI_Width, curveUI_Height);

				curveUI_Width -= 2;
				curveUI_Height -= 4;


				apAnimCurveGL.SetLayoutSize(curveUI_Width, curveUI_Height,
												(int)(lastRect.x) + layoutX - (curveUI_Width + 10),
												(int)(lastRect.y) + layoutY,
												//(int)(lastRect.y) + layoutY - 25,
												scrollValue,
												//Mathf.Min(scrollValue, 115),
												//Mathf.Min(scrollValue, 115 - 57),
												//Mathf.Min(scrollValue, 115 - 80),
												Mathf.Min(scrollValue, (115 - (57 - 27))),
												windowWidth, windowHeight);

				bool isLeftBtnPressed = false;
				if (Event.current.rawType == EventType.MouseDown || Event.current.rawType == EventType.MouseDrag)
				{
					if (Event.current.button == 0)
					{
						isLeftBtnPressed = true;
					}
				}

				apAnimCurveGL.SetMouseValue(isLeftBtnPressed, Editor.Mouse.PosNotBound, Event.current.rawType, this);
				GUI.Box(curveRect, apStringFactory.I.None, apEditorUtil.WhiteGUIStyle_Box);

				//GUILayout.BeginArea(new Rect(lastRect.x + 8, lastRect.y + 124, curveUI_Width - 2, curveUI_Height - 2));
				//GUILayout.BeginArea(new Rect(lastRect.x + 8, lastRect.y + 68, curveUI_Width - 2, curveUI_Height - 2));
				GUILayout.BeginArea(new Rect(lastRect.x + 8, lastRect.y + 68 + 26, curveUI_Width - 2, curveUI_Height - 2));

				Color curveGraphColorA = Color.black;
				Color curveGraphColorB = Color.black;

				if (curveTangentType == apAnimCurve.TANGENT_TYPE.Linear)
				{
					curveGraphColorA = new Color(1.0f, 0.1f, 0.1f, 1.0f);
					curveGraphColorB = new Color(1.0f, 1.0f, 0.1f, 1.0f);
				}
				else if (curveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
				{
					curveGraphColorA = new Color(0.2f, 0.2f, 1.0f, 1.0f);
					curveGraphColorB = new Color(0.2f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					curveGraphColorA = new Color(0.2f, 1.0f, 0.1f, 1.0f);
					curveGraphColorB = new Color(0.1f, 1.0f, 0.6f, 1.0f);
				}

				//Curve 값 변경시..
				//이전
				//bool isCurveChanged = apAnimCurveGL.DrawCurve(_animTimelineCommonCurve._syncCurve_Prev,
				//						_animTimelineCommonCurve._syncCurve_Next,
				//						_animTimelineCommonCurve.SyncCurveResult,
				//						curveGraphColorA, curveGraphColorB);

				//변경 19.12.31
				bool isCurveChanged = apAnimCurveGL.DrawCurve(_animTimelineCommonCurve.GetSyncCurve_Prev(iCurveType),
										_animTimelineCommonCurve.GetSyncCurve_Next(iCurveType),
										_animTimelineCommonCurve.GetSyncCurveResult(iCurveType),
										curveGraphColorA, curveGraphColorB);

				if (isCurves_Sync)
				{
					if (isCurveChanged)
					{
						//커브가 바뀌었음을 알리자
						_animTimelineCommonCurve.SetChanged();
					}

					//마우스 입력에 따른 Sync
					//_animTimelineCommonCurve.ApplySync(false, isLeftBtnPressed);
					_animTimelineCommonCurve.ApplySync(iCurveType, false, isLeftBtnPressed);
				}


				//추가 21.5.19 : Curve 렌더링이 모두 끝났다.
				apAnimCurveGL.EndPass();


				GUILayout.EndArea();

				GUI.backgroundColor = prevColor;

				GUILayout.Space(curveUI_Height - 2);

				if (curveTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
				{
					GUILayout.Space(5);

					EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
					GUILayout.Space(5);

					int smoothPresetBtnWidth = ((width - 10) / 4) - 1;
					if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Default), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
					{
						//커브 프리셋 : 기본
						if (isCurves_Sync)
						{
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																Editor, 
																_portrait, 
																//_animClip._targetMeshGroup, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_animTimelineCommonCurve.SetCurvePreset_Default(iCurveType);
						}
					}
					if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Hard), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
					{
						//커브 프리셋 : 하드
						if (isCurves_Sync)
						{
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																Editor, 
																_portrait, 
																//_animClip._targetMeshGroup, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_animTimelineCommonCurve.SetCurvePreset_Hard(iCurveType);
						}
					}
					if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Acc), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
					{
						//커브 프리셋 : 가속 (느리다가 빠르게)
						if (isCurves_Sync)
						{
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																Editor, 
																_portrait, 
																//_animClip._targetMeshGroup, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_animTimelineCommonCurve.SetCurvePreset_Acc(iCurveType);
						}
					}
					if (GUILayout.Button(Editor.ImageSet.Get(apImageSet.PRESET.Anim_CurvePreset_Dec), apGUILOFactory.I.Width(smoothPresetBtnWidth), apGUILOFactory.I.Height(28)))
					{
						//커브 프리셋 : 감속 (빠르다가 느리게)
						if (isCurves_Sync)
						{
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																Editor, 
																_portrait, 
																//_animClip._targetMeshGroup, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_animTimelineCommonCurve.SetCurvePreset_Dec(iCurveType);
						}
					}

					EditorGUILayout.EndHorizontal();

					if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetSmoothSetting), apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(25)))//"Reset Smooth Setting"
					{
						if (isCurves_Sync)
						{
							apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
																Editor, 
																_portrait, 
																//_animClip._targetMeshGroup, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							_animTimelineCommonCurve.ResetSmoothSetting(iCurveType);
						}
						Editor.SetRepaint();
					}
				}



			}
			else if (Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.AnimProperty_MultipleCurve__NotSync))//"AnimProperty_MultipleCurve : NotSync"
			{
				//"Curves of keyframes are different"
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.CurvesAreDifferent));
				GUILayout.Space(5);
				//아직 커브들이 "동기화"되지 않았다.
				//"Reset curves of all selected keyframes"
				if (GUILayout.Button(Editor.GetUIWord(UIWORD.ResetMultipleCurves), apGUILOFactory.I.Height(25)))
				{
					if (isCurves_NotSync)
					{
						apEditorUtil.SetRecord_Portrait(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
															Editor, 
															_portrait, 
															//_animTimelineCommonCurve, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						_animTimelineCommonCurve.NotSync2SyncStatus(iCurveType);
					}
				}
			}
			else
			{
				//현재는 커브 조작을 할 수 없다.
			}





			// 키프레임들 삭제하기
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			//삭제 단축키 이벤트를 넣자
			//Editor.AddHotKeyEvent(OnHotKeyRemoveKeyframes, apHotKey.LabelText.RemoveKeyframes, KeyCode.Delete, false, false, false, keyframes);//"Remove Keyframes"
			
			//변경 20.12.3
			Editor.AddHotKeyEvent(OnHotKeyRemoveKeyframes, apHotKeyMapping.KEY_TYPE.Anim_RemoveKeyframes, keyframes);//"Remove Keyframes"


			//키 삭제
			//"  Remove " + keyframes.Count +" Keyframes"

			//이전
			//if (GUILayout.Button(new GUIContent("  " + Editor.GetUIWordFormat(UIWORD.RemoveNumKeyframes, keyframes.Count),
			//										Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform)
			//										),
			//						GUILayout.Width(width), GUILayout.Height(24)))
			if (_guiContent_Bottom_Animation_RemoveNumKeyframes == null)
			{
				_guiContent_Bottom_Animation_RemoveNumKeyframes = new apGUIContentWrapper();
				_guiContent_Bottom_Animation_RemoveNumKeyframes.SetImage(Editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_RemoveTransform));
			}

			_guiContent_Bottom_Animation_RemoveNumKeyframes.SetText(2, Editor.GetUIWordFormat(UIWORD.RemoveNumKeyframes, keyframes.Count));

			if (GUILayout.Button(_guiContent_Bottom_Animation_RemoveNumKeyframes.Content, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(24)))
			{
				//bool isResult = EditorUtility.DisplayDialog("Remove Keyframes", "Remove " + keyframes.Count + "s Keyframes?", "Remove", "Cancel");

				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveKeyframes_Title),
																Editor.GetTextFormat(TEXT.RemoveKeyframes_Body, keyframes.Count),
																Editor.GetText(TEXT.Remove),
																Editor.GetText(TEXT.Cancel)
																);

				if (isResult)
				{
					Editor.Controller.RemoveKeyframes(keyframes);
				}
			}


			//단축키 추가 20.12.5 : 단축키를 눌러서 커브를 조작할 수 있다.
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_MultipleKeyframes, apHotKeyMapping.KEY_TYPE.Anim_Curve_Linear, 0);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_MultipleKeyframes, apHotKeyMapping.KEY_TYPE.Anim_Curve_Constant, 1);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_MultipleKeyframes, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_Default, 2);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_MultipleKeyframes, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_AccAndDec, 3);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_MultipleKeyframes, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_Acc, 4);
			Editor.AddHotKeyEvent(OnHotKeyEvent_AnimCurve_MultipleKeyframes, apHotKeyMapping.KEY_TYPE.Anim_Curve_Smooth_Dec, 5);
		}




		
		/// <summary>
		/// 애니메이션 우측 하단 UI : 단일 타임라인 레이어 속성
		/// </summary>
		/// <param name="timelineLayer"></param>
		/// <param name="width"></param>
		private void DrawEditor_Bottom_AnimationProperty_TimelineLayer(apAnimTimelineLayer timelineLayer, int width)
		{
			//EditorGUILayout.LabelField("Timeline Layer");
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TimelineLayer));

			GUILayout.Space(10);
			if (timelineLayer._targetParamSetGroup != null &&
				timelineLayer._parentTimeline != null &&
				timelineLayer._parentTimeline._linkedModifier != null
				)
			{
				apModifierParamSetGroup keyParamSetGroup = timelineLayer._targetParamSetGroup;
				apModifierBase modifier = timelineLayer._parentTimeline._linkedModifier;
				//apAnimTimeline timeline = timelineLayer._parentTimeline;

				//이름
				//설정
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = timelineLayer._guiColor;
				//GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				//guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				//guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box(timelineLayer.DisplayName, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);

				//1. 색상 Modifier라면 색상 옵션을 설정한다.
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{
					//" Color Option On", " Color Option Off",
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
															1, Editor.GetUIWord(UIWORD.ColorOptionOn), Editor.GetUIWord(UIWORD.ColorOptionOff),
															keyParamSetGroup._isColorPropertyEnabled, true,
															width, 24))
					{
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
															Editor, 
															modifier, 
															//_animClip._targetMeshGroup, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						keyParamSetGroup._isColorPropertyEnabled = !keyParamSetGroup._isColorPropertyEnabled;

						_animClip._targetMeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy(false);
					}

					//추가 : Color Option이 가능하면 Extra 옵션도 가능하다.
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ExtraOption),
													1, Editor.GetUIWord(UIWORD.ExtraOptionON), Editor.GetUIWord(UIWORD.ExtraOptionOFF),
													modifier._isExtraPropertyEnabled, true, width, 20))
					{
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
															Editor, 
															modifier, 
															//_animClip._targetMeshGroup, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						modifier._isExtraPropertyEnabled = !modifier._isExtraPropertyEnabled;
						_animClip._targetMeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy(false);
					}
					GUILayout.Space(10);
				}
			}

			//2. GUI Color를 설정
			try
			{
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.LayerGUIColor));//"Layer GUI Color"
				Color nextGUIColor = EditorGUILayout.ColorField(timelineLayer._guiColor, apGUILOFactory.I.Width(width));
				if (nextGUIColor != timelineLayer._guiColor)
				{
					apEditorUtil.SetEditorDirty();
					timelineLayer._guiColor = nextGUIColor;
				}
			}
			catch (Exception) { }

			GUILayout.Space(10);

			Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom_Right_Anim_Property__BoneLayer, 
									timelineLayer._linkedBone != null
									&& timelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier);

			bool isBonePosePropertyVisible = Editor.IsDelayedGUIVisible(apEditor.DELAYED_UI_TYPE.Bottom_Right_Anim_Property__BoneLayer);

			//Pose Export / Import
			//이전
			//if (timelineLayer._linkedBone != null
			//	&& timelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
			//	)
			//변경 20.7.3
			if (isBonePosePropertyVisible)
			{
				//Bone 타입인 경우
				//Pose 복사 / 붙여넣기를 할 수 있다.

				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PoseExportImportLabel), apGUILOFactory.I.Width(width));//"Pose Export / Import"

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
				GUILayout.Space(5);

				//string strExport = " " + Editor.GetUIWord(UIWORD.Export);
				//string strImport = " " + Editor.GetUIWord(UIWORD.Import);

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), 1, Editor.GetUIWord(UIWORD.Export), Editor.GetUIWord(UIWORD.Export), false, true, ((width) / 2) - 2, 25))
				{
					if (timelineLayer._parentAnimClip._targetMeshGroup != null)
					{
						apDialog_RetargetSinglePoseExport.ShowDialog(Editor, timelineLayer._parentAnimClip._targetMeshGroup, timelineLayer._linkedBone);
					}
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones), 1, Editor.GetUIWord(UIWORD.Import), Editor.GetUIWord(UIWORD.Import), false, true, ((width) / 2) - 2, 25))
				{
					if (timelineLayer._parentAnimClip._targetMeshGroup != null)
					{
						_loadKey_SinglePoseImport_Anim = apDialog_RetargetSinglePoseImport.ShowDialog(
							OnRetargetSinglePoseImportAnim, Editor,
							timelineLayer._parentAnimClip._targetMeshGroup,
							timelineLayer._parentAnimClip,
							timelineLayer._parentTimeline,
							timelineLayer._parentAnimClip.CurFrame
							);
					}
				}
				EditorGUILayout.EndHorizontal();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
		}



		


		/// <summary>
		/// 애니메이션 우측 하단 UI : 여러개의 타임라인 레이어 속성
		/// </summary>
		/// <param name="timelineLayers"></param>
		/// <param name="width"></param>
		private void DrawEditor_Bottom_AnimationProperty_MultipleTimelineLayers(List<apAnimTimelineLayer> timelineLayers, int width)
		{
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.TimelineLayers));
			GUILayout.Space(10);
			
			//일괄 편집을 하기 위해 동기화를 하자
			//> 할 수 있는게 GUI Color Option밖에 없는데염..
			_subObjects.Check_TimelineLayer_GUIColor();
			Color syncColor = _subObjects.Sync0.SyncValue_Color;
			try
			{
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.LayerGUIColor));//"Layer GUI Color"
				Color nextGUIColor = EditorGUILayout.ColorField(syncColor, apGUILOFactory.I.Width(width));
				if (nextGUIColor != syncColor)
				{
					apEditorUtil.SetEditorDirty();
					_subObjects.Set_TimelineLayer_GUIColor(nextGUIColor);
				}
			}
			catch (Exception) { }




			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
		}


		/// <summary>
		/// 애니메이션 우측 하단 UI : 타임라인 속성
		/// </summary>
		/// <param name="timeline"></param>
		/// <param name="width"></param>
		private void DrawEditor_Bottom_AnimationProperty_Timeline(apAnimTimeline timeline, int width)
		{
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Timeline));//"Timeline"

			GUILayout.Space(10);

			if (timeline._linkedModifier != null
				)
			{
				apModifierBase modifier = timeline._linkedModifier;


				//이름
				//설정
				Color prevColor = GUI.backgroundColor;

				GUI.backgroundColor = timeline._guiColor;
				//GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				//guiStyle_Box.alignment = TextAnchor.MiddleCenter;
				//guiStyle_Box.normal.textColor = apEditorUtil.BoxTextColor;

				GUILayout.Box(timeline.DisplayName, apGUIStyleWrapper.I.Box_MiddleCenter_BoxTextColor, apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));

				GUI.backgroundColor = prevColor;

				GUILayout.Space(10);

				//" Color Option On", " Color Option Off",
				//1. 색상 Modifier라면 색상 옵션을 설정한다.
				if ((int)(modifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.Color) != 0)
				{
					//추가 21.7.20 : 단, Color Only 모디파이어는 어차피 색상이 항상 true라서 옵션이 필요없다.
					if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.AnimatedColorOnly)
					{
						if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ColorVisibleOption),
																1, Editor.GetUIWord(UIWORD.ColorOptionOn), Editor.GetUIWord(UIWORD.ColorOptionOff),
																modifier._isColorPropertyEnabled, true,
																width, 24))
						{
							apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_KeyframeValueChanged,
																Editor,
																modifier,
																//_animClip._targetMeshGroup, 
																false,
																apEditorUtil.UNDO_STRUCT.ValueOnly);

							modifier._isColorPropertyEnabled = !modifier._isColorPropertyEnabled;
							_animClip._targetMeshGroup.RefreshForce();
							Editor.RefreshControllerAndHierarchy(false);
						}
					}

					//추가 : Color Option이 가능하면 Extra 옵션도 가능하다.
					//"Extra Option On" / "Extra Option Off"
					if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Modifier_ExtraOption),
													1, Editor.GetUIWord(UIWORD.ExtraOptionON), Editor.GetUIWord(UIWORD.ExtraOptionOFF),
													modifier._isExtraPropertyEnabled, true, width, 20))
					{
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_KeyframeValueChanged, 
															Editor, 
															modifier, 
															//_animClip._targetMeshGroup, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);

						modifier._isExtraPropertyEnabled = !modifier._isExtraPropertyEnabled;
						_animClip._targetMeshGroup.RefreshForce();
						Editor.RefreshControllerAndHierarchy(false);
					}
				}
				GUILayout.Space(10);

			}


			//Pose Export / Import
			if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
				&& timeline._linkedModifier != null
				&& timeline._linkedModifier.IsTarget_Bone)
			{
				//Bone 타입인 경우
				//Pose 복사 / 붙여넣기를 할 수 있다.

				GUILayout.Space(5);
				EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.PoseExportImportLabel), apGUILOFactory.I.Width(width));//"Pose Export / Import"

				EditorGUILayout.BeginHorizontal(apGUILOFactory.I.Width(width), apGUILOFactory.I.Height(30));
				GUILayout.Space(5);

				//string strExport = " " + Editor.GetUIWord(UIWORD.Export);
				//string strImport = " " + Editor.GetUIWord(UIWORD.Import);

				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_SaveLoad), 1, Editor.GetUIWord(UIWORD.Export), Editor.GetUIWord(UIWORD.Export), false, true, ((width) / 2) - 2, 25))// " Export"
				{
					if (timeline._parentAnimClip._targetMeshGroup != null)
					{
						apDialog_RetargetSinglePoseExport.ShowDialog(Editor, timeline._parentAnimClip._targetMeshGroup, null);
					}
				}
				if (apEditorUtil.ToggledButton_2Side(Editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones), 1, Editor.GetUIWord(UIWORD.Import), Editor.GetUIWord(UIWORD.Import), false, true, ((width) / 2) - 2, 25))//" Import"
				{
					if (timeline._parentAnimClip._targetMeshGroup != null)
					{
						_loadKey_SinglePoseImport_Anim = apDialog_RetargetSinglePoseImport.ShowDialog(
							OnRetargetSinglePoseImportAnim, Editor,
							timeline._parentAnimClip._targetMeshGroup,
							timeline._parentAnimClip,
							timeline,
							timeline._parentAnimClip.CurFrame
							);
					}
				}
				EditorGUILayout.EndHorizontal();
			}


			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
		}

	}
}