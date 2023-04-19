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
		// 작성해야하는 함수
		// Select : int - (Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		// Move : void - (Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex)
		// Rotate : void - (float deltaAngleW)
		// Scale : void - (Vector2 deltaScaleW)

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
		// Gizmo - Physics Modifier에서 Vertex 선택한다. 제어는 없음
		// Area 선택이 가능하다
		// < ModRenderVert 선택시 ModVertWeight를 선택하도록 주의 >
		//----------------------------------------------------------------
		/// <summary>
		/// Modifier [Physics]에 대한 Gizmo Event의 Set이다.
		/// </summary>
		/// <returns></returns>
		public apGizmos.GizmoEventSet GetEventSet_Modifier_Physics()
		{
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Modifier_Physics,
														Unselect__Modifier_Physics, 
														null, 
														null, 
														null, 
														PivotReturn__Modifier_Physics);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	null,
																null,
																null,
																null,
																null,
																null,
																apGizmos.TRANSFORM_UI.None
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__Modifier_Physics, 
														null, 
														null, 
														null, 
														null, 
														null);
			
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Modifier_Physic, 
																AddHotKeys__Modifier_Physics, 
																null, 
																null, 
																null);

			return apGizmos.GizmoEventSet.I;

			//이전
			//return new apGizmos.GizmoEventSet(
			//	Select__Modifier_Physics,
			//	Unselect__Modifier_Physics,
			//	null, null, null, null,
			//	null, null, null, null, null,
			//	PivotReturn__Modifier_Physics,
			//	MultipleSelect__Modifier_Physics,
			//	null,
			//	null,
			//	null,
			//	null,
			//	null,
			//	apGizmos.TRANSFORM_UI.None,
			//	FirstLink__Modifier_Physic,
			//	AddHotKeys__Modifier_Physics);
		}




		public apGizmos.SelectResult FirstLink__Modifier_Physic()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.ModRenderVerts_All != null)
			{
				//return Editor.Select.ModRenderVertListOfMod.Count;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}

			return null;
		}


		/// <summary>
		/// Physic Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Physics(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			//(Editing 상태일 때)
			//1. Vertex 선택
			//2. (Lock 걸리지 않았다면) 다른 Transform을 선택

			//(Editing 상태가 아닐 때)
			//(Lock 걸리지 않았다면) Transform을 선택한다.
			// Child 선택이 가능하면 MeshTransform을 선택. 그렇지 않아면 MeshGroupTransform을 선택해준다.

			if (Editor.Select.ModRenderVerts_All == null)
			{
				return null;
			}

			int prevSelectedCount = Editor.Select.ModRenderVerts_All.Count;

			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}

			bool isChildMeshTransformSelectable = Editor.Select.Modifier.IsTarget_ChildMeshTransform;

			//apGizmos.SELECT_RESULT result = apGizmos.SELECT_RESULT.None;

			bool isTransformSelectable = false;

			//-------------------->>
			#region [미사용 코드] 단일 선택과 MRV 구버전 처리
			//if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None)
			//{
			//	//(Editing 상태일 때)
			//	//1. Vertex 선택
			//	//2. (Lock 걸리지 않았다면) 다른 Transform을 선택
			//	bool selectVertex = false;
			//	//if (Editor.Select.ExKey_ModMesh != null && Editor.Select.MeshGroup != null)
			//	if (
			//		//Editor.Select.ExKey_ModMesh != null 
			//		Editor.Select.ModMesh_Main != null //변경 20.6.18

			//		&& Editor.Select.MeshGroup != null)
			//	{
			//		//일단 선택한 Vertex가 클릭 가능한지 체크
			//		if (Editor.Select.ModRenderVertOfMod != null)
			//		{
			//			if (Editor.Select.ModRenderVertListOfMod.Count == 1)
			//			{
			//				if (Editor.Controller.IsVertexClickable(apGL.World2GL(Editor.Select.ModRenderVertOfMod._renderVert._pos_World), mousePosGL))
			//				{
			//					if (selectType == apGizmos.SELECT_TYPE.Subtract)
			//					{
			//						//삭제인 경우 : ModVertWeight를 선택한다.
			//						Editor.Select.RemoveModVertexOfModifier(null, null, Editor.Select.ModRenderVertOfMod._modVertWeight, Editor.Select.ModRenderVertOfMod._renderVert);
			//					}
			//					else
			//					{
			//						//그 외에는 => 그대로 갑시다.
			//						selectVertex = true;
			//						//return apGizmos.SELECT_RESULT.SameSelected;
			//					}
			//					//return Editor.Select.ModRenderVertListOfMod.Count;
			//					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			//				}
			//			}
			//			else
			//			{
			//				//여러개라고 하네요.
			//				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;
			//				for (int iModRenderVert = 0; iModRenderVert < modRenderVerts.Count; iModRenderVert++)
			//				{
			//					apSelection.ModRenderVert modRenderVert = modRenderVerts[iModRenderVert];

			//					if (Editor.Controller.IsVertexClickable(apGL.World2GL(modRenderVert._renderVert._pos_World), mousePosGL))
			//					{
			//						if (selectType == apGizmos.SELECT_TYPE.Subtract)
			//						{
			//							//삭제인 경우
			//							//하나 지우고 끝
			//							//결과는 List의 개수
			//							Editor.Select.RemoveModVertexOfModifier(null, null, modRenderVert._modVertWeight, modRenderVert._renderVert);
			//						}
			//						else if (selectType == apGizmos.SELECT_TYPE.Add)
			//						{
			//							//Add 상태에서 원래 선택된걸 누른다면
			//							//추가인 경우 => 그대로
			//							selectVertex = true;
			//						}
			//						else
			//						{
			//							//만약... new 라면?
			//							//다른건 초기화하고
			//							//얘만 선택해야함
			//							apRenderVertex selectedRenderVert = modRenderVert._renderVert;
			//							apModifiedVertexWeight selectedModVertWeight = modRenderVert._modVertWeight;
			//							Editor.Select.SetModVertexOfModifier(null, null, null, null);
			//							Editor.Select.SetModVertexOfModifier(null, null, selectedModVertWeight, selectedRenderVert);
			//						}

			//						//return Editor.Select.ModRenderVertListOfMod.Count;
			//						return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			//					}
			//				}
			//			}

			//		}

			//		if (selectType == apGizmos.SELECT_TYPE.New)
			//		{
			//			//Add나 Subtract가 아닐땐, 잘못 클릭하면 선택을 해제하자 (전부)
			//			Editor.Select.SetModVertexOfModifier(null, null, null, null);
			//		}

			//		if (selectType != apGizmos.SELECT_TYPE.Subtract)
			//		{
			//			if (
			//				//Editor.Select.ExKey_ModMesh._transform_Mesh != null &&
			//				//Editor.Select.ExKey_ModMesh._vertices != null
			//				//변경 20.6.18
			//				Editor.Select.ModMesh_Main._transform_Mesh != null &&
			//				Editor.Select.ModMesh_Main._vertices != null
			//				)
			//			{
			//				//선택된 RenderUnit을 고르자
			//				//이전
			//				//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

			//				//변경 20.6.18
			//				//다음 중 하나를 고르자
			//				//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ModMesh_Main._transform_Mesh);//<1>
			//				apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;//<2>

			//				if (targetRenderUnit != null)
			//				{
			//					for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			//					{
			//						apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
			//						bool isClick = Editor.Controller.IsVertexClickable(apGL.World2GL(renderVert._pos_World), mousePosGL);
			//						if (isClick)
			//						{
			//							//apModifiedVertexWeight selectedModVertWeight = Editor.Select.ExKey_ModMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
			//							apModifiedVertexWeight selectedModVertWeight = Editor.Select.ModMesh_Main._vertWeights.Find(delegate (apModifiedVertexWeight a)
			//							{
			//								return renderVert._vertex._uniqueID == a._vertexUniqueID;
			//							});

			//							if (selectedModVertWeight != null)
			//							{
			//								if (selectType == apGizmos.SELECT_TYPE.New)
			//								{
			//									Editor.Select.SetModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
			//								}
			//								else if (selectType == apGizmos.SELECT_TYPE.Add)
			//								{
			//									Editor.Select.AddModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
			//								}

			//								selectVertex = true;
			//								//result = apGizmos.SELECT_RESULT.NewSelected;
			//								break;
			//							}

			//						}
			//					}
			//				}
			//			}
			//		}
			//	}

			//	//Vertex를 선택한게 없다면
			//	//+ Lock 상태가 아니라면
			//	if (!selectVertex && !Editor.Select.IsSelectionLock)
			//	{
			//		//Transform을 선택
			//		isTransformSelectable = true;
			//	}
			//}
			//else
			//{
			//	//(Editing 상태가 아닐때)
			//	isTransformSelectable = true;

			//	if (
			//		//Editor.Select.ExKey_ModMesh != null 
			//		Editor.Select.ModMesh_Main != null //변경 20.6.18
			//		&& Editor.Select.IsSelectionLock)
			//	{
			//		//뭔가 선택된 상태에서 Lock이 걸리면 다른건 선택 불가
			//		isTransformSelectable = false;
			//	}
			//} 
			#endregion


			//변경 20.6.26 : MRV 신버전 방식
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None)
			{
				//(Editing 상태일 때)
				//1. Vertex 선택
				//2. (Lock 걸리지 않았다면) 다른 Transform을 선택
				bool selectVertex = false;


				//변경 22.4.9 : 버텍스 클릭 개선용 변수들
				apSelection.ModRenderVert clickedMRV_Direct = null;
				apSelection.ModRenderVert clickedMRV_Wide = null;
				float minWideDist = 0.0f;
				float curWideDist = 0.0f;
				apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;


				if (Editor.Select.ModMesh_Main != null
					&& Editor.Select.ModMeshes_All != null//<<다중 선택된 ModMesh도 체크한다.
					&& Editor.Select.MeshGroup != null)
				{
					//일단 선택한 Vertex가 클릭 가능한지 체크
					if (Editor.Select.ModRenderVert_Main != null)
					{
						//변경 22.4.9 : 버텍스 선택 로직 개선 변수 초기화
						clickedMRV_Direct = null;
						clickedMRV_Wide = null;
						minWideDist = 0.0f;
						curWideDist = 0.0f;
						clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

						if (Editor.Select.ModRenderVerts_All.Count == 1)
						{
							Vector2 rPosGL = apGL.World2GL(Editor.Select.ModRenderVert_Main._renderVert._pos_World);
							

							clickResult = Editor.Controller.IsVertexClickable(ref rPosGL, ref mousePosGL, ref curWideDist);

							//메인은 Wide도 그냥 선택
							if(clickResult != apEditorController.VERTEX_CLICK_RESULT.None)
							{
								if (selectType == apGizmos.SELECT_TYPE.Subtract)
								{
									//삭제인 경우 : ModVertWeight를 선택한다.
									Editor.Select.RemoveModVertexOfModifier(Editor.Select.ModRenderVert_Main);
								}
								else
								{
									//그 외에는 => 그대로 갑시다.
									selectVertex = true;
								}
								return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
							}
						}
						else
						{
							//여러개라고 하네요.
							List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
							apSelection.ModRenderVert curMRV = null;
							for (int iModRenderVert = 0; iModRenderVert < modRenderVerts.Count; iModRenderVert++)
							{
								curMRV = modRenderVerts[iModRenderVert];

								Vector2 rPosGL = apGL.World2GL(curMRV._renderVert._pos_World);

								clickResult = Editor.Controller.IsVertexClickable(ref rPosGL, ref mousePosGL, ref curWideDist);

								if(clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
								{
									continue;
								}

								if(clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
								{
									//정확히 클릭함
									clickedMRV_Direct = curMRV;
									break;
								}

								//Wide 클릭함 : 거리 비교
								if(clickedMRV_Wide == null || curWideDist < minWideDist)
								{
									clickedMRV_Wide = curMRV;
									minWideDist = curWideDist;
								}
							}

							//클릭 결과
							apSelection.ModRenderVert resultClicked = null;
							if(clickedMRV_Direct != null)		{ resultClicked = clickedMRV_Direct; }
							else if(clickedMRV_Wide != null)	{ resultClicked = clickedMRV_Wide; }


							if (resultClicked != null)
							{
								if (selectType == apGizmos.SELECT_TYPE.Subtract)
								{
									//삭제인 경우 : 하나 지우고 끝
									//결과는 List의 개수
									Editor.Select.RemoveModVertexOfModifier(resultClicked);
								}
								else if (selectType == apGizmos.SELECT_TYPE.Add)
								{
									//Add 상태에서 원래 선택된걸 누른다면 : 추가인 경우 => 그대로
									selectVertex = true;
								}
								else
								{
									//만약... new 라면? : 다른건 초기화하고 얘만 선택해야함
									Editor.Select.SelectModRenderVertOfModifier(resultClicked);
								}

								return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
							}
						}

					}

					if (selectType == apGizmos.SELECT_TYPE.New)
					{
						//Add나 Subtract가 아닐땐, 잘못 클릭하면 선택을 해제하자 (전부)
						//이전
						//Editor.Select.SetModVertexOfModifier(null, null, null, null);

						//변경 20.6.26
						Editor.Select.SelectModRenderVertOfModifier(null);
					}

					if (selectType != apGizmos.SELECT_TYPE.Subtract)
					{
						//변경 사항 20.6.25 : 기존에는 ModMesh의 Vertex를 찾아서 생성을 선택 > MRV로 생성했는데,
						//이제는 아예 MRV가 생성되어있는 상태이다.
						//이 부분이 가장 크게 바뀌었다.
						List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModData.ModRenderVert_All;
						int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;
						if (nMRVs > 0)
						{
							apSelection.ModRenderVert curMRV = null;

							//변경 22.4.9 : 버텍스 선택 로직 개선 변수 초기화
							clickedMRV_Direct = null;
							clickedMRV_Wide = null;
							minWideDist = 0.0f;
							curWideDist = 0.0f;
							clickResult = apEditorController.VERTEX_CLICK_RESULT.None;


							for (int iMRV = 0; iMRV < nMRVs; iMRV++)
							{
								curMRV = modRenderVerts[iMRV];

								Vector2 rVertPosGL = apGL.World2GL(curMRV._renderVert._pos_World);

								clickResult = Editor.Controller.IsVertexClickable(ref rVertPosGL, ref mousePosGL, ref curWideDist);

								if (clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
								{
									continue;
								}

								if (clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
								{
									//정확하게 클릭했다.
									clickedMRV_Direct = curMRV;
									break;
								}

								// Wide 클릭 : 거리비교
								if (clickedMRV_Wide == null || curWideDist < minWideDist)
								{
									clickedMRV_Wide = curMRV;
									minWideDist = curWideDist;
								}
							}

							//클릭 결과
							apSelection.ModRenderVert resultClicked = null;
							if (clickedMRV_Direct != null)		{ resultClicked = clickedMRV_Direct; }
							else if (clickedMRV_Wide != null)	{ resultClicked = clickedMRV_Wide; }

							if (resultClicked != null)
							{
								if (selectType == apGizmos.SELECT_TYPE.New)
								{
									Editor.Select.SelectModRenderVertOfModifier(resultClicked);
								}
								else if (selectType == apGizmos.SELECT_TYPE.Add)
								{
									Editor.Select.AddModRenderVertOfModifier(resultClicked);
								}

								selectVertex = true;
							}
						}

					}
				}

				//Vertex를 선택한게 없다면
				//+ Lock 상태가 아니라면
				if (!selectVertex && !Editor.Select.IsSelectionLock)
				{
					//Transform을 선택
					isTransformSelectable = true;
				}
			}
			else
			{
				//(Editing 상태가 아닐때)
				isTransformSelectable = true;

				if (
					//Editor.Select.ExKey_ModMesh != null 
					Editor.Select.ModMesh_Main != null //변경 20.6.18
					&& Editor.Select.IsSelectionLock)
				{
					//뭔가 선택된 상태에서 Lock이 걸리면 다른건 선택 불가
					isTransformSelectable = false;
				}
			}



			//-------------------->>

			if (isTransformSelectable && selectType == apGizmos.SELECT_TYPE.New)
			{
				//(Editing 상태가 아닐 때)
				//Transform을 선택한다.

				apTransform_Mesh selectedMeshTransform = null;

				//정렬된 Render Unit
				//List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<이전 : RenderUnits_All 이용
				List<apRenderUnit> renderUnits = Editor.Select.MeshGroup.SortedRenderUnits;//<<변경 : Sorted 리스트 이용

				//이전
				//for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)

				//변경 20.7.3 : 피킹 순서를 "앞에서"부터 하자
				if (renderUnits.Count > 0)
				{
					apRenderUnit renderUnit = null;
					for (int iUnit = renderUnits.Count - 1; iUnit >= 0; iUnit--)
					{
						renderUnit = renderUnits[iUnit];
						if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
						{
							//if (renderUnit._meshTransform._isVisible_Default && renderUnit._meshColor2X.a > 0.1f)//이전
							if (renderUnit._isVisible && renderUnit._meshColor2X.a > 0.1f)//변경
							{
								//Debug.LogError("TODO : Mouse Picking 바꿀것");
								bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(
									mousePosGL, renderUnit);

								if (isPick)
								{
									selectedMeshTransform = renderUnit._meshTransform;
									//찾았어도 계속 찾는다.
									//뒤의 아이템이 "앞쪽"에 있는 것이기 때문

									//변경 20.7.3
									//루프 순서가 바뀌어서 바로 Break를 해도 된다.
									break;
								}
							}
						}
					}
				}

				if (selectedMeshTransform != null)
				{
					//만약 ChildMeshGroup에 속한 거라면,
					//Mesh Group 자체를 선택해야 한다. <- 추가 : Child Mesh Transform이 허용되는 경우 그럴 필요가 없다.
					apMeshGroup parentMeshGroup = Editor.Select.MeshGroup.FindParentMeshGroupOfMeshTransform(selectedMeshTransform);

					object selectedObj = null;

					if (parentMeshGroup == null || parentMeshGroup == Editor.Select.MeshGroup || isChildMeshTransformSelectable)
					{
						Editor.Select.SelectMeshTF(selectedMeshTransform, apSelection.MULTI_SELECT.Main);
						selectedObj = selectedMeshTransform;
					}
					else
					{
						apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
						if (childMeshGroupTransform != null)
						{
							Editor.Select.SelectMeshGroupTF(childMeshGroupTransform, apSelection.MULTI_SELECT.Main);//무조건 Main
							selectedObj = childMeshGroupTransform;
						}
						else
						{
							Editor.Select.SelectMeshTF(selectedMeshTransform, apSelection.MULTI_SELECT.Main);
							selectedObj = selectedMeshTransform;
						}
					}

					//[1.4.2] 선택된 객체에 맞게 자동 스크롤
					if(Editor._option_AutoScrollWhenObjectSelected)
					{
						//스크롤 가능한 상황인지 체크하고
						if(Editor.IsAutoScrollableWhenClickObject_MeshGroup(selectedObj, true))
						{
							//자동 스크롤을 요청한다.
							Editor.AutoScroll_HierarchyMeshGroup(selectedObj);
						}
					}
				}
				else
				{
					Editor.Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
				}

				//Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			//개수에 따라 한번더 결과 보정
			if (Editor.Select.ModRenderVerts_All != null)
			{
				//return Editor.Select.ModRenderVertListOfMod.Count;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}
			return null;
		}


		public void Unselect__Modifier_Physics()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			//이전
			//Editor.Select.SetModVertexOfModifier(null, null, null, null);
			//변경 20.6.26
			Editor.Select.SelectModRenderVertOfModifier(null);

			if (!Editor.Select.IsSelectionLock)
			{
				Editor.Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
			}

			//Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}


		//---------------------------------------------------------------------------------------
		// 단축키
		//---------------------------------------------------------------------------------------
		public void AddHotKeys__Modifier_Physics(bool isGizmoRenderable, apGizmos.CONTROL_TYPE controlType, bool isFFDMode)
		{
			//Editor.AddHotKeyEvent(OnHotKeyEvent__Modifier_Physics__Ctrl_A, apHotKey.LabelText.SelectAllVertices, KeyCode.A, false, false, true, null);
			Editor.AddHotKeyEvent(OnHotKeyEvent__Modifier_Physics__Ctrl_A, apHotKeyMapping.KEY_TYPE.SelectAllVertices_EditMod, null);//변경
		}

		// 단축키 : 버텍스 전체 선택
		private apHotKey.HotKeyResult OnHotKeyEvent__Modifier_Physics__Ctrl_A(object paramObject)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}


			if (Editor.Select.ModRenderVerts_All == null)
			{
				return null;
			}


			//-----------------------------------
			#region [미사용 코드] 단일 처리
			//if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 

			//	//&& Editor.Select.ExKey_ModMesh != null 
			//	&& Editor.Select.ModMesh_Main != null //변경 20.6.18

			//	&& Editor.Select.MeshGroup != null)
			//{
			//	//선택된 RenderUnit을 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

			//	//변경 20.6.18
			//	//다음 중 하나를 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ModMesh_Main._transform_Mesh);//<1>
			//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;//<2>

			//	if (targetRenderUnit != null)
			//	{
			//		//모든 버텍스를 선택
			//		for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			//		{
			//			apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];

			//			//apModifiedVertexWeight selectedModVertWeight = Editor.Select.ExKey_ModMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
			//			apModifiedVertexWeight selectedModVertWeight = Editor.Select.ModMesh_Main._vertWeights.Find(delegate (apModifiedVertexWeight a)
			//			{
			//				return renderVert._vertex._uniqueID == a._vertexUniqueID;
			//			});
			//			Editor.Select.AddModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
			//		}

			//		Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);

			//		Editor.RefreshControllerAndHierarchy(false);
			//		//Editor.Repaint();
			//		Editor.SetRepaint();

			//		Editor.Select.AutoSelectModMeshOrModBone();
			//	}
			//} 
			#endregion

			//변경 20.6.26 : MRV 신버전
			bool isAnyChanged = false;
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 
				&& Editor.Select.ModMesh_Main != null //변경 20.6.18
				&& Editor.Select.ModMeshes_All != null//<<다중 선택된 ModMesh도 체크한다.
				&& Editor.Select.MeshGroup != null)
			{
				//"이미 생성된" MRV 들을 한번에 선택하자
				Editor.Select.AddModRenderVertsOfModifier(Editor.Select.ModData.ModRenderVert_All);
				isAnyChanged = true;
			}

			if (isAnyChanged)
			{
				Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);

				//Editor.Select.AutoSelectModMeshOrModBone();

				Editor.SetRepaint();//<<추가
			}
			//-----------------------------------
			return apHotKey.HotKeyResult.MakeResult();
		}

		//---------------------------------------------------------------------------------------

		/// <summary>
		/// Physics Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [복수 선택]
		/// </summary>
		/// <param name="mousePosGL_Min"></param>
		/// <param name="mousePosGL_Max"></param>
		/// <param name="mousePosW_Min"></param>
		/// <param name="mousePosW_Max"></param>
		/// <param name="areaSelectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult MultipleSelect__Modifier_Physics(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}


			if (Editor.Select.ModRenderVerts_All == null)
			{
				return null;
			}
			// 이건 다중 버텍스 선택밖에 없다.
			//Transform 선택은 없음


			bool isAnyChanged = false;

			//-------------------------------
			#region [미사용 코드] 단일 처리
			//if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 

			//	//&& Editor.Select.ExKey_ModMesh != null 
			//	&& Editor.Select.ModMesh_Main != null 

			//	&& Editor.Select.MeshGroup != null)
			//{
			//	//선택된 RenderUnit을 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

			//	//변경 20.6.18
			//	//다음 중 하나를 고르자
			//	//apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ModMesh_Main._transform_Mesh);//<1>
			//	apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;//<2>

			//	if (targetRenderUnit != null)
			//	{
			//		for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
			//		{
			//			apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
			//			bool isSelectable = (mousePosW_Min.x < renderVert._pos_World.x && renderVert._pos_World.x < mousePosW_Max.x)
			//						&& (mousePosW_Min.y < renderVert._pos_World.y && renderVert._pos_World.y < mousePosW_Max.y);
			//			if (isSelectable)
			//			{
			//				//apModifiedVertexWeight selectedModVertWeight = Editor.Select.ExKey_ModMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
			//				apModifiedVertexWeight selectedModVertWeight = Editor.Select.ModMesh_Main._vertWeights.Find(delegate (apModifiedVertexWeight a)
			//				{
			//					return renderVert._vertex._uniqueID == a._vertexUniqueID;
			//				});

			//				if (selectedModVertWeight != null)
			//				{
			//					if (areaSelectType == apGizmos.SELECT_TYPE.Add ||
			//						areaSelectType == apGizmos.SELECT_TYPE.New)
			//					{
			//						Editor.Select.AddModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
			//					}
			//					else
			//					{
			//						Editor.Select.RemoveModVertexOfModifier(null, null, selectedModVertWeight, renderVert);
			//					}

			//					isAnyChanged = true;
			//				}

			//			}
			//		}

			//		Editor.RefreshControllerAndHierarchy(false);
			//		//Editor.Repaint();
			//		Editor.SetRepaint();
			//	}
			//} 
			#endregion

			//변경 20.6.25 : MRV 새로운 방식
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 
				&& Editor.Select.ModMesh_Main != null //변경
				&& Editor.Select.ModMeshes_All != null //다중 선택된 ModMesh도 체크한다.
				&& Editor.Select.MeshGroup != null)
			{
				//변경 사항 20.6.25 : 기존에는 ModMesh의 Vertex를 찾아서 생성을 선택 > MRV로 생성했는데,
				//이제는 아예 MRV가 생성되어있는 상태이다.
				//이 부분이 가장 크게 바뀌었다.

				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModData.ModRenderVert_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;
				if (nMRVs > 0)
				{
					apSelection.ModRenderVert curMRV = null;
					for (int iMRV = 0; iMRV < nMRVs; iMRV++)
					{
						curMRV = modRenderVerts[iMRV];

						bool isSelectable = (mousePosW_Min.x < curMRV._renderVert._pos_World.x && curMRV._renderVert._pos_World.x < mousePosW_Max.x)
												&& (mousePosW_Min.y < curMRV._renderVert._pos_World.y && curMRV._renderVert._pos_World.y < mousePosW_Max.y);
						if (isSelectable)
						{
							if (areaSelectType == apGizmos.SELECT_TYPE.Add || areaSelectType == apGizmos.SELECT_TYPE.New)
							{
								Editor.Select.AddModRenderVertOfModifier(curMRV);
							}
							else
							{
								Editor.Select.RemoveModVertexOfModifier(curMRV);
							}

							isAnyChanged = true;
						}
					}
				}
			}
			//-------------------------------

			if (isAnyChanged)
			{
				Editor.Select.AutoSelectModMeshOrModBone();
				Editor.SetRepaint();//<<추가
			}

			//return Editor.Select.ModRenderVertListOfMod.Count;
			return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
		}


		public apGizmos.TransformParam PivotReturn__Modifier_Physics()
		{
			//Weight는 Pivot이 없다.
			return null;
		}
	}
}