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
	/// 추가 21.2.18 : 에디터 GUI에 출력하는 아이콘들을 출력하는 클래스
	/// </summary>
	public class apGUIStatBox
	{
		// Sub Class
		//-----------------------------------------------
		//각각의 아이콘의 보여주기 여부와 위치를 결정한다.
		//애니메이션 효과는 두가지이다.
		//- 나타나고 숨기기 : 투명도 효과와 함께 오른쪽에서 원 위치로 약간 이동한다. 오프셋을 계산한다.
		//- 이동하기 (밀려나기) : 다른 아이콘에 의해서 이동하는 애니메이션. "원위치" 자체를 이동시킨다.
		//- 텍스쳐를 바꾸는 효과 : 위치는 그대로, 색상만 반짝거린다. 단, 숨겨진 경우엔 바로 바꾼다.

		public class IconUnit
		{
			public ICON_TYPE _iconType;
			public Texture2D _img;
		
			public bool _isVisible = false;
			
			private bool _isMoving = false;
			private float _tMove = 0.0f;
			private const float MOVE_LENGTH = 0.25f;
			private Vector2 _movePrevPos = Vector2.zero;
			private Vector2 _moveNextPos = Vector2.zero;
			
			private bool _isImgChanging = false;
			private float _tImgChange = 0.0f;
			private const float IMG_LENGTH = 0.4f;
			//private Color _imgCurColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			

			private Color CHANGING_COLOR = new Color(3.0f, 3.0f, 3.0f, 1.0f);
			private Color DEFAULT_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			private float CHANGING_SCALE = 1.4f;
			private float DEFAULT_SCALE = 1.0f;



			public Vector2 _pos = Vector2.zero;
			public Color _color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			public float _scaleRatio = 1.0f;



			public IconUnit(ICON_TYPE iconType)
			{
				_iconType = iconType;
				Clear();
			}

			public void Clear()
			{
				_img = null;
				_isVisible = false;

				_isMoving = false;
				_tMove = 0.0f;
				_movePrevPos = Vector2.zero;
				_moveNextPos = Vector2.zero;
				
				_isImgChanging = false;
				_tImgChange = 0.0f;
			
				_pos = Vector2.zero;
				_color = DEFAULT_COLOR;
				_scaleRatio = DEFAULT_SCALE;
			}

			public void Update(float tDelta)
			{
				if(!_isVisible)
				{
					return;
				}

				//위치 변화 먼저
				if(_isMoving)
				{
					_tMove += tDelta;
					if(_tMove > MOVE_LENGTH)
					{
						//이동 애니메이션 종료
						_isMoving = false;
						_tMove = 0.0f;
						_pos = _moveNextPos;
					}
					else
					{
						float moveLerp = GetAccLerp(_tMove / MOVE_LENGTH);
						_pos = _movePrevPos * (1.0f - moveLerp) + (_moveNextPos * moveLerp);
					}
				}

				//색상 효과도
				if(_isImgChanging)
				{
					_tImgChange += tDelta;
					if(_tImgChange > IMG_LENGTH)
					{
						_isImgChanging = false;
						_tImgChange = 0.0f;
						_color = DEFAULT_COLOR;
						_scaleRatio = DEFAULT_SCALE;
					}
					else
					{
						float colorLerp = GetDccLerp(_tImgChange / IMG_LENGTH);
						float scaleLerp = GetAccLerp(_tImgChange / IMG_LENGTH);
						_color = CHANGING_COLOR * (1.0f - colorLerp) + DEFAULT_COLOR * colorLerp;
						_scaleRatio = CHANGING_SCALE * (1.0f - scaleLerp) + DEFAULT_SCALE * scaleLerp;
					}
				}

			}

			
			/// <summary>
			/// 이미지를 변경한다. (Show/Hide보다 이 함수가 먼저 호출되어야 한다.)
			/// 비활성화 하는 경우엔 null을 입력하자
			/// </summary>
			/// <param name="img"></param>
			public void Show(Texture2D img, Vector2 pos)
			{
				//안보였다면 위치는 그대로 지정
				if (!_isVisible)
				{
					_isMoving = true;
					_tMove = 0.0f;
					_movePrevPos = pos;
					_moveNextPos = pos;
					_pos = pos;
				}
				else
				{	
					_isMoving = true;
					_tMove = 0.0f;
					_movePrevPos = _pos;
					_moveNextPos = pos;
				}


				if(_img != img)
				{
					//상태가 바뀌었다면
					_img = img;
					_isImgChanging = true;
					_tImgChange = 0.0f;
					_color = CHANGING_COLOR;
					_scaleRatio = CHANGING_SCALE;
				}

				_isVisible = true;
			}

			public void Hide()
			{
				_img = null;
				_isVisible = false;
				_isImgChanging = false;
				_tImgChange = 0.0f;
				_color = DEFAULT_COLOR;
				_scaleRatio = DEFAULT_SCALE;

				_isMoving = false;
				_tMove = 0.0f;
				_movePrevPos = _pos;
				_moveNextPos = _pos;
			}


		}

		// Members
		//-----------------------------------------------
		public apEditor _editor = null;
		public apPortrait _prevPortrait = null;//Portrait가 바뀔때마다 전체 리셋
		
		//아이콘 타입들
		//왼쪽엔 ViewStat, 오른쪽엔 EditStat이 있어야 한다.
		//ViewStat과 EditStat이 둘다 하나 이상 나올 경우에만 가운데 구분자가 나오고, 그렇지 않으면 사라진다.
		public enum ICON_TYPE
		{
			//보기 옵션들 (왼쪽부터 나온다.)
			View_LowCPU,
			View_MeshHidden,
			View_BoneHiddenOrOutline,
			View_PhysicsDisabled,
			View_OnionSkin,
			View_VisibilityPreset,
			View_Rotoscoping,
			//편집 옵션들
			Edit_SingleMultiModifier,
			Edit_PreviewResult,
			Edit_SelectionLock,
		}

		
			

		//이미지들
		private Texture2D _img_Delimeter = null;
		private Texture2D _img_LowCPU = null;
		private Texture2D _img_MeshHidden = null;
		private Texture2D _img_BoneHidden = null;
		private Texture2D _img_BoneOutline = null;
		private Texture2D _img_PhysicsDisabled = null;
		private Texture2D _img_OnionSkin = null;
		private Texture2D _img_VisibilityPreset = null;
		private Texture2D _img_Rotoscoping = null;
		private Texture2D _img_SingleModifier = null;
		private Texture2D _img_MultiModifiers = null;
		private Texture2D _img_MultiModifiers_Impossible = null;
		private Texture2D _img_PreviewBones = null;
		private Texture2D _img_PreviewColors = null;
		private Texture2D _img_PreviewBonesAndColors = null;
		private Texture2D _img_SelectionLock = null;
		private Texture2D _img_SelectionUnlock = null;
		private Texture2D _img_SelectionSemiLock = null;

		//조건값들 (각각 다르다)
		//값들
		private enum VALUE_VIEW_LOWCPU { None, LowCPU }
		private enum VALUE_VIEW_MESH { Shown, Hidden }
		private enum VALUE_VIEW_BONE { Shown, Hidden, Outline }
		private enum VALUE_VIEW_PHYSICS { Enabled, Disabled }
		private enum VALUE_VIEW_ONION_SKIN { None, OnionSkin }
		private enum VALUE_VIEW_VISIBILITY_PRESET { None, VisbilityPreset }
		private enum VALUE_VIEW_ROTOSCOPING { None, Rotoscoping }
		private enum VALUE_EDIT_MODIFIER { NotEdit, Single, Multiple, MultipleButImpossible }
		private enum VALUE_EDIT_PREVIEW { NoPreview, PreviewBone, PreviewColor, PreviewBoneAndColor }
		private enum VALUE_EDIT_SELECTIONLOCK { NotEdit, Lock, Unlock, SemiLock }

		//아이콘이 안보이도록 만드는 설정이 기본값
		private VALUE_VIEW_LOWCPU _prevViewLowCpu = VALUE_VIEW_LOWCPU.None;
		private VALUE_VIEW_MESH _prevViewMesh = VALUE_VIEW_MESH.Shown;
		private VALUE_VIEW_BONE _prevViewBone = VALUE_VIEW_BONE.Shown;
		private VALUE_VIEW_PHYSICS _prevViewPhysics = VALUE_VIEW_PHYSICS.Enabled;
		private VALUE_VIEW_ONION_SKIN _prevViewOnionSkin = VALUE_VIEW_ONION_SKIN.None;
		private VALUE_VIEW_VISIBILITY_PRESET _prevViewVisibilityPreset = VALUE_VIEW_VISIBILITY_PRESET.None;
		private VALUE_VIEW_ROTOSCOPING _prevViewRotoscoping = VALUE_VIEW_ROTOSCOPING.None;
		private VALUE_EDIT_MODIFIER _prevEditModifier = VALUE_EDIT_MODIFIER.NotEdit;
		private VALUE_EDIT_PREVIEW _prevEditPreview = VALUE_EDIT_PREVIEW.NoPreview;
		private VALUE_EDIT_SELECTIONLOCK _prevEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.NotEdit;

		private VALUE_VIEW_LOWCPU _curViewLowCpu = VALUE_VIEW_LOWCPU.None;
		private VALUE_VIEW_MESH _curViewMesh = VALUE_VIEW_MESH.Shown;
		private VALUE_VIEW_BONE _curViewBone = VALUE_VIEW_BONE.Shown;
		private VALUE_VIEW_PHYSICS _curViewPhysics = VALUE_VIEW_PHYSICS.Enabled;
		private VALUE_VIEW_ONION_SKIN _curViewOnionSkin = VALUE_VIEW_ONION_SKIN.None;
		private VALUE_VIEW_VISIBILITY_PRESET _curViewVisibilityPreset = VALUE_VIEW_VISIBILITY_PRESET.None;
		private VALUE_VIEW_ROTOSCOPING _curViewRotoscoping = VALUE_VIEW_ROTOSCOPING.None;
		private VALUE_EDIT_MODIFIER _curEditModifier = VALUE_EDIT_MODIFIER.NotEdit;
		private VALUE_EDIT_PREVIEW _curEditPreview = VALUE_EDIT_PREVIEW.NoPreview;
		private VALUE_EDIT_SELECTIONLOCK _curEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.NotEdit;


		private Dictionary<ICON_TYPE, IconUnit> _type2Icons = null;
		private List<IconUnit> _icons = null;
		private int _nIcons = 0;
		
		private Stopwatch _timer = null;
		private const float MAX_TIMEUNIT = 0.1f;//0.1초 (10FPS)보다 오래 걸린 프레임은 0.1로 계산한다.

		//툴팁용 Srting
		private apStringWrapper _str_Tooltip = null;
		private const string TEXT_HOTKEY_1 = " (";
		private const string TEXT_HOTKEY_MID = " / ";
		private const string TEXT_HOTKEY_2 = ")";
		private float _toolTipLength = 0.0f;

		//이전 마지막 툴팁을 기록하자
		private enum TOOLTIP_TYPE
		{
			None,
			LowCPU,
			MeshHidden,
			BoneHidden,
			BoneOutline,
			PhysicsDisabled,
			OnionSkin,
			VisibilityPreset,
			Rotoscoping,
			SingleModifier,
			MultipleModifier,
			PreviewBone,
			PreviewColor,
			PreviewBoneAndColor,
			SelectionLock,
			SelectionUnlock,
			SelectionSemiLock
		}
		private TOOLTIP_TYPE _prevTooltip = TOOLTIP_TYPE.None;
		private Dictionary<TOOLTIP_TYPE, string> _tooltipText = null;



		// Init
		//-----------------------------------------------
		public apGUIStatBox(apEditor editor)
		{
			_editor = editor;
			_prevPortrait = null;

			
			
			_timer = new Stopwatch();
			_timer.Stop();
			_timer.Reset();
			_timer.Start();
			
			InitAndHideAll();
		}

		public void InitAndHideAll()
		{
			_img_Delimeter =		_editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_BG);
			_img_LowCPU =			_editor.ImageSet.Get(apImageSet.PRESET.LowCPU);
			_img_MeshHidden =		_editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_MeshHidden);
			_img_BoneHidden =		_editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_BoneHidden);
			_img_BoneOutline =		_editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_BoneOutline);
			_img_PhysicsDisabled =	_editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_DisablePhysics);
			_img_OnionSkin =		_editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_OnionSkin);
			_img_VisibilityPreset = _editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_PresetVisible);
			_img_Rotoscoping =		_editor.ImageSet.Get(apImageSet.PRESET.GUI_ViewStat_Rotoscoping);
			_img_SingleModifier =	_editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_SingleModifier);
			_img_MultiModifiers =	_editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_MultiModifiers);
			_img_MultiModifiers_Impossible = _editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_MultiModifiers_Impossible);
			_img_PreviewBones =		_editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_PreviewBone);
			_img_PreviewColors =	_editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_PreviewColor);
			_img_PreviewBonesAndColors = _editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_PreviewBoneAndColor);
			_img_SelectionLock =	_editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_SelectionLock);
			_img_SelectionUnlock =	_editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_SelectionUnlock);
			_img_SelectionSemiLock = _editor.ImageSet.Get(apImageSet.PRESET.GUI_EditStat_SemiSelectionLock);

			//조건값들 (각각 다르다)
			_prevViewLowCpu = VALUE_VIEW_LOWCPU.None;
			_prevViewMesh = VALUE_VIEW_MESH.Shown;
			_prevViewBone = VALUE_VIEW_BONE.Shown;
			_prevViewPhysics = VALUE_VIEW_PHYSICS.Enabled;
			_prevViewOnionSkin = VALUE_VIEW_ONION_SKIN.None;
			_prevViewVisibilityPreset = VALUE_VIEW_VISIBILITY_PRESET.None;
			_prevViewRotoscoping = VALUE_VIEW_ROTOSCOPING.None;
			_prevEditModifier = VALUE_EDIT_MODIFIER.NotEdit;
			_prevEditPreview = VALUE_EDIT_PREVIEW.NoPreview;
			_prevEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.NotEdit;

			_curViewLowCpu = VALUE_VIEW_LOWCPU.None;
			_curViewMesh = VALUE_VIEW_MESH.Shown;
			_curViewBone = VALUE_VIEW_BONE.Shown;
			_curViewPhysics = VALUE_VIEW_PHYSICS.Enabled;
			_curViewOnionSkin = VALUE_VIEW_ONION_SKIN.None;
			_curViewVisibilityPreset = VALUE_VIEW_VISIBILITY_PRESET.None;
			_curViewRotoscoping = VALUE_VIEW_ROTOSCOPING.None;
			_curEditModifier = VALUE_EDIT_MODIFIER.NotEdit;
			_curEditPreview = VALUE_EDIT_PREVIEW.NoPreview;
			_curEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.NotEdit;


			if(_type2Icons == null)
			{
				_type2Icons = new Dictionary<ICON_TYPE, IconUnit>();
			}
			if (_icons == null)
			{
				_icons = new List<IconUnit>();
			}
			_type2Icons.Clear();
			_icons.Clear();

			NewIcon(ICON_TYPE.View_LowCPU);
			NewIcon(ICON_TYPE.View_MeshHidden);
			NewIcon(ICON_TYPE.View_BoneHiddenOrOutline);
			NewIcon(ICON_TYPE.View_PhysicsDisabled);
			NewIcon(ICON_TYPE.View_OnionSkin);
			NewIcon(ICON_TYPE.View_VisibilityPreset);
			NewIcon(ICON_TYPE.View_Rotoscoping);
			NewIcon(ICON_TYPE.Edit_SingleMultiModifier);
			NewIcon(ICON_TYPE.Edit_PreviewResult);
			NewIcon(ICON_TYPE.Edit_SelectionLock);

			_nIcons = _icons.Count;


			_str_Tooltip = new apStringWrapper(128);
			_toolTipLength = 0.0f;

			//이전 마지막 툴팁을 기록하자
			//private enum TOOLTIP_TYPE
			//{
			//	None,
			//	LowCPU,
			//	MeshHidden,
			//	BoneHidden,
			//	BoneOutline,
			//	PhysicsDisabled,
			//	OnionSkin,
			//	VisibilityPreset,
			//	Rotoscoping,
			//	SingleModifier,
			//	MultipleModifier,
			//	PreviewBone,
			//	PreviewColor,
			//	PreviewBoneAndColor,
			//	SelectionLock,
			//	SelectionUnlock,
			//	SelectionSemiLock
			//}
			//툴팁 텍스트는 여기서 만든다. (영어로만)
			_prevTooltip = TOOLTIP_TYPE.None;
			_tooltipText = new Dictionary<TOOLTIP_TYPE, string>();
			_tooltipText.Add(TOOLTIP_TYPE.None,	"None");
			_tooltipText.Add(TOOLTIP_TYPE.LowCPU, "Editor performance optimized for laptops (Setting dialog)");
			_tooltipText.Add(TOOLTIP_TYPE.MeshHidden, "Hidden Meshes");
			_tooltipText.Add(TOOLTIP_TYPE.BoneHidden, "Hidden Bones");
			_tooltipText.Add(TOOLTIP_TYPE.BoneOutline, "Outline of Bones");
			_tooltipText.Add(TOOLTIP_TYPE.PhysicsDisabled, "Disabled Physics effect");
			_tooltipText.Add(TOOLTIP_TYPE.OnionSkin, "Onion Skin");
			_tooltipText.Add(TOOLTIP_TYPE.VisibilityPreset, "Visibility Preset");
			_tooltipText.Add(TOOLTIP_TYPE.Rotoscoping, "Rotoscoping");
			_tooltipText.Add(TOOLTIP_TYPE.SingleModifier, "One Modifier works");
			_tooltipText.Add(TOOLTIP_TYPE.MultipleModifier, "Multiple modifiers work if no conflict");
			_tooltipText.Add(TOOLTIP_TYPE.PreviewBone, "Preview the results of Bones");
			_tooltipText.Add(TOOLTIP_TYPE.PreviewColor, "Preview the results of Colors");
			_tooltipText.Add(TOOLTIP_TYPE.PreviewBoneAndColor, "Preview the results of Bones and Colors");
			_tooltipText.Add(TOOLTIP_TYPE.SelectionLock, "Selection Locked");
			_tooltipText.Add(TOOLTIP_TYPE.SelectionUnlock, "Selection Unlocked");
			_tooltipText.Add(TOOLTIP_TYPE.SelectionSemiLock, "Selection Unlocked for only edited objects");
		}


		private void NewIcon(ICON_TYPE iconType)
		{
			IconUnit newUnit = new IconUnit(iconType);
			newUnit.Hide();

			_type2Icons.Add(iconType, newUnit);
			_icons.Add(newUnit);

		}



		// Functions
		//-----------------------------------------------
		public void UpdateAndRender(Vector2 rightPos, Vector2 mousePos)
		{
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

			
			//숨길때를 제외하고는 항상 SetPos를 먼저 하고 ChangeVisible을 한다.

			//일단 리셋 여부 체크
			
			if (_editor._portrait != _prevPortrait)
			{
				//일단 모두 숨기고
				for (int i = 0; i < _nIcons; i++)
				{
					_icons[i].Hide();
				}

				_prevPortrait = _editor._portrait;

				

				//값을 초기화 (아이콘이 안보여질 값으로 지정한다.)
				_prevViewLowCpu = VALUE_VIEW_LOWCPU.None;
				_prevViewMesh = VALUE_VIEW_MESH.Shown;
				_prevViewBone = VALUE_VIEW_BONE.Shown;
				_prevViewPhysics = VALUE_VIEW_PHYSICS.Enabled;
				_prevViewOnionSkin = VALUE_VIEW_ONION_SKIN.None;
				_prevViewVisibilityPreset = VALUE_VIEW_VISIBILITY_PRESET.None;
				_prevViewRotoscoping = VALUE_VIEW_ROTOSCOPING.None;
				_prevEditModifier = VALUE_EDIT_MODIFIER.NotEdit;
				_prevEditPreview = VALUE_EDIT_PREVIEW.NoPreview;
				_prevEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.NotEdit;

				if (_editor._portrait == null)
				{
					//Portrait가 존재하지 않는다면
					return;
				}
			}

			//Portrait가 존재한다면, 값 비교 없이 바로 적용
			//값을 거꾸로 체크
				
			//일단 공통적으로 editMode 체크
			bool isEditMode = false;
			bool isMeshGroupOrAnimClip = false;
			if(_editor.Select != null)
			{
				if(_editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
				{
					isEditMode = _editor.Select.ExEditingMode == apSelection.EX_EDIT.ExOnly_Edit;
					isMeshGroupOrAnimClip = true;
				}
				else if(_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
				{
					isEditMode = _editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.ExOnly_Edit;
					isMeshGroupOrAnimClip = true;
				}
			}

			if (isEditMode)
			{
				if(_editor._exModObjOption_UpdateByOtherMod)
				{
					//편집 모드에서 다른 모디파이어가 동작한다면
					_curEditModifier = VALUE_EDIT_MODIFIER.Multiple;

					//다중 편집이 불가능한 상황도 있다.
					//핀모드에서는 다중 편집이 꺼진다.
					if (_editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Pin)
					{
						//- 모디파이어 / PSG가 선택됨 / 편집 모드 / Morph 모디파이어 / 핀모드가 충족될 때
						if (_editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup
							&& _editor.Select.MeshGroup != null)
						{
							if (_editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier
								&& _editor.Select.Modifier != null
								&& _editor.Select.Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Morph
								&& _editor.Select.SubEditedParamSetGroup != null
								&& _editor.Select.ExEditingMode == apSelection.EX_EDIT.ExOnly_Edit)
							{
								//다중 편집이 불가능하다.
								_curEditModifier = VALUE_EDIT_MODIFIER.MultipleButImpossible;
							}
						}
						else if (_editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
						{
							if(_editor.Select.AnimClip != null
								&& _editor.Select.AnimClip._targetMeshGroup != null
								&& _editor.Select.AnimTimeline != null
								&& _editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.ExOnly_Edit)
							{
								apModifierBase targetModifier = _editor.Select.AnimTimeline._linkedModifier;
								if(targetModifier != null
									&& targetModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
								{
									//다중 편집이 불가능하다.
									_curEditModifier = VALUE_EDIT_MODIFIER.MultipleButImpossible;
								}
							}
						}
					}
					


						
					
				}
				else
				{
					//기본적으로 모디파이어는 하나만 동작한다.
					_curEditModifier = VALUE_EDIT_MODIFIER.Single;
				}

				if(_editor.Select.IsSelectionLockGUI)
				{
					//선택 잠금
					_curEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.Lock;
				}
				else if(_editor._exModObjOption_NotSelectable)
				{
					//선택 잠금은 풀었는데, 편집 불가인 경우엔 선택 불가 (반만 선택)
					_curEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.SemiLock;
				}
				else
				{
					//선택 잠금을 풀었다.
					_curEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.Unlock;
				}
			}
			else
			{
				_curEditModifier = VALUE_EDIT_MODIFIER.NotEdit;
				_curEditSelectionLock = VALUE_EDIT_SELECTIONLOCK.NotEdit;
			}

			if(isEditMode)
			{
				if(_editor._modLockOption_BoneResultPreview && _editor._modLockOption_ColorPreview)
				{
					_curEditPreview = VALUE_EDIT_PREVIEW.PreviewBoneAndColor;
				}
				else if(_editor._modLockOption_BoneResultPreview)
				{
					_curEditPreview = VALUE_EDIT_PREVIEW.PreviewBone;
				}
				else if(_editor._modLockOption_ColorPreview)
				{
					_curEditPreview = VALUE_EDIT_PREVIEW.PreviewColor;
				}
				else
				{
					_curEditPreview = VALUE_EDIT_PREVIEW.NoPreview;
				}
			}
			else
			{
				//다른 메뉴에서는 프리뷰를 볼 수 없다.
				_curEditPreview = VALUE_EDIT_PREVIEW.NoPreview;
			}
			
			

			//View 설정 체크
			_curViewLowCpu = _editor._isLowCPUOption ? VALUE_VIEW_LOWCPU.LowCPU : VALUE_VIEW_LOWCPU.None;
			_curViewMesh = _editor._meshGUIRenderMode == apEditor.MESH_RENDER_MODE.Render ? VALUE_VIEW_MESH.Shown : VALUE_VIEW_MESH.Hidden;
			
			switch (_editor._boneGUIRenderMode)
			{
				case apEditor.BONE_RENDER_MODE.None: _curViewBone = VALUE_VIEW_BONE.Hidden; break;
				case apEditor.BONE_RENDER_MODE.Render: _curViewBone = VALUE_VIEW_BONE.Shown; break;
				case apEditor.BONE_RENDER_MODE.RenderOutline: _curViewBone = VALUE_VIEW_BONE.Outline; break;
			}
			
			_curViewPhysics = (_editor._portrait != null && _editor._portrait._isPhysicsPlay_Editor) ? VALUE_VIEW_PHYSICS.Enabled : VALUE_VIEW_PHYSICS.Disabled;
			_curViewOnionSkin = (_editor.Onion.IsVisible && isMeshGroupOrAnimClip) ? VALUE_VIEW_ONION_SKIN.OnionSkin : VALUE_VIEW_ONION_SKIN.None;			
			_curViewVisibilityPreset = _editor._isAdaptVisibilityPreset ? VALUE_VIEW_VISIBILITY_PRESET.VisbilityPreset : VALUE_VIEW_VISIBILITY_PRESET.None;

			//_curViewRotoscoping = VALUE_VIEW_ROTOSCOPING.Rotoscoping;//일단 True
			_curViewRotoscoping = _editor._isEnableRotoscoping ? VALUE_VIEW_ROTOSCOPING.Rotoscoping : VALUE_VIEW_ROTOSCOPING.None;

			//값이 바뀌면 체크.
			//값에 따라서 어떤게 보일지 확인한다.
			
			//오른쪽부터 거꾸로 체크한다.
			//Vector2 curIconPos = rightPos;
			Vector2 curIconPos = Vector2.zero;
			Vector2 posOffset = new Vector2(-32.0f, 0.0f);
			float scaledIconSize = 28.0f / apGL.Zoom;

			bool isAnyChanged = false;//하나라도 바뀌면, 이후의 것들은 모두 "밀려나듯이" 갱신되어야 한다.

			
			if (_prevViewLowCpu != _curViewLowCpu
				|| _prevViewMesh != _curViewMesh
				|| _prevViewBone != _curViewBone
				|| _prevViewPhysics != _curViewPhysics
				|| _prevViewOnionSkin != _curViewOnionSkin
				|| _prevViewVisibilityPreset != _curViewVisibilityPreset
				|| _prevViewRotoscoping != _curViewRotoscoping
				|| _prevEditModifier != _curEditModifier
				|| _prevEditPreview != _curEditPreview
				|| _prevEditSelectionLock != _curEditSelectionLock)
			{
				isAnyChanged = true;

				_prevViewLowCpu = _curViewLowCpu;
				_prevViewMesh = _curViewMesh;
				_prevViewBone = _curViewBone;
				_prevViewPhysics = _curViewPhysics;
				_prevViewOnionSkin = _curViewOnionSkin;
				_prevViewVisibilityPreset = _curViewVisibilityPreset;
				_prevViewRotoscoping = _curViewRotoscoping;
				_prevEditModifier = _curEditModifier;
				_prevEditPreview = _curEditPreview;
				_prevEditSelectionLock = _curEditSelectionLock;
			}


			//선택 잠금
			if(isAnyChanged)
			{
				switch (_curEditSelectionLock)
				{
					case VALUE_EDIT_SELECTIONLOCK.NotEdit:
						_type2Icons[ICON_TYPE.Edit_SelectionLock].Hide();
						break;

					case VALUE_EDIT_SELECTIONLOCK.Lock:
						_type2Icons[ICON_TYPE.Edit_SelectionLock].Show(_img_SelectionLock, curIconPos);
						curIconPos += posOffset;
						break;

					case VALUE_EDIT_SELECTIONLOCK.Unlock:
						_type2Icons[ICON_TYPE.Edit_SelectionLock].Show(_img_SelectionUnlock, curIconPos);
						curIconPos += posOffset;
						break;

					case VALUE_EDIT_SELECTIONLOCK.SemiLock:
						_type2Icons[ICON_TYPE.Edit_SelectionLock].Show(_img_SelectionSemiLock, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			//모디파이어 동시 실행
			if(isAnyChanged)
			{
				switch (_curEditModifier)
				{
					case VALUE_EDIT_MODIFIER.NotEdit:
						_type2Icons[ICON_TYPE.Edit_SingleMultiModifier].Hide();
						break;

					case VALUE_EDIT_MODIFIER.Single:
						_type2Icons[ICON_TYPE.Edit_SingleMultiModifier].Show(_img_SingleModifier, curIconPos);
						curIconPos += posOffset;
						break;

					case VALUE_EDIT_MODIFIER.Multiple:
						_type2Icons[ICON_TYPE.Edit_SingleMultiModifier].Show(_img_MultiModifiers, curIconPos);
						curIconPos += posOffset;
						break;

					case VALUE_EDIT_MODIFIER.MultipleButImpossible:
						_type2Icons[ICON_TYPE.Edit_SingleMultiModifier].Show(_img_MultiModifiers_Impossible, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			//미리보기
			if(isAnyChanged)
			{
				switch (_curEditPreview)
				{
					case VALUE_EDIT_PREVIEW.NoPreview:
						_type2Icons[ICON_TYPE.Edit_PreviewResult].Hide();
						break;

					case VALUE_EDIT_PREVIEW.PreviewBone:
						_type2Icons[ICON_TYPE.Edit_PreviewResult].Show(_img_PreviewBones, curIconPos);
						curIconPos += posOffset;
						break;

					case VALUE_EDIT_PREVIEW.PreviewColor:
						_type2Icons[ICON_TYPE.Edit_PreviewResult].Show(_img_PreviewColors, curIconPos);
						curIconPos += posOffset;
						break;

					case VALUE_EDIT_PREVIEW.PreviewBoneAndColor:
						_type2Icons[ICON_TYPE.Edit_PreviewResult].Show(_img_PreviewBonesAndColors, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			if(
				//Edit 중에 하나라도 아이콘이 나와야 하며
				(_curEditSelectionLock != VALUE_EDIT_SELECTIONLOCK.NotEdit
				|| _curEditModifier != VALUE_EDIT_MODIFIER.NotEdit
				|| _curEditPreview != VALUE_EDIT_PREVIEW.NoPreview)
				&&
				//View 중에 하나라도 아이콘이 나오는 조건
				(_curViewLowCpu != VALUE_VIEW_LOWCPU.None
				|| _curViewMesh != VALUE_VIEW_MESH.Shown
				|| _curViewBone != VALUE_VIEW_BONE.Shown
				|| _curViewPhysics != VALUE_VIEW_PHYSICS.Enabled
				|| _curViewOnionSkin != VALUE_VIEW_ONION_SKIN.None
				|| _curViewVisibilityPreset != VALUE_VIEW_VISIBILITY_PRESET.None
				|| _curViewRotoscoping != VALUE_VIEW_ROTOSCOPING.None)
				)
			{
				//여기까지 하나라도 출력한게 있다면, 구분자를 넣어야 한다.
				//Edit의 가장 왼쪽것과, View의 가장 오른쪽을 찾자
				Vector2 leftOfEdit = Vector2.zero;				
				Vector2 rightOfView = Vector2.zero;
				

				if(_curEditPreview != VALUE_EDIT_PREVIEW.NoPreview)
				{
					leftOfEdit = _type2Icons[ICON_TYPE.Edit_PreviewResult]._pos;
				}
				else if(_curEditModifier != VALUE_EDIT_MODIFIER.NotEdit)
				{
					leftOfEdit = _type2Icons[ICON_TYPE.Edit_SingleMultiModifier]._pos;
				}
				else//if(_curEditSelectionLock != VALUE_EDIT_SELECTIONLOCK.NotEdit)
				{
					leftOfEdit = _type2Icons[ICON_TYPE.Edit_SelectionLock]._pos;
				}

				rightOfView = leftOfEdit + posOffset * 1.3f;
				Vector2 delimeterPos = (leftOfEdit) * 0.5f + (rightOfView) * 0.5f;
				

				apGL.DrawTextureGL(	_img_Delimeter, delimeterPos + rightPos, 
									scaledIconSize, scaledIconSize, 
									new Color(0.5f, 0.5f, 0.5f, 0.7f), 0.0f);

				curIconPos += posOffset * 0.3f;//반칸 이동한다.
			}

			

			//로토스코핑
			if(isAnyChanged)
			{
				switch (_curViewRotoscoping)
				{
					case VALUE_VIEW_ROTOSCOPING.None:
						_type2Icons[ICON_TYPE.View_Rotoscoping].Hide();
						break;

					case VALUE_VIEW_ROTOSCOPING.Rotoscoping:
						_type2Icons[ICON_TYPE.View_Rotoscoping].Show(_img_Rotoscoping, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			//보이기 프리셋
			if(isAnyChanged)
			{
				switch (_curViewVisibilityPreset)
				{
					case VALUE_VIEW_VISIBILITY_PRESET.None:
						_type2Icons[ICON_TYPE.View_VisibilityPreset].Hide();
						break;

					case VALUE_VIEW_VISIBILITY_PRESET.VisbilityPreset:
						_type2Icons[ICON_TYPE.View_VisibilityPreset].Show(_img_VisibilityPreset, curIconPos);
						curIconPos += posOffset;
						break;
				}	
			}

			//오니온 스킨
			if(isAnyChanged)
			{
				switch (_curViewOnionSkin)
				{
					case VALUE_VIEW_ONION_SKIN.None:
						_type2Icons[ICON_TYPE.View_OnionSkin].Hide();
						break;

					case VALUE_VIEW_ONION_SKIN.OnionSkin:
						_type2Icons[ICON_TYPE.View_OnionSkin].Show(_img_OnionSkin, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			//물리 보기
			if(isAnyChanged)
			{
				switch (_curViewPhysics)
				{
					case VALUE_VIEW_PHYSICS.Enabled:
						_type2Icons[ICON_TYPE.View_PhysicsDisabled].Hide();
						break;

					case VALUE_VIEW_PHYSICS.Disabled:
						_type2Icons[ICON_TYPE.View_PhysicsDisabled].Show(_img_PhysicsDisabled, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			//본 미리보기
			if(isAnyChanged)
			{
				switch (_curViewBone)
				{
					case VALUE_VIEW_BONE.Shown:
						_type2Icons[ICON_TYPE.View_BoneHiddenOrOutline].Hide();
						break;

					case VALUE_VIEW_BONE.Outline:
						_type2Icons[ICON_TYPE.View_BoneHiddenOrOutline].Show(_img_BoneOutline, curIconPos);
						curIconPos += posOffset;
						break;

					case VALUE_VIEW_BONE.Hidden:
						_type2Icons[ICON_TYPE.View_BoneHiddenOrOutline].Show(_img_BoneHidden, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			//메시 미리보기
			if(isAnyChanged)
			{
				switch (_curViewMesh)
				{
					case VALUE_VIEW_MESH.Shown:
						_type2Icons[ICON_TYPE.View_MeshHidden].Hide();
						break;

					case VALUE_VIEW_MESH.Hidden:
						_type2Icons[ICON_TYPE.View_MeshHidden].Show(_img_MeshHidden, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}

			//LowCpu
			if(isAnyChanged)
			{
				switch (_curViewLowCpu)
				{
					case VALUE_VIEW_LOWCPU.None:
						_type2Icons[ICON_TYPE.View_LowCPU].Hide();
						break;

					case VALUE_VIEW_LOWCPU.LowCPU:
						_type2Icons[ICON_TYPE.View_LowCPU].Show(_img_LowCPU, curIconPos);
						curIconPos += posOffset;
						break;
				}
			}


			//업데이트와 렌더링을 하자
			IconUnit curIcon = null;
			
			bool isAnyMouseRollOver = false;
			ICON_TYPE mouseRollOverType = ICON_TYPE.View_LowCPU;

			for (int i = 0; i < _nIcons; i++)
			{
				curIcon = _icons[i];
				curIcon.Update(tDelta);
				if(!curIcon._isVisible)
				{
					continue;
				}
				apGL.DrawTextureGL(	curIcon._img, curIcon._pos + rightPos, 
									scaledIconSize * curIcon._scaleRatio, scaledIconSize * curIcon._scaleRatio, 
									curIcon._color, 0.0f);

				Vector2 deltaPos = mousePos - (curIcon._pos + rightPos);
				float iconHalfSize = scaledIconSize * curIcon._scaleRatio * 0.5f;
				if(-iconHalfSize < deltaPos.x && deltaPos.x < iconHalfSize
					&& -iconHalfSize < deltaPos.y && deltaPos.y < iconHalfSize)
				{
					isAnyMouseRollOver = true;
					mouseRollOverType = curIcon._iconType;
					//UnityEngine.Debug.Log("Icon 선택 : " + mouseRollOverType);
				}
			}

			//Tooltip을 표시해야한다.
			if(isAnyMouseRollOver)
			{
				TOOLTIP_TYPE curToolTipType = TOOLTIP_TYPE.None;
				
				bool isHotkey1 = false;
				apHotKeyMapping.KEY_TYPE hotKeyType1 = apHotKeyMapping.KEY_TYPE.Undo;//일단 임시
				bool isHotkey2 = false;
				apHotKeyMapping.KEY_TYPE hotKeyType2 = apHotKeyMapping.KEY_TYPE.Undo;//일단 임시

				//현재 툴팁이 뭔지 확인하자
				switch (mouseRollOverType)
				{
					case ICON_TYPE.View_LowCPU:
						curToolTipType = TOOLTIP_TYPE.LowCPU;
						break;

					case ICON_TYPE.View_MeshHidden:
						curToolTipType = TOOLTIP_TYPE.MeshHidden;
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.ToggleMeshVisibility;
						break;

					case ICON_TYPE.View_BoneHiddenOrOutline:
						if(_curViewBone == VALUE_VIEW_BONE.Hidden)
						{
							curToolTipType = TOOLTIP_TYPE.BoneHidden;
						}
						else
						{
							curToolTipType = TOOLTIP_TYPE.BoneOutline;
						}
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.ToggleBoneVisibility;
						break;

					case ICON_TYPE.View_PhysicsDisabled:
						curToolTipType = TOOLTIP_TYPE.PhysicsDisabled;
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.TogglePhysicsPreview;
						break;

					case ICON_TYPE.View_OnionSkin:
						curToolTipType = TOOLTIP_TYPE.OnionSkin;
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.ToggleOnionSkin;
						break;

					case ICON_TYPE.View_VisibilityPreset:
						curToolTipType = TOOLTIP_TYPE.VisibilityPreset;
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.TogglePresetVisibility;
						break;

					case ICON_TYPE.View_Rotoscoping:
						curToolTipType = TOOLTIP_TYPE.Rotoscoping;
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.ToggleRotoscoping;
						break;

					case ICON_TYPE.Edit_SingleMultiModifier:
						if(_curEditModifier == VALUE_EDIT_MODIFIER.Single)
						{
							curToolTipType = TOOLTIP_TYPE.SingleModifier;
						}
						else
						{
							curToolTipType = TOOLTIP_TYPE.MultipleModifier;
						}
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.ExObj_UpdateByOtherMod;
						break;
						
					case ICON_TYPE.Edit_PreviewResult:
						if(_curEditPreview == VALUE_EDIT_PREVIEW.PreviewBone)
						{
							curToolTipType = TOOLTIP_TYPE.PreviewBone;
							
						}
						else if(_curEditPreview == VALUE_EDIT_PREVIEW.PreviewColor)
						{
							curToolTipType = TOOLTIP_TYPE.PreviewColor;
						}
						else
						{
							curToolTipType = TOOLTIP_TYPE.PreviewBoneAndColor;
						}
						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.PreviewModBoneResult;
						isHotkey2 = true;
						hotKeyType2 = apHotKeyMapping.KEY_TYPE.PreviewModColorResult;
						break;

					case ICON_TYPE.Edit_SelectionLock:
						if(_curEditSelectionLock == VALUE_EDIT_SELECTIONLOCK.Lock)
						{
							curToolTipType = TOOLTIP_TYPE.SelectionLock;
						}
						else if(_curEditSelectionLock == VALUE_EDIT_SELECTIONLOCK.Unlock)
						{
							curToolTipType = TOOLTIP_TYPE.SelectionUnlock;

							isHotkey2 = true;
							hotKeyType2 = apHotKeyMapping.KEY_TYPE.ExObj_ToggleSelectionSemiLock;
						}
						else
						{
							curToolTipType = TOOLTIP_TYPE.SelectionSemiLock;

							isHotkey2 = true;
							hotKeyType2 = apHotKeyMapping.KEY_TYPE.ExObj_ToggleSelectionSemiLock;
						}

						isHotkey1 = true;
						hotKeyType1 = apHotKeyMapping.KEY_TYPE.ToggleSelectionLock;
						break;
				}

				if(curToolTipType != _prevTooltip && curToolTipType != TOOLTIP_TYPE.None)
				{
					//텍스트를 만들자
					_str_Tooltip.Clear();
					_str_Tooltip.Append(_tooltipText[curToolTipType], false);//TODO : 단축키 추가

					if(isHotkey1)
					{
						_str_Tooltip.Append(TEXT_HOTKEY_1, false);
						_editor.HotKeyMap.AddHotkeyTextToWrapper(hotKeyType1, _str_Tooltip, false);

						if(isHotkey2)
						{
							_str_Tooltip.Append(TEXT_HOTKEY_MID, false);
							_editor.HotKeyMap.AddHotkeyTextToWrapper(hotKeyType2, _str_Tooltip, false);
						}
						_str_Tooltip.Append(TEXT_HOTKEY_2, false);
					}
					_str_Tooltip.MakeString();
					
					_toolTipLength = apUtil.GetStringRealLength(_str_Tooltip.ToString());
				}

				_prevTooltip = curToolTipType;
				if (curToolTipType != TOOLTIP_TYPE.None)
				{
					//TODO : 적절하게 툴팁을 적어주자. 이미지를 이용하면 현재 상태를 더 세분화해서 알려줄 수 있을것
					apGL.DrawTextGL_IgnoreRightClipping(_str_Tooltip.ToString(), rightPos + new Vector2(-_toolTipLength, +16), _toolTipLength + 20, Color.yellow);


					//apGL.DrawTextGL("ABCDEFGHIJKLMNOPQRSTUVWXYZ", rightPos - new Vector2(500, -30), 500, Color.yellow);
					//apGL.DrawTextGL("abcdefghijklmnopqrstuvwxyz", rightPos - new Vector2(500, -50), 500, Color.yellow);
					//apGL.DrawTextGL("1234567890()[] []/+-!?....,,,,><", rightPos - new Vector2(500, -70), 500, Color.yellow);
				}
			}
			else
			{
				_prevTooltip = TOOLTIP_TYPE.None;
			}
		}


		// Render
		//-----------------------------------------------

		// Get / Set
		//-----------------------------------------------
		/// <summary>
		/// 가속도가 포함된 보간값을 리턴한다.
		/// </summary>
		/// <param name="lerp"></param>
		/// <returns></returns>
		public static float GetAccLerp(float lerp)
		{
			return Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(lerp), 0.7f));
		}

		public static float GetDccLerp(float lerp)
		{
			return Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(lerp), 1.4f));
		}

	}
}