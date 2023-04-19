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

	[Serializable]
	public class apOptParamSet
	{
		// Members
		//--------------------------------------------
		[NonSerialized]
		public apOptParamSetGroup _parentParamSetGroup = null;

		//1. Controller Param에 동기화될 때
		public apControlParam SyncControlParam { get { return _parentParamSetGroup._keyControlParam; } }

		//public bool _conSyncValue_Bool = false;
		public int _conSyncValue_Int = 0;
		public float _conSyncValue_Float = 0.0f;
		public Vector2 _conSyncValue_Vector2 = Vector2.zero;
		//public Vector3 _conSyncValue_Vector3 = Vector3.zero;
		//public Color _conSyncValue_Color = Color.black;

		//<추가>
		//2. Keyframe에 동기화될 때
		//Bake때는 ID만 받고, 첫 시작시 Link를 한다.
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


		public int _nMeshData = 0;
		public int _nBoneData = 0;

		[SerializeField]
		public List<apOptModifiedMesh> _meshData = new List<apOptModifiedMesh>();


		//19.5.23 : 최적화를 위해서 기존의 apOptModifiedMesh 에서 apOptModifiedMeshSet로 변경.
		public bool _isUseModMeshSet = false;//1.1.7부터는 이 값은 항상 true가 된다.
		[SerializeField]
		public List<apOptModifiedMeshSet> _meshSetData = new List<apOptModifiedMeshSet>();


		[SerializeField]
		public List<apOptModifiedBone> _boneData = new List<apOptModifiedBone>();


		// Init
		//--------------------------------------------
		public apOptParamSet()
		{

		}

		public void LinkParamSetGroup(apOptParamSetGroup paramSetGroup, apPortrait portrait)
		{
			_parentParamSetGroup = paramSetGroup;

			_syncKeyframe = null;
			if (_keyframeUniqueID >= 0)
			{
				//TODO
				//_syncKeyframe = 
				if (paramSetGroup._keyAnimTimelineLayer != null)
				{
					_syncKeyframe = paramSetGroup._keyAnimTimelineLayer.GetKeyframeByID(_keyframeUniqueID);
				}
			}

			if (_isUseModMeshSet)
			{
				//19.5.23 : 새로운 버전
				for (int i = 0; i < _meshSetData.Count; i++)
				{
					_meshSetData[i].Link(portrait);
				}
			}
			else
			{
				//이전 버전
				for (int i = 0; i < _meshData.Count; i++)
				{
					_meshData[i].Link(portrait);
				}
			}
			

			//TODO : OptBone은 현재 Link할 객체가 없다.
			//필요하다면 Link를 여기에 추가해주자

		}


		public IEnumerator LinkParamSetGroupAsync(apOptParamSetGroup paramSetGroup, apPortrait portrait, apAsyncTimer asyncTimer)
		{
			_parentParamSetGroup = paramSetGroup;

			_syncKeyframe = null;
			if (_keyframeUniqueID >= 0)
			{
				//TODO
				//_syncKeyframe = 
				if (paramSetGroup._keyAnimTimelineLayer != null)
				{
					_syncKeyframe = paramSetGroup._keyAnimTimelineLayer.GetKeyframeByID(_keyframeUniqueID);
				}
			}

			if(asyncTimer.IsYield())
			{
				yield return asyncTimer.WaitAndRestart();
			}

			if (_isUseModMeshSet)
			{
				//19.5.23 : 새로운 버전
				for (int i = 0; i < _meshSetData.Count; i++)
				{
					_meshSetData[i].Link(portrait);

					if (asyncTimer.IsYield())
					{
						yield return asyncTimer.WaitAndRestart();
					}
				}
			}
			else
			{
				//이전 버전
				for (int i = 0; i < _meshData.Count; i++)
				{
					_meshData[i].Link(portrait);

					if (asyncTimer.IsYield())
					{
						yield return asyncTimer.WaitAndRestart();
					}
				}
			}
			

			//TODO : OptBone은 현재 Link할 객체가 없다.
			//필요하다면 Link를 여기에 추가해주자

		}

		public void BakeModifierParamSet(apModifierParamSet srcParamSet, apPortrait portrait, bool isUseModMeshSet)
		{	
			_conSyncValue_Int = srcParamSet._conSyncValue_Int;
			_conSyncValue_Float = srcParamSet._conSyncValue_Float;
			_conSyncValue_Vector2 = srcParamSet._conSyncValue_Vector2;
			

			_keyframeUniqueID = srcParamSet._keyframeUniqueID;
			_syncKeyframe = null;

			_overlapWeight = srcParamSet._overlapWeight;//OverlapWeight를 집어넣자

			_meshData.Clear();
			_boneData.Clear();

			//19.5.23 : meshSetData 추가
			if(_meshSetData == null)
			{
				_meshSetData = new List<apOptModifiedMeshSet>();
			}
			_meshSetData.Clear();
			_isUseModMeshSet = isUseModMeshSet;//<<이 값이 1.1.7부터는 true가 된다.


			if (!_isUseModMeshSet)
			{
				//이전버전
				//SrcModifier ParamSet의 ModMesh, ModBone을 Bake해주자
				for (int i = 0; i < srcParamSet._meshData.Count; i++)
				{
					apModifiedMesh srcModMesh = srcParamSet._meshData[i];
					apOptModifiedMesh optModMesh = new apOptModifiedMesh();
					bool isResult = optModMesh.Bake(srcModMesh, portrait);
					if (isResult)
					{
						_meshData.Add(optModMesh);
					}
				}
			}
			else
			{
				//변경된 버전 : 19.5.23 (v.1.1.7)
				for (int i = 0; i < srcParamSet._meshData.Count; i++)
				{
					apModifiedMesh srcModMesh = srcParamSet._meshData[i];
					apOptModifiedMeshSet optModMeshSet = new apOptModifiedMeshSet();
					bool isResult = optModMeshSet.Bake(
											srcParamSet._parentParamSetGroup._parentModifier,
											srcParamSet._parentParamSetGroup,
											srcModMesh,
											portrait);
					if (isResult)
					{
						_meshSetData.Add(optModMeshSet);
					}
				}
			}


			//ModBone
			for (int i = 0; i < srcParamSet._boneData.Count; i++)
			{
				apModifiedBone srcModBone = srcParamSet._boneData[i];
				apOptModifiedBone optModBone = new apOptModifiedBone();
				bool isResult = optModBone.Bake(srcModBone, portrait);
				if (isResult)
				{
					_boneData.Add(optModBone);
				}
			}
		}

		// Functions
		//--------------------------------------------


		// Get / Set
		//--------------------------------------------

		//public string ControlParamValue
		//{
		//	get
		//	{
		//		if (_controlParam == null)
		//		{
		//			return "<no-control type>";
		//		}

		//		switch (_controlParam._valueType)
		//		{
		//			case apControlParam.TYPE.Bool: return _conSyncValue_Bool.ToString();
		//			case apControlParam.TYPE.Int: return _conSyncValue_Int.ToString();
		//			case apControlParam.TYPE.Float: return _conSyncValue_Float.ToString();
		//			case apControlParam.TYPE.Vector2: return _conSyncValue_Vector2.ToString();
		//			case apControlParam.TYPE.Vector3: return _conSyncValue_Vector3.ToString();
		//			case apControlParam.TYPE.Color: return _conSyncValue_Color.ToString();
		//		}
		//		return "<unknown type>";
		//	}
		//}

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
	}

}