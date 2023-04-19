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

#region [미사용 코드] Auto Gen V1에 대한 코드다.
//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System;
//using System.Collections.Generic;

//using AnyPortrait;

//namespace AnyPortrait
//{
//	/// <summary>
//	/// Mesh를 자동으로 생성할 때 ControlPoint를 제어한다.
//	/// </summary>
//	public partial class apGizmoController
//	{
//		// 작성해야하는 함수
//		// Select : int - (Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
//		// Move : void - (Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex)
//		// Rotate : void - (float deltaAngleW)
//		// Scale : void - (Vector2 deltaScaleW)

//		//Transform은 지원하지 않는다.
//		//	TODO : 현재 Transform이 가능한지도 알아야 할 것 같다.
//		// Transform Position : void - (Vector2 pos, int depth)
//		// Transform Rotation : void - (float angle)
//		// Transform Scale : void - (Vector2 scale)
//		// Transform Color : void - (Color color)

//		// Pivot Return : apGizmos.TransformParam - ()

//		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
//		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
//		// FFD Style Transform Start : bool - ()

//		// Vertex 전용 툴
//		// SoftSelection() : bool
//		// PressBlur(Vector2 pos, float tDelta) : bool


//		//21.1.6 : v1.3.0 변경 내역
//		//AutoGen이 V2로 바뀌면서 이 GizmoController가 필요 없어졌다.
//		//대신, Area를 제어하는 이벤트로 활용한다. (이름도 바꾸고 =3=)


//		//----------------------------------------------------------------
//		// Gizmo - Mesh 자동 생성에서의 컨트롤 포인트
//		//----------------------------------------------------------------
//		/// <summary>
//		/// Mesh Edit메뉴에서 Mesh 자동 생성시 [컨트롤 포인트]에 관한 Gizmo Event의 Set이다.
//		/// </summary>
//		/// <returns></returns>
//		public apGizmos.GizmoEventSet GetEventSet_MeshAreaEdit()
//		{
//			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.

//			apGizmos.GizmoEventSet.I.Clear();
//			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__MeshAreaEdit,
//														Unselect__MeshAreaEdit, 
//														Move__MeshAreaEdit, 
//														//Rotate__MeshAutoGen, 
//														null,
//														//Scale__MeshAutoGen, 
//														null,
//														PivotReturn__MeshAutoGen);

//			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	null,
//																null,
//																null,
//																null,
//																null,
//																null,
//																apGizmos.TRANSFORM_UI.None
//																	);

//			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__MeshAutoGen, 
//														null, 
//														null, 
//														null, 
//														null, 
//														null);

//			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__MeshAreaEdit, 
//																null, 
//																null, 
//																null, 
//																null);

//			return apGizmos.GizmoEventSet.I;

//			//이전
//			//return new apGizmos.GizmoEventSet(
//			//	Select__MeshAutoGen,
//			//	Unselect__MeshAutoGen,
//			//	Move__MeshAutoGen,
//			//	Rotate__MeshAutoGen,
//			//	Scale__MeshAutoGen,
//			//	null,
//			//	null,
//			//	null,
//			//	null,
//			//	null,
//			//	null,
//			//	PivotReturn__MeshAutoGen,
//			//	MultipleSelect__MeshAutoGen,
//			//	null,
//			//	null,
//			//	null,
//			//	null,
//			//	null,
//			//	apGizmos.TRANSFORM_UI.None,
//			//	FirstLink__MeshAutoGen,
//			//	null);
//		}

//		//-------------------------------------------------------------------------------------------
//		// First Link
//		public apGizmos.SelectResult FirstLink__MeshAreaEdit()
//		{
//			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//				|| Editor.Select.Mesh == null
//				|| Editor.Select.Mesh.LinkedTextureData == null
//				|| Editor.Select.Mesh.LinkedTextureData._image == null)
//			{
//				return null;
//			}
//			//if(Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh)
//			//{
//			//	return null;
//			//}
//			//if(Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen)
//			//{
//			//	return null;
//			//}

//			if(!Editor.Select.Mesh._isPSDParsed
//				|| !Editor._isMeshEdit_AreaEditing)
//			{
//				return null;
//			}
//			if(Editor._meshEditMode == apEditor.MESH_EDIT_MODE.Setting
//				|| (Editor._meshEditMode == apEditor.MESH_EDIT_MODE.MakeMesh && Editor._meshEditeMode_MakeMesh_Tab == apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen))
//			{
//				//선택 가능
//				//return apGizmos.SelectResult.Main.SetSingle();
//			}
//			else
//			{
//				//선택 불가능
//				return null;
//			}


//			//Debug.LogError("TODO : GUI 이벤트 수정해야한다.");
//			//if(!Editor.MeshGenerator.IsScanned)
//			//{
//			//	return null;
//			//}

//			//return apGizmos.SelectResult.Main.SetMultiple<apMeshGenMapper.ControlPoint>(Editor.MeshGenerator._selectedControlPoints);


//			return null;

//		}



//		// Select
//		public apGizmos.SelectResult Select__MeshAreaEdit(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
//		{
//			//Debug.Log("Select__MeshAutoGen : " + mousePosGL);
//			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//				|| Editor.Select.Mesh == null
//				|| Editor.Select.Mesh.LinkedTextureData == null
//				|| Editor.Select.Mesh.LinkedTextureData._image == null
//				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
//				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen
//				//|| !Editor.MeshGenerator.IsScanned /삭제
//				)
//			{
//				//Debug.LogError("Select__MeshAutoGen : Failed");
//				return null;
//			}

//			//apMesh mesh = Editor.Select.Mesh;
//			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);

//			//List<apMeshGenMapper.ControlPoint> controlPoints = Editor.MeshGenerator.Mapper.ControlPoints;
//			//List<apMeshGenMapper.ControlPoint> selectedControlPoints = Editor.MeshGenerator._selectedControlPoints;



//			//if (!Editor.Controller.IsMouseInGUI(mousePosGL))
//			//{
//			//	//Debug.LogError("Select__MeshAutoGen : Mouse Over");
//			//	return apGizmos.SelectResult.Main.SetMultiple<apMeshGenMapper.ControlPoint>(selectedControlPoints);

//			//}


//			//apMeshGenMapper.ControlPoint curPoint = null;
//			//Vector2 posGL = Vector2.zero;
//			//bool isAnySelect_ControlPoint = false;
//			//for (int iPoint = 0; iPoint < controlPoints.Count; iPoint++)
//			//{
//			//	curPoint = controlPoints[iPoint];

//			//	posGL= apGL.World2GL(curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset));

//			//	if (IsAutoGenMeshControlPointClickable(posGL, mousePosGL, 13))
//			//	{
//			//		if (selectType == apGizmos.SELECT_TYPE.New)
//			//		{
//			//			selectedControlPoints.Clear();
//			//			selectedControlPoints.Add(curPoint);
//			//		}
//			//		else if (selectType == apGizmos.SELECT_TYPE.Add)
//			//		{
//			//			if (!selectedControlPoints.Contains(curPoint))
//			//			{
//			//				selectedControlPoints.Add(curPoint);
//			//			}
//			//		}
//			//		else//if (selectType == apGizmos.SELECT_TYPE.Subtract)
//			//		{
//			//			if(selectedControlPoints.Contains(curPoint))
//			//			{
//			//				selectedControlPoints.Remove(curPoint);
//			//			}
//			//		}
//			//		isAnySelect_ControlPoint = true;

//			//		break;
//			//	}
//			//}


//			//if(isAnySelect_ControlPoint)
//			//{
//			//	//Debug.Log(">>> Select__MeshAutoGen : Is Any Selected");
//			//	Editor.SetRepaint();
//			//}
//			//else
//			//{
//			//	if(selectType == apGizmos.SELECT_TYPE.New)
//			//	{
//			//		selectedControlPoints.Clear();
//			//		//Debug.Log(">>> Select__MeshAutoGen : Unselected");
//			//		Editor.SetRepaint();
//			//	}
//			//}
//			//return apGizmos.SelectResult.Main.SetMultiple<apMeshGenMapper.ControlPoint>(selectedControlPoints);


//			//TODO

//			return null;
//		}


//		private bool IsAutoGenMeshControlPointClickable(Vector2 controlPointPos, Vector2 mousePos, float dist)
//		{
//			if(!Editor.Controller.IsMouseInGUI(controlPointPos))
//			{
//				return false;
//			}

//			Vector2 difPos = controlPointPos - mousePos;
//			if(Mathf.Abs(difPos.x) < dist && Mathf.Abs(difPos.y) < dist)
//			{
//				return true;
//			}
//			return false;
//		}


//		public void Unselect__MeshAreaEdit()
//		{
//			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//				|| Editor.Select.Mesh == null
//				|| Editor.Select.Mesh.LinkedTextureData == null
//				|| Editor.Select.Mesh.LinkedTextureData._image == null
//				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
//				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen
//				//|| !Editor.MeshGenerator.IsScanned
//				)
//			{
//				return;
//			}

//			//TODO

//			//Editor.MeshGenerator._selectedControlPoints.Clear();
//			Editor.SetRepaint();
//		}


//		//public apGizmos.SelectResult MultipleSelect__MeshAutoGen(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
//		//{
//		//	if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//		//		|| Editor.Select.Mesh == null
//		//		|| Editor.Select.Mesh.LinkedTextureData == null
//		//		|| Editor.Select.Mesh.LinkedTextureData._image == null
//		//		|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
//		//		|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen
//		//		//|| !Editor.MeshGenerator.IsScanned
//		//		)
//		//	{
//		//		return null;
//		//	}

//		//	//apMesh mesh = Editor.Select.Mesh;
//		//	//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);

//		//	//List<apMeshGenMapper.ControlPoint> controlPoints = Editor.MeshGenerator.Mapper.ControlPoints;
//		//	//List<apMeshGenMapper.ControlPoint> selectedControlPoints = Editor.MeshGenerator._selectedControlPoints;

//		//	////if (!Editor.Controller.IsMouseInGUI(mousePosGL))
//		//	////{
//		//	////	return apGizmos.SelectResult.Main.SetMultiple<apMeshGenMapper.ControlPoint>(selectedControlPoints);
//		//	////}

//		//	//if(areaSelectType == apGizmos.SELECT_TYPE.New)
//		//	//{
//		//	//	selectedControlPoints.Clear();
//		//	//}
//		//	//apMeshGenMapper.ControlPoint curPoint = null;
//		//	//Vector2 posW = Vector2.zero;
//		//	//bool isAnyChanged = false;
//		//	//for (int iPoint = 0; iPoint < controlPoints.Count; iPoint++)
//		//	//{
//		//	//	curPoint = controlPoints[iPoint];

//		//	//	posW = curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset);

//		//	//	bool isSelectable = (mousePosW_Min.x < posW.x && posW.x < mousePosW_Max.x)
//		//	//						&& (mousePosW_Min.y < posW.y && posW.y < mousePosW_Max.y);

//		//	//	if(isSelectable)
//		//	//	{
//		//	//		switch (areaSelectType)
//		//	//		{
//		//	//			case apGizmos.SELECT_TYPE.New:
//		//	//			case apGizmos.SELECT_TYPE.Add:
//		//	//				if (!selectedControlPoints.Contains(curPoint))
//		//	//				{
//		//	//					selectedControlPoints.Add(curPoint);
//		//	//				}
//		//	//				break;

//		//	//			case apGizmos.SELECT_TYPE.Subtract:
//		//	//				if(selectedControlPoints.Contains(curPoint))
//		//	//				{
//		//	//					selectedControlPoints.Remove(curPoint);
//		//	//				}
//		//	//				break;

//		//	//		}

//		//	//		isAnyChanged = true;
//		//	//	}

//		//	//}

//		//	//if(isAnyChanged)
//		//	//{
//		//	//	Editor.SetRepaint();
//		//	//}
//		//	//else
//		//	//{
//		//	//	if(areaSelectType == apGizmos.SELECT_TYPE.New)
//		//	//	{
//		//	//		selectedControlPoints.Clear();
//		//	//		Editor.SetRepaint();
//		//	//	}
//		//	//}
//		//	//return apGizmos.SelectResult.Main.SetMultiple<apMeshGenMapper.ControlPoint>(selectedControlPoints);

//		//	//TODO

//		//	return null;
//		//}

//		public void Move__MeshAutoGen(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
//		{
//			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//				|| Editor.Select.Mesh == null
//				|| Editor.Select.Mesh.LinkedTextureData == null
//				|| Editor.Select.Mesh.LinkedTextureData._image == null
//				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
//				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen
//				//|| !Editor.MeshGenerator.IsScanned
//				)
//			{
//				return;
//			}

//			//apMesh mesh = Editor.Select.Mesh;
//			////Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);

//			//List<apMeshGenMapper.ControlPoint> controlPoints = Editor.MeshGenerator.Mapper.ControlPoints;
//			//List<apMeshGenMapper.ControlPoint> selectedControlPoints = Editor.MeshGenerator._selectedControlPoints;

//			//apMeshGenMapper.ControlPoint curPoint = null;

//			//TODO


//			//for (int i = 0; i < selectedControlPoints.Count; i++)
//			//{
//			//	curPoint = selectedControlPoints[i];
//			//	curPoint._pos_Cur += deltaMoveW;
//			//}

//			Editor.SetRepaint();
//		}


//		//public void Rotate__MeshAutoGen(float deltaAngleW, bool isFirstRotate)
//		//{
//		//	if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//		//		|| Editor.Select.Mesh == null
//		//		|| Editor.Select.Mesh.LinkedTextureData == null
//		//		|| Editor.Select.Mesh.LinkedTextureData._image == null
//		//		|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
//		//		|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen
//		//		//|| !Editor.MeshGenerator.IsScanned
//		//		)
//		//	{
//		//		return;
//		//	}

//		//	//apMesh mesh = Editor.Select.Mesh;
//		//	//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);

//		//	////List<apMeshGenMapper.ControlPoint> controlPoints = Editor.MeshGenerator.Mapper.ControlPoints;
//		//	//List<apMeshGenMapper.ControlPoint> selectedControlPoints = Editor.MeshGenerator._selectedControlPoints;
//		//	//if(selectedControlPoints.Count <= 1)
//		//	//{
//		//	//	return;
//		//	//}

//		//	//apMeshGenMapper.ControlPoint curPoint = null;

//		//	//Vector2 posW = Vector2.zero;
//		//	//Vector2 centerPos = Vector2.zero;
//		//	//for (int i = 0; i < selectedControlPoints.Count; i++)
//		//	//{
//		//	//	curPoint = selectedControlPoints[i];
//		//	//	posW = curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset);
//		//	//	centerPos += posW;

//		//	//}
//		//	//centerPos /= selectedControlPoints.Count;

//		//	//if (deltaAngleW > 180.0f)
//		//	//{ deltaAngleW -= 360.0f; }
//		//	//else if (deltaAngleW < -180.0f)
//		//	//{ deltaAngleW += 360.0f; }

//		//	//apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
//		//	//	* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
//		//	//	* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


//		//	//for (int i = 0; i < selectedControlPoints.Count; i++)
//		//	//{
//		//	//	curPoint = selectedControlPoints[i];
//		//	//	posW = curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset);
//		//	//	posW = matrix_Rotate.MultiplyPoint(posW);
//		//	//	curPoint._pos_Cur = posW + (mesh._offsetPos + imageHalfOffset);
//		//	//}

//		//	//TODO

//		//	Editor.SetRepaint();
//		//}


//		//public void Scale__MeshAutoGen(Vector2 deltaScaleW, bool isFirstScale)
//		//{
//		//	if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//		//		|| Editor.Select.Mesh == null
//		//		|| Editor.Select.Mesh.LinkedTextureData == null
//		//		|| Editor.Select.Mesh.LinkedTextureData._image == null
//		//		|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
//		//		|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen
//		//		//|| !Editor.MeshGenerator.IsScanned
//		//		)
//		//	{
//		//		return;
//		//	}

//		//	//apMesh mesh = Editor.Select.Mesh;
//		//	//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);

//		//	////List<apMeshGenMapper.ControlPoint> controlPoints = Editor.MeshGenerator.Mapper.ControlPoints;
//		//	//List<apMeshGenMapper.ControlPoint> selectedControlPoints = Editor.MeshGenerator._selectedControlPoints;
//		//	//if(selectedControlPoints.Count <= 1)
//		//	//{
//		//	//	return;
//		//	//}

//		//	//apMeshGenMapper.ControlPoint curPoint = null;

//		//	//Vector2 posW = Vector2.zero;
//		//	//Vector2 centerPos = Vector2.zero;
//		//	//for (int i = 0; i < selectedControlPoints.Count; i++)
//		//	//{
//		//	//	curPoint = selectedControlPoints[i];
//		//	//	posW = curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset);
//		//	//	centerPos += posW;

//		//	//}
//		//	//centerPos /= selectedControlPoints.Count;

//		//	//Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

//		//	//apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
//		//	//	* apMatrix3x3.TRS(Vector2.zero, 0, scale)
//		//	//	* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


//		//	//for (int i = 0; i < selectedControlPoints.Count; i++)
//		//	//{
//		//	//	curPoint = selectedControlPoints[i];
//		//	//	posW = curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset);
//		//	//	posW = matrix_Scale.MultiplyPoint(posW);
//		//	//	curPoint._pos_Cur = posW + (mesh._offsetPos + imageHalfOffset);
//		//	//}

//		//	//TODO

//		//	Editor.SetRepaint();
//		//}


//		public apGizmos.TransformParam PivotReturn__MeshAutoGen()
//		{
//			if(Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
//				|| Editor.Select.Mesh == null
//				|| Editor.Select.Mesh.LinkedTextureData == null
//				|| Editor.Select.Mesh.LinkedTextureData._image == null
//				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
//				|| Editor._meshEditeMode_MakeMesh_Tab != apEditor.MESH_EDIT_MODE_MAKEMESH_TAB.AutoGen
//				//|| !Editor.MeshGenerator.IsScanned
//				)
//			{
//				//Debug.LogError("Pivot Failed");
//				return null;
//			}

//			//return apGizmos.TransformParam.Make(	posW, 0.0f, Vector2.one,
//			//											0, Color.black,
//			//											true,
//			//											apMatrix3x3.identity,
//			//											false,
//			//											apGizmos.TRANSFORM_UI.Position2D,
//			//											Vector2.zero, 0.0f, Vector2.one);
//			return null;
//			//List<apMeshGenMapper.ControlPoint> selectedControlPoints = Editor.MeshGenerator._selectedControlPoints;

//			//if(selectedControlPoints.Count == 0)
//			//{
//			//	//Debug.LogError("Pivot Failed : Count 0");
//			//	return null;
//			//}

//			//apMesh mesh = Editor.Select.Mesh;
//			//Vector2 imageHalfOffset = new Vector2(mesh.LinkedTextureData._width * 0.5f, mesh.LinkedTextureData._height * 0.5f);

//			//apMeshGenMapper.ControlPoint curPoint = null;
//			//Vector2 posW = Vector2.zero;
//			//if(selectedControlPoints.Count == 1)
//			//{
//			//	curPoint = selectedControlPoints[0];
//			//	posW = curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset);

//			//	return apGizmos.TransformParam.Make(	posW, 0.0f, Vector2.one, 
//			//											0, Color.black, 
//			//											true, 
//			//											apMatrix3x3.identity, 
//			//											false, 
//			//											apGizmos.TRANSFORM_UI.TRS_NoDepth, 
//			//											Vector2.zero, 0.0f, Vector2.one);
//			//}
//			//else
//			//{
//			//	Vector2 centerPos = Vector2.zero;

//			//	for (int i = 0; i < selectedControlPoints.Count; i++)
//			//	{
//			//		curPoint = selectedControlPoints[i];
//			//		posW = curPoint._pos_Cur - (mesh._offsetPos + imageHalfOffset);
//			//		centerPos += posW;
//			//	}
//			//	centerPos /= selectedControlPoints.Count;
//			//	//Debug.Log("Pivot : Multiple " + centerPos + " (" + selectedControlPoints.Count + ")");
//			//	return apGizmos.TransformParam.Make(	centerPos, 0.0f, Vector2.one,
//			//											0, Color.black, true,
//			//											apMatrix3x3.identity, true, apGizmos.TRANSFORM_UI.TRS_NoDepth,
//			//											Vector2.zero, 0.0f, Vector2.one);
//			//}

//			//TODO

//		}
//	}
//} 
#endregion