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

	//에디터에서 작업 객체의 값 복사나 저장을 위한 기능을 제공하는 매니저
	//Stack 방식으로 저장을 한다.
	//각 SnapShot 데이터는 실제로 적용되는 객체에서 관리한다.
	public class apSnapShotManager
	{
		// Singletone
		//-------------------------------------------
		private static apSnapShotManager _instance = new apSnapShotManager();
		private static readonly object _obj = new object();
		public static apSnapShotManager I { get { lock (_obj) { return _instance; } } }



		// Members
		//-------------------------------------------
		public enum SNAPSHOT_TARGET
		{
			Mesh, MeshGroup, ModifiedMesh, Portrait,//ETC.. Keyframe?
		}

		public enum SAVE_TYPE
		{
			Copy,
			Record
		}


		//Copy 타입 (Clipboard)
		
		private apSnapShotStackUnit _clipboard_Keyframe = null;
		private apSnapShotStackUnit _clipboard_VertRig = null;
		//이전
		//private apSnapShotStackUnit _clipboard_ModMesh = null;
		//private apSnapShotStackUnit _clipboard_ModBone = null;

		//슬롯 4개로 변경
		//변경 21.6.24 : 다중 복사를 위해서 ModMesh + ModBone 저장 방식이 바뀐다.
		//- 단일 복사, 다중 복사, 단일 붙여넣기, 다중 붙여넣기로 구분된다.

		//각 복사, 붙여넣기는 다음과 같이 처리된다.
		//- 단일 복사 > 단일 붙여넣기 (조건에 맞으면 공유 가능)
		//- 다중 복사 > 다중 붙여넣기 (다른 객체로의 공유 불가)
		//상호간 교차 처리는 허용되지 않는다.

		private const int NUM_MOD_SLOTS = 4;
		//공통 데이터 (21.6.24)
		private apSnapShotStackUnit[] _clipboard_ModMeshes_Common = null;
		private apSnapShotStackUnit[] _clipboard_ModBones_Common = null;
		//키 오브젝트별 데이터
		private Dictionary<object, apSnapShotStackUnit[]> _clipboard_ModMeshes_KeyObjects = null;
		private Dictionary<object, apSnapShotStackUnit[]> _clipboard_ModBones_KeyObjects = null;



		//추가 3.29 : 여러개의 키프레임을 저장하기 위한 용도. Timeline에서 복사한 경우 해당한다.
		private apAnimClip _clipboard_AnimClipOfKeyframes = null;
		private List<apSnapShotStackUnit> _clipboard_Keyframes = null;
		private int _copied_keyframes_StartFrame = -1;
		private int _copied_keyframes_EndFrame = -1;

		//Record 타입
		private const int MAX_RECORD = 10;
		private List<apSnapShotStackUnit> _snapShotList = new List<apSnapShotStackUnit>();
		//이건 나중에 처리하자
		//private apSnapShotStackUnit _curSnapShot = null;
		//private int _iCurSnapShot = 0;
		//private bool _restoredSnapShot = false;

		//추가 19.8.9 : MultipleVertRig 타입
		private apSnapShot_MultipleVertRig _clipboard_MultipleVertRig = null;

		//추가 21.10.6 : 메시의 버텍스 복사하기
		private apSnapShot_Mesh _clipboard_MeshVertEdges = null;

		//추가 1.4.2 : 메시 핀을 복사하기
		private apSnapShot_MeshPin _clipboard_MeshPin = null;


		// Init
		//-------------------------------------------
		private apSnapShotManager()
		{
			Clear();
		}



		public void Clear()
		{
			//변경 > Null + 생성은 불합리하다. 
			if (_clipboard_Keyframe == null) { _clipboard_Keyframe = new apSnapShotStackUnit(); }
			_clipboard_Keyframe.Clear();

			if (_clipboard_VertRig == null) { _clipboard_VertRig = new apSnapShotStackUnit(); }
			_clipboard_VertRig.Clear();


			//이전
			//if(_clipboard_ModMeshes == null) { _clipboard_ModMeshes = new apSnapShotStackUnit[NUM_MOD_CLIPBOARD]; }
			//if(_clipboard_ModBones == null) { _clipboard_ModBones = new apSnapShotStackUnit[NUM_MOD_CLIPBOARD]; }

			//for (int i = 0; i < NUM_MOD_CLIPBOARD; i++)
			//{
			//	if (_clipboard_ModBones[i] == null) { _clipboard_ModBones[i] = new apSnapShotStackUnit(); }
			//	if (_clipboard_ModMeshes[i] == null) { _clipboard_ModMeshes[i] = new apSnapShotStackUnit(); }
			//	_clipboard_ModBones[i].Clear();
			//	_clipboard_ModMeshes[i].Clear();
			//}

			//변경 21.6.24 : 다중 복사/단일 복사를 모두 구분한다.
			if (_clipboard_ModMeshes_Common == null) { _clipboard_ModMeshes_Common = new apSnapShotStackUnit[NUM_MOD_SLOTS]; }
			if (_clipboard_ModBones_Common == null) { _clipboard_ModBones_Common = new apSnapShotStackUnit[NUM_MOD_SLOTS]; }
			
			for (int i = 0; i < NUM_MOD_SLOTS; i++)
			{	
				if (_clipboard_ModMeshes_Common[i] == null) { _clipboard_ModMeshes_Common[i] = new apSnapShotStackUnit(); }
				if (_clipboard_ModBones_Common[i] == null) { _clipboard_ModBones_Common[i] = new apSnapShotStackUnit(); }				
				_clipboard_ModMeshes_Common[i].Clear();
				_clipboard_ModBones_Common[i].Clear();
			}

			if (_clipboard_ModMeshes_KeyObjects == null) { _clipboard_ModMeshes_KeyObjects = new Dictionary<object, apSnapShotStackUnit[]>(); }
			_clipboard_ModMeshes_KeyObjects.Clear();

			if(_clipboard_ModBones_KeyObjects == null) { _clipboard_ModBones_KeyObjects = new Dictionary<object, apSnapShotStackUnit[]>(); }
			_clipboard_ModBones_KeyObjects.Clear();



			

			_snapShotList.Clear();
			//_curSnapShot = null;
			//_iCurSnapShot = -1;
			//_restoredSnapShot = false;

			//키프레임 복사하기
			_clipboard_AnimClipOfKeyframes = null;
			_clipboard_Keyframes = null;
			_copied_keyframes_StartFrame = -1;
			_copied_keyframes_EndFrame = -1;

			//추가 19.8.9 : Rigging의 Pos-Copy 기능용 코드
			//이건 별도로 처리한다.
			if(_clipboard_MultipleVertRig == null) { _clipboard_MultipleVertRig = new apSnapShot_MultipleVertRig(); }
			_clipboard_MultipleVertRig.Clear();

			if(_clipboard_MeshVertEdges == null) { _clipboard_MeshVertEdges = new apSnapShot_Mesh(); }
			_clipboard_MeshVertEdges.Clear();

			if(_clipboard_MeshPin == null) { _clipboard_MeshPin = new apSnapShot_MeshPin(); }
			_clipboard_MeshPin.Clear();
		}


		// Functions
		//-------------------------------------------

		// Copy / Paste
		//--------------------------------------------------------------------
		// 1. ModMesh
		//--------------------------------------------------------------------

		public void Copy_ModMesh_SingleTarget(apModifiedMesh modMesh, string snapShotName, int iSlot)
		{
			//변경 21.3.19 : 1개가 아닌 4개의 슬롯에 저장할 수 있다.
			//변경 21.6.24 : 단일 복사는 Common에만 저장한다.
			
			//1.공통에 저장하자
			if(_clipboard_ModMeshes_Common == null)
			{
				_clipboard_ModMeshes_Common = new apSnapShotStackUnit[NUM_MOD_SLOTS];
				for (int i = 0; i < NUM_MOD_SLOTS; i++)
				{
					_clipboard_ModMeshes_Common[i] = new apSnapShotStackUnit();
				}
			}

			
			if(_clipboard_ModMeshes_Common[iSlot] == null)
			{
				_clipboard_ModMeshes_Common[iSlot] = new apSnapShotStackUnit();
			}
			apSnapShotStackUnit curUnit = _clipboard_ModMeshes_Common[iSlot];
			curUnit.Clear();
			curUnit.SetName(snapShotName);
			
			bool result = curUnit.SetSnapShot_ModMesh(modMesh, "Clipboard");
			if (!result)
			{
				curUnit.Clear();//<<저장 불가능하다.
			}			
		}



		//추가 21.6.24 : 다중 복사를 하는 경우 (공통에는 저장하지 않는다.)
		public void Copy_ModMesh_MultipleTargets(List<apModifiedMesh> modMeshes, string snapShotName, int iSlot)
		{
			//변경 21.3.19 : 1개가 아닌 4개의 슬롯에 저장할 수 있다.
			//변경 21.6.24 : 다중 복사를 위해서 키 오브젝트에 저장한다. (공통에는 저장하지 않는다.)
			
			int nModMeshes = modMeshes != null ? modMeshes.Count : 0;
			if(nModMeshes == 0)
			{
				return;
			}

			//2. 키 오브젝트에 대해서 각각 저장한다. (MeshTF, MeshGroupTF, Bone)
			apModifiedMesh curModMesh = null;
			apSnapShotStackUnit curUnit = null;
			for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
			{
				curModMesh = modMeshes[iModMesh];

				//다중 복사를 위함
				object keyObj = null;
				if (curModMesh._transform_Mesh != null)
				{
					keyObj = curModMesh._transform_Mesh;
				}
				else if (curModMesh._transform_MeshGroup != null)
				{
					keyObj = curModMesh._transform_MeshGroup;
				}

				//키 오브젝트에 따른 값을 저장하자
				if (keyObj != null)
				{
					if (_clipboard_ModMeshes_KeyObjects == null)
					{
						_clipboard_ModMeshes_KeyObjects = new Dictionary<object, apSnapShotStackUnit[]>();
					}

					apSnapShotStackUnit[] targetUnits = null;

					if (!_clipboard_ModMeshes_KeyObjects.ContainsKey(keyObj))
					{
						targetUnits = new apSnapShotStackUnit[NUM_MOD_SLOTS];
						for (int i = 0; i < NUM_MOD_SLOTS; i++)
						{
							targetUnits[i] = new apSnapShotStackUnit();
						}
						_clipboard_ModMeshes_KeyObjects.Add(keyObj, targetUnits);
					}
					else
					{
						targetUnits = _clipboard_ModMeshes_KeyObjects[keyObj];//키 오브젝트에 대해 데이터를 저장하자
					}

					if (targetUnits != null)
					{
						curUnit = targetUnits[iSlot];

						curUnit.Clear();
						curUnit.SetName(snapShotName);

						bool result = curUnit.SetSnapShot_ModMesh(curModMesh, "Clipboard");
						if (!result)
						{
							curUnit.Clear();//<<저장 불가능하다.
						}
					}
				}
			}
		}







		//1개의 Slot을 복사하는 경우 (단일 붙여넣기, 다중 붙여넣기)
		//공통 데이터에서만 찾기
		//[단일 붙여넣기]
		public bool Paste_ModMesh_SingleSlot_SingleTarget(
												apModifiedMesh targetModMesh, int iSlot, bool isMorphMod,
												//추가 22.7.10 : 붙여넣기할 속성을 지정한다.
												bool isProp_Verts,
												bool isProp_Pins,
												bool isProp_Transform,
												bool isProp_Visibility,
												bool isProp_Color,
												bool isProp_Extra,
												bool isSelectedOnly,
												List<apModifiedVertex> selectedModVerts,
												List<apModifiedPin> selectedModPins)
		{
			if (targetModMesh == null
				//|| _clipboard_ModMeshes == null
				//|| _clipboard_ModMeshes[iSlot] == null
				)
			{
				return false;
			}

			
			//2. 단일 복사의 경우 공통 데이터에서만 찾는다.
			apSnapShotStackUnit targetUnit = null;
			if(_clipboard_ModMeshes_Common != null)
			{
				targetUnit = _clipboard_ModMeshes_Common[iSlot];
			}

			if(targetUnit == null)
			{
				//복사 불가
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = false;
			if(isMorphMod)
			{
				//isKeySync = _clipboard_ModMeshes[iSlot].IsKeySyncable_MorphMod(targetModMesh);
				isKeySync = targetUnit.IsKeySyncable_MorphMod(targetModMesh);
			}
			else
			{
				//isKeySync = _clipboard_ModMeshes[iSlot].IsKeySyncable_TFMod(targetModMesh);
				isKeySync = targetUnit.IsKeySyncable_TFMod(targetModMesh);
			}
			
			if (!isKeySync)
			{
				return false;
			}

			//return targetUnit.Load(targetModMesh);
			return targetUnit.LoadWithProperties(targetModMesh,
												isProp_Verts,
												isProp_Pins,
												isProp_Transform,
												isProp_Visibility,
												isProp_Color,
												isProp_Extra,
												isSelectedOnly,
												selectedModVerts,
												selectedModPins);
		}


		//다중 붙여넣기 + 단일 슬롯 (ModMesh)
		//키 오브젝트 데이터만 참조하여 붙여넣는다. (공통 데이터 참조하지 않음)
		public bool Paste_ModMeshes_SingleSlot_MultipleTargets(		List<apModifiedMesh> targetModMeshes, int iSlot, bool isMorphMod,
																	//추가 22.7.10 : 붙여넣기할 속성을 지정한다.
																	bool isProp_Verts,
																	bool isProp_Pins,
																	bool isProp_Transform,
																	bool isProp_Visibility,
																	bool isProp_Color,
																	bool isProp_Extra,
																	bool isSelectedOnly,
																	List<apModifiedVertex> selectedModVerts,
																	List<apModifiedPin> selectedModPins)
		{
			int nModMeshes = targetModMeshes != null ? targetModMeshes.Count : 0;

			if (nModMeshes == 0)
			{
				return false;
			}

			apModifiedMesh curModMesh = null;
			bool result = false;
			for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
			{
				curModMesh = targetModMeshes[iModMesh];

				object keyObj = null;
				if (curModMesh._transform_Mesh != null)
				{
					keyObj = curModMesh._transform_Mesh;
				}
				else if (curModMesh._transform_MeshGroup != null)
				{
					keyObj = curModMesh._transform_MeshGroup;
				}

				//1. 키 오브젝트에 대해서 데이터를 찾자
				apSnapShotStackUnit targetUnit = null;
				if (_clipboard_ModMeshes_KeyObjects.ContainsKey(keyObj))
				{
					targetUnit = (_clipboard_ModMeshes_KeyObjects[keyObj])[iSlot];
				}

				//2. 공통 데이터에서는 찾지 않는다.
				//if (targetUnit == null && !isMorphMod)
				//{
				//	if (_clipboard_ModMeshes_Common != null)
				//	{
				//		targetUnit = _clipboard_ModMeshes_Common[iSlot];
				//	}
				//}

				if (targetUnit == null)
				{
					//복사 불가
					continue;
				}

				//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
				bool isKeySync = false;
				if (isMorphMod)
				{
					isKeySync = targetUnit.IsKeySyncable_MorphMod(curModMesh);
				}
				else
				{
					isKeySync = targetUnit.IsKeySyncable_TFMod(curModMesh);
				}

				if (!isKeySync)
				{
					continue;
				}

				//result = targetUnit.Load(curModMesh);
				result = targetUnit.LoadWithProperties(	curModMesh,
														isProp_Verts,
														isProp_Pins,
														isProp_Transform,
														isProp_Visibility,
														isProp_Color,
														isProp_Extra,
														isSelectedOnly,
														selectedModVerts,
														selectedModPins);
			}

			return result;
			
			
		}




		//단일 붙여넣기 + 2개 이상의 Slot을 복사하는 경우
		public bool Paste_ModMesh_MultipleSlot_SingleTarget(	apModifiedMesh targetModMesh,
																bool isMorphMod, int iMainSlot, bool[] slots, int methodType,
																//추가 22.7.10 : 붙여넣기할 속성을 지정한다.
																bool isProp_Verts,
																bool isProp_Pins,
																bool isProp_Transform,
																bool isProp_Visibility,
																bool isProp_Color,
																bool isProp_Extra,
																bool isSelectedOnly,
																List<apModifiedVertex> selectedModVerts,
																List<apModifiedPin> selectedModPins)
		{
			if (targetModMesh == null
				//|| _clipboard_ModMeshes == null
				)
			{
				return false;
			}

			//단일 복사는 공통 데이터에서만 찾는다.
			apSnapShotStackUnit[] targetUnits = null;
			if(_clipboard_ModMeshes_Common != null)
			{
				targetUnits = _clipboard_ModMeshes_Common;
			}

			if(targetUnits == null)
			{
				//복사 불가
				return false;
			}

			List<apSnapShotStackUnit> pastableUnits = new List<apSnapShotStackUnit>();

			//슬롯을 하나씩 체크한다.
			apSnapShotStackUnit curUnit = null;
			for (int i = 0; i < NUM_MOD_SLOTS; i++)
			{
				if (i == iMainSlot || slots[i])
				{
					//curUnit = _clipboard_ModMeshes[i];
					curUnit = targetUnits[i];
					if (curUnit != null)
					{
						if(
							(isMorphMod && curUnit.IsKeySyncable_MorphMod(targetModMesh))
							|| (!isMorphMod && curUnit.IsKeySyncable_TFMod(targetModMesh))
							)
						{
							//다중 복사에 적용
							pastableUnits.Add(curUnit);
						}
					}
				}
			}

			if(pastableUnits.Count == 0)
			{
				//복사가 불가능하다.
				return false;
			}

			if(pastableUnits.Count == 1)
			{
				//1개라면 Single과 동일
				return pastableUnits[0].Load(targetModMesh);
			}

			//이제 다중 복사를 해보자
			//가상의 유닛을 만들고,
			//값을 누적시키자.
			//일단 전부 합한 후, Average 타입일때는 합한 개수만큼 나누면 된다.
			apSnapShot_ModifiedMesh tmpModMeshSnapShot = new apSnapShot_ModifiedMesh();
			tmpModMeshSnapShot.Clear();
			tmpModMeshSnapShot.ReadyToAddMultipleSnapShots(methodType == 0);
			
			//각각 더해지는 가중치는 연산 방식 0 : Sum, 1 : Average에 따라 다르다.
			float weight = methodType == 0 ? 1.0f : 1.0f / pastableUnits.Count;

			for (int i = 0; i < pastableUnits.Count; i++)
			{
				//값을 누적시킨다.
				tmpModMeshSnapShot.AddSnapShot(pastableUnits[i]._snapShot as apSnapShot_ModifiedMesh, weight, methodType == 0);
			}

			//누적된 값을 ModMesh에 적용
			//return tmpModMeshSnapShot.Load(targetModMesh);
			return tmpModMeshSnapShot.LoadWithProperties(	targetModMesh,
															isProp_Verts,
															isProp_Pins,
															isProp_Transform,
															isProp_Visibility,
															isProp_Color,
															isProp_Extra,
															isSelectedOnly,
															selectedModVerts,
															selectedModPins);
		}




		//다중 붙여넣기 + 다중 슬롯
		public bool Paste_ModMeshes_MultipleSlot_MultipleTargets(List<apModifiedMesh> targetModMeshes,
																bool isMorphMod, int iMainSlot, bool[] slots, int methodType,
																//추가 22.7.10 : 붙여넣기할 속성을 지정한다.
																bool isProp_Verts,
																bool isProp_Pins,
																bool isProp_Transform,
																bool isProp_Visibility,
																bool isProp_Color,
																bool isProp_Extra,
																bool isSelectedOnly,
																List<apModifiedVertex> selectedModVerts,
																List<apModifiedPin> selectedModPins)
		{
			int nModMeshes = targetModMeshes != null ? targetModMeshes.Count : 0;

			if (nModMeshes == 0)
			{
				return false;
			}

			bool result = false;
			apModifiedMesh curModMesh = null;
			for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
			{
				curModMesh = targetModMeshes[iModMesh];

				object keyObj = null;
				if (curModMesh._transform_Mesh != null)
				{
					keyObj = curModMesh._transform_Mesh;
				}
				else if (curModMesh._transform_MeshGroup != null)
				{
					keyObj = curModMesh._transform_MeshGroup;
				}

				//1. 키 오브젝트에 대해서 데이터를 찾자 (다중 붙여넣기에서는 공통 데이터를 참조하지 않는다.)
				apSnapShotStackUnit[] targetUnits = null;
				if (_clipboard_ModMeshes_KeyObjects.ContainsKey(keyObj))
				{
					targetUnits = _clipboard_ModMeshes_KeyObjects[keyObj];
				}
				if (targetUnits == null)
				{
					//복사 불가
					continue;
				}

				List<apSnapShotStackUnit> pastableUnits = new List<apSnapShotStackUnit>();


				//슬롯을 하나씩 체크한다.
				apSnapShotStackUnit curUnit = null;
				for (int i = 0; i < NUM_MOD_SLOTS; i++)
				{
					if (i == iMainSlot || slots[i])
					{
						//curUnit = _clipboard_ModMeshes[i];
						curUnit = targetUnits[i];
						if (curUnit != null)
						{
							if (
								(isMorphMod && curUnit.IsKeySyncable_MorphMod(curModMesh))
								|| (!isMorphMod && curUnit.IsKeySyncable_TFMod(curModMesh))
								)
							{
								//다중 복사에 적용
								pastableUnits.Add(curUnit);
							}
						}
					}
				}

				if (pastableUnits.Count == 0)
				{
					//복사가 불가능하다.
					continue;
				}

				if (pastableUnits.Count == 1)
				{
					//1개라면 Single과 동일
					result = pastableUnits[0].Load(curModMesh);
					continue;
				}

				//이제 다중 복사를 해보자
				//가상의 유닛을 만들고,
				//값을 누적시키자.
				//일단 전부 합한 후, Average 타입일때는 합한 개수만큼 나누면 된다.
				apSnapShot_ModifiedMesh tmpModMeshSnapShot = new apSnapShot_ModifiedMesh();
				tmpModMeshSnapShot.Clear();
				tmpModMeshSnapShot.ReadyToAddMultipleSnapShots(methodType == 0);

				//각각 더해지는 가중치는 연산 방식 0 : Sum, 1 : Average에 따라 다르다.
				float weight = methodType == 0 ? 1.0f : 1.0f / pastableUnits.Count;

				for (int i = 0; i < pastableUnits.Count; i++)
				{
					//값을 누적시킨다.
					tmpModMeshSnapShot.AddSnapShot(pastableUnits[i]._snapShot as apSnapShot_ModifiedMesh, weight, methodType == 0);
				}

				//누적된 값을 ModMesh에 적용
				//result = tmpModMeshSnapShot.Load(curModMesh);
				result = tmpModMeshSnapShot.LoadWithProperties(curModMesh,
															isProp_Verts,
															isProp_Pins,
															isProp_Transform,
															isProp_Visibility,
															isProp_Color,
															isProp_Extra,
															isSelectedOnly,
															selectedModVerts,
															selectedModPins);

			}
			
			return result;
		}







		//삭제 21.6.24 : 버튼 툴팁용
		//public string GetClipboardName_ModMesh(int iSlot)
		//{
		//	if (_clipboard_ModMeshes == null
		//		|| _clipboard_ModMeshes[iSlot] == null
		//		|| !_clipboard_ModMeshes[iSlot]._isDataSaved)
		//	{
		//		return null;
		//	}

		//	return _clipboard_ModMeshes[iSlot].Name;
		//}

		
		public bool IsPastable_TF_SingleTarget(apModifiedMesh targetModMesh, int iSlot)
		{
			if (targetModMesh == null
				|| _clipboard_ModMeshes_Common == null
				|| _clipboard_ModMeshes_Common[iSlot] == null
				)
			{
				return false;
			}

			//단일 대상은 공통 데이터에서만 확인한다.
			return _clipboard_ModMeshes_Common[iSlot].IsKeySyncable_TFMod(targetModMesh);
		}

		public bool IsPastable_TF_MultipleTargets(List<apModifiedMesh> targetModMeshes, int iSlot)
		{
			int nModMeshes = targetModMeshes != null ? targetModMeshes.Count : 0;

			if (nModMeshes == 0)
			{
				return false;
			}

			//다중 대상은 키 오브젝트 데이터에서만 확인한다.
			//하나라도 복사 가능하면 true를 리턴한다.
			apModifiedMesh curModMesh = null;
			for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
			{
				curModMesh = targetModMeshes[iModMesh];
				//키 오브젝트에 데이터가 있는지 먼저 확인한다.
				//TF의 경우, 공통 데이터에서 찾을 수도 있다. (단일 선택인 경우에만)
				object keyObj = null;
				if (curModMesh._transform_Mesh != null)
				{
					keyObj = curModMesh._transform_Mesh;
				}
				else if (curModMesh._transform_MeshGroup != null)
				{
					keyObj = curModMesh._transform_MeshGroup;
				}
				if (keyObj == null)
				{
					continue;
				}

				//1. 일단 키 오브젝트에 대해서 데이터를 찾자
				apSnapShotStackUnit targetUnit = null;
				if (_clipboard_ModMeshes_KeyObjects.ContainsKey(keyObj))
				{
					targetUnit = (_clipboard_ModMeshes_KeyObjects[keyObj])[iSlot];
				}

				if (targetUnit != null)
				{
					if(targetUnit.IsKeySyncable_TFMod(curModMesh))
					{
						//복사 가능하면 true 리턴
						return true;
					}
				}
			}
			
			return false;
		}




		public bool IsPastable_Morph_SingleTarget(apModifiedMesh targetModMesh, int iSlot)
		{
			//공통 데이터에서만 참고한다.
			if (targetModMesh == null
				|| _clipboard_ModMeshes_Common == null
				|| _clipboard_ModMeshes_Common[iSlot] == null
				)
			{
				return false;
			}
			
			//단일 대상은 공통 데이터에서만 확인한다.
			return _clipboard_ModMeshes_Common[iSlot].IsKeySyncable_MorphMod(targetModMesh);
		}


		public bool IsPastable_Morph_MultipleTargets(List<apModifiedMesh> targetModMeshes, int iSlot)
		{
			int nModMeshes = targetModMeshes != null ? targetModMeshes.Count : 0;

			if (nModMeshes == 0)
			{
				return false;
			}

			//다중 대상은 키 오브젝트 데이터에서만 확인한다.
			//하나라도 복사 가능하면 true를 리턴한다.
			apModifiedMesh curModMesh = null;
			for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
			{
				curModMesh = targetModMeshes[iModMesh];

				//키 오브젝트에 데이터가 있는지 먼저 확인한다.
				//Morph의 경우 공통 데이터는 참조하지 않는다.
				object keyObj = null;
				if (curModMesh._transform_Mesh != null)
				{
					keyObj = curModMesh._transform_Mesh;
				}
				else if (curModMesh._transform_MeshGroup != null)
				{
					keyObj = curModMesh._transform_MeshGroup;
				}
				if(keyObj == null)
				{
					return false;
				}

				//키 오브젝트에 대해서 데이터를 찾자 (공통 데이터에서는 체크하지 않는다.)
				apSnapShotStackUnit targetUnit = null;
				if (_clipboard_ModMeshes_KeyObjects.ContainsKey(keyObj))
				{
					targetUnit = (_clipboard_ModMeshes_KeyObjects[keyObj])[iSlot];
				}
				if(targetUnit != null)
				{
					if(targetUnit.IsKeySyncable_MorphMod(curModMesh))
					{
						//하나라도 붙여넣을게 있다면 true
						return true;
					}
				}
			}

			return false;
			
		}


		//--------------------------------------------------------------------
		// 1-2. ModBone
		//--------------------------------------------------------------------
		public void Copy_ModBone_SingleTarget(apModifiedBone modBone, string snapShotName, int iSlot)
		{
			//변경 21.6.24 : 다중 복사를 위해서 [공통 데이터]에 저장한다.
			if(_clipboard_ModBones_Common == null)
			{
				_clipboard_ModBones_Common = new apSnapShotStackUnit[NUM_MOD_SLOTS];
				for (int i = 0; i < NUM_MOD_SLOTS; i++)
				{
					_clipboard_ModBones_Common[i] = new apSnapShotStackUnit();
				}
			}
			if(_clipboard_ModBones_Common[iSlot] == null)
			{
				_clipboard_ModBones_Common[iSlot] = new apSnapShotStackUnit();
			}

			apSnapShotStackUnit curUnit = _clipboard_ModBones_Common[iSlot];
			curUnit.Clear();
			curUnit.SetName(snapShotName);

			bool result = curUnit.SetSnapShot_ModBone(modBone, "Clipboard");
			if (!result)
			{
				curUnit.Clear();//<<저장 불가능하다.
			}
		}


		//추가 21.6.24 : 여러개의 본들을 복사할 때 (공통에는 저장하지 않는다.)
		public void Copy_ModBones_MultipleTargets(List<apModifiedBone> modBones, string snapShotName, int iSlot)
		{
			int nModBones = modBones != null ? modBones.Count : 0;
			if(nModBones == 0)
			{
				return;
			}

			apModifiedBone curModBone = null;
			apSnapShotStackUnit curUnit = null;

			//2. [키 오브젝트]에 대해서 저장한다. (MeshTF, MeshGroupTF, Bone)
			for (int iModBone = 0; iModBone < nModBones; iModBone++)
			{
				curModBone = modBones[iModBone];

				object keyObj = null;
				if (curModBone._bone != null)
				{
					keyObj = curModBone._bone;
				}

				//키 오브젝트에 따른 값을 저장하자
				if (keyObj != null)
				{
					if (_clipboard_ModBones_KeyObjects == null)
					{
						_clipboard_ModBones_KeyObjects = new Dictionary<object, apSnapShotStackUnit[]>();
					}

					apSnapShotStackUnit[] targetUnits = null;

					if (!_clipboard_ModBones_KeyObjects.ContainsKey(keyObj))
					{
						targetUnits = new apSnapShotStackUnit[NUM_MOD_SLOTS];
						for (int i = 0; i < NUM_MOD_SLOTS; i++)
						{
							targetUnits[i] = new apSnapShotStackUnit();
						}
						_clipboard_ModBones_KeyObjects.Add(keyObj, targetUnits);
					}
					else
					{
						targetUnits = _clipboard_ModBones_KeyObjects[keyObj];//키 오브젝트에 대해 데이터를 저장하자
					}

					if (targetUnits != null)
					{
						curUnit = targetUnits[iSlot];

						curUnit.Clear();
						curUnit.SetName(snapShotName);

						bool result = curUnit.SetSnapShot_ModBone(curModBone, "Clipboard");
						if (!result)
						{
							curUnit.Clear();//<<저장 불가능하다.
						}
					}
				}
			}
		}



		//1개의 Slot을 복사하는 경우 (단일 붙여넣기, 다중 붙여넣기)		
		//[단일 붙여넣기 + 단일 슬롯]
		public bool Paste_ModBone_SingleSlot_SingleTarget(apModifiedBone targetModBone, int iSlot)
		{
			if (targetModBone == null
				//|| _clipboard_ModBones == null
				//|| _clipboard_ModBones[iSlot] == null
				)
			{
				return false;
			}

			//공통 데이터에서만 찾는다.
			//1. 일단 키 오브젝트에 대해서 데이터를 찾자
			apSnapShotStackUnit targetUnit = null;
			if(_clipboard_ModBones_Common != null)
			{
				targetUnit = _clipboard_ModBones_Common[iSlot];
			}

			if(targetUnit == null)
			{
				//복사 불가
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			//bool isKeySync = _clipboard_ModBones[iSlot].IsKeySyncable_TFMod(targetModBone);
			bool isKeySync = targetUnit.IsKeySyncable_TFMod(targetModBone);

			if (!isKeySync)
			{
				return false;
			}

			//return _clipboard_ModBones[iSlot].Load(targetModBone);
			return targetUnit.Load(targetModBone);
		}



		//다중 붙여넣기 + 단일 슬롯
		public bool Paste_ModBones_SingleSlot_MultipleTargets(List<apModifiedBone> targetModBones, int iSlot)
		{
			int nModBones = targetModBones != null ? targetModBones.Count : 0;

			if (nModBones == 0)
			{
				return false;
			}

			//키-오브젝트 데이터에서만 찾는다.
			bool result = false;
			apModifiedBone curModBone = null;
			for (int iModBone = 0; iModBone < nModBones; iModBone++)
			{
				curModBone = targetModBones[iModBone];

				object keyObj = null;
				if (curModBone._bone != null)
				{
					keyObj = curModBone._bone;
				}

				//1. 키 오브젝트에 대해서 데이터를 찾자 (공통 데이터에서는 찾지 않는다.)
				apSnapShotStackUnit targetUnit = null;
				if (_clipboard_ModBones_KeyObjects.ContainsKey(keyObj))
				{
					targetUnit = (_clipboard_ModBones_KeyObjects[keyObj])[iSlot];
				}

				if (targetUnit == null)
				{
					//복사 불가
					continue;
				}

				//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
				bool isKeySync = targetUnit.IsKeySyncable_TFMod(curModBone);

				if (!isKeySync)
				{
					continue;
				}

				result = targetUnit.Load(curModBone);
			}

			return result;
		}






		//단일 붙여넣기 + 다중 슬롯
		public bool Paste_ModBone_MultipleSlot_SingleTarget(apModifiedBone targetModBone, int iMainSlot, bool[] slots, int methodType)
		{
			if (targetModBone == null
				//|| _clipboard_ModBones == null
				)
			{
				return false;
			}

			//공통 데이터에서만 찾는다.			
			//1. 일단 키 오브젝트에 대해서 데이터를 찾자
			apSnapShotStackUnit[] targetUnits = null;
			if(_clipboard_ModBones_Common != null)
			{
				targetUnits = _clipboard_ModBones_Common;
			}

			if(targetUnits == null)
			{
				//복사 불가
				return false;
			}

			List<apSnapShotStackUnit> pastableUnits = new List<apSnapShotStackUnit>();

			//슬롯을 하나씩 체크한다.
			apSnapShotStackUnit curUnit = null;
			for (int i = 0; i < NUM_MOD_SLOTS; i++)
			{
				if (i == iMainSlot || slots[i])
				{
					//curUnit = _clipboard_ModBones[i];
					curUnit = targetUnits[i];
					if (curUnit != null)
					{
						if(curUnit.IsKeySyncable_TFMod(targetModBone))
						{
							//다중 복사에 적용
							pastableUnits.Add(curUnit);
						}
					}
				}
			}

			if(pastableUnits.Count == 0)
			{
				//복사가 불가능하다.
				return false;
			}

			if(pastableUnits.Count == 1)
			{
				//1개라면 Single과 동일
				return pastableUnits[0].Load(targetModBone);
			}

			//이제 다중 복사를 해보자
			//가상의 유닛을 만들고,
			//값을 누적시키자.
			//일단 전부 합한 후, Average 타입일때는 합한 개수만큼 나누면 된다.
			apSnapShot_ModifiedBone tmpModBoneSnapShot = new apSnapShot_ModifiedBone();
			tmpModBoneSnapShot.Clear();
			tmpModBoneSnapShot.ReadyToAddMultipleSnapShots(methodType == 0);
			
			//각각 더해지는 가중치는 연산 방식 0 : Sum, 1 : Average에 따라 다르다.
			float weight = methodType == 0 ? 1.0f : 1.0f / pastableUnits.Count;

			for (int i = 0; i < pastableUnits.Count; i++)
			{
				//값을 누적시킨다.
				tmpModBoneSnapShot.AddSnapShot(pastableUnits[i]._snapShot as apSnapShot_ModifiedBone, weight, methodType == 0);
			}

			//누적된 값을 ModMesh에 적용
			return tmpModBoneSnapShot.Load(targetModBone);
		}




		//다중 붙여넣기 + 다중 슬롯
		public bool Paste_ModBones_MultipleSlot_MultipleTargets(List<apModifiedBone> targetModBones, int iMainSlot, bool[] slots, int methodType)
		{
			int nModBones = targetModBones != null ? targetModBones.Count : 0;

			if (nModBones == 0)
			{
				return false;
			}

			//키-오브젝트 데이터에서만 찾는다.
			apModifiedBone curModBone = null;
			bool result = false;

			for (int iModBone = 0; iModBone < nModBones; iModBone++)
			{
				curModBone = targetModBones[iModBone];

				object keyObj = null;
				if (curModBone._bone != null)
				{
					keyObj = curModBone._bone;
				}

				//1. 키 오브젝트에 대해서 데이터를 찾자 (다중 붙여넣기에서는 공통 데이터를 참조하지 않는다.)
				apSnapShotStackUnit[] targetUnits = null;
				if (_clipboard_ModBones_KeyObjects.ContainsKey(keyObj))
				{
					targetUnits = _clipboard_ModBones_KeyObjects[keyObj];
				}

				if (targetUnits == null)
				{
					//복사 불가
					continue;
				}


				List<apSnapShotStackUnit> pastableUnits = new List<apSnapShotStackUnit>();

				//슬롯을 하나씩 체크한다.
				apSnapShotStackUnit curUnit = null;
				for (int i = 0; i < NUM_MOD_SLOTS; i++)
				{
					if (i == iMainSlot || slots[i])
					{
						//curUnit = _clipboard_ModBones[i];
						curUnit = targetUnits[i];
						if (curUnit != null)
						{
							if (curUnit.IsKeySyncable_TFMod(curModBone))
							{
								//다중 복사에 적용
								pastableUnits.Add(curUnit);
							}
						}
					}
				}

				if (pastableUnits.Count == 0)
				{
					//복사가 불가능하다.
					continue;
				}

				if (pastableUnits.Count == 1)
				{
					//1개라면 Single과 동일
					result = pastableUnits[0].Load(curModBone);
					continue;
				}

				//이제 다중 복사를 해보자
				//가상의 유닛을 만들고,
				//값을 누적시키자.
				//일단 전부 합한 후, Average 타입일때는 합한 개수만큼 나누면 된다.
				apSnapShot_ModifiedBone tmpModBoneSnapShot = new apSnapShot_ModifiedBone();
				tmpModBoneSnapShot.Clear();
				tmpModBoneSnapShot.ReadyToAddMultipleSnapShots(methodType == 0);

				//각각 더해지는 가중치는 연산 방식 0 : Sum, 1 : Average에 따라 다르다.
				float weight = methodType == 0 ? 1.0f : 1.0f / pastableUnits.Count;

				for (int i = 0; i < pastableUnits.Count; i++)
				{
					//값을 누적시킨다.
					tmpModBoneSnapShot.AddSnapShot(pastableUnits[i]._snapShot as apSnapShot_ModifiedBone, weight, methodType == 0);
				}

				//누적된 값을 ModMesh에 적용
				result = tmpModBoneSnapShot.Load(curModBone);
			}

			return result;
		}







		//삭제 21.6.24 : 버튼 툴팁용 함수. 다중 선택때문에 처리가 애매해져서 삭제
		//public string GetClipboardName_ModBone(int iSlot)
		//{
		//	if (_clipboard_ModBones == null
		//		|| _clipboard_ModBones[iSlot] == null
		//		|| !_clipboard_ModBones[iSlot]._isDataSaved)
		//	{
		//		return null;
		//	}
		//	return _clipboard_ModBones[iSlot].Name;
		//}

		public bool IsPastable_SingleModBone(apModifiedBone targetModBone, int iSlot)
		{
			//공통 데이터에서만 찾자
			if (targetModBone == null
				|| _clipboard_ModBones_Common == null
				|| _clipboard_ModBones_Common[iSlot] == null
				)
			{
				return false;
			}

			return _clipboard_ModBones_Common[iSlot].IsKeySyncable_TFMod(targetModBone);
		}


		public bool IsPastable_MultipleModBones(List<apModifiedBone> targetModBones, int iSlot)
		{
			int nModBones = targetModBones != null ? targetModBones.Count : 0;
			

			
			if (nModBones == 0)
			{
				return false;
			}

			apModifiedBone curModBone = null;
			for (int iModBone = 0; iModBone < nModBones; iModBone++)
			{
				curModBone = targetModBones[iModBone];

				//키 오브젝트 데이터에서 하나라도 붙여넣기 가능하면 true 리턴
				object keyObj = null;
				if(curModBone._bone != null)
				{
					keyObj = curModBone._bone;
				}
				if(keyObj == null)
				{
					continue;
				}

				//키 오브젝트에 대한 데이터를 먼저 참조하고, 단일 선택인 경우에 공통 데이터에서 한번 더 체크하자
				apSnapShotStackUnit targetUnit = null;
				if (_clipboard_ModBones_KeyObjects.ContainsKey(keyObj))
				{
					targetUnit = (_clipboard_ModBones_KeyObjects[keyObj])[iSlot];
				}

				if(targetUnit != null)
				{
					if(targetUnit.IsKeySyncable_TFMod(curModBone))
					{
						return true;
					}
				}
			}

			return false;
			
		}

		//--------------------------------------------------------------------
		// 2. Keyframe
		//--------------------------------------------------------------------
		public void Copy_Keyframe(apAnimKeyframe keyframe, string snapShotName)
		{
			if(_clipboard_Keyframe == null)
			{
				_clipboard_Keyframe = new apSnapShotStackUnit();
			}
			_clipboard_Keyframe.Clear();
			_clipboard_Keyframe.SetName(snapShotName);
			bool result = _clipboard_Keyframe.SetSnapShot_Keyframe(keyframe, "Clipboard");
			if (!result)
			{
				_clipboard_Keyframe = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_Keyframe(apAnimKeyframe targetKeyframe)
		{
			if (targetKeyframe == null
				|| _clipboard_Keyframe == null)
			{
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_Keyframe.IsKeySyncable(targetKeyframe);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_Keyframe.Load(targetKeyframe);
		}

		public string GetClipboardName_Keyframe()
		{
			if (_clipboard_Keyframe == null
				|| !_clipboard_Keyframe._isDataSaved)
			{
				return "";
			}
			return _clipboard_Keyframe.Name;
		}

		public bool IsPastable(apAnimKeyframe keyframe)
		{
			if (keyframe == null
				|| _clipboard_Keyframe == null)
			{
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_Keyframe.IsKeySyncable(keyframe);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		// 2.5. Keyframe 여러개 복사하기
		//--------------------------------------------------------------------
		//추가 3.29 : 타임라인 UI에서 키프레임들을 Ctrl+C로 복사하기
		public void Copy_KeyframesOnTimelineUI(apAnimClip animClip, List<apAnimKeyframe> keyframes)
		{
			if (animClip == null || keyframes == null || keyframes.Count == 0)
			{
				_clipboard_AnimClipOfKeyframes = null;
				_clipboard_Keyframes = null;
				_copied_keyframes_StartFrame = -1;
				_copied_keyframes_EndFrame = -1;
				return;
			}


			_clipboard_AnimClipOfKeyframes = animClip;
			if(_clipboard_Keyframes == null)
			{
				_clipboard_Keyframes = new List<apSnapShotStackUnit>();
			}
			else
			{
				_clipboard_Keyframes.Clear();
			}
			
			_copied_keyframes_StartFrame = -1;
			_copied_keyframes_EndFrame = -1;

			apAnimKeyframe srcKeyframe = null;
			for (int i = 0; i < keyframes.Count; i++)
			{
				srcKeyframe = keyframes[i];

				apSnapShotStackUnit newUnit = new apSnapShotStackUnit();
				newUnit.Clear();
				newUnit.SetName("Keyframe");
				newUnit.SetSnapShot_Keyframe(srcKeyframe, "Clipboard");
				_clipboard_Keyframes.Add(newUnit);

				if(i == 0)
				{
					_copied_keyframes_StartFrame = srcKeyframe._frameIndex;
					_copied_keyframes_EndFrame = srcKeyframe._frameIndex;
				}
				else
				{
					_copied_keyframes_StartFrame = Mathf.Min(_copied_keyframes_StartFrame, srcKeyframe._frameIndex);
					_copied_keyframes_EndFrame = Mathf.Max(_copied_keyframes_EndFrame, srcKeyframe._frameIndex);
				}
			}
		}

		public bool IsKeyframesPastableOnTimelineUI(apAnimClip animClip)
		{
			if(_clipboard_AnimClipOfKeyframes != null 
				&& animClip != null
				&& _clipboard_AnimClipOfKeyframes == animClip
				&& _clipboard_Keyframes != null
				&& _clipboard_Keyframes.Count > 0
				)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// 다른 AnimClip으로 복사할 수 있는지 확인한다.
		/// </summary>
		/// <param name="animClip"></param>
		/// <returns></returns>
		public bool IsKeyframesPastableOnTimelineUI_ToOtherAnimClip(apAnimClip animClip)
		{
			if(_clipboard_AnimClipOfKeyframes != null 
				&& animClip != null
				&& animClip._targetMeshGroup != null
				&& _clipboard_AnimClipOfKeyframes._targetMeshGroup != null
				&& animClip._targetMeshGroup == _clipboard_AnimClipOfKeyframes._targetMeshGroup//최소한 MeshGroup은 같아야 한다.
				//&& _clipboard_AnimClipOfKeyframes == animClip
				&& _clipboard_Keyframes != null
				&& _clipboard_Keyframes.Count > 0
				)
			{
				return true;
			}
			return false;
		}

		public List<apSnapShotStackUnit> GetKeyframesOnTimelineUI()
		{
			return _clipboard_Keyframes;
		}

		public int StartFrameOfKeyframesOnTimelineUI
		{
			get
			{
				return _copied_keyframes_StartFrame;
			}
		}

		//--------------------------------------------------------------------
		// 3. Vertex Rigging
		//--------------------------------------------------------------------
		public void Copy_VertRig(apModifiedVertexRig modVertRig, string snapShotName)
		{
			if(_clipboard_VertRig == null)
			{
				_clipboard_VertRig = new apSnapShotStackUnit();
			}
			_clipboard_VertRig.Clear();
			_clipboard_VertRig.SetName(snapShotName);
			bool result = _clipboard_VertRig.SetSnapShot_VertRig(modVertRig, "Clipboard");
			if (!result)
			{
				_clipboard_VertRig = null;//<<저장 불가능하다.
			}
		}

		public bool Paste_VertRig(apModifiedVertexRig targetModVertRig)
		{
			if (targetModVertRig == null
				|| _clipboard_VertRig == null)
			{
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_VertRig.IsKeySyncable(targetModVertRig);
			if (!isKeySync)
			{
				return false;
			}

			return _clipboard_VertRig.Load(targetModVertRig);
		}

		public bool IsPastable(apModifiedVertexRig vertRig)
		{
			if (vertRig == null
				|| _clipboard_VertRig == null)
			{
				return false;
			}

			//만약, 복사-붙여넣기 불가능한 객체이면 생략한다.
			bool isKeySync = _clipboard_VertRig.IsKeySyncable(vertRig);
			if (!isKeySync)
			{
				return false;
			}
			return true;
		}


		//--------------------------------------------------------------------
		//3-2. Rigging의 Pos-Cppy 기능
		//--------------------------------------------------------------------
		public bool IsRiggingPosPastable(apMeshGroup keyMeshGroup, List<apSelection.ModRenderVert> modRenderVerts)
		{
			if(_clipboard_MultipleVertRig == null)
			{
				return false;
			}
			if(modRenderVerts == null || modRenderVerts.Count == 0)
			{
				return false;
			}
			return _clipboard_MultipleVertRig.IsPastable(keyMeshGroup);
		}

		public void Copy_MultipleVertRig(apMeshGroup keyMeshGroup, List<apSelection.ModRenderVert> modRenderVerts)
		{
			if(_clipboard_MultipleVertRig == null)
			{
				_clipboard_MultipleVertRig = new apSnapShot_MultipleVertRig();
			}

			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();

			apSelection.ModRenderVert curModRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				curModRenderVert = modRenderVerts[i];
				if(curModRenderVert != null && curModRenderVert._modVertRig != null)
				{
					vertRigs.Add(curModRenderVert._modVertRig);
				}
			}

			_clipboard_MultipleVertRig.Copy(keyMeshGroup, vertRigs);
		}

		public bool Paste_MultipleVertRig(apMeshGroup keyMeshGroup, List<apSelection.ModRenderVert> modRenderVerts)
		{
			bool isPastable = IsRiggingPosPastable(keyMeshGroup, modRenderVerts);
			if(!isPastable)
			{
				return false;
			}
			
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();

			apSelection.ModRenderVert curModRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				curModRenderVert = modRenderVerts[i];
				if(curModRenderVert != null && curModRenderVert._modVertRig != null)
				{
					vertRigs.Add(curModRenderVert._modVertRig);
				}
			}
			if(vertRigs.Count == 0)
			{
				return false;
			}

			return _clipboard_MultipleVertRig.Paste(keyMeshGroup, vertRigs);
		}



		//-------------------------------------------------------------------
		// Mesh Vertex/Edge 복사 (21.10.6)
		//-------------------------------------------------------------------
		public void Copy_MeshVertices(apMesh mesh, List<apVertex> selectedVertices)
		{
			if(_clipboard_MeshVertEdges == null)
			{
				_clipboard_MeshVertEdges = new apSnapShot_Mesh();
			}

			_clipboard_MeshVertEdges.Copy(selectedVertices, mesh);
		}

		public List<apVertex> Paste_MeshVertices(apMesh targetMesh, apDialog_CopyMeshVertPin.POSITION_SPACE posSpace)
		{
			if(_clipboard_MeshVertEdges == null)
			{
				return null;
			}

			if(targetMesh == null
				|| targetMesh._textureData_Linked == null
				|| targetMesh._textureData_Linked._image == null)
			{
				return null;
			}

			if(!_clipboard_MeshVertEdges.IsPastable())
			{
				return null;
			}

			return _clipboard_MeshVertEdges.Paste(targetMesh, posSpace);

		}

		public bool IsPastable_MeshVertices()
		{
			if(_clipboard_MeshVertEdges == null)
			{
				return false;
			}
			if(!_clipboard_MeshVertEdges.IsPastable())
			{
				return false;
			}
			return true;
		}



		//-------------------------------------------------------------------
		// Mesh의 Pin복사 (v1.4.2)
		//-------------------------------------------------------------------
		public void Copy_MeshPins(apMesh mesh, List<apMeshPin> selectedPins)
		{
			if(_clipboard_MeshPin == null)
			{
				_clipboard_MeshPin = new apSnapShot_MeshPin();
			}

			_clipboard_MeshPin.Copy(selectedPins, mesh);
		}

		public List<apMeshPin> Paste_MeshPins(apMesh targetMesh, apDialog_CopyMeshVertPin.POSITION_SPACE posSpace)
		{
			if(_clipboard_MeshPin == null)
			{
				return null;
			}

			if(targetMesh == null
				|| targetMesh._textureData_Linked == null
				|| targetMesh._textureData_Linked._image == null)
			{
				return null;
			}

			if(!_clipboard_MeshPin.IsPastable())
			{
				return null;
			}

			return _clipboard_MeshPin.Paste(targetMesh, posSpace);

		}

		public bool IsPastable_MeshPins()
		{
			if(_clipboard_MeshPin == null)
			{
				return false;
			}
			if(!_clipboard_MeshPin.IsPastable())
			{
				return false;
			}
			return true;
		}

		// Save / Load
		//--------------------------------------------------------------------




		// Get / Set
		//--------------------------------------------
	}
}