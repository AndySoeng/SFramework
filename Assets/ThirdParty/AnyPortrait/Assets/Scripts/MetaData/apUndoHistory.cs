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

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 추가 21.6.26 : AnyPortrait 에디터에서 Undo 기록을 저장하기 위한 더미 게임 오브젝트.
	/// Serialize의 Undo 특성을 이용한다.
	/// HideFlags를 이용해서 저장이 안되도록 만든다. (그럼 Undo 영향을 받긴 하나)
	/// </summary>
	public class apUndoHistory : MonoBehaviour
	{
		// Static
		//-------------------------------------------
		private static apUndoHistory s_instance = null;
		public static apUndoHistory I
		{
			get
			{
				if(s_instance == null)
				{
					//새로운 게임 오브젝트를 만든다.
					MakeNewGameObject();
				}

				return s_instance;
			}
		}

		private static List<apUndoHistory> s_destroyableGameObjs = null;
		private static void RequestDestroy(apUndoHistory undoObj)
		{
			if(undoObj == s_instance || undoObj == null)
			{
				return;
			}
			if(s_destroyableGameObjs == null)
			{
				s_destroyableGameObjs = new List<apUndoHistory>();				
			}
			if(!s_destroyableGameObjs.Contains(undoObj))
			{
				s_destroyableGameObjs.Add(undoObj);
			}
		}

		
		public static void MakeNewGameObject()
		{
			//기존의 오브젝트 삭제
			//여기서 삭제 안해도 알아서 없어진다.
			if(s_instance != null
				&& s_instance.gameObject != null)
			{
				DestroyImmediate(s_instance.gameObject);
			}
			s_instance = null;

			//요청받은 객체도 삭제
			if(s_destroyableGameObjs != null)
			{
				apUndoHistory curDestroyableUndoObj = null;
				for (int i = 0; i < s_destroyableGameObjs.Count; i++)
				{
					curDestroyableUndoObj = s_destroyableGameObjs[i];
					if(curDestroyableUndoObj != null
						&& curDestroyableUndoObj.gameObject != null)
					{
						DestroyImmediate(curDestroyableUndoObj.gameObject);
					}
				}
				s_destroyableGameObjs = null;
			}

			
			//새로운 게임 오브젝트를 만든다.
			GameObject newGameObject = new GameObject("__AnyPortrait_UndoHistory");
			newGameObject.transform.position = Vector3.zero;

			//실제 : 아예 숨기는 버전
			newGameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;

			//테스트 : 저장은 안되지만 일단 Inspector에서 볼 수 있다.
			//newGameObject.hideFlags = HideFlags.DontSave;

			s_instance = newGameObject.AddComponent<apUndoHistory>();
			s_instance.InitRecord();
		}


		// Members
		//-------------------------------------------
		[Serializable]
		public class SnapshotRecord
		{
			[SerializeField]
			public bool _isDataSaved = false;//추가 21.7.17 : 데이터가 저장되었다면 true

			[SerializeField]
			public int _undoID = -1;

			[SerializeField]
			public string _undoName = null;

			[SerializeField]
			public bool _isAnyAddedOrRemoved = false;//이게 true면, 해당 Undo

			[SerializeField]
			public int _createdCounter = -1;

			//이전 : 데이터를 매번 생성
			//public SnapshotRecord(int undoID, string undoName, bool isAnyAddedOrRemoved, int createdCounter)
			//{
			//	_undoID = undoID;
			//	_undoName = undoName;
			//	_isAnyAddedOrRemoved = isAnyAddedOrRemoved;
			//	_createdCounter = createdCounter;
			//}

			//변경 7.17 : 값을 갱신 (리스트 사용 안함)
			public SnapshotRecord()
			{
				Init();
			}

			public void Init()
			{
				_isDataSaved = false;
				_undoID = -1;
				_undoName = null;
				_isAnyAddedOrRemoved = false;
				_createdCounter = -1;
			}

			public void SetData(int undoID, string undoName, bool isAnyAddedOrRemoved, int createdCounter)
			{
				_isDataSaved = true;
				_undoID = undoID;
				_undoName = undoName;
				_isAnyAddedOrRemoved = isAnyAddedOrRemoved;
				_createdCounter = createdCounter;
			}
		}

		//이전 : 리스트 타입의 데이터
		//[SerializeField]
		//public List<SnapshotRecord> _records = new List<SnapshotRecord>();

		//변경 21.7.17 : 단일 데이터
		[SerializeField]
		public SnapshotRecord _record = new SnapshotRecord();


		//private const int MAX_RECORDS = 5000;
		//private const int MAX_RECORDS = 5;

		private const int MAX_CREATED_COUNTER = 10000;
		//새로운 오브젝트가 추가될 때 마다 "생성 카운터"를 올린다.
		//Undo후 "복구된 상태"의 "생성 카운터"와 "마지막 생성 카운터(Serialize 아님)"가 다르면
		//리셋을 해야한다.
		[NonSerialized]
		private int _lastCreatedCounter = 0;


		public enum UNDO_RESULT
		{
			/// <summary>데이터의 값만 바뀌었다.</summary>
			DataChanged,
			/// <summary>구조가 바뀌었다.</summary>
			StructChanged,
		}

		// Init
		//-------------------------------------------
		void Start()
		{
			this.enabled = false;
		}

		// Update (Not Work)
		//-------------------------------------------
		void Update() { }

		

		private void OnValidate()
		{
			//스크립트 빌드 직후에 만약 객체가 남아있다면 삭제를 요청한다. (여기서 바로 삭제는 안되고, 에디터를 열때 일괄 삭제하도록 만든다.)
			//Debug.LogWarning("OnValidate");
			if(s_instance != this)
			{
				//삭제 요청
				RequestDestroy(this);
			}
		}
		

		

		// Functions
		//-------------------------------------------
		public void InitRecord()
		{
			//이전
			//if(_records == null)
			//{
			//	_records = new List<SnapshotRecord>();
			//}
			//_records.Clear();

			if(_record == null)
			{
				_record = new SnapshotRecord();
			}
			else
			{
				_record.Init();
			}
		}

		public void AddRecord(int undoID, string undoName, bool isAnyAddedOrRemoved)
		{	
			if(isAnyAddedOrRemoved)
			{
				//카운터 증가
				_lastCreatedCounter += 1;
				if(_lastCreatedCounter > MAX_CREATED_COUNTER)
				{
					_lastCreatedCounter = 0;
				}
			}
			Undo.RecordObject(this, undoName);

			//이전
			//SnapshotRecord newRecord = new SnapshotRecord(undoID, undoName, isAnyAddedOrRemoved, _lastCreatedCounter);
			//_records.Add(newRecord);

			//if(_records.Count > MAX_RECORDS)
			//{
			//	//Debug.Log("Record 개수 오버");
			//	_records.RemoveAt(0);
			//}

			//Debug.Log("Add Record : " + undoName + " (" + undoID + ")");

			//변경 21.7.17
			if(_record == null)
			{
				_record = new SnapshotRecord();
			}
			_record.SetData(undoID, undoName, isAnyAddedOrRemoved, _lastCreatedCounter);



			//이것까지 합친다.
			Undo.CollapseUndoOperations(undoID);
		}

		/// <summary>
		/// 객체의 생성/삭제 이벤트가 발생했다면, 마지막 기록에 해당 정보를 기록한다.
		/// </summary>
		public void SetAnyAddedOrRemovedToLastRecord()
		{
			//Debug.Log("SetAnyAddedOrRemovedToLastRecord");

			//이전
			//if(_records.Count == 0)
			//{
			//	return;
			//}

			//변경 21.7.17
			if(_record == null
				|| !_record._isDataSaved)
			{
				//저장된 데이터가 없다면
				return;
			}

			//이전 : 리스트 방식
			//SnapshotRecord lastRecord = _records[_records.Count - 1];
			//if(lastRecord._isAnyAddedOrRemoved)
			//{
			//	//이미 해당 "생성/삭제의 기록"으로 등록되었다.
			//	//Debug.Log(">> (이미 Add/Removed 등록됨) Counter : " + _lastCreatedCounter);
			//	return;
			//}

			//변경 21.7.17
			if(_record._isAnyAddedOrRemoved)
			{
				//이미 해당 "생성/삭제의 기록"으로 등록되었다.
				return;
			}



			//카운터 증가
			_lastCreatedCounter += 1;
			if(_lastCreatedCounter > MAX_CREATED_COUNTER)
			{
				_lastCreatedCounter = 1;
			}

			_record._isAnyAddedOrRemoved = true;
			_record._createdCounter = _lastCreatedCounter;//카운터 반영

			//Debug.Log(">> Counter : " + _lastCreatedCounter);

			int curUndoID = Undo.GetCurrentGroup();

			//만약 마지막 Record의 UndoID와 다르다면 > 병합
			if(_record._undoID != curUndoID)
			{
				//Debug.Log("> SetAnyAddedOrRemovedToLastRecord : Undo ID 미일치 : " + curUndoID + " > 병합 : " + lastRecord._undoID);
				Undo.CollapseUndoOperations(_record._undoID);//이쪽으로 병합한다.
			}
		}



		public UNDO_RESULT OnUndoRedoPerformed()
		{
			//이전 방식 : 리스트 이용
			//SnapshotRecord lastRecord = null;
			//if(_records != null && _records.Count > 0)
			//{
			//	lastRecord = _records[_records.Count - 1];
			//}


			////복구된 스탭샷이 없다면
			//if(lastRecord == null)
			//{
			//	if(_lastCreatedCounter > 0)
			//	{
			//		//구조가 바뀌었을 수 있다.					
			//		//Debug.LogError("실행 취소 : 레코드 없음 <구조 변화> (CID : " + _lastCreatedCounter + " )");
			//		_lastCreatedCounter = 0;
			//		return UNDO_RESULT.StructChanged;
			//	}

			//	//크게 변경된 것은 없었을 것
			//	//Debug.Log("실행 취소 : 레코드 없음 (CID : " + _lastCreatedCounter + " )");
			//	return UNDO_RESULT.DataChanged;
			//}

			//if(lastRecord._createdCounter != _lastCreatedCounter)
			//{
			//	//"생성 카운터"가 변경되었다.
			//	//Debug.LogError("실행 취소 : [" + lastRecord._undoID + " : " + lastRecord._undoName + "] <구조 변화> (CID : " + _lastCreatedCounter + " > " + lastRecord._createdCounter + " )");
			//	_lastCreatedCounter = lastRecord._createdCounter;
			//	return UNDO_RESULT.StructChanged;
			//}
			//else
			//{
			//	//Debug.Log("실행 취소 : [" + lastRecord._undoID + " : " + lastRecord._undoName + "] (CID : " + _lastCreatedCounter + " )");
			//	return UNDO_RESULT.DataChanged;
			//}


			//변경 21.7.17 : 단일 데이터 방식으로 변경
			//복구된 스냅샷이 없다면
			if(_record == null || !_record._isDataSaved)
			{
				if(_lastCreatedCounter > 0)
				{
					//구조가 바뀌었을 수 있다.					
					//Debug.LogError("실행 취소 : 레코드 없음 <구조 변화> (CID : " + _lastCreatedCounter + " )");
					_lastCreatedCounter = 0;
					return UNDO_RESULT.StructChanged;
				}

				//크게 변경된 것은 없었을 것
				//Debug.Log("실행 취소 : 레코드 없음 (CID : " + _lastCreatedCounter + " )");
				return UNDO_RESULT.DataChanged;
			}

			if(_record._createdCounter != _lastCreatedCounter)
			{
				//"생성 카운터"가 변경되었다.
				//Debug.LogError("실행 취소 : [" + _record._undoID + " : " + _record._undoName + "] <구조 변화> (CID : " + _lastCreatedCounter + " > " + _record._createdCounter + " )");
				_lastCreatedCounter = _record._createdCounter;
				return UNDO_RESULT.StructChanged;
			}
			else
			{
				//Debug.Log("실행 취소 : [" + _record._undoID + " : " + _record._undoName + "] (CID : " + _lastCreatedCounter + " )");
				return UNDO_RESULT.DataChanged;
			}
		}
	}
}

#endif