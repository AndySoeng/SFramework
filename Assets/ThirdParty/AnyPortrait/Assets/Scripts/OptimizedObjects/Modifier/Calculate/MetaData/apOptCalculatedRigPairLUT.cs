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
//using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 20.11.26 : 리깅 성능을 높이기 위한 테이블
	/// 미리 Transform - Bone의 Rigging Matrix 연산을 통합하여 처리한 후, 조회만 한다.
	/// struct 타입의 apMatrix3x3대신 apMatrix를 이용하여 연산을 한다. (레퍼런스 전달을 위해)
	/// 키값은 두개를 묶은 하나. (1) Transform과 (2) Bone이다. 객체 키값이 아닌 index로 저장하여 호출한다. Dictionary를 최소화
	/// 이 클래스는 Transform의 CalculatedResultStack의 멤버로 저장된다. 거기서 호출될거니까..
	/// </summary>
	public class apOptCalculatedRigPairLUT
	{
		// Members
		//-----------------------------------------
		private apOptTransform _parentTransform = null;

		public class LUTUnit
		{
			public apOptBone _linkedBone = null;
			public apMatrix3x3 _resultMatrix = new apMatrix3x3();

			//추가 21.9.19 : 본 동기화시 LUT의 대상 본이 바뀐다.
			public apOptBone _syncBone = null;

			public LUTUnit(apOptBone bone)
			{
				_linkedBone = bone;
				_syncBone = bone;
			}

			/// <summary>
			/// [추가 21.9.19] 동기화된 본이 있다면 LUT의 대상이 바뀌어야 한다.
			/// </summary>
			public void EnableSyncBone()
			{
				//동기화된 본이 있다면 그걸 할당, 그렇지 않으면 기존의 본을 그대로 이용하자
				_syncBone = _linkedBone._syncBone != null ? _linkedBone._syncBone : _linkedBone;
			}

			/// <summary>
			/// [추가 21.9.19] 동기화가 끝났다면 이 함수를 호출하자
			/// </summary>
			public void DisableSyncBone()
			{
				_syncBone = _linkedBone;
			}
		}

		public LUTUnit[] _LUT = null;
		private int _nLUT = 0;

		private Dictionary<apOptBone, int> _bone2Index = null;

		private LUTUnit _cal_CurUnit = null;

		// Init
		//-----------------------------------------
		public apOptCalculatedRigPairLUT(apOptTransform parentTransform)
		{
			_parentTransform = parentTransform;

			//일단 초기화. MakeLUT 함수에서 데이터가 완성된다.
			_LUT = null;
			_nLUT = 0;

			if (_bone2Index == null)
			{
				_bone2Index = new Dictionary<apOptBone, int>();
			}
			_bone2Index.Clear();
		}

		/// <summary>
		/// CalculatedResultStack에 입력된 Param들을 순회하면서 RigBone 테이블을 완성하고, index를 만들어두자
		/// </summary>
		/// <param name="calResultStack"></param>
		/// <param name="resultParams"></param>
		public void MakeLUTAndLink(apOptCalculatedResultStack calResultStack, List<apOptCalculatedResultParam> resultParams)
		{
			if(resultParams == null || resultParams.Count == 0)
			{
				return;
			}

			if (_bone2Index == null)
			{
				_bone2Index = new Dictionary<apOptBone, int>();
			}
			_bone2Index.Clear();

			List<apOptBone> boneList = new List<apOptBone>();
			_nLUT = 0;
			int curLUTUnitIndex = 0;//매핑된 본 인덱스

			int nResultParams = resultParams.Count;
			apOptCalculatedResultParam curParam = null;
			apOptVertexRequest curVertReq = null;
			apOptVertexRequest.VertRigWeightTable curVertRigWeightTable = null;
			apOptVertexRequest.RigBoneWeightPair curRigBoneWeightPair = null;
			apOptBone curBone = null;
			int targetLUTUnitIndex = -1;

			for (int iParam = 0; iParam < nResultParams; iParam++)
			{
				curParam = resultParams[iParam];

				//Vertex 리스트를 돌면서 해당 버텍스에 연결된 본들을 확인하여 리스트에 추가한다.
				
				for (int iVR = 0; iVR < curParam._result_VertLocalPairs.Count; iVR++)
				{
					curVertReq = curParam._result_VertLocalPairs[iVR];
					int nBoneWeightTables = curVertReq._rigBoneWeightTables.Length;

					for (int iVert = 0; iVert < nBoneWeightTables; iVert++)
					{
						curVertRigWeightTable = curVertReq._rigBoneWeightTables[iVert];


						for (int iRig = 0; iRig < curVertRigWeightTable._nRigTable; iRig++)
						{
							curRigBoneWeightPair = curVertRigWeightTable._rigTable[iRig];
							curBone = curRigBoneWeightPair._bone;

							//이제 연결될 인덱스를 찾자
							
							if(_bone2Index.ContainsKey(curBone))
							{
								//이미 매핑 정보가 있다!
								targetLUTUnitIndex = _bone2Index[curBone];
							}
							else
							{
								//새로 매핑 정보 추가
								_bone2Index.Add(curBone, curLUTUnitIndex);
								boneList.Add(curBone);
								
								targetLUTUnitIndex = curLUTUnitIndex;
								curLUTUnitIndex++;//인덱스 증가
							}

							//LUT와 연결하자
							curRigBoneWeightPair._iRigPairLUT = targetLUTUnitIndex;
						}
					}
				}
				
			}
			
			//완성된 LUT를 배열로 만들자
			_nLUT = boneList.Count;
			_LUT = new LUTUnit[_nLUT];

			for (int i = 0; i < _nLUT; i++)
			{
				_LUT[i] = new LUTUnit(boneList[i]);
			}
		}

		// Functions
		//-----------------------------------------
		public void Update()
		{
			//Rig LUT를 계산한다.
			//LUT에 Rigging Matrix를 계산한다.
			if(_nLUT == 0)
			{
				return;
			}

			_cal_CurUnit = null;
			for (int i = 0; i < _nLUT; i++)
			{
				_cal_CurUnit = _LUT[i];
				_cal_CurUnit._resultMatrix.SetMatrix(ref _parentTransform._vertMeshWorldNoModInverseMatrix);
				_cal_CurUnit._resultMatrix.Multiply(ref _cal_CurUnit._linkedBone._vertWorld2BoneModWorldMatrix);
				_cal_CurUnit._resultMatrix.Multiply(ref _parentTransform._vertMeshWorldNoModMatrix);
			}
		}


		/// <summary>
		/// Update의 본 동기화 버전.
		/// </summary>
		public void UpdateAsSyncBones()
		{
			//Rig LUT를 계산한다.
			//LUT에 Rigging Matrix를 계산한다.
			if(_nLUT == 0)
			{
				return;
			}

			int nSyncUpdate = 0;
			_cal_CurUnit = null;
			for (int i = 0; i < _nLUT; i++)
			{
				_cal_CurUnit = _LUT[i];
				_cal_CurUnit._resultMatrix.SetMatrix(ref _parentTransform._vertMeshWorldNoModInverseMatrix);
				//_cal_CurUnit._resultMatrix.Multiply(ref _cal_CurUnit._linkedBone._vertWorld2BoneModWorldMatrix);
				_cal_CurUnit._resultMatrix.Multiply(ref _cal_CurUnit._syncBone._vertWorld2BoneModWorldMatrix);//[Sync]
				_cal_CurUnit._resultMatrix.Multiply(ref _parentTransform._vertMeshWorldNoModMatrix);

				if(_cal_CurUnit._syncBone != _cal_CurUnit._linkedBone)
				{
					nSyncUpdate++;
				}
			}
		}


		//추가 21.9.19 : 본 동기화에 따라 LUT를 다르게 처리해야한다.
		public void EnableSyncBones()
		{
			if(_nLUT == 0)
			{
				//Debug.LogError("Enable Sync Bones > LUT 0");
				return;
			}

			for (int i = 0; i < _nLUT; i++)
			{
				_LUT[i].EnableSyncBone();
			}
		}

		public void DisableSyncBones()
		{
			if(_nLUT == 0)
			{
				return;
			}

			for (int i = 0; i < _nLUT; i++)
			{
				_LUT[i].DisableSyncBone();
			}
		}
		
	}
}