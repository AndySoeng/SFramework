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
//using UnityEngine.Profiling;

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


		//---------------------------------------------------------------------------
		//Animation의 현재 선택된 Timeline의 종류에 따라서 다른 이벤트 처리 방식을 가진다.
		// [타임라인이 없을때] - Transform 선택 (수정 불가)
		// [Modifier + Transform 계열] - Transform 선택
		// [Modifier + Vertex 계열] - Vertex / Transform(Lock 가능) 선택
		// [Bone] - Bone 선택
		// [ControlParam] - Transform 선택 (수정 불가)

		public apGizmos.GizmoEventSet GetEventSet__Animation_OnlySelectTransform()
		{
			//선택만 가능하다
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Animation,
														Unselect__Animation, 
														null, 
														null, 
														null, 
														PivotReturn__Animation_OnlySelect);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	null,
																null,
																null,
																null,
																null,
																null,
																apGizmos.TRANSFORM_UI.None);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(null, null, null, null, null, null);
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(FirstLink__Animation, null, null, null, null);

			return apGizmos.GizmoEventSet.I;

			//이전
			//return new apGizmos.GizmoEventSet(Select__Animation,
			//									Unselect__Animation,
			//									null, null, null, null, null, null, null, null, null,
			//									PivotReturn__Animation_OnlySelect,
			//									null, null, null, null, null, null,
			//									apGizmos.TRANSFORM_UI.None,
			//									FirstLink__Animation,
			//									null);
		}

		public apGizmos.GizmoEventSet GetEventSet__Animation_EditTransform()
		{
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Animation,
														Unselect__Animation, 
														Move__Animation_Transform, 
														Rotate__Animation_Transform, 
														Scale__Animation_Transform, 
														PivotReturn__Animation_Transform);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__Animation_Transform,
																TransformChanged_Rotate__Animation_Transform,
																TransformChanged_Scale__Animation_Transform,
																null,
																TransformChanged_Color__Animation_Transform,
																TransformChanged_Extra__Animation_Transform,
																apGizmos.TRANSFORM_UI.TRS_NoDepth //Depth 제외
																	| apGizmos.TRANSFORM_UI.Color 
																	| apGizmos.TRANSFORM_UI.Extra//<<추가
																	| apGizmos.TRANSFORM_UI.BoneIKController
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(null, null, null, null, null, null);
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Animation, 
																null, 
																OnHotKeyEvent__Animation_Transform__Keyboard_Move, 
																OnHotKeyEvent__Animation_Transform__Keyboard_Rotate, 
																OnHotKeyEvent__Animation_Transform__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;
		}

		public apGizmos.GizmoEventSet GetEventSet__Animation_EditVertex()
		{
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Animation,
														Unselect__Animation, 
														Move__Animation_Vertex, 
														Rotate__Animation_Vertex, 
														Scale__Animation_Vertex, 
														PivotReturn__Animation_Vertex);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__Animation_Vertex,
																TransformChanged_Rotate__Animation_Vertex,
																TransformChanged_Scale__Animation_Vertex,
																null,
																TransformChanged_Color__Animation_Transform,//Transform Color 이벤트를 사용한다.
																TransformChanged_Extra__Animation_Transform,
																apGizmos.TRANSFORM_UI.TRS_NoDepth
																	| apGizmos.TRANSFORM_UI.Color //<<Vertex Modifier도 Color를 선택할 수 있어야 한다.
																	| apGizmos.TRANSFORM_UI.Extra//<<추가
																	| apGizmos.TRANSFORM_UI.Vertex_Transform
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	MultipleSelect__Animation_Vertex, 
														FFDTransform__Animation_Vertex, 
														StartFFDTransform__Animation_Vertex, 
														SoftSelection_Animation_Vertex, 
														SyncBlurStatus_Animation_Vertex, 
														PressBlur_Animation_Vertex);
			
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Animation, 
																AddHotKeys__Animation_Vertex, 
																OnHotKeyEvent__Animation_Vertex__Keyboard_Move, 
																OnHotKeyEvent__Animation_Vertex__Keyboard_Rotate, 
																OnHotKeyEvent__Animation_Vertex__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;
			
			//이전
			//return new apGizmos.GizmoEventSet(Select__Animation,
			//									Unselect__Animation,
			//									Move__Animation_Vertex,
			//									Rotate__Animation_Vertex,
			//									Scale__Animation_Vertex,
			//									TransformChanged_Position__Animation_Vertex,
			//									TransformChanged_Rotate__Animation_Vertex,
			//									TransformChanged_Scale__Animation_Vertex,
			//									null,
			//									TransformChanged_Color__Animation_Transform,//<<null > Transform Color 이벤트를 사용한다.
			//									TransformChanged_Extra__Animation_Transform,
			//									PivotReturn__Animation_Vertex,
			//									MultipleSelect__Animation_Vertex,
			//									FFDTransform__Animation_Vertex,
			//									StartFFDTransform__Animation_Vertex,
			//									SoftSelection_Animation_Vertex,
			//									SyncBlurStatus_Animation_Vertex,
			//									PressBlur_Animation_Vertex,
			//									apGizmos.TRANSFORM_UI.TRS_NoDepth
			//									| apGizmos.TRANSFORM_UI.Color //<<Vertex Modifier도 Color를 선택할 수 있어야 한다.
			//									| apGizmos.TRANSFORM_UI.Extra//<<추가
			//									| apGizmos.TRANSFORM_UI.Vertex_Transform,
			//									FirstLink__Animation,
			//									AddHotKeys__Animation_Vertex);
		}


		public apGizmos.GizmoEventSet GetEventSet__Animation_EditColorOnly()
		{
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Animation,
														Unselect__Animation, 
														null, 
														null, 
														null, 
														PivotReturn__Animation_ColorOnly);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	null,
																null,
																null,
																null,
																TransformChanged_Color__Animation_Transform,
																TransformChanged_Extra__Animation_Transform,
																apGizmos.TRANSFORM_UI.Color 
																	| apGizmos.TRANSFORM_UI.Extra//<<추가
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(null, null, null, null, null, null);
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Animation, 
																null, 
																null, 
																null, 
																null);

			return apGizmos.GizmoEventSet.I;
		}


		//---------------------------------------------------------------------------
		//----------------------------------------------------------------------------------------------
		// Select + TRS 제어 이벤트들
		//----------------------------------------------------------------------------------------------

		public apGizmos.SelectResult FirstLink__Animation()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null || Editor.Select.AnimTimeline == null)
			{
				return null;
			}

			bool isTransformSelectable = false;
			bool isBoneSelectable = false;
			bool isVertexSelectable = false;
			//object resultObj = null;

			if (Editor.Select.AnimTimeline != null)
			{
				switch (Editor.Select.AnimTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						isTransformSelectable = true;
						if (Editor.Select.AnimTimeline._linkedModifier != null)
						{
							if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
							{
								//Vertex 데이터가 필요할 때
								isVertexSelectable = true;
							}
							if (Editor.Select.AnimTimeline._linkedModifier.IsTarget_Bone)
							{
								isBoneSelectable = true;//<<Bone 추가. 근데 이거 언제 써먹어요?
							}
						}
						break;

					case apAnimClip.LINK_TYPE.ControlParam:
						break;

					default:
						Debug.LogError("TODO : ???");
						break;
				}
			}
			else
			{
				isTransformSelectable = true;
				isBoneSelectable = true;
			}


			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			apModifiedMesh mainModMesh = Editor.Select.ModMesh_Main;
			apModifiedBone mainModBone = Editor.Select.ModBone_Main;


			if (isVertexSelectable)
			{
				if(workKeyframe != null && mainModMesh != null)
				{
					//편집 대상에 따라 다르다. (v1.4.0)
					if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
					{
						//[ 버텍스 편집 모드 ] 일 때
						if (Editor.Select.ModRenderVerts_All != null
							&& Editor.Select.ModRenderVerts_All.Count > 0)
						{
							return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);//변경 20.6.29
						}
					}
					else
					{
						// [ 핀 편집 모드 ]
						if (Editor.Select.ModRenderPins_All != null
							&& Editor.Select.ModRenderPins_All.Count > 0)
						{
							return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
						}
					}
				}
				
			}

			if (isTransformSelectable)
			{
				//기즈모 메인으로 변경
				//>> [GizmoMain] >>
				if(Editor.Select.MeshGroupTF_Mod_Gizmo != null)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshGroupTF_Mod_Gizmo);
				}
				if(Editor.Select.MeshTF_Mod_Gizmo != null)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshTF_Mod_Gizmo);
				}
				if (isBoneSelectable && Editor.Select.Bone_Mod_Gizmo != null)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone_Mod_Gizmo);
				}
			}

			//return result;
			//return apGizmos.SelectResult.Main.SetSingle(resultObj);
			return null;
		}

		// Select : 이건 통합
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [단일 선택] - 현재 Timeline에 따라 [Transform / Bone / Vertex]를 선택한다.
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Animation(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}



			//MeshTransform, MeshGroupTransform, Bone(TODO)을 선택해야한다.
			//어떤걸 선택할 수 있는지 제한을 걸자
			//음.. 코드를 분리할까
			//0) Timeline을 선택하지 않았다. / 또는 Editing이 아니다. => Transform/Bone을 선택할 수 있다.
			//1) ControlParam => 아무것도 선택할 수 없다.
			//2) AnimMod - Transform => Transform/Bone을 선택할 수 있다.
			//3) AnimMod - Vert => Vertex/Transform을 선택할 수 있다.

			//선택 순서 : (Transform 선택시) Vertex => Bone => Transform
			//0) Bone 선택 -> Transform 선택 -> 아니면 취소
			//1) 선택할 수 없다.
			//2) [Unlock] Bone -> Transform 순으로 선택 / [Lock] 둘다 선택 불가. 해제도 안된다.
			//3) [Unlock]
			//- Transform이 선택되어 있다면) Vertex를 선택한다.
			// -> Vertex 선택이 실패되었다면 -> Transform을 선택한다.
			// [Lock]
			//- Transform이 선택되어있다면 Vertex를 선택한다.

			//변경 20.6.29
			//다중 선택
			//애니메이션 레이어를 여러개 선택할 수 있다 > 해당 객체들이 여러개있다.
			
			apSelection.MULTI_SELECT multiSelect = (selectType == apGizmos.SELECT_TYPE.Add) ? apSelection.MULTI_SELECT.AddOrSubtract : apSelection.MULTI_SELECT.Main;

			bool isSelectionLock = Editor.Select.IsAnimSelectionLock;

			bool isVertexSelectable = false;
			bool isTransformSelectable = false;
			bool isBoneSelectable = false;

			apAnimTimeline timeline = Editor.Select.AnimTimeline;
			//apAnimTimelineLayer timelineLayer_Main = Editor.Select.AnimTimelineLayer_Main;
			//List<apAnimTimelineLayer> timelineLayers_All = Editor.Select.AnimTimelineLayers_All;



			//List<apAnimTimelineLayer> timelineLayers
			bool isAnimEditing = Editor.Select.ExAnimEditingMode != apSelection.EX_EDIT.None;

			if (!isAnimEditing || timeline == null)
			{
				//0) Timeline을 선택하지 않았거나 Anim 작업중이 아니다.
				isTransformSelectable = true;
				isBoneSelectable = true;
			}
			else
			{
				//에디팅 상태일 때
				if (timeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
				{
					//1) ControlParam 타입
					//... 선택을 하지 않는다.
				}
				else if (timeline._linkedModifier != null)
				{
					if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
					{
						//버텍스 모핑이 되는 타임라인 > 메시와 버텍스 선택 가능
						isVertexSelectable = true;
						isTransformSelectable = true;
					}
					else if (Editor.Select.AnimTimeline._linkedModifier.IsTarget_Bone)
					{
						//트랜스폼 타임라인 > 메시와 본 선택
						isTransformSelectable = true;
						isBoneSelectable = true;
					}
					else
					{
						//본은 지원 안하는 타임라인 (?) > 메시만 선택
						isTransformSelectable = true;
					}
				}
			}
			

			List<object> prevSelectedObjs = new List<object>();

			//>> [GizmoMain] >>
			if (Editor.Select.MeshGroupTF_Mod_Gizmo != null)
			{
				prevSelectedObjs.Add(Editor.Select.MeshGroupTF_Mod_Gizmo);
			}
			else if (Editor.Select.MeshTF_Mod_Gizmo != null)
			{
				prevSelectedObjs.Add(Editor.Select.MeshTF_Mod_Gizmo);
			}
			else if (Editor.Select.Bone_Mod_Gizmo != null && isBoneSelectable)
			{
				prevSelectedObjs.Add(Editor.Select.Bone_Mod_Gizmo);
			}
			


			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				//return prevSelected;
				return apGizmos.SelectResult.Main.SetMultiple(prevSelectedObjs);
			}


			bool isVertexSelected = false;
			bool isTransformSelected = false;
			bool isBoneSelected = false;
			
			_prevSelected_TransformBone = null;

			//apModifierBase linkedModifier = null;
			List<apAnimKeyframe> workKeyframes_All = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes_All != null) ? workKeyframes_All.Count : 0;

			
			
			//기존에 선택된 객체들
			int nBones_Prev = Editor.Select.SubObjects.NumBone;
			int nMeshTF_Prev = Editor.Select.SubObjects.NumMeshTF;
			//int nMeshGroupTF_Prev = Editor.Select.SubObjects.NumMeshGroupTF;
			
			
			//선택 우선순위
			//Vertex -> Bone -> Transform

			//추가 20.5.27 : 선택 작업이 끝났다.
			bool isSelectionCompleted = false;

			if (isVertexSelectable)
			{
				//일단 Vertex를 선택한다.
				//Pin 가능

				//편집 대상에 따라 다르다. (v1.4.0)
				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					//[ 버텍스 편집 모드 ] 일 때
					if (//Editor.Select.ModRenderVerts_All != null && //이거 삭제 : 아래서 체크할거임
						nWorkKeyframes > 0)
					{
						apSelection.ModRenderVert selectedMRV_Main = Editor.Select.ModRenderVert_Main;
						List<apSelection.ModRenderVert> selectedMRVs_All = Editor.Select.ModRenderVerts_All;
						int nSelectedMRVs = selectedMRVs_All != null ? selectedMRVs_All.Count : 0;

						//1) 현재 선택한 Vertex가 클릭 가능한지 체크

						if (selectedMRV_Main != null && selectedMRVs_All != null && nSelectedMRVs > 0)
						{
							apSelection.ModRenderVert clickedMRV_Direct = null;
							apSelection.ModRenderVert clickedMRV_Wide = null;
							float minWideClickDist = 0.0f;
							float curWideClickDist = 0.0f;
							apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

							if (nSelectedMRVs == 1)
							{
								//1개인 경우
								Vector2 rVertPosGL = apGL.World2GL(selectedMRV_Main._renderVert._pos_World);
								clickResult = Editor.Controller.IsVertexClickable(ref rVertPosGL, ref mousePosGL, ref curWideClickDist);

								if (clickResult != apEditorController.VERTEX_CLICK_RESULT.None)
								{
									//직간접적으로 클릭을 했다면..
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										Editor.Select.RemoveModRenderVert_ForAnimEdit(selectedMRV_Main);
									}

									isVertexSelected = true;
									isSelectionCompleted = true;

									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
								}
							}
							else
							{
								//여러개인 경우
								//-> 그중 하나를 선택 (+/-/New)
								apSelection.ModRenderVert curMRV = null;
								for (int iModRV = 0; iModRV < nSelectedMRVs; iModRV++)
								{
									curMRV = selectedMRVs_All[iModRV];

									Vector2 rVertPosGL = apGL.World2GL(curMRV._renderVert._pos_World);
									clickResult = Editor.Controller.IsVertexClickable(ref rVertPosGL, ref mousePosGL, ref curWideClickDist);

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
									{
										continue;
									}

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
									{
										//정확하게 클릭
										clickedMRV_Direct = curMRV;
										break;
									}

									//근처에서 클릭 : 거리 비교
									if (clickedMRV_Wide == null || curWideClickDist < minWideClickDist)
									{
										clickedMRV_Wide = curMRV;
										minWideClickDist = curWideClickDist;
									}
								}

								apSelection.ModRenderVert clickedMRV = null;

								if (clickedMRV_Direct != null)		{ clickedMRV = clickedMRV_Direct; }
								else if (clickedMRV_Wide != null)	{ clickedMRV = clickedMRV_Wide; }


								if (clickedMRV != null)
								{
									//클릭한게 있다.
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우 > 하나 지우자
										Editor.Select.RemoveModRenderVert_ForAnimEdit(clickedMRV);
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//추가인 경우 > ? 원래 있는걸 추가한다구요? 패스
									}
									else//if(selectType == apGizmos.SELECT_TYPE.New)
									{
										//새로 선택(New)인 경우 > 다른건 초기화. 이것만 선택한다.
										Editor.Select.SelectModRenderVert_ForAnimEdit(clickedMRV);
									}

									//어쨌거나 선택을 했으면 리턴
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
								}
							}
						}


						//2) 아직 선택했던걸 다시 클릭하진 않았다.
						//새롭게 클릭해서 추가 하는게 있는지 확인하자 (삭제는 예외)
						if (selectType == apGizmos.SELECT_TYPE.New)
						{
							//일단,  New 타입인데 클릭을 했으면 기존에 선택한건 날리자
							Editor.Select.SelectModRenderVert_ForAnimEdit(null);//변경 20.6.29
						}



						if (selectType != apGizmos.SELECT_TYPE.Subtract)
						{
							//변경 사항 20.6.29 : 기존에는 ModMesh의 Vertex를 찾아서 생성을 선택 > MRV로 생성했는데,
							//이제는 아예 MRV가 생성되어있는 상태이다.
							//이 부분이 가장 크게 바뀌었다.
							List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModData.ModRenderVert_All;
							int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;

							if (nMRVs > 0)
							{
								apSelection.ModRenderVert curMRV = null;

								//변경 22.4.9 : 버텍스 클릭 판정 개선 (Direct+Wide 두번)
								apSelection.ModRenderVert clickedMRV_Direct = null;
								apSelection.ModRenderVert clickedMRV_Wide = null;
								float curWideDist = 0.0f;
								float minWideDist = 0.0f;
								apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

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
										//정확히 클릭했다.
										clickedMRV_Direct = curMRV;
										break;
									}

									//거리를 체크한다.
									if (clickedMRV_Wide == null || curWideDist < minWideDist)
									{
										clickedMRV_Wide = curMRV;
										minWideDist = curWideDist;
									}
								}

								apSelection.ModRenderVert resultClickedMRV = null;

								if (clickedMRV_Direct != null)		{ resultClickedMRV = clickedMRV_Direct; }
								else if (clickedMRV_Wide != null)	{ resultClickedMRV = clickedMRV_Wide; }

								//클릭한게 있다.
								if (resultClickedMRV != null)
								{
									if (selectType == apGizmos.SELECT_TYPE.New)
									{
										Editor.Select.SelectModRenderVert_ForAnimEdit(resultClickedMRV);
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										Editor.Select.AddModRenderVert_ForAnimEdit(resultClickedMRV);
									}

									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
								}
							}
						}
					}
				}
				else
				{
					//[ 핀 편집 모드 ]

					if (nWorkKeyframes > 0)
					{
						apSelection.ModRenderPin selectedMRP_Main = Editor.Select.ModRenderPin_Main;
						List<apSelection.ModRenderPin> selectedMRPs_All = Editor.Select.ModRenderPins_All;
						int nSelectedMRPs = selectedMRPs_All != null ? selectedMRPs_All.Count : 0;

						//1) 현재 선택한 Pin이 클릭 가능한지 체크

						if (selectedMRP_Main != null && selectedMRPs_All != null && nSelectedMRPs > 0)
						{
							apSelection.ModRenderPin clickedMRP_Direct = null;
							apSelection.ModRenderPin clickedMRP_Wide = null;
							float minWideClickDist = 0.0f;
							float curWideClickDist = 0.0f;
							apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

							if (nSelectedMRPs == 1)
							{
								//1개인 경우
								Vector2 rPinPosGL = apGL.World2GL(selectedMRP_Main._renderPin._pos_World);
								clickResult = Editor.Controller.IsPinClickable(ref rPinPosGL, ref mousePosGL, ref curWideClickDist);

								if (clickResult != apEditorController.VERTEX_CLICK_RESULT.None)
								{
									//직간접적으로 클릭을 했다면..
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										Editor.Select.RemoveModRenderPin_ForAnimEdit(selectedMRP_Main);
									}

									isVertexSelected = true;
									isSelectionCompleted = true;

									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
								}
							}
							else
							{
								//여러개인 경우
								//-> 그중 하나를 선택 (+/-/New)
								apSelection.ModRenderPin curMRP = null;
								for (int iModRP = 0; iModRP < nSelectedMRPs; iModRP++)
								{
									curMRP = selectedMRPs_All[iModRP];

									Vector2 rPinPosGL = apGL.World2GL(curMRP._renderPin._pos_World);
									clickResult = Editor.Controller.IsPinClickable(ref rPinPosGL, ref mousePosGL, ref curWideClickDist);

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
									{
										continue;
									}

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
									{
										//정확하게 클릭
										clickedMRP_Direct = curMRP;
										break;
									}

									//근처에서 클릭 : 거리 비교
									if (clickedMRP_Wide == null || curWideClickDist < minWideClickDist)
									{
										clickedMRP_Wide = curMRP;
										minWideClickDist = curWideClickDist;
									}
								}

								apSelection.ModRenderPin clickedMRP = null;

								if (clickedMRP_Direct != null)		{ clickedMRP = clickedMRP_Direct; }
								else if (clickedMRP_Wide != null)	{ clickedMRP = clickedMRP_Wide; }


								if (clickedMRP != null)
								{
									//클릭한게 있다.
									if (selectType == apGizmos.SELECT_TYPE.Subtract)
									{
										//삭제인 경우 > 하나 지우자
										Editor.Select.RemoveModRenderPin_ForAnimEdit(clickedMRP);
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										//추가인 경우 > ? 원래 있는걸 추가한다구요? 패스
									}
									else//if(selectType == apGizmos.SELECT_TYPE.New)
									{
										//새로 선택(New)인 경우 > 다른건 초기화. 이것만 선택한다.
										Editor.Select.SelectModRenderPin_ForAnimEdit(clickedMRP);
									}

									//어쨌거나 선택을 했으면 리턴
									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
								}
							}
						}


						//2) 아직 선택했던걸 다시 클릭하진 않았다.
						//새롭게 클릭해서 추가 하는게 있는지 확인하자 (삭제는 예외)
						if (selectType == apGizmos.SELECT_TYPE.New)
						{
							//일단,  New 타입인데 클릭을 했으면 기존에 선택한건 날리자
							Editor.Select.SelectModRenderPin_ForAnimEdit(null);//변경 20.6.29
						}


						if (selectType != apGizmos.SELECT_TYPE.Subtract)
						{
							List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModData.ModRenderPin_All;
							int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

							if (nMRPs > 0)
							{
								apSelection.ModRenderPin curMRP = null;

								//변경 22.4.9 : 버텍스 클릭 판정 개선 (Direct+Wide 두번)
								apSelection.ModRenderPin clickedMRP_Direct = null;
								apSelection.ModRenderPin clickedMRP_Wide = null;
								float curWideDist = 0.0f;
								float minWideDist = 0.0f;
								apEditorController.VERTEX_CLICK_RESULT clickResult = apEditorController.VERTEX_CLICK_RESULT.None;

								for (int iMRP = 0; iMRP < nMRPs; iMRP++)
								{
									curMRP = modRenderPins[iMRP];

									Vector2 rPinPosGL = apGL.World2GL(curMRP._renderPin._pos_World);
									clickResult = Editor.Controller.IsPinClickable(ref rPinPosGL, ref mousePosGL, ref curWideDist);

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.None)
									{
										continue;
									}

									if (clickResult == apEditorController.VERTEX_CLICK_RESULT.DirectClick)
									{
										//정확히 클릭했다.
										clickedMRP_Direct = curMRP;
										break;
									}

									//거리를 체크한다.
									if (clickedMRP_Wide == null || curWideDist < minWideDist)
									{
										clickedMRP_Wide = curMRP;
										minWideDist = curWideDist;
									}
								}

								apSelection.ModRenderPin resultClickedMRP = null;

								if (clickedMRP_Direct != null)		{ resultClickedMRP = clickedMRP_Direct; }
								else if (clickedMRP_Wide != null)	{ resultClickedMRP = clickedMRP_Wide; }

								//클릭한게 있다.
								if (resultClickedMRP != null)
								{
									if (selectType == apGizmos.SELECT_TYPE.New)
									{
										Editor.Select.SelectModRenderPin_ForAnimEdit(resultClickedMRP);
									}
									else if (selectType == apGizmos.SELECT_TYPE.Add)
									{
										Editor.Select.AddModRenderPin_ForAnimEdit(resultClickedMRP);
									}

									return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
								}
							}
						}
					}
				}

				
			}

			if (isVertexSelected)
			{
				//Vertex를 선택했다면 본 선택은 자동 해제
				Editor.Select.SelectBone_ForAnimEdit(null, false, false, apSelection.MULTI_SELECT.Main);//자동 타임라인 레이어 선택은 생략
			}
			

			//추가 21.2.17 : 편집 중이 아닌 오브젝트를 선택하는 건 "편집 모드가 아니거나" / "선택 제한 옵션이 꺼진 경우"이다.
			bool isNotEditObjSelectable = Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || !Editor._exModObjOption_NotSelectable;

			//그 다음엔 Bone이다.
			//Vertex가 선택되지 않았다면
			if (!isSelectionLock)
			{
				if (isBoneSelectable && !isVertexSelected)
				{
					apMeshGroup mainMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
					
					if (selectType == apGizmos.SELECT_TYPE.New ||
						selectType == apGizmos.SELECT_TYPE.Add)
					{
						apBone bone = null;
						apBone resultBone = null;

						//>>Bone Set으로 변경
						apMeshGroup.BoneListSet boneSet = null;
						if (mainMeshGroup._boneListSets != null && mainMeshGroup._boneListSets.Count > 0)
						{
							//for (int iSet = 0; iSet < mainMeshGroup._boneListSets.Count; iSet++)//출력 순서
							for (int iSet = mainMeshGroup._boneListSets.Count - 1; iSet >= 0; iSet--)//선택 순서
							{
								boneSet = mainMeshGroup._boneListSets[iSet];

								if (boneSet._bones_Root != null && boneSet._bones_Root.Count > 0)
								{
									//for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)//출력 순서
									for (int iRoot = boneSet._bones_Root.Count - 1; iRoot >= 0; iRoot--)//선택 순서
									{
										bone = CheckBoneClickRecursive(boneSet._bones_Root[iRoot],
																		mousePosW, mousePosGL, Editor._boneGUIRenderMode,
																		//-1,
																		Editor.Select.IsBoneIKRenderable, isNotEditObjSelectable,
																		false);
										if (bone != null)
										{
											resultBone = bone;
											break;//다른 Root List는 확인하지 않는다.
										}
									}

								}

								if (resultBone != null)
								{
									//다른 Set에서 확인하지 않는다.
									break;
								}
							}
						}
						


						if (resultBone != null)
						{
							//본이 선택되었다.
							//변경 20.6.30 : 통합된 함수
							Editor.Select.SelectSubObject(null, null, resultBone, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);

							isBoneSelected = true;
							isSelectionCompleted = true;//본 선택 또는 선택 해제가 되었다.


							//v1.4.2 : GUI가 아닌 하단UI에서 레이어를 "클릭한 것"과 동일하게 판정하자 (레이어 다중 선택 위함)
							Editor.Select.SyncLastClickedTimelineLayerInfo(resultBone);


							//[1.4.2] 선택된 객체에 맞게 자동 스크롤
							if(Editor._option_AutoScrollWhenObjectSelected)
							{
								//스크롤 가능한 상황인지 체크하고
								if(Editor.IsAutoScrollableWhenClickObject_Animation(resultBone, true))
								{
									//자동 스크롤을 요청한다.
									Editor.AutoScroll_HierarchyAnimation(resultBone);
								}
							}
						}
					}
				}
			}

			if (!isSelectionLock)
			{
				if (isTransformSelectable && !isSelectionCompleted)//같은 내용임
				{
					apTransform_Mesh selectedMeshTransform = null;

					//GUI에서는 MeshTransform만 선택이 가능하다

					if (selectType == apGizmos.SELECT_TYPE.New ||
						selectType == apGizmos.SELECT_TYPE.Add)
					{
						//정렬된 RenderUnit
						//List<apRenderUnit> renderUnits = Editor.Select.AnimClip._targetMeshGroup._renderUnits_All;//<<이전 : RenderUnits_All
						List<apRenderUnit> renderUnits = Editor.Select.AnimClip._targetMeshGroup.SortedRenderUnits;//<<변경 : Sorted List

						if (renderUnits.Count > 0)
						{
							//기존
							//for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)
							//변경 20.5.27 : 체크 순서를 거꾸로 하여 맨 앞부터 체크한다.
							for (int iUnit = renderUnits.Count - 1; iUnit >= 0; iUnit--)
							{
								apRenderUnit renderUnit = renderUnits[iUnit];


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
										bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(
												mousePosGL, renderUnit);

										if (isPick)
										{
											selectedMeshTransform = renderUnit._meshTransform;
											//찾았어도 계속 찾는다. 뒤의 아이템이 "앞쪽"에 있는 것이기 때문

											//>> 변경 20.5.27 : 피킹 순서를 바꿔서 바로 break하면 된다.
											break;
										}
									}
								}
							}
						}

						if (selectedMeshTransform != null)
						{
							//걍 선택을 하자
							//변경 20.6.30 : 통합
							Editor.Select.SelectSubObject(selectedMeshTransform, null, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);
							
							isTransformSelected = true;
							isSelectionCompleted = true;

							//v1.4.2 : GUI가 아닌 하단UI에서 레이어를 "클릭한 것"과 동일하게 판정하자 (레이어 다중 선택 위함)
							Editor.Select.SyncLastClickedTimelineLayerInfo(selectedMeshTransform);
							
							//[1.4.2] 선택된 객체에 맞게 자동 스크롤
							if(Editor._option_AutoScrollWhenObjectSelected)
							{
								//스크롤 가능한 상황인지 체크하고
								if(Editor.IsAutoScrollableWhenClickObject_Animation(selectedMeshTransform, true))
								{
									//자동 스크롤을 요청한다.
									Editor.AutoScroll_HierarchyAnimation(selectedMeshTransform);
								}
							}
						}
					}
				}
			}

			//변경 20.6.3 : 통합
			//Deselect 로직이 따로따로 있으면 안된다.
			if(!isSelectionCompleted
				&& multiSelect == apSelection.MULTI_SELECT.Main
				&& (nMeshTF_Prev > 0 || nBones_Prev > 0)
				&& !isSelectionLock)
			{
				//선택이 하나도 안되었다면
				Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
			}


			Editor.RefreshControllerAndHierarchy(false);
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);//<<선택만 했으므로 큰 변경없이 Refresh
			Editor.SetRepaint();

			

			//변경 20.5.27 : 선택된 것을 List로 <다시> 정리해서 Multi로 넘기기
			prevSelectedObjs.Clear();
			
			//>> [GizmoMain] >>
			//이전 : Mod 오브젝트를 선택했다.
			//> 문제점 : 키프레임이 없는데서 오브젝트를 선택했다면 Mod 오브젝트가 없으므로 기즈모에는 "선택된게 없음"으로 기록되는데
			//> 이 상태에서 프레임을 옮기면 Mod 오브젝트가 자동으로 선택되는 반면 아직 여기서의 "선택된게 없음"에 의해서 기즈모가 동작하지 않는다.

			//[v1.4.3] 변경 : Mod 오브젝트가 아니라 선택된 객체들을 그냥 넣는다. (Mod TF의 Gizmo Controller가 그렇게 동작함)

			//if (Editor.Select.MeshGroupTF_Mod_Gizmo != null)
			//{
			//	prevSelectedObjs.Add(Editor.Select.MeshGroupTF_Mod_Gizmo);
			//}
			//else if (Editor.Select.MeshTF_Mod_Gizmo != null)
			//{
			//	prevSelectedObjs.Add(Editor.Select.MeshTF_Mod_Gizmo);
			//}
			//else if (Editor.Select.Bone_Mod_Gizmo != null && isBoneSelectable)
			//{
			//	prevSelectedObjs.Add(Editor.Select.Bone_Mod_Gizmo);
			//}

			List<apTransform_MeshGroup> selectedMeshGroupTFs = Editor.Select.GetSubSeletedMeshGroupTFs(false);
			int nSelectedMeshGroupTFs = selectedMeshGroupTFs != null ? selectedMeshGroupTFs.Count : 0;
			if(nSelectedMeshGroupTFs > 0)
			{
				for (int iMGTF = 0; iMGTF < nSelectedMeshGroupTFs; iMGTF++)
				{
					prevSelectedObjs.Add(selectedMeshGroupTFs[iMGTF]);
				}
			}

			List<apTransform_Mesh> selectedMeshTFs = Editor.Select.GetSubSeletedMeshTFs(false);
			int nSelectedMeshTFs = selectedMeshTFs != null ? selectedMeshTFs.Count : 0;
			if(nSelectedMeshTFs > 0)
			{
				for (int iMeshTF = 0; iMeshTF < nSelectedMeshTFs; iMeshTF++)
				{
					prevSelectedObjs.Add(selectedMeshTFs[iMeshTF]);
				}
			}

			if(isBoneSelectable)
			{
				List<apBone> selectedBones = Editor.Select.GetSubSeletedBones(false);
				int nSelectedBones = selectedBones != null ? selectedBones.Count : 0;
				if(nSelectedBones > 0)
				{
					for (int iBone = 0; iBone < nSelectedBones; iBone++)
					{
						prevSelectedObjs.Add(selectedBones[iBone]);
					}
				}
			}
			
			


			if ((isBoneSelected || isTransformSelected) && prevSelectedObjs.Count > 0)
			{	
				return apGizmos.SelectResult.Main.SetMultiple(prevSelectedObjs);
			}
			else
			{
				if (isVertexSelectable)
				{
					//편집 대상에 따라 다르다. (v1.4.0)
					if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
					{
						// [ 버텍스 편집 모드 ]
						//Vertex 선택 상태에서는 "Vertex가 선택된 여부"만 리턴한다.
						int nMRVs = Editor.Select.ModRenderVerts_All != null ? Editor.Select.ModRenderVerts_All.Count : 0;
						if (nMRVs == 0)
						{
							return null;
						}
						else if (nMRVs == 1)
						{
							return apGizmos.SelectResult.Main.SetSingle(Editor.Select.ModRenderVerts_All[0]);
						}
						else
						{
							return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
						}
					}
					else
					{
						// [ 핀 편집 모드 ]
						int nMRPs = Editor.Select.ModRenderPins_All != null ? Editor.Select.ModRenderPins_All.Count : 0;
						if (nMRPs == 0)
						{
							return null;
						}
						else if (nMRPs == 1)
						{
							return apGizmos.SelectResult.Main.SetSingle(Editor.Select.ModRenderPins_All[0]);
						}
						else
						{
							return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
						}
					}
					
				}
				else if(prevSelectedObjs.Count > 0)
				{	
					return apGizmos.SelectResult.Main.SetMultiple(prevSelectedObjs);
				}
			}

			return null;
		}



		public void Unselect__Animation()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			//Debug.Log("Unselect_Animation");
			if (Editor.Select.AnimTimeline == null)
			{
				//전부 해제
				Editor.Select.UnselectAllObjects_ForAnimEdit();
			}
			else if(Editor.Select.AnimTimeline._linkedModifier == null)
			{
				//추가 : 7.6 : 전부 해제
				Editor.Select.UnselectAllObjects_ForAnimEdit();
			}
			else
			{
				//Debug.Log("Unselect_Animation -> Timeline Exist");
				if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) != 0)
				{
					//Vertex 타입일 때

					//1. Vertex 먼저
					//2. Transform 다음 (락 안걸려있을 때)
					if(Editor.Gizmos.IsFFDMode)
					{
						//FFD 모드가 켜진 상태에서는 해제가 안된다.
						return;
					}

					Editor.Select.SelectModRenderVert_ForAnimEdit(null);//변경 20.6.29
					Editor.Select.SelectModRenderPin_ForAnimEdit(null);//추가 22.4.29
					
					Editor.Select.SelectBone_ForAnimEdit(null, false, false, apSelection.MULTI_SELECT.Main);//이거 쓰자

					if (!Editor.Select.IsAnimSelectionLock)
					{
						Editor.Select.SelectMeshTF_ForAnimEdit(null, false, false, apSelection.MULTI_SELECT.Main);
						Editor.Select.SelectMeshGroupTF_ForAnimEdit(null, false, false, apSelection.MULTI_SELECT.Main);
					}

					//타임라인 레이어 자동 선택
					Editor.Select.AutoSelectAnimTimelineLayer(false, false);
				}
				else
				{
					//Debug.Log("Transform / Bone Type");

					//Transform / Bone 타입일 때
					//전부 해제
					Editor.Select.UnselectAllObjects_ForAnimEdit();
				}
			}


			Editor.RefreshControllerAndHierarchy(false);
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);//선택 해제만 했으므로 간단한 갱신만
			Editor.SetRepaint();
		}

		// Move
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [이동] - (Autokey가 켜지고 / 임시 값이 열리면) 현재 Timeline에 따라 [Transform / Bone / Vertex]를 선택한다.
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Animation_Transform(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null
				//|| deltaMoveW.sqrMagnitude == 0.0f
				)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			if(deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}


			#region [미사용 코드] 단일 선택 및 처리
			//apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			//apAnimTimelineLayer animTimelineLayer = Editor.Select.AnimTimelineLayer_Main;
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//if(workKeyframe == null)
			//{
			//	//추가 : 5.29
			//	//키프레임이 없을 때 "AutoKey"가 켜져 있다면, 키프레임을 빠르게 만든다.
			//	//Bone IK에 의해 Keyframe이 생성된 것과 달리 이때는 Link를 해야한다. (파라미터 확인)
			//	if(Editor._isAnimAutoKey && animTimelineLayer != null)
			//	{
			//		apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, animTimelineLayer, true, false, false, true);
			//		if(autoCreatedKeyframe != null)
			//		{
			//			Editor.Select.AutoSelectAnimTimelineLayer(true);
			//			workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//		}
			//	}
			//}

			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			//apAnimClip targetAnimClip = Editor.Select.AnimClip;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	return;
			//}

			//bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			//bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//둘다 해당사항이 없다.
			//	return;
			//}


			////마우스가 안에 없다.
			//if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			//{
			//	return;
			//}

			////Undo
			//if (isFirstMove)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform, Editor, linkedModifier, targetRenderUnit, false);
			//}

			//if (isTargetTransform)
			//{
			//	apMatrix resultMatrix = null;
			//	//apMatrix resultMatrixWithoutMod = null;

			//	apMatrix matx_ToParent = null;
			//	//apMatrix matx_LocalModified = null;
			//	apMatrix matx_ParentWorld = null;

			//	object selectedTransform = null;

			//	if (modMesh._isMeshTransform)
			//	{
			//		if (modMesh._transform_Mesh != null)
			//		{
			//			resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;
			//			//resultMatrixWithoutMod = modMesh._transform_Mesh._matrix_TFResult_WorldWithoutMod;
			//			selectedTransform = modMesh._transform_Mesh;

			//			matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_Mesh._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		if (modMesh._transform_MeshGroup != null)
			//		{
			//			resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;
			//			//resultMatrixWithoutMod = modMesh._transform_MeshGroup._matrix_TFResult_WorldWithoutMod;
			//			selectedTransform = modMesh._transform_MeshGroup;

			//			matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_MeshGroup._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if (resultMatrix == null)
			//	{
			//		return;
			//	}

			//	if (selectedTransform != _prevSelected_TransformBone || isFirstMove)
			//	{
			//		_prevSelected_TransformBone = selectedTransform;
			//		_prevSelected_MousePosW = curMousePosW;
			//		//_prevSelected_WorldPos = resultMatrix._pos;
			//	}


			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetPos(resultMatrix._pos + deltaMoveW);

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	//TRS중에 Pos만 넣어도 된다.
			//	modMesh._transformMatrix.SetPos(nextLocalModifiedMatrix._pos);


			//	#region [미사용 코드 : CalLog를 이용한 이동 코드]
			//	//// 목표 WorldPos
			//	////Vector3 worldPos = _prevSelected_WorldPos + new Vector3(deltaMousePosWFromDown.x, deltaMousePosWFromDown.y, 0);
			//	//apCalculatedLog.InverseResult calResult = modMesh.CalculatedLog.World2ModLocalPos_TransformMove(curMousePosW, deltaMoveW);
			//	//if (calResult == null || !calResult._isSuccess)
			//	//{

			//	//	//Scale 없는 Matrix가 필요하다
			//	//	apMatrix noScaleWorldMatrix = new apMatrix();
			//	//	noScaleWorldMatrix.SetPos(resultMatrix._pos);
			//	//	noScaleWorldMatrix.SetRotate(resultMatrix._angleDeg);
			//	//	noScaleWorldMatrix.MakeMatrix();

			//	//	apMatrix noScaleWorldMatrixWithoutMod = new apMatrix();
			//	//	noScaleWorldMatrixWithoutMod.SetPos(resultMatrixWithoutMod._pos);
			//	//	noScaleWorldMatrixWithoutMod.SetRotate(resultMatrixWithoutMod._angleDeg);
			//	//	noScaleWorldMatrixWithoutMod.MakeMatrix();


			//	//	Vector2 worldPosPrev = resultMatrix._pos;
			//	//	Vector2 worldPosNext = worldPosPrev + deltaMoveW;

			//	//	//변경된 위치 W (Pos + Delta) => 변경된 위치 L (Pos + Delta)
			//	//	Vector2 localPosDelta = noScaleWorldMatrixWithoutMod.InvMulPoint2(worldPosNext);

			//	//	//Vector2 deltaModLocalPos = new Vector2(localPosDelta.x, localPosDelta.y);

			//	//	modMesh._transformMatrix.SetPos(localPosDelta);
			//	//	modMesh._transformMatrix.MakeMatrix();
			//	//}
			//	//else
			//	//{
			//	//	modMesh._transformMatrix.SetPos(calResult._posL_next);
			//	//	modMesh._transformMatrix.MakeMatrix();
			//	//} 
			//	#endregion

			//	modMesh.RefreshValues_Check(Editor._portrait);

			//	//이건 너무 많이 Refresh한다.
			//	//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);//<이거 필수. 그래야 다음 프레임 전에 바로 적용이 되서 문제가 없음

			//	//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//	if (targetMeshGroup != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//}
			//else
			//{
			//	//Bone 움직임을 제어하자.
			//	//ModBone에 값을 넣는다.
			//	apBone bone = Editor.Select.Bone;
			//	//apMeshGroup meshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//	//Move로 제어 가능한 경우는
			//	//1. IK Tail일 때
			//	//2. Root Bone일때 (절대값)

			//	bool isAnimKeyAutoAdded = false;//편집 도중에 키프레임이 생성된 경우 -> Refresh를 해줘야 한다.





			//	if (bone._isIKTail)
			//	{
			//		//Debug.Log("Request IK : " + _boneSelectPosW);
			//		float weight = 1.0f;
			//		//bool successIK = bone.RequestIK(_boneSelectPosW, weight, !isFirstSelectBone);

			//		if (bone != _prevSelected_TransformBone || isFirstMove)
			//		{
			//			_prevSelected_TransformBone = bone;
			//			_prevSelected_MousePosW = bone._worldMatrix._pos;
			//		}

			//		_prevSelected_MousePosW += deltaMoveW;
			//		Vector2 bonePosW = _prevSelected_MousePosW;//DeltaPos + 절대 위치 절충
			//		//Vector2 bonePosW = bone._worldMatrix._pos + deltaMoveW;//DeltaPos 이용
			//		//Vector2 bonePosW = curMousePosW;//절대 위치 이용 << IK는 절대 위치를 이용하자..
			//		apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

			//		if (limitedHeadBone == null)
			//		{
			//			return;
			//		}


			//		//apBone headBone = bone._IKHeaderBone;//<<
			//		apBone curBone = bone;
			//		//위로 올라가면서 IK 결과값을 Default에 적용하자

			//		//중요
			//		//만약, IK Chain이 된 상태에서
			//		//Chain에 해당하는 Bone이 현재 Keyframe이 없는 경우 (Layer는 있으나 현재 프레임에 해당하는 Keyframe이 없음)
			//		//자동으로 생성해줘야 한다.

			//		int curFrame = Editor.Select.AnimClip.CurFrame;

			//		while (true)
			//		{
			//			float deltaAngle = curBone._IKRequestAngleResult;

			//			//apModifiedBone targetModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
			//			//{
			//			//	return a._bone == curBone;
			//			//});
			//			apAnimTimelineLayer targetLayer = animTimeline._layers.Find(delegate (apAnimTimelineLayer a)
			//			{
			//				return a._linkedBone == curBone;
			//			});
			//			//현재 Bone과 ModBone이 있는가

			//			apAnimKeyframe targetKeyframe = targetLayer.GetKeyframeByFrameIndex(curFrame);

			//			if (targetKeyframe == null)
			//			{
			//				//해당 키프레임이 없다면
			//				//새로 생성한다.
			//				//단, Refresh는 하지 않는다.

			//				targetKeyframe = Editor.Controller.AddAnimKeyframe(curFrame, targetLayer, false, false, false, false);
			//				isAnimKeyAutoAdded = true;//True로 설정하고 나중에 한꺼번에 Refresh하자
			//			}

			//			apModifiedBone targetModBone = targetKeyframe._linkedModBone_Editor;

			//			if (targetModBone != null)
			//			{
			//				//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
			//				//해당 Bone의 ModBone을 가져온다.

			//				//IK로 ModBone의 각도를 설정하는 경우, 각도는 무조건 180 범위 안에 들어와야 한다.
			//				float nextAngle = apUtil.AngleTo180(curBone._localMatrix._angleDeg + deltaAngle);

			//				//while (nextAngle < -180.0f)
			//				//{
			//				//	nextAngle += 360.0f;
			//				//}
			//				//while (nextAngle > 180)
			//				//{
			//				//	nextAngle -= 360.0f;
			//				//}

			//				//DefaultMatrix말고
			//				//curBone._defaultMatrix.SetRotate(nextAngle);

			//				//ModBone을 수정하자
			//				targetModBone._transformMatrix.SetRotate(nextAngle);
			//			}

			//			curBone._isIKCalculated = false;
			//			curBone._IKRequestAngleResult = 0.0f;

			//			if (curBone == limitedHeadBone)
			//			{
			//				break;
			//			}
			//			if (curBone._parentBone == null)
			//			{
			//				break;
			//			}
			//			curBone = curBone._parentBone;
			//		}

			//		//마지막으론 World Matrix 갱신
			//		//근데 이게 딱히 소용 없을 듯..
			//		//Calculated Refresh를 해야함
			//		//limitedHeadBone.MakeWorldMatrix(true);
			//		//limitedHeadBone.GUIUpdate(true);
			//	}
			//	else if (bone._parentBone == null
			//		|| (bone._parentBone._IKNextChainedBone != bone))
			//	{
			//		//수정 : Parent가 있지만 IK로 연결 안된 경우 / Parent가 없는 경우 2가지 모두 처리한다.

			//		apMatrix parentMatrix = null;

			//		if (bone._parentBone == null)
			//		{
			//			if (bone._renderUnit != null)
			//			{
			//				//Render Unit의 World Matrix를 참조하여
			//				//로컬 값을 Default로 적용하자
			//				parentMatrix = bone._renderUnit.WorldMatrixWrap;
			//			}
			//		}
			//		else
			//		{
			//			parentMatrix = bone._parentBone._worldMatrix;
			//		}


			//		//apMatrix localMatrix = bone._localMatrix;
			//		apMatrix newWorldMatrix = new apMatrix(bone._worldMatrix);
			//		newWorldMatrix.SetPos(newWorldMatrix._pos + deltaMoveW);
			//		//newWorldMatrix.SetPos(curMousePosW);

			//		if (parentMatrix != null)
			//		{
			//			newWorldMatrix.RInverse(parentMatrix);
			//		}
			//		//WorldMatrix에서 Local Space에 위치한 Matrix는
			//		//Default + Local + RigTest이다.

			//		newWorldMatrix.Subtract(bone._defaultMatrix);
			//		if (bone._isRigTestPosing)
			//		{
			//			newWorldMatrix.Subtract(bone._rigTestMatrix);
			//		}

			//		modBone._transformMatrix.SetPos(newWorldMatrix._pos);
			//	}


			//	if (targetMeshGroup != null)
			//	{
			//		//if(modBone._renderUnit != null)
			//		//{
			//		//	targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
			//		//}

			//		targetMeshGroup.RefreshForce();
			//	}

			//	if (isAnimKeyAutoAdded)
			//	{
			//		//Key가 추가되었다면 Refresh를 해야한다.
			//		Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));
			//		//Editor.RefreshControllerAndHierarchy();

			//		//Refresh 추가
			//		//Editor.Select.RefreshAnimEditing(true);

			//		//이전
			//		//Editor.RefreshTimelineLayers(false);

			//		//변경 19.5.21 : 모든 정보를 리셋할 필욘 없다. > 근데 키프레임이 몇개가 추가되었는지는 몰겄다;;
			//		Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, null, null);

			//		Editor.Select.AutoSelectAnimTimelineLayer(true);
			//		Editor.Select.SetBone(bone, apSelection.MULTI_SELECT.Main);
			//	}
			//} 
			#endregion

			//변경 20.6.30 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			//마우스가 안에 없다.
			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			//Undo : 위치가 AutoKey보다 앞쪽에 있어야 한다.
			if (isFirstMove)
			{
				if (Editor._isAnimAutoKey)
				{
					//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform, 
																		Editor, 
																		Editor._portrait,
																		targetMeshGroup,
																		linkedModifier, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					//버그 수정 20.7.19
					//AutoKey가 꺼져 있어도, Bone을 선택한 상태에서 IK를 활용해서 움직일 경우
					//- 선택된 Gizmo 본 리스트의 개수가 0개 이상이고
					//- 선택된 Gizmo 본 리스트 중에 부모가 하나라도 있다.
					
					bool isAnyUnselectedParentBones = false;

					int nGizmoBones = (Editor.Select.ModBones_Gizmo_All != null) ? Editor.Select.ModBones_Gizmo_All.Count : 0;

					if(nGizmoBones > 0)
					{
						apModifiedBone curModBoneToCheck = null;
						for (int iGB = 0; iGB < nGizmoBones; iGB++)
						{
							curModBoneToCheck = Editor.Select.ModBones_Gizmo_All[iGB];
							if(curModBoneToCheck._bone != null && 
								curModBoneToCheck._bone._parentBone != null)
							{
								//선택되지 않은 부모 본이 편집 될 수 있다.
								isAnyUnselectedParentBones = true;
								break;
							}
						}
					}

					if (isAnyUnselectedParentBones)
					{
						//부모 본에서 키프레임이 자동으로 생성될 가능성이 있다.
						apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform,
																		Editor,
																		Editor._portrait,
																		targetMeshGroup,
																		linkedModifier,
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);

						//Debug.LogError("부모 본이 있어서 Undo Record의 범위 확장");
					}
					else
					{
						//일반적인 편집
						apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform,
															Editor, 
															linkedModifier, 
															//null, 
															false,
															apEditorUtil.UNDO_STRUCT.ValueOnly);
						//Debug.LogWarning("일반 편집에 의한 Undo Record");
					}
					

					
				}
				
			}



			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(isFirstMove && Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}

			bool isMain = false;

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//삭제 20.11.2
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;
				object selectedTransform = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if(curModMesh == null)
					{
						continue;
					}

					//isMain = (curModMesh == Editor.Select.ModMesh_Main);
					isMain = (curModMesh == Editor.Select.ModMesh_Gizmo_Main);//<< [GizmoMain]

					//resultMatrix = null;//삭제 20.11.2
					matx_ToParent = null;
					matx_ParentWorld = null;
					selectedTransform = null;

					if(curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;

						selectedTransform = curModMesh._transform_Mesh;
					}
					else if(!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;

						selectedTransform = curModMesh._transform_MeshGroup;
					}
					
					//삭제 20.11.2
					//if(resultMatrix == null)
					//{
					//	continue;
					//}

					//TODO : 이 코드는 확인 필요
					if (isMain)
					{
						if (selectedTransform != _prevSelected_TransformBone || isFirstMove)
						{
							_prevSelected_TransformBone = selectedTransform;
							_prevSelected_MousePosW = curMousePosW;
						}
					}

					#region [미사용 코드] 이전 방식
					//if(_tmpNextWorldMatrix == null)
					//{
					//	_tmpNextWorldMatrix = new apMatrix(resultMatrix);
					//}
					//else
					//{
					//	_tmpNextWorldMatrix.SetMatrix(resultMatrix, false);
					//}
					//_tmpNextWorldMatrix.SetPos(resultMatrix._pos + deltaMoveW, false);
					//_tmpNextWorldMatrix.MakeMatrix();

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					////TRS중에 Pos만 넣어도 된다.
					//curModMesh._transformMatrix.SetPos(nextLocalModifiedMatrix._pos, true);

					//curModMesh.RefreshValues_Check(Editor._portrait); 
					#endregion


					//변경 20.10.31 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}

					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, false);

					//직접 만든 WorldMatrix를 이용해서 위치를 이동
					_tmpNextWorldMatrix._pos += deltaMoveW;

					//Mod Matrix로 이동
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, false);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, false);

					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, true);//버그 해결!
				}
			}

			bool isKeyframeAdded_ByIK = false;

			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null)
					{ continue; }

					bone = curModBone._bone;
					if (bone == null)
					{ continue; }

					//isMain = (curModBone == Editor.Select.ModBone_Main);
					isMain = (curModBone == Editor.Select.ModBone_Gizmo_Main);//<< [GizmoMain]

					if (bone._isIKTail)
					{
						//Debug.Log("Request IK : " + _boneSelectPosW);
						float weight = 1.0f;
						//bool successIK = bone.RequestIK(_boneSelectPosW, weight, !isFirstSelectBone);


						Vector2 bonePosW = Vector2.zero;

						if (isMain)
						{
							if (bone != _prevSelected_TransformBone || isFirstMove)
							{
								_prevSelected_TransformBone = bone;
								_prevSelected_MousePosW = bone._worldMatrix.Pos;
							}

							_prevSelected_MousePosW += deltaMoveW;
							bonePosW = _prevSelected_MousePosW;//DeltaPos + 절대 위치 절충
						}
						else
						{
							//변경 20.6.29 : 상대 좌표로만 계산
							bonePosW = bone._worldMatrix.Pos + deltaMoveW;
						}
						
						apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

						if (limitedHeadBone == null)
						{
							return;
						}


						apBone curIKBone = bone;
						
						//위로 올라가면서 IK 결과값을 Default에 적용하자

						//중요
						//만약, IK Chain이 된 상태에서
						//Chain에 해당하는 Bone이 현재 Keyframe이 없는 경우 (Layer는 있으나 현재 프레임에 해당하는 Keyframe이 없음)
						//자동으로 생성해줘야 한다.

						

						while (true)
						{
							float deltaAngle = curIKBone._IKRequestAngleResult;


							//추가 20.10.9 : 부모 본이 있거나 부모 렌더 유닛의 크기가 한축만 뒤집혔다면 deltaAngle을 반전하자
							if(curIKBone.IsNeedInvertIKDeltaAngle_Gizmo())
							{
								deltaAngle = -deltaAngle;
							}
							

							apAnimTimelineLayer targetLayer = animTimeline._layers.Find(delegate (apAnimTimelineLayer a)
							{
								return a._linkedBone == curIKBone;
							});

							//현재 Bone과 ModBone이 있는가
							apAnimKeyframe targetKeyframe = targetLayer.GetKeyframeByFrameIndex(curFrame);

							if (targetKeyframe == null)
							{
								//해당 키프레임이 없다면
								//새로 생성한다.
								//단, Refresh는 하지 않는다.

								targetKeyframe = Editor.Controller.AddAnimKeyframe(curFrame, targetLayer, false, false, false, false);
								isKeyframeAdded_ByIK = true;//True로 설정하고 나중에 한꺼번에 Refresh하자
							}

							apModifiedBone targetModBone = targetKeyframe._linkedModBone_Editor;

							if (targetModBone != null)
							{
								//해당 Bone의 ModBone을 가져온다.

								//IK로 ModBone의 각도를 설정하는 경우, 각도는 무조건 180 범위 안에 들어와야 한다.
								float nextAngle = apUtil.AngleTo180(curIKBone._localMatrix._angleDeg + deltaAngle);

								//DefaultMatrix말고
								//curBone._defaultMatrix.SetRotate(nextAngle);

								//ModBone을 수정하자
								targetModBone._transformMatrix.SetRotate(nextAngle, true);
							}

							curIKBone._isIKCalculated = false;
							curIKBone._IKRequestAngleResult = 0.0f;

							if (curIKBone == limitedHeadBone)
							{
								break;
							}
							if (curIKBone._parentBone == null)
							{
								break;
							}
							curIKBone = curIKBone._parentBone;
						}
					}
					else if (bone._parentBone == null
						|| (bone._parentBone._IKNextChainedBone != bone))
					{
						
						//---------------------
						// 다시 변경 20.8.19 : BoneWorldMatrix를 이용
						//---------------------
						apBoneWorldMatrix parentMatrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
																	0, Editor._portrait,
																	(bone._parentBone != null ? bone._parentBone._worldMatrix : null),
																	(bone._renderUnit != null ? bone._renderUnit.WorldMatrixWrap : null));

						apBoneWorldMatrix nextWorldMatrix = apBoneWorldMatrix.MakeTempWorldMatrix(1, Editor._portrait, bone._worldMatrix);
						nextWorldMatrix.MoveAsResult(deltaMoveW);
						nextWorldMatrix.SetWorld2Default(parentMatrix);
						nextWorldMatrix.SubtractAsLocalValue(bone._defaultMatrix, false);
						if (bone._isRigTestPosing)
						{
							nextWorldMatrix.SubtractAsLocalValue(bone._rigTestMatrix, false);
						}
						nextWorldMatrix.MakeMatrix(false);

						//Subtract 이후에 MakeMatrix를 해야하지만, 여기서는 Result만 가져와도 될 것 같다. (역연산 직후이므로)
						curModBone._transformMatrix.SetPos(nextWorldMatrix.Pos, true);//<<WorldMatrix 로 변경
						//---------------------
					}
				}

				if(isKeyframeAdded_ByIK)
				{
					Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));
					Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, null, null);

					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);
					
					
					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 강제로 FFD를 취소하자
						Editor.Gizmos.RevertFFDTransformForce();

					}
				}
			}

			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}


		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [이동] - (Autokey가 켜지고 / 임시 값이 열리면) 현재 Timeline에 따라 [Transform / Bone / Vertex]를 선택한다.
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Animation_Vertex(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}
			if (deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}


			//변경 20.6.30 : 다중 처리
			//여러개의 ModMesh를 돌아다니면서 처리를 한다.
			//메인 WorkKeyframe이 없어도 동작할 수 있게 만든다.
			//자동으로 키가 생성되지는 않는다. (애초에 WorkKeyframe이 없으면 버텍스를 선택할 수가 없다..)

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = (timelineLayers != null) ? timelineLayers.Count : 0;

			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes != null) ? workKeyframes.Count : 0;
			
			if (linkedModifier == null 
				|| nTimelineLayers == 0
				|| nWorkKeyframes == 0)
			{
				//수정할 타겟이 없다.
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null
					|| Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null
					|| Editor.Select.ModRenderPins_All.Count == 0)
				{
					return;
				}
			}

			//Undo
			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveVertex, 
													Editor, 
													linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

				if (nMRV == 1)
				{
					//1. 단일 선택일 때
					apRenderVertex renderVert = Editor.Select.ModRenderVert_Main._renderVert;
					
					//삭제
					//renderVert.Calculate();

					//변경 22.5.9 [v1.4.0 : 최적화로 단일 Calculate 함수를 호출할 수 없다.)
					renderVert._parentRenderUnit.CalculateTargetRenderVert(renderVert);



					Vector2 prevDeltaPos2 = Editor.Select.ModRenderVert_Main._modVert._deltaPos;

					

					//변경 21.4.5 : 리깅 적용 (다중 모드 처리시 리깅이 추가될 수 있기 때문)
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


					Vector2 nextWorldPos = prevWorldPos2 + deltaMoveW;

					//이전
					//Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);
					
					//변경 22.5.9
					Vector2 noneMorphedPosM = renderVert._vertex._pos;
					noneMorphedPosM.x += renderVert._parentMesh._matrix_VertToLocal._m02;
					noneMorphedPosM.y += renderVert._parentMesh._matrix_VertToLocal._m12;

					//이전
					//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);

					//변경 21.4.5 : 리깅 적용 (다중 모드 처리시 리깅이 추가될 수 있기 때문)
					//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld
					//							* renderVert._matrix_MeshTransform
					//							* renderVert._matrix_Rigging).inverse).MultiplyPoint(nextWorldPos);

					//변경 22.5.9 : 최적화 코드 적용
					Vector2 nextMorphedPosM = (renderVert._matrix_Rigging_MeshTF_VertWorld.inverse).MultiplyPoint(nextWorldPos);

					Editor.Select.ModRenderVert_Main._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
					Editor.Select.ModRenderVert_Main._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
				}
				else if (nMRV > 1)
				{
					//2. 복수개 선택일 때
					for (int i = 0; i < nMRV; i++)
					{
						curMRV = modRenderVerts[i];
						Vector2 nextWorldPos = curMRV._renderVert._pos_World + deltaMoveW;

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
					int nWeightedMRV = weightedMRVs.Count;

					for (int i = 0; i < nWeightedMRV; i++)
					{
						curMRV = weightedMRVs[i];
						float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos = (curMRV._renderVert._pos_World + deltaMoveW) * weight + (curMRV._renderVert._pos_World) * (1.0f - weight);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]

				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				if (nMRP > 0)
				{
					for (int iMRP = 0; iMRP < nMRP; iMRP++)
					{
						curMRP = modRenderPins[iMRP];
						Vector2 nextWorldPos = curMRP._renderPin._pos_World + deltaMoveW;

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}


				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRP = weightedMRPs.Count;
					
					for (int i = 0; i < nWeightedMRP; i++)
					{
						curMRP = weightedMRPs[i];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos = (curMRP._renderPin._pos_World + deltaMoveW) * weight + (curMRP._renderPin._pos_World) * (1.0f - weight);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
			}
			
			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}




		// Rotate
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [회전] - 현재 Timeline에 따라 [Transform / Bone / Vertex]를 회전한다.
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Animation_Transform(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if(deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}



			#region [미사용 코드] 단일 선택 및 처리
			//apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			//apAnimTimelineLayer animTimelineLayer = Editor.Select.AnimTimelineLayer_Main;
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;

			//if(workKeyframe == null)
			//{
			//	//추가 : 5.29
			//	//키프레임이 없을 때 "AutoKey"가 켜져 있다면, 키프레임을 빠르게 만든다.
			//	//Bone IK에 의해 Keyframe이 생성된 것과 달리 이때는 Link를 해야한다. (파라미터 확인)
			//	if(Editor._isAnimAutoKey && animTimelineLayer != null)
			//	{
			//		apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, animTimelineLayer, true, false, false, true);
			//		if(autoCreatedKeyframe != null)
			//		{
			//			Editor.Select.AutoSelectAnimTimelineLayer(true);
			//			workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//		}
			//	}
			//}


			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	return;
			//}

			//bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			//bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//둘다 해당사항이 없다.
			//	Debug.LogError("Rotate Failed - " + isTargetTransform + " / " + isTargetBone);
			//	return;
			//}


			////Undo
			//if(isFirstRotate)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform, Editor, linkedModifier, targetRenderUnit, false);
			//}


			//if (isTargetTransform)
			//{
			//	apMatrix resultMatrix = null;

			//	apMatrix matx_ToParent = null;
			//	//apMatrix matx_LocalModified = null;
			//	apMatrix matx_ParentWorld = null;


			//	if (modMesh._isMeshTransform)
			//	{
			//		if (modMesh._transform_Mesh != null)
			//		{
			//			resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;

			//			matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_Mesh._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		if (modMesh._transform_MeshGroup != null)
			//		{
			//			resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;

			//			matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_MeshGroup._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if (resultMatrix == null)
			//	{
			//		return;
			//	}


			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW);//각도 변경

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	//변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
			//	float nextLocalAngle = nextLocalModifiedMatrix._angleDeg;
			//	//180각도 제한 옵션 확인후 각도 수정
			//	if(Editor._isAnimRotation180Lock)
			//	{
			//		nextLocalAngle = apUtil.AngleTo180(nextLocalAngle);
			//	}

			//	modMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
			//											nextLocalAngle,
			//											modMesh._transformMatrix._scale);

			//	modMesh.RefreshValues_Check(Editor._portrait);

			//	//apCalculatedLog.InverseResult calResult = modMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(deltaAngleW, Vector2.zero);

			//	//modMesh._transformMatrix.SetRotate(modMesh._transformMatrix._angleDeg + deltaAngleW);


			//	////Pos 보정
			//	//if (calResult != null && calResult._isSuccess)
			//	//{
			//	//	modMesh._transformMatrix.SetPos(calResult._posL_next);
			//	//}


			//	//modMesh._transformMatrix.MakeMatrix();

			//	//이전 코드 : 전체 Refresh
			//	//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//	//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//	if (targetMeshGroup != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//}
			//else if (isTargetBone)
			//{
			//	apBone bone = Editor.Select.Bone;


			//	//Default Angle은 -180 ~ 180 범위 안에 들어간다.
			//	//float nextAngle = bone._defaultMatrix._angleDeg + deltaAngleW;
			//	float nextAngle = modBone._transformMatrix._angleDeg + deltaAngleW;


			//	//변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
			//	if(Editor._isAnimRotation180Lock)
			//	{
			//		nextAngle = apUtil.AngleTo180(nextAngle);
			//	}

			//	modBone._transformMatrix.SetRotate(nextAngle);

			//	if (targetMeshGroup != null)
			//	{
			//		//if (modBone._renderUnit != null)
			//		//{
			//		//	targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
			//		//}
			//		targetMeshGroup.RefreshForce();
			//	}
			//} 
			#endregion


			//변경 20.6.30 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return;
			}




			//Undo
			if(isFirstRotate)
			{	
				if (Editor._isAnimAutoKey)
				{
					//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform, 
																		Editor, 
																		Editor._portrait,
																		targetMeshGroup,
																		linkedModifier, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform,
														Editor, 
														linkedModifier, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
			}

			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(isFirstRotate && Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					//FFD 모드는 강제로 취소한다.
					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}

			//int nCalculated_Mesh = 0;
			//int nCalculated_Bone = 0;

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//삭제 20.11.2
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
					{
						continue;
					}

					//resultMatrix = null;//삭제 20.11.2
					matx_ToParent = null;
					matx_ParentWorld = null;

					if (curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
					else if (!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}

					//삭제 20.11.2
					//if(resultMatrix == null)
					//{
					//	continue;
					//}

					#region [미사용 코드] 이전 방식
					//if(_tmpNextWorldMatrix == null)
					//{
					//	_tmpNextWorldMatrix = new apMatrix(resultMatrix);
					//}
					//else
					//{
					//	_tmpNextWorldMatrix.SetMatrix(resultMatrix, false);
					//}

					//_tmpNextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW, true);//각도 변경

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					////변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
					//float nextLocalAngle = nextLocalModifiedMatrix._angleDeg;
					////180각도 제한 옵션 확인후 각도 수정
					//if(Editor._isAnimRotation180Lock)
					//{
					//	nextLocalAngle = apUtil.AngleTo180(nextLocalAngle);
					//}

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//									nextLocalAngle,
					//									curModMesh._transformMatrix._scale,
					//									true); 
					#endregion



					//변경 20.11.1 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					//추가 : 각도를 바꿀때, 위치도 보존해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}

					curModMesh._transformMatrix.MakeMatrix();

					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					//직접 만든 WorldMatrix를 이용해서 회전
					_tmpNextWorldMatrix.SetRotate(_tmpNextWorldMatrix._angleDeg + deltaAngleW, true);

					//위치를 보정해야한다.

					//Mod Matrix로 이동
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, true);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, true);

					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, false);

					if (Editor._isAnimRotation180Lock)
					{
						//180도 각도 제한
						curModMesh._transformMatrix.SetRotate(apUtil.AngleTo180(_tmpNextWorldMatrix._angleDeg), true);
					}
					else
					{
						//각도 제한이 없는 경우
						curModMesh._transformMatrix.SetRotate(_tmpNextWorldMatrix._angleDeg, true);
					}

					
					

					//nCalculated_Mesh++;
				}
			}

			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }



					//추가 : 21.7.3 : 본의 World Matrix가 반전된 상태면 Delta Angle을 뒤집는다.
					float rotateBoneAngleW = deltaAngleW;					
					if(curModBone._bone._worldMatrix.Is1AxisFlipped())
					{
						rotateBoneAngleW = -deltaAngleW;
					}

					//Default Angle은 -180 ~ 180 범위 안에 들어간다.
					float nextAngle = curModBone._transformMatrix._angleDeg + rotateBoneAngleW;


					//변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
					if(Editor._isAnimRotation180Lock)
					{
						nextAngle = apUtil.AngleTo180(nextAngle);
					}

					curModBone._transformMatrix.SetRotate(nextAngle, true);

					//nCalculated_Bone++;
				}
			}


			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			//targetMeshGroup.RefreshForce();
			Editor.SetRepaint();
			//Debug.Log("Rotate " + deltaAngleW + " / Mesh (" + nCalculated_Mesh + " / " + nModMeshes + ") / Bone (" + nCalculated_Bone + " / " + nModBones + ")");
		}






		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [회전] - 현재 Timeline에 따라 [Transform / Bone / Vertex]를 회전한다.
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Animation_Vertex(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			if (deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}


			//변경 20.6.30 : 다중 처리
			//여러개의 ModMesh를 돌아다니면서 처리를 한다.
			//메인 WorkKeyframe이 없어도 동작할 수 있게 만든다.
			//자동으로 키가 생성되지는 않는다. (애초에 WorkKeyframe이 없으면 버텍스를 선택할 수가 없다..)
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = (timelineLayers != null) ? timelineLayers.Count : 0;

			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes != null) ? workKeyframes.Count : 0;
			
			if (linkedModifier == null 
				|| nTimelineLayers == 0
				|| nWorkKeyframes == 0)
			{
				//수정할 타겟이 없다.
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count == 0)
				{
					return;
				}
			}


			//Undo
			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateVertex, 
													Editor, 
													linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}



			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

				Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPos;

				apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
											* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRV > 1)
				{
					//기본 회전은 MRV가 2개 이상일때부터

					for (int i = 0; i < nMRV; i++)
					{
						curMRV = modRenderVerts[i];
						Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(curMRV._renderVert._pos_World);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos2);
					}
				}
				


				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
					int nWeightedMRVs = weightedMRVs.Count;

					for (int i = 0; i < nWeightedMRVs; i++)
					{
						curMRV = weightedMRVs[i];
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
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				Vector2 centerPos2 = Editor.Select.ModRenderPinsCenterPos;

				apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
											* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRP > 1)
				{
					//기본 회전은 MRP가 2개 이상일때부터
					for (int i = 0; i < nMRP; i++)
					{
						curMRP = modRenderPins[i];
						Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(curMRP._renderPin._pos_World);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos2);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRPs = weightedMRPs.Count;

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

			//업데이트
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}




		// Scale
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [크기] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Animation_Transform(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if(deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			//apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			//apAnimTimelineLayer animTimelineLayer = Editor.Select.AnimTimelineLayer_Main;
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;

			//if(workKeyframe == null)
			//{
			//	//추가 : 5.29
			//	//키프레임이 없을 때 "AutoKey"가 켜져 있다면, 키프레임을 빠르게 만든다.
			//	//Bone IK에 의해 Keyframe이 생성된 것과 달리 이때는 Link를 해야한다. (파라미터 확인)
			//	if(Editor._isAnimAutoKey && animTimelineLayer != null)
			//	{
			//		apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, animTimelineLayer, true, false, false, true);
			//		if(autoCreatedKeyframe != null)
			//		{
			//			Editor.Select.AutoSelectAnimTimelineLayer(true);
			//			workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//		}
			//	}
			//}


			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	return;
			//}

			//bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			//bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//둘다 해당사항이 없다.
			//	return;
			//}

			////Undo
			//if (isFirstScale)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform, Editor, linkedModifier, targetRenderUnit, false);
			//}

			//if (isTargetTransform)
			//{
			//	apMatrix resultMatrix = null;
			//	//apMatrix resultMatrixWithoutMod = null;

			//	apMatrix matx_ToParent = null;
			//	//apMatrix matx_LocalModified = null;
			//	apMatrix matx_ParentWorld = null;

			//	if (modMesh._isMeshTransform)
			//	{
			//		if (modMesh._transform_Mesh != null)
			//		{
			//			resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;
			//			//resultMatrixWithoutMod = modMesh._transform_Mesh._matrix_TFResult_WorldWithoutMod;

			//			matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_Mesh._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		if (modMesh._transform_MeshGroup != null)
			//		{
			//			resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;
			//			//resultMatrixWithoutMod = modMesh._transform_MeshGroup._matrix_TFResult_WorldWithoutMod;

			//			matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_MeshGroup._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if (resultMatrix == null)
			//	{
			//		return;
			//	}

			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleW);

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	modMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
			//										nextLocalModifiedMatrix._angleDeg,
			//										nextLocalModifiedMatrix._scale);

			//	modMesh.RefreshValues_Check(Editor._portrait);

			//	//Vector3 prevScale3 = modMesh._transformMatrix._scale;
			//	//Vector2 prevScale = new Vector2(prevScale3.x, prevScale3.y);

			//	//apCalculatedLog.InverseResult calResult = modMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(0.0f, deltaScaleW);

			//	//modMesh._transformMatrix.SetScale(prevScale + deltaScaleW);

			//	////Pos 보정
			//	//if (calResult != null && calResult._isSuccess)
			//	//{
			//	//	modMesh._transformMatrix.SetPos(calResult._posL_next);
			//	//}


			//	//modMesh._transformMatrix.MakeMatrix();

			//	//이전 코드 : 전체 Refresh
			//	//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//	//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//	if (targetMeshGroup != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//}
			//else if (isTargetBone)
			//{
			//	apBone bone = Editor.Select.Bone;

			//	Vector3 prevScale = modBone._transformMatrix._scale;
			//	Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

			//	modBone._transformMatrix.SetScale(nextScale);

			//	if (targetMeshGroup != null)
			//	{
			//		//if (modBone._renderUnit != null)
			//		//{
			//		//	targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
			//		//}
			//		targetMeshGroup.RefreshForce();
			//	}
			//} 
			#endregion


			//변경 20.6.30 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return;
			}




			//Undo
			if(isFirstScale)
			{	
				if (Editor._isAnimAutoKey)
				{
					//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform, 
																		Editor, 
																		Editor._portrait,
																		targetMeshGroup,
																		linkedModifier, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform,
														Editor, 
														linkedModifier, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
			}

			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(isFirstScale && Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}


			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//삭제 20.11.2
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
					{
						continue;
					}

					//resultMatrix = null;//삭제 20.11.2
					matx_ToParent = null;
					matx_ParentWorld = null;

					if (curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
					else if (!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}

					//if (resultMatrix == null)
					//{
					//	continue;
					//}

					#region [미사용 코드] 이전 방식
					//if(_tmpNextWorldMatrix == null)
					//{
					//	_tmpNextWorldMatrix = new apMatrix(resultMatrix);
					//}
					//else
					//{
					//	_tmpNextWorldMatrix.SetMatrix(resultMatrix, false);
					//}
					//_tmpNextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleW, true);

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//									nextLocalModifiedMatrix._angleDeg,
					//									nextLocalModifiedMatrix._scale,
					//									true); 
					#endregion


					//변경 20.11.1 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					//추가 : 크기를 바꿀때, 위치도 보존해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}

					curModMesh._transformMatrix.MakeMatrix();

					//Scale은 World를 수정하는게 아니라 PosW를 기록한 상태로 Local Scale를 수정하여 Pos를 보정하자
					//1. 현재의 PosW를 계산하기

					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					Vector2 posW_Prev = _tmpNextWorldMatrix._pos;

					//2. Modified의 Scale 수정하기
					curModMesh._transformMatrix.SetScale(curModMesh._transformMatrix._scale + deltaScaleW, true);

					//3. 변경된 Scale로 World Matrix 다시 계산하기
					_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					_tmpNextWorldMatrix.OnBeforeRMultiply();
					
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					//4. 저장된 "이전 PosW"를 대입
					_tmpNextWorldMatrix._pos = posW_Prev;
					_tmpNextWorldMatrix.MakeMatrix();

					//5. 이제 Scale에 따른 Local 위치 변화가 발생했을 것이므로, Pos를 보정한다.
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, true);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, true);

					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, true);

					//코멘트 20.11.2 : 이렇게 하면 Default Matrix에 회전값이 포함된 경우 축이 뒤틀리는 문제가 발생한다.
					//심지어 편집이 힘들 수 있는데, 해결법을 모르겠다.
				}
			}


			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }

					Vector3 prevScale = curModBone._transformMatrix._scale;
					Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

					curModBone._transformMatrix.SetScale(nextScale, true);
				}
			}


			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}




		/// <summary>
		/// Animation 편집중의 Gizmo 이벤트 : [크기] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Animation_Vertex(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			if (deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{ return; }


			//변경 20.6.30 : 다중 처리
			//여러개의 ModMesh를 돌아다니면서 처리를 한다.
			//메인 WorkKeyframe이 없어도 동작할 수 있게 만든다.
			//자동으로 키가 생성되지는 않는다. (애초에 WorkKeyframe이 없으면 버텍스를 선택할 수가 없다..)
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = (timelineLayers != null) ? timelineLayers.Count : 0;

			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes != null) ? workKeyframes.Count : 0;
			
			if (linkedModifier == null 
				|| nTimelineLayers == 0
				|| nWorkKeyframes == 0)
			{
				//수정할 타겟이 없다.
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count == 0)
				{
					return;
				}
			}


			//Undo
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_ScaleVertex,
													Editor, 
													linkedModifier,
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

				Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPos;

				Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

				apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, 0, scale)
											* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRV > 1)
				{
					for (int i = 0; i < nMRV; i++)
					{
						curMRV = modRenderVerts[i];

						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRV._renderVert._pos_World);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos2);
					}
				}
				

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
					int nWeightedMRVs = weightedMRVs.Count;

					for (int i = 0; i < nWeightedMRVs; i++)
					{
						curMRV = weightedMRVs[i];
						float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRV._renderVert._pos_World);
						Vector2 nextWorldPos = nextWorldPos2 * weight + (curMRV._renderVert._pos_World) * (1.0f - weight);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				Vector2 centerPos2 = Editor.Select.ModRenderPinsCenterPos;

				Vector2 scale = new Vector2(1.0f + deltaScaleW.x, 1.0f + deltaScaleW.y);

				apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, 0, scale)
											* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRP > 1)
				{
					for (int i = 0; i < nMRP; i++)
					{
						curMRP = modRenderPins[i];

						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRP._renderPin._pos_World);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos2);
					}
				}
				

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRPs = weightedMRPs.Count;

					for (int i = 0; i < nWeightedMRPs; i++)
					{
						curMRP = weightedMRPs[i];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRP._renderPin._pos_World);
						Vector2 nextWorldPos = nextWorldPos2 * weight + (curMRP._renderPin._pos_World) * (1.0f - weight);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
			}


			//업데이트
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}



		//----------------------------------------------------------------------------------------------
		// Transform Changed 이벤트들
		//----------------------------------------------------------------------------------------------



		// TransformChanged - Position
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [위치값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 위치를 변경한다.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Animation_Transform(Vector2 pos)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			//변경 20.7.1 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return;
			}


			
			//Undo
			if (Editor._isAnimAutoKey)
			{
				//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform,
																	Editor,
																	Editor._portrait,
																	targetMeshGroup,
																	linkedModifier,
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
			}
			else
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform,
													Editor, 
													linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}



			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}




			Vector2 deltaPos = Vector2.zero;
			object targetObj = null;


			//>> [GizmoMain]
			if(Editor.Select.ModMesh_Gizmo_Main != null)
			{
				targetObj = Editor.Select.ModMesh_Gizmo_Main;
				deltaPos = pos - Editor.Select.ModMesh_Gizmo_Main._transformMatrix._pos;
			}
			else if(Editor.Select.ModBone_Gizmo_Main != null)
			{
				targetObj = Editor.Select.ModBone_Gizmo_Main;
				deltaPos = pos - Editor.Select.ModBone_Gizmo_Main._transformMatrix._pos;
			}
			else
			{
				return;
			}




			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				//Debug.LogError("No ModMesh/ModBone (Gizmo)");
				return;
			}

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if(curModMesh == null)
					{
						continue;
					}

					if (curModMesh == targetObj)
					{
						//메인이라면 바로 할당
						curModMesh._transformMatrix.SetPos(pos, true);
					}
					else
					{
						//서브라면 deltaPos를 더한다.
						curModMesh._transformMatrix.SetPos(curModMesh._transformMatrix._pos + deltaPos, true);
					}
				}
			}

			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }


					if (curModBone == targetObj)
					{
						//메인이라면 바로 할당
						curModBone._transformMatrix.SetPos(pos, true);
					}
					else
					{
						//서브라면 deltaPos를 더한다.
						curModBone._transformMatrix.SetPos(curModBone._transformMatrix._pos + deltaPos, true);
					}
				}
			}

			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}




		/// <summary>
		/// Animation 편집 중의 값 변경 : [위치값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 위치를 변경한다.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Animation_Vertex(Vector2 pos)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}


			//변경 20.7.1 : 다중 처리
			//여러개의 ModMesh를 돌아다니면서 처리를 한다.
			//메인 WorkKeyframe이 없어도 동작할 수 있게 만든다.
			//자동으로 키가 생성되지는 않는다. (애초에 WorkKeyframe이 없으면 버텍스를 선택할 수가 없다..)
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = (timelineLayers != null) ? timelineLayers.Count : 0;

			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes != null) ? workKeyframes.Count : 0;
			
			if (linkedModifier == null 
				|| nTimelineLayers == 0
				|| nWorkKeyframes == 0)
			{
				//수정할 타겟이 없다.
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count == 0)
				{
					return;
				}
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveVertex, 
												Editor, 
												linkedModifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);


			
			Vector2 deltaPosChanged = Vector2.zero;

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]

				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;


				if (nMRV == 1)
				{
					deltaPosChanged = pos - Editor.Select.ModRenderVert_Main._modVert._deltaPos;
					Editor.Select.ModRenderVert_Main._modVert._deltaPos = pos;//<<바로 LocalPos로 넣자
				}
				else if(nMRV > 1)
				{
					Vector2 avgDeltaPosPrev = Vector2.zero;
				
					for (int i = 0; i < nMRV; i++)
					{
						avgDeltaPosPrev += modRenderVerts[i]._modVert._deltaPos;
					}
					avgDeltaPosPrev /= nMRV;

					//Prev로부터의 변화값을 모두 대입하자
					Vector2 deltaPos2Next = pos - avgDeltaPosPrev;
					deltaPosChanged = deltaPos2Next;

					for (int i = 0; i < nMRV; i++)
					{
						modRenderVerts[i]._modVert._deltaPos += deltaPos2Next;
					}
				}

				//Soft Selection 상태일때
				//deltaPosChanged를 이용해서 SoftSelection에 적용하자
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
					int nWeightedMRVs = weightedMRVs.Count;

					for (int i = 0; i < nWeightedMRVs; i++)
					{
						curMRV = weightedMRVs[i];
						float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

						////Weight를 적용한 만큼만 움직이자
						curMRV._modVert._deltaPos = ((curMRV._modVert._deltaPos + deltaPosChanged) * weight) + (curMRV._modVert._deltaPos * (1.0f - weight));
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]

				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				if (nMRP == 1)
				{
					deltaPosChanged = pos - Editor.Select.ModRenderPin_Main._modPin._deltaPos;
					Editor.Select.ModRenderPin_Main._modPin._deltaPos = pos;//<<바로 LocalPos로 넣자
				}
				else if(nMRP > 1)
				{
					Vector2 avgDeltaPosPrev = Vector2.zero;
				
					for (int i = 0; i < nMRP; i++)
					{
						avgDeltaPosPrev += modRenderPins[i]._modPin._deltaPos;
					}
					avgDeltaPosPrev /= nMRP;

					//Prev로부터의 변화값을 모두 대입하자
					Vector2 deltaPos2Next = pos - avgDeltaPosPrev;
					deltaPosChanged = deltaPos2Next;

					for (int i = 0; i < nMRP; i++)
					{
						modRenderPins[i]._modPin._deltaPos += deltaPos2Next;
					}
				}

				//Soft Selection 상태일때
				//deltaPosChanged를 이용해서 SoftSelection에 적용하자
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRPs = weightedMRPs.Count;

					for (int i = 0; i < nWeightedMRPs; i++)
					{
						curMRP = weightedMRPs[i];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						curMRP._modPin._deltaPos = ((curMRP._modPin._deltaPos + deltaPosChanged) * weight) + (curMRP._modPin._deltaPos * (1.0f - weight));
					}
				}
			}

			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}


		// TransformChanged - Rotate
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [회전값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 각도를 변경한다.
		/// </summary>
		/// <param name="angle"></param>
		public void TransformChanged_Rotate__Animation_Transform(float angle)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			//apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	return;
			//}

			//bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			//bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//둘다 해당사항이 없다.
			//	return;
			//}

			////Undo
			//apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform, Editor, linkedModifier, targetRenderUnit, false);


			//if (isTargetTransform)
			//{
			//	modMesh._transformMatrix.SetRotate(angle);
			//	modMesh._transformMatrix.MakeMatrix();

			//	//이전 코드 : 전체 Refresh
			//	//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//	//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//	if (targetMeshGroup != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//}
			//else if (isTargetBone)
			//{
			//	apBone bone = Editor.Select.Bone;

			//	//만약 범위 제한이 있으면 그 안으로 제한해야한다.
			//	//변경 : 절대값이 아니라 DefaultMatrix의 AngleDeg + 상대값으로 바꾼다.
			//	if (bone._isIKAngleRange)
			//	{
			//		if (angle < bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower)
			//		{
			//			angle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower;
			//		}
			//		else if (angle > bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper)
			//		{
			//			angle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper;
			//		}
			//	}

			//	modBone._transformMatrix.SetRotate(angle);



			//	if (modBone._renderUnit != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//} 
			#endregion

			//변경 20.7.1 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return;
			}


			
			//Undo
			if (Editor._isAnimAutoKey)
			{
				//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform,
																	Editor,
																	Editor._portrait,
																	targetMeshGroup,
																	linkedModifier,
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
			}
			else
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform,
													Editor, 
													linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}

			float deltaAngle = 0.0f;
			object targetObj = null;

			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;

			////> 메인으로 부터 DeltaAngle을 계산해서 작업해야하므로, 메인이 없으면 취소된다. (자동 키프레임 생성 이후에 처리할 것)
			//if(modMesh != null)
			//{
			//	targetObj = modMesh;
			//	deltaAngle = angle - modMesh._transformMatrix._angleDeg;
			//}
			//else if(modBone != null)
			//{
			//	targetObj = modBone;
			//	deltaAngle = angle - modBone._transformMatrix._angleDeg;
			//}
			//else
			//{
			//	//Debug.LogError("No ModMesh/ModBone");
			//	return;
			//}

			//>> [GizmoMain]
			if(Editor.Select.ModMesh_Gizmo_Main != null)
			{
				targetObj = Editor.Select.ModMesh_Gizmo_Main;
				deltaAngle = angle - Editor.Select.ModMesh_Gizmo_Main._transformMatrix._angleDeg;
			}
			else if(Editor.Select.ModBone_Gizmo_Main != null)
			{
				targetObj = Editor.Select.ModBone_Gizmo_Main;
				deltaAngle = angle - Editor.Select.ModBone_Gizmo_Main._transformMatrix._angleDeg;
			}
			else
			{
				return;
			}


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				//Debug.LogError("No ModMesh/ModBone (Gizmo)");
				return;
			}

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;

				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if(curModMesh == null)
					{
						continue;
					}

					//이전 방식 : 그냥 ModMesh의 값을 변환
					//if (curModMesh == targetObj)
					//{
					//	//메인이라면 바로 할당
					//	curModMesh._transformMatrix.SetRotate(angle, true);
					//}
					//else
					//{
					//	//서브라면 deltaPos를 더한다.
					//	curModMesh._transformMatrix.SetRotate(curModMesh._transformMatrix._angleDeg + deltaAngle, true);
					//}


					//변경 20.11.2 : PosW를 유지한 상태에서 회전할 수 있도록 지정
					//Rotate 함수와도 조금 다르며, 오히려 Scale 함수를 참고하자
					matx_ToParent = null;
					matx_ParentWorld = null;

					if(curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.1
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
					else if(!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.1
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}

					//변경 20.11.1 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					//추가 : 각도를 바꿀때, 위치도 보존해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}

					curModMesh._transformMatrix.MakeMatrix();


					//1. 현재의 PosW를 계산하기

					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					Vector2 posW_Prev = _tmpNextWorldMatrix._pos;

					//2. Modified의 Angle 수정하기 <중요!>
					if (curModMesh == targetObj)
					{
						//메인이라면 바로 할당
						curModMesh._transformMatrix.SetRotate(angle, true);
					}
					else
					{
						//서브라면 deltaPos를 더한다.
						curModMesh._transformMatrix.SetRotate(curModMesh._transformMatrix._angleDeg + deltaAngle, true);
					}

					//3. 변경된 Angle로 World Matrix 다시 계산하기
					_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					_tmpNextWorldMatrix.OnBeforeRMultiply();
					
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					//4. 저장된 "이전 PosW"를 대입
					_tmpNextWorldMatrix._pos = posW_Prev;
					_tmpNextWorldMatrix.MakeMatrix();

					//5. 이제 Scale에 따른 Local 위치 변화가 발생했을 것이므로, Pos를 보정한다.
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, true);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, true);
					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, true);//<중요!>


					curModMesh.RefreshValues_Check(Editor._portrait);
				}
			}

			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }


					if (curModBone == targetObj)
					{
						//메인이라면 바로 할당 (IK 범위는 여기서 고려하지 말자)
						curModBone._transformMatrix.SetRotate(angle, true);
					}
					else
					{
						//서브라면 deltaPos를 더한다.
						curModBone._transformMatrix.SetRotate(curModBone._transformMatrix._angleDeg + deltaAngle, true);
					}
				}
			}

			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}





		/// <summary>
		/// Animation 편집 중의 값 변경 : [회전값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 각도를 변경한다.
		/// </summary>
		/// <param name="angle"></param>
		public void TransformChanged_Rotate__Animation_Vertex(float angle)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			//회전값은... Vertex에선 수정 못함
		}



		// TransformChanged - Scale
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [크기값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="scale"></param>
		public void TransformChanged_Scale__Animation_Transform(Vector2 scale)
		{
			if (Editor.Select.AnimClip == null ||
				Editor.Select.AnimClip._targetMeshGroup == null ||
				Editor.Select.AnimTimeline == null)
			{
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			//apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	return;
			//}

			//bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			//bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//둘다 해당사항이 없다.
			//	return;
			//}


			////Undo
			//apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform, Editor, linkedModifier, targetRenderUnit, false);

			//if (isTargetTransform)
			//{
			//	modMesh._transformMatrix.SetScale(scale);
			//	modMesh._transformMatrix.MakeMatrix();

			//	//이전 코드 : 전체 Refresh
			//	//Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			//	//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//	if (targetMeshGroup != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//}
			//else if (isTargetBone)
			//{
			//	apBone bone = Editor.Select.Bone;


			//	modBone._transformMatrix.SetScale(scale);//<<걍 직접 넣자


			//	if (modBone._renderUnit != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modBone._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//} 
			#endregion



			//변경 20.7.1 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return;
			}


			
			//Undo
			if (Editor._isAnimAutoKey)
			{
				//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform,
																	Editor,
																	Editor._portrait,
																	targetMeshGroup,
																	linkedModifier,
																	//null, 
																	false,
																	apEditorUtil.UNDO_STRUCT.ValueOnly);
			}
			else
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform,
													Editor, 
													linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}



			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}




			//선택된 메인 오브젝트의 크기를 기준으로 deltaScale을 구한다. (비율)
			Vector2 deltaScale = Vector2.one;
			object targetObj = null;

			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			
			//if(modMesh != null)
			//{
			//	targetObj = modMesh;
			//	deltaScale.x = (modMesh._transformMatrix._scale.x != 0.0f) ? (scale.x / modMesh._transformMatrix._scale.x) : 1.0f;
			//	deltaScale.y = (modMesh._transformMatrix._scale.y != 0.0f) ? (scale.y / modMesh._transformMatrix._scale.y) : 1.0f;
			//}
			//else if(modBone != null)
			//{
			//	targetObj = modBone;
			//	deltaScale.x = (modBone._transformMatrix._scale.x != 0.0f) ? (scale.x / modBone._transformMatrix._scale.x) : 1.0f;
			//	deltaScale.y = (modBone._transformMatrix._scale.y != 0.0f) ? (scale.y / modBone._transformMatrix._scale.y) : 1.0f;
			//}
			//else
			//{
			//	//Debug.LogError("No ModMesh/ModBone");
			//	return;
			//}

			//>> [GizmoMain]
			Vector2 mainScale = Vector2.one;
			if(Editor.Select.ModMesh_Gizmo_Main != null)
			{
				targetObj = Editor.Select.ModMesh_Gizmo_Main;
				mainScale = Editor.Select.ModMesh_Gizmo_Main._transformMatrix._scale;
			}
			else if(Editor.Select.ModBone_Gizmo_Main != null)
			{
				targetObj = Editor.Select.ModBone_Gizmo_Main;
				mainScale = Editor.Select.ModBone_Gizmo_Main._transformMatrix._scale;
			}
			else
			{
				return;
			}

			deltaScale.x = (mainScale.x != 0.0f) ? (scale.x / mainScale.x) : 1.0f;
			deltaScale.y = (mainScale.y != 0.0f) ? (scale.y / mainScale.y) : 1.0f;


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				//Debug.LogError("No ModMesh/ModBone (Gizmo)");
				return;
			}

			Vector2 nextScale = Vector2.zero;

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;

				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if(curModMesh == null)
					{
						continue;
					}

					//이전 방식 : 그냥 ModMesh의 값을 변환
					//if (curModMesh == targetObj)
					//{
					//	//메인이라면 바로 할당
					//	nextScale = scale;
					//}
					//else
					//{
					//	//서브라면 deltaScale을 곱한다.
					//	nextScale.x = curModMesh._transformMatrix._scale.x * deltaScale.x;
					//	nextScale.y = curModMesh._transformMatrix._scale.y * deltaScale.y;
					//}
					//curModMesh._transformMatrix.SetScale(nextScale, true);



					//변경 20.11.2 : PosW를 유지한 상태에서 회전할 수 있도록 지정
					//Rotate 함수와도 조금 다르며, 오히려 Scale 함수를 참고하자

					matx_ToParent = null;
					matx_ParentWorld = null;

					if (curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.1
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
					else if (!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.1
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}

					//변경 20.11.1 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					//크기를 바꿀때, 위치도 보존해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}

					curModMesh._transformMatrix.MakeMatrix();


					//1. 현재의 PosW를 계산하기
					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					Vector2 posW_Prev = _tmpNextWorldMatrix._pos;

					//2. Modified의 Angle 수정하기 <중요!>
					if (curModMesh == targetObj)
					{
						//메인이라면 바로 할당
						nextScale = scale;
					}
					else
					{
						//서브라면 deltaScale을 곱한다.
						nextScale.x = curModMesh._transformMatrix._scale.x * deltaScale.x;
						nextScale.y = curModMesh._transformMatrix._scale.y * deltaScale.y;
					}
					curModMesh._transformMatrix.SetScale(nextScale, true);

					//3. 변경된 Scale로 World Matrix 다시 계산하기
					_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					_tmpNextWorldMatrix.OnBeforeRMultiply();

					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					//4. 저장된 "이전 PosW"를 대입
					_tmpNextWorldMatrix._pos = posW_Prev;
					_tmpNextWorldMatrix.MakeMatrix();

					//5. 이제 Scale에 따른 Local 위치 변화가 발생했을 것이므로, Pos를 보정한다.
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, true);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, true);
					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, true);//<중요!>


					curModMesh.RefreshValues_Check(Editor._portrait);
				}
			}

			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }


					if (curModBone == targetObj)
					{
						//메인이라면 바로 할당
						nextScale = scale;
					}
					else
					{
						//서브라면 deltaScale을 곱한다.
						nextScale.x = curModBone._transformMatrix._scale.x * deltaScale.x;
						nextScale.y = curModBone._transformMatrix._scale.y * deltaScale.y;
					}
					curModBone._transformMatrix.SetScale(nextScale, true);
				}
			}

			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}




		/// <summary>
		/// Animation 편집 중의 값 변경 : [크기값 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 크기를 변경한다.
		/// </summary>
		/// <param name="scale"></param>
		public void TransformChanged_Scale__Animation_Vertex(Vector2 scale)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			//크기값은... Vertex에선 수정 못함
		}



		// Transform Changed - Color
		//----------------------------------------------------------------------------------------------

		/// <summary>
		/// Animation 편집 중의 값 변경 : [색상 변경] - 현재 Timeline에 따라 [Transform / Bone / Vertex]의 색상을 변경한다.
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Animation_Transform(Color color, bool isVisible)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}



			#region [미사용 코드] 단일 선택 및 처리
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

			//if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
			//{
			//	//수정할 타겟이 없다.
			//	return;
			//}

			//if (targetModMesh._transform_Mesh == null && targetModMesh._transform_MeshGroup == null)
			//{
			//	//대상이 되는 Mesh/MeshGroup이 없다?
			//	return;
			//}

			//if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			//{
			//	//에디팅 중이 아니다.
			//	return;
			//}

			////Undo
			//apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_Color, Editor, linkedModifier, targetRenderUnit, false);

			//targetModMesh._meshColor = color;
			//targetModMesh._isVisible = isVisible; 
			#endregion


			//변경 20.7.1 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return;
			}

			//색상 옵션은 AutoKey가 적용되지 않는다.

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null || workKeyframes.Count == 0)
			{
				return;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return;
			}


			
			//Undo
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_Color, 
												Editor, 
												linkedModifier, 
												//null, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;

			if(nModMeshes == 0)
			{
				//선택된게 없다면
				return;
			}

			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if(curModMesh == null)
					{
						continue;
					}

					curModMesh._meshColor = color;
					curModMesh._isVisible = isVisible;
				}
			}

			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();
		}





		// Transform Changed - Extra
		//----------------------------------------------------------------------------------------------
		public void TransformChanged_Extra__Animation_Transform()
		{
			//TODO
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return;
			}

			//Extra는 기즈모 메인이 아닌 그냥 메인 ModMesh에 대해서 적용한다.


			apAnimClip animClip = Editor.Select.AnimClip;
			apMeshGroup targetMeshGroup = animClip._targetMeshGroup;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			List<apModifiedMesh> targetModMeshes = Editor.Select.ModMeshes_All;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			List<apRenderUnit> targetRenderUnits = Editor.Select.RenderUnitOfMod_All;



			if (linkedModifier == null 
				//|| workKeyframe == null 
				|| targetModMesh == null || targetRenderUnit == null)
			{
				//수정할 타겟이 없다.
				return;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return;
			}

			if(!linkedModifier._isExtraPropertyEnabled)
			{
				//Extra Option을 사용하지 않는다.
				return;
			}
			
			//Extra Option을 제어하는 Dialog를 호출하자
			//이전
			//apDialog_ExtraOption.ShowDialog(Editor, Editor._portrait, targetMeshGroup, linkedModifier, targetModMesh, targetRenderUnit, true, animClip, workKeyframe);

			//변경 21.10.2
			apDialog_ExtraOption.ShowDialog_Keyframe(Editor, Editor._portrait, targetMeshGroup, linkedModifier, targetRenderUnit, targetRenderUnits, targetModMeshes, animClip);
		}



		//----------------------------------------------------------------------------------------------
		// 단축키 이벤트 (추가 3.24)
		//----------------------------------------------------------------------------------------------
		public void AddHotKeys__Animation_Vertex(bool isGizmoRenderable, apGizmos.CONTROL_TYPE controlType, bool isFFDMode)
		{
			//"Select All Vertices"
			//Editor.AddHotKeyEvent(OnHotKeyEvent__Animation_Vertex__Ctrl_A, apHotKey.LabelText.SelectAllVertices, KeyCode.A, false, false, true, null);
			Editor.AddHotKeyEvent(OnHotKeyEvent__Animation_Vertex__Ctrl_A, apHotKeyMapping.KEY_TYPE.SelectAllVertices_EditMod, null);//변경 20.12.3
		}

		// 단축키 : 버텍스 전체 선택
		private apHotKey.HotKeyResult OnHotKeyEvent__Animation_Vertex__Ctrl_A(object paramObject)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			if (Editor.Select.AnimTimeline == null ||
				Editor.Select.AnimTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier ||
				Editor.Select.AnimTimeline._linkedModifier == null
				)
			{
				return null;
			}

			if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) == 0)
			{
				return null;
			}




			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

			if (workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
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

			bool isAnyChanged = false;

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				
				//핀은 선택 해제
				Editor.Select.SelectModRenderPin_ForAnimEdit(null);

				List<apSelection.ModRenderVert> selectableMRVs = Editor.Select.ModData.ModRenderVert_All;
				int nMRVs = selectableMRVs != null ? selectableMRVs.Count : 0;

				//변경 20.6.29 : 이미 생성된 MRV에서 선택하자
				if (nMRVs > 0)
				{
					apSelection.ModRenderVert curMRV = null;
					for (int iMRV = 0; iMRV < nMRVs; iMRV++)
					{
						curMRV = selectableMRVs[iMRV];
						Editor.Select.AddModRenderVert_ForAnimEdit(curMRV);
					}
					isAnyChanged = true;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]

				// 버텍스는 선택 해제
				Editor.Select.SelectModRenderVert_ForAnimEdit(null);//변경 20.6.29

				List<apSelection.ModRenderPin> selectableMRPs = Editor.Select.ModData.ModRenderPin_All;
				int nMRPs = selectableMRPs != null ? selectableMRPs.Count : 0;

				//이미 생성된 MRP에서 선택하자
				if (nMRPs > 0)
				{
					apSelection.ModRenderPin curMRP = null;
					for (int iMRP = 0; iMRP < nMRPs; iMRP++)
					{
						curMRP = selectableMRPs[iMRP];
						Editor.Select.AddModRenderPin_ForAnimEdit(curMRP);
					}
					isAnyChanged = true;
				}
			}

			if (isAnyChanged)
			{
				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					// [ 버텍스 편집 모드 ]
					Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
				}
				else
				{
					// [ 핀 편집 모드 ]
					Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
				}
				

				//Debug.LogError("성공");
				Editor.RefreshControllerAndHierarchy(false);
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);
				Editor.SetRepaint();
			}

			return apHotKey.HotKeyResult.MakeResult();
		}




		//----------------------------------------------------------------------------------------------
		// 키보드 입력 TRS / Vertex와 Transform 따로
		//----------------------------------------------------------------------------------------------
		//Transform Move/Rotate/Scale
		private bool OnHotKeyEvent__Animation_Transform__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			//Move__Animation_Transform 함수의 코드를 이용

			if (Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip._targetMeshGroup == null
				|| Editor.Select.AnimTimeline == null
				|| Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.IsAnimPlaying
				)
			{
				return false;
			}

			//변경 20.7.1 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return false;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return false;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return false;
			}

			//Undo : 위치가 AutoKey보다 앞쪽에 있어야 한다.
			if (isFirstMove)
			{
				if (Editor._isAnimAutoKey)
				{
					//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform, 
																		Editor, 
																		Editor._portrait,
																		targetMeshGroup,
																		linkedModifier, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveTransform,
														Editor, 
														linkedModifier, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
			}

			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(isFirstMove && Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return false;
			}

			
			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//삭제 20.11.2
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if(curModMesh == null)
					{
						continue;
					}

					//resultMatrix = null;//삭제 20.11.2
					matx_ToParent = null;
					matx_ParentWorld = null;

					if(curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
					else if(!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}

					//삭제 20.11.2
					//if(resultMatrix == null)
					//{
					//	continue;
					//}

					#region [미사용 코드] 이전 방식
					//if(_tmpNextWorldMatrix == null)
					//{
					//	_tmpNextWorldMatrix = new apMatrix(resultMatrix);
					//}
					//else
					//{
					//	_tmpNextWorldMatrix.SetMatrix(resultMatrix, false);
					//}
					//_tmpNextWorldMatrix.SetPos(resultMatrix._pos + deltaMoveW, true);

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					////TRS중에 Pos만 넣어도 된다.
					//curModMesh._transformMatrix.SetPos(nextLocalModifiedMatrix._pos, true);

					//curModMesh.RefreshValues_Check(Editor._portrait); 
					#endregion


					//변경 20.10.31 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}

					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, false);

					//직접 만든 WorldMatrix를 이용해서 위치를 이동
					_tmpNextWorldMatrix._pos += deltaMoveW;

					//Mod Matrix로 이동
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, false);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, false);

					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, true);//버그 해결!



					curModMesh.RefreshValues_Check(Editor._portrait);
				}
			}

			bool isKeyframeAdded_ByIK = false;

			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }

					if (bone._isIKTail)
					{
						//Debug.Log("Request IK : " + _boneSelectPosW);
						float weight = 1.0f;
						
						//변경 20ㅣ.6.29 : 상대 좌표로만 계산
						Vector2 bonePosW = bone._worldMatrix.Pos + deltaMoveW;
						apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

						if (limitedHeadBone == null)
						{
							continue;
						}


						apBone curIKBone = bone;
						
						//위로 올라가면서 IK 결과값을 Default에 적용하자

						//중요
						//만약, IK Chain이 된 상태에서
						//Chain에 해당하는 Bone이 현재 Keyframe이 없는 경우 (Layer는 있으나 현재 프레임에 해당하는 Keyframe이 없음)
						//자동으로 생성해줘야 한다.
						while (true)
						{
							float deltaAngle = curIKBone._IKRequestAngleResult;


							//추가 20.10.9 : 부모 본이 있거나 부모 렌더 유닛의 크기가 한축만 뒤집혔다면 deltaAngle을 반전하자
							if(curIKBone.IsNeedInvertIKDeltaAngle_Gizmo())
							{
								deltaAngle = -deltaAngle;
							}


							apAnimTimelineLayer targetLayer = animTimeline._layers.Find(delegate (apAnimTimelineLayer a)
							{
								return a._linkedBone == curIKBone;
							});

							//현재 Bone과 ModBone이 있는가
							apAnimKeyframe targetKeyframe = targetLayer.GetKeyframeByFrameIndex(curFrame);

							if (targetKeyframe == null)
							{
								//해당 키프레임이 없다면
								//새로 생성한다.
								//단, Refresh는 하지 않는다.

								targetKeyframe = Editor.Controller.AddAnimKeyframe(curFrame, targetLayer, false, false, false, false);
								isKeyframeAdded_ByIK = true;//True로 설정하고 나중에 한꺼번에 Refresh하자
							}

							apModifiedBone targetModBone = targetKeyframe._linkedModBone_Editor;

							if (targetModBone != null)
							{
								//해당 Bone의 ModBone을 가져온다.

								//IK로 ModBone의 각도를 설정하는 경우, 각도는 무조건 180 범위 안에 들어와야 한다.
								float nextAngle = apUtil.AngleTo180(curIKBone._localMatrix._angleDeg + deltaAngle);

								//DefaultMatrix말고 ModBone을 수정하자
								targetModBone._transformMatrix.SetRotate(nextAngle, true);
							}

							curIKBone._isIKCalculated = false;
							curIKBone._IKRequestAngleResult = 0.0f;

							if (curIKBone == limitedHeadBone)
							{
								break;
							}
							if (curIKBone._parentBone == null)
							{
								break;
							}
							curIKBone = curIKBone._parentBone;
						}
					}
					else if (bone._parentBone == null
						|| (bone._parentBone._IKNextChainedBone != bone))
					{
						//수정 : Parent가 있지만 IK로 연결 안된 경우 / Parent가 없는 경우 2가지 모두 처리한다.

						//이전 방식
						//apMatrix parentMatrix = null;//기존
						//apComplexMatrix parentMatrix = null;//20.8.13 : ComplexMatrix


						//if (bone._parentBone == null)
						//{
						//	if (bone._renderUnit != null)
						//	{
						//		//Render Unit의 World Matrix를 참조하여
						//		//로컬 값을 Default로 적용하자
						//		parentMatrix = apComplexMatrix.TempMatrix_1;
						//		parentMatrix.SetMatrix(bone._renderUnit.WorldMatrixWrap, true);
						//	}
						//}
						//else
						//{
						//	parentMatrix = bone._parentBone._worldMatrix;
						//}




						//---------------------
						// 기존 코드 : apMatrix 사용
						//---------------------
						//if(_tmpNextWorldMatrix == null)
						//{
						//	_tmpNextWorldMatrix = new apMatrix(bone._worldMatrix);
						//}
						//else
						//{
						//	_tmpNextWorldMatrix.SetMatrix(bone._worldMatrix);
						//}
						//_tmpNextWorldMatrix.SetPos(_tmpNextWorldMatrix._pos + deltaMoveW);

						//if (parentMatrix != null)
						//{
						//	_tmpNextWorldMatrix.RInverse(parentMatrix);
						//}
						////WorldMatrix에서 Local Space에 위치한 Matrix는
						////Default + Local + RigTest이다.

						//_tmpNextWorldMatrix.Subtract(bone._defaultMatrix);
						//if (bone._isRigTestPosing)
						//{
						//	_tmpNextWorldMatrix.Subtract(bone._rigTestMatrix);
						//}

						//curModBone._transformMatrix.SetPos(_tmpNextWorldMatrix._pos);


						//---------------------
						// 변경 20.8.13 : ComplexMatrix로 변경 TODO (개선할 것)
						//---------------------
						//Debug.LogError("TODO : 코드 개선할 것");							
						//apComplexMatrix tempNextWorldMatrix = apComplexMatrix.TempMatrix_2;//Temp1은 위에서 사용하고 있다.
						//tempNextWorldMatrix.CopyFromComplexMatrix(bone._worldMatrix);
						
						////SetPos대신 Move를 넣는다. (Delta값을 넣어야 한다.)
						//tempNextWorldMatrix.MoveAsPostResult(deltaMoveW);

						//if (parentMatrix != null)
						//{
						//	tempNextWorldMatrix.Inverse(parentMatrix);
						//}
						//else
						//{
						//	//<중요> 역연산을 하지 않더라도 SMultiply 부분은 초기화해야한다.
						//	tempNextWorldMatrix.ResetStep2();
						//}

						////WorldMatrix에서 Local Space에 위치한 Matrix는
						////Default + Local + RigTest이다.

						//tempNextWorldMatrix.Subtract(bone._defaultMatrix);
						//if (bone._isRigTestPosing)
						//{
						//	tempNextWorldMatrix.Subtract(bone._rigTestMatrix);
						//}


						//--------------------------------------
						// 다시 변경된 방식 20.8.19 : BoneWorldMatrix를 이용
						//--------------------------------------
						apBoneWorldMatrix parentMatrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
																	0, Editor._portrait,
																	(bone._parentBone != null ? bone._parentBone._worldMatrix : null),
																	(bone._renderUnit != null ? bone._renderUnit.WorldMatrixWrap : null));

						apBoneWorldMatrix nextWorldMatrix = apBoneWorldMatrix.MakeTempWorldMatrix(1, Editor._portrait, bone._worldMatrix);
						nextWorldMatrix.MoveAsResult(deltaMoveW);
						nextWorldMatrix.SetWorld2Default(parentMatrix);
						nextWorldMatrix.SubtractAsLocalValue(bone._defaultMatrix, false);
						if (bone._isRigTestPosing)
						{
							nextWorldMatrix.SubtractAsLocalValue(bone._rigTestMatrix, false);
						}
						nextWorldMatrix.MakeMatrix(false);

						//Subtract 이후에 MakeMatrix를 해야하지만, 여기서는 Result만 가져와도 될 것 같다. (역연산 직후이므로)
						//curModBone._transformMatrix.SetPos(tempNextWorldMatrix._pos_Step1);//<<Result대신 Step1의 값을 이용했다.
						curModBone._transformMatrix.SetPos(nextWorldMatrix.Pos, true);//<<Result대신 Step1의 값을 이용했다.
						//---------------------
					}
				}

				if(isKeyframeAdded_ByIK)
				{
					Editor._portrait.LinkAndRefreshInEditor(false, apUtil.LinkRefresh.Set_AnimClip(targetAnimClip));
					Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, null, null);
					
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}

			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();


			return true;
		}

		private bool OnHotKeyEvent__Animation_Transform__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__Animation_Transform 함수의 코드를 이용

			if (Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip._targetMeshGroup == null
				|| Editor.Select.AnimTimeline == null
				|| Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.IsAnimPlaying)
			{
				return false;
			}
			#region [미사용 코드] 단일 선택 및 처리
			//apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			//apAnimTimelineLayer animTimelineLayer = Editor.Select.AnimTimelineLayer_Main;
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;

			//if(workKeyframe == null)
			//{
			//	//추가 : 5.29
			//	//키프레임이 없을 때 "AutoKey"가 켜져 있다면, 키프레임을 빠르게 만든다.
			//	//Bone IK에 의해 Keyframe이 생성된 것과 달리 이때는 Link를 해야한다. (파라미터 확인)
			//	if(Editor._isAnimAutoKey && animTimelineLayer != null)
			//	{
			//		apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, animTimelineLayer, true, false, false, true);
			//		if(autoCreatedKeyframe != null)
			//		{
			//			Editor.Select.AutoSelectAnimTimelineLayer(true);
			//			workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//		}
			//	}
			//}


			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	return false;
			//}

			//bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			//bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//둘다 해당사항이 없다.
			//	//Debug.LogError("Rotate Failed - " + isTargetTransform + " / " + isTargetBone);
			//	return false;
			//}


			////Undo
			//if(isFirstRotate)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform, Editor, linkedModifier, targetRenderUnit, false);
			//}


			//if (isTargetTransform)
			//{
			//	apMatrix resultMatrix = null;

			//	apMatrix matx_ToParent = null;
			//	//apMatrix matx_LocalModified = null;
			//	apMatrix matx_ParentWorld = null;


			//	if (modMesh._isMeshTransform)
			//	{
			//		if (modMesh._transform_Mesh != null)
			//		{
			//			resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;

			//			matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_Mesh._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		if (modMesh._transform_MeshGroup != null)
			//		{
			//			resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;

			//			matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			//matx_LocalModified = modMesh._transform_MeshGroup._matrix_TF_LocalModified;
			//			matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if (resultMatrix == null)
			//	{
			//		return false;
			//	}


			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW);//각도 변경

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	//변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
			//	float nextLocalAngle = nextLocalModifiedMatrix._angleDeg;
			//	//180각도 제한 옵션 확인후 각도 수정
			//	if(Editor._isAnimRotation180Lock)
			//	{
			//		nextLocalAngle = apUtil.AngleTo180(nextLocalAngle);
			//	}

			//	modMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
			//											nextLocalAngle,
			//											modMesh._transformMatrix._scale);

			//	modMesh.RefreshValues_Check(Editor._portrait);

			//	//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//	if (targetMeshGroup != null)
			//	{
			//		targetMeshGroup.RefreshForce();
			//	}
			//}
			//else if (isTargetBone)
			//{
			//	apBone bone = Editor.Select.Bone;

			//	//Default Angle은 -180 ~ 180 범위 안에 들어간다.
			//	//float nextAngle = bone._defaultMatrix._angleDeg + deltaAngleW;
			//	float nextAngle = modBone._transformMatrix._angleDeg + deltaAngleW;


			//	//변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
			//	if(Editor._isAnimRotation180Lock)
			//	{
			//		nextAngle = apUtil.AngleTo180(nextAngle);
			//	}

			//	modBone._transformMatrix.SetRotate(nextAngle);

			//	if (targetMeshGroup != null)
			//	{
			//		targetMeshGroup.RefreshForce();
			//	}
			//} 
			#endregion


			//변경 20.6.30 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return false;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return false;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return false;
			}




			//Undo
			if(isFirstRotate)
			{	
				if (Editor._isAnimAutoKey)
				{
					//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform, 
																		Editor, 
																		Editor._portrait,
																		targetMeshGroup,
																		linkedModifier, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateTransform,
														Editor, 
														linkedModifier, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly
														);
				}
			}

			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(isFirstRotate && Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return false;
			}


			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//삭제 20.11.2
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
					{
						continue;
					}

					//resultMatrix = null;//삭제 20.11.2
					matx_ToParent = null;
					matx_ParentWorld = null;

					if(curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
					else if(!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}

					//삭제 20.11.2
					//if(resultMatrix == null)
					//{
					//	continue;
					//}

					#region [미사용 코드] 이전 방식
					//if(_tmpNextWorldMatrix == null)
					//{
					//	_tmpNextWorldMatrix = new apMatrix(resultMatrix);
					//}
					//else
					//{
					//	_tmpNextWorldMatrix.SetMatrix(resultMatrix, false);
					//}

					//_tmpNextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW, true);//각도 변경

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					////변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
					//float nextLocalAngle = nextLocalModifiedMatrix._angleDeg;
					////180각도 제한 옵션 확인후 각도 수정
					//if(Editor._isAnimRotation180Lock)
					//{
					//	nextLocalAngle = apUtil.AngleTo180(nextLocalAngle);
					//}

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//									nextLocalAngle,
					//									curModMesh._transformMatrix._scale,
					//									true); 
					#endregion


					//변경 20.11.1 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					//추가 : 각도를 바꿀때, 위치도 보존해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}					

					curModMesh._transformMatrix.MakeMatrix();

					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					//직접 만든 WorldMatrix를 이용해서 회전
					_tmpNextWorldMatrix.SetRotate(_tmpNextWorldMatrix._angleDeg + deltaAngleW, true);

					//위치를 보정해야한다.

					//Mod Matrix로 이동
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, true);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, true);

					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, false);

					//변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
					if (Editor._isAnimRotation180Lock)
					{
						curModMesh._transformMatrix.SetRotate(apUtil.AngleTo180(_tmpNextWorldMatrix._angleDeg), true);//버그 해결!
					}
					else
					{
						//회전 제한이 없다.
						curModMesh._transformMatrix.SetRotate(_tmpNextWorldMatrix._angleDeg, true);//버그 해결!
					}
					


					curModMesh.RefreshValues_Check(Editor._portrait);
				}
			}

			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }


					//추가 : 21.7.3 : 본의 World Matrix가 반전된 상태면 Delta Angle을 뒤집는다.
					float rotateBoneAngleW = deltaAngleW;					
					if(curModBone._bone._worldMatrix.Is1AxisFlipped())
					{
						rotateBoneAngleW = -deltaAngleW;
					}

					//Default Angle은 -180 ~ 180 범위 안에 들어간다.
					float nextAngle = curModBone._transformMatrix._angleDeg + rotateBoneAngleW;


					//변경 20.1.21 : 각도 제한이 "무조건" > "옵션에 따라"로 변경
					if(Editor._isAnimRotation180Lock)
					{
						nextAngle = apUtil.AngleTo180(nextAngle);
					}

					curModBone._transformMatrix.SetRotate(nextAngle, true);
				}
			}


			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();

			return true;
		}

		private bool OnHotKeyEvent__Animation_Transform__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__Animation_Transform 함수의 코드를 이용
			if (Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip._targetMeshGroup == null
				|| Editor.Select.AnimTimeline == null
				|| Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.IsAnimPlaying)
			{
				return false;
			}

			#region [미사용 코드] 단일 선택 및 처리
			//apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			//apAnimTimelineLayer animTimelineLayer = Editor.Select.AnimTimelineLayer_Main;
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;

			//if(workKeyframe == null)
			//{
			//	//추가 : 5.29
			//	//키프레임이 없을 때 "AutoKey"가 켜져 있다면, 키프레임을 빠르게 만든다.
			//	//Bone IK에 의해 Keyframe이 생성된 것과 달리 이때는 Link를 해야한다. (파라미터 확인)
			//	if(Editor._isAnimAutoKey && animTimelineLayer != null)
			//	{
			//		apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, animTimelineLayer, true, false, false, true);
			//		if(autoCreatedKeyframe != null)
			//		{
			//			Editor.Select.AutoSelectAnimTimelineLayer(true);
			//			workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//		}
			//	}
			//}


			//apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone modBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	return false;
			//}

			//bool isTargetTransform = modMesh != null && targetRenderUnit != null && (modMesh._transform_Mesh != null || modMesh._transform_MeshGroup != null);
			//bool isTargetBone = linkedModifier.IsTarget_Bone && modBone != null && Editor.Select.Bone != null;

			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//둘다 해당사항이 없다.
			//	return false;
			//}

			////Undo
			//if (isFirstScale)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform, Editor, linkedModifier, targetRenderUnit, false);
			//}

			//if (isTargetTransform)
			//{
			//	apMatrix resultMatrix = null;
			//	apMatrix matx_ToParent = null;
			//	apMatrix matx_ParentWorld = null;

			//	if (modMesh._isMeshTransform)
			//	{
			//		if (modMesh._transform_Mesh != null)
			//		{
			//			resultMatrix = modMesh._transform_Mesh._matrix_TFResult_World;
			//			matx_ToParent = modMesh._transform_Mesh._matrix_TF_ToParent;
			//			matx_ParentWorld = modMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		if (modMesh._transform_MeshGroup != null)
			//		{
			//			resultMatrix = modMesh._transform_MeshGroup._matrix_TFResult_World;
			//			matx_ToParent = modMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			matx_ParentWorld = modMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if (resultMatrix == null)
			//	{
			//		return false;
			//	}

			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleL);

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	modMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
			//										nextLocalModifiedMatrix._angleDeg,
			//										nextLocalModifiedMatrix._scale);

			//	modMesh.RefreshValues_Check(Editor._portrait);

			//	//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//	if (targetMeshGroup != null)
			//	{
			//		//targetMeshGroup.AddForceUpdateTarget(modMesh._renderUnit);
			//		targetMeshGroup.RefreshForce();
			//	}
			//}
			//else if (isTargetBone)
			//{
			//	apBone bone = Editor.Select.Bone;

			//	Vector3 prevScale = modBone._transformMatrix._scale;
			//	Vector2 nextScale = new Vector2(prevScale.x + deltaScaleL.x, prevScale.y + deltaScaleL.y);

			//	modBone._transformMatrix.SetScale(nextScale);

			//	if (targetMeshGroup != null)
			//	{
			//		targetMeshGroup.RefreshForce();
			//	}
			//} 
			#endregion



			//변경 20.7.1 : 다중 처리
			//여러개의 레이어 및 키프레임들을 대상으로 편집해야한다.
			//공통으로 유지해야하는건 Timeline이면 된다.
			//타임라인 레이어를 순회하면서 옵션에 따라 키프레임을 자동으로 생성할 수도 있다.
			apAnimTimeline animTimeline = Editor.Select.AnimTimeline;
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			apAnimClip targetAnimClip = Editor.Select.AnimClip;
			int curFrame = Editor.Select.AnimClip.CurFrame;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = timelineLayers != null ? timelineLayers.Count : 0;
			if(nTimelineLayers == 0)
			{
				return false;
			}

			//일단 WorkKeyframe들을 가져오자
			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			if(workKeyframes == null)
			{
				return false;
			}

			if (linkedModifier == null)
			{
				//수정할 타겟이 없다.
				return false;
			}

			//Undo
			if(isFirstScale)
			{	
				if (Editor._isAnimAutoKey)
				{
					//AutoKey 옵션이 켜졌다면 Keyframe이 생성된 후 복구할 수 있게 Undo 범위를 크게 잡아야 한다.
					apEditorUtil.SetRecord_PortraitMeshGroupModifier(	apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform, 
																		Editor, 
																		Editor._portrait,
																		targetMeshGroup,
																		linkedModifier, 
																		//null, 
																		false,
																		apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
				else
				{
					apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_ScaleTransform,
														Editor, 
														linkedModifier, 
														//null, 
														false,
														apEditorUtil.UNDO_STRUCT.ValueOnly);
				}
			}

			apAnimTimelineLayer curLayer = null;

			//자동으로 키프레임을 생성할 수 있는지 체크하자
			//단, isFirstMove인 경우에만
			if(isFirstScale && Editor._isAnimAutoKey)
			{	
				bool isAnyKeyframeAdded = false;
				for (int iLayer = 0; iLayer < nTimelineLayers; iLayer++)
				{
					curLayer = timelineLayers[iLayer];
					bool isWorkKeyfameExist = workKeyframes.Exists(delegate(apAnimKeyframe a)
					{
						return a._parentTimelineLayer == curLayer;
					});

					if(isWorkKeyfameExist)
					{
						//이미 현재 레이어에 대해 WorkKeyframe이 생성되어 있다면 오케이
						continue;
					}

					//이 타임라인 레이어에 대해 키프레임이 없다면 생성하자 (Undo 없음)
					apAnimKeyframe autoCreatedKeyframe = Editor.Controller.AddAnimKeyframe(	curFrame, 
																							curLayer, 
																							true, false, false, true);
					if(autoCreatedKeyframe != null)
					{
						isAnyKeyframeAdded = true;
						//Editor.Select.AutoSelectAnimTimelineLayer(false);
					}
				}
				if(isAnyKeyframeAdded)
				{
					//WorkKeyframe 선택 및 ModData 동기화
					bool isWorkKeyframeChanged = false;
					Editor.Select.AutoSelectAnimWorkKeyframe(out isWorkKeyframeChanged);

					if(isWorkKeyframeChanged && Editor.Gizmos.IsFFDMode)
					{
						//여기서는 FFD를 강제로 해제하자.
						Editor.Gizmos.RevertFFDTransformForce();
					}
				}
			}


			//이제 ModMesh/ModBone들을 선택해서 편집하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && linkedModifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return false;
			}


			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//삭제 20.11.2
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
					{
						continue;
					}

					//resultMatrix = null;//삭제 20.11.2
					matx_ToParent = null;
					matx_ParentWorld = null;

					if (curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;
					}
					else if (!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.11.2
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
					}


					//삭제 20.11.2
					//if (resultMatrix == null)
					//{
					//	continue;
					//}

					#region [미사용 코드] 이전 방식
					//if(_tmpNextWorldMatrix == null)
					//{
					//	_tmpNextWorldMatrix = new apMatrix(resultMatrix);
					//}
					//else
					//{
					//	_tmpNextWorldMatrix.SetMatrix(resultMatrix, false);
					//}
					//_tmpNextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleL, true);

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//									nextLocalModifiedMatrix._angleDeg,
					//									nextLocalModifiedMatrix._scale,
					//									true); 
					#endregion


					//변경 20.11.1 : localModified Matrix는 apMatrixCal의 CalculateLocalPos_ModMesh 함수로 인해서 값이 변하기 때문에 사용 불가
					//따라서 WorldMatrix를 직접 계산해야한다.
					//추가 : 크기를 바꿀때, 위치도 보존해야한다.
					if (_tmpNextWorldMatrix == null)
					{
						_tmpNextWorldMatrix = new apMatrix(matx_ToParent);
					}
					else
					{
						_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					}

					curModMesh._transformMatrix.MakeMatrix();

					//Scale은 World를 수정하는게 아니라 PosW를 기록한 상태로 Local Scale를 수정하여 Pos를 보정하자
					//1. 현재의 PosW를 계산하기

					_tmpNextWorldMatrix.OnBeforeRMultiply();
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					Vector2 posW_Prev = _tmpNextWorldMatrix._pos;

					//2. Modified의 Scale 수정하기
					curModMesh._transformMatrix.SetScale(curModMesh._transformMatrix._scale + deltaScaleL, true);

					//3. 변경된 Scale로 World Matrix 다시 계산하기
					_tmpNextWorldMatrix.SetMatrix(matx_ToParent, true);
					_tmpNextWorldMatrix.OnBeforeRMultiply();
					
					_tmpNextWorldMatrix.RMultiply(curModMesh._transformMatrix, false);
					_tmpNextWorldMatrix.RMultiply(matx_ParentWorld, true);

					//4. 저장된 "이전 PosW"를 대입
					_tmpNextWorldMatrix._pos = posW_Prev;
					//_tmpNextWorldMatrix._angleDeg = angleW_Prev;
					_tmpNextWorldMatrix.MakeMatrix();

					//5. 이제 Scale에 따른 Local 위치 변화가 발생했을 것이므로, Pos를 보정한다.
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, true);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, true);

					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, true);

					//코멘트 20.11.2 : 이렇게 하면 Default Matrix에 회전값이 포함된 경우 축이 뒤틀리는 문제가 발생한다.
					//심지어 편집이 힘들 수 있는데, 해결법을 모르겠다.


					curModMesh.RefreshValues_Check(Editor._portrait);
				}
			}


			//2. ModBone 수정
			if (nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;

				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }

					bone = curModBone._bone;
					if (bone == null) { continue; }

					Vector3 prevScale = curModBone._transformMatrix._scale;
					Vector2 nextScale = new Vector2(prevScale.x + deltaScaleL.x, prevScale.y + deltaScaleL.y);

					curModBone._transformMatrix.SetScale(nextScale, true);
				}
			}


			//전체 갱신하자
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();


			return true;
		}





		//Vertex Move/Rotate/Scale
		private bool OnHotKeyEvent__Animation_Vertex__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			//Move__Animation_Vertex 함수의 코드를 이용

			if (Editor.Select.AnimClip == null 
				|| Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}


			//변경 20.6.30 : 다중 처리
			//여러개의 ModMesh를 돌아다니면서 처리를 한다.
			//메인 WorkKeyframe이 없어도 동작할 수 있게 만든다.
			//자동으로 키가 생성되지는 않는다. (애초에 WorkKeyframe이 없으면 버텍스를 선택할 수가 없다..)

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = (timelineLayers != null) ? timelineLayers.Count : 0;

			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes != null) ? workKeyframes.Count : 0;
			
			if (linkedModifier == null 
				|| nTimelineLayers == 0
				|| nWorkKeyframes == 0)
			{
				//수정할 타겟이 없다.
				return false;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return false;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count == 0)
				{
					return false;
				}
			}

			//Undo
			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_MoveVertex, 
													Editor, 
													linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;


				#region [미사용 코드] 1개, 2개 이상 코드 분리된 상태
				//if (nMRV == 1)
				//{
				//	//1. 단일 선택일 때
				//	apRenderVertex renderVert = Editor.Select.ModRenderVert_Main._renderVert;
				//	renderVert.Calculate();

				//	Vector2 prevDeltaPos2 = Editor.Select.ModRenderVert_Main._modVert._deltaPos;

				//	apMatrix3x3 martrixMorph = apMatrix3x3.TRS(prevDeltaPos2, 0, Vector2.one);

				//	//이전
				//	//Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform * martrixMorph * renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

				//	//변경 21.4.5 : 리깅 적용 (다중 모드 처리시 리깅이 추가될 수 있기 때문)
				//	Vector2 prevWorldPos2 = (renderVert._matrix_Cal_VertWorld
				//								* renderVert._matrix_MeshTransform
				//								* renderVert._matrix_Rigging
				//								* martrixMorph
				//								* renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

				//	Vector2 nextWorldPos = prevWorldPos2 + deltaMoveW;

				//	Vector2 noneMorphedPosM = (renderVert._matrix_Static_Vert2Mesh).MultiplyPoint(renderVert._vertex._pos);

				//	//이전
				//	//Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld * renderVert._matrix_MeshTransform).inverse).MultiplyPoint(nextWorldPos);

				//	//변경 21.4.5 : 리깅 적용 (다중 모드 처리시 리깅이 추가될 수 있기 때문)
				//	Vector2 nextMorphedPosM = ((renderVert._matrix_Cal_VertWorld
				//									* renderVert._matrix_MeshTransform
				//									* renderVert._matrix_Rigging).inverse).MultiplyPoint(nextWorldPos);

				//	Editor.Select.ModRenderVert_Main._modVert._deltaPos.x = (nextMorphedPosM.x - noneMorphedPosM.x);
				//	Editor.Select.ModRenderVert_Main._modVert._deltaPos.y = (nextMorphedPosM.y - noneMorphedPosM.y);
				//}
				//else
				//{
				//	//2. 복수개 선택일 때
				//	//for (int i = 0; i < Editor.Select.ModRenderVertListOfAnim.Count; i++)
				//	for (int i = 0; i < nMRV; i++)
				//	{
				//		apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];
				//		apRenderVertex renderVert = modRenderVert._renderVert;
				//		Vector2 nextWorldPos = renderVert._pos_World + deltaMoveW;

				//		modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
				//	}
				//} 
				#endregion

				if (nMRV > 0)
				{
					//2. 복수개 선택일 때
					for (int i = 0; i < nMRV; i++)
					{
						curMRV = modRenderVerts[i];
						Vector2 nextWorldPos = curMRV._renderVert._pos_World + deltaMoveW;
						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
					int nWeightedMRV = weightedMRVs.Count;

					for (int i = 0; i < nWeightedMRV; i++)
					{
						curMRV = weightedMRVs[i];
						float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos = (curMRV._renderVert._pos_World + deltaMoveW) * weight + (curMRV._renderVert._pos_World) * (1.0f - weight);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]

				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				if (nMRP > 0)
				{
					//2. 복수개 선택일 때
					for (int i = 0; i < nMRP; i++)
					{
						curMRP = modRenderPins[i];
						Vector2 nextWorldPos = curMRP._renderPin._pos_World + deltaMoveW;
						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRP = weightedMRPs.Count;

					for (int i = 0; i < nWeightedMRP; i++)
					{
						curMRP = weightedMRPs[i];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos = (curMRP._renderPin._pos_World + deltaMoveW) * weight + (curMRP._renderPin._pos_World) * (1.0f - weight);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
			}


			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();

			return true;
		}

		private bool OnHotKeyEvent__Animation_Vertex__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__Animation_Vertex 함수의 코드를 이용

			if (Editor.Select.AnimClip == null 
				|| Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			#region [미사용 코드] 단일 선택 및 처리
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

			//if (linkedModifier == null ||
			//	workKeyframe == null ||
			//	targetModMesh == null ||
			//	targetRenderUnit == null ||
			//	Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
			//	Editor.Select.IsAnimPlaying ||
			//	Editor.Select.ModRenderVert_Main == null ||
			//	Editor.Select.ModRenderVerts_All == null)
			//{
			//	//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
			//	return false;
			//}


			////Vertex 계열에서 회전/크기수정하고자 하는 경우 -> 다중 선택에서만 가능
			//if (Editor.Select.ModRenderVerts_All.Count <= 1)
			//{
			//	//단일 선택인 경우는 패스
			//	return false;
			//}

			//Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPos;

			////Gizmo의 +-180도 이내 제한.... 일단 빼보자
			////if(deltaAngleW > 180.0f) { deltaAngleW -= 360.0f; }
			////else if(deltaAngleW < -180.0f) { deltaAngleW += 360.0f; }

			////Quaternion quat = Quaternion.Euler(0.0f, 0.0f, deltaAngleW);

			//apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
			//	* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
			//	* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

			////Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0]._renderVert;
			//	isMultipleVerts = false;
			//}
			//if (isFirstRotate)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_RotateVertex, Editor, linkedModifier, targetVert, isMultipleVerts);
			//}


			//for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			//{
			//	apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];
			//	Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);

			//	modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos2);
			//}


			////Soft Selection 상태일때
			//if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			//{
			//	for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
			//	{
			//		apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
			//		float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

			//		apRenderVertex renderVert = modRenderVert._renderVert;

			//		//Weight를 적용한 만큼만 움직이자
			//		Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(modRenderVert._renderVert._pos_World);
			//		Vector2 nextWorldPos = (nextWorldPos2) * weight + (renderVert._pos_World) * (1.0f - weight);

			//		modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			//	}
			//}
			////변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			//if (targetMeshGroup != null)
			//{
			//	targetMeshGroup.RefreshForce();
			//}

			//Editor.SetRepaint(); 
			#endregion


			//변경 20.6.30 : 다중 처리
			//여러개의 ModMesh를 돌아다니면서 처리를 한다.
			//메인 WorkKeyframe이 없어도 동작할 수 있게 만든다.
			//자동으로 키가 생성되지는 않는다. (애초에 WorkKeyframe이 없으면 버텍스를 선택할 수가 없다..)
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = (timelineLayers != null) ? timelineLayers.Count : 0;

			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes != null) ? workKeyframes.Count : 0;
			
			if (linkedModifier == null 
				|| nTimelineLayers == 0
				|| nWorkKeyframes == 0)
			{
				//수정할 타겟이 없다.
				return false;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return false;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count == 0)
				{
					return false;
				}
			}

			//Undo
			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_RotateVertex, 
													Editor, linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}



			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

				Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPos;

				apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
					* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
					* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRV > 1)
				{
					//기본 회전은 MRV가 2개 이상일때부터

					for (int i = 0; i < nMRV; i++)
					{
						curMRV = modRenderVerts[i];
						Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(curMRV._renderVert._pos_World);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos2);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
					int nWeightedMRVs = weightedMRVs.Count;

					for (int i = 0; i < nWeightedMRVs; i++)
					{
						curMRV = weightedMRVs[i];
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

				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				Vector2 centerPos2 = Editor.Select.ModRenderPinsCenterPos;

				apMatrix3x3 matrix_Rotate = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, deltaAngleW, Vector2.one)
											* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRP > 1)
				{
					//기본 회전은 MRP가 2개 이상일때부터
					for (int i = 0; i < nMRP; i++)
					{
						curMRP = modRenderPins[i];
						Vector2 nextWorldPos2 = matrix_Rotate.MultiplyPoint(curMRP._renderPin._pos_World);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos2);
					}
				}

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRPs = weightedMRPs.Count;

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
			

			//업데이트
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();



			return true;
		}

		private bool OnHotKeyEvent__Animation_Vertex__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__Animation_Vertex 함수의 코드를 이용
			if (Editor.Select.AnimClip == null 
				|| Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			#region [미사용 코드] 단일 선택 및 처리
			//apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

			//if (linkedModifier == null ||
			//	workKeyframe == null ||
			//	targetModMesh == null ||
			//	targetRenderUnit == null ||
			//	Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
			//	Editor.Select.IsAnimPlaying ||
			//	Editor.Select.ModRenderVert_Main == null ||
			//	Editor.Select.ModRenderVerts_All == null)
			//{
			//	//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
			//	return false;
			//}

			////Vertex 계열에서 회전/크기수정하고자 하는 경우 -> 다중 선택에서만 가능
			//if (Editor.Select.ModRenderVerts_All.Count <= 1)
			//{
			//	//단일 선택인 경우는 패스
			//	return false;
			//}

			//Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPos;
			////Vector3 centerPos3 = new Vector3(centerPos2.x, centerPos2.y, 0);

			//Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

			//apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
			//	* apMatrix3x3.TRS(Vector2.zero, 0, scale)
			//	* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

			////Undo
			//bool isMultipleVerts = true;
			//object targetVert = null;
			//if (Editor.Select.ModRenderVerts_All.Count == 1 && !Editor.Gizmos.IsSoftSelectionMode)
			//{
			//	targetVert = Editor.Select.ModRenderVerts_All[0]._renderVert;
			//	isMultipleVerts = false;
			//}
			//if (isFirstScale)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleVertex, Editor, linkedModifier, targetVert, isMultipleVerts);
			//}



			//for (int i = 0; i < Editor.Select.ModRenderVerts_All.Count; i++)
			//{
			//	apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_All[i];

			//	Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);

			//	modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos2);
			//}

			////Soft Selection 상태일때
			//if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
			//{
			//	for (int i = 0; i < Editor.Select.ModRenderVerts_Weighted.Count; i++)
			//	{
			//		apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVerts_Weighted[i];
			//		float weight = Mathf.Clamp01(modRenderVert._vertWeightByTool);

			//		apRenderVertex renderVert = modRenderVert._renderVert;

			//		//Weight를 적용한 만큼만 움직이자
			//		Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(modRenderVert._renderVert._pos_World);
			//		Vector2 nextWorldPos = nextWorldPos2 * weight + (renderVert._pos_World) * (1.0f - weight);

			//		modRenderVert.SetWorldPosToModifier_VertLocal(nextWorldPos);
			//	}
			//}

			////이전 코드 : 전체 Refresh
			////Editor.Select.AnimClip.UpdateMeshGroup_Editor(true, 0.0f, true);

			////변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			//apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;
			//if (targetMeshGroup != null)
			//{
			//	//targetMeshGroup.AddForceUpdateTarget(targetModMesh._renderUnit);
			//	targetMeshGroup.RefreshForce();
			//}
			//Editor.SetRepaint(); 
			#endregion


			//변경 20.6.30 : 다중 처리
			//여러개의 ModMesh를 돌아다니면서 처리를 한다.
			//메인 WorkKeyframe이 없어도 동작할 수 있게 만든다.
			//자동으로 키가 생성되지는 않는다. (애초에 WorkKeyframe이 없으면 버텍스를 선택할 수가 없다..)
			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			List<apAnimTimelineLayer> timelineLayers = Editor.Select.AnimTimelineLayers_All;
			int nTimelineLayers = (timelineLayers != null) ? timelineLayers.Count : 0;

			List<apAnimKeyframe> workKeyframes = Editor.Select.AnimWorkKeyframes_All;
			int nWorkKeyframes = (workKeyframes != null) ? workKeyframes.Count : 0;
			
			if (linkedModifier == null 
				|| nTimelineLayers == 0
				|| nWorkKeyframes == 0)
			{
				//수정할 타겟이 없다.
				return false;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return false;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count == 0)
				{
					return false;
				}
			}

			//Undo
			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Anim_Gizmo_ScaleVertex,
													Editor, linkedModifier,
													//null,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

				Vector2 centerPos2 = Editor.Select.ModRenderVertsCenterPos;

				Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

				apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, 0, scale)
											* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRV > 1)
				{
					for (int i = 0; i < nMRV; i++)
					{
						curMRV = modRenderVerts[i];

						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRV._renderVert._pos_World);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos2);
					}
				}
				

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderVerts_Weighted.Count > 0)
				{
					List<apSelection.ModRenderVert> weightedMRVs = Editor.Select.ModRenderVerts_Weighted;
					int nWeightedMRVs = weightedMRVs.Count;

					for (int i = 0; i < nWeightedMRVs; i++)
					{
						curMRV = weightedMRVs[i];
						float weight = Mathf.Clamp01(curMRV._vertWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRV._renderVert._pos_World);
						Vector2 nextWorldPos = nextWorldPos2 * weight + (curMRV._renderVert._pos_World) * (1.0f - weight);

						curMRV.SetWorldPosToModifier_VertLocal(nextWorldPos);
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				Vector2 centerPos2 = Editor.Select.ModRenderPinsCenterPos;

				Vector2 scale = new Vector2(1.0f + deltaScaleL.x, 1.0f + deltaScaleL.y);

				apMatrix3x3 matrix_Scale = apMatrix3x3.TRS(centerPos2, 0, Vector2.one)
											* apMatrix3x3.TRS(Vector2.zero, 0, scale)
											* apMatrix3x3.TRS(-centerPos2, 0, Vector2.one);

				if (nMRP > 1)
				{
					for (int i = 0; i < nMRP; i++)
					{
						curMRP = modRenderPins[i];

						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRP._renderPin._pos_World);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos2);
					}
				}
				

				//Soft Selection 상태일때
				if (Editor.Gizmos.IsSoftSelectionMode && Editor.Select.ModRenderPins_Weighted.Count > 0)
				{
					List<apSelection.ModRenderPin> weightedMRPs = Editor.Select.ModRenderPins_Weighted;
					int nWeightedMRPs = weightedMRPs.Count;

					for (int i = 0; i < nWeightedMRPs; i++)
					{
						curMRP = weightedMRPs[i];
						float weight = Mathf.Clamp01(curMRP._pinWeightByTool);

						//Weight를 적용한 만큼만 움직이자
						Vector2 nextWorldPos2 = matrix_Scale.MultiplyPoint(curMRP._renderPin._pos_World);
						Vector2 nextWorldPos = nextWorldPos2 * weight + (curMRP._renderPin._pos_World) * (1.0f - weight);

						curMRP.SetWorldToModifier_PinLocal(nextWorldPos);
					}
				}
			}

			//업데이트
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();


			return true;
		}


		//----------------------------------------------------------------------------------------------
		// 다중 선택 / FFD 제어 이벤트들 (Vertex / Bone만 가능하다)
		//----------------------------------------------------------------------------------------------
		public apGizmos.SelectResult MultipleSelect__Animation_Vertex(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			if (Editor.Select.AnimTimeline == null ||
				Editor.Select.AnimTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier ||
				Editor.Select.AnimTimeline._linkedModifier == null
				)
			{
				//Debug.LogError("실패 1");
				//return 0;
				return null;
			}

			if ((int)(Editor.Select.AnimTimeline._linkedModifier.CalculatedValueType & apCalculatedResultParam.CALCULATED_VALUE_TYPE.VertexPos) == 0)
			{
				//VertexPos 계열 Modifier가 아니다.
				//Debug.LogError("실패 2");
				//return 0;
				return null;
			}

			
			//변경 20.7.1 : 다중 선택
			int nWorkKeyframes = (Editor.Select.AnimWorkKeyframes_All != null) ? (Editor.Select.AnimWorkKeyframes_All.Count) : 0;
			int nModMesh = (Editor.Select.ModMeshes_All != null) ? (Editor.Select.ModMeshes_All.Count) : 0;
			
			if(nWorkKeyframes == 0 || nModMesh == 0)
			{
				return null;
			}



			bool isAnyChanged = false;

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]

				//변경 20.6.29 : 선택 가능한 MRV가 이미 만들어져 있으니 그걸 사용하자
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModData.ModRenderVert_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;

				if (nMRVs > 0)
				{
					apSelection.ModRenderVert curMRV = null;
					for (int iMRV = 0; iMRV < nMRVs; iMRV++)
					{
						curMRV = modRenderVerts[iMRV];

						if (curMRV == null || curMRV._renderVert == null)
						{
							continue;
						}

						bool isSelectable = (mousePosW_Min.x < curMRV._renderVert._pos_World.x && curMRV._renderVert._pos_World.x < mousePosW_Max.x)
										&& (mousePosW_Min.y < curMRV._renderVert._pos_World.y && curMRV._renderVert._pos_World.y < mousePosW_Max.y);
						if (isSelectable)
						{
							if (areaSelectType == apGizmos.SELECT_TYPE.Add || areaSelectType == apGizmos.SELECT_TYPE.New)
							{
								//추가한다.
								Editor.Select.AddModRenderVert_ForAnimEdit(curMRV);
							}
							else
							{
								//제외한다.
								Editor.Select.RemoveModRenderVert_ForAnimEdit(curMRV);
							}

							isAnyChanged = true;//<<뭔가 선택에 변동이 생겼다.
						}
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModData.ModRenderPin_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

				if (nMRPs > 0)
				{
					apSelection.ModRenderPin curMRP = null;
					for (int iMRP = 0; iMRP < nMRPs; iMRP++)
					{
						curMRP = modRenderPins[iMRP];

						if (curMRP == null || curMRP._renderPin == null)
						{
							continue;
						}

						bool isSelectable = (mousePosW_Min.x < curMRP._renderPin._pos_World.x && curMRP._renderPin._pos_World.x < mousePosW_Max.x)
										&& (mousePosW_Min.y < curMRP._renderPin._pos_World.y && curMRP._renderPin._pos_World.y < mousePosW_Max.y);
						if (isSelectable)
						{
							if (areaSelectType == apGizmos.SELECT_TYPE.Add || areaSelectType == apGizmos.SELECT_TYPE.New)
							{
								//추가한다.
								Editor.Select.AddModRenderPin_ForAnimEdit(curMRP);
							}
							else
							{
								//제외한다.
								Editor.Select.RemoveModRenderPin_ForAnimEdit(curMRP);
							}

							isAnyChanged = true;//<<뭔가 선택에 변동이 생겼다.
						}
					}
				}
			}

			

			if (isAnyChanged)
			{
				Editor.RefreshControllerAndHierarchy(false);
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.None, null, null);
				Editor.SetRepaint();
			}


			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVerts_All == null)
				{
					return null;
				}
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVerts_All);
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPins_All == null)
				{
					return null;
				}
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderPin>(Editor.Select.ModRenderPins_All);
			}
		}



		


		// FFD
		//-----------------------------------------------------------------------------

		public bool FFDTransform__Animation_Vertex(List<object> srcObjects, List<Vector2> posData, apGizmos.FFD_ASSIGN_TYPE assignType, bool isResultAssign, bool isRecord)
		{
			//결과 적용이 아닌 일반 수정 작업시
			//-> 수정이 불가능한 경우에는 불가하다고 리턴한다.
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;

			if (!isResultAssign)
			{
				
				//이전
				//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
				//apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
				//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

				//변경 20.7.1 : 다중 선택
				int nWorkKeyframes = (Editor.Select.AnimWorkKeyframes_All != null) ? (Editor.Select.AnimWorkKeyframes_All.Count) : 0;
				int nModMesh = (Editor.Select.ModMeshes_All != null) ? (Editor.Select.ModMeshes_All.Count) : 0;

				if (linkedModifier == null ||
					
					//이전
					//workKeyframe == null ||
					//targetModMesh == null ||
					//targetRenderUnit == null ||

					//변경 20.7.1 : 다중 선택 지원
					nWorkKeyframes == 0 ||
					nModMesh == 0 ||

					Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
					Editor.Select.IsAnimPlaying)
				{
					//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
					return false;
				}

				if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
				{
					// [ 버텍스 편집 모드 ]
					if(Editor.Select.ModRenderVert_Main == null || Editor.Select.ModRenderVerts_All == null)
					{
						return false;
					}

					//Vertex 계열에서 FFD 수정시에는 단일 선택인 경우는 패스
					if (Editor.Select.ModRenderVerts_All.Count <= 1)
					{
						return false;
					}
				}
				else
				{
					// [ 핀 편집 모드 ]
					if(Editor.Select.ModRenderPin_Main == null || Editor.Select.ModRenderPins_All == null)
					{
						return false;
					}

					//Vertex 계열에서 FFD 수정시에는 단일 선택인 경우는 패스
					if (Editor.Select.ModRenderPins_All.Count <= 1)
					{
						return false;
					}
				}
			}

			apMeshGroup targetMeshGroup = Editor.Select.AnimClip._targetMeshGroup;

			if (isRecord)//추가 20.7.22 : 요청에 의해서만 Record를 하자
			{
				//Undo
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_FFDVertex, 
													Editor, linkedModifier, 
													//null, 
													true,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				apSelection.ModRenderVert curMRV = null;

				if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
				{
					//World Pos를 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						curMRV = srcObjects[i] as apSelection.ModRenderVert;
						if (curMRV == null)
						{
							continue;
						}
						curMRV.SetWorldPosToModifier_VertLocal(posData[i]);//WorldPos로서 값을 대입한다.
					}
				}
				else//if(assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
				{
					//저장했던 데이터를 그대로 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						curMRV = srcObjects[i] as apSelection.ModRenderVert;
						if (curMRV == null)
						{
							continue;
						}
						curMRV._modVert._deltaPos = posData[i];//ModVert에 직접 대입한다.
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				apSelection.ModRenderPin curMRP = null;

				if (assignType == apGizmos.FFD_ASSIGN_TYPE.WorldPos)
				{
					//World Pos를 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						curMRP = srcObjects[i] as apSelection.ModRenderPin;
						if (curMRP == null)
						{
							continue;
						}
						curMRP.SetWorldToModifier_PinLocal(posData[i]);//WorldPos로서 값을 대입한다.
					}
				}
				else//if(assignType == apGizmos.FFD_ASSIGN_TYPE.LocalData)
				{
					//저장했던 데이터를 그대로 대입하는 경우
					for (int i = 0; i < srcObjects.Count; i++)
					{
						curMRP = srcObjects[i] as apSelection.ModRenderPin;
						if (curMRP == null)
						{
							continue;
						}
						curMRP._modPin._deltaPos = posData[i];//ModVert에 직접 대입한다.
					}
				}
			}
			
			//변경 : 일부만 강제 Refresh하고 나머지는 정상 Update 하도록 지시
			targetMeshGroup.RefreshForce(false, 0.0f, apUtil.LinkRefresh.Set_AnimClip(Editor.Select.AnimClip));
			Editor.SetRepaint();

			return true;
		}




		public bool StartFFDTransform__Animation_Vertex()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;

			//이전
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

			//변경 20.7.1 : 다중 선택
			int nWorkKeyframes = (Editor.Select.AnimWorkKeyframes_All != null) ? (Editor.Select.AnimWorkKeyframes_All.Count) : 0;
			int nModMesh = (Editor.Select.ModMeshes_All != null) ? (Editor.Select.ModMeshes_All.Count) : 0;

			if (linkedModifier == null ||
				
				//이전
				//workKeyframe == null ||
				//targetModMesh == null ||
				//targetRenderUnit == null ||

				//변경 20.7.1 : 다중 선택 지원
				nWorkKeyframes == 0 ||
				nModMesh == 0 ||

				Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None ||
				Editor.Select.IsAnimPlaying
				)
			{
				//수정할 타겟이 없다. + 에디팅 가능한 상태가 아니다 + 선택한 Vertex가 없다.
				return false;
			}

			//편집 대상에 따라 체크
			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if(Editor.Select.ModRenderVert_Main == null || Editor.Select.ModRenderVerts_All == null)
				{
					return false;
				}

				//Vertex 계열에서 FFD 수정시에는 단일 선택인 경우는 패스
				if (Editor.Select.ModRenderVerts_All.Count <= 1)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if(Editor.Select.ModRenderPin_Main == null || Editor.Select.ModRenderPins_All == null)
				{
					return false;
				}

				//Vertex 계열에서 FFD 수정시에는 단일 선택인 경우는 패스
				if (Editor.Select.ModRenderPins_All.Count <= 1)
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
				int nMRV = (modRenderVerts != null) ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

				for (int i = 0; i < nMRV; i++)
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
				int nMRP = (modRenderPins != null) ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

				for (int i = 0; i < nMRP; i++)
				{
					curMRP = modRenderPins[i];
					srcObjectList.Add(curMRP);
					worldPosList.Add(curMRP._renderPin._pos_World);
					modDataList.Add(curMRP._modPin._deltaPos);//<<추가 20.7.22
				}
			}
			Editor.Gizmos.RegistTransformedObjectList(srcObjectList, worldPosList, modDataList);

			return true;
		}



		public bool SoftSelection_Animation_Vertex()
		{
			Editor.Select.ModRenderVerts_Weighted.Clear();
			Editor.Select.ModRenderPins_Weighted.Clear();

			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return false;
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count == 0)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if (Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count == 0)
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
				int nSelectedMRVs = selectedMRVs != null ? selectedMRVs.Count : 0;

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

						for (int iSelectedRV = 0; iSelectedRV < nSelectedMRVs; iSelectedRV++)
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

						//선택된 RenderPin은 제외한다.
						if (selectedMRPs.Contains(curMRP))
						{
							continue;
						}

						//가장 가까운 선택된 RenderPin를 찾는다.
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

		//Blur의 크기와 강도를 리턴하는 함수 (19.7.29 추가)
		public apGizmos.BrushInfo SyncBlurStatus_Animation_Vertex(bool isEnded)
		{
			if(isEnded)
			{
				Editor._blurEnabled = false;
			}
			apGizmos.BRUSH_COLOR_MODE colorMode = apGizmos.BRUSH_COLOR_MODE.Default;
			if(Editor._blurIntensity == 0)		{ colorMode = apGizmos.BRUSH_COLOR_MODE.Default; }
			else if(Editor._blurIntensity < 33)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv1; }
			else if(Editor._blurIntensity < 66)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv2; }
			else								{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv3; }

			//return apGizmos.BrushInfo.MakeInfo(Editor._blurRadius, Editor._blurIntensity, colorMode, null);//이전
			return apGizmos.BrushInfo.MakeInfo(Editor._blurRadiusIndex, Editor._blurIntensity, colorMode, null);//변경 22.1.9 : 인덱스 이용
		}



		public bool PressBlur_Animation_Vertex(Vector2 pos, float tDelta, bool isFirstBlur)
		{
			if (Editor.Select.AnimClip == null 
				|| Editor.Select.AnimClip._targetMeshGroup == null 
				|| Editor.Select.AnimTimeline == null)
			{
				return false;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			if(linkedModifier == null)
			{
				return false;
			}

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				if (Editor.Select.ModRenderVerts_All == null || Editor.Select.ModRenderVerts_All.Count <= 1)
				{
					return false;
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				if (Editor.Select.ModRenderPins_All == null || Editor.Select.ModRenderPins_All.Count <= 1)
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

			_tmpBlurVertices.Clear();
			_tmpBlurPins.Clear();
			_tmpBlurWeights.Clear();

			Vector2 totalModValue = Vector2.zero;
			float totalWeight = 0.0f;

			if (isFirstBlur)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Anim_Gizmo_BlurVertex, 
													Editor, 
													linkedModifier, 
													//null, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			//1. 영역 안의 Vertex를 선택하자 + 마우스 중심점을 기준으로 Weight를 구하자 + ModValue의 가중치가 포함된 평균을 구하자
			//2. 가중치가 포함된 평균값만큼 tDelta * intensity * weight로 바꾸어주자


			//영역 체크는 GL값
			float dist = 0.0f;
			float weight = 0.0f;

			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				//선택된 Vert에 한해서만 처리하자
				List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVerts_All;
				int nMRVs = modRenderVerts != null ? modRenderVerts.Count : 0;

				apSelection.ModRenderVert curMRV = null;

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
					totalModValue /= totalWeight;

					for (int i = 0; i < _tmpBlurVertices.Count; i++)
					{
						//0이면 유지, 1이면 변경
						float itp = Mathf.Clamp01(_tmpBlurWeights[i] * tDelta * intensity);

						_tmpBlurVertices[i]._deltaPos = _tmpBlurVertices[i]._deltaPos * (1.0f - itp) + totalModValue * itp;
					}
				}
			}
			else
			{
				// [ 핀 편집 모드 ]
				//선택된 Pin에 한해서만 처리하자
				List<apSelection.ModRenderPin> modRenderPins = Editor.Select.ModRenderPins_All;
				int nMRPs = modRenderPins != null ? modRenderPins.Count : 0;

				apSelection.ModRenderPin curMRP = null;

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
					totalModValue /= totalWeight;

					for (int i = 0; i < _tmpBlurPins.Count; i++)
					{
						//0이면 유지, 1이면 변경
						float itp = Mathf.Clamp01(_tmpBlurWeights[i] * tDelta * intensity);

						_tmpBlurPins[i]._deltaPos = _tmpBlurPins[i]._deltaPos * (1.0f - itp) + totalModValue * itp;
					}
				}
			}
			

			Editor.Select.AnimClip._targetMeshGroup.RefreshForce();

			return true;
		}


		// Pivot Return 이벤트들
		//----------------------------------------------------------------------------------------------
		public apGizmos.TransformParam PivotReturn__Animation_OnlySelect()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			//? 없나

			return null;
		}


		public apGizmos.TransformParam PivotReturn__Animation_Transform()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null || Editor.Select.AnimTimeline == null)
			{
				return null;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return null;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;

			

			//이전
			//apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			//apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			//apModifiedBone targetModBone = Editor.Select.ModBone_Main;
			//apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

			//if (linkedModifier == null || workKeyframe == null)
			//{
			//	//수정할 타겟이 없다.
			//	//추가 5.29 : AutoKey일 수 있으니 별도의 리턴 함수를 이용한다.
			//	//return null;
			//	return GetPivotReturnWhenAutoKey_Transform();
			//}

			//>> [GizmoMain]

			//다중 선택 + [기즈모 메인]에 
			//- Main ModMesh나 Main ModBone이 존재
			//- WorkKeyframe도 존재
			//- [기즈모 메인]도 존재해야한다.
			if (linkedModifier == null || Editor.Select.AnimWorkKeyframe_Main == null 
				|| (Editor.Select.ModMesh_Main == null && Editor.Select.ModBone_Main == null))
			{
				//수정할 타겟이 없다.
				//AutoKey일 수 있으니 별도의 리턴 함수를 이용한다.
				return GetPivotReturnWhenAutoKey_Transform();
			}

			//기즈모 메인을 대상으로 편집한다.
			apModifiedMesh mainModMesh = Editor.Select.ModMesh_Gizmo_Main;
			apModifiedBone mainModBone = Editor.Select.ModBone_Gizmo_Main;
			
			
			bool isTransformPivot = false;
			bool isBonePivot = false;
			
			
			//if (linkedModifier.IsTarget_Bone && Editor.Select.Bone != null && targetModBone != null)
			//{
			//	isBonePivot = true;
			//}

			//if (targetModMesh != null && (targetModMesh._transform_Mesh != null || targetModMesh._transform_MeshGroup != null))
			//{
			//	isTransformPivot = true;
			//}

			//>> [GizmoMain] >>
			if (linkedModifier.IsTarget_Bone 
				&& Editor.Select.Bone_Mod_Gizmo != null 
				&& mainModBone != null)
			{
				isBonePivot = true;
			}

			if (mainModMesh != null 
				&& (mainModMesh._transform_Mesh != null || mainModMesh._transform_MeshGroup != null)
				&& mainModMesh._renderUnit != null)
			{
				isTransformPivot = true;
			}



			//둘다 없으면 Null
			if (!isTransformPivot && !isBonePivot)
			{	
				//수정할 타겟이 없다.
				//추가 5.29 : AutoKey일 수 있으니 별도의 리턴 함수를 이용한다.
				//return null;
				return GetPivotReturnWhenAutoKey_Transform();
			}

			if (isTransformPivot)
			{
				//이건 삭제. ModMesh가 있는데 다른걸 체크할 필욘 없겠지
				//if (Editor.Select.MeshTF_Main == null && Editor.Select.MeshGroupTF_Main == null)//변경 20.6.17
				//{
				//	return null;
				//}
				//if (targetRenderUnit == null)
				//{
				//	return null;
				//}



				//apMatrix transformPivotMatrix = null;//기본 Pivot
				//apMatrix modifiedMatrix = targetModMesh._transformMatrix;
				apMatrix resultMatrix = null;
				
				//int transformDepth = targetModMesh._renderUnit._depth;//이전
				int transformDepth = mainModMesh._renderUnit.GetDepth();

				if (mainModMesh._renderUnit._meshTransform != null)
				{
					//transformPivotMatrix = targetModMesh._renderUnit._meshTransform._matrix;
					resultMatrix = mainModMesh._renderUnit._meshTransform._matrix_TFResult_World;
				}
				else if (mainModMesh._renderUnit._meshGroupTransform != null)
				{
					//transformPivotMatrix = targetModMesh._renderUnit._meshGroupTransform._matrix;
					resultMatrix = mainModMesh._renderUnit._meshGroupTransform._matrix_TFResult_World;
				}
				else
				{
					return null;
				}

				//Vector3 worldPos3 = resultMatrix.Pos3;
				Vector2 worldPos = resultMatrix._pos;

				float worldAngle = resultMatrix._angleDeg;
				Vector2 worldScale = resultMatrix._scale;

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;//Depth를 제외한 TRS
				if (linkedModifier._isColorPropertyEnabled)
				{
					paramType |= apGizmos.TRANSFORM_UI.Color;//<Color를 지원하는 경우에만
				}
				//추가 : Extra 옵션을 설정할 수 있다.
				if(linkedModifier._isExtraPropertyEnabled)
				{
					paramType |= apGizmos.TRANSFORM_UI.Extra;
				}

				return apGizmos.TransformParam.Make(
					worldPos,
					worldAngle,
					//modifiedMatrix._angleDeg,
					worldScale,
					//transformPivotMatrix._scale,
					transformDepth,
					mainModMesh._meshColor,
					mainModMesh._isVisible,
					//worldMatrix, 
					//modifiedMatrix.MtrxToSpace,
					resultMatrix.MtrxToSpace,
					false, paramType,
					mainModMesh._transformMatrix._pos,
					mainModMesh._transformMatrix._angleDeg,
					mainModMesh._transformMatrix._scale);
			}
			else if (isBonePivot)
			{
				if(Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.None)
				{
					//Bone GUI모드가 꺼져있으면 안보인다.
					return null;
				}

				//apBone bone = Editor.Select.Bone;
				apBone bone = mainModBone._bone;//<< [GizmoMain]

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;//Depth를 제외한 TRS
				if(bone._IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None)
				{
					paramType |= apGizmos.TRANSFORM_UI.BoneIKController;//<<BoneIK를 지원하는 경우
				}
				return apGizmos.TransformParam.Make(
					bone._worldMatrix.Pos,
					bone._worldMatrix.Angle,
					bone._worldMatrix.Scale,
					0, bone._color,
					true,
					bone._worldMatrix.MtrxToSpace,
					false, paramType,
					mainModBone._transformMatrix._pos,
					mainModBone._transformMatrix._angleDeg,
					mainModBone._transformMatrix._scale
					);
			}
			return null;
		}


		public apGizmos.TransformParam PivotReturn__Animation_ColorOnly()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null || Editor.Select.AnimTimeline == null)
			{
				return null;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return null;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;

			

			
			//>> [GizmoMain]

			//다중 선택 + [기즈모 메인]에 
			//- Main ModMesh나 Main ModBone이 존재
			//- WorkKeyframe도 존재
			//- [기즈모 메인]도 존재해야한다.
			if (linkedModifier == null || Editor.Select.AnimWorkKeyframe_Main == null 
				|| (Editor.Select.ModMesh_Main == null && Editor.Select.ModBone_Main == null))
			{
				//수정할 타겟이 없다.
				//AutoKey일 수 있으니 별도의 리턴 함수를 이용한다.
				//return GetPivotReturnWhenAutoKey_Transform();

				//ColorOnly에서는 AutoKey를 지원하지 않는다.
				return null;
			}

			//기즈모 메인을 대상으로 편집한다.
			apModifiedMesh mainModMesh = Editor.Select.ModMesh_Gizmo_Main;
			//apModifiedBone mainModBone = Editor.Select.ModBone_Gizmo_Main;//ModBone이 대상이 아니다.
			
			
			bool isTransformPivot = false;
			//bool isBonePivot = false;
			
			
			
			//>> [GizmoMain] >>
			if (mainModMesh != null 
				&& (mainModMesh._transform_Mesh != null || mainModMesh._transform_MeshGroup != null)
				&& mainModMesh._renderUnit != null)
			{
				isTransformPivot = true;
			}



			//둘다 없으면 Null
			if (!isTransformPivot)
			{	
				//수정할 타겟이 없다.
				//추가 5.29 : AutoKey일 수 있으니 별도의 리턴 함수를 이용한다.
				return null;
				//return GetPivotReturnWhenAutoKey_Transform();//Color Only는 AutoKey를 지원하지 않는다.
			}

			if (isTransformPivot)
			{
				apMatrix resultMatrix = null;
				
				int transformDepth = mainModMesh._renderUnit.GetDepth();

				if (mainModMesh._renderUnit._meshTransform != null)
				{
					resultMatrix = mainModMesh._renderUnit._meshTransform._matrix_TFResult_World;
				}
				else if (mainModMesh._renderUnit._meshGroupTransform != null)
				{
					resultMatrix = mainModMesh._renderUnit._meshGroupTransform._matrix_TFResult_World;
				}
				else
				{
					return null;
				}

				//Vector3 worldPos3 = resultMatrix.Pos3;
				Vector2 worldPos = resultMatrix._pos;

				float worldAngle = resultMatrix._angleDeg;
				Vector2 worldScale = resultMatrix._scale;

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.Color;//Color가 기본
				
				//추가 : Extra 옵션을 설정할 수 있다.
				if(linkedModifier._isExtraPropertyEnabled)
				{
					paramType |= apGizmos.TRANSFORM_UI.Extra;
				}

				return apGizmos.TransformParam.Make(
					worldPos,
					worldAngle,
					//modifiedMatrix._angleDeg,
					worldScale,
					//transformPivotMatrix._scale,
					transformDepth,
					mainModMesh._meshColor,
					mainModMesh._isVisible,
					//worldMatrix, 
					//modifiedMatrix.MtrxToSpace,
					resultMatrix.MtrxToSpace,
					false, paramType,
					mainModMesh._transformMatrix._pos,
					mainModMesh._transformMatrix._angleDeg,
					mainModMesh._transformMatrix._scale);
			}
			
			return null;
		}


		private apGizmos.TransformParam GetPivotReturnWhenAutoKey_Transform()
		{
			//Transform 계열의 Pivot Return시에
			//만약 조건이 안되는 경우 (Keyframe이 없을 때)
			//Editor가 AutoKey가 활성화되어있다면
			//TRS 편집을 가능하다.
			//단 이때 Timeline에는 객체가 추가되어있어야 하며, TimelineLayer가 선택되어야 한다.
			if (!Editor._isAnimAutoKey)
			{
				return null;
			}

			if (Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip._targetMeshGroup == null
				|| Editor.Select.AnimTimeline == null
				|| Editor.Select.AnimTimelineLayer_Main == null)
			{
				return null;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return null;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			if (linkedModifier == null)
			{
				//Debug.LogError("No Modifier");
				return null;
			}


			//기존 : AutoKey를 위해서 AnimTimelineLayer_Main의 객체에 피벗을 보여준다.


			#region [미사용 코드] 기존 방식
			//if (Editor.Select.AnimTimelineLayer_Main._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.Bone
			//	&& Editor.Select.AnimTimelineLayer_Main._linkedBone != null
			//	&& Editor.Select.AnimTimelineLayer_Main._linkedBone == Editor.Select.Bone)
			//{
			//	//1. Bone
			//	if (Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.None)
			//	{
			//		//Bone GUI모드가 꺼져있으면 안보인다.
			//		return null;
			//	}

			//	apBone bone = Editor.Select.Bone;

			//	//Debug.Log("Draw Bone");
			//	//TransforUI에서는 표시가 안된다.
			//	apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;

			//	return apGizmos.TransformParam.Make(
			//		bone._worldMatrix._pos,
			//		bone._worldMatrix._angleDeg,
			//		bone._worldMatrix._scale,
			//		0, bone._color,
			//		true,
			//		bone._worldMatrix.MtrxToSpace,
			//		false, paramType,
			//		Vector2.zero,
			//		0.0f,
			//		Vector2.zero
			//		);
			//}
			//else if (Editor.Select.AnimTimelineLayer_Main._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform
			//	&& Editor.Select.AnimTimelineLayer_Main._linkedMeshTransform != null
			//	&& Editor.Select.MeshTF_Main == Editor.Select.AnimTimelineLayer_Main._linkedMeshTransform)
			//{
			//	//2. Mesh Transform
			//	apTransform_Mesh meshTransform = Editor.Select.MeshTF_Main;
			//	if(meshTransform._linkedRenderUnit == null)
			//	{
			//		return null;
			//	}

			//	apMatrix resultMatrix = meshTransform._matrix_TFResult_World;

			//	//int transformDepth = meshTransform._linkedRenderUnit._depth;
			//	int transformDepth = meshTransform._linkedRenderUnit.GetDepth();

			//	//Vector3 worldPos3 = resultMatrix.Pos3;
			//	Vector2 worldPos = resultMatrix._pos;

			//	float worldAngle = resultMatrix._angleDeg;
			//	Vector2 worldScale = resultMatrix._scale;

			//	apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;

			//	return apGizmos.TransformParam.Make(
			//		worldPos,
			//		worldAngle,
			//		//modifiedMatrix._angleDeg,
			//		worldScale,
			//		//transformPivotMatrix._scale,
			//		transformDepth,
			//		Color.black,
			//		meshTransform._linkedRenderUnit._isVisible,
			//		resultMatrix.MtrxToSpace,
			//		false, paramType,
			//		Vector2.zero,0.0f, Vector2.zero);
			//}
			//else if (Editor.Select.AnimTimelineLayer_Main._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform
			//	&& Editor.Select.AnimTimelineLayer_Main._linkedMeshGroupTransform != null
			//	&& Editor.Select.MeshGroupTF_Main == Editor.Select.AnimTimelineLayer_Main._linkedMeshGroupTransform)
			//{
			//	//3. MeshGroup Transform
			//	apTransform_MeshGroup meshGroupTransform = Editor.Select.MeshGroupTF_Main;
			//	if(meshGroupTransform._linkedRenderUnit == null)
			//	{
			//		return null;
			//	}

			//	apMatrix resultMatrix = meshGroupTransform._matrix_TFResult_World;

			//	//int transformDepth = meshGroupTransform._linkedRenderUnit._depth;
			//	int transformDepth = meshGroupTransform._linkedRenderUnit.GetDepth();

			//	//Vector3 worldPos3 = resultMatrix.Pos3;
			//	Vector2 worldPos = resultMatrix._pos;

			//	float worldAngle = resultMatrix._angleDeg;
			//	Vector2 worldScale = resultMatrix._scale;

			//	apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;

			//	return apGizmos.TransformParam.Make(
			//		worldPos,
			//		worldAngle,
			//		//modifiedMatrix._angleDeg,
			//		worldScale,
			//		//transformPivotMatrix._scale,
			//		transformDepth,
			//		Color.black,
			//		meshGroupTransform._linkedRenderUnit._isVisible,
			//		resultMatrix.MtrxToSpace,
			//		false, paramType,
			//		Vector2.zero,0.0f, Vector2.zero);
			//} 
			#endregion


			//>> [GizmoMain] >>
			//변경 20.7.18 : Gizmo_Main 객체가 타임라인 레이어로 등록되어 있다면 보여준다. (메인도 있어야 함)

			//모든 메인 객체(실제 메인 / 기즈모 메인)을 모두 찾자
			apAnimTimeline timeline = Editor.Select.AnimTimeline;
			apAnimTimelineLayer timelineLayer_Main = Editor.Select.AnimTimelineLayer_Main;
			apAnimTimelineLayer timelineLayer_Gizmo = Editor.Select.AnimTimelineLayer_Gizmo;

			apTransform_MeshGroup meshGroupTF_Main = Editor.Select.MeshGroupTF_Main;
			apTransform_MeshGroup meshGroupTF_Gizmo = Editor.Select.SubObjects.GizmoMeshGroupTF;

			apTransform_Mesh meshTF_Main = Editor.Select.MeshTF_Main;
			apTransform_Mesh meshTF_Gizmo = Editor.Select.SubObjects.GizmoMeshTF;

			apBone bone_Main = Editor.Select.Bone;
			apBone bone_Gizmo = Editor.Select.SubObjects.GizmoBone;

			//조건
			//1) 메인 레이어 > 메인 객체와 연결되었는가
			//2) 메인 객체와 기즈모 객체가 다를 경우, 기즈모 객체도 레이어로 연결된 상태인가 (이건 찾아야 한다.)

			//메인 객체와 기즈모 메인의 타입은 다를 수 있다.
			//각각의 조건만 맞으면 된다.

			//메인 객체나 기즈모 객체가 하나도 없다면 취소
			if (meshGroupTF_Main == null && meshTF_Main == null && bone_Main == null)
			{
				return null;
			}
			if (meshGroupTF_Gizmo == null && meshTF_Gizmo == null && bone_Gizmo == null)
			{
				return null;
			}

			bool isMainGizmoSame = false;

			//1. 메인 객체와 메인 레이어가 일치하는가
			if (meshGroupTF_Main != null)
			{
				if (timelineLayer_Main._linkModType != apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform
					|| timelineLayer_Main._linkedMeshGroupTransform != meshGroupTF_Main)
				{
					return null;
				}

				if (meshGroupTF_Gizmo == meshGroupTF_Main)
				{
					isMainGizmoSame = true;
				}
			}
			if (meshTF_Main != null)
			{
				if (timelineLayer_Main._linkModType != apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform
					|| timelineLayer_Main._linkedMeshTransform != meshTF_Main)
				{
					return null;
				}

				if (meshTF_Gizmo == meshTF_Main)
				{
					isMainGizmoSame = true;
				}
			}
			if (bone_Main != null)
			{
				if (timelineLayer_Main._linkModType != apAnimTimelineLayer.LINK_MOD_TYPE.Bone
					|| timelineLayer_Main._linkedBone != bone_Main)
				{
					return null;
				}

				if (bone_Gizmo == bone_Main)
				{
					isMainGizmoSame = true;
				}
			}

			//2. 기즈모 메인이 메인 객체와 다르다면, 여기에 맞는 타임라인 레이어가 등록되어 있는가
			if (!isMainGizmoSame)
			{
				//기즈모 메인에 연결된 타임라인 레이어가 없거나 
				if(timelineLayer_Gizmo == null)
				{
					return null;
				}
				
				//기즈모용 타임라인 레이어와 실제 객체가 일치하지 않는 경우
				if(meshGroupTF_Gizmo != null)
				{
					if(timelineLayer_Gizmo._linkedMeshGroupTransform != meshGroupTF_Gizmo)
					{
						return null;
					}
				}
				else if(meshTF_Gizmo != null)
				{
					if(timelineLayer_Gizmo._linkedMeshTransform != meshTF_Gizmo)
					{
						return null;
					}
				}
				else if(bone_Gizmo != null)
				{
					if(timelineLayer_Gizmo._linkedBone != bone_Gizmo)
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}

			//조건 체크 끝. 이제 생성하자.
			//피벗의 위치는 기즈모 객체에 생성한다.
			//MeshGroupTF부터
			if(meshGroupTF_Gizmo != null)
			{
				//1. MeshGroup Transform
				if(meshGroupTF_Gizmo._linkedRenderUnit == null)
				{
					return null;
				}

				apMatrix resultMatrix = meshGroupTF_Gizmo._matrix_TFResult_World;

				int transformDepth = meshGroupTF_Gizmo._linkedRenderUnit.GetDepth();

				Vector2 worldPos = resultMatrix._pos;

				float worldAngle = resultMatrix._angleDeg;
				Vector2 worldScale = resultMatrix._scale;

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;
				
				return apGizmos.TransformParam.Make(
					worldPos,
					worldAngle,
					//modifiedMatrix._angleDeg,
					worldScale,
					//transformPivotMatrix._scale,
					transformDepth,
					Color.black,
					meshGroupTF_Gizmo._linkedRenderUnit._isVisible,
					resultMatrix.MtrxToSpace,
					false, paramType,
					Vector2.zero,0.0f, Vector2.zero);
			}
			else if(meshTF_Gizmo != null)
			{
				//2. Mesh Transform
				if(meshTF_Gizmo._linkedRenderUnit == null)
				{
					return null;
				}

				apMatrix resultMatrix = meshTF_Gizmo._matrix_TFResult_World;

				//int transformDepth = meshTransform._linkedRenderUnit._depth;
				int transformDepth = meshTF_Gizmo._linkedRenderUnit.GetDepth();

				//Vector3 worldPos3 = resultMatrix.Pos3;
				Vector2 worldPos = resultMatrix._pos;

				float worldAngle = resultMatrix._angleDeg;
				Vector2 worldScale = resultMatrix._scale;

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;
				
				return apGizmos.TransformParam.Make(
					worldPos,
					worldAngle,
					//modifiedMatrix._angleDeg,
					worldScale,
					//transformPivotMatrix._scale,
					transformDepth,
					Color.black,
					meshTF_Gizmo._linkedRenderUnit._isVisible,
					resultMatrix.MtrxToSpace,
					false, paramType,
					Vector2.zero,0.0f, Vector2.zero);
			}
			else if(bone_Gizmo != null)
			{
				//3. Bone
				if (Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.None)
				{
					//Bone GUI모드가 꺼져있으면 안보인다.
					return null;
				}

				//TransforUI에서는 표시가 안된다.
				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;

				return apGizmos.TransformParam.Make(
					bone_Gizmo._worldMatrix.Pos,
					bone_Gizmo._worldMatrix.Angle,
					bone_Gizmo._worldMatrix.Scale,
					0, bone_Gizmo._color,
					true,
					bone_Gizmo._worldMatrix.MtrxToSpace,
					false, paramType,
					Vector2.zero,
					0.0f,
					Vector2.zero
					);
			}

			return null;

		}



		public apGizmos.TransformParam PivotReturn__Animation_Vertex()
		{
			if (Editor.Select.AnimClip == null || Editor.Select.AnimClip._targetMeshGroup == null)
			{
				return null;
			}

			apModifierBase linkedModifier = Editor.Select.AnimTimeline._linkedModifier;
			apAnimKeyframe workKeyframe = Editor.Select.AnimWorkKeyframe_Main;
			apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;
			apRenderUnit targetRenderUnit = Editor.Select.RenderUnitOfMod_Main;

			if (linkedModifier == null || workKeyframe == null || targetModMesh == null || targetRenderUnit == null)
			{
				//수정할 타겟이 없다.
				return null;
			}

			if (Editor.Select.ExAnimEditingMode == apSelection.EX_EDIT.None || Editor.Select.IsAnimPlaying)
			{
				//에디팅 중이 아니다.
				return null;
			}

			apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.None;
			if (linkedModifier._isColorPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Color;//<Color를 지원하는 경우에만
			}
			if(linkedModifier._isExtraPropertyEnabled)
			{
				paramType |= apGizmos.TRANSFORM_UI.Extra;
			}

			//대상을 하나라도 선택했는가
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

			//선택된게 없다면
			if (!isAnyTargetSelected)
			{
				//단 한개의 선택된 Vertex도 없다. > Color 옵션만이라도 설정할 수 있게 하자
				if (linkedModifier._isColorPropertyEnabled
					|| linkedModifier._isExtraPropertyEnabled)
				{
					return apGizmos.TransformParam.Make(apEditorUtil.InfVector2, 0.0f, Vector2.one, 0,
													targetModMesh._meshColor,
													targetModMesh._isVisible,
													apMatrix3x3.identity,
													false,
													paramType,
													Vector2.zero, 0.0f, Vector2.one
													);
				}
				return null;
			}


			paramType |= apGizmos.TRANSFORM_UI.Position2D;


			if (Editor.Select.MorphEditTarget == apSelection.MORPH_EDIT_TARGET.Vertex)
			{
				// [ 버텍스 편집 모드 ]
				
				int nMRVs = Editor.Select.ModRenderVerts_All.Count;

				if (nMRVs > 1)
				{
					paramType |= apGizmos.TRANSFORM_UI.Vertex_Transform;

					Vector2 avgDeltaPos = Vector2.zero;
					for (int i = 0; i < nMRVs; i++)
					{
						avgDeltaPos += Editor.Select.ModRenderVerts_All[i]._modVert._deltaPos;
					}
					avgDeltaPos /= nMRVs;

					//다중 선택 중
					return apGizmos.TransformParam.Make(Editor.Select.ModRenderVertsCenterPos,
															0.0f,
															Vector2.one,
															targetRenderUnit.GetDepth(),
															//targetRenderUnit.GetColor(),
															targetModMesh._meshColor,
															targetModMesh._isVisible,
															apMatrix3x3.TRS(Vector2.zero, 0, Vector2.one),
															true,
															paramType,
															avgDeltaPos,
															0.0f,
															Vector2.one
															);
				}
				else
				{
					//한개만 선택했다.
					return apGizmos.TransformParam.Make(Editor.Select.ModRenderVert_Main._renderVert._pos_World,
															0.0f,
															Vector2.one,
															targetRenderUnit.GetDepth(),
															//targetRenderUnit.GetColor(),
															targetModMesh._meshColor,
															targetModMesh._isVisible,
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
					paramType |= apGizmos.TRANSFORM_UI.Vertex_Transform;

					Vector2 avgDeltaPos = Vector2.zero;
					for (int i = 0; i < nMRPs; i++)
					{
						avgDeltaPos += Editor.Select.ModRenderPins_All[i]._modPin._deltaPos;
					}
					avgDeltaPos /= nMRPs;

					//다중 선택 중
					return apGizmos.TransformParam.Make(Editor.Select.ModRenderPinsCenterPos,
															0.0f,
															Vector2.one,
															targetRenderUnit.GetDepth(),
															targetModMesh._meshColor,
															targetModMesh._isVisible,
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
					//한개만 선택했다.
					return apGizmos.TransformParam.Make(Editor.Select.ModRenderPin_Main._renderPin._pos_World,
															0.0f,
															Vector2.one,
															targetRenderUnit.GetDepth(),
															targetModMesh._meshColor,
															targetModMesh._isVisible,
															Editor.Select.ModRenderPin_Main._renderPin._matrix_ToWorld,
															false,
															paramType,
															Editor.Select.ModRenderPin_Main._modPin._deltaPos,
															0.0f,
															Vector2.one
															);

				}
			}
			


			//return null;
		}

		
	}

}