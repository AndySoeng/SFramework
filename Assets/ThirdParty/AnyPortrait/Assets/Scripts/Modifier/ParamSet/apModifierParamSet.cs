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
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// (Root)MeshGroup -> ModifierStack -> Modifer -> ParamSet -> ModifiedMesh/Bone [+Vertex] 단계로 저장되는 데이터 중 하나.
	/// Input에 해당하는 값을 가지고 있으며, 그에따른 ModMesh/ModBone을 리스트로 가진다.
	/// </summary>
	[Serializable]
	public class apModifierParamSet
	{
		// Members
		//------------------------------------------
		[NonSerialized]
		public apModifierParamSetGroup _parentParamSetGroup = null;

		public apControlParam SyncControlParam { get { return _parentParamSetGroup._keyControlParam; } }

		//컨트롤러의 어떤 값에 동기화되는가
		//public bool _conSyncValue_Bool = false;
		public int _conSyncValue_Int = 0;
		public float _conSyncValue_Float = 0.0f;
		public Vector2 _conSyncValue_Vector2 = Vector2.zero;
		//public Vector3 _conSyncValue_Vector3 = Vector3.zero;
		//public Color _conSyncValue_Color = Color.black;

		public Vector2 _conSyncValueRange_Under = Vector2.zero;
		public Vector2 _conSyncValueRange_Over = Vector2.zero;


		//3. KeyFrame으로 정의될 때
		public int _keyframeUniqueID = -1;

		[NonSerialized]
		private apAnimKeyframe _syncKeyframe = null;
		public apAnimKeyframe SyncKeyframe { get { return _syncKeyframe; } }


		//추가
		//Control Param 타입에 한해서
		//ParamSet의 Weight를 100%이 아닌 일부로 둘 수 있다.
		//그럼 Overlap 되는 ParamSetGroup의 Weight를 바꿀 수 있다.
		//기존 : [ParamSetGroup Weight]로 보간 Weight 지정
		//변경 : [ParamSetGroup Weight x ParamSet Weight의 가중치합(0~1)]으로 보간 Weight 지정
		//이름은 OverlapWeight로 한다.
		//기본값은 1. Control Param 동기화 타입이 아니라면 이 값은 사용되지 않는다.
		[SerializeField]
		public float _overlapWeight = 1.0f;




		// 변경 사항
		[SerializeField]
		public List<apModifiedMesh> _meshData = new List<apModifiedMesh>();

		[SerializeField]
		public List<apModifiedBone> _boneData = new List<apModifiedBone>();

		// Init
		//------------------------------------------
		public apModifierParamSet()
		{

		}

		public void LinkParamSetGroup(apModifierParamSetGroup paramSetGroup)
		{
			_parentParamSetGroup = paramSetGroup;
		}

		public void LinkSyncKeyframe(apAnimKeyframe keyframe)
		{
			_keyframeUniqueID = keyframe._uniqueID;
			_syncKeyframe = keyframe;
		}


		// Functions
		//------------------------------------------
		//추가되었다.
		//Bake 전에 업데이트하는 부분
		public void UpdateBeforeBake(apPortrait portrait, apMeshGroup mainMeshGroup, apTransform_MeshGroup mainMeshGroupTransform)
		{
			if (_meshData != null && _meshData.Count > 0)
			{
				for (int i = 0; i < _meshData.Count; i++)
				{
					_meshData[i].UpdateBeforeBake(portrait, mainMeshGroup, mainMeshGroupTransform);
				}
			}
			if (_boneData != null && _boneData.Count > 0)
			{
				for (int i = 0; i < _boneData.Count; i++)
				{
					_boneData[i].UpdateBeforeBake(portrait, mainMeshGroup, mainMeshGroupTransform);
				}
			}
		}

		// Get / Set
		//------------------------------------------
		//public bool _conSyncValue_Bool = false;
		//public int _conSyncValue_Int = 0;
		//public float _conSyncValue_Float = 0.0f;
		//public Vector2 _conSyncValue_Vector2 = Vector2.zero;
		//public Vector3 _conSyncValue_Vector3 = Vector3.zero;
		//public Color _conSyncValue_Color = Color.black;
		public string ControlParamValue
		{
			get
			{
				if (SyncControlParam == null)
				{
					return "<no-control type>";
				}

				switch (SyncControlParam._valueType)
				{
					//case apControlParam.TYPE.Bool: return _conSyncValue_Bool.ToString();
					case apControlParam.TYPE.Int:
						return _conSyncValue_Int.ToString();
					case apControlParam.TYPE.Float:
						return _conSyncValue_Float.ToString();
					case apControlParam.TYPE.Vector2:
						return _conSyncValue_Vector2.ToString();
						//case apControlParam.TYPE.Vector3: return _conSyncValue_Vector3.ToString();
						//case apControlParam.TYPE.Color: return _conSyncValue_Color.ToString();
				}
				return "<unknown type>";
			}
		}


		//추가 20.2.23
		/// <summary>
		/// ParamKeyValue간의 키 값의 위치를 비교할 수 있는 값.
		/// 컨트롤 파라미터의 경우 키 값이 클수록 인덱스가 크다.
		/// 키프레임인 경우 키프레임의 인덱스를 그대로 사용한다.
		/// float 타입을 리턴.
		/// </summary>
		public float ComparableIndex
		{
			get
			{
				if(SyncControlParam != null)
				{
					switch (SyncControlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							return _conSyncValue_Int;
						case apControlParam.TYPE.Float:
							return _conSyncValue_Float;
						case apControlParam.TYPE.Vector2:
							return 
								(_conSyncValue_Vector2.x * Mathf.Abs(SyncControlParam._vec2_Max.y - SyncControlParam._vec2_Min.y))
								+ _conSyncValue_Vector2.y;
					}
				}
				if(SyncKeyframe != null)
				{
					return SyncKeyframe._frameIndex;
				}
				return -1;
			}
		}

		

		public bool IsContainMeshTransform(apTransform_Mesh meshTransform)
		{
			if (meshTransform == null)
			{
				return false;
			}
			return _meshData.Exists(delegate (apModifiedMesh a)
			{
				return a._isMeshTransform && a._transform_Mesh == meshTransform;
			});
		}

		public bool IsContainMeshGroupTransform(apTransform_MeshGroup meshGroupTransform)
		{
			if (meshGroupTransform == null)
			{
				return false;
			}
			return _meshData.Exists(delegate (apModifiedMesh a)
			{
				return !a._isMeshTransform && a._transform_MeshGroup == meshGroupTransform;
			});
		}

		public bool IsContainBone(apBone bone)
		{
			if (bone == null)
			{
				return false;
			}
			return _boneData.Exists(delegate (apModifiedBone a)
			{
				return a._bone == bone;
			});
		}
	}
}