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



	/// <summary>
	/// Unity의 Undo 기능을 사용할 때, 불필요한 호출을 막는 용도
	/// "연속된 동일한 요청"을 방지한다.
	/// 중복 체크만 하는 것이므로 1개의 값만 가진다.
	/// </summary>
	public class apUndoGroupData
	{
		// Singletone
		//---------------------------------------------------
		private static apUndoGroupData _instance = new apUndoGroupData();
		public static apUndoGroupData I { get { return _instance; } }

		// Members
		//--------------------------------------------------

		private ACTION _action = ACTION.None;
		[Flags]
		public enum SAVE_TARGET : int
		{
			None = 0,
			Portrait = 1,
			Mesh = 2,
			MeshGroup = 4,
			AllMeshGroups = 8,
			Modifier = 16,
			AllModifiers = 32,
			AllMeshes = 64
		}
		private SAVE_TARGET _saveTarget = SAVE_TARGET.None;
		private apPortrait _portrait = null;
		private apMesh _mesh = null;
		private apMeshGroup _meshGroup = null;
		private apModifierBase _modifier = null;


		//private object _keyObject = null;//키 오브젝트는 삭제. 사용하는 일이 거의 없다.
		private bool _isCallContinuous = false;//여러 항목을 동시에 처리하는 Batch 액션 중인가

		private DateTime _lastUndoTime = new DateTime();
		private bool _isFirstAction = true;

		private const float CONT_SAVE_TIME = 5.0f;//이전 : 1초마다 Cont 작업을 분절해서 Undo > 5초로 변경 (21.7.17)

		public enum ACTION
		{
			None,
			Main_AddImage,
			Main_RemoveImage,
			Main_AddMesh,
			Main_RemoveMesh,
			Main_AddMeshGroup,
			Main_RemoveMeshGroup,
			Main_AddAnimation,
			Main_RemoveAnimation,
			Main_AddParam,
			Main_RemoveParam,

			Portrait_SettingChanged,
			Portrait_BakeOptionChanged,
			Portrait_SetMeshGroup,
			Portrait_ReleaseMeshGroup,



			Image_SettingChanged,

			Image_PSDImport,

			MeshEdit_AddVertex,
			MeshEdit_EditVertex,
			MeshEdit_EditVertexDepth,
			MeshEdit_RemoveVertex,
			MeshEdit_ResetVertices,
			MeshEdit_RemoveAllVertices,
			MeshEdit_AddEdge,
			MeshEdit_EditEdge,
			MeshEdit_RemoveEdge,
			MeshEdit_MakeEdges,
			MeshEdit_EditPolygons,
			MeshEdit_SetImage,
			MeshEdit_SetPivot,
			MeshEdit_SettingChanged,
			MeshEdit_AtlasChanged,
			MeshEdit_AutoGen,
			MeshEdit_VertexCopied,
			MeshEdit_VertexMoved,
			MeshEdit_FFDStart,
			MeshEdit_FFDAdapt,
			MeshEdit_FFDRevert,

			MeshEdit_AddPin,
			MeshEdit_MovePin,
			MeshEdit_MovePin_Rotate,
			MeshEdit_MovePin_Scale,
			MeshEdit_ChangePin,
			MeshEdit_CalculatePinWeight,
			MeshEdit_RemovePin,

			MeshGroup_AttachMesh,
			MeshGroup_AttachMeshGroup,
			MeshGroup_DetachMesh,
			MeshGroup_DetachMeshGroup,
			MeshGroup_ClippingChanged,
			MeshGroup_DepthChanged,
			MeshGroup_AddBone,
			MeshGroup_RemoveBone,
			MeshGroup_RemoveAllBones,
			MeshGroup_BoneSettingChanged,
			MeshGroup_BoneDefaultEdit,
			MeshGroup_AttachBoneToChild,
			MeshGroup_DetachBoneFromChild,
			MeshGroup_SetBoneAsParent,
			MeshGroup_SetBoneAsIKTarget,
			MeshGroup_AddBoneFromRetarget,
			MeshGroup_BoneIKControllerChanged,
			MeshGroup_BoneMirrorChanged,

			MeshGroup_DuplicateMeshTransform,
			MeshGroup_DuplicateMeshGroupTransform,
			MeshGroup_DuplicateBone,


			MeshGroup_Gizmo_MoveTransform,
			MeshGroup_Gizmo_RotateTransform,
			MeshGroup_Gizmo_ScaleTransform,
			MeshGroup_Gizmo_Color,

			MeshGroup_AddModifier,
			MeshGroup_RemoveModifier,
			MeshGroup_RemoveParamSet,
			MeshGroup_RemoveParamSetGroup,

			MeshGroup_DefaultSettingChanged,
			MeshGroup_MigrateMeshTransform,


			Modifier_LinkControlParam,
			Modifier_UnlinkControlParam,
			Modifier_AddStaticParamSetGroup,

			Modifier_LayerChanged,
			Modifier_SettingChanged,
			Modifier_SetBoneWeight,
			Modifier_RemoveBoneWeight,
			Modifier_RemoveBoneRigging,
			Modifier_RemovePhysics,
			Modifier_SetPhysicsWeight,
			Modifier_SetVolumeWeight,
			Modifier_SetPhysicsProperty,

			Modifier_ExtraOptionChanged,

			Modifier_Gizmo_MoveTransform,
			Modifier_Gizmo_RotateTransform,
			Modifier_Gizmo_ScaleTransform,
			Modifier_Gizmo_BoneIKTransform,
			Modifier_Gizmo_MoveVertex,
			Modifier_Gizmo_RotateVertex,
			Modifier_Gizmo_ScaleVertex,
			Modifier_Gizmo_FFDVertex,
			Modifier_Gizmo_Color,
			Modifier_Gizmo_BlurVertex,

			Modifier_Gizmo_MovePin,
			Modifier_Gizmo_RotatePin,
			Modifier_Gizmo_ScalePin,
			Modifier_Gizmo_FFDPin,
			Modifier_Gizmo_BlurPin,

			Modifier_ModMeshValuePaste,
			Modifier_ModMeshValueReset,
			Modifier_AddModMeshToParamSet,
			Modifier_RemoveModMeshFromParamSet,

			Modifier_RiggingWeightChanged,

			Modifier_FFDStart,
			Modifier_FFDAdapt,
			Modifier_FFDRevert,

			Anim_SetMeshGroup,
			Anim_DupAnimClip,
			Anim_ImportAnimClip,
			Anim_AddTimeline,
			Anim_RemoveTimeline,
			Anim_AddTimelineLayer,
			Anim_RemoveTimelineLayer,
			Anim_AddKeyframe,
			Anim_MoveKeyframe,
			Anim_CopyKeyframe,
			Anim_RemoveKeyframe,
			Anim_DupKeyframe,
			Anim_KeyframeValueChanged,
			Anim_AddEvent,
			Anim_RemoveEvent,
			Anim_SortEvents,
			Anim_EventChanged,

			Anim_Gizmo_MoveTransform,
			Anim_Gizmo_RotateTransform,
			Anim_Gizmo_ScaleTransform,
			Anim_Gizmo_BoneIKControllerTransform,

			Anim_Gizmo_MoveVertex,
			Anim_Gizmo_RotateVertex,
			Anim_Gizmo_ScaleVertex,
			Anim_Gizmo_FFDVertex,
			Anim_Gizmo_BlurVertex,

			Anim_Gizmo_Color,

			Anim_SettingChanged,

			ControlParam_SettingChanged,
			ControlParam_Duplicated,

			Retarget_ImportSinglePoseToMod,
			Retarget_ImportSinglePoseToAnim,

			PSDSet_AddNewPSDSet,

			MaterialSetAdded,
			MaterialSetRemoved,
			MaterialSetChanged,

			VisibilityChanged,
			GuidelineChanged,
		}

		private Dictionary<ACTION, string> _undoLabels = null;


		public static string GetLabel(ACTION action)
		{
			return I._undoLabels[action];
		}



		//v1.4.2 : 해당 프레임에서의 Undo는 기능에 상관없이 구분하지 않고 처리한다.
		//또한 Continuous에 대해서도 다음과 같이 변경한다.		
		//- OnGUI 시작시 Undo 기록을 받기 위한 준비를 한다.
		//- Undo가 발생하면
		// > 이 프레임에서 Undo가 처음 발생했다면 Group ID를 새로 생성한다. 그 이후엔 해당 ID에 모두 병합된다.
		// > 이 프레임에서 Undo가 추가로 발생했다면, Group ID를 생성하지 않고 Undo만 기록한다.
		//   >> Object 리스트를 이용하여 중복 기록은 하지 않도록 막는다.
		//   >> 생성/삭제를 하고자 할 땐 Object 리스트에서 비교하지 않는다. 삭제시엔 Object 리스트에서 제거한다.
		// > Continuous라면 Undo 자체를 발생시키지 않는다. 다만, 이전/지금의 요청이 복수개의 Undo 요청이었다면, 이건 Continuous로 간주하지 않고 다 기록한다.
		private bool _isRecording = false;//하나라도 Undo 요청이 들어왔다면 True가 된다.
		private bool _isContinuousRecording = false;//Undo가 Continuous로 시작되었는가
		//private int _curGroupID = -1;
		private List<UnityEngine.Object> _curRecordingObjects = null;

		//기본 Value Only이며 더 강한 StructChanged가 하나라도 들어오면 그 값을 사용한다.
		private apEditorUtil.UNDO_STRUCT _curStructChangedType = apEditorUtil.UNDO_STRUCT.ValueOnly;
		private string _curGroupName = "";
		private const string EMPTY_NAME = "";

		//Undo 요청 타입. 완전히 다른 타입의 액션인 경우엔 무조건 Group ID가 분리되어 구분되어야 한다.
		private enum UNDO_REQUEST_TYPE
		{
			None,
			PropertyChanged,
			CreateOrDestroy
		}
		private UNDO_REQUEST_TYPE _curUndoRequestType = UNDO_REQUEST_TYPE.None;


		// Init
		//--------------------------------------------------
		private apUndoGroupData()
		{
			_lastUndoTime = DateTime.Now;
			_isFirstAction = true;

			_undoLabels = new Dictionary<ACTION, string>();

			//중요 : 텍스트를 추가한다. (21.1.25)

			_undoLabels.Add(ACTION.None, "None");

			_undoLabels.Add(ACTION.Main_AddImage, "Add Image");
			_undoLabels.Add(ACTION.Main_RemoveImage, "Remove Image");
			_undoLabels.Add(ACTION.Main_AddMesh, "Add Mesh");
			_undoLabels.Add(ACTION.Main_RemoveMesh, "Remove Mesh");
			_undoLabels.Add(ACTION.Main_AddMeshGroup, "Add MeshGroup");
			_undoLabels.Add(ACTION.Main_RemoveMeshGroup, "Remove MeshGroup");
			_undoLabels.Add(ACTION.Main_AddAnimation, "Add Animation");
			_undoLabels.Add(ACTION.Main_RemoveAnimation, "Remove Animation");
			_undoLabels.Add(ACTION.Main_AddParam, "Add Parameter");
			_undoLabels.Add(ACTION.Main_RemoveParam, "Remove Parameter");

			_undoLabels.Add(ACTION.Portrait_SettingChanged, "Portrait Setting Changed");
			_undoLabels.Add(ACTION.Portrait_BakeOptionChanged, "Bake Option Changed");
			_undoLabels.Add(ACTION.Portrait_SetMeshGroup, "Set Main MeshGroup");
			_undoLabels.Add(ACTION.Portrait_ReleaseMeshGroup, "Release Main MeshGroup");

			_undoLabels.Add(ACTION.Image_SettingChanged, "Set Image Property");
			_undoLabels.Add(ACTION.Image_PSDImport, "Import PSD");

			_undoLabels.Add(ACTION.MeshEdit_AddVertex, "Add Vertex");
			_undoLabels.Add(ACTION.MeshEdit_EditVertex, "Edit Vertex");
			_undoLabels.Add(ACTION.MeshEdit_EditVertexDepth, "Edit Vertex Settings");

			_undoLabels.Add(ACTION.MeshEdit_RemoveVertex, "Remove Vertex");
			_undoLabels.Add(ACTION.MeshEdit_ResetVertices, "Reset Vertices");
			_undoLabels.Add(ACTION.MeshEdit_RemoveAllVertices, "Remove All Vertices");
			_undoLabels.Add(ACTION.MeshEdit_AddEdge, "Add Edge");
			_undoLabels.Add(ACTION.MeshEdit_EditEdge, "Edit Edge");
			_undoLabels.Add(ACTION.MeshEdit_RemoveEdge, "Remove Edge");
			_undoLabels.Add(ACTION.MeshEdit_MakeEdges, "Make Edges");
			_undoLabels.Add(ACTION.MeshEdit_EditPolygons, "Edit Polygons");
			_undoLabels.Add(ACTION.MeshEdit_SetImage, "Set Image");
			_undoLabels.Add(ACTION.MeshEdit_SetPivot, "Set Mesh Pivot");
			_undoLabels.Add(ACTION.MeshEdit_SettingChanged, "Mesh Setting Changed");
			_undoLabels.Add(ACTION.MeshEdit_AtlasChanged, "Mesh Atals Changed");
			_undoLabels.Add(ACTION.MeshEdit_AutoGen, "Vertices Generated");
			_undoLabels.Add(ACTION.MeshEdit_VertexCopied, "Vertices Copied");
			_undoLabels.Add(ACTION.MeshEdit_VertexMoved, "Vertices Moved");
			_undoLabels.Add(ACTION.MeshEdit_FFDStart, "Edit FFD");
			_undoLabels.Add(ACTION.MeshEdit_FFDAdapt, "Adapt FFD");
			_undoLabels.Add(ACTION.MeshEdit_FFDRevert, "Revert FFD");
			

			_undoLabels.Add(ACTION.MeshEdit_AddPin, "Add Pin");
			_undoLabels.Add(ACTION.MeshEdit_MovePin, "Move Pin");
			_undoLabels.Add(ACTION.MeshEdit_MovePin_Rotate, "Move Pin (Rotate)");
			_undoLabels.Add(ACTION.MeshEdit_MovePin_Scale, "Move Pin (Scale)");
			_undoLabels.Add(ACTION.MeshEdit_ChangePin, "Change Pin Properties");
			_undoLabels.Add(ACTION.MeshEdit_CalculatePinWeight, "Calculate Weights");
			_undoLabels.Add(ACTION.MeshEdit_RemovePin, "Remove Pin");

			_undoLabels.Add(ACTION.MeshGroup_AttachMesh, "Attach Mesh");
			_undoLabels.Add(ACTION.MeshGroup_AttachMeshGroup, "Attach MeshGroup");
			_undoLabels.Add(ACTION.MeshGroup_DetachMesh, "Detach Mesh");
			_undoLabels.Add(ACTION.MeshGroup_DetachMeshGroup, "Detach MeshGroup");
			_undoLabels.Add(ACTION.MeshGroup_ClippingChanged, "Clipping Changed");
			_undoLabels.Add(ACTION.MeshGroup_DepthChanged, "Depth Changed");

			_undoLabels.Add(ACTION.MeshGroup_AddBone, "Add Bone");
			_undoLabels.Add(ACTION.MeshGroup_RemoveBone, "Remove Bone");
			_undoLabels.Add(ACTION.MeshGroup_RemoveAllBones, "Remove All Bones");
			_undoLabels.Add(ACTION.MeshGroup_BoneSettingChanged, "Bone Setting Changed");
			_undoLabels.Add(ACTION.MeshGroup_BoneDefaultEdit, "Bone Edit");
			_undoLabels.Add(ACTION.MeshGroup_AttachBoneToChild, "Attach Bone to Child");
			_undoLabels.Add(ACTION.MeshGroup_DetachBoneFromChild, "Detach Bone from Child");
			_undoLabels.Add(ACTION.MeshGroup_SetBoneAsParent, "Set Bone as Parent");
			_undoLabels.Add(ACTION.MeshGroup_SetBoneAsIKTarget, "Set Bone as IK target");
			_undoLabels.Add(ACTION.MeshGroup_AddBoneFromRetarget, "Add Bones from File");
			_undoLabels.Add(ACTION.MeshGroup_BoneIKControllerChanged, "IK Controller Changed");
			_undoLabels.Add(ACTION.MeshGroup_BoneMirrorChanged, "Mirror Changed");

			_undoLabels.Add(ACTION.MeshGroup_DuplicateMeshTransform, "Duplicate Mesh Transform");
			_undoLabels.Add(ACTION.MeshGroup_DuplicateMeshGroupTransform, "Duplicate Mesh Group Transform");
			_undoLabels.Add(ACTION.MeshGroup_DuplicateBone, "Duplicate Bone");

			_undoLabels.Add(ACTION.MeshGroup_Gizmo_MoveTransform, "Default Position");
			_undoLabels.Add(ACTION.MeshGroup_Gizmo_RotateTransform, "Default Rotation");
			_undoLabels.Add(ACTION.MeshGroup_Gizmo_ScaleTransform, "Default Scaling");
			_undoLabels.Add(ACTION.MeshGroup_Gizmo_Color, "Default Color");

			_undoLabels.Add(ACTION.MeshGroup_AddModifier, "Add Modifier");
			_undoLabels.Add(ACTION.MeshGroup_RemoveModifier, "Remove Modifier");
			_undoLabels.Add(ACTION.MeshGroup_RemoveParamSet, "Remove Modified Key");
			_undoLabels.Add(ACTION.MeshGroup_RemoveParamSetGroup, "Remove Modified Set of Keys");

			_undoLabels.Add(ACTION.MeshGroup_DefaultSettingChanged, "Default Setting Changed");
			_undoLabels.Add(ACTION.MeshGroup_MigrateMeshTransform, "Migrate Mesh Transform");

			_undoLabels.Add(ACTION.Modifier_LinkControlParam, "Link Control Parameter");
			_undoLabels.Add(ACTION.Modifier_UnlinkControlParam, "Unlink Control Parameter");
			_undoLabels.Add(ACTION.Modifier_AddStaticParamSetGroup, "Add StaticPSG");

			_undoLabels.Add(ACTION.Modifier_LayerChanged, "Change Layer Order");
			_undoLabels.Add(ACTION.Modifier_SettingChanged, "Change Layer Setting");
			_undoLabels.Add(ACTION.Modifier_SetBoneWeight, "Set Bone Weight");
			_undoLabels.Add(ACTION.Modifier_RemoveBoneWeight, "Remove Bone Weight");
			_undoLabels.Add(ACTION.Modifier_RemoveBoneRigging, "Remove Bone Rigging");
			_undoLabels.Add(ACTION.Modifier_RemovePhysics, "Remove Physics");
			_undoLabels.Add(ACTION.Modifier_SetPhysicsWeight, "Set Physics Weight");
			_undoLabels.Add(ACTION.Modifier_SetVolumeWeight, "Set Volume Weight");
			_undoLabels.Add(ACTION.Modifier_SetPhysicsProperty, "Set Physics Property");

			_undoLabels.Add(ACTION.Modifier_ExtraOptionChanged, "Extra Option Changed");

			_undoLabels.Add(ACTION.Modifier_Gizmo_MoveTransform, "Move Transform");
			_undoLabels.Add(ACTION.Modifier_Gizmo_RotateTransform, "Rotate Transform");
			_undoLabels.Add(ACTION.Modifier_Gizmo_ScaleTransform, "Scale Transform");
			_undoLabels.Add(ACTION.Modifier_Gizmo_BoneIKTransform, "FK/IK Weight Changed");
			_undoLabels.Add(ACTION.Modifier_Gizmo_MoveVertex, "Move Vertex");
			_undoLabels.Add(ACTION.Modifier_Gizmo_RotateVertex, "Rotate Vertex");
			_undoLabels.Add(ACTION.Modifier_Gizmo_ScaleVertex, "Scale Vertex");
			_undoLabels.Add(ACTION.Modifier_Gizmo_FFDVertex, "Freeform Vertices");
			_undoLabels.Add(ACTION.Modifier_Gizmo_Color, "Set Color");
			_undoLabels.Add(ACTION.Modifier_Gizmo_BlurVertex, "Blur Vertices");

			_undoLabels.Add(ACTION.Modifier_Gizmo_MovePin, "Move Pin");
			_undoLabels.Add(ACTION.Modifier_Gizmo_RotatePin, "Rotate Pin");
			_undoLabels.Add(ACTION.Modifier_Gizmo_ScalePin, "Scale Pin");
			_undoLabels.Add(ACTION.Modifier_Gizmo_FFDPin, "Freeform Pins");
			_undoLabels.Add(ACTION.Modifier_Gizmo_BlurPin, "Blur Pins");

			_undoLabels.Add(ACTION.Modifier_ModMeshValuePaste, "Paste Modified Value");
			_undoLabels.Add(ACTION.Modifier_ModMeshValueReset, "Reset Modified Value");

			_undoLabels.Add(ACTION.Modifier_AddModMeshToParamSet, "Add To Key");
			_undoLabels.Add(ACTION.Modifier_RemoveModMeshFromParamSet, "Remove From Key");

			_undoLabels.Add(ACTION.Modifier_RiggingWeightChanged, "Weight Changed");

			_undoLabels.Add(ACTION.Modifier_FFDStart, "Edit FFD");
			_undoLabels.Add(ACTION.Modifier_FFDAdapt, "Adapt FFD");
			_undoLabels.Add(ACTION.Modifier_FFDRevert, "Revert FFD");

			_undoLabels.Add(ACTION.Anim_SetMeshGroup, "Set MeshGroup");
			_undoLabels.Add(ACTION.Anim_DupAnimClip, "Duplicate AnimClip");
			_undoLabels.Add(ACTION.Anim_ImportAnimClip, "Import AnimClip");
			_undoLabels.Add(ACTION.Anim_AddTimeline, "Add Timeline");
			_undoLabels.Add(ACTION.Anim_RemoveTimeline, "Remove Timeline");
			_undoLabels.Add(ACTION.Anim_AddTimelineLayer, "Add Timeline Layer");
			_undoLabels.Add(ACTION.Anim_RemoveTimelineLayer, "Remove Timeline Layer");

			_undoLabels.Add(ACTION.Anim_AddKeyframe, "Add Keyframe");
			_undoLabels.Add(ACTION.Anim_MoveKeyframe, "Move Keyframe");
			_undoLabels.Add(ACTION.Anim_CopyKeyframe, "Copy Keyframe");
			_undoLabels.Add(ACTION.Anim_RemoveKeyframe, "Remove Keyframe");
			_undoLabels.Add(ACTION.Anim_DupKeyframe, "Duplicate Keyframe");

			_undoLabels.Add(ACTION.Anim_KeyframeValueChanged, "Keyframe Value Changed");
			_undoLabels.Add(ACTION.Anim_AddEvent, "Event Added");
			_undoLabels.Add(ACTION.Anim_RemoveEvent, "Event Removed");
			_undoLabels.Add(ACTION.Anim_EventChanged, "Event Changed");
			_undoLabels.Add(ACTION.Anim_SortEvents, "Events Sorted");

			_undoLabels.Add(ACTION.Anim_Gizmo_MoveTransform, "Move Transform");
			_undoLabels.Add(ACTION.Anim_Gizmo_RotateTransform, "Rotate Transform");
			_undoLabels.Add(ACTION.Anim_Gizmo_ScaleTransform, "Scale Transform");
			_undoLabels.Add(ACTION.Anim_Gizmo_BoneIKControllerTransform, "FK/IK Weight Changed");

			_undoLabels.Add(ACTION.Anim_Gizmo_MoveVertex, "Move Vertex");
			_undoLabels.Add(ACTION.Anim_Gizmo_RotateVertex, "Rotate Vertex");
			_undoLabels.Add(ACTION.Anim_Gizmo_ScaleVertex, "Scale Vertex");
			_undoLabels.Add(ACTION.Anim_Gizmo_FFDVertex, "Freeform Vertices");
			_undoLabels.Add(ACTION.Anim_Gizmo_BlurVertex, "Blur Vertices");
			_undoLabels.Add(ACTION.Anim_Gizmo_Color, "Set Color");
			_undoLabels.Add(ACTION.Anim_SettingChanged, "Animation Setting Changed");

			_undoLabels.Add(ACTION.ControlParam_SettingChanged, "Control Param Setting");
			_undoLabels.Add(ACTION.ControlParam_Duplicated, "Control Param Duplicated");

			_undoLabels.Add(ACTION.Retarget_ImportSinglePoseToMod, "Import Pose");
			_undoLabels.Add(ACTION.Retarget_ImportSinglePoseToAnim, "Import Pose");

			_undoLabels.Add(ACTION.PSDSet_AddNewPSDSet, "New PSD Set");

			_undoLabels.Add(ACTION.MaterialSetAdded, "Material Set Added");
			_undoLabels.Add(ACTION.MaterialSetRemoved, "Material Set Removed");
			_undoLabels.Add(ACTION.MaterialSetChanged, "Material Set Changed");

			_undoLabels.Add(ACTION.VisibilityChanged, "Visibility Changed");
			_undoLabels.Add(ACTION.GuidelineChanged, "Guideline Option Changed");



			_isRecording = false;
			_isContinuousRecording = false;
			if (_curRecordingObjects == null)
			{
				_curRecordingObjects = new List<UnityEngine.Object>();
			}
			_curRecordingObjects.Clear();

			_curStructChangedType = apEditorUtil.UNDO_STRUCT.ValueOnly;
			_curGroupName = EMPTY_NAME;
			_curUndoRequestType = UNDO_REQUEST_TYPE.None;

		}

		public void Clear()
		{
			_action = ACTION.None;
			_saveTarget = SAVE_TARGET.None;
			_portrait = null;
			_mesh = null;
			_meshGroup = null;
			_modifier = null;

			//_keyObject = null;
			_isCallContinuous = false;//여러 항목을 동시에 처리하는 Batch 액션 중인가

			_lastUndoTime = DateTime.Now;

			_isRecording = false;
			if (_curRecordingObjects == null)
			{
				_curRecordingObjects = new List<UnityEngine.Object>();
			}
			_curRecordingObjects.Clear();

			_curStructChangedType = apEditorUtil.UNDO_STRUCT.ValueOnly;
			_curGroupName = EMPTY_NAME;
			_curUndoRequestType = UNDO_REQUEST_TYPE.None;
		}




		// Functions
		//--------------------------------------------------
		public void ReadyToUndo()
		{
			_isRecording = false;
			_isContinuousRecording = false;
			if (_curRecordingObjects == null)
			{
				_curRecordingObjects = new List<UnityEngine.Object>();
			}
			_curRecordingObjects.Clear();

			_curStructChangedType = apEditorUtil.UNDO_STRUCT.ValueOnly;
			_curGroupName = EMPTY_NAME;
			_curUndoRequestType = UNDO_REQUEST_TYPE.None;
		}

		public void EndUndo()
		{
			if (!_isRecording)
			{
				//Undo 기록이 시작된게 아니라면
				return;
			}


			//디버그
			//Debug.Log("(End Undo)");
			//DebugUndoRecord();

			//기록한 것을 Hitory에 남기자 < 중요 >
			//이 함수 안에 UndoID에 의한 Collapse가 포함되어있다.
			apUndoHistory.I.AddRecord(Undo.GetCurrentGroup(), Undo.GetCurrentGroupName(), _curStructChangedType == apEditorUtil.UNDO_STRUCT.StructChanged);

			//Recording 상태를 초기화하자
			_isRecording = false;
			_isContinuousRecording = false;
			_curUndoRequestType = UNDO_REQUEST_TYPE.None;

			if (_curRecordingObjects == null)
			{
				_curRecordingObjects = new List<UnityEngine.Object>();
			}
			_curRecordingObjects.Clear();
		}

		/// <summary>
		/// End를 강제로 호출한 후 다시 Record를 하자. Continuos도 해제한다.
		/// </summary>
		public void RestartUndo()
		{
			if(_isRecording)
			{
				//디버그
				//Debug.Log("(Restart Undo)");
				//DebugUndoRecord();

				//Collapse를 하자.
				apUndoHistory.I.AddRecord(	Undo.GetCurrentGroup(), 
											Undo.GetCurrentGroupName(),
											_curStructChangedType == apEditorUtil.UNDO_STRUCT.StructChanged);

				//Continuous를 리셋한다.
				ResetContinuous();
			}

			//값을 초기화하여 다시 Record를 할 준비를 하자
			ReadyToUndo();
		}



		// Undo 기록하기 < 단순 속성 변화 >

		public void StartRecording(	ACTION action,
									apPortrait portrait,
									apMesh mesh,
									apMeshGroup meshGroup,
									apModifierBase modifier,
									apEditorUtil.UNDO_STRUCT structChanged,
									bool isCallContinuous,
									SAVE_TARGET saveTarget)
		{
			//Record가 활성화된 상태에서 재시작을 해야하는 경우
			//- Undo 처리 타입이 Create/Destroy가 진행중이었던 경우
			//- Continuous 타입이었던 경우 (Continuous는 중첩된 Undo가 불가하다)

			if(_isRecording)
			{
				if(_curUndoRequestType == UNDO_REQUEST_TYPE.CreateOrDestroy
					|| _isContinuousRecording)
				{
					//기존의 Undo를 종료하고 재시작한다.
					RestartUndo();
				}
			}


			//현재 Undo 방식은 Property 변경 방식이다.
			_curUndoRequestType = UNDO_REQUEST_TYPE.PropertyChanged;

			//Continuous는 StructChanged 요청인 경우엔 허용되지 않는다.
			if(structChanged == apEditorUtil.UNDO_STRUCT.StructChanged)
			{
				isCallContinuous = false;
			}

			if(!_isRecording)
			{
				// < 아직 기록이 되지 않은 상태 >
				//새롭게 ID를 만드는 단계이다.
				//단 Continuous라면 ID 생성을 생략한다. (ID는 현재 상태의 값에 병합한다.)

				bool isNewAction = CheckNewAction(action, portrait, mesh, meshGroup, modifier, isCallContinuous, saveTarget);

				_isRecording = true;
				
				_curStructChangedType = structChanged;
				_curGroupName = GetLabel(action);

				if(isNewAction)
				{
					//분절된 새로운 액션이다.
					_isContinuousRecording = false;
					Undo.IncrementCurrentGroup();//Group ID를 증가시킨다.
					Undo.SetCurrentGroupName(_curGroupName);

					//Debug.Log("+Undo ID : " + Undo.GetCurrentGroup());
				}
				else
				{
					_isContinuousRecording = true;//이 요청은 Continuous 타입이다.
				}
			}
			else
			{
				// < 이전에 이미 기록이 된 상태 >
				//ID를 생성하지 않은 상태에서 일부 상태값을 갱신한다.

				//기록 추가시에는 Continuous는 무조건 해제된다.
				ResetContinuous();


				_isRecording = true;
				_isContinuousRecording = false;//<<Continuous는 해제


				//ValueOnly보다 더 처리 강도가 높은 Struct Changed로의 전환만 가능하다.
				if(structChanged == apEditorUtil.UNDO_STRUCT.StructChanged)
				{	
					_curStructChangedType = apEditorUtil.UNDO_STRUCT.StructChanged;
				}
			}

			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

			
		}





		/// <summary>
		/// 오브젝트를 기록한다. Undo.RegisterCompleteObjectUndo의 래퍼 함수
		/// </summary>
		/// <param name="targetObject"></param>
		public void RecordObject(UnityEngine.Object targetObject)
		{
			if(targetObject == null)
			{
				return;
			}

			if(_curRecordingObjects.Contains(targetObject))
			{
				return;
			}

			Undo.RegisterCompleteObjectUndo(targetObject, _curGroupName);
			_curRecordingObjects.Add(targetObject);
		}

		/// <summary>오브젝트를 기록한다. Undo.RegisterCompleteObjectUndo의 래퍼 함수</summary>
		public void RecordObjects<T>(List<T> targetObjects) where T : UnityEngine.Object
		{
			int nObjects = targetObjects != null ? targetObjects.Count : 0;
			if(nObjects == 0)
			{
				return;
			}

			List<UnityEngine.Object> validObjs = new List<UnityEngine.Object>();
			UnityEngine.Object curObj = null;
			for (int i = 0; i < nObjects; i++)
			{
				curObj = targetObjects[i];

				if(curObj == null)
				{
					continue;
				}

				if(_curRecordingObjects.Contains(curObj))
				{
					continue;
				}

				validObjs.Add(curObj);
				_curRecordingObjects.Add(curObj);
			}

			if(validObjs.Count > 0)
			{
				Undo.RegisterCompleteObjectUndo(validObjs.ToArray(), _curGroupName);
			}
		}



		/// <summary>
		/// Undo 전에 중복을 체크하기 위해 Action을 등록한다.
		/// 리턴값이 True이면 "새로운 Action"이므로 Undo 등록을 해야한다.
		/// 만약 Action 타입이 Add, New.. 계열이면 targetObject가 null일 수 있다. (parent는 null이 되어선 안된다)
		/// </summary>
		/// <returns>이어지지 않은 새로운 타입의 Undo Action이면 True</returns>
		private bool CheckNewAction(ACTION action, apPortrait portrait, apMesh mesh, apMeshGroup meshGroup, apModifierBase modifier, bool isCallContinuous, SAVE_TARGET saveTarget)
		{	
			bool isTimeOver = false;
			double lastDeltaTime = DateTime.Now.Subtract(_lastUndoTime).TotalSeconds;
			if(lastDeltaTime > CONT_SAVE_TIME || _isFirstAction)
			{
				//Debug.Log("Undo Delta Time : " + lastDeltaTime + " > " + CONT_SAVE_TIME);

				//1초가 넘었다면 강제 Undo ID 증가
				isTimeOver = true;
				_lastUndoTime = DateTime.Now;
				_isFirstAction = false;
			}

			//특정 조건에서는 UndoID가 증가하지 않는다.
			//유효한 Action이고 시간이 지나지 않았다면
			//+CallContinuous 한정
			if(_action != ACTION.None && !isTimeOver && isCallContinuous)
			{
				//이전과 값이 같을 때에만 Multiple 처리가 된다.
				if(	action == _action &&
					saveTarget == _saveTarget &&
					portrait == _portrait &&
					mesh == _mesh &&
					meshGroup == _meshGroup &&
					modifier == _modifier && 
					isCallContinuous == _isCallContinuous
					)
				{
					//연속 호출이면 KeyObject가 달라도 Undo를 묶는다.
					//>KeyObject는 무시
					return false;
				}
			}

			_action = action;

			_saveTarget = saveTarget;
			_portrait = portrait;
			_mesh = mesh;
			_meshGroup = meshGroup;
			_modifier = modifier;

			_isCallContinuous = isCallContinuous;//여러 항목을 동시에 처리하는 Batch 액션 중인가

			return true;
		}


		/// <summary>
		/// 추가 21.6.30 : 마우스 Up, 다른 객체 선택 (작은 단위까지)시 Undo의 연속성을 초기화한다.
		/// 이 함수가 제대로 작동하면 KeyObject를 사용하지 않아도 된다.
		/// </summary>
		public void ResetContinuous()
		{
			_isFirstAction = true;
			_lastUndoTime = DateTime.Now;
			_isCallContinuous = false;
		}


		// Undo 기록하기 < 객체 생성/삭제 >
		public void StartRecording_CreateOrDestroy(string label)
		{
			//객체 생성/삭제시에는 무조건 ID를 증가시킨다. (같은 방식이어도 마찬가지)
			
			//만약 Property Changed 타입의 기록이 있었다면 기록 재시작을 해야한다.
			//이건 객체 생성/삭제 변경용이다.
			if(_isRecording)
			{
				//이전의 Undo는 모두 종료하고 다시 시작한다.
				RestartUndo();
			}


			// < 아직 기록이 되지 않은 상태 >
			//새롭게 ID를 만드는 단계이다.

			//Continuous를 중단
			ResetContinuous();

			_isRecording = true;
				
			_curStructChangedType = apEditorUtil.UNDO_STRUCT.StructChanged;
			_curGroupName = label;
			_isContinuousRecording = false;
			
			Undo.IncrementCurrentGroup();//무조건 Group ID를 증가시킨다.

			//Debug.Log("+Undo ID : " + Undo.GetCurrentGroup() + "[CorD]");

			UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

			_curUndoRequestType = UNDO_REQUEST_TYPE.CreateOrDestroy;//생성/삭제 타입
		}

		public bool IsRecordingAsCreateOrDestroy()
		{
			return _isRecording && _curUndoRequestType == UNDO_REQUEST_TYPE.CreateOrDestroy;
		}



		/// <summary>
		/// 새로운 GameObject를 생성했음을 기록한다.
		/// </summary>
		public void RecordCreatedMonoObject(MonoBehaviour newMonoObject, string label)
		{
			if(newMonoObject == null)
			{
				return;
			}
			if(!_isRecording || _curUndoRequestType != UNDO_REQUEST_TYPE.CreateOrDestroy)
			{
				//Recording이 진행중이 아니라면
				StartRecording_CreateOrDestroy(label);
			}
			Undo.RegisterCreatedObjectUndo(newMonoObject.gameObject, Undo.GetCurrentGroupName());
		}

		/// <summary>
		/// 게임 오브젝트를 삭제하면서 Undo에 기록한다.
		/// </summary>
		public void RecordDestroyMonoObject(MonoBehaviour destroyableMonoObject, string label)
		{
			if(destroyableMonoObject == null)
			{
				return;
			}

			if(!_isRecording || _curUndoRequestType != UNDO_REQUEST_TYPE.CreateOrDestroy)
			{
				//Recording이 진행중이 아니라면
				StartRecording_CreateOrDestroy(label);
			}

			Undo.DestroyObjectImmediate(destroyableMonoObject.gameObject);
		}


		/// <summary>
		/// 여러개의 게임 오브젝트를 생성했음을 기록한다.
		/// </summary>
		public void RecordCreatedMonoObjects<T>(List<T> newMonoObjects, string label) where T : MonoBehaviour
		{
			int nTargets = newMonoObjects != null ? newMonoObjects.Count : 0;
			if(nTargets == 0)
			{
				return;
			}

			if(!_isRecording || _curUndoRequestType != UNDO_REQUEST_TYPE.CreateOrDestroy)
			{
				//Recording이 진행중이 아니라면
				StartRecording_CreateOrDestroy(label);
			}

			T curObj = null;
			for (int i = 0; i < nTargets; i++)
			{
				curObj = newMonoObjects[i];
				if(curObj == null)
				{
					continue;
				}
				Undo.RegisterCreatedObjectUndo(curObj.gameObject, Undo.GetCurrentGroupName());
				curObj = null;
			}
			
		}



		public void RecordDestroyMonoObjects<T>(List<T> destroyableMonoObjects, string label) where T : MonoBehaviour
		{
			int nTargets = destroyableMonoObjects != null ? destroyableMonoObjects.Count : 0;
			if(nTargets == 0)
			{
				return;
			}

			if(!_isRecording || _curUndoRequestType != UNDO_REQUEST_TYPE.CreateOrDestroy)
			{
				//Recording이 진행중이 아니라면
				StartRecording_CreateOrDestroy(label);
			}

			T curObj = null;
			for (int i = 0; i < nTargets; i++)
			{
				curObj = destroyableMonoObjects[i];
				if(curObj != null)
				{
					Undo.DestroyObjectImmediate(curObj.gameObject);
				}
				curObj = null;
			}
		}

		//디버그 함수
		private void DebugUndoRecord()
		{
			if(!_isRecording)
			{
				Debug.Log("Undo가 기록되지 않음");
				return;
			}

			int undoID = Undo.GetCurrentGroup();
			string recordedUndoName = Undo.GetCurrentGroupName();

			string strDebug = "Undo 기록 - ID : " + undoID + " | Name : " + _curGroupName + " (" + recordedUndoName + ")";
			
			int nRecordedObjects = _curRecordingObjects != null ? _curRecordingObjects.Count : 0;

			strDebug += " - " + nRecordedObjects + "개의 오브젝트";
			if(nRecordedObjects > 0)
			{
				UnityEngine.Object curObj = null;
				for (int i = 0; i < nRecordedObjects; i++)
				{
					curObj = _curRecordingObjects[i];
					if(curObj == null)
					{
						continue;
					}
					if(curObj is apPortrait)
					{
						//apPortrait curPortrait = curObj as apPortrait;
						strDebug += "\n- Portrait";
					}
					else if(curObj is apMesh)
					{
						apMesh curMesh = curObj as apMesh;
						strDebug += "\n- Mesh : " + curMesh._name;
					}
					else if(curObj is apMeshGroup)
					{
						apMeshGroup curMeshGroup = curObj as apMeshGroup;
						strDebug += "\n- MeshGroup : " + curMeshGroup._name;
					}
					else if(curObj is apModifierBase)
					{
						apModifierBase curMod = curObj as apModifierBase;
						strDebug += "\n- Modifier : " + curMod.DisplayName;
					}
				}
			}

			Debug.Log(strDebug);
		}
	}
}