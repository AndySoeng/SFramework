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
	//Gizmo Controller -> Modifier [Color Only]에 대한 내용이 담겨있다.
	public partial class apGizmoController
	{
		//----------------------------------------------------------------
		// Gizmo - MeshGroup : Modifier / TF
		//----------------------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_Modifier_ColorOnly()
		{
			//Morph는 Vertex / VertexPos 계열 이벤트를 사용하며, Color 처리를 한다.
			apGizmos.GizmoEventSet.I.Clear();
			apGizmos.GizmoEventSet.I.SetEvent_1_Basic(	Select__Modifier_ColorOnly,
														Unselect__Modifier_ColorOnly, 
														null, 
														null, 
														null, 
														PivotReturn__Modifier_ColorOnly);

			apGizmos.GizmoEventSet.I.SetEvent_2_TransformGUI(	null,
																null,
																null,
																null,
																TransformChanged_Color__Modifier_ColorOnly,
																TransformChanged_Extra__Modifier_ColorOnly,
																apGizmos.TRANSFORM_UI.Color 
																	| apGizmos.TRANSFORM_UI.Extra//<<Extra 옵션 추가
																	);

			apGizmos.GizmoEventSet.I.SetEvent_3_Tools(	null, 
														null, 
														null, 
														null, 
														null, 
														null);
			
			apGizmos.GizmoEventSet.I.SetEvent_4_EtcAndKeyboard(	FirstLink__Modifier_ColorOnly, 
																null, 
																null, 
																null, 
																null);

			return apGizmos.GizmoEventSet.I;
		}

		public apGizmos.SelectResult FirstLink__Modifier_ColorOnly()
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
			

			return null;
		}


		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : ColorOnly 계열 선택시 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_ColorOnly(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			// (Editing 상태일때)

			//Transform과 달리 Bone은 선택되지 않는다.
			//추가 : Bone 선택
			//렌더링이 Bone이 먼저라서,
			//Bone이 가능하면, Bone을 먼저 선택한다.


			// 1. (Lock 걸리지 않았다면) 다른 Mesh Transform을 선택
			// 그 외에는 선택하는 것이 없다.

			// (Editing 상태가 아닐때)
			// (Lock 걸리지 않았다면) 다른 MeshTransform 선택
			//... Lock만 생각하면 될 것 같다.
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
			//else if (prevSelectedBone != null && isBoneTarget)
			//{
			//	prevSelectedObj = prevSelectedBone;
			//}
			
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
				//apBone selectedBone = null;
				apMeshGroup meshGroup = Editor.Select.MeshGroup;

				//추가 21.2.17 : 편집 중이 아닌 오브젝트를 선택하는 건 "편집 모드가 아니거나" / "선택 제한 옵션이 꺼진 경우"이다.
				bool isNotEditObjSelectable = Editor.Select.ExEditingMode == apSelection.EX_EDIT.None || !Editor._exModObjOption_NotSelectable;


				//Bone 먼저 선택하자 >>> Bone은 선택되지 않는다.
				#region [미사용 코드] : >>> Bone은 선택되지 않는다.
				//if (isBoneTarget)
				//{
				//	apBone bone = null;
				//	apBone resultBone = null;

				//	//<BONE_EDIT>
				//	//List<apBone> boneList = meshGroup._boneList_Root;
				//	//for (int i = 0; i < boneList.Count; i++)
				//	//{
				//	//	//Recursive하게 Bone을 선택한다.
				//	//	bone = CheckBoneClick(boneList[i], mousePosW, mousePosGL, Editor._boneGUIRenderMode, -1, Editor.Select.IsBoneIKRenderable);
				//	//	if (bone != null)
				//	//	{
				//	//		resultBone = bone;
				//	//	}
				//	//}


				//	//>>Bone Set으로 변경
				//	apMeshGroup.BoneListSet boneSet = null;
				//	if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
				//	{
				//		//for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)//출력 순서
				//		for (int iSet = meshGroup._boneListSets.Count - 1; iSet >= 0; iSet--)//선택 순서
				//		{
				//			boneSet = meshGroup._boneListSets[iSet];
				//			if (boneSet._bones_Root != null && boneSet._bones_Root.Count > 0)
				//			{
				//				//for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)//출력 순서
				//				for (int iRoot = boneSet._bones_Root.Count - 1; iRoot >= 0; iRoot--)//선택 순서
				//				{
				//					bone = CheckBoneClickRecursive(	boneSet._bones_Root[iRoot], 
				//													mousePosW, mousePosGL, 
				//													Editor._boneGUIRenderMode, 
				//													//-1, 
				//													Editor.Select.IsBoneIKRenderable, isNotEditObjSelectable,
				//													false);
				//					if (bone != null)
				//					{
				//						resultBone = bone;
				//						break;//다른 Root List는 체크하지 않는다.
				//					}
				//				}
				//			}

				//			if (resultBone != null)
				//			{
				//				//이 Set에서 선택이 완료되었다.
				//				break;
				//			}
				//		}
				//	}


				//	if (resultBone != null)
				//	{
				//		//이전
				//		//Editor.Select.SetBone(resultBone);
				//		//Editor.Select.SetSubMeshGroupInGroup(null);
				//		//Editor.Select.SetSubMeshInGroup(null);

				//		//변경 20.4.11 : 위 함수를 매번 호출하면 느리므로, 한번에 처리하는 함수를 이용하자.
				//		//추가 20.5.27 : 
				//		Editor.Select.SetSubObjectInGroup(null, null, resultBone, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);


				//		//selectedBone = resultBone;//기존 코드
				//		//selectedBone = Editor.Select.Bone;//변경 20.5.27 : 다중 선택때문에 메인이 무엇인지 다시 확인해봐야한다.

				//		//>> [GizmoMain] >>
				//		selectedBone = Editor.Select.Bone_Mod_Gizmo;

				//		prevSelectedObj = selectedBone;

				//		isSelectionCompleted = true;//추가 20.5.27 : 선택된 본과 상관없이 처리가 끝났다.
				//	}
				//} 
				#endregion


				//MeshTransform을 선택
				//정렬된 Render Unit
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
								continue;
							}

							if (renderUnit._isVisible && renderUnit._meshColor2X.a > 0.1f)//변경 21.7.20
							{
								bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(mousePosGL, renderUnit);

								if (isPick)
								{
									selectedMeshTransform = renderUnit._meshTransform;
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

					object selectedObject = null;
					if (parentMeshGroup == null || parentMeshGroup == Editor.Select.MeshGroup || isChildMeshTransformSelectable)
					{
						Editor.Select.SelectSubObject(selectedMeshTransform, null, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);//변경 20.5.27 : 다중 선택 옵션
						selectedObject = selectedMeshTransform;
					}
					else
					{
						apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
						if (childMeshGroupTransform != null)
						{
							Editor.Select.SelectSubObject(null, childMeshGroupTransform, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);//변경 20.5.27 : 다중 선택
							selectedObject = childMeshGroupTransform;
						}
						else
						{
							Editor.Select.SelectSubObject(selectedMeshTransform, null, null, multiSelect, apSelection.TF_BONE_SELECT.Exclusive);//변경 20.5.27 : 다중 선택
							selectedObject = selectedMeshTransform;
						}
					}

					isSelectionCompleted = true;

					//>> [GizmoMain] >>
					selectedMeshTransform = Editor.Select.MeshTF_Mod_Gizmo;

					prevSelectedObj = selectedMeshTransform;


					//[1.4.2] 선택된 객체에 맞게 자동 스크롤
					if(Editor._option_AutoScrollWhenObjectSelected)
					{
						//스크롤 가능한 상황인지 체크하고
						if(Editor.IsAutoScrollableWhenClickObject_MeshGroup(selectedObject, true))
						{
							//자동 스크롤을 요청한다.
							Editor.AutoScroll_HierarchyMeshGroup(selectedObject);
						}
					}
				}

				if(!isSelectionCompleted && multiSelect == apSelection.MULTI_SELECT.Main)//변경 20.5.27 : "추가 선택"이 아닌데 선택된게 없다면 취소.
				{
					Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);
					prevSelectedObj = null;
				}
			}

			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();

			//return result;
			return apGizmos.SelectResult.Main.SetSingle(prevSelectedObj);
		}



		public void Unselect__Modifier_ColorOnly()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}
			if (!Editor.Select.IsSelectionLock)
			{
				//락이 풀려있어야 한다.
				Editor.Select.SelectSubObject(null, null, null, apSelection.MULTI_SELECT.Main, apSelection.TF_BONE_SELECT.Exclusive);


				Editor.RefreshControllerAndHierarchy(false);
				Editor.SetRepaint();
			}
		}


		//-----------------------------------------------------------------

		/// <summary>
		/// Modifier내에서의 Gizmo 이벤트 : (Transform 참조) Transform의 색상값 [Color]
		/// </summary>
		/// <param name="color"></param>
		public void TransformChanged_Color__Modifier_ColorOnly(Color color, bool isVisible)
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


		public void TransformChanged_Extra__Modifier_ColorOnly()
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
			apModifiedMesh modMesh = Editor.Select.ModMesh_Main;			
			apRenderUnit renderUnit = Editor.Select.RenderUnitOfMod_Main;

			if(!modifier._isExtraPropertyEnabled)
			{
				return;
			}

			//Extra Option을 제어하는 Dialog를 호출하자
			//이전
			//apDialog_ExtraOption.ShowDialog(Editor, Editor._portrait, meshGroup, modifier, modMesh, renderUnit, false, null, null);

			//변경 : 21.10.2
			apDialog_ExtraOption.ShowDialog_Modifier(Editor, Editor._portrait, meshGroup, modifier, renderUnit, Editor.Select.RenderUnitOfMod_All, Editor.Select.ModMeshes_All);
		}



		public apGizmos.TransformParam PivotReturn__Modifier_ColorOnly()
		{
			if (Editor.Select.MeshGroup == null ||
				Editor.Select.Modifier == null)
			{
				return null;
			}

			bool isTransformPivot = false;
			//bool isBonePivot = false;

			//Editing 상태가 아니면..
			if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				return null;
			}


			//기즈모 메인과 별개로, 메인이 선택되지 않았다면 기즈모는 동작하지 않는다.
			//(그런 일이 있나..) 
			if(Editor.Select.ModMesh_Main == null
				//&& Editor.Select.ModBone_Main == null//ModBone은 대상이 아니다.
				)
			{
				return null;
			}

			
			//>> [GizmoMain] >>
			//apModifiedBone mainModBone = null;
			apModifiedMesh mainModMesh = null;

			//ModBone은 대상이 아니다.
			//if (Editor.Select.Modifier.IsTarget_Bone 
			//	&& Editor.Select.ModBone_Gizmo_Main != null
			//	&& Editor.Select.Bone_Mod_Gizmo != null
			//	)
			//{
			//	mainModBone = Editor.Select.ModBone_Gizmo_Main;
			//	isBonePivot = true;
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
			if (!isTransformPivot
				//&& !isBonePivot
				)
			{
				return null;
			}

			//Editing 상태가 아니면

			if (isTransformPivot)
			{
				apMatrix resultMatrix = null;

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

				//>TF
				//apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.TRS_NoDepth;
				//if (Editor.Select.Modifier._isColorPropertyEnabled)
				//{
				//	paramType |= apGizmos.TRANSFORM_UI.Color;//<<칼라 옵션이 있는 경우에!
				//}
				//if (Editor.Select.Modifier._isExtraPropertyEnabled)
				//{
				//	//추가 : Extra 옵션이 있다면 Gizmo 이벤트에도 추가
				//	paramType |= apGizmos.TRANSFORM_UI.Extra;
				//}

				apGizmos.TRANSFORM_UI paramType = apGizmos.TRANSFORM_UI.Color;
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
			return null;

		}
	}
}