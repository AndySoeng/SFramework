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
	/// <summary>
	/// Mesh를 자동으로 생성할 때 ControlPoint를 제어한다.
	/// </summary>
	public partial class apGizmoController
	{
		// 작성해야하는 함수
		// Select : int - (Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		// Move : void - (Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex)
		// Rotate : void - (float deltaAngleW)
		// Scale : void - (Vector2 deltaScaleW)

		//Transform은 지원하지 않는다.
		//	TODO : 현재 Transform이 가능한지도 알아야 할 것 같다.
		// Transform Position : void - (Vector2 pos, int depth)
		// Transform Rotation : void - (float angle)
		// Transform Scale : void - (Vector2 scale)
		// Transform Color : void - (Color color)

		// Pivot Return : apGizmos.TransformParam - ()

		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
		// FFD Style Transform Start : bool - ()

		// Vertex 전용 툴
		// SoftSelection() : bool
		// PressBlur(Vector2 pos, float tDelta) : bool
		
		//----------------------------------------------------------------
		// Gizmo - Mesh TRS에서 버텍스를 선택하고 이동한다.
		//----------------------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_MeshTRS()
		{
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__MeshTRS,
														Unselect__MeshTRS, 
														Move__MeshTRS, 
														Rotate__MeshTRS, 
														Scale__MeshTRS, 
														PivotReturn__MeshTRS);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__MeshTRS,
																null,
																null,
																null,
																null,
																null,
																apGizmos.TRANSFORM_UI.Position2D 
																	| apGizmos.TRANSFORM_UI.Vertex_Transform
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__MeshTRS, 
														FFDTransform__MeshTRS, 
														StartFFDTransform__MeshTRS, 
														null, 
														null, 
														null);
			
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__MeshTRS, 
																AddHotKeys__MeshTRS, 
																OnHotKeyEvent__MeshTRS__Keyboard_Move, 
																OnHotKeyEvent__MeshTRS__Keyboard_Rotate, 
																OnHotKeyEvent__MeshTRS__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;

			//이전
			//return new apGizmos.GizmoEventSet(
			//	Select__MeshTRS,
			//	Unselect__MeshTRS,
			//	Move__MeshTRS,
			//	Rotate__MeshTRS,
			//	Scale__MeshTRS,
			//	TransformChanged_Position__MeshTRS,
			//	null, null, null, null, null,
			//	PivotReturn__MeshTRS,
			//	MultipleSelect__MeshTRS,
			//	FFDTransform__MeshTRS,
			//	StartFFDTransform__MeshTRS,
			//	null,
			//	null,
			//	null,
			//	apGizmos.TRANSFORM_UI.Position2D | apGizmos.TRANSFORM_UI.Vertex_Transform,
			//	FirstLink__MeshTRS,
			//	AddHotKeys__MeshTRS
			//	);
		}


		//-------------------------------------------------------------------------------------------
		// First Link
		public apGizmos.SelectResult FirstLink__MeshTRS()
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null)
			{
				return null;
			}
			if(Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh)
			{
				return null;
			}
			if(Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return null;
			}

			return apGizmos.SelectResult.Main.SetMultiple<apVertex>(Editor.VertController.Vertices);
		}


		// Select
		public apGizmos.SelectResult Select__MeshTRS(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				//Debug.LogWarning("Select Failed");
				return null;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			if(vertices.Count == 0)
			{
				//Debug.LogWarning("Vert Count 0");
				return null;
			}
			
			

			apVertex curVert = null;

			//변경 22.4.9 : 버텍스 선택 로직 개선
			apVertex clickedVert_Direct = null;
			apVertex clickedVert_Wide = null;
			float minWideDist = 0.0f;
			float curWideDist = 0.0f;
			apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

			for (int iVert = 0; iVert < vertices.Count; iVert++)
			{
				curVert = vertices[iVert];

				//클릭할 수 있는가
				Vector2 vertPosGL = apGL.World2GL(curVert._pos - mesh._offsetPos);
				clickResult = Editor.Controller.IsVertexClickable(ref vertPosGL, ref mousePosGL, ref curWideDist);

				if(clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
				{
					//클릭되지 않음
					continue;
				}

				if(clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
				{
					//정확히 클릭했다.
					clickedVert_Direct = curVert;
					break;
				}

				//Wide 클릭 : 거리 비교
				if(clickedVert_Wide == null || curWideDist < minWideDist)
				{
					clickedVert_Wide = curVert;
					minWideDist = curWideDist;
				}
				
			}

			//클릭 결과
			apVertex resultClicked = null;
			if(clickedVert_Direct != null)		{ resultClicked = clickedVert_Direct; }
			else if(clickedVert_Wide != null)	{ resultClicked = clickedVert_Wide; }


			if (resultClicked != null)
			{
				List<apVertex> tmpVertex = new List<apVertex>();
				tmpVertex.Add(resultClicked);

				Editor.VertController.SelectVertices(tmpVertex, selectType);
				Editor.SetRepaint();
			}
			else if(selectType == apGizmos.SELECT_TYPE.New)
			{
				Editor.VertController.UnselectVertex();
				Editor.SetRepaint();
			}

			
			return apGizmos.SelectResult.Main.SetMultiple<apVertex>(Editor.VertController.Vertices);
		}


		// Unselect
		public void Unselect__MeshTRS()
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return;
			}

			if(Editor.Gizmos.IsFFDMode)
			{
				//FFD 모드에서는 버텍스 취소가 안된다.
				return;
			}

			Editor.VertController.UnselectVertex();
		}

		//---------------------------------------------------------------------------------------------
		// 단축키
		//---------------------------------------------------------------------------------------------
		public void AddHotKeys__MeshTRS(bool isGizmoRenderable, apGizmos.CONTROL_TYPE controlType, bool isFFDMode)
		{
			//Editor.AddHotKeyEvent(OnHotKeyEvent__MeshTRS__Ctrl_A, apHotKey.LabelText.SelectAllVertices, KeyCode.A, false, false, true, null);
			Editor.AddHotKeyEvent(OnHotKeyEvent__MeshTRS__Ctrl_A, apHotKeyMapping.KEY_TYPE.MakeMesh_SelectAllVertices, null);//변경 20.12.3
		}

		// 단축키 : 버텍스 전체 선택
		private apHotKey.HotKeyResult OnHotKeyEvent__MeshTRS__Ctrl_A(object paramObject)
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
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

			apMesh mesh = Editor.Select.Mesh;

			List<apVertex> vertices = mesh._vertexData;
			if(vertices.Count == 0)
			{
				return null;
			}
			//전체 선택
			Editor.VertController.SelectVertices(vertices, apGizmos.SELECT_TYPE.Add);
			Editor.SetRepaint();

			Editor.Gizmos.SetSelectResultForce_Multiple<apVertex>(Editor.VertController.Vertices);

			return apHotKey.HotKeyResult.MakeResult();
		}


		// 키보드 TRS 단축키
		private bool OnHotKeyEvent__MeshTRS__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			//Move__MeshTRS 함수의 코드를 이용

			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return false;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//Vector2 prevPos = Editor.VertController.Vertex._pos;

			apVertex curVert = null;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				curVert._pos += deltaMoveW;
			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}
			
			Editor.SetRepaint();

			return true;
		}

		private bool OnHotKeyEvent__MeshTRS__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__MeshTRS 함수의 코드를 이용

			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return false;
			}
			

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apVertex curVert = null;
			Vector2 centerPos = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				centerPos += curVert._pos - mesh._offsetPos;
			}
			centerPos /= selectedVertices.Count;

			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				posW = curVert._pos - (mesh._offsetPos);
				posW = matrix_Rotate.MultiplyPoint(posW);
				curVert._pos = posW + (mesh._offsetPos);
			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}

			Editor.SetRepaint();

			return true;
		}

		private bool OnHotKeyEvent__MeshTRS__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__MeshTRS 함수의 코드를 이용

			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return false;
			}
			
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apVertex curVert = null;
			Vector2 centerPos = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				centerPos += curVert._pos - mesh._offsetPos;
			}
			centerPos /= selectedVertices.Count;

			Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0.0f, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				posW = curVert._pos - (mesh._offsetPos);
				posW = matrix_Rotate.MultiplyPoint(posW);
				curVert._pos = posW + (mesh._offsetPos);
			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}

			Editor.SetRepaint();


			return true;
		}

		//---------------------------------------------------------------------------------------
		public apGizmos.SelectResult MultipleSelect__MeshTRS(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return null;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			if(vertices.Count == 0)
			{
				return null;
			}
			
			List<apVertex> tmpVertex = null;

			apVertex curVert = null;
			Vector2 vertPos = Vector2.zero;
			for (int iVert = 0; iVert < vertices.Count; iVert++)
			{
				curVert = vertices[iVert];

				//클릭할 수 있는가
				//vertPos = curVert._pos - (mesh._offsetPos + imageHalfOffset);
				vertPos = curVert._pos - (mesh._offsetPos);

				bool isSelectable = (mousePosW_Min.x < vertPos.x && vertPos.x < mousePosW_Max.x)
									&& (mousePosW_Min.y < vertPos.y && vertPos.y < mousePosW_Max.y);

				if (isSelectable)						
				{
					//일단 리스트에 추가
					if(tmpVertex == null)
					{
						tmpVertex = new List<apVertex>();
					}
					tmpVertex.Add(curVert);
				}
			}
			if(tmpVertex != null)
			{
				Editor.VertController.SelectVertices(tmpVertex, areaSelectType);
				Editor.SetRepaint();
			}
			else if(areaSelectType == apGizmos.SELECT_TYPE.New)
			{
				Editor.VertController.UnselectVertex();
				Editor.SetRepaint();
			}

			return apGizmos.SelectResult.Main.SetMultiple<apVertex>(Editor.VertController.Vertices);
		}

		//-----------------------------------------------------------------------------------

		// Move
		public void Move__MeshTRS(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			if(deltaMoveW.sqrMagnitude == 0.0f)
			{
				return;
			}

			

			//Vector2 prevPos = Editor.VertController.Vertex._pos;

			apVertex curVert = null;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				curVert._pos += deltaMoveW;
			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}
			

			
			//TODO : 나중에 미러 동기화 처리하자
			//if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
			//{
			//	//Debug.Log("Mirror");
			//	Editor.MirrorSet.MoveMirrorVertex(Editor.VertController.Vertex, prevPos, Editor.VertController.Vertex._pos, Editor.Select.Mesh);
			//	Editor.MirrorSet.RefreshMeshWork(Editor.Select.Mesh, Editor.VertController);
			//}
			Editor.SetRepaint();
		}



		// Rotate
		public void Rotate__MeshTRS(float deltaAngleW, bool isFirstRotate)
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return;
			}
			

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if(Mathf.Abs(deltaAngleW) == 0.0f)
			{
				return;
			}

			apVertex curVert = null;
			Vector2 centerPos = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				centerPos += curVert._pos - mesh._offsetPos;
			}
			centerPos /= selectedVertices.Count;

			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				posW = curVert._pos - (mesh._offsetPos);
				posW = matrix_Rotate.MultiplyPoint(posW);
				curVert._pos = posW + (mesh._offsetPos);
			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}

			Editor.SetRepaint();
		}


		// Scale
		public void Scale__MeshTRS(Vector2 deltaScaleW, bool isFirstScale)
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return;
			}
			
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apVertex curVert = null;
			Vector2 centerPos = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				centerPos += curVert._pos - mesh._offsetPos;
			}
			centerPos /= selectedVertices.Count;

			Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0.0f, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			Vector2 posW = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				posW = curVert._pos - (mesh._offsetPos);
				posW = matrix_Rotate.MultiplyPoint(posW);
				curVert._pos = posW + (mesh._offsetPos);
			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}

			Editor.SetRepaint();
		}


		//------------------------------------------------------------------------------------

		// Transform Changed (Pos)
		public void TransformChanged_Position__MeshTRS(Vector2 pos)
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return;
			}

			if(Editor.Gizmos.IsFFDMode)
			{
				//FFD 모드에서는 처리가 안된다.
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return;
			}
			
			apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_EditVertex, 
											Editor, 
											mesh, 
											//mesh, 
											true,
											apEditorUtil.UNDO_STRUCT.ValueOnly);
			
			apVertex curVert = null;
			Vector2 centerPos = Vector2.zero;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				centerPos += curVert._pos - mesh._offsetPos;
			}
			centerPos /= selectedVertices.Count;

			pos -= mesh._offsetPos;

			Vector2 deltaMoveW = pos - centerPos;
			//Debug.Log("Input : " + pos + " / Cur Center : " + centerPos + " >> Delta : " + deltaMoveW);
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				curVert = selectedVertices[i];
				curVert._pos += deltaMoveW;
			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}
			
			Editor.SetRepaint();
		}


		//-----------------------------------------------------------------------------------------

		// Pivot
		public apGizmos.TransformParam PivotReturn__MeshTRS()
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return null;
			}
			if(Editor.VertController.Vertex == null)
			{
				return null;
			}

			apMesh mesh = Editor.Select.Mesh;
			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);

			if(Editor.VertController.Vertices.Count == 1)
			{
				//1개 선택되었다.
				apVertex vert = Editor.VertController.Vertices[0];

				return apGizmos.TransformParam.Make(vert._pos - (mesh._offsetPos),
					0.0f, Vector2.one, 0, Color.black,
					true,
					apMatrix3x3.identity,
					false,
					apGizmos.TRANSFORM_UI.Position2D, vert._pos, 0.0f, Vector2.one);
			}
			else if(Editor.VertController.Vertices.Count > 1)
			{
				apVertex vert = null;
				Vector2 centerPos = Vector2.zero;
				int nVert = Editor.VertController.Vertices.Count;
				for (int i = 0; i < Editor.VertController.Vertices.Count; i++)
				{
					vert = Editor.VertController.Vertices[i];
					centerPos += vert._pos;
				}

				centerPos /= nVert;

				return apGizmos.TransformParam.Make(centerPos - mesh._offsetPos,
					0.0f, Vector2.one, 0, Color.black,
					true,
					apMatrix3x3.identity,
					true,
					apGizmos.TRANSFORM_UI.Position2D | apGizmos.TRANSFORM_UI.Vertex_Transform, centerPos, 0.0f, Vector2.one);
			}
			return null;
		}


		//-----------------------------------------------------------------------------------------

		// FFD Start
		public bool StartFFDTransform__MeshTRS()
		{
			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
			{
				return false;
			}

			apMesh mesh = Editor.Select.Mesh;
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if(vertices.Count == 0 || selectedVertices.Count == 0)
			{
				return false;
			}

			List<object> srcObjectList = new List<object>();
			List<Vector2> worldPosList = new List<Vector2>();
			List<Vector2> orgPosList = new List<Vector2>();
			apVertex vert = null;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				vert = selectedVertices[i];
				srcObjectList.Add(vert);
				worldPosList.Add(vert._pos - mesh._offsetPos);
				orgPosList.Add(vert._pos);//추가 20.7.22
			}
			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList, orgPosList);//<<True로 리턴할거면 이 함수를 호출해주자
			return true;
		}

		// FFD Process
		public bool FFDTransform__MeshTRS(List<object> srcObjects, List<Vector2> posData, apGizmos.FFD_ASSIGN_TYPE assignType, bool isResultAssign, bool isRecord)
		{
			if(!isResultAssign)
			{
				//결과 적용이 아닌 일반 수정 작업시
				//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.

				if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.TRS)
				{
					return false;
				}

				if(Editor.Select.Mesh._vertexData.Count == 0 || Editor.VertController.Vertices.Count == 0)
				{
					return false;
				}
			}

			apMesh mesh = Editor.Select.Mesh;
			//List<apVertex> selectedVertices = Editor.VertController.Vertices;

			if (isRecord)
			{
				apEditorUtil.SetRecord_Mesh(	apUndoGroupData.ACTION.MeshEdit_VertexMoved, 
												Editor, 
												mesh, 
												//mesh, 
												true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apVertex vert = null;
			if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
			{
				//World Pos를 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					vert = srcObjects[i] as apVertex;
					if (vert == null)
					{
						continue;
					}
					Vector2 worldPos = posData[i];
					vert._pos = worldPos + mesh._offsetPos;
					mesh.RefreshVertexAutoUV(vert);
				}
			}
			else//if (assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
			{
				//저장된 데이터를 직접 대입하는 경우
				for (int i = 0; i < srcObjects.Count; i++)
				{
					vert = srcObjects[i] as apVertex;
					if (vert == null)
					{
						continue;
					}
					vert._pos = posData[i];
					mesh.RefreshVertexAutoUV(vert);
				}
			}
			

			return true;
		}

		
	}
}