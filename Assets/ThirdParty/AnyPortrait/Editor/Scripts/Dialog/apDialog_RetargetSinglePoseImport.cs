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
	public class apDialog_RetargetSinglePoseImport : EditorWindow
	{
		// Members
		//---------------------------------------------------------------------------
		public delegate void FUNC_RETARGET_SINGLE_POSE_IMPORT_ANIM(object loadKey, bool isSuccess, apRetarget resultRetarget,
																apMeshGroup targetMeshGroup,
																apAnimClip targetAnimClip, 
																apAnimTimeline targetAnimTimeline,
																int targetFrame,
																IMPORT_METHOD importMethod);

		public delegate void FUNC_RETARGET_SINGLE_POSE_IMPORT_MOD(object loadKey, bool isSuccess, apRetarget resultRetarget,
																apMeshGroup targetMeshGroup,
																apModifierBase targetModifier, apModifierParamSet targetParamSet,
																IMPORT_METHOD importMethod);

		private static apDialog_RetargetSinglePoseImport s_window = null;

		private FUNC_RETARGET_SINGLE_POSE_IMPORT_ANIM _funcResult_Anim = null;
		private FUNC_RETARGET_SINGLE_POSE_IMPORT_MOD _funcResult_Mod = null;

		public enum IMPORT_METHOD
		{
			Normal,
			Mirror
		}

		private apEditor _editor = null;
		private object _loadKey = null;
		private apMeshGroup _targetMeshGroup = null;

		private apModifierBase _targetModifier = null;
		private apModifierParamSet _targetParamSet = null;
		private apAnimClip _targetAnimClip = null;
		private apAnimTimeline _targetAnimTimeline = null;
		private int _targetFrame = -1;

		private apRetarget _retarget = new apRetarget();

		private Vector2 _scrollList_Pose = new Vector2();

		//private Texture2D _imgIcon_Bone = null;


		private enum CATEGORY
		{
			SameMeshGroup,
			SamePortrait,
			AllPoses,
		}

		private CATEGORY _category = CATEGORY.SameMeshGroup;
		private int _meshGroupUniqueID = -1;
		private string _portraitName = "";

		private apRetargetPoseListFile.FileMetaData _selectedBonePoseFile = null;
		private bool _isValidPose = false;

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(FUNC_RETARGET_SINGLE_POSE_IMPORT_ANIM funcResult_Anim, apEditor editor, apMeshGroup targetMeshGroup, 
										apAnimClip targetAnimClip, apAnimTimeline targetAnimTimeline, int targetFrame)
		{
			return ShowDialog(funcResult_Anim, null, editor, targetMeshGroup, null, null, targetAnimClip, targetAnimTimeline, targetFrame);
		}

		public static object ShowDialog(FUNC_RETARGET_SINGLE_POSE_IMPORT_MOD funcResult_Mod, apEditor editor, apMeshGroup targetMeshGroup, 
										apModifierBase targetModifier, apModifierParamSet targetParamSet)
		{
			return ShowDialog(null, funcResult_Mod, editor, targetMeshGroup, targetModifier, targetParamSet, null, null, -1);
		}


		private static object ShowDialog(FUNC_RETARGET_SINGLE_POSE_IMPORT_ANIM funcResult_Anim, FUNC_RETARGET_SINGLE_POSE_IMPORT_MOD funcResult_Mod, apEditor editor, apMeshGroup targetMeshGroup, 
			apModifierBase targetModifier, apModifierParamSet targetParamSet, //<<일반 Modifier에서 작업하는 경우
			apAnimClip targetAnimClip, apAnimTimeline targetAnimTimeline, int targetFrame//<<애니메이션에서 Pose를 여는 경우
			)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_RetargetSinglePoseImport), true, "Import Pose", true);
			apDialog_RetargetSinglePoseImport curTool = curWindow as apDialog_RetargetSinglePoseImport;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 500;
				int height = 700;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				s_window.Init(funcResult_Anim, funcResult_Mod, editor, loadKey, targetMeshGroup, 
								targetModifier, targetParamSet,
								targetAnimClip, targetAnimTimeline, targetFrame);

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
		public void Init(	FUNC_RETARGET_SINGLE_POSE_IMPORT_ANIM funcResult_Anim, FUNC_RETARGET_SINGLE_POSE_IMPORT_MOD funcResult_Mod, 
							apEditor editor, object loadKey, apMeshGroup targetMeshGroup, 
							apModifierBase targetModifier, apModifierParamSet targetParamSet, //<<일반 Modifier에서 작업하는 경우
							apAnimClip targetAnimClip, apAnimTimeline targetAnimTimeline, int targetFrame)
		{
			_editor = editor;
			_loadKey = loadKey;
			_targetMeshGroup = targetMeshGroup;

			_funcResult_Anim = funcResult_Anim;
			_funcResult_Mod = funcResult_Mod;

			_targetModifier = targetModifier;
			_targetParamSet = targetParamSet;
			_targetAnimClip = targetAnimClip;
			_targetAnimTimeline = targetAnimTimeline;
			_targetFrame = targetFrame;

			//_imgIcon_Bone = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Bone);
			
			_category = CATEGORY.SameMeshGroup;

			_retarget.LoadSinglePoseFileList(editor);
			_selectedBonePoseFile = null;
			_isValidPose = false;

			_meshGroupUniqueID = _targetMeshGroup._uniqueID;
			_portraitName = _targetMeshGroup._parentPortrait.name;
		}

		void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;
			if (_editor == null || _targetMeshGroup == null)
			{
				CloseDialog();
				return;
			}

			try
			{

				Color prevColor = GUI.backgroundColor;

				//레이아웃 구조
				// 타이틀
				// Pose 리스트 경로 + 변경 + 갱신
				// Pose 보기 종류 : 동일 MeshGroup만, 동일 Portrait만, 모든 Scene에서의 Pose 기록 (Import 안될 수도 있음)
				// Pose 리스트
				// 선택한 Pose 정보
				// Pose 이름, 설명
				// 저장된 Bone 이름들 (최대 4개 보여주기)
				// Import / Close

				width -= 10;

				//1. 다이얼로그 타이틀
				GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
				guiStyleBox.alignment = TextAnchor.MiddleCenter;
				guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUIStyle guiStyleBox_Left = new GUIStyle(GUI.skin.textField);
				guiStyleBox_Left.alignment = TextAnchor.MiddleLeft;

				//"  Import Pose"
				GUILayout.Box(new GUIContent("  " + _editor.GetText(TEXT.DLG_ImportPose), _editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones)), guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Space(5);

				//2. Pose 리스트 경로 + 변경 + 갱신
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
				int changeBtnWidth = 70;
				int refreshBtnWidth = 80;
				int pathWidth = width - (changeBtnWidth + refreshBtnWidth + 4 + 10);

				GUILayout.Space(5);
				EditorGUILayout.TextField(_editor._bonePose_BaseFolderName, GUILayout.Width(pathWidth), GUILayout.Height(20));

				//"Change"
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_Change), GUILayout.Width(changeBtnWidth), GUILayout.Height(20)))
				{
					string pathResult = EditorUtility.SaveFolderPanel("Set the Pose Folder", _editor._bonePose_BaseFolderName, "");
					if (!string.IsNullOrEmpty(pathResult))
					{
						Uri targetUri = new Uri(pathResult);
						Uri baseUri = new Uri(Application.dataPath);

						string relativePath = baseUri.MakeRelativeUri(targetUri).ToString();


						_editor._bonePose_BaseFolderName = apUtil.ConvertEscapeToPlainText(relativePath);//변경 21.7.3 : 이스케이프 문자 삭제
						
						apEditorUtil.SetEditorDirty();

						_editor.SaveEditorPref();

						_retarget.LoadSinglePoseFileList(_editor);
						_selectedBonePoseFile = null;
						_isValidPose = false;
					}
				}
				//"Refresh"
				if(GUILayout.Button(_editor.GetText(TEXT.DLG_Refresh), GUILayout.Width(refreshBtnWidth), GUILayout.Height(20)))
				{
					_retarget.LoadSinglePoseFileList(_editor);
					_selectedBonePoseFile = null;
					_isValidPose = false;
				}

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				//3. Pose 보기 종류
				int categoryBtnWidth = (width - 10) / 3 - 1;
				int categoryBtnHeight = 20;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(categoryBtnHeight));
				GUILayout.Space(5);

				//"Same MeshGroup"
				if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_SameGroup), _category == CATEGORY.SameMeshGroup, true, categoryBtnWidth, categoryBtnHeight))
				{
					_category = CATEGORY.SameMeshGroup;
					_selectedBonePoseFile = null;
					_isValidPose = false;
				}

				//"Same Portrait"
				if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_SamePortrait), _category == CATEGORY.SamePortrait, true, categoryBtnWidth, categoryBtnHeight))
				{
					_category = CATEGORY.SamePortrait;
					_selectedBonePoseFile = null;
					_isValidPose = false;
				}

				//"All Poses"
				if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_AllPoses), _category == CATEGORY.AllPoses, true, categoryBtnWidth, categoryBtnHeight))
				{
					_category = CATEGORY.AllPoses;
					_selectedBonePoseFile = null;
					_isValidPose = false;
				}

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				//4. Pose 리스트
				int listHeight = height - 500;

				Rect lastRect = GUILayoutUtility.GetLastRect();

				GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
				GUI.Box(new Rect(0, lastRect.y + 5, width + 10, listHeight), "");

				List<apRetargetPoseListFile.FileMetaData> metaDataList = _retarget.SinglePoseList._metaDataList;

				int itemWidth = width - 20;
				int itemHeight = 20;

				int poseNameWidth = 120;
				int portraitNameWidth = 100;
				int meshGroupNameWidth = 100;
				int selectBtnWidth = (width - 40) - (poseNameWidth + portraitNameWidth + meshGroupNameWidth + 6);

				GUIStyle guiStyle_ItemLabel = new GUIStyle(GUI.skin.label);
				guiStyle_ItemLabel.alignment = TextAnchor.MiddleLeft;

				GUIStyle guiStyle_ItemCategory = new GUIStyle(GUI.skin.label);
				//guiStyle_ItemCategory.alignment = TextAnchor.MiddleCenter;
				if(EditorGUIUtility.isProSkin)
				{
					guiStyle_ItemCategory.normal.textColor = Color.cyan;
				}
				else
				{
					guiStyle_ItemCategory.normal.textColor = Color.blue;
				}
				

				_scrollList_Pose = EditorGUILayout.BeginScrollView(_scrollList_Pose, false, true, GUILayout.Width(width + 10), GUILayout.Height(listHeight));
				EditorGUILayout.BeginVertical(GUILayout.Width(width - 10));

				if(!_retarget.SinglePoseList._isFolderExist)
				{
					//"Pose Folder does not exist."
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_PoseFolderNotExist), GUILayout.Width(itemWidth));
				}
				else
				{
					//Pose 이름 / Portrait 이름 / MeshGroup 이름 / 선택

					EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
					GUILayout.Space(5);
					
					//"Name"
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Name), guiStyle_ItemCategory, GUILayout.Width(poseNameWidth), GUILayout.Height(itemHeight));

					//"Portrait"
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Portrait), guiStyle_ItemCategory, GUILayout.Width(portraitNameWidth), GUILayout.Height(itemHeight));

					//"Mesh Group"
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_MeshGroup), guiStyle_ItemCategory, GUILayout.Width(meshGroupNameWidth), GUILayout.Height(itemHeight));
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(10);

					apRetargetPoseListFile.FileMetaData metaData = null;
					for (int i = 0; i < metaDataList.Count; i++)
					{
						metaData = metaDataList[i];

						if(_category == CATEGORY.AllPoses)
						{
							//그냥 통과
						}
						else if(_category == CATEGORY.SamePortrait)
						{
							if(!string.Equals(_portraitName, metaData._portraitName))
							{
								//Portrait 이름이 다르면 스킵
								continue;
							}
						}
						else if(_category == CATEGORY.SameMeshGroup)
						{
							if(!string.Equals(_portraitName, metaData._portraitName)
								|| _meshGroupUniqueID != metaData._meshGroupUniqueID)
							{
								//Portrait 이름이 다르거나 MeshGroupUniqueID가 다르다면 스킵
								continue;
							}
						}

						EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
						GUILayout.Space(5);
						EditorGUILayout.LabelField(metaData._poseName, guiStyle_ItemLabel, GUILayout.Width(poseNameWidth), GUILayout.Height(itemHeight));
						EditorGUILayout.LabelField(metaData._portraitName, guiStyle_ItemLabel, GUILayout.Width(portraitNameWidth), GUILayout.Height(itemHeight));
						EditorGUILayout.LabelField(metaData._meshGroupName, guiStyle_ItemLabel, GUILayout.Width(meshGroupNameWidth), GUILayout.Height(itemHeight));

						//"Selected", "Select"
						if(apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_Selected),
															_editor.GetText(TEXT.DLG_Select), metaData == _selectedBonePoseFile, true, selectBtnWidth, itemHeight))
						{
							_selectedBonePoseFile = metaData;

							_isValidPose = string.Equals(_portraitName, metaData._portraitName) 
								&& _meshGroupUniqueID == metaData._meshGroupUniqueID;
						}
						EditorGUILayout.EndHorizontal();
					}
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(listHeight + 100);

				EditorGUILayout.EndScrollView();

				GUILayout.Space(10);
				//정보를 출력한다.
				//Pose 이름
				//설명
				//Portrait
				//MeshGroup
				//MeshGroup이 다르면 경고를 준다.
				//Bone 개수와 이름들
				//string selectedPoseName = "No Pose Selected";
				string selectedPoseName = _editor.GetText(TEXT.DLG_NoPoseSelected);
				string selectedDesc = "";
				string selectedPortrait = "";
				string selectedMeshGroupName = "";
				int boneCount = 0;
				string selectedBoneNames = "";

				if(_selectedBonePoseFile != null)
				{
					selectedPoseName = _selectedBonePoseFile._poseName;
					selectedDesc = _selectedBonePoseFile._description;
					selectedPortrait = _selectedBonePoseFile._portraitName;
					selectedMeshGroupName = _selectedBonePoseFile._meshGroupName;
					boneCount = _selectedBonePoseFile._nBones;
					selectedBoneNames = _selectedBonePoseFile._boneNames;
					
				}

				GUIStyle guiStyle_Desc = new GUIStyle(GUI.skin.textField);
				guiStyle_Desc.wordWrap = true;
				
				if(_selectedBonePoseFile != null)
				{
					GUI.backgroundColor = new Color(GUI.backgroundColor.r * 1.0f, GUI.backgroundColor.g * 1.8f, GUI.backgroundColor.b * 1.2f, 1.0f);
				}
				GUILayout.Box(selectedPoseName, guiStyleBox, GUILayout.Width(width), GUILayout.Height(30));
				if(_selectedBonePoseFile != null)
				{
					GUI.backgroundColor = prevColor;
				}

				//"Description"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_Description), GUILayout.Width(width));
				EditorGUILayout.LabelField(selectedDesc, guiStyle_Desc, GUILayout.Width(width), GUILayout.Height(45));

				GUILayout.Space(5);
				
				EditorGUILayout.LabelField(string.Format("{0} : {1}", _editor.GetText(TEXT.DLG_Portrait), selectedPortrait), GUILayout.Width(width));//"Portrait : "
				EditorGUILayout.LabelField(string.Format("{0} : {1}", _editor.GetText(TEXT.DLG_MeshGroup), selectedMeshGroupName), GUILayout.Width(width));//"Mesh Group : "
				GUILayout.Space(5);
				EditorGUILayout.LabelField(string.Format("{0} : {1}", _editor.GetText(TEXT.DLG_NumberBones), boneCount), GUILayout.Width(width));//"Number of Bones : "

				if (_selectedBonePoseFile == null)
				{
					//"No Bones"
					GUILayout.Box(_editor.GetText(TEXT.DLG_NoBones), GUILayout.Width(width), GUILayout.Height(95));
				}
				else
				{
					if (_isValidPose)
					{
						GUI.backgroundColor = new Color(GUI.backgroundColor.r * 1.0f, GUI.backgroundColor.g * 1.5f, GUI.backgroundColor.b * 1.2f, 1.0f);
						GUILayout.Box(selectedBoneNames, guiStyleBox, GUILayout.Width(width), GUILayout.Height(95));
						GUI.backgroundColor = prevColor;
					}
					else
					{
						GUI.backgroundColor = new Color(GUI.backgroundColor.r * 1.5f, GUI.backgroundColor.g * 0.7f, GUI.backgroundColor.b * 0.7f, 1.0f);
						//"[Warning. Import may not work properly]\n" + selectedBoneNames
						GUILayout.Box(string.Format("[{0}]\n{1}",_editor.GetText(TEXT.DLG_Warningproperly), selectedBoneNames), guiStyleBox, GUILayout.Width(width), GUILayout.Height(95));
						GUI.backgroundColor = prevColor;
					}
				}

				//"Remove Pose", "Remove Pose"
				if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.DLG_RemovePose), _editor.GetText(TEXT.DLG_RemovePose), false, _selectedBonePoseFile != null, width, 20))
				{
					bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.Retarget_RemoveSinglePose_Title),
						_editor.GetTextFormat(TEXT.Retarget_RemoveSinglePose_Body, selectedPoseName),
						_editor.GetText(TEXT.Remove), _editor.GetText(TEXT.Cancel)
						);

					if(isResult && _selectedBonePoseFile != null)
					{
						_retarget.SinglePoseList.RemoveFile(_selectedBonePoseFile);

						//리스트 갱신
						_retarget.LoadSinglePoseFileList(_editor);
						_selectedBonePoseFile = null;
						_isValidPose = false;
					}
				}

				string strImportPose = " " + _editor.GetText(TEXT.DLG_ImportPose);
				string strImportPoseSymm = " " + _editor.GetText(TEXT.DLG_ImportPoseToMirror);
				GUILayout.Space(10);

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(4);

				int widthBtnHalf = ((width - 6) / 2) + 1;

				//" Import Pose", " Import Pose"
				if (apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBones), 
					strImportPose, strImportPose, false, _selectedBonePoseFile != null, widthBtnHalf, 30))
				{
					//1. Retarget Bone Pose에 선택한 MetaData 정보를 넣자
					if (_selectedBonePoseFile != null)
					{
						bool isSuccess = _retarget.LoadSinglePose(_selectedBonePoseFile._filePath);

						if (!isSuccess)
						{
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Title),
							_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Body_Error),
							_editor.GetText(TEXT.Close));
						}
						else
						{
							//2. 리턴 함수 콜
							if (_funcResult_Anim != null)
							{
								_funcResult_Anim(_loadKey, true, _retarget, _targetMeshGroup, _targetAnimClip, _targetAnimTimeline, _targetFrame, IMPORT_METHOD.Normal);
							}
							else if (_funcResult_Mod != null)
							{
								_funcResult_Mod(_loadKey, true, _retarget, _targetMeshGroup, _targetModifier, _targetParamSet, IMPORT_METHOD.Normal);
							}

							//3. 창 닫기
							CloseDialog();
						}
					}
					else
					{
						EditorUtility.DisplayDialog(_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Title),
							_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Body_NoFile),
							_editor.GetText(TEXT.Close));
					}
					
				}

				//" Import Pose to Mirror Bones", " Import Pose to Mirror Bones"
				if (apEditorUtil.ToggledButton_2Side(_editor.ImageSet.Get(apImageSet.PRESET.Rig_LoadBonesMirror), 
					strImportPoseSymm, strImportPoseSymm, false, _selectedBonePoseFile != null, widthBtnHalf, 30))
				{
					//1. Retarget Bone Pose에 선택한 MetaData 정보를 넣자
					if (_selectedBonePoseFile != null)
					{
						bool isSuccess = _retarget.LoadSinglePose(_selectedBonePoseFile._filePath);

						if (!isSuccess)
						{
							EditorUtility.DisplayDialog(_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Title),
							_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Body_Error),
							_editor.GetText(TEXT.Close));
						}
						else
						{
							//2. 리턴 함수 콜
							if (_funcResult_Anim != null)
							{
								_funcResult_Anim(_loadKey, true, _retarget, _targetMeshGroup, _targetAnimClip, _targetAnimTimeline, _targetFrame, IMPORT_METHOD.Mirror);
							}
							else if (_funcResult_Mod != null)
							{
								_funcResult_Mod(_loadKey, true, _retarget, _targetMeshGroup, _targetModifier, _targetParamSet, IMPORT_METHOD.Mirror);
							}

							//3. 창 닫기
							CloseDialog();
						}
					}
					else
					{
						EditorUtility.DisplayDialog(_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Title),
							_editor.GetText(TEXT.Retarget_SinglePoseImportFailed_Body_NoFile),
							_editor.GetText(TEXT.Close));
					}
					
				}

				EditorGUILayout.EndHorizontal();

				//"Close"
				if (GUILayout.Button(_editor.GetText(TEXT.DLG_Close), GUILayout.Width(width), GUILayout.Height(25)))
				{
					if (_funcResult_Anim != null)
					{
						_funcResult_Anim(null, false, _retarget, _targetMeshGroup, _targetAnimClip, _targetAnimTimeline, _targetFrame, IMPORT_METHOD.Normal);
					}
					else if (_funcResult_Mod != null)
					{
						_funcResult_Mod(null, false, _retarget, _targetMeshGroup, _targetModifier, _targetParamSet, IMPORT_METHOD.Normal);
					}

					CloseDialog();
				}

			}
			catch (Exception)
			{

			}
		}
	}
}