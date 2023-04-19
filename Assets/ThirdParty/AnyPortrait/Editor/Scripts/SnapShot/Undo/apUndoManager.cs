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

//	public class apUndoManager
//	{
//		// Singletone
//		//-----------------------------------------------------------------
//		private static apUndoManager _instance = new apUndoManager();
//		public static apUndoManager I { get { return _instance; } }



//		// Members
//		//-----------------------------------------------------------------
//		private apUndoUnitBase _lastUnit = null;
//		private apUndoUnitBase _curUnit = null;
//		private int _iCurUnit = -1;
//		private List<apUndoUnitBase> _units = new List<apUndoUnitBase>();

//		private apPortrait _curPortrait = null;

//		private const int MAX_UNDO_UNIT = 20;

//		public enum COMMAND
//		{
//			MeshVertex,

//		}

//		public enum ACTION_TYPE
//		{
//			None,
//			Changed,
//			Add,
//			Remove,
//		}

//		// Init
//		//-----------------------------------------------------------------
//		private apUndoManager()
//		{
//			_curPortrait = null;
//			Clear();
//		}


//		public void Clear()
//		{
//			_lastUnit = null;
//			_curUnit = null;
//			_iCurUnit = -1;
//			_units.Clear();
//		}


//		// Functions
//		//-----------------------------------------------------------------
//		/// <summary>
//		/// Undo작업을 위한 Unit을 생성한다.
//		/// 생성과 동시에 COMMAND에 맞는 현재 상태를 저장하며, 이후 생성된 Unit에 Refresh()를 호출해야한다.
//		/// 만약 Undo를 한 상태에서 이 함수를 호출한다면 되돌려진 Undo 기록은 삭제된다.
//		/// </summary>
//		/// <param name="command"></param>
//		/// <param name="keyObj"></param>
//		/// <param name="actionType"></param>
//		/// <param name="label"></param>
//		/// <param name="portrait"></param>
//		/// <returns></returns>
//		public apUndoUnitBase MakeUndo(COMMAND command, object keyObj, ACTION_TYPE actionType, string label, apPortrait portrait)
//		{
//			if (_curPortrait != portrait || _curPortrait == null)
//			{
//				//Portrait가 다르면 자동으로 Undo는 리셋
//				Clear();

//				_curPortrait = portrait;
//				if (_curPortrait == null)
//				{
//					return null;
//				}
//			}

//			//Make 과정에서는 항상
//			//새로운("또는 연결된") 유닛이 [마지막]이어야한다.
//			//curUnit이 중간에 있다면 그 사이는 삭제되어야 한다.

//			bool isSameUnit = true;
//			if (_lastUnit == null)
//			{
//				isSameUnit = false;
//			}
//			else if (!_lastUnit.IsSameUnit((int)command, keyObj, actionType))
//			{
//				isSameUnit = false;
//			}
//			else if (!_lastUnit.IsContinuedRecord())
//			{
//				//연속으로 기록이 불가능할 경우에도 체크
//				isSameUnit = false;
//			}

//			if (isSameUnit)
//			{
//				_curUnit = _lastUnit;
//				return _lastUnit;
//			}

//			//Debug.Log("Make Undo");

//			apUndoUnitBase newUndoUnit = null;

//			//타입에 따라 다른 상속된 클래스로 생성한다.
//			switch (command)
//			{
//				case COMMAND.MeshVertex:
//					newUndoUnit = new apUndoUnit_MeshVertex();
//					break;

//				default:
//					Debug.LogError("Undo 에러 : 정의되지 않은 Command [" + command + "]");
//					return null;
//			}

//			newUndoUnit.Init((int)command, keyObj, actionType, label);
//			newUndoUnit.SavePrevStatus();//<<현재 상태를 저장하자

//			//리스트에 추가하기 전에
//			//_curUnit의 뒤쪽에 위치한 객체들은 모두 삭제하자
//			//[시작] ..... [cur] ...... [last]
//			//< [시작] ....[cur] > + [new]        (~[last] 삭제)
//			//만약 cur가 없거나(리스트가 비어있음) "cur = last"라면 패스한다.
//			if (_curUnit != null && _curUnit != _lastUnit)
//			{
//				_iCurUnit = -1;
//				//0, 1, 2, 3, 4 [5]
//				//3 삭제
//				//5 - 3 = 2 (3, 4)
//				for (int i = 0; i < _units.Count; i++)
//				{
//					if (_curUnit == _units[i])
//					{
//						_iCurUnit = i;
//						break;
//					}
//				}
//				if (_iCurUnit > 0)
//				{
//					int iRemove = _iCurUnit + 1;
//					int nRemove = _units.Count - iRemove;
//					if (nRemove > 0)
//					{
//						_units.RemoveRange(iRemove, nRemove);
//					}
//				}

//				_lastUnit = _curUnit;
//			}

//			//이제 맨 뒤에 newUndoUnit을 붙이자
//			_units.Add(newUndoUnit);
//			_lastUnit = newUndoUnit;
//			_curUnit = _lastUnit;

//			//Debug.Log("Make New Undo [" + _units.Count + "]");

//			return newUndoUnit;
//		}



//		//TODO : Undo 스택 관리
//		/// <summary>
//		/// 현재 작업을 취소하고, 이전 작업으로 넘어간다.
//		/// </summary>
//		/// <returns></returns>
//		public apUndoUnitBase Undo(apPortrait portrait, apEditor editor)
//		{
//			if (_curPortrait != portrait || _curPortrait == null)
//			{
//				//Portrait가 다르면 자동으로 Undo는 리셋
//				Clear();

//				_curPortrait = portrait;
//				//Debug.LogError("Portrait가 맞지 않는다.");
//				return null;
//			}

//			if (_units.Count == 0)
//			{
//				//Debug.LogError("Unit 카운트가 0");
//				return null;
//			}

//			if (_curUnit == null)
//			{
//				//더이상 Undo를 할 수 없다.
//				//Debug.LogError("Cur Unit이 없다.");
//				return null;
//			}

//			//Debug.Log("Units : " + _units.Count);

//			//Undo Execute 수행을 먼저하고,
//			//인덱스를 이전으로 옮기자
//			apUndoUnitBase executedUnit = _curUnit;
//			_curUnit.ExecuteNext2Prev(editor);


//			_iCurUnit = GetIndex(_curUnit);
//			if (_iCurUnit < 0)
//			{
//				_curUnit = null;
//			}
//			else
//			{
//				int iPrevUnit = _iCurUnit - 1;
//				if (iPrevUnit < 0)
//				{
//					_curUnit = null;
//					_iCurUnit = -1;
//				}
//				else
//				{
//					//이전 유닛으로 이동한다.
//					_curUnit = _units[iPrevUnit];
//					_iCurUnit = iPrevUnit;
//				}
//			}

//			return executedUnit;
//		}

//		public apUndoUnitBase Redo(apPortrait portrait, apEditor editor)
//		{
//			//인덱스를 다음으로 옮기고 Execute를 수행하자
//			if (_curPortrait != portrait || _curPortrait == null)
//			{
//				//Portrait가 다르면 자동으로 Undo는 리셋
//				Clear();

//				_curPortrait = portrait;
//				return null;
//			}

//			if (_units.Count == 0)
//			{
//				return null;
//			}

//			if (_curUnit == null)
//			{
//				//더이상 Undo를 할 수 없다.
//				return null;
//			}

//			//Undo Execute 수행을 먼저하고,
//			//인덱스를 이전으로 옮기자
//			_iCurUnit = GetIndex(_curUnit);
//			if (_iCurUnit < 0)
//			{
//				_curUnit = null;
//				return null;
//			}

//			int iNextUnit = _iCurUnit + 1;
//			if (iNextUnit < _units.Count)
//			{
//				_curUnit = _units[iNextUnit];
//				_curUnit.ExecutePrev2Next(editor);
//			}
//			else
//			{
//				//더이상 진행이 안되네요..
//				_curUnit = _units[_units.Count - 1];
//				_iCurUnit = _units.Count - 1;
//				return null;
//			}

//			return _curUnit;
//		}


//		private int GetIndex(apUndoUnitBase undoUnit)
//		{
//			if (_units.Count == 0 || undoUnit == null)
//			{
//				return -1;
//			}

//			return _units.IndexOf(undoUnit);
//		}
//		// Get / Set
//		//-----------------------------------------------------------------


//	}

//}