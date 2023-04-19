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
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	public class apDialog_Rotoscoping : EditorWindow
	{
		// Members
		//--------------------------------------------------------
		private static apDialog_Rotoscoping s_window = null;

		private apEditor _editor = null;


		private Vector2 _scroll_Left = Vector2.zero;
		private Vector2 _scroll_Right = Vector2.zero;

		private apRotoscoping.ImageSetData _selectedImageSet = null;

		private GUIStyle _guiStyle_None = null;
		private GUIStyle _guiStyle_Selected = null;
		private GUIStyle _guiStyle_FilePath = null;

		private Texture2D _img_FoldDown = null;
		private Texture2D _img_Rotoscoping = null;
		private Texture2D _img_LayerUp = null;
		private Texture2D _img_LayerDown = null;
		private Texture2D _img_Remove = null;

		// Show Window
		//--------------------------------------------------------------
		public static void ShowDialog(apEditor editor)
		{
			CloseDialog();

			if (editor == null)
			{
				return;
			}

			//Debug.Log("Rotoscoping Setting Open");

			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_Rotoscoping), true, "Rotoscoping", true);
			apDialog_Rotoscoping curTool = curWindow as apDialog_Rotoscoping;

			
			if (curTool != null && curTool != s_window)
			{
				int width = 700;
				int height = 500;
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
			_selectedImageSet = null;

			_img_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);
			_img_Rotoscoping = _editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_Rotoscoping);
			
			_img_LayerUp = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerUp);
			_img_LayerDown = _editor.ImageSet.Get(apImageSet.PRESET.Modifier_LayerDown);
			_img_Remove = _editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey);
			


			if(_editor.Rotoscoping._imageSetDataList != null && 
				_editor.Rotoscoping._imageSetDataList.Count > 0)
			{
				//맨 위의 것 또는 에디터에서 선택중인 데이터를 자동으로 선택하자
				if(_editor._selectedRotoscopingData != null && 
					_editor.Rotoscoping._imageSetDataList.Contains(_editor._selectedRotoscopingData))
				{
					_selectedImageSet = _editor._selectedRotoscopingData;
				}
				else
				{
					_selectedImageSet = _editor.Rotoscoping._imageSetDataList[0];
				}
			}
		}

		// GUI
		//--------------------------------------------------------------------
		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null)
			{
				CloseDialog();
				return;
			}

			//GUI Style 만들기
			if (_guiStyle_None == null || _guiStyle_Selected == null)
			{
				_guiStyle_None = new GUIStyle(GUIStyle.none);
				_guiStyle_Selected = new GUIStyle(GUIStyle.none);

				if (EditorGUIUtility.isProSkin)
				{
					_guiStyle_Selected.normal.textColor = Color.cyan;
					_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
				}
				else
				{
					_guiStyle_Selected.normal.textColor = Color.white;
					_guiStyle_None.normal.textColor = Color.black;
				}
			}
			if(_guiStyle_FilePath == null)
			{
				_guiStyle_FilePath = new GUIStyle(GUI.skin.textField);
				_guiStyle_FilePath.alignment = TextAnchor.MiddleRight;
			}


			Color prevColor = GUI.backgroundColor;

			//왼쪽 : 공통 속성과 ImageSetData 리스트
			//오른쪽 : 선택된 ImageSetData와 속성, 파일 리스트
			int width_Left = (int)((width - 15) * 0.4f);
			int width_Right = (width - 15) - (width_Left + 6);

			int width_Label = 120;
			int width_Value_Left = width_Left - (width_Label + 10 + 4);
			int width_Value_Right = width_Right - (width_Label + 10 + 4);
			int width_Value_Left_Half = (width_Value_Left / 2) - 2;

			int height_LeftScroll = height - (159 + 28);
			int height_RightScroll = height - (170 + 34);

			int height_ListItem = 30;

			bool isAnyChanged = false;

			Color guiColor_Default = GUI.backgroundColor * new Color(0.95f, 0.95f, 0.95f, 1.0f);
			Color guiColor_ListNoEditible = GUI.backgroundColor * new Color(0.4f, 0.4f, 0.4f, 1.0f);
			Color guiColor_NotSelected = GUI.backgroundColor * new Color(0.5f, 0.5f, 0.5f, 1.0f);

			GUI.backgroundColor = guiColor_Default;
			GUI.Box(new Rect(5, 149 - (25 + 23 - 28), width_Left + 1, height_LeftScroll + 2), apStringFactory.I.None);//왼쪽 위 리스트
			GUI.backgroundColor = prevColor;

			GUI.backgroundColor = _selectedImageSet != null ? guiColor_Default : guiColor_ListNoEditible;
			GUI.Box(new Rect(5 + width_Left + 5, 150 - (15 + 23 - 34), width_Right + 1, height_RightScroll + 2), apStringFactory.I.None);//오른쪽 리스트
			GUI.backgroundColor = prevColor;


			height -= 10;
			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left), GUILayout.Height(height));

			//왼쪽 공통 속성
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DisplayOption));//"Display Options"
			GUILayout.Space(5);

			//Offset X, Y
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Position), GUILayout.Width(width_Label));
			int nextPosOffsetX = EditorGUILayout.DelayedIntField(_editor.Rotoscoping._posOffset_X, GUILayout.Width(width_Value_Left_Half));
			int nextPosOffsetY = EditorGUILayout.DelayedIntField(_editor.Rotoscoping._posOffset_Y, GUILayout.Width(width_Value_Left_Half));
			EditorGUILayout.EndHorizontal();

			if(nextPosOffsetX != _editor.Rotoscoping._posOffset_X
				|| nextPosOffsetY != _editor.Rotoscoping._posOffset_Y)
			{
				_editor.Rotoscoping._posOffset_X = nextPosOffsetX;
				_editor.Rotoscoping._posOffset_Y = nextPosOffsetY;
				isAnyChanged = true;
			}

			//Opacity
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.Opacity), GUILayout.Width(width_Label));//"Opacity"
			int prevOpacity = _editor.Rotoscoping._opacity;
			_editor.Rotoscoping._opacity = Mathf.Clamp((int)GUILayout.HorizontalSlider(_editor.Rotoscoping._opacity, 0, 256, GUILayout.Width(width_Value_Left - 54)), 0, 255);
			_editor.Rotoscoping._opacity = EditorGUILayout.DelayedIntField(_editor.Rotoscoping._opacity, GUILayout.Width(50));
			//_editor.Rotoscoping._opacity = EditorGUILayout.IntSlider(_editor.Rotoscoping._opacity, 0, 255, GUILayout.Width(width_Value_Left));
			EditorGUILayout.EndHorizontal();

			if(prevOpacity != _editor.Rotoscoping._opacity)
			{
				_editor.Rotoscoping._opacity = Mathf.Clamp(_editor.Rotoscoping._opacity, 0, 255);
				apEditorUtil.ReleaseGUIFocus();
				isAnyChanged = true;
			}

			//Scale Ratio
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetText(TEXT.ScalePercent), GUILayout.Width(width_Label));//"Scale (%)"
			int prevScale = _editor.Rotoscoping._scaleWithinScreen;
			_editor.Rotoscoping._scaleWithinScreen = Mathf.Clamp((int)GUILayout.HorizontalSlider(_editor.Rotoscoping._scaleWithinScreen, 10, 300, GUILayout.Width(width_Value_Left - 54)), 10, 300);
			_editor.Rotoscoping._scaleWithinScreen = EditorGUILayout.DelayedIntField(_editor.Rotoscoping._scaleWithinScreen, GUILayout.Width(50));

			//int nextScale = EditorGUILayout.IntSlider(_editor.Rotoscoping._scaleWithinScreen, 10, 300, GUILayout.Width(width_Value_Left));
			EditorGUILayout.EndHorizontal();

			if(prevScale != _editor.Rotoscoping._scaleWithinScreen)
			{
				//_editor.Rotoscoping._scaleWithinScreen = nextScale;
				_editor.Rotoscoping._scaleWithinScreen = Mathf.Clamp(_editor.Rotoscoping._scaleWithinScreen, 10, 300);
				apEditorUtil.ReleaseGUIFocus();
				isAnyChanged = true;
			}

			GUILayout.Space(10);
			if(GUILayout.Button(_editor.GetText(TEXT.DLG_Setting_RestoreDefaultSetting), GUILayout.Height(20)))
			{
				//기본 값을 초기화
				_editor.Rotoscoping._posOffset_X = 0;
				_editor.Rotoscoping._posOffset_Y = 0;		
				_editor.Rotoscoping._opacity = 128;//255면 불투명
				_editor.Rotoscoping._scaleWithinScreen = 80;//작업 공간 대비 80% 비율로 들어간다. 세로 기준
				apEditorUtil.ReleaseGUIFocus();
				isAnyChanged = true;
			}
			GUILayout.Space(10);


			// 왼쪽 리스트
			_scroll_Left = EditorGUILayout.BeginScrollView(_scroll_Left, GUILayout.Width(width_Left), GUILayout.Height(height_LeftScroll));
			EditorGUILayout.BeginVertical(GUILayout.Width(width_Left - 24));

			//"Rotoscoping Data List"
			GUILayout.Button(new GUIContent(_editor.GetText(TEXT.RotoscopingDataList), _img_FoldDown), _guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

			int nImageSets = _editor.Rotoscoping._imageSetDataList != null ? _editor.Rotoscoping._imageSetDataList.Count : 0;

			if (nImageSets > 0)
			{
				apRotoscoping.ImageSetData curImageSetData = null;
				for (int iImageSet = 0; iImageSet < _editor.Rotoscoping._imageSetDataList.Count; iImageSet++)
				{
					curImageSetData = _editor.Rotoscoping._imageSetDataList[iImageSet];
					if (DrawImageSetData(curImageSetData, _selectedImageSet == curImageSetData, width_Left - 24, height_ListItem, _scroll_Left.x))
					{
						_selectedImageSet = curImageSetData;
					}
				}
			}

			GUILayout.Space(height);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();

			if(GUILayout.Button(_editor.GetText(TEXT.AddNewRotoscopingData), GUILayout.Height(30)))//"Add New Rotoscoping Data"
			{
				_selectedImageSet = _editor.Rotoscoping.AddNewImageSet();
				isAnyChanged = true;
			}

			EditorGUILayout.EndVertical();

			GUILayout.Space(5);

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right), GUILayout.Height(height));

			//오른쪽 속성과 리스트
			GUILayout.Space(5);
			GUILayout.Label(_editor.GetText(TEXT.RotoscopingDataProperties));//"Rotoscoping Data Properties"
			GUILayout.Space(5);


			//ImageSet 이름
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Name), GUILayout.Width(width_Label));

			if(_selectedImageSet != null)
			{
				string newName = EditorGUILayout.DelayedTextField(_selectedImageSet._name, GUILayout.Width(width_Value_Right));
				if(!string.Equals(newName, _selectedImageSet._name))
				{
					//이름 바꾸고 저장
					_selectedImageSet._name = newName;
					isAnyChanged = true;
				}
			}
			else
			{
				GUI.backgroundColor = guiColor_NotSelected;
				EditorGUILayout.TextField("<None>", GUILayout.Width(width_Value_Right));
				GUI.backgroundColor = prevColor;
			}

			
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);


			//Sync To Animation
			bool isSync = false;
			if(_selectedImageSet != null)
			{
				isSync = _selectedImageSet._isSyncToAnimation;
			}
			//"Sync To Animation"
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.SyncToAnimation), isSync, _selectedImageSet != null, width_Right - 9, 25))
			{
				//Sync 버튼
				if(_selectedImageSet != null)
				{
					_selectedImageSet._isSyncToAnimation = !_selectedImageSet._isSyncToAnimation;
					isAnyChanged = true;
				}
			}

			//Sync - 전환 프레임수 / 오프셋
			int width_Label_FrameOffset = 100;
			//int width_Value_Right = width_Right - (width_Label + 10 + 4);
			int width_Value_FrameOffset = ((width_Right - 15) / 2) - (width_Label_FrameOffset + 4);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			

			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Frame), GUILayout.Width(width_Label_FrameOffset));
			if(_selectedImageSet != null && _selectedImageSet._isSyncToAnimation)
			{
				int nextFramePerSwitch = EditorGUILayout.DelayedIntField(_selectedImageSet._framePerSwitch, GUILayout.Width(width_Value_FrameOffset));
				if(nextFramePerSwitch != _selectedImageSet._framePerSwitch)
				{
					_selectedImageSet._framePerSwitch = nextFramePerSwitch;
					isAnyChanged = true;
				}
			}
			else
			{
				GUI.backgroundColor = guiColor_NotSelected;
				EditorGUILayout.DelayedIntField(5, GUILayout.Width(width_Value_FrameOffset));
				GUI.backgroundColor = prevColor;
			}
			GUILayout.Space(5);
			EditorGUILayout.LabelField(_editor.GetUIWord(UIWORD.Offset), GUILayout.Width(width_Label_FrameOffset));
			if(_selectedImageSet != null && _selectedImageSet._isSyncToAnimation)
			{
				int nextFrameOffset = EditorGUILayout.IntField(_selectedImageSet._frameOffsetToSwitch, GUILayout.Width(width_Value_FrameOffset - 4));
				if(nextFrameOffset != _selectedImageSet._frameOffsetToSwitch)
				{
					_selectedImageSet._frameOffsetToSwitch = nextFrameOffset;
					isAnyChanged = true;
				}
			}
			else
			{
				GUI.backgroundColor = guiColor_NotSelected;
				EditorGUILayout.IntField(0, GUILayout.Width(width_Value_FrameOffset));
				GUI.backgroundColor = prevColor;
			}

			
			EditorGUILayout.EndHorizontal();



			GUILayout.Space(10);		

			//"Remove Data"
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.RemoveRotoscopingData), false, _selectedImageSet != null, width_Right - 9, 20))
			{
				if(_selectedImageSet != null)
				{
					//"Remove Data"
					//"Are you sure you want to remove the selected data? This operation cannot be undone."
					bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveRotoscopingData), 
																_editor.GetText(TEXT.DLG_RemoveRotoscopingData_Body), 
																_editor.GetText(TEXT.Remove), 
																_editor.GetText(TEXT.Cancel));
					if (isResult)
					{
						_editor.Rotoscoping.RemoveImageSet(_selectedImageSet);
						if(_selectedImageSet == _editor._selectedRotoscopingData)
						{
							//현재 선택된게 삭제되었다.
							_editor._selectedRotoscopingData = null;
							_editor._isEnableRotoscoping = false;
						}
						
						_selectedImageSet = null;
						isAnyChanged = true;
					}
				}
			}
			
			GUILayout.Space(10);
			
			
			//오른쪽 리스트
			_scroll_Right = EditorGUILayout.BeginScrollView(_scroll_Right, GUILayout.Width(width_Right), GUILayout.Height(height_RightScroll));

			apRotoscoping.ImageFileData removableImageFileData = null;

			EditorGUILayout.BeginVertical(GUILayout.Width(width_Right - 24));
			
			GUILayout.Button(new GUIContent(_editor.GetUIWord(UIWORD.Images), _img_FoldDown), _guiStyle_None, GUILayout.Height(height_ListItem));//<투명 버튼

			if(_selectedImageSet != null)
			{
				apRotoscoping.ImageFileData curImageFileData = null;
				for (int iImaeFile = 0; iImaeFile < _selectedImageSet._filePathList.Count; iImaeFile++)
				{
					curImageFileData = _selectedImageSet._filePathList[iImaeFile];
					
					IMAGE_FILE_ITEM_RESULT imageFileResult = DrawImageFile(curImageFileData, width_Right - 24, 20, _scroll_Right.x);

					if(imageFileResult != IMAGE_FILE_ITEM_RESULT.None)
					{
						switch (imageFileResult)
						{
							case IMAGE_FILE_ITEM_RESULT.ChangePath:
								{
									string dirPath = "";
									
									if (!string.IsNullOrEmpty(curImageFileData._filePath))//추가 21.9.10 : 빈 경로 체크
									{
										System.IO.FileInfo fi = new System.IO.FileInfo(curImageFileData._filePath);//경로 문제 체크함 (21.9.10)
										if (fi.Exists)
										{
											dirPath = fi.Directory.FullName;
										}
									}
									
									string newPath = EditorUtility.OpenFilePanel("Change Image File", dirPath, "png");
									if (!string.IsNullOrEmpty(newPath))
									{
										//추가 21.7.3 : 이스케이프 문자 삭제
										newPath = apUtil.ConvertEscapeToPlainText(newPath);

										curImageFileData._filePath = newPath;
										isAnyChanged = true;
									}
								}
								break;

							case IMAGE_FILE_ITEM_RESULT.OrderUp:
								{
									_selectedImageSet.ChangeFileOrder(curImageFileData, true);
									isAnyChanged = true;
								}
								break;

							case IMAGE_FILE_ITEM_RESULT.OrderDown:
								{
									_selectedImageSet.ChangeFileOrder(curImageFileData, false);
									isAnyChanged = true;
								}
								break;

							case IMAGE_FILE_ITEM_RESULT.Remove:
								{
									//"Remove Image File"
									//"Are you sure you want to remove the image file from the list?\n(This file is not deleted from the disk.)"
									//"Remove"
									bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.RemoveImageFile), 
																				_editor.GetText(TEXT.DLG_RemoveImageFileFromRotoscoping), 
																				_editor.GetText(TEXT.Remove), 
																				_editor.GetText(TEXT.Cancel));
									if(isResult)
									{
										removableImageFileData = curImageFileData;
									}
									
								}
								break;
						}
					}

					GUILayout.Space(5);
				}
			}	


			GUILayout.Space(height);
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();

			if(_selectedImageSet != null && removableImageFileData != null)
			{
				_selectedImageSet.RemoveImageFile(removableImageFileData);
				isAnyChanged = true;

				//현재 선택된 이미지라면 인덱스 바꿔야함
				if(_editor._selectedRotoscopingData == _selectedImageSet)
				{
					int nImageFiles = _selectedImageSet._filePathList != null ? _selectedImageSet._filePathList.Count : 0;
					if (nImageFiles == 0)
					{
						_editor._iRotoscopingImageFile = 0;
					}
					else
					{
						if (_editor._iRotoscopingImageFile >= nImageFiles)
						{
							_editor._iRotoscopingImageFile = nImageFiles - 1;
						}
					}
				}
			}

			//"Add Image File"
			if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.AddImageFile), false, _selectedImageSet != null, width_Right - 9, 30))
			{
				if (_selectedImageSet != null)
				{
					string path = EditorUtility.OpenFilePanel("Load Image File", apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.Rotoscoping), "png");
					if (!string.IsNullOrEmpty(path))
					{
						//추가 21.7.3 : 이스케이프 문자 삭제
						path = apUtil.ConvertEscapeToPlainText(path);

						_selectedImageSet.AddImageFile(path);
						isAnyChanged = true;

						apEditorUtil.SetLastExternalOpenSaveFilePath(path, apEditorUtil.SAVED_LAST_FILE_PATH.Rotoscoping);
					}
				}
				
			}
			


			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();


			//뭔가 바뀌었다면 저장한다.
			if(isAnyChanged)
			{
				//Debug.Log(">> 저장한다.");
				_editor.Rotoscoping.Save();
			}
		}


		private bool DrawImageSetData(apRotoscoping.ImageSetData imageSetData, bool isSelected, int width, int height, float scrollX)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();

			if (isSelected)
			{
				int yOffset = height;

				#region [미사용 코드]
				//Color prevColor = GUI.backgroundColor;

				//if(EditorGUIUtility.isProSkin)
				//{
				//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				//}
				//else
				//{
				//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
				//}

				//GUI.Box(new Rect(lastRect.x + scrollX, lastRect.y + yOffset , width + 10, height + 3), apStringFactory.I.None);
				//GUI.backgroundColor = prevColor; 
				#endregion

				//변경 v1.4.2
				apEditorUtil.DrawListUnitBG(lastRect.x + scrollX + 1, lastRect.y + yOffset , width + 10 - 2, height + 3, apEditorUtil.UNIT_BG_STYLE.Main);
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			bool isClick = false;


			if(GUILayout.Button(new GUIContent(" " + imageSetData._name, _img_Rotoscoping), (isSelected ? _guiStyle_Selected : _guiStyle_None), GUILayout.Height(height)))
			{
				isClick = true;
			}

			EditorGUILayout.EndHorizontal();

			return isClick;
		}

		private enum IMAGE_FILE_ITEM_RESULT
		{
			None, Remove, OrderUp, OrderDown, ChangePath
		}

		private IMAGE_FILE_ITEM_RESULT DrawImageFile(apRotoscoping.ImageFileData imageFileData, int width, int height, float scrollX)
		{
			IMAGE_FILE_ITEM_RESULT result = IMAGE_FILE_ITEM_RESULT.None;

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));
			GUILayout.Space(5);

			

			//경로, 폴더 변경, 레이어 순서, 삭제
			if(GUILayout.Button(_img_LayerUp, GUILayout.Width(20), GUILayout.Height(height)))
			{
				result = IMAGE_FILE_ITEM_RESULT.OrderUp;
			}
			if(GUILayout.Button(_img_LayerDown, GUILayout.Width(20), GUILayout.Height(height)))
			{
				result = IMAGE_FILE_ITEM_RESULT.OrderDown;
			}
			GUILayout.Space(5);

			EditorGUILayout.TextField(imageFileData._filePath, _guiStyle_FilePath, GUILayout.Width(width - 170), GUILayout.Height(height));
			if (GUILayout.Button(_editor.GetUIWord(UIWORD.Set), GUILayout.Width(80), GUILayout.Height(height)))
			{
				result = IMAGE_FILE_ITEM_RESULT.ChangePath;
			}
			GUILayout.Space(5);
			if(GUILayout.Button(_img_Remove, GUILayout.Width(20), GUILayout.Height(height)))
			{
				result = IMAGE_FILE_ITEM_RESULT.Remove;
			}

			EditorGUILayout.EndHorizontal();

			return result;
		}
	}
}