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
using System.Text;
using System.Collections.Generic;
using AnyPortrait;

namespace AnyPortrait
{
	//ModifiedMesh의 Extra Option을 설정한다.
	public class apDialog_ExtraOption : EditorWindow
	{
		// Members
		//------------------------------------------------------------------
		//public delegate void FUNC_EXTRA_OPTION_CHANGED(object loadKey, 
		//	bool isAnimEdit,
		//	apMeshGroup meshGroup, 
		//	apModifierBase modifier, 
		//	apModifiedMesh modMesh,
		//	bool isExtraOptionEnabled,

		//	);


		public enum TAB
		{
			Depth,
			Image
		}


		private static apDialog_ExtraOption s_window = null;

		private apEditor _editor;
		private apPortrait _portrait;
		
		private apMeshGroup _meshGroup;
		private apModifierBase _modifier;		
		
		//21.9.29 변경 : 다중 편집이 가능하다
		// <모디파이어의 경우>
		// 단순 다중 편집 (Main, All 구분 가능)
		// <애니메이션의 경우>
		// 단일 키프레임 선택시 : 단일 편집 + 프레임 이동 가능
		// 다중 키프레임 선택시 : 다중 편집 + (프레임 이동 불가)
		
		//다중 편집의 경우, 리스트는 각각 (Depth, Image)로 나누어서 저장한다.
		//대상이 다르기 때문
		//- Depth : Main과 Parent가 같아야한다. 클리핑된 메시는 불가
		//- Image : Mesh만 가능

		//이전 (단일 대상만 가능)
		//private apModifiedMesh _modMesh;
		//private apRenderUnit _renderUnit;

		//변경 21.10.2
		//선택된 객체들을 유닛으로 저장하자 (21.10.1)
		public class ModSet
		{	
			public apRenderUnit _renderUnit = null;

			public bool _isEditable_Depth = false;
			public bool _isEditable_Image = false;

			public apModifiedMesh _modMesh = null;//이게 Null일 수 있다. (특히, 키프레임 이동시)
			public apAnimKeyframe _keyframe = null;//이게 Null일 수 있다. (애니메이션 편집 또는 키프레임 이동시)
			public apAnimTimelineLayer _timelineLayer = null;

			//메인은 각각 다르다
			public bool _isMain_Depth = false;
			public bool _isMain_Image = false;

			public ModSet(apRenderUnit renderUnit)
			{
				_renderUnit = renderUnit;

				_isEditable_Depth = false;
				_isEditable_Image = false;

				_modMesh = null;
				_keyframe = null;

				_isMain_Depth = false;
				_isMain_Image = false;
			}

			public void SetData(apModifiedMesh modMesh, apAnimKeyframe keyframe)
			{
				_modMesh = modMesh;
				_keyframe = keyframe;
				if(_keyframe != null)
				{
					_timelineLayer = _keyframe._parentTimelineLayer;
				}
				else
				{
					_timelineLayer = null;
				}
			}

			public void SetEditable(TAB tab, bool isEditable)
			{
				if(tab == TAB.Depth)
				{
					_isEditable_Depth = isEditable;
				}
				else
				{
					_isEditable_Image = isEditable;
				}
			}
			

			public void SetMain(TAB tab)
			{
				if(tab == TAB.Depth)
				{
					_isMain_Depth = true;
				}
				else
				{
					_isMain_Image = true;
				}
			}
		}

		private ModSet _modSet_MainIgnoreCond = null;//탭과 무관한 메인
		private Dictionary<TAB, ModSet> _modSet_Main = null;//메인은 각각 취급
		private List<ModSet> _modSets = null;//리스트는 공유한다. (여기에 메인도 포함. ModMesh가 없거나 해당 안되는 경우도 그냥 다 포함한다.)
		private int _nModSets = 0;
		private Dictionary<apRenderUnit, ModSet> _renderUnit2ModSet = null;


		private bool _isAnimEdit;
		private apAnimClip _animClip = null;

		//private apAnimKeyframe _keyframe = null;//이전

		//변경 21.9.29
		//키프레임은 리스트, 메인 또는 유효한 키프레임들 중 하나, 현재 값의 동기화 여부를 저장한다.
		//기준 키프레임이 메인 객체를 의미하는 것이 아닐 수 있다.
		private apAnimKeyframe _keyframe_Base = null;//기준 키프레임 (메인이 아니다. 리스트 중 하나일 뿐). 없을 수도 있다.
		private List<apAnimKeyframe> _keyframes = null;

		private Vector2 _scrollList = new Vector2();//<<Depth 바꿀때 쓰는 리스트

		private apSelection.SELECTION_TYPE _selectionType = apSelection.SELECTION_TYPE.None;

		private bool _isDepthEditable = false;
		private int _targetDepth = 0;

		private Texture2D _img_DepthCursor = null;
		private Texture2D _img_DepthMidCursor = null;
		private Texture2D _img_MeshTF = null;
		private Texture2D _img_MeshGroupTF = null;
		private Texture2D _img_MeshTF_Moved = null;
		private Texture2D _img_MeshGroupTF_Moved = null;

		private bool _isImageEditable = false;
		//변경 : 텍스쳐도 동기화를 해야한다.
		
		//Src는 서로 달라도 된다. 동기화 필요 없음
		//Dst
		//- 동기화 안됨
		//- 동기화 됨 : Null / 값이 있다.

		private bool _isSrcTextureSync = false;
		private apTextureData _srcTexureData = null;

		private bool _isDstTextureSync = false;
		private apTextureData _dstTexureData = null;
		//private bool _isImageChangable = false;//이건 빼도 된다. 매번 동기화 체크를 할거임

		
		
		private TAB _tab = TAB.Depth;

		//추가 : 아무것도 없는 유닛이 맨위, 맨아래에 붙어야 한다.
		public enum SUBUNIT_TYPE
		{
			MeshTransform,
			MeshGroupTransform,
			//MeshTransform_Moved,
			//MeshGroupTransform_Moved,
			Empty
		}
		public enum UNIT_SELECTED
		{
			None,
			Main,
			Sub
		}
		public class SubUnit
		{
			//public bool _isRoot = false;
			//public int _level = 0;
			
			//public bool _isTarget = false;
			public UNIT_SELECTED _selectedType = UNIT_SELECTED.None;
			public bool _isTarget_Sub = false;
			
			public string _name = null;
			public int _depth_Org = 0;
			public int _depth_Delta = 0;
			public int _depth_Result = 0;
			public int _depth_UI = 0;
			//public bool _isMeshTransform = false;
			public SUBUNIT_TYPE _subUnitType = SUBUNIT_TYPE.Empty;
			//public SubUnit _orgSubUnit = null;//이동 타입이 아닌 경우의 서브 유닛

			public ModSet _linkedModSet = null;

			
			public SubUnit(apRenderUnit renderUnit, 
				//int level, bool isRoot
				UNIT_SELECTED selectedType,
				ModSet linkedModSet

				)
			{
				//_isRoot = isRoot;
				//_level = level;
				//_isTarget = isTarget;
				_name = renderUnit.Name;
				
				_depth_Org = renderUnit.GetDepth();
				_depth_Delta = 0;
				_depth_Result = _depth_Org;
				_depth_UI = _depth_Result;

				//이전
				//_isMeshTransform = (renderUnit._meshTransform != null);

				//변경 21.9.27
				if((renderUnit._meshTransform != null))
				{
					_subUnitType = SUBUNIT_TYPE.MeshTransform;
				}
				else
				{
					_subUnitType = SUBUNIT_TYPE.MeshGroupTransform;
				}
				
				_selectedType = selectedType;
				//_orgSubUnit = null;
				_linkedModSet = linkedModSet;
			}

			#region [미사용 코드]
			//public SubUnit(ModSet modSet, SubUnit orgSubUnit)
			//{
			//	//이동된 Depth를 이름에 바로 반영하자
			//	if(modSet._modMesh._isExtraValueEnabled && modSet._modMesh._extraValue._isDepthChanged)
			//	{
			//		if(modSet._modMesh._extraValue._deltaDepth >= 0)
			//		{
			//			//+x
			//			_name = "(+" + modSet._modMesh._extraValue._deltaDepth + ") " + modSet._renderUnit.Name;
			//		}
			//		else
			//		{
			//			//-x
			//			_name = "(" + modSet._modMesh._extraValue._deltaDepth + ") " + modSet._renderUnit.Name;
			//		}
			//	}
			//	else
			//	{
			//		//사실 이 경우엔 리스트에 나오면 안된다.
			//		_name = "(..) " + modSet._renderUnit.Name;
			//	}

			//	_depth_Org = modSet._renderUnit.GetDepth();
			//	_depth_Delta = modSet._renderUnit._extraDeltaDepth;
			//	_depth_Result = _depth_Org + _depth_Delta;
			//	_depth_UI = _depth_Result;

			//	//이전
			//	//_isMeshTransform = (renderUnit._meshTransform != null);

			//	//변경 21.9.27
			//	if(modSet._renderUnit._meshTransform != null)
			//	{
			//		_subUnitType = SUBUNIT_TYPE.MeshTransform_Moved;
			//	}
			//	else
			//	{
			//		_subUnitType = SUBUNIT_TYPE.MeshGroupTransform_Moved;
			//	}

			//	_selectedType = UNIT_SELECTED.None;
			//	_orgSubUnit = null;
			//} 
			#endregion

			//추가 21.9.27 : Empty
			public SubUnit(string strName)
			{
				//Empty
				//_isRoot = false;
				//_level = 0;
				//_isTarget = false;
				_name = strName;
				
				_depth_Org = 0;
				_depth_Delta = 0;
				_depth_Result = 0;
				_depth_UI = _depth_Result;

				_selectedType = UNIT_SELECTED.None;

				_subUnitType = SUBUNIT_TYPE.Empty;
				//_orgSubUnit = null;
			}
		}

		
		private List<SubUnit> _subUnits_All = new List<SubUnit>();
		private int _subUnitDepth_Max = 0;
		private int _subUnitDepth_Min = 0;

		private enum DEPTH_CURSOR_TYPE
		{
			None,
			Mid,
			Target
		}


		// GUIContents 들
		apGUIContentWrapper _guiContent_DepthMidCursor = null;
		apGUIContentWrapper _guiContent_DepthCursor = null;
		apGUIContentWrapper _guiContent_MeshIcon = null;
		apGUIContentWrapper _guiContent_MeshGroupIcon = null;
		apGUIContentWrapper _guiContent_MeshIcon_Moved = null;
		apGUIContentWrapper _guiContent_MeshGroupIcon_Moved = null;

		GUIStyle _guiStyle_Button_TextBoxMargin = null;
		GUIStyle _guiStyle_Button_LikeLabel = null;

		// Show Window / Close Dialog
		//------------------------------------------------------------------------
		public static object ShowDialog_Modifier(	apEditor editor, 
													apPortrait portrait,
													apMeshGroup meshGroup, 
													apModifierBase modifier, 
													apRenderUnit renderUnit_Main,
													List<apRenderUnit> renderUnits_All,
													List<apModifiedMesh> modMeshes_All
													)
		{
			return ShowDialog(	editor, portrait, meshGroup, modifier, 
								renderUnit_Main, renderUnits_All,
								modMeshes_All, 								
								false, null);
		}
		
		public static object ShowDialog_Keyframe(	apEditor editor, 
													apPortrait portrait,
													apMeshGroup meshGroup, 
													apModifierBase modifier, 
													apRenderUnit renderUnit_Main,
													List<apRenderUnit> renderUnits_All,
													List<apModifiedMesh> modMeshes_All, 
													apAnimClip animClip)
		{
			return ShowDialog(	editor, portrait, meshGroup, modifier, 
								renderUnit_Main, renderUnits_All,
								modMeshes_All, 
								true, animClip);
		}



		private static object ShowDialog(	apEditor editor, 
											apPortrait portrait,
											apMeshGroup meshGroup, 
											apModifierBase modifier, 
											apRenderUnit renderUnit_Main,
											List<apRenderUnit> renderUnits_All,
											List<apModifiedMesh> modMeshes_All, 
											bool isAnimEdit,
											apAnimClip animClip
			)
		{
			CloseDialog();

			if (editor == null 
				|| editor._portrait == null 
				|| meshGroup == null 
				|| modifier == null
				)
			{
				return null;
			}

			if(modMeshes_All == null || modMeshes_All.Count == 0)
			{
				return null;
			}


			if(renderUnit_Main == null
				|| renderUnits_All == null || renderUnits_All.Count == 0)
			{
				return null;
			}

			//if(isAnimEdit && (keyframes_All == null || keyframes_All.Count == 0))
			//{
			//	return null;
			//}



			EditorWindow curWindow = EditorWindow.GetWindow(typeof(apDialog_ExtraOption), true, "Extra Properties", true);
			apDialog_ExtraOption curTool = curWindow as apDialog_ExtraOption;

			object loadKey = new object();
			if (curTool != null && curTool != s_window)
			{
				int width = 400;
				
				int height = 620;
				if(isAnimEdit)
				{
					height = 715;
				}
				s_window = curTool;
				s_window.position = new Rect((editor.position.xMin + editor.position.xMax) / 2 - (width / 2),
												(editor.position.yMin + editor.position.yMax) / 2 - (height / 2),
												width, height);

				//s_window.Init(editor, portrait, meshGroup, modifier, modMesh, renderUnit, isAnimEdit, animClip, keyframe);
				bool initResult = s_window.Init(	editor, portrait, meshGroup, modifier,
												renderUnit_Main,renderUnits_All,
												modMeshes_All, 
												isAnimEdit, animClip);

				if(!initResult)
				{
					//초기화 실패시 다이얼로그를 닫아야 한다.
					CloseDialog();

					Debug.LogError("AnyPortrait : There is no object to which Extra Option can be applied.");

					return null;
				}

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
		//------------------------------------------------------------------------------------------------
		private bool Init(apEditor editor,
							apPortrait portrait,
							apMeshGroup meshGroup,
							apModifierBase modifier,
							apRenderUnit renderUnit_Main,
							List<apRenderUnit> renderUnits_All,
							List<apModifiedMesh> modMeshes_All, 
							bool isAnimEdit,
							apAnimClip animClip)
		{
			_editor = editor;
			_portrait = portrait;
			_meshGroup = meshGroup;
			_modifier = modifier;
			
			_isAnimEdit = isAnimEdit;
			_animClip = animClip;

			//이전
			//_modMesh = modMesh;
			//_renderUnit = modMesh._renderUnit;
			//_keyframe = keyframe;
			
			//다중 선택인 경우
			//- 먼저 대상 RenderUnit들을 찾는다.
			//- Main과 동일한 Parent를 가져야 한다. (다른 Parent인 경우에는 무효)
			//- 선택된 RenderUnit을 대상으로 하는 ModMesh만 취합한다.
			//- 선택된 ModMesh들만 대상으로 하는 Keyframe만 취합한다.
			apRenderUnit curRenderUnit = null;


			_modSet_Main = new Dictionary<TAB, ModSet>();
			_modSet_Main.Add(TAB.Depth, null);
			_modSet_Main.Add(TAB.Image, null);

			_modSets = new List<ModSet>();
			_renderUnit2ModSet = new Dictionary<apRenderUnit, ModSet>();
			
			_modSet_MainIgnoreCond = null;

			//이 모디파이어가 다루는 타임라인을 찾자
			apAnimTimeline targetAnimTimeline = null;

			if (isAnimEdit)
			{
				_keyframe_Base = null;
				_keyframes = new List<apAnimKeyframe>();

				targetAnimTimeline = animClip._timelines.Find(delegate(apAnimTimeline a)
				{
					return a._linkedModifier = modifier;
				});
			}
			else
			{
				_keyframe_Base = null;
				_keyframes = null;
			}

			//Depth용 렌더 유닛 찾기
			//없으면 대체제를 이용한다.
			//1. 클리핑 Child면 안된다. > Main 대체제를 찾는다.
			//2. Main이 있다는 조건하에 Main과 Parent가 같아야 한다. 

			//Image용 렌더 유닛 찾기
			//- 메시만 가능

			//이 메인 렌더 유닛들을 바탕으로 ModSet 리스트를 정하자
			apRenderUnit mainRenderUnit_Depth = null;
			apRenderUnit mainRenderUnit_Image = null;

			
			//Depth 메인 찾기
			if(!renderUnit_Main.IsClippedChild)
			{
				//오케이
				mainRenderUnit_Depth = renderUnit_Main;
			}
			else
			{
				//Main이 적절하지 않다. 찾아야한다.
				if (renderUnits_All != null)
				{
					mainRenderUnit_Depth = renderUnits_All.Find(delegate (apRenderUnit a)
					{
						return !a.IsClippedChild;
					});
				}
			}

			//Image 메인 찾기
			if(renderUnit_Main._meshTransform != null)
			{
				//메인으로 적당하다.
				mainRenderUnit_Image = renderUnit_Main;
			}
			else
			{
				//Main이 적절하지 않다. Mesh인 메인을 찾자
				if (renderUnits_All != null)
				{
					mainRenderUnit_Image = renderUnits_All.Find(delegate (apRenderUnit a)
					{
						return a._meshTransform != null;
					});
				}
			}

			//이제 전체 리스트를 돌면서 체크하자
			int nSrcRenderUnits = renderUnits_All != null ? renderUnits_All.Count : 0;
			for (int iRenderUnit = 0; iRenderUnit < nSrcRenderUnits; iRenderUnit++)
			{
				curRenderUnit = renderUnits_All[iRenderUnit];
				if(_renderUnit2ModSet.ContainsKey(curRenderUnit))
				{
					continue;
				}

				ModSet newModSet = new ModSet(curRenderUnit);
				_modSets.Add(newModSet);
				_renderUnit2ModSet.Add(curRenderUnit, newModSet);
			}

			//Main이 안들어갔다면 추가
			if(renderUnit_Main != null && !_renderUnit2ModSet.ContainsKey(renderUnit_Main))
			{
				ModSet newModSet = new ModSet(renderUnit_Main);
				_modSets.Add(newModSet);
				_renderUnit2ModSet.Add(renderUnit_Main, newModSet);
			}

			_nModSets = _modSets.Count;

			if(_nModSets > 0)
			{
				_modSet_MainIgnoreCond = _modSets[0];
			}

			//이제 ModSet 기준으로 ModMesh와 Keyframe을 연결한다.
			//Editable도 체크해야한다.
			//Main도 연결
			ModSet curModSet = null;
			apModifiedMesh linkedModMesh = null;
			apAnimKeyframe linkedKeyframe = null;
			for (int iModSet = 0; iModSet < _nModSets; iModSet++)
			{
				curModSet = _modSets[iModSet];
				curRenderUnit = curModSet._renderUnit;

				//1. Main 연결
				if(mainRenderUnit_Depth == curRenderUnit)
				{
					curModSet.SetMain(TAB.Depth);
					_modSet_Main[TAB.Depth] = curModSet;
				}

				if(mainRenderUnit_Image == curRenderUnit)
				{
					curModSet.SetMain(TAB.Image);
					_modSet_Main[TAB.Image] = curModSet;
				}

				//2. Editable 체크
				bool isEditable_Depth = false;
				bool isEditable_Image = false;

				//Depth 체크 : Main이 있을때만 가능
				if (mainRenderUnit_Depth != null)
				{
					if (!curRenderUnit.IsClippedChild
					&& curRenderUnit._parentRenderUnit == mainRenderUnit_Depth._parentRenderUnit)
					{
						//클리핑이 아니어야 하고, 메인과 같은 레벨이어야 한다.
						isEditable_Depth = true;
					}
				}

				//Image 체크 : Mesh면 오케이
				if(curRenderUnit._meshTransform != null)
				{
					isEditable_Image = true;
				}
				
				curModSet.SetEditable(TAB.Depth, isEditable_Depth);
				curModSet.SetEditable(TAB.Image, isEditable_Image);


				//3. 데이터 연결
				linkedModMesh = null;
				linkedKeyframe = null;

				linkedModMesh = modMeshes_All.Find(delegate (apModifiedMesh a)
				{
					return a._renderUnit == curRenderUnit;
				});

				if(isAnimEdit && linkedModMesh != null && targetAnimTimeline != null)
				{	
					//키프레임을 찾자
					//먼저 타임라인 레이어를 찾자
					apAnimTimelineLayer targetTimelineLayer = targetAnimTimeline._layers.Find(delegate(apAnimTimelineLayer a)
					{
						if(curRenderUnit._meshTransform != null)
						{
							return a._linkedMeshTransform == curRenderUnit._meshTransform;
						}
						else if(curRenderUnit._meshGroupTransform != null)
						{
							return a._linkedMeshGroupTransform == curRenderUnit._meshGroupTransform;
						}
						return false;
					});

					if(targetTimelineLayer != null)
					{
						linkedKeyframe = targetTimelineLayer._keyframes.Find(delegate(apAnimKeyframe a)
						{
							return a._linkedModMesh_Editor == linkedModMesh;
						});
					}
					//linkedKeyframe = keyframes_All.Find(delegate(apAnimKeyframe a)
					//{
					//	return a._linkedModMesh_Editor == linkedModMesh;
					//});

					//키프레임 리스트에도 등록
					if(linkedKeyframe != null)
					{
						if(_keyframe_Base == null)
						{
							_keyframe_Base = linkedKeyframe;
						}
						_keyframes.Add(linkedKeyframe);
					}
				}

				curModSet.SetData(linkedModMesh, linkedKeyframe);

				
			}


			_selectionType = _editor.Select.SelectionType;

			_isDepthEditable = _modSet_Main[TAB.Depth] != null;
			_isImageEditable = _modSet_Main[TAB.Image] != null;

			//메인이 둘다 없다.
			if(!_isDepthEditable && !_isImageEditable)
			{
				return false;
			}

			//Depth 탭 내용 초기화
			if(_isDepthEditable)
			{
				_targetDepth = _modSet_Main[TAB.Depth]._renderUnit.GetDepth();
			}
			else
			{
				_targetDepth = 0;
			}
			


			_img_DepthCursor = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_DepthCursor);
			_img_DepthMidCursor = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_DepthMidCursor);
			_img_MeshTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			_img_MeshGroupTF = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			_img_MeshTF_Moved = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_MovedMesh);
			_img_MeshGroupTF_Moved = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_MovedMeshGroup);

			#region [미사용 코드] RefreshSubUnits 함수로 변경
			//if(_subUnits_All == null)
			//{
			//	_subUnits_All = new List<SubUnit>();
			//}
			//_subUnits_All.Clear();


			//apRenderUnit parentUnit = _isDepthEditable ? _modSet_Main[TAB.Depth]._renderUnit._parentRenderUnit : null;

			//for (int i = 0; i < _meshGroup._renderUnits_All.Count; i++)
			//{
			//	curRenderUnit = _meshGroup._renderUnits_All[i];

			//	//Parent가 같은 형제 렌더 유닛에 대해서만 처리한다.
			//	//단, MeshTransform일 때, Clipping Child는 생략한다.
			//	if(curRenderUnit._meshTransform != null && curRenderUnit._meshTransform._isClipping_Child)
			//	{
			//		continue;
			//	}

			//	if (curRenderUnit._parentRenderUnit != parentUnit)
			//	{
			//		continue;
			//	}

			//	if(_isDepthEditable)
			//	{
			//		//SubUnit subUnit = new SubUnit(curRenderUnit, curRenderUnit._level, (curRenderUnit == _modSet_Main[TAB.Depth]._renderUnit), (curRenderUnit == _meshGroup._rootRenderUnit));

			//		UNIT_SELECTED selectedType = UNIT_SELECTED.None;
			//		if (curRenderUnit == _modSet_Main[TAB.Depth]._renderUnit)
			//		{
			//			selectedType = UNIT_SELECTED.Main;
			//		}
			//		else
			//		{
			//			bool isSubSelectedUnit = _modSets.Exists(delegate (ModSet a)
			//			{
			//				return a._renderUnit == curRenderUnit && a._isEditable_Depth && a._modMesh != null;
			//			});

			//			if (isSubSelectedUnit)
			//			{
			//				selectedType = UNIT_SELECTED.Sub;
			//			}

			//		}

			//		SubUnit subUnit = new SubUnit(curRenderUnit, selectedType);
			//		_subUnits_All.Add(subUnit);
			//	}
			//	else
			//	{
			//		//SubUnit subUnit = new SubUnit(curRenderUnit, curRenderUnit._level, false, (curRenderUnit == _meshGroup._rootRenderUnit));
			//		SubUnit subUnit = new SubUnit(curRenderUnit, UNIT_SELECTED.None);
			//		_subUnits_All.Add(subUnit);
			//	}

			//}

			//_subUnits_All.Sort(delegate(SubUnit a, SubUnit b)
			//{
			//	return b._depth_Result - a._depth_Result;
			//});




			////추가 21.9.27
			////맨 위와 맨 아래에 하나 더 붙인다.
			//List<SubUnit> emptyAddedList = new List<SubUnit>();
			//emptyAddedList.Add(new SubUnit("(Top)"));//맨 앞에 하나 넣고
			////리스트 복사하고
			//for (int i = 0; i < _subUnits_All.Count; i++)
			//{
			//	emptyAddedList.Add(_subUnits_All[i]);
			//}
			//emptyAddedList.Add(new SubUnit("(Bottom)"));//맨 뒤에 하나더 넣고

			////리스트 교체
			//_subUnits_All = emptyAddedList;

			////여기서는 실제 Depth보다 상대적 Depth만 고려한다.
			//int curDepth = 0;

			//_subUnitDepth_Max = 0;
			//_subUnitDepth_Min = 0;
			//for (int i = _subUnits_All.Count - 1; i >= 0; i--)
			//{
			//	_subUnits_All[i]._depth_UI = curDepth;
			//	if(_isDepthEditable && _subUnits_All[i]._selectedType == UNIT_SELECTED.Main)
			//	{
			//		_targetDepth = curDepth;
			//	}
			//	_subUnitDepth_Max = Mathf.Max(curDepth, _subUnitDepth_Max);
			//	curDepth++;
			//} 
			#endregion

			RefreshSubUnits();

			//이미지를 바꿀 수 있는가
			RefreshImagePreview();
			
			return true;
		}
		
		private void RefreshImagePreview()
		{
			_srcTexureData = null;
			_dstTexureData = null;
			_isSrcTextureSync = false;
			_isDstTextureSync = false;
			
			//_isImageChangable = false;

			//동기화를 하자

			//이전
			//if(!_isImageEditable 
			//	|| _modSet_Main[TAB.Image] == null
			//	|| _modSet_Main[TAB.Image]._modMesh == null)
			//{
			//	return;
			//}

			//apModifiedMesh mainModMesh = _modSet_Main[TAB.Image]._modMesh;

			//if(mainModMesh._transform_Mesh == null
			//	|| mainModMesh._transform_Mesh._mesh == null
			//	|| mainModMesh._transform_Mesh._mesh._textureData_Linked == null)
			//{
			//	return;
			//}

			//apTextureData linkedTextureData = mainModMesh._transform_Mesh._mesh._textureData_Linked;
					
			////_isImageChangable = true;

			//_srcTexureData = linkedTextureData;

			//if(mainModMesh._extraValue._textureDataID >= 0)
			//{
			//	_dstTexureData = _portrait.GetTexture(mainModMesh._extraValue._textureDataID);
			//	if(_dstTexureData == null)
			//	{
			//		mainModMesh._extraValue._textureDataID = -1;
			//	}
			//}

			//변경
			_srcTexureData = CheckSync_Texture(SYNC_VAR_TYPE_TEXTURE.SrcTexture, ref _isSrcTextureSync);
			_dstTexureData = CheckSync_Texture(SYNC_VAR_TYPE_TEXTURE.DstTexture, ref _isDstTextureSync);
		}


		// GUI
		//------------------------------------------------------------------------------------------------
		private void OnGUI()
		{
			int width = (int)position.width;
			int height = (int)position.height;

			width -= 10;

			//만약 이 다이얼로그가 켜진 상태로 뭔가 바뀌었다면 종료
			bool isClose = false;
			bool isMoveAnimKeyframe = false;
			bool isMoveAnimKeyframeToNext = false;
			if (_editor == null || _meshGroup == null || _modifier == null
				//|| _modMesh == null || _renderUnit == null//이전
				|| (!_isDepthEditable && !_isImageEditable)
				)
			{
				//데이터가 없다.
				isClose = true;
			}
			else if (_editor.Select.SelectionType != _selectionType)
			{
				//선택 타입이 바뀌었다.
				isClose = true;
			}
			else
			{

				if (!_isAnimEdit)
				{
					//1. 일반 모디파이어 편집시
					//- 현재 선택된 MeshGroup이 바뀌었다면
					//- 현재 선택된 Modifier가 바뀌었다면
					//- Modifier 메뉴가 아니라면
					//- ExEditingMode가 꺼졌다면
					//> 해제
					if (_editor.Select.ExEditingMode == apSelection.EX_EDIT.None
						|| _editor.Select.MeshGroup != _meshGroup
						|| _editor.Select.Modifier != _modifier
						|| _editor._meshGroupEditMode != apEditor.MESHGROUP_EDIT_MODE.Modifier)
					{
						isClose = true;
					}
				}
				else
				{
					//2. 애니메이션 편집 시
					//- 현재 선택된 AnimationClip이 바뀌었다면
					//- 현재 선택된 MeshGroup이 바뀌었다면 (AnimClip이 있을 때)
					//- AnimExEditingMode가 꺼졌다면
					//- 재생 중이라면
					//> 해제
					if (_editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None
						|| _editor.Select.AnimClip != _animClip
						|| _animClip == null
						//|| _keyframe == null						
						|| _editor.Select.AnimClip._targetMeshGroup != _meshGroup
						|| _editor.Select.IsAnimPlaying)
					{
						isClose = true;
					}
				}
			}

			if (isClose)
			{
				CloseDialog();
				return;
			}


			if(_guiStyle_Button_TextBoxMargin == null)
			{
				_guiStyle_Button_TextBoxMargin = new GUIStyle(GUI.skin.button);
				_guiStyle_Button_TextBoxMargin.margin = GUI.skin.textField.margin;
			}

			if(_guiStyle_Button_LikeLabel == null)
			{
				_guiStyle_Button_LikeLabel = new GUIStyle(GUI.skin.label);
			}


			//------------------------------------------------------------

			//1. 선택된 객체 정보
			//- RenderUnit 아이콘과 이름

			//2. <애니메이션인 경우>
			//- 현재 키프레임과 키프레임 이동하기

			//3. 가중치
			//- [일반] Weight CutOut
			//- [Anim] Prev / Next CutOut

			// (구분선)

			//4. Depth
			//- 아이콘과 Chainging Depth 레이블
			//- On / Off 버튼
			//- Depth 증감과 리스트 (좌우에 배치)

			//5. Texture (RenderUnit이 MeshTransform인 경우)
			//- 현재 텍스쳐
			//- 바뀔 텍스쳐
			//- 텍스쳐 선택하기 버튼

			//1. 선택된 객체 정보
			//- RenderUnit 아이콘과 이름
			//int iconSize = 25;
			GUIStyle guiStyle_TargetBox = new GUIStyle(GUI.skin.box);
			guiStyle_TargetBox.alignment = TextAnchor.MiddleCenter;
			Color prevColor = GUI.backgroundColor;

			// RenderUnit 이름
			//기존 : 단일 객체
			//변경 : 경우의 수가 많다.
			//- 현재의 탭에 따라서 메인이 다르다.
			//- 메인이 없을 수도 있다. (편집 불가)
			//- 여러개일 수도 있다.
			Texture2D iconRenderUnit = null;
			//이전
			//if(_renderUnit._meshTransform != null)
			//{
			//	iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
			//}
			//else
			//{
			//	iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
			//}

			//변경
			Color titleBoxColor = Color.black;
			string strName = null;
			if(_nModSets > 0 && _modSet_MainIgnoreCond != null)
			{
				//선택된게 있다. 여러개 편집 가능한가.
				if (_nModSets > 1)
				{
					iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MultiSelected);
					strName = "  " + _editor.GetUIWord(UIWORD.MultipleSelected);
					titleBoxColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
				}
				else
				{
					//하나만 편집 가능하다.
					if (_modSet_MainIgnoreCond._renderUnit._meshTransform != null)
					{
						iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_Mesh);
					}
					else
					{
						iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_MeshGroup);
					}

					strName = "  " + _modSet_MainIgnoreCond._renderUnit.Name;
					titleBoxColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
				}
			}
			else
			{
				//선택된게 없다.
				strName = "  " + _editor.GetUIWord(UIWORD.NotSelected);
				iconRenderUnit = _editor.ImageSet.Get(apImageSet.PRESET.Hierarchy_None);
				titleBoxColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
			}




			//이전
			//GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);
			//GUILayout.Box(new GUIContent("  " + _renderUnit.Name, iconRenderUnit), guiStyle_TargetBox, GUILayout.Width(width), GUILayout.Height(30));

			//변경
			GUI.backgroundColor = titleBoxColor;
			GUILayout.Box(new GUIContent(strName, iconRenderUnit), guiStyle_TargetBox, GUILayout.Width(width), GUILayout.Height(30));


			GUI.backgroundColor = prevColor;

			GUILayout.Space(5);


			//변경 21.10.1 : 값들을 동기화해야한다.
			//"Extra Property ON", "Extra Property OFF"
			//이전
			//if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.ExtraOpt_ExtraPropertyOn), _editor.GetText(TEXT.ExtraOpt_ExtraPropertyOff), _modMesh._isExtraValueEnabled, true, width, 25))


			//변경 21.10.1 : 동기화를 하여 체크하자
			
			//Extra Property에 대한 동기화 체크
			bool isExtraProp_Sync = true;
			bool isExtraProp_Enabled = true;
			bool isExtraProp_Available = true;			
			CheckSync_Bool(SYNC_VAR_TYPE_BOOL.ExtraOption, ref isExtraProp_Sync, ref isExtraProp_Enabled, ref isExtraProp_Available);
			
			if (apEditorUtil.ToggledButton_2Side_Sync(	_editor.GetText(TEXT.ExtraOpt_ExtraPropertyOn), 
														_editor.GetText(TEXT.ExtraOpt_ExtraPropertyOff),
														isExtraProp_Enabled,
														isExtraProp_Available,
														isExtraProp_Sync, width, 25))
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

				//이전
				//_modMesh._isExtraValueEnabled = !_modMesh._isExtraValueEnabled;

				//변경 21.10.1
				//다음값 = (동기화 되었다면 동기화된 값의 반대) / (동기화 안되었다면 true)
				if (isExtraProp_Available)
				{
					bool nextExtraValue = isExtraProp_Sync ? !isExtraProp_Enabled : true;
					SyncValue_Bool(SYNC_VAR_TYPE_BOOL.ExtraOption, nextExtraValue);//값을 동기화한다.
				}

				_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<옵션의 형태가 바뀌면 Modifier의 Link를 다시 해야한다.
				apEditorUtil.ReleaseGUIFocus();
			}


			GUILayout.Space(5);

			if(_isAnimEdit)
			{
				GUILayout.Space(10);

				//2. <애니메이션인 경우>
				//- 현재 키프레임과 키프레임 이동하기

				//변경 21.10.1
				//

				//"Target Frame"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_TargetFrame));
				int frameBtnSize = 20;
				int frameCurBtnWidth = 100;
				int frameMoveBtnWidth = (width - (10 + frameCurBtnWidth)) / 2;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(frameBtnSize));
				GUILayout.Space(5);
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToPrevFrame), GUILayout.Width(frameMoveBtnWidth), GUILayout.Height(frameBtnSize)))
				{
					//이전 프레임으로 이동하기
					isMoveAnimKeyframe = true;
					isMoveAnimKeyframeToNext = false;
				}
				if(GUILayout.Button(_keyframe_Base != null ? _keyframe_Base._frameIndex.ToString() : apStringFactory.I.QuestionMark, GUILayout.Width(frameCurBtnWidth), GUILayout.Height(frameBtnSize)))
				{
					if(_keyframe_Base != null)
					{
						_animClip.SetFrame_Editor(_keyframe_Base._frameIndex);
					}
					_editor.SetRepaint();
				}
				if(GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.Anim_MoveToNextFrame), GUILayout.Width(frameMoveBtnWidth), GUILayout.Height(frameBtnSize)))
				{
					//다음 프레임으로 이동하기
					isMoveAnimKeyframe = true;
					isMoveAnimKeyframeToNext = true;
				}

				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(10);
			
			//3. 가중치
			//- [일반] Weight CutOut
			//- [Anim] Prev / Next CutOut
			
			EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_WeightSettings));//"Weight Settings"
			GUILayout.Space(5);

			int width_CutoutLabel = (int)(width * 0.5f);
			int width_CutoutValue = width - (14 + width_CutoutLabel);
			int height_Cutout = 20;

			if(!_isAnimEdit)
			{
				bool isWeightCutout_Sync = true;
				float weightCutout_Value = 0.0f;

				CheckSync_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_Normal, ref isWeightCutout_Sync, ref weightCutout_Value, 0.5f);//기본값은 0.5

				//일반이면 CutOut이 1개
				//apModifiedMesh _modMesh;
				//이전
				//float cutOut = EditorGUILayout.DelayedFloatField(_editor.GetText(TEXT.ExtraOpt_Offset), _modMesh._extraValue._weightCutout);//"Offset (0~1)"

				EditorGUILayout.BeginHorizontal(GUILayout.Height(height_Cutout));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_Offset), GUILayout.Width(width_CutoutLabel));

				//변경 [동기화]
				if(!isWeightCutout_Sync)
				{
					//동기화가 안되었다면 TODO : 언어
					if(GUILayout.Button(_editor.GetUIWord(UIWORD.ResetValue), _guiStyle_Button_TextBoxMargin, GUILayout.Width(width_CutoutValue)))
					{
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
														_editor, 
														_modifier, 
														//_modMesh._extraValue, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						SyncValue_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_Normal, 0.5f);//기본값으로 동기화

						apEditorUtil.ReleaseGUIFocus();
					}
				}
				else
				{
					//동기화가 되었다면
					float cutOut = EditorGUILayout.DelayedFloatField(weightCutout_Value, GUILayout.Width(width_CutoutValue));//"Offset (0~1)"

					//if(cutOut != _modMesh._extraValue._weightCutout)
					if(cutOut != weightCutout_Value)//변경
					{
						cutOut = Mathf.Clamp01(cutOut);
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
														_editor, 
														_modifier, 
														//_modMesh._extraValue, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						//_modMesh._extraValue._weightCutout = cutOut;
						SyncValue_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_Normal, cutOut);//변경

						apEditorUtil.ReleaseGUIFocus();
					}
				}

				EditorGUILayout.EndHorizontal();
			}
			else
			{
				//애니메이션이면 CutOut이 2개
				bool isWeightCutout_Sync_Prev = true;
				bool isWeightCutout_Sync_Next = true;
				float weightCutout_Value_Prev = 0.0f;
				float weightCutout_Value_Next = 0.0f;

				CheckSync_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimPrev, ref isWeightCutout_Sync_Prev, ref weightCutout_Value_Prev, 0.5f);//Prev의 기본값은 0.5
				CheckSync_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimNext, ref isWeightCutout_Sync_Next, ref weightCutout_Value_Next, 0.6f);//Next의 기본값은 0.6

				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_Offset));
				

				//Prev
				EditorGUILayout.BeginHorizontal(GUILayout.Height(height_Cutout));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_OffsetPrevKeyframe), GUILayout.Width(width_CutoutLabel));

				if(!isWeightCutout_Sync_Prev)
				{
					//동기화가 안되었다면 TODO : 언어
					if(GUILayout.Button(_editor.GetUIWord(UIWORD.ResetValue), _guiStyle_Button_TextBoxMargin, GUILayout.Width(width_CutoutValue)))
					{
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

						SyncValue_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimPrev, 0.5f);//기본값으로 동기화

						apEditorUtil.ReleaseGUIFocus();
					}
				}
				else
				{
					//Prev값 지정
					//이전
					//float animPrevCutOut = EditorGUILayout.DelayedFloatField(_editor.GetText(TEXT.ExtraOpt_OffsetPrevKeyframe), _modMesh._extraValue._weightCutout_AnimPrev);//"Prev Keyframe"

					//변경
					float animPrevCutOut = EditorGUILayout.DelayedFloatField(weightCutout_Value_Prev, GUILayout.Width(width_CutoutValue));//"Prev Keyframe"

					if(animPrevCutOut != weightCutout_Value_Prev)
					{
						animPrevCutOut = Mathf.Clamp01(animPrevCutOut);
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
														_editor, 
														_modifier, 
														//_modMesh._extraValue, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						SyncValue_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimPrev, weightCutout_Value_Prev);
						apEditorUtil.ReleaseGUIFocus();
					}
				}
				EditorGUILayout.EndHorizontal();




				//Next
				EditorGUILayout.BeginHorizontal(GUILayout.Height(height_Cutout));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_OffsetNextKeyframe), GUILayout.Width(width_CutoutLabel));

				if (!isWeightCutout_Sync_Next)
				{
					//동기화가 안되었다면 TODO : 언어
					if (GUILayout.Button(_editor.GetUIWord(UIWORD.ResetValue), _guiStyle_Button_TextBoxMargin, GUILayout.Width(width_CutoutValue)))
					{
						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_ExtraOptionChanged,
													_editor,
													_modifier,
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

						SyncValue_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimNext, 0.6f);//기본값으로 동기화 (0.6)

						apEditorUtil.ReleaseGUIFocus();
					}
				}
				else
				{
					//동기화가 되었다면

					//이전
					//float animNextCutOut = EditorGUILayout.DelayedFloatField(_editor.GetText(TEXT.ExtraOpt_OffsetNextKeyframe), _modMesh._extraValue._weightCutout_AnimNext);//"Next Keyframe"
					//변경
					float animNextCutOut = EditorGUILayout.DelayedFloatField(weightCutout_Value_Next, GUILayout.Width(width_CutoutValue));//"Next Keyframe"

					if (animNextCutOut != weightCutout_Value_Next)
					{
						animNextCutOut = Mathf.Clamp01(animNextCutOut);

						apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_ExtraOptionChanged,
														_editor,
														_modifier,
														//_modMesh._extraValue, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

						//이전
						//_modMesh._extraValue._weightCutout_AnimPrev = animPrevCutOut;
						//_modMesh._extraValue._weightCutout_AnimNext = animNextCutOut;

						//변경 (동기화)
						SyncValue_Float(SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimNext, weightCutout_Value_Next);

						apEditorUtil.ReleaseGUIFocus();
					}
				}

				EditorGUILayout.EndHorizontal();
			}
			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);

			int tabBtnWidth = ((width - 10) / 2);
			int tabBtnHeight = 25;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(tabBtnHeight));
			GUILayout.Space(5);
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.ExtraOpt_Tab_Depth), _tab == TAB.Depth, tabBtnWidth, tabBtnHeight))//"Depth"
			{
				_tab = TAB.Depth;
			}
			if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.ExtraOpt_Tab_Image), _tab == TAB.Image, tabBtnWidth, tabBtnHeight))//"Image"
			{
				_tab = TAB.Image;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);

			if (_tab == TAB.Depth)
			{

				//4. Depth
				//- 아이콘과 Chainging Depth 레이블
				//- On / Off 버튼
				//- Depth 증감과 리스트 (좌우에 배치)

				bool isDepth_Sync = true;
				bool isDepth_Enabled = true;
				bool isDepth_Available = true;

				//동기화
				CheckSync_Bool(SYNC_VAR_TYPE_BOOL.DepthEnabled, ref isDepth_Sync, ref isDepth_Enabled, ref isDepth_Available);

				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_ChangingDepth));//"Changing Depth"
				GUILayout.Space(5);

				//"Depth Option ON", "Depth Option OFF"
				//이전
				//if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.ExtraOpt_DepthOptOn), _editor.GetText(TEXT.ExtraOpt_DepthOptOff), _modMesh._extraValue._isDepthChanged, _modMesh._isExtraValueEnabled, width, 25))
				//변경 (동기화)
				if (apEditorUtil.ToggledButton_2Side_Sync(	_editor.GetText(TEXT.ExtraOpt_DepthOptOn), 
														_editor.GetText(TEXT.ExtraOpt_DepthOptOff),
														isDepth_Enabled,
														isDepth_Available && isExtraProp_Enabled && isExtraProp_Sync,
														isDepth_Sync,
														width, 25))
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					//이전
					//_modMesh._extraValue._isDepthChanged = !_modMesh._extraValue._isDepthChanged;

					//변경 : 동기화
					bool nextDepthEnabled = isDepth_Sync ? !isDepth_Enabled : true;
					SyncValue_Bool(SYNC_VAR_TYPE_BOOL.DepthEnabled, nextDepthEnabled);

					_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<Modifier Link를 다시 해야한다.
					_editor.SetRepaint();
				}
				GUILayout.Space(5);

				//이전
				//bool isDepthAvailable = _modMesh._extraValue._isDepthChanged && _modMesh._isExtraValueEnabled;
				//변경 (조건이 많다)
				bool isDepthSubOptionAvailable = isDepth_Sync 
												&& isDepth_Available 
												&& isDepth_Enabled 
												&& isExtraProp_Enabled 
												&& isExtraProp_Sync;

				int depthListWidth_Left = 80;
				int depthListWidth_Right = width - (10 + depthListWidth_Left);
				int depthListWidth_RightInner = depthListWidth_Right - 20;
				
				//이전 : 고정
				//int depthListHeight = 276;
				//변경 : Height에 상대적인 값

				int depthListHeight = height - 350;
				if(_isAnimEdit)
				{
					//Anim UI는 위에서 95를 더 사용한다.
					depthListHeight = height - (350 + 95);
				}


				//int depthListHeight_LeftBtn = (depthListHeight - 40) / 2;
				int depthListHeight_LeftBtn = 40;
				int depthListHeight_LeftSpace = (depthListHeight - (40 + depthListHeight_LeftBtn * 2)) / 2;
				int depthListHeight_RightList = 20;

				//리스트 배경
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if(!isDepthSubOptionAvailable)
				{
					GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f, 1.0f);
				}
				GUI.Box(new Rect(5 + depthListWidth_Left + 8, lastRect.y + 8, depthListWidth_Right, depthListHeight), "");
				if(!isDepthSubOptionAvailable)
				{
					GUI.backgroundColor = prevColor;
				}

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(depthListHeight));
				GUILayout.Space(5);
				EditorGUILayout.BeginVertical(GUILayout.Width(depthListWidth_Left), GUILayout.Height(depthListHeight));
				// Depth List의 왼쪽
				// Depth 증감 버튼과 값
				GUILayout.Space(depthListHeight_LeftSpace);

				Texture2D img_AddWeight = _editor.ImageSet.Get(apImageSet.PRESET.Rig_AddWeight);
				Texture2D img_SubtractWeight = _editor.ImageSet.Get(apImageSet.PRESET.Rig_SubtractWeight);
				

				//if (GUILayout.Button(, GUILayout.Width(depthListWidth_Left), GUILayout.Height(depthListHeight_LeftBtn)))
				if(apEditorUtil.ToggledButton(img_AddWeight, false, isDepthSubOptionAvailable, depthListWidth_Left, depthListHeight_LeftBtn))
				{
					//Depth 증가
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					//이전
					//_modMesh._extraValue._deltaDepth++;

					//변경 [동기화]
					SyncValue_Int_AddValue(SYNC_VAR_TYPE_INT.DeltaDepth, 1);

					_editor.SetRepaint();
					apEditorUtil.ReleaseGUIFocus();
				}

				//"Delta Depth"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_DeltaDepth), GUILayout.Width(depthListWidth_Left));


				//동기화
				bool isDeltaDepth_Sync = true;
				int deltaDepth_Sync = 0;

				//동기화를 체크한다. (기본값은 0)
				CheckSync_Int(SYNC_VAR_TYPE_INT.DeltaDepth, ref isDeltaDepth_Sync, ref deltaDepth_Sync, 0);

				if(!isDeltaDepth_Sync)
				{
					if(GUILayout.Button(_editor.GetText(TEXT.DLG_PSD_Reset), _guiStyle_Button_TextBoxMargin, GUILayout.Width(depthListWidth_Left)))
					{
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

						//값을 동기화한다.
						SyncValue_Int(SYNC_VAR_TYPE_INT.DeltaDepth, 0);

						apEditorUtil.ReleaseGUIFocus();
					}
				}
				else
				{
					//이전
					//int deltaDepth = EditorGUILayout.DelayedIntField(_modMesh._extraValue._deltaDepth, GUILayout.Width(depthListWidth_Left));

					//변경 : 동기화된 값
					int deltaDepth = EditorGUILayout.DelayedIntField(deltaDepth_Sync, GUILayout.Width(depthListWidth_Left));
					
					//if (deltaDepth != _modMesh._extraValue._deltaDepth)//이전
					if (deltaDepth != deltaDepth_Sync)
					{
						if (isDepthSubOptionAvailable)
						{
							apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
														_editor, 
														_modifier, 
														//_modMesh._extraValue, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);

							//이전
							//_modMesh._extraValue._deltaDepth = deltaDepth;

							//변경 : 동기화
							SyncValue_Int(SYNC_VAR_TYPE_INT.DeltaDepth, deltaDepth);

							_editor.SetRepaint();
							apEditorUtil.ReleaseGUIFocus();
						}
					}
				}
				

				//if (GUILayout.Button(_editor.ImageSet.Get(apImageSet.PRESET.Rig_SubtractWeight), GUILayout.Width(depthListWidth_Left), GUILayout.Height(depthListHeight_LeftBtn)))
				if(apEditorUtil.ToggledButton(img_SubtractWeight, false, isDepthSubOptionAvailable, depthListWidth_Left, depthListHeight_LeftBtn))
				{
					//Depth 감소
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					//이전
					//_modMesh._extraValue._deltaDepth--;

					//변경 [동기화]
					SyncValue_Int_AddValue(SYNC_VAR_TYPE_INT.DeltaDepth, -1);

					_editor.SetRepaint();
					apEditorUtil.ReleaseGUIFocus();
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(depthListWidth_Right), GUILayout.Height(depthListHeight));
				// RenderUnit 리스트와 변환될 Depth 위치
				_scrollList = EditorGUILayout.BeginScrollView(_scrollList, false, true, GUILayout.Width(depthListWidth_Right), GUILayout.Height(depthListHeight));

				EditorGUILayout.BeginVertical(GUILayout.Width(depthListWidth_RightInner), GUILayout.Height(depthListHeight));
				GUILayout.Space(5);

				SubUnit curSubUnit = null;



				//int cursorDepth = _targetDepth + _modMesh._extraValue._deltaDepth;//이전

				//변경 : 동기화 관련 코드
				int cursorDepth = _targetDepth;
				bool isDepthCursorVisible = false;//추가. 이게 false면 커서는 보이지 않는다.
				if(_modSet_Main[TAB.Depth] != null)
				{
					cursorDepth = _targetDepth + _modSet_Main[TAB.Depth]._modMesh._extraValue._deltaDepth;
					isDepthCursorVisible = true;
				}
				

				//GUI Content 생성 [11.16 수정]
				if(_guiContent_DepthMidCursor == null) { _guiContent_DepthMidCursor = apGUIContentWrapper.Make(_img_DepthMidCursor); }
				if(_guiContent_DepthCursor == null) { _guiContent_DepthCursor = apGUIContentWrapper.Make(_img_DepthCursor); }
				if(_guiContent_MeshIcon == null) { _guiContent_MeshIcon = apGUIContentWrapper.Make(_img_MeshTF); }
				if(_guiContent_MeshGroupIcon == null) { _guiContent_MeshGroupIcon = apGUIContentWrapper.Make(_img_MeshGroupTF); }
				if(_guiContent_MeshIcon_Moved == null) { _guiContent_MeshIcon_Moved = apGUIContentWrapper.Make(_img_MeshTF_Moved); }
				if(_guiContent_MeshGroupIcon_Moved == null) { _guiContent_MeshGroupIcon_Moved = apGUIContentWrapper.Make(_img_MeshGroupTF_Moved); }
				
				int depthCursorSize = depthListHeight_RightList;

				int overMovedDepth = 0;
				SubUnit clickedSubUnit = null;
				for (int i = 0; i < _subUnits_All.Count; i++)
				{
					curSubUnit = _subUnits_All[i];
					overMovedDepth = 0;

					if (curSubUnit._selectedType != UNIT_SELECTED.None)
					{
						//타겟이면 배경색을 그려주자
						lastRect = GUILayoutUtility.GetLastRect();

						apEditorUtil.UNIT_BG_STYLE bgStyle = apEditorUtil.UNIT_BG_STYLE.Main;

						if (curSubUnit._selectedType == UNIT_SELECTED.Main)
						{
							//if (EditorGUIUtility.isProSkin)
							//{
							//	GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
							//}
							//else
							//{
							//	GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
							//}

							bgStyle = apEditorUtil.UNIT_BG_STYLE.Main;//v1.4.2
						}
						else
						{
							//같이 선택되었다면
							//if (EditorGUIUtility.isProSkin)
							//{
							//	GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
							//}
							//else
							//{
							//	GUI.backgroundColor = new Color(1.0f, 0.4f, 0.4f, 1.0f);
							//}

							bgStyle = apEditorUtil.UNIT_BG_STYLE.Sub;//v1.4.2
						}
						int yOffset = 3;
						if (i == 0)
						{
							yOffset = 4 - depthListHeight_RightList;
						}
						
						//GUI.Box(new Rect(lastRect.x, lastRect.y + depthListHeight_RightList + yOffset, depthListWidth_RightInner + 10, depthListHeight_RightList + 5), "");
						//GUI.backgroundColor = prevColor;


						//변경 v1.4.2
						apEditorUtil.DrawListUnitBG(	lastRect.x + 1,
														lastRect.y + depthListHeight_RightList + yOffset,
														depthListWidth_RightInner + 10 - 2,
														depthListHeight_RightList + 5,
														bgStyle);
					}

					EditorGUILayout.BeginHorizontal(GUILayout.Width(depthListWidth_RightInner), GUILayout.Height(depthListHeight_RightList));


					//TODO : Depth 커서 그려주기
					//GUILayout.Space(20);

					DEPTH_CURSOR_TYPE depthCursorType = DEPTH_CURSOR_TYPE.None;

					//가능한 경우만 (추가 21.10.2)
					if (isDepthCursorVisible)
					{
						if (curSubUnit._selectedType != UNIT_SELECTED.Main)
						{
							if (cursorDepth != _targetDepth)
							{
								if (cursorDepth == curSubUnit._depth_UI)
								{
									depthCursorType = DEPTH_CURSOR_TYPE.Target;
								}
								else
								{
									if (cursorDepth > _targetDepth)
									{
										//Depth가 증가했을 때
										if (_targetDepth < curSubUnit._depth_UI && curSubUnit._depth_UI < cursorDepth)
										{
											//만약 현재 유닛이 Top Empty이고 최대치를 넘겼다면
											if (curSubUnit._subUnitType == SUBUNIT_TYPE.Empty && cursorDepth > _subUnitDepth_Max)
											{
												//타겟으로 표기											
												depthCursorType = DEPTH_CURSOR_TYPE.Target;

												overMovedDepth = cursorDepth - _subUnitDepth_Max;//초과량 저장
											}
											else
											{
												//그외 : 중간
												depthCursorType = DEPTH_CURSOR_TYPE.Mid;
											}

										}
									}
									else
									{
										//Depth가 감소했을 때
										if (cursorDepth < curSubUnit._depth_UI && curSubUnit._depth_UI < _targetDepth)
										{
											//만약 현재 유닛이 Top Empty이고 최대치를 넘겼다면
											if (curSubUnit._subUnitType == SUBUNIT_TYPE.Empty && cursorDepth < _subUnitDepth_Min)
											{
												//타겟으로 표기
												depthCursorType = DEPTH_CURSOR_TYPE.Target;

												overMovedDepth = cursorDepth - _subUnitDepth_Min;//초과량 저장
											}
											else
											{
												depthCursorType = DEPTH_CURSOR_TYPE.Mid;
											}

										}
									}
								}
							}
						}
						else
						{
							if (cursorDepth != _targetDepth)
							{
								depthCursorType = DEPTH_CURSOR_TYPE.Mid;
							}
							else
							{
								depthCursorType = DEPTH_CURSOR_TYPE.Target;
							}
						}
					}

					GUILayout.Space(5);
					switch (depthCursorType)
					{
						case DEPTH_CURSOR_TYPE.None:
							GUILayout.Space(depthCursorSize + 4);
							break;

						case DEPTH_CURSOR_TYPE.Mid:
							EditorGUILayout.LabelField(_guiContent_DepthMidCursor.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));
							break;

						case DEPTH_CURSOR_TYPE.Target:
							EditorGUILayout.LabelField(_guiContent_DepthCursor.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));
							break;
					}

					//이전
					//EditorGUILayout.LabelField(curSubUnit._isMeshTransform ? _guiContent_MeshIcon.Content : _guiContent_MeshGroupIcon.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));

					//변경 (21.9.27)
					if(curSubUnit._subUnitType == SUBUNIT_TYPE.Empty)
					{
						//빈칸
						GUILayout.Space(depthCursorSize + 4);
					}					
					else
					{
						if(depthCursorType == DEPTH_CURSOR_TYPE.None || curSubUnit._selectedType == UNIT_SELECTED.Main)
						{
							//일반 아이콘
							EditorGUILayout.LabelField(curSubUnit._subUnitType == SUBUNIT_TYPE.MeshTransform ? _guiContent_MeshIcon.Content : _guiContent_MeshGroupIcon.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));
						}
						else
						{
							//위치가 교환되는 아이콘
							EditorGUILayout.LabelField(curSubUnit._subUnitType == SUBUNIT_TYPE.MeshTransform ? _guiContent_MeshIcon_Moved.Content : _guiContent_MeshGroupIcon_Moved.Content, GUILayout.Width(depthCursorSize), GUILayout.Height(depthCursorSize));
						}
						
					}
					
					
					EditorGUILayout.LabelField(curSubUnit._depth_UI.ToString(), GUILayout.Width(20), GUILayout.Height(depthListHeight_RightList));

					//이전 Label
					//> 버튼
					bool isButtonClicked = false;
					if(curSubUnit._subUnitType == SUBUNIT_TYPE.Empty && overMovedDepth != 0)
					{
						if(overMovedDepth > 0)
						{
							//EditorGUILayout.LabelField(	curSubUnit._name + "  +" + overMovedDepth,
							//						GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
							//						GUILayout.Height(depthListHeight_RightList)
							//						);

							if(GUILayout.Button(curSubUnit._name + "  +" + overMovedDepth,
													_guiStyle_Button_LikeLabel,
													GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
													GUILayout.Height(depthListHeight_RightList)
													))
							{
								isButtonClicked = true;
							}
						}
						else
						{
							//EditorGUILayout.LabelField(	curSubUnit._name + "  " + overMovedDepth,
							//						GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
							//						GUILayout.Height(depthListHeight_RightList)
							//						);

							if(GUILayout.Button(	curSubUnit._name + "  " + overMovedDepth,
													_guiStyle_Button_LikeLabel,
													GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
													GUILayout.Height(depthListHeight_RightList)
													))
							{
								isButtonClicked = true;
							}
						}
						
					}
					else
					{
						if (curSubUnit._depth_Delta != 0)//Delta 값은 안쓰기로..
						{
							if(curSubUnit._depth_Delta > 0)
							{
								//EditorGUILayout.LabelField(curSubUnit._name + " +" + curSubUnit._depth_Delta,
								//					GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
								//					GUILayout.Height(depthListHeight_RightList)
								//					);

								if(GUILayout.Button(curSubUnit._name + " +" + curSubUnit._depth_Delta,
													_guiStyle_Button_LikeLabel,
													GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
													GUILayout.Height(depthListHeight_RightList)
													))
								{
									isButtonClicked = true;
								}
							}
							else
							{
								//EditorGUILayout.LabelField(curSubUnit._name + " " + curSubUnit._depth_Delta,
								//					GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
								//					GUILayout.Height(depthListHeight_RightList)
								//					);

								if(GUILayout.Button(curSubUnit._name + " " + curSubUnit._depth_Delta,
													_guiStyle_Button_LikeLabel,
													GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
													GUILayout.Height(depthListHeight_RightList)
													))
								{
									isButtonClicked = true;
								}
							}
							
						}
						else
						{
							//EditorGUILayout.LabelField(curSubUnit._name,
							//						GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
							//						GUILayout.Height(depthListHeight_RightList)
							//						);

							if(GUILayout.Button(curSubUnit._name,
								_guiStyle_Button_LikeLabel,
													GUILayout.Width(depthListWidth_RightInner - (24 + 5 + depthCursorSize + depthCursorSize + 20 + 8)),
													GUILayout.Height(depthListHeight_RightList)
													))
							{
								isButtonClicked = true;
							}
						}
					}
					

					if(isButtonClicked)
					{
						clickedSubUnit = curSubUnit;
					}
					

					EditorGUILayout.EndHorizontal();
				}

				GUILayout.Space(depthListHeight + 100);
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndScrollView();
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();

				//클릭을 하면 다른걸 볼 수 있다.
				//단, 선택된 대상이어야 하며, 
				if(clickedSubUnit != null)
				{
					if(clickedSubUnit._linkedModSet != null
						&& clickedSubUnit._linkedModSet._isEditable_Depth
						&& clickedSubUnit._linkedModSet._renderUnit != null
						&& clickedSubUnit._linkedModSet._modMesh != null
						&& _modSet_Main[TAB.Depth] != clickedSubUnit._linkedModSet
						)
					{
						//메인을 바꾸자
						_modSet_Main[TAB.Depth] = clickedSubUnit._linkedModSet;

						RefreshSubUnits();
					}
				}
			}
			else
			{
				//5. Texture (RenderUnit이 MeshTransform인 경우)
				//- 현재 텍스쳐
				//- 바뀔 텍스쳐
				//- 텍스쳐 선택하기 버튼

				//"Changing Image"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_ChangingImage));
				GUILayout.Space(5);


				//동기화 변수
				bool isTextureChanged_Sync = true;
				bool isTextureChanged_Enabled = true;
				bool isTextureChanged_Available = true;

				CheckSync_Bool(SYNC_VAR_TYPE_BOOL.TextureEnabled, ref isTextureChanged_Sync, ref isTextureChanged_Enabled, ref isTextureChanged_Available);


				//"Image Option ON", "Image Option OFF"
				//이전
				//if (apEditorUtil.ToggledButton_2Side(_editor.GetText(TEXT.ExtraOpt_ImageOptOn), _editor.GetText(TEXT.ExtraOpt_ImageOptOff), _modMesh._extraValue._isTextureChanged, _isImageChangable && _modMesh._isExtraValueEnabled, width, 25))

				//변경 : 동기화
				if (apEditorUtil.ToggledButton_2Side_Sync(	_editor.GetText(TEXT.ExtraOpt_ImageOptOn), 
															_editor.GetText(TEXT.ExtraOpt_ImageOptOff),
															isTextureChanged_Enabled,
															isTextureChanged_Available && isExtraProp_Enabled && isExtraProp_Sync,
															isTextureChanged_Sync,
															width, 25))
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					//이전
					//_modMesh._extraValue._isTextureChanged = !_modMesh._extraValue._isTextureChanged;

					//변경 : 동기화
					bool nextTextureEnabled = isTextureChanged_Sync ? !isTextureChanged_Enabled : true;
					SyncValue_Bool(SYNC_VAR_TYPE_BOOL.TextureEnabled, nextTextureEnabled);

					_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<Modifier Link를 다시 해야한다.
					_editor.SetRepaint();
				}
				GUILayout.Space(5);

				//이전
				//bool isTextureSubOptionAvailable = _modMesh._extraValue._isTextureChanged && _isImageChangable && _modMesh._isExtraValueEnabled;

				//변경 (조건이 많다)
				bool isTextureSubOptionAvailable = isTextureChanged_Sync 
												&& isTextureChanged_Enabled 
												&& isTextureChanged_Available 
												&& isExtraProp_Enabled 
												&& isExtraProp_Sync;

				int imageSlotSize = 170;
				int imageSlotSpaceSize = width - (imageSlotSize * 2 + 6 + 10);
				int imageSlotHeight = imageSlotSize + 50;

				Texture2D img_Src = null;
				Texture2D img_Dst = null;
				string strSrcName = "< None >";
				string strDstName = "< None >";

				//이전
				//if (_srcTexureData != null && _srcTexureData._image != null)
				//{
				//	img_Src = _srcTexureData._image;
				//	strSrcName = _srcTexureData._name;
				//}
				//if (_dstTexureData != null && _dstTexureData._image != null)
				//{
				//	img_Dst = _dstTexureData._image;
				//	strDstName = _dstTexureData._name;
				//}

				//변경 : 동기화
				if (_isSrcTextureSync)
				{
					//동기화가 된 경우
					if (_srcTexureData != null && _srcTexureData._image != null)
					{
						img_Src = _srcTexureData._image;
						strSrcName = _srcTexureData._name;
					}
					else
					{
						img_Src = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_NonImage);
					}
				}
				else
				{
					//동기화가 안된 경우
					img_Src = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_UnsyncImage);
					strSrcName = "< Inconsistent >";
				}

				if (_isDstTextureSync)
				{
					//동기화가 된 경우
					if (_dstTexureData != null && _dstTexureData._image != null)
					{
						img_Dst = _dstTexureData._image;
						strDstName = _dstTexureData._name;
					}
					else
					{
						img_Dst = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_NonImage);
					}
				}
				else
				{
					//동기화가 안된 경우
					img_Dst = _editor.ImageSet.Get(apImageSet.PRESET.ExtraOption_UnsyncImage);
					strDstName = "< Inconsistent >";
				}
				

				GUIStyle guiStyle_ImageSlot = new GUIStyle(GUI.skin.box);
				guiStyle_ImageSlot.alignment = TextAnchor.MiddleCenter;

				GUIStyle guiStyle_ImageName = new GUIStyle(GUI.skin.label);
				guiStyle_ImageName.alignment = TextAnchor.MiddleCenter;

				EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(imageSlotHeight));
				GUILayout.Space(5);

				//이미지 슬롯 1 : 원래 이미지
				EditorGUILayout.BeginVertical(GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotHeight));
				//"Original"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_SlotOriginal), GUILayout.Width(imageSlotSize));
				GUILayout.Box(img_Src, guiStyle_ImageSlot, GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(strSrcName, guiStyle_ImageName, GUILayout.Width(imageSlotSize));
				EditorGUILayout.EndVertical();

				GUILayout.Space(imageSlotSpaceSize);

				//이미지 슬롯 1 : 원래 이미지
				EditorGUILayout.BeginVertical(GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotHeight));
				//"Changed"
				EditorGUILayout.LabelField(_editor.GetText(TEXT.ExtraOpt_SlotChanged), GUILayout.Width(imageSlotSize));
				GUILayout.Box(img_Dst, guiStyle_ImageSlot, GUILayout.Width(imageSlotSize), GUILayout.Height(imageSlotSize));
				GUILayout.Space(5);
				EditorGUILayout.LabelField(strDstName, guiStyle_ImageName, GUILayout.Width(imageSlotSize));
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
				//"Set Image"
				if(apEditorUtil.ToggledButton(_editor.GetText(TEXT.ExtraOpt_SelectImage), false, isTextureSubOptionAvailable, width, 30))
				{
					//이미지 열기 열기
					_loadKey_TextureSelect = apDialog_SelectTextureData.ShowDialog(_editor, null, OnTextureDataSelected);
				}
				//"Reset Image"
				if (GUILayout.Button(_editor.GetText(TEXT.ExtraOpt_ResetImage), GUILayout.Width(width), GUILayout.Height(20)))
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);

					//이전
					//_modMesh._extraValue._textureDataID = -1;
					//_modMesh._extraValue._linkedTextureData = null;

					//변경 : 동기화
					SyncValue_Texture(SYNC_VAR_TYPE_TEXTURE.DstTexture, null);
					
					RefreshImagePreview();

					Repaint();
				}
			}
			
			GUILayout.Space(10);
			apEditorUtil.GUI_DelimeterBoxH(width);
			GUILayout.Space(10);
			//"Close"
			if(GUILayout.Button(_editor.GetText(TEXT.Close), GUILayout.Height(30)))
			{
				isClose = true;
			}
			if(isClose)
			{
				CloseDialog();
			}

			if(isMoveAnimKeyframe)
			{
				//키프레임 이동의 경우,
				//타임라인 레이어를 따라서 전,후로 이동한다.
				//이때 ModMesh가 아예 바뀌기 때문에 여기서 처리해야한다.
				//이전 > 단일 키프레임에서의 이동
				//변경 > 모든 키프레임에서 Prev-Next 키프레임 이동

				//이전
				//if(_keyframe != null && _keyframe._parentTimelineLayer != null && _animClip != null)
				//{
				//	apAnimKeyframe moveKeyframe = (isMoveAnimKeyframeToNext ? _keyframe._nextLinkedKeyframe : _keyframe._prevLinkedKeyframe);
				//	if(moveKeyframe != null && moveKeyframe._linkedModMesh_Editor != null)
				//	{
				//		_keyframe = moveKeyframe;
				//		_modMesh = _keyframe._linkedModMesh_Editor;
				//		_animClip.SetFrame_Editor(moveKeyframe._frameIndex);

				//		RefreshImagePreview();

				//		apEditorUtil.ReleaseGUIFocus();

				//		Repaint();
				//		_editor.SetRepaint();
				//	}
				//}

				
				if(_keyframe_Base != null 
					&& _keyframe_Base._parentTimelineLayer != null 
					&& _animClip != null)
				{
					//- ModSet의 키프레임들의 이전/다음 키프레임을 찾아서 연결
					//- Keyframe 리스트와 Base 다시 설정

					if(_nModSets > 0)
					{
						ModSet curModSet = null;
						apAnimKeyframe curKeyframe = null;
						if(_keyframes == null)
						{
							_keyframes = new List<apAnimKeyframe>();
						}
						_keyframes.Clear();

						bool isBaseKeyframeFind = false;

						for (int iModSet = 0; iModSet < _nModSets; iModSet++)
						{
							curModSet = _modSets[iModSet];
							curKeyframe = curModSet._keyframe;
							
							if(curKeyframe == null)
							{
								continue;
							}

							bool isBaseModSet = false;
							if(!isBaseKeyframeFind)
							{
								if(curKeyframe == _keyframe_Base)
								{
									//이게 Base 키프레임에 해당하는 ModSet이다.
									isBaseModSet = true;
									isBaseKeyframeFind = true;
								}
							}	

							if (isMoveAnimKeyframeToNext)
							{
								//다음 키프레임으로 이동
								if(curModSet._keyframe._nextLinkedKeyframe != null)
								{
									curModSet._keyframe = curModSet._keyframe._nextLinkedKeyframe;
									curModSet._modMesh = curModSet._keyframe._linkedModMesh_Editor;
								}
							}
							else
							{
								//이전 키프레임으로 이동
								if(curModSet._keyframe._prevLinkedKeyframe != null)
								{
									curModSet._keyframe = curModSet._keyframe._prevLinkedKeyframe;
									curModSet._modMesh = curModSet._keyframe._linkedModMesh_Editor;
								}
							}

							if(isBaseModSet)
							{
								_keyframe_Base = curModSet._keyframe;
							}

							_keyframes.Add(curModSet._keyframe);
						}

						if(_keyframe_Base == null && _keyframes.Count > 0)
						{
							_keyframe_Base = _keyframes[0];
						}

						if(_keyframes.Count > 0)
						{
							_editor.Select.SelectAnimMultipleKeyframes(_keyframes, apGizmos.SELECT_TYPE.New, false);

							if(_keyframe_Base != null)
							{
								_animClip.SetFrame_Editor(_keyframe_Base._frameIndex);
							}
						}

						RefreshImagePreview();

						apEditorUtil.ReleaseGUIFocus();

						Repaint();
						_editor.SetRepaint();
					}
				}
			}
		}





		// 동기화 함수
		//-------------------------------------------------------------------------------------
		public enum SYNC_VAR_TYPE_BOOL
		{
			ExtraOption,
			DepthEnabled,
			TextureEnabled,
		}

		public enum SYNC_VAR_TYPE_FLOAT
		{
			WeightCutout_Normal,
			WeightCutout_AnimPrev,
			WeightCutout_AnimNext,
		}

		public enum SYNC_VAR_TYPE_INT
		{
			DeltaDepth
		}

		public enum SYNC_VAR_TYPE_TEXTURE
		{
			SrcTexture,
			DstTexture
		}

		private void CheckSync_Bool(SYNC_VAR_TYPE_BOOL syncVarType, ref bool isSync, ref bool isEnabledAll, ref bool isAvailable)
		{
			if(_nModSets == 0)
			{
				isAvailable = false;
				isSync = true;
				isEnabledAll = false;
				return;
			}

			isSync = true;
			isEnabledAll = false;
			isAvailable = true;
			bool isFirstValue = true;
			int nChecked = 0;
			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_BOOL.ExtraOption:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								if(isFirstValue)
								{
									isEnabledAll = curModSet._modMesh._isExtraValueEnabled;
									isFirstValue = false;
								}
								else
								{
									if(isEnabledAll != curModSet._modMesh._isExtraValueEnabled)
									{
										//동기화가 안되었다
										isSync = false;
										return;
									}
								}
								nChecked += 1;
							}
						}
						break;

					case SYNC_VAR_TYPE_BOOL.DepthEnabled:
						{
							if(curModSet._isEditable_Depth)
							{
								if(isFirstValue)
								{
									isEnabledAll = curModSet._modMesh._extraValue._isDepthChanged;
									isFirstValue = false;
								}
								else
								{
									if(isEnabledAll != curModSet._modMesh._extraValue._isDepthChanged)
									{
										//동기화가 안되었다
										isSync = false;
										return;
									}
								}
								nChecked += 1;
							}
						}
						break;

					case SYNC_VAR_TYPE_BOOL.TextureEnabled:
						{
							if(curModSet._isEditable_Image)
							{
								if(isFirstValue)
								{
									isEnabledAll = curModSet._modMesh._extraValue._isTextureChanged;
									isFirstValue = false;
								}
								else
								{
									if(isEnabledAll != curModSet._modMesh._extraValue._isTextureChanged)
									{
										//동기화가 안되었다
										isSync = false;
										return;
									}
								}
								nChecked += 1;
							}
						}
						break;
				}
			}

			if(nChecked == 0)
			{
				//대상이 없다.
				isAvailable = false;
				isSync = true;
				isEnabledAll = false;
			}

		}


		private void CheckSync_Float(SYNC_VAR_TYPE_FLOAT syncVarType, ref bool isSync, ref float syncFloatValue, float defaultValue)
		{
			if(_nModSets == 0)
			{
				isSync = true;
				syncFloatValue = defaultValue;
				return;
			}

			isSync = true;
			syncFloatValue = defaultValue;
			
			bool isFirstValue = true;
			int nChecked = 0;
			ModSet curModSet = null;

			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_FLOAT.WeightCutout_Normal:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								if(isFirstValue)
								{
									syncFloatValue = curModSet._modMesh._extraValue._weightCutout;
									isFirstValue = false;
								}
								else
								{
									if(Mathf.Abs(syncFloatValue - curModSet._modMesh._extraValue._weightCutout) > 0.001f)
									{
										//동기화가 안되었다
										isSync = false;
										return;
									}
								}
								nChecked += 1;
							}
						}
						break;

					case SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimPrev:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								if(isFirstValue)
								{
									syncFloatValue = curModSet._modMesh._extraValue._weightCutout_AnimPrev;
									isFirstValue = false;
								}
								else
								{
									if(Mathf.Abs(syncFloatValue - curModSet._modMesh._extraValue._weightCutout_AnimPrev) > 0.001f)
									{
										//동기화가 안되었다
										isSync = false;
										return;
									}
								}
								nChecked += 1;
							}
						}
						break;

					case SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimNext:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								if(isFirstValue)
								{
									syncFloatValue = curModSet._modMesh._extraValue._weightCutout_AnimNext;
									isFirstValue = false;
								}
								else
								{
									if(Mathf.Abs(syncFloatValue - curModSet._modMesh._extraValue._weightCutout_AnimNext) > 0.001f)
									{
										//동기화가 안되었다
										isSync = false;
										return;
									}
								}
								nChecked += 1;
							}
						}
						break;
				}
			}

			if(nChecked == 0)
			{
				//대상이 없다.
				isSync = true;
				syncFloatValue = defaultValue;
			}

		}





		private void CheckSync_Int(SYNC_VAR_TYPE_INT syncVarType, ref bool isSync, ref int intValue, int defaultValue)
		{
			if(_nModSets == 0)
			{
				isSync = true;
				intValue = defaultValue;
				return;
			}

			isSync = true;
			intValue = defaultValue;

			bool isFirstValue = true;
			int nChecked = 0;
			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_INT.DeltaDepth:
						{
							if(curModSet._isEditable_Depth)
							{
								if(isFirstValue)
								{
									intValue = curModSet._modMesh._extraValue._deltaDepth;
									isFirstValue = false;
								}
								else
								{
									if(intValue != curModSet._modMesh._extraValue._deltaDepth)
									{
										//동기화가 안되었다
										isSync = false;
										return;
									}
								}
								nChecked += 1;
							}
						}
						break;
				}
			}

			if(nChecked == 0)
			{
				//대상이 없다.
				isSync = true;
				intValue = defaultValue;
			}

		}
		



		private apTextureData CheckSync_Texture(SYNC_VAR_TYPE_TEXTURE syncVarType, ref bool isSync)
		{
			if(_nModSets == 0)
			{
				isSync = true;
				return null;
			}

			isSync = true;
			apTextureData resultTexture = null;

			bool isFirstValue = true;
			int nChecked = 0;
			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_TEXTURE.SrcTexture:
						{
							if(curModSet._isEditable_Image
								&& curModSet._renderUnit._meshTransform != null
								&& curModSet._renderUnit._meshTransform._mesh != null
								&& curModSet._renderUnit._meshTransform._mesh._textureData_Linked != null)
							{
								if(isFirstValue)
								{
									resultTexture = curModSet._renderUnit._meshTransform._mesh._textureData_Linked;
									isFirstValue = false;
								}
								else
								{
									if(resultTexture != curModSet._renderUnit._meshTransform._mesh._textureData_Linked)
									{
										//동기화가 안되었다
										isSync = false;
										return null;
									}
								}
								nChecked += 1;
							}
						}
						break;

					case SYNC_VAR_TYPE_TEXTURE.DstTexture:
						{
							if(curModSet._isEditable_Image
								&& curModSet._renderUnit._meshTransform != null
								&& curModSet._renderUnit._meshTransform._mesh != null)
							{
								apTextureData linkedTextureData = null;
								if(curModSet._modMesh._extraValue._textureDataID >= 0)
								{
									if(curModSet._modMesh._extraValue._linkedTextureData != null)
									{
										linkedTextureData = curModSet._modMesh._extraValue._linkedTextureData;
									}
									else
									{
										linkedTextureData = _portrait.GetTexture(curModSet._modMesh._extraValue._textureDataID);
										if(linkedTextureData == null)
										{
											curModSet._modMesh._extraValue._textureDataID = -1;
										}
									}
									
								}

								if(isFirstValue)
								{
									resultTexture = linkedTextureData;
									isFirstValue = false;
								}
								else
								{
									if(resultTexture != linkedTextureData)
									{
										//동기화가 안되었다
										isSync = false;
										return null;
									}
								}
								nChecked += 1;
							}
						}
						break;
				}
			}

			if(nChecked == 0)
			{
				//대상이 없다.
				isSync = true;
				return null;
			}

			return resultTexture;

		}


		private void SyncValue_Bool(SYNC_VAR_TYPE_BOOL syncVarType, bool isEnabled)
		{
			if(_nModSets == 0)
			{
				return;
			}

			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_BOOL.ExtraOption:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								curModSet._modMesh._isExtraValueEnabled = isEnabled;
							}
						}
						break;

					case SYNC_VAR_TYPE_BOOL.DepthEnabled:
						{
							if(curModSet._isEditable_Depth)
							{
								curModSet._modMesh._extraValue._isDepthChanged = isEnabled;
							}
						}
						break;

					case SYNC_VAR_TYPE_BOOL.TextureEnabled:
						{
							if(curModSet._isEditable_Image)
							{
								curModSet._modMesh._extraValue._isTextureChanged = isEnabled;
							}
						}
						break;
				}
			}
		}

		private void SyncValue_Float(SYNC_VAR_TYPE_FLOAT syncVarType, float floatValue)
		{
			if(_nModSets == 0)
			{
				return;
			}

			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_FLOAT.WeightCutout_Normal:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								curModSet._modMesh._extraValue._weightCutout = floatValue;
							}
						}
						break;

					case SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimPrev:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								curModSet._modMesh._extraValue._weightCutout_AnimPrev = floatValue;
							}
						}
						break;

					case SYNC_VAR_TYPE_FLOAT.WeightCutout_AnimNext:
						{
							if(curModSet._isEditable_Depth || curModSet._isEditable_Image)
							{
								curModSet._modMesh._extraValue._weightCutout_AnimNext = floatValue;
							}
						}
						break;
				}
			}
		}



		private void SyncValue_Int(SYNC_VAR_TYPE_INT syncVarType, int intValue)
		{
			if(_nModSets == 0)
			{
				return;
			}

			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_INT.DeltaDepth:
						{
							if(curModSet._isEditable_Depth)
							{
								curModSet._modMesh._extraValue._deltaDepth = intValue;
							}
						}
						break;
				}
			}
		}


		private void SyncValue_Int_AddValue(SYNC_VAR_TYPE_INT syncVarType, int addValue)
		{
			if(_nModSets == 0)
			{
				return;
			}

			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_INT.DeltaDepth:
						{
							if(curModSet._isEditable_Depth)
							{
								curModSet._modMesh._extraValue._deltaDepth += addValue;
							}
						}
						break;
				}
			}
		}



		private void SyncValue_Texture(SYNC_VAR_TYPE_TEXTURE syncVarType, apTextureData textureData)
		{
			if(_nModSets == 0)
			{
				return;
			}

			ModSet curModSet = null;
			for (int i = 0; i < _nModSets; i++)
			{
				curModSet = _modSets[i];
				if(curModSet._modMesh == null)
				{
					continue;
				}

				switch (syncVarType)
				{
					case SYNC_VAR_TYPE_TEXTURE.DstTexture:
						{
							if(curModSet._isEditable_Image
								&& curModSet._renderUnit._meshTransform != null
								&& curModSet._renderUnit._meshTransform._mesh != null)
							{
								if(textureData != null)
								{
									curModSet._modMesh._extraValue._textureDataID = textureData._uniqueID;
									curModSet._modMesh._extraValue._linkedTextureData = textureData;
								}
								else
								{
									curModSet._modMesh._extraValue._textureDataID = -1;
									curModSet._modMesh._extraValue._linkedTextureData = null;
								}
							}
						}
						break;
				}
			}
		}



		// SubUnit 갱신
		//-------------------------------------------------------------------------------------
		private void RefreshSubUnits()
		{
			if (_subUnits_All == null)
			{
				_subUnits_All = new List<SubUnit>();
			}
			_subUnits_All.Clear();



			apRenderUnit parentUnit = (_isDepthEditable && _modSet_Main[TAB.Depth] != null) ? _modSet_Main[TAB.Depth]._renderUnit._parentRenderUnit : null;
			apRenderUnit curRenderUnit = null;
			ModSet curLinkedModSet = null;
			for (int i = 0; i < _meshGroup._renderUnits_All.Count; i++)
			{
				curRenderUnit = _meshGroup._renderUnits_All[i];

				//Parent가 같은 형제 렌더 유닛에 대해서만 처리한다.
				//단, MeshTransform일 때, Clipping Child는 생략한다.
				if (curRenderUnit._meshTransform != null && curRenderUnit._meshTransform._isClipping_Child)
				{
					continue;
				}

				if (curRenderUnit._parentRenderUnit != parentUnit)
				{
					continue;
				}

				if (_isDepthEditable)
				{
					curLinkedModSet = null;
					//SubUnit subUnit = new SubUnit(curRenderUnit, curRenderUnit._level, (curRenderUnit == _modSet_Main[TAB.Depth]._renderUnit), (curRenderUnit == _meshGroup._rootRenderUnit));

					UNIT_SELECTED selectedType = UNIT_SELECTED.None;
					if (curRenderUnit == _modSet_Main[TAB.Depth]._renderUnit)
					{
						selectedType = UNIT_SELECTED.Main;
						curLinkedModSet = _modSet_Main[TAB.Depth];
					}
					else
					{
						curLinkedModSet = _modSets.Find(delegate (ModSet a)
						{
							return a._renderUnit == curRenderUnit && a._isEditable_Depth && a._modMesh != null;
						});

						if (curLinkedModSet != null)
						{
							selectedType = UNIT_SELECTED.Sub;
						}
					}

					SubUnit subUnit = new SubUnit(curRenderUnit, selectedType, curLinkedModSet);
					_subUnits_All.Add(subUnit);

					////만약 SelectedType이 Main이나 Sub라면,
					////예상 위치도 만들어야 한다. (옵션이 켜진 경우에만)
					////경우의 수가 많으니 Main만
					//if ((selectedType == UNIT_SELECTED.Main)
					//	&& curLinkedModSet != null
					//	&& curLinkedModSet._modMesh != null
					//	&& curLinkedModSet._modMesh._isExtraValueEnabled
					//	&& curLinkedModSet._modMesh._extraValue._isDepthChanged
					//	&& curLinkedModSet._modMesh._extraValue._deltaDepth != 0)
					//{
					//	SubUnit movedSubUnit = new SubUnit(curLinkedModSet, subUnit);
					//	_subUnits_All.Add(movedSubUnit);
					//}
				}
				else
				{
					//SubUnit subUnit = new SubUnit(curRenderUnit, curRenderUnit._level, false, (curRenderUnit == _meshGroup._rootRenderUnit));
					SubUnit subUnit = new SubUnit(curRenderUnit, UNIT_SELECTED.None, null);
					_subUnits_All.Add(subUnit);
				}

			}


			//Sort 방법

			//증가 방향 : Expected가 큰게 먼저 (내림차순)
			//감소 방향 : Expected가 작은게 먼저 (오름차순)
			//방향이 다른 경우 : 증가 방향 먼저
			//Expected가 같은 경우 : 해당 순서로 Original 비교

			//참고 apSortedRenderBuffer : 982

			_subUnits_All.Sort(delegate (SubUnit a, SubUnit b)
			{
				return b._depth_Result - a._depth_Result;
			});

			//추가 21.9.27
			//맨 위와 맨 아래에 하나 더 붙인다.
			List<SubUnit> emptyAddedList = new List<SubUnit>();
			emptyAddedList.Add(new SubUnit("(Top)"));//맨 앞에 하나 넣고
			//리스트 복사하고
			for (int i = 0; i < _subUnits_All.Count; i++)
			{
				emptyAddedList.Add(_subUnits_All[i]);
			}
			emptyAddedList.Add(new SubUnit("(Bottom)"));//맨 뒤에 하나더 넣고

			//리스트 교체
			_subUnits_All = emptyAddedList;

			//여기서는 실제 Depth보다 상대적 Depth만 고려한다.
			int curDepth = 0;

			_subUnitDepth_Max = 0;
			_subUnitDepth_Min = 0;
			for (int i = _subUnits_All.Count - 1; i >= 0; i--)
			{
				_subUnits_All[i]._depth_UI = curDepth;
				if(_isDepthEditable && _subUnits_All[i]._selectedType == UNIT_SELECTED.Main)
				{
					_targetDepth = curDepth;
				}
				_subUnitDepth_Max = Mathf.Max(curDepth, _subUnitDepth_Max);
				curDepth++;
			}
		}


		//텍스쳐 선택 이벤트
		//-------------------------------------------------------------------------------------
		private object _loadKey_TextureSelect = null;
		private void OnTextureDataSelected(bool isSuccess, apMesh targetMesh, object loadKey, apTextureData resultTextureData)
		{
			if(!isSuccess)
			{
				_loadKey_TextureSelect = null;
				return;
			}

			if(_loadKey_TextureSelect != loadKey)
			{
				_loadKey_TextureSelect = null;
				return;
			}

			_loadKey_TextureSelect = null;

			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_ExtraOptionChanged, 
													_editor, 
													_modifier, 
													//_modMesh._extraValue, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			
			
			//이전
			////일단 초기화
			//_modMesh._extraValue._textureDataID = -1;
			//_modMesh._extraValue._linkedTextureData = null;
			//_dstTexureData = null;
			

			//if(_modMesh != null && _modMesh._isMeshTransform && _modMesh._transform_Mesh != null)
			//{
				
			//	if(resultTextureData != null)
			//	{
			//		_modMesh._extraValue._textureDataID = resultTextureData._uniqueID;
			//	}
			//}


			//if (_modMesh._extraValue._textureDataID >= 0)
			//{
			//	_dstTexureData = _portrait.GetTexture(_modMesh._extraValue._textureDataID);
			//	_modMesh._extraValue._linkedTextureData = _dstTexureData;

			//	if (_dstTexureData == null)
			//	{
			//		_modMesh._extraValue._textureDataID = -1;
			//	}
			//}

			//변경 : 동기화
			SyncValue_Texture(SYNC_VAR_TYPE_TEXTURE.DstTexture, resultTextureData);

			RefreshImagePreview();

			Repaint();

			
			_meshGroup.RefreshModifierLink(apUtil.LinkRefresh.Set_MeshGroup_Modifier(_meshGroup, _modifier));//<<Modifier Link를 다시 해야한다.
			_editor.SetRepaint();
		}
		
	}
}