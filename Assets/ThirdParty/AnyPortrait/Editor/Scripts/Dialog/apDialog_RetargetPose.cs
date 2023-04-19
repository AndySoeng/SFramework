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

	/// <summary>
	/// RetargetBase와 달리 AnimClip에 적용되는 다이얼로그
	/// Morph를 제외한 모든 타임라인의 값을 가져온다.
	/// </summary>
	public class apDialog_RetargetPose : EditorWindow
	{

		// Members
		//----------------------------------------------------------------------------
		public delegate void FUNC_LOAD_ANIM_RETARGET(bool isSuccess, object loadKey, apRetarget retargetData, apMeshGroup targetMeshGroup, apAnimClip targetAnimClip, bool isMerge);

		private static apDialog_RetargetPose s_window = null;



		private apEditor _editor = null;
		private object _loadKey = null;
		private apMeshGroup _targetMeshGroup = null;
		private apAnimClip _targetAnimClip = null;

		private FUNC_LOAD_ANIM_RETARGET _funcResult;

		private apRetarget _retargetData = new apRetarget();

		private apRetargetMapping _mapping = new apRetargetMapping();
		private Vector2 _scrollList_Bone = new Vector2();
		private Vector2 _scrollList_Timeline = new Vector2();

		//매핑 정보는 한개가 아니라 몇가지 종류로 나뉜다.
		private enum MAPPING_CATEGORY
		{
			Transform,		//Mesh Transform, MeshGroup Transform을 연결한다.
			Bone,			//Bone을 서로 연결한다.
			ControlParam,	//ControlParam을 연결한다.
			Timeline,		//Timeline/TimelineLayer에 연동될 ControlParam과 Modifier를 연결한다.
			Event,			//Event의 Import 여부를 결정한다.

		}

		private MAPPING_CATEGORY _mappingCategory = MAPPING_CATEGORY.Transform;

		private Texture2D _imgIcon_Mesh = null;
		private Texture2D _imgIcon_MeshGroup = null;
		private Texture2D _imgIcon_Bone = null;
		private Texture2D _imgIcon_Timeline_ControlParam = null;
		private Texture2D _imgIcon_Timeline_AnimModifier = null;
		private Texture2D _imgIcon_Event = null;
		private Texture2D _imgIcon_ControlParam = null;

		private Texture2D _imgIcon_FoldRight = null;
		private Texture2D _imgIcon_FoldDown = null;

		
		public class TargetUnit
		{
			public string _name = "";

			public enum TYPE
			{
				MeshTransform, MeshGroupTransform, Bone, Timeline, TimelineLayer, ControlParam
			}

			public TYPE _type = TYPE.Bone;

			public apTransform_Mesh _meshTransform = null;
			public apTransform_MeshGroup _meshGroupTransform = null;
			public apBone _bone = null;
			public apAnimTimeline _timeline = null;
			public apAnimTimelineLayer _timelineLayer = null;
			public apControlParam _controlParam = null;

			public TargetUnit _parentUnit = null;

			//연결된 Src
			public bool _isLinked = false;
			public apRetargetSubUnit _linkedSubUnit = null;
			public apRetargetTimelineUnit _linkedTimelineUnit = null;
			public apRetargetTimelineLayerUnit _linkedTimelineLayerUnit = null;
			public apRetargetControlParam _linkedControlParam = null;

			public bool _isFold = true;


			public TargetUnit(apTransform_Mesh meshTransform)
			{
				_name = meshTransform._nickName;
				_type = TYPE.MeshTransform;
				_meshTransform = meshTransform;

				ClearLink();
			}

			public TargetUnit(apTransform_MeshGroup meshGroupTransform)
			{
				_name = meshGroupTransform._nickName;
				_type = TYPE.MeshGroupTransform;
				_meshGroupTransform = meshGroupTransform;

				ClearLink();
			}

			public TargetUnit(apBone bone)
			{
				_name = bone._name;
				_type = TYPE.Bone;
				_bone = bone;

				ClearLink();
			}

			public TargetUnit(apAnimTimeline timeline)
			{
				_name = timeline.DisplayName;
				_type = TYPE.Timeline;
				_timeline = timeline;

				ClearLink();
			}

			public TargetUnit(apAnimTimelineLayer timelineLayer, TargetUnit parentUnit)
			{
				_name = timelineLayer.DisplayName;
				_type = TYPE.TimelineLayer;
				_timelineLayer = timelineLayer;
				_parentUnit = parentUnit;

				ClearLink();
			}

			public TargetUnit(apControlParam controlParam)
			{
				_name = controlParam._keyName;
				_type = TYPE.ControlParam;
				_controlParam = controlParam;

				ClearLink();
			}

			public void ClearLink()
			{
				_isLinked = false;
				_linkedSubUnit = null;
				_linkedTimelineUnit = null;
				_linkedTimelineLayerUnit = null;
				_linkedControlParam = null;
			}
		}



		private List<TargetUnit> _targetTransforms = new List<TargetUnit>();
		private List<TargetUnit> _targetBones = new List<TargetUnit>();
		private List<TargetUnit> _targetTimelines = new List<TargetUnit>();
		private Dictionary<TargetUnit, List<TargetUnit>> _targetTimelineLayers = new Dictionary<TargetUnit, List<TargetUnit>>();
		private List<TargetUnit> _targetControlParams = new List<TargetUnit>();

		//Link를 위해 Select를 한다.
		private bool _isSelectLinkMode = false;
		// Import Src -> Target 으로 연결
		private MAPPING_CATEGORY _selectCategory = MAPPING_CATEGORY.Bone;
		private apRetargetSubUnit _srcTransformSubUnit = null;
		private apRetargetSubUnit _srcBoneSubUnit = null;
		private apRetargetTimelineUnit _srcTimelineUnit = null;
		private apRetargetControlParam _srcControlParamUnit = null;
		//private TargetUnit _dstSubUnit = null;

		

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog(apEditor editor, apMeshGroup targetMeshGroup, apAnimClip targetAnimClip, FUNC_LOAD_ANIM_RETARGET funcResult)
		{
			CloseDialog();

			if (editor == null || editor._portrait == null || editor._portrait._controller == null)
			{
				return null;
			}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_RetargetPose), true, "Export/Import Animation", true);
			apDialog_RetargetPose curTool = curWindow as apDialog_RetargetPose;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 900;
				int height = 800;
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);
				s_window.Init(editor, loadKey, targetMeshGroup, targetAnimClip, funcResult);

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
		public void Init(apEditor editor, object loadKey, apMeshGroup targetMeshGroup, apAnimClip targetAnimClip, FUNC_LOAD_ANIM_RETARGET funcResult)
		{
			_editor = editor;
			_loadKey = loadKey;
			_funcResult = funcResult;
			_targetMeshGroup = targetMeshGroup;
			_targetAnimClip = targetAnimClip;


			_imgIcon_Mesh = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			_imgIcon_MeshGroup = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			_imgIcon_Bone = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Bone);
			_imgIcon_Timeline_ControlParam = _editor.ImageSet.Get(apImageSet.PRESET.Anim_WithControlParam);
			_imgIcon_Timeline_AnimModifier = _editor.ImageSet.Get(apImageSet.PRESET.Anim_WithMod);
			_imgIcon_Event = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Animation);
			_imgIcon_ControlParam = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Param);

			_imgIcon_FoldRight = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldRight);
			_imgIcon_FoldDown = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_FoldDown);

			_targetTransforms.Clear();
			_targetBones.Clear();
			_targetTimelines.Clear();
			_targetTimelineLayers.Clear();
			_targetControlParams.Clear();

			for (int i = 0; i < _targetMeshGroup._renderUnits_All.Count; i++)
			{
				if (_targetMeshGroup._renderUnits_All[i]._meshTransform != null)
				{
					_targetTransforms.Add(new TargetUnit(_targetMeshGroup._renderUnits_All[i]._meshTransform));
				}
				else if(_targetMeshGroup._renderUnits_All[i]._meshGroupTransform != null)
				{
					if(_targetMeshGroup._renderUnits_All[i]._meshGroupTransform != _targetMeshGroup._rootMeshGroupTransform)
					{
						//Root는 제외한다.
						_targetTransforms.Add(new TargetUnit(_targetMeshGroup._renderUnits_All[i]._meshGroupTransform));
					}
					
				}
			}
			_targetTransforms.Reverse();//<<순서 반대로

			//<BONE_EDIT>
			//for (int i = 0; i < _targetMeshGroup._boneList_All.Count; i++)
			//{
			//	_targetBones.Add(new TargetUnit(_targetMeshGroup._boneList_All[i]));
			//}
			
			//>>Bone Set 이용
			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < _targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = _targetMeshGroup._boneListSets[iSet];
				for (int iBone = 0; iBone < boneSet._bones_All.Count; iBone++)
				{
					_targetBones.Add(new TargetUnit(boneSet._bones_All[iBone]));
				}
			}

			for (int iTL = 0; iTL < targetAnimClip._timelines.Count; iTL++)
			{
				apAnimTimeline timeline = targetAnimClip._timelines[iTL];

				TargetUnit timelineUnit = new TargetUnit(timeline);
				_targetTimelines.Add(timelineUnit);

				_targetTimelineLayers.Add(timelineUnit, new List<TargetUnit>());
				for (int iL = 0; iL < timeline._layers.Count; iL++)
				{

					_targetTimelineLayers[timelineUnit].Add(new TargetUnit(timeline._layers[iL], timelineUnit));
				}
			}

			for (int i = 0; i < _targetMeshGroup._parentPortrait._controller._controlParams.Count; i++)
			{
				_targetControlParams.Add(new TargetUnit(_targetMeshGroup._parentPortrait._controller._controlParams[i]));
			}

		}


		// GUI
		//------------------------------------------------------------------------
		void OnGUI()
		{
			try
			{
				int width = (int)position.width;
				int height = (int)position.height;
				if (_editor == null || _funcResult == null || _targetMeshGroup == null || _targetAnimClip == null)
				{
					CloseDialog();
					return;
				}

				Color prevColor = GUI.backgroundColor;

				//레이아웃 구조
				//1. Save
				// - 저장 버튼
				//2. Load
				// - 로드 버튼
				// - 본 정보 리스트
				//   - <색상> 인덱스, 이름 -> 적용 여부 + IK + 색상 로드
				// - 전체 선택 / 해제
				// - 전체 IK 포함 여부, 
				// - 옵션 : 크기

				width -= 10;

				//1. Save
				GUIStyle guiStyleBox = new GUIStyle(GUI.skin.box);
				guiStyleBox.alignment = TextAnchor.MiddleCenter;
				guiStyleBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUIStyle guiStyleBox_Left = new GUIStyle(GUI.skin.textField);
				guiStyleBox_Left.alignment = TextAnchor.MiddleLeft;

				string strExport = _editor.GetText(TEXT.DLG_Export);
				string strImport = _editor.GetText(TEXT.DLG_Import);
				string strSelect = _editor.GetText(TEXT.DLG_Select);

				//"  Export Animation Clip"
				GUILayout.Box(new GUIContent("  " + _editor.GetText(TEXT.DLG_ExportAnimationClip), _editor.ImageSet.Get(apImageSet.PRESET.Anim_Save)), guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Space(5);
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				int nTimelines = _targetAnimClip._timelines.Count;

				string strTimeline = "";
				if (nTimelines == 0)
				{
					//strTimeline = "No Timelines to Export";
					strTimeline = _editor.GetText(TEXT.DLG_NoTimelinesToExport);
				}
				else if (nTimelines == 1)
				{
					//strTimeline = "1 Timeline to Export";
					strTimeline = _editor.GetText(TEXT.DLG_1TimelinesToExport);
				}
				else
				{
					//strTimeline = nTimelines + " Timelines to Export";
					strTimeline = _editor.GetTextFormat(TEXT.DLG_NTimelinesToExport, nTimelines);
				}
				if (nTimelines > 0)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.8f, prevColor.g * 1.5f, prevColor.b * 1.5f, 1.0f);
				}
				GUILayout.Box(strTimeline, guiStyleBox, GUILayout.Width(width - 120), GUILayout.Height(25));
				GUI.backgroundColor = prevColor;

				if (apEditorUtil.ToggledButton(_editor.ImageSet.Get(apImageSet.PRESET.Anim_Save), " " + strExport, false, (nTimelines > 0), 115, 25))//" Export"
				{
					string saveFilePath = EditorUtility.SaveFilePanel("Save Animation Clip",
																		apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport), "", "ani");
					if (!string.IsNullOrEmpty(saveFilePath))
					{
						//추가 21.7.3 : 이스케이프 문자 삭제
						saveFilePath = apUtil.ConvertEscapeToPlainText(saveFilePath);

						//TODO : Save를 하자
						bool isResult = apRetarget.SaveAnimClip(_targetAnimClip, saveFilePath);

						apEditorUtil.SetLastExternalOpenSaveFilePath(saveFilePath, apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport);//추가 21.3.1.

						if (isResult)
						{
							_editor.Notification("[" + saveFilePath + "] is Saved", false, false);
							//Debug.Log("[" + saveFilePath + "] is Saved");
						}
						else
						{
							//Debug.LogError("File Save Failed");
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);


				//2. Load
				// - 로드 버튼
				// - 본 정보 리스트
				//   - <색상> 인덱스, 이름 -> 적용 여부 + IK + 색상 로드
				// - 전체 선택 / 해제
				// - 전체 IK 포함 여부, 
				// - 옵션 : 크기
				//"  Import Animation Clip"
				GUILayout.Box(new GUIContent("  " + _editor.GetText(TEXT.DLG_ImportAnimationClip), _editor.ImageSet.Get(apImageSet.PRESET.Anim_Load)), guiStyleBox, GUILayout.Width(width), GUILayout.Height(35));
				GUILayout.Space(5);


				//로드한 파일 정보
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				//TODO : 
				bool isFileLoaded = _retargetData.IsAnimFileLoaded;
				string strFileName = _retargetData.AnimLoadedFilePath;
				if (isFileLoaded)
				{
					GUI.backgroundColor = new Color(prevColor.r * 0.8f, prevColor.g * 2.0f, prevColor.b * 0.8f, 1.0f);
				}
				else
				{
					//strFileName = "No File is Imported";
					strFileName = _editor.GetText(TEXT.DLG_NoFileIsImported);
					GUI.backgroundColor = new Color(prevColor.r * 1.5f, prevColor.g * 0.8f, prevColor.b * 0.8f, 1.0f);
				}

				EditorGUILayout.TextField(strFileName, guiStyleBox_Left, GUILayout.Width(width - 120), GUILayout.Height(25));
				GUI.backgroundColor = prevColor;

				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_LoadFile), false, true, 115, 25))//"Load File"
				{
					string loadFilePath = EditorUtility.OpenFilePanel("Open Animation Clip",
																		apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport), "ani");
					if (!string.IsNullOrEmpty(loadFilePath))
					{
						//추가 21.7.3 : 이스케이프 문자 삭제
						loadFilePath = apUtil.ConvertEscapeToPlainText(loadFilePath);


						bool loadResult = _retargetData.LoadAnimClip(loadFilePath);
						apEditorUtil.SetLastExternalOpenSaveFilePath(loadFilePath, apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport);//추가 21.3.1

						if (loadResult)
						{
							_editor.Notification("[" + loadFilePath + "] is Loaded", false, false);
							//Debug.Log("[" + loadFilePath + "] is Loaded");

							EndSelectLinkMode();//<<Select LinkMode를 초기화
												//연결도 초기화

							for (int i = 0; i < _targetTransforms.Count; i++)
							{
								_targetTransforms[i].ClearLink();
							}
							for (int i = 0; i < _targetBones.Count; i++)
							{
								_targetBones[i].ClearLink();
							}
							for (int i = 0; i < _targetTimelines.Count; i++)
							{
								_targetTimelines[i].ClearLink();
							}

							foreach (KeyValuePair<TargetUnit, List<TargetUnit>> layers in _targetTimelineLayers)
							{
								for (int i = 0; i < layers.Value.Count; i++)
								{
									layers.Value[i].ClearLink();
								}
							}

							//EnableAll을 먼저 수행한다.
							EnableAll(false);


						}
					}
				}

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				bool isDrawRightList = true;
				int categoryWidth = ((width - 10) / 5) - 2;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(5);
				//"Meshes / MeshGroups"
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_MeshesMeshGroups), _mappingCategory == MAPPING_CATEGORY.Transform, categoryWidth, 30))
				{
					_mappingCategory = MAPPING_CATEGORY.Transform;
				}
				//"Bones"
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Bones), _mappingCategory == MAPPING_CATEGORY.Bone, categoryWidth, 30))
				{
					_mappingCategory = MAPPING_CATEGORY.Bone;
				}
				//"Control Parameters"
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_ControlParameters), _mappingCategory == MAPPING_CATEGORY.ControlParam, categoryWidth, 30))
				{
					_mappingCategory = MAPPING_CATEGORY.ControlParam;
				}
				//"Timelines"
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Timelines), _mappingCategory == MAPPING_CATEGORY.Timeline, categoryWidth, 30))
				{
					_mappingCategory = MAPPING_CATEGORY.Timeline;
				}
				//"Events"
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_AnimEvents), _mappingCategory == MAPPING_CATEGORY.Event, categoryWidth, 30))
				{
					_mappingCategory = MAPPING_CATEGORY.Event;
				}
				EditorGUILayout.EndHorizontal();


				if (_mappingCategory == MAPPING_CATEGORY.Event)
				{
					isDrawRightList = false;
				}




				GUILayout.Space(5);

				//리스트를 좌우로 나눈다.
				int listHeight = height - 450;
				int leftWidth = ((width + 10) / 2) + 70;
				int rightWidth = ((width + 10) / 2) - 70;
				if (!isDrawRightList)
				{
					leftWidth = width + 10;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(20));
				GUILayout.Space(10);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_LoadedData), GUILayout.Width(150));//"Loaded Data"
				if (isDrawRightList)
				{
					GUILayout.Space(leftWidth - (160));
					EditorGUILayout.LabelField(_editor.GetText(TEXT.DLG_TargetObjects), GUILayout.Width(150));//"Target Objects"
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);

				GUIStyle guiStyle_ItemLabel = new GUIStyle(GUI.skin.label);
				guiStyle_ItemLabel.alignment = TextAnchor.MiddleLeft;

				GUIStyle guiStyle_ItemLabel_Linked = new GUIStyle(GUI.skin.label);
				guiStyle_ItemLabel_Linked.alignment = TextAnchor.MiddleLeft;
				if (EditorGUIUtility.isProSkin)
				{
					guiStyle_ItemLabel_Linked.normal.textColor = Color.cyan;
				}
				else
				{
					guiStyle_ItemLabel_Linked.normal.textColor = new Color(0.0f, 0.2f, 1.0f, 1.0f);
				}


				GUIStyle guiStyle_ItemTextBox = new GUIStyle(GUI.skin.textField);
				guiStyle_ItemTextBox.alignment = TextAnchor.MiddleLeft;
				guiStyle_ItemTextBox.normal.textColor = apEditorUtil.BoxTextColor;

				GUIStyle guiStyle_Box = new GUIStyle(GUI.skin.box);
				guiStyle_Box.margin = GUI.skin.textField.margin;
				guiStyle_Box.alignment = TextAnchor.MiddleLeft;
				guiStyle_Box.wordWrap = guiStyle_ItemLabel.wordWrap;

				GUIStyle guiStyle_Fold = new GUIStyle(GUI.skin.label);

				Rect lastRect = GUILayoutUtility.GetLastRect();

				GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
				GUI.Box(new Rect(0, lastRect.y + 5, leftWidth, listHeight), "");


				//링크 선택 모드 체크하자
				CheckSelectLinkMode();

				if (isDrawRightList)
				{
					if (_isSelectLinkMode)
					{
						GUI.backgroundColor = new Color(0.7f, 1.0f, 0.8f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f);
					}

					GUI.Box(new Rect(leftWidth, lastRect.y + 5, rightWidth, listHeight), "");
				}
				GUI.backgroundColor = prevColor;

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width + 10), GUILayout.Height(listHeight));

				apRetargetAnimFile animFile = _retargetData.AnimFile;


				//리타겟 정보와 타임라인 정보 두개를 보여줘야 한다.
				// 1. 왼쪽 리스트 : 타임라인 정보
				_scrollList_Timeline = EditorGUILayout.BeginScrollView(_scrollList_Timeline, false, true, GUILayout.Width(leftWidth), GUILayout.Height(listHeight));
				EditorGUILayout.BeginVertical(GUILayout.Width(leftWidth - 30));

				if (_targetMeshGroup != null && animFile != null)
				{
					//연결이 될 "대상"이다.
					//카테고리에 따라 나오는게 다르다.
					int itemWidth = leftWidth - 20;
					int itemHeight = 20;



					switch (_mappingCategory)
					{
						case MAPPING_CATEGORY.Transform:
							{
								//Transform을 그린다.
								int nTransforms = animFile._transforms_Total.Count;
								apRetargetSubUnit subUnit = null;
								for (int i = 0; i < nTransforms; i++)
								{
									subUnit = animFile._transforms_Total[i];
									EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
									GUILayout.Space(10);

									switch (subUnit._type)
									{
										case apRetargetSubUnit.TYPE.MeshTransform:
											EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Mesh), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
											break;

										case apRetargetSubUnit.TYPE.MeshGroupTransform:
											EditorGUILayout.LabelField(new GUIContent("", _imgIcon_MeshGroup), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
											break;

										case apRetargetSubUnit.TYPE.Bone:
											EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Bone), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
											break;
									}
									EditorGUILayout.LabelField(subUnit._name, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));


									//TODO
									// Import를 할 것인지
									// 대상이 무엇인지
									// Change 할 것인지를 결정
									GUILayout.Space(10);
									//"Import", "Import"
									if (apEditorUtil.ToggledButton_2Side(strImport, strImport, subUnit._isImported, true, 70, itemHeight))
									{
										subUnit._isImported = !subUnit._isImported;
									}
									GUILayout.Space(5);

									if (subUnit._isImported)
									{
										bool isSelected = false;
										bool isAvailable = false;
										string strLinkName = subUnit.LinkedName;

										if (_isSelectLinkMode)
										{
											//선택 모드일때
											if (_srcTransformSubUnit == subUnit)
											{
												strLinkName = " >>> ";
												isSelected = true;
												isAvailable = true;
											}
											else
											{
												isSelected = false;
												isAvailable = false;
											}
										}
										else
										{
											//일반 모드일때
											isSelected = subUnit._isImported && subUnit.IsLinked;
											isAvailable = subUnit._isImported;
										}

										if (apEditorUtil.ToggledButton_2Side(strLinkName, strLinkName, isSelected, isAvailable, 150, itemHeight))
										{
											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}
											else
											{
												//연결을 시작하자
												StartSelectLinkMode(subUnit, _mappingCategory);
											}
										}

										if (GUILayout.Button("X", GUILayout.Width(itemHeight), GUILayout.Height(itemHeight)))
										{
											UnlinkTransform(subUnit);

											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}

										}
									}
									EditorGUILayout.EndHorizontal();

								}

							}
							break;

						case MAPPING_CATEGORY.Bone:
							{
								//Bone을 그린다.
								int nBones = animFile._bones_Total.Count;
								apRetargetSubUnit subUnit = null;
								for (int i = 0; i < nBones; i++)
								{
									subUnit = animFile._bones_Total[i];
									EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
									GUILayout.Space(10);

									EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Bone), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));

									EditorGUILayout.LabelField(subUnit._name, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));

									//TODO
									// Import를 할 것인지
									// 대상이 무엇인지
									// Change 할 것인지를 결정
									GUILayout.Space(10);
									//"Import", "Import"
									if (apEditorUtil.ToggledButton_2Side(strImport, strImport, subUnit._isImported, true, 70, itemHeight))
									{
										subUnit._isImported = !subUnit._isImported;
									}
									GUILayout.Space(5);

									if (subUnit._isImported)
									{
										bool isSelected = false;
										bool isAvailable = false;
										string strLinkName = subUnit.LinkedName;

										if (_isSelectLinkMode)
										{
											//선택 모드일때
											if (_srcBoneSubUnit == subUnit)
											{
												strLinkName = " >>> ";
												isSelected = true;
												isAvailable = true;
											}
											else
											{
												isSelected = false;
												isAvailable = false;
											}
										}
										else
										{
											//일반 모드일때
											isSelected = subUnit._isImported && subUnit.IsLinked;
											isAvailable = subUnit._isImported;
										}

										if (apEditorUtil.ToggledButton_2Side(strLinkName, strLinkName, isSelected, isAvailable, 150, itemHeight))
										{
											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}
											else
											{
												//연결을 시작하자
												StartSelectLinkMode(subUnit, _mappingCategory);
											}

										}

										if (GUILayout.Button("X", GUILayout.Width(itemHeight), GUILayout.Height(itemHeight)))
										{
											UnlinkBone(subUnit);

											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}

										}
									}


									EditorGUILayout.EndHorizontal();

								}
							}
							break;

						case MAPPING_CATEGORY.ControlParam:
							{
								//Control Param을 그린다.
								int nControlParams = animFile._controlParams.Count;
								apRetargetControlParam cpUnit = null;
								for (int i = 0; i < nControlParams; i++)
								{
									cpUnit = animFile._controlParams[i];
									EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
									GUILayout.Space(10);

									EditorGUILayout.LabelField(new GUIContent("", _imgIcon_ControlParam), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));

									EditorGUILayout.LabelField(cpUnit._keyName, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));

									// Import를 할 것인지
									// 대상이 무엇인지
									// Change 할 것인지를 결정
									GUILayout.Space(10);
									//"Import", "Import"
									if (apEditorUtil.ToggledButton_2Side(strImport, strImport, cpUnit._isImported, true, 70, itemHeight))
									{
										cpUnit._isImported = !cpUnit._isImported;
									}
									GUILayout.Space(5);

									if (cpUnit._isImported)
									{
										bool isSelected = false;
										bool isAvailable = false;
										string strLinkName = cpUnit.LinkedName;

										if (_isSelectLinkMode)
										{
											//선택 모드일때
											if (_srcControlParamUnit == cpUnit)
											{
												strLinkName = " >>> ";
												isSelected = true;
												isAvailable = true;
											}
											else
											{
												isSelected = false;
												isAvailable = false;
											}
										}
										else
										{
											//일반 모드일때
											isSelected = cpUnit._isImported && cpUnit.IsLinked;
											isAvailable = cpUnit._isImported;
										}

										if (apEditorUtil.ToggledButton_2Side(strLinkName, strLinkName, isSelected, isAvailable, 150, itemHeight))
										{
											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}
											else
											{
												//연결을 시작하자
												StartSelectLinkMode(cpUnit, _mappingCategory);
											}

										}

										if (GUILayout.Button("X", GUILayout.Width(itemHeight), GUILayout.Height(itemHeight)))
										{
											UnlinkControlParam(cpUnit);

											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}

										}
									}


									EditorGUILayout.EndHorizontal();

								}
							}
							break;

						case MAPPING_CATEGORY.Timeline:
							{
								//Timeline과 TimelineLayer를 그린다.
								int nTimeline = animFile._timelineUnits.Count;
								apRetargetTimelineUnit timelineUnit = null;
								for (int iT = 0; iT < nTimeline; iT++)
								{
									timelineUnit = animFile._timelineUnits[iT];

									//타임라인을 먼저 출력하고, 
									EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
									GUILayout.Space(5);

									Texture2D iconFold = timelineUnit._isFold ? _imgIcon_FoldRight : _imgIcon_FoldDown;
									if (GUILayout.Button(iconFold, guiStyle_Fold, GUILayout.Width(itemHeight), GUILayout.Height(itemHeight)))
									{
										timelineUnit._isFold = !timelineUnit._isFold;
									}

									string strName = "";
									if (timelineUnit._linkType == apAnimClip.LINK_TYPE.ControlParam)
									{
										EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Timeline_ControlParam), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
										strName = "Control Parameters";
									}
									else
									{
										EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Timeline_AnimModifier), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));

										switch (timelineUnit._linkedModifierType)
										{
											case apModifierBase.MODIFIER_TYPE.AnimatedTF:
												strName = "Transform";
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
												strName = "Morph";
												break;

											case apModifierBase.MODIFIER_TYPE.AnimatedColorOnly://추가 21.7.21
												strName = "Color Only";
												break;

											default:
												break;
										}
									}


									//if(timelineUnit._linkedModifierType == apModifierBase.MODIFIER_TYPE.)
									EditorGUILayout.LabelField(strName, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));

									//TODO
									// Import를 할 것인지
									// 대상이 무엇인지
									// Change 할 것인지를 결정
									GUILayout.Space(10);
									//"Import", "Import"
									if (apEditorUtil.ToggledButton_2Side(strImport, strImport, timelineUnit._isImported, true, 70, itemHeight))
									{
										timelineUnit._isImported = !timelineUnit._isImported;
									}
									GUILayout.Space(5);

									if (timelineUnit._isImported)
									{
										bool isSelected = false;
										bool isAvailable = false;
										string strLinkName = timelineUnit.LinkedName;

										if (_isSelectLinkMode)
										{
											//선택 모드일때
											if (_srcTimelineUnit == timelineUnit)
											{
												strLinkName = " >>> ";
												isSelected = true;
												isAvailable = true;
											}
											else
											{
												isSelected = false;
												isAvailable = false;
											}
										}
										else
										{
											//일반 모드일때
											isSelected = timelineUnit._isImported && timelineUnit.IsLinked;
											isAvailable = timelineUnit._isImported;
										}

										if (apEditorUtil.ToggledButton_2Side(strLinkName, strLinkName, isSelected, isAvailable, 150, itemHeight))
										{
											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}
											else
											{
												//연결을 시작하자
												StartSelectLinkMode(timelineUnit, _mappingCategory);
											}

										}

										if (GUILayout.Button("X", GUILayout.Width(itemHeight), GUILayout.Height(itemHeight)))
										{
											UnlinkTimeline(timelineUnit);

											if (_isSelectLinkMode)
											{
												//연결 중이라면 종료
												EndSelectLinkMode();
											}

										}
									}

									EditorGUILayout.EndHorizontal();

									if (!timelineUnit._isFold)
									{
										//내부 레이어도 그리자
										apRetargetTimelineLayerUnit layerUnit = null;
										for (int i = 0; i < timelineUnit._layerUnits.Count; i++)
										{
											layerUnit = timelineUnit._layerUnits[i];

											EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
											GUILayout.Space(30);
											EditorGUILayout.LabelField("", GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));


											EditorGUILayout.LabelField(layerUnit._displayName, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));

											//TODO
											// Import를 할 것인지
											// 대상이 무엇인지
											// Change 할 것인지를 결정
											GUILayout.Space(10);





											EditorGUILayout.EndHorizontal();
										}
									}

								}
							}
							break;

						case MAPPING_CATEGORY.Event:
							{
								//Animation Event를 그린다.
								int nEvent = animFile._animEvents.Count;
								apRetargetAnimEvent animEvent = null;
								for (int i = 0; i < nEvent; i++)
								{
									animEvent = animFile._animEvents[i];

									EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
									GUILayout.Space(10);

									EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Event), GUILayout.Width(30), GUILayout.Height(itemHeight));

									EditorGUILayout.LabelField("[" + animEvent._frameIndex + "] " + animEvent._eventName, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));

									//TODO
									// Import를 할 것인지
									// 대상이 무엇인지
									// Change 할 것인지를 결정
									GUILayout.Space(10);
									//"Import", "Import"
									if (apEditorUtil.ToggledButton_2Side(strImport, strImport, animEvent._isImported, true, 70, itemHeight))
									{
										animEvent._isImported = !animEvent._isImported;
									}
									GUILayout.Space(5);


									EditorGUILayout.EndHorizontal();
								}
							}
							break;
					}
				}

				EditorGUILayout.EndVertical();
				GUILayout.Space(listHeight + 100);
				EditorGUILayout.EndScrollView();


				//오른쪽 리스트
				//----------------------------
				if (isDrawRightList)
				{
					//2. 오른쪽 스크롤 : 오브젝트, 본 리타겟
					_scrollList_Bone = EditorGUILayout.BeginScrollView(_scrollList_Bone, false, true, GUILayout.Width(rightWidth), GUILayout.Height(listHeight));
					EditorGUILayout.BeginVertical(GUILayout.Width(rightWidth - 30));

					if (_targetMeshGroup != null)
					{
						int itemWidth = rightWidth - 20;
						int itemHeight = 20;

						int linkedWidth = 140;

						//연결이 될 "대상"이다.
						//카테고리에 따라 나오는게 다르다.
						TargetUnit selectedLinkTargetUnit = null;
						switch (_mappingCategory)
						{
							case MAPPING_CATEGORY.Transform:
								{
									//Transform을 그린다.
									//처음 로드할때 리스트를 만들어두자
									TargetUnit targetUnit = null;
									for (int i = 0; i < _targetTransforms.Count; i++)
									{
										targetUnit = _targetTransforms[i];
										EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
										GUILayout.Space(10);

										if (targetUnit._type == TargetUnit.TYPE.MeshTransform)
										{
											EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Mesh), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
										}
										else
										{
											EditorGUILayout.LabelField(new GUIContent("", _imgIcon_MeshGroup), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
										}

										if (targetUnit._isLinked)
										{
											EditorGUILayout.LabelField(targetUnit._name, guiStyle_ItemLabel_Linked, GUILayout.Width(150), GUILayout.Height(itemHeight));
										}
										else
										{
											EditorGUILayout.LabelField(targetUnit._name, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));
										}

										//TODO
										// Import를 할 것인지
										// 대상이 무엇인지
										// Change 할 것인지를 결정
										if (_isSelectLinkMode)
										{
											if (_srcTransformSubUnit != null)
											{
												if ((_srcTransformSubUnit._type == apRetargetSubUnit.TYPE.MeshTransform && targetUnit._meshTransform != null)
													|| (_srcTransformSubUnit._type == apRetargetSubUnit.TYPE.MeshGroupTransform && targetUnit._meshGroupTransform != null))
												{
													//타입이 맞다면 선택 가능
													//string strBtn = "Select";
													string strBtn = strSelect;
													if (targetUnit._linkedSubUnit != null && targetUnit._linkedSubUnit._isImported)
													{
														strBtn = targetUnit._linkedSubUnit._name;
													}

													if (GUILayout.Button(strBtn, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight)))
													{
														//TODO
														selectedLinkTargetUnit = targetUnit;
													}
												}

											}

										}
										else
										{
											if (targetUnit._linkedSubUnit != null && targetUnit._linkedSubUnit._isImported)
											{
												GUILayout.Box(targetUnit._linkedSubUnit._name, guiStyle_Box, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight));
											}
										}

										EditorGUILayout.EndHorizontal();
									}
								}
								break;

							case MAPPING_CATEGORY.Bone:
								{
									//Bone을 그린다.
									//처음 로드할때 리스트를 만들어두자.
									TargetUnit targetUnit = null;
									for (int i = 0; i < _targetBones.Count; i++)
									{
										targetUnit = _targetBones[i];
										EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
										GUILayout.Space(10);

										EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Bone), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));

										if (targetUnit._isLinked)
										{
											EditorGUILayout.LabelField(targetUnit._name, guiStyle_ItemLabel_Linked, GUILayout.Width(150), GUILayout.Height(itemHeight));
										}
										else
										{
											EditorGUILayout.LabelField(targetUnit._name, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));
										}

										//TODO
										// Import를 할 것인지
										// 대상이 무엇인지
										// Change 할 것인지를 결정
										if (_isSelectLinkMode)
										{
											if (_srcBoneSubUnit != null)
											{
												//string strBtn = "Select";
												string strBtn = strSelect;
												if (targetUnit._linkedSubUnit != null && targetUnit._linkedSubUnit._isImported)
												{
													strBtn = targetUnit._linkedSubUnit._name;
												}

												if (GUILayout.Button(strBtn, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight)))
												{
													//TODO
													selectedLinkTargetUnit = targetUnit;
												}
											}

										}
										else
										{
											if (targetUnit._linkedSubUnit != null && targetUnit._linkedSubUnit._isImported)
											{
												GUILayout.Box(targetUnit._linkedSubUnit._name, guiStyle_Box, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight));
											}
										}

										EditorGUILayout.EndHorizontal();
									}
								}
								break;

							case MAPPING_CATEGORY.ControlParam:
								{
									TargetUnit targetUnit = null;
									for (int i = 0; i < _targetControlParams.Count; i++)
									{
										targetUnit = _targetControlParams[i];

										EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
										GUILayout.Space(10);

										EditorGUILayout.LabelField(new GUIContent("", _imgIcon_ControlParam), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));

										if (targetUnit._isLinked)
										{
											EditorGUILayout.LabelField(targetUnit._name, guiStyle_ItemLabel_Linked, GUILayout.Width(150), GUILayout.Height(itemHeight));
										}
										else
										{
											EditorGUILayout.LabelField(targetUnit._name, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));
										}

										//TODO
										// Import를 할 것인지
										// 대상이 무엇인지
										// Change 할 것인지를 결정
										if (_isSelectLinkMode)
										{
											if (_srcControlParamUnit != null
												&& _srcControlParamUnit._valueType == targetUnit._controlParam._valueType)
											{
												//string strBtn = "Select";
												string strBtn = strSelect;
												if (targetUnit._linkedControlParam != null && targetUnit._linkedControlParam._isImported)
												{
													strBtn = targetUnit._linkedControlParam._keyName;
												}

												if (GUILayout.Button(strBtn, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight)))
												{
													//TODO
													selectedLinkTargetUnit = targetUnit;
												}
											}

										}
										else
										{
											if (targetUnit._linkedControlParam != null && targetUnit._linkedControlParam._isImported)
											{
												GUILayout.Box(targetUnit._linkedControlParam._keyName, guiStyle_Box, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight));
											}
										}

										EditorGUILayout.EndHorizontal();
									}
								}
								break;

							case MAPPING_CATEGORY.Timeline:
								{
									TargetUnit targetUnit = null;
									TargetUnit targetUnitLayer = null;
									for (int i = 0; i < _targetTimelines.Count; i++)
									{
										targetUnit = _targetTimelines[i];

										EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
										GUILayout.Space(10);

										Texture2D iconFold = targetUnit._isFold ? _imgIcon_FoldRight : _imgIcon_FoldDown;
										if (GUILayout.Button(iconFold, guiStyle_Fold, GUILayout.Width(itemHeight), GUILayout.Height(itemHeight)))
										{
											targetUnit._isFold = !targetUnit._isFold;
										}

										if (targetUnit._type == TargetUnit.TYPE.Timeline)
										{
											if (targetUnit._timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
											{
												EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Timeline_AnimModifier), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
											}
											else
											{
												EditorGUILayout.LabelField(new GUIContent("", _imgIcon_Timeline_ControlParam), GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
											}
										}
										else
										{
											EditorGUILayout.LabelField("", GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));
										}

										EditorGUILayout.LabelField(targetUnit._name, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));

										//TODO
										// Import를 할 것인지
										// 대상이 무엇인지
										// Change 할 것인지를 결정
										if (_isSelectLinkMode)
										{
											if (_srcTimelineUnit != null)
											{
												//1. Timeline 선택 중일때
												//타입이 맞아야 한다.
												if (targetUnit._type == TargetUnit.TYPE.Timeline
														&& _srcTimelineUnit._linkType == targetUnit._timeline._linkType)
												{
													//string strBtn = "Select";
													string strBtn = strSelect;
													if (targetUnit._linkedTimelineUnit != null && targetUnit._linkedTimelineUnit._isImported)
													{
														strBtn = targetUnit._linkedTimelineUnit.LinkedName;
													}

													if (GUILayout.Button(strBtn, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight)))
													{
														//TODO
														selectedLinkTargetUnit = targetUnit;
													}
												}
											}

										}
										else
										{
											if (targetUnit._type == TargetUnit.TYPE.Timeline
												&& targetUnit._linkedTimelineUnit != null
												&& targetUnit._linkedTimelineUnit._isImported)
											{
												GUILayout.Box(targetUnit._linkedTimelineUnit.LinkedName, guiStyle_Box, GUILayout.Width(linkedWidth), GUILayout.Height(itemHeight));
											}
											//else if(targetUnit._type == TargetUnit.TYPE.TimelineLayer
											//	&& targetUnit._linkedTimelineLayerUnit != null)
											//{
											//	GUILayout.Box(targetUnit._linkedTimelineLayerUnit.LinkedControlParamName, guiStyle_Box, GUILayout.Width(120), GUILayout.Height(itemHeight));
											//}
										}

										EditorGUILayout.EndHorizontal();

										if (!targetUnit._isFold)
										{
											//레이어를 출력하자
											List<TargetUnit> timelineLayers = _targetTimelineLayers[targetUnit];

											for (int iL = 0; iL < timelineLayers.Count; iL++)
											{
												targetUnitLayer = timelineLayers[iL];
												EditorGUILayout.BeginHorizontal(GUILayout.Width(itemWidth), GUILayout.Height(itemHeight));
												GUILayout.Space(30);

												EditorGUILayout.LabelField("", GUILayout.Width(itemHeight), GUILayout.Height(itemHeight));

												EditorGUILayout.LabelField(targetUnitLayer._name, guiStyle_ItemLabel, GUILayout.Width(150), GUILayout.Height(itemHeight));

												EditorGUILayout.EndHorizontal();
											}
										}
									}
								}
								break;

							case MAPPING_CATEGORY.Event:
								//Event는 그리지 않는다.
								break;
						}

						if (selectedLinkTargetUnit != null)
						{
							//Link 선택을 했다.
							SelectTargetUnit(selectedLinkTargetUnit);
						}
					}

					EditorGUILayout.EndVertical();
					GUILayout.Space(listHeight + 100);
					EditorGUILayout.EndScrollView();
				}

				EditorGUILayout.EndHorizontal();


				GUILayout.Space(20);

				// - 전체 선택 / 해제
				// - 전체 IK 포함 여부, 
				// - 옵션 : 크기
				int widthBottom = ((width - 20) / 5) - 2;
				int widthBottom2 = ((width - 20) / 4);

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				string strEnableObjectsType = "";
				switch (_mappingCategory)
				{
					case MAPPING_CATEGORY.Transform:
						//strEnableObjectsType = "Mesh / MeshGroup";
						strEnableObjectsType = _editor.GetText(TEXT.DLG_MeshesMeshGroups);
						break;

					case MAPPING_CATEGORY.Bone:
						//strEnableObjectsType = "Bones";
						strEnableObjectsType = _editor.GetText(TEXT.DLG_Bones);
						break;

					case MAPPING_CATEGORY.ControlParam:
						//strEnableObjectsType = "Control Parameters";
						strEnableObjectsType = _editor.GetText(TEXT.DLG_ControlParameters);
						break;

					case MAPPING_CATEGORY.Event:
						//strEnableObjectsType = "Events";
						strEnableObjectsType = _editor.GetText(TEXT.DLG_AnimEvents);
						break;

					case MAPPING_CATEGORY.Timeline:
						//strEnableObjectsType = "Timelines";
						strEnableObjectsType = _editor.GetText(TEXT.DLG_Timelines);
						break;
				}

				if (_mappingCategory != MAPPING_CATEGORY.Event)
				{
					//"Auto Mapping " + strEnableObjectsType
					if (apEditorUtil.ToggledButton(string.Format("{0} {1}", _editor.GetText(TEXT.DLG_AutoMapping), strEnableObjectsType), false, isFileLoaded, widthBottom2 + 30, 25))
					{
						AutoMapping(_mappingCategory);
					}
				}
				else
				{
					//Event는 Auto Mapping이 없다.
					GUILayout.Space(widthBottom2 + 30 + 4);
				}

				GUILayout.Space(13 + widthBottom2 - 30);
				//"Enable " + strEnableObjectsType
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Enable) + " " + strEnableObjectsType, false, isFileLoaded, widthBottom2, 25))
				{
					EnableObjects(_mappingCategory);
				}
				//"Disable " + strEnableObjectsType
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Disable) + " " + strEnableObjectsType, false, isFileLoaded, widthBottom2, 25))
				{
					DisableObjects(_mappingCategory);
				}
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_AutoMappingAll), false, isFileLoaded, widthBottom, 25))//"Auto Mapping All"
				{
					AutoMappingAll();
				}
				GUILayout.Space(5);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_SaveMapping), false, isFileLoaded, widthBottom, 25))//"Save Mapping"
				{
					SaveMapping();
				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_LoadMapping), false, isFileLoaded, widthBottom, 25))//"Load Mapping"
				{
					LoadMapping();
				}
				GUILayout.Space(8);
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_EnableAll), false, isFileLoaded, widthBottom, 25))//"Enable All"
				{
					EnableAll();
				}
				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_DisableAll), false, isFileLoaded, widthBottom, 25))//"Disable All"
				{
					DisableAll();
				}
				EditorGUILayout.EndHorizontal();





				GUILayout.Space(10);

				//int widthValue = width - 155;

				bool isClose = false;
				bool isSelectBtnAvailable = _retargetData.IsAnimFileLoaded;//<<TODO : 파일을 연게 있다면 이게 true

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(30));
				GUILayout.Space(5);

				//"  Import Merge"
				if (apEditorUtil.ToggledButton(_editor.ImageSet.Get(apImageSet.PRESET.Anim_Load), "  " + _editor.GetText(TEXT.DLG_ImportMerge), false, isSelectBtnAvailable, (width - 10) / 2, 30))
				{
					bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.Retarget_ImportAnim_Title),
																_editor.GetText(TEXT.Retarget_ImportAnimMerge_Body),
																_editor.GetText(TEXT.Import),
																_editor.GetText(TEXT.Cancel));

					if (isResult)
					{
						_funcResult(true, _loadKey, _retargetData, _targetMeshGroup, _targetAnimClip, true);
						isClose = true;
					}
				}

				//"  Import Replace"
				if (apEditorUtil.ToggledButton(_editor.ImageSet.Get(apImageSet.PRESET.Anim_Load), "  " + _editor.GetText(TEXT.DLG_ImportReplace), false, isSelectBtnAvailable, (width - 10) / 2, 30))
				{
					bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.Retarget_ImportAnim_Title),
																_editor.GetText(TEXT.Retarget_ImportAnimReplace_Body),
																_editor.GetText(TEXT.Import),
																_editor.GetText(TEXT.Cancel));

					if (isResult)
					{
						_funcResult(true, _loadKey, _retargetData, _targetMeshGroup, _targetAnimClip, false);
						isClose = true;
					}
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(10);
				apEditorUtil.GUI_DelimeterBoxH(width);
				GUILayout.Space(10);

				if (apEditorUtil.ToggledButton(_editor.GetText(TEXT.DLG_Close), false, true, width, 30))//"Close"
				{
					//_funcResult(false, _loadKey, null, null);
					_funcResult(false, _loadKey, null, _targetMeshGroup, _targetAnimClip, false);
					isClose = true;
				}

				if (isClose)
				{
					CloseDialog();
				}
			}
			catch(Exception ex)
			{
				//추가 21.3.17 : Try-Catch 추가. Mac에서 에러가 발생하기 쉽다.
				Debug.LogError("AnyPortrait : Exception occurs : " + ex);
			}
		}

		private void StartSelectLinkMode(apRetargetSubUnit subUnit, MAPPING_CATEGORY category)
		{
			EndSelectLinkMode();

			_isSelectLinkMode = true;
			_selectCategory = category;
			if(_selectCategory == MAPPING_CATEGORY.Transform)
			{
				_srcTransformSubUnit = subUnit;
			}
			else if(_selectCategory == MAPPING_CATEGORY.Bone)
			{
				_srcBoneSubUnit = subUnit;
			}
			else
			{
				Debug.LogError("AnyPortrait : Wrong SelectLinkMode");
				EndSelectLinkMode();
				return;
			}
			
		}

		private void StartSelectLinkMode(apRetargetControlParam controlParamUnit, MAPPING_CATEGORY category)
		{
			EndSelectLinkMode();

			_isSelectLinkMode = true;
			_selectCategory = category;
			_srcControlParamUnit = controlParamUnit;
		}

		private void StartSelectLinkMode(apRetargetTimelineUnit timelineUnit, MAPPING_CATEGORY category)
		{
			EndSelectLinkMode();

			_isSelectLinkMode = true;
			_selectCategory = category;
			_srcTimelineUnit = timelineUnit;
		}

		//private void StartSelectLinkMode(apRetargetTimelineLayerUnit timelineLayerUnit, MAPPING_CATEGORY category)
		//{
		//	EndSelectLinkMode();

		//	_isSelectLinkMode = true;
		//	_selectCategory = category;
		//	_srcTimelineLayerUnit = timelineLayerUnit;
		//}




		private void CheckSelectLinkMode()
		{
			if(_isSelectLinkMode)
			{
				if(_selectCategory != _mappingCategory)
				{
					EndSelectLinkMode();
				}
				else
				{
					switch (_selectCategory)
					{
						case MAPPING_CATEGORY.Transform:
							if(_srcTransformSubUnit == null)
							{
								EndSelectLinkMode();
							}
							break;

						case MAPPING_CATEGORY.Bone:
							if (_srcBoneSubUnit == null)
							{
								EndSelectLinkMode();
							}
							break;

						case MAPPING_CATEGORY.ControlParam:
							if(_srcControlParamUnit == null)
							{
								EndSelectLinkMode();
							}
							break;
						case MAPPING_CATEGORY.Timeline:
							if(_srcTimelineUnit == null)
							{
								EndSelectLinkMode();
							}
							break;

						default:
							EndSelectLinkMode();
							break;
					}
				}
			}
		}

		private void EndSelectLinkMode()
		{
			_isSelectLinkMode = false;
			_selectCategory = MAPPING_CATEGORY.Bone;
			_srcTransformSubUnit = null;
			_srcBoneSubUnit = null;
			_srcTimelineUnit = null;
			//_srcTimelineLayerUnit = null;
			_srcControlParamUnit = null;
			//_dstSubUnit = null;
		}


		private void SelectTargetUnit(TargetUnit targetUnit)
		{
			if (_retargetData.AnimFile != null &&
				_retargetData.AnimFile.IsLoaded)
			{
				
				switch (targetUnit._type)
				{
					case TargetUnit.TYPE.MeshTransform:
						{
							if (_srcTransformSubUnit != null &&
								_srcTransformSubUnit._type == apRetargetSubUnit.TYPE.MeshTransform)
							{
								//연결을 해주자
								//그 전에,
								//이 MeshTransform을 가진 다른 SubUnit을 해제한다.
								apRetargetSubUnit subUnit = null;
								for (int i = 0; i < _retargetData.AnimFile._transforms_Total.Count; i++)
								{
									subUnit = _retargetData.AnimFile._transforms_Total[i];

									if(subUnit == _srcTransformSubUnit)
									{
										targetUnit._linkedSubUnit = subUnit;
										targetUnit._isLinked = true;
										subUnit._targetMeshTransform = targetUnit._meshTransform;
									}
									else
									{
										if(subUnit._targetMeshTransform == targetUnit._meshTransform)
										{
											//같은 MeshTransform을 참조했다면 링크를 끊자
											subUnit._targetMeshTransform = null;
										}
									}
								}

								//링크를 Refresh한다.
								Dictionary<apTransform_Mesh, apRetargetSubUnit> selectedMeshTransforms = new Dictionary<apTransform_Mesh, apRetargetSubUnit>();
								for (int i = 0; i < _retargetData.AnimFile._transforms_Total.Count; i++)
								{
									subUnit = _retargetData.AnimFile._transforms_Total[i];
									if(subUnit._targetMeshTransform != null)
									{
										selectedMeshTransforms.Add(subUnit._targetMeshTransform, subUnit);
									}
								}

								for (int i = 0; i < _targetTransforms.Count; i++)
								{
									if(_targetTransforms[i]._type == TargetUnit.TYPE.MeshTransform)
									{
										if (selectedMeshTransforms.ContainsKey(_targetTransforms[i]._meshTransform))
										{
											apRetargetSubUnit linkedUnit = selectedMeshTransforms[_targetTransforms[i]._meshTransform];
											if (linkedUnit._targetMeshTransform == _targetTransforms[i]._meshTransform)
											{
												_targetTransforms[i]._linkedSubUnit = linkedUnit;
												_targetTransforms[i]._isLinked = true;
											}
											else
											{
												_targetTransforms[i]._isLinked = false;
												_targetTransforms[i]._linkedSubUnit = null;
											}
										}
										else
										{
											_targetTransforms[i]._isLinked = false;
											_targetTransforms[i]._linkedSubUnit = null;
										}
									}	
								}
							}
						}

						break;


					case TargetUnit.TYPE.MeshGroupTransform:
						{
							if (_srcTransformSubUnit != null &&
								_srcTransformSubUnit._type == apRetargetSubUnit.TYPE.MeshGroupTransform)
							{
								//연결을 해주자
								//그 전에,
								//이 MeshGroupTransform을 가진 다른 SubUnit을 해제한다.
								apRetargetSubUnit subUnit = null;
								for (int i = 0; i < _retargetData.AnimFile._transforms_Total.Count; i++)
								{
									subUnit = _retargetData.AnimFile._transforms_Total[i];

									if(subUnit == _srcTransformSubUnit)
									{
										targetUnit._linkedSubUnit = subUnit;
										targetUnit._isLinked = true;
										subUnit._targetMeshGroupTransform = targetUnit._meshGroupTransform;
									}
									else
									{
										if(subUnit._targetMeshGroupTransform == targetUnit._meshGroupTransform)
										{
											//같은 MeshTransform을 참조했다면 링크를 끊자
											subUnit._targetMeshGroupTransform = null;
										}
									}
								}

								//링크를 Refresh한다.
								Dictionary<apTransform_MeshGroup, apRetargetSubUnit> selectedMeshGroupTransforms = new Dictionary<apTransform_MeshGroup, apRetargetSubUnit>();
								for (int i = 0; i < _retargetData.AnimFile._transforms_Total.Count; i++)
								{
									subUnit = _retargetData.AnimFile._transforms_Total[i];
									if(subUnit._targetMeshGroupTransform != null)
									{
										selectedMeshGroupTransforms.Add(subUnit._targetMeshGroupTransform, subUnit);
									}
								}

								for (int i = 0; i < _targetTransforms.Count; i++)
								{
									if(_targetTransforms[i]._type == TargetUnit.TYPE.MeshGroupTransform)
									{
										if (selectedMeshGroupTransforms.ContainsKey(_targetTransforms[i]._meshGroupTransform))
										{
											apRetargetSubUnit linkedUnit = selectedMeshGroupTransforms[_targetTransforms[i]._meshGroupTransform];
											if (linkedUnit._targetMeshGroupTransform == _targetTransforms[i]._meshGroupTransform)
											{
												_targetTransforms[i]._linkedSubUnit = linkedUnit;
												_targetTransforms[i]._isLinked = true;
											}
											else
											{
												_targetTransforms[i]._isLinked = false;
												_targetTransforms[i]._linkedSubUnit = null;
											}
										}
										else
										{
											_targetTransforms[i]._isLinked = false;
											_targetTransforms[i]._linkedSubUnit = null;
										}
									}	
								}
							}
						}
						break;

					case TargetUnit.TYPE.Bone:
						{
							if (_srcBoneSubUnit != null)
							{
								//연결을 해주자
								//그 전에,
								//이 Bone을 가진 다른 SubUnit을 해제한다.
								apRetargetSubUnit subUnit = null;
								for (int i = 0; i < _retargetData.AnimFile._bones_Total.Count; i++)
								{
									subUnit = _retargetData.AnimFile._bones_Total[i];

									if(subUnit == _srcBoneSubUnit)
									{
										targetUnit._linkedSubUnit = subUnit;
										targetUnit._isLinked = true;
										subUnit._targetBone = targetUnit._bone;
									}
									else
									{
										if(subUnit._targetBone == targetUnit._bone)
										{
											//같은 MeshTransform을 참조했다면 링크를 끊자
											subUnit._targetBone = null;
										}
									}
								}

								//링크를 Refresh한다.
								Dictionary<apBone, apRetargetSubUnit> selectedBones = new Dictionary<apBone, apRetargetSubUnit>();
								for (int i = 0; i < _retargetData.AnimFile._bones_Total.Count; i++)
								{
									subUnit = _retargetData.AnimFile._bones_Total[i];
									if(subUnit._targetBone != null)
									{
										selectedBones.Add(subUnit._targetBone, subUnit);
									}
								}

								for (int i = 0; i < _targetBones.Count; i++)
								{
									if (selectedBones.ContainsKey(_targetBones[i]._bone))
									{
										apRetargetSubUnit linkedUnit = selectedBones[_targetBones[i]._bone];
										if (linkedUnit._targetBone == _targetBones[i]._bone)
										{
											_targetBones[i]._linkedSubUnit = linkedUnit;
											_targetBones[i]._isLinked = true;
										}
										else
										{
											_targetBones[i]._isLinked = false;
											_targetBones[i]._linkedSubUnit = null;
										}
									}
									else
									{
										_targetBones[i]._isLinked = false;
										_targetBones[i]._linkedSubUnit = null;
									}	
								}
							}
						}
						break;

					case TargetUnit.TYPE.ControlParam:
						{
							if (_srcControlParamUnit != null)
							{
								//연결을 해주자
								//그 전에,
								//이 ControlParam을 가진 다른 SubUnit을 해제한다.
								apRetargetControlParam cpUnit = null;
								for (int i = 0; i < _retargetData.AnimFile._controlParams.Count; i++)
								{
									cpUnit = _retargetData.AnimFile._controlParams[i];

									if(cpUnit == _srcControlParamUnit)
									{
										targetUnit._linkedControlParam = cpUnit;
										targetUnit._isLinked = true;
										cpUnit._targetControlParam = targetUnit._controlParam;
									}
									else
									{
										if(cpUnit._targetControlParam == targetUnit._controlParam)
										{
											//같은 ControlParam을 참조했다면 링크를 끊자
											cpUnit._targetControlParam = null;
										}
									}
								}

								//링크를 Refresh한다.
								Dictionary<apControlParam, apRetargetControlParam> selectedControlParams = new Dictionary<apControlParam, apRetargetControlParam> ();
								for (int i = 0; i < _retargetData.AnimFile._controlParams.Count; i++)
								{
									cpUnit = _retargetData.AnimFile._controlParams[i];
									if(cpUnit._targetControlParam != null)
									{
										selectedControlParams.Add(cpUnit._targetControlParam, cpUnit);
									}
								}

								for (int i = 0; i < _targetControlParams.Count; i++)
								{
									if (selectedControlParams.ContainsKey(_targetControlParams[i]._controlParam))
									{
										apRetargetControlParam linkedUnit = selectedControlParams[_targetControlParams[i]._controlParam];
										if (linkedUnit._targetControlParam == _targetControlParams[i]._controlParam)
										{
											_targetControlParams[i]._linkedControlParam = linkedUnit;
											_targetControlParams[i]._isLinked = true;
										}
										else
										{
											_targetControlParams[i]._isLinked = false;
											_targetControlParams[i]._linkedControlParam = null;
										}
									}
									else
									{
										_targetControlParams[i]._isLinked = false;
										_targetControlParams[i]._linkedControlParam = null;
									}	
								}
							}
						}
						break;

					case TargetUnit.TYPE.Timeline:
						{
							if(_srcTimelineUnit != null
								&& targetUnit._timeline._linkType == _srcTimelineUnit._linkType)//타입도 같아야 한다.
							{
								
								//연결을 해주자
								//기존 연결이 있다면 TimelineLayer를 포함해서 해제해야한다.
								apRetargetTimelineUnit srcTimelineUnit = null;

								for (int i = 0; i < _retargetData.AnimFile._timelineUnits.Count; i++)
								{
									srcTimelineUnit = _retargetData.AnimFile._timelineUnits[i];

									if(srcTimelineUnit == _srcTimelineUnit)
									{
										targetUnit._linkedTimelineUnit = srcTimelineUnit;
										targetUnit._isLinked = true;
										srcTimelineUnit._targetTimeline = targetUnit._timeline;
									}
									else
									{
										if(srcTimelineUnit._targetTimeline == targetUnit._timeline)
										{
											//같은 ControlParam을 참조했다면 링크를 끊자
											srcTimelineUnit._targetTimeline = null;
										}
									}
								}

								//링크를 Refresh한다.
								Dictionary<apAnimTimeline, apRetargetTimelineUnit> selectedTimelines = new Dictionary<apAnimTimeline, apRetargetTimelineUnit> ();
								for (int i = 0; i < _retargetData.AnimFile._timelineUnits.Count; i++)
								{
									srcTimelineUnit = _retargetData.AnimFile._timelineUnits[i];
									if(srcTimelineUnit._targetTimeline != null)
									{
										selectedTimelines.Add(srcTimelineUnit._targetTimeline, srcTimelineUnit);
									}
								}
								TargetUnit curTargetUnit = null;
								for (int i = 0; i < _targetTimelines.Count; i++)
								{
									curTargetUnit = _targetTimelines[i];
									if(curTargetUnit._type != TargetUnit.TYPE.Timeline)
									{
										continue;
									}
									if (selectedTimelines.ContainsKey(curTargetUnit._timeline))
									{
										apRetargetTimelineUnit linkedUnit = selectedTimelines[curTargetUnit._timeline];
										if (linkedUnit._targetTimeline == curTargetUnit._timeline)
										{
											curTargetUnit._linkedTimelineUnit = linkedUnit;
											curTargetUnit._isLinked = true;
										}
										else
										{
											curTargetUnit._isLinked = false;
											curTargetUnit._linkedTimelineUnit = null;
										}
									}
									else
									{
										curTargetUnit._isLinked = false;
										curTargetUnit._linkedTimelineUnit = null;
									}	
								}
							}
						}
						break;

					case TargetUnit.TYPE.TimelineLayer:
						break;
				}
			}

			EndSelectLinkMode();
		}



		private void UnlinkTransform(apRetargetSubUnit subUnit)
		{
			subUnit._targetMeshTransform = null;
			subUnit._targetMeshGroupTransform = null;

			TargetUnit targetUnit = null;
			for (int i = 0; i < _targetTransforms.Count; i++)
			{
				targetUnit = _targetTransforms[i];
				if(targetUnit._linkedSubUnit == subUnit)
				{
					//링크를 끊자
					targetUnit._isLinked = false;
					targetUnit._linkedSubUnit = null;
				}
			}
		}

		private void UnlinkBone(apRetargetSubUnit subUnit)
		{
			subUnit._targetBone = null;

			TargetUnit targetUnit = null;
			for (int i = 0; i < _targetBones.Count; i++)
			{
				targetUnit = _targetBones[i];
				if(targetUnit._linkedSubUnit == subUnit)
				{
					//링크를 끊자
					targetUnit._isLinked = false;
					targetUnit._linkedSubUnit = null;
				}
			}
		}

		private void UnlinkControlParam(apRetargetControlParam cpUnit)
		{
			cpUnit._targetControlParam = null;

			TargetUnit targetUnit = null;
			for (int i = 0; i < _targetControlParams.Count; i++)
			{
				targetUnit = _targetControlParams[i];
				if(targetUnit._linkedControlParam == cpUnit)
				{
					//링크를 끊자
					targetUnit._isLinked = false;
					targetUnit._linkedControlParam = null;
				}
			}
		}

		private void UnlinkTimeline(apRetargetTimelineUnit tlUnit)
		{
			tlUnit._targetTimeline = null;

			TargetUnit targetUnit = null;
			for (int i = 0; i < _targetTimelines.Count; i++)
			{
				targetUnit = _targetTimelines[i];
				if(targetUnit._linkedTimelineUnit == tlUnit)
				{
					//링크를 끊자
					targetUnit._isLinked = false;
					targetUnit._linkedTimelineUnit = null;
				}
			}
		}


		

		//---------------------------------------------------------------
		// 주요 함수들
		//---------------------------------------------------------------
		private void AutoMappingAll()
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}
			bool isResult = EditorUtility.DisplayDialog(
				_editor.GetText(TEXT.Retarget_AutoMapping_Title),
				_editor.GetText(TEXT.Retarget_AutoMapping_Body),
				_editor.GetText(TEXT.Retarget_AutoMapping),
				_editor.GetText(TEXT.Cancel)
				);

			if(!isResult)
			{
				return;
			}

			_mapping.AutoMapping_Transform(_retargetData.AnimFile, _targetTransforms);
			_mapping.AutoMapping_Bone(_retargetData.AnimFile, _targetBones);
			_mapping.AutoMapping_ControlParam(_retargetData.AnimFile, _targetControlParams);
			_mapping.AutoMapping_Timeline(_retargetData.AnimFile, _targetTimelines);
		}

		public void SaveMapping()
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}

			string filePath = EditorUtility.SaveFilePanel("Save Mapping", apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport), "", "amp");
			if(!string.IsNullOrEmpty(filePath))
			{
				//추가 21.7.3 : 이스케이프 문자 삭제
				filePath = apUtil.ConvertEscapeToPlainText(filePath);

				apEditorUtil.SetLastExternalOpenSaveFilePath(filePath, apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport);//추가 21.3.1

				_mapping.SaveMapping(filePath, _retargetData.AnimFile);
			}
		}

		private void LoadMapping()
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}

			string filePath = EditorUtility.OpenFilePanel("Load Mapping", apEditorUtil.GetLastOpenSaveFileDirectoryPath(apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport), "amp");
			if(!string.IsNullOrEmpty(filePath))
			{
				//추가 21.7.3 : 이스케이프 문자 삭제
				filePath = apUtil.ConvertEscapeToPlainText(filePath);

				apEditorUtil.SetLastExternalOpenSaveFilePath(filePath, apEditorUtil.SAVED_LAST_FILE_PATH.BoneAnimExport);//21.3.1
				_mapping.LoadMapping(filePath, _retargetData.AnimFile, _targetTransforms, _targetBones, _targetControlParams, _targetTimelines);
			}
		}

		private void EnableAll(bool isDialog = true)
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}
			if (isDialog)
			{
				bool isResult = EditorUtility.DisplayDialog(
					_editor.GetText(TEXT.Retarget_EnableAll_Title),
					_editor.GetText(TEXT.Retarget_EnableAll_Body),
					_editor.GetText(TEXT.Okay),
					_editor.GetText(TEXT.Cancel)
					);

				if (!isResult)
				{
					return;
				}
			}

			for (int i = 0; i < _retargetData.AnimFile._transforms_Total.Count; i++)
			{
				_retargetData.AnimFile._transforms_Total[i]._isImported = true;
			}
			for (int i = 0; i < _retargetData.AnimFile._bones_Total.Count; i++)
			{
				_retargetData.AnimFile._bones_Total[i]._isImported = true;
			}
			for (int i = 0; i < _retargetData.AnimFile._controlParams.Count; i++)
			{
				_retargetData.AnimFile._controlParams[i]._isImported = true;
			}
			for (int i = 0; i < _retargetData.AnimFile._timelineUnits.Count; i++)
			{
				_retargetData.AnimFile._timelineUnits[i]._isImported = true;
			}
			//List<apRetargetAnimEvent> events = _retargetData.AnimFile._animEvents;
			for (int i = 0; i < _retargetData.AnimFile._animEvents.Count; i++)
			{
				_retargetData.AnimFile._animEvents[i]._isImported = true;
			}
		}

		private void DisableAll()
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}
			bool isResult = EditorUtility.DisplayDialog(
				_editor.GetText(TEXT.Retarget_DisableAll_Title),
				_editor.GetText(TEXT.Retarget_DisableAll_Body),
				_editor.GetText(TEXT.Okay),
				_editor.GetText(TEXT.Cancel)
				);

			if(!isResult)
			{
				return;
			}

			for (int i = 0; i < _retargetData.AnimFile._transforms_Total.Count; i++)
			{
				_retargetData.AnimFile._transforms_Total[i]._isImported = false;
			}
			for (int i = 0; i < _retargetData.AnimFile._bones_Total.Count; i++)
			{
				_retargetData.AnimFile._bones_Total[i]._isImported = false;
			}
			for (int i = 0; i < _retargetData.AnimFile._controlParams.Count; i++)
			{
				_retargetData.AnimFile._controlParams[i]._isImported = false;
			}
			for (int i = 0; i < _retargetData.AnimFile._timelineUnits.Count; i++)
			{
				_retargetData.AnimFile._timelineUnits[i]._isImported = false;
			}
			//List<apRetargetAnimEvent> events = _retargetData.AnimFile._animEvents;
			for (int i = 0; i < _retargetData.AnimFile._animEvents.Count; i++)
			{
				_retargetData.AnimFile._animEvents[i]._isImported = false;
			}
		}

		private void AutoMapping(MAPPING_CATEGORY category)
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}
			bool isResult = EditorUtility.DisplayDialog(
				_editor.GetText(TEXT.Retarget_AutoMapping_Title),
				_editor.GetTextFormat(TEXT.Retarget_AutoMappingPart_Body, GetCategoryName(category)),
				_editor.GetText(TEXT.Retarget_AutoMapping),
				_editor.GetText(TEXT.Cancel)
				);

			if(!isResult)
			{
				return;
			}

			switch (_mappingCategory)
			{
				case MAPPING_CATEGORY.Transform:
					_mapping.AutoMapping_Transform(_retargetData.AnimFile, _targetTransforms);
					break;

				case MAPPING_CATEGORY.Bone:
					_mapping.AutoMapping_Bone(_retargetData.AnimFile, _targetBones);
					break;

				case MAPPING_CATEGORY.ControlParam:
					_mapping.AutoMapping_ControlParam(_retargetData.AnimFile, _targetControlParams);
					break;

				case MAPPING_CATEGORY.Timeline:
					_mapping.AutoMapping_Timeline(_retargetData.AnimFile, _targetTimelines);
					break;
			}

			
		}

		private void EnableObjects(MAPPING_CATEGORY category)
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}
			bool isResult = EditorUtility.DisplayDialog(
				_editor.GetText(TEXT.Retarget_EnableAll_Title),
				_editor.GetTextFormat(TEXT.Retarget_EnablePart_Body, GetCategoryName(category)),
				_editor.GetText(TEXT.Okay),
				_editor.GetText(TEXT.Cancel)
				);

			if(!isResult)
			{
				return;
			}

			switch (category)
			{
				case MAPPING_CATEGORY.Transform:
					{
						List<apRetargetSubUnit> subUnits = _retargetData.AnimFile._transforms_Total;
						for (int i = 0; i < subUnits.Count; i++)
						{
							subUnits[i]._isImported = true;
						}
					}
					break;

				case MAPPING_CATEGORY.Bone:
					{
						List<apRetargetSubUnit> subUnits = _retargetData.AnimFile._bones_Total;
						for (int i = 0; i < subUnits.Count; i++)
						{
							subUnits[i]._isImported = true;
						}
					}
					break;

				case MAPPING_CATEGORY.ControlParam:
					{
						List<apRetargetControlParam> cpUnits = _retargetData.AnimFile._controlParams;
						for (int i = 0; i < cpUnits.Count; i++)
						{
							cpUnits[i]._isImported = true;
						}
					}
					break;

				case MAPPING_CATEGORY.Timeline:
					{
						List<apRetargetTimelineUnit> timelines = _retargetData.AnimFile._timelineUnits;
						for (int i = 0; i < timelines.Count; i++)
						{
							timelines[i]._isImported = true;
						}
					}
					break;

				case MAPPING_CATEGORY.Event:
					{
						List<apRetargetAnimEvent> events = _retargetData.AnimFile._animEvents;
						for (int i = 0; i < events.Count; i++)
						{
							events[i]._isImported = true;
						}
					}
					break;
			}
		}
		
		private void DisableObjects(MAPPING_CATEGORY category)
		{
			if(!_retargetData.IsAnimFileLoaded)
			{
				return;
			}
			bool isResult = EditorUtility.DisplayDialog(
				_editor.GetText(TEXT.Retarget_DisableAll_Title),
				_editor.GetTextFormat(TEXT.Retarget_DisablePart_Body, GetCategoryName(category)),
				_editor.GetText(TEXT.Okay),
				_editor.GetText(TEXT.Cancel)
				);

			if(!isResult)
			{
				return;
			}

			switch (category)
			{
				case MAPPING_CATEGORY.Transform:
					{
						List<apRetargetSubUnit> subUnits = _retargetData.AnimFile._transforms_Total;
						for (int i = 0; i < subUnits.Count; i++)
						{
							subUnits[i]._isImported = false;
						}
					}
					break;

				case MAPPING_CATEGORY.Bone:
					{
						List<apRetargetSubUnit> subUnits = _retargetData.AnimFile._bones_Total;
						for (int i = 0; i < subUnits.Count; i++)
						{
							subUnits[i]._isImported = false;
						}
					}
					break;

				case MAPPING_CATEGORY.ControlParam:
					{
						List<apRetargetControlParam> cpUnits = _retargetData.AnimFile._controlParams;
						for (int i = 0; i < cpUnits.Count; i++)
						{
							cpUnits[i]._isImported = false;
						}
					}
					break;

				case MAPPING_CATEGORY.Timeline:
					{
						List<apRetargetTimelineUnit> timelines = _retargetData.AnimFile._timelineUnits;
						for (int i = 0; i < timelines.Count; i++)
						{
							timelines[i]._isImported = false;
						}
					}
					break;

				case MAPPING_CATEGORY.Event:
					{
						List<apRetargetAnimEvent> events = _retargetData.AnimFile._animEvents;
						for (int i = 0; i < events.Count; i++)
						{
							events[i]._isImported = false;
						}
					}
					break;
			}
		}		

		private string GetCategoryName(MAPPING_CATEGORY category)
		{
			switch (category)
			{
				case MAPPING_CATEGORY.Transform:		return "Mesh/MeshGroups";
				case MAPPING_CATEGORY.Bone:				return "Bones";
				case MAPPING_CATEGORY.ControlParam:		return "Control Parameters";
				case MAPPING_CATEGORY.Timeline:			return "Timelines";
				case MAPPING_CATEGORY.Event:			return "Events";
			}
			return "";
		}
	}
}