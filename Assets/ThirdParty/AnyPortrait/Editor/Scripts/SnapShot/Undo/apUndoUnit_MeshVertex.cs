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

//	public class apUndoUnit_MeshVertex : apUndoUnitBase
//	{
//		// Members
//		//----------------------------------------------
//		public class CopiedVertexData
//		{
//			public int _index;//Index Buffer에 들어가는 ID (배열 ID)
//			public int _uniqueID = -1;//<<고유번호를 발급하자
//			public Vector3 _pos;
//			public Vector2 _uv;

//			//Weight 들
//			//public float _volumeWeight = 0.0f;//양각 Weight (0~1)
//			//public float _physicsWeight = -1.0f;//물리 Weight (-1 / 0~1)

//			public CopiedVertexData(apVertex src)
//			{
//				_index = src._index;
//				_uniqueID = src._uniqueID;
//				_pos = src._pos;
//				_uv = src._uv;
//				//_volumeWeight = src._volumeWeight;
//				//_physicsWeight = src._physicsWeight;
//			}
//		}

//		private List<CopiedVertexData> _copiedVertices_Prev = new List<CopiedVertexData>();
//		private List<CopiedVertexData> _copiedVertices_Next = new List<CopiedVertexData>();


//		// Init
//		//----------------------------------------------
//		public override void Init(int commandType, object keyObj, apUndoManager.ACTION_TYPE actionType, string label)
//		{
//			base.Init(commandType, keyObj, actionType, label);
//		}


//		public override bool IsContinuedRecord()
//		{
//			if (_actionType == apUndoManager.ACTION_TYPE.Add ||
//				_actionType == apUndoManager.ACTION_TYPE.Remove)
//			{
//				return false;
//			}

//			return true;
//		}

//		// Functions
//		//----------------------------------------------
//		/// <summary>
//		/// 현재 변수값을 저장한다.
//		/// 오브젝트의 현재 값을 그대로 저장한다.
//		/// </summary>
//		public override void SavePrevStatus()
//		{
//			Debug.Log("Record Prev Status : Mesh Vertex : " + _actionType + " / " + _label);
//			// 액션 타입에 따라 keyObj와 저장된 데이터가 다르다.
//			switch (_actionType)
//			{
//				case apUndoManager.ACTION_TYPE.Add:
//					{
//						apMesh mesh = _keyObj as apMesh;
//						if (mesh == null)
//						{ return; }

//						//현재의 VertexData를 기록한다.
//						_copiedVertices_Prev.Clear();
//						for (int i = 0; i < mesh._vertexData.Count; i++)
//						{
//							_copiedVertices_Prev.Add(new CopiedVertexData(mesh._vertexData[i]));
//						}
//					}
//					break;

//				case apUndoManager.ACTION_TYPE.Remove:
//					break;

//				case apUndoManager.ACTION_TYPE.Changed:
//					break;
//			}
//		}

//		/// <summary>
//		/// 변경된(갱신, 추가, 삭제)된 valueObj를 반영하자
//		/// </summary>
//		public override void Refresh()
//		{
//			switch (_actionType)
//			{
//				case apUndoManager.ACTION_TYPE.Add:
//					{
//						apMesh mesh = _keyObj as apMesh;
//						if (mesh == null)
//						{ return; }

//						//변경된 VertexData를 기록한다.
//						_copiedVertices_Next.Clear();
//						for (int i = 0; i < mesh._vertexData.Count; i++)
//						{
//							_copiedVertices_Next.Add(new CopiedVertexData(mesh._vertexData[i]));
//						}
//					}
//					break;

//				case apUndoManager.ACTION_TYPE.Remove:
//					break;

//				case apUndoManager.ACTION_TYPE.Changed:
//					break;
//			}
//		}



//		public override void ExecutePrev2Next(apEditor editor)
//		{
//			try
//			{
//				switch (_actionType)
//				{
//					case apUndoManager.ACTION_TYPE.Add:
//						{
//							apMesh mesh = _keyObj as apMesh;
//							if (mesh == null)
//							{ return; }

//							//Prev에서 Next로 Vertex List를 비교하여 [추가]해준다.
//							Debug.Log("Redo : Add -> Remove");
//						}
//						break;

//					case apUndoManager.ACTION_TYPE.Remove:
//						break;

//					case apUndoManager.ACTION_TYPE.Changed:
//						break;
//				}
//			}
//			catch (Exception ex)
//			{
//				Debug.LogError("Redo Exception : " + ex);
//			}
//		}

//		public override void ExecuteNext2Prev(apEditor editor)
//		{
//			try
//			{
//				switch (_actionType)
//				{
//					case apUndoManager.ACTION_TYPE.Add:
//						{
//							apMesh mesh = _keyObj as apMesh;
//							if (mesh == null)
//							{ return; }

//							//Next에서 Prev로 Vertex List를 비교하여 [제거]해준다.
//							//Add의 반대는 Remove
//							Debug.Log("Undo : Add -> Remove");

//						}
//						break;

//					case apUndoManager.ACTION_TYPE.Remove:
//						break;

//					case apUndoManager.ACTION_TYPE.Changed:
//						break;
//				}
//			}
//			catch (Exception ex)
//			{
//				Debug.LogError("Undo Exception : " + ex);
//			}
//		}

//		// Get / Set
//		//----------------------------------------------
//	}

//}