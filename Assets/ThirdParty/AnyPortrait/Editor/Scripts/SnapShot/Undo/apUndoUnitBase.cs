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

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;

//using AnyPortrait;

//namespace AnyPortrait
//{

//	// Undo 클래스
//	// 타겟 오브젝트를 키값으로 하여
//	// 타겟의 "멤버 변수"의 값의 "이전 상태"와 "변경되 후의 상태"를 기록한다.
//	// 변경의 종류를 기록하여 Undo/Redo가 가능하게 만든다.
//	// 데이터의 형식에 따라서 다른 처리방식을 가지므로, 상속하여 사용한다.
//	// Set Record와 달리
//	// [저장할 데이터 생성 또는 호출] -> 데이터 변경 -> [갱신 요청]의 단계를 가진다.
//	// 값의 형태는 레퍼런스가 아닌 Value로 저장한다.
//	// 그 자체로 Prev, Next 상태를 가지므로 Undo, Redo가 가능하다
//	public class apUndoUnitBase
//	{
//		// Members
//		//----------------------------------------------------------------
//		public int _commandType = -1;
//		public object _keyObj = null;
//		public string _label = "";


//		public apUndoManager.ACTION_TYPE _actionType = apUndoManager.ACTION_TYPE.None;


//		// Init
//		//----------------------------------------------------------------
//		public apUndoUnitBase()
//		{

//		}


//		public virtual void Init(int commandType, object keyObj, apUndoManager.ACTION_TYPE actionType, string label)
//		{
//			_commandType = commandType;
//			_keyObj = keyObj;
//			_actionType = actionType;
//			_label = label;
//		}

//		/// <summary>
//		/// 동일한 키를 가진 Record는 누적해서 저장할 수 있는가?
//		/// 만약 연속적으로 위치값을 바꾸는 경우에는 Record를 새로 생성할 필요 없이 Refresh만 하면 된다.
//		/// Add, Remove와 같은 경우에는 false로 둬서 매번 만들어주도록 하자
//		/// </summary>
//		/// <returns></returns>
//		public virtual bool IsContinuedRecord()
//		{
//			return true;
//		}

//		//추가 멤버는 별도로 처리하자

//		// Functions
//		//----------------------------------------------------------------
//		/// <summary>
//		/// 현재 변수값을 저장한다.
//		/// 오브젝트의 현재 값을 그대로 저장한다.
//		/// </summary>
//		public virtual void SavePrevStatus()
//		{

//		}

//		/// <summary>
//		/// 변경된(갱신, 추가, 삭제)된 valueObj를 반영하자
//		/// </summary>
//		public virtual void Refresh()
//		{

//		}

//		/// <summary>
//		/// Prev 상태에서 Next 상태로 작업을 수행한다.
//		/// Redo 작업시 수행된다.
//		/// </summary>
//		public virtual void ExecutePrev2Next(apEditor editor)
//		{

//		}

//		/// <summary>
//		/// Next 상태에서 Prev 상태로 되돌린다.
//		/// Undo 작업시 수행된다.
//		/// </summary>
//		public virtual void ExecuteNext2Prev(apEditor editor)
//		{
//			Debug.Log("Undo : Base");
//		}


//		// Get / Set
//		//----------------------------------------------------------------
//		public bool IsSameUnit(int commandType, object keyObj, apUndoManager.ACTION_TYPE actionType)
//		{
//			return _commandType == commandType && _keyObj == keyObj && _actionType == actionType;
//		}

//	}

//}