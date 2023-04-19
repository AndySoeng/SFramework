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

namespace AnyPortrait
{

	/// <summary>
	/// Editor의 "선택된 대상"에 대한 UI및 주요 처리를 하는 클래스
	/// </summary>
	public partial class apSelection
	{
		// Members
		//-------------------------------------
		public apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }

		public enum SELECTION_TYPE
		{
			None,
			ImageRes,
			Mesh,
			Face,
			MeshGroup,
			Animation,
			Overall,
			Param
		}

		private SELECTION_TYPE _selectionType = SELECTION_TYPE.None;

		public SELECTION_TYPE SelectionType { get { return _selectionType; } }

		private apPortrait _portrait = null;
		private apRootUnit _rootUnit = null;
		private apTextureData _image = null;
		private apMesh _mesh = null;
		private apMeshGroup _meshGroup = null;
		private apControlParam _param = null;
		private apAnimClip _animClip = null;

		//Overall 선택시, 선택가능한 AnimClip 리스트
		private List<apAnimClip> _rootUnitAnimClips = new List<apAnimClip>();
		private apAnimClip _curRootUnitAnimClip = null;


		//Texture 선택시
		private Texture2D _imageImported = null;
		private TextureImporter _imageImporter = null;


		//메시 선택시
		//Area 영역 편집하기 21.1.6
		public enum MESH_AREA_POINT_EDIT
		{
			NotSelected,
			LT, RT, LB, RB
		}
		public MESH_AREA_POINT_EDIT _meshAreaPointEditType = MESH_AREA_POINT_EDIT.NotSelected;




		//Anim Clip 내에서 단일 선택시
		private apAnimTimeline _subAnimTimeline = null;//<<타임라인 단일 선택시
		//이전 : 단일 선택만 적용 > SelectedSubObject로 래핑되어 다중 선택을 지원한다.
		//private apAnimTimelineLayer _subAnimTimelineLayer = null;//타임 라인의 레이어 단일 선택시
		//private apAnimKeyframe _subAnimWorkKeyframe = null;//<<자동으로 선택되는 키프레임이다. "현재 프레임"에 위치한 "레이어의 프레임"이다.

		
		private bool _isAnimTimelineLayerGUIScrollRequest = false;

		//추가 20.7.19 : Shift로 여러개 선택하려면, "이전에 마지막으로 클릭한 타임라인 레이어"를 기억해야한다.
		private apAnimTimelineLayer _lastClickTimelineLayer = null;

		//키프레임을 직접! 선택시 (WorkKeyframe과 다름)
		private apAnimKeyframe _subAnimKeyframe = null;//단일 선택한 키프레임
		private List<apAnimKeyframe> _subAnimKeyframeList = new List<apAnimKeyframe>();//여러개의 키프레임을 선택한 경우 (주로 복불/이동 할때)
		private EX_EDIT _exAnimEditingMode = EX_EDIT.None;//<애니메이션 수정 작업을 하고 있는가
		public EX_EDIT ExAnimEditingMode { get { if (IsAnimEditable) { return _exAnimEditingMode; } return EX_EDIT.None; } }

		//레이어에 상관없이 키프레임을 관리하고자 하는 경우
		//Common 선택 -> 각각의 Keyframe 선택 (O)
		//각각의 Keyframe 선택 -> Common 선택 (X)
		//각각의 Keyframe 선택 -> 해당 FrameIndex의 모든 Keyframe이 선택되었는지 확인 -> Common 선택 (O)
		private List<apAnimCommonKeyframe> _subAnimCommonKeyframeList = new List<apAnimCommonKeyframe>();
		private List<apAnimCommonKeyframe> _subAnimCommonKeyframeList_Selected = new List<apAnimCommonKeyframe>();//<<선택된 Keyframe만 따로 표시한다.

		public List<apAnimCommonKeyframe> AnimCommonKeyframes { get { return _subAnimCommonKeyframeList; } }
		public List<apAnimCommonKeyframe> AnimCommonKeyframes_Selected { get { return _subAnimCommonKeyframeList_Selected; } }

		//추가 3.30 : 키프레임들을 동시에 편집하고자 하는 경우
		public apTimelineCommonCurve _animTimelineCommonCurve = new apTimelineCommonCurve();


		//애니메이션 선택 잠금
		private bool _isAnimSelectionLock = false;


		

		/// <summary>애니메이션 수정 작업이 가능한가?</summary>
		private bool IsAnimEditable
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _subAnimTimeline == null)
				{
					return false;
				}
				if (_animClip._targetMeshGroup == null)
				{
					return false;
				}
				return true;
			}
		}
		public bool IsAnimPlaying
		{
			get
			{
				if (AnimClip == null)
				{
					return false;
				}
				return AnimClip.IsPlaying_Editor;
			}
		}

		//애니메이션 기즈모 리셋을 할지 여부를 체크하기 위한 변수 (v1.4.2)
		private enum ANIM_GIZMO_LINKED_STATUS
		{
			/// <summary>아직 연결되지 않았다. AnimClip를 교체하거나 선택하기 전의 값.</summary>
			None,
			/// <summary>타임라인이 선택되지 않은 상태. 기즈모가 존재하긴 한다.</summary>
			NoTimeline,
			/// <summary>Vertex 편집이 가능한 Anim 모디파이어</summary>
			AnimMod_EditVertex,
			/// <summary>Color Only용 Anim 모디파이어</summary>
			AnimMod_EditColor,
			/// <summary>TF만 편집 가능한 Anim 모디파이어</summary>
			AnimMod_EditTF,
			/// <summary>Control Param 타입의 타임라인</summary>
			ControlParam,
			/// <summary>알 수 없는 방식의 타임라인</summary>
			UnknownTimeline,
		}
		private ANIM_GIZMO_LINKED_STATUS _animGizmoLinkedStatus = ANIM_GIZMO_LINKED_STATUS.None;




		// Bone

		//변경 20.5.26 : MeshTransform, MeshGroupTransform, Bone의 선택 정보를 통합하여 관리하자
		private apMultiSubObjects _subObjects = null;
		public apMultiSubObjects SubObjects { get { return _subObjects; } }
		public object GetSelectedMainObject() { return _subObjects.SelectedObject; }

		private apMultiModData _modData = null;
		public apMultiModData ModData { get { return _modData; } }

		/// <summary>
		/// 객체 선택 후 현재 어떤 상태인지에 관한 결과
		/// </summary>
		public enum SUB_SELECTED_RESULT
		{
			None, Main, Added
		}

		public delegate SUB_SELECTED_RESULT FUNC_IS_SUB_SELECTED(object subObject);

		

		/// <summary>
		/// 메시 그룹의 MeshTF, MeshGroupTF, Bone이 선택되었는지 체크. Hierarchy에서 체크한다.
		/// Anim용 ControlParam은 포함되지 않는다.
		/// </summary>
		/// <param name="subObject"></param>
		/// <returns></returns>
		public SUB_SELECTED_RESULT IsSubSelected(object subObject)
		{
			if(subObject == null) { return SUB_SELECTED_RESULT.None; }
			if(subObject is apTransform_Mesh) { return _subObjects.IsSelected(subObject as apTransform_Mesh); }
			if(subObject is apTransform_MeshGroup) { return _subObjects.IsSelected(subObject as apTransform_MeshGroup); }
			if(subObject is apBone) { return _subObjects.IsSelected(subObject as apBone); }
			return SUB_SELECTED_RESULT.None;
		}

		//선택된 MeshTF, MeshGroupTF, Bone을 리턴한다. 전체 또는 추가(Sub)를 리턴한다.
		//메인으로 선택된건 All 리스트에 있거나 다른 변수를 이용하자
		public List<apTransform_Mesh>		GetSubSeletedMeshTFs(bool isSubOnly)				{ return isSubOnly ? _subObjects.SubMeshTFs : _subObjects.AllMeshTFs; }
		public List<apTransform_MeshGroup>	GetSubSeletedMeshGroupTFs(bool isSubOnly)	{ return isSubOnly ? _subObjects.SubMeshGroupTFs : _subObjects.AllMeshGroupTFs; }
		public List<apBone>					GetSubSeletedBones(bool isSubOnly)							{ return isSubOnly ? _subObjects.SubBones : _subObjects.AllBones; }

		



		//추가 20.5.26 : 선택된 MeshTransform / MeshGroupTransform / Bone을 각각 처리하지 말고 래핑을 하자
		//다중 선택을 지원한다.
		/// <summary>
		/// 현재 선택한게 그냥 클릭했는지 아니면 Ctrl or Shift 키를 누르고 클릭(AddorSubtract)했는지
		/// </summary>
		public enum MULTI_SELECT
		{
			Main,
			AddOrSubtract
		}


		public enum TF_BONE_SELECT
		{
			/// <summary>TF, Bone을 동시에 선택할 수 있다.</summary>
			Inclusive,
			/// <summary>TF, Bone을 동시에 선택할 수 없다.</summary>
			Exclusive
		}

		

		private apModifierBase _modifier = null;

		//Modifier 작업시 선택하는 객체들
		private apModifierParamSet _paramSetOfMod = null;

		

		//추가
		//modBone으로 등록 가능한 apBone 리스트
		private List<apBone> _modRegistableBones = new List<apBone>();
		public List<apBone> ModRegistableBones { get { return _modRegistableBones; } }

		

		//버텍스에 대해서
		//단일 선택일때
		//복수개의 선택일때
		private ModRenderVert		_modRenderVert_Main = null;
		private List<ModRenderVert> _modRenderVerts_All = new List<ModRenderVert>();//<<1개만 선택해도 리스트엔 들어가있다.
		private List<ModRenderVert> _modRenderVerts_Weighted = new List<ModRenderVert>();//<<Soft Selection, Blur, Volume 등에 포함되는 "Weight가 포함된 리스트"

		//추가 22.4.6 [v1.4.0]
		private ModRenderPin		_modRenderPin_Main = null;
		private List<ModRenderPin>	_modRenderPins_All = new List<ModRenderPin>();//여러개 (1개 이상)의 선택된 MRP
		private List<ModRenderPin>	_modRenderPins_Weighted = new List<ModRenderPin>();//<Soft Selection, Blur등에 포함되는 Weight가 포함된 리스트



		//추가 20.7.3 : Undo 직후 MRV가 초기화되는 문제 수정
		//재연결을 위해서 apVertex를 키값으로 한다.
		private bool				_isAnyMRVStoredToRecover = false;
		private apTransform_Mesh	_recoverVert2MRV_MainMeshTF = null;
		private apVertex			_recoverVert2MRV_MainVert = null;
		private Dictionary<apTransform_Mesh, List<apVertex>>	_recoverVert2MRV_All = new Dictionary<apTransform_Mesh, List<apVertex>>();
		private apMeshPin			_recoverPin2MRP_MainPin = null;
		private Dictionary<apTransform_Mesh, List<apMeshPin>>	_recoverPin2MRP_All = new Dictionary<apTransform_Mesh, List<apMeshPin>>();//추가 22.4.6 : 복구시 MRP도 복구하자
		
		/// <summary>Modifier에서 현재 선택중인 ParamSetGroup [주의 : Animated Modifier에서는 이 값을 사용하지 말고 다른 값을 사용해야한다]</summary>
		private apModifierParamSetGroup _subEditedParamSetGroup = null;

		/// <summary>Animated Modifier에서 현재 선택중인 ParamSetGroup의 Pack. [주의 : Animataed Modifier에서만 사용가능하다]</summary>
		private apModifierParamSetGroupAnimPack _subEditedParamSetGroupAnimPack = null;


		//추가 22.6.10 [v1.4.0]
		//현재 모디파이어에 등록된 컨트롤 파라미터를 알 수 있을까
		private Dictionary<apControlParam, apModifierParamSetGroup> _subControlParam2ParamSetGroup = null;
		public Dictionary<apControlParam, apModifierParamSetGroup> ModControlParam2PSGMapping { get { return _subControlParam2ParamSetGroup; } }


		//추가 21.3.18 : Copy & Paste를 1개가 아닌 4개의 슬롯에 넣을 수 있다.
		//슬롯 여러개를 선택했을때, 병합하는 방법도 지정할 수 있다.
		
		private string[] _multiPasteSlotNames_NotMultiSelected = new string[] {"<None>"};
		private string[] _multiPasteSlotNames_MultiSelected = new string[] {"Sum", "Average"};
		private int _iMultiPasteSlotMethod = 0;
		private int _iPasteSlot_Main = 0;
		private const int NUM_PASTE_SLOTS = 4;
		private bool[] _isPasteSlotSelected = null;





		public apPortrait		Portrait { get { return _portrait; } }

		public apRootUnit		RootUnit				{ get { return (_selectionType == SELECTION_TYPE.Overall && _portrait != null) ? _rootUnit : null; } }
		public List<apAnimClip> RootUnitAnimClipList	{ get { return (_selectionType == SELECTION_TYPE.Overall && _portrait != null) ? _rootUnitAnimClips : null; } }
		public apAnimClip		RootUnitAnimClip		{ get { return (_selectionType == SELECTION_TYPE.Overall && _portrait != null) ? _curRootUnitAnimClip : null; } }


		public apTextureData	TextureData	{ get { return (_selectionType == SELECTION_TYPE.ImageRes) ? _image : null; } }
		public apMesh			Mesh		{ get { return (_selectionType == SELECTION_TYPE.Mesh) ? _mesh : null; } }
		public apMeshGroup		MeshGroup	{ get { return (_selectionType == SELECTION_TYPE.MeshGroup) ? _meshGroup : null; } }
		public apControlParam	Param		{ get { return (_selectionType == SELECTION_TYPE.Param) ? _param : null; } }
		public apAnimClip		AnimClip	{ get { return (_selectionType == SELECTION_TYPE.Animation) ? _animClip : null; } }

		//Mesh Group에서 서브 선택
		//Mesh/MeshGroup Transform
		
		//변경 20.5.27 : 래핑되었다.
		public apTransform_Mesh MeshTF_Main					{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _subObjects.MeshTF : null; } }
		public apTransform_MeshGroup MeshGroupTF_Main		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _subObjects.MeshGroupTF : null; } }
		public List<apTransform_Mesh> MeshTFs_All			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _subObjects.AllMeshTFs : null; } }
		public List<apTransform_MeshGroup> MeshGroupTFs_All	{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _subObjects.AllMeshGroupTFs : null; } }
		public List<apBone> Bones_All						{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _subObjects.AllBones : null; } }

		//ParamSetGroup / ParamSetGroupAnimPack
		/// <summary>Modifier에서 현재 선택중인 ParamSetGroup [주의 : Animated Modifier에서는 이 값을 사용하지 말고 다른 값을 사용해야한다]</summary>
		public apModifierParamSetGroup SubEditedParamSetGroup { get { return (_selectionType == SELECTION_TYPE.MeshGroup) ? _subEditedParamSetGroup : null; } }
		

		
		//MeshGroup Setting에서 Pivot을 바꿀 때
		private bool _isMeshGroupSetting_EditDefaultTransform = false;
		public bool IsMeshGroupSettingEditDefaultTransform { get { return _isMeshGroupSetting_EditDefaultTransform; } }

		/// <summary>현재 선택된 Modifier</summary>
		public apModifierBase Modifier				{ get { return (_selectionType == SELECTION_TYPE.MeshGroup) ? _modifier : null; } }

		//Modifier 작업시 선택하는 객체들
		public apModifierParamSet ParamSetOfMod		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup) ? _paramSetOfMod : null; } }
		
		//변경 20.6.10 : 다중 선택 + 애니메이션일 때에도 포함
		public apModifiedMesh	ModMesh_Main			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModMesh : null; } }
		public apModifiedBone	ModBone_Main			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModBone : null; } }
		public apRenderUnit		RenderUnitOfMod_Main	{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.RenderUnit : null; } }
		public List<apModifiedMesh> ModMeshes_All		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModMeshes_All : null; } }
		public List<apModifiedBone> ModBones_All		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModBones_All : null; } }		
		public apModifiedMesh	ModMesh_Gizmo_Main		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModMesh_Gizmo : null; } }
		public apModifiedBone	ModBone_Gizmo_Main		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModBone_Gizmo : null; } }
		public apRenderUnit		RenderUnit_Gizmo_Main	{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.RenderUnit_Gizmo : null; } }


		/// <summary>ModMesh_Gizmo_Main의 MeshGroupTF (있는 경우만)</summary>
		public apTransform_MeshGroup MeshGroupTF_Mod_Gizmo	{ get { return (ModMesh_Gizmo_Main != null) ? ModMesh_Gizmo_Main._transform_MeshGroup : null; } }
		/// <summary>ModMesh_Gizmo_Main의 MeshGroupTF (있는 경우만)</summary>
		public apTransform_Mesh MeshTF_Mod_Gizmo			{ get { return (ModMesh_Gizmo_Main != null) ? ModMesh_Gizmo_Main._transform_Mesh : null; } }
		/// <summary>ModBone_Gizmo_Main의 Bone (있는 경우만)</summary>
		public apBone Bone_Mod_Gizmo						{ get { return (ModBone_Gizmo_Main != null) ? ModBone_Gizmo_Main._bone : null; } }


		public List<apModifiedMesh> ModMeshes_Gizmo_All			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModMeshes_Gizmo_All : null; } }
		public List<apModifiedBone> ModBones_Gizmo_All			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.ModBones_Gizmo_All : null; } }
		public List<apRenderUnit>	RenderUnitOfMod_All			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modData.RenderUnits_All : null; } }

		public ModRenderVert		ModRenderVert_Main			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modRenderVert_Main : null; } }
		public List<ModRenderVert>	ModRenderVerts_All			{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modRenderVerts_All : null; } }
		public List<ModRenderVert>	ModRenderVerts_Weighted		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modRenderVerts_Weighted : null; } }

		public ModRenderPin ModRenderPin_Main					{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modRenderPin_Main : null; } }
		public List<ModRenderPin> ModRenderPins_All				{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modRenderPins_All : null; } }
		public List<ModRenderPin> ModRenderPins_Weighted		{ get { return (_selectionType == SELECTION_TYPE.MeshGroup || AnimClip != null) ? _modRenderPins_Weighted : null; } }


		/// <summary>선택된 Mod-Render Vertex 들의 중앙 위치 (World)</summary>
		public Vector2 ModRenderVertsCenterPos
		{
			get
			{	
				if ((_selectionType != SELECTION_TYPE.MeshGroup && AnimClip == null) || _modRenderVerts_All.Count == 0)
				{
					return Vector2.zero;
				}
				Vector2 centerPos = Vector2.zero;
				for (int i = 0; i < _modRenderVerts_All.Count; i++)
				{
					centerPos += _modRenderVerts_All[i]._renderVert._pos_World;
				}
				centerPos /= _modRenderVerts_All.Count;
				return centerPos;
			}
		}

		/// <summary>선택된 Mod-Render Pin 들의 중앙 위치 (World)</summary>
		public Vector2 ModRenderPinsCenterPos
		{
			get
			{	
				if ((_selectionType != SELECTION_TYPE.MeshGroup && AnimClip == null) || _modRenderPins_All.Count == 0)
				{
					return Vector2.zero;
				}
				Vector2 centerPos = Vector2.zero;
				for (int i = 0; i < _modRenderPins_All.Count; i++)
				{
					centerPos += _modRenderPins_All[i]._renderPin._pos_World;
				}
				centerPos /= _modRenderPins_All.Count;
				return centerPos;
			}
		}



		//Mesh Group을 본격적으로 수정할 땐, 다른 기능이 잠겨야 한다.

		//삭제 22.5.14 : 이제 의미가 없는 값. 초기에 "편집 대상"을 구분하기 위한 값이었으나 이제 구분하지 않는다.
		//public enum EX_EDIT_KEY_VALUE
		//{
		//	None,//<<별 제한없이 컨트롤 가능하며 별도의 UI가 등장하지 않는다.
		//	ModMeshAndParamKey_ModVert,
		//	ParamKey_ModMesh,
		//	ParamKey_Bone
		//}
		////private bool _isExclusiveModifierEdit = false;//<true이면 몇가지 기능이 잠긴다.
		//private EX_EDIT_KEY_VALUE _exEditKeyValue = EX_EDIT_KEY_VALUE.None;
		//public EX_EDIT_KEY_VALUE ExEditMode { get { if (_selectionType == SELECTION_TYPE.MeshGroup) { return _exEditKeyValue; } return EX_EDIT_KEY_VALUE.None; } }

		/// <summary>
		/// Modifier / Animation 작업시 다른 Modifier/AnimLayer를 제외시킬 것인가에 대한 타입.
		/// </summary>
		public enum EX_EDIT
		{
			None,
			///// <summary>수동으로 제한시키지 않는한 최소한의 제한만 작동하는 모드</summary>
			//General_Edit,//삭제 21.1.13 : Modifier Lock을 삭제하고, 공통 옵션만 적용한다.
			/// <summary>수동으로 제한하여 1개의 Modifier(ParamSet)/AnimLayer만 허용하는 모드</summary>
			ExOnly_Edit,
		}
		private EX_EDIT _exclusiveEditing = EX_EDIT.None;//해당 모드에서 제한적 에디팅 중인가
		public EX_EDIT ExEditingMode { get { return (_selectionType == SELECTION_TYPE.MeshGroup) ? _exclusiveEditing : EX_EDIT.None; } }



		//선택잠금
		private bool _isSelectionLock = false;
		public bool IsSelectionLock { get { return _isSelectionLock; } }


		public bool IsExEditable
		{
			get
			{
				if (_selectionType != SELECTION_TYPE.MeshGroup)
				{
					return false;
				}

				if (_meshGroup == null || _modifier == null)
				{
					return false;
				}


				//이전
				//switch (ExEditMode)
				//{
				//	case EX_EDIT_KEY_VALUE.None:
				//		return false;

				//	case EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert:
				//		//return (ExKey_ModParamSetGroup != null) && (ExKey_ModParamSet != null);//이전
				//		return SubEditedParamSetGroup != null && ParamSetOfMod != null;//변경 20.6.10

				//	case EX_EDIT_KEY_VALUE.ParamKey_Bone:
				//	case EX_EDIT_KEY_VALUE.ParamKey_ModMesh:
				//		//return (ExKey_ModParamSetGroup != null) && (ExKey_ModParamSet != null);//이전
				//		return SubEditedParamSetGroup != null && ParamSetOfMod != null;//변경 20.6.10

				//	default:
				//		Debug.LogError("TODO : IsExEditable에 정의되지 않는 타입이 들어왔습니다. [" + ExEditMode + "]");
				//		break;
				//}
				//return false;

				//변경 22.5.14 : ExEditMode 삭제됨
				return SubEditedParamSetGroup != null && ParamSetOfMod != null;//변경 20.6.10
			}
		}

		//추가 22.3.20 : Morph 모디파이어 편집시 (MeshGroup-Modifier(Controller) 또는 Animation)
		//Vertex 편집과 Pin 편집을 선택할 수 있다.
		public enum MORPH_EDIT_TARGET
		{
			Vertex, Pin
		}
		private MORPH_EDIT_TARGET _morphEditTarget = MORPH_EDIT_TARGET.Vertex;
		public MORPH_EDIT_TARGET MorphEditTarget { get { return _morphEditTarget; } }



		//삭제 20.6.10 : 조건없이 바로 해당 변수를 사용할 것. > 다중 선택 처리를 위해서 이 작업이 불필요해졌다.
		#region [미사용 코드]
		////키값으로 사용할 것 - 키로 사용하는 것들
		//public apModifierParamSetGroup ExKey_ModParamSetGroup
		//{
		//	get
		//	{
		//		if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert
		//			|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_Bone
		//			|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
		//		{
		//			return SubEditedParamSetGroup;
		//		}
		//		return null;
		//	}
		//}

		//public apModifierParamSet ExKey_ModParamSet
		//{
		//	get
		//	{
		//		if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert
		//			|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_Bone
		//			|| ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
		//		{
		//			return ParamSetOfMod;
		//		}
		//		return null;
		//	}
		//}

		//public apModifiedMesh ExKey_ModMesh
		//{
		//	get
		//	{
		//		if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
		//		{
		//			return ModMeshOfMod;
		//		}
		//		return null;
		//	}
		//}

		//public apModifiedMesh ExValue_ModMesh
		//{
		//	get
		//	{
		//		if (ExEditMode == EX_EDIT_KEY_VALUE.ParamKey_ModMesh)
		//		{
		//			return ModMeshOfMod;
		//		}
		//		return null;
		//	}
		//}

		//public ModRenderVert ExValue_ModVert
		//{
		//	get
		//	{
		//		if (ExEditMode == EX_EDIT_KEY_VALUE.ModMeshAndParamKey_ModVert)
		//		{
		//			return ModRenderVertOfMod;
		//		}
		//		return null;
		//	}
		//} 
		#endregion



		//리깅 전용 변수 
		//private bool _rigEdit_isBindingEdit = false;//Rig 작업중인가 > 삭제 22.5.15
		private bool _rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가
												   //삭제 19.7.31 > Editor로 변수를 옮겼다.
												   //public enum RIGGING_EDIT_VIEW_MODE
												   //{
												   //	WeightColorOnly,
												   //	WeightWithTexture,
												   //}
												   //public RIGGING_EDIT_VIEW_MODE _rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture;
												   //public bool _rigEdit_isBoneColorView = true;



		private float _rigEdit_setWeightValue = 0.5f;
		private float _rigEdit_scaleWeightValue = 0.95f;
		public bool _rigEdit_isAutoNormalize = true;
		public bool IsRigEditTestPosing { get { return _rigEdit_isTestPosing; } }
		public bool IsRigEditBinding
		{
			get
			{
				//return _rigEdit_isBindingEdit;//이전
				return _exclusiveEditing == EX_EDIT.ExOnly_Edit;//변경 22.5.15
			}
		}

		//추가 19.7.24
		//리깅툴 v2

		//리깅툴 중 Weight를 설정하는 패널의 모드
		public enum RIGGING_WEIGHT_TOOL_MODE
		{
			NumericTool,
			BrushTool
		}
		private RIGGING_WEIGHT_TOOL_MODE _rigEdit_WeightToolMode = RIGGING_WEIGHT_TOOL_MODE.NumericTool;

		public enum RIGGING_BRUSH_TOOL_MODE
		{
			None,
			Add,
			Multiply,
			Blur
		}
		private RIGGING_BRUSH_TOOL_MODE _rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None;
		public RIGGING_BRUSH_TOOL_MODE RiggingBrush_Mode
		{
			get
			{
				return (_rigEdit_WeightToolMode == RIGGING_WEIGHT_TOOL_MODE.NumericTool) ? RIGGING_BRUSH_TOOL_MODE.None : _rigEdit_BrushToolMode;
			}
		}
		//이전 방식 : 직접 크기 조절
		//private int _rigEdit_BrushRadius = 50;
		//public float RiggingBrush_Radius { get { return _rigEdit_BrushRadius; } }

		//변경 22.1.9 : 프리셋 인덱스를 이용
		private int _rigEdit_BrushRadius_Index = apGizmos.DEFAULT_BRUSH_INDEX;
		public int RiggingBrush_RadiusIndex { get { return _rigEdit_BrushRadius_Index; } }


		private float _rigEdit_BrushIntensity_Add = 0.1f;//초당 가중치 더하는 정도
		private float _rigEdit_BrushIntensity_Multiply = 1.1f;//초당 가중치 곱하는 정도
		private int _rigEdit_BrushIntensity_Blur = 50;//초당 가중치 블러 강도 (중심 기준)
		
		public float RiggingBrush_Intensity_Add			{ get { return _rigEdit_BrushIntensity_Add; } }
		public float RiggingBrush_Intensity_Multiply	{ get { return _rigEdit_BrushIntensity_Multiply; } }
		public float RiggingBrush_Intensity_Blur		{ get { return (float)_rigEdit_BrushIntensity_Blur; } }
		public void ResetRiggingBrushMode()
		{
			_rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None;
		}



		private float _physics_setWeightValue = 0.5f;
		private float _physics_scaleWeightValue = 0.95f;
		private float _physics_windSimulationScale = 1000.0f;
		private Vector2 _physics_windSimulationDir = new Vector2(1.0f, 0.5f);

		//추가
		//본 생성시 Width를 한번 수정했으면 그 값이 이어지도록 한다.
		//단, Parent -> Child로 추가되서 자동으로 변경되는 경우는 제외 (직접 수정하는 경우만 적용)
		public int _lastBoneShapeWidth = 30;
		public int _lastBoneShapeTaper = 100;
		public bool _isLastBoneShapeWidthChanged = false;

		//Mesh Edit 변수
		private float _meshEdit_zDepthWeight = 0.5f;

		//추가 : 5.22 새로 생성한 Mesh인 경우 Setting탭이 나와야 한다.
		public List<apMesh> _createdNewMeshes = new List<apMesh>();//<<<Portrait가 바뀌면 초기화한다.


		//추가 21.3.6 : 메시 자동 생성시 QuickGenerate는 3개의 프리셋 + 고급 옵션 전환이 제공된다.
		private string[] _quickMeshGeneratePresetNames = new string[] {"Simple", "Moderate", "Complex", "(Advanced Settings)"};


		/// <summary>
		/// Rigging 시에 "현재 Vertex에 연결된 Bone 정보"를 저장한다.
		/// 복수의 Vertex를 선택할 경우를 대비해서 몇가지 변수가 추가
		/// </summary>
		public class VertRigData
		{
			public apBone _bone = null;
			public int _nRig = 0;
			public float _weight = 0.0f;
			public float _weight_Min = 0.0f;
			public float _weight_Max = 0.0f;
			public VertRigData(apBone bone, float weight)
			{
				_bone = bone;
				_nRig = 1;
				_weight = weight;
				_weight_Min = _weight;
				_weight_Max = _weight;
			}
			public void AddRig(float weight)
			{
				_weight = ((_weight * _nRig) + weight) / (_nRig + 1);
				_nRig++;
				_weight_Min = Mathf.Min(weight, _weight_Min);
				_weight_Max = Mathf.Max(weight, _weight_Max);
			}
		}
		private List<VertRigData> _rigEdit_vertRigDataList = new List<VertRigData>();

		// 애니메이션 선택 정보
		public apAnimTimeline AnimTimeline						{ get { return (AnimClip != null) ? _subAnimTimeline : null; } }

		//이전 : 1개의 타임라인 레이어 + 1개의 Work 키프레임을 선택한다. (대상 객체가 1개이므로)
		//public apAnimTimelineLayer AnimTimelineLayer { get { if (AnimClip != null) { return _subAnimTimelineLayer; } return null; } }
		//public apAnimKeyframe AnimWorkKeyframe { get { if (AnimTimelineLayer != null) { return _subAnimWorkKeyframe; } return null; } }

		//변경 10.6.10 : 다중 선택으로 여러개의 타임라인 레이어 + 여러개의 Work 키프레임들을 선택할 수 있다. 아이고 복잡해라.
		public apAnimTimelineLayer AnimTimelineLayer_Main		{ get { return (AnimClip != null) ? _subObjects.TimelineLayer : null; } }
		public apAnimTimelineLayer AnimTimelineLayer_Gizmo		{ get { return (AnimClip != null) ? _subObjects.TimelineLayer_Gizmo : null; } }
		public apAnimKeyframe AnimWorkKeyframe_Main				{ get { return (AnimClip != null) ? _subObjects.WorkKeyframe : null; } }
		public List<apAnimTimelineLayer> AnimTimelineLayers_All { get { return (AnimClip != null) ? _subObjects.AllTimelineLayers : null; } }
		public List<apAnimKeyframe> AnimWorkKeyframes_All		{ get { return (AnimClip != null) ? _subObjects.AllWorkKeyframes : null; } }
		public int NumAnimTimelineLayers						{ get { return (AnimTimelineLayers_All != null) ? AnimTimelineLayers_All.Count : 0; } }
		public int NumAnimWorkKeyframes							{ get { return (AnimWorkKeyframes_All != null) ? AnimWorkKeyframes_All.Count : 0; } }
		
		public apAnimKeyframe AnimKeyframe			{ get { return (AnimClip != null) ? _subAnimKeyframe : null; } }
		public List<apAnimKeyframe> AnimKeyframes	{ get { return (AnimClip != null) ? _subAnimKeyframeList : null; } }
		public bool IsAnimKeyframeMultipleSelected	{ get { return (AnimClip != null) ? (_subAnimKeyframeList.Count > 1) : false; } }
		
		
		public bool IsSelectedKeyframe(apAnimKeyframe keyframe)
		{
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null)
			{
				Debug.LogError("Not Animation Type");
				return false;
			}
			return _subAnimKeyframeList.Contains(keyframe);
		}

		public void CancelAnimEditing() { _exAnimEditingMode = EX_EDIT.None; _isAnimSelectionLock = false; }


		public enum ANIM_SINGLE_PROPERTY_UI { Value, Curve }
		public ANIM_SINGLE_PROPERTY_UI _animPropertyUI = ANIM_SINGLE_PROPERTY_UI.Value;

		public enum ANIM_SINGLE_PROPERTY_CURVE_UI { Prev, Next }
		public ANIM_SINGLE_PROPERTY_CURVE_UI _animPropertyCurveUI = ANIM_SINGLE_PROPERTY_CURVE_UI.Next;

		//추가 19.12.31 : 다중 키프레임의 커브 속성
		public enum ANIM_MULTI_PROPERTY_CURVE_UI { Prev, Middle, Next }
		public ANIM_MULTI_PROPERTY_CURVE_UI _animPropertyCurveUI_Multi = ANIM_MULTI_PROPERTY_CURVE_UI.Next;

		//변경 20.5.27 : 래핑됨 > MeshTF와 MeshGroupTF는 기본과 같은걸 이용한다.
		public apControlParam SelectedControlParamOnAnimClip { get { return (AnimClip != null) ? _subObjects.ControlParamForAnim : null; } }
		

		public bool IsAnimSelectionLock { get { return (AnimClip != null) ? _isAnimSelectionLock : false; } }

		private float _animKeyframeAutoScrollTimer = 0.0f;


		//애니메이션 하단 오른쪽 UI의 타입
		//작은 데이터가 우선이다.
		private enum ANIM_BOTTOM_PROPERTY_UI
		{
			NoSelected,
			SingleKeyframe,
			MultipleKeyframes,
			SingleTimelineLayer,
			MultipleTimelineLayers,
			Timeline//타임라인은 여러개 선택되지 않으므로
		}


		//메시 그룹의 TF 설정 UI의 타입
		private enum MESHGROUP_RIGHT_SETTING_PROPERTY_UI
		{
			NoSelected,
			SingleMeshTF,
			MultipleMeshTF,
			SingleMeshGroupTF,
			MultipleMeshGroupTF,
			Mixed
		}



		//Bone 편집
		//private apBone _bone = null;//현재 선택한 Bone (어떤 모드에서든지 참조 가능)
		//이전
		//public apBone Bone { get { return _bone; } }

		//변경 20.5.27 : 래핑됨
		public apBone Bone { get { return _subObjects.Bone; } }
		

		private bool _isBoneDefaultEditing = false;
		public bool IsBoneDefaultEditing { get { return _isBoneDefaultEditing; } }

		public enum BONE_EDIT_MODE
		{
			None,
			SelectOnly,
			Add,
			SelectAndTRS,
			Link
		}
		private BONE_EDIT_MODE _boneEditMode = BONE_EDIT_MODE.None;
		//public BONE_EDIT_MODE BoneEditMode { get { if (!_isBoneDefaultEditing) { return BONE_EDIT_MODE.None; } return _boneEditMode; } }
		public BONE_EDIT_MODE BoneEditMode { get { return _boneEditMode; } }

		public enum MESHGROUP_CHILD_HIERARCHY { ChildMeshes, Bones }
		public MESHGROUP_CHILD_HIERARCHY _meshGroupChildHierarchy = MESHGROUP_CHILD_HIERARCHY.ChildMeshes;
		public MESHGROUP_CHILD_HIERARCHY _meshGroupChildHierarchy_Anim = MESHGROUP_CHILD_HIERARCHY.ChildMeshes;

		//추가 20.3.28 : 리깅 작업시, (또는 그 외의 상황에서)
		//현재 작업 대상인 본들을 리스트로 만들어서, 그 외의 본들을 반투명하게 만드는게 작업에 효과적이다.
		private apRenderUnit _prevRenderUnit_CheckLinkedToModBones = null;
		private Dictionary<apBone, bool> _linkedToModBones = new Dictionary<apBone, bool>();
		/// <summary>현재 모디파이어 (주로 리깅)에 연결되었던 본들.</summary>
		public Dictionary<apBone, bool> LinkedToModifierBones { get { return _linkedToModBones; } }


		// 통계 GUI
		private bool _isStatisticsNeedToRecalculate = true;//재계산이 필요하다.
		private bool _isStatisticsAvailable = false;

		private int _statistics_NumMesh = 0;
		private int _statistics_NumVert = 0;
		private int _statistics_NumEdge = 0;
		private int _statistics_NumTri = 0;
		private int _statistics_NumClippedMesh = 0;
		private int _statistics_NumClippedVert = 0;//클리핑은 따로 계산(Parent+Child)
		private int _statistics_NumTimelineLayer = 0;
		private int _statistics_NumKeyframes = 0;
		private int _statistics_NumBones = 0;//추가 19.12.25


		//추가 : Ex Edit를 위한 RenderUnit Flag 갱신시, 중복 처리를 막기 위함
		private apMeshGroup _prevExFlag_MeshGroup = null;
		private apModifierBase _prevExFlag_Modifier = null;
		private apModifierParamSetGroup _prevExFlag_ParamSetGroup = null;
		private apAnimClip _prevExFlag_AnimClip = null;
		private apAnimTimeline _prevExFlag_AnimTimeline = null;
		private bool _prevExFlag_EditMode = false;
		private bool _prevExFlag_IsDisabledExRunAvailable = false;


		//추가 22.3.2 : 핀 편집 모드에서 선택된 핀들
		private apMeshPin _selectedPin = null;
		private List<apMeshPin> _selectedPinList = null;

		private apMeshPin _snapPin = null;//Ctrl 버튼을 눌렀을 때 보여주는 가장 가까운 핀
		private bool _isPinMouseWire = false;//핀을 추가하기 위해 이전 위치 + 마우스까지의 와이어 그리기
		private Vector2 _pinMouseWirePosW = Vector2.zero;

		public apMeshPin MeshPin { get { return _selectedPin; } }
		public List<apMeshPin> MeshPins { get { return _selectedPinList; } }
		public int NumMeshPins { get { return _selectedPinList != null ? _selectedPinList.Count : 0; } }
		
		public apMeshPin SnapPin { get { return _snapPin; } }
		public bool IsPinEdit_MouseWire { get { return _isPinMouseWire; } }
		public Vector2 PinEdit_MouseWirePosW { get { return _pinMouseWirePosW; } }


		//캡쳐 변수
		private enum CAPTURE_MODE
		{
			None,
			Capturing_Thumbnail,//<<썸네일 캡쳐중
			Capturing_ScreenShot,//<<ScreenShot 캡쳐중
			Capturing_GIF_Animation,//GIF 애니메이션 캡쳐중
			Capturing_MP4_Animation,//추가 : MP4 애니메이션 캡쳐중
			Capturing_Spritesheet
		}
		private CAPTURE_MODE _captureMode = CAPTURE_MODE.None;
		private object _captureLoadKey = null;
		private string _capturePrevFilePath_Directory = "";
		private apAnimClip _captureSelectedAnimClip = null;
		private bool _captureGIF_IsProgressDialog = false;

		private bool _captureSprite_IsAnimClipInit = false;
		private List<apAnimClip> _captureSprite_AnimClips = new List<apAnimClip>();
		private List<bool> _captureSprite_AnimClipFlags = new List<bool>();


		//추가 19.6.10 : MeshTransform의 속성 관련
		private string[] _shaderMode_Names = new string[] { "Material Set", "Custom Shader" };
		private const int MESH_SHADER_MODE__MATERIAL_SET = 0;
		private const int MESH_SHADER_MODE__CUSTOM_SHADER = 1;

		//추가 19.6.29 : TmpWorkVisible 변경 여부를 검사하자.
		private bool _isTmpWorkVisibleChanged_Meshes = false;
		private bool _isTmpWorkVisibleChanged_Bones = false;
		public void SetTmpWorkVisibleChanged(bool isAnyMeshChanged, bool isAnyBoneChanged)
		{
			_isTmpWorkVisibleChanged_Meshes = isAnyMeshChanged;
			_isTmpWorkVisibleChanged_Bones = isAnyBoneChanged;
		}
		public bool IsTmpWorkVisibleChanged_Meshes { get { return _isTmpWorkVisibleChanged_Meshes; } }
		public bool IsTmpWorkVisibleChanged_Bones { get { return _isTmpWorkVisibleChanged_Bones; } }



		//추가 20.9.8 : 모디파이어에 객체를 추가할 수 없을때, 그 사유를 정할 수 있다.
		private enum MOD_ADD_FAIL_REASON
		{
			/// <summary>이 모디파이어에서는 선택된 객체의 타입을 지원하지 않는다.[기본값]</summary>
			NotSupportedType,
			/// <summary>리깅이 적용된 하위 메시 그룹의 메시는 TF 모디파이어에 추가할 수 없다.</summary>
			RiggedChildMeshInTFMod,
		}




		//기타 지역 변수 대용으로 쓰이는 변수들
		private object _prevSelectedAnimObject = null;
		private object _prevSelectedAnimTimeline = null;
		private object _prevSelectedAnimTimelineLayer = null;
		private int _prevSelectedNumAnimTimelineLayer = -1;
		private bool _isIgnoreAnimTimelineGUI = false;
		private bool _isFoldUI_AnimationTimelineLayers = false;


		private object _loadKey_ImportAnimClipRetarget = null;
		private object _loadKey_SelectPhysicsParam = null;

		private object _loadKey_SelectControlParamToPhyWind = null;
		private object _loadKey_SelectControlParamToPhyGravity = null;

		private object _physicModifier_prevSelectedTransform = null;
		private bool _physicModifier_prevIsContained = false;

		private object _riggingModifier_prevSelectedTransform = null;
		private bool _riggingModifier_prevIsContained = false;
		private int _riggingModifier_prevNumBoneWeights = 0;
		private int _riggingModifier_prevInfoMode = -1;

		private object _loadKey_SinglePoseImport_Mod = null;
		private object _loadKey_AddControlParam = null;

		private Vector2 _scrollBottom_Status = Vector2.zero;

		private object _loadKey_DuplicateBone = null;

		private object _loadKey_SelectControlParamForIKController = null;

		private object _loadKey_SelectBone = null;

		private apBone _prevBone_BoneProperty = null;
		private int _prevBone_NumSelected = -1;
		private int _prevChildBoneCount = 0;

		private object _loadKey_SelectOtherMeshTransformForCopyingSettings = null;

		private object _loadKey_SelectMaterialSetOfMeshTransform = null;
		private apTransform_Mesh _tmp_PrevMeshTransform_MeshGroupSettingUI = null;

		private apAnimKeyframe _tmpPrevSelectedAnimKeyframe = null;
		private object _loadKey_SinglePoseImport_Anim = null;

		private bool _isTimelineWheelDrag = false;
		private Vector2 _prevTimelineWheelDragPos = Vector2.zero;
		private Vector2 _scrollPos_BottomAnimationRightProperty = Vector2.zero;

		private Vector2 _scroll_Timeline = new Vector2();
		//private float _scroll_Timeline_DummyY = 0.0f;
		private bool _isScrollingTimelineY = false;//타임라인의 Y스크롤 중인가.
		//private const string GUI_NAME_TIMELINE_SCROLL_Y = "GUI_TimlineScroll_Y";
		//private const string GUI_NAME_TIMELINE_SCROLL_X = "GUI_TimlineScroll_X";
		

		private object _loadKey_AddModifier = null;
		private object _loadKey_OnBoneStructureLoaded = null;
		private object _loadKey_SelectTextureDataToMesh = null;
		private object _loadKey_OnSelectControlParamPreset = null;
		private apControlParam _prevParam = null;
		private object _loadKey_SelectMeshGroupToAnimClip = null;
		private object _loadKey_AddTimelineToAnimClip = null;
		private object _loadKey_SelectTextureAsset = null;
		private object _loadKey_SelectBonesForAutoRig = null;
		private object _loadKey_MigrateMeshTransform = null;

		private int _timlineGUIWidth = -1;



		//추가 19.11.20 : GUIContent들 (Wrapper) > Auto-Mesh V1 UI이다. 사용 안함
		//private apGUIContentWrapper _guiContent_StepCompleted = null;
		//private apGUIContentWrapper _guiContent_StepUncompleted = null;
		//private apGUIContentWrapper _guiContent_StepUnUsed = null;


		private apGUIContentWrapper _guiContent_imgValueUp = null;
		private apGUIContentWrapper _guiContent_imgValueDown = null;
		private apGUIContentWrapper _guiContent_imgValueLeft = null;
		private apGUIContentWrapper _guiContent_imgValueRight = null;

		private apGUIContentWrapper _guiContent_MeshProperty_ResetVerts = null;
		private apGUIContentWrapper _guiContent_MeshProperty_RemoveMesh = null;
		private apGUIContentWrapper _guiContent_MeshProperty_ChangeImage = null;
		private apGUIContentWrapper _guiContent_MeshProperty_AutoLinkEdge = null;
		private apGUIContentWrapper _guiContent_MeshProperty_Draw_MakePolygones = null;
		private apGUIContentWrapper _guiContent_MeshProperty_MakePolygones = null;
		private apGUIContentWrapper _guiContent_MeshProperty_RemoveAllVertices = null;
		//private apGUIContentWrapper _guiContent_MeshProperty_HowTo_MouseLeft = null;
		//private apGUIContentWrapper _guiContent_MeshProperty_HowTo_MouseMiddle = null;
		//private apGUIContentWrapper _guiContent_MeshProperty_HowTo_MouseRight = null;
		//private apGUIContentWrapper _guiContent_MeshProperty_HowTo_KeyDelete = null;
		//private apGUIContentWrapper _guiContent_MeshProperty_HowTo_KeyCtrl = null;
		//private apGUIContentWrapper _guiContent_MeshProperty_HowTo_KeyShift = null;
		private apGUIContentWrapper _guiContent_MeshProperty_Texture = null;

		private apGUIContentWrapper _guiContent_MeshProperty_PinRangeOption = null;
		private apGUIContentWrapper _guiContent_MeshProperty_PinCalculateWeight = null;
		private apGUIContentWrapper _guiContent_MeshProperty_PinResetTestPos = null;
		private apGUIContentWrapper _guiContent_MeshProperty_RemoveAllPins = null;
		

		private apGUIContentWrapper _guiContent_Bottom2_Physic_WindON = null;
		private apGUIContentWrapper _guiContent_Bottom2_Physic_WindOFF = null;

		private apGUIContentWrapper _guiContent_Image_RemoveImage = null;
		private apGUIContentWrapper _guiContent_Animation_SelectMeshGroupBtn = null;
		private apGUIContentWrapper _guiContent_Animation_AddTimeline = null;
		private apGUIContentWrapper _guiContent_Animation_RemoveAnimation = null;
		private apGUIContentWrapper _guiContent_Animation_TimelineUnit_AnimMod = null;
		private apGUIContentWrapper _guiContent_Animation_TimelineUnit_ControlParam = null;

		private apGUIContentWrapper _guiContent_Overall_SelectedAnimClp = null;
		private apGUIContentWrapper _guiContent_Overall_MakeThumbnail = null;
		private apGUIContentWrapper _guiContent_Overall_TakeAScreenshot = null;
		private apGUIContentWrapper _guiContent_Overall_AnimItem = null;
		private apGUIContentWrapper _guiContent_Overall_Unregister = null;

		private apGUIContentWrapper _guiContent_Param_Presets = null;
		private apGUIContentWrapper _guiContent_Param_RemoveParam = null;
		private apGUIContentWrapper _guiContent_Param_IconPreset = null;

		private apGUIContentWrapper _guiContent_MeshGroupProperty_RemoveMeshGroup = null;
		private apGUIContentWrapper _guiContent_MeshGroupProperty_RemoveAllBones = null;
		private apGUIContentWrapper _guiContent_MeshGroupProperty_ModifierLayerUnit = null;
		private apGUIContentWrapper _guiContent_MeshGroupProperty_SetRootUnit = null;
		private apGUIContentWrapper _guiContent_MeshGroupProperty_AddModifier = null;

		private apGUIContentWrapper _guiContent_Bottom_Animation_TimelineLayerInfo = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_RemoveKeyframes = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_RemoveNumKeyframes = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_Fit = null;

		private apGUIContentWrapper _guiContent_Right_MeshGroup_MaterialSet = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_CustomShader = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_MatSetName = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_CopySettingToOtherMeshes = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_RiggingIconAndText = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_ParamIconAndText = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_RemoveBone = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_RemoveModifier = null;

		private apGUIContentWrapper _guiContent_Modifier_ParamSetItem = null;
		private apGUIContentWrapper _guiContent_Modifier_AddControlParameter = null;
		private apGUIContentWrapper _guiContent_CopyTargetIcon = null;
		private apGUIContentWrapper _guiContent_CopyTextIcon = null;
		private apGUIContentWrapper _guiContent_PasteTextIcon = null;
		private apGUIContentWrapper _guiContent_Modifier_RigExport = null;
		private apGUIContentWrapper _guiContent_Modifier_RigImport = null;
		private apGUIContentWrapper _guiContent_Modifier_RemoveFromKeys = null;
		private apGUIContentWrapper _guiContent_Modifier_AddToKeys = null;
		private apGUIContentWrapper _guiContent_Modifier_RemoveAllKeys = null;
		private apGUIContentWrapper _guiContent_Modifier_AnimIconText = null;
		private apGUIContentWrapper _guiContent_Modifier_RemoveFromRigging = null;
		private apGUIContentWrapper _guiContent_Modifier_AddToRigging = null;
		private apGUIContentWrapper _guiContent_Modifier_AddToPhysics = null;
		private apGUIContentWrapper _guiContent_Modifier_RemoveFromPhysics = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_NameIcon = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_Basic = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_Stretchiness = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_Inertia = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_Restoring = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_Viscosity = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_Gravity = null;
		private apGUIContentWrapper _guiContent_Modifier_PhysicsSetting_Wind = null;
		private apGUIContentWrapper _guiContent_Right_Animation_AllObjectToLayers = null;
		private apGUIContentWrapper _guiContent_Right_Animation_RemoveTimeline = null;
		private apGUIContentWrapper _guiContent_Right_Animation_AddTimelineLayerToEdit = null;
		private apGUIContentWrapper _guiContent_Right_Animation_RemoveTimelineLayer = null;
		//private apGUIContentWrapper _guiContent_Bottom_EditMode_CommonIcon = null;
		private apGUIContentWrapper _guiContent_Icon_ModTF_Pos = null;
		private apGUIContentWrapper _guiContent_Icon_ModTF_Rot = null;
		private apGUIContentWrapper _guiContent_Icon_ModTF_Scale = null;
		private apGUIContentWrapper _guiContent_Icon_Mod_Color = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_MeshIcon = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_MeshGroupIcon = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_MultipleSelected = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_ModIcon = null;
		private apGUIContentWrapper _guiContent_Right_MeshGroup_AnimIcon = null;
		private apGUIContentWrapper _guiContent_Right_Animation_TimelineIcon_AnimWithMod = null;
		private apGUIContentWrapper _guiContent_Right_Animation_TimelineIcon_AnimWithControlParam = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_FirstFrame = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_PrevFrame = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_Play = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_Pause = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_NextFrame = null;
		private apGUIContentWrapper _guiContent_Bottom_Animation_LastFrame = null;

		//Auto-Mesh V1의 UI
		//private apGUIContentWrapper _guiContent_MakeMesh_PointCount_X = null;
		//private apGUIContentWrapper _guiContent_MakeMesh_PointCount_Y = null;
		//private apGUIContentWrapper _guiContent_MakeMesh_AutoGenPreview = null;
		private apGUIContentWrapper _guiContent_MakeMesh_GenerateMesh = null;
		private apGUIContentWrapper _guiContent_MakeMesh_QuickGenerate = null;
		private apGUIContentWrapper _guiContent_MakeMesh_MultipleQuickGenerate = null;

		private apGUIContentWrapper _guiContent_MeshEdit_Area_Enabled = null;
		private apGUIContentWrapper _guiContent_MeshEdit_Area_Disabled = null;
		private apGUIContentWrapper _guiContent_MeshEdit_AreaEditing_Off = null;
		private apGUIContentWrapper _guiContent_MeshEdit_AreaEditing_On = null;

		private apGUIContentWrapper _guiContent_AnimKeyframeProp_PrevKeyLabel = null;
		private apGUIContentWrapper _guiContent_AnimKeyframeProp_NextKeyLabel = null;

		private apGUIContentWrapper _guiContent_Right2MeshGroup_ObjectProp_Name = null;
		private apGUIContentWrapper _guiContent_Right2MeshGroup_ObjectProp_Type = null;
		private apGUIContentWrapper _guiContent_Right2MeshGroup_ObjectProp_NickName = null;

		private apGUIContentWrapper _guiContent_Right2MeshGroup_JiggleBone = null;

		private apGUIContentWrapper _guiContent_MaterialSet_ON = null;
		private apGUIContentWrapper _guiContent_MaterialSet_OFF = null;

		private apGUIContentWrapper _guiContent_Right2MeshGroup_MaskParentName = null;
		private apGUIContentWrapper _guiContent_Right2MeshGroup_DuplicateTransform = null;
		private apGUIContentWrapper _guiContent_Right2MeshGroup_MigrateTransform = null;
		private apGUIContentWrapper _guiContent_Right2MeshGroup_DetachObject = null;


		private apGUIContentWrapper _guiContent_ModProp_ParamSetTarget_Name = null;
		private apGUIContentWrapper _guiContent_ModProp_ParamSetTarget_StatusText = null;

		private apGUIContentWrapper _guiContent_ModProp_Rigging_VertInfo = null;
		private apGUIContentWrapper _guiContent_ModProp_Rigging_BoneInfo = null;

		private apGUIContentWrapper _guiContent_RiggingBoneWeightLabel = null;
		private apGUIContentWrapper _guiContent_RiggingBoneWeightBoneName = null;


		private apGUIContentWrapper _guiContent_PhysicsGroupID_None = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_1 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_2 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_3 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_4 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_5 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_6 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_7 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_8 = null;
		private apGUIContentWrapper _guiContent_PhysicsGroupID_9 = null;

		private apGUIContentWrapper _guiContent_Right2_Animation_TargetObjectName = null;


		//TODO : GUIContent 추가시 ResetGUIContents() 함수에 초기화 코드를 추가할 것

		private GUIStyle _guiStyle_RigIcon_Lock = null;


		private apStringWrapper _strWrapper_64 = null;
		private apStringWrapper _strWrapper_128 = null;
		private string[] _imageColorSpaceNames = new string[] { "Gamma", "Linear" };
		private string[] _imageQualityNames = new string[] { "Compressed [Low Quality]", "Compressed [Default]", "Compressed [High Quality]", "Uncompressed" };
		private string[] _captureSpritePackSizeNames = new string[] { "256", "512", "1024", "2048", "4096" };











		// Init
		//-------------------------------------
		public apSelection(apEditor editor)
		{
			_editor = editor;
			Clear();
		}

		public void Clear()
		{
			_selectionType = SELECTION_TYPE.None;

			_portrait = null;
			_image = null;
			_mesh = null;
			_meshGroup = null;
			_param = null;
			_modifier = null;
			_animClip = null;


			//이전
			//_bone = null;
			//_bone_Multi.Clear();//>추가 20.5.26

			//_subMeshTransformInGroup = null;
			//_subMeshGroupTransformInGroup = null;
			//_subMeshTransformInGroup_Multi.Clear();
			//_subMeshGroupTransformInGroup_Multi.Clear();

			//변경 20.5.27 : 선택된 객체에 대해 래핑을 했다.
			if(_subObjects == null)
			{
				_subObjects = new apMultiSubObjects(null);
			}
			_subObjects.Clear();

			if(_modData == null)
			{
				_modData = new apMultiModData(this);
			}
			_modData.ClearAll();
			
			
			_subEditedParamSetGroup = null;
			_subEditedParamSetGroupAnimPack = null;
			
			if(_subControlParam2ParamSetGroup == null)
			{
				_subControlParam2ParamSetGroup = new Dictionary<apControlParam, apModifierParamSetGroup>();
			}
			_subControlParam2ParamSetGroup.Clear();



			//_exEditKeyValue = EX_EDIT_KEY_VALUE.None;//삭제 22.5.14
			_exclusiveEditing = EX_EDIT.None;
			_isSelectionLock = false;

			//_renderUnitOfMod = null;//<<삭제 20.6.10 : 래핑되었다 (_selectedModData)

			_modRenderVert_Main = null;
			_modRenderVerts_All.Clear();
			_modRenderVerts_Weighted.Clear();

			//추가 22.4.6 [v1.4.0]
			_modRenderPin_Main = null;
			_modRenderPins_All.Clear();
			_modRenderPins_Weighted.Clear();


			//삭제 20.5.25
			//_subMeshTransformListInGroup.Clear();
			//_subMeshGroupTransformListInGroup.Clear();

			_isMeshGroupSetting_EditDefaultTransform = false;

			_subAnimTimeline = null;

			//삭제 20.6.10
			//_subAnimTimelineLayer = null;
			//_subAnimWorkKeyframe = null;

			_subAnimKeyframe = null;

			//삭제 20.5.27
			//_subMeshTransformOnAnimClip = null;
			//_subMeshGroupTransformOnAnimClip = null;
			//_subControlParamOnAnimClip = null;

			_subAnimKeyframeList.Clear();
			//_isAnimEditing = false;
			_exAnimEditingMode = EX_EDIT.None;
			//_isAnimAutoKey = false;
			_isAnimSelectionLock = false;

			_animTimelineCommonCurve.Clear();//추가 3.30

			_subAnimCommonKeyframeList.Clear();
			_subAnimCommonKeyframeList_Selected.Clear();


			//삭제 20.6.10 : 래핑되었다.
			//_modMeshOfAnim = null;
			//_modBoneOfAnim = null;
			//_renderUnitOfAnim = null;


			//삭제 20.6.29 : Mod 변수와 통합
			//_modRenderVertOfAnim = null;
			//_modRenderVertListOfAnim.Clear();
			//_modRenderVertListOfAnim_Weighted.Clear();




			_isBoneDefaultEditing = false;

			//_rigEdit_isBindingEdit = false;//Rig 작업중인가 > 삭제 22.5.15 : 편집 모드 모두 통일
			_rigEdit_isTestPosing = false;//Rig 중에 Test Pose를 제어하고 있는가
			//_rigEdit_viewMode = RIGGING_EDIT_VIEW_MODE.WeightWithTexture//<<이건 초기화 안된다.

			_imageImported = null;
			_imageImporter = null;

			_createdNewMeshes.Clear();

			_linkedToModBones.Clear();
			_prevRenderUnit_CheckLinkedToModBones = null;


			_iMultiPasteSlotMethod = 0;
			_iPasteSlot_Main = 0;
			if(_isPasteSlotSelected == null)
			{
				_isPasteSlotSelected = new bool[NUM_PASTE_SLOTS];
			}
			for (int i = 0; i < NUM_PASTE_SLOTS; i++)
			{
				_isPasteSlotSelected[i] = false;
			}
			

			//추가 20.4.13 : VisibilityController 추가됨
			Editor.VisiblityController.ClearAll();

			
		}


		// Functions
		//-------------------------------------

		//삭제 22.5.14
		////모디파이어 편집 모드 설정
		///// <summary>
		///// [MeshGroup 편집시] 모디파이어의 편집 모드 설정
		///// </summary>
		///// <param name="editMode"></param>
		///// <returns></returns>
		//public bool SetModifierEditMode(EX_EDIT_KEY_VALUE editMode)
		//{
		//	if (_selectionType != SELECTION_TYPE.MeshGroup
		//		|| _modifier == null)
		//	{
		//		_exEditKeyValue = EX_EDIT_KEY_VALUE.None;
		//		_exclusiveEditing = EX_EDIT.None;
		//		return false;
		//	}

		//	if (_exEditKeyValue != editMode)
		//	{
		//		_exclusiveEditing = EX_EDIT.None;
		//		_isSelectionLock = false;

		//		if (MeshGroup != null)
		//		{
		//			//Exclusive 모두 해제
		//			MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					
		//			//이전
		//			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup, false, true, true);

		//			//변경 20.4.13
		//			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	MeshGroup, 
		//																apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff,
		//																apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

		//			//이전
		//			//RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가
					
		//			//변경 21.2.15
		//			RefreshMeshGroupExEditingFlags(false);
		//		}
		//	}
		//	_exEditKeyValue = editMode;

		//	RefreshModifierExclusiveEditing();//<<Mod Lock 갱신

		//	Editor.Gizmos.OnSelectedObjectsChanged();//20.7.5 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.

		//	Editor.RefreshControllerAndHierarchy(false);

		//	return true;
		//}

		//삭제 22.5.14
		///// <summary>
		///// [MeshGroup 편집시] Modifier 편집시 Mod Lock을 갱신한다.
		///// SetModifierExclusiveEditing() 함수를 호출하는 것과 같으나,
		///// Lock-Unlock이 전환되지는 않는다.
		///// </summary>
		//public void RefreshModifierExclusiveEditing(bool isIgnoreExEditable = false)
		//{
		//	if (_selectionType != SELECTION_TYPE.MeshGroup
		//		|| _modifier == null
		//		|| _subEditedParamSetGroup == null
		//		|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None)
		//	{
		//		_exclusiveEditing = EX_EDIT.None;
		//	}


		//	SetModifierExclusiveEditing(_exclusiveEditing, isIgnoreExEditable);
		//}

		//모디파이어의 Exclusive Editing (Modifier Lock)
		/// <summary>
		/// [MeshGroup 편집시] 모디파이어의 다중 편집 옵션 변경
		/// </summary>
		/// <param name="exclusiveEditing"></param>
		/// <param name="isIgnoreExEditable"></param>
		/// <returns></returns>
		public bool SetModifierExclusiveEditing(EX_EDIT exclusiveEditing, bool isIgnoreExEditable = false)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				//|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None//삭제
				)
			{
				_exclusiveEditing = EX_EDIT.None;
				return false;
			}

			//이전 : 상황에 따라 편집 모드가 자동으로 해제된다.
			//bool isExEditable = IsExEditable;
			//if (MeshGroup == null || Modifier == null || SubEditedParamSetGroup == null)
			//{
			//	isExEditable = false;
			//}

			////기존
			//if (!isIgnoreExEditable)
			//{
			//	if (isExEditable)
			//	{
			//		_exclusiveEditing = exclusiveEditing;
			//	}
			//	else
			//	{
			//		_exclusiveEditing = EX_EDIT.None;
			//	}
			//}
			//else
			//{
			//	//추가 3.31 : ExEditing 모드를 유지하는 옵션 추가
			//	_exclusiveEditing = exclusiveEditing;
			//}


			//변경 22.5.14 : 일단 무조건 적용
			_exclusiveEditing = exclusiveEditing;


			//작업중인 Modifier 외에는 일부 제외를 하자
			//switch (_exclusiveEditing)
			//{
			//	case EX_EDIT.None:
			//		//모든 Modifier를 활성화한다.
			//		{
			//			//이전
			//			//if (MeshGroup != null)
			//			//{
			//			//	//Exclusive 모두 해제
			//			//	MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
							
			//			//	//이전
			//			//	//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup, false, true, true);
							
			//			//	//변경 20.4.13
			//			//	Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	MeshGroup, 
			//			//														apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff,
			//			//														apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);


			//			//	//RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//<<추가

			//			//	//변경 21.2.15
			//			//	RefreshMeshGroupExEditingFlags(false);
			//			//}

						

			//			//_modRenderVert_Main = null;
			//			//_modRenderVerts_All.Clear();
			//			//_modRenderVerts_Weighted.Clear();

			//			//_modRenderPin_Main = null;
			//			//_modRenderPins_All.Clear();
			//			//_modRenderPins_Weighted.Clear();
			//		}
			//		break;

					
			//	case EX_EDIT.ExOnly_Edit:
			//		{
			//			//작업중인 Modifier만 활성화한다. (Mod Lock)

			//			//이전
			//			//apModifierStack.OTHER_MOD_RUN_OPTION otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.Disabled;
			//			//if(Editor._exModObjOption_UpdateByOtherMod)
			//			//{
			//			//	//다중 편집 허용 중이라면 (D키)
			//			//	otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveAllPossible;
			//			//}
			//			//else if(Editor._modLockOption_ColorPreview)
			//			//{
			//			//	//다중 편집은 아니지만 색상 미리보기라도 지원한다면
			//			//	otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveColorOnly;
			//			//}
			//			////변경 21.2.15
			//			//RefreshMeshGroupExEditingFlags(false);
			//		}
			//		break;
			//}

			//변경 22.5.14 : 함수로 깔끔하게 정리
			AutoRefreshModifierExclusiveEditing();

			if(_exclusiveEditing == EX_EDIT.None)
			{
				//편집 모드 종료시 버텍스/핀은 선택 해제
				_modRenderVert_Main = null;
				_modRenderVerts_All.Clear();
				_modRenderVerts_Weighted.Clear();

				_modRenderPin_Main = null;
				_modRenderPins_All.Clear();
				_modRenderPins_Weighted.Clear();
			}

			
			Editor.Gizmos.OnSelectedObjectsChanged();// 추가 22.5.14 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
			Editor.RefreshControllerAndHierarchy(false);

			

			return true;
		}


		private enum MODIFIER_ACTIVE_REQUEST
		{
			EnableAll,
			DisableAll,
			EnableExEdit
		}



		/// <summary>
		/// 추가 22.5.14 : 현재 옵션에 따라서 모디파이어와 PSG의 활성 여부를 자동으로 결정한다.
		/// 기존의 SetModifierExclusiveEditing, SetModifierExclusiveEditing_Anim 함수의 기능을 포괄한다.
		/// 다른 플래그, 변수들에 대한 제어는 이 함수를 호출하는 함수에서 직접하다. (이 함수는 모디파이어의 ON/OFF만 관리한다.)
		/// 특정 메뉴에서는 편집 모드 자체를 자동으로 비활성화 한다.
		/// </summary>
		public void AutoRefreshModifierExclusiveEditing()
		{
			apMeshGroup targetMeshGroup = null;
			apAnimClip targetAnimClip = null;
			MODIFIER_ACTIVE_REQUEST request = MODIFIER_ACTIVE_REQUEST.EnableAll;

			bool isTurnOffEditMode = false;

			//Pin 모드에서는 Rigging도 동작하지 않는다.
			bool isPinEditMode = false;

			if(_selectionType == SELECTION_TYPE.MeshGroup)
			{
				//선택된 메뉴가 [ 메시 그룹 ]인 경우
				targetMeshGroup = _meshGroup;
				if (targetMeshGroup != null)
				{
					switch (Editor._meshGroupEditMode)
					{
						case apEditor.MESHGROUP_EDIT_MODE.Setting:
							{
								// 설정 탭
								if (_isMeshGroupSetting_EditDefaultTransform)
								{
									//Default Transform 편집 중이라면 > 모두 비활성화
									request = MODIFIER_ACTIVE_REQUEST.DisableAll;
								}
								else
								{
									//그 외에는 모두 활성화
									request = MODIFIER_ACTIVE_REQUEST.EnableAll;
								}

								isTurnOffEditMode = true;
							}

							break;

						case apEditor.MESHGROUP_EDIT_MODE.Modifier:
							{
								if (_modifier != null
									&& _subEditedParamSetGroup != null)
								{
									//< 중요 >
									//모디파이어/PSG를 선택했다
									//편집 모드인가
									//단 Rigging인 경우엔 다르게 처리한다.

									if(_modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Morph
										&& _morphEditTarget == MORPH_EDIT_TARGET.Pin)
									{
										//Morph 모디파이어 + 핀 편집 모드에서는 리깅도 제한된다.
										isPinEditMode = true;
									}

									if(_exclusiveEditing == EX_EDIT.ExOnly_Edit)
									{
										// 편집 모드 > 대상 모디파이어만 활성
										request = MODIFIER_ACTIVE_REQUEST.EnableExEdit;
									}
									else
									{
										// 편집 모드가 아니다. > 모두 활성화
										request = MODIFIER_ACTIVE_REQUEST.EnableAll;
									}
								}
								else
								{
									//선택된 모디파이어가 없다. (모디파이어/PSG가 추가된 적이 없다면) > 설정탭과 동일하므로 모두 활성화
									request = MODIFIER_ACTIVE_REQUEST.EnableAll;
									isTurnOffEditMode = true;
								}
							}
							break;

						case apEditor.MESHGROUP_EDIT_MODE.Bone:
						default:
							{
								if(_isBoneDefaultEditing)
								{
									//본 편집 모드에서는 모두 비활성화
									request = MODIFIER_ACTIVE_REQUEST.DisableAll;
								}
								else
								{
									//일반 모드에서는 모두 활성화
									request = MODIFIER_ACTIVE_REQUEST.EnableAll;
								}

								isTurnOffEditMode = true;
							}
							break;
					}
				}
				else
				{
					//메시 그룹이 없다. (에러상황) > 모두 비활성화
					request = MODIFIER_ACTIVE_REQUEST.DisableAll;
					isTurnOffEditMode = true;
				}
			}
			else if(_selectionType == SELECTION_TYPE.Animation)
			{
				//선택된 메뉴가 [ 애니메이션 ]인 경우
				targetAnimClip = _animClip;
				targetMeshGroup = _animClip != null ? _animClip._targetMeshGroup : null;
				if(targetMeshGroup == null)
				{
					//연결된 메시 그룹이 없다.
					request = MODIFIER_ACTIVE_REQUEST.DisableAll;
					isTurnOffEditMode = true;
				}
				else 
				{	
					if(_subAnimTimeline == null)
					{
						//타임라인이 선택되지 않았다. > 모두 활성화
						request = MODIFIER_ACTIVE_REQUEST.EnableAll;
						isTurnOffEditMode = true;
					}
					else
					{
						//선택된 타임라인의 종류에 따라서 결정하자
						if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
							&& _subAnimTimeline._linkedModifier != null)
						{
							// 모디파이어 타입의 타임라인이라면, 편집 모드를 따른다.
							if (_exAnimEditingMode == EX_EDIT.ExOnly_Edit)
							{
								// 편집 모드 > 대상 모디파이어만 활성
								request = MODIFIER_ACTIVE_REQUEST.EnableExEdit;
							}
							else
							{
								// 편집 모드가 아니다. > 모두 활성화
								request = MODIFIER_ACTIVE_REQUEST.EnableAll;
							}

							if (_subAnimTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
							{
								// [ Animated Morph ] 타입이라면
								//핀 편집 모드를 체크하자
								if(_morphEditTarget == MORPH_EDIT_TARGET.Pin)
								{
									isPinEditMode = true;
								}
							}
						}
						else
						{
							// 컨트롤 파라미터 타입의 타임라인이라면, 모두 동작하게 만든다.
							request = MODIFIER_ACTIVE_REQUEST.EnableAll;
						}
					}
				}
			}
			else if(_selectionType == SELECTION_TYPE.Overall)
			{
				//선택된 메뉴가 [ 루트 유닛 ]인 경우
				//모두 실행하자
				if(_rootUnit != null)
				{
					targetMeshGroup = _rootUnit._childMeshGroup;
				}
				request = MODIFIER_ACTIVE_REQUEST.EnableAll;
				isTurnOffEditMode = true;
			}
			else
			{
				request = MODIFIER_ACTIVE_REQUEST.DisableAll;
				isTurnOffEditMode = true;
			}


			//편집 모드가 자동으로 무조건 꺼져야 하는 상태라면
			if(isTurnOffEditMode)
			{
				_exclusiveEditing = EX_EDIT.None;
				_exAnimEditingMode = EX_EDIT.None;
			}


			//연결된 메시 그룹이 없으면 실패
			if(targetMeshGroup == null
				|| targetMeshGroup._modifierStack == null)
			{
				return;
			}

			

			if(request == MODIFIER_ACTIVE_REQUEST.EnableAll)
			{
				// 모두 활성화
				targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();

				//메시 그룹내 유닛들의 Tmp World Visibility를 제어한다.
				Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	targetMeshGroup, 
																	apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff,
																	apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);
			}
			else if(request == MODIFIER_ACTIVE_REQUEST.DisableAll)
			{
				//모두 비활성화
				targetMeshGroup._modifierStack.SetDisableForceAllModifier();
			}
			else
			{
				//선택 대상인 일부의 모디파이어만 활성화한다.

				apModifierStack.OTHER_MOD_RUN_OPTION otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.Disabled;
				if(Editor._exModObjOption_UpdateByOtherMod && !isPinEditMode)
				{
					//다중 편집 허용 중이라면 (D키) + 핀 편집 모드가 아니라면
					otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveAllPossible;//다중 편집 허용
				}
				else if(Editor._modLockOption_ColorPreview)
				{
					//다중 편집은 아니지만 색상 미리보기라도 지원한다면
					otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveColorOnly;
				}

				if(targetAnimClip == null)
				{
					// 애니메이션 클립이 아닌 [ 메시 그룹 ] 에서의 편집 모드라면
					targetMeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, _subEditedParamSetGroup, otherModRunOption);
				}
				else
				{
					// [ 애니메이션 ] 에서의 편집 모드
					//현재의 AnimTimeline에 해당하는 ParamSet만 선택하자
					List<apModifierParamSetGroup> exParamSetGroups = new List<apModifierParamSetGroup>();
					List<apModifierParamSetGroup> linkParamSetGroups = _subAnimTimeline._linkedModifier._paramSetGroup_controller;
					
					int nPSGs = linkParamSetGroups != null ? linkParamSetGroups.Count : 0;
					apModifierParamSetGroup linkPSG = null;
					for (int iP = 0; iP < nPSGs; iP++)
					{
						linkPSG = linkParamSetGroups[iP];
						if (linkPSG._keyAnimTimeline == _subAnimTimeline &&
							linkPSG._keyAnimClip == _animClip)
						{
							exParamSetGroups.Add(linkPSG);
						}
					}

					targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_Anim(
																			_subAnimTimeline._linkedModifier,
																			exParamSetGroups,
																			otherModRunOption);
				}
			}

			//어느 객체가 편집 중인지 확인하자
			RefreshMeshGroupExEditingFlags(false);

			if(targetAnimClip != null)
			{
				// 애니메이션 모드라면, 이 함수가 호출될 때 
				AutoSelectAnimTimelineLayer(false);
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Info, null, null);
				SetAnimClipGizmoEvent();
			}

			//이 함수 호출 이후엔, 다음의 코드가 실행되어야 한다.
			//Editor.Gizmos.OnSelectedObjectsChanged();//20.7.5 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
			//Editor.RefreshControllerAndHierarchy(false);
		}






		//Ex Bone 렌더링용 함수
		//많은 내용이 빠져있다.
		public bool SetModifierExclusiveEditing_Tmp(EX_EDIT exclusiveEditing)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				|| _subEditedParamSetGroup == null
				//|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None
				)
			{
				_exclusiveEditing = EX_EDIT.None;
				return false;
			}

			bool isExEditable = IsExEditable;
			if (MeshGroup == null || Modifier == null || SubEditedParamSetGroup == null)
			{
				isExEditable = false;
			}

			if (isExEditable)
			{
				_exclusiveEditing = exclusiveEditing;
			}
			else
			{
				_exclusiveEditing = EX_EDIT.None;
			}

			//작업중인 Modifier 외에는 일부 제외를 하자
			switch (_exclusiveEditing)
			{
				case EX_EDIT.None:
					//모든 Modifier를 활성화한다.
					{
						if (MeshGroup != null)
						{
							//Exclusive 모두 해제
							MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
							//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup);
							//RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//삭제 21.2.15 : 아래에 일괄 호출
						}
					}
					break;

				case EX_EDIT.ExOnly_Edit:
					{
						//작업중인 Modifier만 활성화한다. (Mod Lock)
						//이전
						//MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup, Editor._modLockOption_ColorPreview, Editor._exModObjOption_UpdateByOtherMod);

						apModifierStack.OTHER_MOD_RUN_OPTION otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.Disabled;
						if(Editor._exModObjOption_UpdateByOtherMod)
						{
							//다중 편집 허용 중이라면 (D키)
							otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveAllPossible;
						}
						else if(Editor._modLockOption_ColorPreview)
						{
							//다중 편집은 아니지만 색상 미리보기라도 지원한다면
							otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveColorOnly;
						}

						MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup, otherModRunOption);
						//RefreshMeshGroupExEditingFlags(MeshGroup, _modifier, SubEditedParamSetGroup, null, false);//삭제 21.2.15 : 아래에 일괄 호출
					}
					break;
			}

			//변경 21.2.15
			Editor.Select.RefreshMeshGroupExEditingFlags(false);

			return true;
		}


		/// <summary>
		/// 특정 메시그룹의 RenderUnit과 Bone의 ExEdit에 대한 Flag를 갱신한다.
		/// Ex Edit가 변경되는 모든 시점(편집 모드 전환, 객체 추가/삭제 등)에서 이 함수를 호출한다.
		/// AnimClip이 선택되어 있다면 animClip이 null이 아닌 값을 넣어준다.
		/// AnimClip이 없다면 ParamSetGroup이 있어야 한다. (Static 타입 제외)
		/// 둘다 null이라면 Ex Edit가 아닌 것으로 처리한다.
		/// Child MeshGroup으로 재귀적으로 호출한다.
		/// </summary>
		/// <param name="targetModifier"></param>
		/// <param name="targetAnimClip"></param>
		public void RefreshMeshGroupExEditingFlags(	//apMeshGroup targetMeshGroup,
													
													////MeshGroup을 선택한 경우
													//apModifierBase targetModifier,
													//apModifierParamSetGroup targetParamSetGroup,
													////AnimClip을 선택한 경우
													//apAnimClip targetAnimClip,
													//apAnimTimeline targetAnimTimeline,

													
													bool isForce
													//bool isRecursiveCall = false
													)
		{
			//변경 사항 21.2.14
			//이전에는 모디파이어의 여러개의 PSG중 하나만 해당해도 ExEdit가 활성화되었다.
			//이제는 해당 PSG 또는 AnimTimeline에 직접 포함되어 있어야 한다.

			//MeshGroup을 선택한 경우
			apMeshGroup targetMeshGroup = null;
			apModifierBase targetModifier = null;
			apModifierParamSetGroup targetParamSetGroup = null;
			
			//AnimClip을 선택한 경우
			apAnimClip targetAnimClip = null;
			apAnimTimeline targetAnimTimeline = null;

			bool isEditMode = false;

			if(_selectionType == SELECTION_TYPE.MeshGroup)
			{
				targetMeshGroup = MeshGroup;
				targetModifier = Modifier;
				targetParamSetGroup = SubEditedParamSetGroup;
				
				//이전
				//isEditMode = ExEditingMode == EX_EDIT.ExOnly_Edit;
				
				//변경 22.5.14
				isEditMode = targetModifier != null
					&& Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier
					&& _exclusiveEditing == EX_EDIT.ExOnly_Edit;
			}
			else if(_selectionType == SELECTION_TYPE.Animation)
			{
				targetAnimClip = AnimClip;
				if(targetAnimClip != null)
				{
					targetMeshGroup = targetAnimClip._targetMeshGroup;
					targetAnimTimeline = AnimTimeline;
				}
				isEditMode = ExAnimEditingMode == EX_EDIT.ExOnly_Edit;
			}

			

			//재귀적인 호출이 아니라면
			//if (!isRecursiveCall)
			//{
			if (!isForce)
			{
				if(targetMeshGroup != null
					&& targetMeshGroup == _prevExFlag_MeshGroup
					&& targetModifier == _prevExFlag_Modifier
					&& targetParamSetGroup == _prevExFlag_ParamSetGroup
					&& targetAnimClip == _prevExFlag_AnimClip
					&& targetAnimTimeline == _prevExFlag_AnimTimeline
					&& _prevExFlag_EditMode == isEditMode
					&& _prevExFlag_IsDisabledExRunAvailable == Editor._exModObjOption_UpdateByOtherMod)
				{
					//중복 요청이다.
					return;
				}
			}

			_prevExFlag_MeshGroup = targetMeshGroup;
			_prevExFlag_Modifier = targetModifier;
			_prevExFlag_ParamSetGroup = targetParamSetGroup;
			_prevExFlag_AnimClip = targetAnimClip;
			_prevExFlag_AnimTimeline = targetAnimTimeline;
			_prevExFlag_EditMode = isEditMode;
			_prevExFlag_IsDisabledExRunAvailable = Editor._exModObjOption_UpdateByOtherMod;//외부에 의해서 실행 가능한지 판별

			
			if (targetMeshGroup == null)
			{
				//Debug.LogError("Target MeshGroup is Null");
				return;
			}


			//변경 21.2.15
			//PSG 또는 Timeline에 등록된 객체들의 Ex 상태를 갱신한다. (모디파이어 단위 아님)
			//Edit 모드가 아니면 모두 해제한다.
			//자식 객체들도 처리하되, 재귀 함수를 이용하자
			Dictionary<apRenderUnit, bool> syncRenderUnits = null;
			Dictionary<apBone, bool> syncBones = null;
			if(isEditMode)
			{
				apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;
				apBone curBone = null;

				//편집모드에서는 "편집 중인 객체들"을 Dictionary로 정리하자
				if(_selectionType == SELECTION_TYPE.MeshGroup)
				{
					if(targetParamSetGroup != null)
					{
						syncRenderUnits = new Dictionary<apRenderUnit, bool>();
						syncBones = new Dictionary<apBone, bool>();

						int nSyncMeshTF = targetParamSetGroup._syncTransform_Mesh != null ? targetParamSetGroup._syncTransform_Mesh.Count : 0;
						int nSyncMeshGroupTF = targetParamSetGroup._syncTransform_MeshGroup != null ? targetParamSetGroup._syncTransform_MeshGroup.Count : 0;
						int nSyncBone = targetParamSetGroup._syncBone != null ? targetParamSetGroup._syncBone.Count : 0;

						

						if (nSyncMeshTF > 0)
						{
							for (int i = 0; i < nSyncMeshTF; i++)
							{
								curMeshTF = targetParamSetGroup._syncTransform_Mesh[i];

								if(curMeshTF != null 
									&& curMeshTF._linkedRenderUnit != null
									&& !syncRenderUnits.ContainsKey(curMeshTF._linkedRenderUnit))
								{
									syncRenderUnits.Add(curMeshTF._linkedRenderUnit, true);
								}
							}
						}

						if (nSyncMeshGroupTF > 0)
						{
							for (int i = 0; i < nSyncMeshGroupTF; i++)
							{
								curMeshGroupTF = targetParamSetGroup._syncTransform_MeshGroup[i];
								
								if(curMeshGroupTF != null 
									&& curMeshGroupTF._linkedRenderUnit != null
									&& !syncRenderUnits.ContainsKey(curMeshGroupTF._linkedRenderUnit))
								{
									syncRenderUnits.Add(curMeshGroupTF._linkedRenderUnit, true);
								}
							}
						}

						if(nSyncBone > 0)
						{
							for (int i = 0; i < nSyncBone; i++)
							{
								curBone = targetParamSetGroup._syncBone[i];
								
								if(curBone != null && !syncBones.ContainsKey(curBone))
								{
									syncBones.Add(curBone, true);
								}
							}
						}
						
					}
				}
				else if(_selectionType == SELECTION_TYPE.Animation)
				{
					if(targetAnimTimeline != null && targetAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						syncRenderUnits = new Dictionary<apRenderUnit, bool>();
						syncBones = new Dictionary<apBone, bool>();

						int nLayers = targetAnimTimeline._layers != null ? targetAnimTimeline._layers.Count : 0;

						if(nLayers > 0)
						{
							apAnimTimelineLayer curLayer = null;
							for (int i = 0; i < nLayers; i++)
							{
								curLayer = targetAnimTimeline._layers[i];
								if (curLayer._linkedMeshTransform != null)
								{
									curMeshTF = curLayer._linkedMeshTransform;

									if (curMeshTF != null
										&& curMeshTF._linkedRenderUnit != null
										&& !syncRenderUnits.ContainsKey(curMeshTF._linkedRenderUnit))
									{
										syncRenderUnits.Add(curMeshTF._linkedRenderUnit, true);
									}
								}
								else if (curLayer._linkedMeshGroupTransform != null)
								{
									curMeshGroupTF = curLayer._linkedMeshGroupTransform;

									if (curMeshGroupTF != null
										&& curMeshGroupTF._linkedRenderUnit != null
										&& !syncRenderUnits.ContainsKey(curMeshGroupTF._linkedRenderUnit))
									{
										syncRenderUnits.Add(curMeshGroupTF._linkedRenderUnit, true);
									}
								}
								else if (curLayer._linkedBone != null)
								{
									curBone = curLayer._linkedBone;

									if (curBone != null && !syncBones.ContainsKey(curBone))
									{
										syncBones.Add(curBone, true);
									}
								}
							}
						}
					}
				}
			}

			//이제 하나씩 편집 플래그를 입력하자
			RefreshRenderUnitAndBoneExFlag_Recursive(	targetMeshGroup, targetMeshGroup, 
														isEditMode, Editor._exModObjOption_UpdateByOtherMod, 
														syncRenderUnits, syncBones);
		}



		/// <summary>
		/// RefreshMeshGroupExEditingFlags 함수와 유사하지만, 실제 편집 여부에 관계없이 모두 Enable로 강제하는 함수이다.
		/// </summary>
		public void SetEnableMeshGroupExEditingFlagsForce()
		{
			apMeshGroup targetMeshGroup = null;
			
			//AnimClip을 선택한 경우
			apAnimClip targetAnimClip = null;
			
			if(_selectionType == SELECTION_TYPE.MeshGroup)
			{
				targetMeshGroup = MeshGroup;
			}
			else if(_selectionType == SELECTION_TYPE.Animation)
			{
				targetAnimClip = AnimClip;
				if(targetAnimClip != null)
				{
					targetMeshGroup = targetAnimClip._targetMeshGroup;
				}
			}
			else if(_selectionType == SELECTION_TYPE.Overall)
			{
				if(RootUnit != null)
				{
					targetMeshGroup = RootUnit._childMeshGroup;
				}
			}

			_prevExFlag_MeshGroup = targetMeshGroup;
			_prevExFlag_Modifier = null;
			_prevExFlag_ParamSetGroup = null;
			_prevExFlag_AnimClip = targetAnimClip;
			_prevExFlag_AnimTimeline = null;
			_prevExFlag_EditMode = false;
			_prevExFlag_IsDisabledExRunAvailable = false;

			if (targetMeshGroup == null)
			{
				//Debug.LogError("Target MeshGroup is Null");
				return;
			}

			RefreshRenderUnitAndBoneExFlag_Recursive(	targetMeshGroup, targetMeshGroup, 
														false, Editor._exModObjOption_UpdateByOtherMod, 
														null, null);
		}

		private void RefreshRenderUnitAndBoneExFlag_Recursive(	apMeshGroup curMeshGroup, apMeshGroup rootMeshGroup,
																bool isEditMode, //편집 모드인가
																bool isCalculatedIfNotEdited, //옵션에 따라, 편집중이 아닌 객체도 다른 모디파이어로 업데이트 가능한가
																//apModifierParamSetGroup modPSG_ifMeshGroupMode,
																//apAnimTimeline animTimeline_ifAnimClipMode,
																Dictionary<apRenderUnit, bool> syncRenderUnits,
																Dictionary<apBone, bool> syncBones

																)
		{
			apRenderUnit curRenderUnit = null;
			apBone curBone = null;

			
			

			int nRenderUnits = curMeshGroup._renderUnits_All != null ? curMeshGroup._renderUnits_All.Count : 0;
			int nBones = curMeshGroup._boneList_All != null ? curMeshGroup._boneList_All.Count : 0;

			if (!isEditMode || (syncRenderUnits == null || syncBones == null))
			{
				//편집 모드가 아닐때 > 모두 해제
				//RenderUnit (MeshTransform / MeshGroupTransform)을 체크하자

				for (int i = 0; i < nRenderUnits; i++)
				{
					curRenderUnit = curMeshGroup._renderUnits_All[i];
					curRenderUnit._exCalculateMode = apRenderUnit.EX_CALCULATE.Enabled_Run;//모두 해제
				}

				for (int i = 0; i < nBones; i++)
				{
					curBone = curMeshGroup._boneList_All[i];
					curBone._exCalculateMode = apBone.EX_CALCULATE.Enabled_Run;//모두 해제
				}
			}
			else
			{
				//편집 모드일때

				for (int i = 0; i < nRenderUnits; i++)
				{
					curRenderUnit = curMeshGroup._renderUnits_All[i];

					if (syncRenderUnits.ContainsKey(curRenderUnit))
					{
						curRenderUnit._exCalculateMode = apRenderUnit.EX_CALCULATE.Enabled_Edit;
					}
					else if (isCalculatedIfNotEdited)
					{
						//편집 중이 아닌 Modifier를 적용해야한다면 (옵션에 의해서)
						curRenderUnit._exCalculateMode = apRenderUnit.EX_CALCULATE.Disabled_ExRun;
					}
					else
					{
						//그냥 업데이트에서 제외
						curRenderUnit._exCalculateMode = apRenderUnit.EX_CALCULATE.Disabled_NotEdit;
					}
				}

				for (int i = 0; i < nBones; i++)
				{
					curBone = curMeshGroup._boneList_All[i];

					if(syncBones.ContainsKey(curBone))
					{
						//편집중인 Bone이다.
						curBone._exCalculateMode = apBone.EX_CALCULATE.Enabled_Edit;
					}
					else if(isCalculatedIfNotEdited)
					{
						//편집 중이 아닌 Modifier를 적용해야한다면 (옵션에 의해서)
						curBone._exCalculateMode = apBone.EX_CALCULATE.Disabled_ExRun;
					}
					else
					{
						//그냥 업데이트에서 제외
						curBone._exCalculateMode = apBone.EX_CALCULATE.Disabled_NotEdit;
					}
				}
			}

			//자식 메시 그룹에 대해서도 동일하게 처리
			if(curMeshGroup._childMeshGroupTransforms != null && curMeshGroup._childMeshGroupTransforms.Count > 0)
			{
				apTransform_MeshGroup childMeshGroupTF = null;
				for (int iChild = 0; iChild < curMeshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					childMeshGroupTF = curMeshGroup._childMeshGroupTransforms[iChild];
					if(childMeshGroupTF != null 
						&& childMeshGroupTF._meshGroup != null 
						&& childMeshGroupTF._meshGroup != curMeshGroup
						&& childMeshGroupTF._meshGroup != rootMeshGroup)
					{
						RefreshRenderUnitAndBoneExFlag_Recursive(childMeshGroupTF._meshGroup, rootMeshGroup,
																isEditMode,
																isCalculatedIfNotEdited,
																syncRenderUnits,
																syncBones);
					}
				}
			}
		}



		/// <summary>
		/// [MeshGroup - Rigging 모디파이어에서] 바인딩 모드를 전환한다.
		/// </summary>
		private void ToggleRigEditBinding()
		{
			
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _meshGroup == null 
				|| _modifier == null
				|| _subEditedParamSetGroup == null)
			{
				//편집이 가능하지 않으면 넘어감
				return;
			}
			
			bool isRiggingModifier = (_modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging);

			//변경 22.5.15 : 모디파이어 종류에 상관없이 토글
			if(_exclusiveEditing == EX_EDIT.ExOnly_Edit)
			{
				_exclusiveEditing = EX_EDIT.None;//ON > OFF
			}
			else
			{
				_exclusiveEditing = EX_EDIT.ExOnly_Edit;//OFF > ON
			}


			if (isRiggingModifier)
			{
				//1. Rigging 타입의 Modifier인 경우
				//_rigEdit_isBindingEdit = !_rigEdit_isBindingEdit;//이전

				
				_rigEdit_isTestPosing = false;

				#region [미사용 코드]
				////작업중인 Modifier 외에는 일부 제외를 하자
				//if (_rigEdit_isBindingEdit)
				//{
				//	MeshGroup._modifierStack.SetExclusiveModifierInEditing(_modifier, SubEditedParamSetGroup, apModifierStack.OTHER_MOD_RUN_OPTION.Disabled);

				//	//변경 3.23 : 선택 잠금을 무조건 켜는게 아니라, 에디터 설정에 따라 켤지 말지 결정한다.
				//	//true 또는 변경 없음 (false가 아님)
				//	if (Editor._isSelectionLockOption_RiggingPhysics)
				//	{
				//		_isSelectionLock = true;
				//	}

				//}
				//else
				//{
				//	if (MeshGroup != null)
				//	{
				//		//Exclusive 모두 해제
				//		MeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();

				//		//이전
				//		//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(MeshGroup, false, true, true);

				//		//변경 20.4.13
				//		Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	MeshGroup, 
				//															apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff, 
				//															apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);

				//		//RefreshMeshGroupExEditingFlags(MeshGroup, null, null, null, false);//아래로 옮김 21.2.15


				//	}
				//	_isSelectionLock = false;
				//} 
				#endregion


				//변경 22.5.15
				AutoRefreshModifierExclusiveEditing();

				//선택 잠금 켜기/끄기
				if(_exclusiveEditing == EX_EDIT.ExOnly_Edit)
				{
					//선택 잠금을 무조건 켜는게 아니라, 에디터 설정에 따라 켤지 말지 결정한다.
					//true 또는 변경 없음 (false가 아님)
					if (Editor._isSelectionLockOption_RiggingPhysics)
					{
						_isSelectionLock = true;
					}
				}
				else
				{
					_isSelectionLock = false;
				}

				_rigEdit_BrushToolMode = RIGGING_BRUSH_TOOL_MODE.None;
				Editor.Gizmos.EndBrush();

				AutoSelectModMeshOrModBone();//<<추가

				//추가 19.7.27 : 본의 RigLock을 해제
				Editor.Controller.ResetBoneRigLock(MeshGroup);

				
			}
			else
			{
				//2. 일반 Modifier일때
				//SetModifierExclusiveEditing(nextResult);

				AutoRefreshModifierExclusiveEditing();

				if (_exclusiveEditing == EX_EDIT.ExOnly_Edit)
				{
					//변경 3.23 : 선택 잠금을 무조건 켜는게 아니라, 에디터 설정에 따라 켤지 말지 결정한다.
					//true 또는 변경 없음 (false가 아님)
					//모디파이어의 종류에 따라서 다른 옵션을 적용
					if (_modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Physic)
					{
						if (Editor._isSelectionLockOption_RiggingPhysics)
						{
							_isSelectionLock = true;//처음 Editing 작업시 Lock을 거는 것으로 변경
						}
					}
					else if (_modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Morph ||
						_modifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
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
				else
				{
					_isSelectionLock = false;//Editing 해제시 Lock 해제

					//편집 모드 종료시 버텍스/핀은 선택 해제
					_modRenderVert_Main = null;
					_modRenderVerts_All.Clear();
					_modRenderVerts_Weighted.Clear();

					_modRenderPin_Main = null;
					_modRenderPins_All.Clear();
					_modRenderPins_Weighted.Clear();
				}
			}

			//변경 21.2.15
			//RefreshMeshGroupExEditingFlags(false); > 위의 AutoRefreshModifierExclusiveEditing에서 호출된 코드다.


			Editor.Gizmos.OnSelectedObjectsChanged();//추가 20.7.5 : 편집 모드 전후로 이게 호출되어야 한다.

			Editor.RefreshControllerAndHierarchy(false);
		}

		


		public void SetModifierSelectionLock(bool isLock)
		{
			if (_selectionType != SELECTION_TYPE.MeshGroup
				|| _modifier == null
				//|| _exEditKeyValue == EX_EDIT_KEY_VALUE.None
				)
			{
				_isSelectionLock = false;
				return;
			}
			_isSelectionLock = isLock;
		}

		





		//---------------------------------------------------------------
		//애니메이션 편집 모드 변경

		private void SetAnimEditingToggle()
		{
			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//>> Off
				//_isAnimEditing = false;
				_exAnimEditingMode = EX_EDIT.None;
				//_isAnimAutoKey = false;
				_isAnimSelectionLock = false;
			}
			else
			{
				if (IsAnimEditable)
				{
					//_isAnimEditing = true;//<<편집 시작!
					//_isAnimAutoKey = false;
					_exAnimEditingMode = EX_EDIT.ExOnly_Edit;//<<배타적 Mod 선택이 기본값이다.

					//변경 3.23 : 선택 잠금을 무조건 켜는게 아니라, 옵션에 따라 켜거나 그대로 둘지 결정한다.
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						if (Editor._isSelectionLockOption_ControlParamTimeline)
						{
							_isAnimSelectionLock = true;//기존의 False에서 True로 변경
						}
					}
					else
					{
						if (_subAnimTimeline._linkedModifier != null)
						{
							if (_subAnimTimeline._linkedModifier.ModifierType == apModifierBase.MODIFIER_TYPE.AnimatedMorph)
							{
								if (Editor._isSelectionLockOption_Morph)
								{
									_isAnimSelectionLock = true;//기존의 False에서 True로 변경
								}
							}
							else
							{
								if (Editor._isSelectionLockOption_Transform)
								{
									_isAnimSelectionLock = true;//기존의 False에서 True로 변경
								}
							}
						}
						else
						{
							//에러 : 모디파이어를 알 수 없다.
							_isAnimSelectionLock = true;//기존의 False에서 True로 변경
						}
					}


					bool isVertexTarget = false;
					bool isControlParamTarget = false;
					bool isTransformTarget = false;
					bool isBoneTarget = false;

					//현재 객체가 현재 Timeline에 맞지 않다면 선택을 해제해야한다.
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						isControlParamTarget = true;
					}
					else if (_subAnimTimeline._linkedModifier != null)
					{
						if ((int)(_subAnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
						{
							isVertexTarget = true;
							isTransformTarget = true;
						}
						else if (_subAnimTimeline._linkedModifier.IsTarget_Bone)
						{
							isTransformTarget = true;
							isBoneTarget = true;
						}
						else
						{
							isTransformTarget = true;
						}
					}
					else
					{
						//?? 뭘 선택할까요.
						Debug.LogError("Anim Toggle Error : Animation Modifier 타입인데 Modifier가 연결 안됨");
					}

					if (!isVertexTarget)
					{
						//변경 20.6.29 : Mod와 통합되었다.
						_modRenderVert_Main = null;
						_modRenderVerts_All.Clear();
						_modRenderVerts_Weighted.Clear();

						//추가 22.4.6
						_modRenderPin_Main = null;
						_modRenderPins_All.Clear();
						_modRenderPins_Weighted.Clear();
					}
					if (!isControlParamTarget)
					{
						//변경 20.6.9 : 래핑
						_subObjects.ClearControlParam();
					}
					if (!isTransformTarget)
					{
						//변경 20.5.27 : 래핑
						_subObjects.ClearTF();
					}
					if (!isBoneTarget)
					{
						//변경 20.5.27 : 래핑
						_subObjects.ClearBone();
					}


				}
			}


			//RefreshAnimEditing(true);//이전
			AutoRefreshModifierExclusiveEditing();//변경 22.5.15

			Editor.RefreshControllerAndHierarchy(false);
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.Info, null, null);

			Editor.Gizmos.OnSelectedObjectsChanged();//20.7.5 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.
		}

		public bool SetAnimExclusiveEditing_Tmp(EX_EDIT exEditing, bool isGizmoReset)
		{
			if (!IsAnimEditable && exEditing != EX_EDIT.None)
			{
				//편집중이 아니라면 None으로 강제한다.
				exEditing = EX_EDIT.None;
				return false;
			}


			if (_exAnimEditingMode == exEditing)
			{
				return true;
			}

			_exAnimEditingMode = exEditing;
			

			//Editing 상태에 따라 Refresh 코드가 다르다
			if (ExAnimEditingMode != EX_EDIT.None)
			{
				//현재 선택한 타임라인에 따라서 Modifier를 On/Off할지 결정한다.
				bool isExclusiveActive = false;
				if (_subAnimTimeline != null)
				{
					if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						if (_subAnimTimeline._linkedModifier != null && _animClip._targetMeshGroup != null)
						{
							if (ExAnimEditingMode == EX_EDIT.ExOnly_Edit)
							{
								//현재의 AnimTimeline에 해당하는 ParamSet만 선택하자
								List<apModifierParamSetGroup> exParamSetGroups = new List<apModifierParamSetGroup>();
								List<apModifierParamSetGroup> linkParamSetGroups = _subAnimTimeline._linkedModifier._paramSetGroup_controller;
								for (int iP = 0; iP < linkParamSetGroups.Count; iP++)
								{
									apModifierParamSetGroup linkPSG = linkParamSetGroups[iP];
									if (linkPSG._keyAnimTimeline == _subAnimTimeline &&
										linkPSG._keyAnimClip == _animClip)
									{
										exParamSetGroups.Add(linkPSG);
									}
								}

								//이전
								//_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_Anim(
								//											_subAnimTimeline._linkedModifier,
								//											exParamSetGroups,
								//											Editor._modLockOption_ColorPreview, 
								//											Editor._exModObjOption_UpdateByOtherMod);

								//변경 22.5.14
								apModifierStack.OTHER_MOD_RUN_OPTION otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.Disabled;
								if(Editor._exModObjOption_UpdateByOtherMod)
								{
									//다중 편집 허용 중이라면 (D키)
									otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveAllPossible;
								}
								else if(Editor._modLockOption_ColorPreview)
								{
									//다중 편집은 아니지만 색상 미리보기라도 지원한다면
									otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveColorOnly;
								}

								_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_Anim(
																			_subAnimTimeline._linkedModifier,
																			exParamSetGroups,
																			otherModRunOption);


								isExclusiveActive = true;
							}
							//TODO : 이것도 확인해야한다.
							//else if (ExAnimEditingMode == EX_EDIT.General_Edit)
							//{
							//	//추가 : General Edit 모드
							//	//선택한 것과 허용되는 Modifier는 모두 허용한다.
							//	_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(
							//												_subAnimTimeline._linkedModifier,
							//												_animClip,
							//												isModLock_ColorUpdate,
							//												isModLock_OtherMod);
							//	isExclusiveActive = true;
							//}
						}
					}
				}

				if (!isExclusiveActive)
				{
					//Modifier와 연동된게 아니라면
					if (_animClip._targetMeshGroup != null)
					{
						_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
						//RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
					}
				}
				//else
				//{
				//	//RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, _subAnimTimeline._linkedModifier, null, _animClip, false);//<<추가
				//}
			}
			else
			{
				//모든 Modifier의 Exclusive 선택을 해제하고 모두 활성화한다.
				if (_animClip._targetMeshGroup != null)
				{
					_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();
					//RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
				}
			}

			//변경 21.2.15
			RefreshMeshGroupExEditingFlags(false);

			Editor.Gizmos.OnSelectedObjectsChanged();//20.7.5 기즈모 이 함수를 호출해야 기즈모 시작시 선택이 제대로 처리된다.

			return true;
		}


		#region [미사용 코드] 삭제 22.5.15
		///// <summary>
		///// Mod Lock을 갱신한다.
		///// Animation Clip 선택시 이걸 호출한다.
		///// SetAnimEditingLayerLockToggle() 함수를 다시 호출한 것과 같다.
		///// </summary>
		//public void RefreshAnimEditingLayerLock()
		//{
		//	if (_animClip == null ||
		//		SelectionType != SELECTION_TYPE.Animation)
		//	{
		//		return;
		//	}

		//	if (ExAnimEditingMode == EX_EDIT.None)
		//	{
		//		_exAnimEditingMode = EX_EDIT.None;
		//	}

		//	RefreshAnimEditing(true);
		//}

		///// <summary>
		///// 애니메이션 작업 도중 타임라인 추가/삭제, 키프레임 추가/삭제/이동과 같은 변동사항이 있을때 호출되어야 하는 함수
		///// </summary>
		//public void RefreshAnimEditing(bool isGizmoEventReset)
		//{
		//	//Debug.Log("RefreshAnimEditing");
		//	if (_animClip == null)
		//	{
		//		return;
		//	}

		//	//Editing 상태에 따라 Refresh 코드가 다르다
		//	if (ExAnimEditingMode != EX_EDIT.None)
		//	{

		//		//bool isModLock_ColorUpdate = Editor.GetModLockOption_ColorPreview(ExAnimEditingMode);
		//		//bool isModLock_ColorUpdate = Editor.GetModLockOption_ColorPreview();//변경 21.2.13
		//		//bool isModLock_OtherMod = Editor.GetModLockOption_CalculateIfNotAddedOther(ExAnimEditingMode);//삭제 21.2.17


		//		//현재 선택한 타임라인에 따라서 Modifier를 On/Off할지 결정한다.
		//		bool isExclusiveActive = false;
		//		if (_subAnimTimeline != null)
		//		{
		//			if (_subAnimTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
		//			{
		//				if (_subAnimTimeline._linkedModifier != null && _animClip._targetMeshGroup != null)
		//				{
		//					if (ExAnimEditingMode == EX_EDIT.ExOnly_Edit)
		//					{
		//						//현재의 AnimTimeline에 해당하는 ParamSet만 선택하자
		//						List<apModifierParamSetGroup> exParamSetGroups = new List<apModifierParamSetGroup>();
		//						List<apModifierParamSetGroup> linkParamSetGroups = _subAnimTimeline._linkedModifier._paramSetGroup_controller;
		//						for (int iP = 0; iP < linkParamSetGroups.Count; iP++)
		//						{
		//							apModifierParamSetGroup linkPSG = linkParamSetGroups[iP];
		//							if (linkPSG._keyAnimTimeline == _subAnimTimeline &&
		//								linkPSG._keyAnimClip == _animClip)
		//							{
		//								exParamSetGroups.Add(linkPSG);
		//							}
		//						}

		//						//Debug.Log("Set Anim Editing > Exclusive Enabled [" + _subAnimTimeline._linkedModifier.DisplayName + "]");

		//						//이전
		//						//_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_Anim(
		//						//											_subAnimTimeline._linkedModifier,
		//						//											exParamSetGroups,
		//						//											Editor._modLockOption_ColorPreview, 
		//						//											Editor._exModObjOption_UpdateByOtherMod);

		//						//변경 22.5.14
		//						apModifierStack.OTHER_MOD_RUN_OPTION otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.Disabled;
		//						if(Editor._exModObjOption_UpdateByOtherMod)
		//						{
		//							//다중 편집 허용 중이라면 (D키)
		//							otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveAllPossible;
		//						}
		//						else if(Editor._modLockOption_ColorPreview)
		//						{
		//							//다중 편집은 아니지만 색상 미리보기라도 지원한다면
		//							otherModRunOption = apModifierStack.OTHER_MOD_RUN_OPTION.ActiveColorOnly;
		//						}


		//						_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_Anim(
		//																	_subAnimTimeline._linkedModifier,
		//																	exParamSetGroups,
		//																	otherModRunOption);

		//						isExclusiveActive = true;
		//					}
		//					//TODO : 이거 확인해야한다.
		//					//else if (ExAnimEditingMode == EX_EDIT.General_Edit)
		//					//{
		//					//	//추가 : General Edit 모드
		//					//	//선택한 것과 허용되는 Modifier는 모두 허용한다.
		//					//	_animClip._targetMeshGroup._modifierStack.SetExclusiveModifierInEditing_MultipleParamSetGroup_General(
		//					//												_subAnimTimeline._linkedModifier,
		//					//												_animClip,
		//					//												isModLock_ColorUpdate,
		//					//												isModLock_OtherMod);
		//					//	isExclusiveActive = true;
		//					//}
		//				}
		//			}
		//		}

		//		if (!isExclusiveActive)
		//		{
		//			//Modifier와 연동된게 아니라면
		//			if (_animClip._targetMeshGroup != null)
		//			{
		//				_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();

		//				//이전
		//				//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup, false, true, true);

		//				//변경 20.4.13
		//				Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	_animClip._targetMeshGroup,
		//																	apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff,
		//																	apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);


		//				//RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
		//			}
		//		}
		//		//else
		//		//{
		//		//	RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, _subAnimTimeline._linkedModifier, null, _animClip, false);//<<추가
		//		//}
		//	}
		//	else
		//	{
		//		//모든 Modifier의 Exclusive 선택을 해제하고 모두 활성화한다.
		//		if (_animClip._targetMeshGroup != null)
		//		{
		//			_animClip._targetMeshGroup._modifierStack.ActiveAllModifierFromExclusiveEditing();

		//			//이전
		//			//Editor.Controller.SetMeshGroupTmpWorkVisibleReset(_animClip._targetMeshGroup, false, true, true);

		//			//변경 20.4.13
		//			Editor.Controller.SetMeshGroupTmpWorkVisibleReset(	_animClip._targetMeshGroup, 
		//																apEditorController.RESET_VISIBLE_ACTION.OnlyRefreshIfOptionIsOff, 
		//																apEditorController.RESET_VISIBLE_TARGET.RenderUnitsAndBones);



		//			//RefreshMeshGroupExEditingFlags(_animClip._targetMeshGroup, null, null, null, false);//<<추가
		//		}
		//	}

		//	//변경 21.2.15
		//	RefreshMeshGroupExEditingFlags(false);

		//	AutoSelectAnimTimelineLayer(false);
		//	Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Info, null, null);
		//	SetAnimClipGizmoEvent(isGizmoEventReset);
		//} 
		#endregion





		/// <summary>
		/// Is Auto Scroll 옵션이 켜져있으면 스크롤을 자동으로 선택한다.
		/// 재생중에도 스크롤을 움직인다.
		/// </summary>
		public void SetAutoAnimScroll()
		{
			int curFrame = 0;
			int startFrame = 0;
			int endFrame = 0;
			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _timlineGUIWidth <= 0)
			{
				return;
			}
			if (!_animClip.IsPlaying_Editor)
			{
				return;
			}

			curFrame = _animClip.CurFrame;
			startFrame = _animClip.StartFrame;
			endFrame = _animClip.EndFrame;

			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;



			//화면에 보여지는 프레임 범위는?
			
			int startFrame_Visible = (int)((float)(_scroll_Timeline.x / (float)widthPerFrame) + startFrame);
			int endFrame_Visible = (int)(((float)_timlineGUIWidth / (float)widthPerFrame) + startFrame_Visible);

			//앞에 여백이 있어서 보여지는 범위가 조금 다르다
			int visibleBias = (int)(((float)apTimelineGL.X_OFFSET/(float)widthPerFrame) + 0.5f);//앞에 약간의 여백을 두고 렌더링이 되므로
			startFrame_Visible -= visibleBias;
			endFrame_Visible -= visibleBias;

			//Margin과 관계없이 End가 영역 안에 들어왔다면 스크롤이 되지 않는다.
			if(startFrame_Visible <= startFrame && endFrame <= endFrame_Visible)
			{
				//Debug.Log("오토 스크롤이 발생하지 않는 애니메이션 범위 [ 소스 : " 
				//	+ startFrame + "~" + endFrame 
				//	+ " / 화면 구역 : " + startFrame_Visible + "~" + endFrame_Visible + " ]");
				return;
			}

			int marginFrame = 10;
			int targetFrame = -1;


			startFrame_Visible += marginFrame;
			endFrame_Visible -= marginFrame;

			//"이동해야할 범위와 실제로 이동되는 범위는 다르다"
			if (curFrame < startFrame_Visible)
			{
				//커서가 화면 왼쪽에 붙도록 하자
				targetFrame = curFrame - marginFrame;
			}
			else if (curFrame > endFrame_Visible)
			{
				//커서가 화면 오른쪽에 붙도록 하자
				targetFrame = (curFrame + marginFrame) - (int)((float)_timlineGUIWidth / (float)widthPerFrame);
			}
			else
			{
				return;
			}

			targetFrame -= startFrame;
			float nextScroll = Mathf.Clamp((targetFrame * widthPerFrame), 0, widthForScrollFrame);

			_scroll_Timeline.x = nextScroll;
		}

		/// <summary>
		/// 마우스 편집 중에 스크롤을 자동으로 해야하는 경우
		/// AnimClip의 프레임은 수정하지 않는다. (마우스 위치에 따른 TargetFrame을 넣어주자)
		/// </summary>
		public void AutoAnimScrollWithoutFrameMoving(int requestFrame, int marginFrame)
		{

			if (_selectionType != SELECTION_TYPE.Animation || _animClip == null || _timlineGUIWidth <= 0)
			{
				return;
			}

			int startFrame = _animClip.StartFrame;
			int endFrame = _animClip.EndFrame;

			if (requestFrame < startFrame)
			{
				requestFrame = startFrame;
			}
			else if (requestFrame > endFrame)
			{
				requestFrame = endFrame;
			}

			int widthPerFrame = Editor.WidthPerFrameInTimeline;
			int nFrames = Mathf.Max((endFrame - startFrame) + 1, 1);
			int widthForTotalFrame = nFrames * widthPerFrame;
			int widthForScrollFrame = widthForTotalFrame;

			//화면에 보여지는 프레임 범위는?
			int startFrame_Visible = (int)((float)(_scroll_Timeline.x / (float)widthPerFrame) + startFrame);
			int endFrame_Visible = (int)(((float)_timlineGUIWidth / (float)widthPerFrame) + startFrame_Visible);

			//int marginFrame = 10;


			startFrame_Visible += marginFrame;
			endFrame_Visible -= marginFrame;

			int targetFrame = 0;

			//"이동해야할 범위와 실제로 이동되는 범위는 다르다"
			if (requestFrame < startFrame_Visible)
			{
				//커서가 화면 왼쪽에 붙도록 하자
				targetFrame = requestFrame - marginFrame;
			}
			else if (requestFrame > endFrame_Visible)
			{
				//커서가 화면 오른쪽에 붙도록 하자
				targetFrame = (requestFrame + marginFrame) - (int)((float)_timlineGUIWidth / (float)widthPerFrame);
			}
			else
			{
				return;
			}

			targetFrame -= startFrame;
			float nextScroll = Mathf.Clamp((targetFrame * widthPerFrame), 0, widthForScrollFrame);

			_scroll_Timeline.x = nextScroll;
		}

		private const float ANIM_SCROLL_BAR_BTN_SIZE_Y = 50.0f;
		private const float ANIM_SCROLL_BAR_BTN_SIZE_X = 20.0f;

		


		
		
		// 본 편집 모드 변경
		//----------------------------------------------------------------
		/// <summary>
		/// isEditing : Default Matrix를 수정하는가
		/// isBoneMenu : 현재 Bone Menu인가
		/// </summary>
		/// <param name="isEditing"></param>
		/// <param name="isBoneMenu"></param>
		public void SetBoneEditing(bool isEditing, bool isBoneMenu)
		{
			//bool isChanged = _isBoneDefaultEditing != isEditing;

			_isBoneDefaultEditing = isEditing;

			


			if (_isBoneDefaultEditing)
			{
				SetBoneEditMode(BONE_EDIT_MODE.SelectAndTRS, isBoneMenu);
				//Debug.LogError("TODO : Default Bone Tranform을 활성화할 때에는 다른 Rig Modifier를 꺼야한다.");

				//Editor.Gizmos.LinkObject()
			}
			else
			{
				if (isBoneMenu)
				{
					SetBoneEditMode(BONE_EDIT_MODE.SelectOnly, isBoneMenu);
				}
				else
				{
					SetBoneEditMode(BONE_EDIT_MODE.None, isBoneMenu);
				}
				//Debug.LogError("TODO : Default Bone Tranform을 종료할 때에는 다른 Rig Modifier를 켜야한다.");
			}
		}

		public void SetBoneEditMode(BONE_EDIT_MODE boneEditMode, bool isBoneMenu)
		{
			_boneEditMode = boneEditMode;

			if (!_isBoneDefaultEditing)
			{
				if (isBoneMenu)
				{
					_boneEditMode = BONE_EDIT_MODE.SelectOnly;
				}
				else
				{
					_boneEditMode = BONE_EDIT_MODE.None;
				}
			}

			Editor.Controller.SetBoneEditInit();
			//Gizmo 이벤트를 설정하자
			switch (_boneEditMode)
			{
				case BONE_EDIT_MODE.None:
					Editor.Gizmos.Unlink();
					break;

				case BONE_EDIT_MODE.SelectOnly:
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Bone_SelectOnly());
					break;

				case BONE_EDIT_MODE.SelectAndTRS:
					//Select에서는 Gizmo 이벤트를 받는다.
					//Transform 제어를 해야하기 때문
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_Bone_Default());
					break;

				case BONE_EDIT_MODE.Add:
					Editor.Gizmos.Unlink();
					break;

				case BONE_EDIT_MODE.Link:
					Editor.Gizmos.Unlink();
					break;
			}
		}

		/// <summary>
		/// Rigging시 Pose Test를 하는지 여부를 설정한다.
		/// 모든 MeshGroup에 대해서 설정한다.
		/// _rigEdit_isTestPosing값을 먼저 설정한다.
		/// </summary>
		public void SetBoneRiggingTest()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				meshGroup.SetBoneRiggingTest(_rigEdit_isTestPosing);
			}
		}

		/// <summary>
		/// Rigging시, Test중인 Pose를 리셋한다.
		/// </summary>
		public void ResetRiggingTestPose()
		{
			if (Editor._portrait == null)
			{
				return;
			}
			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				meshGroup.ResetRiggingTestPose();
			}
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}




		//Pin 모드를 변경한다.
		

		public void SetPinMode(apEditor.MESH_EDIT_PIN_TOOL_MODE pinMode)
		{
			if (Editor._meshEditMode_Pin_ToolMode != pinMode)
			{
				Editor._meshEditMode_Pin_ToolMode = pinMode;
				RefreshPinModeEvent();
			}
		}


		private void RefreshPinModeEvent()
		{
			Editor.Gizmos.Unlink();

			//TODO : 기즈모 이벤트 추가할 것
			switch (Editor._meshEditMode_Pin_ToolMode)
			{
				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Select:
					//Pin 편집 모드 중 [선택] 모드
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshPinEdit_Default());
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Add:
					//Pin 편집 모드 중 [추가] 모드
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Link:
					//Pin 편집 모드 중 [연결] 모드
					break;

				case apEditor.MESH_EDIT_PIN_TOOL_MODE.Test:
					//Pin 편집 모드 중 [테스트] 모드
					Editor.Gizmos.LinkObject(Editor.GizmoController.GetEventSet_MeshPinEdit_Test());
					break;
			}
		}


		// 객체 통계 함수들
		//-------------------------------------------------------------------------------
		/// <summary>
		/// 객체 통계를 다시 계산할 필요가 있을때 호출한다.
		/// </summary>
		public void SetStatisticsRefresh()
		{
			_isStatisticsNeedToRecalculate = true;
		}


		public void CalculateStatistics()
		{
			if(!_isStatisticsNeedToRecalculate)
			{
				//재계산이 필요없으면 생략
				return;
			}
			_isStatisticsNeedToRecalculate = false;

			_isStatisticsAvailable = false;
			_statistics_NumMesh = 0;
			_statistics_NumVert = 0;
			_statistics_NumEdge = 0;
			_statistics_NumTri = 0;
			_statistics_NumClippedMesh = 0;
			_statistics_NumClippedVert = 0;

			_statistics_NumTimelineLayer = -1;
			_statistics_NumKeyframes = -1;
			_statistics_NumBones = 0;

			if(Editor._portrait == null)
			{	
				return;
			}
			
			//apMesh mesh = null;
			//apTransform_Mesh meshTransform = null;
			switch (_selectionType)
			{
				case SELECTION_TYPE.Overall:
					{
						if (_rootUnit == null || _rootUnit._childMeshGroup == null)
						{
							return;
						}

						CalculateStatisticsMeshGroup(_rootUnit._childMeshGroup);

						if (_curRootUnitAnimClip != null)
						{
							_statistics_NumTimelineLayer = 0;
							_statistics_NumKeyframes = 0;

							apAnimTimeline timeline = null;
							for (int i = 0; i < _curRootUnitAnimClip._timelines.Count; i++)
							{
								timeline = _curRootUnitAnimClip._timelines[i];

								_statistics_NumTimelineLayer += timeline._layers.Count;
								for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
								{
									_statistics_NumKeyframes += timeline._layers[iLayer]._keyframes.Count;
								}
							}
						}
					}
					break;

				case SELECTION_TYPE.Mesh:
					{
						if (_mesh == null)
						{
							return;
						}

						_statistics_NumMesh = -1;//<<어차피 1개인데 이건 출력 생략
						_statistics_NumClippedVert = -1;
						_statistics_NumVert = _mesh._vertexData.Count;
						_statistics_NumEdge = _mesh._edges.Count;
						_statistics_NumTri = (_mesh._indexBuffer.Count / 3);
						_statistics_NumBones = 0;
					}
					
					break;

				case SELECTION_TYPE.MeshGroup:
					{
						if (_meshGroup == null)
						{
							return;
						}

						CalculateStatisticsMeshGroup(_meshGroup);
					}
					break;

				case SELECTION_TYPE.Animation:
					{
						if(_animClip == null)
						{
							return;
						}

						if(_animClip._targetMeshGroup == null)
						{
							return;
						}

						CalculateStatisticsMeshGroup(_animClip._targetMeshGroup);

						_statistics_NumTimelineLayer = 0;
						_statistics_NumKeyframes = 0;

						apAnimTimeline timeline = null;
						for (int i = 0; i < _animClip._timelines.Count; i++)
						{
							timeline = _animClip._timelines[i];

							_statistics_NumTimelineLayer += timeline._layers.Count;
							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								_statistics_NumKeyframes += timeline._layers[iLayer]._keyframes.Count;
							}
						}
						
					}
					break;

				default:
					return;
			}

			if(_statistics_NumClippedMesh == 0)
			{
				_statistics_NumClippedMesh = -1;
				_statistics_NumClippedVert = -1;
			}

			_isStatisticsAvailable = true;
		}

		private void CalculateStatisticsMeshGroup(apMeshGroup targetMeshGroup)
		{
			if (targetMeshGroup == null)
			{
				return;
			}

			apMesh mesh = null;
			apTransform_Mesh meshTransform = null;

			for (int i = 0; i < targetMeshGroup._childMeshTransforms.Count; i++)
			{
				meshTransform = targetMeshGroup._childMeshTransforms[i];
				if (meshTransform == null)
				{
					continue;
				}

				mesh = meshTransform._mesh;
				if (mesh == null)
				{
					continue;
				}
				_statistics_NumMesh += 1;
				_statistics_NumVert += mesh._vertexData.Count;
				_statistics_NumEdge += mesh._edges.Count;
				_statistics_NumTri += (mesh._indexBuffer.Count / 3);

				//클리핑이 되는 경우 Vert를 따로 계산해준다.
				//Parent도 같이 포함한다. (렌더링은 같이 되므로)
				if (meshTransform._isClipping_Child)
				{
					_statistics_NumClippedMesh +=1;
					_statistics_NumClippedVert += mesh._vertexData.Count;

					if(meshTransform._clipParentMeshTransform != null &&
						meshTransform._clipParentMeshTransform._mesh != null)
					{
						_statistics_NumClippedVert += meshTransform._clipParentMeshTransform._mesh._vertexData.Count;
					}
				}
			}

			//추가 19.12.25 : 본 개수도 표시
			_statistics_NumBones += targetMeshGroup._boneList_All.Count;

			//Child MeshGroupTransform이 있으면 재귀적으로 호출하자
			for (int i = 0; i < targetMeshGroup._childMeshGroupTransforms.Count; i++)
			{
				CalculateStatisticsMeshGroup(targetMeshGroup._childMeshGroupTransforms[i]._meshGroup);
			}
		}


		public bool IsStatisticsCalculated		{  get { return _isStatisticsAvailable; } }
		public int Statistics_NumMesh			{  get { return _statistics_NumMesh; } }
		public int Statistics_NumVertex			{  get { return _statistics_NumVert; } }
		public int Statistics_NumEdge			{  get { return _statistics_NumEdge; } }
		public int Statistics_NumTri			{  get { return _statistics_NumTri; } }
		public int Statistics_NumClippedMesh	{  get { return _statistics_NumClippedMesh; } }
		public int Statistics_NumClippedVertex	{  get { return _statistics_NumClippedVert; } }
		public int Statistics_NumTimelineLayer	{  get { return _statistics_NumTimelineLayer; } }
		public int Statistics_NumKeyframe		{  get { return _statistics_NumKeyframes; } }
		public int Statistics_NumBone			{  get { return _statistics_NumBones; } }


		public bool IsSelectionLockGUI
		{
			get
			{
				if(_selectionType == SELECTION_TYPE.Animation)
				{
					return IsAnimSelectionLock;
				}
				else if(_selectionType == SELECTION_TYPE.MeshGroup)
				{
					return IsSelectionLock;
				}
				return false;
			}
		}




		// 추가 : Bone IK Update/Rendering 옵션을 계산한다.
		//---------------------------------------------------------------------------------------------
		/// <summary>
		/// Bone의 IK Matrix를 업데이트할 수 있는가
		/// </summary>
		/// <returns></returns>
		public bool IsBoneIKMatrixUpdatable
		{
			get
			{
				switch (_selectionType)
				{
					case SELECTION_TYPE.Overall:
						return true;

					case SELECTION_TYPE.MeshGroup:
						if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier)
						{
							if (Modifier != null)
							{
								if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
								{
									//리깅 모디파이어 타입인 경우
									//그냥 IK 불가
									return false;
								}
								else
								{
									if (ExEditingMode == EX_EDIT.None)
									{
										//편집 모드가 아닐땐 True
										return true;
									}
								}
							}
						}
						else if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
						{
							return true;
						}
						else
						{
							//변경 20.7.15 : Bone 탭에서 본 편집 모드가 아니면 IK 적용
							//(원래는 항상 false)
							if(!IsBoneDefaultEditing)
							{
								return true;
							}
						}
						return false;

					case SELECTION_TYPE.Animation:
						if (ExAnimEditingMode == EX_EDIT.None)
						{
							// 편집 모드가 아닐 때
							return true;
						}
						//else if(Editor.GetModLockOption_BonePreview(ExAnimEditingMode))
						//{
						//	//편집모드 일때 + Bone Preview 모드일때 
						//	return true;
						//}
						return false;
				}
				//그 외에는 False
				return false;
			}
		}

		/// <summary>
		/// Bone의 IK 계산이 Rigging에 적용되어야 하는가
		/// </summary>
		/// <returns></returns>
		public bool IsBoneIKRiggingUpdatable
		{
			get
			{
				switch (_selectionType)
				{
					case SELECTION_TYPE.Overall:
						return true;

					case SELECTION_TYPE.MeshGroup:
						if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier)
						{
							if (Modifier != null)
							{
								if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
								{
									//리깅 모디파이어 타입인 경우
									//그냥 IK 불가
									return false;
								}
								else
								{
									if (ExEditingMode == EX_EDIT.None)
									{
										//편집 모드가 아닐땐 True
										return true;
									}
								}
							}
						}
						else if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
						{
							return true;
						}
						return false;

					case SELECTION_TYPE.Animation:
						if (ExAnimEditingMode == EX_EDIT.None)
						{
							// 편집 모드가 아닐 때
							return true;
						}
						return false;
				}
				//그 외에는 False
				return false;
			}
		}


		/// <summary>
		/// IK가 적용된 Bone이 렌더링 되는 경우 (아웃라인은 아니다)
		/// 작업 중일 때에는 렌더링이 되면 안되는 것이 원칙
		/// </summary>
		/// <returns></returns>
		public bool IsBoneIKRenderable
		{
			get
			{
				switch (_selectionType)
				{
					case SELECTION_TYPE.Overall:
						return true;

					case SELECTION_TYPE.MeshGroup:
						if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Modifier)
						{
							if (Modifier != null)
							{
								if (Modifier.ModifierType == apModifierBase.MODIFIER_TYPE.Rigging)
								{
									//리깅 모디파이어 타입인 경우
									//그냥 IK 불가
									return false;
								}
								else
								{
									if (ExEditingMode == EX_EDIT.None)
									{
										//편집 모드가 아닐땐 True
										return true;
									}
								}
							}
						}
						else if (Editor._meshGroupEditMode == apEditor.MESHGROUP_EDIT_MODE.Setting)
						{
							//Setting에서도 True
							return true;
						}
						else
						{
							//변경 20.7.15 : Bone 탭에서 본 편집 모드가 아니면 IK 적용
							//(원래는 항상 false)
							if(!IsBoneDefaultEditing)
							{
								return true;
							}
						}
						return false;

					case SELECTION_TYPE.Animation:
						if (ExAnimEditingMode == EX_EDIT.None)
						{
							// 편집 모드가 아닐 때
							return true;
						}
						//else if(Editor.GetModLockOption_BonePreview(ExAnimEditingMode))
						//{
						//	//편집모드 일때 + Bone Preview 모드일때 
						//	return true;
						//}
						return false;
				}
				//그 외에는 False
				return false;
			}
		}


		// 복구 함수들
		//--------------------------------------------------------------------------------------------
		public class RestoredResult
		{
			public bool _isAnyRestored = false;
			public bool _isRestoreToAdded = false;//삭제된 것이 복원되었다.
			public bool _isRestoreToRemoved = false;//추가되었던 것이 다시 없어졌다.

			public apTextureData _restoredTextureData = null;
			public apMesh _restoredMesh = null;
			public apMeshGroup _restoredMeshGroup = null;
			public apAnimClip _restoredAnimClip = null;
			public apControlParam _restoredControlParam = null;
			public apModifierBase _restoredModifier = null;

			public SELECTION_TYPE _changedType = SELECTION_TYPE.None;

			private static RestoredResult _instance = null;
			public static RestoredResult I { get { if(_instance == null) { _instance = new RestoredResult(); } return _instance; } }

			private RestoredResult()
			{

			}

			public void Init()
			{
				_isAnyRestored = false;
				_isRestoreToAdded = false;//삭제된 것이 복원되었다.
				_isRestoreToRemoved = false;//추가되었던 것이 다시 없어졌다.

				_restoredTextureData = null;
				_restoredMesh = null;
				_restoredMeshGroup = null;
				_restoredAnimClip = null;
				_restoredControlParam = null;
				_restoredModifier = null;

				_changedType = SELECTION_TYPE.None;
			}
		}

		

		/// <summary>
		/// Editor에서 Undo가 수행될 때, Undo 직전의 상태를 확인하여 자동으로 페이지를 전환한다.
		/// RestoredResult를 리턴한다.
		/// </summary>
		public RestoredResult SetAutoSelectWhenUndoPerformed(		apPortrait portrait,
																	List<int> recordList_TextureData,
																	List<int> recordList_Mesh,
																	//List<int> recordList_MeshGroup,
																	List<int> recordList_AnimClip,
																	List<int> recordList_ControlParam,
																	List<int> recordList_Modifier,
																	List<int> recordList_AnimTimeline,
																	List<int> recordList_AnimTimelineLayer,
																	//List<int> recordList_Transform,
																	List<int> recordList_Bone,
																	Dictionary<int, List<int>> recordList_MeshGroupAndTransform,
																	Dictionary<int, int> recordList_AnimClip2TargetMeshGroup,//<<추가
																	bool isStructChanged)
		{
			//추가. 만약 개수가 변경된 경우, 그것이 삭제 되거나 추가된 경우이다.
			//Prev  <-- ( Undo ) --- Next
			// 있음        <-        없음 : 삭제된 것이 복원 되었다. 해당 메뉴를 찾아서 이동해야한다.
			// 없음        <-        있음 : 새로 추가되었다. 

			RestoredResult.I.Init();

			if(portrait == null)
			{
				return RestoredResult.I;
			}

			
			//개수로 체크하면 빠르다.

			//1. 텍스쳐
			if (portrait._textureData != null && portrait._textureData.Count != recordList_TextureData.Count)
			{
				//텍스쳐 리스트와 개수가 다른 경우
				if (portrait._textureData.Count > recordList_TextureData.Count)
				{
					//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
					RestoredResult.I._isRestoreToAdded = true;
					RestoredResult.I._changedType = SELECTION_TYPE.ImageRes;

					
					//복원된 것을 찾자
					for (int i = 0; i < portrait._textureData.Count; i++)
					{
						int uniqueID = portrait._textureData[i]._uniqueID;
						if(!recordList_TextureData.Contains(uniqueID))
						{
							//Undo 전에 없었던 ID이다. 새로 추가되었다.
							RestoredResult.I._restoredTextureData = portrait._textureData[i];
							break;
						}
					}
				}
				else
				{
					//Restored < Record : 추가되었던 것이 삭제되었다.
					RestoredResult.I._isRestoreToRemoved = true;
					RestoredResult.I._changedType = SELECTION_TYPE.ImageRes;
				}
			}

			//2. 메시
			if (portrait._meshes != null)
			{
				//실제 Monobehaviour를 체크
				if(portrait._subObjectGroup_Mesh != null)
				{
					//Unity에서 제공하는 childMeshes를 기준으로 동기화를 해야한다.
					apMesh[] childMeshes = portrait._subObjectGroup_Mesh.GetComponentsInChildren<apMesh>();

					int nMeshesInList = 0;
					int nMeshesInGameObj = 0;
					for (int i = 0; i < portrait._meshes.Count; i++)
					{
						if (portrait._meshes[i] != null)
						{
							nMeshesInList++;
						}
					}
					if(portrait._meshes.Count != nMeshesInList)
					{
						//실제 데이터와 다르다. (Null이 포함되어 있다.)
						portrait._meshes.RemoveAll(delegate(apMesh a)
						{
							return a == null;
						});
					}

					if(childMeshes == null)
					{
						//Debug.LogError("Child Mesh가 없다.");
					}
					else
					{
						//Debug.LogError("Child Mesh의 개수 [" + childMeshes.Length + "] / 리스트 데이터 상의 개수 [" + portrait._meshes.Count + "]");
						nMeshesInGameObj = childMeshes.Length;
					}

					if(nMeshesInList != nMeshesInGameObj)
					{
						if(nMeshesInGameObj > 0)
						{
							for (int i = 0; i < childMeshes.Length; i++)
							{
								apMesh childMesh = childMeshes[i];
								if(!portrait._meshes.Contains(childMesh))
								{
									portrait._meshes.Add(childMesh);
								}
							}
						}
					}
				}

				if (portrait._meshes.Count != recordList_Mesh.Count)
				{
					//Mesh 리스트와 개수가 다른 경우
					if (portrait._meshes.Count > recordList_Mesh.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Mesh;

						//복원된 것을 찾자
						for (int i = 0; i < portrait._meshes.Count; i++)
						{
							int uniqueID = portrait._meshes[i]._uniqueID;
							if (!recordList_Mesh.Contains(uniqueID))
							{
								//Undo 전에 없었던 ID이다. 새로 추가되었다.
								RestoredResult.I._restoredMesh = portrait._meshes[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Mesh;
					}
				}
			}


			//3. 메시 그룹
			if (portrait._meshGroups != null)
			{
				if (portrait._subObjectGroup_MeshGroup != null)
				{
					//Unity에서 제공하는 childMeshGroups를 기준으로 동기화를 해야한다.
					apMeshGroup[] childMeshGroups = portrait._subObjectGroup_MeshGroup.GetComponentsInChildren<apMeshGroup>();

					int nMeshGroupsInList = 0;
					int nMeshGroupsInGameObj = 0;
					for (int i = 0; i < portrait._meshGroups.Count; i++)
					{
						if(portrait._meshGroups[i] != null)
						{
							nMeshGroupsInList++;
						}
					}
					if(portrait._meshGroups.Count != nMeshGroupsInList)
					{
						//실제 데이터와 다르다. (Null이 포함되어 있다.)
						portrait._meshGroups.RemoveAll(delegate(apMeshGroup a)
						{
							return a == null;
						});
					}

					if(childMeshGroups != null)
					{
						nMeshGroupsInGameObj = childMeshGroups.Length;
					}

					if(nMeshGroupsInList != nMeshGroupsInGameObj)
					{
						if(nMeshGroupsInGameObj > 0)
						{
							for (int i = 0; i < childMeshGroups.Length; i++)
							{
								apMeshGroup childMeshGroup = childMeshGroups[i];
								if(!portrait._meshGroups.Contains(childMeshGroup))
								{
									portrait._meshGroups.Add(childMeshGroup);
								}
							}
						}
					}

				}

				//변경 20.1.28
				if (portrait._meshGroups.Count != recordList_MeshGroupAndTransform.Count)
				{
					//메시 그룹 리스트와 다른 경우
					if (portrait._meshGroups.Count > recordList_MeshGroupAndTransform.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;

						
						//복원된 것을 찾자
						for (int i = 0; i < portrait._meshGroups.Count; i++)
						{
							int uniqueID = portrait._meshGroups[i]._uniqueID;
							if (!recordList_MeshGroupAndTransform.ContainsKey(uniqueID))
							{
								//Undo 전에 없었던 ID이다. 새로 추가되었다.
								RestoredResult.I._restoredMeshGroup = portrait._meshGroups[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
					}
				}
				else
				{
					//MeshGroup과 비교를 했으며, 만약 개수에 변화가 없다면
					//Transform과 비교를 하자
					apMeshGroup curMeshGroup = null;
					List<int> transforms = null;
					int nTransform_Recorded = 0;
					int nTransform_Current = 0;
					for (int iMG = 0; iMG < portrait._meshGroups.Count; iMG++)
					{
						curMeshGroup = portrait._meshGroups[iMG];
						if (!recordList_MeshGroupAndTransform.ContainsKey(curMeshGroup._uniqueID))
						{
							//만약 개수가 같지만 모르는 메시 그룹이 나왔을 경우
							RestoredResult.I._isRestoreToAdded = true;
							RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
							RestoredResult.I._restoredMeshGroup = curMeshGroup;
							break;
						}
						//Transform을 비교하자
						transforms = recordList_MeshGroupAndTransform[curMeshGroup._uniqueID];
						int nMeshTransform = curMeshGroup._childMeshTransforms == null ? 0 : curMeshGroup._childMeshTransforms.Count;
						int nMeshGroupTransform = curMeshGroup._childMeshGroupTransforms == null ? 0 : curMeshGroup._childMeshGroupTransforms.Count;
						
						nTransform_Current = nMeshTransform + nMeshGroupTransform;
						nTransform_Recorded = transforms.Count;

						if(nTransform_Current > nTransform_Recorded)
						{
							//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
							RestoredResult.I._isRestoreToAdded = true;
							RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
							break;
						}
						else if(nTransform_Current < nTransform_Recorded)
						{
							//Restored < Record : 추가되었던 것이 삭제되었다.
							RestoredResult.I._isRestoreToRemoved = true;
							RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
							break;
						}
					}
					
				}
			}


			//4. 애니메이션 클립
			if (portrait._animClips != null)
			{
				if (portrait._animClips.Count != recordList_AnimClip.Count)
				{
					//Anim 리스트와 개수가 다른 경우
					if (portrait._animClips.Count > recordList_AnimClip.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;

						
						//복원된 것을 찾자
						for (int i = 0; i < portrait._animClips.Count; i++)
						{
							int uniqueID = portrait._animClips[i]._uniqueID;
							if (!recordList_AnimClip.Contains(uniqueID))
							{
								//Undo 전에 없었던 ID이다. 새로 추가되었다.
								RestoredResult.I._restoredAnimClip = portrait._animClips[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;
					}
				}
				else
				{
					//만약, AnimClip 개수는 변동이 없는데, 타임라인 개수에 변동이 있다면
					//최소한 Refresh는 해야한다.
					int nTimeline = 0;
					int nTimelineLayer = 0;
					apAnimClip animClip = null;
					
					//추가 20.3.19 : TargetMeshGroup이 바뀌었다면?
					bool isTargetMeshGroupChanged = false;
					apAnimClip targetChangedAnimClip = null;
					

					for (int iAnimClip = 0; iAnimClip < portrait._animClips.Count; iAnimClip++)
					{
						animClip = portrait._animClips[iAnimClip];
						nTimeline += animClip._timelines.Count;

						for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
						{
							nTimelineLayer += animClip._timelines[iTimeline]._layers.Count;
						}

						if(recordList_AnimClip2TargetMeshGroup.ContainsKey(animClip._uniqueID))
						{
							int recLinkedTargetMeshGroupID = recordList_AnimClip2TargetMeshGroup[animClip._uniqueID];
							int curLinkedTargetMeshGroupID = (animClip._targetMeshGroup != null ? animClip._targetMeshGroup._uniqueID : -1);
							if(recLinkedTargetMeshGroupID != curLinkedTargetMeshGroupID)
							{
								//연결된 MeshGroup이 변경되었다.
								isTargetMeshGroupChanged = true;
								targetChangedAnimClip = animClip;

								//Debug.LogError("AnimClip [" + targetChangedAnimClip._name + "]과 연결된 메시 그룹이 변경되었다.");
							}
						}
						else
						{
							isTargetMeshGroupChanged = true;
							targetChangedAnimClip = animClip;
						}
					}

					if(nTimeline > recordList_AnimTimeline.Count
						|| nTimelineLayer > recordList_AnimTimelineLayer.Count)
					{
						//타임라인이나 타임라인 레이어가 증가했다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;
					}
					else if(nTimeline < recordList_AnimTimeline.Count
							|| nTimelineLayer < recordList_AnimTimelineLayer.Count)
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;
					}
					else if(isTargetMeshGroupChanged)
					{
						//변경 내역이 있는 AnimClip이다.
						RestoredResult.I._restoredAnimClip = targetChangedAnimClip;
						isStructChanged = true;
						RestoredResult.I._changedType = SELECTION_TYPE.Animation;
					}
				}
				
			}


			//5. 컨트롤 파라미터
			if (portrait._controller._controlParams != null 
				&& portrait._controller._controlParams.Count != recordList_ControlParam.Count)
			{
				//Param 리스트와 개수가 다른 경우
				if (portrait._controller._controlParams.Count > recordList_ControlParam.Count)
				{
					//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
					RestoredResult.I._isRestoreToAdded = true;
					RestoredResult.I._changedType = SELECTION_TYPE.Param;

					//복원된 것을 찾자
					for (int i = 0; i < portrait._controller._controlParams.Count; i++)
					{
						int uniqueID = portrait._controller._controlParams[i]._uniqueID;
						if(!recordList_ControlParam.Contains(uniqueID))
						{
							//Undo 전에 없었던 ID이다. 새로 추가되었다.
							RestoredResult.I._restoredControlParam = portrait._controller._controlParams[i];
							break;
						}
					}
				}
				else
				{
					//Restored < Record : 추가되었던 것이 삭제되었다.
					RestoredResult.I._isRestoreToRemoved = true;
					RestoredResult.I._changedType = SELECTION_TYPE.Param;
				}
			}

			//6. 모디파이어 > TODO
			if(RestoredResult.I._changedType != SELECTION_TYPE.MeshGroup)
			{
				//MeshGroup에서 복원 기록이 없는 경우에 한해서 Modifier의 추가가 있었는지 확인한다.
				//MeshGroup의 복원 기록이 있다면 Modifier는 자동으로 포함되기 때문
				//모든 모디파이어를 모아야 한다.
				List<apModifierBase> allModifiers = new List<apModifierBase>();

				apMeshGroup meshGroup = null;
				apModifierBase modifier = null;

				for (int iMG = 0; iMG < portrait._meshGroups.Count; iMG++)
				{
					meshGroup = portrait._meshGroups[iMG];
					if(meshGroup == null)
					{
						continue;
					}

					for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
					{
						modifier = meshGroup._modifierStack._modifiers[iMod];
						if(modifier == null)
						{
							continue;
						}
						allModifiers.Add(modifier);
					}
				}

				//이제 실제 포함된 Modifier를 비교해야한다.
				//이건 데이터 누락이 있을 수 있다.
				if(portrait._subObjectGroup_Modifier != null)
				{
					//Unity에서 제공하는 childModifer기준으로 동기화를 해야한다.
					apModifierBase[] childModifiers = portrait._subObjectGroup_Modifier.GetComponentsInChildren<apModifierBase>();

					int nModInList = allModifiers.Count;
					int nModInGameObj = 0;
					
					if(childModifiers != null)
					{
						nModInGameObj = childModifiers.Length;
					}

					if(nModInList != nModInGameObj)
					{
						if(nModInGameObj > 0)
						{
							for (int i = 0; i < childModifiers.Length; i++)
							{
								apModifierBase childModifier = childModifiers[i];
								//이제 어느 MeshGroup의 Modifier인지 찾아야 한다 ㅜㅜ

								if(childModifier._meshGroup == null)
								{
									//연결이 안되었다면 찾자
									int meshGroupUniqueID = childModifier._meshGroupUniqueID;
									childModifier._meshGroup = portrait.GetMeshGroup(meshGroupUniqueID);
								}

								if(childModifier._meshGroup != null)
								{
									if(!childModifier._meshGroup._modifierStack._modifiers.Contains(childModifier))
									{
										childModifier._meshGroup._modifierStack._modifiers.Add(childModifier);
									}

									//체크용 allModifiers 리스트에도 넣자
									if (!allModifiers.Contains(childModifier))
									{
										allModifiers.Add(childModifier);
									}

								}

							}
						}
					}
				}

				if(allModifiers.Count != recordList_Modifier.Count)
				{
					//모디파이어 리스트와 다른 경우 => 뭔가 복원 되었거나 삭제된 것이다.
					
					if(allModifiers.Count > recordList_Modifier.Count)
					{
						//Restored > Record : 삭제된 것이 다시 복원되어 추가 되었다.
						RestoredResult.I._isRestoreToAdded = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;

						//복원된 것을 찾자
						for (int i = 0; i < allModifiers.Count; i++)
						{
							int uniqueID = allModifiers[i]._uniqueID;
							if(!recordList_Modifier.Contains(uniqueID))
							{
								RestoredResult.I._restoredModifier = allModifiers[i];
								break;
							}
						}
					}
					else
					{
						//Restored < Record : 추가되었던 것이 삭제되었다.
						RestoredResult.I._isRestoreToRemoved = true;
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
					}
				}
			}

			//7. RootUnit -> MeshGroup의 변동 사항이 없다면 RootUnit을 체크해볼 필요가 있다.
			if(RestoredResult.I._changedType != SELECTION_TYPE.MeshGroup)
			{
				//RootUnit의 ID와 실제 RootUnit이 같은지 확인한다.
				int nRootUnit = portrait._rootUnits.Count;
				int nMainMeshGroup = portrait._mainMeshGroupList.Count;
				int nMainMeshGroupID = portrait._mainMeshGroupIDList.Count;

				if (nRootUnit != nMainMeshGroup ||
					nMainMeshGroup != nMainMeshGroupID ||
					nRootUnit != nMainMeshGroupID)
				{
					//3개의 값이 다르다.
					//ID를 기준으로 하자
					if(nRootUnit < nMainMeshGroupID ||
						nMainMeshGroup < nMainMeshGroupID ||
						nRootUnit < nMainMeshGroup)
					{
						//ID가 더 많다. -> 복원할게 있다.
						RestoredResult.I._changedType = SELECTION_TYPE.Overall;
						RestoredResult.I._isRestoreToAdded = true;
					}
					else
					{
						//ID가 더 적다. -> 삭제할게 있다.
						RestoredResult.I._changedType = SELECTION_TYPE.Overall;
						RestoredResult.I._isRestoreToRemoved = true;
					}
				}
				else
				{
					//개수는 같은데, 데이터가 빈게 있나.. 아니면 다를수도
					apRootUnit rootUnit = null;
					apMeshGroup mainMeshGroup = null;
					int mainMeshGroupID = -1;
					for (int i = 0; i < nRootUnit; i++)
					{
						rootUnit = portrait._rootUnits[i];
						mainMeshGroup = portrait._mainMeshGroupList[i];
						mainMeshGroupID = portrait._mainMeshGroupIDList[i];

						if(rootUnit == null || mainMeshGroup == null)
						{
							//데이터가 없다.
							if(mainMeshGroupID >= 0)
							{
								//유효한 ID가 있다. -> 복원할게 있다.
								RestoredResult.I._changedType = SELECTION_TYPE.Overall;
								RestoredResult.I._isRestoreToAdded = true;
							}
							else
							{
								//유효하지 않는 ID와 데이터가 있다. -> 삭제할 게 있다.
								RestoredResult.I._changedType = SELECTION_TYPE.Overall;
								RestoredResult.I._isRestoreToRemoved = true;
							}
						}
						else if(rootUnit._childMeshGroup == null 
							|| rootUnit._childMeshGroup != mainMeshGroup
							|| rootUnit._childMeshGroup._uniqueID != mainMeshGroupID)
						{
							//데이터가 맞지 않다.
							//삭제인지 추가인지 모르지만 일단 갱신 필요
							RestoredResult.I._changedType = SELECTION_TYPE.Overall;
							RestoredResult.I._isRestoreToRemoved = true;
						}
					}
				}
			}

			if (!RestoredResult.I._isRestoreToAdded && !RestoredResult.I._isRestoreToRemoved)
			{
				//MeshGroup의 변동이 없을 때
				//-> 1. Transform에 변동이 있는가
				//-> 2. Bone에 변동이 있는가
				//만약, MeshGroup은 그대로지만, Trasnform이 다른 경우 -> 갱신 필요
				//변경, Transform은 위에서 체크했으므로, 여기의 코드는 생략한다. (20.1.28)
				//List<int> allTransforms = new List<int>();
				List<int> allBones = new List<int>();
				for (int iMSG = 0; iMSG < portrait._meshGroups.Count; iMSG++)
				{
					apMeshGroup meshGroup = portrait._meshGroups[iMSG];
					
					//<BONE_EDIT> 모든 Bone이므로 수정하지 않음
					for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
					{
						allBones.Add(meshGroup._boneList_All[iBone]._uniqueID);
					}
				}

				//1. Transform 체크
				

				//2. Bone 체크
				if(allBones.Count != recordList_Bone.Count)
				{
					//Bone 개수가 Undo를 전후로 바뀌었다.
					if(allBones.Count > recordList_Bone.Count)
					{
						//삭제 -> 복원
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
						RestoredResult.I._isRestoreToAdded = true;
					}
					else
					{
						//추가 -> 삭제
						RestoredResult.I._changedType = SELECTION_TYPE.MeshGroup;
						RestoredResult.I._isRestoreToRemoved = true;
					}
				}

			}


			if (!RestoredResult.I._isRestoreToAdded 
				&& !RestoredResult.I._isRestoreToRemoved
				&& !isStructChanged//추가 20.1.21
				)
			{
				RestoredResult.I._isAnyRestored = false;
			}
			else
			{
				RestoredResult.I._isAnyRestored = true;
			}

			return RestoredResult.I;
			
		}

		public void SetAutoSelectOrUnselectFromRestore(RestoredResult restoreResult, apPortrait portrait)
		{
			if (!restoreResult._isRestoreToAdded && !restoreResult._isRestoreToRemoved)
			{
				//아무것도 바뀐게 없다면
				return;
			}

			if (restoreResult._isRestoreToAdded)
			{
				// 삭제 -> 복원해서 새로운게 생겼을 경우 : 그걸 선택해야한다.
				switch (restoreResult._changedType)
				{
					case SELECTION_TYPE.ImageRes:
						if (restoreResult._restoredTextureData != null)
						{
							SelectImage(restoreResult._restoredTextureData);
						}
						break;

					case SELECTION_TYPE.Mesh:
						if (restoreResult._restoredMesh != null)
						{
							SelectMesh(restoreResult._restoredMesh);
						}
						break;

					case SELECTION_TYPE.MeshGroup:
						if (restoreResult._restoredMeshGroup != null)
						{
							SelectMeshGroup(restoreResult._restoredMeshGroup);
						}
						else if(restoreResult._restoredModifier != null)
						{
							if(restoreResult._restoredModifier._meshGroup != null)
							{
								SelectMeshGroup(restoreResult._restoredModifier._meshGroup);
							}
						}
						break;

					case SELECTION_TYPE.Animation:
						if (restoreResult._restoredAnimClip != null)
						{
							SelectAnimClip(restoreResult._restoredAnimClip);
						}
						break;

					case SELECTION_TYPE.Param:
						if (restoreResult._restoredControlParam != null)
						{
							SelectControlParam(restoreResult._restoredControlParam);
						}
						break;

					case SELECTION_TYPE.Overall:
						//RootUnit은 새로 복원되어도 별도의 행동을 취하지 않는다.
						break;

					default:
						//뭐징..
						restoreResult.Init();
						return;
				}
			}

			if (restoreResult._isRestoreToRemoved)
			{
				// 추가 -> 취소해서 삭제되었을 경우 : 타입을 보고 해당 페이지의 것이 이미 사라진 것인지 확인
				//페이지를 나와야 한다.
				bool isRemovedPage = false;
				if (SelectionType == restoreResult._changedType)
				{
					switch (restoreResult._changedType)
					{
						case SELECTION_TYPE.ImageRes:
							if (_image != null)
							{
								if (!portrait._textureData.Contains(_image))
								{
									//삭제되어 없는 이미지를 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Mesh:
							if (_mesh != null)
							{
								if (!portrait._meshes.Contains(_mesh))
								{
									//삭제되어 없는 메시를 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.MeshGroup:
							if (_meshGroup != null)
							{
								if (!portrait._meshGroups.Contains(_meshGroup))
								{
									//삭제되어 없는 메시 그룹을 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Animation:
							if (_animClip != null)
							{
								if (!portrait._animClips.Contains(_animClip))
								{
									//삭제되어 없는 AnimClip을 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Param:
							if (_param != null)
							{
								if (!portrait._controller._controlParams.Contains(_param))
								{
									//삭제되어 없는 Param을 가리키고 있다.
									isRemovedPage = true;
								}
							}
							break;

						case SELECTION_TYPE.Overall:
							{
								if(_rootUnit != null)
								{
									if(!portrait._rootUnits.Contains(_rootUnit))
									{
										isRemovedPage = true;
									}
								}
							}
							break;
						
						default:
							//뭐징..
							restoreResult.Init();
							return;
					}
				}

				if (isRemovedPage)
				{
					SelectNone();
				}
			}

			restoreResult.Init();
		}

		//--------------------------------------------------------------------------------------
		// Reset GUI Contents
		//--------------------------------------------------------------------------------------
		public void ResetGUIContents()
		{
			//_guiContent_StepCompleted = null;
			//_guiContent_StepUncompleted = null;
			//_guiContent_StepUnUsed = null;

			_guiContent_imgValueUp = null;
			_guiContent_imgValueDown = null;
			_guiContent_imgValueLeft = null;
			_guiContent_imgValueRight = null;

			_guiContent_MeshProperty_ResetVerts = null;
			_guiContent_MeshProperty_RemoveMesh = null;
			_guiContent_MeshProperty_ChangeImage = null;
			_guiContent_MeshProperty_AutoLinkEdge = null;
			_guiContent_MeshProperty_Draw_MakePolygones = null;
			_guiContent_MeshProperty_MakePolygones = null;
			_guiContent_MeshProperty_RemoveAllVertices = null;
			//_guiContent_MeshProperty_HowTo_MouseLeft = null;
			//_guiContent_MeshProperty_HowTo_MouseMiddle = null;
			//_guiContent_MeshProperty_HowTo_MouseRight = null;
			//_guiContent_MeshProperty_HowTo_KeyDelete = null;
			//_guiContent_MeshProperty_HowTo_KeyCtrl = null;
			//_guiContent_MeshProperty_HowTo_KeyShift = null;
			_guiContent_MeshProperty_Texture = null;

			_guiContent_MeshProperty_PinRangeOption = null;
			_guiContent_MeshProperty_PinCalculateWeight = null;
			_guiContent_MeshProperty_PinResetTestPos = null;
			_guiContent_MeshProperty_RemoveAllPins = null;

			_guiContent_Bottom2_Physic_WindON = null;
			_guiContent_Bottom2_Physic_WindOFF = null;

			_guiContent_Image_RemoveImage = null;
			_guiContent_Animation_SelectMeshGroupBtn = null;
			_guiContent_Animation_AddTimeline = null;
			_guiContent_Animation_RemoveAnimation = null;
			_guiContent_Animation_TimelineUnit_AnimMod = null;
			_guiContent_Animation_TimelineUnit_ControlParam = null;

			_guiContent_Overall_SelectedAnimClp = null;
			_guiContent_Overall_MakeThumbnail = null;
			_guiContent_Overall_TakeAScreenshot = null;
			_guiContent_Overall_AnimItem = null;

			_guiContent_Param_Presets = null;
			_guiContent_Param_RemoveParam = null;
			_guiContent_Param_IconPreset = null;

			_guiContent_MeshGroupProperty_RemoveMeshGroup = null;
			_guiContent_MeshGroupProperty_RemoveAllBones = null;
			_guiContent_MeshGroupProperty_ModifierLayerUnit = null;
			_guiContent_MeshGroupProperty_SetRootUnit = null;
			_guiContent_MeshGroupProperty_AddModifier = null;

			_guiContent_Bottom_Animation_TimelineLayerInfo = null;
			_guiContent_Bottom_Animation_RemoveKeyframes = null;
			_guiContent_Bottom_Animation_RemoveNumKeyframes = null;
			_guiContent_Bottom_Animation_Fit = null;

			_guiContent_Right_MeshGroup_MaterialSet = null;
			_guiContent_Right_MeshGroup_CustomShader = null;
			_guiContent_Right_MeshGroup_MatSetName = null;
			_guiContent_Right_MeshGroup_CopySettingToOtherMeshes = null;
			_guiContent_Right_MeshGroup_RiggingIconAndText = null;
			_guiContent_Right_MeshGroup_ParamIconAndText = null;
			_guiContent_Right_MeshGroup_RemoveBone = null;
			_guiContent_Right_MeshGroup_RemoveModifier = null;

			_guiContent_Modifier_ParamSetItem = null;
			_guiContent_Modifier_AddControlParameter = null;
			_guiContent_CopyTargetIcon = null;
			_guiContent_CopyTextIcon = null;
			_guiContent_PasteTextIcon = null;
			_guiContent_Modifier_RigExport = null;
			_guiContent_Modifier_RigImport = null;
			_guiContent_Modifier_RemoveFromKeys = null;
			_guiContent_Modifier_AddToKeys = null;
			_guiContent_Modifier_RemoveAllKeys = null;
			_guiContent_Modifier_AnimIconText = null;
			_guiContent_Modifier_RemoveFromRigging = null;
			_guiContent_Modifier_AddToRigging = null;
			_guiContent_Modifier_AddToPhysics = null;
			_guiContent_Modifier_RemoveFromPhysics = null;
			_guiContent_Modifier_PhysicsSetting_NameIcon = null;
			_guiContent_Modifier_PhysicsSetting_Basic = null;
			_guiContent_Modifier_PhysicsSetting_Stretchiness = null;
			_guiContent_Modifier_PhysicsSetting_Inertia = null;
			_guiContent_Modifier_PhysicsSetting_Restoring = null;
			_guiContent_Modifier_PhysicsSetting_Viscosity = null;
			_guiContent_Modifier_PhysicsSetting_Gravity = null;
			_guiContent_Modifier_PhysicsSetting_Wind = null;
			_guiContent_Right_Animation_AllObjectToLayers = null;
			_guiContent_Right_Animation_RemoveTimeline = null;
			_guiContent_Right_Animation_AddTimelineLayerToEdit = null;
			_guiContent_Right_Animation_RemoveTimelineLayer = null;
			//_guiContent_Bottom_EditMode_CommonIcon = null;
			_guiContent_Icon_ModTF_Pos = null;
			_guiContent_Icon_ModTF_Rot = null;
			_guiContent_Icon_ModTF_Scale = null;
			_guiContent_Icon_Mod_Color = null;
			_guiContent_Right_MeshGroup_MeshIcon = null;
			_guiContent_Right_MeshGroup_MeshGroupIcon = null;
			_guiContent_Right_MeshGroup_MultipleSelected = null;
			_guiContent_Right_MeshGroup_ModIcon = null;
			_guiContent_Right_MeshGroup_AnimIcon = null;
			_guiContent_Right_Animation_TimelineIcon_AnimWithMod = null;
			_guiContent_Right_Animation_TimelineIcon_AnimWithControlParam = null;
			_guiContent_Bottom_Animation_FirstFrame = null;
			_guiContent_Bottom_Animation_PrevFrame = null;
			_guiContent_Bottom_Animation_Play = null;
			_guiContent_Bottom_Animation_Pause = null;
			_guiContent_Bottom_Animation_NextFrame = null;
			_guiContent_Bottom_Animation_LastFrame = null;

			//_guiContent_MakeMesh_PointCount_X = null;
			//_guiContent_MakeMesh_PointCount_Y = null;
			//_guiContent_MakeMesh_AutoGenPreview = null;
			_guiContent_MakeMesh_GenerateMesh = null;
			_guiContent_MakeMesh_QuickGenerate = null;
			_guiContent_MakeMesh_MultipleQuickGenerate = null;
			_guiContent_MeshEdit_Area_Enabled = null;
			_guiContent_MeshEdit_Area_Disabled = null;
			_guiContent_MeshEdit_AreaEditing_Off = null;
			_guiContent_MeshEdit_AreaEditing_On = null;

			_guiContent_AnimKeyframeProp_PrevKeyLabel = null;
			_guiContent_AnimKeyframeProp_NextKeyLabel = null;

			_guiContent_Right2MeshGroup_ObjectProp_Name = null;
			_guiContent_Right2MeshGroup_ObjectProp_Type = null;
			_guiContent_Right2MeshGroup_ObjectProp_NickName = null;

			_guiContent_Right2MeshGroup_JiggleBone = null;

			_guiContent_MaterialSet_ON = null;
			_guiContent_MaterialSet_OFF = null;

			_guiContent_Right2MeshGroup_MaskParentName = null;
			_guiContent_Right2MeshGroup_DuplicateTransform = null;
			_guiContent_Right2MeshGroup_MigrateTransform = null;
			_guiContent_Right2MeshGroup_DetachObject = null;

			_guiContent_ModProp_ParamSetTarget_Name = null;
			_guiContent_ModProp_ParamSetTarget_StatusText = null;

			_guiContent_ModProp_Rigging_VertInfo = null;
			_guiContent_ModProp_Rigging_BoneInfo = null;

			_guiContent_RiggingBoneWeightLabel = null;
			_guiContent_RiggingBoneWeightBoneName = null;

			_guiContent_PhysicsGroupID_None = null;
			_guiContent_PhysicsGroupID_1 = null;
			_guiContent_PhysicsGroupID_2 = null;
			_guiContent_PhysicsGroupID_3 = null;
			_guiContent_PhysicsGroupID_4 = null;
			_guiContent_PhysicsGroupID_5 = null;
			_guiContent_PhysicsGroupID_6 = null;
			_guiContent_PhysicsGroupID_7 = null;
			_guiContent_PhysicsGroupID_8 = null;
			_guiContent_PhysicsGroupID_9 = null;

			_guiContent_Right2_Animation_TargetObjectName = null;
			

			//GUI Content 추가시 여기에 코드를 적자
		}
	}
}