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
using NUnit.Framework.Constraints;

namespace AnyPortrait
{

	public class apImageSet
	{
		// Members
		//---------------------------------------------
		public enum PRESET : int
		{
			ToolBtn_Select,
			ToolBtn_Move,
			ToolBtn_Rotate,
			ToolBtn_Scale,
			ToolBtn_Transform,
			ToolBtn_TransformAdapt,
			ToolBtn_TransformRevert,
			ToolBtn_SoftSelection,
			ToolBtn_Blur,
			ToolBtn_BoneVisible,
			ToolBtn_BoneVisibleOutlineOnly,
			ToolBtn_Bake,
			ToolBtn_Setting,
			ToolBtn_Physic,
			ToolBtn_OnionRecord,
			ToolBtn_OnionView,
			ToolBtn_MeshVisible,
			ToolBtn_TabOpen,
			ToolBtn_TabFolded,
			ToolBtn_MaterialLibrary,

			Gizmo_OriginNone,
			Gizmo_OriginAxis,
			Gizmo_Transform_Move,
			Gizmo_Transform_Rotate,
			Gizmo_Transform_RotateBar,
			Gizmo_Transform_Scale,
			Gizmo_Helper,
			Gizmo_Bone_Origin,
			Gizmo_Bone_Body,

			Gizmo_TFBorder,

			Gizmo_RigCircle,

			Hierarchy_MakeNewPortrait,

			Hierarchy_Root,
			Hierarchy_Image,
			Hierarchy_Mesh,
			Hierarchy_MeshGroup,
			Hierarchy_Face,
			Hierarchy_Animation,
			Hierarchy_Param,
			Hierarchy_Modifier,
			Hierarchy_Bone,
			Hierarchy_Add,
			Hierarchy_AddPSD,
			Hierarchy_FoldDown,
			Hierarchy_FoldRight,
			Hierarchy_Folder,
			Hierarchy_Registered,

			//추가 22.6.10 : 작은 크기
			Hierarchy_Root_16px,
			Hierarchy_Image_16px,
			Hierarchy_Mesh_16px,
			Hierarchy_MeshGroup_16px,
			Hierarchy_Animation_16px,
			Hierarchy_Param_16px,

			Hierarchy_All,
			Hierarchy_None,

			Hierarchy_SortMode,
			Hierarchy_SortMode_RegOrder,
			Hierarchy_SortMode_AlphaNum,
			Hierarchy_SortMode_Custom,

			//수정 : Visible 아이콘의 기존 방식에서 새로운 출력 방식으로 변경
			//Hierarchy_Visible,
			//Hierarchy_NonVisible,
			//Hierarchy_Visible_Mod,
			//Hierarchy_NonVisible_Mod,
			//Hierarchy_Visible_NoKey,
			//Hierarchy_NonVisible_NoKey,
			Hierarchy_Visible_Current,
			Hierarchy_NonVisible_Current,
			Hierarchy_Visible_TmpWork,
			Hierarchy_NonVisible_TmpWork,
			Hierarchy_Visible_ModKey,
			Hierarchy_NonVisible_ModKey,
			Hierarchy_Visible_Default,
			Hierarchy_NonVisible_Default,
			Hierarchy_Visible_Rule,//추가 21.1.27
			Hierarchy_NonVisible_Rule,
			Hierarchy_BypassVisible,//추가 21.2.8

			Hierarchy_NoKey,
			Hierarchy_NoKeyDisabled,
			
			Hierarchy_Clipping,

			Hierarchy_SetClipping,
			Hierarchy_OpenLayout,
			Hierarchy_HideLayout,
			Hierarchy_AddTransform,
			Hierarchy_RemoveTransform,

			Hierarchy_Setting,

			Hierarchy_MultiSelected,

			ControlParam_Palette,


			Transform_Move,
			Transform_Rotate,
			Transform_Scale,
			Transform_Depth,
			Transform_Color,
			Transform_IKController,
			Transform_ExtraOption,

			UI_Zoom,
			GUI_Center,
			GUI_FullScreen,
			GUI_TabFoldLeft_x16,
			GUI_TabFoldRight_x16,
			GUI_TabFoldVShow_x16,
			GUI_TabFoldVHide_x16,

			Modifier_LayerUp,
			Modifier_LayerDown,
			Modifier_Volume,
			Modifier_Morph,
			Modifier_AnimatedMorph,
			Modifier_Rigging,
			Modifier_Physic,
			Modifier_TF,
			Modifier_AnimatedTF,
			Modifier_FFD,
			Modifier_AnimatedFFD,
			Modifier_BoneTF,
			Modifier_AnimBoneTF,
			Modifier_ColorOnly,
			Modifier_AnimatedColorOnly,

			Modifier_Active,
			Modifier_Deactive,

			Modifier_ColorVisibleOption,
			Modifier_ExtraOption,

			Modifier_AddNewMod,
			Modifier_AddToControlParamKey,
			Modifier_AddToPhysics,
			Modifier_AddToRigging,
			Modifier_RemoveFromControlParamKey,
			Modifier_RemoveFromPhysics,
			Modifier_RemoveFromRigging,

			Modifier_RotationByAngles,
			Modifier_RotationByVectors,

			CopyPaste_SingleTarget,
			CopyPaste_MultiTarget,

			Controller_Default,
			Controller_Edit,
			Controller_MakeRecordKey,
			Controller_RemoveRecordKey,
			Controller_ScrollBtn,
			Controller_ScrollBtn_Recorded,
			Controller_SlotDeactive,
			Controller_SlotActive,
			Controller_NewAtlas,
			Controller_Select,
			Controller_AddAndRecordKey,

			Edit_Lock,
			Edit_Unlock,
			Edit_Record,
			Edit_Recording,
			Edit_NoRecord,
			Edit_Vertex,
			Edit_Edge,
			Edit_ExEdit,

			Edit_ModLock,
			Edit_ModUnlock,
			Edit_SelectionLock,
			Edit_SelectionUnlock,

			Edit_ExModOption,

			Edit_Copy,
			Edit_Paste,

			Edit_MouseLeft,
			Edit_MouseMiddle,
			Edit_MouseRight,
			Edit_KeyDelete,
			Edit_KeyCtrl,
			Edit_KeyShift,
			Edit_KeyAlt,

			Edit_MeshGroupDefaultTransform,

			MeshEdit_VertexEdge,
			MeshEdit_VertexOnly,
			MeshEdit_EdgeOnly,
			MeshEdit_Polygon,
			MeshEdit_AutoLink,
			MeshEdit_MakePolygon,

			MeshEdit_MeshEditMenu,
			MeshEdit_ModifyMenu,
			MeshEdit_PivotMenu,

			MeshEdit_MakeTab_Add,
			MeshEdit_MakeTab_Auto,
			MeshEdit_MakeTab_TRS,

			MeshEdit_AutoGen_Scan,
			MeshEdit_AutoGen_Preview,
			MeshEdit_AutoGen_Complete,
			MeshEdit_StepCompleted,
			MeshEdit_StepUncompleted,
			MeshEdit_StepUnused,
			MeshEdit_Map_Quad,
			MeshEdit_Map_TFQuad,
			MeshEdit_Map_Radial,
			MeshEdit_Map_Ring,

			MeshEdit_ValueChange_Up,
			MeshEdit_ValueChange_Down,
			MeshEdit_ValueChange_Left,
			MeshEdit_ValueChange_Right,

			MeshEdit_InnerPointAxisLimited,
			MeshEdit_InnerPointLocked,

			MeshEdit_Align_XCenter,
			MeshEdit_Align_XLeft,
			MeshEdit_Align_XRight,
			MeshEdit_Align_YCenter,
			MeshEdit_Align_YDown,
			MeshEdit_Align_YUp,
			MeshEdit_Distribute_X,
			MeshEdit_Distribute_Y,
			MeshEdit_MirrorAxis_X,
			MeshEdit_MirrorAxis_Y,
			MeshEdit_MirrorCopy_X,
			MeshEdit_MirrorCopy_Y,

			MeshEdit_Area,
			MeshEdit_AreaEditing,
			MeshEdit_QuickMake,
			MeshEdit_MultipleQuickMake,

			MeshEdit_PinMenu,
			MeshEdit_PinSelect,
			MeshEdit_PinAdd,
			MeshEdit_PinLink,
			MeshEdit_PinTest,

			MeshEdit_RemoveVertices,



			TransformControlPoint,
			TransformAutoGenMapperCtrl,

			Anim_Play,
			Anim_Pause,
			Anim_PrevFrame,
			Anim_NextFrame,
			Anim_FirstFrame,
			Anim_LastFrame,
			Anim_Loop,
			Anim_KeyOn,
			Anim_KeyOff,
			Anim_Keyframe,
			Anim_KeyframeDummy,
			Anim_KeySummary,
			Anim_KeySummaryMove,
			Anim_PlayBarHead,
			Anim_TimelineSize1,
			Anim_TimelineSize2,
			Anim_TimelineSize3,

			Anim_TimelineBGStart,
			Anim_TimelineBGEnd,

			Anim_EventMark,
			Anim_OnionMark,
			Anim_OnionRangeStart,
			Anim_OnionRangeEnd,

			Anim_AutoZoom,
			Anim_WithMod,
			Anim_WithControlParam,
			Anim_WithBone,
			Anim_MoveToCurrentFrame,
			Anim_MoveToNextFrame,
			Anim_MoveToPrevFrame,
			Anim_KeyLoopLeft,
			Anim_KeyLoopRight,
			Anim_AddKeyframe,
			Anim_CurrentKeyframe,
			Anim_KeyframeCursor,
			Anim_KeyframeMoveSrc,
			Anim_KeyframeMove,
			Anim_KeyframeCopy,
			Anim_AutoKey,

			Anim_ValueMode,
			Anim_CurveMode,

			Anim_AutoScroll,

			Anim_HideLayer,
			Anim_SortABC,
			Anim_SortDepth,
			Anim_SortRegOrder,

			Anim_Load,
			Anim_Save,

			Anim_AddTimeline,
			Anim_AddAllBonesToLayer,
			Anim_AddAllMeshesToLayer,
			Anim_AddAllControlParamsToLayer,
			Anim_RemoveTimelineLayer,

			Anim_CurvePreset_Acc,
			Anim_CurvePreset_Dec,
			Anim_CurvePreset_Default,
			Anim_CurvePreset_Hard,

			Anim_180Lock,
			Anim_180Unlock,

			AnimEvent_MainIcon,
			AnimEvent_Presets,
			

			Curve_ControlPoint,
			Curve_Linear,
			Curve_Smooth,
			Curve_Stepped,
			Curve_Prev,
			Curve_Next,

			Rig_Add,
			Rig_Select,
			Rig_Link,
			Rig_EditMode,

			Rig_IKDisabled,
			Rig_IKHead,
			Rig_IKChained,
			Rig_IKSingle,

			Rig_SaveLoad,
			Rig_LoadBones,
			Rig_LoadBonesMirror,

			Rig_HierarchyIcon_IKHead,
			Rig_HierarchyIcon_IKChained,
			Rig_HierarchyIcon_IKSingle,

			Rig_EditBinding,
			Rig_AutoNormalize,
			Rig_Auto,
			Rig_Blend,
			Rig_Normalize,
			Rig_Prune,
			Rig_AddWeight,
			Rig_MultiplyWeight,
			Rig_SubtractWeight,
			Rig_TestPosing,
			Rig_WeightColorOnly,
			Rig_WeightColorWithTexture,
			Rig_Grow,
			Rig_Shrink,

			Rig_BrushAdd,
			Rig_BrushMultiply,
			Rig_BrushBlur,
			Rig_Lock16px,
			Rig_Unlock16px,
			Rig_WeightMode16px,
			Rig_PaintMode16px,
			Rig_BoneColor,
			Rig_NoBoneColor,
			Rig_SquareColorVert,
			Rig_CircleVert,
			
			Rig_ShowAllBones,
			Rig_TransculentBones,
			Rig_HideBones,

			Rig_Jiggle,
			

			Physic_Stretch,
			Physic_Bend,
			Physic_Gravity,
			Physic_Wind,
			Physic_SetMainVertex,
			Physic_VertConst,
			Physic_VertMain,
			Physic_BasicSetting,
			Physic_Inertia,
			Physic_Recover,
			Physic_Viscosity,
			Physic_Palette,

			Physic_PresetCloth1,
			Physic_PresetCloth2,
			Physic_PresetCloth3,
			Physic_PresetFlag,
			Physic_PresetHair,
			Physic_PresetRibbon,
			Physic_PresetRubberHard,
			Physic_PresetRubberSoft,
			Physic_PresetCustom1,
			Physic_PresetCustom2,
			Physic_PresetCustom3,

			ParamPreset_Body,
			ParamPreset_Cloth,
			ParamPreset_Equip,
			ParamPreset_Etc,
			ParamPreset_Eye,
			ParamPreset_Face,
			ParamPreset_Force,
			ParamPreset_Hair,
			ParamPreset_Hand,
			ParamPreset_Head,

			Demo_Logo,

			AutoSave_Frame1,
			AutoSave_Frame2,

			GUI_SelectionLock,

			StartPageLogo_Full,
			StartPageLogo_Demo,
			StartPage_GettingStarted,
			StartPage_VideoTutorial,
			StartPage_Manual,
			StartPage_Forum,

			SmallMod_AnimMorph,
			SmallMod_Morph,
			SmallMod_AnimTF,
			SmallMod_TF,
			SmallMod_Physics,
			SmallMod_Rigging,
			SmallMod_ControlLayer,
			SmallMod_ExEnabled,
			SmallMod_ExSubEnabled,
			SmallMod_ExDisabled,
			SmallMod_ColorEnabled,
			SmallMod_ColorDisabled,
			
			SmallMod_CursorLocked,
			SmallMod_CursorUnlocked,

			SmallMod_ColorOnly,
			SmallMod_AnimColorOnly,

			Capture_Frame,
			Capture_Tab,
			Capture_Thumbnail,
			Capture_Image,
			Capture_GIF,
			Capture_Sprite,
			Capture_ExportThumb,
			Capture_ExportScreenshot,
			Capture_ExportGIF,
			Capture_ExportSprite,
			Capture_ExportSequence,
			Capture_ExportMP4,

			OnionSkin_SingleFrame,
			OnionSkin_MultipleFrame,

			PSD_Set,
			PSD_BakeEnabled,
			PSD_BakeDisabled,
			PSD_LinkView,
			PSD_Overlay,
			PSD_Switch,
			PSD_SetOutline,
			PSD_MeshOutline,
			PSD_SetSecondary,
			PSD_SetSecondaryOutline,
			PSD_LinkedMain,
			PSD_LinkedMainOutline,
			PSD_TextureSampling,

			ExtraOption_DepthCursor,
			ExtraOption_DepthMidCursor,
			ExtraOption_UnsyncImage,
			ExtraOption_MovedMesh,
			ExtraOption_MovedMeshGroup,
			ExtraOption_NonImage,

			LowCPU,

			GUI_Button_Menu,
			GUI_Button_Menu_Roll,
			GUI_Button_RecordOnion,
			GUI_Button_RecordOnion_Roll,
			
			GUI_Button_EditVert_Disabled,
			GUI_Button_EditVert_RollOver,
			GUI_Button_EditVert_Enabled,
			GUI_Button_EditVert_EnabledRollOver,
			GUI_Button_EditPin_Disabled,
			GUI_Button_EditPin_RollOver,
			GUI_Button_EditPin_Enabled,
			GUI_Button_EditPin_EnabledRollOver,

			GUI_ViewStat_BoneHidden,
			GUI_ViewStat_BoneOutline,
			GUI_ViewStat_DisablePhysics,
			GUI_ViewStat_MeshHidden,
			GUI_ViewStat_OnionSkin,
			GUI_ViewStat_PresetVisible,
			GUI_ViewStat_Rotoscoping,

			GUI_ViewStat_BG,
			GUI_EditStat_SingleModifier,
			GUI_EditStat_MultiModifiers,
			GUI_EditStat_MultiModifiers_Impossible,
			GUI_EditStat_PreviewBone,
			GUI_EditStat_PreviewBoneAndColor,
			GUI_EditStat_PreviewColor,
			GUI_EditStat_SelectionLock,
			GUI_EditStat_SelectionUnlock,
			GUI_EditStat_SemiSelectionLock,


			MaterialSet,
			MaterialSetIcon_Unlit,
			MaterialSetIcon_Lit,
			MaterialSetIcon_LitSpecular,
			MaterialSetIcon_LitSpecularEmission,
			MaterialSetIcon_LitRim,
			MaterialSetIcon_LitRamp,
			MaterialSetIcon_FX,
			MaterialSetIcon_Cartoon,
			MaterialSetIcon_Custom1,
			MaterialSetIcon_Custom2,
			MaterialSetIcon_Custom3,
			MaterialSetIcon_UnlitVR,
			MaterialSetIcon_LitVR,
			MaterialSetIcon_MergeableUnlit,
			MaterialSetIcon_MergeableLit,
			MaterialSet_BasicSettings,
			MaterialSet_ShaderProperties,
			MaterialSet_Shaders,
			MaterialSet_Reserved,
			MaterialSet_CustomShader,
			MaterialSet_LWRP,
			MaterialSet_VR,
			MaterialSet_URP,
			MaterialSet_Mergeable,

			RestoreTmpVisibility_ON,
			RestoreTmpVisibility_OFF,

			RecommendedIcon,

			BoneSpriteSheet,
			PinVertGUIAtlas,
			PinOption_Range,
			PinOption_Tangent_Smooth,
			PinOption_Tangent_Sharp,
			PinCalculateWeight,
			PinCalculateWeightAuto,
			PinRestTestPos,

			BakeBtn_Normal,
			BakeBtn_Optimized,

			SetValueProp_Color,
			SetValueProp_Extra,
			SetValueProp_Pin,
			SetValueProp_Transform,
			SetValueProp_Vert,
			SetValueProp_Visibility,

			SyncSettingToFile16px,
			SyncSettingToFile12px,

			
		}

		private Dictionary<PRESET, Texture2D> _images = new Dictionary<PRESET, Texture2D>();
		
		//개선 이미지 가져오기 속도 빠르게
		private Texture2D[] _index2Image = null;
		private int _nImages = -1;
		private Texture2D _cal_Result = null;
		
		private bool _isAllLoaded = false;

		public enum ReloadResult
		{
			NewLoaded,
			AlreadyLoaded,
			Error,
		}

		//Pro/Personal 스킨이 바뀐다면 리셋할 필요가 있다.
		private bool _isProSkin = false;

		

		// Init
		//---------------------------------------------------------
		public apImageSet()
		{
			_isAllLoaded = false;

			if(_images == null) { _images = new Dictionary<PRESET, Texture2D>(); }
			_images.Clear();

			_index2Image = null;
			_nImages = -1;

			_isProSkin = EditorGUIUtility.isProSkin;
		}


		public ReloadResult ReloadImages()
		{
			//만약 ProSkin 여부가 변경되었다면 리셋
			if (_isAllLoaded)
			{
				if (_isProSkin != EditorGUIUtility.isProSkin)
				{
					//Debug.LogError("스킨이 바뀌었다.");
					_isProSkin = EditorGUIUtility.isProSkin;

					_isAllLoaded = false;
					
					if(_images == null) { _images = new Dictionary<PRESET, Texture2D>(); }
					_images.Clear();

					_index2Image = null;
					_nImages = -1;
				}
			}
			

			if (_isAllLoaded)
			{
				
				//return false;
				//이미 로딩이 끝난 상태
				return ReloadResult.AlreadyLoaded;
			}

			_isAllLoaded = true;

			//이미지를 로드할 준비
			if(_images == null) { _images = new Dictionary<PRESET, Texture2D>(); }
			_images.Clear();

			_index2Image = null;

			//배열을 만들자
			_nImages = apEditorUtil.GetEnumCount(typeof(PRESET));
			if (_nImages > 0)
			{
				_index2Image = new Texture2D[_nImages];
				//Null로 일단 초기화
				for (int i = 0; i < _nImages; i++)
				{
					_index2Image[i] = null;
				}
			}
			
			
			CheckImageAndLoad(PRESET.ToolBtn_Select, "ButtonIcon_Select", true);
			CheckImageAndLoad(PRESET.ToolBtn_Move, "ButtonIcon_Move", true);
			CheckImageAndLoad(PRESET.ToolBtn_Rotate, "ButtonIcon_Rotate", true);
			CheckImageAndLoad(PRESET.ToolBtn_Scale, "ButtonIcon_Scale", true);
			CheckImageAndLoad(PRESET.ToolBtn_Transform, "ButtonIcon_Transform", true);

			CheckImageAndLoad(PRESET.ToolBtn_TransformAdapt, "ButtonIcon_TransformAdapt");
			CheckImageAndLoad(PRESET.ToolBtn_TransformRevert, "ButtonIcon_TransformRevert");

			CheckImageAndLoad(PRESET.ToolBtn_SoftSelection, "ButtonIcon_SoftSelection", true);
			CheckImageAndLoad(PRESET.ToolBtn_Blur, "ButtonIcon_Blur", true);
			CheckImageAndLoad(PRESET.ToolBtn_BoneVisible, "ButtonIcon_BoneVisible", true);
			CheckImageAndLoad(PRESET.ToolBtn_BoneVisibleOutlineOnly, "ButtonIcon_BoneVisibleOutlineOnly", true);


			CheckImageAndLoad(PRESET.ToolBtn_Bake, "ButtonIcon_Bake", true);
			CheckImageAndLoad(PRESET.ToolBtn_Setting, "ButtonIcon_Setting", true);
			CheckImageAndLoad(PRESET.ToolBtn_Physic, "ButtonIcon_Physics", true);
			CheckImageAndLoad(PRESET.ToolBtn_OnionRecord, "ButtonIcon_OnionRecord", true);
			CheckImageAndLoad(PRESET.ToolBtn_OnionView, "ButtonIcon_OnionView", true);
			CheckImageAndLoad(PRESET.ToolBtn_MeshVisible, "ButtonIcon_Mesh", true);
			CheckImageAndLoad(PRESET.ToolBtn_TabOpen,	"ButtonIcon_TabOpen", true);
			CheckImageAndLoad(PRESET.ToolBtn_TabFolded,	"ButtonIcon_TabFolded", true);
			CheckImageAndLoad(PRESET.ToolBtn_MaterialLibrary,	"ButtonIcon_MaterialLibrary", true);
			

			CheckImageAndLoad(PRESET.Gizmo_OriginNone, "Gizmo_Origin_None");
			CheckImageAndLoad(PRESET.Gizmo_OriginAxis, "Gizmo_Origin_Axis");
			CheckImageAndLoad(PRESET.Gizmo_Transform_Move, "Gizmo_Transform_Move");
			CheckImageAndLoad(PRESET.Gizmo_Transform_Rotate, "Gizmo_Transform_Rotate");
			CheckImageAndLoad(PRESET.Gizmo_Transform_RotateBar, "Gizmo_Transform_RotateBar");
			CheckImageAndLoad(PRESET.Gizmo_Transform_Scale, "Gizmo_Transform_Scale");
			CheckImageAndLoad(PRESET.Gizmo_Helper, "Gizmo_Helper");
			CheckImageAndLoad(PRESET.Gizmo_Bone_Origin, "Gizmo_Bone_Origin");
			CheckImageAndLoad(PRESET.Gizmo_Bone_Body, "Gizmo_Bone_Body");
			CheckImageAndLoad(PRESET.Gizmo_TFBorder, "Gizmo_TFBorder");
			CheckImageAndLoad(PRESET.Gizmo_RigCircle, "Gizmo_RigCircle");

			CheckImageAndLoad(PRESET.Hierarchy_MakeNewPortrait, "HierarchyIcon_MakeNewPortrait");

			CheckImageAndLoad(PRESET.Hierarchy_Root, "HierarchyIcon_Root");
			CheckImageAndLoad(PRESET.Hierarchy_Image, "HierarchyIcon_Image");
			CheckImageAndLoad(PRESET.Hierarchy_Mesh, "HierarchyIcon_Mesh");
			CheckImageAndLoad(PRESET.Hierarchy_MeshGroup, "HierarchyIcon_MeshGroup");
			CheckImageAndLoad(PRESET.Hierarchy_Face, "HierarchyIcon_Face");
			CheckImageAndLoad(PRESET.Hierarchy_Animation, "HierarchyIcon_Animation");
			CheckImageAndLoad(PRESET.Hierarchy_Param, "HierarchyIcon_Param");
			CheckImageAndLoad(PRESET.Hierarchy_Add, "HierarchyIcon_Add", true);
			CheckImageAndLoad(PRESET.Hierarchy_AddPSD, "HierarchyIcon_AddPSD", true);
			CheckImageAndLoad(PRESET.Hierarchy_FoldDown, "HierarchyIcon_FoldDown", true);
			CheckImageAndLoad(PRESET.Hierarchy_FoldRight, "HierarchyIcon_FoldRight", true);
			CheckImageAndLoad(PRESET.Hierarchy_Folder, "HierarchyIcon_Folder", true);
			CheckImageAndLoad(PRESET.Hierarchy_Registered, "HierarchyIcon_Registered");

			CheckImageAndLoad(PRESET.Hierarchy_Root_16px,		"HierarchyIcon_Root_16px");
			CheckImageAndLoad(PRESET.Hierarchy_Image_16px,		"HierarchyIcon_Image_16px");
			CheckImageAndLoad(PRESET.Hierarchy_Mesh_16px,		"HierarchyIcon_Mesh_16px");
			CheckImageAndLoad(PRESET.Hierarchy_MeshGroup_16px,	"HierarchyIcon_MeshGroup_16px");
			CheckImageAndLoad(PRESET.Hierarchy_Animation_16px,	"HierarchyIcon_Animation_16px");
			CheckImageAndLoad(PRESET.Hierarchy_Param_16px,		"HierarchyIcon_Param_16px");

			CheckImageAndLoad(PRESET.Hierarchy_Modifier, "HierarchyIcon_Modifier");
			CheckImageAndLoad(PRESET.Hierarchy_Bone, "HierarchyIcon_Bone");

			CheckImageAndLoad(PRESET.Hierarchy_All, "HierarchyIcon_All", true);
			CheckImageAndLoad(PRESET.Hierarchy_None, "HierarchyIcon_None", true);

			CheckImageAndLoad(PRESET.Hierarchy_SortMode, "HierarchyIcon_SortMode", true);
			CheckImageAndLoad(PRESET.Hierarchy_SortMode_RegOrder, "HierarchyIcon_SortMode_RegOrder");
			CheckImageAndLoad(PRESET.Hierarchy_SortMode_AlphaNum, "HierarchyIcon_SortMode_AlphaNum");
			CheckImageAndLoad(PRESET.Hierarchy_SortMode_Custom, "HierarchyIcon_SortMode_Custom");
			

			
			CheckImageAndLoad(PRESET.Hierarchy_Visible_Current, "HierarchyIcon_Visible_Current", true);
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_Current, "HierarchyIcon_NonVisible_Current", true);
			CheckImageAndLoad(PRESET.Hierarchy_Visible_TmpWork, "HierarchyIcon_Visible_TmpWork");
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_TmpWork, "HierarchyIcon_NonVisible_TmpWork");
			CheckImageAndLoad(PRESET.Hierarchy_Visible_ModKey, "HierarchyIcon_Visible_ModKey");
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_ModKey, "HierarchyIcon_NonVisible_ModKey");
			CheckImageAndLoad(PRESET.Hierarchy_Visible_Default, "HierarchyIcon_Visible_Default");
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_Default, "HierarchyIcon_NonVisible_Default");
			CheckImageAndLoad(PRESET.Hierarchy_Visible_Rule, "HierarchyIcon_Visible_ByRule");
			CheckImageAndLoad(PRESET.Hierarchy_NonVisible_Rule, "HierarchyIcon_NonVisible_ByRule");
			CheckImageAndLoad(PRESET.Hierarchy_BypassVisible, "HierarchyIcon_BypassVisible");
			
			CheckImageAndLoad(PRESET.Hierarchy_NoKey, "HierarchyIcon_NoKey");
			CheckImageAndLoad(PRESET.Hierarchy_NoKeyDisabled, "HierarchyIcon_NoKeyDisabled");


			CheckImageAndLoad(PRESET.Hierarchy_Clipping, "HierarchyIcon_Clipping", true);


			CheckImageAndLoad(PRESET.Hierarchy_SetClipping, "HierarchyIcon_SetClipping", true);
			CheckImageAndLoad(PRESET.Hierarchy_OpenLayout, "HierarchyIcon_OpenLayout", true);
			CheckImageAndLoad(PRESET.Hierarchy_HideLayout, "HierarchyIcon_HideLayout", true);
			CheckImageAndLoad(PRESET.Hierarchy_AddTransform, "HierarchyIcon_AddTransform", true);
			CheckImageAndLoad(PRESET.Hierarchy_RemoveTransform, "HierarchyIcon_RemoveTransform", true);

			CheckImageAndLoad(PRESET.Hierarchy_Setting, "HierarchyIcon_Setting");
			CheckImageAndLoad(PRESET.Hierarchy_MultiSelected, "HierarchyIcon_MultiSelected");

			CheckImageAndLoad(PRESET.ControlParam_Palette, "ControlParam_Palette");


			CheckImageAndLoad(PRESET.Transform_Move, "TransformIcon_Move", true);
			CheckImageAndLoad(PRESET.Transform_Rotate, "TransformIcon_Rotate", true);
			CheckImageAndLoad(PRESET.Transform_Scale, "TransformIcon_Scale", true);
			CheckImageAndLoad(PRESET.Transform_Depth, "TransformIcon_Depth", true);
			CheckImageAndLoad(PRESET.Transform_Color, "TransformIcon_Color", true);
			CheckImageAndLoad(PRESET.Transform_IKController, "TransformIcon_IKController", true);
			CheckImageAndLoad(PRESET.Transform_ExtraOption, "TransformIcon_ExtraOption", true);
			

			CheckImageAndLoad(PRESET.UI_Zoom,			"TransformIcon_Zoom", true);
			CheckImageAndLoad(PRESET.GUI_Center,		"GUI_Center", true);
			CheckImageAndLoad(PRESET.GUI_FullScreen,	"GUI_FullScreen", true);
			CheckImageAndLoad(PRESET.GUI_TabFoldLeft_x16,	"GUI_TabFoldLeft_x16");
			CheckImageAndLoad(PRESET.GUI_TabFoldRight_x16,	"GUI_TabFoldRight_x16");
			CheckImageAndLoad(PRESET.GUI_TabFoldVShow_x16,	"GUI_TabFoldVShow_x16");
			CheckImageAndLoad(PRESET.GUI_TabFoldVHide_x16,	"GUI_TabFoldVHide_x16");

			CheckImageAndLoad(PRESET.Modifier_LayerUp, "Modifier_LayerUp", true);
			CheckImageAndLoad(PRESET.Modifier_LayerDown, "Modifier_LayerDown", true);
			CheckImageAndLoad(PRESET.Modifier_Volume, "Modifier_Volume");
			CheckImageAndLoad(PRESET.Modifier_Morph, "Modifier_Morph");
			CheckImageAndLoad(PRESET.Modifier_AnimatedMorph, "Modifier_AnimatedMorph");
			CheckImageAndLoad(PRESET.Modifier_Rigging, "Modifier_Rigging");
			CheckImageAndLoad(PRESET.Modifier_Physic, "Modifier_Physic");

			CheckImageAndLoad(PRESET.Modifier_TF, "Modifier_TF");
			CheckImageAndLoad(PRESET.Modifier_AnimatedTF, "Modifier_AnimatedTF");
			CheckImageAndLoad(PRESET.Modifier_FFD, "Modifier_FFD", true);
			CheckImageAndLoad(PRESET.Modifier_AnimatedFFD, "Modifier_AnimatedFFD");

			CheckImageAndLoad(PRESET.Modifier_BoneTF, "Modifier_BoneTF", true);
			CheckImageAndLoad(PRESET.Modifier_AnimBoneTF, "Modifier_AnimBoneTF", true);

			CheckImageAndLoad(PRESET.Modifier_ColorOnly,			"Modifier_ColorOnly");
			CheckImageAndLoad(PRESET.Modifier_AnimatedColorOnly,	"Modifier_AnimatedColorOnly");


			CheckImageAndLoad(PRESET.Modifier_Active, "Modifier_Active");
			CheckImageAndLoad(PRESET.Modifier_Deactive, "Modifier_Deactive");

			CheckImageAndLoad(PRESET.Modifier_ColorVisibleOption, "Modifier_ColorVisibleOption", true);
			CheckImageAndLoad(PRESET.Modifier_ExtraOption, "Modifier_ExtraOption", true);

			CheckImageAndLoad(PRESET.Modifier_AddNewMod, "Modifier_AddNewMod", true);
			CheckImageAndLoad(PRESET.Modifier_AddToControlParamKey, "Modifier_AddToControlParamKey", true);
			CheckImageAndLoad(PRESET.Modifier_AddToPhysics, "Modifier_AddToPhysics", true);
			CheckImageAndLoad(PRESET.Modifier_AddToRigging, "Modifier_AddToRigging", true);
			CheckImageAndLoad(PRESET.Modifier_RemoveFromControlParamKey, "Modifier_RemoveFromControlParamKey", true);
			CheckImageAndLoad(PRESET.Modifier_RemoveFromPhysics, "Modifier_RemoveFromPhysics", true);
			CheckImageAndLoad(PRESET.Modifier_RemoveFromRigging, "Modifier_RemoveFromRigging", true);

			CheckImageAndLoad(PRESET.Modifier_RotationByAngles, "Modifier_RotationByAngles");
			CheckImageAndLoad(PRESET.Modifier_RotationByVectors, "Modifier_RotationByVectors");

			CheckImageAndLoad(PRESET.CopyPaste_SingleTarget, "CopyPaste_SingleTarget");
			CheckImageAndLoad(PRESET.CopyPaste_MultiTarget, "CopyPaste_MultiTarget");

			CheckImageAndLoad(PRESET.Controller_Default, "Controller_Default", true);
			CheckImageAndLoad(PRESET.Controller_Edit, "Controller_Edit", true);
			CheckImageAndLoad(PRESET.Controller_MakeRecordKey, "Controller_MakeRecordKey");
			CheckImageAndLoad(PRESET.Controller_RemoveRecordKey, "Controller_RemoveRecordKey", true);//Pro 버전으로 변경 [v1.4.1]
			CheckImageAndLoad(PRESET.Controller_ScrollBtn, "Controller_ScrollBtn");
			CheckImageAndLoad(PRESET.Controller_ScrollBtn_Recorded, "Controller_ScrollBtn_Recorded");
			CheckImageAndLoad(PRESET.Controller_SlotDeactive, "Controller_Slot_Deactive");
			CheckImageAndLoad(PRESET.Controller_SlotActive, "Controller_Slot_Active");
			CheckImageAndLoad(PRESET.Controller_NewAtlas, "Controller_NewAtlas");
			CheckImageAndLoad(PRESET.Controller_Select,				"Controller_Select", true);
			CheckImageAndLoad(PRESET.Controller_AddAndRecordKey,	"Controller_AddAndRecordKey");

			CheckImageAndLoad(PRESET.Edit_Lock, "Edit_Lock");
			CheckImageAndLoad(PRESET.Edit_Unlock, "Edit_Unlock");
			CheckImageAndLoad(PRESET.Edit_Record, "Edit_Record", true);
			CheckImageAndLoad(PRESET.Edit_NoRecord, "Edit_NoRecord", true);
			CheckImageAndLoad(PRESET.Edit_Recording, "Edit_Recording", true);
			CheckImageAndLoad(PRESET.Edit_Vertex, "Edit_Vertex", true);
			CheckImageAndLoad(PRESET.Edit_Edge, "Edit_Edge", true);
			CheckImageAndLoad(PRESET.Edit_ExEdit, "Edit_ExEdit");

			CheckImageAndLoad(PRESET.Edit_ModLock, "Edit_ModLock");
			CheckImageAndLoad(PRESET.Edit_ModUnlock, "Edit_ModUnlock");
			CheckImageAndLoad(PRESET.Edit_SelectionLock, "Edit_SelectionLock");
			CheckImageAndLoad(PRESET.Edit_SelectionUnlock, "Edit_SelectionUnlock");

			CheckImageAndLoad(PRESET.Edit_ExModOption, "Edit_ExModOption");
			

			CheckImageAndLoad(PRESET.Edit_Copy, "Edit_Copy", true);
			CheckImageAndLoad(PRESET.Edit_Paste, "Edit_Paste", true);


			CheckImageAndLoad(PRESET.Edit_MouseLeft,	"Edit_MouseLeft");
			CheckImageAndLoad(PRESET.Edit_MouseMiddle,	"Edit_MouseMiddle");
			CheckImageAndLoad(PRESET.Edit_MouseRight,	"Edit_MouseRight");
			CheckImageAndLoad(PRESET.Edit_KeyDelete,	"Edit_KeyDelete");
			
#if UNITY_EDITOR_OSX
			CheckImageAndLoad(PRESET.Edit_KeyCtrl,		"Edit_KeyCommand");//Mac에서는 Ctrl대신 Command 단축키를 사용한다.
			CheckImageAndLoad(PRESET.Edit_KeyAlt,		"Edit_KeyOption");//Mac에서는 Alt대신 Option 단축키를 사용한다.
#else
			CheckImageAndLoad(PRESET.Edit_KeyCtrl,		"Edit_KeyCtrl");
			CheckImageAndLoad(PRESET.Edit_KeyAlt,		"Edit_KeyAlt");
#endif
			CheckImageAndLoad(PRESET.Edit_KeyShift,		"Edit_KeyShift");


			CheckImageAndLoad(PRESET.Edit_MeshGroupDefaultTransform, "Edit_MeshGroupDefaultTransform", true);

			CheckImageAndLoad(PRESET.MeshEdit_VertexEdge, "MeshEdit_VertexEdge");
			CheckImageAndLoad(PRESET.MeshEdit_VertexOnly, "MeshEdit_VertexOnly");
			CheckImageAndLoad(PRESET.MeshEdit_EdgeOnly, "MeshEdit_EdgeOnly");
			CheckImageAndLoad(PRESET.MeshEdit_Polygon, "MeshEdit_Polygon");
			CheckImageAndLoad(PRESET.MeshEdit_AutoLink, "MeshEdit_AutoLink");
			CheckImageAndLoad(PRESET.MeshEdit_MakePolygon, "MeshEdit_MakePolygon");

			CheckImageAndLoad(PRESET.MeshEdit_MeshEditMenu, "MeshEdit_MeshEditMenu");
			CheckImageAndLoad(PRESET.MeshEdit_ModifyMenu, "MeshEdit_ModifyMenu");
			CheckImageAndLoad(PRESET.MeshEdit_PivotMenu, "MeshEdit_PivotMenu");

			CheckImageAndLoad(PRESET.MeshEdit_MakeTab_Add, "MeshEdit_MakeTab_Add");
			CheckImageAndLoad(PRESET.MeshEdit_MakeTab_Auto, "MeshEdit_MakeTab_Auto");
			CheckImageAndLoad(PRESET.MeshEdit_MakeTab_TRS, "MeshEdit_MakeTab_TRS");

			CheckImageAndLoad(PRESET.MeshEdit_AutoGen_Scan, "MeshEdit_AutoGen_Scan");
			CheckImageAndLoad(PRESET.MeshEdit_AutoGen_Preview, "MeshEdit_AutoGen_Preview");
			CheckImageAndLoad(PRESET.MeshEdit_AutoGen_Complete, "MeshEdit_AutoGen_Complete");
			CheckImageAndLoad(PRESET.MeshEdit_StepCompleted, "MeshEdit_StepCompleted");
			CheckImageAndLoad(PRESET.MeshEdit_StepUncompleted, "MeshEdit_StepUncompleted");
			CheckImageAndLoad(PRESET.MeshEdit_StepUnused, "MeshEdit_StepUnUsed");
			CheckImageAndLoad(PRESET.MeshEdit_Map_Quad, "MeshEdit_Map_Quad");
			CheckImageAndLoad(PRESET.MeshEdit_Map_TFQuad, "MeshEdit_Map_TFQuad");
			CheckImageAndLoad(PRESET.MeshEdit_Map_Radial, "MeshEdit_Map_Radial");
			CheckImageAndLoad(PRESET.MeshEdit_Map_Ring, "MeshEdit_Map_Ring");

			CheckImageAndLoad(PRESET.MeshEdit_ValueChange_Up,	"MeshEdit_ValueChange_Up", true);
			CheckImageAndLoad(PRESET.MeshEdit_ValueChange_Down,	"MeshEdit_ValueChange_Down", true);
			CheckImageAndLoad(PRESET.MeshEdit_ValueChange_Left,	"MeshEdit_ValueChange_Left", true);
			CheckImageAndLoad(PRESET.MeshEdit_ValueChange_Right,	"MeshEdit_ValueChange_Right", true);

			CheckImageAndLoad(PRESET.MeshEdit_InnerPointAxisLimited,	"MeshEdit_InnerPointAxisLimited");
			CheckImageAndLoad(PRESET.MeshEdit_InnerPointLocked,			"MeshEdit_InnerPointLocked");

			CheckImageAndLoad(PRESET.MeshEdit_Align_XCenter,	"MeshEdit_Align_XCenter");
			CheckImageAndLoad(PRESET.MeshEdit_Align_XLeft,		"MeshEdit_Align_XLeft");
			CheckImageAndLoad(PRESET.MeshEdit_Align_XRight,		"MeshEdit_Align_XRight");
			CheckImageAndLoad(PRESET.MeshEdit_Align_YCenter,	"MeshEdit_Align_YCenter");
			CheckImageAndLoad(PRESET.MeshEdit_Align_YDown,		"MeshEdit_Align_YDown");
			CheckImageAndLoad(PRESET.MeshEdit_Align_YUp,		"MeshEdit_Align_YUp");
			CheckImageAndLoad(PRESET.MeshEdit_Distribute_X,		"MeshEdit_Distribute_X");
			CheckImageAndLoad(PRESET.MeshEdit_Distribute_Y,		"MeshEdit_Distribute_Y");
			CheckImageAndLoad(PRESET.MeshEdit_MirrorAxis_X,		"MeshEdit_MirrorAxis_X");
			CheckImageAndLoad(PRESET.MeshEdit_MirrorAxis_Y,		"MeshEdit_MirrorAxis_Y");
			CheckImageAndLoad(PRESET.MeshEdit_MirrorCopy_X,		"MeshEdit_MirrorCopy_X");
			CheckImageAndLoad(PRESET.MeshEdit_MirrorCopy_Y,		"MeshEdit_MirrorCopy_Y");

			CheckImageAndLoad(PRESET.MeshEdit_Area,				"MeshEdit_Area");
			CheckImageAndLoad(PRESET.MeshEdit_AreaEditing,		"MeshEdit_AreaEditing");
			CheckImageAndLoad(PRESET.MeshEdit_QuickMake,		"MeshEdit_QuickMake");
			CheckImageAndLoad(PRESET.MeshEdit_MultipleQuickMake, "MeshEdit_MultipleQuickMake");

			CheckImageAndLoad(PRESET.MeshEdit_PinMenu,			"MeshEdit_PinMenu");
			CheckImageAndLoad(PRESET.MeshEdit_PinSelect,		"MeshEdit_PinSelect");
			CheckImageAndLoad(PRESET.MeshEdit_PinAdd,			"MeshEdit_PinAdd");
			CheckImageAndLoad(PRESET.MeshEdit_PinLink,			"MeshEdit_PinLink");
			CheckImageAndLoad(PRESET.MeshEdit_PinTest,			"MeshEdit_PinTest");
			CheckImageAndLoad(PRESET.MeshEdit_RemoveVertices,	"MeshEdit_RemoveVertices", true);
			

			CheckImageAndLoad(PRESET.TransformControlPoint, "TransformControlPoint");
			CheckImageAndLoad(PRESET.TransformAutoGenMapperCtrl, "TransformAutoGenMapperCtrl");


			CheckImageAndLoad(PRESET.Anim_Play, "Anim_Play", true);
			CheckImageAndLoad(PRESET.Anim_Pause, "Anim_Pause", true);
			CheckImageAndLoad(PRESET.Anim_PrevFrame, "Anim_PrevFrame", true);
			CheckImageAndLoad(PRESET.Anim_NextFrame, "Anim_NextFrame", true);
			CheckImageAndLoad(PRESET.Anim_FirstFrame, "Anim_FirstFrame", true);
			CheckImageAndLoad(PRESET.Anim_LastFrame, "Anim_LastFrame", true);
			CheckImageAndLoad(PRESET.Anim_Loop, "Anim_Loop", true);
			CheckImageAndLoad(PRESET.Anim_KeyOn, "Anim_KeyOn");
			CheckImageAndLoad(PRESET.Anim_KeyOff, "Anim_KeyOff");
			CheckImageAndLoad(PRESET.Anim_Keyframe, "Anim_Keyframe");
			CheckImageAndLoad(PRESET.Anim_KeyframeDummy, "Anim_KeyframeDummy");
			CheckImageAndLoad(PRESET.Anim_KeySummary, "Anim_KeySummary");
			CheckImageAndLoad(PRESET.Anim_KeySummaryMove, "Anim_KeySummaryMove");
			CheckImageAndLoad(PRESET.Anim_PlayBarHead, "Anim_PlayBarHead");

			CheckImageAndLoad(PRESET.Anim_TimelineSize1, "Anim_TimelineSize1", true);
			CheckImageAndLoad(PRESET.Anim_TimelineSize2, "Anim_TimelineSize2", true);
			CheckImageAndLoad(PRESET.Anim_TimelineSize3, "Anim_TimelineSize3", true);

			CheckImageAndLoad(PRESET.Anim_TimelineBGStart, "Anim_TimelineBGStart");
			CheckImageAndLoad(PRESET.Anim_TimelineBGEnd, "Anim_TimelineBGEnd");

			CheckImageAndLoad(PRESET.Anim_EventMark, "Anim_EventMark");
			CheckImageAndLoad(PRESET.Anim_OnionMark, "Anim_OnionMark");
			CheckImageAndLoad(PRESET.Anim_OnionRangeStart, "Anim_OnionRangeStart");
			CheckImageAndLoad(PRESET.Anim_OnionRangeEnd, "Anim_OnionRangeEnd");
			

			CheckImageAndLoad(PRESET.Anim_AutoZoom, "Anim_AutoZoom", true);
			CheckImageAndLoad(PRESET.Anim_WithMod, "Anim_WithMod");
			CheckImageAndLoad(PRESET.Anim_WithControlParam, "Anim_WithControlParam");
			CheckImageAndLoad(PRESET.Anim_WithBone, "Anim_WithBone");


			CheckImageAndLoad(PRESET.Anim_MoveToCurrentFrame, "Anim_MoveToCurrentFrame", true);
			CheckImageAndLoad(PRESET.Anim_MoveToNextFrame, "Anim_MoveToNextFrame", true);
			CheckImageAndLoad(PRESET.Anim_MoveToPrevFrame, "Anim_MoveToPrevFrame", true);
			CheckImageAndLoad(PRESET.Anim_KeyLoopLeft, "Anim_KeyLoopLeft");
			CheckImageAndLoad(PRESET.Anim_KeyLoopRight, "Anim_KeyLoopRight");
			CheckImageAndLoad(PRESET.Anim_AddKeyframe, "Anim_AddKeyframe");
			CheckImageAndLoad(PRESET.Anim_CurrentKeyframe, "Anim_CurrentKeyframe");
			CheckImageAndLoad(PRESET.Anim_KeyframeCursor, "Anim_KeyframeCursor");

			CheckImageAndLoad(PRESET.Anim_KeyframeMoveSrc, "Anim_KeyFrameMoveSrc");
			CheckImageAndLoad(PRESET.Anim_KeyframeMove, "Anim_KeyFrameMove");
			CheckImageAndLoad(PRESET.Anim_KeyframeCopy, "Anim_KeyFrameCopy");

			CheckImageAndLoad(PRESET.Anim_AutoKey, "Anim_AutoKey");
			

			CheckImageAndLoad(PRESET.Anim_ValueMode, "Anim_ValueMode", true);
			CheckImageAndLoad(PRESET.Anim_CurveMode, "Anim_CurveMode");

			CheckImageAndLoad(PRESET.Anim_AutoScroll, "Anim_AutoScroll");

			CheckImageAndLoad(PRESET.Anim_HideLayer, "Anim_HideLayer", true);
			CheckImageAndLoad(PRESET.Anim_SortABC, "Anim_SortABC");
			CheckImageAndLoad(PRESET.Anim_SortDepth, "Anim_SortDepth");
			CheckImageAndLoad(PRESET.Anim_SortRegOrder, "Anim_SortRegOrder");

			CheckImageAndLoad(PRESET.Anim_Load, "Anim_Load");
			CheckImageAndLoad(PRESET.Anim_Save, "Anim_Save");

			CheckImageAndLoad(PRESET.Anim_AddTimeline, "Anim_AddTimeline", true);
			CheckImageAndLoad(PRESET.Anim_AddAllBonesToLayer, "Anim_AddAllBonesToLayer", true);
			CheckImageAndLoad(PRESET.Anim_AddAllMeshesToLayer, "Anim_AddAllMeshesToLayer", true);
			CheckImageAndLoad(PRESET.Anim_AddAllControlParamsToLayer, "Anim_AddAllControlParamsToLayer", true);
			CheckImageAndLoad(PRESET.Anim_RemoveTimelineLayer, "Anim_RemoveTimelineLayer", true);

			CheckImageAndLoad(PRESET.Anim_CurvePreset_Acc, "Anim_CurvePreset_Acc");
			CheckImageAndLoad(PRESET.Anim_CurvePreset_Dec, "Anim_CurvePreset_Dec");
			CheckImageAndLoad(PRESET.Anim_CurvePreset_Default, "Anim_CurvePreset_Default");
			CheckImageAndLoad(PRESET.Anim_CurvePreset_Hard, "Anim_CurvePreset_Hard");

			CheckImageAndLoad(PRESET.Anim_180Lock, "Anim_180Lock");
			CheckImageAndLoad(PRESET.Anim_180Unlock, "Anim_180Unlock");

			CheckImageAndLoad(PRESET.AnimEvent_MainIcon, "AnimEvent_MainIcon");
			CheckImageAndLoad(PRESET.AnimEvent_Presets, "AnimEvent_Presets");
			
			

			CheckImageAndLoad(PRESET.Curve_ControlPoint, "Curve_ControlPoint");
			CheckImageAndLoad(PRESET.Curve_Linear, "Curve_Linear");
			CheckImageAndLoad(PRESET.Curve_Smooth, "Curve_Smooth");
			CheckImageAndLoad(PRESET.Curve_Stepped, "Curve_Stepped");
			CheckImageAndLoad(PRESET.Curve_Prev, "Curve_Prev");
			CheckImageAndLoad(PRESET.Curve_Next, "Curve_Next");

			CheckImageAndLoad(PRESET.Rig_Add, "Rig_Add");
			CheckImageAndLoad(PRESET.Rig_EditMode, "Rig_EditMode");
			CheckImageAndLoad(PRESET.Rig_Select, "Rig_Select");
			CheckImageAndLoad(PRESET.Rig_Link, "Rig_Link");

			CheckImageAndLoad(PRESET.Rig_IKDisabled, "Rig_IKDisabled");
			CheckImageAndLoad(PRESET.Rig_IKHead, "Rig_IKHead");
			CheckImageAndLoad(PRESET.Rig_IKChained, "Rig_IKChained");
			CheckImageAndLoad(PRESET.Rig_IKSingle, "Rig_IKSingle");

			CheckImageAndLoad(PRESET.Rig_SaveLoad, "Rig_SaveLoad");
			CheckImageAndLoad(PRESET.Rig_LoadBones, "Rig_LoadBones");
			CheckImageAndLoad(PRESET.Rig_LoadBonesMirror, "Rig_LoadBonesMirror");
			

			CheckImageAndLoad(PRESET.Rig_HierarchyIcon_IKHead, "Rig_HierarchyIcon_IKHead");
			CheckImageAndLoad(PRESET.Rig_HierarchyIcon_IKChained, "Rig_HierarchyIcon_IKChained");
			CheckImageAndLoad(PRESET.Rig_HierarchyIcon_IKSingle, "Rig_HierarchyIcon_IKSingle");

			CheckImageAndLoad(PRESET.Rig_EditBinding, "Rig_EditBinding");
			CheckImageAndLoad(PRESET.Rig_AutoNormalize, "Rig_AutoNormalize");
			CheckImageAndLoad(PRESET.Rig_Auto, "Rig_Auto");
			CheckImageAndLoad(PRESET.Rig_Blend, "Rig_Blend");
			CheckImageAndLoad(PRESET.Rig_Normalize, "Rig_Normalize");
			CheckImageAndLoad(PRESET.Rig_Prune, "Rig_Prune");
			CheckImageAndLoad(PRESET.Rig_AddWeight, "Rig_AddWeight", true);
			CheckImageAndLoad(PRESET.Rig_MultiplyWeight, "Rig_MultiplyWeight", true);
			CheckImageAndLoad(PRESET.Rig_SubtractWeight, "Rig_SubtractWeight", true);
			CheckImageAndLoad(PRESET.Rig_TestPosing, "Rig_TestPosing");
			CheckImageAndLoad(PRESET.Rig_WeightColorOnly, "Rig_WeightColorOnly");
			CheckImageAndLoad(PRESET.Rig_WeightColorWithTexture, "Rig_WeightColorWithTexture");

			CheckImageAndLoad(PRESET.Rig_Grow,		"Rig_Grow");
			CheckImageAndLoad(PRESET.Rig_Shrink,	"Rig_Shrink");

			CheckImageAndLoad(PRESET.Rig_BrushAdd,			"Rig_BrushAdd");
			CheckImageAndLoad(PRESET.Rig_BrushMultiply,		"Rig_BrushMultiply");
			CheckImageAndLoad(PRESET.Rig_BrushBlur,			"Rig_BrushBlur");
			CheckImageAndLoad(PRESET.Rig_Lock16px,			"Rig_Lock16px");
			CheckImageAndLoad(PRESET.Rig_Unlock16px,		"Rig_Unlock16px");
			CheckImageAndLoad(PRESET.Rig_WeightMode16px,	"Rig_WeightMode16px");
			CheckImageAndLoad(PRESET.Rig_PaintMode16px,		"Rig_PaintMode16px");

			CheckImageAndLoad(PRESET.Rig_BoneColor,			"Rig_BoneColor");
			CheckImageAndLoad(PRESET.Rig_NoBoneColor,		"Rig_NoBoneColor");
			CheckImageAndLoad(PRESET.Rig_SquareColorVert,	"Rig_SquareColorVert");
			CheckImageAndLoad(PRESET.Rig_CircleVert,		"Rig_CircleVert");

			CheckImageAndLoad(PRESET.Rig_ShowAllBones,		"Rig_ShowAllBones");
			CheckImageAndLoad(PRESET.Rig_TransculentBones,	"Rig_TransculentBones");
			CheckImageAndLoad(PRESET.Rig_HideBones,			"Rig_HideBones");

			CheckImageAndLoad(PRESET.Rig_Jiggle,			"Rig_Jiggle");


			CheckImageAndLoad(PRESET.Physic_Stretch, "Physic_Stretch");
			CheckImageAndLoad(PRESET.Physic_Bend, "Physic_Bend");
			CheckImageAndLoad(PRESET.Physic_Gravity, "Physic_Gravity");
			CheckImageAndLoad(PRESET.Physic_Wind, "Physic_Wind");
			CheckImageAndLoad(PRESET.Physic_SetMainVertex, "Physic_SetMainVertex");
			CheckImageAndLoad(PRESET.Physic_VertConst, "Physic_VertConst");
			CheckImageAndLoad(PRESET.Physic_VertMain, "Physic_VertMain");

			CheckImageAndLoad(PRESET.Physic_BasicSetting, "Physic_BasicSetting", true);
			CheckImageAndLoad(PRESET.Physic_Inertia, "Physic_Inertia");
			CheckImageAndLoad(PRESET.Physic_Recover, "Physic_Recover");
			CheckImageAndLoad(PRESET.Physic_Viscosity, "Physic_Viscosity");
			CheckImageAndLoad(PRESET.Physic_Palette, "Physic_Palette");

			CheckImageAndLoad(PRESET.Physic_PresetCloth1, "Physic_PresetCloth1");
			CheckImageAndLoad(PRESET.Physic_PresetCloth2, "Physic_PresetCloth2");
			CheckImageAndLoad(PRESET.Physic_PresetCloth3, "Physic_PresetCloth3");
			CheckImageAndLoad(PRESET.Physic_PresetFlag, "Physic_PresetFlag");
			CheckImageAndLoad(PRESET.Physic_PresetHair, "Physic_PresetHair");
			CheckImageAndLoad(PRESET.Physic_PresetRibbon, "Physic_PresetRibbon");
			CheckImageAndLoad(PRESET.Physic_PresetRubberHard, "Physic_PresetRubberHard");
			CheckImageAndLoad(PRESET.Physic_PresetRubberSoft, "Physic_PresetRubberSoft");
			CheckImageAndLoad(PRESET.Physic_PresetCustom1, "Physic_PresetCustom1");
			CheckImageAndLoad(PRESET.Physic_PresetCustom2, "Physic_PresetCustom2");
			CheckImageAndLoad(PRESET.Physic_PresetCustom3, "Physic_PresetCustom3");

			CheckImageAndLoad(PRESET.ParamPreset_Body, "ParamPreset_Body");
			CheckImageAndLoad(PRESET.ParamPreset_Cloth, "ParamPreset_Cloth");
			CheckImageAndLoad(PRESET.ParamPreset_Equip, "ParamPreset_Equip");
			CheckImageAndLoad(PRESET.ParamPreset_Etc, "ParamPreset_Etc");
			CheckImageAndLoad(PRESET.ParamPreset_Eye, "ParamPreset_Eye");
			CheckImageAndLoad(PRESET.ParamPreset_Face, "ParamPreset_Face");
			CheckImageAndLoad(PRESET.ParamPreset_Force, "ParamPreset_Force");
			CheckImageAndLoad(PRESET.ParamPreset_Hair, "ParamPreset_Hair");
			CheckImageAndLoad(PRESET.ParamPreset_Hand, "ParamPreset_Hand");
			CheckImageAndLoad(PRESET.ParamPreset_Head, "ParamPreset_Head");

			CheckImageAndLoad(PRESET.Demo_Logo, "Demo_Logo");

			CheckImageAndLoad(PRESET.AutoSave_Frame1, "AutoSave_Frame1");
			CheckImageAndLoad(PRESET.AutoSave_Frame2, "AutoSave_Frame2");

			CheckImageAndLoad(PRESET.GUI_SelectionLock, "GUI_SelectionLock");

			CheckImageAndLoad(PRESET.StartPageLogo_Full, "StartPageLogo_Full");
			CheckImageAndLoad(PRESET.StartPageLogo_Demo, "StartPageLogo_Demo");
			CheckImageAndLoad(PRESET.StartPage_GettingStarted, "StartPage_GettingStarted");
			CheckImageAndLoad(PRESET.StartPage_VideoTutorial, "StartPage_VideoTutorial");
			CheckImageAndLoad(PRESET.StartPage_Manual, "StartPage_Manual");
			CheckImageAndLoad(PRESET.StartPage_Forum, "StartPage_Forum");

			CheckImageAndLoad(PRESET.SmallMod_AnimMorph,		"SmallMod_AnimMorph");
			CheckImageAndLoad(PRESET.SmallMod_Morph,			"SmallMod_Morph");
			CheckImageAndLoad(PRESET.SmallMod_AnimTF,			"SmallMod_AnimTF");
			CheckImageAndLoad(PRESET.SmallMod_TF,				"SmallMod_TF");
			CheckImageAndLoad(PRESET.SmallMod_Physics,			"SmallMod_Physics");
			CheckImageAndLoad(PRESET.SmallMod_Rigging,			"SmallMod_Rigging");
			CheckImageAndLoad(PRESET.SmallMod_ControlLayer,		"SmallMod_ControlLayer");
			CheckImageAndLoad(PRESET.SmallMod_ExEnabled,		"SmallMod_ExEnabled");
			CheckImageAndLoad(PRESET.SmallMod_ExSubEnabled,		"SmallMod_ExSubEnabled");
			CheckImageAndLoad(PRESET.SmallMod_ExDisabled,		"SmallMod_ExDisabled");
			CheckImageAndLoad(PRESET.SmallMod_ColorEnabled,		"SmallMod_ColorEnabled");
			CheckImageAndLoad(PRESET.SmallMod_ColorDisabled,	"SmallMod_ColorDisabled");

			CheckImageAndLoad(PRESET.SmallMod_CursorLocked,		"SmallMod_CursorLocked");
			CheckImageAndLoad(PRESET.SmallMod_CursorUnlocked,	"SmallMod_CursorUnlocked");

			CheckImageAndLoad(PRESET.SmallMod_ColorOnly,		"SmallMod_ColorOnly");
			CheckImageAndLoad(PRESET.SmallMod_AnimColorOnly,	"SmallMod_AnimColorOnly");

			CheckImageAndLoad(PRESET.Capture_Frame,				"Capture_Frame", true);
			CheckImageAndLoad(PRESET.Capture_Tab,				"Capture_Tab");
			CheckImageAndLoad(PRESET.Capture_Thumbnail,			"Capture_Thumbnail");
			CheckImageAndLoad(PRESET.Capture_Image,				"Capture_Image");
			CheckImageAndLoad(PRESET.Capture_GIF,				"Capture_GIF");
			CheckImageAndLoad(PRESET.Capture_Sprite,			"Capture_Sprite");

			CheckImageAndLoad(PRESET.Capture_ExportThumb,		"Capture_ExportThumb");
			CheckImageAndLoad(PRESET.Capture_ExportScreenshot,	"Capture_ExportScreenshot");
			CheckImageAndLoad(PRESET.Capture_ExportGIF,			"Capture_ExportGIF");
			CheckImageAndLoad(PRESET.Capture_ExportSprite,		"Capture_ExportSprite");
			CheckImageAndLoad(PRESET.Capture_ExportSequence,	"Capture_ExportSequence");
			CheckImageAndLoad(PRESET.Capture_ExportMP4,			"Capture_ExportMP4");

			CheckImageAndLoad(PRESET.OnionSkin_SingleFrame,		"OnionSkin_SingleFrame");
			CheckImageAndLoad(PRESET.OnionSkin_MultipleFrame,	"OnionSkin_MultipleFrame");

			CheckImageAndLoad(PRESET.PSD_Set,					"PSD_Set");
			CheckImageAndLoad(PRESET.PSD_BakeEnabled,			"PSD_BakeEnabled");
			CheckImageAndLoad(PRESET.PSD_BakeDisabled,			"PSD_BakeDisabled");
			CheckImageAndLoad(PRESET.PSD_LinkView,				"PSD_LinkView");
			CheckImageAndLoad(PRESET.PSD_Overlay,				"PSD_Overlay");
			CheckImageAndLoad(PRESET.PSD_Switch,				"PSD_Switch");
			CheckImageAndLoad(PRESET.PSD_SetOutline,			"PSD_SetOutline");
			CheckImageAndLoad(PRESET.PSD_MeshOutline,			"PSD_MeshOutline");
			CheckImageAndLoad(PRESET.PSD_SetSecondary,			"PSD_SetSecondary");
			CheckImageAndLoad(PRESET.PSD_SetSecondaryOutline,	"PSD_SetSecondaryOutline");
			CheckImageAndLoad(PRESET.PSD_LinkedMain,			"PSD_LinkedMain");
			CheckImageAndLoad(PRESET.PSD_LinkedMainOutline,		"PSD_LinkedMainOutline");
			CheckImageAndLoad(PRESET.PSD_TextureSampling,		"PSD_TextureSampling");
			

			CheckImageAndLoad(PRESET.ExtraOption_DepthCursor,		"ExtraOption_DepthCursor");
			CheckImageAndLoad(PRESET.ExtraOption_DepthMidCursor,	"ExtraOption_DepthMidCursor");
			CheckImageAndLoad(PRESET.ExtraOption_UnsyncImage,		"ExtraOption_UnsyncImage");
			CheckImageAndLoad(PRESET.ExtraOption_MovedMesh,			"ExtraOption_MovedMesh");
			CheckImageAndLoad(PRESET.ExtraOption_MovedMeshGroup,	"ExtraOption_MovedMeshGroup");
			CheckImageAndLoad(PRESET.ExtraOption_NonImage,			"ExtraOption_NonImage");
			

			CheckImageAndLoad(PRESET.LowCPU,					"LowCPU");
			CheckImageAndLoad(PRESET.GUI_Button_Menu,				"GUI_Button_Menu");
			CheckImageAndLoad(PRESET.GUI_Button_Menu_Roll,			"GUI_Button_Menu_Roll");
			CheckImageAndLoad(PRESET.GUI_Button_RecordOnion,		"GUI_Button_RecordOnion");
			CheckImageAndLoad(PRESET.GUI_Button_RecordOnion_Roll,	"GUI_Button_RecordOnion_Roll");

			CheckImageAndLoad(PRESET.GUI_Button_EditVert_Disabled,	"GUI_Button_EditVert_Disabled");
			CheckImageAndLoad(PRESET.GUI_Button_EditVert_RollOver,	"GUI_Button_EditVert_RollOver");
			CheckImageAndLoad(PRESET.GUI_Button_EditVert_Enabled,	"GUI_Button_EditVert_Enabled");
			CheckImageAndLoad(PRESET.GUI_Button_EditVert_EnabledRollOver,	"GUI_Button_EditVert_EnabledRollOver");
			CheckImageAndLoad(PRESET.GUI_Button_EditPin_Disabled,	"GUI_Button_EditPin_Disabled");
			CheckImageAndLoad(PRESET.GUI_Button_EditPin_RollOver,	"GUI_Button_EditPin_RollOver");
			CheckImageAndLoad(PRESET.GUI_Button_EditPin_Enabled,	"GUI_Button_EditPin_Enabled");
			CheckImageAndLoad(PRESET.GUI_Button_EditPin_EnabledRollOver,	"GUI_Button_EditPin_EnabledRollOver");

			CheckImageAndLoad(PRESET.GUI_ViewStat_BoneHidden,		"GUI_ViewStat_BoneHidden");
			CheckImageAndLoad(PRESET.GUI_ViewStat_BoneOutline,		"GUI_ViewStat_BoneOutline");
			CheckImageAndLoad(PRESET.GUI_ViewStat_DisablePhysics,	"GUI_ViewStat_DisablePhysics");
			CheckImageAndLoad(PRESET.GUI_ViewStat_MeshHidden,		"GUI_ViewStat_MeshHidden");
			CheckImageAndLoad(PRESET.GUI_ViewStat_OnionSkin,		"GUI_ViewStat_OnionSkin");
			CheckImageAndLoad(PRESET.GUI_ViewStat_PresetVisible,	"GUI_ViewStat_PresetVisible");
			CheckImageAndLoad(PRESET.GUI_ViewStat_Rotoscoping,		"GUI_ViewStat_Rotoscoping");

			
			CheckImageAndLoad(PRESET.GUI_ViewStat_BG,					"GUI_ViewStat_BG");
			CheckImageAndLoad(PRESET.GUI_EditStat_SingleModifier,		"GUI_EditStat_SingleModifier");
			CheckImageAndLoad(PRESET.GUI_EditStat_MultiModifiers,		"GUI_EditStat_MultiModifiers");
			CheckImageAndLoad(PRESET.GUI_EditStat_MultiModifiers_Impossible, "GUI_EditStat_MultiModifiers_Impossible");
			CheckImageAndLoad(PRESET.GUI_EditStat_PreviewBone,			"GUI_EditStat_PreviewBone");
			CheckImageAndLoad(PRESET.GUI_EditStat_PreviewBoneAndColor,	"GUI_EditStat_PreviewBoneAndColor");
			CheckImageAndLoad(PRESET.GUI_EditStat_PreviewColor,			"GUI_EditStat_PreviewColor");
			CheckImageAndLoad(PRESET.GUI_EditStat_SelectionLock,		"GUI_EditStat_SelectionLock");
			CheckImageAndLoad(PRESET.GUI_EditStat_SelectionUnlock,		"GUI_EditStat_SelectionUnlock");
			CheckImageAndLoad(PRESET.GUI_EditStat_SemiSelectionLock,	"GUI_EditStat_SemiSelectionLock");

			CheckImageAndLoad(PRESET.MaterialSet,				"MaterialSet");
			CheckImageAndLoad(PRESET.MaterialSetIcon_Unlit,		"MaterialSetIcon_Unlit");
			CheckImageAndLoad(PRESET.MaterialSetIcon_Lit,		"MaterialSetIcon_Lit");
			CheckImageAndLoad(PRESET.MaterialSetIcon_LitSpecular,			"MaterialSetIcon_LitSpecular");
			CheckImageAndLoad(PRESET.MaterialSetIcon_LitSpecularEmission,	"MaterialSetIcon_LitSpecularEmission");
			CheckImageAndLoad(PRESET.MaterialSetIcon_LitRim,				"MaterialSetIcon_LitRim");
			CheckImageAndLoad(PRESET.MaterialSetIcon_LitRamp,	"MaterialSetIcon_LitRamp");
			CheckImageAndLoad(PRESET.MaterialSetIcon_FX,		"MaterialSetIcon_FX");
			CheckImageAndLoad(PRESET.MaterialSetIcon_Cartoon,	"MaterialSetIcon_Cartoon");
			CheckImageAndLoad(PRESET.MaterialSetIcon_Custom1,	"MaterialSetIcon_Custom1");
			CheckImageAndLoad(PRESET.MaterialSetIcon_Custom2,	"MaterialSetIcon_Custom2");
			CheckImageAndLoad(PRESET.MaterialSetIcon_Custom3,	"MaterialSetIcon_Custom3");
			CheckImageAndLoad(PRESET.MaterialSetIcon_UnlitVR,	"MaterialSetIcon_UnlitVR");
			CheckImageAndLoad(PRESET.MaterialSetIcon_LitVR,		"MaterialSetIcon_LitVR");
			CheckImageAndLoad(PRESET.MaterialSetIcon_MergeableUnlit,	"MaterialSetIcon_MergeableUnlit");
			CheckImageAndLoad(PRESET.MaterialSetIcon_MergeableLit,		"MaterialSetIcon_MergeableLit");

			CheckImageAndLoad(PRESET.MaterialSet_BasicSettings,		"MaterialSet_BasicSettings");
			CheckImageAndLoad(PRESET.MaterialSet_ShaderProperties,	"MaterialSet_ShaderProperties");
			CheckImageAndLoad(PRESET.MaterialSet_Shaders,			"MaterialSet_Shaders");
			CheckImageAndLoad(PRESET.MaterialSet_Reserved,			"MaterialSet_Reserved");
			CheckImageAndLoad(PRESET.MaterialSet_CustomShader,		"MaterialSet_CustomShader");
			CheckImageAndLoad(PRESET.MaterialSet_LWRP,				"MaterialSet_LWRP");
			CheckImageAndLoad(PRESET.MaterialSet_VR,				"MaterialSet_VR");
			CheckImageAndLoad(PRESET.MaterialSet_URP,				"MaterialSet_URP");
			CheckImageAndLoad(PRESET.MaterialSet_Mergeable,			"MaterialSet_Mergeable");
			
			

			CheckImageAndLoad(PRESET.RestoreTmpVisibility_ON,	"RestoreTmpVisibility_ON");
			CheckImageAndLoad(PRESET.RestoreTmpVisibility_OFF,	"RestoreTmpVisibility_OFF");

			CheckImageAndLoad(PRESET.RecommendedIcon,	"RecommendedIcon");

			CheckImageAndLoad(PRESET.BoneSpriteSheet,	"BoneSpriteSheet");
			CheckImageAndLoad(PRESET.PinVertGUIAtlas,	"PinVertGUIAtlas");
			CheckImageAndLoad(PRESET.PinOption_Range,			"PinOption_Range");
			CheckImageAndLoad(PRESET.PinOption_Tangent_Smooth,	"PinOption_Tangent_Smooth");
			CheckImageAndLoad(PRESET.PinOption_Tangent_Sharp,	"PinOption_Tangent_Sharp");
			CheckImageAndLoad(PRESET.PinCalculateWeight,		"PinCalculateWeight");
			CheckImageAndLoad(PRESET.PinCalculateWeightAuto,	"PinCalculateWeightAuto");
			CheckImageAndLoad(PRESET.PinRestTestPos,			"PinRestTestPos");

			CheckImageAndLoad(PRESET.BakeBtn_Normal,		"BakeBtn_Normal");
			CheckImageAndLoad(PRESET.BakeBtn_Optimized,		"BakeBtn_Optimized");

			CheckImageAndLoad(PRESET.SetValueProp_Color,		"SetValueProp_Color");
			CheckImageAndLoad(PRESET.SetValueProp_Extra,		"SetValueProp_Extra");
			CheckImageAndLoad(PRESET.SetValueProp_Pin,			"SetValueProp_Pin");
			CheckImageAndLoad(PRESET.SetValueProp_Transform,	"SetValueProp_Transform");
			CheckImageAndLoad(PRESET.SetValueProp_Vert,			"SetValueProp_Vert");
			CheckImageAndLoad(PRESET.SetValueProp_Visibility,	"SetValueProp_Visibility");

			CheckImageAndLoad(PRESET.SyncSettingToFile16px,		"SyncSettingToFile16px");
			CheckImageAndLoad(PRESET.SyncSettingToFile12px,		"SyncSettingToFile12px");
			
			if(!_isAllLoaded)
			{
				//문제가 발생했다.
				return ReloadResult.Error;
			}



			//return true;
			return ReloadResult.NewLoaded;

		}

		private void CheckImageAndLoad(PRESET imageType, string strFileNameWOExp, bool isProSkinVersion = false)
		{
			bool isLoadProSkin = EditorGUIUtility.isProSkin && isProSkinVersion;


			#region [미사용 코드] 비효율적인 코드
			//if (_images.ContainsKey(imageType))
			//{
			//	if (_images[imageType] == null)
			//	{
			//		//기본 경로 변경
			//		//"Assets/Editor/AnyPortraitTool/Images/" => apEditorUtil.ResourcePath_Icon

			//		//이전 코드
			//		//if (EditorGUIUtility.isProSkin && isProSkinVersion)//정식 코드
			//		////if (!EditorGUIUtility.isProSkin && isProSkinVersion)//테스트 코드
			//		//{
			//		//	_images[imageType] = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + "ProSkin/" + strFileNameWOExp + ".png");
			//		//}
			//		//else
			//		//{
			//		//	_images[imageType] = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + strFileNameWOExp + ".png");
			//		//}

			//		//변경된 코드 20.3.17
			//		_images[imageType] = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.MakePath_Icon(strFileNameWOExp, isLoadProSkin));
			//	}
			//}
			//else
			//{
			//	//이전 코드
			//	//if (EditorGUIUtility.isProSkin && isProSkinVersion)//정식 코드
			//	////if (!EditorGUIUtility.isProSkin && isProSkinVersion)//테스트 코드
			//	//{
			//	//	_images.Add(imageType, AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + "ProSkin/" + strFileNameWOExp + ".png"));
			//	//}
			//	//else
			//	//{
			//	//	_images.Add(imageType, AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.ResourcePath_Icon + strFileNameWOExp + ".png"));
			//	//}

			//	//변경된 코드 20.3.7
			//	_images.Add(imageType, AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.MakePath_Icon(strFileNameWOExp, isLoadProSkin)));

			//}

			//if (_images[imageType] == null)
			//{
			//	Debug.LogError("Editor Image Load Failed : " + imageType);
			//	_isAllLoaded = false;
			//} 
			#endregion

			//변경 v1.4.2
			Texture2D loadedTexture = null;

			if (_images.ContainsKey(imageType))
			{
				loadedTexture = _images[imageType];
				if (loadedTexture == null)
				{	
					loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.MakePath_Icon(strFileNameWOExp, isLoadProSkin));
					_images[imageType] = loadedTexture;
				}
			}
			else
			{
				loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(apEditorUtil.MakePath_Icon(strFileNameWOExp, isLoadProSkin));

				_images.Add(imageType, loadedTexture);
			}

			if (loadedTexture == null)
			{
				Debug.LogError("Editor Image Load Failed : " + imageType);
				_isAllLoaded = false;
			}

			//추가 v1.4.2 : 빠른 접근을 위한 배열을 만들어서 넣자
			_index2Image[(int)imageType] = loadedTexture;
			
		}


		

		//----------------------------------------------------------------------------
		public Texture2D Get(PRESET imageType)
		{
			//이전 방식
			//if (!_images.ContainsKey(imageType))
			//{
			//	_isAllLoaded = false;
			//	return null;
			//}

			//if (_images[imageType] == null)
			//{
			//	_isAllLoaded = false;
			//	return null;
			//}

			//return _images[imageType];

			//변경 v1.4.2 : 배열을 이용해서 더 빠르게 수행
			int iImageType = (int)imageType;

			//초기화가 필요한 경우
			if (!_isAllLoaded
				|| _index2Image == null
				|| iImageType >= _nImages)
			{
				_isAllLoaded = false;
				return null;
			}

			_cal_Result = _index2Image[iImageType];
			if (_cal_Result == null)
			{
				//null 이미지가 있다면 다시 초기화를 해야한다.
				_isAllLoaded = false;
				return null;
			}

			return _cal_Result;
		}
	}
}