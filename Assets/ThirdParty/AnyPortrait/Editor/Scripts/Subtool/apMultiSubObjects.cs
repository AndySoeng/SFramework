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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AnyPortrait
{
	//추가 20.6.1 
	//apSelection에서 사용되는 "MeshGroup"의 선택된 객체들. (TF, Bone)
	//선택 함수 및 특정 속성이 동일한지 알려준다.
	public class apMultiSubObjects
	{
		// Members
		//-------------------------------------------------------
		//private apSelection _selection = null;


		private apTransform_Mesh _meshTF = null;
		private apTransform_MeshGroup _meshGroupTF = null;
		private apBone _bone = null;

		private List<apTransform_Mesh> _meshTF_List_Sub = new List<apTransform_Mesh>();
		private List<apTransform_Mesh> _meshTF_List_All = new List<apTransform_Mesh>();

		//기즈모 처리용
		private apTransform_Mesh _meshTF_GizmoMain = null;//기즈모 전체 중 메인
		private List<apTransform_Mesh> _meshTF_List_Gizmo = new List<apTransform_Mesh>();//기즈모 전체

		private List<apTransform_MeshGroup> _meshGroupTF_List_Sub = new List<apTransform_MeshGroup>();
		private List<apTransform_MeshGroup> _meshGroupTF_List_All = new List<apTransform_MeshGroup>();

		//기즈모 처리용
		private apTransform_MeshGroup _meshGroupTF_GizmoMain = null;//기즈모 전체 중 메인
		private List<apTransform_MeshGroup> _meshGroupTF_List_Gizmo = new List<apTransform_MeshGroup>();//기즈모 전체

		private List<apBone> _bone_List_Sub = new List<apBone>();
		private List<apBone> _bone_List_All = new List<apBone>();

		//기즈모 처리용
		private apBone _bone_GizmoMain = null;
		private List<apBone> _bone_List_Gizmo = new List<apBone>();

		//자식 메시 그룹이나 거기에 속한 객체들이 선택되었는가.
		private bool _isChildMeshGroupObjectSelected_Gizmo_TF = false;
		private bool _isChildMeshGroupObjectSelected_Gizmo_Bone = false;

		//애니메이션의 경우, ControlParam도 선택한다.
		//단 이건 다중 선택은 안된다.
		private apControlParam _controlParam_Anim = null;

		

		//각 선택된 객체의 개수 (All List의 개수와 같다.)
		private int _nMeshTF = 0;
		private int _nMeshGroupTF = 0;
		private int _nBone = 0;
		

		//TimelineLayer도 선택한다.
		//TimelineLayer <-> LinkedObject도 선택할 수 있다.
		private apAnimTimeline _timeline = null;
		private apAnimTimelineLayer _timelineLayer = null;
		private List<apAnimTimelineLayer> _timelineLayers_Sub = new List<apAnimTimelineLayer>();
		private List<apAnimTimelineLayer> _timelineLayers_All = new List<apAnimTimelineLayer>();

		//추가 : 기즈모용
		private apAnimTimelineLayer _timelineLayer_Gizmo = null;

		private int _nTimelineLayers = 0;

		//WorkKeyframe까지 찾자.
		//이것까지해야 MultiSubObject에서의 처리가 끝난다.		
		private apAnimKeyframe _workKeyframe = null;
		private List<apAnimKeyframe> _workKeyframes_Sub = new List<apAnimKeyframe>();
		private List<apAnimKeyframe> _workKeyframes_All = new List<apAnimKeyframe>();
		private int _nWorkKeyframes = 0;
		private int _workAnimFrameIndex = -1;//Work Frame을 기록할 때 애니메이션 프레임도 기록한다.

		




		private const float FLOAT_APPROX_BIAS = 0.0001f;
		private const float COLOR_APPROX_BIAS = 0.002f;


		//속성 동기화 연산 결과 (변수를 하나만 사용하자)
		public class SyncResult
		{
			private bool _isValid = false;//값을 설정할 수 없다.
			private bool _isSync = false;
			private bool _isFirstInput = false;

			private bool _syncValue_Bool = false;
			private int _syncValue_Int = 0;
			private float _syncValue_Float = 0.0f;
			private Vector2 _syncValue_Vec2 = Vector2.zero;
			private Vector3 _syncValue_Vec3 = Vector3.zero;
			private Vector4 _syncValue_Vec4 = Vector4.zero;
			private Color _syncValue_Color = Color.black;

			public bool IsValid { get { return _isValid; } }
			public bool IsSync	{ get { return _isSync; } }

			public bool SyncValue_Bool		{ get { return _syncValue_Bool; } }
			public int SyncValue_Int		{ get { return _syncValue_Int; } }
			public float SyncValue_Float	{ get { return _syncValue_Float; } }
			public Vector2 SyncValue_Vec2	{ get { return _syncValue_Vec2; } }
			public Vector3 SyncValue_Vec3	{ get { return _syncValue_Vec3; } }
			public Vector4 SyncValue_Vec4	{ get { return _syncValue_Vec4; } }
			public Color SyncValue_Color	{ get { return _syncValue_Color; } }
			

			public SyncResult()
			{
				ReadyToSync();
			}

			public void ReadyToSync()
			{
				_isValid = true;
				_isSync = true;
				_isFirstInput = true;

				_syncValue_Bool = false;
				_syncValue_Int = 0;
				_syncValue_Float = 0.0f;
				_syncValue_Vec2 = Vector2.zero;
				_syncValue_Vec3 = Vector3.zero;
				_syncValue_Vec4 = Vector4.zero;
				_syncValue_Color = Color.black;
			}

			public void SetInvalid()
			{
				_isValid = false;
			}

			//값 입력 및 동기화 처리
			//첫 입력시에는 동기화 값을 그냥 대입한다.
			public void SetValue_Bool(bool value)
			{
				if(_isFirstInput)
				{	
					_syncValue_Bool = value;//첫 입력시에는 그냥 값을 대입
					_isFirstInput = false;
				}
				else
				{
					if(!_isSync) { return; }

					if(_syncValue_Bool != value)
					{
						_isSync = false;//값이 다르다. 동기화 실패
					}
				}
			}
			
			public void SetValue_Int(int value)
			{
				if(_isFirstInput)
				{	
					_syncValue_Int = value;//첫 입력시에는 그냥 값을 대입
					_isFirstInput = false;
				}
				else
				{
					if(!_isSync) { return; }

					if(_syncValue_Int != value)
					{
						_isSync = false;//값이 다르다. 동기화 실패
					}
				}
			}

			public void SetValue_Float(float value)
			{
				if(_isFirstInput)
				{	
					_syncValue_Float = value;//첫 입력시에는 그냥 값을 대입
					_isFirstInput = false;
				}
				else
				{
					if(!_isSync) { return; }

					if(Mathf.Abs(_syncValue_Float - value) > FLOAT_APPROX_BIAS)
					{
						_isSync = false;//값이 다르다. 동기화 실패
					}
				}
			}

			public void SetValue_Vector4(Vector4 value)
			{
				if(_isFirstInput)
				{	
					_syncValue_Vec4 = value;//첫 입력시에는 그냥 값을 대입
					_isFirstInput = false;
				}
				else
				{
					if(!_isSync) { return; }

					//if(_syncValue_Float != value)
					if(	Mathf.Abs(_syncValue_Vec4.x - value.x) > FLOAT_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Vec4.y - value.y) > FLOAT_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Vec4.z - value.z) > FLOAT_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Vec4.w - value.w) > FLOAT_APPROX_BIAS)
					{
						_isSync = false;//값이 다르다. 동기화 실패
					}
				}
			}

			public void SetValue_Vector3(Vector3 value)
			{
				if(_isFirstInput)
				{	
					//첫 입력시에는 그냥 값을 대입
					_syncValue_Vec3 = value;
					_isFirstInput = false;
				}
				else
				{
					if(!_isSync) { return; }

					//if(_syncValue_Float != value)
					if(	Mathf.Abs(_syncValue_Vec3.x - value.x) > FLOAT_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Vec3.y - value.y) > FLOAT_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Vec3.z - value.z) > FLOAT_APPROX_BIAS)
					{
						_isSync = false;//값이 다르다. 동기화 실패
					}
				}
			}

			public void SetValue_Vector2(Vector2 value)
			{
				if(_isFirstInput)
				{	
					//첫 입력시에는 그냥 값을 대입
					_syncValue_Vec2 = value;
					_isFirstInput = false;
				}
				else
				{
					if(!_isSync) { return; }

					//if(_syncValue_Float != value)
					if(	Mathf.Abs(_syncValue_Vec2.x - value.x) > FLOAT_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Vec2.y - value.y) > FLOAT_APPROX_BIAS)
					{
						_isSync = false;//값이 다르다. 동기화 실패
					}
				}
			}


			public void SetValue_Color(Color value)
			{
				if(_isFirstInput)
				{	
					//첫 입력시에는 그냥 값을 대입
					_syncValue_Color = value;
					_isFirstInput = false;
				}
				else
				{
					if(!_isSync) { return; }

					//if(_syncValue_Float != value)
					if(	Mathf.Abs(_syncValue_Color.r - value.r) > COLOR_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Color.g - value.g) > COLOR_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Color.b - value.b) > COLOR_APPROX_BIAS ||
						Mathf.Abs(_syncValue_Color.a - value.a) > COLOR_APPROX_BIAS)
					{
						_isSync = false;//값이 다르다. 동기화 실패
					}
				}
			}

		}


		private const int NUM_SYNC = 20;
		private SyncResult[] _sync = null;


		//계산용 변수
		private List<apAnimTimelineLayer> _prevTimelineLayers = null;


		// Init
		//-------------------------------------------------------
		public apMultiSubObjects(apSelection selection)
		{
			//_selection = selection;

			_meshTF = null;
			_meshGroupTF = null;
			_bone = null;
			_controlParam_Anim = null;

			if (_meshTF_List_Sub == null)	{ _meshTF_List_Sub = new List<apTransform_Mesh>(); }
			if (_meshTF_List_All == null)	{ _meshTF_List_All = new List<apTransform_Mesh>(); }
			if (_meshTF_List_Gizmo == null) { _meshTF_List_Gizmo = new List<apTransform_Mesh>(); }

			_meshTF_List_Sub.Clear();
			_meshTF_List_All.Clear();

			_meshTF_GizmoMain = null;
			_meshTF_List_Gizmo.Clear();


			if (_meshGroupTF_List_Sub == null)		{ _meshGroupTF_List_Sub = new List<apTransform_MeshGroup>(); }
			if (_meshGroupTF_List_All == null)		{ _meshGroupTF_List_All = new List<apTransform_MeshGroup>(); }
			if (_meshGroupTF_List_Gizmo == null)	{ _meshGroupTF_List_Gizmo = new List<apTransform_MeshGroup>(); }

			_meshGroupTF_List_Sub.Clear();
			_meshGroupTF_List_All.Clear();

			_meshGroupTF_GizmoMain = null;
			_meshGroupTF_List_Gizmo.Clear();

			if (_bone_List_Sub == null)		{ _bone_List_Sub = new List<apBone>(); }
			if (_bone_List_All == null)		{ _bone_List_All = new List<apBone>(); }
			if (_bone_List_Gizmo == null)	{ _bone_List_Gizmo = new List<apBone>(); }

			_bone_List_Sub.Clear();
			_bone_List_All.Clear();

			_bone_GizmoMain = null;
			_bone_List_Gizmo.Clear();

			_isChildMeshGroupObjectSelected_Gizmo_TF = false;
			_isChildMeshGroupObjectSelected_Gizmo_Bone = false;


			_nMeshTF = 0;
			_nMeshGroupTF = 0;
			_nBone = 0;

			//애니메이션 타임라인 레이어도 여기에서 선택된다.
			_timeline = null;//모든 타임라인 레이어는 동일한 타임라인의 아래에 들어가야 한다.
			_timelineLayer = null;
			_timelineLayer_Gizmo = null;
			if(_timelineLayers_Sub == null) { _timelineLayers_Sub = new List<apAnimTimelineLayer>(); }
			if(_timelineLayers_All == null) { _timelineLayers_All = new List<apAnimTimelineLayer>(); }
			_timelineLayers_Sub.Clear();
			_timelineLayers_All.Clear();
			_nTimelineLayers = 0;


			_workKeyframe = null;
			if (_workKeyframes_Sub == null) { _workKeyframes_Sub = new List<apAnimKeyframe>(); }
			if (_workKeyframes_All == null) { _workKeyframes_All = new List<apAnimKeyframe>(); }
			_workKeyframes_Sub.Clear();
			_workKeyframes_All.Clear();
			_workAnimFrameIndex = -1;

			_nWorkKeyframes = 0;

			_sync = new SyncResult[NUM_SYNC];
			for (int i = 0; i < NUM_SYNC; i++)
			{
				_sync[i] = new SyncResult();
			}


			//계산용 변수
			if(_prevTimelineLayers == null) { _prevTimelineLayers = new List<apAnimTimelineLayer>(); }
			_prevTimelineLayers.Clear();
		}

		public void Clear()
		{
			_meshTF = null;
			_meshGroupTF = null;
			_bone = null;
			_controlParam_Anim = null;

			_meshTF_List_Sub.Clear();
			_meshTF_List_All.Clear();

			_meshTF_GizmoMain = null;
			_meshTF_List_Gizmo.Clear();

			_meshGroupTF_List_Sub.Clear();
			_meshGroupTF_List_All.Clear();

			_meshGroupTF_GizmoMain = null;
			_meshGroupTF_List_Gizmo.Clear();

			_bone_List_Sub.Clear();
			_bone_List_All.Clear();

			_bone_GizmoMain = null;
			_bone_List_Gizmo.Clear();

			_isChildMeshGroupObjectSelected_Gizmo_TF = false;
			_isChildMeshGroupObjectSelected_Gizmo_Bone = false;

			_nMeshTF = 0;
			_nMeshGroupTF = 0;
			_nBone = 0;

			_timeline = null;
			_timelineLayer = null;
			_timelineLayer_Gizmo = null;
			_timelineLayers_Sub.Clear();
			_timelineLayers_All.Clear();

			_nTimelineLayers = 0;

			_workKeyframe = null;
			_workKeyframes_Sub.Clear();
			_workKeyframes_All.Clear();
			_workAnimFrameIndex = -1;

			_nWorkKeyframes = 0;
		}

		public void ClearTF()
		{
			_meshTF = null;
			_meshGroupTF = null;

			_meshTF_List_Sub.Clear();
			_meshTF_List_All.Clear();

			_meshTF_GizmoMain = null;
			_meshTF_List_Gizmo.Clear();

			_meshGroupTF_List_Sub.Clear();
			_meshGroupTF_List_All.Clear();

			_meshGroupTF_GizmoMain = null;
			_meshGroupTF_List_Gizmo.Clear();

			_isChildMeshGroupObjectSelected_Gizmo_TF = false;

			_nMeshTF = 0;
			_nMeshGroupTF = 0;
		}

		public void ClearBone()
		{
			_bone = null;
			_bone_List_Sub.Clear();
			_bone_List_All.Clear();

			_bone_GizmoMain = null;
			_bone_List_Gizmo.Clear();
			
			_isChildMeshGroupObjectSelected_Gizmo_Bone = false;

			_nBone = 0;
		}


		public void ClearControlParam()
		{
			_controlParam_Anim = null;
		}

		public void ClearTimelineLayers()
		{
			_timeline = null;
			_timelineLayer = null;
			_timelineLayer_Gizmo = null;
			_timelineLayers_Sub.Clear();
			_timelineLayers_All.Clear();

			_nTimelineLayers = 0;

			//타임라인 초기화시 WorkKeyframe들도 자동으로 초기화
			ClearWorkKeyframes();
		}


		public void ClearWorkKeyframes()
		{
			_workKeyframe = null;
			_workKeyframes_Sub.Clear();
			_workKeyframes_All.Clear();
			_nWorkKeyframes = 0;
			_workAnimFrameIndex = -1;
		}

		// Function - Select : 선택과 관련된 함수
		//-------------------------------------------------------
		public apSelection.SUB_SELECTED_RESULT IsSelected(apTransform_Mesh meshTF)
		{
			if (meshTF == null)		{ return apSelection.SUB_SELECTED_RESULT.None; }	
			//if (_meshTF == null)	{ return apSelection.SUB_SELECTED_RESULT.None; }	//선택 안됨
			if (_meshTF != null && _meshTF == meshTF)	{ return apSelection.SUB_SELECTED_RESULT.Main; }	//메인으로 선택된 상태
			if (_meshTF_List_Sub.Count > 0)
			{
				//추가로 선택 되었거나 아님
				return _meshTF_List_Sub.Contains(meshTF) ? apSelection.SUB_SELECTED_RESULT.Added : apSelection.SUB_SELECTED_RESULT.None;
			}
			return apSelection.SUB_SELECTED_RESULT.None;
		}

		public apSelection.SUB_SELECTED_RESULT IsSelected(apTransform_MeshGroup meshGroupTF)
		{
			if (meshGroupTF == null)			{ return apSelection.SUB_SELECTED_RESULT.None; }
			//if (_meshGroupTF == null)			{ return apSelection.SUB_SELECTED_RESULT.None; }	//선택 안됨
			if (_meshGroupTF != null && _meshGroupTF == meshGroupTF)	{ return apSelection.SUB_SELECTED_RESULT.Main; }	//메인으로 선택된 상태
			if (_meshGroupTF_List_Sub.Count > 0)
			{
				//추가로 선택 되었거나 아님
				return _meshGroupTF_List_Sub.Contains(meshGroupTF) ? apSelection.SUB_SELECTED_RESULT.Added : apSelection.SUB_SELECTED_RESULT.None;
			}
			return apSelection.SUB_SELECTED_RESULT.None;
		}

		public apSelection.SUB_SELECTED_RESULT IsSelected(apBone bone)
		{	
			if (bone == null)	{ return apSelection.SUB_SELECTED_RESULT.None; }
			//if (_bone == null)	{ return apSelection.SUB_SELECTED_RESULT.None; }	//선택 안됨
			if (_bone != null && _bone == bone)	{ return apSelection.SUB_SELECTED_RESULT.Main; }    //메인으로 선택된 상태
			if (_bone_List_Sub.Count > 0)
			{
				//추가로 선택 되었거나 아님
				return _bone_List_Sub.Contains(bone) ? apSelection.SUB_SELECTED_RESULT.Added : apSelection.SUB_SELECTED_RESULT.None;
			}

			return apSelection.SUB_SELECTED_RESULT.None;
		}

		public apSelection.SUB_SELECTED_RESULT IsSelected(apControlParam controlParam_Anim)
		{
			if (controlParam_Anim == null)	{ return apSelection.SUB_SELECTED_RESULT.None; }
			if (_controlParam_Anim == null)	{ return apSelection.SUB_SELECTED_RESULT.None; }	//선택 안됨
			if (_controlParam_Anim == controlParam_Anim)	{ return apSelection.SUB_SELECTED_RESULT.Main; }    //메인으로 선택된 상태
			//ControlParamAnim은 메인밖에 없다.

			return apSelection.SUB_SELECTED_RESULT.None;
		}


		/// <summary>
		/// 요청된 객체가 포함(=선택)되었는가. 타입 상관없이 + Main/Sub 상관없이 알고자 할 때
		/// </summary>
		/// <param name="targetObj"></param>
		/// <returns></returns>
		public bool IsContain(object targetObj)
		{
			if (targetObj == null)
			{
				return false;
			}

			if (targetObj is apTransform_Mesh)			{ return _meshTF_List_All.Contains((targetObj as apTransform_Mesh)); }
			if (targetObj is apTransform_MeshGroup)		{ return _meshGroupTF_List_All.Contains((targetObj as apTransform_MeshGroup)); }
			if (targetObj is apBone)					{ return _bone_List_All.Contains((targetObj as apBone)); }
			if (targetObj is apControlParam)			{ return _controlParam_Anim == targetObj; }

			return false;
		}


		//Select 함수들
		/// <summary>
		/// Mesh Transform을 선택합니다. 변경이 되었다면 true 리턴.
		/// </summary>
		public bool SelectMeshTF(apTransform_Mesh targetMeshTF, apSelection.MULTI_SELECT multiSelect)
		{
			apTransform_Mesh prevMeshTF = _meshTF;
			
			if (multiSelect == apSelection.MULTI_SELECT.Main || //단순 선택이거나
				(_meshTF == null && _meshGroupTF == null) ||	//기존에 선택된게 없거나 (둘다)
				targetMeshTF == null							//그냥 null 입력(선택 취소)이 들어온 경우
				)
			{
				_meshTF = targetMeshTF;

				_meshTF_List_All.Clear();
				_meshTF_List_Sub.Clear();

				if(targetMeshTF != null)
				{
					_meshTF_List_All.Add(targetMeshTF);//All에도 추가
				}

				if (targetMeshTF != null)
				{
					//메인인 경우에 한해서 > MeshGroupTF를 Null로 강제한다.
					_meshGroupTF = null;
					_meshGroupTF_List_All.Clear();
					_meshGroupTF_List_Sub.Clear();
				}

			}
			else if (multiSelect == apSelection.MULTI_SELECT.AddOrSubtract)
			{
				//일단 추가하고 동기화를 하자
				if (targetMeshTF != null)
				{
					if (!_meshTF_List_All.Contains(targetMeshTF))
					{
						//기존 리스트에 추가를 하자
						_meshTF_List_All.Add(targetMeshTF);
					}
					else
					{
						//기존 리스트에서 삭제한다.
						_meshTF_List_All.Remove(targetMeshTF);

						//선택된 거였다면, 리스트에서 다음거를 참조하자.
						if (_meshTF == targetMeshTF)
						{
							_meshTF = null;
							if (_meshTF_List_All.Count > 0)
							{
								_meshTF = _meshTF_List_All[0];
								_meshGroupTF = null;
							}
							else if (_meshGroupTF_List_All.Count > 0)
							{
								//MeshGroupTF로 넘어간다.
								_meshGroupTF = _meshGroupTF_List_All[0];
							}
						}
					}
				}

				//All > Sub 동기화
				//MeshGroupTF도 같이 갱신한다.
				_meshTF_List_Sub.Clear();
				_meshGroupTF_List_Sub.Clear();

				//서로 Main은 베타적이어야 한다.
				if(_meshTF != null)
				{
					_meshGroupTF = null;
				}
				else if(_meshGroupTF != null)
				{
					_meshTF = null;
				}

				apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;
				for (int i = 0; i < _meshTF_List_All.Count; i++)
				{
					curMeshTF = _meshTF_List_All[i];
					if (curMeshTF != _meshTF)
					{
						_meshTF_List_Sub.Add(curMeshTF);
					}
				}

				for (int i = 0; i < _meshGroupTF_List_All.Count; i++)
				{
					curMeshGroupTF = _meshGroupTF_List_All[i];
					if (curMeshGroupTF != _meshGroupTF)
					{
						_meshGroupTF_List_Sub.Add(curMeshGroupTF);
					}
				}

			}

			//개수 갱신
			_nMeshTF = _meshTF_List_All.Count;
			_nMeshGroupTF = _meshGroupTF_List_All.Count;

			//기즈모 리스트를 갱신한다.
			RefreshGizmoList(true, false);

			return (prevMeshTF != _meshTF);
		}



		/// <summary>
		/// MeshGroup Transform을 선택합니다. 변경이 되었다면 true 리턴
		/// </summary>
		public bool SelectMeshGroupTF(apTransform_MeshGroup targetMeshGroupTF, apSelection.MULTI_SELECT multiSelect)
		{
			apTransform_MeshGroup prevMeshGroupTF = _meshGroupTF;

			if (multiSelect == apSelection.MULTI_SELECT.Main || //단순 선택이거나
				(_meshGroupTF == null && _meshTF == null) ||	//기존에 선택된게 없거나(둘다)
				targetMeshGroupTF == null						//그냥 null 입력(선택 취소)이 들어온 경우
				)
			{
				_meshGroupTF = targetMeshGroupTF;
				_meshGroupTF_List_All.Clear();
				_meshGroupTF_List_Sub.Clear();
				if (targetMeshGroupTF != null)
				{
					_meshGroupTF_List_All.Add(targetMeshGroupTF);//All에도 추가
				}

				if (targetMeshGroupTF != null)
				{
					//메인 설정시 MeshGroupTF는 자동으로 선택 취소
					_meshTF = null;
					_meshTF_List_All.Clear();
					_meshTF_List_Sub.Clear();
				}

			}
			else if (multiSelect == apSelection.MULTI_SELECT.AddOrSubtract)
			{
				//일단 추가하고 동기화를 하자
				if (!_meshGroupTF_List_All.Contains(targetMeshGroupTF))
				{
					//기존 리스트에 추가를 하자
					_meshGroupTF_List_All.Add(targetMeshGroupTF);
				}
				else
				{
					//기존 리스트에서 삭제한다.
					_meshGroupTF_List_All.Remove(targetMeshGroupTF);

					//선택된 거였다면, 리스트에서 다음거를 참조하자.
					if (_meshGroupTF == targetMeshGroupTF)
					{
						_meshGroupTF = null;
						if (_meshGroupTF_List_All.Count > 0)
						{
							_meshGroupTF = _meshGroupTF_List_All[0];
							_meshTF = null;
						}
						else if (_meshTF_List_All.Count > 0)
						{
							//MeshTF로 넘어간다.
							_meshTF = _meshTF_List_All[0];
						}
					}
				}

				//All > Sub 동기화
				_meshTF_List_Sub.Clear();
				_meshGroupTF_List_Sub.Clear();

				//서로 Main은 베타적이어야 한다.
				if(_meshTF != null)
				{
					_meshGroupTF = null;
				}
				else if(_meshGroupTF != null)
				{
					_meshTF = null;
				}

				apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;
				for (int i = 0; i < _meshTF_List_All.Count; i++)
				{
					curMeshTF = _meshTF_List_All[i];
					if (curMeshTF != _meshTF)
					{
						_meshTF_List_Sub.Add(curMeshTF);
					}
				}

				for (int i = 0; i < _meshGroupTF_List_All.Count; i++)
				{
					curMeshGroupTF = _meshGroupTF_List_All[i];
					if (curMeshGroupTF != _meshGroupTF)
					{
						_meshGroupTF_List_Sub.Add(curMeshGroupTF);
					}
				}

			}

			//개수 갱신
			_nMeshTF = _meshTF_List_All.Count;
			_nMeshGroupTF = _meshGroupTF_List_All.Count;

			//기즈모 리스트를 갱신한다.
			RefreshGizmoList(true, false);

			return (prevMeshGroupTF != _meshGroupTF);
		}



		/// <summary>
		/// 본을 선택한다.
		/// </summary>
		/// <param name="targetBone"></param>
		/// <param name="multiSelect"></param>
		/// <returns></returns>
		public bool SelectBone(apBone targetBone, apSelection.MULTI_SELECT multiSelect)
		{
			apBone prevBone = _bone;

			if (multiSelect == apSelection.MULTI_SELECT.Main ||
				_bone == null ||
				targetBone == null)
			{
				//단일 선택을 하자
				_bone = targetBone;
				_bone_List_All.Clear();
				_bone_List_Sub.Clear();
				if (targetBone != null)
				{
					_bone_List_All.Add(targetBone);
				}
			}
			else if (multiSelect == apSelection.MULTI_SELECT.AddOrSubtract)
			{
				//추가를 하고 동기화를 하자
				if (!_bone_List_All.Contains(targetBone))
				{
					//기존 리스트에 추가
					_bone_List_All.Add(targetBone);
				}
				else
				{
					//기존 리스트에서 삭제 후 다음꺼 선택
					_bone_List_All.Remove(targetBone);

					if (_bone == targetBone)
					{
						_bone = null;
						if (_bone_List_All.Count > 0)
						{
							_bone = _bone_List_All[0];
						}
					}
				}

				//All > Sub 동기화
				apBone curBone = null;
				_bone_List_Sub.Clear();
				for (int i = 0; i < _bone_List_All.Count; i++)
				{
					curBone = _bone_List_All[i];
					if (curBone != _bone)
					{
						_bone_List_Sub.Add(curBone);
					}
				}
			}

			//개수 갱신
			_nBone = _bone_List_All.Count;

			//기즈모 리스트를 갱신한다.
			RefreshGizmoList(false, true);

			return prevBone != _bone;
		}

		//애니메이션인 경우) 컨트롤 파라미터 선택
		//다중 선택은 지원되지 않는다.
		public bool SelectControlParamForAnim(apControlParam controlParam)
		{
			apControlParam prevControlParam = _controlParam_Anim;

			_controlParam_Anim = controlParam;

			return prevControlParam != _controlParam_Anim;
		}


		/// <summary>
		/// MeshTF, MeshGroupTF, Bone을 선택한다. 메인인 경우 다른 타입이 배제된다. 
		/// (즉, 리깅에서는 이 함수를 사용하면 안된다.)
		/// </summary>
		/// <param name="targetMeshTF"></param>
		/// <param name="targetMeshGroupTF"></param>
		/// <param name="targetBone"></param>
		/// <param name="multiSelect"></param>
		/// <returns></returns>
		public bool Select(	apTransform_Mesh targetMeshTF, 
							apTransform_MeshGroup targetMeshGroupTF, 
							apBone targetBone, 
							apSelection.MULTI_SELECT multiSelect,
							apSelection.TF_BONE_SELECT selectTFBoneType)
		{
			bool isChanged = false;
			if (targetMeshTF != null)
			{
				isChanged = SelectMeshTF(targetMeshTF, multiSelect);
				
				if (multiSelect == apSelection.MULTI_SELECT.Main)
				{
					isChanged |= SelectMeshGroupTF(null, multiSelect);
				}

				if(multiSelect == apSelection.MULTI_SELECT.Main
					|| selectTFBoneType == apSelection.TF_BONE_SELECT.Exclusive)
				{
					//TF 선택시 Bone은 선택이 안될 수도 있다. 
					isChanged |= SelectBone(null, multiSelect);
				}
				
			}
			else if (targetMeshGroupTF != null)
			{
				isChanged = SelectMeshGroupTF(targetMeshGroupTF, multiSelect);
				if (multiSelect == apSelection.MULTI_SELECT.Main)
				{
					isChanged |= SelectMeshTF(null, multiSelect);
				}

				if(multiSelect == apSelection.MULTI_SELECT.Main
					|| selectTFBoneType == apSelection.TF_BONE_SELECT.Exclusive)
				{
					//TF 선택시 Bone은 선택이 안될 수도 있다. 
					isChanged |= SelectBone(null, multiSelect);
				}
			}
			else if (targetBone != null)
			{
				isChanged = SelectBone(targetBone, multiSelect);
				
				if(multiSelect == apSelection.MULTI_SELECT.Main
					|| selectTFBoneType == apSelection.TF_BONE_SELECT.Exclusive)
				{
					//Bone 선택시 TF는 선택이 안될 수도 있다. 
					isChanged |= SelectMeshTF(null, multiSelect);
					isChanged |= SelectMeshGroupTF(null, multiSelect);
				}
			}
			else
			{
				isChanged = SelectMeshTF(null, multiSelect);
				isChanged |= SelectMeshGroupTF(null, multiSelect);
				isChanged |= SelectBone(null, multiSelect);
			}

			

			//개수 갱신
			_nMeshTF = _meshTF_List_All.Count;
			_nMeshGroupTF = _meshGroupTF_List_All.Count;
			_nBone = _bone_List_All.Count;

			//기즈모 리스트를 갱신한다.
			RefreshGizmoList(true, true);

			return isChanged;
		}



		//20.6.23 : 기즈모용 리스트 갱신
		//기즈모에서는 부모-자식관계에 의해서 모든 객체가 아닌 부모만 편집할 필요가 있을때가 있다.
		private void RefreshGizmoList(bool isTF, bool isBone)
		{
			if(isTF)
			{
				_meshTF_GizmoMain = null;
				_meshTF_List_Gizmo.Clear();

				_meshGroupTF_GizmoMain = null;
				_meshGroupTF_List_Gizmo.Clear();
				
				apTransform_Mesh curMeshTF = null;
				apTransform_MeshGroup curMeshGroupTF = null;
				
				//한개만 있다면 복잡한 로직을 갱신할 필요가 없다.
				if(_nMeshTF > 0 && _nMeshGroupTF == 0)
				{
					//메시 TF만 여러개 선택된 경우
					//그냥 복사한다.
					for (int i = 0; i < _nMeshTF; i++)
					{
						curMeshTF = _meshTF_List_All[i];
						_meshTF_List_Gizmo.Add(curMeshTF);

						if(curMeshTF == _meshTF)
						{
							_meshTF_GizmoMain = curMeshTF;//메인 그대로 할당
						}
					}
				}
				else if(_nMeshTF == 0 && _nMeshGroupTF == 1)
				{
					curMeshGroupTF = _meshGroupTF_List_All[0];
					_meshGroupTF_List_Gizmo.Add(curMeshGroupTF);
					if(curMeshGroupTF == _meshGroupTF)
					{
						_meshGroupTF_GizmoMain = curMeshGroupTF;//메인 그대로 할당
					}
					
				}
				else if(_nMeshGroupTF > 1 || _nMeshTF + _nMeshGroupTF > 1)
				{
					//MeshGroupTF가 2개 이상인 경우 또는 
					//하나씩 돌면서, "자신의 재귀적인 부모 MeshGroupTF"가 선택된 상태라면,
					//Gizmo에는 포함시키지 않는다.
					
					if (_nMeshTF > 0)
					{
						for (int i = 0; i < _nMeshTF; i++)
						{
							curMeshTF = _meshTF_List_All[i];
							
							if(!IsParentRenderUnitInSelectedList(curMeshTF._linkedRenderUnit))
							{
								//이 MeshTF의 부모는 선택되지 않았다.
								_meshTF_List_Gizmo.Add(curMeshTF);

								if(curMeshTF == _meshTF)
								{	
									_meshTF_GizmoMain = curMeshTF;//메인이면 할당
								}
							}
						}
					}

					for (int i = 0; i < _nMeshGroupTF; i++)
					{
						curMeshGroupTF = _meshGroupTF_List_All[i];
						if(!IsParentRenderUnitInSelectedList(curMeshGroupTF._linkedRenderUnit))
						{
							//이 MeshGroupTF의 부모는 선택되지 않았다.
							_meshGroupTF_List_Gizmo.Add(curMeshGroupTF);

							if(curMeshGroupTF == _meshGroupTF)
							{
								_meshGroupTF_GizmoMain = curMeshGroupTF;//메인이면 할당
							}
						}
					}

					//메인을 바꿀 필요가 있는가
					//만약, MeshTF나 MeshGroupTF가 있는데, 기즈모용 메인이 할당되지 않았다면
					//별도의 메인을 찾아야 한다.
					if((_meshTF != null || _meshGroupTF != null) //메인은 있는데
						&& (_meshTF_GizmoMain == null && _meshGroupTF_GizmoMain == null)//기즈모 메인은 없는 경우
						)
					{
						//기즈모 메인으로 삼을 객체를 찾자.
						//이 경우는 MeshGroupTF만 해당한다.
						if(_meshTF != null)
						{
							_meshGroupTF_GizmoMain = GetParentMeshGroupTF_InGizmoList(_meshTF._linkedRenderUnit);
						}
						else if(_meshGroupTF != null)
						{
							_meshGroupTF_GizmoMain = GetParentMeshGroupTF_InGizmoList(_meshGroupTF._linkedRenderUnit);
						}

						//만약 제대로 찾지 못했다면 그냥 메인을 그대로 간다. (에러 상황)
						if(_meshGroupTF_GizmoMain == null)
						{
							if (_meshTF != null)
							{
								_meshTF_GizmoMain = _meshTF;
								_meshGroupTF_GizmoMain = null;
							}
							else if (_meshGroupTF != null)
							{
								_meshTF_GizmoMain = null;
								_meshGroupTF_GizmoMain = _meshGroupTF;
							}
						}
					}
				}

				//선택된 TF 중에 하위 메시 그룹에 속한 객체가 있는지 확인해야한다.
				_isChildMeshGroupObjectSelected_Gizmo_TF = false;
				if(_meshGroupTF_List_Gizmo.Count > 0)
				{
					//서브 메시 그룹을 선택했다면 무조건 TRUE
					_isChildMeshGroupObjectSelected_Gizmo_TF = true;
				}
				else if(_meshTF_List_Gizmo.Count > 0)
				{
					//하나씩 체크해야한다.
					for (int i = 0; i < _meshTF_List_Gizmo.Count; i++)
					{
						curMeshTF = _meshTF_List_Gizmo[i];
						if(curMeshTF._linkedRenderUnit == null)
						{
							continue;
						}
						if(curMeshTF._linkedRenderUnit._meshGroup == null)
						{
							continue;
						}

						if(curMeshTF._linkedRenderUnit._meshGroup._parentMeshGroup != null ||
							curMeshTF._linkedRenderUnit._meshGroup._parentMeshGroupID >= 0)
						{
							//부모 메시 그룹이 따로 존재한다. > 하위 MeshTF가 선택된 상태
							_isChildMeshGroupObjectSelected_Gizmo_TF = true;
							break;
						}
					}
				}

				
				
			}

			if(isBone)
			{
				_bone_GizmoMain = null;
				_bone_List_Gizmo.Clear();
				
				apBone curBone = null;

				if(_nBone == 1)
				{
					//한개만 선택되어 있다.
					curBone = _bone_List_All[0];
					_bone_List_Gizmo.Add(_bone_List_All[0]);

					if(curBone == _bone)
					{
						_bone_GizmoMain = curBone;//메인으로 할당
					}
				}
				else if(_nBone > 1)
				{
					//여러개가 선택되어 있다면,
					//부모가 선택되어 있으면 안된다.

					for (int i = 0; i < _nBone; i++)
					{
						curBone = _bone_List_All[i];

						if(!IsParentBoneInSelectedList(curBone))
						{
							//부모 본이 선택되지 않았다. > 기즈모에 넣자
							_bone_List_Gizmo.Add(curBone);

							if(curBone == _bone)
							{
								_bone_GizmoMain = curBone;//메인으로 할당
							}
						}
					}


					//메인 본이 기즈모 리스트에 포함되지 않았다면 기즈모용 메인은 따로 찾아야 한다.
					if(_bone != null && _bone_GizmoMain == null)
					{
						_bone_GizmoMain = GetParentBone_InGizmoList(_bone);

						//만약 못찾았다면, 메인을 그대로 유지한다. (에러 상황)
						if(_bone_GizmoMain == null)
						{
							_bone_GizmoMain = _bone;
						}
					}
				}

				//선택된 Bone 중에 하위 메시 그룹에 속한 객체가 있는지 확인해야한다.
				_isChildMeshGroupObjectSelected_Gizmo_Bone = false;
				if(_bone_List_Gizmo.Count > 0)
				{
					for (int i = 0; i < _bone_List_Gizmo.Count; i++)
					{
						curBone = _bone_List_Gizmo[i];

						if(curBone._meshGroup == null)
						{
							continue;
						}

						if(curBone._meshGroup._parentMeshGroup != null ||
							curBone._meshGroup._parentMeshGroupID >= 0)
						{
							//부모 메시 그룹이 따로 존재한다. > 하위 Bone이  선택된 상태
							_isChildMeshGroupObjectSelected_Gizmo_Bone = true;
							break;
						}
					}
				}
			}
		}

		//서브 함수 : 선택된 렌더 유닛의 부모가 MeshGroupTF_All 리스트에 포함되어 있는가.
		private bool IsParentRenderUnitInSelectedList(apRenderUnit renderUnit)
		{
			if(renderUnit == null)
			{
				return false;
			}

			apRenderUnit curParentRenderUnit = renderUnit._parentRenderUnit;
			if(curParentRenderUnit == null)
			{
				return false;
			}
			if(curParentRenderUnit._meshGroupTransform == null)
			{
				return false;
			}

			while (true)
			{
				if(curParentRenderUnit == null)
				{
					//부모가 없다.. (?!)
					break;
				}
				if (curParentRenderUnit._unitType != apRenderUnit.UNIT_TYPE.GroupNode ||
					curParentRenderUnit._meshGroupTransform == null)
				{
					//현재의 부모 렌더 유닛에 MeshGroupTransform이 없넹?
					break;
				}

				if(_meshGroupTF_List_All.Contains(curParentRenderUnit._meshGroupTransform))
				{
					//True! 선택된 MeshGroupTF가 재귀적인 부모에 존재한다.
					return true;
				}

				//다음 부모를 찾자
				curParentRenderUnit = curParentRenderUnit._parentRenderUnit;
			}

			return false;
		}


		//서브 함수 : 선택된 본의 부모가 Bone_All 리스트에 포함되어 있는가
		private bool IsParentBoneInSelectedList(apBone bone)
		{
			if(bone == null)
			{
				return false;
			}
			if(bone._parentBone == null)
			{
				return false;
			}

			apBone curParentBone = bone._parentBone;

			while(true)
			{
				if(curParentBone == null)
				{
					break;
				}

				if(_bone_List_All.Contains(curParentBone))
				{
					//재귀적인 부모가 이미 선택된 상태이다.
					return true;
				}

				//다음 부모를 찾자
				curParentBone = curParentBone._parentBone;
			}

			return false;
		}


		//해당 렌더 유닛(MeshTF/MeshGroupTF)의 부모 중에서 GizmoList에 포함된 것을 찾자
		//부모니까 무조건 MeshGroupTF
		private apTransform_MeshGroup GetParentMeshGroupTF_InGizmoList(apRenderUnit renderUnit)
		{
			if(renderUnit == null)
			{
				return null;
			}

			apRenderUnit curParentRenderUnit = renderUnit._parentRenderUnit;
			if(curParentRenderUnit == null)
			{
				return null;
			}
			if(curParentRenderUnit._meshGroupTransform == null)
			{
				return null;
			}

			while (true)
			{
				if(curParentRenderUnit == null)
				{
					//부모가 없다.. (?!)
					break;
				}
				if (curParentRenderUnit._unitType != apRenderUnit.UNIT_TYPE.GroupNode ||
					curParentRenderUnit._meshGroupTransform == null)
				{
					//현재의 부모 렌더 유닛에 MeshGroupTransform이 없넹?
					break;
				}

				if(_meshGroupTF_List_Gizmo.Contains(curParentRenderUnit._meshGroupTransform))
				{
					//선택된 부모 MeshGroupTF가 Gizmo리스트에 포함되어 있다.
					return curParentRenderUnit._meshGroupTransform;
				}

				//다음 부모를 찾자
				curParentRenderUnit = curParentRenderUnit._parentRenderUnit;
			}

			return null;
		}

		private apBone GetParentBone_InGizmoList(apBone bone)
		{
			if(bone == null)
			{
				return null;
			}
			if(bone._parentBone == null)
			{
				return null;
			}

			apBone curParentBone = bone._parentBone;

			while(true)
			{
				if(curParentBone == null)
				{
					break;
				}

				if(_bone_List_Gizmo.Contains(curParentBone))
				{
					//Gizmo리스트에 포함된 ParentBone을 찾았다.
					return curParentBone;
				}

				//다음 부모를 찾자
				curParentBone = curParentBone._parentBone;
			}

			return null;
		}









		//애니메이션 작업시
		//1. TimelineLayer를 선택하면 > 거기에 맞게 오브젝트를 선택한다.
		//2. 반대로, 선택된 오브젝트를 선택하면 > TimelineLayer를 선택한다.
		//둘다 선택가능해야한다.
		
		/// <summary>
		/// 타임라인 레이어를 선택하면, 거기에 맞게 오브젝트들이 선택된다. (애니메이션 전용)
		/// 선택된 타임라인 레이어가 변동된 경우 true로 리턴. 없어도 true 리턴
		/// </summary>
		public bool SelectAnimTimelineLayer(apAnimTimelineLayer targetTimelineLayer, apSelection.MULTI_SELECT multiSelect)
		{
			//기존과 비교하기 위해 이전 결과를 리스트로 저장
			_prevTimelineLayers.Clear();
			for (int i = 0; i < _timelineLayers_All.Count; i++)
			{
				_prevTimelineLayers.Add(_timelineLayers_All[i]);
			}

			//타임라인 레이어가 다르다면 전부 초기화한다. (Add/Subtract도 마찬가지)
			if(targetTimelineLayer != null && _timeline != null)
			{
				if(targetTimelineLayer._parentTimeline != _timeline)
				{
					//다른 타임라인 레이어를 선택했다. > 타임라인 레이어 리셋
					ClearTimelineLayers();
				}
			}

			if(multiSelect == apSelection.MULTI_SELECT.Main
				|| _timelineLayer == null
				|| targetTimelineLayer == null)
			{
				//메인으로 선택한다.
				_timeline = targetTimelineLayer != null ? targetTimelineLayer._parentTimeline : null;
				_timelineLayer = targetTimelineLayer;
				_timelineLayers_Sub.Clear();
				_timelineLayers_All.Clear();
				
				if(_timelineLayer != null)
				{
					_timelineLayers_All.Add(_timelineLayer);
				}
			}
			else
			{
				//추가 혹은 삭제
				if(_timelineLayers_All.Contains(targetTimelineLayer))
				{
					//이미 있는 경우 > 삭제
					_timelineLayers_All.Remove(targetTimelineLayer);
					_timelineLayers_Sub.Remove(targetTimelineLayer);
					if(_timelineLayer == targetTimelineLayer)
					{
						//메인에서 해제되는 경우
						//Sub의 맨 앞을 Main으로 선택한다.
						_timelineLayer = null;

						if(_timelineLayers_Sub.Count > 0)
						{
							_timelineLayer = _timelineLayers_Sub[0];
							_timelineLayers_Sub.Remove(_timelineLayer);
						}
					}
				}
				else
				{
					//없다면 > 추가
					if(_timelineLayers_All.Count == 0 || _timelineLayer == null)
					{
						//메인으로 등록
						_timelineLayer = targetTimelineLayer;
					}
					else
					{
						//서브로 등록
						_timelineLayers_Sub.Add(targetTimelineLayer);
					}

					_timelineLayers_All.Add(targetTimelineLayer);
				}
			}

			_nTimelineLayers = _timelineLayers_All.Count;

			//if (isAutoSelectObjects)
			//{
			//	//이제 선택된 TimelineLayer들을 바탕으로 오브젝트들을 선택한다.
			//	AutoSelectObjectsFromTimelineLayers();
			//}
			
			

			bool isChanged = false;
			if(_timelineLayer != targetTimelineLayer)
			{	
				isChanged = true;//메인이 바뀌었다.
			}
			else if(_prevTimelineLayers.Count != _nTimelineLayers)
			{
				isChanged = true;//개수가 바뀌었다.
			}
			else
			{
				for (int i = 0; i < _timelineLayers_All.Count; i++)
				{
					if(!_prevTimelineLayers.Contains(_timelineLayers_All[i]))
					{
						isChanged = true;//리스트 내용이 다르다.
						break;
					}
				}
			}

			if(isChanged && _nTimelineLayers > 0)
			{
				//선택된 타임라인 레이어가 있다면 항상 동기화를 하자
				AutoSelectObjectsFromTimelineLayers();

				//기즈모 메인에 대한 타임라인 레이어도 찾자
				AutoSelectGizmoTimelineLayer();
			}
			
			////변경이 되었거나 강제라면
			//if(isChanged || isAutoSelectWorkKeyframe)
			//{
			//	//WorkKeyframe도 다시 선택하자
			//	AutoSelectWorkKeyframes(animClip, isPlaying);
			//}

			return isChanged;
		}



		/// <summary>
		/// 타임라인 레이어를 선택한다.
		/// 추가/삭제 없이 리스트로 입력된 것으로 아예 대체한다.
		/// 가능한 Main 오브젝트를 유지하는 것이 중요.
		/// 변경 내역이 있거나 선택된게 없다면 true 리턴
		/// </summary>
		/// <param name="targetTimelineLayers"></param>
		/// <param name="multiSelect"></param>
		/// <returns></returns>
		public bool SelectAnimTimelineLayers(List<apAnimTimelineLayer> targetTimelineLayers, apAnimTimeline commonTimeline)
		{
			if(targetTimelineLayers == null || targetTimelineLayers.Count == 0 || commonTimeline == null)
			{
				//입력값이 없다면..
				//또는 공통의 타임라인이 없다면
				//선택 불가
				return SelectAnimTimelineLayer(null, apSelection.MULTI_SELECT.Main);
			}


			//기존과 비교하기 위해 이전 결과를 리스트로 저장
			_prevTimelineLayers.Clear();
			for (int i = 0; i < _timelineLayers_All.Count; i++)
			{
				_prevTimelineLayers.Add(_timelineLayers_All[i]);
			}

			apAnimTimelineLayer prevMainTimelineLayer = _timelineLayer;
			
			if(prevMainTimelineLayer != null && !targetTimelineLayers.Contains(prevMainTimelineLayer))
			{
				//기존의 Main 타임라인 레이어를 유지할 수 있는 상태가 아니라면
				prevMainTimelineLayer = null;
			}

			//이 함수에서는 무조건 초기화
			ClearTimelineLayers();

			_timeline = commonTimeline;

			apAnimTimelineLayer srcTimelineLayer = null;
			for (int i = 0; i < targetTimelineLayers.Count; i++)
			{
				srcTimelineLayer = targetTimelineLayers[i];
				_timelineLayers_All.Add(srcTimelineLayer);

				if(prevMainTimelineLayer != null
					&& srcTimelineLayer == prevMainTimelineLayer)
				{
					//기존의 메인이라면
					_timelineLayer = srcTimelineLayer;
				}
			}

			//메인 레이어를 못찾았다면 All의 첫번째를 메인으로 선택하자
			if(_timelineLayer == null && _timelineLayers_All.Count > 0)
			{
				_timelineLayer = _timelineLayers_All[0];
			}

			//메인을 제외한 나머지 레이어를 Sub로 넣자.
			apAnimTimelineLayer curLayer = null;
			for (int i = 0; i < _timelineLayers_All.Count; i++)
			{
				curLayer = _timelineLayers_All[i];
				if(curLayer != _timelineLayer)
				{
					_timelineLayers_Sub.Add(curLayer);
				}
			}

			_nTimelineLayers = _timelineLayers_All.Count;

			bool isChanged = false;
			if(_timelineLayer != prevMainTimelineLayer)
			{	
				isChanged = true;//메인이 바뀌었다.
			}
			else if(_prevTimelineLayers.Count != _nTimelineLayers)
			{
				isChanged = true;//개수가 바뀌었다.
			}
			else
			{
				for (int i = 0; i < _timelineLayers_All.Count; i++)
				{
					if(!_prevTimelineLayers.Contains(_timelineLayers_All[i]))
					{
						isChanged = true;//리스트 내용이 다르다.
						break;
					}
				}
			}
			

			if(isChanged && _nTimelineLayers > 0)
			{
				//선택된 타임라인 레이어가 있다면 항상 동기화를 하자
				AutoSelectObjectsFromTimelineLayers();

				//기즈모 메인에 대한 타임라인 레이어도 찾자
				AutoSelectGizmoTimelineLayer();
			}

			return isChanged;
		}





		/// <summary>
		/// 다수의 타임라인 레이어를 선택한다.
		/// SelectAnimTimelineLayers() 함수와 다르며, 기존의 선택을 유지한다.
		/// 입력값에 따라 "비활성 > 활성" 또는 "활성 > 비활성"으로만 동작한다.
		/// </summary>
		/// <param name="targetTimelineLayers"></param>
		/// <param name="multiSelect"></param>
		/// <returns></returns>
		public void SelectAnimTimelineLayersAddable(List<apAnimTimelineLayer> targetTimelineLayers, bool isDeselected2Selected)
		{
			if(targetTimelineLayers == null 
				|| targetTimelineLayers.Count == 0)
			{
				//입력값이 없다면..
				return;
			}

			//대체하는게 아니므로, 조건이 맞지 않으면 그냥 취소한다.
			
			bool isChanged = false;
			apAnimTimelineLayer srcTimelineLayer = null;
			for (int i = 0; i < targetTimelineLayers.Count; i++)
			{
				srcTimelineLayer = targetTimelineLayers[i];

				//타임라인이 다르므로 제외
				if(srcTimelineLayer._parentTimeline != _timeline)
				{
					continue;
				}

				if(isDeselected2Selected)
				{
					//입력 값을 모두 "선택"에 추가한다.
					if(!_timelineLayers_All.Contains(srcTimelineLayer))
					{
						_timelineLayers_All.Add(srcTimelineLayer);
						isChanged = true;
					}
				}
				else
				{
					//입력 값을 모두 "선택"에서 삭제한다.
					if(_timelineLayers_All.Contains(srcTimelineLayer))
					{
						_timelineLayers_All.Remove(srcTimelineLayer);
						isChanged = true;
					}
					//메인인 경우, 메인을 제거
					if(_timelineLayer == srcTimelineLayer)
					{
						_timelineLayer = null;
						isChanged = true;
					}
				}
			}

			//메인 레이어를 못찾았다면 All의 첫번째를 메인으로 선택하자
			if(_timelineLayer == null && _timelineLayers_All.Count > 0)
			{
				_timelineLayer = _timelineLayers_All[0];
			}

			//메인을 제외한 나머지 레이어를 Sub로 넣자.
			apAnimTimelineLayer curLayer = null;
			for (int i = 0; i < _timelineLayers_All.Count; i++)
			{
				curLayer = _timelineLayers_All[i];
				if(curLayer != _timelineLayer)
				{
					_timelineLayers_Sub.Add(curLayer);
				}
			}

			_nTimelineLayers = _timelineLayers_All.Count;
			

			if(isChanged)
			{
				//선택된 타임라인 레이어가 있다면 항상 동기화를 하자
				AutoSelectObjectsFromTimelineLayers();

				//기즈모 메인에 대한 타임라인 레이어도 찾자
				AutoSelectGizmoTimelineLayer();
			}
		}








		/// <summary>
		/// [타임라인 레이어 선택됨] > [오브젝트를 자동으로 선택]
		/// 타임라인 레이어가 선택된 상태에서 > 해당 오브젝트들을 선택한다.
		/// </summary>
		public void AutoSelectObjectsFromTimelineLayers()
		{
			if(_nTimelineLayers == 0)
			{
				//타임라인이 없어서 선택이 힘들다
				Clear();
				return;
			}

			//일단 선택 정보는 초기화
			ClearTF();
			ClearBone();
			ClearControlParam();

			
			apAnimTimeline parentTimeline = null;

			//메인을 먼저 검사한다.
			if(_timelineLayer != null && _timelineLayer._parentTimeline != null)
			{
				parentTimeline = _timelineLayer._parentTimeline;

				if(parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					//타임라인의 타입이 "애니메이션 모디파이어"인 경우
					//각각의 타임라인 레이어와 연결된 오브젝트를 찾자
					switch (_timelineLayer._linkModType)
					{
						case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
							if(_timelineLayer._linkedMeshTransform != null)
							{
								//MeshTF 선택
								Select(_timelineLayer._linkedMeshTransform, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							break;

						case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
							if(_timelineLayer._linkedMeshGroupTransform != null)
							{
								//MeshGroupTF 선택
								Select(null, _timelineLayer._linkedMeshGroupTransform, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							break;

						case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
							if(_timelineLayer._linkedBone != null)
							{
								//Bone 선택
								Select(null, null, _timelineLayer._linkedBone, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
							}
							break;
					}
				}
				else if(parentTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
				{
					//타임라인의 타입이 "컨트롤 파라미터"인 경우
					if(_timelineLayer._linkedControlParam != null)
					{
						SelectControlParamForAnim(_timelineLayer._linkedControlParam);
					}
				}
			}

			
			//여기서 서브만 검사하자
			apSelection.MULTI_SELECT multiSelect = apSelection.MULTI_SELECT.Main;
			apAnimTimelineLayer curTimelineLayer = null;

			for (int iTL = 0; iTL < _nTimelineLayers; iTL++)
			{
				curTimelineLayer = _timelineLayers_All[iTL];
				if(curTimelineLayer == null || curTimelineLayer == _timelineLayer)
				{
					continue;
				}


				parentTimeline = curTimelineLayer._parentTimeline;
				if(parentTimeline == null) { continue; }

				
				//multiSelect = (curTimelineLayer == _timelineLayer) ? apSelection.MULTI_SELECT.Main : apSelection.MULTI_SELECT.AddOrSubtract;
				multiSelect = apSelection.MULTI_SELECT.AddOrSubtract;

				if(parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					//타임라인의 타입이 "애니메이션 모디파이어"인 경우
					//각각의 타임라인 레이어와 연결된 오브젝트를 찾자
					switch (curTimelineLayer._linkModType)
					{
						case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
							if(curTimelineLayer._linkedMeshTransform != null)
							{
								//MeshTF 선택
								Select(curTimelineLayer._linkedMeshTransform, null, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);
							}
							break;

						case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
							if(curTimelineLayer._linkedMeshGroupTransform != null)
							{
								//MeshGroupTF 선택
								Select(null, curTimelineLayer._linkedMeshGroupTransform, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);
							}
							break;

						case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
							if(curTimelineLayer._linkedBone != null)
							{
								//Bone 선택
								Select(null, null, curTimelineLayer._linkedBone, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);
							}
							break;
					}
				}
				else if(parentTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
				{
					//타임라인의 타입이 "컨트롤 파라미터"인 경우
					if(curTimelineLayer._linkedControlParam != null)
					{
						SelectControlParamForAnim(curTimelineLayer._linkedControlParam);
					}
				}
			}
		}


		/// <summary>
		/// [오브젝트 선택된 상태] > [타임라인 레이어] + [WorkKeyframe] 선택
		/// 오브젝트가 선택된 상태에서 > 타임라인 레이어들을 자동으로 선택한다.
		/// 현재 선택된 Timeline을 입력하면 거기에 해당하는 타임라인 레이어만 찾고, null을 입력하면 타임라인 레이어도 자동으로 찾는다.
		/// 리턴은 공통된 타임라인.
		/// 단, 현재의 AnimClip은 무조건 입력해야한다.
		/// </summary>
		public apAnimTimeline AutoSelectTimelineLayers(	apAnimTimeline selectedTimeline, 
														apAnimClip animClip,
														apSelection.EX_EDIT exEditMode)
		{
			//Debug.Log("AutoSelectTimelineLayers > " + GetTimelineLayerName());

			//타임라인은 초기화
			ClearTimelineLayers();

			if(animClip == null)
			{
				return null;
			}

			
			object selectedMainObject = SelectedObject;
			apAnimTimelineLayer targetLayer = null;


			if(selectedTimeline == null)
			{
				
				//선택된 타임라인이 없다면
				//메인 오브젝트를 통해서 CommonTimeline을 찾자
				//없을 수도 있다.

				//- 여러개가 있다면 자동으로 선택되지 않는다.
				//- ExEdit 모드 중에는 선택되지 않는다.
				//- 선택된 오브젝트가 없다면 선택하지 않는다.
				
				if(exEditMode != apSelection.EX_EDIT.None || selectedMainObject == null)
				{
					//Timeline을 찾을 수 없다.
					return null;
				}

				List<apAnimTimeline> resultTimeline = new List<apAnimTimeline>();

				apAnimTimeline curTimeline = null;
				

				for (int iT = 0; iT < animClip._timelines.Count; iT++)
				{
					curTimeline = animClip._timelines[iT];

					//만약 애니메이션 타임라인인데 연결된 모디파이어가 없다면.. (에러)
					if(curTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
						&& curTimeline._linkedModifier == null)
					{
						continue;
					}

					targetLayer = curTimeline.GetTimelineLayer(selectedMainObject);
					if(targetLayer != null)
					{
						//적당한 레이어가 있다. > 이 타임라인은 자동 선택 후보이다.
						resultTimeline.Add(curTimeline);
					}
				}
				
				if(resultTimeline.Count == 1)
				{
					//자동 선택할만한 타임라인이 딱 1개인 경우
					selectedTimeline = resultTimeline[0];
				}



				//다시 찾았는데도 없다면 리턴...
				if(selectedTimeline == null)
				{
					return null;
				}
			}

			//각각의 객체에 해당하는 타임라인 레이어를 찾자
			//selectedTimeline의 레이어만 찾으면 되며, 해당되는 레이어가 없을 수도 있다.
			//원래는 한개의 레이어만 찾으면 되지만, 여기서는 여러개를 찾아야 한다.
			_timeline = selectedTimeline;
			//불필요한 선택을 막기 위해 TF+Bone / ControlParam을 따로 체크한다.
			if(_timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				if(_timeline._linkedModifier != null)
				{
					//TF, Bone을 모두 각각 체크한다.
					apTransform_Mesh curMeshTF = null;
					apTransform_MeshGroup curMeshGroupTF = null;
					apBone curBone = null;
					
					for (int i = 0; i < _nMeshTF; i++)
					{
						curMeshTF = _meshTF_List_All[i];
						targetLayer = _timeline.GetTimelineLayer(curMeshTF);
						
						if(targetLayer == null) { continue; }

						//타임라인 리스트에 추가
						_timelineLayers_All.Add(targetLayer);
						
						//메인/서브 판단후 추가한다.
						if(curMeshTF == selectedMainObject)	{ _timelineLayer = targetLayer; }
						else								{ _timelineLayers_Sub.Add(targetLayer); }
					}

					for (int i = 0; i < _nMeshGroupTF; i++)
					{
						curMeshGroupTF = _meshGroupTF_List_All[i];
						targetLayer = _timeline.GetTimelineLayer(curMeshGroupTF);
						
						if(targetLayer == null) { continue; }

						//타임라인 리스트에 추가
						_timelineLayers_All.Add(targetLayer);
						
						//메인/서브 판단후 추가한다.
						if(curMeshGroupTF == selectedMainObject)	{ _timelineLayer = targetLayer; }
						else										{ _timelineLayers_Sub.Add(targetLayer); }
					}

					for (int i = 0; i < _nBone; i++)
					{
						curBone = _bone_List_All[i];
						targetLayer = _timeline.GetTimelineLayer(curBone);
						
						if(targetLayer == null) { continue; }

						//타임라인 리스트에 추가
						_timelineLayers_All.Add(targetLayer);
						
						//메인/서브 판단후 추가한다.
						if(curBone == selectedMainObject)	{ _timelineLayer = targetLayer; }
						else								{ _timelineLayers_Sub.Add(targetLayer); }
					}

				}
			}
			else if(_timeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				if(_controlParam_Anim != null)
				{
					targetLayer = _timeline.GetTimelineLayer(_controlParam_Anim);
					if(targetLayer != null)
					{
						//적당한 레이어가 있다.
						_timelineLayers_All.Add(targetLayer);
						//컨트롤 파라미터의 경우 자동으로 메인으로 선택된다.
						_timelineLayer = targetLayer;
					}
				}
			}

			_nTimelineLayers = _timelineLayers_All.Count;
			
			////WorkKeyframe도 찾아서 선택하자. > 이 함수는 외부에서 선택
			//AutoSelectWorkKeyframes(animClip, isPlaying);


			//기즈모 메인에 대한 타임라인 레이어도 찾자
			AutoSelectGizmoTimelineLayer();


			return selectedTimeline;
		}


		/// <summary>
		/// 애니메이션의 프레임이 변경되거나, 선택 정보가 바뀌면 이 함수를 무조건 호출하자.
		/// AutoSelectAnimWorkKeyframe 함수의 역할을 한다.		
		/// </summary>
		public void AutoSelectWorkKeyframes(apAnimClip animClip, bool isPlaying, out bool isWorkKeyframeChanged)
		{
			//이전의 Work Keyframe들을 저장한다. (변화를 체크하기 위함)
			//이전의 Work Keyframe들이 새롭게 선택된 키프레임들에 모두 포함되어 있다면, 변화가 없는 것으로 판단. (더 추가되는건 괜찮다?)

			bool isAnimFrameIndexChanged = false;//애니메이션 프레임이 바뀌었다면 귀찮게 Keyframe을 하나씩 대조하지 말자

			if (animClip != null)
			{
				//비교를 하자
				if (_workAnimFrameIndex != animClip.CurFrame)
				{
					//재생되는 프레임이 바뀌었다.
					isAnimFrameIndexChanged = true;
				}
			}
			else
			{
				isAnimFrameIndexChanged = true;//AnimClip이 없다면 애니메이션 프레임이 바뀐 것과 마찬가지
			}

			List<apAnimKeyframe> prevKeyframes_All = null;
			int nPrevWorkKeyframes = 0;
			if (!isAnimFrameIndexChanged)
			{
				//재생되는 프레임이 바뀌지 않았다면
				//일일이 대조를 해야한다.
				prevKeyframes_All = new List<apAnimKeyframe>();
				nPrevWorkKeyframes = _workKeyframes_All != null ? _workKeyframes_All.Count : 0;
				if (nPrevWorkKeyframes > 0)
				{
					for (int i = 0; i < nPrevWorkKeyframes; i++)
					{
						prevKeyframes_All.Add(_workKeyframes_All[i]);
					}
				}
			}

			//WorkKeyframe들을 일단 초기화.
			ClearWorkKeyframes();

			if (animClip == null
				|| isPlaying                //플레이 중에는 WorkKeyframe이 설정되지 않는다.
				|| _nTimelineLayers == 0    //선택된 타임라인 레이어가 없다.
				)
			{
				isWorkKeyframeChanged = true;//기존의 WorkKeyframe이 모두 선택되지 않았으므로 바뀐것과 마찬가지
				return;
			}


			int curFrame = animClip.CurFrame;
			apAnimTimelineLayer curTimelineLayer = null;
			apAnimKeyframe curKeyframe = null;


			//재생중인 프레임을 기록하자
			_workAnimFrameIndex = curFrame;

			for (int iTL = 0; iTL < _nTimelineLayers; iTL++)
			{
				curTimelineLayer = _timelineLayers_All[iTL];
				if (curTimelineLayer == null) { continue; }

				curKeyframe = curTimelineLayer.GetKeyframeByFrameIndex(curFrame);

				if (curKeyframe == null) { continue; }

				if (curTimelineLayer == _timelineLayer)
				{
					//메인인 경우
					_workKeyframe = curKeyframe;
				}
				else
				{
					//서브
					_workKeyframes_Sub.Add(curKeyframe);
				}

				//전체 추가
				_workKeyframes_All.Add(curKeyframe);
			}

			_nWorkKeyframes = _workKeyframes_All.Count;


			//재생되는 프레임이 바뀌었다면
			if (isAnimFrameIndexChanged || nPrevWorkKeyframes == 0)
			{
				isWorkKeyframeChanged = true;
				return;
			}


			//이전의 키프레임들이 모두 선택되어있는지 체크하자
			
			//선택된 키프레임들이 이전의 키프레임보다 적다면 체크할 필요가 없다.
			if(_nWorkKeyframes < nPrevWorkKeyframes)
			{
				isWorkKeyframeChanged = true;//이전의 키프레임들 중에 선택되지 않은게 분명히 있다.
				return;
			}


			bool isAnyNotContained = false;
			apAnimKeyframe prevKeyframe = null;
			for (int iPrev = 0; iPrev < nPrevWorkKeyframes; iPrev++)
			{
				prevKeyframe = prevKeyframes_All[iPrev];
				if(!_workKeyframes_All.Contains(prevKeyframe))
				{
					//제외된 이전의 키프레임 발견
					isAnyNotContained = true;
					break;
				}
			}

			isWorkKeyframeChanged = isAnyNotContained;//제외된게 하나라도 있다.
		}



		//기즈모 메인 객체에 대한 타임라인 레이어도 찾자
		public void AutoSelectGizmoTimelineLayer()
		{
			_timelineLayer_Gizmo = null;

			if(_timeline == null || _timelineLayer == null)
			{
				//타임라인이나 타임라인 메인이 선택되지 않았다면 생략
				return;
			}

			if(_timeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
			{
				//컨트롤 파라미터 타입의 타임라인도 선택 생략
				return;
			}

			apModifierBase linkedModifier = _timeline._linkedModifier;
			if(linkedModifier == null)
			{
				//모디파이어가 없넹..
				return;
			}

			if(_meshTF_GizmoMain == null && _meshGroupTF_GizmoMain == null && _bone_GizmoMain == null)
			{
				//기즈모 메인이 없어도 생략
				return;
			}

			//메인과 같다면 그대로 사용하고, 다르다면 전체 "선택된 레이어 중에서 찾자"
			//없을 수도 있다.
			if(_meshTF_GizmoMain != null)
			{
				if(_meshTF_GizmoMain == _meshTF)
				{
					//MeshTF로 같다면
					_timelineLayer_Gizmo = _timelineLayer;
				}
				else
				{
					//찾자
					_timelineLayer_Gizmo = _timelineLayers_All.Find(delegate(apAnimTimelineLayer a)
					{
						return a._linkedMeshTransform == _meshTF_GizmoMain;
					});
				}
			}
			else if(_meshGroupTF_GizmoMain != null)
			{
				if(_meshGroupTF_GizmoMain == _meshGroupTF)
				{
					//MeshGroupTF로 같다면
					_timelineLayer_Gizmo = _timelineLayer;
				}
				else
				{
					//찾자
					_timelineLayer_Gizmo = _timelineLayers_All.Find(delegate(apAnimTimelineLayer a)
					{
						return a._linkedMeshGroupTransform == _meshGroupTF_GizmoMain;
					});
				}
			}
			else if(_bone_GizmoMain != null && linkedModifier.IsTarget_Bone)
			{
				if(_bone_GizmoMain == _bone)
				{
					//Bone이 같다면
					_timelineLayer_Gizmo = _timelineLayer;
				}
				else
				{
					//찾자
					_timelineLayer_Gizmo = _timelineLayers_All.Find(delegate(apAnimTimelineLayer a)
					{
						return a._linkedBone == _bone_GizmoMain;
					});
				}
			}
		}

		
		// Function - Get/Set 공통 속성들
		//-------------------------------------------------------
		public SyncResult Sync0 { get { return _sync[0]; } }
		public SyncResult Sync1 { get { return _sync[1]; } }
		public SyncResult Sync2 { get { return _sync[2]; } }
		public SyncResult Sync3 { get { return _sync[3]; } }
		public SyncResult Sync4 { get { return _sync[4]; } }
		public SyncResult Sync5 { get { return _sync[5]; } }
		public SyncResult Sync6 { get { return _sync[6]; } }
		public SyncResult Sync7 { get { return _sync[7]; } }
		public SyncResult Sync8 { get { return _sync[8]; } }
		public SyncResult Sync9 { get { return _sync[9]; } }
		public SyncResult Sync10 { get { return _sync[10]; } }
		public SyncResult Sync11 { get { return _sync[11]; } }
		public SyncResult Sync12 { get { return _sync[12]; } }
		public SyncResult Sync13 { get { return _sync[13]; } }
		public SyncResult Sync14 { get { return _sync[14]; } }
		public SyncResult Sync15 { get { return _sync[15]; } }
		public SyncResult Sync16 { get { return _sync[16]; } }
		public SyncResult Sync17 { get { return _sync[17]; } }
		public SyncResult Sync18 { get { return _sync[18]; } }
		public SyncResult Sync19 { get { return _sync[19]; } }
		
		// 1. Mesh/MeshGroup TF : _isSocket
		public void Check_TF_IsSocket()
		{
			RunCheck_TF_Bool(0, FuncCheck_TF_IsSocket);
		}

		private bool FuncCheck_TF_IsSocket(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF)
		{
			if(targetMeshTF != null) { return targetMeshTF._isSocket; }
			if(targetMeshGroupTF != null) { return targetMeshGroupTF._isSocket; }
			return false;
		}

		public void Set_TF_IsSocket()
		{
			RunSet_TF_Toggle(0, FuncToggle_TF_IsSocket);
		}

		private void FuncToggle_TF_IsSocket(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, bool boolValue)
		{
			if(targetMeshTF != null)			{ targetMeshTF._isSocket = boolValue; }
			else if(targetMeshGroupTF != null)	{ targetMeshGroupTF._isSocket = boolValue; }
		}


		//2. Mesh TF : _shaderType
		public void Check_Mesh_ShaderType()
		{
			RunCheck_TF_Int(0, FuncCheck_Mesh_ShaderType);
		}

		private int FuncCheck_Mesh_ShaderType(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF)
		{
			if(targetMeshTF != null) { return (int)targetMeshTF._shaderType; }
			return -1;
		}

		public void Set_Mesh_ShaderType(apPortrait.SHADER_TYPE shaderType)
		{
			RunSet_TF_Int(0, FuncSet_Mesh_ShaderType, (int)shaderType);
		}

		private void FuncSet_Mesh_ShaderType(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, int intValue)
		{
			if(targetMeshTF != null)
			{
				targetMeshTF._shaderType = (apPortrait.SHADER_TYPE)intValue;
			}
		}


		//3. Bone : DefaultMatrix : 동시에 Pos, Angle, Scale을 동기화/수정하자
		//여러개의 Sync를 모두 사용 (0 : Pos / 1 : Angle / 2 : Scale)
		public void Check_Bone_DefaultMatrix()
		{
			RunCheck_Bone_Vector2(0, FuncCheck_Bone_DefaultMatrix_Pos);
			RunCheck_Bone_Float(1, FuncCheck_Bone_DefaultMatrix_Angle);
			RunCheck_Bone_Vector2(2, FuncCheck_Bone_DefaultMatrix_Scale);
		}

		private Vector2 FuncCheck_Bone_DefaultMatrix_Pos(apBone targetBone) { return (targetBone != null) ? targetBone._defaultMatrix._pos : Vector2.zero; }
		private float FuncCheck_Bone_DefaultMatrix_Angle(apBone targetBone) { return (targetBone != null) ? targetBone._defaultMatrix._angleDeg : 0.0f; }
		private Vector2 FuncCheck_Bone_DefaultMatrix_Scale(apBone targetBone)	{ return (targetBone != null) ? targetBone._defaultMatrix._scale : Vector2.one; }

		//값 적용은 각각
		public void Set_Bone_DefaultMatrix_Pos(Vector2 pos, bool isX, bool isY)
		{
			RunSet_Bone_Vector2(0, FuncSet_DefaultMatrix_Pos, pos, isX, isY);
		}

		public void Set_Bone_DefaultMatrix_Angle(float angle)
		{
			RunSet_Bone_Float(1, FuncSet_DefaultMatrix_Angle, angle);
		}

		public void Set_Bone_DefaultMatrix_Scale(Vector2 scale, bool isX, bool isY)
		{
			RunSet_Bone_Vector2(2, FuncSet_DefaultMatrix_Scale, scale, isX, isY);
		}

		private void FuncSet_DefaultMatrix_Pos(apBone bone, Vector2 vec2Value, bool isX, bool isY)
		{
			if(bone != null)
			{
				if(isX) { bone._defaultMatrix._pos.x = vec2Value.x; }
				if(isY) { bone._defaultMatrix._pos.y = vec2Value.y; }
				bone._defaultMatrix.MakeMatrix();
			}
		}

		private void FuncSet_DefaultMatrix_Angle(apBone bone, float floatValue)
		{
			if(bone != null)
			{
				bone._defaultMatrix._angleDeg = floatValue;
				bone._defaultMatrix.MakeMatrix();
			}
		}

		private void FuncSet_DefaultMatrix_Scale(apBone bone, Vector2 vec2Value, bool isX, bool isY)
		{
			if(bone != null)
			{
				if(isX) { bone._defaultMatrix._scale.x = vec2Value.x; }
				if(isY) { bone._defaultMatrix._scale.y = vec2Value.y; }
				bone._defaultMatrix.MakeMatrix();
			}
		}

		

		//4. Bone IsSocket
		public void Check_Bone_IsSocketEnabled()
		{
			RunCheck_Bone_Bool(0, FuncCheck_Bone_IsSocketEnabled);
		}

		private bool FuncCheck_Bone_IsSocketEnabled(apBone bone)
		{
			if(bone != null) { return bone._isSocketEnabled; }
			return false;
		}

		public void Set_Bone_IsSocketEnabled()
		{
			RunSet_Bone_Toggle(0, FuncToggle_Bone_IsSocketEnabled);
		}

		private void FuncToggle_Bone_IsSocketEnabled(apBone bone, bool boolValue)
		{
			if(bone != null)
			{
				bone._isSocketEnabled = boolValue;
			}
		}

		//5. Bone Shape(0:Color, 1:Width, 2:Length, 3:Taper, 4:Helper)
		public void Check_Bone_Shape()
		{
			RunCheck_Bone_Color(0, FuncCheck_Bone_Shape_Color);
			RunCheck_Bone_Int(1, FuncCheck_Bone_Shape_Width);
			RunCheck_Bone_Int(2, FuncCheck_Bone_Shape_Length);
			RunCheck_Bone_Int(3, FuncCheck_Bone_Shape_Taper);
			RunCheck_Bone_Bool(4, FuncCheck_Bone_Shape_Helper);
		}

		private Color FuncCheck_Bone_Shape_Color(apBone bone)	{ return (bone != null) ? bone._color : Color.black; }
		private int FuncCheck_Bone_Shape_Width(apBone bone)		{ return (bone != null) ? bone._shapeWidth : 30; }
		private int FuncCheck_Bone_Shape_Length(apBone bone)	{ return (bone != null) ? bone._shapeLength : 50; }
		private int FuncCheck_Bone_Shape_Taper(apBone bone)		{ return (bone != null) ? bone._shapeTaper : 100; }
		private bool FuncCheck_Bone_Shape_Helper(apBone bone)	{ return (bone != null) ? bone._shapeHelper : false; }


		public void Set_Bone_Shape_Color(Color color)
		{
			RunSet_Bone_Color(0, FuncSet_Bone_Shape_Color, color, true, true, true, true);
		}

		public void Set_Bone_Shape_Width(int shapeWidth)
		{
			RunSet_Bone_Int(1, FuncSet_Bone_Shape_Width, shapeWidth);
		}

		public void Set_Bone_Shape_Length(int shapeLength)
		{
			RunSet_Bone_Int(2, FuncSet_Bone_Shape_Length, shapeLength);
		}

		public void Set_Bone_Shape_Taper(int shapeTaper)
		{
			RunSet_Bone_Int(3, FuncSet_Bone_Shape_Taper, shapeTaper);
		}

		public void Set_Bone_Shape_Helper()
		{
			RunSet_Bone_Toggle(4, FuncToggleSet_Bone_Shape_Taper);
		}

		private void FuncSet_Bone_Shape_Color(apBone bone, Color color, bool isR, bool isG, bool isB, bool isA)		{ if(bone != null) { bone._color = color; } }
		private void FuncSet_Bone_Shape_Width(apBone bone, int width)		{ if(bone != null) { bone._shapeWidth = width; } }
		private void FuncSet_Bone_Shape_Length(apBone bone, int length)		{ if(bone != null) { bone._shapeLength = length; } }
		private void FuncSet_Bone_Shape_Taper(apBone bone, int taper)		{ if(bone != null) { bone._shapeTaper = taper; } }
		private void FuncToggleSet_Bone_Shape_Taper(apBone bone, bool isHelper) { if(bone != null) { bone._shapeHelper = isHelper; } }


		//6. Bone : Jiggle 설정들
		//0 : _isJiggle (bool)
		//1 : _jiggle_Mass (float)
		//2 : _jiggle_K (float)
		//3 : _jiggle_Drag (float)
		//4 : _jiggle_Damping (float)
		//5 : _isJiggleAngleConstraint (bool)
		//6 : _jiggle_AngleLimit_Min (float)
		//7 : _jiggle_AngleLimit_Max (float)
		public void Check_Bone_Jiggle()
		{
			RunCheck_Bone_Bool(0, FuncCheck_Bone_Jiggle_IsEnabled);
			RunCheck_Bone_Float(1, FuncCheck_Bone_Jiggle_Mass);
			RunCheck_Bone_Float(2, FuncCheck_Bone_Jiggle_K);
			RunCheck_Bone_Float(3, FuncCheck_Bone_Jiggle_Drag);
			RunCheck_Bone_Float(4, FuncCheck_Bone_Jiggle_Damping);
			RunCheck_Bone_Bool(5, FuncCheck_Bone_Jiggle_IsConstraint);
			RunCheck_Bone_Float(6, FuncCheck_Bone_Jiggle_AngleMin);
			RunCheck_Bone_Float(7, FuncCheck_Bone_Jiggle_AngleMax);
			RunCheck_Bone_Bool(8, FuncCheck_Bone_Jiggle_WeightLinkControlParam);
			RunCheck_Bone_Int(9, FuncCheck_Bone_Jiggle_WeightControlParamID);
		}

		private bool FuncCheck_Bone_Jiggle_IsEnabled(apBone bone)	{ return (bone != null) ? bone._isJiggle : false; }
		private float FuncCheck_Bone_Jiggle_Mass(apBone bone)		{ return (bone != null) ? bone._jiggle_Mass : 0.0f; }
		private float FuncCheck_Bone_Jiggle_K(apBone bone)			{ return (bone != null) ? bone._jiggle_K : 0.0f; }
		private float FuncCheck_Bone_Jiggle_Drag(apBone bone)		{ return (bone != null) ? bone._jiggle_Drag : 0.0f; }
		private float FuncCheck_Bone_Jiggle_Damping(apBone bone)	{ return (bone != null) ? bone._jiggle_Damping : 0.0f; }
		private bool FuncCheck_Bone_Jiggle_IsConstraint(apBone bone) { return (bone != null) ? bone._isJiggleAngleConstraint : false; }
		private float FuncCheck_Bone_Jiggle_AngleMin(apBone bone)	{ return (bone != null) ? bone._jiggle_AngleLimit_Min : 0.0f; }
		private float FuncCheck_Bone_Jiggle_AngleMax(apBone bone)	{ return (bone != null) ? bone._jiggle_AngleLimit_Max : 0.0f; }
		private bool FuncCheck_Bone_Jiggle_WeightLinkControlParam(apBone bone)		{ return (bone != null) ? bone._jiggle_IsControlParamWeight : false; }
		private int FuncCheck_Bone_Jiggle_WeightControlParamID(apBone bone)			{ return (bone != null) ? bone._jiggle_WeightControlParamID : -1; }


		public void Set_Bone_Jiggle_IsEnabled()					{ RunSet_Bone_Toggle(0, FuncToggleSet_Bone_Jiggle_IsEnabled); }
		public void Set_Bone_Jiggle_Mass(float mass)			{ RunSet_Bone_Float(1, FuncSet_Bone_Jiggle_Mass, mass); }
		public void Set_Bone_Jiggle_K(float kValue)				{ RunSet_Bone_Float(2, FuncSet_Bone_Jiggle_K, kValue); }
		public void Set_Bone_Jiggle_Drag(float drag)			{ RunSet_Bone_Float(3, FuncSet_Bone_Jiggle_Drag, drag); }
		public void Set_Bone_Jiggle_Damping(float damping)		{ RunSet_Bone_Float(4, FuncSet_Bone_Jiggle_Damping, damping); }
		public void Set_Bone_Jiggle_IsConstraint()				{ RunSet_Bone_Toggle(5, FuncToggleSet_Bone_Jiggle_IsConstraint); }
		public void Set_Bone_Jiggle_AngleMin(float angleMin)	{ RunSet_Bone_Float(6, FuncSet_Bone_Jiggle_AngleMin, angleMin); }
		public void Set_Bone_Jiggle_AngleMax(float angleMax)	{ RunSet_Bone_Float(7, FuncSet_Bone_Jiggle_AngleMax, angleMax); }
		public void Set_Bone_Jiggle_WeightLinkControlParam(bool isLinkControlParam)			{ RunSet_Bone_Bool(8, FuncSet_Bone_Jiggle_WeightLinkControlParam, isLinkControlParam); }
		public void Set_Bone_Jiggle_WeightControlParamID(int controlParamID)				{ RunSet_Bone_Int(9, FuncSet_Bone_Jiggle_WeightControlParamID, controlParamID); }

		private void FuncToggleSet_Bone_Jiggle_IsEnabled(apBone bone, bool isEnabled)		{ if(bone != null) { bone._isJiggle = isEnabled; } }
		private void FuncSet_Bone_Jiggle_Mass(apBone bone, float mass)						{ if(bone != null) { bone._jiggle_Mass = mass; } }
		private void FuncSet_Bone_Jiggle_K(apBone bone, float kValue)						{ if(bone != null) { bone._jiggle_K = kValue; } }
		private void FuncSet_Bone_Jiggle_Drag(apBone bone, float drag)						{ if(bone != null) { bone._jiggle_Drag = drag; } }
		private void FuncSet_Bone_Jiggle_Damping(apBone bone, float damping)				{ if(bone != null) { bone._jiggle_Damping = damping; } }
		private void FuncToggleSet_Bone_Jiggle_IsConstraint(apBone bone, bool isConstraint) { if(bone != null) { bone._isJiggleAngleConstraint = isConstraint; } }
		private void FuncSet_Bone_Jiggle_AngleMin(apBone bone, float angleMin)				{ if(bone != null) { bone._jiggle_AngleLimit_Min = angleMin; } }
		private void FuncSet_Bone_Jiggle_AngleMax(apBone bone, float angleMax)				{ if(bone != null) { bone._jiggle_AngleLimit_Max = angleMax; } }
		private void FuncSet_Bone_Jiggle_WeightLinkControlParam(apBone bone, bool isLinkControlParam)	{ if(bone != null) { bone._jiggle_IsControlParamWeight = isLinkControlParam; } }
		private void FuncSet_Bone_Jiggle_WeightControlParamID(apBone bone, int controlParamID)			{ if(bone != null) { bone._jiggle_WeightControlParamID = controlParamID; } }




		public void Check_TimelineLayer_GUIColor() { RunCheck_TimelineLayer_Color(0, FuncCheck_TimelineLayer_GUIColor); }
		private Color FuncCheck_TimelineLayer_GUIColor(apAnimTimelineLayer timelineLayer)	{ return (timelineLayer != null) ? timelineLayer._guiColor : Color.black; }

		public void Set_TimelineLayer_GUIColor(Color guiColor) { RunSet_TimelineLayer_Color(0, FuncSet_TimelineLayer_GUIColor, guiColor); }
		private void FuncSet_TimelineLayer_GUIColor(apAnimTimelineLayer timelineLayer, Color color)	{ if(timelineLayer != null) { timelineLayer._guiColor = color; } }



		#region 동기화 함수들 (Check)
		// 공통 속성을 가져오기 위한 동기화 함수 원형
		//-------------------------------------------------------
		private delegate bool		FUNC_CHECK_TF_BOOL(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF);
		private delegate int		FUNC_CHECK_TF_INT(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF);
		private delegate float		FUNC_CHECK_TF_FLOAT(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF);
		private delegate Vector2	FUNC_CHECK_TF_VEC2(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF);
		private delegate Vector3	FUNC_CHECK_TF_VEC3(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF);
		private delegate Vector4	FUNC_CHECK_TF_VEC4(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF);
		private delegate Color		FUNC_CHECK_TF_COLOR(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF);

		private delegate bool		FUNC_CHECK_BONE_BOOL(apBone targetBone);
		private delegate int		FUNC_CHECK_BONE_INT(apBone targetBone);
		private delegate float		FUNC_CHECK_BONE_FLOAT(apBone targetBone);
		private delegate Vector2	FUNC_CHECK_BONE_VEC2(apBone targetBone);
		private delegate Vector3	FUNC_CHECK_BONE_VEC3(apBone targetBone);
		private delegate Vector4	FUNC_CHECK_BONE_VEC4(apBone targetBone);
		private delegate Color		FUNC_CHECK_BONE_COLOR(apBone targetBone);

		private delegate bool		FUNC_CHECK_TIMELINELAYER_BOOL(apAnimTimelineLayer timelineLayer);
		private delegate int		FUNC_CHECK_TIMELINELAYER_INT(apAnimTimelineLayer timelineLayer);
		private delegate float		FUNC_CHECK_TIMELINELAYER_FLOAT(apAnimTimelineLayer timelineLayer);
		private delegate Vector2	FUNC_CHECK_TIMELINELAYER_VEC2(apAnimTimelineLayer timelineLayer);
		private delegate Vector3	FUNC_CHECK_TIMELINELAYER_VEC3(apAnimTimelineLayer timelineLayer);
		private delegate Vector4	FUNC_CHECK_TIMELINELAYER_VEC4(apAnimTimelineLayer timelineLayer);
		private delegate Color		FUNC_CHECK_TIMELINELAYER_COLOR(apAnimTimelineLayer timelineLayer);

		private void RunCheck_TF_Bool(int iSync, FUNC_CHECK_TF_BOOL funcCheckTFBool)
		{
			if (_nMeshTF == 0 && _nMeshGroupTF == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}

			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nMeshTF; i++)			{ _sync[iSync].SetValue_Bool(funcCheckTFBool(_meshTF_List_All[i], null)); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ _sync[iSync].SetValue_Bool(funcCheckTFBool(null, _meshGroupTF_List_All[i])); }
		}

		private void RunCheck_TF_Int(int iSync, FUNC_CHECK_TF_INT funcCheckTFInt)
		{
			if (_nMeshTF == 0 && _nMeshGroupTF == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}

			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nMeshTF; i++)			{ _sync[iSync].SetValue_Int(funcCheckTFInt(_meshTF_List_All[i], null)); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ _sync[iSync].SetValue_Int(funcCheckTFInt(null, _meshGroupTF_List_All[i])); }
		}

		private void RunCheck_TF_Float(int iSync, FUNC_CHECK_TF_FLOAT funcCheckTFFloat)
		{
			if (_nMeshTF == 0 && _nMeshGroupTF == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nMeshTF; i++)			{ _sync[iSync].SetValue_Float(funcCheckTFFloat(_meshTF_List_All[i], null)); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ _sync[iSync].SetValue_Float(funcCheckTFFloat(null, _meshGroupTF_List_All[i])); }
		}

		private void RunCheck_TF_Vector2(int iSync, FUNC_CHECK_TF_VEC2 funcCheckTFVec2)
		{
			if (_nMeshTF == 0 && _nMeshGroupTF == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nMeshTF; i++)			{ _sync[iSync].SetValue_Vector2(funcCheckTFVec2(_meshTF_List_All[i], null)); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ _sync[iSync].SetValue_Vector2(funcCheckTFVec2(null, _meshGroupTF_List_All[i])); }
		}

		private void RunCheck_TF_Vector3(int iSync, FUNC_CHECK_TF_VEC3 funcCheckTFVec3)
		{
			if (_nMeshTF == 0 && _nMeshGroupTF == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nMeshTF; i++)			{ _sync[iSync].SetValue_Vector3(funcCheckTFVec3(_meshTF_List_All[i], null)); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ _sync[iSync].SetValue_Vector3(funcCheckTFVec3(null, _meshGroupTF_List_All[i])); }
		}

		private void RunCheck_TF_Vector4(int iSync, FUNC_CHECK_TF_VEC4 funcCheckTFVec4)
		{
			if (_nMeshTF == 0 && _nMeshGroupTF == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nMeshTF; i++)		{ _sync[iSync].SetValue_Vector4(funcCheckTFVec4(_meshTF_List_All[i], null)); }
			for (int i = 0; i < _nMeshGroupTF; i++)	{ _sync[iSync].SetValue_Vector4(funcCheckTFVec4(null, _meshGroupTF_List_All[i])); }
		}

		private void RunCheck_TF_Color(int iSync, FUNC_CHECK_TF_COLOR funcCheckTFColor)
		{
			if (_nMeshTF == 0 && _nMeshGroupTF == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nMeshTF; i++)			{ _sync[iSync].SetValue_Color(funcCheckTFColor(_meshTF_List_All[i], null)); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ _sync[iSync].SetValue_Color(funcCheckTFColor(null, _meshGroupTF_List_All[i])); }
		}



		private void RunCheck_Bone_Bool(int iSync, FUNC_CHECK_BONE_BOOL funcCheckBoneBool)
		{
			if (_nBone == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nBone; i++)	{ _sync[iSync].SetValue_Bool(funcCheckBoneBool(_bone_List_All[i])); }
		}

		private void RunCheck_Bone_Int(int iSync, FUNC_CHECK_BONE_INT funcCheckBoneInt)
		{
			if (_nBone == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nBone; i++)	{ _sync[iSync].SetValue_Int(funcCheckBoneInt(_bone_List_All[i])); }
		}

		private void RunCheck_Bone_Float(int iSync, FUNC_CHECK_BONE_FLOAT funcCheckBoneFloat)
		{
			if (_nBone == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nBone; i++)	{ _sync[iSync].SetValue_Float(funcCheckBoneFloat(_bone_List_All[i])); }
		}

		private void RunCheck_Bone_Vector2(int iSync, FUNC_CHECK_BONE_VEC2 funcCheckBoneVec2)
		{
			if (_nBone == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nBone; i++)	{ _sync[iSync].SetValue_Vector2(funcCheckBoneVec2(_bone_List_All[i])); }
		}

		private void RunCheck_Bone_Vector3(int iSync, FUNC_CHECK_BONE_VEC3 funcCheckBoneVec3)
		{
			if (_nBone == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nBone; i++)	{ _sync[iSync].SetValue_Vector3(funcCheckBoneVec3(_bone_List_All[i])); }
		}

		private void RunCheck_Bone_Vector4(int iSync, FUNC_CHECK_BONE_VEC4 funcCheckBoneVec4)
		{
			if (_nBone == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nBone; i++)	{ _sync[iSync].SetValue_Vector4(funcCheckBoneVec4(_bone_List_All[i])); }
		}

		private void RunCheck_Bone_Color(int iSync, FUNC_CHECK_BONE_COLOR funcCheckBoneColor)
		{
			if (_nBone == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nBone; i++)	{ _sync[iSync].SetValue_Color(funcCheckBoneColor(_bone_List_All[i])); }
		}





		private void RunCheck_TimelineLayer_Bool(int iSync, FUNC_CHECK_TIMELINELAYER_BOOL funcCheckTimelineLayerBool)
		{
			if (_nTimelineLayers == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nTimelineLayers; i++)	{ _sync[iSync].SetValue_Bool(funcCheckTimelineLayerBool(_timelineLayers_All[i])); }
		}

		private void RunCheck_TimelineLayer_Int(int iSync, FUNC_CHECK_TIMELINELAYER_INT funcCheckTimelineLayerInt)
		{
			if (_nTimelineLayers == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nTimelineLayers; i++)	{ _sync[iSync].SetValue_Int(funcCheckTimelineLayerInt(_timelineLayers_All[i])); }
		}

		private void RunCheck_TimelineLayer_Float(int iSync, FUNC_CHECK_TIMELINELAYER_FLOAT funcCheckTimelineLayerFloat)
		{
			if (_nTimelineLayers == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nTimelineLayers; i++)	{ _sync[iSync].SetValue_Float(funcCheckTimelineLayerFloat(_timelineLayers_All[i])); }
		}

		private void RunCheck_TimelineLayer_Vector2(int iSync, FUNC_CHECK_TIMELINELAYER_VEC2 funcCheckTimelineLayerVec2)
		{
			if (_nTimelineLayers == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nTimelineLayers; i++)	{ _sync[iSync].SetValue_Vector2(funcCheckTimelineLayerVec2(_timelineLayers_All[i])); }
		}

		private void RunCheck_TimelineLayer_Vector3(int iSync, FUNC_CHECK_TIMELINELAYER_VEC3 funcCheckTimelineLayerVec3)
		{
			if (_nTimelineLayers == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nTimelineLayers; i++)	{ _sync[iSync].SetValue_Vector3(funcCheckTimelineLayerVec3(_timelineLayers_All[i])); }
		}

		private void RunCheck_TimelineLayer_Vector4(int iSync, FUNC_CHECK_TIMELINELAYER_VEC4 funcCheckTimelineLayerVec4)
		{
			if (_nTimelineLayers == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nTimelineLayers; i++)	{ _sync[iSync].SetValue_Vector4(funcCheckTimelineLayerVec4(_timelineLayers_All[i])); }
		}

		private void RunCheck_TimelineLayer_Color(int iSync, FUNC_CHECK_TIMELINELAYER_COLOR funcCheckTimelineLayerColor)
		{
			if (_nTimelineLayers == 0)
			{
				_sync[iSync].SetInvalid();//실패
				return;
			}
			_sync[iSync].ReadyToSync();

			for (int i = 0; i < _nTimelineLayers; i++)	{ _sync[iSync].SetValue_Color(funcCheckTimelineLayerColor(_timelineLayers_All[i])); }
		}
		#endregion


		#region 동기화 함수들 (Set)
		//동기화 속성을 적용하는 함수들
		private delegate void FUNC_SET_TF_BOOL(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, bool boolValue);
		private delegate void FUNC_SET_TF_INT(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, int intValue);
		private delegate void FUNC_SET_TF_FLOAT(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, float floatValue);
		private delegate void FUNC_SET_TF_VEC2(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, Vector2 vec2Value, bool isX, bool isY);
		private delegate void FUNC_SET_TF_VEC3(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, Vector3 vec3Value, bool isX, bool isY, bool isZ);
		private delegate void FUNC_SET_TF_VEC4(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, Vector4 vec4Value, bool isX, bool isY, bool isZ, bool isW);
		private delegate void FUNC_SET_TF_COLOR(apTransform_Mesh targetMeshTF, apTransform_MeshGroup targetMeshGroupTF, Color colorValue, bool isR, bool isG, bool isB, bool isA);

		private delegate void FUNC_SET_BONE_BOOL(apBone targetBone, bool boolValue);
		private delegate void FUNC_SET_BONE_INT(apBone targetBone, int intValue);
		private delegate void FUNC_SET_BONE_FLOAT(apBone targetBone, float floatValue);
		private delegate void FUNC_SET_BONE_VEC2(apBone targetBone, Vector2 vec2Value, bool isX, bool isY);
		private delegate void FUNC_SET_BONE_VEC3(apBone targetBone, Vector3 vec3Value, bool isX, bool isY, bool isZ);
		private delegate void FUNC_SET_BONE_VEC4(apBone targetBone, Vector4 vec4Value, bool isX, bool isY, bool isZ, bool isW);
		private delegate void FUNC_SET_BONE_COLOR(apBone targetBone, Color colorValue, bool isR, bool isG, bool isB, bool isA);

		private delegate void FUNC_SET_TIMELINELAYER_BOOL(apAnimTimelineLayer timelineLaye, bool boolValue);
		private delegate void FUNC_SET_TIMELINELAYER_INT(apAnimTimelineLayer timelineLaye, int intValue);
		private delegate void FUNC_SET_TIMELINELAYER_FLOAT(apAnimTimelineLayer timelineLaye, float floatValue);
		private delegate void FUNC_SET_TIMELINELAYER_VEC2(apAnimTimelineLayer timelineLaye, Vector2 vec2Value, bool isX, bool isY);
		private delegate void FUNC_SET_TIMELINELAYER_VEC3(apAnimTimelineLayer timelineLaye, Vector3 vec3Value, bool isX, bool isY, bool isZ);
		private delegate void FUNC_SET_TIMELINELAYER_VEC4(apAnimTimelineLayer timelineLaye, Vector4 vec4Value, bool isX, bool isY, bool isZ, bool isW);
		private delegate void FUNC_SET_TIMELINELAYER_COLOR(apAnimTimelineLayer timelineLaye, Color colorValue);

		private void RunSet_TF_Bool(int iSync, FUNC_SET_TF_BOOL funcSetTFBool, bool boolValue)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFBool(_meshTF_List_All[i], null, boolValue); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFBool(null, _meshGroupTF_List_All[i], boolValue); }
		}


		private void RunSet_TF_Toggle(int iSync, FUNC_SET_TF_BOOL funcSetTFBool)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			//동기화가 안되었다면 > True로 강제
			//그 외는 값 전환
			bool nextValue = false;

			if (!_sync[iSync].IsSync)	{ nextValue = true; }
			else						{ nextValue = !_sync[iSync].SyncValue_Bool; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFBool(_meshTF_List_All[i], null, nextValue); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFBool(null, _meshGroupTF_List_All[i], nextValue); }
		}

		private void RunSet_TF_Int(int iSync, FUNC_SET_TF_INT funcSetTFInt, int intValue)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFInt(_meshTF_List_All[i], null, intValue); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFInt(null, _meshGroupTF_List_All[i], intValue); }
		}

		private void RunSet_TF_Float(int iSync, FUNC_SET_TF_FLOAT funcSetTFFloat, float floatValue)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFFloat(_meshTF_List_All[i], null, floatValue); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFFloat(null, _meshGroupTF_List_All[i], floatValue); }
		}

		private void RunSet_TF_Vector2(int iSync, FUNC_SET_TF_VEC2 funcSetTFVec2, Vector2 vec2Value, bool isX, bool isY)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFVec2(_meshTF_List_All[i], null, vec2Value, isX, isY); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFVec2(null, _meshGroupTF_List_All[i], vec2Value, isX, isY); }
		}

		private void RunSet_TF_Vector3(int iSync, FUNC_SET_TF_VEC3 funcSetTFVec3, Vector3 vec3Value, bool isX, bool isY, bool isZ)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFVec3(_meshTF_List_All[i], null, vec3Value, isX, isY, isZ); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFVec3(null, _meshGroupTF_List_All[i], vec3Value, isX, isY, isZ); }
		}

		private void RunSet_TF_Vector4(int iSync, FUNC_SET_TF_VEC4 funcSetTFVec4, Vector4 vec4Value, bool isX, bool isY, bool isZ, bool isW)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFVec4(_meshTF_List_All[i], null, vec4Value, isX, isY, isZ, isW); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFVec4(null, _meshGroupTF_List_All[i], vec4Value, isX, isY, isZ, isW); }
		}

		private void RunSet_TF_Color(int iSync, FUNC_SET_TF_COLOR funcSetTFColor, Color colorValue, bool isR, bool isG, bool isB, bool isA)
		{
			if ((_nMeshTF == 0 && _nMeshGroupTF == 0) || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nMeshTF; i++)			{ funcSetTFColor(_meshTF_List_All[i], null, colorValue, isR, isG, isB, isA); }
			for (int i = 0; i < _nMeshGroupTF; i++)		{ funcSetTFColor(null, _meshGroupTF_List_All[i], colorValue, isR, isG, isB, isA); }
		}





		private void RunSet_Bone_Bool(int iSync, FUNC_SET_BONE_BOOL funcSetBoneBool, bool boolValue)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneBool(_bone_List_All[i], boolValue); }
		}

		private void RunSet_Bone_Toggle(int iSync, FUNC_SET_BONE_BOOL funcSetBoneBool)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			//동기화가 안되었다면 > False로 강제
			//그 외는 값 전환
			bool nextValue = false;

			if (!_sync[iSync].IsSync) { nextValue = true; }
			else						{ nextValue = !_sync[iSync].SyncValue_Bool; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneBool(_bone_List_All[i], nextValue); }
		}

		private void RunSet_Bone_Int(int iSync, FUNC_SET_BONE_INT funcSetBoneInt, int intValue)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneInt(_bone_List_All[i], intValue); }
		}

		private void RunSet_Bone_Float(int iSync, FUNC_SET_BONE_FLOAT funcSetBoneFloat, float floatValue)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneFloat(_bone_List_All[i], floatValue); }
		}

		private void RunSet_Bone_Vector2(int iSync, FUNC_SET_BONE_VEC2 funcSetBoneVec2, Vector2 vec2Value, bool isX, bool isY)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneVec2(_bone_List_All[i], vec2Value, isX, isY); }
		}

		private void RunSet_Bone_Vector3(int iSync, FUNC_SET_BONE_VEC3 funcSetBoneVec3, Vector3 vec3Value, bool isX, bool isY, bool isZ)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneVec3(_bone_List_All[i], vec3Value, isX, isY, isZ); }
		}

		private void RunSet_Bone_Vector4(int iSync, FUNC_SET_BONE_VEC4 funcSetBoneVec4, Vector4 vec4Value, bool isX, bool isY, bool isZ, bool isW)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneVec4(_bone_List_All[i], vec4Value, isX, isY, isZ, isW); }
		}

		private void RunSet_Bone_Color(int iSync, FUNC_SET_BONE_COLOR funcSetBoneColor, Color colorValue, bool isR, bool isG, bool isB, bool isA)
		{
			if (_nBone == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nBone; i++)	{ funcSetBoneColor(_bone_List_All[i], colorValue, isR, isG, isB, isA); }
		}




		private void RunSet_TimelineLayer_Bool(int iSync, FUNC_SET_TIMELINELAYER_BOOL funcSetTimelineLayerBool, bool boolValue)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerBool(_timelineLayers_All[i], boolValue); }
		}

		private void RunSet_TimelineLayer_Toggle(int iSync, FUNC_SET_TIMELINELAYER_BOOL funcSetTimelineLayerBool)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			//동기화가 안되었다면 > False로 강제
			//그 외는 값 전환
			bool nextValue = false;

			if (!_sync[iSync].IsSync)	{ nextValue = true; }
			else						{ nextValue = !_sync[iSync].SyncValue_Bool; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerBool(_timelineLayers_All[i], nextValue); }
		}

		private void RunSet_TimelineLayer_Int(int iSync, FUNC_SET_TIMELINELAYER_INT funcSetTimelineLayerInt, int intValue)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerInt(_timelineLayers_All[i], intValue); }
		}

		private void RunSet_TimelineLayer_Float(int iSync, FUNC_SET_TIMELINELAYER_FLOAT funcSetTimelineLayerFloat, float floatValue)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerFloat(_timelineLayers_All[i], floatValue); }
		}

		private void RunSet_TimelineLayer_Vector2(int iSync, FUNC_SET_TIMELINELAYER_VEC2 funcSetTimelineLayerVec2, Vector2 vec2Value, bool isX, bool isY)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerVec2(_timelineLayers_All[i], vec2Value, isX, isY); }
		}

		private void RunSet_TimelineLayer_Vector3(int iSync, FUNC_SET_TIMELINELAYER_VEC3 funcSetTimelineLayerVec3, Vector3 vec3Value, bool isX, bool isY, bool isZ)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerVec3(_timelineLayers_All[i], vec3Value, isX, isY, isZ); }
		}

		private void RunSet_TimelineLayer_Vector4(int iSync, FUNC_SET_TIMELINELAYER_VEC4 funcSetTimelineLayerVec4, Vector4 vec4Value, bool isX, bool isY, bool isZ, bool isW)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerVec4(_timelineLayers_All[i], vec4Value, isX, isY, isZ, isW); }
		}

		private void RunSet_TimelineLayer_Color(int iSync, FUNC_SET_TIMELINELAYER_COLOR funcSetTimelineLayerColor, Color colorValue)
		{
			if (_nTimelineLayers == 0 || !_sync[iSync].IsValid)
			{ return; }

			for (int i = 0; i < _nTimelineLayers; i++)	{ funcSetTimelineLayerColor(_timelineLayers_All[i], colorValue); }
		}
		#endregion


		// Get / Set
		//-------------------------------------------------------
		/// <summary>
		/// 현재 선택된 메인 오브젝트. 타입에 관계 없으며 기즈모에서 사용하는 용도
		/// </summary>
		public object SelectedObject
		{
			get
			{
				if (_meshTF != null)		{ return _meshTF; }
				if (_meshGroupTF != null)	{ return _meshGroupTF; }
				if (_bone != null)			{ return _bone; }
				if (_controlParam_Anim != null) { return _controlParam_Anim; }
				return null;
			}
		}

		public object SelectedObject_WithoutControlParam
		{
			get
			{
				if (_meshTF != null)		{ return _meshTF; }
				if (_meshGroupTF != null)	{ return _meshGroupTF; }
				if (_bone != null)			{ return _bone; }
				return null;
			}
		}

		//선택된 메인 객체들
		public apTransform_Mesh MeshTF				{ get { return _meshTF; } }
		public apTransform_MeshGroup MeshGroupTF	{ get { return _meshGroupTF; } }
		public apBone Bone							{ get { return _bone; } }
		public apControlParam ControlParamForAnim	{ get { return _controlParam_Anim; } }

		
		//선택된 서브 객체들
		public List<apTransform_Mesh> SubMeshTFs			{ get { return _meshTF_List_Sub; } }
		public List<apTransform_MeshGroup> SubMeshGroupTFs	{ get { return _meshGroupTF_List_Sub; } }
		public List<apBone> SubBones						{ get { return _bone_List_Sub; } }

		//선택된 모든(메인+서브) 객체들
		public List<apTransform_Mesh> AllMeshTFs			{ get { return _meshTF_List_All; } }
		public List<apTransform_MeshGroup> AllMeshGroupTFs	{ get { return _meshGroupTF_List_All; } }
		public List<apBone> AllBones						{ get { return _bone_List_All; } }

		//기즈모 편집용 객체들
		public apTransform_Mesh GizmoMeshTF
		{
			get
			{
				//if(_meshTF_GizmoMain != null) { return _meshTF_GizmoMain; }
				//if(_meshTF != null) { return _meshTF; }
				//return null;
				return _meshTF_GizmoMain;
			}
		}

		public apTransform_MeshGroup GizmoMeshGroupTF
		{
			get
			{
				//if(_meshGroupTF_GizmoMain != null) { return _meshGroupTF_GizmoMain; }
				//if(_meshGroupTF != null) { return _meshGroupTF; }
				//return null;
				return _meshGroupTF_GizmoMain;
			}
		}

		public apBone GizmoBone
		{
			get
			{
				//if(_bone_GizmoMain != null) { return _bone_GizmoMain; }
				//if(_bone != null) { return _bone; }
				//return null;
				return _bone_GizmoMain;
			}
		}

		public List<apTransform_Mesh> AllGizmoMeshTFs				{ get { return _meshTF_List_Gizmo; } }
		public List<apTransform_MeshGroup> AllGizmoMeshGroupTFs		{ get { return _meshGroupTF_List_Gizmo; } }
		public List<apBone> AllGizmoBones							{ get { return _bone_List_Gizmo; } }

		//[기즈모용] 선택된 기즈모용 객체들 중에서 자식 메시그룹에 속한게 하나라도 있나.
		public bool IsChildMeshGroupObjectSelectedForGizmo(bool isTF, bool isBone)
		{
			if (isTF && _isChildMeshGroupObjectSelected_Gizmo_TF)
			{
				return true;
			}
			if (isBone && _isChildMeshGroupObjectSelected_Gizmo_Bone)
			{
				return true;
			}
			return false;
		}
			

		//선택된 객체들의 수
		public int NumMeshTF		{ get { return _nMeshTF; } }
		public int NumMeshGroupTF	{ get { return _nMeshGroupTF; } }
		public int NumBone			{ get { return _nBone; } }
		
		public int NumGizmoMeshTF		{ get { return _meshTF_List_Gizmo.Count; } }
		public int NumGizmoMeshGroupTF	{ get { return _meshGroupTF_List_Gizmo.Count; } }
		public int NumGizmoBone			{ get { return _bone_List_Gizmo.Count; } }

		//애니메이션의 타임라인 레이어와 Work 키프레임들
		public apAnimTimelineLayer TimelineLayer { get { return _timelineLayer; } }
		public List<apAnimTimelineLayer> SubTimelineLayers { get { return _timelineLayers_Sub; } }
		public List<apAnimTimelineLayer> AllTimelineLayers { get { return _timelineLayers_All; } }
		public apAnimTimelineLayer TimelineLayer_Gizmo { get { return _timelineLayer_Gizmo; } }
		public int NumTimelineLayers { get { return _nTimelineLayers; } }

		public apAnimKeyframe WorkKeyframe { get { return _workKeyframe; } }
		public List<apAnimKeyframe> SubWorkKeyframes { get { return _workKeyframes_Sub; } }
		public List<apAnimKeyframe> AllWorkKeyframes { get { return _workKeyframes_All; } }
		public int NumWorkKeyframes { get { return _nWorkKeyframes; } }


		//디버깅용 Get 함수
		public string GetTimelineLayerName() { return (_timelineLayer != null) ? _timelineLayer.DisplayName : "<Null>"; }


		// 참조용 함수들
		//------------------------------------------------------
		/// <summary>
		/// 키프레임이 타임라인레이어 중 하나에 포함되었는가
		/// </summary>
		public bool IsContainKeyframInAllTimelineLayers(apAnimKeyframe keyframe)
		{
			if(_nTimelineLayers == 0)
			{
				return false;
			}
			for (int i = 0; i < _nTimelineLayers; i++)
			{
				if(_timelineLayers_All[i].IsKeyframeContain(keyframe))
				{
					return true;
				}
			}
			return false;
		}

	}
}