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
using System.Text;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 19.12.2 : 에디터에 포함되는 각종 문자열을 미리 만들고 호출하자. (번역 안되는 텍스트들)
	/// apEditor에 포함되며, static 호출이 가능하다.
	/// </summary>
	public class apStringFactory
	{
		// Members
		//-------------------------------------------
		private bool _isInitialized = false;
		private static apStringFactory s_instance = null;

		private string _str_None = null;
		private string _str_Space1 = null;
		private string _str_Space2 = null;
		private string _str_Space3 = null;
		private string _str_UnityEditorIsPlaying = null;
		private string _str_ResetZoomAndPosition = null;
		private string _str_ToggleWorkspaceSize = null;
		private string _str_ToggleWorkdspaceSize_HotKey = null;
		private string _str_SettingsOfTherPortraitAndEditor = null;
		private string _str_BakeToScene = null;

		private string _str_Ctrl = null;
		private string _str_Command = null;
		private string _str_Alt = null;
		private string _str_Option = null;
		private string _str_Shift = null;

		private string _str_BoneVisibility = null;
		private string _str_MeshVisibleToolTip = null;
		private string _str_PhysicFxEnableToolTip = null;
		private string _str_FFDModeToolTip_1 = null;
		private string _str_FFDModeToolTip_2 = null;
		private string _str_ApplyFFD = null;
		private string _str_RevertFFD = null;
		private string _str_SoftSelectionToolTip_WithHotkey = null;
		private string _str_BlurToolTip_WithHotkey = null;
		private string _str_SoftSelectionToolTip_WOHotkey = null;
		private string _str_BlurToolTip_WOHotkey = null;
		private string _str_ToggleOnionSkin = null;
		private string _str_RecordOnionRecord = null;

		private string _str_Select = null;
		private string _str_Move = null;
		private string _str_Rotate = null;
		private string _str_Scale = null;

		private string _str_IncreaseBrushSize = null;
		private string _str_DecreaseBrushSize = null;
		private string _str_RemovePolygon = null;
		private string _str_CreateANewPortrait = null;
		private string _str_SearchPortraitsAgainInTheScene = null;
		private string _str_LoadBackupFileToolTip = null;

		private string _str_RootUnits = null;
		private string _str_Images = null;
		private string _str_Meshes = null;
		private string _str_MeshGroups = null;
		private string _str_AnimationClips = null;
		private string _str_ControlParameters = null;

		private string _str_HierarchySortModeToolTip_RegOrder = null;
		private string _str_HierarchySortModeToolTip_AlphaNum = null;
		private string _str_HierarchySortModeToolTip_Custom = null;
		private string _str_HierarchySortModeToolTip_Toggle = null;

		private string _str_ParamEdit = null;

		private string _str_Keyframes = null;
		private string _str_TimelineLayers = null;
		private string _str_ClippedVertices = null;
		private string _str_ClippedMeshes = null;
		private string _str_Triangles = null;
		private string _str_Edges = null;
		private string _str_Vertices = null;
		private string _str_Statistics = null;

		private string _str_AddSubMeshMeshGroup = null;
		private string _str_SetClippingMask = null;
		private string _str_LayerUp = null;
		private string _str_LayerDown = null;
		private string _str_RemoveSubMeshMeshGroup = null;

		private string _str_Bracket_1_L = null;
		private string _str_Bracket_1_R = null;
		private string _str_Bracket_2_L = null;
		private string _str_Bracket_2_R = null;
		private string _str_Bracket_3_L = null;
		private string _str_Bracket_3_R = null;
		private string _str_Bracket_4_L = null;
		private string _str_Bracket_4_R = null;

		private string _str_Colon = null;
		private string _str_Colon_Space = null;
		private string _str_Dot1 = null;
		private string _str_Dot2 = null;
		private string _str_Dot3 = null;
		private string _str_Plus = null;
		private string _str_Minus = null;
		private string _str_Slash = null;
		private string _str_Slash_Space = null;
		private string _str_Comma = null;
		private string _str_Comma_Space = null;
		private string _str_Return = null;
		private string _str_Gamma = null;
		private string _str_Linear = null;
		private string _str_QuestionMark = null;

		private string _str_SettingsOfMesh = null;
		private string _str_MakeVerticesAndPolygons = null;
		private string _str_EditPivotOfMesh = null;
		private string _str_ModifyVertices = null;
		private string _str_SettingsOfMeshGroup = null;
		private string _str_BonesOfMeshGroup = null;
		private string _str_ModifiersOfMeshGroup = null;
		private string _str_SettingsOfRootUnit = null;
		private string _str_CapturingTheScreenShot = null;

		private string _str_ErrorNoMeshGroupLinked = null;
		private string _str_MakeAThumbnail = null;
		private string _str_MakeAScreenshot = null;
		private string _str_MakeAGIFAnimation = null;
		private string _str_MakeSpriteSheets = null;

		private string _str_X = null;
		private string _str_Y = null;
		private string _str_Z = null;
		private string _str_Set = null;

		private string _str_L = null;
		private string _str_T = null;
		private string _str_R = null;
		private string _str_B = null;

		private string _str_GIF = null;
		private string _str_XML = null;
		private string _str_JSON = null;
		private string _str_TXT = null;
		private string _str_NoImage = null;
		private string _str_RemoveAllVerticesAndPolygons = null;

		private string _str_Index_Colon = null;
		private string _str_DepthZeroToOne = null;
		private string _str_UV = null;
		private string _str_VerticesSelected = null;
		private string _str_Min = null;
		private string _str_Max = null;
		private string _str_Average = null;
		private string _str_Min_Colon = null;
		private string _str_Max_Colon = null;
		private string _str_Average_Colon = null;
		private string _str_SetMeshZDepthWeightTooltip = null;
		private string _str_NoVertexSelected = null;
		private string _str_MakePolygonsAndRefreshMesh = null;
		private string _str_CreateAMeshManually = null;
		private string _str_SelectAndModifyVertices = null;
		private string _str_GenerateAMeshAutomatically = null;

		private string _str_MakeMeshTooltip_AddVertexLinkEdge = null;
		private string _str_MakeMeshTooltip_AddVertex = null;
		private string _str_MakeMeshTooltip_LinkEdge = null;
		private string _str_MakeMeshTooltip_SelectPolygon = null;

		private string _str_RemoveVertices = null;

		private string _str_EditDefaultTransformsOfSubMeshesMeshGroups = null;

		private string _str_MakeMeshGroupAsRootUnit = null;
		private string _str_EditBones = null;

		private string _str_NotEditable = null;

		private string _str_HowToControl_None = null;
		private string _str_HowToControl_MoveView = null;

		private string _str_AddANewModifier = null;

		private string _str_BindingModeToggleTooltip = null;
		private string _str_EditModeToggleTooltip = null;
		private string _str_SelectionLockToggleTooltip = null;
		//private string _str_ModifierLockToggleTooltip_1 = null;
		//private string _str_ModifierLockToggleTooltip_2 = null;
		//private string _str_ToggleEditingMode = null;
		//private string _str_ToggleSelectionLock = null;
		//private string _str_ToggleModifierLock = null;
		//private string _str_ToggleLayerLock = null;
		private string _str_RiggingViewModeTooltip_ColorWithTexture = null;
		private string _str_RiggingViewModeTooltip_BoneColor = null;
		private string _str_RiggingViewModeTooltip_CircleVert = null;
		private string _str_RiggingViewModeTooltip_NoLinkedBoneVisibility = null;
		private string _str_RiggingViewModeTooltip_TestPose = null;

		private string _str_SimulateWindForce = null;
		private string _str_ClearWindForce = null;
		private string _str_AnimationEditModeToggleTooltip = null;

		//private string _str_TimelineLayerLockToggleTooltip_1 = null;
		//private string _str_TimelineLayerLockToggleTooltip_2 = null;
		private string _str_AddKeyframe = null;
		private string _str_RemoveKeyframe = null;
		private string _str_RemoveKeyframes = null;
		//private string _str_AddNewKeyframe = null;

		private string _str_PlayPause = null;
		private string _str_PreviousFrame = null;
		private string _str_NextFrame = null;
		private string _str_FirstFrame = null;
		private string _str_LastFrame = null;
		private string _str_CopyKeyframes = null;
		private string _str_PasteKeyframes = null;
		private string _str_ToggleAnimLoop = null;
		private string _str_AnimTimelineSort_RegOrder = null;
		private string _str_AnimTImelineSort_Name = null;
		private string _str_AnimTimelineSort_Depth = null;
		private string _str_AnimTimelineSize_Small = null;
		private string _str_AnimTimelineSize_Medium = null;
		private string _str_AnimTimelineSize_Large = null;
		private string _str_AnimTimelineFit = null;
		private string _str_AnimTimelineFitTooltip = null;
		private string _str_AnimTimelineAutoScrollTooltip = null;
		private string _str_AnimTimelineAutoKeyTooltip = null;
		private string _str_AnimCurveTooltip_Linear = null;
		private string _str_AnimCurveTooltip_Smooth = null;
		private string _str_AnimCurveTooltip_Constant = null;

		private string _str_ON = null;
		private string _str_OFF = null;

		private string _str_NoMaskParent = null;

		private string _str_IKSingle = null;
		private string _str_IKHead = null;
		private string _str_IKChain = null;
		private string _str_Disabled = null;

		private string _str_EmptyClipboard = null;

		private string _str_NoVertex = null;
		private string _str_Vertex = null;
		private string _str_VertexWithBracket = null;
		private string _str_VerticesWithSpace = null;
		private string _str_NoBone = null;
		private string _str_Weight_00 = null;
		private string _str_Weight_01 = null;
		private string _str_Weight_03 = null;
		private string _str_Weight_05 = null;
		private string _str_Weight_07 = null;
		private string _str_Weight_09 = null;
		private string _str_Weight_10 = null;

		private string _str_IncreaseBrushRadius = null;
		private string _str_DecreaseBrushRadius = null;
		private string _str_BrushMode_Add = null;
		private string _str_BrushMode_Multiply = null;
		private string _str_BrushMode_Blur = null;
		private string _str_IncreaseBrushIntensity = null;
		private string _str_DecreaseBrushIntensity = null;
		private string _str_RiggingTooltip_Blend = null;
		private string _str_RiggingTooltip_Normalize = null;
		private string _str_RiggingTooltip_Prune = null;
		private string _str_RiggingTooltip_AutoRig = null;
		private string _str_RiggingTooltip_Grow = null;
		private string _str_RiggingTooltip_Shrink = null;
		private string _str_RiggingTooltip_SelectVerticesOfBone = null;

		private string _str_Float = null;
		private string _str_Integer = null;
		private string _str_Vector = null;
		private string _str_Texture = null;
		private string _str_Color = null;

		private string _str_Num0 = null;
		private string _str_Num1 = null;
		private string _str_Num2 = null;
		private string _str_Num3 = null;
		private string _str_Num4 = null;
		private string _str_Num5 = null;
		private string _str_Num6 = null;
		private string _str_Num7 = null;
		private string _str_Num8 = null;
		private string _str_Num9 = null;

		private string _str_FPS = null;

		private string _str_Bones = null;

		private string _str_SelectAllVertices = null;
		private string _str_MoveVertices = null;

		//이건 GUI ID용 텍스트
		private string _str_GUIID_MeshName = null;
		private string _str_GUIID_MeshGroupName = null;
		private string _str_GUIID_SubTransformName = null;
		private string _str_GUIID_BoneName = null;
		private string _str_GUIID_AnimClipName = null;
		private string _str_GUIID_ControlParamName = null;
		private string _str_GUIID_NewPortrait = null;
		private string _str_GUIID_Rename = null;
		private string _str_GUIID_Search = null;

		//본 보여주기 상태
		private string _str_Show = null;
		private string _str_Outline = null;
		private string _str_Hide = null;

		//메시 탭 이름
		private string _str_Setting = null;
		private string _str_AddTool = null;
		private string _str_EditTool = null;
		private string _str_AutoTool = null;
		private string _str_Pivot = null;
		private string _str_Modify = null;
		private string _str_Pin = null;

		//로토스코핑 이미지..등
		private string _str_PrevImage = null;
		private string _str_NextImage = null;
		
		//특수 문자
		private string _str_Symbol_FilledCircle = null;
		private string _str_Symbol_EmptyCircle = null;

		//Size 중간의 x자
		private string _str_InterXofSize = null;

		//핀 모드
		private string _str_SelectPins = null;
		private string _str_AddPins = null;
		private string _str_LinkPins = null;
		private string _str_TestPins = null;
		private string _str_AddAndEditPins = null;

		private string _str_MorphTarget_Vertex = null;
		private string _str_MorphTarget_Pin = null;


		//툴팁용 StringWrapper (단축키때문에)
		private apStringWrapper _tooltipStr = null;

		// Init
		//-------------------------------------------
		public apStringFactory()
		{
			_isInitialized = false;
			s_instance = this;

			_tooltipStr = new apStringWrapper(128);
		}

		public void Init()
		{
			if (_isInitialized)
			{
				return;
			}

			_isInitialized = true;
			s_instance = this;

			//문자열을 여기서 만들자
			_str_None = "";
			_str_Space1 = " ";
			_str_Space2 = "  ";
			_str_Space3 = "   ";

			_str_UnityEditorIsPlaying = "Unity Editor is Playing.";
			_str_ResetZoomAndPosition = "Reset Zoom and Position";
			_str_ToggleWorkspaceSize = "Toggle Workspace Size";
			_str_ToggleWorkdspaceSize_HotKey = "Toogle Workspace Size";//단축키 추가되어야 함//(Alt+W)
			_str_SettingsOfTherPortraitAndEditor = "Settings of the Portrait and Editor";
			_str_BakeToScene = "Bake to Scene";

			_str_Ctrl = "Ctrl";
			_str_Command = "Command";
			_str_Alt = "Alt";
			_str_Option = "Option";
			_str_Shift = "Shift";

			_str_BoneVisibility = "Change Bone Visiblity";//변경 21.1.21

			_str_MeshVisibleToolTip = "Enable/Disable Mesh Visiblity";
			_str_PhysicFxEnableToolTip = "Enable/Disable Physics Effect";

			_str_FFDModeToolTip_1 = "Free Form Deformation / If you press the button while holding down [";
			_str_FFDModeToolTip_2 = "], a dialog appears allowing you to change the number of control points.";

			_str_ApplyFFD = "Apply FFD";
			_str_RevertFFD = "Revert FFD";

			_str_SoftSelectionToolTip_WithHotkey = "Soft Selection / Adjust brush size with ";
			_str_BlurToolTip_WithHotkey = "Blur / Adjust brush size with ";
			_str_SoftSelectionToolTip_WOHotkey = "Soft Selection";
			_str_BlurToolTip_WOHotkey = "Blur";

			_str_ToggleOnionSkin = "Show/Hide Onion Skin";
			_str_RecordOnionRecord = "Record Onion Skin";

			_str_Select = "Select";
			_str_Move = "Move";
			_str_Rotate = "Rotate";
			_str_Scale = "Scale";

			_str_IncreaseBrushSize = "Increase Brush Size";
			_str_DecreaseBrushSize = "Decrease Brush Size";
			_str_RemovePolygon = "Remove Polygon";

			_str_CreateANewPortrait = "Create a new Portrait";
			_str_SearchPortraitsAgainInTheScene = "Search Portraits again in the scene";
			_str_LoadBackupFileToolTip = "Open a Portrait saved as a backup file. It will be created as a new Portrait";

			_str_RootUnits = "Root Units";
			_str_Images = "Images";
			_str_Meshes = "Meshes";
			_str_MeshGroups = "Mesh Groups";
			_str_AnimationClips = "Animation Clips";
			_str_ControlParameters = "Control Parameters";

			_str_HierarchySortModeToolTip_RegOrder = "Show in order of registration";
			_str_HierarchySortModeToolTip_AlphaNum = "Show in order of name's alphanumeric";
			_str_HierarchySortModeToolTip_Custom = "Show in order of custom";
			_str_HierarchySortModeToolTip_Toggle = "Toggle Sort Mode";

			_str_ParamEdit = "Param Edit";

			_str_Keyframes = "Keyframes";
			_str_TimelineLayers = "Timeline Layers";
			_str_ClippedVertices = "Clipped Vertices";
			_str_ClippedMeshes = "Clipped Meshes";
			_str_Triangles = "Triangles";
			_str_Edges = "Edges";
			_str_Vertices = "Vertices";
			_str_Statistics = "Statistics";

			_str_AddSubMeshMeshGroup = "Add Sub Mesh / Mesh Group";
			_str_SetClippingMask = "Set Clipping Mask";
			_str_LayerUp = "Layer Up";
			_str_LayerDown = "Layer Down";
			_str_RemoveSubMeshMeshGroup = "Remove Sub Mesh / Mesh Group";

			_str_Bracket_1_L = "(";
			_str_Bracket_1_R = ")";
			_str_Bracket_2_L = "[";
			_str_Bracket_2_R = "]";
			_str_Bracket_3_L = "{";
			_str_Bracket_3_R = "}";
			_str_Bracket_4_L = "<";
			_str_Bracket_4_R = ">";

			_str_Colon = ":";
			_str_Colon_Space = " : ";
			_str_Dot1 = ".";
			_str_Dot2 = "..";
			_str_Dot3 = "...";
			_str_Plus = "+";
			_str_Minus = "-";
			_str_Slash = "/";
			_str_Slash_Space = " / ";
			_str_Comma = ",";
			_str_Comma_Space = ", ";
			_str_Return = "\n";

			_str_Gamma = "Gamma";
			_str_Linear = "Linear";

			_str_QuestionMark = "?";

			_str_SettingsOfMesh = "Settings of Mesh";
			_str_MakeVerticesAndPolygons = "Make Vertices and Polygons";
			_str_EditPivotOfMesh = "Edit Pivot of Mesh";
			_str_ModifyVertices = "Modify Vertices";
			_str_SettingsOfMeshGroup = "Settings of Mesh Group";
			_str_BonesOfMeshGroup = "Bones of Mesh Group";
			_str_ModifiersOfMeshGroup = "Modifiers of Mesh Group";
			_str_SettingsOfRootUnit = "Settings of Root Unit";
			_str_CapturingTheScreenShot = "Capturing the screenshot";

			_str_ErrorNoMeshGroupLinked = "Error! No MeshGroup Linked";

			_str_MakeAThumbnail = "Make a Thumbnail";
			_str_MakeAScreenshot = "Make a Screenshot";
			_str_MakeAGIFAnimation = "Make a GIF Animation";
			_str_MakeSpriteSheets = "Make Spritesheets";

			_str_X = "X";
			_str_Y = "Y";
			_str_Z = "Z";
			_str_Set = "Set";

			_str_L = "L";
			_str_T = "T";
			_str_R = "R";
			_str_B = "B";

			_str_GIF = "GIF";
			_str_XML = "XML";
			_str_JSON = "JSON";
			_str_TXT = "TXT";

			_str_NoImage = "(No Image)";
			_str_RemoveAllVerticesAndPolygons = "Remove all Vertices and Polygons";

			_str_Index_Colon = "Index : ";
			_str_DepthZeroToOne = " (0~1)";

			_str_UV = "UV";

			_str_VerticesSelected = "Vertices Selected";

			_str_Min = "Min";
			_str_Max = "Max";
			_str_Average = "Average";

			_str_Min_Colon = "Min : ";
			_str_Max_Colon = "Max : ";
			_str_Average_Colon = "Average : ";
			_str_SetMeshZDepthWeightTooltip = "Specify the Z value of the vertex. The larger the value, the more in front.";
			_str_NoVertexSelected = "No vertex selected";

			_str_MakePolygonsAndRefreshMesh = "Make Polygons and Refresh Mesh";
			_str_CreateAMeshManually = "Create a mesh manually";
			_str_SelectAndModifyVertices = "Select and modify vertices";
			_str_GenerateAMeshAutomatically = "Generate a mesh automatically";

			_str_MakeMeshTooltip_AddVertexLinkEdge = "Add Vertex / Link Edge";
			_str_MakeMeshTooltip_AddVertex = "Add Vertex";
			_str_MakeMeshTooltip_LinkEdge = "Link Edge";
			_str_MakeMeshTooltip_SelectPolygon = "Select Polygon";

			_str_RemoveVertices = "Remove Vertices";

			_str_EditDefaultTransformsOfSubMeshesMeshGroups = "Edit Default Transforms of Sub Meshes/MeshGroups";
			_str_MakeMeshGroupAsRootUnit = "Make Mesh Group as Root Unit";
			_str_EditBones = "Edit Bones";

			_str_NotEditable = "Not Editable";

			_str_HowToControl_None = "None";
			_str_HowToControl_MoveView = "Move View";

			_str_AddANewModifier = "Add a New Modifier";
			_str_BindingModeToggleTooltip = "Enable/Disable Bind Mode";
			_str_EditModeToggleTooltip = "Enable/Disable Edit Mode";
			_str_SelectionLockToggleTooltip = "Selection Lock/Unlock";

//			_str_ModifierLockToggleTooltip_1 = "Modifier Lock/Unlock";
//#if UNITY_EDITOR_OSX
//			_str_ModifierLockToggleTooltip_2 = " / If you press the button while holding down [Command], the Setting dialog will be opened";
//#else		
//			_str_ModifierLockToggleTooltip_2 = " / If you press the button while holding down [Ctrl], the Setting dialog will be opened";
//#endif
			
			//_str_ToggleEditingMode = "Toggle Editing Mode";
			//_str_ToggleSelectionLock = "Toggle Selection Lock";
			//_str_ToggleModifierLock = "Toggle Modifier Lock";
			//_str_ToggleLayerLock = "Toggle Layer Lock";

			_str_RiggingViewModeTooltip_ColorWithTexture = "Whether to render the Rigging weight with the texture of the image";
			_str_RiggingViewModeTooltip_BoneColor = "Whether to render the Rigging weight by the color of the Bone";
			_str_RiggingViewModeTooltip_CircleVert = "Whether to render vertices into circular shapes";
			_str_RiggingViewModeTooltip_NoLinkedBoneVisibility = "How to display bones that are not connected to the mesh";
			_str_RiggingViewModeTooltip_TestPose = "Enable/Disable Pose Test Mode";

			_str_SimulateWindForce = "Simulate wind forces";
			_str_ClearWindForce = "Clear wind force";

			_str_AnimationEditModeToggleTooltip = "Animation Edit Mode";

//			_str_TimelineLayerLockToggleTooltip_1 = "Timeline Layer Lock/Unlock";
//#if UNITY_EDITOR_OSX
//			_str_TimelineLayerLockToggleTooltip_2 = " / If you press the button while holding down [Command], the Setting dialog will be opened";
//#else
//			_str_TimelineLayerLockToggleTooltip_2 = " / If you press the button while holding down [Ctrl], the Setting dialog will be opened";
//#endif


			_str_AddKeyframe = "Add Keyframe";
			_str_RemoveKeyframe = "Remove Keyframe";
			_str_RemoveKeyframes = "Remove Keyframes";
			//_str_AddNewKeyframe = "Add New Keyframe";

			_str_PlayPause = "Play/Pause";
			_str_PreviousFrame = "Previous Frame";
			_str_NextFrame = "Next Frame";
			_str_FirstFrame = "First Frame";
			_str_LastFrame = "Last Frame";
			_str_CopyKeyframes = "Copy Keyframes";
			_str_PasteKeyframes = "Paste Keyframes";

			_str_ToggleAnimLoop = "Enable/Disable Loop";
			_str_AnimTimelineSort_RegOrder = "Sort by registeration order";
			_str_AnimTImelineSort_Name = "Sort by name";
			_str_AnimTimelineSort_Depth = "Sort by Depth";

			_str_AnimTimelineSize_Small = "Timeline UI Size [Small]";
			_str_AnimTimelineSize_Medium = "Timeline UI Size [Medium]";
			_str_AnimTimelineSize_Large = "Timeline UI Size [Large]";

			_str_AnimTimelineFit = " Fit";
			_str_AnimTimelineFitTooltip = "Zoom to fit the animation length";
			_str_AnimTimelineAutoScrollTooltip = "Scrolls automatically according to the frame of the animation";
			_str_AnimTimelineAutoKeyTooltip = "When you move the object, keyframes are automatically created";

			_str_AnimCurveTooltip_Linear = "Linear Curve";
			_str_AnimCurveTooltip_Smooth = "Smooth Curve";
			_str_AnimCurveTooltip_Constant = "Constant Curve";

			_str_ON = "ON";
			_str_OFF = "OFF";

			_str_NoMaskParent = "<No Mask Parent>";

			_str_IKSingle = "IK Single";
			_str_IKHead = "IK Head";
			_str_IKChain = "IK Chain";
			_str_Disabled = "Disabled";

			_str_EmptyClipboard = "<Empty Clipboard>";

			_str_NoVertex = "No Vertex";
			_str_Vertex = "Vertex";
			_str_VertexWithBracket = "Vertex [";
			_str_VerticesWithSpace = " Vertices";
			_str_NoBone = "No Bone";

			_str_Weight_00 = "0";
			_str_Weight_01 = ".1";
			_str_Weight_03 = ".3";
			_str_Weight_05 = ".5";
			_str_Weight_07 = ".7";
			_str_Weight_09 = ".9";
			_str_Weight_10 = "1";

			_str_IncreaseBrushRadius = "Increase Brush Radius";
			_str_DecreaseBrushRadius = "Decrease Brush Radius";
			_str_BrushMode_Add = "Brush Mode - Add";
			_str_BrushMode_Multiply = "Brush Mode - Multiply";
			_str_BrushMode_Blur = "Brush Mode - Blur";
			_str_IncreaseBrushIntensity = "Increase Brush Intensity";
			_str_DecreaseBrushIntensity = "Decrease Brush Intensity";

			_str_RiggingTooltip_Blend = "Blend the weights of vertices";
			_str_RiggingTooltip_Normalize = "Normalize rigging weights";
			_str_RiggingTooltip_Prune = "Remove rigging bones its weight is under 0.01";
			_str_RiggingTooltip_AutoRig = "Rig Automatically";
			_str_RiggingTooltip_Grow = "Select more of the surrounding vertices";
			_str_RiggingTooltip_Shrink = "Reduce selected vertices";
#if UNITY_EDITOR_OSX
			_str_RiggingTooltip_SelectVerticesOfBone = "Select vertices connected to the current bone. Hold down [Command] key and press the button to select with existing vertices.";
#else
			_str_RiggingTooltip_SelectVerticesOfBone = "Select vertices connected to the current bone. Hold down [Ctrl] key and press the button to select with existing vertices.";
#endif

			_str_Float = "Float";
			_str_Integer = "Integer";
			_str_Vector = "Vector";
			_str_Texture = "Texture";
			_str_Color = "Color";

			_str_Num0 = "0";
			_str_Num1 = "1";
			_str_Num2 = "2";
			_str_Num3 = "3";
			_str_Num4 = "4";
			_str_Num5 = "5";
			_str_Num6 = "6";
			_str_Num7 = "7";
			_str_Num8 = "8";
			_str_Num9 = "9";

			_str_FPS = "FPS";

			_str_Bones = "Bones";

			_str_SelectAllVertices = "Select All Vertices";
			_str_MoveVertices = "Move Vertices";

			//GUI용 ID
			_str_GUIID_MeshName = "ANYPORTRAIT_GUI_MESH_NAME";
			_str_GUIID_MeshGroupName = "ANYPORTRAIT_GUI_MESHGROUP_NAME";
			_str_GUIID_SubTransformName = "ANYPORTRAIT_GUI_SUBTRANSFORM_NAME";
			_str_GUIID_BoneName = "ANYPORTRAIT_GUI_BONE_NAME";
			_str_GUIID_AnimClipName = "ANYPORTRAIT_GUI_ANIMCLIP_NAME";
			_str_GUIID_ControlParamName = "ANYPORTRAIT_GUI_CONTROLPARAM_NAME";
			_str_GUIID_NewPortrait = "ANYPORTRAIT_GUI_NEWPORTRAIT_NAME";
			_str_GUIID_Rename = "ANYPORTRAIT_GUI_OBJECT_RENAME";
			_str_GUIID_Search = "ANYPORTRAIT_GUI_SEARCH_WORD";

			_str_Show = "Show";
			_str_Outline = "Outline";
			_str_Hide = "Hide";

			_str_Setting = "Setting";
			_str_AddTool = "Add Tool";
			_str_EditTool = "Edit Tool";
			_str_AutoTool = "Auto Tool";
			_str_Pivot = "Pivot";
			_str_Modify = "Modify";
			_str_Pin = "Pin";

			_str_PrevImage = "Prev Image";
			_str_NextImage = "Next Image";

			_str_Symbol_FilledCircle = "●";
			_str_Symbol_EmptyCircle = "○";

			_str_InterXofSize = " x ";

			_str_SelectPins = "Select Pins";
			_str_AddPins = "Add Pins";
			_str_LinkPins = "Link Pins";
			_str_TestPins = "Test Pins";
			_str_AddAndEditPins = "Add and Edit Pins";

			_str_MorphTarget_Vertex = "Vertex";
			_str_MorphTarget_Pin = "Pin";
		}




		// Get
		//-------------------------------------------
		public static apStringFactory I { get { return s_instance; } }
		public bool IsInitialize() { return _isInitialized; }


		//문자열들
		public string None { get { return _str_None; } }
		public string Space1 { get { return _str_Space1; } }
		public string Space2 { get { return _str_Space2; } }
		public string Space3 { get { return _str_Space3; } }

		public string UnityEditorIsPlaying { get { return _str_UnityEditorIsPlaying; } }
		public string ResetZoomAndPositon { get { return _str_ResetZoomAndPosition; } }
		public string ToggleWorkspaceSize { get { return _str_ToggleWorkspaceSize; } }

		//툴팁
		public string GetHotkeyTooltip_ToggleWorkspaceSize(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_ToggleWorkdspaceSize_HotKey, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleWorkspaceSize, _tooltipStr, true);
			return _tooltipStr.ToString();
		}
		public string SettingsOfTherPortraitAndEditor { get { return _str_SettingsOfTherPortraitAndEditor; } }
		public string BakeToScene { get { return _str_BakeToScene; } }

		public string GetHotkeyTooltip_BoneVisibility(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_BoneVisibility, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleBoneVisibility, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string GetHotkeyTooltip_MeshVisibility(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_MeshVisibleToolTip, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleMeshVisibility, _tooltipStr, true);
			return _tooltipStr.ToString();
		}
		public string GetHotkeyTooltip_PhysicsFxEnable(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_PhysicFxEnableToolTip, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.TogglePhysicsPreview, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string FFDModeToolTip_1 { get { return _str_FFDModeToolTip_1; } }
		public string FFDModeToolTip_2 { get { return _str_FFDModeToolTip_2; } }
		public string ApplyFFD { get { return _str_ApplyFFD; } }
		public string RevertFFD { get { return _str_RevertFFD; } }
		public string GetHotkeyTooltip_SoftSelectionToolTip(apHotKeyMapping hotkeyMapping)
		{
			if (hotkeyMapping.IsHotkeyAvailable(apHotKeyMapping.KEY_TYPE.IncreaseModToolBrushSize)
				&& hotkeyMapping.IsHotkeyAvailable(apHotKeyMapping.KEY_TYPE.DecreaseModToolBrushSize))
			{
				//브러시 크기가 모두 유효한 경우
				_tooltipStr.Clear();
				_tooltipStr.Append(_str_SoftSelectionToolTip_WithHotkey, false);
				hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.DecreaseModToolBrushSize, _tooltipStr, false);
				_tooltipStr.Append(apStringFactory.I.Comma_Space, false);
				hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.IncreaseModToolBrushSize, _tooltipStr, false);
				return _tooltipStr.ToString();
			}
			return _str_SoftSelectionToolTip_WOHotkey;
		}
		public string GetHotkeyTooltip_BlurToolTip(apHotKeyMapping hotkeyMapping)
		{
			if (hotkeyMapping.IsHotkeyAvailable(apHotKeyMapping.KEY_TYPE.IncreaseModToolBrushSize)
				&& hotkeyMapping.IsHotkeyAvailable(apHotKeyMapping.KEY_TYPE.DecreaseModToolBrushSize))
			{
				//브러시 크기가 모두 유효한 경우
				_tooltipStr.Clear();
				_tooltipStr.Append(_str_BlurToolTip_WithHotkey, false);
				hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.DecreaseModToolBrushSize, _tooltipStr, false);
				_tooltipStr.Append(apStringFactory.I.Comma_Space, false);
				hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.IncreaseModToolBrushSize, _tooltipStr, false);
				return _tooltipStr.ToString();
			}
			return _str_BlurToolTip_WOHotkey;
		}

		public string GetHotkeyTooltip_ToggleOnionSkin(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_ToggleOnionSkin, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleOnionSkin, _tooltipStr, true);
			return _tooltipStr.ToString();
		}
		public string RecordOnionSkin { get { return _str_RecordOnionRecord; } } 

		public string IncreaseBrushSize { get { return _str_IncreaseBrushSize; } }
		public string DecreaseBrushSize { get { return _str_DecreaseBrushSize; } }
		public string RemovePolygon { get { return _str_RemovePolygon; } }

		public string CreateANewPortrait { get { return _str_CreateANewPortrait; } }
		public string SearchPortraitsAgainInTheScene { get { return _str_SearchPortraitsAgainInTheScene; } }
		public string LoadBackupFileToolTip { get { return _str_LoadBackupFileToolTip; } }

		public string Select { get { return _str_Select; } }
		public string Move { get { return _str_Move; } }
		public string Rotate { get { return _str_Rotate; } }
		public string Scale { get { return _str_Scale; } }

		public string GetHotkeyTooltip_SelectTool(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_Select, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.Gizmo_Select, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string GetHotkeyTooltip_MoveTool(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_Move, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.Gizmo_Move, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string GetHotkeyTooltip_RotateTool(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_Rotate, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.Gizmo_Rotate, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string GetHotkeyTooltip_ScaleTool(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_Scale, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.Gizmo_Scale, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string Ctrl { get { return _str_Ctrl; } }
		public string Command { get { return _str_Command; } }
		public string Alt { get { return _str_Alt; } }
		public string Option { get { return _str_Option; } }
		public string Shift { get { return _str_Shift; } }

		/// <summary>
		/// Windows에서는 Ctrl, OSX에서는 Command를 리턴하는 함수
		/// </summary>
		/// <returns></returns>
		public string GetCtrlOrCommand()
		{
#if UNITY_EDITOR_OSX
			return _str_Command;
#else
			return _str_Ctrl;
#endif
		}

		/// <summary>
		/// Windows에서는 Alt, OSX에서는 Option을 리턴하는 함수
		/// </summary>
		/// <returns></returns>
		public string GetAltOrOption()
		{
#if UNITY_EDITOR_OSX
			return _str_Option;
#else
			return _str_Alt;
#endif
		}

		public string RootUnits { get { return _str_RootUnits; } }
		public string Images { get { return _str_Images; } }
		public string Meshes { get { return _str_Meshes; } }
		public string MeshGroups { get { return _str_MeshGroups; } }
		public string AnimationClips { get { return _str_AnimationClips; } }
		public string ControlParameters { get { return _str_ControlParameters; } }

		public string HierarchySortModeToolTip_RegOrder { get { return _str_HierarchySortModeToolTip_RegOrder; } }
		public string HierarchySortModeToolTip_AlphaNum { get { return _str_HierarchySortModeToolTip_AlphaNum; } }
		public string HierarchySortModeToolTip_Custom { get { return _str_HierarchySortModeToolTip_Custom; } }
		public string HierarchySortModeToolTip_Toggle { get { return _str_HierarchySortModeToolTip_Toggle; } }

		public string ParamEdit { get { return _str_ParamEdit; } }

		public string Keyframes { get { return _str_Keyframes; } }
		public string TimelineLayers { get { return _str_TimelineLayers; } }
		public string ClippedVertices { get { return _str_ClippedVertices; } }
		public string ClippedMeshes { get { return _str_ClippedMeshes; } }
		public string Triangles { get { return _str_Triangles; } }
		public string Edges { get { return _str_Edges; } }
		public string Vertices { get { return _str_Vertices; } }
		public string Statistics { get { return _str_Statistics; } }

		public string AddSubMeshMeshGroup { get { return _str_AddSubMeshMeshGroup; } }
		public string SetClippingMask { get { return _str_SetClippingMask; } }
		public string LayerUp { get { return _str_LayerUp; } }
		public string LayerDown { get { return _str_LayerDown; } }
		public string RemoveSubMeshMeshGroup { get { return _str_RemoveSubMeshMeshGroup; } }

		/// <summary> ( </summary>
		public string Bracket_1_L { get { return _str_Bracket_1_L; } }
		/// <summary> ) </summary>
		public string Bracket_1_R { get { return _str_Bracket_1_R; } }
		/// <summary> [ </summary>
		public string Bracket_2_L { get { return _str_Bracket_2_L; } }
		/// <summary> ] </summary>
		public string Bracket_2_R { get { return _str_Bracket_2_R; } }
		/// <summary> { </summary>
		public string Bracket_3_L { get { return _str_Bracket_3_L; } }
		/// <summary> } </summary>
		public string Bracket_3_R { get { return _str_Bracket_3_R; } }
		/// <summary> < </summary>
		public string Bracket_4_L { get { return _str_Bracket_4_L; } }
		/// <summary> > </summary>
		public string Bracket_4_R { get { return _str_Bracket_4_R; } }
		/// <summary> : </summary>
		public string Colon { get { return _str_Colon; } }
		/// <summary> " : " </summary>
		public string Colon_Space { get { return _str_Colon_Space; } }
		/// <summary> . </summary>
		public string Dot1 { get { return _str_Dot1; } }
		/// <summary> .. </summary>
		public string Dot2 { get { return _str_Dot2; } }
		/// <summary> ... </summary>
		public string Dot3 { get { return _str_Dot3; } }
		/// <summary> + </summary>
		public string Plus { get { return _str_Plus; } }
		/// <summary> - </summary>
		public string Minus { get { return _str_Minus; } }
		/// <summary> / </summary>
		public string Slash { get { return _str_Slash; } }
		/// <summary> " / " </summary>
		public string Slash_Space { get { return _str_Slash_Space; } }
		/// <summary> , </summary>
		public string Comma { get { return _str_Comma; } }
		/// <summary> ", " </summary>
		public string Comma_Space { get { return _str_Comma_Space; } }
		
		public string Return { get { return _str_Return; } }


		public string Gamma { get { return _str_Gamma; } }
		public string Linear { get { return _str_Linear; } }

		/// <summary> ? </summary>
		public string QuestionMark { get { return _str_QuestionMark; } }

		public string SettingsOfMesh { get { return _str_SettingsOfMesh; } }
		public string MakeVerticesAndPolygons { get { return _str_MakeVerticesAndPolygons; } }
		public string EditPivotOfMesh { get { return _str_EditPivotOfMesh; } }
		public string ModifyVertices { get { return _str_ModifyVertices; } }
		public string SettingsOfMeshGroup { get { return _str_SettingsOfMeshGroup; } }
		public string BonesOfMeshGroup { get { return _str_BonesOfMeshGroup; } }
		public string ModifiersOfMeshGroup { get { return _str_ModifiersOfMeshGroup; } }
		public string SettingsOfRootUnit { get { return _str_SettingsOfRootUnit; } }
		public string CapturingTheScreenShot { get { return _str_CapturingTheScreenShot; } }

		public string ErrorNoMeshGroupLinked { get { return _str_ErrorNoMeshGroupLinked; } }
		public string MakeAThumbnail { get { return _str_MakeAThumbnail; } }
		public string MakeAScreenshot { get { return _str_MakeAScreenshot; } }
		public string MakeAGIFAnimation { get { return _str_MakeAGIFAnimation; } }
		public string MakeSpriteSheets { get { return _str_MakeSpriteSheets; } }

		public string X { get { return _str_X; } }
		public string Y { get { return _str_Y; } }
		public string Z { get { return _str_Z; } }
		public string Set { get { return _str_Set; } }

		public string L { get { return _str_L; } }
		public string T { get { return _str_T; } }
		public string R { get { return _str_R; } }
		public string B { get { return _str_B; } }

		public string GIF { get { return _str_GIF; } }
		public string XML { get { return _str_XML; } }
		public string JSON { get { return _str_JSON; } }
		public string TXT { get { return _str_TXT; } }

		public string NoImage { get { return _str_NoImage; } }
		public string RemoveAllVerticesAndPolygons { get { return _str_RemoveAllVerticesAndPolygons; } }

		public string Index_Colon { get { return _str_Index_Colon; } }
		public string DepthZeroToOne { get { return _str_DepthZeroToOne; } }

		public string UV { get { return _str_UV; } }
		public string VerticesSelected { get { return _str_VerticesSelected; } }

		public string Min { get { return _str_Min; } }
		public string Max { get { return _str_Max; } }
		public string Average { get { return _str_Average; } }

		public string Min_Colon { get { return _str_Min_Colon; } }
		public string Max_Colon { get { return _str_Max_Colon; } }
		public string Average_Colon { get { return _str_Average_Colon; } }

		public string SetMeshZDepthWeightTooltip { get { return _str_SetMeshZDepthWeightTooltip; } }
		public string NoVertexSelected { get { return _str_NoVertexSelected; } }

		public string MakePolygonsAndRefreshMesh { get { return _str_MakePolygonsAndRefreshMesh; } }
		public string CreateAMeshManually { get { return _str_CreateAMeshManually; } }
		public string SelectAndModifyVertices { get { return _str_SelectAndModifyVertices; } }
		public string GenerateAMeshAutomatically { get { return _str_GenerateAMeshAutomatically; } }

		public string MakeMeshTooltip_AddVertexLinkEdge { get { return _str_MakeMeshTooltip_AddVertexLinkEdge; } }
		public string MakeMeshTooltip_AddVertex { get { return _str_MakeMeshTooltip_AddVertex; } }
		public string MakeMeshTooltip_LinkEdge { get { return _str_MakeMeshTooltip_LinkEdge; } }
		public string MakeMeshTooltip_SelectPolygon { get { return _str_MakeMeshTooltip_SelectPolygon; } }

		public string RemoveVertices { get { return _str_RemoveVertices; } }

		public string EditDefaultTransformsOfSubMeshesMeshGroups { get { return _str_EditDefaultTransformsOfSubMeshesMeshGroups; } }
		public string MakeMeshGroupAsRootUnit { get { return _str_MakeMeshGroupAsRootUnit; } }
		public string EditBones { get { return _str_EditBones; } }
		public string NotEditable { get { return _str_NotEditable; } }

		public string HowToControl_None { get { return _str_HowToControl_None; } }
		public string HowToControl_MoveView { get { return _str_HowToControl_MoveView; } }

		public string AddANewModifier { get { return _str_AddANewModifier; } }

		public string GetHotkeyTooltip_BindingModeToggle(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_BindingModeToggleTooltip, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleEditingMode, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string GetHotkeyTooltip_EditModeToggle(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_EditModeToggleTooltip, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleEditingMode, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string GetHotkeyTooltip_SelectionLockToggle(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_SelectionLockToggleTooltip, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleSelectionLock, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		//삭제 21.2.13 : 모디파이어 잠금을 사용하지 않는다.
		//public string GetHotkeyTooltip_ModifierLockToggle(apHotKeyMapping hotkeyMapping)
		//{
		//	_tooltipStr.Clear();
		//	_tooltipStr.Append(_str_ModifierLockToggleTooltip_1, false);
		//	hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleModifierLock, _tooltipStr, true);
		//	_tooltipStr.Append(_str_ModifierLockToggleTooltip_2, true);
		//	return _tooltipStr.ToString();
		//}

		//public string ToggleEditingMode { get { return _str_ToggleEditingMode; } }
		//public string ToggleSelectionLock { get { return _str_ToggleSelectionLock; } }
		//public string ToggleModifierLock { get { return _str_ToggleModifierLock; } }
		//public string ToggleLayerLock { get { return _str_ToggleLayerLock; } }

		public string RiggingViewModeTooltip_ColorWithTexture { get { return _str_RiggingViewModeTooltip_ColorWithTexture; } }
		public string RiggingViewModeTooltip_BoneColor { get { return _str_RiggingViewModeTooltip_BoneColor; } }
		public string RiggingViewModeTooltip_CircleVert { get { return _str_RiggingViewModeTooltip_CircleVert; } }
		public string RiggingViewModeTooltip_NoLinkedBoneVisibility { get { return _str_RiggingViewModeTooltip_NoLinkedBoneVisibility; } }
		
		public string RiggingViewModeTooltip_TestPose { get { return _str_RiggingViewModeTooltip_TestPose; } }

		public string SimulateWindForce { get { return _str_SimulateWindForce; } }
		public string ClearWindForce { get { return _str_ClearWindForce; } }

		public string GetHotkeyTooltip_AnimationEditModeToggle(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_AnimationEditModeToggleTooltip, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleEditingMode, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		//삭제 21.2.13 : 모디파이어 잠금을 사용하지 않는다.
		//public string GetHotkeyTooltip_TimelineLayerLockToggle(apHotKeyMapping hotkeyMapping)
		//{
		//	_tooltipStr.Clear();
		//	_tooltipStr.Append(_str_TimelineLayerLockToggleTooltip_1, false);
		//	hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.ToggleModifierLock, _tooltipStr, true);
		//	_tooltipStr.Append(_str_TimelineLayerLockToggleTooltip_2, true);
		//	return _tooltipStr.ToString();
		//}


		public string AddKeyframe { get { return _str_AddKeyframe; } }
		public string GetHotkeyTooltip_AddKeyframe(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_AddKeyframe, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.Anim_AddKeyframes, _tooltipStr, true);
			return _tooltipStr.ToString();
		}
		public string RemoveKeyframe { get { return _str_RemoveKeyframe; } }
		public string RemoveKeyframes { get { return _str_RemoveKeyframes; } }
		//public string AddNewKeyframe { get { return _str_AddNewKeyframe; } }

		public string PlayPause { get { return _str_PlayPause; } }
		public string PreviousFrame { get { return _str_PreviousFrame; } }
		public string NextFrame { get { return _str_NextFrame; } }
		public string FirstFrame { get { return _str_FirstFrame; } }
		public string LastFrame { get { return _str_LastFrame; } }
		public string CopyKeyframes { get { return _str_CopyKeyframes; } }
		public string PasteKeyframes { get { return _str_PasteKeyframes; } }
		public string ToggleAnimLoop { get { return _str_ToggleAnimLoop; } }

		public string AnimTimelineSort_RegOrder { get { return _str_AnimTimelineSort_RegOrder; } }
		public string AnimTImelineSort_Name { get { return _str_AnimTImelineSort_Name; } }
		public string AnimTimelineSort_Depth { get { return _str_AnimTimelineSort_Depth; } }

		public string AnimTimelineSize_Small { get { return _str_AnimTimelineSize_Small; } }
		public string AnimTimelineSize_Medium { get { return _str_AnimTimelineSize_Medium; } }
		public string AnimTimelineSize_Large { get { return _str_AnimTimelineSize_Large; } }

		public string AnimTimelineFit { get { return _str_AnimTimelineFit; } }
		public string AnimTimelineFitTooltip { get { return _str_AnimTimelineFitTooltip; } }
		public string AnimTimelineAutoScrollTooltip { get { return _str_AnimTimelineAutoScrollTooltip; } }
		public string GetHotkeyTooltip_AnimTimelineAutoKeyTooltip(apHotKeyMapping hotkeyMapping)
		{
			_tooltipStr.Clear();
			_tooltipStr.Append(_str_AnimTimelineAutoKeyTooltip, false);
			hotkeyMapping.AddHotkeyTextToWrapper(apHotKeyMapping.KEY_TYPE.Anim_ToggleAutoKey, _tooltipStr, true);
			return _tooltipStr.ToString();
		}

		public string AnimCurveTooltip_Linear { get { return _str_AnimCurveTooltip_Linear; } }
		public string AnimCurveTooltip_Smooth { get { return _str_AnimCurveTooltip_Smooth; } }
		public string AnimCurveTooltip_Constant { get { return _str_AnimCurveTooltip_Constant; } }

		public string ON { get { return _str_ON; } }
		public string OFF { get { return _str_OFF; } }

		public string NoMaskParent { get { return _str_NoMaskParent; } }

		public string IKSingle { get { return _str_IKSingle; } }
		public string IKHead { get { return _str_IKHead; } }
		public string IKChain { get { return _str_IKChain; } }
		public string Disabled { get { return _str_Disabled; } }

		public string EmptyClipboard { get { return _str_EmptyClipboard; } }

		public string NoVertex { get { return _str_NoVertex; } }
		public string Vertex { get { return _str_Vertex; } }
		public string VertexWithBracket { get { return _str_VertexWithBracket; } }
		public string VerticesWithSpace { get { return _str_VerticesWithSpace; } }
		public string NoBone { get { return _str_NoBone; } }

		public string Weight_00 { get { return _str_Weight_00; } }
		public string Weight_01 { get { return _str_Weight_01; } }
		public string Weight_03 { get { return _str_Weight_03; } }
		public string Weight_05 { get { return _str_Weight_05; } }
		public string Weight_07 { get { return _str_Weight_07; } }
		public string Weight_09 { get { return _str_Weight_09; } }
		public string Weight_10 { get { return _str_Weight_10; } }

		public string IncreaseBrushRadius { get { return _str_IncreaseBrushRadius; } }
		public string DecreaseBrushRadius { get { return _str_DecreaseBrushRadius; } }
		public string BrushMode_Add { get { return _str_BrushMode_Add; } }
		public string BrushMode_Multiply { get { return _str_BrushMode_Multiply; } }
		public string BrushMode_Blur { get { return _str_BrushMode_Blur; } }
		public string IncreaseBrushIntensity { get { return _str_IncreaseBrushIntensity; } }
		public string DecreaseBrushIntensity { get { return _str_DecreaseBrushIntensity; } }

		public string RiggingTooltip_Blend				{ get { return _str_RiggingTooltip_Blend; } }
		public string RiggingTooltip_Normalize			{ get { return _str_RiggingTooltip_Normalize; } }
		public string RiggingTooltip_Prune				{ get { return _str_RiggingTooltip_Prune; } }
		public string RiggingTooltip_AutoRig			{ get { return _str_RiggingTooltip_AutoRig; } }
		public string RiggingTooltip_Grow				{ get { return _str_RiggingTooltip_Grow; } }
		public string RiggingTooltip_Shrink				{ get { return _str_RiggingTooltip_Shrink; } }
		public string RiggingTooltip_SelectVerticesOfBone { get { return _str_RiggingTooltip_SelectVerticesOfBone; } }

		public string Float			{ get { return _str_Float; } }
		public string Integer		{ get { return _str_Integer; } }
		public string Vector		{ get { return _str_Vector; } }
		public string Texture		{ get { return _str_Texture; } }
		public string Color			{ get { return _str_Color; } }

		public string Num0 { get { return _str_Num0; } }
		public string Num1 { get { return _str_Num1; } }
		public string Num2 { get { return _str_Num2; } }
		public string Num3 { get { return _str_Num3; } }
		public string Num4 { get { return _str_Num4; } }
		public string Num5 { get { return _str_Num5; } }
		public string Num6 { get { return _str_Num6; } }
		public string Num7 { get { return _str_Num7; } }
		public string Num8 { get { return _str_Num8; } }
		public string Num9 { get { return _str_Num9; } }

		public string FPS { get { return _str_FPS; } }

		public string Bones { get { return _str_Bones; } }

		public string SelectAllVertices { get { return _str_SelectAllVertices; } }
		public string MoveVertices { get { return _str_MoveVertices; } }


		//GUI용 ID
		public string GUI_ID__MeshName			{ get { return _str_GUIID_MeshName; } }
		public string GUI_ID__MeshGroupName		{ get { return _str_GUIID_MeshGroupName; } }
		public string GUI_ID__SubTransformName	{ get { return _str_GUIID_SubTransformName; } }
		public string GUI_ID__BoneName			{ get { return _str_GUIID_BoneName; } }
		public string GUI_ID__AnimClipName		{ get { return _str_GUIID_AnimClipName; } }
		public string GUI_ID__ControlParamName	{ get { return _str_GUIID_ControlParamName; } }
		public string GUI_ID__NewPortraitName	{ get { return _str_GUIID_NewPortrait; } }
		public string GUI_ID__Rename			{ get { return _str_GUIID_Rename; } }
		public string GUI_ID__SearchWord		{ get { return _str_GUIID_Search; } }

		//본 보여주기
		public string Show { get { return _str_Show; } }
		public string Outline { get { return _str_Outline; } }
		public string Hide { get { return _str_Hide; } }

		//메시 탭
		public string Setting { get { return _str_Setting; } }
		public string AddTool { get { return _str_AddTool; } }
		public string EditTool { get { return _str_EditTool; } }
		public string AutoTool { get { return _str_AutoTool; } }
		public string Pivot { get { return _str_Pivot; } }
		public string Modify { get { return _str_Modify; } }
		public string Pin { get { return _str_Pin; } }

		//로토스코핑
		public string PrevImage { get { return _str_PrevImage; } }
		public string NextImage { get { return _str_NextImage; } }

		//특수 문자
		/// <summary>특수 문자 : ●</summary>
		public string Symbol_FilledCircle { get { return _str_Symbol_FilledCircle; } }
		/// <summary>특수 문자 : ○</summary>
		public string Symbol_EmptyCircle { get { return _str_Symbol_EmptyCircle; } }


		//Width, Height 사이의 글자
		/// <summary>
		/// Width_x_Height
		/// </summary>
		public string InterXofSize { get { return _str_InterXofSize; } }

		public string SelectPins { get { return _str_SelectPins; } }
		public string AddPins { get { return _str_AddPins; } }
		public string LinkPins { get { return _str_LinkPins; } }
		public string TestPins { get { return _str_TestPins; } }
		public string AddAndEditPins { get { return _str_AddAndEditPins; } }
		public string MorphTarget_Vertex	{ get { return _str_MorphTarget_Vertex; } }
		public string MorphTarget_Pin		{ get { return _str_MorphTarget_Pin; } }
	}
}