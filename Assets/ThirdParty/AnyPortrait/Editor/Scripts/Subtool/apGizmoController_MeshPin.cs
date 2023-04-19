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
using System;
using System.Collections.Generic;

using AnyPortrait;

namespace AnyPortrait
{
	public partial class apGizmoController
	{
		// Mesh 메뉴의 Pin 편집에 대한 기즈모 이벤트
		// [ 기즈모 이벤트 있음 ]
		//1. 테스트 모드
		//2. 편집 모드 중 선택(TRS) 모드

		// [ 기즈모 이벤트 없음 ] - 단독 처리
		//3. 편집 모드 중 추가 모드 (거의 모든 처리 가능)
		//4. 편집 모드 중 연결 모드 > 기즈모 이벤트가 없다.


		/// <summary>
		/// [선택] 모드에서의 핀 편집 기즈모
		/// </summary>
		/// <returns></returns>
		public apGizmos.GizmoEventSet GetEventSet_MeshPinEdit_Default()
		{
			//변경 20.1.27
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__MeshPinEdit_Default,
														Unselect__MeshPinEdit, 
														Move__MeshPinEdit_Default, 
														Rotate__MeshPinEdit_Default, 
														Scale__MeshPinEdit__Default, 
														PivotReturn__MeshPinEdit_Default);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__MeshPinEdit_Default,
																null,
																null,
																null,
																null,
																null,
																apGizmos.TRANSFORM_UI.Position2D
																| apGizmos.TRANSFORM_UI.Vertex_Transform);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__MeshPinEdit_Default, 
														FFDTransform__MeshPinEdit_Default, 
														StartFFDTransform__MeshPinEdit_Default, 
														null, 
														null, 
														null);

			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__MeshPinEdit, 
																AddHotKeys__MeshPinEdit, 
																OnHotKeyEvent__MeshPinEdit_Default__Keyboard_Move,
																OnHotKeyEvent__MeshPinEdit_Default__Keyboard_Rotate, 
																OnHotKeyEvent__MeshPinEdit_Default__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;
		}

		/// <summary>
		/// [테스트] 모드에서의 핀 편집 Gizmo
		/// </summary>
		/// <returns></returns>
		public apGizmos.GizmoEventSet GetEventSet_MeshPinEdit_Test()
		{
			//변경 20.1.27
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__MeshPinEdit_Test,
														Unselect__MeshPinEdit, 
														Move__MeshPinEdit_Test, 
														Rotate__MeshPinEdit_Test, 
														Scale__MeshPinEdit__Test, 
														PivotReturn__MeshPinEdit_Test);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__MeshPinEdit_Test,
																null,
																null,
																null,
																null,
																null,
																apGizmos.TRANSFORM_UI.Position2D);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__MeshPinEdit_Test, 
														FFDTransform__MeshPinEdit_Test, 
														StartFFDTransform__MeshPinEdit_Test, 
														null, 
														null,
														null);
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__MeshPinEdit, 
																AddHotKeys__MeshPinEdit, 
																OnHotKeyEvent__MeshPinEdit_Test__Keyboard_Move, 
																OnHotKeyEvent__MeshPinEdit_Test__Keyboard_Rotate, 
																OnHotKeyEvent__MeshPinEdit_Test__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;
		}


		//-------------------------------------------------------------------------
		// 
		//-------------------------------------------------------------------------
		public apGizmos.SelectResult FirstLink__MeshPinEdit()
		{
			if(Editor.Select.Mesh == null)
			{
				return null;
			}
			
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				//선택된게 없다.
				return null;
			}

			if(nPins == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshPin);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apMeshPin>(Editor.Select.MeshPins);
			}
		}

		//------------------------------------------------------------------------------
		// 선택하기 (Select) - 공통
		//------------------------------------------------------------------------------
		public apGizmos.SelectResult Select__MeshPinEdit_Default(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return null;
			}
			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = pinGroup.NumPins;

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;

			apMeshPin curPin = null;

			float nearestWideClickDist = 0.0f;
			float curWideClickDist = 0.0f;
			apMeshPin clickedPin_Default = null;
			apMeshPin clickedPin_Wide = null;

			apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

			if (Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				for (int i = 0; i < nPins; i++)
				{
					curPin = pinGroup._pins_All[i];

					curWideClickDist = 0.0f;

					Vector2 pinPosGL = apGL.World2GL(curPin._defaultPos - meshOffset);
					clickResult = Editor.Controller.IsPinClickable(ref pinPosGL, ref mousePosGL, ref curWideClickDist);

					if(clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
					{
						continue;
					}

					if (clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
					{
						clickedPin_Default = curPin;
						break;
					}

					if (clickedPin_Wide == null || curWideClickDist < nearestWideClickDist)
					{
						clickedPin_Wide = curPin;
						nearestWideClickDist = curWideClickDist;
					}
				}
			}
			if(clickedPin_Default != null)
			{
				//기본 범위에서 선택
				Editor.Select.SelectMeshPin(clickedPin_Default, selectType);
			}
			else if(clickedPin_Wide != null)
			{
				//확장된 범위에서 선택
				Editor.Select.SelectMeshPin(clickedPin_Wide, selectType);
			}
			else
			{
				if(selectType == apGizmos.SELECT_TYPE.New)
				{
					Editor.Select.UnselectMeshPins();
				}
			}
			Editor.SetRepaint();
			

			if(Editor.Select.NumMeshPins == 0)
			{
				return null;
			}
			if(Editor.Select.NumMeshPins == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshPin);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apMeshPin>(Editor.Select.MeshPins);
			}

		}





		public apGizmos.SelectResult Select__MeshPinEdit_Test(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return null;
			}
			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = pinGroup.NumPins;

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;

			apMeshPin curPin = null;

			float nearestWideClickDist = 0.0f;
			float curWideClickDist = 0.0f;
			apMeshPin clickedPin_Default = null;
			apMeshPin clickedPin_Wide = null;

			apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

			if (Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				for (int i = 0; i < nPins; i++)
				{
					curPin = pinGroup._pins_All[i];

					curWideClickDist = 0.0f;

					Vector2 tmpPinPosGL = apGL.World2GL(curPin.TmpPos_MeshTest - meshOffset);

					clickResult = Editor.Controller.IsPinClickable(ref tmpPinPosGL, ref mousePosGL, ref curWideClickDist);

					if(clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
					{
						continue;
					}

					if(clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
					{
						clickedPin_Default = curPin;
						break;
					}

					if (clickedPin_Wide == null || curWideClickDist < nearestWideClickDist)
					{
						clickedPin_Wide = curPin;
						nearestWideClickDist = curWideClickDist;
					}
				}
			}
			if(clickedPin_Default != null)
			{
				//기본 범위에서 선택
				Editor.Select.SelectMeshPin(clickedPin_Default, selectType);
			}
			else if(clickedPin_Wide != null)
			{
				//확장된 범위에서 선택
				Editor.Select.SelectMeshPin(clickedPin_Wide, selectType);
			}
			else
			{
				if(selectType == apGizmos.SELECT_TYPE.New)
				{
					Editor.Select.UnselectMeshPins();
				}
			}
			Editor.SetRepaint();


			

			if(Editor.Select.NumMeshPins == 0)
			{
				return null;
			}
			if(Editor.Select.NumMeshPins == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshPin);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apMeshPin>(Editor.Select.MeshPins);
			}

		}



		public void Unselect__MeshPinEdit()
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			Editor.Select.UnselectMeshPins();
			Editor.SetRepaint();
		}


		//------------------------------------------------------------------------------
		// 단축키
		//------------------------------------------------------------------------------
		public void AddHotKeys__MeshPinEdit(bool isGizmoRenderable, apGizmos.CONTROL_TYPE controlType, bool isFFDMode)
		{
			Editor.AddHotKeyEvent(OnHotKeyEvent__MeshPinEdit__Ctrl_A, apHotKeyMapping.KEY_TYPE.MakeMesh_SelectAllVertices, null);//변경 20.12.3
		}

		// 단축키 : 버텍스 전체 선택
		private apHotKey.HotKeyResult OnHotKeyEvent__MeshPinEdit__Ctrl_A(object paramObject)
		{
			if (Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return null;
			}

			//v1.4.2 : FFD 모드시에는 FFD 포인트를 선택해야한다.
			if(Editor.Gizmos.IsFFDMode)
			{
				Editor.Gizmos.SelectAllFFDPoints();
				Editor.SetRepaint();
				return apHotKey.HotKeyResult.MakeResult();
			}


			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;

			Editor.Select.SelectMeshPins(pinGroup._pins_All, apGizmos.SELECT_TYPE.Add);

			Editor.SetRepaint();

			Editor.Gizmos.SetSelectResultForce_Multiple<apMeshPin>(Editor.Select.MeshPins);

			return apHotKey.HotKeyResult.MakeResult();
		}

		// 여러개 선택
		//--------------------------------------------------------------------------
		public apGizmos.SelectResult MultipleSelect__MeshPinEdit_Default(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return null;
			}

			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = pinGroup.NumPins;

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;

			apMeshPin curPin = null;

			List<apMeshPin> selectedPins = new List<apMeshPin>();

			for (int i = 0; i < nPins; i++)
			{
				curPin = pinGroup._pins_All[i];

				Vector2 pinPosGL = apGL.World2GL(curPin._defaultPos - meshOffset);

				bool isSelectable = (mousePosGL_Min.x < pinPosGL.x && pinPosGL.x < mousePosGL_Max.x
									&& mousePosGL_Min.y < pinPosGL.y && pinPosGL.y < mousePosGL_Max.y);

				if(isSelectable)
				{
					selectedPins.Add(curPin);
				}
			}
			
			Editor.Select.SelectMeshPins(selectedPins, areaSelectType);

			Editor.SetRepaint();

			if(Editor.Select.NumMeshPins == 0)
			{
				return null;
			}
			if(Editor.Select.NumMeshPins == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshPin);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apMeshPin>(Editor.Select.MeshPins);
			}
		}



		public apGizmos.SelectResult MultipleSelect__MeshPinEdit_Test(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return null;
			}

			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = pinGroup.NumPins;

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;

			apMeshPin curPin = null;

			List<apMeshPin> selectedPins = new List<apMeshPin>();

			for (int i = 0; i < nPins; i++)
			{
				curPin = pinGroup._pins_All[i];

				Vector2 pinPosGL = apGL.World2GL(curPin.TmpPos_MeshTest - meshOffset);

				bool isSelectable = (mousePosGL_Min.x < pinPosGL.x && pinPosGL.x < mousePosGL_Max.x
									&& mousePosGL_Min.y < pinPosGL.y && pinPosGL.y < mousePosGL_Max.y);

				if(isSelectable)
				{
					selectedPins.Add(curPin);
				}
			}
			
			Editor.Select.SelectMeshPins(selectedPins, areaSelectType);

			Editor.SetRepaint();

			if(Editor.Select.NumMeshPins == 0)
			{
				return null;
			}
			if(Editor.Select.NumMeshPins == 1)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshPin);
			}
			else
			{
				return apGizmos.SelectResult.Main.SetMultiple<apMeshPin>(Editor.Select.MeshPins);
			}
		}





		// 이동
		//-------------------------------------------------------------------------------------
		public void Move__MeshPinEdit_Default(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apMeshPin curPin = null;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				curPin._defaultPos += deltaMoveW;
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();

			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			Editor.SetRepaint();
		}

		public void Move__MeshPinEdit_Test(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			//apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apMeshPin curPin = null;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				//curPin._testPos += deltaMoveW;
				curPin.SetTmpPos_MeshTest(curPin.TmpPos_MeshTest + deltaMoveW);
			}

			//커브 계산 (바로 업데이트)
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);
			Editor.SetRepaint();
		}


		// 회전
		//----------------------------------------------------------------------------------------------------------
		public void Rotate__MeshPinEdit_Default(float deltaAngleW, bool isFirstRotate)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			apMeshPinGroup pinGroup = mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin_Rotate,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if(Mathf.Abs(deltaAngleW) == 0.0f)
			{
				return;
			}


			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin._defaultPos - mesh._offsetPos;
			}
			centerPos /= nPins;

			//변환 행렬
			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			//다시 돌면서 적용한다.
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				posW = curPin._defaultPos - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);
				curPin._defaultPos = posW + mesh._offsetPos;
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();

			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			Editor.SetRepaint();
		}




		public void Rotate__MeshPinEdit_Test(float deltaAngleW, bool isFirstRotate)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			//apMeshPinGroup pinGroup = mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin_Rotate,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if(Mathf.Abs(deltaAngleW) == 0.0f) { return; }


			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin.TmpPos_MeshTest - mesh._offsetPos;
			}
			centerPos /= nPins;

			//변환 행렬
			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			//다시 돌면서 적용한다.
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];

				posW = curPin.TmpPos_MeshTest - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);

				curPin.SetTmpPos_MeshTest(posW + mesh._offsetPos);
			}

			//커브 계산
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);

			Editor.SetRepaint();
		}



		// 크기 변환
		//--------------------------------------------------------------------------------------
		public void Scale__MeshPinEdit__Default(Vector2 deltaScaleW, bool isFirstScale)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}
			
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MovePin_Scale, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin._defaultPos - mesh._offsetPos;
			}
			centerPos /= nPins;

			Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0.0f, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				posW = curPin._defaultPos - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);
				curPin._defaultPos = posW + mesh._offsetPos;
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();


			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			Editor.SetRepaint();
		}





		public void Scale__MeshPinEdit__Test(Vector2 deltaScaleW, bool isFirstScale)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			//apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}
			
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MovePin_Scale, 
												Editor, 
												mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin.TmpPos_MeshTest - mesh._offsetPos;
			}
			centerPos /= nPins;

			Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0.0f, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				posW = curPin.TmpPos_MeshTest - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);
				curPin.SetTmpPos_MeshTest(posW + mesh._offsetPos);
			}

			//커브 계산
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);

			Editor.SetRepaint();
		}




		// Transform UI (Pos)
		//-----------------------------------------------------------------------------------------------------------
		public void TransformChanged_Position__MeshPinEdit_Default(Vector2 pos)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			if(Editor.Gizmos.IsFFDMode)
			{
				//FFD 모드에서는 처리가 안된다.
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}

			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin,
										Editor,
										Editor.Select.Mesh, 
										false,
										apEditorUtil.UNDO_STRUCT.ValueOnly);
			
			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin._defaultPos - mesh._offsetPos;
			}
			centerPos /= nPins;

			pos -= mesh._offsetPos;

			Vector2 deltaMoveW = pos - centerPos;

			//위치를 적용한다.			
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				curPin._defaultPos += deltaMoveW;
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();
			
			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			Editor.SetRepaint();
		}


		public void TransformChanged_Position__MeshPinEdit_Test(Vector2 pos)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return;
			}

			if(Editor.Gizmos.IsFFDMode)
			{
				//FFD 모드에서는 처리가 안된다.
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			//apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return;
			}

			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin,
										Editor,
										Editor.Select.Mesh, 
										false,
										apEditorUtil.UNDO_STRUCT.ValueOnly);
			
			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin.TmpPos_MeshTest - mesh._offsetPos;
			}
			centerPos /= nPins;

			pos -= mesh._offsetPos;

			Vector2 deltaMoveW = pos - centerPos;

			//위치를 적용한다.			
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				curPin.SetTmpPos_MeshTest(curPin.TmpPos_MeshTest + deltaMoveW);
			}

			//커브 계산
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);
			
			Editor.SetRepaint();
		}




		// FFD
		//------------------------------------------------------------------------------------------------
		// FFD Start
		public bool StartFFDTransform__MeshPinEdit_Default()
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			List<object> srcObjectList = new List<object>();
			List<Vector2> worldPosList = new List<Vector2>();
			List<Vector2> orgPosList = new List<Vector2>();
			apMeshPin curPin = null;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				srcObjectList.Add(curPin);
				worldPosList.Add(curPin._defaultPos - mesh._offsetPos);
				orgPosList.Add(curPin._defaultPos);
			}
			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList, orgPosList);//<<True로 리턴할거면 이 함수를 호출해주자
			return true;
		}


		public bool StartFFDTransform__MeshPinEdit_Test()
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			List<object> srcObjectList = new List<object>();
			List<Vector2> worldPosList = new List<Vector2>();
			List<Vector2> orgPosList = new List<Vector2>();
			apMeshPin curPin = null;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				srcObjectList.Add(curPin);
				worldPosList.Add(curPin.TmpPos_MeshTest - mesh._offsetPos);
				orgPosList.Add(curPin.TmpPos_MeshTest);
			}
			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList, orgPosList);//<<True로 리턴할거면 이 함수를 호출해주자
			return true;
		}



		// FFD Process
		public bool FFDTransform__MeshPinEdit_Default(List<object> srcObjects, List<Vector2> posData, apGizmos.FFD_ASSIGN_TYPE assignType, bool isResultAssign, bool isRecord)
		{
			if(!isResultAssign)
			{
				//결과 적용이 아닌 일반 수정 작업시
				//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.

				if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
				{
					return false;
				}


				if(Editor.Select.Mesh._pinGroup.NumPins == 0)
				{
					return false;
				}
			}

			apMesh mesh = Editor.Select.Mesh;
			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			if (isRecord)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MovePin,
												Editor,
												Editor.Select.Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apMeshPin curPin = null;
			if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
			{
				//World Pos를 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					curPin = srcObjects[i] as apMeshPin;
					if (curPin == null)
					{
						continue;
					}
					Vector2 worldPos = posData[i];
					curPin._defaultPos = worldPos + mesh._offsetPos;
				}
			}
			else//if (assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
			{
				//저장된 데이터를 직접 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					curPin = srcObjects[i] as apMeshPin;
					if (curPin == null)
					{
						continue;
					}
					curPin._defaultPos = posData[i];
				}
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();

			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			return true;
		}



		public bool FFDTransform__MeshPinEdit_Test(List<object> srcObjects, List<Vector2> posData, apGizmos.FFD_ASSIGN_TYPE assignType, bool isResultAssign, bool isRecord)
		{
			if(!isResultAssign)
			{
				//결과 적용이 아닌 일반 수정 작업시
				//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.

				if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
				{
					return false;
				}


				if(Editor.Select.Mesh._pinGroup.NumPins == 0)
				{
					return false;
				}
			}

			apMesh mesh = Editor.Select.Mesh;
			//apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			if (isRecord)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MovePin,
												Editor,
												Editor.Select.Mesh, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apMeshPin curPin = null;
			if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
			{
				//World Pos를 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					curPin = srcObjects[i] as apMeshPin;
					if (curPin == null)
					{
						continue;
					}
					Vector2 worldPos = posData[i];
					curPin.SetTmpPos_MeshTest(worldPos + mesh._offsetPos);
				}
			}
			else//if (assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
			{
				//저장된 데이터를 직접 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					curPin = srcObjects[i] as apMeshPin;
					if (curPin == null)
					{
						continue;
					}
					curPin.SetTmpPos_MeshTest(posData[i]);
				}
			}

			//커브 계산 (바로 업데이트)
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);

			return true;
		}


		// 키보드 입력
		//------------------------------------------------------------------------------------------------------
		private bool OnHotKeyEvent__MeshPinEdit_Default__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			//Move__MeshPinEdit 함수의 코드를 이용
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apMeshPin curPin = null;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				curPin._defaultPos += deltaMoveW;
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();

			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			Editor.SetRepaint();

			return true;
		}



		private bool OnHotKeyEvent__MeshPinEdit_Test__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			
			//apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apMeshPin curPin = null;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				//curPin._testPos += deltaMoveW;
				curPin.SetTmpPos_MeshTest(curPin.TmpPos_MeshTest + deltaMoveW);
			}

			//커브 계산 (바로 업데이트)
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);
			Editor.SetRepaint();

			return true;
		}






		private bool OnHotKeyEvent__MeshPinEdit_Default__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__MeshPinEdit 함수의 코드를 이용
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			apMeshPinGroup pinGroup = mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin_Rotate,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			

			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin._defaultPos - mesh._offsetPos;
			}
			centerPos /= nPins;

			//변환 행렬
			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			//다시 돌면서 적용한다.
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				posW = curPin._defaultPos - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);
				curPin._defaultPos = posW + mesh._offsetPos;
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();

			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			Editor.SetRepaint();

			return true;
		}




		private bool OnHotKeyEvent__MeshPinEdit_Test__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__MeshPinEdit 함수의 코드를 이용
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			//apMeshPinGroup pinGroup = mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MovePin_Rotate,
													Editor,
													Editor.Select.Mesh, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			
			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin.TmpPos_MeshTest - mesh._offsetPos;
			}
			centerPos /= nPins;

			//변환 행렬
			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			//다시 돌면서 적용한다.
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];

				posW = curPin.TmpPos_MeshTest - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);

				curPin.SetTmpPos_MeshTest(posW + mesh._offsetPos);
			}

			//커브 계산
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);

			Editor.SetRepaint();

			return true;
		}




		private bool OnHotKeyEvent__MeshPinEdit_Default__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__MeshTRS 함수의 코드를 이용
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Select
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}
			
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MovePin_Scale, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin._defaultPos - mesh._offsetPos;
			}
			centerPos /= nPins;

			Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0.0f, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				posW = curPin._defaultPos - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);
				curPin._defaultPos = posW + mesh._offsetPos;
			}

			//커브 계산
			pinGroup.Default_UpdateCurves();


			//옵션에 따른 가중치 재계산
			Editor.Select.RecalculatePinWeightByOption();

			Editor.SetRepaint();

			return true;
		}




		private bool OnHotKeyEvent__MeshPinEdit_Test__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__MeshTRS 함수의 코드를 이용
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin
				|| Editor._meshEditMode_Pin_ToolMode != apEditor.MESH_EDIT_PIN_TOOL_MODE.Test
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			//apMeshPinGroup pinGroup = Editor.Select.Mesh._pinGroup;
			int nPins = Editor.Select.NumMeshPins;
			if(nPins == 0)
			{
				return false;
			}
			
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_MovePin_Scale, 
												Editor, 
												mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//중점을 계산한다.
			apMeshPin curPin = null;
			Vector2 centerPos = Vector2.zero;
			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				centerPos += curPin.TmpPos_MeshTest - mesh._offsetPos;
			}
			centerPos /= nPins;

			Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0.0f, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;

			for (int iPin = 0; iPin < nPins; iPin++)
			{
				curPin = Editor.Select.MeshPins[iPin];
				posW = curPin.TmpPos_MeshTest - mesh._offsetPos;
				posW = matrix_Rotate.MultiplyPoint(posW);
				curPin.SetTmpPos_MeshTest(posW + mesh._offsetPos);
			}

			//커브 계산
			Editor.Select.Mesh.UpdatePinTestMode(Editor.Select.MeshPin);

			Editor.SetRepaint();

			return true;
		}




		// 피벗
		//----------------------------------------------------------------------------------------------------------
		public apGizmos.TransformParam PivotReturn__MeshPinEdit_Default()
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin				
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return null;
			}

			int nMeshPins = Editor.Select.NumMeshPins;

			if(nMeshPins == 0)
			{
				return null;
			}

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;
			if(nMeshPins == 1 && Editor.Select.MeshPin != null)
			{
				apMeshPin mainMeshPin = Editor.Select.MeshPin;
				return apGizmos.TransformParam.Make(	mainMeshPin._defaultPos - meshOffset,
														0.0f, Vector2.one, 0, Color.white, true,
														apMatrix3x3.TRS(mainMeshPin._defaultPos - meshOffset, 0.0f, Vector2.one), 
														false,
														apGizmos.TRANSFORM_UI.Position2D,
														mainMeshPin._defaultPos, 0.0f, Vector2.one
														);
			}
			else
			{
				Vector2 posCenter = Vector2.zero;
				apMeshPin curPin = null;
				for (int iPin = 0; iPin < nMeshPins; iPin++)
				{
					curPin = Editor.Select.MeshPins[iPin];
					posCenter += curPin._defaultPos - meshOffset;
				}

				posCenter.x /= (float)nMeshPins;
				posCenter.y /= (float)nMeshPins;

				return apGizmos.TransformParam.Make(	posCenter,
														0.0f, Vector2.one, 0, Color.white, true,
														apMatrix3x3.TRS(posCenter, 0.0f, Vector2.one), 
														true,
														apGizmos.TRANSFORM_UI.Position2D,
														posCenter, 0.0f, Vector2.one
														);
			}
		}


		public apGizmos.TransformParam PivotReturn__MeshPinEdit_Test()
		{
			if(Editor.Select.Mesh == null 
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.Pin				
				|| Editor.Select.Mesh._pinGroup == null)
			{
				return null;
			}

			int nMeshPins = Editor.Select.NumMeshPins;

			if(nMeshPins == 0)
			{
				return null;
			}

			Vector2 meshOffset = Editor.Select.Mesh._offsetPos;
			if(nMeshPins == 1 && Editor.Select.MeshPin != null)
			{
				apMeshPin mainMeshPin = Editor.Select.MeshPin;
				return apGizmos.TransformParam.Make(	mainMeshPin.TmpPos_MeshTest - meshOffset,
														0.0f, Vector2.one, 0, Color.white, true,
														apMatrix3x3.TRS(mainMeshPin._defaultPos - meshOffset, 0.0f, Vector2.one), 
														false,
														apGizmos.TRANSFORM_UI.Position2D,
														mainMeshPin._defaultPos, 0.0f, Vector2.one
														);
			}
			else
			{
				Vector2 posCenter = Vector2.zero;
				apMeshPin curPin = null;
				for (int iPin = 0; iPin < nMeshPins; iPin++)
				{
					curPin = Editor.Select.MeshPins[iPin];
					posCenter += curPin.TmpPos_MeshTest - meshOffset;
				}

				posCenter.x /= (float)nMeshPins;
				posCenter.y /= (float)nMeshPins;

				return apGizmos.TransformParam.Make(	posCenter,
														0.0f, Vector2.one, 0, Color.white, true, 
														apMatrix3x3.TRS(posCenter, 0.0f, Vector2.one), 
														true,
														apGizmos.TRANSFORM_UI.Position2D,
														posCenter, 0.0f, Vector2.one
														);
			}
		}
	}
}