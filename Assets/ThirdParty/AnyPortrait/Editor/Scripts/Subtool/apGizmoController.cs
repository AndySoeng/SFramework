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
		// Member
		//--------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }


		// Init
		//--------------------------------------------------
		public apGizmoController()
		{

		}

		public void SetEditor(apEditor editor)
		{
			_editor = editor;

		}




		// Gizmo - MeshGroup : Setting
		//--------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_MeshGroupSetting()
		{
			//MeshGroup 내의 Mesh의 기본 위치를 바꾼다.
			//다중 선택과 FFD Transform이 제한된다. (null...)
			//변경 20.1.27
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__MeshGroup_Setting,
														Unselect__MeshGroup_Setting, 
														Move__MeshGroup_Setting, 
														Rotate__MeshGroup_Setting, 
														Scale__MeshGroup_Setting, 
														PivotReturn__MeshGroup_Setting);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__MeshGroup_Setting,
																TransformChanged_Rotate__MeshGroup_Setting,
																TransformChanged_Scale__MeshGroup_Setting,
																TransformChanged_Depth__MeshGroup_Setting,
																TransformChanged_Color__MeshGroup_Setting,
																null,
																apGizmos.TRANSFORM_UI.TRS_WithDepth | apGizmos.TRANSFORM_UI.Color);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(null, null, null, null, null, null);
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(FirstLink__MeshGroup_Setting, null, 
				OnHotKeyEvent__MeshGroup_Setting__Keyboard_Move, 
				OnHotKeyEvent__MeshGroup_Setting__Keyboard_Rotate, 
				OnHotKeyEvent__MeshGroup_Setting__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;

			//이전
			//return new apGizmos.GizmoEventSet(
			//	Select__MeshGroup_Setting,
			//	Unselect__MeshGroup_Setting,
			//	Move__MeshGroup_Setting,
			//	Rotate__MeshGroup_Setting,
			//	Scale__MeshGroup_Setting,
			//	TransformChanged_Position__MeshGroup_Setting,
			//	TransformChanged_Rotate__MeshGroup_Setting,
			//	TransformChanged_Scale__MeshGroup_Setting,
			//	TransformChanged_Depth__MeshGroup_Setting,
			//	TransformChanged_Color__MeshGroup_Setting,
			//	null,
			//	PivotReturn__MeshGroup_Setting,
			//	null,
			//	null,
			//	null,
			//	null,
			//	null,
			//	null,
			//	apGizmos.TRANSFORM_UI.TRS_WithDepth | apGizmos.TRANSFORM_UI.Color,
			//	FirstLink__MeshGroup_Setting,
			//	null);
		}

		public apGizmos.SelectResult FirstLink__MeshGroup_Setting()
		{
			if (Editor.Select.MeshGroup == null)
			{
				return null;
			}

			//이전
			//if (Editor.Select.SubMeshGroupTransform != null)
			//{
			//	return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshGroupTransform);
			//}
			//if (Editor.Select.SubMeshTransform != null)
			//{
			//	return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubMeshTransform);
			//}

			//변경 20.6.17 : 다중 선택 인식? > 일단 기즈모에서는 메인만 인식하자
			//if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshGroupTF_Main);
			//}
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshTF_Main);
			//}

			//>>>> [GizmoMain]
			//변경2 20.7.18 : Gizmo Main으로 변경 
			if (Editor.Select.SubObjects.GizmoMeshTF != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubObjects.GizmoMeshTF);
			}
			if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubObjects.GizmoMeshGroupTF);
			}


			return null;
		}

		public apGizmos.SelectResult Select__MeshGroup_Setting(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null)
			{
				return null;
			}

			apTransform_MeshGroup prevSelectedMeshGroupTransform = Editor.Select.MeshGroupTF_Main;

			//object resultObj = null;//기존


			apSelection.MULTI_SELECT multiSelect = (selectType == apGizmos.SELECT_TYPE.Add) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main;

			if (Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				apTransform_Mesh selectedMeshTransform = null;

				//정렬된 Render Unit
				//List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<이전 : RenderUnits_All 이용
				List<apRenderUnit> renderUnits = Editor.Select.MeshGroup.SortedRenderUnits;//<<변경 : Sorted 리스트 이용

				//이전
				//for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)

				//변경 20.5.27 : 루프순서를 바꿔서 앞부터 체크하자
				if (renderUnits.Count > 0)
				{
					for (int iUnit = renderUnits.Count - 1; iUnit >= 0; iUnit--)
					{
						apRenderUnit renderUnit = renderUnits[iUnit];
						if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
						{
							//if (renderUnit._meshTransform._isVisible_Default)//이전
							if (renderUnit._isVisible)//변경 21.7.20
							{
								bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(
									mousePosGL, renderUnit);

								if (isPick)
								{
									selectedMeshTransform = renderUnit._meshTransform;
									//찾았어도 계속 찾는다. 뒤의 아이템이 "앞쪽"에 있는 것이기 때문

									//변경 : 체크 순서가 바뀌었으므로 피킹 후 바로 Break
									break;
								}
							}
						}
					}
				}

				if (selectedMeshTransform != null)
				{
					//수정된 버전
					//>> 그냥 MeshGroup Transform은 마우스로 선택 못하는 걸로 하자
					Editor.Select.SelectMeshTF(selectedMeshTransform, multiSelect);//다중 선택 가능

					//resultObj = selectedMeshTransform;//기존

					//[1.4.2] 선택된 객체에 맞게 자동 스크롤
					if(Editor._option_AutoScrollWhenObjectSelected)
					{
						//스크롤 가능한 상황인지 체크하고
						if(Editor.IsAutoScrollableWhenClickObject_MeshGroup(selectedMeshTransform, true))
						{
							//자동 스크롤을 요청한다.
							Editor.AutoScroll_HierarchyMeshGroup(selectedMeshTransform);
						}
					}
				}
				else
				{
					if(multiSelect == apSelection.MULTI_SELECT.Main)
					{
						//메인 선택시 > 선택이 안되었을때
						Editor.Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
					}
					
				}

				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			//기존
			//return result;
			//return apGizmos.SelectResult.Main.SetSingle(resultObj);

			//변경 20.5.27 : 이걸로 변경해보자
			//if(Editor.Select.MeshTF_Main != null)
			//{
			//	return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshTF_Main);
			//}
			//if(Editor.Select.MeshGroupTF_Main != null)
			//{
			//	return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshGroupTF_Main);
			//}


			//>> [GizmoMain] >>
			//20.7.18 기즈모 메인으로 변경
			if (Editor.Select.SubObjects.GizmoMeshTF != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubObjects.GizmoMeshTF);
			}
			if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.SubObjects.GizmoMeshGroupTF);
			}

			return null;
		}


		public void Unselect__MeshGroup_Setting()
		{
			if (Editor.Select.MeshGroup == null)
			{
				return;
			}

			Editor.Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}


		public void Move__MeshGroup_Setting(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return;
			}

			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			apMatrix targetMatrix = null;
			//object targetObj = null;//<삭제 21.6.30>
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			bool isRootMeshGroup = false;

			//Modifier가 적용이 안된 상태이므로
			//World Matrix = ParentWorld x ToParent(Default) 가 성립한다.


			#region [미사용 코드] 단일 처리
			//이전 : 단일
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	targetMatrix = Editor.Select.MeshTF_Main._matrix;//=ToParent
			//	targetObj = Editor.Select.MeshTF_Main;
			//	worldMatrix = new apMatrix(Editor.Select.MeshTF_Main._matrix_TFResult_World);
			//	parentWorldMatrix = Editor.Select.MeshTF_Main._matrix_TF_ParentWorld;

			//	isRootMeshGroup = Editor.Select.MeshGroup._childMeshTransforms.Contains(Editor.Select.MeshTF_Main);
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	targetMatrix = Editor.Select.MeshGroupTF_Main._matrix;//=ToParent
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//	worldMatrix = new apMatrix(Editor.Select.MeshGroupTF_Main._matrix_TFResult_World);
			//	parentWorldMatrix = Editor.Select.MeshGroupTF_Main._matrix_TF_ParentWorld;

			//	isRootMeshGroup = false;
			//}
			//else
			//{
			//	return;
			//}

			//worldMatrix._pos += deltaMoveW;
			//worldMatrix.MakeMatrix();
			//worldMatrix.RInverse(parentWorldMatrix);//ParentWorld-1 x World = ToParent


			//Vector2 newLocalPos = worldMatrix._pos;


			////Undo
			//if (isFirstMove)
			//{
			//	apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, Editor, Editor.Select.MeshGroup, targetObj, false, !isRootMeshGroup);
			//}

			////targetMatrix.SetPos(targetMatrix._pos.x + deltaMoveW.x, targetMatrix._pos.y + deltaMoveW.y);
			//targetMatrix.SetPos(newLocalPos.x, newLocalPos.y);
			//targetMatrix.MakeMatrix();

			//Editor.RefreshControllerAndHierarchy(); 
			#endregion


			//변경 20.6.20 : 다중 처리
			//단순히 전체 처리를 하면 안된다.
			//만약 MeshGroupTF을 선택했다면, 그 자식인 MeshTF와 MeshGroupTF는 모두 처리를 생략해야한다.
			//따라서 Gizmo용 리스트를 따로 계산해야한다.
			//Bone도 마찬가지. 부모가 선택이 되었다면 자식은 Gizmo에서 편집되지 않는다.

			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				return;
			}

			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			//Undo
			if (isFirstMove)
			{	
				//Undo에 저장하기 위해서 데이터를 미리 모아야 한다. <중요>
				//targetObj = null;
				isRootMeshGroup = true;//하나라도 RootMeshGroup에 속하지 않았으면 Undo에는 재귀적으로 기록해야한다.

				//targetObj = (Editor.Select.MeshTF_Main != null ? (object)Editor.Select.MeshTF_Main : (object)Editor.Select.MeshGroupTF_Main);
				
				//>> [GizmoMain] >> (삭제 21.6.30)
				//targetObj = (Editor.Select.SubObjects.GizmoMeshTF != null ? (object)Editor.Select.SubObjects.GizmoMeshTF : (object)Editor.Select.SubObjects.GizmoMeshGroupTF);

				#region [미사용 코드]
				//if (nGizmoMeshGroupTF > 0)
				//{
				//	//서브 메시 그룹이 선택되었다 > 루트가 아님
				//	isRootMeshGroup = false;
				//}
				//else
				//{
				//	for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
				//	{
				//		curMeshTF = Editor.Select.SubObjects.GizmoMeshTFs[iMeshTF];

				//		if(!Editor.Select.MeshGroup._childMeshTransforms.Contains(curMeshTF))
				//		{
				//			//루트 메시 그룹에 속하지 않은 MeshTF가 있다.
				//			isRootMeshGroup = false;
				//			break;
				//		}
				//	}
				//}

				//if (isRootMeshGroup)
				//{
				//	for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
				//	{
				//		curMeshGroupTF = Editor.Select.SubObjects.GizmoMeshGroupTF[iMeshGroupTF];
				//	}
				//} 
				#endregion

				//위의 코드와 같은 역할을 하는 함수이다.
				if(Editor.Select.SubObjects.IsChildMeshGroupObjectSelectedForGizmo(true, false))
				{
					//자식 메시 그룹이 이 선택에 관여되어 있다.
					isRootMeshGroup = false;
				}

				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, 
													Editor, 
													Editor.Select.MeshGroup, 
													//targetObj,
													false, !isRootMeshGroup,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}
			
			
			//이제 하나씩 이동시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];

				targetMatrix = curMeshTF._matrix;//=ToParent
				worldMatrix = new apMatrix(curMeshTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshTF._matrix_TF_ParentWorld;

				//이동 코드 계산
				worldMatrix._pos += deltaMoveW;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				Vector2 newLocalPos = worldMatrix._pos;

				targetMatrix.SetPos(newLocalPos.x, newLocalPos.y, false);
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];

				targetMatrix = curMeshGroupTF._matrix;//=ToParent
				worldMatrix = new apMatrix(curMeshGroupTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshGroupTF._matrix_TF_ParentWorld;

				//이동 코드 계산
				worldMatrix._pos += deltaMoveW;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				Vector2 newLocalPos = worldMatrix._pos;

				targetMatrix.SetPos(newLocalPos.x, newLocalPos.y, false);
				targetMatrix.MakeMatrix();
			}

			
		}
		public void Rotate__MeshGroup_Setting(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return;
			}

			if (deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			


			#region [미사용 코드] 단일 선택 방식
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	targetMatrix = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//	worldMatrix = new apMatrix(Editor.Select.MeshTF_Main._matrix_TFResult_World);
			//	parentWorldMatrix = Editor.Select.MeshTF_Main._matrix_TF_ParentWorld;

			//	isRootMeshGroup = Editor.Select.MeshGroup._childMeshTransforms.Contains(Editor.Select.MeshTF_Main);
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	targetMatrix = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//	worldMatrix = new apMatrix(Editor.Select.MeshGroupTF_Main._matrix_TFResult_World);
			//	parentWorldMatrix = Editor.Select.MeshGroupTF_Main._matrix_TF_ParentWorld;

			//	isRootMeshGroup = false;
			//}
			//else
			//{
			//	return;
			//}

			//float nextAngle = worldMatrix._angleDeg + deltaAngleW;
			//while (nextAngle < -180.0f)
			//{
			//	nextAngle += 360.0f;
			//}
			//while (nextAngle > 180.0f)
			//{
			//	nextAngle -= 360.0f;
			//}
			//worldMatrix._angleDeg = nextAngle;
			//worldMatrix.MakeMatrix();
			//worldMatrix.RInverse(parentWorldMatrix);//ParentWorld-1 x World = ToParent


			////Undo
			//if (isFirstRotate)
			//{
			//	apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_RotateTransform, Editor, Editor.Select.MeshGroup, targetObj, false, !isRootMeshGroup);
			//}

			////targetMatrix.SetRotate(deltaAngleW + targetMatrix._angleDeg);
			//targetMatrix.SetRotate(worldMatrix._angleDeg);
			//targetMatrix.MakeMatrix(); 
			#endregion

			//변경 20.6.23 : 다중 처리
			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				return;
			}


			//Undo
			if (isFirstRotate)
			{
				//object targetObj = null;//<삭제 21.6.30>
				bool isRootMeshGroup = false;

				//targetObj = (Editor.Select.MeshTF_Main != null ? (object)Editor.Select.MeshTF_Main : (object)Editor.Select.MeshGroupTF_Main);
				//>> [GizmoMain] >> 삭제 21.6.30
				//targetObj = (Editor.Select.SubObjects.GizmoMeshTF != null ? (object)Editor.Select.SubObjects.GizmoMeshTF : (object)Editor.Select.SubObjects.GizmoMeshGroupTF);

				if (Editor.Select.SubObjects.IsChildMeshGroupObjectSelectedForGizmo(true, false))
				{
					//자식 메시 그룹이 이 선택에 관여되어 있다.
					isRootMeshGroup = false;
				}
				else
				{
					//루트 메시 그룹의 객체들만 포함되었다.
					isRootMeshGroup = true;
				}
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_RotateTransform, 
													Editor, 
													Editor.Select.MeshGroup, 
													//targetObj, 
													false, !isRootMeshGroup,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			apMatrix targetMatrix = null;
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			//이제 하나씩 회전시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];

				//float prevAngle = 0.0f;

				////디버그하자
				//if(curMeshTF._nickName.Contains("Debug"))
				//{
				//	Debug.LogWarning("------------------- Gizmo 테스트 -------------------");
				//	Debug.Log("> World Matrix : " + curMeshTF._matrix_TFResult_World.ToString());
				//	Debug.Log("> Default Matrix : " + curMeshTF._matrix.ToString());
				//	Debug.Log("> World Matrix의 초기 스케일 부호 : " + curMeshTF._matrix_TFResult_World._isInitScalePositive_X + ", " + curMeshTF._matrix_TFResult_World._isInitScalePositive_Y);
				//}

				targetMatrix = curMeshTF._matrix;
				worldMatrix = new apMatrix(curMeshTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshTF._matrix_TF_ParentWorld;

				

				//if(curMeshTF._nickName.Contains("Debug"))
				//{
				//	Debug.Log("> Parent World Matrix : " + curMeshTF._matrix_TF_ParentWorld.ToString());
				//	Debug.Log("> Parent World Matrix의 초기 스케일 부호 : " + curMeshTF._matrix_TF_ParentWorld._isInitScalePositive_X + ", " + curMeshTF._matrix_TF_ParentWorld._isInitScalePositive_Y);
				//	Debug.Log("> 복사된 World Matrix의 초기 스케일 부호 : " + worldMatrix._isInitScalePositive_X + ", " + worldMatrix._isInitScalePositive_Y);

				//	Debug.Log("> 이전 각도 : " + worldMatrix._angleDeg);
				//}
				
				float nextAngle = worldMatrix._angleDeg + deltaAngleW;
				while (nextAngle < -180.0f)
				{
					nextAngle += 360.0f;
				}
				while (nextAngle > 180.0f)
				{
					nextAngle -= 360.0f;
				}

				worldMatrix._angleDeg = nextAngle;
				worldMatrix.MakeMatrix();

				//if(curMeshTF._nickName.Contains("Debug"))
				//{	
				//	Debug.Log("> 다음 각도 : " + worldMatrix._angleDeg + " (Delta : " + deltaAngleW + ")");
				//}

				
				worldMatrix.RInverse(parentWorldMatrix, false/*, curMeshTF._nickName.Contains("Debug")*/);//ParentWorld-1 x World = ToParent

				//if(curMeshTF._nickName.Contains("Debug"))
				//{
				//	Debug.Log("> RInverse : " + worldMatrix.ToString());
				//	Debug.Log(">> 각도 변화 : " + targetMatrix._angleDeg + " >>> " + worldMatrix._angleDeg + " (Delta : " + deltaAngleW + ")");
				//}

				targetMatrix.SetRotate(worldMatrix._angleDeg, false);

				targetMatrix.MakeMatrix();

				//if(curMeshTF._nickName.Contains("Debug"))
				//{
				//	Debug.LogWarning("> 완료 : " + worldMatrix.ToString());
				//	Debug.LogWarning("> 결론 : " + prevAngle + " >> " + targetMatrix._angleDeg);
				//	Debug.LogWarning("--------------------------------------------------");

				//	//테스트
				//	//Debug.LogError("< World Matrix 테스트 > ");
				//	//curMeshTF.ReadyToCalculate();
				//	//curMeshTF.MakeTransformMatrix();
				//	//Debug.LogError("-> WorldMatrix 갱신 후 : " + curMeshTF._matrix_TFResult_World.ToString());
				//}
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];

				targetMatrix = curMeshGroupTF._matrix;
				worldMatrix = new apMatrix(curMeshGroupTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshGroupTF._matrix_TF_ParentWorld;

				float nextAngle = worldMatrix._angleDeg + deltaAngleW;
				while (nextAngle < -180.0f)
				{
					nextAngle += 360.0f;
				}
				while (nextAngle > 180.0f)
				{
					nextAngle -= 360.0f;
				}
				worldMatrix._angleDeg = nextAngle;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				targetMatrix.SetRotate(worldMatrix._angleDeg, false);
				targetMatrix.MakeMatrix();
			}
		}


		public void Scale__MeshGroup_Setting(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return;
			}

			if (deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{
				return;
			}

			#region [미사용 코드] 단일 선택 방식
			//apMatrix targetMatrix = null;
			//object targetObj = null;
			//apMatrix worldMatrix = null;
			//apMatrix parentWorldMatrix = null;
			////Modifier가 적용이 안된 상태이므로
			////World Matrix = ParentWorld x ToParent(Default) 가 성립한다.

			//bool isRootMeshGroup = false;

			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	targetMatrix = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//	worldMatrix = new apMatrix(Editor.Select.MeshTF_Main._matrix_TFResult_World);
			//	parentWorldMatrix = Editor.Select.MeshTF_Main._matrix_TF_ParentWorld;

			//	isRootMeshGroup = Editor.Select.MeshGroup._childMeshTransforms.Contains(Editor.Select.MeshTF_Main);
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	targetMatrix = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//	worldMatrix = new apMatrix(Editor.Select.MeshGroupTF_Main._matrix_TFResult_World);
			//	parentWorldMatrix = Editor.Select.MeshGroupTF_Main._matrix_TF_ParentWorld;

			//	isRootMeshGroup = false;
			//}
			//else
			//{
			//	return;
			//}
			//worldMatrix._scale += deltaScaleW;
			//worldMatrix.MakeMatrix();
			//worldMatrix.RInverse(parentWorldMatrix);//ParentWorld-1 x World = ToParent


			////Undo
			//if (isFirstScale)
			//{
			//	apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_ScaleTransform, Editor, Editor.Select.MeshGroup, targetObj, false, !isRootMeshGroup);
			//}

			////Vector2 scale2 = new Vector2(targetMatrix._scale.x, targetMatrix._scale.y);
			////targetMatrix.SetScale(deltaScaleW + scale2);
			//targetMatrix.SetScale(worldMatrix._scale);
			//targetMatrix.MakeMatrix(); 
			#endregion

			//변경 20.6.24 : 다중 처리
			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				return;
			}


			//Undo
			if (isFirstScale)
			{
				//object targetObj = null;//삭제 21.6.30
				bool isRootMeshGroup = false;

				//targetObj = (Editor.Select.MeshTF_Main != null ? (object)Editor.Select.MeshTF_Main : (object)Editor.Select.MeshGroupTF_Main);
				//>> [GizmoMain] >> 삭제 21.6.30
				//targetObj = (Editor.Select.SubObjects.GizmoMeshTF != null ? (object)Editor.Select.SubObjects.GizmoMeshTF : (object)Editor.Select.SubObjects.GizmoMeshGroupTF);

				if (Editor.Select.SubObjects.IsChildMeshGroupObjectSelectedForGizmo(true, false))
				{
					//자식 메시 그룹이 이 선택에 관여되어 있다.
					isRootMeshGroup = false;
				}
				else
				{
					//루트 메시 그룹의 객체들만 포함되었다.
					isRootMeshGroup = true;
				}

				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_ScaleTransform, 
													Editor, 
													Editor.Select.MeshGroup, 
													//targetObj, 
													false, !isRootMeshGroup,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			apMatrix targetMatrix = null;
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			//Modifier가 적용이 안된 상태이므로
			//World Matrix = ParentWorld x ToParent(Default) 가 성립한다.

			//이제 하나씩 크기를 변경시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];

				targetMatrix = curMeshTF._matrix;
				worldMatrix = new apMatrix(curMeshTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshTF._matrix_TF_ParentWorld;

				worldMatrix._scale += deltaScaleW;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				targetMatrix.SetScale(worldMatrix._scale, false);
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];

				targetMatrix = curMeshGroupTF._matrix;
				worldMatrix = new apMatrix(curMeshGroupTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshGroupTF._matrix_TF_ParentWorld;

				worldMatrix._scale += deltaScaleW;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				targetMatrix.SetScale(worldMatrix._scale, false);
				targetMatrix.MakeMatrix();
			}

			
		}



		public void TransformChanged_Position__MeshGroup_Setting(Vector2 pos)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null) { return; }

			//>> [GizmoMain] >>
			if(Editor.Select.SubObjects.GizmoMeshTF == null && Editor.Select.SubObjects.GizmoMeshGroupTF == null) { return; }

			#region [미사용 코드] 단일 선택 방식
			//apRenderUnit curRenderUnit = null;
			//apMatrix curMatrixParam = null;

			//object targetObj = null;
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshTF_Main);
			//	curMatrixParam = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshGroupTF_Main);
			//	curMatrixParam = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//}

			//if (curRenderUnit == null)
			//{ return; }

			////Undo
			//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			//curMatrixParam.SetPos(pos);
			//curMatrixParam.MakeMatrix();
			//Editor.SetRepaint(); 
			#endregion


			//변경 20.6.24 : 다중 처리
			//값을 입력하여 변경하는 경우
			//일단 > 메인을 찾아서 deltaPos를 계산한다.
			//메인에는 값을 넣고, 나머지는 deltaPos으로 계산한다.

			apMatrix mainMatrixParam = null;
			Vector2 deltaPos = Vector2.zero;

			object targetObj = null;
			//기존 방식
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	mainMatrixParam = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	mainMatrixParam = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//}

			//>> [GizmoMain] >>
			if (Editor.Select.SubObjects.GizmoMeshTF != null)
			{
				mainMatrixParam = Editor.Select.SubObjects.GizmoMeshTF._matrix;
				targetObj = Editor.Select.SubObjects.GizmoMeshTF;
			}
			else if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			{
				mainMatrixParam = Editor.Select.SubObjects.GizmoMeshGroupTF._matrix;
				targetObj = Editor.Select.SubObjects.GizmoMeshGroupTF;
			}




			if (targetObj == null) { return; }

			deltaPos = pos - mainMatrixParam._pos;

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, 
												Editor, 
												Editor.Select.MeshGroup, 
												//targetObj, 
												false, true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);


			//이제 선택된 객체들을 하나씩 확인하면서 수정한다.
			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			apMatrix targetMatrix = null;

			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				//기즈모용 객체에 없다면,
				//메인 행렬만 수정하고 리턴 (오류 상황이긴 하다.)
				mainMatrixParam.SetPos(pos, false);
				mainMatrixParam.MakeMatrix();
				Editor.SetRepaint();
				return;
			}

			//이제 하나씩 적용시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];
				targetMatrix = curMeshTF._matrix;

				//if(curMeshTF == Editor.Select.MeshTF_Main)
				//>> [GizmoMain] >>
				if(curMeshTF == Editor.Select.SubObjects.GizmoMeshTF)
				{	
					targetMatrix.SetPos(pos, false);//메인 MeshTF라면 직접 대입
				}
				else
				{
					targetMatrix._pos += deltaPos;//서브라면 deltaPos를 가감
				}
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];
				targetMatrix = curMeshGroupTF._matrix;

				//if(curMeshGroupTF == Editor.Select.MeshGroupTF_Main)
				//>> [GizmoMain] >>
				if(curMeshGroupTF == Editor.Select.SubObjects.GizmoMeshGroupTF)
				{	
					targetMatrix.SetPos(pos, false);//메인 MeshGroupTF라면 직접 대입
				}
				else
				{
					targetMatrix._pos += deltaPos;//서브라면 deltaPos를 가감
				}
				targetMatrix.MakeMatrix();
			}

			Editor.SetRepaint();
		}



		public void TransformChanged_Rotate__MeshGroup_Setting(float angle)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null)
			{
				return;
			}

			//>> [GizmoMain] >>
			if(Editor.Select.SubObjects.GizmoMeshTF == null && Editor.Select.SubObjects.GizmoMeshGroupTF == null) { return; }

			#region [미사용 코드] 단일 선택 방식
			//apRenderUnit curRenderUnit = null;
			//apMatrix curMatrixParam = null;

			//object targetObj = null;
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshTF_Main);
			//	curMatrixParam = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshGroupTF_Main);
			//	curMatrixParam = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//}

			//if (curRenderUnit == null)
			//{ return; }

			////Undo
			//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_RotateTransform, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			//curMatrixParam.SetRotate(angle);
			//curMatrixParam.MakeMatrix();
			//Editor.SetRepaint(); 
			#endregion

			//변경 20.6.24 : 다중 처리
			//값을 입력하여 변경하는 경우
			//일단 > 메인을 찾아서 deltaAngle를 계산한다.
			//메인에는 값을 넣고, 나머지는 deltaAngle으로 계산한다.

			apMatrix mainMatrixParam = null;
			float deltaAngle = 0.0f;

			object targetObj = null;
			
			//기존 방식
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	mainMatrixParam = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	mainMatrixParam = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//}

			//>> [GizmoMain] >>
			if (Editor.Select.SubObjects.GizmoMeshTF != null)
			{
				mainMatrixParam = Editor.Select.SubObjects.GizmoMeshTF._matrix;
				targetObj = Editor.Select.SubObjects.GizmoMeshTF;
			}
			else if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			{
				mainMatrixParam = Editor.Select.SubObjects.GizmoMeshGroupTF._matrix;
				targetObj = Editor.Select.SubObjects.GizmoMeshGroupTF;
			}



			if (targetObj == null) { return; }

			deltaAngle = angle - mainMatrixParam._angleDeg;

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_RotateTransform, 
												Editor, 
												Editor.Select.MeshGroup, 
												//targetObj, 
												false, true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			//이제 선택된 객체들을 하나씩 확인하면서 수정한다.
			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			apMatrix targetMatrix = null;

			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				//기즈모용 객체에 없다면,
				//메인 행렬만 수정하고 리턴 (오류 상황이긴 하다.)
				mainMatrixParam.SetRotate(angle, false);
				mainMatrixParam.MakeMatrix();
				Editor.SetRepaint();
				return;
			}

			//이제 하나씩 적용시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];
				targetMatrix = curMeshTF._matrix;

				//if(curMeshTF == Editor.Select.MeshTF_Main)
				//>> [GizmoMain] >>
				if(curMeshTF == Editor.Select.SubObjects.GizmoMeshTF)
				{	
					targetMatrix.SetRotate(angle, false);//메인이라면 직접 대입
				}
				else
				{
					targetMatrix.SetRotate(apUtil.AngleTo180(targetMatrix._angleDeg + deltaAngle), false);//서브라면 deltaAngle를 가감
				}
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];
				targetMatrix = curMeshGroupTF._matrix;

				//if(curMeshGroupTF == Editor.Select.MeshGroupTF_Main)
				//>> [GizmoMain] >>
				if(curMeshGroupTF == Editor.Select.SubObjects.GizmoMeshGroupTF)
				{	
					targetMatrix.SetRotate(angle, false);//메인이라면 직접 대입
				}
				else
				{
					targetMatrix.SetRotate(apUtil.AngleTo180(targetMatrix._angleDeg + deltaAngle), false);//서브라면 deltaAngle를 가감
				}
				targetMatrix.MakeMatrix();
			}
			
			Editor.SetRepaint();
		}



		public void TransformChanged_Scale__MeshGroup_Setting(Vector2 scale)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return;
			}
			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null) { return; }

			//>> [GizmoMain] >>
			if(Editor.Select.SubObjects.GizmoMeshTF == null && Editor.Select.SubObjects.GizmoMeshGroupTF == null) { return; }


			#region [미사용 코드] 단일 선택 방식
			//apRenderUnit curRenderUnit = null;
			//apMatrix curMatrixParam = null;
			//object targetObj = null;
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshTF_Main);
			//	curMatrixParam = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshGroupTF_Main);
			//	curMatrixParam = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//}

			//if (curRenderUnit == null)
			//{ return; }

			////Undo
			//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_ScaleTransform, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			//curMatrixParam.SetScale(scale);
			//curMatrixParam.MakeMatrix();
			//Editor.SetRepaint(); 
			#endregion

			//변경 20.6.24 : 다중 처리
			//값을 입력하여 변경하는 경우
			//일단 > 메인을 찾아서 deltaScale를 계산한다.
			//메인에는 값을 넣고, 나머지는 deltaScale으로 계산한다. (비율로)

			apMatrix mainMatrixParam = null;
			object targetObj = null;
			Vector2 deltaScale = Vector2.one;

			//기존 방식
			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	mainMatrixParam = Editor.Select.MeshTF_Main._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	mainMatrixParam = Editor.Select.MeshGroupTF_Main._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//}

			//>> [GizmoMain] >>
			if (Editor.Select.SubObjects.GizmoMeshTF != null)
			{
				mainMatrixParam = Editor.Select.SubObjects.GizmoMeshTF._matrix;
				targetObj = Editor.Select.SubObjects.GizmoMeshTF;
			}
			else if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			{
				mainMatrixParam = Editor.Select.SubObjects.GizmoMeshGroupTF._matrix;
				targetObj = Editor.Select.SubObjects.GizmoMeshGroupTF;
			}



			if (targetObj == null) { return; }

			//DeltaScale은 비율로 계산한다.
			deltaScale.x = (mainMatrixParam._scale.x != 0.0f) ? (scale.x / mainMatrixParam._scale.x) : (1.0f);
			deltaScale.y = (mainMatrixParam._scale.y != 0.0f) ? (scale.y / mainMatrixParam._scale.y) : (1.0f);

			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_ScaleTransform, 
												Editor, 
												Editor.Select.MeshGroup, 
												//targetObj, 
												false, true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			//이제 선택된 객체들을 하나씩 확인하면서 수정한다.
			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			apMatrix targetMatrix = null;

			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				//기즈모용 객체에 없다면,
				//메인 행렬만 수정하고 리턴 (오류 상황이긴 하다.)
				mainMatrixParam.SetScale(scale, false);
				mainMatrixParam.MakeMatrix();
				Editor.SetRepaint();
				return;
			}

			//이제 하나씩 적용시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];
				targetMatrix = curMeshTF._matrix;

				//if(curMeshTF == Editor.Select.MeshTF_Main)
				//>> [GizmoMain] >>
				if(curMeshTF == Editor.Select.SubObjects.GizmoMeshTF)
				{	
					targetMatrix.SetScale(scale, false);//메인 MeshTF라면 직접 대입
				}
				else
				{
					//서브라면 deltaScale를 곱함
					targetMatrix._scale.x *= deltaScale.x;
					targetMatrix._scale.y *= deltaScale.y;
				}
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];
				targetMatrix = curMeshGroupTF._matrix;

				//if(curMeshGroupTF == Editor.Select.MeshGroupTF_Main)
				//>> [GizmoMain] >>
				if(curMeshGroupTF == Editor.Select.SubObjects.GizmoMeshGroupTF)
				{	
					targetMatrix.SetScale(scale, false);//메인 MeshGroupTF라면 직접 대입
				}
				else
				{
					//서브라면 deltaScale를 곱함
					targetMatrix._scale.x *= deltaScale.x;
					targetMatrix._scale.y *= deltaScale.y;
				}
				targetMatrix.MakeMatrix();
			}

			Editor.SetRepaint();


			mainMatrixParam.SetScale(scale, false);
			mainMatrixParam.MakeMatrix();
			Editor.SetRepaint();
		}


		public void TransformChanged_Depth__MeshGroup_Setting(int depth)
		{
			apMeshGroup targetMeshGroup = Editor.Select.MeshGroup;
			if (targetMeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null)
			{
				return;
			}

			//>> [GizmoMain] >>
			if(Editor.Select.SubObjects.GizmoMeshTF == null && Editor.Select.SubObjects.GizmoMeshGroupTF == null)
			{
				return;
			}

			apRenderUnit curRenderUnit = null;
			
			//apMatrix curMatrixParam = null;

			//코멘트 20.6.25 : Depth는 다중 선택을 지원하지 않는다.

			//object targetObj = null;//삭제 21.6.30

			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshTF_Main);
			//	//curMatrixParam = Editor.Select.SubMeshInGroup._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshGroupTF_Main);
			//	//curMatrixParam = Editor.Select.SubMeshGroupInGroup._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//}

			//>> [GizmoMain] >>
			if (Editor.Select.SubObjects.GizmoMeshTF != null)
			{
				curRenderUnit = Editor.Select.SubObjects.GizmoMeshTF._linkedRenderUnit;
				//targetObj = Editor.Select.SubObjects.GizmoMeshTF;
			}
			else if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			{
				curRenderUnit = Editor.Select.SubObjects.GizmoMeshGroupTF._linkedRenderUnit;
				//targetObj = Editor.Select.SubObjects.GizmoMeshGroupTF;
			}




			if (curRenderUnit == null) { return; }

			//Undo
			//이전
			//apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, 
			//									Editor, 
			//									Editor.Select.MeshGroup, 
			//									//targetObj, 
			//									false, true,
			//									//apEditorUtil.UNDO_STRUCT.ValueOnly
			//									apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
			//									);

			//변경 22.8.19 : 관련된 모든 메시 그룹들이 영향을 받는다. [v1.4.2]
			apEditorUtil.SetRecord_MeshGroup_AllParentAndChildren(
												apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform,
												Editor, 
												Editor.Select.MeshGroup, 
												apEditorUtil.UNDO_STRUCT.StructChanged//변경 22.8.2 [v1.4.1] 실행 취소 버그 막기 위해
												);

			//bool bSort = false;
			if (curRenderUnit.GetDepth() != depth)
			{
				//curRenderUnit.SetDepth(depth);
				Editor.Select.MeshGroup.ChangeRenderUnitDepth(curRenderUnit, depth);//여기에 이미 Sorting이 들어간다.

				Editor.OnAnyObjectAddedOrRemoved(true);

				//bSort = true;
				apEditorUtil.ReleaseGUIFocus();
			}

			//삭제. ChangeRenderUnitDetph 코드에 이미 Sort가 포함되어있다.
			//if (bSort)
			//{
			//	Editor.Select.MeshGroup.SortRenderUnits(true);
			//}
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}



		public void TransformChanged_Color__MeshGroup_Setting(Color color, bool isVisible)
		{
			if (Editor.Select.MeshGroup == null
				//|| !Editor.Select.IsMeshGroupSettingChangePivot//수정 : Pivot 변경 상태가 아니어도 변경 가능
				)
			{
				return;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null) { return; }

			//>> [GizmoMain] >>
			if(Editor.Select.SubObjects.GizmoMeshTF == null && Editor.Select.SubObjects.GizmoMeshGroupTF == null) { return; }

			#region [미사용 코드] 단일 선택 방식
			//apRenderUnit curRenderUnit = null;
			////apMatrix curMatrixParam = null;
			//object targetObj = null;

			////Undo
			//apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_Gizmo_Color, Editor, Editor.Select.MeshGroup, targetObj, false, true);

			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshTF_Main);
			//	//curMatrixParam = Editor.Select.SubMeshInGroup._matrix;
			//	targetObj = Editor.Select.MeshTF_Main;
			//	Editor.Select.MeshTF_Main._meshColor2X_Default = color;
			//	Editor.Select.MeshTF_Main._isVisible_Default = isVisible;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshGroupTF_Main);
			//	//curMatrixParam = Editor.Select.SubMeshGroupInGroup._matrix;
			//	targetObj = Editor.Select.MeshGroupTF_Main;
			//	Editor.Select.MeshGroupTF_Main._meshColor2X_Default = color;
			//	Editor.Select.MeshGroupTF_Main._isVisible_Default = isVisible;
			//}

			//if (curRenderUnit == null)
			//{ return; }



			////curRenderUnit.SetColor(color);
			//Editor.RefreshControllerAndHierarchy(false);//Show/Hide 아이콘 갱신 땜시
			//Editor.SetRepaint(); 
			#endregion

			//변경 20.6.24 : 다중 처리
			//object targetObj = null;//삭제 21.6.310

			//>> [GizmoMain] >> 삭제 21.6.30
			//if (Editor.Select.SubObjects.GizmoMeshTF != null)
			//{
			//	targetObj = Editor.Select.SubObjects.GizmoMeshTF;
			//}
			//else if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			//{
			//	targetObj = Editor.Select.SubObjects.GizmoMeshGroupTF;
			//}



			//Undo
			apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_Color, 
												Editor, 
												Editor.Select.MeshGroup, 
												//targetObj, 
												false, true,
												apEditorUtil.UNDO_STRUCT.ValueOnly);


			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if (nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				//기즈모용 객체에 없다면,
				//메인 TF만 수정하고 리턴 (오류 상황이긴 하다.)
				//if (Editor.Select.MeshTF_Main != null)
				//{
				//	Editor.Select.MeshTF_Main._meshColor2X_Default = color;
				//	Editor.Select.MeshTF_Main._isVisible_Default = isVisible;
				//}
				//else if (Editor.Select.MeshGroupTF_Main != null)
				//{
				//	Editor.Select.MeshGroupTF_Main._meshColor2X_Default = color;
				//	Editor.Select.MeshGroupTF_Main._isVisible_Default = isVisible;
				//}


				if (Editor.Select.SubObjects.GizmoMeshTF != null)
				{
					Editor.Select.SubObjects.GizmoMeshTF._meshColor2X_Default = color;
					Editor.Select.SubObjects.GizmoMeshTF._isVisible_Default = isVisible;
				}
				else if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
				{
					Editor.Select.SubObjects.GizmoMeshGroupTF._meshColor2X_Default = color;
					Editor.Select.SubObjects.GizmoMeshGroupTF._isVisible_Default = isVisible;
				}

				Editor.RefreshControllerAndHierarchy(false);//Show/Hide 아이콘 갱신 땜시
				Editor.SetRepaint();
				return;
			}


			//이제 하나씩 적용시키자
			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];

				curMeshTF._meshColor2X_Default = color;
				curMeshTF._isVisible_Default = isVisible;
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];

				curMeshGroupTF._meshColor2X_Default = color;
				curMeshGroupTF._isVisible_Default = isVisible;
			}

			//curRenderUnit.SetColor(color);
			Editor.RefreshControllerAndHierarchy(false);//Show/Hide 아이콘 갱신 땜시
			Editor.SetRepaint();
		}

		public apGizmos.TransformParam PivotReturn__MeshGroup_Setting()
		{

			if (Editor.Select.MeshGroup == null)
			{
				return null;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null)
			{
				return null;
			}

			#region [미사용 코드] Gizmo Main에 상관없이 Main 객체에서 피벗이 형성되는 코드
			//apRenderUnit curRenderUnit = null;
			//apMatrix curMatrixParam = null;
			//apMatrix resultMatrix = null;
			//Color meshColor2X = Color.gray;
			//bool isVisible = true;

			//if (Editor.Select.MeshTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshTF_Main);
			//	curMatrixParam = Editor.Select.MeshTF_Main._matrix;
			//	meshColor2X = Editor.Select.MeshTF_Main._meshColor2X_Default;
			//	isVisible = Editor.Select.MeshTF_Main._isVisible_Default;
			//}
			//else if (Editor.Select.MeshGroupTF_Main != null)
			//{
			//	curRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.MeshGroupTF_Main);
			//	curMatrixParam = Editor.Select.MeshGroupTF_Main._matrix;
			//	meshColor2X = Editor.Select.MeshGroupTF_Main._meshColor2X_Default;
			//	isVisible = Editor.Select.MeshGroupTF_Main._isVisible_Default;
			//}

			//if (curRenderUnit == null)
			//{
			//	return null;
			//}

			//if (curRenderUnit._meshTransform != null)
			//{
			//	resultMatrix = curRenderUnit._meshTransform._matrix_TFResult_World;
			//}
			//else if (curRenderUnit._meshGroupTransform != null)
			//{
			//	resultMatrix = curRenderUnit._meshGroupTransform._matrix_TFResult_World;
			//}
			//else
			//{
			//	return null;
			//}

			////Root의 MeshGroupTransform을 추가

			//apMatrix curMatrixParam_Result = new apMatrix(curMatrixParam);
			//curMatrixParam_Result.RMultiply(Editor.Select.MeshGroup._rootMeshGroupTransform._matrix);

			////TODO : Pivot 수정중엔 Calculated 데이터가 제외되어야 한다.
			////Vector3 posW3 = curRenderUnit.WorldMatrixOfNode.GetPosition();
			//Vector2 posW2 = resultMatrix._pos;

			//if (!Editor.Select.IsMeshGroupSettingChangePivot)
			//{
			//	return apGizmos.TransformParam.Make(
			//									posW2,//<<Calculate를 포함한다.
			//										  //curMatrixParam._pos, 
			//										  //curMatrixParam_Result._angleDeg,
			//										  //curMatrixParam_Result._scale,
			//									resultMatrix._angleDeg,
			//									resultMatrix._scale,
			//									curRenderUnit.GetDepth(),
			//									//curRenderUnit.GetColor(),
			//									meshColor2X,
			//									isVisible,
			//									curRenderUnit.WorldMatrix,
			//									false,
			//									apGizmos.TRANSFORM_UI.Color,//색상만 설정 가능
			//									curMatrixParam._pos,
			//									curMatrixParam._angleDeg,
			//									curMatrixParam._scale);
			//}
			//else
			//{
			//	return apGizmos.TransformParam.Make(
			//									//curMatrixParam_Result._pos,//<<Calculate를 포함한다.
			//									posW2,//<<Calculate를 포함한다.
			//										  //curMatrixParam._pos, 
			//										  //curMatrixParam_Result._angleDeg,
			//										  //curMatrixParam_Result._scale,
			//									resultMatrix._angleDeg,
			//									resultMatrix._scale,

			//									curRenderUnit.GetDepth(),
			//									//curRenderUnit.GetColor(),
			//									meshColor2X,
			//									isVisible,
			//									//curMatrixParam_Result.MtrxToSpace,
			//									curRenderUnit.WorldMatrix,
			//									false,
			//									//apGizmos.TRANSFORM_UI.TRS,
			//									apGizmos.TRANSFORM_UI.TRS_WithDepth//Depth 포함한 TRS 
			//									| apGizmos.TRANSFORM_UI.Color,//색상도 포함시킨다.
			//									curMatrixParam._pos,
			//									curMatrixParam._angleDeg,
			//									curMatrixParam._scale
			//									);
			//} 
			#endregion


			//>> [GizmoMain]
			//변경 20.7.18 : Gizmo Main으로 변경 
			if(Editor.Select.SubObjects.GizmoMeshTF == null && Editor.Select.SubObjects.GizmoMeshGroupTF == null)
			{
				return null;
			}

			
			apRenderUnit curRenderUnit = null;
			apMatrix curMatrixParam = null;
			apMatrix resultMatrix = null;
			Color meshColor2X = Color.gray;
			bool isVisible = true;

			if (Editor.Select.SubObjects.GizmoMeshTF != null)
			{
				apTransform_Mesh gizmoMainMeshTF = Editor.Select.SubObjects.GizmoMeshTF;
				curRenderUnit =		gizmoMainMeshTF._linkedRenderUnit;
				curMatrixParam =	gizmoMainMeshTF._matrix;
				meshColor2X =		gizmoMainMeshTF._meshColor2X_Default;
				isVisible =			gizmoMainMeshTF._isVisible_Default;
			}
			else if (Editor.Select.SubObjects.GizmoMeshGroupTF != null)
			{
				apTransform_MeshGroup gizmoMainMeshGroupTF = Editor.Select.SubObjects.GizmoMeshGroupTF;
				curRenderUnit =		gizmoMainMeshGroupTF._linkedRenderUnit;
				curMatrixParam =	gizmoMainMeshGroupTF._matrix;
				meshColor2X =		gizmoMainMeshGroupTF._meshColor2X_Default;
				isVisible =			gizmoMainMeshGroupTF._isVisible_Default;
			}

			if (curRenderUnit == null)
			{
				Debug.LogError("Pivot Error > No RenderUnit");
				return null;
			}

			if (curRenderUnit._meshTransform != null)
			{
				resultMatrix = curRenderUnit._meshTransform._matrix_TFResult_World;
			}
			else if (curRenderUnit._meshGroupTransform != null)
			{
				resultMatrix = curRenderUnit._meshGroupTransform._matrix_TFResult_World;
			}
			else
			{
				return null;
			}

			//Root의 MeshGroupTransform을 추가

			apMatrix curMatrixParam_Result = new apMatrix(curMatrixParam);
			
			//추가 20.8.6. [RMultiply Scale 이슈]
			curMatrixParam_Result.OnBeforeRMultiply();

			curMatrixParam_Result.RMultiply(Editor.Select.MeshGroup._rootMeshGroupTransform._matrix, true);

			Vector2 posW2 = resultMatrix._pos;

			if (!Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return apGizmos.TransformParam.Make(
												posW2,
												resultMatrix._angleDeg,
												resultMatrix._scale,
												curRenderUnit.GetDepth(),
												meshColor2X,
												isVisible,
												curRenderUnit.WorldMatrix,
												false,
												apGizmos.TRANSFORM_UI.Color,//색상만 설정 가능
												curMatrixParam._pos,
												curMatrixParam._angleDeg,
												curMatrixParam._scale);
			}
			else
			{
				return apGizmos.TransformParam.Make(
												posW2,
												resultMatrix._angleDeg,
												resultMatrix._scale,
												curRenderUnit.GetDepth(),
												//curRenderUnit.GetColor(),
												meshColor2X,
												isVisible,
												//curMatrixParam_Result.MtrxToSpace,
												curRenderUnit.WorldMatrix,
												false,
												//apGizmos.TRANSFORM_UI.TRS,
												apGizmos.TRANSFORM_UI.TRS_WithDepth//Depth 포함한 TRS 
												| apGizmos.TRANSFORM_UI.Color,//색상도 포함시킨다.
												curMatrixParam._pos,
												curMatrixParam._angleDeg,
												curMatrixParam._scale
												);
			}

		}


		// 키보드 입력
		//-------------------------------------------------------------------------------------------------------
		private bool OnHotKeyEvent__MeshGroup_Setting__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return false;
			}

			apMatrix targetMatrix = null;
			//object targetObj = null;//삭제 21.6.30
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			bool isRootMeshGroup = false;

			//Modifier가 적용이 안된 상태이므로
			//World Matrix = ParentWorld x ToParent(Default) 가 성립한다.


			//변경 20.6.20 : 다중 처리
			//단순히 전체 처리를 하면 안된다.
			//만약 MeshGroupTF을 선택했다면, 그 자식인 MeshTF와 MeshGroupTF는 모두 처리를 생략해야한다.
			//따라서 Gizmo용 리스트를 따로 계산해야한다.
			//Bone도 마찬가지. 부모가 선택이 되었다면 자식은 Gizmo에서 편집되지 않는다.

			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				return false;
			}

			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			//Undo
			if (isFirstMove)
			{	
				//Undo에 저장하기 위해서 데이터를 미리 모아야 한다. <중요>
				//targetObj = null;
				isRootMeshGroup = true;//하나라도 RootMeshGroup에 속하지 않았으면 Undo에는 재귀적으로 기록해야한다.

				//targetObj = (Editor.Select.MeshTF_Main != null ? (object)Editor.Select.MeshTF_Main : (object)Editor.Select.MeshGroupTF_Main);

				//>> [GizmoMain] >> 삭제 21.6.30
				//targetObj = (Editor.Select.SubObjects.GizmoMeshTF != null ? (object)Editor.Select.SubObjects.GizmoMeshTF : (object)Editor.Select.SubObjects.GizmoMeshGroupTF);

				
				//위의 코드와 같은 역할을 하는 함수이다.
				if(Editor.Select.SubObjects.IsChildMeshGroupObjectSelectedForGizmo(true, false))
				{
					//자식 메시 그룹이 이 선택에 관여되어 있다.
					isRootMeshGroup = false;
				}

				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_MoveTransform, 
													Editor, 
													Editor.Select.MeshGroup, 
													//targetObj,
													false, !isRootMeshGroup,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}
			
			
			//이제 하나씩 이동시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];

				targetMatrix = curMeshTF._matrix;//=ToParent
				worldMatrix = new apMatrix(curMeshTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshTF._matrix_TF_ParentWorld;

				//이동 코드 계산
				worldMatrix._pos += deltaMoveW;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				Vector2 newLocalPos = worldMatrix._pos;

				targetMatrix.SetPos(newLocalPos.x, newLocalPos.y, false);
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];

				targetMatrix = curMeshGroupTF._matrix;//=ToParent
				worldMatrix = new apMatrix(curMeshGroupTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshGroupTF._matrix_TF_ParentWorld;

				//이동 코드 계산
				worldMatrix._pos += deltaMoveW;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				Vector2 newLocalPos = worldMatrix._pos;

				targetMatrix.SetPos(newLocalPos.x, newLocalPos.y, false);
				targetMatrix.MakeMatrix();
			}

			return true;
		}

		private bool OnHotKeyEvent__MeshGroup_Setting__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return false;
			}

			//변경 20.6.23 : 다중 처리
			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				return false;
			}


			//Undo
			if (isFirstRotate)
			{
				//object targetObj = null;//삭제 21.6.30
				bool isRootMeshGroup = false;
				
				//targetObj = (Editor.Select.MeshTF_Main != null ? (object)Editor.Select.MeshTF_Main : (object)Editor.Select.MeshGroupTF_Main);
				
				//>> [GizmoMain] >> 삭제 21.6.30
				//targetObj = (Editor.Select.SubObjects.GizmoMeshTF != null ? (object)Editor.Select.SubObjects.GizmoMeshTF : (object)Editor.Select.SubObjects.GizmoMeshGroupTF);



				if (Editor.Select.SubObjects.IsChildMeshGroupObjectSelectedForGizmo(true, false))
				{
					//자식 메시 그룹이 이 선택에 관여되어 있다.
					isRootMeshGroup = false;
				}
				else
				{
					//루트 메시 그룹의 객체들만 포함되었다.
					isRootMeshGroup = true;
				}
				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_RotateTransform, 
													Editor, 
													Editor.Select.MeshGroup, 
													//targetObj, 
													false, !isRootMeshGroup,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			apMatrix targetMatrix = null;
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			//이제 하나씩 회전시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];

				targetMatrix = curMeshTF._matrix;
				worldMatrix = new apMatrix(curMeshTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshTF._matrix_TF_ParentWorld;

				float nextAngle = worldMatrix._angleDeg + deltaAngleW;
				while (nextAngle < -180.0f)
				{
					nextAngle += 360.0f;
				}
				while (nextAngle > 180.0f)
				{
					nextAngle -= 360.0f;
				}
				worldMatrix._angleDeg = nextAngle;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				targetMatrix.SetRotate(worldMatrix._angleDeg, false);
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];

				targetMatrix = curMeshGroupTF._matrix;
				worldMatrix = new apMatrix(curMeshGroupTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshGroupTF._matrix_TF_ParentWorld;

				float nextAngle = worldMatrix._angleDeg + deltaAngleW;
				while (nextAngle < -180.0f)
				{
					nextAngle += 360.0f;
				}
				while (nextAngle > 180.0f)
				{
					nextAngle -= 360.0f;
				}
				worldMatrix._angleDeg = nextAngle;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				targetMatrix.SetRotate(worldMatrix._angleDeg, false);
				targetMatrix.MakeMatrix();
			}

			return true;
		}

		private bool OnHotKeyEvent__MeshGroup_Setting__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null || !Editor.Select.IsMeshGroupSettingEditDefaultTransform)
			{
				return false;
			}

			//변경 20.6.24 : 다중 처리
			int nGizmoMeshTF = Editor.Select.SubObjects.NumGizmoMeshTF;
			int nGizmoMeshGroupTF = Editor.Select.SubObjects.NumGizmoMeshGroupTF;

			if(nGizmoMeshTF == 0 && nGizmoMeshGroupTF == 0)
			{
				return false;
			}


			//Undo
			if (isFirstScale)
			{
				//object targetObj = null;//삭제 21.6.30
				bool isRootMeshGroup = false;

				//targetObj = (Editor.Select.MeshTF_Main != null ? (object)Editor.Select.MeshTF_Main : (object)Editor.Select.MeshGroupTF_Main);

				//>> [GizmoMain] >> 삭제 21.6.30
				//targetObj = (Editor.Select.SubObjects.GizmoMeshTF != null ? (object)Editor.Select.SubObjects.GizmoMeshTF : (object)Editor.Select.SubObjects.GizmoMeshGroupTF);


				if (Editor.Select.SubObjects.IsChildMeshGroupObjectSelectedForGizmo(true, false))
				{
					//자식 메시 그룹이 이 선택에 관여되어 있다.
					isRootMeshGroup = false;
				}
				else
				{
					//루트 메시 그룹의 객체들만 포함되었다.
					isRootMeshGroup = true;
				}

				apEditorUtil.SetRecord_MeshGroup(	apUndoGroupData.ACTION.MeshGroup_Gizmo_ScaleTransform, 
													Editor, Editor.Select.MeshGroup, 
													//targetObj, 
													false, !isRootMeshGroup,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			apTransform_Mesh curMeshTF = null;
			apTransform_MeshGroup curMeshGroupTF = null;

			apMatrix targetMatrix = null;
			apMatrix worldMatrix = null;
			apMatrix parentWorldMatrix = null;

			//Modifier가 적용이 안된 상태이므로
			//World Matrix = ParentWorld x ToParent(Default) 가 성립한다.

			//이제 하나씩 크기를 변경시키자
			for (int iMeshTF = 0; iMeshTF < nGizmoMeshTF; iMeshTF++)
			{
				curMeshTF = Editor.Select.SubObjects.AllGizmoMeshTFs[iMeshTF];

				targetMatrix = curMeshTF._matrix;
				worldMatrix = new apMatrix(curMeshTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshTF._matrix_TF_ParentWorld;

				worldMatrix._scale += deltaScaleL;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				targetMatrix.SetScale(worldMatrix._scale, false);
				targetMatrix.MakeMatrix();
			}

			for (int iMeshGroupTF = 0; iMeshGroupTF < nGizmoMeshGroupTF; iMeshGroupTF++)
			{
				curMeshGroupTF = Editor.Select.SubObjects.AllGizmoMeshGroupTFs[iMeshGroupTF];

				targetMatrix = curMeshGroupTF._matrix;
				worldMatrix = new apMatrix(curMeshGroupTF._matrix_TFResult_World);
				parentWorldMatrix = curMeshGroupTF._matrix_TF_ParentWorld;

				worldMatrix._scale += deltaScaleL;
				worldMatrix.MakeMatrix();
				worldMatrix.RInverse(parentWorldMatrix, false);//ParentWorld-1 x World = ToParent

				targetMatrix.SetScale(worldMatrix._scale, false);
				targetMatrix.MakeMatrix();
			}

			return true;
		}

	}

}