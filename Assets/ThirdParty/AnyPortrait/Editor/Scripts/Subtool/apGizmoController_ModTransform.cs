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

	//GizmoController -> Modifier [TF]에 대한 내용이 담겨있다.
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

		// FirstLink : int

		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
		// FFD Style Transform Start : bool - ()

		//----------------------------------------------------------------
		// Gizmo - MeshGroup : Modifier / TF
		//----------------------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_Modifier_TF()
		{
			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Modifier_Transform,
														Unselect__Modifier_Transform, 
														Move__Modifier_Transform, 
														Rotate__Modifier_Transform, 
														Scale__Modifier_Transform, 
														PivotReturn__Modifier_Transform);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	TransformChanged_Position__Modifier_Transform,
																TransformChanged_Rotate__Modifier_Transform,
																TransformChanged_Scale__Modifier_Transform,
																null,
																TransformChanged_Color__Modifier_Transform,
																TransformChanged_Extra__Modifier_Transform,
																apGizmos.TRANSFORM_UI.TRS_NoDepth 
																	| apGizmos.TRANSFORM_UI.Color 
																	| apGizmos.TRANSFORM_UI.BoneIKController
																	| apGizmos.TRANSFORM_UI.Extra//<<Extra 옵션 추가
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	null, 
														null, 
														null, 
														null, 
														null, 
														null);
			
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Modifier_Transform, 
																null, 
																OnHotKeyEvent__Modifier_Transform__Keyboard_Move, 
																OnHotKeyEvent__Modifier_Transform__Keyboard_Rotate, 
																OnHotKeyEvent__Modifier_Transform__Keyboard_Scale);

			return apGizmos.GizmoEventSet.I;

			//이전
			//return new apGizmos.GizmoEventSet(
			//	Select__Modifier_Transform,
			//	Unselect__Modifier_Transform,
			//	Move__Modifier_Transform,
			//	Rotate__Modifier_Transform,
			//	Scale__Modifier_Transform,
			//	TransformChanged_Position__Modifier_Transform,
			//	TransformChanged_Rotate__Modifier_Transform,
			//	TransformChanged_Scale__Modifier_Transform,
			//	null,
			//	TransformChanged_Color__Modifier_Transform,
			//	TransformChanged_Extra__Modifier_Transform,
			//	//TransformChanged_BoneIKController__Modifier_Transform,
			//	PivotReturn__Modifier_Transform,
			//	null, null, null, null, null, null,
			//	apGizmos.TRANSFORM_UI.TRS_NoDepth 
			//	| apGizmos.TRANSFORM_UI.Color 
			//	| apGizmos.TRANSFORM_UI.BoneIKController
			//	| apGizmos.TRANSFORM_UI.Extra,//<<Extra 옵션 추가
			//	FirstLink__Modifier_Transform,
			//	null);
		}


		public apGizmos.SelectResult FirstLink__Modifier_Transform()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			//>> [GizmoMain] >>
			if(Editor.Select.MeshGroupTF_Mod_Gizmo != null)
			{
				Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshGroupTF_Mod_Gizmo);
			}
			if(Editor.Select.MeshTF_Mod_Gizmo != null)
			{
				Editor.Select.SelectBone(null, apSelection.MULTI_SELECT.Main);
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.MeshTF_Mod_Gizmo);
			}
			if (Editor.Select.Modifier.IsTarget_Bone && Editor.Select.Bone_Mod_Gizmo != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone_Mod_Gizmo);
			}
			

			return null;
		}


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Transform(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			


			// (Editing 상태일때)

			//추가 : Bone 선택
			//렌더링이 Bone이 먼저라서,
			//Bone이 가능하면, Bone을 먼저 선택한다.


			// 1. (Lock 걸리지 않았다면) 다른 Mesh Transform을 선택
			// 그 외에는 선택하는 것이 없다.

			// (Editing 상태가 아닐때)
			// (Lock 걸리지 않았다면) 다른 MeshTransform 선택
			//... Lock만 생각하면 될 것 같다.
			bool isBoneTarget = Editor.Select.Modifier.IsTarget_Bone;

			bool isChildMeshTransformSelectable = Editor.Select.Modifier.IsTarget_ChildMeshTransform;

			//>> [GizmoMain] >>
			apTransform_MeshGroup prevSelectedMeshGroupTransform = Editor.Select.MeshGroupTF_Mod_Gizmo;
			apTransform_Mesh prevSelectedMeshTransform = Editor.Select.MeshTF_Mod_Gizmo;
			apBone prevSelectedBone = Editor.Select.Bone_Mod_Gizmo;

			//int prevSelected = 0;
			object prevSelectedObj = null;
			if (prevSelectedMeshGroupTransform != null)
			{
				prevSelectedObj = prevSelectedMeshGroupTransform;
			}
			else if (prevSelectedMeshTransform != null)
			{
				prevSelectedObj = prevSelectedMeshTransform;
			}
			else if (prevSelectedBone != null && isBoneTarget)
			{
				prevSelectedObj = prevSelectedBone;
			}
			
			if (!Editor.Controller.IsMouseInGUI(mousePosGL) 
				|| selectType == apGizmos.SELECT_TYPE.Subtract//추가 20.5.27 : Alt키를 누른 상태에서는 아무런 동작을 하지 않는다.
				)
			{
				return apGizmos.SelectResult.Main.SetSingle(prevSelectedObj);
			}


			
			_prevSelected_TransformBone = null;

			//추가 20.5.27 : 선택 작업이 끝났다.
			bool isSelectionCompleted = false;

			//추가 20.5.27 : 다중 선택 방식
			apSelection.MULTI_SELECT multiSelect = (selectType == apGizmos.SELECT_TYPE.New) ? apSelection.MULTI_SELECT.Main : apSelection.MULTI_SELECT.AddOrSubtract;

			//GUI에서는 MeshGroup을 선택할 수 없다.
			//리스트 UI에서만 선택 가능함
			//단, Child Mesh Transform을 허용하지 않는다면 얘기는 다르다.

			//int result = prevSelected;

			if (!Editor.Select.IsSelectionLock)
			{
				//Lock이 걸려있지 않다면 새로 선택할 수 있다.
				//여러개를 선택할 수도 있다....단일 선택만 할까.. => 단일 선택만 하자

				apTransform_Mesh selectedMeshTransform = null;
				apBone selectedBone = null;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;

				//추가 21.2.17 : 편집 중이 아닌 오브젝트를 선택하는 건 "편집 모드가 아니거나" / "선택 제한 옵션이 꺼진 경우"이다.
				bool isNotEditObjSelectable = Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || !Editor._exModObjOption_NotSelectable;


				//Bone 먼저 선택하자
				if (isBoneTarget)
				{
					apBone bone = null;
					apBone resultBone = null;

					//>>Bone Set으로 변경
					apMeshGroup.BoneListSet boneSet = null;
					if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
					{
						//for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)//출력 순서
						for (int iSet = meshGroup._boneListSets.Count - 1; iSet >= 0; iSet--)//선택 순서
						{
							boneSet = meshGroup._boneListSets[iSet];
							if (boneSet._bones_Root != null && boneSet._bones_Root.Count > 0)
							{
								//for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)//출력 순서
								for (int iRoot = boneSet._bones_Root.Count - 1; iRoot >= 0; iRoot--)//선택 순서
								{
									bone = CheckBoneClickRecursive(	boneSet._bones_Root[iRoot], 
																	mousePosW, mousePosGL, 
																	Editor._boneGUIRenderMode, 
																	//-1, 
																	Editor.Select.IsBoneIKRenderable, isNotEditObjSelectable,
																	false);
									if (bone != null)
									{
										resultBone = bone;
										break;//다른 Root List는 체크하지 않는다.
									}
								}
							}

							if (resultBone != null)
							{
								//이 Set에서 선택이 완료되었다.
								break;
							}
						}
					}


					if (resultBone != null)
					{
						//변경 20.4.11 : 위 함수를 매번 호출하면 느리므로, 한번에 처리하는 함수를 이용하자.
						//추가 20.5.27 : 
						Editor.Select.SelectSubObject(null, null, resultBone, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);


						//selectedBone = resultBone;//기존 코드
						//selectedBone = Editor.Select.Bone;//변경 20.5.27 : 다중 선택때문에 메인이 무엇인지 다시 확인해봐야한다.

						//>> [GizmoMain] >>
						selectedBone = Editor.Select.Bone_Mod_Gizmo;

						prevSelectedObj = selectedBone;

						isSelectionCompleted = true;//추가 20.5.27 : 선택된 본과 상관없이 처리가 끝났다.

						//[1.4.2] 선택된 객체에 맞게 자동 스크롤
						if(Editor._option_AutoScrollWhenObjectSelected)
						{
							//스크롤 가능한 상황인지 체크하고
							if(Editor.IsAutoScrollableWhenClickObject_MeshGroup(resultBone, true))
							{
								//자동 스크롤을 요청한다.
								Editor.AutoScroll_HierarchyMeshGroup(resultBone);
							}
						}
					}
				}


				//if (selectedBone == null)//기존 : 본이 선택 안되었으면 연속으로 선택
				if (!isSelectionCompleted)//변경 20.5.27 : 선택 처리가 아직 안끝났다면
				{
					//Bone이 선택 안되었다면 MeshTransform을 선택

					//정렬된 Render Unit
					//List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<이전 : RenderUnits_All 이용
					List<apRenderUnit> renderUnits = Editor.Select.MeshGroup.SortedRenderUnits;//<<변경 : Sorted 리스트 이용

					if (renderUnits.Count > 0)
					{
						apRenderUnit renderUnit = null;
						//for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)//기존 : 뒤에서부터 앞으로 선택
						for (int iUnit = renderUnits.Count - 1; iUnit >= 0; iUnit--)//변경 20.5.27 : 앞에서부터 뒤로 선택. 선택시 바로 break 할 수 있게
						{
							renderUnit = renderUnits[iUnit];

							


							if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
							{
								//추가 21.2.17
								if(!isNotEditObjSelectable &&
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
									//bool isPick = apEditorUtil.IsMouseInMesh(
									//	mousePosGL,
									//	renderUnit._meshTransform._mesh,
									//	renderUnit.WorldMatrixOfNode.inverse
									//	);
									bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(mousePosGL, renderUnit);

									if (isPick)
									{
										selectedMeshTransform = renderUnit._meshTransform;
										//기존
										//찾았어도 계속 찾는다. 뒤의 아이템이 "앞쪽"에 있는 것이기 때문

										//변경 20.5.27
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
						//Mesh Group 자체를 선택해야 한다.
						apMeshGroup parentMeshGroup = Editor.Select.MeshGroup.FindParentMeshGroupOfMeshTransform(selectedMeshTransform);

						object selectedObj = null;

						if (parentMeshGroup == null || parentMeshGroup == Editor.Select.MeshGroup || isChildMeshTransformSelectable)
						{
							//Editor.Select.SetSubMeshInGroup(selectedMeshTransform);//이전
							//Editor.Select.SetSubObjectInGroup(selectedMeshTransform, null, null);//변경 20.4.11
							Editor.Select.SelectSubObject(selectedMeshTransform, null, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);//변경 20.5.27 : 다중 선택 옵션
							selectedObj = selectedMeshTransform;
						}
						else
						{
							apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
							if (childMeshGroupTransform != null)
							{
								//Editor.Select.SetSubMeshGroupInGroup(childMeshGroupTransform);//이전
								//Editor.Select.SetSubObjectInGroup(null, childMeshGroupTransform, null);//변경 20.4.11
								Editor.Select.SelectSubObject(null, childMeshGroupTransform, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);//변경 20.5.27 : 다중 선택
								selectedObj = childMeshGroupTransform;
							}
							else
							{
								//Editor.Select.SetSubMeshInGroup(selectedMeshTransform);//이전
								//Editor.Select.SetSubObjectInGroup(selectedMeshTransform, null, null);//변경 20.4.11
								Editor.Select.SelectSubObject(selectedMeshTransform, null, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);//변경 20.5.27 : 다중 선택
								selectedObj = selectedMeshTransform;
							}
						}

						isSelectionCompleted = true;

						//Editor.Select.SetBone(null);//삭제 20.4.11

						//추가 20.5.27 : 다중 선택도 있으므로 현재 선택한 MeshTF가 꼭 선택된 상태가 아닐 수 있다.
						//selectedMeshTransform = Editor.Select.MeshTF_Main;

						//>> [GizmoMain] >>
						selectedMeshTransform = Editor.Select.MeshTF_Mod_Gizmo;
						
						prevSelectedObj = selectedMeshTransform;

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
				}


				//삭제 20.5.27 : 아래에서 처리
				//if(!_isSelectionCompleted)
				//{
				//	//Editor.Select.SetBone(null);
				//	Editor.Select.SetSubMeshInGroup(null, multiSelect);
				//}


				//if (selectedMeshTransform == null && selectedBone == null)//이전
				if(!isSelectionCompleted && multiSelect == apSelection.MULTI_SELECT.Main)//변경 20.5.27 : "추가 선택"이 아닌데 선택된게 없다면 취소.
				{
					//이전
					//Editor.Select.SetBone(null);
					//Editor.Select.SetSubMeshGroupInGroup(null);
					//Editor.Select.SetSubMeshInGroup(null);

					//변경 20.4.11 : 위 코드를 한번에 처리하는 함수를 이용하자
					Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
					prevSelectedObj = null;
				}
			}

			Editor.RefreshControllerAndHierarchy(false);
			//Editor.Repaint();
			Editor.SetRepaint();

			//return result;
			return apGizmos.SelectResult.Main.SetSingle(prevSelectedObj);
		}



		public void Unselect__Modifier_Transform()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}
			if (!Editor.Select.IsSelectionLock)
			{
				//락이 풀려있어야 한다.
				//이전
				//Editor.Select.SetBone(null);
				//Editor.Select.SetSubMeshGroupInGroup(null);
				//Editor.Select.SetSubMeshInGroup(null);

				//변경 20.4.11
				Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);


				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}
		}

		//정확한 처리를 위한 작업

		private object _prevSelected_TransformBone = null;
		private Vector2 _prevSelected_MousePosW = Vector2.zero;
		//private Vector2 _prevSelected_WorldPos = Vector2.zero;

		private apMatrix _tmpNextWorldMatrix = null;

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 위치값을 수정할 때
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		public void Move__Modifier_Transform(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null
				)
			{
				return;
			}

			if(deltaMoveW.sqrMagnitude == 0.0f && !isFirstMove)
			{
				return;
			}


			//Editing 상태가 아니면 패스 + ParamSet이 없으면 패스
			if (Editor.Select.ParamSetOfMod == null//변경 20.6.17 : ExKey를 모두 삭제해서 그냥 변수를 참조하면 된다.
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				return;
			}

			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}
			
			if (!Editor.Controller.IsMouseInGUI(curMouseGL))
			{
				return;
			}

			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_MoveTransform, 
													Editor, 
													Editor.Select.Modifier, 
													//targetObj, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}

			bool isMain = false;

			//1. ModMesh 수정
			if(nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//사용하지 않게 되었다 (20.10.31)
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;
				object selectedTransform = null;
				

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];
					
					//isMain = (curModMesh == Editor.Select.ModMesh_Main);
					isMain = (curModMesh == Editor.Select.ModMesh_Gizmo_Main);//<< [GizmoMain]

					if(curModMesh == null)
					{
						continue;
					}

					//resultMatrix = null;//삭제 20.10.31
					matx_ToParent = null;
					matx_ParentWorld = null;
					selectedTransform = null;

					if(curModMesh._isMeshTransform && curModMesh._transform_Mesh != null)
					{
						//resultMatrix = curModMesh._transform_Mesh._matrix_TFResult_World;//삭제 20.10.31
						matx_ToParent = curModMesh._transform_Mesh._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_Mesh._matrix_TF_ParentWorld;

						selectedTransform = curModMesh._transform_Mesh;
					}
					else if(!curModMesh._isMeshTransform && curModMesh._transform_MeshGroup != null)
					{
						//resultMatrix = curModMesh._transform_MeshGroup._matrix_TFResult_World;//삭제 20.10.31
						matx_ToParent = curModMesh._transform_MeshGroup._matrix_TF_ToParent;
						matx_ParentWorld = curModMesh._transform_MeshGroup._matrix_TF_ParentWorld;

						selectedTransform = curModMesh._transform_MeshGroup;
					}
					
					//삭제 20.10.31
					//if(resultMatrix == null)
					//{
					//	continue;
					//}

					if (isMain)
					{
						if (selectedTransform != _prevSelected_TransformBone || isFirstMove)
						{
							_prevSelected_TransformBone = selectedTransform;
							_prevSelected_MousePosW = curMousePosW;
						}
					}

					//resultMatrix.MakeMatrix();//삭제 20.10.31
					matx_ToParent.MakeMatrix();
					matx_ParentWorld.MakeMatrix();

					#region [미사용 코드] 이전 방식 + 디버그 코드들
					//if(_tmpNextWorldMatrix == null)
					//{
					//	_tmpNextWorldMatrix = new apMatrix(resultMatrix);
					//}
					//else
					//{
					//	_tmpNextWorldMatrix.SetMatrix(resultMatrix, true);
					//}

					////중요
					////apMatrixCal의 CalculateLocalPos_ModMesh 함수가 추가됨(20.9.10)에 따라서
					////편집모드에서도 [ _transform_Mesh._matrix_TF_LocalModified ]와 [ curModMesh._transformMatrix ]가 동일하지 않게 되었다.


					////테스트로 Default와 Mod를 계산해보자
					//apMatrix testDefaultAndModMatrix = new apMatrix(matx_ToParent);
					//testDefaultAndModMatrix.OnBeforeRMultiply();

					////Debug.Log("> Default : " + testDefaultAndModMatrix.ToString());
					////Debug.Log("(ModMesh) : " + curModMesh._transformMatrix.ToString());
					//testDefaultAndModMatrix.RMultiply(curModMesh._transformMatrix, true);
					////Debug.Log("> Default+Mod Matrix : " + testDefaultAndModMatrix.ToString() + " / 초기 Scale 부호 : " + testDefaultAndModMatrix._isInitScalePositive_X + ", " + testDefaultAndModMatrix._isInitScalePositive_Y);
					//testDefaultAndModMatrix.RMultiply(matx_ParentWorld, true);
					////Debug.Log("> World Matrix : " + testDefaultAndModMatrix.ToString());

					////testDefaultAndModMatrix._pos = apMatrix.RReverseInverse(matx_ToParent, testDefaultAndModMatrix)._pos;
					////Debug.LogWarning("<<>> 보정된 Default+ModMatrix : " + testDefaultAndModMatrix.ToString());


					//testDefaultAndModMatrix.SetPos(testDefaultAndModMatrix._pos + deltaMoveW, true);
					//testDefaultAndModMatrix.RInverse(matx_ParentWorld, true);
					//testDefaultAndModMatrix.RInverseToMid(matx_ToParent, true);


					//apMatrix defaultModMatrix = apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true);

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					////apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, defaultModMatrix);



					////apMatrix reReverseInversedModMatrix = apMatrix.RRecover_ReverseInverse(matx_ToParent, nextLocalModifiedMatrix);
					////Debug.LogError("->> ReverseInverse로부터 복구한 ModMesh의 Matrix : " + reReverseInversedModMatrix.ToString());

					////TRS중에 Pos만 넣어도 된다.




					////Vector2 deltaPosL = nextLocalModifiedMatrix._pos - curModMesh._transform_Mesh._matrix_TF_LocalModified._pos;
					////curModMesh._transformMatrix.SetPos(nextLocalModifiedMatrix._pos, true);//이게 버그 (이전 코드)
					//curModMesh._transformMatrix.SetPos(testDefaultAndModMatrix._pos, true);//이걸로 바꾸자 
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
					

					//curModMesh.RefreshValues_Check(Editor._portrait);//필요없당.
				}
			}


			//2. ModBone 수정
			if(nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;
				apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.17
				
				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }
					
					//isMain = (curModBone == Editor.Select.ModBone_Main);
					isMain = (curModBone == Editor.Select.ModBone_Gizmo_Main);//<< [GizmoMain]

					bone = curModBone._bone;
					if(bone == null) { continue; }
					

					//Move로 제어 가능한 경우는
					//1. IK Tail일 때
					//2. Root Bone일때 (절대값)
					if (bone._isIKTail)
					{
						//Debug.Log("Request IK : " + _boneSelectPosW);
						float weight = 1.0f;

						Vector2 bonePosW = Vector2.zero;


						//이게 기존 코드
						if (isMain)
						{
							//메인은 
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
							//이게 상대 좌표로만 계산
							bonePosW = bone._worldMatrix.Pos + deltaMoveW;
						}

						//Vector2 bonePosW = bone._worldMatrix._pos + deltaMoveW;//DeltaPos 이용
						//Vector2 bonePosW = curMousePosW;//절대 위치 이용

						//IK 계산
						apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

						if (limitedHeadBone == null)
						{
							continue;
						}


						apBone curIKBone = bone;
						//위로 올라가면서 IK 결과값을 Default에 적용하자

						while (true)
						{
							float deltaAngle = curIKBone._IKRequestAngleResult;

							//추가 20.10.9 : 부모 본이 있거나 부모 렌더 유닛의 크기가 한축만 뒤집혔다면 deltaAngle을 반전하자
							if(curIKBone.IsNeedInvertIKDeltaAngle_Gizmo())
							{
								deltaAngle = -deltaAngle;
							}


							
							//if(Mathf.Abs(deltaAngle) > 30.0f)
							//{
							//	deltaAngle *= deltaAngle * 0.1f;
							//}

							apModifiedBone targetIKModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
							{
								return a._bone == curIKBone;
							});
							if (targetIKModBone != null)
							{
								//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
								//해당 Bone의 ModBone을 가져온다.

								float nextAngle = apUtil.AngleTo180(curIKBone._localMatrix._angleDeg + deltaAngle);
								
								//DefaultMatrix말고
								//curBone._defaultMatrix.SetRotate(nextAngle);

								//ModBone을 수정하자
								targetIKModBone._transformMatrix.SetRotate(nextAngle, true);
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
						#region [미사용 코드]
						//--------------------------------------
						// 기존 방식
						//--------------------------------------
						//apMatrix parentMatrix = null;
						//if (bone._parentBone == null)
						//{
						//	if (bone._renderUnit != null)
						//	{
						//		//Render Unit의 World Matrix를 참조하여
						//		//로컬 값을 Default로 적용하자
						//		parentMatrix = bone._renderUnit.WorldMatrixWrap;
						//	}
						//}
						//else
						//{
						//	parentMatrix = bone._parentBone._worldMatrix;
						//}

						////apMatrix localMatrix = bone._localMatrix;
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

						//--------------------------------------
						// 변경된 방식 20.8.14 : ComplexMatrix로 변경
						// Temp를 이용하자
						//--------------------------------------
						//apComplexMatrix parentMatrix = null;
						//if (bone._parentBone == null)
						//{
						//	if (bone._renderUnit != null)
						//	{
						//		//Render Unit의 World Matrix를 참조하여
						//		//로컬 값을 Default로 적용하자
						//		parentMatrix = apComplexMatrix.TempMatrix_1;
						//		parentMatrix.SetMatrix_Step2(bone._renderUnit.WorldMatrixWrap, true);
						//	}
						//}
						//else
						//{
						//	parentMatrix = bone._parentBone._worldMatrix;
						//}

						//apComplexMatrix tmpNextWorldMatrix = apComplexMatrix.TempMatrix_2;
						//tmpNextWorldMatrix.CopyFromComplexMatrix(bone._worldMatrix);
						//tmpNextWorldMatrix.MoveAsPostResult(deltaMoveW);


						//if (parentMatrix != null)
						//{
						//	tmpNextWorldMatrix.Inverse(parentMatrix);
						//}
						//else
						//{
						//	//Inverse를 안했다면 Step2를 리셋해야한다.
						//} 
						#endregion


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



						//WorldMatrix에서 Local Space에 위치한 Matrix는
						//Default + Local + RigTest이다.

						//이전
						//_tmpNextWorldMatrix.Subtract(bone._defaultMatrix);
						//if (bone._isRigTestPosing)
						//{
						//	_tmpNextWorldMatrix.Subtract(bone._rigTestMatrix);
						//}

						//curModBone._transformMatrix.SetPos(_tmpNextWorldMatrix._pos);

						//변경 20.8.19 : 래핑된 코드
						nextWorldMatrix.SubtractAsLocalValue(bone._defaultMatrix, false);
						if (bone._isRigTestPosing)
						{
							nextWorldMatrix.SubtractAsLocalValue(bone._rigTestMatrix, false);
						}
						nextWorldMatrix.MakeMatrix(false);
						curModBone._transformMatrix.SetPos(nextWorldMatrix.Pos, true);
						
					}
				}
			}

			//전체 갱신하자
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();

		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 회전값을 수정할 때
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Modifier_Transform(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null
				)
			{
				return;
			}
			if(deltaAngleW == 0.0f && !isFirstRotate)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//|| Editor.Select.ExKey_ModParamSet == null//이전
				|| Editor.Select.ParamSetOfMod == null//변경 20.6.17
				)
			{
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			////ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			////이전
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			////변경 20.6.17 : ExKey/Value 삭제함
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	//Debug.LogError("Rotate Error");
			//	return;
			//}

			////Undo
			//object targetObj = null;

			////이전
			////if (isTargetTransform) { targetObj = Editor.Select.ExValue_ModMesh; }
			////else { targetObj = Editor.Select.ModBoneOfMod; }

			////변경 20.6.17
			//if (isTargetTransform)	{ targetObj = Editor.Select.ModMesh_Main; }
			//else					{ targetObj = Editor.Select.ModBone_Main; }

			//if (isFirstRotate)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, Editor, Editor.Select.Modifier, targetObj, false);
			//}

			//if (isTargetTransform)
			//{
			//	//apCalculatedLog.InverseResult calResult = Editor.Select.ExValue_ModMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(deltaAngleW, Vector2.zero);

			//	//Editor.Select.ExValue_ModMesh._transformMatrix.SetRotate(Editor.Select.ExValue_ModMesh._transformMatrix._angleDeg + deltaAngleW);

			//	////Pos 보정
			//	//if (calResult != null && calResult._isSuccess)
			//	//{
			//	//	Editor.Select.ExValue_ModMesh._transformMatrix.SetPos(calResult._posL_next);
			//	//}

			//	//apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;//이전
			//	apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;//변경 20.6.17

			//	apMatrix resultMatrix = null;
			//	apMatrix matx_ToParent = null;
			//	apMatrix matx_ParentWorld = null;

			//	if(targetModMesh._isMeshTransform)
			//	{
			//		if(targetModMesh._transform_Mesh != null)
			//		{
			//			//Mesh Transform
			//			resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;
			//			matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		//if(Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
			//		if(Editor.Select.ModMesh_Main._transform_MeshGroup != null)//변경 20.6.17
			//		{
			//			//Mesh Group Transform
			//			resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;
			//			matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if(resultMatrix == null)
			//	{
			//		return;
			//	}

			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW);//각도 변경

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	targetModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
			//											apUtil.AngleTo180(nextLocalModifiedMatrix._angleDeg),
			//											targetModMesh._transformMatrix._scale);
			//}
			//else if (isTargetBone)
			//{
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경

			//	//<BONE_EDIT> 하위 본도 수정 가능
			//	//if (bone._meshGroup != meshGroup)
			//	//{
			//	//	return;
			//	//}


			//	//Default Angle은 -180 ~ 180 범위 안에 들어간다.
			//	//float nextAngle = bone._defaultMatrix._angleDeg + deltaAngleW;
			//	float nextAngle = modBone._transformMatrix._angleDeg + deltaAngleW;

			//	while (nextAngle < -180.0f)
			//	{
			//		nextAngle += 360.0f;
			//	}
			//	while (nextAngle > 180.0f)
			//	{
			//		nextAngle -= 360.0f;
			//	}

			//	modBone._transformMatrix.SetRotate(nextAngle);
			//}
			////강제로 업데이트할 객체를 선택하고 Refresh

			//Editor.Select.MeshGroup.RefreshForce(); 
			#endregion



			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}

			//Undo
			//object targetObj = null;//삭제 21.6.30
			
			
			//>> [GizmoMain]
			//if(Editor.Select.ModMesh_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Gizmo_Main;
			//}
			//else if(Editor.Select.ModBone_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Gizmo_Main;
			//}

			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, 
													Editor, 
													Editor.Select.Modifier, 
													//targetObj,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;//삭제 20.11.1
				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
					{
						continue;
					}

					//resultMatrix = null;//삭제 20.11.1
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

					//삭제 20.11.1
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

					//_tmpNextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW, true);//각도 변경

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//									apUtil.AngleTo180(nextLocalModifiedMatrix._angleDeg),
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

					//변경 21.9.2 : 회전 잠금 옵션이 애니메이션과 동일하게 추가되었다.
					if(Editor._isModRotation180Lock)
					{
						curModMesh._transformMatrix.SetRotate(apUtil.AngleTo180(_tmpNextWorldMatrix._angleDeg), true);//버그 해결!
					}
					else
					{
						//회전 잠금이 없는 경우 : 각도 제한이 없다.
						curModMesh._transformMatrix.SetRotate(_tmpNextWorldMatrix._angleDeg, true);
					}
					
					

					//curModMesh.RefreshValues_Check(Editor._portrait);//필요없당.
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
					if(bone == null) { continue; }


					//추가 : 21.7.3 : 본의 World Matrix가 반전된 상태면 Delta Angle을 뒤집는다.
					float rotateBoneAngleW = deltaAngleW;					
					if(curModBone._bone._worldMatrix.Is1AxisFlipped())
					{
						rotateBoneAngleW = -deltaAngleW;
					}

					//변경 21.9.2 : 회전 잠금 옵션이 애니메이션과 동일하게 추가되었다.
					if(Editor._isModRotation180Lock)
					{
						curModBone._transformMatrix.SetRotate(apUtil.AngleTo180(curModBone._transformMatrix._angleDeg + rotateBoneAngleW), true);
					}
					else
					{
						//회전 잠금이 없는 경우
						curModBone._transformMatrix.SetRotate(curModBone._transformMatrix._angleDeg + rotateBoneAngleW, true);
					}
					
				}
			}

			//메시 그룹 업데이트
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : Transform 계열 크기값을 수정할 때
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Modifier_Transform(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			if(deltaScaleW.sqrMagnitude == 0.0f && !isFirstScale)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//|| Editor.Select.ExKey_ModParamSet == null//이전
				|| Editor.Select.ParamSetOfMod == null//변경 20.6.18 (ExKey/Value 삭제됨)
				)
			{
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			////ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			////이전
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			////변경 20.6.18 : ExKey/Value 삭제
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	return;
			//}

			////Undo
			//object targetObj = null;
			////이전
			////if (isTargetTransform) { targetObj = Editor.Select.ExValue_ModMesh; }
			////else { targetObj = Editor.Select.ModBoneOfMod; }

			////변경 20.6.18
			//if (isTargetTransform) { targetObj = Editor.Select.ModMesh_Main; }
			//else { targetObj = Editor.Select.ModBone_Main; }

			//if (isFirstScale)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, Editor, Editor.Select.Modifier, targetObj, false);
			//}


			//if (isTargetTransform)
			//{

			//	//apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;
			//	apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;

			//	apMatrix resultMatrix = null;

			//	apMatrix matx_ToParent = null;
			//	//apMatrix matx_LocalModified = null;
			//	apMatrix matx_ParentWorld = null;

			//	//if (Editor.Select.ExValue_ModMesh._isMeshTransform)
			//	if (Editor.Select.ModMesh_Main._isMeshTransform)
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_Mesh != null)
			//		if (Editor.Select.ModMesh_Main._transform_Mesh != null)
			//		{
			//			resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;

			//			matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
			//			//matx_LocalModified = targetModMesh._transform_Mesh._matrix_TF_LocalModified;
			//			matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
			//		if (Editor.Select.ModMesh_Main._transform_MeshGroup != null)
			//		{
			//			resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;

			//			matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			//matx_LocalModified = targetModMesh._transform_MeshGroup._matrix_TF_LocalModified;
			//			matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
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

			//	targetModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
			//											nextLocalModifiedMatrix._angleDeg,
			//											nextLocalModifiedMatrix._scale);

			//	targetModMesh.RefreshValues_Check(Editor._portrait);


			//	//Vector3 prevScale3 = Editor.Select.ExValue_ModMesh._transformMatrix._scale;
			//	//Vector2 prevScale = new Vector2(prevScale3.x, prevScale3.y);

			//	//apCalculatedLog.InverseResult calResult = Editor.Select.ExValue_ModMesh.CalculatedLog.World2ModLocalPos_TransformRotationScaling(0.0f, deltaScaleW);

			//	//Editor.Select.ExValue_ModMesh._transformMatrix.SetScale(prevScale + deltaScaleW);

			//	////Pos 보정
			//	//if (calResult != null && calResult._isSuccess)
			//	//{
			//	//	Editor.Select.ExValue_ModMesh._transformMatrix.SetPos(calResult._posL_next);
			//	//}

			//	//Editor.Select.ExValue_ModMesh._transformMatrix.MakeMatrix();

			//	////강제로 업데이트할 객체를 선택하고 Refresh
			//	////Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			//}
			//else if (isTargetBone)
			//{
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;

			//	//<BONE_EDIT> 하위 본도 수정 가능
			//	//if (bone._meshGroup != meshGroup)
			//	//{
			//	//	return;
			//	//}


			//	Vector3 prevScale = modBone._transformMatrix._scale;
			//	Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

			//	modBone._transformMatrix.SetScale(nextScale);
			//}
			//Editor.Select.MeshGroup.RefreshForce();
			//Editor.SetRepaint(); 
			#endregion

			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}

			//Undo
			//object targetObj = null;//삭제 21.6.30
			
			//>> [GizmoMain]
			//if(Editor.Select.ModMesh_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Gizmo_Main;
			//}
			//else if(Editor.Select.ModBone_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Gizmo_Main;
			//}



			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, 
													Editor, 
													Editor.Select.Modifier, 
													//targetObj, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}


			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;
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

					//_tmpNextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleW, true);

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//										nextLocalModifiedMatrix._angleDeg,
					//										nextLocalModifiedMatrix._scale,
					//										true); 
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
					//_tmpNextWorldMatrix._angleDeg = angleW_Prev;
					_tmpNextWorldMatrix.MakeMatrix();

					//5. 이제 Scale에 따른 Local 위치 변화가 발생했을 것이므로, Pos를 보정한다.
					_tmpNextWorldMatrix.RInverse(matx_ParentWorld, true);

					//새로 만든 RInverse와 유사하지만 상위 Matrix (A @ B = X에서 A가 아닌 B)를 추출하는 함수를 이용해서 변환된 Transform Matrix를 구하자
					_tmpNextWorldMatrix.RInverseToMid(matx_ToParent, true);

					curModMesh._transformMatrix.SetPos(_tmpNextWorldMatrix._pos, true);

					//코멘트 20.11.2 : 이렇게 하면 Default Matrix에 회전값이 포함된 경우 축이 뒤틀리는 문제가 발생한다.
					//심지어 편집이 힘들 수 있는데, 해결법을 모르겠다.


					//curModMesh.RefreshValues_Check(Editor._portrait);//필요없당.
				}
			}



			//2. ModBone 수정
			if(nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;
				
				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }
					
					bone = curModBone._bone;
					if(bone == null) { continue; }

					Vector3 prevScale = curModBone._transformMatrix._scale;
					Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

					curModBone._transformMatrix.SetScale(nextScale, true);
				}
			}

			//메시 그룹 갱신
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}





		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 위치값 [Position]
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="depth"></param>
		public void TransformChanged_Position__Modifier_Transform(Vector2 pos)
		{
			//Depth는 무시한다.

			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ParamSet이 없으면 패스
			if (Editor.Select.ParamSetOfMod == null //변경 20.6.18 ExKey 삭제
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			////이전
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			////변경 20.6.18
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	return;
			//}

			////새로 ModMesh의 Matrix 값을 만들어주자 //TODO : 해당 Matrix를 사용하는 Modifier 구현 필요
			////Undo
			//object targetObj = null;

			////이전
			////if (isTargetTransform)	{ targetObj = Editor.Select.ExValue_ModMesh; }
			////else						{ targetObj = Editor.Select.ModBoneOfMod; }

			////변경 20.6.18 
			//if (isTargetTransform)	{ targetObj = Editor.Select.ModMesh_Main; }
			//else					{ targetObj = Editor.Select.ModBone_Main; }

			//apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveTransform, Editor, Editor.Select.Modifier, targetObj, false);

			//if (isTargetTransform)
			//{
			//	apMatrix localStaticMatrix = null;
			//	apMatrix resultMatrix = null;

			//	//if (Editor.Select.ExValue_ModMesh._isMeshTransform)
			//	if (Editor.Select.ModMesh_Main._isMeshTransform)
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_Mesh != null)
			//		if (Editor.Select.ModMesh_Main._transform_Mesh != null)
			//		{
			//			//localStaticMatrix = Editor.Select.ExValue_ModMesh._transform_Mesh._matrix;
			//			//resultMatrix = Editor.Select.ExValue_ModMesh._transform_Mesh._matrix_TFResult_World;

			//			localStaticMatrix = Editor.Select.ModMesh_Main._transform_Mesh._matrix;
			//			resultMatrix = Editor.Select.ModMesh_Main._transform_Mesh._matrix_TFResult_World;
			//		}
			//	}
			//	else
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
			//		if (Editor.Select.ModMesh_Main._transform_MeshGroup != null)
			//		{
			//			//localStaticMatrix = Editor.Select.ExValue_ModMesh._transform_MeshGroup._matrix;
			//			//resultMatrix = Editor.Select.ExValue_ModMesh._transform_MeshGroup._matrix_TFResult_World;

			//			localStaticMatrix = Editor.Select.ModMesh_Main._transform_MeshGroup._matrix;
			//			resultMatrix = Editor.Select.ModMesh_Main._transform_MeshGroup._matrix_TFResult_World;
			//		}
			//	}

			//	if (localStaticMatrix == null || resultMatrix == null)
			//	{
			//		return;
			//	}


			//	//>> 직접 적용한다.
			//	//Editor.Select.ExValue_ModMesh._transformMatrix.SetPos(pos);
			//	Editor.Select.ModMesh_Main._transformMatrix.SetPos(pos);//변경 20.6.18

			//	Editor.SetRepaint();

			//	//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			//}
			//else if (isTargetBone)
			//{
			//	//Bone 움직임을 제어하자.
			//	//ModBone에 값을 넣는다.
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;

			//	//<BONE_EDIT> 하위 본도 수정 가능
			//	//if (bone._meshGroup != meshGroup)
			//	//{
			//	//	return;
			//	//}

			//	//그냥 직접 제어합니다.
			//	modBone._transformMatrix.SetPos(pos);


			//}
			//Editor.Select.MeshGroup.RefreshForce(true); 
			#endregion


			
			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}

			//선택된 메인 오브젝트의 위치를 기준으로 deltaPos를 구한다.
			Vector2 deltaPos = Vector2.zero;

			//새로 ModMesh의 Matrix 값을 만들어주자
			//Undo
			object targetObj = null;
			
			//if(Editor.Select.ModMesh_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Main;
			//	deltaPos = pos - Editor.Select.ModMesh_Main._transformMatrix._pos;
			//}
			//else if(Editor.Select.ModBone_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Main;
			//	deltaPos = pos - Editor.Select.ModBone_Main._transformMatrix._pos;
			//}


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




			if(targetObj == null)
			{
				return;
			}

			
			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_MoveTransform, 
												Editor, 
												Editor.Select.Modifier, 
												//targetObj, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);


			
			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;


				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null)
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
			if(nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;
				apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.17
				
				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }
					
					bone = curModBone._bone;
					if(bone == null) { continue; }

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
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}




		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 위치값 [Rotation]
		/// </summary>
		/// <param name="angle"></param>
		public void TransformChanged_Rotate__Modifier_Transform(float angle)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			//if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || Editor.Select.ExKey_ModParamSet == null)
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				
				//|| Editor.Select.ExKey_ModParamSet == null
				|| Editor.Select.ParamSetOfMod == null//변경 20.6.18
				)
			{
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			////ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			////이전
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;
			////변경 20.6.18 : ExKey/Value 삭제
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	return;
			//}

			////Undo
			//object targetObj = null;
			////이전
			////if (isTargetTransform) { targetObj = Editor.Select.ExValue_ModMesh; }
			////else { targetObj = Editor.Select.ModBoneOfMod; }

			////변경 20.6.18
			//if (isTargetTransform)	{ targetObj = Editor.Select.ModMesh_Main; }
			//else					{ targetObj = Editor.Select.ModBone_Main; }

			//apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, Editor, Editor.Select.Modifier, targetObj, false);

			//if (isTargetTransform)
			//{
			//	//이전
			//	//Editor.Select.ExValue_ModMesh._transformMatrix.SetRotate(angle);
			//	//Editor.Select.ExValue_ModMesh._transformMatrix.MakeMatrix();

			//	//변경 20.6.18
			//	Editor.Select.ModMesh_Main._transformMatrix.SetRotate(angle);
			//	Editor.Select.ModMesh_Main._transformMatrix.MakeMatrix();

			//	//강제로 업데이트할 객체를 선택하고 Refresh
			//	//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			//}
			//else if (isTargetBone)
			//{
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;

			//	//<BONE_EDIT> 하위 본도 수정 가능
			//	//if (bone._meshGroup != meshGroup)
			//	//{
			//	//	return;
			//	//}

			//	//직접 적용한다.
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

			//}
			//Editor.Select.MeshGroup.RefreshForce(); 
			#endregion


			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}

			//선택된 메인 오브젝트의 위치를 기준으로 deltaAngle을 구한다.
			float deltaAngle = 0.0f;

			//새로 ModMesh의 Matrix 값을 만들어주자
			//Undo
			object targetObj = null;
			

			//if(Editor.Select.ModMesh_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Main;
			//	deltaAngle = angle - Editor.Select.ModMesh_Main._transformMatrix._angleDeg;
			//}
			//else if(Editor.Select.ModBone_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Main;
			//	deltaAngle = angle - Editor.Select.ModBone_Main._transformMatrix._angleDeg;
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



			if(targetObj == null)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, 
												Editor, 
												Editor.Select.Modifier, 
												//targetObj, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);




			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;

				apMatrix matx_ToParent = null;
				apMatrix matx_ParentWorld = null;

				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null) { continue; }


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
			if(nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;
				apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.17
				
				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }
					
					bone = curModBone._bone;
					if(bone == null) { continue; }

					float nextAngle = 0.0f;
					if (curModBone == targetObj)
					{
						//메인이라면 바로 할당
						nextAngle = angle;
					}
					else
					{
						//서브라면 deltaPos를 더한다.
						nextAngle = curModBone._transformMatrix._angleDeg + deltaAngle;
					}

					//삭제 : 각도 제한이 좀 이상하다.
					////Bone IK가 켜진 경우 각도 제한
					////직접 적용한다.
					//if (bone._isIKAngleRange)
					//{
					//	if (nextAngle < bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower)
					//	{
					//		nextAngle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower;
					//	}
					//	else if (nextAngle > bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper)
					//	{
					//		nextAngle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper;
					//	}
					//}

					curModBone._transformMatrix.SetRotate(nextAngle, true);
				}
			}


			//전체 갱신하자
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}



		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 위치값 [Scale]
		/// </summary>
		/// <param name="scale"></param>
		public void TransformChanged_Scale__Modifier_Transform(Vector2 scale)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				|| Editor.Select.ParamSetOfMod == null//변경 20.6.18
				)
			{
				return;
			}

			#region [미사용 코드] 단일 선택 및 처리
			////ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			////이전
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			////변경 20.6.18
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	return;
			//}


			////Undo
			//object targetObj = null;
			////이전
			////if (isTargetTransform) { targetObj = Editor.Select.ExValue_ModMesh; }
			////else { targetObj = Editor.Select.ModBoneOfMod; }
			////변경 20.6.18
			//if (isTargetTransform)	{ targetObj = Editor.Select.ModMesh_Main; }
			//else					{ targetObj = Editor.Select.ModBone_Main; }

			//apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, Editor, Editor.Select.Modifier, targetObj, false);

			//if (isTargetTransform)
			//{
			//	//이전
			//	//Editor.Select.ExValue_ModMesh._transformMatrix.SetScale(scale);
			//	//Editor.Select.ExValue_ModMesh._transformMatrix.MakeMatrix();

			//	//변경 20.6.18
			//	Editor.Select.ModMesh_Main._transformMatrix.SetScale(scale);
			//	Editor.Select.ModMesh_Main._transformMatrix.MakeMatrix();


			//	//강제로 업데이트할 객체를 선택하고 Refresh
			//	//Editor.Select.MeshGroup.AddForceUpdateTarget(Editor.Select.ExValue_ModMesh._renderUnit);
			//}
			//else if (isTargetBone)
			//{
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.18

			//	//<BONE_EDIT> 하위 본이라도 수정할 수 있다.
			//	//if (bone._meshGroup != meshGroup)
			//	//{
			//	//	return;
			//	//}


			//	//직접 적용
			//	modBone._transformMatrix.SetScale(scale);
			//}
			//Editor.Select.MeshGroup.RefreshForce(); 
			#endregion


			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return;
			}

			//선택된 메인 오브젝트의 위치를 기준으로 deltaScale을 구한다.
			//deltaScale은 비율로 구한다.
			Vector2 deltaScale = Vector2.one;
			Vector2 mainScale = Vector2.one;
			//새로 ModMesh의 Matrix 값을 만들어주자
			//Undo
			object targetObj = null;
			

			//if(Editor.Select.ModMesh_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Main;
			//	mainScale = Editor.Select.ModMesh_Main._transformMatrix._scale;
			//}
			//else if(Editor.Select.ModBone_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Main;
			//	mainScale = Editor.Select.ModBone_Main._transformMatrix._scale;
			//}


			//>> [GizmoMain]
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



			if(targetObj == null)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, 
												Editor, 
												Editor.Select.Modifier, 
												//targetObj, 
												false,
												apEditorUtil.UNDO_STRUCT.ValueOnly);

			deltaScale.x = (mainScale.x != 0.0f) ? (scale.x / mainScale.x) : 1.0f;
			deltaScale.y = (mainScale.y != 0.0f) ? (scale.y / mainScale.y) : 1.0f;

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

					if (curModMesh == null)
					{ continue; }

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
			if(nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;
				apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.17
				
				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }
					
					bone = curModBone._bone;
					if(bone == null) { continue; }

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
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}



		


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 색상값 [Color]
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Modifier_Transform(Color color, bool isVisible)
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//|| Editor.Select.ExValue_ModMesh == null || Editor.Select.ExKey_ModParamSet == null//이전
				|| Editor.Select.ModMesh_Main == null 
				|| Editor.Select.ModMesh_Gizmo_Main == null
				|| Editor.Select.ParamSetOfMod == null//변경 20.6.18 : ExKey/Value 삭제
				)
			{
				return;
			}


			//변경 20.6.28 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;

			if(nModMeshes == 0)
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

			#region [미사용 코드] 단일 선택 및 처리
			////이전
			////Editor.Select.ExValue_ModMesh._meshColor = color;
			////Editor.Select.ExValue_ModMesh._isVisible = isVisible;

			////변경 20.6.18 ExValue 삭제
			//Editor.Select.ModMesh_Main._meshColor = color;
			//Editor.Select.ModMesh_Main._isVisible = isVisible; 
			#endregion


			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;


				for (int iModMesh = 0; iModMesh < nModMeshes; iModMesh++)
				{
					curModMesh = modMeshes_Gizmo[iModMesh];

					if (curModMesh == null) { continue; }

					curModMesh._meshColor = color;
					curModMesh._isVisible = isVisible;
				}
			}

			//강제로 업데이트할 객체를 선택하고 Refresh
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
		}


		public void TransformChanged_Extra__Modifier_Transform()
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return;
			}

			//Extra는 기즈모 메인이 아닌 그냥 메인 ModMesh에 대해서 적용한다.

			//Editing 상태가 아니면 패스 + ModMesh가 없다면 패스
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 
				//변경 20.6.18
				|| Editor.Select.ModMesh_Main == null 
				|| Editor.Select.ParamSetOfMod == null
				|| Editor.Select.RenderUnitOfMod_Main == null
				)
			{
				return;
			}

			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			apModifierBase modifier = Editor.Select.Modifier;
			//apModifiedMesh modMesh = Editor.Select.ExValue_ModMesh;
			//apRenderUnit renderUnit = Editor.Select.RenderUnitOfMod;

			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;
			apRenderUnit renderUnit = Editor.Select.RenderUnitOfMod_Main;

			if(!modifier._isExtraPropertyEnabled)
			{
				return;
			}

			//Extra Option을 제어하는 Dialog를 호출하자
			//이전
			//apDialog_ExtraOption.ShowDialog(Editor, Editor._portrait, meshGroup, modifier, modMesh, renderUnit, false, null, null);

			//변경 21.10.2
			apDialog_ExtraOption.ShowDialog_Modifier(Editor, Editor._portrait, meshGroup, modifier, renderUnit, Editor.Select.RenderUnitOfMod_All, Editor.Select.ModMeshes_All);
		}



		public apGizmos.TransformParam PivotReturn__Modifier_Transform()
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return null;
			}

			bool isTransformPivot = false;
			bool isBonePivot = false;

			//Editing 상태가 아니면..
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				return null;
			}


			//기즈모 메인과 별개로, 메인이 선택되지 않았다면 기즈모는 동작하지 않는다.
			//(그런 일이 있나..) 
			if(Editor.Select.ModMesh_Main == null && Editor.Select.ModBone_Main == null)
			{
				return null;
			}

			//이전
			//if (Editor.Select.Modifier.IsTarget_Bone && Editor.Select.Bone != null && Editor.Select.ModBone_Main != null)
			//{
			//	isBonePivot = true;
			//}

			//>> [GizmoMain] >>
			apModifiedBone mainModBone = null;
			apModifiedMesh mainModMesh = null;

			if (Editor.Select.Modifier.IsTarget_Bone 
				&& Editor.Select.ModBone_Gizmo_Main != null
				&& Editor.Select.Bone_Mod_Gizmo != null
				)
			{
				mainModBone = Editor.Select.ModBone_Gizmo_Main;
				isBonePivot = true;
			}

			//이전
			//if (Editor.Select.ModMesh_Main != null && Editor.Select.ModMesh_Main._renderUnit != null)//변경 20.6.18
			//{
			//	//이전
			//	//if (Editor.Select.ExValue_ModMesh._renderUnit._meshTransform != null ||
			//	//	Editor.Select.ExValue_ModMesh._renderUnit._meshGroupTransform != null)
				
			//	//변경 20.6.18
			//	if (Editor.Select.ModMesh_Main._renderUnit._meshTransform != null ||
			//		Editor.Select.ModMesh_Main._renderUnit._meshGroupTransform != null)
			//	{
			//		isTransformPivot = true;
			//	}
			//}

			//>> [GizmoMain] >>
			if (Editor.Select.ModMesh_Gizmo_Main != null 
				&& Editor.Select.ModMesh_Gizmo_Main._renderUnit != null)
			{
				mainModMesh = Editor.Select.ModMesh_Gizmo_Main;

				if (mainModMesh._renderUnit._meshTransform != null ||
					mainModMesh._renderUnit._meshGroupTransform != null)
				{	
					isTransformPivot = true;
				}
			}


			//둘다 없으면 Null
			if (!isTransformPivot && !isBonePivot)
			{
				return null;
			}

			//Editing 상태가 아니면

			if (isTransformPivot)
			{
				apMatrix resultMatrix = null;

				//기존 방식
				//int transformDepth = Editor.Select.ModMesh_Main._renderUnit.GetDepth();

				//if (Editor.Select.ModMesh_Main._renderUnit._meshTransform != null)
				//{
				//	resultMatrix = Editor.Select.ModMesh_Main._renderUnit._meshTransform._matrix_TFResult_World;
				//}
				//else if (Editor.Select.ModMesh_Main._renderUnit._meshGroupTransform != null)
				//{
				//	resultMatrix = Editor.Select.ModMesh_Main._renderUnit._meshGroupTransform._matrix_TFResult_World;
				//}
				//else
				//{
				//	return null;
				//}


				//>> [GizmoMain] >>
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


				Vector2 worldPos = resultMatrix._pos;
				float worldAngle = resultMatrix._angleDeg;
				Vector2 worldScale = resultMatrix._scale;

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;
				if (Editor.Select.Modifier._isColorPropertyEnabled)
				{
					paramType |= apGizmos.TRANSFORM_UI.Color;//<<칼라 옵션이 있는 경우에!
				}
				if (Editor.Select.Modifier._isExtraPropertyEnabled)
				{
					//추가 : Extra 옵션이 있다면 Gizmo 이벤트에도 추가
					paramType |= apGizmos.TRANSFORM_UI.Extra;
				}

				return apGizmos.TransformParam.Make(
					worldPos,
					worldAngle,
					worldScale,
					transformDepth,

					mainModMesh._meshColor,
					mainModMesh._isVisible,

					resultMatrix.MtrxToSpace,
					false, paramType,

					mainModMesh._transformMatrix._pos,
					mainModMesh._transformMatrix._angleDeg,
					mainModMesh._transformMatrix._scale
					);
			}

			if (isBonePivot)
			{
				//삭제
				//apBone bone = Editor.Select.Bone;
				//>> [GizmoMain] >>
				apBone bone = mainModBone._bone;//<<이게 정확하겠다.
				

				if(Editor._boneGUIRenderMode == apEditor.BONE_RENDER_MODE.None)
				{
					//Bone GUI모드가 꺼져있으면 안보인다.
					return null;
				}

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;

				if(bone._IKController._controllerType != apBoneIKController.CONTROLLER_TYPE.None)
				{
					paramType |= apGizmos.TRANSFORM_UI.BoneIKController;
				}

				return apGizmos.TransformParam.Make(
					bone._worldMatrix.Pos,
					bone._worldMatrix.Angle,
					bone._worldMatrix.Scale,
					0, 
					bone._color,
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

		// 키보드 입력
		//-----------------------------------------------------------------------------------------
		private bool OnHotKeyEvent__Modifier_Transform__Keyboard_Move(Vector2 deltaMoveW, bool isFirstMove)
		{
			//Move__Modifier_Transform 함수의 코드를 이용
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Modifier == null
				
				//|| Editor.Select.ExKey_ModParamSet == null
				|| Editor.Select.ParamSetOfMod == null//변경 20.6.18

				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				)
			{
				return false;
			}

			#region [미사용 코드] 단일 선택 및 처리
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			////변경 20.6.18 : ExKey 삭제
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	return false;
			//}

			////새로 ModMesh의 Matrix 값을 만들어주자 //TODO : 해당 Matrix를 사용하는 Modifier 구현 필요
			////Undo
			//object targetObj = null;

			////if (isTargetTransform)	{ targetObj = Editor.Select.ExValue_ModMesh; }
			////else						{ targetObj = Editor.Select.ModBoneOfMod; }

			////변경 20.6.18 ExKey 삭제
			//if (isTargetTransform)	{ targetObj = Editor.Select.ModMesh_Main; }
			//else					{ targetObj = Editor.Select.ModBone_Main; }

			//if (isFirstMove)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_MoveTransform, Editor, Editor.Select.Modifier, targetObj, false);
			//}

			//if (isTargetTransform)
			//{
			//	//기존의 Pivot이 포함된 Marix를 확인해야한다.
			//	//여기서 제어하는 건 Local 값이지만, 노출되는건 World이다.
			//	//값이 이상해질테니 수정을 하자

			//	//apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;
			//	apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;//변경 20.6.18

			//	apMatrix resultMatrix = null;

			//	apMatrix matx_ToParent = null;
			//	apMatrix matx_ParentWorld = null;

			//	//if (Editor.Select.ExValue_ModMesh._isMeshTransform)
			//	if (Editor.Select.ModMesh_Main._isMeshTransform)
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_Mesh != null)
			//		if (Editor.Select.ModMesh_Main._transform_Mesh != null)
			//		{
			//			resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;

			//			matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
			//		if (Editor.Select.ModMesh_Main._transform_MeshGroup != null)
			//		{
			//			resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;
			//			matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if (resultMatrix == null)
			//	{
			//		return false;
			//	}


			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetPos(resultMatrix._pos + deltaMoveW);

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	//TRS중에 Pos만 넣어도 된다.
			//	targetModMesh._transformMatrix.SetPos(nextLocalModifiedMatrix._pos);

			//	targetModMesh.RefreshValues_Check(Editor._portrait);


			//	Editor.Select.MeshGroup.RefreshForce();

			//	Editor.SetRepaint();
			//}
			//else if (isTargetBone)
			//{
			//	//Bone 움직임을 제어하자.
			//	//ModBone에 값을 넣는다.
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.18

			//	//Move로 제어 가능한 경우는
			//	//1. IK Tail일 때
			//	//2. Root Bone일때 (절대값)
			//	if (bone._isIKTail)
			//	{
			//		float weight = 1.0f;

			//		if (bone != _prevSelected_TransformBone || isFirstMove)
			//		{
			//			_prevSelected_TransformBone = bone;
			//			_prevSelected_MousePosW = bone._worldMatrix._pos;
			//		}

			//		_prevSelected_MousePosW += deltaMoveW;
			//		Vector2 bonePosW = _prevSelected_MousePosW;//DeltaPos + 절대 위치 절충

			//		apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

			//		if (limitedHeadBone == null)
			//		{
			//			return false;
			//		}

			//		apBone curBone = bone;
			//		//위로 올라가면서 IK 결과값을 Default에 적용하자

			//		while (true)
			//		{
			//			float deltaAngle = curBone._IKRequestAngleResult;

			//			apModifiedBone targetModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
			//			{
			//				return a._bone == curBone;
			//			});
			//			if (targetModBone != null)
			//			{
			//				float nextAngle = curBone._localMatrix._angleDeg + deltaAngle;
			//				while (nextAngle < -180.0f)
			//				{
			//					nextAngle += 360.0f;
			//				}
			//				while (nextAngle > 180)
			//				{
			//					nextAngle -= 360.0f;
			//				}

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

			//		apMatrix newWorldMatrix = new apMatrix(bone._worldMatrix);
			//		newWorldMatrix.SetPos(newWorldMatrix._pos + deltaMoveW);

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

			//	Editor.Select.MeshGroup.RefreshForce();//<<이거 필수. 이게 있어야 이번 Repaint에서 바로 적용이 된다.
			//}

			//return true; 
			#endregion


			//변경 20.6.28 : 다중 처리
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return false;
			}

			//새로 ModMesh의 Matrix 값을 만들어주자 //TODO : 해당 Matrix를 사용하는 Modifier 구현 필요
			//Undo
			//object targetObj = null;//삭제 21.6.30
			
			
			//>> [GizmoMain]
			//if(Editor.Select.ModMesh_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Gizmo_Main;
			//}
			//else if(Editor.Select.ModBone_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Gizmo_Main;
			//}


			if (isFirstMove)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_MoveTransform, 
													Editor,
													Editor.Select.Modifier,
													//targetObj,
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
			}



			//1. ModMesh 수정
			if (nModMeshes > 0)
			{
				apModifiedMesh curModMesh = null;
				//apMatrix resultMatrix = null;
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

					matx_ToParent.MakeMatrix();
					matx_ParentWorld.MakeMatrix();


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


			//2. ModBone 수정
			if(nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;
				apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.17
				
				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }
					
					bone = curModBone._bone;
					if(bone == null) { continue; }
					

					//Move로 제어 가능한 경우는
					//1. IK Tail일 때
					//2. Root Bone일때 (절대값)
					if (bone._isIKTail)
					{
						//Debug.Log("Request IK : " + _boneSelectPosW);
						float weight = 1.0f;

						Vector2 bonePosW = Vector2.zero;


						//이게 상대 좌표로만 계산
						bonePosW = bone._worldMatrix.Pos + deltaMoveW;

						//Vector2 bonePosW = bone._worldMatrix._pos + deltaMoveW;//DeltaPos 이용
						//Vector2 bonePosW = curMousePosW;//절대 위치 이용

						//IK 계산
						apBone limitedHeadBone = bone.RequestIK_Limited(bonePosW, weight, true, Editor.Select.ModRegistableBones);

						if (limitedHeadBone == null)
						{
							continue;
						}


						apBone curIKBone = bone;
						//위로 올라가면서 IK 결과값을 Default에 적용하자

						while (true)
						{
							float deltaAngle = curIKBone._IKRequestAngleResult;

							//추가 20.10.9 : 부모 본이 있거나 부모 렌더 유닛의 크기가 한축만 뒤집혔다면 deltaAngle을 반전하자
							if(curIKBone.IsNeedInvertIKDeltaAngle_Gizmo())
							{
								deltaAngle = -deltaAngle;
							}


							//if(Mathf.Abs(deltaAngle) > 30.0f)
							//{
							//	deltaAngle *= deltaAngle * 0.1f;
							//}

							apModifiedBone targetIKModBone = paramSet._boneData.Find(delegate (apModifiedBone a)
							{
								return a._bone == curIKBone;
							});
							if (targetIKModBone != null)
							{
								//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
								//해당 Bone의 ModBone을 가져온다.

								float nextAngle = apUtil.AngleTo180(curIKBone._localMatrix._angleDeg + deltaAngle);
								
								//DefaultMatrix말고
								//curBone._defaultMatrix.SetRotate(nextAngle);

								//ModBone을 수정하자
								targetIKModBone._transformMatrix.SetRotate(nextAngle, true);
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

						//-----------------------------------
						// 기존 방식
						//-----------------------------------
						//apMatrix parentMatrix = null;
						//if (bone._parentBone == null)
						//{
						//	if (bone._renderUnit != null)
						//	{
						//		//Render Unit의 World Matrix를 참조하여
						//		//로컬 값을 Default로 적용하자
						//		parentMatrix = bone._renderUnit.WorldMatrixWrap;
						//	}
						//}
						//else
						//{
						//	parentMatrix = bone._parentBone._worldMatrix;
						//}

						////apMatrix localMatrix = bone._localMatrix;
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

						

						//--------------------------------------
						// 변경된 방식 20.8.19 : BoneWorldMatrix를 이용
						//--------------------------------------
						apBoneWorldMatrix parentMatrix = apBoneWorldMatrix.MakeTempParentWorldMatrix(
																	0, Editor._portrait,
																	(bone._parentBone != null ? bone._parentBone._worldMatrix : null),
																	(bone._renderUnit != null ? bone._renderUnit.WorldMatrixWrap : null));

						apBoneWorldMatrix nextWorldMatrix = apBoneWorldMatrix.MakeTempWorldMatrix(1, Editor._portrait, bone._worldMatrix);
						nextWorldMatrix.MoveAsResult(deltaMoveW);
						nextWorldMatrix.SetWorld2Default(parentMatrix);

						//WorldMatrix에서 Local Space에 위치한 Matrix는
						//Default + Local + RigTest이다.

						//이전
						//_tmpNextWorldMatrix.Subtract(bone._defaultMatrix);
						//if (bone._isRigTestPosing)
						//{
						//	_tmpNextWorldMatrix.Subtract(bone._rigTestMatrix);
						//}

						//curModBone._transformMatrix.SetPos(_tmpNextWorldMatrix._pos);

						//변경 20.8.19 : 래핑된 코드
						nextWorldMatrix.SubtractAsLocalValue(bone._defaultMatrix, false);
						if (bone._isRigTestPosing)
						{
							nextWorldMatrix.SubtractAsLocalValue(bone._rigTestMatrix, false);
						}
						nextWorldMatrix.MakeMatrix(false);
						curModBone._transformMatrix.SetPos(nextWorldMatrix.Pos, true);
					}
				}
			}

			//전체 갱신하자
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();

			return true;
		}

		private bool OnHotKeyEvent__Modifier_Transform__Keyboard_Rotate(float deltaAngleW, bool isFirstRotate)
		{
			//Rotate__Modifier_Transform 함수의 코드 이용

			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Modifier == null
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None 

				//|| Editor.Select.ExKey_ModParamSet == null
				|| Editor.Select.ParamSetOfMod == null
				)
			{
				return false;
			}


			#region [미사용 코드] 단일 선택 및 처리
			////ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			////변경 20.6.18 : ExKey 삭제
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	return false;
			//}

			////Undo
			//object targetObj = null;

			////if (isTargetTransform) { targetObj = Editor.Select.ExValue_ModMesh; }
			////else { targetObj = Editor.Select.ModBoneOfMod; }

			////변경 20.6.18
			//if (isTargetTransform)	{ targetObj = Editor.Select.ModMesh_Main; }
			//else					{ targetObj = Editor.Select.ModBone_Main; }

			//if (isFirstRotate)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, Editor, Editor.Select.Modifier, targetObj, false);
			//}

			//if (isTargetTransform)
			//{
			//	//apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;
			//	apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;

			//	apMatrix resultMatrix = null;
			//	apMatrix matx_ToParent = null;
			//	apMatrix matx_ParentWorld = null;

			//	if(targetModMesh._isMeshTransform)
			//	{
			//		if(targetModMesh._transform_Mesh != null)
			//		{
			//			//Mesh Transform
			//			resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;
			//			matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		//if(Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
			//		if(Editor.Select.ModMesh_Main._transform_MeshGroup != null)
			//		{
			//			//Mesh Group Transform
			//			resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;
			//			matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if(resultMatrix == null)
			//	{
			//		return false;
			//	}

			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW);//각도 변경

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	targetModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
			//											apUtil.AngleTo180(nextLocalModifiedMatrix._angleDeg),
			//											targetModMesh._transformMatrix._scale);
			//}
			//else if (isTargetBone)
			//{
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;

			//	//Default Angle은 -180 ~ 180 범위 안에 들어간다.
			//	float nextAngle = modBone._transformMatrix._angleDeg + deltaAngleW;

			//	while (nextAngle < -180.0f)
			//	{
			//		nextAngle += 360.0f;
			//	}
			//	while (nextAngle > 180.0f)
			//	{
			//		nextAngle -= 360.0f;
			//	}

			//	modBone._transformMatrix.SetRotate(nextAngle);
			//}
			////강제로 업데이트할 객체를 선택하고 Refresh

			//Editor.Select.MeshGroup.RefreshForce(); 
			#endregion


			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return false;
			}

			//Undo
			//object targetObj = null;//삭제 21.6.30
			

			//>> [GizmoMain]
			//if(Editor.Select.ModMesh_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Gizmo_Main;
			//}
			//else if(Editor.Select.ModBone_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Gizmo_Main;
			//}


			if (isFirstRotate)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_RotateTransform, 
													Editor, 
													Editor.Select.Modifier, 
													//targetObj, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
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

					//_tmpNextWorldMatrix.SetRotate(resultMatrix._angleDeg + deltaAngleW, true);//각도 변경

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//									apUtil.AngleTo180(nextLocalModifiedMatrix._angleDeg),
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


					//변경 21.9.2 : 회전 잠금 옵션이 애니메이션과 동일하게 추가되었다.
					if (Editor._isModRotation180Lock)
					{
						curModMesh._transformMatrix.SetRotate(apUtil.AngleTo180(_tmpNextWorldMatrix._angleDeg), true);//버그 해결!
					}
					else
					{
						//회전 제한이 없다.
						curModMesh._transformMatrix.SetRotate(_tmpNextWorldMatrix._angleDeg, true);
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
					if(bone == null) { continue; }


					//추가 : 21.7.3 : 본의 World Matrix가 반전된 상태면 Delta Angle을 뒤집는다.
					float rotateBoneAngleW = deltaAngleW;					
					if(curModBone._bone._worldMatrix.Is1AxisFlipped())
					{
						rotateBoneAngleW = -deltaAngleW;
					}

					//변경 21.9.2 : 회전 잠금 옵션이 애니메이션과 동일하게 추가되었다.
					if (Editor._isModRotation180Lock)
					{
						curModBone._transformMatrix.SetRotate(apUtil.AngleTo180(curModBone._transformMatrix._angleDeg + rotateBoneAngleW), true);
					}
					else
					{
						//회전 제한이 없다.
						curModBone._transformMatrix.SetRotate(curModBone._transformMatrix._angleDeg + rotateBoneAngleW, true);
					}
				}
			}

			//메시 그룹 업데이트
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();


			return true;
		}





		private bool OnHotKeyEvent__Modifier_Transform__Keyboard_Scale(Vector2 deltaScaleL, bool isFirstScale)
		{
			//Scale__Modifier_Transform 함수의 코드를 이용

			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Modifier == null
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None

				//|| Editor.Select.ExKey_ModParamSet == null
				|| Editor.Select.ParamSetOfMod == null//변경
				)
			{
				return false;
			}

			#region [미사용 코드] 단일 선택 및 처리
			////ModMesh와 ModBone을 선택했는지 여부 확인후 처리한다.
			////bool isTargetTransform = Editor.Select.ExValue_ModMesh != null;
			////bool isTargetBone = Editor.Select.Bone != null
			////						&& Editor.Select.ModBoneOfMod != null
			////						&& Editor.Select.Modifier.IsTarget_Bone
			////						&& Editor.Select.ModBoneOfMod._bone == Editor.Select.Bone;

			////변경 20.6.18 ExKey/Value 삭제
			//bool isTargetTransform = Editor.Select.ModMesh_Main != null;
			//bool isTargetBone = Editor.Select.Bone != null
			//						&& Editor.Select.ModBone_Main != null
			//						&& Editor.Select.Modifier.IsTarget_Bone
			//						&& Editor.Select.ModBone_Main._bone == Editor.Select.Bone;

			////우선 순위는 ModMesh
			//if (!isTargetTransform && !isTargetBone)
			//{
			//	return false;
			//}

			////Undo
			//object targetObj = null;
			////if (isTargetTransform)	{ targetObj = Editor.Select.ExValue_ModMesh; }
			////else					{ targetObj = Editor.Select.ModBoneOfMod; }

			////변경 20.6.18
			//if (isTargetTransform)	{ targetObj = Editor.Select.ModMesh_Main; }
			//else					{ targetObj = Editor.Select.ModBone_Main; }

			//if (isFirstScale)
			//{
			//	apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, Editor, Editor.Select.Modifier, targetObj, false);
			//}


			//if (isTargetTransform)
			//{
			//	//apModifiedMesh targetModMesh = Editor.Select.ExValue_ModMesh;
			//	apModifiedMesh targetModMesh = Editor.Select.ModMesh_Main;//변경 20.6.18

			//	apMatrix resultMatrix = null;

			//	apMatrix matx_ToParent = null;
			//	apMatrix matx_ParentWorld = null;

			//	//if (Editor.Select.ExValue_ModMesh._isMeshTransform)
			//	if (Editor.Select.ModMesh_Main._isMeshTransform)
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_Mesh != null)
			//		if (Editor.Select.ModMesh_Main._transform_Mesh != null)
			//		{
			//			resultMatrix = targetModMesh._transform_Mesh._matrix_TFResult_World;

			//			matx_ToParent = targetModMesh._transform_Mesh._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_Mesh._matrix_TF_ParentWorld;
			//		}
			//	}
			//	else
			//	{
			//		//if (Editor.Select.ExValue_ModMesh._transform_MeshGroup != null)
			//		if (Editor.Select.ModMesh_Main._transform_MeshGroup != null)
			//		{
			//			resultMatrix = targetModMesh._transform_MeshGroup._matrix_TFResult_World;

			//			matx_ToParent = targetModMesh._transform_MeshGroup._matrix_TF_ToParent;
			//			matx_ParentWorld = targetModMesh._transform_MeshGroup._matrix_TF_ParentWorld;
			//		}
			//	}

			//	if (resultMatrix == null)
			//	{
			//		return false;
			//	}

			//	//기본 코드 : World 2 Local
			//	apMatrix nextWorldMatrix = new apMatrix(resultMatrix);
			//	nextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleL);

			//	//ToParent x LocalModified x ParentWorld = Result
			//	//LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
			//	apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(nextWorldMatrix, matx_ParentWorld));

			//	targetModMesh._transformMatrix.SetTRS(nextLocalModifiedMatrix._pos,
			//											nextLocalModifiedMatrix._angleDeg,
			//											nextLocalModifiedMatrix._scale);

			//	//키보드에선 바로 Local 값을 바로 수정한다.
			//	//targetModMesh._transformMatrix.SetScale(targetModMesh._transformMatrix._scale + deltaScaleL);

			//	targetModMesh.RefreshValues_Check(Editor._portrait);
			//}
			//else if (isTargetBone)
			//{
			//	apModifiedBone modBone = Editor.Select.ModBone_Main;
			//	apBone bone = Editor.Select.Bone;
			//	apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//	//apModifierParamSet paramSet = Editor.Select.ExKey_ModParamSet;
			//	apModifierParamSet paramSet = Editor.Select.ParamSetOfMod;//변경 20.6.18

			//	Vector3 prevScale = modBone._transformMatrix._scale;
			//	Vector2 nextScale = new Vector2(prevScale.x + deltaScaleL.x, prevScale.y + deltaScaleL.y);

			//	modBone._transformMatrix.SetScale(nextScale);
			//}
			//Editor.Select.MeshGroup.RefreshForce();
			//Editor.SetRepaint(); 
			#endregion


			//변경 20.6.27 : 다중 처리
			//ModMesh 리스트와 ModBone 리스트를 모두 돌아서 처리하자
			//기즈모용 리스트를 받아와서 사용한다.
			List<apModifiedMesh> modMeshes_Gizmo = Editor.Select.ModMeshes_Gizmo_All;
			List<apModifiedBone> modBones_Gizmo = Editor.Select.ModBones_Gizmo_All;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			int nModMeshes = (modMeshes_Gizmo != null) ? modMeshes_Gizmo.Count : 0;
			int nModBones = (modBones_Gizmo != null && Editor.Select.Modifier.IsTarget_Bone) ? modBones_Gizmo.Count : 0;

			if(nModMeshes == 0 && nModBones == 0)
			{
				//선택된게 없다면
				return false;
			}



			//Undo
			//object targetObj = null;//삭제 21.6.30

			//>> [GizmoMain]
			//if (Editor.Select.ModMesh_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModMesh_Gizmo_Main;
			//}
			//else if(Editor.Select.ModBone_Gizmo_Main != null)
			//{
			//	targetObj = Editor.Select.ModBone_Gizmo_Main;
			//}


			if (isFirstScale)
			{
				apEditorUtil.SetRecord_Modifier(	apUndoGroupData.ACTION.Modifier_Gizmo_ScaleTransform, 
													Editor, 
													Editor.Select.Modifier, 
													//targetObj, 
													false,
													apEditorUtil.UNDO_STRUCT.ValueOnly);
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

					//_tmpNextWorldMatrix.SetScale(resultMatrix._scale + deltaScaleL, true);

					////ToParent x LocalModified x ParentWorld = Result
					////LocalModified = ToParent-1 x (Result' x ParentWorld-1) < 결합법칙 성립 안되므로 연산 순서 중요함
					//apMatrix nextLocalModifiedMatrix = apMatrix.RReverseInverse(matx_ToParent, apMatrix.RInverse(_tmpNextWorldMatrix, matx_ParentWorld, true));

					//curModMesh._transformMatrix.SetTRS(	nextLocalModifiedMatrix._pos,
					//										nextLocalModifiedMatrix._angleDeg,
					//										nextLocalModifiedMatrix._scale,
					//										true); 
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
			if(nModBones > 0)
			{
				apModifiedBone curModBone = null;
				apBone bone = null;
				
				for (int iModBone = 0; iModBone < nModBones; iModBone++)
				{
					curModBone = modBones_Gizmo[iModBone];
					if (curModBone == null) { continue; }
					
					bone = curModBone._bone;
					if(bone == null) { continue; }

					Vector3 prevScale = curModBone._transformMatrix._scale;
					Vector2 nextScale = new Vector2(prevScale.x + deltaScaleL.x, prevScale.y + deltaScaleL.y);

					curModBone._transformMatrix.SetScale(nextScale, true);
				}
			}

			//메시 그룹 갱신
			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();


			return true;
		}
	}
}