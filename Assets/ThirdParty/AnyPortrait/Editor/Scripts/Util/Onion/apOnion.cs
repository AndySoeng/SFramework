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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// 잔상을 나타내는 객체
	/// ControlParam, AnimClip의 프레임 값을 임시로 저장하여
	/// 잔상을 렌더링 하는 것에 사용한다.
	/// Record -> Adapt Record Value -> Recover 순서대로 작동한다.
	/// Editor에 속한다.
	/// </summary>
	public class apOnion
	{
		// Members
		//--------------------------------------------------------
		private bool _isRecorded = false;//<<값이 저장된 적이 있는가.

		private class ControlParamRecord
		{
			public apControlParam _controlParam = null;//<<값을 저장한 ControlParam
			public int _intValue_Record = -1;
			public float _floatValue_Record = 0.0f;
			public Vector2 _vec2Value_Record = Vector2.zero;

			public int _intValue_Cur = -1;
			public float _floatValue_Cur = 0.0f;
			public Vector2 _vec2Value_Cur = Vector2.zero;

			public ControlParamRecord(apControlParam controlParam)
			{
				_controlParam = controlParam;
				
				//현재 상태값을 저장한다.
				_intValue_Record = _controlParam._int_Cur;
				_floatValue_Record = _controlParam._float_Cur;
				_vec2Value_Record = _controlParam._vec2_Cur;

				_intValue_Cur = _intValue_Record;
				_floatValue_Cur = _floatValue_Record;
				_vec2Value_Cur = _vec2Value_Record;
			}
		}

		private List<ControlParamRecord> _controlParamRecords = new List<ControlParamRecord>();

		private int _animClipFrame_Record = -1;
		private int _animClipFrame_Cur = -1;


		private bool _isVisible = false;




		// Init
		//--------------------------------------------------------
		public apOnion()
		{
			Clear();
		}


		public void Clear()
		{
			_isVisible = false;
			_animClipFrame_Record = -1;
			_animClipFrame_Cur = -1;

			_isRecorded = false;
		
			_controlParamRecords.Clear();
		}


		// Functions
		//--------------------------------------------------------

		public void Record(apEditor editor)
		{
			if (editor._portrait == null)
			{
				return;
			}

			_isRecorded = false;
			_controlParamRecords.Clear();

			_animClipFrame_Record = -1;
			_animClipFrame_Cur = -1;


			for (int i = 0; i < editor._portrait._controller._controlParams.Count; i++)
			{
				_controlParamRecords.Add(new ControlParamRecord(editor._portrait._controller._controlParams[i]));
			}


			if (editor.Select.AnimClip != null)
			{
				_animClipFrame_Record = editor.Select.AnimClip.CurFrame;
				_animClipFrame_Cur = _animClipFrame_Record;
			}

			apMeshGroup curMeshGroup = null;
			if(editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				curMeshGroup = editor.Select.MeshGroup;
			}
			else if(editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				if(editor.Select.AnimClip != null)
				{
					curMeshGroup = editor.Select.AnimClip._targetMeshGroup;
				}
				
			}

			//if(curMeshGroup != null)
			//{
			//	apBone
			//	curMeshGroup._boneList_Root[0]._
			//}


			_isRecorded = true;
		}

		public bool AdaptRecord(apEditor editor)
		{
			if(!_isRecorded || editor._portrait == null)
			{
				return false;
			}

			//현재 Portrait의 값은 잠시 Cur 변수에 넣어두고, 저장되었던 값을 넣어주자
			ControlParamRecord cpRecord = null;
			for (int i = 0; i < _controlParamRecords.Count; i++)
			{
				cpRecord = _controlParamRecords[i];
				if(cpRecord._controlParam != null)
				{
					switch (cpRecord._controlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							cpRecord._intValue_Cur = cpRecord._controlParam._int_Cur;
							cpRecord._controlParam._int_Cur = cpRecord._intValue_Record;
							break;

						case apControlParam.TYPE.Float:
							cpRecord._floatValue_Cur = cpRecord._controlParam._float_Cur;
							cpRecord._controlParam._float_Cur = cpRecord._floatValue_Record;

							//Debug.Log("AdaptRecord : "+ cpRecord._controlParam._keyName + " : " + cpRecord._floatValue_Record);
							break;

						case apControlParam.TYPE.Vector2:
							cpRecord._vec2Value_Cur = cpRecord._controlParam._vec2_Cur;
							cpRecord._controlParam._vec2_Cur = cpRecord._vec2Value_Record;
							break;

					}
				}
			}

			if (editor.Select.AnimClip != null)
			{
				_animClipFrame_Cur = editor.Select.AnimClip.CurFrame;
				editor.Select.AnimClip.SetFrame_EditorNotStop(_animClipFrame_Record);
			}

			return true;
			
		}

		public void Recorver(apEditor editor)
		{
			if(!_isRecorded || editor._portrait == null)
			{
				return;
			}

			//Onion 처리 전의 값으로 다시 만들어준다.
			//현재 Portrait의 값은 잠시 Cur 변수에 넣어두고, 저장되었던 값을 넣어주자
			ControlParamRecord cpRecord = null;
			for (int i = 0; i < _controlParamRecords.Count; i++)
			{
				cpRecord = _controlParamRecords[i];
				if(cpRecord._controlParam != null)
				{
					switch (cpRecord._controlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							cpRecord._controlParam._int_Cur = cpRecord._intValue_Cur;
							break;

						case apControlParam.TYPE.Float:
							cpRecord._controlParam._float_Cur = cpRecord._floatValue_Cur;
							break;

						case apControlParam.TYPE.Vector2:
							cpRecord._controlParam._vec2_Cur = cpRecord._vec2Value_Cur;
							break;

					}
				}
			}

			if (editor.Select.AnimClip != null)
			{
				editor.Select.AnimClip.SetFrame_Editor(_animClipFrame_Cur);
			}
			
		}


		public void SetVisible(bool isVisible)
		{
			_isVisible = isVisible;
		}

		// Get / Set
		//--------------------------------------------------------
		public bool IsVisible {  get { return _isVisible; } }
		public bool IsRecorded {  get { return _isRecorded; } }

		public int RecordAnimFrame {  get { return _animClipFrame_Record; } }


	}
}