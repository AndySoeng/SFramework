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

using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 20.6.8
	/// apMultiSubObjects에 의해서 ModMesh, ModBone은 물론이고, ModRenderVert와 Keyframe등도 동기화를 하자.
	/// Main/Sub로 구분한다.
	/// ModRenderVert는 풀을 이용하자. (워낙에 생성/해제가 많아서)
	/// apSelection + apMultiSubObjects의 정보를 받아서 동기화를 한다.
	/// apSelection의 하위 객체이다.
	/// </summary>
	public class apMultiModData
	{
		// Members
		//--------------------------------------------------------------------
		public enum MODE
		{
			ModEdit,
			AnimEdit
		}
		private MODE _mode = MODE.ModEdit;
		public MODE Mode { get { return _mode; } }

		//링크 정보
		public enum SELECTION_TYPE
		{
			Main, Sub
		}

		public enum KEY_TYPE
		{
			MeshTF, MeshGroupTF, Bone, ControlParam
		}

		//키값 > 편집 데이터의 링크 정보
		public class LinkUnit
		{
			// Members
			//------------------------------------------------------
			public SELECTION_TYPE _unitType = SELECTION_TYPE.Main;
			
			//키값
			public KEY_TYPE _keyType = KEY_TYPE.MeshTF;
			public apTransform_Mesh _key_MeshTF = null;
			public apTransform_MeshGroup _key_MeshGroupTF = null;
			public apBone _key_Bone = null;
			public apControlParam _key_ControlParam_Anim = null;//애니메이션 전용 컨트롤 파라미터

			public apRenderUnit _linkedRenderUnit = null;

			//링크된 데이터
			public bool _isValidData = false;//키값과 연결될 ParamSet/Keyframe이 없는 경우 이 값은 false이다.

			public apModifiedMesh _modMesh = null;
			public apModifiedBone _modBone = null;
			
			//이게 기즈모에서도 유효한 선택인가
			//(다중 선택되었다 하더라도, 기즈모에서는 무시되는 경우가 있다.)
			public bool _isGizmoSelected = false;
			public bool _isGizmoMain = false;



			//전체 Vert 타입의 모디파이어를 편집하는 경우,
			//모든 ModRenderVert를 미리 만들자.
			//ModRenderVert는 apModifiedVertex / apModifiedVertexRig / apModifiedVertexWeight 중에 하나와 연결된다.
			//기존에는 "선택할때" 매번 new로 생성하는데, 여기서는 미리 만들어서 선택을 빠르게 만든다.
			public List<apSelection.ModRenderVert> _modRenderVerts = new List<apSelection.ModRenderVert>();

			//추가 22.4.6 [v1.4.0] Mod Render Pin 추가
			public List<apSelection.ModRenderPin> _modRenderPins = new List<apSelection.ModRenderPin>();


			//애니메이션인 경우
			//연결된 타임라인 레이어와 키프레임
			public apAnimTimelineLayer _timelineLayer = null;
			public apAnimKeyframe _keyframe = null;

			public apMultiModData _parentModData = null;

			// Init
			//------------------------------------------------------
			public LinkUnit(apMultiModData parentModData)
			{
				_parentModData = parentModData;
				ClearAll();
			}

			public void ClearAll()
			{
				_unitType = SELECTION_TYPE.Main;
			
				_keyType = KEY_TYPE.MeshTF;
				_key_MeshTF = null;
				_key_MeshGroupTF = null;
				_key_Bone = null;
				_key_ControlParam_Anim = null;
				_linkedRenderUnit = null;
				_isGizmoSelected = false;
				_isGizmoMain = false;

				ClearData();
			}

			public void ClearData()
			{
				_isValidData = false;

				_modMesh = null;
				_modBone = null;
				_linkedRenderUnit = null;

				_isGizmoSelected = false;
				_isGizmoMain = false;

				if (_modRenderVerts == null)
				{
					_modRenderVerts = new List<apSelection.ModRenderVert>();
				}

				//이미 MRV가 있다면
				if(_modRenderVerts.Count > 0)
				{
					PushMRVAll();
				}
				_modRenderVerts.Clear();

				//추가 22.4.6 [v1.4.0] MRP도
				if(_modRenderPins == null)
				{
					_modRenderPins = new List<apSelection.ModRenderPin>();
				}
				if(_modRenderPins.Count > 0)
				{
					PushMRPAll();//MRP를 모두 Ready상태로 만든다.
				}
				_modRenderPins.Clear();

				_timelineLayer = null;
				_keyframe = null;
			}

			public void SetKey(apTransform_Mesh meshTF, bool isGizmoSelected, bool isGizmoMain)
			{
				_keyType = KEY_TYPE.MeshTF;
				_key_MeshTF = meshTF;
				_key_MeshGroupTF = null;
				_key_Bone = null;
				_key_ControlParam_Anim = null;

				_linkedRenderUnit = _key_MeshTF._linkedRenderUnit;
				
				_isGizmoSelected = isGizmoSelected;
				_isGizmoMain = isGizmoMain;
			}

			public void SetKey(apTransform_MeshGroup meshGroupTF, bool isGizmoSelected, bool isGizmoMain)
			{
				_keyType = KEY_TYPE.MeshGroupTF;
				_key_MeshTF = null;
				_key_MeshGroupTF = meshGroupTF;
				_key_Bone = null;
				_key_ControlParam_Anim = null;

				_linkedRenderUnit = _key_MeshGroupTF._linkedRenderUnit;

				_isGizmoSelected = isGizmoSelected;
				_isGizmoMain = isGizmoMain;
			}

			public void SetKey(apBone bone, bool isGizmoSelected, bool isGizmoMain)
			{
				_keyType = KEY_TYPE.Bone;
				_key_MeshTF = null;
				_key_MeshGroupTF = null;
				_key_Bone = bone;
				_key_ControlParam_Anim = null;

				_linkedRenderUnit = null;

				_isGizmoSelected = isGizmoSelected;
				_isGizmoMain = isGizmoMain;
			}

			public void SetKey(apControlParam controlParam)
			{
				_keyType = KEY_TYPE.ControlParam;
				_key_MeshTF = null;
				_key_MeshGroupTF = null;
				_key_Bone = null;
				_key_ControlParam_Anim = controlParam;

				_linkedRenderUnit = null;

				_isGizmoSelected = false;
			}

			

			public void SetSelectionType(SELECTION_TYPE selectionType)
			{
				_unitType = selectionType;
			}



			// Functions
			//------------------------------------------------------
			public void SetModMesh(apModifiedMesh modMesh, apModifierBase modifier)
			{
				_isValidData = true;
				_modMesh = modMesh;
				_modBone = null;
				

				if(_linkedRenderUnit == null)
				{
					return;
				}
				
				int nRenderVerts = _linkedRenderUnit._renderVerts != null ? _linkedRenderUnit._renderVerts.Length : 0;
				int nRenderPins = _linkedRenderUnit._renderPinGroup != null ? _linkedRenderUnit._renderPinGroup.NumPins : 0;//추가 22.4.6

				if(nRenderVerts == 0 && nRenderPins == 0)
				{
					return;
				}

				if(nRenderVerts > 0)
				{
					//모디파이어의 종류에 따라서 MRV를 미리 만들어두자
					//타입별로 리스트 체크
					//크기가 같다면 바로 인덱스 체크 > 성공시 바로 생성하자
					//그렇지 않거나 1차 테스트에서 실패시 (느리지만) Find
					apRenderVertex renderVert = null;
					
					bool isSameSize = false;
					bool isFound = false;

					if ((int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0
						&& _modMesh._vertices != null 
						&& _modMesh._vertices.Count > 0)
					{

						//RenderVert 순서대로
						apModifiedVertex modVert = null;
						isSameSize = _modMesh._vertices.Count == nRenderVerts;
						
						for (int iRV = 0; iRV < nRenderVerts; iRV++)
						{
							renderVert = _linkedRenderUnit._renderVerts[iRV];
							isFound = false;

							if (isSameSize)
							{
								modVert = _modMesh._vertices[iRV];
								if (modVert._vertexUniqueID == renderVert._vertex._uniqueID)
								{
									isFound = true;
								}
							}
							if (!isFound)
							{
								//직접 찾아야 한다.
								modVert = _modMesh._vertices.Find(delegate (apModifiedVertex a)
								{
									return a._vertexUniqueID == renderVert._vertex._uniqueID;
								});

								if (modVert != null)
								{
									isFound = true;
								}
							}

							if (!isFound) { continue; }

							//MRV 추가
							AddMRV(modVert, renderVert);
						}
					}
					else if ((int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.BoneVertexWeightList) != 0
							&& _modMesh._vertRigs != null 
							&& _modMesh._vertRigs.Count > 0)
					{
						//RenderVert 순서대로
						apModifiedVertexRig modVertRig = null;
						isSameSize = _modMesh._vertRigs.Count == nRenderVerts;
						
						for (int iRV = 0; iRV < nRenderVerts; iRV++)
						{
							renderVert = _linkedRenderUnit._renderVerts[iRV];
							isFound = false;

							if (isSameSize)
							{
								modVertRig = _modMesh._vertRigs[iRV];
								if (modVertRig._vertexUniqueID == renderVert._vertex._uniqueID)
								{
									isFound = true;
								}
							}
							if (!isFound)
							{
								//직접 찾아야 한다.
								modVertRig = _modMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
								{
									return a._vertexUniqueID == renderVert._vertex._uniqueID;
								});

								if (modVertRig != null)
								{
									isFound = true;
								}
							}

							if (!isFound) { continue; }

							//MRV 추가
							AddMRV(modVertRig, renderVert);
						}
					}
					else if (
						( (int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Physics) != 0
						|| (int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexWeightList_Volume) != 0 )
						&& _modMesh._vertWeights != null 
						&& _modMesh._vertWeights.Count > 0)
					{
						//RenderVert 순서대로
						apModifiedVertexWeight modVertWeight = null;
						isSameSize = _modMesh._vertWeights.Count == nRenderVerts;
						
						for (int iRV = 0; iRV < nRenderVerts; iRV++)
						{
							renderVert = _linkedRenderUnit._renderVerts[iRV];
							isFound = false;

							if (isSameSize)
							{
								modVertWeight = _modMesh._vertWeights[iRV];
								if (modVertWeight._vertexUniqueID == renderVert._vertex._uniqueID)
								{
									isFound = true;
								}
							}
							if (!isFound)
							{
								//직접 찾아야 한다.
								modVertWeight = _modMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
								{
									return a._vertexUniqueID == renderVert._vertex._uniqueID;
								});

								if (modVertWeight != null)
								{
									isFound = true;
								}
							}

							if (!isFound) { continue; }

							//MRV 추가
							AddMRV(modVertWeight, renderVert);
						}
					}
				}


				if (nRenderPins > 0)
				{
					//MRP도 확인하고 추가한다. (추가 22.4.6)
					if ((int)(modifier.ModifiedValueType & apModifiedMesh.MOD_VALUE_TYPE.VertexPosList) != 0
						&& _modMesh._pins != null
						&& _modMesh._pins.Count > 0)
					{

						//RenderPin 순서대로
						apRenderPin renderPin = null;
						apModifiedPin modPin = null;

						bool isSameSize = _modMesh._pins.Count == nRenderPins;
						bool isFound = false;
						
						

						for (int iRP = 0; iRP < nRenderPins; iRP++)
						{
							renderPin = _linkedRenderUnit._renderPinGroup._pins[iRP];
							isFound = false;

							//연결될 ModPin을 찾자
							if (isSameSize)
							{
								modPin = _modMesh._pins[iRP];
								if (modPin._pinUniqueID == renderPin._srcPin._uniqueID)
								{
									isFound = true;
								}
							}
							if (!isFound)
							{
								//직접 찾아야 한다.
								modPin = _modMesh._pins.Find(delegate (apModifiedPin a)
								{
									return a._pinUniqueID == renderPin._srcPin._uniqueID;
								});

								if (modPin != null)
								{
									isFound = true;
								}
							}

							if (!isFound) { continue; }

							//MRP 추가
							AddMRP(modPin, renderPin);
						}
					}
				}
			}

			public void SetModBone(apModifiedBone modBone)
			{
				_isValidData = true;
				_modMesh = null;
				_modBone = modBone;
			}

			public void SetAnimKeyframe(apAnimKeyframe workKeyframe)
			{
				_keyframe = workKeyframe;
				_timelineLayer = _keyframe._parentTimelineLayer;
			}


			
			// MRV 함수들
			//----------------------------------------------------------
			//모든 MRV를 반납한다.
			private void PushMRVAll()
			{
				for (int i = 0; i < _modRenderVerts.Count; i++)
				{
					_parentModData.PushMRV(_modRenderVerts[i]);
				}
				_modRenderVerts.Clear();
			}



			//MRV를 추가한다 (Pop 이용)
			public void AddMRV(apModifiedVertex modVert, apRenderVertex renderVert)
			{
				
				apSelection.ModRenderVert newMRV = _parentModData.PopMRV();
				newMRV._renderVert = renderVert;
				newMRV._modVert = modVert;
				newMRV._modVertRig = null;
				newMRV._modVertWeight = null;
				newMRV._vertWeightByTool = 1.0f;
				
				_modRenderVerts.Add(newMRV);

				//전체 리스트에도 추가
				_parentModData.AddMRVOfMod(newMRV);
			}

			public void AddMRV(apModifiedVertexRig modVertRig, apRenderVertex renderVert)
			{
				apSelection.ModRenderVert newMRV = _parentModData.PopMRV();
				newMRV._renderVert = renderVert;
				newMRV._modVert = null;
				newMRV._modVertRig = modVertRig;
				newMRV._modVertWeight = null;
				newMRV._vertWeightByTool = 1.0f;

				_modRenderVerts.Add(newMRV);

				//전체 리스트에도 추가
				_parentModData.AddMRVOfMod(newMRV);
			}

			public void AddMRV(apModifiedVertexWeight modVertWeight, apRenderVertex renderVert)
			{
				apSelection.ModRenderVert newMRV = _parentModData.PopMRV();
				newMRV._renderVert = renderVert;
				newMRV._modVert = null;
				newMRV._modVertRig = null;
				newMRV._modVertWeight = modVertWeight;
				newMRV._vertWeightByTool = modVertWeight._weight;

				_modRenderVerts.Add(newMRV);

				//전체 리스트에도 추가
				_parentModData.AddMRVOfMod(newMRV);
			}



			// MRP (Mod Render Pin) 함수들
			//----------------------------------------------------------
			//모든 MRV를 반납한다.
			private void PushMRPAll()
			{
				int nMRPs = _modRenderPins.Count;
				for (int i = 0; i < nMRPs; i++)
				{
					_parentModData.PushMRP(_modRenderPins[i]);
				}
				_modRenderPins.Clear();
			}


			//MRP를 추가한다 (Pop 이용)
			public void AddMRP(apModifiedPin modPin, apRenderPin renderPin)
			{
				
				apSelection.ModRenderPin newMRP = _parentModData.PopMRP();
				newMRP._modPin = modPin;
				newMRP._renderPin = renderPin;
				newMRP._pinWeightByTool = 1.0f;

				_modRenderPins.Add(newMRP);

				//전체 리스트에도 추가
				_parentModData.AddMRPOfMod(newMRP);
			}


			// Get / Set
			//------------------------------------------------------

		}


		//링크 데이터와 선택 결과
		private List<LinkUnit> _linkUnits_All = new List<LinkUnit>();
		private LinkUnit _linkUnit_Main = null;
		private List<LinkUnit> _linkUnits_Sub = new List<LinkUnit>();

		private Dictionary<object, LinkUnit> _keyObject_2_LinkUnit = new Dictionary<object, LinkUnit>();
		private Dictionary<apTransform_Mesh, LinkUnit> _keyMeshTF_2_LinkUnit = new Dictionary<apTransform_Mesh, LinkUnit>();
		private Dictionary<apTransform_MeshGroup, LinkUnit> _keyMeshGroupTF_2_LinkUnit = new Dictionary<apTransform_MeshGroup, LinkUnit>();
		private Dictionary<apBone, LinkUnit> _keyBone_2_LinkUnit = new Dictionary<apBone, LinkUnit>();

		//-------------------------------------------------------------------
		// Mod Render Vert 변수들
		//-------------------------------------------------------------------
		//ModRenderVert 오브젝트 풀
		private const int MRVP_INIT_POOL_SIZE = 200;
		private const int MRVP_ADD_POOL_SIZE = 100;
		private List<apSelection.ModRenderVert> _mrvPool_All = new List<apSelection.ModRenderVert>();
		private List<apSelection.ModRenderVert> _mrvPool_Live = new List<apSelection.ModRenderVert>();
		private List<apSelection.ModRenderVert> _mrvPool_Ready = new List<apSelection.ModRenderVert>();

		//LinkUnit에 들어간 전체 MRV 리스트 (결국 Live Pool과 같을 것 같긴 한데..)
		private List<apSelection.ModRenderVert>									_mrv_List = new List<apSelection.ModRenderVert>();
		private Dictionary<apModifiedVertex, apSelection.ModRenderVert>			_modVert2MRV = new Dictionary<apModifiedVertex, apSelection.ModRenderVert>();
		private Dictionary<apModifiedVertexRig, apSelection.ModRenderVert>		_modVertRig2MRV = new Dictionary<apModifiedVertexRig, apSelection.ModRenderVert>();
		private Dictionary<apModifiedVertexWeight, apSelection.ModRenderVert>	_modVertWeight2MRV = new Dictionary<apModifiedVertexWeight, apSelection.ModRenderVert>();
		private Dictionary<apTransform_Mesh, Dictionary<apVertex, apSelection.ModRenderVert>>	_vert2MRV = new Dictionary<apTransform_Mesh, Dictionary<apVertex, apSelection.ModRenderVert>>();


		//-------------------------------------------------------------------
		// Mod Render Pin 변수들 [v1.4.0]
		//-------------------------------------------------------------------
		//ModRenderPin 오브젝트 풀
		private List<apSelection.ModRenderPin> _mrpPool_All = new List<apSelection.ModRenderPin>();
		private List<apSelection.ModRenderPin> _mrpPool_Live = new List<apSelection.ModRenderPin>();
		private List<apSelection.ModRenderPin> _mrpPool_Ready = new List<apSelection.ModRenderPin>();

		//LinkUnit에 들어간 전체 MRP 리스트
		private List<apSelection.ModRenderPin>													_mrp_List = new List<apSelection.ModRenderPin>();
		private Dictionary<apModifiedPin, apSelection.ModRenderPin>								_modPin2MRP = new Dictionary<apModifiedPin, apSelection.ModRenderPin>();
		private Dictionary<apTransform_Mesh, Dictionary<apMeshPin, apSelection.ModRenderPin>>	_pin2MRP = new Dictionary<apTransform_Mesh, Dictionary<apMeshPin, apSelection.ModRenderPin>>();



		//결과 값
		//메인과 서브로 나눈다.
		private bool _result_IsMultipleModData = false;//여러개의 모드 데이터를 찾았는가. (여러개를 선택했지만, 모드 데이터는 하나일 수 있다.)
		private int _result_NumModData = 0;
		private apRenderUnit _result_Main_RenderUnit = null;
		private apModifiedMesh _result_Main_ModMesh = null;
		private apModifiedBone _result_Main_ModBone = null;

		private List<apRenderUnit> _result_All_RenderUnits = null;
		private List<apModifiedMesh> _result_All_ModMeshes = null;
		private List<apModifiedBone> _result_All_ModBones = null;

		//중요 > 기즈모용은 다르다. SubObject에서 Gizmo 리스트로부터 받는다.
		private apModifiedMesh _result_Gizmo_Main_ModMesh = null;
		private apModifiedBone _result_Gizmo_Main_ModBone = null;
		private apRenderUnit _result_Gizmo_Main_RenderUnit = null;
		private List<apModifiedMesh> _result_Gizmo_ModMeshes = null;
		private List<apModifiedBone> _result_Gizmo_ModBones = null;

		//계산용 변수
		//변동 사항이 있는지 체크하기 위한 변수
		private List<apRenderUnit> _prevRenderUnits = null;
		private List<apModifiedMesh> _prevModMeshes = null;
		private List<apModifiedBone> _prevModBones = null;

		

		//계산/참조용 변수들
		private apSelection _selection = null;

		private Dictionary<apVertex, apSelection.ModRenderVert> _tmp_GetMRV_Dictionary = null;
		private Dictionary<apMeshPin, apSelection.ModRenderPin> _tmp_GetMRP_Dictionary = null;
		
		// Init
		//--------------------------------------------------------------------
		public apMultiModData(apSelection selection)
		{
			_selection = selection;
			
			if(_prevRenderUnits == null)	{ _prevRenderUnits = new List<apRenderUnit>(); }
			if(_prevModMeshes == null)		{ _prevModMeshes = new List<apModifiedMesh>(); }
			if(_prevModBones == null)		{ _prevModBones = new List<apModifiedBone>(); }

			_prevRenderUnits.Clear();
			_prevModMeshes.Clear();
			_prevModBones.Clear();

			InitPool();
			ClearAll();
		}


		public void ClearAll()
		{
			_linkUnit_Main = null;

			if(_linkUnits_All == null) { _linkUnits_All = new List<LinkUnit>(); }
			if(_linkUnits_Sub == null) { _linkUnits_Sub = new List<LinkUnit>(); }
			_linkUnits_All.Clear();
			_linkUnits_Sub.Clear();
			
			if (_keyObject_2_LinkUnit == null)		{ _keyObject_2_LinkUnit = new Dictionary<object, LinkUnit>(); }
			if (_keyMeshTF_2_LinkUnit == null)		{ _keyMeshTF_2_LinkUnit = new Dictionary<apTransform_Mesh, LinkUnit>(); }
			if (_keyMeshGroupTF_2_LinkUnit == null)	{ _keyMeshGroupTF_2_LinkUnit = new Dictionary<apTransform_MeshGroup, LinkUnit>(); }
			if (_keyBone_2_LinkUnit == null)		{ _keyBone_2_LinkUnit = new Dictionary<apBone, LinkUnit>(); }
			
			_keyObject_2_LinkUnit.Clear();
			_keyMeshTF_2_LinkUnit.Clear();
			_keyMeshGroupTF_2_LinkUnit.Clear();
			_keyBone_2_LinkUnit.Clear();

			PushAllMRV();
			PushAllMRP();//추가 22.4.6

			//결과도 초기화
			_result_IsMultipleModData = false;//여러개의 모드 데이터를 찾았는가. (여러개를 선택했지만, 모드 데이터는 하나일 수 있다.)
			_result_NumModData = 0;
			_result_Main_RenderUnit = null;
			_result_Main_ModMesh = null;
			_result_Main_ModBone = null;
			
			if (_result_All_RenderUnits == null)	{ _result_All_RenderUnits = new List<apRenderUnit>(); }
			if (_result_All_ModMeshes == null)		{ _result_All_ModMeshes = new List<apModifiedMesh>(); }
			if (_result_All_ModBones == null)		{ _result_All_ModBones = new List<apModifiedBone>(); }
			
			_result_All_RenderUnits.Clear();
			_result_All_ModMeshes.Clear();
			_result_All_ModBones.Clear();

			if (_result_Gizmo_ModMeshes == null)	{ _result_Gizmo_ModMeshes = new List<apModifiedMesh>(); }
			if (_result_Gizmo_ModBones == null)		{ _result_Gizmo_ModBones = new List<apModifiedBone>(); }
			
			_result_Gizmo_Main_ModMesh = null;
			_result_Gizmo_Main_ModBone = null;
			_result_Gizmo_Main_RenderUnit = null;
			_result_Gizmo_ModMeshes.Clear();
			_result_Gizmo_ModBones.Clear();

			//MRV 리스트 (Pool이 아니며 현재 "선택된 ModMesh"로 부터 생성되어 "선택 가능한" MRV들)
			if (_mrv_List == null)			{ _mrv_List = new List<apSelection.ModRenderVert>(); }
			if (_modVert2MRV == null)		{ _modVert2MRV = new Dictionary<apModifiedVertex, apSelection.ModRenderVert>(); }
			if (_modVertRig2MRV == null)	{ _modVertRig2MRV = new Dictionary<apModifiedVertexRig, apSelection.ModRenderVert>(); }
			if (_modVertWeight2MRV == null)	{ _modVertWeight2MRV = new Dictionary<apModifiedVertexWeight, apSelection.ModRenderVert>(); }
			if (_vert2MRV == null)			{ _vert2MRV = new Dictionary<apTransform_Mesh, Dictionary<apVertex, apSelection.ModRenderVert>>(); }

			_mrv_List.Clear();
			_modVert2MRV.Clear();
			_modVertRig2MRV.Clear();
			_modVertWeight2MRV.Clear();
			_vert2MRV.Clear();

			//MRP 리스트 초기화
			if (_mrp_List == null)		{ _mrp_List = new List<apSelection.ModRenderPin>(); }
			if (_modPin2MRP == null)	{ _modPin2MRP = new Dictionary<apModifiedPin, apSelection.ModRenderPin>(); }
			if (_pin2MRP == null)		{ _pin2MRP = new Dictionary<apTransform_Mesh, Dictionary<apMeshPin, apSelection.ModRenderPin>>(); }
			
			_mrp_List.Clear();
			_modPin2MRP.Clear();
			_pin2MRP.Clear();

		}

		private void InitPool()
		{
			if (_mrvPool_All == null)	{ _mrvPool_All = new List<apSelection.ModRenderVert>(); }
			if (_mrvPool_Live == null)	{ _mrvPool_Live = new List<apSelection.ModRenderVert>(); }
			if (_mrvPool_Ready == null)	{ _mrvPool_Ready = new List<apSelection.ModRenderVert>(); }
			_mrvPool_All.Clear();
			_mrvPool_Live.Clear();
			_mrvPool_Ready.Clear();

			if (_mrpPool_All == null)	{ _mrpPool_All = new List<apSelection.ModRenderPin>(); }
			if (_mrpPool_Live == null)	{ _mrpPool_Live = new List<apSelection.ModRenderPin>(); }
			if (_mrpPool_Ready == null) { _mrpPool_Ready = new List<apSelection.ModRenderPin>(); }

			_mrpPool_All.Clear();
			_mrpPool_Live.Clear();
			_mrpPool_Ready.Clear();

			//Pool 크기를 처음으로 설정하자
			AddMRVUnits(MRVP_INIT_POOL_SIZE);
			AddMRPUnits(MRVP_INIT_POOL_SIZE);

		}
		// Functions
		//--------------------------------------------------------------------
		


		//AutoSelect 함수
		//-모디파이어 편집 모드와 애니메이션에 따라 다르다.



		/// <summary>
		/// AutoSelectModMeshOrModBone 함수의 역할을 하는 함수
		/// 변동사항이 있다면 true로 리턴한다. (선택된게 없어도 true를 리턴한다.)
		/// </summary>
		public bool SyncModData_ModEdit()
		{
			_mode = MODE.ModEdit;

			if(_selection.SelectionType != apSelection.SELECTION_TYPE.MeshGroup ||
				_selection.MeshGroup == null ||
				_selection.Modifier == null ||
				_selection.ParamSetOfMod == null ||
				_selection.SubEditedParamSetGroup == null)
			{
				//선택된게 없다.
				ClearAll();
				return true;
			}

			//선택 정보가 바뀌었는지 확인하자
			_prevRenderUnits.Clear();
			_prevModMeshes.Clear();
			_prevModBones.Clear();

			//기존 정보를 입력하자
			if(_result_All_RenderUnits.Count > 0)
			{
				for (int i = 0; i < _result_All_RenderUnits.Count; i++)
				{
					_prevRenderUnits.Add(_result_All_RenderUnits[i]);
				}
			}
			if(_result_All_ModMeshes.Count > 0)
			{	
				for (int i = 0; i < _result_All_ModMeshes.Count; i++)
				{
					_prevModMeshes.Add(_result_All_ModMeshes[i]);
				}
			}
			if(_result_All_ModBones.Count > 0)
			{
				for (int i = 0; i < _result_All_ModBones.Count; i++)
				{
					_prevModBones.Add(_result_All_ModBones[i]);
				}
			}

			apMeshGroup meshGroup = _selection.MeshGroup;
			apModifierBase modifier = _selection.Modifier;
			apModifierParamSet paramSet = _selection.ParamSetOfMod;
			//apModifierParamSetGroup paramSetGroup = _selection.SubEditedParamSetGroup;
			apMultiSubObjects subObjects = _selection.SubObjects;

			//일단 선택된 정보를 바탕으로 LinkData를 만들어야 한다.
			//이 함수안에 ClearAll이 포함되어 있다. < 중요!
			SyncSelectedObjectToLinkUnit(subObjects);

			//LinkData의 내용을 넣어주자
			
			apModifiedMesh curModMesh = null;
			LinkUnit linkUnit = null;
			
			
			for (int i = 0; i < paramSet._meshData.Count; i++)
			{
				curModMesh = paramSet._meshData[i];

				if(curModMesh._transform_Mesh != null)
				{
					if(_keyMeshTF_2_LinkUnit.ContainsKey(curModMesh._transform_Mesh))
					{
						//선택된 MeshTF타입의 ModMesh이다. 값을 넣어주자
						linkUnit = _keyMeshTF_2_LinkUnit[curModMesh._transform_Mesh];
						if(linkUnit == null) { continue; }

						linkUnit.SetModMesh(curModMesh, modifier);
					}
				}
				else if(curModMesh._transform_MeshGroup != null)
				{
					if(_keyMeshGroupTF_2_LinkUnit.ContainsKey(curModMesh._transform_MeshGroup))
					{
						//선택된 MeshGroupTF타입의 ModMesh이다. 값을 넣어주자
						linkUnit = _keyMeshGroupTF_2_LinkUnit[curModMesh._transform_MeshGroup];
						if(linkUnit == null) { continue; }

						linkUnit.SetModMesh(curModMesh, modifier);
					}
				}
			}

			apModifiedBone curModBone = null;
			for (int i = 0; i < paramSet._boneData.Count; i++)
			{
				curModBone = paramSet._boneData[i];
				if(curModBone._bone != null)
				{
					if(_keyBone_2_LinkUnit.ContainsKey(curModBone._bone))
					{
						//선택된 Bone타입의 ModBone이다. 값을 넣어주자
						linkUnit = _keyBone_2_LinkUnit[curModBone._bone];
						if(linkUnit == null) { continue; }

						linkUnit.SetModBone(curModBone);
					}
				}
			}


			//마지막으로 결과를 정리하자.
			return MakeResult();
		}





		//애니메이션 선택시의 ModMesh/ModBone을 계산하자.
		/// <summary>
		/// apMultiSubObject에서 오브젝트와 타임라인 레이어들을 선택한 상태여야 한다.
		/// </summary>
		public bool SyncModData_AnimEdit()
		{
			_mode = MODE.AnimEdit;

			if(_selection.SelectionType != apSelection.SELECTION_TYPE.Animation ||
				_selection.AnimClip == null ||
				_selection.AnimClip._targetMeshGroup == null ||
				_selection.AnimTimeline == null || //<<이걸 자동으로 찾는 기능은 없다. 외부에서 찾아올 것.
				_selection.IsAnimPlaying//애니메이션이 재생 중일 때에도 찾지 않는다.
				)
			{
				//선택된게 없다. > 이것도 True
				ClearAll();
				return true;
			}

			apMultiSubObjects subObjects = _selection.SubObjects;

			//일단 선택된 정보를 바탕으로 LinkData를 만들어야 한다.
			//이 함수안에 ClearAll이 포함되어 있다.
			SyncSelectedObjectToLinkUnit(subObjects);

			//LinkData의 내용을 넣어주자
			//타임라인의 타임라인 레이어들을 찾아서 체크하자.
			
			LinkUnit linkUnit = null;
			
			//선택된 WorkKeyframe들을 기준으로 찾아보자.
			//애니메이션은 WorkKeyframe을 기준으로 선택하자.
			int nWorkKeyframes = subObjects.NumWorkKeyframes;
			List<apAnimKeyframe> workKeyframes = subObjects.AllWorkKeyframes;
			apAnimKeyframe curWorkKeyframe = null;
			apAnimTimelineLayer curTimelineLayer = null;
			apModifierBase linkedModifier = null;

			
			for (int iWK = 0; iWK < nWorkKeyframes; iWK++)
			{
				curWorkKeyframe = workKeyframes[iWK];
				if(curWorkKeyframe == null ||
					curWorkKeyframe._parentTimelineLayer == null)
				{
					continue;
				}

				curTimelineLayer = curWorkKeyframe._parentTimelineLayer;
				if(curTimelineLayer._parentTimeline == null
					|| curTimelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam
					|| curTimelineLayer._parentTimeline._linkedModifier == null
					)
				{
					continue;
				}
				
				linkedModifier = curTimelineLayer._parentTimeline._linkedModifier;

				if(curTimelineLayer._linkedMeshTransform != null
					&& curWorkKeyframe._linkedModMesh_Editor != null)
				{
					if(_keyMeshTF_2_LinkUnit.ContainsKey(curTimelineLayer._linkedMeshTransform))
					{
						//선택된 MeshTF타입의 ModMesh이다. 값을 넣어주자
						linkUnit = _keyMeshTF_2_LinkUnit[curTimelineLayer._linkedMeshTransform];
						if (linkUnit == null) { continue; }

						linkUnit.SetModMesh(curWorkKeyframe._linkedModMesh_Editor, linkedModifier);
						linkUnit.SetAnimKeyframe(curWorkKeyframe);
					}
				}
				else if(curTimelineLayer._linkedMeshGroupTransform != null
					&& curWorkKeyframe._linkedModMesh_Editor != null)
				{
					if(_keyMeshGroupTF_2_LinkUnit.ContainsKey(curTimelineLayer._linkedMeshGroupTransform))
					{
						//선택된 MeshGroupTF타입의 ModMesh이다. 값을 넣어주자
						linkUnit = _keyMeshGroupTF_2_LinkUnit[curTimelineLayer._linkedMeshGroupTransform];
						if (linkUnit == null) { continue; }

						linkUnit.SetModMesh(curWorkKeyframe._linkedModMesh_Editor, linkedModifier);
						linkUnit.SetAnimKeyframe(curWorkKeyframe);
					}
				}
				else if(curTimelineLayer._linkedBone != null
					&& curWorkKeyframe._linkedModBone_Editor != null)
				{
					if(_keyBone_2_LinkUnit.ContainsKey(curTimelineLayer._linkedBone))
					{
						//선택된 MeshGroupTF타입의 ModMesh이다. 값을 넣어주자
						linkUnit = _keyBone_2_LinkUnit[curTimelineLayer._linkedBone];
						if (linkUnit == null) { continue; }

						linkUnit.SetModBone(curWorkKeyframe._linkedModBone_Editor);
						linkUnit.SetAnimKeyframe(curWorkKeyframe);
					}
				}
			}

			


			//마지막으로 결과를 정리하자.
			return MakeResult();
		}



		//--------------------------------------------------------------------

		//결과를 정리하자
		private bool MakeResult()
		{
			//일단 결과는 초기화되었다는 가정. (이 함수가 호출될 시점에서는 초기화되었어야 한다.)
			_result_IsMultipleModData = false;//여러개의 모드 데이터를 찾았는가. (여러개를 선택했지만, 모드 데이터는 하나일 수 있다.)
			_result_NumModData = 0;
			_result_Main_RenderUnit = null;
			_result_Main_ModMesh = null;
			_result_Main_ModBone = null;

			if (_result_All_RenderUnits == null) { _result_All_RenderUnits = new List<apRenderUnit>(); }
			if (_result_All_ModMeshes == null) { _result_All_ModMeshes = new List<apModifiedMesh>(); }
			if (_result_All_ModBones == null) { _result_All_ModBones = new List<apModifiedBone>(); }

			_result_All_RenderUnits.Clear();
			_result_All_ModMeshes.Clear();
			_result_All_ModBones.Clear();

			if (_result_Gizmo_ModMeshes == null) { _result_Gizmo_ModMeshes = new List<apModifiedMesh>(); }
			if (_result_Gizmo_ModBones == null) { _result_Gizmo_ModBones = new List<apModifiedBone>(); }

			_result_Gizmo_Main_ModMesh = null;
			_result_Gizmo_Main_ModBone = null;
			_result_Gizmo_Main_RenderUnit = null;
			_result_Gizmo_ModMeshes.Clear();
			_result_Gizmo_ModBones.Clear();

			//전체 LinkUnit을 검색하자.
			int nLinkUnits = _linkUnits_All.Count;
			LinkUnit curLinkUnit = null;
			for (int iLU = 0; iLU < nLinkUnits; iLU++)
			{
				curLinkUnit = _linkUnits_All[iLU];
				if (curLinkUnit == null)
				{
					continue;
				}
				if (!curLinkUnit._isValidData)
				{
					//연결된 데이터가 없다면..
					continue;
				}
				if (curLinkUnit._unitType == SELECTION_TYPE.Main)
				{
					//메인은 별도로 데이터를 넣어주자.
					_result_Main_RenderUnit = curLinkUnit._linkedRenderUnit;
					_result_Main_ModMesh = curLinkUnit._modMesh;
					_result_Main_ModBone = curLinkUnit._modBone;
				}

				//All 리스트에 값을 넣되 null이 아닌 경우만.
				//즉, 모든 리스트의 크기가 _result_NumModData과 같은건 아니다. (더 작다.)
				if (curLinkUnit._linkedRenderUnit != null)
				{
					_result_All_RenderUnits.Add(curLinkUnit._linkedRenderUnit);
				}
				if (curLinkUnit._modMesh != null)
				{
					_result_All_ModMeshes.Add(curLinkUnit._modMesh);

					if (curLinkUnit._isGizmoSelected)
					{
						//기즈모 편집이 가능하다면
						_result_Gizmo_ModMeshes.Add(curLinkUnit._modMesh);

						//만약 이게 기즈모 메인이면
						if (curLinkUnit._isGizmoMain)
						{
							_result_Gizmo_Main_ModMesh = curLinkUnit._modMesh;
							_result_Gizmo_Main_RenderUnit = curLinkUnit._linkedRenderUnit;
						}
					}
				}
				if (curLinkUnit._modBone != null)
				{
					_result_All_ModBones.Add(curLinkUnit._modBone);

					if (curLinkUnit._isGizmoSelected)
					{
						//기즈모 편집이 가능하다면
						_result_Gizmo_ModBones.Add(curLinkUnit._modBone);

						//만약 이게 기즈모 메인이면
						if (curLinkUnit._isGizmoMain)
						{
							_result_Gizmo_Main_ModBone = curLinkUnit._modBone;
						}
					}
				}

				_result_NumModData++;
			}

			if (_result_NumModData > 1)
			{
				_result_IsMultipleModData = true;
			}
			else
			{
				_result_IsMultipleModData = false;
			}

			//만약 기즈모 메인이 없다면
			//메인을 기즈모 메인으로 설정한다.
			if ((_result_Gizmo_Main_ModMesh == null && _result_Gizmo_Main_ModBone == null)
				&& (_result_Main_ModMesh != null || _result_Main_ModBone != null)
				)
			{
				if(_result_Main_ModMesh != null)
				{
					_result_Gizmo_Main_ModMesh = _result_Main_ModMesh;
					_result_Gizmo_Main_RenderUnit = _result_Main_RenderUnit;
				}
				if(_result_Main_ModBone != null)
				{
					_result_Gizmo_Main_ModBone = _result_Main_ModBone;
				}
			}


			return CheckIsResultChanged();
		}




		//이전 결과를 저장하자
		private void CopyPrevResult()
		{
			_prevRenderUnits.Clear();
			_prevModMeshes.Clear();
			_prevModBones.Clear();

			//기존 정보를 입력하자
			if(_result_All_RenderUnits.Count > 0)
			{
				for (int i = 0; i < _result_All_RenderUnits.Count; i++)
				{
					_prevRenderUnits.Add(_result_All_RenderUnits[i]);
				}
			}
			if(_result_All_ModMeshes.Count > 0)
			{	
				for (int i = 0; i < _result_All_ModMeshes.Count; i++)
				{
					_prevModMeshes.Add(_result_All_ModMeshes[i]);
				}
			}
			if(_result_All_ModBones.Count > 0)
			{
				for (int i = 0; i < _result_All_ModBones.Count; i++)
				{
					_prevModBones.Add(_result_All_ModBones[i]);
				}
			}
		}

		//결과가 바뀌었는지 체크하자
		private bool CheckIsResultChanged()
		{
			//RenderUnit, ModMesh, ModBone 중 하나라도 바뀐게 있다면 Changed
			//일단 개수 체크
			if(_result_All_RenderUnits.Count != _prevRenderUnits.Count)
			{
				return true;
			}
			if(_result_All_ModMeshes.Count != _prevModMeshes.Count)
			{
				return true;
			}
			if(_result_All_ModBones.Count != _prevModBones.Count)
			{
				return true;
			}

			//이제 개수는 다 동일하니,
			//리스트에 겹치지 않는게 있는지 확인하자
			if(_result_All_RenderUnits.Count > 0)
			{
				for (int i = 0; i < _result_All_RenderUnits.Count; i++)
				{
					if(!_prevRenderUnits.Contains(_result_All_RenderUnits[i]))
					{
						return true;
					}
				}
			}
			if(_result_All_ModMeshes.Count > 0)
			{	
				for (int i = 0; i < _result_All_ModMeshes.Count; i++)
				{
					if(!_prevModMeshes.Contains(_result_All_ModMeshes[i]))
					{
						return true;
					}
				}
			}
			if(_result_All_ModBones.Count > 0)
			{
				for (int i = 0; i < _result_All_ModBones.Count; i++)
				{
					if(!_prevModBones.Contains(_result_All_ModBones[i]))
					{
						return true;
					}
				}
			}

			//선택된게 없어도 true
			if(!IsAnyRenderUnit && !IsAnyModMesh && !IsAnyModBone)
			{
				return true;
			}

			return false;
		}


		// Pool 함수들 : MRV

		private void AddMRVUnits(int nIncrement)
		{
			for (int i = 0; i < nIncrement; i++)
			{
				apSelection.ModRenderVert newMRV = new apSelection.ModRenderVert();
				_mrvPool_All.Add(newMRV);
				_mrvPool_Ready.Add(newMRV);
			}
		}

		private apSelection.ModRenderVert PopMRV()
		{
			if(_mrvPool_Ready.Count == 0)
			{
				//남아있는게 없다. > 크기 증가
				AddMRVUnits(MRVP_ADD_POOL_SIZE);
			}

			//하나를 꺼내자
			apSelection.ModRenderVert popUnit = _mrvPool_Ready[0];
			_mrvPool_Ready.RemoveAt(0);
			_mrvPool_Live.Add(popUnit);

			return popUnit;
		}

		private void PushMRV(apSelection.ModRenderVert mrv)
		{
			//반납한다.
			if(!_mrvPool_Ready.Contains(mrv))
			{
				_mrvPool_Ready.Add(mrv);
			}
			if(_mrvPool_Live.Contains(mrv))
			{
				_mrvPool_Live.Remove(mrv);
			}
		}

		private void PushAllMRV()
		{
			//모든 MRV를 Ready 상태로 만든다.
			int nMRV = _mrvPool_All.Count;
			_mrvPool_Live.Clear();
			_mrvPool_Ready.Clear();
			for (int i = 0; i < nMRV; i++)
			{
				_mrvPool_Ready.Add(_mrvPool_All[i]);
			}
		}


		//현재 사용하는 Mod의 MRV 추가하기
		public void AddMRVOfMod(apSelection.ModRenderVert mrv)
		{
			_mrv_List.Add(mrv);
			if(mrv._modVert != null)
			{
				_modVert2MRV.Add(mrv._modVert, mrv);
			}
			if(mrv._modVertRig != null)
			{
				_modVertRig2MRV.Add(mrv._modVertRig, mrv);
			}
			if(mrv._modVertWeight != null)
			{
				_modVertWeight2MRV.Add(mrv._modVertWeight, mrv);
			}	
			if(mrv._renderVert != null 
				&& mrv._renderVert._vertex != null
				&& mrv._modVert != null
				&& mrv._modVert._modifiedMesh != null
				&& mrv._modVert._modifiedMesh._transform_Mesh != null)
			{
				//mrv._modVert._modifiedMesh._transform_Mesh
				apTransform_Mesh meshTF = mrv._modVert._modifiedMesh._transform_Mesh;
				if(!_vert2MRV.ContainsKey(meshTF))
				{
					_vert2MRV.Add(meshTF, new Dictionary<apVertex, apSelection.ModRenderVert>());
				}
				_vert2MRV[meshTF].Add(mrv._renderVert._vertex, mrv);
			}
			
		}




		// Pool 함수들 : MRP

		private void AddMRPUnits(int nIncrement)
		{
			for (int i = 0; i < nIncrement; i++)
			{
				apSelection.ModRenderPin newMRP = new apSelection.ModRenderPin();
				_mrpPool_All.Add(newMRP);
				_mrpPool_Ready.Add(newMRP);
			}
		}

		private apSelection.ModRenderPin PopMRP()
		{
			if(_mrpPool_Ready.Count == 0)
			{
				//남아있는게 없다. > 크기 증가
				AddMRPUnits(MRVP_ADD_POOL_SIZE);
			}

			//하나를 꺼내자
			apSelection.ModRenderPin popUnit = _mrpPool_Ready[0];
			_mrpPool_Ready.RemoveAt(0);
			_mrpPool_Live.Add(popUnit);

			return popUnit;
		}

		private void PushMRP(apSelection.ModRenderPin mrp)
		{
			//반납한다.
			if(!_mrpPool_Ready.Contains(mrp))
			{
				_mrpPool_Ready.Add(mrp);
			}
			if(_mrpPool_Live.Contains(mrp))
			{
				_mrpPool_Live.Remove(mrp);
			}
		}

		private void PushAllMRP()
		{
			//모든 MRP를 Ready 상태로 만든다.
			int nMRP = _mrpPool_All.Count;
			_mrpPool_Live.Clear();
			_mrpPool_Ready.Clear();
			for (int i = 0; i < nMRP; i++)
			{
				_mrpPool_Ready.Add(_mrpPool_All[i]);
			}
		}


		//현재 사용하는 Mod의 MRP 추가하기
		public void AddMRPOfMod(apSelection.ModRenderPin mrp)
		{
			_mrp_List.Add(mrp);
			if(mrp._modPin != null)
			{
				_modPin2MRP.Add(mrp._modPin, mrp);
			}
			if(mrp._renderPin != null 
				&& mrp._renderPin._srcPin != null
				&& mrp._modPin != null
				&& mrp._modPin._modifiedMesh != null
				&& mrp._modPin._modifiedMesh._transform_Mesh != null)
			{
				apTransform_Mesh meshTF = mrp._modPin._modifiedMesh._transform_Mesh;
				if(!_pin2MRP.ContainsKey(meshTF))
				{
					_pin2MRP.Add(meshTF, new Dictionary<apMeshPin, apSelection.ModRenderPin>());
				}
				_pin2MRP[meshTF].Add(mrp._renderPin._srcPin, mrp);
			}
			
		}




		//----------------------------------------------------------------------------


		// 동기화 후 LinkUnit 생성 함수들
		private void SyncSelectedObjectToLinkUnit(apMultiSubObjects subObjects)
		{
			ClearAll();

			int nMeshTF = subObjects.NumMeshTF;
			int nMeshGroupTF = subObjects.NumMeshGroupTF;
			int nBone = subObjects.NumBone;

			//Debug.Log(">> NumMeshTF : " + nMeshTF);
			//Debug.Log(">> nMeshGroupTF : " + nMeshGroupTF);
			//Debug.Log(">> nBone : " + nBone);

			if(nMeshTF > 0)
			{
				List<apTransform_Mesh> meshTFs = subObjects.AllMeshTFs;
				apTransform_Mesh mainMeshTF = subObjects.MeshTF;
				apTransform_Mesh curMeshTF = null;

				for (int i = 0; i < nMeshTF; i++)
				{
					curMeshTF = meshTFs[i];
					MakeLinkUnit(	curMeshTF, 
									mainMeshTF == curMeshTF ? SELECTION_TYPE.Main : SELECTION_TYPE.Sub,
									subObjects.AllGizmoMeshTFs.Contains(curMeshTF),
									subObjects.GizmoMeshTF == curMeshTF);
				}
			}
			if(nMeshGroupTF > 0)
			{
				List<apTransform_MeshGroup> meshGroupTFs = subObjects.AllMeshGroupTFs;
				apTransform_MeshGroup mainMeshGroupTF = subObjects.MeshGroupTF;
				apTransform_MeshGroup curMeshGroupTF = null;

				for (int i = 0; i < nMeshGroupTF; i++)
				{
					curMeshGroupTF = meshGroupTFs[i];
					MakeLinkUnit(	curMeshGroupTF, 
									mainMeshGroupTF == curMeshGroupTF ? SELECTION_TYPE.Main : SELECTION_TYPE.Sub,
									subObjects.AllGizmoMeshGroupTFs.Contains(curMeshGroupTF),
									subObjects.GizmoMeshGroupTF == curMeshGroupTF);
				}
			}
			if (nBone > 0)
			{
				List<apBone> bones = subObjects.AllBones;
				apBone mainBone = subObjects.Bone;
				apBone curBone = null;

				for (int i = 0; i < nBone; i++)
				{
					curBone = bones[i];
					MakeLinkUnit(	curBone, 
									mainBone == curBone ? SELECTION_TYPE.Main : SELECTION_TYPE.Sub,
									subObjects.AllGizmoBones.Contains(curBone),
									subObjects.GizmoBone == curBone);
				}
			}
		}

		private void MakeLinkUnit(apTransform_Mesh meshTF, SELECTION_TYPE selectionType, bool isGizmoSelected, bool isGizmoMain)
		{
			if(_keyMeshTF_2_LinkUnit.ContainsKey(meshTF))
			{
				//이미 등록되었다.
				return;
			}
			
			LinkUnit newLinkUnit = new LinkUnit(this);
			newLinkUnit.SetKey(meshTF, isGizmoSelected, isGizmoMain);
			newLinkUnit.SetSelectionType(selectionType);
			
			_linkUnits_All.Add(newLinkUnit);

			_keyObject_2_LinkUnit.Add(meshTF, newLinkUnit);
			_keyMeshTF_2_LinkUnit.Add(meshTF, newLinkUnit);

			if(selectionType == SELECTION_TYPE.Main && _linkUnit_Main == null)
			{
				_linkUnit_Main = newLinkUnit;
			}
			else
			{
				_linkUnits_Sub.Add(newLinkUnit);
			}
		}

		private void MakeLinkUnit(apTransform_MeshGroup meshGroupTF, SELECTION_TYPE selectionType, bool isGizmoSelected, bool isGizmoMain)
		{
			if(_keyMeshGroupTF_2_LinkUnit.ContainsKey(meshGroupTF))
			{
				//이미 등록되었다.
				return;
			}
			
			LinkUnit newLinkUnit = new LinkUnit(this);
			newLinkUnit.SetKey(meshGroupTF, isGizmoSelected, isGizmoMain);
			newLinkUnit.SetSelectionType(selectionType);
			
			_linkUnits_All.Add(newLinkUnit);

			_keyObject_2_LinkUnit.Add(meshGroupTF, newLinkUnit);
			_keyMeshGroupTF_2_LinkUnit.Add(meshGroupTF, newLinkUnit);

			if(selectionType == SELECTION_TYPE.Main && _linkUnit_Main == null)
			{
				_linkUnit_Main = newLinkUnit;
			}
			else
			{
				_linkUnits_Sub.Add(newLinkUnit);
			}
		}

		private void MakeLinkUnit(apBone bone, SELECTION_TYPE selectionType, bool isGizmoSelected, bool isGizmoMain)
		{
			if(_keyBone_2_LinkUnit.ContainsKey(bone))
			{
				//이미 등록되었다.
				return;
			}
			
			LinkUnit newLinkUnit = new LinkUnit(this);
			newLinkUnit.SetKey(bone, isGizmoSelected, isGizmoMain);
			newLinkUnit.SetSelectionType(selectionType);
			
			_linkUnits_All.Add(newLinkUnit);

			_keyObject_2_LinkUnit.Add(bone, newLinkUnit);
			_keyBone_2_LinkUnit.Add(bone, newLinkUnit);

			if(selectionType == SELECTION_TYPE.Main && _linkUnit_Main == null)
			{
				_linkUnit_Main = newLinkUnit;
			}
			else
			{
				_linkUnits_Sub.Add(newLinkUnit);
			}
		}

		// Get / Set
		//--------------------------------------------------------------------
		public bool IsMultiple { get { return _result_IsMultipleModData; } }
		public apModifiedMesh ModMesh	{ get { return _result_Main_ModMesh; } }
		public apModifiedBone ModBone	{ get { return _result_Main_ModBone; } }
		public apRenderUnit RenderUnit	{ get { return _result_Main_RenderUnit; } }
		public List<apModifiedMesh> ModMeshes_All	{ get { return _result_All_ModMeshes; } }
		public List<apModifiedBone> ModBones_All	{ get { return _result_All_ModBones; } }
		public List<apRenderUnit> RenderUnits_All	{ get { return _result_All_RenderUnits; } }

		//리스트 말고, 기즈모용 Main이 있으니 그것도 동기화할것
		public apModifiedMesh ModMesh_Gizmo { get { return _result_Gizmo_Main_ModMesh; } }
		public apModifiedBone ModBone_Gizmo { get { return _result_Gizmo_Main_ModBone; } }
		public apRenderUnit RenderUnit_Gizmo { get { return _result_Gizmo_Main_RenderUnit; } }
		public List<apModifiedMesh> ModMeshes_Gizmo_All	{ get { return _result_Gizmo_ModMeshes; } }
		public List<apModifiedBone> ModBones_Gizmo_All	{ get { return _result_Gizmo_ModBones; } }

		public bool IsAnyModMesh { get { return _result_All_ModMeshes.Count > 0; } }
		public bool IsAnyModBone { get { return _result_All_ModBones.Count > 0; } }
		public bool IsAnyRenderUnit { get { return _result_All_RenderUnits.Count > 0; } }

		public List<apSelection.ModRenderVert> ModRenderVert_All { get { return _mrv_List; } }
		public int NumModRenderVert_All { get { return _mrv_List.Count; } }

		public List<apSelection.ModRenderPin> ModRenderPin_All { get { return _mrp_List; } }
		public int NumModRenderPin_All { get { return _mrp_List.Count; } }

		public apSelection.ModRenderVert GetMRV(apModifiedVertex modVert)
		{
			if(_modVert2MRV.ContainsKey(modVert))
			{
				return _modVert2MRV[modVert];
			}
			return null;
		}
		
		public apSelection.ModRenderVert GetMRV(apModifiedVertexRig modVertRig)
		{
			if(_modVertRig2MRV.ContainsKey(modVertRig))
			{
				return _modVertRig2MRV[modVertRig];
			}
			return null;
		}

		public apSelection.ModRenderVert GetMRV(apModifiedVertexWeight modVertWeight)
		{
			if(_modVertWeight2MRV.ContainsKey(modVertWeight))
			{
				return _modVertWeight2MRV[modVertWeight];
			}
			return null;
		}

		
		public apSelection.ModRenderVert GetMRV(apTransform_Mesh meshTransform, apVertex vert)
		{
			if(_vert2MRV.ContainsKey(meshTransform))
			{
				_tmp_GetMRV_Dictionary = _vert2MRV[meshTransform];
				if(_tmp_GetMRV_Dictionary.ContainsKey(vert))
				{
					return _tmp_GetMRV_Dictionary[vert];
				}
			}
			return null;
		}

		public apSelection.ModRenderPin GetMRP(apModifiedPin modPin)
		{
			if(_modPin2MRP.ContainsKey(modPin))
			{
				return _modPin2MRP[modPin];
			}
			return null;
		}

		public apSelection.ModRenderPin GetMRP(apTransform_Mesh meshTransform, apMeshPin pin)
		{
			if(_pin2MRP.ContainsKey(meshTransform))
			{
				_tmp_GetMRP_Dictionary = _pin2MRP[meshTransform];
				if(_tmp_GetMRP_Dictionary.ContainsKey(pin))
				{
					return _tmp_GetMRP_Dictionary[pin];
				}
			}
			return null;
		}
	}
}