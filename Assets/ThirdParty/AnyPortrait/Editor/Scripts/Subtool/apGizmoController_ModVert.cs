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

	//GizmoController -> Modifier [Vertex를 선택하는 타입]에 대한 내용이 담겨있다.
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
		// Gizmo - MeshGroup : Modifier / Morph계열 및 Vertex를 선택하는 Weight 계열의 모디파이어
		//----------------------------------------------------------------
		/// <summary>
		/// Modifier [Morph]에 대한 Gizmo Event의 Set이다.
		/// </summary>
		/// <returns></returns>
		public apGizmos.GizmoEventSet GetEventSet_Modifier_Morph()
		{
			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Modifier_Vertex,
														Unselect__Modifier_Vertex, 
														Move__Modifier_VertexPos, 
														Rotate__Modifier_VertexPos, 
														Scale__Modifier_VertexPos, 
														PivotReturn__Modifier_Vertex);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__Modifier_VertexPos,
																null,
																null,
																null,
																TransformChanged_Color__Modifier_Vertex,
																TransformChanged_Extra__Modifier_Vertex,
																apGizmos.TRANSFORM_UI.Position2D 
																	| apGizmos.TRANSFORM_UI.Vertex_Transform 
																	| apGizmos.TRANSFORM_UI.Color
																	| apGizmos.TRANSFORM_UI.Extra
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__Modifier_Vertex, 
														FFDTransform__Modifier_VertexPos, 
														StartFFDTransform__Modifier_VertexPos, 
														SoftSelection__Modifier_VertexPos, 
														SyncBlurStatus__Modifier_VertexPos, 
														PressBlur__Modifier_VertexPos);
			
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Modifier_Vertex, 
																AddHotKeys__Modifier_Vertex, 
																OnHotKeyEvent__Modifier_Vertex__Keyboard_Move, 
																OnHotKeyEvent__Modifier_Vertex__Keyboard_Rotate, 
																OnHotKeyEvent__Modifier_Vertex__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;
		}




		public apGizmos.SelectResult FirstLink__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			//편집 대상에 따라 다르다. (v1.4.0)
			if(Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				//[ 버텍스 편집 모드 ] 일 때
				if (Editor.Select.ModRenderVerts_All != null)
				{	
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
				}
			}
			else
			{
				//[ 핀 편집 모드] 일 때 (22.4.9) [v1.4.0]
				if(Editor.Select.ModRenderPins_All != null)
				{
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
				}
			}


			
			return null;
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Vertex(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
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
			// Child 선택이 가능하면 MeshTransform을 선택. 그렇지 않으면 MeshGroupTransform을 선택해준다.


			//마우스 클릭 위치가 잘못되었다면
			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					// [ 버텍스 편집 모드 ] 일 때
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
				}
				else
				{
					// [ 핀 편집 모드 ] 일 때
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
				}
				
			}

			bool isChildMeshTransformSelectable = Editor.Select.Modifier.IsTarget_ChildMeshTransform;

			//추가 20.5.27 : 다중 선택
			//Vertex Mod는 다중 선택이 가능하다.
			apSelection.MULTI_SELECT multiSelect = (selectType == apGizmos.SELECT_TYPE.Add) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main;



			bool isTransformSelectable = false;


			//변경 20.6.25 : MRV 신버전 방식
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None)
			{
				//(Editing 상태일 때)
				//1. Vertex 선택
				//2. (Lock 걸리지 않았다면) 다른 Transform을 선택

				//변경 22.4.9 (v1.4.0) 편집 모드에 따라 선택하는 대상이 바뀐다.
				bool vertOrPinSelected = false;

				
				if (Editor.Select.ModMesh_Main != null && 
					Editor.Select.ModMeshes_All != null &&//<<다중 선택된 ModMesh도 체크한다.
					Editor.Select.MeshGroup != null)
				{
					if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
					{
						// [ 버텍스 편집 모드 ]

						//버텍스 선택 로직 개선 (22.4.9)
						apSelection.ModRenderVert clickedMRV_Direct = null;
						apSelection.ModRenderVert clickedMRV_Wide = null;
						float minWideDist = 0.0f;
						float curWideDist = 0.0f;
						apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;


						//일단 선택한 Vertex가 클릭 가능한지 체크
						if (Editor.Select.ModRenderVert_Main != null)
						{
							if (Editor.Select.ModRenderVerts_All.Count == 1)
							{
								Vector2 rVertPosGL = apGL.World2GL(Editor.Select.ModRenderVert_Main._renderVert._pos_World);
								clickResult = Editor.Controller.IsVertexClickable(ref rVertPosGL, ref mousePosGL, ref curWideDist);

								//하나 클릭한 경우엔 Wide라도 확정 선택
								if (clickResult != apEditorController.VERTEX_CLICK_RESULT.None)
								{
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우
										Editor.Select.RemoveModVertexOfModifier(Editor.Select.ModRenderVert_Main);

										if (Editor.Select.ModRenderVerts_All.Count > 0)
										{
											vertOrPinSelected = true;
										}
									}
									else
									{
										//그 외에는 => 그대로 갑시다.
										vertOrPinSelected = true;
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

									//Wide 클릭 : 거리 비교
									if (clickedMRV_Wide == null || curWideDist < minWideDist)
									{
										clickedMRV_Wide = curMRV;
										minWideDist = curWideDist;
									}
								}
								//클릭 결과 체크
								apSelection.ModRenderVert resultClicked = null;
								if (clickedMRV_Direct != null)		{ resultClicked = clickedMRV_Direct; }
								else if (clickedMRV_Wide != null)	{ resultClicked = clickedMRV_Wide; }

								if (resultClicked != null)
								{
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우 > 하나 지우고 끝
										//결과는 List의 개수

										Editor.Select.RemoveModVertexOfModifier(resultClicked);

										if (Editor.Select.ModRenderVerts_All.Count > 0)
										{
											vertOrPinSelected = true;
										}
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//Add 상태에서 원래 선택된걸 누른다면 : 추가인 경우 => 그대로
										vertOrPinSelected = true;
									}
									else
									{
										//만약... new 라면? : 다른건 초기화하고 얘만 선택해야함
										Editor.Select.SelectModRenderVertOfModifier(resultClicked);
									}

									//선택 후 바로 종료
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
								}
							}

						}

						//선택된 거를 다시 선택하지 않았다.
						//새로 선택하자.

						if (selectType == apGizmos.SELECT_TYPE.New)
						{
							//Add나 Subtract가 아닐땐, 잘못 클릭하면 선택을 해제하자 (전부)
							Editor.Select.SelectModRenderVertOfModifier(null);
						}

						if (selectType != apGizmos.SELECT_TYPE.Subtract)
						{
							//변경 사항 20.6.25 : 기존에는 ModMesh의 Vertex를 찾아서 생성을 선택 > MRV로 생성했는데,
							//이제는 아예 MRV가 생성되어있는 상태이다.
							//이 부분이 가장 크게 바뀌었다.
							List<apSelection.ModRenderVert> selectableModRenderVerts = Editor.Select.ModData.ModRenderVert_All;
							int nMRVs = selectableModRenderVerts != null ? selectableModRenderVerts.Count : 0;
							if (nMRVs > 0)
							{
								apSelection.ModRenderVert curMRV = null;

								clickedMRV_Direct = null;
								clickedMRV_Wide = null;
								minWideDist = 0.0f;
								curWideDist = 0.0f;


								for (int iMRV = 0; iMRV < nMRVs; iMRV++)
								{
									curMRV = selectableModRenderVerts[iMRV];

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

									// Wide 클릭 : 거리 비교
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

									vertOrPinSelected = true;
								}
							}
						}
					}
					else
					{
						// [ 핀 편집 모드 ]
						//선택한 Pin을 선택할 수 있는지 체크
						apSelection.ModRenderPin clickedMRP_Direct = null;
						apSelection.ModRenderPin clickedMRP_Wide = null;
						float minWideDist = 0.0f;
						float curWideDist = 0.0f;

						apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

						//선택한 핀이 있는 경우
						if (Editor.Select.ModRenderPin_Main != null)
						{
							if(Editor.Select.ModRenderPins_All.Count == 1)
							{
								//한개의 MRP만 선택된 경우 > 해당 핀만 체크한다.
								Vector2 rPinPosGL = apGL.World2GL(Editor.Select.ModRenderPin_Main._renderPin._pos_World);
								clickResult = Editor.Controller.IsPinClickable(ref rPinPosGL, ref mousePosGL, ref curWideDist);

								//하나 클릭한 경우엔 Wide라도 확정 선택
								if(clickResult != apEditorController.VERTEX_CLICK_RESULT.None)
								{
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우
										Editor.Select.RemoveModPinOfModifier(Editor.Select.ModRenderPin_Main);

										if (Editor.Select.ModRenderPins_All.Count > 0)
										{
											vertOrPinSelected = true;
										}
									}
									else
									{
										//그 외에는 => 그대로 갑시다.
										vertOrPinSelected = true;
									}
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
								}
							}
							else
							{
								//여러개의 MRP가 선택된 경우
								List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
								apSelection.ModRenderPin curMRP = null;
								int nMRPs = modRenderPins.Count;

								for (int iMRP = 0; iMRP < nMRPs; iMRP++)
								{
									curMRP = modRenderPins[iMRP];

									Vector2 rPinPosGL = apGL.World2GL(curMRP._renderPin._pos_World);
									clickResult = Editor.Controller.IsPinClickable(ref rPinPosGL, ref mousePosGL, ref curWideDist);

									if(clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
									{
										continue;
									}

									if(clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
									{
										//정확하게 클릭했다.
										clickedMRP_Direct = curMRP;
										break;
									}

									//Wide 클릭 : 거리 비교
									if(clickedMRP_Wide == null || curWideDist < minWideDist)
									{
										clickedMRP_Wide = curMRP;
										minWideDist = curWideDist;
									}
								}

								//클릭 결과 체크
								apSelection.ModRenderPin resultClickedMRP = null;
								if(clickedMRP_Direct != null)		{ resultClickedMRP = clickedMRP_Direct; }
								else if(clickedMRP_Wide != null)	{ resultClickedMRP = clickedMRP_Wide; }

								if(resultClickedMRP != null)
								{
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우 > 하나 지우고 끝
										//결과는 List의 개수

										Editor.Select.RemoveModPinOfModifier(resultClickedMRP);

										if (Editor.Select.ModRenderPins_All.Count > 0)
										{
											vertOrPinSelected = true;
										}
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//Add 상태에서 원래 선택된걸 누른다면 : 추가인 경우 => 그대로
										vertOrPinSelected = true;
									}
									else
									{
										//만약... new 라면? : 다른건 초기화하고 얘만 선택해야함
										Editor.Select.SelectModRenderPinOfModifier(resultClickedMRP);
									}

									//선택 후 바로 종료
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
								}
								
							}
						}


						//선택된 핀을 클릭한게 아니다.
						//새로 찾아서 선택해야한다.
						if(selectType == apGizmos.SELECT_TYPE.New)
						{
							//새로 선택할 때는 체크 전에 선택된 핀을 모두 해제한다.
							Editor.Select.SelectModRenderPinOfModifier(null);
						}

						//New 또는 Add 방식으로 선택한다면
						if (selectType != apGizmos.SELECT_TYPE.Subtract)
						{
							List<apSelection.ModRenderPin> selectableModRenderPins = Editor.Select.ModData.ModRenderPin_All;//전체 선택 대상
							int nMRPs = selectableModRenderPins != null ? selectableModRenderPins.Count : 0;
							if (nMRPs > 0)
							{
								apSelection.ModRenderPin curMRP = null;

								clickedMRP_Direct = null;
								clickedMRP_Wide = null;
								minWideDist = 0.0f;
								curWideDist = 0.0f;


								for (int iMRP = 0; iMRP < nMRPs; iMRP++)
								{
									curMRP = selectableModRenderPins[iMRP];

									Vector2 rVertPosGL = apGL.World2GL(curMRP._renderPin._pos_World);

									clickResult = Editor.Controller.IsVertexClickable(ref rVertPosGL, ref mousePosGL, ref curWideDist);

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
									{
										continue;
									}

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
									{
										//정확하게 클릭했다.
										clickedMRP_Direct = curMRP;
										break;
									}

									// Wide 클릭 : 거리 비교
									if (clickedMRP_Wide == null || curWideDist < minWideDist)
									{
										clickedMRP_Wide = curMRP;
										minWideDist = curWideDist;
									}


								}

								//클릭 결과
								apSelection.ModRenderPin resultClickedPin = null;
								if (clickedMRP_Direct != null)		{ resultClickedPin = clickedMRP_Direct; }
								else if (clickedMRP_Wide != null)	{ resultClickedPin = clickedMRP_Wide; }

								if (resultClickedPin != null)
								{
									if (selectType == apGizmos.SELECT_TYPE.New)
									{
										Editor.Select.SelectModRenderPinOfModifier(resultClickedPin);
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										Editor.Select.AddModRenderPinOfModifier(resultClickedPin);
									}

									vertOrPinSelected = true;
								}
							}
						}

					}

					
				}

				//Vertex를 선택한게 없다면 + Selection Lock 상태가 아니라면 >> Transform을 선택할 수 있도록 하자
				if (!vertOrPinSelected && !Editor.Select.IsSelectionLock)
				{
					isTransformSelectable = true;
				}
			}
			else
			{
				//(Editing 상태가 아닐때)
				isTransformSelectable = true;

				if (Editor.Select.ModMesh_Main != null //변경 20.6.18
					&& Editor.Select.IsSelectionLock)
				{
					//뭔가 선택된 상태에서 Lock이 걸리면 다른건 선택 불가
					isTransformSelectable = false;
				}
			}


			// 여기서부터는 동일하다 (20.6.25 변경 사항으로부터)
			//---------------------------------------------------

			//메시 추가 선택 가능
			if (isTransformSelectable
				 //&& selectType == apGizmos.SELECT_TYPE.New//<<이거 삭제해야 제대로 다른 메시들이 선택된다.
				 )
			{
				//(Editing 상태가 아닐 때)
				//Transform을 선택한다.

				//추가 21.2.17 : 편집 중이 아닌 오브젝트를 선택하는 건 "편집 모드가 아니거나" / "선택 제한 옵션이 꺼진 경우"이다.
				bool isNotEditObjSelectable = Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || !Editor._exModObjOption_NotSelectable;


				apTransform_Mesh selectedMeshTransform = null;

				//정렬된 Render Unit
				List<apRenderUnit> renderUnits = Editor.Select.MeshGroup.SortedRenderUnits;//<<변경 : Sorted 리스트 이용

				//이전
				//for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)

				//변경 20.5.27 : 피킹 순서를 "앞에서"부터 하자
				if (renderUnits.Count > 0)
				{
					apRenderUnit renderUnit = null;
					for (int iUnit = renderUnits.Count - 1; iUnit >= 0; iUnit--)
					{
						renderUnit = renderUnits[iUnit];
						if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
						{
							//추가 21.2.17
							if (!isNotEditObjSelectable &&
								(renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_NotEdit || renderUnit._exCalculateMode == apRenderUnit.EX_CALCULATE.Disabled_ExRun))
							{
								//모디파이어에 등록되지 않은 메시는 선택 불가이다.
								//Debug.LogError("[" + renderUnit.Name + "] 옵션이 꺼져서 선택 불가");
								continue;
							}

							//if (renderUnit._meshTransform._isVisible_Default && renderUnit._meshColor2X.a > 0.1f)//이전
							if (renderUnit._isVisible && renderUnit._meshColor2X.a > 0.1f)//변경 21.7.20
							{
								//Debug.LogError("TODO : Mouse Picking 바꿀것");
								bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(
									mousePosGL, renderUnit);

								if (isPick)
								{
									selectedMeshTransform = renderUnit._meshTransform;
									//찾았어도 계속 찾는다. 뒤의 아이템이 "앞쪽"에 있는 것이기 때문
									//>> 피킹 순서가 "앞에서"부터 찾는거라 바로 break;하면 됨
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
						Editor.Select.SelectMeshTF(selectedMeshTransform, multiSelect);
						selectedObj = selectedMeshTransform;
					}
					else
					{
						apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
						if (childMeshGroupTransform != null)
						{
							Editor.Select.SelectMeshGroupTF(childMeshGroupTransform, multiSelect);
							selectedObj = childMeshGroupTransform;
						}
						else
						{
							Editor.Select.SelectMeshTF(selectedMeshTransform, multiSelect);
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
					//선택 없음 + 다중 선택이 아닌 경우
					if(multiSelect == apSelection.MULTI_SELECT.Main)
					{
						Editor.Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
					}
					
				}

				//Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			//개수에 따라 한번더 결과 보정 [ 편집 모드에 따라 다름 ] (22.4.11)
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드일 때 ]
				if (Editor.Select.ModRenderVerts_All != null)
				{
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
				}
			}
			else
			{
				// [ 핀 편집 모드일 때 ]
				if (Editor.Select.ModRenderPins_All != null)
				{
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
				}
			}
			
			return null;
		}



		public void Unselect__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}
			if(Editor.Gizmos.IsFFDMode)
			{
				//Debug.Log("IsFFD Mode");
				//추가 : FFD 모드에서는 버텍스 취소가 안된다.
				return;
			}

			//변경 22.4.11 : 모드에 따라 다르다.
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드의 경우 ]
				Editor.Select.SelectModRenderVertOfModifier(null);//변경 20.6.25
			}
			else
			{
				// [ 핀 편집 모드의 경우 ]
				Editor.Select.SelectModRenderPinOfModifier(null);//추가 22.4.11
			}


			if (!Editor.Select.IsSelectionLock)
			{
				//SubMesh 해제를 위해서는 Lock이 풀려있어야함
				Editor.Select.SelectMeshTF(null, apSelection.MULTI_SELECT.Main);
			}

			Editor.SetRepaint();
		}

		//-------------------------------------------------------------------------------------
		// 단축키 등록
		//-------------------------------------------------------------------------------------
		public void AddHotKeys__Modifier_Vertex(bool isGizmoRenderable, apGizmos.CONTROL_TYPE controlType, bool isFFDMode)
		{
			//Editor.AddHotKeyEvent(OnHotKeyEvent__Modifier_Vertex__Ctrl_A, apHotKey.LabelText.SelectAllVertices, KeyCode.A, false, false, true, null);
			Editor.AddHotKeyEvent(OnHotKeyEvent__Modifier_Vertex__Ctrl_A, apHotKeyMapping.KEY_TYPE.SelectAllVertices_EditMod, null);//변경 20.12.3
		}

		// 단축키 : 버텍스 전체 선택
		private apHotKey.HotKeyResult OnHotKeyEvent__Modifier_Vertex__Ctrl_A(object paramObject)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
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


			// 선택할 대상이 없으면 조기 리턴 : 편집 대상에 따라 조건 체크가 다르다 (22.4.11)
			
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				//선택된 MRV가 없다.
				int nSelectedMRVs = Editor.Select.ModRenderVerts_All != null ? Editor.Select.ModRenderVerts_All.Count : 0;
				if (nSelectedMRVs == 0)
				{
					return null;
				}
			}
			else
			{
				int nSelectedMRPs = Editor.Select.ModRenderPins_All != null ? Editor.Select.ModRenderPins_All.Count : 0;
				if(nSelectedMRPs == 0)
				{
					return null;
				}
			}
			
			bool isAnyChanged = false;


			//------------------------------

			//변경 20.6.25 : MRV 신버전 방식
			if (Editor.Select.ExEditingMode != apSelection.EX_EDIT.None 
				&& Editor.Select.ModMesh_Main != null //변경 20.6.18
				&& Editor.Select.ModMeshes_All != null//<<다중 선택된 ModMesh도 체크한다.
				&& Editor.Select.MeshGroup != null)
			{

				// "전체의" MRV/MRP 들을 한번에 선택하자
				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					// [버텍스 편집 모드에서]
					Editor.Select.AddModRenderVertsOfModifier(Editor.Select.ModData.ModRenderVert_All);
				}
				else
				{
					// [핀 편집 모드에서]
					Editor.Select.AddModRenderPinsOfModifier(Editor.Select.ModData.ModRenderPin_All);
				}
				
				isAnyChanged = true;
			}
			//------------------------------- > 변경 범위

			if (isAnyChanged)
			{
				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					// [버텍스 편집 모드에서]
					Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
				}
				else
				{
					// [핀 편집 모드에서]
					Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
				}
				//Editor.Select.AutoSelectModMeshOrModBone();

				Editor.SetRepaint();//<<추가

			}
			return apHotKey.HotKeyResult.MakeResult();
		}

		//추가 20.1.27 : 키보드로 버텍스 이동
		private bool OnHotKeyEvent__Modifier_Vertex__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			//Move__Modifier_VertexPos 함수의 코드를 이용함

			//이걸로 선택된 버텍스를 이동하는 로직을 만들어보자
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return false;
			}

			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				
				//|| Editor.Select.ExKey_ModMesh == null//이전
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18
				
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVert_Main == null)
			{
				return false;
			}

			//Undo
			
			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex, 
												Editor, 
												Editor.Select.Modifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if (Editor.Select.ModRenderVerts_All.Count == 1)
			{
				//1. 단일 선택일 때
				apRenderVertex renderVert = Editor.Select.ModRenderVert_Main._renderVert;
				
				//이전
				//renderVert.Calculate();

				//변경 22.5.9 [v1.4.0 : 최적화로 단일 Calculate 함수를 호출할 수 없다.)
				renderVert._parentRenderUnit.CalculateTargetRenderVert(renderVert);



				Vector2 prevDeltaPos2 = Editor.Select.ModRenderVert_Main._modVert._deltaPos;
				
				//이전
				//apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos2, 0, Vector2.one);
				//Vector2 prevWorldPos2 = (	renderVert._matrix_Cal_VertWorld 
				//							* renderVert._matrix_MeshTransform 
				//							* renderVert._matrix_Rigging
				//							* martrixMorph 
				//							* renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

				//변경 22.5.9 : 최적화
				Vector2 prevMorphPos = renderVert._vertex._pos;
					
				//Mesh의 Vert2Local 변환
				prevMorphPos.x += renderVert._parentMesh._matrix_VertToLocal._m02;
				prevMorphPos.y += renderVert._parentMesh._matrix_VertToLocal._m12;

				//Vert Local 연산값을 교체
				prevMorphPos += prevDeltaPos2;

				//Rigging > MeshTF > Vert World 변환 적용
				Vector2 prevWorldPos2 = renderVert._matrix_Rigging_MeshTF_VertWorld.MultiplyPoint(prevMorphPos);


				Vector2 nextWorldPos = new Vector2(prevWorldPos2.x, prevWorldPos2.y) + deltaMoveW;
				
				//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
				//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
				//기존 계산 : Matrix를 구해서 일일이 계산한다.
				
				
				//이전
				//Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
					
				//변경 22.5.9
				Vector2 noneMorphedPosM = renderVert._vertex._pos;
				noneMorphedPosM.x += renderVert._parentMesh._matrix_VertToLocal._m02;
				noneMorphedPosM.y += renderVert._parentMesh._matrix_VertToLocal._m12;
				
				//이전
				//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);

				//변경 21.4.5 : 리깅 적용
				//Vector2 nextMorphedPosM = ((	renderVert._matrix_Cal_VertWorld 
				//								* renderVert._matrix_MeshTransform
				//								* renderVert._matrix_Rigging).inverse).MultiplyPoint(nextWorldPos);

				//변경 22.5.9 : 최적화 코드 적용
				Vector2 nextMorphedPosM = (renderVert._matrix_Rigging_MeshTF_VertWorld.inverse).MultiplyPoint(nextWorldPos);

				Editor.Select.ModRenderVert_Main._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				Editor.Select.ModRenderVert_Main._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
			}
			else
			{
				//2. 복수개 선택일 때
				for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];
					apRenderVertex renderVert = modRenderVert._renderVert;
					Vector2 nextWorldPos = renderVert._pos_World + deltaMoveW;
					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos = (renderVert._pos_World + deltaMoveW) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();

			return true;
		}


		//추가 20.1.27 : 키보드로 버텍스 회전
		private bool OnHotKeyEvent__Modifier_Vertex__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__Modifier_VertexPos 함수의 코드를 이용함

			if (Editor.Select.MeshGroup == null 
				|| Editor.Select.Modifier == null
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None

				//|| Editor.Select.ExKey_ModMesh == null
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return false;
			}


			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;
			
			if (deltaAngleW > 180.0f)		{ deltaAngleW -= 360.0f; }
			else if (deltaAngleW < -180.0f)	{ deltaAngleW += 360.0f; }

			apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_RotateVertex, 
													Editor, Editor.Select.Modifier, 
													//null,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

				Vector2 nextWorldPos = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}


			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();

			return true;


		}

		//추가 20.1.27 : 키보드로 버텍스 크기 설정
		private bool OnHotKeyEvent__Modifier_Vertex__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__Modifier_VertexPos 함수의 코드를 이용
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				
				//|| Editor.Select.ExKey_ModMesh == null
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18

				|| Editor.Select.ModRenderVerts_All == null
				|| Editor.Select.ModRenderVerts_All.Count <= 1)
			{
				return false;
			}

			Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;
			//Vector3 centerPos3 = new Vector3(centerPos.x, centerPos.y, 0.0f);

			Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

			apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
				* apMatrix3x3.TRS(Vector2.zero, 0, scale)
				* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

			//Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0];
			//	isMultipleVerts = false;
			//}

			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_ScaleVertex, 
													Editor, Editor.Select.Modifier, 
													//null,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}



			//선택된 RenderVert의 Mod 값을 바꾸자
			for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

				Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);

				modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			}

			//Soft Selection 상태일때
			if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			{
				for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
				{
					apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
					float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

					apRenderVertex renderVert = modRenderVert._renderVert;

					//Weight를 적용한 만큼만 움직이자
					Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);
					Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

					modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();

			return true;
		}
		
		//-------------------------------------------------------------------------------------
		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex 계열 선택시 [복수 선택]
		/// </summary>
		/// <param name="mousePosGL_Min"></param>
		/// <param name="mousePosGL_Max"></param>
		/// <param name="mousePosW_Min"></param>
		/// <param name="mousePosW_Max"></param>
		/// <param name="areaSelectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult MultipleSelect__Modifier_Vertex(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			
			

			// 이건 다중 버텍스 선택밖에 없다.
			//Transform 선택은 없음

			//----------------------------------------------
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.ModMesh_Main == null
				|| Editor.Select.ModMeshes_All == null
				|| Editor.Select.MeshGroup == null)
			{
				//실패
				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					// [ 버텍스 편집 모드 ] 일 때
					if(Editor.Select.ModRenderVerts_All == null)
					{
						return null;
					}
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
				}
				else
				{
					// [ 핀 편집 모드 ] 일 때
					if(Editor.Select.ModRenderPins_All == null)
					{
						return null;
					}
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
				}
			}

			bool isAnyChanged = false;


			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]

				//변경 20.6.25 : MRV 새로운 방식
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

						if(!isSelectable) { continue; }

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
			else
			{
				// [ 핀 편집 모드 ]
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModData.ModRenderPin_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

				if(nMRPs > 0)
				{
					
					apSelection.ModRenderPin curMRP = null;
					for (int iMRP = 0; iMRP < nMRPs; iMRP++)
					{
						curMRP = modRenderPins[iMRP];

						bool isSelectable = 
							mousePosW_Min.x < curMRP._renderPin._pos_World.x && curMRP._renderPin._pos_World.x < mousePosW_Max.x
							&& mousePosW_Min.y < curMRP._renderPin._pos_World.y && curMRP._renderPin._pos_World.y < mousePosW_Max.y;

						if(!isSelectable)
						{
							continue;
						}

						if (areaSelectType == apGizmos.SELECT_TYPE.Add || areaSelectType == apGizmos.SELECT_TYPE.New)
						{
							Editor.Select.AddModRenderPinOfModifier(curMRP);
							
						}
						else
						{
							Editor.Select.RemoveModPinOfModifier(curMRP);
						}

						isAnyChanged = true;
					}
				}
			}
			
			//----------------------------------------------

			if (isAnyChanged)
			{
				Editor.Select.AutoSelectModMeshOrModBone();
				Editor.SetRepaint();//<<추가
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ] 일 때
				if(Editor.Select.ModRenderVerts_All == null)
				{
					return null;
				}
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}
			else
			{
				// [ 핀 편집 모드 ] 일 때
				if(Editor.Select.ModRenderPins_All == null)
				{
					return null;
				}
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
			}

		}


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Move]
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Modifier_VertexPos(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}

			//(Editing 상태일 때)
			//1. 선택된 Vertex가 있다면
			//2. 없다면 -> 패스

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				
				//|| Editor.Select.ExKey_ModMesh == null //이전
				|| Editor.Select.ModMesh_Main == null //변경 20.6.18
				
				|| Editor.Select.MeshGroup == null)
			{
				return;
			}

			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}


			//선택된 대상이 있는지 체크 (모드에 따라 : 22.4.11)
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVert_Main == null)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if (Editor.Select.ModRenderPin_Main == null)
				{
					return;
				}
			}
			

			//Undo			
			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(	(Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex ? 
														apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex : apUndoGroupData.ACTION.Modifier_Gizmo_MovePin), 
													Editor, Editor.Select.Modifier, 
													//null,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			// 편집 대상 (버텍스 or 핀)을 이동하자
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드일 때 ]
				if (Editor.Select.ModRenderVerts_All.Count == 1)
				{
					//1. 단일 선택일 때
					apRenderVertex renderVert = Editor.Select.ModRenderVert_Main._renderVert;
					
					//삭제
					//renderVert.Calculate();

					//변경 22.5.9 [v1.4.0 : 최적화로 단일 Calculate 함수를 호출할 수 없다.)
					renderVert._parentRenderUnit.CalculateTargetRenderVert(renderVert);



					Vector2 prevDeltaPos2 = Editor.Select.ModRenderVert_Main._modVert._deltaPos;

					//이전
					//apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos2, 0, Vector2.one);
					//Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld
					//							* renderVert._matrix_MeshTransform
					//							* renderVert._matrix_Rigging
					//							* martrixMorph
					//							* renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

					//변경 22.5.9 : 최적화
					Vector2 prevMorphPos = renderVert._vertex._pos;
					
					//Mesh의 Vert2Local 변환
					prevMorphPos.x += renderVert._parentMesh._matrix_VertToLocal._m02;
					prevMorphPos.y += renderVert._parentMesh._matrix_VertToLocal._m12;

					//Vert Local 연산값을 교체
					prevMorphPos += prevDeltaPos2;

					//Rigging > MeshTF > Vert World 변환 적용
					Vector2 prevWorldPos2 = renderVert._matrix_Rigging_MeshTF_VertWorld.MultiplyPoint(prevMorphPos);



					Vector2 nextWorldPos = new Vector2(prevWorldPos2.x, prevWorldPos2.y) + deltaMoveW;

					//NextWorld Pos에서 -> [VertWorld] -> [MeshTransform] -> Vert Local 적용 후의 좌표 -> Vert Local 적용 전의 좌표 
					//적용 전-후의 좌표 비교 = 그 차이값을 ModVert에 넣자
					//기존 계산 : Matrix를 구해서 일일이 계산한다.
					
					//이전
					//Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
					
					//변경 22.5.9
					Vector2 noneMorphedPosM = renderVert._vertex._pos;
					noneMorphedPosM.x += renderVert._parentMesh._matrix_VertToLocal._m02;
					noneMorphedPosM.y += renderVert._parentMesh._matrix_VertToLocal._m12;


					//기존
					//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);

					//변경 21.4.5 : 리깅 적용 (다중 모드 처리시 리깅이 추가될 수 있기 때문)
					//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld
					//								* renderVert._matrix_MeshTransform
					//								* renderVert._matrix_Rigging).inverse).MultiplyPoint(nextWorldPos);

					//변경 22.5.9 : 최적화 코드 적용
					Vector2 nextMorphedPosM = (renderVert._matrix_Rigging_MeshTF_VertWorld.inverse).MultiplyPoint(nextWorldPos);




					Editor.Select.ModRenderVert_Main._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
					Editor.Select.ModRenderVert_Main._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
				}
				else
				{
					//2. 복수개 선택일 때
					for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
					{
						apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

						apRenderVertex renderVert = modRenderVert._renderVert;

						Vector2 nextWorldPos = renderVert._pos_World + deltaMoveW;

						modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
						//apMatrix3x3 matToAfterVertLocal = (renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse;
						//Vector3 nextLocalMorphedPos = matToAfterVertLocal.MultiplyPoint3x4(new Vector3(nextWorldPos.x, nextWorldPos.y, 0));
						//Vector3 beforeLocalMorphedPos = (renderVert._matrix_Cal_VertLocal * renderVert._matrix_Static_Vert2Mesh).MultiplyPoint3x4(renderVert._vertex._pos);


						//modRenderVert._modVert._deltaPos.x += (nextLocalMorphedPos.x - beforeLocalMorphedPos.x);
						//modRenderVert._modVert._deltaPos.y += (nextLocalMorphedPos.y - beforeLocalMorphedPos.y);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
					{
						apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
						float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

						apRenderVertex renderVert = modRenderVert._renderVert;

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos = (renderVert._pos_World + deltaMoveW) * weight + (renderVert._pos_World) * (1.0f - weight);

						modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 일 때 ]
				int nMRPs = Editor.Select.ModRenderPins_All != null ? Editor.Select.ModRenderPins_All.Count : 0;
				apSelection.ModRenderPin curMRP = null;
				List<apSelection.ModRenderPin> selectedMRPS = Editor.Select.ModRenderPins_All;
				if (nMRPs > 0)
				{
					for (int iMRP = 0; iMRP < nMRPs; iMRP++)
					{
						curMRP = selectedMRPS[iMRP];

						//새로운 위치를 지정하자
						Vector2 nextWorldPos = curMRP._renderPin._pos_World + deltaMoveW;
						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode 
					&& Editor.Select.ModRenderPins_Weighted != null 
					&& Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					int nSoftMRPs = Editor.Select.ModRenderPins_Weighted.Count;
					List<apSelection.ModRenderPin> softMRPs = Editor.Select.ModRenderPins_Weighted;

					for (int iSoftMRP = 0; iSoftMRP < nSoftMRPs; iSoftMRP++)
					{
						curMRP = softMRPs[iSoftMRP];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos = (curMRP._renderPin._pos_World + deltaMoveW) * weight + (curMRP._renderPin._pos_World) * (1.0f - weight);
						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
			}
			

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();
			//Editor.RefreshControllerAndHierarchy();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Rotate]
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Modifier_VertexPos(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if (deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.ModMesh_Main == null
				)
			{
				return;
			}

			//선택된 대상이 있는지 체크 (모드에 따라 다르다)
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVerts_All == null)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPin_Main == null)
				{
					return;
				}
			}
			

			//삭제 22.4.27 : 1개 버텍스인 경우에도 Return하진 않는다. (Soft Selection 때문)
			//if (Editor.Select.ModRenderVerts_All.Count <= 1)
			//{
			//	return;
			//}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_RotateVertex, 
													Editor, Editor.Select.Modifier, 
													//null,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;
				//Vector3 centerPos3 = new Vector3(centerPos.x, centerPos.y, 0.0f);

				if (deltaAngleW > 180.0f)			{ deltaAngleW -= 360.0f; }
				else if (deltaAngleW < -180.0f)		{ deltaAngleW += 360.0f; }

				//Quaternion quat = Quaternion.Euler(0.0f, 0.0f, deltaAngleW);

				apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
												* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
												* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

				//선택된 RenderVert의 Mod 값을 바꾸자
				apSelection.ModRenderVert curMRV = null;

				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;

				if (nMRVs > 1)
				{
					for (int i = 0; i < nMRVs; i++)
					{
						curMRV = modRenderVerts[i];

						Vector2 nextWorldPos = matrix_Rotate.MultiplyPoint(curMRV._renderVert._pos_World);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}
				


				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
					{
						curMRV = Editor.Select.ModRenderVerts_Weighted[i];
						float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(curMRV._renderVert._pos_World);
						Vector2 nextWorldPos = (nextWorldPos2) * weight + (curMRV._renderVert._pos_World) * (1.0f - weight);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}

			}
			else
			{
				// [ 핀 편집 모드 ]
				Vector2 centerPos = Editor.Select.ModRenderPinsCenterPos;

				if (deltaAngleW > 180.0f)			{ deltaAngleW -= 360.0f; }
				else if (deltaAngleW < -180.0f)		{ deltaAngleW += 360.0f; }

				apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
												* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
												* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);

				//선택된 RenderVert의 Mod 값을 바꾸자
				apSelection.ModRenderPin curMRP = null;

				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

				if (nMRPs > 1)
				{
					for (int i = 0; i < nMRPs; i++)
					{
						curMRP = modRenderPins[i];

						Vector2 nextWorldPos = matrix_Rotate.MultiplyPoint(curMRP._renderPin._pos_World);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
				


				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRPs = weightedMRPs != null ? weightedMRPs.Count : 0;

					for (int i = 0; i < nWeightedMRPs; i++)
					{
						curMRP = weightedMRPs[i];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);
						
						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(curMRP._renderPin._pos_World);
						Vector2 nextWorldPos = (nextWorldPos2) * weight + (curMRP._renderPin._pos_World) * (1.0f - weight);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
			}

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Vertex의 위치값을 수정할 때 [Scale]
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Modifier_VertexPos(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null)
			{
				return;
			}

			if (deltaScaleW.sqrMagnitude == 0.0f && isFirstScale)
			{
				return;
			}

			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.ModMesh_Main == null)
			{
				return;
			}


			//선택된 대상이 있는지 체크 (모드에 따라 다르다)
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVerts_All == null)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPin_Main == null)
				{
					return;
				}
			}


			//삭제 22.4.27 : 1개 버텍스인 경우에도 Return하진 않는다. (Soft Selection 때문)
			//if (Editor.Select.ModRenderVerts_All.Count <= 1)
			//{
			//	return;
			//}

			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_ScaleVertex, 
													Editor, 
													Editor.Select.Modifier, 
													//null,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				Vector2 centerPos = Editor.Select.ModRenderVertsCenterPos;

				Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

				apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
					* apMatrix3x3.TRS(Vector2.zero, 0, scale)
					* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


				//선택된 RenderVert의 Mod 값을 바꾸자
				apSelection.ModRenderVert curMRV = null;
				
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;

				if (nMRVs > 1)
				{
					for (int i = 0; i < nMRVs; i++)
					{
						curMRV = modRenderVerts[i];

						Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(curMRV._renderVert._pos_World);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}
					
				

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
					{
						curMRV = Editor.Select.ModRenderVerts_Weighted[i];
						float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRV._renderVert._pos_World);
						Vector2 nextWorldPos = (nextWorldPos2) * weight + (curMRV._renderVert._pos_World) * (1.0f - weight);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}

			}
			else
			{
				// [ 핀 편집 모드 ]
				Vector2 centerPos = Editor.Select.ModRenderPinsCenterPos;

				Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

				apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos, 0, Vector2.one)
					* apMatrix3x3.TRS(Vector2.zero, 0, scale)
					* apMatrix3x3.TRS(-centerPos, 0, Vector2.one);


				//선택된 RenderPin의 Mod 값을 바꾸자
				apSelection.ModRenderPin curMRP = null;
				
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

				if (nMRPs > 1)
				{
					for (int i = 0; i < nMRPs; i++)
					{
						curMRP = modRenderPins[i];

						Vector2 nextWorldPos = matrix_Scale.MultiplyPoint(curMRP._renderPin._pos_World);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
					
				

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRPs = weightedMRPs != null ? weightedMRPs.Count : 0;

					for (int i = 0; i < nWeightedMRPs; i++)
					{
						curMRP = weightedMRPs[i];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRP._renderPin._pos_World);
						Vector2 nextWorldPos = (nextWorldPos2) * weight + (curMRP._renderPin._pos_World) * (1.0f - weight);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
			}

			Editor.Select.MeshGroup.RefreshForce();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Vertex의 위치값 [Position]
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Modifier_VertexPos(Vector2 pos)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.ExEditingMode == apSelection.EX_EDIT.None ||
				Editor.Select.ModMesh_Main == null ||
				Editor.Select.RenderUnitOfMod_Main == null)
			{
				//편집 가능한 상태가 아니면 패스
				return;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null)
			{
				return;
			}

			//편집 모드에 따라 편집 대상이 다르다.
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVert_Main == null)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if (Editor.Select.ModRenderPin_Main == null)
				{
					return;
				}
			}

			// Undo 저장
			
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_MoveVertex, 
												Editor, 
												Editor.Select.Modifier, 
												//null,
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			Vector2 deltaPosChanged = Vector2.zero;
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드일 때 ]
				
				//Depth는 신경쓰지 말자

				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;

				if (nMRVs > 0)
				{
					if (nMRVs == 1)
					{
						//수정 : 직접 대입한다.
						deltaPosChanged = pos - Editor.Select.ModRenderVert_Main._modVert._deltaPos;
						Editor.Select.ModRenderVert_Main._modVert._deltaPos = pos;
					}
					else if (nMRVs > 1)
					{
						//복수 선택시
						//AvgCenterDeltaPos의 변화값을 대입한다.

						Vector2 avgDeltaPos = Vector2.zero;

						for (int i = 0; i < nMRVs; i++)
						{
							avgDeltaPos += modRenderVerts[i]._modVert._deltaPos;
						}
						avgDeltaPos /= nMRVs;

						Vector2 deltaPos2Next = pos - avgDeltaPos;
						deltaPosChanged = deltaPos2Next;

						for (int i = 0; i < nMRVs; i++)
						{
							modRenderVerts[i]._modVert._deltaPos += deltaPos2Next;
						}
					}

					//Soft Selection 상태일때
					if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
					{
						apSelection.ModRenderVert curMRV = null;

						List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
						int nWeightedMRVs = weightedMRVs != null ? weightedMRVs.Count : 0;

						for (int i = 0; i < nWeightedMRVs; i++)
						{
							curMRV = weightedMRVs[i];
							float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

							//변경 : DeltaPos의 변경 값으로만 계산한다.
							curMRV._modVert._deltaPos = ((curMRV._modVert._deltaPos + deltaPosChanged) * weight) + (curMRV._modVert._deltaPos * (1.0f - weight));
						}
					}
				}
			}
			else
			{
				// [ 핀 편집 모드일 때 ]

				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

				if (nMRPs > 0)
				{
					if (nMRPs == 1)
					{
						//수정 : 직접 대입한다.
						deltaPosChanged = pos - Editor.Select.ModRenderPin_Main._modPin._deltaPos;
						Editor.Select.ModRenderPin_Main._modPin._deltaPos = pos;
					}
					else if (nMRPs > 1)
					{
						//복수 선택시 : AvgCenterDeltaPos의 변화값을 대입한다.

						Vector2 avgDeltaPos = Vector2.zero;

						for (int i = 0; i < nMRPs; i++)
						{
							avgDeltaPos += modRenderPins[i]._modPin._deltaPos;
						}
						avgDeltaPos /= nMRPs;

						Vector2 deltaPos2Next = pos - avgDeltaPos;
						deltaPosChanged = deltaPos2Next;

						for (int i = 0; i < nMRPs; i++)
						{
							modRenderPins[i]._modPin._deltaPos += deltaPos2Next;
						}
					}

					//Soft Selection 상태일때
					if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
					{
						apSelection.ModRenderPin curMRP = null;

						List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
						int nWeightedMRPs = weightedMRPs != null ? weightedMRPs.Count : 0;

						for (int i = 0; i < nWeightedMRPs; i++)
						{
							curMRP = weightedMRPs[i];
							float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

							//변경 : DeltaPos의 변경 값으로만 계산한다.
							curMRP._modPin._deltaPos = ((curMRP._modPin._deltaPos + deltaPosChanged) * weight) + (curMRP._modPin._deltaPos * (1.0f - weight));
						}
					}
				}
				
			}
			

			//강제로 업데이트할 객체 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();
		}



		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 색상값 [Color]
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Modifier_Vertex(Color color, bool isVisible)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.ModMesh_Main == null 
				|| Editor.Select.ModMesh_Gizmo_Main == null
				|| Editor.Select.ParamSetOfMod == null
				)
			{
				return;
			}

			//다중 처리 + 기즈모 메인 방식
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;

			if (nModMeshes == 0)
			{
				//선택된게 없다면
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_Color,
											Editor, Editor.Select.Modifier,
											//Editor.Select.ModMesh_Gizmo_Main,//<< [GizmoMain]
											false,
											apEditorUtil.UNDO_STRUCT.ValueOnly);//변경 20.6.18

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;


				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
					{ continue; }

					curModMesh._meshColor = color;
					curModMesh._isVisible = isVisible;
				}
			}

			//강제로 업데이트할 객체를 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}



		public void TransformChanged_Extra__Modifier_Vertex()
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.ModMesh_Main == null 
				|| Editor.Select.ParamSetOfMod == null
				|| Editor.Select.RenderUnitOfMod_Main == null
				)
			{
				return;
			}

			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			apModifierBase modifier = Editor.Select.Modifier;

			//변경 20.6.18
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			apRenderUnit renderUnit = Editor.Select.RenderUnitOfMod_Main;

			if(!modifier._isExtraPropertyEnabled)
			{
				return;
			}

			//Extra Option을 제어하는 Dialog를 호출하자
			
			//변경 21.10.2
			apDialog_ExtraOption.ShowDialog_Modifier(Editor, Editor._portrait, meshGroup, modifier, renderUnit, Editor.Select.RenderUnitOfMod_All, Editor.Select.ModMeshes_All);
		}

		public apGizmos.TransformParam PivotReturn__Modifier_Vertex()
		{

			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main== null)
			{
				return null;
			}



			if (Editor.Select.RenderUnitOfMod_Main == null)
			{
				return null;
			}



			//(Editng 상태가 아니면)
			// 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//|| Editor.Select.ExKey_ModMesh == null
				|| Editor.Select.ModMesh_Main == null//변경 20.6.18
				)
			{
				return null;
			}

			apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.None;
			if (Editor.Select.Modifier._isColorPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Color;//<Color를 지원하는 경우에만
			}
			if (Editor.Select.Modifier._isExtraPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Extra;//추가 : Extra 옵션
			}


			// 어떤 대상을 선택했는가
			bool isAnyTargetSelected = false;
			if(Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				isAnyTargetSelected = Editor.Select.ModRenderVert_Main != null && Editor.Select.ModRenderVerts_All != null;
			}
			else
			{
				// [ 핀 편집 모드 ]
				isAnyTargetSelected = Editor.Select.ModRenderPin_Main != null && Editor.Select.ModRenderPins_All != null;
			}

			//아무것도 선택하지 않았다면
			if(!isAnyTargetSelected)
			{
				//Color 옵션만이라도 설정하게 하자
				if (Editor.Select.Modifier._isColorPropertyEnabled
					|| Editor.Select.Modifier._isExtraPropertyEnabled)
				{
					return apGizmos.TransformParam.Make(apEditorUtil.InfVector2, 0.0f, Vector2.one, 0,
													Editor.Select.ModMesh_Main._meshColor,
													Editor.Select.ModMesh_Main._isVisible,
													apMatrix3x3.identity,
													false,
													paramType,
													Vector2.zero, 0.0f, Vector2.one
													);
				}
				return null;
			}


			//선택한 위치에 Gizmo를 띄우자
			//여러개의 Vert/Pin을 수정할 수 있도록 한다.
			paramType |= apGizmos.TRANSFORM_UI.Position2D;

			// 편집 모드에 따라 Gizmo의 대상이 다르다.
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				int nMRVs = Editor.Select.ModRenderVerts_All.Count;
				if (nMRVs > 1)
				{
					//< 여러개의 버텍스 >
					paramType |= apGizmos.TRANSFORM_UI.Vertex_Transform;

					Vector2 avgDeltaPos = Vector2.zero;
					
					for (int i = 0; i < nMRVs; i++)
					{
						avgDeltaPos += Editor.Select.ModRenderVerts_All[i]._modVert._deltaPos;
					}
					avgDeltaPos /= nMRVs;


					return apGizmos.TransformParam.Make(Editor.Select.ModRenderVertsCenterPos,
														0.0f,
														Vector2.one,
														Editor.Select.RenderUnitOfMod_Main.GetDepth(),
														Editor.Select.ModMesh_Main._meshColor,
														Editor.Select.ModMesh_Main._isVisible,

														apMatrix3x3.identity,
														true,
														paramType,
														avgDeltaPos,
														0.0f,
														Vector2.one
														);
				}
				else
				{
					//< 한개의 버텍스 >
					return apGizmos.TransformParam.Make(Editor.Select.ModRenderVert_Main._renderVert._pos_World,
														0.0f,
														Vector2.one,
														
														Editor.Select.RenderUnitOfMod_Main.GetDepth(),
														Editor.Select.ModMesh_Main._meshColor,
														Editor.Select.ModMesh_Main._isVisible,

														Editor.Select.ModRenderVert_Main._renderVert._matrix_ToWorld,
														false,
														paramType,
														Editor.Select.ModRenderVert_Main._modVert._deltaPos,
														0.0f,
														Vector2.one
														);
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				int nMRPs = Editor.Select.ModRenderPins_All.Count;
				if (nMRPs > 1)
				{
					//< 여러개의 핀 >
					paramType |= apGizmos.TRANSFORM_UI.Vertex_Transform;

					Vector2 avgDeltaPos = Vector2.zero;

					for (int i = 0; i < nMRPs; i++)
					{
						avgDeltaPos += Editor.Select.ModRenderPins_All[i]._modPin._deltaPos;
					}
					avgDeltaPos /= nMRPs;

					return apGizmos.TransformParam.Make(Editor.Select.ModRenderPinsCenterPos,
														0.0f,
														Vector2.one,
														Editor.Select.RenderUnitOfMod_Main.GetDepth(),
														Editor.Select.ModMesh_Main._meshColor,
														Editor.Select.ModMesh_Main._isVisible,

														apMatrix3x3.identity,
														true,
														paramType,
														avgDeltaPos,
														0.0f,
														Vector2.one
														);
				}
				else
				{
					//< 한개의 핀
					return apGizmos.TransformParam.Make(Editor.Select.ModRenderPin_Main._renderPin._pos_World,
														0.0f,
														Vector2.one,
														
														Editor.Select.RenderUnitOfMod_Main.GetDepth(),
														Editor.Select.ModMesh_Main._meshColor,
														Editor.Select.ModMesh_Main._isVisible,

														Editor.Select.ModRenderPin_Main._renderPin._matrix_ToWorld,
														false,
														paramType,
														Editor.Select.ModRenderPin_Main._modPin._deltaPos,
														0.0f,
														Vector2.one
														);
				}

				
			}

			
			

			
		}

		public bool FFDTransform__Modifier_VertexPos(List<object> srcObjects, List<Vector2> posData, apGizmos.FFD_ASSIGN_TYPE assignType, bool isResultAssign, bool isRecord)
		{
			if (!isResultAssign)
			{
				//결과 적용이 아닌 일반 수정 작업시
				//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.

				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					// [ 버텍스 편집 모드 ]
					if (Editor.Select.ModRenderVerts_All == null
						|| Editor.Select.ModRenderVerts_All.Count <= 1)
					{
						return false;
					}
				}
				else
				{
					// [ 핀 편집 모드 ]
					if (Editor.Select.ModRenderPins_All == null
						|| Editor.Select.ModRenderPins_All.Count <= 1)
					{
						return false;
					}
				}
				
			}

			//Undo
			if (isRecord)//Undo 요청이 있을 때만 저장
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_FFDVertex,
													Editor, Editor.Select.Modifier,
													//null,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				apSelection.ModRenderVert modRenderVert = null;
				if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
				{
					//World Pos를 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						modRenderVert = srcObjects[i] as apSelection.ModRenderVert;
						if (modRenderVert == null)
						{
							continue;
						}

						modRenderVert.SetWorldPosToModifier_VertLocal(posData[i]);
					}
				}
				else//if (assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
				{
					//저장된 데이터를 직접 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						modRenderVert = srcObjects[i] as apSelection.ModRenderVert;
						if (modRenderVert == null)
						{
							continue;
						}

						modRenderVert._modVert._deltaPos = posData[i];
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				apSelection.ModRenderPin modRenderPin = null;
				if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
				{
					//World Pos를 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						modRenderPin = srcObjects[i] as apSelection.ModRenderPin;
						if (modRenderPin == null)
						{
							continue;
						}

						modRenderPin.SetWorldToModifier_PinLocal(posData[i]);
					}
				}
				else//if (assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
				{
					//저장된 데이터를 직접 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						modRenderPin = srcObjects[i] as apSelection.ModRenderPin;
						if (modRenderPin == null)
						{
							continue;
						}

						modRenderPin._modPin._deltaPos = posData[i];
					}
				}
			}
			
			Editor.Select.MeshGroup.RefreshForce();
			return true;
		}

		public bool StartFFDTransform__Modifier_VertexPos()
		{
			if (Editor.Select.MeshGroup == null)
			{
				return false;
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVerts_All == null
					|| Editor.Select.ModRenderVerts_All.Count <= 1)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if (Editor.Select.ModRenderPins_All == null
					|| Editor.Select.ModRenderPins_All.Count <= 1)
				{
					return false;
				}
			}

			List<object> srcObjectList = new List<object>();
			List<Vector2> worldPosList = new List<Vector2>();
			List<Vector2> modDataList = new List<Vector2>();

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

				for (int i = 0; i < nMRVs; i++)
				{
					curMRV = modRenderVerts[i];
					srcObjectList.Add(curMRV);
					worldPosList.Add(curMRV._renderVert._pos_World);
					modDataList.Add(curMRV._modVert._deltaPos);//<<추가 20.7.22
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				for (int i = 0; i < nMRPs; i++)
				{
					curMRP = modRenderPins[i];
					srcObjectList.Add(curMRP);
					worldPosList.Add(curMRP._renderPin._pos_World);
					modDataList.Add(curMRP._modPin._deltaPos);//<<추가 20.7.22
				}
			}
			
			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList, modDataList);//<<True로 리턴할거면 이 함수를 호출해주자

			return true;
		}



		public bool SoftSelection__Modifier_VertexPos()
		{
			Editor.Select.ModRenderVerts_Weighted.Clear();
			Editor.Select.ModRenderPins_Weighted.Clear();

			if (Editor.Select.MeshGroup == null)
			{
				return false;
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVerts_All == null
					|| Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if (Editor.Select.ModRenderPins_All == null
					|| Editor.Select.ModRenderPins_All.Count == 0)
				{
					return false;
				}
			}
			

			float radius = (float)Editor.Gizmos.SoftSelectionRadius;
			if (radius <= 0.0f)
			{
				return false;
			}


			bool isConvex = Editor.Gizmos.SoftSelectionCurveRatio >= 0;
			float curveRatio = Mathf.Clamp01(Mathf.Abs((float)Editor.Gizmos.SoftSelectionCurveRatio / 100.0f));//0이면 직선, 1이면 커브(볼록/오목)

			//선택되지 않은 Vertex 중에서
			//"기본 위치 값"을 기준으로 영역을 선택해주자
			float minDist = 0.0f;
			float dist = 0.0f;


			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]

				apSelection.ModRenderVert minRV = null;


				//전체 MRV를 돌면서 "선택되지 않은 MRV"를 찾자
				List<apSelection.ModRenderVert> allMRVs = Editor.Select.ModData.ModRenderVert_All;
				List<apSelection.ModRenderVert> selectedMRVs = Editor.Select.ModRenderVerts_All;

				int nMRVs = allMRVs != null ? allMRVs.Count : 0;
				if (nMRVs > 0)
				{
					apSelection.ModRenderVert curMRV = null;
					for (int iMRV = 0; iMRV < nMRVs; iMRV++)
					{
						curMRV = allMRVs[iMRV];

						//선택된 RenderVert는 제외한다.
						if (selectedMRVs.Contains(curMRV))
						{
							continue;
						}

						//가장 가까운 선택된 RenderVert를 찾는다.
						minDist = 0.0f;
						dist = 0.0f;
						minRV = null;

						for (int iSelectedRV = 0; iSelectedRV < selectedMRVs.Count; iSelectedRV++)
						{
							apSelection.ModRenderVert selectedModRV = selectedMRVs[iSelectedRV];
							//현재 World위치로 선택해보자.

							dist = Vector2.Distance(selectedModRV._renderVert._pos_World, curMRV._renderVert._pos_World);
							if (dist < minDist || minRV == null)
							{
								minRV = selectedModRV;
								minDist = dist;
							}
						}

						if (minRV != null && minDist <= radius)
						{
							//radius에 들어가는 Vert 발견.
							//Weight는 CurveRatio에 맞게 (minDist가 0에 가까울수록 Weight는 1이 된다.)
							float itp_Linear = minDist / radius;
							float itp_Curve = 0.0f;
							if (isConvex)
							{
								//Weight가 더 1에 가까워진다. => minDist가 0이 되는 곳에 Control Point를 넣자
								itp_Curve = (1.0f * (itp_Linear * itp_Linear))
									+ (2.0f * 0.0f * itp_Linear * (1.0f - itp_Linear))
									+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
							}
							else
							{
								//Weight가 더 0에 가까워진다. => minDist가 radius가 되는 곳에 Control Point를 넣자
								itp_Curve = (1.0f * (itp_Linear * itp_Linear))
									+ (2.0f * 1.0f * itp_Linear * (1.0f - itp_Linear))
									+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
							}
							float itp = itp_Linear * (1.0f - curveRatio) + itp_Curve * curveRatio;
							float weight = 0.0f * itp + 1.0f * (1.0f - itp);

							//Weight를 추가로 넣어주고 리스트에 넣자
							curMRV._vertWeightByTool = weight;

							Editor.Select.ModRenderVerts_Weighted.Add(curMRV);
						}
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]

				apSelection.ModRenderPin minRP = null;

				//전체 MRP를 돌면서 "선택되지 않은 MRP"를 찾자
				List<apSelection.ModRenderPin> allMRPs = Editor.Select.ModData.ModRenderPin_All;
				List<apSelection.ModRenderPin> selectedMRPs = Editor.Select.ModRenderPins_All;


				int nMRPs = allMRPs != null ? allMRPs.Count : 0;
				int nSelectedMRPs = selectedMRPs != null ? selectedMRPs.Count : 0;

				if (nMRPs > 0)
				{
					apSelection.ModRenderPin curMRP = null;
					for (int iMRP = 0; iMRP < nMRPs; iMRP++)
					{
						curMRP = allMRPs[iMRP];

						//선택된 RenderVert는 제외한다.
						if (selectedMRPs.Contains(curMRP))
						{
							continue;
						}

						//가장 가까운 선택된 RenderPin을 찾는다.
						minDist = 0.0f;
						dist = 0.0f;
						minRP = null;

						for (int iSelectedRP = 0; iSelectedRP < nSelectedMRPs; iSelectedRP++)
						{
							apSelection.ModRenderPin selectedModRP = selectedMRPs[iSelectedRP];
							//현재 World위치로 선택해보자.

							dist = Vector2.Distance(selectedModRP._renderPin._pos_World, curMRP._renderPin._pos_World);
							if (dist < minDist || minRP == null)
							{
								minRP = selectedModRP;
								minDist = dist;
							}
						}

						if (minRP != null && minDist <= radius)
						{
							//radius에 들어가는 Vert 발견.
							//Weight는 CurveRatio에 맞게 (minDist가 0에 가까울수록 Weight는 1이 된다.)
							float itp_Linear = minDist / radius;
							float itp_Curve = 0.0f;
							if (isConvex)
							{
								//Weight가 더 1에 가까워진다. => minDist가 0이 되는 곳에 Control Point를 넣자
								itp_Curve = (1.0f * (itp_Linear * itp_Linear))
									+ (2.0f * 0.0f * itp_Linear * (1.0f - itp_Linear))
									+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
							}
							else
							{
								//Weight가 더 0에 가까워진다. => minDist가 radius가 되는 곳에 Control Point를 넣자
								itp_Curve = (1.0f * (itp_Linear * itp_Linear))
									+ (2.0f * 1.0f * itp_Linear * (1.0f - itp_Linear))
									+ (0.0f * (1.0f - itp_Linear) * (1.0f - itp_Linear));
							}
							float itp = itp_Linear * (1.0f - curveRatio) + itp_Curve * curveRatio;
							float weight = 0.0f * itp + 1.0f * (1.0f - itp);

							//Weight를 추가로 넣어주고 리스트에 넣자
							curMRP._pinWeightByTool = weight;

							Editor.Select.ModRenderPins_Weighted.Add(curMRP);
						}
					}
				}
			}

			return true;
		}

		private List<apModifiedVertex> _tmpBlurVertices = new List<apModifiedVertex>();
		private List<float> _tmpBlurWeights = new List<float>();

		private List<apModifiedPin> _tmpBlurPins = new List<apModifiedPin>();

		public apGizmos.BrushInfo SyncBlurStatus__Modifier_VertexPos(bool isEnded)
		{
			if (isEnded)
			{
				Editor._blurEnabled = false;
			}
			apGizmos.BRUSH_COLOR_MODE colorMode = apGizmos.BRUSH_COLOR_MODE.Default;
			if(Editor._blurIntensity == 0)		{ colorMode = apGizmos.BRUSH_COLOR_MODE.Default; }
			else if(Editor._blurIntensity < 33)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv1; }
			else if(Editor._blurIntensity < 66)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv2; }
			else								{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv3; }

			//이전
			//return apGizmos.BrushInfo.MakeInfo(Editor._blurRadius, Editor._blurIntensity, colorMode, null);

			//변경 22.1.9 : 인덱스로 조절
			return apGizmos.BrushInfo.MakeInfo(Editor._blurRadiusIndex, Editor._blurIntensity, colorMode, null);
		}

		public bool PressBlur__Modifier_VertexPos(Vector2 pos, float tDelta, bool isFirstBlur)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.ModMesh_Main == null
				|| Editor.Select.ModMesh_Main._transform_Mesh == null)
			{
				return false;
			}

			//모드에 따라서 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null
					|| Editor.Select.ModRenderVerts_All.Count <= 1)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null
					|| Editor.Select.ModRenderPins_All.Count <= 1)
				{
					return false;
				}
			}

			float radius = Editor.Gizmos.BrushRadiusGL;
			float intensity = Mathf.Clamp01((float)Editor.Gizmos.BrushIntensity / 100.0f);

			if (radius <= 0.0f || intensity <= 0.0f)
			{
				return false;
			}

			if(isFirstBlur)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_BlurVertex, 
												Editor, 
												Editor.Select.Modifier, 
												//null,
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			Vector2 totalModValue = Vector2.zero;
			float totalWeight = 0.0f;

			//영역 체크는 GL값
			float dist = 0.0f;
			float weight = 0.0f;

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				
				_tmpBlurVertices.Clear();
				_tmpBlurWeights.Clear();

				//1. 영역 안의 Vertex를 선택하자 + 마우스 중심점을 기준으로 Weight를 구하자 + ModValue의 가중치가 포함된 평균을 구하자
				//2. 가중치가 포함된 평균값만큼 tDelta * intensity * weight로 바꾸어주자

				//선택된 Vert에 한해서만 처리하자
				apSelection.ModRenderVert curMRV = null;
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;
				
				for (int i = 0; i < nMRVs; i++)
				{
					curMRV = modRenderVerts[i];
					dist = Vector2.Distance(curMRV._renderVert._pos_GL, pos);
					if (dist > radius)
					{
						continue;
					}

					weight = (radius - dist) / radius;
					totalModValue += curMRV._modVert._deltaPos * weight;
					totalWeight += weight;

					_tmpBlurVertices.Add(curMRV._modVert);
					_tmpBlurWeights.Add(weight);
				}

				if (_tmpBlurVertices.Count > 0 && totalWeight > 0.0f)
				{
					//Debug.Log("Blur : " + _tmpBlurVertices.Count + "s Verts / " + totalWeight);
					int nBlur = _tmpBlurVertices.Count;

					totalModValue /= totalWeight;

					apModifiedVertex curModVert = null;

					for (int i = 0; i < nBlur; i++)
					{
						//0이면 유지, 1이면 변경
						curModVert = _tmpBlurVertices[i];
						float itp = Mathf.Clamp01(_tmpBlurWeights[i] * tDelta * intensity * 5.0f);

						curModVert._deltaPos = curModVert._deltaPos * (1.0f - itp) + totalModValue * itp;
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]

				_tmpBlurPins.Clear();
				_tmpBlurWeights.Clear();

				//1. 영역 안의 Pin을 선택하자 + 마우스 중심점을 기준으로 Weight를 구하자 + ModValue의 가중치가 포함된 평균을 구하자
				//2. 가중치가 포함된 평균값만큼 tDelta * intensity * weight로 바꾸어주자

				//선택된 Vert에 한해서만 처리하자
				apSelection.ModRenderPin curMRP = null;
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;
				
				for (int i = 0; i < nMRPs; i++)
				{
					curMRP = modRenderPins[i];
					dist = Vector2.Distance(apGL.World2GL(curMRP._renderPin._pos_World), pos);
					if (dist > radius)
					{
						continue;
					}

					weight = (radius - dist) / radius;
					totalModValue += curMRP._modPin._deltaPos * weight;
					totalWeight += weight;

					_tmpBlurPins.Add(curMRP._modPin);
					_tmpBlurWeights.Add(weight);
				}

				if (_tmpBlurPins.Count > 0 && totalWeight > 0.0f)
				{
					int nBlur = _tmpBlurPins.Count;

					totalModValue /= totalWeight;

					apModifiedPin curModPin = null;

					for (int i = 0; i < nBlur; i++)
					{
						//0이면 유지, 1이면 변경
						curModPin = _tmpBlurPins[i];
						float itp = Mathf.Clamp01(_tmpBlurWeights[i] * tDelta * intensity * 5.0f);

						curModPin._deltaPos = curModPin._deltaPos * (1.0f - itp) + totalModValue * itp;
					}
				}
			}

			

			//강제로 업데이트할 객체 선택하고 Refresh
			//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExKey_ModMesh._renderUnit);
			Editor.Select.MeshGroup.RefreshForce();

			return true;
		}
	}

}